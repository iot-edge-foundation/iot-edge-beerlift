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
    }
}