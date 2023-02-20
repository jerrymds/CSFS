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
    public class CheckNoSettingController : AppController
    {
        CheckNoSettingBIZ cnsBiz;

        public CheckNoSettingController()
        {
            cnsBiz = new CheckNoSettingBIZ(this);
        }
        public ActionResult Index()
        {
            return View();
        }

        //public void BindUseStatus()
        //{
        //    List<SelectListItem> list = new List<SelectListItem>
        //    {
        //        new SelectListItem() {Text =Lang.csfs_UseStatus1, Value = Lang.csfs_UseStatus1},
        //        new SelectListItem() {Text =Lang.csfs_UseStatus2, Value = Lang.csfs_UseStatus2},
        //    };
        //    ViewBag.UseStatusList = list;
        //}

        public ActionResult _QueryResult(CheckNoSetting cns, int pageNum = 1, string strSortExpression = "CheckIntervalID", string strSortDirection = "asc")
        {
            return PartialView("_QueryResult", SearchList(cns, pageNum, strSortExpression, strSortDirection));
        }

        public CheckNoSettingViewModel SearchList(CheckNoSetting cns, int pageNum = 1, string strSortExpression = "CheckIntervalID", string strSortDirection = "asc")
        {

            IList<CheckNoSetting> result = cnsBiz.GetQueryList(cns, pageNum, strSortExpression, strSortDirection);

            var viewModel = new CheckNoSettingViewModel()
            {
                CheckNoSetting = cns,
                CheckNoSettingList = result,
            };

            //分頁相關設定
            viewModel.CheckNoSetting.PageSize = cnsBiz.PageSize;
            viewModel.CheckNoSetting.CurrentPage = cnsBiz.PageIndex;
            viewModel.CheckNoSetting.TotalItemCount = cnsBiz.DataRecords;
            viewModel.CheckNoSetting.SortExpression = strSortExpression;
            viewModel.CheckNoSetting.SortDirection = strSortDirection;

            viewModel.CheckNoSetting.CheckIntervalID = cns.CheckIntervalID;
            return viewModel;
        }

        public ActionResult Create()
        {
            //BindUseStatus();
            return View(new CheckNoSetting());
        }

        [HttpPost]
        public ActionResult Create(CheckNoSetting model)
        {
            model.UseStatus = Lang.csfs_UseStatus1;
            return Json(cnsBiz.Create(model) > 0 ? new JsonReturn() { ReturnCode = "1", ReturnMsg = "" }
                                                        : new JsonReturn() { ReturnCode = "0", ReturnMsg = Lang.csfs_add_fail });
        }
        public ActionResult Edit(Int64 CheckIntervalID)
        {
            //BindUseStatus();
            CheckNoSetting model = cnsBiz.Select(CheckIntervalID);
            ViewBag.EditMode = "Edit";
            return View("Edit", model);
        }

        [HttpPost]
        public ActionResult Edit(Int64 checkIntervalId, Int64 checkNo)
        {
            //CasePayeeSetting md = new CasePayeeSetting();
            //md.CheckNo = checkNo.ToString();
            //// 要刪除傳入支票號碼數據
            //// ???/
            ////
            CheckNoSetting model = new CheckNoSetting();
            model.CheckNo = checkNo;
            model.Kind = Lang.csfs_Pay;
            model.IsUsed = 0;
            model.IsPreserve = 0;
            model.CheckIntervalID = checkIntervalId;
            return Json(cnsBiz.Setting(model) ? new JsonReturn() { ReturnCode = "1", ReturnMsg = "" }
                                                        : new JsonReturn() { ReturnCode = "0", ReturnMsg = Lang.csfs_setting_fail });
        }
        //public ActionResult Edit(CheckNoSetting model)
        //{
        //    return Json(cnsBiz.Edit(model) > 0 ? new JsonReturn() { ReturnCode = "1", ReturnMsg = "" }
        //                                                : new JsonReturn() { ReturnCode = "0", ReturnMsg = Lang.csfs_edit_fail });
        //}

        public ActionResult Delete(Int64 CheckIntervalID)
        {
            return Json(cnsBiz.Delete(CheckIntervalID) > 0 ? new JsonReturn() { ReturnCode = "1", ReturnMsg = "" }
                                                        : new JsonReturn() { ReturnCode = "0", ReturnMsg = Lang.csfs_del_fail });
        }

        public ActionResult Active(Int64 CheckIntervalID)
        {
            CheckNoSetting model = cnsBiz.Select(CheckIntervalID);
            model.UseStatus = Lang.csfs_UseStatus2;
            model.Kind = Lang.csfs_Pay;
            model.IsUsed = 0;
            return Json(cnsBiz.Active(model) > 0 ? new JsonReturn() { ReturnCode = "1", ReturnMsg = "" }
                                                        : new JsonReturn() { ReturnCode = "0", ReturnMsg = Lang.csfs_enable + Lang.csfs_fail });
        }

        public ActionResult Detail(Int64 CheckIntervalID)
        {
            return View(new CheckNoSetting() { CheckIntervalID = CheckIntervalID });
        }

        public ActionResult _QueryDetail(Int64 checkIntervalId, Int32? checkNoS, Int32? checkNoE, int pageNum = 1, string strSortExpression = "CheckNo", string strSortDirection = "asc")
        {
            return PartialView("_QueryDetail", SearchDetail(checkIntervalId, checkNoS, checkNoE, pageNum, strSortExpression, strSortDirection));
        }

        public CheckNoSettingViewModel SearchDetail(Int64 checkIntervalId, Int32? checkNoS, Int32? checkNoE, int pageNum = 1, string strSortExpression = "CheckNo", string strSortDirection = "asc")
        {

            IList<CheckNoSetting> result = cnsBiz.GetDetailList(checkIntervalId, checkNoS, checkNoE,pageNum, strSortExpression, strSortDirection);

            var viewModel = new CheckNoSettingViewModel()
            {
                CheckNoSetting = new CheckNoSetting() { CheckIntervalID = checkIntervalId, CheckNoS = checkNoS ?? 0, CheckNoE= checkNoE ?? 0 },
                CheckNoSettingList = result,
            };

            //分頁相關設定
            viewModel.CheckNoSetting.PageSize = cnsBiz.PageSize;
            viewModel.CheckNoSetting.CurrentPage = cnsBiz.PageIndex;
            viewModel.CheckNoSetting.TotalItemCount = cnsBiz.DataRecords;
            viewModel.CheckNoSetting.SortExpression = strSortExpression;
            viewModel.CheckNoSetting.SortDirection = strSortDirection;

            viewModel.CheckNoSetting.CheckIntervalID = checkIntervalId;
            return viewModel;
        }

        public ActionResult Pay(Int64 checkIntervalId,Int64 checkNo)
        {
            CheckNoSetting model = new CheckNoSetting();
            model.CheckNo = checkNo;
            model.Kind = Lang.csfs_Pay;
            model.IsUsed = 0;
            model.CheckIntervalID = checkIntervalId;
            return Json(cnsBiz.Setting(model)  ? new JsonReturn() { ReturnCode = "1", ReturnMsg = "" }
                                                        : new JsonReturn() { ReturnCode = "0", ReturnMsg = Lang.csfs_setting_fail });
        }

        public ActionResult Invalid(Int64 checkIntervalId, Int64 checkNo)
        {
            CheckNoSetting model = new CheckNoSetting();
            model.CheckNo = checkNo;
            model.Kind = Lang.csfs_invalid;
            model.IsUsed = 1;
            model.CheckIntervalID = checkIntervalId;
            return Json(cnsBiz.Setting(model)  ? new JsonReturn() { ReturnCode = "1", ReturnMsg = "" }
                                                        : new JsonReturn() { ReturnCode = "0", ReturnMsg = Lang.csfs_setting_fail });
        }

        public ActionResult Others(Int64 checkIntervalId, Int64 checkNo)
        {
            CheckNoSetting model = new CheckNoSetting();
            model.CheckNo = checkNo;
            model.Kind = Lang.csfs_others;
            model.IsUsed = 1;
            model.CheckIntervalID = checkIntervalId;
            return Json(cnsBiz.Setting(model)  ? new JsonReturn() { ReturnCode = "1", ReturnMsg = "" }
                                                        : new JsonReturn() { ReturnCode = "0", ReturnMsg = Lang.csfs_setting_fail });
        }
        public ActionResult Modify(Int64 checkIntervalId, Int64 checkNo)
        {
            CheckNoSetting model = new CheckNoSetting();
            model.CheckNo = checkNo;
            model.Kind = Lang.csfs_Pay;
            model.IsUsed = 0;
            model.IsPreserve = 0;
            model.CheckIntervalID = checkIntervalId;
            return Json(cnsBiz.Setting(model) ? new JsonReturn() { ReturnCode = "1", ReturnMsg = "" }
                                                        : new JsonReturn() { ReturnCode = "0", ReturnMsg = Lang.csfs_setting_fail });
        }
	}
}