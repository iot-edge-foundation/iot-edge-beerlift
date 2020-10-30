using Newtonsoft.Json;

namespace IoTEdgeConversationDashboard.Data
{
    public class BottleHoldersResponse : DirectMethodResponse
    {
        public BottleHoldersResponse() : base()
        {
        }

        public BottleHoldersPayload BeerHoldersPayload { get; private set; }

        public override void DeserializePayload(string json)
        {
            BeerHoldersPayload = JsonConvert.DeserializeObject<BottleHoldersPayload>(json);
        }
    }
}