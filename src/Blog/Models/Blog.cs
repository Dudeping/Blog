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
    /// 随笔和文章数据模型
    /// </summary>
    public class Blog
    {
        [Key]
        public int Id { get; set; }

        [DisplayName("标题")]
        [Required(ErrorMessage ="{0}为必填项!")]
        [StringLength(50, ErrorMessage ="{0}应该在{2}-{1}位之间!", MinimumLength =2)]
        [Remote("CheckBlogTitle", "ManageBlogs", ErrorMessage ="你已发布该博客!请勿重复添加.",HttpMethod ="POST")]
        public string Title { get; set; }
        
        [DisplayName("内容")]
        [DataType(DataType.MultilineText)]
        [Required(ErrorMessage = "{0}为必填项!")]
        [StringLength(99999, ErrorMessage = "{0}应该在{2}-{1}位之间!", MinimumLength = 2)]
        public string Content { get; set; }

        [DisplayName("摘要")]
        public string Brief { get; set; }

        [DisplayName("发布时间")]
        [DataType(DataType.DateTime)]
        public DateTime CreateTime { get; set; }

        [DisplayName("修改时间")]
        [DataType(DataType.DateTime)]
        public DateTime EidtTime { get; set; }

        [DisplayName("收藏次数")]
        [Required(ErrorMessage = "{0}为必填项!")]
        [Description("这个作为推荐博客的辅助标准")]
        public int CollectionTimes { get; set; }

        [DisplayName("作者")]
        public virtual User Author { get; set; }

        [DisplayName("阅读次数")]
        [Description("这个为最热博客的标准")]
        [Required(ErrorMessage = "{0}为必填项!")]
        public int LookNum { get; set; }

        [DisplayName("类型")]
        [Required(ErrorMessage = "{0}为必填项!")]
        [Description("分随笔和文章两种")]
        public string BlogType { get; set; }

        [DisplayName("是否发布")]
        [Required(ErrorMessage = "{0}为必填项!")]
        [Description("没发布就在草稿箱中")]
        public bool IsRelease { get; set; }

        [DisplayName("是否公开")]
        [Required(ErrorMessage ="{0}为必填项!")]
        [Description("不公开别人无法看见你的这篇文章")]
        public bool IsPulish { get; set; }

        [DisplayName("是否发布到是首页")]
        [Required(ErrorMessage = "{0}为必填项!")]
        public bool IsPush { get; set; }

        [DisplayName("标签")]
        public string Tags { get; set; }

        [DisplayName("所属类别")]
        public virtual ICollection<Category> Categories { get; set; }
    }

    /// <summary>
    /// 博客流
    /// </summary>
    public class ShowBlog
    {
        [Key]
        public int Id { get; set; }

        [DisplayName("标题")]
        public string Title { get; set; }

        [DisplayName("摘要")]
        public string Brief { get; set; }

        [DisplayName("发布时间")]
        [DataType(DataType.DateTime)]
        public DateTime CreateTime { get; set; }

        [DisplayName("收藏次数")]
        public int CollectionTimes { get; set; }

        [DisplayName("作者Id")]
        public int AuthorId { get; set; }

        [DisplayName("作者")]
        public string Author { get; set; }

        [DisplayName("阅读次数")]
        public int LookNum { get; set; }

        [DisplayName("类型")]
        public string BlogType { get; set; }
    }

    /// <summary>
    /// 创建和编辑博客模板
    /// </summary>
    public class BlogModel
    {
        [Key]
        public int Id { get; set; }

        [DisplayName("标题")]
        [Required(ErrorMessage = "{0}为必填项!")]
        [StringLength(50, ErrorMessage = "{0}应该在{2}-{1}位之间!", MinimumLength = 2)]
        [Remote("CheckBlogTitle", "ManageBlogs", ErrorMessage = "你已发布该博客!请勿重复发布.", HttpMethod = "POST")]
        public string Title { get; set; }

        [DisplayName("内容")]
        [DataType(DataType.MultilineText)]
        [Required(ErrorMessage = "{0}为必填项!")]
        [StringLength(99999, ErrorMessage = "{0}应该在{2}-{1}位之间!", MinimumLength = 2)]
        public string Content { get; set; }

        [DisplayName("类型")]
        [Required(ErrorMessage = "{0}为必填项!")]
        public string BlogType { get; set; }

        [DisplayName("标签")]
        [StringLength(40, ErrorMessage = "{0}应该在小于{1}位!")]
        public string Tags { get; set; }

        [DisplayName("类别")]
        public int[] Category { get; set; }

        [DisplayName("是否发布")]
        public bool IsRelease { get; set; }

        [DisplayName("是否公开")]
        [Description("不公开别人无法看见你的这篇文章")]
        public bool IsPulish { get; set; }

        [DisplayName("是否发布到是首页")]
        public bool IsPush { get; set; }
    }

    /// <summary>
    /// 博客类别
    /// </summary>
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [DisplayName("类别名")]
        public string Name { get; set; }

        /// <summary>
        /// 类别作者
        /// system：表示系统类别
        /// </summary>
        [DisplayName("标签作者")]
        public string User { get; set; }

        [DisplayName("博客数")]
        public int BlogCount { get; set; }

        [DisplayName("博客")]
        public virtual ICollection<Blog> Blogs { get; set; }
    }

    /// <summary>
    /// 后台添加博客类别
    /// </summary>
    public class CreateCategory
    {
        [DisplayName("类别名")]
        [Required(ErrorMessage ="{0}为必填项！")]
        [StringLength(20, ErrorMessage ="{0}应该少于{1}字！")]
        [Remote("CheckCategoryName", "Admin", HttpMethod ="POST", ErrorMessage ="已有该类别，请别重复添加！")]
        public string Name { get; set; }
    }

    /// <summary>
    /// 前台添加博客类别
    /// </summary>
    public class CategoryModel
    {
        [DisplayName("类别名")]
        [Required(ErrorMessage = "{0}为必填项！")]
        [StringLength(20, ErrorMessage = "{0}应该少于{1}字！")]
        [Remote("CheckCategoryName", "ManageBlogs", HttpMethod = "POST", ErrorMessage = "已有该类别，请别重复添加！")]
        public string Name { get; set; }
    }

    /// <summary>
    /// 收藏数据模型
    /// </summary>
    public class Collection
    {
        [Key]
        public int Id { get; set; }

        [DisplayName("博客ID")]
        public Blog BlogId { get; set; }

        [DisplayName("用户ID")]
        [Required(ErrorMessage = "{0}为必填项!")]
        public User UserId { get; set; }
    }

    /// <summary>
    /// 上一篇和下一篇博客模板
    /// </summary>
    public class PANBlog
    {
        public int Id { get; set; }

        public string Title { get; set; }
    }

    /// <summary>
    /// 前台管理博客模型
    /// </summary>
    public class ManageBlog
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public DateTime CreateTime { get; set; }

        public int LookNum { get; set; }

        public bool IsPulish { get; set; }
    }

    /// <summary>
    /// 后台管理博客模型
    /// </summary>
    public class AdminManageBlog
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public DateTime EidtTime { get; set; }

        public int LookNum { get; set; }

        public bool IsPush { get; set; }

        public virtual User Author { get; set; }

        public string BlogType { get; set; }

        public Category Category { get; set; }
    }

    /// <summary>
    /// 收藏博客数据模型
    /// </summary>
    public class BlogCollection
    {
        public int Id { get; set; }

        public int CId { get; set; }

        public string Title { get; set; }

        public DateTime EidtTime { get; set; }

        public int LookNum { get; set; }

        public virtual User Author { get; set; }
    }

    /// <summary>
    /// 博客类型
    /// </summary>
    public enum BlogType
    {
        Jotting,   //随笔
        Article //文章
    }
}