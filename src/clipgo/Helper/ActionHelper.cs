using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using System.Xml;

namespace clipgo.Helper
{
    class ActionHelper
    {
        public static bool StopProcessClipboard = false;

        public static void StopListenClipboard(int ms)
        {
            StopProcessClipboard = true;
            Thread t = new Thread(delegate ()
            {
                Thread.Sleep(ms);
                StopProcessClipboard = false;
            });
            t.Start();
        }

        public static void StartListenClipboard()
        {
            StopProcessClipboard = false;
        }

        public static void Alert(string text)
        {
            MessageBox.Show(text, "Alert", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public static void SendKeyText(string text)
        {
            Console.WriteLine("sent key {0}", text);
            SendKeys.Send(text);
        }

        public static void FormatXMLAction(string xml)
        {
            LinkFormHelper helper = new LinkFormHelper();

            List<string> links = new List<string>()
            {
                "Format Xml"
            };

            helper.BuildForm("FormatXml", FormPosotion.MiddleCenter, links);

            helper.OnClickEvent += delegate (Form linkForm, int index)
            {
                StopListenClipboard(100);
                string outXml = FormatXML(xml);
                if (!xml.StartsWith("<?xml"))
                {
                    outXml = outXml.Replace("<?xml version=\"1.0\"?>", "");
                }
                Clipboard.SetDataObject(outXml, true);
                linkForm.Close();
            };
            helper.ShowForm();

        }

        public static string FormatXML(string XMLstring)
        {
            XmlDocument xmlDocument = GetXmlDocument(XMLstring);
            return ConvertXmlDocumentTostring(xmlDocument);
        }

        public static string ConvertXmlDocumentTostring(XmlDocument xmlDocument)
        {
            MemoryStream memoryStream = new MemoryStream();
            XmlTextWriter writer = new XmlTextWriter(memoryStream, null)
            {
                Formatting = System.Xml.Formatting.Indented
            };
            xmlDocument.Save(writer);
            StreamReader streamReader = new StreamReader(memoryStream);
            memoryStream.Position = 0;
            string xmlString = streamReader.ReadToEnd();
            streamReader.Close();
            memoryStream.Close();
            return xmlString;
        }

        public static XmlDocument GetXmlDocument(string xmlString)
        {
            XmlDocument document = new XmlDocument();
            document.LoadXml(xmlString);
            return document;
        }

        public static void OpenActionForm(MatchItem matchItem, string clipboardText, string workingDir)
        {

            StopListenClipboard(1000);

            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmMain));

            // process regex map

            Regex regex = new Regex(matchItem.Node.Attributes["params"].Value, RegexOptions.IgnoreCase | RegexOptions.Singleline);
            Match match = regex.Match(clipboardText);

            XmlNodeList nodes = matchItem.Node.SelectNodes("actionForm/links/link");

            Form linkForm = new Form();
            linkForm.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            linkForm.Text = "ClipGo";
            linkForm.Width = 300;
            linkForm.Height = 200;
            if (nodes.Count > 5)
            {
                linkForm.Height += (nodes.Count - 5) * 30;
            }

            linkForm.StartPosition = FormStartPosition.Manual;
            //Set Position RightBottom

            linkForm.Top = SystemInformation.WorkingArea.Height - linkForm.Height;
            linkForm.Left = SystemInformation.WorkingArea.Width - linkForm.Width;

            if (ConfigHelper.ActionFormStartPosition == "FollowMouse")
            {
                var mousePoint = MouseHelper.GetCursorPosition();
                linkForm.Top = mousePoint.Y + 20;
                linkForm.Left = mousePoint.X + 20;
            }

            linkForm.MaximizeBox = false;
            linkForm.MinimizeBox = false;

            int linkTop = 10;
            int linkIndex = 0;
            foreach (XmlNode node in nodes)
            {
                linkIndex++;

                string linkText = node.Attributes["title"].Value;
                string linkAction = node.Attributes["action"].Value;
                //replace the matches
                for (int i = 0; i < match.Groups.Count; i++)
                {
                    linkText = linkText.Replace("{" + i.ToString() + "}", match.Groups[i].Value);
                    linkAction = linkAction.Replace("{" + i.ToString() + "}", match.Groups[i].Value);
                }

                //linkText = linkIndex + ". " + linkText;

                //Button label = new Button();

                //label.FlatAppearance.BorderSize = 0;
                //label.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
                //label.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                //label.ForeColor = System.Drawing.Color.Blue;
                //label.Location = new System.Drawing.Point(43, 418);
                //label.Size = new System.Drawing.Size(75, 23);
                //label.TabIndex = 2;
                //label.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
                //label.UseVisualStyleBackColor = true;

                LinkLabel label = new LinkLabel();

                linkForm.Controls.Add(label);
                label.Top = linkTop;
                label.Left = 20;
                label.Width = 250;
                string labelText = linkText;
                label.Text = labelText;

                label.Click += delegate
                {
                    string browser = node.Attributes["browser"] == null ? "default" : node.Attributes["browser"].Value;
                    string action = string.Format(linkAction, clipboardText);

                    if (action.EndsWith(".ps1"))
                    {
                        System.Diagnostics.Process.Start("powershell.exe", $"-NoProfile -ExecutionPolicy Bypass -File \"{action}\"");
                    }
                    else if (action.EndsWith(".bat") || action.EndsWith(".cmd"))
                    {
                        if (File.Exists(action))
                        {
                            System.Diagnostics.Process.Start(action, clipboardText);
                        }
                        else
                        {
                            MessageBox.Show("can't find file :" + action);
                        }
                    }
                    else
                    {
                        System.Diagnostics.Process.Start(action);
                    }

                    linkForm.Close();
                };
                linkTop += 30;
            }

            // Delay Close
            System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
            timer.Interval = ConfigHelper.ActionFormDelayClose;
            timer.Tick += delegate
            {
                linkForm.Close();
            };
            timer.Enabled = true;
            linkForm.KeyPreview = true;
            linkForm.ShowInTaskbar = false;
            linkForm.TopMost = true;
            linkForm.Focus();

            linkForm.ShowDialog();

        }

    }
}
