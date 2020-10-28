﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BeerliftDashboard.Data;
using BeerliftDashboard.Models;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

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
        {
            await _telemetryService.SendTelemetry(beerliftMessage);
        }
    }
}