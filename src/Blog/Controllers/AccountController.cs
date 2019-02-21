using BlogPlus.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace BlogPlus.Controllers
{
    public class AccountController : BaseController
    {
        /// <summary>
        /// 展示登录页面
        /// </summary>
        /// <returns>登录页面</returns>
        [HttpGet]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        /// <summary>
        /// 处理登录
        /// </summary>
        /// <param name="model">登录数据</param>
        /// <returns>登录结果:模型校验失败, 邮箱或密码错误, 验证码错误, 用户不存在, 账号被锁, 邮箱没激活, 登录成功</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginModel model)
        {
            if (ModelState.IsValid)
            {
                //if (CheckCode(model.Code))
                //{
                //    model.Code = "";
                //    ModelState.AddModelError("Code", "验证码错误!");
                //    return View(model);
                //}

                string password = GetMD5(model.Password, model.Email);
                var entity = db.UserLogins.FirstOrDefault(p => p.Email == model.Email && p.Password == password);
                if (entity == null)
                {
                    entity = db.UserLogins.FirstOrDefault(p => p.Email == model.Email);
                    if (entity != null)
                    {
                        if (entity.LockingTime.AddMinutes(30) > DateTime.Now)
                        {
                            LogHelper("登录失败", LogType.login.ToString(), "账号被锁了仍然在尝试登录,并且密码不正确!", model.Email, IpHelper.GetIp());
                            //model.Code = "";
                            ModelState.AddModelError("Email", "该账号已被锁住!请稍后重试.");
                            return View(model);
                        }
                        else if (entity.LockingTime.AddMinutes(30) < DateTime.Now && entity.ErrorTimes >= 5)
                        {
                            entity.ErrorTimes = 0;
                        }

                        entity.ErrorTimes++;
                        if (entity.ErrorTimes >= 5)
                        {
                            entity.LockingTime = DateTime.Now;
                        }

                        db.Entry<UserLogin>(entity).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();

                        //model.Code = "";
                        LogHelper("登录失败", LogType.login.ToString(), "密码错误!", model.Email, IpHelper.GetIp());
                        ModelState.AddModelError("", "邮箱或密码错误!连续5次错误账号将被锁住30分钟,请谨慎操作.");
                        return View(model);
                    }
                    //model.Code = "";
                    LogHelper("登录失败", LogType.login.ToString(), "账号不存在!", model.Email, IpHelper.GetIp());
                    ModelState.AddModelError("", "邮箱或密码错误!连续5次错误账号将被锁住30分钟,请谨慎操作.");
                    return View(model);
                }
                else
                {
                    if (entity.Gog == true)
                    {
                        LogHelper("登录失败", LogType.login.ToString(), "账号被禁了仍然在尝试登录!", model.Email, IpHelper.GetIp());
                        ModelState.AddModelError("Email", "该账户已被禁!!!");
                        return View(model);
                    }
                    if (entity.RCode != "")
                    {
                        //model.Code = "";
                        LogHelper("登录失败", LogType.login.ToString(), "账号未激活!", model.Email, IpHelper.GetIp());
                        ModelState.AddModelError("Email", "邮箱尚未激活!请前往邮箱激活账号.");
                        return View(model);
                    }
                    else if(entity.LockingTime.AddMinutes(30) > DateTime.Now)
                    {
                        //model.Code = "";
                        LogHelper("登录失败", LogType.login.ToString(), "账号被锁了仍然在尝试登录,输入账号密码正确!", model.Email, IpHelper.GetIp());
                        ModelState.AddModelError("Email", "该账号已被锁住! 请稍后重试.");
                        return View(model);
                    }
                    else
                    {
                        entity.ErrorTimes = 0;
                        db.Entry<UserLogin>(entity).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();

                        string role = entity.Role.Name;
                        FormsAuthenticationTicket authTicket = new FormsAuthenticationTicket(
                            1,
                            model.Email,
                            DateTime.Now,
                            DateTime.Now.AddYears(1),
                            true,
                            role
                           );
                        string encryptedTicket = FormsAuthentication.Encrypt(authTicket);
                        HttpCookie authCookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket);
                        System.Web.HttpContext.Current.Response.Cookies.Add(authCookie);

                        if (Url.IsLocalUrl(model.ReturnUrl))
                        {
                            return Redirect(model.ReturnUrl);
                        }

                        LogHelper("登录成功", LogType.login.ToString(), "登录成功!", model.Email, IpHelper.GetIp());
                        return Redirect("/");
                    }
                }
            }
            //model.Code = "";
            return View(model);
        }

        /// <summary>
        /// 展示注册页面
        /// </summary>
        /// <returns>注册页面</returns>
        [HttpGet]
        public ActionResult Register()
        {
            return View();
        }

        /// <summary>
        /// 处理注册请求
        /// </summary>
        /// <param name="model">页面上传数据</param>
        /// <returns>处理结果: 模型校验失败, 验证码错误, 账号已注册, 邮件发送失败, 注册成功</returns>
        [HttpPost]
        public ActionResult Register(RegisterModel model)
        {
            if (ModelState.IsValid)
            {
                //if (CheckCode(model.Code))
                //{
                //    model.Code = "";
                //    ModelState.AddModelError("Code", "验证码错误!");
                //    return View(model);
                //}

                var entity = db.UserLogins.FirstOrDefault(p => p.Email == model.Email);

                if (entity!=null)
                {
                    if (entity.RCode == "")
                    {
                        //model.Code = "";
                        LogHelper("注册失败", LogType.register.ToString(), "账号已被注册!", model.Email, IpHelper.GetIp());
                        ModelState.AddModelError("Email", "该账号已注册!");
                        return View(model);
                    }
                    else
                    {
                        entity.Password = GetMD5(model.Password, model.Email);
                        entity.RCode = Guid.NewGuid().ToString();
                        entity.Invalid = DateTime.Now;
                        entity.Role = entity.Role;
                    }
                }
                else
                {
                    entity = new UserLogin();
                    entity.Email = model.Email;
                    entity.RCode = Guid.NewGuid().ToString();
                    entity.Password = GetMD5(model.Password, model.Email);
                    entity.Invalid = DateTime.Now;
                    entity.ErrorTimes = 0;
                    entity.Role = db.Roles.FirstOrDefault(p => p.Name == "User");
                    entity.LockingTime = DateTime.Parse("1990-01-01");
                }

                string url = new UriBuilder(Request.Url)
                {
                    Path = "/Account/Actiation/"+ entity.RCode,
                    Query = ""
                }.ToString();
                var content = "欢迎注册本站，请点击 <a target='_blank' href='" + url + "'> 此处 </a> 来激活你的账号, 若不能跳转, 请将以下链接复制到浏览器地址栏中进行跳转:<br/>" + url;
                if (!SeedEmail(model.Email, "激活邮件", content))
                {
                    //model.Code = "";
                    LogHelper("注册失败", LogType.register.ToString(), "发送邮件失败!", model.Email, IpHelper.GetIp());
                    ModelState.AddModelError("", "邮件发送失败! 请重试.");
                    return View(model);
                }

                //保存前判断是否已有
                if (db.UserLogins.Count(p => p.Email == entity.Email) == 0)
                {
                    db.UserLogins.Add(entity);
                }
                else
                {
                    db.Entry<UserLogin>(entity).State = System.Data.Entity.EntityState.Modified;
                }
                db.SaveChanges();
                LogHelper("注册成功", LogType.register.ToString(), "发送邮件成功，但是用户并未激活!RCode:"+entity.RCode, model.Email, IpHelper.GetIp());
                return Content("<Script>alert('邮件已发送!请到邮箱查收邮件来激活账户,链接24小时之内有效.');location.href='/';</Script>");
            }
            //model.Code = "";
            return View(model);
        }

        /// <summary>
        /// 处理激活账号请求
        /// </summary>
        /// <param name="id">激活code</param>
        /// <returns>处理结果: code为空, code已过期, 激活成功</returns>
        [HttpGet]
        public ActionResult Actiation(string id)
        {
            var model = db.UserLogins.FirstOrDefault(p => p.RCode == id);
            if (model == null)
            {
                LogHelper("注册失败", LogType.danger.ToString(), "激活链接并不存在!RCode:" + id, "游客", IpHelper.GetIp());
                return Content("<Script>alert('激活失败!请重新注册.');location.href='/Account/Register';</Script>");
            }
            else if(model.Invalid.AddDays(1) < DateTime.Now)
            {
                LogHelper("注册失败", LogType.register.ToString(), "激活链接已过期!RCode:" + id, model.Email, IpHelper.GetIp());
                return Content("<Script>alert('链接已过期!请重新注册.');location.href='/Account/Register';</Script>");
            }
            else
            {
                char[] character = { '1', '2', '3', '4', '5', '6', '7', '8', '9', 'a', 'b', 'd', 'e', 'f', 'h', 'k', 'm', 'n', 'r', 'x', 'y', 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'J', 'K', 'L', 'M', 'N', 'P', 'R', 'S', 'T', 'W', 'X', 'Y' };
                Random rnd = new Random();

                User entity = new Models.User();
                entity.Email = model.Email;
                for (int i = 0; i < 8; i++)
                {
                    entity.NickName += character[rnd.Next(character.Length)];
                }
                entity.NickName += DateTime.Now.Year;
                model.RCode = "";
                entity.FriendLink = "友情链接一-----http://blog.ydath.cn";
                entity.IsPubulish = true;
                entity.JoinTime = DateTime.Now;
                entity.LookNum = 0;
                entity.SchoolName = "SICAU";
                entity.PicLink = DateTime.Now.Second % 2 == 0 ? "/icon/DH.png" : "/icon/GH.jpg";
                db.Users.Add(entity);
                db.Entry<UserLogin>(model).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();

                LogHelper("注册成功", LogType.register.ToString(), "激活成功!RCode:" + id, model.Email, IpHelper.GetIp());
                return Content("<Script>alert('激活成功!请登录.');location.href='/Account/Login';</Script>");
            }
        }

        /// <summary>
        /// 展示忘记密码页面
        /// </summary>
        /// <returns>忘记密码页面</returns>
        [HttpGet]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        /// <summary>
        /// 处理忘记密码
        /// </summary>
        /// <param name="model">页面上传数据</param>
        /// <returns>处理结果: 模型校验失败, 验证码错误, 邮箱不存在, 账号被锁, 账户尚未激活, 邮件发送失败, 申请成功</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ForgotPassword(ForgotPassword model)
        {
            if (ModelState.IsValid)
            {
                //if(CheckCode(model.Code))
                //{
                //    model.Code = "";
                //    ModelState.AddModelError("Code", "验证码错误!");
                //    return View(model);
                //}

                var entity = db.UserLogins.FirstOrDefault(p => p.Email == model.Email);
                if (entity == null)
                {
                    //虽然不存在, 但还是发同样的邮件
                    LogHelper("忘记密码失败", LogType.danger.ToString(), "邮箱并不存在!", model.Email, IpHelper.GetIp());
                    return Content("<Script>alert('邮件已发送!请到邮箱查收邮件进行重置密码,链接24小时之内有效.');Location.href='/';</Script>");
                }
                else
                {
                    if (entity.Gog == true)
                    {
                        LogHelper("忘记密码失败", LogType.login.ToString(), "账户已被禁!", model.Email, IpHelper.GetIp());
                        ModelState.AddModelError("Email", "该账户已被禁!!!");
                        return View(model);
                    }

                    if (entity.LockingTime.AddMinutes(30) > DateTime.Now)
                    {
                        //model.Code = "";
                        LogHelper("忘记密码失败", LogType.login.ToString(), "账户已被锁住!", model.Email, IpHelper.GetIp());
                        ModelState.AddModelError("", "该账号已被锁住!请稍后重试.");
                        return View(model);
                    }
                    else if (entity.RCode != "")
                    {
                        //model.Code = "";
                        LogHelper("忘记密码失败", LogType.login.ToString(), "账户尚未激活!", model.Email, IpHelper.GetIp());
                        ModelState.AddModelError("Email", "该用户尚未激活!请前往邮箱激活账号或重新注册.");
                        return View(model);
                    }
                    else
                    {
                        entity.FCode = Guid.NewGuid().ToString();
                        var url = new UriBuilder(Request.Url)
                        {
                            Path = "/Account/ResetPassword/" + entity.FCode,
                            Query = ""
                        }.ToString();

                        string content = "请点击 <a target='_blank' href='" + url + "'> 此处 </a> 来重置密码.若无法跳转, 请将以下链接复制到浏览器地址栏进行跳转:<br/>" + url;
                        if (!SeedEmail(model.Email, "重置密码", content))
                        {
                            ModelState.AddModelError("", "邮件发送失败!请重试.");
                            //model.Code = "";
                            return View(model);
                        }

                        entity.Invalid = DateTime.Now;
                        db.Entry<UserLogin>(entity).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();

                        LogHelper("忘记密码成功", LogType.login.ToString(), "发送链接成功!FCode:"+entity.FCode, model.Email, IpHelper.GetIp());
                        return Content("<Script>alert('邮件已发送!请到邮箱查收邮件进行重置密码,链接24小时之内有效.');location.href='/';</Script>");
                    }
                }

            }
            return View();
        }

        /// <summary>
        /// 展示重置密码界面
        /// </summary>
        /// <param name="id">重置密码code</param>
        /// <returns>重置密码页面</returns>
        [HttpGet]
        public ActionResult ResetPassword(string id)
        {
            var model = db.UserLogins.FirstOrDefault(p => p.FCode == id);
            if (model == null)
            {
                LogHelper("重置密码失败", LogType.danger.ToString(), "链接不存在!FCode:" + id, "游客", IpHelper.GetIp());
                return Content("<Script>alert('处理失败!请重新申请.');location.href='/Account/ForgotPassword';</Script>");
            }
            else if (model.Invalid.AddDays(1) < DateTime.Now)
            {
                LogHelper("重置密码失败", LogType.login.ToString(), "链接已过期!FCode:" + id + id, model.Email, IpHelper.GetIp());
                return Content("<Script>alert('链接已过期!请重新申请.');location.href='/Account/ForgotPassword';</Script>");
            }
            else
            {
                ViewBag.FCode = id;
                return View();
            }
        }

        /// <summary>
        /// 处理重置密码
        /// </summary>
        /// <param name="model">页面上传数据</param>
        /// <returns>处理结果: 模型校验失败, 验证按错误, code错误或不存在, code已过期, 重置密码成功</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ResetPassword(ResetPassword model)
        {
            if (ModelState.IsValid)
            {
                //if (CheckCode(model.Code))
                //{
                //    model.Code = "";
                //    ModelState.AddModelError("Code", "验证码错误!");
                //    return View(model);
                //}

                var entity = db.UserLogins.FirstOrDefault(p => p.FCode == model.FCode);
                if (entity == null)
                {
                    //model.Code = "";
                    LogHelper("重置密码失败", LogType.danger.ToString(), "code不存在!FCode:" + model.FCode, "游客", IpHelper.GetIp());
                    ModelState.AddModelError("", "处理异常!请重试.");
                    return View(model);
                }
                else if (entity.Invalid.AddDays(1) < DateTime.Now)
                {
                    LogHelper("重置密码失败", LogType.login.ToString(), "code已过期!FCode:" + model.FCode, entity.Email, IpHelper.GetIp());
                    return Content("<Script>alert('票证已过期!请重新申请.');location.href='/Account/ForgotPassword';</Script>");
                }
                else
                {
                    entity.FCode = "";
                    entity.Password = GetMD5(model.Password, entity.Email);

                    db.Entry<UserLogin>(entity).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();

                    LogHelper("重置密码成功", LogType.login.ToString(), "重置密码成功!FCode:" + model.FCode, entity.Email, IpHelper.GetIp());
                    return Content("<Script>alert('密码重置成功!请登录.');location.href='/Account/Login';</Script>");
                }
            }
            return View(model);
        }

        /// <summary>
        /// 展示更改密码页面
        /// </summary>
        /// <returns>更改密码页面</returns>
        [HttpGet]
        public ActionResult ChangePassword()
        {
            return View();
        }

        /// <summary>
        /// 处理更改密码
        /// </summary>
        /// <param name="model">页面上传的数据</param>
        /// <returns>处理结果: 模型校验失败, 验证码错误, 旧密码错误, </returns>
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword(ChangePassword model)
        {
            if (ModelState.IsValid)
            {
                //if (CheckCode(model.Code))
                //{
                //    model.Code = "";
                //    ModelState.AddModelError("Code", "验证码错误!");
                //    return View(model);
                //}

                string password = GetMD5(model.OldPassword, User.Identity.Name);
                var entity = db.UserLogins.FirstOrDefault(p => p.Password == password);

                if (entity == null)
                {
                    var user = db.UserLogins.FirstOrDefault(p => p.Email == User.Identity.Name);
                    if (user != null)
                    {
                        user.ErrorTimes++;
                        if (user.ErrorTimes >= 5)
                        {
                            user.LockingTime = DateTime.Now;
                        }

                        db.Entry<UserLogin>(user).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();

                        if (user.ErrorTimes >= 5)
                        {
                            LogHelper("更改密码失败", LogType.login.ToString(), "账户被锁!", user.Email, IpHelper.GetIp());
                            return Content("<Script>alert('账号被锁!已注销登录.');location.href='/Acccount/Logout';</Script>");
                        }

                    }
                    //model.Code = "";
                    LogHelper("更改密码失败", LogType.login.ToString(), "旧密码错误!", user.Email, IpHelper.GetIp());
                    ModelState.AddModelError("OldPassword", "旧密码错误!连续错误5次账号将被锁30分钟!请谨慎操作.");
                    return View(model);
                }
                else
                {
                    entity.Password = GetMD5(model.Password, User.Identity.Name);
                    entity.ErrorTimes = 0;
                    db.Entry<UserLogin>(entity).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();

                    LogHelper("更改密码成功", LogType.login.ToString(), "更改密码成功!", entity.Email, IpHelper.GetIp());
                    return Content("<Script>alert('修改密码成功!请重新登录.'); location.href='/Account/Login';</Script>");
                }
            }
            return View();
        }

        /// <summary>
        /// 校验邮箱
        /// 注册时校验邮箱是否已注册
        /// </summary>
        /// <param name="email">待校验邮箱</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult CheckEmail(string email)
        {
            if (db.UserLogins.Count(p => p.Email == email && p.RCode == "") == 0)
            {
                return Json(true);
            }
            LogHelper("注册失败", LogType.register.ToString(), "异步校验邮箱已注册!", email, IpHelper.GetIp());
            return Json(false);
        }

        /// <summary>
        /// 注销登录
        /// </summary>
        /// <returns>重定向到首页</returns>
        //[HttpPost]
        public ActionResult Logout()
        {
            LogHelper("注销成功", LogType.register.ToString(), "注销成功!", User.Identity.Name, IpHelper.GetIp());
            FormsAuthentication.SignOut();
            Session.RemoveAll();
            Session.Clear();

            return Redirect("/");
        }

        /// <summary>
        /// 初始化系统
        /// 初始化角色和管理员
        /// </summary>
        /// <returns>初始化结果</returns>
        [HttpGet]
        public ActionResult DefultSystem()
        {
            if (!db.Roles.Any())
            {
                List<Role> entity = new List<Role>() { new Role { Name = "User" }, new Role { Name = "Administrator" } };
                db.Roles.AddRange(entity);
                db.SaveChanges();
            }

            if (!db.UserLogins.Any())
            {
                List<User> entity = new List<Models.User>() {
                    new User {
                        Email = "2512303162@qq.com",
                        IsPubulish = true,
                        LookNum = 0,
                        SchoolName = "SICAU",
                        FriendLink = "友情链接一-----http://blog.ydath.cn",
                        PicLink = "/icon/DH.png",
                        JoinTime = DateTime.Now,
                        NickName = "DepingDu",
                        Belief = "那时候我们有梦，关于文学，关于爱情，关于环游世界的旅行; 如今我们深夜饮酒，杯子碰在一起，都是梦破碎的声音。"
                    },
                    new User {
                        Email = "419347932@qq.com",
                        IsPubulish = true,
                        LookNum = 0,
                        SchoolName = "SICAU",
                        FriendLink = "友情链接一-----http://blog.ydath.cn",
                        PicLink = "/icon/GH.jpg",
                        JoinTime = DateTime.Now,
                        NickName = "Gaby",
                        Belief = "医药法律商业工程这些都是高贵的理想，也是维生的必要条件。但诗、美浪漫、爱这些才是我们生存的原因"
                    }
                };
                List<UserLogin> model = new List<UserLogin>()
                {
                    new UserLogin{
                        Email = "2512303162@qq.com",
                        ErrorTimes = 0,
                        RCode = "",
                        Invalid = DateTime.Now,
                        LockingTime = DateTime.Parse("1990-01-01"),
                        Role = db.Roles.FirstOrDefault(p => p.Name == "Administrator"),
                        Password = GetMD5("Dudeping.2016sicau", "2512303162@qq.com")
                    },
                    new UserLogin{
                        Email = "419347932@qq.com",
                        ErrorTimes = 0,
                        RCode = "",
                        Invalid = DateTime.Now,
                        LockingTime = DateTime.Parse("1990-01-01"),
                        Role = db.Roles.FirstOrDefault(p => p.Name == "Administrator"),
                        Password = GetMD5("Gaby.2017blog", "419347932@qq.com")
                    }
                };

                db.Users.AddRange(entity);
                db.UserLogins.AddRange(model);
                db.SaveChanges();
            }

            LogHelper("系统初始化成功", LogType.danger.ToString(), "系统初始化成功!", "admin", IpHelper.GetIp());
            return Content("<Script>alert('系统初始化成功!');location.href='/';</Script>");
        }
    }
}