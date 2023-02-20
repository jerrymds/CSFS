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

namespace CTBC.CSFS.Areas.CaseCust.Contrllers
{
    public class CaseCustHistoryController : AppController
    {
        #region 全局變量
        CaseCustHistoryBIZ _CaseCustHistoryBIZ = new CaseCustHistoryBIZ();
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

            CaseCustCondition model = new CaseCustCondition();

            if (isBack == "1")
            {
                //*執行cookie里的內容
                HttpCookie cookies = Request.Cookies.Get("CaseCustHistoryWhere");
                if (cookies != null)
                {
                    if (cookies.Values["FileGovenment"] != null) model.FileGovenment = cookies.Values["FileGovenment"];
                    if (cookies.Values["DocNo"] != null) model.DocNo = cookies.Values["DocNo"];
                    if (cookies.Values["FinishDateStart"] != null) model.FinishDateStart = cookies.Values["FinishDateStart"];
                    if (cookies.Values["FinishDateEnd"] != null) model.FinishDateEnd = cookies.Values["FinishDateEnd"];
                    if (cookies.Values["CaseStatus"] != null) model.CaseStatus = cookies.Values["CaseStatus"];
                    if (cookies.Values["FileNo"] != null) model.FileNo = cookies.Values["FileNo"];
                    if (cookies.Values["ProcessingMethod"] != null) model.ProcessingMethod = cookies.Values["ProcessingMethod"];
                    if (cookies.Values["DateStart"] != null) model.DateStart = cookies.Values["DateStart"];
                    if (cookies.Values["DateEnd"] != null) model.DateEnd = cookies.Values["DateEnd"];
                    if (cookies.Values["SearchProgram"] != null) model.SearchProgram = cookies.Values["SearchProgram"];
                    if (cookies.Values["CustIdNo"] != null) model.CustIdNo = cookies.Values["CustIdNo"];
                    if (cookies.Values["GoFileNo"] != null) model.GoFileNo = cookies.Values["GoFileNo"];
                    ViewBag.CurrentPage = cookies.Values["CurrentPage"];
                    ViewBag.isQuery = "1";
                }
            }

            // 當前登錄者是否有重查的權限
            if (LogonUser != null && !string.IsNullOrEmpty(LogonUser.RCAFAccount) && !string.IsNullOrEmpty(LogonUser.RCAFBranch))
            {
                model.IsEnable = "Y";
            }
            else
            {
                model.IsEnable = "N";
            }


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
        public ActionResult _QueryResult(CaseCustCondition model, int pageNum = 1, string strSortExpression = "CaseCustQuery.DocNo,CaseCustQuery.Version", string strSortDirection = "asc")
        {
            #region 查詢條件記錄在cookie
            HttpCookie modelCookie = new HttpCookie("CaseCustHistoryWhere");
            modelCookie.Values.Add("FileGovenment", model.FileGovenment);
            modelCookie.Values.Add("DocNo", model.DocNo);
            modelCookie.Values.Add("FinishDateStart", model.FinishDateStart);
            modelCookie.Values.Add("FinishDateEnd", model.FinishDateEnd);
            modelCookie.Values.Add("CaseStatus", model.CaseStatus);
            modelCookie.Values.Add("FileNo", model.FileNo);
            modelCookie.Values.Add("ProcessingMethod", model.ProcessingMethod);
            modelCookie.Values.Add("DateStart", model.DateStart);
            modelCookie.Values.Add("DateEnd", model.DateEnd);
            modelCookie.Values.Add("SearchProgram", model.SearchProgram);
            modelCookie.Values.Add("CustIdNo", model.CustIdNo);
            modelCookie.Values.Add("GoFileNo", model.GoFileNo);
            modelCookie.Values.Add("CurrentPage", pageNum.ToString());
            Response.Cookies.Add(modelCookie);
            #endregion

            #region 日期格式轉換
            // 結案日期 起
            if (!string.IsNullOrEmpty(model.FinishDateStart))
            {
                model.FinishDateStart = UtlString.FormatDateTwStringToAd(model.FinishDateStart);
            }
            // 結案日期 訖
            if (!string.IsNullOrEmpty(model.FinishDateEnd))
            {
                model.FinishDateEnd = UtlString.FormatDateTwStringToAd(model.FinishDateEnd);
            }

            // 建檔日期 起
            if (!string.IsNullOrEmpty(model.DateStart))
            {
                model.DateStart = UtlString.FormatDateTwStringToAd(model.DateStart);
            }
            // 建檔日期 訖
            if (!string.IsNullOrEmpty(model.DateEnd))
            {
                model.DateEnd = UtlString.FormatDateTwStringToAd(model.DateEnd);
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

        #region 自定義方法
        #region 下拉選單
        /// <summary>
        /// 查詢條件清單綁值
        /// </summary>
        public void BindList()
        {
            // 案件狀態
            ViewBag.CaseStatusList = new SelectList(_CaseCustHistoryBIZ.GetCodeData("CaseCustStatus"), "CodeNo", "CodeDesc");
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
        public CaseCustViewModel SearchList(CaseCustCondition model, string strSortExpression, string strSortDirection, int pageNum = 1)
        {
            // 查詢清單資料
            IList<CaseCustQuery> result = _CaseCustHistoryBIZ.GetHistoryQueryList(model, pageNum, strSortExpression, strSortDirection);

            var viewModel = new CaseCustViewModel()
            {
                CaseCustCondition = model,
                CaseCustQueryList = result,
            };

            // 資料清單之每頁資料數、當前頁頁碼、資料總筆數賦值
            viewModel.CaseCustCondition.PageSize = _CaseCustHistoryBIZ.PageSize;
            viewModel.CaseCustCondition.CurrentPage = _CaseCustHistoryBIZ.PageIndex;
            viewModel.CaseCustCondition.TotalItemCount = _CaseCustHistoryBIZ.DataRecords;
            viewModel.CaseCustCondition.SortExpression = strSortExpression;
            viewModel.CaseCustCondition.SortDirection = strSortDirection;

            return viewModel;
        }

        /// <summary>
        /// 歷史記錄查詢與重送維護-保存
        /// </summary>
        /// <param name="Key">主鍵</param>
        /// <param name="Content">輸入內容</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SaveResult(string Key, string Content)
        {
            bool flag = _CaseCustHistoryBIZ.EndCase(Key, LogonUser.Account, Content);

            return Json(flag, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 匯出
        /// </summary>
        /// <param name="model">勾選資料</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Export(CaseCustCondition model)
        {
            MemoryStream ms = new MemoryStream();
            string fileName = string.Empty;

            // 勾選的資料
            CheckedData_P MasterList = JsonConvert.DeserializeObject<CheckedData_P>(model.CheckedDatas);

            // 匯出資料
            ms = _CaseCustHistoryBIZ.ExportExcel(model.CheckedData);

            // 匯出文件名稱
            fileName = "歷史記錄查詢與重送_" + Lang.csfs_debtexcel + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xls";

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

        /// <summary>
        /// 重查檢核
        /// </summary>
        /// <param name="pDocNo">案件編號</param>
        /// <param name="pVersion">版本號</param>
        /// <param name="pCaseStatus">案件狀態</param>
        /// <returns></returns>
        public ActionResult SearchAgainCheck(string pDocNo, string pVersion, string pCaseStatus, string pCountDocNo)
        {
            string flag = _CaseCustHistoryBIZ.SearchAgainCheck(pDocNo, pVersion, pCaseStatus, pCountDocNo);

            return Json(flag, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 重查
        /// </summary>
        /// <param name="pDocNo">案件編號</param>
        /// <param name="pVersion">版本號</param>
        /// <param name="pCaseStatus">案件狀態</param>
        /// <returns></returns>
        public ActionResult SearchAgain(string pDocNo, string pVersion, string pCaseStatus, string pCountDocNo)
        {
            string strFilePath = ConfigurationManager.AppSettings["txtFilePath"] + @"\";

            bool flag = _CaseCustHistoryBIZ.SearchAgain(pDocNo, pVersion, pCaseStatus, pCountDocNo, LogonUser, strFilePath);

            #region 刪除產出的回文資料

            // 取同案件下最大版本號
            DataTable dt = _CaseCustHistoryBIZ.ChangeDataTable(pDocNo, pVersion, pCaseStatus, pCountDocNo);
            DataView dataView = dt.DefaultView;
            DataTable dtDistinct = dataView.ToTable(true, "DocNo");

            for (int i = 0; i < dtDistinct.Rows.Count; i++)
            {
               DataTable dtCaseCustQuery = _CaseCustHistoryBIZ.GetCaseCustQueryByDocNo(dtDistinct.Rows[i]["DocNo"].ToString());

               // 取同案件下最大版本號
               DataRow dR = dtCaseCustQuery.Select("DocNo ='" + dtDistinct.Rows[i]["DocNo"] + "'", "Version DESC")[0];

               // 案件狀態
               string strCaseStatus = dR["Status"].ToString();

               if (!(strCaseStatus == "03" || strCaseStatus == "07" || strCaseStatus == "66"))
               {
                  OpenFileStream webService = new OpenFileStream();
                  dynamic stream = null;

                  webService.Url = ConfigurationManager.AppSettings["ServiceURL"];

                  // 回文檔名稱（存款帳戶開戶資料）
                  stream = JsonConvert.DeserializeObject(webService.DeleteFile(strFilePath + dR["ROpenFileName"].ToString()));

                  // 回文檔名稱（存款往來明細資料）
                  stream = JsonConvert.DeserializeObject(webService.DeleteFile(strFilePath + dR["RFileTransactionFileName"].ToString()));

                  // 回文首頁（第一頁PDF）
                  stream = JsonConvert.DeserializeObject(webService.DeleteFile(strFilePath + pDocNo + "_" + dR["Version"].ToString() + "_001.pdf"));

                  // 回文首頁(ALL PDF)
                  stream = JsonConvert.DeserializeObject(webService.DeleteFile(strFilePath + pDocNo + "_" + dR["Version"].ToString() + ".pdf"));
               }
            }

            #endregion

            return Json(flag, JsonRequestBehavior.AllowGet);
        }
        #endregion
    }
}