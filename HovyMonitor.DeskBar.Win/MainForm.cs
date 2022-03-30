using HovyMonitor.Entity;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace HovyMonitor.DeskBar.Win
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            var screen = Screen.FromPoint(Location);
            Location = new Point(screen.WorkingArea.Right - Width + 7, 
                screen.WorkingArea.Bottom - Height + 7);
            base.OnLoad(e);
        }


        private void MainForm_Load(object sender, EventArgs e)
        {
            dateTimePicker1.Value = DateTime.Now;

            Program.DetectionsService.GetSensorDetectionsList((detections) =>
                plotView1.Invoke((MethodInvoker)delegate
                {
                    plotView1.Model = GetPlotModelDetections(detections);
                }),
                dateTimePicker1.Value
            );
        }

        private PlotModel GetPlotModelDetections(List<SensorDetection> detections)
        {
            var xAxis = new TimeSpanAxis
            {
                Position = AxisPosition.Bottom,
                Title = "Time Of Day",
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.None,
            };

            var plotModel = new PlotModel
            {
                Title = "Temperature, Humidity, CO2"
            };
            plotModel.Axes.Add(xAxis);
            plotModel.Axes.Add(new LinearAxis());

            var grouped = detections.GroupBy(x => x.FullName);
            foreach (var groupedDetections in grouped)
            {
                FunctionSeries fs = new FunctionSeries()
                {
                    Title = groupedDetections.First().FullName
                };

                foreach (var detection in groupedDetections)
                {
                    var point = new DataPoint(TimeSpanAxis.ToDouble(detection.DateTime.TimeOfDay), detection.Value);
                    fs.Points.Add(point);
                }

                plotModel.Series.Add(fs);
            }

            return plotModel;
        }

        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            Program.DetectionsService.GetSensorDetectionsList((detections) =>
                plotView1.Invoke((MethodInvoker)delegate
                {
                    plotView1.Model = GetPlotModelDetections(detections);
                }),
                ((DateTimePicker)sender).Value
            );
        }
    }
}