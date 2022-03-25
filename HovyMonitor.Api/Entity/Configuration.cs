namespace HovyMonitor.Api.Entity
{
    public class Configuration
    {
        public SerialPortConfiguration SerialPort { get; set; } 
            = new SerialPortConfiguration();
    }

    public class SerialPortConfiguration
    {
        public string Name { get; set; } = "COM4";
        public int BaundRate { get; set; } = 9600;
        public byte DataBits { set; get; } = 8;
        public int ReadTimeout { get; set; } = 2000;
    }
}
