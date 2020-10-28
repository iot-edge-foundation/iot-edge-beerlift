using Newtonsoft.Json;

namespace IoTEdgeConversationDashboard.Data
{
    public class MarkPositionResponse : DirectMethodResponse
    {
        public MarkPositionResponse() : base()
        {
        }

        public MarkPositionPayload MarkPositionPayload { get; private set; }

        public override void DeserializePayload(string json)
        {
            MarkPositionPayload = JsonConvert.DeserializeObject<MarkPositionPayload>(json);
        }
    }
}