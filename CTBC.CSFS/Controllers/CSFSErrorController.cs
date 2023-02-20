/// <summary>
/// 程式說明:CSFSError Controller - Error 記錄與顯示
/// 維護部門:資訊管理處
/// CSFS Version:v3.0
/// 中國信託銀行 版權所有  ©  All Rights Reserved. 
/// </summary>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CTBC.CSFS.Pattern;
using CTBC.CSFS.Resource;

namespace CTBC.CSFS.Controllers
{
    public class CSFSErrorController : AppController
    {
        string now = DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString();
        string errCode = "CSFS-SYS-9999";
        public ActionResult Index()
        {
            if (ViewData["ErrId"] == null) ViewData["ErrId"] = errCode;
            if (ViewData["ErrDesc"] == null) ViewData["ErrDesc"] = "";
            ViewData["ErrTime"] = now;
            ViewData["ErrType"] = "View or Other Error";
            return View();
        }

        public ActionResult NotFound()
        {
            ViewData["ErrId"] = "CSFS-SYS-0002";
            if (ViewData["ErrDesc"] == null) ViewData["ErrDesc"] = "";
            else ViewData["ErrDesc"] = "Page Not Found";
            ViewData["ErrTime"] = now;
            ViewData["ErrType"] = "Page Not Found";
            return View();
        }

        public ActionResult AJAXError(string errId,string errDesc)
        {
            if (errId == null) ViewData["ErrId"] = errCode;
            else ViewData["ErrId"] = errId;
            if (errDesc == null) ViewData["ErrDesc"] = "";
            else ViewData["ErrDesc"] = errDesc;
            ViewData["ErrTime"] = now;
            ViewData["ErrType"] = "SQL or AJAX Request Error";
            return View(); 
        }

        public ActionResult Unauthorized(string k1, string k2)
        {
            k1 = (string.IsNullOrEmpty(k1)) ? "" : k1;
            k2 = (string.IsNullOrEmpty(k2)) ? "" : k2;
            ViewData["ErrId"] = "CSFS-SYS-0003";
            ViewData["ErrDesc"] = Lang.csfs_err_noright_sys + " " + k1 + "." + k2;//k1=controllerName,k2=actionName
            ViewData["ErrTime"] = now;
            ViewData["ErrType"] = "Unauthorized Error";
            return View("Unauthorized");
        }

        public ActionResult GeneralError(HandleErrorInfo model)
        {
            ViewData["ErrId"] = errCode;
            ViewData["ErrDesc"] = "";//model.Exception.Message + " " + model.Exception.Source;
            ViewData["ErrTime"] = now;
            ViewData["ErrType"] = "Other Error";
            return View(model); 
        }

        public ActionResult OverLength()
        {
            if (ViewData["ErrId"] == null) ViewData["ErrId"] = "CSFS-SYS-9997";
            if (ViewData["ErrDesc"] == null) ViewData["ErrDesc"] = Lang.csfs_overlength;
            ViewData["ErrTime"] = now;
            ViewData["ErrType"] = "Request Over Length Error";
            //因Request超過系統限制,log需在此處理,方可取得Session相關資訊.
            Log(ViewData["ErrId"].ToString(), ViewData["ErrDesc"].ToString());
            return View();
        }

        private void Log(string errid,string errdesc)
        {
            ExceptionFilter exa = new ExceptionFilter();
            CSFSException csfsExp = new CSFSException
            {
                errId = errid,
                errDesc = errdesc,
                errDetail = errdesc,
                controller = "CSFSBulkImport",
                action = "Index",
                sverity = "Error"
            };
            exa.WriteExceptionLog(csfsExp);
        }
    }
}
