namespace BeerLiftModule
{
    public class AmbiantResponse 
    {
        public AmbiantResponse(LiftState state)
        {
            liftState = state.ToString();
            responseState = 0;
        }

        public int responseState { get; set; }

        public string errorMessage { get; set; }

        public double temperature {get; set;}

        public double humidity {get; set;}

        public string liftState {get; set;}

        public bool flooded {get; set;}

        public int attempts {get; set;}
    }
}
