using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PictureSync.Logic
{
    class Config
    {
        public static Config config;

        private string token;
        public string Token
        {
            get { return token; }
            set { token = value; }
        }

        private string path_root;
        public string Path_root
        {
            get { return path_root; }
            set { path_root = value; }
        }

        public string Path_photos
        {
            get { return path_root + @"pic\"; }
        }

        public string Path_log
        {
            get { return path_root + @"log.txt"; }
        }

        public string Path_users
        {
            get { return path_root + @"users.dat"; }
        }

        public string Path_config
        {
            get { return path_root + @"config.dat"; }
        }

        private string auth_key;
        public string Auth_key
        {
            get { return auth_key; }
            set { auth_key = value; }
        }
    }
}
