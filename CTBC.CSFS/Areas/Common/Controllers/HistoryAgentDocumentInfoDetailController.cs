using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CTBC.CSFS.Models;
using CTBC.CSFS.ViewModels;
using CTBC.FrameWork.Util;
using CTBC.CSFS.BussinessLogic;
using CTBC.CSFS.Pattern;
using System.Text;

namespace CTBC.CSFS.Areas.Common.Controllers
{
    public class HistoryAgentDocumentInfoDetailController : AppController
    {
        HistoryCaseSeizureViewModel caseview = new HistoryCaseSeizureViewModel();
        HistoryCaseMasterBIZ casemaster = new HistoryCaseMasterBIZ();
        HistoryCaseAttachmentBIZ attachment = new HistoryCaseAttachmentBIZ();
        HistoryCaseObligorBIZ obligor = new HistoryCaseObligorBIZ();
        HistoryCaseAccountBiz caseAccount = new HistoryCaseAccountBiz();
        HistoryImportEDocBiz CaseEdocFile = new HistoryImportEDocBiz();
        //
        // GET: /Common/HistoryAgentDocumentInfoDetail/
        public ActionResult Index(Guid CaseId, string FromControl)
        {
            ViewBag.CaseId = CaseId;
            ViewBag.FromControl = FromControl;
            caseview.HistoryCaseMaster = casemaster.MasterModel(CaseId);  //* 得到CaseMaster的model
            caseview.HistoryCaseMaster.GovDate = UtlString.FormatDateTw(caseview.HistoryCaseMaster.GovDate);//將西元年轉換為民國年
            caseview.HistoryCaseObligorlistO = new List<HistoryCaseObligor>(10);//* 初始化義務人員行數
            caseview.HistoryCaseAttachmentlistO = attachment.AttachmentList(CaseId);//* 得到CaseAttachment的model
            caseview.HistoryCaseEdocFilelist = CaseEdocFile.GetCaseEdocFileList(CaseId);//* 得到CaseEdocFile的model
            List<HistoryCaseObligor> list = obligor.ObligorModel(CaseId);    //* 得到CaseObligor的model
            for (int i = 0; i < list.Count; i++)
            {
                caseview.HistoryCaseObligorlistO.Add(list[i]);
            }
            for (int i = list.Count; i < 10; i++)
            {
                caseview.HistoryCaseObligorlistO.Add(new HistoryCaseObligor());
            }
			HistoryCaseEdocFile caseEdocFile = caseAccount.OpenTxtDoc(CaseId);
			ViewBag.HasFile = caseEdocFile == null ? "0" : "1";
            #region //受文者、來函扣押總金額、金額未達毋需扣押 默認值顯示 IR-1008

            #endregion

            //APLog Redis ader 2022-07-07 - ADD
            if (caseview.HistoryCaseObligorlistO != null && caseview.HistoryCaseObligorlistO.Count > 0)
            {
                obligor.SaveAPLog(caseview.HistoryCaseObligorlistO.Select(x => x.ObligorNo).ToArray());
            }
            //APLog Redis ader 2022-07-07 - END

            return View(caseview);
        }
    }
}