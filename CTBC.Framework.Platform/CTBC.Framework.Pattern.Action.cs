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
using System.Data.Objects;
using System.Data.Objects.DataClasses;

namespace CTBC.FrameWork.Pattern
{
	public class AppAction
	{
		// properties
		public string Actions = "";							// authorized action codes to this controller
		public string AuthZCode = "*V";						// the action codes needed to access this controller
		public string FunctionId = "";						// associated function ID of this controller
		public string LocationId = "";						// current controller location ID
		public string FunctionDesc = "";
		public User LogonUser;

		public XmlDocument dataDom = new XmlDocument();
		public AppLog AppLog = new AppLog();
		public AppException AppException = new AppException();
		public AppValidator AppValidator = new AppValidator();
		public LogWriter LogWriter = null;

		public string ActionNamePrefix = "Action";
		public string ActionName = "";
		public string ControllerName = "";
		public AppController appController = null;

		// constructor
		public AppAction(AppController controller)
		{
			// init log writer
			this.LogWriter = this.AppLog.Writer;

			// determine action and controller name
			this.ActionName = controller.ActionName;
			this.appController = controller;
			this.ControllerName = controller.ControllerName;

			// get logon User role and authorizations
			if (this.LogonUser == null)
				try { this.LogonUser = this.appController.LogonUser; } catch { }

			// get authorized action codes for this object
			this.AuthZ();
		}

		// get authorization action code
		public string AuthZ()
		{
			this.FunctionId = this.ActionNamePrefix + "." + this.ControllerName + "." + this.ActionName;
			try { this.Actions = this.LogonUser.GetAuthZActions(this.FunctionId, ""); } catch { }
			return this.Actions;
		}

		public void WriteEntryLog()
		{
            //if (this.AppLog.CheckEntryLog(this.FunctionId, "") == false) return;
            this.AppLog.Categories.Add(AppLog.CUF_Log_FuncEntryCategory);
            this.AppLog.EventId = AppLog.CUF_Log_EntryEventID;
            this.AppLog.FunctionId = this.FunctionId + this.FunctionDesc;
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
            this.AppLog.FunctionId = this.FunctionId + this.FunctionDesc;
            this.AppLog.Message = "Function " + this.FunctionId + " Exit!";
            this.AppLog.Title = this.AppLog.Message;
            this.AppLog.Severity = System.Diagnostics.TraceEventType.Information;
            this.AppLog.Priority = 3;
            this.LogWriter.Write(this.AppLog);
		}

		public void WriteLocLog(string locationId)
		{
		}

		public void WriteLog()
		{
			this.LogWriter.Write(this.AppLog);
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
	}

	public class AppActionQuery<C, E> : AppAction
		where E : EntityObject
		where C : ObjectContext
	{
		// properties
		public AppEntity<C, E> AE;
		public AppController AC;

		// constructor
		public AppActionQuery(C oc, E eo, AppController controller) : base(controller)
		{
			AE = new AppEntity<C, E>(oc, eo);
			AC = controller;
		}

		// Query Action
		public Object Query()
		{
			this.FunctionDesc = "";
			this.WriteEntryLog();

			object oResult = AE.Query();
			this.WriteExitLog();
			return oResult;
		}
	}

	public class AppActionQueryByPage<C, E> : AppAction
		where E : EntityObject
		where C : ObjectContext
	{
		// properties
		public AppEntity<C, E> AE;
		public AppController AC;

		// constructor
		public AppActionQueryByPage(C oc, E eo, AppController controller) : base(controller)
		{
			AE = new AppEntity<C, E>(oc, eo);
			AC = controller;
		}

		// QueryByPage Action
		public Object QueryByPage(string orderBy, int page = 1)
		{
			this.FunctionDesc = "(OrderBy:" + orderBy + ", Page=" + page.ToString() + ")";
			this.WriteEntryLog();
			
			AE.OrderBy = orderBy;
			object oResult = AE.QueryByPage(AC.ViewBag, page);
			
			this.WriteExitLog();
			return oResult;
		}
	}

	public class AppActionExecute<C, E> : AppAction
		where E : EntityObject
		where C : ObjectContext
	{
		// properties
		public AppEntity<C, E> AE;
		public AppController AC;
		public string Command = "";

		// constructor
		public AppActionExecute(C oc, E eo, AppController controller, string cmd)
			: base(controller)
		{
			this.Command = cmd;
			AE = new AppEntity<C, E>(oc, eo);
			AC = controller;
		}

		// Execute Action
		public string Execute(E eo)
		{
			this.FunctionDesc = "(Cmd=" + this.Command + ")";
			this.WriteEntryLog();

			// check authorization
			string sResult;
			if (this.LogonUser.IsAuthorized(this.ActionNamePrefix + "." + this.ControllerName + ".Execute", "", this.Command + "*") == false)
				sResult = "?UNAUTH?";
			else
				sResult = AE.Execute(eo, this.AC.ModelState, this.Command);
			this.WriteExitLog();
			return sResult;
		}
	}
}
