﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PictureSync.Properties;
using Telegram.Bot.Args;

namespace PictureSync.Logic
{
    internal static class ImageProcessing
    {
        /// <summary>
        /// Extracts time and date when the picture was taken from the metadata
        /// </summary>
        /// <param name="image"></param>
        /// <param name="e"></param>
        /// <param name="messageId"></param>
        /// <returns></returns>
        public static string Date_taken(Image image, MessageEventArgs e, int messageId)
        {
            try
            {
                var propItem = image.GetPropertyItem(36867);
                var originalDateString = Encoding.UTF8.GetString(propItem.Value);
                originalDateString = originalDateString.Remove(originalDateString.Length - 1);
                return originalDateString.Replace(":", "-").Replace(" ", "_");
            }
            catch
            {
                Trace.WriteLine(Server.NowLog + " " + Server.MessageIDformat(messageId) + " " + Resources.TelegramBot_Date_taken_no_capturetime + " " + e.Message.Chat.Username);
                return DateTime.Today.ToString("yyyy-MM-dd") + "_" + DateTime.Now.ToString("HH-mm-ss", DateTimeFormatInfo.InvariantInfo) + "_noCaptureTime";
            }
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