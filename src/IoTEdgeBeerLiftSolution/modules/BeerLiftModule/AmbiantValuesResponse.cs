namespace BeerLiftModule
{
    public class AmbiantValuesResponse 
    {
        public int responseState { get; set; }

        public string errorMessage { get; set; }

        public double Temperature {get; set;}

        public double Humidity {get; set;}

        public string State {get; set;}
    }
}
