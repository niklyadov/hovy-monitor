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

        public DetectionsService(DetectionServiceConfiguration configuration)
        {
            _configuration = configuration;
        }

        public List<SensorDetection> GetDetectionsForSensor(string sensorName, int count)
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

                return JsonConvert.DeserializeObject<List<SensorDetection>>(contents);
            }
            catch
            {
                return null;
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
                    var response = GetDetectionsForSensor(sensor.Name, sensor.Detections.Count);

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
                            .Replace("{{date}}", selectDatetime.ToString(_configuration.DateTimeFormat));

                        contents = wc.DownloadString(url);
                    }

                    action.Invoke(JsonConvert.DeserializeObject<List<SensorDetection>>(contents));
                }
                catch
                {
                }
            });
        }
    }
}