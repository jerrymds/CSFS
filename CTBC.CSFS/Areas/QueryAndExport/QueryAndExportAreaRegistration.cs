using System.Web.Mvc;

namespace CTBC.CSFS.Areas.QueryAndExport
{
    public class QueryAndExportAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "QueryAndExport";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "QueryAndExport_default",
                "QueryAndExport/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}