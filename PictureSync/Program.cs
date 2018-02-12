using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using System.Drawing;
using System.IO;

namespace PictureSync
{
    class Program
    {
        private static string path = @"C:\Users\Maddox\Desktop\test";

        static void Main(string[] args)
        {
            bot.OnMessage += Bot_OnMessage;
            bot.OnMessageEdited += Bot_OnMessage;

            bot.StartReceiving();
            Console.ReadLine();
            bot.StopReceiving();
        }

        private static void Bot_OnMessage(object sender, Telegram.Bot.Args.MessageEventArgs e)
        {
            if (e.Message.Type == Telegram.Bot.Types.Enums.MessageType.TextMessage)
            {
                // Textmessage
                Console.WriteLine(GetNowLog() + " Note: " + e.Message.Text);
                bot.SendTextMessageAsync(e.Message.Chat.Id, "Note accepted");
            }

            if (e.Message.Type == Telegram.Bot.Types.Enums.MessageType.PhotoMessage)
            {
                // Picture
                Console.WriteLine(GetNowLog() + " Photo incoming from " + e.Message.Chat.Username);
                Download_img(e);
            }

        }

        static private async Task Download_img(Telegram.Bot.Args.MessageEventArgs e)
        {
            // Create dir for username if not exists
            Directory.CreateDirectory(path + @"\" + e.Message.Chat.Username);

            // Get and save file
            Telegram.Bot.Types.File img = await bot.GetFileAsync(e.Message.Photo[e.Message.Photo.Count() - 1].FileId);
            var image = Bitmap.FromStream(img.FileStream);
            image.Save(path + @"\" + e.Message.Chat.Username + @"\" + TimeOfE(e) + ".png"); //Dafuq is da fehler ???

            await bot.SendTextMessageAsync(e.Message.Chat.Id, "Bild akzeptiert");
            Console.WriteLine(GetNowLog() + " Received photo from " + e.Message.Chat.Username);
        }

        private static string GetNowLog()
        {
            return DateTime.Today.ToString("yyyy.MM.dd") + " " + DateTime.Now.ToString("HH:mm:ss.fff", System.Globalization.DateTimeFormatInfo.InvariantInfo);
        }

        private static string TimeOfE(Telegram.Bot.Args.MessageEventArgs e)
        {
            return e.Message.Date.ToString("yyMMdd_HHmmss");
        }
    }
}
