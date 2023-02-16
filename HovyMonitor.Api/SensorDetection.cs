using System;

namespace HovyMonitor.Entity
{
    public class SensorDetection
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public double Value { get; set; }
        public DateTime DateTime { get; set; }
        public string SensorName { get; set; }
        public string FullName { get; set; }

    }
}