using System;
using System.Threading.Tasks;
using HovyMonitor.Entity;
using Newtonsoft.Json;

namespace HovyMonitor.DeskBar.Win
{
    class DetectionsService
    {
        private readonly DetectionServiceConfiguration _configuration;

        public DetectionsService(DetectionServiceConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void GetDetectionsForSensor(string sensorName, Action<SensorDetections> action)
        {
            Task.Run(() =>
            {
                try
                {
                    string contents;
                    using (var wc = new System.Net.WebClient())
                    {
                        contents = wc.DownloadString(_configuration.ApiString.Replace("{{sensorName}}", sensorName));
                    }

                    action.Invoke(JsonConvert.DeserializeObject<SensorDetections>(contents));
                }
                catch
                {
                    action.Invoke(null);
                }
            });
        }
    }
}