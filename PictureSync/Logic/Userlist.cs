using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace PictureSync.Logic
{
    internal static class Userlist
    {
        private static List<string> users = new List<string>();

        /// <summary>
        /// Returns a List of strings with users
        /// </summary>
        public static List<string> Users
        {
            get
            {
                var temp = File.ReadAllLines(Config.config.Path_users).ToList();
                foreach (var user in temp)
                {
                    var userdata = user.Split(',');
                    users.Add(userdata[0]);
                }
                return users;
            }
        }

        /// <summary>
        /// Checks if images shall be comressed for user
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public static bool HasCompression(string username)
        {
            var temp = File.ReadAllLines(Config.config.Path_users).ToList();
            foreach (var user in temp)
            {
                var userdata = user.Split(',');
                if (userdata[0] == username && userdata[1] == "1")
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Sets compression (yes|no) for user, returns true if state was changed
        /// </summary>
        /// <param name="username"></param>
        /// <param name="compress"></param>
        /// <returns></returns>
        public static bool SetCompression(string username, bool compress)
        {
            var temp = File.ReadAllLines(Config.config.Path_users).ToList();
            var statechanged = false;

            foreach (var user in temp)
            {
                var userdata = user.Split(',');
                if (userdata[0] == username)
                {
                    if (userdata[1] != Convert.ToString(Convert.ToInt32(compress)))
                        statechanged = true;
                    userdata[1] = Convert.ToInt32(compress).ToString();
                    WriteUserdata(userdata);
                }
            }
            return statechanged;
        }

        /// <summary>
        /// Checks if user is admin
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public static bool HasAdminPrivilege(string username)
        {
            var temp = File.ReadAllLines(Config.config.Path_users).ToList();
            foreach (var user in temp)
            {
                var userdata = user.Split(',');
                if (userdata[0] == username && userdata[2] == "1")
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Returns the amount of pictures sent in a given date range
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public static int GetPictureAmount(string username)
        {
            var temp = File.ReadAllLines(Config.config.Path_users).ToList();
            foreach (var user in temp)
            {
                var userdata = user.Split(',');
                if (userdata[0] == username)
                    return Convert.ToInt32(userdata[3]);
            }
            return 0;
        }

        /// <summary>
        /// Adds +1 for picturecount for user
        /// </summary>
        /// <param name="username"></param>
        public static void AddPictureAmount(string username)
        {
            var temp = File.ReadAllLines(Config.config.Path_users).ToList();
            foreach (var user in temp)
            {
                var userdata = user.Split(',');
                if (userdata[0] == username)
                {
                    userdata[3] = Convert.ToString(Convert.ToInt32(userdata[3]) + 1);
                    WriteUserdata(userdata);
                }
            }
        }

        /// <summary>
        /// check if user is authorized
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public static bool HasAuth(string username)
        {
            return Users.Contains(username);
        }

        private static void WriteUserdata(string[] userdata)
        {
            var temp = File.ReadAllLines(Config.config.Path_users);
            for (var i = 0; i < temp.Length; i++)
            {
                var line = temp[i].Split(',');
                if (line[0] == userdata[0])
                {
                    var b = new StringBuilder();
                    foreach (var property in userdata)
                    {
                        b.Append(property);
                        b.Append(',');
                    }
                    b.Remove(b.Length - 1, 1);

                    temp[i] = b.ToString();
                }
            }
            File.WriteAllLines(Config.config.Path_users,temp);
        }
    }
}
