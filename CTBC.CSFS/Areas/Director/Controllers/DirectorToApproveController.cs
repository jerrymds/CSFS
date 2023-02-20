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
using System.IO;
using CTBC.CSFS.Resource;
namespace CTBC.CSFS.Areas.Director.Controllers
{
    public class DirectorToApproveController : AppController
    {
        DirectorToApproveBIZ _directorBiz;
        public DirectorToApproveController()
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
                    //Add by zhangwei 20180315 start
                    if (cookies.Values["CaseKind2"] != null) model.CaseKind2 = cookies.Values["CaseKind2"];
                    //Add by zhangwei 20180315 end
                    if (cookies.Values["GovUnit"] != null) model.GovUnit = cookies.Values["GovUnit"];
                    if (cookies.Values["CaseNo"] != null) model.CaseNo = cookies.Values["CaseNo"];
                    if (cookies.Values["GovDateS"] != null) model.GovDateS = cookies.Values["GovDateS"];
                    if (cookies.Values["GovDateE"] != null) model.GovDateE = cookies.Values["GovDateE"];
                    if (cookies.Values["Speed"] != null) model.Speed = cookies.Values["Speed"];
                    if (cookies.Values["ReceiveKind"] != null) model.ReceiveKind = cookies.Values["ReceiveKind"];
                    if (cookies.Values["GovNo"] != null) model.GovNo = cookies.Values["GovNo"];
                    if (cookies.Values["CreatedDateS"] != null) model.CreatedDateS = cookies.Values["CreatedDateS"];
                    if (cookies.Values["CreatedDateE"] != null) model.CreatedDateE = cookies.Values["CreatedDateE"];
                    if (cookies.Values["Unit"] != null) model.Unit = cookies.Values["Unit"];
                    if (cookies.Values["CreateUser"] != null) model.CreateUser = cookies.Values["CreateUser"];
                    if (cookies.Values["SendDateS"] != null) model.SendDateS = cookies.Values["SendDateS"];
                    if (cookies.Values["SendDateE"] != null) model.SendDateE = cookies.Values["SendDateE"];
                    //20170630 緊急 RQ-2015-019666-018 派件至跨單位 宏祥 update start
                    //if (cookies.Values["AgentUser"] != null) model.AgentUser = cookies.Values["AgentUser"];
                    if (cookies.Values["AgentDepartment"] != null) model.AgentDepartment = cookies.Values["AgentDepartment"];
                    //if (cookies.Values["AgentDepartment2"] != null) model.AgentDepartment2 = cookies.Values["AgentDepartment2"];
                    //if (cookies.Values["AgentDepartmentUser"] != null) model.AgentDepartmentUser = cookies.Values["AgentDepartmentUser"];
                    //20171103 RC RQ-2015-019666-003 系統功能修正 宏祥 update start
                    if (cookies.Values["AgentDepartment2"] != null) ViewBag.AgentDepartment2Query = cookies.Values["AgentDepartment2"];
                    if (cookies.Values["AgentDepartmentUser"] != null) ViewBag.AgentDepartmentUserQuery = cookies.Values["AgentDepartmentUser"];
                    //20171103 RC RQ-2015-019666-003 系統功能修正 宏祥 update end
                    //20170630 緊急 RQ-2015-019666-018 派件至跨單位 宏祥 update end
					if (cookies.Values["SendKind"] != null) model.SendKind = cookies.Values["SendKind"];
                    if (cookies.Values["Status"] != null) model.Status = cookies.Values["Status"];
                    ViewBag.CurrentPage = cookies.Values["CurrentPage"];
                    ViewBag.isQuery = "1";
                }
            }

            //20170421 固定 RQ-2015-019666-017 派件至跨單位 宏祥 add start
            //判斷分行主管角色關閉發文資訊頁籤功能
            int intRolesCount = LogonUser.Roles.Count;
            string strRoleLDAPId = "";
            for (int i = 0; i < intRolesCount; i++)
            {
               if (LogonUser.Roles[i].RoleLDAPId != "")
               {
                  strRoleLDAPId = LogonUser.Roles[i].RoleLDAPId;
               }
            }
            ViewBag.IsBranchDirector = "0";
            if (strRoleLDAPId == "CSFS015")
            {
               ViewBag.IsBranchDirector = "1";
            }
            //判斷收發代辦案件是否逾期
            PARMCodeBIZ para = new PARMCodeBIZ();
            ViewBag.AddDay = (Convert.ToInt32(para.GetCASE_END_TIME("OVERDUE_DAYS"))) * 1;
            //20170421 固定 RQ-2015-019666-017 派件至跨單位 宏祥 add end

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
            modelCookie.Values.Add("CreatedDateS", model.CreatedDateS);
            modelCookie.Values.Add("CreatedDateE", model.CreatedDateE);
            //20170630 緊急 RQ-2015-019666-018 派件至跨單位 宏祥 update start
            //modelCookie.Values.Add("AgentUser", model.AgentUser);
            modelCookie.Values.Add("AgentDepartment", model.AgentDepartment);
            modelCookie.Values.Add("AgentDepartment2", model.AgentDepartment2);
            modelCookie.Values.Add("AgentDepartmentUser", model.AgentDepartmentUser);
            //20170630 緊急 RQ-2015-019666-018 派件至跨單位 宏祥 update end
            modelCookie.Values.Add("CreateUser", model.CreateUser);
            modelCookie.Values.Add("Unit", model.Unit);
            modelCookie.Values.Add("SendDateS", model.SendDateS);
            modelCookie.Values.Add("SendDateE", model.SendDateE);
            modelCookie.Values.Add("CurrentPage", pageNum.ToString());
            modelCookie.Values.Add("SendKind", model.SendKind);
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

            if (!string.IsNullOrEmpty(model.SendDateS))
            {
                model.SendDateS = UtlString.FormatDateTwStringToAd(model.SendDateS);
            }
            if (!string.IsNullOrEmpty(model.SendDateE))
            {
                model.SendDateE = UtlString.FormatDateTwStringToAd(model.SendDateE);
            }
            //20170630 緊急 RQ-2015-019666-018 派件至跨單位 宏祥 add start
            //判斷是否為分行主管角色，增加搜尋條件
            int intRolesCount = LogonUser.Roles.Count;
            string strRoleLDAPId = "";
            for (int i = 0; i < intRolesCount; i++)
            {
               if (LogonUser.Roles[i].RoleLDAPId != "")
               {
                  strRoleLDAPId = LogonUser.Roles[i].RoleLDAPId;
               }
            }

            if (strRoleLDAPId == "CSFS015")
            {
               model.IsBranchDirector = "1";
            }
            //20170630 緊急 RQ-2015-019666-018 派件至跨單位 宏祥 add end
            IList<DirectorToApprove> list = _directorBiz.GetDirectorToApproveData(model, pageNum, strSortExpression, strSortDirection, UserId);
            var dtvm = new DirectorToApproveViewModel()
            {
                DirectorToApprove = model,
                DirectorToApprovelist = list,
            };
            //分頁相關設定
            //dtvm.DirectorToApprove.PageSize = _directorBiz.PageSize;
            /// Adam 20180302 ///
            /// 修改主管每頁30筆///
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
            dtvm.DirectorToApprove.LimitDate = model.LimitDate;
            dtvm.DirectorToApprove.SendKind = model.SendKind;
            dtvm.DirectorToApprove.Status = model.Status;

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
            ViewBag.ReturnReasonList = new SelectList(parm.GetCodeData("DIRECTOR_RETURNREASON"), "CodeDesc", "CodeDesc");
            ViewBag.SendKindList = new SelectList(parm.GetCodeData("SendKind"), "CodeDesc", "CodeDesc");        //*發文方式
            LdapEmployeeBiz emp = new LdapEmployeeBiz();
            List<string> leaderList = emp.GetTopEmployeeDirectorIdList();
            ViewBag.isTopDirector = leaderList.Contains(LogonUser.Account);
            //20170630 緊急 RQ-2015-019666-018 派件至跨單位 宏祥 update start
            //ViewBag.AgentUserList = new SelectList(new LdapEmployeeBiz().GetAllEmployeeNoMangerView(), "EmpID", "EmpIdAndName");
            int intRolesCount = LogonUser.Roles.Count;
            string strRoleLDAPId = "";
            for (int i = 0; i < intRolesCount; i++)
            {
               if (LogonUser.Roles[i].RoleLDAPId != "")
               {
                  strRoleLDAPId = LogonUser.Roles[i].RoleLDAPId;
               }
            }

            ViewBag.IsBranchDirector = "0";
            if (strRoleLDAPId == "CSFS015")
            {
               ViewBag.IsBranchDirector = "1";
               IList<PARMCode> list = parm.GetCodeData("CollectionToAgent_AgentDDLDepartment");
               if (list != null && list.Any())
               {
                  foreach (var item in list)
                  {
                     int intcount = _directorBiz.checkAgentDepartment(item.CodeNo, LogonUser.Account);
                     if (intcount == 1)
                     {
                        ViewBag.AgentDepartmentList = new SelectList(parm.GetPARMCodeByCodeType(item.CodeType, item.CodeNo), "CodeNo", "CodeDesc");
                     }
                  }
               }
               ViewBag.AgentDepartment2List = new SelectList(new AgentSettingBIZ().GetDirectorReassignAgentDepartment2View(LogonUser.Account), "SectionName", "SectionName");
               ViewBag.AgentDepartmentUserList = new SelectList(new AgentSettingBIZ().GetDirectorReassignAgentDepartmentUserView(LogonUser.Account), "EmpID", "EmpIdAndName");
            }
            else
            {
               ViewBag.AgentDepartmentList = new SelectList(parm.GetCodeData("CollectionToAgent_AgentDDLDepartment"), "CodeNo", "CodeDesc");
               ViewBag.AgentDepartment2List = new SelectList(new AgentSettingBIZ().GetAgentDepartment2View(), "SectionName", "SectionName");
               ViewBag.AgentDepartmentUserList = new SelectList(new AgentSettingBIZ().GetAgentDepartmentUserView(), "EmpID", "EmpIdAndName");
            }
           //20170630 緊急 RQ-2015-019666-018 派件至跨單位 宏祥 update end
        }
        /// <summary>
        /// 放行
        /// </summary>
        /// <param name="caseIdarr"></param>
        /// <returns></returns>
        public ActionResult FangXing(string statusArr, string caseIdarr)
        {
            string userId = LogonUser.Account;
            string[] caseid = caseIdarr.Split(',');
            string[] status = statusArr.Split(',');
            List<Guid> aryCaseId = (from id in caseid where !string.IsNullOrEmpty(id) select new Guid(id)).ToList();
            List<String> aryStatus = (from s in status where !string.IsNullOrEmpty(s) select s).ToList();
            return Json(_directorBiz.DirectorApprove(aryStatus, aryCaseId, userId));
        }


        /// <summary>
        /// 呈核
        /// </summary>
        /// <returns></returns>
        public ActionResult ChenHe(string statusArr, string caseIdarr)
        {
            string userId = LogonUser.Account;
            string[] caseid = caseIdarr.Split(',');
            string[] status = statusArr.Split(',');
            List<Guid> aryCaseId = (from id in caseid where !string.IsNullOrEmpty(id) select new Guid(id)).ToList();
            List<String> aryStatus = (from s in status where !string.IsNullOrEmpty(s) select s).ToList();
            AgentToHandleBIZ atoHbiz = new AgentToHandleBIZ();
            string agentIdList = atoHbiz.GetManagerID(userId);
            caseid = agentIdList.Split(',');
            List<string> aryAgentId = (from id in caseid where !string.IsNullOrEmpty(id) select id).ToList();

            if (agentIdList == "")//最高主管
                return Json(new JsonReturn() { ReturnCode = "2" });
            else
                return Json(_directorBiz.DirectorSubmit(aryStatus, aryCaseId, aryAgentId, LogonUser.Account));
        }

        //20170421 固定 RQ-2015-019666-017 派件至跨單位 宏祥 add start
        /// <summary>
        /// 收發代辦
        /// </summary>
        /// <param name="caseIdList"></param>
        /// <param name="agentIdList"></param>
        /// <returns></returns>
        public ActionResult AssignSet(DirectorToApprove model, string caseIdList)
        {
           string[] aryId = caseIdList.Split(',');
           List<Guid> aryCaseId = (from id in aryId where !string.IsNullOrEmpty(id) select new Guid(id)).ToList();
           string userId = LogonUser.Account;
           return Json(_directorBiz.AssignSet(model, aryCaseId, userId));
        }

        /// <summary>
        /// 退回
        /// </summary>
        /// <param name="caseIdList"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Return(DirectorToApprove model, string strIds, string statusArr)
        {
            string[] caseid = strIds.Split(',');
            string[] status = statusArr.Split(',');
            List<Guid> aryCaseId = (from id in caseid where !string.IsNullOrEmpty(id) select new Guid(id)).ToList();
            List<String> aryStatus = (from s in status where !string.IsNullOrEmpty(s) select s).ToList();
            string userId = LogonUser.Account;
            return Json(_directorBiz.DirectorReturn(model, aryCaseId, aryStatus, userId));
        }
        //Add by zhangwei 20180315 start
        public ActionResult BatchFangXing(string statusArr, string caseIdarr)
        {
            string userId = LogonUser.Account;
            string[] caseid = caseIdarr.Split(',');
            string[] status = statusArr.Split(',');
            List<Guid> aryCaseId = (from id in caseid where !string.IsNullOrEmpty(id) select new Guid(id)).ToList();
            List<String> aryStatus = (from s in status where !string.IsNullOrEmpty(s) select s).ToList();
            return Json(_directorBiz.DirectorBatchApprove(aryStatus, aryCaseId, userId));
        }
        public ActionResult Remit()
        {
            DirectorToApprove model = new DirectorToApprove();
            HttpCookie cookies = Request.Cookies.Get("QueryCookie");
            if (cookies != null)
            {
                if (cookies.Values["CaseKind"] != null) model.CaseKind = cookies.Values["CaseKind"];
                if (cookies.Values["CaseKind2"] != null) ViewBag.CaseKind2Query = cookies.Values["CaseKind2"];
                //Add by zhangwei 20180315 start
                if (cookies.Values["CaseKind2"] != null) model.CaseKind2 = cookies.Values["CaseKind2"];
                //Add by zhangwei 20180315 end
                if (cookies.Values["GovUnit"] != null) model.GovUnit = cookies.Values["GovUnit"];
                if (cookies.Values["CaseNo"] != null) model.CaseNo = cookies.Values["CaseNo"];
                if (cookies.Values["GovDateS"] != null) model.GovDateS = cookies.Values["GovDateS"];
                if (cookies.Values["GovDateE"] != null) model.GovDateE = cookies.Values["GovDateE"];
                if (cookies.Values["Speed"] != null) model.Speed = cookies.Values["Speed"];
                if (cookies.Values["ReceiveKind"] != null) model.ReceiveKind = cookies.Values["ReceiveKind"];
                if (cookies.Values["GovNo"] != null) model.GovNo = cookies.Values["GovNo"];
                if (cookies.Values["CreatedDateS"] != null) model.CreatedDateS = cookies.Values["CreatedDateS"];
                if (cookies.Values["CreatedDateE"] != null) model.CreatedDateE = cookies.Values["CreatedDateE"];
                if (cookies.Values["Unit"] != null) model.Unit = cookies.Values["Unit"];
                if (cookies.Values["CreateUser"] != null) model.CreateUser = cookies.Values["CreateUser"];
                if (cookies.Values["SendDateS"] != null) model.SendDateS = cookies.Values["SendDateS"];
                if (cookies.Values["SendDateE"] != null) model.SendDateE = cookies.Values["SendDateE"];
                //20170630 緊急 RQ-2015-019666-018 派件至跨單位 宏祥 update start
                //if (cookies.Values["AgentUser"] != null) model.AgentUser = cookies.Values["AgentUser"];
                if (cookies.Values["AgentDepartment"] != null) model.AgentDepartment = cookies.Values["AgentDepartment"];
                //if (cookies.Values["AgentDepartment2"] != null) model.AgentDepartment2 = cookies.Values["AgentDepartment2"];
                //if (cookies.Values["AgentDepartmentUser"] != null) model.AgentDepartmentUser = cookies.Values["AgentDepartmentUser"];
                //20171103 RC RQ-2015-019666-003 系統功能修正 宏祥 update start
                if (cookies.Values["AgentDepartment2"] != null) ViewBag.AgentDepartment2Query = cookies.Values["AgentDepartment2"];
                if (cookies.Values["AgentDepartmentUser"] != null) ViewBag.AgentDepartmentUserQuery = cookies.Values["AgentDepartmentUser"];
                //20171103 RC RQ-2015-019666-003 系統功能修正 宏祥 update end
                //20170630 緊急 RQ-2015-019666-018 派件至跨單位 宏祥 update end
                if (cookies.Values["SendKind"] != null) model.SendKind = cookies.Values["SendKind"];
                if (cookies.Values["Status"] != null) model.Status = cookies.Values["Status"];
            }
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
            if (!string.IsNullOrEmpty(model.SendDateS))
            {
                model.SendDateS = UtlString.FormatDateTwStringToAd(model.SendDateS);
            }
            if (!string.IsNullOrEmpty(model.SendDateE))
            {
                model.SendDateE = UtlString.FormatDateTwStringToAd(model.SendDateE);
            }
            string UserId = LogonUser.Account;
            AutoAuditAssignmentsBIZ aotuAssignBiz = new AutoAuditAssignmentsBIZ();
            MemoryStream ms = null;
            if (model.CaseKind2 != "扣押" && model.CaseKind2 != "撤銷"&& model.CaseKind2 != "扣押並支付")
            {
                string[] headerColumns = new[]
                    {
                        "案件編號",
                        "來文機關",
                        "來文日期",
                        "來文字號",
                        "經辦人員",
                        "發文日期",
                        "類別",
                        "細分類",
                        "速別",
                        "經辦限辦日期"
                    };
                ms = aotuAssignBiz.Excel(headerColumns, model, UserId); 
            }
            else
            {
                if (model.CaseKind2 == "扣押"||model.CaseKind2 == "扣押並支付" )
                {
                    string[] headerColumns = new[]
                    {
                        "細分類",
                        "案件編號",
                        "義務人",
                        "法定代理人",
                        "身份證統一編號",
                        "營利事業編號",
                        "來函扣押總金額",
                        "存款帳號",
                        "幣別",
                        "扣押金額",
                        "扣完的可用餘額",
                        "呈核經辦",
                        "鍵檔日期",
                        "備註"
                    };
                    ms = aotuAssignBiz.ExcelForSeizure(headerColumns, model, UserId,"0");
                }
                else//撤銷
                {
                    string[] headerColumns = new[]
                   {
                        "細分類",
                        "案件編號",
                        "存款帳號",
                        "幣別",
                        "扣押命令發文字號",
                        "扣押金額",
                        "已撤銷金額",
                        "呈核經辦",
                        "鍵檔日期",
                        "備註"
                    };

                    ms = aotuAssignBiz.ExcelForUndo(headerColumns, model, UserId,"0");
                }
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
            var fileName = "待核決" + "_" + Lang.csfs_debtexcel + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xls";
            return File(ms.ToArray(), "application/vnd.ms-excel", fileName);
        }
        //Add by zhangwei 20180315 end
    }
}