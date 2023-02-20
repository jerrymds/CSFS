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
    public class ImportEDocQueryController : AppController
    {
        ImportEDocBiz _ImportEDocBiz;
        public ImportEDocQueryController()
        {
            _ImportEDocBiz = new ImportEDocBiz();
        }
        [RootPageFilter]
        public ActionResult Index()
        {
            ImportEdocData model = new ImportEdocData();
            return View(model);
        }
        [HttpPost]
        public ActionResult _QueryResult(ImportEdocData model, int pageNum = 1, string strSortExpression = "CaseNo", string strSortDirection = "asc")
        {
            string UserId = LogonUser.Account;
            if (!string.IsNullOrEmpty(model.GovDateE))
            {
                model.GovDateE = UtlString.FormatDateTwStringToAd(model.GovDateE);
            }
            if (!string.IsNullOrEmpty(model.GovDateS))
            {
                model.GovDateS = UtlString.FormatDateTwStringToAd(model.GovDateS);
            }
            if (!string.IsNullOrEmpty(model.ExecutedDateS))
            {
                model.ExecutedDateS = UtlString.FormatDateTwStringToAd(model.ExecutedDateS);
            }
            if (!string.IsNullOrEmpty(model.ExecutedDateE))
            {
                model.ExecutedDateE = UtlString.FormatDateTwStringToAd(model.ExecutedDateE);
            }
            IList<ImportEdocData> list = _ImportEDocBiz.GetImportEdocData(model, pageNum, strSortExpression, strSortDirection, UserId);
            var dtvm = new ImportEdocDataViewModel()
            {
                ImportEdocData = model,
                ImportEdocDatalist = list,
            };
            //分頁相關設定
            dtvm.ImportEdocData.PageSize = _ImportEDocBiz.PageSize;
            dtvm.ImportEdocData.CurrentPage = _ImportEDocBiz.PageIndex;
            dtvm.ImportEdocData.TotalItemCount = _ImportEDocBiz.DataRecords;
            dtvm.ImportEdocData.SortExpression = strSortExpression;
            dtvm.ImportEdocData.SortDirection = strSortDirection;

            dtvm.ImportEdocData.GovUnit = model.GovUnit;
            dtvm.ImportEdocData.GovDateS = model.GovDateS;
            dtvm.ImportEdocData.GovDateE = model.GovDateE;
            dtvm.ImportEdocData.GovNo = model.GovNo;
            dtvm.ImportEdocData.ExecutedDateS = model.ExecutedDateS;
            dtvm.ImportEdocData.ExecutedDateE = model.ExecutedDateE;
            
            return PartialView("_QueryResult", dtvm);
        }
        
	}
}