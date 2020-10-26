using Newtonsoft.Json;

namespace IoTEdgeConversationDashboard.Data
{
    public class UpResponse : DirectMethodResponse
    {
        public UpResponse() : base()
        {
        }

        public UpPayload UpPayload { get; private set; }

        public override void DeserializePayload(string json)
        {
            UpPayload = JsonConvert.DeserializeObject<UpPayload>(json);
        }
    }
}