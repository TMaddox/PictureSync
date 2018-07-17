using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static PictureSync.Logic.Config;
using static PictureSync.Logic.Userlist;

namespace PictureSync.Logic
{
    static class Statistics
    {
        /// <summary>
        /// Returns the filesize in bytes
        /// </summary>
        private static long GetFileSize(string path)
        {
            var fileinfo = new FileInfo(path);
            return fileinfo.Length;
        }
        /// <summary>
        /// Returns the file size of the user in bytes
        /// </summary>
        private static long GetFileSizeOfUser(string user)
        {
            try
            {
                var files = Directory.GetFiles(Path.Combine(PathPhotos, user)).ToList();
                return files.Aggregate<string, long>(0, (size, file) => size + GetFileSize(file));
            }
            catch (Exception)
            {
                return 0;
            }
        }
        /// <summary>
        /// Returns the total filesize of all photos in bytes
        /// </summary>
        public static long GetFileSizeTotal()
        {
            try
            {
                var userDirs = Directory.GetDirectories(PathPhotos).ToList();
                return userDirs.Aggregate<string, long>(0, (size, user) => size + GetFileSizeOfUser(user));
            }
            catch (Exception)
            {
                return 0;
            }
        }

        /// <summary>
        /// Returns total Amount of received Pictures
        /// </summary>
        public static long GetPictureAmountTotal()
        {
            long amount = 0;
            foreach (var user in Users)
            {
                var userdata = user.Split(',');
                amount += Convert.ToInt32(userdata[3]);
            }
            return amount;
        }

        /// <summary>
        /// Filesize Humaniser
        /// </summary>
        /// <param name="byteCount">Filesize in bytes</param>
        /// <returns>Readable string containing the filesize</returns>
        public static string HumaniserBytesToString(long byteCount)
        {
            string[] suf = { " B", " KB", " MB", " GB", " TB", " PB", " EB" };
            if (byteCount == 0)
                return "0" + suf[0];
            var bytes = Math.Abs(byteCount);
            var place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            var num = Math.Round(bytes / Math.Pow(1024, place), 1);
            return (Math.Sign(byteCount) * num) + suf[place];
        }
    }
}
