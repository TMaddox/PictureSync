using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using System.Drawing;
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Reflection;

namespace PictureSync
{
    public class Program
    {
        static string basedir;
        private static NotifyIcon TrayIcon = new NotifyIcon();

        [DllImport("kernel32.dll", ExactSpelling = true)]
        private static extern IntPtr GetConsoleWindow();
        private static IntPtr ThisConsole = GetConsoleWindow();

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        private static Int32 showWindow = 0; //0 - SW_HIDE - Hides the window and activates another window.

        static void Main(string[] args)
        {
            Assembly currentAssembly = Assembly.GetExecutingAssembly();
            TrayIcon.Icon = Properties.Resources.icon;
            TrayIcon.MouseDoubleClick += new MouseEventHandler(TrayIcon_DoubleClick);
            TrayIcon.ContextMenuStrip = new ContextMenuStrip();
            TrayIcon.ContextMenuStrip.Items.AddRange(new ToolStripItem[] { new ToolStripMenuItem() });
            TrayIcon.ContextMenuStrip.Items[0].Text = "Exit";
            TrayIcon.ContextMenuStrip.Items[0].Click += new EventHandler(smoothExit);
            TrayIcon.Visible = true;

            Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\PictureSync\");
            basedir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\PictureSync\";

            //var exitEvent = new ManualResetEvent(false);
            //Console.CancelKeyPress += (sender, eventArgs) => {
            //    eventArgs.Cancel = true;
            //    exitEvent.Set();
            //};

            Logic.Server serverlogic = new Logic.Server();
            serverlogic.Create_Config(basedir);

            // Fill config object with configs from file
            serverlogic.ReadConfig(basedir);

            // Create files (log, Users)
            serverlogic.Create_files();

            // Create global telebot
            Logic.Telegram_Bot.telebot = new Logic.Telegram_Bot();

            // Initiate Logging, if a WriteLine shall be included in the log, use Tracer.Writeline instead of Console.Writeline
            serverlogic.InitiateTracer();

            // Start bot
            Logic.Telegram_Bot.telebot.Start_bot();
            Trace.WriteLine(serverlogic.NowLog + " Bot started");

            Application.Run();
        }

        private static void TrayIcon_DoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
            {
                //reserve right click for context menu
                showWindow = ++showWindow % 2;
                ShowWindow(ThisConsole, showWindow);
            }
        }

        private static void smoothExit(object sender, EventArgs e)
        {
            Logic.Server serverlogic = new Logic.Server();
            Logic.Telegram_Bot.telebot.Stop_bot();
            Trace.WriteLine(serverlogic.NowLog + " Bot stopped");

            TrayIcon.Visible = false;
            Application.Exit();
            Environment.Exit(1);
        }
    }
}
