using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;
using static PictureSync.Logic.Config;
using static PictureSync.Logic.Userlist;
using static PictureSync.Logic.ImageProcessing;

namespace PictureSync.Logic
{
    internal static class Statistics
    {
        /// <summary>
        /// Returns the filesize in bytes, of files, which were stored befor the given date
        /// </summary>
        private static long GetFileSize(string path, DateTime date)
        {
            try
            {
                var fileinfo = new FileInfo(path);
                var filedate = GetDateTakenFromImage(path);
                return filedate < date ? fileinfo.Length : 0;
            }
            catch (Exception)
            {
                return 0;
            }

        }
        /// <summary>
        /// Returns the file size of the user in bytes
        /// </summary>
        private static long GetFileSizeOfUser(string user, DateTime date)
        {
            try
            {
                var files = Directory.GetFiles(Path.Combine(PathPhotos, user)).ToList();
                return files.Aggregate<string, long>(0, (size, file) => size + GetFileSize(file, date));
            }
            catch (Exception)
            {
                return 0;
            }
        }
        /// <summary>
        /// Returns the total filesize of all photos in bytes
        /// </summary>
        public static long GetFileSizeTotal(DateTime? date = null)
        {
            var finalDate = date ?? DateTime.MaxValue;
            try
            {
                var userDirs = Directory.GetDirectories(PathPhotos).ToList();
                return userDirs.Aggregate<string, long>(0, (size, user) => size + GetFileSizeOfUser(user, finalDate));
            }
            catch (Exception)
            {
                return 0;
            }
        }

        /// <summary>
        /// returns 1 if a Picture shall be counted and 0 if not
        /// </summary>
        private static int CountPicture(string path, DateTime date)
        {
            try
            {
                var filedate = GetDateTakenFromImage(path);
                return filedate < date ? 1 : 0;
            }
            catch (Exception)
            {
                return 0;
            }
        }
        /// <summary>
        /// Returns total Amount of stored Pictures of a user, before specified date
        /// </summary>
        private static int GetPictureAmountDirOfUser(string user, DateTime date)
        {
            try
            {
                var files = Directory.GetFiles(Path.Combine(PathPhotos, user)).ToList();
                return files.Sum(file => CountPicture(file, date));
            }
            catch (Exception)
            {
                return 0;
            }
        }
        /// <summary>
        /// Returns total Amount of received Pictures, which are stored in a Directory, before  the specified date
        /// </summary>
        public static int GetPictureAmountDirTotal(DateTime? date = null)
        {
            var finalDate = date ?? DateTime.MaxValue;
            try
            {
                var userDirs = Directory.GetDirectories(PathPhotos).ToList();
                return userDirs.Sum(user => GetPictureAmountDirOfUser(user, finalDate));
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
            try
            {
                return Users.Sum(GetPictureAmountOfUser);
            }
            catch (Exception)
            {
                return 0;
            }
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
