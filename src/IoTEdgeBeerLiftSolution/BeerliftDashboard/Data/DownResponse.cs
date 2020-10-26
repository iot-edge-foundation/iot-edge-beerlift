using Newtonsoft.Json;

namespace IoTEdgeConversationDashboard.Data
{
    public class DownResponse : DirectMethodResponse
    {
        public DownResponse() : base()
        {
        }

        public DownPayload DownPayload { get; private set; }

        public override void DeserializePayload(string json)
        {
            DownPayload = JsonConvert.DeserializeObject<DownPayload>(json);
        }
    }

}