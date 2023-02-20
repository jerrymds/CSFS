using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
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
    public class AgentTransactionRecordsController : AppController
    {
        TransactionRecordsBIZ biz = new TransactionRecordsBIZ();
        // GET: Agent/AgentTransactionRecords
        public ActionResult Index(CaseDataLog TransRecords)
        {
            ViewBag.CaseId = TransRecords.CaseId;
            TransactionRecordsViewModel viewModel = new TransactionRecordsViewModel();
            viewModel.TransRecords = TransRecords;

            return View(viewModel);
        }
        [HttpPost]
        public ActionResult Query(TransactionRecordsViewModel viewModel, int pageNum = 1)
        {
            ViewBag.CaseId = viewModel.CaseId;
            CaseDataLog casedata = new CaseDataLog();
            casedata.CaseId = viewModel.CaseId;
            IList<CaseDataLog> master = biz.GetQueryList(casedata, pageNum);
            IList<CaseDataLog> detail = biz.GetQueryDetail(casedata);
            viewModel.TransRecordsMaster = master;
            viewModel.TransRecordsDetail = detail;
            viewModel.TransRecords = casedata;

            //分頁相關設定
            viewModel.TransRecords.PageSize = biz.PageSize;
            viewModel.TransRecords.CurrentPage = biz.PageIndex;
            viewModel.TransRecords.TotalItemCount = biz.DataRecords;
            return View(viewModel);
        }
        [HttpPost]
        public ActionResult Detail(CaseDataLog TransRecords)
        {
            ViewBag.CaseId = TransRecords.CaseId;
            TransactionRecordsViewModel viewModel = new TransactionRecordsViewModel();
            IList<CaseDataLog> result = biz.GetDetailList(TransRecords);
            viewModel.TransRecordsDetail = result;
            viewModel.TransRecords = result[0];
            return View(viewModel);
        }
    }
}