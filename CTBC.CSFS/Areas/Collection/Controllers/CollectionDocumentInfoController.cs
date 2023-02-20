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

namespace CTBC.CSFS.Areas.Collection.Controllers
{
    public class CollectionDocumentInfoController : AppController
    {
        //
        // GET: /Collection/CollectionDocumentInfo/
        CaseMasterBIZ casemaster = new CaseMasterBIZ();
        CaseSeizureViewModel caseview = new CaseSeizureViewModel();
        PARMCodeBIZ parm = new PARMCodeBIZ();
        CaseAttachmentBIZ attachment = new CaseAttachmentBIZ();
        CaseObligorBIZ obligor = new CaseObligorBIZ();
		CaseAccountBiz caseAccount = new CaseAccountBiz();

        // GET: Agent/AgentDocumentInfo
        public ActionResult Index(Guid caseId)
        {
            ViewBag.CaseId = caseId;
            InitDropdownListOptions();              //* 綁定頁面下拉列表

            //*判斷類型是否只讀(都沒有這個值得時候,就不能修改)
            if (casemaster.IsCaseIdExist("CaseAccountExternal", caseId) == "0" && casemaster.IsCaseIdExist("CaseSeizure", caseId) == "0" && casemaster.IsCaseIdExist("CasePayeeSetting", caseId) == "0" && casemaster.IsCaseIdExist("CaseSendSetting", caseId) == "0")
            {
                ViewBag.IsReadOnly = "isnotreadonly";
            }

            caseview.CaseMaster = casemaster.MasterModel(caseId);  //* 得到CaseMaster的model
            caseview.CaseMaster.GovDate = UtlString.FormatDateTw(caseview.CaseMaster.GovDate);//將西元年轉換為民國年
            if (caseview.CaseMaster != null)
                caseview.CaseMaster.OldCaseKind = caseview.CaseMaster.CaseKind;
            //adam 取消只有財產目錄呈現
            if (caseview.CaseMaster.CaseKind == "外來文案件") //  CaseKind2 == "財產申報")
            {
                caseview.CaseMaster.PropertyDeclaration = parm.GetCaseIdByMemo(caseview.CaseMaster.CaseId);
            }
            //}

            //* 以下是頁面初始化值
            ViewBag.CaseKindList = new SelectList(parm.GetCodeData("CASE_KIND"), "CodeDesc", "CodeDesc", caseview.CaseMaster.CaseKind);       //* 類別-大類
            string codeNo = parm.GetCodeNoByCodeDesc(caseview.CaseMaster.CaseKind);//根據大類獲得小類的list
            ViewBag.CaseKind2List = new SelectList(parm.GetCodeData(codeNo), "CodeDesc", "CodeDesc", caseview.CaseMaster.CaseKind2);   //* 類別-小類-扣押
            ViewBag.GovUnitList = new SelectList(parm.SelectGovUnitByGOV_KIND(caseview.CaseMaster.GovKind), "CodeDesc", "CodeDesc");//* 綁定來文機關下拉列表

            caseview.CaseObligorlistO = new List<CaseObligor>(10);//* 初始化義務人員行數
            caseview.CaseAttachmentlistO = attachment.AttachmentList(caseId);//* 得到CaseAttachment的model
            List<CaseObligor> list = obligor.ObligorModel(caseId);    //* 得到CaseObligor的model
            for (int i = 0; i < list.Count; i++)
            {
                caseview.CaseObligorlistO.Add(list[i]);
            }
            for (int i = list.Count; i < 10; i++)
            {
                caseview.CaseObligorlistO.Add(new CaseObligor());
            }

			CaseEdocFile caseEdocFile = caseAccount.OpenTxtDoc(caseId);
			ViewBag.HasFile = caseEdocFile == null ? "0" : "1";
            return View(caseview);
        }

        /// <summary>
        /// 綁定下拉菜單
        /// </summary>
        public void InitDropdownListOptions()
        {
            ViewBag.CASE_END_TIME = parm.GetCASE_END_TIME("CASE_END_TIME");                                     //* 每天最晚時間
            ViewBag.GOV_KINDList = new SelectList(parm.GetCodeData("GOV_KIND"), "CodeDesc", "CodeDesc");        //* 來文機關類型
            ViewBag.SpeedList = new SelectList(parm.GetCodeData("INCOME_SPEED"), "CodeDesc", "CodeDesc");       //* 速別
            ViewBag.ReceiveKindList = new SelectList(parm.GetCodeData("INCOME_TYPE"), "CodeDesc", "CodeDesc");  //* 來文方式
        }
    }
}