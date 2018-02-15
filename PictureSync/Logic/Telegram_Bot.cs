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
    class Telegram_Bot
    {
        Logic.Server serverlogic = new Logic.Server();

        // global static object because only one bot is needed
        public static Telegram_Bot telebot;

        private TelegramBotClient bot = new TelegramBotClient(Config.config.Token);

        // Get the time when message was sent
        public static string TimeOfE(Telegram.Bot.Args.MessageEventArgs e) => e.Message.Date.ToString("yyyy-MM-dd_HH-mm-ss");

        // Check if user is authorized
        public bool CheckAuth(Telegram.Bot.Args.MessageEventArgs e)
        {
            List<string> whitelist = File.ReadAllLines(Config.config.Path_users).ToList();
            return whitelist.Contains(e.Message.Chat.Username);
        }

        public async Task Download_img(Telegram.Bot.Args.MessageEventArgs e)
        {
            // Create dir for username if not exists
            Directory.CreateDirectory(Config.config.Path_photos + e.Message.Chat.Username);

            // Get and save file
            Telegram.Bot.Types.File img = await bot.GetFileAsync(e.Message.Photo[e.Message.Photo.Count() - 1].FileId);
            var image = Bitmap.FromStream(img.FileStream);
            image.Save(Config.config.Path_photos + e.Message.Chat.Username + @"\" + TimeOfE(e) + ".png");

            await bot.SendTextMessageAsync(e.Message.Chat.Id, "Bild akzeptiert");
            Trace.WriteLine(serverlogic.NowLog + " Received photo from " + e.Message.Chat.Username);
        }

        public async Task Download_document(Telegram.Bot.Args.MessageEventArgs e)
        {
            // Create dir for username if not exists
            Directory.CreateDirectory(Config.config.Path_photos + e.Message.Chat.Username);

            if (e.Message.Document.MimeType == "image/png")
            {
                // Get and save file
                Telegram.Bot.Types.File file = await bot.GetFileAsync(e.Message.Document.FileId);
                var image = Bitmap.FromStream(file.FileStream);
                image.Save(Config.config.Path_photos + e.Message.Chat.Username + @"\" + TimeOfE(e) + ".png");

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
                    bot.SendTextMessageAsync(e.Message.Chat.Id, "Note accepted");
                }

                if (e.Message.Type == Telegram.Bot.Types.Enums.MessageType.PhotoMessage)
                {
                    //Disabled because metadata is cut when sending a photo
                    bot.SendTextMessageAsync(e.Message.Chat.Id, "Bitte senden sie da Foto als Datei.");
                    
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
    }
}
