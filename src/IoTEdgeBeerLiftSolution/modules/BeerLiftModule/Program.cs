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
    using System.Globalization;

    class Program
    {
        private static Mcp23xxx _mcp23xxxRead = null;

        private static Mcp23xxx _mcp23xxxWrite = null;

        private static string _state = "unknown";
        private const int DefaultInterval = 5000;

        private const int DefaultUpDownInterval = 20000;

        // GPIO 17 which is physical pin 11
        private const int DefaultUpRelayPin = 17;

        // GPIO 27 is physical pin 13
        private const int DefaultDownRelayPin = 27;

        // GPIO 4 is used for 1-Wire
        private const int DefaultDht22Pin = 4;

        private const int DefaultI2CAddressRead = 0x20;

        // I2C Read banks at 0x22
        private const int DefaultI2CAddressWrite = 0x22;

        private static string _lastState;

        private static byte _lastDataPortA = 0;

        private static byte _lastDataPortB = 0;

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

            await ioTHubModuleClient.SetMethodHandlerAsync(
                "Circus",
                CircusMethodCallBack,
                ioTHubModuleClient);

            Console.WriteLine("Attached method handler: Circus.");   

            await ioTHubModuleClient.SetMethodHandlerAsync(
                "FirstEmptySpot",
                FirstEmptySpotMethodCallBack,
                ioTHubModuleClient);

            Console.WriteLine("Attached method handler: FirstEmptySpot.");   

            SetupI2CRead();

            SetupI2CWrite();

            //// start reading beer state
            if (_mcp23xxxRead != null)
            {
                var thread = new Thread(() => ThreadBody(ioTHubModuleClient));
                thread.Start();
            }
        }

        private static void SetupI2CRead()
        {
            try
            {
                var i2cConnectionSettings = new I2cConnectionSettings(1, I2CAddressRead);
                var i2cDevice = I2cDevice.Create(i2cConnectionSettings);

                _mcp23xxxRead = new Mcp23017(i2cDevice);

                if (_mcp23xxxRead is Mcp23x1x mcp23x1x)
                {
                    // Input direction for switches.
                    mcp23x1x.WriteByte(Register.IODIR, 0b0000_0000, Port.PortA);
                    mcp23x1x.WriteByte(Register.IODIR, 0b0000_0000, Port.PortB);

                    Console.WriteLine("Mcp23017 Read Initialized.");   
                }
                else
                {
                    Console.WriteLine("Unable to initialize Mcp23017 Read.");   
                }    
            }  
            catch (Exception ex)
            {
                Console.WriteLine($"Error when initializing Mcp23017 at Read address '0x{I2CAddressRead:X4}': {ex.Message}");   
            }         
        }

        private static void SetupI2CWrite()
        {
            try
            {
                var i2cConnectionSettings = new I2cConnectionSettings(1, I2CAddressWrite);
                var i2cDevice = I2cDevice.Create(i2cConnectionSettings);

                _mcp23xxxWrite = new Mcp23017(i2cDevice);

                if (_mcp23xxxWrite is Mcp23x1x mcp23x1x)
                {
                    // Input direction for Leds.
                    mcp23x1x.WriteByte(Register.IODIR, 0b0000_0000, Port.PortA);
                    mcp23x1x.WriteByte(Register.IODIR, 0b0000_0000, Port.PortB);

                    Console.WriteLine("Mcp23017 Write Initialized.");   
                }
                else
                {
                    Console.WriteLine("Unable to initialize Mcp23017 Write.");   
                }
            }  
            catch (Exception ex)
            {
                Console.WriteLine($"Error when initializing Mcp23017 at Write address '0x{I2CAddressWrite:X4}': {ex.Message}");   
            }          
        }

        private static async void ThreadBody(object userContext)
        {
            var client = userContext as ModuleClient;

            if (client == null)
            {
                throw new InvalidOperationException("UserContext doesn't contain " + "expected values");
            }

            var  mcp23x1x = _mcp23xxxRead as Mcp23x1x;

            if (mcp23x1x == null)
            {
                Console.WriteLine("Unable to cast Mcp23017 Read GPIO.");   

                return;
            }

            while (true)
            {
                byte dataPortA = mcp23x1x.ReadByte(Register.GPIO, Port.PortA);
                byte dataPortB = mcp23x1x.ReadByte(Register.GPIO, Port.PortB);

                Console.WriteLine($"Ports read. A = {dataPortA} - B = {dataPortB}");

                if (dataPortA != _lastDataPortA
                        || dataPortB != _lastDataPortB
                        || _state != _lastState)
                {
                    _lastDataPortA = dataPortA;
                    _lastDataPortB = dataPortB;
                    _lastState = _state;

                    var beerLiftMessage = new BeerLiftMessage(dataPortA, dataPortB, _state);
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

        private static int Interval { get; set; } = DefaultInterval;
        private static int UpDownInterval { get; set; } = DefaultUpDownInterval;
        private static int UpRelayPin { get; set; } = DefaultUpRelayPin;
        private static int DownRelayPin { get; set; } = DefaultDownRelayPin;
        private static int Dht22Pin { get; set; } = DefaultDht22Pin;
        private static int I2CAddressRead { get; set; } = DefaultI2CAddressRead;
        private static int I2CAddressWrite { get; set; } = DefaultI2CAddressWrite;

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

                if (desiredProperties.Contains("i2cAddressRead")) 
                {
                    if (desiredProperties["i2cAddressRead"] != null)
                    {
                        string prop = desiredProperties["i2cAddressRead"];

                        I2CAddressRead = Int32.Parse(prop.ToLower().Split('x')[1], NumberStyles.HexNumber);
                    }
                    else
                    {
                        I2CAddressRead = DefaultI2CAddressRead;
                    }

                    Console.WriteLine($"I2CAddressRead changed to 0x{I2CAddressRead:X4}");
                    Console.WriteLine($"Restart module to access new address");

                    reportedProperties["i2cAddressRead"] = I2CAddressRead;
                }

                if (desiredProperties.Contains("i2cAddressWrite")) 
                {
                    if (desiredProperties["i2cAddressWrite"] != null)
                    {
                        string prop = desiredProperties["i2cAddressWrite"];

                        I2CAddressRead = Int32.Parse(prop.ToLower().Split('x')[1], NumberStyles.HexNumber);
                    }
                    else
                    {
                        I2CAddressRead = DefaultI2CAddressWrite;
                    }

                    Console.WriteLine($"I2CAddressRead changed to 0x{I2CAddressWrite:X4}");
                    Console.WriteLine($"Restart module to access new address");

                    reportedProperties["i2cAddressWrite"] = I2CAddressWrite;
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
                _state = "Moving up";

                _controller.Write(UpRelayPin, PinValue.Low); // start action
             
                await Task.Delay(UpDownInterval);

                _controller.Write(UpRelayPin, PinValue.High); // stop action

                Console.WriteLine($"Up at {DateTime.UtcNow}.");

                _state = "Up";
            }
            catch (Exception ex)
            {
                _state = "Exception up";

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
                _state = "Moving down";

                _controller.Write(DownRelayPin, PinValue.Low); // start action
             
                await Task.Delay(UpDownInterval);

                _controller.Write(DownRelayPin, PinValue.High); // stop action

                Console.WriteLine($"Down at {DateTime.UtcNow}.");

                _state = "Down";
            }
            catch (Exception ex)
            {
                _state = "Exception down";

                downResponse.errorMessage = ex.Message;   
                downResponse.responseState = -999;
            }
            
            var json = JsonConvert.SerializeObject(downResponse);
            var response = new MethodResponse(Encoding.UTF8.GetBytes(json), 200);

            return response;
        }   

        private static async Task<MethodResponse> FirstEmptySpotMethodCallBack(MethodRequest methodRequest, object userContext)
        {
            Console.WriteLine($"Executing FirstEmptySpotMethodCallBack at {DateTime.UtcNow}");

            var firstEmptySpotResponse = new FirstEmptySpotResponse{responseState = 0};

            try
            {
                var beerLiftMessage = new BeerLiftMessage(_lastDataPortA, _lastDataPortB);

                firstEmptySpotResponse.FindFirstEmpty = beerLiftMessage.FindFirstEmptySpot();

                var  mcp23x1x = _mcp23xxxWrite as Mcp23x1x;

                if (mcp23x1x == null)
                {
                    Console.WriteLine("Unable to cast Mcp23017 Read GPIO.");   

                    firstEmptySpotResponse.errorMessage = "Unable to cast Mcp23017 Read GPIO";   
                    firstEmptySpotResponse.responseState = 1;
                }
                else
                {
                    for (var i = 0; i<25 ; i++)
                    {
                        if (firstEmptySpotResponse.FindFirstEmpty == 0)
                        {
                            Console.WriteLine("Skip blink");
                            continue;
                        }

                        // blink led on i % 2 on else off

                        if (firstEmptySpotResponse.FindFirstEmpty <= 8)
                        {
                            var j = (i % 2) == Math.Pow(2, firstEmptySpotResponse.FindFirstEmpty) ? 1 : 0;
                            mcp23x1x.WriteByte(Register.GPIO, (byte) j , Port.PortA);
                        }
                        else
                        {
                            var j = (i % 2) == Math.Pow(2, firstEmptySpotResponse.FindFirstEmpty - 8) ? 1 : 0;
                            mcp23x1x.WriteByte(Register.GPIO, (byte) j, Port.PortB);
                        }

                        await Task.Delay(100);
                    }

                    Console.WriteLine();
                }

                Console.WriteLine($"FirstEmptySpot at {DateTime.UtcNow}.");
            }
            catch (Exception ex)
            {
                firstEmptySpotResponse.errorMessage = ex.Message;   
                firstEmptySpotResponse.responseState = -999;
            }
            
            var json = JsonConvert.SerializeObject(firstEmptySpotResponse);
            var response = new MethodResponse(Encoding.UTF8.GetBytes(json), 200);

            return response;  
        }

        private static async Task<MethodResponse> CircusMethodCallBack(MethodRequest methodRequest, object userContext)    
        {
            Console.WriteLine($"Executing CircusMethodCallBack at {DateTime.UtcNow}");

            var circusResponse = new CircusResponse{responseState = 0};

            try
            {
                var  mcp23x1x = _mcp23xxxWrite as Mcp23x1x;

                if (mcp23x1x == null)
                {
                    Console.WriteLine("Unable to cast Mcp23017 Read GPIO.");   

                    circusResponse.errorMessage = "Unable to cast Mcp23017 Read GPIO";   
                    circusResponse.responseState = 1;
                }
                else
                {
                    Console.WriteLine("Ra da da da da da da da Circus");
                    Console.WriteLine("Da da da da da da da da");
                    Console.WriteLine("Afro Circus, Afro Circus, Afro");
                    Console.WriteLine("Polka dot, polka dot, polka dot, Afro!");

                    for(var i = 0; i< 256; i++)
                    {
                        mcp23x1x.WriteByte(Register.GPIO, (byte) i , Port.PortA);
                        mcp23x1x.WriteByte(Register.GPIO, (byte) i, Port.PortB);

                        await Task.Delay(20);

                        //Console.Write($".{i}");   
                    }  

                    Console.WriteLine();
                }

                Console.WriteLine($"Circus at {DateTime.UtcNow}.");
            }
            catch (Exception ex)
            {
                circusResponse.errorMessage = ex.Message;   
                circusResponse.responseState = -999;
            }
            
            var json = JsonConvert.SerializeObject(circusResponse);
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
                ambiantValuesResponse.State = _state;

                await Task.Delay(1);    

                Console.WriteLine($"AmbiantValues at {DateTime.UtcNow} - Temperature:{ambiantValuesResponse.Temperature} / Humidity:{ambiantValuesResponse.Humidity} / Attempts:{ambiantValues.Attempts} / State:{_state}.");
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
