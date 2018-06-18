using System;
using System.IO;
using System.Diagnostics;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Reflection;
using static PictureSync.Logic.Server;
using static PictureSync.Logic.TelegramBot;

namespace PictureSync
{
    public class Program
    {

        private static string _basedir;
        private static readonly NotifyIcon TrayIcon = new NotifyIcon();

        [DllImport("kernel32.dll", ExactSpelling = true)]
        private static extern IntPtr GetConsoleWindow();
        private static readonly IntPtr ThisConsole = GetConsoleWindow();

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        private static int _showWindow = 0; //0 - SW_HIDE - Hides the window and activates another window.

        private static void Main(string[] args)
        {
            Assembly.GetExecutingAssembly();
            TrayIcon.Icon = Properties.Resources.icon;
            TrayIcon.MouseDoubleClick += TrayIcon_DoubleClick;
            TrayIcon.ContextMenuStrip = new ContextMenuStrip();
            TrayIcon.ContextMenuStrip.Items.AddRange(new ToolStripItem[] { new ToolStripMenuItem(), new ToolStripMenuItem() });
            TrayIcon.ContextMenuStrip.Items[0].Text = "Exit";
            TrayIcon.ContextMenuStrip.Items[0].Click += SmoothExit;
            TrayIcon.ContextMenuStrip.Items[1].Text = "Hide";
            TrayIcon.ContextMenuStrip.Items[1].Click += Hide_Show_Click;
            TrayIcon.Visible = true;

            // on ctrl-c
            Console.CancelKeyPress += SmoothExit;

            Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\PictureSync\");
            _basedir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\PictureSync\";

            ReadConfig(_basedir);

            // Create files (log, Users)
            Create_files();

            // Create global telebot
            try
            {
                Telebot = new Logic.TelegramBot();
            }
            catch (Exception)
            {
                Create_Config(_basedir);
            }
            

            // Initiate Logging, if a WriteLine shall be included in the log, use Tracer.Writeline instead of Console.Writeline
            InitiateTracer();

            // Start bot
            Telebot.Start_bot();
            Trace.WriteLine(NowLog + " Bot started");

            Application.Run();
        }

        /// <summary>
        /// Hides and shows the CLI
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void Hide_Show_Click(object sender, EventArgs e)
        {
            _showWindow = ++_showWindow % 2;
            ShowWindow(ThisConsole, _showWindow);

            if (_showWindow == 1)
                TrayIcon.ContextMenuStrip.Items[1].Text = "Hide";
            else
                TrayIcon.ContextMenuStrip.Items[1].Text = "Show";
        }

        /// <summary>
        /// Handles a double click on the tray icon
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void TrayIcon_DoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
            {
                //reserve right click for context menu
                _showWindow = ++_showWindow % 2;
                ShowWindow(ThisConsole, _showWindow);

                if (_showWindow == 1)
                    TrayIcon.ContextMenuStrip.Items[1].Text = "Hide";
                else
                    TrayIcon.ContextMenuStrip.Items[1].Text = "Show";
            }
        }

        /// <summary>
        /// Performs a smooth exit
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void SmoothExit(object sender, EventArgs e)
        {
            Telebot.Stop_bot();
            Trace.WriteLine(NowLog + " Bot stopped");

            TrayIcon.Visible = false;
            Application.Exit();
            Environment.Exit(1);
        }
    }
}
