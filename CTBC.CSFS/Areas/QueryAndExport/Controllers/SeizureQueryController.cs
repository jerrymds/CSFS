using CTBC.CSFS.Pattern;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CTBC.CSFS.BussinessLogic;
using CTBC.CSFS.Models;
using System.IO;
using CTBC.CSFS.Filter;
using CTBC.FrameWork.Util;
using CTBC.CSFS.ViewModels;
using CTBC.CSFS.Resource;

namespace CTBC.CSFS.Areas.QueryAndExport.Controllers
{
    public class SeizureQueryController : AppController
    {
        SeizureQueryBIZ SQBIZ = new SeizureQueryBIZ();
        PARMCodeBIZ parm = new PARMCodeBIZ();
        static string where = string.Empty;
        //
        // GET: /QueryAndExport/SeizureQuery/
        [RootPageFilter]
        public ActionResult Index(string isBack)
        {
            SeizureQuery model = new SeizureQuery();
            if (isBack == "1")
            {
                HttpCookie cookies = Request.Cookies.Get("QueryCookie");
                if (cookies != null)
                {
                    if (cookies.Values["CaseKind"] != null) model.CaseKind = cookies.Values["CaseKind"];
                    if (cookies.Values["CaseKind2"] != null) ViewBag.CaseKind2Query = cookies.Values["CaseKind2"];
                    if (cookies.Values["GovUnit"] != null) model.GovUnit = cookies.Values["GovUnit"];
                    if (cookies.Values["CaseNo"] != null) model.CaseNo = cookies.Values["CaseNo"];
                    if (cookies.Values["GovDateS"] != null) model.GovDateS = cookies.Values["GovDateS"];
                    if (cookies.Values["GovDateE"] != null) model.GovDateE = cookies.Values["GovDateE"];
                    if (cookies.Values["Speed"] != null) model.Speed = cookies.Values["Speed"];
                    if (cookies.Values["ReceiveKind"] != null) model.ReceiveKind = cookies.Values["ReceiveKind"];
                    if (cookies.Values["GovNo"] != null) model.GovNo = cookies.Values["GovNo"];
                    if (cookies.Values["CreatedDateS"] != null) model.CreatedDateS = cookies.Values["CreatedDateS"];
                    if (cookies.Values["CreatedDateE"] != null) model.CreatedDateE = cookies.Values["CreatedDateE"];
                    if (cookies.Values["BranchNo"] != null) model.BranchNo = cookies.Values["BranchNo"];
                    if (cookies.Values["CreateUser"] != null) model.CreateUser = cookies.Values["CreateUser"];
                    if (cookies.Values["CustId"] != null) model.CustId = cookies.Values["CustId"];
                    if (cookies.Values["CustName"] != null) model.CustName = cookies.Values["CustName"];
                    if (cookies.Values["Account"] != null) model.Account = cookies.Values["Account"];
                    if (cookies.Values["SendDateS"] != null) model.SendDateS = cookies.Values["SendDateS"];
                    if (cookies.Values["SendDateE"] != null) model.SendDateE = cookies.Values["SendDateE"];
                    if (cookies.Values["SendNo"] != null) model.SendNo = cookies.Values["SendNo"];
                    if (cookies.Values["OverDateS"] != null) model.OverDateS = cookies.Values["OverDateS"];
                    if (cookies.Values["OverDateE"] != null) model.OverDateE = cookies.Values["OverDateE"];
                    if (cookies.Values["Status"] != null) model.Status = cookies.Values["Status"];
                    if (cookies.Values["AgentUser"] != null) model.AgentUser = cookies.Values["AgentUser"];
                    ViewBag.CurrentPage = cookies.Values["CurrentPage"];
                    ViewBag.isQuery = "1";
                }
            }

            List<SelectListItem> item = new List<SelectListItem>
            {
                new SelectListItem() {Text = Lang.csfs_menu_tit_caseseizure, Value = Lang.csfs_menu_tit_caseseizure},
            };
            ViewBag.CaseKindList = item;
            InitDropdownListOptions();
            return View(model);
        }

        /// <summary>
        /// 綁定小類
        /// </summary>
        /// <param name="govKind"></param>
        /// <returns></returns>
        public JsonResult ChangCaseKind1(string caseKind)
        {
            PARMCodeBIZ parm = new PARMCodeBIZ();
            List<KeyValuePair<string, string>> items = new List<KeyValuePair<string, string>>();
            if (string.IsNullOrEmpty(caseKind)) return Json(items);
            //* 取得大類的CodeNo,以知道小類的CodeType
            var itemKind = parm.GetCodeData("CASE_KIND").FirstOrDefault(a => a.CodeDesc == caseKind);
            if (itemKind == null) return Json(items);

            var list = parm.GetCodeData(itemKind.CodeNo);
            if (list.Any())
            {
                items.AddRange(list.Select(govUnit => new KeyValuePair<string, string>(govUnit.CodeDesc.ToString(), govUnit.CodeDesc)));
            }
            return Json(items);
        }

        public ActionResult _QueryResult(SeizureQuery model, int pageNum = 1, string strSortExpression = "CaseId", string strSortDirection = "asc")
        {
            #region Cookies
            HttpCookie modelCookie = new HttpCookie("QueryCookie");
            modelCookie.Values.Add("CaseKind", model.CaseKind);
            modelCookie.Values.Add("CaseKind2", model.CaseKind2);
            modelCookie.Values.Add("GovUnit", model.GovUnit);
            modelCookie.Values.Add("CaseNo", model.CaseNo);
            modelCookie.Values.Add("GovDateS", model.GovDateS);
            modelCookie.Values.Add("GovDateE", model.GovDateE);
            modelCookie.Values.Add("Speed", model.Speed);
            modelCookie.Values.Add("ReceiveKind", model.ReceiveKind);
            modelCookie.Values.Add("GovNo", model.GovNo);
            modelCookie.Values.Add("CreatedDateS", model.CreatedDateS);
            modelCookie.Values.Add("CreatedDateE", model.CreatedDateE);
            modelCookie.Values.Add("BranchNo", model.BranchNo);
            modelCookie.Values.Add("CreateUser", model.CreateUser);
            modelCookie.Values.Add("CustId", model.CustId);
            modelCookie.Values.Add("CustName", model.CustName);
            modelCookie.Values.Add("Account", model.Account);
            modelCookie.Values.Add("SendDateS", model.SendDateS);
            modelCookie.Values.Add("SendDateE", model.SendDateE);
            modelCookie.Values.Add("SendNo", model.SendNo);
            modelCookie.Values.Add("OverDateS", model.OverDateS);
            modelCookie.Values.Add("OverDateE", model.OverDateE);
            modelCookie.Values.Add("Status", model.Status);
            modelCookie.Values.Add("AgentUser", model.AgentUser);
            modelCookie.Values.Add("CurrentPage", pageNum.ToString());
            Response.Cookies.Add(modelCookie);
            #endregion

            string UserId = LogonUser.Account;
            if (!string.IsNullOrEmpty(model.GovDateE))
            {
                model.GovDateE = UtlString.FormatDateTwStringToAd(model.GovDateE);
            }
            if (!string.IsNullOrEmpty(model.GovDateS))
            {
                model.GovDateS = UtlString.FormatDateTwStringToAd(model.GovDateS);
            }
            if (!string.IsNullOrEmpty(model.CreatedDateE))
            {
                model.CreatedDateE = UtlString.FormatDateTwStringToAd(model.CreatedDateE);
            }
            if (!string.IsNullOrEmpty(model.CreatedDateS))
            {
                model.CreatedDateS = UtlString.FormatDateTwStringToAd(model.CreatedDateS);
            }
            IList<SeizureQuery> list = SQBIZ.GetData(model, pageNum, strSortExpression, strSortDirection, UserId, ref where);
            var dtvm = new SeizureQueryViewModel()
            {
                SeizureQuery = model,
                SeizureQueryList = list,
            };

            //APLog Redis ader 2022-07-07 - ADD
            if (dtvm.SeizureQueryList != null && dtvm.SeizureQueryList.Count > 0)
            {
                SQBIZ.SaveAPLog(dtvm.SeizureQueryList.Select(x => x.CustId).ToArray());//APLog Redis ader 2022-07-07 - ADD
            }
            //APLog Redis ader 2022-07-07 - END

            //分頁相關設定
            dtvm.SeizureQuery.PageSize = SQBIZ.PageSize;
            dtvm.SeizureQuery.CurrentPage = SQBIZ.PageIndex;
            dtvm.SeizureQuery.TotalItemCount = SQBIZ.DataRecords;
            dtvm.SeizureQuery.SortExpression = strSortExpression;
            dtvm.SeizureQuery.SortDirection = strSortDirection;

            dtvm.SeizureQuery.CaseKind = model.CaseKind;
            dtvm.SeizureQuery.CaseKind2 = model.CaseKind2;
            dtvm.SeizureQuery.GovKind = model.GovKind;
            dtvm.SeizureQuery.GovUnit = model.GovUnit;
            dtvm.SeizureQuery.CaseNo = model.CaseNo;
            dtvm.SeizureQuery.GovDateS = model.GovDateS;
            dtvm.SeizureQuery.GovDateE = model.GovDateE;
            dtvm.SeizureQuery.ReceiveKind = model.ReceiveKind;
            dtvm.SeizureQuery.GovNo = model.GovNo;
            dtvm.SeizureQuery.CreatedDateS = model.CreatedDateS;
            dtvm.SeizureQuery.CreatedDateE = model.CreatedDateE;
            dtvm.SeizureQuery.Unit = model.Unit;
            dtvm.SeizureQuery.CreateUser = model.CreateUser;
            dtvm.SeizureQuery.CustId = model.CustId;
            dtvm.SeizureQuery.CustName = model.CustName;
            dtvm.SeizureQuery.Account = model.Account;
            dtvm.SeizureQuery.SendDateS = model.SendDateS;
            dtvm.SeizureQuery.SendDateE = model.SendDateE;
            dtvm.SeizureQuery.SendNo = model.SendNo;
            dtvm.SeizureQuery.Speed = model.Speed;
            dtvm.SeizureQuery.OverDateS = model.OverDateS;
            dtvm.SeizureQuery.OverDateE = model.OverDateE;
            dtvm.SeizureQuery.Status = model.Status;
            dtvm.SeizureQuery.AgentUser = model.AgentUser;

            return PartialView("_QueryResult", dtvm);
        }
        public void InitDropdownListOptions()
        {
            ViewBag.StatusName = new SelectList(SQBIZ.GetStatusName("STATUS_NAME"), "CodeNo", "CodeDesc");
            //GetStatusName
            ViewBag.CASE_END_TIME = parm.GetCASE_END_TIME("CASE_END_TIME");                                     //* 每天最晚時間
            ViewBag.GOV_KINDList = new SelectList(parm.GetCodeData("GOV_KIND"), "CodeDesc", "CodeDesc");        //* 來文機關類型
            ViewBag.SpeedList = new SelectList(parm.GetCodeData("INCOME_SPEED"), "CodeDesc", "CodeDesc");       //* 速別
            ViewBag.ReceiveKindList = new SelectList(parm.GetCodeData("INCOME_TYPE"), "CodeDesc", "CodeDesc");  //* 來文方式
            //ViewBag.CaseKindList = new SelectList(parm.GetCodeData("CASE_KIND"), "CodeDesc", "CodeDesc");
            ViewBag.CaseKind2List = new SelectList(parm.GetCodeData("CASE_SEIZURE"), "CodeDesc", "CodeDesc");
            ViewBag.AgentUserList = new SelectList(new LdapEmployeeBiz().GetAllEmployeeInEmployeeView(), "EmpID", "EmpIdAndName");
        }
        public ActionResult BuildSimpleExcel()
        {
            MemoryStream ms = new MemoryStream();
            string fileName = string.Empty;
            string[] headerColumns = new[]
                    {
                        Lang.csfs_case_no,
                        Lang.csfs_id_1,
                        Lang.csfs_name,
                        Lang.csfs_case_unit,
                        Lang.csfs_bank_name,
                        Lang.csfs_deposit_account,
                        Lang.csfs_currency_1,
                        Lang.csfs_balance,
                        Lang.csfs_cal_caseamount,
                        Lang.csfs_rate,
                        Lang.csfs_t_amt,
                        Lang.csfs_close_kind,
                        Lang.csfs_close_caseno
                    };
            ms = SQBIZ.Excel(headerColumns, where);
            fileName = Lang.csfs_menu_tit_seizurequery + "_" + Lang.csfs_debtexcel + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xls";

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
    }
}