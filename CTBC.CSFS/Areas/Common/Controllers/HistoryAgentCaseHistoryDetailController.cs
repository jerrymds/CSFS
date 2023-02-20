using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CTBC.CSFS.BussinessLogic;
using CTBC.CSFS.Models;
using CTBC.CSFS.ViewModels;
using CTBC.CSFS.Pattern;

namespace CTBC.CSFS.Areas.Common.Controllers
{
    public class HistoryAgentCaseHistoryDetailController : AppController
    {
        HistoryAgentHandleDetailBIZ ahdBIZ = new HistoryAgentHandleDetailBIZ();
        // GET: Agent/AgentCaseHistory
        public ActionResult Index(HistoryAgentHandleDetail AHD, string FromControl)
        {
            ViewBag.CaseId = AHD.CaseId;
            ViewBag.FromControl = FromControl;
            return View(SearchList(AHD));
        }

        public HistoryAgentHandleDetailViewModel SearchList(HistoryAgentHandleDetail AHD)
        {
            HistoryAgentHandleDetailViewModel viewModel;
            AHD.LanguageType = Session["CultureName"].ToString();
            IList<HistoryAgentHandleDetail> result = ahdBIZ.GetQueryList(AHD);

            viewModel = new HistoryAgentHandleDetailViewModel()
            {
                AgentHandleDetail = AHD,
                AgentHandleDetailList = result,
            };

            return viewModel;
        }
    }
}