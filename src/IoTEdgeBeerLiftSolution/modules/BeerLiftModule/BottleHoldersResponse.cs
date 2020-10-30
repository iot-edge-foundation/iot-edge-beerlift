namespace BeerLiftModule
{
    public class BottleHoldersResponse
    {
        public BeerLiftMessage BeerLiftMessage {get; set;}
        
        public int responseState { get; set; }

        public string errorMessage { get; set; }
    }
}


