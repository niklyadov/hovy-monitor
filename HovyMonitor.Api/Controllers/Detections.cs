using HovyMonitor.Api.Services;
using HovyMonitor.Entity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

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
        public IActionResult GetSensorDetections([FromQuery] string sensorName)
        {
            if(!_sensorDetections.TryGetLastDetection(sensorName, out SensorDetections result))
            {
                return NotFound();
            }
            return Ok(result);
        }
    }
}