using BeerliftDashboard.Models;
using System;
using System.Threading.Tasks;

namespace BeerliftDashboard.Data
{
    public class HeartbeatService
    {
        public event EventHandler<HeartbeatMessage> InputMessageReceived;

        private async Task OnInputMessageReceived(HeartbeatMessage message)
        {
            await Task.Run(() => { InputMessageReceived?.Invoke(this, message); });
        }

        public async Task SendHeartbeat(HeartbeatMessage message)
        {
            if (message != null)
            {
                await OnInputMessageReceived(message);
            }
        }
    }
}