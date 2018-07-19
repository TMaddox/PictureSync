using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using PictureSync.Properties;
using Telegram.Bot.Args;

namespace PictureSync.Logic
{
    internal static class ImageProcessing
    {
        /// <summary>
        /// Extracts time and date when the picture was taken from the metadata
        /// </summary>
        public static string GetFileName(Image image, MessageEventArgs e, long messageId)
        {
            try
            {
                var originalDateString = GetDateTakenFromImage(image).ToString("yyyy-MM-dd_HH-mm-ss");
                return originalDateString.Replace(":", "-").Replace(".", "-").Replace(" ", "_");
            }
            catch
            {
                Trace.WriteLine(Server.NowLog + " " + Server.MessageIDformat(messageId) + " " + Resources.TelegramBot_Date_taken_no_capturetime + " " + e.Message.Chat.Username);
                return DateTime.Today.ToString("yyyy-MM-dd") + "_" + DateTime.Now.ToString("HH-mm-ss", DateTimeFormatInfo.InvariantInfo) + "_noCaptureTime";
            }
        }

        private static readonly Regex r = new Regex(":");
        /// <summary>
        /// Extracts time and date when the picture was taken from the metadata
        /// </summary>
        private static DateTime GetDateTakenFromImage(string path)
        {
            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            using (var myImage = Image.FromStream(fs, false, false))
            {
                var propItem = myImage.GetPropertyItem(36867);
                var dateTaken = r.Replace(Encoding.UTF8.GetString(propItem.Value), "-", 2);
                return DateTime.Parse(dateTaken);
            }
        }
        /// <summary>
        /// Extracts time and date when the picture was taken from the metadata
        /// </summary>
        private static DateTime GetDateTakenFromImage(Image image)
        {
            var propItem = image.GetPropertyItem(36867);
            var dateTaken = r.Replace(Encoding.UTF8.GetString(propItem.Value), "-", 2);
            return DateTime.Parse(dateTaken);
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
            var finalRect = new Rectangle(0, 0, width, height);
            var finalImage = new Bitmap(width, height);

            finalImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(finalImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, finalRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return finalImage;
        }

        /// <summary>
        /// Gets the encoder of a image format
        /// </summary>
        /// <param name="format">image format</param>
        public static ImageCodecInfo GetEncoder(ImageFormat format)
        {
            var codecs = ImageCodecInfo.GetImageDecoders();
            return codecs.FirstOrDefault(codec => codec.FormatID == format.Guid);
        }
    }
}