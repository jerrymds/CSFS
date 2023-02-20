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

namespace CTBC.CSFS.Areas.Common.Controllers
{
    public class HistoryAgentSendSettingInfoDetailController : AppController
    {
        HistoryCaseSendSettingBIZ cssBIZ = new HistoryCaseSendSettingBIZ();
        PARMCodeBIZ parm = new PARMCodeBIZ();
        #region 查詢
        public ActionResult Index(Guid caseId, string FromControl)
        {
            ViewBag.CaseId = caseId;
            ViewBag.FromControl = FromControl;
            PARMCodeBIZ para = new PARMCodeBIZ();
            ViewBag.AddDay = (Convert.ToInt32(para.GetCASE_END_TIME("OVERDUE_DAYS"))) * 1;
            HistoryCaseMasterBIZ casemaster = new HistoryCaseMasterBIZ();
            HistoryCaseMaster model = casemaster.MasterModel(caseId);
            ViewBag.LimiteDate = model.LimitDate;
            ViewBag.CaseNo = model.CaseNo;
            ViewBag.Status = model.Status;
            ViewBag.CaseKind2 = model.CaseKind2;
            ViewBag.AfterSeizureApproved = model.AfterSeizureApproved;
            ViewBag.ReturnReasonList = new SelectList(parm.GetCodeData("DIRECTOR_RETURNREASON"), "CodeDesc", "CodeDesc");
            //20170421 固定 RQ-2015-019666-017 派件至跨單位 宏祥 add start
            //判斷分行主管角色關閉發文資訊功能
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
            if (strRoleLDAPId == "CSFS015")
            {
               ViewBag.IsBranchAgent = "1";
            }
            //20170421 固定 RQ-2015-019666-017 派件至跨單位 宏祥 add end
            return View(SearchList(caseId));
        }

        public IList<HistoryCaseSendSettingQueryResultViewModel> SearchList(Guid caseId)
        {
            IList<HistoryCaseSendSettingQueryResultViewModel> listRtn = cssBIZ.GetSendSettingList(caseId);
            IList<HistoryCaseSendSettingDetails> listDetails = cssBIZ.GetSendSettingDetails(caseId);
            if (listRtn != null && listRtn.Any() && listDetails != null && listDetails.Any())
            {
                foreach (HistoryCaseSendSettingQueryResultViewModel item in listRtn)
                {
                    string receiver = "";
                    string cc = "";
                    foreach (HistoryCaseSendSettingDetails details in listDetails)
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
        }

        //新增
        public ActionResult _SendCreate(Guid caseId)
        {
            ViewBag.CaseId = caseId;
            Bind();
            CaseMaster master = new CaseMasterBIZ().MasterModel(caseId);
            ViewBag.AfterSeizureApproved = master.AfterSeizureApproved;
            IList<HistoryCaseSendSettingQueryResultViewModel> listRtn = cssBIZ.GetSendSettingList(caseId);
            HistoryCaseSendSettingCreateViewModel css = new HistoryCaseSendSettingCreateViewModel { CaseId = caseId };
            IList<HistoryCasePayeeSetting> cpsList = new HistoryCasePayeeSettingBIZ().GetPayeeSettingWhichNotSendSetting(caseId);
            #region SendDate
            //*當細類=’扣押並支付’時，第一次發文(扣押文)帶入系統日
            if (master.CaseKind2 == Lang.csfs_seizureandpay && !listRtn.Any())
            {
                css.SendDate = DateTime.Today.AddYears(-1911);
            }
            else if (ViewBag.CaseKind2 == Lang.csfs_Pay)
            {
                //* 當細類=’支付’時，如星期一處理案件，此欄位帶入本周三，如為周二~下周一處理的案件，帶入隔週三的日期
                if (Convert.ToDateTime(DateTime.Now.ToString("yyyy/MM/dd")).DayOfWeek.ToString() == "Monday")
                {
                    css.SendDate = DateTime.Now.AddDays(2).AddYears(-1911);
                }
                else
                {
                    css.SendDate =
                        DateTime.Now.AddDays(7)
                            .AddDays(-Convert.ToInt32(DateTime.Now.AddDays(6).DayOfWeek))
                            .AddDays(2)
                            .AddYears(-1911);
                }
            }
            else
            {
                css.SendDate = DateTime.Today.AddYears(-1911);
            }
            #endregion
            #region Template
            List<SelectListItem> list = new List<SelectListItem>();
            if (master.CaseKind == Lang.csfs_receive_case)
            {
                //* 除了細類為165調閱及財產申報範本;其他一律使用非165調閱範本
                list.Add(master.CaseKind2 == Lang.csfs_165_reading
                    ? new SelectListItem { Text = Lang.csfs_165_reading, Value = Lang.csfs_165_reading }
                    : new SelectListItem { Text = Lang.csfs_not_165_reading, Value = Lang.csfs_not_165_reading });
            }
            if (master.CaseKind == Lang.csfs_menu_tit_caseseizure)
            {

                if (master.CaseKind2 == Lang.csfs_seizure)
                {
                    list.Add(new SelectListItem { Text = Lang.csfs_seizure, Value = Lang.csfs_seizure });
                }
                else if (master.CaseKind2 == Lang.csfs_Pay)
                {
                    list.Add(new SelectListItem { Text = Lang.csfs_Pay, Value = Lang.csfs_Pay });
                }
                else if (master.CaseKind2 == Lang.csfs_seizureandpay)
                {
                    list.Add(new SelectListItem { Text = Lang.csfs_seizure, Value = Lang.csfs_seizure });
                    list.Add(new SelectListItem { Text = Lang.csfs_Pay, Value = Lang.csfs_Pay });
                }
                //switch (master.CaseKind2)
                //{   
                //    case Lang.csfs_seizure:
                //        list.Add(new SelectListItem { Text = Lang.csfs_seizure, Value = Lang.csfs_seizure });
                //        break;
                //    case Lang.csfs_Pay:
                //        list.Add(new SelectListItem { Text = Lang.csfs_Pay, Value = Lang.csfs_Pay });
                //        break;
                //    case Lang.csfs_seizureandpay:
                //        list.Add(new SelectListItem { Text = Lang.csfs_seizure, Value = Lang.csfs_seizure });
                //        list.Add(new SelectListItem { Text = Lang.csfs_Pay, Value = Lang.csfs_Pay });
                //        break;
                //}
            }
            ViewBag.TemplateList = list;
            #endregion
            if (cpsList != null && cpsList.Any())
            {
                css.ReceiveList = new List<HistoryCaseSendSettingDetails>();
                css.CcList = new List<HistoryCaseSendSettingDetails>();
                foreach (HistoryCasePayeeSetting item in cpsList)
                {
                    css.ReceiveList.Add(new HistoryCaseSendSettingDetails() { CaseId = caseId, SendType = 1, GovName = item.Receiver, GovAddr = item.Address });
                    css.CcList.Add(new HistoryCaseSendSettingDetails() { CaseId = caseId, SendType = 2, GovName = item.CCReceiver, GovAddr = item.Currency });
                }
            }
            return View(css);
        }

        [HttpPost]
        public ActionResult _SendCreate(HistoryCaseSendSettingCreateViewModel model)
        {
            model.SendDate = UtlString.FormatDateTwStringToAd(model.SendDate);
            model.SendNo = cssBIZ.SendNo();
            if (string.IsNullOrEmpty(model.SendNo)) { return Json(new JsonReturn() { ReturnCode = "2", ReturnMsg = Lang.csfa_NoMax }); }
            return Json(cssBIZ.SaveCreate(model) ? new JsonReturn { ReturnCode = "1", ReturnMsg = model.SendNo }
                                                        : new JsonReturn { ReturnCode = "0", ReturnMsg = Lang.csfs_save_fail });
        }
        #endregion

        //修改
        public ActionResult _SendEdit(int serialId, Guid caseId)
        {
            ViewBag.CaseId = caseId;
            Bind();
            HistoryCaseSendSettingCreateViewModel model = cssBIZ.GetCaseSettingAndDetails(serialId);
            model.SendDate = model.SendDate.AddYears(-1911);
            #region Template

            List<SelectListItem> list = new List<SelectListItem>
            {
                new SelectListItem {Text = model.Template, Value = model.Template}
            };
            ViewBag.TemplateList = list;
            #endregion

            return View("_SendEdit", model);
        }

        [HttpPost]
        public ActionResult _SendEdit(HistoryCaseSendSettingCreateViewModel model)
        {
            model.SendDate = UtlString.FormatDateTwStringToAd(model.SendDate);
            return Json(cssBIZ.SaveEdit(model) ? new JsonReturn() { ReturnCode = "1", ReturnMsg = "" }
                                                        : new JsonReturn() { ReturnCode = "0", ReturnMsg = Lang.csfs_edit_fail });
        }
        //刪除
        public ActionResult _SendDelete(int serialId)
        {
            return Content(cssBIZ.Delete(serialId) > 0 ? "true" : "false");
        }

        public ActionResult GetSubjectAndDescription(Guid caseId, string template)
        {
            HistorySendSettingRefBiz refBiz = new HistorySendSettingRefBiz();
            return Json(refBiz.GetSubjectAndDescription(caseId, template, ""));
        }
    }
}