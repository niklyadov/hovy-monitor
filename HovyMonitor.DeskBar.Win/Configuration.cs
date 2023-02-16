using System.Collections.Generic;
using System.Net.Http.Headers;

namespace HovyMonitor.DeskBar.Win
{
    public class Configuration
    {
        public DetectionServiceConfiguration DetectionService { get; set; }
            = new DetectionServiceConfiguration();

        public UIConfiguration UI { get; set; } 
            =  new UIConfiguration();
    }

    public class DetectionServiceConfiguration
    {
        public string BaseUri { get; set; } = "http://localhost:6400/detections";
        public string LastSensorDetectionsUrl { get; set; } = "/last?sensorName={{sensorName}}&count={{count}}";
        public string ListOfSensorDetectionsUrl { get; set; } = "/";
        public List<SensorConfiguration> Sensors { get; set; }
            = new List<SensorConfiguration>();
        public string DateTimeFormat { get; set; } = "MM.dd.yyyy";

        public int RefreshTimeout { get; set; } = 5000;
        public int AvgDetectionTimeInMinutes { get; set; } = 60;
    }

    public class UIConfiguration
    {

        public LabelConfiguration FirstLabel { get; set; }

        public LabelConfiguration SecondLabel { get; set; }
    }

    public class SensorConfiguration
    {
        public string Name { get; set; }
        public List<string> Detections { get; set; }
            = new List<string>();

    }

    public class LabelConfiguration
    {
        public List<LabelDetectionConfigurations> Detections { get; set; } 
            = new List<LabelDetectionConfigurations>();
    }

    public class LabelDetectionConfigurations
    {
        public string Format { get; set; }
        public bool CustomColors { get; set; }

        public List<LabelColorsConfiguration> Colors { get; set; }
            = new List<LabelColorsConfiguration>();
    }

    public class LabelColorsConfiguration
    {
        public string SensorName { get; set; }
        public string SensorDetection { get; set; }
        public double[] Values { get; set; }
        public string ColorHEX { get; set; }
    }
}
