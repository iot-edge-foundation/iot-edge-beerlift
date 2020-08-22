namespace BeerLiftModule
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Runtime.Loader;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;
    using System.Device.Gpio;
    using System.Device.I2c;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Devices.Client;
    using Microsoft.Azure.Devices.Client.Transport.Mqtt;
    using Newtonsoft.Json;
    using Iot.Device.Mcp23xxx;
    using Microsoft.Azure.Devices.Shared;
    using Iot.Device.DHTxx;

    class Program
    {
        private const int DefaultInterval = 5000;

        private const int DefaultUpDownInterval = 20000;

        private static byte lastDataPortA = 0;

        private static byte lastDataPortB = 0;

        // I2C Read banks at 0x20
        private static readonly int _deviceAddressRead = 0x20;

        // GPIO 17 which is physical pin 11
        private static int DefaultUpRelayPin = 17;

        // GPIO 27 is physical pin 13
        private static int DefaultDownRelayPin = 27;

        // GPIO 4 is used for 1-Wire
        private static int DefaultDht22Pin = 4;

        private static GpioController _controller;

        private static string _moduleId; 

        private static string _deviceId;

        static void Main(string[] args)
        {
            Init().Wait();

            // Wait until the app unloads or is cancelled
            var cts = new CancellationTokenSource();
            AssemblyLoadContext.Default.Unloading += (ctx) => cts.Cancel();
            Console.CancelKeyPress += (sender, cpe) => cts.Cancel();
            WhenCancelled(cts.Token).Wait();
        }

        /// <summary>
        /// Handles cleanup operations when app is cancelled or unloads
        /// </summary>
        public static Task WhenCancelled(CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<bool>();
            cancellationToken.Register(s => ((TaskCompletionSource<bool>)s).SetResult(true), tcs);
            return tcs.Task;
        }

        /// <summary>
        /// Initializes the ModuleClient and sets up the callback to receive
        /// messages containing temperature information
        /// </summary>
        static async Task Init()
        {
            _deviceId = System.Environment.GetEnvironmentVariable("IOTEDGE_DEVICEID");
            _moduleId = Environment.GetEnvironmentVariable("IOTEDGE_MODULEID");

            Console.WriteLine();
            Console.WriteLine("  _     _              _              _                 _ _  __ _   ");
            Console.WriteLine(" (_)___| |_ ___ ___ __| |__ _ ___ ___| |__  ___ ___ _ _| (_)/ _| |_ ");
            Console.WriteLine(" | / _ \\  _|___/ -_) _` / _` / -_)___| '_ \\/ -_) -_) '_| | |  _|  _|");
            Console.WriteLine(" |_\\___/\\__|   \\___\\__,_\\__, \\___|   |_.__/\\___\\___|_| |_|_|_|  \\__|");
            Console.WriteLine("                        |___/                                       ");
            Console.WriteLine();
            Console.WriteLine("   Copyright © 2020 - josa josa josa");
            Console.WriteLine(" ");

            MqttTransportSettings mqttSetting = new MqttTransportSettings(TransportType.Mqtt_Tcp_Only);
            ITransportSettings[] settings = { mqttSetting };

            //// Open a connection to the Edge runtime

            ModuleClient ioTHubModuleClient = await ModuleClient.CreateFromEnvironmentAsync(settings);

            // Attach callback for Twin desired properties updates
            await ioTHubModuleClient.SetDesiredPropertyUpdateCallbackAsync(onDesiredPropertiesUpdate, ioTHubModuleClient);

            // Execute callback method for Twin desired properties updates
            var twin = await ioTHubModuleClient.GetTwinAsync();
            await onDesiredPropertiesUpdate(twin.Properties.Desired, ioTHubModuleClient);

            Console.WriteLine("Device twin initialized."); 

            await ioTHubModuleClient.OpenAsync();

            Console.WriteLine($"Module '{_deviceId}'-'{_moduleId}' initialized.");

            Console.WriteLine("Attached routing output: output1."); 

            //// Initialize GPIO

            _controller = new GpioController();

            _controller.OpenPin(UpRelayPin, PinMode.Output);
            _controller.OpenPin(DownRelayPin, PinMode.Output);

            _controller.Write(UpRelayPin, PinValue.High);  //by default high
            _controller.Write(DownRelayPin, PinValue.High);  //by default high

            Console.WriteLine("Default GPIO relays Initialized.");   

            //// Direct methods

            await ioTHubModuleClient.SetMethodHandlerAsync(
                "Up",
                UpMethodCallBack,
                ioTHubModuleClient);

            Console.WriteLine("Attached method handler: Up.");   

            await ioTHubModuleClient.SetMethodHandlerAsync(
                "Down",
                DownMethodCallBack,
                ioTHubModuleClient);

            Console.WriteLine("Attached method handler: Down.");   

            await ioTHubModuleClient.SetMethodHandlerAsync(
                "Ambiant",
                AmbiantValuesMethodCallBack,
                ioTHubModuleClient);

            Console.WriteLine("Attached method handler: Ambiant.");   

            //// start reading beer state

            var thread = new Thread(() => ThreadBody(ioTHubModuleClient));
            thread.Start();
        }

        private static async void ThreadBody(object userContext)
        {
            var client = userContext as ModuleClient;

            if (client == null)
            {
                throw new InvalidOperationException("UserContext doesn't contain " + "expected values");
            }

            var i2cConnectionSettings = new I2cConnectionSettings(1, _deviceAddressRead);
            var i2cDevice = I2cDevice.Create(i2cConnectionSettings);

            using Mcp23xxx mcp23xxx = new Mcp23017(i2cDevice);

            Console.WriteLine("Mcp23017 GPIO Initialized.");   

            if (mcp23xxx is Mcp23x1x mcp23x1x)
            {
                // Input direction for switches.
                mcp23x1x.WriteByte(Register.IODIR, 0b0000_0000, Port.PortA);
                mcp23x1x.WriteByte(Register.IODIR, 0b0000_0000, Port.PortB);

                while (true)
                {
                    byte dataPortA = mcp23x1x.ReadByte(Register.GPIO, Port.PortA);
                    byte dataPortB = mcp23x1x.ReadByte(Register.GPIO, Port.PortB);

                    Console.WriteLine($"Ports read. A = {dataPortA} - B = {dataPortB}");

                    if (dataPortA != lastDataPortA || 
                            dataPortB != lastDataPortB)
                    {
                        lastDataPortA = dataPortA;
                        lastDataPortB = dataPortB;

                        var beerLiftMessage = new BeerLiftMessage(dataPortA, dataPortB);
                        var json = JsonConvert.SerializeObject(beerLiftMessage);

                        using (var pipeMessage = new Message(Encoding.UTF8.GetBytes(json)))
                        {
                            pipeMessage.Properties.Add("StateLength", "16");

                            await client.SendEventAsync("output1", pipeMessage);

                            Console.WriteLine($"Message sent: {beerLiftMessage}");
                        }
                    }

                    await Task.Delay(Interval);
                }
            }
        }

        private static int Interval { get; set; } = DefaultInterval;
        private static int UpDownInterval { get; set; } = DefaultUpDownInterval;
        private static int UpRelayPin { get; set; } = DefaultUpRelayPin;
        private static int DownRelayPin { get; set; } = DefaultDownRelayPin;
        private static int Dht22Pin { get; set; } = DefaultDht22Pin;

        private static Task onDesiredPropertiesUpdate(TwinCollection desiredProperties, object userContext)
        {
            if (desiredProperties.Count == 0)
            {
                return Task.CompletedTask;
            }

            try
            {
                Console.WriteLine("Desired property change:");
                Console.WriteLine(JsonConvert.SerializeObject(desiredProperties));

                var client = userContext as ModuleClient;

                if (client == null)
                {
                    throw new InvalidOperationException($"UserContext doesn't contain expected ModuleClient");
                }

                var reportedProperties = new TwinCollection();

                if (desiredProperties.Contains("interval")) 
                {
                    if (desiredProperties["interval"] != null)
                    {
                        Interval = desiredProperties["interval"];
                    }
                    else
                    {
                        Interval = DefaultInterval;
                    }

                    Console.WriteLine($"Interval changed to {Interval}");

                    reportedProperties["interval"] = Interval;
                }

                if (desiredProperties.Contains("upDownInterval")) 
                {
                    if (desiredProperties["upDownInterval"] != null)
                    {
                        UpDownInterval = desiredProperties["upDownInterval"];
                    }
                    else
                    {
                        UpDownInterval = DefaultUpDownInterval;
                    }

                    Console.WriteLine($"UpDownInterval changed to {UpDownInterval}");

                    reportedProperties["upDownInterval"] = UpDownInterval;
                }

                if (desiredProperties.Contains("upRelayPin")) 
                {
                    if (desiredProperties["upRelayPin"] != null)
                    {
                        UpRelayPin = desiredProperties["upRelayPin"];
                    }
                    else
                    {
                        UpRelayPin = DefaultUpRelayPin;
                    }

                    Console.WriteLine($"UpRelayPin changed to {UpRelayPin}");

                    reportedProperties["upRelayPin"] = UpRelayPin;
                }

                if (desiredProperties.Contains("downRelayPin")) 
                {
                    if (desiredProperties["downRelayPin"] != null)
                    {
                        DownRelayPin = desiredProperties["downRelayPin"];
                    }
                    else
                    {
                        DownRelayPin = DefaultDownRelayPin;
                    }

                    Console.WriteLine($"DownRelayPin changed to {DownRelayPin}");

                    reportedProperties["downRelayPin"] = DownRelayPin;
                }

                if (desiredProperties.Contains("dht22Pin")) 
                {
                    if (desiredProperties["dht22Pin"] != null)
                    {
                        Dht22Pin = desiredProperties["dht22Pin"];
                    }
                    else
                    {
                        Dht22Pin = DefaultDht22Pin;
                    }

                    Console.WriteLine($"Dht22Pin changed to {Dht22Pin}");

                    reportedProperties["dht22Pin"] = Dht22Pin;
                }

                if (reportedProperties.Count > 0)
                {
                    client.UpdateReportedPropertiesAsync(reportedProperties).ConfigureAwait(false);
                }
            }
            catch (AggregateException ex)
            {
                foreach (Exception exception in ex.InnerExceptions)
                {
                    Console.WriteLine();
                    Console.WriteLine("Error when receiving desired property: {0}", exception);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine("Error when receiving desired property: {0}", ex.Message);
            }

            return Task.CompletedTask;
        }

        static async Task<MethodResponse> UpMethodCallBack(MethodRequest methodRequest, object userContext)        
        {
            Console.WriteLine($"Executing UpMethodCallBack at {DateTime.UtcNow}");

            var upResponse = new UpResponse{responseState = 0};

            try
            {
                _controller.Write(UpRelayPin, PinValue.Low); // start action
             
                await Task.Delay(UpDownInterval);

                _controller.Write(UpRelayPin, PinValue.High); // stop action

                Console.WriteLine($"Up at {DateTime.UtcNow}.");
            }
            catch (Exception ex)
            {
                upResponse.errorMessage = ex.Message;   
                upResponse.responseState = -999;
            }

            var json = JsonConvert.SerializeObject(upResponse);
            var response = new MethodResponse(Encoding.UTF8.GetBytes(json), 200);

            return response;
        } 

        static async Task<MethodResponse> DownMethodCallBack(MethodRequest methodRequest, object userContext)        
        {
            Console.WriteLine($"Executing DownMethodCallBack at {DateTime.UtcNow}");

            var downResponse = new DownResponse{responseState = 0};

            try
            {
                _controller.Write(DownRelayPin, PinValue.Low); // start action
             
                await Task.Delay(UpDownInterval);

                _controller.Write(DownRelayPin, PinValue.High); // stop action

                Console.WriteLine($"Down at {DateTime.UtcNow}.");
            }
            catch (Exception ex)
            {
                downResponse.errorMessage = ex.Message;   
                downResponse.responseState = -999;
            }
            
            var json = JsonConvert.SerializeObject(downResponse);
            var response = new MethodResponse(Encoding.UTF8.GetBytes(json), 200);

            return response;
        }   

        
       static async Task<MethodResponse> AmbiantValuesMethodCallBack(MethodRequest methodRequest, object userContext)        
        {
            Console.WriteLine($"Executing AmbiantValuesMethodCallBack at {DateTime.UtcNow}");

            var ambiantValuesResponse = new AmbiantValuesResponse{responseState = 0};

            try
            {
                var ambiantValues = ReadAmbiantValues();
            
                ambiantValuesResponse.Temperature = ambiantValues.Temperature;
                ambiantValuesResponse.Humidity = ambiantValues.Humidity;

                await Task.Delay(10);    

                Console.WriteLine($"AmbiantValues at {DateTime.UtcNow} - Temperature:{ambiantValuesResponse.Temperature} / Humidity:{ambiantValuesResponse.Humidity} / Attempts:{ambiantValues.Attempts}.");
            }
            catch (Exception ex)
            {
                ambiantValuesResponse.errorMessage = ex.Message;   
                ambiantValuesResponse.responseState = -999;
            }
            
            var json = JsonConvert.SerializeObject(ambiantValuesResponse);
            var response = new MethodResponse(Encoding.UTF8.GetBytes(json), 200);

            return response;
        } 
        private static AmbiantValues ReadAmbiantValues()
        {
             var ambiantValues = new AmbiantValues{Temperature = -273, Humidity = -1 };

            for (int i = 1; i <= 100; i++)
            {
                ambiantValues.Attempts = i;

                using (Dht22 dht = new Dht22(Dht22Pin))
                {
                    var temperature = dht.Temperature;
                    var humidity = dht.Humidity;

                    if (temperature.Kelvins != 0)
                    {
                        ambiantValues.Temperature = temperature.DegreesCelsius;
                        ambiantValues.Humidity = humidity.Percent;

                        return ambiantValues;
                    }
                }
            }

            return ambiantValues;
        }     
    }
}
