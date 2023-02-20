using CTBC.CSFS.Pattern;
using System;
using System.Collections.Generic;
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

namespace CTBC.CSFS.Areas.Director.Controllers
{
    public class DirectorReassignController : AppController
    {
        DirectorReassignBIZ DRbiz = new DirectorReassignBIZ();
        //CaseQueryBIZ CQBIZ = new CaseQueryBIZ();
        PARMCodeBIZ parm = new PARMCodeBIZ();
        ////20170421 固定 RQ-2015-019666-017 派件至跨單位 宏祥 add start
        //CollectionToAgentBIZ CTBZ = new CollectionToAgentBIZ();
        ////20170421 固定 RQ-2015-019666-017 派件至跨單位 宏祥 add end
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
                    if (cookies.Values["CaseKind"] != null) model.CaseKind = cookies.Values["CaseKind"];
                    if (cookies.Values["CaseKind2"] != null) ViewBag.CaseKind2Query = cookies.Values["CaseKind2"];
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
                    //20170421 固定 RQ-2015-019666-017 派件至跨單位 宏祥 update start
                    //if (cookies.Values["AgentUser"] != null) model.AgentUser = cookies.Values["AgentUser"];
                    if (cookies.Values["AgentDepartment"] != null) model.AgentDepartment = cookies.Values["AgentDepartment"];
                    //if (cookies.Values["AgentDepartment2"] != null) model.AgentDepartment2 = cookies.Values["AgentDepartment2"];
                    //if (cookies.Values["AgentDepartmentUser"] != null) model.AgentDepartmentUser = cookies.Values["AgentDepartmentUser"];
                    //20171103 RC RQ-2015-019666-003 系統功能修正 宏祥 update end
                    if (cookies.Values["AgentDepartment2"] != null) ViewBag.AgentDepartment2Query = cookies.Values["AgentDepartment2"];
                    if (cookies.Values["AgentDepartmentUser"] != null) ViewBag.AgentDepartmentUserQuery = cookies.Values["AgentDepartmentUser"];
                    //20171103 RC RQ-2015-019666-003 系統功能修正 宏祥 update end
                    //20170421 固定 RQ-2015-019666-017 派件至跨單位 宏祥 update end
                    if (cookies.Values["ObligorNo"] != null) model.ObligorNo = cookies.Values["ObligorNo"];
                    if (cookies.Values["ObligorName"] != null) model.ObligorName = cookies.Values["ObligorName"];
                    if (cookies.Values["SendNo"] != null) model.SendNo = cookies.Values["SendNo"];
                    if (cookies.Values["OverDateS"] != null) model.OverDateS = cookies.Values["OverDateS"];
                    if (cookies.Values["OverDateE"] != null) model.OverDateE = cookies.Values["OverDateE"];
                    if (cookies.Values["Status"] != null) model.Status = cookies.Values["Status"];
					if (cookies.Values["SendKind"] != null) model.SendKind = cookies.Values["SendKind"];
                    ViewBag.CurrentPage = cookies.Values["CurrentPage"];
                    ViewBag.isQuery = "1";
                }
            }

            ViewBag.CaseKindList = new SelectList(parm.GetCodeData("CASE_KIND"), "CodeDesc", "CodeDesc");
            InitDropdownListOptions();
            return View(model);
        }
        [HttpPost]
        public ActionResult _QueryResult(CaseQuery model, int pageNum = 1, string strSortExpression = "CaseId", string strSortDirection = "asc")
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
            //20170421 固定 RQ-2015-019666-017 派件至跨單位 宏祥 update start
            //modelCookie.Values.Add("AgentUser", model.AgentUser);
            modelCookie.Values.Add("AgentDepartment", model.AgentDepartment);
            modelCookie.Values.Add("AgentDepartment2", model.AgentDepartment2);
            modelCookie.Values.Add("AgentDepartmentUser", model.AgentDepartmentUser);
            //20170421 固定 RQ-2015-019666-017 派件至跨單位 宏祥 update end
            modelCookie.Values.Add("CreateUser", model.CreateUser);
            modelCookie.Values.Add("Unit", model.Unit);
            modelCookie.Values.Add("SendDateS", model.SendDateS);
            modelCookie.Values.Add("SendDateE", model.SendDateE);
            modelCookie.Values.Add("ObligorNo", model.ObligorNo);
            modelCookie.Values.Add("ObligorName", model.ObligorName);
            modelCookie.Values.Add("SendNo", model.SendNo);
            modelCookie.Values.Add("OverDateS", model.OverDateS);
            modelCookie.Values.Add("OverDateE", model.OverDateE);
            modelCookie.Values.Add("Status", model.Status);
            modelCookie.Values.Add("SendKind", model.SendKind);

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
            //20170421 固定 RQ-2015-019666-017 派件至跨單位 宏祥 add start
            //判斷是否為分行主管角色，增加主管改派案件搜尋條件
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
            //20170421 固定 RQ-2015-019666-017 派件至跨單位 宏祥 add end
            IList<CaseQuery> list = DRbiz.GetData(model, pageNum, strSortExpression, strSortDirection, UserId, ref where);
            var dtvm = new CaseQueryViewModel()
            {
                CaseQuery = model,
                CaseQueryList = list,
            };
            //分頁相關設定
            dtvm.CaseQuery.PageSize = DRbiz.PageSize;
            dtvm.CaseQuery.CurrentPage = DRbiz.PageIndex;
            dtvm.CaseQuery.TotalItemCount = DRbiz.DataRecords;
            dtvm.CaseQuery.SortExpression = strSortExpression;
            dtvm.CaseQuery.SortDirection = strSortDirection;

            dtvm.CaseQuery.CaseKind = model.CaseKind;
            dtvm.CaseQuery.CaseKind2 = model.CaseKind2;
            dtvm.CaseQuery.GovKind = model.GovKind;
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
            dtvm.CaseQuery.Speed = model.Speed;
            dtvm.CaseQuery.OverDateS = model.OverDateS;
            dtvm.CaseQuery.OverDateE = model.OverDateE;
            dtvm.CaseQuery.Status = model.Status;
            dtvm.CaseQuery.AgentUser = model.AgentUser;
            dtvm.CaseQuery.SendKind = model.SendKind;
            

            return PartialView("_QueryResult", dtvm);
        }
        public void InitDropdownListOptions()
        {
            ViewBag.StatusName = new SelectList(DRbiz.GetStatusName("STATUS_NAME"), "CodeNo", "CodeDesc");
            //GetStatusName
            ViewBag.CASE_END_TIME = parm.GetCASE_END_TIME("CASE_END_TIME");                                     //* 每天最晚時間
            ViewBag.GOV_KINDList = new SelectList(parm.GetCodeData("GOV_KIND"), "CodeDesc", "CodeDesc");        //* 來文機關類型
            ViewBag.SpeedList = new SelectList(parm.GetCodeData("INCOME_SPEED"), "CodeDesc", "CodeDesc");       //* 速別
            ViewBag.ReceiveKindList = new SelectList(parm.GetCodeData("INCOME_TYPE"), "CodeDesc", "CodeDesc");  //* 來文方式
            ViewBag.CaseKindList = new SelectList(parm.GetCodeData("CASE_KIND"), "CodeDesc", "CodeDesc");
            ViewBag.CaseKind2List = new SelectList(DRbiz.GetCodeData("CASE_SEIZURE"), "CodeDesc", "CodeDesc");
            //20170421 固定 RQ-2015-019666-017 派件至跨單位 宏祥 update start
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
		               int intcount = DRbiz.checkAgentDepartment(item.CodeNo,LogonUser.Account);
                     if (intcount == 1)
	                  {
		                  ViewBag.AgentDepartmentList = new SelectList(parm.GetPARMCodeByCodeType(item.CodeType,item.CodeNo),"CodeNo", "CodeDesc");
                        //IList<PARMCode> list2 = parm.GetPARMCodeByCodeType(item.CodeType, item.CodeNo);
                        //var obj = list2.FirstOrDefault();
                        //ViewBag.CodeDesc = obj.CodeDesc;
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
            //20170421 固定 RQ-2015-019666-017 派件至跨單位 宏祥 update end
            ViewBag.SendKindList = new SelectList(parm.GetCodeData("SendKind"), "CodeDesc", "CodeDesc");        //*發文方式

            LdapEmployeeBiz empBiz = new LdapEmployeeBiz();//GetAgentAndBu
            AgentSettingBIZ ASBIZ = new AgentSettingBIZ();

            
            //20170421 固定 RQ-2015-019666-017 派件至跨單位 宏祥 update start
            //ViewBag.Department = new SelectList(ASBIZ.GetKeBie(), "Department", "Department");
            if (strRoleLDAPId == "CSFS015") 
            {
               string strAgentList = JsonHelper.ObjectToJson(ASBIZ.GetDirectorReassignAgentDepartmentUserView(LogonUser.Account));
               ViewBag.AgentList = strAgentList;
               ViewBag.Department = new SelectList(ASBIZ.GetDirectorReassignAgentDepartment2View(LogonUser.Account), "SectionName", "SectionName");
            }
            else
            {
               string strAgentList = JsonHelper.ObjectToJson(empBiz.GetAgentAndBuInfosByBuId(""));
               ViewBag.AgentList = strAgentList;
               ViewBag.Department = new SelectList(ASBIZ.GetAgentSettingDepartment(), "Department", "Department");
            }            
            //20170421 固定 RQ-2015-019666-017 派件至跨單位 宏祥 update end
        }

        public ActionResult AssignAgent(string caseIdList, string agentIdList)
        {
            string[] aryId = caseIdList.Split(',');
            List<Guid> aryCaseId = (from id in aryId where !string.IsNullOrEmpty(id) select new Guid(id)).ToList();
            aryId = agentIdList.Split(',');
            List<string> aryAgentId = (from id in aryId where !string.IsNullOrEmpty(id) select id).ToList();
            string userId = LogonUser.Account;
            return Json(DRbiz.DirectReassign(aryCaseId, aryAgentId, userId));
        }

        ////20170421 固定 RQ-2015-019666-017 派件至跨單位 宏祥 add start
        ///// <summary>
        ///// 選擇經辦人員下拉選單(處)
        ///// </summary>
        ///// <param name="AgentDepartment"></param>
        ///// <returns></returns>
        //public JsonResult ChangAgentDepartment1(string AgentDepartment)
        //{
        //   List<KeyValuePair<string, string>> items = new List<KeyValuePair<string, string>>();
        //   if (string.IsNullOrEmpty(AgentDepartment)) return Json(items);
        //   var list = CTBZ.GetAgentDepartment2View(AgentDepartment);
        //   if (list.Any())
        //   {
        //      items.AddRange(list.Select(AgentDepartment2 => new KeyValuePair<string, string>(AgentDepartment2.DepartmentID, AgentDepartment2.SectionName)));
        //   }
        //   return Json(items);
        //}
        ///// <summary>
        ///// 選擇經辦人員下拉選單(科組)
        ///// </summary>
        ///// <param name="AgentDepartment"></param>
        ///// <returns></returns>
        //public JsonResult ChangAgentDepartment2(string AgentDepartment)
        //{
        //   List<KeyValuePair<string, string>> items = new List<KeyValuePair<string, string>>();
        //   if (string.IsNullOrEmpty(AgentDepartment)) return Json(items);
        //   var list = CTBZ.GetAgentDepartmentUserView(AgentDepartment);
        //   if (list.Any())
        //   {
        //      items.AddRange(list.Select(AgentDepartmentUser => new KeyValuePair<string, string>(AgentDepartmentUser.EmpId, AgentDepartmentUser.EmpIdAndName)));
        //   }
        //   return Json(items);
        //}
        ////20170421 固定 RQ-2015-019666-017 派件至跨單位 宏祥 add start
    }
}