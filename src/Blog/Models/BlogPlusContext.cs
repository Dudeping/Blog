using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace BlogPlus.Models
{
    public class BlogPlusContext : DbContext
    {
        // You can add custom code to this file. Changes will not be overwritten.
        // 
        // If you want Entity Framework to drop and regenerate your database
        // automatically whenever you change your model schema, please use data migrations.
        // For more information refer to the documentation:
        // http://msdn.microsoft.com/en-us/data/jj591621.aspx
    
        public BlogPlusContext() : base("name=BlogPlusContext")
        {
        }

        public System.Data.Entity.DbSet<BlogPlus.Models.Blog> Blogs { get; set; }

        public System.Data.Entity.DbSet<BlogPlus.Models.User> Users { get; set; }

        public System.Data.Entity.DbSet<BlogPlus.Models.UserLogin> UserLogins { get; set; }

        public System.Data.Entity.DbSet<BlogPlus.Models.Role> Roles { get; set; }

        public System.Data.Entity.DbSet<BlogPlus.Models.Category> Categories { get; set; }

        public System.Data.Entity.DbSet<BlogPlus.Models.Log> Logs { get; set; }

        public System.Data.Entity.DbSet<BlogPlus.Models.Letter> Letters { get; set; }

        public System.Data.Entity.DbSet<BlogPlus.Models.SysNews> SysNews { get; set; }

        public System.Data.Entity.DbSet<BlogPlus.Models.Collection> Collections { get; set; }

        public System.Data.Entity.DbSet<BlogPlus.Models.WebConfig> WebConfigs { get; set; }
    }
}
