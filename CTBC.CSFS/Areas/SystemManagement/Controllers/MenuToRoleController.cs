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

namespace CTBC.CSFS.Areas.SystemManagement.Controllers
{
    public class MenuToRoleController : AppController
    {
        private PARMMenuBIZ _parmMenuBiz;

        public MenuToRoleController()
        {
            //創建數據庫操作類對象
            _parmMenuBiz = new PARMMenuBIZ(this);
        }

        // GET: SystemManagement/MenuToRole
        public ActionResult Index()
        {
            PARMMenuViewModel viewModel = new PARMMenuViewModel(){ PARMMenuXMLNodeList = _parmMenuBiz.GetMenuList("M")};
            return View(viewModel);
        }

        public ActionResult Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return RedirectToAction("Index","MenuToRole", new { area = "SystemManagement"});
            }

            PARMMenuXMLNode menu = _parmMenuBiz.GetOneMenu(id, "M", "1");
            PARMMenuViewModel viewModel = new PARMMenuViewModel()
            {
                PARMMenuXMLNode = menu,
                CSFSRoleList = _parmMenuBiz.GetCheckedRoleList(menu.md_AuthZ)
            };
            return View(viewModel);
        }
        
        public ActionResult DoEdit(FormCollection model)
        {
            //每個menu至少需勾選[系統管理者]角色
            string mgr = string.IsNullOrEmpty(Config.GetValue("AAAMgr")) ? "CSFS001" : Config.GetValue("AAAMgr");
            //if (!string.IsNullOrEmpty(model["md_AuthZ_Seleted"]) && model["md_AuthZ_Seleted"].Contains(mgr))
            if (!string.IsNullOrEmpty(model["md_AuthZ_Seleted"]))
            {
                return Json(_parmMenuBiz.SaveOneMenu(Convert.ToInt32(model["PARMMenuXMLNode.ID"]), model["md_AuthZ_Seleted"], "M", "1") >= 0
                    ? new JsonReturn() {ReturnCode = "1", ReturnMsg = ""}
                    : new JsonReturn() {ReturnCode = "0", ReturnMsg = Lang.csfs_edit_fail});
            }
            return Json(new JsonReturn() { ReturnCode = "2", ReturnMsg = string.Format(Lang.csfs_menu_msg3, mgr) });
        }
        /// <summary>
        /// 點選同步按鈕,同步[AuthZ]
        /// </summary>
        /// <returns></returns>
        public ActionResult SyncAuthZ()
        {
            string aaaxml = SyncAuthZxml();
            return Json(!string.IsNullOrEmpty(aaaxml) ? new JsonReturn() { ReturnCode = "1", ReturnMsg = "" }
                                                        : new JsonReturn() { ReturnCode = "0", ReturnMsg = Lang.csfs_sync_fail });
        }

        /// <summary>
        /// 同步DB中的AtuhZ
        /// </summary>
        /// <returns></returns>
        private string SyncAuthZxml()
        {
            PARMMenuXML menu = _parmMenuBiz.SyncAuthZXML();
            string aaaxml = CSFSXMLUtil.Serialize(menu);
            aaaxml = aaaxml.Replace("PARMMenuXML", "Node").Replace("Role+Title", "Role%20Title");
            _parmMenuBiz.DeployAuthZ(aaaxml);
            SyncMenuToCache();//20140123 horace
            return aaaxml;
        }
        
        /// <summary>
        /// 自DB同步AuthZ到Cache中,20140123 horace
        /// </summary>
        private void SyncMenuToCache()
        {
            CacheManager cm = new CacheManager();
            cm.AuthZToCache();
        }
    }
}