using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CTBC.CSFS.BussinessLogic;
using CTBC.CSFS.Models;
using CTBC.CSFS.Pattern;
using CTBC.CSFS.Resource;
using CTBC.CSFS.ViewModels;
using Microsoft.Reporting.WebForms;
using CTBC.FrameWork.Util;

namespace CTBC.CSFS.Areas.Common.Controllers
{

    public class CommonController : AppController
    {
        List<ReportDataSource> subDataSource = new List<ReportDataSource>();
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
        ///// <summary>
        ///// 更改來文機關類型.取得來文機關
        ///// </summary>
        ///// <param name="govKind"></param>
        ///// <returns></returns>
        //public JsonResult ChangGovUnit(string govKind)
        //{
        //    PARMCodeBIZ parm = new PARMCodeBIZ();
        //    List<KeyValuePair<string, string>> items = new List<KeyValuePair<string, string>>();
        //    if (string.IsNullOrEmpty(govKind)) return Json(items);
        //    //* 取得大類的CodeNo,以知道小類的CodeType
        //    var itemKind = parm.GetCodeData("GOV_KIND").FirstOrDefault(a => a.CodeDesc == govKind);
        //    if (itemKind == null) return Json(items);

        //    var list = parm.GetCodeData(itemKind.CodeNo);
        //    if (list.Any())
        //    {
        //        items.AddRange(list.Select(govUnit => new KeyValuePair<string, string>(govUnit.CodeNo.ToString(), govUnit.CodeDesc)));
        //    }
        //    return Json(items);
        //}
        public ActionResult Report(string caseIdList, string Con, string fileName)
        {
            CaseMasterBIZ masterBiz = new CaseMasterBIZ();
            List<ReportParameter> listParm = new List<ReportParameter>();

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

            List<string> aryCaseIdList = caseIdList.Split(',').ToList();
            //* master
            DataTable dtMaster = masterBiz.GetCaseMasterByCaseIdList(aryCaseIdList);
            //* 發文設定
            DataTable dtSendSetting = null;
            if (Con == "Pay")
            {
                dtSendSetting = new CaseSendSettingBIZ().GetSeizurePayByCaseIdList(aryCaseIdList);
            }
            else
            {
                 dtSendSetting = new CaseSendSettingBIZ().GetSendSettingByCaseIdList(aryCaseIdList);
            }
            if (dtSendSetting.Rows.Count > 0)
            {
                PARMCodeBIZ parm = new PARMCodeBIZ();
                for (int i = 0; i < dtSendSetting.Rows.Count; i++)
                {
                    // adam 20220809 CRPA
                    if (dtMaster.Rows[0]["CaseKind"].ToString() == "外來文案件")
                    {
                        if (dtMaster.Rows[0]["AgentUser"].ToString().Substring(0, 4) == "CRPA")
                        {
                            codeItem = masterBiz.GetCodeData("CRPA", "AgentUser").FirstOrDefault();
                            if (codeItem != null)
                            {
                                dtSendSetting.Rows[i]["CreatedUser"] =  codeItem.CodeDesc;

                            }
                            codeItem = masterBiz.GetCodeData("CRPA", "Tel").FirstOrDefault();
                            if (codeItem != null)
                            {
                                dtSendSetting.Rows[i]["TelNo"] =  codeItem.CodeDesc;
                            }
                        }
                    }
                    // adam end
                    if (dtSendSetting.Rows[i]["Security"].ToString().Trim() == "密")
                        {
                        dtSendSetting.Rows[i]["Security"] = "密(收到即解密)";
                        }
                }
                dtSendSetting.AcceptChanges();
            }

            //adam 2015-09-03 主管扣押並支付 支付時 不能出現扣押
            if (Con == "Director" || Con == null)
            {
                for (int i = 0; i < dtSendSetting.Rows.Count; i++)
                {
                    DataRow[] old = dtMaster.Select("CaseId='" + dtSendSetting.Rows[i]["CaseId"].ToString() + "' and CaseKind2 ='扣押並支付' and AfterSeizureApproved = 1 ");
                if (old.Count() != 0 )
                {
                         if (dtSendSetting.Rows[i]["Template"].ToString().Trim() == "扣押")
                         {
                             dtSendSetting.Rows[i].Delete();
                         }
                    }
                }
                dtSendSetting.AcceptChanges();
            }
            //adam 2015-09-03
            //* 20150723 新增空白判斷
            if (dtSendSetting == null || dtSendSetting.Rows.Count <= 0)
            {
                return Content("<script>alert('" + Lang.csfs_no_data + "');</script>");
            }
            //* 20150723 

            //* 外來文帳務明細
            DataTable dtExternal = new CaseAccountBiz().GetCaseAccountExternalByCaseIdList(aryCaseIdList);
            //* 發票
            DataTable dtReceipt = new CaseAccountBiz().GetCaseReceiptByCaseIdList(aryCaseIdList);
            for (int i = 0; i < dtReceipt.Rows.Count; i++)
            {
                if(dtReceipt.Rows[i]["SendDate"].ToString()!=""){
                    dtReceipt.Rows[i]["SendDate"] = UtlString.FormatDateTw(dtReceipt.Rows[i]["SendDate"].ToString());
                }else{
                     dtReceipt.Rows[i]["SendDate"]="";
                }
            }

           
            DataTable dtSendDesc = GetDescTable(dtSendSetting);

            LocalReport localReport = null;
            if (Con == "Director")//主管作業排序
            {
                localReport = new LocalReport { ReportPath = Server.MapPath("~/Reports/Rdlc/CaseMasterForDirector.rdlc") };
            }
            else
            {
                localReport = new LocalReport { ReportPath = Server.MapPath("~/Reports/Rdlc/CaseMaster.rdlc") };
            }
            
            localReport.SetParameters(listParm); //*添加參數
            localReport.DataSources.Add(new ReportDataSource("DataSet1", dtMaster));   //* 添加數據源,可以多個
            localReport.DataSources.Add(new ReportDataSource("SendSetting", dtSendSetting));   //* 添加數據源,可以多個
            localReport.SubreportProcessing += SubreportProcessingEventHandler;

            subDataSource.Add(new ReportDataSource("SendSetting", dtSendSetting));
            subDataSource.Add(new ReportDataSource("CaseAccountExternal", dtExternal));
            subDataSource.Add(new ReportDataSource("CaseReceipt", dtReceipt));
            subDataSource.Add(new ReportDataSource("SendSettingDesc", dtSendDesc));

            Warning[] warnings;
            string[] streams;
            string mimeType;
            string encoding;
            string fileNameExtension;

            if (string.IsNullOrEmpty(fileName))
            {
                fileName = "Report";
            }

            var renderedBytes = localReport.Render("PDF",
                null,
                out  mimeType,
                out encoding,
                out fileNameExtension,
                out streams,
                out warnings);
            localReport.Dispose();

            Response.ClearContent();
            Response.ClearHeaders();


            return File(renderedBytes, mimeType, fileName+".pdf");
        }
        void SubreportProcessingEventHandler(object sender, SubreportProcessingEventArgs e)
        {
            foreach (var reportDataSource in subDataSource)
            {
                e.DataSources.Add(reportDataSource);
            }
        }

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

        /// <summary>
        /// 判斷當前是否可以發文
        /// </summary>
        /// <returns></returns>
        public JsonResult CanBeSendKeyin()
        {
            JsonReturn rtn = new JsonReturn();
            LdapEmployeeBiz ldapEmploee = new LdapEmployeeBiz();
            //20170714 固定 RQ-2015-019666-019 派件至跨單位 宏祥 update start
            //List<LDAPEmployee> list = ldapEmploee.GetAllEmployeeInEmployeeView();
            List<LDAPEmployee> list = ldapEmploee.GetAllDepartmentEmployeeInEmployeeView();
            //20170714 固定 RQ-2015-019666-019 派件至跨單位 宏祥 update end
            ViewBag.Emploee = false;
            //* 如果是一二三科的人可以不受限制
            if (list.Any(item => item.EmpId == LogonUser.Account))
            {
                rtn.ReturnCode = "1";
                rtn.ReturnMsg = "";
                return Json(rtn);
            }

            int endTime;
            int nowTime;
            PARMCodeBIZ parm = new PARMCodeBIZ();
            string caseEndTime = parm.GetCASE_END_TIME("CASE_END_TIME");
            int.TryParse(caseEndTime, out endTime);
            int.TryParse(DateTime.Now.ToString("HHmm"), out nowTime);
            int result = endTime - nowTime;
            if (result <= 0)
            {
                //* 已過期
                rtn.ReturnCode = "0";
                rtn.ReturnMsg = Lang.csfs_case_end_msg + "(" + DateTime.Now.ToString("HH:mm") + ")";
            }
            else if (result <= 50)
            {
                //* 即將過期
                rtn.ReturnCode = "2";
                rtn.ReturnMsg = string.Format(Lang.csfs_case_end_soon_msg, caseEndTime) + "(" + DateTime.Now.ToString("HH:mm") + ")";
            }
            else
            {
                rtn.ReturnCode = "1";
                rtn.ReturnMsg = "";
            }
            return Json(rtn);
        }
    }
}