using Blog.Models;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace Blog.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class AdminController : BaseController
    {
        /// <summary>
        /// 管理员首页
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            // 博客
            ViewBag.BlogNums = db.Blogs.Count(p => p.IsRelease == true);
            // 类别
            ViewBag.Categories = db.Categories.Count(p => p.User == "system");
            // 用户
            ViewBag.UserNums = db.Users.Count()-2;
            // 意见反馈
            ViewBag.Feedback = db.Letters.Count(p => p.To == "admin@blog.ydath.cn"&& p.Reply == null);
            // 系统报错
            ViewBag.SysErrors = db.Logs.Count(p => p.LogType == LogType.error.ToString() && p.IsRead == false);
            return View();
        }

       /// <summary>
       /// 博客管理
       /// </summary>
       /// <returns></returns>
        public ActionResult ManageBlogs(string searchStr, int page = 1)
        {
            ViewBag.Page = page;
            ViewBag.Categories = db.Categories.Where(p => p.User == "system").ToList();
            if (string.IsNullOrWhiteSpace(searchStr))
            {
                return View(db.Blogs.Include("Author").Where(p => p.IsPulish == true && p.IsRelease == true).OrderByDescending(p => p.Id).Select(p => new AdminManageBlog() { Author = p.Author, EidtTime = p.EidtTime, Id = p.Id, LookNum = p.LookNum, Title = p.Title, BlogType = p.BlogType, IsPush = p.IsPush, Category = p.Categories.FirstOrDefault(x => x.User =="system") }).ToPagedList(page, pageSize * 3));
            }
            else
            {
                ViewBag.SearchStr = searchStr;
                return View(db.Blogs.Include("Author").Where(p => p.IsPulish == true && p.IsRelease == true && p.Title.Contains(searchStr)).OrderByDescending(p => p.Id).Select(p => new AdminManageBlog() { Author = p.Author, EidtTime = p.EidtTime, Id = p.Id, LookNum = p.LookNum, Title = p.Title, BlogType = p.BlogType, IsPush = p.IsPush, Category = p.Categories.FirstOrDefault(x => x.User == "system") }).ToPagedList(page, pageSize * 3));
            }
        }

        /// <summary>
        /// 添加博客到类别
        /// </summary>
        /// <param name="blogId"></param>
        /// <param name="categoryId"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        public ActionResult AddBlogToCategory(int blogId, int categoryId, int page=1)
        {
            var blog = db.Blogs.Find(blogId);
            if (blog == null)
            {
                return Content($"<script>alert('没有该博客！');location.href='/Admin/ManageBlogs?page={page}'</script>");
            }
            var category = db.Categories.Find(categoryId);
            if (category == null)
            {
                return Content($"<script>alert('没有该类别！');location.href='/Admin/ManageBlogs?page={page}'</script>");
            }
            
            // 清除旧的系统类别
            List<Category> oldCategories = blog.Categories.ToList();
            oldCategories.RemoveAll(p => p.User == "system");
            blog.Categories.Clear();
            blog.Categories = oldCategories;

            category.Blogs.Add(blog);
            category.BlogCount = category.Blogs.Count();
            db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
            db.Entry(category).State = System.Data.Entity.EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("ManageBlogs",new { page = page });
        }
        
        /// <summary>
        /// 将违规博客设置为不公开
        /// </summary>
        /// <param name="id">博客Id</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SetPrivateBlog(int id)
        {
            var entity = db.Blogs.Find(id);
            if (entity == null)
            {
                return Json(false);
            }
            entity.IsPulish = false;
            db.SaveChanges();
            // 记录
            string email = entity.Author.Email;
            string content = "由于您的博客《" + entity.Title + "》内容不够正能量, 届时已被设置为非公开! 详情请咨询 admin.blog@ydath.cn。";
            SeedMessage(email, "博客被设为私有", content);
            SeedEmail(entity.Author.Email, "博客被设为私有", content);
            LogHelper("博客设为私有成功", LogType.danger.ToString(), "将博客《<a href='/Users/BlogDetails/"+entity.Id+"'>" + entity.Title + "'</a>》设为私有成功!", User.Identity.Name, IpHelper.GetIp());
            return Json(true);
        }

        /// <summary>
        /// 移除首页
        /// </summary>
        /// <param name="id">博客Id</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult RemoveHome(int id)
        {
            var entity = db.Blogs.Find(id);
            if (entity == null)
            {
                return Json(false);
            }
            entity.IsPush = false;
            db.SaveChanges();
            // 记录
            string email = entity.Author.Email;
            string content = "由于您的博客《" + entity.Title + "》内容不符合达不到一定水准, 届时已被移除首页! 详情请咨询 admin.blog@ydath.cn。";
            SeedMessage(email, "博客被移除首页", content);
            SeedEmail(entity.Author.Email, "博客被移除首页", content);
            LogHelper("博客移除首页成功", LogType.danger.ToString(), "将博客《<a href='/Users/BlogDetails/" + entity.Id + "'>" + entity.Title + "'</a>》移除首页成功!", User.Identity.Name, IpHelper.GetIp());
            return Json(true);
        }

        /// <summary>
        /// 删除违规博客
        /// </summary>
        /// <param name="id">博客Id</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult DeleteBlog(int id)
        {
            var entity = db.Blogs.Find(id);
            if (entity ==null)
            {
                return Json(false);
            }
            string email = entity.Author.Email;
            // 记录
            string content = "由于您的博客《" + entity.Title + "》内容不符合相关规范,影响非常恶劣, 届时已被删除! 详情请咨询 admin.blog@ydath.cn。";
            SeedMessage(email, "博客被删除", content);
            SeedEmail(email, "博客被删除", content);
            LogHelper("删除博客成功", LogType.danger.ToString(), "删除博客《" + entity.Title + "》成功!", User.Identity.Name, IpHelper.GetIp());
            db.Blogs.Remove(entity);
            db.SaveChanges();
            return Json(true);
        }

        /// <summary>
        /// 意见反馈
        /// </summary>
        /// <returns></returns>
        public ActionResult Feedback(int page = 1)
        {
            var model = db.Letters.Where(p => p.To == "admin@blog.ydath.cn").OrderByDescending(p => p.Id).ToPagedList(page, pageSize);
            return View(model);
        }

        /// <summary>
        /// 回复反馈
        /// </summary>
        /// <param name="id">反馈消息Id</param>
        /// <param name="reply">回复内容</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult FeedReply(int id, string reply)
        {
            var model = db.Letters.Find(id);
            if (model == null)
            {
                ViewBag.Content = "反馈消息不存在!";
                return View($"NotFound");
            }
            else if (string.IsNullOrWhiteSpace(reply))
            {
                return Content("<script>alert('内容不能为空!');location.href='/Admin/Feedback'</script>");
            }
            else
            {
                model.Reply = reply;
                model.IsRead = true;
                db.SaveChanges();

                // 回复
                return Content(SeedMessage(model.From.Email, "感谢反馈", reply) ? "<script>alert('回复成功!');location.href='/Admin/Feedback';</script>" : "<script>alert('回复失败, 请重试!');location.href='/Admin/Feedback';</script>");
            }
        }

        /// <summary>
        /// 删除意见反馈
        /// </summary>
        /// <param name="id">消息Id</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult DeleteFeedBack(int id)
        {
            var model = db.Letters.Find(id);
            if (model == null)
            {
                return Json(false);
            }

            db.Letters.Remove(model);
            db.SaveChanges();
            return Json(true);
        }

        /// <summary>
        /// 用户管理
        /// </summary>
        /// <returns></returns>
        public ActionResult ManageUsers(int page = 1)
        {
            // 去除管理员
            var admin = db.UserLogins.Where(p => p.Role.Name == "Administrator");
            return View(db.UserLogins.Except(admin).OrderByDescending(p => p.Id).ToPagedList(page, pageSize * 3));
        }
        
        /// <summary>
        /// 禁言非法用户
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SetGogUser(int id)
        {
            var entity = db.UserLogins.Find(id);
            if (entity ==null)
            {
                return Json(false);
            }

            entity.Gog = !entity.Gog;
            db.SaveChanges();
            // 记录
            SeedEmail(entity.Email, "被禁止登陆", "由于您的操作不符合相关规范,带了了恶劣影响，所以已被禁止登陆,详情请咨询 admin.blog@ydath.cn。");
            LogHelper("禁言用户成功", LogType.danger.ToString(), "禁言了用户：" + entity.Email, User.Identity.Name, IpHelper.GetIp());
            return Json(true);
        }

        /// <summary>
        /// 删除非法用户
        /// </summary>
        /// <param name="id">用户Id</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult DeleteUser(int id)
        {
            var model = db.UserLogins.Find(id);
            if (model == null)
            {
                return Json(false);
            }

            var user = db.Users.FirstOrDefault(p => p.Email == model.Email);
            if (user != null || model.RCode == "")
            {
                LogHelper("删除用户失败", LogType.danger.ToString(), "用户已注册成功!不能删除,只能禁言,该功能只提供来删除无法认证的邮箱!", User.Identity.Name, IpHelper.GetIp());
                return Json(false);
            }

            // 记录
            SeedEmail(model.Email, "被删除", "由于您的行为不符合相关规范,带了了恶劣影响，所以已被删除,详情请咨询 admin.blog@ydath.cn。");
            LogHelper("删除用户成功", LogType.danger.ToString(), "删除了用户：" + model.Email, User.Identity.Name, IpHelper.GetIp());

            db.UserLogins.Remove(model);
            db.SaveChanges();
            return Json(true);
        }

        /// <summary>
        /// 返回指定用户信息
        /// </summary>
        /// <param name="id">用户Id</param>
        [HttpPost]
        public ActionResult ShowUserDetails(int id)
        {
            // 序列化用户信息:只包含昵称、信条、标签和公开博客
            string email = db.UserLogins.Find(id).Email;
            var model = db.Users.Where(p => p.Email == email).Select(p => new { Id = p.Id, NickName = p.NickName, Belief= p.Belief, CategoryNum = db.Categories.Count(x => x.User == p.Email), BlogNum = p.BlogCount }).FirstOrDefault();
            return model == null ? Json(false) : Json((new JavaScriptSerializer()).Serialize(model));
        }

        /// <summary>
        /// 公告管理
        /// </summary>
        /// <returns></returns>
        public ActionResult ManageSysNews(int page = 1)
        {
            return View(db.SysNews.OrderByDescending(p => p.Id).ToPagedList(page, pageSize*3));
        }

        /// <summary>
        /// 添加公告
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult AddSysNews()
        {
            return View();
        }

        [HttpPost]
        [ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public ActionResult AddSysNews([Bind(Include ="Title,Content")]SysNews model)
        {
            if (!ModelState.IsValid) return View(model);
            db.SysNews.Add(new SysNews() {Title = model.Title, Content = model.Content, CreateTime = DateTime.Now});
            db.SaveChanges();
            return Content("<Script>alert('发布成功!');location.href='/Admin/ManageSysNews';</Script>");
        }

        /// <summary>
        /// 校验公告标题
        /// </summary>
        /// <param name="title">公告标题</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult CheckSysNewsTitle(string title)
        {
            return Json(!db.SysNews.Any(p => p.Title == title));
        }

        /// <summary>
        /// 类别管理
        /// </summary>
        /// <returns></returns>
        public ActionResult ManageCategories()
        {
            ViewBag.Categories = db.Categories.Where(p => p.User == "system").OrderByDescending(p => p.BlogCount);
            return View();
        }

        /// <summary>
        /// 添加类别
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateCategory(CreateCategory model)
        {
            if (ModelState.IsValid)
            {
                if (db.Categories.Any(p => p.Name == model.Name && p.User == "system"))
                {
                    ModelState.AddModelError("", "已有该类别！");
                    LogHelper("添加类别失败", LogType.danger.ToString(), "已有该类别！客户端逃过了Js验证.", User.Identity.Name, IpHelper.GetIp());
                    return View("ManageCategories");
                }
                Category entity = new Category()
                {
                    Name = model.Name,
                    User = "system",
                    BlogCount = 0
                };
                db.Categories.Add(entity);
                db.SaveChanges();
                return RedirectToAction("ManageCategories");
            }
            return View("ManageCategories");
        }

        /// <summary>
        /// 检查类别名
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult CheckCategoryName(string name)
        {
            if (db.Categories.Any(p => p.Name == name && p.User == "system"))
            {
                return Json(false);
            }
            else
            {
                return Json(true);
            }
        }

        /// <summary>
        /// 编辑类别
        /// </summary>
        /// <param name="id">类别Id</param>
        /// <param name="name">类别名</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditCategory(int id, string name)
        {
            var entity = db.Categories.Find(id);
            if (entity ==null)
            {
                LogHelper("修改类别失败", LogType.danger.ToString(), "没有找到该类别！客户端修改了Id，或者读到了脏数据.", User.Identity.Name, IpHelper.GetIp());
                return Content("<script>alert('没有该类别！');location.href='/Admin/ManageCategories';</script>");
            }
            entity.Name = name;
            db.Entry(entity).State = System.Data.Entity.EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("ManageCategories");
        }

        /// <summary>
        /// 删除类别
        /// </summary>
        /// <param name="id">类别Id</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult DeleteCategory(int id)
        {
            var entity = db.Categories.Find(id);
            if (entity == null || entity.BlogCount > 1)
            {
                return Json(false);
            }

            string name = entity.Name;
            db.Categories.Remove(entity);
            db.SaveChanges();

            // 记录
            LogHelper("删除类别成功", LogType.danger.ToString(), "删除类别<" + name + ">成功!", User.Identity.Name, IpHelper.GetIp());
            return Json(true);
        }

        /// <summary>
        /// 日志管理
        /// </summary>
        /// <returns></returns>
        public ActionResult ManageLogs(string logType, int page = 1)
        {
            var model = from x in db.Logs select x;
            if (!string.IsNullOrWhiteSpace(logType) && logType != "all")
            {
                model = model.Where(p => p.LogType == logType);
            }
            ViewBag.LogType = logType;
            var log = model.OrderByDescending(p => p.Id).ToPagedList(page, pageSize * 10);

            foreach (var item in log)
            {
                // 写IP所在地区
                if (item.IpLocation == null || item.IpLocation.Length <= 0)
                    item.IpLocation = IpHelper.GetLocation(item.Ip);

                // 标记已读
                if (item.IsRead != true)
                    item.IsRead = true;
            }
            if (log.Any())
            {
                db.SaveChanges();
            }
            return View(log);
        }

        /// <summary>
        /// 获取博客图片
        /// </summary>
        /// <returns></returns>
        public ActionResult GetPic()
        {
            var blog = db.Blogs.ToList();
            string content = "";

            foreach (var item in blog)
            {
                MatchCollection mc = Regex.Matches(item.Content, "<img\\s*src=\"https://blog.ydath.cn:443/UploadFile/([^\"]*)\"");
                foreach (Match match in mc)
                {
                    content += match.Groups[1].Value;
                }
                MatchCollection mc1 = Regex.Matches(item.Content, "<img\\s*src=\"https://blog.ydath.cn/UploadFile/([^\"]*)\"");
                foreach (Match match in mc1)
                {
                    content += match.Groups[1].Value;
                }
            }
            return Content(content + "web.config");
        }
    }
}