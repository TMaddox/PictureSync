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

        public void ReadConfig()
        {
            //Read from file
            string[] file = File.ReadAllLines(@"C:\Users\Maddox\Desktop\test\config.dat");
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
                Path_root = result.ElementAt(1),
                Auth_key = result.ElementAt(2)
            };
        }
    }
}
