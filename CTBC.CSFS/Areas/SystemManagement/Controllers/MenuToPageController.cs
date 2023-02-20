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
    public class MenuToPageController : AppController
    {
        private PARMMenuBIZ _parmMenuBiz;

        public MenuToPageController()
        {
            //創建數據庫操作類對象
            _parmMenuBiz = new PARMMenuBIZ(this);
        }

        // GET: SystemManagement/MenuToPage
        /// <summary>
        /// 顯示查詢畫面
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            PARMMenuViewModel viewModel = new PARMMenuViewModel() { PARMMenuXMLNodeList = _parmMenuBiz.GetMenuList("P") };
            return View(viewModel);
        }

        /// <summary>
        /// 顯示修改畫面
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public ActionResult Edit(string ID, string TITLE, string md_funcID)
        {
            PARMMenuXMLNode node = new PARMMenuXMLNode()
            {
                ID = Convert.ToInt32(ID),
                TITLE = TITLE,
                md_FuncID = md_funcID
            };
            List<PARMMenuXMLNode> menu = _parmMenuBiz.GetOneMenuToPage(ID);
            PARMMenuViewModel viewModel = new PARMMenuViewModel()
            {
                PARMMenuXMLNode = node,
                PARMMenuXMLNodeList = _parmMenuBiz.GetCheckedMenuToPageList(menu)
            };
            return View(viewModel);
        }

        /// <summary>
        /// 儲存修改
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public ActionResult DoEdit(FormCollection model)
        {
            int rtn = 0;
            rtn = _parmMenuBiz.SaveOneMenu(Convert.ToInt32(model["PARMMenuXMLNode.ID"]), model["md_AuthZ_Seleted"], "M", "2");
            if (rtn >= 0) return Json(new JsonReturn() { ReturnCode = "1", ReturnMsg = "alert('" + Lang.csfs_menu_msg + "[" + Lang.ResourceManager.GetString(model["PARMMenuXMLNode.TITLE"].ToString().Trim()) + "]" + Lang.csfs_menu_msg1 + "')" });
            else
                return Json(new JsonReturn() { ReturnCode = "0", ReturnMsg = Lang.csfs_edit_fail });
        }
    }
}