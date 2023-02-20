using CTBC.CSFS.BussinessLogic;
using CTBC.CSFS.Models;
using CTBC.CSFS.Resource;
using CTBC.CSFS.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CTBC.CSFS.Pattern;

namespace CTBC.CSFS.Areas.SystemManagement.Controllers
{
    public class SendSettingRefController : AppController
    {
        SendSettingRefBiz ssrBiz = new SendSettingRefBiz();
        public ActionResult Index()
        {
            Bind();
            return View();
        }
      
        public void Bind()
        {
            List<SelectListItem> item = new List<SelectListItem>
            {
                new SelectListItem() {Text =Lang.csfs_receive_case, Value = Lang.csfs_receive_case},
                new SelectListItem() {Text =Lang.csfs_seizure_case, Value =Lang.csfs_seizure_case},
            };
            ViewBag.CaseKindList = item;
            List<SelectListItem> list = new List<SelectListItem>
            {
                new SelectListItem() {Text =Lang.csfs_165_reading, Value = Lang.csfs_165_reading},
                new SelectListItem() {Text =Lang.csfs_not_165_reading, Value = Lang.csfs_not_165_reading},
                new SelectListItem() {Text ="國稅局死亡", Value = "國稅局死亡"},
                new SelectListItem() {Text =Lang.csfs_seizure, Value = Lang.csfs_seizure},
                new SelectListItem() {Text =Lang.csfs_Pay, Value = Lang.csfs_Pay},
                new SelectListItem() {Text ="支付電子回文", Value = "支付電子回文"},
                new SelectListItem() {Text =Lang.csfs_seizureEdoc, Value = Lang.csfs_seizureEdoc}
            };
            ViewBag.CaseKind2List = list;
        }

        public JsonResult CaseKind2(string CaseKind2)
        {
            List<KeyValuePair<string, string>> items = new List<KeyValuePair<string, string>>();
            if (!string.IsNullOrEmpty(CaseKind2))
            {
                var list = ssrBiz.select(CaseKind2);
                if (list.Any())
                {
                    items.AddRange(list.Select(sub => new KeyValuePair<string, string>(sub.Subject.ToString(), sub.Description.ToString())));
                }
            }
            return Json(items);
        }

        [HttpPost]
        public ActionResult EditSSR(SendSettingRef model)
        {
            return Json(ssrBiz.EditSSR(model) > 0 ? new JsonReturn() { ReturnCode = "1", ReturnMsg = "" }
                                                        : new JsonReturn() { ReturnCode = "0", ReturnMsg = Lang.csfs_edit_fail });
        }
    }
}