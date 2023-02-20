using CTBC.CSFS.Pattern;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
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
using CTBC.FrameWork.Platform;

namespace CTBC.CSFS.Areas.QueryAndExport.Controllers
{
    public class WarningImportQueryController : AppController
    {
        WarningImportQueryBiz _WarningImportQueryBiz;
        public WarningImportQueryController()
        {
            _WarningImportQueryBiz = new WarningImportQueryBiz();
        }
        [RootPageFilter]
        public ActionResult Index()
        {
            WarningImportQueryData model = new WarningImportQueryData();
            return View(model);
        }
        [HttpPost]
        public ActionResult _QueryResult(WarningImportQueryData model, int pageNum = 1, string strSortExpression = "Col_ID", string strSortDirection = "asc")
        {
            string UserId = LogonUser.Account;
            if (!string.IsNullOrEmpty(model.ExecutedDateE))
            {
                model.ExecutedDateE = UtlString.FormatDateTwStringToAd(model.ExecutedDateE);
            }
            if (!string.IsNullOrEmpty(model.ExecutedDateS))
            {
                model.ExecutedDateS = UtlString.FormatDateTwStringToAd(model.ExecutedDateS);
            }

            IList<WarningImportQueryData> list = _WarningImportQueryBiz.GetWarningImportQueryData(model, pageNum, strSortExpression, strSortDirection, UserId);
            var dtvm = new WarningImportQueryDataViewModel()
            {
                WarningImportQueryData = model,
                WarningImportQueryDatalist = list,
            };
            //分頁相關設定
            dtvm.WarningImportQueryData.PageSize = _WarningImportQueryBiz.PageSize;
            dtvm.WarningImportQueryData.CurrentPage = _WarningImportQueryBiz.PageIndex;
            dtvm.WarningImportQueryData.TotalItemCount = _WarningImportQueryBiz.DataRecords;
            dtvm.WarningImportQueryData.SortExpression = strSortExpression;
            dtvm.WarningImportQueryData.SortDirection = strSortDirection;

            dtvm.WarningImportQueryData.ExecutedDateS = model.ExecutedDateS;
            dtvm.WarningImportQueryData.ExecutedDateE = model.ExecutedDateE;
            dtvm.WarningImportQueryData.COL_PID = model.COL_PID;
            dtvm.WarningImportQueryData.COL_ID = model.COL_ID;
            dtvm.WarningImportQueryData.COL_165CASE = model.COL_165CASE;
            dtvm.WarningImportQueryData.COL_ACCOUNT2 = model.COL_ACCOUNT2;

            
            return PartialView("_QueryResult", dtvm);
        }
        
	}
}