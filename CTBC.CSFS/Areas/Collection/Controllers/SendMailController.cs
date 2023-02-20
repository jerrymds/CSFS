using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CTBC.CSFS.Models;
using CTBC.CSFS.Resource;
using CTBC.CSFS.BussinessLogic;
using CTBC.CSFS.ViewModels;
using CTBC.FrameWork.Util;
using CTBC.CSFS.Pattern;
using System.IO;
using System.Data;
using CTBC.CSFS.Filter;
using Microsoft.Reporting.WebForms;
using System.Configuration;
using System.Text;
using iTextSharp.text;
using iTextSharp.text.pdf;
using CTBC.CSFS.WebService.OpenStream;
using Newtonsoft.Json;

namespace CTBC.CSFS.Areas.Collection.Controllers
{
    public class SendMailController : AppController
    {
        List<ReportDataSource> subDataSource = new List<ReportDataSource>();
        CaseMasterBIZ master = new CaseMasterBIZ();
        static string strSqlWhere = string.Empty;

        //
        // GET: /Collection/SendMail/

        [RootPageFilter]
        public ActionResult Index()
        {
            BindType();
            ViewBag.MailDate = UtlString.FormatDateTw(DateTime.Now.ToString("yyyy/MM/dd"));
            //20170811 RC RQ-2015-019666-020 派件至跨單位 宏祥 update start
            //ViewBag.AgentUserList = new SelectList(new LdapEmployeeBiz().GetAllEmployeeInEmployeeView(), "EmpID", "EmpIdAndName");
            PARMCodeBIZ parm = new PARMCodeBIZ();
            ViewBag.CheckDays = parm.GetPARMCodeByCodeTypeAndCodeDesc("CheckDays", "檢查日期範圍天數");
            ViewBag.AgentDepartmentList = new SelectList(parm.GetCodeData("CollectionToAgent_AgentDDLDepartment"), "CodeNo", "CodeDesc");
            ViewBag.AgentDepartment2List = new SelectList(new AgentSettingBIZ().GetAgentDepartment2View(), "SectionName", "SectionName");
            ViewBag.AgentDepartmentUserList = new SelectList(new AgentSettingBIZ().GetAgentDepartmentUserView(), "EmpID", "EmpIdAndName");
            //20170811 RC RQ-2015-019666-020 派件至跨單位 宏祥 update end
            return View();
        }

        public void BindType()
        {
            List<SelectListItem> list = new List<SelectListItem>()
            {
                new SelectListItem(){Text=Lang.csfs_select,Value="-1"},
                new SelectListItem(){Text="扣押類",Value="0"},
                new SelectListItem(){Text="支付類",Value="1"},
                new SelectListItem(){Text="外來文類",Value="2"},
                new SelectListItem(){Text="國稅局死亡",Value="3"}
            };
            ViewBag.Type = list;
            List<SelectListItem> listItem = new List<SelectListItem>()
            {
                 new SelectListItem(){Text=Lang.csfs_select,Value="0"},
                new SelectListItem(){Text="未掛號",Value="1"},
                new SelectListItem(){Text="已掛號",Value="2"}
            };
            ViewBag.MailStatus = listItem;           
        }

        #region 查詢主管機關結果集
        /// <summary>
        /// 結果列表
        /// </summary>
        /// <param name="AToH"></param>
        /// <param name="pageNum"></param>
        /// <param name="strSortExpression"></param>
        /// <param name="strSortDirection"></param>
        /// <returns></returns>
        public ActionResult _QueryResult(CaseMaster model, int pageNum = 1, string strSortExpression = "Sno", string strSortDirection = "asc")
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
        public CaseSeizureViewModel SearchList(CaseMaster model, int pageNum = 1, string strSortExpression = "Sno", string strSortDirection = "asc")
        {
            model.SendDateStart = UtlString.FormatDateTwStringToAd(model.SendDateStart);
            model.SendDateEnd = UtlString.FormatDateTwStringToAd(model.SendDateEnd);
            model.CloseDateStart = UtlString.FormatDateTwStringToAd(model.CloseDateStart);
            model.CloseDateEnd = UtlString.FormatDateTwStringToAd(model.CloseDateEnd);
            model.MailDateStart = UtlString.FormatDateTwStringToAd(model.MailDateStart);
            model.MailDateEnd = UtlString.FormatDateTwStringToAd(model.MailDateEnd);

            IList<CaseMaster> result = master.GetQueryList(model, pageNum, strSortExpression, strSortDirection, ref strSqlWhere);

            UInt64 minNo = 0;
            UInt64 maxNo = 0;

//

            if (result != null && result.Any())
            {
                if (result.Any(m => !string.IsNullOrEmpty(m.MailNo)))
                {
                    minNo = result.Where(m => !string.IsNullOrEmpty(m.MailNo)).Select(n => Convert.ToUInt64(n.MailNo)).ToList().Min();
                    maxNo = result.Where(m => !string.IsNullOrEmpty(m.MailNo)).Select(n => Convert.ToUInt64(n.MailNo)).ToList().Max();
                }                
            }
            ViewBag.MinNo = minNo;
            ViewBag.MaxNo = maxNo;
            foreach (var r in result)
            {
                if (r.MailNo.Length > 0)
                {
                    r.MailNo = r.MailNo.PadLeft(14, '0');
                }
            }

            var viewModel = new CaseSeizureViewModel()
            {
                CaseMaster = model,
                CaseMasterlist = result
            };
            // 只針對地址條每日250件,先暫時固定每頁300,明年可改模版 2022/12/12
            viewModel.CaseMaster.PageSize = 300;
            //viewModel.CaseMaster.PageSize = master.PageSize;
            viewModel.CaseMaster.CurrentPage = master.PageIndex;
            viewModel.CaseMaster.TotalItemCount = master.DataRecords;
            viewModel.CaseMaster.SortExpression = strSortExpression;
            viewModel.CaseMaster.SortDirection = strSortDirection;

            return viewModel;
        }
        #endregion

        #region 設定掛號號碼
        public ActionResult CreateMailNo(CaseMaster model, string strIds,string strType)
        {           
            model.MailDate = UtlString.FormatDateTwStringToAd(model.MailDate);
            string userId = LogonUser.Account;
            string[] aryId = strIds.Split(',');
        //    return Json(master.CreateMailNo(model, aryId, userId) == 1 ? new JsonReturn() { ReturnCode = "1", ReturnMsg = "" }
        //: master.CreateMailNo(model, aryId, userId) == 2 ? new JsonReturn() { ReturnCode = "2", ReturnMsg = "" } :
        //                               new JsonReturn() { ReturnCode = "0", ReturnMsg = Lang.csfs_del_fail });
            return Json(master.CreateMailNo(model, aryId, userId,strType) == 1 ? new JsonReturn() { ReturnCode = "1", ReturnMsg = "" }
                 : master.CreateMailNo(model, aryId, userId,strType) == 2 ? new JsonReturn() { ReturnCode = "2", ReturnMsg = "" } :
                                                new JsonReturn() { ReturnCode = "0", ReturnMsg = Lang.csfs_del_fail });
        }
        #endregion

        #region 取消掛號號碼
        //* 是否存在
        public ActionResult IsExistMailNo(string strIds)
        {
            List<string> idList = strIds.Split(',').ToList();
            return Json(master.IsExistsMailNo(idList) == idList.Count ? new JsonReturn() { ReturnCode = "1", ReturnMsg = "" }
                                                        : new JsonReturn() { ReturnCode = "0", ReturnMsg = Lang.csfs_del_fail });
        }

        //刪除
        public ActionResult DeleteMailNo(string strIds)
        {
            List<string> idList = strIds.Split(',').ToList();
            return Json(master.DeleteMailNo(idList) == 1 ? new JsonReturn() { ReturnCode = "1", ReturnMsg = "" }
                : master.DeleteMailNo(idList) == 2 ? new JsonReturn() { ReturnCode = "2", ReturnMsg = "" } :
                    new JsonReturn() { ReturnCode = "0", ReturnMsg = Lang.csfs_del_fail });
        }
        #endregion

        public ActionResult ExcelMailNo(string postType, string strMailNo1, string strMailNo2)
        {
            MemoryStream ms = new MemoryStream();
            string fileName = string.Empty;
            string TemplatePath = Server.MapPath("~/Reports/ExcelTemplate/MailNo2020.xls");
            ms = master.MailNoExcel_NPOI(TemplatePath, postType, strSqlWhere, strMailNo1, strMailNo2, base.LogonUser.Name);
            fileName = "大宗掛號單_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xls";

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

        public ActionResult Report(string detailIdList)
        {
            List<ReportParameter> listParm = new List<ReportParameter>();

            //* CTBC的地址.電話.傳真
            PARMCode codeItem = master.GetCodeData("REPORT_SETTING", "Address").FirstOrDefault();
            listParm.Add(new ReportParameter("CtbcAddr", codeItem == null ? "" : codeItem.CodeDesc));
            LdapEmployeeBiz empBiz = new LdapEmployeeBiz();
            LDAPEmployee empNow = empBiz.GetLdapEmployeeByEmpId(LogonUser.Account);
            string tel = empNow != null && !string.IsNullOrEmpty(empNow.TelNo) ? empNow.TelNo : "";
            tel = tel + (empNow != null && !string.IsNullOrEmpty(empNow.TelExt) ? " 分機 " + empNow.TelExt : "");
            listParm.Add(new ReportParameter("CtbcTel", tel));
            string ctbcTel = codeItem == null ? "" : codeItem.CodeDesc;
            codeItem = master.GetCodeData("REPORT_SETTING", "Fax").FirstOrDefault();
            listParm.Add(new ReportParameter("CtbcFax", codeItem == null ? "" : codeItem.CodeDesc));
            codeItem = master.GetCodeData("REPORT_SETTING", "ButtomLine").FirstOrDefault();
            listParm.Add(new ReportParameter("CtbcButtomLine", codeItem == null ? "" : codeItem.CodeDesc));
            codeItem = master.GetCodeData("REPORT_SETTING", "ButtomLine2").FirstOrDefault();
            listParm.Add(new ReportParameter("CtbcButtomLine2", codeItem == null ? "" : codeItem.CodeDesc));

            List<string> aryDetailIdList = detailIdList.Split(',').ToList();
            //* 發文設定
            DataTable dtMailNo = master.MailNoReportList(aryDetailIdList);
            dtMailNo.DefaultView.Sort = "Sort asc";

            DataTable dtSendDesc = GetDescTable(dtMailNo);

            LocalReport localReport = new LocalReport { ReportPath = Server.MapPath("~/Reports/Rdlc/SeizureSeizureForMail.rdlc") };
            localReport.SetParameters(listParm); //*添加參數
            localReport.DataSources.Add(new ReportDataSource("SendSetting", dtMailNo));   //* 添加數據源,可以多個
            //localReport.DataSources.Add(new ReportDataSource("SendSetting", dtMailNo));

            subDataSource.Add(new ReportDataSource("SendSettingDesc", dtSendDesc));
            localReport.SubreportProcessing += SubreportProcessingEventHandler;

            Warning[] warnings;
            string[] streams;
            string mimeType;
            string encoding;
            string fileNameExtension;

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
            return File(renderedBytes, mimeType, "Report.pdf");
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
                DataRow[] rows = dtSendInfo.Select("1=1", "SerialID ASC");
                string[] ary = { "一、", "二、", "三、", "四、", "五、", "六、", "七、", "八、", "九、", "十、" };
                foreach (DataRow row in rows)
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
                            string content = strDesc.Substring(iStart, i - iStart + 2);
                            if (content.Substring(content.Length - 2) == "\r\n")
                                content = content.Substring(0, content.Length - 2);
                            //* 最後2位
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

        public ActionResult ReportAddr(string postType, string strMailNo1, string strMailNo2, string inMailNo, string inDetailsId)
        {
            DataTable DT = new DataTable();
            DataTable dtMap = new DataTable();
            dtMap.Columns.Add("DetailsId");
            dtMap.Columns.Add("No");
            CaseMasterBIZ masterBiz = new CaseMasterBIZ();
            List<ReportParameter> listParm = new List<ReportParameter>();

            string strDetailsId = "";
            string[] aryId = inDetailsId.Split(',');
            List<string> DetailIdList = aryId.ToList();//獲取detailsId

            foreach (var item in DetailIdList)//判斷是否是當天設置的
            {
               // string mailCreateDate = MailNoFroCreateDate(item.Split('|')[0]);//item
                if (item.Split('|')[0].ToString().Trim().Length > 0)
                {
                    strDetailsId = strDetailsId + "'" + item.Split('|')[0].ToString() + "',";
                    DataRow drMap = dtMap.NewRow();
                    drMap["DetailsId"] = item.Split('|')[0].ToString().Trim();
                    drMap["No"] = item.Split('|')[1].ToString().Trim();
                    dtMap.Rows.Add(drMap);
                }
            }
            if (strDetailsId.Length > 0)
            {
                strDetailsId = strDetailsId.Substring(0, strDetailsId.Length - 1);
            }

            DataTable dtSendSetting = master.AddrSearchList(postType, strMailNo1, strMailNo2, inMailNo, strDetailsId);
            DataTable dtSendDesc = GetAddr(dtSendSetting);

            LocalReport localReport = new LocalReport { ReportPath = Server.MapPath("~/Reports/Rdlc/ReportAddr.rdlc") };
            localReport.SetParameters(listParm); //*添加參數
            localReport.DataSources.Add(new ReportDataSource("SendSetting", dtSendDesc));

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
            string newFileName = LogonUser.Account + "_" + DateTime.Now.ToString("yyyyMMddhhmmssfff") + "_ReportAddress.pdf";
            string FilePath = ConfigurationManager.AppSettings["txtMailAddressPath"] + @"\"+ newFileName;
            using (FileStream fs = new FileStream(FilePath, FileMode.Create, FileAccess.Write))
            {
                fs.Write(renderedBytes, 0, renderedBytes.Length);
            }


            Response.ClearContent();
            Response.ClearHeaders();
            //return File(renderedBytes, mimeType, "Report.pdf");
            return Content(newFileName);
        }
        public ActionResult OpenTxtDoc(string FileName, string ConfigPath, string FileFormat)
        {
            // 文件路徑
            //string FilePath = ConfigurationManager.AppSettings[ConfigPath] + @"\";

            //string FileType = string.Format("application/{0}", FileFormat);

            //OpenFileStream webService = new OpenFileStream();

            //webService.Url = ConfigurationManager.AppSettings["MailAddressServiceURL"];


            //byte[] stream = webService.OpenFile(FilePath + FileName);
            //if (stream != null)
            //{               
            //    return File(stream, FileType, FileName);
            //}
            //else
            //{               
            //    return null;
            //}
            // 文件路徑
            string FilePath = ConfigurationManager.AppSettings[ConfigPath] + @"\";

            string FileType = string.Format("application/{0}", FileFormat);

            string absoluFilePath = FilePath + FileName;
            if (!System.IO.File.Exists(absoluFilePath))
            {
                //return Content("<script>alert('" + Lang.csfs_FileNotFound +"');</script>");
                CSFSURL.attDownload csDown = new CSFSURL.attDownload();
                string returnFile = csDown.UrlDownloadFile(FilePath, FileName);
                if (returnFile != null)
                {
                    byte[] file = Convert.FromBase64String(returnFile);
                    return File(file, "application/pdf", FileName);
                }
                else
                {
                    return Content("<script>alert('" + Lang.csfs_no_data + "');</script>");
                }
            }
            return File(new FileStream(absoluFilePath, FileMode.Open), "application/octet-stream", Server.UrlEncode(FileName));

        }
        public ActionResult OpenPDF(string FileName)
        {
            // 文件路徑
            string FilePath = ConfigurationManager.AppSettings["txtFilePath"] + @"\";

            OpenFileStream webService = new OpenFileStream();

            webService.Url = ConfigurationManager.AppSettings["ServiceURL"];

            dynamic stream = webService.OpenFile(FilePath + FileName);

            if (stream != null)
            {
                return File(stream.Content.ToObject<byte[]>(), "application/pdf", FileName);
            }
            else
            {
                return Content("<script>alert('" + Lang.csfs_FileNotFound + "');</script>");
                //return null;
            }

            //return File(FilePath + FileName, "application/pdf", FileName);

        }

        //public ActionResult ReportAddr(string postType, string strMailNo1, string strMailNo2)
        //{
        //    CaseMasterBIZ masterBiz = new CaseMasterBIZ();
        //    List<ReportParameter> listParm = new List<ReportParameter>();

        //    DataTable dtSendSetting = master.AddrSearchList(postType, strMailNo1, strMailNo2);
        //    DataTable dtSendDesc = GetAddr(dtSendSetting);

        //    LocalReport localReport = new LocalReport { ReportPath = Server.MapPath("~/Reports/Rdlc/ReportAddr.rdlc") };
        //    localReport.SetParameters(listParm); //*添加參數
        //    localReport.DataSources.Add(new ReportDataSource("SendSetting", dtSendDesc));

        //    Warning[] warnings;
        //    string[] streams;
        //    string mimeType;
        //    string encoding;
        //    string fileNameExtension;

        //    var renderedBytes = localReport.Render("PDF",
        //        null,
        //        out  mimeType,
        //        out encoding,
        //        out fileNameExtension,
        //        out streams,
        //        out warnings);
        //    localReport.Dispose();

        //    Response.ClearContent();
        //    Response.ClearHeaders();
        //    return File(renderedBytes, mimeType, "Report.pdf");
        //}

        private DataTable GetAddr(DataTable dtAddr)
        {
            DataTable rtn = new DataTable();
            rtn.Columns.Add(new DataColumn("gov"));
            rtn.Columns.Add(new DataColumn("gov2"));
            if (dtAddr == null || dtAddr.Rows.Count <= 0)
                return rtn;
            for (int i = 0; i < dtAddr.Rows.Count; i++)
            {
                DataRow dr = rtn.NewRow();
                dr["gov"] = Convert.ToString(dtAddr.Rows[i][0]);
                if (i + 1 < dtAddr.Rows.Count)
                {
                    dr["gov2"] = Convert.ToString(dtAddr.Rows[i + 1][0]);
                    i = i + 1;
                }
                else
                {
                    dr["gov2"] = "";
                }
                rtn.Rows.Add(dr);
            }
            return rtn;
        }
    }
}