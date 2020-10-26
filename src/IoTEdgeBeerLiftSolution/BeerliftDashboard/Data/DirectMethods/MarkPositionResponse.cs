using Newtonsoft.Json;

namespace IoTEdgeConversationDashboard.Data
{
    public class MarkPositionResponse : DirectMethodResponse
    {
        public MarkPositionResponse() : base()
        {
        }

        public MarkPositionPayload LedTestPayload { get; private set; }

        public override void DeserializePayload(string json)
        {
            LedTestPayload = JsonConvert.DeserializeObject<MarkPositionPayload>(json);
        }
    }
}