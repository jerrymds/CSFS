using CTBC.CSFS.Pattern;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CTBC.CSFS.BussinessLogic;
using CTBC.CSFS.Filter;
using CTBC.CSFS.Resource;
using CTBC.CSFS.ViewModels;
using CTBC.CSFS.Models;
using CTBC.FrameWork.Util;

namespace CTBC.CSFS.Areas.QueryAndExport.Controllers
{
    public class OriginalInfoQueryController : AppController
    {
        LendDataBIZ lenddata = new LendDataBIZ();
        // GET: QueryAndExport/OriginalInfoQuery
        [RootPageFilter]
        public ActionResult Index(string isBack)
        {
            LendData model = new LendData();
            if (isBack == "1")
            {
                HttpCookie cookies = Request.Cookies.Get("QueryCookie");
                if (cookies != null)
                {
                    if (cookies.Values["GovUnit"] != null) model.GovUnit = cookies.Values["GovUnit"];
                    if (cookies.Values["GovNo"] != null) model.GovNo = cookies.Values["GovNo"];
                    //20170811 RC RQ-2015-019666-020 派件至跨單位 宏祥 update start
                    //if (cookies.Values["AgentUser"] != null) model.AgentUser = cookies.Values["AgentUser"];
                    if (cookies.Values["AgentDepartment"] != null) model.AgentDepartment = cookies.Values["AgentDepartment"];
                    //if (cookies.Values["AgentDepartment2"] != null) model.AgentDepartment2 = cookies.Values["AgentDepartment2"];
                    //if (cookies.Values["AgentDepartmentUser"] != null) model.AgentDepartmentUser = cookies.Values["AgentDepartmentUser"];
                    //20171103 RC RQ-2015-019666-003 系統功能修正 宏祥 update start
                    if (cookies.Values["AgentDepartment2"] != null) ViewBag.AgentDepartment2Query = cookies.Values["AgentDepartment2"];
                    if (cookies.Values["AgentDepartmentUser"] != null) ViewBag.AgentDepartmentUserQuery = cookies.Values["AgentDepartmentUser"];
                    //20171103 RC RQ-2015-019666-003 系統功能修正 宏祥 update end
                    //20170811 RC RQ-2015-019666-020 派件至跨單位 宏祥 update end
                    if (cookies.Values["Speed"] != null) model.Speed = cookies.Values["Speed"];
                    if (cookies.Values["ReceiveKind"] != null) model.ReceiveKind = cookies.Values["ReceiveKind"];
                    if (cookies.Values["CaseKind"] != null) model.CaseKind = cookies.Values["CaseKind"];
                    if (cookies.Values["CaseKind2"] != null) ViewBag.CaseKind2Query = cookies.Values["CaseKind2"];
                    if (cookies.Values["GovDateStart"] != null) model.GovDateStart = cookies.Values["GovDateStart"];
                    if (cookies.Values["GovDateEnd"] != null) model.GovDateEnd = cookies.Values["GovDateEnd"];
                    if (cookies.Values["ClientID"] != null) model.ClientID = cookies.Values["ClientID"];
                    if (cookies.Values["LendStatus"] != null) model.LendStatus = cookies.Values["LendStatus"];
                    if (cookies.Values["CaseNo"] != null) model.CaseNo = cookies.Values["CaseNo"];
                    ViewBag.CurrentPage = cookies.Values["CurrentPage"];
                    ViewBag.isQuery = "1";
                }
            }

            InitDropdownListOptions();
            return View(model);
        }

        /// <summary>
        /// 綁定小類
        /// </summary>
        /// <param name="govKind"></param>
        /// <returns></returns>
        public JsonResult ChangCaseKind(string caseKind)
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
                items.AddRange(list.Select(govUnit => new KeyValuePair<string, string>(govUnit.CodeDesc.ToString(), govUnit.CodeDesc)));
            }
            return Json(items);
        }

        /// <summary>
        /// 綁定下拉菜單
        /// </summary>
        public void InitDropdownListOptions()
        {
            PARMCodeBIZ parm = new PARMCodeBIZ();
            ViewBag.CaseKindList = new SelectList(parm.GetCodeData("CASE_KIND"), "CodeDesc", "CodeDesc");       //* 類別-大類
            ViewBag.CaseKind2List = new SelectList(parm.GetCodeData("CASE_SEIZURE"), "CodeDesc", "CodeDesc");   //* 類別-小類
            ViewBag.GOV_KINDList = new SelectList(parm.GetCodeData("GOV_KIND"), "CodeDesc", "CodeDesc");        //* 來文機關類型
            ViewBag.SpeedList = new SelectList(parm.GetCodeData("INCOME_SPEED"), "CodeDesc", "CodeDesc");       //* 速別
            ViewBag.ReceiveKindList = new SelectList(parm.GetCodeData("INCOME_TYPE"), "CodeDesc", "CodeDesc");  //* 來文方式
            //20170811 RC RQ-2015-019666-020 派件至跨單位 宏祥 update start
            //ViewBag.AgentUserList = new SelectList(new LdapEmployeeBiz().GetAllEmployeeInEmployeeView(), "EmpID", "EmpIdAndName");
            ViewBag.AgentDepartmentList = new SelectList(parm.GetCodeData("CollectionToAgent_AgentDDLDepartment"), "CodeNo", "CodeDesc");
            ViewBag.AgentDepartment2List = new SelectList(new AgentSettingBIZ().GetAgentDepartment2View(), "SectionName", "SectionName");
            ViewBag.AgentDepartmentUserList = new SelectList(new AgentSettingBIZ().GetAgentDepartmentUserView(), "EmpID", "EmpIdAndName");
            //20170811 RC RQ-2015-019666-020 派件至跨單位 宏祥 update end
            BindIsEnableList();//*綁定歸還狀態
        }

        /// <summary>
        /// 綁定歸還狀態
        /// </summary>
        public void BindIsEnableList()
        {
            List<SelectListItem> item2 = new List<SelectListItem>
            {
                new SelectListItem() {Text =Lang.csfs_not_return, Value = "0"},
                new SelectListItem() {Text =Lang.csfs_has_return, Value = "1"}
            };
            ViewBag.LendStatusList = item2;
        }

        public ActionResult _QueryResult(LendData model, int pageNum = 1, string strSortExpression = "LendID", string strSortDirection = "asc")
        {
            HttpCookie modelCookie = new HttpCookie("QueryCookie");
            modelCookie.Values.Add("GovUnit", model.GovUnit);
            modelCookie.Values.Add("GovNo", model.GovNo);
            //20170811 RC RQ-2015-019666-020 派件至跨單位 宏祥 update start
            //modelCookie.Values.Add("AgentUser", model.AgentUser);
            modelCookie.Values.Add("AgentDepartment", model.AgentDepartment);
            modelCookie.Values.Add("AgentDepartment2", model.AgentDepartment2);
            modelCookie.Values.Add("AgentDepartmentUser", model.AgentDepartmentUser);
            //20170811 RC RQ-2015-019666-020 派件至跨單位 宏祥 update end
            modelCookie.Values.Add("Speed", model.Speed);
            modelCookie.Values.Add("ReceiveKind", model.ReceiveKind);
            modelCookie.Values.Add("CaseKind", model.CaseKind);
            modelCookie.Values.Add("CaseKind2", model.CaseKind2);
            modelCookie.Values.Add("GovDateStart", model.GovDateStart);
            modelCookie.Values.Add("GovDateEnd", model.GovDateEnd);
            modelCookie.Values.Add("ClientID", model.ClientID);
            modelCookie.Values.Add("LendStatus", model.LendStatus);
            modelCookie.Values.Add("CaseNo", model.CaseNo);
            modelCookie.Values.Add("CurrentPage", pageNum.ToString());
            Response.Cookies.Add(modelCookie);

            return PartialView("_QueryResult", LendDataSearchList(model, pageNum, strSortExpression, strSortDirection));
        }

        /// <summary>
        /// 實際查詢所有案件未歸還資料動作
        /// </summary>
        /// <param name="LendData"></param>
        /// <param name="pageNum"></param>
        /// <param name="strSortExpression"></param>
        /// <param name="strSortDirection"></param>
        /// <returns></returns>
        public AgentOriginalInfoViewModel LendDataSearchList(LendData model, int pageNum = 1, string strSortExpression = "LendID", string strSortDirection = "asc")
        {
            model.GovDateStart = UtlString.FormatDateTwStringToAd(model.GovDateStart);
            model.GovDateEnd = UtlString.FormatDateTwStringToAd(model.GovDateEnd);

            IList<LendData> result = lenddata.LendDataSearchList(model, pageNum, strSortExpression, strSortDirection);

            var viewModel = new AgentOriginalInfoViewModel()
            {
                LendDataInfo = model,
                LendDataInfoList = result,
            };

            viewModel.LendDataInfo.PageSize = lenddata.PageSize;
            viewModel.LendDataInfo.CurrentPage = lenddata.PageIndex;
            viewModel.LendDataInfo.TotalItemCount = lenddata.DataRecords;
            viewModel.LendDataInfo.SortExpression = strSortExpression;
            viewModel.LendDataInfo.SortDirection = strSortDirection;

            return viewModel;
        }
    }
}