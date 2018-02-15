using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;

namespace PictureSync.Logic
{
    class Telegram_Bot
    {
        Logic.Server serverlogic = new Logic.Server();

        // global static object because only one bot is needed
        public static Telegram_Bot telebot;

        private TelegramBotClient bot = new TelegramBotClient(Config.config.Token);

        // Check if user is authorized
        public bool CheckAuth(Telegram.Bot.Args.MessageEventArgs e)
        {
            List<string> whitelist = File.ReadAllLines(Config.config.Path_users).ToList();
            return whitelist.Contains(e.Message.Chat.Username);
        }

        //public async Task Download_img(Telegram.Bot.Args.MessageEventArgs e)
        //{
        //    // Create dir for username if not exists
        //    Directory.CreateDirectory(Config.config.Path_photos + e.Message.Chat.Username);

        //    // Get and save file
        //    Telegram.Bot.Types.File img = await bot.GetFileAsync(e.Message.Photo[e.Message.Photo.Count() - 1].FileId);
        //    var image = Bitmap.FromStream(img.FileStream);
        //    image.Save(Config.config.Path_photos + e.Message.Chat.Username + @"\" + TimeOfE(e) + ".png");

        //    await bot.SendTextMessageAsync(e.Message.Chat.Id, "Bild akzeptiert");
        //    Trace.WriteLine(serverlogic.NowLog + " Received photo from " + e.Message.Chat.Username);
        //}

        public async Task Download_document(Telegram.Bot.Args.MessageEventArgs e)
        {
            // Create dir for username if not exists
            Directory.CreateDirectory(Config.config.Path_photos + e.Message.Chat.Username);

            if (e.Message.Document.MimeType == "image/png")
            {
                // Get and save file
                Telegram.Bot.Types.File file = await bot.GetFileAsync(e.Message.Document.FileId);
                Image image = Bitmap.FromStream(file.FileStream);
                image.Save(Config.config.Path_photos + e.Message.Chat.Username + @"\" + Date_taken(image) + ".png");

                await bot.SendTextMessageAsync(e.Message.Chat.Id, "Bild akzeptiert");
                Trace.WriteLine(serverlogic.NowLog + " Received photo from " + e.Message.Chat.Username);
            }
            else if (e.Message.Document.MimeType == "image/jpeg")
            {
                // Get and save file
                Telegram.Bot.Types.File file = await bot.GetFileAsync(e.Message.Document.FileId);
                Image image = Bitmap.FromStream(file.FileStream);
                image.Save(Config.config.Path_photos + e.Message.Chat.Username + @"\" + Date_taken(image) + ".jpg");

                await bot.SendTextMessageAsync(e.Message.Chat.Id, "Bild akzeptiert");
                Trace.WriteLine(serverlogic.NowLog + " Received photo from " + e.Message.Chat.Username);
            }
            else
            {
                await bot.SendTextMessageAsync(e.Message.Chat.Id, "Error. Wrong File Type");
                Trace.WriteLine(serverlogic.NowLog + " Error, wrong file Type from " + e.Message.Chat.Username);
            }
        }

        public void Bot_OnMessage(object sender, Telegram.Bot.Args.MessageEventArgs e)
        {
            if (CheckAuth(e))
            {
                if (e.Message.Type == Telegram.Bot.Types.Enums.MessageType.TextMessage)
                {
                    // Textmessage
                    Trace.WriteLine(serverlogic.NowLog + " Note: " + e.Message.Text);
                    bot.SendTextMessageAsync(e.Message.Chat.Id, "Note accepted.");
                }

                if (e.Message.Type == Telegram.Bot.Types.Enums.MessageType.PhotoMessage)
                {
                    //Disabled because metadata is cut when sending a photo
                    bot.SendTextMessageAsync(e.Message.Chat.Id, "Please send the photo as file.");
                    
                    // Picture
                    //Trace.WriteLine(serverlogic.NowLog + " Photo incoming from " + e.Message.Chat.Username);
                    //Download_img(e);
                }

                if (e.Message.Type == Telegram.Bot.Types.Enums.MessageType.DocumentMessage)
                {
                    Trace.WriteLine(serverlogic.NowLog + " Document incoming from " + e.Message.Chat.Username);
                    Download_document(e);
                }
            }
            else if (e.Message.Type == Telegram.Bot.Types.Enums.MessageType.TextMessage && e.Message.Text == "/auth " + Config.config.Auth_key)
            {
                File.AppendAllText(Config.config.Path_users, e.Message.Chat.Username + Environment.NewLine);
                Trace.WriteLine(serverlogic.NowLog + " " + e.Message.Chat.Username + " has just authenticated a new Device.");
                bot.SendTextMessageAsync(e.Message.Chat.Id, "Erfolgreich Authentifiziert.");
            }
            else
            {
                bot.SendTextMessageAsync(e.Message.Chat.Id, "Authorisation failed.");
            }
        }

        public void Start_bot()
        {
            bot.OnMessage += Bot_OnMessage;
            bot.OnMessageEdited += Bot_OnMessage;
            bot.StartReceiving();
        }

        public void Stop_bot()
        {
            bot.StopReceiving();
            bot.OnMessage -= Bot_OnMessage;
            bot.OnMessageEdited -= Bot_OnMessage;
        }

        public string Date_taken(Image image)
        {
            PropertyItem[] propItems = image.PropertyItems;
            Encoding _Encoding = Encoding.UTF8;
            var DataTakenProperty1 = propItems.Where(a => a.Id.ToString("x") == "9004").FirstOrDefault();
            var DataTakenProperty2 = propItems.Where(a => a.Id.ToString("x") == "9003").FirstOrDefault();
            string originalDateString = _Encoding.GetString(DataTakenProperty1.Value);
            originalDateString = originalDateString.Remove(originalDateString.Length - 1);
            return originalDateString.Replace(":", "-").Replace(" ", "_");
        }
    }
}
