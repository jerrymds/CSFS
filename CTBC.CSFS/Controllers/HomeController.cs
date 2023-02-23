/// <summary>
/// 程式說明:Home Controller- Iframe
/// 維護部門:資訊管理處
/// CSFS Version:v3.0
/// 中國信託銀行 版權所有  ©  All Rights Reserved. /// 
/// </summary>

using System;
using System.Globalization;
using System.Web.Mvc;
using System.Web;
using CTBC.CSFS.Pattern;
using CTBC.CSFS.Resource;
using CTBC.CSFS.Models;//20140713 horace
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CTBC.CSFS.BussinessLogic;
using System.Configuration;

//20140713 horace

namespace CTBC.CSFS.Controllers
{
    public class HomeController : AppController
    {
        /// <summary>
        /// 主頁
        /// </summary>
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// 頭部顯示
        /// </summary>
        public ActionResult Header()
        {
            return View();
        }

        /// <summary>
        /// 住區塊顯示
        /// </summary>
        public ActionResult Main()
        {          
            return View();
        }

        ///// <summary>
        ///// 樹
        ///// </summary>
        //public ActionResult MenuTree()
        //{
        //    return View();
        //}

        #region Add by Ge.Song
        /// <summary>
        /// 下載附件
        /// </summary>
        /// <param name="uploadkind"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult DownFile(string uploadkind, int id)
        {
            string filePath = "";
            string fileName = "";
            string fileServerName = "";
            if (id < 0)
                return null;

            if (uploadkind == Uploadkind.CaseAttach)
            {
                CaseAttachment attach = new CaseAttachmentBIZ().GetAttachmentInfo(id);
                if (attach == null)
                    return null;
                filePath = attach.AttachmentServerPath;
                fileServerName = attach.AttachmentServerName;
                fileName = attach.AttachmentName;
                //CSFSURL.attDownload csDown = new CSFSURL.attDownload();
                //string returnFile = csDown.UrlDownloadFile(filePath, fileServerName);
                //if (returnFile != null)
                //{
                //    byte[] file = Convert.FromBase64String(returnFile);
                //    return File(file, "application/txt", fileName);
                //}
                //else
                //{
                //    return Content("<script>alert('" + Lang.csfs_no_data + "');</script>");
                //}
            }
            else if (uploadkind == Uploadkind.LendAttach)
            {
                LendAttachment lendAttach = new LendAttachmentBIZ().GetAttachmentInfo(id);
                if (lendAttach == null)
                    return null;

                filePath = lendAttach.LendAttachServerPath;
                fileServerName = lendAttach.LendAttachServerName;
                fileName = lendAttach.LendAttachName;
            }
            else if (uploadkind == Uploadkind.MeetingResultAttachment)
            {
                MeetingResultDetail attach = new MeetingResultDetailBIZ().GetAttachDetailInfo(id);
                if (attach == null)
                    return null;
                filePath = attach.AttatchDetailServerPath;
                fileServerName = attach.AttatchDetailServerName;
                fileName = attach.AttatchDetailName;
            }
            else if (uploadkind == Uploadkind.WarnAttach)
            {
                WarningAttachment attach = new CaseWarningBIZ().GetAttachDetailInfo(id);
                if (attach == null)
                    return null;
                filePath = attach.AttachmentServerPath;
                fileServerName = attach.AttachmentServerName;
                fileName = attach.AttachmentName;
            }
            //下載聯防附件
            else if(uploadkind == Uploadkind.WarnFraudAttach)
            {
                WarningFraudAttach attach = new WarningFraudBIZ().GetAttachInfo(id);
                if (attach == null)
                    return null;
                filePath = attach.AttachmentServerPath;
                fileServerName = attach.AttachmentServerName;
                fileName = attach.AttachmentName;
            }
            else if (uploadkind.Length>20)
            {
                int at = uploadkind.IndexOf("|");
                string strCaseId = uploadkind.Substring(0, at);
                string strFileName = uploadkind.Substring(at+1, (uploadkind.Length -at-1));
                CaseAccountBiz caseAccount = new CaseAccountBiz();
                ImportEDocBiz CaseEdocFile = new ImportEDocBiz();
                CaseEdocFile caseEdocFile = caseAccount.OpenHisDoc(strCaseId, strFileName,id);
                //CaseEdocFile caseEdocFile = caseAccount.OpenTxtDoc(strCaseId, strFileName);
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

                //if (caseEdocFile != null)
                //{
                //    byte[] file = caseEdocFile.FileObject;
                //    text = System.Text.Encoding.UTF8.GetString(file);
                //}
                //string ReturnMsg = string.IsNullOrEmpty(text) ? Lang.csfs_txtdocnotfound : text;
                //return PartialView("TxtOpen", new CaseMaster { Memo = ReturnMsg });
            }
            else
            {
                return null;
            }
            //OpenFileStream webService = new OpenFileStream();
            //webService.Url = ConfigurationManager.AppSettings["ServiceURL"];

            //byte[] stream = null;

            //    stream = webService.OpenFile(newpath);

                string absoluFilePath = Path.Combine(Server.MapPath(filePath), fileServerName);
            if (!System.IO.File.Exists(absoluFilePath))
            {
                //return Content("<script>alert('" + Lang.csfs_FileNotFound +"');</script>");
                CSFSURL.attDownload csDown = new CSFSURL.attDownload();
                string returnFile = csDown.UrlDownloadFile(filePath, fileServerName);
                if (returnFile != null)
                {
                    byte[] file = Convert.FromBase64String(returnFile);
                    return File(file, "application/txt", fileName);
                }
                else
                {
                    return Content("<script>alert('" + Lang.csfs_no_data + "');</script>");
                }
            }
            return File(new FileStream(absoluFilePath, FileMode.Open), "application/octet-stream", Server.UrlEncode(fileName));
        }

        public ActionResult HistoryDownFile(string uploadkind, int id)
        {
            string filePath = "";
            string fileName = "";
            string fileServerName = "";
            if (id < 0)
                return null;

            if (uploadkind == Uploadkind.CaseAttach)
            {
                HistoryCaseAttachment attach = new CaseAttachmentBIZ().GetHistoryAttachmentInfo(id);
                if (attach == null)
                    return null;
                filePath = attach.AttachmentServerPath;
                fileServerName = attach.AttachmentServerName;
                fileName = attach.AttachmentName;
                //CSFSURL.attDownload csDown = new CSFSURL.attDownload();
                //string returnFile = csDown.UrlDownloadFile(filePath, fileServerName);
                //if (returnFile != null)
                //{
                //    byte[] file = Convert.FromBase64String(returnFile);
                //    return File(file, "application/txt", fileName);
                //}
                //else
                //{
                //    return Content("<script>alert('" + Lang.csfs_no_data + "');</script>");
                //}
            }
            else if (uploadkind == Uploadkind.LendAttach)
            {
                HistoryLendAttachment lendAttach = new LendAttachmentBIZ().GetHistoryAttachmentInfo(id);
                if (lendAttach == null)
                    return null;

                filePath = lendAttach.LendAttachServerPath;
                fileServerName = lendAttach.LendAttachServerName;
                fileName = lendAttach.LendAttachName;
            }
            else if (uploadkind == Uploadkind.MeetingResultAttachment)
            {
                MeetingResultDetail attach = new MeetingResultDetailBIZ().GetAttachDetailInfo(id);
                if (attach == null)
                    return null;
                filePath = attach.AttatchDetailServerPath;
                fileServerName = attach.AttatchDetailServerName;
                fileName = attach.AttatchDetailName;
            }
            else if (uploadkind == Uploadkind.WarnAttach)
            {
                WarningAttachment attach = new CaseWarningBIZ().GetAttachDetailInfo(id);
                if (attach == null)
                    return null;
                filePath = attach.AttachmentServerPath;
                fileServerName = attach.AttachmentServerName;
                fileName = attach.AttachmentName;
            }
            else if (uploadkind.Length > 20)
            {
                int at = uploadkind.IndexOf("|");
                string strCaseId = uploadkind.Substring(0, at);
                string strFileName = uploadkind.Substring(at + 1, (uploadkind.Length - at - 1));
                HistoryCaseAccountBiz caseAccount = new HistoryCaseAccountBiz();
                //ImportEDocBiz CaseEdocFile = new ImportEDocBiz();
                HistoryCaseEdocFile caseEdocFile = caseAccount.OpenHisDoc(strCaseId, strFileName, id);
                //CaseEdocFile caseEdocFile = caseAccount.OpenTxtDoc(strCaseId, strFileName);
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

                //if (caseEdocFile != null)
                //{
                //    byte[] file = caseEdocFile.FileObject;
                //    text = System.Text.Encoding.UTF8.GetString(file);
                //}
                //string ReturnMsg = string.IsNullOrEmpty(text) ? Lang.csfs_txtdocnotfound : text;
                //return PartialView("TxtOpen", new CaseMaster { Memo = ReturnMsg });
            }
            else
            {
                return null;
            }
            //OpenFileStream webService = new OpenFileStream();
            //webService.Url = ConfigurationManager.AppSettings["ServiceURL"];

            //byte[] stream = null;

            //    stream = webService.OpenFile(newpath);

            string absoluFilePath = Path.Combine(Server.MapPath(filePath), fileServerName);
            if (!System.IO.File.Exists(absoluFilePath))
            {
                //return Content("<script>alert('" + Lang.csfs_FileNotFound +"');</script>");
                CSFSURL.attDownload csDown = new CSFSURL.attDownload();
                string returnFile = csDown.UrlDownloadFile(filePath, fileServerName);
                if (returnFile != null)
                {
                    byte[] file = Convert.FromBase64String(returnFile);
                    return File(file, "application/txt", fileName);
                }
                else
                {
                    return Content("<script>alert('" + Lang.csfs_no_data + "');</script>");
                }
            }
            return File(new FileStream(absoluFilePath, FileMode.Open), "application/octet-stream", Server.UrlEncode(fileName));
        }

        /// <summary>
        /// 綁定來文機關
        /// </summary>
        /// <param name="govKind"></param>
        /// <returns></returns>
        public JsonResult GetGovNameByGoveKind(string govKind)
        {
            var biz = new GovAddressBIZ();
            var list = biz.GetEnabledGovAddrByGovKind(govKind);
            List<string> strList = new List<string>();
            if (list != null && list.Any())
            {
                strList.AddRange(list.Select(item => item.GovName));
            }
            return Json(strList);
        }

        /// <summary>
        /// 進入選擇QueryGovAddress畫面
        /// </summary>
        /// <param name="govKind"></param>
        /// <param name="govName"></param>
        /// <returns></returns>
        public ActionResult QueryGovAddress(string govKind, string govName)
        {
            return View();
        }
        /// <summary>
        /// 查詢結果grid顯示
        /// </summary>
        /// <param name="govKind"></param>
        /// <param name="govName"></param>
        /// <returns></returns>
        public ActionResult _QueryGovAddressResult(string govKind, string govName)
        {
            List<GovAddress> listRtn = new GovAddressBIZ().QueryGovAddrByGovKindAndName(govKind, govName);
            return View(listRtn);
        }

        public ActionResult BuildSimpleExcel()
        {
            CaseHistoryBIZ historyBiz = new CaseHistoryBIZ();
            MemoryStream ms = historyBiz.SimpleSampleExcel(new Guid("51383082-4158-44A5-91C7-18B5F1C2F456"));
            if (ms != null && ms.Length > 0)
            {
                Response.ClearContent();
                Response.ClearHeaders();
            }
            else
            {
                ms = new MemoryStream();
            }
            string fileName = "simpleExcel_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xls";
            return File(ms.ToArray(), "application/vnd.ms-excel", fileName);
        }
        #endregion
        #region 20150108 horace 弱掃 mark
        ///// <summary>
        ///// 修改语言
        ///// </summary>
        ///// <param name="lang">参数 lang 为用户要使用的语言</param>
        ///// <param name="returnUrl">参数 returnUrl 用户点击修改语言时停留的页面</param>
        ///// <returns></returns>
        //public ActionResult ChangeCulture(string lang, string returnUrl)
        //{
        //    Session["Culture"] = new CultureInfo(lang);
        //    return Redirect(returnUrl);
        //}
        #endregion
    }
}
