using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Web;
using System.Web.Mvc;
using CTBC.CSFS.BussinessLogic;
using CTBC.CSFS.Filter;
using CTBC.CSFS.ViewModels;
using CTBC.CSFS.Models;
using CTBC.CSFS.Pattern;
using CTBC.CSFS.Resource;
using CTBC.FrameWork.Util;

namespace CTBC.CSFS.Areas.KeyInput.Controllers
{
    /// <summary>
    /// 建檔作業-退件待辦理
    /// </summary>
    public class CaseReturnController : AppController
    {
        PARMCodeBIZ parm = new PARMCodeBIZ();
        CaseReturnBIZ crb = new BussinessLogic.CaseReturnBIZ();

        [RootPageFilter]
        public ActionResult Index()
        {
            ViewBag.CaseKindList = new SelectList(parm.GetCodeData("CASE_KIND"), "CodeDesc", "CodeDesc");

            CaseReturn model = new CaseReturn();

            //*20150811新需求, 分行角色只能看自己分行資料且不能修改
            LdapEmployeeBiz empBiz = new LdapEmployeeBiz();
            IList<LDAPEmployee> list = empBiz.GetNoRoleEmployeeIdList();
            IList<LDAPEmployee> list2 = empBiz.GetNoRoleEmployeeIdList2();
            if ((list != null && list.Any(m => m.EmpId.Trim().ToUpper() == LogonUser.Account.Trim().ToUpper()))
                || (list2 != null && list2.Any(m => m.EmpId.Trim().ToUpper() == LogonUser.Account.Trim().ToUpper())))
            {
                //* 屬於分行角色
                model.Unit = new LdapEmployeeBiz().GetBranchId();
                ViewBag.UnitRead = "1";
            }

            
            InitDropdownListOptions();
            return View(model);
        }
        public ActionResult Query(CaseReturn model, int pageNum = 1, string strSortExpression = "CaseNo", string strSortDirection = "asc")
        {
            if (!string.IsNullOrEmpty(model.GovDate))
            {
                model.GovDate = UtlString.FormatDateTwStringToAd(model.GovDate);
            }
            if (!string.IsNullOrEmpty(model.GovDateS))
            {
                model.GovDateS = UtlString.FormatDateTwStringToAd(model.GovDateS);
            }
            if (!string.IsNullOrEmpty(model.GovDateE))
            {
                model.GovDateE = UtlString.FormatDateTwStringToAd(model.GovDateE);
            }
            if (!string.IsNullOrEmpty(model.LimitDate))
            {
                model.LimitDate = UtlString.FormatDateTwStringToAd(model.LimitDate);
            }
            IList<CaseReturn> CaseReturnList = crb.Getlist(model, pageNum, strSortExpression, strSortDirection);
            var crvm = new CaseReturnViewModel()
            {
                CaseReturn = model,
                CaseReturnList = CaseReturnList,
            };
            crvm.CaseReturn.PageSize = crb.PageSize;
            crvm.CaseReturn.CurrentPage = crb.PageIndex;
            crvm.CaseReturn.TotalItemCount = crb.DataRecords;
            crvm.CaseReturn.SortExpression = strSortExpression;
            crvm.CaseReturn.SortDirection = strSortDirection;

            crvm.CaseReturn.DocNo = model.DocNo;
            crvm.CaseReturn.GovKind = model.GovKind;
            crvm.CaseReturn.CaseNo = model.CaseNo;
            crvm.CaseReturn.Speed = model.Speed;
            crvm.CaseReturn.GovUnit = model.GovUnit;
            crvm.CaseReturn.GovNo = model.GovNo;
            crvm.CaseReturn.GovDate = model.GovDate;
            crvm.CaseReturn.LimitDate = model.LimitDate;
            crvm.CaseReturn.Unit = model.Unit;
            crvm.CaseReturn.Person = model.Person;
            return PartialView("Query", crvm);
        }
        public void InitDropdownListOptions()
        {
            ViewBag.CASE_END_TIME = parm.GetCASE_END_TIME("CASE_END_TIME");                                     //* 每天最晚時間
            ViewBag.GOV_KINDList = new SelectList(parm.GetCodeData("GOV_KIND"), "CodeDesc", "CodeDesc");        //* 來文機關類型
            ViewBag.SpeedList = new SelectList(parm.GetCodeData("INCOME_SPEED"), "CodeDesc", "CodeDesc");       //* 速別
            ViewBag.ReceiveKindList = new SelectList(parm.GetCodeData("INCOME_TYPE"), "CodeDesc", "CodeDesc");  //* 來文方式
            ViewBag.CaseKindList = new SelectList(parm.GetCodeData("CASE_KIND"), "CodeDesc", "CodeDesc");
            ViewBag.CaseKind2List = new SelectList(parm.GetCodeData("CASE_SEIZURE"), "CodeDesc", "CodeDesc");
        }
        public ActionResult CloseCase(Guid caseId, string ClosedReson, string ReturnAnswer)
        {
            return Content(new CaseReturnBIZ().KeyInpuCloseCase(caseId, ClosedReson, ReturnAnswer) ? "1" : "0");
        }
    }
}