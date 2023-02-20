using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CTBC.CSFS.BussinessLogic;
using CTBC.CSFS.Models;
using CTBC.CSFS.ViewModels;
using CTBC.CSFS.Pattern;

namespace CTBC.CSFS.Areas.Agent.Controllers
{
    public class AgentCaseHistoryController : AppController
    {
        AgentHandleDetailBIZ ahdBIZ = new AgentHandleDetailBIZ();
        // GET: Agent/AgentCaseHistory
        public ActionResult Index(AgentHandleDetail AHD)
        {
            ViewBag.CaseId = AHD.CaseId;
            return View(SearchList(AHD));
        }

        public AgentHandleDetailViewModel SearchList(AgentHandleDetail AHD)
        {
            AgentHandleDetailViewModel viewModel;
            AHD.LanguageType = Session["CultureName"].ToString();
            IList<AgentHandleDetail> result = ahdBIZ.GetQueryList(AHD);

            viewModel = new AgentHandleDetailViewModel()
            {
                AgentHandleDetail = AHD,
                AgentHandleDetailList = result,
            };

            return viewModel;
        }
    }
}