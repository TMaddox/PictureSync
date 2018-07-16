using Telegram.Bot;

namespace PictureSync.Logic
{
    internal static class Config
    {
        /// <summary>
        /// Telegram bot token, is obtained from Botfather
        /// </summary>
        public static string Token { get; set; }

        /// <summary>
        /// Global TelegramBotClient, so it can be accessed in different classes, even if they are static
        /// </summary>
        public static TelegramBotClient Bot { get; set; }

        /// <summary>
        /// Root path of config files
        /// </summary>
        public static string PathRoot { private get; set; }

        /// <summary>
        /// Path where photos will be stored
        /// </summary>
        public static string PathPhotos { get; set; }

        /// <summary>
        /// Path for the logfile
        /// </summary>
        public static string PathLog => PathRoot + @"log.txt";

        /// <summary>
        /// Path for the User file
        /// </summary>
        public static string PathUsers => PathRoot + @"users.dat";

        /// <summary>
        /// Path for the config file
        /// </summary>
        public static string PathConfig => PathRoot + @"config.dat";

        /// <summary>
        /// The Hash of the Admin PW
        /// </summary>
        public static string Hash { get; set; }
 
        /// <summary>
        /// Salt of the Admin PW
        /// </summary>
        public static string Salt { get; set; }

        /// <summary>
        /// Because download of Msg runs async, Messages need a unique id to track them in the log
        /// </summary>
        public static int MsgIncrement { get; set; }

        /// <summary>
        /// Max lenght of a saved picture
        /// </summary>
        public static int MaxLen { get; set; }

        /// <summary>
        /// Quality of the jpg encoder (1-100)
        /// </summary>
        public static int EncodeQ { get; set; }

        /// <summary>
        /// Language
        /// </summary>
        public static string Localization { get; set; }
    }
}
