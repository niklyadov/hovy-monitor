using System;
using System.IO;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace HovyMonitor.DeskBar.Win
{
    internal static class Program
    {
        public static readonly Configuration Configuration;
        public static readonly DetectionsService DetectionsService;

        static Program()
        {
            var configPath = Path.Combine(Environment.CurrentDirectory, "appsettings.json");
            if(!File.Exists(configPath))
            {
                Configuration = new Configuration();
                File.WriteAllText(configPath, JsonConvert.SerializeObject(Configuration));
            } else
            {
                Configuration = JsonConvert.DeserializeObject<Configuration>(File.ReadAllText(configPath));
            }
            DetectionsService = new DetectionsService(Configuration.DetectionService);
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
        }
    }
}
