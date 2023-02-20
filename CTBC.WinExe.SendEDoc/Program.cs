using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CTBC.CSFS.BussinessLogic;
using CTBC.CSFS.Models;
using CTBC.CSFS.Pattern;
using CTBC.CSFS.Resource;
using CTBC.CSFS.ViewModels;
using CTBC.CSFS.Service;
using System.Configuration;
using System.Net;
using CTBC.FrameWork.Util;
using System.IO;
using System.Collections;
using System.Xml;
using System.Text.RegularExpressions;
using System.Data;
//using Microsoft.Reporting.WebForms;
using System.Web.UI;
namespace CTBC.WinExe.SendEDoc
{
    class Program
    {
        private static CaseMasterBIZ masterBiz;
        private static CaseSendSettingBIZ _CaseSendSettingBIZ;
        private static ImportEDocBiz _ImportEDocBiz;
        private static SendEDocBiz _SendEDocBiz;
        private static FileLog m_fileLog;
        private static string loaclFilePath;
        //20160909 宏祥 增加寫入BackupFolder start
        private static string loaclFileBackupPath;
        //20160909 宏祥 增加寫入BackupFolder end
        private static string reportPath;
        //private static List<ReportDataSource> subDataSource;
        static Program()
        {
            //string s = UtlString.EncodeBase64("joseph");
            masterBiz = new CaseMasterBIZ();
            _CaseSendSettingBIZ = new CaseSendSettingBIZ();
            _SendEDocBiz = new SendEDocBiz();
            _ImportEDocBiz = new ImportEDocBiz();
            m_fileLog = new FileLog(ConfigurationManager.AppSettings["filelog"], AppDomain.CurrentDomain.FriendlyName.Replace(".vshost", "").Replace(".exe", ""));
            loaclFilePath = ConfigurationManager.AppSettings["loaclFilePath"];
            //20160909 宏祥 增加寫入BackupFolder start
            loaclFileBackupPath = ConfigurationManager.AppSettings["loaclFileBackupPath"];
            //20160909 宏祥 增加寫入BackupFolder end
            reportPath = ConfigurationManager.AppSettings["reportPath"];
            //subDataSource = new List<ReportDataSource>();
        }
        static void Main(string[] args)
        {
            try
            {
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "=============================================");
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息-------- 電子發文作業開始----------------");
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息-------- 正在獲取發文資料----------------");
                IList<BatchControl> list = _SendEDocBiz.GetBatchControlF();
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息-------- 獲取發文資料完成----------------");
                if (list != null && list.Count > 0)
                {
                    //simon 2016/10/19
                    int mSuccessfulCount = 0;
                    m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------讀入產檔資料筆數: " + list.Count + " 筆------------");

                    m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息-------- 開始產檔----------------");
                    foreach (var item in list)
                    {
                        Guid caseId = item.CaseId;
                        string mCaseNo=SendEdoc(caseId);
                        if (mCaseNo == "")
                            mSuccessfulCount += 0;
                        else if (mCaseNo.IndexOf("紙本發文") > 0)
                        {
                            m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------" + mCaseNo + " ------------");
                        }
                        else if (mCaseNo.IndexOf("已發文") > 0)
                        {
                            m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------" + mCaseNo + " ------------");
                        }
                        else
                        {
                            m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------成功產檔資料: " + mCaseNo + " ------------");
                            mSuccessfulCount += 1;
                        }
                    }
                    m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------成功產檔資料筆數: " + mSuccessfulCount + " 筆------------");

                    m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息-------- 產檔結束----------------");
                }
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息-------- 電子發文作業結束----------------");
            }
            catch (Exception ex)
            {
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------電子發文作業失敗，失敗原因：" + ex.Message + "----------------");
            }
        }

        private static string SendEdoc(Guid caseId)
        {
            string mCaseNo = "";

            //caseId = new Guid("b1237353-fc7c-4365-bc2b-53404465efe4");
            if (loaclFilePath.Trim() != "")
            {
                if (!Directory.Exists(loaclFilePath))
                {
                    Directory.CreateDirectory(loaclFilePath);
                }
            }
            else
            {
                loaclFilePath = AppDomain.CurrentDomain.BaseDirectory;
            }

            //20160909 宏祥 增加寫入BackupFolder start
            if (loaclFileBackupPath.Trim() != "")
            {
                if (!Directory.Exists(loaclFileBackupPath))
                {
                    Directory.CreateDirectory(loaclFileBackupPath);
                }
            }
            else
            {
                loaclFileBackupPath = AppDomain.CurrentDomain.BaseDirectory;
            }
            //20160909 宏祥 增加寫入BackupFolder end

            FileStream fs_di = null;
            FileStream fs_sw = null;
            FileStream fs_pdf = null;
            FileStream fs_txt = null;

            IList<CaseSendSettingQueryResultViewModel> caseSendSettingList = _CaseSendSettingBIZ.GetSendSettingList(caseId);

			List<string> aryCaseIdList = new List<string>();
			aryCaseIdList.Add(caseId.ToString());
			DataTable dtSendSetting = new CaseSendSettingBIZ().GetSendSettingByCaseIdList(aryCaseIdList);

            caseSendSettingList = caseSendSettingList.Where(item => item.SendKind == "電子發文").ToList();
            try
            {
                if (caseSendSettingList != null && caseSendSettingList.Count > 0)
                {
                    //List<ReportParameter> listParm = new List<ReportParameter>();
                    IList<PARMCode> ParmCodeList = _SendEDocBiz.GetPARMCodeByCodeType("REPORT_SETTING");
                    //* CTBC的地址.電話.傳真
                    //listParm.Add(new ReportParameter("CtbcAddr", ParmCodeList.Where(m => m.CodeNo == "Address").ToList()[0].CodeDesc));
                    //listParm.Add(new ReportParameter("CtbcTel", ParmCodeList.Where(m => m.CodeNo == "Tel").ToList()[0].CodeDesc));
                    //listParm.Add(new ReportParameter("CtbcFax", ParmCodeList.Where(m => m.CodeNo == "Fax").ToList()[0].CodeDesc));
                    //listParm.Add(new ReportParameter("CtbcButtomLine", ParmCodeList.Where(m => m.CodeNo == "ButtomLine").ToList()[0].CodeDesc));
                    //listParm.Add(new ReportParameter("CtbcButtomLine2", ParmCodeList.Where(m => m.CodeNo == "ButtomLine2").ToList()[0].CodeDesc));

                    //判斷是否已發文（當一個caseId有多筆且有一個狀態為1時，表明已發文，則將其餘的STATUS_Create都更新為1，避免重複發文）Modify by Nianhuaxiao 20171017 
                    IList<BatchControl> listG = _SendEDocBiz.GetBatchControlG(caseId);
                    if(listG != null && listG.Count > 0)
                    {
                        _SendEDocBiz.UpdateBatchControlT(caseId);
                        mCaseNo = caseId + "已發文";
                    }
                    else
                    {
                        SendEDocBiz seBiz = new SendEDocBiz();

                        foreach (var item in caseSendSettingList)
                        {
                            IList<CaseSendSettingDetails> caseSendSettingDetailsList = _SendEDocBiz.GetSendSettingDetails(item.SerialId);
                            DataTable caseSendSettingDetailsDt = _SendEDocBiz.GetSendSettingBySerialID(item.SerialId);
                            DataTable dtSendDesc = GetDescTable(caseSendSettingDetailsDt);
                            DataTable dtMaster = _SendEDocBiz.GetCaseMasterByCaseId(caseId);


                            //20210825, 要讀取parmcode, Codetype='AutoPayReply' 的Enable狀況...
                            var oAutoPayReply = masterBiz.GetPARMCodeByCodeType("AutoPayReply", "ccReply").FirstOrDefault();
                            bool isAutoPayReply = false;
                            if( oAutoPayReply!=null && oAutoPayReply.Enable!=null)
							{
                                isAutoPayReply = (bool)oAutoPayReply.Enable;
							}

                            // 20210825, 邏輯是.. 若是AutoPayReply=True時, 不管是扣押(TXT,DI,SW) , 支付(DI,SW), 都要產出
                            //                                 若是AutoPayReply=False時, 只有扣押(TXT,DI,SW), 要產出, 支付不產出  ....


                            if ( item.Template.Contains("支付") )
							{
                                if(isAutoPayReply)
                                { 
                                    #region 20210605, 產生DI,SW的格式
                                string mGovNo = "";
                                mGovNo = dtMaster.Rows[0]["GovNo"] + "";
                                mCaseNo = dtMaster.Rows[0]["CaseNo"] + "";
                                string mGovSendNo = ""; //來文號
                                                        //int mPos = 0;
                                                        //mPos = mGovNo.IndexOf("字第");
                                                        //if (mPos >= 0)
                                                        //{ mGovSendNo = mGovNo.Substring(mPos+2);
                                                        //mGovSendNo = mGovSendNo.Replace("號", "");
                                                        //}
                                                        //else
                                                        //{ mGovSendNo = mGovNo; }
                                mGovSendNo = item.SendNo + ".txt";// +"0822.txt";  //加上中信銀行代碼(TXT檔名)

                                //產DI檔
                                StringBuilder text_di = new StringBuilder();
                                DataRow dr = caseSendSettingDetailsDt.Rows[0];
                                string sendDate = UtlString.FormatDateTw(Convert.ToDateTime(dr["SendDate"].ToString()).ToString("yyyy/MM/dd"));
                                string sendDate1 = sendDate.Insert(sendDate.IndexOf("/"), "年").Insert(sendDate.LastIndexOf("/") + 1, "月").Replace("/", "") + "日";
                                //string yaer = sendDate.Substring(0, sendDate.IndexOf("/"));
                                string sendNo = dr["SendNo"].ToString();
                                string year = sendNo.Length == 10 ? sendNo.Substring(0, 3) : sendDate.Substring(0, sendDate.IndexOf("/"));

                                PARMCode codeItem = masterBiz.GetPARMCodeByCodeType("REPORT_SETTING", "Fax").FirstOrDefault();


                                string AgentUser= dtSendSetting.Rows[0]["CreatedUser"].ToString(); 
                                string Address= ParmCodeList.Where(m => m.CodeNo == "Address").ToList()[0].CodeDesc; 
                                string Tel= dtSendSetting.Rows[0]["TelNo"].ToString();
                                string Fax= codeItem.CodeDesc==null ? "" : codeItem.CodeDesc;
                                string Title = ParmCodeList.Where(x => x.CodeNo == "ButtomLine").FirstOrDefault().CodeDesc;
                                string MasterName= ParmCodeList.Where(x => x.CodeNo == "ButtomLine2").FirstOrDefault().CodeDesc;

                                text_di.AppendLine(string.Format(GetDIDocAutoPay(), new string[] {
                            dr["SendNo"].ToString(),                            //0
                            //dr["SendWord"].ToString(),                          //1
							ConfigurationManager.AppSettings["GovName"],
                            //"",                                                 //2
							ConfigurationManager.AppSettings["GovCode"],
                            ParmCodeList.Where(m => m.CodeNo == "Address").ToList()[0].CodeDesc,                                            //3
                            //ParmCodeList.Where(m => m.CodeNo == "PersonAndTel").ToList()[0].CodeDesc,                                  //4
							dtSendSetting.Rows[0]["CreatedUser"].ToString(),
                            //ParmCodeList.Where(m => m.CodeNo == "Tel").ToList()[0].CodeDesc,                                  //5
							dtSendSetting.Rows[0]["TelNo"].ToString(),
                            sendDate1,                                          //6
                            dr["SendWord"].ToString(),                          //7
                            year,                                               //8
                            //dr["SendNo"].ToString(),                            //9
							sendNo.Length == 10 ? sendNo.Substring(3,7) : "",
                            dr["Speed"].ToString(),                             //10
                            dr["Subject"].ToString(),                           //11
                            dtSendDesc.Rows[0]["Content"].ToString(),           //12
                            dtSendDesc.Rows[1]["Content"].ToString(),           //13
                            dtSendDesc.Rows.Count > 2 ? dtSendDesc.Rows[2]["Content"].ToString() : "",           //14
                            dr["Receive"].ToString(),                           //15
                            dr["Cc"].ToString(),                                //16
                            codeItem == null ? "" : codeItem.CodeDesc, //17
                            mGovSendNo  //18 附件檔名
                        }));


                                string localFile_di = loaclFilePath.TrimEnd('\\') + "\\" + item.SendNo + ".di";
                                //20160909 宏祥 增加寫入BackupFolder start
                                string localFileBackup_di = loaclFileBackupPath.TrimEnd('\\') + "\\" + item.SendNo + ".di";


                                CaseEdocFile caseEdocFile_di = CreateDiFile(localFile_di, caseId, text_di, item.SendNo);
                                CreateDiFile(localFileBackup_di, caseId, text_di, item.SendNo);

                                //20210929, 支付案件, 只需要產生副本, 因此, 只要有SendType=2的, 才要產出SW
                                caseSendSettingDetailsList = caseSendSettingDetailsList.Where(x => x.SendType == 2).ToList();
                                //產SW檔
                                string text_sw = GetSWDoc(caseSendSettingDetailsList);

                                //20210923, 沒有附件.. 所以
                                text_sw = text_sw.Replace("<含附件>含附件</含附件>", "<含附件></含附件>");

                                string localFile_sw = loaclFilePath.TrimEnd('\\') + "\\" + item.SendNo + ".sw";
                                //20160909 宏祥 增加寫入BackupFolder start
                                string localFileBackup_sw = loaclFileBackupPath.TrimEnd('\\') + "\\" + item.SendNo + ".sw";
                                CaseEdocFile caseEdocFile_sw = CreateSwFile(localFile_sw, caseId, text_sw, item.SendNo);
                                CreateSwFile(localFileBackup_sw, caseId, text_sw, item.SendNo);

                                //寫入CaseEDoc資料
                                //_ImportEDocBiz.InsertCaseEdocFile(caseEdocFile_pdf);
                                _ImportEDocBiz.InsertCaseEdocFile(caseEdocFile_di);
                                _ImportEDocBiz.InsertCaseEdocFile(caseEdocFile_sw);


                                // 20210605, 新增CaseMasterDoc 記錄發文當時的承辦人及主管
                                // InsertCaseMasterDoc(Guid caseId,string AgentUser,string Address,string Tel,string Fax,string Title,string MasterName)
                                seBiz.InsertCaseMasterDoc(caseId, AgentUser, Address, Tel, Fax, Title, MasterName);

                                    #endregion
                                }
                            }
							else
							{
                                #region 20210605, 原本扣押產生TXT,DI,SW的邏輯
                                //扣押資訊
                                DataTable dtCaseSeizure = _SendEDocBiz.GetCaseSeizureByCaseId(caseId);
                                //義務人資訊
                                DataTable dtCaseObligor = _SendEDocBiz.GetCaseObligorByCaseId(caseId);

                                //產TXT檔--扣押資訊資料
                                //須先產生TXT檔,再產生DI檔,DI檔案中須寫入附件TXT的檔名
                                string mGovNo = "";
                                mGovNo = dtMaster.Rows[0]["GovNo"] + "";
                                mCaseNo = dtMaster.Rows[0]["CaseNo"] + "";
                                string mGovSendNo = ""; //來文號
                                                        //int mPos = 0;
                                                        //mPos = mGovNo.IndexOf("字第");
                                                        //if (mPos >= 0)
                                                        //{ mGovSendNo = mGovNo.Substring(mPos+2);
                                                        //mGovSendNo = mGovSendNo.Replace("號", "");
                                                        //}
                                                        //else
                                                        //{ mGovSendNo = mGovNo; }
                                mGovSendNo = item.SendNo + ".txt";// +"0822.txt";  //加上中信銀行代碼(TXT檔名)

                                string text_txt = GetTXTDoc(dtCaseSeizure, dtCaseObligor, mGovNo);
                                string localFile_txt = loaclFilePath.TrimEnd('\\') + "\\" + item.SendNo + ".txt";

                                //20160909 宏祥 增加寫入BackupFolder start
                                string localFileBackup_txt = loaclFileBackupPath.TrimEnd('\\') + "\\" + item.SendNo + ".txt";
                                CaseEdocFile caseEdocFile_txt = CreateTxtFile(localFile_txt, caseId, text_txt, item.SendNo);
                                CreateTxtFile(localFileBackup_txt, caseId, text_txt, item.SendNo);
                                //fs_txt = new FileStream(localFile_txt, FileMode.Create, FileAccess.ReadWrite);
                                //byte[] bytes_txt = Encoding.UTF8.GetBytes(text_txt.ToString());
                                //fs_txt.Write(bytes_txt, 0, bytes_txt.Length);

                                //CaseEdocFile caseEdocFile_txt = new CaseEdocFile();
                                //caseEdocFile_txt.CaseId = caseId;
                                //caseEdocFile_txt.SendNo = item.SendNo;
                                //caseEdocFile_txt.Type = "發文";
                                //caseEdocFile_txt.FileType = "txt";
                                //caseEdocFile_txt.FileName = mGovSendNo;// item.SendNo + ".txt";
                                //caseEdocFile_txt.FileObject = bytes_txt;
                                //fs_txt.Flush();
                                //20160909 宏祥 增加寫入BackupFolder end


                                //產DI檔
                                StringBuilder text_di = new StringBuilder();
                                DataRow dr = caseSendSettingDetailsDt.Rows[0];
                                string sendDate = UtlString.FormatDateTw(Convert.ToDateTime(dr["SendDate"].ToString()).ToString("yyyy/MM/dd"));
                                string sendDate1 = sendDate.Insert(sendDate.IndexOf("/"), "年").Insert(sendDate.LastIndexOf("/") + 1, "月").Replace("/", "") + "日";
                                //string yaer = sendDate.Substring(0, sendDate.IndexOf("/"));
                                string sendNo = dr["SendNo"].ToString();
                                string year = sendNo.Length == 10 ? sendNo.Substring(0, 3) : sendDate.Substring(0, sendDate.IndexOf("/"));

                                PARMCode codeItem = masterBiz.GetPARMCodeByCodeType("REPORT_SETTING", "Fax").FirstOrDefault();
                                text_di.AppendLine(string.Format(GetDIDoc(), new string[] {
                            dr["SendNo"].ToString(),                            //0
                            //dr["SendWord"].ToString(),                          //1
							ConfigurationManager.AppSettings["GovName"],
                            //"",                                                 //2
							ConfigurationManager.AppSettings["GovCode"],
                            ParmCodeList.Where(m => m.CodeNo == "Address").ToList()[0].CodeDesc,                                            //3
                            //ParmCodeList.Where(m => m.CodeNo == "PersonAndTel").ToList()[0].CodeDesc,                                  //4
							dtSendSetting.Rows[0]["CreatedUser"].ToString(),
                            //ParmCodeList.Where(m => m.CodeNo == "Tel").ToList()[0].CodeDesc,                                  //5
							dtSendSetting.Rows[0]["TelNo"].ToString(),
                            sendDate1,                                          //6
                            dr["SendWord"].ToString(),                          //7
                            year,                                               //8
                            //dr["SendNo"].ToString(),                            //9
							sendNo.Length == 10 ? sendNo.Substring(3,7) : "",
                            dr["Speed"].ToString(),                             //10
                            dr["Subject"].ToString(),                           //11
                            dtSendDesc.Rows[0]["Content"].ToString(),           //12
                            dtSendDesc.Rows[1]["Content"].ToString(),           //13
                            dtSendDesc.Rows.Count > 2 ? dtSendDesc.Rows[2]["Content"].ToString() : "",           //14
                            dr["Receive"].ToString(),                           //15
                            dr["Cc"].ToString(),                                //16
                            codeItem == null ? "" : codeItem.CodeDesc, //17
                            mGovSendNo  //18 附件檔名
                        }));
                                string localFile_di = loaclFilePath.TrimEnd('\\') + "\\" + item.SendNo + ".di";
                                //20160909 宏祥 增加寫入BackupFolder start
                                string localFileBackup_di = loaclFileBackupPath.TrimEnd('\\') + "\\" + item.SendNo + ".di";
                                CaseEdocFile caseEdocFile_di = CreateDiFile(localFile_di, caseId, text_di, item.SendNo);
                                CreateDiFile(localFileBackup_di, caseId, text_di, item.SendNo);
                                //fs_di = new FileStream(localFile_di, FileMode.Create, FileAccess.ReadWrite);
                                //byte[] bytes_di = Encoding.UTF8.GetBytes(text_di.ToString());
                                //fs_di.Write(bytes_di, 0, bytes_di.Length);
                                //CaseEdocFile caseEdocFile_di = new CaseEdocFile();
                                //caseEdocFile_di.CaseId = caseId;
                                //caseEdocFile_di.SendNo = item.SendNo;
                                //caseEdocFile_di.Type = "發文";
                                //caseEdocFile_di.FileType = "di";
                                //caseEdocFile_di.FileName = item.SendNo + ".di";
                                //caseEdocFile_di.FileObject = bytes_di;
                                //fs_di.Flush();
                                //20160909 宏祥 增加寫入BackupFolder end

                                //產SW檔
                                string text_sw = GetSWDoc(caseSendSettingDetailsList);
                                string localFile_sw = loaclFilePath.TrimEnd('\\') + "\\" + item.SendNo + ".sw";
                                //20160909 宏祥 增加寫入BackupFolder start
                                string localFileBackup_sw = loaclFileBackupPath.TrimEnd('\\') + "\\" + item.SendNo + ".sw";
                                CaseEdocFile caseEdocFile_sw = CreateSwFile(localFile_sw, caseId, text_sw, item.SendNo);
                                CreateSwFile(localFileBackup_sw, caseId, text_sw, item.SendNo);
                                //fs_sw = new FileStream(localFile_sw, FileMode.Create, FileAccess.ReadWrite);
                                //byte[] bytes_sw = Encoding.UTF8.GetBytes(text_sw.ToString());
                                //fs_sw.Write(bytes_sw, 0, bytes_sw.Length);
                                //CaseEdocFile caseEdocFile_sw = new CaseEdocFile();
                                //caseEdocFile_sw.CaseId = caseId;
                                //caseEdocFile_sw.SendNo = item.SendNo;
                                //caseEdocFile_sw.Type = "發文";
                                //caseEdocFile_sw.FileType = "sw";
                                //caseEdocFile_sw.FileName = item.SendNo + ".sw";
                                //caseEdocFile_sw.FileObject = bytes_sw;
                                //fs_sw.Flush();
                                //20160909 宏祥 增加寫入BackupFolder end

                                //產PDF檔
                                //string localFile_pdf = loaclFilePath.TrimEnd('\\') + "\\" + item.SendNo + ".pdf";
                                //fs_pdf = new FileStream(localFile_pdf, FileMode.Create);
                                //byte[] bytes_pdf = GetPDFDoc(dtMaster, caseSendSettingDetailsDt, dtSendDesc, caseId, listParm);
                                //fs_pdf.Write(bytes_pdf, 0, bytes_pdf.Length);
                                //CaseEdocFile caseEdocFile_pdf = new CaseEdocFile();
                                //caseEdocFile_pdf.CaseId = caseId;
                                //caseEdocFile_pdf.SendNo = item.SendNo;
                                //caseEdocFile_pdf.Type = "發文";
                                //caseEdocFile_pdf.FileType = "pdf";
                                //caseEdocFile_pdf.FileName = item.SendNo + ".pdf";
                                //caseEdocFile_pdf.FileObject = bytes_pdf;
                                //fs_pdf.Flush();

                                //寫入CaseEDoc資料
                                //_ImportEDocBiz.InsertCaseEdocFile(caseEdocFile_pdf);
                                _ImportEDocBiz.InsertCaseEdocFile(caseEdocFile_di);
                                _ImportEDocBiz.InsertCaseEdocFile(caseEdocFile_sw);
                                _ImportEDocBiz.InsertCaseEdocFile(caseEdocFile_txt);
                                #endregion
                            }

                        }

                        _SendEDocBiz.UpdateBatchControlT(caseId);
                    }
                }
                else
                {
                    _SendEDocBiz.DeleteBatchControl(caseId);
                    mCaseNo = caseId + "為紙本發文,資料已刪除";
                }
            }
            catch (Exception ex)
            {
                mCaseNo = "";
                throw ex;
                
            }
            finally
            {
                if(fs_di!=null)
                fs_di.Close();
                if (fs_sw != null)
                fs_sw.Close();
                if (fs_pdf != null)
                fs_pdf.Close();
            }
            return mCaseNo;
        }

        /// <summary>
        /// 20160909 宏祥 增加寫入BackupFolder(TxtFile)
        /// </summary>
        /// <param name="localFile_di"></param>
        /// <param name="caseId"></param>
        /// <param name="strText_di"></param>
        /// <param name="strSendNo"></param>
        private static CaseEdocFile CreateTxtFile(string localFile_txt, Guid caseId, string strText_txt, string strSendNo)
        {
            FileStream fs_txt = null;

            fs_txt = new FileStream(localFile_txt, FileMode.Create, FileAccess.ReadWrite);

            byte[] bytes_txt = Encoding.UTF8.GetBytes(strText_txt.ToString());
            bytes_txt = Encoding.Convert(Encoding.UTF8, Encoding.Default, bytes_txt);
            //byte[] bytes_txt = Encoding.ASCII.GetBytes(strText_txt.ToString());

            fs_txt.Write(bytes_txt, 0, bytes_txt.Length);

            CaseEdocFile caseEdocFile_txt = new CaseEdocFile();
            caseEdocFile_txt.CaseId = caseId;
            caseEdocFile_txt.SendNo = strSendNo;
            caseEdocFile_txt.Type = "發文";
            caseEdocFile_txt.FileType = "txt";
            caseEdocFile_txt.FileName = strSendNo + ".txt";
            caseEdocFile_txt.FileObject = bytes_txt;
            fs_txt.Flush();

            return caseEdocFile_txt;
        }

        /// <summary>
        /// 20160909 宏祥 增加寫入BackupFolder(DiFile)
        /// </summary>
        /// <param name="localFile_di"></param>
        /// <param name="caseId"></param>
        /// <param name="strText_di"></param>
        /// <param name="strSendNo"></param>
        private static CaseEdocFile CreateDiFile(string localFile_di, Guid caseId, StringBuilder strText_di, string strSendNo)
        {
            FileStream fs_di = null;

            fs_di = new FileStream(localFile_di, FileMode.Create, FileAccess.ReadWrite);
            //byte[] bytes_di = Encoding.UTF8.GetBytes(strText_di.ToString());
            //byte[] bytes_di = Encoding.ASCII.GetBytes(strText_di.ToString());

            byte[] bytes_di = Encoding.UTF8.GetBytes(strText_di.ToString());
            //20181228 固定變更 update start
            //bytes_di = Encoding.Convert(Encoding.UTF8, Encoding.Default, bytes_di);
            //20181228 固定變更 update start

            fs_di.Write(bytes_di, 0, bytes_di.Length);
            CaseEdocFile caseEdocFile_di = new CaseEdocFile();
            caseEdocFile_di.CaseId = caseId;
            caseEdocFile_di.SendNo = strSendNo;
            caseEdocFile_di.Type = "發文";
            caseEdocFile_di.FileType = "di";
            caseEdocFile_di.FileName = strSendNo + ".di";
            caseEdocFile_di.FileObject = bytes_di;
            fs_di.Flush();

            return caseEdocFile_di;
        }

        /// <summary>
        /// 20160909 宏祥 增加寫入BackupFolder(SwFile)
        /// </summary>
        /// 
        /// <param name="localFile_sw"></param>
        /// <param name="caseId"></param>
        /// <param name="strText_sw"></param>
        /// <param name="strSendNo"></param>
        /// <returns></returns>
        private static CaseEdocFile CreateSwFile(string localFile_sw, Guid caseId, string strText_sw, string strSendNo)
        {
            FileStream fs_sw = null;

            fs_sw = new FileStream(localFile_sw, FileMode.Create, FileAccess.ReadWrite);
            //byte[] bytes_sw = Encoding.UTF8.GetBytes(strText_sw.ToString());
            //byte[] bytes_sw = Encoding.ASCII.GetBytes(strText_sw.ToString());

            byte[] bytes_sw = Encoding.UTF8.GetBytes(strText_sw.ToString());
            //20181228 固定變更 update start
            //bytes_sw = Encoding.Convert(Encoding.UTF8, Encoding.Default, bytes_sw);
            //20181228 固定變更 update end

            fs_sw.Write(bytes_sw, 0, bytes_sw.Length);
            CaseEdocFile caseEdocFile_sw = new CaseEdocFile();
            caseEdocFile_sw.CaseId = caseId;
            caseEdocFile_sw.SendNo = strSendNo;
            caseEdocFile_sw.Type = "發文";
            caseEdocFile_sw.FileType = "sw";
            caseEdocFile_sw.FileName = strSendNo + ".sw";
            caseEdocFile_sw.FileObject = bytes_sw;
            fs_sw.Flush();

            return caseEdocFile_sw;
        }

        //private static void SubreportProcessingEventHandler(object sender, SubreportProcessingEventArgs e)
        //{
        //    foreach (var reportDataSource in subDataSource)
        //    {
        //        e.DataSources.Add(reportDataSource);
        //    }
        //}
        //private static byte[] GetPDFDoc(DataTable dtMaster, DataTable dtSendSetting, DataTable dtSendDesc, Guid caseId, List<ReportParameter> listParm)
        //{
        //    if (reportPath.Trim() != "")
        //    {
        //        if (!Directory.Exists(reportPath))
        //        {
        //            Directory.CreateDirectory(reportPath);
        //        }
        //    }
        //    else
        //    {
        //        reportPath = AppDomain.CurrentDomain.BaseDirectory;
        //    }
        //    reportPath = reportPath.TrimEnd('\\') + "\\" + "Rdlc\\CaseMaster.rdlc";
        //    List<string> aryCaseIdList = new List<string>();
        //    aryCaseIdList.Add(caseId.ToString());
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

        //    DataTable dt = new DataTable();
        //    if (dtSendSetting != null && dtSendSetting.Rows.Count > 0)
        //    {
        //        dt = dtSendSetting.Clone();
        //        dt.ImportRow(dtSendSetting.Rows[0]);
        //        dt.Rows[0]["GovName"] = "如正副本行文單位";
        //    }

        //    LocalReport localReport = null;
        //    localReport = new LocalReport { ReportPath = reportPath };
         
        //    localReport.SetParameters(listParm); //*添加參數
        //    localReport.DataSources.Add(new ReportDataSource("DataSet1", dtMaster));   //* 添加數據源,可以多個
        //    localReport.DataSources.Add(new ReportDataSource("SendSetting", dt));   //* 添加數據源,可以多個
        //    localReport.SubreportProcessing += SubreportProcessingEventHandler;

        //    subDataSource.Add(new ReportDataSource("SendSetting", dt));
        //    subDataSource.Add(new ReportDataSource("CaseAccountExternal", dtExternal));
        //    subDataSource.Add(new ReportDataSource("CaseReceipt", dtReceipt));
        //    subDataSource.Add(new ReportDataSource("SendSettingDesc", dtSendDesc));

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
        //    subDataSource.RemoveRange(0, 4);
        //    return renderedBytes;
        //}

        private static string GetSWDoc(IList<CaseSendSettingDetails> caseSendSettingDetailsList)
        {
            StringBuilder text_sw = new StringBuilder();
            //20181228 固定變更 update start
            text_sw.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            //text_sw.AppendLine("<?xml version=\"1.0\" encoding=\"big5\"?>");
            //text_sw.AppendLine("<!DOCTYPE 交換表單 SYSTEM \"99_roster_big5.dtd\">");
            text_sw.AppendLine("<!DOCTYPE 交換表單 SYSTEM \"104_roster_utf8.dtd\">");
            //20181228 固定變更 update end
            text_sw.AppendLine("<交換表單>");
            foreach (var item in caseSendSettingDetailsList)
            {
                text_sw.AppendLine("<全銜>" + item.GovName + "</全銜><機關代碼>" + item.GovCode + "</機關代碼><含附件>含附件</含附件>");
            }
            text_sw.AppendLine("</交換表單>");
            return text_sw.ToString();
        }

        private static string GetTXTDoc(DataTable dtCaseSeizure, DataTable dtCaseObligor, string ReceiveNo)
        {
            StringBuilder text_txt = new StringBuilder();
            text_txt.AppendLine("執行命令種類：扣押存款");
            
            string ms = Repeat("-",144);
            text_txt.AppendLine(ms);

            string mLine = "";
            mLine = FixLengthString("分署發文字號",40," ",true) + " ";
            mLine += FixLengthString("義務人", 70, " ", true) + " ";
            mLine += FixLengthString("統一編號", 14, " ", true) + " ";
            mLine += FixLengthString("扣押金額", 11, " ", false) + " ";
            mLine += FixLengthString("手續費", 6, " ", false);
            text_txt.AppendLine(mLine);
            //mLine = mLine + string.Empty.PadRight(40 - Encoding.Default.GetByteCount(mLine));

            text_txt.AppendLine(ms);

            //foreach (var item in caseSendSettingDetailsList)
            //{
            //    text_txt.AppendLine("<全銜>" + item.GovName + "</全銜><機關代碼>" + item.GovCode + "</機關代碼><含附件>含附件</含附件>");
            //}
            //text_txt.AppendLine("</交換表單>");
            if (dtCaseSeizure.Rows.Count > 0)
            {
                //扣押資訊
                foreach (DataRow row in dtCaseSeizure.Rows)
                {
                    mLine = "";
                    mLine = FixLengthString(ReceiveNo, 40, " ", true) + " ";
                    //CSFS-66 modify by nianhuaxiao 20170728 start
                    mLine += FixLengthString(row["ObligorName"] + "", 70, " ", true) + " ";
                    mLine += FixLengthString(row["ObligorNo"] + "", 14, " ", true) + " ";
                    int mTotalAmount = 0;

                    //金額為空則轉化為數字0，否則轉化為相應的數字
                    string strTotalAmount = string.IsNullOrEmpty(row["TotalAmount"].ToString()) ? "0" : row["TotalAmount"].ToString();
                    double numbTotalAmount = Convert.ToDouble(strTotalAmount);
                    //CSFS-66 modify by nianhuaxiao 20170728 end

                    //20161104 宏祥 update 扣押$0案件，txt檔為$-250 start
                    if (Convert.ToInt32(numbTotalAmount) == 0)
                    {
                        mLine += FixLengthString(mTotalAmount.ToString(), 11, " ", false) + " ";
                        mLine += FixLengthString("0", 6, " ", false); //固定手續費=顯示0
                    }
                    else
                    {
                        //mTotalAmount = Convert.ToInt32(row["TotalAmount"]) - 250;
                        var aa = row["TotalAmount"].ToString();
                        mTotalAmount = (int)Math.Floor(decimal.Parse(aa)) - 250;//不進位,只舍不入
                        mLine += FixLengthString(mTotalAmount.ToString(), 11, " ", false) + " ";
                        mLine += FixLengthString("250", 6, " ", false); //固定手續費=250
                    }
                    text_txt.AppendLine(mLine);
                    //20161104 宏祥 update 扣押$0案件，txt檔為$-250 end
                }
            }
            else
            { 
                //來文的義務人資訊,其餘欄位顯示0.(沒有扣押資訊)
                //讀取義務人資訊

                foreach (DataRow row in dtCaseObligor.Rows)
                {
                    mLine = "";
                    mLine = FixLengthString(ReceiveNo, 40, " ", true) + " ";
                    mLine += FixLengthString(row["ObligorName"] + "", 70, " ", true) + " ";
                    mLine += FixLengthString(row["ObligorNo"] + "", 14, " ", true) + " ";

                    int mTotalAmount = 0;
                    //mTotalAmount = Convert.ToInt32(row["TotalAmount"]) - 250;
                    mLine += FixLengthString(mTotalAmount.ToString(), 11, " ", false) + " ";
                    mLine += FixLengthString("0", 6, " ", false); //固定手續費=顯示0
                    text_txt.AppendLine(mLine);
                }

            }
            return text_txt.ToString();
        }

        //回傳固定長度的 "字元" 所組成的字串,不足部分補上特定(空白)字元
        private static string FixLengthString(string pOriString, int pFixLength, string FillChar, Boolean isRightSide)
        {
            string mOriString = pOriString.Replace(".00","");
            string mLine = "";
            try
            {
                if (isRightSide)
                    mLine = mOriString + Repeat(FillChar, (pFixLength - Encoding.Default.GetByteCount(mOriString)));
                else
                    mLine = Repeat(FillChar, (pFixLength - Encoding.Default.GetByteCount(mOriString))) + mOriString;
            }
            catch {
                mLine = mOriString;
            }
            return mLine;
        }

        ////回傳重複的 "字元" 所組成的字串
        //public static string Repeat(this char chatToRepeat, int repeat)
        //{
        //    return new string(chatToRepeat, repeat);
        //}

        //回傳重複的 "字串" 所組成的字串
        private static string Repeat(string stringToRepeat, int repeat)
        {
            var builder = new StringBuilder(repeat * stringToRepeat.Length);
            for (int i = 0; i < repeat; i++)
            {
                builder.Append(stringToRepeat);
            }
            return builder.ToString();
        }

        

        private static string GetDIDoc()
        {
            StringBuilder text_di = new StringBuilder();
            //20181228 固定變更 update start
            //text_di.AppendLine("<?xml version=\"1.0\" encoding=\"big5\"?>");
			   text_di.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            //text_di.AppendLine("<!DOCTYPE 函 SYSTEM \"99_2_big5.dtd\" [");
            text_di.AppendLine("<!DOCTYPE 函 SYSTEM \"104_2_utf8.dtd\" [");
            //20181228 固定變更 update start
            text_di.AppendLine("<!ENTITY ATTCH1 SYSTEM \"{18}\" NDATA _X>");
            //text_di.AppendLine("<!ENTITY ATTCH1 SYSTEM \"{18}.txt\" NDATA _X>");
            //text_di.AppendLine("<!ENTITY ATTCH2 SYSTEM \"{0}.docx\" NDATA _X>");
            text_di.AppendLine("<!ENTITY 表單 SYSTEM \"{0}.sw\" NDATA DI>");
            text_di.AppendLine("<!NOTATION DI SYSTEM \"\">");
            text_di.AppendLine("<!NOTATION _X SYSTEM \"\">]>");
            text_di.AppendLine("<函>");
            text_di.AppendLine("<發文機關>");
            text_di.AppendLine("<全銜>{1}</全銜>");
            text_di.AppendLine("<機關代碼>{2}</機關代碼>");
            text_di.AppendLine("</發文機關>");
            text_di.AppendLine("<函類別 代碼=\"函\"/>");
            text_di.AppendLine("<地址>{3}</地址>");
            text_di.AppendLine("<聯絡方式>承辦人：{4}</聯絡方式>");
            text_di.AppendLine("<聯絡方式>電話：{5}</聯絡方式>");
			text_di.AppendLine("<聯絡方式>傳真：{17}</聯絡方式>");
            text_di.AppendLine("<受文者>");
            text_di.AppendLine("<交換表 交換表單=\"表單\">如正副本行文單位</交換表>");
            text_di.AppendLine("</受文者>");
            text_di.AppendLine("<發文日期>");
            text_di.AppendLine("<年月日>中華民國{6}</年月日>");
            text_di.AppendLine("</發文日期>");
            text_di.AppendLine("<發文字號>");
            text_di.AppendLine("<字>{7}</字>");
            text_di.AppendLine("<文號>");
            text_di.AppendLine("<年度>{8}</年度>");
            text_di.AppendLine("<流水號>{9}</流水號>");
            text_di.AppendLine("</文號>");
            text_di.AppendLine("</發文字號>");
            text_di.AppendLine("<速別 代碼=\"{10}\"/>");
            text_di.AppendLine("<密等及解密條件或保密期限>");
            text_di.AppendLine("<密等/>");
            text_di.AppendLine("<解密條件或保密期限></解密條件或保密期限>");
            text_di.AppendLine("</密等及解密條件或保密期限>");
            text_di.AppendLine("<附件>");
            text_di.AppendLine("<文字>執行命令扣押存款</文字>");
            //text_di.AppendLine("<文字>如附件</文字>");
            //text_di.AppendLine("<文字></文字>");
            //text_di.AppendLine("<附件檔名 附件名=\"ATTCH1 ATTCH2\"/>");
            text_di.AppendLine("<附件檔名 附件名=\"ATTCH1\"/>");
            text_di.AppendLine("</附件>");
            text_di.AppendLine("<主旨>");
            text_di.AppendLine("<文字>{11}</文字>");
            text_di.AppendLine("</主旨>");
            text_di.AppendLine("<段落 段名=\"說明：\">");
            text_di.AppendLine("<文字></文字>");
            text_di.AppendLine("<條列 序號=\"一、\">");
            text_di.AppendLine("<文字>{12}</文字>");
            text_di.AppendLine("</條列>");
            text_di.AppendLine("<條列 序號=\"二、\">");
            text_di.AppendLine("<文字>{13}</文字>");
            text_di.AppendLine("</條列>");
            text_di.AppendLine("<條列 序號=\"三、\">");
            text_di.AppendLine("<文字>{14}</文字>");
            text_di.AppendLine("</條列>");
            text_di.AppendLine("</段落>");
            text_di.AppendLine("<正本>");
            text_di.AppendLine("<全銜>{15}</全銜>");
            text_di.AppendLine("</正本>	");
            text_di.AppendLine("<副本>");
            text_di.AppendLine("<全銜>{16}</全銜>");
            text_di.AppendLine("</副本>");
			text_di.AppendLine("</函>");
            return text_di.ToString();
        }


        //20210923, 支付沒有附件, 所以留空白
        private static string GetDIDocAutoPay()
        {
            StringBuilder text_di = new StringBuilder();
            //20181228 固定變更 update start
            //text_di.AppendLine("<?xml version=\"1.0\" encoding=\"big5\"?>");
            text_di.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            //text_di.AppendLine("<!DOCTYPE 函 SYSTEM \"99_2_big5.dtd\" [");
            text_di.AppendLine("<!DOCTYPE 函 SYSTEM \"104_2_utf8.dtd\" [");
            //20181228 固定變更 update start
            //text_di.AppendLine("<!ENTITY ATTCH1 SYSTEM \"{18}\" NDATA _X>");
            //text_di.AppendLine("<!ENTITY ATTCH1 SYSTEM \"{18}.txt\" NDATA _X>");
            //text_di.AppendLine("<!ENTITY ATTCH2 SYSTEM \"{0}.docx\" NDATA _X>");
            text_di.AppendLine("<!ENTITY 表單 SYSTEM \"{0}.sw\" NDATA DI>");
            text_di.AppendLine("<!NOTATION DI SYSTEM \"\">");
            text_di.AppendLine("<!NOTATION _X SYSTEM \"\">]>");
            text_di.AppendLine("<函>");
            text_di.AppendLine("<發文機關>");
            text_di.AppendLine("<全銜>{1}</全銜>");
            text_di.AppendLine("<機關代碼>{2}</機關代碼>");
            text_di.AppendLine("</發文機關>");
            text_di.AppendLine("<函類別 代碼=\"函\"/>");
            text_di.AppendLine("<地址>{3}</地址>");
            text_di.AppendLine("<聯絡方式>承辦人：{4}</聯絡方式>");
            text_di.AppendLine("<聯絡方式>電話：{5}</聯絡方式>");
            text_di.AppendLine("<聯絡方式>傳真：{17}</聯絡方式>");
            text_di.AppendLine("<受文者>");
            text_di.AppendLine("<交換表 交換表單=\"表單\">如正副本行文單位</交換表>");
            text_di.AppendLine("</受文者>");
            text_di.AppendLine("<發文日期>");
            text_di.AppendLine("<年月日>中華民國{6}</年月日>");
            text_di.AppendLine("</發文日期>");
            text_di.AppendLine("<發文字號>");
            text_di.AppendLine("<字>{7}</字>");
            text_di.AppendLine("<文號>");
            text_di.AppendLine("<年度>{8}</年度>");
            text_di.AppendLine("<流水號>{9}</流水號>");
            text_di.AppendLine("</文號>");
            text_di.AppendLine("</發文字號>");
            text_di.AppendLine("<速別 代碼=\"{10}\"/>");
            text_di.AppendLine("<密等及解密條件或保密期限>");
            text_di.AppendLine("<密等/>");
            text_di.AppendLine("<解密條件或保密期限></解密條件或保密期限>");
            text_di.AppendLine("</密等及解密條件或保密期限>");
            text_di.AppendLine("<附件>");
            text_di.AppendLine("<文字></文字>");
            text_di.AppendLine("</附件>");
            text_di.AppendLine("<主旨>");
            text_di.AppendLine("<文字>{11}</文字>");
            text_di.AppendLine("</主旨>");
            text_di.AppendLine("<段落 段名=\"說明：\">");
            text_di.AppendLine("<文字></文字>");
            text_di.AppendLine("<條列 序號=\"一、\">");
            text_di.AppendLine("<文字>{12}</文字>");
            text_di.AppendLine("</條列>");
            text_di.AppendLine("<條列 序號=\"二、\">");
            text_di.AppendLine("<文字>{13}</文字>");
            text_di.AppendLine("</條列>");
            text_di.AppendLine("<條列 序號=\"三、\">");
            text_di.AppendLine("<文字>{14}</文字>");
            text_di.AppendLine("</條列>");
            text_di.AppendLine("</段落>");
            text_di.AppendLine("<正本>");
            text_di.AppendLine("<全銜>{15}</全銜>");
            text_di.AppendLine("</正本>	");
            text_di.AppendLine("<副本>");
            text_di.AppendLine("<全銜>{16}</全銜>");
            text_di.AppendLine("</副本>");
            text_di.AppendLine("</函>");
            var output = text_di.ToString();
            return output;
        }

        private static DataTable GetDescTable(DataTable dtSendInfo)
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
    }
}
