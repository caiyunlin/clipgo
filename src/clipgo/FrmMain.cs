using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

using clipgo.Helper;

namespace clipgo
{

    public partial class FrmMain : Form
    {
        private static object syncLock = new object();
        private bool menuExit = false;

        private List<MatchItem> matchItems = new List<MatchItem>();

        public FrmMain(bool isConsole, string workingDir, string configFile)
        {
            ConfigHelper.WorkingDir = workingDir;
            ConfigHelper.ConfigFile = configFile;
            ConfigHelper.IsConsole = isConsole;

            InitializeComponent();
        }

        private void FrmMain_Load(object sender, EventArgs e)
        {
            InitConfig();
            InitForm();
            InitWebServer();
        }

        private void InitConfig()
        {
            ConfigHelper.LoadXmlConfig();
        }

        private void InitForm()
        {
            if (!ConfigHelper.IsConsole)
            {
                ShowNotification();
            }

            AddLog("Loading " + ConfigHelper.AppName + " ...");
            AddLog("Working Directory is " + ConfigHelper.WorkingDir);
            AddLog("Config file is " + ConfigHelper.ConfigFile);

            // register matchItems
            int index = 1;
            XmlNodeList nodes = ConfigHelper.XmlDoc.SelectNodes("/config/clipboard/match");
            foreach (XmlNode node in nodes)
            {
                MatchItem matchItem = new MatchItem(node);
                matchItem.Index = index;
                matchItems.Add(matchItem);
                index++;
            }
            foreach (MatchItem matchItem in matchItems)
            {
                if (matchItem.Type == MatchType.Text)
                {
                    AddLog(string.Format("Start monitoring clipboard text : {0}", matchItem.Params));
                }
            }

            //Place window in the system-maintained clipboard format listener list
            ClipboardHelper.AddClipboardFormatListener(Handle);

        }


        private void InitWebServer()
        {
            Thread thread1 = new Thread(new ThreadStart(WebServer.StartListener));
            thread1.Start();
        }



        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            if (m.Msg == ClipboardHelper.WM_CLIPBOARDUPDATE)
            {
                Console.WriteLine("deteced clipboard change1 : {0}", ActionHelper.StopProcessClipboard);
                //Ignore Auto Clipboard Set
                if (ActionHelper.StopProcessClipboard)
                {
                    return;
                }

                Console.WriteLine("deteced clipboard change2");

                //Get the date and time for the current moment expressed as coordinated universal time (UTC).
                //DateTime saveUtcNow = DateTime.UtcNow;
                //Console.WriteLine("Copy event detected at {0} (UTC)!", saveUtcNow);
                //Write to stdout active window
                IntPtr active_window = ClipboardHelper.GetForegroundWindow();
                int length = ClipboardHelper.GetWindowTextLength(active_window);
                StringBuilder sb = new StringBuilder(length + 1);
                ClipboardHelper.GetWindowText(active_window, sb, sb.Capacity);
                string windowText = sb.ToString();
                //Console.WriteLine("Clipboard Active Window: " + sb.ToString());
                if (ConfigHelper.IgnoreList.Contains(windowText))
                {
                    return;
                }

                // detector clipboard type
                IDataObject d = Clipboard.GetDataObject();
                if (d.GetDataPresent(DataFormats.Bitmap))
                {
                    AddLog(string.Format("Detected clipboard image"));
                    //AddClipboardImage();
                }

                string clipboardText = ClipboardHelper.GetText().Trim();
                if (clipboardText.Length > 0)
                {
                    AddLog(string.Format("Detected clipboard text : {0} ", clipboardText));
                    foreach (MatchItem matchItem in matchItems)
                    {
                        if (matchItem.Type == MatchType.Text)
                        {
                            if (Regex.IsMatch(clipboardText, matchItem.Params, RegexOptions.IgnoreCase | RegexOptions.Singleline))
                            {
                                AddLog(string.Format("Trigger action form params: {0} ", matchItem.Params));
                                try
                                {
                                    DoAction(matchItem, clipboardText, windowText);
                                }
                                catch (Exception ex)
                                {
                                    AddLog("ERROR:" + ex.ToString());
                                }
                                break;
                            }
                        }
                    }

                    if (clipboardText.Length == 16) //hardcode case id
                    {
                        ClipboardHelper.LastClipboardText = clipboardText;
                    }

                }
            }
        }

        protected void DoAction(MatchItem matchItem, string clipboardText = null, string windowText = null)
        {
            // parameter replace
            // {timestamp}  --> 2022010102030222
            // {clipboard_text}
            if (clipboardText != null)
            {
                clipboardText = clipboardText.Trim();
            }
            ActionHelper.OpenActionForm(matchItem, clipboardText, ConfigHelper.WorkingDir);
        }

        #region WinForm related

        private void FrmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!menuExit)
            {
                e.Cancel = true;
                this.Hide();
            }
            else
            {
                WebServer.StopListener();
            }
        }

        private void ShowNotification()
        {
            notifyIcon1.Visible = true;
        }

        private void mnExit_Click(object sender, EventArgs e)
        {
            menuExit = true;
            this.Close();
        }
        private void mnShowLogs_Click(object sender, EventArgs e)
        {
            //ShowForm();
            string logFile = ConfigHelper.LogPath + "\\" + DateTime.Now.ToString("yyyy-MM-dd") + ".txt";
            System.Diagnostics.Process.Start(logFile);
        }
        private void notifyIcon1_DoubleClick(object sender, EventArgs e)
        {
            //ShowForm();
        }

        private void ShowForm()
        {
            this.ShowInTaskbar = true;
            this.Opacity = 100;
            this.Show();
            this.TopMost = true;
            this.BringToFront();
            this.Focus();
            this.TopMost = false;
        }

        private void AddLog(string log, bool withtime = true)
        {
            if (withtime)
            {
                log = DateTime.Now.ToString("[yyyy-MM-dd HH:mm:ss] - ") + log + "\n";
            }
            if (ConfigHelper.IsConsole)
            {
                Console.Write(log);
            }
            else
            {
                //txtLog.AppendText(log);

                if (!Directory.Exists(ConfigHelper.LogPath))
                {
                    Directory.CreateDirectory(ConfigHelper.LogPath);
                }
                string logFile = ConfigHelper.LogPath + "\\" + DateTime.Now.ToString("yyyy-MM-dd") + ".txt";
                StreamWriter sw = new StreamWriter(logFile, true);
                sw.Write(log);
                sw.Flush();
                sw.Close();
            }
        }

        #endregion

        private void mnHelp_Click(object sender, EventArgs e)
        {
            string helpUrl = string.Format("http://localhost:{0}/", ConfigHelper.WebServerPort);
            System.Diagnostics.Process.Start(helpUrl);
        }

        private void mnCases_Click(object sender, EventArgs e)
        {
            string helpUrl = string.Format("http://localhost:{0}/case.html", ConfigHelper.WebServerPort);
            System.Diagnostics.Process.Start(helpUrl);
        }
    }
}
