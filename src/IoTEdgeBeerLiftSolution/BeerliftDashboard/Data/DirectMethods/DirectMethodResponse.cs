namespace IoTEdgeConversationDashboard.Data
{
    public class DirectMethodResponse
    {
        public DirectMethodResponse()
        {
        }

        public int ResponseStatus { get; set; }

        public string ResponseException { get; set; }

        public virtual void DeserializePayload(string json)
        {
        }
    }
}