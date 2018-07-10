﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.IO;
using HashLibrary;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using Telegram.Bot.Args;
using static PictureSync.Logic.Config;

namespace PictureSync.Logic
{
    internal static class Server
    {
        /// <summary>
        /// Initiate a Tracer, Use Tracker.WriteLine instead of Console.WriteLine to write output to logfile
        /// </summary>
        public static void InitiateTracer()
        {
            Trace.Listeners.Clear();
            var twtl = new TextWriterTraceListener(Config.PathLog)
            {
                Name = "TextLogger",
                TraceOutputOptions = TraceOptions.ThreadId | TraceOptions.DateTime
            };
            var ctl = new ConsoleTraceListener(false) { TraceOutputOptions = TraceOptions.DateTime };
            Trace.Listeners.Add(twtl);
            Trace.Listeners.Add(ctl);
            Trace.AutoFlush = true;
        }

        /// <summary>
        /// Creates a timestampstring for the log
        /// </summary>
        public static string NowLog => "[" + DateTime.Today.ToString("yyyy.MM.dd") + " " +
                                       DateTime.Now.ToString("HH:mm:ss.fff",
                                           System.Globalization.DateTimeFormatInfo.InvariantInfo) + "]";

        /// <summary>
        /// retuns the log
        /// </summary>
        /// <param name="maxCount">maximum amount of returned lines</param>
        /// <returns></returns>
        public static List<string> GetLogList(int maxCount)
        {
            var final = new List<string>();
            var stream = File.Open(PathLog, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            var reader = new StreamReader(stream);
            var file = reader.ReadToEnd().Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None).ToList();

            file.Reverse();
            if (maxCount < file.Count)
                for (var i = 0; i < maxCount; i++)
                    final.Add(file[i]);
            else
                final = file;

            return final;
        }

        /// <summary>
        /// Creates a msgID string
        /// </summary>
        /// <param name="msgId">unique msgID ot the MEssage</param>
        /// <returns></returns>
        public static string MessageIDformat(int msgId) => "<" + msgId.ToString().PadLeft(6, '0') + ">";

        /// <summary>
        /// Creates Log and User file if the do not exist
        /// </summary>
        public static void Create_files()
        {
            if (!File.Exists(Config.PathLog))
                using (File.AppendText(Config.PathLog));
            if (!File.Exists(Config.PathUsers))
                using (File.AppendText(Config.PathUsers));
        }

        /// <summary>
        /// Reades the Config file and saves it to Config.config
        /// </summary>
        /// <param name="path">Path of the config file</param>
        public static void ReadConfig(string path)
        {
            try
            {
                //Read from file
                var file = File.ReadAllLines(path + "config.dat");
                var result = (from item in file let pFrom = item.IndexOf("[") + "[".Length let pTo = item.LastIndexOf("]") select item.Substring(pFrom, pTo - pFrom)).ToList();

                Token = result.ElementAt(0);
                Hash = result.ElementAt(1);
                Salt = result.ElementAt(2);
                PathPhotos = result.ElementAt(3);
                MaxLen = Convert.ToInt32(result.ElementAt(4));
                EncodeQ = Convert.ToInt32(result.ElementAt(5));
                Localization = result.ElementAt(6);
                PathRoot = path;
            }
            catch (Exception)
            {
                Create_Config(path);
            }
            
        }

        /// <summary>
        /// Creates a new config file
        /// </summary>
        /// <param name="path">path  for the new config file</param>
        public static void Create_Config(string path)
        {
            File.Delete(path + "config.dat");
            using (var sw = File.AppendText(path + "config.dat"))
            {
                Console.Write("Token: ");
                var token = Console.ReadLine();
                sw.WriteLine("Token = [" + token + "]");

                Console.Write("Auth_key: ");
                var authKey = Console.ReadLine();
                var hasher = new Hasher();
                var hashedPw = hasher.HashPassword(authKey);
                sw.WriteLine("Hash = [" + hashedPw.Hash + "]");
                sw.WriteLine("Salt = [" + hashedPw.Salt + "]");

                Console.Write("Path for pictures: ");
                var pathPictures = Console.ReadLine();
                sw.WriteLine("path_pictures = [" + pathPictures + "]");

                Console.Write("Maximal lenght of pictures: ");
                var maxLen = Console.ReadLine();
                sw.WriteLine("max_picture_lenght = [" + maxLen + "]");

                Console.Write("Quality of encoding (1-100): ");
                var encodingQ = Console.ReadLine();
                sw.WriteLine("encoding_Quality = [" + encodingQ + "]");

                Console.Write("Localisation (en|de): ");
                var localisation = Console.ReadLine();
                sw.WriteLine("localization = [" + localisation + "]");
            }
            RestartBot();
        }

        /// <summary>
        /// Updates the config file from config class
        /// </summary>
        public static void Update_Config()
        {
            File.Delete(PathConfig);
            using (var sw = File.AppendText(PathConfig))
            {
                sw.WriteLine("Token = [" + Token + "]");

                sw.WriteLine("Hash = [" + Hash + "]");
                sw.WriteLine("Salt = [" + Salt + "]");

                sw.WriteLine("path_pictures = [" + PathPhotos + "]");

                sw.WriteLine("max_picture_lenght = [" + MaxLen + "]");

                sw.WriteLine("encoding_Quality = [" + EncodeQ + "]");

                sw.WriteLine("localization = [" + Localization + "]");
            }
            RestartBot();
        }

        /// <summary>
        /// Restarts the Application
        /// </summary>
        private static void RestartBot()
        {
            Process.Start(Application.ExecutablePath);
            Environment.Exit(0);
        }

        /// <summary>
        /// Sort users in users.dat alphabetically
        /// </summary>
        public static void SortUsers()
        {
            var file = File.ReadAllLines(Config.PathUsers);
            var result = file.ToList();
            result.Sort();
            File.WriteAllLines(Config.PathUsers, result);
        }

        /// <summary>
        /// Resize the image to the specified width and height.
        /// </summary>
        /// <param name="image">The image to resize.</param>
        /// <param name="width">The width to resize to.</param>
        /// <param name="height">The height to resize to.</param>
        /// <returns>The resized image.</returns>
        public static Bitmap ResizeImg(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }
            
            return destImage;
        }

        /// <summary>
        /// Gets the encoder of a image format
        /// </summary>
        /// <param name="format">image format</param>
        /// <returns></returns>
        public static ImageCodecInfo GetEncoder(ImageFormat format)
        {
            var codecs = ImageCodecInfo.GetImageDecoders();
            return codecs.FirstOrDefault(codec => codec.FormatID == format.Guid);
        }
    }
}
