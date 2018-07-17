using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using HashLibrary;
using System.Windows.Forms;
using System.Security.Principal;
using PictureSync.Properties;
using static PictureSync.Logic.Config;
using static PictureSync.Logic.TelegramBot;

namespace PictureSync.Logic
{
    internal static class Server
    {
        /// <summary>
        /// Initiate a Tracer, Use Tracker.WriteLine instead of Console.WriteLine to write output to logfile
        /// </summary>
        public static void InitiateTracer()
        {
            Trace.Listeners.Clear();
            var twtl = new TextWriterTraceListener(PathLog)
            {
                Name = "TextLogger",
                TraceOutputOptions = TraceOptions.ThreadId | TraceOptions.DateTime
            };
            var ctl = new ConsoleTraceListener(false) { TraceOutputOptions = TraceOptions.DateTime };
            Trace.Listeners.Add(twtl);
            Trace.Listeners.Add(ctl);
            Trace.AutoFlush = true;
        }

        /// <summary>
        /// Creates a timestampstring for the log
        /// </summary>
        public static string NowLog => "[" + DateTime.Today.ToString("yyyy.MM.dd") + " " + DateTime.Now.ToString("HH:mm:ss.fff", System.Globalization.DateTimeFormatInfo.InvariantInfo) + "]";

        /// <summary>
        /// retuns the log
        /// </summary>
        /// <param name="maxCount">maximum amount of returned lines</param>
        /// <returns> a list of the log containg the last 100 lines</returns>
        public static List<string> GetLogList(int maxCount)
        {
            var final = new List<string>();
            var stream = File.Open(PathLog, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            var reader = new StreamReader(stream);
            var file = reader.ReadToEnd().Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None).ToList();

            file.Reverse();
            if (maxCount < file.Count)
                for (var i = 0; i < maxCount; i++)
                    final.Add(file[i]);
            else
                final = file;

            final.Reverse();
            return final;
        }

        /// <summary>
        /// Creates a msgID string
        /// </summary>
        /// <param name="msgId">unique msgID ot the Message</param>
        public static string MessageIDformat(long msgId) => "<" + msgId.ToString().PadLeft(6, '0') + ">";

        /// <summary>
        /// Creates Log and User file if the do not exist
        /// </summary>
        public static void CreateFiles()
        {
            if (!File.Exists(PathLog))
                using (File.AppendText(PathLog)) { }
            if (!File.Exists(PathUsers))
                using (File.AppendText(PathUsers)) { }
        }

        /// <summary>
        /// Reades the Config file and saves it to Config.config
        /// </summary>
        /// <param name="path">Path of the config file</param>
        /// <returns>true if successful</returns>
        public static bool ReadConfig(string path)
        {
            try
            {
                PathRoot = path;

                //Read from file
                var file = File.ReadAllLines(path + "config.dat");
                var result = (from item in file let pFrom = item.IndexOf("[") + "[".Length let pTo = item.LastIndexOf("]") select item.Substring(pFrom, pTo - pFrom)).ToList();

                Token = result.ElementAt(0);
                Hash = result.ElementAt(1);
                Salt = result.ElementAt(2);
                PathPhotos = result.ElementAt(3);
                MaxLen = Convert.ToInt32(result.ElementAt(4));
                EncodeQ = Convert.ToInt32(result.ElementAt(5));
                Localization = result.ElementAt(6);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Creates a new config file
        /// </summary>
        /// <param name="path">path  for the new config file</param>
        public static void CreateConfig(string path)
        {
            if (IsService())
            {
                OutputResult(NowLog + " " + Resources.Error_No_Config);
                Program.Stop();
            }
            else
            {
                File.Delete(path);
                using (var sw = File.AppendText(path))
                {
                    Console.Write("Token: ");
                    var token = Console.ReadLine();
                    sw.WriteLine("Token = [" + token + "]");

                    Console.Write("Auth_key: ");
                    var authKey = Console.ReadLine();
                    var hasher = new Hasher();
                    var hashedPw = hasher.HashPassword(authKey);
                    sw.WriteLine("Hash = [" + hashedPw.Hash + "]");
                    sw.WriteLine("Salt = [" + hashedPw.Salt + "]");

                    Console.Write("Path for pictures: ");
                    var pathPictures = Console.ReadLine();
                    sw.WriteLine("path_pictures = [" + pathPictures + "]");

                    Console.Write("Maximal lenght of pictures: ");
                    var maxLen = Console.ReadLine();
                    sw.WriteLine("max_picture_lenght = [" + maxLen + "]");

                    Console.Write("Quality of encoding (1-100): ");
                    var encodingQ = Console.ReadLine();
                    sw.WriteLine("encoding_Quality = [" + encodingQ + "]");

                    Console.Write("Localisation (en|de): ");
                    var localisation = Console.ReadLine();
                    sw.WriteLine("localization = [" + localisation + "]");
                }
                Restart();
            }
        }

        /// <summary>
        /// Updates the config file from config class
        /// </summary>
        public static void UpdateConfig()
        {
            File.Delete(PathConfig);
            using (var sw = File.AppendText(PathConfig))
            {
                sw.WriteLine("Token = [" + Token + "]");

                sw.WriteLine("Hash = [" + Hash + "]");
                sw.WriteLine("Salt = [" + Salt + "]");

                sw.WriteLine("path_pictures = [" + PathPhotos + "]");

                sw.WriteLine("max_picture_lenght = [" + MaxLen + "]");

                sw.WriteLine("encoding_Quality = [" + EncodeQ + "]");

                sw.WriteLine("localization = [" + Localization + "]");
            }
            Restart();
        }

        /// <summary>
        /// Restarts the Application
        /// </summary>
        public static void Restart()
        {
            Process.Start(Application.ExecutablePath);
            Environment.Exit(0);
        }

        /// <summary>
        /// Sort users in users.dat alphabetically
        /// </summary>
        public static void SortUsers()
        {
            var file = File.ReadAllLines(PathUsers);
            var result = file.ToList();
            result.Sort();
            File.WriteAllLines(PathUsers, result);
        }

        /// <summary>
        /// Checks if Application runs with admin permissions
        /// </summary>
        /// <returns>true if runs as admin</returns>
        private static bool IsAdministrator()
        {
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        /// <summary>
        /// Restarts the application with admin permissions
        /// </summary>
        public static void RestartAsAdmin()
        {
            if (IsAdministrator()) return;

            // Restart program and run as admin
            var exeName = Process.GetCurrentProcess().MainModule.FileName;
            var startInfo = new ProcessStartInfo(exeName) {Verb = "runas"};
            Process.Start(startInfo);
            Environment.Exit(0);
        }

        /// <summary>
        /// Returns true if Application runs as a service
        /// </summary>
        public static bool IsService()
        {
            return !Environment.UserInteractive;
        }
    }
}
