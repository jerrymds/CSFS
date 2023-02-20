using System.Web;
using System.Web.Mvc;

namespace CTBC.CSFS
{
    public static class SiteHelper
    {
        /// <summary>
        /// 生成Tab中的返回按鈕.
        /// </summary>
        /// <param name="helper"></param>
        /// <returns></returns>
        public static HtmlString GetBackButtonInTab(this HtmlHelper helper)
        {
            string rtn = "";
            try
            {
                string backUrl = GetBackUrl(helper);
                if (!string.IsNullOrEmpty(backUrl))
                {
                    if (backUrl == "/CaseCust/CaseCustManager?isBack=1"|| backUrl== "/CaseCust/CaseCustHistory?isBack=1")
                    {
                        rtn = @"
                                <a href='" + backUrl + @"' class='btn btn-default btn-xs pull-right'>
                                    <i class='glyphicon glyphicon-chevron-left'></i>
                                </a>
                                ";
                    }
                    else
                    {
                        rtn = @"<li class='pull-right'>
                                        <div style='padding: 10px 15px;'>
                                            <a href='" + backUrl + @"' class='btn btn-default btn-xs'>
                                                <i class='glyphicon glyphicon-chevron-left'></i>
                                            </a>
                                        </div>
                                </li>";
                    }
                }
            }
            catch
            {

            }
            return new MvcHtmlString(rtn);
        }

        /// <summary>
        /// 取得Cookies中返回的Url
        /// </summary>
        /// <param name="helper"></param>
        /// <returns></returns>
        public static string GetBackUrl(this HtmlHelper helper)
        {
            try
            {
                //* 取得cookie 這cookie在菜單根目錄的RootPageFilterAttribute中有寫
                HttpCookie cookies = helper.ViewContext.HttpContext.Request.Cookies.Get("RouteCookie");
                if (cookies != null)
                {
                    string strArea = cookies.Values["area"];
                    string strController = cookies.Values["controller"];
                    string strAction = cookies.Values["action"];

                    if (!string.IsNullOrEmpty(strController) && !string.IsNullOrEmpty(strAction))
                    {
                        UrlHelper url = new UrlHelper(helper.ViewContext.RequestContext);
                        string strUrl = url.Action(strAction, strController, new { area = strArea, isBack = 1 });
                        return strUrl;
                    }
                }
            }
            catch
            {

            }
            return "";
        }

        /// <summary>
        /// 取得返回的ControllerName
        /// </summary>
        /// <param name="helper"></param>
        /// <returns></returns>
        public static string GetBackControllerName(this HtmlHelper helper)
        {
            try
            {
                //* 取得cookie 這cookie在菜單根目錄的RootPageFilterAttribute中有寫
                HttpCookie cookies = helper.ViewContext.HttpContext.Request.Cookies.Get("RouteCookie");
                if (cookies != null)
                {
                    string strController = cookies.Values["controller"];
                    return strController;
                }
            }
            catch
            {

            }
            return "";
        }
    }
}