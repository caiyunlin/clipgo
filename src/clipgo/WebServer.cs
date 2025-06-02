
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using clipgo.Helper;

namespace clipgo
{
    public class WebServer
    {
        public static HttpListener listener;
        public static bool Stopped = false;

        private static dynamic GetPostData(HttpListenerContext context)
        {
            StreamReader reader = new StreamReader(context.Request.InputStream);
            string text = reader.ReadToEnd();
            byte[] bytes = Convert.FromBase64String(text);
            string rawJson = System.Text.Encoding.UTF8.GetString(bytes);
            var dyn = JsonConvert.DeserializeObject<dynamic>(rawJson);
            return dyn;
        }

        public static void StartListener()
        {
            string port = ConfigHelper.WebServerPort;
            string wwwroot = ConfigHelper.WebServerRoot;

            string prefix = string.Format("http://localhost:{0}/", port);
            listener = new HttpListener();
            listener.Prefixes.Add(prefix);
            listener.Start();

            while (true)
            {
                if (Stopped || !listener.IsListening)
                {
                    break;
                }

                try
                {
                    // Note: The GetContext method blocks while waiting for a request.
                    HttpListenerContext context = listener.GetContext();
                    HttpListenerRequest request = context.Request;

                    string responseString = "{\"status\":\"error\",\"message\":\"404 - Not Found!\"}";

                    if (request.HttpMethod == "GET")
                    {
                        string requestUrl = request.Url.LocalPath;
                        if (requestUrl == "/")
                        {
                            requestUrl = "index.html";
                        }
                        string localFile = wwwroot + "\\" + requestUrl;
                        if (File.Exists(localFile))
                        {

                            if (requestUrl.EndsWith("html")) //Text
                            {
                                StreamReader reader = new StreamReader(localFile);
                                responseString = reader.ReadToEnd();
                                reader.Close();
                            }
                            else //Picture
                            {
                                Stream stream = File.OpenRead(localFile);
                                stream.CopyTo(context.Response.OutputStream);
                                stream.Close();
                                context.Response.Close();
                                continue;
                            }
                        }

                    }
                    else if (request.HttpMethod == "POST")
                    {
                        // Handle API
                        string localPath = request.Url.LocalPath;

                        // dynamic response
                        dynamic res = new JObject();
                        res.status = "success";

                        if (localPath == "/get_json")
                        {
                            dynamic postData = GetPostData(context);
                            string key = postData.key;
                            res.content = LoadJson(key);
                        }
                        else if (localPath == "/save_json")
                        {
                            dynamic postData = GetPostData(context);
                            if (postData != null)
                            {
                                string key = postData.key;
                                string content = postData.content;
                                SaveJson(key, content);
                            }
                            else
                            {
                                res.status = "error";
                                res.message = "Failed to Save Json";
                            }
                        }
                        else if(localPath == "/get_clipboard")
                        {
                            string clipboardText = ClipboardHelper.GetText().Trim();
                            res.content = clipboardText;
                        }
                        else
                        {
                            res.status = "error";
                            res.content = "Not Support Method";
                        }

                        responseString = res.ToString();
                    }
                    // Obtain a response object.
                    HttpListenerResponse response = context.Response;
                    // Construct a response.                
                    byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
                    // Get a response stream and write the response to it.
                    response.ContentLength64 = buffer.Length;
                    System.IO.Stream output = response.OutputStream;
                    output.Write(buffer, 0, buffer.Length);
                    // You must close the output stream.
                    output.Close();
                }
                catch (HttpListenerException ex)
                {
                    Console.WriteLine(ex.Message);
                }

            }

        }

        public static void StopListener()
        {
            Stopped = true;
            listener.Stop();
        }

        private static string LoadJson(string key)
        {
            string wwwroot = ConfigHelper.WebServerRoot;
            string jsonFile = wwwroot + "\\" + key + ".json";
            StreamReader sr = new StreamReader(jsonFile);
            string json = sr.ReadToEnd();
            sr.Close();
            return json;
        }

        private static void SaveJson(string key, string content)
        {
            string wwwroot = ConfigHelper.WebServerRoot;
            string jsonFile = wwwroot + "\\" + key + ".json";
            string jsonFolder = Path.GetDirectoryName(jsonFile);
            if (!Directory.Exists(jsonFolder))
            {
                Directory.CreateDirectory(jsonFolder);
            }

            StreamWriter sw = new StreamWriter(jsonFile);
            sw.Write(content);
            sw.Flush();
            sw.Close();
        }
    }
}
