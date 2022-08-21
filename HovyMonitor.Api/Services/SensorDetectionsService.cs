using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using HovyMonitor.Api.Data.Repository;
using HovyMonitor.Entity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace HovyMonitor.Api.Services
{
    public class SensorDetectionsService
    {
        private readonly ILogger<SensorDetectionsService> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        private List<SensorDetection> _sensorDetections
            = new List<SensorDetection>();

        public SensorDetectionsService(ILogger<SensorDetectionsService> logger,
            IServiceScopeFactory serviceScopeFactory)
        {
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
        }

        public async Task<List<SensorDetection>> GetListDetections(DateTime date)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var repository = scope.ServiceProvider
                .GetRequiredService<SensorDetectionsRepository>();

            return await repository.GetList(x => x.DateTime.Date == date.Date);
        }

        public async Task WriteDetectionsAsync(string detections)
        {
            // bad data, ignore that
            if (string.IsNullOrEmpty(detections))
            {
                return;
            }
            
            var first = _sensorDetections.FirstOrDefault();

            if(first != null && (DateTime.Now - first.DateTime).TotalSeconds >= 60)
            {
                 var savedDetections = _sensorDetections
                    .Take(_sensorDetections.Count / 2)
                    .ToList();

                _sensorDetections = _sensorDetections
                    .TakeLast(_sensorDetections.Count / 2)
                    .ToList();

                savedDetections = savedDetections
                    .GroupBy(x => x.FullName)
                    .Select(cl => new SensorDetection
                    {
                        Name = cl.First().Name,
                        SensorName = cl.First().SensorName,
                        DateTime = DateTime.Now,
                        FullName = cl.First().FullName,
                        Value = cl.Sum(c => c.Value) / cl.Count(),
                    }).ToList();

                using var scope = _serviceScopeFactory.CreateScope();
                var repository = scope.ServiceProvider.GetRequiredService<SensorDetectionsRepository>();

                foreach (var savedDetection in savedDetections)
                {
                    await repository.Add(savedDetection);
                }
            }



            if (_sensorDetections.Count() > 100)
            {
                _logger.LogInformation($"All sensor detections was cleaned, because it's count is {_sensorDetections.Count()}");
                _sensorDetections = _sensorDetections.TakeLast(10).ToList();
            }

            _sensorDetections.AddRange(ParseDetections(detections));
        }

        private List<SensorDetection> ParseDetections(string detections)
        {
            // dht11:t=27.00;h=15.00;

            var detectionsList = new List<SensorDetection>();
            var colSymbolPosition = detections.IndexOf(':');
            var sensorName = detections.Substring(0, colSymbolPosition);

            var potentialDetections = detections.Substring(colSymbolPosition + 1, detections.Length - colSymbolPosition - 1).Split(';');
            foreach (var potentialDetection in potentialDetections)
            {
                var equalsSymbolPosition = potentialDetection.IndexOf('=');
                if (equalsSymbolPosition == -1)
                {
                    break;
                }

                var detectionName = potentialDetection.Substring(0, equalsSymbolPosition);

                var detectionValueLength = potentialDetection.Length - (equalsSymbolPosition + 1);
                var detectionValue = potentialDetection.Substring(equalsSymbolPosition + 1, detectionValueLength);


                if (double.TryParse(detectionValue, NumberStyles.Any, CultureInfo.InvariantCulture, out double valueD))
                {
                    detectionsList.Add(new SensorDetection()
                    {
                        Name = detectionName,
                        Value = valueD,
                        DateTime = DateTime.Now,
                        SensorName = sensorName,
                        FullName = $"{sensorName}_{detectionName}"
                    });
                }
            }

            return detectionsList;
        }

        public bool TryGetLastDetections(string sensorName, out List<SensorDetection> result)
        {
            var detections = GetDetections(sensorName);

            result = detections
                .GroupBy(x => x.FullName)
                .Select(x => x.First())
                .ToList();

            return result.Any();
        }

        private List<SensorDetection> GetDetections(string sensorName)
        {
            if (string.IsNullOrEmpty(sensorName) || _sensorDetections is null)
                return _sensorDetections ?? new List<SensorDetection>();

            return _sensorDetections
                .Where(x => x.SensorName.Equals(sensorName))
                .ToList();
        }
    }
}