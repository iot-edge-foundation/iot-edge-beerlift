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

    class Program
    {
        private const int DefaultInterval = 5000;

        private static readonly int s_deviceAddress = 0x20;

        // GPIO 17 which is physical pin 11
        static int DefaultR1Pin = 17;

        // GPIO 27 is physical pin 13
        static int DefaultR2Pin = 27;

        static GpioController _controller;

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
            Console.WriteLine();
            Console.WriteLine("  _     _              _              _                 _ _  __ _   ");
            Console.WriteLine(" (_)___| |_ ___ ___ __| |__ _ ___ ___| |__  ___ ___ _ _| (_)/ _| |_ ");
            Console.WriteLine(" | / _ \\  _|___/ -_) _` / _` / -_)___| '_ \\/ -_) -_) '_| | |  _|  _|");
            Console.WriteLine(" |_\\___/\\__|   \\___\\__,_\\__, \\___|   |_.__/\\___\\___|_| |_|_|_|  \\__|");
            Console.WriteLine("                        |___/                                       ");
            Console.WriteLine();

            MqttTransportSettings mqttSetting = new MqttTransportSettings(TransportType.Mqtt_Tcp_Only);
            ITransportSettings[] settings = { mqttSetting };

            //// Open a connection to the Edge runtime

            ModuleClient ioTHubModuleClient = await ModuleClient.CreateFromEnvironmentAsync(settings);

            // Attach callback for Twin desired properties updates
            await ioTHubModuleClient.SetDesiredPropertyUpdateCallbackAsync(onDesiredPropertiesUpdate, ioTHubModuleClient);

            // Execute callback method for Twin desired properties updates
            var twin = await ioTHubModuleClient.GetTwinAsync();
            await onDesiredPropertiesUpdate(twin.Properties.Desired, ioTHubModuleClient);

            await ioTHubModuleClient.OpenAsync();
            Console.WriteLine("IoT Hub module client initialized.");

            //// Initialize GPIO

            _controller = new GpioController();

            _controller.OpenPin(R1Pin, PinMode.Output);
            _controller.OpenPin(R2Pin, PinMode.Output);

            Console.WriteLine("GPIO Initialized");   

            //// Direct methods

            await ioTHubModuleClient.SetMethodHandlerAsync(
                "Open",
                OpenMethodCallBack,
                ioTHubModuleClient);

            Console.WriteLine("Attached method handler: Open");   

            await ioTHubModuleClient.SetMethodHandlerAsync(
                "Close",
                CloseMethodCallBack,
                ioTHubModuleClient);

            Console.WriteLine("Attached method handler: Close");   

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

            var i2cConnectionSettings = new I2cConnectionSettings(1, s_deviceAddress);
            var i2cDevice = I2cDevice.Create(i2cConnectionSettings);

            using Mcp23xxx mcp23xxx = new Mcp23017(i2cDevice);

            GpioController controllerUsingMcp = new GpioController(PinNumberingScheme.Logical, mcp23xxx);

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

                    // var BeerStateMessageBody = new BeerStateMessageBody
                    // {
                    //     timeStamp = DateTime.UtcNow,
                    // };

                    // await _moduleOutputs.GetModuleOutput("output1")?.SendMessage(heartbeatMessageBody);

                    await Task.Delay(Interval);
                }
            }
        }

        private static int Interval { get; set; } = DefaultInterval;

       private static int R1Pin { get; set; } = DefaultR1Pin;
       private static int R2Pin { get; set; } = DefaultR2Pin;

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

                if (desiredProperties.Contains("r1Pin")) 
                {
                    if (desiredProperties["r1Pin"] != null)
                    {
                        R1Pin = desiredProperties["r1Pin"];
                    }
                    else
                    {
                        R1Pin = DefaultR1Pin;
                    }

                    Console.WriteLine($"R1Pin changed to {R1Pin}");

                    reportedProperties["r1Pin"] = R1Pin;
                }

                if (desiredProperties.Contains("r2Pin")) 
                {
                    if (desiredProperties["r2Pin"] != null)
                    {
                        R2Pin = desiredProperties["r2Pin"];
                    }
                    else
                    {
                        R2Pin = DefaultR2Pin;
                    }

                    Console.WriteLine($"R2Pin changed to {R2Pin}");

                    reportedProperties["r2Pin"] = R2Pin;
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

        static async Task<MethodResponse> OpenMethodCallBack(MethodRequest methodRequest, object userContext)        
        {
            Console.WriteLine("Executing OpenMethodCallBack");

            var openResponse = new OpenResponse{responseState = 0};

            try
            {
                _controller.Write(R1Pin, PinValue.High);
             
                await Task.Delay(20000);

                _controller.Write(R1Pin, PinValue.Low);

                Console.WriteLine("Opened.");
            }
            catch (Exception ex)
            {
                openResponse.errorMessage = ex.Message;   
                openResponse.responseState = -999;
            }
            var json = JsonConvert.SerializeObject(openResponse);
            var response = new MethodResponse(Encoding.UTF8.GetBytes(json), 200);

            return response;
        } 

        static async Task<MethodResponse> CloseMethodCallBack(MethodRequest methodRequest, object userContext)        
        {
            Console.WriteLine("Executing CloseMethodCallBack");

            var closeResponse = new CloseResponse{responseState = 0};

            try
            {
                _controller.Write(R2Pin, PinValue.High);
             
                await Task.Delay(20000);

                _controller.Write(R2Pin, PinValue.Low);

                Console.WriteLine("Closed.");
            }
            catch (Exception ex)
            {
                closeResponse.errorMessage = ex.Message;   
                closeResponse.responseState = -999;
            }
            var json = JsonConvert.SerializeObject(closeResponse);
            var response = new MethodResponse(Encoding.UTF8.GetBytes(json), 200);

            return response;
        }        
    }
}
