using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CTBC.CSFS.BussinessLogic;
using CTBC.CSFS.Models;
using CTBC.CSFS.Pattern;
using CTBC.CSFS.ViewModels;
using CTBC.CSFS.Resource;

namespace CTBC.CSFS.Areas.Agent.Controllers
{
    public class AgentDepartmentAccessController : AppController
    {
        AgentDepartmentAccessBIZ ada = new AgentDepartmentAccessBIZ();
        CommonBIZ comb = new CommonBIZ();
        // GET: Agent/AgentDepartmentAccess
        public ActionResult Index(Guid caseId)
        {
            ViewBag.CaseId = caseId;
            //ada.GetCodeData("");
            IList<AgentDepartmentAccess> list = ada.GetDataFromCaseDtAccess(caseId);
            return View(list);
        }
        public ActionResult Create(Guid CaseID)
        {
            AgentDepartmentAccess ADAccess = new AgentDepartmentAccess();
            ADAccess.CaseId = CaseID;
            AgentDepartmentAccessViewModel model = new AgentDepartmentAccessViewModel()
            {
                AgentDeptAccess = ADAccess,
            };
            ViewBag.IsuseList = new SelectList(comb.GetCodeData("DEPT_ACCESS_SHORT"), "CodeMemo", "CodeDesc");
            //IList<PARMCode> list= comb.GetCodeData("DEPT_ACCESS_SHORT");
            return PartialView("Create", model);
        }
        public ActionResult Edit(int AccessId)
        {
            AgentDepartmentAccess list = ada.GetDataByAccessId(AccessId);
            AgentDepartmentAccessViewModel model = new AgentDepartmentAccessViewModel();
            {
                model.AgentDeptAccess = list;
            };
            ViewBag.IsuseList = new SelectList(comb.GetCodeData("DEPT_ACCESS_SHORT"), "CodeMemo", "CodeDesc");
            return PartialView("Edit", model);
        }
        public ActionResult Delete(int AccessId)
        {
            return Json(ada.Delete(AccessId) > 0 ? new JsonReturn() { ReturnCode = "1", ReturnMsg = "" }
                                                                    : new JsonReturn() { ReturnCode = "0", ReturnMsg = Lang.csfs_add_fail });
        }
        public ActionResult DoCreate(AgentDepartmentAccess model)
        {
            model.CreatedUser = LogonUser.Account;
            return Json(ada.Create(model) ? new JsonReturn() { ReturnCode = "1", ReturnMsg = "" }
                                                               : new JsonReturn() { ReturnCode = "0", ReturnMsg = Lang.csfs_add_fail });
           
        }
        public ActionResult DoEdit(AgentDepartmentAccess model)
        {
            model.ModifiedUser = LogonUser.Account;
            return Json(ada.Edit(model) ? new JsonReturn() { ReturnCode = "1", ReturnMsg = "" }
                                                               : new JsonReturn() { ReturnCode = "0", ReturnMsg = Lang.csfs_add_fail });
        }
    }
}