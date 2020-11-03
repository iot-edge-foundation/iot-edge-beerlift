using BeerliftDashboard.Models;
using System;
using System.Threading.Tasks;
using System.Timers;

namespace BeerliftDashboard.Data
{
    public class HeartbeatService
    {
        private Timer _timer;
        private DateTime _LastHeartbeat = DateTime.MinValue;

        public event EventHandler<HeartbeatMessage> InputMessageReceived;

        public HeartbeatService()
        {
            _timer = new Timer(60000);
            _timer.Elapsed += _timer_Elapsed;
            _timer.AutoReset = true;
            _timer.Start();
        }

        private async void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            var message = new HeartbeatMessage { elapsed = true };

            await Task.Run(() => { InputMessageReceived?.Invoke(this, message); });
        }

        private async Task OnInputMessageReceived(HeartbeatMessage message)
        {
            _timer.Stop();

            _LastHeartbeat = DateTime.Now;

            await Task.Run(() => { InputMessageReceived?.Invoke(this, message); });

            _timer.Start();
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