using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace Blog.Models
{
    public class BlogContext : DbContext
    {
        // You can add custom code to this file. Changes will not be overwritten.
        // 
        // If you want Entity Framework to drop and regenerate your database
        // automatically whenever you change your model schema, please use data migrations.
        // For more information refer to the documentation:
        // http://msdn.microsoft.com/en-us/data/jj591621.aspx
    
        public BlogContext() : base("name=BlogDB")
        {
        }

        public DbSet<BlogInfo> Blogs { get; set; }

        public DbSet<User> Users { get; set; }

        public DbSet<UserLogin> UserLogins { get; set; }

        public DbSet<Role> Roles { get; set; }

        public DbSet<Category> Categories { get; set; }

        public DbSet<Log> Logs { get; set; }

        public DbSet<Letter> Letters { get; set; }

        public DbSet<SysNews> SysNews { get; set; }

        public DbSet<Collection> Collections { get; set; }

        public DbSet<WebConfig> WebConfigs { get; set; }
    }
}
