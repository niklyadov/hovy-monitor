using System.Collections.Generic;
using System.Linq;
using HovyMonitor.Entity;
using Microsoft.Extensions.Logging;

namespace HovyMonitor.Api.Services
{
    public class SensorDetectionsService
    {
        private readonly ILogger<SensorDetectionsService> _logger;
        private List<SensorDetections> _sensorDetections
            = new List<SensorDetections>();

        public SensorDetectionsService(ILogger<SensorDetectionsService> logger)
        {
            _logger = logger;
        }

        public void WriteDetections(string detections)
        {
            if(_sensorDetections.Count() > 100)
            {
                _logger.LogInformation($"All sensor detections was cleaned, because it's count is {_sensorDetections.Count()}");
                _sensorDetections = _sensorDetections.TakeLast(10).ToList();
            }

            _sensorDetections.Add(new SensorDetections(detections));
        }

        public bool TryGetLastDetection(string sensorName, out SensorDetections result)
        {
            if(string.IsNullOrEmpty(sensorName))
            {
                result = _sensorDetections.LastOrDefault();
            } else
            {
                result = _sensorDetections.LastOrDefault(x => x.SensorName.Equals(sensorName));

            }

            return result != null;
        }
    }
}