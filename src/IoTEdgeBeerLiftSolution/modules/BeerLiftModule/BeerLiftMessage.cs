using System;

namespace BeerLiftModule
{
    public class BeerLiftMessage
    {
        private int _stateA; 
        
        private int _stateB;

        public BeerLiftMessage()
        {
            Timestamp = DateTime.UtcNow;
        }

        public BeerLiftMessage(int stateA, int stateB) : this()
        {
            _stateA = stateA;
            _stateB = stateB;
            
            BeerState01 = (stateA & 1) == 1;
            BeerState02 = (stateA & 2) == 2;
            BeerState03 = (stateA & 4) == 4;
            BeerState04 = (stateA & 8) == 8;
            BeerState05 = (stateA & 16) == 16;
            BeerState06 = (stateA & 32) == 32;
            BeerState07 = (stateA & 64) == 64;
            BeerState08 = (stateA & 128) == 128;

            BeerState09 = (stateB & 1) == 1;
            BeerState10 = (stateB & 2) == 2;
            BeerState11 = (stateB & 4) == 4;
            BeerState12 = (stateB & 8) == 8;
            BeerState13 = (stateB & 16) == 16;
            BeerState14 = (stateB & 32) == 32;
            BeerState15 = (stateB & 64) == 64;
            BeerState16 = (stateB & 128) == 128;
        }

        public bool BeerState01 {get; set;}
        public bool BeerState02 {get; set;}
        public bool BeerState03 {get; set;}
        public bool BeerState04 {get; set;}
        public bool BeerState05 {get; set;}
        public bool BeerState06 {get; set;}
        public bool BeerState07 {get; set;}
        public bool BeerState08 {get; set;}
        public bool BeerState09 {get; set;}
        public bool BeerState10 {get; set;}
        public bool BeerState11 {get; set;}
        public bool BeerState12 {get; set;}
        public bool BeerState13 {get; set;}
        public bool BeerState14 {get; set;}
        public bool BeerState15 {get; set;}
        public bool BeerState16 {get; set;}
        DateTime Timestamp {get; set;}

        public override string ToString()
        {
            return $"A:{_stateA} - B:{_stateB}";
        }  
    }
}
