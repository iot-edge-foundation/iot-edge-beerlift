using Newtonsoft.Json;

namespace IoTEdgeConversationDashboard.Data
{
    public class FindEmptySlotResponse : DirectMethodResponse
    {
        public FindEmptySlotResponse() : base()
        {
        }

        public FindEmptySlotPayload FindEmptySlotPayload { get; private set; }

        public override void DeserializePayload(string json)
        {
            FindEmptySlotPayload = JsonConvert.DeserializeObject<FindEmptySlotPayload>(json);
        }
    }
}