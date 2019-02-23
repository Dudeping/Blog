using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Blog.Models
{
    /// <summary>
    /// 用户角色
    /// 两种:Administrator 和 User
    /// </summary>
    public class Role
    {
        [Key]
        public int Id { get; set; }

        [DisplayName("角色名")]
        [Required(ErrorMessage = "{0}为必填项!")]
        public string Name { get; set; }

        public virtual ICollection<UserLogin> UserLogin { get; set; }
    }
}