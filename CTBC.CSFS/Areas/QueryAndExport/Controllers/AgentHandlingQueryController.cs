using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CTBC.CSFS.Pattern;
using CTBC.CSFS.BussinessLogic;
using System.IO;
using CTBC.CSFS.Filter;
using CTBC.CSFS.Models;
using CTBC.FrameWork.Util;
using CTBC.CSFS.Resource;

namespace CTBC.CSFS.Areas.QueryAndExport.Controllers
{
    public class AgentHandlingQueryController : AppController
    {
        CaseMasterBIZ casemaster = new CaseMasterBIZ();

        // GET: QueryAndExport/AgentHandlingQuery
        [RootPageFilter]
        public ActionResult Index()
        {
            InitDropdownListOptions();
            return View();
        }

        /// <summary>
        /// 綁定小類
        /// </summary>
        /// <param name="govKind"></param>
        /// <returns></returns>
        public JsonResult ChangCaseKind(string caseKind)
        {
            PARMCodeBIZ parm = new PARMCodeBIZ();
            List<KeyValuePair<string, string>> items = new List<KeyValuePair<string, string>>();
            if (string.IsNullOrEmpty(caseKind)) return Json(items);
            //* 取得大類的CodeNo,以知道小類的CodeType
            var itemKind = parm.GetCodeData("CASE_KIND").FirstOrDefault(a => a.CodeDesc == caseKind);
            if (itemKind == null) return Json(items);

            var list = parm.GetCodeData(itemKind.CodeNo);
            if (list.Any())
            {
                items.AddRange(list.Select(govUnit => new KeyValuePair<string, string>(govUnit.CodeDesc.ToString(), govUnit.CodeDesc)));
            }
            return Json(items);
        }

        /// <summary>
        /// 綁定下拉菜單
        /// </summary>
        public void InitDropdownListOptions()
        {
            PARMCodeBIZ parm = new PARMCodeBIZ();
            ViewBag.CaseKindList = new SelectList(parm.GetCodeData("CASE_KIND"), "CodeDesc", "CodeDesc");       //* 類別-大類
            ViewBag.CaseKind2List = new SelectList(parm.GetCodeData("CASE_SEIZURE"), "CodeDesc", "CodeDesc");   //* 類別-小類
            BindAccountKindList();//*綁定報表類型
            BindDepart();//*科別
        }

        /// <summary>
        /// 綁定報表類型
        /// </summary>
        public void BindAccountKindList()
        {
            List<SelectListItem> item2 = new List<SelectListItem>
            {
                new SelectListItem() {Text =Lang.csfs_AccountKind1, Value = "0"},
                new SelectListItem() {Text =Lang.csfs_AccountKind2, Value = "1"},
                new SelectListItem() {Text =Lang.csfs_AccountKind3, Value = "2"}
            };
            ViewBag.AccountKindList = item2;
        }

        //public void BindDepart()
        //{
        //    List<SelectListItem> item2 = new List<SelectListItem>
        //    {
        //         new SelectListItem() {Text =Lang.csfs_select, Value = "0"},
        //        new SelectListItem() {Text =Lang.csfs_Depart1, Value = "1"},
        //        new SelectListItem() {Text =Lang.csfs_Depart2, Value = "2"},
        //        new SelectListItem() {Text =Lang.csfs_Depart3, Value = "3"}
        //    };
        //    ViewBag.DepartList = item2;
        //}

        //20170811 RC RQ-2015-019666-020 派件至跨單位 宏祥 add start
        public void BindDepart()
        {
           LdapEmployeeBiz empBiz = new LdapEmployeeBiz();
           IList<PARMCode> listCode = empBiz.GetCodeData("Department");
           List<SelectListItem> item2 = new List<SelectListItem>();
           item2.Add(new SelectListItem { Text = Lang.csfs_select, Value = "0" });
           if (listCode != null && listCode.Any())
           {
              listCode = listCode.OrderBy(m => m.SortOrder).ToList();
              foreach (PARMCode code in listCode)
              {
                 item2.Add(new SelectListItem { Text = code.CodeDesc, Value = code.CodeNo });
              }
           }
           ViewBag.DepartList = item2;
        }
        //20170811 RC RQ-2015-019666-020 派件至跨單位 宏祥 add end

        public ActionResult CaseMasterExcel(CaseMaster model)
        {
            model.CreatedDateStart = UtlString.FormatDateTwStringToAd(model.CreatedDateStart);
            model.CreatedDateEnd = UtlString.FormatDateTwStringToAd(model.CreatedDateEnd);
            model.CloseDateStart = UtlString.FormatDateTwStringToAd(model.CloseDateStart);
            model.CloseDateEnd = UtlString.FormatDateTwStringToAd(model.CloseDateEnd);

            MemoryStream ms = new MemoryStream();
            string fileName = string.Empty;

            //20170811 RC RQ-2015-019666-020 派件至跨單位(原產生邏輯程式寫死，組織異動必出問題，改為參數) 宏祥 add start
            LdapEmployeeBiz empBiz = new LdapEmployeeBiz();
            IList<PARMCode> listCode = empBiz.GetCodeData("Department");
            //20170811 RC RQ-2015-019666-020 派件至跨單位(原產生邏輯程式寫死，組織異動必出問題，改為參數) 宏祥 add end

            if (model.AccountKind == "0")//各科
            {
                //20170811 RC RQ-2015-019666-020 派件至跨單位(原產生邏輯程式寫死，組織異動必出問題，改為參數) 宏祥 add start
                //ms = casemaster.CaseMasterListForDepartReportExcel_NPOI(model);
                ms = casemaster.CaseMasterListForDepartReportExcel_NPOI(model, listCode);
                //20170811 RC RQ-2015-019666-020 派件至跨單位(原產生邏輯程式寫死，組織異動必出問題，改為參數) 宏祥 add end
                fileName = Lang.csfs_AccountKind1+"_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xls";
            }
            if (model.AccountKind == "1")//經辦
            {
                //20170811 RC RQ-2015-019666-020 派件至跨單位(原產生邏輯程式寫死，組織異動必出問題，改為參數) 宏祥 add start
                //ms = casemaster.CaseMasterListReportExcel_NPOI(model);
                ms = casemaster.CaseMasterListReportExcel_NPOI(model, listCode);
                //20170811 RC RQ-2015-019666-020 派件至跨單位(原產生邏輯程式寫死，組織異動必出問題，改為參數) 宏祥 add end
                fileName = Lang.csfs_AccountKind2+"_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xls";
            }
            if (model.AccountKind == "2")//收發退件
            {
                //20170811 RC RQ-2015-019666-020 派件至跨單位(原產生邏輯程式寫死，組織異動必出問題，改為參數) 宏祥 add start
                //ms = casemaster.CaseMasterListDetailExcel_NPOI(model);
                ms = casemaster.CaseMasterListDetailExcel_NPOI(model, listCode);
                //20170811 RC RQ-2015-019666-020 派件至跨單位(原產生邏輯程式寫死，組織異動必出問題，改為參數) 宏祥 add end
                fileName = Lang.csfs_AccountKind3+"_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xls";
            }
            if (ms != null && ms.Length > 0)
            {
                Response.ClearContent();
                Response.ClearHeaders();
            }
            else
            {
                ms = new MemoryStream();
            }
            return File(ms.ToArray(), "application/vnd.ms-excel", fileName);
        }
    }
}