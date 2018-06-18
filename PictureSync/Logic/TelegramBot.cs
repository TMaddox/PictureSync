using HashLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;

namespace PictureSync.Logic
{
    internal class TelegramBot
    {
        /// <summary>
        /// static because only one bot is required
        /// </summary>
        public static TelegramBot Telebot;

        /// <summary>
        /// Create a TelegramBotClient with telegram token
        /// </summary>
        private readonly TelegramBotClient _bot = new TelegramBotClient(Config.config.Token);

        /// <summary>
        /// Download a document from telegram Server
        /// </summary>
        /// <param name="e"></param>
        /// <param name="messageId"></param>
        /// <returns></returns>
        private async Task Download_document(MessageEventArgs e, int messageId)
        {
            // Create dir for username if not exists
            Directory.CreateDirectory(Config.config.PathPhotos + e.Message.Chat.Username);

            if (e.Message.Document.MimeType == "image/png" || e.Message.Document.MimeType == "image/jpeg")
            {
                // Get and save file
                var file = await _bot.GetFileAsync(e.Message.Document.FileId);
                var image = Image.FromStream(file.FileStream);
                var filename = Save_image(e, image, messageId);

                // Log file saved
                if(Userlist.HasCompression(e.Message.Chat.Username))
                    await _bot.SendTextMessageAsync(e.Message.Chat.Id, "Bild akzeptiert.");
                else
                    await _bot.SendTextMessageAsync(e.Message.Chat.Id, "Unkomprimiertes Bild akzeptiert.");
                Trace.WriteLine(Server.NowLog + " " + Server.MessageIDformat(messageId) + " Saved photo from " +
                                e.Message.Chat.Username + " as " + filename);

                // Add +1 to picture counter, auto enable compression
                Userlist.AddPictureAmount(e.Message.Chat.Username);
                if (Userlist.SetCompression(e.Message.Chat.Username, true))
                {
                    Trace.WriteLine(Server.NowLog + " " + e.Message.Chat.Username + " autoenable compression");
                    await _bot.SendTextMessageAsync(e.Message.Chat.Id, "Komprimieren ist wieder aktiviert");
                }
            }
            else
            {
                await _bot.SendTextMessageAsync(e.Message.Chat.Id, "Error. Wrong File Type");
                Trace.WriteLine(Server.NowLog + " " + Server.MessageIDformat(messageId) +
                                " Error, wrong file Type from " + e.Message.Chat.Username);
            }
        }

        /// <summary>
        /// Compress and save a image
        /// </summary>
        /// <param name="e"></param>
        /// <param name="image"></param>
        /// <param name="messageId"></param>
        /// <returns></returns>
        private static string Save_image(MessageEventArgs e, Image image, int messageId)
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
                height = hasCompression ? Config.config.MaxLen : height;
                width = hasCompression ? Convert.ToInt16(height * res): width;
            }
            else
            {
                //Querformat
                width = hasCompression ? Config.config.MaxLen : width;
                height = hasCompression ? Convert.ToInt16(width / res): height;
            }

            if (image.Width > width && image.Height > height)
                finalImage = Server.ResizeImg(image, width, height);
            else
                finalImage = Server.ResizeImg(image, image.Width, image.Height);

            var jpgEncoder = Server.GetEncoder(ImageFormat.Jpeg);
            var myEncoder = System.Drawing.Imaging.Encoder.Quality;
            var encoder = new EncoderParameters(1);
            var encoderParameter = new EncoderParameter(myEncoder, hasCompression ? Config.config.EncodeQ : 93L);
            encoder.Param[0] = encoderParameter;

            if (!File.Exists(Config.config.PathPhotos + e.Message.Chat.Username + @"\" + dateTaken + ".jpg"))
            {
                finalImage.Save(Config.config.PathPhotos + e.Message.Chat.Username + @"\" + dateTaken + ".jpg", jpgEncoder, encoder);
                return dateTaken + ".jpg";
            }
            else if (!File.Exists(Config.config.PathPhotos + e.Message.Chat.Username + @"\" + dateTaken + " (2)" + ".jpg"))
            {
                finalImage.Save(Config.config.PathPhotos + e.Message.Chat.Username + @"\" + dateTaken + " (2)" + ".jpg", jpgEncoder, encoder);
                return dateTaken + " (2)" + ".jpg";
            }
            else
            {
                var path = Config.config.PathPhotos + e.Message.Chat.Username + @"\";
                var dir = new DirectoryInfo(path);
                var number = 0;

                foreach (var file in dir.GetFiles("*" + ".jpg"))
                {
                    int test;
                    try
                    {
                        var pFrom = file.ToString().IndexOf("(") + "(".Length;
                        var pTo = file.ToString().LastIndexOf(")");

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
                finalImage.Save(Config.config.PathPhotos + e.Message.Chat.Username + @"\" + dateTaken + " (" + number.ToString() + ")" + ".jpg", jpgEncoder, encoder);
                return dateTaken + " (" + number.ToString() + ")" + ".jpg";
            }
        }

        /// <summary>
        /// is triggered if someone sends a message to the telegram bot, determines message type and takes actions
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Bot_OnMessage(object sender, MessageEventArgs e)
        {
            if (Userlist.HasAuth(e.Message.Chat.Username))
            {
                var messageId = Config.config.MsgIncrement;
                Config.config.MsgIncrement++;

                // Message Types
                switch (e.Message.Type)
                {
                    case Telegram.Bot.Types.Enums.MessageType.TextMessage:
                        // Textmessage
                        ParseCommands(e);
                        break;
                    case Telegram.Bot.Types.Enums.MessageType.PhotoMessage:
                        //Disabled because metadata is cut when sending a photo
                        _bot.SendTextMessageAsync(e.Message.Chat.Id, "Bild abgelehnt, bitte als Datei senden.");
                        Trace.WriteLine(Server.NowLog + " " + Server.MessageIDformat(messageId) + e.Message.Chat.Username + " tried to send a picture, but sent not as file.");

                        // Picture
                        //Trace.WriteLine(serverlogic.NowLog + " Photo incoming from " + e.Message.Chat.Username);
                        //Download_img(e);
                        break;
                    case Telegram.Bot.Types.Enums.MessageType.DocumentMessage:
                        Trace.WriteLine(Server.NowLog + " " + Server.MessageIDformat(messageId) + " Document incoming from " + e.Message.Chat.Username);
                        Download_document(e, messageId);
                        break;
                }
            }
            else if (e.Message.Type == Telegram.Bot.Types.Enums.MessageType.TextMessage && e.Message.Chat.Username != null && e.Message.Text.StartsWith("/auth "))
            {
                var hasher = new Hasher();
                if (hasher.Check(e.Message.Text.Remove(0, 6), new HashedPassword(Config.config.Hash, Config.config.Salt)))
                {
                    File.AppendAllText(Config.config.PathUsers, e.Message.Chat.Username + Environment.NewLine);
                    Trace.WriteLine(Server.NowLog + " " + e.Message.Chat.Username + " has just authenticated a new Device.");
                    _bot.SendTextMessageAsync(e.Message.Chat.Id, "Erfolgreich Authentifiziert.");
                }
                else
                {
                    Trace.WriteLine(Server.NowLog + " " + e.Message.Chat.Username + " tried to authenticate, but entered wrong password.");
                    _bot.SendTextMessageAsync(e.Message.Chat.Id, "Authentifizierung fehlgeschlagen. Falsches Passwort.");
                }
            }
            else
            {
                _bot.SendTextMessageAsync(e.Message.Chat.Id, "Authentifizierung fehlgeschlagen. Stellen Sie sicher dass ein Benutzername gesetzt ist.");
            }
        }

        /// <summary>
        /// Starts listener
        /// </summary>
        public void Start_bot()
        {
            _bot.OnMessage += Bot_OnMessage;
            _bot.OnMessageEdited += Bot_OnMessage;
            _bot.StartReceiving();
        }

        /// <summary>
        /// Stops listener
        /// </summary>
        public void Stop_bot()
        {
            _bot.StopReceiving();
            _bot.OnMessage -= Bot_OnMessage;
            _bot.OnMessageEdited -= Bot_OnMessage;
        }

        /// <summary>
        /// Extracts time and date when the picture was taken from the metadata
        /// </summary>
        /// <param name="image"></param>
        /// <param name="e"></param>
        /// <param name="messageId"></param>
        /// <returns></returns>
        private static string Date_taken(Image image, MessageEventArgs e, int messageId)
        {
            try
            {
                var propItems = image.PropertyItems;
                var encoding = Encoding.UTF8;
                var dataTakenProperty1 = propItems.FirstOrDefault(a => a.Id.ToString("x") == "9004");
                var dataTakenProperty2 = propItems.FirstOrDefault(a => a.Id.ToString("x") == "9003");
                var originalDateString = encoding.GetString(dataTakenProperty1.Value);
                originalDateString = originalDateString.Remove(originalDateString.Length - 1);
                return originalDateString.Replace(":", "-").Replace(" ", "_");
            }
            catch
            {
                Trace.WriteLine(Server.NowLog + " " + Server.MessageIDformat(messageId) + " Photo has no capture time (using servertime instead) from " + e.Message.Chat.Username);
                return DateTime.Today.ToString("yyyy-MM-dd") + "_" + DateTime.Now.ToString("HH-mm-ss", System.Globalization.DateTimeFormatInfo.InvariantInfo) + "_noCaptureTime";
            }
        }

        /// <summary>
        /// Parses Commands sent via a user
        /// </summary>
        /// <param name="e"></param>
        private void ParseCommands(MessageEventArgs e)
        {
            var temp = e.Message.Text.Split(' ');
            var command = temp[0];

            if (Userlist.HasAdminPrivilege(e.Message.Chat.Username))
                AdminCommands(e, command);
            else
                CommonCommands(e, command);
        }

        /// <summary>
        /// executes admin commands
        /// </summary>
        /// <param name="e"></param>
        /// <param name="command"></param>
        private void AdminCommands(MessageEventArgs e, string command)
        {
            // ADMIN AREA
            switch (command)
            {
                case "/activity":
                    var b = new StringBuilder();
                    var list = Userlist.GetUseractivity();
                    for (int i = 0; i < Userlist.UsersAmount; i++)
                    {
                        b.AppendLine(list[i, 0] + " - " + list[i, 1]);
                    }
                    _bot.SendTextMessageAsync(e.Message.Chat.Id, b.ToString());
                    break;
                case "/party":
                    _bot.SendTextMessageAsync(e.Message.Chat.Id, "Du bist jetzt im Partymodus. Nur Admins können in den Partymodus wechseln. GZ!");
                    break;
                default:
                    //Admin can of course execute normal commands too
                    CommonCommands(e, command);
                    break;
            }
        }

        /// <summary>
        /// executes normal commands
        /// </summary>
        /// <param name="e"></param>
        /// <param name="command"></param>
        private void CommonCommands(MessageEventArgs e, string command)
        {
            // NORMAL AREA
            switch (command)
            {
                case "/help":
                    var commandsList = new List<string>();
                    
                    if (Userlist.HasAdminPrivilege(e.Message.Chat.Username))
                    {
                        commandsList.Add("/activity - Zeigt Gesamtaktivität aller registrierten Benutzer an");
                    }
                    commandsList.Add("/koff - Komprimierung für ein Bild ausschalten");
                    commandsList.Add("/kon - Komprimierung einschalten");
                    commandsList.Add("/admin <pw> - Adminrechte freischalten");
                    commandsList.Add("/auth <pw> - Authentifiziert einen neuen Benutzer"); // auth is handled in Bot_OnMessage
                    commandsList.Add("/help - Zeigt diesen Text an");
                    commandsList.Sort();

                    var b = new StringBuilder();
                    b.AppendLine("Befehle:");
                    foreach (var line in commandsList)
                        b.AppendLine(line);
                    _bot.SendTextMessageAsync(e.Message.Chat.Id, b.ToString());
                    break;
                case "/admin":
                    var hasher = new Hasher();
                    if (hasher.Check(e.Message.Text.Remove(0, 7), new HashedPassword(Config.config.Hash, Config.config.Salt)))
                    {
                        Userlist.SetAdminPrivilege(e.Message.Chat.Username, true);
                        Trace.WriteLine(
                            Server.NowLog + " " + e.Message.Chat.Username + " has just authenticated as Admin");
                        _bot.SendTextMessageAsync(e.Message.Chat.Id, "Sie sind jetzt Admin.");
                    }
                    else
                    {
                        Userlist.SetAdminPrivilege(e.Message.Chat.Username, false);
                        Trace.WriteLine(Server.NowLog + " " + e.Message.Chat.Username +
                                        " tried to get admin, but entered wrong password.");
                        _bot.SendTextMessageAsync(e.Message.Chat.Id,
                            "Authentifizierung fehlgeschlagen. Falsches Passwort. Sie sind jetzt kein Admin.");
                    }
                    break;
                case "/koff":
                    Userlist.SetCompression(e.Message.Chat.Username, false);
                    Trace.WriteLine(Server.NowLog + " " + e.Message.Chat.Username + " disabled compression");
                    _bot.SendTextMessageAsync(e.Message.Chat.Id, "Komprimieren ist für das nächste Bild deaktiviert");
                    break;
                case "/kon":
                    Userlist.SetCompression(e.Message.Chat.Username, true);
                    Trace.WriteLine(Server.NowLog + " " + e.Message.Chat.Username + " enabled compression");
                    _bot.SendTextMessageAsync(e.Message.Chat.Id, "Komprimieren ist aktiviert");
                    break;
                default:
                    Trace.WriteLine(Server.NowLog + " Note: " + e.Message.Text);
                    _bot.SendTextMessageAsync(e.Message.Chat.Id, "Dieser Befehl hat keine Bedeutung.");
                    break;
            }
        }
    }
}
