using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;

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

        public void Create_files()
        {
            if (!File.Exists(Config.config.Path_log))
                using (StreamWriter sw = File.AppendText(Config.config.Path_log));
            if (!File.Exists(Config.config.Path_users))
                using (StreamWriter sw = File.AppendText(Config.config.Path_users));
        }

        public void ReadConfig(string path)
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
                Auth_key = result.ElementAt(1),
                Path_root = path
            };
        }

        public void Create_Config(string path)
        {
            if (!File.Exists(path + "config.dat"))
            {
                using (StreamWriter sw = File.AppendText(path + "config.dat"))
                {
                    Console.Write("Token: ");
                    string token = Console.ReadLine();
                    sw.WriteLine("Token = [" + token + "]");

                    Console.Write("Auth_key: ");
                    string auth_key = Console.ReadLine();
                    sw.WriteLine("Auth_key = [" + auth_key + "]");

                    Console.Clear();
                }
            }
        }
    }
}
