using Newtonsoft.Json;

namespace IoTEdgeConversationDashboard.Data
{
    public class AmbiantResponse : DirectMethodResponse
    {
        public AmbiantResponse() : base()
        {
        }

        public AmbiantPayload AmbiantPayload { get; private set; }

        public override void DeserializePayload(string json)
        {
            AmbiantPayload = JsonConvert.DeserializeObject<AmbiantPayload>(json);
        }
    }
}