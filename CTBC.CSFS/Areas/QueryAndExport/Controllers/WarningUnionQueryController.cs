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
using NPOI.HSSF.UserModel;

namespace CTBC.CSFS.Areas.QueryAndExport.Controllers
{
    public class WarningUnionQueryController : AppController
    {
        DirectorToApproveBIZ _directorBiz;
        CommonBIZ commonBIZ = new CommonBIZ();
        PARMCodeBIZ parm = new PARMCodeBIZ();
        CustomerInfoBIZ CIBZ = new CustomerInfoBIZ();
        WarningUnionQueryBIZ wqBiz = new WarningUnionQueryBIZ();
        // GET: /QueryAndExport/WarningUnionQuery/
        [RootPageFilter]
        public ActionResult Index()
        {
            LdapEmployeeBiz empBiz = new LdapEmployeeBiz();
            WarningUnionQuery model = new WarningUnionQuery();
            //model.ForCDateS = UtlString.FormatDateTw(DateTime.Today.ToString("yyyy/MM/dd"));
            //model.ForCDateE = UtlString.FormatDateTw(DateTime.Today.ToString("yyyy/MM/dd"));

            IList<PARMCode> list = wqBiz.GetCodeData("ExportRoleList");
            if (list != null && list.Any())
            {
                string[] ary = { };
                var obj = list.FirstOrDefault();
                if (obj != null && !string.IsNullOrEmpty(obj.CodeMemo))
                {
                    ary = obj.CodeMemo.Split(';');
                }
                if (ary.Length > 0 && LogonUser.Roles.Any() && LogonUser.Roles.Any(m => ary.Contains(m.RoleLDAPId)))
                    ViewBag.CanExport = "1";
            }
            //20170421 固定 RQ-2015-019666-017 派件至跨單位 宏祥 add start
            string strAgentList = JsonHelper.ObjectToJson(empBiz.GetAgentAndBuInfosByBuId(""));
            ViewBag.AgentList = strAgentList;
            string struserId = LogonUser.Account;
            string strBranchName = LogonUser.BranchName;
            string strSectionName = "";
            int intFlag = 0;
            AgentSettingBIZ ASBIZ = new AgentSettingBIZ();
            ViewBag.Unit = new SelectList(ASBIZ.GetAgentSettingDepartment(strSectionName, strBranchName, intFlag), "Department", "Department");
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
            BindDDl();
            return View(model);
        }
        public void BindDDl()
        {
            List<SelectListItem> Setlist = new List<SelectListItem>
            {
                new SelectListItem() {Text ="-請選擇-", Value = ""},
                new SelectListItem() {Text ="Y", Value = "Y"},
                new SelectListItem() {Text ="N", Value = "N"},
            };
            ViewBag.SetList = Setlist;
            List<SelectListItem> list = new List<SelectListItem>
            {
                new SelectListItem() {Text ="-請選擇-", Value = ""},
                new SelectListItem() {Text ="新增", Value = "1"},
                new SelectListItem() {Text ="刪除", Value = "5"},
                new SelectListItem() {Text ="修改", Value = "4"},
            };
            ViewBag.TypeList = list;
            List<SelectListItem> YNlist = new List<SelectListItem>
            {
                new SelectListItem() {Text ="-請選擇-", Value = ""},
                new SelectListItem() {Text ="Y", Value = "Y"},
                new SelectListItem() {Text ="N", Value = "N"},
            };
            ViewBag.YNlist = YNlist;
            List<SelectListItem> COL_SOURCEList = new List<SelectListItem>
            {
                new SelectListItem() {Text ="-請選擇-", Value = ""},
                new SelectListItem() {Text ="他行", Value = "他行"},
                new SelectListItem() {Text ="165", Value = "165"},
            };
            ViewBag.COL_SOURCEList = COL_SOURCEList;
            List<SelectListItem> Kindlist = new List<SelectListItem>
            {
                new SelectListItem() {Text ="-請選擇-", Value = ""},
                new SelectListItem() {Text ="聯防", Value = "聯防"},
                new SelectListItem() {Text ="165預警", Value = "165預警"},
            };
            ViewBag.KindList = Kindlist;
            ViewBag.BankID = new SelectList(commonBIZ.GetCodeData("RCAF_BRANCH"), "CodeNo", "CodeNo");
        }
        /// <summary>
        /// 結果列表
        /// </summary>
        public ActionResult _QueryResult(WarningUnionQuery model)
        {
            return PartialView("_QueryResult", SearchList(model));
        }
        public void BindDropDownList()
        {
            ViewBag.NotificationContentList = new SelectList(parm.GetCodeData("NotificationContent"), "CodeDesc", "CodeDesc");//*通報內容
            ViewBag.NotificationSourceList = new SelectList(parm.GetCodeData("NotificationSource"), "CodeDesc", "CodeDesc", "電話詐財");//*通報來源

            ViewBag.NotificationUnitList = new SelectList(parm.GetCodeData("NotificationUnit"), "CodeDesc", "CodeDesc");//*通報單位
            List<int> delItem = new List<int>() { 0, 2, 4 };
            var ddd = parm.GetCodeData("CASE_WARNING").Where(x => !delItem.Contains(x.SortOrder));

            ViewBag.KindList = new SelectList(ddd, "CodeDesc", "CodeDesc");//*通報單位

        }
        public ActionResult CreateWarn(string COL_C1003CASE, string Currency)
        {
            BindDropDownList();
            WarningUnionQuery model = new WarningUnionQuery();
            model.CreatedDate = UtlString.FormatDateTw(DateTime.Now.ToString("yyyy/MM/dd"));
            //model.HappenDateTimeForHour = DateTime.Now.ToString("HH:mm");
            model.COL_CSIBLOCKDATE2 = UtlString.FormatDateTw(DateTime.Now.ToString("yyyy/MM/dd"));
            model.COL_CSIBLOCKTIME2 = DateTime.Now.ToString("HH:mm");
            //model.ExtendDate = UtlString.FormatDateTw(DateTime.Now.ToString("yyyy/MM/dd"));
            //model.ExtendDateHour = DateTime.Now.ToString("HH:mm");
            //model.No_e = "";
            //model.ForCDate = UtlString.FormatDateTw(DateTime.Now.ToString("yyyy/MM/dd"));
            var WM = wqBiz.GetWarningUnionQueryListByDocNo(COL_C1003CASE);
            //model.CaseId = WM.CaseId;
            return View(model);
        }
        public ActionResult EditWarn(string SerialID)
        {
            CaseWarningBIZ warn = new CaseWarningBIZ();
            WarningDetails model = warn.GetWarnDetailBySerialID(SerialID);
            string strTime = model.EtabsDatetime;
            string strExtend = model.ExtendDate == null ? "" : model.ExtendDate.ToString();
            string strHappen = model.HappenDateTime;
            string strReleaseDate = model.ReleaseDate == null ? "" : model.ReleaseDate.ToString();

            model.EtabsDatetime = strTime.Trim() == "" ? null : UtlString.FormatDateTw(Convert.ToDateTime(strTime).ToString("yyyy/MM/dd"));
            model.EtabsDatetimeHour = strTime.Trim() == "" ? null : Convert.ToDateTime(strTime).ToString("HH:mm");
            model.ExtendDate = strExtend.Trim() == "" ? null : UtlString.FormatDateTw(Convert.ToDateTime(strExtend).ToString("yyyy/MM/dd"));
            model.ExtendDateHour = strExtend.Trim() == "" ? null : Convert.ToDateTime(strExtend).ToString("HH:mm");
            model.ReleaseDate = strReleaseDate.Trim() == "" ? null : UtlString.FormatDateTw(Convert.ToDateTime(strReleaseDate).ToString("yyyy/MM/dd"));
            model.ReleaseDateForHour = strReleaseDate.Trim() == "" ? null : Convert.ToDateTime(strReleaseDate).ToString("HH:mm");
            model.HappenDateTime = strHappen.Trim() == "" ? null : UtlString.FormatDateTw(Convert.ToDateTime(strHappen).ToString("yyyy/MM/dd"));
            model.HappenDateTimeForHour = strHappen.Trim() == "" ? null : Convert.ToDateTime(strHappen).ToString("HH:mm");
            model.ForCDate = UtlString.FormatDateTw(model.ForCDate);
            List<int> delItem = new List<int>() { 0, 4 };
            var ddd = parm.GetCodeData("CASE_WARNING").Where(x => !delItem.Contains(x.SortOrder));

            ViewBag.KindList = new SelectList(ddd, "CodeDesc", "CodeDesc");//*通報單位
            ViewBag.NotificationContentList = new SelectList(parm.GetCodeData("NotificationContent"), "CodeDesc", "CodeDesc", model.NotificationContent);//*通報內容
            ViewBag.NotificationSourceList = new SelectList(parm.GetCodeData("NotificationSource"), "CodeDesc", "CodeDesc", model.NotificationSource);//*通報來源
            ViewBag.NotificationUnitList = new SelectList(parm.GetCodeData("NotificationUnit"), "CodeDesc", "CodeDesc", model.NotificationUnit);//*通報單位
            //Add by zhangwei 20180315 start
            List<SelectListItem> Originallist = new List<SelectListItem>
            {
                new SelectListItem() {Text ="-請選擇-", Value = "0"},
                new SelectListItem() {Text ="Y", Value = "Y"},
                new SelectListItem() {Text ="N", Value = "N"},
            };
            ViewBag.OriginalList = Originallist;
            //Add by zhangwei 20180315 end
            return View(model);
        }


        [HttpPost]
        public ActionResult EditWarn(WarningDetails model)
        {
            model.Status = "C01";
            if (model.bool_909113 == true) { model.Flag_909113 = "1"; } else { model.Flag_909113 = "0"; }
            if (model.bool_Retry == true) { model.Retry = "1"; } else { model.Retry = "0"; }
            if (model.bool_Set == true) { model.Set = "1"; } else { model.Set = "0"; }
            if (model.bool_Extend == true) { model.Extend = "1"; } else { model.Extend = "0"; }
            if (model.bool_Release == true) { model.Release = "1"; } else { model.Release = "0"; }
            if (model.bool_Fix == true) { model.FIX = "1"; } else { model.FIX = "0"; }
            if (model.bool_FIXSEND == true) { model.FIXSEND = "1"; } else { model.FIXSEND = "0"; }
            BindWarningStatusList();
            if (!model.HappenDateTimeForHour.Contains(":") && !model.HappenDateTimeForHour.Contains("："))//*不包含冒號的時候
            {
                model.HappenDateTimeForHour = model.HappenDateTimeForHour.Substring(0, 2) + ":" + model.HappenDateTimeForHour.Substring(2, 2);
            }
            if (!model.ReleaseDateForHour.Contains(":") && !model.ReleaseDateForHour.Contains("："))//*不包含冒號的時候
            {
                model.ReleaseDateForHour = model.ReleaseDateForHour.Substring(0, 2) + ":" + model.ReleaseDateForHour.Substring(2, 2);
            }
            model.ReleaseDate = UtlString.FormatDateTwStringToAd(model.ReleaseDate) + " " + model.ReleaseDateForHour; ;
            model.HappenDateTime = UtlString.FormatDateTwStringToAd(model.HappenDateTime) + " " + model.HappenDateTimeForHour;
            model.EtabsDatetime = UtlString.FormatDateTwStringToAd(model.EtabsDatetime) + " " + model.EtabsDatetimeHour;
            model.ExtendDate = UtlString.FormatDateTwStringToAd(model.ExtendDate) + " " + model.ExtendDateHour;
            model.ForCDate = UtlString.FormatDateTwStringToAd(model.ForCDate);
            model.ModifiedUser = LogonUser.Account;
            if (model.EtabsDatetime.Trim() == "")
            {
                model.EtabsDatetime = null;
            }
            if (model.ReleaseDate.Trim() == "")
            {
                model.ReleaseDate = null;
            }
            if (model.ExtendDate.Trim() == "")
            {
                model.ExtendDate = null;
            }
            CaseWarningBIZ warn = new CaseWarningBIZ();
            return Json(warn.EditWarnContent(model) ? new JsonReturn() { ReturnCode = "1", ReturnMsg = "" }
                                                                 : new JsonReturn() { ReturnCode = "0", ReturnMsg = Lang.csfs_add_fail });
        }
        public ActionResult ReturnClose(string statusArr, string caseIdarr)
        {
            string userId = LogonUser.Account;
            string[] caseid = caseIdarr.Split(',');
            string[] status = statusArr.Split(',');
            List<Guid> aryCaseId = (from id in caseid where !string.IsNullOrEmpty(id) select new Guid(id)).ToList();
            List<String> aryStatus = (from s in status where !string.IsNullOrEmpty(s) select s).ToList();
            return Json(_directorBiz.DirectorApprove(aryStatus, aryCaseId, userId));
        }
        public ActionResult Return(DirectorToApprove model, string strIds, string statusArr)
        {
            string[] caseid = strIds.Split(',');
            string[] status = statusArr.Split(',');
            List<Guid> aryCaseId = (from id in caseid where !string.IsNullOrEmpty(id) select new Guid(id)).ToList();
            List<String> aryStatus = (from s in status where !string.IsNullOrEmpty(s) select s).ToList();
            string userId = LogonUser.Account;
            return Json(_directorBiz.DirectorReturn(model, aryCaseId, aryStatus, userId));
        }

        /// <summary>
        /// 實際查詢動作
        /// </summary>
        public WarningUnionQueryViewModel SearchList(WarningUnionQuery model)
        {
            string COL_PID = "";
            // 新增個資LOG
            string _user = (HttpContext.Session["UserAccount"] != null) ? HttpContext.Session["UserAccount"].ToString() : "";
            System.Collections.Specialized.NameValueCollection _parameters = new System.Collections.Specialized.NameValueCollection();
            //System.Collections.Specialized.NameValueCollection _parameters = ("POST".Equals(HttpContext.Request.HttpMethod)) ? HttpContext.Request.Form : HttpContext.Request.QueryString;
            string _controller = (this.ControllerContext.RouteData.Values["Controller"] != null) ? this.ControllerContext.RouteData.Values["Controller"].ToString() : "";
            string _action = (this.ControllerContext.RouteData.Values["Action"] != null) ? this.ControllerContext.RouteData.Values["Action"].ToString() : "";
            string _ip = (HttpContext.Request != null && !string.IsNullOrEmpty(HttpContext.Request.ServerVariables["REMOTE_ADDR"])) ? HttpContext.Request.ServerVariables["REMOTE_ADDR"] : "";
            BaseBusinessRule _business = new BaseBusinessRule();
            #region Cookie
            HttpCookie modelCookie = new HttpCookie("QueryCookie");
            modelCookie.Values.Add("COL_PID", model.COL_PID);
            modelCookie.Values.Add("Kind", model.Kind);
            modelCookie.Values.Add("Set", model.Set);
            modelCookie.Values.Add("COL_165CASE", model.COL_165CASE);
            modelCookie.Values.Add("COL_ACCOUNT2", model.COL_ACCOUNT2);
            modelCookie.Values.Add("DocNo", model.DocNo);
            //modelCookie.Values.Add("VictimName", model.VictimName);
            //modelCookie.Values.Add("ForCDateS", model.ForCDateS);
            //modelCookie.Values.Add("ForCDateE", model.ForCDateE);
            modelCookie.Values.Add("StateType", model.StateType);
            modelCookie.Values.Add("RelieveDateS", model.RelieveDateS);
            modelCookie.Values.Add("RelieveDateE", model.RelieveDateE);
            modelCookie.Values.Add("Original", model.Original);
            //modelCookie.Values.Add("ModifyDateS", model.ModifyDateS);
            //modelCookie.Values.Add("ModifyDateE", model.ModifyDateE);
            Response.Cookies.Add(modelCookie);
            #endregion
            //model.ForCDateS = UtlString.FormatDateTwStringToAd(model.ForCDateS);
            //model.ForCDateE = UtlString.FormatDateTwStringToAd(model.ForCDateE);
            //Add by zhangwei 20180315 start
            model.RelieveDateS = UtlString.FormatDateTwStringToAd(model.RelieveDateS);
            model.RelieveDateE = UtlString.FormatDateTwStringToAd(model.RelieveDateE);
            //model.ModifyDateS = UtlString.FormatDateTwStringToAd(model.ModifyDateS);
            //model.ModifyDateE = UtlString.FormatDateTwStringToAd(model.ModifyDateE);
            //Add by zhangwei 20180315 end
            IList<WarningUnionQuery> result = wqBiz.GetQueryList(model);
            if (result.Count > 0)
            {
                for (int i = 0; i < result.Count; i++)
                {
                    if (!String.IsNullOrEmpty(result[i].COL_PID))
                    {
                        COL_PID = result[i].COL_PID.Trim();
                        _business.ExecuteAPLOGSave(_user, _controller, _action, _ip, "DOCNO=" + result[i].DocNo, COL_PID);
                    }
                }
            }

            var viewModel = new WarningUnionQueryViewModel()
            {
                WarningUnionQuery = model,
                WarningUnionQueryList = result,
            };

            return viewModel;
        }
        public ActionResult WarningCustomerInfo(string COL_PID, string DocNo)
        {
            return View(CIBZ.GetWarningData(COL_PID, DocNo));
        }
        public ActionResult Excel(WarningUnionQuery model)
        {

            //model.ForCDateS = UtlString.FormatDateTwStringToAd(model.ForCDateS);
            //model.ForCDateE = UtlString.FormatDateTwStringToAd(model.ForCDateE);

            //MemoryStream ms = new MemoryStream();
            //string fileName = string.Empty;
            //ms = wqBiz.ParmCodeExcel_NPOI(model);
            HttpCookie cookies = Request.Cookies.Get("QueryCookie");
            if (cookies != null)
            {
                if (cookies.Values["COL_PID"] != null) model.COL_PID = cookies.Values["COL_PID"];
                if (cookies.Values["COL_ACCOUNT2"] != null) model.COL_ACCOUNT2 = cookies.Values["COL_ACCOUNT2"];
                if (cookies.Values["DocNo"] != null) model.DocNo = cookies.Values["DocNo"];
                //if (cookies.Values["VictimName"] != null) model.VictimName = cookies.Values["VictimName"];
                //if (cookies.Values["ForCDateS"] != null) model.ForCDateS = cookies.Values["ForCDateS"];
                //if (cookies.Values["ForCDateE"] != null) model.ForCDateE = cookies.Values["ForCDateE"];
                if (cookies.Values["StateType"] != null) model.StateType = cookies.Values["StateType"];
                if (cookies.Values["RelieveDateS"] != null) model.RelieveDateS = cookies.Values["RelieveDateS"];
                if (cookies.Values["RelieveDateE"] != null) model.RelieveDateE = cookies.Values["RelieveDateE"];
                if (cookies.Values["Original"] != null) model.Original = cookies.Values["Original"];
                if (cookies.Values["ModifyDateS"] != null) model.ModifyDateS = cookies.Values["ModifyDateS"];
                if (cookies.Values["ModifyDateE"] != null) model.ModifyDateE = cookies.Values["ModifyDateE"];
            }
            //model.ForCDateS = UtlString.FormatDateTwStringToAd(model.ForCDateS);
            //model.ForCDateE = UtlString.FormatDateTwStringToAd(model.ForCDateE);
            //Add by zhangwei 20180315 start
            model.RelieveDateS = UtlString.FormatDateTwStringToAd(model.RelieveDateS);
            model.RelieveDateE = UtlString.FormatDateTwStringToAd(model.RelieveDateE);
            model.ModifyDateS = UtlString.FormatDateTwStringToAd(model.ModifyDateS);
            model.ModifyDateE = UtlString.FormatDateTwStringToAd(model.ModifyDateE);
            MemoryStream ms = new MemoryStream();
            ms = wqBiz.ParmCodeExcel_NPOI(model);
            string fileName = string.Empty;
            fileName = Lang.csfs_menu_tit_warningquery + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xls";

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

        public void BindWarningStatusList()
        {
            PARMCodeBIZ parm = new PARMCodeBIZ();
            ViewBag.StatusList = new SelectList(parm.GetCodeData("WarningStatus"), "CodeNo", "CodeDesc");
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
        private IList<WarningUnionQuery> GetQueryList(WarningUnionQuery model)
        {
            throw new NotImplementedException();
        }


        public ActionResult Details(string DocNo)
        {
            ViewBag.DocNo = DocNo;
            BindWarningStatusList();
            CaseWarningBIZ warn = new CaseWarningBIZ();

            CaseWarningViewModel viewmodel = new CaseWarningViewModel()
            {
                WarningMaster = warn.GetWarnMasterListByDocNo(DocNo),
                WarningAttachmentList = warn.GetWarnAttatchmentList(DocNo),
                WarningStateList = warn.GetWarnStateQueryList(DocNo),
                WarningDetailsList = warn.WarningDetailsSearchList(DocNo)
            };
            return PartialView("Details", viewmodel);
        }
    }
}