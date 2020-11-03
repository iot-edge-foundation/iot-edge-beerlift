using System;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace BeerliftDashboard.Models
{
    public class HeartbeatMessage
    {
        public string deviceId { get; set; }
        public int counter { get; set; }
        public DateTime timeStamp { get; set; }

        public override string ToString()
        {
            var result = $"Hearbeat '{counter}' at {DateTime.Now}";

            return result;
        }

        public bool elapsed = false;
    }
}