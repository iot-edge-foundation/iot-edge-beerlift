using Microsoft.AspNetCore.Components;
using System;
using System.Threading.Tasks;

namespace BeerliftDashboard.Data
{
    public class BusyService
    {
        private bool _lastBusy = false;

        public event EventHandler<bool> BusyEvent;

        public void SetBusy(bool? busy)
        {
            if (busy.HasValue)
            {
                _lastBusy = busy.Value;
            }

            BusyEvent?.Invoke(this, _lastBusy);
        }
    }
}