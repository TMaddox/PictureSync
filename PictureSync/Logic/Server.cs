using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using HashLibrary;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace PictureSync.Logic
{
    class Server
    {
        public void InitiateTracer()
        {
            Trace.Listeners.Clear();
            var twtl = new TextWriterTraceListener(Config.config.Path_log)
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
        public string MessageIDformat(int msgID) => "<" + msgID.ToString().PadLeft(6, '0') + ">";

        public void Create_files()
        {
            if (!File.Exists(Config.config.Path_log))
                using (StreamWriter sw = File.AppendText(Config.config.Path_log));
            if (!File.Exists(Config.config.Path_users))
                using (StreamWriter sw = File.AppendText(Config.config.Path_users));
        }

        public void ReadConfig(string path)
        {
            try
            {
                //Read from file
                string[] file = File.ReadAllLines(path + "config.dat");
                List<string> result = new List<string>();

                foreach (string item in file)
                {
                    int pFrom = item.IndexOf("[") + "[".Length;
                    int pTo = item.LastIndexOf("]");

                    result.Add(item.Substring(pFrom, pTo - pFrom));
                }

                Config.config = new Config
                {
                    Token = result.ElementAt(0),
                    Hash = result.ElementAt(1),
                    Salt = result.ElementAt(2),
                    Path_photos = result.ElementAt(3),
                    Max_len = Convert.ToInt32(result.ElementAt(4)),
                    EncodeQ = Convert.ToInt32(result.ElementAt(5)),
                    Path_root = path
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
            using (StreamWriter sw = File.AppendText(path + "config.dat"))
            {
                Console.Write("Token: ");
                string token = Console.ReadLine();
                sw.WriteLine("Token = [" + token + "]");

                Console.Write("Auth_key: ");
                string auth_key = Console.ReadLine();
                var hasher = new Hasher();
                var hashedPW = hasher.HashPassword(auth_key);
                sw.WriteLine("Hash = [" + hashedPW.Hash + "]");
                sw.WriteLine("Salt = [" + hashedPW.Salt + "]");

                Console.Write("Path for pictures: ");
                string path_pictures = Console.ReadLine();
                sw.WriteLine("path_pictures = [" + path_pictures + "]");

                Console.Write("Maximal lenght of pictures: ");
                string max_len = Console.ReadLine();
                sw.WriteLine("max_picture_lenght = [" + max_len + "]");

                Console.Write("Quality of encoding (1-100): ");
                string encodingQ = Console.ReadLine();
                sw.WriteLine("encoding_Quality = [" + encodingQ + "]");

                Console.Clear();
            }
            System.Diagnostics.Process.Start(Application.ExecutablePath);
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
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
            foreach (ImageCodecInfo codec in codecs)
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
