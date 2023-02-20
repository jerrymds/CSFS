using CTBC.CSFS.BussinessLogic;
using CTBC.CSFS.Models;
using CTBC.CSFS.Pattern;
using CTBC.CSFS.Resource;
using CTBC.CSFS.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CTBC.CSFS.Areas.SystemManagement.Controllers
{
    public class SendNoSettingController : AppController
    {
        SendNoSettingBIZ snsBiz;

        public SendNoSettingController()
        {
            snsBiz = new SendNoSettingBIZ(this);
        }
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult _QueryResult(SendNoSetting sns, int pageNum = 1, string strSortExpression = "SendNoId", string strSortDirection = "asc")
        {
            return PartialView("_QueryResult", SearchList(sns, pageNum, strSortExpression, strSortDirection));
        }

        public SendNoSettingViewModel SearchList(SendNoSetting sns, int pageNum = 1, string strSortExpression = "SendNoId", string strSortDirection = "asc")
        {

            IList<SendNoSetting> result = snsBiz.GetQueryList(sns, pageNum, strSortExpression, strSortDirection);

            var viewModel = new SendNoSettingViewModel()
            {
                SendNoSetting = sns,
                SendNoSettingList = result,
            };

            //分頁相關設定
            viewModel.SendNoSetting.PageSize = snsBiz.PageSize;
            viewModel.SendNoSetting.CurrentPage = snsBiz.PageIndex;
            viewModel.SendNoSetting.TotalItemCount = snsBiz.DataRecords;
            viewModel.SendNoSetting.SortExpression = strSortExpression;
            viewModel.SendNoSetting.SortDirection = strSortDirection;

            viewModel.SendNoSetting.SendNoYear = sns.SendNoYear;
            return viewModel;
        }

        public ActionResult Create()
        {
            return View(new SendNoSetting());
        }

        [HttpPost]
        public ActionResult Create(SendNoSetting model)
        {
            return Json(snsBiz.Create(model) > 0 ? new JsonReturn() { ReturnCode = "1", ReturnMsg = "" }
                                                        : new JsonReturn() { ReturnCode = "0", ReturnMsg = Lang.csfs_add_fail });
        }
        public ActionResult Edit(int SendNoId)
        {
            SendNoSetting model = snsBiz.Select(SendNoId);
            ViewBag.EditMode = "Edit";
            return View("Edit", model);
        }

        [HttpPost]
        public ActionResult Edit(SendNoSetting model)
        {
            return Json(snsBiz.Edit(model) > 0 ? new JsonReturn() { ReturnCode = "1", ReturnMsg = "" }
                                                        : new JsonReturn() { ReturnCode = "0", ReturnMsg = Lang.csfs_edit_fail });
        }

        public ActionResult Delete(int SendNoId)
        {
            return Json(snsBiz.Delete(SendNoId) > 0 ? new JsonReturn() { ReturnCode = "1", ReturnMsg = "" }
                                                        : new JsonReturn() { ReturnCode = "0", ReturnMsg = Lang.csfs_del_fail });
        }
	}
}