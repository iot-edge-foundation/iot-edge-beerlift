using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BeerliftDashboard.Data;
using BeerliftDashboard.Models;
using Microsoft.AspNetCore.Mvc;

// WARNING: PUBLIC ENDPOINT - NO SECURITY

namespace BeerliftDashboard.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TelemetryController : ControllerBase
    {
        private TelemetryService _telemetryService;

        public TelemetryController(TelemetryService telemetryService)
        {
            _telemetryService = telemetryService;
        }

        // POST api/<Telemetry>
        [HttpPost]
        public async Task Post([FromBody] BeerliftMessage beerliftMessage)
            //, [FromHeader(Name = "APIKEY")] string key)
        {
            await _telemetryService.SendTelemetry(beerliftMessage);
        }
    }
}