using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CTBC.CSFS.Models;
using CTBC.CSFS.Resource;
using CTBC.CSFS.BussinessLogic;
using CTBC.CSFS.ViewModels;
using CTBC.CSFS.Pattern;

namespace CTBC.CSFS.Areas.SystemManagement.Controllers
{
    public class GovAddressController : AppController
    {
        PARMCodeBIZ parm = new PARMCodeBIZ();
        GovAddressBIZ govAddr = new GovAddressBIZ();
        //GovAddressBIZ govAddr = new GovAddressBIZ();
        //
        // GET: /SystemManagement/GovAddress/
        public ActionResult Index()
        {
            BindGovKindList();
            return View();
        }

        #region 新增主管機關
        public ActionResult Create()
        {
            BindGovKindList();//來文機關類型
            BindIsEnableList();//啟用狀態類型
            return View();
        }

        [HttpPost]
        public ActionResult Create(GovAddress model)
        {
            model.CreatedDate = DateTime.Now;
            model.CreatedUser = Session["UserAccount"].ToString();
            return Json(govAddr.Create(model) > 0 ? new JsonReturn() { ReturnCode = "1", ReturnMsg = "" }
                                                      : new JsonReturn() { ReturnCode = "0", ReturnMsg = Lang.csfs_add_fail });
        }
        #endregion

        #region 查詢主管機關結果集
        /// <summary>
        /// 結果列表
        /// </summary>
        /// <param name="AToH"></param>
        /// <param name="pageNum"></param>
        /// <param name="strSortExpression"></param>
        /// <param name="strSortDirection"></param>
        /// <returns></returns>
        public ActionResult _QueryResult(GovAddress model, int pageNum = 1, string strSortExpression = "GovAddrId", string strSortDirection = "asc")
        {
            return PartialView("_QueryResult", SearchList(model, pageNum, strSortExpression, strSortDirection));
        }

        /// <summary>
        /// 實際查詢動作
        /// </summary>
        /// <param name="AToH"></param>
        /// <param name="pageNum"></param>
        /// <param name="strSortExpression"></param>
        /// <param name="strSortDirection"></param>
        /// <returns></returns>
        public GovAddressViewModel SearchList(GovAddress model, int pageNum = 1, string strSortExpression = "GovAddrId", string strSortDirection = "asc")
        {
            IList<GovAddress> result = govAddr.GetQueryList(model, pageNum, strSortExpression, strSortDirection);

            var viewModel = new GovAddressViewModel()
            {
                GovAddress = model,
                GovAddressList = result,
            };

            viewModel.GovAddress.PageSize = govAddr.PageSize;
            viewModel.GovAddress.CurrentPage = govAddr.PageIndex;
            viewModel.GovAddress.TotalItemCount = govAddr.DataRecords;
            viewModel.GovAddress.SortExpression = strSortExpression;
            viewModel.GovAddress.SortDirection = strSortDirection;

            return viewModel;
        }
        #endregion

        #region 編輯主管機關
        public ActionResult Edit(int govId)
        {
            BindIsEnableList();
            GovAddress model = govAddr.getGovAddressByGovId(govId);
            ViewBag.GovKindList1 = new SelectList(parm.SelectGovUnitByGOV_KIND(Lang.csfs_gov_type), "CodeDesc", "CodeDesc", model.GovKind);   //* 類別-小類-扣押
            return PartialView("Edit", model);
        }

        [HttpPost]
        public ActionResult Edit(GovAddress model)
        {
            model.ModifiedDate = DateTime.Now;
            model.ModifiedUser = Session["UserAccount"].ToString();

            return Json(govAddr.Update(model) > 0 ? new JsonReturn() { ReturnCode = "1", ReturnMsg = "" }
                                                       : new JsonReturn() { ReturnCode = "0", ReturnMsg = Lang.csfs_edit_fail });
        }
        #endregion

        /// <summary>
        /// 綁定啟用狀態
        /// </summary>
        public void BindIsEnableList()
        {
            List<SelectListItem> item2 = new List<SelectListItem>
            {
                new SelectListItem() {Text = Lang.csfs_enable, Value = "true"},
                new SelectListItem() {Text = Lang.csfs_disable, Value = "false"}
            };
            ViewBag.IsEnableList = item2;
        }

        /// <summary>
        /// 綁定來文機關
        /// </summary>
        public void BindGovKindList()
        {
            IEnumerable<PARMCode> list = parm.SelectGovUnitByGOV_KIND(Lang.csfs_gov_type);
            List<SelectListItem> listitem = new List<SelectListItem>();
            foreach (var item in list)
            {
                listitem.Add(new SelectListItem() { Text = item.CodeDesc, Value = item.CodeDesc });
            }
            ViewBag.GovKindList = listitem;
        }
    }
}