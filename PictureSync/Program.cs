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
        Logic.Server Serverlogic = new Logic.Server();

        static void Main(string[] args)
        {
            Logic.Config config = new Logic.Config
            {
                Path_root = @"C:\Users\Maddox\Desktop\test\",
                Auth_key = "123456"
            };

            Logic.Telegram_Bot bot = new Logic.Telegram_Bot
            {
                Bot = new TelegramBotClient(config.Token),
                Config = config
            };

            Logic.Server serverlogic = new Logic.Server
            {
                Config = config
            };

            // Initiate Logging, if a WriteLine shall be included in the log, use Tracer.Writeline instead of Console.Writeline
            serverlogic.InitiateTracer();

            // Start bot
            UI.CL_UI.StartUp(bot);
        }
    }
}
