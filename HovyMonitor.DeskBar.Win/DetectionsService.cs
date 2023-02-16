using HovyMonitor.Entity;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HovyMonitor.DeskBar.Win
{
    class DetectionsService
    {
        private readonly DetectionServiceConfiguration _configuration;

        private SearchOptions _searchOptions = new SearchOptions(DateTime.Now, 1);


        public SearchOptions SearchOptions { 
            get
            {
                return _searchOptions;
            }
            set
            {
                _searchOptions = value;
            }
        }

        private List<SensorDetection> _sensorDetectionsCache
            = new List<SensorDetection>();

        public DetectionsService(DetectionServiceConfiguration configuration)
        {
            _configuration = configuration;
        }

        public List<SensorDetection> GetDetectionsForSensor(string sensorName)
        {
            try
            {
                string contents;
                using (var wc = new System.Net.WebClient())
                {
                    var url = (_configuration.BaseUri + _configuration.LastSensorDetectionsUrl)
                        .Replace("{{sensorName}}", sensorName);

                    contents = wc.DownloadString(url);
                }

                var detections = JsonConvert.DeserializeObject<List<SensorDetection>>(contents);

                DetectionsAvgTrendStore(detections);

                return detections;
            }
            catch
            {
                return null;
            }
        }

        private void DetectionsAvgTrendStore(List<SensorDetection> detections)
        {
            foreach (var detection in detections)
            {
                var cachedWithNameOld = _sensorDetectionsCache
                    .Where(x => x.FullName == detection.FullName)
                    .Where(x => DateTime.UtcNow - x.DateTime >
                        TimeSpan.FromMinutes(_configuration.AvgDetectionTimeInMinutes))
                    .OrderBy(x => x.DateTime)
                    .ToList();

                foreach (var item in cachedWithNameOld)
                    _sensorDetectionsCache.Remove(item);

                _sensorDetectionsCache.Add(detection);
            }
        }

        public void GetLastSensorDetections(Action<List<SensorDetection>> action)
        {
            Task.Run(() =>
            {
                var detections = new List<SensorDetection>();

                var definedSensors = Program.Configuration.DetectionService.Sensors;
                foreach (var sensor in definedSensors)
                {
                    var response = GetDetectionsForSensor(sensor.Name);

                    if (response != null)
                    {
                        detections.AddRange(response);
                    }

                }

                action.Invoke(detections);
            });
        }

        public void GetSensorDetectionsList(Action<List<SensorDetection>> action, DateTime selectDatetime)
        {
            Task.Run(() =>
            {
                try
                {
                    string contents;
                    using (var wc = new System.Net.WebClient())
                    {
                        var url = (_configuration.BaseUri + _configuration.ListOfSensorDetectionsUrl)
                            .Replace("{{date}}", selectDatetime.ToShortDateString());

                        contents = wc.DownloadString(url);
                    }

                    action.Invoke(JsonConvert.DeserializeObject<List<SensorDetection>>(contents));
                }
                catch
                {
                }

            });
        }

        public AvgTrend GetAvgTrend(string sensorname)
        {
            var cachedWithName = _sensorDetectionsCache
                .Where(x => x.FullName == sensorname)
                .OrderBy(x => x.DateTime)
                .ToList();

            if (cachedWithName.Count < 2)
                return AvgTrend.Netural;

            var avgWithoutLast = cachedWithName
                .Take(cachedWithName.Count - 1)
                .Average(x => x.Value);

            var lastValue = cachedWithName.Last().Value;

            if (lastValue == avgWithoutLast) 
                return AvgTrend.Netural;

            return lastValue > avgWithoutLast 
                ? AvgTrend.Up : AvgTrend.Down;
        }
    }

    class SearchOptions
    {
        public DateTime SearchDateTime { get; private set; }

        public uint LastDays { get; private set; }

        public SearchOptions(DateTime searchDateTime, uint lastDays) {
            SearchDateTime = searchDateTime;
            LastDays = lastDays;
        }
    }

    public enum AvgTrend
    {
        Down = 0,
        Netural,
        Up
    }
}