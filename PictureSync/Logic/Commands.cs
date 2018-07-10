using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using HashLibrary;
using PictureSync.Properties;
using Telegram.Bot.Args;
using static PictureSync.Logic.Config;

namespace PictureSync.Logic
{
    internal static class Commands
    {
        /// <summary>
        /// Parses Commands sent via a user
        /// </summary>
        /// <param name="e"></param>
        public static void ParseCommands(MessageEventArgs e)
        {
            var temp = e.Message.Text.Split(' ');
            var command = temp[0];

            if (Userlist.HasAdminPrivilege(e.Message.Chat.Username))
                AdminCommands(e, command);
            else
                CommonCommands(e, command);
        }

        /// <summary>
        /// executes admin commands
        /// </summary>
        /// <param name="e"></param>
        /// <param name="command"></param>
        private static void AdminCommands(MessageEventArgs e, string command)
        {
            // ADMIN AREA
            switch (command)
            {
                case "/activity_amount":
                    var b1 = new StringBuilder();
                    var list1 = Userlist.GetUseractivity_Amount();
                    for (var i = 0; i < Userlist.UsersAmount; i++)
                    {
                        b1.AppendLine(list1[i, 0] + " - " + list1[i, 1]);
                    }
                    Trace.WriteLine(Server.NowLog + " " + e.Message.Chat.Username + " " + Resources.TelegramBot_AdminCommands_activity_accessed);
                    Bot.SendTextMessageAsync(e.Message.Chat.Id, b1.ToString());
                    break;
                case "/activity_time":
                    var b = new StringBuilder();
                    var list = Userlist.GetUseractivity_Time();
                    for (var i = 0; i < Userlist.UsersAmount; i++)
                    {
                        b.AppendLine(list[i, 0] + " - " + list[i, 1]);
                    }
                    Trace.WriteLine(Server.NowLog + " " + e.Message.Chat.Username + " " + Resources.TelegramBot_AdminCommands_activity_accessed);
                    Bot.SendTextMessageAsync(e.Message.Chat.Id, b.ToString());
                    break;
                case "/party":
                    Bot.SendTextMessageAsync(e.Message.Chat.Id, Resources.TelegramBot_AdminCommands_party);
                    break;
                default:
                    //Admin can of course execute normal commands too
                    CommonCommands(e, command);
                    break;
            }
        }

        /// <summary>
        /// executes normal commands
        /// </summary>
        /// <param name="e"></param>
        /// <param name="command"></param>
        private static void CommonCommands(MessageEventArgs e, string command)
        {
            // NORMAL AREA
            switch (command)
            {
                case "/help":
                    var commandsList = new List<string>();
                    
                    if (Userlist.HasAdminPrivilege(e.Message.Chat.Username))
                    {
                        commandsList.Add(Resources.TelegramBot_CommonCommands_help_activity_amount);
                        commandsList.Add(Resources.TelegramBot_CommonCommands_help_activity_time);
                    }
                    commandsList.Add(Resources.TelegramBot_CommonCommands_help_coff);
                    commandsList.Add(Resources.TelegramBot_CommonCommands_help_con);
                    commandsList.Add(Resources.TelegramBot_CommonCommands_help_admin);
                    commandsList.Add(Resources.TelegramBot_CommonCommands_help_auth); // auth is handled in Bot_OnMessage
                    commandsList.Add(Resources.TelegramBot_CommonCommands_help_help);
                    commandsList.Sort();

                    var b = new StringBuilder();
                    b.AppendLine(Resources.TelegramBot_CommonCommands_commands);
                    foreach (var line in commandsList)
                        b.AppendLine(line);
                    Bot.SendTextMessageAsync(e.Message.Chat.Id, b.ToString());
                    break;
                case "/admin":
                    var hasher = new Hasher();
                    if (hasher.Check(e.Message.Text.Remove(0, 7), new HashedPassword(Config.Hash, Config.Salt)))
                    {
                        Userlist.SetAdminPrivilege(e.Message.Chat.Username, true);
                        Trace.WriteLine(
                            Server.NowLog + " " + e.Message.Chat.Username + " " + Resources.TelegramBot_CommonCommands_admin_successful_log);
                        Bot.SendTextMessageAsync(e.Message.Chat.Id, Resources.TelegramBot_CommonCommands_admin_successful);
                    }
                    else
                    {
                        Userlist.SetAdminPrivilege(e.Message.Chat.Username, false);
                        Trace.WriteLine(Server.NowLog + " " + e.Message.Chat.Username +
                                        " " + Resources.TelegramBot_CommonCommands_admin_not_successful_log);
                        Bot.SendTextMessageAsync(e.Message.Chat.Id,
                            Resources.TelegramBot_CommonCommands_admin_not_successful);
                    }
                    break;
                case "/coff":
                    Userlist.SetCompression(e.Message.Chat.Username, false);
                    Trace.WriteLine(Server.NowLog + " " + e.Message.Chat.Username + " " + Resources.TelegramBot_CommonCommands_coff_log);
                    Bot.SendTextMessageAsync(e.Message.Chat.Id, Resources.TelegramBot_CommonCommands_coff);
                    break;
                case "/con":
                    Userlist.SetCompression(e.Message.Chat.Username, true);
                    Trace.WriteLine(Server.NowLog + " " + e.Message.Chat.Username + " " + Resources.TelegramBot_CommonCommands_con_log);
                    Bot.SendTextMessageAsync(e.Message.Chat.Id, Resources.TelegramBot_CommonCommands_con);
                    break;
                default:
                    Trace.WriteLine(Server.NowLog + " " + Resources.TelegramBot_CommonCommands_Note_log + " " + e.Message.Text);
                    Bot.SendTextMessageAsync(e.Message.Chat.Id, Resources.TelegramBot_CommonCommands_nocommand);
                    break;
            }
        }
    }
}