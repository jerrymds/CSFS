using System.Web.Mvc;

namespace CTBC.CSFS.Areas.KeyInput
{
    public class KeyInputAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "KeyInput";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "KeyInput_default",
                "KeyInput/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}