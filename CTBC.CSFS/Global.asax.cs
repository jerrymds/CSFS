using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;
using System.Web.SessionState;
using System.Configuration;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using CTBC.CSFS.Models;
using CTBC.CSFS.Pattern;
using CTBC.FrameWork.Platform;
using CTBC.CSFS.BussinessLogic;
//////using CTBC.CSFS.Util;

namespace CTBC.CSFS
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_AcquireRequestState(object sender, EventArgs e)
        {
            if (HttpContext.Current.Session != null)
            {
                CultureInfo cultureInfo = (CultureInfo)this.Session["Culture"];

                // 判断Session中是否有值，没有就设置默认值
                if (cultureInfo == null)
                {
                    string langName = "zh-TW";
                    //if (HttpContext.Current.Request.UserLanguages != null && HttpContext.Current.Request.UserLanguages.Length != 0)
                    //{
                    //    langName = HttpContext.Current.Request.UserLanguages[0];
                    //}
                    cultureInfo = new CultureInfo(langName);
                    this.Session["Culture"] = cultureInfo;
                }

                System.Threading.Thread.CurrentThread.CurrentCulture = cultureInfo;
                System.Threading.Thread.CurrentThread.CurrentUICulture = cultureInfo;
                System.Threading.Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(cultureInfo.Name);
                System.Threading.Thread.CurrentThread.CurrentUICulture = CultureInfo.CreateSpecificCulture(cultureInfo.Name);
            }
        }

        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.MapPageRoute(
                "ReportRoute",                         // Route name      
                "Reports/{reportname}",                // URL      
                "~/Reports/{reportname}.aspx"   // File      
                );
            routes.MapRoute(
                "Default", // 路由名稱
                "{controller}/{action}/{id}", // URL 及參數
                new { controller = "Home", action = "Index", id = UrlParameter.Optional } // 參數預設值
            );
        }

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            //欲放到Cache中的資料,由此Upload上去
            CacheManager upToCache = new CacheManager();
            upToCache.UploadToCache();
            Dictionary<string, User> OnlineUserList = new Dictionary<string, User>();
            if (CTBC.FrameWork.Platform.AppCache.InCache("ONLINE_USER_LIST")) CTBC.FrameWork.Platform.AppCache.Erase("ONLINE_USER_LIST");
            CTBC.FrameWork.Platform.AppCache.Add(OnlineUserList, "ONLINE_USER_LIST");
        }

        //20130314 horace 
        protected void Session_End(object sender, EventArgs e)
        {
            if (CTBC.FrameWork.Platform.AppCache.InCache("ONLINE_USER_LIST"))
            {
                Dictionary<string, User> ursLst2 = (Dictionary<string, User>)CTBC.FrameWork.Platform.AppCache.Get("ONLINE_USER_LIST");
                if (!string.IsNullOrEmpty((string)Session["UserAccount"]))
                {
                    if (ursLst2.ContainsKey((string)Session["UserAccount"]))
                    {
                        ursLst2.Remove((string)Session["UserAccount"]);
                        CTBC.FrameWork.Platform.AppCache.Update(ursLst2, "ONLINE_USER_LIST");
                    }
                }
            }
        }

        protected void Session_Start(object sender, EventArgs e)
        {
            //////if (CTBC.FrameWork.Platform.AppCache.InCache("ONLINE_USER_LIST"))
            //////{
            //////    Dictionary<string, User> ursLst2 = (Dictionary<string, User>)CTBC.FrameWork.Platform.AppCache.Get("ONLINE_USER_LIST");
            //////    if (!string.IsNullOrEmpty((string)Session["UserAccount"]))
            //////    {
            //////        if (ursLst2.ContainsKey((string)Session["UserAccount"]))
            //////        {
            //////            ursLst2.Remove((string)Session["UserAccount"]);
            //////            CTBC.FrameWork.Platform.AppCache.Update(ursLst2, "ONLINE_USER_LIST");
            //////        }
            //////    }
            //////}
        }

        //處理程式無法攔截的錯誤
        protected void Application_Error(object sender, EventArgs e)
        {
            var httpContext = ((MvcApplication)sender).Context;
            var currentRouteData = RouteTable.Routes.GetRouteData(new HttpContextWrapper(httpContext));
            var currentController = " ";
            var currentAction = " ";
            if (currentRouteData != null)
            {
                if (currentRouteData.Values["controller"] != null && !String.IsNullOrEmpty(currentRouteData.Values["controller"].ToString()))
                {
                    currentController = currentRouteData.Values["controller"].ToString();
                }

                if (currentRouteData.Values["action"] != null && !String.IsNullOrEmpty(currentRouteData.Values["action"].ToString()))
                {
                    currentAction = currentRouteData.Values["action"].ToString();
                }
            }

            //--------------------------------------------------------------------------------
            //判斷錯誤類型
            //--------------------------------------------------------------------------------
            var controller = new CTBC.CSFS.Controllers.CSFSErrorController();
            var routeData = new RouteData();
            var ex = Server.GetLastError();
            var action = "Index";//20150216弱掃只留此定義

            //--------------------------------------------------------------------------------
            //處理記錄錯誤Log
            //--------------------------------------------------------------------------------
            ExceptionFilter exa = new ExceptionFilter();
            CSFSException csfsExp = null;
            if (ex.InnerException != null)
            {
                if (ex.InnerException.InnerException is CSFSException)
                {
                    csfsExp = (CSFSException)ex.InnerException.InnerException;
                    csfsExp.GetErrInfo();
                }
                if (ex is System.Web.HttpUnhandledException)
                {
                    csfsExp = new CSFSException
                    {
                        message = ex.InnerException.Message,
                        errId = "CSFS-SYS-9999",
                        errDesc = ex.InnerException.Message + " " + ex.InnerException.Source,
                        errDetail = ex.InnerException.ToString()
                    };//ex.InnerException.Message,ex.InnerException);                    
                }
                else csfsExp = (CSFSException)ex.InnerException;
            }
            else
            {
                csfsExp = new CSFSException
                {
                    errId = (action == "NotFound") ? "CSFS-SYS-0002" : "CSFS-SYS-9999",
                    errDesc = ex.Message + " " + ex.Source,
                    errDetail = ex.ToString(),
                    controller = currentController,
                    action = currentAction,
                    sverity = "Error"
                };
            }
            exa.WriteExceptionLog(csfsExp);

            //--------------------------------------------------------------------------------
            //拋出錯誤訊息與畫面
            //--------------------------------------------------------------------------------
            httpContext.ClearError();
            httpContext.Response.Clear();
            httpContext.Response.StatusCode = ex is HttpException ? ((HttpException)ex).GetHttpCode() : 500;
            httpContext.Response.TrySkipIisCustomErrors = true;
            routeData.Values["controller"] = "CSFSError";
            routeData.Values["action"] = action;

            if (ex.InnerException != null)
            {
                if (csfsExp == null)
                {
                    csfsExp = new CSFSException(ex.Message, null);
                    controller.ViewData.Model = new HandleErrorInfo(ex, currentController, currentAction);
                }
                else
                {
                    controller.ViewData["ErrId"] = csfsExp.errId;
                    controller.ViewData["ErrDesc"] = csfsExp.errDesc;
                    if (ex.InnerException.InnerException is CSFSException)
                    {
                        controller.ViewData.Model = new HandleErrorInfo(ex.InnerException.InnerException, currentController, currentAction);
                    }
                    else
                    {
                        //ex is System.Web.HttpUnhandledException
                        controller.ViewData.Model = new HandleErrorInfo(ex, currentController, currentAction);
                    }
                }
            }
            else
            {
                controller.ViewData.Model = new HandleErrorInfo(ex, currentController, currentAction);
            }
            ((IController)controller).Execute(new RequestContext(new HttpContextWrapper(httpContext), routeData));
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            try
            {
                //------------------------------
                //上傳資料長度過長之錯誤處理
                //------------------------------
                HttpRuntimeSection section = (HttpRuntimeSection)ConfigurationManager.GetSection("system.web/httpRuntime");
                if (Request.ContentLength > (section.MaxRequestLength * 1024))
                {
                    HttpContext context = ((HttpApplication)sender).Context;
                    IServiceProvider provider = (IServiceProvider)context;
                    HttpWorkerRequest workerRequest = (HttpWorkerRequest)provider.GetService(typeof(HttpWorkerRequest));
                    // Check if body contains data
                    if (workerRequest.HasEntityBody())
                    {
                        // get the total body length
                        int requestLength = workerRequest.GetTotalEntityBodyLength();
                        // Get the initial bytes loaded
                        int initialBytes = 0;
                        if (workerRequest.GetPreloadedEntityBody() != null)
                            initialBytes = workerRequest.GetPreloadedEntityBody().Length;
                        if (!workerRequest.IsEntireEntityBodyIsPreloaded())
                        {
                            byte[] buffer = new byte[512000];
                            // Set the received bytes to initial bytes before start reading
                            int receivedBytes = initialBytes;
                            while (requestLength - receivedBytes >= initialBytes)
                            {
                                // Read another set of bytes
                                initialBytes = workerRequest.ReadEntityBody(buffer, buffer.Length);
                                // Update the received bytes
                                receivedBytes += initialBytes;
                            }
                            initialBytes = workerRequest.ReadEntityBody(buffer, requestLength - receivedBytes);
                        }
                    }
                    // Redirect the user to the same page with querystring action=exception.
                    //context.Response.Redirect(this.Request.Url.LocalPath + "?action=overlength");
                    //context.Response.Redirect("GoCatchSection.aspx");
                    
                    Response.Redirect("~/CSFSError/OverLength");
                }
            }
            catch (Exception ex)
            {
                Response.Redirect("~/CSFSError/OverLength");
                //Response.Redirect("~/CSFSError/Index?errId=CSFS-SYS-9997&errDesc=" + ex.Message);
            }
        }
    }
}
