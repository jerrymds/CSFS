using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CTBC.CSFS.Models;
using CTBC.CSFS.BussinessLogic;
using CTBC.FrameWork.Util;
using CTBC.CSFS.Pattern;
using System.Web.Mvc;
using CTBC.CSFS.Filter;
using CTBC.CSFS.ViewModels;

namespace CTBC.CSFS.Areas.Director.Controllers
{
    public class DirectorApprovedController : AppController
    {
        DirectorToApproveBIZ _directorBiz;
        public DirectorApprovedController()
        {
            _directorBiz = new DirectorToApproveBIZ(this);
        }
        [RootPageFilter]
        public ActionResult Index(string isBack)
        {
            DirectorToApprove model = new DirectorToApprove();
            if (isBack == "1")
            {
                HttpCookie cookies = Request.Cookies.Get("QueryCookie");
                if (cookies != null)
                {
                    if (cookies.Values["CaseKind"] != null) model.CaseKind = cookies.Values["CaseKind"];
                    if (cookies.Values["CaseKind2"] != null) ViewBag.CaseKind2Query = cookies.Values["CaseKind2"];
                    if (cookies.Values["GovUnit"] != null) model.GovUnit = cookies.Values["GovUnit"];
                    if (cookies.Values["CaseNo"] != null) model.CaseNo = cookies.Values["CaseNo"];
                    if (cookies.Values["GovDateS"] != null) model.GovDateS = cookies.Values["GovDateS"];
                    if (cookies.Values["GovDateE"] != null) model.GovDateE = cookies.Values["GovDateE"];
                    if (cookies.Values["Speed"] != null) model.Speed = cookies.Values["Speed"];
                    if (cookies.Values["ReceiveKind"] != null) model.ReceiveKind = cookies.Values["ReceiveKind"];
                    if (cookies.Values["GovNo"] != null) model.GovNo = cookies.Values["GovNo"];
                    if (cookies.Values["ApproveDateS"] != null) model.ApproveDateS = cookies.Values["ApproveDateS"];
                    if (cookies.Values["ApproveDateE"] != null) model.ApproveDateE = cookies.Values["ApproveDateE"];
                    if (cookies.Values["Unit"] != null) model.Unit = cookies.Values["Unit"];
                    if (cookies.Values["CreateUser"] != null) model.CreateUser = cookies.Values["CreateUser"];
                    if (cookies.Values["SendDateS"] != null) model.SendDateS = cookies.Values["SendDateS"];
                    if (cookies.Values["SendDateE"] != null) model.SendDateE = cookies.Values["SendDateE"];
                    //20170811 RC RQ-2015-019666-020 派件至跨單位 宏祥 update start
                    //if (cookies.Values["AgentUser"] != null) model.AgentUser = cookies.Values["AgentUser"];
                    if (cookies.Values["AgentDepartment"] != null) model.AgentDepartment = cookies.Values["AgentDepartment"];
                    if (cookies.Values["AgentDepartment2"] != null) model.AgentDepartment2 = cookies.Values["AgentDepartment2"];
                    if (cookies.Values["AgentDepartmentUser"] != null) model.AgentDepartmentUser = cookies.Values["AgentDepartmentUser"];
                    //20170811 RC RQ-2015-019666-020 派件至跨單位 宏祥 update end
					if (cookies.Values["SendKind"] != null) model.SendKind = cookies.Values["SendKind"];
                    if (cookies.Values["ApproveManager"] != null) model.ApproveManager = cookies.Values["ApproveManager"];
                    if (cookies.Values["ID"] != null) model.ID = cookies.Values["ID"];
                    ViewBag.CurrentPage = cookies.Values["CurrentPage"];
                    ViewBag.isQuery = "1";
                }
            }

            InitDropdownListOptions();
            return View(model);
        }
        [HttpPost]
        public ActionResult _QueryResult(DirectorToApprove model, int pageNum = 1, string strSortExpression = "CaseNo", string strSortDirection = "asc")
        {
            #region Cookie
            HttpCookie modelCookie = new HttpCookie("QueryCookie");
            modelCookie.Values.Add("CaseKind", model.CaseKind);
            modelCookie.Values.Add("CaseKind2", model.CaseKind2);
            modelCookie.Values.Add("GovUnit", model.GovUnit);
            modelCookie.Values.Add("CaseNo", model.CaseNo);
            modelCookie.Values.Add("GovDateS", model.GovDateS);
            modelCookie.Values.Add("GovDateE", model.GovDateE);
            modelCookie.Values.Add("Speed", model.Speed);
            modelCookie.Values.Add("ReceiveKind", model.ReceiveKind);
            modelCookie.Values.Add("GovNo", model.GovNo);
            modelCookie.Values.Add("ApproveDateS", model.ApproveDateS);
            modelCookie.Values.Add("ApproveDateE", model.ApproveDateE);
            //20170811 RC RQ-2015-019666-020 派件至跨單位 宏祥 update start
            //modelCookie.Values.Add("AgentUser", model.AgentUser);
            modelCookie.Values.Add("AgentDepartment", model.AgentDepartment);
            modelCookie.Values.Add("AgentDepartment2", model.AgentDepartment2);
            modelCookie.Values.Add("AgentDepartmentUser", model.AgentDepartmentUser);
            //20170811 RC RQ-2015-019666-020 派件至跨單位 宏祥 update end
            modelCookie.Values.Add("CreateUser", model.CreateUser);
            modelCookie.Values.Add("Unit", model.Unit);
            modelCookie.Values.Add("SendDateS", model.SendDateS);
            modelCookie.Values.Add("SendDateE", model.SendDateE);
            modelCookie.Values.Add("CurrentPage", pageNum.ToString());
            modelCookie.Values.Add("SendKind", model.SendKind);
            modelCookie.Values.Add("ApproveManager", model.ApproveManager);
            modelCookie.Values.Add("ID", model.ID);
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
            if (!string.IsNullOrEmpty(model.ApproveDateE))
            {
                model.ApproveDateE = UtlString.FormatDateTwStringToAd(model.ApproveDateE);
            }
            if (!string.IsNullOrEmpty(model.ApproveDateS))
            {
                model.ApproveDateS = UtlString.FormatDateTwStringToAd(model.ApproveDateS);
            }

            if (!string.IsNullOrEmpty(model.SendDateS))
            {
                model.SendDateS = UtlString.FormatDateTwStringToAd(model.SendDateS);
            }
            if (!string.IsNullOrEmpty(model.SendDateE))
            {
                model.SendDateE = UtlString.FormatDateTwStringToAd(model.SendDateE);
            }
            IList<DirectorToApprove> list = _directorBiz.GetApprovedCase(model, pageNum, strSortExpression, strSortDirection, UserId);
            var dtvm = new DirectorToApproveViewModel()
            {
                DirectorToApprove = model,
                DirectorToApprovelist = list,
            };
            //分頁相關設定
            dtvm.DirectorToApprove.PageSize = _directorBiz.PageSize;
            dtvm.DirectorToApprove.CurrentPage = _directorBiz.PageIndex;
            dtvm.DirectorToApprove.TotalItemCount = _directorBiz.DataRecords;
            dtvm.DirectorToApprove.SortExpression = strSortExpression;
            dtvm.DirectorToApprove.SortDirection = strSortDirection;

            dtvm.DirectorToApprove.CaseNo = model.CaseNo;
            dtvm.DirectorToApprove.GovUnit = model.GovUnit;
            dtvm.DirectorToApprove.GovDate = model.GovDate;
            dtvm.DirectorToApprove.GovNo = model.GovNo;
            dtvm.DirectorToApprove.Person = model.Person;
            dtvm.DirectorToApprove.CaseKind = model.CaseKind;
            dtvm.DirectorToApprove.CaseKind2 = model.CaseKind2;
            dtvm.DirectorToApprove.Speed = model.Speed;

            dtvm.DirectorToApprove.SendKind = model.SendKind;
            dtvm.DirectorToApprove.ApproveManager = model.ApproveManager;
            dtvm.DirectorToApprove.ID = model.ID;
            
            return PartialView("_QueryResult", dtvm);
        }
        public void InitDropdownListOptions()
        {
            PARMCodeBIZ parm = new PARMCodeBIZ();
            ViewBag.CASE_END_TIME = parm.GetCASE_END_TIME("CASE_END_TIME");                                     //* 每天最晚時間
            ViewBag.GOV_KINDList = new SelectList(parm.GetCodeData("GOV_KIND"), "CodeDesc", "CodeDesc");        //* 來文機關類型
            ViewBag.SpeedList = new SelectList(parm.GetCodeData("INCOME_SPEED"), "CodeDesc", "CodeDesc");       //* 速別
            ViewBag.ReceiveKindList = new SelectList(parm.GetCodeData("INCOME_TYPE"), "CodeDesc", "CodeDesc");  //* 來文方式
            ViewBag.CaseKindList = new SelectList(parm.GetCodeData("CASE_KIND"), "CodeDesc", "CodeDesc");
            ViewBag.CaseKind2List = new SelectList(parm.GetCodeData("CASE_SEIZURE"), "CodeDesc", "CodeDesc");
            ViewBag.SendKindList = new SelectList(parm.GetCodeData("SendKind"), "CodeDesc", "CodeDesc");        //*發文方式
            ViewBag.ReturnReasonList = new SelectList(parm.GetCodeData("DIRECTOR_RETURNREASON"), "CodeDesc", "CodeDesc");
            //LdapEmployeeBiz emp = new LdapEmployeeBiz();
            //List<string> leaderList = emp.GetTopEmployeeDirectorIdList();
            //ViewBag.isTopDirector = leaderList.Contains(LogonUser.Account);
            //20170811 RC RQ-2015-019666-020 派件至跨單位 宏祥 update start
            //ViewBag.AgentUserList = new SelectList(new LdapEmployeeBiz().GetAllEmployeeNoMangerView(), "EmpID", "EmpIdAndName");
            ViewBag.AgentDepartmentList = new SelectList(parm.GetCodeData("CollectionToAgent_AgentDDLDepartment"), "CodeNo", "CodeDesc");
            ViewBag.AgentDepartment2List = new SelectList(new AgentSettingBIZ().GetAgentDepartment2View(), "SectionName", "SectionName");
            ViewBag.AgentDepartmentUserList = new SelectList(new AgentSettingBIZ().GetAgentDepartmentUserView(), "EmpID", "EmpIdAndName");
            //20170811 RC RQ-2015-019666-020 派件至跨單位 宏祥 update end
            //ViewBag.ApproveManagerList = new SelectList(new LdapEmployeeBiz().GetAllEmployeeNoMangerView(), "EmpID", "EmpIdAndName");
			   ViewBag.ApproveManagerList = new SelectList(new LdapEmployeeBiz().GetAllMangers(), "EmpID", "EmpIdAndName");
        }
	}
}