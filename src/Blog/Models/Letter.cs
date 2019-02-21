using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BlogPlus.Models
{
    /// <summary>
    /// 站内信数据模型
    /// </summary>
    public class Letter
    {
        [Key]
        public int Id { get; set; }

        [DisplayName("标题")]
        [Required(ErrorMessage ="{0}为必填项!")]
        [StringLength(50, ErrorMessage ="{0}应该在{2}-{1}之间!", MinimumLength =2)]
        public string Title { get; set; }

        [DisplayName("内容")]
        [Required(ErrorMessage = "{0}为必填项!")]
        [DataType(DataType.MultilineText)]
        [StringLength(1000, ErrorMessage = "{0}应该在{2}-{1}之间!", MinimumLength =2)]
        public string Content { get; set; }

        [DisplayName("是否已读")]
        [Required(ErrorMessage ="{0}为必填项!")]
        public bool IsRead { get; set; }

        [DisplayName("回复")]
        public string Reply { get; set; }

        [DisplayName("收件人")]
        [Required(ErrorMessage = "{0}为必填项!")]
        [MaxLength(255, ErrorMessage ="{0}应该在{1}位以内!")]
        [DataType(DataType.EmailAddress, ErrorMessage ="请输入正确的邮件格式!")]
        public string To { get; set; }

        [DisplayName("发件人")]
        public virtual User From { get; set; }

        [DisplayName("创建时间")]
        [DataType(DataType.DateTime)]
        public DateTime CreateTime { get; set; }
    }

    /// <summary>
    /// 公告模型
    /// </summary>
    public class SysNews
    {
        [Key]
        public int Id { get; set; }

        [DisplayName("标题")]
        [Required(ErrorMessage ="{0}为必填项!")]
        [StringLength(20, ErrorMessage ="{0}应该在{2}-{1}之间!", MinimumLength =2)]
        [Remote("CheckSysNewsTitle", "Admin", ErrorMessage ="已有该公告,请勿重复添加!", HttpMethod ="POST")]
        public string Title { get; set; }

        [DisplayName("内容")]
        [Required(ErrorMessage = "{0}为必填项!")]
        [StringLength(1000, ErrorMessage = "{0}应该在{2}-{1}之间!", MinimumLength = 2)]
        public string Content { get; set; }

        [DisplayName("创建时间")]
        public DateTime CreateTime { get; set; }

        [DisplayName("是否阅读")]
        [Description("阅读之后就将其加入进来")]
        public virtual ICollection<User> IsRead { get; set; }
    }

    /// <summary>
    /// 意见反馈消息模型
    /// </summary>
    public class FBLetter
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string Content { get; set; }

        public DateTime CreateTime { get; set; }
    }

    public class Feedback
    {
        [DisplayName("标题")]
        [Required(ErrorMessage = "{0}为必填项!")]
        [StringLength(50, ErrorMessage = "{0}应该在{2}-{1}之间!", MinimumLength = 2)]
        public string Title { get; set; }

        [DisplayName("内容")]
        [Required(ErrorMessage = "{0}为必填项!")]
        [DataType(DataType.MultilineText)]
        [StringLength(1000, ErrorMessage = "{0}应该在{2}-{1}之间!", MinimumLength = 2)]
        public string Content { get; set; }
    }
}