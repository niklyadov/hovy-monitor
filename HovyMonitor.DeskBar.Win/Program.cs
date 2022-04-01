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
        public static readonly string CurrentDir = @"C:\Program Files\HovyMonitorBar";

        static Program()
        {
            var configPath = Path.Combine(CurrentDir, "appsettings.json");
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
            //Application.Run(new MainForm());
        }
    }
}
