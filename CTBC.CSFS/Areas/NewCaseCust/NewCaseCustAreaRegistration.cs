using System.Web.Mvc;

namespace CTBC.CSFS.Areas.NewCaseCust
{
    public class NewCaseCustAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "NewCaseCust";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "NewCaseCust_default",
                "NewCaseCust/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}