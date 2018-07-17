using HashLibrary;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using PictureSync.Properties;
using Telegram.Bot;
using Telegram.Bot.Args;

using static PictureSync.Logic.Config;
using static PictureSync.Logic.Server;
using static PictureSync.Logic.Userlist;
using static PictureSync.Logic.Commands;
using static PictureSync.Logic.ImageProcessing;

namespace PictureSync.Logic
{
    internal static class TelegramBot
    {
        /// <summary>
        /// Download a document from telegram Server
        /// </summary>
        private static async Task Download_document(MessageEventArgs e, long messageId)
        {
            // Create dir for username if not exists
            Directory.CreateDirectory(PathPhotos + e.Message.Chat.Username);

            if (e.Message.Document.MimeType == "image/png" || e.Message.Document.MimeType == "image/jpeg")
            {
                // Get and save file
                var file = await Bot.GetFileAsync(e.Message.Document.FileId);
                var filename = SaveImage(e, Image.FromStream(file.FileStream), messageId);

                // Log file saved
                OutputResult(NowLog + " " + MessageIDformat(messageId) + " " + Resources.TelegramBot_picture_accepted_log + " " +
                             e.Message.Chat.Username + " " + Resources.TelegramBot_picture_saved_as_log + " " + filename, 
                    e, HasCompression(e.Message.Chat.Username)
                        ? Resources.TelegramBot_picture_accepted
                        : Resources.TelegramBot_picture_accepted_uncompressed);

                // Add +1 to picture counter, auto enable compression
                AddPictureAmount(e.Message.Chat.Username);
                if (SetCompression(e.Message.Chat.Username, true))
                {
                    OutputResult(NowLog + " " + Resources.TelegramBot_picture_compression_autoenable_log + " " + e.Message.Chat.Username, e, Resources.TelegramBot_picture_compression_autoenable);
                }
                SetLatestActivity(e.Message.Chat.Username,DateTime.Today);
            }
            else
            {
                OutputResult(NowLog + " " + MessageIDformat(messageId) + " " + Resources.TelegramBot_picture_wrong_filetype_log + " " + e.Message.Chat.Username, e, Resources.TelegramBot_picture_wrong_filetype);
            }
        }

        /// <summary>
        /// Compress and save a image
        /// </summary>
        private static string SaveImage(MessageEventArgs e, Image image, long messageId)
        {
            Bitmap finalImage;
            var hasCompression = HasCompression(e.Message.Chat.Username);
            var res = (double)image.Width / image.Height;
            var username = e.Message.Chat.Username;
            int width = image.Width,
                height = image.Height;
            var dateTaken = GetDateTaken(image, e, messageId);

            GetDimensions(res, hasCompression, ref height, ref width);

            if (image.Width > width && image.Height > height)
                finalImage = ResizeImg(image, width, height);
            else
                finalImage = ResizeImg(image, image.Width, image.Height);

            var jpgEncoder = GetEncoder(ImageFormat.Jpeg);
            var myEncoder = Encoder.Quality;
            var encoder = new EncoderParameters(1);
            var encoderParameter = new EncoderParameter(myEncoder, hasCompression ? EncodeQ : 93L);
            encoder.Param[0] = encoderParameter;

            var capturetime = GetDateTakenFromImage(image);
            if (capturetime != null)
            {
                finalImage.SetPropertyItem(capturetime);
            }
            
            if (!File.Exists(PathPhotos + username + @"\" + dateTaken + ".jpg"))
            {
                finalImage.Save(PathPhotos + username + @"\" + dateTaken + ".jpg", jpgEncoder, encoder);
                return dateTaken + ".jpg";
            }
            else if (!File.Exists(PathPhotos + username + @"\" + dateTaken + " (2)" + ".jpg"))
            {
                finalImage.Save(PathPhotos + username + @"\" + dateTaken + " (2)" + ".jpg", jpgEncoder, encoder);
                return dateTaken + " (2)" + ".jpg";
            }
            else
            {
                var number = GetFileIterationNumber(username, dateTaken);
                number++;
                finalImage.Save(PathPhotos + username + @"\" + dateTaken + " (" + number + ")" + ".jpg", jpgEncoder, encoder);
                return dateTaken + " (" + number + ")" + ".jpg";
            }
        }

        /// <summary>
        /// Returns the number of the file. Usecase: example(3).jpg
        /// </summary>
        private static int GetFileIterationNumber(string username, string filename)
        {
            var path = PathPhotos + username + @"\";
            var dir = new DirectoryInfo(path);
            var number = 0;

            foreach (var file in dir.GetFiles(filename +"*" + ".jpg"))
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

                number = number < test ? test : number;
            }
            return number;
        }

        /// <summary>
        /// Returns the Dimensions of a picture, depending on if compression is enabled and resolution
        /// </summary>
        private static void GetDimensions(double res, bool hasCompression, ref int height, ref int width)
        {
            if (res <= 1)
            {
                //Portrait
                height = hasCompression ? MaxLen : height;
                width = hasCompression ? Convert.ToInt16(height * res) : width;
            }
            else
            {
                //Landscape
                width = hasCompression ? MaxLen : width;
                height = hasCompression ? Convert.ToInt16(width / res) : height;
            }
        }

        /// <summary>
        /// is triggered if someone sends a message to the telegram bot, determines message type and takes actions
        /// </summary>
        private static void Bot_OnMessage(object sender, MessageEventArgs e)
        {
            if (HasAuth(e.Message.Chat.Username))
            {
                ReceiveMessage(e);
            }
            else if (e.Message.Type == Telegram.Bot.Types.Enums.MessageType.TextMessage && e.Message.Chat.Username != null && e.Message.Text.StartsWith("/auth "))
            {
                Auth(e);
            }
            else
            {
                OutputResult("", e, Resources.TelegramBot_Bot_OnMessage_auth_no_username);
            }
        }

        /// <summary>
        /// Receives the Message
        /// </summary>
        private static void ReceiveMessage(MessageEventArgs e)
        {
            var messageId = e.Message.MessageId;
            var username = e.Message.Chat.Username;

            // Message Types
            switch (e.Message.Type)
            {
                case Telegram.Bot.Types.Enums.MessageType.TextMessage:
                    // Textmessage
                    ParseCommands(e);
                    break;
                case Telegram.Bot.Types.Enums.MessageType.PhotoMessage:
                    //Disabled because metadata is cut when sending a photo and we need capture date
                    OutputResult(NowLog + " " + MessageIDformat(messageId) + username + " " + Resources.TelegramBot_Bot_OnMessage_deny_picture_log, e, Resources.TelegramBot_Bot_OnMessage_deny_picture);
                    break;
                case Telegram.Bot.Types.Enums.MessageType.DocumentMessage:
                    OutputResult(NowLog + " " + MessageIDformat(messageId) + " " + Resources.TelegramBot_Bot_OnMessage_document_incoming_log + " " + username, e, "");
                    Download_document(e, messageId);
                    break;
            }
        }

        /// <summary>
        /// Starts listener
        /// </summary>
        public static void Start_bot()
        {
            Bot = new TelegramBotClient(Token);
            Bot.OnMessage += Bot_OnMessage;
            Bot.OnMessageEdited += Bot_OnMessage;
            Bot.StartReceiving();
        }

        /// <summary>
        /// Stops listener
        /// </summary>
        public static void Stop_bot()
        {
            Bot.StopReceiving();
            Bot.OnMessage -= Bot_OnMessage;
            Bot.OnMessageEdited -= Bot_OnMessage;
        }

        /// <summary>
        /// Outputs the result to the log and the user
        /// </summary>
        /// <param name="log">What to write to the log</param>
        /// <param name="e">Message Args</param>
        /// <param name="user">What to tell the user</param>
        public static void OutputResult(string log = "", MessageEventArgs e = null, string user = "")
        {
            if (log != "")
                Trace.WriteLine(log);
            if (user != "")
                Bot.SendTextMessageAsync(e.Message.Chat.Id, user);
        }
    }
}
