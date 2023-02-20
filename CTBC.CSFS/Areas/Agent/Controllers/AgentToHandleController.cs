using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CTBC.CSFS.BussinessLogic;
using CTBC.CSFS.Filter;
using CTBC.CSFS.Models;
using CTBC.CSFS.Pattern;
using CTBC.CSFS.Resource;
using CTBC.CSFS.ViewModels;
using CTBC.FrameWork.Util;
using Microsoft.Reporting.WebForms;
using CTBC.FrameWork.Platform; //20170421 固定 RQ-2015-019666-017 派件至跨單位 宏祥 add

namespace CTBC.CSFS.Areas.Agent.Controllers
{
    public class AgentToHandleController : AppController
    {
        List<ReportDataSource> subDataSource = new List<ReportDataSource>();
        LdapEmployeeBiz empBiz = new LdapEmployeeBiz();//GetAgentAndBu
        AgentToHandleBIZ AToHBIZ;
       

        public AgentToHandleController()
        {
            AToHBIZ = new AgentToHandleBIZ(this);
        }
        /// <summary>
        /// 查詢
        /// </summary>
        /// <returns></returns>
        [RootPageFilter]
        public ActionResult Index(string isBack)
        {
            AgentToHandle model = new AgentToHandle();           
            if (isBack == "1")
            {
                //*執行cookie里的內容
                HttpCookie getQueryCookie = Request.Cookies.Get("QueryCookie");
                if (getQueryCookie != null)
                {
                    if (getQueryCookie.Values["GovUnit"] != null) model.GovUnit = getQueryCookie.Values["GovUnit"];
                    if (getQueryCookie.Values["GovNo"] != null) model.GovNo = getQueryCookie.Values["GovNo"];
                    if (getQueryCookie.Values["Person"] != null) model.Person = getQueryCookie.Values["Person"];
                    if (getQueryCookie.Values["Speed"] != null) model.Speed = getQueryCookie.Values["Speed"];
                    if (getQueryCookie.Values["ReceiveKind"] != null) model.ReceiveKind = getQueryCookie.Values["ReceiveKind"];
                    if (getQueryCookie.Values["CaseKind"] != null) ViewBag.CaseKindQuery = getQueryCookie.Values["CaseKind"];
                    if (getQueryCookie.Values["CaseKind2"] != null) ViewBag.CaseKind2Query = getQueryCookie.Values["CaseKind2"];
                    if (getQueryCookie.Values["GovDateS"] != null) model.GovDateS = getQueryCookie.Values["GovDateS"];
                    if (getQueryCookie.Values["GovDateE"] != null) model.GovDateE = getQueryCookie.Values["GovDateE"];
                    if (getQueryCookie.Values["Unit"] != null) model.Unit = getQueryCookie.Values["Unit"];
                    if (getQueryCookie.Values["CreatedDateS"] != null) model.CreatedDateS = getQueryCookie.Values["CreatedDateS"];
                    if (getQueryCookie.Values["CreatedDateE"] != null) model.CreatedDateE = getQueryCookie.Values["CreatedDateE"];
                    if (getQueryCookie.Values["CaseNo"] != null) model.CaseNo = getQueryCookie.Values["CaseNo"];
                    ViewBag.CurrentPage = getQueryCookie.Values["CurrentPage"];
                    ViewBag.isQuery = "1";
                }
            }
            Bind();
          
            return View(model);
        }
        /// <summary>
        /// 初始化查詢
        /// </summary>
        public void Bind()
        {          
            AgentSettingBIZ ASBIZ = new AgentSettingBIZ();
            string strAgentList = JsonHelper.ObjectToJson(empBiz.GetAgentAndBuInfosByBuId(""));
            ViewBag.AgentList = strAgentList;

            //20170421 固定 RQ-2015-019666-017 派件至跨單位 宏祥 update start
            //ViewBag.Department = new SelectList(ASBIZ.GetKeBie(), "Department", "Department");
            //Dictionary<string, User> userLst = new Dictionary<string, User>();
            //userLst = (Dictionary<string, User>)AppCache.Get("ONLINE_USER_LIST");
            string struserId = LogonUser.Account;           
            //string strBranchName = userLst[struserId].BranchName;
            string strBranchName = LogonUser.BranchName;
            string strSectionName = "";
            int intFlag = 0;
            if (strBranchName == "")
            {
               strSectionName = ASBIZ.GetSectionName(struserId);
               intFlag = ASBIZ.GetAgentToHandleDepartmentDDL(strSectionName);
            }            
            ViewBag.Department = new SelectList(ASBIZ.GetAgentSettingDepartment(strSectionName, strBranchName, intFlag), "Department", "Department");

            //判斷分行經辦角色隱藏收發代辦及取消再次發文功能
            int intRolesCount = LogonUser.Roles.Count;
            string strRoleLDAPId = "";
            for (int i = 0; i < intRolesCount; i++)
            {
               if (LogonUser.Roles[i].RoleLDAPId != "")
               {
                  strRoleLDAPId = LogonUser.Roles[i].RoleLDAPId;
               }
            }
            ViewBag.IsBranchAgent = "0";
            if (strRoleLDAPId == "CSFS011")
            {
               ViewBag.IsBranchAgent = "1";
            }
            //20170421 固定 RQ-2015-019666-017 派件至跨單位 宏祥 update end

            ViewBag.SpeedList = new SelectList(empBiz.GetCodeData("INCOME_SPEED"), "CodeDesc", "CodeDesc");
            ViewBag.ReceiveKindList = new SelectList(empBiz.GetCodeData("INCOME_TYPE"), "CodeDesc", "CodeDesc");
            ViewBag.CaseKindList = new SelectList(empBiz.GetCodeData("CASE_KIND"), "CodeDesc", "CodeDesc");
            ViewBag.CaseKind2List = new SelectList(empBiz.GetCodeData("CASE_SEIZURE"), "CodeDesc", "CodeDesc");
            ViewBag.GOV_KINDList = new SelectList(empBiz.GetCodeData("GOV_KIND"), "CodeDesc", "CodeDesc");
            PARMCodeBIZ para = new PARMCodeBIZ();
            ViewBag.AddDay = (Convert.ToInt32(para.GetCASE_END_TIME("OVERDUE_DAYS"))) * 1;
        }

        /// <summary>
        /// 結果列表
        /// </summary>
        /// <param name="AToH"></param>
        /// <param name="pageNum"></param>
        /// <param name="strSortExpression"></param>
        /// <param name="strSortDirection"></param>
        /// <returns></returns>
        public ActionResult _QueryResult(AgentToHandle AToH, int pageNum = 1, string strSortExpression = "", string strSortDirection = "")
        {
            HttpCookie  AToHCookie = new HttpCookie("QueryCookie");
            AToHCookie.Values.Add("GovUnit", AToH.GovUnit);
            AToHCookie.Values.Add("GovNo", AToH.GovNo);
            AToHCookie.Values.Add("Person", AToH.Person);
            AToHCookie.Values.Add("Speed", AToH.Speed);
            AToHCookie.Values.Add("ReceiveKind", AToH.ReceiveKind);
            AToHCookie.Values.Add("CaseKind", AToH.CaseKind);
            AToHCookie.Values.Add("CaseKind2", AToH.CaseKind2);
            AToHCookie.Values.Add("GovDateS", AToH.GovDateS);
            AToHCookie.Values.Add("GovDateE", AToH.GovDateE);
            AToHCookie.Values.Add("Unit", AToH.Unit);
            AToHCookie.Values.Add("CreatedDateS", AToH.CreatedDateS);
            AToHCookie.Values.Add("CreatedDateE", AToH.CreatedDateE);
            AToHCookie.Values.Add("CaseNo", AToH.CaseNo);
            AToHCookie.Values.Add("CurrentPage", pageNum.ToString());
            Response.Cookies.Add(AToHCookie);

            return PartialView("_QueryResult", SearchList(AToH, pageNum, strSortExpression, strSortDirection));
        }
        /// <summary>
        /// 實際查詢動作
        /// </summary>
        /// <param name="AToH"></param>
        /// <param name="pageNum"></param>
        /// <param name="strSortExpression"></param>
        /// <param name="strSortDirection"></param>
        /// <returns></returns>
        public AgentToHandleViewModel SearchList(AgentToHandle AToH, int pageNum = 1, string strSortExpression = "", string strSortDirection = "")
        {
            AToH.LanguageType = Session["CultureName"].ToString();

            AToH.CreatedDateS = UtlString.FormatDateTwStringToAd(AToH.CreatedDateS);
            AToH.CreatedDateE = UtlString.FormatDateTwStringToAd(AToH.CreatedDateE);
            AToH.GovDateS = UtlString.FormatDateTwStringToAd(AToH.GovDateS);
            AToH.GovDateE = UtlString.FormatDateTwStringToAd(AToH.GovDateE);

            string EmpId = LogonUser.Account;
            IList<AgentToHandle> result = AToHBIZ.GetQueryList(AToH, pageNum, strSortExpression, strSortDirection, EmpId);

            var viewModel = new AgentToHandleViewModel()
            {
                AgentToHandle = AToH,
                AgentToHandleList = result,
            };

         

            //分頁相關設定
            viewModel.AgentToHandle.PageSize = AToHBIZ.PageSize;
            viewModel.AgentToHandle.CurrentPage = AToHBIZ.PageIndex;
            viewModel.AgentToHandle.TotalItemCount = AToHBIZ.DataRecords;
            viewModel.AgentToHandle.SortExpression = strSortExpression;
            viewModel.AgentToHandle.SortDirection = strSortDirection;

            viewModel.AgentToHandle.GovUnit = AToH.GovUnit;
            viewModel.AgentToHandle.GovNo = AToH.GovNo;
            viewModel.AgentToHandle.Person = AToH.Person;
            viewModel.AgentToHandle.Speed = AToH.Speed;
            viewModel.AgentToHandle.ReceiveKind = AToH.ReceiveKind;
            viewModel.AgentToHandle.CaseKind = AToH.CaseKind;
            viewModel.AgentToHandle.CaseKind2 = AToH.CaseKind2;
            viewModel.AgentToHandle.GovDateS = AToH.GovDateS;
            viewModel.AgentToHandle.GovDateE = AToH.GovDateE;
            viewModel.AgentToHandle.Unit = AToH.Unit;
            viewModel.AgentToHandle.CreatedDateS = AToH.CreatedDateS;
            viewModel.AgentToHandle.CreatedDateE = AToH.CreatedDateE;
            return viewModel;
        }

        /// <summary>
        /// 案件退回
        /// </summary>
        /// <param name="strIds"></param>
        /// <returns></returns>
        public ActionResult ReturnClose(AgentToHandle model, string strIds)
        {
            string[] aryId = strIds.Split(',');
            List<Guid> guidList = (from id in aryId where !string.IsNullOrEmpty(id) select new Guid(id)).ToList();
            string userId = LogonUser.Account;
            return Json(AToHBIZ.ReturnClose(model, guidList, userId) > 0 ? new JsonReturn() { ReturnCode = "1", ReturnMsg = "" }
                                                        : new JsonReturn() { ReturnCode = "0", ReturnMsg = Lang.csfs_del_fail });
        }
        /// <summary>
        /// 收發待辦
        /// </summary>
        /// <param name="caseIdList"></param>
        /// <param name="agentIdList"></param>
        /// <returns></returns>
        public ActionResult AssignSet(AgentToHandle model, string caseIdList)
        {
            //AssignAgents
            string[] aryId = caseIdList.Split(',');
            List<Guid> aryCaseId = (from id in aryId where !string.IsNullOrEmpty(id) select new Guid(id)).ToList();
            //aryId = agentIdList.Split(',');
            //List<string> aryAgentId = (from id in aryId where !string.IsNullOrEmpty(id) select id).ToList();
            string userId = LogonUser.Account;
            return Json(AToHBIZ.AssignSet(model,aryCaseId, userId));
        }
        /// <summary>
        /// 實際分派動作
        /// </summary>
        /// <param name="caseIdList"></param>
        /// <param name="agentIdList"></param>
        /// <returns></returns>
        public ActionResult AssignAgent(string caseIdList, string agentIdList)
        {
            string[] aryId = caseIdList.Split(',');
            List<Guid> aryCaseId = (from id in aryId where !string.IsNullOrEmpty(id) select new Guid(id)).ToList();
            aryId = agentIdList.Split(',');
            List<string> aryAgentId = (from id in aryId where !string.IsNullOrEmpty(id) select id).ToList();
            string userId = LogonUser.Account;
            return Json(AToHBIZ.AgentReassign(aryCaseId, aryAgentId, userId));
        }
        /// <summary>
        /// 呈核
        /// </summary>
        /// <returns></returns>
        public ActionResult ChenHe(string CaseIdarr)
        {
            string userId = LogonUser.Account;
            string[] caseid = CaseIdarr.Split(',');
            List<Guid> aryCaseId = (from id in caseid where !string.IsNullOrEmpty(id) select new Guid(id)).ToList();
            string agentIdList = AToHBIZ.GetManagerID(userId);
            caseid = agentIdList.Split(',');
            List<string> aryAgentId = (from id in caseid where !string.IsNullOrEmpty(id) select id).ToList();
            return Json(AToHBIZ.AgentSubmit(aryCaseId, aryAgentId, userId));
        }
        public ActionResult OverDue(AgentToHandle model, string strIds)
        {
            string userId = LogonUser.Account;
            string[] caseid = strIds.Split(',');
            List<Guid> aryCaseId = (from id in caseid where !string.IsNullOrEmpty(id) select new Guid(id)).ToList();
            string agentIdList = AToHBIZ.GetManagerID(userId);
            caseid = agentIdList.Split(',');
            List<string> aryAgentId = (from id in caseid where !string.IsNullOrEmpty(id) select id).ToList();
            return Json(AToHBIZ.OverDue(model, aryCaseId, aryAgentId, userId));
        }

        ///// <summary>
        ///// 更改來文機關類型.取得來文機關
        ///// </summary>
        ///// <param name="govKind"></param>
        ///// <returns></returns>
        //public JsonResult ChangGovUnit(string govKind)
        //{
        //    PARMCodeBIZ parm = new PARMCodeBIZ();
        //    List<KeyValuePair<string, string>> items = new List<KeyValuePair<string, string>>();
        //    if (string.IsNullOrEmpty(govKind)) return Json(items);
        //    //* 取得大類的CodeNo,以知道小類的CodeType
        //    var itemKind = parm.GetCodeData("GOV_KIND").FirstOrDefault(a => a.CodeDesc == govKind);
        //    if (itemKind == null) return Json(items);

        //    var list = parm.GetCodeData(itemKind.CodeNo);
        //    if (list.Any())
        //    {
        //        items.AddRange(list.Select(govUnit => new KeyValuePair<string, string>(govUnit.CodeNo.ToString(), govUnit.CodeDesc)));
        //    }
        //    return Json(items);
        //}
        /// <summary>
        /// 根據案件類型大類(扣押/外來文)來取小類
        /// </summary>
        /// <param name="caseKind"></param>
        /// <returns></returns>
        public JsonResult ChangCaseKind1(string caseKind)
        {
            PARMCodeBIZ parm = new PARMCodeBIZ();
            List<KeyValuePair<string, string>> items = new List<KeyValuePair<string, string>>();
            if (string.IsNullOrEmpty(caseKind)) return Json(items);
            //* 取得大類的CodeNo,以知道小類的CodeType
            var itemKind = parm.GetCodeData("CASE_KIND").FirstOrDefault(a => a.CodeDesc == caseKind);
            if (itemKind == null) return Json(items);

            var list = parm.GetCodeData(itemKind.CodeNo);
            if (list.Any())
            {
                items.AddRange(list.Select(govUnit => new KeyValuePair<string, string>(govUnit.CodeNo.ToString(), govUnit.CodeDesc)));
            }
            return Json(items);
        }

        public ActionResult CancelSendAgain(string strCaseNos)
        {
            return Json(AToHBIZ.CancelSendAgain(strCaseNos));
        }
    }
}