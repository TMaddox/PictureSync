using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static PictureSync.Logic.Config;

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
            var files = Directory.GetFiles(Path.Combine(PathPhotos, user)).ToList();
            return files.Aggregate<string, long>(0, (size, file) => size + GetFileSize(file));
        }

        /// <summary>
        /// Returns the total filesize of all photos in bytes
        /// </summary>
        private static long GetFileSizeTotal()
        {
            var userDirs = Directory.GetDirectories(PathPhotos).ToList();
            return userDirs.Aggregate<string, long>(0, (size, user) => size + GetFileSizeOfUser(user));
        }
    }
}
