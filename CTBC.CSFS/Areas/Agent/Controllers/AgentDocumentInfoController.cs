using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using CTBC.CSFS.BussinessLogic;
using CTBC.CSFS.Models;
using CTBC.CSFS.Pattern;
using CTBC.CSFS.Resource;
using CTBC.CSFS.ViewModels;
using CTBC.FrameWork.Util;

namespace CTBC.CSFS.Areas.Agent.Controllers
{
    public class AgentDocumentInfoController : AppController
    {
        CaseMasterBIZ casemaster = new CaseMasterBIZ();
        CaseSeizureViewModel caseview = new CaseSeizureViewModel();
        PARMCodeBIZ parm = new PARMCodeBIZ();
        CaseAttachmentBIZ attachment = new CaseAttachmentBIZ();
        CaseObligorBIZ obligor = new CaseObligorBIZ();
        CaseAccountBiz caseAccount = new CaseAccountBiz();
        ImportEDocBiz CaseEdocFile = new ImportEDocBiz();

        public static string pageFromName = "";

        // GET: Agent/AgentDocumentInfo
        public ActionResult Index(Guid caseId, String pageFrom = "")
        {
            ViewBag.CaseId = caseId;
			if (pageFrom == "1")
			{
				pageFromName = "1";
				ViewBag.PageFrom = "1";
			}
			else if (pageFrom == "2")
			{
				pageFromName = "2";
				ViewBag.PageFrom = "2";
			}
            InitDropdownListOptions();              //* 綁定頁面下拉列表

            //*判斷類型是否只讀(都沒有這個值得時候,就不能修改)
            if (casemaster.IsCaseIdExist("CaseAccountExternal", caseId) == "0" && casemaster.IsCaseIdExist("CaseSeizure", caseId) == "0" && casemaster.IsCaseIdExist("CasePayeeSetting", caseId) == "0" && casemaster.IsCaseIdExist("CaseSendSetting", caseId) == "0")
            {
                ViewBag.IsReadOnly = "isnotreadonly";
            }

            caseview.CaseMaster = casemaster.MasterModel(caseId);  //* 得到CaseMaster的model
            caseview.CaseMaster.GovDate = UtlString.FormatDateTw(caseview.CaseMaster.GovDate);//將西元年轉換為民國年
            if (caseview.CaseMaster != null)
                caseview.CaseMaster.OldCaseKind = caseview.CaseMaster.CaseKind;
            //adam 取消只有財產目錄呈現
            if (caseview.CaseMaster.CaseKind == "外來文案件") // == "財產申報")
            {
            caseview.CaseMaster.PropertyDeclaration = parm.GetCaseIdByMemo(caseview.CaseMaster.CaseId);
            }

            //* 以下是頁面初始化值
            ViewBag.CaseKindList = new SelectList(parm.GetCodeData("CASE_KIND"), "CodeDesc", "CodeDesc", caseview.CaseMaster.CaseKind);       //* 類別-大類
            string codeNo = parm.GetCodeNoByCodeDesc(caseview.CaseMaster.CaseKind);//根據大類獲得小類的list
            ViewBag.CaseKind2List = new SelectList(parm.GetCodeData(codeNo), "CodeDesc", "CodeDesc", caseview.CaseMaster.CaseKind2);   //* 類別-小類-扣押
            ViewBag.GovUnitList = new SelectList(parm.SelectGovUnitByGOV_KIND(caseview.CaseMaster.GovKind), "CodeDesc", "CodeDesc");//* 綁定來文機關下拉列表

            caseview.CaseObligorlistO = new List<CaseObligor>(10);//* 初始化義務人員行數
            caseview.CaseAttachmentlistO = attachment.AttachmentList(caseId);//* 得到CaseAttachment的model
            caseview.CaseEdocFilelist = CaseEdocFile.GetCaseEdocFileList(caseId);//* 得到CaseEdocFile的model
            List<CaseObligor> list = obligor.ObligorModel(caseId);    //* 得到CaseObligor的model
            for (int i = 0; i < list.Count; i++)
            {
                caseview.CaseObligorlistO.Add(list[i]);
            }
            for (int i = list.Count; i < 10; i++)
            {
                caseview.CaseObligorlistO.Add(new CaseObligor());
            }
            ViewBag.IsGovNameExist = "1";
            //判斷來文機關的資料是否有在DB的資料中,如果沒有時,請於右方顯示紅色字[請維護參數]
            if (caseview.CaseMaster.ReceiveKind == "電子公文")
            {
                if (casemaster.IsGovNameExist(caseview.CaseMaster.GovUnit) == "0")
                {
                    ViewBag.IsGovNameExist = "0";
                }
                #region 去掉默認值
                //if (caseview.CaseMaster.CaseKind2 == CaseKind2.CaseSeizure || caseview.CaseMaster.CaseKind2 == CaseKind2.CaseSeizureAndPay)//細類為扣押或扣押並支付時，這三個欄位才有默認值
                //{
                //    //受文者
                //    if (caseview.CaseMaster.Receiver == "")
                //    {
                //        caseview.CaseMaster.Receiver = "8888";//1.電子來文 8888(預設值)
                //    }
                //    //來函扣押總金額 txt的「合計」如果合計沒有才顯示DB里的ReceiveAmount IR-1008
                //    ////if (caseview.CaseMaster.ReceiveAmount == 0)
                //    //{
                //    //    CaseEdocFile caseEdocFile = caseAccount.OpenTxtDoc(caseview.CaseMaster.CaseId);
                //    //    string text = string.Empty;
                //    //    if (caseEdocFile != null )
                //    //    {
                //    //        byte[] file = caseEdocFile.FileObject;
                //    //        text = Encoding.UTF8.GetString(file);
                //    //        int beginIndex = text.IndexOf("合計：");
                //    //        int endIndex = text.IndexOf("備註：");
                //    //        if(beginIndex > 0 && endIndex > 0 && beginIndex < endIndex)
                //    //        {
                //    //            string amt = text.Substring(beginIndex + 3, endIndex - beginIndex - 3).Trim();
                //    //            if (!string.IsNullOrEmpty(amt))
                //    //            {
                //    //                caseview.CaseMaster.ReceiveAmount = int.Parse(amt);
                //    //            }
                //    //        }
                //    //        //else
                //    //        //{
                //    //        //    caseview.CaseMaster.ReceiveAmount = 0;
                //    //        //}
                //    //    }
                //    //}
                //    //金額未達毋需扣押
                //    if (caseview.CaseMaster.NotSeizureAmount == 0)
                //    {
                //        caseview.CaseMaster.NotSeizureAmount = 450;//1.電子來文 450(預設值)
                //    }
                //}
                #endregion
            }
            #region 去掉默認值
            //else//紙本
            //{
            //    if (caseview.CaseMaster.CaseKind2 == CaseKind2.CaseSeizure || caseview.CaseMaster.CaseKind2 == CaseKind2.CaseSeizureAndPay)//細類為扣押或扣押並支付時，這三個欄位才有默認值
            //    {
            //        //受文者
            //        if (caseview.CaseMaster.Receiver == "")
            //        {
            //            caseview.CaseMaster.Receiver = caseview.CaseMaster.Unit;//紙本來文 預設與分行別同
            //        }
            //        //來函扣押總金額 紙本來文 依分行人員鍵檔

            //        //金額未達毋需扣押
            //        if (caseview.CaseMaster.NotSeizureAmount == 0)
            //        {
            //            //紙本來文 預設值:法院1250 執行署450
            //            if (caseview.CaseMaster.GovKind == "法院")
            //            {
            //                caseview.CaseMaster.NotSeizureAmount = 1250;
            //            }
            //            else
            //            {
            //                caseview.CaseMaster.NotSeizureAmount = 450;
            //            }
            //        }
            //    }
            //}
            #endregion
            // 新增個資LOG
            string _user = (HttpContext.Session["UserAccount"] != null) ? HttpContext.Session["UserAccount"].ToString() : "";
            System.Collections.Specialized.NameValueCollection _parameters = new System.Collections.Specialized.NameValueCollection();
            //System.Collections.Specialized.NameValueCollection _parameters = ("POST".Equals(HttpContext.Request.HttpMethod)) ? HttpContext.Request.Form : HttpContext.Request.QueryString;
            string _controller = (this.ControllerContext.RouteData.Values["Controller"] != null) ? this.ControllerContext.RouteData.Values["Controller"].ToString() : "";
            string _action = (this.ControllerContext.RouteData.Values["Action"] != null) ? this.ControllerContext.RouteData.Values["Action"].ToString() : "";
            string _ip = (HttpContext.Request != null && !string.IsNullOrEmpty(HttpContext.Request.ServerVariables["REMOTE_ADDR"])) ? HttpContext.Request.ServerVariables["REMOTE_ADDR"] : "";
            BaseBusinessRule _business = new BaseBusinessRule();
            if (list.Count > 0)
            {
                //for (int i = 0; i < list.Count; i++)
                //{
                _business.ExecuteAPLOGSave(_user, _controller, _action, _ip, "CASEID=" + caseId.ToString(), list[0].ObligorNo.ToString());
                //}
            }
            // 新增結束
            return View(caseview);
        }

        /// <summary>
        /// 綁定下拉菜單
        /// </summary>
        public void InitDropdownListOptions()
        {
            ViewBag.CASE_END_TIME = parm.GetCASE_END_TIME("CASE_END_TIME");                                     //* 每天最晚時間
            ViewBag.GOV_KINDList = new SelectList(parm.GetCodeData("GOV_KIND"), "CodeDesc", "CodeDesc");        //* 來文機關類型
            ViewBag.SpeedList = new SelectList(parm.GetCodeData("INCOME_SPEED"), "CodeDesc", "CodeDesc");       //* 速別
            ViewBag.ReceiveKindList = new SelectList(parm.GetCodeData("INCOME_TYPE"), "CodeDesc", "CodeDesc");  //* 來文方式
        }

        [HttpPost]
        public ActionResult EditMaster(CaseSeizureViewModel model, IEnumerable<HttpPostedFileBase> fileAttNames)
        {
            model.CaseMaster.ModifiedUser = LogonUser.Account;   //*修改人
            model.CaseMaster.GovDate = UtlString.FormatDateTwStringToAd(model.CaseMaster.GovDate);
            model.CaseAttachmentlistO = new List<CaseAttachment>();

            #region  保存附件
            try
            {
                if (fileAttNames != null)
                {
                    foreach (var aModel in fileAttNames.Select(UploadFile).Where(aModel => aModel != null))
                    {
                        model.CaseAttachmentlistO.Add(aModel);
                    }
                }
            }
            catch (Exception ex)
            {
                return Content(@"<script>parent.showMessage('0','" + ex.Message + "');</script>");
            }
            #endregion

			casemaster.UserId = LogonUser.Account;
			casemaster.CaseId = model.CaseMaster.CaseId.ToString();
			casemaster.CaseNo = model.CaseMaster.CaseNo;
			casemaster.PageFrom = pageFromName;
            var scriptStr = casemaster.EditCase(ref model) ? "parent.showMessage('1','" + string.Format(Lang.csfs_case_delete_success0, model.CaseMaster.CaseNo) + "');"
                                                             : "parent.showMessage('0','" + Lang.csfs_edit_fail + "');";
            return Content(@"<script>" + scriptStr + "</script>");
        }

        /// <summary>
        /// 儲存上傳文件
        /// </summary>
        /// <param name="upFile"></param>
        /// <returns></returns>
        public CaseAttachment UploadFile(HttpPostedFileBase upFile)
        {
            if (upFile == null || upFile.ContentLength <= 0) return null;

            //获取用户上传文件的后缀名,重命名為當前登入者ID+年月日時分秒毫秒
            string newFileName = LogonUser.Account + "_" + DateTime.Now.ToString("yyyyMMddhhmmssfff") + Path.GetExtension(upFile.FileName);

            string serverPath = Path.Combine("~/", ConfigurationManager.AppSettings["UploadFolder"], DateTime.Today.ToString("yyyyMM"));
            string realPath = Server.MapPath(serverPath);
            //*月份文件夾不存在則新增
            if (!FrameWork.Util.UtlFileSystem.FolderIsExist(realPath))
                FrameWork.Util.UtlFileSystem.CreateFolder(realPath);

            //利用file.SaveAs保存图片
            string name = Path.Combine(realPath, newFileName);
            upFile.SaveAs(name);

            CaseAttachment aModel = new CaseAttachment
            {
                AttachmentName = Path.GetFileName(upFile.FileName),
                AttachmentServerName = newFileName,
                AttachmentServerPath = serverPath,
                isDelete = 0,
                CreatedUser = LogonUser.Account
            };
            return aModel;
        }

        /// <summary>
        /// 刪除一筆附件資料
        /// </summary>
        /// <param name="attachId"></param>
        /// <returns></returns>
        public ActionResult DeleteAttatch(string attachId)
        {
			attachment.UserId = LogonUser.Account;
			//casemaster.CaseId = model.CaseMaster.CaseId.ToString();
			//casemaster.CaseNo = model.CaseMaster.CaseNo;
            return Json(attachment.DeleteAttatch(attachId) > 0 ? new JsonReturn { ReturnCode = "1", ReturnMsg = Lang.csfs_del_ok }
                                                    : new JsonReturn { ReturnCode = "0", ReturnMsg = Lang.csfs_del_fail });
        }

        public ActionResult OpenTxtDoc(Guid CaseId)
        {
            CaseEdocFile caseEdocFile = caseAccount.OpenTxtDoc(CaseId);
            string text = string.Empty;
            if (caseEdocFile != null)
            {
                byte[] file = caseEdocFile.FileObject;
                text = Encoding.UTF8.GetString(file);
            }
            string ReturnMsg = string.IsNullOrEmpty(text) ? Lang.csfs_txtdocnotfound : text;
            return PartialView("TxtOpen", new CaseMaster { Memo = ReturnMsg });
            //return Json(string.IsNullOrEmpty(text) ? new JsonReturn { ReturnCode = "0", ReturnMsg = Lang.csfs_txtdocnotfound } : new JsonReturn { ReturnCode = "1", ReturnMsg = text });
        }

        public ActionResult OpenPdfDoc(string caseId)
        {
            CaseEdocFile caseEdocFile = caseAccount.OpenPdfDoc(caseId);
            string text = string.Empty;
            if (caseEdocFile != null)
            {
                byte[] file = caseEdocFile.FileObject;
                return File(file, "application/pdf", caseEdocFile.FileName);
            }
            else
            {
                return Content("<script>alert('" + Lang.csfs_no_data + "');</script>");
            }
        }

        /// <summary>
        /// 刪除一筆附件資料
        /// </summary>
        /// <param name="attachId"></param>
        /// <returns></returns>
        //public ActionResult DeleteFile(Guid CaseId)
        //{
        //    return Json(CaseEdocFile.DeleteAttatch(CaseId) > 0 ? new JsonReturn { ReturnCode = "1", ReturnMsg = Lang.csfs_del_ok }
        //                                            : new JsonReturn { ReturnCode = "0", ReturnMsg = Lang.csfs_del_fail });
        //}

        public ActionResult DeleteFile(Guid CaseId, string fName)
        {
            return Json(CaseEdocFile.DeleteAttatch(CaseId, fName) > 0 ? new JsonReturn { ReturnCode = "1", ReturnMsg = Lang.csfs_del_ok }
                                                    : new JsonReturn { ReturnCode = "0", ReturnMsg = Lang.csfs_del_fail });
        }
        // public ActionResult DownloadTxtDoc(Guid CaseId,string fName)
        //{
        //    CaseEdocFile caseEdocFile = caseAccount.OpenTxtDoc(CaseId,fName);
        //    string text = string.Empty;
        //    if (caseEdocFile != null)
        //    {
        //        byte[] file = caseEdocFile.FileObject;
        //        text = Encoding.UTF8.GetString(file);
        //    }
        //    string ReturnMsg = string.IsNullOrEmpty(text) ? Lang.csfs_txtdocnotfound : text;
        //    return PartialView("TxtOpen", new CaseMaster { Memo = ReturnMsg });
        //    //return Json(string.IsNullOrEmpty(text) ? new JsonReturn { ReturnCode = "0", ReturnMsg = Lang.csfs_txtdocnotfound } : new JsonReturn { ReturnCode = "1", ReturnMsg = text });
        //}
        public ActionResult IsGovNoExist(string txtGovNo)
        {
            CaseMasterBIZ casemaster = new CaseMasterBIZ();
            //* 1-有重複 0-不重複
            return Json(casemaster.IsGovNoExist(txtGovNo) == "1" ? new JsonReturn { ReturnCode = "1", ReturnMsg = "" }
                                                                : new JsonReturn { ReturnCode = "0", ReturnMsg = "" });
        }

    }
}