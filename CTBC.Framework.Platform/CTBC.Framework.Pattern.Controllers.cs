using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Reflection;
using System.Xml;
using System.IO;
using CTBC.FrameWork.Platform;
using Microsoft.Practices.EnterpriseLibrary.Logging;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Validation;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.Objects;
using System.Data.Objects.DataClasses;
using System.Data.Entity;
using System.Web.Security;
using System.Threading;//20140813 horace

namespace CTBC.FrameWork.Pattern
{
	public static class PagingExtensions
	{
		//used by LINQ to SQL
		public static IQueryable<TSource> Page<TSource>(this IQueryable<TSource> source, int page, int pageSize)
		{
			return source.Skip((page - 1) * pageSize).Take(pageSize);
		}

		//used by LINQ
		public static IEnumerable<TSource> Page<TSource>(this IEnumerable<TSource> source, int page, int pageSize)
		{
			return source.Skip((page - 1) * pageSize).Take(pageSize);
		}
	}

    public class AppController : System.Web.Mvc.Controller
	{
		// properties
        public string ControllerName = "";					// this controller name
		public string ActionName = "";						// current action name
		public string Actions = "";							// authorized action codes to this controller
		public string AuthZCode = "*V";						// the action codes needed to access this controller
		public string ActionNamePrefix = "Action";			// action name prefix. Action time format: <prefix>.<controller>.<action>
		public string ControllerNamePrefix = "Controller";	// controller name prefix. Contrller name format : <prefix>.<controller>
		public string FunctionId = "";						// associated function ID of this controller
		public string LocationId = "";						// current controller location ID
		public User LogonUser = null;
        static string UnauthErrId = "CSFS-SYS-0003";
        static string UnauthErrDesc = "抱歉，您未被授權使用此功能!";
		public XmlDocument dataDom = new XmlDocument();		// XML data DOM post from client
		public AppLog AppLog = new AppLog();
		public AppException AppException = new AppException();
		public AppValidator AppValidator = new AppValidator();
		public LogWriter LogWriter = null;

		// constructor
		public AppController()
		{
			// init applog
		}

		// get authorization action code
		public string AuthZ()
		{
			this.FunctionId = this.ControllerNamePrefix + "." + this.ControllerName;
			try { this.Actions = this.LogonUser.GetAuthZActions(this.FunctionId, ""); } catch { }
			return this.Actions;
		}

        public void WriteEntryLog()
        {
            //if (this.AppLog.CheckEntryLog(this.FunctionId, "") == false) return;
            this.AppLog.Categories.Add(AppLog.CUF_Log_FuncEntryCategory);
            this.AppLog.EventId = AppLog.CUF_Log_EntryEventID;
            this.AppLog.FunctionId = this.FunctionId;
            this.AppLog.Message = "Function " + this.FunctionId + " Entry!";
            this.AppLog.Title = this.AppLog.Message;
            this.AppLog.Severity = System.Diagnostics.TraceEventType.Information;
            this.AppLog.Priority = 3;
            this.LogWriter.Write(this.AppLog);
        }

        public void WriteExitLog()
        {
            //if (this.AppLog.CheckExitLog(this.FunctionId, "") == false) return;
            this.AppLog.Categories.Add(AppLog.CUF_Log_FuncExitCategory);
            this.AppLog.EventId = AppLog.CUF_Log_ExitEventID;
            this.AppLog.FunctionId = this.FunctionId;
            this.AppLog.Message = "Function " + this.FunctionId + " Exit!";
            this.AppLog.Title = this.AppLog.Message;
            this.AppLog.Severity = System.Diagnostics.TraceEventType.Information;
            this.AppLog.Priority = 3;
            this.LogWriter.Write(this.AppLog);
        }

		public void WriteLocLog(string locationId)
		{
		}

		protected override void OnActionExecuting(ActionExecutingContext filterContext)
		{
            //-------------------------------------------------
            //20130323 horace 取得Controller Name與Action Name
            //-------------------------------------------------
			this.ControllerName = filterContext.ActionDescriptor.ControllerDescriptor.ControllerName;
			this.ActionName = filterContext.ActionDescriptor.ActionName;

            try
            {
                //---------------------------------------------
                //20130323 horace 取出Session["LogonUser"],若Session已斷掉時,導至登入頁
                //---------------------------------------------
                if (Session["LogonUser"] != null)
                {
                    this.LogonUser = (User)Session["LogonUser"];
                }
                else
                {
                    if ((Session["LogonUser"] == null && this.ControllerName != "Home" && this.ControllerName != "Login"))
                    {
                        this.Response.Cache.SetExpires(DateTime.UtcNow.AddMinutes(-1));
                        this.Response.Cache.SetCacheability(HttpCacheability.NoCache);
                        this.Response.Cache.SetNoStore();
                        this.Response.Cookies.Clear();
                        FormsAuthentication.SignOut();
                        Session.Clear();
                        Session.Abandon();
                        filterContext.Result = this.Redirect("~/Home/Login");//20130323 horace
                    }
                }
            }
            catch {}//20130323 horace

            //----------------------------------
            //20130313 horace記錄此Action執行時間
            //----------------------------------
            if (CTBC.FrameWork.Platform.AppCache.InCache("ONLINE_USER_LIST"))
            {
                Dictionary<string, User> ursLst2 = (Dictionary<string, User>)CTBC.FrameWork.Platform.AppCache.Get("ONLINE_USER_LIST");
                //* modify by Ge.Song
                if (Session != null && Session["UserAccount"] != null)
                {
                    if (ursLst2.ContainsKey((string)Session["UserAccount"]))
                    {
                        ursLst2[(string)Session["UserAccount"]].LastActiveTime = DateTime.Now;
                        CTBC.FrameWork.Platform.AppCache.Update(ursLst2, "ONLINE_USER_LIST");
                    }
                }
            } 

            //-------------------------------------------------
            //init log writer
            //-------------------------------------------------
			this.LogWriter = this.AppLog.Writer;

            //-------------------------------------------------
            //read Menu權限data DOM if any
            //-------------------------------------------------
			try {
                StreamReader lo_Reader = new StreamReader(Request.InputStream);
                this.dataDom.LoadXml(lo_Reader.ReadToEnd());
            }
            catch (System.Runtime.InteropServices.ExternalException exc)
            {
                //throw exc;
                filterContext.Controller.ViewData["ErrId"] = "CSFS-SYS-9997";
                filterContext.Controller.ViewData["ErrDesc"] = this.ControllerName + "." + this.ActionName;
                filterContext.Controller.ViewData["ErrTime"] = DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString();
                filterContext.Controller.ViewData["ErrType"] = "Over Length";
                filterContext.Result = new ViewResult()
                {
                    ViewName = "Error",
                    ViewData = filterContext.Controller.ViewData
                };
                return;
            }
            catch {}

            #region 20130323 horace mark
			// get logon User role and authorizations
            //if (ViewBag.User == null)
            //    try	{
            //        //20130311 horace改取Session["LogonUser"]中的user物件,若已無該User物件,則直接登出.
            //        if (Session["LogonUser"] != null)
            //        {
            //            this.LogonUser = (User)Session["LogonUser"];
            //            ViewBag.User = this.LogonUser;
            //        }
            //        else {
            //            //20130308 horace改放入AppCach("ONLINE_USER_LIST")的Key=登入員編,若AppCache已無該User物件,則直接登出.
            //            Dictionary<string, User> userLst = (Dictionary<string, User>)AppCache.Get("ONLINE_USER_LIST");
            //            if(userLst.ContainsKey((string)Session["UserAccount"]))
            //            {
            //                this.LogonUser = (User)userLst[(string)Session["UserAccount"]]; 
            //            }
            //            if (this.LogonUser == null && this.ControllerName != "Home" && this.ControllerName != "Login")
            //            {
            //                this.Response.Cache.SetExpires(DateTime.UtcNow.AddMinutes(-1));
            //                this.Response.Cache.SetCacheability(HttpCacheability.NoCache);
            //                this.Response.Cache.SetNoStore();
            //                this.Response.Cookies.Clear();
            //                FormsAuthentication.SignOut();
            //                Session.Clear();
            //                Session.Abandon();
            //                filterContext.Result = this.Redirect("~/");//20130323
            //            }
            //            ViewBag.User = this.LogonUser;
            //        }
            //    }
            //    catch { }
            //----------------------------------------------------------------------------------
            #endregion

            //-------------------------------------------------
            //get authorized action codes for this controller
            //-------------------------------------------------
			this.AuthZ();

            //-------------------------------------------------
            //Logout 處理
            //-------------------------------------------------
            #region
            // is it a logout?
            if (this.ActionName == "Logout")
            {
                //Get Logout Log
                LogEntry log = new LogEntry();
                log.Message = "";
                log.Categories.Add("LogonLogout");
                log.Priority = 1;
                log.EventId = 102;
                log.TimeStamp = DateTime.Now;
                log.Severity = System.Diagnostics.TraceEventType.Stop;
                log.Title = "Logout";
                Dictionary<string, object> dic = new Dictionary<string, object>();
                string usrAcc = "";
                if (this.LogonUser == null)
                {
                    if (Session["UserAccount"] != null)
                        usrAcc = (string)Session["UserAccount"];
                    else usrAcc = "Anonymous";
                }
                else {
                    if (!string.IsNullOrEmpty(this.LogonUser.Account))
                        usrAcc = this.LogonUser.Account;
                    else usrAcc = "Anonymous";
                }

                dic.Add("User", usrAcc);

                // erase user in cache
                try {
                    //20130307 horace Logout時,自AppCache("ONLINE_USER_LIST")->線上使用者list中移除目前使用者
                    if (CTBC.FrameWork.Platform.AppCache.InCache("ONLINE_USER_LIST")) {
                        Dictionary<string, User> ursLst2 = (Dictionary<string, User>)CTBC.FrameWork.Platform.AppCache.Get("ONLINE_USER_LIST");
                        if (!string.IsNullOrEmpty(this.LogonUser.Account))
                        {
                            if (ursLst2.ContainsKey(this.LogonUser.Account))
                            {
                                ursLst2.Remove(this.LogonUser.Account);
                                CTBC.FrameWork.Platform.AppCache.Update(ursLst2, "ONLINE_USER_LIST");
                            }
                        }
                    }         
                }
                catch { }

                // clear session cache
                this.Response.Cache.SetExpires(DateTime.UtcNow.AddMinutes(-1));
                this.Response.Cache.SetCacheability(HttpCacheability.NoCache);
                this.Response.Cache.SetNoStore();
                this.Response.Cookies.Clear();	// Remove((string)Session["UserAccount"]);
                try
                {
                    FormsAuthentication.SignOut();
                    dic.Add("Result", "S");
                }
                catch (Exception ex)
                {
                    dic.Add("Result", "F");
                    log.Message = ex.Message + ";" + ex.Source + ";" + ex.StackTrace;
                }

                dic.Add("ActionCode", "O");
                dic.Add("TranFlag", "BF");
                dic.Add("FunctionId", "Action.Home.Logout");
                dic.Add("SessionId", HttpContext.Session.SessionID);
                dic.Add("URL", HttpContext.Request.RawUrl);
                dic.Add("IP", HttpContext.Request.UserHostAddress);
                dic.Add("MachineName", HttpContext.Request.UserHostName);
                log.ExtendedProperties = dic;
                // end the session
                Session.Clear();
                Session.Abandon();

                //Save Logout Log
                Logger.Write(log);
                log.Categories.Remove("LogonLogout");

                // redirect
                filterContext.Result = this.Redirect("~/");
                return;
            }
            #endregion

            //-------------------------------------------------------------------
            //Does current user been authorized to visit this controller action?
            //判斷若Action=Child Action則不檢查權限
            //-------------------------------------------------------------------
			if (this.ControllerContext.IsChildAction || 
                (
                    this.ActionName != "Unauthorized" &&
				    this.ActionName != "Ajax_Error" &&
				    this.ActionName != "Ajax_Success" &&
				    this.LogonUser != null
                    )
                ) {
                    //如果本Action 不等於 ChildAction 則需再近一步檢查權限
                    //*ge.song
                    if (!this.ControllerContext.IsChildAction)
                    {
                        if ((this.ActionName == "Execute" &&
                             this.LogonUser.IsAuthorized(this.ActionNamePrefix + "." + this.ControllerName + "." + this.ActionName, "", "CUD*") == false) ||
                            (this.ActionName != "Execute" &&
                             this.LogonUser.IsAuthorized(this.ActionNamePrefix + "." + this.ControllerName + "." + this.ActionName, "", this.AuthZCode) == false))
                        {
                            filterContext.Result = this.HandleUnauthorized();
                            return;
                        }
                    }
			}
			else
				// invoke base
				base.OnActionExecuting(filterContext);
		}

		// after action executing
		protected override void OnActionExecuted(ActionExecutedContext ctx)
		{
			// write controller exit log
            //this.WriteExitLog();//2012-11-26

			// invoke base
			base.OnActionExecuted(ctx);
		}

        protected override IAsyncResult BeginExecuteCore(AsyncCallback callback, object state)
        {
            //---------------------------------------------
            //設定多國語系
            //20140808 horace
            //---------------------------------------------
            Culture Culture = new Culture();
            if (Session["CultureName"] != null)
                Culture.SetCulture((string)Session["CultureName"]);
            else
                 Culture.SetCulture("");
            return base.BeginExecuteCore(callback, state);
        }

		// Unauthorized handler
		public ActionResult HandleUnauthorized() {
            if (Request.IsAjaxRequest())
            {
                LogUnauthorized();
                HttpContext.Response.Clear();
                HttpContext.Response.StatusCode = 500;
                return Json(new { errId = UnauthErrId, errDesc = UnauthErrDesc });// errId請與DB中ErrorCode相對應
            }
            else
            {
                LogUnauthorized();
                ViewData["ErrId"] = UnauthErrId;
                ViewData["ErrDesc"] = this.ControllerName + "." + this.ActionName + UnauthErrDesc;
                ViewData["ErrTime"] = DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString();
                ViewData["ErrType"] = "unauthorized error";
                return this.RedirectToAction("Unauthorized", "CSFSError", new
                {
                    area = "",
                    k1 = this.ControllerName,
                    k2 = this.ActionName
                });
            }
		}

        public void LogUnauthorized()
        {
            //Get Log Info
            LogEntry log = new LogEntry();
            log.Message = "ErrId=" + UnauthErrId + ";" + "ErrDesc=" + this.ControllerName + "." + this.ActionName + UnauthErrDesc; ;
            log.Categories.Add("Exception");
            log.Priority = 1;
            log.EventId = 103;
            log.TimeStamp = DateTime.Now;
            log.Severity = System.Diagnostics.TraceEventType.Stop;
            log.Title = "Exception";
            Dictionary<string, object> dic = new Dictionary<string, object>();
            dic.Add("User", this.LogonUser.Account);
            dic.Add("Result", "S");
            dic.Add("ActionCode", "E");
            dic.Add("TranFlag", "BF");
            dic.Add("FunctionId", this.FunctionId);
            dic.Add("SessionId", HttpContext.Session.SessionID);
            dic.Add("URL", HttpContext.Request.RawUrl);
            dic.Add("IP", HttpContext.Request.UserHostAddress);
            dic.Add("MachineName", HttpContext.Request.UserHostName);
            log.ExtendedProperties = dic;
            Logger.Write(log);
            log.Categories.Remove("Exception");        
        }

        // Logout action
        public ActionResult Logout()
        {
            return View();
        }

		// get XML post data
		public void XML_2_Object(object O)
		{
			XmlNode Node = null;
			try { Node = this.dataDom.DocumentElement; } catch { }
			if (Node == null) return;
			foreach (XmlNode D in Node.ChildNodes)
				try { O.GetType().GetProperty(D.Name).SetValue(O, D.InnerText, null); }
				catch { }
		}

		// get XML post data
		public void Object_2_Object(object oFrom, object oTo)
		{
			foreach (PropertyInfo PI in oFrom.GetType().GetProperties())
				try { oTo.GetType().GetProperty(PI.Name).SetValue(oTo, oFrom.GetType().GetProperty(PI.Name).GetValue(oFrom, null), null); }
				catch { }
		}

		// serialize object to XML
		public string Object_2_XML(object o)
		{
			try { return XML.Object_2_XML(o); }	catch { throw; };
		}

	// end of AppCpntroller
	}

	
	public class AppEntityController<C, E> : AppController
		where E : EntityObject
		where C : ObjectContext
	{
		// properties
		public C OC = null;									// EDM object context
		public E EO = null;
		public string OrderBy = "";
        public AppActionQueryByPage<C, E> AQBP;
        public AppActionQuery<C, E> AQ;
        //public AppActionExecute<C, E> AE;

		// constructor
		public AppEntityController(C oc, E eo)
		{
			this.OC = oc;
			this.EO = eo;
            AQBP = new AppActionQueryByPage<C, E>(OC, EO, this);
            AQ = new AppActionQuery<C, E>(OC, EO, this);
            //AE = new AppActionExecute<C, E>(OC, EO, this, cmd);
		}

		// Action - Query by Page
		[HttpGet]
		[OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
		public virtual ViewResult QueryByPage(int page = 1)
		{  
			//AppActionQueryByPage<C, E> AQBP = new AppActionQueryByPage<C, E>(OC, EO, this);
			return View(AQBP.QueryByPage(this.OrderBy, page));
		}

		// Action - Query
		[HttpGet]
		[OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
		public ViewResult Query()
		{
			//AppActionQuery<C, E> AQ = new AppActionQuery<C, E>(OC, EO, this);
			return View(AQ.Query());
		}

		// Action - Execute (Create, Update, Delete)
		[HttpPost]
		[OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        
		public ActionResult Execute(E eo, string cmd = "?")
		{
            //this.AppValidator.Validate(eo, abc);

			AppActionExecute<C, E> AE = new AppActionExecute<C, E>(OC, EO, this, cmd);
            //AppValidator.Validate(eo, null);
			string sResult = AE.Execute(eo);
			if (sResult == "?UNAUTH?") return this.HandleUnauthorized();
			return Content(sResult);
		}

		// Dispose
		protected override void Dispose(bool disposing)
		{
			OC.Dispose();
			base.Dispose(disposing);
		}
	}

}
