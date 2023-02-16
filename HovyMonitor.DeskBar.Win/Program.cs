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
        public static readonly string AppsettingsLocation =
            Path.Combine(CurrentDir, "appsettings.json");
        public static readonly string ChangelogLocation =
            Path.Combine(CurrentDir, "Changelog.txt");
        public static readonly string UpdaterLocation = 
            Path.Combine(CurrentDir, "HovyMonitor.Deskbar.Win.Updater.exe");
        public static readonly string ReinstallScriptLocation =
            Path.Combine(CurrentDir, "install_script.bat");
        public static readonly string UninstallScriptLocation =
            Path.Combine(CurrentDir, "uninstall_script.bat");

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

            #if DEBUG
                Application.Run(new MainForm());
            #endif
        }
    }
}
