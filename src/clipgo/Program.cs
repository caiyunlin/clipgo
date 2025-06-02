using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace clipgo
{
    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        public static void Main(string[] args)
        {
            bool ret;
            System.Threading.Mutex mutex = new System.Threading.Mutex(true, Application.ProductName, out ret);
            if (ret)
            {
                string applicationDir = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName.Substring(0, System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName.LastIndexOf('\\'));
                string configFile = string.Format("{0}\\data\\config.xml", applicationDir);

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new FrmMain(false, applicationDir, configFile));

                mutex.ReleaseMutex();
            }
            else
            {
                MessageBox.Show(null, "clipgo is already running", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Application.Exit();//退出程序
            }
        }
    }
}
