using System;
using System.Collections.Generic;

namespace BeerLiftModule
{
    public class BeerLiftMessage
    {
        private int _stateA; 
        
        private int _stateB;

        public BeerLiftMessage()
        {
            timeStamp = DateTime.UtcNow;
            isFlooded = true;
        }

        public BeerLiftMessage(string deviceId, int stateA, int stateB) : this()
        {
            this.deviceId = deviceId;

            _stateA = stateA;
            _stateB = stateB;
            
            slot01 = (stateA & 1) == 1;
            slot02 = (stateA & 2) == 2;
            slot03 = (stateA & 4) == 4;
            slot04 = (stateA & 8) == 8;
            slot05 = (stateA & 16) == 16;
            slot06 = (stateA & 32) == 32;
            slot07 = (stateA & 64) == 64;
            slot08 = (stateA & 128) == 128;

            slot09 = (stateB & 1) == 1;
            slot10 = (stateB & 2) == 2;
            slot11 = (stateB & 4) == 4;
            slot12 = (stateB & 8) == 8;
            slot13 = (stateB & 16) == 16;
            slot14 = (stateB & 32) == 32;
            slot15 = (stateB & 64) == 64;
            slot16 = (stateB & 128) == 128;
        }

        public BeerLiftMessage(string deviceId, int stateA, int stateB, LiftState state) : this(deviceId, stateA, stateB)
        {
            this.liftState = state.ToString();
        }
        
        public string deviceId {get; set;}

        public bool slot01 {get; set;}
        public bool slot02 {get; set;}
        public bool slot03 {get; set;}
        public bool slot04 {get; set;}
        public bool slot05 {get; set;}
        public bool slot06 {get; set;}
        public bool slot07 {get; set;}
        public bool slot08 {get; set;}
        public bool slot09 {get; set;}
        public bool slot10 {get; set;}
        public bool slot11 {get; set;}
        public bool slot12 {get; set;}
        public bool slot13 {get; set;}
        public bool slot14 {get; set;}
        public bool slot15 {get; set;}
        public bool slot16 {get; set;}
        public DateTime timeStamp {get; set;}
        
        // unknown, movingUp, up, movingDown, down
        public string liftState {get; set;}

        public bool isFlooded {get; set;}

        public override string ToString()
        {
            return $"A:{_stateA} - B:{_stateB} - lift state:{liftState} - flooded:{isFlooded}";
        }  

        // Returns a value between 1 and 16 (or 0 is all occupied) 
        public int FindEmptySlot()
        {
            if (!slot01) return 1;
            if (!slot02) return 2;
            if (!slot03) return 3;
            if (!slot04) return 4;
            if (!slot05) return 5;
            if (!slot06) return 6;
            if (!slot07) return 7;
            if (!slot08) return 8;
            if (!slot09) return 9;
            if (!slot10) return 10;
            if (!slot11) return 11;
            if (!slot12) return 12;
            if (!slot13) return 13;
            if (!slot14) return 14;
            if (!slot15) return 15;
            if (!slot16) return 16;

            return 0;
        }


        private List<int> FindUsedSlots()
        {
            var result = new List<int>();

            if (slot01) { result.Add(1); };
            if (slot02) { result.Add(2); };
            if (slot03) { result.Add(3); };
            if (slot04) { result.Add(4); };
            if (slot05) { result.Add(5); };
            if (slot06) { result.Add(6); };
            if (slot07) { result.Add(7); };
            if (slot08) { result.Add(8); };
            if (slot09) { result.Add(9); };
            if (slot10) { result.Add(10); };
            if (slot11) { result.Add(11); };
            if (slot12) { result.Add(12); };
            if (slot13) { result.Add(13); };
            if (slot14) { result.Add(14); };
            if (slot15) { result.Add(15); };
            if (slot16) { result.Add(16); };

            return result;
        }

        public int Roulette()
        {
            var usedSlots = FindUsedSlots();
            if (usedSlots.Count == 0)
            {
                // no empty slots found, return 0
                return 0;
            }

            var found = false;

            var random = new Random(DateTime.Now.Millisecond);

            var r = -1;

            while (!found)
            {
                r = random.Next(1, 17); // returns 1 to 16

                // check if a bottle is available there
                found = (usedSlots.Contains(r));
            }

            return r;
        }
    }
}
