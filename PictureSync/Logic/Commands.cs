﻿using System;
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

            if (HasAdminPrivilege(e.Message.Chat.Username))
                AdminCommands(e, command);
            else
                CommonCommands(e, command);
        }

        /// <summary>
        /// Outputs the result to the log and the user
        /// </summary>
        /// <param name="e">Message Args</param>
        /// <param name="log">What to write to the log</param>
        /// <param name="user">What to tell the user</param>
        private static void OutputResult(MessageEventArgs e, string log = "", string user = "")
        {
            if (log != "")
                Trace.WriteLine(log);
            if (user != "")
                Bot.SendTextMessageAsync(e.Message.Chat.Id, user);
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
                    AmountActivityCommand(e);
                    break;
                case "/timeactivity":
                    TimeActivityCommand(e);
                    break;
                case "/amountactivity_clear":
                    AmountActivityClearCommand(e);
                    break;
                case "/timeactivity_clear":
                    TimeActivityClearCommand(e);
                    break;
                case "/add_user":
                    AddUserCommand(e);
                    break;
                case "/log":
                    LogCommand(e);
                    break;
                case "/pwedit":
                    //PwEditCommand(e);
                    break;
                case "/party":
                    PartyCommand(e);
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
                    HelpCommand(e);
                    break;
                case "/admin":
                    GetAdminCommand(e);
                    break;
                case "/coff":
                    CompressionOffCommand(e);
                    break;
                case "/con":
                    CompressionOnCommand(e);
                    break;
                case "/start":
                    StartCommand(e);
                    break;
                default:
                    EmptyCommand(e);
                    break;
            }
        }

        /// <summary>
        /// Happy Easter
        /// </summary>
        private static void PartyCommand(MessageEventArgs e)
        {
            Bot.SendTextMessageAsync(e.Message.Chat.Id, Resources.TelegramBot_AdminCommands_party);
        }

        /// <summary>
        /// Changes the Password
        /// </summary>
        private static void PwEditCommand(MessageEventArgs e)
        {
            var strings = e.Message.Text.Split(' ');
            var hasher = new Hasher();
            if (hasher.Check(strings[1], new HashedPassword(Hash, Salt)))
            {
                var hashedPw = hasher.HashPassword(strings[2]);
                Hash = hashedPw.Hash;
                Salt = hashedPw.Salt;
                Update_Config();
            }
        }

        /// <summary>
        /// Returns the last 100 lines of the log
        /// </summary>
        private static void LogCommand(MessageEventArgs e)
        {
            var logList = GetLogList(100);
            var b = new StringBuilder();
            foreach (var line in logList)
                b.AppendLine(line);

            OutputResult(e, NowLog + " " + e.Message.Chat.Username + " " + Resources.TelegramBot_AdminCommands_log_accessed, b.ToString());
        }

        /// <summary>
        /// Adds a new user
        /// </summary>
        private static void AddUserCommand(MessageEventArgs e)
        {
            var strings = e.Message.Text.Split(' ');
            if (strings.Length == 2)
            {
                if (!AddUser(strings[1]))
                {
                    // User already exists
                    OutputResult(e, NowLog + " " + e.Message.Chat.Username + " " + Resources.TelegramBot_AdminCommands_add_user_already_exists_log + " " + strings[1], Resources.TelegramBot_AdminCommands_add_user_already_exists);
                    return;
                }
                SortUsers();

                OutputResult(e, NowLog + " " + e.Message.Chat.Username + " " + Resources.TelegramBot_AdminCommands_add_user_success_log + " " + strings[1], Resources.TelegramBot_AdminCommands_add_user_success);
            }
            else
            {
                OutputResult(e, "", Resources.Error);
            }
        }

        /// <summary>
        /// Clears stored dates of when the user has sent the last picture
        /// </summary>
        private static void TimeActivityClearCommand(MessageEventArgs e)
        {
            ResetAllActivity();
            OutputResult(e, NowLog + " " + e.Message.Chat.Username + " " + Resources.TelegramBot_AdminCommands_timeactivity_cleared_log, Resources.Success);
        }

        /// <summary>
        /// Clears the picture counter of the users
        /// </summary>
        private static void AmountActivityClearCommand(MessageEventArgs e)
        {
            ResetAllAmount();
            OutputResult(e, NowLog + " " + e.Message.Chat.Username + " " + Resources.TelegramBot_AdminCommands_amountactivity_cleared_log, Resources.Success);
        }

        /// <summary>
        /// Displays the dates of when the user has sent the last picture
        /// </summary>
        private static void TimeActivityCommand(MessageEventArgs e)
        {
            var b = new StringBuilder();
            var list = GetUseractivity_Time();
            for (var i = 0; i < UsersAmount; i++)
                b.AppendLine(list[i, 0] + " - " + list[i, 1]);

            OutputResult(e, NowLog + " " + e.Message.Chat.Username + " " + Resources.TelegramBot_AdminCommands_activity_accessed, b.ToString());
        }

        /// <summary>
        /// Displays the picture counter of the users
        /// </summary>
        private static void AmountActivityCommand(MessageEventArgs e)
        {
            var b = new StringBuilder();
            var list1 = GetUseractivity_Amount();
            for (var i = 0; i < UsersAmount; i++)
                b.AppendLine(list1[i, 0] + " - " + list1[i, 1]);

            OutputResult(e, NowLog + " " + e.Message.Chat.Username + " " + Resources.TelegramBot_AdminCommands_activity_accessed, b.ToString());
        }
        
        /// <summary>
        /// User entered a non existent command
        /// </summary>
        private static void EmptyCommand(MessageEventArgs e)
        {
            OutputResult(e, NowLog + " " + Resources.TelegramBot_CommonCommands_Note_log + " " + e.Message.Text, Resources.TelegramBot_CommonCommands_nocommand);
        }

        /// <summary>
        /// respond to the common /start command telegram uses to initiate first contact with a bot
        /// </summary>
        private static void StartCommand(MessageEventArgs e)
        {
            OutputResult(e, Resources.TelegramBot_CommonCommands_start);
        }

        /// <summary>
        /// Turns comression ON
        /// </summary>
        private static void CompressionOnCommand(MessageEventArgs e)
        {
            SetCompression(e.Message.Chat.Username, true);

            OutputResult(e, NowLog + " " + e.Message.Chat.Username + " " + Resources.TelegramBot_CommonCommands_con_log, Resources.TelegramBot_CommonCommands_con);
        }

        /// <summary>
        /// Turns the compression OFF for the next picture
        /// </summary>
        private static void CompressionOffCommand(MessageEventArgs e)
        {
            SetCompression(e.Message.Chat.Username, false);

            OutputResult(e, NowLog + " " + e.Message.Chat.Username + " " + Resources.TelegramBot_CommonCommands_coff_log, Resources.TelegramBot_CommonCommands_coff);
        }

        /// <summary>
        /// Elevates user from "User" to "Admin"
        /// </summary>
        private static void GetAdminCommand(MessageEventArgs e)
        {
            var hasher = new Hasher();
            if (hasher.Check(e.Message.Text.Remove(0, 7), new HashedPassword(Hash, Salt)))
            {
                SetAdminPrivilege(e.Message.Chat.Username, true);
                OutputResult(e, NowLog + " " + e.Message.Chat.Username + " " + Resources.TelegramBot_CommonCommands_admin_successful_log, Resources.TelegramBot_CommonCommands_admin_successful);
            }
            else
            {
                SetAdminPrivilege(e.Message.Chat.Username, false);
                OutputResult(e, NowLog + " " + e.Message.Chat.Username + " " + Resources.TelegramBot_CommonCommands_admin_not_successful_log, Resources.TelegramBot_CommonCommands_admin_not_successful);
            }
        }

        /// <summary>
        /// Displays a help page, where all commands are Explained
        /// </summary>
        private static void HelpCommand(MessageEventArgs e)
        {
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

            OutputResult(e, "", b.ToString());
        }
    }
}