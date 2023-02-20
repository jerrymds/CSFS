using CTBC.CSFS.Pattern;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CTBC.CSFS.Models;
using System.IO;
using CTBC.CSFS.BussinessLogic;
using CTBC.CSFS.Filter;
using CTBC.FrameWork.Util;
using CTBC.CSFS.Resource;

namespace CTBC.CSFS.Areas.QueryAndExport.Controllers
{
    public class MeetingSummaryListController : AppController
    {
        // GET: QueryAndExport/MeetingSummaryList
        [RootPageFilter]
        public ActionResult Index(string date)
        {
            date = string.IsNullOrEmpty(date) ? DateTime.Now.ToString("yyyy/MM/dd") : UtlString.FormatDateTwStringToAd(date);
            MeetingSummaryList model = new MeetingSummaryList();
            model.GovDateS = UtlString.FormatDateTw(date);
            model.GovDateE = UtlString.FormatDateTw(date);
            return View(model);
        }
        public ActionResult Excel(string GovDateS, string GovDateE)
        {
            MemoryStream ms = new MemoryStream();
            MeetingSummaryListBIZ MSBIZ = new MeetingSummaryListBIZ();
            GovDateS = UtlString.FormatDateTwStringToAd(GovDateS);
            GovDateE = UtlString.FormatDateTwStringToAd(Convert.ToDateTime(UtlString.FormatDateString(GovDateE)).AddDays(1).ToString("yyyy/MM/dd"));
            string fileName = string.Empty;
            if (!string.IsNullOrEmpty(GovDateS) && !string.IsNullOrEmpty(GovDateE))
            {
                ms = MSBIZ.ExportExcel(GovDateS, GovDateE);
                fileName = Lang.csfs_meeting_summary_list+"_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xls";
            }
            if (ms != null && ms.Length > 0)
            {
                Response.ClearContent();
                Response.ClearHeaders();
            }
            else
            {
                ms = new MemoryStream();
            }
            return File(ms.ToArray(), "application/vnd.ms-excel", fileName);
        }
    }
}