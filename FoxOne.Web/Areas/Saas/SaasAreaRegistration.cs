using System.Web.Mvc;

namespace FoxOne.Web.Areas.Saas
{
    public class SaasAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Saas";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "Saas_default",
                "Saas/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
