using BeerliftDashboard.Models;

namespace IoTEdgeConversationDashboard.Data
{
    public class BottleHoldersPayload
    {
        public BeerliftMessage BeerLiftMessage { get; set; }

        public int responseState { get; set; }

        public string errorMessage { get; set; }
    }
}