using CTBC.CSFS.Pattern;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CTBC.CSFS.BussinessLogic;
using CTBC.CSFS.Models;
using System.IO;
using CTBC.CSFS.Filter;
using CTBC.FrameWork.Util;
using CTBC.CSFS.ViewModels;
using CTBC.CSFS.Resource;
using CTBC.FrameWork.Platform;

namespace CTBC.CSFS.Areas.QueryAndExport.Controllers
{
    public class HistoryQueryController : AppController
    {
        HistoryQueryBIZ CQBIZ = new HistoryQueryBIZ();
        PARMCodeBIZ parm = new PARMCodeBIZ();
        static string where = string.Empty;

        // GET: QueryAndExport/HistoryQuery
        [RootPageFilter]
        public ActionResult Index(string isBack)
        {
            HistoryQuery model = new HistoryQuery();
            if (isBack == "1")
            {
                HttpCookie cookies = Request.Cookies.Get("QueryCookie");
                if (cookies != null)
                {
                    if (cookies.Values["GovUnit"] != null) model.GovUnit = cookies.Values["GovUnit"];
                    if (cookies.Values["GovNo"] != null) model.GovNo = cookies.Values["GovNo"];
                    if (cookies.Values["AgentDepartment"] != null) model.AgentDepartment = cookies.Values["AgentDepartment"];
                    if (cookies.Values["AgentDepartment2"] != null) ViewBag.AgentDepartment2Query = cookies.Values["AgentDepartment2"];
                    if (cookies.Values["AgentDepartmentUser"] != null) ViewBag.AgentDepartmentUserQuery = cookies.Values["AgentDepartmentUser"];
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
                    if (cookies.Values["SendKind"] != null) model.SendKind = cookies.Values["SendKind"];
                    if (cookies.Values["RMType"] != null) model.RMType = cookies.Values["RMType"];
                    ViewBag.CurrentPage = cookies.Values["CurrentPage"];
                    ViewBag.isQuery = "1";
                }
            }

            //*分行角色只能看自己分行資料且不能修改
            LdapEmployeeBiz empBiz = new LdapEmployeeBiz();
            IList<LDAPEmployee> list = empBiz.GetNoRoleEmployeeIdList();
            IList<LDAPEmployee> list2 = empBiz.GetNoRoleEmployeeIdList2();
            if ((list != null && list.Any(m => m.EmpId.Trim().ToUpper() == LogonUser.Account.Trim().ToUpper()))
                || (list2 != null && list2.Any(m => m.EmpId.Trim().ToUpper() == LogonUser.Account.Trim().ToUpper())))
            {
                //* 屬於分行角色
                model.Unit = new LdapEmployeeBiz().GetBranchId();
                ViewBag.UnitRead = "1";
            }

            ViewBag.CaseKindList = new SelectList(parm.GetCodeData("CASE_KIND"), "CodeDesc", "CodeDesc");
            InitDropdownListOptions();
            return View(model);
        }
        [HttpPost]
        public ActionResult _QueryResult(HistoryQuery model, int pageNum = 1, string strSortExpression = "CaseNo", string strSortDirection = "asc")
        {
            #region Cookie
            HttpCookie modelCookie = new HttpCookie("QueryCookie");
            modelCookie.Values.Add("GovUnit", model.GovUnit);
            modelCookie.Values.Add("GovNo", model.GovNo);
            modelCookie.Values.Add("AgentDepartment", model.AgentDepartment);
            modelCookie.Values.Add("AgentDepartment2", model.AgentDepartment2);
            modelCookie.Values.Add("AgentDepartmentUser", model.AgentDepartmentUser);
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
            modelCookie.Values.Add("SendKind", model.SendKind);
            modelCookie.Values.Add("RMType", model.RMType);
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
            IList<HistoryQuery> list = CQBIZ.GetData(model, pageNum, strSortExpression, strSortDirection, UserId, ref where);
            var dtvm = new HistoryQueryViewModel()
            {
                HistoryQuery = model,
                HistoryQueryList = list,
            };

            Session["where"] = model;

            //分頁相關設定
            dtvm.HistoryQuery.PageSize = CQBIZ.PageSize;
            dtvm.HistoryQuery.CurrentPage = CQBIZ.PageIndex;
            dtvm.HistoryQuery.TotalItemCount = CQBIZ.DataRecords;
            dtvm.HistoryQuery.SortExpression = strSortExpression;
            dtvm.HistoryQuery.SortDirection = strSortDirection;

            dtvm.HistoryQuery.CaseKind = model.CaseKind;
            dtvm.HistoryQuery.CaseKind2 = model.CaseKind2;
            dtvm.HistoryQuery.GovUnit = model.GovUnit;
            dtvm.HistoryQuery.CaseNo = model.CaseNo;
            dtvm.HistoryQuery.GovDateS = model.GovDateS;
            dtvm.HistoryQuery.GovDateE = model.GovDateE;
            dtvm.HistoryQuery.Speed = model.Speed;
            dtvm.HistoryQuery.ReceiveKind = model.ReceiveKind;
            dtvm.HistoryQuery.GovNo = model.GovNo;
            dtvm.HistoryQuery.CreatedDateS = model.CreatedDateS;
            dtvm.HistoryQuery.CreatedDateE = model.CreatedDateE;
            dtvm.HistoryQuery.Unit = model.Unit;
            dtvm.HistoryQuery.CreateUser = model.CreateUser;
            dtvm.HistoryQuery.ObligorNo = model.ObligorNo;
            dtvm.HistoryQuery.ObligorName = model.ObligorName;
            dtvm.HistoryQuery.SendDateS = model.SendDateS;
            dtvm.HistoryQuery.SendDateE = model.SendDateE;
            dtvm.HistoryQuery.SendNo = model.SendNo;
            dtvm.HistoryQuery.OverDateS = model.OverDateS;
            dtvm.HistoryQuery.OverDateE = model.OverDateE;
            dtvm.HistoryQuery.Status = model.Status;
            dtvm.HistoryQuery.AgentUser = model.AgentUser;
            dtvm.HistoryQuery.SendKind = model.SendKind;

            return PartialView("_QueryResult", dtvm);
        }
        public void InitDropdownListOptions()
        {
            ViewBag.StatusName = new SelectList(CQBIZ.GetStatusName("STATUS_NAME"), "CodeNo", "CodeDesc");
            //GetStatusName
            ViewBag.CASE_END_TIME = parm.GetCASE_END_TIME("CASE_END_TIME");                                     //* 每天最晚時間
            ViewBag.GOV_KINDList = new SelectList(parm.GetCodeData("GOV_KIND"), "CodeDesc", "CodeDesc");        //* 來文機關類型
            ViewBag.SpeedList = new SelectList(parm.GetCodeData("INCOME_SPEED"), "CodeDesc", "CodeDesc");       //* 速別
            ViewBag.ReceiveKindList = new SelectList(parm.GetCodeData("INCOME_TYPE"), "CodeDesc", "CodeDesc");  //* 來文方式
            ViewBag.CaseKindList = new SelectList(parm.GetCodeData("CASE_KIND"), "CodeDesc", "CodeDesc");
            ViewBag.CaseKind2List = new SelectList(CQBIZ.GetCodeData("CASE_SEIZURE"), "CodeDesc", "CodeDesc");
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
               IList<PARMCode> listparm = parm.GetCodeData("CollectionToAgent_AgentDDLDepartment");
               if (listparm != null && listparm.Any())
               {
                  foreach (var item in listparm)
                  {
                     int intcount = CQBIZ.checkAgentDepartment(item.CodeNo, LogonUser.Account);
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
            ViewBag.SendKindList = new SelectList(parm.GetCodeData("SendKind"), "CodeDesc", "CodeDesc");  //* 發文方式

            IList<PARMCode> list = parm.GetCodeData("ExportRoleList");
            if (list != null && list.Any())
            {
                string[] ary = { };
                var obj = list.FirstOrDefault();
                if (obj != null && !string.IsNullOrEmpty(obj.CodeMemo))
                {
                    ary = obj.CodeMemo.Split(';');
                }
                if (ary.Length > 0 && LogonUser.Roles.Any() && LogonUser.Roles.Any(m => ary.Contains(m.RoleLDAPId)))
                {
                    ViewBag.CanExport = "1";
                }
                //ViewBag.CanExport = "1";
            }
            //Add by zhangwei 20180315 start
            List<SelectListItem> listRMType = new List<SelectListItem>
            {
                new SelectListItem() {Text ="-請選擇-", Value = "0"},
                new SelectListItem() {Text ="是", Value = "1"},
                new SelectListItem() {Text ="否", Value = "2"},
            };
            ViewBag.RMTypeList = listRMType;
            //Add by zhangwei 20180315 end
        }
        [HttpPost]
        public ActionResult BuildSimpleExcel(HistoryQuery model)
        {
            MemoryStream ms = new MemoryStream();
            string fileName = string.Empty;
            if (model.hiddenVal == "0")
            {
                HistoryQuery CaseQuer = Session["where"] as HistoryQuery;
                //if (CaseQuer.OverDateE != null)
                //{
                //    CaseQuer.OverDateE = Convert.ToDateTime(UtlString.FormatDateTwStringToAd(CaseQuer.OverDateE)).AddDays(-1).ToString("yyyy/MM/dd");
                //}
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
                ms = CQBIZ.Excel(CaseQuer);
                fileName = Lang.csfs_menu_History_Query + "_" + Lang.csfs_debtexcel + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xls";
                //Session["where"] = null;
            }
            else
            {
                LdapEmployeeBiz empBiz = new LdapEmployeeBiz();
                IList<PARMCode> listCode = empBiz.GetCodeData("Department");
                ms = CQBIZ.Exportlist(model.CaseIdarr, model.CreatedDateS, model.CreatedDateE, listCode);
                fileName = Lang.csfs_menu_History_Query + "_" + Lang.csfs_export_filelist + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xls";
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