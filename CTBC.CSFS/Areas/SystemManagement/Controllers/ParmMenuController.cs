using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CTBC.CSFS.BussinessLogic;
using CTBC.CSFS.Models;
using CTBC.CSFS.ViewModels;
using CTBC.CSFS.Pattern;
using CTBC.CSFS.Resource;

namespace CTBC.CSFS.Areas.SystemManagement.Controllers
{
    public class PARMMenuController : AppController
    {

        PARMMenuBIZ _PARMMenuBiz;

        public PARMMenuController()
        {
            //創建數據庫操作類對象
            _PARMMenuBiz = new PARMMenuBIZ(this);
        }
        
        /// <summary>
        /// 查詢排程設定資料條件頁
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            return View(new PARMMenuVO());
        }
        #region PARMMenu _QueryResult
        /// <summary>
        /// 查詢系統參數資料
        /// </summary>
        /// <param name="parmSchVO"></param>
        /// <param name="pageNum"></param>
        /// <returns></returns>
        public ActionResult _QueryResult(PARMMenuVO qryCsfsVO, int pageNum = 1, string strSortExpression = "ID", string strSortDirection = "asc")
        {
            return View("_QueryResult", PARMMenuVOList(qryCsfsVO, pageNum, strSortExpression, strSortDirection));
        }


        /// <summary>
        /// 取得系統參數資料(分頁)
        /// </summary>
        /// <param name="parmSchVO"></param>
        /// <param name="pageNum"></param>
        /// <returns></returns>
        public PARMMenuViewModel PARMMenuVOList(PARMMenuVO qryCsfsVO, int pageNum = 1, string strSortExpression = "ID", string strSortDirection = "asc")
        {
            IList<PARMMenuVO> result = _PARMMenuBiz.GetQueryList(qryCsfsVO, pageNum, strSortExpression, strSortDirection);

            var viewModel = new PARMMenuViewModel()
            {
                PARMMenuVO = qryCsfsVO,
                PARMMenuVOList = result,
            };

            //分頁相關設定
            viewModel.PARMMenuVO.PageSize = _PARMMenuBiz.PageSize;
            viewModel.PARMMenuVO.CurrentPage = _PARMMenuBiz.PageIndex;
            viewModel.PARMMenuVO.TotalItemCount = _PARMMenuBiz.DataRecords;
            viewModel.PARMMenuVO.SortExpression = strSortExpression;
            viewModel.PARMMenuVO.SortDirection = strSortDirection;

            viewModel.PARMMenuVO.QuickSearchCon = qryCsfsVO.QuickSearchCon;
            return viewModel;
        }
        #endregion

        #region PARMMenu Create
        /// <summary>
        /// 新增一筆PARMMenu設定
        /// </summary>
        /// <returns></returns>
        public ActionResult Create()
        {
            return View(new PARMMenuVO());
        }

        /// <summary>
        /// 新增一筆PARMMenu設定
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Create(PARMMenuVO model)
        {
            return Json(_PARMMenuBiz.Create(model) > 0 ? new JsonReturn(){ReturnCode = "1",ReturnMsg = ""} 
                                                        : new JsonReturn() { ReturnCode = "0", ReturnMsg = Lang.csfs_add_fail });
        }

        #endregion

        #region PARMMenu Edit
        /// <summary>
        /// 修改一筆PARMMenu設定
        /// </summary>
        /// <param name="id">排程ID</param>
        /// <returns></returns>
        public ActionResult Edit(int id)
        {
            PARMMenuVO model = _PARMMenuBiz.Select(id);
            ViewBag.EditMode = "Edit";
            return View("Edit", model);
        }


        /// <summary>
        /// 修改一筆PARMMenu設定
        /// </summary>
        /// <param name="model">排程</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Edit(PARMMenuVO model)
        {
            return Json(_PARMMenuBiz.Edit(model) > 0 ? new JsonReturn() { ReturnCode = "1", ReturnMsg = "" }
                                                        : new JsonReturn() { ReturnCode = "0", ReturnMsg = Lang.csfs_edit_fail });
        }
        #endregion
        
        public ActionResult Delete(int id)
        {
            return Json(_PARMMenuBiz.Delete(id) > 0 ? new JsonReturn() { ReturnCode = "1", ReturnMsg = "" }
                                                        : new JsonReturn() { ReturnCode = "0", ReturnMsg = Lang.csfs_del_fail });
        }
    }
}