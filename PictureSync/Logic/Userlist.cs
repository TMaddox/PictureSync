using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PictureSync.Logic
{
    static class Userlist
    {
        private static List<string> users = new List<string>();
        private static List<bool> compression = new List<bool>();

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
        /// check if user is authorized
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public static bool HasAuth(string username)
        {
            return Users.Contains(username);
        }
    }
}
