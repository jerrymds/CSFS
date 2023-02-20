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
    public class DeadRecordController : AppController
    {
        #region 全局變量
        CaseDeadQueryRecordBIZ _CaseDeadQueryRecordBIZ = new CaseDeadQueryRecordBIZ();
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

            CaseRecordCondition model = new CaseRecordCondition();

            if (isBack == "1")
            {
                //*執行cookie里的內容
                HttpCookie cookies = Request.Cookies.Get("CaseDeadQueryRecordWhere");
                if (cookies != null)
                {
                    if (cookies.Values["DocNo"] != null) model.DocNo = cookies.Values["DocNo"];
                    if (cookies.Values["HeirId"] != null) model.HeirId = cookies.Values["HeirId"];
                    //if (cookies.Values["CustAccount"] != null) model.CustAccount = cookies.Values["CustAccount"];
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
            if (LogonUser != null && !string.IsNullOrEmpty(LogonUser.RCAFAccount) && !string.IsNullOrEmpty(LogonUser.RCAFBranch))
            {
                model.IsEnable = "Y";
            }
            else
            {
                model.IsEnable = "N";
            }


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
        public ActionResult _QueryResult(CaseRecordCondition model, int pageNum = 1, string strSortExpression = "CaseDeadVersion.DocNo,CaseDeadVersion.TrnNum", string strSortDirection = "asc")
        {
            #region 查詢條件記錄在cookie
            HttpCookie modelCookie = new HttpCookie("CaseDeadQueryRecordWhere");
            modelCookie.Values.Add("DocNo", model.DocNo);
            modelCookie.Values.Add("HeirId", model.HeirId);
           // modelCookie.Values.Add("CustAccount", model.CustAccount);
           // modelCookie.Values.Add("Currency", model.Currency);
           // modelCookie.Values.Add("CaseStatus", model.CaseStatus);
            modelCookie.Values.Add("ForCDateS", model.ForCDateS);
            modelCookie.Values.Add("ForCDateE", model.ForCDateE);
            modelCookie.Values.Add("AgentDepartment", model.AgentDepartment);
            modelCookie.Values.Add("AgentDepartment2", model.AgentDepartment2);
            modelCookie.Values.Add("AgentDepartmentUser", model.AgentDepartmentUser);
            //modelCookie.Values.Add("FileName", model.FileName);
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
        //public ActionResult Edit(string pKey, string pFlag)
        public ActionResult Edit(string Content)
        {
            //ViewBag.Key = pKey;
            //ViewBag.Flag = pFlag;
            //return View();
            return Json(_CaseDeadQueryRecordBIZ.EditCaseTrsQuery(Content) > 0 ? new JsonReturn { ReturnCode = "1", ReturnMsg = Lang.csfs_del_ok }
                                          : new JsonReturn { ReturnCode = "0", ReturnMsg = Lang.csfs_del_fail });
        }
        [HttpPost]
       // public ActionResult DetailExport(string Content, HttpPostedFileBase fileAttNames)
        public ActionResult DetailExport()
        {
            //string Inputerror = "";
            string FileName = "";
            string xlsname = "";
            string DocNo = Request["Docno"].ToString();
            string ReTry = Request["Retry"].ToString();
            CaseDeadBIZ Biz = new CaseDeadBIZ();
            if (ReTry == "")
            {
                string UsedCaseNo = Biz.GetDailyCase();
                if (UsedCaseNo.Length > 0)
                {
                    return Json(new JsonReturn { ReturnCode = "今日已有啟動案件 :" + UsedCaseNo + "，是否繼續執行 ?", ReturnMsg = "今日已有啟動案件 :" + UsedCaseNo + "，是否繼續執行 ?" });
                }
            }
            string strCaseTrsNewID = Biz.GetCaseMasterbyCaseNo(DocNo);
            if (strCaseTrsNewID.Length>8)
            {
                Guid CaseId = Guid.Parse(strCaseTrsNewID);
                Biz.DeleteXlsxCaseEdocFile(CaseId);
            }
            DataTable DT = new DataTable();

            if (Request.Files.Count > 0)
            {
                    //  Get all files from Request object  
                    HttpFileCollectionBase files = Request.Files;
                    for (int i = 0; i < files.Count; i++)
                    {
                        //string path = AppDomain.CurrentDomain.BaseDirectory + "Uploads/";  
                        //string filename = Path.GetFileName(Request.Files[i].FileName);  

                        HttpPostedFileBase file = files[i];
                        string fname;

                        // Checking for Internet Explorer  
                        if (Request.Browser.Browser.ToUpper() == "IE" || Request.Browser.Browser.ToUpper() == "INTERNETEXPLORER")
                        {
                            string[] testfiles = file.FileName.Split(new char[] { '\\' });
                            fname = testfiles[testfiles.Length - 1];
                        }
                        else
                        {
                            fname = file.FileName;
                        }

                        // Get the complete folder path and store the file inside it.  
                        var path = Path.Combine(Server.MapPath("~/Uploads"), fname);
                        file.SaveAs(path);
                        xlsname = path;
                        FileName = path;                       
                        MemoryStream ms = new MemoryStream();
                        DT.Clear();
                        DT = Biz.ImportXLSX_MONEY(xlsname, strCaseTrsNewID,fname, DocNo);                        
                }

            }

            if (DT.Rows.Count > 0)
            {
               // 判斷EXCEL 是否
                DataTable TempTable = new DataTable();
                TempTable.Clear();
                TempTable = DT.Copy();
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                //for (int chk = 0; chk < TempTable.Rows.Count; chk++)
                //{
                //    // 判斷如果當天已經有另一筆案號已設定,整個EXCEL都不做
                //    isDup = Biz.GetDailyData(TempTable.Rows[chk][1].ToString(), TempTable.Rows[chk][2].ToString());
                //    if (isDup > 0)
                //    {
                //        break;
                //    }
                //}
                //if (isDup > 0)
                //{
                //    return Json(new JsonReturn { ReturnCode = "0", ReturnMsg = "出現重覆;今日已有拋查內容" });
                //}

                //for (int i = 0; i < DT.Rows.Count; i++)
                //{
                //    if ((string.IsNullOrEmpty(DT.Rows[i][5].ToString())))
                //    {
                //        Inputerror = Inputerror + "F" + "-不能為空白\r";
                //        break;
                //    }
                //    if ((string.IsNullOrEmpty(DT.Rows[i][6].ToString())))
                //    {
                //        Inputerror = Inputerror + "G" + "-不能為空白\r";
                //        break;
                //    }
                //    if ((string.IsNullOrEmpty(DT.Rows[i][7].ToString())))
                //    {
                //        Inputerror = Inputerror + "H" + "-不能為空白\r";
                //        break;
                //    }
                //    if ((string.IsNullOrEmpty(DT.Rows[i][8].ToString())))
                //    {
                //        Inputerror = Inputerror + "I" + "-不能為空白\r";
                //        break;
                //    }
                //    if ((string.IsNullOrEmpty(DT.Rows[i][9].ToString())))
                //    {
                //        Inputerror = Inputerror + "J" + "-不能為空白\r";
                //        break;
                //    }
                //    if ((string.IsNullOrEmpty(DT.Rows[i][10].ToString())))
                //    {
                //        Inputerror = Inputerror + "K" + "-不能為空白\r";
                //        break;
                //    }
                //    if ((string.IsNullOrEmpty(DT.Rows[i][16].ToString())))
                //    {
                //        Inputerror = Inputerror + "Q" + "-不能為空白\r";
                //        break;
                //    }

                //}
            }

             Biz.DeleteCaseDeathLoan(strCaseTrsNewID);
            this.LogWriter = this.AppLog.Writer;
            // 先將查詢資料或上傳資料寫入CaseDeadVersion
            if (DT.Rows.Count > 0)
            {
                //string strCaseTrsNewID = Biz.GetCaseMasterbyCaseNo(DocNo);
                bool succes = Biz.InsertCaseDeathLoan(DT, DocNo, strCaseTrsNewID);
                _CaseDeadQueryRecordBIZ.UpdateCaseDeadExcelSetup(DocNo);  
            }
            //_CaseDeadQueryRecordBIZ.UpdateCaseDeadExcelSetup(DocNo);
            //return Json(rtn ? new JsonReturn { ReturnCode = "1", ReturnMsg = "上傳成功" }
            //                    : new JsonReturn { ReturnCode = "0", ReturnMsg = "上傳失敗" });
            return Json(_CaseDeadQueryRecordBIZ.EditCaseTrsQuery(DocNo) > 0 ? new JsonReturn { ReturnCode = "1", ReturnMsg = "啟動成功" }
                                                 : new JsonReturn { ReturnCode = "0", ReturnMsg = "啟動失敗" });

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
 

        }
        #region 自定義方法
        #region 下拉選單
        /// <summary>
        /// 查詢條件清單綁值
        /// </summary>
        public void BindList()
        {
            // 案件狀態
            ViewBag.CaseStatusList = new SelectList(_CaseDeadQueryRecordBIZ.GetCaseTrsCodeData("CaseCustStatus"), "CodeNo", "CodeDesc");
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
        public CaseDeadViewModel SearchList(CaseRecordCondition model, string strSortExpression, string strSortDirection, int pageNum = 1)
        {
            // 查詢清單資料
            IList<CaseDeadVersion> result = _CaseDeadQueryRecordBIZ.GetHisQueryList(model, pageNum, strSortExpression, strSortDirection);

            var viewModel = new CaseDeadViewModel()
            {
                CaseRecordCondition = model,
                CaseDeadVersionList = result,
            };

            // 資料清單之每頁資料數、當前頁頁碼、資料總筆數賦值
            viewModel.CaseRecordCondition.PageSize = _CaseDeadQueryRecordBIZ.PageSize;
            viewModel.CaseRecordCondition.CurrentPage = _CaseDeadQueryRecordBIZ.PageIndex;
            viewModel.CaseRecordCondition.TotalItemCount = _CaseDeadQueryRecordBIZ.DataRecords;
            viewModel.CaseRecordCondition.SortExpression = strSortExpression;
            viewModel.CaseRecordCondition.SortDirection = strSortDirection;

            return viewModel;
        }

 
  

        public ActionResult Export(string uploadkind, string id,string DocNo =null)
        {
            DataTable DeadResult = new DataTable();
            string filePath = "";
            string fileName = "";
            string fileServerName = "";
            if (id == null) return null;
            if (uploadkind == "xlsx3")
            {
                Guid CaseId = new Guid(id);
                CaseAccountBiz caseAccount = new CaseAccountBiz();
                ImportEDocBiz CaseEdocFile = new ImportEDocBiz();
                CaseEdocFile caseEdocFile = caseAccount.OpenDeadExcel3(CaseId);
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

            if (uploadkind == "xlsx1")
            {
                MemoryStream ms = new MemoryStream();
                fileName = string.Empty;
                // 匯出資料
                ms = _CaseDeadQueryRecordBIZ.ExportExcel1(id);

                // 匯出文件名稱
                fileName = "設定回饋檔" + "_" + DocNo + ".xlsx";

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

            if (uploadkind == "xlsx2")
            {
                MemoryStream ms = new MemoryStream();
                fileName = string.Empty;
                // 匯出資料
                ms = _CaseDeadQueryRecordBIZ.ExportExcel(id);

                // 匯出文件名稱
                fileName = "死亡回饋檔" + "_" + DocNo+ ".xlsx";

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
            if (uploadkind == "pdf")
            {
                Guid CaseId = new Guid(id);
                CaseAccountBiz caseAccount = new CaseAccountBiz();
                ImportEDocBiz CaseEdocFile = new ImportEDocBiz();
                CaseEdocFile caseEdocFile = caseAccount.OpenDeadPdf(CaseId);
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
        //    return Json(_CaseDeadQueryRecordBIZ.DeleteAttatch(attachId) > 0 ? new JsonReturn { ReturnCode = "1", ReturnMsg = Lang.csfs_del_ok }
        //                                            : new JsonReturn { ReturnCode = "0", ReturnMsg = Lang.csfs_del_fail });
        //}
        public ActionResult Delete(string Content)
        {
            //return Json(_CaseDeadQueryRecordBIZ.DeleteCaseTrsQuery(Content), JsonRequestBehavior.AllowGet);
            return Json(_CaseDeadQueryRecordBIZ.DeleteCaseTrsQuery(Content) > 0 ? new JsonReturn { ReturnCode = "1", ReturnMsg = Lang.csfs_del_ok }
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
        //    ms = _CaseDeadQueryRecordBIZ.ExportExcel(model.CheckedData);

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

        //    bool flag = _CaseDeadQueryRecordBIZ.SearchAgain(pDocNo, pVersion, pCaseStatus, pCountDocNo, LogonUser, strFilePath);

        //    #region 刪除產出的回文資料

        //    // 取同案件下最大版本號
        //    DataTable dt = _CaseDeadQueryRecordBIZ.ChangeDataTable(pDocNo, pVersion, pCaseStatus, pCountDocNo);
        //    DataView dataView = dt.DefaultView;
        //    DataTable dtDistinct = dataView.ToTable(true, "DocNo");

        //    for (int i = 0; i < dtDistinct.Rows.Count; i++)
        //    {
        //       DataTable dtCaseCustQuery = _CaseDeadQueryRecordBIZ.GetCaseTrsQueryByDocNo(dtDistinct.Rows[i]["DocNo"].ToString());

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