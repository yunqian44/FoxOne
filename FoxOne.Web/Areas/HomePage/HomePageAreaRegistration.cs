using System.Web.Mvc;

namespace FoxOne.Web.Areas.HomePage
{
    public class HomePageAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "HomePage";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "HomePage_default",
                "HomePage/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
