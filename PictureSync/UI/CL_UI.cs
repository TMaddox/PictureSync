using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PictureSync.UI
{
    class CL_UI
    {
        private Logic.Config config;
        public Logic.Config Config
        {
            get { return null; }
            set { config = value; }
        }

        public static void StartUp(Logic.Telegram_Bot bot)
        {
            Console.WriteLine("*****************************");
            Console.WriteLine("*                           *");
            Console.WriteLine("*     Picture Sync 0.1      *");
            Console.WriteLine("*                           *");
            Console.WriteLine("*****************************");
            Console.WriteLine("");
            Console.WriteLine("1) start bot");
            Console.WriteLine("2) edit config");
            Console.WriteLine("");
            Console.Write("Option: ");
            string answer = Console.ReadLine();

            if(answer == "1")
            {
                bot.Start_bot();
                Console.Clear();
                Console.ReadLine();
            }
            else
            {

            }
        }
    }
}
