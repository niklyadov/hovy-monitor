using HovyMonitor.Entity;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using SelectionMode = System.Windows.Forms.SelectionMode;

namespace HovyMonitor.DeskBar.Win
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private SensorDetection[] _cachedDetections;
        private string[] _cachedSensornames;

        protected override void OnLoad(EventArgs e)
        {
            var screen = Screen.FromPoint(Location);
            Location = new Point(screen.WorkingArea.Right - Width + 7, 
                screen.WorkingArea.Bottom - Height + 7);
            base.OnLoad(e);
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            UseWaitCursor = true;
            UpdateListOfDetections(DateTime.Now, 2, () =>
            {
                sensorsLB.Invoke((MethodInvoker)delegate
                {
                    sensorsLB.SelectionMode = SelectionMode.MultiExtended;

                    foreach (var item in _cachedSensornames)
                    {
                        sensorsLB.Items.Add(item);
                        sensorsLB.SelectedItems.Add(item);
                    }
                });

                plotView1.Invoke((MethodInvoker)delegate
                {
                    plotView1.Model = GetPlotModelDetections(_cachedDetections);
                });

                dateTimePicker.Invoke((MethodInvoker)delegate
                {
                    dateTimePicker.Value = DateTime.Now;
                });

                UseWaitCursor = false;
            });
        }

        private void UpdateListOfDetections(DateTime dateTime, uint lastDays, Action callback)
        {
            Program.DetectionsService.SearchOptions = new SearchOptions(dateTime, lastDays);
            Program.DetectionsService.GetSensorDetectionsList((detections) =>
            {
                _cachedDetections = detections.ToArray();
                _cachedSensornames = ExtractSensorNames(detections).ToArray();
                callback.Invoke();
            });
        }

        private PlotModel GetPlotModelDetections(SensorDetection[] detections)
        {
            DateTimeAxis axesX = new DateTimeAxis
            {
                Position = AxisPosition.Bottom,
                StringFormat = "dd MM (HH:mm)",
                Title = "Date time",
                MinorIntervalType = DateTimeIntervalType.Hours,
                IntervalType = DateTimeIntervalType.Days,
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Dot,
            };

            PlotModel n = new PlotModel
            {
                Title = string.Join(", ", ExtractSensorNames(detections).ToArray())
            };

            foreach (var groupedDetections in detections
                .Where(x => sensorsLB.SelectedItems.Contains(x.FullName))
                .GroupBy(x => x.FullName))
            {
                FunctionSeries fs = new FunctionSeries()
                {
                    Title = groupedDetections.First().FullName,
                };

                foreach (var detection in groupedDetections.OrderBy(x => x.DateTime))
                {
                    var timeOfDay = detection.DateTime.ToLocalTime();
                    var point = new DataPoint(DateTimeAxis.ToDouble(timeOfDay), detection.Value);
                    fs.Points.Add(point);
                }

                n.Series.Add(fs);
            }


           
            n.Axes.Add(axesX);
            n.Axes.Add(new LinearAxis());

            return n;
        }

        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (sensorsLB.SelectedItems.Count == 1 
                && _cachedDetections != null 
                && _cachedDetections.Length > 0)
            {
                var valuesForDetection = _cachedDetections
                    .Where(x => x.FullName.Equals(sensorsLB.SelectedItems[0]))
                    .OrderByDescending(x => x.Value).ToList();

                var max = valuesForDetection.FirstOrDefault();
                var min = valuesForDetection.LastOrDefault();

                if (min == null || max == null)
                {
                    maxForSelectedValue.Text = $"no data";
                    minForSelectedValue.Text = $"no data";
                }

                maxForSelectedValue.Text = $"Max {max.Value} \t(for {max.FullName}) \tat {max.DateTime.ToLocalTime().ToString("dddd, dd MMMM yyyy HH:mm:ss")}";
                minForSelectedValue.Text = $"Min {min.Value} \t(for {max.FullName}) \tat {min.DateTime.ToLocalTime().ToString("dddd, dd MMMM yyyy HH:mm:ss")}";
                
                plotView1.Invoke((MethodInvoker)delegate
                {
                    plotView1.Model = GetPlotModelDetections(_cachedDetections);
                });
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            UseWaitCursor = true;
            UpdateListOfDetections(dateTimePicker.Value, (uint)lastDaysNUD.Value, () =>
            {
                plotView1.Invoke((MethodInvoker)delegate
                {
                    plotView1.Model = GetPlotModelDetections(_cachedDetections);
                });

                UseWaitCursor = false;
            });
        }

        private IEnumerable<string> ExtractSensorNames(ICollection<SensorDetection> detections)
        //    => Program.Configuration.DetectionService.Sensors.Select(x => x.Name);
            => detections.Select(x => x.FullName).Distinct();
    }
}