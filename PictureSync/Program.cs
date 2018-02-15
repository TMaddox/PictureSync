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
        static void Main(string[] args)
        {
            var exitEvent = new ManualResetEvent(false);

            Console.CancelKeyPress += (sender, eventArgs) => {
                eventArgs.Cancel = true;
                exitEvent.Set();
            };

            Logic.Server serverlogic = new Logic.Server();

            // Fill config object with configs from file
            serverlogic.ReadConfig();

            // Create global telebot
            Logic.Telegram_Bot.telebot = new Logic.Telegram_Bot();

            // Initiate Logging, if a WriteLine shall be included in the log, use Tracer.Writeline instead of Console.Writeline
            serverlogic.InitiateTracer();

            // Start bot
            Logic.Telegram_Bot.telebot.Start_bot();
            Trace.WriteLine(serverlogic.NowLog + " Bot started");

            // wait for stop, to be implemented
            exitEvent.WaitOne();
            Logic.Telegram_Bot.telebot.Stop_bot();
            Trace.WriteLine(serverlogic.NowLog + " Bot stopped");
        }
    }
}
