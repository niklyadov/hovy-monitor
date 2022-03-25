using System;
using CSDeskBand;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using CSDeskBand.Win;
using System.Diagnostics;
using System.Drawing;
using System.Linq;

namespace HovyMonitor.DeskBar.Win
{
    [ComVisible(true)]
    [Guid("5731FC61-8530-404C-86C1-86CCB8738D05")]
    [CSDeskBandRegistration(Name = "HovyMonitorBar")]
    public partial class Deskband : CSDeskBandWin
    {
        private Label humidityAndTemperature;
        private Label CO2Dim;
        private Timer Timer;

        public Deskband()
        {
            Options.MinHorizontalSize = new System.Drawing.Size(75, 30);
            InitializeComponent();

            ContextMenu cm = new ContextMenu();
            cm.MenuItems.Add("Configure options", Deskband_Context_Show_Options_Click);
            cm.MenuItems.Add("-");
            cm.MenuItems.Add("Re-install", Deskband_Context_Reinstall_Click);
            cm.MenuItems.Add("Uninstall", Deskband_Context_Uninstall_Click);
            cm.MenuItems.Add("-");
            cm.MenuItems.Add("About", Deskband_Context_About_Click);
            ContextMenu = cm;

            Timer = new Timer();
            Timer.Tick += new EventHandler(Timer_Tick);
            Timer.Interval = Program.Configuration.Timer.UpdateTimeout;
            Timer.Start();
        }

        private void InitializeComponent()
        {
            this.humidityAndTemperature = new System.Windows.Forms.Label();
            this.CO2Dim = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // humidityAndTemperature
            // 
            this.humidityAndTemperature.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.humidityAndTemperature.ForeColor = System.Drawing.SystemColors.Control;
            this.humidityAndTemperature.Location = new System.Drawing.Point(0, 0);
            this.humidityAndTemperature.Margin = new System.Windows.Forms.Padding(0);
            this.humidityAndTemperature.Name = "humidityAndTemperature";
            this.humidityAndTemperature.Size = new System.Drawing.Size(75, 20);
            this.humidityAndTemperature.TabIndex = 0;
            this.humidityAndTemperature.Text = "? ? ?";
            this.humidityAndTemperature.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            // 
            // CO2Dim
            // 
            this.CO2Dim.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.CO2Dim.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.CO2Dim.ForeColor = System.Drawing.SystemColors.Control;
            this.CO2Dim.Location = new System.Drawing.Point(0, 20);
            this.CO2Dim.Margin = new System.Windows.Forms.Padding(0);
            this.CO2Dim.Name = "CO2Dim";
            this.CO2Dim.Size = new System.Drawing.Size(75, 20);
            this.CO2Dim.TabIndex = 1;
            this.CO2Dim.Text = "? ? ?";
            this.CO2Dim.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // Deskband
            // 
            this.BackColor = System.Drawing.Color.Black;
            this.Controls.Add(this.CO2Dim);
            this.Controls.Add(this.humidityAndTemperature);
            this.Name = "Deskband";
            this.Size = new System.Drawing.Size(75, 40);
            this.Click += new System.EventHandler(this.Deskband_Click);
            this.ResumeLayout(false);

        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            Program.DetectionsService.GetDetectionsForSensor("dht11", (dhtDetections) =>
            {
                if (dhtDetections == null)
                    return;

                var humidity = dhtDetections.Detections.Find(x => x.Name == "h");
                var temperature = dhtDetections.Detections.Find(x => x.Name == "t");

                if (temperature == null || humidity == null)
                    return;

                humidityAndTemperature.Invoke((MethodInvoker)delegate
                {
                    humidityAndTemperature.Text = $"{temperature.Value} °C, {humidity.Value} %";

                    if(Program.Configuration.UI.UseColorsForText)
                    {
                        humidityAndTemperature.ForeColor = GetColorForHumidity(humidity.Value);
                    }
                });
            });

            Program.DetectionsService.GetDetectionsForSensor("mh-z19b", (z19Detections) =>
            {
                if (z19Detections == null)
                    return;

                var CO2Value = z19Detections.Detections.Find(x => x.Name == "co2");

                if (CO2Value == null)
                    return;

                CO2Dim.Invoke((MethodInvoker)delegate
                {
                    CO2Dim.Text = $"{CO2Value.Value} Ppm";


                    if (Program.Configuration.UI.UseColorsForText)
                    {
                        CO2Dim.ForeColor = GetColorForCo2(CO2Value.Value);
                    }
                });
            });

            //var CO2Value = random.Next(1800, 1900);
            //CO2Dim.ForeColor = (CO2Value > 1850) ? Color.Red : Color.WhiteSmoke;
            //CO2Dim.Text = $"{random.Next(1800, 1900)}";
        }

        private void Deskband_Click(object sender, EventArgs e)
        {

        }

        private void Deskband_Context_Show_Options_Click(object sender, EventArgs e)
        {
            ExploreFile(System.IO.Path.Combine(Environment.CurrentDirectory, "appsettings.json"));
        }

        private void Deskband_Context_Reinstall_Click(object sender, EventArgs e)
        {
            RunBat(System.IO.Path.Combine(Environment.CurrentDirectory, "install_script.bat"), Environment.CurrentDirectory);
        }

        private void Deskband_Context_Uninstall_Click(object sender, EventArgs e)
        {
            RunBat(System.IO.Path.Combine(Environment.CurrentDirectory, "uninstall_script.bat"), Environment.CurrentDirectory);
        }

        private void Deskband_Context_About_Click(object sender, EventArgs e)
        {
            MessageBox.Show("HovyMonitor(.DeskBar.Win) - v.0.1.1" +
                "\n\n" +
                "24.03.2022");
        }

        public bool ExploreFile(string filePath)
        {
            if (!System.IO.File.Exists(filePath))
            {
                return false;
            }
            //Clean up file path so it can be navigated OK
            filePath = System.IO.Path.GetFullPath(filePath);
            System.Diagnostics.Process.Start("explorer.exe", string.Format("/select,\"{0}\"", filePath));
            return true;
        }

        private Color GetColorForHumidity(double value)
        {

            if (TestRange(value, 0, 20))
                return Color.IndianRed;

            if(TestRange(value, 21, 40) || TestRange(value, 66, 100))
                return Color.Yellow;

            return Color.White;
        }

        private Color GetColorForCo2(double value)
        {
            if (TestRange(value, 0, 400))
                return Color.AliceBlue;

            if (TestRange(value, 700, 1000))
                return Color.Yellow;

            if (TestRange(value, 1000, 2000))
                return Color.IndianRed;


            return Color.White;
        }

        bool TestRange(double numberToCheck, int bottom, int top)
        {
            return (numberToCheck >= bottom && numberToCheck <= top);
        }
        public void RunBat(string filePath, string workingDir)
        {
            try
            {
                ProcessStartInfo procInfo = new ProcessStartInfo();
                procInfo.UseShellExecute = true;
                procInfo.FileName = filePath;  //The file in that DIR.
                procInfo.WorkingDirectory = workingDir; //The working DIR.
                procInfo.Verb = "runas";
                Process.Start(procInfo);  //Start that process.

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }
    }
}