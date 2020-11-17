namespace BeerLiftModule
{
    public class FindEmptySlotResponse
    {
        public int emptySlot {get; set;}
        
        public int responseState { get; set; }

        public string errorMessage { get; set; }
    }

    public class RouletteResponse
    {
        public int shot {get; set;}
        
        public int responseState { get; set; }

        public string errorMessage { get; set; }
    }
}
