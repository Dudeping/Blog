using Blog.Models;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Blog.Controllers
{
    [Authorize]
    public class ManageCollectionController : BaseController
    {
        /// <summary>
        /// 管理收藏首页
        /// </summary>
        /// <param name="page">页数</param>
        /// <returns></returns>
        public ActionResult Index(int page = 1)
        {
            var usr = db.Users.FirstOrDefault(p => p.Email == User.Identity.Name);
            var model = db.Collections.Where(p => p.UserId.Id == usr.Id);

            var list = from x in db.Blogs
                       join c in model on x.Id equals c.BlogId.Id
                       select new BlogCollection { Id = x.Id, Author = x.Author, EidtTime = x.EidtTime, CId = c.Id, LookNum = x.LookNum, Title = x.Title };

            return View(list.OrderByDescending(p => p.Id).ToPagedList(page, pageSize * 3));
        }

        /// <summary>
        /// 收藏页面
        /// </summary>
        /// <param name="id">博客id</param>
        /// <param name="email">用户email</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Collection(int id, string email)
        {
            var user = db.Users.FirstOrDefault(p => p.Email == email);
            if (user == null)
            {
                return Json(false);
            }

            var blog = db.Blogs.Find(id);
            if (blog == null)
            {
                return Json(false);
            }

            Collection entity = new Collection
            {
                BlogId = blog,
                UserId = user
            };

            blog.CollectionTimes++;
            db.Collections.Add(entity);
            db.SaveChanges();

            return Json(true);
        }

        /// <summary>
        /// 删除收藏
        /// </summary>
        /// <param name="id">收藏id</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Delete(int id)
        {
            var model = db.Collections.Where(p => p.Id == id);
            if (model == null)
            {
                return Json(false);
            }

            var blog = (from x in db.Blogs
                        join c in model on x.Id equals c.BlogId.Id
                        select x).FirstOrDefault();
            if (blog != null)
            {
                blog.CollectionTimes--;
            }
            db.Collections.Remove(model.FirstOrDefault());
            db.SaveChanges();
            return Json(true);
        }
    }
}