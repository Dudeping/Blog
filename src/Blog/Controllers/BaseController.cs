using BlogPlus.Models;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.WebPages;

namespace BlogPlus.Controllers
{
    /// <summary>
    /// 基类控制器
    /// </summary>
#if DEBUG
    //[RequireHttps]
#else
    [RequireHttps]
#endif
    public class BaseController : Controller
    {
        /// <summary>
        /// 数据上下文
        /// </summary>
        protected readonly BlogContext db = new BlogContext();
        protected int pageSize = 5; //每页显示条数

        /// <summary>
        /// 校验验证码
        /// </summary>
        /// <returns>检验结果,true:验证失败; false:验证成功</returns>
        private bool CheckCode(string code)
        {
            return Session["blogPlus_session_verifycode"] == null || Session["blogPlus_session_verifycode"].ToString() != GetMD5(code.ToLower(), "verifycode");
        }

        /// <summary>
        /// 添加日志
        /// </summary>
        /// <param name="title">标题</param>
        /// <param name="logType">类型</param>
        /// <param name="content">内容</param>
        /// <param name="user">操作人</param>
        /// <param name="Ip">Ip地址</param>
        protected void LogHelper(string title, string logType, string content, string user, string Ip)
        {
            db.Logs.Add(new Log() { Title = title, LogType = logType, Content = content, IsRead = false, User = user, Ip = Ip, CreateTime = DateTime.Now });
            db.SaveChanges();
        }

        /// <summary>
        /// 创建验证码
        /// </summary>
        /// <returns>验证码字节流</returns>
        private byte[] GetVerifyCode()
        {
            // 验证码图片高宽
            int codeW = 100;
            int codeH = 30;
            // 验证码字号
            int fontSize = 16;
            string chkCode = string.Empty;
            // 验证码颜色
            Color[] color = { Color.Black, Color.Red, Color.Blue, Color.Green, Color.Orange, Color.Brown, Color.Brown, Color.DarkBlue };
            // 验证码字体
            string[] font = { "Times New Roman" };
            // 验证码内容
            char[] character = { '1', '2', '3', '4', '5', '6', '7', '8', '9', 'a', 'b', 'd', 'e', 'f', 'h', 'k', 'm', 'n', 'r', 'x', 'y', 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'J', 'K', 'L', 'M', 'N', 'P', 'R', 'S', 'T', 'W', 'X', 'Y' };
            Random rnd = new Random();
            // 随机生成验证码内容
            for (int i = 0; i < 5; i++)
            {
                chkCode += character[rnd.Next(character.Length)];
            }
            // 用MD5散列算法进行加密后保存到session
            WebHelper.WriteSession("blogPlus_session_verifycode", BitConverter.ToString(Encoding.Default.GetBytes(chkCode + "verifycode")).ToLower().Replace("-", ""));
            // 创建一个位图
            Bitmap bmp = new Bitmap(codeW, codeH);
            // 创建画布
            Graphics g = Graphics.FromImage(bmp);
            g.Clear(Color.White);
            // 画验证码
            for (int i = 0; i < chkCode.Length; i++)
            {
                g.DrawString(chkCode[i].ToString(), new Font(font[rnd.Next(font.Length)], fontSize), new SolidBrush(color[rnd.Next(color.Length)]), (float)i * 18, (float)rnd.Next(codeH-fontSize-2));
            }
            for (int i = 0; i < 6; i++)
            {
                // 画干扰线
                g.DrawLine(new Pen(color[rnd.Next(color.Length)]), rnd.Next(codeW), rnd.Next(codeH), rnd.Next(codeW), rnd.Next(codeH));
            }
            
            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    // 保存在内存流中
                    bmp.Save(ms, ImageFormat.Png);
                    // 返回字节流
                    return ms.ToArray();
                }
            }
            catch (Exception)
            {
                return null;
            }
            finally
            {
                g.Dispose();
                bmp.Dispose();
            }
        }

        /// <summary>
        /// 获得验证码
        /// </summary>
        /// <returns>验证码图片</returns>
        public ActionResult GetAuthCode()
        {
            return File(GetVerifyCode(), @"image/Gif");
        }

        /// <summary>
        /// 校验验证码
        /// </summary>
        /// <param name="code">要校验的字符串</param>
        /// <returns>校验结果 true:验证码正确, false:验证码错误</returns>
        [HttpPost]
        public ActionResult CheckVerifyCode(string code)
        {
            if (CheckCode(code))
            {
                return Json(false);
            }
            return Json(true);
        }

        /// <summary>
        /// Md5散列算法
        /// </summary>
        /// <param name="sDataIn">要加密的字符串</param>
        /// <param name="code">盐</param>
        /// <returns>加密后的密文</returns>
        protected string GetMD5(string sDataIn, string code)
        {
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            var bytValue = System.Text.Encoding.UTF8.GetBytes(sDataIn + code);
            var bytHash = md5.ComputeHash(bytValue);
            md5.Clear();
            string sTemp = bytHash.Aggregate("", (current, t) => current + t.ToString("X").PadLeft(2, '0'));
            return sTemp.ToLower();
        }

        /// <summary>
        /// 发送邮件
        /// </summary>
        /// <param name="email">接收邮箱</param>
        /// <param name="sub">邮件主题</param>
        /// <param name="content"></param>
        /// <returns></returns>
        public bool SeedEmail(string email, string sub, string content)
        {
            MailMessage msg = new MailMessage();

            msg.To.Add(email);
            msg.Subject = sub;
            msg.From = new MailAddress("admin.blog@ydath.cn");
            msg.SubjectEncoding = Encoding.UTF8;//邮件标题编码
            msg.Body = content;
            msg.BodyEncoding = Encoding.UTF8;//邮件内容编码
            msg.IsBodyHtml = true;
            msg.Priority = MailPriority.High;
            SmtpClient client = new SmtpClient
            {
                Credentials = new NetworkCredential("admin.blog@ydath.cn", "Dudeping.2016sicau"),
                Host = "smtp.exmail.qq.com"
            };

            try
            {
                client.Send(msg);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// 发送站内信
        /// </summary>
        /// <param name="email">收件人地址</param>
        /// <param name="sub">主题</param>
        /// <param name="content">内容</param>
        /// <returns></returns>
        public bool SeedMessage(string email, string sub, string content)
        {
            try
            {
                Letter entity = new Letter
                {
                    Title = sub,
                    To = email,
                    CreateTime = DateTime.Now,
                    Content = content,
                    From = db.Users.FirstOrDefault(p => p.Email == User.Identity.Name),
                    IsRead = false
                };
                db.Letters.Add(entity);
                db.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// 获得未读消息条数
        /// </summary>
        /// <returns>未读消息条数</returns>
        public int GetNewsNum()
        {
            return db.Letters.Count(p => p.To == User.Identity.Name && p.IsRead == false) + db.SysNews.Count(p => p.IsRead.Count(x => x.Email == User.Identity.Name) <= 0);
        }
    }

    /// <summary>
    /// 操作Session
    /// </summary>
    public static class WebHelper
    {
        /// <summary>
        /// 写Session
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        private static void WriteSession<T>(string key, T value)
        {
            if (key.IsEmpty())
                return;
            HttpContext.Current.Session[key] = value;
        }

        public static void WriteSession(string key, string value)
        {
            WriteSession<string>(key, value);
        }
    }

    public static class IpHelper
    {

        private static bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {
            return true; //总是接受  
        }

        #region Get

        /// <summary>
        /// HTTP GET方式请求数据.
        /// </summary>
        /// <param name="url">URL.</param>
        /// <param name="headht"></param>
        /// <returns></returns>
        private static string HttpGet(string url, Hashtable headht = null)
        {
            HttpWebRequest request;

            //如果是发送HTTPS请求  
            if (url.StartsWith("https", StringComparison.OrdinalIgnoreCase))
            {
                ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
                request = WebRequest.Create(url) as HttpWebRequest;
                request.ProtocolVersion = HttpVersion.Version10;
            }
            else
            {
                request = WebRequest.Create(url) as HttpWebRequest;
            }
            request.Method = "GET";
            //request.ContentType = "application/x-www-form-urlencoded";
            request.Accept = "*/*";
            request.Timeout = 15000;
            request.AllowAutoRedirect = false;
            WebResponse response = null;
            string responseStr = null;
            if (headht != null)
            {
                foreach (DictionaryEntry item in headht)
                {
                    request.Headers.Add(item.Key.ToString(), item.Value.ToString());
                }
            }

            try
            {
                response = request.GetResponse();

                if (response != null)
                {
                    StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                    responseStr = reader.ReadToEnd();
                    reader.Close();
                }
            }
            catch (Exception)
            {
                throw;
            }
            return responseStr;
        }

        private static string HttpGet(string url, Encoding encodeing, Hashtable headht = null)
        {
            HttpWebRequest request;

            //如果是发送HTTPS请求  
            if (url.StartsWith("https", StringComparison.OrdinalIgnoreCase))
            {
                ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
                request = WebRequest.Create(url) as HttpWebRequest;
                request.ProtocolVersion = HttpVersion.Version10;
            }
            else
            {
                request = WebRequest.Create(url) as HttpWebRequest;
            }
            request.Method = "GET";
            //request.ContentType = "application/x-www-form-urlencoded";
            request.Accept = "*/*";
            request.Timeout = 15000;
            request.AllowAutoRedirect = false;
            WebResponse response = null;
            string responseStr = null;
            if (headht != null)
            {
                foreach (DictionaryEntry item in headht)
                {
                    request.Headers.Add(item.Key.ToString(), item.Value.ToString());
                }
            }

            try
            {
                response = request.GetResponse();

                if (response != null)
                {
                    StreamReader reader = new StreamReader(response.GetResponseStream(), encodeing);
                    responseStr = reader.ReadToEnd();
                    reader.Close();
                }
            }
            catch (Exception)
            {
                throw;
            }
            return responseStr;
        }
        #endregion


        public static string GetLocation(string ip)
        {
            string res = "";
            try
            {
                string url = "http://apis.juhe.cn/ip/ip2addr?ip=" + ip + "&dtype=json&key=b39857e36bee7a305d55cdb113a9d725";
                res = HttpGet(url);
                var resjson = res.ToObject<objex>();
                res = resjson.result.area + " " + resjson.result.location;
            }
            catch
            {
                res = "";
            }
            if (!string.IsNullOrEmpty(res))
            {
                return res;
            }
            try
            {
                string url = "https://sp0.baidu.com/8aQDcjqpAAV3otqbppnN2DJv/api.php?query=" + ip + "&resource_id=6006&ie=utf8&oe=gbk&format=json";
                res = HttpGet(url, Encoding.GetEncoding("GBK"));
                var resjson = res.ToObject<obj>();
                res = resjson.data[0].location;
            }
            catch
            {
                res = "";
            }
            return res;
        }


        #region Ip(获取Ip)
        /// <summary>
        /// 获取Ip
        /// </summary>
        public static string GetIp()
        {
            var result = string.Empty;
            if (HttpContext.Current != null)
                result = GetWebClientIp();
            if (result.IsEmpty())
                result = GetLanIp();
            return result;
        }

        /// <summary>
        /// 获取Web客户端的Ip
        /// </summary>
        private static string GetWebClientIp()
        {
            var ip = GetWebRemoteIp();
            foreach (var hostAddress in Dns.GetHostAddresses(ip))
            {
                if (hostAddress.AddressFamily == AddressFamily.InterNetwork)
                    return hostAddress.ToString();
            }
            return string.Empty;
        }

        /// <summary>
        /// 获取Web远程Ip
        /// </summary>
        private static string GetWebRemoteIp()
        {
            return HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"] ?? HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
        }

        /// <summary>
        /// 获取局域网IP
        /// </summary>
        private static string GetLanIp()
        {
            foreach (var hostAddress in Dns.GetHostAddresses(Dns.GetHostName()))
            {
                if (hostAddress.AddressFamily == AddressFamily.InterNetwork)
                    return hostAddress.ToString();
            }
            return string.Empty;
        }

        #endregion

    }
    public static class Json
    {
        public static T ToObject<T>(this string Json)
        {
            return Json == null ? default(T) : JsonConvert.DeserializeObject<T>(Json);
        }
    }
    /// <summary>
    /// 百度接口
    /// </summary>
    public class obj
    {
        public List<dataone> data { get; set; }
    }
    public class dataone
    {
        public string location { get; set; }
    }
    /// <summary>
    /// 聚合数据
    /// </summary>
    public class objex
    {
        public string resultcode { get; set; }
        public dataoneex result { get; set; }
        public string reason { get; set; }
        public string error_code { get; set; }
    }
    public class dataoneex
    {
        public string area { get; set; }
        public string location { get; set; }
    }
}
