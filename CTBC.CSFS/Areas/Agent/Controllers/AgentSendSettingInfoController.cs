using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Mime;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls.Expressions;
using CTBC.CSFS.Models;
using CTBC.CSFS.BussinessLogic;
using CTBC.CSFS.ViewModels;
using CTBC.FrameWork.Pattern;
using CTBC.CSFS.Resource;
using CTBC.FrameWork.Util;
using System.IO;
using System.Configuration;



namespace CTBC.CSFS.Areas.Agent.Controllers
{
    public class AgentSendSettingInfoController : AppController
    {
        AgentToHandleBIZ AToHBIZ = new AgentToHandleBIZ();
        CaseSendSettingBIZ cssBIZ = new CaseSendSettingBIZ();

        public dynamic AccountInfoFlag { get; private set; }
        #region 查詢
        /// <summary>
        /// 查詢進入畫面
        /// </summary>
        /// <param name="caseId"></param>
        /// <returns></returns>
        public ActionResult Index(Guid caseId)
        {
            ViewBag.CaseId = caseId;
            ViewBag.SendGovName = Lang.csfs_ctci_bank;            
            PARMCodeBIZ para = new PARMCodeBIZ();
            ViewBag.AddDay = (Convert.ToInt32(para.GetCASE_END_TIME("OVERDUE_DAYS"))) * 1;
            CaseMasterBIZ casemaster = new CaseMasterBIZ();
            CaseMaster model = casemaster.MasterModel(caseId);
            ViewBag.LimiteDate = model.LimitDate;
            ViewBag.CaseNo = model.CaseNo;
            ViewBag.CaseKind2 = model.CaseKind2;
            ViewBag.AfterSeizureApproved = model.AfterSeizureApproved;
            ViewData["AfterSeizureApproved2"] = model.AfterSeizureApproved;
            //20170421 固定 RQ-2015-019666-017 派件至跨單位 宏祥 add start
            //判斷分行經辦角色關閉發文資訊功能
            int intRolesCount = LogonUser.Roles.Count;
            string strRoleLDAPId = "";
            for (int i = 0; i < intRolesCount; i++)
            {
               if (LogonUser.Roles[i].RoleLDAPId != "")
               {
                  strRoleLDAPId = LogonUser.Roles[i].RoleLDAPId;
               }
            }
            ViewBag.IsBranchAgent = "0";
            if (strRoleLDAPId == "CSFS011")
            {
               ViewBag.IsBranchAgent = "1";
            }
            //20170421 固定 RQ-2015-019666-017 派件至跨單位 宏祥 add end
            return View(SearchList(caseId));
        }
        /// <summary>
        /// 查詢結果列表
        /// </summary>
        /// <param name="caseId"></param>
        /// <returns></returns>
        public IList<CaseSendSettingQueryResultViewModel> SearchList(Guid caseId)
        {
            IList<CaseSendSettingQueryResultViewModel> listRtn = cssBIZ.GetSendSettingList(caseId);
            IList<CaseSendSettingDetails> listDetails = cssBIZ.GetSendSettingDetails(caseId);
            if (listRtn != null && listRtn.Any() && listDetails != null && listDetails.Any())
            {
                foreach (CaseSendSettingQueryResultViewModel item in listRtn)
                {
                    string receiver = "";
                    string cc = "";
                    foreach (CaseSendSettingDetails details in listDetails)
                    {
                        if (item.CaseId == details.CaseId && item.SerialId == details.SerialID)
                        {
                            if (details.SendType == CaseSettingDetailType.Receive)
                            {
                                receiver = receiver + details.GovName + ",";
                            }
                            else if (details.SendType == CaseSettingDetailType.Cc)
                            {
                                cc = cc + details.GovName + ",";
                            }
                        }
                    }
                    if (receiver.Length > 1)
                        receiver = receiver.Substring(0, receiver.Length - 1);
                    if (cc.Length > 1)
                        cc = cc.Substring(0, cc.Length - 1);
                    item.Receiver = receiver;
                    item.Cc = cc;
                }
            }
            return listRtn;
        }
        #endregion

        #region 新增

        public void Bind()
        {
            ViewBag.SpeedList = new SelectList(cssBIZ.GetCodeData("INCOME_SPEED"), "CodeDesc", "CodeDesc", Lang.csfs_speed1);
            ViewBag.SecurityList = new SelectList(cssBIZ.GetCodeData("SECURITY"), "CodeDesc", "CodeDesc", Lang.csfs_security1);
            ViewBag.SendKindList = new SelectList(cssBIZ.GetCodeData("SendKind"), "CodeDesc", "CodeDesc");  //* 發文方式
        }
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
        public ActionResult _SendCreateByDead()
        {            
            string strcaseId = Request["caseid"].ToString();
            Guid caseId = Guid.Parse(strcaseId);
            DataTable DT = new DataTable();
            string FileName = "";
            string xlsname = "";
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
                    DT = cssBIZ.ImportXLSX(xlsname, caseId.ToString(), fname);
                }

            }
            //ViewBag.CaseId = caseId;
            // caseId = ViewBag.caseid;
            ViewBag.AccountInfoFlag = AccountInfoFlag;//是否來自賬務資訊畫面的開啟發文資訊內容按鈕
            Bind();
            //* 案件主表信息
            CaseMaster master = new CaseMasterBIZ().MasterModel(caseId);
            //* 讀取該案件以前發文儲存的資料
            IList<CaseSendSettingQueryResultViewModel> listRtn = cssBIZ.GetSendSettingList(caseId);
            //* 用以返回的viewmodel
            CaseSendSettingCreateViewModel css = new CaseSendSettingCreateViewModel
            {
                CaseId = caseId,
                ReceiveKind = master.ReceiveKind,
                ReceiveList = new List<CaseSendSettingDetails>(),
                CcList = new List<CaseSendSettingDetails>()
            };

            //* 取得以前沒有更新發文資訊的
            //IList<CasePayeeSetting> cpsList = new CasePayeeSettingBIZ().GetPayeeSettingWhichNotSendSetting(caseId);

            //* 20150526.CR.進入畫面帶入來文機關資料.取資料
            string govAddr = new GovAddressBIZ().GetEnabledGovAddrByGovName(master.GovUnit);

            #region SendDate
            if (master.CaseKind2 == "國稅局死亡")
            {
                //* 扣押 (當天)
                css.SendDate = DateTime.Today;
            }
  
            #endregion
            #region Template
            List<SelectListItem> list = new List<SelectListItem>();
            if (master.CaseKind2 == "國稅局死亡")
            {
                //* 20150526 CR 3.外來文 發文正本預設為來文機關
                
                //css.ReceiveList.Add(new CaseSendSettingDetails { CaseId = caseId, SendType = 1, GovName = master.GovUnit, GovAddr = govAddr });
               list.Add(new SelectListItem { Text = "死亡回文", Value = "死亡回文" });        
            }
             ViewBag.TemplateList = list;
            #endregion

            //* 沒有發文的受款人正本.副本

            AgentToHandleBIZ AToHBIZ = new AgentToHandleBIZ();
            SendSettingRefBiz refBiz = new SendSettingRefBiz();
            css.Template = "國稅局死亡";
            css.ReceiveList.Clear();
            css.CcList.Clear();
            css.Security = "密(收到即解密)";
            //@@@! 2018,0828 測試多義務人發文..
            css.SendKind = master.ReceiveKind == "紙本" ? "紙本發文" : "電子發文"; 
            for (int i = 0; i < DT.Rows.Count; i++)
            {
                if (DT.Rows[i][22].ToString().Trim() == "Y" || DT.Rows[i][22].ToString().Trim() == "y" || DT.Rows[i][23].ToString().Trim() == "Y" || DT.Rows[i][23].ToString().Trim() == "y" || DT.Rows[i][24].ToString().Trim() == "Y" || DT.Rows[i][24].ToString().Trim() == "y" || DT.Rows[i][25].ToString().Trim() == "Y" || DT.Rows[i][25].ToString().Trim() == "y" || DT.Rows[i][26].ToString().Trim() != "N" || DT.Rows[i][27].ToString().Trim() == "Y" || DT.Rows[i][27].ToString().Trim() == "y" || DT.Rows[i][28].ToString().Trim() == "Y" || DT.Rows[i][28].ToString().Trim() == "y" )
                {
                    SendSettingRef SSref = refBiz.DeadGetSubjectAndDescription(DT.Rows[i],caseId, css.Template, css.SendKind, 0);
                    css.Subject = SSref.Subject;
                    css.Description = SSref.Description;
                    // 取得 ParmCode 中, CodeType='SendGovName' 的CodeDesc
                    PARMCodeBIZ pbiz = new PARMCodeBIZ();
                    var pz = pbiz.GetParmCodeByCodeType("SendGovName").FirstOrDefault();
                    string strCodeDesc = "";
                    if (pz != null)
                    {
                        strCodeDesc = pz.CodeDesc;
                    }
                    //css.SendWord = master.ReceiveKind == "紙本" ? Lang.csfs_ctci_bank : strCodeDesc;
                    css.SendWord = master.ReceiveKind == "紙本" ? Lang.csfs_ctci_bank : cssBIZ.GetFirstCodeDataByDesc("SendGovName", "").CodeDesc;
                    css.Speed = master.Speed;
                    css.Security = Lang.csfs_security1;
                    css.ReceiveList.Add(new CaseSendSettingDetails() { CaseId = caseId, SendType = 1, GovName = DT.Rows[i][10].ToString(), GovAddr = DT.Rows[i][21].ToString() });
                    css.GovName= DT.Rows[i][10].ToString();
                    css.GovAddr = DT.Rows[i][21].ToString();
                    css.GovNameCc = "";
                    css.GovAddrCc = "";
                    //css.SendNo = cssBIZ.SendNo();
                    //css.SendNo = (DateTime.Now.Year - 1911) + "2" + css.SendNo.Substring(9);
                    //css.CcList.Add(new CaseSendSettingDetails() { CaseId = caseId, SendType = 2, GovName = item.CCReceiver, GovAddr = item.Currency });
                    cssBIZ.SaveDeadCreate(css);
                    css.ReceiveList.Clear();
                }
            }
            //return Json(new JsonReturn { ReturnCode = "1", ReturnMsg = model.SendNo });
            return Json(new JsonReturn { ReturnCode = "1", ReturnMsg = "新增死亡發文成功" });                                       
        }


        /// <summary>
        /// 儲存新增死亡
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SaveDeadCreate(CaseSendSettingCreateViewModel model,DataTable dt, string flag)
        {
            if (!string.IsNullOrEmpty(flag))
            {
                model.flag = flag;
            }
            model.SendDate = UtlString.FormatDateTwStringToAd(model.SendDate);
            model.Security = "密(收到即解密)";
            for (int i=0;i<dt.Rows.Count; i++)
            {
                cssBIZ.SaveCreate(model);
            }
            return Json(new JsonReturn { ReturnCode = "1", ReturnMsg = model.SendNo });
        }
        /// <summary>
        /// 進入新增畫面.帶出初始值
        /// </summary>
        /// <param name="caseId"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public ActionResult _SendCreate(Guid caseId, string AccountInfoFlag)
        {
            ViewBag.CaseId = caseId;
            ViewBag.AccountInfoFlag = AccountInfoFlag;//是否來自賬務資訊畫面的開啟發文資訊內容按鈕
            Bind();
            //* 案件主表信息
            CaseMaster master = new CaseMasterBIZ().MasterModel(caseId);
            //* 讀取該案件以前發文儲存的資料
            IList<CaseSendSettingQueryResultViewModel> listRtn = cssBIZ.GetSendSettingList(caseId);
            //* 用以返回的viewmodel
            CaseSendSettingCreateViewModel css = new CaseSendSettingCreateViewModel
            {
                CaseId = caseId,
                ReceiveKind = master.ReceiveKind,
                CaseKind2 = master.CaseKind2,
                ReceiveList = new List<CaseSendSettingDetails>(),
                CcList = new List<CaseSendSettingDetails>()
            };
            ViewBag.SendGovName = Lang.csfs_ctci_bank;
            ViewBag.eSendGovName = cssBIZ.GetFirstCodeDataByDesc("SendGovName", "").CodeDesc;
            if (master.ReceiveKind == "電子公文" && master.CaseKind2 == "支付")
            {
                css.Template = "支付電子回文";              
            }
            if (master.ReceiveKind != "電子公文" && master.CaseKind2 == "支付")
            {
                css.Template = "支付";
            }
            //* 取得以前沒有更新發文資訊的
            IList<CasePayeeSetting> cpsList = new CasePayeeSettingBIZ().GetPayeeSettingWhichNotSendSetting(caseId);
            
            //* 20150526.CR.進入畫面帶入來文機關資料.取資料
            string govAddr = new GovAddressBIZ().GetEnabledGovAddrByGovName(master.GovUnit);

            #region SendDate
            if (master.CaseKind2 == Lang.csfs_seizure)
            {
                //* 扣押 (當天)
                css.SendDate = Convert.ToDateTime(UtlString.FormatDateTw(DateTime.Today.ToString("yyyy/MM/dd")));
            }
            else if (master.CaseKind2 == Lang.csfs_seizureandpay && master.AfterSeizureApproved != 1)
            {
                //* 扣押並支付 的扣押 (當天)
                css.SendDate = Convert.ToDateTime(UtlString.FormatDateTw(DateTime.Today.ToString("yyyy/MM/dd")));
            }
            else if (ViewBag.CaseKind2 == Lang.csfs_Pay)
            {
                //* 支付類 (看Master)
                css.SendDate = Convert.ToDateTime(UtlString.FormatDateTw(master.PayDate));
            }
            else if (master.CaseKind2 == Lang.csfs_seizureandpay && master.AfterSeizureApproved == 1)
            {
                //* 扣押並支付 的支付(看Master)
                css.SendDate = Convert.ToDateTime(UtlString.FormatDateTw(master.PayDate));
            }
            else
            {
                //* 其他(當天)
                css.SendDate = Convert.ToDateTime(UtlString.FormatDateTw(DateTime.Today.ToString("yyyy/MM/dd")));
            }
            css.SendWord = master.ReceiveKind == "紙本" ? Lang.csfs_ctci_bank : cssBIZ.GetFirstCodeDataByDesc("SendGovName", "").CodeDesc;           
            #endregion
            #region Template
            List<SelectListItem> list = new List<SelectListItem>();
            if (master.CaseKind == Lang.csfs_receive_case)
            {
                //* 20150526 CR 3.外來文 發文正本預設為來文機關
                css.ReceiveList.Add(new CaseSendSettingDetails { CaseId = caseId, SendType = 1, GovName = master.GovUnit, GovAddr = govAddr });

                //* 除了細類為165調閱及財產申報範本;其他一律使用非165調閱範本
                //list.Add(master.CaseKind2 == Lang.csfs_165_reading
                //    ? new SelectListItem { Text = Lang.csfs_165_reading, Value = Lang.csfs_165_reading }
                //    : new SelectListItem { Text = Lang.csfs_not_165_reading, Value = Lang.csfs_not_165_reading });
                //* 20150702 需求變更,新增財產申報
                if (master.CaseKind2 == Lang.csfs_165_reading)
                {
                    list.Add(new SelectListItem { Text = Lang.csfs_165_reading, Value = Lang.csfs_165_reading });
                }
                else if (master.CaseKind2 == Lang.csfs_property_declaration1)
                {
                    list.Add(new SelectListItem { Text = Lang.csfs_property_declaration1, Value = Lang.csfs_property_declaration1 });
                }
                else
                {
                    list.Add(new SelectListItem { Text = Lang.csfs_not_165_reading, Value = Lang.csfs_not_165_reading });   
                }
            }
            if (master.CaseKind == Lang.csfs_menu_tit_caseseizure)
            {

                if (master.CaseKind2 == Lang.csfs_seizure)
                {
                    //* 20150526 CR 1.扣押 發文正本預設為來文機關
                    css.ReceiveList.Add(new CaseSendSettingDetails { CaseId = caseId, SendType = 1, GovName = master.GovUnit, GovAddr = govAddr });

                    list.Add(new SelectListItem { Text = Lang.csfs_seizure, Value = Lang.csfs_seizure });
                }
                else if (master.CaseKind2 == Lang.csfs_Pay)
                {
                    //* 20150526 CR 2.支付 發文副本預設為來文機關
                    css.CcList.Add(new CaseSendSettingDetails { CaseId = caseId, SendType = 2, GovName = master.GovUnit+"(執)", GovAddr = govAddr });
                    if (master.ReceiveKind == "電子公文")
                    {
                        list.Add(new SelectListItem { Text = "支付電子回文", Value = "支付電子回文" });
                        list.Add(new SelectListItem { Text = "支付", Value = "支付" });
                    }
                    else
                    {
                        list.Add(new SelectListItem { Text = Lang.csfs_Pay, Value = Lang.csfs_Pay });
                    }
                }
                else if (master.CaseKind2 == Lang.csfs_seizureandpay)
                {
                    //* 20150526 CR 1.扣押 發文正本預設為來文機關
                    css.ReceiveList.Add(new CaseSendSettingDetails { CaseId = caseId, SendType = 1, GovName = master.GovUnit, GovAddr = govAddr });
                    list.Add(master.AfterSeizureApproved == 1
                        ? new SelectListItem {Text = Lang.csfs_Pay, Value = Lang.csfs_Pay}
                        : new SelectListItem {Text = Lang.csfs_seizure, Value = Lang.csfs_seizure});
                }
                else
                {
                    //* 20150526 CR 1.扣押 發文正本預設為來文機關
                    css.ReceiveList.Add(new CaseSendSettingDetails { CaseId = caseId, SendType = 1, GovName = master.GovUnit, GovAddr = govAddr });
                }
            }
            ViewBag.TemplateList = list;
            #endregion
            
            //* 沒有發文的受款人正本.副本
            if (cpsList != null && cpsList.Any())
            {
                foreach (CasePayeeSetting item in cpsList)
                {
                    css.ReceiveList.Add(new CaseSendSettingDetails() { CaseId = caseId, SendType = 1, GovName = item.Receiver, GovAddr = item.Address });
                    css.CcList.Add(new CaseSendSettingDetails() { CaseId = caseId, SendType = 2, GovName = item.CCReceiver, GovAddr = item.Currency });
                }
            }

            return View(css);
        }


        /// <summary>
        /// 儲存新增
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult _SendCreate(CaseSendSettingCreateViewModel model, string flag)
        {
            if(!string.IsNullOrEmpty(flag))
            {
                model.flag = flag;
            }
			//model.SendNo = cssBIZ.SendNo();
			//if (model.SendKind == "電子發文")
			//{
			//	//第四碼固定為2 --simon 2016/08/05
			//	//model.SendNo = model.SendDate.Year + "00" + model.SendNo.Substring(9);
			//	model.SendNo = model.SendDate.Year + "20" + model.SendNo.Substring(9);
			//}
			model.SendDate = UtlString.FormatDateTwStringToAd(model.SendDate);           
            model.Security = "密(收到即解密)";
            //model.Security = "密";
            //if (string.IsNullOrEmpty(model.SendNo)) { return Json(new JsonReturn() { ReturnCode = "2", ReturnMsg = Lang.csfa_NoMax }); }
            return Json(cssBIZ.SaveCreate(model) ? new JsonReturn { ReturnCode = "1", ReturnMsg = model.SendNo }
                                                        : new JsonReturn { ReturnCode = "0", ReturnMsg = Lang.csfs_save_fail });
        }
        #endregion

        /// <summary>
        /// 進入修改
        /// </summary>
        /// <param name="serialId"></param>
        /// <param name="caseId"></param>
        /// <returns></returns>
        public ActionResult _SendEdit(int serialId, Guid caseId, string AccountInfoFlag)
        {
            ViewBag.CaseId = caseId;
            ViewBag.AccountInfoFlag = AccountInfoFlag;//是否來自賬務資訊畫面的開啟發文資訊內容按鈕
            CaseMaster master = new CaseMasterBIZ().MasterModel(caseId);
            ViewBag.RecvKind = master.ReceiveKind ;
            ViewBag.CaseKind2 = master.CaseKind2;
            ViewBag.AfterSeizureApproved = master.AfterSeizureApproved;
            Bind();
            CaseSendSettingCreateViewModel model = cssBIZ.GetCaseSettingAndDetails(serialId);
            model.SendDate = model.SendDate.AddYears(-1911);
            model.ReceiveKind = master.ReceiveKind;
            model.CaseKind2 = master.CaseKind2;
            #region Template
            model.SendWord = model.SendKind == "電子發文" ? cssBIZ.GetFirstCodeDataByDesc("SendGovName", "").CodeDesc  : Lang.csfs_ctci_bank;
            ViewBag.eSendGovName = cssBIZ.GetFirstCodeDataByDesc("SendGovName", "").CodeDesc;
            ViewBag.SendGovName = Lang.csfs_ctci_bank;
            //ViewBag.SendGovName =  model.SendKind == "電子發文" ? cssBIZ.GetFirstCodeDataByDesc("SendGovName", "").CodeDesc : Lang.csfs_ctci_bank;
            List<SelectListItem> list = new List<SelectListItem>
            {
                new SelectListItem {Text = model.Template, Value = model.Template}
            };

            if (master.CaseKind2 == Lang.csfs_Pay)
            {
                    list.Clear();
                    list.Add(new SelectListItem { Text = "支付", Value = "支付" });
                    list.Add(new SelectListItem { Text = "支付電子回文", Value = "支付電子回文" });
            }
            ViewBag.TemplateList = list;
            #endregion

            return View("_SendEdit", model);
        }
        /// <summary>
        /// 儲存修改
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult _SendEdit(CaseSendSettingCreateViewModel model)
        {
            model.SendDate = UtlString.FormatDateTwStringToAd(model.SendDate);
            return Json(cssBIZ.SaveEdit(model) ? new JsonReturn() { ReturnCode = "1", ReturnMsg = "" }
                                                        : new JsonReturn() { ReturnCode = "0", ReturnMsg = Lang.csfs_edit_fail });
        }
        /// <summary>
        /// 儲存刪除
        /// </summary>
        /// <param name="serialId"></param>
        /// <returns></returns>
        public ActionResult _SendDelete(int serialId)
        {
            string userId = LogonUser.Account;
            if (cssBIZ.DeleteCheck(serialId) < 1)
            {  
                return Content(cssBIZ.Delete(serialId,userId) > 0 ? "已刪除!!" : "刪除失敗!!");
            }
            else
            {
                return Content("已開支票,請先取消票號!!");
            }
        }
        /// <summary>
        /// 根據案件讀取模版
        /// </summary>
        /// <param name="caseId"></param>
        /// <param name="template"></param>
        /// <returns></returns>
        public ActionResult GetSubjectAndDescription(Guid caseId, string template, string sendKind ,int serialId)
        {
			if (sendKind == "")
			{
				sendKind = @"電子發文";
			}
            CaseMaster master = new CaseMasterBIZ().MasterModel(caseId);
            SendSettingRefBiz refBiz = new SendSettingRefBiz();
            if (master.CaseKind2 == Lang.csfs_Pay && master.ReceiveKind == "電子公文")
            {
                return Json(refBiz.GetChangeSubjectAndDescription(caseId, template, sendKind, serialId));
            }
            else
            {
                return Json(refBiz.GetSubjectAndDescription(caseId, template, sendKind));
            }
        }

        /// <summary>
        /// 呈核
        /// </summary>
        /// <returns></returns>
        public ActionResult ChenHe(string CaseIdarr)
        {
            string userId = LogonUser.Account;
            string[] caseid = CaseIdarr.Split(',');
            List<Guid> aryCaseId = (from id in caseid where !string.IsNullOrEmpty(id) select new Guid(id)).ToList();
            string agentIdList = AToHBIZ.GetManagerID(userId);
            caseid = agentIdList.Split(',');
            List<string> aryAgentId = (from id in caseid where !string.IsNullOrEmpty(id) select id).ToList();
            return Json(AToHBIZ.AgentSubmit(aryCaseId, aryAgentId, userId));
        }
        public ActionResult OverDue(string OverDueMemo, string strIds)
        {
            string userId = LogonUser.Account;
            string[] caseid = strIds.Split(',');
            List<Guid> aryCaseId = (from id in caseid where !string.IsNullOrEmpty(id) select new Guid(id)).ToList();
            string agentIdList = AToHBIZ.GetManagerID(userId);
            caseid = agentIdList.Split(',');
            List<string> aryAgentId = (from id in caseid where !string.IsNullOrEmpty(id) select id).ToList();
            AgentToHandle model = new AgentToHandle();
            model.OverDueMemo = OverDueMemo;
            return Json(AToHBIZ.OverDue(model, aryCaseId, aryAgentId, userId));
        }
    }
}