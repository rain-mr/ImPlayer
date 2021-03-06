﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using Newtonsoft.Json;
using System.Web;
using System.Text.RegularExpressions;
using System.Collections;

namespace BaiduYun
{
    public class Baidu
    {
        private string userName = "";
        private string pwd = "";
        private CookieCollection responseCookie = new CookieCollection();//用于记录登陆过程中的cookie
        private CookieContainer requestCookie = new CookieContainer();//用于记录登陆过程中的cookie
        private string Token = "";
        private string SkyDrive_app_id = "250528";//网盘对应的应用ID
        private bool isLogin = true;
        public bool IsLogin{
            get { return isLogin; }
        }

        public Baidu(string _username, string _pwd)
        {
            userName = _username;
            pwd = _pwd;
            Login();//登录百度

        }

        #region 公开方法
        /// <summary>
        /// 获取文件目录
        /// </summary>
        /// <returns></returns>
        public List<BaiDuFile> GetFileDir()
        {
            if (!IsLogin) return null;
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("app_id", SkyDrive_app_id.ToString());
            string url = "http://pan.baidu.com/api/list?";
            
            UrolsPage page = new UrolsPage ();
            foreach (KeyValuePair<string,string> item in data) {
                url += item.Key + "=" + item.Value + "&";
            }
            page.Url = url;
            page.RequestCookie = requestCookie;
            page.GET();

            string html = page.Html;
            Regex reg = new Regex(@",.*,");
            Match list_m = reg.Match(html);
            reg = new Regex(@"\{.*?\}");
            MatchCollection mc = reg.Matches(list_m.Value);
            List<BaiDuFile> fileList = new List<BaiDuFile>();
            BaiDuFile fileItem;
            foreach (Match item in mc)
            {
                fileItem = new BaiDuFile ();
                if (GetJsonValue("isdir", item.Value) == "1")
                    fileItem.Type = 2;
                else
                    fileItem.Type = 1;
                fileItem.Name = GetJsonValue("server_filename", item.Value);
                fileList.Add(fileItem);
            }
            return fileList;
        }

        /// <summary>
        /// 下载文件
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public FileOper DownFile(string path,string localPath)
        {
            string url = "http://c.pcs.baidu.com/rest/2.0/pcs/file?";
            url += "method=download&path=" + path + "&app_id=" + SkyDrive_app_id;
            try
            {
                HttpWebRequest webReq = (HttpWebRequest)WebRequest.Create(url);         
                webReq.CookieContainer = requestCookie;
                GetAllCookies(requestCookie);
                webReq.Method = "GET";
                HttpWebResponse webResp = (HttpWebResponse)webReq.GetResponse();
                responseCookie.Add(webResp.Cookies);
                using (Stream st = webResp.GetResponseStream())
                {
                    using (Stream so = new System.IO.FileStream(localPath, System.IO.FileMode.Create))
                    {
                        long totalDownloadedByte = 0;
                        byte[] by = new byte[1024];
                        int osize = st.Read(by, 0, (int)by.Length);
                        while (osize > 0)
                        {
                            totalDownloadedByte = osize + totalDownloadedByte;
                            so.Write(by, 0, osize);
                            osize = st.Read(by, 0, (int)by.Length);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return FileOper.下载文件失败;
            }
            return FileOper.下载文件成功;
        }

        /// <summary>
        /// 上传文件
        /// </summary>
        /// <param name="filePath"></param>
        public FileOper UpLoadFile(string remotePath, string localPath) {
            try
            {
                //上传文件url
                string url = "http://c.pcs.baidu.com/rest/2.0/pcs/file?";
                Dictionary<string, string> data = new Dictionary<string, string>();
                data.Add("method", "upload");//方法
                data.Add("path", remotePath);//上传到网盘的路径
                data.Add("ondup", "overwrite");//模式，覆盖
                data.Add("app_id", SkyDrive_app_id.ToString());
                StringBuilder sb = new StringBuilder();
                foreach (KeyValuePair<string, string> kv in data)
                {
                    sb.Append(HttpUtility.UrlEncode(kv.Key));
                    sb.Append("=");
                    sb.Append(HttpUtility.UrlEncode(kv.Value));
                    sb.Append("&");
                }
                string uploadUrl = url + sb.ToString();

                string boundary = GetBoundary();
                UrolsPage page = new UrolsPage();
                page.RequestCookie = requestCookie;
                FileInfo fileInfo = new FileInfo(localPath);
                if (fileInfo.Exists)
                {
                    using (FileStream fileStream = fileInfo.Open(FileMode.Open, FileAccess.Read))
                    {
                        string contenttype = GetContextType(fileInfo);

                        StringBuilder sbHeader = new StringBuilder();
                        sbHeader.Append("--");
                        sbHeader.Append(boundary);
                        sbHeader.Append("\r\n");
                        sbHeader.Append("Content-Disposition: form-data;filename=\"" + Path.GetFileName(localPath) + "\"");
                        sbHeader.Append("\r\n");
                        sbHeader.Append("Content-Type: ");
                        sbHeader.Append(contenttype);
                        sbHeader.Append("\r\n");
                        sbHeader.Append("\r\n");

                        string postHeader = sbHeader.ToString();
                        byte[] postHeaderBytes = Encoding.UTF8.GetBytes(postHeader);

                        string result = page.POST(uploadUrl, boundary, postHeaderBytes, fileStream);
                    }
                }
                else {
                    return FileOper.下载文件不存在;
                }
                return FileOper.上传文件成功;
            }
            catch (Exception ex) {
                return FileOper.上传文件失败;
            }
            return FileOper.上传文件成功;
        }
        
        #endregion

        /// <summary>
        /// 登陆
        /// </summary>
        private void Login()
        {
            GetPara();//获取登陆所需的参数
            string loginUrl = "https://passport.baidu.com/v2/api/?login";
            UrolsPage page = new UrolsPage();
            page.Url = loginUrl;
            page.RequestCookie = requestCookie;
            Dictionary<string, string> Data = new Dictionary<string, string>();
            Data.Add("staticpage", "http://zhidao.baidu.com/static/html/v3Jump_bf2a8d6e.html");
            Data.Add("charset", "GBK");
            Data.Add("token", Token);
            Data.Add("tpl", "ik");
            Data.Add("apiver", "v3");
            Data.Add("tt", Utility.GetCurrentTimeStamp().ToString());
            Data.Add("codestring", "");
            Data.Add("isPhone", "false");
            Data.Add("safeflg", "0");
            Data.Add("u", "http://www.baidu.com/");
            Data.Add("username", userName);
            Data.Add("password", pwd);
            Data.Add("verifycode", "");
            Data.Add("mem_pass", "on");
            Data.Add("ppui_logintime", "22429");
            Data.Add("callback", "parent.bd__pcbs__7doo5q");

            StringBuilder sb = new StringBuilder();
            foreach (KeyValuePair<string, string> kv in Data)
            {
                sb.Append(HttpUtility.UrlEncode(kv.Key));
                sb.Append("=");
                sb.Append(HttpUtility.UrlEncode(kv.Value));
                sb.Append("&");
            }

            byte[] data = Encoding.UTF8.GetBytes(sb.ToString().TrimEnd('&'));
            Stream stream = new MemoryStream(data);
            page.POST(loginUrl, "", null, stream);

            requestCookie = page.RequestCookie;
            List<Cookie> listCookie = GetAllCookies(requestCookie);
            if(listCookie.Select(o => o.Name == "BDUSS").Count()==0){
                isLogin = false;
                return;
            }
            //Bduss = Convert.ToString(page.ResponseCookie["BDUSS"].Value);
        }

        /// <summary>
        /// 获取BAIDUID和Token值
        /// </summary>
        private void GetPara()
        {
            //获取BAIDUID
            UrolsPage page = new UrolsPage("http://www.baidu.com", Encoding.UTF8);
            requestCookie = page.RequestCookie;
            GetAllCookies(requestCookie);
            //获取Token,CodeString
            GetTokenAndCodeString();//获取 Token, CodeString
        }

        /// <summary>
        /// 获取Token，用于登陆用
        /// </summary>
        private void GetTokenAndCodeString()
        {
            string url = string.Format("https://passport.baidu.com/v2/api/?getapi&tpl=ik&apiver=v3&tt={0}&class=login", Utility.GetCurrentTimeStamp());
            UrolsPage page = new UrolsPage();
            page.Url = url;
            page.RequestCookie = requestCookie;
            page.GET();
            requestCookie = page.RequestCookie;
            GetAllCookies(requestCookie);
            ResultData result = JsonConvert.DeserializeObject<ResultData>(page.Html);
            if (result.ErrInfo.no == "0")
            {
                Token = result.Data.Token;
                //CodeString = result.Data.CodeString;
            }
        }

        /// <summary>
        /// 获取json字符串对应的key值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        private string GetJsonValue(string key, string input)
        {
            string key1 = @"""" + key + @""":"".*?""";
            string key2 = @"""" + key + @""":.*?,";
            string key3 = @"""" + key + @""":.*?}";
            bool isNum = false;
            //尝试匹配第一种情况
            Regex reg = new Regex(key1);
            Match m = reg.Match(input);
            if (m.Value == null || string.IsNullOrEmpty(m.Value))
            {
                //尝试匹配第二种情况
                reg = new Regex(key2);
                m = reg.Match(input);
                isNum = true;
            }
            if (m.Value == null || string.IsNullOrEmpty(m.Value))
            {
                //尝试匹配第三种情况
                reg = new Regex(key3);
                m = reg.Match(input);
                isNum = true;
            }
            string value = "";
            if (!isNum)
            {
                reg = new Regex(@""".*?""");
                MatchCollection mc = reg.Matches(m.Value);
                value = mc[1].Value;
                value = value.Replace("\"", "");
                value = SwitchValue(value);
            }
            else
            {
                value = m.Value;
                value = value.Split(':')[1];
                value = value.Replace(",", "");
                value = value.Replace("}", "");
            }
            return value;
        }

        /// <summary>
        /// 将\u编码转为中文
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private string SwitchValue(string input)
        {
            Regex regex = new Regex(@"\\u(\w{4})");
            string result = regex.Replace(input, delegate(Match m)
            {
                string hexStr = m.Groups[1].Value;
                string charStr = ((char)int.Parse(hexStr, System.Globalization.NumberStyles.HexNumber)).ToString();
                return charStr;
            });
            return result;
        }

        /// <summary>
        /// 获取ContextType
        /// </summary>
        /// <param name="fileInfo"></param>
        /// <returns></returns>
        private string GetContextType(FileInfo fileInfo)
        {
            string result = "";
            switch (fileInfo.Extension.ToLower())
            {
                case ".jpeg": result = "image/jpeg"; break;
                case ".jpg": result = "image/jpeg"; break;
                case ".txt": result = "text/plain"; break;
                case ".png": result = "image/png"; break;
                default: result = "text/plain"; break;
            }
            return result;
        }

        /// <summary>
        /// http中分隔符
        /// </summary>
        /// <returns></returns>
        private string GetBoundary()
        {
            return "----------" + DateTime.Now.Ticks.ToString("x");
        }
        
        /// <summary>
        /// 获取CookieContainer中的Cookie值
        /// </summary>
        /// <param name="cc"></param>
        /// <returns></returns>
        public static List<Cookie> GetAllCookies(CookieContainer cc)
        {
            List<Cookie> lstCookies = new List<Cookie>();

            Hashtable table = (Hashtable)cc.GetType().InvokeMember("m_domainTable", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.GetField | System.Reflection.BindingFlags.Instance, null, cc, new object[] { });
            StringBuilder sb = new StringBuilder();
            foreach (object pathList in table.Values)
            {
                SortedList lstCookieCol = (SortedList)pathList.GetType().InvokeMember("m_list", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.GetField | System.Reflection.BindingFlags.Instance, null, pathList, new object[] { });
                foreach (CookieCollection colCookies in lstCookieCol.Values)
                    foreach (Cookie c in colCookies)
                    {
                        lstCookies.Add(c);
                        sb.AppendLine(c.Domain + ":" + c.Name + "____" + c.Value + "\r\n");
                    }
            }
            return lstCookies;
        }
    }

    public class UrolsPage
    {

        private string url;
        public string Url
        {
            get { return url; }
            set { url = value; }
        }

        private string method;
        public string Method
        {
            get
            {
                if (string.IsNullOrEmpty(method))
                    method = "GET";
                method = method.ToUpper();
                if (method != "GET" && method != "POST")
                    method = "GET";
                return method;
            }
            set
            {
                method = value;
            }
        }

        private Encoding encoder;
        public Encoding Encoder
        {
            get
            {
                if (encoder == null)
                    encoder = Encoding.UTF8;
                return encoder;
            }
            set { encoder = value; }
        }

        private string html;
        public string Html { get { return html; } }

        private CookieContainer requestCookie;
        public CookieContainer RequestCookie
        {
            get
            {
                if (requestCookie == null)
                    requestCookie = new CookieContainer();
                return requestCookie;
            }
            set { requestCookie = value; }
        }

        private CookieCollection responseCookie;
        public CookieCollection ResponseCookie
        {
            get
            {
                if (responseCookie == null)
                    responseCookie = new CookieCollection();
                return responseCookie;
            }
            set { responseCookie = value; }
        }

        private Dictionary<string, string> data;
        public Dictionary<string, string> Data
        {
            get
            {
                if (data == null)
                    data = new Dictionary<string, string>();
                return data;
            }
            set { data = value; }
        }

        private string contentType;
        public string ContentType
        {
            get
            {
                if (string.IsNullOrEmpty(contentType))
                    contentType = "application/x-www-form-urlencoded";
                return contentType;
            }
            set
            {
                contentType = value;
            }
        }

        private string accept;
        public string Accept
        {
            get
            {
                if (string.IsNullOrEmpty(accept))
                    accept = "*/*";
                return accept;
            }
            set
            {
                accept = value;
            }
        }

        private string userAgent;
        public string UserAgent
        {
            get
            {
                if (string.IsNullOrEmpty(userAgent))
                    userAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/27.0.1453.94 Safari/537.36";
                return userAgent;
            }
            set
            {
                userAgent = value;
            }
        }

        private string referer;
        public string Referer
        {
            get
            {
                if (string.IsNullOrEmpty(referer))
                    referer = "";
                return referer;
            }
            set
            {
                referer = value;
            }
        }

        private long contentLength;
        public long ContentLength {
            get { return contentLength; }
            set { contentLength = value; }
        }

        public UrolsPage()
        {

        }
        public UrolsPage(string _url, Encoding _encoder)
        {
            url = _url;
            encoder = _encoder;
            GET();
        }

        public void GET()
        {
            //request 请求
            HttpWebRequest webReq = (HttpWebRequest)WebRequest.Create(url);
            webReq.CookieContainer = RequestCookie;//存储网页返回的cookie值
            webReq.Method = Method;
            //response 返回
            HttpWebResponse webResp = (HttpWebResponse)webReq.GetResponse();
            ResponseCookie.Add(webResp.Cookies);
            //获取返回的response流
            using (Stream stream = webResp.GetResponseStream())
            {
                using (StreamReader read = new StreamReader(stream, Encoder))
                {
                    html = read.ReadToEnd();
                }
            }
        }

        public string POST(string url, string boundary, byte[] postHeaderBytes, Stream stram)
        {
            string result = "";
            HttpWebRequest webrequest = (HttpWebRequest)WebRequest.Create(url);
            webrequest.CookieContainer = RequestCookie;
            if (!string.IsNullOrEmpty(boundary))
                webrequest.ContentType = "multipart/form-data; boundary=" + boundary;
            else
                webrequest.ContentType = "application/x-www-form-urlencoded";

            webrequest.Method = "POST";
            //生成header
            //string postHeader = sbHeader.ToString();
            //byte[] postHeaderBytes = Encoding.UTF8.GetBytes(postHeader);
            byte[] boundaryBytes = Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");
            //header长度+字节流长度=内容长度
            long length = 0;
            if (postHeaderBytes != null)
                length += postHeaderBytes.Length;
            length += stram.Length + boundaryBytes.Length;
            webrequest.ContentLength = length;//header的长度
            //想request中写入数据
            using (Stream requestStream = webrequest.GetRequestStream())
            {
                if (postHeaderBytes != null)
                {
                    //将header写入request请求
                    requestStream.Write(postHeaderBytes, 0, postHeaderBytes.Length);
                }
                //将字节流写入request请求
                byte[] buffer = new Byte[(int)stram.Length];
                int bytesRead = 0;
                while ((bytesRead = stram.Read(buffer, 0, buffer.Length)) != 0)
                    requestStream.Write(buffer, 0, bytesRead);
                //将分隔符写入request请求
                requestStream.Write(boundaryBytes, 0, boundaryBytes.Length);//分隔符
            }
            
            //获取返回响应
            using (HttpWebResponse webResp = (HttpWebResponse)webrequest.GetResponse())
            {
                using (Stream stream = webResp.GetResponseStream())
                {
                    using (StreamReader sr = new StreamReader(stream, Encoder))
                    {
                        result = sr.ReadToEnd();
                    }
                }
            }
            return result;
        }
    }

    public class BaiDuFile{
        public string Name { get; set; }
        public int Type { get; set; }//1=文件，2=文件夹
    }

    public enum FileOper
    {
        下载文件不存在 = 1,
        下载文件失败 = 2,
        下载文件成功 = 3,
        上传文件不存 = 4,
        上传文件失败 = 5,
        上传文件成功 = 6
    }
}
