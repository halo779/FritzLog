using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;

namespace FritzboxLog
{
    class Program
    {
        static void Main(string[] args)
        {
            Version newestVer = Config.GetNewestVersion();
            if (newestVer > System.Reflection.Assembly.GetExecutingAssembly().GetName().Version)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\nUpdate Available! - http://hio77.com/fritzlog/download/" + newestVer + "/fritzboxlog.zip\n");
                Console.ResetColor();

            }
            Config config = new Config();
            if(File.Exists(Environment.CurrentDirectory + @"\conf.xml"))
            {
                config = LoadSettings();
            }
            else
            {
                config = LoadDefaults(config);
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("\nFirst Run Configuration\n");
                Console.ResetColor();
                Console.Write("Fritzbox ip/hostname? [" + Config.DEFAULT_BASEURL + "] : ");
                string k = Console.ReadLine();
                config.baseurl = String.IsNullOrWhiteSpace(k) ? Config.DEFAULT_BASEURL : k;
                Console.Write("Fritzbox password? [" + Config.DEFAULT_PASSWORD + "] : ");
                k = Console.ReadLine();
                config.password = String.IsNullOrWhiteSpace(k) ? Config.DEFAULT_PASSWORD : k;

                Console.Write("Log To Text File? (Y/N) [N] : ");
                k = Console.ReadKey().KeyChar.ToString().ToUpperInvariant();
                Console.WriteLine(Environment.NewLine);
                config.logToTextFile = String.IsNullOrWhiteSpace(k) ? Config.DEFAULT_LOGTOTEXTFILE : k == "Y" ? true : false;

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\nDone!\n");
                SaveSettings(config);
                Console.ResetColor();
            }

            System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();
            stopWatch.Start();
            
            
            DataCollector dc = new DataCollector();

            string challange = dc.GetChallenge(config);
            string response = dc.GetResponse(challange, config.password);
            string sid = dc.GetSid(challange, response, config);
            Console.WriteLine("SID: " + sid);
            if (sid == "0000000000000000")
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[Error] Password or authentication process is wrong!");
                Console.ReadKey();
                Environment.Exit(0);
            }
            Console.WriteLine("\n-=- DSL Info -=-\n");
            
            DslInfo info;
            info = dc.GetDslStats(sid, config);

            foreach (var field in typeof(DslInfo).GetFields())
            {
                if (field.Name != "SNRGraph" && field.Name != "BitLoaderGraph")
                    Console.WriteLine("{0}: {1}", field.Name, field.GetValue(info));
            }

            if (config.logToTextFile)
            {
                try
                {
                    if(!Directory.Exists(Environment.CurrentDirectory + @"\logs\"))
                        Directory.CreateDirectory(Environment.CurrentDirectory + @"\logs");

                    using (System.IO.StreamWriter file = new System.IO.StreamWriter(Environment.CurrentDirectory + @"\logs\capture-" + DateTime.Now.ToString("yyyyMMddHHmm") + ".txt",false))
                    {
                        foreach (var field in typeof(DslInfo).GetFields())
                        {
                             file.WriteLine("{0}: {1}", field.Name, field.GetValue(info));
                        }
                        file.Flush();
                        file.Close();
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.Write("\nStats saved to file");
                        Console.ResetColor();
                        Console.Write(" - " + Environment.CurrentDirectory + @"\logs\capture-" + DateTime.Now.ToString("yyyyMMddHHmm") + ".txt");
                    }
                }
                catch (Exception)
                {
                    
                }
                
            }

            stopWatch.Stop();
            TimeSpan ts = stopWatch.Elapsed;
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
            ts.Hours, ts.Minutes, ts.Seconds,
            ts.Milliseconds / 10);
            Console.WriteLine("\n-=- RunTime " + elapsedTime + " -=-\n");
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write("Press any key to close.");
            Console.ReadKey();
            Console.ResetColor();
            Environment.Exit(0);
        }

        public static void SaveSettings(Config conf)
        {
            // Create a new XmlSerializer instance with the type of the test class
            XmlSerializer SerializerObj = new XmlSerializer(typeof(Config));
            try
            {
                // Create a new file stream to write the serialized object to a file  
                TextWriter WriteFileStream = new StreamWriter(Environment.CurrentDirectory + @"\conf.xml");
                SerializerObj.Serialize(WriteFileStream, conf);

                // Cleanup
                WriteFileStream.Close();
            }
            catch (UnauthorizedAccessException)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[Error] Cant write config file!");
                Console.ReadKey();
                Environment.Exit(0);
            } 
        }

        public static Config LoadSettings()
        {
            XmlSerializer SerializerObj = new XmlSerializer(typeof(Config));

            Config LoadedObj = new Config();
            try
            {
                // Create a new file stream for reading the XML file
                FileStream ReadFileStream = new FileStream(Environment.CurrentDirectory + @"\conf.xml", FileMode.Open, FileAccess.Read, FileShare.Read);

                // Load the object saved above by using the Deserialize function
                LoadedObj = (Config)SerializerObj.Deserialize(ReadFileStream);

                // Cleanup
                ReadFileStream.Close();
            }
            catch (Exception)
            {
                //do nothing.
            }

            return LoadedObj;
        }

        public static Config LoadDefaults(Config conf)
        {
            if (String.IsNullOrWhiteSpace(conf.baseurl))
                conf.baseurl = Config.DEFAULT_BASEURL;
            if (conf.ConfVersion == 0)
                conf.ConfVersion = Config.DEFAULT_CONFVERSION;
            if (String.IsNullOrWhiteSpace(conf.password))
                conf.password = Config.DEFAULT_PASSWORD;
            conf.logToTextFile = Config.DEFAULT_LOGTOTEXTFILE;

            return conf;
        }
    }
}
