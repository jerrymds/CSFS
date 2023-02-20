using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CTBC.CSFS.BussinessLogic;
using CTBC.CSFS.Models;
using CTBC.CSFS.Pattern;
using CTBC.CSFS.Resource;
using CTBC.CSFS.ViewModels;

namespace CTBC.CSFS.Areas.Common.Controllers
{
    public class HistoryAgentCaseMemoDetailController : AppController
    {
        HistoryCaseMemoBiz memoBiz = new HistoryCaseMemoBiz();
        // GET: Agent/AgentCaseMemoDetail

        public ActionResult Index(HistoryCaseMemo memo, string FromControl)
        {
            ViewBag.CaseId = memo.CaseId;
            ViewBag.FromControl = FromControl;
            return View(SearchList(memo, memo.CaseId));
        }

        public HistoryCaseMemoViewModel SearchList(HistoryCaseMemo memo,Guid caseId)
        {
            HistoryCaseMemo Model =new HistoryCaseMemo();
            memo.LanguageType = Session["CultureName"].ToString();
            IList<HistoryCaseMemo> result = memoBiz.GetQueryList(memo, CaseMemoType.CaseMemo);           
            Model = memoBiz.Memo(caseId, CaseMemoType.CaseMemo);
            Model.CaseId = caseId;
            HistoryCaseMemoViewModel viewModel = new HistoryCaseMemoViewModel()
            {
                HistoryCaseMemo = Model,
                HistoryCaseMemoList = result,
            };

            return viewModel;
        }

        public ActionResult CreateMemo(HistoryCaseMemoViewModel model)
        {
            model.HistoryCaseMemo.MemoType = CaseMemoType.CaseMemo;
            model.HistoryCaseMemo.MemoUser = LogonUser.Account;
            return Json(memoBiz.SaveMemo(model.HistoryCaseMemo) ? new JsonReturn() { ReturnCode = "1", ReturnMsg = "" }
                                                : new JsonReturn() { ReturnCode = "0", ReturnMsg = Lang.csfs_save_fail });
            
        }        
    }
}