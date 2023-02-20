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

namespace CTBC.FrameWork.Pattern
{
	public class AppModel
	{
		// properties
		public string Actions = "";							// authorized action codes to this controller
		public string AuthZCode = "*V";						// the action codes needed to access this controller
		public string FunctionId = "";						// associated function ID of this controller
		public string LocationId = "";						// current controller location ID
		public User LogonUser;

		public XmlDocument dataDom = new XmlDocument();
		public AppLog AppLog = new AppLog();
		public AppException AppException = new AppException();
		public AppValidator AppValidator = new AppValidator();
		public LogWriter LogWriter = null;

		public string PageNamePrefix = "Page";
		public string PageName = "";

		// constructor
		public AppModel()
		{
			// init log writer
			this.LogWriter = this.AppLog.Writer;

			// get authorized action codes for this object
			this.AuthZ();
		}

		// get authorization action code
		public string AuthZ()
		{
			this.FunctionId = this.PageNamePrefix + "." + this.PageName;
			try { this.Actions = this.LogonUser.GetAuthZActions(this.FunctionId, ""); } catch { }
			return this.Actions;
		}

		public void WriteEntryLog()
		{
		}

		public void WriteExitLog()
		{
		}

		public void WriteLocLog(string locationId)
		{
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
}
