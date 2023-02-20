using CTBC.CSFS.Pattern;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CTBC.CSFS.Models;
using CTBC.CSFS.BussinessLogic;
using CTBC.FrameWork.Util;
using CTBC.CSFS.ViewModels;
using System.IO;
using CTBC.CSFS.Filter;
using CTBC.CSFS.Resource;

namespace CTBC.CSFS.Areas.QueryAndExport.Controllers
{
    public class DebtPersonQueryController : AppController
    {
        PARMCodeBIZ parm = new PARMCodeBIZ();
        CaseMasterBIZ casemaster = new CaseMasterBIZ();
        static string strWhere = string.Empty;

        // GET: QueryAndExport/DebtPersonQuery
        [RootPageFilter]
        public ActionResult Index(string isBack)
        {
            CaseMaster model = new CaseMaster();
            if (isBack == "1")
            {
                //*執行cookie里的內容
                HttpCookie getQueryCookie = Request.Cookies.Get("QueryCookie");
                if (getQueryCookie != null)
                {
                    if (getQueryCookie.Values["ObligorNo"] != null) model.ObligorNo = getQueryCookie.Values["ObligorNo"];
                    if (getQueryCookie.Values["GovNo"] != null) model.GovNo = getQueryCookie.Values["GovNo"];
                    if (getQueryCookie.Values["CaseKind"] != null) model.CaseKind = getQueryCookie.Values["CaseKind"];
                    if (getQueryCookie.Values["CaseKind2"] != null) ViewBag.CaseKind2Query = getQueryCookie.Values["CaseKind2"];
                    if (getQueryCookie.Values["CreatedDateStart"] != null) model.CreatedDateStart = getQueryCookie.Values["CreatedDateStart"];
                    if (getQueryCookie.Values["CreatedDateEnd"] != null) model.CreatedDateEnd = getQueryCookie.Values["CreatedDateEnd"];
                    ViewBag.CurrentPage = getQueryCookie.Values["CurrentPage"];
                    ViewBag.isQuery = "1";
                }
            }

            //ViewBag.CaseKindList = new SelectList(parm.GetCodeData("CASE_KIND"), "CodeDesc", "CodeDesc");
            ViewBag.CaseKind2List = new SelectList(parm.GetCodeData("CASE_SEIZURE"), "CodeDesc", "CodeDesc");
            List<SelectListItem> item = new List<SelectListItem>
            {
                new SelectListItem() {Text = Lang.csfs_menu_tit_caseseizure, Value = Lang.csfs_menu_tit_caseseizure},
            };
            ViewBag.CaseKindList = item;
            return View(model);
        }

        public ActionResult _QueryResult(CaseMaster model, int pageNum = 1, string strSortExpression = "CreatedDate", string strSortDirection = "asc")
        {
            HttpCookie modelCookie = new HttpCookie("QueryCookie");
            modelCookie.Values.Add("ObligorNo", model.ObligorNo);
            modelCookie.Values.Add("GovNo", model.GovNo);
            modelCookie.Values.Add("CaseKind", model.CaseKind);
            modelCookie.Values.Add("CaseKind2", model.CaseKind2);
            modelCookie.Values.Add("CreatedDateStart", model.CreatedDateStart);
            modelCookie.Values.Add("CreatedDateEnd", model.CreatedDateEnd);
            modelCookie.Values.Add("CurrentPage", pageNum.ToString());
            Response.Cookies.Add(modelCookie);

            //APLog Redis ader 2022-07-07 - START
            //return PartialView("_QueryResult", CaseMasterSearchList(model, pageNum, strSortExpression, strSortDirection));
            var vm = CaseMasterSearchList(model, pageNum, strSortExpression, strSortDirection);
            if (vm.CaseMasterlistO != null && vm.CaseMasterlistO.Count > 0)
            {
                casemaster.SaveAPLog(vm.CaseMasterlistO.Select(x => x.ObligorNo).ToArray());
            }
            return PartialView("_QueryResult", vm);
            //APLog Redis ader 2022-07-07 - END

        }

        /// <summary>
        /// 實際查詢所有案件未歸還資料動作
        /// </summary>
        /// <param name="AToH"></param>
        /// <param name="pageNum"></param>
        /// <param name="strSortExpression"></param>
        /// <param name="strSortDirection"></param>
        /// <returns></returns>
        public CaseSeizureViewModel CaseMasterSearchList(CaseMaster model, int pageNum = 1, string strSortExpression = "CreatedDate", string strSortDirection = "asc")
        {
            model.CreatedDateStart = UtlString.FormatDateTwStringToAd(model.CreatedDateStart);
            model.CreatedDateEnd = UtlString.FormatDateTwStringToAd(model.CreatedDateEnd);

            List<CaseMaster> result = casemaster.CaseMasterSearchList(model, pageNum, strSortExpression, strSortDirection, ref strWhere);

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

        public ActionResult CaseObligorExcel(string ObligorNo, string StartDate, string EndDate, string CaseKind, string CaseKind2, string GovNo)
        {
            StartDate = UtlString.FormatDateTwStringToAd(StartDate);
            EndDate = UtlString.FormatDateTwStringToAd(EndDate);
            MemoryStream ms = new MemoryStream();
            string fileName = string.Empty;
            ms = casemaster.CaseObligorListReportExcel_NPOI(strWhere);
            fileName = Lang.csfs_menu_tit_debtpersonquery + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xls";

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