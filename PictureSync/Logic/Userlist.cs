using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using static PictureSync.Logic.Config;

namespace PictureSync.Logic
{
    internal static class Userlist
    {
        private static List<string> _users = new List<string>();

        /// <summary>
        /// Returns a List of strings with users
        /// </summary>
        private static List<string> Users
        {
            get
            {
                _users = File.ReadAllLines(PathUsers).ToList();
                return _users;
            }
        }

        /// <summary>
        /// Returns n of Uers
        /// </summary>
        public static int UsersAmount => Users.Count;

        /// <summary>
        /// Add a new user
        /// </summary>
        /// <param name="username">Username of the user to be added</param>
        /// <returns>true if new user was created, false if it already existed</returns>
        public static bool AddUser(string username)
        {
            if (CheckIfUserExists(username)) return false;

            File.AppendAllText(PathUsers, username + ",1,0,0," + DateTime.Today.ToString("yyyy-MM-dd") + Environment.NewLine);
            return true;
        }

        /// <summary>
        /// Checks if a username exists already
        /// </summary>
        /// <returns>Returns true if username exists</returns>
        public static bool CheckIfUserExists(string username)
        {
            foreach (var user in Users)
            {
                var userdata = user.Split(',');
                if (userdata[0] == username)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Checks if images shall be comressed for user
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public static bool HasCompression(string username)
        {
            foreach (var user in Users)
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
            var statechanged = false;

            foreach (var user in Users)
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
        /// <returns>if user has admin privileges</returns>
        public static bool HasAdminPrivilege(string username)
        {
            foreach (var user in Users)
            {
                var userdata = user.Split(',');
                if (userdata[0] == username && userdata[2] == "1")
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Sets admin privileges (yes|no) for user, returns true if state was changed
        /// </summary>
        /// <param name="username"></param>
        /// <param name="adminprivilege"></param>
        /// <returns>if admin state was changed</returns>
        public static bool SetAdminPrivilege(string username, bool adminprivilege)
        {
            var statechanged = false;

            foreach (var user in Users)
            {
                var userdata = user.Split(',');
                if (userdata[0] == username)
                {
                    if (userdata[2] != Convert.ToString(Convert.ToInt32(adminprivilege)))
                        statechanged = true;
                    userdata[2] = Convert.ToInt32(adminprivilege).ToString();
                    WriteUserdata(userdata);
                }
            }
            return statechanged;
        }

        /// <summary>
        /// Returns the amount of pictures sent in a given date range
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        private static int GetPictureAmount(string username)
        {
            foreach (var user in Users)
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
            foreach (var user in Users)
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
        /// Resets all amounts of sent pictures for all users
        /// </summary>
        public static void ResetAllAmount()
        {
            foreach (var user in Users)
            {
                var userdata = user.Split(',');
                userdata[3] = Convert.ToString(0);
                WriteUserdata(userdata);
            }
        }

        /// <summary>
        /// Gets latest activity per user
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        private static DateTime GetLatestActivity(string username)
        {
            foreach (var user in Users)
            {
                var userdata = user.Split(',');
                if (userdata[0] == username)
                    return Convert.ToDateTime(userdata[4]);
            }
            return DateTime.MinValue;
        }

        /// <summary>
        /// sets latest acitvity per user
        /// </summary>
        /// <param name="username"></param>
        /// <param name="date"></param>
        public static void SetLatestActivity(string username, DateTime date)
        {
            foreach (var user in Users)
            {
                var userdata = user.Split(',');
                if (userdata[0] == username)
                {
                    userdata[4] = date.ToString("yyyy-MM-dd"); 
                    WriteUserdata(userdata);
                }
            }
        }

        /// <summary>
        /// Resets All activities stored for users
        /// </summary>
        public static void ResetAllActivity()
        {
            foreach (var user in Users)
            {
                var userdata = user.Split(',');
                userdata[4] = "2000-01-01";
                WriteUserdata(userdata);
            }
        }

        /// <summary>
        /// check if user is authorized to interact with the bot
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public static bool HasAuth(string username)
        {
            foreach (var user in Users)
            {
                var userdata = user.Split(',');
                if (userdata[0] == username)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Refreshes user.dat
        /// </summary>
        /// <param name="userdata"></param>
        private static void WriteUserdata(string[] userdata)
        {
            var temp = File.ReadAllLines(PathUsers);
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
            File.WriteAllLines(PathUsers,temp);
        }

        /// <summary>
        /// Gets total sent pictures
        /// </summary>
        /// <returns>Array [username, picturecount]</returns>
        public static string[,] GetUseractivity_Amount()
        {
            var temp = Users;
            var userName = new string[temp.Count];
            var userActivity = new int[temp.Count];
            var final = new string[temp.Count, 2];

            for (var i = 0; i < temp.Count; i++)
            {
                var userdata = temp[i].Split(',');
                userName[i] = userdata[0];
                userActivity[i] = GetPictureAmount(userName[i]);
            }

            Array.Sort(userActivity,userName);
            Array.Reverse(userName);
            Array.Reverse(userActivity);

            for (var i = 0; i < temp.Count; i++)
            {
                final[i, 0] = userName[i];
                final[i, 1] = userActivity[i].ToString();
            }

            return final;
        }

        /// <summary>
        /// Gets date of latest activity (picture received)
        /// </summary>
        /// <returns>Array [username, date_lastpic]</returns>
        public static string[,] GetUseractivity_Time()
        {
            var temp = Users;
            var userName = new string[temp.Count];
            var userActivity = new DateTime[temp.Count];
            var final = new string[temp.Count, 2];

            for (var i = 0; i < temp.Count; i++)
            {
                var userdata = temp[i].Split(',');
                userName[i] = userdata[0];
                userActivity[i] = GetLatestActivity(userName[i]);
            }

            Array.Sort(userActivity, userName);
            Array.Reverse(userName);
            Array.Reverse(userActivity);

            for (var i = 0; i < temp.Count; i++)
            {
                final[i, 0] = userName[i];
                final[i, 1] = userActivity[i].ToString("dd.MM.yyyy");
            }

            return final;
        }
    }
}
