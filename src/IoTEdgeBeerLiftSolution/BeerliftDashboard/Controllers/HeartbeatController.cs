using System.Threading.Tasks;
using BeerliftDashboard.Data;
using BeerliftDashboard.Models;
using Microsoft.AspNetCore.Mvc;

// WARNING: PUBLIC ENDPOINT - NO SECURITY

namespace BeerliftDashboard.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HeartbeatController : ControllerBase
    {
        private HeartbeatService _heartbeatService;

        public HeartbeatController(HeartbeatService heartbeatService)
        {
            _heartbeatService = heartbeatService;
        }

        // POST api/<Heartbeat>
        [HttpPost]
        public async Task Post([FromBody] HeartbeatMessage heartbeatMessage)
            //, [FromHeader(Name = "APIKEY")] string key)
        {
            await _heartbeatService.SendHeartbeat(heartbeatMessage);
        }
    }
}