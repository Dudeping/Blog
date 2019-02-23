using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Blog.Models
{
    public class Log
    {
        [Key]
        public int Id { get; set; }

        [DisplayName("标题")]
        public string Title { get; set; }

        [DisplayName("类型")]
        public string LogType { get; set; }

        [DisplayName("内容")]
        public string Content { get; set; }

        [DisplayName("是否已读")]
        public bool IsRead { get; set; }

        [DisplayName("操作人")]
        public string User { get; set; }

        [DisplayName("时间")]
        public DateTime CreateTime { get; set; }

        [DisplayName("IP地址")]
        public string Ip { get; set; }

        [DisplayName("IP所在地区")]
        public string IpLocation { get; set; }
    }

    public enum LogType
    {
        add,        // 添加博客
        register,   // 注册
        error,      // 系统错误
        login,      // 登录
        danger      // 高危操作
    }
}