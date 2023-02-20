using CTBC.CSFS.BussinessLogic;
using CTBC.CSFS.Models;
using CTBC.CSFS.Resource;
using CTBC.CSFS.ViewModels;
using CTBC.FrameWork.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CTBC.FrameWork.Pattern;
using CTBC.CSFS.Filter;
using System.Configuration;
using System.IO;
using iTextSharp.text.pdf;
using System.Text;
using iTextSharp.text;
using CTBC.CSFS.WebService.OpenStream;
using Newtonsoft.Json;
using System.Web.Script.Serialization;

namespace CTBC.CSFS.Areas.CaseCust.Contrllers
{
    public class CaseCustManagerController : AppController
    {
        #region 全局變量
        CaseCustManagerBIZ _CaseCustManagerBIZ = new CaseCustManagerBIZ();
        #endregion

        #region 頁面
        /// <summary>
        /// 主管檢視頁面
        /// </summary>
        /// <param name="isBack">isBack=“1”：重新加載此頁面</param>
        /// <returns></returns>
        [RootPageFilter]
        public ActionResult Index(string isBack)
        {
            // 調用查詢清單綁定方法
            BindList();

            // 查詢條件
            CaseCustCondition model = new CaseCustCondition();

            if (isBack == "1")
            {
                //*執行cookie里的內容
                HttpCookie cookies = Request.Cookies.Get("CaseCustQueryCookie");
                if (cookies != null)
                {
                    if (cookies.Values["FileGovenment"] != null) model.FileGovenment = cookies.Values["FileGovenment"];
                    if (cookies.Values["DocNo"] != null) model.DocNo = cookies.Values["DocNo"];
                    if (cookies.Values["FileDateStart"] != null) model.FileDateStart = cookies.Values["FileDateStart"];
                    if (cookies.Values["FileDateEnd"] != null) model.FileDateEnd = cookies.Values["FileDateEnd"];
                    if (cookies.Values["SearchProgram"] != null) model.SearchProgram = cookies.Values["SearchProgram"];
                    if (cookies.Values["FileNo"] != null) model.FileNo = cookies.Values["FileNo"];
                    if (cookies.Values["Result"] != null) model.Result = cookies.Values["Result"];
                    if (cookies.Values["DateStart"] != null) model.DateStart = cookies.Values["DateStart"];
                    if (cookies.Values["DateEnd"] != null) model.DateEnd = cookies.Values["DateEnd"];
                    if (cookies.Values["Status"] != null) model.Status = cookies.Values["Status"];
                    ViewBag.CurrentPage = cookies.Values["CurrentPage"];
                    ViewBag.isQuery = "1";
                }
            }

            return View(model);
        }

        /// <summary>
        /// 主管檢視頁面-查詢清單
        /// </summary>
        /// <param name="model">查詢條件</param>
        /// <param name="pageNum">當前頁</param>
        /// <param name="strSortExpression">排序欄位</param>
        /// <param name="strSortDirection">排序方式</param>
        /// <returns></returns>
        public ActionResult _QueryResult(CaseCustCondition model, int pageNum = 1, string strSortExpression = "CaseCustQuery.DocNo,CaseCustQuery.Version,CaseCustQueryVersion.IdNo", string strSortDirection = "asc")
        {
            #region 查詢條件記錄在cookie
            HttpCookie modelCookie = new HttpCookie("CaseCustQueryCookie");
            modelCookie.Values.Add("FileGovenment", model.FileGovenment);
            modelCookie.Values.Add("DocNo", model.DocNo);
            modelCookie.Values.Add("FileDateStart", model.FileDateStart);
            modelCookie.Values.Add("FileDateEnd", model.FileDateEnd);
            modelCookie.Values.Add("SearchProgram", model.SearchProgram);
            modelCookie.Values.Add("FileNo", model.FileNo);
            modelCookie.Values.Add("Result", model.Result);
            modelCookie.Values.Add("DateStart", model.DateStart);
            modelCookie.Values.Add("DateEnd", model.DateEnd);
            modelCookie.Values.Add("Status", model.Status);
            modelCookie.Values.Add("CurrentPage", pageNum.ToString());
            Response.Cookies.Add(modelCookie);
            #endregion

            #region 日期格式轉換
            // 來文日期 起
            if (!string.IsNullOrEmpty(model.FileDateStart))
            {
                model.FileDateStart = UtlString.FormatDateTwStringToAd(model.FileDateStart);
            }
            // 來文日期 訖
            if (!string.IsNullOrEmpty(model.FileDateEnd))
            {
                model.FileDateEnd = UtlString.FormatDateTwStringToAd(model.FileDateEnd);
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
        #endregion

        #region 自定義方法
        #region 下拉選單
        /// <summary>
        /// 查詢條件清單綁值
        /// </summary>
        public void BindList()
        {
            // 查詢項目
            ViewBag.SearchProgramList = BindSearchProgramList();

            // 拋查結果
            ViewBag.ResultList = BindResultList();

            // 審核狀態
            ViewBag.StatusList = BindStatusList();
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

        /// <summary>
        /// 拋查結果
        /// </summary>
        /// <returns></returns>
        public SelectList BindResultList()
        {
            // 選項集合
            IList<SelectListItem> lst = new List<SelectListItem>();

            lst.Add(new SelectListItem() { Value = "03", Text = "成功" });
            lst.Add(new SelectListItem() { Value = "07", Text = "重查成功" });

            return new SelectList(lst, "Value", "Text");
        }

        /// <summary>
        /// 審核狀態
        /// </summary>
        /// <returns></returns>
        public SelectList BindStatusList()
        {
            // 選項集合
            IList<SelectListItem> lst = new List<SelectListItem>();

            lst.Add(new SelectListItem() { Value = "Y", Text = "Y" });
            lst.Add(new SelectListItem() { Value = "N", Text = "N" });

            return new SelectList(lst, "Value", "Text");
        }

        #endregion

        /// <summary>
        /// 實際查詢動作
        /// </summary>
        /// <param name="model">實體類</param>
        /// <param name="strSortExpression">排序欄位</param>
        /// <param name="strSortDirection">排序方式</param>
        /// <param name="pageNum">當前頁面</param>
        /// <returns></returns>
        public CaseCustViewModel SearchList(CaseCustCondition model, string strSortExpression, string strSortDirection, int pageNum = 1)
        {
            // 查詢清單資料
            IList<CaseCustQuery> result = _CaseCustManagerBIZ.GetManagerQueryList(model, pageNum, strSortExpression, strSortDirection);

            // 實體類賦值
            var viewModel = new CaseCustViewModel()
            {
                CaseCustCondition = model,
                CaseCustQueryList = result,
            };

            // 資料清單之每頁資料數、當前頁頁碼、資料總筆數賦值
            viewModel.CaseCustCondition.PageSize = _CaseCustManagerBIZ.PageSize;
            viewModel.CaseCustCondition.CurrentPage = _CaseCustManagerBIZ.PageIndex;
            viewModel.CaseCustCondition.TotalItemCount = _CaseCustManagerBIZ.DataRecords;
            viewModel.CaseCustCondition.SortExpression = strSortExpression;
            viewModel.CaseCustCondition.SortDirection = strSortDirection;

            return viewModel;
        }

        /// <summary>
        /// 審核完成
        /// </summary>
        /// <param name="strKey"></param>
        /// <param name="flag">2:主管放行明細頁面的審核完成事件,1:主管放行也買你審核按鈕事件</param>
        /// <returns></returns>
        public ActionResult AuditFinish(string strKey, string flag)
        {
            bool returnResult = _CaseCustManagerBIZ.AuditFinishs(strKey, flag, LogonUser.Account, LogonUser.Name);

            return Json(returnResult, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 上傳
        /// </summary>
        /// <param name="strKey">主檔主鍵</param>
        /// <param name="strStatus">主檔案件狀態</param>
        /// <param name="strAuditStatus">主檔審核狀態</param>
        /// <returns></returns>
        public ActionResult Upload(string strKey, string strStatus, string strAuditStatus)
        {
            bool returnResult = _CaseCustManagerBIZ.Upload(strKey, strAuditStatus, LogonUser.Account);

            return Json(returnResult, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region 回文資料

        /// <summary>
        /// 回文資料
        /// </summary>
        /// <param name="strPk">主鍵</param>
        /// <returns></returns>
        public FileResult ReturnsView(string strPk)
        {

            string strFilePath = ConfigurationManager.AppSettings["txtFilePath"] + @"\";

            IList<CaseCustQuery> lists = _CaseCustManagerBIZ.GedReturnFile(strPk);

            string fileName = Guid.NewGuid().ToString();

            #region 將有關回文放到list

            List<string> strList = new List<string>();

            // 將有關回文放到list
            foreach (CaseCustQuery item in lists)
            {
                // 回文檔名稱（存款帳戶開戶資料）
                string strROpenFileName = strFilePath + item.PDFFileName;

                strList.Add(strROpenFileName);
            }

            #endregion

            // 臨時文件
            string TempPath = Server.MapPath("~/Template/Template/");

            if (!Directory.Exists(TempPath))
            {
                Directory.CreateDirectory(TempPath);
            }

            #region 產生pdf

            string pdfFile = TempPath + fileName + ".pdf";

            mergePDFFiles(pdfFile, strList);

            return File(pdfFile, "application/pdf", fileName + ".pdf");

            #endregion
        }

        /// <summary>
        /// 將多個PDF文件合併為一個PDF
        /// </summary>
        /// <param name="outMergeFile">outMergeFile是pdf文件合并后的输出路径</param>
        /// <param name="lstFile">lstFile里存放要进行合并的pdf文件的路径</param>
        public void mergePDFFiles(string outMergeFile, List<string> lstFile)
        {
            if (!string.IsNullOrEmpty(outMergeFile))
            {
                try
                {
                    PdfReader reader;
                    Document document = new Document();

                    // 創建pdf文檔
                    PdfWriter writer = PdfWriter.GetInstance(document, new FileStream(outMergeFile, FileMode.Create));

                    document.Open();
                    PdfContentByte cb = writer.DirectContent;
                    PdfImportedPage newPage;

                    OpenFileStream webService = new OpenFileStream();

                    webService.Url = ConfigurationManager.AppSettings["ServiceURL"];

                    byte[] stream = null;

                    // 循環需要合併的pdf文件
                    for (int i = 0; i < lstFile.Count; i++)
                    {
                        // 去的pdf路徑
                        string newpath = lstFile[i];
                        //reader = new PdfReader(newpath);

                        stream = webService.OpenFile(newpath);

                        if (stream != null)
                        {
                           reader = new PdfReader(stream);

                           // 計算pdf文件的頁數
                           int iPageNum = reader.NumberOfPages;
                           int startPage = 1;
                           int rotation;

                           // 從第一頁開始倒入文件數據
                           while (startPage <= iPageNum)
                           {
                               document.SetPageSize(reader.GetPageSizeWithRotation(startPage));
                               document.NewPage();
                               newPage = writer.GetImportedPage(reader, startPage);

                               // 獲取每一頁pdf文件的rotation 
                               rotation = reader.GetPageRotation(startPage);

                               // 根據每一頁的rotation重置寬高，否則都按首頁寬高合併可能會造成信息丟失
                               switch (rotation)
                               {
                                   case 90:
                                       cb.AddTemplate(newPage, 0, -1f, 1f, 0, 0, reader.GetPageSizeWithRotation(startPage).Height);
                                       break;

                                   case 180:
                                       cb.AddTemplate(newPage, -1f, 0, 0, -1f, reader.GetPageSizeWithRotation(startPage).Width, reader.GetPageSizeWithRotation(startPage).Height);
                                       break;

                                   case 270:
                                       cb.AddTemplate(newPage, 0, 1f, -1f, 0, reader.GetPageSizeWithRotation(startPage).Width, 0);
                                       break;

                                   default:
                                       cb.AddTemplate(newPage, 1f, 0, 0, 1f, 0, 0);
                                       break;
                               }

                               startPage++;
                           }
                        }
                    }

                    document.Close();
                }
                catch (Exception ex)
                {
                    outMergeFile = string.Empty;
                    throw ex;
                }
            }
        }

        #endregion

    }
}