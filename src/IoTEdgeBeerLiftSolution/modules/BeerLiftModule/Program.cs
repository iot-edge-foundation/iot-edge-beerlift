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

    class Program
    {
        // GPIO 17 which is physical pin 11
        static int r1Pin = 17;
        // GPIO 27 is physical pin 13
        static int r2Pin = 27;

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
            MqttTransportSettings mqttSetting = new MqttTransportSettings(TransportType.Mqtt_Tcp_Only);
            ITransportSettings[] settings = { mqttSetting };

            // Open a connection to the Edge runtime
            ModuleClient ioTHubModuleClient = await ModuleClient.CreateFromEnvironmentAsync(settings);
            await ioTHubModuleClient.OpenAsync();
            Console.WriteLine("IoT Hub module client initialized.");

            _controller = new GpioController();

            _controller.OpenPin(r1Pin, PinMode.Output);
            _controller.OpenPin(r2Pin, PinMode.Output);

            // Direct methods

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
        }

       static async Task<MethodResponse> OpenMethodCallBack(MethodRequest methodRequest, object userContext)        
        {
            Console.WriteLine("Executing OpenMethodCallBack");

            var openResponse = new OpenResponse{responseState = 0};

            try
            {
                _controller.Write(r1Pin, PinValue.High);
             
                await Task.Delay(20000);

                _controller.Write(r1Pin, PinValue.Low);

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
                _controller.Write(r2Pin, PinValue.High);
             
                await Task.Delay(20000);

                _controller.Write(r2Pin, PinValue.Low);

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



        // /// <summary>
        // /// This method is called whenever the module is sent a message from the EdgeHub. 
        // /// It just pipe the messages without any change.
        // /// It prints all the incoming messages.
        // /// </summary>
        // static async Task<MessageResponse> PipeMessage(Message message, object userContext)
        // {
        //     int counterValue = Interlocked.Increment(ref counter);

        //     var moduleClient = userContext as ModuleClient;
        //     if (moduleClient == null)
        //     {
        //         throw new InvalidOperationException("UserContext doesn't contain " + "expected values");
        //     }

        //     byte[] messageBytes = message.GetBytes();
        //     string messageString = Encoding.UTF8.GetString(messageBytes);
        //     Console.WriteLine($"Received message: {counterValue}, Body: [{messageString}]");

        //     if (!string.IsNullOrEmpty(messageString))
        //     {
        //         using (var pipeMessage = new Message(messageBytes))
        //         {
        //             foreach (var prop in message.Properties)
        //             {
        //                 pipeMessage.Properties.Add(prop.Key, prop.Value);
        //             }
        //             await moduleClient.SendEventAsync("output1", pipeMessage);
                
        //             Console.WriteLine("Received message sent");
        //         }
        //     }
        //     return MessageResponse.Completed;
        // }
    }
}
