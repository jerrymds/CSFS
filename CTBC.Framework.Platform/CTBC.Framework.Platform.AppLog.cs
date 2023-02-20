using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.EnterpriseLibrary.Logging;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;
using System.Xml;

namespace CTBC.FrameWork.Platform
{
	public class AppLog : LogEntry
	{
        // properties
        public string FunctionId = "";
        public string LocationId = "";
        public LogWriter Writer = null;

        //20131022 horace mark
        //public string CUF_LoggingTreePath = "";
        public int CUF_LogLevel = 3;
        public string CUF_Log_FuncEntryCategory = "FuncEntry";
        public string CUF_Log_FuncExitCategory = "FuncExit";
        public string CUF_Log_FuncLocCategory = "FuncLoc";
        public int CUF_Log_EntryEventID = 101;
        public int CUF_Log_ExitEventID = 102;
        public int CUF_Log_LocEventID = 103;
        public XmlDocument LogTreeDom = new XmlDocument();

        //---------------------------------------
        //			Public Method
        //---------------------------------------

        // constructor
        public AppLog()
        {
            // set default severity
            this.Severity = System.Diagnostics.TraceEventType.Information;

            // create log writer
            if (Writer == null) Writer =  EnterpriseLibraryContainer.Current.GetInstance<LogWriter>();

            // get applications settings
            //CUF_LoggingTreePath = (System.Web.Configuration.WebConfigurationManager.AppSettings["CUF_LoggingTreePath"] == null ?
            //    "" : System.Web.Configuration.WebConfigurationManager.AppSettings["CUF_LoggingTreePath"]);
            try
            {
                CUF_LogLevel = (System.Web.Configuration.WebConfigurationManager.AppSettings["CUF_LogLevel"] == null ?
                    3 : Convert.ToInt32(System.Web.Configuration.WebConfigurationManager.AppSettings["CUF_LogLevel"]));
            }
            catch { }

            // load log tree
            //this.LogTreeDom = XML.Create_Load(CUF_LoggingTreePath);

            // read more log settings
            CUF_Log_FuncEntryCategory = (System.Web.Configuration.WebConfigurationManager.AppSettings["CUF_Log_FuncEntryCategory"] == null ?
                "FuncEntry" : System.Web.Configuration.WebConfigurationManager.AppSettings["CUF_Log_FuncEntryCategory"]);
            CUF_Log_FuncExitCategory = (System.Web.Configuration.WebConfigurationManager.AppSettings["CUF_Log_FuncExitCategory"] == null ?
                "FuncExit" : System.Web.Configuration.WebConfigurationManager.AppSettings["CUF_Log_FuncExitCategory"]);
            CUF_Log_FuncLocCategory = (System.Web.Configuration.WebConfigurationManager.AppSettings["CUF_Log_FuncLocCategory"] == null ?
                "FuncLoc" : System.Web.Configuration.WebConfigurationManager.AppSettings["CUF_Log_FuncLocCategory"]);

            try
            {
                CUF_Log_EntryEventID = (System.Web.Configuration.WebConfigurationManager.AppSettings["CUF_Log_EntryEventID"] == null ?
                    101 : Convert.ToInt32(System.Web.Configuration.WebConfigurationManager.AppSettings["CUF_Log_EntryEventID"]));
            }
            catch { }
            try
            {
                CUF_Log_ExitEventID = (System.Web.Configuration.WebConfigurationManager.AppSettings["CUF_Log_ExitEventID"] == null ?
                    102 : Convert.ToInt32(System.Web.Configuration.WebConfigurationManager.AppSettings["CUF_Log_ExitEventID"]));
            }
            catch { }
            try
            {
                CUF_Log_LocEventID = (System.Web.Configuration.WebConfigurationManager.AppSettings["CUF_Log_LocEventID"] == null ?
                    103 : Convert.ToInt32(System.Web.Configuration.WebConfigurationManager.AppSettings["CUF_Log_LocEventID"]));
            }
            catch { }

        }

        // check entry log//20131022 horace mark
        //public bool CheckEntryLog(string functionID, string itemID)
        //{
        //    try
        //    {
        //        XmlNode lo_Node = LogTreeDom.DocumentElement.SelectSingleNode("//*[@md_FuncID='" + functionID + "']");
        //        if (lo_Node == null) return false;
        //        return Convert.ToBoolean(XML.GetAttribute(lo_Node, "md_LogEntry"));
        //    }
        //    catch { return false; }
        //}

        // check exit log//20131022 horace mark
        //public bool CheckExitLog(string functionID, string itemID)
        //{
        //    try
        //    {
        //        XmlNode lo_Node = LogTreeDom.DocumentElement.SelectSingleNode("//*[@md_FuncID='" + functionID + "']");
        //        if (lo_Node == null) return false;
        //        return Convert.ToBoolean(XML.GetAttribute(lo_Node, "md_LogExit"));
        //    }
        //    catch { return false; }
        //}

        // check location log
        public bool CheckLocLog(string functionID, string itemID)
        {
            try
            {
                XmlNode lo_Node = LogTreeDom.DocumentElement.SelectSingleNode("//*[@md_FuncID='" + functionID + "']");
                if (lo_Node == null) return false;
                string ls_XML = XML.GetAttribute(lo_Node, "md_LogLocation");
                XmlDocument lo_DOM = XML.Create();
                try { lo_DOM.LoadXml(Utility.Escape(ls_XML)); }
                catch (Exception) { return false; }

                lo_Node = lo_DOM.DocumentElement.SelectSingleNode("//*[LocID='" + functionID + "']");
                if (lo_Node == null) return false;
                return Convert.ToBoolean(XML.GetAttribute(lo_Node, "md_LogEnabled"));
            }
            catch { return false; }
        }
	}
}
