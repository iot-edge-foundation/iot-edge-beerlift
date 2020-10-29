using System;
using System.Net.Http;
using System.Text;

public static void Run(string myIoTHubMessage, ILogger log)
{
    log.LogInformation($"C# IoT Hub trigger function processed a message: {myIoTHubMessage}");

    var url = string.Empty;

    if (myIoTHubMessage.Contains("isFlooded"))
    {
        url = "https://beerlift-weu-wa.azurewebsites.net/api/telemetry";
    }
    else
    {
        url = "https://beerlift-weu-wa.azurewebsites.net/api/heartbeat";
    }

    var stringContent = new StringContent(myIoTHubMessage, Encoding.UTF8, "application/json");

    using var httpClient = new HttpClient();

    var response = httpClient.PostAsync(url, stringContent).Result;

    Console.WriteLine($"Response {response.StatusCode}");
}