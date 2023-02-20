/// <summary>
/// 程式說明:PARMMenu Controller - Menu
/// 維護部門:資訊管理處
/// CSFS Version:v3.0
/// 中國信託銀行 版權所有  ©  All Rights Reserved. 
/// </summary>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CTBC.FrameWork.Platform;
using CTBC.CSFS.Models;
using CTBC.CSFS.Pattern;
using CTBC.CSFS.ViewModels;
using CTBC.CSFS.Resource;
using System.Xml;
using System.Xml.Serialization;
using CTBC.CSFS.BussinessLogic;
using CSFSXMLUtil = CTBC.CSFS.Models.CSFSXMLUtil;

namespace CTBC.CSFS.Controllers
{
    public class PARMMenuController : AppController
    {
        PARMMenuBIZ _parmMenuBiz;

        public PARMMenuController()
        {
            //創建數據庫操作類對象
            _parmMenuBiz = new PARMMenuBIZ(this);
        }

        //同步到DB中的AuthZ
        public ActionResult Index()
        {
            string AAAXML = SyncAuthZXML();
            ViewBag.MenuToXML = AAAXML;
            return View();
        }

        //同步到DB中的AuthZ
        public ActionResult SyncAuthZ()
        {
            string AAAXML = SyncAuthZXML();
            if (string.IsNullOrEmpty(AAAXML))
                return Content("N");
            else
                return Content("Y");        
        }

        //MenuToRole
        public ActionResult MenuToRole()
        {
            PARMMenuViewModel viewModel = new PARMMenuViewModel();
            List<PARMMenuXMLNode> list = _parmMenuBiz.GetMenuList("M");
            viewModel.PARMMenuXMLNodeList = list;
            return View(viewModel);
        }

        [HttpGet]
        public ActionResult EditMenuToRole(string menuID)
        {
            PARMMenuXMLNode menu = _parmMenuBiz.GetOneMenu(menuID,"M","1");
            PARMMenuViewModel viewModel = new PARMMenuViewModel(){
                PARMMenuXMLNode = menu,
                CSFSRoleList = _parmMenuBiz.GetCheckedRoleList(menu.md_AuthZ)
            };
            return View(viewModel);        
        }

        [HttpPost]
        public ActionResult EditMenuToRole(FormCollection model)
        {
            string scriptStr = "";
            string mgr = "";

            //每個menu至少需勾選[系統管理者]角色
            if(string.IsNullOrEmpty(Config.GetValue("AAAMgr"))) 
                mgr = "CSFSM0001";
            else 
                mgr = Config.GetValue("AAAMgr");
            if (!string.IsNullOrEmpty(model["md_AuthZ_Seleted"]))
            {
                if (model["md_AuthZ_Seleted"].Contains(mgr))
                {
                    int rtn = 0;
                    rtn = _parmMenuBiz.SaveOneMenu(Convert.ToInt32(model["PARMMenuXMLNode.ID"]), model["md_AuthZ_Seleted"], "M", "1");
                    if (rtn >= 0) scriptStr = "alert('" + Lang.csfs_menu_msg1 + "[" + Lang.ResourceManager.GetString(model["PARMMenuXMLNode.TITLE"].ToString().Trim()) + "]" + Lang.csfs_menu_msg2 + "');";
                    else scriptStr = "alert('" + Lang.csfs_edit_fail + "')";
                }
                else
                {
                    scriptStr = "alert('" + Lang.csfs_menu_msg3 + mgr + "')";
                }
            }
            else
            {
                scriptStr = "alert('" + Lang.csfs_menu_msg3 + mgr + "')";
            }
            return JavaScript(scriptStr);
        }

        //PageToAction
        public ActionResult PageToAction()
        {
            PARMMenuViewModel viewModel = new PARMMenuViewModel();
            List<PARMMenuXMLNode> list = _parmMenuBiz.GetPageList();
            viewModel.PARMMenuXMLNodeList = list;
            return View(viewModel);       
        }

        [HttpGet]
        public ActionResult EditPageToAction(string menuID, string title, string funcid)
        {
            PARMMenuXMLNode node = new PARMMenuXMLNode(){
                ID = Convert.ToInt32(menuID),
                TITLE = title,
                md_FuncID = funcid
            };
            List<PARMMenuXMLNode> menuList = _parmMenuBiz.GetOnePage(menuID);
            PARMMenuViewModel viewModel = new PARMMenuViewModel()
            {
                PARMMenuXMLNode = node,
                PARMMenuXMLNodeList = _parmMenuBiz.GetCheckedActionList(menuList)
            };
            return View(viewModel);
        }

        [HttpPost]
        public ActionResult EditPageToAction(FormCollection model)
        {
            int rtn = 0;
            rtn = _parmMenuBiz.SaveOneMenu(Convert.ToInt32(model["PARMMenuXMLNode.ID"]), model["md_AuthZ_Seleted"], "A", "3");
            string scriptStr = "";
            if (rtn >= 0) scriptStr = "alert('" + Lang.csfs_menu_msg4 + "[" + Lang.ResourceManager.GetString(model["PARMMenuXMLNode.TITLE"].ToString().Trim()) + "]" + Lang.csfs_menu_msg5 + "');";
            else scriptStr = Lang.csfs_edit_fail;
            return JavaScript(scriptStr);
        }

        //MenuToPage
        public ActionResult MenuToPage()
        {
            PARMMenuViewModel viewModel = new PARMMenuViewModel();
            List<PARMMenuXMLNode> list = _parmMenuBiz.GetMenuList("P");
            viewModel.PARMMenuXMLNodeList = list;
            return View(viewModel);
        }

        [HttpGet]
        public ActionResult EditMenuToPage(string menuID, string title, string funcid)
        {
            PARMMenuXMLNode node = new PARMMenuXMLNode()
            {
                ID = Convert.ToInt32(menuID),
                TITLE = title,
                md_FuncID = funcid
            };
            List<PARMMenuXMLNode> menu = _parmMenuBiz.GetOneMenuToPage(menuID);
            PARMMenuViewModel viewModel = new PARMMenuViewModel()
            {
                PARMMenuXMLNode = node,
                PARMMenuXMLNodeList = _parmMenuBiz.GetCheckedMenuToPageList(menu)
            };
            return View(viewModel);
        }

        [HttpPost]
        public ActionResult EditMenuToPage(FormCollection model)
        {
            int rtn = 0;
            rtn = _parmMenuBiz.SaveOneMenu(Convert.ToInt32(model["PARMMenuXMLNode.ID"]), model["md_AuthZ_Seleted"], "M", "2");
            string scriptStr = "";
            if (rtn >= 0) scriptStr = "alert('" + Lang.csfs_menu_msg6 + "[" + Lang.ResourceManager.GetString(model["PARMMenuXMLNode.TITLE"].ToString().Trim()) + "]" + Lang.csfs_menu_msg7 + "');";
            else scriptStr = Lang.csfs_edit_fail;
            return JavaScript(scriptStr);
        }

        public ActionResult DeleteMenu()
        {
            return View();
        }

        private string SyncAuthZXML()
        {
            PARMMenuXML menu = _parmMenuBiz.SyncAuthZXML();
            string AAAXML = CSFSXMLUtil.Serialize(menu);
            AAAXML = AAAXML.Replace("PARMMenuXML", "Node").Replace("Role+Title", "Role%20Title");
            _parmMenuBiz.DeployAuthZ(AAAXML);
            SyncMenuToCache();//20140123 horace
            return AAAXML;
        }

        //自DB同步AuthZ到Cache中,20140123 horace
        private void SyncMenuToCache()
        {
            CacheManager cm = new CacheManager();
            cm.AuthZToCache();        
        }

        //------------------------------------------
        // PARMMenu管理維護
        //------------------------------------------

        /// <summary>
        /// 查詢排程設定資料條件頁
        /// </summary>
        /// <returns></returns>
        public ActionResult Query()
        {
            PARMMenuVO model = new PARMMenuVO();
            return View(model);
        }

        /// <summary>
        /// 查詢排程設定資料
        /// </summary>
        /// <param name="parmSchVO"></param>
        /// <param name="pageNum"></param>
        /// <returns></returns>
        public ActionResult _QueryResult(PARMMenuVO parmMenuVO, int pageNum = 1)
        {
            return View("_QueryResult", GetScheduleList(parmMenuVO, pageNum));
        }

        /// <summary>
        /// 取得排程設定資料(分頁)
        /// </summary>
        /// <param name="parmSchVO"></param>
        /// <param name="pageNum"></param>
        /// <returns></returns>
        public PARMMenuViewModel GetScheduleList(PARMMenuVO parmMenuVO, int pageNum = 1)
        {
            PARMMenuViewModel viewModel;

            //取出符合條件的排程設定資訊
            IList<PARMMenuVO> result = _parmMenuBiz.GetQueryList(parmMenuVO, pageNum);

            viewModel = new PARMMenuViewModel()
            {
                PARMMenuVOList = result,
                PARMMenuVO = parmMenuVO,
            };

            //分頁相關設定
            viewModel.PARMMenuVO.PageSize = _parmMenuBiz.PageSize;
            viewModel.PARMMenuVO.CurrentPage = _parmMenuBiz.PageIndex;
            viewModel.PARMMenuVO.TotalItemCount = _parmMenuBiz.DataRecords;

            return viewModel;
        }


        /// <summary>
        /// 新增一筆PARMMenu設定
        /// </summary>
        /// <returns></returns>
        public ActionResult Create()
        {
            PARMMenuVO model = new PARMMenuVO();
            ViewBag.EditMode = "Create";
            return View(model);
        }

        /// <summary>
        /// 新增一筆PARMMenu設定
        /// </summary>
        /// <param name="model">排程</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Create(PARMMenuVO model)
        {
            string scriptStr = "";
            int rtn = _parmMenuBiz.Create(model);
            if (rtn > 0)
            {
                scriptStr = "alert('" + Lang.csfs_add_ok + "');location.href='" + Url.Action("Query", "PARMMenu") + "';";
            }
            else
            {
                scriptStr = "alert('" + Lang.csfs_add_fail + "');";
            }
            return JavaScript(scriptStr);
        }

        /// <summary>
        /// 修改一筆PARMMenu設定
        /// </summary>
        /// <param name="id">排程ID</param>
        /// <returns></returns>
        public ActionResult Edit(int id)
        {
            PARMMenuVO model = _parmMenuBiz.Select(id);
            ViewBag.EditMode = "Edit";
            return View(model);
        }

        /// <summary>
        /// 修改一筆PARMMenu設定
        /// </summary>
        /// <param name="model">排程</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Edit(PARMMenuVO model)
        {
            string scriptStr = "";
            int rtn = _parmMenuBiz.Edit(model);
            if (rtn > 0)
            {
                scriptStr = "alert('" + Lang.csfs_edit_ok + "');location.href='" + Url.Action("Query", "PARMMenu") + "';";
            }
            else
            {
                scriptStr = "alert('" + Lang.csfs_edit_fail + "');";
            }
            return JavaScript(scriptStr);
        }

        /// <summary>
        /// 刪除一筆PARMMenu設定
        /// </summary>
        /// <param name="id">排程ID</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Delete(int id)
        {
            string rtnStr = "0";
            int rtn = _parmMenuBiz.Delete(id);
            if (rtn > 0)
                rtnStr = "1";
            return Content(rtnStr);
        }
    }
}
