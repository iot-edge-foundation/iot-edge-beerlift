using System;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace BeerliftDashboard.Models
{
    public class BeerliftMessage
    {
        public string deviceId { get; set; }
        public bool slot01 { get; set; }
        public bool slot02 { get; set; }
        public bool slot03 { get; set; }
        public bool slot04 { get; set; }
        public bool slot05 { get; set; }
        public bool slot06 { get; set; }
        public bool slot07 { get; set; }
        public bool slot08 { get; set; }
        public bool slot09 { get; set; }
        public bool slot10 { get; set; }
        public bool slot11 { get; set; }
        public bool slot12 { get; set; }
        public bool slot13 { get; set; }
        public bool slot14 { get; set; }
        public bool slot15 { get; set; }
        public bool slot16 { get; set; }
        public string liftState { get; set; }
        public bool isFlooded { get; set; }
        public DateTime timeStamp { get; set; }

        public override string ToString()
        {
            var isFloodedText = isFlooded ? "ALARM FLOODING" : "not flooded";

            var beerholdersText = "";
            beerholdersText += (slot01) ? "*" : "_";
            beerholdersText += (slot02) ? "*" : "_";
            beerholdersText += (slot03) ? "*" : "_";
            beerholdersText += (slot04) ? "*" : "_";
            beerholdersText += (slot05) ? "*" : "_";
            beerholdersText += (slot06) ? "*" : "_";
            beerholdersText += (slot07) ? "*" : "_";
            beerholdersText += (slot08) ? "*" : "_";
            beerholdersText += (slot09) ? "*" : "_";
            beerholdersText += (slot10) ? "*" : "_";
            beerholdersText += (slot11) ? "*" : "_";
            beerholdersText += (slot12) ? "*" : "_";
            beerholdersText += (slot13) ? "*" : "_";
            beerholdersText += (slot14) ? "*" : "_";
            beerholdersText += (slot15) ? "*" : "_";
            beerholdersText += (slot16) ? "*" : "_";
            var result = $"deviceId: {deviceId} {beerholdersText}; {isFloodedText}; Lift is now '{liftState}' at {DateTime.Now}";

            return result;
        }

        public bool IsSlotInUse(int slotId)
        {
            if (slotId == 1) return slot01;
            if (slotId == 2) return slot02;
            if (slotId == 3) return slot03;
            if (slotId == 4) return slot04;
            if (slotId == 5) return slot05;
            if (slotId == 6) return slot06;
            if (slotId == 7) return slot07;
            if (slotId == 8) return slot08;
            if (slotId == 9) return slot09;
            if (slotId == 10) return slot10;
            if (slotId == 11) return slot11;
            if (slotId == 12) return slot12;
            if (slotId == 13) return slot13;
            if (slotId == 14) return slot14;
            if (slotId == 15) return slot15;
            if (slotId == 16) return slot16;

            return false;
        }
    }
}