using BeerliftDashboard.Models;

namespace BeerliftDashboard.Data
{
    public class SessionService
    {
        public bool Validated { get; set; }

        public HeartbeatMessage HeartbeatMessage { get; set; } = null;

        public BeerliftMessage BeerliftMessage { get; set; } = null;
    }
}