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
            
            Slot01 = (stateA & 1) == 1;
            Slot02 = (stateA & 2) == 2;
            Slot03 = (stateA & 4) == 4;
            Slot04 = (stateA & 8) == 8;
            Slot05 = (stateA & 16) == 16;
            Slot06 = (stateA & 32) == 32;
            Slot07 = (stateA & 64) == 64;
            Slot08 = (stateA & 128) == 128;

            Slot09 = (stateB & 1) == 1;
            Slot10 = (stateB & 2) == 2;
            Slot11 = (stateB & 4) == 4;
            Slot12 = (stateB & 8) == 8;
            Slot13 = (stateB & 16) == 16;
            Slot14 = (stateB & 32) == 32;
            Slot15 = (stateB & 64) == 64;
            Slot16 = (stateB & 128) == 128;
        }

        public BeerLiftMessage(int stateA, int stateB, string state) : this(stateA, stateB)
        {
            State = state;
        }

        public bool Slot01 {get; set;}
        public bool Slot02 {get; set;}
        public bool Slot03 {get; set;}
        public bool Slot04 {get; set;}
        public bool Slot05 {get; set;}
        public bool Slot06 {get; set;}
        public bool Slot07 {get; set;}
        public bool Slot08 {get; set;}
        public bool Slot09 {get; set;}
        public bool Slot10 {get; set;}
        public bool Slot11 {get; set;}
        public bool Slot12 {get; set;}
        public bool Slot13 {get; set;}
        public bool Slot14 {get; set;}
        public bool Slot15 {get; set;}
        public bool Slot16 {get; set;}
        public DateTime Timestamp {get; set;}
        public string State {get; set;}

        public override string ToString()
        {
            return $"A:{_stateA} - B:{_stateB} - state:{State}";
        }  

        public int FindFirstEmptySpot()
        {
            if (!Slot01) return 1;
            if (!Slot02) return 2;
            if (!Slot03) return 3;
            if (!Slot04) return 4;
            if (!Slot05) return 5;
            if (!Slot06) return 6;
            if (!Slot07) return 7;
            if (!Slot08) return 8;
            if (!Slot09) return 9;
            if (!Slot10) return 10;
            if (!Slot11) return 11;
            if (!Slot12) return 12;
            if (!Slot13) return 13;
            if (!Slot14) return 14;
            if (!Slot15) return 15;
            if (!Slot16) return 16;

            return 0;
        }
    }
}
