using System.Threading.Tasks;
using BeerliftDashboard.Data;
using BeerliftDashboard.Models;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace BeerliftDashboard.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HeartbeatController : ControllerBase
    {
        private HeartbeatService _hearbeatService;

        public HeartbeatController(HeartbeatService hearbeatService)
        {
            _hearbeatService = hearbeatService;
        }

        // POST api/<Heartbeat>
        [HttpPost]
        public async Task Post([FromBody] HeartbeatMessage hearbeatMessage)
        {
            await _hearbeatService.SendHeartbeat(hearbeatMessage);
        }
    }
}