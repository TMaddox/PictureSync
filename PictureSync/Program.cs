using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using System.Drawing;
using System.IO;
using System.Diagnostics;

namespace PictureSync
{
    public class Program
    {
        static void Main(string[] args)
        {
            Logic.Server serverlogic = new Logic.Server();

            serverlogic.ReadConfig();

            Logic.Telegram_Bot bot = new Logic.Telegram_Bot();

            // Initiate Logging, if a WriteLine shall be included in the log, use Tracer.Writeline instead of Console.Writeline
            serverlogic.InitiateTracer();

            // Start bot
            UI.CL_UI.StartUp(bot);
        }
    }
}
