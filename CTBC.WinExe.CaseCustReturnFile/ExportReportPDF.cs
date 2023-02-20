using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.IO;
using CTBC.CSFS.Pattern;
using Microsoft.Reporting.WinForms;
using CTBC.FrameWork.Util;
using System.Windows.Forms;
using System.Configuration;
using iTextSharp.text.pdf;
using iTextSharp.text;
using CTBC.CSFS.Models;
using CTBC.CSFS.BussinessLogic;
using Microsoft.International.Formatters;
using System.Globalization;
using ICSharpCode.SharpZipLib.Zip;

namespace CTBC.WinExe.CaseCustReturnFile
{
    public class ExportReportPDF : BaseBusinessRule
    {
        #region 全局變量

        private static FileLog _m_fileLog;
        private string filePath = ConfigurationManager.AppSettings["PDFFilePath"].ToString();
        private string txtFilePath = ConfigurationManager.AppSettings["txtFilePath"].ToString();

        List<ReportDataSource> subDataSource = new List<ReportDataSource>();

        /// <summary>
        /// 可獨立代入m_fileLog
        /// </summary>
        public FileLog m_fileLog
        {
            set { _m_fileLog = value; }
        }

        #endregion 全局變量

        #region 構造函數

        public ExportReportPDF(FileLog p_m_fileLog)
        {
            _m_fileLog = p_m_fileLog;
        }

        #endregion 構造函數

        #region 匯出報表

        /// <summary>
        /// 保存為PDF
        /// </summary>
        /// <param name="drPDFList">回文主檔內容</param>
        /// <param name="strFlag">Y: 產生首頁，N:不產生首頁 </param>
        /// <param name="strSendNoNow">發文字號，strFlag為Y才有 </param>
        public string SavePDF(DataRow drPDFList, string strFlag, string strSendNoNow)
        {
            string strReturn = "Y";

            try
            {
                _m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.Information, "--------------------------[存款資料及存款往來明細資料]列印開始--------------------------");

                string strNewID = "";
                if (drPDFList != null)
                {
                    strNewID = drPDFList["NewID"].ToString();
                }
                // 查詢案件下所有人員HTG發查明細
                DataTable dtSheetData = Get401RecvData(strNewID);
                _m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.Information, "--------------------------基資, 共" + dtSheetData.Rows.Count + "筆");
                DataTable dtSheetTwoDetailData = GetRFDMRecvData(strNewID);
                _m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.Information, "--------------------------明細, 共" + dtSheetTwoDetailData.Rows.Count + "筆");
                DataTable dtSBoxData = GetSbox(strNewID);
                _m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.Information, "--------------------------保管箱, 共" + dtSBoxData.Rows.Count + "筆");




                // 20220504, 若是dtShetTwoDetailsData 重新編寫JRNL_NO, group by Account_NO, TRAN_DATE , 由1開始...
                dtSheetTwoDetailData = reOrderJRNL(dtSheetTwoDetailData);





                DataTable dtSendSetting = null;

                if ((dtSheetData == null || dtSheetData.Rows.Count == 0) 
                    && (dtSheetTwoDetailData == null || dtSheetTwoDetailData.Rows.Count == 0)
                    && (dtSBoxData == null || dtSBoxData.Rows.Count == 0))
                {
                    _m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.Information, "未產生PDF文件, 因查無資料");
                    strReturn = "N";
                    return strReturn;
                }

                #region 產生首頁 & .sw檔案 & .di檔案

                // 判斷產生首頁, Y: 產生
                string attachment = string.Empty;

                if (strFlag == "Y")
                {
                    #region PDF首頁

                    dtSendSetting = new DataTable();
                    dtSendSetting = drPDFList.Table.Clone();
                    dtSendSetting.Clear();
                    dtSendSetting.ImportRow(drPDFList);
                    dtSendSetting.Columns.Add("SendNo");
                    //20180622 RC 線上投單CR修正 宏祥 add start
                    dtSendSetting.Columns.Add("SendGovName");
                    //20180622 RC 線上投單CR修正 宏祥 add end

                    // 判斷實際有那些檔案存在
                    if (File.Exists(txtFilePath + "\\" + drPDFList["ROpenFileName"].ToString()))
                    {
                        attachment = drPDFList["ROpenFileName"].ToString();
                    }

                    if (File.Exists(txtFilePath + "\\" + drPDFList["RFileTransactionFileName"].ToString()))
                    {
                        attachment += ("、" + drPDFList["RFileTransactionFileName"]);
                    }

                    if (!string.IsNullOrEmpty(attachment))
                    {
                        if (attachment.StartsWith("、"))
                        {
                            attachment = attachment.Replace("、", "");
                        }
                    }

                    dtSendSetting.Columns.Add("Attachment");
                    dtSendSetting.Rows[0]["Attachment"] = attachment;
                }

                // Report資料來源及參數
                List<ReportDataSource> mainDataSource = new List<ReportDataSource>();
                string strFileName = drPDFList["DocNo"].ToString() + "_" + drPDFList["Version"].ToString();

                // 存放檔案名稱list
                List<string> strListFileName = new List<string>();

                // 判斷文件夾是否存在，若不存在，則創建
                if (!Directory.Exists(filePath)) Directory.CreateDirectory(filePath);

                if (dtSendSetting != null && dtSendSetting.Rows.Count > 0)
                {
                    List<ReportParameter> listParm = new List<ReportParameter>();
                    DataRow drData = dtSendSetting.Rows[0];

                    listParm.Add(new ReportParameter("CtbcAddr", drData["Address"].ToString()));
                    listParm.Add(new ReportParameter("CtbcButtomLine", drData["ButtomLine"].ToString()));
                    listParm.Add(new ReportParameter("CtbcButtomLine2", drData["ButtomLine2"].ToString()));

                    string strSubject = drData["Subject"].ToString();
                    string strDescription = drData["Description"].ToString();

                    string strGovName = "";
                    if (drData["LetterNo"] != null && drData["LetterNo"].ToString() != "")
                    {
                        strGovName = drData["LetterNo"].ToString();
                    }

                    strSubject = strSubject.Replace("{GovNo}", strGovName);

                    string dtTimeNow = "";
                    if (drData["LetterDate"] != null && drData["LetterDate"].ToString() != "")
                    {
                        dtTimeNow = Convert.ToDateTime(drData["LetterDate"].ToString()).ToString("yyyy/MM/dd");
                    }

                    if (dtTimeNow != "")
                    {
                        int iyy = Convert.ToInt16(dtTimeNow.Substring(0, 4)) - 1911;
                        string strmm = dtTimeNow.Substring(5, 2);
                        string strdd = dtTimeNow.Substring(8, 2);
                        dtTimeNow = iyy.ToString() + strmm + strdd;
                    }

                    strSubject = strSubject.Replace("{GovDate}", dtTimeNow);
                    strDescription = strDescription.Replace("{ObligorNo}", drData["CustIdNo"].ToString());
                    strDescription = strDescription.Replace("{CaseNo}", drData["DocNo"].ToString());
                    strDescription = strDescription.Replace("{InCharge}", drData["InCharge"].ToString());

                    drData["Subject"] = strSubject;
                    drData["Description"] = strDescription.Replace("\n", "\n\n");
                    drData["SendNo"] = strSendNoNow;
                    //20180622 RC 線上投單CR修正 宏祥 add start                 
                    string strSendGovName = getCodeDesc("SendGovName");
                    drData["SendGovName"] = strSendGovName;
                    //20180622 RC 線上投單CR修正 宏祥 add end

                    mainDataSource = new List<ReportDataSource>();
                    mainDataSource.Add(new ReportDataSource("SendSetting", dtSendSetting));

                    string strFileNameOne = strFileName + "_001";
                    SavePDFByLocalReport("RptRecieve.rdlc", strFileNameOne, mainDataSource, listParm);

                    strListFileName.Add(filePath + @"\" + strFileNameOne + ".pdf");

                    #endregion PDF首頁

                    #region 20180323,PeterHsieh : 中信擴充需求，產生 .sw & .di檔案
                    // 產生 .sw檔

                    // 取得 .sw內容，產生實體檔案
                    WriteFile(filePath, strFileName + ".sw",
                       string.Format(GetSWContentTemplate(),
                                     drPDFList["LetterDeptName"].ToString(),
                                     drPDFList["LetterDeptNo"].ToString()
                                    )
                    );

                    // 產生 di檔
                    // 取得 .di檔案內容樣版
                    String[] descContent = strDescription.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);

                    string diContentTemplate = GetDIContentTemplate(descContent.Length);

                    string govName = ConfigurationManager.AppSettings["GovName"];
                    string govCode = ConfigurationManager.AppSettings["GovCode"];

                    /*
                     * 填入資料
                     * {0} : .zip filename
                     * {1} : .sw filename
                     * {2} : 發文機關全銜
                     * {3} : 發文機關機關代碼
                     * {4} : 發文機關地址
                     * {5} : 發文機關聯絡方式(承辦人)
                     * {6} : 發文機關聯絡方式(電話 / 分機)
                     * {7} : 發文日期, fmt. 中華民國xxx年xx月xx日
                     * {8} : 發文字號(字) //20180622 RC 線上投單CR修正 宏祥 add
                     * {9} : 發文字號(年度)
                     * {10} : 發文字號(流水號)
                     * {11} : 附件(基本資料檔名 + 交易明細檔名)
                     * {12} : 主旨
                     * {13~n} : 說明(1 ~ n)
                     * {n+1} : 收文機關全銜
                     */
                    string diContent = string.Format(diContentTemplate,
                        strFileName + ".zip",
                        strFileName + ".sw",
                        govName,
                        govCode,
                        drData["Address"].ToString(),
                        drPDFList["EmpName"].ToString(),
                        drPDFList["TelNo"].ToString() + (string.IsNullOrEmpty(drPDFList["TelExt"].ToString()) ? "" : "  分機 " + drPDFList["TelExt"].ToString()),
                        ToFullTaiwanDate(DateTime.Now),
                        //20180622 RC 線上投單CR修正 宏祥 update start
                        strSendGovName,
                        //20180622 RC 線上投單CR修正 宏祥 update end
                        strSendNoNow.Substring(0, 3),
                        strSendNoNow.Substring(3),
                        attachment,
                        strSubject,
                        descContent[0].Substring(descContent[0].IndexOf("、") + 1).Replace("\r", "").Replace("{ObligorNo}", drData["CustIdNo"].ToString()),
                        descContent[1].Substring(descContent[1].IndexOf("、") + 1).Replace("\r", "").Replace("{InCharge}", drData["InCharge"].ToString()),
                        descContent[2].Substring(descContent[2].IndexOf("、") + 1).Replace("\r", ""),
                        descContent[3].Substring(descContent[3].IndexOf("、") + 1).Replace("\r", "").Replace("{CaseNo}", drData["DocNo"].ToString()),
                        descContent[4].Substring(descContent[4].IndexOf("、") + 1).Replace("\r", ""),
                        drData["LetterDeptName"].ToString()
                        );

                    // 產生實體檔案
                    WriteFile(filePath, strFileName + ".di", diContent);

                    _m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.Information, "已產生DI, SW檔");

                    #endregion

                }

                #endregion

                #region 第1個Sheet與第2個Sheeet資料來源

                if (dtSheetData != null && dtSheetData.Rows.Count > 0)
                {
                    mainDataSource = new List<ReportDataSource>();
                    mainDataSource.Add(new ReportDataSource("dtSheet1", dtSheetData));

                    string strFileNameOne = strFileName + "_002";

                    try
                    {
                        SavePDFByLocalReport("RptRecieve01.rdlc", strFileNameOne, mainDataSource);
                        _m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.Information, "產生PDF文件" + strFileNameOne);
                    }
                    catch (Exception ex)
                    {
                        _m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.Information, "產生PDF文件發生錯誤" + strFileNameOne + "  " + ex.Message.ToString());
                    }

                    strListFileName.Add(filePath + @"\" + strFileNameOne + ".pdf");
                }

                if (dtSheetTwoDetailData != null && dtSheetTwoDetailData.Rows.Count > 0)
                {
                    mainDataSource = new List<ReportDataSource>();
                    mainDataSource.Add(new ReportDataSource("dtSheet2", dtSheetTwoDetailData));

                    string strFileNameOne = strFileName + "_003";

                    try
                    {
                        SavePDFByLocalReport("RptRecieve02.rdlc", strFileNameOne, mainDataSource);
                        _m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.Information, "產生PDF文件" + strFileNameOne);
                    }
                    catch (Exception ex)
                    {
                        _m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.Information, "產生PDF文件發生錯誤" + strFileNameOne + "  " + ex.Message.ToString());
                    }

                    strListFileName.Add(filePath + @"\" + strFileNameOne + ".pdf");
                }

                // 20211206, 產生保管箱檔...
                if( dtSBoxData!=null && dtSBoxData.Rows.Count >0 )
                {
                    mainDataSource = new List<ReportDataSource>();
                    mainDataSource.Add(new ReportDataSource("dtSheet3", dtSBoxData));
                    string strFileNameOne = strFileName + "_004";

                    try
                    {
                        SavePDFByLocalReport("RptRecieve03.rdlc", strFileNameOne, mainDataSource);
                        _m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.Information, "產生PDF文件" + strFileNameOne);
                    }
                    catch (Exception ex)
                    {
                        _m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.Information, "產生PDF文件發生錯誤" + strFileNameOne + "  " + ex.Message.ToString());
                    }

                    strListFileName.Add(filePath + @"\" + strFileNameOne + ".pdf");
                }


                #endregion  第1個Sheet與第2個Sheeet資料來源



                #region 產生最終使用PDF文件


                try
                {
                    mergePDFFiles(filePath + @"\" + strFileName + ".pdf", strListFileName);
                    _m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.Information, "合併PDF文件發生成功" + filePath + @"\" + strFileName + ".pdf");
                    // 刪除多餘文件
                    foreach (string fileName in strListFileName)
                    {
                        FileInfo fileInfo = new FileInfo(fileName);

                        // 留著首頁
                        if (fileName.Contains("_001.pdf"))
                        {
                            continue;
                        }

                        if (fileInfo.Exists)
                        {
                            //fileInfo.Delete();
                        }
                    }
                }
                catch (Exception ex)
                {                    
                    _m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.Information, "合併PDF文件發生錯誤" + ex.Message.ToString());
                }
                #endregion  產生最終使用PDF文件





                _m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.Information, "--------------------------[存款資料及存款往來明細資料]列印結束--------------------------");
            }
            catch (Exception ex)
            {
                _m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.Information, "[存款資料及存款往來明細資料]列印報錯: " + ex.Message);
                strReturn = "N";
            }

            return strReturn;
        }

        private DataTable reOrderJRNL(DataTable dt)
        {
            //20220504 , 重新編寫JRNL_NO, group by Account_NO, TRAN_DATE , 由1開始...

            var temp1 = dt.Select("( JRNL_NO like '此區間無交易往來明細%' OR JRNL_NO like '%與本行無存款往來%' OR JRNL_NO like '查核區間日期有誤%' OR  JRNL_NO like '貴單位來函提供之帳號%')");
            var temp2 = dt.Select("NOT ( JRNL_NO like '此區間無交易往來明細%' OR JRNL_NO like '%與本行無存款往來%' OR JRNL_NO like '查核區間日期有誤%' OR  JRNL_NO like '貴單位來函提供之帳號%')");

            var grp1 = temp2.GroupBy(r => new { a = r["ACCT_NO"].ToString(), d = r["TRAN_DATE"].ToString() })
                   .Select(g => new { a = g.Key.a, d = g.Key.d }).ToList();                   

            //var grp =(from c in drRecvData
            //    group c by new {c.ACCT_NO, c.TRAN_DATE } into g
            //    select new { a= g.Key.ACCT_NO, d= g.Key.TRAN_DATE }).ToList();            
            //var grp = dt.AsEnumerable()
            //       .GroupBy(r => new { a = r["ACCT_NO"].ToString(), d = r["TRAN_DATE"].ToString() })
            //       .Select(g => new { a = g.Key.a, d = g.Key.d }).ToList();                   
            
            foreach (var g in grp1)
            {
                //var samegroup = drRecvData.Where(x => x.ACCT_NO == g.a && x.TRAN_DATE == g.d).OrderBy(y => y.JNRST_TIME).ToList();
                int seq = 1;
                //var ddd = dt.AsEnumerable().Where(x => x["TRAN_DATE"].ToString() == g.d).ToList();
                //var lst = dt.AsEnumerable().Where(x => x["ACCT_NO"].ToString() == g.a && x["TRAN_DATE"].ToString() == g.d).OrderBy(y => y["JNRST_TIME"].ToString()).ToList();
                //int iCount = dt.Rows.Count;
                foreach (var s in temp2.Where(x => x["ACCT_NO"].ToString() == g.a && x["TRAN_DATE"].ToString() == g.d).OrderBy(y => y["JNRST_TIME"].ToString()))
                {
                    s["JRNL_NO"] = (seq++).ToString().PadLeft(8, '0');
                }
            }

            //var debug = dt.AsEnumerable().Where(x=> x["TRAN_DATE"].ToString().Trim() == "20180226")
            DataTable Result = new DataTable();
            if (temp2.Count() > 0)
            {
                Result = temp2.OrderBy(x => x["ACCT_NO"].ToString()).ThenBy(y => y["TRAN_DATE"].ToString()).ThenBy(z => z["JRNL_NO"].ToString()).CopyToDataTable();
            }
            else
            {
                Result = dt.Clone();
            }




            foreach (var dr in temp1)
            {                
                var drCopy = Result.NewRow();
                drCopy["IdNo"] = dr["IdNo"];
                drCopy["JRNL_NO"] = dr["JRNL_NO"];
                drCopy["ACCT_NO"] = dr["ACCT_NO"];
                drCopy["CustIdNo"] = dr["CustIdNo"];
                drCopy["GroupId"] = dr["CustIdNo"].ToString() + dr["ACCT_NO"].ToString();
                Result.Rows.Add(drCopy);
            }

            return Result;
        }

        private DataTable GetSbox(string strNewID)
        {
            DataTable dtReturn = new DataTable();

            try
            {
                // 20180201,PeterHsieh:查無資料也要顯示

                string sqlSelect = @"select  CUST_ID_NO, RENT_BRANCH, RENT_KIND, CUSTOMER_NAME, NIGTEL_NO, MOBIL_NO, COMM_ADDR, CUST_ADD, BOX_NO, MODIFIED_DATE, RENT_START, RENT_END, MEMO
                    from CaseCustOutputF3 as F3
                    where F3.MasterId=@NewID";


                // 清空容器
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@NewID", strNewID));

                dtReturn = base.Search(sqlSelect);



                // 20220223, 發現, 無交易往來, 沒有輸出到PDF, 所以要補到dtReturn 這個Table 中...
                string sql2 = @"select * from CaseCustDetails where casecustmasterid=@MasterID and querytype in (5)
 and detailsid not in (select distinct detailsid from  CaseCustOutputF3 where masterid=@MasterID)";



                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@MasterID", strNewID));

                var dtReturn2 = base.Search(sql2);
                if (dtReturn2.Rows.Count > 0)
                {
                    foreach (DataRow dr in dtReturn2.Rows)
                    {
                        // 補上...  身分證統一編號 + '與本行無存款往來'

                        var newRow = dtReturn.NewRow();
                        newRow[0] = dr["CustIdNo"]; ; newRow[1] = ""; newRow[2] = ""; newRow[3] = ""; newRow[4] = ""; newRow[5] = ""; newRow[6] = "";
                        newRow[7] = ""; newRow[8] = ""; newRow[9] = ""; newRow[10] = ""; newRow[11] = ""; newRow[12] = "查詢對象於本行無保管箱(室)承租資料"; 
                        dtReturn.Rows.Add(newRow);

                    }
                }



            }
            catch (Exception ex)
            {
                _m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.Information, "查詢[存款基本資料]報錯:" + ex.Message);
                throw ex;
            }

            return dtReturn;
        }



        /// <summary>
        /// 子報表添加數據源
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LocalReport_SubreportProcessing(object sender, SubreportProcessingEventArgs e)
        {
            foreach (var reportDataSource in subDataSource)
            {
                e.DataSources.Add(reportDataSource);
            }
        }

        #endregion  匯出報表

        #region 匯出PDF的自定義方法

        /// <summary>
        /// 通過rdlc報表產生PDF文件
        /// </summary>
        /// <param name="pReportName">報表名稱</param>
        /// <param name="pFileName">文件名稱</param> 
        public void SavePDFByLocalReport(string pReportName, string pFileName, List<ReportDataSource> mainDataSource, List<ReportParameter> listParm = null)
        {
            try
            {
                LocalReport localReport = null;

                localReport = new LocalReport();
                localReport.ReportPath = Application.StartupPath + @"\Template\" + pReportName;

                // 當有參數時, 報表添加參數
                if (listParm != null && listParm.Count > 0)
                {
                    localReport.SetParameters(listParm); //*添加參數
                }

                // 添加數據源,可以多個
                if (mainDataSource != null && mainDataSource.Count > 0)
                {
                    foreach (var reportDataSource in mainDataSource)
                    {
                        localReport.DataSources.Add(reportDataSource);
                    }
                }

                Warning[] warnings;
                string[] streams;
                string mimeType;
                string encoding;
                string fileNameExtension;
                string fileName = pFileName;

                var renderedBytes = localReport.Render("PDF",
                    null,
                    out mimeType,
                    out encoding,
                    out fileNameExtension,
                    out streams,
                    out warnings);
                localReport.Dispose();

                using (FileStream fs = new FileStream(filePath + @"\" + fileName + ".pdf", FileMode.Create, FileAccess.Write))
                {
                    fs.Write(renderedBytes, 0, renderedBytes.Length);
                }
            }
            catch (Exception ex)
            {
                _m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.Information, "LocalReport 產生 PDF文件報錯: " + ex.Message);
                throw ex;
            }
        }

        /// <summary>
        /// 將多個PDF文件合併為一個PDF
        /// </summary>
        /// <param name="outMergeFile">outMergeFile是pdf文件合并后的输出路径</param>
        /// <param name="lstFile">lstFile里存放要进行合并的pdf文件的路径</param>
        public void mergePDFFiles(string outMergeFile, List<string> lstFile)
        {
            if (!string.IsNullOrEmpty(outMergeFile))
            {
                try
                {
                    PdfReader reader;
                    Document document = new Document();

                    // 創建pdf文檔
                    PdfWriter writer = PdfWriter.GetInstance(document, new FileStream(outMergeFile, FileMode.Create));

                    document.Open();
                    PdfContentByte cb = writer.DirectContent;
                    PdfImportedPage newPage;

                    // 循環需要合併的pdf文件
                    for (int i = 0; i < lstFile.Count; i++)
                    {
                        // 去的pdf路徑
                        string newpath = lstFile[i];
                        reader = new PdfReader(newpath);

                        // 計算pdf文件的頁數
                        int iPageNum = reader.NumberOfPages;
                        int startPage = 1;
                        int rotation;

                        // 從第一頁開始倒入文件數據
                        while (startPage <= iPageNum)
                        {
                            document.SetPageSize(reader.GetPageSizeWithRotation(startPage));
                            document.NewPage();
                            newPage = writer.GetImportedPage(reader, startPage);

                            // 獲取每一頁pdf文件的rotation 
                            rotation = reader.GetPageRotation(startPage);

                            // 根據每一頁的rotation重置寬高，否則都按首頁寬高合併可能會造成信息丟失
                            switch (rotation)
                            {
                                case 90:
                                    cb.AddTemplate(newPage, 0, -1f, 1f, 0, 0, reader.GetPageSizeWithRotation(startPage).Height);
                                    break;

                                case 180:
                                    cb.AddTemplate(newPage, -1f, 0, 0, -1f, reader.GetPageSizeWithRotation(startPage).Width, reader.GetPageSizeWithRotation(startPage).Height);
                                    break;

                                case 270:
                                    cb.AddTemplate(newPage, 0, 1f, -1f, 0, reader.GetPageSizeWithRotation(startPage).Width, 0);
                                    break;

                                default:
                                    cb.AddTemplate(newPage, 1f, 0, 0, 1f, 0, 0);
                                    break;
                            }

                            startPage++;
                        }
                    }

                    document.Close();

                    // 20220106, 發現因為會咬住.. 002.PDF 003.PDF 或004.PDF 造成無法砍掉, 所以去讀001.pdf (因為函文., 不需要砍掉)
                    var file001 = lstFile.Where(x => x.EndsWith("001.pdf")).FirstOrDefault();
                    reader = new PdfReader(file001);
                    reader.Close();
                    
                }
                catch (Exception ex)
                {
                    outMergeFile = string.Empty;
                    throw ex;
                }
            }
        }

        #endregion 匯出PDF的自定義方法

        #region 獲取匯出檔案需要資料
        /// <summary>
        /// 查詢HTG回文[存款基本資料]
        /// </summary>
        /// <param name="pDocNo"></param>
        /// <returns></returns>
        public DataTable Get401RecvData(string parCaseCustNewID)
        {
            DataTable dtReturn = new DataTable();

            try
            {
                // 20180201,PeterHsieh:查無資料也要顯示
                string sqlSelect = @"select  MasterId, d.Idno, F1.Cust_Id_No as CUST_ID_NO,m.ROpenFileName,m.docNo,
	                    CONVERT(varchar(100),d.HTGModifiedDate, 111) as HTGQDateS,
	                    CASE 
		                    WHEN ISNULL(F1.BRANCH_NO,'') <> '' THEN F1.BRANCH_NO
		                    ELSE '與本行無存款往來'
	                    END BRANCH_NO, --分行別
	                    F1.PD_TYPE_DESC, --產品別
	                    F1.CURRENCY,  --幣別
	                    F1.CUSTOMER_NAME, --戶名
	                    NIGTEL_NO, --住家電話
	                    MOBIL_NO, --手機號碼
	                    COMM_ADDR, --通訊地址
	                    CUST_ADD,--戶籍/證照地址
	                    ACCT_NO, --帳號
	                    OPEN_DATE, --開戶日
	                    CLOSE_DATE, --結清日
	                    CUR_BAL --目前餘額
                    from CaseCustOutputF1 as F1
                    inner join CaseCustDetails d on f1.DetailsId=d.detailsId
                    inner join CaseCustMaster m on f1.MasterId = m.NewID

                    where F1.MasterId=@NewID AND d.QueryType in ('1','3')";


                // 清空容器
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@NewID", parCaseCustNewID));

                dtReturn = base.Search(sqlSelect);




                // 20220223, 發現, 無交易往來, 沒有輸出到PDF, 所以要補到dtReturn 這個Table 中...
                string sql2 = @"select * from CaseCustDetails where casecustmasterid=@MasterID and querytype in (1,3)
 and detailsid not in (select distinct detailsid from  CaseCustOutputF1 where masterid=@MasterID)";

                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@MasterID", parCaseCustNewID));

                var dtReturn2 = base.Search(sql2);
                if( dtReturn2.Rows.Count > 0)
                {
                    foreach(DataRow dr in dtReturn2.Rows)
                    {
                        // 補上...  身分證統一編號 + '與本行無存款往來'
                        if (dr["QueryType"].ToString() == "1")
                        {
                            var newRow = dtReturn.NewRow();
                            newRow["MasterId"] = dr["CaseCustMasterId"];
                            newRow["Idno"] = dr["Idno"];
                            newRow["CUST_ID_NO"] = dr["CustIdNo"];
                            newRow[3] = ""; newRow[4] = ""; newRow[5] = ""; newRow[6] = "查詢對象於本行無開戶資料";
                            newRow[7] = ""; newRow[8] = ""; newRow[9] = ""; newRow[10] = ""; newRow[11] = ""; newRow[12] = ""; newRow[13] = ""; newRow[14] = ""; newRow[15] = ""; newRow[15] = "";
                            dtReturn.Rows.Add(newRow);
                        }
                        if (dr["QueryType"].ToString() == "3")
                        {
                            var newRow = dtReturn.NewRow();
                            newRow["MasterId"] = dr["CaseCustMasterId"];
                            newRow["Idno"] = dr["Idno"];
                            newRow["CUST_ID_NO"] = "";
                            newRow[3] = ""; newRow[4] = ""; newRow[5] = ""; newRow[6] = "所查詢之帳號非本行帳號";
                            newRow[7] = ""; newRow[8] = ""; newRow[9] = ""; newRow[10] = ""; newRow[11] = ""; newRow[12] = ""; newRow[13] = ""; newRow[14] = dr["CustAccount"]; newRow[15] = ""; newRow[15] = "";
                            //templae = templae.Replace("ID", "").Replace("ACC", d.CustAccount).Replace("MEMO", "所查詢之帳號非本行帳號");
                            dtReturn.Rows.Add(newRow);
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                _m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.Information, "查詢[存款基本資料]報錯:" + ex.Message);
                throw ex;
            }

            return dtReturn;
        }

        #region 存款明細表共用Sql(共用資料來源)
        // 存款明細表共用Sql(共用資料來源)


        #endregion 存款明細表共用Sql(共用資料來源)

        /// <summary>
        /// 查詢HTG回文[存款往來明細資料]
        /// </summary>
        /// <param name="pDocNo"></param>
        /// <returns></returns>
        public DataTable GetRFDMRecvData(string parCaseCustNewID)
        {
            DataTable dtReturn = new DataTable();

            try
            {
                // 20180201,PeterHsieh:查無資料也要顯示
                /*
                 * 20180718,PeterHsieh : 配合 CR處理，修改 ATM或端末機代號欄位取得來源
                 *      (ATM.交易日期 = RFDM.交易日期 and ATM.交易序號 = RFDM.交易序號 and ATM.交易時間 = RFDM.交易時間(前 6bytes)
                 * 
                 * 20180725,PeterHsieh : 調整如下
                 *      01.交易序號 : 改取 CaseCustRFDMRecv.FISC_BANK(金資序號)，但若此欄位為空或'00000000'時，則取 CaseCustRFDMRecv.JRNL_NO(8 bytes)
                 *      02.ATM或端末機代號 : 修改 Join條件
                 *          (ATM.交易日期 = RFDM.交易日期 and ('0'+ATM.交易序號) = RFDM.金資序號)
                 */


                #region 原SQL 
//                string sqlSelect = @"
//   with 
//cr as
// ( 
//                                     SELECT 
//	                                    (Cast(IdNo as varchar)
//	                                     + CustIdNo
//	                                     + ACCT_NO
//	                                     + PD_TYPE_DESC
//	                                     + ISNULL(CURRENCY, '')
//	                                     + ISNULL(QDateS, '')
//	                                     + ISNULL(QDateE, '')
//	                                     + TELLER
//	                                    ) AS GroupId
//	                                    ,*
//                                    FROM (
//	                                    select
//		                                    CaseCustQueryVersion.CaseCustNewID
//		                                    ,CaseCustQueryVersion.IdNo
//		                                    ,CaseCustQueryVersion.CustIdNo
//		                                    ,CaseCustQuery.RFileTransactionFileName
//		                                    ,CaseCustQuery.DocNo
//		                                    ,CASE
//		                                    WHEN LEN(CaseCustQueryVersion.QDateS) > 0 
//			                                    and CaseCustQueryVersion.QDateS <> ''
//			                                    THEN CONVERT(NVARCHAR(10), CONVERT(DATETIME, CaseCustQueryVersion.QDateS), 111) ELSE '' 
//		                                    END AS QDateS
//		                                    ,CASE
//			                                    WHEN LEN(CaseCustQueryVersion.QDateE) > 0 
//				                                    and CaseCustQueryVersion.QDateE <> ''
//			                                    THEN CONVERT(NVARCHAR(10), CONVERT(DATETIME, CaseCustQueryVersion.QDateE), 111) ELSE '' 
//		                                    END AS QDateE
//		                                    ,ISNULL(BOPS067050Recv.CUSTOMER_NAME, '') AS CUSTOMER_NAME --戶名
//		                                    ,ISNULL(CaseCustRFDMRecv.ACCT_NO, CaseCustQueryVersion.CustIdNo) AS ACCT_NO --帳號	X(20)
//		                                    ,CASE
//			                                    WHEN ISNULL(JRNL_NO,'') = ''
//			                                    THEN ''
//			                                    ELSE isnull(
//                                            (select top 1 PARMCode.CodeNo+','+PARMCode.CodeDesc
//                                                 from PARMCode
//                                                 where PARMCode.CodeType = 'PD_TYPE_DESC'
//                                                 and PARMCode.CodeMemo = BOPS000401Recv.PD_TYPE_DESC
//                                                 order by PARMCode.CodeNo
//                                            ),'99,其它') 
//		                                    END as PD_TYPE_DESC --產品別
//		                                    ,ISNULL(BOPS000401Recv.CURRENCY, '') AS CURRENCY  --幣別
//		                                    ,CaseCustQuery.QueryUserID as TELLER --櫃員代號	X(20)
//		                                    ,CASE WHEN ISNULL(JRNL_NO,'') = ''
//			                                    THEN '此區間無交易往來明細'
//			                                    ELSE 
//                                	                CASE WHEN ISNULL(CaseCustRFDMRecv.FISC_SEQNO, '') = '' OR (CaseCustRFDMRecv.FISC_SEQNO = '00000000')
//                                                        THEN RIGHT('00000000'+JRNL_NO,8)
//                                                        ELSE CaseCustRFDMRecv.FISC_SEQNO
//                                                    END
//		                                    END as JRNL_NO--交易序號	9(08)
//		                                    ,CASE
//			                                    WHEN ISNULL(TRAN_DATE, '') = '' 
//			                                    THEN ''
//			                                    ELSE CONVERT(NVARCHAR(10), TRAN_DATE, 111)
//		                                    END as TRAN_DATE--交易日期	X(08)
//		                  	,( select top 1 CASE WHEN ISNULL(CaseCustATMRecv.[YBTXLOG_TXN_HHMMSS],'') = ''  THEN '' ELSE SUBSTRING(CaseCustATMRecv.[YBTXLOG_TXN_HHMMSS],1,2)+':'+SUBSTRING(CaseCustATMRecv.[YBTXLOG_TXN_HHMMSS],3,2)+':'+SUBSTRING(CaseCustATMRecv.[YBTXLOG_TXN_HHMMSS],5,2) END
//                                  from CaseCustATMRecv
//                                       where CaseCustATMRecv.[YBTXLOG_YYYYMMDD] = CONVERT(Varchar, CaseCustRFDMRecv.TRAN_DATE, 112)
//                                       and ('0'+CaseCustATMRecv.[YBTXLOG_STAND_NO]) = CaseCustRFDMRecv.FISC_SEQNO
//                                       order by CaseCustATMRecv.CreatedDate DESC
//                                    ) as ATM_TIME
//											,CASE
//											  WHEN ISNULL(JNRST_TIME, '') = ''
//												THEN ''
//												ELSE 
//												  CASE WHEN (JNRST_TIME = JRNL_NO OR JNRST_TIME = CONVERT(varchar, JNRST_DATE, 112))
//													THEN ''
//													ELSE
//													  CASE WHEN TRY_CONVERT(datetime, SUBSTRING(JNRST_TIME,1,2)+':'+SUBSTRING(JNRST_TIME,3,2)+':'+SUBSTRING(JNRST_TIME,5,2)) IS NULL
//														THEN ''
//														ELSE SUBSTRING(JNRST_TIME,1,2)+':'+SUBSTRING(JNRST_TIME,3,2)+':'+SUBSTRING(JNRST_TIME,5,2)
//													  END
//												  END
//											END AS JNRST_TIME --交易時間	X(06)
//		                                    ,CASE 
//			                                    WHEN isnull(TRAN_BRANCH,'') <> '' THEN '822' + isnull(TRAN_BRANCH,'') 
//			                                    ELSE ''
//		                                    END TRAN_BRANCH --交易行(或所屬分行代號)	X(07)
//		                                    ,isnull(TXN_DESC,'') as TXN_DESC--交易摘要	X(40)
//		                                    ,ISNULL(TRAN_AMT, 0) AS TRAN_AMT --支出金額	X(16)
//		                                    ,ISNULL(TRAN_AMT, 0) AS SaveAMT--存入金額	X(16)
//		                                    ,ISNULL(BALANCE, 0) AS BALANCE --餘額	X(16)
//                                	         ,(
//											   select top 1 CaseCustATMRecv.[YBTXLOG_SAFE_TMNL_ID]
//											   from CaseCustATMRecv
//											   where CaseCustATMRecv.[YBTXLOG_YYYYMMDD] = CONVERT(Varchar, CaseCustRFDMRecv.TRAN_DATE, 112)
//											   and ('0'+CaseCustATMRecv.[YBTXLOG_STAND_NO]) = CaseCustRFDMRecv.FISC_SEQNO
//											   order by CaseCustATMRecv.CreatedDate DESC
//                                             ) as ATM_NO --ATM或端末機代號	X(20)
//		                                    ,CASE
//			                                    WHEN CAST(isnull(TRF_ACCT,'0') AS NUMERIC) = 0
//			                                    THEN ''
//			                                    ELSE replace(replace(isnull(TRF_BANK,''),'448','822'),'000','822') + isnull(TRF_ACCT,'')
//		                                    END as TRF_BANK --轉出入行庫代碼及帳號 (RFDM)TRF_BANK+TRF_ACCT
//		                                    ,isnull(NARRATIVE,'') as NARRATIVE  --備註 (RFDM) NARRATIVE
//		                                    ,CONVERT(NVARCHAR(10), GETDATE(), 111) AS PrintDate --列印日期
//	                                    from 
//		                                    CaseCustQuery
//	                                    inner join
//		                                    CaseCustQueryVersion
//	                                    on CaseCustQueryVersion.CaseCustNewID = CaseCustQuery.NewID
//	                                    left join
//		                                    CaseCustRFDMRecv
//	                                    on CaseCustRFDMRecv.VersionNewID = CaseCustQueryVersion.NewID
//	                                    left join
//		                                    BOPS067050Recv
//	                                    on BOPS067050Recv.VersionNewID = CaseCustQueryVersion.NewID
//	                                        and BOPS067050Recv.CUST_ID_NO = CaseCustQueryVersion.CustIdNo
//	                                    left join
//		                                    BOPS000401Recv
//	                                    on BOPS000401Recv.VersionNewID = CaseCustRFDMRecv.VersionNewID
//	                                        and BOPS000401Recv.ACCT_NO = CaseCustRFDMRecv.ACCT_NO
//	                                    where 
//		                                    CaseCustQueryVersion.Status <> '99'
//                                            and CaseCustQueryVersion.TransactionFlag = 'Y'
//                                            and  CaseCustQueryVersion.RFDMSendStatus = '99'
//		                                    and CaseCustQueryVersion.CaseCustNewID = @CaseCustNewID
//                                    ) as temp
//  )
//
//select GroupId,IdNo,CustIdNo,RFileTransactionFileName,DocNo,QDateS,QDateE,CUSTOMER_NAME,ACCT_NO,PD_TYPE_DESC,CURRENCY,TELLER, 
//JRNL_NO,TRAN_DATE,TRAN_BRANCH,TXN_DESC,TRAN_AMT,SaveAMT,BALANCE,ATM_NO,TRF_BANK,NARRATIVE ,PrintDate, 
//(CASE WHEN ISNULL(ATM_TIME,'')<>'' THEN ATM_TIME
//              ELSE JNRST_TIME END) as JNRST_TIME from cr ORDER BY GroupId,TRAN_DATE,JNRST_TIME,JRNL_NO                                
//
//                                ";

//                // 清空容器
//                base.Parameter.Clear();
//                base.Parameter.Add(new CommandParameter("@CaseCustNewID", parCaseCustNewID));

                #endregion


                #region 第二版SQL
                //                string sqlSelect = @"Select 'GroupId' as GroupId, d.IdNo, d.CustIdNo, m.RFileTransactionFileName,m.DocNo,
                //	CASE
                //	WHEN LEN(d.QDateS) > 0 
                //		and d.QDateS <> ''
                //		THEN CONVERT(NVARCHAR(10), CONVERT(DATETIME, d.QDateS), 111) ELSE '' 
                //	END AS QDateS
                //	,CASE
                //		WHEN LEN(d.QDateE) > 0 
                //			and d.QDateE <> ''
                //		THEN CONVERT(NVARCHAR(10), CONVERT(DATETIME, d.QDateE), 111) ELSE '' 
                //	END AS QDateE,
                //	ISNULL(f2.CUSTOMER_NAME, '') AS CUSTOMER_NAME --戶名	
                //	,ISNULL(f2.ACCT_NO, d.CustIdNo) AS ACCT_NO --帳號	X(20)
                //	,f2.PD_TYPE_DESC --產品別
                //	,ISNULL(f2.CURRENCY, '') AS CURRENCY  --幣別
                //	,m.QueryUserID as TELLER --櫃員代號	X(20)
                //	,f2.JRNL_NO as JRNL_NO --交易序號	9(08)
                //	,f2.TRAN_DATE --交易日期	X(08)
                //	,f2.JNRST_TIME --交易時間	X(06)
                //	,f2.TRAN_BRANCH --交易行(或所屬分行代號)	X(07)
                //	,isnull(TXN_DESC,'') as TXN_DESC--交易摘要	X(40)
                //--	,ISNULL(TRAN_AMT, 0) AS TRAN_AMT --支出金額	X(16)
                //--	,ISNULL(TRAN_AMT, 0) AS SaveAMT--存入金額	X(16)
                //--	,ISNULL(BALANCE, 0) AS BALANCE --餘額	X(16)
                //	,CAST(ISNULL(TRAN_AMT, 0) AS DECIMAL(18,2)) AS TRAN_AMT --支出金額	X(16)
                //	,CAST(ISNULL(TRAN_AMT, 0) AS DECIMAL(18,2)) AS SaveAMT--存入金額	X(16)
                //	,CAST(ISNULL(BALANCE, 0) AS DECIMAL(18,2)) AS BALANCE --餘額	X(16)
                //	,f2.ATM_NO --ATM或端末機代號	X(20)
                //	,f2.TRF_BANK --轉出入行庫代碼及帳號 (RFDM)TRF_BANK+TRF_ACCT
                //	,isnull(NARRATIVE,'') as NARRATIVE  --備註 (RFDM) NARRATIVE
                //	,CONVERT(NVARCHAR(10), GETDATE(), 111) AS PrintDate --列印日期
                //	
                //from CaseCustOutputF2 f2 inner join CaseCustDetails d on f2.Detailsid = d.DetailsId
                //	inner join CaseCustMaster m on m.NewID = f2.MasterId
                //where f2.MasterId=@NewID"; 
                #endregion



                string sqlSelect = @"with cr as (
	Select  d.IdNo, d.CustIdNo, m.RFileTransactionFileName,m.DocNo,
	CASE
	WHEN LEN(d.QDateS) > 0 
		and d.QDateS <> ''
		THEN CONVERT(NVARCHAR(10), CONVERT(DATETIME, d.QDateS), 111) ELSE '' 
	END AS QDateS
	,CASE
		WHEN LEN(d.QDateE) > 0 
			and d.QDateE <> ''
		THEN CONVERT(NVARCHAR(10), CONVERT(DATETIME, d.QDateE), 111) ELSE '' 
	END AS QDateE,
	ISNULL(f2.CUSTOMER_NAME, '') AS CUSTOMER_NAME --戶名	
	,ISNULL(f2.ACCT_NO, d.CustIdNo) AS ACCT_NO --帳號	X(20)
	,f2.PD_TYPE_DESC --產品別
	,ISNULL(f2.CURRENCY, '') AS CURRENCY  --幣別
	,m.QueryUserID as TELLER --櫃員代號	X(20)
	,f2.JRNL_NO as JRNL_NO --交易序號	9(08)
	,f2.TRAN_DATE --交易日期	X(08)
	,f2.JNRST_TIME --交易時間	X(06)
	,f2.TRAN_BRANCH --交易行(或所屬分行代號)	X(07)
	,isnull(TXN_DESC,'') as TXN_DESC--交易摘要	X(40)
--	,ISNULL(TRAN_AMT, 0) AS TRAN_AMT --支出金額	X(16)
--	,ISNULL(TRAN_AMT, 0) AS SaveAMT--存入金額	X(16)
--	,ISNULL(BALANCE, 0) AS BALANCE --餘額	X(16)
	,CAST(ISNULL(TRAN_AMT, 0) AS DECIMAL(18,2)) AS TRAN_AMT --支出金額	X(16)
	,CAST(ISNULL(TRAN_AMT, 0) AS DECIMAL(18,2)) AS SaveAMT--存入金額	X(16)
	,CAST(ISNULL(BALANCE, 0) AS DECIMAL(18,2)) AS BALANCE --餘額	X(16)
	,f2.ATM_NO --ATM或端末機代號	X(20)
	,f2.TRF_BANK --轉出入行庫代碼及帳號 (RFDM)TRF_BANK+TRF_ACCT
	,isnull(NARRATIVE,'') as NARRATIVE  --備註 (RFDM) NARRATIVE
	,CONVERT(NVARCHAR(10), GETDATE(), 111) AS PrintDate --列印日期
	
from CaseCustOutputF2 f2 inner join CaseCustDetails d on f2.Detailsid = d.DetailsId
	inner join CaseCustMaster m on m.NewID = f2.MasterId
where f2.MasterId=@NewID
)
select (Cast(IdNo as varchar) + CustIdNo + ACCT_NO + ISNULL(PD_TYPE_DESC, '') + ISNULL(CURRENCY, '')) AS GroupId,* from cr;";



                // 清空容器
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@NewID", parCaseCustNewID));

                dtReturn = base.Search(sqlSelect);
                //dtReturn = base.Search(strCaseDetailCommonSql + sqlSelect);





                // 20220223, 發現, 無明細, 沒有輸出到PDF, 所以要補到dtReturn 這個Table 中...
 
                string sql2 = @"select * from CaseCustDetails where casecustmasterid=@MasterID and querytype in (2,4)
 and detailsid not in (select distinct detailsid from  CaseCustOutputF2 where masterid=@MasterID)";

                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@MasterID", parCaseCustNewID));

                var dtReturn2 = base.Search(sql2);
                if (dtReturn2.Rows.Count > 0)
                {
                    int i = 0;
                    foreach (DataRow dr in dtReturn2.Rows)
                    {
                        // 20230103, 要判斷.. 未開戶跟區間無往來.. 所以呼叫method來判斷... 因此呼叫CheckCustomer 方法....而不是寫死
                        string strMessage = CheckCustomer(dr["DetailsId"].ToString());
                        string errMessage = dr["ErrorMessage"].ToString().Trim();


                        if (!string.IsNullOrEmpty(errMessage) && (errMessage.Contains("查核區間日期有誤") || errMessage.Contains("存款帳號碼數不符")))
                        {
                            if (dr["QueryType"].ToString() == "2" && errMessage.Contains("查核區間日期有誤"))
                                strMessage = "查核區間日期有誤。";
                            if (dr["QueryType"].ToString() == "4" && errMessage.Contains("存款帳號碼數不符"))
                                strMessage = "貴單位來函提供之帳號與本行存款帳號碼數不符(本行帳號碼數為12碼), 故無法提供相關資料。";
                            if (dr["QueryType"].ToString() == "4" &&  errMessage.Contains("查核區間日期有誤"))
                                if (strMessage.Contains("貴單位來函提供"))
                                {
                                    strMessage += "; 查核區間日期有誤。";
                                }
                                else
                                    strMessage = "查核區間日期有誤。";
                        }


                        // 補上...  身分證統一編號 + '與本行無存款往來'
                        if (string.IsNullOrEmpty(dr["CustAccount"].ToString().Trim()))
                            dr["CustAccount"] = dr["CustIdNo"];
                        if (string.IsNullOrEmpty(dr["CustIdNo"].ToString().Trim()))
                            dr["CustIdNo"] = dr["CustAccount"];
                        var newRow = dtReturn.NewRow();
                        newRow[0] = (i++).ToString();
                        newRow[1] = 0;
                        newRow[2] = dr["CustIdNo"];
                        newRow[3] = ""; newRow[4] = ""; newRow[5] = ""; newRow[6] = "";
                        newRow[7] = ""; newRow[8] = dr["CustAccount"]; newRow[9] = ""; newRow[10] = ""; newRow[11] = ""; newRow[12] = strMessage; newRow[13] = ""; newRow[14] = ""; newRow[15] = ""; newRow[15] = "";
                        newRow[16] = ""; newRow[17] = 0.0; newRow[18] = 0.0; newRow[19] = 0.0; newRow[20] = ""; newRow[21] = ""; newRow[22] = ""; newRow[23] = ""; 

                        dtReturn.Rows.Add(newRow);
                        
                    }
                }








            }
            catch (Exception ex)
            {
                _m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.Information, "查詢[存款交易明細資料]報錯:" + ex.Message);
                throw ex;
            }

            return dtReturn;
        }

        /// <summary>
        /// 判斷是'未開戶'或'無查詢區間資料'
        /// </summary>
        /// <returns></returns>
        public string CheckCustomer(string VersionNewID)
        {
            string desc = string.Empty;


            try
            {
                // 20230103, 判斷方法, 去讀401Send中的QueryErrMsg , 若有.. 檢查碼錯誤... 
                //            QueryErrMsg
                //提示代碼:0169 提示原因:檢查碼錯誤 

                string sqlSelect = @"
                                SELECT count(*)
                                from BOPS000401Send
                                where BOPS000401Send.VersionNewID = @VersionNewID
                                AND QueryErrMsg like '%提示代碼:0169%'
                              ";


                // 清空容器
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@VersionNewID", VersionNewID));


                DataTable dtResult = base.Search(sqlSelect);

                if (dtResult != null && int.Parse(dtResult.Rows[0][0].ToString()) > 0)
                {
                    desc = "與本行無存款往來";
                }
                else
                {
                    desc = "此區間無交易往來明細";

                }

                dtResult = null;
            }
            catch (Exception ex)
            {
                
                _m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------'未開戶'或'無查詢區間資料發生錯誤------" + ex.Message.ToString());
            }

            return desc;
        }



        /// <summary>
        /// 查詢HTG回文[存款往來明細資料]
        /// Group by 戶名、帳號、產品別、幣別獲得筆數
        /// </summary>
        /// <param name="pDocNo"></param>
        /// <returns></returns>
        public DataTable GetRFDMRecvGroupData(string parCaseCustNewID)
        {            DataTable dtReturn = new DataTable();

            try
            {
                string sqlSelect = @"
                                select CUSTOMER_NAME,ACCT_NO,PD_TYPE_DESC,CURRENCY,QDateS,QDateE,TELLER, count(0) AS cnt
                                from tempdata2
                                group by CUSTOMER_NAME,ACCT_NO,PD_TYPE_DESC,CURRENCY,QDateS,QDateE,TELLER ";

                // 清空容器
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@CaseCustNewID", parCaseCustNewID));

                //dtReturn = base.Search(strCaseDetailCommonSql + sqlSelect);
                dtReturn = base.Search(sqlSelect);
            }
            catch (Exception ex)
            {
                _m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.Information, "查詢[存款交易明細資料Group By]報錯:" + ex.Message);
                throw ex;
            }

            return dtReturn;
        }

        /// <summary>
        /// 去掉
        /// </summary>
        /// <param name="caseKind"></param>
        /// <param name="casekind2"></param>
        /// <returns></returns>
        public SendSettingRef GetSendSettingRef(string caseKind, string casekind2)
        {
            string sql = @"SELECT [CaseKind],[CaseKind2],[Subject],[Description] 
                            FROM [SendSettingRef] WHERE [CaseKind] = @CaseKind AND [CaseKind2] = @CaseKind2";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("CaseKind", caseKind));
            Parameter.Add(new CommandParameter("CaseKind2", casekind2));
            IList<SendSettingRef> list = SearchList<SendSettingRef>(sql);
            return list.FirstOrDefault();
        }

        /// <summary>
        ///  表單類型
        /// </summary>
        /// <param name="codeType">代碼類別</param>
        /// <param name="codeNo">代碼</param>
        /// <returns></returns>
        public IList<PARMCode> GetCodeData(string codeType, string codeNo)
        {
            try
            {
                string sql = "";

                sql = @"SELECT 
	                        CodeType
	                        ,CodeTypeDesc 
	                        ,CodeNo  
	                        ,CodeDesc 
	                        ,SortOrder 
	                        ,CodeTag 
	                        ,CodeMemo 
	                        ,Enable 
	                        ,BANCSCode
                        From 
	                        PARMCode
                        WHERE PARMCode.Enable = 1 ";

                string sqlWhere = "";

                // 清空容器
                base.Parameter.Clear();

                // 代碼類別
                if (!string.IsNullOrEmpty(codeType))
                {
                    sqlWhere += " AND CodeType = @CodeType ";
                    base.Parameter.Add(new CommandParameter("@CodeType", codeType));
                }

                // 參數細項代碼
                if (!string.IsNullOrEmpty(codeNo))
                {
                    sqlWhere += " AND CodeNo = @CodeNo ";
                    base.Parameter.Add(new CommandParameter("@CodeNo", codeNo));
                }

                if (sqlWhere != "")
                {
                    sql = sql + sqlWhere;
                }

                string strOrder = "";
                strOrder = " order by SortOrder ";

                if (strOrder != "")
                {
                    sql = sql + strOrder;
                }

                return base.SearchList<PARMCode>(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion 獲取匯出檔案需要資料

        #region 處理報表顯示內容


        #endregion 處理報表顯示內容

        #region 資料格式化相關方法

        /// <summary>
        /// 時間格式化：  hh:MM:ss
        /// </summary>
        /// <param name="pTime"></param>
        /// <returns></returns>
        public string GetTimeFormat(string pTime)
        {
            string strReturn = "";

            if (pTime.Length >= 6)
            {
                strReturn = pTime.Substring(0, 2) + ":"
                    + pTime.Substring(2, 2) + ":" + pTime.Substring(4, 2);
            }

            return strReturn;
        }

        /// <summary>
        /// 金額格式化： 三位一撇
        /// </summary>
        /// <param name="pAMT"></param>
        /// <returns></returns>
        public string GetAMTFormat(string pAMT)
        {
            string strReturn = "";

            if (pAMT.Length > 0)
            {
                strReturn = Convert.ToDecimal(pAMT).ToString("###,##0.000");
            }

            return strReturn;
        }

        #endregion 資料格式化相關方法

        /// <summary>
        /// 回傳 .sw檔案內容
        /// </summary>
        /// <returns>檔案內容樣版</returns>
        string GetSWContentTemplate()
        {
            StringBuilder content = new StringBuilder();

            //20181228 固定變更 update start
            //content.AppendLine("<?xml version=\"1.0\" encoding=\"big5\"?>")
            //       .AppendLine("<!DOCTYPE 交換表單 SYSTEM \"99_roster_big5.dtd\">")
            //       .AppendLine("<交換表單>")
            //       .AppendLine("<全銜>{0}</全銜><機關代碼>{1}</機關代碼><含附件>含附件</含附件>")
            //       .AppendLine("</交換表單>");
            content.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>")
                   .AppendLine("<!DOCTYPE 交換表單 SYSTEM \"104_roster_utf8.dtd\">")
                   .AppendLine("<交換表單>")
                   .AppendLine("<全銜>{0}</全銜><機關代碼>{1}</機關代碼><含附件>含附件</含附件>")
                   .AppendLine("</交換表單>");
            //20181228 固定變更 update end

            return content.ToString();
        }

        /// <summary>
        /// 產生 .di檔案內容格式
        /// </summary>
        /// <param name="descriptLines">內文行數</param>
        /// <returns>檔案內容樣版</returns>
        string GetDIContentTemplate(int descriptLines)
        {
            StringBuilder content = new StringBuilder();

            int no = 2;

            //20181228 固定變更 update start
            //content.AppendLine("<?xml version=\"1.0\" encoding=\"big5\"?>")
            //       .AppendLine("<!DOCTYPE 函 SYSTEM \"99_2_big5.dtd\" [")
            content.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>")
                   .AppendLine("<!DOCTYPE 函 SYSTEM \"104_2_utf8.dtd\" [")
                //20181228 固定變更 update end
                   .AppendLine("<!ENTITY ATTCH1 SYSTEM \"{0}\" NDATA _X>")
                   .AppendLine("<!ENTITY 表單 SYSTEM \"{1}\" NDATA DI>")
                   .AppendLine("<!NOTATION DI SYSTEM \"\">")
                   .AppendLine("<!NOTATION _X SYSTEM \"\">]>")
                   .AppendLine("<函>")
                   .AppendLine("<發文機關>")
                   .AppendLine(string.Format("<全銜>{{{0}}}</全銜>", no++))
                   .AppendLine(string.Format("<機關代碼>{{{0}}}</機關代碼>", no++))
                   .AppendLine("</發文機關>")
                   .AppendLine("<函類別 代碼=\"函\"/>")
                   .AppendLine(string.Format("<地址>{{{0}}}</地址>", no++))
                   .AppendLine(string.Format("<聯絡方式>承辦人：{{{0}}}</聯絡方式>", no++))
                   .AppendLine(string.Format("<聯絡方式>電話：{{{0}}}</聯絡方式>", no++))
                   .AppendLine("<受文者>")
                   .AppendLine("<交換表 交換表單=\"表單\">如正副本行文單位</交換表>")
                   .AppendLine("</受文者>")
                   .AppendLine("<發文日期>")
                   .AppendLine(string.Format("<年月日>{{{0}}}</年月日>", no++))
                   .AppendLine("</發文日期>")
                   .AppendLine("<發文字號>")
                //20180622 RC 線上投單CR修正 宏祥 update start
                //.AppendLine("<字>中信銀個集作字第</字>")
                   .AppendLine(string.Format("<字>{{{0}}}</字>", no++))
                //20180622 RC 線上投單CR修正 宏祥 update end
                   .AppendLine("<文號>")
                   .AppendLine(string.Format("<年度>{{{0}}}</年度>", no++))
                   .AppendLine(string.Format("<流水號>{{{0}}}</流水號>", no++))
                   .AppendLine("</文號>")
                   .AppendLine("</發文字號>")
                   .AppendLine("<速別 代碼=\"速件\"/>")
                   .AppendLine("<密等及解密條件或保密期限>")
                   .AppendLine("<密等/>")
                   .AppendLine("<解密條件或保密期限></解密條件或保密期限>")
                   .AppendLine("</密等及解密條件或保密期限>")
                   .AppendLine("<附件>")
                   .AppendLine(string.Format("<文字>{{{0}}}</文字>", no++))
                   .AppendLine("<附件檔名 附件名=\"ATTCH1\"/>")
                   .AppendLine("</附件>")
                   .AppendLine("<主旨>")
                   .AppendLine(string.Format("<文字>{{{0}}}</文字>", no++))
                   .AppendLine("</主旨>")
                   .AppendLine("<段落 段名=\"說明：\">")
                   .AppendLine("<文字></文字>");

            // 依內文有多少行產生
            for (int lineNo = 1; lineNo <= descriptLines; lineNo++)
            {
                content.AppendLine(string.Format("<條列 序號=\"{0}、\">", EastAsiaNumericFormatter.FormatWithCulture("Ln", lineNo, null, new CultureInfo("zh-tw"))))
                   .AppendLine(string.Format("<文字>{{{0}}}</文字>", no++))
                   .AppendLine("</條列>");
            }

            content.AppendLine("</段落>")
                   .AppendLine("<正本>")
                   .AppendLine(string.Format("<全銜>{{{0}}}</全銜>", no++))
                   .AppendLine("</正本>")
                   .AppendLine("<副本>")
                   .AppendLine("<全銜></全銜>")
                   .AppendLine("</副本>")
                   .AppendLine("</函>");

            return content.ToString();
        }

        /// <summary>
        /// To the full taiwan date.
        /// </summary>
        /// <param name="datetime">The datetime.</param>
        /// <returns></returns>
        string ToFullTaiwanDate(DateTime datetime)
        {
            TaiwanCalendar taiwanCalendar = new TaiwanCalendar();

            return string.Format("中華民國{0}年{1}月{2}日",
                taiwanCalendar.GetYear(datetime),
                datetime.Month,
                datetime.Day);
        }

        /// <summary>
        /// 產生實體檔案
        /// </summary>
        /// <param name="path">檔案所在目錄</param>
        /// <param name="filename">檔案名稱</param>
        /// <param name="content">檔案內容</param>
        void WriteFile(string path, string filename, string content)
        {
            // 判斷路徑是否存在，不存在創建路徑
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            string fullFilename = path + @"\" + filename;

            // 匯出檔案內容
            FileStream fs_sw = new FileStream(fullFilename, FileMode.Create, FileAccess.ReadWrite);

            byte[] bytes_sw = Encoding.UTF8.GetBytes(content.ToString());
            //20181228 固定變更 update start
            //bytes_sw = Encoding.Convert(Encoding.UTF8, Encoding.Default, bytes_sw);
            //20181228 固定變更 update end

            fs_sw.Write(bytes_sw, 0, bytes_sw.Length);
            fs_sw.Flush();

        }

        //20180622 RC 線上投單CR修正 宏祥 add start
        /// <summary>
        /// 取得參數檔裏的有關設定
        /// </summary>
        /// <param name="strCodeNo"></param>
        /// <returns></returns>
        public string getCodeDesc(string strCodeType)
        {
            string sqlSelect = @" SELECT CodeDesc FROM PARMCode WHERE CodeType = @CodeType ";

            base.Parameter.Clear();

            base.Parameter.Add(new CommandParameter("@CodeType", strCodeType));

            return base.ExecuteScalar(sqlSelect).ToString();

        }
        //20180622 RC 線上投單CR修正 宏祥 add end
    }
}
