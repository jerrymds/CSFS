using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CTBC.CSFS.BussinessLogic;
using CTBC.CSFS.Filter;
using CTBC.CSFS.Models;
using CTBC.FrameWork.Util;
using CTBC.CSFS.ViewModels;
using CTBC.CSFS.Resource;
using CTBC.CSFS.Pattern;

namespace CTBC.CSFS.Areas.Agent.Controllers
{
    public class SendAgainController : AppController
    {
        CaseQueryBIZ CQBIZ = new CaseQueryBIZ();
        PARMCodeBIZ parm = new PARMCodeBIZ();

        //
        // GET: /Agent/SendAgain/
        [RootPageFilter]
        public ActionResult Index(string isBack)
        {
            CaseQuery model = new CaseQuery();
            if (isBack == "1")
            {
                //*執行cookie里的內容
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
                    if (cookies.Values["CreatedDateS"] != null) model.CreatedDateS = cookies.Values["CreatedDateS"];
                    if (cookies.Values["CreatedDateE"] != null) model.CreatedDateE = cookies.Values["CreatedDateE"];
                    if (cookies.Values["GovDateS"] != null) model.GovDateS = cookies.Values["GovDateS"];
                    if (cookies.Values["GovDateE"] != null) model.GovDateE = cookies.Values["GovDateE"];
                    if (cookies.Values["Unit"] != null) model.Unit = cookies.Values["Unit"];
                    if (cookies.Values["CreateUser"] != null) model.CreateUser = cookies.Values["CreateUser"];
                    if (cookies.Values["ObligorNo"] != null) model.ObligorNo = cookies.Values["ObligorNo"];
                    if (cookies.Values["ObligorName"] != null) model.ObligorName = cookies.Values["ObligorName"];
                    if (cookies.Values["SendDateS"] != null) model.SendDateS = cookies.Values["SendDateS"];
                    if (cookies.Values["SendDateE"] != null) model.SendDateE = cookies.Values["SendDateE"];
                    if (cookies.Values["SendNo"] != null) model.SendNo = cookies.Values["SendNo"];
                    if (cookies.Values["OverDateS"] != null) model.OverDateS = cookies.Values["OverDateS"];
                    if (cookies.Values["OverDateE"] != null) model.OverDateE = cookies.Values["OverDateE"];
                    if (cookies.Values["Status"] != null) model.Status = cookies.Values["Status"];
                    if (cookies.Values["CaseNo"] != null) model.CaseNo = cookies.Values["CaseNo"];
                    ViewBag.CurrentPage = cookies.Values["CurrentPage"];
                    ViewBag.isQuery = "1";
                }
            }

            InitDropdownListOptions();
            return View(model);
        }

        public void InitDropdownListOptions()
        {
            List<SelectListItem> StatusName = new List<SelectListItem>{ 
                new SelectListItem() { Value = "Z01", Text = "主管-放行結案" },
                new SelectListItem() { Value = "D03", Text = "主管-待20日支付" }
            };
            ViewBag.StatusName = StatusName;
            ViewBag.CASE_END_TIME = parm.GetCASE_END_TIME("CASE_END_TIME");                                     //* 每天最晚時間
            ViewBag.GOV_KINDList = new SelectList(parm.GetCodeData("GOV_KIND"), "CodeDesc", "CodeDesc");        //* 來文機關類型
            ViewBag.SpeedList = new SelectList(parm.GetCodeData("INCOME_SPEED"), "CodeDesc", "CodeDesc");       //* 速別
            ViewBag.ReceiveKindList = new SelectList(parm.GetCodeData("INCOME_TYPE"), "CodeDesc", "CodeDesc");  //* 來文方式
            ViewBag.CaseKindList = new SelectList(parm.GetCodeData("CASE_KIND"), "CodeDesc", "CodeDesc");
            ViewBag.CaseKind2List = new SelectList(CQBIZ.GetCodeData("CASE_SEIZURE"), "CodeDesc", "CodeDesc");
            //20170811 RC RQ-2015-019666-020 派件至跨單位 宏祥 update start
            //ViewBag.AgentUserList = new SelectList(new LdapEmployeeBiz().GetAllEmployeeInEmployeeView(), "EmpID", "EmpIdAndName");
            ViewBag.AgentDepartmentList = new SelectList(parm.GetCodeData("CollectionToAgent_AgentDDLDepartment"), "CodeNo", "CodeDesc");
            ViewBag.AgentDepartment2List = new SelectList(new AgentSettingBIZ().GetAgentDepartment2View(), "SectionName", "SectionName");
            ViewBag.AgentDepartmentUserList = new SelectList(new AgentSettingBIZ().GetAgentDepartmentUserView(), "EmpID", "EmpIdAndName");
            //20170811 RC RQ-2015-019666-020 派件至跨單位 宏祥 update end
        }

        public ActionResult _QueryResult(CaseQuery model, int pageNum = 1, string strSortExpression = "CaseId", string strSortDirection = "asc")
        {
            #region 返回cookie
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
            modelCookie.Values.Add("CreatedDateS", model.CreatedDateS);
            modelCookie.Values.Add("CreatedDateE", model.CreatedDateE);
            modelCookie.Values.Add("GovDateS", model.GovDateS);
            modelCookie.Values.Add("GovDateE", model.GovDateE);
            modelCookie.Values.Add("CaseNo", model.CaseNo);
            modelCookie.Values.Add("Unit", model.Unit);
            modelCookie.Values.Add("CreateUser", model.CreateUser);
            modelCookie.Values.Add("ObligorNo", model.ObligorNo);
            modelCookie.Values.Add("ObligorName", model.ObligorName);
            modelCookie.Values.Add("SendDateS", model.SendDateS);
            modelCookie.Values.Add("SendDateE", model.SendDateE);
            modelCookie.Values.Add("SendNo", model.SendNo);
            modelCookie.Values.Add("OverDateS", model.OverDateS);
            modelCookie.Values.Add("OverDateE", model.OverDateE);
            modelCookie.Values.Add("Status", model.Status);
            modelCookie.Values.Add("CurrentPage", pageNum.ToString());
            Response.Cookies.Add(modelCookie);
            #endregion

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
            IList<CaseQuery> list = CQBIZ.GetDataForSendAgain(model, pageNum, strSortExpression, strSortDirection);
            var dtvm = new CaseQueryViewModel()
            {
                CaseQuery = model,
                CaseQueryList = list,
            };
            //分頁相關設定
            dtvm.CaseQuery.PageSize = CQBIZ.PageSize;
            dtvm.CaseQuery.CurrentPage = CQBIZ.PageIndex;
            dtvm.CaseQuery.TotalItemCount = CQBIZ.DataRecords;
            dtvm.CaseQuery.SortExpression = strSortExpression;
            dtvm.CaseQuery.SortDirection = strSortDirection;

            dtvm.CaseQuery.CaseKind = model.CaseKind;
            dtvm.CaseQuery.CaseKind2 = model.CaseKind2;
            dtvm.CaseQuery.GovUnit = model.GovUnit;
            dtvm.CaseQuery.CaseNo = model.CaseNo;
            dtvm.CaseQuery.GovDateS = model.GovDateS;
            dtvm.CaseQuery.GovDateE = model.GovDateE;
            dtvm.CaseQuery.Speed = model.Speed;
            dtvm.CaseQuery.ReceiveKind = model.ReceiveKind;
            dtvm.CaseQuery.GovNo = model.GovNo;
            dtvm.CaseQuery.CreatedDateS = model.CreatedDateS;
            dtvm.CaseQuery.CreatedDateE = model.CreatedDateE;
            dtvm.CaseQuery.Unit = model.Unit;
            dtvm.CaseQuery.CreateUser = model.CreateUser;
            dtvm.CaseQuery.ObligorNo = model.ObligorNo;
            dtvm.CaseQuery.ObligorName = model.ObligorName;
            dtvm.CaseQuery.SendDateS = model.SendDateS;
            dtvm.CaseQuery.SendDateE = model.SendDateE;
            dtvm.CaseQuery.SendNo = model.SendNo;
            dtvm.CaseQuery.OverDateS = model.OverDateS;
            dtvm.CaseQuery.OverDateE = model.OverDateE;
            dtvm.CaseQuery.Status = model.Status;
            dtvm.CaseQuery.AgentUser = model.AgentUser;

            

            return PartialView("_QueryResult", dtvm);
        }

        public ActionResult SendAgainForMaster(string strIds)
        {
            string userId = LogonUser.Account;
            string[] aryId = strIds.Split(',');
            return Json(CQBIZ.SaveSendAgain(new Guid(aryId[0]), userId));
        }
    }
}