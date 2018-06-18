namespace PictureSync.Logic
{
    class Config
    {
        /// <summary>
        /// global static object because only one config is needed
        /// </summary>
        public static Config config;

        /// <summary>
        /// Telegram bot token, is obtained from Botfather
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// Root path of config files
        /// </summary>
        public string PathRoot { get; set; }

        /// <summary>
        /// Path where photos will be stored
        /// </summary>
        public string PathPhotos { get; set; }

        /// <summary>
        /// Path for the logfile
        /// </summary>
        public string PathLog => PathRoot + @"log.txt";

        /// <summary>
        /// Path for the User file
        /// </summary>
        public string PathUsers => PathRoot + @"users.dat";

        /// <summary>
        /// Path for the config file
        /// </summary>
        public string PathConfig => PathRoot + @"config.dat";

        /// <summary>
        /// The Hash of the Admin PW
        /// </summary>
        public string Hash { get; set; }
 
        /// <summary>
        /// Salt of the Admin PW
        /// </summary>
        public string Salt { get; set; }

        /// <summary>
        /// Because download of Msg runs async, Messages need a unique id to track them in the log
        /// </summary>
        public int MsgIncrement { get; set; }

        /// <summary>
        /// Max lenght of a saved picture
        /// </summary>
        public int MaxLen { get; set; }

        /// <summary>
        /// Quality of the jpg encoder (1-100)
        /// </summary>
        public int EncodeQ { get; set; }

        /// <summary>
        /// Language
        /// </summary>
        public string Localization { get; set; }
    }
}
