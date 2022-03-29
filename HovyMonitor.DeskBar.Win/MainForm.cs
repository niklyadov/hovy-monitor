using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
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

            #region shit
            //var chartArea = chart1.ChartAreas[0];
            //chartArea.AxisX.ScaleView.Zoomable = true;
            //chartArea.CursorX.AutoScroll = true;
            //chartArea.CursorX.IsUserSelectionEnabled = true;
            ////chartArea.AxisX.LabelStyle.Format = "dd/MM/yyyy";
            //chartArea.AxisX.Interval = 1;
            ////chartArea.AxisX.IntervalType = DateTimeIntervalType.Days;
            //chartArea.AxisX.IntervalOffset = 1;
            ////chartArea.AxisX.Maximum = DateTime.Now.Ticks;


            //var random = new Random();
            //var series = new Series()
            //{
            //    Color = Color.Red,
            //    //Legend = "Legend",
            //    ChartArea = "ChartArea1",
            //    ChartType = SeriesChartType.FastLine,
            //    XValueType = ChartValueType.DateTime
            //};

            //DateTime dt = DateTime.Now;

            //for (int i = 1; i < 100; i++)
            //{
            //    dt = dt.AddDays(i);
            //    series.Points.AddXY(dt, random.NextDouble() * 25);
            //}

            //chart1.Series.Clear();
            //chart1.Series.Add(series);

            #endregion

            TimeSpanAxis xAxis = new TimeSpanAxis
            {
                Position = AxisPosition.Bottom,
                Title = "Time Of Day",
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.None,
            };

            var myModel = new PlotModel
            {
                Title = "Temperature, Humidity, CO2"
            };
            myModel.Axes.Add(xAxis);
            myModel.Axes.Add(new LinearAxis());

            Program.DetectionsService.GetSensorDetectionsList((detections) =>
            {
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

                    myModel.Series.Add(fs);
                }
            });

          
            plotView1.Model = myModel;
        }
    }
}