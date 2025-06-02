using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace clipgo.Helper
{
    public enum FormPosotion
    {
        TopLeft,
        TopCenter,
        TopRight,
        MiddleLeft,
        MiddleCenter,
        MiddleRight,
        BottomLeft,
        BottomCenter,
        BottomRight,
        FollowMouse
    }

    public class LinkFormHelper
    {
        public delegate void OnClick(Form form, int index);
        public event OnClick OnClickEvent;

        private Form myForm;
        public LinkFormHelper()
        {
        }

        public void BuildForm(string title, FormPosotion position, List<string> links)
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmMain));

            myForm = new Form();
            myForm.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            myForm.Text = title;
            myForm.Width = 300;
            myForm.Height = 200;
            myForm.Top = SystemInformation.WorkingArea.Height - myForm.Height;
            myForm.Left = SystemInformation.WorkingArea.Width - myForm.Width;
            myForm.StartPosition = FormStartPosition.Manual;
            myForm.TopMost = true;

            int linkTop = 10;
            for (int i = 0; i < links.Count; i++)
            {
                LinkLabel label = new LinkLabel();
                myForm.Controls.Add(label);
                label.Top = linkTop;
                label.Left = 20;
                label.Width = 250;
                string labelText = links[i];
                label.Text = labelText;
                label.Click += delegate
                {
                    if (OnClickEvent != null)
                    {
                        OnClickEvent(myForm, i);
                    }
                };
                linkTop += 30;
            }
        }

        public void ShowForm()
        {
            // Delay Close
            System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
            timer.Interval = 4000;
            timer.Tick += delegate
            {
                myForm.Close();
            };
            timer.Enabled = true;

            myForm.Show();
        }

    }
}
