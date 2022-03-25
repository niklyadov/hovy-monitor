using System.Collections.Generic;

namespace HovyMonitor.Entity
{
    public class SensorDetections
    {
        public string Id { get; set; }
        public string SensorName { get; set; }
        public List<SensorDetection> Detections { get; set; }
            = new List<SensorDetection>();

        public SensorDetections()
        {

        }

        public SensorDetections(string detections)
        {
            // dht11:t=27.00;h=15.00;

            var colSymbolPosition = detections.IndexOf(':');
            SensorName = detections.Substring(0, colSymbolPosition);

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

                Detections.Add(new SensorDetection(detectionName, detectionValue));
            }

        }
    }
}