using CTBC.CSFS.Pattern;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CTBC.CSFS.Models;
using CTBC.CSFS.BussinessLogic;
using CTBC.CSFS.ViewModels;
using CTBC.CSFS.Resource;
using CTBC.FrameWork.Util;
using System.IO;
using CTBC.CSFS.Filter;

namespace CTBC.CSFS.Areas.QueryAndExport.Controllers
{
    public class FeesTableController : AppController
    {
        CaseMasterBIZ casemaster = new CaseMasterBIZ();
        // GET: QueryAndExport/FeesTable
        [RootPageFilter]
        public ActionResult Index()
        {
            ViewBag.Date = UtlString.FormatDateTw(DateTime.Now.ToString("yyyy/MM/dd"));
            InitDropdownListOptions();
            //CaseMaster model = new CaseMaster { HangingDate = UtlString.FormatDateTw(DateTime.Now.ToString("yyyy/MM/dd")) };
            //return View(model);
            return View();
        }

        /// <summary>
        /// 綁定下拉菜單
        /// </summary>
        public void InitDropdownListOptions()
        {
            PARMCodeBIZ parm = new PARMCodeBIZ();
            ViewBag.GOV_KINDList = new SelectList(parm.GetCodeData("GOV_KIND"), "CodeDesc", "CodeDesc");        //* 來文機關類型
            ViewBag.SpeedList = new SelectList(parm.GetCodeData("INCOME_SPEED"), "CodeDesc", "CodeDesc");       //* 速別
            ViewBag.ReceiveKindList = new SelectList(parm.GetCodeData("INCOME_TYPE"), "CodeDesc", "CodeDesc");  //* 來文方式
            ViewBag.AgentUserList = new SelectList(new LdapEmployeeBiz().GetAllEmployeeInEmployeeView(), "EmpID", "EmpIdAndName");
            BindIsEnableList();//*綁定歸還狀態

        }

        /// <summary>
        /// 綁定歸還狀態
        /// </summary>
        public void BindIsEnableList()
        {
            List<SelectListItem> item2 = new List<SelectListItem>
            {
                new SelectListItem() {Text ="未銷帳", Value = "1"},
                new SelectListItem() {Text ="已銷帳", Value = "2"}
            };
            ViewBag.PayStatusList = item2;
        }

        public ActionResult _QueryResult(CaseMaster model, int pageNum = 1, string strSortExpression = "CaseId", string strSortDirection = "asc")
        {
            model.GovDateStart = UtlString.FormatDateTwStringToAd(model.GovDateStart);
            model.GovDateEnd = UtlString.FormatDateTwStringToAd(model.GovDateEnd);

            model.SendDateS = UtlString.FormatDateTwStringToAd(model.SendDateS);
            model.SendDateE = UtlString.FormatDateTwStringToAd(model.SendDateE);

            model.HangingDateStart = UtlString.FormatDateTwStringToAd(model.HangingDateStart);
            model.HangingDateEnd = UtlString.FormatDateTwStringToAd(model.HangingDateEnd);

            model.ChargeOffsDateStart = UtlString.FormatDateTwStringToAd(model.ChargeOffsDateStart);
            model.ChargeOffsDateEnd = UtlString.FormatDateTwStringToAd(model.ChargeOffsDateEnd);

            if (model.buttontype == "excel")//匯出
            {
                MemoryStream ms = new MemoryStream();
                string fileName = string.Empty;
                ms = casemaster.FeeChargeOffsExcel_NPOI(model);
                fileName = "手續費統計表_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xls";

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
            else//查詢
            {
                return PartialView("_QueryResult", CaseMasterForPaySearchList(model, pageNum, strSortExpression, strSortDirection));
            }
        }

        /// <summary>
        /// 實際查詢所有案件未歸還資料動作
        /// </summary>
        /// <param name="AToH"></param>
        /// <param name="pageNum"></param>
        /// <param name="strSortExpression"></param>
        /// <param name="strSortDirection"></param>
        /// <returns></returns>
        public CaseSeizureViewModel CaseMasterForPaySearchList(CaseMaster model, int pageNum = 1, string strSortExpression = "CaseId", string strSortDirection = "asc")
        {
            List<CaseMaster> result = casemaster.CaseMasterForPaySearchList(model, pageNum, strSortExpression, strSortDirection);

            string strCaseId = string.Empty;
            if (result != null && result.Any())
            {
                foreach (var item in result)
                {
                    strCaseId += "'" + item.CaseId + "',";
                    if (item.SendDate != null && item.SendDate != "")
                    { item.SendDate = Convert.ToDateTime(item.SendDate).ToString("yyyy/MM/dd"); }
                    if (item.HangingDate != null && item.HangingDate != "")
                    { item.HangingDate = Convert.ToDateTime(item.HangingDate).ToString("yyyy/MM/dd"); }
                    if (item.ChargeOffsDate != null && item.ChargeOffsDate != "")
                    { item.ChargeOffsDate = Convert.ToDateTime(item.ChargeOffsDate).ToString("yyyy/MM/dd"); }
                }
                strCaseId = strCaseId.TrimEnd(',');
                List<CaseMemo> listMemo = new CaseMemoBiz().MemoList(strCaseId);
                foreach (var item in result)
                {
                    foreach (var items in listMemo.Where(m => m.CaseId == item.CaseId))
                    {
                        if (!string.IsNullOrEmpty(items.Memo))
                        {
                            item.ReceivePerson = items.Memo;
                        }
                    }
                }
            }

            var viewModel = new CaseSeizureViewModel()
            {
                CaseMaster = model,
                CaseMasterlistO = result,
            };

            viewModel.CaseMaster.PageSize = casemaster.PageSize;
            viewModel.CaseMaster.CurrentPage = casemaster.PageIndex;
            viewModel.CaseMaster.TotalItemCount = casemaster.DataRecords;
            viewModel.CaseMaster.SortExpression = strSortExpression;
            viewModel.CaseMaster.SortDirection = strSortDirection;

            return viewModel;
        }

        public ActionResult SetHangingDate(string strIds, string hangingDate)
        {
            //* 如果不為空,則轉換日期格式
            hangingDate = !string.IsNullOrEmpty(hangingDate) ? UtlString.FormatDateTwStringToAd(hangingDate) : hangingDate;
            string[] aryId = strIds.Split(',');
            return Json(casemaster.SetHangingDate(aryId, hangingDate));
        }

        public ActionResult SetChargeDate(Guid CaseId, string PayeeId, string ChargeDate, string ChargeAmount, string Memo)
        {
            if (string.IsNullOrEmpty(ChargeDate))
            {
                ChargeDate = null;
                ChargeAmount = null;
                Memo = null;
            }
            else
            {
                ChargeDate = UtlString.FormatDateTwStringToAd(ChargeDate);
            }
            string userId = LogonUser.Account;
            return Json(casemaster.SetChargeDate(CaseId, PayeeId, ChargeDate, ChargeAmount, Memo, userId) > 0 ? new JsonReturn() { ReturnCode = "1", ReturnMsg = "" }
                                                        : new JsonReturn() { ReturnCode = "0", ReturnMsg = Lang.csfs_del_fail });
        }
    }
}