using System.Web.Mvc;
using System.Web.Routing;

namespace BlogPlus
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Home",
                url: "{controller}/{action}/{id}.html",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional },
                constraints: new
                {
                    controller = "Home",
                    action = "Index"
                }
            );

            routes.MapRoute(
                name: "BlogDetail",
                url: "{controller}/{action}/{id}.html",
                defaults: new { controller = "Users", action = "BlogDetails", id = UrlParameter.Optional },
                constraints: new
                {
                    controller = "Users",
                    action = "BlogDetails"
                }
            );

            routes.MapRoute(
                name: "Categoty",
                url: "{controller}/{action}/{page}/{id}.html",
                defaults: new { controller = "Users", action = "Category", page = 1, id = UrlParameter.Optional },
                constraints: new
                {
                    controller = "Users",
                    action = "Category"
                }
            );

            routes.MapRoute(
                name: "UserIndex",
                url: "{controller}/{action}/{id}/{page}.html",
                defaults: new { controller = "Users", action = "Index", page = 1, id = UrlParameter.Optional },
                constraints: new
                {
                    controller = "Users",
                    action = "Index"
                }
            );

            routes.MapRoute(
                name: "EditBlog",
                url: "{controller}/{action}/{id}.html",
                defaults: new { controller = "ManageBlogs", action = "Edit", id = UrlParameter.Optional },
                constraints: new
                {
                    controller = "ManageBlogs",
                    action = "Edit"
                }
            );

            routes.MapRoute(
                name: "HomePage",
                url: "{controller}/{action}.html",
                defaults: new { controller = "Home", action = "Index" }
            );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
