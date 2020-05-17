using System.Web.Mvc;

namespace FoxOne.Web.Areas.Vote
{
    public class VoteAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Vote";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "Vote_default",
                "Vote1/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
