using CTBC.CSFS.BussinessLogic;
using CTBC.CSFS.Pattern;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Text;
using CTBC.CSFS.Models;
using System.Text.RegularExpressions;
using System.Linq;
using NPOI;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.HSSF.Util;
using System.Windows.Forms;
using ICSharpCode.SharpZipLib.Zip;
using Microsoft.Reporting.WinForms;
using Microsoft.VisualBasic;
using System.Threading;
using System.Threading.Tasks;
using CTBC.FrameWork.Util;

namespace CTBC.WinExe.CSFS.NewHistoryFile
{
    class Program : BBRule
    {
        #region 全局變量

        // HTG/RFDM文件路徑
        private static string txtFilePath = ConfigurationManager.AppSettings["txtFilePath"];
        private static string txtZipPath = ConfigurationManager.AppSettings["txtZipPath"];
        //private string filePath = ConfigurationManager.AppSettings["PDFFilePath"].ToString();
        private static string pdfFilePath = ConfigurationManager.AppSettings["pdfFilePath"];
        private static string pdfZipPath = ConfigurationManager.AppSettings["pdfZipPath"];
        private static Int32 intRetry = Convert.ToInt32(ConfigurationManager.AppSettings["iRetry"]);
        private static Int32 intSleep = Convert.ToInt32(ConfigurationManager.AppSettings["iSleep"]);
        private static Int32 intTask = Convert.ToInt32(ConfigurationManager.AppSettings["iTask"]);
        private static string CheckATMFlag =  ConfigurationManager.AppSettings["Check81019"];
        private static string[] mailTo = ConfigurationManager.AppSettings["mailTo"].ToString().Split(',');
        // 獲取log路徑
        private static FileLog m_fileLog = new FileLog(ConfigurationManager.AppSettings["fileLog"], AppDomain.CurrentDomain.FriendlyName.Replace(".vshost", "").Replace(".exe", ""));

        // 半形空白變量
        public string strNull = " ";

        public DataTable CurrencyList = null;

        public object FillPatternType { get; private set; }
        #endregion

        /// <summary>
        /// 程序入口
        /// </summary>
        static void Main()
        {
            Program mainProgram = new Program();
            mainProgram.Process();
        }

        /// <summary>
        /// 主方法
        /// </summary>
        private void Process()
        {
            try
            {
                if (ConfigurationManager.AppSettings["WaitTimeForTest"] != null)
                {
                    int WaitTime = 0;
                    WaitTime = int.Parse(ConfigurationManager.AppSettings["WaitTimeForTest"].ToString());
                    m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.Information, string.Format("目前故意只是等待  {0}   秒!! ", WaitTime));
                    System.Threading.Thread.Sleep(1000 * WaitTime);
                }

                // 防止重覆執行....  
                string exeFile =  ConfigurationManager.AppSettings["ExeFile"];
                if (IsProcessRunning(exeFile))
                {
                    m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.Information, string.Format("前一支 {0} 正在執行中， 故本次不進行執行!! ", exeFile));
                    return;
                }
                DateTime thenow = DateTime.Now;
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, DateTime.Now.ToString());
                //20201120, 要避開營業日
                bool? isWorkDay = getWorkDay(thenow); // 應該要讀取前一天的工作日才對
                if (isWorkDay == null)
                {
                    m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "工作日設定為空 !!");
                    return;
                }
                else
                {
                    if (!(bool)isWorkDay) //假日
                    {
                        m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "非工作日 !!");
                        return;
                    }
                }
                //20211029 檢查81019Receive 是否有資料,只要有一筆就要去執行,否則Retry
                int isReturn = 0;

                for (int retry = 0; retry < intRetry; retry++)
                {
                    isReturn = check81019(DateTime.Now.ToString("yyyyMMdd"));
                    if (CheckATMFlag != "Y")
                    {
                        isReturn = 1;
                    }
                    //測試用 isReturn = 0;
                    if (isReturn < 1)
                    {
                        m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "81019 is No Data !!" + "Retry:" + (retry + 1).ToString());
                        Thread.Sleep(intSleep * 60 * 1000);
                    }
                    else
                    {
                        break;
                    }
                }
                //超過3次 1分鐘結束
                if (isReturn < 1)
                {
                    m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "Retry 結束  81019 No Data !!");
                    return;
                }

                //檢查是否有需要產檔資料
                DataTable dtGen = GetMultiData();
                ///增加多工處理將原本Status = 02 改成 66 ,多工改取 66
                if (dtGen != null && dtGen.Rows.Count > 0)
                {
                    UpdateMultiDataStatus();
                }
                else
                {
                    m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "查無資料需要產檔 !!");
                    return;
                }

                // 獲取幣別代碼檔資料-帳務外幣怕有用到先註記
                // CurrencyList = GetParmCodeCurrency();

                // 程序開始記入log
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------歷史紀錄產生開始------");
                // 讀取 Status = 66 本次產檔案件

                DataTable dt66 = GetMultiData66();
                string DocNo = string.Empty;

                if (dt66 != null && dt66.Rows.Count > 0)
                {

                    // 改為用MultiThread的方式來產生 .
                    // 預設ThreadPool 最高用10個Thread即可. 以免吃太多記憶體.

                    ThreadPool.SetMaxThreads(intTask, 1000);

                    List<Task> TaskList = new List<Task>();

                    foreach (DataRow dr in dt66.Rows)
                    {
                        m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------案號:" + dr["DocNo"].ToString());
                        List<string> obj = new List<string>() { dr["DocNo"].ToString() };
                        Task HistoryFileTask = Task.Run(() => GenerateDocNo(obj));
                        TaskList.Add(HistoryFileTask);
                        Thread.Sleep(1000);
                    }
                    Task.WaitAll(TaskList.ToArray());
                }

                // Task, ================= END
                // 暫時新增若有壓縮成功,先將狀態壓成功,允許下載,因為WEB 是以成功判斷下載
                int download66 = Download66Error();
                if (download66 > 0)
                {
                    m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------強制成功:" + download66 + "筆");
                }
                //
                // ZIP 產生成功 更新 為 狀態 03 成功 EXCEL 或 PDF 有一個Y就是成功,其他失敗
                int error66 = ModifyVersion66Error();
                if (error66 > 0)
                {
                    m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------強制結案:"+error66+"筆");
                }
                // 程序結束記入log
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, DateTime.Now.ToString());
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------產檔結束------");
            }
            catch (Exception ex)
            {
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "Process 錯誤:" + ex.Message.ToString());
                noticeMail(mailTo, "程式:NewHistoryFile-Process " +"錯誤: " + ex.Message.ToString());
            }
        }
        public bool IsProcessRunning(string name)
        {
            try
            {
                List<System.Diagnostics.Process> res = new List<System.Diagnostics.Process>();

                foreach (System.Diagnostics.Process clsProcess in System.Diagnostics.Process.GetProcesses())
                {
                    if (clsProcess.ProcessName.Contains(name))
                    {
                        res.Add(clsProcess);
                    }
                }

                //var c = res;如果大於1個, 代表排除自己後, 還有另一個在RUNING.. 
                return res.Count > 1;
            }
            catch (Exception ex)
            {
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------IsProcessRunning 錯誤: " + name + ex.Message.ToString());
                noticeMail(mailTo, "程式:NewHistoryFile-IsProcessRunning " + "錯誤: Name" + name + ex.Message.ToString());
                return false;
            }
        }
        private static void noticeMail(string[] mailFromTo, string InitMessage)
        {
            try
            {
                string mailFrom = ConfigurationManager.AppSettings["mailFrom"];
                string subject = ConfigurationManager.AppSettings["subject"];  
                string body = subject+"，錯誤原因：" + InitMessage;
                string host = ConfigurationManager.AppSettings["mailHost"];
                UtlMail.SendEmail(mailFrom, mailFromTo, subject, body, host);
            }
            catch (Exception ex)
            {
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------noticeMail無法傳送: 請查系統設定"+ex.Message.ToString() );
            }
        }
        public int check81019(string strDate)
        {
            try
            {
                
                string sql = @"SELECT count(NewId)
                                FROM  CaseCustATMRecv where DATA_DATE = '" + strDate + "' ";               
                
                
                var ret = base.Search(sql);
                return Convert.ToInt32(ret.Rows[0][0]);
            }
            catch (Exception ex)
            {
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "check81019錯誤:" + ex.Message.ToString());
                noticeMail(mailTo, "程式:NewHistoryFile-check81019 " + "錯誤: " + ex.Message.ToString());
                return -1;
            }

        }

        internal bool? getWorkDay(DateTime BizDate)
        {
            try
            {
                string SQL = "SELECT TOP 1 * FROM [dbo].[PARMWorkingDay] where date ='{0}' ";
                SQL = string.Format(SQL, BizDate.ToString("yyyy-MM-dd"));
                var d = base.SearchList<PARMWorkingDay>(SQL).FirstOrDefault();
                if (d != null)
                {
                    return d.Flag;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "getWorkDay錯誤:" + ex.Message.ToString());
                noticeMail(mailTo, "程式:NewHistoryFile-getWorkDay " + "錯誤: " + ex.Message.ToString());
                return null;
            }
        }




        #region HTG回文


        public void ExportHtgPdf(string strDocNo,NewFileLog newfilelog)
        {
            try
            {
                newfilelog.Write(NewFileLog.WorkType.Work, NewFileLog.ErrorType.None, "信息--------PDF讀取基本資料電文結果（PDF 存款帳戶資料）開始------");

                // 查詢產生電文檔案的資料
                DataTable dtHTGResult = GetHTGData(strDocNo);

                // 判斷是否有HTG電文資料
                if (dtHTGResult != null && dtHTGResult.Rows.Count > 0)
                {
                    DataTable dt401Recv = new DataTable();
                    string txtDocDirPath = "";
                    string DocNo = string.Empty;

                    // 將當日所有案號依案號順序產生基本資料
                    for (int p = 0; p < dtHTGResult.Rows.Count; p++)
                    {
                        try
                        {
                            // 依案號獲取存款帳戶開戶資料
                            dt401Recv = Get401RecvData(dtHTGResult.Rows[p]["CaseTrsNewID"].ToString());

                            if (DocNo != dtHTGResult.Rows[p]["DocNo"].ToString())
                            {
                                txtDocDirPath = pdfFilePath + "\\" + dtHTGResult.Rows[p]["DocNo"].ToString();
                                DocNo = dtHTGResult.Rows[p]["DocNo"].ToString();
                            }

                            // HTG&RFDM內容及對應資料筆數變量
                            int DataCount1 = 0;
                            string txtDocIDPath = "";
                            DataTable dt = new DataTable();
                            dt.Columns.Add("CUST_ID_NO");
                            dt.Columns.Add("CUSTOMER_NAME");
                            dt.Columns.Add("DATE_OF_BIRTH");
                            dt.Columns.Add("NIGTEL_NO");
                            dt.Columns.Add("REGTEL_NO");
                            dt.Columns.Add("MOBIL_NO");
                            dt.Columns.Add("CUST_ADD");
                            dt.Columns.Add("COMM_ADDR");
                            dt.Columns.Add("MASTER_NAME");
                            dt.Columns.Add("MASTER_ID");
                            dt.Columns.Add("IdNo02");
                            //dt.Columns.Add("BRANCH_NO");
                            //dt.Columns.Add("PD_TYPE_DESC");
                            //dt.Columns.Add("CURRENCY");
                            if (dt401Recv != null && dt401Recv.Rows.Count > 0)
                            {
                                // 拼接存款帳戶開戶資料
                                for (int q = 0; q < dt401Recv.Rows.Count; q++)
                                {
                                    //新版改用
                                    string SECDIR = "";
                                    if (dt401Recv.Rows[q]["CustID"].ToString().Length < 8)
                                    {
                                        SECDIR = dt401Recv.Rows[q]["CustAccount"].ToString();
                                    }
                                    else
                                    {
                                        SECDIR = dt401Recv.Rows[q]["ID_NO"].ToString();
                                    }
                                    txtDocIDPath = txtDocDirPath + "\\" + SECDIR;
                                    //txtDocIDPath = txtDocDirPath + "\\" + dt401Recv.Rows[q]["ID_NO"].ToString();
                                    if (!Directory.Exists(txtDocIDPath)) Directory.CreateDirectory(txtDocIDPath);
                                    #region 內容拼接

                                    DataRow strRow = dt.NewRow();
                                    // 身分證統一編號
                                    strRow["CUST_ID_NO"] = ChangeValue(dt401Recv.Rows[q]["ID_NO"].ToString(), 11);
                                    //strRow["CUST_ID_NO"] = ChangeValue(dt401Recv.Rows[q]["CUST_ID_NO"].ToString(), 11);
                                    // 開戶行總、分支機構代碼
                                    //strRow["BRANCH_NO"] = ChangeValue(dt401Recv.Rows[q]["BRANCH_NO"].ToString(), 7);
                                    // 存款種類	X(02)
                                    string strPD_TYPE_DESC = ChangeValue(dt401Recv.Rows[q]["PD_TYPE_DESC"].ToString(), 2);
                                    strPD_TYPE_DESC = (string.IsNullOrEmpty(strPD_TYPE_DESC.Trim()) ? "99" : strPD_TYPE_DESC);
                                    //strRow["PD_TYPE_DESC"] = strPD_TYPE_DESC;
                                    // 幣別 X(03)
                                    //strRow["CURRENCY"] = ChangeValue(dt401Recv.Rows[q]["CURRENCY"].ToString(), 3);
                                    // 戶名  X(60)
                                    strRow["CUSTOMER_NAME"] = ChangeChiness(dt401Recv.Rows[q]["CUSTOMER_NAME"].ToString(), 60);
                                    // 住家電話    X(20)
                                    strRow["REGTEL_NO"] = ChangeValue(dt401Recv.Rows[q]["REGTEL_NO"].ToString(), 20);
                                    // 住家電話    X(20)
                                    strRow["NIGTEL_NO"] = ChangeValue(dt401Recv.Rows[q]["NIGTEL_NO"].ToString(), 20);
                                    // 行動電話    X(20)
                                    strRow["MOBIL_NO"] = ChangeValue(dt401Recv.Rows[q]["MOBIL_NO"].ToString(), 20);
                                    // 戶籍地址    X(200)
                                    strRow["CUST_ADD"] = ChangeChiness(dt401Recv.Rows[q]["CUST_ADD"].ToString(), 200);
                                    // 通訊地址    X(200)
                                    strRow["COMM_ADDR"] = ChangeChiness(dt401Recv.Rows[q]["COMM_ADDR"].ToString(), 200);
                                    // 67050V6 67050V4
                                    strRow["DATE_OF_BIRTH"] = ChangeValue(dt401Recv.Rows[q]["DATE_OF_BIRTH"].ToString(), 12);
                                    strRow["MASTER_NAME"] = ChangeValue(dt401Recv.Rows[q]["MASTER_NAME"].ToString(), 20); ;//負責人
                                    strRow["MASTER_ID"] = ChangeValue(dt401Recv.Rows[q]["MASTER_ID"].ToString(), 20);//負責人
                                    strRow["IdNo02"] = ChangeValue("", 20);
                                    for (int n = 1; n < 11; n++)
                                    {
                                        string strIdType = dt401Recv.Rows[q]["ID_TYPE_" + n.ToString("00")].ToString().Trim();
                                        if (strIdType == "0003")
                                        {
                                            strRow["IdNo02"] = ChangeValue(dt401Recv.Rows[q]["Id_No_" + n.ToString("00")].ToString(), 20);//居留證
                                        }
                                    }
                                    //strRow["IdNo02"] = ChangeValue(dt401Recv.Rows[q]["Id_No_02"].ToString(), 20);//居留證
                                    bool bENg = IsEG(strRow["CUST_ID_NO"].ToString().Substring(strRow["CUST_ID_NO"].ToString().Length - 3, 3));
                                    if (bENg == true)
                                    {
                                        strRow["CUST_ID_NO"] = strRow["CUST_ID_NO"] + "/" + strRow["IdNo02"].ToString(); //"身分證統一編號/居留證號");
                                    }
                                    dt.Rows.Add(strRow);
                                    CreatePdf(dtHTGResult.Rows[p]["CustId"].ToString(), dt, txtDocIDPath);
                                    #endregion

                                    /// 更部狀態 20230119
                                    int HTGCount = UpdatePDFFILEStatus(dtHTGResult.Rows[p]["CaseTrsNewID"].ToString());
                                    if (HTGCount > 0)
                                    {
                                        newfilelog.Write(NewFileLog.WorkType.Work, NewFileLog.ErrorType.None, "信息--------更新基本資料產檔狀態完成");
                                    }
                                    else
                                    {
                                        newfilelog.Write(NewFileLog.WorkType.Work, NewFileLog.ErrorType.None, "信息--------更新基本資料產檔狀態異常");
                                    }
                                    /// 更新結束

                                    // 記錄LOG
                                    newfilelog.Write(NewFileLog.WorkType.Work, NewFileLog.ErrorType.None, "信息--------PDF :" + txtDocIDPath + "文件中增加基本資料!");
                                    dt.Clear();
                                    DataCount1++;
                                }

                            }
                            else
                            {
                                /// 更部狀態 20230119
                                int HTGCount = UpdatePDFFILEStatusNA(dtHTGResult.Rows[p]["CaseTrsNewID"].ToString());
                                if (HTGCount > 0)
                                {
                                    newfilelog.Write(NewFileLog.WorkType.Work, NewFileLog.ErrorType.None, "信息--------無基本資料產檔狀態更新完成");
                                }
                                else
                                {
                                    newfilelog.Write(NewFileLog.WorkType.Work, NewFileLog.ErrorType.None, "信息--------無基本資料產檔狀態更新異常");
                                }
                                /// 更新結束
                                dt.Clear();
                                newfilelog.Write(NewFileLog.WorkType.Work, NewFileLog.ErrorType.None, "DocNo:" + dtHTGResult.Rows[p]["DocNo"].ToString() + " CustID: " + dtHTGResult.Rows[p]["CustId"].ToString()  + "與本行無存款往來");
                            }

                            // 更新HTGSendStatus狀態
                            //if (dt401Recv != null && dt401Recv.Rows.Count > 0)
                            //{
                            //    if (dtHTGResult.Rows[p]["CaseTrsNewID"].ToString().Length > 5)
                            //    {
                            //        int HTGCount = UpdatePDFFILEStatus(dtHTGResult.Rows[p]["CaseTrsNewID"].ToString());

                            //        // 記錄LOG
                            //        if (HTGCount > 0)
                            //        {
                            //            newfilelog.Write(NewFileLog.WorkType.Work, NewFileLog.ErrorType.None, "信息--------PDF 基本資料產檔完成");
                            //        }
                            //        else
                            //        {
                            //            newfilelog.Write(NewFileLog.WorkType.Work, NewFileLog.ErrorType.None, "信息--------無PDF 基本資料產檔");
                            //        }
                            //    }
                            //    else
                            //    {
                            //        newfilelog.Write(NewFileLog.WorkType.Work, NewFileLog.ErrorType.None, "信息--------無PDF 基本資料產檔");
                            //    }
                            //}
                        }
                        catch (Exception ex)
                        {
                            newfilelog.Write(NewFileLog.WorkType.Work, NewFileLog.ErrorType.None, "信息--------ID:" + dtHTGResult.Rows[p]["CustId"].ToString() + "產檔錯誤:" + ex.Message.ToString());
                            noticeMail(mailTo, "程式:NewHistoryFile-Process " + "錯誤: ID" + dtHTGResult.Rows[p]["CustId"].ToString() + ex.Message.ToString());
                        }
                    }
                }
                else
                {
                    newfilelog.Write(NewFileLog.WorkType.Work, NewFileLog.ErrorType.None, "信息--------沒有查到PDF基本資料（存款帳戶開戶資料）");
                }

                newfilelog.Write(NewFileLog.WorkType.Work, NewFileLog.ErrorType.None, "信息--------匯出PDF基本資料（存款帳戶開戶資料）結束------");
            }
            catch (Exception ex)
            {
                newfilelog.Write(NewFileLog.WorkType.Work, NewFileLog.ErrorType.None, "ExportHtgPdf錯誤:" + ex.Message.ToString());
                noticeMail(mailTo, "程式:NewHistoryFile-ExportHtgPdf " + "錯誤: " + ex.Message.ToString());
            }
        }

        // zip
        public int PdfZip(string DocNo,NewFileLog newfilelog)
        {
            try
            {
                newfilelog.Write(NewFileLog.WorkType.Work, NewFileLog.ErrorType.None, "信息--------產生案號:" + DocNo + " Pdf zip 並上傳CaseEdocFile開始------");

                // 除了PDF DocNo 也要上傳到 CaseEdocFile ,一個案號可能有多NewId ,每個newid 都要上傳至CaseEdocFile提供網頁直接下載
                DataTable dtHTGzipResult = GetPDFzip(DocNo);

                /// 以基本資料為案號
                /// 
                string pdfpath = pdfFilePath;//pdfDocDirPath;//要壓縮檔案的目錄路徑
                if (dtHTGzipResult != null && dtHTGzipResult.Rows.Count > 0)
                {
                    for (int p = 0; p < dtHTGzipResult.Rows.Count; p++)
                    {
                        string pdfDocDirPath = pdfpath + "\\" + dtHTGzipResult.Rows[p]["DocNo"].ToString();
                        if (Directory.Exists(pdfDocDirPath))                     //如果当前是文件夹，递归
                        {
                            if (File.Exists(pdfDocDirPath + ".zip"))
                            {
                                File.Delete(pdfDocDirPath + ".zip");
                            }
                            CreateZip(pdfDocDirPath, pdfDocDirPath);
                            //}
                            // 上傳到CaseEdocFile
                            Guid caseId = (Guid)dtHTGzipResult.Rows[p]["CaseTrsNewID"];//改CaseID
                            string UploadFile = pdfDocDirPath + ".zip";
                            FileStream ZipFileStream = System.IO.File.OpenRead(UploadFile);
                            byte[] ZipBytes = new byte[ZipFileStream.Length];
                            ZipFileStream.Read(ZipBytes, 0, ZipBytes.Length);

                            CaseAccountBiz caseAccount = new CaseAccountBiz();
                            CaseEdocFile EdocFile = caseAccount.OpenPdf(caseId);
                            //
                            if (EdocFile != null)
                            {
                                DeleteCaseEdocFilePdf(caseId);
                            }
                            CaseEdocFile caseEdocFile = new CaseEdocFile();
                            caseEdocFile.CaseId = caseId;
                            caseEdocFile.Type = "歷史";
                            caseEdocFile.FileType = "pdf";
                            caseEdocFile.FileName = Path.GetFileName(pdfDocDirPath) + ".zip";
                            caseEdocFile.FileObject = ZipBytes;
                            caseEdocFile.SendNo = "";
                            InsertCaseEdocFile(caseEdocFile);
                            newfilelog.Write(NewFileLog.WorkType.Work, NewFileLog.ErrorType.None, "信息--------上傳PDF: " + dtHTGzipResult.Rows[p]["DocNo"].ToString() + "  成功------");
                        }
                    }
                    return 1;
                }
                else
                {
                    return 0;
                }
                
            }
            catch (Exception ex)
            {
                newfilelog.Write(NewFileLog.WorkType.Work, NewFileLog.ErrorType.None, "PdfZip錯誤:" + DocNo + ex.Message.ToString());
                noticeMail(mailTo, "程式:NewHistoryFile-PdfZip " + "錯誤: " + DocNo+ex.Message.ToString());
                return 0;
            }
        }

        
        public int ExcelZip(string DocNo,NewFileLog newfilelog)
        {
            try
            {
                newfilelog.Write(NewFileLog.WorkType.Work, NewFileLog.ErrorType.None, "信息--------產生案號:" + DocNo + " Excel zip 並上傳CaseEdocFile開始------");

                // 除了ZIP DocNo 也要上傳到 CaseEdocFile ,一個案號可能有多NewId ,每個newid 都要上傳至CaseEdocFile提供網頁直接下載
                DataTable dtHTGzipResult = GetExcelzip(DocNo);

                /// 以基本資料為案號
                /// 
                string zippath = txtFilePath;//txtDocDirPath;//要壓縮檔案的目錄路徑
                if (dtHTGzipResult != null && dtHTGzipResult.Rows.Count > 0)
                {
                    for (int p = 0; p < dtHTGzipResult.Rows.Count; p++)
                    {
                        string txtDocDirPath = zippath + "\\" + dtHTGzipResult.Rows[p]["DocNo"].ToString();
                        if (Directory.Exists(txtDocDirPath))                     
                        {
                            if (File.Exists(txtDocDirPath + ".zip"))
                            {
                                File.Delete(txtDocDirPath + ".zip");
                            }
                            CreateZip(txtDocDirPath, txtDocDirPath);
                            // 上傳到CaseEdocFile
                            Guid caseId = (Guid)dtHTGzipResult.Rows[p]["CaseTrsNewID"];//改CaseID
                            string UploadFile = txtDocDirPath + ".zip";
                            FileStream ZipFileStream = System.IO.File.OpenRead(UploadFile);
                            byte[] ZipBytes = new byte[ZipFileStream.Length];
                            ZipFileStream.Read(ZipBytes, 0, ZipBytes.Length);
                            CaseAccountBiz caseAccount = new CaseAccountBiz();
                            CaseEdocFile EdocFile = caseAccount.OpenExcel(caseId);
                            //
                            if (EdocFile != null)
                            {
                                DeleteCaseEdocFileExcel(caseId);
                            }

                            CaseEdocFile caseEdocFile = new CaseEdocFile();
                            caseEdocFile.CaseId = caseId;
                            caseEdocFile.Type = "歷史";
                            caseEdocFile.FileType = "xlsx";
                            caseEdocFile.FileName = Path.GetFileName(txtDocDirPath) + ".zip";
                            caseEdocFile.FileObject = ZipBytes;
                            caseEdocFile.SendNo = "";
                            InsertCaseEdocFile(caseEdocFile);
                            newfilelog.Write(NewFileLog.WorkType.Work, NewFileLog.ErrorType.None, "信息--------上傳EXCEL :" + dtHTGzipResult.Rows[p]["DocNo"].ToString() + "  成功------");
                        }                     
                    }
                    return 1;
                }
                else
                {
                   return 0;
                }
            }
            catch (Exception ex)
            {
                newfilelog.Write(NewFileLog.WorkType.Work, NewFileLog.ErrorType.None, "ExcelZip錯誤:" + DocNo + ex.Message.ToString());
                noticeMail(mailTo, "程式:NewHistoryFile-ExcelZip " + "錯誤: " + DocNo+ex.Message.ToString());
                return 0;
            }
        }
        // 產生EXCEL 
        public int ExportHtgExcel(string strDocNo,NewFileLog newfilelog)
        {
            try
            {
                int iSuccess = 0;
                newfilelog.Write(NewFileLog.WorkType.Work, NewFileLog.ErrorType.None, "信息--------讀取EXCEL電文-基本資料（存款帳戶開戶資料）開始------");

                // 查詢產生電文檔案的資料
                DataTable dtHTGResult = GetHTGData(strDocNo);

                // 判斷是否有HTG電文資料
                if (dtHTGResult != null && dtHTGResult.Rows.Count > 0)
                {
                    DataTable dt401Recv = new DataTable();
                    string txtDocDirPath = "";
                    string DocNo = string.Empty;

                    // 遍歷HTGResult,將資料寫進對應的txt文件中
                    for (int p = 0; p < dtHTGResult.Rows.Count; p++)
                    {
                        try
                        {
                            // 獲取存款帳戶開戶資料
                            dt401Recv = Get401RecvData(dtHTGResult.Rows[p]["CaseTrsNewID"].ToString());

                            if (DocNo != dtHTGResult.Rows[p]["DocNo"].ToString())
                            {
                                txtDocDirPath = txtFilePath + "\\" + dtHTGResult.Rows[p]["DocNo"].ToString();
                                DocNo = dtHTGResult.Rows[p]["DocNo"].ToString();
                            }

                            // HTG&RFDM內容及對應資料筆數變量
                            int DataCount1 = 0;
                            string txtDocIDPath = "";
                            DataTable dt = new DataTable();
                            dt.Columns.Add("CUST_ID_NO");
                            dt.Columns.Add("CUSTOMER_NAME");
                            dt.Columns.Add("DATE_OF_BIRTH");
                            dt.Columns.Add("NIGTEL_NO");
                            dt.Columns.Add("REGTEL_NO");
                            dt.Columns.Add("MOBIL_NO");
                            dt.Columns.Add("CUST_ADD");
                            dt.Columns.Add("COMM_ADDR");
                            dt.Columns.Add("MASTER_NAME");
                            dt.Columns.Add("MASTER_ID");
                            dt.Columns.Add("IdNo02");
                            if (dt401Recv != null && dt401Recv.Rows.Count > 0)
                            {
                                // 拼接存款帳戶開戶資料
                                for (int q = 0; q < dt401Recv.Rows.Count; q++)
                                {

                                    //新版改用
                                    string SECDIR = "";
                                    if (dt401Recv.Rows[q]["CustID"].ToString().Length < 8)
                                    {
                                        SECDIR = dt401Recv.Rows[q]["CustAccount"].ToString();
                                    }
                                    else
                                    {
                                        SECDIR = dt401Recv.Rows[q]["ID_NO"].ToString();
                                    }
                                    txtDocIDPath = txtDocDirPath + "\\" + SECDIR;
                                    if (!Directory.Exists(txtDocIDPath)) Directory.CreateDirectory(txtDocIDPath);
                                    #region 內容拼接

                                    DataRow strRow = dt.NewRow();
                                    // 身分證統一編號
                                    strRow["CUST_ID_NO"] = ChangeValue(dt401Recv.Rows[q]["ID_NO"].ToString(), 11);
                                    // 開戶行總、分支機構代碼
                                    //strRow["BRANCH_NO"] = ChangeValue(dt401Recv.Rows[q]["BRANCH_NO"].ToString(), 7);
                                    // 存款種類	X(02)
                                    string strPD_TYPE_DESC = ChangeValue(dt401Recv.Rows[q]["PD_TYPE_DESC"].ToString(), 2);
                                    strPD_TYPE_DESC = (string.IsNullOrEmpty(strPD_TYPE_DESC.Trim()) ? "99" : strPD_TYPE_DESC);
                                    //strRow["PD_TYPE_DESC"] = strPD_TYPE_DESC;
                                    // 幣別 X(03)
                                    //strRow["CURRENCY"] = ChangeValue(dt401Recv.Rows[q]["CURRENCY"].ToString(), 3);
                                    // 戶名  X(60)
                                    strRow["CUSTOMER_NAME"] = ChangeChiness(dt401Recv.Rows[q]["CUSTOMER_NAME"].ToString(), 60);
                                    // 住家電話    X(20)
                                    strRow["REGTEL_NO"] = ChangeValue(dt401Recv.Rows[q]["REGTEL_NO"].ToString(), 20);
                                    // 住家電話    X(20)
                                    strRow["NIGTEL_NO"] = ChangeValue(dt401Recv.Rows[q]["NIGTEL_NO"].ToString(), 20);
                                    // 行動電話    X(20)
                                    strRow["MOBIL_NO"] = ChangeValue(dt401Recv.Rows[q]["MOBIL_NO"].ToString(), 20);
                                    // 戶籍地址    X(200)
                                    strRow["CUST_ADD"] = ChangeChiness(dt401Recv.Rows[q]["CUST_ADD"].ToString(), 200);
                                    // 通訊地址    X(200)
                                    strRow["COMM_ADDR"] = ChangeChiness(dt401Recv.Rows[q]["COMM_ADDR"].ToString(), 200);
                                    strRow["DATE_OF_BIRTH"] = ChangeValue(dt401Recv.Rows[q]["DATE_OF_BIRTH"].ToString(), 12);
                                    strRow["MASTER_NAME"] = ChangeValue(dt401Recv.Rows[q]["MASTER_NAME"].ToString(), 20); ;//負責人
                                    strRow["MASTER_ID"] = ChangeValue(dt401Recv.Rows[q]["MASTER_ID"].ToString(), 20);//負責人
                                                                                                                     //strRow["IdNo02"] = ChangeValue(dt401Recv.Rows[q]["Id_No_02"].ToString(), 20);//居留證
                                    strRow["IdNo02"] = ChangeValue("", 20);
                                    for (int n = 1; n < 11; n++)
                                    {
                                        string strIdType = dt401Recv.Rows[q]["ID_TYPE_" + n.ToString("00")].ToString().Trim();
                                        if (strIdType == "0003")
                                        {
                                            strRow["IdNo02"] = ChangeValue(dt401Recv.Rows[q]["Id_No_" + n.ToString("00")].ToString(), 20);//居留證
                                        }
                                    }
                                    //strRow["IdNo02"] = ChangeValue(dt401Recv.Rows[q]["Id_No_02"].ToString(), 20);//居留證
                                    dt.Rows.Add(strRow);
                                    CreateExcel(dtHTGResult.Rows[p]["CustId"].ToString(), dt, txtDocIDPath);
                                    #endregion
                                    /// 更部狀態 20230119
                                    int HTGCount = UpdateEXCELFILEStatus(dtHTGResult.Rows[p]["CaseTrsNewID"].ToString());
                                    if (HTGCount > 0)
                                    {
                                        iSuccess = 1;
                                        newfilelog.Write(NewFileLog.WorkType.Work, NewFileLog.ErrorType.None, "信息--------更新基本資料EXCEL產檔狀態完成");
                                    }
                                    else
                                    {
                                        iSuccess = 0;
                                        newfilelog.Write(NewFileLog.WorkType.Work, NewFileLog.ErrorType.None, "信息--------更新基本資料EXCEL產檔狀態異常");
                                    }
                                    /// 更新結束
                                    // 記錄LOG
                                    newfilelog.Write(NewFileLog.WorkType.Work, NewFileLog.ErrorType.None, "信息--------向" + txtDocIDPath + "EXCEL文件中增加基本資料!");
                                    dt.Clear();
                                    DataCount1++;
                                }
                            }
                            else
                            {
                                /// 更部狀態 20230119
                                int HTGCount = UpdateEXCELFILEStatusNA(dtHTGResult.Rows[p]["CaseTrsNewID"].ToString());
                                if (HTGCount > 0)
                                {
                                    newfilelog.Write(NewFileLog.WorkType.Work, NewFileLog.ErrorType.None, "信息--------更新無基本資料EXCEL產檔狀態完成");
                                }
                                else
                                {
                                    newfilelog.Write(NewFileLog.WorkType.Work, NewFileLog.ErrorType.None, "信息--------更新無基本資料EXCEL產檔狀態異常");
                                }
                                /// 更新結束
                                dt.Clear();
                                newfilelog.Write(NewFileLog.WorkType.Work, NewFileLog.ErrorType.None, "DocNo:" + dtHTGResult.Rows[p]["DocNo"].ToString() + " CustID: " + dtHTGResult.Rows[p]["CustId"].ToString()  + "與本行無存款往來");
                            }

                        }
                        catch (Exception ex)
                        {
                            newfilelog.Write(NewFileLog.WorkType.Work, NewFileLog.ErrorType.None, "信息--------ID:" + dtHTGResult.Rows[p]["CustId"].ToString() + "產檔錯誤:" + ex.Message.ToString());
                            noticeMail(mailTo, "程式:NewHistoryFile-ExportHtgExcel  CustID:" + dtHTGResult.Rows[p]["CustId"].ToString() + "產檔錯誤:"+ ex.Message.ToString());
                        }
                    }
                }
                else
                {
                    newfilelog.Write(NewFileLog.WorkType.Work, NewFileLog.ErrorType.None, "信息--------沒有查到匯出（存款帳戶開戶資料）");
                }
                newfilelog.Write(NewFileLog.WorkType.Work, NewFileLog.ErrorType.None, "信息--------匯出EXECL基本資料（存款帳戶開戶資料）結束------");
                return iSuccess;
            }
            catch (Exception ex)
            {
                newfilelog.Write(NewFileLog.WorkType.Work, NewFileLog.ErrorType.None, "信息--------ExportHtgExcel:" + ex.Message.ToString());
                noticeMail(mailTo, "程式:NewHistoryFile-ExportHtgExcel " + "錯誤: " + ex.Message.ToString());
                return 0;
            }
        }

        public DataTable GetExcelzip(string DocNo)
        {
            try
            {
                CommandParameterCollection CPC = new CommandParameterCollection();
                string sqlSelect = @"
                    select distinct CaseTrsNewID,CaseTrsQueryVersion.DocNo as DocNo  
                        from CaseTrsQueryVersion
                    where CaseTrsQueryVersion.DocNo = @DocNoGetExcelzip and CONVERT(VARCHAR(10),CaseTrsQueryVersion.ModifiedDate,111) = CONVERT(VARCHAR(10),getdate(),111) ;";

                // 清空容器
                CPC.Clear();
                CPC.Add(new CommandParameter("@DocNoGetExcelzip", DocNo));
                return base.Search(sqlSelect, CPC);
                //base.Parameter.Clear();
                //base.Parameter.Add(new CommandParameter("@DocNoGetExcelzip", DocNo));
                //return base.Search(sqlSelect);
            }
            catch (Exception ex)
            {
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------GetExcelzip:" +DocNo + ex.Message.ToString());
                noticeMail(mailTo, "程式:NewHistoryFile- GetExcelzip " + "錯誤: " + DocNo+ ex.Message.ToString());
                return null;
            }

        }

        public DataTable GetPDFzip(string DocNo)
        {
            try
            {
                CommandParameterCollection CPC = new CommandParameterCollection();
                string sqlSelect = @"
                              select distinct CaseTrsNewID,CaseTrsQueryVersion.DocNo as DocNo  
                                from CaseTrsQueryVersion
                                where CaseTrsQueryVersion.DocNo = @DocNoGetPDFzip  and CONVERT(VARCHAR(10),CaseTrsQueryVersion.ModifiedDate,111) = CONVERT(VARCHAR(10),getdate(),111)";
                // 清空容器
                CPC.Clear();
                CPC.Add(new CommandParameter("@DocNoGetPDFzip", DocNo));
                return base.Search(sqlSelect, CPC);
                //base.Parameter.Clear();
                //base.Parameter.Add(new CommandParameter("@DocNoGetPDFzip", DocNo));
                //return base.Search(sqlSelect);
            }
            catch (Exception ex)
            {
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------GetPDFzip:" + DocNo + ex.Message.ToString());
                noticeMail(mailTo, "程式:NewHistoryFile- GetPDFzip " + "錯誤: " + DocNo + ex.Message.ToString());
                return null;
            }
        }

   

        public int UpdateMultiDataStatus()
        {
            try
            {
                string sql = @"Update  CaseTrsQueryVersion set Status = '66'
                                where NewId in (  select distinct CaseTrsQueryVersion.NewId  FROM CaseTrsQueryVersion
                                where CaseTrsQueryVersion.Status ='02' and   HTGSendStatus  in ('4','6') )
                                     ";
                // 清空容器
                base.Parameter.Clear();
                return base.ExecuteNonQuery(sql);
            }
            catch (Exception ex)
            {
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "UpdateMultiDataStatus錯誤:" + ex.Message.ToString());
                noticeMail(mailTo, "程式:NewHistoryFile-UpdateMultiDataStatus SQL" + "錯誤: " + ex.Message.ToString());
                return -1;
            }
        }
        /// <summary>
        /// 查詢產生回文檔案的資料
        /// </summary>
        /// <returns></returns>
 
        public DataTable GetHTGData(string DocNo)
        {
            try
            {
                CommandParameterCollection CPC = new CommandParameterCollection();
                string sqlSelect = @"select CustId,NewID as CaseTrsNewID,CaseTrsQueryVersion.docNo as DocNo
                                    from  CaseTrsQueryVersion
                                    where  CaseTrsQueryVersion.docNo = @DocNo and Status='66' ";
                // 清空容器
                CPC.Clear();
                CPC.Add(new CommandParameter("@DocNo", DocNo));
                return base.Search(sqlSelect,CPC);
            }
            catch (Exception ex)
            {
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------GetHTGData " + "錯誤: 案號 :" + DocNo + ":" + ex.Message.ToString());
                noticeMail(mailTo, "程式:NewHistoryFile-GetHTGData " + "錯誤: 案號 :" + DocNo + ":" + ex.Message.ToString());
                return null;
            }
        }

        /// <summary>
        /// 查詢HTG回文txt資料
        /// </summary>
        /// <param name="VersionNewID"></param>
        /// <returns></returns>
        public DataTable Get401RecvData(string VersionNewID)
        {
            try
            {
                CommandParameterCollection CPC = new CommandParameterCollection();
                string sqlSelect = @"     SELECT 	BOPS000401Recv.CUST_ID_NO   -- 統一編號
                                	,CASE 
                                        WHEN ISNULL(BOPS000401Recv.BRANCH_NO,'') <> '' THEN '822' + BOPS000401Recv.BRANCH_NO 
                                        ELSE ''
                                    END BRANCH_NO --分行別
                                	,isnull( PARMCode.CodeNo,'99') as  PD_TYPE_DESC--產品別
                                	,BOPS000401Recv.CURRENCY --幣別
                                    ,BOPS067050Recv.CUST_ID_NO  as   ID_NO -- 客戶ID
                                	,BOPS067050Recv.CUSTOMER_NAME --戶名
                                	,BOPS067050Recv.NIGTEL_NO --晚上電話
                                	,BOPS067050Recv.MOBIL_NO --手機號碼
                                    ,BOPS067050Recv.REGTEL_NO --戶籍電話
                                    ,CONVERT(VARCHAR,CONVERT(int,substring(BOPS067050Recv.DATE_OF_BIRTH,5,4))-1911)+ '/'+substring(BOPS067050Recv.DATE_OF_BIRTH,3,2)+'/'+substring(BOPS067050Recv.DATE_OF_BIRTH,1,2)  as DATE_OF_BIRTH --生日
                                	,isnull(BOPS067050Recv.POST_CODE,'') + ' ' 
                                	+isnull(BOPS067050Recv.COMM_ADDR1,'')
                                	+isnull(BOPS067050Recv.COMM_ADDR2,'')
                                	+isnull(BOPS067050Recv.COMM_ADDR3,'') as COMM_ADDR--通訊地址
                                	,isnull(BOPS067050Recv.ZIP_CODE,'') + ' ' + 
                                	isnull(BOPS067050Recv.CUST_ADD1,'') +
                                	isnull(BOPS067050Recv.CUST_ADD2,'') +
                                	isnull(BOPS067050Recv.CUST_ADD3,'') as CUST_ADD--戶籍/證照地址
                                	,BOPS000401Recv.ACCT_NO --帳號
                                	,CASE 
                                      WHEN LEN(BOPS000401Recv.OPEN_DATE) > 0 
                                           and BOPS000401Recv.OPEN_DATE <>'00/00/0000'
                                      THEN substring(BOPS000401Recv.OPEN_DATE,7,4)
                                      + substring(BOPS000401Recv.OPEN_DATE,4, 2)
                                      +substring(BOPS000401Recv.OPEN_DATE,1, 2) ELSE '' END as OPEN_DATE --開戶日
                                	,CASE
                                		 WHEN LEN(BOPS000401Recv.CLOSE_DATE) > 0 
                                            and BOPS000401Recv.CLOSE_DATE <> '00/00/0000'
                                		 THEN substring(BOPS000401Recv.CLOSE_DATE,7,4)
                                		 + substring(BOPS000401Recv.CLOSE_DATE,4, 2)
                                		 +substring(BOPS000401Recv.CLOSE_DATE,1, 2) ELSE '' END as CLOSE_DATE --結清日
                                	,BOPS000401Recv.CUR_BAL --目前餘額
                                	,BOPS000401Recv.ACCT_NO --帳號
									--,BOPS067050V4Recv.ID_NO_02 -- 居留證號
                                    ,BOPS067050V4Recv.[ID_TYPE_01],BOPS067050V4Recv.[ID_NO_01],
                                    BOPS067050V4Recv.[ID_TYPE_02],BOPS067050V4Recv.[ID_NO_02],
                                    BOPS067050V4Recv.[ID_TYPE_03],BOPS067050V4Recv.[ID_NO_03],
                                    BOPS067050V4Recv.[ID_TYPE_04],BOPS067050V4Recv.[ID_NO_04],
                                    BOPS067050V4Recv.[ID_TYPE_05],BOPS067050V4Recv.[ID_NO_05],
                                    BOPS067050V4Recv.[ID_TYPE_06],BOPS067050V4Recv.[ID_NO_06],
                                    BOPS067050V4Recv.[ID_TYPE_07],BOPS067050V4Recv.[ID_NO_07],
                                    BOPS067050V4Recv.[ID_TYPE_08],BOPS067050V4Recv.[ID_NO_08],
                                    BOPS067050V4Recv.[ID_TYPE_09],BOPS067050V4Recv.[ID_NO_09],
                                    BOPS067050V4Recv.[ID_TYPE_10],BOPS067050V4Recv.[ID_NO_10]
									,BOPS067050V6Recv.RESP_CUST_ID as MASTER_ID --負責人ID
									,BOPS067050V6Recv.RESP_NAME  as MASTER_NAME --負責人姓名	
                                    ,CaseTrsQueryVersion.CustId,CaseTrsQueryVersion.CustAccount	-- 目錄產檔										 
                                FROM CaseTrsQueryVersion inner join
								BOPS067050Recv on CaseTrsQueryVersion.NewId = BOPS067050Recv.VersionNewID
								left join BOPS000401Recv
								     on BOPS067050Recv.VersionNewID = BOPS000401Recv.VersionNewID and BOPS000401Recv.ACCT_NO = BOPS067050Recv.CIF_NO
								LEFT join BOPS067050V4Recv 
								    on BOPS067050Recv.VersionNewID = BOPS067050V4Recv.VersionNewID and BOPS067050V4Recv.CUST_ID_NO=BOPS067050V4Recv.CUST_ID_NO
								LEFT join BOPS067050V6Recv  
								    on BOPS067050Recv.VersionNewID = BOPS067050V6Recv.VersionNewID
							    left join PARMCode on PARMCode.CodeType = 'PD_TYPE_DESC'
				                        and PARMCode.CodeMemo = BOPS000401Recv.PD_TYPE_DESC
                                WHERE BOPS067050Recv.VersionNewID = @VersionNewID ";
                // 清空容器
                CPC.Clear();
                CPC.Add(new CommandParameter("@VersionNewID", VersionNewID));
                return base.Search(sqlSelect, CPC);
                //base.Parameter.Clear();
                //base.Parameter.Add(new CommandParameter("@VersionNewID", VersionNewID));
                //return base.Search(sqlSelect);

            }
            catch (Exception ex)
            {
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------Get401RecvData" + "錯誤: 案號 :" + VersionNewID + ":" + ex.Message.ToString());
                noticeMail(mailTo, "程式:NewHistoryFile-Get401RecvData" + "錯誤: NewId :" + VersionNewID + ":" + ex.Message.ToString());
                return null;
            }
        }


     
        #endregion

        #region RFDM

        public void ExportRFDMPdf(string strDocNo,NewFileLog newfilelog)
        {
            try
            {
                newfilelog.Write(NewFileLog.WorkType.Work, NewFileLog.ErrorType.None, "信息--------產生歷史交易PDF（存款往來明細資料）開始------");
                // 查詢產生表頭檔案的資料
                DataTable dtResult = GetRFDMData(strDocNo);

                string DocNo = string.Empty;

                if (dtResult.Rows.Count > 0)
                {
                        newfilelog.Write(NewFileLog.WorkType.Work, NewFileLog.ErrorType.None, "信息--------案號:" + strDocNo + "產檔開始");
                        GenerateDetailsPDF(strDocNo, dtResult, newfilelog);
                }
                else
                {
                    newfilelog.Write(NewFileLog.WorkType.Work, NewFileLog.ErrorType.None, "信息--------沒有PDF（存款往來明細資料）");
                }
                newfilelog.Write(NewFileLog.WorkType.Work, NewFileLog.ErrorType.None, "信息--------產出歷史交易PDF（存款往來明細資料）結束------");
            }
            catch (Exception ex)
            {
                newfilelog.Write(NewFileLog.WorkType.Work, NewFileLog.ErrorType.None, "信息--------ExportRFDMPdf:" + strDocNo + ex.Message.ToString());
                noticeMail(mailTo, "程式:NewHistoryFile-ExportRFDMPdf " + "錯誤: 案號 :" + strDocNo +":"+ ex.Message.ToString());
            }
        }


        void GenerateDetailsPDF(string strDocNo, DataTable drRecvAccount, NewFileLog newfilelog)
		{
            try
            {
                string DocNo = strDocNo;
                newfilelog.Write(NewFileLog.WorkType.Work, NewFileLog.ErrorType.None, "-------建立PDF----:" + strDocNo);

                string txtDocDirPath = pdfFilePath + "\\" + strDocNo;
                string txtDocIDPath = string.Empty;

                string Acct_DateFm_DateTo = "";


                // 遍歷並追加txt文件
                #region 建立dt
                DataTable dt = new DataTable();
                dt.Columns.Add("CUST_ID_NO");
                dt.Columns.Add("ACCT_NO");
                dt.Columns.Add("TRAN_DATE");
                dt.Columns.Add("JNRST_TIME");
                dt.Columns.Add("TRAN_BRANCH");
                dt.Columns.Add("JRNL_NO");
                dt.Columns.Add("TRANS_CODE");
                dt.Columns.Add("TXN_DESC");
                dt.Columns.Add("TRAN_AMT");
                dt.Columns.Add("SaveAMT");
                dt.Columns.Add("BALANCE");
                //
                dt.Columns.Add("Currency");
                dt.Columns.Add("ATM_NO");
                dt.Columns.Add("TRF_BANK");
                //81019
                dt.Columns.Add("YBTXLOG_SRC_ID");
                dt.Columns.Add("YBTXLOG_STAND_NO");
                dt.Columns.Add("YBTXLOG_SAFE_TMNL_ID");
                dt.Columns.Add("YBTXLOG_IC_MEMO_CARDNO");
                dt.Columns.Add("Member_No");//會員編號
                                            //
                dt.Columns.Add("CHQ_NO");//票號
                                         //adam 20221118

                dt.Columns.Add("NARRATIVE");//備註
                dt.Columns.Add("REMARK");// 註記
                                         //81019 附言再確認
                dt.Columns.Add("ADD_NARRATIVE");
                //
                dt.Columns.Add("TELLER");
                dt.Columns.Add("CUSTOMER_NAME");
                dt.Columns.Add("PD_TYPE_DESC");
                dt.Columns.Add("QDateS");
                dt.Columns.Add("QDateE");
                dt.Columns.Add("PrintDate");
                dt.Columns.Add("CreatedUser");
                #endregion

                if (drRecvAccount.Rows.Count > 0)
                {
                    for (int a = 0; a < drRecvAccount.Rows.Count; a++)
                    {
                        Acct_DateFm_DateTo = drRecvAccount.Rows[a]["ACCT_NO"].ToString() + "-" + drRecvAccount.Rows[a]["QDateS"].ToString() + "-" + drRecvAccount.Rows[a]["QDateE"].ToString();
                        DataTable drAccountData = GetRFDMRecvData(drRecvAccount.Rows[a]["CaseTrsNewID"].ToString(), drRecvAccount.Rows[a]["ACCT_NO"].ToString(), drRecvAccount.Rows[a]["QDateS"].ToString(), drRecvAccount.Rows[a]["QDateE"].ToString(), DateTime.Now.ToString("yyyyMMdd"));
                        // 2023 如果ATM有資料則 JNRST_TIME用ＡＴＭ＿ＴＩＭＥ=YBTXLOG_TXN_HHMMSS 
                        for (int p = 0; p < drAccountData.Rows.Count; p++)
                        {

                            if (!String.IsNullOrEmpty(drAccountData.Rows[p]["ATM_TIME"].ToString()))
                            {
                                    drAccountData.Rows[p]["JNRST_TIME"] = drAccountData.Rows[p]["ATM_TIME"].ToString();
                            }
                            //
                        }
                        //以規格書重新排序,可能會調依實際需求
                        DataView dv = drAccountData.DefaultView;
                        dv.Sort = "CUST_ID_NO,ACCT_NO,TRAN_DATE,JNRST_TIME";
                        drAccountData = dv.ToTable();
                        for (int j = 0; j < drAccountData.Rows.Count; j++)
                        {
                            //新版改用
                            string SECDIR = "";
                            if (drAccountData.Rows[j]["CustID"].ToString().Length < 8)
                            {
                                SECDIR = drAccountData.Rows[j]["CustAccount"].ToString();
                            }
                            else
                            {
                                SECDIR = drAccountData.Rows[j]["CUST_ID_NO"].ToString();
                            }
                            txtDocIDPath = txtDocDirPath + "\\" + SECDIR;
                            //txtDocIDPath = txtDocDirPath + "\\" + drAccountData.Rows[j]["CUST_ID_NO"].ToString();
                            if (!Directory.Exists(txtDocIDPath)) Directory.CreateDirectory(txtDocIDPath);
                            #region 拼接內容
                            DataRow strRow = dt.NewRow();
                            // 身分證統一編號 X(10)
                            strRow["CUST_ID_NO"] = ChangeValue(drAccountData.Rows[j]["CUST_ID_NO"].ToString(), 11);
                            strRow["ACCT_NO"] = ChangeAccount(drAccountData.Rows[j]["ACCT_NO"].ToString(), 20);
                            // 交易序號    9(08)
                            strRow["JRNL_NO"] = ChangeValue(drAccountData.Rows[j]["JRNL_NO"].ToString(), 8);
                            // 交易日期    X(08)
                            strRow["TRAN_DATE"] = ChangeValue(drAccountData.Rows[j]["TRAN_DATE"].ToString(), 8);
                            // 交易時間    X(08) 
                            //if (drAccountData.Rows[j]["JNRST_TIME"].ToString().Length < 8)
                            //{
                            //    strRow["JNRST_TIME"] = ChangeValue(drAccountData.Rows[j]["RFDM_HHMMSS"].ToString(), 8);//增加: 所以取8碼
                            //}
                            //else
                            if (drAccountData.Rows[j]["JNRST_TIME"].ToString().Length > 5) drAccountData.Rows[j]["JNRST_TIME"] = drAccountData.Rows[j]["JNRST_TIME"].ToString().Substring(0, 2) + ":" + drAccountData.Rows[j]["JNRST_TIME"].ToString().Substring(2, 2) + ":" + drAccountData.Rows[j]["JNRST_TIME"].ToString().Substring(4, 2);
                            strRow["JNRST_TIME"] = ChangeValue(drAccountData.Rows[j]["JNRST_TIME"].ToString(), 8);//增加: 所以取8碼
                                                                                                                  // 交易行(或所屬分行代號)    X(07)
                            strRow["TRAN_BRANCH"] = ChangeValue(drAccountData.Rows[j]["TRAN_BRANCH"].ToString(), 7);
                            // 交易櫃員    X(05)
                            strRow["TRANS_CODE"] = ChangeValue(drAccountData.Rows[j]["TRANS_CODE"].ToString(), 5);
                            // 交易摘要    X(40)
                            strRow["TXN_DESC"] = ChangeChiness(drAccountData.Rows[j]["TXN_DESC"].ToString(), 40);
                            // 支出金額    X(16)
                            // 20180119,PeterHsieh : CTBC修改規格為，當金額為負數就放在支出金額，為正就放在存入金額，另一金額欄位就放'+0'
                            string strTRAN_AMT = drAccountData.Rows[j]["TRAN_AMT"].ToString();
                            strTRAN_AMT = strTRAN_AMT.Contains("-") ? strTRAN_AMT : "+0.00";
                            //20180622 RC 線上投單CR修正 宏祥 add start
                            string strTRAN_AMT2 = ChangeNumber(strTRAN_AMT, 16, 2, false);
                            //strRow["TRAN_AMT"] = strTRAN_AMT2.Contains("-") ? strTRAN_AMT2.Replace("-", "+") : strTRAN_AMT2;
                            strRow["TRAN_AMT"] = ChangeValue(drAccountData.Rows[j]["TRAN_AMT"].ToString(), 16);
                            // 存入金額    X(16)
                            // 20180119,PeterHsieh : CTBC修改規格為，當金額為負數就放在支出金額，為正就放在存入金額，另一金額欄位就放'+0'
                            string SaveAMT = drAccountData.Rows[j]["TRAN_AMT"].ToString();
                            SaveAMT = SaveAMT.Contains("-") ? "+0.00" : ("+" + SaveAMT);
                            strRow["SaveAMT"] = ChangeValue(drAccountData.Rows[j]["SaveAMT"].ToString(), 16);
                            // strRow["SaveAMT"] = ChangeNumber(SaveAMT, 16, 2, false);
                            // 幣別  X(03)
                            strRow["Currency"] = ChangeValue(drAccountData.Rows[j]["Currency"].ToString(), 3);
                            // 餘額  X(16)
                            string strBALANCE = drAccountData.Rows[j]["BALANCE"].ToString();
                            strBALANCE = strBALANCE.Contains("-") ? strBALANCE : "+" + strBALANCE;
                            //strRow["Currency"] = ChangeValue(drAccountData.Rows[j]["Currency"].ToString(), 3);
                            string strZERO = ChangeValue(drAccountData.Rows[j]["BALANCE"].ToString(), 16);
                            if (String.IsNullOrEmpty(strZERO.Trim()) || strZERO.Trim() == "")
                            {
                                if (strRow["Currency"].ToString() == "TWD")
                                {
                                    strRow["BALANCE"] = "0               ";
                                }
                                else
                                {
                                    strRow["BALANCE"] = "0.00            ";
                                }
                            }
                            else
                            {
                                strRow["BALANCE"] = strZERO;
                            }
                            //string Currency = GetCurrency(drAccountData.Rows[j]["ACCT_NO"].ToString());
                            //strRow["Currency"] = Currency;
                            // ATM或端末機代號   X(20)
                            strRow["ATM_NO"] = ChangeValue(drAccountData.Rows[j]["ATM_NO"].ToString(), 20);
                            // 櫃員代號    X(20)
                            strRow["TELLER"] = ChangeValue(drAccountData.Rows[j]["TELLER"].ToString(), 20);
                            // 轉出入行庫代碼及帳號 (RFDM) TRF_BANK + TRF_ACCT  X(20)
                            if (drAccountData.Rows[j]["REMARK"].ToString().Trim() == "XML" || drAccountData.Rows[j]["REMARK"].ToString().Trim() == "FEDI" || drAccountData.Rows[j]["REMARK"].ToString().Trim() == "ＸＭＬ" || drAccountData.Rows[j]["REMARK"].ToString().Trim() == "ＦＥＤＩ")
                            {
                                int intLength = drAccountData.Rows[j]["TRF_BANK"].ToString().Trim().Length;
                                if (intLength > 3)
                                {
                                    strRow["TRF_BANK"] = ChangeValue("   " + drAccountData.Rows[j]["TRF_BANK"].ToString().Trim().Substring(3, intLength - 3), 20);
                                }
                                else
                                {
                                    strRow["TRF_BANK"] = ChangeValue("   ", 20);
                                }
                            }
                            else
                            {
                                strRow["TRF_BANK"] = ChangeValue(drAccountData.Rows[j]["TRF_BANK"].ToString(), 20);
                            }
                            // 票號(RFDM)  
                            strRow["CHQ_NO"] = ChangeChiness(drAccountData.Rows[j]["CHQ_NO"].ToString(), 8);
                            // 備註(RFDM) NARRATIVE 
                            strRow["NARRATIVE"] = ChangeChiness(drAccountData.Rows[j]["NARRATIVE"].ToString(), 40);
                            // 註記(RFDM) REMARK X(300)
                            strRow["REMARK"] = ChangeChiness(drAccountData.Rows[j]["REMARK"].ToString(), 40);
                            //81019
                            strRow["YBTXLOG_SRC_ID"] = ChangeChiness(drAccountData.Rows[j]["YBTXLOG_SRC_ID"].ToString(), 7); // 設備代理行
                            strRow["YBTXLOG_STAND_NO"] = ChangeChiness(drAccountData.Rows[j]["YBTXLOG_STAND_NO"].ToString(), 7); // 交易序號
                            strRow["YBTXLOG_SAFE_TMNL_ID"] = ChangeChiness(drAccountData.Rows[j]["YBTXLOG_SAFE_TMNL_ID"].ToString(), 8); //機器編號
                            strRow["YBTXLOG_IC_MEMO_CARDNO"] = ChangeChiness(drAccountData.Rows[j]["YBTXLOG_IC_MEMO_CARDNO"].ToString(), 16);//卡號
                                                                                                                                             // 會員編號(ATM)
                            strRow["Member_No"] = ChangeChiness(drAccountData.Rows[j]["Member_No"].ToString(), 20);
                            strRow["ADD_NARRATIVE"] = ChangeChiness(drAccountData.Rows[j]["ADD_NARRATIVE"].ToString(), 40); //附言
                            strRow["CUSTOMER_NAME"] = ChangeChiness(drRecvAccount.Rows[a]["CUSTOMER_NAME"].ToString(), 60); //客戶名稱
                            strRow["PrintDate"] = ChangeChiness(System.DateTime.Now.ToShortDateString(), 10); //列印日期
                            strRow["QDateS"] = ChangeChiness(drRecvAccount.Rows[a]["QDateS"].ToString(), 10); //查詢起
                            strRow["QDateE"] = ChangeChiness(drRecvAccount.Rows[a]["QDateE"].ToString(), 10); //查詢迄
                            strRow["CreatedUser"] = ChangeValue(drAccountData.Rows[j]["CreatedUser"].ToString(), 20);
                            dt.Rows.Add(strRow);
                            #endregion
                            // 換行
                            //fileContent2 += "\r\n";
                        }

                        if (dt.Rows.Count > 0)
                        {
                            CreateRFDMPdf(Acct_DateFm_DateTo, dt, txtDocIDPath);
                            int RFDMInt = UpdateTransPDFFILEStatus(drRecvAccount.Rows[a]["CaseTrsNewID"].ToString());
                            if (RFDMInt > 0)
                            {
                                // 記錄LOG
                                newfilelog.Write(NewFileLog.WorkType.Work, NewFileLog.ErrorType.None, "信息-------- 帳戶: " + Acct_DateFm_DateTo + " PDF_FILE  交易產檔狀態更新完成  ");
                            }
                            else
                            {
                                newfilelog.Write(NewFileLog.WorkType.Work, NewFileLog.ErrorType.None, "信息-------- 帳戶: " + Acct_DateFm_DateTo + " PDF_FILE  交易產檔狀態更新失敗 ");
                            }
                        }
                        else
                        {
                                newfilelog.Write(NewFileLog.WorkType.Work, NewFileLog.ErrorType.None, "信息-------- 帳戶: " + Acct_DateFm_DateTo + " PDF_FILE  無交易明細產檔狀態更新完成  ");
                        }
                        dt.Clear();
                        // 記錄LOG
                        newfilelog.Write(NewFileLog.WorkType.Work, NewFileLog.ErrorType.None, "信息--------帳戶:" + Acct_DateFm_DateTo + "PDF 文件中增加" + drAccountData.Rows.Count + "筆資料!");
                    }
                }
                else
                {
                    dt.Clear();
                    // 記錄LOG
                    newfilelog.Write(NewFileLog.WorkType.Work, NewFileLog.ErrorType.None, "信息--------案號:" + DocNo + "無交易明細資料");
                }


            }
            catch (Exception ex)
            {
                newfilelog.Write(NewFileLog.WorkType.Work, NewFileLog.ErrorType.None, "信息--------GenerateDetailsPDF:" + strDocNo + ex.Message.ToString());
                noticeMail(mailTo, "程式:NewHistoryFile-GenerateDetailsPDF " + "錯誤: 案號 :" + strDocNo + ":" + ex.Message.ToString());
            }
		}
   

        public void ExportRFDMExcel(string strDocNo,NewFileLog newfilelog)
        {
            try
            {
                newfilelog.Write(NewFileLog.WorkType.Work, NewFileLog.ErrorType.None, "信息--------產生EXCEL歷史帳戶交易資料（存款往來明細資料）開始------");
                // 查詢產生檔案的資料
                DataTable dtResult = GetRFDMData(strDocNo);
                string DocNo = string.Empty;

                if (dtResult.Rows.Count > 0)
                {

                    newfilelog.Write(NewFileLog.WorkType.Work, NewFileLog.ErrorType.None, "信息--------案號:" + strDocNo);
                    GenerateDetailExcel(strDocNo, dtResult, newfilelog);
                }
                else
                {
                    newfilelog.Write(NewFileLog.WorkType.Work, NewFileLog.ErrorType.None, "信息--------沒有查詢名稱（存款往來明細資料）");
                }

                newfilelog.Write(NewFileLog.WorkType.Work, NewFileLog.ErrorType.None, "信息--------EXCEL歷史交易檔名稱（存款往來明細資料）結束------");
            }
            catch (Exception ex)
            {
                newfilelog.Write(NewFileLog.WorkType.Work, NewFileLog.ErrorType.None, "信息--------ExportRFDMExcel:" + strDocNo + ex.Message.ToString());
                noticeMail(mailTo, "程式:NewHistoryFile-ExportRFDMExcel " + "錯誤: 案號 :" + strDocNo + ":" + ex.Message.ToString());
            }
        }

        private void GenerateDocNo(object obj)
        {
            try
            {
                var lstString = (List<string>)obj;
                string DocNo = lstString[0];
                int threadId = System.Threading.Thread.CurrentThread.ManagedThreadId;

                var newfilelog = new NewFileLog(ConfigurationManager.AppSettings["fileLog"], AppDomain.CurrentDomain.FriendlyName.Replace(".vshost", "").Replace(".exe", ""), threadId.ToString());

                newfilelog.Write(NewFileLog.WorkType.Work, NewFileLog.ErrorType.None, "Task--------EXCLE thread Id:" + threadId.ToString());
                newfilelog.Write(NewFileLog.WorkType.Work, NewFileLog.ErrorType.None, "Task--------執行Excel產檔----案號:" + DocNo);
                // 建立PDF及Excel目錄+案號
                if (!Directory.Exists(pdfFilePath + "\\" + DocNo)) Directory.CreateDirectory(pdfFilePath + "\\" + DocNo);
                if (!Directory.Exists(txtFilePath + "\\" + DocNo)) Directory.CreateDirectory(txtFilePath + "\\" + DocNo);
                string txtDocDirPath = pdfFilePath + "\\" + DocNo;
                string txtDocIDPath = string.Empty;
                int intExcelZip = 0;
                int intPdfZip = 0;
                ///
                /// Excel 基本資料產檔
                int htgData = ExportHtgExcel(DocNo, newfilelog);
                if (htgData > 0)
                {
                    ///  PDF 基本資料產檔
                    ExportHtgPdf(DocNo, newfilelog);
                    ///
                    //檢查DocNo TransactionFlag=Y 才產明細 20230111
                    ///
                    DataTable dtDoc = CheckTransactionFlag(DocNo, newfilelog);
                    if (dtDoc.Rows.Count > 0)
                    {
                        // EXCEL 交易明細產檔
                        ExportRFDMExcel(DocNo, newfilelog);
                        ////// PDF 交易明細產檔
                        ExportRFDMPdf(DocNo, newfilelog);
                    }
                    //壓縮EXCEL(含基資/明細)
                    intExcelZip = ExcelZip(DocNo, newfilelog);
                    //壓縮PDF(含基資/明細)
                    intPdfZip = PdfZip(DocNo, newfilelog);
                    //
                }
                //更新狀態
                // ZIP 產生成功 更新 為 狀態 03 成功 EXCEL = Y 且 PDF = Y 就是成功,其他失敗
                ModifyVersionStatus1(DocNo, newfilelog, intExcelZip, intPdfZip);
                // 程序結束記入log
                newfilelog.Write(NewFileLog.WorkType.Work, NewFileLog.ErrorType.None, DateTime.Now.ToString());
                newfilelog.Write(NewFileLog.WorkType.Work, NewFileLog.ErrorType.None, "信息--------產檔結束------");
            }
            catch (Exception ex)
            {
                var lstString = (List<string>)obj;
                string DocNo = lstString[0];
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------GenerateDocNo " + "錯誤: 案號 :" + DocNo + ":" + ex.Message.ToString());
                noticeMail(mailTo, "程式:NewHistoryFile-GenerateDocNo " + "錯誤: 案號 :" + DocNo + ":" + ex.Message.ToString());
            }
        }
        private void GenerateDetailExcel( string strDocNo, DataTable drRecvAccount, NewFileLog newfilelog)
		{
            try
            {
                string DocNo = strDocNo;
                newfilelog.Write(NewFileLog.WorkType.Work, NewFileLog.ErrorType.None, "-------建立EXCEL----:" + strDocNo);
                string txtDocDirPath = txtFilePath + "\\" + strDocNo;
                string txtDocIDPath = string.Empty;

                // RFDM內容及對應資料筆數變量
                //string fileContent2 = "";
                string Acct_DateFm_DateTo = "";

                #region 遍歷並追加txt文件

                // 遍歷並追加txt文件
                DataTable dt = new DataTable();
                dt.Columns.Add("CUST_ID_NO");
                dt.Columns.Add("ACCT_NO");
                dt.Columns.Add("TRAN_DATE");
                dt.Columns.Add("JNRST_TIME");
                dt.Columns.Add("TRAN_BRANCH");
                dt.Columns.Add("TRANS_CODE");
                dt.Columns.Add("TELLER");
                //dt.Columns.Add("JRNL_NO");
                //dt.Columns.Add("TRANS_CODE");
                dt.Columns.Add("TXN_DESC");
                dt.Columns.Add("TRAN_AMT");
                dt.Columns.Add("SaveAMT");
                dt.Columns.Add("BALANCE");
                //
                dt.Columns.Add("Currency");
                dt.Columns.Add("TRF_BANK");
                //81019
                // 20221118 新增會員編號
                dt.Columns.Add("Member_No");
                dt.Columns.Add("YBTXLOG_SRC_ID");
                dt.Columns.Add("YBTXLOG_STAND_NO");
                dt.Columns.Add("YBTXLOG_SAFE_TMNL_ID");
                dt.Columns.Add("YBTXLOG_IC_MEMO_CARDNO");

                //
                dt.Columns.Add("CHQ_NO");//票號
                dt.Columns.Add("NARRATIVE");//備註
                dt.Columns.Add("REMARK");// 註記
                                         //81019 附言再確認
                dt.Columns.Add("ADD_NARRATIVE");
                //
                dt.Columns.Add("JRNL_NO");
                //dt.Columns.Add("TELLER");
                dt.Columns.Add("CUSTOMER_NAME");
                dt.Columns.Add("PD_TYPE_DESC");
                dt.Columns.Add("QDateS");
                dt.Columns.Add("QDateE");
                dt.Columns.Add("PrintDate");
                dt.Columns.Add("ATM_NO");
                dt.Columns.Add("CreatedUser");
                #endregion



                if (drRecvAccount.Rows.Count > 0) //所有帳戶交易
                {
                    for (int a = 0; a < drRecvAccount.Rows.Count; a++)
                    {
                        Acct_DateFm_DateTo = drRecvAccount.Rows[a]["ACCT_NO"].ToString() + "-" + drRecvAccount.Rows[a]["QDateS"].ToString() + "-" + drRecvAccount.Rows[a]["QDateE"].ToString();
                        // 遍歷並追加txt文件
                        DataTable drAccountData = GetRFDMRecvData(drRecvAccount.Rows[a]["CaseTrsNewID"].ToString(), drRecvAccount.Rows[a]["ACCT_NO"].ToString(), drRecvAccount.Rows[a]["QDateS"].ToString(), drRecvAccount.Rows[a]["QDateE"].ToString(), DateTime.Now.ToString("yyyyMMdd"));
                        // 2023 如果ATM有資料則 JNRST_TIME用ＡＴＭ＿ＴＩＭＥ = YBTXLOG_TXN_HHMMSS
                        for (int p = 0; p < drAccountData.Rows.Count; p++)
                        {

                            if (!String.IsNullOrEmpty(drAccountData.Rows[p]["ATM_TIME"].ToString()))
                            {
                                drAccountData.Rows[p]["JNRST_TIME"] = drAccountData.Rows[p]["ATM_TIME"].ToString();
                            }
                            //
                        }
                        //以規格書重新排序,可能會調依實際需求
                        DataView dv = drAccountData.DefaultView;
                        dv.Sort = "CUST_ID_NO,ACCT_NO,TRAN_DATE,JNRST_TIME";
                        drAccountData = dv.ToTable();
                        for (int j = 0; j < drAccountData.Rows.Count; j++)
                        {
                            //新版改用
                            string SECDIR = "";
                            if (drAccountData.Rows[j]["CustID"].ToString().Length < 8)
                            {
                                SECDIR = drAccountData.Rows[j]["CustAccount"].ToString();
                            }
                            else
                            {
                                SECDIR = drAccountData.Rows[j]["CUST_ID_NO"].ToString();
                            }
                            txtDocIDPath = txtDocDirPath + "\\" + SECDIR;
                            if (!Directory.Exists(txtDocIDPath)) Directory.CreateDirectory(txtDocIDPath);
                            #region 拼接內容
                            DataRow strRow = dt.NewRow();
                            // 身分證統一編號 X(10)
                            strRow["CUST_ID_NO"] = ChangeValue(drAccountData.Rows[j]["CUST_ID_NO"].ToString(), 11);
                            strRow["ACCT_NO"] = ChangeAccount(drAccountData.Rows[j]["ACCT_NO"].ToString(), 20);
                            // 交易序號    9(08)
                            strRow["JRNL_NO"] = ChangeValue(drAccountData.Rows[j]["JRNL_NO"].ToString(), 8);
                            // 交易日期    X(08)
                            strRow["TRAN_DATE"] = ChangeValue(drAccountData.Rows[j]["TRAN_DATE"].ToString(), 8);
                            // 交易時間    X(06)???
                            //if (drAccountData.Rows[j]["JNRST_TIME"].ToString().Length < 8)
                            //{
                            //    strRow["JNRST_TIME"] = ChangeValue(drAccountData.Rows[j]["RFDM_HHMMSS"].ToString(), 8);//增加: 所以取8碼
                            //}
                            //else
                            {
                                strRow["JNRST_TIME"] = ChangeValue(drAccountData.Rows[j]["JNRST_TIME"].ToString(), 8);//增加: 所以取8碼
                            }

                            // 交易行(或所屬分行代號)    X(07)
                            strRow["TRAN_BRANCH"] = ChangeValue(drAccountData.Rows[j]["TRAN_BRANCH"].ToString(), 7);
                            // 交易櫃員    X(05)
                            strRow["TRANS_CODE"] = ChangeValue(drAccountData.Rows[j]["TRANS_CODE"].ToString(), 5);
                            // 交易摘要    X(40)
                            strRow["TXN_DESC"] = ChangeChiness(drAccountData.Rows[j]["TXN_DESC"].ToString(), 40);
                            // 支出金額    X(16)
                            // 20180119,PeterHsieh : CTBC修改規格為，當金額為負數就放在支出金額，為正就放在存入金額，另一金額欄位就放'+0'
                            string strTRAN_AMT = drAccountData.Rows[j]["TRAN_AMT"].ToString();
                            strTRAN_AMT = strTRAN_AMT.Contains("-") ? strTRAN_AMT : "+0.00";
                            //20180622 RC 線上投單CR修正 宏祥 add start
                            string strTRAN_AMT2 = ChangeNumber(strTRAN_AMT, 16, 2, false);
                            //strRow["TRAN_AMT"] = strTRAN_AMT2.Contains("-") ? strTRAN_AMT2.Replace("-", "+") : strTRAN_AMT2;
                            strRow["TRAN_AMT"] = ChangeValue(drAccountData.Rows[j]["TRAN_AMT"].ToString(), 16);
                            // 存入金額    X(16)
                            // 20180119,PeterHsieh : CTBC修改規格為，當金額為負數就放在支出金額，為正就放在存入金額，另一金額欄位就放'+0'
                            string SaveAMT = drAccountData.Rows[j]["TRAN_AMT"].ToString();
                            SaveAMT = SaveAMT.Contains("-") ? "+0.00" : ("+" + SaveAMT);
                            strRow["SaveAMT"] = ChangeValue(drAccountData.Rows[j]["SaveAMT"].ToString(), 16);
                            // strRow["SaveAMT"] = ChangeNumber(SaveAMT, 16, 2, false);
                            // 幣別  X(03)
                            strRow["Currency"] = ChangeValue(drAccountData.Rows[j]["Currency"].ToString(), 3);
                            // 餘額  X(16)
                            string strBALANCE = drAccountData.Rows[j]["BALANCE"].ToString();
                            strBALANCE = strBALANCE.Contains("-") ? strBALANCE : "+" + strBALANCE;
                            string strZERO = ChangeValue(drAccountData.Rows[j]["BALANCE"].ToString(), 16);
                            if (String.IsNullOrEmpty(strZERO.Trim()) || strZERO.Trim() == "")
                            {
                                if (strRow["Currency"].ToString() == "TWD")
                                {
                                    strRow["BALANCE"] = "0";
                                }
                                else
                                {
                                    strRow["BALANCE"] = "0.00";
                                }
                            }
                            else
                            {
                                strRow["BALANCE"] = strZERO;
                            }

                            // 幣別  X(03)
                            //string Currency = GetCurrency(drAccountData.Rows[j]["ACCT_NO"].ToString());
                            //strRow["Currency"] = Currency;
                            // ATM或端末機代號   X(20)
                            strRow["ATM_NO"] = ChangeValue(drAccountData.Rows[j]["ATM_NO"].ToString(), 20);
                            // 櫃員代號    X(20)
                            strRow["TELLER"] = ChangeValue(drAccountData.Rows[j]["TELLER"].ToString(), 20);
                            // 轉出入行庫代碼及帳號 (RFDM) TRF_BANK + TRF_ACCT  X(20)
                            //strRow["TRF_BANK"] = ChangeValue(drAccountData.Rows[j]["TRF_BANK"].ToString(), 20);
                            // 票號(RFDM)  
                            strRow["CHQ_NO"] = ChangeChiness(drAccountData.Rows[j]["CHQ_NO"].ToString(), 8);
                            // 20221118 新增會員編號
                            // 會員編號(ATM) Member_No 
                            //strRow["Member_No"] = ChangeChiness(drAccountData.Rows[j]["Member_No"].ToString(), 40);
                            // 備註(RFDM) NARRATIVE 
                            strRow["NARRATIVE"] = ChangeChiness(drAccountData.Rows[j]["NARRATIVE"].ToString(), 40);
                            // 註記(RFDM) REMARK X(300)
                            if (drAccountData.Rows[j]["REMARK"].ToString().Trim() == "XML" || drAccountData.Rows[j]["REMARK"].ToString().Trim() == "FEDI" || drAccountData.Rows[j]["REMARK"].ToString().Trim() == "ＸＭＬ" || drAccountData.Rows[j]["REMARK"].ToString().Trim() == "ＦＥＤＩ")
                            {
                                int intLength = drAccountData.Rows[j]["TRF_BANK"].ToString().Trim().Length;
                                if (intLength > 3)
                                {
                                    strRow["TRF_BANK"] = ChangeValue("   " + drAccountData.Rows[j]["TRF_BANK"].ToString().Trim().Substring(3, intLength - 3), 20);
                                }
                                else
                                {
                                    strRow["TRF_BANK"] = ChangeValue("   ", 20);
                                }
                            }
                            else
                            {
                                strRow["TRF_BANK"] = ChangeValue(drAccountData.Rows[j]["TRF_BANK"].ToString(), 20);
                            }
                            strRow["REMARK"] = ChangeChiness(drAccountData.Rows[j]["REMARK"].ToString(), 40);
                            //81019
                            strRow["Member_No"] = ChangeChiness(drAccountData.Rows[j]["Member_No"].ToString(), 20);//會員編號
                            strRow["YBTXLOG_SRC_ID"] = ChangeChiness(drAccountData.Rows[j]["YBTXLOG_SRC_ID"].ToString(), 7); // 設備代理行
                            strRow["YBTXLOG_STAND_NO"] = ChangeChiness(drAccountData.Rows[j]["YBTXLOG_STAND_NO"].ToString(), 7); // 交易序號
                            strRow["YBTXLOG_SAFE_TMNL_ID"] = ChangeChiness(drAccountData.Rows[j]["YBTXLOG_SAFE_TMNL_ID"].ToString(), 8); //機器編號
                            strRow["YBTXLOG_IC_MEMO_CARDNO"] = ChangeChiness(drAccountData.Rows[j]["YBTXLOG_IC_MEMO_CARDNO"].ToString(), 16);//卡號
                            strRow["ADD_NARRATIVE"] = ChangeChiness(drAccountData.Rows[j]["ADD_NARRATIVE"].ToString(), 40); //附言
                            strRow["CUSTOMER_NAME"] = ChangeChiness(drRecvAccount.Rows[a]["CUSTOMER_NAME"].ToString(), 60); //客戶名稱
                            strRow["PrintDate"] = ChangeChiness(System.DateTime.Now.ToShortDateString(), 10); //列印日期
                            strRow["QDateS"] = ChangeChiness(drRecvAccount.Rows[a]["QDateS"].ToString(), 10); //查詢起
                            strRow["QDateE"] = ChangeChiness(drRecvAccount.Rows[a]["QDateE"].ToString(), 10); //查詢迄
                            strRow["CreatedUser"] = ChangeValue(drAccountData.Rows[j]["CreatedUser"].ToString(), 20);
                            dt.Rows.Add(strRow);
                            #endregion
                            // 換行
                            //fileContent2 += "\r\n";
                        }


                        if (dt.Rows.Count > 0)
                        {
                            // 更新EXCEL FILE Status狀態
                            CreateRFDMExcel(Acct_DateFm_DateTo, dt, txtDocIDPath, newfilelog);
                            int RFDMInt = UpdateTransEXCELFILEStatus(drRecvAccount.Rows[a]["CaseTrsNewID"].ToString());
                            if (RFDMInt > 0)
                            {
                                // 記錄LOG
                                newfilelog.Write(NewFileLog.WorkType.Work, NewFileLog.ErrorType.None, "信息--------  EXCEL_FILE 帳戶:" + Acct_DateFm_DateTo + " EXCEL交易明細產檔狀態更新完成  ");
                            }
                            else
                            {
                                newfilelog.Write(NewFileLog.WorkType.Work, NewFileLog.ErrorType.None, "信息--------  EXCEL_FILE 帳戶 " + Acct_DateFm_DateTo + " EXCEL交易明細產檔狀態更新失敗 ");
                            }
                        }
                        else
                        {
                                newfilelog.Write(NewFileLog.WorkType.Work, NewFileLog.ErrorType.None, "信息--------  EXCEL_FILE 帳戶:" + Acct_DateFm_DateTo + "  EXCEL無交易明細產檔狀態更新完成  ");
                        }
                        dt.Clear();
                        // 記錄LOG
                        newfilelog.Write(NewFileLog.WorkType.Work, NewFileLog.ErrorType.None, "信息--------帳戶:" + Acct_DateFm_DateTo + "EXCEL文件中增加" + drAccountData.Rows.Count + "筆資料!");
                    }
                }
                else
                {
                    dt.Clear();
                    // 記錄LOG
                    newfilelog.Write(NewFileLog.WorkType.Work, NewFileLog.ErrorType.None, "信息--------帳戶:" + DocNo + "無交易明細資料");
                }
            }
            catch (Exception ex)
            {
                newfilelog.Write(NewFileLog.WorkType.Work, NewFileLog.ErrorType.None, "信息--------GenerateDetailExcel " + "錯誤: 案號 :" + strDocNo + ":" + ex.Message.ToString());
                noticeMail(mailTo, "程式:NewHistoryFile-GenerateDetailExcel " + "錯誤: 案號 :" + strDocNo + ":" + ex.Message.ToString());
            }
		}

		public int DeleteCaseEdocFileExcel(Guid caseid)
        {
            try
            {
                CommandParameterCollection CPC = new CommandParameterCollection();
                int rtn = 0;
                string sql = @"delete CaseEdocFile Where CaseId=@CaseId and FileType='xlsx' and Type='歷史'";
                CPC.Clear();
                CPC.Add(new CommandParameter("@CaseId", caseid));
                return base.ExecuteNonQuery(sql, CPC);
                //base.Parameter.Clear();
                //base.Parameter.Add(new CommandParameter("@CaseId", caseid));
                //rtn = base.ExecuteNonQuery(sql);
            }
            catch (Exception ex)
            {
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------DeleteCaseEdocFileExcel錯誤!!" + ex.Message.ToString());
                noticeMail(mailTo, "DeleteCaseEdocFileExcel錯誤!!" + ex.Message.ToString());
                return 0;
            }
        }
        public int DeleteCaseEdocFilePdf(Guid caseid)
        {
            try
            {
                CommandParameterCollection CPC = new CommandParameterCollection();
                int rtn = 0;
                string sql = @"delete CaseEdocFile where CaseId=@CaseId and FileType='pdf' and Type='歷史'";
                CPC.Clear();
                CPC.Add(new CommandParameter("@CaseId", caseid));
                return base.ExecuteNonQuery(sql, CPC);
                //base.Parameter.Clear();
                //base.Parameter.Add(new CommandParameter("@CaseId", caseid));
                //rtn = base.ExecuteNonQuery(sql);
                //return rtn;
            }
            catch (Exception ex)
            {
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------DeleteCaseEdocFileExcel錯誤!!" + ex.Message.ToString());
                noticeMail(mailTo, "DeleteCaseEdocFileExcel錯誤!!" + ex.Message.ToString());
                return 0;
            }
        }

        public int InsertCaseEdocFile(CaseEdocFile caseEdocFile)
        {
            try
            {
                CommandParameterCollection CPC = new CommandParameterCollection();
                int rtn = 0;
                string sql = @"insert into CaseEdocFile values(@CaseId,@Type,@FileType,@FileName,@FileObject,@SendNo)";
                base.Parameter.Clear();
                CPC.Add(new CommandParameter("@CaseId", caseEdocFile.CaseId));
                CPC.Add(new CommandParameter("@Type", caseEdocFile.Type));
                CPC.Add(new CommandParameter("@FileType", caseEdocFile.FileType));
                CPC.Add(new CommandParameter("@FileName", caseEdocFile.FileName));
                CPC.Add(new CommandParameter("@SendNo", caseEdocFile.SendNo));
                CPC.Add(new CommandParameter("@FileObject", caseEdocFile.FileObject, SqlDbType.VarBinary, 0));
                //base.Parameter.Add(new cpc.CommandParameter("@CaseId", caseEdocFile.CaseId));
                //base.Parameter.Add(new CommandParameter("@Type", caseEdocFile.Type));
                //base.Parameter.Add(new CommandParameter("@FileType", caseEdocFile.FileType));
                //base.Parameter.Add(new CommandParameter("@FileName", caseEdocFile.FileName));
                //base.Parameter.Add(new CommandParameter("@SendNo", caseEdocFile.SendNo));
                //base.Parameter.Add(new CommandParameter("@FileObject", caseEdocFile.FileObject, SqlDbType.VarBinary, 0));
                
                return base.ExecuteNonQuery(sql, CPC);
                //rtn = base.ExecuteNonQuery(sql);
                //return rtn;
            }
            catch (Exception ex)
            {
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------InsertCaseEdocFile錯誤!!" + ex.Message.ToString());
                noticeMail(mailTo, "InsertCaseEdocFile錯誤!!" + ex.Message.ToString());
                return 0;
            }
        }

        /// <summary>
        /// 補充指定長度的空格
        /// </summary>
        /// <param name="strSpaceLen">指定長度</param>
        /// <returns></returns>
        public string AddSpace(int strSpaceLen, string flag)
        {
            string result = "";

            // 拼接strNullNumber個半形空白
            for (int m = 0; m < strSpaceLen; m++)
            {
                result += flag;
            }

            return result;
        }

        /// <summary>
        /// 根據帳號中的標誌位獲取幣別
        /// </summary>
        /// <param name="strValue"></param>
        /// <returns></returns>
        public string GetCurrency(string strValue)
        {
            string strCurrency = "";

            if (!string.IsNullOrEmpty(strValue))
            {
                // 截取標誌位
                string strFlag = strValue.Length > 4 ? strValue.Substring(0, 4) : "";

                // strFlag != "0000",標誌位爲帳號的後3位
                strFlag = strFlag == "0000" ? strFlag : strValue.Substring(strValue.Length - 3, 3);

                // 獲取幣別
                DataRow[] dr = CurrencyList.Select("CodeNo ='" + strFlag + "'");
                strCurrency = dr != null && dr.Length > 0 ? dr[0]["CodeDesc"].ToString() : "";

                // 如果幣別爲空,就用空白代替
                if (string.IsNullOrEmpty(strCurrency) || strCurrency.Length < 3)
                {
                    strCurrency = strCurrency + AddSpace(3 - strCurrency.Length, strNull);
                }
                else
                {
                    strCurrency = strCurrency.Substring(0, 3);
                }
            }
            else
            {
                strCurrency = AddSpace(3, strNull);
            }

            return strCurrency;
        }
        public string ChangeAccount(string strValue, int strValueLen)
        {
            // 帳號
            string strAccount = "";

            if (!string.IsNullOrEmpty(strValue))
            {
                // 截取標誌位
                string strFlag = strValue.Length > 4 ? strValue.Substring(0, 4) : "";

                // strFlag == "0000",帳號爲strValue後12位,否則爲前幾位
                if (strFlag == "0000")
                {
                    // 截取帳號
                    strAccount = strValue.Substring(4, strValue.Length - 4);
                }
                else
                {
                    // 截取帳號
                    strAccount = strValue.Substring(1, strValue.Length - 4);

                    // 截取標誌位
                    //strFlag = strValue.Substring(strValue.Length - 3, 3);
                }

                // 獲取幣別
                //DataRow[] dr = CurrencyList.Select("CodeNo ='" + strFlag + "'");
                //string strCurrency = dr != null && dr.Length > 0 ? dr[0]["CodeDesc"].ToString() : "";
                // 如果幣別爲空,就用空白代替
                //if (string.IsNullOrEmpty(strCurrency) || strCurrency.Length < 3)
                //{
                //   strCurrency = strCurrency + AddSpace(3 - strCurrency.Length, strNull);
                //}
                //else
                //{
                //   strCurrency = strCurrency.Substring(0, 3);
                //}

                // 拼接帳號和幣別
                //strAccount = strAccount + strCurrency;
            }

            // 拼接產品別
            //strAccount += "-" + strPD_TYPE_DESC;

            // 指定長度-字串長度
            int strNullNumber = strValueLen - strAccount.Length;

            // strNullNumber>0,就在字串後拼接strNullNumber個半形空白,否則就截取指定長度
            strAccount = strNullNumber > 0 ? strAccount + AddSpace(strNullNumber, strNull) : strAccount.Substring(0, strValueLen);

            return strAccount;
        }
        public string ChangeValue(string strValue, int strValueLen)
        {
            if (!string.IsNullOrEmpty(strValue))
            {
                // 差值 = 字串指定長度-字串實際長度
                int strNullNumber = strValueLen - strValue.Length;

                // strNullNumber>0,就在字串後拼接strNullNumber個半形空白,否則就截取指定長度
                strValue = strNullNumber > 0 ? strValue + AddSpace(strNullNumber, strNull) : strValue.Substring(0, strValueLen);
            }
            else
            {
                // 拼接strValueLen個半形空白
                strValue += AddSpace(strValueLen, strNull);
            }
            return strValue;
        }

        private static void CreateZipFile(string sFilePath, ZipOutputStream zipS, string staticF)
        {
            string[] filesArray = Directory.GetFileSystemEntries(sFilePath);
            foreach (string file in filesArray)
            {
                if (Directory.Exists(file))                     //如果当前是文件夹，递归
                {
                    string pPath = sFilePath;
                    pPath += file.Substring(file.LastIndexOf("\\") + 1);
                    pPath += "\\";
                    CreateZipFile(file, zipS, staticF);
                }
                else                                            //如果是文件，开始压缩
                {
                    FileStream fileStream = File.OpenRead(file);
                    byte[] buffer = new byte[fileStream.Length];
                    fileStream.Read(buffer, 0, buffer.Length);
                    string tempFile = file.Substring(staticF.LastIndexOf("\\") + 1);
                    ZipEntry entry = new ZipEntry(Path.GetFileName(file));

                    entry.DateTime = DateTime.Now;
                    entry.Size = fileStream.Length;
                    fileStream.Close();
                    zipS.PutNextEntry(entry);
                    zipS.Write(buffer, 0, buffer.Length);
                }
            }
        }


        public static void addFolderToZip(ZipFile f, string root, string folder)
        {
            string relative = folder.Substring(root.Length);
            if (relative.Length > 0)
            {
                f.AddDirectory(relative);
            }

            foreach (string file in Directory.GetFiles(folder))
            {
                relative = file.Substring(root.Length);
                f.Add(file, relative);
            }

            foreach (string subFolder in Directory.GetDirectories(folder))
            {
                addFolderToZip(f, root, subFolder);
            }
        }
  
        public static void CreateZip(string sourceFilePath, string destinationZipFilePath)
        {
            try
            {
                ICSharpCode.SharpZipLib.Zip.ZipFile zipFile = ICSharpCode.SharpZipLib.Zip.ZipFile.Create(sourceFilePath + ".zip");
                zipFile.BeginUpdate();
                addFolderToZip(zipFile, sourceFilePath, sourceFilePath);
                zipFile.CommitUpdate();
                zipFile.Close();
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------成功壓縮 " + sourceFilePath + " 資料!");
            }
            catch (Exception ex)
            {
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "程式:NewHistoryFile-CreateZip" + "錯誤: " + sourceFilePath + destinationZipFilePath + ex.Message.ToString());
                noticeMail(mailTo, "程式:NewHistoryFile-CreateZip" + "錯誤: " + sourceFilePath + destinationZipFilePath + ex.Message.ToString());
            }

        }
  

        public DataTable GetMultiData()
        {
            try
            {
                string sqlSelect = @" select distinct CaseTrsQueryVersion.docno 
                                                FROM CaseTrsQueryVersion
                                        where CaseTrsQueryVersion.Status ='02' and   HTGSendStatus  in ('4','6') ";
                // 清空容器
                base.Parameter.Clear();
                return base.Search(sqlSelect);
            }
            catch (Exception ex)
            {
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "程式:NewHistoryFile-GetMultiData SQL" + "錯誤: " + ex.Message.ToString());
                noticeMail(mailTo, "程式:NewHistoryFile-GetMultiData SQL" + "錯誤: " + ex.Message.ToString());
                return null;
            }
        }
        public DataTable GetMultiData66()
        {
            try
            {
                string sqlSelect = @" select distinct CaseTrsQueryVersion.docno  
                                              from CaseTrsQueryVersion
                                      where CaseTrsQueryVersion.Status ='66' ";

                // 清空容器
                base.Parameter.Clear();
                return base.Search(sqlSelect);
            }
            catch (Exception ex)
            {
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "程式:NewHistoryFile-GetMultiData66 SQL" + "錯誤: " + ex.Message.ToString());
                noticeMail(mailTo, "程式:NewHistoryFile-GetMultiData66 SQL" + "錯誤: " + ex.Message.ToString());
                return null;
            }
        }
  
        //
        //
        //
 
   

        public DataTable CheckTransactionFlag(string DocNo,NewFileLog newfilelog)
        {
            try
            {
                CommandParameterCollection CPC = new CommandParameterCollection();
                // 依規格書
                string sqlSelect = @"select DocNo              
                                from  CaseTrsQueryVersion
						         where CaseTrsQueryVersion.DocNo = @DocNoCheckTransactionFlag and TransactionFlag='Y'  and status = '66' ";

                // 清空容器
                CPC.Clear();
                CPC.Add(new CommandParameter("@DocNoCheckTransactionFlag", DocNo));
                return base.Search(sqlSelect, CPC);
                //base.Parameter.Clear();
                //base.Parameter.Add(new CommandParameter("@DocNoCheckTransactionFlag", DocNo));
                //return base.Search(sqlSelect);
            }
            catch (Exception ex)
            {
                newfilelog.Write(NewFileLog.WorkType.Work, NewFileLog.ErrorType.None, "信息--------CheckTransactionFlag 錯誤: " + DocNo + ex.Message.ToString());
                noticeMail(mailTo, "程式:NewHistoryFile-CheckTransactionFlag " + "錯誤: DocNo:" + DocNo + ex.Message.ToString());
                return null;
            }
        }
        public DataTable GetRFDMData(string DocNo)
        {
            try
            {
                // 依規格書
                CommandParameterCollection CPC = new CommandParameterCollection();
                string sqlSelect = @"
                                 select
                                  CaseTrsQueryVersion.CustId
                                  ,CaseTrsQueryVersion.NewID as CaseTrsNewID
                                  ,CaseTrsQueryVersion.DocNo as DocNo
								  ,BOPS000401Recv.ACCT_NO
								  ,BOPS000401Recv.CURRENCY
								  ,ISNULL(QDateS, '') as QDateS, ISNULL(QDateE, '')  as   QDateE  
								  ,ISNULL(BOPS067050Recv.CUSTOMER_NAME, '') AS CUSTOMER_NAME
                                from
                                  CaseTrsQueryVersion
								  LEFT JOIN BOPS000401Recv ON BOPS000401Recv.VersionNewID = CaseTrsQueryVersion.NewID
								  LEFT JOIN BOPS067050Recv  on BOPS067050Recv.VersionNewID = CaseTrsQueryVersion.NewID
                               where
                                    CaseTrsQueryVersion.DocNo = @DocNo	and Status='66'
		

                              ";

                // 清空容器
                CPC.Clear();
                CPC.Add(new CommandParameter("@DocNo", DocNo));
                return base.Search(sqlSelect, CPC);
                //base.Parameter.Clear();
                //base.Parameter.Add(new CommandParameter("@DocNo", DocNo));
                //return base.Search(sqlSelect);
            }
            catch (Exception ex)
            {
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "程式:NewHistoryFile-GetRFDMData SQL" + "錯誤: " + DocNo + ex.Message.ToString());
                noticeMail(mailTo, "程式:NewHistoryFile-GetRFDMData SQL" + "錯誤: " + DocNo+ex.Message.ToString());
                return null;
            }

        }

        /// <summary>
        /// 判斷是'未開戶'或'無查詢區間資料'
        /// </summary>
        /// <returns></returns>
  
        /// <summary>
        /// 递归压缩文件
        /// </summary>
        /// <param name="sourceFilePath">待压缩的文件或文件夹路径</param>
        /// <param name="zipStream">打包结果的zip文件路径（类似 D:\WorkSpace\a.zip）,全路径包括文件名和.zip扩展名</param>
        /// <param name="staticFile"></param>
        private static void CreateZipFiles(string sourceFilePath, ZipOutputStream zipStream, string staticFile)
        {
            //Crc32 crc = new Crc32();
            string[] filesArray = Directory.GetFileSystemEntries(sourceFilePath);
            foreach (string file in filesArray)
            {
                if (Directory.Exists(file))                     //如果当前是文件夹，递归
                {
                    CreateZipFiles(file, zipStream, staticFile);
                }

                else                                            //如果是文件，开始压缩
                {
                    FileStream fileStream = File.OpenRead(file);

                    byte[] buffer = new byte[fileStream.Length];
                    fileStream.Read(buffer, 0, buffer.Length);
                    string tempFile = file.Substring(staticFile.LastIndexOf("\\") + 1);
                    ZipEntry entry = new ZipEntry(Path.GetFileName(file));

                    entry.DateTime = DateTime.Now;
                    entry.Size = fileStream.Length;
                    fileStream.Close();
                    // crc.Reset();
                    //  crc.Update(buffer);
                    //  entry.Crc = crc.Value;
                    zipStream.PutNextEntry(entry);

                    zipStream.Write(buffer, 0, buffer.Length);
                }
            }
        }


        /// <summary>
        /// 查詢HTG回文txt資料
        /// </summary>
        /// <param name="VersionNewID"></param>
        /// <returns></returns>
        public DataTable GetRFDMRecvData(string VersionNewID,string ACCT_NO,string QDateS,string QDateE,string Data_Date)
        {
            try
            {
                // 依規格書
                CommandParameterCollection CPC = new CommandParameterCollection();
                string sqlSelect = @"
                SELECT CustId as CUST_ID_NO -- 客戶編號
                ,case   when LEN(CaseTrsQueryVersion.QDateS)=8 and CaseTrsQueryVersion.QDateS <> '19110101' 
                        then substring(CaseTrsQueryVersion.QDateS,1, 4) + substring(CaseTrsQueryVersion.QDateS,5,2) + substring(CaseTrsQueryVersion.QDateS, 7,2) 	
                        WHEN CaseTrsQueryVersion.QDateS = '19110101' and LEN(BOPS000401Recv.OPEN_DATE) > 0
                        THEN  substring(BOPS000401Recv.OPEN_DATE,7,4)+ substring(BOPS000401Recv.OPEN_DATE,4, 2) +substring(BOPS000401Recv.OPEN_DATE,1, 2) 
		                ELSE '' END QDateS
                ,QDateE   
                ,TRANS_CODE
                ,PROMO_CODE
                ,CaseTrsRFDMRecv.ACCT_NO --帳號	X(20)
                ,CASE WHEN ISNULL(CaseTrsRFDMRecv.FISC_SEQNO, '') = '' OR (CaseTrsRFDMRecv.FISC_SEQNO = '00000000')
                    THEN RIGHT('00000000'+JRNL_NO,8)
                    ELSE CaseTrsRFDMRecv.FISC_SEQNO
                END AS JRNL_NO--交易序號	9(08)
                ,CONVERT(nvarchar(8),TRAN_DATE,112 ) as TRAN_DATE--交易日期	X(08)
				,CaseCustATMRecv.YBTXLOG_TXN_HHMMSS  as ATM_TIME
                    ,JNRST_TIME -- RFDM 交易時間                                   
                ,CASE WHEN isnull(TRAN_BRANCH,'') <> '' 
                    THEN '822' + isnull(TRAN_BRANCH,'') 
                    ELSE ''
                END TRAN_BRANCH --交易行(或所屬分行代號)	X(07)
                ,isnull(TXN_DESC,'') as TXN_DESC--交易摘要	X(40)
                ,CASE 
					WHEN TRAN_AMT < 0  and (isnull(BOPS000401Recv.Currency,'')  = ''  or BOPS000401Recv.Currency  = 'TWD')  THEN FORMAT(TRAN_AMT*(-1), '#,#') 
					WHEN TRAN_AMT < 0  and (isnull(BOPS000401Recv.Currency,'')  <> '' and BOPS000401Recv.Currency <> 'TWD')  THEN FORMAT(TRAN_AMT*(-1), '#,0.00')  
					ELSE FORMAT(0, '#,#') END TRAN_AMT --支出金額	X(16)
                ,CASE 
					WHEN TRAN_AMT >= 0  and (isnull(BOPS000401Recv.Currency,'')  = ''  or BOPS000401Recv.Currency  = 'TWD')  THEN FORMAT(TRAN_AMT, '#,#') 
					WHEN TRAN_AMT >= 0  and (isnull(BOPS000401Recv.Currency,'') <> '' and BOPS000401Recv.Currency <> 'TWD')  THEN FORMAT(TRAN_AMT, '#,0.00') 
					ELSE FORMAT(0, '#,#') END as  SaveAMT  --存入金額	X(16)
				,CASE 
					WHEN (isnull(BOPS000401Recv.Currency,'')  = ''  or BOPS000401Recv.Currency  = 'TWD') THEN FORMAT(BALANCE, '#,#') 
					ELSE  FORMAT(BALANCE, '#,0.00') END as  BALANCE --餘額	X(16) 
                    ,CASE WHEN ISNULL(BOPS000401Recv.Currency,'') = ''  THEN 'TWD' ELSE  BOPS000401Recv.Currency  END as Currency
                ,CaseCustATMRecv.[YBTXLOG_SAFE_TMNL_ID]  as ATM_NO --ATM或端末機代號	X(20)
                ,CaseCustATMRecv.YBTXLOG_SRC_ID  as YBTXLOG_SRC_ID --- 設備代理行
				,CaseCustATMRecv.YBTXLOG_STAND_NO as YBTXLOG_STAND_NO ---  交易序號	
				,CaseCustATMRecv.YBTXLOG_SAFE_TMNL_ID as YBTXLOG_SAFE_TMNL_ID ---  機器編號	
				,CaseCustATMRecv.YBTXLOG_IC_MEMO_CARDNO as YBTXLOG_IC_MEMO_CARDNO ---  卡號	
				,CaseCustATMRecv.NARRATIVE as ADD_NARRATIVE ---  附言
				,CaseCustATMRecv.Member_No as Member_No ---  會員編號
                ,CaseCustATMRecv.YBTXLOG_TXN_HHMMSS 
                ,CaseTrsRFDMRecv.TELLER as TELLER --櫃員代號	X(20)
                ,CaseTrsQueryVersion.CreatedUser as CreatedUser --建檔人員	X(20)
				,CaseTrsQueryVersion.CustID -- 目錄建檔用
				,CaseTrsQueryVersion.CustAccount -- 目錄建檔用
                ,CHQ_NO   -- 票號
                ,REMARK  -- 註記
	            ,CASE WHEN CAST(isnull(TRF_ACCT,'0') AS NUMERIC) = 0
                    THEN ''
                    ELSE replace(replace(isnull(TRF_BANK,''),'448','822'),'000','822') + isnull(TRF_ACCT,'')
                END as TRF_BANK --轉出入行庫代碼及帳號 (RFDM)TRF_BANK+TRF_ACCT
                ,isnull(CaseTrsRFDMRecv.NARRATIVE,'') as NARRATIVE  --備註 (RFDM) NARRATIVE
                ,CASE WHEN ISNULL(JRNL_NO,'') = ''
                    THEN ''
                    ELSE isnull(PARMCode.CodeNo,'99') 
		        END as PD_TYPE_DESC --產品別                             
                FROM CaseTrsQueryVersion 
				inner join CaseTrsRFDMRecv on CaseTrsQueryVersion.NewID = CaseTrsRFDMRecv.VersionNewID                                 
                LEFT JOIN BOPS000401Recv ON BOPS000401Recv.VersionNewID = CaseTrsRFDMRecv.VersionNewID
	                            and BOPS000401Recv.ACCT_NO = CaseTrsRFDMRecv.ACCT_NO
				-- 舊資料測試要補0 新資料若先補0 join 要拿掉0 
				left join CaseCustATMRecv 
							on  CaseCustATMRecv.[YBTXLOG_DATE] = CaseTrsRFDMRecv.TRAN_DATE
                                and CaseCustATMRecv.[FISC_SEQNO] = CaseTrsRFDMRecv.FISC_SEQNO and CaseCustATMRecv.Data_Date=@Data_DateKey 
				left join PARMCode on PARMCode.CodeType = 'PD_TYPE_DESC'
				        and PARMCode.CodeMemo = BOPS000401Recv.PD_TYPE_DESC
                WHERE
                    CaseTrsRFDMRecv.VersionNewID = @VersionNewIDKey
                    and  CaseTrsRFDMRecv.ACCT_NO=@ACCT_NOKey and ISNULL(QDateS, '')= @QDateSKey and ISNULL(QDateE, '')=@QDateEKey
                        ";

                // 清空容器
                CPC.Clear();
                CPC.Add(new CommandParameter("@VersionNewIDKey", VersionNewID));
                CPC.Add(new CommandParameter("@ACCT_NOKey", ACCT_NO));
                CPC.Add(new CommandParameter("@QDateSKey", QDateS));
                CPC.Add(new CommandParameter("@QDateEKey", QDateE));
                CPC.Add(new CommandParameter("@Data_DateKey", Data_Date));
                return base.Search(sqlSelect, CPC);
                //base.Parameter.Clear();
                //base.Parameter.Add(new CommandParameter("@VersionNewIDKey", VersionNewID));
                //base.Parameter.Add(new CommandParameter("@ACCT_NOKey", ACCT_NO));
                //base.Parameter.Add(new CommandParameter("@QDateSKey", QDateS));
                //base.Parameter.Add(new CommandParameter("@QDateEKey", QDateE));
                //base.Parameter.Add(new CommandParameter("@Data_DateKey", Data_Date));
                //return base.Search(sqlSelect);
            }
            catch (Exception ex)
            {
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "GetRFDMRecvData_SQL錯誤!!" + ex.Message.ToString());
                noticeMail(mailTo, "GetRFDMRecvData_SQL錯誤!!" + ex.Message.ToString());
                throw ex;
            }
        }

  
        /// <summary>
        /// 更新CaseTrsQueryVersion表RFDMSendStatus
        /// </summary>
        /// <param name="VersionNewID"></param>
        /// 
   
    
        public int UpdateTransEXCELFILEStatus(string strNewID)
        {
            try
            {
                CommandParameterCollection CPC = new CommandParameterCollection();
                string sql = @"Update CaseTrsQueryVersion 
                            set ModifiedDate=getdate() ,EXCEL_FILE =  'Y'  
                            where NewID = @NewID ";
                // 清空容器

                CPC.Clear();
                CPC.Add(new CommandParameter("@NewID", strNewID));
                return base.ExecuteNonQuery(sql, CPC);
                //base.Parameter.Clear();
                //return base.ExecuteNonQuery(sql);
            }
            catch (Exception ex)
            {
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "UpdateTransEXCELFILEStatus錯誤!!" + strNewID + ex.Message.ToString());
                noticeMail(mailTo, "UpdateTransEXCELFILEStatus錯誤!!" + strNewID + ex.Message.ToString());
                return -1;
            }
        }

        public int UpdateEXCELFILEStatus(string strNewID)
        {
            try
            {
                CommandParameterCollection CPC = new CommandParameterCollection();
                string sql = @"Update CaseTrsQueryVersion 
                                set ModifiedDate=getdate(),
                                EXCEL_FILE =  case when TransactionFlag = 'Y' then '1' else 'Y' end
                                where NewID = @NewIDUpdateEXCELFILES ";
                // 清空容器
                CPC.Clear();
                CPC.Add(new CommandParameter("@NewIDUpdateEXCELFILES", strNewID));
                return base.ExecuteNonQuery(sql, CPC);
                //base.Parameter.Clear();
                //base.Parameter.Add(new CommandParameter("@NewIDUpdateEXCELFILES", strNewID));
                //return base.ExecuteNonQuery(sql);
            }
            catch (Exception ex)
            {
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "UpdateEXCELFILEStatus_SQL錯誤!!" + strNewID + ex.Message.ToString());
                noticeMail(mailTo, "UpdateEXCELFILEStatus_SQL錯誤!!" + strNewID+ex.Message.ToString());
                return -1;
            }

        }
        public int UpdateEXCELFILEStatusNA(string strNewID)
        {
            try
            {
                CommandParameterCollection CPC = new CommandParameterCollection();
                string sql = @"Update CaseTrsQueryVersion 
                                set ModifiedDate=getdate(),
                                EXCEL_FILE =  'NA' 
                                where NewID = @NewID ";
                // 清空容器
                CPC.Clear();
                CPC.Add(new CommandParameter("@NewID", strNewID));
                return base.ExecuteNonQuery(sql, CPC);
            }
            catch (Exception ex)
            {
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "UpdateEXCELFILEStatus_SQL錯誤!!" + strNewID + ex.Message.ToString());
                noticeMail(mailTo, "UpdateEXCELFILEStatus_SQL錯誤!!" + strNewID + ex.Message.ToString());
                return -1;
            }

        }


        public int UpdatePDFFILEStatus(string strNewID)
        {
            try
            {
                CommandParameterCollection CPC = new CommandParameterCollection();
                string sql = @"Update CaseTrsQueryVersion 
                                set ModifiedDate=getdate() ,
                                PDF_FILE =  case when TransactionFlag = 'Y' then '1' else 'Y' end
                                where NewID = @NewID ";
                // 清空容器
                CPC.Clear();
                CPC.Add(new CommandParameter("@NewID", strNewID));
                return base.ExecuteNonQuery(sql,CPC);

                //base.Parameter.Clear();
                //base.Parameter.Add(new CommandParameter("@NewID", strNewID));
                //return base.ExecuteNonQuery(sql);
            }
            catch (Exception ex)
            {
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "UpdatePDFFILEStatus_SQL錯誤!!" + strNewID + ex.Message.ToString());
                noticeMail(mailTo, "UpdatePDFFILEStatus_SQL錯誤!!" + strNewID+ex.Message.ToString());
                return -1;
            }
        }
        public int UpdatePDFFILEStatusNA(string strNewID)
        {
            try
            {
                CommandParameterCollection CPC = new CommandParameterCollection();
                string sql = @"Update CaseTrsQueryVersion 
                                set ModifiedDate=getdate() ,
                                PDF_FILE = 'NA'   
                                where NewID = @NewID ";
                // 清空容器
                //base.Parameter.Clear();
                //base.Parameter.Add(new CommandParameter("@NewID", strNewID));
                //return base.ExecuteNonQuery(sql);
                CPC.Clear();
                CPC.Add(new CommandParameter("@NewID", strNewID));
                return base.ExecuteNonQuery(sql, CPC);
            }
            catch (Exception ex)
            {
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "UpdatePDFFILEStatus_SQL錯誤!!" + strNewID + ex.Message.ToString());
                noticeMail(mailTo, "UpdatePDFFILEStatus_SQL錯誤!!" + strNewID + ex.Message.ToString());
                return -1;
            }
        }
        public int UpdateTransPDFFILEStatus(string strNewID)
        {
            try
            {
                CommandParameterCollection CPC = new CommandParameterCollection();
                string sql = @"Update CaseTrsQueryVersion 
                             set ModifiedDate=getdate() ,PDF_FILE = 'Y'  
                             where NewID = @NewID ";
                // 清空容器
                CPC.Clear();
                CPC.Add(new CommandParameter("@NewID", strNewID));
                return base.ExecuteNonQuery(sql, CPC);
                //base.Parameter.Clear();
                //return base.ExecuteNonQuery(sql);
            }
            catch (Exception ex)
            {
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "UpdateTransPDFFILEStatus錯誤!!" + strNewID + ex.Message.ToString());
                noticeMail(mailTo, "UpdateTransPDFFILEStatus錯誤!!" + strNewID + ex.Message.ToString());
                return -1;
            }
        }

  
        #endregion

        #region 更新案件主檔的狀態和Version狀態更新成成功或者重查成功

        /// <summary>
        /// HTG發查回文產生成功 並且 RFDF回文產生成功，將Version狀態更新成成功或者重查成功
        /// </summary>
        public int Download66Error()
        {
            try
            {
                // 取出案件狀態04 但有成功壓縮的案件編號先壓狀態成功
                string sql = @"SELECT  distinct DocNo from  CaseTrsQueryVersion  c
                                inner join caseedocfile e on e.CaseId = c.CaseTrsNewID 
                                WHERE c.Status = '04'  and convert(varchar, ModifiedDate, 112) >=convert(varchar, getdate()-1, 112)";
                DataTable dt04 = base.Search(sql);
                if (dt04.Rows.Count > 0)
                {
                    string DocNos = "";
                    foreach (DataRow dr in dt04.Rows)
                    {
                        DocNos = DocNos + dr["DocNo"].ToString() + ", ";
                    }
                    m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "強制成功04狀態03 案號:" + DocNos);
                    // 將發查中的案件狀態【失敗】更新為【成功】
                    sql = @" Update  CaseTrsQueryVersion set Status = '03'
                             where NewId in ( SELECT  distinct NewId from  CaseTrsQueryVersion c
                                              inner join caseedocfile e on e.CaseId = c.CaseTrsNewID 
                                              WHERE c.Status = '04'  and convert(varchar, ModifiedDate, 112) >=convert(varchar, getdate()-1, 112) ) ";
                    return base.ExecuteNonQuery(sql);
                }
                else
                {
                    return 0;
                }
            }
            catch (Exception ex)
            {
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "強制結案04狀態03:" + ex.Message.ToString());
                noticeMail(mailTo, "Download66Error_錯誤!!" + ex.Message.ToString());
                return 0;
            }
        }
        public int ModifyVersion66Error()
        {
            try
            {
                // 取出案件狀態66未更新成功或失敗的案件編號
                string sql = @"SELECT  distinct DocNo from CaseTrsQueryVersion
                                 WHERE CaseTrsQueryVersion.Status = '66' ";
                DataTable dt66=base.Search(sql);  
                if (dt66.Rows.Count > 0)
                {
                    string DocNos = "";
                    foreach (DataRow dr in dt66.Rows)
                    {
                        DocNos = DocNos + dr["DocNo"].ToString()+", ";
                    }
                    m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "強制結案04狀態66 案號:" + DocNos);
                    // 將發查中的案件狀態更新為【失敗】
                    sql = @" UPDATE CaseTrsQueryVersion SET Status = '04'    WHERE  Status = '66'  ";
                    return  base.ExecuteNonQuery(sql);
                }
                else
                {
                    return 0;
                }
            }
            catch (Exception ex)
            {
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "強制結案04狀態66:" + ex.Message.ToString());
                noticeMail(mailTo, "ModifyVersion66Error_錯誤!!" + ex.Message.ToString());
                return 0;
            }
        }
        public void ModifyVersionStatus1(string DocNo, NewFileLog newfilelog,int intexcelzip,int intpdfzip)
        {
            try
            {                
                CommandParameterCollection CPC = new CommandParameterCollection();
                if (intexcelzip > 0 && intpdfzip > 0)
                {
                    string sql = @" UPDATE CaseTrsQueryVersion  SET Status = 
                             case when ( Status = '66') AND  EXCEL_FILE = 'Y'  and PDF_FILE = 'Y'
                                  then '03' 
	                              when ( Status = '66') AND  EXCEL_FILE = 'NA'  and PDF_FILE = 'NA'
	                              then '03' 
	                         else '04' end
                                where docno = @DocNo and Status = '66' ";
                    CPC.Clear();
                    CPC.Add(new CommandParameter("@DocNo", DocNo));
                    base.ExecuteNonQuery(sql, CPC);
                }
                else
                {
                    string sql = @" UPDATE CaseTrsQueryVersion  SET Status = '04' 
                                where docno = @DocNo and Status = '66' ";
                    CPC.Clear();
                    CPC.Add(new CommandParameter("@DocNo", DocNo));
                    base.ExecuteNonQuery(sql, CPC);
                }
            }
            catch (Exception ex)
            {
                newfilelog.Write(NewFileLog.WorkType.Work, NewFileLog.ErrorType.None, "更新狀態錯誤:" + ex.Message.ToString());
                noticeMail(mailTo, "ModifyVersionStatus1_SQL錯誤!!" + DocNo+ex.Message.ToString());
            }


        }

        /// <summary>
        /// 更新案件主檔的狀態
        /// </summary>
   
        /// <summary>
        /// 查詢Version檔成功的案件筆數，和案件下應該發查的資料筆數
        /// </summary>
        /// <returns></returns>
        /// <summary>
        /// 更新案件主檔的狀態
        /// </summary>
        /// <param name="Status"></param>
        /// <param name="NewID"></param>
        /// <returns></returns>
        #endregion

        #region 共用自定義方法

        protected void CreateRFDMPdf(string AccountName, DataTable dt, string strFileName)
        {
            try
            {
                //string strReturn = ""; 
                List<string> strListFileName = new List<string>();
                string strPath = strFileName + "\\" + AccountName + ".pdf";
                if (File.Exists(strPath))
                {
                    File.Delete(strPath);
                }

                List<ReportDataSource> mainDataSource = new List<ReportDataSource>();
                if (dt != null && dt.Rows.Count > 0)
                {
                    mainDataSource = new List<ReportDataSource>();
                    mainDataSource.Add(new ReportDataSource("dtSheet2", dt));

                    //SavePDFByLocalReport("RptRecieve02.rdlc", strPath, mainDataSource);
                    SavePDFByLocalReport("RptRecieve02.rdlc", strPath, mainDataSource);
                    strListFileName.Add(strPath);
                }
            }
            catch (Exception ex)
            {
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.Information, "CreateRFDMPdf錯誤!!" + AccountName + ex.Message.ToString());
                noticeMail(mailTo, "CreateRFDMPdf錯誤!!" + AccountName + ex.Message.ToString());
            }
        }


        protected void CreateRFDMExcel(string AccountName, DataTable dt, string strFileName,NewFileLog newfilelog)
        {
            try
            {
                string strPath = strFileName + "\\" + AccountName + ".xls";
                if (File.Exists(strPath))
                {
                    File.Delete(strPath);
                }

                HSSFWorkbook workbook = new HSSFWorkbook();
                MemoryStream ms = new MemoryStream();
                FileStream file = new FileStream(strPath, FileMode.Create);
                    ISheet sheet = workbook.CreateSheet("交易明細_" + AccountName.Substring(0, 12));

                    #region 設置字體樣式

                    //普通字體，無邊框
                    ICellStyle style = workbook.CreateCellStyle();
                    style.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Left;
                    style.WrapText = true;//自动换行
                    IFont font = workbook.CreateFont();
                    font.FontName = "微軟正黑體";
                    font.FontHeightInPoints = 10;
                    style.SetFont(font);

                    //
                    ICellStyle styleUnderLine = workbook.CreateCellStyle();
                    styleUnderLine.BorderBottom = NPOI.SS.UserModel.BorderStyle.Medium;

                    //表頭粗體字體
                    ICellStyle styleHead = workbook.CreateCellStyle();
                    styleHead.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Center;
                    styleHead.VerticalAlignment = VerticalAlignment.Center;
                    styleHead.WrapText = true;
                    IFont fontHead = workbook.CreateFont();
                    fontHead.FontName = "微軟正黑體";
                    fontHead.FontHeightInPoints = 12;
                    fontHead.Boldweight = short.MaxValue;
                    styleHead.SetFont(fontHead);

                    //有邊框的普通字體
                    ICellStyle styleTable = workbook.CreateCellStyle();
                    styleTable.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Center;
                    styleTable.VerticalAlignment = VerticalAlignment.Center;
                    styleTable.WrapText = true;//自动换行
                    styleTable.BorderBottom = NPOI.SS.UserModel.BorderStyle.Thin;
                    styleTable.BorderLeft = NPOI.SS.UserModel.BorderStyle.Thin;
                    styleTable.BorderRight = NPOI.SS.UserModel.BorderStyle.Thin;
                    styleTable.BorderTop = NPOI.SS.UserModel.BorderStyle.Thin;
                    styleTable.SetFont(font);

                    //紅色文字
                    ICellStyle styleRed = workbook.CreateCellStyle();
                    styleRed.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Center;
                    styleRed.VerticalAlignment = VerticalAlignment.Center;
                    styleRed.WrapText = true;
                    styleRed.BorderBottom = NPOI.SS.UserModel.BorderStyle.Thin;
                    styleRed.BorderLeft = NPOI.SS.UserModel.BorderStyle.Thin;
                    styleRed.BorderRight = NPOI.SS.UserModel.BorderStyle.Thin;
                    styleRed.BorderTop = NPOI.SS.UserModel.BorderStyle.Thin;
                    IFont fontRed = workbook.CreateFont();
                    fontRed.FontName = "微軟正黑體";
                    fontRed.FontHeightInPoints = 10;
                    fontRed.Color = HSSFColor.Red.Index;
                    styleRed.SetFont(fontRed);

                    //黃色背景
                    ICellStyle styleYellow = workbook.CreateCellStyle();
                    styleYellow.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Center;
                    styleYellow.VerticalAlignment = VerticalAlignment.Center;
                    styleYellow.WrapText = true;
                    styleYellow.BorderBottom = NPOI.SS.UserModel.BorderStyle.Thin;
                    styleYellow.BorderLeft = NPOI.SS.UserModel.BorderStyle.Thin;
                    styleYellow.BorderRight = NPOI.SS.UserModel.BorderStyle.Thin;
                    styleYellow.BorderTop = NPOI.SS.UserModel.BorderStyle.Thin;
                    styleYellow.FillForegroundColor = HSSFColor.Yellow.Index;
                    styleYellow.FillPattern = NPOI.SS.UserModel.FillPattern.SolidForeground;
                    styleYellow.SetFont(font);

                    //淡綠色背景
                    ICellStyle stylePaleGreen = workbook.CreateCellStyle();
                    stylePaleGreen.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Center;
                    stylePaleGreen.VerticalAlignment = VerticalAlignment.Center;
                    stylePaleGreen.WrapText = true;
                    stylePaleGreen.BorderBottom = NPOI.SS.UserModel.BorderStyle.Thin;
                    stylePaleGreen.BorderLeft = NPOI.SS.UserModel.BorderStyle.Thin;
                    stylePaleGreen.BorderRight = NPOI.SS.UserModel.BorderStyle.Thin;
                    stylePaleGreen.BorderTop = NPOI.SS.UserModel.BorderStyle.Thin;
                    stylePaleGreen.FillForegroundColor = HSSFColor.LightGreen.Index;
                    stylePaleGreen.FillPattern = NPOI.SS.UserModel.FillPattern.SolidForeground;
                    stylePaleGreen.SetFont(font);
                    #endregion

                    #region 每列的寬度調節
                    //要设置的实际列宽中加上列宽基数：0.72
                    //sheet1.SetColumnWidth(0,  50 * 256);  
                    // 在EXCEL文档中实际列宽为49.29
                    sheet.SetColumnWidth(0, (int)((20 + 0.72) * 256));//A
                    sheet.SetColumnWidth(1, (int)((20 + 0.72) * 256));//B
                    sheet.SetColumnWidth(2, (int)((15 + 0.72) * 256));//C
                    sheet.SetColumnWidth(3, (int)((15 + 0.72) * 256));//D
                    sheet.SetColumnWidth(4, (int)((15 + 0.72) * 256));//E
                    sheet.SetColumnWidth(5, (int)((15 + 0.72) * 256));//F
                    sheet.SetColumnWidth(6, (int)((60 + 0.72) * 256));//G
                    sheet.SetColumnWidth(7, (int)((60 + 0.72) * 256));//H
                    sheet.SetColumnWidth(8, (int)((15 + 0.72) * 256));//I
                    sheet.SetColumnWidth(9, (int)((15 + 0.72) * 256));//J
                    sheet.SetColumnWidth(10, (int)((12 + 0.72) * 256));//K
                    sheet.SetColumnWidth(11, (int)((20 + 0.72) * 256));//L
                    sheet.SetColumnWidth(12, (int)((20 + 0.72) * 256));//M
                    sheet.SetColumnWidth(13, (int)((20 + 0.72) * 256));//M
                    sheet.SetColumnWidth(14, (int)((10 + 0.72) * 256));//N
                    sheet.SetColumnWidth(15, (int)((10 + 0.72) * 256));//O
                    sheet.SetColumnWidth(16, (int)((10 + 0.72) * 256));//P
                    sheet.SetColumnWidth(17, (int)((20 + 0.72) * 256));//Q
                    sheet.SetColumnWidth(18, (int)((30 + 0.72) * 256));//R
                    sheet.SetColumnWidth(19, (int)((24 + 0.72) * 256));//S
                    sheet.SetColumnWidth(20, (int)((24 + 0.72) * 256));//S
                    #endregion

                    #region 第一行
                    sheet.AddMergedRegion(new CellRangeAddress(0, 2, 0, 4));
                    IRow row = sheet.CreateRow(0);
                    row.HeightInPoints = (float)28.5;
                    ICell cell = row.CreateCell(0);
                    cell.SetCellValue(" 存款交易明細 \r\n  ");
                    //必须设置style.WrapText = true 时\r\n才有效
                    cell.CellStyle = styleHead;

                    //下面的線條
                    //cell = row.CreateCell(5);
                    //cell.SetCellValue(" ");
                    //cell.CellStyle = styleHead;

                    //for (int i = 5; i <= 19; i++)
                    //{
                    //    cell = row.CreateCell(i);
                    //    cell.CellStyle = styleUnderLine;
                    //}
                    //sheet.AddMergedRegion(new CellRangeAddress(0, 0, 5, 13));

                    cell = row.CreateCell(20);
                    cell.SetCellValue("出表日期:");
                    cell.CellStyle = style;

                    cell = row.CreateCell(21);
                    cell.SetCellValue(DateTime.Now.ToString("yyyy/MM/dd hh:mm"));
                    cell.CellStyle = style;

                    //cell = row.CreateCell(17);
                    //cell.SetCellValue("查詢條件：全部");
                    //cell.CellStyle = style;


                    #endregion

                    #region 第二行
                    row = sheet.CreateRow(1);
                    cell = row.CreateCell(2);
                    cell.SetCellValue("戶名:");
                    cell.CellStyle = style;
                    cell = row.CreateCell(3);
                    //adam 20221117 防呆 null
                    if (dt.Rows[0]["CUSTOMER_NAME"] == null)
                    {
                        cell.SetCellValue("");
                    }
                    else
                    {
                        cell.SetCellValue(dt.Rows[0]["CUSTOMER_NAME"].ToString());
                    }
                    //cell.SetCellValue(dt.Rows[0]["CUSTOMER_NAME"].ToString());
                    cell.CellStyle = style;
                    //row.HeightInPoints = (float)28.5;
                    //cell = row.CreateCell(5);
                    //cell.SetCellValue(" ");
                    //cell.CellStyle = styleHead;

                    //cell = row.CreateCell(15);
                    //cell.SetCellValue(" ");
                    //cell.CellStyle = style;

                    //cell = row.CreateCell(16);
                    //cell.SetCellValue(System.DateTime.Now.ToShortDateString());
                    //cell.CellStyle = style;
                    #endregion

                    #region 第三行
                    row = sheet.CreateRow(2);
                    cell = row.CreateCell(2);
                    cell.SetCellValue("帳號:");
                    cell.CellStyle = style;
                    cell = row.CreateCell(3);
                    //adam 20221117 防呆 null
                    if (dt.Rows[0]["ACCT_NO"] == null)
                    {
                        cell.SetCellValue("");
                    }
                    else
                    {
                        cell.SetCellValue(dt.Rows[0]["ACCT_NO"].ToString());
                    }
                    //cell.SetCellValue(dt.Rows[0]["ACCT_NO"].ToString());
                    cell.CellStyle = style;

                    cell = row.CreateCell(8);
                    cell.SetCellValue("幣別:");
                    cell.CellStyle = style;
                    cell = row.CreateCell(9);
                    //adam 20221117 防呆 null
                    if (dt.Rows[0]["CURRENCY"] == null)
                    {
                        cell.SetCellValue("");
                    }
                    else
                    {
                        cell.SetCellValue(dt.Rows[0]["CURRENCY"].ToString());
                    }
                    //cell.SetCellValue(dt.Rows[0]["CURRENCY"].ToString());
                    cell.CellStyle = style;

                    cell = row.CreateCell(13);
                    cell.SetCellValue("查詢起日:");
                    cell.CellStyle = style;
                    cell = row.CreateCell(14);
                    //adam 20221117 防呆 null
                    if (dt.Rows[0]["QDateS"] == null)
                    {
                        cell.SetCellValue("");
                    }
                    else
                    {
                        cell.SetCellValue(dt.Rows[0]["QDateS"].ToString());
                    }
                    //cell.SetCellValue(dt.Rows[0]["QDateS"].ToString());
                    cell.CellStyle = style;

                    cell = row.CreateCell(17);
                    cell.SetCellValue("查詢迄日:");
                    cell.CellStyle = style;
                    cell = row.CreateCell(18);
                    //adam 20221117 防呆 null
                    if (dt.Rows[0]["QDateE"] == null)
                    {
                        cell.SetCellValue("");
                    }
                    else
                    {
                        cell.SetCellValue(dt.Rows[0]["QDateE"].ToString());
                    }
                    //cell.SetCellValue(dt.Rows[0]["QDateE"].ToString());
                    cell.CellStyle = style;

                    //cell = row.CreateCell(15);
                    //cell.SetCellValue("列印人 :");
                    //cell.CellStyle = style;

                    //cell = row.CreateCell(16);
                    //cell.SetCellValue("批次產生");
                    //cell.CellStyle = style;
                    #endregion

                    #region 中文表頭
                    row = sheet.CreateRow(3);
                    row.HeightInPoints = (float)28.5;
                    cell = row.CreateCell(0);
                    cell.SetCellValue("身分證統一編號");
                    cell.CellStyle = styleTable;

                    cell = row.CreateCell(1);
                    cell.SetCellValue("帳號");
                    cell.CellStyle = styleTable;

                    cell = row.CreateCell(2);
                    cell.SetCellValue("交易日期");
                    cell.CellStyle = styleTable;

                    cell = row.CreateCell(3);
                    cell.SetCellValue("交易時間");
                    cell.CellStyle = styleTable;

                    cell = row.CreateCell(4);
                    cell.SetCellValue("交易分行");
                    cell.CellStyle = styleTable;

                    cell = row.CreateCell(5);
                    cell.SetCellValue("交易代號");
                    cell.CellStyle = styleTable;

                    cell = row.CreateCell(6);
                    cell.SetCellValue("交易櫃員");
                    cell.CellStyle = styleTable;

                    cell = row.CreateCell(7);
                    cell.SetCellValue("摘要");
                    cell.CellStyle = styleTable;

                    cell = row.CreateCell(8);
                    cell.SetCellValue("支出金額");
                    cell.CellStyle = styleTable;

                    cell = row.CreateCell(9);
                    cell.SetCellValue("存入金額");
                    cell.CellStyle = styleTable;

                    cell = row.CreateCell(10);
                    cell.SetCellValue("餘額");
                    cell.CellStyle = styleTable;

                    cell = row.CreateCell(11);
                    cell.SetCellValue("幣別");
                    cell.CellStyle = styleTable;

                    cell = row.CreateCell(12);
                    cell.SetCellValue("轉出入行庫代碼及帳號");
                    cell.CellStyle = styleTable;
                    // 20221118 新增會員編號
                    cell = row.CreateCell(13);
                    cell.SetCellValue("合作機構會員編號");
                    cell.CellStyle = styleTable;

                    cell = row.CreateCell(14);
                    cell.SetCellValue("設備代理行");
                    cell.CellStyle = styleTable;

                    cell = row.CreateCell(15);
                    cell.SetCellValue("交易序號");
                    cell.CellStyle = styleTable;

                    cell = row.CreateCell(16);
                    cell.SetCellValue("機器編號");
                    cell.CellStyle = styleTable;

                    cell = row.CreateCell(17);
                    cell.SetCellValue("卡號");
                    cell.CellStyle = styleTable;

                    cell = row.CreateCell(18);
                    cell.SetCellValue("票號");
                    cell.CellStyle = styleTable;

                    cell = row.CreateCell(19);
                    cell.SetCellValue("備註");
                    cell.CellStyle = styleTable;

                    cell = row.CreateCell(20);
                    cell.SetCellValue("註記");
                    cell.CellStyle = styleTable;

                    cell = row.CreateCell(21);
                    cell.SetCellValue("附言");
                    cell.CellStyle = styleTable;
                    #endregion

                    #region 第二表頭


                    #endregion

                    #region 數據的添加
                    int IBeginCount = 4;//開始寫數據的行號
                                        // 20221118 新增會員編號
                    int IColumnCount = 22;//數據列數 21+1
                    int ITotalCount = dt.Rows.Count;//表的數據行數
                    int IFoot = IBeginCount + ITotalCount;//開始寫頁尾的數
                    for (int i = IBeginCount; i < IBeginCount + ITotalCount; i++)
                    {
                        // 一筆2列,要調整
                        row = sheet.CreateRow(i);
                        for (int j = 0; j < IColumnCount; j++)
                        {
                            cell = row.CreateCell(j);
                            if (dt.Rows[i - IBeginCount][j] == null)
                            {
                                cell.SetCellValue("");
                            }
                            else
                            {
                                cell.SetCellValue(dt.Rows[i - IBeginCount][j].ToString());
                            }

                        }
                    }
                    #endregion

                    workbook.Write(ms);
                    ms.WriteTo(file);
                    ms.Flush();
            }
            catch(Exception ex)
            {
                newfilelog.Write(NewFileLog.WorkType.Work, NewFileLog.ErrorType.None, "CreateRFDMExcel錯誤!!" + ex.Message.ToString());
                noticeMail(mailTo, "CreateRFDMExcel錯誤!!" + ex.Message.ToString());
            }
        }

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

                using (FileStream fs = new FileStream(fileName , FileMode.Create, FileAccess.Write))
                {
                    fs.Write(renderedBytes, 0, renderedBytes.Length);
                }
            }
            catch (Exception ex)
            {
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.Information, "SavePDFByLocalReport錯誤!!" + ex.Message.ToString());
                noticeMail(mailTo, "SavePDFByLocalReport錯誤!!" + ex.Message.ToString());
            }
        }

        protected void CreatePdf(string DocNo, DataTable dt, string strFileName)
        {
            //string strReturn = ""; 
            // 存放檔案名稱list
            List<string> strListFileName = new List<string>();
            string strPath = strFileName + @"\基本資料.pdf";
            if (File.Exists(strPath))
            {
                File.Delete(strPath);
            }
            List<ReportDataSource> mainDataSource = new List<ReportDataSource>();
            if (dt != null && dt.Rows.Count > 0)
            {
                mainDataSource = new List<ReportDataSource>();
                mainDataSource.Add(new ReportDataSource("dtSheet1", dt));

                SavePDFByLocalReport("RptRecieve01.rdlc", strPath, mainDataSource);

                strListFileName.Add(strPath);
            }
            
        }

        protected void CreateExcel(string DocNo,DataTable dt,string strFileName)
        {
            try
            {
                //string strReturn = ""; 

                string strPath = strFileName + @"\基本資料.xls";
                if (File.Exists(strPath))
                {
                    File.Delete(strPath);
                }
                HSSFWorkbook workbook = new HSSFWorkbook();
                MemoryStream ms = new MemoryStream();
                FileStream file = new FileStream(strPath, FileMode.Create);
                try
                {
                    ISheet sheet = workbook.CreateSheet("基本資料_" + DocNo);

                    #region 設置字體樣式

                    //普通字體，無邊框
                    ICellStyle style = workbook.CreateCellStyle();
                    style.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Left;
                    style.WrapText = true;//自动换行
                    IFont font = workbook.CreateFont();
                    font.FontName = "微軟正黑體";
                    font.FontHeightInPoints = 10;
                    style.SetFont(font);

                    //
                    ICellStyle styleUnderLine = workbook.CreateCellStyle();
                    styleUnderLine.BorderBottom = NPOI.SS.UserModel.BorderStyle.Medium;

                    //表頭粗體字體
                    ICellStyle styleHead = workbook.CreateCellStyle();
                    styleHead.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Center;
                    styleHead.VerticalAlignment = VerticalAlignment.Center;
                    styleHead.WrapText = true;
                    IFont fontHead = workbook.CreateFont();
                    fontHead.FontName = "微軟正黑體";
                    fontHead.FontHeightInPoints = 12;
                    fontHead.Boldweight = short.MaxValue;
                    styleHead.SetFont(fontHead);

                    //有邊框的普通字體
                    ICellStyle styleTable = workbook.CreateCellStyle();
                    styleTable.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Center;
                    styleTable.VerticalAlignment = VerticalAlignment.Center;
                    styleTable.WrapText = true;//自动换行
                    styleTable.BorderBottom = NPOI.SS.UserModel.BorderStyle.Thin;
                    styleTable.BorderLeft = NPOI.SS.UserModel.BorderStyle.Thin;
                    styleTable.BorderRight = NPOI.SS.UserModel.BorderStyle.Thin;
                    styleTable.BorderTop = NPOI.SS.UserModel.BorderStyle.Thin;
                    styleTable.SetFont(font);

                    //紅色文字
                    ICellStyle styleRed = workbook.CreateCellStyle();
                    styleRed.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Center;
                    styleRed.VerticalAlignment = VerticalAlignment.Center;
                    styleRed.WrapText = true;
                    styleRed.BorderBottom = NPOI.SS.UserModel.BorderStyle.Thin;
                    styleRed.BorderLeft = NPOI.SS.UserModel.BorderStyle.Thin;
                    styleRed.BorderRight = NPOI.SS.UserModel.BorderStyle.Thin;
                    styleRed.BorderTop = NPOI.SS.UserModel.BorderStyle.Thin;
                    IFont fontRed = workbook.CreateFont();
                    fontRed.FontName = "微軟正黑體";
                    fontRed.FontHeightInPoints = 10;
                    fontRed.Color = HSSFColor.Red.Index;
                    styleRed.SetFont(fontRed);

                    //黃色背景
                    ICellStyle styleYellow = workbook.CreateCellStyle();
                    styleYellow.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Center;
                    styleYellow.VerticalAlignment = VerticalAlignment.Center;
                    styleYellow.WrapText = true;
                    styleYellow.BorderBottom = NPOI.SS.UserModel.BorderStyle.Thin;
                    styleYellow.BorderLeft = NPOI.SS.UserModel.BorderStyle.Thin;
                    styleYellow.BorderRight = NPOI.SS.UserModel.BorderStyle.Thin;
                    styleYellow.BorderTop = NPOI.SS.UserModel.BorderStyle.Thin;
                    styleYellow.FillForegroundColor = HSSFColor.Yellow.Index;
                    styleYellow.FillPattern = NPOI.SS.UserModel.FillPattern.SolidForeground;
                    styleYellow.SetFont(font);

                    //淡綠色背景
                    ICellStyle stylePaleGreen = workbook.CreateCellStyle();
                    stylePaleGreen.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Center;
                    stylePaleGreen.VerticalAlignment = VerticalAlignment.Center;
                    stylePaleGreen.WrapText = true;
                    stylePaleGreen.BorderBottom = NPOI.SS.UserModel.BorderStyle.Thin;
                    stylePaleGreen.BorderLeft = NPOI.SS.UserModel.BorderStyle.Thin;
                    stylePaleGreen.BorderRight = NPOI.SS.UserModel.BorderStyle.Thin;
                    stylePaleGreen.BorderTop = NPOI.SS.UserModel.BorderStyle.Thin;
                    stylePaleGreen.FillForegroundColor = HSSFColor.LightGreen.Index;
                    stylePaleGreen.FillPattern = NPOI.SS.UserModel.FillPattern.SolidForeground;
                    stylePaleGreen.SetFont(font);
                    #endregion

                    #region 每列的寬度調節
                    //要设置的实际列宽中加上列宽基数：0.72
                    //sheet1.SetColumnWidth(0,  50 * 256);  
                    // 在EXCEL文档中实际列宽为49.29
                    sheet.SetColumnWidth(0, (int)((20 + 0.72) * 256));//A
                    sheet.SetColumnWidth(1, (int)((20 + 0.72) * 256));//B
                    sheet.SetColumnWidth(2, (int)((15 + 0.72) * 256));//C
                    sheet.SetColumnWidth(3, (int)((15 + 0.72) * 256));//D
                    sheet.SetColumnWidth(4, (int)((15 + 0.72) * 256));//E
                    sheet.SetColumnWidth(5, (int)((15 + 0.72) * 256));//F
                    sheet.SetColumnWidth(6, (int)((60 + 0.72) * 256));//G
                    sheet.SetColumnWidth(7, (int)((60 + 0.72) * 256));//H
                    sheet.SetColumnWidth(8, (int)((15 + 0.72) * 256));//I
                    sheet.SetColumnWidth(9, (int)((15 + 0.72) * 256));//J
                    #endregion


                    //sheet.GetRow(rowIdx).Cells.Clear();
                    //var cra = new NPOI.SS.Util.CellRangeAddress(rowIdx, rowIdx, 0, 69);
                    ////合併儲存格
                    //sheet.AddMergedRegion(cra);
                    //sheet.GetRow(rowIdx).GetCell(0).SetCellValue("***查無該項資訊***");
                    //sheet.GetRow(rowIdx).GetCell(0).CellStyle.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Center;
                    #region 第一行
                    sheet.AddMergedRegion(new CellRangeAddress(0, 1, 0, 11));
                    IRow row = sheet.CreateRow(0);
                    row.HeightInPoints = (float)22.5;
                    ICell cell = row.CreateCell(0);
                    cell.SetCellValue(" 存款基本資料 \r\n  ");
                    //必须设置style.WrapText = true 时\r\n才有效
                    cell.CellStyle = styleHead;

                    //下面的線條
                    //cell = row.CreateCell(5);
                    //cell.SetCellValue(" ");
                    //cell.CellStyle = styleHead;

                    for (int i = 5; i <= 8; i++)
                    {
                        cell = row.CreateCell(i);
                        cell.CellStyle = styleUnderLine;
                    }
                    //sheet.AddMergedRegion(new CellRangeAddress(0, 0, 5, 8));

                    cell = row.CreateCell(9);
                    cell.SetCellValue("出表日期:");
                    cell.CellStyle = style;

                    cell = row.CreateCell(10);
                    cell.SetCellValue(DateTime.Now.ToString("yyyy/MM/dd hh:mm"));
                    cell.CellStyle = style;

                    //cell = row.CreateCell(17);
                    //cell.SetCellValue("查詢條件：全部");
                    //cell.CellStyle = style;


                    #endregion

                    #region 第二行
                    row = sheet.CreateRow(1);
                    cell = row.CreateCell(9);
                    cell.SetCellValue("出表日期:");
                    cell.CellStyle = style;

                    cell = row.CreateCell(10);
                    cell.SetCellValue(DateTime.Now.ToString("yyyy/MM/dd hh:mm"));
                    cell.CellStyle = style;
                    //row.HeightInPoints = (float)28.5;
                    //cell = row.CreateCell(5);
                    //cell.SetCellValue(" ");
                    //cell.CellStyle = styleHead;

                    //cell = row.CreateCell(15);
                    //cell.SetCellValue(" ");
                    //cell.CellStyle = style;

                    //cell = row.CreateCell(16);
                    //cell.SetCellValue(System.DateTime.Now.ToShortDateString());
                    //cell.CellStyle = style;
                    #endregion

                    #region 第三行
                    row = sheet.CreateRow(2);
                    cell = row.CreateCell(9);
                    cell.SetCellValue("出表日期:");
                    cell.CellStyle = style;

                    cell = row.CreateCell(10);
                    cell.SetCellValue(DateTime.Now.ToString("yyyy/MM/dd hh:mm"));
                    cell.CellStyle = style;
                    //row.HeightInPoints = (float)28.5;
                    //cell = row.CreateCell(5);
                    //cell.SetCellValue(" ");
                    //cell.CellStyle = styleHead;
                    //cell = row.CreateCell(6);
                    //cell.SetCellValue("");
                    //cell.CellStyle = styleHead;

                    //cell = row.CreateCell(15);
                    //cell.SetCellValue("列印人 :");
                    //cell.CellStyle = style;

                    //cell = row.CreateCell(16);
                    //cell.SetCellValue("批次產生");
                    //cell.CellStyle = style;
                    #endregion

                    #region 中文表頭
                    row = sheet.CreateRow(3);
                    row.HeightInPoints = (float)28.5;
                    cell = row.CreateCell(0);
                    cell.SetCellValue("身分證統一編號/居留證號");
                    cell.CellStyle = styleTable;

                    cell = row.CreateCell(1);
                    cell.SetCellValue("戶名");
                    cell.CellStyle = styleTable;

                    cell = row.CreateCell(2);
                    cell.SetCellValue("出生日期");
                    cell.CellStyle = styleTable;

                    cell = row.CreateCell(3);
                    cell.SetCellValue("居住電話");
                    cell.CellStyle = styleTable;

                    cell = row.CreateCell(4);
                    cell.SetCellValue("戶籍電話");
                    cell.CellStyle = styleTable;

                    cell = row.CreateCell(5);
                    cell.SetCellValue("行動電話");
                    cell.CellStyle = styleTable;

                    cell = row.CreateCell(6);
                    cell.SetCellValue("通訊地址");
                    cell.CellStyle = styleTable;

                    cell = row.CreateCell(7);
                    cell.SetCellValue("戶籍地址");
                    cell.CellStyle = styleTable;

                    cell = row.CreateCell(8);
                    cell.SetCellValue("負責人姓名");
                    cell.CellStyle = styleTable;

                    cell = row.CreateCell(9);
                    cell.SetCellValue("統一編號");
                    cell.CellStyle = styleTable;


                    #endregion

                    #region 第二表頭




                    #endregion
                    // 身分證統一編號
                    //strRow["CUST_ID_NO"] = ChangeValue(dt401Recv.Rows[q]["CUST_ID_NO"].ToString(), 11);
                    // 開戶行總、分支機構代碼
                    //strRow["BRANCH_NO"] = ChangeValue(dt401Recv.Rows[q]["BRANCH_NO"].ToString(), 7);
                    // 存款種類	X(02)
                    //string strPD_TYPE_DESC = ChangeValue(dt401Recv.Rows[q]["PD_TYPE_DESC"].ToString(), 2);
                    //strPD_TYPE_DESC = (string.IsNullOrEmpty(strPD_TYPE_DESC.Trim()) ? "99" : strPD_TYPE_DESC);
                    //strRow["PD_TYPE_DESC"] = strPD_TYPE_DESC;
                    // 幣別 X(03)
                    //strRow["CURRENCY"] = ChangeValue(dt401Recv.Rows[q]["CURRENCY"].ToString(), 3);
                    // 戶名  X(60)
                    //strRow["CUSTOMER_NAME"] = ChangeChiness(dt401Recv.Rows[q]["CUSTOMER_NAME"].ToString(), 60);
                    // 住家電話    X(20)
                    //strRow["REGTEL_NO"] = ChangeValue(dt401Recv.Rows[q]["REGTEL_NO"].ToString(), 20);
                    // 住家電話    X(20)
                    //strRow["NIGTEL_NO"] = ChangeValue(dt401Recv.Rows[q]["NIGTEL_NO"].ToString(), 20);
                    // 行動電話    X(20)
                    //strRow["MOBIL_NO"] = ChangeValue(dt401Recv.Rows[q]["MOBIL_NO"].ToString(), 20);
                    // 戶籍地址    X(200)
                    //strRow["CUST_ADD"] = ChangeChiness(dt401Recv.Rows[q]["CUST_ADD"].ToString(), 200);
                    // 通訊地址    X(200)
                    //["COMM_ADDR"] = ChangeChiness(dt401Recv.Rows[q]["COMM_ADDR"].ToString(), 200);
                    //strRow["DATE_OF_BIRTH"] = ChangeValue(dt401Recv.Rows[q]["DATE_OF_BIRTH"].ToString(), 12);
                    //strRow["MASTER_NAME"] = ChangeValue(dt401Recv.Rows[q]["MASTER_NAME"].ToString(), 20); ;//負責人
                    //strRow["MASTER_ID"] = ChangeValue(dt401Recv.Rows[q]["MASTER_ID"].ToString(), 20);//負責人
                    //strRow["IdNo02"] = ChangeValue(dt401Recv.Rows[q]["IdNo02"].ToString(), 20);//居留證
                    #region 數據的添加
                    int IBeginCount = 4;//開始寫數據的行號
                                        //int IColumnCount = 10;//數據列數
                    int ITotalCount = dt.Rows.Count;//表的數據行數
                    int IFoot = IBeginCount + ITotalCount;//開始寫頁尾的數
                    for (int i = IBeginCount; i < IBeginCount + ITotalCount; i++)
                    {
                        // 一筆2列,要調整
                        row = sheet.CreateRow(i);
                        //for (int j = 0; j < IColumnCount; j++)
                        //{
                        cell = row.CreateCell(0);
                        bool bENg = IsEG(dt.Rows[i - IBeginCount]["CUST_ID_NO"].ToString().Substring(dt.Rows[i - IBeginCount]["CUST_ID_NO"].ToString().Length - 3, 3));
                        if (bENg == true)
                        {
                            cell.SetCellValue(dt.Rows[i - IBeginCount]["CUST_ID_NO"].ToString() + "/" + dt.Rows[i - IBeginCount]["IdNo02"].ToString()); //"身分證統一編號/居留證號");
                        }
                        else
                        {
                            cell.SetCellValue(dt.Rows[i - IBeginCount]["CUST_ID_NO"].ToString()); //"身分證統一編號/居留證號");
                        }
                        cell.CellStyle = styleTable;

                        cell = row.CreateCell(1);
                        cell.SetCellValue(dt.Rows[i - IBeginCount]["CUSTOMER_NAME"].ToString());//戶名");
                        cell.CellStyle = styleTable;

                        cell = row.CreateCell(2);
                        cell.SetCellValue(dt.Rows[i - IBeginCount]["DATE_OF_BIRTH"].ToString());//"出生日期");
                        cell.CellStyle = styleTable;

                        cell = row.CreateCell(3);
                        cell.SetCellValue(dt.Rows[i - IBeginCount]["NIGTEL_NO"].ToString());//"居住電話");
                        cell.CellStyle = styleTable;

                        cell = row.CreateCell(4);
                        cell.SetCellValue(dt.Rows[i - IBeginCount]["REGTEL_NO"].ToString());//"戶籍電話");
                        cell.CellStyle = styleTable;

                        cell = row.CreateCell(5);
                        cell.SetCellValue(dt.Rows[i - IBeginCount]["MOBIL_NO"].ToString());//"行動電話");
                        cell.CellStyle = styleTable;

                        cell = row.CreateCell(6);
                        cell.SetCellValue(dt.Rows[i - IBeginCount]["COMM_ADDR"].ToString());//"通訊地址");
                        cell.CellStyle = styleTable;

                        cell = row.CreateCell(7);
                        cell.SetCellValue(dt.Rows[i - IBeginCount]["CUST_ADD"].ToString());//"戶籍地址");
                        cell.CellStyle = styleTable;

                        cell = row.CreateCell(8);
                        cell.SetCellValue(dt.Rows[i - IBeginCount]["MASTER_NAME"].ToString());//"負責人姓名");
                        cell.CellStyle = styleTable;

                        cell = row.CreateCell(9);
                        cell.SetCellValue(dt.Rows[i - IBeginCount]["MASTER_ID"].ToString());//"統一編號");
                        cell.CellStyle = styleTable;

                    }
                    #endregion

                    #region 寫入表尾
                    //sheet.AddMergedRegion(new CellRangeAddress(IFoot, IFoot + 4, 0, 18));
                    row = sheet.CreateRow(IFoot);
                    cell = row.CreateCell(0);
                    cell.SetCellValue(@"");
                    cell.CellStyle = styleRed;
                    #endregion


                    workbook.Write(ms);
                    ms.WriteTo(file);
                    ms.Flush();

                }
                finally
                {
                    ms.Close();
                    file.Close();
                }
            }
            catch (Exception ex)
            {
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.Information, "CreateExcel錯誤!!" + DocNo + ex.Message.ToString());
                noticeMail(mailTo, "CreateExcel錯誤!!" + DocNo+ex.Message.ToString());
            }

        }

        public bool IsEG(string word)
        {
            Regex EG = new Regex("[^A-Za-z]");
            return !EG.IsMatch(word);
        }
        /// <summary>
        /// 將符合條件的內容拼接到指定txt文件中
        /// </summary>
        /// <param name="FileName">指定txt文件</param>
        /// <param name="Content">拼接的內容</param>
        /// <param name="DataCount">拼接資料筆數</param>
        public void AppendContent(string FileName, string Content, int DataCount)
        {
            // 資料筆數>0時,向對應的txt文件追加內容
            if (DataCount > 0)
            {
                // 文件路徑
                string filePath = txtFilePath + "\\" + FileName + "基本資料.csv";

                #region 向指定文件追加TXT內容

                // 判斷文件是否存在，若不存在，則創建
                if (!File.Exists(filePath))
                {
                    FileStream cFile = File.Create(filePath);
                    cFile.Dispose();
                    cFile.Close();
                }

                // 記錄相關信息
                FileStream p_FS = new FileStream(filePath, FileMode.Append);

                StreamWriter p_SW = new StreamWriter(p_FS, System.Text.Encoding.GetEncoding("UTF-8"));

                DateTime time = DateTime.Now;

                // 追加拼接內容
                p_SW.Write(Content);

                // 關閉資料流
                p_SW.Close();
                p_FS.Dispose();
                p_FS.Close();
                #endregion
            }
        }

      

        //讀取目錄下所有檔案
        private static System.Collections.ArrayList GetFiles(string path)
        {
            System.Collections.ArrayList files = new System.Collections.ArrayList();

            if (Directory.Exists(path))
            {
                files.AddRange(Directory.GetFiles(path));
            }

            return files;
        }

        //建立目錄
        private static void CreateDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }


        ///
        /// 壓縮檔案
        ///
        ///壓縮檔案路徑
        ///密碼
        ///註解
        private void ZipFiles(string path, string password, string comment)
        {
            ZipOutputStream zos = null;

            try
            {
                string zipPath = path + @"\" + Path.GetFileName(path) + ".zip";
                System.Collections.ArrayList  files = GetFiles(path);
                zos = new ZipOutputStream(File.Create(zipPath));
                if (password != null && password != string.Empty) zos.Password = password;
                if (comment != null && comment != "") zos.SetComment(comment);
                zos.SetLevel(9);//Compression level 0-9 (9 is highest)
                byte[] buffer = new byte[4096];

                foreach (string f in files)
                {
                    ZipEntry entry = new ZipEntry(Path.GetFileName(f));
                    entry.DateTime = DateTime.Now;
                    zos.PutNextEntry(entry);
                    FileStream fs = File.OpenRead(f);
                    int sourceBytes;

                    do
                    {
                        sourceBytes = fs.Read(buffer, 0, buffer.Length);
                        zos.Write(buffer, 0, sourceBytes);
                    } while (sourceBytes > 0);

                    fs.Close();
                    fs.Dispose();
                }
            }

            catch (Exception ex)
            {
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.Information, "ZipFiles錯誤: " + ex.Message);
                noticeMail(mailTo, "ZipFiles錯誤!!" + ex.Message.ToString());
            }
            finally
            {
                zos.Finish();
                zos.Close();
                zos.Dispose();
            }
        }

        ///
        /// 解壓縮檔案
        ///
        ///解壓縮檔案目錄路徑
        ///密碼
        private void UnZipFiles(string path, string password)
        {
            ZipInputStream zis = null;

            try
            {
                string unZipPath = path.Replace(".zip", "");
                CreateDirectory(unZipPath);
                zis = new ZipInputStream(File.OpenRead(path));
                if (password != null && password != string.Empty) zis.Password = password;
                ZipEntry entry;

                while ((entry = zis.GetNextEntry()) != null)
                {
                    string filePath = unZipPath + @"\" + entry.Name;

                    if (entry.Name != "")
                    {
                        FileStream fs = File.Create(filePath);
                        int size = 2048;
                        byte[] buffer = new byte[2048];
                        while (true)
                        {
                            size = zis.Read(buffer, 0, buffer.Length);
                            if (size > 0) { fs.Write(buffer, 0, size); }
                            else { break; }
                        }

                        fs.Close();
                        fs.Dispose();
                    }
                }
            }

            catch (Exception ex)
            {
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.Information, "UnZipFiles錯誤: " + ex.Message);
                noticeMail(mailTo, "UnZipFiles錯誤!!" + ex.Message.ToString());
            }
            finally
            {
                zis.Close();
                zis.Dispose();
            }
        }

    
        /// <summary>
        /// 轉換含有正負號字串
        /// </summary>
        /// <param name="strValue">指定字串</param>
        /// <param name="strValueLen">指定長度</param>
        /// <param name="floatLen">小數位數</param>
        /// <param name="flag">true:正負號在最後一位,false:正負號在第一位</param>
        /// <returns></returns>
        public string ChangeNumber(string strValue, int strValueLen, int floatLen, bool flag)
        {
            string strResult = "";

            // 判斷是否有值,沒有值固定顯示"+000000000000000"
            if (!string.IsNullOrEmpty(strValue))
            {
                // 正負號變量
                string strLastFlag = "";

                // 正負號在最後一位
                if (flag)
                {
                    // 截取正負號
                    strLastFlag = strValue.Substring(strValue.Length - 1, 1);

                    // 去掉正負號
                    strValue = strValue.Substring(0, strValue.Length - 1);
                }
                else
                {
                    strLastFlag = strValue.Substring(0, 1);

                    // 去掉正負號
                    strValue = strValue.Substring(1, strValue.Length - 1);
                }

                // 判斷是否有值,如果沒有值,就在正負號後追加半形空白
                if (strValue != "")
                {
                    #region 獲取小數位
                    // 獲取小數點位置
                    int dotIndex = strValue.IndexOf('.');

                    // 截取小數位
                    string strFloat = strValue.Substring(dotIndex + 1);

                    // 判斷小數位長度,如果大於指定長度,就截取指定長度,否則在右邊補充0
                    int floatNum = floatLen - strFloat.Length;
                    strFloat = floatNum > 0 ? strFloat + AddSpace(floatNum, "0") : strFloat.Substring(0, floatLen);

                    #endregion

                    #region 獲取整數位
                    // 截取整數位
                    string strInt = strValue.Substring(0, dotIndex);

                    // 計算正整數最大長度
                    int intMaxLength = strValueLen - floatLen - 1;

                    // 判斷整數位長度,如果大於指定長度,就截取指定長度,否則在左邊補充0
                    int inttNum = intMaxLength - strInt.Length;
                    strInt = inttNum > 0 ? AddSpace(inttNum, "0") + strInt : strInt.Substring(0, intMaxLength);

                    #endregion

                    strResult = strLastFlag + strInt + strFloat;
                }
                else
                {
                    strResult = strLastFlag + AddSpace(strValueLen - 1, "0");
                }
            }
            else
            {
                strResult = "+" + AddSpace(strValueLen - 1, "0");
            }
            return strResult;
        }

        /// <summary>
        /// 用Byte截長度
        /// </summary>
        /// <param name="a_SrcStr"></param>
        /// <param name="a_Cnt"></param>
        /// <returns></returns>
        public string ChangeChiness(string strValue, int strLength)
        {
            string strResult = "";

            if (!string.IsNullOrEmpty(strValue))
            {
                Encoding l_Encoding = Encoding.GetEncoding("big5");
                byte[] l_byte = l_Encoding.GetBytes(strValue);

                strResult = l_byte.Length > strLength ? l_Encoding.GetString(l_byte, 0, strLength) : l_Encoding.GetString(l_byte, 0, l_byte.Length) + AddSpace(strLength - l_byte.Length, strNull);

                strResult = strResult.Replace("?", strNull);
            }
            else
            {
                strResult = AddSpace(strLength, strNull);
            }

            return strResult;
        }
        #endregion

        #region 產生回文文檔

        /// <summary>
        /// 列印案件的PDF檔案
        /// </summary>
    
    

 
        /// <summary>
        /// 將PDF列印狀態更新到系統中
        /// </summary>
        /// <param name="VersionNewID"></param>
        /// <param name="strPDFStatus">W: 沒有回文編號</param>
        /// <param name="strStatus">77：未獲取回文字號</param>
        /// <returns></returns>
    
   
        #endregion
    }
}