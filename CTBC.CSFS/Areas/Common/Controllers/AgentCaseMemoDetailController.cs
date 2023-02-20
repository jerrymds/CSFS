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
    public class AgentCaseMemoDetailController : AppController
    {
        CaseMemoBiz memoBiz = new CaseMemoBiz();
        // GET: Agent/AgentCaseMemoDetail

        public ActionResult Index(CaseMemo memo, string FromControl)
        {
            ViewBag.CaseId = memo.CaseId;
            ViewBag.FromControl = FromControl;
            return View(SearchList(memo, memo.CaseId));
        }

        public CaseMemoViewModel SearchList(CaseMemo memo,Guid caseId)
        {
            CaseMemo Model=new CaseMemo();
            memo.LanguageType = Session["CultureName"].ToString();
            IList<CaseMemo> result = memoBiz.GetQueryList(memo, CaseMemoType.CaseMemo);           
            Model = memoBiz.Memo(caseId, CaseMemoType.CaseMemo);
            Model.CaseId = caseId;
            CaseMemoViewModel viewModel = new CaseMemoViewModel()
            {
                CaseMemo = Model,
                CaseMemoList = result,
            };

            return viewModel;
        }

        public ActionResult CreateMemo(CaseMemoViewModel model)
        {
            model.CaseMemo.MemoType = CaseMemoType.CaseMemo;
            model.CaseMemo.MemoUser = LogonUser.Account;
            return Json(memoBiz.SaveMemo(model.CaseMemo) ? new JsonReturn() { ReturnCode = "1", ReturnMsg = "" }
                                                : new JsonReturn() { ReturnCode = "0", ReturnMsg = Lang.csfs_save_fail });
            
        }        
    }
}