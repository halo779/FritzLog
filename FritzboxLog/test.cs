using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using OxyPlot.WindowsForms;

namespace FritzboxLog
{
    public partial class test : Form
    {

        public test()
        {
            InitializeComponent();
        }

        private void test_Load(object sender, EventArgs e)
        {
            Plot plot = new Plot();
            plot.Model = new OxyPlot.PlotModel();
            plot.Dock = DockStyle.Fill;
            this.Controls.Add(plot);

            plot.Model.PlotType = OxyPlot.PlotType.XY;

            var DS = new OxyPlot.Series.LineSeries();
            var US = new OxyPlot.Series.LineSeries();
            var piolt = new OxyPlot.Series.StemSeries();
            US.Color = OxyPlot.OxyColors.Green;
            DS.Color = OxyPlot.OxyColors.Blue;
            piolt.Color = OxyPlot.OxyColors.OrangeRed;

            

            string[] tmp = "0,0,0,0,5,9,10,10,10,10,11,10,10,10,10,9,9,9,9,9,8,8,8,9,9,8,9,9,8,8,8,8,8,8,8,8,8,9,9,9,8,9,8,8,8,9,8,9,9,9,8,9,9,8,9,8,8,8,8,8,8,8,8,8,13,14,14,14,14,14,14,14,15,13,14,14,14,14,14,14,14,14,14,14,13,11,0,2,6,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,6,6,7,6,6,6,6,6,6,6,6,5,4,6,6,2,6,6,4,4,6,5,5,5,5,5,5,5,5,5,5,5,4,5,5,5,4,4,5,5,4,1,7,12,12,13,12,13,13,12,13,12,12,13,12,12,13,12,12,12,12,12,10,12,12,12,12,12,12,12,12,12,12,12,10,12,12,12,12,12,12,12,12,12,12,10,12,12,12,12,12,12,12,12,12,12,11,7,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0".Split(',');
            int i = 0;
            int c = 0;
            string profile = "8b";

 
            foreach (var item in tmp)
            {
                if (profile == "8b" && i < 2048)
                {
                    if (c < 87 || c > 147)
                    {
                        DS.Points.Add(new OxyPlot.DataPoint((double)i, Convert.ToDouble(item)));
                        US.Points.Add(new OxyPlot.DataPoint((double)i, 0));
                    }
                    else
                    {
                        DS.Points.Add(new OxyPlot.DataPoint((double)i, 0));
                        US.Points.Add(new OxyPlot.DataPoint((double)i, Convert.ToDouble(item)));
                    }
                    if (c == 74)
                    {
                        piolt.Points.Add(new OxyPlot.DataPoint((double)i, 16));
                    }
                    i += 8;
                    c++;
                }
            }

            plot.Model.Series.Add(DS);
            plot.Model.Series.Add(US);
            plot.Model.Series.Add(piolt);
        }
    }
}
