using CTBC.CSFS.Pattern;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CTBC.CSFS.BussinessLogic;
using CTBC.CSFS.Models;
using System.IO;
using CTBC.FrameWork.Util;
using CTBC.CSFS.ViewModels;
using Microsoft.Reporting.WebForms;
using System.Data;
using CTBC.CSFS.Filter;
using CTBC.CSFS.Resource;
using iTextSharp.text.pdf;
using iTextSharp.text;


namespace CTBC.CSFS.Areas.QueryAndExport.Controllers
{
    public class CheckQueryAndPrintController : AppController
    {
        /// <summary>
        /// 進入查詢畫面
        /// </summary>
        /// <param name="isBack"></param>
        /// <returns></returns>
        [RootPageFilter]
        public ActionResult Index(string isBack)
        {
            CheckQueryAndPrint model = new CheckQueryAndPrint();
            if (isBack == "1")
            {
                //*執行cookie里的內容
                HttpCookie getQueryCookie = Request.Cookies.Get("QueryCookie");
                if (getQueryCookie != null)
                {
                    if (getQueryCookie.Values["CheckDate"] != null) model.CheckDate = UtlString.FormatDateTw(getQueryCookie.Values["CheckDate"]);
                    if (getQueryCookie.Values["Type"] != null) model.Type = getQueryCookie.Values["Type"];
                    //Add by zhangwei 20180315 start
                    if (getQueryCookie.Values["CheckNoStart"] != null) model.CheckNoStart = getQueryCookie.Values["CheckNoStart"];
                    if (getQueryCookie.Values["CheckNoEnd"] != null) model.CheckNoEnd = getQueryCookie.Values["CheckNoEnd"];
                    if (getQueryCookie.Values["AmtConsistentType"] != null) model.AmtConsistentType = getQueryCookie.Values["AmtConsistentType"];
                    //Add by zhangwei 20180315 end
                    ViewBag.CurrentPage = getQueryCookie.Values["CurrentPage"];
                    ViewBag.isQuery = "1";
                }
            }
            else
            {
                model.CheckDate = UtlString.FormatDateTw(UtlString.GetWednesday().ToString("yyyy/MM/dd"));
            }

            BindType();
            //Add by zhangwei 20180315 start
            BindAmtConsistentList();
            //Add by zhangwei 20180315 end
            return View(model);
        }
        public void BindType()
        {
            List<SelectListItem> list = new List<SelectListItem>
            {
                new SelectListItem() {Text =Lang.csfs_check_print, Value = "1"},
                new SelectListItem() {Text =Lang.csfs_pay_detail, Value = "2"},
                new SelectListItem() {Text ="發文-正本", Value = "3"},
                new SelectListItem() {Text ="發文-副本", Value = "4"},
                new SelectListItem() {Text ="發文-收據", Value = "5"},
            };
            ViewBag.TypeList = list;
        }
        public void BindAmtConsistentList()
        {
            List<SelectListItem> list = new List<SelectListItem>
            {
                new SelectListItem() {Text ="-請選擇-", Value = "0"},
                new SelectListItem() {Text ="相符", Value = "1"},
                new SelectListItem() {Text ="不相符", Value = "2"},
            };
            ViewBag.AmtConsistentList = list;
        }
        [HttpPost]
        public ActionResult Query(CheckQueryAndPrint model, int pageNum = 1, string strSortExpression = "CaseId", string strSortDirection = "asc")
        {
            CheckQueryAndPrintBIZ CKP = new CheckQueryAndPrintBIZ();
            string UserId = LogonUser.Account;
            if (!string.IsNullOrEmpty(model.CheckDate))
            {
                model.CheckDate = UtlString.FormatDateTwStringToAd(model.CheckDate);
            }
            IList<CheckQueryAndPrint> list = CKP.GetData(model, pageNum, strSortExpression, strSortDirection, UserId);
            if (list != null && list.Any() && model.Type != "1")
            {
                //* 明細.同統編的要清清除掉帳戶合計
                for (int i = 1; i < list.Count; i++)
                {
                    if (list[i].CaseId == list[i - 1].CaseId && list[i].CustName == list[i - 1].CustName)
                    {
                        list[i - 1].sum1 = "";
                    }
                }
            }


            var CPvm = new CheckQueryAndPrintViewModel()
            {
                CheckQueryAndPrint = model,
                CheckQueryAndPrintlist = list,
            };
            CPvm.CheckQueryAndPrint.PageSize = CKP.PageSize;
            CPvm.CheckQueryAndPrint.CurrentPage = CKP.PageIndex;
            CPvm.CheckQueryAndPrint.TotalItemCount = CKP.DataRecords;
            CPvm.CheckQueryAndPrint.SortExpression = strSortExpression;
            CPvm.CheckQueryAndPrint.SortDirection = strSortDirection;
            //Add by zhangwei 20180315 start
            CPvm.CheckQueryAndPrint.TotalPayment = list != null && list.Any() ? list[0].TotalPayment:"0";
            CPvm.CheckQueryAndPrint.TotalSeizureAmount = list != null && list.Any() ? list[0].TotalSeizureAmount:"0";
            CPvm.CheckQueryAndPrint.TotalFee = list != null && list.Any() ? list[0].TotalFee:"0";
            if (model.Type == "2")
            {
                CPvm.CheckQueryAndPrint.TotalID = list != null && list.Any() ? list[0].TotalID:"0";
            }
            //Add by zhangwei 20180315 end
            HttpCookie AToHCookie = new HttpCookie("QueryCookie");
            AToHCookie.Values.Add("CheckDate", model.CheckDate);
            AToHCookie.Values.Add("Type", model.Type);
            AToHCookie.Values.Add("CurrentPage", pageNum.ToString());
            //Add by zhangwei 20180315 start
            AToHCookie.Values.Add("CheckNoStart", model.CheckNoStart);
            AToHCookie.Values.Add("CheckNoEnd", model.CheckNoEnd);
            AToHCookie.Values.Add("AmtConsistentType", model.AmtConsistentType);
            //Add by zhangwei 20180315 end
            Response.Cookies.Add(AToHCookie);

            CPvm.CheckQueryAndPrint.CheckDate = model.CheckDate;
            CPvm.CheckQueryAndPrint.Type = model.Type;
            return PartialView("Query", CPvm);
        }

        List<ReportDataSource> subDataSource = new List<ReportDataSource>();
        public ActionResult Report(string type, string payeeIdList)
        {
            if (type == "3" || type == "4" || type == "5") //adam 2021/6/8
            {
                CheckQueryAndPrintBIZ CKP = new CheckQueryAndPrintBIZ();
                List<ReportParameter> listParm = new List<ReportParameter>();
                List<string> aryPayeeIdList = payeeIdList.Split(',').ToList();
                DataTable dtPayeeSetting = CKP.GetQueryAndPrintByCaseIdList(aryPayeeIdList);
                string CaseIdList = "";
                foreach (DataRow row in dtPayeeSetting.Rows)
                {

                    CaseIdList = CaseIdList + row["CaseId"].ToString() + ",";
                }
                // 一次全印不能只取一筆
                //if (dtPayeeSetting.Rows.Count > 0)
                //{
                //    if (CaseIdList.Substring(CaseIdList.Length - 1, 1) == ",")
                //    {
                //        CaseIdList = CaseIdList.Substring(0, CaseIdList.Length - 1);
                //    }
                //}
                // ReportPay(CaseIdList, null, "ReportPay" + DateTime.Now.ToLongDateString());
                //直接列印 20200528
                CaseMasterBIZ masterBiz = new CaseMasterBIZ();
                //List<ReportParameter> listParm = new List<ReportParameter>();

                //* CTBC的地址.電話.傳真
                PARMCode codeItem = masterBiz.GetCodeData("REPORT_SETTING", "Address").FirstOrDefault();
                listParm.Add(new ReportParameter("CtbcAddr", codeItem == null ? "" : codeItem.CodeDesc));
                //* 20150618 電話號碼改為當前登入者的電話
                //codeItem = masterBiz.GetCodeData("REPORT_SETTING", "Tel").FirstOrDefault();
                LdapEmployeeBiz empBiz = new LdapEmployeeBiz();
                LDAPEmployee empNow = empBiz.GetLdapEmployeeByEmpId(LogonUser.Account);
                string tel = empNow != null && !string.IsNullOrEmpty(empNow.TelNo) ? empNow.TelNo : " ";
                tel = tel + (empNow != null && !string.IsNullOrEmpty(empNow.TelExt) ? " 分機 " + empNow.TelExt : "");
                listParm.Add(new ReportParameter("CtbcTel", tel));
                string ctbcTel = codeItem == null ? "" : codeItem.CodeDesc;
                codeItem = masterBiz.GetCodeData("REPORT_SETTING", "Fax").FirstOrDefault();
                listParm.Add(new ReportParameter("CtbcFax", codeItem == null ? "" : codeItem.CodeDesc));
                codeItem = masterBiz.GetCodeData("REPORT_SETTING", "ButtomLine").FirstOrDefault();
                listParm.Add(new ReportParameter("CtbcButtomLine", codeItem == null ? "" : codeItem.CodeDesc));
                codeItem = masterBiz.GetCodeData("REPORT_SETTING", "ButtomLine2").FirstOrDefault();
                listParm.Add(new ReportParameter("CtbcButtomLine2", codeItem == null ? "" : codeItem.CodeDesc));

                //if (dtPayeeSetting.Rows.Count > 1)
                //{
                //    if (CaseIdList.Substring(CaseIdList.Length - 1, 1) == ",")
                //    {
                //        CaseIdList = CaseIdList.Substring(0, CaseIdList.Length - 1);
                //    }
                //}
                List<string> aryCaseIdList = CaseIdList.TrimEnd(',').Split(',').ToList();
                //* master
                DataTable dtMaster = masterBiz.GetCaseMasterByCaseIdList(aryCaseIdList);
                //* 發文設定
                DataTable dtSendSetting = null;
                string Con = null;
                string strOther = "";
                string strGovNo = "";
                if (dtMaster.Rows[0]["GovNo"].ToString().Length > 3)
                {
                    int at = dtMaster.Rows[0]["GovNo"].ToString().IndexOf("號");
                    if (at < 0)
                    {
                        strGovNo = dtMaster.Rows[0]["GovNo"].ToString().Substring(0, dtMaster.Rows[0]["GovNo"].ToString().Length);
                        strOther = strGovNo.Substring(strGovNo.Length - 1, 1);
                    }
                    else
                    {
                        strGovNo = dtMaster.Rows[0]["GovNo"].ToString().Substring(0, at);
                        strOther = strGovNo.Substring(strGovNo.Length - 1, 1);
                    }
                }

                List<string> CaseIDSequence = new List<string>(); // CaseID 的輸出順序

                if (Con == "Pay")
                {
                    dtSendSetting = new CaseSendSettingBIZ().GetSeizurePayByCaseIdList(aryCaseIdList);
                }
                else
                {
                    if (type == "3" || type == "4" || type == "5") // adam 2021/6/8
                    {

                        if (type == "3")
                        {
                            //dtSendSetting = new CaseSendSettingBIZ().GetSendSettingByCaseIdListWithType("1", aryCaseIdList);
                            dtSendSetting = new CaseSendSettingBIZ().GetSendSettingByCaseIdListWithType("1", aryCaseIdList, aryPayeeIdList, ref CaseIDSequence);
                        }
                        if (type == "4")
                        {
                            dtSendSetting = new CaseSendSettingBIZ().GetSendSettingByCaseIdListWithType("2", aryCaseIdList, aryPayeeIdList, ref CaseIDSequence);
                        }
                        if (type == "5") // adam 2021/6/8
                        {
                            dtSendSetting = new CaseSendSettingBIZ().GetSendSettingByCaseIdListWithNoType( aryCaseIdList, aryPayeeIdList, ref CaseIDSequence);
                        }

                        //dtMaster.Columns.Add()



                    }
                    else
                    {
                        dtSendSetting = new CaseSendSettingBIZ().GetSendSettingByCaseIdList(aryCaseIdList);
                    }
                }
                //adam 2015-09-03 主管扣押並支付 支付時 不能出現扣押
                if (Con == "Director" || Con == null)
                {
                    for (int i = 0; i < dtSendSetting.Rows.Count; i++)
                    {
                        DataRow[] old = dtMaster.Select("CaseId='" + dtSendSetting.Rows[i]["CaseId"].ToString() + "' and CaseKind2 ='扣押並支付' and AfterSeizureApproved = 1 ");
                        if (old.Count() != 0)
                        {
                            if (dtSendSetting.Rows[i]["Template"].ToString().Trim() == "扣押")
                            {
                                dtSendSetting.Rows[i].Delete();
                            }
                        }
                    }
                    dtSendSetting.AcceptChanges();
                }
                ////adam 2015-09-03
                ////* 20150723 新增空白判斷
                //if (dtSendSetting == null || dtSendSetting.Rows.Count <= 0)
                //{
                //    return Content("<script>alert('" + Lang.csfs_no_data + "');</script>");
                //}
                ////* 20150723 

                //* 外來文帳務明細
                DataTable dtExternal = new CaseAccountBiz().GetCaseAccountExternalByCaseIdList(aryCaseIdList);
                //* 發票
                DataTable dtReceipt = new CaseAccountBiz().GetCaseReceiptByCaseIdList_2(aryCaseIdList);
                for (int i = 0; i < dtReceipt.Rows.Count; i++)
                {
                    if (dtReceipt.Rows[i]["SendDate"].ToString() != "")
                    {
                        dtReceipt.Rows[i]["SendDate"] = UtlString.FormatDateTw(dtReceipt.Rows[i]["SendDate"].ToString());
                    }
                    else
                    {
                        dtReceipt.Rows[i]["SendDate"] = "";
                    }
                }

                DataTable dtSendDesc = GetDescTable(dtSendSetting);

                //LocalReport localReport = null;
                //if (type == "3")//發文正本
                //{
                //    //localReport = new LocalReport { ReportPath = Server.MapPath("~/Reports/Rdlc/CaseMasterNoCopy.rdlc") };
                //    localReport = new LocalReport { ReportPath = Server.MapPath("~/Reports/Rdlc/SeizureSeizureCheckNo.rdlc") };

                //}
                //if (type == "4")//發文副本
                //{
                //   localReport = new LocalReport { ReportPath = Server.MapPath("~/Reports/Rdlc/CaseMasterNoCopy.rdlc") };
                //   //localReport = new LocalReport { ReportPath = Server.MapPath("~/Reports/Rdlc/CaseMasterJustCopy.rdlc") };
                //}

                //if (type != "4" && type != "3")//其他
                //{
                //    localReport = new LocalReport { ReportPath = Server.MapPath("~/Reports/Rdlc/CaseMaster.rdlc") };
                //}





                //localReport.DataSources.Add(new ReportDataSource("DataSet1", dtMaster));   //* 添加數據源,可以多個


                //subDataSource.Add(new ReportDataSource("SendSetting", dtSendSetting));
                //subDataSource.Add(new ReportDataSource("CaseAccountExternal", dtExternal));

                //20201005, Type=3的.. 要輸出收據 (不論X或Y)
                if (type == "3")
                {

                    #region Type3 Receipt
                    foreach (DataRow dr in dtReceipt.Rows)
                    {
                        string NoType = "X";
                        int at = dr["GovNo"].ToString().IndexOf("號");
                        if (at > 0)
                        {
                            strGovNo = dr["GovNo"].ToString().Substring(0, at);
                            NoType = strGovNo.Substring(strGovNo.Length - 1, 1);
                        }
                        if (!NoType.Equals("Y"))
                        {
                            dr.Delete();
                        }
                    }
                    dtReceipt.AcceptChanges();
                    #endregion
                }

                //20201005, 若Type=4的... X要輸出收據.. Y不用                
                if (type == "4")
                {
                    #region Type4 Receipt
                    //DataTable dtReceiptNew = new DataTable();
                    foreach (DataRow dr in dtReceipt.Rows)
                    {
                        string NoType = "X";
                        int at = dr["GovNo"].ToString().IndexOf("號");
                        if (at > 0)
                        {
                            strGovNo = dr["GovNo"].ToString().Substring(0, at);
                            NoType = strGovNo.Substring(strGovNo.Length - 1, 1);
                        }
                        if (NoType.Equals("Y"))
                        {
                            dr.Delete();
                        }
                    }

                    dtReceipt.AcceptChanges();
                    #endregion

                }



                #region 產出收據檔
                //if (dtReceipt.Rows.Count > 0)

                List<MultiKeyValue> dicReceipt = new List<MultiKeyValue>();


                Warning[] warnings1;
                string[] streams1;
                string mimeType1;
                string encoding1;
                string fileNameExtension1;
                string fileName1 = "ReportReceipt";

                foreach (DataRow dr in dtReceipt.Rows)
                {
                    string filter = string.Format("CaseId='{0}'", dr["CaseId"]);
                    DataTable dt = dtReceipt.Select(filter).CopyToDataTable();


                    LocalReport ReceiptReport = null;
                    ReceiptReport = new LocalReport { ReportPath = Server.MapPath("~/Reports/Rdlc/ReceiptSeizureCheckNo.rdlc") };
                    ReceiptReport.DataSources.Add(new ReportDataSource("CaseReceipt", dt));
                    var renderedReceiptBytes = ReceiptReport.Render("PDF",
                        null,
                        out mimeType1,
                        out encoding1,
                        out fileNameExtension1,
                        out streams1,
                        out warnings1);
                    ReceiptReport.Dispose();
                    dicReceipt.Add(new MultiKeyValue() { Caseid = Guid.Parse(dr["CaseId"].ToString()), Content = renderedReceiptBytes });
                    //dicReceipt.Add(Guid.Parse(dr["CaseId"].ToString()), renderedReceiptBytes);
                }


                #endregion


                //subDataSource.Add(new ReportDataSource("CaseReceipt", dtReceipt));

                // 準備合併
                List<byte[]> Result = new List<byte[]>();
                Warning[] warnings;
                string[] streams;
                string mimeType = "";
                string encoding;
                string fileNameExtension;
                string fileName = "ReportPay";


                if (type == "3" || type == "4")  // adam 2021/06/08
                {
                    foreach (DataRow dr in dtSendSetting.Rows)
                    {
                        string filter = string.Format("SerialID='{0}'", dr["SerialID"]);
                        DataTable dt = dtSendSetting.Select(filter).CopyToDataTable();

                        Guid caseid = Guid.Parse(dr["CaseId"].ToString());

                        LocalReport localReport = null;
                        if (type == "3")//發文正本
                        {
                            //localReport = new LocalReport { ReportPath = Server.MapPath("~/Reports/Rdlc/CaseMasterNoCopy.rdlc") };
                            localReport = new LocalReport { ReportPath = Server.MapPath("~/Reports/Rdlc/SeizureSeizureCheckNo.rdlc") };

                        }
                        if (type == "4")//發文副本
                        {
                            localReport = new LocalReport { ReportPath = Server.MapPath("~/Reports/Rdlc/SeizureSeizureCheckNoCC.rdlc") };
                            //localReport = new LocalReport { ReportPath = Server.MapPath("~/Reports/Rdlc/CaseMasterJustCopy.rdlc") };
                        }

                        localReport.SetParameters(listParm); //*添加參數

                        localReport.DataSources.Add(new ReportDataSource("Type3", dt));   //* 添加數據源,可以多個
                        localReport.SubreportProcessing += SubreportProcessingEventHandler;
                        subDataSource.Add(new ReportDataSource("SendSettingDesc", dtSendDesc));

                        string mimeTypeNew;

                        var renderedBytes = localReport.Render("PDF",
                            null,
                            out mimeTypeNew,
                            out encoding,
                            out fileNameExtension,
                            out streams,
                            out warnings);
                        localReport.Dispose();
                        Result.Add(renderedBytes);
                        // 找出收據檔.. 20201117, 收據檔, 放在第一個發文的後面...
                        if (dicReceipt.Any(x => x.Caseid == caseid))
                        {
                            var rec = dicReceipt.First(x => x.Caseid == caseid);
                            Result.Add(rec.Content);
                            dicReceipt.Remove(rec);
                        }

                    }
                }
                if( type=="5")
				{
                    dicReceipt.ForEach(x =>
                    {
                        Result.Add(x.Content);
                    });
                }

                Response.ClearContent();
                Response.ClearHeaders();

                byte[] totalResult = mergePdfs(Result);

                //return File(renderedBytes, mimeType, fileName + ".pdf");
                return File(totalResult, "application/pdf", fileName + ".pdf");

            }
            else
            {
                if (type == "2")
                {
                    string[] headerColumns = new[]
                        {
                        Lang.csfs_menu_sort,
                        Lang.csfs_case_no,
                        Lang.csfs_keyin_date,
                        Lang.csfs_usr_id,
                        Lang.csfs_client_id,
                        Lang.csfs_client_name,
                        Lang.csfs_pay_amt,
                        Lang.csfs_sum1,
                        Lang.csfs_gov_no,
                        Lang.csfs_case_kind2,
                        Lang.csfs_agnet,
                        "解扣日期",
                    };
                    CheckQueryAndPrintBIZ ckp = new CheckQueryAndPrintBIZ();
                    MemoryStream ms = ckp.Excel(headerColumns, new CheckQueryAndPrint { Type = "2" });
                    var fileName = Lang.csfs_menu_tit_checkqueryandprint + "_" + Lang.csfs_debtexcel + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xls";
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
                else
                { 
                    CheckQueryAndPrintBIZ CKP = new CheckQueryAndPrintBIZ();
                    List<ReportParameter> listParm = new List<ReportParameter>();
                    List<string> aryPayeeIdList = payeeIdList.Split(',').ToList();
                    DataTable dtPayeeSetting = CKP.GetQueryAndPrintByCaseIdList(aryPayeeIdList);

                    //Add by zhangwei 20180315 start
                    //判斷支票列印日期是否工作日，如果為非工作日自動帶入下一個營業日期
                    foreach (DataRow dr in dtPayeeSetting.Rows)
                    {
                        string strDate = dr["CheckDate"].ToString();
                        string strDateNew = CKP.GetWorkingDays(strDate);
                        dr["CheckDate"] = strDateNew;
                    }
                    //Add by zhangwei 20180315 end
                    LocalReport localReport = new LocalReport { ReportPath = Server.MapPath("~/Reports/Rdlc/ReportCheckNo.rdlc") };
                    localReport.SetParameters(listParm); //*添加參數
                    localReport.DataSources.Add(new ReportDataSource("CasePayeeSetting", dtPayeeSetting));

                    Warning[] warnings;
                    string[] streams;
                    string mimeType;
                    string encoding;
                    string fileNameExtension;

                    var renderedBytes = localReport.Render("PDF",
                        null,
                        out mimeType,
                        out encoding,
                        out fileNameExtension,
                        out streams,
                        out warnings);
                    localReport.Dispose();

                    Response.ClearContent();
                    Response.ClearHeaders();
                    return File(renderedBytes, mimeType, "Report.pdf");
                }
            }
        }

        internal byte[] mergePdfs(List<byte[]> pdfs)
        {
            MemoryStream outStream = new MemoryStream();
            using (Document document = new Document())
            using (PdfCopy copy = new PdfCopy(document, outStream))
            {
                document.Open();
                pdfs.ForEach(x => copy.AddDocument(new PdfReader(x)));
            }
            return outStream.ToArray();
        }

        //public ActionResult ReportPay(string caseIdList, string Con, string fileName)
        //{
        //    CaseMasterBIZ masterBiz = new CaseMasterBIZ();
        //    List<ReportParameter> listParm = new List<ReportParameter>();

        //    //* CTBC的地址.電話.傳真
        //    PARMCode codeItem = masterBiz.GetCodeData("REPORT_SETTING", "Address").FirstOrDefault();
        //    listParm.Add(new ReportParameter("CtbcAddr", codeItem == null ? "" : codeItem.CodeDesc));
        //    //* 20150618 電話號碼改為當前登入者的電話
        //    //codeItem = masterBiz.GetCodeData("REPORT_SETTING", "Tel").FirstOrDefault();
        //    LdapEmployeeBiz empBiz = new LdapEmployeeBiz();
        //    LDAPEmployee empNow = empBiz.GetLdapEmployeeByEmpId(LogonUser.Account);
        //    string tel = empNow != null && !string.IsNullOrEmpty(empNow.TelNo) ? empNow.TelNo : " ";
        //    tel = tel + (empNow != null && !string.IsNullOrEmpty(empNow.TelExt) ? " 分機 " + empNow.TelExt : "");
        //    listParm.Add(new ReportParameter("CtbcTel", tel));
        //    string ctbcTel = codeItem == null ? "" : codeItem.CodeDesc;
        //    codeItem = masterBiz.GetCodeData("REPORT_SETTING", "Fax").FirstOrDefault();
        //    listParm.Add(new ReportParameter("CtbcFax", codeItem == null ? "" : codeItem.CodeDesc));
        //    codeItem = masterBiz.GetCodeData("REPORT_SETTING", "ButtomLine").FirstOrDefault();
        //    listParm.Add(new ReportParameter("CtbcButtomLine", codeItem == null ? "" : codeItem.CodeDesc));
        //    codeItem = masterBiz.GetCodeData("REPORT_SETTING", "ButtomLine2").FirstOrDefault();
        //    listParm.Add(new ReportParameter("CtbcButtomLine2", codeItem == null ? "" : codeItem.CodeDesc));

        //    List<string> aryCaseIdList = caseIdList.Split(',').ToList();
        //    //* master
        //    DataTable dtMaster = masterBiz.GetCaseMasterByCaseIdList(aryCaseIdList);
        //    //* 發文設定
        //    DataTable dtSendSetting = null;
        //    if (Con == "Pay")
        //    {
        //        dtSendSetting = new CaseSendSettingBIZ().GetSeizurePayByCaseIdList(aryCaseIdList);
        //    }
        //    else
        //    {
        //        dtSendSetting = new CaseSendSettingBIZ().GetSendSettingByCaseIdList(aryCaseIdList);
        //    }
        //    //adam 2015-09-03 主管扣押並支付 支付時 不能出現扣押
        //    if (Con == "Director" || Con == null)
        //    {
        //        for (int i = 0; i < dtSendSetting.Rows.Count; i++)
        //        {
        //            DataRow[] old = dtMaster.Select("CaseId='" + dtSendSetting.Rows[i]["CaseId"].ToString() + "' and CaseKind2 ='扣押並支付' and AfterSeizureApproved = 1 ");
        //            if (old.Count() != 0)
        //            {
        //                if (dtSendSetting.Rows[i]["Template"].ToString().Trim() == "扣押")
        //                {
        //                    dtSendSetting.Rows[i].Delete();
        //                }
        //            }
        //        }
        //        dtSendSetting.AcceptChanges();
        //    }
        //    //adam 2015-09-03
        //    //* 20150723 新增空白判斷
        //    if (dtSendSetting == null || dtSendSetting.Rows.Count <= 0)
        //    {
        //        return Content("<script>alert('" + Lang.csfs_no_data + "');</script>");
        //    }
        //    //* 20150723 

        //    //* 外來文帳務明細
        //    DataTable dtExternal = new CaseAccountBiz().GetCaseAccountExternalByCaseIdList(aryCaseIdList);
        //    //* 發票
        //    DataTable dtReceipt = new CaseAccountBiz().GetCaseReceiptByCaseIdList(aryCaseIdList);
        //    for (int i = 0; i < dtReceipt.Rows.Count; i++)
        //    {
        //        if (dtReceipt.Rows[i]["SendDate"].ToString() != "")
        //        {
        //            dtReceipt.Rows[i]["SendDate"] = UtlString.FormatDateTw(dtReceipt.Rows[i]["SendDate"].ToString());
        //        }
        //        else
        //        {
        //            dtReceipt.Rows[i]["SendDate"] = "";
        //        }
        //    }


        //    DataTable dtSendDesc = GetDescTable(dtSendSetting);

        //    LocalReport localReport = null;
        //    if (Con == "Director")//主管作業排序
        //    {
        //        localReport = new LocalReport { ReportPath = Server.MapPath("~/Reports/Rdlc/CaseMasterForDirector.rdlc") };
        //    }
        //    else
        //    {
        //        localReport = new LocalReport { ReportPath = Server.MapPath("~/Reports/Rdlc/CaseMaster.rdlc") };
        //    }

        //    localReport.SetParameters(listParm); //*添加參數
        //    localReport.DataSources.Add(new ReportDataSource("DataSet1", dtMaster));   //* 添加數據源,可以多個
        //    localReport.DataSources.Add(new ReportDataSource("SendSetting", dtSendSetting));   //* 添加數據源,可以多個
        //    localReport.SubreportProcessing += SubreportProcessingEventHandler;

        //    subDataSource.Add(new ReportDataSource("SendSetting", dtSendSetting));
        //    subDataSource.Add(new ReportDataSource("CaseAccountExternal", dtExternal));
        //    subDataSource.Add(new ReportDataSource("CaseReceipt", dtReceipt));
        //    subDataSource.Add(new ReportDataSource("SendSettingDesc", dtSendDesc));

        //    Warning[] warnings;
        //    string[] streams;
        //    string mimeType;
        //    string encoding;
        //    string fileNameExtension;

        //    if (string.IsNullOrEmpty(fileName))
        //    {
        //        fileName = "Report";
        //    }

        //    var renderedBytes = localReport.Render("PDF",
        //        null,
        //        out mimeType,
        //        out encoding,
        //        out fileNameExtension,
        //        out streams,
        //        out warnings);
        //    localReport.Dispose();

        //    Response.ClearContent();
        //    Response.ClearHeaders();


        //    return File(renderedBytes, mimeType, fileName + ".pdf");
        //}

        private DataTable GetDescTable(DataTable dtSendInfo)
        {

            DataTable rtn = new DataTable();
            rtn.Columns.Add(new DataColumn("SerialID"));
            rtn.Columns.Add(new DataColumn("Title"));
            rtn.Columns.Add(new DataColumn("Content"));
            string strSerialId = "";
            if (dtSendInfo == null || dtSendInfo.Rows.Count <= 0)
                return rtn;
            try
            {
                string[] ary = { "一、", "二、", "三、", "四、", "五、", "六、", "七、", "八、", "九、", "十、" };
                foreach (DataRow row in dtSendInfo.Rows)
                {
                    if (Convert.ToString(row["SerialID"]) == strSerialId)
                        continue;
                    strSerialId = Convert.ToString(row["SerialID"]);
                    string strDesc = Convert.ToString(row["Description"]);
                    int iStart = 0;
                    string oldId = "";
                    for (int i = 0; i <= strDesc.Length - 2; i++)
                    {
                        if (ary.Contains(strDesc.Substring(i, 2)))
                        {
                            if (iStart == 0)
                            {
                                iStart = i + 2;
                                oldId = strDesc.Substring(i, 2);
                            }
                            else
                            {
                                string content = strDesc.Substring(iStart, i - iStart);
                                if (content.Substring(content.Length - 2) == "\r\n")
                                    content = content.Substring(0, content.Length - 2);
                                DataRow dr = rtn.NewRow();
                                dr["SerialID"] = strSerialId;
                                dr["Title"] = oldId;
                                dr["Content"] = content;
                                rtn.Rows.Add(dr);

                                iStart = i + 2;
                                oldId = strDesc.Substring(i, 2);
                            }
                        }

                        if (i == strDesc.Length - 2)
                        {
                            //* 最後2位
                            string content = strDesc.Substring(iStart, i - iStart + 2);
                            if (content.Substring(content.Length - 2) == "\r\n")
                                content = content.Substring(0, content.Length - 2);
                            DataRow dr = rtn.NewRow();
                            dr["SerialID"] = strSerialId;
                            dr["Title"] = oldId;
                            dr["Content"] = content;
                            rtn.Rows.Add(dr);
                        }
                    }
                }
                return rtn;
            }
            catch
            {
                return rtn;
            }

        }
        void SubreportProcessingEventHandler(object sender, SubreportProcessingEventArgs e)
        {
            foreach (var reportDataSource in subDataSource)
            {
                e.DataSources.Add(reportDataSource);
            }
        }

        public ActionResult OtherCheckNo(int payeeId)
        {
            return Json(new CheckNoSettingBIZ().OtherCheck(payeeId));
        }

        /// <summary>
        /// 設定保留號碼
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public ActionResult SavePreservCheck(int num)
        {
            return Json(new CheckNoSettingBIZ().PreservCheck(num));
        }

        /// <summary>
        /// 歸還保留號碼
        /// </summary>
        /// <returns></returns>
        public ActionResult ReturnPreservCheck()
        {
            return Json(new CheckNoSettingBIZ().ReturnPreservCheck());
        }

        public ActionResult ExportForExcel(string Type,string CheckDate,string CheckNoStart,string CheckNoEnd,string AmtConsistentType)
        {
            CheckQueryAndPrintBIZ ckp = new CheckQueryAndPrintBIZ();
            if (!string.IsNullOrEmpty( CheckDate.Trim()))
            {
                CheckDate= UtlString.FormatDateTwStringToAd(CheckDate);
            }
            MemoryStream ms = null;
            var fileName = "";//文檔名
            if (Type.Trim() == "1")
            {
                ms = ckp.ExportExcelForType1(CheckDate, CheckNoStart, CheckNoEnd, AmtConsistentType);
                fileName= Lang.csfs_menu_tit_checkqueryandprint +"_支票套印" + "_" + Lang.csfs_debtexcel + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xls";
            }
            else
            {
                ms = ckp.ExportExcelForType2(CheckDate, CheckNoStart, CheckNoEnd, AmtConsistentType);
                fileName = Lang.csfs_menu_tit_checkqueryandprint + "_支付明細" + "_" + Lang.csfs_debtexcel + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xls";
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