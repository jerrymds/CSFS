/// <summary>
/// 程式說明:Controller基類
/// 維護部門:資訊管理處
/// CSFS Version:v3.0
/// 中國信託銀行 版權所有  ©  All Rights Reserved.  
/// </summary>
using System;
using System.Data.Objects;
using System.Data.Objects.DataClasses;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using CTBC.CSFS.Resource;
using Microsoft.Practices.EnterpriseLibrary.Logging;

namespace CTBC.CSFS.Pattern
{
	//------------------------------------------------
	//	  以下為繼承CTBC.FrameWork.Platform元件,請勿更動
	//------------------------------------------------
	[ExceptionFilter]
	[HandleError]
	public class AppController : CTBC.FrameWork.Pattern.AppController
	{
        public CSFSLog ApLog = new CSFSLog();
		public AppController()
		{
			this.ValidateRequest = false;
		}

        protected override void OnActionExecuted(ActionExecutedContext context)
        {
            try
            {
                //20130418 horace
                if (Config.GetValue("TranLog").ToUpper() == "Y")//如果啟動TranLog記錄錄
                {
                    if (!this.ApLog.Categories.Contains("CSFS"))
                    {
                        this.ApLog.Categories.Add("CSFS");
                    }
                    WriteCSFSLog();

                    string _user = (HttpContext.Session["UserAccount"] != null) ? HttpContext.Session["UserAccount"].ToString() : "";
                    System.Collections.Specialized.NameValueCollection _parameters = ("POST".Equals(HttpContext.Request.HttpMethod)) ? HttpContext.Request.Form : HttpContext.Request.QueryString;
                    string _controller = (this.ControllerContext.RouteData.Values["Controller"] != null) ? this.ControllerContext.RouteData.Values["Controller"].ToString() : "";
                    string _action = (this.ControllerContext.RouteData.Values["Action"] != null) ? this.ControllerContext.RouteData.Values["Action"].ToString() : "";
                    string _ip = (HttpContext.Request != null && !string.IsNullOrEmpty(HttpContext.Request.ServerVariables["REMOTE_ADDR"])) ? HttpContext.Request.ServerVariables["REMOTE_ADDR"] : "";

                    ApLog.PersonalProcessLog(_user, _controller, _action, _ip, _parameters); //處理APLOG (個資)
                }
            }
            catch
            {

            }
        }

        public void GetUserSiteInfo()
        {
            //20130418 horace
            if (HttpContext.Session != null && HttpContext.Request != null && this.ControllerContext !=null)
            {
                this.ApLog.dic["UserId"] = (HttpContext.Session["UserAccount"] != null) ? HttpContext.Session["UserAccount"].ToString() : "Anonymous";
                this.ApLog.dic["FunctionId"] = "Action." + this.ControllerContext.RouteData.Values["Controller"] + "." + this.ControllerContext.RouteData.Values["Action"];
                this.ApLog.dic["SessionId"] = HttpContext.Session.SessionID;
                this.ApLog.dic["URL"] = HttpContext.Request.RawUrl;
                this.ApLog.dic["IP"] = HttpContext.Request.UserHostAddress;
                this.ApLog.dic["MachineName"] = HttpContext.Request.UserHostName;
                this.ApLog.dic["Result"] = Result.Success;
                this.ApLog.dic["TranFlag"] = TranFlag.After;
                this.ApLog.dic["ActionCode"] = GetActionCode(this.ControllerContext.RouteData.Values["Action"].ToString());
            }
            else {
                string tmp = "Session Lost";
                this.ApLog.dic["UserId"] = tmp;
                this.ApLog.dic["FunctionId"] = tmp;
                this.ApLog.dic["SessionId"] = tmp;
                this.ApLog.dic["URL"] = tmp;
                this.ApLog.dic["IP"] = tmp;
                this.ApLog.dic["MachineName"] = tmp;
                this.ApLog.dic["Result"] = Result.Failure;
                this.ApLog.dic["TranFlag"] = TranFlag.After;
                this.ApLog.dic["ActionCode"] = ActionCode.Error;
            }
        }

        public void WriteLog()
        {
            Logger.Write(this.ApLog);
            this.ApLog.Categories.Remove("CSFS");
        }

        public void WriteBaseInfo()
        {
            this.ApLog.Title = "CSFS";
            this.ApLog.Priority = 1;
            this.ApLog.EventId = 105;
            this.ApLog.Severity = System.Diagnostics.TraceEventType.Information;
            this.ApLog.TimeStamp = DateTime.Now;
            this.ApLog.ExtendedProperties = this.ApLog.dic;
        }


        public void WriteCSFSLog()
        {
            GetUserSiteInfo();
            WriteBaseInfo();
            WriteLog();
        }

        public static string GetActionCode(string ac)
        {
            if (!string.IsNullOrEmpty(ac))
            {
                if (ac.ToUpper().Contains("DEL")) return ActionCode.Delete;
                if (ac.ToUpper().Contains("ADD") || ac.ToUpper().Contains("CREATE")) return ActionCode.Create;
                if (ac.ToUpper().Contains("SAVE") || 
                    ac.ToUpper().Contains("EDIT") || 
                    ac.ToUpper().Contains("UPDATE") ||
                    ac.ToUpper().Contains("COPY") ||
                    ac.ToUpper().Contains("SEND") ||
                    ac.ToUpper().Contains("LOAD") ||
                    ac.ToUpper().Contains("COMPLETE") ||
                    ac.ToUpper().Contains("CANCEL")) 
                    return ActionCode.Update;
                if (ac.ToUpper().Contains("APPROVE")) return ActionCode.Approve;
            }
            return ActionCode.View;
        }
	}


	/// <summary>
	/// 異常處理
    /// </summary>
    #region
    public class ExceptionFilter : FilterAttribute, IExceptionFilter
    {
        private string _errId = "";
        private string _errDesc = "";
        private string _errDetail = "";
        private ExceptionContext _filterContext = null;
        /// <summary>
        /// 異常攔截
        /// </summary>
        /// <param name="filterContext"></param>
        void IExceptionFilter.OnException(ExceptionContext filterContext)
        {
            #region
            _errId = "CSFS-SYS-9999";
            _errDesc = filterContext.Exception.Message + " " + filterContext.Exception.Source;
            _errDetail = filterContext.Exception.ToString();
            _filterContext = filterContext;

            CSFSException csfsExp = new CSFSException();

            if (_filterContext.Exception is System.Data.SqlClient.SqlException)
            {
                csfsExp.errId = "CSFS-SYS-0001";
                csfsExp.errDesc = _errDesc;
                csfsExp.errDetail = _errDetail;
            }
            else if (_filterContext.Exception is CSFSException)
            {
                csfsExp = (CSFSException)filterContext.Exception;
                csfsExp.GetErrInfo();
            }
            else
            {
                csfsExp.errId = _errId;
                csfsExp.errDesc = _errDesc;
                csfsExp.errDetail = _errDetail;            
            }

            //寫異常log (1.錯誤分類 2.錯誤代碼 3.錯誤描述 4.Controller 5.Action 6.Stack(errDetail)，並定義嚴重等級Severity至System.Diagnostics.TraceEventType.Error)
            csfsExp.sverity = (_filterContext.Exception is System.Data.SqlClient.SqlException) ? "Critical" : "Error";
            csfsExp.controller = filterContext.RouteData.Values["Controller"].ToString();
            csfsExp.action = filterContext.RouteData.Values["Action"].ToString();
            WriteExceptionLog(csfsExp);

            //將欲呈現至網頁的錯誤訊息，裝飾至ViewData (包含給前端顯示的1.錯誤代碼、2.錯誤描述)
            //Error.aspx透過<%= Html.Encode(ViewData["ErrorMessage"])%>顯示錯誤訊息
            filterContext.Controller.ViewData["ErrId"] = csfsExp.errId;
            filterContext.Controller.ViewData["ErrDesc"] = csfsExp.errDesc;
            filterContext.Controller.ViewData["ErrTime"] = DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString();
            filterContext.Controller.ViewData["ErrType"] = "SQL or General Error";
      
            //-------------------------------------------------------------------------------
            //Request為Ajax Error時之錯誤處理
            //-------------------------------------------------------------------------------
            if (filterContext.HttpContext.Request.IsAjaxRequest())
            {
                filterContext.Result = new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new
                    {
                        errId = csfsExp.errId,
                        errDesc = csfsExp.errDesc//filterContext.Exception.Message
                    }
                };
                filterContext.ExceptionHandled = true;
                filterContext.HttpContext.Response.StatusCode = 500;
                filterContext.HttpContext.Response.TrySkipIisCustomErrors = true;
            }
            else
            {
                //-------------------------------------------------------------------------------
                //Request為一般Error時之錯誤處理
                //-------------------------------------------------------------------------------
                filterContext.Result = new ViewResult()
                {
                    ViewName = "Error",
                    ViewData = filterContext.Controller.ViewData
                };

                // 終止異常
                filterContext.ExceptionHandled = true;
                filterContext.HttpContext.Response.Clear();                
                filterContext.HttpContext.Response.StatusCode = 500;
                filterContext.HttpContext.Response.TrySkipIisCustomErrors = true;
            }
            #endregion
        }

        public void WriteExceptionLog(CSFSException csfsExp)
        {
            CSFSLog log = new CSFSLog();
            if (csfsExp == null)
            {
                csfsExp = new CSFSException();
                Exception eex = csfsExp.GetBaseException();
                csfsExp.errId = "CSFS-SYS-9999";
                csfsExp.errDesc = eex.Message + " " + eex.Source;
                csfsExp.errDetail = eex.ToString();
            }
            log.Categories.Add("Exception");
            log.Message = "errId=" + csfsExp.errId + ";errDesc=" + csfsExp.errDesc + ";errDetail=" + csfsExp.errDetail;
            log.TimeStamp = DateTime.Now;
            log.Title = "Exception";
            log.Priority = 1;
            log.EventId = 103;
            log.Severity = (csfsExp.sverity == "Critical") ? System.Diagnostics.TraceEventType.Critical : System.Diagnostics.TraceEventType.Error;
            //20130225 horace 加HttpContext.Current.Session == null時判斷
            if (HttpContext.Current.Session == null)
            {
                log.dic["UserId"] = "Anonymous";
                log.dic["SessionId"] = "Not Yet Asign a SessionId";
            }
            else
            {
                log.dic["UserId"] = (HttpContext.Current.Session["UserAccount"] != null) ? HttpContext.Current.Session["UserAccount"].ToString() : "Anonymous";
                log.dic["SessionId"] = HttpContext.Current.Session.SessionID;
            }
            log.dic["Result"] = Result.Failure;
            log.dic["ActionCode"] = ActionCode.Error;
            log.dic["TranFlag"] = TranFlag.After;
            log.dic["FunctionId"] = "Action." + csfsExp.controller + "." + csfsExp.action;
            log.dic["URL"] = HttpContext.Current.Request.RawUrl;
            log.dic["IP"] = HttpContext.Current.Request.UserHostAddress;
            log.dic["MachineName"] = HttpContext.Current.Request.UserHostName;
            log.ExtendedProperties = log.dic;
            Logger.Write(log);
        }
    }
    #endregion
    public class AppEntityController<C, E> : CTBC.FrameWork.Pattern.AppEntityController<C, E>
		where E : EntityObject
		where C : ObjectContext
	{
		public AppEntityController(C oc, E eo) : base(oc, eo) { }
	}
	//------------------------------------------------
	//      以上為繼承CTBC.FrameWork.Platform元件,請勿更動
	//------------------------------------------------
}
