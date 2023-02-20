using System.Web.Mvc;

namespace CTBC.CSFS.Areas.CaseCust
{
    public class CaseCustAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "CaseCust";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "CaseCust_default",
                "CaseCust/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}