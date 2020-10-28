using BeerliftDashboard.Models;
using System;
using System.Threading.Tasks;

namespace BeerliftDashboard.Data
{
    public class TelemetryService
    {
        public event EventHandler<BeerliftMessage> InputMessageReceived;

        private async Task OnInputMessageReceived(BeerliftMessage message)
        {
            await Task.Run(() => { InputMessageReceived?.Invoke(this, message); });
        }

        public async Task SendTelemetry(BeerliftMessage message)
        {
            if (message != null)
            {
                await OnInputMessageReceived(message);
            }
        }
    }
}