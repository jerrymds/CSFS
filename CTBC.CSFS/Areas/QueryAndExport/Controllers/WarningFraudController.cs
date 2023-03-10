using System;
using System.Data;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using CTBC.FrameWork.Platform;
using CTBC.CSFS.Filter;
using CTBC.CSFS.Pattern;
using CTBC.CSFS.BussinessLogic;
using CTBC.CSFS.Models;
using CTBC.CSFS.ViewModels;
using CTBC.FrameWork.Util;
using ICSharpCode.SharpZipLib.Zip;


namespace CTBC.CSFS.Areas.QueryAndExport.Controllers
{
    public class WarningFraudController : AppController
    {
        PARMCodeBIZ parm = new PARMCodeBIZ();
        WarningFraudBIZ wqBiz;

        // GET: QueryAndExport/WarningFraud
        [RootPageFilter]
        public ActionResult Index()
        {
            //日期區間設定
            if (!AppCache.InCache("CheckDays"))
            {
                var checkDaysList = parm.GetCodeByCodeType("CheckDays");
                if(checkDaysList.Count > 0)
                {
                    AppCache.Add(checkDaysList.First().CodeNo, "CheckDays");
                }
                else
                {
                    AppCache.Add("90", "CheckDays");
                }
            }

            var checkDays = AppCache.Get("CheckDays").ToString();

            //表單預設值
            WarningFraud model = new WarningFraud();
            model.CreateDateS = DateTime.Now.AddDays((0 - int.Parse(checkDays))).AddYears(-1911).ToString("yyy/MM/dd");
            model.CreateDateE = DateTime.Now.AddYears(-1911).ToString("yyy/MM/dd");
            model.CurrentPage = 1;
            model.PageSize = 10;
            model.SortExpression = "CreatedDate,COL_165CASE,COL_ACCOUNT2";
            BindDropDownList();
            return View(model);
        }

        /// <summary>
        /// 綁定下拉選單
        /// </summary>
        public void BindDropDownList(string unitSel = "")
        {
            var unitList = parm.GetCodeByCodeType("NotificationUnit").ToList();
            unitList.ForEach(m => m.CodeNo = m.CodeDesc);
            unitList.Insert(0, new PARMCode { CodeNo = "", CodeDesc = "請選擇" });
            ViewBag.Unit = new SelectList(unitList, "CodeNo", "CodeDesc", unitSel);
        }

        [HttpPost]
        public ActionResult _QueryResult(WarningFraud model)
        {
            try
            {
                var vmodel = SearchList(model);
                return PartialView("_QueryResult", vmodel);
            }
            catch(Exception ex)
            {
                WriteExceptionLog(ex);
                return Json(new JsonReturn { ReturnCode = "0", ReturnMsg = ex.Message });
            }
        }

        /// <summary>
        /// 查詢
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public WarningFraudViewModel SearchList(WarningFraud model)
        {
            wqBiz = new WarningFraudBIZ
            { 
                PageIndex = model.CurrentPage,
                PageSize = model.PageSize,
                SortExpression = model.SortExpression
            };

            //民國年轉西元年
            if (!string.IsNullOrWhiteSpace(model.CreateDateS))
            {
                model.CreateDateS = UtlString.FormatDateTwStringToAd(model.CreateDateS);
            }
            if (!string.IsNullOrWhiteSpace(model.CreateDateE))
            {
                model.CreateDateE = UtlString.FormatDateTwStringToAd(model.CreateDateE);
            }

            var list = wqBiz.GetQueryList(model).ToList();
            list.ForEach(m => m.CreatedDate = UtlString.FormatDateTw(m.CreatedDate));

            var viewModel = new WarningFraudViewModel()
            {
                CurrentPage = model.CurrentPage,
                PageSize = model.PageSize,
                SortExpression = model.SortExpression,
                TotalItemCount = wqBiz.TotalDataCount,
                WarningFraud = model,
                WarningFraudList = list,
            };

            return viewModel;
        }

        /// <summary>
        /// 新增頁
        /// </summary>
        /// <returns></returns>
        public ActionResult CreateWarn()
        {
            //上傳檔案大小限制
            ViewBag.UploadMaxLength = ConfigurationManager.AppSettings["UploadMaxLength"] ?? "10485760";
            ViewBag.NowPage = "CreateWarn";
            BindDropDownList("客服中心");
            WarningFraud model = new WarningFraud
            {
                CreatedDate = UtlString.FormatDateTw(DateTime.Now.ToString("yyyy/MM/dd")),
                WarningFraudAttach = new List<WarningFraudAttach>()
            };
            return View("Edit", model);
        }

        /// <summary>
        /// 新增案件
        /// </summary>
        /// <param name="model"></param>
        /// <param name="attachFile"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult CreateWarn(WarningFraud model, HttpPostedFileBase attachFile)
        {
            var jsonReturn = new JsonReturn();
            
            try
            {
                wqBiz = new WarningFraudBIZ();
                //檢核165編號是否重複
                var isCaseNoExists = wqBiz.Check_165CaseNo(model.COL_165CASE);
                if(isCaseNoExists)
                {
                    jsonReturn.ReturnCode = "0";
                    jsonReturn.ReturnMsg = "165編號已存在";
                    return Json(jsonReturn);
                }

                #region 儲存檔案
                if(attachFile != null)
                {
                    model.WarningFraudAttach = UploadFile(attachFile, model.COL_165CASE);
                }
                #endregion

                //民國年轉換成西元年
                if(!string.IsNullOrWhiteSpace(model.CreatedDate))
                {
                    model.CreatedDate = UtlString.FormatDateTwStringToAd(model.CreatedDate);
                }

                model.CreatedUser = LogonUser.Account;
                model.ModifiedUser = LogonUser.Account;

                var result = wqBiz.CreateWarningFraud(model);

                if(result)
                {
                    jsonReturn.ReturnCode = "1";
                    jsonReturn.ReturnMsg = "新增成功";
                }
                else
                {
                    throw new Exception("新增失敗");
                }
            }
            catch(Exception ex)
            {
                DelFile(model.WarningFraudAttach);
                jsonReturn.ReturnCode = "0";
                jsonReturn.ReturnMsg = ex.Message;
                WriteExceptionLog(ex);
            }
            return Json(jsonReturn);
        }

        /// <summary>
        /// 修改頁面
        /// </summary>
        /// <param name="no"></param>
        /// <returns></returns>
        public ActionResult EditWarn(int no)
        {
            try
            {
                //上傳檔案大小限制
                ViewBag.UploadMaxLength = ConfigurationManager.AppSettings["UploadMaxLength"] ?? "10485760";

                if (no <= 0)
                {
                    throw new Exception("取得聯防案件失敗");
                }

                wqBiz = new WarningFraudBIZ();
                var vmodel = wqBiz.GetWarningFraud(no);
                if(vmodel != null)
                {
                    vmodel.COL_165CASE_OLD = vmodel.COL_165CASE;

                    if(vmodel.AttachmentId > 0)
                    {
                        vmodel.WarningFraudAttach = wqBiz.GetAttachList(vmodel.AttachmentId);
                    }

                    if (vmodel.WarningFraudAttach == null)
                    {
                        vmodel.WarningFraudAttach = new List<WarningFraudAttach>();
                    }

                    BindDropDownList(vmodel.Unit);
                    ViewBag.NowPage = "EditWarn";
                }
                else
                {
                    throw new Exception("取得聯防案件失敗");
                }
                return View("Edit", vmodel);
            }
            catch(Exception ex)
            {
                WriteExceptionLog(ex);
                return Json(new JsonReturn { ReturnCode = "0", ReturnMsg = ex.Message });
            }
        }

        /// <summary>
        /// 修改案件
        /// </summary>
        /// <param name="model"></param>
        /// <param name="attachFile"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult EditWarn(WarningFraud model, HttpPostedFileBase attachFile)
        {
            var jsonReturn = new JsonReturn();

            try
            {
                wqBiz = new WarningFraudBIZ();
                //檢核165CASE是否重複
                if(model.COL_165CASE != model.COL_165CASE_OLD)
                {
                    var isCaseNoExists = wqBiz.Check_165CaseNo(model.COL_165CASE);
                    if (isCaseNoExists)
                    {
                        jsonReturn.ReturnCode = "0";
                        jsonReturn.ReturnMsg = "165編號已存在";
                        return Json(jsonReturn);
                    }
                }

                #region 儲存檔案
                if (attachFile != null)
                {
                    model.WarningFraudAttach = UploadFile(attachFile, model.COL_165CASE);
                }
                #endregion

                //民國年轉換成西元年
                if (!string.IsNullOrWhiteSpace(model.CreatedDate))
                {
                    model.CreatedDate = UtlString.FormatDateTwStringToAd(model.CreatedDate);
                }

                model.ModifiedUser = LogonUser.Account;

                var result = wqBiz.EditWarningFraud(model);
                if (result)
                {
                    jsonReturn.ReturnCode = "1";
                    jsonReturn.ReturnMsg = "修改成功";
                }
                else
                {
                    throw new Exception("修改失敗");
                }
            }
            catch (Exception ex)
            {
                DelFile(model.WarningFraudAttach);
                jsonReturn.ReturnCode = "0";
                jsonReturn.ReturnMsg = ex.Message;
                WriteExceptionLog(ex);
            }
            return Json(jsonReturn);
        }

        /// <summary>
        /// 匯出Excel
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Export(WarningFraud model)
        {
            wqBiz = new WarningFraudBIZ();
            if (!string.IsNullOrWhiteSpace(model.CreateDateS))
            {
                model.CreateDateS = UtlString.FormatDateTwStringToAd(model.CreateDateS);
            }
            if (!string.IsNullOrWhiteSpace(model.CreateDateE))
            {
                model.CreateDateE = UtlString.FormatDateTwStringToAd(model.CreateDateE);
            }
            if (model.Unit == "請選擇")
            {
                model.Unit = "";
            }

            MemoryStream ms = new MemoryStream();
            ms = wqBiz.Export_Excel(model);
            string fileName = "聯防案件編輯及查詢_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xls";
            if(ms != null && ms.Length > 0)
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
        /// 刪除一筆資料
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult DeleteWarn(WarningFraud model)
        {
            var jsonReturn = new JsonReturn();
            try
            {
                if(model.No == 0)
                {
                    throw new Exception("刪除失敗");
                }

                wqBiz = new WarningFraudBIZ();
                var result = wqBiz.DeleteWarningFraud(model.No);
                if(result)
                {
                    jsonReturn.ReturnCode = "1";
                    jsonReturn.ReturnMsg = "刪除成功";
                }
                else
                {
                    throw new Exception("刪除失敗");
                }
            }
            catch(Exception ex)
            {
                jsonReturn.ReturnCode = "0";
                jsonReturn.ReturnMsg = "刪除失敗";
                WriteExceptionLog(ex);
            }
            return Json(jsonReturn);
        }

        /// <summary>
        /// 刪除附件
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult DeleteAttatch(int attachmentId)
        {
            var jsonReturn = new JsonReturn { ReturnCode = "1", ReturnMsg = "刪除成功" };
            try
            {
                wqBiz = new WarningFraudBIZ();
                var amodel = wqBiz.GetAttachInfo(attachmentId);
                if(amodel != null)
                {
                    #region 刪除實體檔
                    DelFile(new List<WarningFraudAttach> { amodel });
                    #endregion

                    #region 刪除資料庫
                    var result = wqBiz.DelAttach(attachmentId);
                    if(result == false)
                    {
                        jsonReturn.ReturnCode = "0";
                        jsonReturn.ReturnMsg = "刪除失敗";
                    }
                    #endregion
                }
            }
            catch (Exception ex)
            {
                jsonReturn.ReturnCode = "0";
                jsonReturn.ReturnMsg = ex.Message;
                WriteExceptionLog(ex);
            }
            return Json(jsonReturn);
        }

        /// <summary>
        /// 下載檔案
        /// </summary>
        /// <param name="warningFraudNo"></param>
        /// <returns></returns>
        public ActionResult Download(int warningFraudNo)
        {
            try
            {
                wqBiz = new WarningFraudBIZ();
                var warningFraud = wqBiz.GetWarningFraud(warningFraudNo);
                var dwnFileName = string.Format("{0}_{1}.zip", warningFraud.COL_165CASE, DateTime.Now.ToString("yyyyMMddHHmmss"));
                var attachs = wqBiz.GetAttachList(warningFraudNo);
                using (MemoryStream memStream = new MemoryStream())
                {
                    using (ZipOutputStream zipOutStream = new ZipOutputStream(memStream))
                    {
                        attachs.ForEach(f =>
                        {
                            AddFileToZip(zipOutStream, f);
                        });
                    }
                    return File(memStream.ToArray(), "application/octet-stream", dwnFileName);
                }
            }
            catch(Exception ex)
            {
                WriteExceptionLog(ex);
                Response.StatusCode = 400;
                return Content("No Data");
            }
        }

        /// <summary>
        /// 下載附件打包成 ZIP
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="attach"></param>
        private void AddFileToZip(ZipOutputStream stream, WarningFraudAttach attach)
        {
            byte[] buffer = new byte[4096];
            string absFilePath = Path.Combine(Server.MapPath(attach.AttachmentServerPath), attach.AttachmentServerName);
            if(System.IO.File.Exists(absFilePath))
            {
                ZipEntry entry = new ZipEntry(attach.AttachmentName);
                stream.PutNextEntry(entry);
                using (FileStream fs = System.IO.File.OpenRead(absFilePath))
                {
                    int readLength;
                    do
                    {
                        readLength = fs.Read(buffer, 0, buffer.Length);
                        if(readLength > 0)
                        {
                            stream.Write(buffer, 0, readLength);
                        }
                    } 
                    while (readLength > 0);
                }
            }
            else
            {
                //透過 WebService 去另一台下載檔案
                CSFSURL.attDownload csDown = new CSFSURL.attDownload();
                string fileBase64String = csDown.UrlDownloadFile(attach.AttachmentServerPath, attach.AttachmentServerName);
                if(!string.IsNullOrWhiteSpace(fileBase64String))
                {
                    ZipEntry entry = new ZipEntry(attach.AttachmentName);
                    stream.PutNextEntry(entry);
                    byte[] fileBytes = Convert.FromBase64String(fileBase64String);
                    using (MemoryStream ms = new MemoryStream(fileBytes))
                    {
                        int readLength;
                        do
                        {
                            readLength = ms.Read(buffer, 0, buffer.Length);
                            if(readLength > 0)
                            {
                                stream.Write(buffer, 0, readLength);
                            }
                        }
                        while (readLength > 0);
                    }
                }
                else
                {
                    //第二台 Web Server 找不到檔案到 Batch Server 下載
                    string serverPath = ConfigurationManager.AppSettings["WarningAttachFile"].ToString();
                    WebService.OpenStream.OpenFileStream openFileStream = new WebService.OpenStream.OpenFileStream();
                    string fileName = Path.Combine(serverPath, attach.AttachmentServerPath.Replace("~/", "").TrimEnd(), attach.AttachmentServerName);
                    fileName = fileName.Replace("/", "\\");
                    byte[] fileBytes = openFileStream.OpenFile(fileName);
                    if (fileBytes != null)
                    {
                        ZipEntry entry = new ZipEntry(attach.AttachmentName);
                        stream.PutNextEntry(entry);
                        using (MemoryStream ms = new MemoryStream(fileBytes))
                        {
                            int readLength;
                            do
                            {
                                readLength = ms.Read(buffer, 0, buffer.Length);
                                if (readLength > 0)
                                {
                                    stream.Write(buffer, 0, readLength);
                                }
                            }
                            while (readLength > 0);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 儲存上傳附件
        /// </summary>
        /// <param name="attachFile"></param>
        /// <param name="COL_165CASE"></param>
        /// <returns></returns>
        private List<WarningFraudAttach> UploadFile(HttpPostedFileBase attachFile, string COL_165CASE)
        {
            string newName = string.Format("{0}_{1}{2}", COL_165CASE, DateTime.Now.ToString("yyyyMMddHHmmssfff"), Path.GetExtension(attachFile.FileName));
            string serverPath = Path.Combine("~/" + ConfigurationManager.AppSettings["UploadFolder"], "WarningFraud", DateTime.Now.ToString("yyyyMM"));
            string realPath = Server.MapPath(serverPath);
            if (!UtlFileSystem.FolderIsExist(realPath))
                UtlFileSystem.CreateFolder(realPath);

            string fileName = Path.Combine(realPath, newName);
            attachFile.SaveAs(fileName);

            var aModel = new WarningFraudAttach
            {
                AttachmentName = Path.GetFileName(attachFile.FileName),
                AttachmentServerName = newName,
                AttachmentServerPath = serverPath,
                CreatedUser = LogonUser.Account
            };

            return new List<WarningFraudAttach> { aModel };
        }

        private void DelFile(List<WarningFraudAttach> attach)
        {
            if (attach.Count > 0)
            {
                attach.ForEach(m =>
                {
                    if(m.AttachmentServerPath.Contains("~/File"))
                    {
                        string serverPath = ConfigurationManager.AppSettings["WarningAttachFile"].ToString();
                        WebService.OpenStream.OpenFileStream openFileStream = new WebService.OpenStream.OpenFileStream();
                        string fileName = Path.Combine(serverPath, m.AttachmentServerPath.Replace("~/", ""), m.AttachmentServerName);
                        openFileStream.DeleteFile(fileName);
                    }
                    else
                    {
                        string path = System.IO.Path.Combine(Server.MapPath(m.AttachmentServerPath), m.AttachmentServerName);
                        if (System.IO.File.Exists(path))
                        {
                            System.IO.File.Delete(path);
                        }
                    }
                });
            }
        }
    }
}