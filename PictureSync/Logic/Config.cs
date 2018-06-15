namespace PictureSync.Logic
{
    class Config
    {
        // global static object because only one config is needed
        public static Config config;

        public string Token { get; set; }

        public string PathRoot { get; set; }

        public string PathPhotos { get; set; }

        public string PathLog => PathRoot + @"log.txt";

        public string PathUsers => PathRoot + @"users.dat";

        public string PathConfig => PathRoot + @"config.dat";

        public string Hash { get; set; }

        public string Salt { get; set; }

        public int Msg_Increment { get; set; }

        // Max lenght of a saved picture
        public int Max_len { get; set; }

        // Quality of the jpg encoder (1-100)
        public int EncodeQ { get; set; }
    }
}
