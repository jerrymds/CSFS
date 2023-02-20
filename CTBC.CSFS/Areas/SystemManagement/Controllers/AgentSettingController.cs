using CTBC.CSFS.Pattern;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CTBC.CSFS.BussinessLogic;
using CTBC.FrameWork.Util;
using CTBC.CSFS.ViewModels;
using CTBC.CSFS.Resource;

namespace CTBC.CSFS.Areas.SystemManagement.Controllers
{
    public class AgentSettingController : AppController
    {
        AgentSettingBIZ ASBIZ = new AgentSettingBIZ();
        public ActionResult Index()
        {
            BindDropList();
            return View();
        }
        public ActionResult DoSave(string Empidarr, string Department, string IsAutoDispatch, string IsAutoDispatchFS)
        {
            string data = ASBIZ.Dosave(Empidarr, Department, IsAutoDispatch, IsAutoDispatchFS);
            int AutoDispatch = ASBIZ.GetAutoDispatchNum();//查扣押經辦人數
            int AutoDispatchFS = ASBIZ.GetAutoDispatchFSNum();//查外來文經辦人數
            bool isAutoDispatch = ASBIZ.GetEnable().IsAutoDispatch;//AutoDispatch是否啟用
            bool isAutoDispatchFS = ASBIZ.GetEnable().IsAutoDispatchFS;//AutoDispatchFS是否啟用
            if (isAutoDispatch && isAutoDispatchFS)
            {
                return Json(data == "1" ? new JsonReturn() { ReturnCode = "1", ReturnMsg = string.Format(Lang.csfs_pm_AutoDispatch_Seizure, AutoDispatch) + "<br/>" + string.Format(Lang.csfs_pm_AutoDispatch_FS, AutoDispatchFS) }
                : new JsonReturn() { ReturnCode = "0", ReturnMsg = Lang.csfs_save_fail });
            }
            else if (isAutoDispatch && !isAutoDispatchFS)
            {
                return Json(data == "1" ? new JsonReturn() { ReturnCode = "1", ReturnMsg = string.Format(Lang.csfs_pm_AutoDispatch_Seizure, AutoDispatch) }
                : new JsonReturn() { ReturnCode = "0", ReturnMsg = Lang.csfs_save_fail });
            }
            else if (!isAutoDispatch && isAutoDispatchFS)
            {
                return Json(data == "1" ? new JsonReturn() { ReturnCode = "1", ReturnMsg = string.Format(Lang.csfs_pm_AutoDispatch_FS, AutoDispatchFS) }
                : new JsonReturn() { ReturnCode = "0", ReturnMsg = Lang.csfs_save_fail });
            }
            else
            {
                return Json(data == "1" ? new JsonReturn() { ReturnCode = "1", ReturnMsg = Lang.csfs_save_ok }
                : new JsonReturn() { ReturnCode = "0", ReturnMsg = Lang.csfs_save_fail });
            }
        }

        public ActionResult BindAgentSetting(string Department)
        {
            string EmpidArry = ASBIZ.GetAgentSettingAll(Department);
           return Content(EmpidArry);
        }
        public void BindDropList()
        {
            LdapEmployeeBiz empBiz = new LdapEmployeeBiz();
            string AgentsList = JsonHelper.ObjectToJson(empBiz.GetAgentAndBu(""));
            ViewBag.AgentsList = AgentsList;
            ViewBag.Department = new SelectList(ASBIZ.GetKeBie(), "Department", "Department");
        }
    }
}