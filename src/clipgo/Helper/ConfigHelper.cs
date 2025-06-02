using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace clipgo.Helper
{
    /// <summary>
    /// Global Config Settings
    /// </summary>
    internal class ConfigHelper
    {
        /// <summary>
        /// Application Name
        /// </summary>
        public static string AppName = "Clip Go";

        /// <summary>
        /// Clip Go Working Directory
        /// </summary>
        public static string WorkingDir = "";

        /// <summary>
        /// Clip Go Config File Path
        /// </summary>
        public static string ConfigFile = "";

        /// <summary>
        /// Is Clip Go Running on Console Mode
        /// </summary>
        public static bool IsConsole = false;

        /// <summary>
        /// Ingore List for specific Form
        /// </summary>
        public static string[] IgnoreList = new string[] { "Clip Go" };

        /// <summary>
        /// Log Path for debug
        /// </summary>
        public static string LogPath;

        /// <summary>
        /// Web Server Port used to start mini web server
        /// </summary>
        public static string WebServerPort = "1207";

        public static string WebServerRoot = "data\\wwwroot";

        /// <summary>
        /// Action Form Start Position
        /// </summary>
        public static string ActionFormStartPosition;

        /// <summary>
        /// Action Form Delay Close time, default value is 3000
        /// </summary>
        public static int ActionFormDelayClose = 3000;

        /// <summary>
        /// Action Form Default Browser
        /// </summary>
        public static string ActionFormDefaultBrowser = "default";

        public static XmlDocument XmlDoc;



        public static void LoadXmlConfig()
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(ConfigFile);
            AppName = xmlDoc.SelectSingleNode("/config/name").InnerText;
            IgnoreList = xmlDoc.SelectSingleNode("/config/ignore").InnerText.Split(',');
            LogPath = xmlDoc.SelectSingleNode("/config/logPath").InnerText;
            LogPath = LogPath.Contains(":") ? LogPath : WorkingDir + "\\" + LogPath;
            ActionFormStartPosition =xmlDoc.SelectSingleNode("/config/actionFormStartPosition").InnerText;
            ActionFormDelayClose = Convert.ToInt32(xmlDoc.SelectSingleNode("/config/actionFormDelayClose").InnerText);

            WebServerPort = xmlDoc.SelectSingleNode("/config/webServerPort").InnerText;
            WebServerRoot = xmlDoc.SelectSingleNode("/config/webServerRoot").InnerText;

            if (!WebServerRoot.Contains(":"))
            {
                WebServerRoot = WorkingDir + "\\" + WebServerRoot;
            }

            XmlDoc = xmlDoc;
        }
    }
}
