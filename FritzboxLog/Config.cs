using System;

namespace FritzboxLog
{
    /// <summary>
    /// Holds all Config fields, all non serialiezed values are exported to the config file
    /// </summary>
    [Serializable()]
    public class Config
    {
        public string password, baseurl;//dont recase theses. @TODO: update conf with tweaked naming with recased names.
        public int ConfVersion;
        public bool logToTextFile;//dont recase this. @TODO: update conf with tweaked naming with recased names.
        public bool TelnetDebug;

        [NonSerialized()]
        public const string DEFAULT_PASSWORD = "admin", DEFAULT_BASEURL = "fritz.box";

        public const int DEFAULT_CONFVERSION = 1;
        public const bool DEFAULT_LOGTOTEXTFILE = false, LOG_TO_SINGLE_FILE = false;
        public static string AttemptOutPath = Environment.CurrentDirectory;
        public static string AttemptWriteToSingleFile = AttemptOutPath + @"\latest.log";

        public bool FritzOs5 = false, TimeOutOfAlignment = false;

        /// <summary>
        /// Gets the newest version, if connecting to webserver fails, use the current version.
        /// </summary>
        /// <returns>Version Newest Version Number</returns>
        public static Version GetNewestVersion()
        {
            string fritzLogCurrentVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            try
            {
                fritzLogCurrentVersion = new System.Net.WebClient().DownloadString("http://hio77.com/fritzlog/version.txt");
            }
            catch (Exception)
            {
                fritzLogCurrentVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            }

            return Version.Parse(fritzLogCurrentVersion);
        }

    }
}