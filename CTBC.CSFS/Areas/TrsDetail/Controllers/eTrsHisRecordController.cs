using CTBC.CSFS.BussinessLogic;
using CTBC.CSFS.Filter;
using CTBC.CSFS.Models;
using CTBC.CSFS.Pattern;
using CTBC.CSFS.Resource;
using CTBC.CSFS.ViewModels;
using CTBC.FrameWork.Util;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using System.Data;
using CTBC.CSFS.WebService.OpenStream;

namespace CTBC.CSFS.Areas.TrsDetail.Contrllers
{
    public class eTrsHisRecordController : AppController
    {
        #region 全局變量
        CaseTrsQueryHistoryBIZ _CaseTrsQueryHistoryBIZ = new CaseTrsQueryHistoryBIZ();
        CaseQueryBIZ CQBIZ = new CaseQueryBIZ();
        PARMCodeBIZ parm = new PARMCodeBIZ();
        #endregion

        #region 頁面
        /// <summary>
        /// 歷史記錄查詢與重送
        /// </summary>
        /// <returns></returns>
        [RootPageFilter]
        public ActionResult Index(string isBack)
        {
            // 調用查詢清單綁定方法
            BindList();

            CaseHisCondition model = new CaseHisCondition();

            if (isBack == "1")
            {
                //*執行cookie里的內容
                HttpCookie cookies = Request.Cookies.Get("CaseTrsQueryHistoryWhere");
                if (cookies != null)
                {
                    if (cookies.Values["DocNo"] != null) model.DocNo = cookies.Values["DocNo"];
                    if (cookies.Values["CustId"] != null) model.CustId = cookies.Values["CustId"];
                    if (cookies.Values["CustAccount"] != null) model.CustAccount = cookies.Values["CustAccount"];
                    if (cookies.Values["ForCDateS"] != null) model.ForCDateS = cookies.Values["ForCDateS"];
                    if (cookies.Values["ForCDateE"] != null) model.ForCDateE = cookies.Values["ForCDateE"];
                    if (cookies.Values["AgentDepartment"] != null) model.AgentDepartment = cookies.Values["AgentDepartment"];
                    if (cookies.Values["AgentDepartment2"] != null) ViewBag.AgentDepartment2Query = cookies.Values["AgentDepartment2"];
                    if (cookies.Values["AgentDepartmentUser"] != null) ViewBag.AgentDepartmentUserQuery = cookies.Values["AgentDepartmentUser"];
                    ViewBag.CurrentPage = cookies.Values["CurrentPage"];
                    ViewBag.isQuery = "1";
                }
            }

            //// 當前登錄者是否有重查的權限
            //if (LogonUser != null && !string.IsNullOrEmpty(LogonUser.RCAFAccount) && !string.IsNullOrEmpty(LogonUser.RCAFBranch))
            //{
            //    model.IsEnable = "Y";
            //}
            //else
            //{
            //    model.IsEnable = "N";
            //}

            InitDropdownListOptions();

            return View(model);
        }

        /// <summary>
        /// 歷史記錄查詢與重送-清單頁面
        /// </summary>
        /// <param name="model">查詢條件</param>
        /// <param name="pageNum">當前頁</param>
        /// <param name="strSortExpression">排序欄位</param>
        /// <param name="strSortDirection">排序方式</param>
        /// <returns></returns>
        public ActionResult _QueryResult(CaseHisCondition model, int pageNum = 1, string strSortExpression = "CaseTrsQueryVersion.DocNo,CaseTrsQueryVersion.TrnNum", string strSortDirection = "asc")
        {
            #region 查詢條件記錄在cookie
            HttpCookie modelCookie = new HttpCookie("CaseTrsQueryHistoryWhere");
            modelCookie.Values.Add("DocNo", model.DocNo);
            modelCookie.Values.Add("CustId", model.CustId);
            modelCookie.Values.Add("CustAccount", model.CustAccount);
            modelCookie.Values.Add("Currency", model.Currency);
            modelCookie.Values.Add("CaseStatus", model.CaseStatus);
            modelCookie.Values.Add("ForCDateS", model.ForCDateS);
            modelCookie.Values.Add("ForCDateE", model.ForCDateE);
            modelCookie.Values.Add("AgentDepartment", model.AgentDepartment);
            modelCookie.Values.Add("AgentDepartment2", model.AgentDepartment2);
            modelCookie.Values.Add("AgentDepartmentUser", model.AgentDepartmentUser);
            modelCookie.Values.Add("CurrentPage", pageNum.ToString());
            Response.Cookies.Add(modelCookie);
            #endregion

            #region 日期格式轉換

            // 建檔日期 起
            if (!string.IsNullOrEmpty(model.ForCDateS))
            {
                model.ForCDateS = UtlString.FormatDateTwStringToAd(model.ForCDateS);
            }
            // 建檔日期 訖
            if (!string.IsNullOrEmpty(model.ForCDateE))
            {
                model.ForCDateE = UtlString.FormatDateTwStringToAd(model.ForCDateE);
            }
            #endregion

            return PartialView("_QueryResult", SearchList(model, strSortExpression, strSortDirection, pageNum));
        }

        /// <summary>
        /// 歷史記錄查詢與重送維護
        /// </summary>
        /// <param name="pKey">要更新的資料主鍵</param>
        /// <param name="pFlag">註記 pFlag=1：主管檢視放行；pFlag=2：歷史記錄查詢與重送</param>
        /// <returns></returns>
        public ActionResult Edit(string pKey, string pFlag)
        {
            ViewBag.Key = pKey;
            ViewBag.Flag = pFlag;

            return View();
        }
        #endregion


        public void InitDropdownListOptions()
        {
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
 
            //IList<PARMCode> list = parm.GetCodeData("ExportRoleList");
            //if (list != null && list.Any())
            //{
            //    string[] ary = { };
            //    var obj = list.FirstOrDefault();
            //    if (obj != null && !string.IsNullOrEmpty(obj.CodeMemo))
            //    {
            //        ary = obj.CodeMemo.Split(';');
            //    }
            //    if (ary.Length > 0 && LogonUser.Roles.Any() && LogonUser.Roles.Any(m => ary.Contains(m.RoleLDAPId)))
            //    {
            //        ViewBag.CanExport = "1";
            //    }
            //    //ViewBag.CanExport = "1";
            //}
            ////Add by zhangwei 20180315 start
            //List<SelectListItem> listRMType = new List<SelectListItem>
            //{
            //    new SelectListItem() {Text ="-請選擇-", Value = "0"},
            //    new SelectListItem() {Text ="是", Value = "1"},
            //    new SelectListItem() {Text ="否", Value = "2"},
            //};
            //ViewBag.RMTypeList = listRMType;
            //Add by zhangwei 20180315 end
        }
        #region 自定義方法
        #region 下拉選單
        /// <summary>
        /// 查詢條件清單綁值
        /// </summary>
        public void BindList()
        {
            // 案件狀態
            ViewBag.CaseStatusList = new SelectList(_CaseTrsQueryHistoryBIZ.GetCaseTrsCodeData("CaseCustStatus"), "CodeNo", "CodeDesc");
            // 處理方式
            ViewBag.ProcessingMethodList = BindProcessingMethodList();

            // 查詢項目
            ViewBag.SearchProgramList = BindSearchProgramList();
        }

        /// <summary>
        /// 處理方式
        /// </summary>
        /// <returns></returns>
        public SelectList BindProcessingMethodList()
        {
            // 選項集合
            IList<SelectListItem> lst = new List<SelectListItem>();

            lst.Add(new SelectListItem() { Value = "01", Text = "未處理" });
            lst.Add(new SelectListItem() { Value = "66", Text = "已處理" });
            lst.Add(new SelectListItem() { Value = "Y", Text = "已上傳" });

            return new SelectList(lst, "Value", "Text");
        }

        /// <summary>
        /// 查詢項目
        /// </summary>
        /// <returns></returns>
        public SelectList BindSearchProgramList()
        {
            // 選項集合
            IList<SelectListItem> lst = new List<SelectListItem>();

            lst.Add(new SelectListItem() { Value = "1", Text = "1.基本資料" });
            lst.Add(new SelectListItem() { Value = "2", Text = "2.存款明細" });
            lst.Add(new SelectListItem() { Value = "3", Text = "3.基本 + 存款明細" });

            return new SelectList(lst, "Value", "Text");
        }

        #endregion

        /// <summary>
        /// 實際查詢動作
        /// </summary>
        /// <param name="model">實體類</param>
        /// <param name="pageNum">當前頁面</param>
        /// <returns></returns>
        public CaseTrsViewModel SearchList(CaseHisCondition model, string strSortExpression, string strSortDirection, int pageNum = 1)
        {
            // 查詢清單資料
            IList<CaseTrsQueryVersion> result = _CaseTrsQueryHistoryBIZ.GetHisQueryList(model, pageNum, strSortExpression, strSortDirection);

            var viewModel = new CaseTrsViewModel()
            {
                CaseHisCondition = model,
                CaseTrsQueryVersionList = result,
            };

            // 資料清單之每頁資料數、當前頁頁碼、資料總筆數賦值
            viewModel.CaseHisCondition.PageSize = _CaseTrsQueryHistoryBIZ.PageSize;
            viewModel.CaseHisCondition.CurrentPage = _CaseTrsQueryHistoryBIZ.PageIndex;
            viewModel.CaseHisCondition.TotalItemCount = _CaseTrsQueryHistoryBIZ.DataRecords;
            viewModel.CaseHisCondition.SortExpression = strSortExpression;
            viewModel.CaseHisCondition.SortDirection = strSortDirection;

            return viewModel;
        }

        /// <summary>
        /// 歷史記錄查詢與重送維護-保存
        /// </summary>
        /// <param name="Key">主鍵</param>
        /// <param name="Content">輸入內容</param>
        /// <returns></returns>
        //[HttpPost]
        //public ActionResult SaveResult(string Key, string Content)
        //{
        //    bool flag = _CaseTrsQueryHistoryBIZ.EndCase(Key, LogonUser.Account, Content);

        //    return Json(flag, JsonRequestBehavior.AllowGet);
        //}

        //[HttpPost]
        //public ActionResult downloadXLSX(CaseTrsViewModel model)
        //{
        //    MemoryStream ms = new MemoryStream();
        //    string fileName = string.Empty;

        //    // 勾選的資料
        //    CheckedData_P MasterList = JsonConvert.DeserializeObject<CheckedData_P>(model.CheckedDatas);

        //    // 匯出資料
        //    ms = _CaseTrsQueryHistoryBIZ.ExportExcel(model.CheckedData);

        //    // 匯出文件名稱
        //    fileName = "歷史記錄查詢與重送_" + Lang.csfs_debtexcel + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xls";

        //    if (ms != null && ms.Length > 0)
        //    {
        //        Response.ClearContent();
        //        Response.ClearHeaders();
        //    }
        //    else
        //    {
        //        ms = new MemoryStream();
        //    }
        //    return File(ms.ToArray(), "application/vnd.ms-excel", fileName);
        //}

        //public ActionResult DownFile(string uploadkind, int id)
        //{
        //    string filePath = "";
        //    string fileName = "";
        //    string fileServerName = "";
        //    if (id < 0)
        //        return null;

        //    if (uploadkind == Uploadkind.CaseAttach)
        //    {
        //        CaseAttachment attach = new CaseAttachmentBIZ().GetAttachmentInfo(id);
        //        if (attach == null)
        //            return null;
        //        filePath = attach.AttachmentServerPath;
        //        fileServerName = attach.AttachmentServerName;
        //        fileName = attach.AttachmentName;
        //    }
        //    else if (uploadkind == Uploadkind.LendAttach)
        //    {
        //        LendAttachment lendAttach = new LendAttachmentBIZ().GetAttachmentInfo(id);
        //        if (lendAttach == null)
        //            return null;

        //        filePath = lendAttach.LendAttachServerPath;
        //        fileServerName = lendAttach.LendAttachServerName;
        //        fileName = lendAttach.LendAttachName;
        //    }
        //    else if (uploadkind == Uploadkind.MeetingResultAttachment)
        //    {
        //        MeetingResultDetail attach = new MeetingResultDetailBIZ().GetAttachDetailInfo(id);
        //        if (attach == null)
        //            return null;
        //        filePath = attach.AttatchDetailServerPath;
        //        fileServerName = attach.AttatchDetailServerName;
        //        fileName = attach.AttatchDetailName;
        //    }
        //    else if (uploadkind == Uploadkind.WarnAttach)
        //    {
        //        WarningAttachment attach = new CaseWarningBIZ().GetAttachDetailInfo(id);
        //        if (attach == null)
        //            return null;
        //        filePath = attach.AttachmentServerPath;
        //        fileServerName = attach.AttachmentServerName;
        //        fileName = attach.AttachmentName;
        //    }
        //    else if (uploadkind.Length > 20)
        //    {
        //        int at = uploadkind.IndexOf("|");
        //        string strCaseId = uploadkind.Substring(0, at);
        //        string strFileName = uploadkind.Substring(at + 1, (uploadkind.Length - at - 1));
        //        CaseAccountBiz caseAccount = new CaseAccountBiz();
        //        ImportEDocBiz CaseEdocFile = new ImportEDocBiz();
        //        CaseEdocFile caseEdocFile = caseAccount.OpenTxtDoc(strCaseId, strFileName);
        //        string text = string.Empty;
        //        //
        //        if (caseEdocFile != null)
        //        {
        //            byte[] file = caseEdocFile.FileObject;
        //            return File(file, "application/txt", caseEdocFile.FileName);
        //        }
        //        else
        //        {
        //            return Content("<script>alert('" + Lang.csfs_no_data + "');</script>");
        //        }

        //        //if (caseEdocFile != null)
        //        //{
        //        //    byte[] file = caseEdocFile.FileObject;
        //        //    text = System.Text.Encoding.UTF8.GetString(file);
        //        //}
        //        //string ReturnMsg = string.IsNullOrEmpty(text) ? Lang.csfs_txtdocnotfound : text;
        //        //return PartialView("TxtOpen", new CaseMaster { Memo = ReturnMsg });
        //    }
        //    else
        //    {
        //        return null;
        //    }
        //}
  

        public ActionResult Export(string uploadkind, string id)
        {
            string filePath = "";
            string fileName = "";
            string fileServerName = "";
            if (id == null) return null;

            if (uploadkind == "xlsx")
            {
                Guid CaseId = new Guid(id);
                CaseAccountBiz caseAccount = new CaseAccountBiz();
                ImportEDocBiz CaseEdocFile = new ImportEDocBiz();
                CaseEdocFile caseEdocFile = caseAccount.OpenExcel(CaseId);
                string text = string.Empty;
                //
                if (caseEdocFile != null)
                {
                    byte[] file = caseEdocFile.FileObject;
                    return File(file, "application/txt", caseEdocFile.FileName);
                }
                else
                {
                    return Content("<script>alert('" + Lang.csfs_no_data + "');</script>");
                }

            }
            if (uploadkind == "pdf")
            {
                Guid CaseId = new Guid(id);
                CaseAccountBiz caseAccount = new CaseAccountBiz();
                ImportEDocBiz CaseEdocFile = new ImportEDocBiz();
                CaseEdocFile caseEdocFile = caseAccount.OpenPdf(CaseId);
                string text = string.Empty;
                //
                if (caseEdocFile != null)
                {
                    byte[] file = caseEdocFile.FileObject;
                    return File(file, "application/zip", caseEdocFile.FileName);
                }
                else
                {
                    return Content("<script>alert('" + Lang.csfs_no_data + "');</script>");
                }

            }

            //else
            //{
            //    return null;
            //}
            string absoluFilePath = Path.Combine(Server.MapPath(filePath), fileServerName);
            if (!System.IO.File.Exists(absoluFilePath))
            {
                return Content("<script>alert('" + Lang.csfs_FileNotFound + "');</script>");
            }
            return File(new FileStream(absoluFilePath, FileMode.Open), "application/octet-stream", Server.UrlEncode(fileName));

        }

        /// <summary>
        /// 刪除一筆資料
        /// </summary>
        /// <param name="NewId"></param>
        /// <returns></returns>
        //public ActionResult Delete(string NewId)
        //{
        //    //attachment.UserId = LogonUser.Account;
        //    //casemaster.CaseId = model.CaseMaster.CaseId.ToString();
        //    //casemaster.CaseNo = model.CaseMaster.CaseNo;
        //    return Json(_CaseTrsQueryHistoryBIZ.DeleteAttatch(attachId) > 0 ? new JsonReturn { ReturnCode = "1", ReturnMsg = Lang.csfs_del_ok }
        //                                            : new JsonReturn { ReturnCode = "0", ReturnMsg = Lang.csfs_del_fail });
        //}
        public ActionResult Delete(string Content)
        {
            //return Json(_CaseTrsQueryHistoryBIZ.DeleteCaseTrsQuery(Content), JsonRequestBehavior.AllowGet);
            return Json(_CaseTrsQueryHistoryBIZ.DeleteCaseTrsQuery(Content) > 0 ? new JsonReturn { ReturnCode = "1", ReturnMsg = Lang.csfs_del_ok }
                                                    : new JsonReturn { ReturnCode = "0", ReturnMsg = Lang.csfs_del_fail });
        }
        /// <summary>
        /// 匯出
        /// </summary>
        /// <param name="model">勾選資料</param>
        /// <returns></returns>
        //[HttpPost]
        //public ActionResult Export(CaseCustCondition model)
        //{
        //    MemoryStream ms = new MemoryStream();
        //    string fileName = string.Empty;

        //    // 勾選的資料
        //    CheckedData_P MasterList = JsonConvert.DeserializeObject<CheckedData_P>(model.CheckedDatas);

        //    // 匯出資料
        //    ms = _CaseTrsQueryHistoryBIZ.ExportExcel(model.CheckedData);

        //    // 匯出文件名稱
        //    fileName = "歷史記錄查詢與重送_" + Lang.csfs_debtexcel + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xls";

        //    if (ms != null && ms.Length > 0)
        //    {
        //        Response.ClearContent();
        //        Response.ClearHeaders();
        //    }
        //    else
        //    {
        //        ms = new MemoryStream();
        //    }
        //    return File(ms.ToArray(), "application/vnd.ms-excel", fileName);
        //}

        /// <summary>
        /// 重查檢核
        /// </summary>
        /// <param name="pDocNo">案件編號</param>
        /// <param name="pVersion">版本號</param>
        /// <param name="pCaseStatus">案件狀態</param>
        /// <returns></returns>


        /// <summary>
        /// 重查
        /// </summary>
        /// <param name="pDocNo">案件編號</param>
        /// <param name="pVersion">版本號</param>
        /// <param name="pCaseStatus">案件狀態</param>
        /// <returns></returns>
        //public ActionResult SearchAgain(string pDocNo, string pVersion, string pCaseStatus, string pCountDocNo)
        //{
        //    string strFilePath = ConfigurationManager.AppSettings["txtFilePath"] + @"\";

        //    bool flag = _CaseTrsQueryHistoryBIZ.SearchAgain(pDocNo, pVersion, pCaseStatus, pCountDocNo, LogonUser, strFilePath);

        //    #region 刪除產出的回文資料

        //    // 取同案件下最大版本號
        //    DataTable dt = _CaseTrsQueryHistoryBIZ.ChangeDataTable(pDocNo, pVersion, pCaseStatus, pCountDocNo);
        //    DataView dataView = dt.DefaultView;
        //    DataTable dtDistinct = dataView.ToTable(true, "DocNo");

        //    for (int i = 0; i < dtDistinct.Rows.Count; i++)
        //    {
        //       DataTable dtCaseCustQuery = _CaseTrsQueryHistoryBIZ.GetCaseTrsQueryByDocNo(dtDistinct.Rows[i]["DocNo"].ToString());

        //       // 取同案件下最大版本號
        //       DataRow dR = dtCaseCustQuery.Select("DocNo ='" + dtDistinct.Rows[i]["DocNo"] + "'", "Version DESC")[0];

        //       // 案件狀態
        //       string strCaseStatus = dR["Status"].ToString();

        //       if (!(strCaseStatus == "03" || strCaseStatus == "07" || strCaseStatus == "66"))
        //       {
        //          OpenFileStream webService = new OpenFileStream();
        //          dynamic stream = null;

        //          webService.Url = ConfigurationManager.AppSettings["ServiceURL"];

        //          // 回文檔名稱（存款帳戶開戶資料）
        //          stream = JsonConvert.DeserializeObject(webService.DeleteFile(strFilePath + dR["ROpenFileName"].ToString()));

        //          // 回文檔名稱（存款往來明細資料）
        //          stream = JsonConvert.DeserializeObject(webService.DeleteFile(strFilePath + dR["RFileTransactionFileName"].ToString()));

        //          // 回文首頁（第一頁PDF）
        //          stream = JsonConvert.DeserializeObject(webService.DeleteFile(strFilePath + pDocNo + "_" + dR["Version"].ToString() + "_001.pdf"));

        //          // 回文首頁(ALL PDF)
        //          stream = JsonConvert.DeserializeObject(webService.DeleteFile(strFilePath + pDocNo + "_" + dR["Version"].ToString() + ".pdf"));
        //       }
        //    }

        //    #endregion

        //    return Json(flag, JsonRequestBehavior.AllowGet);
        //}
        #endregion
    }
}