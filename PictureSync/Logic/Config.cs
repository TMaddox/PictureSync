using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PictureSync.Logic
{
    class Config
    {
        // global static object because only one config is needed
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

        private string path_photos;
        public string Path_photos
        {
            get { return path_photos; }
            set { path_photos = value; }
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

        private string hash;
        public string Hash
        {
            get { return hash; }
            set { hash = value; }
        }

        private string salt;
        public string Salt
        {
            get { return salt; }
            set { salt = value; }
        }

        private int msg_increment;
        public int Msg_Increment
        {
            get { return msg_increment; }
            set { msg_increment = value; }
        }

        // Max lenght of a saved picture
        private int max_len;
        public int Max_len
        {
            get { return max_len; }
            set { max_len = value; }
        }

        // Quality of the jpg encoder (1-100)
        private int encodeQ;
        public int EncodeQ
        {
            get { return encodeQ; }
            set { encodeQ = value; }
        }
    }
}
