using Blog.Models;
using PagedList;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;

namespace Blog.Controllers
{
    public class HomeController : BaseController
    {
        /// <summary>
        /// 网站首页
        /// </summary>
        /// <param name="searchStr">搜索字符串</param>
        /// <param name="id">页码</param>
        /// <returns></returns>
        [OutputCache(CacheProfile = "SqlDependencyCacheForHomeIndex")]
        public ActionResult Index(string searchStr, int id = 1)
        {
            var model = from x in db.Blogs.Include("Author") select x;

            //是否有搜索
            if (!string.IsNullOrWhiteSpace(searchStr))
            {
                model = model.Where(p => p.Title.Contains(searchStr));

                //权限控制
                if (Request.IsAuthenticated)
                {
                    model = model.Where(p => (p.IsPulish == true && p.IsRelease == true) || (p.IsPulish == false && p.Author.Email == User.Identity.Name));
                }
                else
                {
                    model = model.Where(p => p.IsPulish == true && p.IsRelease == true);
                }
                ViewBag.searchStr = searchStr;
                ViewBag.Title = "搜索 - " + searchStr;
            }
            else
            {
                model = model.Where(p => p.IsPulish == true && p.IsPush == true && p.IsRelease == true);
            }
            // 推荐博客
            ViewBag.GroomBlog = db.Blogs.Where(p => p.IsPulish == true && p.IsRelease == true && p.IsPush == true).Select(p => new ManageBlog { Id = p.Id, Title = p.Title, LookNum = p.LookNum }).OrderByDescending(p => p.LookNum).Take(10).ToList();
            // 博客类别
            ViewBag.Categries = db.Categories.Where(p => p.User == "system").ToList();
            return View("AIndex", model.Select(p => new ShowBlog(){Author = p.Author.NickName, AuthorId = p.Author.Id, Id = p.Id, Brief = p.Brief, BlogType = p.BlogType, CreateTime = p.CreateTime, LookNum = p.LookNum, Title = p.Title}).OrderByDescending(p => p.CreateTime).ToPagedList(id, pageSize*2));
        }

        /// <summary>
        /// 关于页面
        /// </summary>
        /// <returns></returns>
        public ActionResult About()
        {
            int num = db.WebConfigs.FirstOrDefault().LookNums;
            List<int> looklist = new List<int>();
            while(true)
            {
                looklist.Add(num % 10);
                num = num / 10;
                if (num == 0)
                    break;
            }
            looklist.Reverse();
            ViewBag.LookNum = looklist;
            return View();
        }

        /// <summary>
        /// 用户页面
        /// </summary>
        /// <returns></returns>
        public ActionResult Users(string searchUser, int page = 1)
        {
            var usr = from x in db.Users select x;
            if (string.IsNullOrWhiteSpace(searchUser))
            {
                usr = usr.Where(p => p.IsPubulish == true);
            }
            else
            {
                ViewBag.searchUser = searchUser;
                usr = usr.Where(p => p.IsPubulish == true && (searchUser.Contains(p.NickName) || p.NickName.Contains(searchUser)));
            }
            
            return View(usr.OrderBy(p => p.Id).ToPagedList(page, 28));
        }

        /// <summary>
        /// 登录信息页面片段
        /// </summary>
        /// <returns></returns>
        public ActionResult LoginInfo(string searchStr, string blogType)
        {
            ViewBag.BlogType = blogType;
            ViewBag.SearchStr = searchStr;
            if (Request.IsAuthenticated)
                ViewBag.UsrId = db.Users.FirstOrDefault(p => p.Email == User.Identity.Name).Id;
            return View();
        }

        /// <summary>
        /// 自定义访问路径错误处理
        /// </summary>
        /// <param name="actionName"></param>
        public ActionResult ErrorHandle(int id = 404)
        {
            if (id == 404)
            {
                ViewBag.Content = "路径错误或资源不存在!";
                return View("NotFound");
            }
            else
            {
                return View($"Error");
            }
        }

        public void LookNumCount()
        {
            var entity = db.WebConfigs.FirstOrDefault();
            if (entity == null)
            {
                entity = new Models.WebConfig {LookNums = 1000};
                db.WebConfigs.Add(entity);
            }
            entity.LookNums++;
            db.SaveChanges();
        }

        // 临时方法
        public ActionResult Temp()
        {
            var blog = db.Blogs.ToList();
            foreach (var item in blog)
            {
                item.Content = item.Content.Replace("https://blog.ydath.cn/UploadFile/", "https://blog.ydath.cn:443/UploadFile/");
                db.Entry(item).State = EntityState.Modified;
                db.SaveChanges();
            }
            return Content("OK");
        }
    }
}