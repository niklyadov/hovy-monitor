using Microsoft.Extensions.Configuration;
using System;

namespace HovyMonitor.Api.Entity
{
    public class Configuration
    {
        public SerialPortConfiguration SerialPort { get; set; } 
            = new ();

        public List<String> CommandsForSurveysOfSensors { get; set; } 
            = new();
    }

    public class SerialPortConfiguration
    {
        public int BandRate { get; set; } = 115200;
        public byte DataBits { set; get; } = 8;
        public int ReadTimeout { get; set; } = 2000;
        public int SendInterval { get; set; } = 5000;
    }
}
