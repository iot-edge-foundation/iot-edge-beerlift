namespace IoTEdgeConversationDashboard.Data
{
    public class FindEmptySlotPayload
    {
        public int emptySlot { get; set; }

        public int responseState { get; set; }

        public string errorMessage { get; set; }
    }
}