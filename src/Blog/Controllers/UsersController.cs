using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Blog.Models;
using PagedList;

namespace Blog.Controllers
{
    public class UsersController : BaseController
    {
        /// <summary>
        /// 用户首页
        /// </summary>
        /// <param name="id">用户Id</param>
        /// <param name="page">页数</param>
        /// <returns></returns>
        [OutputCache(CacheProfile = "CacheForUserIndex")]
        public ActionResult Index(int id, int page = 1)
        {
            var usr = (from x in db.Users where x.Id == id select x).FirstOrDefault();
            //用户不存在或者用户不公开信息
            if (usr == null)
            {
                ViewBag.Content = "用户不存在!";
                return View("NotFound");
            }

            if (usr.IsPubulish == false && usr.Email != User.Identity.Name)
            {
                ViewBag.Content = "用户不公开首页!";
                return View("NoAccess");
            }

            var model = from x in db.Blogs select x;
            if (Request.IsAuthenticated && usr.Email == User.Identity.Name)
            {
                model = model.Where(p => p.Author.Email == usr.Email && p.IsRelease == true);
            }
            else
            {
                model = model.Where(p => p.Author.Id == usr.Id && p.IsRelease == true && p.IsPulish == true);
            }
            //用户信息
            ViewBag.UserInfo = usr;
            ViewBag.Categries = db.Categories.Where(p => p.User == usr.Email).ToList();
            return View(model.Select(p => new ShowBlog(){BlogType = p.BlogType, LookNum = p.LookNum, Brief = p.Brief, Title = p.Title, Id = p.Id, CollectionTimes = p.CollectionTimes, CreateTime = p.CreateTime}).OrderByDescending(p => p.CreateTime).ToPagedList(page, pageSize*2));
        }

        /// <summary>
        /// 博客详情
        /// </summary>
        /// <param name="id">博客Id</param>
        /// <returns></returns>
        public ActionResult BlogDetails(int id)
        {
            // 提高响应速度
            var model = db.Blogs.FirstOrDefault(p => p.Id == id);
            if (model == null)
            {
                ViewBag.Content = "博客不存在!";
                return View("NotFound");
            }

            var usr = db.Users.FirstOrDefault(p => p.Id == model.Author.Id);
            if (usr == null)
            {
                ViewBag.Content = "用户不存在!";
                return View("NotFound");
            }

            //权限控制
            if ((model.IsPulish == false || model.IsRelease == false) && (!Request.IsAuthenticated || usr.Email != User.Identity.Name))
            {
                ViewBag.Content = "用户不公开本篇博客!";
                return View("NoAccess");
            }

            //统计浏览量
            if (usr.Email != User.Identity.Name)
            {
                model.LookNum++;
                usr.BlogLookNum++;
                db.Entry(usr).State = EntityState.Modified;
                db.Entry(model).State = EntityState.Modified;
                db.SaveChanges();
            }
            
            //上一篇，下一篇
            //权限判断
            if (Request.IsAuthenticated && usr.Email == User.Identity.Name)
            {
                ViewBag.PreBlog = db.Blogs.Where(p => p.IsRelease == true && p.Author.Email == usr.Email && p.CreateTime > model.CreateTime).Select(p => new PANBlog() { Id = p.Id, Title = p.Title}).FirstOrDefault();
                ViewBag.NextBlog = db.Blogs.Where(p => p.IsRelease == true && p.Author.Email == usr.Email && p.CreateTime < model.CreateTime).OrderByDescending(p => p.CreateTime).Select(p => new PANBlog() { Id = p.Id, Title = p.Title }).FirstOrDefault();
            }
            else
            {
                ViewBag.PreBlog = db.Blogs.Where(p => p.IsPulish == true && p.Author.Email == usr.Email && p.IsRelease == true && p.CreateTime > model.CreateTime).Select(p => new PANBlog() { Id = p.Id, Title = p.Title }).FirstOrDefault();
                ViewBag.NextBlog = db.Blogs.Where(p => p.IsPulish == true && p.Author.Email == usr.Email && p.IsRelease == true &&  p.CreateTime < model.CreateTime).OrderByDescending(p => p.CreateTime).Select(p => new PANBlog() { Id = p.Id, Title = p.Title }).FirstOrDefault();
            }
            //用户信息
            ViewBag.UserInfo = usr;
            // 是否收藏
            if (Request.IsAuthenticated)
            {
                ViewBag.IsCollection = db.Collections.Count(p => p.BlogId.Id == model.Id && p.UserId.Email == User.Identity.Name);
            }
            return View(model);
        }

        /// <summary>
        /// 关于我
        /// </summary>
        /// <param name="id">用户Id</param>
        /// <returns></returns>
        public ActionResult About(int id)
        {
            var usr = db.Users.FirstOrDefault(p => p.Id == id);
            if (usr == null)
            {
                ViewBag.Content = "用户不存在!";
                return View("NotFound");
            }

            //统计浏览量
            if (usr.Email != User.Identity.Name)
            {
                usr.LookNum++;
                db.Entry(usr).State = EntityState.Modified;
                db.SaveChanges();
            }
            //用户信息
            ViewBag.UserInfo = usr;
            ViewBag.Categories = db.Categories.Where(p => p.User == usr.Email).ToList();

            return View();
        }

        /// <summary>
        /// 搜索标签
        /// </summary>
        /// <param name="searchTag"></param>
        /// <param name="page">页数</param>
        /// <param name="usrId"></param>
        /// <returns></returns>
        public ActionResult SearchTag(int usrId, string searchTag, int page = 1)
        {
            var model = db.Blogs.Where(p => p.Tags.Contains(searchTag));
            ViewBag.searchTag = searchTag;
            ViewBag.UsrId = usrId;
            ViewBag.Title = "搜索标签 - " + searchTag;

            //查询用户
            var usrInfo = db.Users.FirstOrDefault(p => p.Id == usrId);
            if (usrInfo ==null)
            {
                return Content("<script>history.go(-1);</script>");
            }

            ViewBag.UserInfo = usrInfo;
            ViewBag.Categries = db.Categories.Where(p => p.User == usrInfo.Email).ToList();
            //是否登录
            if (Request.IsAuthenticated && usrInfo.Email == User.Identity.Name)
            {
                //自己看自己，只要发布都可以
                model = model.Where(p => p.IsRelease == true && p.Author.Id == usrId);
                return View("Index", model.Select(p => new ShowBlog() { BlogType = p.BlogType, LookNum = p.LookNum, Brief = p.Brief, Title = p.Title, Id = p.Id, CollectionTimes = p.CollectionTimes, CreateTime = p.CreateTime }).OrderByDescending(p => p.CreateTime).ToPagedList(page, pageSize * 2));
            }
            else
            {
                model = model.Where(p => p.IsPulish == true && p.IsRelease == true && p.Author.Id == usrId);
                return View("Index", model.Select(p => new ShowBlog() { BlogType = p.BlogType, LookNum = p.LookNum, Brief = p.Brief, Title = p.Title, Id = p.Id, CollectionTimes = p.CollectionTimes, CreateTime = p.CreateTime }).OrderByDescending(p => p.CreateTime).ToPagedList(page, pageSize * 2));
            }
        }

        /// <summary>
        /// 搜索类别
        /// </summary>
        /// <param name="category"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public ActionResult Category(int id, int page=1)
        {
            var category = db.Categories.Find(id);
            if (category == null)
            {
                return Content("<script>history.go(-1);</script>");
            }
            ViewBag.CategoryId = id;
            ViewBag.Title = "搜索类别 - " + category.Name;
            var user = db.Users.FirstOrDefault(p => p.Email == category.User);
            ViewBag.UserInfo = user;
            if (category.User == "system")
            {
                // 推荐博客
                ViewBag.GroomBlog = db.Blogs.Where(p => p.IsPulish == true && p.IsRelease == true && p.IsPush == true).Select(p => new ManageBlog { Id = p.Id, Title = p.Title, LookNum = p.LookNum }).OrderByDescending(p => p.LookNum).Take(10).ToList();
                // 博客类别
                ViewBag.Categries = db.Categories.Where(p => p.User == "system").ToList();
                var model = db.Blogs.Include("Author").Where(p => p.IsPulish == true && p.IsRelease == true && p.Categories.Any(x => x.Id == category.Id));
                return View("AIndex", model.Select(p => new ShowBlog() { Author = p.Author.NickName, AuthorId = p.Author.Id, Id = p.Id, Brief = p.Brief, BlogType = p.BlogType, CreateTime = p.CreateTime, LookNum = p.LookNum, Title = p.Title }).OrderByDescending(p => p.CreateTime).ToPagedList(page, pageSize * 2));
            }
            else
            {
                ViewBag.Categries = db.Categories.Where(p => p.User == category.User).ToList();
                var model = db.Blogs.Include("Author").Where(p => p.IsRelease == true && p.Categories.Any(x => x.Id == category.Id));
                if (!Request.IsAuthenticated || category.User != User.Identity.Name)
                {
                    model = model.Where(p => p.IsPulish == true);
                }
                return View("Index", model.Select(p => new ShowBlog() { Author = p.Author.NickName, AuthorId = p.Author.Id, Id = p.Id, Brief = p.Brief, BlogType = p.BlogType, CreateTime = p.CreateTime, LookNum = p.LookNum, Title = p.Title }).OrderByDescending(p => p.CreateTime).ToPagedList(page, pageSize * 2));
            }
        }

        /// <summary>
        /// 意见反馈
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Authorize(Roles = "User")]
        public ActionResult Feedback()
        {
            ViewBag.SysNewsNum = GetNewsNum();
            return View();
        }

        /// <summary>
        /// 意见反馈
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ValidateInput(false)]
        [ValidateAntiForgeryToken]
        [Authorize(Roles ="User")]
        public ActionResult Feedback(Feedback model)
        {
            ViewBag.SysNewsNum = GetNewsNum();
            if (!ModelState.IsValid) return View(model);
            Letter entity = new Letter
            {
                Title = model.Title,
                Content = model.Content
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
            entity.CreateTime = DateTime.Now;
            entity.From = db.Users.FirstOrDefault(p => p.Email == User.Identity.Name);
            entity.To = "admin@blog.ydath.cn";
            entity.IsRead = false;

            db.Letters.Add(entity);
            db.SaveChanges();
            return Content("<script>alert('反馈已记录,谢谢,我们会尽快回复!');location.href='/Users/SysNews';</script>");
        }

        /// <summary>
        /// 未读消息
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = "User")]
        public ActionResult SysNews()
        {
            ViewBag.SysNewsNum = GetNewsNum();
            ViewBag.FeelBack = db.Letters.Where(p => p.To == User.Identity.Name && p.IsRead == false).OrderByDescending(p => p.Id).Select(p => new FBLetter() { Content = p.Content, Id = p.Id, Title = p.Title, CreateTime = p.CreateTime });
            ViewBag.SysNews = db.SysNews.Where(p => !p.IsRead.Where(x => x.Email == User.Identity.Name).OrderByDescending(x => x.Id).Any());
            return View();
        }

        /// <summary>
        /// 已读消息
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = "User")]
        public ActionResult Message()
        {
            ViewBag.SysNewsNum = GetNewsNum();
            ViewBag.FeelBack = db.Letters.Where(p => p.To == User.Identity.Name && p.IsRead == true).OrderByDescending(x => x.Id).Select(p => new FBLetter() { Content = p.Content, Id = p.Id, Title = p.Title, CreateTime = p.CreateTime });
            ViewBag.SysNews = db.SysNews.Where(p => p.IsRead.Any(x => x.Email == User.Identity.Name)).OrderByDescending(x => x.Id);
            return View();
        }

        /// <summary>
        /// 删除回复消息
        /// </summary>
        /// <param name="id">消息Id</param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Roles = "User")]
        public ActionResult DeleteReply(int id)
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
        /// 标记已读(消息回复)
        /// </summary>
        /// <param name="id">消息Id</param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Roles = "User")]
        public ActionResult TabFeedIsRead(int id)
        {
            var model = db.Letters.Find(id);
            if (model == null)
            {
                return Json(false);
            }
            model.IsRead = true;
            db.SaveChanges();
            return Json(true);
        }

        /// <summary>
        /// 标记已读(消息回复)
        /// </summary>
        /// <param name="id">消息Id</param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Roles = "User")]
        public ActionResult TabSysIsRead(int id)
        {
            var model = db.SysNews.Find(id);
            if (model == null)
            {
                return Json(false);
            }
            model.IsRead.Add(db.Users.FirstOrDefault(p => p.Email == User.Identity.Name));
            db.SaveChanges();
            return Json(true);
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
