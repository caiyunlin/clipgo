using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace clipgo.Helper
{
    class ClipboardHelper
    {
        //Reference https://docs.microsoft.com/en-us/windows/desktop/dataxchg/wm-clipboardupdate
        public const int WM_CLIPBOARDUPDATE = 0x031D;
        //Reference https://www.pinvoke.net/default.aspx/Constants.HWND
        public static IntPtr HWND_MESSAGE = new IntPtr(-3);

        //Reference https://www.pinvoke.net/default.aspx/user32/AddClipboardFormatListener.html
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool AddClipboardFormatListener(IntPtr hwnd);

        //Reference https://www.pinvoke.net/default.aspx/user32.setparent
        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        //Reference https://www.pinvoke.net/default.aspx/user32/getwindowtext.html
        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        //Reference https://www.pinvoke.net/default.aspx/user32.getwindowtextlength
        [DllImport("user32.dll")]
        public static extern int GetWindowTextLength(IntPtr hWnd);

        //Reference https://www.pinvoke.net/default.aspx/user32.getforegroundwindow
        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();


        public static string GetText()
        {
            string ReturnValue = string.Empty;
            Thread STAThread = new Thread(
                delegate ()
                {
                    // Use a fully qualified name for Clipboard otherwise it
                    // will end up calling itself.
                    ReturnValue = System.Windows.Forms.Clipboard.GetText();
                });
            STAThread.SetApartmentState(ApartmentState.STA);
            STAThread.Start();
            STAThread.Join();

            return ReturnValue;
        }


        static Dictionary<string, object> contents = new Dictionary<string, object>();

        /// <summary>
        /// Backup Clipboard Objects
        /// </summary>
        public static void Backup()
        {
            contents.Clear();
            IDataObject o = Clipboard.GetDataObject();
            foreach (string format in o.GetFormats())
                contents.Add(format, o.GetData(format));
        }

        /// <summary>
        /// Resource Clipboard Objects
        /// </summary>
        public static void Restore()
        {
            DataObject o = new DataObject();
            foreach (string format in contents.Keys)
            {
                o.SetData(format, contents[format]);
            }
            Clipboard.SetDataObject(o);
        }

        /// <summary>
        /// Last Clipboard Text
        /// </summary>
        public static string LastClipboardText = "";
    }
}
