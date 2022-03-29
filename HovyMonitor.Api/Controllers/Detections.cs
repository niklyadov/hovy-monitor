using HovyMonitor.Api.Services;
using HovyMonitor.Entity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HovyMonitor.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class Detections : ControllerBase
    {
        private readonly ILogger<Detections> _logger;
        private readonly SensorDetectionsService _sensorDetections;

        public Detections(ILogger<Detections> logger, SensorDetectionsService sensorDetections)
        {
            _logger = logger;
            _sensorDetections = sensorDetections;
        }

        [HttpGet]
        [Route("last")]
        public IActionResult GetLastSensorDetections([FromQuery] string sensorName)
        {

            if (!_sensorDetections.TryGetLastDetections(sensorName, out List<SensorDetection> result))
            {
                return NotFound();
            }
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetSensorDetections([FromRoute] int accuracyMinutes = 10)
        {
            var detections = await _sensorDetections.GetListDetections();

            return Ok(detections);
        }

    }
}