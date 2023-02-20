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
    public class CaseQueryController : AppController
    {
        CaseQueryBIZ CQBIZ = new CaseQueryBIZ();
        PARMCodeBIZ parm = new PARMCodeBIZ();
        static string where = string.Empty;

        // GET: QueryAndExport/CaseQuery
        [RootPageFilter]
        public ActionResult Index(string isBack)
        {
            CaseQuery model = new CaseQuery();
            if (isBack == "1")
            {
                HttpCookie cookies = Request.Cookies.Get("QueryCookie");
                if (cookies != null)
                {
                    if (cookies.Values["GovUnit"] != null) model.GovUnit = cookies.Values["GovUnit"];
                    if (cookies.Values["GovNo"] != null) model.GovNo = cookies.Values["GovNo"];
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
                    //Add by zhangwei 20180315 start
                    if (cookies.Values["RMType"] != null) model.RMType = cookies.Values["RMType"];
                    //Add by zhangwei 20180315 end
                    ViewBag.CurrentPage = cookies.Values["CurrentPage"];
                    ViewBag.isQuery = "1";
                }
            }

            //*20150811新需求, 分行角色只能看自己分行資料且不能修改
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
        public ActionResult _QueryResult(CaseQuery model, int pageNum = 1, string strSortExpression = "CaseNo", string strSortDirection = "asc")
        {
            #region Cookie
            HttpCookie modelCookie = new HttpCookie("QueryCookie");
            modelCookie.Values.Add("GovUnit", model.GovUnit);
            modelCookie.Values.Add("GovNo", model.GovNo);
            //20170630 緊急 RQ-2015-019666-018 派件至跨單位 宏祥 update start
            //modelCookie.Values.Add("AgentUser", model.AgentUser);
            modelCookie.Values.Add("AgentDepartment", model.AgentDepartment);
            modelCookie.Values.Add("AgentDepartment2", model.AgentDepartment2);
            modelCookie.Values.Add("AgentDepartmentUser", model.AgentDepartmentUser);
            //20170630 緊急 RQ-2015-019666-018 派件至跨單位 宏祥 update end
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
            //Add by zhangwei 20180315 start
            modelCookie.Values.Add("RMType", model.RMType);
            //Add by zhangwei 20180315 end
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
            IList<CaseQuery> list = CQBIZ.GetData(model, pageNum, strSortExpression, strSortDirection, UserId, ref where);
            var dtvm = new CaseQueryViewModel()
            {
                CaseQuery = model,
                CaseQueryList = list,
            };

            Session["where"] = model;

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
            dtvm.CaseQuery.SendKind = model.SendKind;

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
            //20170630 緊急 RQ-2015-019666-018 派件至跨單位 宏祥 update start
            //ViewBag.AgentUserList = new SelectList(new LdapEmployeeBiz().GetAllEmployeeInEmployeeView(), "EmpID", "EmpIdAndName");
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
            //20170630 緊急 RQ-2015-019666-018 派件至跨單位 宏祥 update end
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
        public ActionResult BuildSimpleExcel(CaseQuery model)
        {
            MemoryStream ms = new MemoryStream();
            string fileName = string.Empty;
            if (model.hiddenVal == "0")
            {
                CaseQuery CaseQuer = Session["where"] as CaseQuery;
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
                fileName = Lang.csfs_menu_tit_casequery + "_" + Lang.csfs_debtexcel + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xls";
                //Session["where"] = null;
            }
            else
            {
                //20171103 RC RQ-2015-019666-003 系統功能修正 宏祥 update start
                //ms = CQBIZ.Exportlist(model.CaseIdarr, model.CreatedDateS, model.CreatedDateE);
                LdapEmployeeBiz empBiz = new LdapEmployeeBiz();
                IList<PARMCode> listCode = empBiz.GetCodeData("Department");
                ms = CQBIZ.Exportlist(model.CaseIdarr, model.CreatedDateS, model.CreatedDateE, listCode);
                //20171103 RC RQ-2015-019666-003 系統功能修正 宏祥 update end
                fileName = Lang.csfs_menu_tit_casequery + "_" + Lang.csfs_export_filelist + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xls";
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