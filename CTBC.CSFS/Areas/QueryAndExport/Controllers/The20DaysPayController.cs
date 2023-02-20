using System;
using System.Collections.Generic;
using System.EnterpriseServices;
using System.IO;
using System.Data;
using System.Web;
using System.Web.Mvc;
using System.Linq;
using CTBC.CSFS.BussinessLogic;
using CTBC.CSFS.Filter;
using CTBC.CSFS.Models;
using CTBC.CSFS.Pattern;
using CTBC.CSFS.Resource;
using CTBC.CSFS.ViewModels;
using CTBC.FrameWork.Util;
using Microsoft.Reporting.WebForms;

namespace CTBC.CSFS.Areas.QueryAndExport.Controllers
{
    public class The20DaysPayController : AppController
    {
        List<ReportDataSource> subDataSource = new List<ReportDataSource>();
        CaseQueryBIZ caseQueryBiz = new CaseQueryBIZ();
        static string where = "";
        // GET: QueryAndExport/The20DaysPay
        [RootPageFilter]
        public ActionResult Index(string isBack)
        {
            CaseQuery model = new CaseQuery();
            if (isBack == "1")
            {
                //*執行cookie里的內容
                HttpCookie getQueryCookie = Request.Cookies.Get("QueryCookie");
                if (getQueryCookie != null)
                {
                    if (getQueryCookie.Values["Days"] != null) model.Date = getQueryCookie.Values["Days"];
                    ViewBag.CurrentPage = getQueryCookie.Values["CurrentPage"];
                    ViewBag.isQuery = "1";
                }
            }
            else
            {
                model.Date = UtlString.FormatDateTw(UtlString.GetWednesday().ToString("yyyy/MM/dd"));
            }

            PARMCodeBIZ para = new PARMCodeBIZ();
            ViewBag.AddDay = (Convert.ToInt32(para.GetCASE_END_TIME("OVERDUE_DAYS"))) * 1;
            ViewBag.Date = UtlString.GetWednesday().ToString("yyyy/MM/dd");

            return View(model);
        }
        public void InitDropdownListOptions()
        {
            ViewBag.StatusName = new SelectList(caseQueryBiz.GetStatusName("STATUS_NAME"), "CodeNo", "CodeDesc");
            ViewBag.CaseKindList = new SelectList(caseQueryBiz.GetCodeData("CASE_KIND"), "CodeDesc", "CodeDesc");
            ViewBag.GOV_KINDList = new SelectList(caseQueryBiz.GetCodeData("GOV_KIND"), "CodeDesc", "CodeDesc");        //* 來文機關類型
            ViewBag.SpeedList = new SelectList(caseQueryBiz.GetCodeData("INCOME_SPEED"), "CodeDesc", "CodeDesc");       //* 速別
            ViewBag.ReceiveKindList = new SelectList(caseQueryBiz.GetCodeData("INCOME_TYPE"), "CodeDesc", "CodeDesc");  //* 來文方式
            ViewBag.CaseKindList = new SelectList(caseQueryBiz.GetCodeData("CASE_KIND"), "CodeDesc", "CodeDesc");
            ViewBag.CaseKind2List = new SelectList(caseQueryBiz.GetCodeData("CASE_SEIZURE"), "CodeDesc", "CodeDesc");
        }
        public ActionResult _QueryResult(CaseQuery model, int pageNum = 1, string strSortExpression = "CaseNo", string strSortDirection = "DESC")
        {
            string UserId = LogonUser.Account;
            if (!string.IsNullOrEmpty(model.Date))
            {
                model.Date = UtlString.FormatDateTwStringToAd(model.Date);
            }
            IList<CaseQuery> list = caseQueryBiz.GetData20Days(model, pageNum, strSortExpression, strSortDirection, UserId, ref where);
            var dtvm = new CaseQueryViewModel()
            {
                CaseQuery = model,
                CaseQueryList = list,
            };


            HttpCookie AToHCookie = new HttpCookie("QueryCookie");
            AToHCookie.Values.Add("Days", model.Date);
            AToHCookie.Values.Add("CurrentPage", pageNum.ToString());
            Response.Cookies.Add(AToHCookie);

            //分頁相關設定
            dtvm.CaseQuery.PageSize = caseQueryBiz.PageSize;
            dtvm.CaseQuery.CurrentPage = caseQueryBiz.PageIndex;
            dtvm.CaseQuery.TotalItemCount = caseQueryBiz.DataRecords;
            dtvm.CaseQuery.SortExpression = strSortExpression;
            dtvm.CaseQuery.SortDirection = strSortDirection;

            return PartialView("_QueryResult", dtvm);
        }

        //呈核
        public ActionResult ChengHe(string CaseIdarr)
        {
            CollectionToAgentBIZ CTBZ = new CollectionToAgentBIZ();
            bool rtn = true;
            string[] caseid = CaseIdarr.Split(',');
            List<Guid> aryCaseId = (from id in caseid where !string.IsNullOrEmpty(id) select new Guid(id)).ToList();
            foreach (Guid caseId in aryCaseId)
            {
                string userId = CTBZ.GetAgent(caseId) ?? "";
                string agentIdList = CTBZ.GetManagerID(userId);
                List<string> aryAgentId = (from id in agentIdList.Split(',') where !string.IsNullOrEmpty(id) select id).ToList();
                List<Guid> list = new List<Guid>();
                list.Add(caseId);
                rtn = rtn & CTBZ.AgentSubmit(list, aryAgentId, LogonUser.Account);
            }
            return Json(rtn ? new JsonReturn { ReturnCode = "1" } : new JsonReturn { ReturnCode = "0" });
        }

        //逾期呈核
        public ActionResult OverDueForChengHe(AgentToHandle model, string strIds)
        {
            CollectionToAgentBIZ CTBZ = new CollectionToAgentBIZ();
            bool rtn = true;
            string[] caseid = strIds.Split(',');
            List<Guid> aryCaseId = (from id in caseid where !string.IsNullOrEmpty(id) select new Guid(id)).ToList();
            foreach (Guid caseId in aryCaseId)
            {
                string userId = CTBZ.GetAgent(caseId) ?? "";
                string agentIdList = CTBZ.GetManagerID(userId);
                List<string> aryAgentId = (from id in agentIdList.Split(',') where !string.IsNullOrEmpty(id) select id).ToList();
                List<Guid> list = new List<Guid>();
                list.Add(caseId);
                rtn = rtn & CTBZ.OverDue(model, list, aryAgentId, LogonUser.Account);
            }
            return Json(rtn ? new JsonReturn { ReturnCode = "1" } : new JsonReturn { ReturnCode = "0" });
        }

        //public ActionResult Report(string caseIdList)
        //{
        //    CaseMasterBIZ masterBiz = new CaseMasterBIZ();
        //    List<ReportParameter> listParm = new List<ReportParameter>();

        //    //* CTBC的地址.電話.傳真
        //    PARMCode codeItem = masterBiz.GetCodeData("REPORT_SETTING", "Address").FirstOrDefault();
        //    listParm.Add(new ReportParameter("CtbcAddr", codeItem == null ? "" : codeItem.CodeDesc));
        //    codeItem = masterBiz.GetCodeData("REPORT_SETTING", "Tel").FirstOrDefault();
        //    listParm.Add(new ReportParameter("CtbcTel", codeItem == null ? "" : codeItem.CodeDesc));
        //    string ctbcTel = codeItem == null ? "" : codeItem.CodeDesc;
        //    codeItem = masterBiz.GetCodeData("REPORT_SETTING", "Fax").FirstOrDefault();
        //    listParm.Add(new ReportParameter("CtbcFax", codeItem == null ? "" : codeItem.CodeDesc));
        //    codeItem = masterBiz.GetCodeData("REPORT_SETTING", "ButtomLine").FirstOrDefault();
        //    listParm.Add(new ReportParameter("CtbcButtomLine", codeItem == null ? "" : codeItem.CodeDesc));
        //    codeItem = masterBiz.GetCodeData("REPORT_SETTING", "ButtomLine2").FirstOrDefault();
        //    listParm.Add(new ReportParameter("CtbcButtomLine2", codeItem == null ? "" : codeItem.CodeDesc));

        //    List<string> aryCaseIdList = caseIdList.Split(',').ToList();
        //    //* master
        //    DataTable dtMaster = masterBiz.GetCaseMasterByCaseIdList(aryCaseIdList);
        //    //* 發文設定
        //    DataTable dtSendSetting = new CaseSendSettingBIZ().GetSendSettingByCaseIdList(aryCaseIdList);
        //    //* 外來文帳務明細
        //    DataTable dtExternal = new CaseAccountBiz().GetCaseAccountExternalByCaseIdList(aryCaseIdList);
        //    //* 發票
        //    DataTable dtReceipt = new CaseAccountBiz().GetCaseReceiptByCaseIdList(aryCaseIdList);

        //    LocalReport localReport = new LocalReport { ReportPath = Server.MapPath("~/Reports/Rdlc/CaseMaster.rdlc") };
        //    localReport.SetParameters(listParm); //*添加參數
        //    localReport.DataSources.Add(new ReportDataSource("DataSet1", dtMaster));   //* 添加數據源,可以多個
        //    localReport.DataSources.Add(new ReportDataSource("SendSetting", dtSendSetting));   //* 添加數據源,可以多個
        //    localReport.SubreportProcessing += SubreportProcessingEventHandler;

        //    subDataSource.Add(new ReportDataSource("SendSetting", dtSendSetting));
        //    subDataSource.Add(new ReportDataSource("CaseAccountExternal", dtExternal));
        //    subDataSource.Add(new ReportDataSource("CaseReceipt", dtReceipt));


        //    Warning[] warnings;
        //    string[] streams;
        //    string mimeType;
        //    string encoding;
        //    string fileNameExtension;

        //    var renderedBytes = localReport.Render("PDF",
        //        null,
        //        out  mimeType,
        //        out encoding,
        //        out fileNameExtension,
        //        out streams,
        //        out warnings);

        //    Response.ClearContent();
        //    Response.ClearHeaders();
        //    return File(renderedBytes, mimeType, "Report.pdf");
        //}
        //void SubreportProcessingEventHandler(object sender, SubreportProcessingEventArgs e)
        //{
        //    foreach (var reportDataSource in subDataSource)
        //    {
        //        e.DataSources.Add(reportDataSource);
        //    }
        //}

        //public ActionResult BuildSimpleExcel(CaseQuery model)
        //{
        //    MemoryStream ms = new MemoryStream();
        //    string fileName = string.Empty;
        //    if (model.hiddenVal == "0")
        //    {
        //        ms = caseQueryBiz.Excel(where);
        //        fileName = Lang.csfs_menu_tit_casequery + "_" + Lang.csfs_debtexcel + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xls";
        //    }
        //    else
        //    {
        //        ms = caseQueryBiz.Exportlist(model.CaseIdarr,model.GovDateS,model.GovDateE);
        //        fileName = Lang.csfs_menu_tit_casequery + "_" + Lang.csfs_export_filelist + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xls";
        //    }

        //    if (ms != null && ms.Length > 0)
        //    {
        //        Response.ClearContent();
        //        Response.ClearHeaders();
        //    }
        //    else
        //    {
        //        ms = new MemoryStream();
        //    }
        //    return File(ms.ToArray(), "application/vnd.ms-excel", fileName);
        //}
    }
}