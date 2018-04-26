using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using HashLibrary;
using System.Windows.Forms;

namespace PictureSync.Logic
{
    class Server
    {
        public void InitiateTracer()
        {
            Trace.Listeners.Clear();
            var twtl = new TextWriterTraceListener(Config.config.Path_log)
            {
                Name = "TextLogger",
                TraceOutputOptions = TraceOptions.ThreadId | TraceOptions.DateTime
            };
            var ctl = new ConsoleTraceListener(false) { TraceOutputOptions = TraceOptions.DateTime };
            Trace.Listeners.Add(twtl);
            Trace.Listeners.Add(ctl);
            Trace.AutoFlush = true;
        }

        public string NowLog => "[" + DateTime.Today.ToString("yyyy.MM.dd") + " " + DateTime.Now.ToString("HH:mm:ss.fff", System.Globalization.DateTimeFormatInfo.InvariantInfo) + "]";
        public string MessageIDformat(int msgID) => "<" + msgID.ToString().PadLeft(6, '0') + ">";

        public void Create_files()
        {
            if (!File.Exists(Config.config.Path_log))
                using (StreamWriter sw = File.AppendText(Config.config.Path_log));
            if (!File.Exists(Config.config.Path_users))
                using (StreamWriter sw = File.AppendText(Config.config.Path_users));
        }

        public void ReadConfig(string path)
        {
            try
            {
                //Read from file
                string[] file = File.ReadAllLines(path + "config.dat");
                List<string> result = new List<string>();

                foreach (string item in file)
                {
                    int pFrom = item.IndexOf("[") + "[".Length;
                    int pTo = item.LastIndexOf("]");

                    result.Add(item.Substring(pFrom, pTo - pFrom));
                }

                Config.config = new Config
                {
                    Token = result.ElementAt(0),
                    Hash = result.ElementAt(1),
                    Salt = result.ElementAt(2),
                    Path_photos = result.ElementAt(3),
                    Path_root = path
                };
            }
            catch (Exception)
            {
                Create_Config(path);
            }
            
        }

        public void Create_Config(string path)
        {
            File.Delete(path + "config.dat");
            using (StreamWriter sw = File.AppendText(path + "config.dat"))
            {
                Console.Write("Token: ");
                string token = Console.ReadLine();
                sw.WriteLine("Token = [" + token + "]");

                Console.Write("Auth_key: ");
                string auth_key = Console.ReadLine();
                var hasher = new Hasher();
                var hashedPW = hasher.HashPassword(auth_key);
                sw.WriteLine("Hash = [" + hashedPW.Hash + "]");
                sw.WriteLine("Salt = [" + hashedPW.Salt + "]");

                Console.Write("Path for pictures: ");
                string path_pictures = Console.ReadLine();
                sw.WriteLine("path_pictures = [" + path_pictures + "]");

                Console.Clear();
            }
            System.Diagnostics.Process.Start(Application.ExecutablePath);
            Environment.Exit(0);
        }
    }
}
