/* Stonk Widget for Twitch/OBS
 * Author: Nixka
 * Date: 26/04/2021 */
using System;
using LiveCharts;
using LiveCharts.Configurations;
using LiveCharts.Wpf;
using System.Windows.Forms;
using System.IO;
using System.Text;

namespace StonkMarket
{
    public partial class Form1 : Form
    {
        private double stonkChange = 0;
        private double stonkPrice = 1;
        private int viewers = 0;
        private double followers = 0;
        private double subcount = 0;
        private int chats = 0;
        private double tips = 0;
        private double bits = 0;

        private int oldi = 0;

        private bool HALT;
        //private double progressiveChange;
        //private bool isProgressive;

        public Form1()
        {
            InitializeComponent();
            Setup();
        }

        Func<double, string> formatFunc = (x) => string.Format("{0:0.00}", x);

        public void Setup()
        {
            var mapper = Mappers.Xy<MeasureModel>()
               .X(model => model.DateTime.Ticks)   //use DateTime.Ticks as X
               .Y(model => model.Value);           //use the value property as Y

            //lets save the mapper globally.
            Charting.For<MeasureModel>(mapper);

            //the ChartValues property will store our values array
            ChartValues = new ChartValues<MeasureModel>();
            cartesianChart1.Series = new SeriesCollection
            {
                new LineSeries
                {
                    Values = ChartValues,
                    PointGeometrySize = 0,
                    StrokeThickness = 8,
                    LineSmoothness = 0.1,
                    Fill = System.Windows.Media.Brushes.Transparent,
                    Stroke = System.Windows.Media.Brushes.White
                }
            };
            cartesianChart1.AxisX.Add(new Axis
            {
                DisableAnimations = true,
                LabelFormatter = value => new DateTime((long)value).ToString("mm:ss"),
                Separator = new Separator
                {
                    Step = TimeSpan.FromSeconds(1).Ticks,
                    StrokeThickness = 0
                }
            });
            cartesianChart1.AxisY.Add(new Axis
            {
                FontSize = 20,
                LabelFormatter = formatFunc,
                Separator = new Separator
                {
                    StrokeThickness = 0
                }
            });;

            // Get Initial Values
            bits = double.Parse(File.ReadAllText("C:/Users/Nikat/Desktop/Twitch Stuff/Labels/total_cheer_amount.txt"));
            tips = double.Parse(File.ReadAllText("C:/Users/Nikat/Desktop/Twitch Stuff/Labels/total_donation_amount.txt").Split('$')[1]);
            followers = double.Parse(File.ReadAllText("C:/Users/Nikat/Desktop/Twitch Stuff/Labels/total_follower_count.txt"));
            subcount = int.Parse(File.ReadAllText("C:/Users/Nikat/Desktop/Twitch Stuff/Labels/total_subscriber_count.txt"));

            SetAxisLimits(DateTime.Now);

            //The next code simulates data changes every 500 ms
            Timer = new Timer
            {
                Interval = 2000
            };
            Timer.Tick += TimerOnTick;
            R = new Random();
            Timer.Start();
        }

        private ChartValues<MeasureModel> ChartValues { get; set; }
        public Timer Timer { get; set; }
        public Random R { get; set; }

        private void SetAxisLimits(DateTime now)
        {
            //cartesianChart1.AxisX[0].MaxValue = now.Ticks + TimeSpan.FromSeconds(10).Ticks; // lets force the axis to be 100ms ahead
            cartesianChart1.AxisX[0].MinValue = now.Ticks - TimeSpan.FromSeconds(60).Ticks; // we only care about the last 30 seconds
        }

        private void UpdateStonks()
        {
            /*
             * Method to update stonks
             */

            // Followers and Subs
            // Get follower and sub count
            double newFollowers = double.Parse(File.ReadAllText("C:/Users/Nikat/Desktop/Twitch Stuff/Labels/total_follower_count.txt"));
            double newSubcount = double.Parse(File.ReadAllText("C:/Users/Nikat/Desktop/Twitch Stuff/Labels/total_subscriber_count.txt"));
            double newBits = double.Parse(File.ReadAllText("C:/Users/Nikat/Desktop/Twitch Stuff/Labels/total_cheer_amount.txt"));
            double newTips = double.Parse(File.ReadAllText("C:/Users/Nikat/Desktop/Twitch Stuff/Labels/total_donation_amount.txt").Split('$')[1]);

            // Apply changes according 
            if (newFollowers != followers)
            {
                if (newFollowers > followers)
                {
                    stonkChange += (newFollowers - followers) * 0.1;
                }
                else
                {
                    stonkChange += (newFollowers - followers) * 0.1;
                }

                followers = newFollowers;
            }
            if (newSubcount != subcount)
            {
                if (newSubcount > subcount)
                {
                    stonkChange += (newSubcount - subcount) * 1;
                }

                subcount = newSubcount;
            }
            if (newBits > bits)
            {
                stonkChange += (newBits - bits) * 0.005;
                bits = newBits;
            }
            if (newTips > tips)
            {
                stonkChange += (newTips - tips) * 0.5;
                tips = newTips;
            }

            // Viewers and Chat
            // Get chat logs
            using (var fs = new FileStream("C:/Users/Nikat/Desktop/Twitch Stuff/StonkMarket/Data/#nixka.log", FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var sr = new StreamReader(fs, Encoding.Default))
            {
                int i = 0;
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    if (line.Contains("VIEWERS") && i > oldi)
                    {
                        int newViewers = int.Parse(line.Split(':')[1].Substring(1));
                        if (newViewers > viewers)
                        {
                            stonkChange += (newViewers - viewers) * 0.1;
                            //MessageBox.Show($"new {newViewers} and old {viewers}, stonkchange {stonkChange}, index {i}");
                        }
                        else if (newViewers < viewers)
                        {
                            stonkChange -= (viewers - newViewers) * 0.05;
                        }
                        oldi = i;
                        viewers = newViewers;
                    }
                    else 
                    { 
                        i++;
                    }
                }
                if (i > chats)
                {
                    stonkChange += (i- chats) * 0.005;
                    chats = i;
                }
                
            }
            /* 
             * CANT USE THIS METHOD WHEN TEXT FILE BEING USED BY OTHER PROCESS
             * 
            string[] lines = File.ReadAllLines("C:/Users/Nika/Desktop/Twitch Stuff/StonkMarket/Data/#nixka.log");
            foreach (string line in lines)
            {
                if (line.Contains("VIEWERS"))
                {
                    int newViewers = int.Parse(line.Split(':')[2].Substring(1));
                    if (newViewers > viewers)
                    {
                        stonkChange += (newViewers - viewers) * 0.2;
                    }
                }
            }
            if (lines.Length > chats)
            {
                stonkChange += (lines.Length - chats) * 0.01;
                chats = lines.Length;
            }*/

            // Change label color if anything changed prior to auto adjustment
            if (stonkChange > 0)
            {
                label1.ForeColor = System.Drawing.Color.Lime;
            }
            else if (stonkChange < 0)
            {
                label1.ForeColor = System.Drawing.Color.Red;
            }
            else
            {
                label1.ForeColor = System.Drawing.Color.White;
            }

            // Randomizer (randomize stonkchange so its not a straight line)
            if (stonkChange == 0 && !HALT)
            {
                Random r = new Random();
                int num = r.Next(0, 10);
                if (num > 4)
                {
                    stonkChange += double.Parse($"0.00{r.Next(0, 10)}");
                }
                else
                {
                    stonkChange -= double.Parse($"0.00{r.Next(0, 10)}");
                }
            }


            /* Update stonk price progressively if high 
            
            if (stonkChange > 2) {
                progressiveChange = stonkChange / 5;
            }
            else {
                stonkPrice += stonkChange;
            }*/

            // Update stonk price with changes
            stonkPrice += stonkChange;
            label1.Text = "$" + stonkPrice.ToString("0.00");
            stonkChange = 0;
        }

        private void TimerOnTick(object sender, EventArgs eventArgs)
        {
            var now = DateTime.Now;

            ChartValues.Add(new MeasureModel
            {
                DateTime = now,
                Value = stonkPrice
            });

            SetAxisLimits(now);
            UpdateStonks();
            
            // lets only use the last 30 values
            if (ChartValues.Count > 90) ChartValues.RemoveAt(0);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            stonkChange += 0.5;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            stonkChange -= 0.5;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            HALT = !HALT;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            stonkPrice = double.Parse(textBox1.Text);
        }
    }
}
