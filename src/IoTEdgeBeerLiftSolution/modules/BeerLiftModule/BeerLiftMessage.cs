using System;

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

        public int Roulette()
        {
            if (FindEmptySlot() == 0)
            {
                // no empty slot found, return 0
                return 0;
            }

            var found = false;

            var random = new Random(DateTime.Now.Millisecond);

            var r = -1;

            while (!found)
            {
                r = random.Next(1, 17); // returns 1 to 16

                // check if a bottle is available there
                found = (!IsEmptySlot(r));
            }

            return r;
        }

        private bool IsEmptySlot(int slot)
        {
            // check is a certain location is empty

            if (slot==1 && !slot01) return true;
            if (slot==2 && !slot02) return true;
            if (slot==3 && !slot03) return true;
            if (slot==4 && !slot04) return true;
            if (slot==5 && !slot05) return true;
            if (slot==6 && !slot06) return true;
            if (slot==7 && !slot07) return true;
            if (slot==8 && !slot08) return true;
            if (slot==9 && !slot09) return true;
            if (slot==10 && !slot10) return true;
            if (slot==11 && !slot11) return true;
            if (slot==12 && !slot12) return true;
            if (slot==13 && !slot13) return true;
            if (slot==14 && !slot14) return true;
            if (slot==15 && !slot15) return true;
            if (slot==16 && !slot16) return true;

            return false;
        }
    }
}
