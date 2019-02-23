using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Blog.Models
{
    /// <summary>
    /// 用户数据模型
    /// </summary>
    public class User
    {
        [Key]
        public int Id { get; set; }

        [DisplayName("邮箱")]
        [DataType(DataType.EmailAddress)]
        [Required(ErrorMessage = "{0}为必填项!")]
        [StringLength(255, ErrorMessage = "{0不能超过{1}!")]
        public string Email { get; set; }

        [DisplayName("昵称")]
        [Required(ErrorMessage = "{0}为必填项!")]
        [StringLength(255, ErrorMessage = "{0应该在{2}-{1}之间!", MinimumLength = 2)]
        public string NickName { get; set; }

        [DisplayName("个人信条")]
        [DataType(DataType.MultilineText)]
        [StringLength(500, ErrorMessage = "{0}应该在{2}-{1}之间!", MinimumLength = 2)]
        public string Belief { get; set; }

        [DisplayName("博客数")]
        public int BlogCount { get; set; }

        [DisplayName("随笔数")]
        public int JottingNum { get; set; }

        [DisplayName("文章数")]
        public int ArticleNum { get; set; }

        [DisplayName("博客浏览量")]
        public int BlogLookNum { get; set; }

        [DisplayName("注册时间")]
        public DateTime JoinTime { get; set; }

        [DisplayName("关于页面浏览次数")]
        public int LookNum { get; set; }

        [DisplayName("上传次数")]
        public int CreateNum { get; set; }

        [DisplayName("上次上传时间")]
        public DateTime CreateTime { get; set; } = DateTime.Now;

        [DisplayName("学校名称")]
        public string SchoolName { get; set; }

        [DisplayName("用户头像")]
        public string PicLink { get; set; }

        [DisplayName("友情链接")]
        [Description("友情链接一 ----- http://www.ydath.cn @@@@@ 友情链接二 ----- http://www.ydath.cn")]
        public string FriendLink { get; set; }

        [DisplayName("是否公开信息")]
        [Description("这里未公开信息表示自己的个人博客页面没有人能访问到!")]
        public bool IsPubulish { get; set; }

        public virtual ICollection<BlogInfo> Blogs { get; set; }

        public virtual ICollection<Letter> Letters { get; set; }

        public virtual ICollection<SysNews> SysNews { get; set; }
    }

    /// <summary>
    /// 用户登录数据模型
    /// </summary>
    public class UserLogin
    {
        [Key]
        public int Id { get; set; }

        [DisplayName("邮箱")]
        [DataType(DataType.EmailAddress)]
        [Required(ErrorMessage ="{0}为必填项!")]
        [StringLength(255, ErrorMessage ="{0}不能超过{1}位!")]
        public string Email { get; set; }

        [DisplayName("密码")]
        [DataType(DataType.Password)]
        [Required(ErrorMessage = "{0}为必填项!")]
        [StringLength(40, ErrorMessage = "{0}应该在{2}-{1}位之间!", MinimumLength = 6)]
        public string Password { get; set; }

        [DisplayName("密码错误次数!")]
        [Description("连续输入错误5次将锁住账号30分钟!")]
        public int ErrorTimes { get; set; }

        [DisplayName("禁言")]
        public bool Gog { get; set; } = false;

        [DisplayName("被锁时间")]
        [DataType(DataType.DateTime)]
        public DateTime LockingTime { get; set; }

        [DisplayName("注册激活码")]
        public string RCode { get; set; }

        [DisplayName("code过期时间")]
        [DataType(DataType.DateTime)]
        public DateTime Invalid { get; set; }

        [DisplayName("忘记密码Code")]
        public string FCode { get; set; }

        [DisplayName("用户角色")]
        public virtual Role Role { get; set; }

        [DisplayName("系统公告")]
        public virtual ICollection<SysNews> SysNews { get; set; }
    }

    /// <summary>
    /// 登录页面数据模型
    /// </summary>
    public class LoginModel
    {
        [Key]
        public int Id { get; set; }

        [DisplayName("邮箱")]
        [DataType(DataType.EmailAddress)]
        [Required(ErrorMessage = "{0}为必填项!")]
        [StringLength(255, ErrorMessage = "{0}不能超过{1}位!")]
        public string Email { get; set; }

        [DisplayName("密码")]
        [DataType(DataType.Password)]
        [Required(ErrorMessage = "{0}为必填项!")]
        [StringLength(40, ErrorMessage = "{0}应该在{2}-{1}位之间!", MinimumLength = 6)]
        public string Password { get; set; }
        
        [DisplayName("返回地址")]
        public string ReturnUrl { get; set; }

        //[DisplayName("验证码")]
        //[Required(ErrorMessage ="{0}为必填项!")]
        //[StringLength(4, ErrorMessage ="{0}应该为4位!", MinimumLength =4)]
        //[Remote("CheckVerifyCode", "Base", ErrorMessage ="验证码错误!", HttpMethod ="POST")]
        //public string Code { get; set; }
    }

    /// <summary>
    /// 注册数据模型
    /// </summary>
    public class RegisterModel
    {
        [Key]
        public int Id { get; set; }

        [DisplayName("邮箱")]
        [DataType(DataType.EmailAddress)]
        [Required(ErrorMessage = "{0}为必填项!")]
        [StringLength(255, ErrorMessage = "{0}不能超过{1}位!")]
        [Remote("CheckEmail", "Account", ErrorMessage ="该邮箱已注册!", HttpMethod ="POST")]
        public string Email { get; set; }

        [DisplayName("密码")]
        [DataType(DataType.Password)]
        [Required(ErrorMessage = "{0}为必填项!")]
        [StringLength(40, ErrorMessage = "{0}应该在{2}-{1}位之间!", MinimumLength = 6)]
        public string Password { get; set; }

        [DisplayName("确认密码")]
        [DataType(DataType.Password)]
        [Required(ErrorMessage = "{0}为必填项!")]
        [StringLength(40, ErrorMessage = "{0}应该在{2}-{1}位之间!", MinimumLength = 6)]
        [System.ComponentModel.DataAnnotations.Compare("Password", ErrorMessage ="密码与确认密码不一致!")]
        public string RePassword { get; set; }

        //[DisplayName("验证码")]
        //[Required(ErrorMessage = "{0}为必填项!")]
        //[StringLength(4, ErrorMessage = "{0}应该为4位!", MinimumLength = 4)]
        //[Remote("CheckVerifyCode", "Base", ErrorMessage = "验证码错误!", HttpMethod = "POST")]
        //public string Code { get; set; }
    }

    /// <summary>
    /// 忘记密码数据模型
    /// </summary>
    public class ForgotPassword
    {
        [Key]
        public int Id { get; set; }

        [DisplayName("邮箱")]
        [DataType(DataType.EmailAddress)]
        [Required(ErrorMessage = "{0}为必填项!")]
        [StringLength(255, ErrorMessage = "{0}不能超过{1}位!")]
        public string Email { get; set; }

        //[DisplayName("验证码")]
        //[Required(ErrorMessage = "{0}为必填项!")]
        //[StringLength(4, ErrorMessage = "{0}应该为4位!", MinimumLength = 4)]
        //[Remote("CheckVerifyCode", "Base", ErrorMessage = "验证码错误!", HttpMethod = "POST")]
        //public string Code { get; set; }
    }

    /// <summary>
    /// 重置密码数据模型
    /// </summary>
    public class ResetPassword
    {
        [Key]
        public string FCode { get; set; }//忘记密码Code

        [DisplayName("密码")]
        [DataType(DataType.Password)]
        [Required(ErrorMessage = "{0}为必填项!")]
        [StringLength(40, ErrorMessage = "{0}应该在{2}-{1}位之间!", MinimumLength = 6)]
        public string Password { get; set; }

        [DisplayName("确认密码")]
        [DataType(DataType.Password)]
        [Required(ErrorMessage = "{0}为必填项!")]
        [StringLength(40, ErrorMessage = "{0}应该在{2}-{1}位之间!", MinimumLength = 6)]
        [System.ComponentModel.DataAnnotations.Compare("Password", ErrorMessage = "密码与确认密码不一致!")]
        public string RePassword { get; set; }

        //[DisplayName("验证码")]
        //[Required(ErrorMessage = "{0}为必填项!")]
        //[StringLength(4, ErrorMessage = "{0}应该为4位!", MinimumLength = 4)]
        //[Remote("CheckVerifyCode", "Base", ErrorMessage = "验证码错误!", HttpMethod = "POST")]
        //public string Code { get; set; }
    }

    /// <summary>
    /// 更改密码数据模型
    /// </summary>
    public class ChangePassword
    {
        [Key]
        public int Id { get; set; }

        [DisplayName("旧密码")]
        [DataType(DataType.Password)]
        [Required(ErrorMessage = "{0}为必填项!")]
        [StringLength(40, ErrorMessage = "{0}应该在{2}-{1}位之间!", MinimumLength =6)]
        public string OldPassword { get; set; }

        [DisplayName("密码")]
        [DataType(DataType.Password)]
        [Required(ErrorMessage = "{0}为必填项!")]
        [StringLength(40, ErrorMessage = "{0}应该在{2}-{1}位之间!", MinimumLength = 6)]
        public string Password { get; set; }

        [DisplayName("确认密码")]
        [DataType(DataType.Password)]
        [Required(ErrorMessage = "{0}为必填项!")]
        [StringLength(40, ErrorMessage = "{0}应该在{2}-{1}位之间!", MinimumLength = 6)]
        [System.ComponentModel.DataAnnotations.Compare("Password", ErrorMessage = "密码与确认密码不一致!")]
        public string RePassword { get; set; }

        //[DisplayName("验证码")]
        //[Required(ErrorMessage = "{0}为必填项!")]
        //[StringLength(4, ErrorMessage = "{0}应该为4位!", MinimumLength = 4)]
        //[Remote("CheckVerifyCode", "Base", ErrorMessage = "验证码错误!", HttpMethod = "POST")]
        //public string Code { get; set; }
    }

    public class ModUser
    {
        [DisplayName("昵称")]
        [Required(ErrorMessage = "{0}为必填项!")]
        [StringLength(255, ErrorMessage = "{0应该在{2}-{1}之间!", MinimumLength = 2)]
        public string NickName { get; set; }

        [DisplayName("个人信条")]
        [DataType(DataType.MultilineText)]
        [StringLength(500, ErrorMessage = "{0}应该在{2}-{1}之间!", MinimumLength = 2)]
        public string Belief { get; set; }

        [DisplayName("学校名称")]
        public string SchoolName { get; set; }

        [DisplayName("用户头像")]
        public string PicLink { get; set; }

        [DisplayName("友情链接")]
        [Description("友情链接一 ----- http://www.ydath.cn")]
        public string FriendLink1 { get; set; }

        [DisplayName("友情链接")]
        public string FriendLink2 { get; set; }

        [DisplayName("友情链接")]
        public string FriendLink3 { get; set; }

        [DisplayName("是否公开信息")]
        [Description("这里未公开信息表示自己的个人博客页面没有人能访问到!")]
        public bool IsPubulish { get; set; }
    }
}