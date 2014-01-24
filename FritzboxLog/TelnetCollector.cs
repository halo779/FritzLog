using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Globalization;

namespace FritzboxLog
{
    class TelnetCollector
    {


        public void ProcessTelnet(Config conf, DslInfo dsl)
        {
            OutputScriptFile(conf.password, conf.baseurl, System.Windows.Forms.Application.StartupPath + @"\telnetscript.tmp");
            string outputPath;

            if(conf.TelnetDebug)
                outputPath = "telnetoutput-" + DateTime.Now.ToString("yyyyMMddHHmm") + ".txt";
            else
                outputPath = "telnetoutput.tmp";

            ProcessStartInfo pInfo = new ProcessStartInfo();
            pInfo.Arguments = "/r:telnetscript.tmp /o:" + outputPath + " /m";
            pInfo.WorkingDirectory = System.Windows.Forms.Application.StartupPath;
            pInfo.FileName = "TST10.exe";
            Process p = Process.Start(pInfo);
            Console.WriteLine("Querying Telnet");
            byte timeout = 15;
            while (!p.WaitForExit(1000))
            {
                Console.Write(".");
                if (timeout != 0)
                {
                    timeout--;
                }
                else
                {
                    Console.WriteLine("/nTelnet is taking too long to process. Skipping");
                    p.CloseMainWindow();
                    p.Close();
                    break;
                }
            }
            

            Console.WriteLine();
            ProcessOutput(System.Windows.Forms.Application.StartupPath + @"\" + outputPath, conf, dsl);

            //var lines = System.IO.File.ReadAllLines("test.txt");
            //System.IO.File.WriteAllLines("test.txt", lines.Skip(1).ToArray());            

            if(!conf.TelnetDebug)
            {
                File.Delete(System.Windows.Forms.Application.StartupPath + @"\telnetscript.tmp");
                File.Delete(System.Windows.Forms.Application.StartupPath + @"\" + outputPath);
            }

        }

        private void OutputScriptFile(string password, string ip, string outpath)
        {
            using(StreamWriter writer = new StreamWriter(outpath))
            {
                writer.WriteLine(ip);
                writer.WriteLine("WAIT \"Fritz!Box web password:\"");
                writer.WriteLine("SEND \"" + password + "\\m\"");
                writer.WriteLine("WAIT \"#\"");
                writer.WriteLine("SEND \"vdsl\\m\"");
                writer.WriteLine("WAIT \"cpe>\"");
                writer.WriteLine("SEND \"11\\m\"");
                writer.WriteLine("WAIT \"Downstream Retransmission status:\"");
                writer.WriteLine("SEND \"12\\m\"");
                writer.WriteLine("WAIT \"FE_UNAVL_ES\"");
                writer.WriteLine("SEND \"13\\m\"");
                writer.WriteLine("WAIT \"Downstream attainable payload rate\"");
                writer.WriteLine("SEND \"20\\m\"");
                writer.WriteLine("WAIT \"cpe>\"");
            }
        }

        private void ProcessOutput(string TelnetLog, Config conf, DslInfo dsl)
        {
            //string[] Log = File.ReadAllLines(TelnetLog);
            FileInfo fi = new FileInfo(TelnetLog);

            string tmp;

            tmp = fi.HuntForLine(s => s.Contains("Downstream Training Margin:"));
            if (!String.IsNullOrWhiteSpace(tmp))
            {
                try
                {
                    Match m = Regex.Match(tmp, "[0-9]+[.[0-9]+]?");
                    double number = Convert.ToDouble(m.Value, CultureInfo.InvariantCulture);
                    dsl.DownstreamTrainingMargin = number;
                }
                catch
                {

                }
                tmp = null;
            }

            tmp = fi.HuntForLine(s => s.Contains("Tx total power "));
            if (!String.IsNullOrWhiteSpace(tmp))
            {
                try
                {
                    Match m = Regex.Match(tmp, "[0-9]+[.[0-9]+]?");
                    double number = Convert.ToDouble(m.Value, CultureInfo.InvariantCulture);
                    dsl.TxTotalPower = number;
                }
                catch
                {

                }
                tmp = null;
            }

            tmp = fi.HuntForLine(s => s.Contains("FE Tx total power "));
            if (!String.IsNullOrWhiteSpace(tmp))
            {
                try
                {
                    Match m = Regex.Match(tmp, "[0-9]+[.[0-9]+]?");
                    double number = Convert.ToDouble(m.Value, CultureInfo.InvariantCulture);
                    dsl.FeTxTotalPower = number;
                }
                catch
                {

                }
                tmp = null;
            }

            tmp = fi.HuntForLine(s => s.Contains("VDSL Estimated Loop Length"));
            if (!String.IsNullOrWhiteSpace(tmp))
            {
                try
                {
                    Match m = Regex.Match(tmp, "[0-9]+[.[0-9]+]?");
                    long number = Convert.ToInt64(m.Value, CultureInfo.InvariantCulture);
                    dsl.VdslEstimatedLoopLengthFt = number;
                    dsl.VdslEstimatedLoopLengthMeters = Math.Round(Helpers.FeetToMeters(number), 2);
                }
                catch
                {

                }
                tmp = null;
            }

            tmp = fi.HuntForLine(s => s.Contains("G.Hs Estimated Near End Loop Length"));
            if (!String.IsNullOrWhiteSpace(tmp))
            {
                try
                {
                    Match m = Regex.Match(tmp, "[0-9]+[.[0-9]+]?");
                    double number = Convert.ToDouble(m.Value, CultureInfo.InvariantCulture);
                    dsl.GHsEstimatedNearEndLoopLengthFt = number;
                    dsl.GHsEstimatedNearEndLoopLengthMeters = Math.Round(Helpers.FeetToMeters(number), 2);
                }
                catch
                {

                }
                tmp = null;
            }

            tmp = fi.HuntForLine(s => s.Contains("G.Hs Estimated Far End Loop Length"));
            if (!String.IsNullOrWhiteSpace(tmp))
            {
                try
                {
                    Match m = Regex.Match(tmp, "[0-9]+[.[0-9]+]?");
                    double number = Convert.ToDouble(m.Value, CultureInfo.InvariantCulture);
                    dsl.GHsEstimatedFarEndLoopLengthFt = number;
                    dsl.GHsEstimatedFarEndLoopLengthMeters = Math.Round(Helpers.FeetToMeters(number), 2);
                }
                catch
                {

                }
                tmp = null;
            }
        }

        
    }
}
