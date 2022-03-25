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
        public string Proto { get; set; } = "http";
        public string Host { get; set; } = "localhost";
        public uint Port { get; set; } = 6400;
    }

    public class TimerConfiguration
    {
        public int UpdateTimeout { get; set; } = 5000;
    }

    public class UIConfiguration
    {
        public bool UseColorsForText { get; set; } = true;
    }
}
