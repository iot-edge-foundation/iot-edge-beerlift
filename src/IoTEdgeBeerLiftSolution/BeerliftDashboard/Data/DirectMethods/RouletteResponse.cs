using Newtonsoft.Json;

namespace IoTEdgeConversationDashboard.Data
{
    public class RouletteResponse : DirectMethodResponse
    {
        public RouletteResponse() : base()
        {
        }

        public RoulettePayload RoulettePayload { get; private set; }

        public override void DeserializePayload(string json)
        {
            RoulettePayload = JsonConvert.DeserializeObject<RoulettePayload>(json);
        }
    }
}