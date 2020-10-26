using Microsoft.Azure.Devices;
using System;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace IoTEdgeConversationDashboard.Data
{
    public class IoTHubServiceClientService : IDisposable
    {
        private ServiceClient _serviceClient = null;

        public IoTHubServiceClientService(string connectionString)
        {
            Console.WriteLine("Starting IoTHubServiceClientService singleton");

            _serviceClient = ServiceClient.CreateFromConnectionString(connectionString);
        }

        public async Task<long> CountConnectedDevices()
        {
            var stats = await _serviceClient.GetServiceStatisticsAsync();

            return stats.ConnectedDeviceCount;
        }

        public async Task<U> SendDirectMethod<T, U>(string deviceId, string moduleId, string methodName, T request) where T : DirectMethodRequest where U : DirectMethodResponse, new()
        {
            var directMethodResponse = new U();

            try
            {
                var requestMethod = new CloudToDeviceMethod(methodName);

                var jsonText = JsonConvert.SerializeObject(request);

                requestMethod.SetPayloadJson(jsonText);

                var response = await _serviceClient.InvokeDeviceMethodAsync(deviceId, moduleId, requestMethod);

                directMethodResponse.ResponseStatus = response.Status;

                if (directMethodResponse.ResponseStatus == 200)
                {
                    var jsonResponse = response.GetPayloadAsJson();

                    if (!string.IsNullOrEmpty(jsonResponse))
                    {
                        directMethodResponse.DeserializePayload(jsonResponse);

                        return directMethodResponse;
                    }
                }
            }
            catch (Exception ex)
            {
                directMethodResponse.ResponseException = ex.Message;
            }

            return (U)Convert.ChangeType(directMethodResponse, typeof(U));
        }

        public void Dispose()
        {
            if (_serviceClient != null)
            {
                _serviceClient.CloseAsync().Wait();

                _serviceClient = null;
            }
        }
    }
}