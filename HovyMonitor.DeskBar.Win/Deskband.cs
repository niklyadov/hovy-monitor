using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Diagnostics;
using System.Linq;
using System.Drawing;
using System.Globalization;
using System.IO;
using HovyMonitor.Entity;
using System.Collections.Generic;
using CSDeskBand.Win;
using CSDeskBand;
using System.Reflection;

namespace HovyMonitor.DeskBar.Win
{
    [ComVisible(true)]
    [Guid("5731FC61-8530-404C-86C1-86CCB8738D05")]
    [CSDeskBandRegistration(Name = "HovyMonitorBar")]
    public partial class Deskband : CSDeskBandWin
    {
        private Label FirstLabel;
        private Label SecondLabel;
        private Color FirstLabelTargetColor;
        private Color SecondLabelTargetColor;
        private Timer FetchNewDataTimer;

        private Form FormGui;

        private int LastFirstLabelDetectionsIndex = -1;
        private int LastSecondLabelDetectionsIndex = -1;

        public Deskband()
        {
            Options.MinHorizontalSize = new System.Drawing.Size(75, 30);
            InitializeComponent();

            ContextMenu cm = new ContextMenu();

            cm.MenuItems.Add("Show GUI", (object sender, EventArgs e) => {
                if(FormGui != null)
                {
                    FormGui.Close();
                }
                FormGui = new MainForm();
                FormGui.Show();
            });

            cm.MenuItems.Add("Configure options", (object sender, EventArgs e) => 
                ExploreFile(Program.AppsettingsLocation));

            cm.MenuItems.Add("-");

            cm.MenuItems.Add("Install last update (*New)", (object sender, EventArgs e) =>
                RunBat(Program.UpdaterLocation, Environment.CurrentDirectory));

            cm.MenuItems.Add("Re-install", (object sender, EventArgs e) => 
                RunBat(Program.ReinstallScriptLocation, Environment.CurrentDirectory));

            cm.MenuItems.Add("Uninstall", (object sender, EventArgs e) =>
                RunBat(Program.UninstallScriptLocation, Environment.CurrentDirectory));

            cm.MenuItems.Add("-");

            cm.MenuItems.Add("About", (object sender, EventArgs e) =>
                MessageBox.Show($"HovyMonitor(.DeskBar.Win) - {Assembly.GetExecutingAssembly().GetName().Version}" +
                    "\n\n" +
                    File.ReadAllText(Program.ChangelogLocation)));

            ContextMenu = cm;

            FetchNewDataTimer = new Timer();
            FetchNewDataTimer.Tick += new EventHandler(Timer_Tick);
            FetchNewDataTimer.Interval = Program.Configuration.DetectionService.RefreshTimeout;
            FetchNewDataTimer.Start();
        }

        private void InitializeComponent()
        {
            this.FirstLabel = new System.Windows.Forms.Label();
            this.SecondLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // FirstLabel
            // 
            this.FirstLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.FirstLabel.BackColor = System.Drawing.Color.Transparent;
            this.FirstLabel.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.FirstLabel.ForeColor = System.Drawing.SystemColors.Control;
            this.FirstLabel.Location = new System.Drawing.Point(0, 0);
            this.FirstLabel.Margin = new System.Windows.Forms.Padding(0);
            this.FirstLabel.Name = "FirstLabel";
            this.FirstLabel.Size = new System.Drawing.Size(100, 20);
            this.FirstLabel.TabIndex = 0;
            this.FirstLabel.Text = "? ? ?";
            this.FirstLabel.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            // 
            // SecondLabel
            // 
            this.SecondLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.SecondLabel.BackColor = System.Drawing.Color.Transparent;
            this.SecondLabel.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.SecondLabel.ForeColor = System.Drawing.SystemColors.Control;
            this.SecondLabel.Location = new System.Drawing.Point(0, 20);
            this.SecondLabel.Margin = new System.Windows.Forms.Padding(0);
            this.SecondLabel.Name = "SecondLabel";
            this.SecondLabel.Size = new System.Drawing.Size(100, 20);
            this.SecondLabel.TabIndex = 1;
            this.SecondLabel.Text = "? ? ?";
            this.SecondLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // Deskband
            // 
            this.BackColor = System.Drawing.Color.Black;
            this.Controls.Add(this.SecondLabel);
            this.Controls.Add(this.FirstLabel);
            this.Name = "Deskband";
            this.Size = new System.Drawing.Size(100, 40);
            this.ResumeLayout(false);

        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            var firstLabelOptions = Program.Configuration.UI.FirstLabel;
            var secondLabelOptions = Program.Configuration.UI.SecondLabel;

            var indexOfFirstLabelToRender = LastFirstLabelDetectionsIndex + 1;

            if (indexOfFirstLabelToRender >= firstLabelOptions.Detections.Count)
                indexOfFirstLabelToRender = 0;

            LastFirstLabelDetectionsIndex = indexOfFirstLabelToRender;

            var indexOfSecondLabelToRender = LastSecondLabelDetectionsIndex + 1;

            if (indexOfSecondLabelToRender >= secondLabelOptions.Detections.Count)
                indexOfSecondLabelToRender = 0;

            LastSecondLabelDetectionsIndex = indexOfSecondLabelToRender;

            Program.DetectionsService.GetLastSensorDetections((detections) =>
            {
                ApplyLabel(FirstLabel, firstLabelOptions.Detections[indexOfFirstLabelToRender], detections);
                ApplyLabel(SecondLabel, secondLabelOptions.Detections[indexOfSecondLabelToRender], detections);
            });
        }

        private void ApplyLabel(Label label, LabelDetectionConfigurations labelConfiguration, List<SensorDetection> detections)
        {
            label.Invoke((MethodInvoker)delegate
            {
                var labelFormat = labelConfiguration.Format.ToString();
                var labelText = labelFormat;
                var labelColor = label.ForeColor;

                foreach (var detection in detections)
                {
                    var avgTrend = Program.DetectionsService.GetAvgTrend(detection.FullName);
                    var detectionStamp = "{{" + $"{detection.SensorName},{detection.Name}" + "}}";
                    labelText = labelText.Replace(detectionStamp, $"{detection.Value} {avgTrend}");

                    if (labelConfiguration.CustomColors)
                    {
                        var colorConfig = labelConfiguration.Colors
                            .FirstOrDefault(x => x.SensorName == detection.SensorName &&
                                        x.SensorDetection == detection.Name &&
                                        Math.Floor(detection.Value) >= x.Values[0] && Math.Floor(detection.Value) <= x.Values[1]);

                        if (colorConfig != null)
                        {
                            int argb = int.Parse(colorConfig.ColorHEX.Replace("#", ""),
                                NumberStyles.HexNumber);

                            labelColor = Color.FromArgb(argb);
                        }
                    }
                }

                label.Text = (labelText != labelFormat) ? labelText : " . . . ";

                if (label == FirstLabel)
                    FirstLabelTargetColor = labelColor;
                
                if(label == SecondLabel)
                    SecondLabelTargetColor = labelColor;
            });
        }

        private bool ExploreFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return false;
            }
            //Clean up file path so it can be navigated OK
            filePath = Path.GetFullPath(filePath);
            Process.Start("explorer.exe", string.Format("/select,\"{0}\"", filePath));
            return true;
        }

        private void RunBat(string filePath, string workingDir)
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

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            FirstLabel.ForeColor = MoveColorToColor(FirstLabel.ForeColor, FirstLabelTargetColor);
            SecondLabel.ForeColor = MoveColorToColor(SecondLabel.ForeColor, SecondLabelTargetColor);
        }

        private Color MoveColorToColor(Color colorFrom, Color colorTo)
        {
            byte r = colorFrom.R;
            byte g = colorFrom.G;
            byte b = colorFrom.B;

            if (r > colorTo.R) r--;
            else if (r < colorTo.R) r++;

            if (g > colorTo.G) g--;
            else if (g < colorTo.G) g++;

            if (b > colorTo.B) b--;
            else if (b < colorTo.B) b++;

            return Color.FromArgb(r, g, b);
        }
    }
}