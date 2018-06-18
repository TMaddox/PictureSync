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
using PictureSync.Properties;
using Telegram.Bot;
using Telegram.Bot.Args;
using static PictureSync.Logic.Config;
using static PictureSync.Logic.Server;
using static PictureSync.Logic.Userlist;

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
        private readonly TelegramBotClient _bot = new TelegramBotClient(config.Token);

        /// <summary>
        /// Download a document from telegram Server
        /// </summary>
        /// <param name="e"></param>
        /// <param name="messageId"></param>
        /// <returns></returns>
        private async Task Download_document(MessageEventArgs e, int messageId)
        {
            // Create dir for username if not exists
            Directory.CreateDirectory(config.PathPhotos + e.Message.Chat.Username);

            if (e.Message.Document.MimeType == "image/png" || e.Message.Document.MimeType == "image/jpeg")
            {
                // Get and save file
                var file = await _bot.GetFileAsync(e.Message.Document.FileId);
                var image = Image.FromStream(file.FileStream);
                var filename = Save_image(e, image, messageId);

                // Log file saved
                if(HasCompression(e.Message.Chat.Username))
                    await _bot.SendTextMessageAsync(e.Message.Chat.Id, Resources.TelegramBot_picture_accepted);
                else
                    await _bot.SendTextMessageAsync(e.Message.Chat.Id, Resources.TelegramBot_picture_accepted_uncompressed);
                Trace.WriteLine(NowLog + " " + MessageIDformat(messageId) + " " + Resources.TelegramBot_picture_accepted_log + " " +
                                e.Message.Chat.Username + " " + Resources.TelegramBot_picture_saved_as_log + " " + filename);

                // Add +1 to picture counter, auto enable compression
                AddPictureAmount(e.Message.Chat.Username);
                if (SetCompression(e.Message.Chat.Username, true))
                {
                    Trace.WriteLine(NowLog + " " + Resources.TelegramBot_picture_compression_autoenable_log + " " + e.Message.Chat.Username);
                    await _bot.SendTextMessageAsync(e.Message.Chat.Id, Resources.TelegramBot_picture_compression_autoenable);
                }
                SetLatestActivity(e.Message.Chat.Username,DateTime.Today);
            }
            else
            {
                await _bot.SendTextMessageAsync(e.Message.Chat.Id, Resources.TelegramBot_picture_wrong_filetype);
                Trace.WriteLine(NowLog + " " + MessageIDformat(messageId) +
                                " " + Resources.TelegramBot_picture_wrong_filetype_log + " " + e.Message.Chat.Username);
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
            var hasCompression = HasCompression(e.Message.Chat.Username);
            var res = (double)image.Width / image.Height;
            int width = image.Width,
                height = image.Height;
            var dateTaken = Date_taken(image, e, messageId);

            if(res <= 1)
            {
                //Hochformat
                height = hasCompression ? config.MaxLen : height;
                width = hasCompression ? Convert.ToInt16(height * res): width;
            }
            else
            {
                //Querformat
                width = hasCompression ? config.MaxLen : width;
                height = hasCompression ? Convert.ToInt16(width / res): height;
            }

            if (image.Width > width && image.Height > height)
                finalImage = ResizeImg(image, width, height);
            else
                finalImage = ResizeImg(image, image.Width, image.Height);

            var jpgEncoder = GetEncoder(ImageFormat.Jpeg);
            var myEncoder = System.Drawing.Imaging.Encoder.Quality;
            var encoder = new EncoderParameters(1);
            var encoderParameter = new EncoderParameter(myEncoder, hasCompression ? config.EncodeQ : 93L);
            encoder.Param[0] = encoderParameter;

            if (!File.Exists(config.PathPhotos + e.Message.Chat.Username + @"\" + dateTaken + ".jpg"))
            {
                finalImage.Save(config.PathPhotos + e.Message.Chat.Username + @"\" + dateTaken + ".jpg", jpgEncoder, encoder);
                return dateTaken + ".jpg";
            }
            else if (!File.Exists(config.PathPhotos + e.Message.Chat.Username + @"\" + dateTaken + " (2)" + ".jpg"))
            {
                finalImage.Save(config.PathPhotos + e.Message.Chat.Username + @"\" + dateTaken + " (2)" + ".jpg", jpgEncoder, encoder);
                return dateTaken + " (2)" + ".jpg";
            }
            else
            {
                var path = config.PathPhotos + e.Message.Chat.Username + @"\";
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
                finalImage.Save(config.PathPhotos + e.Message.Chat.Username + @"\" + dateTaken + " (" + number + ")" + ".jpg", jpgEncoder, encoder);
                return dateTaken + " (" + number + ")" + ".jpg";
            }
        }

        /// <summary>
        /// is triggered if someone sends a message to the telegram bot, determines message type and takes actions
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Bot_OnMessage(object sender, MessageEventArgs e)
        {
            if (HasAuth(e.Message.Chat.Username))
            {
                var messageId = config.MsgIncrement;
                config.MsgIncrement++;

                // Message Types
                switch (e.Message.Type)
                {
                    case Telegram.Bot.Types.Enums.MessageType.TextMessage:
                        // Textmessage
                        ParseCommands(e);
                        break;
                    case Telegram.Bot.Types.Enums.MessageType.PhotoMessage:
                        //Disabled because metadata is cut when sending a photo
                        _bot.SendTextMessageAsync(e.Message.Chat.Id, Resources.TelegramBot_Bot_OnMessage_deny_picture);
                        Trace.WriteLine(NowLog + " " + MessageIDformat(messageId) + e.Message.Chat.Username + " " + Resources.TelegramBot_Bot_OnMessage_deny_picture_log);

                        // Picture
                        //Trace.WriteLine(serverlogic.NowLog + " Photo incoming from " + e.Message.Chat.Username);
                        //Download_img(e);
                        break;
                    case Telegram.Bot.Types.Enums.MessageType.DocumentMessage:
                        Trace.WriteLine(NowLog + " " + MessageIDformat(messageId) + " " + Resources.TelegramBot_Bot_OnMessage_document_incoming_log + " " + e.Message.Chat.Username);
                        Download_document(e, messageId);
                        break;
                }
            }
            else if (e.Message.Type == Telegram.Bot.Types.Enums.MessageType.TextMessage && e.Message.Chat.Username != null && e.Message.Text.StartsWith("/auth "))
            {
                var hasher = new Hasher();
                if (hasher.Check(e.Message.Text.Remove(0, 6), new HashedPassword(config.Hash, config.Salt)))
                {
                    File.AppendAllText(config.PathUsers, e.Message.Chat.Username + ",1,0,0," + DateTime.Today.ToString("yyyy-MM-dd") + Environment.NewLine);
                    SortUsers();
                    Trace.WriteLine(NowLog + " " + e.Message.Chat.Username + " " + Resources.TelegramBot_Bot_OnMessage_auth_successful_log);
                    _bot.SendTextMessageAsync(e.Message.Chat.Id, Resources.TelegramBot_Bot_OnMessage_auth_successful);
                }
                else
                {
                    Trace.WriteLine(NowLog + " " + e.Message.Chat.Username + " " + Resources.TelegramBot_Bot_OnMessage_auth_not_successful_log);
                    _bot.SendTextMessageAsync(e.Message.Chat.Id, Resources.TelegramBot_Bot_OnMessage_auth_not_successful);
                }
            }
            else
            {
                _bot.SendTextMessageAsync(e.Message.Chat.Id, Resources.TelegramBot_Bot_OnMessage_auth_no_username);
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
                Trace.WriteLine(NowLog + " " + MessageIDformat(messageId) + " " + Resources.TelegramBot_Date_taken_no_capturetime + " " + e.Message.Chat.Username);
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

            if (HasAdminPrivilege(e.Message.Chat.Username))
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
                case "/activity_amount":
                    var b1 = new StringBuilder();
                    var list1 = GetUseractivity_Amount();
                    for (var i = 0; i < UsersAmount; i++)
                    {
                        b1.AppendLine(list1[i, 0] + " - " + list1[i, 1]);
                    }
                    Trace.WriteLine(NowLog + " " + e.Message.Chat.Username + " " + Resources.TelegramBot_AdminCommands_activity_accessed);
                    _bot.SendTextMessageAsync(e.Message.Chat.Id, b1.ToString());
                    break;
                case "/activity_time":
                    var b = new StringBuilder();
                    var list = GetUseractivity_Time();
                    for (var i = 0; i < UsersAmount; i++)
                    {
                        b.AppendLine(list[i, 0] + " - " + list[i, 1]);
                    }
                    Trace.WriteLine(NowLog + " " + e.Message.Chat.Username + " " + Resources.TelegramBot_AdminCommands_activity_accessed);
                    _bot.SendTextMessageAsync(e.Message.Chat.Id, b.ToString());
                    break;
                case "/party":
                    _bot.SendTextMessageAsync(e.Message.Chat.Id, Resources.TelegramBot_AdminCommands_party);
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
                    
                    if (HasAdminPrivilege(e.Message.Chat.Username))
                    {
                        commandsList.Add(Resources.TelegramBot_CommonCommands_help_activity_amount);
                        commandsList.Add(Resources.TelegramBot_CommonCommands_help_activity_time);
                    }
                    commandsList.Add(Resources.TelegramBot_CommonCommands_help_coff);
                    commandsList.Add(Resources.TelegramBot_CommonCommands_help_con);
                    commandsList.Add(Resources.TelegramBot_CommonCommands_help_admin);
                    commandsList.Add(Resources.TelegramBot_CommonCommands_help_auth); // auth is handled in Bot_OnMessage
                    commandsList.Add(Resources.TelegramBot_CommonCommands_help_help);
                    commandsList.Sort();

                    var b = new StringBuilder();
                    b.AppendLine(Resources.TelegramBot_CommonCommands_commands);
                    foreach (var line in commandsList)
                        b.AppendLine(line);
                    _bot.SendTextMessageAsync(e.Message.Chat.Id, b.ToString());
                    break;
                case "/admin":
                    var hasher = new Hasher();
                    if (hasher.Check(e.Message.Text.Remove(0, 7), new HashedPassword(config.Hash, config.Salt)))
                    {
                        SetAdminPrivilege(e.Message.Chat.Username, true);
                        Trace.WriteLine(
                            NowLog + " " + e.Message.Chat.Username + " " + Resources.TelegramBot_CommonCommands_admin_successful_log);
                        _bot.SendTextMessageAsync(e.Message.Chat.Id, Resources.TelegramBot_CommonCommands_admin_successful);
                    }
                    else
                    {
                        SetAdminPrivilege(e.Message.Chat.Username, false);
                        Trace.WriteLine(NowLog + " " + e.Message.Chat.Username +
                                        " " + Resources.TelegramBot_CommonCommands_admin_not_successful_log);
                        _bot.SendTextMessageAsync(e.Message.Chat.Id,
                            Resources.TelegramBot_CommonCommands_admin_not_successful);
                    }
                    break;
                case "/coff":
                    SetCompression(e.Message.Chat.Username, false);
                    Trace.WriteLine(NowLog + " " + e.Message.Chat.Username + " " + Resources.TelegramBot_CommonCommands_coff_log);
                    _bot.SendTextMessageAsync(e.Message.Chat.Id, Resources.TelegramBot_CommonCommands_coff);
                    break;
                case "/con":
                    SetCompression(e.Message.Chat.Username, true);
                    Trace.WriteLine(NowLog + " " + e.Message.Chat.Username + " " + Resources.TelegramBot_CommonCommands_con_log);
                    _bot.SendTextMessageAsync(e.Message.Chat.Id, Resources.TelegramBot_CommonCommands_con);
                    break;
                default:
                    Trace.WriteLine(NowLog + " " + Resources.TelegramBot_CommonCommands_Note_log + " " + e.Message.Text);
                    _bot.SendTextMessageAsync(e.Message.Chat.Id, Resources.TelegramBot_CommonCommands_nocommand);
                    break;
            }
        }
    }
}
