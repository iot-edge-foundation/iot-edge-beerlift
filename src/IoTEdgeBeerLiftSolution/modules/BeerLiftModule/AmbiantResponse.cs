namespace BeerLiftModule
{
    public class AmbiantResponse 
    {
        public int responseState { get; set; }

        public string errorMessage { get; set; }

        public double temperature {get; set;}

        public double humidity {get; set;}

        public string state {get; set;}
    }
}
