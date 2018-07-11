using System;
using System.IO;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Reflection;
using System.ServiceProcess;
using System.Threading;
using PictureSync.Logic;
using PictureSync.Properties;
using static PictureSync.Logic.Config;
using static PictureSync.Logic.Server;
using static PictureSync.Logic.TelegramBot;

namespace PictureSync
{
    public class Program
    {
        #region Nested class to support running as service
        public const string ServiceName = "PictureSyncService";

        public class Service : ServiceBase
        {
            public Service()
            {
                ServiceName = Program.ServiceName;
            }

            protected override void OnStart(string[] args)
            {
                Program.Start(args);
            }

            protected override void OnStop()
            {
                Program.Stop();
            }
        }
        #endregion

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
            if (!Environment.UserInteractive)
            {
                // running as service
                using (var service = new Service())
                    ServiceBase.Run(service);
            }
            else
            {
                // running as console app
                // Assembly.GetExecutingAssembly();
                TrayIcon.Icon = Resources.icon;
                TrayIcon.MouseDoubleClick += TrayIcon_DoubleClick;
                TrayIcon.ContextMenuStrip = new ContextMenuStrip();
                TrayIcon.ContextMenuStrip.Items.AddRange(new ToolStripItem[] { new ToolStripMenuItem(), new ToolStripMenuItem() });
                TrayIcon.ContextMenuStrip.Items[0].Text = Resources.Program_Main_Exit;
                TrayIcon.ContextMenuStrip.Items[0].Click += UserClosedApp;
                TrayIcon.ContextMenuStrip.Items[1].Text = Resources.Program_Main_Hide;
                TrayIcon.ContextMenuStrip.Items[1].Click += Hide_Show_Click;
                TrayIcon.Visible = true;

                // on ctrl-c
                Console.CancelKeyPress += UserClosedApp;
                Start(args);
            }
        }

        /// <summary>
        /// Starts the Bot
        /// </summary>
        private static void Start(string[] args)
        {
            Directory.CreateDirectory(GetApplicationPath() + @"\files\");
            _basedir = GetApplicationPath() + @"\files\";

            ReadConfig(_basedir);

            var culture = CultureInfo.CreateSpecificCulture(Localization);
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;

            // Create files (log, Users)
            Create_files();
            SortUsers();

            // Create global telebot
            try
            {
                Telebot = new TelegramBot();
            }
            catch (Exception)
            {
                Create_Config(_basedir);
            }


            // Initiate Logging, if a WriteLine shall be included in the log, use Tracer.Writeline instead of Console.Writeline
            InitiateTracer();

            // Start bot
            Start_bot();
            Trace.WriteLine(NowLog + " " + Resources.Program_Main_Bot_started_log);

            Application.Run();
        }
        /// <summary>
        /// Performs a smooth exit
        /// </summary>
        private static void Stop()
        {
            Stop_bot();
            Trace.WriteLine(NowLog + " " + Resources.Program_Main_Bot_stopped_log);

            TrayIcon.Visible = false;
            Application.Exit();
            Environment.Exit(1);
        }

        /// <summary>
        /// Gets the path of the Application
        /// </summary>
        private static string GetApplicationPath()
        {
            var assemblyPath = Assembly.GetEntryAssembly().CodeBase;
            assemblyPath = new Uri(assemblyPath).LocalPath;
            assemblyPath = Path.GetDirectoryName(assemblyPath);
            return assemblyPath;
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
                TrayIcon.ContextMenuStrip.Items[1].Text = Resources.Program_Main_Hide;
            else
                TrayIcon.ContextMenuStrip.Items[1].Text = Resources.Program_Main_Show;
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
                    TrayIcon.ContextMenuStrip.Items[1].Text = Resources.Program_Main_Hide;
                else
                    TrayIcon.ContextMenuStrip.Items[1].Text = Resources.Program_Main_Show;
            }
        }

        /// <summary>
        /// The user closed the application
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void UserClosedApp(object sender, EventArgs e)
        {
            Stop();
        }
    }
}
