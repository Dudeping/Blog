using Blog.Models;
using PagedList;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Blog.Controllers
{
    [Authorize]
    public class ManageBlogsController : BaseController
    {
        /// <summary>
        /// 管理博客首页
        /// </summary>
        /// <param name="type">博客类别</param>
        /// <returns></returns>
        public ActionResult Index(string type, string searchStr, int page = 1)
        {
            //固定范围
            if (string.IsNullOrEmpty(type) || type != BlogType.Article.ToString())
            {
                type = "Jotting";
            }
            var model = from x in db.Blogs
                        where x.Author.Email == User.Identity.Name && x.BlogType == type && x.IsRelease == true
                        select x;
            // 搜索
            if (!string.IsNullOrWhiteSpace(searchStr))
            {
                ViewBag.SearchStr = searchStr;
                model = model.Where(p => p.Title.Contains(searchStr));
            }

            // 排序、投影、分页
            var data = model.OrderByDescending(p => p.CreateTime).Select(p => new ManageBlog() { Id = p.Id, IsPulish = p.IsPulish, LookNum = p.LookNum, CreateTime = p.CreateTime, Title = p.Title }).ToPagedList(page, pageSize * 3);
            ViewBag.BlogType = type;

            if (type == BlogType.Article.ToString())
            {
                //文章
                return View("AIndex", data);
            }

            // 消息
            ViewBag.SysNewsNum = GetNewsNum();

            //随笔
            return View(data);
        }

        /// <summary>
        /// 创建博客
        /// </summary>
        /// <param name="type">博客类别</param>
        /// <returns></returns>
        public ActionResult Create(string type)
        {
            if (string.IsNullOrEmpty(type) || type != BlogType.Article.ToString())
            {
                ViewBag.BlogType = "Jotting";
            }
            else
            {
                ViewBag.BlogType = type;
            }
            // 博客类别
            ViewBag.Categories = db.Categories.Where(p => p.User == User.Identity.Name).ToList();
            return View();
        }

        /// <summary>
        /// 处理创建博客
        /// </summary>
        /// <param name="model">页面上传数据</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public ActionResult Create(BlogModel model)
        {
            ViewBag.Categories = db.Categories.Where(p => p.User == User.Identity.Name).ToList();
            if (!ModelState.IsValid) return View(model);
            //检验标签
            List<string> tags = new List<string>();
            try
            {
                if (!String.IsNullOrWhiteSpace(model.Tags))
                {
                    tags.AddRange(model.Tags.Split(new string[] {",", "，"}, StringSplitOptions.RemoveEmptyEntries).Select(item => item.Trim(' ')));
                }
            }
            catch (Exception)
            {
                LogHelper("添加博客失败", LogType.add.ToString(), "标签不合法!Tag:"+model.Tags, User.Identity.Name, IpHelper.GetIp());
                ModelState.AddModelError("Tags", "标签不合法!");
                return View(model);
            }

            // 时间控制 一天最多只能发布20篇, 两次间隔要超过2分钟
            var usr = db.Users.FirstOrDefault(p => p.Email == User.Identity.Name);
            if (usr.CreateNum < 20 && usr.CreateTime.AddMinutes(2) > DateTime.Now)
            {
                LogHelper("添加博客失败", LogType.danger.ToString(), "两次添加博客时间应该间隔低于2分钟!Title:"+model.Title, User.Identity.Name, IpHelper.GetIp());
                ModelState.AddModelError("", "两次添加博客时间应该间隔2分钟!");
                return View(model);
            }
            else if (usr.CreateNum >= 20 && usr.CreateTime.AddDays(1) > DateTime.Now)
            {
                LogHelper("添加博客失败", LogType.danger.ToString(), "一天添加超过20篇", User.Identity.Name, IpHelper.GetIp());
                ModelState.AddModelError("", "一天最多只能添加20篇!");
                return View(model);
            }
            else if (usr.CreateTime.AddDays(1) < DateTime.Now)
            {
                usr.CreateNum = 1;
                usr.CreateTime = DateTime.Now;
                db.SaveChanges();
            }
            else
            {
                usr.CreateTime = DateTime.Now;
                usr.CreateNum++;
                db.SaveChanges();
            }

            // 当前用户
            var user = db.Users.FirstOrDefault(p => p.Email == User.Identity.Name);
            //添加博客
            BlogInfo entity = new BlogInfo
            {
                Author = user,
                BlogType = model.BlogType
            };
            if (model.Content.Contains("onerror"))
            {
                LogHelper("XSS攻击", LogType.danger.ToString(), "博客内容含有onerror恶意代码Conten:" + model.Content, User.Identity.Name, IpHelper.GetIp());
                entity.Content = model.Content.Replace("onerror", "");
            }
            else
            {
                entity.Content = model.Content;
            }
            // 创建摘要
            var brief = System.Text.RegularExpressions.Regex.Replace(HttpUtility.HtmlDecode(entity.Content.Replace("&nbsp;", "")), "<[^>]*>", "");
            if (brief.Length > 150)
            {
                brief = brief.Substring(0, 150);
            }
            entity.Brief = brief;
            entity.CreateTime = DateTime.Now;
            entity.EidtTime = DateTime.Now;
            entity.IsPulish = model.IsPulish;
            entity.IsPush = model.IsPush;
            entity.IsRelease = model.IsRelease;
            entity.LookNum = 0;
            entity.Title = model.Title;
            entity.Tags = string.Join(",", tags);
            // 统计博客数
            if (entity.IsRelease == true)
            {
                user.BlogCount++;
                if (entity.BlogType == BlogType.Jotting.ToString())
                {
                    user.JottingNum++;
                }
                else
                {
                    user.ArticleNum++;
                }
                db.Entry(user).State = EntityState.Modified;
                db.SaveChanges();
            }

            // 保存类别
            foreach (var item in model.Category ?? new int[] { })
            {
                var category = db.Categories.Find(item);
                if (category != null && category.User == User.Identity.Name)
                {
                    category.BlogCount++;
                    if (entity.Categories == null)
                    {
                        entity.Categories = new List<Category>();
                    }
                    entity.Categories.Add(category);
                    db.Entry(category).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }
            
            // 保存博客
            db.Blogs.Add(entity);
            db.SaveChanges();

            LogHelper("添加博客成功", LogType.add.ToString(), "添加博客成功!" + (entity.IsPulish == true ? "<a href='/Users/BlogDetails/" + entity.Id + "'>" + entity.Title + "</a>" : "博客不公开"), User.Identity.Name, IpHelper.GetIp());

            return RedirectToAction("AddSuccess", new { id = entity.Id, blogType = entity.BlogType });
        }

        /// <summary>
        /// 编辑博客
        /// </summary>
        /// <param name="id">博客Id</param>
        /// <returns></returns>
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                LogHelper("编辑博客失败", LogType.danger.ToString(), "修改博客的请求链接中id为null，用户可能存在修改请求链接的行为!", User.Identity.Name, IpHelper.GetIp());
                return View("Error");
            }
            BlogInfo blog = db.Blogs.Find(id);
            if (blog == null)
            {
                LogHelper("编辑博客失败", LogType.danger.ToString(), "博客不存在,用户很可能存在修改请求链接id的行为!", User.Identity.Name, IpHelper.GetIp());
                ViewBag.Content = "博客不存在!";
                return View("NotFound");
            }
            string email = blog.Author.Email;
            if (email != User.Identity.Name) // 身份认证
            {
                LogHelper("编辑博客失败", LogType.danger.ToString(), "用户修改的博客非本人创建的博客,用户极可能存在修改请求链接id的行为!Blog.Author:"+email, User.Identity.Name, IpHelper.GetIp());
                return View("NoAccess");
            }
            BlogModel model = new BlogModel
            {
                Id = blog.Id,
                BlogType = blog.BlogType,
                Content = blog.Content,
                IsPulish = blog.IsPulish,
                IsPush = blog.IsPush,
                IsRelease = blog.IsRelease,
                Tags = blog.Tags,
                Title = blog.Title,
                Category = blog.Categories.Where(p => p.User != "system").Select(p => p.Id).ToArray()
            };
            ViewBag.Categories = db.Categories.Where(p => p.User == User.Identity.Name).ToList();

            return View(model);
        }

        /// <summary>
        /// 编辑博客
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(BlogModel model)
        {
            ViewBag.Categories = db.Categories.Where(p => p.User == User.Identity.Name).ToList();
            if (ModelState.IsValid)
            {
                //检验标签
                List<string> tags = new List<string>();
                try
                {
                    if (!string.IsNullOrWhiteSpace(model.Tags))
                    {
                        tags.AddRange(model.Tags.Split(new string[] {",", "，"}, StringSplitOptions.RemoveEmptyEntries).Select(item => item.Trim(' ')));
                    }
                }
                catch (Exception)
                {
                    LogHelper("编辑博客失败", LogType.add.ToString(), "标签不合法!Tag:" + model.Tags, User.Identity.Name, IpHelper.GetIp());
                    ModelState.AddModelError("Tags", "标签不合法!");
                    return View(model);
                }

                var entity = db.Blogs.Find(model.Id);
                if (entity == null)
                {
                    LogHelper("编辑博客失败", LogType.danger.ToString(), "博客不存在,用户很可能存在修改请求链接id的行为!", User.Identity.Name, IpHelper.GetIp());
                    ViewBag.Content = "博客不存在!";
                    return View($"NotFound");
                }
                // 身份认证
                string email = entity.Author.Email;
                if (email != User.Identity.Name)
                {
                    LogHelper("编辑博客失败", LogType.danger.ToString(), "用户修改的博客非本人创建的博客,用户极可能存在修改请求链接id的行为!Blog.Author:" + email, User.Identity.Name, IpHelper.GetIp());
                    return View($"NoAccess");
                }

                entity.Title = model.Title;
                if (model.Content.Contains("onerror"))
                {
                    LogHelper("XSS攻击", LogType.danger.ToString(), "博客内容含有onerror恶意代码Conten:" + model.Content, User.Identity.Name, IpHelper.GetIp());
                    entity.Content = model.Content.Replace("onerror", "");
                }
                else
                {
                    entity.Content = model.Content;
                }
                // 创建摘要
                var brief = System.Text.RegularExpressions.Regex.Replace(HttpUtility.HtmlDecode(entity.Content.Replace("&nbsp;", "")), "<[^>]*>", "");
                if (brief.Length > 150)
                {
                    brief = brief.Substring(0, 150);
                }
                entity.Brief = brief;
                entity.EidtTime = DateTime.Now;
                // 统计博客数
                if (entity.IsRelease == false && model.IsRelease == true)
                {
                    var _user = entity.Author;
                    _user.BlogCount++;
                    if (entity.BlogType == BlogType.Jotting.ToString())
                    {
                        _user.JottingNum++;
                    }
                    else
                    {
                        _user.ArticleNum++;
                    }
                    db.Entry(_user).State = EntityState.Modified;
                }
                entity.IsPulish = model.IsPulish;
                entity.IsPush = model.IsPush;
                entity.Tags = string.Join(",", tags);
                if (entity.IsRelease != model.IsRelease)
                {
                    entity.CreateTime = DateTime.Now;
                }
                entity.IsRelease = model.IsRelease;
                // 保存类别
                List<Category> modelCategories = new List<Category>();
                List<Category> entityCategories = entity.Categories.Where(p => p.User != "system").ToList();
                foreach (var item in model.Category ?? new int[] { })
                {
                    var category = db.Categories.Find(item);
                    if (category != null && category.User == User.Identity.Name)
                    {
                        modelCategories.Add(category);
                    }
                }
                var delCategories = entityCategories.Except(modelCategories);
                var addCategories = modelCategories.Except(entityCategories);
                foreach (var item in delCategories)
                {
                    item.BlogCount--;
                    entity.Categories.Remove(item);
                    db.Entry(item).State = EntityState.Modified;
                    db.SaveChanges();
                }
                foreach (var item in addCategories)
                {
                    item.BlogCount++;
                    entity.Categories.Add(item);
                    db.Entry(item).State = EntityState.Modified;
                    db.SaveChanges();
                }

                db.Entry(entity).State = EntityState.Modified;
                db.SaveChanges();

                LogHelper("编辑博客成功", LogType.add.ToString(), "编辑博客成功!", User.Identity.Name, IpHelper.GetIp());

                return RedirectToAction($"EditSuccess", new { id = entity.Id, blogType = entity.BlogType });
            }
            return View(model);
        }

        /// <summary>
        /// 草稿箱
        /// </summary>
        /// <param name="type">博客类别</param>
        /// <returns></returns>
        public ActionResult Draft(string type)
        {
            if (string.IsNullOrEmpty(type) || type != BlogType.Article.ToString())
            {
                type = "Jotting";
            }

            var model = db.Blogs.Where(p => p.BlogType == type && p.IsRelease == false && p.Author.Email == User.Identity.Name);
            ViewBag.BlogType = type;
            return View(model.ToList());
        }

        /// <summary>
        /// 删除博客
        /// </summary>
        /// <param name="id">博客Id</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                LogHelper("删除博客失败", LogType.danger.ToString(), "博客id不存在,用户可能存在修改请求链接id的行为!!", User.Identity.Name, IpHelper.GetIp());
                return Json(false);
            }
            BlogInfo blog = db.Blogs.Find(id);
            if (blog == null)
            {
                LogHelper("删除博客失败", LogType.danger.ToString(), "博客不存在,用户很可能存在修改请求链接id的行为!!", User.Identity.Name, IpHelper.GetIp());
                return Json(false);
            }

            string email = blog.Author.Email;
            if (blog.Author.Email != User.Identity.Name)
            {
                LogHelper("删除博客失败", LogType.danger.ToString(), "用户删除的博客非本人创建的博客,用户极可能存在修改请求链接id的行为!Blog.Author:" + email, User.Identity.Name, IpHelper.GetIp());
                return Json(false);
            }
            if (blog.IsRelease == true)
            {
                var user = blog.Author;
                user.BlogCount--;
                if (blog.BlogType == BlogType.Jotting.ToString())
                {
                    user.JottingNum--;
                }
                else
                {
                    user.ArticleNum--;
                }
                db.Entry(user).State = EntityState.Modified;
                db.SaveChanges();
            }
            var categories = blog.Categories;
            foreach (var item in categories)
            {
                item.BlogCount--;
                db.Entry(item).State = EntityState.Modified;
                db.SaveChanges();
            }
            var collections = db.Collections.Where(p => p.BlogId.Id == blog.Id);
            db.Collections.RemoveRange(collections);
            db.Blogs.Remove(blog);
            db.SaveChanges();

            LogHelper("删除博客成功", LogType.add.ToString(), "删除博客成功!", User.Identity.Name, IpHelper.GetIp());
            return Json(true);
        }

        /// <summary>
        /// 管理类别
        /// </summary>
        /// <returns></returns>
        public ActionResult ManageCategory()
        {
            ViewBag.Categories = db.Categories.Where(p => p.User == User.Identity.Name).ToList();
            return View();
        }

        /// <summary>
        /// 添加类别
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateCategory(CategoryModel model)
        {
            if (ModelState.IsValid)
            {
                if (db.Categories.Any(p => p.Name == model.Name && p.User == User.Identity.Name))
                {
                    ModelState.AddModelError("", "已有该类别！");
                    LogHelper("添加类别失败", LogType.danger.ToString(), "已有该类别！客户端逃过了Js验证.", User.Identity.Name, IpHelper.GetIp());
                    return View("ManageCategory");
                }
                Category entity = new Category()
                {
                    Name = model.Name,
                    User = User.Identity.Name,
                    BlogCount = 0
                };
                db.Categories.Add(entity);
                db.SaveChanges();
                return RedirectToAction("ManageCategory");
            }
            return View("ManageCategory");
        }

        /// <summary>
        /// 检查类别名
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult CheckCategoryName(string name)
        {
            if (db.Categories.Any(p => p.Name == name && p.User == User.Identity.Name))
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
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditCategory(int id, string name)
        {
            var entity = db.Categories.Find(id);
            if (entity == null)
            {
                LogHelper("修改类别失败", LogType.danger.ToString(), "没有找到该类别！客户端修改了Id，或者读到了脏数据.", User.Identity.Name, IpHelper.GetIp());
                return Content("<script>alert('没有该类别！');location.href='/ManageBlogs/ManageCategory';</script>");
            }
            entity.Name = name;
            db.Entry(entity).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("ManageCategory");
        }

        /// <summary>
        /// 删除类别
        /// </summary>
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
        /// 添加博客成功
        /// </summary>
        /// <returns></returns>
        public ActionResult AddSuccess(int id, string blogType)
        {
            ViewBag.BlogId = id;
            ViewBag.BlogType = blogType;
            return View();
        }

        /// <summary>
        /// 修改博客成功
        /// </summary>
        /// <returns></returns>
        public ActionResult EditSuccess(int id, string blogType)
        {
            ViewBag.BlogId = id;
            ViewBag.BlogType = blogType;
            return View();
        }

        /// <summary>
        /// 检验博客标题
        /// </summary>
        /// <param name="title"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult CheckBlogTitle(string title)
        {
            var model = db.Blogs.Where(p => p.Title == title && p.Author.Email == User.Identity.Name);
            if (!model.Any())
            {
                return Json(true);
            }
            else
            {
                //有多份，则看是在草稿箱还是已发布
                foreach (var item in model)
                {
                    if (item.IsRelease == true)
                    {
                        return Json(false);
                    }
                }
                return Json(true);
            }
        }

        /// <summary>
        /// 上传图片
        /// </summary>
        /// <param name="usrId">参数</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult UploadImg(string usrId)
        {
            var imgFile = Request.Files["BlogPlusImg"];
            // 校验
            if (imgFile != null && Request.IsAuthenticated && User.Identity.Name == usrId)
            {
                // 文件格式
                string exName = Path.GetExtension(imgFile.FileName).ToLower();
                if (exName != ".jpg" && exName != ".jpeg" && exName != ".png" && exName != ".bmp" && exName != ".gif")
                {
                    LogHelper("上传图片失败", LogType.danger.ToString(), "文件格式错误!<br/>exName:" + exName, User.Identity.Name, IpHelper.GetIp());
                    return Content("error|文件格式错误(.jpg|.jpeg|.png|.bmp)!");
                }
                var usr = db.Users.FirstOrDefault(p => p.Email == usrId);
                if (usr == null)
                {
                    LogHelper("上传图片失败", LogType.danger.ToString(), "用户不存在(可能存在攻击)，这里是不可能发生的错误!", User.Identity.Name, IpHelper.GetIp());
                    return Content("error|用户不存在!");
                }

                // 存放图片
                string path = Server.MapPath("~/UploadFile/");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                string filePath = DateTime.Now.ToString("yyyyMMddHHmmss") + Path.GetExtension(imgFile.FileName);

                #region 加水印
                //if (exName != ".gif")
                //{
                //    Image img = Image.FromStream(imgFile.InputStream);
                //    Graphics g = Graphics.FromImage(img);
                //    string url = "blog.ydath.cn/Users/Index/" + usr.Id.ToString();
                //    if (img.Height > 44)
                //    {
                //        if (img.Width > usr.NickName.Length * 24)
                //        {
                //            g.DrawString(usr.NickName, new Font("微软雅黑", 12), new SolidBrush(Color.Red), new PointF(img.Width - usr.NickName.Length * 24, img.Height - 44));
                //        }
                //        if (img.Width > url.Length * 10)
                //        {
                //            g.DrawString(url, new Font("微软雅黑", 12), new SolidBrush(Color.Red), new PointF(img.Width - url.Length * 10, img.Height - 22));
                //        }
                //    }

                //    img.Save(path + filePath, ImageFormat.Jpeg);

                //    g.Dispose();
                //    img.Dispose();
                //}
                //else
                //{
                //    // GDI+不能修改gif格式的文件
                //    imgFile.SaveAs(path + filePath);
                //}
                #endregion

                // 保存图片
                imgFile.SaveAs(path + filePath);

                // 成功 注意：必须返回纯文本，不能用Json返回！
                //return Content(new UriBuilder(Request.Url) { Path = Url.Action("GetBlogImg", "ManageBlogs", new { id = filePath.Replace(".", "@@") }), Query = "" }.ToString()); //防盗链使用
                return Content(new UriBuilder(Request.Url) { Path = "/UploadFile/" + filePath, Query = "" }.ToString());
            }
            // 失败
            LogHelper("上传图片失败", LogType.danger.ToString(), "可能是图片为空，可能是用户没登录(非人操作)，可能是当前登录用户和校验的用户名不一样(可能存在攻击)!", User.Identity.Name??"游客", IpHelper.GetIp());
            return Content("error|图片存储失败!");
        }

        /// <summary>
        /// 防盗链
        /// </summary>
        /// <param name="id">图片文件名</param>
        /// <returns>文件字节流或null</returns>
        [AllowAnonymous]
        public ActionResult GetBlogImg(string id)
        {
            string path = Server.MapPath("~/UploadFile/") + id.Replace("@@",".");
            if (System.IO.File.Exists(path))
            {
                #region 反盗链代码
                //if (Uri.Compare(Request.UrlReferrer, Request.Url, UriComponents.HostAndPort, UriFormat.SafeUnescaped, StringComparison.CurrentCulture) == 0)
                //{
                //    using (MemoryStream ms = new MemoryStream())
                //    {
                //        Image img = Image.FromFile(path);
                //        img.Save(ms, ImageFormat.Jpeg);
                //        return File(ms.ToArray(), "image/jpeg");
                //    }
                //}

                //// 返回指定图片
                //LogHelper("下载图片失败", LogType.danger.ToString(), "可能存在盗链!<br/> " + "Referrer：" + Request.UrlReferrer.Authority + Request.UrlReferrer.AbsolutePath, String.IsNullOrWhiteSpace(User.Identity.Name) ? "游客" : User.Identity.Name, IpHelper.GetIp());
                //return File(Server.MapPath("~/icon/link.jpg"), "image/jpeg");
                #endregion

                using (MemoryStream ms = new MemoryStream())
                {
                    Image img = Image.FromFile(path);
                    img.Save(ms, ImageFormat.Jpeg);
                    return File(ms.ToArray(), "image/jpeg");
                }
            }
            // 返回指定图片
            LogHelper("下载图片失败", LogType.danger.ToString(), "图片不存在，可能已被删除!<br/>IMG：http://blog.ydath.cn/UploadFile/" + id.Replace("@@","."), String.IsNullOrWhiteSpace(User.Identity.Name) ? "游客" : User.Identity.Name, IpHelper.GetIp());
            return File(Server.MapPath("~/icon/delete.jpg"), "image/jpeg");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
