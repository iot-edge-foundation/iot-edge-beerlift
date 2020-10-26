using Newtonsoft.Json;

namespace IoTEdgeConversationDashboard.Data
{
    public class AdvertiseResponse : DirectMethodResponse
    {
        public AdvertiseResponse() : base()
        {
        }

        public AdvertisePayload AdvertisePayload { get; private set; }

        public override void DeserializePayload(string json)
        {
            AdvertisePayload = JsonConvert.DeserializeObject<AdvertisePayload>(json);
        }
    }
}