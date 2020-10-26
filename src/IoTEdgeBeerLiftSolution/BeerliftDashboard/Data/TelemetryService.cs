using System;
using System.Threading.Tasks;

namespace BeerliftDashboard.Data
{
    public class TelemetryService
    {
        public event EventHandler<string> InputMessageReceived;

        private async Task OnInputMessageReceived(string messageString)
        {
            await Task.Run(() => { InputMessageReceived?.Invoke(this, messageString); });
        }

        public async Task SendMessage(string messageString)
        {
            if (!string.IsNullOrEmpty(messageString))
            {
                await OnInputMessageReceived(messageString);
            }
        }
    }
}