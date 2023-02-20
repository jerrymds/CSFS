using System;
using System.Collections.Generic;
using System.Web.Mvc;
using CTBC.CSFS.BussinessLogic;
using CTBC.CSFS.Models;
using CTBC.CSFS.Pattern;
using CTBC.CSFS.Resource;
using CTBC.CSFS.ViewModels;

namespace CTBC.CSFS.Areas.SystemManagement.Controllers
{
    /// <summary>
    /// Page to Action
    /// </summary>
    public class PageToActionController : AppController
    {
        private PARMMenuBIZ _parmMenuBiz;

        public PageToActionController()
        {
            //創建數據庫操作類對象
            _parmMenuBiz = new PARMMenuBIZ(this);
        }

        
        /// <summary>
        /// 查詢
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            PARMMenuViewModel viewModel = new PARMMenuViewModel{ PARMMenuXMLNodeList = _parmMenuBiz.GetPageList() };
            return View(viewModel);
        }

        /// <summary>
        /// 顯示修改畫面
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
       
        public ActionResult Edit(PARMMenuXMLNode node)
        {
            List<PARMMenuXMLNode> menuList = _parmMenuBiz.GetOnePage(node.ID.ToString());
            PARMMenuViewModel viewModel = new PARMMenuViewModel()
            {
                PARMMenuXMLNode = node,
                PARMMenuXMLNodeList = _parmMenuBiz.GetCheckedActionList(menuList)
            };
            return View(viewModel);
        }
        /// <summary>
        /// 實際進行修改動作.返回json
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public ActionResult DoEdit(FormCollection model)
        {
            return Json(_parmMenuBiz.SaveOneMenu(Convert.ToInt32(model["PARMMenuXMLNode.ID"]), model["md_AuthZ_Seleted"], "A", "3") > 0 
                                                        ? new JsonReturn() { ReturnCode = "1", ReturnMsg = "" }
                                                        : new JsonReturn() { ReturnCode = "0", ReturnMsg = Lang.csfs_edit_fail });
        }

    }
}