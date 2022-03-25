using System.Globalization;

namespace HovyMonitor.Entity
{
    public class SensorDetection
    {
        public string Name { get; set; }
        public double Value { get; set; }

        public SensorDetection()
        {

        }

        public SensorDetection(string name, string value)
        {
            Name = name;

            if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out double valueD))
            {
                Value = valueD;
            }
        }
    }
}
