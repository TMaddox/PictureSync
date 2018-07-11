using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using HashLibrary;
using PictureSync.Properties;
using Telegram.Bot.Args;

using static PictureSync.Logic.Config;
using static PictureSync.Logic.Server;
using static PictureSync.Logic.Userlist;

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
                case "/amountactivity":
                    var b1 = new StringBuilder();
                    var list1 = GetUseractivity_Amount();
                    for (var i = 0; i < UsersAmount; i++)
                        b1.AppendLine(list1[i, 0] + " - " + list1[i, 1]);

                    Trace.WriteLine(NowLog + " " + e.Message.Chat.Username + " " + Resources.TelegramBot_AdminCommands_activity_accessed);
                    Bot.SendTextMessageAsync(e.Message.Chat.Id, b1.ToString());
                    break;
                case "/timeactivity":
                    var b3 = new StringBuilder();
                    var list = GetUseractivity_Time();
                    for (var i = 0; i < UsersAmount; i++)
                        b3.AppendLine(list[i, 0] + " - " + list[i, 1]);

                    Trace.WriteLine(NowLog + " " + e.Message.Chat.Username + " " + Resources.TelegramBot_AdminCommands_activity_accessed);
                    Bot.SendTextMessageAsync(e.Message.Chat.Id, b3.ToString());
                    break;
                case "/amountactivity_clear":
                    ResetAllAmount();
                    Bot.SendTextMessageAsync(e.Message.Chat.Id, Resources.Success);
                    break;
                case "/timeactivity_clear":
                    ResetAllActivity();
                    Bot.SendTextMessageAsync(e.Message.Chat.Id, Resources.Success);
                    break;
                case "/add_user":
                    var strings = e.Message.Text.Split(' ');
                    if (strings.Length == 2)
                    {
                        if (!AddUser(strings[1]))
                        {
                            // User already exists
                            Trace.WriteLine(NowLog + " " + e.Message.Chat.Username + " " + Resources.TelegramBot_AdminCommands_add_user_already_exists_log + " " + strings[1]);
                            Bot.SendTextMessageAsync(e.Message.Chat.Id, Resources.TelegramBot_AdminCommands_add_user_already_exists);
                            break;
                        }
                        SortUsers();

                        Trace.WriteLine(NowLog + " " + e.Message.Chat.Username + " " + Resources.TelegramBot_AdminCommands_add_user_success_log + " " + strings[1]);
                        Bot.SendTextMessageAsync(e.Message.Chat.Id, Resources.TelegramBot_AdminCommands_add_user_success);
                    }
                    else
                    {
                        Bot.SendTextMessageAsync(e.Message.Chat.Id, Resources.Error);
                    }
                    break;
                case "/log":
                    var logList = GetLogList(100);
                    var b2 = new StringBuilder();
                    foreach (var line in logList)
                        b2.AppendLine(line);

                    Trace.WriteLine(NowLog + " " + e.Message.Chat.Username + " " + Resources.TelegramBot_AdminCommands_log_accessed);
                    Bot.SendTextMessageAsync(e.Message.Chat.Id, b2.ToString());
                    break;
                //case "/pwedit":
                //    var strings = e.Message.Text.Split(' ');
                //    var hasher = new Hasher();
                //    if (hasher.Check(strings[1], new HashedPassword(Hash, Salt)))
                //    {
                //        var hashedPw = hasher.HashPassword(strings[2]);
                //        Hash = hashedPw.Hash;
                //        Salt = hashedPw.Salt;
                //        Update_Config();
                //    }
                //    break;
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
                    
                    if (HasAdminPrivilege(e.Message.Chat.Username))
                    {
                        commandsList.Add(Resources.TelegramBot_CommonCommands_help_activity_amount);
                        commandsList.Add(Resources.TelegramBot_CommonCommands_help_activity_time);
                        commandsList.Add(Resources.TelegramBot_CommonCommands_help_add_user);
                        //commandsList.Add(Resources.TelegramBot_CommonCommands_help_change_pw);
                        commandsList.Add(Resources.TelegramBot_CommonCommands_help_clear_activity);
                        commandsList.Add(Resources.TelegramBot_CommonCommands_help_clear_amount);
                        commandsList.Add(Resources.TelegramBot_CommonCommands_help_log);
                    }
                    commandsList.Add(Resources.TelegramBot_CommonCommands_help_coff);
                    commandsList.Add(Resources.TelegramBot_CommonCommands_help_con);
                    commandsList.Add(Resources.TelegramBot_CommonCommands_help_admin);
                    commandsList.Add(Resources.TelegramBot_CommonCommands_help_auth); // auth is handled in Bot_OnMessage
                    commandsList.Add(Resources.TelegramBot_CommonCommands_help_help);
                    commandsList.Sort(); // sort commands alpahbetically

                    var b = new StringBuilder();
                    b.AppendLine(Resources.TelegramBot_CommonCommands_commands);
                    foreach (var line in commandsList)
                        b.AppendLine(line);
                    Bot.SendTextMessageAsync(e.Message.Chat.Id, b.ToString());
                    break;
                case "/admin":
                    var hasher = new Hasher();
                    if (hasher.Check(e.Message.Text.Remove(0, 7), new HashedPassword(Hash, Salt)))
                    {
                        SetAdminPrivilege(e.Message.Chat.Username, true);
                        Trace.WriteLine(NowLog + " " + e.Message.Chat.Username + " " + Resources.TelegramBot_CommonCommands_admin_successful_log);
                        Bot.SendTextMessageAsync(e.Message.Chat.Id, Resources.TelegramBot_CommonCommands_admin_successful);
                    }
                    else
                    {
                        SetAdminPrivilege(e.Message.Chat.Username, false);
                        Trace.WriteLine(NowLog + " " + e.Message.Chat.Username + " " + Resources.TelegramBot_CommonCommands_admin_not_successful_log);
                        Bot.SendTextMessageAsync(e.Message.Chat.Id, Resources.TelegramBot_CommonCommands_admin_not_successful);
                    }
                    break;
                case "/coff":
                    SetCompression(e.Message.Chat.Username, false);
                    Trace.WriteLine(NowLog + " " + e.Message.Chat.Username + " " + Resources.TelegramBot_CommonCommands_coff_log);
                    Bot.SendTextMessageAsync(e.Message.Chat.Id, Resources.TelegramBot_CommonCommands_coff);
                    break;
                case "/con":
                    SetCompression(e.Message.Chat.Username, true);
                    Trace.WriteLine(NowLog + " " + e.Message.Chat.Username + " " + Resources.TelegramBot_CommonCommands_con_log);
                    Bot.SendTextMessageAsync(e.Message.Chat.Id, Resources.TelegramBot_CommonCommands_con);
                    break;
                case "/start":
                    Bot.SendTextMessageAsync(e.Message.Chat.Id, Resources.TelegramBot_CommonCommands_start);
                    break;
                default:
                    Trace.WriteLine(NowLog + " " + Resources.TelegramBot_CommonCommands_Note_log + " " + e.Message.Text);
                    Bot.SendTextMessageAsync(e.Message.Chat.Id, Resources.TelegramBot_CommonCommands_nocommand);
                    break;
            }
        }
    }
}