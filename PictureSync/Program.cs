using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using System.Drawing;
using System.IO;
using System.Diagnostics;
using System.Threading;

namespace PictureSync
{
    public class Program
    {
        static string basedir;

        static void Main(string[] args)
        {
            Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\PictureSync\");
            basedir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\PictureSync\";

            var exitEvent = new ManualResetEvent(false);
            Console.CancelKeyPress += (sender, eventArgs) => {
                eventArgs.Cancel = true;
                exitEvent.Set();
            };

            Logic.Server serverlogic = new Logic.Server();
            serverlogic.Create_Config(basedir);

            // Fill config object with configs from file
            serverlogic.ReadConfig(basedir);

            // Create files (log, Users)
            serverlogic.Create_files();

            // Create global telebot
            Logic.Telegram_Bot.telebot = new Logic.Telegram_Bot();

            // Initiate Logging, if a WriteLine shall be included in the log, use Tracer.Writeline instead of Console.Writeline
            serverlogic.InitiateTracer();

            // Start bot
            Logic.Telegram_Bot.telebot.Start_bot();
            Trace.WriteLine(serverlogic.NowLog + " Bot started");

            // wait for ctrl-c
            exitEvent.WaitOne();
            Logic.Telegram_Bot.telebot.Stop_bot();
            Trace.WriteLine(serverlogic.NowLog + " Bot stopped");
        }
    }
}
