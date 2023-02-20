using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CTBC.CSFS.Models;
using CTBC.CSFS.BussinessLogic;
using CTBC.CSFS.ViewModels;
using CTBC.CSFS.Resource;
using CTBC.FrameWork.Util;
using CTBC.CSFS.Pattern;
using Microsoft.Reporting.WebForms;
using System.Data;
using CTBC.CSFS.Filter;
using Microsoft.Practices.EnterpriseLibrary.Logging;


namespace CTBC.CSFS.Areas.KeyInput.Controllers
{
    /// <summary>
    /// 建檔作業-警示通報
    /// </summary>
    public class CaseWarningController : AppController
    {
        //public CTBC.FrameWork.Platform.AppLog AppLog = new CTBC.FrameWork.Platform.AppLog();
        //public LogWriter LogWriter = null;
        CaseWarningBIZ warn = new CaseWarningBIZ();
        WarningQueryBIZ wqbiz = new WarningQueryBIZ();
        PARMCodeBIZ parm = new PARMCodeBIZ();
        CaseMasterBIZ caseMaster = new CaseMasterBIZ();
        string strFlag_909113 = "0";
        string strFIX = "0";
        string strSET = "0";
        string strRelease = "0";
        string strExtend = "0";
        string strRetry = "0";

        public List<WarningDetails> WarningDetailsList { get; private set; }

        // GET: KeyInput/CaseWarning
        [RootPageFilter]
        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Index(WarningMaster model)
        {
            
            BindWarningStatusList();
            List<WarningMaster> masterList = warn.GetWarnMasterListByCustAccount(model.CustAccount);
            //* 找不到歷史案件就新增
            if (masterList == null || !masterList.Any())
                return RedirectToAction("Create", new { CustAccount = model.CustAccount });


            foreach (WarningMaster master in masterList)
            {
                //* 有未結清的則編輯
                if (warn.IsAllStatus(new WarningState() { DocNo = master.DocNo }) > 0)//舊尚未結案則編輯
                {
                    return RedirectToAction("Edit", new { DocNo = master.DocNo });
                }
                if (warn.IsEmptyStatus(new WarningState() { DocNo = master.DocNo }) == 0)//舊尚未結案則編輯
                {
                    return RedirectToAction("Edit", new { DocNo = master.DocNo });
                }
                //if (( master.DocNo.Length > 0 )  &&  (warn.IsAllStatus(new WarningState() { DocNo = master.DocNo }) == 0)) //主檔已儲存
                //{
                //    return RedirectToAction("Edit", new { DocNo = master.DocNo });
                //}
                //if ((masterList.Count > 0) && (warn.IsAllStatus(new WarningState() { DocNo = master.DocNo }) == 0)) //主檔已儲存
                //{
                //    return RedirectToAction("Edit", new { DocNo = master.DocNo });
                //}

            }
            return RedirectToAction("Create", new { CustAccount = model.CustAccount });

        }

        public ActionResult ChangeID(string CustIdOld, string CustIdNew)
        {            
            return Json(warn.UpdateCustId(CustIdOld,CustIdNew, LogonUser));
        }

        /// <summary>
        /// 綁定帳戶狀態
        /// </summary>
        public void BindWarningStatusList()
        {
            ViewBag.StatusList = new SelectList(parm.GetCodeData("WarningStatus"), "CodeNo", "CodeDesc");
            ViewBag.BankID = new SelectList(caseMaster.GetCodeData("RCAF_BRANCH"), "CodeNo", "CodeNo");
            List<SelectListItem> Kindlist = new List<SelectListItem>
            {
                new SelectListItem() {Text ="-請選擇-", Value = ""},
                new SelectListItem() {Text ="通報聯徵", Value = "1"},
               // new SelectListItem() {Text ="解除", Value = "2"},
                new SelectListItem() {Text ="修改聯徵", Value = "3"},
                new SelectListItem() {Text ="ID變更", Value = "4"},
                new SelectListItem() {Text ="延長", Value = "5"},
            };
            ViewBag.KindList = Kindlist;
        }

        /// <summary>
        /// 新增
        /// </summary>
        /// <returns></returns>
        public ActionResult Create(string CustAccount)
        {
            BindWarningStatusList();
            WarningMaster model = new WarningMaster();
            model.CustAccount = CustAccount;
            ViewBag.CustAccount = CustAccount;
            model.BankName = new SelectList(caseMaster.GetCodeData("RCAF_BRANCH"), "CodeNo", "CodeDesc").FirstOrDefault().Text;
            //Add by zhangwei 20180315 start
            //發送電文33401
            string strErrorCodeAndMsg = "";
            //當經辦登入外來文系統時未輸入RCAF的帳號、密碼及分行時，登入之權限應不可執行任何有關電文的交易按鈕。 IR-0048
            if (!string.IsNullOrEmpty(LogonUser.RCAFAccount.Trim()) && !string.IsNullOrEmpty(LogonUser.RCAFPs.Trim()) && !string.IsNullOrEmpty(LogonUser.RCAFBranch.Trim()))
            {
                warn.Require33401(model, ref strErrorCodeAndMsg, LogonUser);
            }
            //Add by zhangwei 20180315 end
            CaseWarningViewModel viewmodel = new CaseWarningViewModel()
            {
                WarningMaster = model
            };
            
            return View(viewmodel);
        }

        /// <summary>
        /// 實際進行Create動作
        /// </summary>
        /// <param name="model"></param>
        /// <param name="fileAttNames"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Create(CaseWarningViewModel model, IEnumerable<HttpPostedFileBase> fileAttNames)
        {
            model.WarningMaster.CreatedUser = LogonUser.Account;
            model.WarningAttachmentList = new List<WarningAttachment>();

            #region  保存附件
            try
            {
                if (fileAttNames != null)
                {
                    foreach (var aModel in fileAttNames.Select(UploadFile).Where(aModel => aModel != null))
                    {
                        model.WarningAttachmentList.Add(aModel);
                    }
                }
            }
            catch (Exception ex)
            {

            }
            #endregion
            //Add by zhangwei 20180315 start
            if (!string.IsNullOrEmpty(model.WarningMaster.ClosedDate))
            {
                if(model.WarningMaster.ClosedDate.Trim().Length==10)//說明為電文傳回格式
                {
                    if(model.WarningMaster.ClosedDate.Trim()!="00/00/0000")
                    {
                        string[] list = model.WarningMaster.ClosedDate.Trim().Split('/');
                        model.WarningMaster.ClosedDate = list[2] + "/" + list[1] + "/" + list[0];
                    }
                    else
                    {
                        model.WarningMaster.ClosedDate = null;
                    }
                }
                else
                {
                    //model.WarningMaster.ClosedDate = UtlString.FormatDateTwStringToAd(model.WarningMaster.ClosedDate);
                    DateTime dt = DateTime.Parse(model.WarningMaster.ClosedDate);
                    model.WarningMaster.ClosedDate = dt.ToString("yyyy-MM-dd");
                }
                
            }
            #region 去掉千分位符號
            model.WarningMaster.CurBal = string.IsNullOrEmpty(model.WarningMaster.CurBal) ? model.WarningMaster.CurBal : model.WarningMaster.CurBal.Replace(",",string.Empty);
            model.WarningMaster.MD=string.IsNullOrEmpty(model.WarningMaster.MD)?model.WarningMaster.MD:model.WarningMaster.MD.Replace(",",string.Empty);
            model.WarningMaster.NotifyBal= string.IsNullOrEmpty(model.WarningMaster.NotifyBal) ? model.WarningMaster.NotifyBal : model.WarningMaster.NotifyBal.Replace(",",string.Empty);
            model.WarningMaster.ReleaseBal = string.IsNullOrEmpty(model.WarningMaster.ReleaseBal) ? model.WarningMaster.ReleaseBal : model.WarningMaster.ReleaseBal.Replace(",", string.Empty);
            model.WarningMaster.VD = string.IsNullOrEmpty(model.WarningMaster.VD) ? model.WarningMaster.VD : model.WarningMaster.VD.Replace(",", string.Empty);
            #endregion
            //Add by zhangwei 20180315 end
            if (warn.CreateWarnCase(ref model))
            {
                return RedirectToAction("Edit", new { DocNo = model.WarningMaster.DocNo });
            }
            else
            {
                return Json(new JsonReturn() { ReturnCode = "0", ReturnMsg = Lang.csfs_add_fail });
            }
            //return Json(warn.CreateWarnCase(ref model) ? new JsonReturn() { ReturnCode = "1", ReturnMsg = model.WarningMaster.DocNo }
            //                                          : new JsonReturn() { ReturnCode = "0", ReturnMsg = Lang.csfs_add_fail });
        }

        /// <summary>
        /// 儲存上傳文件
        /// </summary>
        /// <param name="upFile"></param>
        /// <returns></returns>
        public WarningAttachment UploadFile(HttpPostedFileBase upFile)
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

            WarningAttachment aModel = new WarningAttachment
            {
                AttachmentName = Path.GetFileName(upFile.FileName),
                AttachmentServerName = newFileName,
                AttachmentServerPath = serverPath,
                CreatedUser = LogonUser.Account
            };
            return aModel;
        }

        public ActionResult Edit(string DocNo)
        {
            ViewBag.DocNo = DocNo;
            BindWarningStatusList();

            CaseWarningViewModel viewmodel = new CaseWarningViewModel()
            {
                WarningMaster = warn.GetWarnMasterListByDocNo(DocNo),
                WarningAttachmentList = warn.GetWarnAttatchmentList(DocNo),
                WarningStateList = warn.GetWarnStateQueryList(DocNo),
                WarningDetailsList = warn.WarningDetailsSearchList(DocNo),
                WarningGenAcctList = warn.WarningGenAcctSearchList(DocNo),
                //CaseCustRFDMRecvList = warn.GetCaseCustRFDMRecvList(DocNo)
                WarningHistoryList = warn.GetWarningHistoryList(DocNo)
            };
            viewmodel.WarningMaster.NotifyBal = UtlString.FormatCurrency(viewmodel.WarningMaster.NotifyBal, 2);//通報時餘額目前等於目前餘額
            viewmodel.WarningMaster.VD = UtlString.FormatCurrency(viewmodel.WarningMaster.VD, 2);
            viewmodel.WarningMaster.MD = UtlString.FormatCurrency(viewmodel.WarningMaster.MD, 2);
            viewmodel.WarningMaster.CurBal = UtlString.FormatCurrency(viewmodel.WarningMaster.CurBal, 2);//目前餘額
            viewmodel.WarningMaster.ReleaseBal = UtlString.FormatCurrency(viewmodel.WarningMaster.ReleaseBal, 2);
            //viewmodel.WarningDetails.CaseId = viewmodel.WarningMaster.CaseId;
            ViewBag.Currency = viewmodel.WarningMaster.Currency;
            return PartialView("Edit", viewmodel);
        }

        [HttpPost]
        public ActionResult Edit(CaseWarningViewModel model, IEnumerable<HttpPostedFileBase> fileAttNames)
        {
            
            model.WarningMaster.ModifiedUser = LogonUser.Account;   //*修改人
            model.WarningAttachmentList = new List<WarningAttachment>();

            #region  保存附件
            try
            {
                if (fileAttNames != null)
                {
                    foreach (var aModel in fileAttNames.Select(UploadFile).Where(aModel => aModel != null))
                    {
                        model.WarningAttachmentList.Add(aModel);
                    }
                }
            }
            catch (Exception ex)
            {

            }
            #endregion
            //Add by zhangwei 20180315 start
            if (!string.IsNullOrEmpty(model.WarningMaster.ClosedDate))
            {
                // model.WarningMaster.ClosedDate = UtlString.FormatDateTwStringToAd(model.WarningMaster.ClosedDate);
                DateTime dt = DateTime.Parse(model.WarningMaster.ClosedDate);
                model.WarningMaster.ClosedDate = dt.ToString("yyyy-MM-dd");

            }
            //Add by zhangwei 20180315 end
            #region 去掉千分位符號
            model.WarningMaster.CurBal = string.IsNullOrEmpty(model.WarningMaster.CurBal) ? model.WarningMaster.CurBal : model.WarningMaster.CurBal.Replace(",", string.Empty);
            model.WarningMaster.MD = string.IsNullOrEmpty(model.WarningMaster.MD) ? model.WarningMaster.MD : model.WarningMaster.MD.Replace(",", string.Empty);
            model.WarningMaster.NotifyBal = string.IsNullOrEmpty(model.WarningMaster.NotifyBal) ? model.WarningMaster.NotifyBal : model.WarningMaster.NotifyBal.Replace(",", string.Empty);
            model.WarningMaster.ReleaseBal = string.IsNullOrEmpty(model.WarningMaster.ReleaseBal) ? model.WarningMaster.ReleaseBal : model.WarningMaster.ReleaseBal.Replace(",", string.Empty);
            model.WarningMaster.VD = string.IsNullOrEmpty(model.WarningMaster.VD) ? model.WarningMaster.VD : model.WarningMaster.VD.Replace(",", string.Empty);
            #endregion
            var scriptStr = warn.EditWarnCase(model) ? "parent.showMessage('1','" + Lang.csfs_edit_ok + "');"
                                                            : "parent.showMessage('0','" + Lang.csfs_edit_fail + "');";
            return Content(@"<script>" + scriptStr + "</script>");
        }

        public ActionResult DeleteAttatch(string attachId)
        {
            return Json(warn.DeleteAttatch(attachId) > 0 ? new JsonReturn { ReturnCode = "1", ReturnMsg = Lang.csfs_del_ok }
                                                  : new JsonReturn { ReturnCode = "0", ReturnMsg = Lang.csfs_del_fail });
        }

        public void BindDropDownList()
        {
            ViewBag.NotificationContentList = new SelectList(parm.GetCodeData("NotificationContent"), "CodeDesc", "CodeDesc");//*通報內容
            ViewBag.NotificationSourceList = new SelectList(parm.GetCodeData("NotificationSource"), "CodeDesc", "CodeDesc", "電話詐財");//*通報來源

            ViewBag.NotificationUnitList = new SelectList(parm.GetCodeData("NotificationUnit"), "CodeDesc", "CodeDesc");//*通報單位
            List<int> delItem = new List<int>() { 0, 2, 4 };
            var ddd = parm.GetCodeData("CASE_WARNING").Where( x=> !delItem.Contains( x.SortOrder));

            ViewBag.KindList = new SelectList(ddd, "CodeDesc", "CodeDesc");//*通報單位
            
        }

        public ActionResult CreateWarn(string DocNo,string Currency)
        {
            List<SelectListItem> Originallist = new List<SelectListItem>
            {
                new SelectListItem() {Text ="N", Value = "N"},
                new SelectListItem() {Text ="Y", Value = "Y"},
            };
            ViewBag.OriginalList = Originallist;
            BindDropDownList();
            WarningDetails model = new WarningDetails();
            model.HappenDateTime = UtlString.FormatDateTw(DateTime.Now.ToString("yyyy/MM/dd"));
            model.HappenDateTimeForHour = DateTime.Now.ToString("HH:mm");
            model.EtabsDatetime = UtlString.FormatDateTw(DateTime.Now.ToString("yyyy/MM/dd"));
            model.EtabsDatetimeHour = DateTime.Now.ToString("HH:mm");
            //model.ExtendDate = UtlString.FormatDateTw(DateTime.Now.ToString("yyyy/MM/dd"));
            //model.ExtendDateHour = DateTime.Now.ToString("HH:mm");
            model.No_e = "";
            model.ForCDate = UtlString.FormatDateTw(DateTime.Now.ToString("yyyy/MM/dd"));
            var WM = warn.GetWarnMasterListByDocNo(DocNo);
            model.CaseId = WM.CaseId;
            return View(model);
        }

        public ActionResult CopyWarn(string DocNo)
        {
            BindDropDownList();

            WarningDetails model = warn.GetLastWarningDetails(DocNo);
            //WarningMaster wm = warn.GetWarnMasterListByDocNo(DocNo);
            //model.CustAccount = wm.CustAccount;
            string strTime = model.EtabsDatetime;
            string strHappen = model.HappenDateTime;
            model.EtabsDatetime = UtlString.FormatDateTw(DateTime.Now.ToString("yyyy/MM/dd"));
            model.EtabsDatetimeHour = DateTime.Now.ToString("HH:mm");
            model.ExtendDate = UtlString.FormatDateTw(DateTime.Now.ToString("yyyy/MM/dd"));
            model.ExtendDateHour = DateTime.Now.ToString("HH:mm");
            model.HappenDateTime = UtlString.FormatDateTw(DateTime.Now.ToString("yyyy/MM/dd"));
            model.HappenDateTimeForHour = DateTime.Now.ToString("HH:mm");
            model.ForCDate = UtlString.FormatDateTw(DateTime.Now.ToString("yyyy/MM/dd"));
            return View(model);
        }

        //*新增案發內容
        [HttpPost]
        public ActionResult CreateWarn(WarningDetails model)
        {
            if (model.bool_Retry == true) { model.Retry = "1"; } else { model.Retry = "0"; }
            model.Status = "C01";
            if (model.bool_Fix == true) { model.FIX = "1"; } else { model.FIX = "0"; }
            model.HappenDateTimeForHour = Convert.ToDateTime(model.HappenDateTimeForHour).ToString("HH:mm");
            model.HappenDateTime = UtlString.FormatDateTwStringToAd(model.HappenDateTime) + " " + model.HappenDateTimeForHour;
            model.EtabsDatetime = UtlString.FormatDateTwStringToAd(model.EtabsDatetime) + " " + model.EtabsDatetimeHour;
            model.ExtendDate = UtlString.FormatDateTwStringToAd(model.ExtendDate) + " " + model.ExtendDateHour;
            model.ForCDate = UtlString.FormatDateTwStringToAd(model.ForCDate);
            model.CreatedUser = LogonUser.Account;
            if (string.IsNullOrEmpty(model.No_e))
            {
                model.No_e = "";
            }
            if (model.EtabsDatetime.Trim() == "")
            {
                model.EtabsDatetime = null;
            }
            return Json(warn.CreateWarnContent(model,LogonUser) ? new JsonReturn() { ReturnCode = "1", ReturnMsg = "" }
                                                                 : new JsonReturn() { ReturnCode = "0", ReturnMsg = Lang.csfs_add_fail });
        }
        public ActionResult ChenHe(WarningDetails model)
        {
            model.Status = "D01";
            model.HappenDateTimeForHour = Convert.ToDateTime(model.HappenDateTimeForHour).ToString("HH:mm");
            model.HappenDateTime = UtlString.FormatDateTwStringToAd(model.HappenDateTime) + " " + model.HappenDateTimeForHour;
            model.EtabsDatetime = UtlString.FormatDateTwStringToAd(model.EtabsDatetime) + " " + model.EtabsDatetimeHour;
            model.ReleaseDate = UtlString.FormatDateTwStringToAd(model.ReleaseDate) + " " + model.ReleaseDateForHour;
            model.ExtendDate = UtlString.FormatDateTwStringToAd(model.ExtendDate) + " " + model.ExtendDateHour;
            model.ForCDate = UtlString.FormatDateTwStringToAd(model.ForCDate);
            model.CreatedUser = LogonUser.Account;
            if (string.IsNullOrEmpty(model.No_e))
            {
                model.No_e = "";
            }
            if (model.EtabsDatetime.Trim() == "")
            {
                model.EtabsDatetime = null;
            }
            if (model.ReleaseDate.Trim() == "")
            {
                model.ReleaseDate = null;
            }
            return Json(warn.CreateWarnContent(model,this.LogonUser) ? new JsonReturn() { ReturnCode = "1", ReturnMsg = "" }
                                                                 : new JsonReturn() { ReturnCode = "0", ReturnMsg = Lang.csfs_add_fail });
        }
        [HttpPost]
        public ActionResult EditChenHe(WarningDetails model)
        {
            if (model.bool_909113 == true) { model.Flag_909113 = "1";} else { model.Flag_909113 = "0";}
            if (model.bool_Retry == true) { model.Retry = "1"; } else { model.Retry = "0"; }
            if (model.bool_Set == true) { model.Set = "1"; } else { model.Set = "0"; }
            if (model.bool_Extend == true) { model.Extend = "1"; } else { model.Extend = "0"; }
            if (model.bool_Release == true) { model.Release = "1"; } else { model.Release = "0"; }
            if (model.bool_Fix == true) { model.FIX = "1"; } else { model.FIX = "0"; }
            if (model.bool_FIXSEND == true) { model.FIXSEND = "1"; } else { model.FIXSEND = "0"; }
            model.Status = "D01";
            BindWarningStatusList();
            if (!model.HappenDateTimeForHour.Contains(":") && !model.HappenDateTimeForHour.Contains("："))//*不包含冒號的時候
            {
                model.HappenDateTimeForHour = model.HappenDateTimeForHour.Substring(0, 2) + ":" + model.HappenDateTimeForHour.Substring(2, 2);
            }

            model.HappenDateTime = UtlString.FormatDateTwStringToAd(model.HappenDateTime) + " " + model.HappenDateTimeForHour;
            model.EtabsDatetime = UtlString.FormatDateTwStringToAd(model.EtabsDatetime) + " " + model.EtabsDatetimeHour;
            model.ExtendDate = UtlString.FormatDateTwStringToAd(model.ExtendDate) + " " + model.ExtendDateHour;
            model.ReleaseDate = UtlString.FormatDateTwStringToAd(model.ReleaseDate) + " " + model.ReleaseDateForHour;
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
            return Json(warn.EditWarnContent(model) ? new JsonReturn() { ReturnCode = "1", ReturnMsg = "" }
                                                                 : new JsonReturn() { ReturnCode = "0", ReturnMsg = Lang.csfs_add_fail });
        }

        public ActionResult EditWarn(string SerialID)
        {
            WarningDetails model = warn.GetWarnDetailBySerialID(SerialID);
            string strTime = model.EtabsDatetime;
            string strExtend = model.ExtendDate==null ? "": model.ExtendDate.ToString();
            string strHappen = model.HappenDateTime;
            string strReleaseDate = model.ReleaseDate== null ? "" : model.ReleaseDate.ToString();

            model.EtabsDatetime =strTime.Trim()==""?null: UtlString.FormatDateTw(Convert.ToDateTime(strTime).ToString("yyyy/MM/dd"));
            model.EtabsDatetimeHour = strTime.Trim() == "" ? null : Convert.ToDateTime(strTime).ToString("HH:mm");
            model.ExtendDate = strExtend.Trim() == "" ? null : UtlString.FormatDateTw(Convert.ToDateTime(strExtend).ToString("yyyy/MM/dd"));
            model.ExtendDateHour = strExtend.Trim() == "" ? null : Convert.ToDateTime(strExtend).ToString("HH:mm");
            model.ReleaseDate = strReleaseDate.Trim() == "" ? null : UtlString.FormatDateTw(Convert.ToDateTime(strReleaseDate).ToString("yyyy/MM/dd"));
            model.ReleaseDateForHour = strReleaseDate.Trim() == "" ? null : Convert.ToDateTime(strReleaseDate).ToString("HH:mm");
            model.HappenDateTime = strHappen.Trim() == "" ? null : UtlString.FormatDateTw(Convert.ToDateTime(strHappen).ToString("yyyy/MM/dd"));
            model.HappenDateTimeForHour = strHappen.Trim() == "" ? null : Convert.ToDateTime(strHappen).ToString("HH:mm");
            model.ForCDate = UtlString.FormatDateTw(model.ForCDate);
            //ViewBag.KindList = new SelectList(parm.GetCodeData("CASE_WARNING"), "CodeDesc", "CodeDesc", model.Kind);//* 
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

        public ActionResult EditWarnAccount(string ID)
        {
            WarningGenAcct model = warn.GetWarnAccountBySerialID(ID);
            string strTime = DateTime.ParseExact(model.TRAN_DATE, "yyyyMMdd", System.Globalization.CultureInfo.CurrentCulture).ToString("yyyy/MM/dd");
            //string strTime = Convert.ToDateTime(model.TRAN_DATE).ToString("yyyy/MM/dd"); //TransDateTime;
            //string strHappen = model.HappenDateTime;
            model.TRAN_DATE = strTime.Trim() == "" ? null : UtlString.FormatDateTw(Convert.ToDateTime(strTime).ToString("yyyy/MM/dd"));
            //model.EtabsDatetimeHour = strTime.Trim() == "" ? null : Convert.ToDateTime(strTime).ToString("HH:mm");
            //model.HappenDateTime = UtlString.FormatDateTw(Convert.ToDateTime(strHappen).ToString("yyyy/MM/dd"));
            //model.HappenDateTimeForHour = Convert.ToDateTime(strHappen).ToString("HH:mm");
            //model.ForCDate = UtlString.FormatDateTw(model.ForCDate);

            //ViewBag.NotificationContentList = new SelectList(parm.GetCodeData("NotificationContent"), "CodeDesc", "CodeDesc", model.NotificationContent);//*通報內容
            //ViewBag.NotificationSourceList = new SelectList(parm.GetCodeData("NotificationSource"), "CodeDesc", "CodeDesc", model.NotificationSource);//*通報來源
            //ViewBag.NotificationUnitList = new SelectList(parm.GetCodeData("NotificationUnit"), "CodeDesc", "CodeDesc", model.NotificationUnit);//*通報單位
            //Add by zhangwei 20180315 start
            //List<SelectListItem> Originallist = new List<SelectListItem>
            //{
            //    new SelectListItem() {Text ="-請選擇-", Value = "0"},
            //    new SelectListItem() {Text ="Y", Value = "Y"},
            //    new SelectListItem() {Text ="N", Value = "N"},
            //};
            //ViewBag.OriginalList = Originallist;
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
            if (model.ReleaseDateForHour != null)
            {
                if (!model.ReleaseDateForHour.Contains(":") && !model.ReleaseDateForHour.Contains("："))//*不包含冒號的時候
                {
                    model.ReleaseDateForHour = model.ReleaseDateForHour.Substring(0, 2) + ":" + model.ReleaseDateForHour.Substring(2, 2);
                }
            }
            model.ReleaseDate = UtlString.FormatDateTwStringToAd(model.ReleaseDate) + " " + model.ReleaseDateForHour; ;
            model.HappenDateTime = UtlString.FormatDateTwStringToAd(model.HappenDateTime) + " " + model.HappenDateTimeForHour;
            model.EtabsDatetime = UtlString.FormatDateTwStringToAd(model.EtabsDatetime) + " " + model.EtabsDatetimeHour;
            model.ExtendDate = UtlString.FormatDateTwStringToAd(model.ExtendDate) + " " + model.ExtendDateHour;
            model.ForCDate = UtlString.FormatDateTwStringToAd(model.ForCDate);
            model.ModifiedUser = LogonUser.Account;
            if(model.EtabsDatetime.Trim()=="")
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
            return Json(warn.EditWarnContent(model) ? new JsonReturn() { ReturnCode = "1", ReturnMsg = "" }
                                                                 : new JsonReturn() { ReturnCode = "0", ReturnMsg = Lang.csfs_add_fail });
        }
        [HttpPost]
        public ActionResult EditWarnAccount(WarningGenAcct model)
        {
            //if (!model.HappenDateTimeForHour.Contains(":") && !model.HappenDateTimeForHour.Contains("："))//*不包含冒號的時候
            //{
            //    model.HappenDateTimeForHour = model.HappenDateTimeForHour.Substring(0, 2) + ":" + model.HappenDateTimeForHour.Substring(2, 2);
            //}

            //model.HappenDateTime = UtlString.FormatDateTwStringToAd(model.HappenDateTime) + " " + model.HappenDateTimeForHour;
            //model.EtabsDatetime = UtlString.FormatDateTwStringToAd(model.EtabsDatetime) + " " + model.EtabsDatetimeHour;
            //model.TRAN_DATE = UtlString.FormatDateTwStringToAd(model.TRAN_DATE);
            model.ModifiedUser = LogonUser.Account;
            //if (model.TimeLog==null )
            //{
            //        model.TimeLog = null;
            //}
            
            if (model.Memo==null)
            {
                    model.Memo = null;
            }
            if (model.eTabs==null)
            {
                    model.eTabs = "N";
            }
            return Json(warn.EditWarnAccount(model) ? new JsonReturn() { ReturnCode = "1", ReturnMsg = "" }
                                                                 : new JsonReturn() { ReturnCode = "0", ReturnMsg = Lang.csfs_add_fail });
        }

        public ActionResult DeleteWarn(string serialID, string DocNo, string Source)
        {
            return Json(warn.DeleteWarnContents(serialID, DocNo, Source) ? new JsonReturn() { ReturnCode = "1", ReturnMsg = "" }
                                                               : new JsonReturn() { ReturnCode = "0", ReturnMsg = Lang.csfs_add_fail });
        }
        /// <summary>
        ///  adam 帳卡刪除
        /// </summary>
        /// <param name="DocNo"></param>
        /// <param name="交易日"></param>
        /// <returns></returns>
        public ActionResult DeleteWarnAccount(string id, string DocNo )
        {
            return Json(warn.DeleteWarnAccount(id, DocNo) ? new JsonReturn() { ReturnCode = "1", ReturnMsg = "" }
                                                               : new JsonReturn() { ReturnCode = "0", ReturnMsg = Lang.csfs_add_fail });
        }
        public ActionResult SetStatus(string DocNo, string NotificationSource)
        {
            string strTime = "";
            ViewBag.RelieveReasonList = new SelectList(parm.GetCodeData("RelieveReason"), "CodeDesc", "CodeDesc");//*解除原因
            WarningState model = warn.GetWarnStateInfo(DocNo, NotificationSource);
            if (model.Flag_Release == "1")
            {
                model.text_Release = "V";
            } 
            else 
                model.text_Release = " ";
            strTime = model.RelieveDate;
            model.RelieveDate = strTime.Trim() == "" ? null : UtlString.FormatDateTw(Convert.ToDateTime(strTime).ToString("yyyy/MM/dd"));
            model.RelieveDateTimeForHour = strTime.Trim() == "" ? null : Convert.ToDateTime(strTime).ToString("HH:mm");
            if (model.RelieveDateTimeForHour == "00:00") model.RelieveDateTimeForHour = "";
            //if (String.IsNullOrEmpty(strTime))
            //{  
            //    model.RelieveDateTimeForHour = DateTime.Now.ToString("HH:mm");
            //}
            //model.RelieveDate = UtlString.FormatDateTw(DateTime.Now.ToString("yyyy/MM/dd"));
            return View(model);
        }

        [HttpPost]
        public ActionResult SetStatus(WarningState model)
        {
            model.Status = "C01";
            model.ModifiedUser = LogonUser.Account;
            //model.RelieveDate = UtlString.FormatDateTwStringToAd(model.RelieveDate);
            model.RelieveDate = UtlString.FormatDateTwStringToAd(model.RelieveDate) + " " + model.RelieveDateTimeForHour;
            return Json(warn.SetStatus(model));
        }

        [HttpPost]
        public ActionResult WarningReleaseChenHe(WarningState model)
        {
            model.Status = "D01";
            model.ModifiedUser = LogonUser.Account;
            model.Kind = "解除";
            model.RelieveDate = UtlString.FormatDateTwStringToAd(model.RelieveDate);
            return Json(warn.SetStatus(model));
        }
        public ActionResult ReportWarningGenAcct(string DocNo)
        {
            MemoryStream ms = new MemoryStream();
            string fileName = string.Empty;
                ms = wqbiz.Excel(DocNo);
                fileName = "警示帳務LOG" + "_" + Lang.csfs_debtexcel + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".csv";
            //}
            //else
            //{
            //    //20171103 RC RQ-2015-019666-003 系統功能修正 宏祥 update start
            //    //ms = CQBIZ.Exportlist(model.CaseIdarr, model.CreatedDateS, model.CreatedDateE);
            //    LdapEmployeeBiz empBiz = new LdapEmployeeBiz();
            //    IList<PARMCode> listCode = empBiz.GetCodeData("Department");
            //    ms = CQBIZ.Exportlist(model.CaseIdarr, model.CreatedDateS, model.CreatedDateE, listCode);
            //    //20171103 RC RQ-2015-019666-003 系統功能修正 宏祥 update end
            //    fileName = Lang.csfs_menu_tit_casequery + "_" + Lang.csfs_export_filelist + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xls";
            //}

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
 
        public ActionResult ReportWarning(string SerialID)
        {
            CaseMasterBIZ masterBiz = new CaseMasterBIZ();
            List<ReportParameter> listParm = new List<ReportParameter>();

            //* 通報窗口.聯絡人及聯絡電話
            PARMCode codeItem = masterBiz.GetCodeData("REPORT_SETTING", "Window").FirstOrDefault();
            listParm.Add(new ReportParameter("Window", codeItem == null ? "" : codeItem.CodeDesc));
            codeItem = masterBiz.GetCodeData("REPORT_SETTING", "PersonAndTel").FirstOrDefault();
            listParm.Add(new ReportParameter("PersonAndTel", codeItem == null ? "" : codeItem.CodeDesc));

            DataTable dtWarning = warn.GetCaseWarningBySerialID(SerialID);

            LocalReport localReport = new LocalReport { ReportPath = Server.MapPath("~/Reports/Rdlc/ReportWarning.rdlc") };
            localReport.SetParameters(listParm); //*添加參數
            localReport.DataSources.Add(new ReportDataSource("CaseWarning", dtWarning));   //* 添加數據源,可以多個

            Warning[] warnings;
            string[] streams;
            string mimeType;
            string encoding;
            string fileNameExtension;

            var renderedBytes = localReport.Render("PDF",
                null,
                out mimeType,
                out encoding,
                out fileNameExtension,
                out streams,
                out warnings);
            localReport.Dispose();

            Response.ClearContent();
            Response.ClearHeaders();
            return File(renderedBytes, mimeType, "Report.pdf");
        }
        /// <summary>
        /// 產生交易明細
        /// </summary>
        /// <returns></returns>
        public ActionResult GetGenDetail(string DocNo, string CustId, string CustAccount, string ForCDateS, string ForCDateE)
        {
            CaseWarningBIZ Biz = new CaseWarningBIZ();
            if (!string.IsNullOrEmpty(ForCDateS))
            {
                ForCDateS = UtlString.FormatDateTwStringToAd(ForCDateS);
            }
            if (!string.IsNullOrEmpty(ForCDateE))
            {
                ForCDateE = UtlString.FormatDateTwStringToAd(ForCDateE);
            }
            string strTrnNum = "";
            this.LogWriter = this.AppLog.Writer;
            int i_state = 0;
            i_state = Biz.ProduceGenDetail(DocNo, CustId, CustAccount, ForCDateS, ForCDateE, LogonUser.Account,ref strTrnNum,"S");
            i_state = Biz.ProduceGenDetail(DocNo, CustId, CustAccount, ForCDateS, ForCDateE, LogonUser.Account, ref strTrnNum, "Q");
            i_state = Biz.ProduceGenDetail(DocNo, CustId, CustAccount, ForCDateS, ForCDateE, LogonUser.Account, ref strTrnNum, "T");
            //if(i_state==1)//向警示歷史記錄增添一條記錄成功后同時開始跟主機交互
            //{
            //    this.AppLog.Categories.Add(AppLog.CUF_Log_FuncEntryCategory);
            //    this.AppLog.EventId = AppLog.CUF_Log_EntryEventID;
            //    this.AppLog.FunctionId = this.FunctionId;
            //    this.AppLog.Message = "Function " + this.FunctionId + " Esb:" + DocNo + "Cust:" + CustId + " Account:" + CustAccount + ForCDateS + ForCDateE+strTrnNum;
            //    this.AppLog.Title = this.AppLog.Message;
            //    this.AppLog.Severity = System.Diagnostics.TraceEventType.Information;
            //    this.AppLog.Priority = 3;
            //    this.LogWriter.Write(this.AppLog);
            //    //Biz.GenDetailESB(DocNo, CustId, CustAccount, ForCDateS, ForCDateE,strTrnNum);
            //}
            return i_state == 1 ? Json(new JsonReturn() { ReturnCode = "1" }) : Json(new JsonReturn() { ReturnCode = "0", ReturnMsg = "產生交易明細失敗！" });
        }
        /// <summary>
        /// 設定按鈕觸發事件
        /// </summary>
        /// <param name="DocNo"></param>
        /// <returns></returns>
        public ActionResult GetOriginalData(string DocNo,string NotificationSource,string codeflag,string Setdate,string Setdatetime,string Currency,string Kind,string ExtendDate)
        {
            int ct = 0;
            int Y = 0;
            if (Kind == "正本")
            {
               ct = warn.WarningDetailsKindCount(DocNo);
               Y  = warn.WarningDetailsKindYCount(DocNo);
                if (ct > 0)
                {
                    if ( Y < ct)
                    {
                        return Json(new { ReturnCode = "0", ErrorCode = "", ErrorMsg = "正本非全為Y，請先確認！!" });
                    }
                }
            }
            
            WarningMaster WM  = new WarningMaster();
            WM = warn.GetWarnMasterListByDocNo(DocNo);
            if (WM.Currency != null )
            {
                Currency = WM.Currency.ToString();
            }
            else
            {
                Currency = "TWD";
            }
            string strOriginal = "N";//正本
            string NotificationSourceCode = "";
            if (NotificationSource == "非電話詐財")
            {
                NotificationSourceCode = "11";
            }
            else if(NotificationSource == "電話詐財")
            {
                NotificationSourceCode = "12";
            }
            else if(NotificationSource == "偵辦刑事案件")
            {
                NotificationSourceCode = "14";
            }
             WarningDetails wdetails = new WarningDetails();
            string strSetTime = "";
            if(Setdate!=""&& Setdatetime!="")
            {
                //strSetTime=UtlString.FormatDateTwStringToAd(Setdate) + " " + Setdatetime;
                strSetTime = FormatDateTwStringToddmmyyyy(Setdate);
            }
            else
            {
                strSetTime = DateTime.Now.ToString("ddMMyyyy");
            }
            // 20220825, Patrick , 通報案件, 設定時, 
            //當..選延長時, ===> 會打設定..時, 要打9098(IF Kind == "延長")
            //當..選聯徵通報時, ===> 打.. 9091... ELSE 

            //當經辦登入外來文系統時未輸入RCAF的帳號、密碼及分行時，登入之權限應不可執行任何有關電文的交易按鈕。 IR-0048
            if (!string.IsNullOrEmpty(LogonUser.RCAFAccount.Trim()) && !string.IsNullOrEmpty(LogonUser.RCAFPs.Trim()) && !string.IsNullOrEmpty(LogonUser.RCAFBranch.Trim()))
            {
                //string strResult = warn.Require9091(wdetails, codeflag, DocNo, "", NotificationSourceCode, "", "", "", strSetTime, LogonUser);
                if (codeflag == "0")//如果是新增通報第一次是
                {
                    strOriginal = "N";
                }
                else//否則如果是1就是修改
                {
                    strOriginal = "Y";
                    strFIX = "1";
                }


                if (Kind == "延長")
                {
                    // 要將 ExtendDate 傳入的日期, 改為主機用的日期..
                    //DateTime eDate;

                    if (!string.IsNullOrEmpty(ExtendDate))
                    {
                        ExtendDate = FormatDateTwStringToddmmyyyy(ExtendDate);
                    }
                    else
                    {
                        return Json(new { ReturnCode = "0", ErrorMsg = "未填寫延長日期！" });
                    }


                    // 20220826, 若是延長, 則把設定時間, 用延長時間去打.. 
                    // 20220826, 發現電文..延長時間欄位.. 是無效的...
                    //string result9098 = warn.Require9098(wdetails, codeflag, DocNo, "", NotificationSourceCode, DocNo, "", System.Guid.NewGuid().ToString(), strSetTime, ExtendDate, strOriginal, LogonUser);
                    string result9098 = warn.Require9098(wdetails, codeflag, DocNo, "", NotificationSourceCode, DocNo, "", System.Guid.NewGuid().ToString(), ExtendDate, ExtendDate, strOriginal, LogonUser);
                    if (result9098.StartsWith("0000|"))
                    {
                        return Json(new { ReturnCode = "1", ErrorMsg = "9098延長成功" });
                    }
                    else
                    {
                        return Json(new { ReturnCode = "0", ErrorMsg = "執行9098延長電文發生錯誤！" });
                    }
                }
                else // 原來打9091的程式
                {
                    WarningDetails resultRtn = warn.Require9091(wdetails, codeflag, DocNo, Currency, NotificationSourceCode, "", "", System.Guid.NewGuid().ToString(), strSetTime, LogonUser, strOriginal);

                    string EtabsDatetime = "";//設定時間
                    string EtabsDatetimeHour = "";//設定時間時分
                    //string ExtendDate = "";//延長時間
                    string ExtendDateHour = "";//延長時間時分
                    string ReturnCode = "";
                    //if (strResult != "")
                    if (resultRtn.ReturnResult9091 != "")
                    {
                        //string[] strlist = strResult.Split('|');
                        string[] strlist = resultRtn.ReturnResult9091.Split('|');
                        if (strlist[0] == "0000")//如果執行成功
                        {
                            if (codeflag == "0")//如果是新增
                            {
                                strOriginal = "N";
                            }
                            else//否則如果是1就是修改
                            {
                                strOriginal = "Y";
                                strFIX = "1";
                            }
                            EtabsDatetime = resultRtn.EtabsDatetime;
                            EtabsDatetimeHour = resultRtn.EtabsDatetimeHour;
                            ExtendDate = resultRtn.ExtendDate;
                            ExtendDateHour = resultRtn.ExtendDateHour;
                            ReturnCode = "1";
                            strFlag_909113 = "1";
                        }
                        else
                        {
                            if (codeflag == "0")//如果是新增
                            {
                                strOriginal = "N";

                            }
                            else//否則如果是1就是修改
                            {
                                strOriginal = "N";
                            }
                            ReturnCode = "0";
                            strFlag_909113 = "0";
                        }
                        // adam 20220330  延長是否要打電文,待確認
                        return Json(new { ReturnCode = ReturnCode, Original = strOriginal, EtabsDatetime = EtabsDatetime, EtabsDatetimeHour = EtabsDatetimeHour, ErrorMsg = strlist[1] });
                    }
                    else
                    {
                        return Json(new { ReturnCode = "0", ErrorMsg = "執行電文發生錯誤！" });
                    }

                }




            }
            else
            {
                return Json(new { ReturnCode = "0", ErrorCode = "", ErrorMsg = "抱歉, 您的RCAF帳號登入異常, 無法使用此功能!" });
            }

        }
        /// <summary>
        /// 將104/08/01 格式化為ddmmyyyy
        /// </summary>
        /// <param name="dateTw"></param>
        /// <param name="sperator"></param>
        /// <returns></returns>
        public static string FormatDateTwStringToddmmyyyy(string dateTw, char sperator = '/')
        {
            if (string.IsNullOrEmpty(dateTw))
                return "";
            string[] list = dateTw.Split(sperator);
            int year = Convert.ToInt32(list[0]);
            if (year <= 200)
                year = year + 1911;
            list[0] = Convert.ToString(year);
            list[1] = ("00" + list[1]).Substring(list[1].Length + 2 - 2);
            list[2] = ("00" + list[2]).Substring(list[2].Length + 2 - 2);
            return list[2] + list[1] + list[0];
        }
        /// <summary>
        /// 沖正
        /// </summary>
        /// <param name="DocNo"></param>
        /// <returns></returns>
        public ActionResult CancelOriginalData(string DocNo, string NotificationSource, string codeflag, string Setdate, string Setdatetime)
        {

            string NotificationSourceCode = "";
            if (NotificationSource == "非電話詐財")
            {
                NotificationSourceCode = "11";
            }
            else if (NotificationSource == "電話詐財")
            {
                NotificationSourceCode = "12";
            }
            else if (NotificationSource == "偵辦刑事案件")
            {
                NotificationSourceCode = "14";
            }

            WarningDetails wdetails = new WarningDetails();
            string strSetTime = "";
            if (Setdate != "" && Setdatetime != "")
            {
                strSetTime = UtlString.FormatDateTwStringToAd(Setdate) + " " + Setdatetime;
            }
            //當經辦登入外來文系統時未輸入RCAF的帳號、密碼及分行時，登入之權限應不可執行任何有關電文的交易按鈕。 IR-0048
            if (!string.IsNullOrEmpty(LogonUser.RCAFAccount.Trim()) && !string.IsNullOrEmpty(LogonUser.RCAFPs.Trim()) && !string.IsNullOrEmpty(LogonUser.RCAFBranch.Trim()))
            {
                // 20220825 Patrick , 通報案件, 沖正的Button..
                //當通報數==1 時, 直接打9092
                //當通報數>1 時, 打9098, 日期, 才要回到.. 9091 或9098 的最後一筆日期....

                // 取得通報筆數, 但要排除.. 14的...
                var list = warn.WarningDetailsSearchList(DocNo).Where(x => x.NotificationSource != "偵辦刑事案件").ToList();


                string strOriginal = "N";//正本
                string EtabsDatetime = "";//設定時間
                string EtabsDatetimeHour = "";//設定時間時分
                string ExtendDate = "";//延長時間
                string ExtendDateHour = "";//延長時間時分
                string ReturnCode = "";

                string strResult = "";


                if (list.Count() > 1) // 打9098
                {
                    // 要先找.. 倒數第二筆...9091 或9098 打的日期...
                    string DTSRC_DATE = warn.get9091LastDay(DocNo, LogonUser);
                    if (DTSRC_DATE.StartsWith("0000|"))
                    {
                        string sDate = DTSRC_DATE.Split('|')[1];
                        string Due_date = "";
                        strResult = warn.Require9098(wdetails, codeflag, DocNo, "", NotificationSourceCode, DocNo, "", System.Guid.NewGuid().ToString(), sDate, Due_date, strOriginal, LogonUser);
                    }
                    else
                        strResult = "0001|找不到最後一次打9091的日期";

                }
                else // 打9092
                {
                    strResult = warn.Require9092(wdetails, codeflag, DocNo, "", NotificationSourceCode, DocNo, "", System.Guid.NewGuid().ToString(), strSetTime, LogonUser);
                }


                if (strResult != "")
                {
                    string[] strlist = strResult.Split('|');
                    if (strlist[0] == "0000")//如果執行成功
                    {
                        if (codeflag == "0")//如果是新增
                        {
                            strOriginal = "N";
                        }
                        else//否則如果是1就是修改
                        {
                            strOriginal = "N";
                        }
                        EtabsDatetime = "";
                        EtabsDatetimeHour = "";
                        ExtendDate = "";
                        ExtendDateHour = "";
                        ReturnCode = "1";
                        wdetails.Flag_909113 = "0";
                        wdetails.bool_909113 = false;
                        strFlag_909113 = "0";
                    }
                    else
                    {
                        strFlag_909113 = "1";
                        ReturnCode = "0";
                    }
                    // adam 20220330 延長是否要打電文
                    return Json(new { ReturnCode = ReturnCode, Original = strOriginal, EtabsDatetime = EtabsDatetime, EtabsDatetimeHour = EtabsDatetimeHour, ErrorMsg = strlist[1] });
                }
                else
                {
                    return Json(new { ReturnCode = "0", ErrorMsg = "執行電文發生錯誤！" });
                }
            }
            else
            {
                return Json(new { ReturnCode = "0", ErrorCode = "", ErrorMsg = "抱歉, 您的RCAF帳號登入異常, 無法使用此功能!" });
            }
        }
        /// <summary>
        /// 警示狀態解除
        /// </summary>
        /// <param name="DocNo"></param>
        /// <returns></returns>
        public ActionResult WarningRemove(string DocNo, string NotificationSource, string EtabsNo,string No_165)
        {
            string NotificationSourceCode = "";
            if (NotificationSource == "非電話詐財")
            {
                NotificationSourceCode = "11";
            }
            else if (NotificationSource == "電話詐財")
            {
                NotificationSourceCode = "12";
            }
            else if (NotificationSource == "偵辦刑事案件")
            {
                NotificationSourceCode = "14";
            }
            WarningState wstate = new WarningState();
            //string EtabsNo = "";
            //List<WarningState> WarningStateList = warn.GetWarnStateQueryList(DocNo);
            //if (WarningStateList != null && WarningStateList.Any())
            //{
            //    string[] ary = { };

            //    var obj = WarningStateList.FirstOrDefault();
            //    if (obj != null && !string.IsNullOrEmpty(obj.EtabsNo))
            //    {
            //        EtabsNo = obj.EtabsNo;
            //    }
            //}

            //當經辦登入外來文系統時未輸入RCAF的帳號、密碼及分行時，登入之權限應不可執行任何有關電文的交易按鈕。 IR-0048
            if (!String.IsNullOrEmpty(No_165))
            {
                // No_165 有值用今天解除寫到
                string strRelieveDate = "";//解除警示日期
                string ReturnCode = "";
                strRelieveDate = UtlString.FormatDateTw(DateTime.Now.ToString("yyyy/MM/dd"));//wstate.RelieveDate;
                string strTime = Convert.ToDateTime(DateTime.Now.ToString()).ToString("HH:mm");
                ReturnCode = "1";                
                int i_result1 = warn.WriteOriginal(DocNo, NotificationSource, "",No_165);
                return Json(new { ReturnCode = ReturnCode, RelieveDate = strRelieveDate,RelieveTime = strTime, ErrorMsg = "" });
            }
            else
            {
                if (!string.IsNullOrEmpty(LogonUser.RCAFAccount.Trim()) && !string.IsNullOrEmpty(LogonUser.RCAFPs.Trim()) && !string.IsNullOrEmpty(LogonUser.RCAFBranch.Trim()))
                {
                    string strResult = warn.Require9092ForRemove(wstate, "2", DocNo, "", NotificationSourceCode, EtabsNo, "", System.Guid.NewGuid().ToString(), "", LogonUser);
                    string strRelieveDate = "";//解除警示日期
                    string ReturnCode = "";
                    string strTime = "";
                    if (strResult != "")
                    {
                        string[] strlist = strResult.Split('|');
                        if (strlist[0] == "0000")//如果執行成功
                        {
                            //解除自動帶當前日期20180726
                            strRelieveDate = UtlString.FormatDateTw(DateTime.Now.ToString("yyyy/MM/dd"));//wstate.RelieveDate;
                            strTime = Convert.ToDateTime(DateTime.Now.ToString()).ToString("HH:mm");
                            ReturnCode = "1";
                            //並同時拋查33401之可用餘額寫回外來文系統,且於查詢解除警示外顯欄位新增[解除帳號時之可用餘額]
                            string strReleaseBal = "";//解除餘額
                            string strErrorCodeAndMsg = "";
                            strReleaseBal = warn.Require33401ForRemove(DocNo, ref strErrorCodeAndMsg, LogonUser);
                            //將33401主機返回的可用餘額寫回到WarningMaster的解除餘額
                            int i_result = warn.WriteReleaseBal(DocNo, strReleaseBal);
                            //將WarningDetails表中的正本欄位賦空值20180726
                            int i_result1 = warn.WriteOriginal(DocNo, NotificationSource, "",No_165);
                        }
                        else
                        {
                            strRelieveDate = "";
                            ReturnCode = "0";
                            strTime = "";
                        }
                        return Json(new { ReturnCode = ReturnCode, RelieveDate = strRelieveDate, RelieveTime = strTime, ErrorMsg = strlist[1] });
                    }
                    else
                    {
                        return Json(new { ReturnCode = "0", ErrorMsg = "執行電文發生錯誤！" });
                    }
                }
                else
                {
                    return Json(new { ReturnCode = "0", ErrorCode = "", ErrorMsg = "抱歉, 您的RCAF帳號登入異常, 無法使用此功能!" });
                }
            }
        }
        /// <summary>
        /// 警示狀態取消解除
        /// </summary>
        /// <param name="DocNo"></param>
        /// <returns></returns>
        public ActionResult WarningCancelRemove(string DocNo, string NotificationSource,string No_165)
        {
            
            string NotificationSourceCode = "";
            if (NotificationSource == "非電話詐財")
            {
                NotificationSourceCode = "11";
            }
            else if (NotificationSource == "電話詐財")
            {
                NotificationSourceCode = "12";
            }
            else if (NotificationSource == "偵辦刑事案件")
            {
                NotificationSourceCode = "14";
            }
            WarningState wstate = new WarningState();
            //當經辦登入外來文系統時未輸入RCAF的帳號、密碼及分行時，登入之權限應不可執行任何有關電文的交易按鈕。 IR-0048
            if (!String.IsNullOrEmpty(No_165))
            {
                // No_165 有值用今天解除寫到
                string strRelieveDate = "";//解除警示日期
                string ReturnCode = "";
                strRelieveDate = "";
                string strTime = "";
                ReturnCode = "1";
                //將WarningDetails表中的正本欄位賦Y值20180726
                int i_result1 = warn.CancelOriginal(DocNo, NotificationSource, "Y",No_165);
                return Json(new { ReturnCode = ReturnCode, RelieveDate = strRelieveDate, RelieveTime = strTime, ErrorMsg = "" });
            }
            else
            {
                if (!string.IsNullOrEmpty(LogonUser.RCAFAccount.Trim()) && !string.IsNullOrEmpty(LogonUser.RCAFPs.Trim()) && !string.IsNullOrEmpty(LogonUser.RCAFBranch.Trim()))
                {
                    string strResult = warn.Require9091ForRemove(wstate, "0", DocNo, "", NotificationSourceCode, "", "", System.Guid.NewGuid().ToString(), "", LogonUser, "Y");
                    string strRelieveDate = "";//解除警示日期
                    string ReturnCode = "";
                    string strTime = "";
                    if (strResult != "")
                    {
                        string[] strlist = strResult.Split('|');
                        if (strlist[0] == "0000")//如果執行成功
                        {
                            strRelieveDate = "";
                            ReturnCode = "1";
                            //將WarningDetails表中的正本欄位賦Y值20180726
                            int i_result1 = warn.CancelOriginal(DocNo, NotificationSource, "Y","");
                        }
                        else
                        {
                            ReturnCode = "0";
                        }
                        return Json(new { ReturnCode = ReturnCode, RelieveDate = strRelieveDate, RelieveTime = strTime, ErrorMsg = strlist[1] });
                    }
                    else
                    {
                        return Json(new { ReturnCode = "0", ErrorMsg = "執行電文發生錯誤！" });
                    }
                }
                else
                {
                    return Json(new { ReturnCode = "0", ErrorCode = "", ErrorMsg = "抱歉, 您的RCAF帳號登入異常, 無法使用此功能!" });
                }
            }
        }
        /// <summary>
        /// 重查按鈕方法
        /// </summary>
        /// <param name="CustAccount"></param>
        /// <returns></returns>
        public ActionResult RequireAgain33401(string CustAccount, string Currency, string DocNo)
        {
            WarningMaster model = new WarningMaster();
            model.CustAccount = CustAccount;
            ViewBag.CustAccount = CustAccount;
            model.DocNo = DocNo;
            model.BankName = new SelectList(caseMaster.GetCodeData("RCAF_BRANCH"), "CodeNo", "CodeDesc").FirstOrDefault().Text;
            //WarningMaster model = new WarningMaster();
            //model.CustAccount = CustAccount;
            model.Currency = Currency;
            //Add by zhangwei 20180315 start
            //發送電文33401
            string strErrorCodeAndMsg = "";
            //當經辦登入外來文系統時未輸入RCAF的帳號、密碼及分行時，登入之權限應不可執行任何有關電文的交易按鈕。 IR-0048
            if (!string.IsNullOrEmpty(LogonUser.RCAFAccount.Trim()) && !string.IsNullOrEmpty(LogonUser.RCAFPs.Trim()) && !string.IsNullOrEmpty(LogonUser.RCAFBranch.Trim()))
            {
                int updSet = warn.UpdateSet(DocNo, LogonUser);
                warn.Require33401(model, ref strErrorCodeAndMsg, LogonUser);
                CaseWarningViewModel viewmodel = new CaseWarningViewModel()
                {
                    WarningMaster = model
                };
                if (strErrorCodeAndMsg == "")//代表電文交互成功
                {
                    return Json(new { ReturnCode = "1", DocNo = model.DocNo, CustId = model.CustId, ClosedDate = model.ClosedDate, CustName = model.CustName, BankID = model.BankID, BankName = model.BankName, NotifyBal = model.NotifyBal, CurBal = model.CurBal, Currency = model.Currency, VD = model.VD, MD = model.MD });
                }
                else
                {
                    string[] strlist = strErrorCodeAndMsg.Split('|');
                    return Json(new { ReturnCode = "0", ErrorCode = strlist[0], ErrorMsg = strlist[1] });
                }
            }
            else
            {
                return Json(new { ReturnCode = "0", ErrorCode = "", ErrorMsg = "抱歉, 您的RCAF帳號登入異常, 無法使用此功能!" });
            }
               
        }

        public ActionResult DownLoadExcel(string NewID,string TrnNum,string CustAccount,string ForCDateS,string ForCDateE)
        {
            string UserId = LogonUser.Account;
            MemoryStream ms = null;
            ms = warn.ExcelForTransactionDetail(NewID, TrnNum);
            if (ms != null && ms.Length > 0)
            {
                Response.ClearContent();
                Response.ClearHeaders();
            }
            else
            {
                ms = new MemoryStream();
            }
            var fileName = CustAccount +  ForCDateS + "~" + ForCDateE + ".xls";
            return File(ms.ToArray(), "application/vnd.ms-excel", fileName);
        }
        /// <summary>
        /// 根據主鍵刪除警示歷史數據
        /// </summary>
        /// <param name="NewID"></param>
        /// <returns></returns>
        public ActionResult DeleteWarningHistory(string NewID)
        {
            try
            {
                int i_result = warn.DeleteWarningHistoryData(NewID);
                return Json(new { ReturnCode = "1" });
            }
            catch(Exception ex)
            {
                return Json(new { ReturnCode = "0" });
            }
            
        }
    }
}