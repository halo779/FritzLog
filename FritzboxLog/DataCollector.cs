using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Security.Cryptography;
using System.Net;
using System.IO;
using HtmlAgilityPack;
using System.Text.RegularExpressions;

namespace FritzboxLog
{
    class DataCollector
    {
        public string GetChallenge(Config conf)
        {
            try
            {
                XDocument doc = XDocument.Load("http://" + conf.baseurl + "/cgi-bin/webcm?getpage=../html/login_sid.xml");
                XElement info = doc.FirstNode as XElement;

                return info.Element("Challenge").Value;
            }
            catch (WebException)
            {

                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("[Error] Cant connect to fritzbox.");
                Console.ReadKey();

                Environment.Exit(0);

                return null;
            }
            
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

        public string GetResponse(string challenge, string password) 
        { 
            return GetMD5Hash(challenge + "-" + password); 
        }

        public string GetSid(string challenge, string response, Config conf)
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
            return info.Element("SID").Value;
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

        public DslInfo GetDslStats(string sid, Config conf)
        {
            DslInfo stats = new DslInfo();

            XDocument doc = XDocument.Load("http://" + conf.baseurl + "/cgi-bin/webcm?getpage=../html/de/internet/adsldaten.xml" + "&sid=" + sid);
            
            XElement info = doc.Element("DSL").Element("DATA").Element("Bitswap");
            stats.BitswapRx = GetBool(info, "rx");
            stats.BitswapTx = GetBool(info, "tx");

            info = doc.Element("DSL").Element("STATISTIC").Element("CRC_min");
            stats.CRCPerMinExchange = GetDouble(info,"coe");
            stats.CRCPerMinDevice = GetDouble(info,"cpe");

            info = doc.Element("DSL").Element("DATA").Element("ActDataRate");
            stats.CurrentThroughputRx = GetLong(info,"rx");
            stats.CurrentThroughputTx = GetLong(info, "tx");

            info = doc.Element("DSL");
            stats.DslCarrierState = GetByte(info, "carrierState");
            stats.DslMode = GetByte(info, "mode");

            info = doc.Element("DSL").Element("STATISTIC").Element("FEC_min");
            stats.FECPerMinExchange =  GetDouble(info,"coe");
            stats.FECPerMinDevice = GetDouble(info,"cpe");

            info = doc.Element("DSL").Element("DATA").Element("INP");
            stats.INPRx = GetDouble(info,"rx");
            stats.INPTx = GetDouble(info,"tx");

            info = doc.Element("DSL").Element("DATA").Element("Latenz").Element("RX");
            stats.latancyDelayRx = GetByte(info, "delay");
            stats.latancyRx = GetBool(info, "interleave");

            info = doc.Element("DSL").Element("DATA").Element("Latenz").Element("TX");
            stats.latancyDelayTx = GetByte(info, "delay");
            stats.latancyTx = GetBool(info, "interleave");

            info = doc.Element("DSL").Element("DATA").Element("LineLoss");
            stats.LineAttenuationRx = GetByte(info, "rx");
            stats.LineAttenuationTx = GetByte(info, "tx");

            info = doc.Element("DSL").Element("DATA").Element("SignalNoiseDistance");
            stats.SignalNoiseRatioRx = GetByte(info, "rx");
            stats.SignalNoiseRatioTx = GetByte(info, "tx");


            doc = XDocument.Load(@"http://" + conf.baseurl + "/cgi-bin/webcm?getpage=../html/de/internet/overviewdaten.xml" + "&sid=" + sid);

            info = doc.Element("DSLOverview").Element("DSLStatus");
            stats.DslUpTime = GetLongEl(info, "ShowtimeInSec");

            info = doc.Element("DSLOverview").Element("DSLAMInfo").Element("ATCU");
            stats.ATCUId = GetStringEl(info, "ID");
            info = info.Element("VERSION");
            stats.ATCUVendor = GetString(info, "vendor");
            stats.ATCUHybrid = GetString(info, "hybrid");

            info = doc.Element("DSLOverview").Element("DSLAMInfo").Element("DSLAM");
            stats.DSLAMId = GetStringEl(info, "ID");
            stats.DSLAMVersion = GetStringEl(info, "VERSION");
            stats.DSLAMSerial = GetStringEl(info, "SERIAL");

            doc = XDocument.Load(@"http://" + conf.baseurl + "/cgi-bin/webcm?getpage=../html/de/internet/bitsdaten.xml" + "&sid=" + sid);

            info = doc.Element("DSLSpectrum").Element("SNRDIAGRAM");
            stats.SNRGraph = GetStringEl(info, "VALUES");

            info = doc.Element("DSLSpectrum").Element("BITSDIAGRAM");
            stats.BitLoaderGraph = GetStringEl(info, "VALUES");

            info = info.Element("PILOT");
            stats.BitPilotReference = (int)GetLong(info, "tone");




            HttpWebRequest request = WebRequest.Create(@"http://" + conf.baseurl + "/cgi-bin/webcm?getpage=../html/de/menus/menu2.html&var:pagename=adsl&var:menu=internet" + "&sid=" + sid) as HttpWebRequest;
            request.Method = "GET";
            HttpWebResponse wr = request.GetResponse() as HttpWebResponse;
            StreamReader reader = new StreamReader(wr.GetResponseStream());
            string str = reader.ReadToEnd();


            HtmlDocument htdoc = new HtmlDocument();
            htdoc.LoadHtml(str);

            stats.LineProfile = htdoc.GetElementbyId("Profile").ChildNodes[3].InnerText;

            stats.CalcuateDLM();

            request = WebRequest.Create(@"http://" + conf.baseurl + "/cgi-bin/webcm?getpage=../html/de/menus/menu2.html&var:pagename=overview&var:menu=internet" + "&sid=" + sid) as HttpWebRequest;
            request.Method = "GET";
            wr = request.GetResponse() as HttpWebResponse;
            reader = new StreamReader(wr.GetResponseStream());
            str = reader.ReadToEnd();


            htdoc = new HtmlDocument();
            htdoc.LoadHtml(str);

            stats.DSLVersion = htdoc.DocumentNode.SelectSingleNode("//*[@class='foredialog']//table//tr[3]//td[1]//text()[2]").InnerText;

            request = WebRequest.Create(@"http://" + conf.baseurl + "/internet/dsl_line_settings.lua?" + "sid=" + sid) as HttpWebRequest;
            request.Method = "GET";
            wr = request.GetResponse() as HttpWebResponse;
            reader = new StreamReader(wr.GetResponseStream());
            str = reader.ReadToEnd();


            htdoc = new HtmlDocument();
            htdoc.LoadHtml(str);

            string s = htdoc.GetElementbyId("logqueries").SelectSingleNode("pre").SelectSingleNode("code").InnerText.Replace("\r", string.Empty).Replace(@"\", string.Empty);
            string[] s2 = s.Split(Environment.NewLine.ToCharArray());

            foreach (string item in s2)
                if (item.Contains("box:status/localtime"))
                {
                    string q = item.Split('=')[1].Split('"')[1];
                    stats.LocalTime = q;

                    break;
                }
            foreach (string item in s2)
                if (item.Contains("sar:settings/Annex"))
                {
                    string q = item.Split('=')[1].Split('"')[1];
                    stats.Annex = q;

                    break;
                }
            foreach (string item in s2)
                if (item.Contains("sar:settings/DownstreamMarginOffset"))
                {
                    string q = item.Split('=')[1].Split('"')[1];
                    stats.DownstreamMarginOffset = q;

                    break;
                }
            foreach (string item in s2)
                if (item.Contains("sar:settings/DsINP"))
                {
                    string q = item.Split('=')[1].Split('"')[1];
                    stats.DsINP = q;

                    break;
                }

            foreach (string item in s2)
                if (item.Contains("sar:settings/RFI_mode"))
                {
                    string q = item.Split('=')[1].Split('"')[1];
                    stats.RFI_mode = q;

                    break;
                }
            foreach (string item in s2)
                if (item.Contains("sar:settings/UsNoiseBits"))
                {
                    string q = item.Split('=')[1].Split('"')[1];
                    stats.UsNoiseBits = q;

                    break;
                }
            foreach (string item in s2)
                if (item.Contains("sar:status/AdvisedDownstreamMarginOffset"))
                {
                    string q = item.Split('=')[1].Split('"')[1];
                    stats.AdvisedDownstreamMarginOffset = q;

                    break;
                }
            foreach (string item in s2)
                if (item.Contains("sar:status/AdvisedDsINP"))
                {
                    string q = item.Split('=')[1].Split('"')[1];
                    stats.AdvisedDsINP = q;

                    break;
                }
            foreach (string item in s2)
                if (item.Contains("sar:status/AdvisedRFI_mode"))
                {
                    string q = item.Split('=')[1].Split('"')[1];
                    stats.AdvisedRFIMode = q;

                    break;
                }
            foreach (string item in s2)
                if (item.Contains("sar:status/AdvisedUsNoiseBits"))
                {
                    string q = item.Split('=')[1].Split('"')[1];
                    stats.AdvisedUsNoiseBits = q;

                    break;
                }
            foreach (string item in s2)
                if (item.Contains("sar:status/gui_version_number"))
                {
                    string q = item.Split('=')[1].Split('"')[1];
                    stats.FritzGuiVersion = q;

                    break;
                }


            return stats;
        }

        private long GetLong(XElement element, string Attribute)
        {
            long output = 0;

            try
            {
                output = Convert.ToInt64(element.Attribute(Attribute).Value);
            }
            catch (FormatException)
            {
                
                //do nothing
            }

            return output;
        }
        private long GetLongEl(XElement element, string secondElement)
        {
            long output = 0;

            try
            {
                output = Convert.ToInt64(element.Element(secondElement).Value);
            }
            catch (FormatException)
            {

                //do nothing
            }

            return output;
        }

        private byte GetByte(XElement element, string Attribute)
        {
            byte output = 0;

            try
            {
                output = Convert.ToByte(element.Attribute(Attribute).Value);
            }
            catch (FormatException)
            {

                //do nothing
            }

            return output;
        }

        private bool GetBool(XElement element, string Attribute)
        {
            bool output = false;

            try
            {
                output = Convert.ToBoolean(Convert.ToByte(element.Attribute(Attribute).Value));
            }
            catch (FormatException)
            {

                //do nothing
            }

            return output;
        }

        private double GetDouble(XElement element, string Attribute)
        {
            double output = 0;

            try
            {
                output = Convert.ToDouble(element.Attribute(Attribute).Value);
            }
            catch (FormatException)
            {

                //do nothing
            }

            return output;
        }

        private string GetStringEl(XElement element, string secondElement)
        {
            string output = "";

            try
            {
                output = element.Element(secondElement).Value.Replace("\n", string.Empty);
            }
            catch (FormatException)
            {

                //do nothing
            }

            return output;
        }

        private string GetString(XElement element, string Attribute)
        {
            string output = "";

            try
            {
                output = element.Attribute(Attribute).Value.Replace("\n", string.Empty);
            }
            catch (FormatException)
            {

                //do nothing
            }

            return output;
        }

    }
}
