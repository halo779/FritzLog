﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml.Linq;
using System.Security.Cryptography;
using System.Net;
using System.IO;
using HtmlAgilityPack;
using System.Text.RegularExpressions;
using System.Xml;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace FritzboxLog
{
    class DataCollector
    {
        public void MonitorErrorRatesForMins(string sid, Config conf, ref DslInfo stats, int calcuationTime)
        {
            Console.WriteLine("Monitoring Error Rates for " + calcuationTime + " Mins");
            System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();
            stopWatch.Start();
            Dictionary<string, string> modemStats = getInformationFromFritzLuaQuery(sid, conf, "sar:status/ds_fec", "sar:status/us_fec", "sar:status/ds_crc", "sar:status/us_crc");

            long Lastdscrc = GetLong(modemStats["sar:status/ds_crc"]);
            long Lastuscrc = GetLong(modemStats["sar:status/us_crc"]);
            long Lastdsfec = GetLong(modemStats["sar:status/ds_fec"]);
            long Lastusfec = GetLong(modemStats["sar:status/us_fec"]);

            double[] dsfecCounts = new double[calcuationTime];
            double[] usfecCounts = new double[calcuationTime];
            double[] dscrcCounts = new double[calcuationTime];
            double[] uscrcCounts = new double[calcuationTime];

            System.IO.StreamWriter file = new StreamWriter(Environment.CurrentDirectory + @"\logs\errors\capture-" +
                             DateTime.Now.ToString("yyyyMMddHHmm") + ".csv");
            file.AutoFlush = true;
            file.WriteLine("time,crcdown,crcup,fecdown,fecup");
            Console.WriteLine( DateTime.Now );
            for (int i = 0; i < calcuationTime; i++)
            {
                Thread.Sleep(1000 * 60 * 5);
                modemStats = getInformationFromFritzLuaQuery(sid, conf, "sar:status/ds_fec", "sar:status/us_fec", "sar:status/ds_crc", "sar:status/us_crc");

                dsfecCounts[i] = GetLong(modemStats["sar:status/ds_fec"]) - Lastdsfec;
                usfecCounts[i] = GetLong(modemStats["sar:status/us_fec"]) - Lastusfec;

                dscrcCounts[i] = GetLong(modemStats["sar:status/ds_crc"]) - Lastdscrc;
                uscrcCounts[i] = GetLong(modemStats["sar:status/us_crc"]) - Lastuscrc;

                Lastdscrc = GetLong(modemStats["sar:status/ds_crc"]);
                Lastuscrc = GetLong(modemStats["sar:status/us_crc"]);
                Lastdsfec = GetLong(modemStats["sar:status/ds_fec"]);
                Lastusfec = GetLong(modemStats["sar:status/us_fec"]);
                TimeSpan ts = stopWatch.Elapsed;
                 string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}", ts.Hours, ts.Minutes, ts.Seconds);
                Console.WriteLine("(" + elapsedTime + ") CRC Downstream:" + dscrcCounts[i] + " Upstream: " + uscrcCounts[i] + " FEC Downstream: " + dsfecCounts[i] + " Upstream: " + usfecCounts[i]);
                file.WriteLine(DateTime.Now + "," + dscrcCounts[i] + "," + uscrcCounts[i] + "," + dsfecCounts[i] + "," + usfecCounts[i]);
                i = + 4;

            }

            file.Flush();
            file.Close();

            double dsfecaverage = RoundValue(dsfecCounts.Take(dsfecCounts.Count()).Average());
            double usfeccaverage = RoundValue(usfecCounts.Take(usfecCounts.Count()).Average());

            double dscrcaverage = RoundValue(dscrcCounts.Take(dscrcCounts.Count()).Average());
            double uscrccaverage = RoundValue(uscrcCounts.Take(uscrcCounts.Count()).Average());

            double dsfecStdDev = CalculateStdDev(from values in dsfecCounts select values);
            double usfecStdDev = CalculateStdDev(from values in usfecCounts select values);

            double dscrcStdDev = CalculateStdDev(from values in dscrcCounts select values);
            double uscrcStdDev = CalculateStdDev(from values in uscrcCounts select values);

            double test = CalculateStdDev(from values in dsfecCounts select values);

            Console.WriteLine("");
            Console.WriteLine("Averages - FEC Downstream: " + dsfecaverage + " Upstream: " + usfeccaverage + " CRC Downstream: " + dscrcaverage + " Upstream: " + uscrccaverage);
            Console.WriteLine("Standard Deviation  - FEC Downstream: " + dsfecStdDev + " Upstream: " + usfecStdDev + " CRC Downstream: " + dscrcStdDev + " Upstream: " + uscrcStdDev);
            Console.WriteLine("");



        }

        public void GetDslStats(string sid, Config conf, ref DslInfo stats)
        {
            Dictionary<string, string> modemStats = getInformationFromFritzLuaQuery(sid, conf, "sar:status/ds_attenuation", "sar:status/ds_crc_minute", "sar:status/ds_crc_per15min", "sar:status/ds_delay", "sar:status/ds_es", "sar:status/ds_fec_minute", "sar:status/ds_fec_per15min", "sar:status/ds_margin", "sar:status/ds_path", "sar:status/ds_ses", "sar:status/dsl_ds_rate", "sar:status/dsl_tone_set", "sar:status/dsl_us_rate", "sar:status/exp_ds_inp_act", "sar:status/exp_ds_olr_Bitswap", "sar:status/exp_us_inp_act", "sar:status/exp_us_olr_Bitswap", "sar:status/us_attenuation", "sar:status/us_crc_minute", "sar:status/us_crc_per15min", "sar:status/us_delay", "sar:status/us_es", "sar:status/us_fec_minute", "sar:status/us_fec_per15min", "sar:status/us_margin", "sar:status/us_path", "sar:status/us_ses", "sar:status/vdsl_profile_string", "sar:status/dsl_train_state", "sar:status/dsl_carrier_state");

            stats.BitswapDownstream = GetBool(modemStats["sar:status/ds_path"]);
            stats.BitswapUpstream = GetBool(modemStats["sar:status/us_path"]);

            stats.CRCPerMinDownstream = GetDouble(modemStats["sar:status/ds_crc_minute"]); //Downstream CRC errors
            stats.CRCPerMinUpstream = GetDouble(modemStats["sar:status/us_crc_minute"]); // Upstream CRC errors

            stats.CurrentDslSyncRateDownstream = GetLong(modemStats["sar:status/dsl_ds_rate"]);
            stats.CurrentDslSyncRateUpstream = GetLong(modemStats["sar:status/dsl_us_rate"]);

            stats.DslTrainedModulation = GetByte(modemStats["sar:status/dsl_train_state"]);
            stats.DslTrainingState = GetByte(modemStats["sar:status/dsl_carrier_state"]);


            stats.FECPerMinDownstream = GetDouble(modemStats["sar:status/ds_fec_minute"]);
            stats.FECPerMinUpstream = GetDouble(modemStats["sar:status/us_fec_minute"]);

            stats.INPDownstream = GetDouble(modemStats["sar:status/exp_ds_inp_act"]);
            stats.INPUpstream = GetDouble(modemStats["sar:status/exp_us_inp_act"]);

            stats.LatencyDelayDownstream = GetByte(modemStats["sar:status/ds_delay"]);
            stats.LatencyModeDownstream = GetBool(modemStats["sar:status/ds_path"]); //might wanna change to a char...

            stats.LatencyDelayUpstream = GetByte(modemStats["sar:status/us_delay"]);
            stats.LatencyModeUpstream = GetBool(modemStats["sar:status/us_path"]);

            stats.LineAttenuationDownstream = GetByte(modemStats["sar:status/ds_attenuation"]);
            stats.LineAttenuationUpstream = GetByte(modemStats["sar:status/us_attenuation"]);//~not that it really matters.

            stats.SignalNoiseRatioDownstream = GetByte(modemStats["sar:status/ds_margin"]);
            stats.SignalNoiseRatioUpstream = GetByte(modemStats["sar:status/us_margin"]);

            stats.TextualLineProfile = modemStats["sar:status/vdsl_profile_string"];

            stats.CalcuateDLM();


            modemStats = getInformationFromFritzLuaQuery(sid, conf, "sar:status/ATUC_vendor_ID", "sar:status/ATUC_vendor_version", "sar:status/dslam_VendorID", "sar:status/dslam_VendorID", "sar:status/dslam_VersionNumber", "sar:status/dslam_SerialNumber", "sar:status/DSP_Datapump_ver");

            stats.ATCUId = modemStats["sar:status/ATUC_vendor_ID"];

            stats.ATCUVendor = modemStats["sar:status/ATUC_vendor_version"];

            stats.DSLAMId = modemStats["sar:status/dslam_VendorID"];
            stats.DSLAMVersion = modemStats["sar:status/dslam_VersionNumber"];
            stats.DSLAMSerial = modemStats["sar:status/dslam_SerialNumber"];

            stats.DSLVersion = modemStats["sar:status/DSP_Datapump_ver"];

            modemStats = getInformationFromFritzLuaQuery(sid, conf, "sar:status/ds_snrArrayXML", "sar:status/bitsArrayXML", "sar:status/pilot");

            stats.SNRGraph = modemStats["sar:status/ds_snrArrayXML"];

            stats.BitLoaderGraph = modemStats["sar:status/bitsArrayXML"];

            stats.BitPilotReference = (int)GetLong(modemStats["sar:status/pilot"]);


            modemStats = getInformationFromFritzLuaQuery(sid, conf, "box:status/localtime", "sar:settings/Annex", "sar:settings/DownstreamMarginOffset", "sar:settings/DsINP", "sar:settings/RFI_mode", "sar:settings/UsNoiseBits", "sar:status/AdvisedDownstreamMarginOffset", "sar:status/AdvisedDsINP", "sar:status/AdvisedRFI_mode", "sar:status/AdvisedUsNoiseBits", "sar:status/gui_version_number", "tr069:settings/UpgradesManaged", "logic:status/nspver", "logic:status/uptime_hours", "logic:status/uptime_minutes", "sar:status/modem_ShowtimeSecs");

            stats.DeviceCurrentTime = modemStats["box:status/localtime"];
            stats.Annex = modemStats["sar:settings/Annex"];
            stats.DownstreamMarginOffset = modemStats["sar:settings/DownstreamMarginOffset"];
            stats.DsINP = modemStats["sar:settings/DsINP"];
            stats.RFI_mode = modemStats["sar:settings/RFI_mode"];
            stats.UsNoiseBits = modemStats["sar:settings/UsNoiseBits"];
            stats.AdvisedDownstreamMarginOffset = modemStats["sar:status/AdvisedDownstreamMarginOffset"];
            stats.AdvisedDsINP = modemStats["sar:status/AdvisedDsINP"];
            stats.AdvisedRFIMode = modemStats["sar:status/AdvisedRFI_mode"];
            stats.AdvisedUsNoiseBits = modemStats["sar:status/AdvisedUsNoiseBits"];
            stats.FritzGuiVersion = modemStats["sar:status/gui_version_number"];
            stats.UpgradesManaged = GetBool(modemStats["tr069:settings/UpgradesManaged"]);
            stats.NspVersion = modemStats["logic:status/nspver"];

            DateTime local = DateTime.Now;

            try
            {
                local = Convert.ToDateTime(stats.DeviceCurrentTime);
            }
            catch (Exception)
            {
                //couldnt convert, using system time.
            }
            TimeSpan timediff = DateTime.Now - local;

            TimeSpan ts = new TimeSpan(Convert.ToInt32(GetLong(modemStats["logic:status/uptime_hours"])), Convert.ToInt32(GetLong(modemStats["logic:status/uptime_hours"])), 0);
            DateTime BootTime = local - ts;

            stats.DslUptimeTotalSeconds = ts.TotalSeconds;

            stats.DeviceBootTime = BootTime.ToString("HH:mm:ss dd.MM.yyyy");

            ts = new TimeSpan(0, 0, Convert.ToInt32(modemStats["sar:status/modem_ShowtimeSecs"]));
            DateTime ConnUpTime = local - ts;

            stats.DslConnectedSince = ConnUpTime.ToString("HH:mm:ss dd.MM.yyyy");


            if (timediff.Minutes > 5)
                conf.TimeOutOfAlignment = true;


            modemStats = getInformationFromFritzLuaQuery(sid, conf, "sar:status/ds_los", "sar:status/us_los", "sar:status/ds_sef", "sar:status/us_sef", "sar:status/ds_fec", "sar:status/us_fec", "sar:status/ds_crc", "sar:status/us_crc", "sar:status/ds_es", "sar:status/us_es", "sar:status/ds_ses", "sar:status/us_ses", "sar:status/vdsl_profile", "sar:status/vdsl_transfer_mode");

            stats.LossOfSignalDownstream = GetLong(modemStats["sar:status/ds_los"]);
            stats.LossOfSignalUpstream = GetLong(modemStats["sar:status/us_los"]);

            stats.LossOfFrameDownstream = GetLong(modemStats["sar:status/ds_sef"]);
            stats.LossOfFrameUpstream = GetLong(modemStats["sar:status/us_sef"]);

            stats.FECDownstreamTotal = GetLong(modemStats["sar:status/ds_fec"]);
            stats.FECUpstreamTotal = GetLong(modemStats["sar:status/us_fec"]);

            stats.CRCDownstreamTotal = GetLong(modemStats["sar:status/ds_crc"]);
            stats.CRCUpstreamTotal = GetLong(modemStats["sar:status/us_crc"]);

            stats.ErroredSecondsDownstream = GetLong(modemStats["sar:status/ds_es"]);
            stats.ErroredSecondsUpstream = GetLong(modemStats["sar:status/us_es"]);

            stats.SeverelyErroredSecondsDownstream = GetLong(modemStats["sar:status/ds_ses"]);
            stats.SeverelyErroredSecondsUpstream = GetLong(modemStats["sar:status/us_ses"]);

            stats.VdslLineProfile = (DslInfo.VdslProfiles)GetByte(modemStats["sar:status/vdsl_profile"]);
            stats.VdslTransferMode = (DslInfo.VdslTransferModes)GetByte(modemStats["sar:status/vdsl_transfer_mode"]);

            stats.CalcuatedFECsPerMinDownstream = Math.Round(stats.FECDownstreamTotal / (stats.DslUptimeTotalSeconds / 60), 4);
            stats.CalcuatedFECsPerMinUpstream = Math.Round(stats.FECUpstreamTotal / (stats.DslUptimeTotalSeconds / 60), 4);

            stats.CalcuatedCRCsPerMinDownstream = Math.Round(stats.CRCDownstreamTotal / (stats.DslUptimeTotalSeconds / 60), 4);
            stats.CalcuatedCRCsPerMinUpstream = Math.Round(stats.CRCPerMinUpstream / (stats.DslUptimeTotalSeconds / 60), 4);

        }
        /// <summary>
        /// Calculates the Standard Deviation.
        /// Designed for use with LINQ.
        /// </summary>
        /// <param name="values">Values to be averaged</param>
        /// <returns>Standard Deviation of values</returns>
        private static double CalculateStdDev(IEnumerable<double> values)
        {
            double ret = 0;

            if (values.Count() > 0)
            {
                //Compute the Average
                double avg = values.Average();

                //Perform the Sum of (value-avg)^2
                double sum = values.Sum(d => Math.Pow(d - avg, 2));

                //Put it all together
                ret = Math.Sqrt((sum) / values.Count() - 1);
            }

            return RoundValue(ret);
        }

        private static double RoundValue(double value)
        {
            if (!Double.IsNaN(value) && !Double.IsInfinity(value))
                value = Math.Round(value, 2);

            return value;
        }
        /// <summary>
        /// Converts value to an Byte, while catching errors
        /// </summary>
        /// <param name="value">The value to be Converted.</param>
        /// <returns>The Converted value.</returns>
        private static byte GetByte(string value)
        {
            byte output = 0;

            try
            {
                output = Convert.ToByte(value);
            }
            catch (FormatException)
            {

                Console.WriteLine("[Error] GetByte - Coverting '" + value + "' to a Byte failed.");
            }

            return output;
        }

        /// <summary>
        /// Converts value to an Boolean, while catching errors
        /// </summary>
        /// <param name="value">The value to be Converted.</param>
        /// <returns>The Converted value.</returns>
        private static bool GetBool(string value)
        {
            bool output = false;

            try
            {
                output = Convert.ToBoolean(Convert.ToByte(value));
            }
            catch (FormatException)
            {

                Console.WriteLine("[Error] GetBool - Coverting '" + value + "' to a Boolean failed.");
            }

            return output;
        }

        /// <summary>
        /// Converts value to an Byte, while catching errors
        /// </summary>
        /// <param name="value">The value to be Converted.</param>
        /// <returns>The Converted value.</returns>
        private static long GetLong(string value)
        {
            long output = 0;

            try
            {
                output = Convert.ToInt64(value);
            }
            catch (FormatException)
            {

                Console.WriteLine("[Error] GetLong - Coverting '" + value + "' to a Long failed.");
            }

            return output;
        }

        /// <summary>
        /// Converts value to an Double, while catching errors
        /// </summary>
        /// <param name="value">The value to be Converted.</param>
        /// <returns>The Converted value.</returns>
        private static double GetDouble(string value)
        {
            double output = 0;

            try
            {
                output = Convert.ToDouble(value);
            }
            catch (FormatException)
            {
                Console.WriteLine("[Error] GetDouble - Coverting '" + value + "' to a double failed.");
            }

            return output;
        }

        /// <summary>
        /// Gets the information from fritzbox lua Query.
        /// </summary>
        /// <param name="sid">String current sid for fritz</param>
        /// <param name="conf">Config current configurations for Fritz Log</param>
        /// <param name="query">String Array fields to Query from the Fritz</param>
        /// <returns>Dictionary </returns>
        private Dictionary<string, string> getInformationFromFritzLuaQuery(string sid, Config conf, params string[] query)
        {
            StringBuilder queryStr = new StringBuilder();
            foreach (string qr in query)
            {
                queryStr.Append(qr + "=" + qr + "&");
            }
            HttpWebRequest requestq = WebRequest.Create(@"http://" + conf.baseurl + "/query.lua?" + queryStr.ToString() + "sid=" + sid) as HttpWebRequest;
            requestq.Method = "GET";
            HttpWebResponse wrq = requestq.GetResponse() as HttpWebResponse;
            StreamReader readerq = new StreamReader(wrq.GetResponseStream());
            string strq = readerq.ReadToEnd();


            Dictionary<string, string> jsontest = JsonConvert.DeserializeObject<Dictionary<string, string>>(strq);

            return jsontest;
        }

        private string getXMLValue(XDocument doc, string name, XNamespace nameSpace)
        {
            XElement info = doc.FirstNode as XElement;
            return info.Element(nameSpace + name).Value;
        }

        public string GetMD5Hash(string input)
        {
            MD5 md5Hasher = MD5.Create();
            byte[] data = md5Hasher.ComputeHash(Encoding.Unicode.GetBytes(input));
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                sb.Append(data[i].ToString("x2"));
            }
            return sb.ToString();
        }

        public string GetChallenge(Config conf)
        {
            try
            {
                XDocument doc = XDocument.Load("http://" + conf.baseurl + "/login_sid.lua");
                XElement info = doc.FirstNode as XElement;

                return info.Element("Challenge").Value;
            }
            catch (Exception e)
            {
                if(e.GetType() == typeof(WebException))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write("[Error] Cant connect to fritzbox.");
                }
                else if (e.GetType() == typeof(System.Xml.XmlException))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write("[Error] Cant Read XML.");
                }
                Console.ReadKey();

                Environment.Exit(0);

                return null;
            }
            
        }

        public string GetResponse(string challenge, string password) 
        { 
            return GetMD5Hash(challenge + "-" + password); 
        }

        public string GetSid(string challenge, string response, Config conf)
        {
            string SID = "0000000000000000";
            if (conf.FritzOs5)
            {
                XDocument doc = XDocument.Load("http://" + conf.baseurl + "/login_sid.lua?username=&response=" + challenge + "-" + response);

                XElement info = doc.FirstNode as XElement;
                SID = info.Element("SID").Value;
            }
            else
            {
                HttpWebRequest request = WebRequest.Create("http://" + conf.baseurl + "/cgi-bin/webcm") as HttpWebRequest;
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                string parameter = String.Format(@"login:command/response={0}-{1}&amp;getpage=../html/login_sid.xml", challenge, response);
                byte[] bytes = Encoding.ASCII.GetBytes(parameter);
                request.ContentLength = bytes.Length;
                Stream stream = request.GetRequestStream();
                stream.Write(bytes, 0, bytes.Length);
                stream.Close();
                HttpWebResponse wr = request.GetResponse() as HttpWebResponse;
                StreamReader reader = new StreamReader(wr.GetResponseStream());
                string str = reader.ReadToEnd();
                XDocument doc = XDocument.Parse(str);
                XElement info = doc.FirstNode as XElement;
                SID = info.Element("SID").Value;
            }
            return SID;
        }

        public string GetPage(string url, string sid)
        {
            Uri uri = new Uri(url + "?sid=" + sid);
            HttpWebRequest request = WebRequest.Create(uri) as HttpWebRequest;
            HttpWebResponse response = request.GetResponse() as HttpWebResponse;
            StreamReader reader = new StreamReader(response.GetResponseStream());
            string str = reader.ReadToEnd();
            return str;
        }

        public string GetFirmwareVersion(Config conf)
        {
            try
            {
                XDocument doc = XDocument.Load("http://" + conf.baseurl + "/jason_boxinfo.xml");
                string ver = getXMLValue(doc, "Version", "http://jason.avm.de/updatecheck/");

                return ver;
            }
            catch (Exception e)
            {
                if (e.GetType() == typeof(WebException))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write("[Error] Cant connect to fritzbox.");
                }
                else if (e.GetType() == typeof(System.Xml.XmlException))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write("[Error] Cant Read XML.");
                }
                Console.ReadKey();

                Environment.Exit(0);

                return null; // doesnt get hit, but compiler is silly.. 
            }
        }
    }
}
