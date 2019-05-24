﻿using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;

namespace Server
{
    class HttpServer : HttpService {
       
        Thread thread;

        public HttpServer() {
            host = Settings.HTTPIPAddress;          
        }

        public void Start() {
            thread = new Thread(new ThreadStart(Listen));
            thread.Start();
        }

        public new void Stop() {
            base.Stop();
            thread.Abort();
        }


        public override void OnGetRequest(HttpListenerRequest request, HttpListenerResponse response)
		{
            string url = request.Url.PathAndQuery;
            if (url.Contains("?"))
            {
                url = url.Substring(0,url.IndexOf("?"));
            }
            try
            {
                switch (url)
                {
                    case "/":
                        writeRresponse(response, GameLanguage.GameName);
                        break;
                    case "/regist":
                        string id = request.QueryString["id"].ToString();
                        string psd = request.QueryString["psd"].ToString();
                        string email = request.QueryString["email"].ToString();
                        string name = request.QueryString["name"].ToString();
                        string question = request.QueryString["question"].ToString();
                        string answer = request.QueryString["answer"].ToString();
                        string ip = request.QueryString["ip"].ToString();
                        ClientPackets.NewAccount p = new ClientPackets.NewAccount();
                        p.AccountID = id;
                        p.Password = psd;
                        p.EMailAddress = email;
                        p.UserName = name;
                        p.SecretQuestion = question;
                        p.SecretAnswer = answer;
                        int result = SMain.Envir.HTTPNewAccount(p,ip);
                        writeRresponse(response, result.ToString());
                        break;
                    case "/login":
                        id = request.QueryString["id"].ToString();
                        psd = request.QueryString["psd"].ToString();
                        result = SMain.Envir.HTTPLogin(id, psd);
                        writeRresponse(response, result.ToString());                        
                        break;
                    case "/addNameList":
                        id = request.QueryString["id"].ToString();
                        string fileName = request.QueryString["fileName"].ToString();                   
                        addNameList(id, fileName);
                        writeRresponse(response, "true");
                        break;
                    default:
                        writeRresponse(response, "error");
                        break;
                }
            }
            catch (Exception error)
            {
                writeRresponse(response, "error" + error);
            }
        }

        void addNameList(string playerName,string fileName) {
            fileName = Settings.NameListPath + fileName;
            string sDirectory = Path.GetDirectoryName(fileName);
            Directory.CreateDirectory(sDirectory);
            string tempString = fileName;
            if (File.ReadAllLines(tempString).All(t => playerName != t))
            {
                using (var line = File.AppendText(tempString))
                {
                    line.WriteLine(playerName);
                }
            }
        }


        void writeRresponse(HttpListenerResponse response , string responseString) {          
            try
            {
                response.ContentLength64 = Encoding.UTF8.GetByteCount(responseString);
                response.ContentType = "text/html; charset=UTF-8";
            }
            finally
            {
                Stream output = response.OutputStream;
                StreamWriter writer = new StreamWriter(output);
                writer.Write(responseString);
                writer.Close();
            }
        }

        public override void OnPostRequest(HttpListenerRequest request, HttpListenerResponse response) {
            Console.WriteLine("POST request: {0}", request.Url);
        }
    }

}
