using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PictureSync.UI
{
    class CL_UI
    {
        public static void StartUp()
        {
            Console.WriteLine("*****************************");
            Console.WriteLine("*                           *");
            Console.WriteLine("*     Picture Sync 0.1      *");
            Console.WriteLine("*                           *");
            Console.WriteLine("*****************************");
            Console.WriteLine("");
            Console.WriteLine("1) start bot");
            Console.WriteLine("2) show config");
            Console.WriteLine("");
            Console.Write("Option: ");
            string answer = Console.ReadLine();

            if(answer == "1")
            {
                Logic.Telegram_Bot.telebot.Start_bot();
                Console.Clear();
                Console.ReadLine();
            }
            else if (answer == "2")
            {

            }
            else
            {

            }
        }
    }
}
