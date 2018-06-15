using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using HashLibrary;

namespace PictureSync.Logic
{
    internal class TelegramBot
    {
        private readonly Server _serverlogic = new Server();

        // global static object because only one bot is needed
        public static TelegramBot Telebot;

        private readonly TelegramBotClient _bot = new TelegramBotClient(Config.config.Token);

        public async Task Download_document(Telegram.Bot.Args.MessageEventArgs e, int messageId)
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
                Trace.WriteLine(_serverlogic.NowLog + " " + _serverlogic.MessageIDformat(messageId) + " Saved photo from " + e.Message.Chat.Username + " as " + filename);

                // Add +1 to picture counter, autoenable compression
                Userlist.AddPictureAmount(e.Message.Chat.Username);
                if (Userlist.SetCompression(e.Message.Chat.Username, true))
                {
                    Trace.WriteLine(_serverlogic.NowLog + " " + e.Message.Chat.Username + " autoenable compression");
                    await _bot.SendTextMessageAsync(e.Message.Chat.Id, "Komprimieren ist wieder aktiviert");
                }
            }
            else
            {
                await _bot.SendTextMessageAsync(e.Message.Chat.Id, "Error. Wrong File Type");
                Trace.WriteLine(_serverlogic.NowLog + " " + _serverlogic.MessageIDformat(messageId) + " Error, wrong file Type from " + e.Message.Chat.Username);
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
                finalImage = _serverlogic.ResizeImg(image, width, height);
            else
                finalImage = _serverlogic.ResizeImg(image, image.Width, image.Height);

            var jpgEncoder = _serverlogic.GetEncoder(ImageFormat.Jpeg);
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

        public void Bot_OnMessage(object sender, Telegram.Bot.Args.MessageEventArgs e)
        {
            if (Userlist.HasAuth(e.Message.Chat.Username))
            {
                var messageId = Config.config.Msg_Increment;
                Config.config.Msg_Increment++;

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
                        Trace.WriteLine(_serverlogic.NowLog + " " + _serverlogic.MessageIDformat(messageId) + e.Message.Chat.Username + " tried to send a picture, but sent not as file.");

                        // Picture
                        //Trace.WriteLine(serverlogic.NowLog + " Photo incoming from " + e.Message.Chat.Username);
                        //Download_img(e);
                        break;
                    case Telegram.Bot.Types.Enums.MessageType.DocumentMessage:
                        Trace.WriteLine(_serverlogic.NowLog + " " + _serverlogic.MessageIDformat(messageId) + " Document incoming from " + e.Message.Chat.Username);
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
                    Trace.WriteLine(_serverlogic.NowLog + " " + e.Message.Chat.Username + " has just authenticated a new Device.");
                    _bot.SendTextMessageAsync(e.Message.Chat.Id, "Erfolgreich Authentifiziert.");
                }
                else
                {
                    Trace.WriteLine(_serverlogic.NowLog + " " + e.Message.Chat.Username + " tried to authenticate, but entered wrong password.");
                    _bot.SendTextMessageAsync(e.Message.Chat.Id, "Authentifizierung fehlgeschlagen. Falsches Passwort.");
                }
            }
            else
            {
                _bot.SendTextMessageAsync(e.Message.Chat.Id, "Authentifizierung fehlgeschlagen. Stellen Sie sicher dass ein Benutzername gesetzt ist.");
            }
        }

        public void Start_bot()
        {
            _bot.OnMessage += Bot_OnMessage;
            _bot.OnMessageEdited += Bot_OnMessage;
            _bot.StartReceiving();
        }

        public void Stop_bot()
        {
            _bot.StopReceiving();
            _bot.OnMessage -= Bot_OnMessage;
            _bot.OnMessageEdited -= Bot_OnMessage;
        }

        public string Date_taken(Image image, Telegram.Bot.Args.MessageEventArgs e, int messageId)
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
                Trace.WriteLine(_serverlogic.NowLog + " " + _serverlogic.MessageIDformat(messageId) + " Photo has no capture time (using servertime instead) from " + e.Message.Chat.Username);
                return DateTime.Today.ToString("yyyy-MM-dd") + "_" + DateTime.Now.ToString("HH-mm-ss", System.Globalization.DateTimeFormatInfo.InvariantInfo) + "_noCaptureTime";
            }
        }

        public void ParseCommands(Telegram.Bot.Args.MessageEventArgs e)
        {
            var temp = e.Message.Text.Split(' ');
            var command = temp[0];

            if (Userlist.HasAdminPrivilege(e.Message.Chat.Username))
            {
                // ADMIN AREA
                switch (command)
                {
                    case "":
                        break;
                }
            }

            // NORMAL AREA
            switch (command)
            {
                case "/help":
                    var b = new StringBuilder();
                    b.AppendLine("Befehle:");
                    // auth is handled in Bot_OnMessage
                    b.AppendLine("/koff - Komprimierung für ein Bild ausschalten");
                    b.AppendLine("/kon - Komprimierung einschalten");
                    b.AppendLine("/admin <pw> - Adminrechte freischalten");
                    b.AppendLine("/auth <pw> - Authentifiziert einen neuen Benutzer");
                    b.AppendLine("/help - Zeigt diesen Text an");
                    _bot.SendTextMessageAsync(e.Message.Chat.Id, b.ToString());
                    break;

                case "/admin":
                    var hasher = new Hasher();
                    if (hasher.Check(e.Message.Text.Remove(0, 7), new HashedPassword(Config.config.Hash, Config.config.Salt)))
                    {
                        Userlist.SetAdminPrivilege(e.Message.Chat.Username, true);
                        Trace.WriteLine(_serverlogic.NowLog + " " + e.Message.Chat.Username + " has just authenticated as Admin");
                        _bot.SendTextMessageAsync(e.Message.Chat.Id, "Sie sind jetzt Admin.");
                    }
                    else
                    {
                        Userlist.SetAdminPrivilege(e.Message.Chat.Username, false);
                        Trace.WriteLine(_serverlogic.NowLog + " " + e.Message.Chat.Username + " tried to get admin, but entered wrong password.");
                        _bot.SendTextMessageAsync(e.Message.Chat.Id, "Authentifizierung fehlgeschlagen. Falsches Passwort. Sie sind jetzt kein Admin.");
                    }
                    break;
                case "/koff":
                    Userlist.SetCompression(e.Message.Chat.Username, false);
                    Trace.WriteLine(_serverlogic.NowLog + " " + e.Message.Chat.Username + " disabled compression");
                    _bot.SendTextMessageAsync(e.Message.Chat.Id, "Komprimieren ist für das nächste Bild deaktiviert");
                    break;
                case "/kon":
                    Userlist.SetCompression(e.Message.Chat.Username, true);
                    Trace.WriteLine(_serverlogic.NowLog + " " + e.Message.Chat.Username + " enabled compression");
                    _bot.SendTextMessageAsync(e.Message.Chat.Id, "Komprimieren ist aktiviert");
                    break;
                default:
                    Trace.WriteLine(_serverlogic.NowLog + " Note: " + e.Message.Text);
                    _bot.SendTextMessageAsync(e.Message.Chat.Id, "Dieser Befehl hat keine Bedeutung.");
                    break;
            }
        }
    }
}
