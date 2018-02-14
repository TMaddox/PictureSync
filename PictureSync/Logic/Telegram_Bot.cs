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
        public static string TimeOfE(Telegram.Bot.Args.MessageEventArgs e) => e.Message.Date.ToString("yyMMdd_HHmmss");

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
            image.Save(Config.config.Path_photos + e.Message.Chat.Username + @"\" + Logic.Telegram_Bot.TimeOfE(e) + ".png"); //Dafuq is da fehler ???

            await bot.SendTextMessageAsync(e.Message.Chat.Id, "Bild akzeptiert");
            Trace.WriteLine(serverlogic.NowLog + " Received photo from " + e.Message.Chat.Username);
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
                    // Picture
                    Trace.WriteLine(serverlogic.NowLog + " Photo incoming from " + e.Message.Chat.Username);
                    Download_img(e);
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
