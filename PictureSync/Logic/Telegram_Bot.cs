using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using System.Diagnostics;
using System.Drawing;

namespace PictureSync.Logic
{
    public class Telegram_Bot
    {
        Logic.Server serverlogic = new Logic.Server();

        private TelegramBotClient bot;
        public TelegramBotClient BotID
        {
            get { return null; }
            set { bot = value; }
        }

        private string path_users;
        private string path_photos;
        public string Path_users
        {
            get { return null; }
            set { path_users = value; }
        }
        public string Path_photos
        {
            get { return null; }
            set { path_photos = value; }
        }


        public static string TimeOfE(Telegram.Bot.Args.MessageEventArgs e) => e.Message.Date.ToString("yyMMdd_HHmmss");

        /// <summary>
        /// Check if the user is Authorized
        /// </summary>
        /// <param name="e"></param>
        /// <param name="path_users"></param>
        /// <returns>Returns if user is autorized</returns>
        public bool CheckAuth(Telegram.Bot.Args.MessageEventArgs e, string path_users)
        {
            List<string> whitelist = File.ReadAllLines(path_users).ToList();
            return whitelist.Contains(e.Message.Chat.Username);
        }

        public async Task Download_img(Telegram.Bot.Args.MessageEventArgs e, string path_photos)
        {
            // Create dir for username if not exists
            Directory.CreateDirectory(path_photos + e.Message.Chat.Username);

            // Get and save file
            Telegram.Bot.Types.File img = await bot.GetFileAsync(e.Message.Photo[e.Message.Photo.Count() - 1].FileId);
            var image = Bitmap.FromStream(img.FileStream);
            image.Save(path_photos + e.Message.Chat.Username + @"\" + Logic.Telegram_Bot.TimeOfE(e) + ".png"); //Dafuq is da fehler ???

            await bot.SendTextMessageAsync(e.Message.Chat.Id, "Bild akzeptiert");
            Trace.WriteLine(serverlogic.NowLog + " Received photo from " + e.Message.Chat.Username);
        }

        public void Bot_OnMessage(object sender, Telegram.Bot.Args.MessageEventArgs e)
        {
            if (CheckAuth(e, path_users))
            {
                if (e.Message.Type == Telegram.Bot.Types.Enums.MessageType.TextMessage)
                {
                    // Textmessage
                    Trace.WriteLine(serverlogic.NowLog + " Note: " + e.Message.Text);
                    bot.SendTextMessageAsync(e.Message.Chat.Id, "Note accepted");
                }

                if (e.Message.Type == Telegram.Bot.Types.Enums.MessageType.PhotoMessage)
                {
                    // Picture
                    Trace.WriteLine(serverlogic.NowLog + " Photo incoming from " + e.Message.Chat.Username);
                    Download_img(e, path_photos);
                }
            }
            else if (e.Message.Type == Telegram.Bot.Types.Enums.MessageType.TextMessage && e.Message.Text == "/auth " + PictureSync.Program.key)
            {
                File.AppendAllText(path_users, e.Message.Chat.Username + Environment.NewLine);
                Trace.WriteLine(serverlogic.NowLog + " " + e.Message.Chat.Username + " has just authenticated a new Device.");
                bot.SendTextMessageAsync(e.Message.Chat.Id, "Erfolgreich Authentifiziert.");
            }
            else
            {
                bot.SendTextMessageAsync(e.Message.Chat.Id, "Authorisation failed.");
            }
        }

        public void Initiate_eventhandlers()
        {
            bot.OnMessage += Bot_OnMessage;
            bot.OnMessageEdited += Bot_OnMessage;
        }

        public void Start_bot()
        {
            bot.StartReceiving();
        }

        public void Stop_bot()
        {
            bot.StopReceiving();
        }
    }
}
