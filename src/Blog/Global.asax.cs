using System;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;

namespace Blog
{
    public class MvcApplication : HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        protected MvcApplication()
        {
            BeginRequest += MvcApplication_BeginRequest;
            AuthorizeRequest += MvcApplication_AuthorizeRequest;
        }
        
        private void MvcApplication_AuthorizeRequest(object sender, EventArgs e)
        {
            // 处理角色信息
            var id = Context.User.Identity as FormsIdentity;
            if (id != null && id.IsAuthenticated)
            {
                var roles = id.Ticket.UserData.Split(',');
                Context.User = new GenericPrincipal(id, roles);
            }
        }
        
        private void MvcApplication_BeginRequest(object sender, EventArgs e)
        {
            #if DEBUG
            
            #else
            // 301永久定向
            if (!Request.IsSecureConnection)
            {
                var newUrl = new UriBuilder(Request.Url)
                {
                    Port = 443,
                    Scheme = "HTTPS"
                }.ToString();

                Response.RedirectPermanent(newUrl);
            }
            #endif
        }
    }
}
