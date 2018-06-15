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
using System.Text.RegularExpressions;
using HashLibrary;
using System.Windows.Forms;

namespace PictureSync.Logic
{
    class Telegram_Bot
    {
        Logic.Server serverlogic = new Logic.Server();

        // global static object because only one bot is needed
        public static Telegram_Bot Telebot;

        private readonly TelegramBotClient bot = new TelegramBotClient(Config.config.Token);

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

        public async Task Download_document(Telegram.Bot.Args.MessageEventArgs e, int messageId)
        {
            // Create dir for username if not exists
            Directory.CreateDirectory(Config.config.Path_photos + e.Message.Chat.Username);

            if (e.Message.Document.MimeType == "image/png" || e.Message.Document.MimeType == "image/jpeg")
            {
                // Get and save file
                var file = await bot.GetFileAsync(e.Message.Document.FileId);
                var image = Image.FromStream(file.FileStream);
                var filename = Save_image(e, image, messageId);

                if(Userlist.HasCompression(e.Message.Chat.Username))
                    await bot.SendTextMessageAsync(e.Message.Chat.Id, "Bild akzeptiert.");
                else
                    await bot.SendTextMessageAsync(e.Message.Chat.Id, "Unkomprimiertes Bild akzeptiert.");
                Trace.WriteLine(serverlogic.NowLog + " " + serverlogic.MessageIDformat(messageId) + " Saved photo from " + e.Message.Chat.Username + " as " + filename);

                Userlist.AddPictureAmount(e.Message.Chat.Username);
                if (Userlist.SetCompression(e.Message.Chat.Username, true))
                {
                    Trace.WriteLine(serverlogic.NowLog + " " + e.Message.Chat.Username + " autoenable compression");
                    await bot.SendTextMessageAsync(e.Message.Chat.Id, "Komprimieren ist wieder aktiviert");
                }
            }
            else
            {
                await bot.SendTextMessageAsync(e.Message.Chat.Id, "Error. Wrong File Type");
                Trace.WriteLine(serverlogic.NowLog + " " + serverlogic.MessageIDformat(messageId) + " Error, wrong file Type from " + e.Message.Chat.Username);
            }
        }

        private string Save_image(Telegram.Bot.Args.MessageEventArgs e, Image image, int messageId)
        {
            Bitmap finalImage;
            var hasCompression = Userlist.HasCompression(e.Message.Chat.Username);
            var res = (double)image.Width / image.Height;
            int width = image.Width,
                height = image.Height;
            var dateTaken = Date_taken(image, e, messageId);

            if(res <= 1)
            {
                //Hochformat
                height = hasCompression ? Config.config.Max_len : height;
                width = hasCompression ? Convert.ToInt16(height * res): width;
            }
            else
            {
                //Querformat
                width = hasCompression ? Config.config.Max_len : width;
                height = hasCompression ? Convert.ToInt16(width / res): height;
            }

            if (image.Width > width && image.Height > height)
                finalImage = serverlogic.ResizeImg(image, width, height);
            else
                finalImage = serverlogic.ResizeImg(image, image.Width, image.Height);

            var jpgEncoder = serverlogic.GetEncoder(ImageFormat.Jpeg);
            var myEncoder = System.Drawing.Imaging.Encoder.Quality;
            var encoder = new EncoderParameters(1);
            var encoderParameter = new EncoderParameter(myEncoder, hasCompression ? Config.config.EncodeQ : 93L);
            encoder.Param[0] = encoderParameter;

            if (!File.Exists(Config.config.Path_photos + e.Message.Chat.Username + @"\" + dateTaken + ".jpg"))
            {
                finalImage.Save(Config.config.Path_photos + e.Message.Chat.Username + @"\" + dateTaken + ".jpg", jpgEncoder, encoder);
                return dateTaken + ".jpg";
            }
            else if (!File.Exists(Config.config.Path_photos + e.Message.Chat.Username + @"\" + dateTaken + " (2)" + ".jpg"))
            {
                finalImage.Save(Config.config.Path_photos + e.Message.Chat.Username + @"\" + dateTaken + " (2)" + ".jpg", jpgEncoder, encoder);
                return dateTaken + " (2)" + ".jpg";
            }
            else
            {
                string path = Config.config.Path_photos + e.Message.Chat.Username + @"\";
                DirectoryInfo dir = new DirectoryInfo(path);
                int number = 0;
                int test;

                foreach (var file in dir.GetFiles("*" + ".jpg"))
                {
                    try
                    {
                        int pFrom = file.ToString().IndexOf("(", StringComparison.Ordinal) + "(".Length;
                        int pTo = file.ToString().LastIndexOf(")");

                        test = Convert.ToInt16(file.ToString().Substring(pFrom, pTo - pFrom));
                    }
                    catch (Exception)
                    {
                        test = 0;
                    }
                    
                    if (number < test)
                        number = test;
                }
                number++;
                finalImage.Save(Config.config.Path_photos + e.Message.Chat.Username + @"\" + dateTaken + " (" + number.ToString() + ")" + ".jpg", jpgEncoder, encoder);
                return dateTaken + " (" + number.ToString() + ")" + ".jpg";
            }
        }

        public void Bot_OnMessage(object sender, Telegram.Bot.Args.MessageEventArgs e)
        {
            if (Userlist.HasAuth(e.Message.Chat.Username))
            {
                int messageID = Config.config.Msg_Increment;
                Config.config.Msg_Increment++;

                if (e.Message.Type == Telegram.Bot.Types.Enums.MessageType.TextMessage)
                {
                    // Textmessage
                    switch (e.Message.Text)
                    {
                        case ".":
                            Userlist.SetCompression(e.Message.Chat.Username, false);
                            Trace.WriteLine(serverlogic.NowLog + " " + e.Message.Chat.Username + " disabled compression");
                            bot.SendTextMessageAsync(e.Message.Chat.Id, "Komprimieren ist für das nächste Bild deaktiviert");
                            break;
                        case ",":
                            Userlist.SetCompression(e.Message.Chat.Username, true);
                            Trace.WriteLine(serverlogic.NowLog + " " + e.Message.Chat.Username + " enabled compression");
                            bot.SendTextMessageAsync(e.Message.Chat.Id, "Komprimieren ist aktiviert");
                            break;
                        default:
                            Trace.WriteLine(serverlogic.NowLog + " Note: " + e.Message.Text);
                            bot.SendTextMessageAsync(e.Message.Chat.Id, "Dieser Befehl hat keine Bedeutung.");
                            break;
                    }
                }

                if (e.Message.Type == Telegram.Bot.Types.Enums.MessageType.PhotoMessage)
                {
                    //Disabled because metadata is cut when sending a photo
                    bot.SendTextMessageAsync(e.Message.Chat.Id, "Bild abgelehnt, bitte als Datei senden.");
                    Trace.WriteLine(serverlogic.NowLog + " " + serverlogic.MessageIDformat(messageID) + e.Message.Chat.Username + " tried to send a picture, but sent not as file.");

                    // Picture
                    //Trace.WriteLine(serverlogic.NowLog + " Photo incoming from " + e.Message.Chat.Username);
                    //Download_img(e);
                }

                if (e.Message.Type == Telegram.Bot.Types.Enums.MessageType.DocumentMessage)
                {
                    Trace.WriteLine(serverlogic.NowLog + " " + serverlogic.MessageIDformat(messageID) + " Document incoming from " + e.Message.Chat.Username);
                    Download_document(e, messageID);
                }
            }
            else if (e.Message.Type == Telegram.Bot.Types.Enums.MessageType.TextMessage && e.Message.Chat.Username != null && e.Message.Text.StartsWith("/auth "))
            {
                var hasher = new Hasher();
                if (hasher.Check(e.Message.Text.Remove(0, 6), new HashedPassword(Config.config.Hash, Config.config.Salt)))
                {
                    File.AppendAllText(Config.config.Path_users, e.Message.Chat.Username + Environment.NewLine);
                    Trace.WriteLine(serverlogic.NowLog + " " + e.Message.Chat.Username + " has just authenticated a new Device.");
                    bot.SendTextMessageAsync(e.Message.Chat.Id, "Erfolgreich Authentifiziert.");
                }
                else
                {
                    Trace.WriteLine(serverlogic.NowLog + " " + e.Message.Chat.Username + " tried to authenticate, but entered wrong password.");
                    bot.SendTextMessageAsync(e.Message.Chat.Id, "Authentifizierung fehlgeschlagen. Falsches Passwort.");
                }
            }
            else
            {
                bot.SendTextMessageAsync(e.Message.Chat.Id, "Authentifizierung fehlgeschlagen. Stellen Sie sicher dass ein Benutzername gesetzt ist.");
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

        public string Date_taken(Image image, Telegram.Bot.Args.MessageEventArgs e, int messageID)
        {
            try
            {
                PropertyItem[] propItems = image.PropertyItems;
                Encoding _Encoding = Encoding.UTF8;
                var DataTakenProperty1 = propItems.Where(a => a.Id.ToString("x") == "9004").FirstOrDefault();
                var DataTakenProperty2 = propItems.Where(a => a.Id.ToString("x") == "9003").FirstOrDefault();
                string originalDateString = _Encoding.GetString(DataTakenProperty1.Value);
                originalDateString = originalDateString.Remove(originalDateString.Length - 1);
                return originalDateString.Replace(":", "-").Replace(" ", "_");
            }
            catch
            {
                Trace.WriteLine(serverlogic.NowLog + " " + serverlogic.MessageIDformat(messageID) + " Photo has no capture time (using servertime instead) from " + e.Message.Chat.Username);
                return DateTime.Today.ToString("yyyy-MM-dd") + "_" + DateTime.Now.ToString("HH-mm-ss", System.Globalization.DateTimeFormatInfo.InvariantInfo) + "_noCaptureTime";
            }
        }
    }
}
