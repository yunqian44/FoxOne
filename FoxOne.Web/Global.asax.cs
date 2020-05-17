using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Http;

namespace FoxOne.Web
{

    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            WebApiRegister(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            RegisterCenter.RegisterType();
            RegisterCenter.RegisterEntityEvent();
        }

        public static void WebApiRegister(HttpConfiguration config)
        {
            config.Routes.MapHttpRoute(name: "uc", routeTemplate: "api/UC/{action}/{id}/{systemId}", defaults: new {
                controller = "UC",
                id = RouteParameter.Optional,
                systemId = RouteParameter.Optional
            });
            config.Routes.MapHttpRoute(name: "crud", routeTemplate: "api/{id}", defaults: new { controller = "CRUD", id = RouteParameter.Optional });
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.MapRoute(
                name: "PagePreview",
                url: "Page/PreView",
                defaults: new { controller = "Page", action = "PreView" }
            );
            routes.MapRoute(
                name: "Page",
                url: "Page/{pageId}/{ctrlId}",
                defaults: new { controller = "Page", action = "Index", ctrlId = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Entity",
                url: "Entity/{action}/{entityName}/{id}",
                defaults: new { controller = "Entity", action = "List", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "LogOn", id = UrlParameter.Optional }
            );
        }
    }
}