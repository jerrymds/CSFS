using System;
using System.Web;
using System.Web.Caching;
using System.Web.Mvc;
using System.Web.Routing;

namespace CTBC.CSFS.Filter
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class RootPageFilterAttribute : ActionFilterAttribute
    {
        /// <summary>
        /// 如果當前頁面是菜單中的頁面.則可以添加此filter.route信息寫入cookies以便返回時使用
        /// </summary>
        /// <param name="filterContext"></param>
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            string controller = Convert.ToString(filterContext.RouteData.Values["controller"]);
            string action = Convert.ToString(filterContext.RouteData.Values["action"]);
            string area = Convert.ToString(filterContext.RouteData.DataTokens["area"]);
            HttpCookie route = new HttpCookie("RouteCookie");
            route.Values.Add("area", area);
            route.Values.Add("action", action);
            route.Values.Add("controller", controller);
            filterContext.HttpContext.Response.Cookies.Add(route);
            base.OnActionExecuting(filterContext);
        }
    }
}