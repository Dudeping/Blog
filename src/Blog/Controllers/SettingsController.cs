using Blog.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Blog.Controllers
{
    [Authorize]
    public class SettingsController : BaseController
    {
        /// <summary>
        /// 个人设置
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            var model = db.Users.FirstOrDefault(p => p.Email == User.Identity.Name);
            ModUser usr = new ModUser
            {
                Belief = model.Belief,
                IsPubulish = model.IsPubulish,
                NickName = model.NickName,
                PicLink = model.PicLink,
                SchoolName = model.SchoolName
            };
            string friendLink = model.FriendLink;
            int index = 0;
            foreach (var item in friendLink.Split(new string[] { "@@@@@" }, StringSplitOptions.RemoveEmptyEntries))
            {
                switch (index)
                {
                    case 0:
                        usr.FriendLink1 = item.Trim(' ');
                        break;
                    case 1:
                        usr.FriendLink2 = item.Trim(' ');
                        break;
                    default:
                        usr.FriendLink3 = item.Trim(' ');
                        break;
                }
                index++;
            }
            return View(usr);
        }

        /// <summary>
        /// 个人信息设置
        /// </summary>
        /// <param name="model">数据</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(ModUser model)
        {
            if (ModelState.IsValid)
            {
                var entity = db.Users.FirstOrDefault(p => p.Email == User.Identity.Name);
                if (entity == null)
                {
                    return HttpNotFound();
                }

                entity.NickName = model.NickName;
                entity.Belief = model.Belief;
                entity.IsPubulish = model.IsPubulish;
                entity.SchoolName = model.SchoolName;
                //友情链接
                entity.FriendLink = "";
                try
                {
                    if (!String.IsNullOrWhiteSpace(model.FriendLink1))
                    {
                        var veri = model.FriendLink1.Split(new string[] { "-----" }, StringSplitOptions.RemoveEmptyEntries)[1];
                        entity.FriendLink += model.FriendLink1;
                    }
                    if (!String.IsNullOrWhiteSpace(model.FriendLink2))
                    {
                        var veri = model.FriendLink2.Split(new string[] { "-----" }, StringSplitOptions.RemoveEmptyEntries)[1];
                        if (!String.IsNullOrWhiteSpace(entity.FriendLink))
                        {
                            entity.FriendLink += "@@@@@" + model.FriendLink2;
                        }
                        else
                        {
                            entity.FriendLink += model.FriendLink2;
                        }
                    }
                    if (!String.IsNullOrWhiteSpace(model.FriendLink3))
                    {
                        var veri = model.FriendLink3.Split(new string[] { "-----" }, StringSplitOptions.RemoveEmptyEntries)[1];
                        if (!String.IsNullOrWhiteSpace(entity.FriendLink))
                        {
                            entity.FriendLink += "@@@@@" + model.FriendLink3;
                        }
                        else
                        {
                            entity.FriendLink += model.FriendLink3;
                        }
                    }
                }
                catch (Exception)
                {
                    ModelState.AddModelError("", "友情链接不符合规范!");
                    return View("Index", model);
                }

                // 保存头像
                var picLink = Request.Files["PicLink"];
                if (picLink.ContentLength != 0)
                {
                    // 文件格式
                    string exName = Path.GetExtension(picLink.FileName).ToLower();
                    if (exName != ".jpg" && exName != ".jpeg" && exName != ".png" && exName != ".bmp")
                    {
                        LogHelper("上传图片失败", LogType.danger.ToString(), "文件格式错误!exName:" + exName, User.Identity.Name, IpHelper.GetIp());
                        return Content("<Script>alert('error|文件格式错误(.jpg|.jpeg|.png|.bmp)!');location.href='/Settings/Index';</Script>");
                    }
                    string path = Server.MapPath("~/icon/");
                    if (Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                    // 取得图片
                    Image originalImage = Image.FromStream(picLink.InputStream);

                    const int towidth = 48;
                    const int toheight = 48;

                    int x, y, ow, oh;

                    // 裁剪(宽和高哪个大依哪个)
                    if ((double)originalImage.Width / (double)originalImage.Height > (double)towidth / (double)toheight)
                    {
                        oh = originalImage.Height;
                        ow = originalImage.Height * towidth / toheight;
                        y = 0;
                        x = (originalImage.Width - ow) / 2; // 居中
                    }
                    else
                    {
                        ow = originalImage.Width;
                        oh = originalImage.Width * toheight / towidth;
                        x = 0;
                        y = (originalImage.Height - oh) / 2;
                    }

                    // 新建一个bmp图片 
                    Image bitmap = new Bitmap(towidth, toheight);
                     
                    // 新建一个画板 
                    Graphics g = Graphics.FromImage(bitmap);

                    // 设置高质量插值法 
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;

                    // 设置高质量,低速度呈现平滑程度 
                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

                    // 清空画布并以透明背景色填充 
                    g.Clear(Color.Transparent);

                    // 在指定位置并且按指定大小绘制原图片的指定部分 
                    g.DrawImage(originalImage, new Rectangle(0, 0, towidth, toheight),
                     new Rectangle(x, y, ow, oh),
                     GraphicsUnit.Pixel);

                    try
                    {
                        // 以jpg格式保存缩略图 
                        string fileName = "thumb_" + DateTime.Now.ToString("yyyyMMddHHmmss") + exName;
                        bitmap.Save(path + fileName, System.Drawing.Imaging.ImageFormat.Jpeg);
                        // 删除原有文件
                        if (entity.PicLink != "/icon/GH.png" && entity.PicLink != "/icon/DH.png")
                        {
                            System.IO.File.Delete(Server.MapPath("~") + entity.PicLink);
                        }
                        // 保存新文件
                        entity.PicLink = "/icon/" + fileName;
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("", "图片保存失败!");
                        LogHelper("头像保存失败", LogType.error.ToString(), ex.Message, User.Identity.Name, IpHelper.GetIp());
                        return View("Index", model);
                    }
                    finally
                    {
                        originalImage.Dispose();
                        bitmap.Dispose();
                        g.Dispose();
                    }
                }

                db.Entry(entity).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                return Content("<Script>alert('修改完成！');location.href='/Settings/Index';</Script>");
            }
            return View("Index", model);
        }
    }
}