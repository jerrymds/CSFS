using CTBC.CSFS.BussinessLogic;
using CTBC.CSFS.Models;
using CTBC.CSFS.Pattern;
using CTBC.CSFS.Resource;
using CTBC.FrameWork.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CTBC.CSFS.Filter;

namespace CTBC.CSFS.Areas.QueryAndExport.Controllers
{
    public class CaseClosedQueryController : AppController
    {
        // GET: QueryAndExport/CaseClosedQuery
        [RootPageFilter]
        public ActionResult Index()
        {
            Bind();
            return View();
        }

        public void Bind()
        {
            LdapEmployeeBiz empBiz = new LdapEmployeeBiz();
            ViewBag.CaseKindList = new SelectList(empBiz.GetCodeData("CASE_KIND"), "CodeDesc", "CodeDesc");
            ViewBag.CaseKind2List = new SelectList(empBiz.GetCodeData("CASE_SEIZURE"), "CodeDesc", "CodeDesc");
            ViewBag.SendKindList = new SelectList(empBiz.GetCodeData("SendKind"), "CodeDesc", "CodeDesc");
            BindAccountKindList();//*綁定報表類型
            BindDepart();//*科別
        }

        public void BindAccountKindList()
        {
            List<SelectListItem> item2 = new List<SelectListItem>
            {
                //new SelectListItem() {Text =Lang.csfs_AccountKind4+@"(結案)", Value = "0"},
                //new SelectListItem() {Text =Lang.csfs_AccountKind5+@"(結案)", Value = "1"},
                //new SelectListItem() {Text =Lang.csfs_AccountKind6+@"(結案)", Value = "2"},
                //new SelectListItem() {Text =Lang.csfs_AccountKind7+@"(結案)", Value = "3"},
                //new SelectListItem() {Text =Lang.csfs_AccountKind8+@"(結案)", Value = "4"},
                //new SelectListItem() {Text =Lang.csfs_AccountKind9+@"(結案)", Value = "5"},
                new SelectListItem() {Text =Lang.csfs_AccountKind4, Value = "6"},
                new SelectListItem() {Text =Lang.csfs_AccountKind5, Value = "7"},
                new SelectListItem() {Text =Lang.csfs_AccountKind6, Value = "8"},
                new SelectListItem() {Text =Lang.csfs_AccountKind7, Value = "9"},
                new SelectListItem() {Text =Lang.csfs_AccountKind8, Value = "10"},
                new SelectListItem() {Text =Lang.csfs_AccountKind9, Value = "11"},
                new SelectListItem() {Text =Lang.csfs_AccountKind10, Value = "12"},
                new SelectListItem() {Text =Lang.csfs_AccountKind11, Value = "13"},
            };
            ViewBag.AccountKindList = item2;
        }

        //public void BindDepart()
        //{
        //    List<SelectListItem> item2 = new List<SelectListItem>
        //    {
        //        new SelectListItem() {Text =Lang.csfs_select, Value = "0"},
        //        new SelectListItem() {Text =Lang.csfs_Depart1, Value = "1"},
        //        new SelectListItem() {Text =Lang.csfs_Depart2, Value = "2"},
        //        new SelectListItem() {Text =Lang.csfs_Depart3, Value = "3"},
        //        //20170630 緊急 RQ-2015-019666-018 派件至跨單位 宏祥 add start
        //        new SelectListItem() {Text =Lang.csfs_Depart4, Value = "4"}
        //        //20170630 緊急 RQ-2015-019666-018 派件至跨單位 宏祥 add end
        //    };
        //    ViewBag.DepartList = item2;
        //}

        //20170714 固定 RQ-2015-019666-019 派件至跨單位 宏祥 add start
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
        //20170714 固定 RQ-2015-019666-019 派件至跨單位 宏祥 add end

        /// <summary>
        /// 根據案件類型大類(扣押/外來文)來取小類
        /// </summary>
        /// <param name="caseKind"></param>
        /// <returns></returns>
        public JsonResult ChangCaseKind1(string caseKind)
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
                items.AddRange(list.Select(govUnit => new KeyValuePair<string, string>(govUnit.CodeNo.ToString(), govUnit.CodeDesc)));
            }
            return Json(items);
        }
        [HttpPost]
        public ActionResult CaseMasterExcel(CaseClosedQuery model)
        {
            model.ReceiveDateStart = UtlString.FormatDateTwStringToAd(model.ReceiveDateStart);
            model.ReceiveDateEnd = UtlString.FormatDateTwStringToAd(model.ReceiveDateEnd);
            model.SendDateStart = UtlString.FormatDateTwStringToAd(model.SendDateStart);
            model.SendDateEnd = UtlString.FormatDateTwStringToAd(model.SendDateEnd);
            //model.CloseDateStart = UtlString.FormatDateTwStringToAd(model.CloseDateStart);
            //model.CloseDateEnd = UtlString.FormatDateTwStringToAd(model.CloseDateEnd);
            model.ApproveDateStart = UtlString.FormatDateTwStringToAd(model.ApproveDateStart);
            model.ApproveDateEnd = UtlString.FormatDateTwStringToAd(model.ApproveDateEnd);

            model.SendUpDateStart = UtlString.FormatDateTwStringToAd(model.SendUpDateStart);
            model.SendUpDateEnd = UtlString.FormatDateTwStringToAd(model.SendUpDateEnd);

            CaseClosedQueryBIZ CCQ = new CaseClosedQueryBIZ();
            MemoryStream ms = new MemoryStream();
            string fileName = string.Empty;
            //20170714 固定 RQ-2015-019666-019 派件至跨單位(原產生邏輯程式寫死，組織異動必出問題，改為參數) 宏祥 add start
            LdapEmployeeBiz empBiz = new LdapEmployeeBiz();
            IList<PARMCode> listCode = empBiz.GetCodeData("Department");
            //20170714 固定 RQ-2015-019666-019 派件至跨單位(原產生邏輯程式寫死，組織異動必出問題，改為參數) 宏祥 add end
            //if (model.AccountKind == "0")
            //{
            //    ms = CCQ.ListReportExcel_NPOI(model);
            //    fileName = Lang.csfs_AccountKind4 + " " + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xls";
            //}
            //if (model.AccountKind == "1")
            //{
            //    ms = CCQ.CaseMasterListReportExcel_NPOI(model);
            //    fileName = Lang.csfs_AccountKind5 + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xls";
            //}
            //if (model.AccountKind == "2")
            //{
            //    ms = CCQ.TradeListReportExcel_NPOI(model);
            //    fileName = Lang.csfs_AccountKind6 + " " + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xls";
            //}
            //if (model.AccountKind == "3")
            //{
            //    ms = CCQ.ApproveReportExcel_NPOI(model);
            //    fileName = Lang.csfs_AccountKind7 + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xls";
            //}
            //if (model.AccountKind == "4")
            //{
            //    ms = CCQ.ReturnDetailReportExcel_NPOI(model);
            //    fileName = Lang.csfs_AccountKind8 + " " + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xls";
            //}
            //if (model.AccountKind == "5")
            //{
            //    ms = CCQ.OverDateReportExcel_NPOI(model);
            //    fileName = Lang.csfs_AccountKind9 + " " + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xls";
            //}
            if (model.AccountKind == "6")
            {
                //20170714 固定 RQ-2015-019666-019 派件至跨單位(原產生邏輯程式寫死，組織異動必出問題，改為參數) 宏祥 update start
                //ms = CCQ.ListReportExcel_NPOI1(model);
                ms = CCQ.ListReportExcel_NPOI1(model, listCode);
                //20170714 固定 RQ-2015-019666-019 派件至跨單位(原產生邏輯程式寫死，組織異動必出問題，改為參數) 宏祥 update end
                fileName = Lang.csfs_AccountKind4 + " " + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xls";
            }
            if (model.AccountKind == "7")
            {
                //20170714 固定 RQ-2015-019666-019 派件至跨單位(原產生邏輯程式寫死，組織異動必出問題，改為參數) 宏祥 update start
                //ms = CCQ.CaseMasterListReportExcel_NPOI1(model);
                ms = CCQ.CaseMasterListReportExcel_NPOI1(model, listCode);
                fileName = Lang.csfs_AccountKind5 + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xls";
                //20170714 固定 RQ-2015-019666-019 派件至跨單位(原產生邏輯程式寫死，組織異動必出問題，改為參數) 宏祥 update end
            }
            if (model.AccountKind == "8")
            {
                //20170811 RC RQ-2015-019666-020 派件至跨單位(原產生邏輯程式寫死，組織異動必出問題，改為參數) 宏祥 update start
                //ms = CCQ.TradeListReportExcel_NPOI1(model);
                ms = CCQ.TradeListReportExcel_NPOI1(model, listCode);
                //20170811 RC RQ-2015-019666-020 派件至跨單位(原產生邏輯程式寫死，組織異動必出問題，改為參數) 宏祥 update end
                fileName = Lang.csfs_AccountKind6 + " " + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xls";
            }
            if (model.AccountKind == "9")
            {
                //20170811 RC RQ-2015-019666-020 派件至跨單位(原產生邏輯程式寫死，組織異動必出問題，改為參數) 宏祥 update start
                //ms = CCQ.ApproveReportExcel_NPOI1(model);
                ms = CCQ.ApproveReportExcel_NPOI1(model, listCode);
                //20170811 RC RQ-2015-019666-020 派件至跨單位(原產生邏輯程式寫死，組織異動必出問題，改為參數) 宏祥 update end
                fileName = Lang.csfs_AccountKind7 + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xls";
            }
            if (model.AccountKind == "10")
            {
                //20170811 RC RQ-2015-019666-020 派件至跨單位(原產生邏輯程式寫死，組織異動必出問題，改為參數) 宏祥 update start
                //ms = CCQ.ReturnDetailReportExcel_NPOI1(model);
                ms = CCQ.ReturnDetailReportExcel_NPOI1(model, listCode);
                //20170811 RC RQ-2015-019666-020 派件至跨單位(原產生邏輯程式寫死，組織異動必出問題，改為參數) 宏祥 update end
                fileName = Lang.csfs_AccountKind8 + " " + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xls";
            }
            if (model.AccountKind == "11")
            {
                //20170811 RC RQ-2015-019666-020 派件至跨單位(原產生邏輯程式寫死，組織異動必出問題，改為參數) 宏祥 update start
                //ms = CCQ.OverDateReportExcel_NPOI1(model);
                ms = CCQ.OverDateReportExcel_NPOI1(model, listCode);
                //20170811 RC RQ-2015-019666-020 派件至跨單位(原產生邏輯程式寫死，組織異動必出問題，改為參數) 宏祥 update end
                fileName = Lang.csfs_AccountKind9 + " " + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xls";
            }
            if (model.AccountKind == "12")
            {
                model.SendKind = "電子發文";
                //20170811 RC RQ-2015-019666-020 派件至跨單位(原產生邏輯程式寫死，組織異動必出問題，改為參數) 宏祥 update start
                //ms = CCQ.ListReportExcel_Send1(model);
                ms = CCQ.ListReportExcel_Send1(model, listCode);
                //20170811 RC RQ-2015-019666-020 派件至跨單位(原產生邏輯程式寫死，組織異動必出問題，改為參數) 宏祥 update end
                fileName = Lang.csfs_AccountKind10 + " " + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xls";
            }
            if (model.AccountKind == "13")
            {
                model.SendKind = "電子發文";
                //20170811 RC RQ-2015-019666-020 派件至跨單位(原產生邏輯程式寫死，組織異動必出問題，改為參數) 宏祥 update start
                //ms = CCQ.ListReportExcel_Send2(model);
                ms = CCQ.ListReportExcel_Send2(model, listCode);
                //20170811 RC RQ-2015-019666-020 派件至跨單位(原產生邏輯程式寫死，組織異動必出問題，改為參數) 宏祥 update end
                fileName = Lang.csfs_AccountKind11 + " " + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xls";
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