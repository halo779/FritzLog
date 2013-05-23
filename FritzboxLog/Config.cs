using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FritzboxLog
{
    [Serializable()]
    public class Config
    {
        public string password, baseurl;
        public int ConfVersion;
        public bool logToTextFile;

        
        public static Version GetNewestVersion()
        {
            string v = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            try 
	        {	        
		        System.Net.WebClient wc = new System.Net.WebClient();
                v = wc.DownloadString("http://hio77.com/fritzlog/version.txt");
	        }
	        catch (Exception)
	        {
                v = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
	        }

            return Version.Parse(v);
        }

        [NonSerialized()]
        public const string DEFAULT_PASSWORD = "admin", DEFAULT_BASEURL = "fritz.box";
        public const int DEFAULT_CONFVERSION = 1;
        public const bool DEFAULT_LOGTOTEXTFILE = false;

    }
}
