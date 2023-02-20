using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using CTBC.CSFS.Models;
using CTBC.CSFS.BussinessLogic;
using CTBC.CSFS.Pattern;
using System.Web.Mvc;
using CTBC.CSFS.Filter;
using CTBC.FrameWork.Util;
using Microsoft.Reporting.WebForms;
using CTBC.CSFS.ViewModels;

namespace CTBC.CSFS.Areas.Collection.Controllers
{
    public class CollectionToAgentController : AppController
    {
        List<ReportDataSource> subDataSource = new List<ReportDataSource>();
        CaseQueryBIZ CQBIZ = new CaseQueryBIZ();
        PARMCodeBIZ parm = new PARMCodeBIZ();
        CollectionToAgentBIZ CTBZ;
        public CollectionToAgentController()
        {
            CTBZ = new CollectionToAgentBIZ(this);
        }

        [RootPageFilter]
        public ActionResult Index(string isBack)
        {
            CollectionToAgent model = new CollectionToAgent(); 
            if (isBack == "1")
            {
                //*執行cookie里的內容
                HttpCookie getQueryCookie = Request.Cookies.Get("QueryCookie");
                if (getQueryCookie != null)
                {
                    if (getQueryCookie.Values["CaseKind"] != null) ViewBag.CaseKindQuery = getQueryCookie.Values["CaseKind"];
                    if (getQueryCookie.Values["CaseKind2"] != null) ViewBag.CaseKind2Query = getQueryCookie.Values["CaseKind2"];
                    if (getQueryCookie.Values["CaseNo"] != null) model.CaseNo = getQueryCookie.Values["CaseNo"];
                    if (getQueryCookie.Values["GovDateS"] != null) model.GovDateS = getQueryCookie.Values["GovDateS"];
                    if (getQueryCookie.Values["GovDateE"] != null) model.GovDateE = getQueryCookie.Values["GovDateE"];
                    if (getQueryCookie.Values["Speed"] != null) model.Speed = getQueryCookie.Values["Speed"];
                    if (getQueryCookie.Values["ReceiveKind"] != null) model.ReceiveKind = getQueryCookie.Values["ReceiveKind"];
                    if (getQueryCookie.Values["GovNo"] != null) model.GovNo = getQueryCookie.Values["GovNo"];
                    if (getQueryCookie.Values["CreatedDateS"] != null) model.CreatedDateS = getQueryCookie.Values["CreatedDateS"];
                    if (getQueryCookie.Values["CreatedDateE"] != null) model.CreatedDateE = getQueryCookie.Values["CreatedDateE"];
                    if (getQueryCookie.Values["Unit"] != null) model.Unit = getQueryCookie.Values["Unit"];
                    if (getQueryCookie.Values["GovUnit"] != null) model.GovUnit = getQueryCookie.Values["GovUnit"];
                    if (getQueryCookie.Values["CreatedUser"] != null) model.CreatedUser = getQueryCookie.Values["CreatedUser"];
                    //20170421 固定 RQ-2015-019666-017 派件至跨單位 宏祥 update start
                    //if (getQueryCookie.Values["AgentUser"] != null) model.AgentUser = getQueryCookie.Values["AgentUser"];
                    if (getQueryCookie.Values["AgentDepartment"] != null) model.AgentDepartment = getQueryCookie.Values["AgentDepartment"];
                    if (getQueryCookie.Values["AgentDepartment2"] != null) model.AgentDepartment2 = getQueryCookie.Values["AgentDepartment2"];
                    if (getQueryCookie.Values["AgentDepartmentUser"] != null) model.AgentDepartmentUser = getQueryCookie.Values["AgentDepartmentUser"];
                    //20170421 固定 RQ-2015-019666-017 派件至跨單位 宏祥 update end
                    ViewBag.CurrentPage = getQueryCookie.Values["CurrentPage"];
                    ViewBag.isQuery = "1";
                }
            }

            InitDropdownListOptions();
            return View(model);
        }

        public ActionResult _QueryResult(CollectionToAgent model, int pageNum = 1, string strSortExpression = "CaseId", string strSortDirection = "asc")
        {

            string UserId = LogonUser.Account;
            if (!string.IsNullOrEmpty(model.GovDateE))
            {
                model.GovDateE = UtlString.FormatDateTwStringToAd(model.GovDateE);
            }
            if (!string.IsNullOrEmpty(model.GovDateS))
            {
                model.GovDateS = UtlString.FormatDateTwStringToAd(model.GovDateS);
            }
            if (!string.IsNullOrEmpty(model.CreatedDateE))
            {
                model.CreatedDateE = UtlString.FormatDateTwStringToAd(model.CreatedDateE);
            }
            if (!string.IsNullOrEmpty(model.CreatedDateS))
            {
                model.CreatedDateS = UtlString.FormatDateTwStringToAd(model.CreatedDateS);
            }
            IList<CollectionToAgent> list = CTBZ.GetData(model, pageNum, strSortExpression, strSortDirection, UserId);
            var dtvm = new CollectionToAgentViewModel()
            {
                CollectionToAgent = model,
                CollectionToAgentList = list,
            };
            //分頁相關設定
            dtvm.CollectionToAgent.PageSize = CTBZ.PageSize;
            dtvm.CollectionToAgent.CurrentPage = CTBZ.PageIndex;
            dtvm.CollectionToAgent.TotalItemCount = CTBZ.DataRecords;
            dtvm.CollectionToAgent.SortExpression = strSortExpression;
            dtvm.CollectionToAgent.SortDirection = strSortDirection;

            dtvm.CollectionToAgent.CaseKind = model.CaseKind;
            dtvm.CollectionToAgent.CaseKind2 = model.CaseKind2;
            dtvm.CollectionToAgent.GovKind = model.GovKind;
            dtvm.CollectionToAgent.GovUnit = model.GovUnit;
            dtvm.CollectionToAgent.CaseNo = model.CaseNo;
            dtvm.CollectionToAgent.GovDateS = model.GovDateS;
            dtvm.CollectionToAgent.GovDateE = model.GovDateE;
            dtvm.CollectionToAgent.Speed = model.Speed;
            dtvm.CollectionToAgent.ReceiveKind = model.ReceiveKind;
            dtvm.CollectionToAgent.GovNo = model.GovNo;
            dtvm.CollectionToAgent.CreatedDateS = model.CreatedDateS;
            dtvm.CollectionToAgent.CreatedDateE = model.CreatedDateE;
            dtvm.CollectionToAgent.Unit = model.Unit;
            //20170421 固定 RQ-2015-019666-017 派件至跨單位 宏祥 update start
            //dtvm.CollectionToAgent.AgentUser = model.AgentUser;
            dtvm.CollectionToAgent.AgentDepartment = model.AgentDepartment;
            dtvm.CollectionToAgent.AgentDepartment2 = model.AgentDepartment2;
            dtvm.CollectionToAgent.AgentDepartmentUser = model.AgentDepartmentUser;
            //20170421 固定 RQ-2015-019666-017 派件至跨單位 宏祥 update end


            HttpCookie AToHCookie = new HttpCookie("QueryCookie");
            AToHCookie.Values.Add("CaseKind", model.CaseKind);
            AToHCookie.Values.Add("CaseKind2", model.CaseKind2);
            AToHCookie.Values.Add("GovUnit", model.GovUnit);
            AToHCookie.Values.Add("CaseNo", model.CaseNo);
            AToHCookie.Values.Add("GovDateS", model.GovDateS);
            AToHCookie.Values.Add("GovDateE", model.GovDateE);
            AToHCookie.Values.Add("Speed", model.Speed);
            AToHCookie.Values.Add("ReceiveKind", model.ReceiveKind);
            AToHCookie.Values.Add("GovNo", model.GovNo);
            AToHCookie.Values.Add("CreatedDateS", model.CreatedDateS);
            AToHCookie.Values.Add("CreatedDateE", model.CreatedDateE);
            AToHCookie.Values.Add("Unit", model.Unit);
            AToHCookie.Values.Add("CreatedUser", model.CreatedUser);
            //20170421 固定 RQ-2015-019666-017 派件至跨單位 宏祥 update start
            //AToHCookie.Values.Add("AgentUser", model.AgentUser);
            AToHCookie.Values.Add("AgentDepartment", model.AgentDepartment);
            AToHCookie.Values.Add("AgentDepartment2", model.AgentDepartment2);
            AToHCookie.Values.Add("AgentDepartmentUser", model.AgentDepartmentUser);
            //20170421 固定 RQ-2015-019666-017 派件至跨單位 宏祥 update start
            AToHCookie.Values.Add("CurrentPage", pageNum.ToString());
            Response.Cookies.Add(AToHCookie);
            return PartialView("_QueryResult", dtvm);
        }
        
        /// <summary>
        /// 送經辦
        /// </summary>
        /// <param name="caseIdList"></param>
        /// <returns></returns>
        public ActionResult SendAgent(string caseIdList)
        {
            string[] aryId = caseIdList.Split(',');
            List<Guid> aryCaseId = (from id in aryId where !string.IsNullOrEmpty(id) select new Guid(id)).ToList();
            string userId = LogonUser.Account;
            return Json(CTBZ.SendAgents(aryCaseId, userId));
        }
        //呈核
        public ActionResult ChenHe(string CaseIdarr)
        {
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
                rtn =  rtn & CTBZ.AgentSubmit(list, aryAgentId, LogonUser.Account);
            }
            return Json(rtn ? new JsonReturn { ReturnCode = "1" } : new JsonReturn { ReturnCode = "0" });
        }
        public ActionResult OverDue(AgentToHandle model, string strIds)
        {
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
                rtn =  rtn & CTBZ.OverDue(model, list, aryAgentId, LogonUser.Account);
            }
            return Json(rtn ? new JsonReturn { ReturnCode = "1" } : new JsonReturn { ReturnCode = "0" });
        }
        public void InitDropdownListOptions()
        {
            LdapEmployeeBiz empBiz = new LdapEmployeeBiz();
            string AgentsList = JsonHelper.ObjectToJson(empBiz.GetAgentAndBu(""));
            ViewBag.AgentsList = AgentsList;
            ViewBag.CaseKindList = new SelectList(parm.GetCodeData("CASE_KIND"), "CodeDesc", "CodeDesc");
            ViewBag.StatusName = new SelectList(CQBIZ.GetStatusName("STATUS_NAME"), "CodeNo", "CodeDesc");
            ViewBag.CASE_END_TIME = parm.GetCASE_END_TIME("CASE_END_TIME");                                     //* 每天最晚時間
            ViewBag.GOV_KINDList = new SelectList(parm.GetCodeData("GOV_KIND"), "CodeDesc", "CodeDesc");        //* 來文機關類型
            ViewBag.SpeedList = new SelectList(parm.GetCodeData("INCOME_SPEED"), "CodeDesc", "CodeDesc");       //* 速別
            ViewBag.ReceiveKindList = new SelectList(parm.GetCodeData("INCOME_TYPE"), "CodeDesc", "CodeDesc");  //* 來文方式
            ViewBag.CaseKindList = new SelectList(parm.GetCodeData("CASE_KIND"), "CodeDesc", "CodeDesc");
            ViewBag.CaseKind2List = new SelectList(empBiz.GetCodeData("CASE_SEIZURE"), "CodeDesc", "CodeDesc");
            //20170421 固定 RQ-2015-019666-017 派件至跨單位 宏祥 update start
            //ViewBag.AgentUserList = new SelectList(new LdapEmployeeBiz().GetAllEmployeeInEmployeeView(), "EmpID", "EmpIdAndName");
            ViewBag.AgentDepartmentList = new SelectList(parm.GetCodeData("CollectionToAgent_AgentDDLDepartment"), "CodeNo", "CodeDesc");
            ViewBag.AgentDepartment2List = new SelectList(new AgentSettingBIZ().GetAgentDepartment2View(), "DepartmentID", "SectionName");
            ViewBag.AgentDepartmentUserList = new SelectList(new AgentSettingBIZ().GetAgentDepartmentUserView(), "EmpID", "EmpIdAndName");
            //20170421 固定 RQ-2015-019666-017 派件至跨單位 宏祥 update end
            PARMCodeBIZ para = new PARMCodeBIZ();
            ViewBag.AddDay = (Convert.ToInt32(para.GetCASE_END_TIME("OVERDUE_DAYS"))) * 1;

        }

        //20170421 固定 RQ-2015-019666-017 派件至跨單位 宏祥 add start
        /// <summary>
        /// 選擇經辦人員下拉選單(處)
        /// </summary>
        /// <param name="AgentDepartment"></param>
        /// <returns></returns>
        public JsonResult ChangAgentDepartment1(string AgentDepartment)
        {
           List<KeyValuePair<string, string>> items = new List<KeyValuePair<string, string>>();
           if (string.IsNullOrEmpty(AgentDepartment)) return Json(items);
           var list = CTBZ.GetAgentDepartment2View(AgentDepartment);
           if (list.Any())
           {
              items.AddRange(list.Select(AgentDepartment2 => new KeyValuePair<string, string>(AgentDepartment2.DepartmentID, AgentDepartment2.SectionName)));
           }
           return Json(items);
        }
        /// <summary>
        /// 選擇經辦人員下拉選單(科組)
        /// </summary>
        /// <param name="AgentDepartment"></param>
        /// <returns></returns>
        public JsonResult ChangAgentDepartment2(string AgentDepartment)
        {
           List<KeyValuePair<string, string>> items = new List<KeyValuePair<string, string>>();
           if (string.IsNullOrEmpty(AgentDepartment)) return Json(items);
           var list = CTBZ.GetAgentDepartmentUserView(AgentDepartment);
           if (list.Any())
           {
              items.AddRange(list.Select(AgentDepartmentUser => new KeyValuePair<string, string>(AgentDepartmentUser.EmpId, AgentDepartmentUser.EmpIdAndName)));
           }
           return Json(items);
        }
        //20170421 固定 RQ-2015-019666-017 派件至跨單位 宏祥 add start
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
    }
}