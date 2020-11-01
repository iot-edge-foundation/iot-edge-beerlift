using Microsoft.AspNetCore.Components;
using System;
using System.Threading.Tasks;

namespace BeerliftDashboard.Data
{
    public class BusyService
    {
        public event EventHandler<bool> BusyEvent;

        public void SetBusy(bool busy)
        {
            BusyEvent?.Invoke(this, busy);
        }
    }
}