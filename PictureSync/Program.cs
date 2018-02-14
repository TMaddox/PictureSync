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
        private static string path_root = @"C:\Users\Maddox\Desktop\test\";
        private static string path_photos = path_root + @"pic\";
        private static string path_logs = path_root + @"log.txt";
        private static string path_users = path_root + @"users.dat";
        public static string key = "123456";

        Logic.Server Serverlogic = new Logic.Server();

        static void Main(string[] args)
        {
            Logic.Telegram_Bot bot = new Logic.Telegram_Bot();
            bot.BotID = new TelegramBotClient(token);
            bot.Path_users = path_users;
            bot.Path_photos = path_photos;

            Logic.Server serverlogic = new Logic.Server();

            Console.WriteLine("*****************************");
            Console.WriteLine("*                           *");
            Console.WriteLine("*     Picture Sync 0.1      *");
            Console.WriteLine("*                           *");
            Console.WriteLine("*****************************");

            // Logging
            serverlogic.InitiateTracer(path_logs);

            // Initiate eventhandlers
            bot.Initiate_eventhandlers();

            // Start bot
            bot.Start_bot();
        }
    }
}
