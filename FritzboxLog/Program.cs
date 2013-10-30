using System;
using System.IO;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Xml.Serialization;

namespace FritzboxLog
{
    class Program
    {
        static void Main(string[] args)
        {
            bool console = true, logToSingleFile = false, autoClose = false;
            string attemptOutPath = Environment.CurrentDirectory;
            string attemptWriteToSingleFile =  attemptOutPath + @"\latest.log";
            if (args.Length > 0)
            {
                for (int i = 0; i < args.Length; i++)
                {
                    switch (args[i])
                    {
                        case "-nc":
                        case "-noconsole":
                            {
                                console = false;
                                break;
                            }
                        case "-latestoutput":
                        case "-l":
                            {
                                logToSingleFile = true;
                                if (args.Length > (i + 1) && args[i] != null && args[i] != "")
                                {
                                    i++;
                                    attemptWriteToSingleFile = attemptOutPath + @"\" + args[i].ToString();
                                }

                                break;
                            }
                        case "-ac":
                        case "-autoclose":
                            {
                                autoClose = true;
                                break;
                            }
                        default:
                            break;
                    }
                }
            }
            if (console)
                ShowConsoleWindow();
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

            Console.WriteLine("Fritzbox Status");
            Console.WriteLine("---------------");
            Console.Write("HTTP: ");
            bool httpStatus = IsServerUp(config.baseurl, 80, 200);
            WritelineColouredBool(httpStatus);
            Console.Write("Telnet: ");
            WritelineColouredBool(IsServerUp(config.baseurl, 23, 200));

            if (!httpStatus)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[Error] Fritz box was unresposive.");
                goto ProgramClose;
            }

            

            System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();
            stopWatch.Start();

            
            
            DataCollector dc = new DataCollector();
            DslInfo info = new DslInfo();

            info.Firmware = dc.GetFirmwareVersion(config);
            int mainOsVer = 0;
            int minorOsVer = 0;
            int.TryParse(info.Firmware.Split('.')[1], out mainOsVer);
            int.TryParse(info.Firmware.Split('.')[2], out minorOsVer);

            if (mainOsVer > 5 || (mainOsVer == 5 && minorOsVer >= 50))
            {
                config.FritzOs5 = true;
                Console.WriteLine("Fritz OS 5.0 Detected.");
            }
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

            dc.GetDslStats(sid, config, ref info);


            /* RRD stuff
            NHawkCommand.Instance.RRDCommandPath = @"C:\rrd\rrdtool.exe";

            if (!File.Exists("test.rrd"))
            {
                RRD FritzStatsNew = new RRD("test.rrd", Helpers.ToUnixTime(DateTime.Now));
                FritzStatsNew.step = 300;
                FritzStatsNew.addDS(new DS("CurrentThroughputRx", DS.TYPE.DERIVE, 600, 0, 250000));
                FritzStatsNew.addDS(new DS("CurrentThroughputTx", DS.TYPE.DERIVE, 600, 0, 250000));
                FritzStatsNew.addRRA(new RRA(RRA.CF.MAX, 0.5, 1, 210240));
                FritzStatsNew.create(true);
                
            }

            RRD FritzStats = RRD.load("test.rrd");

            FritzStats.update(Helpers.ToUnixTime(DateTime.Now), new object[] { info.CurrentThroughputRx, info.CurrentThroughputTx });

            FritzStats.export("dump.xml", false);

            */

            foreach (var field in typeof(DslInfo).GetFields())
            {
                if (field.Name != "SNRGraph" && field.Name != "BitLoaderGraph")
                    Console.WriteLine("{0}: {1}", field.Name, field.GetValue(info));
                /*
                if (field.Name == "SNRGraph")
                {
                    
                    RRD SNRGraph = new RRD("test.rrd", 920800000);
                    SNRGraph.step = 1;
                    SNRGraph.addDS(new DS("SNR", DS.TYPE.COUNTER, 600, DS.U, DS.U));
                    SNRGraph.addRRA(new RRA(RRA.CF.AVERAGE, 0, 1, 60));
                    SNRGraph.create(true);

                    SNRGraph.update(920800001, new object[] { 20 });
                    SNRGraph.update(920800002, new object[] { 20 });
                    SNRGraph.update(920800006, new object[] { 20 });
                    SNRGraph.update(920800010, new object[] { 20 });

                    GRAPH grout = new GRAPH("test.png", "920800000", (920800000 + 4096).ToString());
                    grout.addDEF(new DEF("SNR", SNRGraph, "SNR", RRA.CF.AVERAGE));
                    grout.graph();
                    Console.WriteLine("blah");
                }
                */
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

            if (logToSingleFile)
            {
                try
                {
                    using (System.IO.StreamWriter file = new System.IO.StreamWriter(attemptWriteToSingleFile, false))
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
                        Console.Write(" - " + attemptWriteToSingleFile);
                    }
                }
                catch (Exception)
                {

                }

            }

            //System.Windows.Forms.Form testy = new test();

           // testy.ShowDialog();


            stopWatch.Stop();
            TimeSpan ts = stopWatch.Elapsed;
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
            ts.Hours, ts.Minutes, ts.Seconds,
            ts.Milliseconds / 10);
            Console.WriteLine("\n-=- RunTime " + elapsedTime + " -=-\n");
            if (info.UpgradesManaged)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[Warning] Your Fritz is configured to automatically update its firmware.");
                Console.WriteLine("[Warning] Your Fritz is configured to automatically update its firmware.");
                Console.WriteLine("[Warning] Your Fritz is configured to automatically update its firmware.");
                Console.ResetColor();
            }

            if (config.TimeOutOfAlignment)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[Warning] Your Fritz Timezone May be wrong or out by more than 5 minutes.");
                Console.WriteLine("[Warning] Your Fritz Timezone May be wrong or out by more than 5 minutes.");
                Console.WriteLine("[Warning] Your Fritz Timezone May be wrong or out by more than 5 minutes.");
                Console.ResetColor();
            }
            ProgramClose:

            if (!autoClose)
            {
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.Write("Press any key to close.");
                Console.ReadKey();  
            }

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

        public static void WritelineColouredBool(bool inn)
        {
            if (inn)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write(inn.ToString() + "\n");
                Console.ResetColor();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write(inn.ToString() + "\n");
                Console.ResetColor();
            }

        }

        public static bool IsServerUp(string server, int port, int timeout)
        {
            Socket socket = null;
            try
            {
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, false);


                IAsyncResult result = socket.BeginConnect(server, port, null, null);
                bool success = result.AsyncWaitHandle.WaitOne(timeout, true);

                return socket.Connected;
            }
            catch
            {
                return false;
            }
            finally
            {
                if (null != socket)
                    socket.Close();
            }
        }

        public static void ShowConsoleWindow()
        {
            //Get a pointer to the forground window.  The idea here is that
            //IF the user is starting our application from an existing console
            //shell, that shell will be the uppermost window.  We'll get it
            //and attach to it
            IntPtr ptr = GetForegroundWindow();

            int u;

            GetWindowThreadProcessId(ptr, out u);

            System.Diagnostics.Process process = System.Diagnostics.Process.GetProcessById(u);


            

            if (process.ProcessName == "cmd")    //Is the uppermost window a cmd process?
            {
                AttachConsole(process.Id);
            }
            else
            {
                var handle = GetConsoleWindow();

                if (handle == IntPtr.Zero)
                {
                    AllocConsole();
                }
                else
                {
                    ShowWindow(handle, SW_SHOW);
                }
            }
        }

        public static void HideConsoleWindow()
        {
            var handle = GetConsoleWindow();

            ShowWindow(handle, SW_HIDE);
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool AllocConsole();

        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", SetLastError = true)]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);

        [DllImport("kernel32", SetLastError = true)]
        static extern bool AttachConsole(int dwProcessId);


        const int SW_HIDE = 0;
        const int SW_SHOW = 5;
    }
}
