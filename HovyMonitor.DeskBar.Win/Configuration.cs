using System.Collections.Generic;

namespace HovyMonitor.DeskBar.Win
{
    public class Configuration
    {
        public DetectionServiceConfiguration DetectionService { get; set; }
            = new DetectionServiceConfiguration();
        public TimerConfiguration Timer { get; set; } 
            = new TimerConfiguration();

        public UIConfiguration UI { get; set; } 
            =  new UIConfiguration();
    }

    public class DetectionServiceConfiguration
    {
        public string ApiString { get; set; } = "http://localhost:6400/detections/last?sensorName={{sensorName}}";
    }

    public class TimerConfiguration
    {
        public int UpdateTimeout { get; set; } = 5000;
    }

    public class UIConfiguration
    {
        public bool UseColorsForText { get; set; } = true;
        public List<SensorDetectionConfiguration> SensorDetections { get; set; }
            = new List<SensorDetectionConfiguration>();
    }


    public class SensorDetectionConfiguration
    {
        public string Name { get; set; }
        public int[] Values { get; set; }
        public string Color { get; set; }
        public string Description { get; set; }
    }
}
