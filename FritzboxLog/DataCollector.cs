﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        private string getXMLValue(XDocument doc, string name, XNamespace nameSpace)
        {
            XElement info = doc.FirstNode as XElement;
            return info.Element(nameSpace + name).Value;
        }

        public string GetFirmwareVersion(Config conf)
        {
            try
            {
                XDocument doc = XDocument.Load("http://" + conf.baseurl + "/jason_boxinfo.xml");
                string ver = getXMLValue(doc, "Version", "http://jason.avm.de/updatecheck/");

                return ver.ToString();
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

        public void GetDslStats(string sid, Config conf, ref DslInfo stats)
        {
            Dictionary<string, string> modemStats = getJsonQueryDict(sid, conf, "sar:status/ds_attenuation", "sar:status/ds_crc_minute", "sar:status/ds_crc_per15min", "sar:status/ds_delay", "sar:status/ds_es", "sar:status/ds_fec_minute", "sar:status/ds_fec_per15min", "sar:status/ds_margin", "sar:status/ds_path", "sar:status/ds_ses", "sar:status/dsl_ds_rate", "sar:status/dsl_tone_set", "sar:status/dsl_us_rate", "sar:status/exp_ds_inp_act", "sar:status/exp_ds_olr_Bitswap", "sar:status/exp_us_inp_act", "sar:status/exp_us_olr_Bitswap", "sar:status/us_attenuation", "sar:status/us_crc_minute", "sar:status/us_crc_per15min", "sar:status/us_delay", "sar:status/us_es", "sar:status/us_fec_minute", "sar:status/us_fec_per15min", "sar:status/us_margin", "sar:status/us_path", "sar:status/us_ses", "sar:status/vdsl_profile_string", "sar:status/dsl_train_state");


            XDocument doc = new XDocument();

            stats.BitswapRx = GetBool(modemStats["sar:status/ds_path"]);
            stats.BitswapTx = GetBool(modemStats["sar:status/us_path"]);

            stats.CRCPerMinExchange = GetDouble(modemStats["sar:status/ds_crc_minute"]);
            stats.CRCPerMinDevice = GetDouble(modemStats["sar:status/us_crc_minute"]);

            stats.CurrentThroughputRx = GetLong(modemStats["sar:status/dsl_ds_rate"]);
            stats.CurrentThroughputTx = GetLong(modemStats["sar:status/dsl_us_rate"]);

            stats.DslMode = GetByte(modemStats["sar:status/dsl_train_state"]);

            stats.FECPerMinExchange = GetDouble(modemStats["sar:status/ds_fec_minute"]);
            stats.FECPerMinDevice = GetDouble(modemStats["sar:status/us_fec_minute"]);

            stats.INPRx = GetDouble(modemStats["sar:status/exp_ds_inp_act"]);
            stats.INPTx = GetDouble(modemStats["sar:status/exp_us_inp_act"]);

            stats.LatencyDelayRx = GetByte(modemStats["sar:status/ds_delay"]);
            stats.LatencyRx = GetBool(modemStats["sar:status/ds_path"]);

            stats.LatencyDelayTx = GetByte(modemStats["sar:status/us_delay"]);
            stats.LatencyTx = GetBool(modemStats["sar:status/us_path"]);

            stats.LineAttenuationRx = GetByte(modemStats["sar:status/ds_attenuation"]);
            stats.LineAttenuationTx = GetByte(modemStats["sar:status/us_attenuation"]);

            stats.SignalNoiseRatioRx = GetByte(modemStats["sar:status/ds_margin"]);
            stats.SignalNoiseRatioTx = GetByte(modemStats["sar:status/us_margin"]);

            stats.LineProfile = modemStats["sar:status/vdsl_profile_string"];

            stats.CalcuateDLM();


            modemStats = getJsonQueryDict(sid, conf, "sar:status/modem_ShowtimeSecs", "sar:status/ATUC_vendor_ID", "sar:status/ATUC_vendor_version", "sar:status/dslam_VendorID", "sar:status/dslam_VendorID", "sar:status/dslam_VersionNumber", "sar:status/dslam_SerialNumber", "sar:status/DSP_Datapump_ver");


            stats.DslUpTime = GetLong(modemStats["sar:status/modem_ShowtimeSecs"]);

            stats.ATCUId = modemStats["sar:status/ATUC_vendor_ID"];

            stats.ATCUVendor = modemStats["sar:status/ATUC_vendor_version"];

            stats.DSLAMId = modemStats["sar:status/dslam_VendorID"];
            stats.DSLAMVersion = modemStats["sar:status/dslam_VersionNumber"];
            stats.DSLAMSerial = modemStats["sar:status/dslam_SerialNumber"];

            stats.DSLVersion = modemStats["sar:status/DSP_Datapump_ver"];

            modemStats = getJsonQueryDict(sid, conf, "sar:status/ds_snrArrayXML", "sar:status/bitsArrayXML", "sar:status/pilot");

            stats.SNRGraph = modemStats["sar:status/ds_snrArrayXML"];

            stats.BitLoaderGraph = modemStats["sar:status/bitsArrayXML"];

            stats.BitPilotReference = (int)GetLong(modemStats["sar:status/pilot"]);


            modemStats = getJsonQueryDict(sid, conf, "box:status/localtime", "sar:settings/Annex", "sar:settings/DownstreamMarginOffset", "sar:settings/DsINP", "sar:settings/RFI_mode", "sar:settings/UsNoiseBits", "sar:status/AdvisedDownstreamMarginOffset", "sar:status/AdvisedDsINP", "sar:status/AdvisedRFI_mode", "sar:status/AdvisedUsNoiseBits", "sar:status/gui_version_number");

            stats.LocalTime = modemStats["box:status/localtime"];
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
        }

        private long GetLong(string value)
        {
            long output = 0;

            try
            {
                output = Convert.ToInt64(value);
            }
            catch (FormatException)
            {

                //do nothing
            }

            return output;
        }

        private byte GetByte(string value)
        {
            byte output = 0;

            try
            {
                output = Convert.ToByte(value);
            }
            catch (FormatException)
            {

                //do nothing
            }

            return output;
        }

        private bool GetBool(string value)
        {
            bool output = false;

            try
            {
                output = Convert.ToBoolean(Convert.ToByte(value));
            }
            catch (FormatException)
            {

                //do nothing
            }

            return output;
        }

        private double GetDouble(string value)
        {
            double output = 0;

            try
            {
                output = Convert.ToDouble(value);
            }
            catch (FormatException)
            {

                //do nothing
            }

            return output;
        }

        private Dictionary<string, string> getJsonQueryDict(string sid, Config conf, params string[] query)
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

    }
}
