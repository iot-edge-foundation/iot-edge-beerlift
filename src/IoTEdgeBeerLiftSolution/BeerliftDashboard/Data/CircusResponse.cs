using Newtonsoft.Json;

namespace IoTEdgeConversationDashboard.Data
{
    public class CircusResponse : DirectMethodResponse
    {
        public CircusResponse() : base()
        {
        }

        public CircusPayload CircusPayload { get; private set; }

        public override void DeserializePayload(string json)
        {
            CircusPayload = JsonConvert.DeserializeObject<CircusPayload>(json);
        }
    }

}