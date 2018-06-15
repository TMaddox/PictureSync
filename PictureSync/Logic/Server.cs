using System;
using System.Linq;
using System.Diagnostics;
using System.IO;
using HashLibrary;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace PictureSync.Logic
{
    internal class Server
    {
        public void InitiateTracer()
        {
            Trace.Listeners.Clear();
            var twtl = new TextWriterTraceListener(Config.config.PathLog)
            {
                Name = "TextLogger",
                TraceOutputOptions = TraceOptions.ThreadId | TraceOptions.DateTime
            };
            var ctl = new ConsoleTraceListener(false) { TraceOutputOptions = TraceOptions.DateTime };
            Trace.Listeners.Add(twtl);
            Trace.Listeners.Add(ctl);
            Trace.AutoFlush = true;
        }

        public string NowLog => "[" + DateTime.Today.ToString("yyyy.MM.dd") + " " + DateTime.Now.ToString("HH:mm:ss.fff", System.Globalization.DateTimeFormatInfo.InvariantInfo) + "]";
        public string MessageIDformat(int msgId) => "<" + msgId.ToString().PadLeft(6, '0') + ">";

        public void Create_files()
        {
            if (!File.Exists(Config.config.PathLog))
                using (File.AppendText(Config.config.PathLog));
            if (!File.Exists(Config.config.PathUsers))
                using (File.AppendText(Config.config.PathUsers));
        }

        public void ReadConfig(string path)
        {
            try
            {
                //Read from file
                var file = File.ReadAllLines(path + "config.dat");
                var result = (from item in file let pFrom = item.IndexOf("[") + "[".Length let pTo = item.LastIndexOf("]") select item.Substring(pFrom, pTo - pFrom)).ToList();

                Config.config = new Config
                {
                    Token = result.ElementAt(0),
                    Hash = result.ElementAt(1),
                    Salt = result.ElementAt(2),
                    PathPhotos = result.ElementAt(3),
                    Max_len = Convert.ToInt32(result.ElementAt(4)),
                    EncodeQ = Convert.ToInt32(result.ElementAt(5)),
                    PathRoot = path
                };
            }
            catch (Exception)
            {
                Create_Config(path);
            }
            
        }

        public void Create_Config(string path)
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

                Console.Clear();
            }
            Process.Start(Application.ExecutablePath);
            Environment.Exit(0);
        }

        /// <summary>
        /// Resize the image to the specified width and height.
        /// </summary>
        /// <param name="image">The image to resize.</param>
        /// <param name="width">The width to resize to.</param>
        /// <param name="height">The height to resize to.</param>
        /// <returns>The resized image.</returns>
        public Bitmap ResizeImg(Image image, int width, int height)
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

        public ImageCodecInfo GetEncoder(ImageFormat format)
        {
            var codecs = ImageCodecInfo.GetImageDecoders();
            foreach (var codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null;
        }
    }
}
