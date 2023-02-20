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

namespace CTBC.WinExe.CSFS.HistoryFile
{
    class Program : BaseBusinessRule
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
        //private static ImportEDocBiz _ImportEDocBiz;
        // 獲取log路徑
        private static FileLog m_fileLog = new FileLog(ConfigurationManager.AppSettings["fileLog"], AppDomain.CurrentDomain.FriendlyName.Replace(".vshost", "").Replace(".exe", ""));

        // 半形空白變量
        public string strNull = " ";

        public DataTable CurrencyList = null;
        //private object CellBorderType;

        public object FillPatternType { get; private set; }
        #endregion

        /// <summary>
        /// 程序入口
        /// </summary>
        static void Main()
        {
            Program mainProgram = new Program();

            // 產生Excel
            mainProgram.Process();

        }

        /// <summary>
        /// 主方法
        /// </summary>
        private void Process()
        {

            DateTime thenow = DateTime.Now;
            m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, DateTime.Now.ToString());
            try
            {
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
            }
            catch (Exception ex)
            {
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "工作日參數設定為空:"+ex.Message.ToString());
                //throw ex;
            }

            //20211029 檢查81019Receive 是否有資料,只要有一筆就要去執行,否則Retry
            int isReturn = 0;  
            
            for (int retry = 0; retry < intRetry; retry++)
            {
                isReturn = check81019();
                //測試用 isReturn = 0;
                if (isReturn < 1)
                {
                    m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "81019 is No Data !!"+"Retry:"+(retry+1).ToString());
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

            //thenow = new DateTime(2020, 11, 19);
            ///增加多工處理將原本Status = 02 改成 66 ,多工改取 66
            UpdateHTGDataStatus();
            UpdateRFDMDataStatus();
            ///
            // 獲取幣別代碼檔資料
            CurrencyList = GetParmCodeCurrency();

            // 程序開始記入log
            m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------歷史紀錄產生開始------");

            // 判斷文件夾是否存在，若不存在，則創建
            if (!Directory.Exists(txtFilePath)) Directory.CreateDirectory(txtFilePath);

            // 基本資料
            try
              { 
              ExportHtgExcel();
              }
            catch (Exception ex)
            {
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "基本資料EXCEL產檔錯誤:"+ex.ToString());
                //throw ex;
            }
            // 交易明細
            try
            { 
              ExportRFDMExcel();
            }
            catch (Exception ex)
            {
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "交易資料EXCEL產檔錯誤:" + ex.ToString());
                //throw ex;
            }
            //
            try
            {
                ExcelZip();
            }
            catch (Exception ex)
            {
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "EXCEL ZIP錯誤:" + ex.ToString());
               // throw ex;
            }

            ////// 基本資料
            try
            {
                ExportHtgPdf();
            }
            catch (Exception ex)
            {
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "基本資料PDF產檔錯誤:" + ex.ToString());
                //throw ex;
            }

            ////// 交易明細
            try
            {
                ExportRFDMPdf();
            }
            catch (Exception ex)
            {
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "交易資料PDF產檔錯誤:" + ex.ToString());
                //throw ex;
            }
            //
            try
            {
                PdfZip();
            }
            catch (Exception ex)
            {
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "PDF ZIP錯誤:" + ex.ToString());
                //throw ex;
            }
            //更新狀態88,99
            try
            {
                ModifyHTGRFDMStatus();

                // ZIP 產生成功 更新 為 狀態 03 成功 EXCEL 或 PDF 有一個Y就是成功,其他失敗
                ModifyVersionStatus1();
            }
            catch (Exception ex)
            {
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "更新狀態錯誤:" + ex.ToString());
                //throw ex;
            }
            // 程序結束記入log
            m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, DateTime.Now.ToString());
            m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------產檔結束------");
        }
        public int check81019()
        {

            string sql = @"select Count(*) from CaseMaster								
                                left join
                                  CaseTrsQueryVersion
                                on CaseMaster.CaseID = CaseTrsQueryVersion.CaseTrsNewID
								inner join  BOPS081019Send  
								on CaseTrsQueryVersion.NewID = BOPS081019Send.VersionNewID
                                where
                                  TransactionFlag = 'Y'
                                  and CaseTrsQueryVersion.RFDMSendStatus = '8'
                                  and CaseTrsQueryVersion.Status = '02'
                                  and BOPS081019Send.ATMFlag = 'Y'
                                  and CaseMaster.Status is not Null ";
            base.Parameter.Clear();
            //int val = base.ExecuteNonQuery(sql);\
            var ret = base.Search(sql);
            return Convert.ToInt32(ret.Rows[0][0]);
        }

        internal bool? getWorkDay(DateTime BizDate)
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




        #region HTG回文


        public void ExportHtgPdf()
        {
            m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------PDF讀取基本資料電文結果（PDF 存款帳戶資料）開始------");

            // 查詢產生電文檔案的資料
            DataTable dtHTGResult = GetHTGData();

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
                            // 判斷文件夾是否存在，若不存在，則創建
                            if (!Directory.Exists(pdfFilePath + "\\" + dtHTGResult.Rows[p]["DocNo"].ToString())) Directory.CreateDirectory(pdfFilePath + "\\" + dtHTGResult.Rows[p]["DocNo"].ToString());
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
                                    string strIdType = dt401Recv.Rows[q]["ID_TYPE_"+n.ToString("00")].ToString().Trim();
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


                                // 記錄LOG
                                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------PDF :" + txtDocIDPath + "文件中增加基本資料!");
                                dt.Clear();
                                DataCount1++;
                            }

                        }
                        else
                        {
                            dt.Clear();
                            m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "DocNo:"+dtHTGResult.Rows[p]["DocNo"].ToString()+" CustID: "+dtHTGResult.Rows[p]["CustId"].ToString()+ " Account: " + dtHTGResult.Rows[p]["CustAccount"].ToString() + "與本行無存款往來");
                        }

                        // 更新HTGSendStatus狀態
                        if (dt401Recv != null && dt401Recv.Rows.Count > 0)
                        {
                            int HTGCount = UpdatePDFFILEStatus(dtHTGResult.Rows[p]["CaseTrsNewID"].ToString());

                            // 記錄LOG
                            if (HTGCount > 0)
                            {
                                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------PDF 基本資料產檔完成");
                            }
                            else
                            {
                                //UpdateNoPDFFILEStatus(dtHTGResult.Rows[p]["CaseTrsNewID"].ToString());
                                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------無PDF 基本資料產檔");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------ID:" + dtHTGResult.Rows[p]["CustId"].ToString() + "產檔錯誤:" + ex.ToString());
                        int HTGInt = UpdateNoEXCELFILEHTGMessage(dtHTGResult.Rows[p]["CaseTrsNewID"].ToString(), ex.ToString());
                        //throw;
                    }
                }
            }
            else
            {
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------沒有查到PDF基本資料（存款帳戶開戶資料）");
            }

            m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------匯出PDF基本資料（存款帳戶開戶資料）結束------");
        }

        // zip
        public void PdfZip()
        {
            m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------產生Pdf zip 並上傳CaseEdocFile開始------");

            // 查詢產生電文檔案的資料
            DataTable dtHTGzipResult = GetPDFzip();

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
                        //if (!File.Exists(pdfDocDirPath + ".zip"))
                        //{
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
                        m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------上傳PDF: " + dtHTGzipResult.Rows[p]["DocNo"].ToString() + "  成功------");
                    }
                }
            }
        }

        
        public void ExcelZip()
        {
            m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------產生Excel zip 並上傳CaseEdocFile開始------");

            // 查詢產生電文檔案的資料
            DataTable dtHTGzipResult = GetHTGzip();

            /// 以基本資料為案號
            /// 
            string zippath = txtFilePath;//txtDocDirPath;//要壓縮檔案的目錄路徑
            if (dtHTGzipResult != null && dtHTGzipResult.Rows.Count > 0)
            {
                for (int p = 0; p < dtHTGzipResult.Rows.Count; p++)
                {
                    string txtDocDirPath = zippath + "\\"+dtHTGzipResult.Rows[p]["DocNo"].ToString();
                    if (Directory.Exists(txtDocDirPath))                     //如果当前是文件夹，递归
                    {
                        if (File.Exists(txtDocDirPath + ".zip"))
                        {
                            File.Delete(txtDocDirPath + ".zip");
                        }
                        //if (!File.Exists(txtDocDirPath + ".zip"))
                        //{
                            CreateZip(txtDocDirPath, txtDocDirPath);
                        //}
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
                        m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------上傳EXCEL :" + dtHTGzipResult.Rows[p]["DocNo"].ToString() + "  成功------");
                    }
                }
            }
        }
        // 產生EXCEL 
        public void ExportHtgExcel()
        {
            m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------讀取EXCEL電文-基本資料（存款帳戶開戶資料）開始------");


            // 查詢產生電文檔案的資料
            DataTable dtHTGResult = GetHTGData();

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
                            // 判斷文件夾是否存在，若不存在，則創建
                            if (!Directory.Exists(txtFilePath + "\\" + dtHTGResult.Rows[p]["DocNo"].ToString())) Directory.CreateDirectory(txtFilePath + "\\" + dtHTGResult.Rows[p]["DocNo"].ToString());
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
                                // 
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
                                // 記錄LOG
                                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------向" + txtDocIDPath + "EXCEL文件中增加基本資料!");
                                dt.Clear();
                                DataCount1++;
                            }

                        }
                        else
                        {
                            dt.Clear();
                            m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "DocNo:" + dtHTGResult.Rows[p]["DocNo"].ToString() + " CustID: " + dtHTGResult.Rows[p]["CustId"].ToString() + " Account: " + dtHTGResult.Rows[p]["CustAccount"].ToString() + "與本行無存款往來");
                        }
                        // 更新EXCEL FILE Status狀態
                        if (dt401Recv.Rows.Count > 0)
                        {
                            int HTGInt = UpdateEXCELFILEStatus(dtHTGResult.Rows[p]["CaseTrsNewID"].ToString());
                            if (HTGInt > 0)
                            {
                                // 記錄LOG
                                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------  EXCEL_FILE  基本資料產檔完成  ");
                            }
                        }
                        else
                        {
                            //int HTGInt = UpdateNoEXCELFILEStatus(dtHTGResult.Rows[p]["CaseTrsNewID"].ToString());
                            m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------  EXCEL_FILE  無基本資料產檔 ");
                        }
                    }
                    catch (Exception ex)
                    {
                        m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------ID:" + dtHTGResult.Rows[p]["CustId"].ToString() + "產檔錯誤:" + ex.ToString());
                        int HTGInt = UpdateNoEXCELFILEHTGMessage(dtHTGResult.Rows[p]["CaseTrsNewID"].ToString(), ex.ToString());
                        //throw;
                    }
                }
            }
            else
            {
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------沒有查到匯出（存款帳戶開戶資料）");
            }
  

            m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------匯出EXECL基本資料（存款帳戶開戶資料）結束------");
        }

    

        public DataTable GetHTGzip()
        {
            string sqlSelect = @"
           select
                                 distinct CaseTrsNewID,CaseMaster.CaseNo as DocNo  --,CaseTrsQueryVersion.NewID
                                from
                                  CaseMaster
                                left join
                                  CaseTrsQueryVersion
                                on CaseMaster.CaseID = CaseTrsQueryVersion.CaseTrsNewID
                                where
                                  (( OpenFlag = 'Y' and CaseTrsQueryVersion.EXCEL_FILE = 'Y' ) or  ( TransactionFlag = 'Y' and CaseTrsQueryVersion.PDF_FILE = 'Y' ))
                                  and CONVERT(VARCHAR(10),CaseTrsQueryVersion.ModifiedDate,111) = CONVERT(VARCHAR(10),getdate(),111) 
                                  and CaseMaster.Status is not Null   
                                  and CaseTrsQueryVersion.CaseTrsNewID  in (
                                    select 
                                      CaseMaster.CaseID
                                    from
                                      CaseMaster
                                    left join
                                      CaseTrsQueryVersion
                                    on CaseMaster.CaseID = CaseTrsQueryVersion.CaseTrsNewID
                                    where
                                      CaseMaster.Status is not Null                           
                                      and  (( OpenFlag = 'Y' and CaseTrsQueryVersion.EXCEL_FILE = 'Y' ) or  ( TransactionFlag = 'Y' and CaseTrsQueryVersion.PDF_FILE = 'Y' ))                                      
                                    group by
                                      CaseMaster.CaseID
                                  );
                              ";

            // 清空容器
            base.Parameter.Clear();

            return base.Search(sqlSelect);
        }

        public DataTable GetPDFzip()
        {
            string sqlSelect = @"
           select
                                 distinct CaseTrsNewID,CaseMaster.CaseNo as DocNo  -- ,CaseTrsQueryVersion.NewID
                                from
                                  CaseMaster
                                left join
                                  CaseTrsQueryVersion
                                on CaseMaster.CaseID = CaseTrsQueryVersion.CaseTrsNewID
                                where
                                  (( OpenFlag = 'Y' and CaseTrsQueryVersion.EXCEL_FILE = 'Y' ) or  ( TransactionFlag = 'Y' and CaseTrsQueryVersion.PDF_FILE = 'Y' ))
                                  and CONVERT(VARCHAR(10),CaseTrsQueryVersion.ModifiedDate,111) = CONVERT(VARCHAR(10),getdate(),111) 
                                  and CaseMaster.Status is not Null   
                                  and CaseTrsQueryVersion.CaseTrsNewID  in (
                                    select 
                                      CaseMaster.CaseID
                                    from
                                      CaseMaster
                                    left join
                                      CaseTrsQueryVersion
                                    on CaseMaster.CaseID = CaseTrsQueryVersion.CaseTrsNewID
                                    where
                                      CaseMaster.Status is not Null                           
                                      and  (( OpenFlag = 'Y' and CaseTrsQueryVersion.EXCEL_FILE = 'Y' ) or  ( TransactionFlag = 'Y' and CaseTrsQueryVersion.PDF_FILE = 'Y' ))                                      
                                    group by
                                      CaseMaster.CaseID
                                  );
                              ";

            // 清空容器
            base.Parameter.Clear();

            return base.Search(sqlSelect);
        }

        public int UpdateHTGDataStatus()
        {
            string sql = @"Update  CaseTrsQueryVersion set Status = '66'
                            where NewID in ( select                          
                                  CaseTrsQueryVersion.NewID
                                  from
                                  CaseMaster
                                left join
                                  CaseTrsQueryVersion
                                on CaseMaster.CaseID = CaseTrsQueryVersion.CaseTrsNewID
                                where
                                  OpenFlag = 'Y'
                                  and CaseTrsQueryVersion.HTGSendStatus  in ('4','2','6')   
                                  and CaseTrsQueryVersion.Status not in ('03','04')
                                  and CaseMaster.Status is not Null )
                                     ";
            // 清空容器
            base.Parameter.Clear();

            return base.ExecuteNonQuery(sql);
        }
        /// <summary>
        /// 查詢產生回文檔案的資料
        /// </summary>
        /// <returns></returns>
        public DataTable GetHTGDataMulti()
        {
            string sqlSelect = @"
                                 select
                                  CaseTrsQueryVersion.CustId
                                  ,CaseTrsQueryVersion.CustAccount
                                  ,CaseTrsQueryVersion.NewID as CaseTrsNewID
                                  ,CONVERT(varchar(100),CaseTrsQueryVersion.HTGModifiedDate, 112) as HTGModifiedDate
                                    --,CaseMaster.ROpenFileName  
                                  ,CaseMaster.CaseNo as DocNo
                                from
                                  CaseMaster
                                left join
                                  CaseTrsQueryVersion
                                on CaseMaster.CaseID = CaseTrsQueryVersion.CaseTrsNewID
                                where
                                  OpenFlag = 'Y'
                                  and CaseTrsQueryVersion.HTGSendStatus  in ('4','2','6')   
                                  and CaseTrsQueryVersion.Status = '66'
                                  and CaseMaster.Status is not Null 
                                  and CaseTrsQueryVersion.CaseTrsNewID  in (
                                    select 
                                      CaseMaster.CaseID
                                    from
                                      CaseMaster
                                    left join
                                      CaseTrsQueryVersion
                                    on CaseMaster.CaseID = CaseTrsQueryVersion.CaseTrsNewID
                                    where
                                      CaseMaster.Status is not Null  and   CaseTrsQueryVersion.Status = '66'
                                      and OpenFlag = 'Y'
                                      and CaseTrsQueryVersion.HTGSendStatus  in ('4','2','6')   
                                    group by
                                      CaseMaster.CaseID
                                  )
                                order by
                                  CaseMaster.DocNo
                                  ,CaseTrsQueryVersion.CustId;

                              ";

            // 清空容器
            base.Parameter.Clear();

            return base.Search(sqlSelect);
        }
        public DataTable GetHTGData()
        {
            string sqlSelect = @"
                                 select
                                  CaseTrsQueryVersion.CustId
                                  ,CaseTrsQueryVersion.CustAccount
                                  ,CaseTrsQueryVersion.NewID as CaseTrsNewID
                                  ,CONVERT(varchar(100),CaseTrsQueryVersion.HTGModifiedDate, 112) as HTGModifiedDate
                                    --,CaseMaster.ROpenFileName  
                                  ,CaseMaster.CaseNo as DocNo
                                from
                                  CaseMaster
                                left join
                                  CaseTrsQueryVersion
                                on CaseMaster.CaseID = CaseTrsQueryVersion.CaseTrsNewID
                                where
                                  OpenFlag = 'Y'
                                  and CaseTrsQueryVersion.HTGSendStatus  in ('4','2','6')   
                                  and CaseTrsQueryVersion.Status not in ('03','04')
                                  and CaseMaster.Status is not Null 
                                  and CaseTrsQueryVersion.CaseTrsNewID  in (
                                    select 
                                      CaseMaster.CaseID
                                    from
                                      CaseMaster
                                    left join
                                      CaseTrsQueryVersion
                                    on CaseMaster.CaseID = CaseTrsQueryVersion.CaseTrsNewID
                                    where
                                      CaseMaster.Status is not Null  and   CaseTrsQueryVersion.Status not in ('03','04')
                                      and OpenFlag = 'Y'
                                      and CaseTrsQueryVersion.HTGSendStatus  in ('4','2','6')   
                                    group by
                                      CaseMaster.CaseID
                                  )
                                order by
                                  CaseMaster.DocNo
                                  ,CaseTrsQueryVersion.CustId;

                              ";

            // 清空容器
            base.Parameter.Clear();

            return base.Search(sqlSelect);
        }

        /// <summary>
        /// 查詢HTG回文txt資料
        /// </summary>
        /// <param name="VersionNewID"></param>
        /// <returns></returns>
        public DataTable Get401RecvData(string VersionNewID)
        {
  string sqlSelect = @"SELECT 
                                	BOPS000401Recv.CUST_ID_NO   -- 統一編號
                                	,CASE 
                                        WHEN ISNULL(BOPS000401Recv.BRANCH_NO,'') <> '' THEN '822' + BOPS000401Recv.BRANCH_NO 
                                        ELSE ''
                                    END BRANCH_NO --分行別
                                	,isnull((select top 1 PARMCode.CodeNo
				                        from PARMCode
				                        where PARMCode.CodeType = 'PD_TYPE_DESC'
				                        and PARMCode.CodeMemo = BOPS000401Recv.PD_TYPE_DESC
										      order by PARMCode.CodeNo
			                        ),'99') as  PD_TYPE_DESC--產品別
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
                                FROM BOPS067050Recv
								left join BOPS000401Recv
								     on BOPS067050Recv.VersionNewID = BOPS000401Recv.VersionNewID and BOPS000401Recv.ACCT_NO = BOPS067050Recv.CIF_NO
								LEFT join BOPS067050V4Recv 
								    on BOPS067050Recv.VersionNewID = BOPS067050V4Recv.VersionNewID and BOPS067050V4Recv.CUST_ID_NO=BOPS067050V4Recv.CUST_ID_NO
								LEFT join BOPS067050V6Recv  
								    on BOPS067050Recv.VersionNewID = BOPS067050V6Recv.VersionNewID
                                LEFT JOIN CaseTrsQueryVersion
                                 ON CaseTrsQueryVersion.NewId = BOPS067050Recv.VersionNewID
                                WHERE BOPS067050Recv.VersionNewID = @VersionNewID
                                order by CaseTrsQueryVersion.CustId,CaseTrsQueryVersion.CustAccount,BOPS067050Recv.CUST_ID_NO,OPEN_DATE";
            // 清空容器
            base.Parameter.Clear();
            base.Parameter.Add(new CommandParameter("@VersionNewID", VersionNewID));

            return base.Search(sqlSelect);
        }


        public int UpdateExcelHTGSendStatus(string VersionNewID)
        {
            string sql = @"
                         Update CaseTrsQueryVersion 
                         set HTGSendStatus = '88' , ModifiedDate=getdate()  ,EXCEL_FILE = 'Y' 
                         where NewID = @NewID ";

            // 清空容器
            base.Parameter.Clear();
            base.Parameter.Add(new CommandParameter("@NewID", VersionNewID));

            return base.ExecuteNonQuery(sql);
        }/// 
        /// <summary>
        /// 更新CaseTrsQueryVersion表HTGSendStatus
        /// </summary>
        /// <param name="VersionNewID"></param>
//        public int UpdatePDFHTGSendStatus(string VersionNewID)
//        {
//            string sql = @"
//                         Update CaseTrsQueryVersion 
//                         set HTGSendStatus = '88' , ModifiedDate=getdate()  ,EXCEL_FILE = 'Y' 
//                         where NewID = @NewID ";

//            // 清空容器
//            base.Parameter.Clear();
//            base.Parameter.Add(new CommandParameter("@NewID", VersionNewID));

//            return base.ExecuteNonQuery(sql);
//        }/// 

//        public int UpdateHTGSendStatus(string VersionNewID)
//        {
//            string sql = @"
//                         Update CaseTrsQueryVersion 
//                         set HTGSendStatus = '99'  ,ModifiedDate=getdate()
//                         where NewID = @NewID ";

//            // 清空容器
//            base.Parameter.Clear();
//            base.Parameter.Add(new CommandParameter("@NewID", VersionNewID));

//            return base.ExecuteNonQuery(sql);
//        }
        #endregion

        #region RFDM

        public void ExportRFDMPdf()
        {
            m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------產生歷史交易PDF（存款往來明細資料）開始------");
            // 查詢產生檔案的資料
            //DataTable dtResult = GetRFDMData();
            //改成多工處理
            DataTable dtResult = GetRFDMDataMulti();

            string DocNo = string.Empty;
            string txtDocDirPath = "";
            string txtDocIDPath = "";
            if (dtResult != null && dtResult.Rows.Count > 0)
            {
                // 遍歷去除重複的資料
                
                // Patrick , 20210616, =================Start 
                // 改為用MultiThread的方式來產生PDF檔..
                // 預設ThreadPool 最高用10個Thread即可. 以免吃太多記憶體.

                ThreadPool.SetMaxThreads(intTask, 1000);

                List<Task> TaskList = new List<Task>();

                foreach (DataRow dr in dtResult.Rows)
                {
                    try
					{
      //                  if( dr["DocNo"].ToString()== "00000200131006061")
						//{
      //                      string aaa = dr["DocNo"].ToString();
						//}
                        m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------證號:" + dr["CustId"].ToString() +"-"+ dr["CustAccount"].ToString());
                        List<string> obj = new List<string>() { dr[2].ToString(),  dr[5].ToString(), dr[0].ToString() };

                       Task pdfTask = Task.Run(() => GenerateDetailsPDF(obj));
                        TaskList.Add(pdfTask);
                        Thread.Sleep(3000);

                    }
					catch (Exception ex)
                    {
                        m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------案號:" + dr["DocNo"].ToString() + "產檔錯誤:" + ex.ToString());
                        int RFDMInt = UpdateNoPDFFILEMessage(dr["CaseTrsNewID"].ToString(), ex.ToString());
                        //throw;
                    }
                }

                Task.WaitAll(TaskList.ToArray());

                // Patrick , 20210616, ================= END
            }
            else
            {
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------沒有PDF（存款往來明細資料）");
            }
            m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------產出歷史交易PDF（存款往來明細資料）結束------");
  
        }


        void GenerateDetailsPDF(object obj)
		{           
            var lstString = (List<string>)obj;
            string DocNo = lstString[1];
            int threadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
            m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "Task--------PDF thread Id:" + threadId.ToString());
            m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "Task--------執行PDF----:" + DocNo);
            string CaseTrsNewID = lstString[0];
            string CustId = lstString[2];


            if (!Directory.Exists(pdfFilePath + "\\" + DocNo)) Directory.CreateDirectory(pdfFilePath + "\\" + DocNo);

			string txtDocDirPath = pdfFilePath + "\\" + DocNo;
            string txtDocIDPath = string.Empty;


            // 根據主檔主鍵獲取該案件下身分證統一編號資料
            DataTable drRecvAccount = GetRFDMRecvAccount(CaseTrsNewID);
			DataTable drRecvData = GetRFDMRecvData(CaseTrsNewID);
            //adam 20211210 提高效能
                string sqlread = "";

            for (int p = 0; p < drRecvData.Rows.Count; p++)
            {

                sqlread = @"select top 1 *
                                        from CaseCustATMRecv
                                       where CaseCustATMRecv.[YBTXLOG_YYYYMMDD] = '" + drRecvData.Rows[p]["TRAN_DATE"].ToString() + "' and  ('0'+CaseCustATMRecv.[YBTXLOG_STAND_NO]) = '" + drRecvData.Rows[p]["FISC_SEQNO"].ToString() + "' order by CaseCustATMRecv.CreatedDate DESC";
                DataTable DTtemp = base.Search(sqlread);
                DTtemp = base.Search(sqlread);

                // 新增會員編號
                //if (DTtemp.Rows.Count > 0)
                //{
                //    if (DTtemp.Rows[0]["Member_No"] != null)
                //    {
                //        drRecvData.Rows[p]["Member_No"] = DTtemp.Rows[0]["Member_No"].ToString();
                //    }
                //}
                // JNRST_TIME                
                if (drRecvData.Rows[p]["JNRST_TIME"].ToString().Length == 0)
                {
                    if (DTtemp.Rows.Count > 0)
                    {
                        if (String.IsNullOrEmpty(DTtemp.Rows[0]["YBTXLOG_TXN_HHMMSS"].ToString()))
                        {
                            drRecvData.Rows[p]["JNRST_TIME"] = DTtemp.Rows[0]["YBTXLOG_TXN_HHMMSS"].ToString();
                        }
                        else
                        {
                            drRecvData.Rows[p]["JNRST_TIME"] = DTtemp.Rows[0]["YBTXLOG_TXN_HHMMSS"].ToString().Substring(0, 2) + ":" + DTtemp.Rows[0]["YBTXLOG_TXN_HHMMSS"].ToString().Substring(2, 2) + ":" + DTtemp.Rows[0]["YBTXLOG_TXN_HHMMSS"].ToString().Substring(4, 2);
                        }
                    }
                }
                else
                {
                    if (drRecvData.Rows[p]["JNRST_TIME"].ToString() == drRecvData.Rows[p]["JRNL_NO"].ToString() || drRecvData.Rows[p]["JNRST_TIME"].ToString() == drRecvData.Rows[p]["ATM_TIME"].ToString())
                    {
                        if (DTtemp.Rows.Count > 0)
                        {
                            if (String.IsNullOrEmpty(DTtemp.Rows[0]["YBTXLOG_TXN_HHMMSS"].ToString()))
                            {
                                drRecvData.Rows[p]["JNRST_TIME"] = DTtemp.Rows[0]["YBTXLOG_TXN_HHMMSS"].ToString();
                            }
                            else
                            {
                                drRecvData.Rows[p]["JNRST_TIME"] = DTtemp.Rows[0]["YBTXLOG_TXN_HHMMSS"].ToString().Substring(0, 2) + ":" + DTtemp.Rows[0]["YBTXLOG_TXN_HHMMSS"].ToString().Substring(2, 2) + ":" + DTtemp.Rows[0]["YBTXLOG_TXN_HHMMSS"].ToString().Substring(4, 2);
                            }
                        }
                        else
                        {
                            if (drRecvData.Rows[p]["JNRST_TIME"].ToString().Length > 5) drRecvData.Rows[p]["JNRST_TIME"] = drRecvData.Rows[p]["JNRST_TIME"].ToString().Substring(0, 2) + ":" + drRecvData.Rows[p]["JNRST_TIME"].ToString().Substring(2, 2) + ":" + drRecvData.Rows[p]["JNRST_TIME"].ToString().Substring(4, 2);
                        }
                    }
                    else
                    {
                        if (drRecvData.Rows[p]["JNRST_TIME"].ToString().Length > 5) drRecvData.Rows[p]["JNRST_TIME"] = drRecvData.Rows[p]["JNRST_TIME"].ToString().Substring(0, 2) + ":" + drRecvData.Rows[p]["JNRST_TIME"].ToString().Substring(2, 2) + ":" + drRecvData.Rows[p]["JNRST_TIME"].ToString().Substring(4, 2);
                    }
                }

                // ATM_TIME
                if (DTtemp.Rows.Count > 0)
                {
                    if (String.IsNullOrEmpty(DTtemp.Rows[0]["YBTXLOG_TXN_HHMMSS"].ToString()))
                    {
                        drRecvData.Rows[p]["ATM_TIME"] = DTtemp.Rows[0]["YBTXLOG_TXN_HHMMSS"].ToString();
                    }
                    else
                    {
                        drRecvData.Rows[p]["ATM_TIME"] = DTtemp.Rows[0]["YBTXLOG_TXN_HHMMSS"].ToString().Substring(0, 2) + ":" + DTtemp.Rows[0]["YBTXLOG_TXN_HHMMSS"].ToString().Substring(2, 2) + ":" + DTtemp.Rows[0]["YBTXLOG_TXN_HHMMSS"].ToString().Substring(4, 2);
                    }
                }
                else
                {
                    drRecvData.Rows[p]["ATM_TIME"] = "";
                }
                //

                // YBTXLOG_SRC_ID --設備代理行
                // YBTXLOG_STAND_NO --交易序號
                // YBTXLOG_SAFE_TMNL_ID --機器編號
                //  YBTXLOG_IC_MEMO_CARDNO --卡號
                //  ADD_NARRATIVE --附言
                //  會員編號 --附言
                if (DTtemp.Rows.Count > 0) drRecvData.Rows[p]["YBTXLOG_SRC_ID"] = DTtemp.Rows[0]["YBTXLOG_SRC_ID"].ToString();
                if (DTtemp.Rows.Count > 0) drRecvData.Rows[p]["YBTXLOG_STAND_NO"] = DTtemp.Rows[0]["YBTXLOG_STAND_NO"].ToString();
                if (DTtemp.Rows.Count > 0) drRecvData.Rows[p]["YBTXLOG_SAFE_TMNL_ID"] = DTtemp.Rows[0]["YBTXLOG_SAFE_TMNL_ID"].ToString();
                if (DTtemp.Rows.Count > 0) drRecvData.Rows[p]["YBTXLOG_IC_MEMO_CARDNO"] = DTtemp.Rows[0]["YBTXLOG_IC_MEMO_CARDNO"].ToString();
                if (DTtemp.Rows.Count > 0) drRecvData.Rows[p]["ADD_NARRATIVE"] = DTtemp.Rows[0]["NARRATIVE"].ToString();
                if (DTtemp.Rows.Count > 0) drRecvData.Rows[p]["Member_No"] = DTtemp.Rows[0]["Member_No"].ToString();
                // 
                if (drRecvData.Rows[p]["PD_TYPE_DESC"].ToString() != "99" && drRecvData.Rows[p]["PD_TYPE_DESC"].ToString().Length > 0)
                {
                    sqlread = @"select top 1 PARMCode.CodeNo
                                               from PARMCode
                                               where PARMCode.CodeType = 'PD_TYPE_DESC'
                                               and PARMCode.CodeMemo = '" + drRecvData.Rows[p]["PD_TYPE_DESC"] + "' order by PARMCode.CodeNo";
                    DataTable DTpara = base.Search(sqlread);
                    if (DTpara.Rows.Count > 0) drRecvData.Rows[p]["PD_TYPE_DESC"] = DTpara.Rows[0]["CodeNo"].ToString(); else drRecvData.Rows[p]["PD_TYPE_DESC"] = "99";
                }

            }

            //adam 20211210 end 

            // RFDM內容及對應資料筆數變量
            //string fileContent2 = "";
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

			if (drRecvData != null && drRecvData.Rows.Count > 0)
			{
				for (int a = 0; a < drRecvAccount.Rows.Count; a++)
				{
					//Acct_DateFm_DateTo = drRecvData.Rows[a]["GroupId"].ToString();
					Acct_DateFm_DateTo = drRecvAccount.Rows[a]["ACCT_NO"].ToString() + "-" + drRecvAccount.Rows[a]["QDateS"].ToString() + "-" + drRecvAccount.Rows[a]["QDateE"].ToString();
					DataTable drAccountData = GetRFDMRecvData(CaseTrsNewID, drRecvAccount.Rows[a]["ACCT_NO"].ToString() + drRecvAccount.Rows[a]["QDateS"].ToString() + drRecvAccount.Rows[a]["QDateE"].ToString());
                    //adam 20220818 提高效能

                    for (int p = 0; p < drAccountData.Rows.Count; p++)
                    {

                        sqlread = @"select top 1 *
                                        from CaseCustATMRecv
                                       where CaseCustATMRecv.[YBTXLOG_YYYYMMDD] = '" + drAccountData.Rows[p]["TRAN_DATE"].ToString() + "' and  ('0'+CaseCustATMRecv.[YBTXLOG_STAND_NO]) = '" + drAccountData.Rows[p]["FISC_SEQNO"].ToString() + "' order by CaseCustATMRecv.CreatedDate DESC";
                        DataTable DTtemp = base.Search(sqlread);
                        DTtemp = base.Search(sqlread);

                        // JNRST_TIME
                        if (drAccountData.Rows[p]["JNRST_TIME"].ToString().Length == 0)
                        {
                            if (DTtemp.Rows.Count > 0)
                            {
                                if (String.IsNullOrEmpty(DTtemp.Rows[0]["YBTXLOG_TXN_HHMMSS"].ToString()))
                                {
                                    drAccountData.Rows[p]["JNRST_TIME"] = DTtemp.Rows[0]["YBTXLOG_TXN_HHMMSS"].ToString();
                                }
                                else
                                {
                                    drAccountData.Rows[p]["JNRST_TIME"] = DTtemp.Rows[0]["YBTXLOG_TXN_HHMMSS"].ToString().Substring(0, 2) + ":" + DTtemp.Rows[0]["YBTXLOG_TXN_HHMMSS"].ToString().Substring(2, 2) + ":" + DTtemp.Rows[0]["YBTXLOG_TXN_HHMMSS"].ToString().Substring(4, 2);
                                }
                            }
                        }
                        else
                        {
                            if (drAccountData.Rows[p]["JNRST_TIME"].ToString() == drAccountData.Rows[p]["JRNL_NO"].ToString() || drAccountData.Rows[p]["JNRST_TIME"].ToString() == drAccountData.Rows[p]["ATM_TIME"].ToString())
                            {
                                if (DTtemp.Rows.Count > 0)
                                {
                                    if (String.IsNullOrEmpty(DTtemp.Rows[0]["YBTXLOG_TXN_HHMMSS"].ToString()))
                                    {
                                        drAccountData.Rows[p]["JNRST_TIME"] = DTtemp.Rows[0]["YBTXLOG_TXN_HHMMSS"].ToString();
                                    }
                                    else
                                    {
                                        drAccountData.Rows[p]["JNRST_TIME"] = DTtemp.Rows[0]["YBTXLOG_TXN_HHMMSS"].ToString().Substring(0, 2) + ":" + DTtemp.Rows[0]["YBTXLOG_TXN_HHMMSS"].ToString().Substring(2, 2) + ":" + DTtemp.Rows[0]["YBTXLOG_TXN_HHMMSS"].ToString().Substring(4, 2);
                                    }
                                }
                                else
                                {
                                    if (drAccountData.Rows[p]["JNRST_TIME"].ToString().Length > 5) drAccountData.Rows[p]["JNRST_TIME"] = drAccountData.Rows[p]["JNRST_TIME"].ToString().Substring(0, 2) + ":" + drAccountData.Rows[p]["JNRST_TIME"].ToString().Substring(2, 2) + ":" + drAccountData.Rows[p]["JNRST_TIME"].ToString().Substring(4, 2);
                                }
                            }
                            else
                            {
                                if (drAccountData.Rows[p]["JNRST_TIME"].ToString().Length > 5) drAccountData.Rows[p]["JNRST_TIME"] = drAccountData.Rows[p]["JNRST_TIME"].ToString().Substring(0, 2) + ":" + drAccountData.Rows[p]["JNRST_TIME"].ToString().Substring(2, 2) + ":" + drAccountData.Rows[p]["JNRST_TIME"].ToString().Substring(4, 2);
                            }
                        }

                        // ATM_TIME
                        if (DTtemp.Rows.Count > 0)
                        {
                            if (String.IsNullOrEmpty(DTtemp.Rows[0]["YBTXLOG_TXN_HHMMSS"].ToString()))
                            {
                                drAccountData.Rows[p]["ATM_TIME"] = DTtemp.Rows[0]["YBTXLOG_TXN_HHMMSS"].ToString();
                            }
                            else
                            {
                                drAccountData.Rows[p]["ATM_TIME"] = DTtemp.Rows[0]["YBTXLOG_TXN_HHMMSS"].ToString().Substring(0, 2) + ":" + DTtemp.Rows[0]["YBTXLOG_TXN_HHMMSS"].ToString().Substring(2, 2) + ":" + DTtemp.Rows[0]["YBTXLOG_TXN_HHMMSS"].ToString().Substring(4, 2);
                            }
                        }
                        else
                        {
                            drAccountData.Rows[p]["ATM_TIME"] = "";
                        }
                        //

                        // YBTXLOG_SRC_ID --設備代理行
                        // YBTXLOG_STAND_NO --交易序號
                        // YBTXLOG_SAFE_TMNL_ID --機器編號
                        //  YBTXLOG_IC_MEMO_CARDNO --卡號
                        //  ADD_NARRATIVE --附言
                        // Member_No -- 會員編號
                        if (DTtemp.Rows.Count > 0) drAccountData.Rows[p]["YBTXLOG_SRC_ID"] = DTtemp.Rows[0]["YBTXLOG_SRC_ID"].ToString();
                        if (DTtemp.Rows.Count > 0) drAccountData.Rows[p]["YBTXLOG_STAND_NO"] = DTtemp.Rows[0]["YBTXLOG_STAND_NO"].ToString();
                        if (DTtemp.Rows.Count > 0) drAccountData.Rows[p]["YBTXLOG_SAFE_TMNL_ID"] = DTtemp.Rows[0]["YBTXLOG_SAFE_TMNL_ID"].ToString();
                        if (DTtemp.Rows.Count > 0) drAccountData.Rows[p]["YBTXLOG_IC_MEMO_CARDNO"] = DTtemp.Rows[0]["YBTXLOG_IC_MEMO_CARDNO"].ToString();
                        if (DTtemp.Rows.Count > 0) drAccountData.Rows[p]["Member_No"] = DTtemp.Rows[0]["Member_No"].ToString();
                        if (DTtemp.Rows.Count > 0) drAccountData.Rows[p]["ADD_NARRATIVE"] = DTtemp.Rows[0]["NARRATIVE"].ToString();
                        // 
                        if (drAccountData.Rows[p]["PD_TYPE_DESC"].ToString() != "99" && drAccountData.Rows[p]["PD_TYPE_DESC"].ToString().Length > 0)
                        {
                            sqlread = @"select top 1 PARMCode.CodeNo
                                               from PARMCode
                                               where PARMCode.CodeType = 'PD_TYPE_DESC'
                                               and PARMCode.CodeMemo = '" + drAccountData.Rows[p]["PD_TYPE_DESC"] + "' order by PARMCode.CodeNo";
                            DataTable DTpara = base.Search(sqlread);
                            if (DTpara.Rows.Count > 0) drAccountData.Rows[p]["PD_TYPE_DESC"] = DTpara.Rows[0]["CodeNo"].ToString(); else drAccountData.Rows[p]["PD_TYPE_DESC"] = "99";
                        }

                    }

                    //adam 20220818 end 
                    // 遍歷並追加txt文件
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
						strRow["CUSTOMER_NAME"] = ChangeChiness(drAccountData.Rows[j]["CUSTOMER_NAME"].ToString(), 60); //客戶名稱
						strRow["PrintDate"] = ChangeChiness(System.DateTime.Now.ToShortDateString(), 10); //列印日期
						strRow["QDateS"] = ChangeChiness(drAccountData.Rows[j]["QDateS"].ToString(), 10); //查詢起
						strRow["QDateE"] = ChangeChiness(drAccountData.Rows[j]["QDateE"].ToString(), 10); //查詢迄
						strRow["CreatedUser"] = ChangeValue(drAccountData.Rows[j]["CreatedUser"].ToString(), 20);
						dt.Rows.Add(strRow);
						#endregion
						// 換行
						//fileContent2 += "\r\n";
					}

					// 只能在 讀後強迫改寫檔名 2020/06/29;但如果多帳號就有風險
					if (drRecvAccount.Rows[a]["QDateS"].ToString().Length > 4)
					{
						if (drRecvAccount.Rows[a]["QDateS"].ToString().Substring(0, 4) == "1911")
						{
							Acct_DateFm_DateTo = drRecvAccount.Rows[a]["ACCT_NO"].ToString().Trim() + "-" + dt.Rows[0]["QDateS"].ToString().Trim() + "-" + dt.Rows[0]["QDateE"].ToString().Trim();
						}
					}
					CreateRFDMPdf(Acct_DateFm_DateTo, dt, txtDocIDPath);

					dt.Clear();
					// 記錄LOG
					m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------帳戶:" + Acct_DateFm_DateTo + "文件中增加" + drAccountData.Rows.Count + "筆資料!");
				}
			}
			else
			{
				dt.Clear();
				// 記錄LOG
				m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------帳戶:" + DocNo + CaseTrsNewID + "無交易明細資料");
			}


			// 更新EXCEL FILE Status狀態
			if (drRecvData.Rows.Count > 0)
			{
				int RFDMInt = UpdatePDFFILEStatus(CaseTrsNewID);
				if (RFDMInt > 0)
				{
					// 記錄LOG
					m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息-------- ID: " + CustId +" PDF_FILE  交易產檔完成  ");
				}
			}
			else
			{
				//int RFDMInt = UpdateNoPDFFILEStatus(CaseTrsNewID);
				m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息-------- ID: " + CustId + " PDF_FILE  無交易資料產檔 ");
			}
		}


		public void ExportRFDMExcel()
        {
            m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------產生EXCEL歷史帳戶交易資料（存款往來明細資料）開始------");
            // 查詢產生檔案的資料
            //DataTable dtResult = GetRFDMData();
            // 改成多工處理
            DataTable dtResult = GetRFDMDataMulti();
            string DocNo = string.Empty;
            string txtDocDirPath = "";
            string txtDocIDPath = "";
            if (dtResult != null && dtResult.Rows.Count > 0)
            {
                // Patrick , 20210616, =================Start 
                // 改為用MultiThread的方式來產生PDF檔..
                // 預設ThreadPool 最高用10個Thread即可. 以免吃太多記憶體.

                ThreadPool.SetMaxThreads(intTask, 1000);

                List<Task> TaskList = new List<Task>();

                // 遍歷去除重複的資料
                //for (int i = 0; i < dtResult.Rows.Count; i++)
                foreach (DataRow dr in dtResult.Rows)
                {
                    try
					{

						//List<string> obj = new List<string>() { dr[2].ToString(), dr[5].ToString(), dr[0].ToString() };
						//GenerateDetailExcel(obj);

						m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------證號:" + dr["CustId"].ToString() + "-" + dr["CustAccount"].ToString());
						List<string> obj = new List<string>() { dr[2].ToString(), dr[5].ToString(), dr[0].ToString() };

						Task pdfTask = Task.Run(  ( ) => GenerateDetailExcel(obj)   ) ;
						TaskList.Add(pdfTask);
						Thread.Sleep(3000);

					}
					catch (Exception ex)
                    {
                        m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------案號:" + dr["DocNo"].ToString() + "產檔錯誤:" + ex.ToString());
                        int RFDMInt = UpdateNoEXCELFILEMessage(dr["CaseTrsNewID"].ToString(),ex.ToString());
                        //throw;
                    }
                }

                Task.WaitAll(TaskList.ToArray());

                // Patrick , 20210616, ================= END


            }
            else
            {
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------沒有查詢名稱（存款往來明細資料）");
            }

            m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------EXCEL歷史交易檔名稱（存款往來明細資料）結束------");
        }

		private void GenerateDetailExcel(object obj)
		{

            var lstString = (List<string>)obj;
            string DocNo = lstString[1];
            int threadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
            m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "Task--------EXCLE thread Id:" + threadId.ToString());
            m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "Task--------執行Excel----:" + DocNo);
            string CaseTrsNewID = lstString[0];
            string CustId = lstString[2];


            if (!Directory.Exists(pdfFilePath + "\\" + DocNo)) Directory.CreateDirectory(pdfFilePath + "\\" + DocNo);

            string txtDocDirPath = pdfFilePath + "\\" + DocNo;
            string txtDocIDPath = string.Empty;




            //if (DocNo != dr["DocNo"].ToString())
			{
				//string Filename = txtFilePath + "\\" + dr["RFileTransactionFileName"].ToString();

				//if (File.Exists(Filename))
				//{
				//    File.Delete(Filename);
				//}

				//DocNo = dr["DocNo"].ToString();

				if (!Directory.Exists(txtFilePath + "\\" + DocNo)) Directory.CreateDirectory(txtFilePath + "\\" + DocNo);
				txtDocDirPath = txtFilePath + "\\" + DocNo;
				//string Filename = txtFilePath + "\\" + dtHTGResult.Rows[p]["ROpenFileName"].ToString();
				// //查詢 CaseEdocFile 是否已存在,存在清空,暫時不處理
				//if (File.Exists(Filename))
				//{
				//    File.Delete(Filename);
				//}

				//DocNo = DocNo;
			}

			// 根據主檔主鍵獲取該案件下身分證統一編號資料
			DataTable drRecvAccount = GetRFDMRecvAccount(CaseTrsNewID);
			// ID 所有帳戶全部交易非單一帳戶
			DataTable drRecvData = GetRFDMRecvData(CaseTrsNewID);

            //adam 20211210 提高效能
            string sqlread = "";

            for (int p = 0; p < drRecvData.Rows.Count; p++)
            {

                sqlread = @"select top 1 *
                                        from CaseCustATMRecv
                                       where CaseCustATMRecv.[YBTXLOG_YYYYMMDD] = '" + drRecvData.Rows[p]["TRAN_DATE"].ToString() + "' and  ('0'+CaseCustATMRecv.[YBTXLOG_STAND_NO]) = '"+ drRecvData.Rows[p]["FISC_SEQNO"].ToString() + "' order by CaseCustATMRecv.CreatedDate DESC";
                DataTable DTtemp = base.Search(sqlread);
                DTtemp = base.Search(sqlread);
                // 新增會員編號
                //if (DTtemp.Rows.Count > 0)
                //{
                //    if (DTtemp.Rows[0]["Member_No"] != null)
                //    {
                //        drRecvData.Rows[p]["Member_No"] = DTtemp.Rows[0]["Member_No"].ToString();
                //    }
                //}
                // JNRST_TIME
                if (drRecvData.Rows[p]["JNRST_TIME"].ToString().Length == 0)
                {
                    if (DTtemp.Rows.Count > 0)
                    {
                        if (String.IsNullOrEmpty(DTtemp.Rows[0]["YBTXLOG_TXN_HHMMSS"].ToString()))
                        {
                            drRecvData.Rows[p]["JNRST_TIME"] = DTtemp.Rows[0]["YBTXLOG_TXN_HHMMSS"].ToString();
                        }
                        else
                        {
                            drRecvData.Rows[p]["JNRST_TIME"] = DTtemp.Rows[0]["YBTXLOG_TXN_HHMMSS"].ToString().Substring(0, 2) + ":" + DTtemp.Rows[0]["YBTXLOG_TXN_HHMMSS"].ToString().Substring(2, 2) + ":" + DTtemp.Rows[0]["YBTXLOG_TXN_HHMMSS"].ToString().Substring(4, 2);
                        }
                    }
                }
                else
                {
                    if (drRecvData.Rows[p]["JNRST_TIME"].ToString() == drRecvData.Rows[p]["JRNL_NO"].ToString() || drRecvData.Rows[p]["JNRST_TIME"].ToString() == drRecvData.Rows[p]["ATM_TIME"].ToString())
                    {
                        if (DTtemp.Rows.Count > 0)
                        {
                            if (String.IsNullOrEmpty(DTtemp.Rows[0]["YBTXLOG_TXN_HHMMSS"].ToString()))
                            {
                                drRecvData.Rows[p]["JNRST_TIME"] = DTtemp.Rows[0]["YBTXLOG_TXN_HHMMSS"].ToString();
                            }
                            else
                            {
                                drRecvData.Rows[p]["JNRST_TIME"] = DTtemp.Rows[0]["YBTXLOG_TXN_HHMMSS"].ToString().Substring(0, 2) + ":" + DTtemp.Rows[0]["YBTXLOG_TXN_HHMMSS"].ToString().Substring(2, 2) + ":" + DTtemp.Rows[0]["YBTXLOG_TXN_HHMMSS"].ToString().Substring(4, 2);
                            }
                        }
                        else
                        {
                            if (drRecvData.Rows[p]["JNRST_TIME"].ToString().Length > 5) drRecvData.Rows[p]["JNRST_TIME"] = drRecvData.Rows[p]["JNRST_TIME"].ToString().Substring(0, 2) + ":" + drRecvData.Rows[p]["JNRST_TIME"].ToString().Substring(2, 2) + ":" + drRecvData.Rows[p]["JNRST_TIME"].ToString().Substring(4, 2);
                        }
                    }
                    else
                    {
                        if (drRecvData.Rows[p]["JNRST_TIME"].ToString().Length > 5) drRecvData.Rows[p]["JNRST_TIME"] = drRecvData.Rows[p]["JNRST_TIME"].ToString().Substring(0, 2) + ":" + drRecvData.Rows[p]["JNRST_TIME"].ToString().Substring(2, 2) + ":" + drRecvData.Rows[p]["JNRST_TIME"].ToString().Substring(4, 2);
                    }
                }

                // ATM_TIME
                if (DTtemp.Rows.Count > 0)
                {
                    if (String.IsNullOrEmpty(DTtemp.Rows[0]["YBTXLOG_TXN_HHMMSS"].ToString()))
                    {
                        drRecvData.Rows[p]["ATM_TIME"] = DTtemp.Rows[0]["YBTXLOG_TXN_HHMMSS"].ToString();
                    }
                    else
                    {
                        drRecvData.Rows[p]["ATM_TIME"] = DTtemp.Rows[0]["YBTXLOG_TXN_HHMMSS"].ToString().Substring(0, 2) + ":" + DTtemp.Rows[0]["YBTXLOG_TXN_HHMMSS"].ToString().Substring(2, 2) + ":" + DTtemp.Rows[0]["YBTXLOG_TXN_HHMMSS"].ToString().Substring(4, 2);
                    }
                }
                else
                {
                    drRecvData.Rows[p]["ATM_TIME"] = "";
                }
                //

                // YBTXLOG_SRC_ID --設備代理行
                // YBTXLOG_STAND_NO --交易序號
                // YBTXLOG_SAFE_TMNL_ID --機器編號
                //  YBTXLOG_IC_MEMO_CARDNO --卡號
                //  ADD_NARRATIVE --附言
                // Member_No -- 會員編號
                if (DTtemp.Rows.Count > 0) drRecvData.Rows[p]["YBTXLOG_SRC_ID"] = DTtemp.Rows[0]["YBTXLOG_SRC_ID"].ToString();
                if (DTtemp.Rows.Count > 0) drRecvData.Rows[p]["YBTXLOG_STAND_NO"] = DTtemp.Rows[0]["YBTXLOG_STAND_NO"].ToString();
                if (DTtemp.Rows.Count > 0) drRecvData.Rows[p]["YBTXLOG_SAFE_TMNL_ID"] = DTtemp.Rows[0]["YBTXLOG_SAFE_TMNL_ID"].ToString();
                if (DTtemp.Rows.Count > 0) drRecvData.Rows[p]["YBTXLOG_IC_MEMO_CARDNO"] = DTtemp.Rows[0]["YBTXLOG_IC_MEMO_CARDNO"].ToString();
                if (DTtemp.Rows.Count > 0) drRecvData.Rows[p]["ADD_NARRATIVE"] = DTtemp.Rows[0]["NARRATIVE"].ToString();
                if (DTtemp.Rows.Count > 0) drRecvData.Rows[p]["Member_No"] = DTtemp.Rows[0]["Member_No"].ToString();
                // 
                if (drRecvData.Rows[p]["PD_TYPE_DESC"].ToString() != "99" && drRecvData.Rows[p]["PD_TYPE_DESC"].ToString().Length > 0)
                {
                    sqlread = @"select top 1 PARMCode.CodeNo
                                               from PARMCode
                                               where PARMCode.CodeType = 'PD_TYPE_DESC'
                                               and PARMCode.CodeMemo = '" + drRecvData.Rows[p]["PD_TYPE_DESC"] + "' order by PARMCode.CodeNo";
                    DataTable DTpara = base.Search(sqlread);
                    if (DTpara.Rows.Count > 0) drRecvData.Rows[p]["PD_TYPE_DESC"] = DTpara.Rows[0]["CodeNo"].ToString(); else drRecvData.Rows[p]["PD_TYPE_DESC"] = "99";
                }

            }

            //adam 20211210 end 

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


			if (drRecvData != null && drRecvData.Rows.Count > 0) //所有帳戶交易
			{
				for (int a = 0; a < drRecvAccount.Rows.Count; a++)
				{
					//Acct_DateFm_DateTo = drRecvData.Rows[a]["GroupId"].ToString();
					Acct_DateFm_DateTo = drRecvAccount.Rows[a]["ACCT_NO"].ToString() + "-" + drRecvAccount.Rows[a]["QDateS"].ToString() + "-" + drRecvAccount.Rows[a]["QDateE"].ToString();
					// 遍歷並追加txt文件
					DataTable drAccountData = GetRFDMRecvData(CaseTrsNewID, drRecvAccount.Rows[a]["ACCT_NO"].ToString() + drRecvAccount.Rows[a]["QDateS"].ToString() + drRecvAccount.Rows[a]["QDateE"].ToString());
                    //adam 20220818 提高效能

                    for (int p = 0; p < drAccountData.Rows.Count; p++)
                    {

                        sqlread = @"select top 1 *
                                        from CaseCustATMRecv
                                       where CaseCustATMRecv.[YBTXLOG_YYYYMMDD] = '" + drAccountData.Rows[p]["TRAN_DATE"].ToString() + "' and  ('0'+CaseCustATMRecv.[YBTXLOG_STAND_NO]) = '" + drAccountData.Rows[p]["FISC_SEQNO"].ToString() + "' order by CaseCustATMRecv.CreatedDate DESC";
                        DataTable DTtemp = base.Search(sqlread);
                        DTtemp = base.Search(sqlread);

                        // JNRST_TIME
                        if (drAccountData.Rows[p]["JNRST_TIME"].ToString().Length == 0)
                        {
                            if (DTtemp.Rows.Count > 0)
                            {
                                if (String.IsNullOrEmpty(DTtemp.Rows[0]["YBTXLOG_TXN_HHMMSS"].ToString()))
                                {
                                    drAccountData.Rows[p]["JNRST_TIME"] = DTtemp.Rows[0]["YBTXLOG_TXN_HHMMSS"].ToString();
                                }
                                else
                                {
                                    drAccountData.Rows[p]["JNRST_TIME"] = DTtemp.Rows[0]["YBTXLOG_TXN_HHMMSS"].ToString().Substring(0, 2) + ":" + DTtemp.Rows[0]["YBTXLOG_TXN_HHMMSS"].ToString().Substring(2, 2) + ":" + DTtemp.Rows[0]["YBTXLOG_TXN_HHMMSS"].ToString().Substring(4, 2);
                                }
                            }
                        }
                        else
                        {
                            if (drAccountData.Rows[p]["JNRST_TIME"].ToString() == drAccountData.Rows[p]["JRNL_NO"].ToString() || drAccountData.Rows[p]["JNRST_TIME"].ToString() == drAccountData.Rows[p]["ATM_TIME"].ToString())
                            {
                                if (DTtemp.Rows.Count > 0)
                                {
                                    if (String.IsNullOrEmpty(DTtemp.Rows[0]["YBTXLOG_TXN_HHMMSS"].ToString()))
                                    {
                                        drAccountData.Rows[p]["JNRST_TIME"] = DTtemp.Rows[0]["YBTXLOG_TXN_HHMMSS"].ToString();
                                    }
                                    else
                                    {
                                        drAccountData.Rows[p]["JNRST_TIME"] = DTtemp.Rows[0]["YBTXLOG_TXN_HHMMSS"].ToString().Substring(0, 2) + ":" + DTtemp.Rows[0]["YBTXLOG_TXN_HHMMSS"].ToString().Substring(2, 2) + ":" + DTtemp.Rows[0]["YBTXLOG_TXN_HHMMSS"].ToString().Substring(4, 2);
                                    }
                                }
                                else
                                {
                                    if (drAccountData.Rows[p]["JNRST_TIME"].ToString().Length > 5) drAccountData.Rows[p]["JNRST_TIME"] = drAccountData.Rows[p]["JNRST_TIME"].ToString().Substring(0, 2) + ":" + drAccountData.Rows[p]["JNRST_TIME"].ToString().Substring(2, 2) + ":" + drAccountData.Rows[p]["JNRST_TIME"].ToString().Substring(4, 2);
                                }
                            }
                            else
                            {
                                if (drAccountData.Rows[p]["JNRST_TIME"].ToString().Length > 5) drAccountData.Rows[p]["JNRST_TIME"] = drAccountData.Rows[p]["JNRST_TIME"].ToString().Substring(0, 2) + ":" + drAccountData.Rows[p]["JNRST_TIME"].ToString().Substring(2, 2) + ":" + drAccountData.Rows[p]["JNRST_TIME"].ToString().Substring(4, 2);
                            }
                        }

                        // ATM_TIME
                        if (DTtemp.Rows.Count > 0)
                        {
                            if (String.IsNullOrEmpty(DTtemp.Rows[0]["YBTXLOG_TXN_HHMMSS"].ToString()))
                            {
                                drAccountData.Rows[p]["ATM_TIME"] = DTtemp.Rows[0]["YBTXLOG_TXN_HHMMSS"].ToString();
                            }
                            else
                            {
                                drAccountData.Rows[p]["ATM_TIME"] = DTtemp.Rows[0]["YBTXLOG_TXN_HHMMSS"].ToString().Substring(0, 2) + ":" + DTtemp.Rows[0]["YBTXLOG_TXN_HHMMSS"].ToString().Substring(2, 2) + ":" + DTtemp.Rows[0]["YBTXLOG_TXN_HHMMSS"].ToString().Substring(4, 2);
                            }
                        }
                        else
                        {
                            drAccountData.Rows[p]["ATM_TIME"] = "";
                        }
                        //

                        // YBTXLOG_SRC_ID --設備代理行
                        // YBTXLOG_STAND_NO --交易序號
                        // YBTXLOG_SAFE_TMNL_ID --機器編號
                        //  YBTXLOG_IC_MEMO_CARDNO --卡號
                        //  ADD_NARRATIVE --附言
                        // 會員編號
                        if (DTtemp.Rows.Count > 0) drAccountData.Rows[p]["YBTXLOG_SRC_ID"] = DTtemp.Rows[0]["YBTXLOG_SRC_ID"].ToString();
                        if (DTtemp.Rows.Count > 0) drAccountData.Rows[p]["YBTXLOG_STAND_NO"] = DTtemp.Rows[0]["YBTXLOG_STAND_NO"].ToString();
                        if (DTtemp.Rows.Count > 0) drAccountData.Rows[p]["YBTXLOG_SAFE_TMNL_ID"] = DTtemp.Rows[0]["YBTXLOG_SAFE_TMNL_ID"].ToString();
                        if (DTtemp.Rows.Count > 0) drAccountData.Rows[p]["YBTXLOG_IC_MEMO_CARDNO"] = DTtemp.Rows[0]["YBTXLOG_IC_MEMO_CARDNO"].ToString();
                        if (DTtemp.Rows.Count > 0) drAccountData.Rows[p]["ADD_NARRATIVE"] = DTtemp.Rows[0]["NARRATIVE"].ToString();
                        if (DTtemp.Rows.Count > 0) drAccountData.Rows[p]["Member_No"] = DTtemp.Rows[0]["Member_No"].ToString();
                        // 
                        if (drAccountData.Rows[p]["PD_TYPE_DESC"].ToString() != "99" && drAccountData.Rows[p]["PD_TYPE_DESC"].ToString().Length > 0)
                        {
                            sqlread = @"select top 1 PARMCode.CodeNo
                                               from PARMCode
                                               where PARMCode.CodeType = 'PD_TYPE_DESC'
                                               and PARMCode.CodeMemo = '" + drAccountData.Rows[p]["PD_TYPE_DESC"] + "' order by PARMCode.CodeNo";
                            DataTable DTpara = base.Search(sqlread);
                            if (DTpara.Rows.Count > 0) drAccountData.Rows[p]["PD_TYPE_DESC"] = DTpara.Rows[0]["CodeNo"].ToString(); else drAccountData.Rows[p]["PD_TYPE_DESC"] = "99";
                        }

                    }

                    //adam 20220818 end 
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
						strRow["CUSTOMER_NAME"] = ChangeChiness(drAccountData.Rows[j]["CUSTOMER_NAME"].ToString(), 60); //客戶名稱
						strRow["PrintDate"] = ChangeChiness(System.DateTime.Now.ToShortDateString(), 10); //列印日期
						strRow["QDateS"] = ChangeChiness(drAccountData.Rows[j]["QDateS"].ToString(), 10); //查詢起
						strRow["QDateE"] = ChangeChiness(drAccountData.Rows[j]["QDateE"].ToString(), 10); //查詢迄
						strRow["CreatedUser"] = ChangeValue(drAccountData.Rows[j]["CreatedUser"].ToString(), 20);
						dt.Rows.Add(strRow);
						#endregion
						// 換行
						//fileContent2 += "\r\n";
					}

					// 只能在 讀後強迫改寫檔名 2020/06/29;但如果多帳號就有風險
					if (drRecvAccount.Rows[a]["QDateS"].ToString().Length > 4)
					{
						if (drRecvAccount.Rows[a]["QDateS"].ToString().Substring(0, 4) == "1911")
						{
							Acct_DateFm_DateTo = drRecvAccount.Rows[a]["ACCT_NO"].ToString().Trim() + "-" + dt.Rows[0]["QDateS"].ToString().Trim() + "-" + dt.Rows[0]["QDateE"].ToString().Trim();
						}
					}
					CreateRFDMExcel(Acct_DateFm_DateTo, dt, txtDocIDPath);
					// 調用內容拼接方法 
					//AppendContent(dr["RFileTransactionFileName"].ToString(), fileContent2, drRecvData.Rows.Count);
					dt.Clear();
					// 記錄LOG
					m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------帳戶:" + Acct_DateFm_DateTo + "EXCEL文件中增加" + drAccountData.Rows.Count + "筆資料!");
				}
			}
			else
			{
				dt.Clear();
				// 記錄LOG
				m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------帳戶:" + DocNo + CaseTrsNewID + "無交易明細資料");
			}

			// 更新EXCEL FILE Status狀態
			if (drRecvData.Rows.Count > 0)
			{
				int RFDMInt = UpdateEXCELFILEStatus(CaseTrsNewID);
				if (RFDMInt > 0)
				{
					// 記錄LOG
					m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------  EXCEL_FILE  產檔完成  ");
				}
			}
			else
			{
				//int RFDMInt = UpdateNoEXCELFILEStatus(CaseTrsNewID);
				m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------  EXCEL_FILE  無交易資料產檔 ");
			}
		}

		public int DeleteCaseEdocFileExcel(Guid caseid)
        {
            try
            {
                int rtn = 0;
                string sql = @"delete CaseEdocFile Where CaseId=@CaseId and FileType='xlsx' and Type='歷史'";
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@CaseId", caseid));
                rtn = base.ExecuteNonQuery(sql);
                return rtn;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public int DeleteCaseEdocFilePdf(Guid caseid)
        {
            try
            {
                int rtn = 0;
                string sql = @"delete CaseEdocFile where CaseId=@CaseId and FileType='pdf' and Type='歷史'";
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@CaseId", caseid));
                rtn = base.ExecuteNonQuery(sql);
                return rtn;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public int InsertCaseEdocFile(CaseEdocFile caseEdocFile)
        {
            try
            {
                int rtn = 0;
                string sql = @"insert into CaseEdocFile values(@CaseId,@Type,@FileType,@FileName,@FileObject,@SendNo)";
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@CaseId", caseEdocFile.CaseId));
                base.Parameter.Add(new CommandParameter("@Type", caseEdocFile.Type));
                base.Parameter.Add(new CommandParameter("@FileType", caseEdocFile.FileType));
                base.Parameter.Add(new CommandParameter("@FileName", caseEdocFile.FileName));
                base.Parameter.Add(new CommandParameter("@SendNo", caseEdocFile.SendNo));
                base.Parameter.Add(new CommandParameter("@FileObject", caseEdocFile.FileObject, SqlDbType.VarBinary, 0));
                rtn = base.ExecuteNonQuery(sql);
                return rtn;
            }
            catch (Exception ex)
            {
                throw ex;
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
            ICSharpCode.SharpZipLib.Zip.ZipFile zipFile = ICSharpCode.SharpZipLib.Zip.ZipFile.Create(sourceFilePath+".zip");
            zipFile.BeginUpdate();
            addFolderToZip(zipFile, sourceFilePath, sourceFilePath);
            zipFile.CommitUpdate(); 
            zipFile.Close();
            m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------成功壓縮 " + sourceFilePath + " 資料!");
            // 舊方法
            /*
            if (sourceFilePath[sourceFilePath.Length - 1] != System.IO.Path.DirectorySeparatorChar)
                sourceFilePath += System.IO.Path.DirectorySeparatorChar;

            ZipOutputStream zipStream = new ZipOutputStream(File.Create(destinationZipFilePath + Path.GetFileName(sourceFilePath) + ".zip"));
            zipStream.SetLevel(9);  // 压缩级别 0-9
            if (Directory.Exists(sourceFilePath))                     //如果当前是文件夹，递归
            {                
                //CreateZipFiles(sourceFilePath, zipStream, sourceFilePath);
                CreateZipFile(sourceFilePath, zipStream, sourceFilePath);
                zipStream.Finish();
                zipStream.Close();
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------成功壓縮 " + sourceFilePath + " 資料!");
            }
            else
            {
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------壓縮找不到 " + sourceFilePath + " 資料!");
            }
             * */
        }
        //public void ExportRFDMTxt()
        //{
        //    m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------回文檔名稱（存款往來明細資料）開始------");
        //    // 查詢產生回文檔案的資料
        //    DataTable dtResult = GetRFDMData();

        //    string DocNo = string.Empty;

        //    if (dtResult != null && dtResult.Rows.Count > 0)
        //    {
        //       // 遍歷去除重複的資料
        //        for (int i = 0; i < dtResult.Rows.Count; i++)
        //        {
        //           if (DocNo != dtResult.Rows[i]["DocNo"].ToString())
        //           {
        //              string Filename = txtFilePath + "\\" + dtResult.Rows[i]["RFileTransactionFileName"].ToString();

        //              if (File.Exists(Filename))
        //              {
        //                 File.Delete(Filename);
        //              }

        //              DocNo = dtResult.Rows[i]["DocNo"].ToString();
        //           }

        //           // 根據主檔主鍵獲取該案件下身分證統一編號資料
        //            DataTable drRecvData = GetRFDMRecvData(dtResult.Rows[i]["CaseTrsNewID"].ToString());

        //            // RFDM內容及對應資料筆數變量
        //            string fileContent2 = "";

        //            // 遍歷並追加txt文件
        //            if (drRecvData != null && drRecvData.Rows.Count > 0)
        //            {
        //                // 遍歷並追加txt文件
        //                for (int j = 0; j < drRecvData.Rows.Count; j++)
        //                {
        //                    #region 拼接內容
        //                    // 身分證統一編號 X(10)
        //                    fileContent2 += ChangeValue(dtResult.Rows[i]["CustIdNo"].ToString(), 10);

        //                    // 帳號  X(20)
        //                    //20180622 RC 線上投單CR修正 宏祥 update start
        //                    //fileContent2 += ChangeAccount(drRecvData.Rows[j]["ACCT_NO"].ToString(), 20, drRecvData.Rows[j]["PD_TYPE_DESC"].ToString());
        //                    fileContent2 += ChangeAccount(drRecvData.Rows[j]["ACCT_NO"].ToString(), 20);
        //                    //20180622 RC 線上投單CR修正 宏祥 update start

        //                    // 交易序號    9(08)
        //                    fileContent2 += ChangeValue(drRecvData.Rows[j]["JRNL_NO"].ToString(), 8);

        //                    // 交易日期    X(08)
        //                    fileContent2 += ChangeValue(drRecvData.Rows[j]["TRAN_DATE"].ToString(), 8);

        //                    // 交易時間    X(06)???
        //                    fileContent2 += ChangeValue(drRecvData.Rows[j]["JNRST_TIME"].ToString(), 6);

        //                    // 交易行(或所屬分行代號)    X(07)
        //                    fileContent2 += ChangeValue(drRecvData.Rows[j]["TRAN_BRANCH"].ToString(), 7);

        //                    // 交易摘要    X(40)
        //                    fileContent2 += ChangeChiness(drRecvData.Rows[j]["TXN_DESC"].ToString(), 40);

        //                    // 支出金額    X(16)
        //                    // 20180119,PeterHsieh : CTBC修改規格為，當金額為負數就放在支出金額，為正就放在存入金額，另一金額欄位就放'+0'
        //                    string strTRAN_AMT = drRecvData.Rows[j]["TRAN_AMT"].ToString();
        //                    strTRAN_AMT = strTRAN_AMT.Contains("-") ? strTRAN_AMT : "+0.00";

        //                    //20180622 RC 線上投單CR修正 宏祥 add start
        //                    string strTRAN_AMT2 = ChangeNumber(strTRAN_AMT, 16, 2, false);
        //                    fileContent2 += strTRAN_AMT2.Contains("-") ? strTRAN_AMT2.Replace("-", "+") : strTRAN_AMT2;
        //                    //20180622 RC 線上投單CR修正 宏祥 add start                            

        //                    // 存入金額    X(16)
        //                    // 20180119,PeterHsieh : CTBC修改規格為，當金額為負數就放在支出金額，為正就放在存入金額，另一金額欄位就放'+0'
        //                    string SaveAMT = drRecvData.Rows[j]["TRAN_AMT"].ToString();
        //                    SaveAMT = SaveAMT.Contains("-") ? "+0.00" : ("+" + SaveAMT);

        //                    fileContent2 += ChangeNumber(SaveAMT, 16, 2, false);

        //                    // 餘額  X(16)
        //                    string strBALANCE = drRecvData.Rows[j]["BALANCE"].ToString();
        //                    strBALANCE = strBALANCE.Contains("-") ? strBALANCE : "+" + strBALANCE;

        //                    fileContent2 += ChangeNumber(strBALANCE, 16, 2, false);

        //                    // 幣別  X(03)
        //                    string Currency = GetCurrency(drRecvData.Rows[j]["ACCT_NO"].ToString());

        //                    fileContent2 += Currency;

        //                    // ATM或端末機代號   X(20)
        //                    fileContent2 += ChangeValue(drRecvData.Rows[j]["ATM_NO"].ToString(), 20);

        //                    // 櫃員代號    X(20)
        //                    fileContent2 += ChangeValue(drRecvData.Rows[j]["TELLER"].ToString(), 20);

        //                    // 轉出入行庫代碼及帳號 (RFDM) TRF_BANK + TRF_ACCT  X(20)
        //                    fileContent2 += ChangeValue(drRecvData.Rows[j]["TRF_BANK"].ToString(), 20);

        //                    // 備註(RFDM) NARRATIVE X(300)
        //                    fileContent2 += ChangeChiness(drRecvData.Rows[j]["NARRATIVE"].ToString(), 300);
        //                    #endregion

        //                    // 換行
        //                    fileContent2 += "\r\n";
        //                }

        //                // 調用內容拼接方法 
        //                AppendContent(dtResult.Rows[i]["RFileTransactionFileName"].ToString(), fileContent2, drRecvData.Rows.Count);

        //                // 記錄LOG
        //                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------向" + dtResult.Rows[i]["RFileTransactionFileName"].ToString() + "文件中增加" + drRecvData.Rows.Count + "筆資料!");
        //            }
        //            else
        //            {
        //               // 查無資料也要寫檔
        //               // 身分證統一編號 + '與本行無存款往來'
        //               fileContent2 += ChangeValue(string.Format("{0}此區間無交易往來明細", dtResult.Rows[i]["CustIdNo"].ToString()), 400) + "\r\n";

        //               // 將內容拼接到對應的txt文件中
        //               AppendContent(dtResult.Rows[i]["RFileTransactionFileName"].ToString(), fileContent2, 1);

        //               // 記錄LOG
        //               m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------向" + dtResult.Rows[i]["RFileTransactionFileName"].ToString() + "文件中增加1筆資料!");
        //            }

        //            // 更新RFDMSendStatus狀態
        //            int RFDMInt = UpdateRFDMSendStatus(dtResult.Rows[i]["CaseTrsNewID"].ToString());

        //            if (RFDMInt > 0)
        //            {
        //                // 記錄LOG
        //                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------更新CaseTrsQueryVersion表RFDMSendStatus=" + dtResult.Rows[i]["CaseTrsNewID"].ToString() + "的RFDMSendStatus='99'");
        //            }
        //        }
        //    }
        //    else
        //    {
        //        m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------沒有查詢回文檔名稱（存款往來明細資料）");
        //    }

        //    m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------回文檔名稱（存款往來明細資料）結束------");
        //}

        public DataTable GetRFDMDataMulti()
        {
            string sqlSelect = @"
                                select
                                  CaseTrsQueryVersion.CustId
                                  ,CaseTrsQueryVersion.CustAccount
                                  ,CaseTrsQueryVersion.NewID as CaseTrsNewID
                                  --,CaseMaster.RFileTransactionFileName
                                  ,CaseTrsQueryVersion.TransactionFlag
                                  ,CaseTrsQueryVersion.RFDMSendStatus
                                  ,CaseMaster.CaseNo as DocNo
                                from
                                  CaseMaster
                                left join
                                  CaseTrsQueryVersion
                                on CaseMaster.CaseID = CaseTrsQueryVersion.CaseTrsNewID
                                where
                                  TransactionFlag = 'Y'
                                  and CaseTrsQueryVersion.RFDMSendStatus = '8'
                                  and CaseTrsQueryVersion.Status = '66'
                                  and CaseMaster.Status is not Null 
								  --and ( select top 1 ATMFlag from BOPS081019Send where BOPS081019Send.VersionNewID = CaseTrsQueryVersion.NewID) = 'Y'
                                  and CaseTrsQueryVersion.CaseTrsNewID  in (
                                    select 
                                      CaseMaster.CaseID
                                    from
                                      CaseMaster
                                    left join
                                      CaseTrsQueryVersion
                                    on CaseMaster.CaseID = CaseTrsQueryVersion.CaseTrsNewID
                                    where
                                     CaseMaster.Status is not Null  and CaseTrsQueryVersion.Status = '66'                                  
                                      and TransactionFlag = 'Y'
                                      and CaseTrsQueryVersion.RFDMSendStatus  in ('8','99')
                                    group by
                                      CaseMaster.CaseID
                                  )
                                order by
                                  CaseMaster.DocNo
                                  ,CaseTrsQueryVersion.CustId  ,CaseTrsQueryVersion.CustAccount
                              ";

            // 清空容器
            base.Parameter.Clear();

            return base.Search(sqlSelect);
        }
        //
        //
        //
        public int UpdateRFDMDataStatus()
        {
            string sql = @" Update  CaseTrsQueryVersion set Status = '66'
                     where NewID in ( select                          
                                  CaseTrsQueryVersion.NewID						  
                                from
                                  CaseMaster
                                left join
                                  CaseTrsQueryVersion
                                on CaseMaster.CaseID = CaseTrsQueryVersion.CaseTrsNewID
                                where
                                  TransactionFlag = 'Y'
                                  and CaseTrsQueryVersion.RFDMSendStatus = '8'
                                  and CaseTrsQueryVersion.Status not in ('03','04')
                                  and CaseMaster.Status is not Null ) ";

            // 清空容器
            base.Parameter.Clear();

            return base.ExecuteNonQuery(sql);
        }

        /// <summary>
        /// 查詢產生回文檔案的資料
        /// </summary>
        /// <returns></returns>
        public DataTable GetRFDMData()
        {
            string sqlSelect = @"
                                select
                                  CaseTrsQueryVersion.CustId
                                  ,CaseTrsQueryVersion.CustAccount
                                  ,CaseTrsQueryVersion.NewID as CaseTrsNewID
                                  --,CaseMaster.RFileTransactionFileName
                                  ,CaseTrsQueryVersion.TransactionFlag
                                  ,CaseTrsQueryVersion.RFDMSendStatus
                                  ,CaseMaster.CaseNo as DocNo
                                from
                                  CaseMaster
                                left join
                                  CaseTrsQueryVersion
                                on CaseMaster.CaseID = CaseTrsQueryVersion.CaseTrsNewID
                                where
                                  TransactionFlag = 'Y'
                                  and CaseTrsQueryVersion.RFDMSendStatus = '8'
                                  and CaseTrsQueryVersion.Status not in ('03','04')
                                  and CaseMaster.Status is not Null 
								  -- and ( select top 1 ATMFlag from BOPS081019Send where BOPS081019Send.VersionNewID = CaseTrsQueryVersion.NewID) = 'Y'
                                  and CaseTrsQueryVersion.CaseTrsNewID  in (
                                    select 
                                      CaseMaster.CaseID
                                    from
                                      CaseMaster
                                    left join
                                      CaseTrsQueryVersion
                                    on CaseMaster.CaseID = CaseTrsQueryVersion.CaseTrsNewID
                                    where
                                     CaseMaster.Status is not Null  and CaseTrsQueryVersion.Status not in ('03','04')                                     
                                      and TransactionFlag = 'Y'
                                      and CaseTrsQueryVersion.RFDMSendStatus  in ('8','99')
                                    group by
                                      CaseMaster.CaseID
                                  )
                                order by
                                  CaseMaster.DocNo
                                  ,CaseTrsQueryVersion.CustId  ,CaseTrsQueryVersion.CustAccount
                              ";

            // 清空容器
            base.Parameter.Clear();

            return base.Search(sqlSelect);
        }

        /// <summary>
        /// 判斷是'未開戶'或'無查詢區間資料'
        /// </summary>
        /// <returns></returns>
        public string CheckCustomer(string VersionNewID, string CustIdNo)
        {
           string desc = string.Empty;

           string sqlSelect = @"
                                SELECT BOPS067050Recv.CUSTOMER_NAME 
                                from BOPS067050Recv
                                where BOPS067050Recv.VersionNewID = @VersionNewID
                                and BOPS067050Recv.CUST_ID_NO = @CustIdNo
                              ";

           // 清空容器
           base.Parameter.Clear();
           base.Parameter.Add(new CommandParameter("@VersionNewID", VersionNewID));
           base.Parameter.Add(new CommandParameter("@CustIdNo", CustIdNo));

            DataTable dtResult =base.Search(sqlSelect);

            if (dtResult != null && dtResult.Rows.Count > 0)
            {
               desc = "此區間無交易往來明細";
            }
            else
            {
               desc = "與本行無存款往來";
            }

            dtResult = null;

            return desc;
        }

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


        public DataTable GetRFDMRecvAccount(string VersionNewID)
        {
 
            string sqlSelect = @"
 SELECT  ( CaseTrsRFDMRecv.ACCT_NO
	                                     + ISNULL(QDateS, '')
	                                     + ISNULL(QDateE, '')
                                    ) AS GroupId   
                                ,CaseTrsRFDMRecv.ACCT_NO ,  ISNULL(QDateS, '') as QDateS, ISNULL(QDateE, '')  as   QDateE   

                                FROM 
                                    CaseTrsRFDMRecv 
                                LEFT JOIN CaseTrsQueryVersion ON CaseTrsQueryVersion.NewID = CaseTrsRFDMRecv.VersionNewID
                                LEFT JOIN CaseMaster ON CaseMaster.CaseID = CaseTrsQueryVersion.CaseTrsNewID
                                LEFT JOIN BOPS000401Recv ON BOPS000401Recv.VersionNewID = CaseTrsRFDMRecv.VersionNewID
	                                    and BOPS000401Recv.ACCT_NO = CaseTrsRFDMRecv.ACCT_NO
                                WHERE 
                                    CaseTrsRFDMRecv.VersionNewID = @VersionNewID
                               group by  ( CaseTrsRFDMRecv.ACCT_NO
	                                     + ISNULL(QDateS, '')
	                                     + ISNULL(QDateE, '')
                                    ),CaseTrsRFDMRecv.ACCT_NO ,  ISNULL(QDateS, '') , ISNULL(QDateE, '')  
                        ";

            // 清空容器
            base.Parameter.Clear();
            base.Parameter.Add(new CommandParameter("@VersionNewID", VersionNewID));

            return base.Search(sqlSelect);
        }
        /// <summary>
        /// 查詢HTG回文txt資料
        /// </summary>
        /// <param name="VersionNewID"></param>
        /// <returns></returns>
        public DataTable GetRFDMRecvData(string VersionNewID,string GroupId)
        {
            try
            {
                string sqlSelect = @"
with 
cr as
 (
SELECT distinct
                                    ( CaseTrsRFDMRecv.ACCT_NO
	                                     + ISNULL(QDateS, '')
	                                     + ISNULL(QDateE, '')
                                    ) AS GroupId
                                    ,CustId as CUST_ID_NO -- 客戶編號
                                    ,case when  LEN(CaseTrsQueryVersion.QDateS)=8 and CaseTrsQueryVersion.QDateS <> '19110101' 
                then     substring(CaseTrsQueryVersion.QDateS,1, 4) + substring(CaseTrsQueryVersion.QDateS,5,2) + substring(CaseTrsQueryVersion.QDateS, 7,2) 	
            WHEN CaseTrsQueryVersion.QDateS = '19110101' and LEN(BOPS000401Recv.OPEN_DATE) > 0
        THEN  substring(BOPS000401Recv.OPEN_DATE,7,4)+ substring(BOPS000401Recv.OPEN_DATE,4, 2) +substring(BOPS000401Recv.OPEN_DATE,1, 2) 
		ELSE '' END QDateS
                                    ,QDateE
                                    ,TRANS_CODE
                                    ,PROMO_CODE
                                    ,CaseTrsRFDMRecv.ACCT_NO --帳號	X(20)
                                    ,ISNULL(BOPS067050Recv.CUSTOMER_NAME, '') AS CUSTOMER_NAME --戶名
                                	,CASE WHEN ISNULL(CaseTrsRFDMRecv.FISC_SEQNO, '') = '' OR (CaseTrsRFDMRecv.FISC_SEQNO = '00000000')
                                        THEN RIGHT('00000000'+JRNL_NO,8)
                                        ELSE CaseTrsRFDMRecv.FISC_SEQNO
                                    END AS JRNL_NO--交易序號	9(08)
                                	,CONVERT(nvarchar(8),TRAN_DATE,112 ) as TRAN_DATE--交易日期	X(08)
                                    ,JNRST_TIME as ATM_TIME -- 20221229
                                    ,JNRST_TIME -- 20220818
                                    ,CASE WHEN isnull(TRAN_BRANCH,'') <> '' 
                                        THEN '822' + isnull(TRAN_BRANCH,'') 
                                        ELSE ''
                                    END TRAN_BRANCH --交易行(或所屬分行代號)	X(07)
                                	,isnull(TXN_DESC,'') as TXN_DESC--交易摘要	X(40)
                          	    ,CASE 
									   WHEN TRAN_AMT < 0  and (isnull(BOPS000401Recv.Currency,'')  = ''  or BOPS000401Recv.Currency  = 'TWD')  THEN FORMAT(TRAN_AMT*(-1), '#,#') 
									   WHEN TRAN_AMT < 0  and (isnull(BOPS000401Recv.Currency,'') <> '' and BOPS000401Recv.Currency <> 'TWD')  THEN FORMAT(TRAN_AMT*(-1), '#,0.00')  
									  ELSE FORMAT(0, '#,#') END TRAN_AMT --支出金額	X(16)
                                	,CASE 
									   WHEN TRAN_AMT >= 0  and (isnull(BOPS000401Recv.Currency,'')  = ''  or BOPS000401Recv.Currency  = 'TWD')  THEN FORMAT(TRAN_AMT, '#,#') 
									   WHEN TRAN_AMT >= 0  and (isnull(BOPS000401Recv.Currency,'') <> '' and BOPS000401Recv.Currency <> 'TWD')  THEN FORMAT(TRAN_AMT, '#,0.00') 
									   ELSE FORMAT(0, '#,#') END as  SaveAMT  --存入金額	X(16)
									 ,CASE 
									   WHEN (isnull(BOPS000401Recv.Currency,'')  = ''  or BOPS000401Recv.Currency  = 'TWD') THEN FORMAT(BALANCE, '#,#') 
									   ELSE  FORMAT(BALANCE, '#,0.00') END as  BALANCE --餘額	X(16) 
                                     ,CASE WHEN ISNULL(BOPS000401Recv.Currency,'') = ''  THEN 'TWD' ELSE  BOPS000401Recv.Currency  END as Currency
                                     , '' as ATM_NO --20220818
                                     , '' as YBTXLOG_SRC_ID --20220818
							         , '' as YBTXLOG_STAND_NO --20220818
	                                 , '' as YBTXLOG_SAFE_TMNL_ID --20220818
		                             , '' as YBTXLOG_IC_MEMO_CARDNO --20220818
		                             , '' as ADD_NARRATIVE --20220818
                                	,CaseTrsRFDMRecv.TELLER as TELLER --櫃員代號	X(20)
                                	,CaseTrsQueryVersion.CreatedUser as CreatedUser --建檔人員	X(20)
									,CaseTrsQueryVersion.CustID -- 目錄建檔用
									,CaseTrsQueryVersion.CustAccount -- 目錄建檔用
                                    ,CHQ_NO   -- 票號
                                    ,'' as Member_No -- 會員編號
                                    ,REMARK  -- 註記
                                    ,FISC_BANK -- 20220818
                                    ,FISC_SEQNO -- 20220818
	                               	,CASE WHEN CAST(isnull(TRF_ACCT,'0') AS NUMERIC) = 0
                                        THEN ''
                                        ELSE replace(replace(isnull(TRF_BANK,''),'448','822'),'000','822') + isnull(TRF_ACCT,'')
                                    END as TRF_BANK --轉出入行庫代碼及帳號 (RFDM)TRF_BANK+TRF_ACCT
                                	,isnull(CaseTrsRFDMRecv.NARRATIVE,'') as NARRATIVE  --備註 (RFDM) NARRATIVE
                                    ,CASE WHEN ISNULL(JRNL_NO,'') = ''
                                        THEN ''
                                        ELSE isnull(
										    (select top 1 PARMCode.CodeNo
											    from PARMCode
											    where PARMCode.CodeType = 'PD_TYPE_DESC'
											    and PARMCode.CodeMemo = BOPS000401Recv.PD_TYPE_DESC
										    order by PARMCode.CodeNo
										    ),'99') 
		                            END as PD_TYPE_DESC --產品別
                                FROM 
                                    CaseTrsRFDMRecv 
                                LEFT JOIN CaseTrsQueryVersion ON CaseTrsQueryVersion.NewID = CaseTrsRFDMRecv.VersionNewID
                                LEFT JOIN CaseMaster ON CaseMaster.CaseID = CaseTrsQueryVersion.CaseTrsNewID
                                LEFT JOIN BOPS067050Recv  on BOPS067050Recv.VersionNewID = CaseTrsQueryVersion.NewID
	                                        -- and BOPS067050Recv.CUST_ID_NO = CaseTrsQueryVersion.CustId
                                LEFT JOIN BOPS000401Recv ON BOPS000401Recv.VersionNewID = CaseTrsRFDMRecv.VersionNewID
	                                    and BOPS000401Recv.ACCT_NO = CaseTrsRFDMRecv.ACCT_NO
                                WHERE 
                                    CaseTrsRFDMRecv.VersionNewID = @VersionNewID
                                    and  ( CaseTrsRFDMRecv.ACCT_NO+ ISNULL(QDateS, '')+ ISNULL(QDateE, '') ) = @GroupId
)
select GroupId,CUST_ID_NO,QDateS,QDateE,TRANS_CODE,PROMO_CODE,ACCT_NO,CUSTOMER_NAME,JRNL_NO,TRAN_DATE,ATM_TIME,TRAN_BRANCH,TXN_DESC,TRAN_AMT,SaveAMT,BALANCE,Currency,ATM_NO,YBTXLOG_SRC_ID,YBTXLOG_STAND_NO,YBTXLOG_SAFE_TMNL_ID,YBTXLOG_IC_MEMO_CARDNO,ADD_NARRATIVE,TELLER,CreatedUser,CustID,CustAccount,CHQ_NO,Member_No,REMARK,FISC_BANK,FISC_SEQNO,TRF_BANK,NARRATIVE,PD_TYPE_DESC, 
(CASE WHEN ISNULL(ATM_TIME,'')<>'' THEN ATM_TIME
              ELSE JNRST_TIME END) as JNRST_TIME from cr ORDER BY GroupId,TRAN_DATE,JNRST_TIME,JRNL_NO
                        ";

                // 清空容器
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@VersionNewID", VersionNewID));
                base.Parameter.Add(new CommandParameter("@GroupId", GroupId));
                return base.Search(sqlSelect);
            }
            catch (Exception ex)
            {
                ModifyVersionStatusError(VersionNewID,ex.ToString().Substring(0,900));
                string errsql = @"
SELECT distinct
                                    ( CaseTrsRFDMRecv.ACCT_NO
	                                     + ISNULL(QDateS, '')
	                                     + ISNULL(QDateE, '')
                                    ) AS GroupId
                                    ,CustId as CUST_ID_NO -- 客戶編號
                                    ,case when  LEN(CaseTrsQueryVersion.QDateS)=8 and CaseTrsQueryVersion.QDateS <> '19110101' 
                then     substring(CaseTrsQueryVersion.QDateS,1, 4) + substring(CaseTrsQueryVersion.QDateS,5,2) + substring(CaseTrsQueryVersion.QDateS, 7,2) 	
            WHEN CaseTrsQueryVersion.QDateS = '19110101' and LEN(BOPS000401Recv.OPEN_DATE) > 0
        THEN  substring(BOPS000401Recv.OPEN_DATE,7,4)+ substring(BOPS000401Recv.OPEN_DATE,4, 2) +substring(BOPS000401Recv.OPEN_DATE,1, 2) 
		ELSE '' END QDateS
                                    ,QDateE
                                    ,TRANS_CODE
                                    ,PROMO_CODE
                                    ,CaseTrsRFDMRecv.ACCT_NO --帳號	X(20)
                                    ,ISNULL(BOPS067050Recv.CUSTOMER_NAME, '') AS CUSTOMER_NAME --戶名
                                	,CASE WHEN ISNULL(CaseTrsRFDMRecv.FISC_SEQNO, '') = '' OR (CaseTrsRFDMRecv.FISC_SEQNO = '00000000')
                                        THEN RIGHT('00000000'+JRNL_NO,8)
                                        ELSE CaseTrsRFDMRecv.FISC_SEQNO
                                    END AS JRNL_NO--交易序號	9(08)
                                	,CONVERT(nvarchar(8),TRAN_DATE,112 ) as TRAN_DATE--交易日期	X(08)
                                    ,JNRST_TIME as ATM_TIME -- 20221229
                                    ,JNRST_TIME -- 20220818
                                    ,CASE WHEN isnull(TRAN_BRANCH,'') <> '' 
                                        THEN '822' + isnull(TRAN_BRANCH,'') 
                                        ELSE ''
                                    END TRAN_BRANCH --交易行(或所屬分行代號)	X(07)
                                	,isnull(TXN_DESC,'') as TXN_DESC--交易摘要	X(40)
                          	    ,CASE 
									   WHEN TRAN_AMT < 0  and (isnull(BOPS000401Recv.Currency,'')  = ''  or BOPS000401Recv.Currency  = 'TWD')  THEN FORMAT(TRAN_AMT*(-1), '#,#') 
									   WHEN TRAN_AMT < 0  and (isnull(BOPS000401Recv.Currency,'') <> '' and BOPS000401Recv.Currency <> 'TWD')  THEN FORMAT(TRAN_AMT*(-1), '#,0.00')  
									  ELSE FORMAT(0, '#,#') END TRAN_AMT --支出金額	X(16)
                                	,CASE 
									   WHEN TRAN_AMT >= 0  and (isnull(BOPS000401Recv.Currency,'')  = ''  or BOPS000401Recv.Currency  = 'TWD')  THEN FORMAT(TRAN_AMT, '#,#') 
									   WHEN TRAN_AMT >= 0  and (isnull(BOPS000401Recv.Currency,'') <> '' and BOPS000401Recv.Currency <> 'TWD')  THEN FORMAT(TRAN_AMT, '#,0.00') 
									   ELSE FORMAT(0, '#,#') END as  SaveAMT  --存入金額	X(16)
									 ,CASE 
									   WHEN (isnull(BOPS000401Recv.Currency,'')  = ''  or BOPS000401Recv.Currency  = 'TWD') THEN FORMAT(BALANCE, '#,#') 
									   ELSE  FORMAT(BALANCE, '#,0.00') END as  BALANCE --餘額	X(16) 
                                     ,CASE WHEN ISNULL(BOPS000401Recv.Currency,'') = ''  THEN 'TWD' ELSE  BOPS000401Recv.Currency  END as Currency
                                     , '' as ATM_NO --20220818
                                     , '' as YBTXLOG_SRC_ID --20220818
							         , '' as YBTXLOG_STAND_NO --20220818
	                                 , '' as YBTXLOG_SAFE_TMNL_ID --20220818
		                             , '' as YBTXLOG_IC_MEMO_CARDNO --20220818
		                             , '' as ADD_NARRATIVE --20220818
                                	,CaseTrsRFDMRecv.TELLER as TELLER --櫃員代號	X(20)
                                	,CaseTrsQueryVersion.CreatedUser as CreatedUser --建檔人員	X(20)
									,CaseTrsQueryVersion.CustID -- 目錄建檔用
									,CaseTrsQueryVersion.CustAccount -- 目錄建檔用
                                    ,CHQ_NO   -- 票號
                                    ,REMARK  -- 註記
                                    ,FISC_BANK -- 20220818
                                    ,FISC_SEQNO -- 20220818
	                               	,CASE WHEN CAST(isnull(TRF_ACCT,'0') AS NUMERIC) = 0
                                        THEN ''
                                        ELSE replace(replace(isnull(TRF_BANK,''),'448','822'),'000','822') + isnull(TRF_ACCT,'')
                                    END as TRF_BANK --轉出入行庫代碼及帳號 (RFDM)TRF_BANK+TRF_ACCT
                                	,isnull(CaseTrsRFDMRecv.NARRATIVE,'') as NARRATIVE  --備註 (RFDM) NARRATIVE
                                    ,CASE WHEN ISNULL(JRNL_NO,'') = ''
                                        THEN ''
                                        ELSE isnull(
										    (select top 1 PARMCode.CodeNo
											    from PARMCode
											    where PARMCode.CodeType = 'PD_TYPE_DESC'
											    and PARMCode.CodeMemo = BOPS000401Recv.PD_TYPE_DESC
										    order by PARMCode.CodeNo
										    ),'99') 
		                            END as PD_TYPE_DESC --產品別
                                FROM 
                                    CaseTrsRFDMRecv 
                                LEFT JOIN CaseTrsQueryVersion ON CaseTrsQueryVersion.NewID = CaseTrsRFDMRecv.VersionNewID
                                LEFT JOIN CaseMaster ON CaseMaster.CaseID = CaseTrsQueryVersion.CaseTrsNewID
                                LEFT JOIN BOPS067050Recv  on BOPS067050Recv.VersionNewID = CaseTrsQueryVersion.NewID
	                                        -- and BOPS067050Recv.CUST_ID_NO = CaseTrsQueryVersion.CustId
                                LEFT JOIN BOPS000401Recv ON BOPS000401Recv.VersionNewID = CaseTrsRFDMRecv.VersionNewID
	                                    and BOPS000401Recv.ACCT_NO = CaseTrsRFDMRecv.ACCT_NO
                                where     CaseTrsRFDMRecv.VersionNewID = '99999999-9999-9999-9999-9999999999999' ";
                return base.Search(errsql);
            }
        }

        public DataTable GetRFDMRecvData(string VersionNewID)
        {
            /*
             * 20180718,PeterHsieh : 配合 CR處理，修改 ATM或端末機代號欄位取得來源
             *      (ATM.交易日期 = RFDM.交易日期 and ATM.交易序號 = RFDM.交易序號 and ATM.交易時間 = RFDM.交易時間(前 6bytes)
             * 
             * 20180725,PeterHsieh : 調整如下
             *      01.交易序號 : 改取 CaseCustRFDMRecv.FISC_BANK(金資序號)，但若此欄位為空或'00000000'時，則取 CaseCustRFDMRecv.JRNL_NO(8 bytes)
             *      02.ATM或端末機代號 : 修改 Join條件
             *          (ATM.交易日期 = RFDM.交易日期 and ('0'+ATM.交易序號) = RFDM.金資序號)
             */
            try
            {
                string sqlSelect = @"
with 
cr as
 (
SELECT distinct
                                    ( CaseTrsRFDMRecv.ACCT_NO
	                                     + ISNULL(QDateS, '')
	                                     + ISNULL(QDateE, '')
                                    ) AS GroupId
                                    ,CustId as CUST_ID_NO -- 客戶編號
                                    ,case when  LEN(CaseTrsQueryVersion.QDateS)=8 and CaseTrsQueryVersion.QDateS <> '19110101' 
                then     substring(CaseTrsQueryVersion.QDateS,1, 4) + substring(CaseTrsQueryVersion.QDateS,5,2) + substring(CaseTrsQueryVersion.QDateS, 7,2) 	
            WHEN CaseTrsQueryVersion.QDateS = '19110101' and LEN(BOPS000401Recv.OPEN_DATE) > 0
        THEN  substring(BOPS000401Recv.OPEN_DATE,7,4)+ substring(BOPS000401Recv.OPEN_DATE,4, 2) +substring(BOPS000401Recv.OPEN_DATE,1, 2) 
		ELSE '' END QDateS
                                    ,QDateE   
                                    ,TRANS_CODE
                                    ,PROMO_CODE
                                    ,CaseTrsRFDMRecv.ACCT_NO --帳號	X(20)
                                    ,ISNULL(BOPS067050Recv.CUSTOMER_NAME, '') AS CUSTOMER_NAME --戶名
                                	,CASE WHEN ISNULL(CaseTrsRFDMRecv.FISC_SEQNO, '') = '' OR (CaseTrsRFDMRecv.FISC_SEQNO = '00000000')
                                        THEN RIGHT('00000000'+JRNL_NO,8)
                                        ELSE CaseTrsRFDMRecv.FISC_SEQNO
                                    END AS JRNL_NO--交易序號	9(08)
                                	,CONVERT(nvarchar(8),TRAN_DATE,112 ) as TRAN_DATE--交易日期	X(08)
                                    , JNRST_TIME as ATM_TIME -- 20221229
  	                                , JNRST_TIME
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
                                     , '' as ATM_NO --ATM或端末機代號	X(20)
							   , '' as YBTXLOG_SRC_ID 
							  , '' as YBTXLOG_STAND_NO
	                            , '' as YBTXLOG_SAFE_TMNL_ID 
		                            , '' as YBTXLOG_IC_MEMO_CARDNO
		                            , '' as ADD_NARRATIVE
                                	,CaseTrsRFDMRecv.TELLER as TELLER --櫃員代號	X(20)
                                    ,CaseTrsQueryVersion.CreatedUser as CreatedUser --建檔人員	X(20)
									,CaseTrsQueryVersion.CustID -- 目錄建檔用
									,CaseTrsQueryVersion.CustAccount -- 目錄建檔用
                                    ,CHQ_NO   -- 票號
                                    ,'' as Member_No -- 會員編號
                                    ,REMARK  -- 註記
                                    ,FISC_BANK
                                    ,FISC_SEQNO
	                               	,CASE WHEN CAST(isnull(TRF_ACCT,'0') AS NUMERIC) = 0
                                        THEN ''
                                        ELSE replace(replace(isnull(TRF_BANK,''),'448','822'),'000','822') + isnull(TRF_ACCT,'')
                                    END as TRF_BANK --轉出入行庫代碼及帳號 (RFDM)TRF_BANK+TRF_ACCT
                                	,isnull(CaseTrsRFDMRecv.NARRATIVE,'') as NARRATIVE  --備註 (RFDM) NARRATIVE
                                    ,CASE WHEN ISNULL(JRNL_NO,'') = ''
                                        THEN ''
                                        ELSE  BOPS000401Recv.PD_TYPE_DESC
										     
		                            END as PD_TYPE_DESC --產品別                             
                                FROM 
                                    CaseTrsRFDMRecv 
                                LEFT JOIN CaseTrsQueryVersion ON CaseTrsQueryVersion.NewID = CaseTrsRFDMRecv.VersionNewID
                                LEFT JOIN CaseMaster ON CaseMaster.CaseID = CaseTrsQueryVersion.CaseTrsNewID
                                LEFT JOIN BOPS067050Recv  on BOPS067050Recv.VersionNewID = CaseTrsQueryVersion.NewID
	                                        -- and BOPS067050Recv.CUST_ID_NO = CaseTrsQueryVersion.CustId
                                LEFT JOIN BOPS000401Recv ON BOPS000401Recv.VersionNewID = CaseTrsRFDMRecv.VersionNewID
	                                    and BOPS000401Recv.ACCT_NO = CaseTrsRFDMRecv.ACCT_NO
                                WHERE 
                                    CaseTrsRFDMRecv.VersionNewID = @VersionNewID
     	)

select GroupId,CUST_ID_NO,QDateS,QDateE,TRANS_CODE,PROMO_CODE,ACCT_NO,CUSTOMER_NAME,JRNL_NO,TRAN_DATE,ATM_TIME,TRAN_BRANCH,TXN_DESC,TRAN_AMT,SaveAMT,BALANCE,Currency,ATM_NO,YBTXLOG_SRC_ID,YBTXLOG_STAND_NO,YBTXLOG_SAFE_TMNL_ID,YBTXLOG_IC_MEMO_CARDNO,Member_No,ADD_NARRATIVE,TELLER,CreatedUser,CustID,CustAccount,CHQ_NO,REMARK,FISC_BANK,FISC_SEQNO,TRF_BANK,NARRATIVE,PD_TYPE_DESC, 
(CASE WHEN ISNULL(ATM_TIME,'')<>'' THEN ATM_TIME
              ELSE JNRST_TIME END) as JNRST_TIME from cr ORDER BY GroupId,TRAN_DATE,JNRST_TIME,JRNL_NO

                        ";

                // 清空容器
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@VersionNewID", VersionNewID));
                return base.Search(sqlSelect);
            }
            catch (Exception ex)
            {
                ModifyVersionStatusError(VersionNewID,ex.ToString());
                string errsql = @"
SELECT distinct
                                    ( CaseTrsRFDMRecv.ACCT_NO
	                                     + ISNULL(QDateS, '')
	                                     + ISNULL(QDateE, '')
                                    ) AS GroupId
                                    ,CustId as CUST_ID_NO -- 客戶編號
                                    ,case when  LEN(CaseTrsQueryVersion.QDateS)=8 and CaseTrsQueryVersion.QDateS <> '19110101' 
                then     substring(CaseTrsQueryVersion.QDateS,1, 4) + substring(CaseTrsQueryVersion.QDateS,5,2) + substring(CaseTrsQueryVersion.QDateS, 7,2) 	
            WHEN CaseTrsQueryVersion.QDateS = '19110101' and LEN(BOPS000401Recv.OPEN_DATE) > 0
        THEN  substring(BOPS000401Recv.OPEN_DATE,7,4)+ substring(BOPS000401Recv.OPEN_DATE,4, 2) +substring(BOPS000401Recv.OPEN_DATE,1, 2) 
		ELSE '' END QDateS
                                    ,QDateE   
                                    ,TRANS_CODE
                                    ,PROMO_CODE
                                    ,CaseTrsRFDMRecv.ACCT_NO --帳號	X(20)
                                    ,ISNULL(BOPS067050Recv.CUSTOMER_NAME, '') AS CUSTOMER_NAME --戶名
                                	,CASE WHEN ISNULL(CaseTrsRFDMRecv.FISC_SEQNO, '') = '' OR (CaseTrsRFDMRecv.FISC_SEQNO = '00000000')
                                        THEN RIGHT('00000000'+JRNL_NO,8)
                                        ELSE CaseTrsRFDMRecv.FISC_SEQNO
                                    END AS JRNL_NO--交易序號	9(08)
                                	,CONVERT(nvarchar(8),TRAN_DATE,112 ) as TRAN_DATE--交易日期	X(08)
									,  JNRST_TIME as ATM_TIME -- 20221229
		                              , JNRST_TIME
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
                                     , '' as ATM_NO --ATM或端末機代號	X(20)
							   , '' as YBTXLOG_SRC_ID 
							  , '' as YBTXLOG_STAND_NO
	                            , '' as YBTXLOG_SAFE_TMNL_ID 
		                            , '' as YBTXLOG_IC_MEMO_CARDNO
		                            , '' as ADD_NARRATIVE
                                	,CaseTrsRFDMRecv.TELLER as TELLER --櫃員代號	X(20)
                                    ,CaseTrsQueryVersion.CreatedUser as CreatedUser --建檔人員	X(20)
									,CaseTrsQueryVersion.CustID -- 目錄建檔用
									,CaseTrsQueryVersion.CustAccount -- 目錄建檔用
                                    ,CHQ_NO   -- 票號
                                    ,REMARK  -- 註記
                                    ,FISC_BANK
                                    ,FISC_SEQNO
	                               	,CASE WHEN CAST(isnull(TRF_ACCT,'0') AS NUMERIC) = 0
                                        THEN ''
                                        ELSE replace(replace(isnull(TRF_BANK,''),'448','822'),'000','822') + isnull(TRF_ACCT,'')
                                    END as TRF_BANK --轉出入行庫代碼及帳號 (RFDM)TRF_BANK+TRF_ACCT
                                	,isnull(CaseTrsRFDMRecv.NARRATIVE,'') as NARRATIVE  --備註 (RFDM) NARRATIVE
                                    ,CASE WHEN ISNULL(JRNL_NO,'') = ''
                                        THEN ''
                                        ELSE  BOPS000401Recv.PD_TYPE_DESC
										     
		                            END as PD_TYPE_DESC --產品別                             
                                FROM 
                                    CaseTrsRFDMRecv 
                                LEFT JOIN CaseTrsQueryVersion ON CaseTrsQueryVersion.NewID = CaseTrsRFDMRecv.VersionNewID
                                LEFT JOIN CaseMaster ON CaseMaster.CaseID = CaseTrsQueryVersion.CaseTrsNewID
                                LEFT JOIN BOPS067050Recv  on BOPS067050Recv.VersionNewID = CaseTrsQueryVersion.NewID
	                                        -- and BOPS067050Recv.CUST_ID_NO = CaseTrsQueryVersion.CustId
                                LEFT JOIN BOPS000401Recv ON BOPS000401Recv.VersionNewID = CaseTrsRFDMRecv.VersionNewID
	                                    and BOPS000401Recv.ACCT_NO = CaseTrsRFDMRecv.ACCT_NO
                                WHERE
                                    CaseTrsRFDMRecv.VersionNewID = '99999999-9999-9999-9999-9999999999999' ";
                return base.Search(errsql);
            }
        }



        /// <summary>
        /// 更新CaseTrsQueryVersion表RFDMSendStatus
        /// </summary>
        /// <param name="VersionNewID"></param>
        /// 
        public int UpdateEXCELFILEBaseStatus(string strNewID)
        {
            string sql = @"
                         Update CaseTrsQueryVersion 
                         set ModifiedDate=getdate() ,EXCEL_FILE = 'Y' 
                         where NewID = @NewID  and HTGSendStatus = '88' ";
            // 清空容器
            base.Parameter.Clear();
            base.Parameter.Add(new CommandParameter("@NewID", strNewID));
            return base.ExecuteNonQuery(sql);
        }
        public int UpdateEXCELFILEStatus(string strNewID)
        {
            string sql = @"
                         Update CaseTrsQueryVersion 
                         set ModifiedDate=getdate() ,EXCEL_FILE = 'Y' 
                         where NewID = @NewID ";

            // 清空容器
            base.Parameter.Clear();
            base.Parameter.Add(new CommandParameter("@NewID", strNewID));

            return base.ExecuteNonQuery(sql);
        }

        public int UpdateNoEXCELFILEStatus(string strNewID)
        {
            string sql = @"
                         Update CaseTrsQueryVersion 
                         set ModifiedDate=getdate() ,EXCEL_FILE = 'N'  
                         where NewID = @NewID  and OpenFlag = 'Y'   ";

            // 清空容器
            base.Parameter.Clear();
            base.Parameter.Add(new CommandParameter("@NewID", strNewID));

            return base.ExecuteNonQuery(sql);
        }

        public int UpdateNoEXCELFILEHTGMessage(string strNewID, string err)
        {
            string sql = @"Update CaseTrsQueryVersion 
                         set ModifiedDate=getdate() ,HTGQryMessage = @ERR  where NewID = @NewID  ";

            // 清空容器
            base.Parameter.Clear();
            base.Parameter.Add(new CommandParameter("@NewID", strNewID));
            base.Parameter.Add(new CommandParameter("@ERR", err));

            return base.ExecuteNonQuery(sql);
        }
        public int UpdateNoEXCELFILEMessage(string strNewID,string err)
        {
            string sql = @"Update CaseTrsQueryVersion 
                         set ModifiedDate=getdate() ,RFDMQryMessage = @ERR  where NewID = @NewID  ";

            // 清空容器
            base.Parameter.Clear();
            base.Parameter.Add(new CommandParameter("@NewID", strNewID));
            base.Parameter.Add(new CommandParameter("@ERR", err));

            return base.ExecuteNonQuery(sql);
        }
        public int UpdateNoPDFFILEMessage(string strNewID, string err)
        {
            string sql = @"Update CaseTrsQueryVersion 
                         set ModifiedDate=getdate() ,RFDMQryMessage = @ERR where NewID = @NewID ";

            // 清空容器
            base.Parameter.Clear();
            base.Parameter.Add(new CommandParameter("@NewID", strNewID));
            base.Parameter.Add(new CommandParameter("@ERR", err));

            return base.ExecuteNonQuery(sql);
        }

        public int UpdateNoPDFFILEStatus(string strNewID)
        {
            string sql = @"
                         Update CaseTrsQueryVersion 
                         set ModifiedDate=getdate() ,PDF_FILE = 'N' 
                         where NewID = @NewID and TransactionFlag = 'Y' ";

            // 清空容器
            base.Parameter.Clear();
            base.Parameter.Add(new CommandParameter("@NewID", strNewID));

            return base.ExecuteNonQuery(sql);
        }
           public int UpdatePDFFILEStatus(string strNewID)
        {
            string sql = @"
                         Update CaseTrsQueryVersion 
                         set ModifiedDate=getdate() ,PDF_FILE = 'Y' 
                         where NewID = @NewID ";

            // 清空容器
            base.Parameter.Clear();
            base.Parameter.Add(new CommandParameter("@NewID", strNewID));

            return base.ExecuteNonQuery(sql);
        }
//        public int UpdateRFDMSendStatus(string strNewID)
//        {
//            string sql = @"
//                         Update CaseTrsQueryVersion 
//                         set HTGSendStatus = '88', RFDMSendStatus = '99'   ,ModifiedDate=getdate() ,PDF_FILE = 'Y' 
//                         where NewID = @NewID ";

//            // 清空容器
//            base.Parameter.Clear();
//            base.Parameter.Add(new CommandParameter("@NewID", strNewID));

//            return base.ExecuteNonQuery(sql);
//        }
        #endregion

        #region 更新案件主檔的狀態和Version狀態更新成成功或者重查成功

        /// <summary>
        /// HTG發查回文產生成功 並且 RFDF回文產生成功，將Version狀態更新成成功或者重查成功
        /// </summary>
           public void ModifyHTGRFDMStatus()
           {
               try
               {
                   // 將發查中的案件HTG,RFDM SENDSTATUS 改為 88 
                   string sql = @"
                 Update CaseTrsQueryVersion 
                                         set HTGSendStatus = '88'  ,ModifiedDate=getdate()   WHERE  EXISTS
                (
                SELECT NEWID
                 FROM
                 (
	                SELECT NEWID from CaseTrsQueryVersion WHERE OpenFlag = 'Y'    AND Status = '02'  AND EXCEL_FILE = 'Y'
                )RESULT
                WHERE CaseTrsQueryVersion.NewID = RESULT.NewID
                ); ";
                   base.ExecuteNonQuery(sql);
               }
               catch (Exception ex)
               {
                 m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "更新狀態錯誤:" + ex.ToString());
                //throw ex;
               }
               //
               try
               {
                   // 將發查中的案件HTG,RFDM SENDSTATUS 改為 99
                   string sql = @"
                 Update CaseTrsQueryVersion 
                                         set  RFDMSendStatus = '99' ,ModifiedDate=getdate()   WHERE  EXISTS
                (
                SELECT NEWID
                 FROM
                 (
	                SELECT NEWID from CaseTrsQueryVersion WHERE TransactionFlag = 'Y'   AND Status = '02'  AND PDF_FILE = 'Y'
                )RESULT
                WHERE CaseTrsQueryVersion.NewID = RESULT.NewID
                ); ";
                   base.ExecuteNonQuery(sql);
               }
               catch (Exception ex)
               {
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "更新狀態錯誤:" + ex.ToString());
                //throw ex;
               }

           }
        public void ModifyVersionStatusError(string newid,string errmessage)
        {
            //
            try
            {
                // 將發查中的案件狀態更新為【失敗】
                string sql = @" UPDATE CaseTrsQueryVersion SET Status = '04' ,  RFDMQryMessage = '" + errmessage + "'  WHERE  NewID = '" + newid+ "' ";
                base.ExecuteNonQuery(sql);
            }
            catch (Exception ex)
            {
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "更新狀態錯誤:" + ex.ToString());
                //throw ex;
            }
        }
        public void ModifyVersionStatus1()
        {
            //
            try
            {
                // 將發查中的案件狀態更新為【成功】
                string sql = @"
                UPDATE CaseTrsQueryVersion SET Status = '03' WHERE  EXISTS
                (
                SELECT NEWID
                 FROM
                 (
	                SELECT NEWID from CaseTrsQueryVersion WHERE  ( Status = '66')  AND ( isnull(EXCEL_FILE,'') = 'Y' OR  isnull(PDF_FILE,'')  = 'Y')  -- AND ( isnull(HTGSendStatus,'')  = '88' OR isnull(RFDMSendStatus,'') = '99' )
                )RESULT
                WHERE CaseTrsQueryVersion.NewID = RESULT.NewID
                ); ";
                base.ExecuteNonQuery(sql);
            }
            catch (Exception ex)
            {
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "更新狀態錯誤:" + ex.ToString());
                //throw ex;
            }

            try
            {
                // 將發查中的案件狀態更新為【失敗】
                string sql = @"
UPDATE CaseTrsQueryVersion SET Status = '04' WHERE  EXISTS
(
SELECT NEWID
 FROM
 (
	SELECT NEWID from CaseTrsQueryVersion WHERE  Status = '66'  AND  isnull(EXCEL_FILE,'') <> 'Y'  AND  isnull(PDF_FILE,'') <>  'Y'    AND  ( isnull(HTGSendStatus,'') <> '4' OR isnull(RFDMSendStatus,'')  <> '8' )
)RESULT
WHERE CaseTrsQueryVersion.NewID = RESULT.NewID
);";
                base.ExecuteNonQuery(sql);
            }
            catch (Exception ex)
            {
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "更新狀態錯誤:" + ex.ToString());
                //throw ex;
            }
        }

        /// <summary>
        /// 更新案件主檔的狀態
        /// </summary>
        public void ModifyCaseStatus2()
        {
            // 查詢發查中的案件主檔
            DataTable dtSuccessSend = GetSuccessSend();

            if (dtSuccessSend != null && dtSuccessSend.Rows.Count > 0)
            {
                for (int i = 0; i < dtSuccessSend.Rows.Count; i++)
                {
                    // 案件編號狀態為發查中，並且version檔的資料全部發查成功，將主檔狀態更新為03
                    if (dtSuccessSend.Rows[i]["VERSIONCOUNT"].ToString() == dtSuccessSend.Rows[i]["CASECOUNT"].ToString()
                        && dtSuccessSend.Rows[i]["Status"].ToString() == "02")
                    {
                        UpdateCaseQueryStatus("03", dtSuccessSend.Rows[i]["CaseTrsNewID"].ToString());
                    }
                    // 案件編號狀態為重查拋查中，並且version檔的資料全部發查成功，將主檔狀態更新為07[重查成功]
                    else if (dtSuccessSend.Rows[i]["VERSIONCOUNT"].ToString() == dtSuccessSend.Rows[i]["CASECOUNT"].ToString()
                        && dtSuccessSend.Rows[i]["Status"].ToString() == "06")
                    {
                        UpdateCaseQueryStatus("07", dtSuccessSend.Rows[i]["CaseTrsNewID"].ToString());
                    }
                }
            }
        }

        /// <summary>
        /// 查詢Version檔成功的案件筆數，和案件下應該發查的資料筆數
        /// </summary>
        /// <returns></returns>
        public DataTable GetSuccessSend()
        {
            try
            {
                string strDataSql = @"
SELECT 
	RESULT.VERSIONCOUNT, RESULT.Status, CaseTrsNewID,
	(SELECT COUNT(CaseTrsNewID) FROM  CaseTrsQueryVersion WHERE CaseTrsQueryVersion.CaseTrsNewID = RESULT.CaseTrsNewID  ) AS CASECOUNT
FROM
(
	SELECT CaseTrsNewID , CaseMaster.Status, COUNT(CaseTrsNewID) AS VERSIONCOUNT
	from CaseTrsQueryVersion
	INNER JOIN CaseMaster	
		ON  CaseTrsQueryVersion.CaseTrsNewID = CaseMaster.CaseID
		AND CaseMaster.Status IN ('02', '06') -- 主檔案件狀態為發查中
	WHERE CaseTrsQueryVersion.Status = '03' OR CaseTrsQueryVersion.Status = '07' --Version狀態為發查成功或重查成功
	GROUP BY CaseTrsQueryVersion.CaseTrsNewID, CaseMaster.Status
)RESULT
                    ";

                return base.Search(strDataSql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 更新案件主檔的狀態
        /// </summary>
        /// <param name="Status"></param>
        /// <param name="NewID"></param>
        /// <returns></returns>
        public int UpdateCaseQueryStatus(string Status, string NewID)
        {
            try
            {
                string sql = @"UPDATE CaseMaster SET Status = @Status  WHERE  NewID = @NewID ";

                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@Status", Status));
                base.Parameter.Add(new CommandParameter("@NewID", NewID));

                return base.ExecuteNonQuery(sql);
            }
            catch (Exception ex)
            {
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "更新狀態錯誤:" + ex.ToString());
                throw ex;
            }
        }

        #endregion

        #region 共用自定義方法

        protected void CreateRFDMPdf(string AccountName, DataTable dt, string strFileName)
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


        protected void CreateRFDMExcel(string AccountName, DataTable dt, string strFileName)
        {
            //string strReturn = ""; 

            string strPath = strFileName + "\\" + AccountName + ".xls";
            if (File.Exists(strPath))
            {
                File.Delete(strPath);
            }
            //using (FileStream file = new FileStream(@"c:\temp\testfile.xls", FileMode.Open, FileAccess.Read))
            //{
            //    hssfwb = new HSSFWorkbook(file);
            //    file.Close();
            //}
            HSSFWorkbook workbook = new HSSFWorkbook();
            MemoryStream ms = new MemoryStream();
            FileStream file = new FileStream(strPath, FileMode.Create);
            try
            {
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
                        if (dt.Rows[i - IBeginCount][j]==null)
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
            catch (Exception ex)
            {
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "EXCEL產檔錯誤檔名:" + strFileName + ex.ToString());
            }
            finally
            {
                ms.Close();
                file.Close();
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
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.Information, "LocalReport 產生 PDF文件報錯: " + ex.Message);
                //throw ex;
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
            //string strReturn = ""; 
           
            string strPath =  strFileName+@"\基本資料.xls";
            if (File.Exists(strPath))
            {
                File.Delete(strPath);
            }
            //using (FileStream file = new FileStream(@"c:\temp\testfile.xls", FileMode.Open, FileAccess.Read))
            //{
            //    hssfwb = new HSSFWorkbook(file);
            //    file.Close();
            //}
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
                        bool bENg= IsEG(dt.Rows[i - IBeginCount]["CUST_ID_NO"].ToString().Substring(dt.Rows[i - IBeginCount]["CUST_ID_NO"].ToString().Length - 3, 3));
                        if (bENg == true)
                        {
                            cell.SetCellValue(dt.Rows[i - IBeginCount]["CUST_ID_NO"].ToString()+"/"+ dt.Rows[i - IBeginCount]["IdNo02"].ToString()); //"身分證統一編號/居留證號");
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
                Console.WriteLine(ex.Message);
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
                Console.WriteLine(ex.Message);
            }

            finally
            {
                zis.Close();
                zis.Dispose();
            }
        }

        //public string ChangeAccount(string strValue, int strValueLen, string strPD_TYPE_DESC)
        //{
        //    // 帳號
        //    string strAccount = "";

        //    if (!string.IsNullOrEmpty(strValue))
        //    {
        //        // 截取標誌位
        //        string strFlag = strValue.Length > 4 ? strValue.Substring(0, 4) : "";

        //        // strFlag == "0000",帳號爲strValue後12位,否則爲前幾位
        //        if (strFlag == "0000")
        //        {
        //            // 截取帳號
        //            strAccount = strValue.Substring(4, strValue.Length - 4);
        //        }
        //        else
        //        {
        //            // 截取帳號
        //            strAccount = strValue.Substring(0, strValue.Length - 3);

        //            // 截取標誌位
        //            strFlag = strValue.Substring(strValue.Length - 3, 3);
        //        }

        //        // 獲取幣別
        //        DataRow[] dr = CurrencyList.Select("CodeNo ='" + strFlag + "'");
        //        string strCurrency = dr != null && dr.Length > 0 ? dr[0]["CodeDesc"].ToString() : "";
        //        // 如果幣別爲空,就用空白代替
        //        if (string.IsNullOrEmpty(strCurrency) || strCurrency.Length < 3)
        //        {
        //            strCurrency = strCurrency + AddSpace(3 - strCurrency.Length, strNull);
        //        }
        //        else
        //        {
        //            strCurrency = strCurrency.Substring(0, 3);
        //        }

        //        // 拼接帳號和幣別
        //        strAccount = strAccount + strCurrency;
        //    }

        //    // 拼接產品別
        //    strAccount += "-" + strPD_TYPE_DESC;

        //    // 指定長度-字串長度
        //    int strNullNumber = strValueLen - strAccount.Length;

        //    // strNullNumber>0,就在字串後拼接strNullNumber個半形空白,否則就截取指定長度
        //    strAccount = strNullNumber > 0 ? strAccount + AddSpace(strNullNumber, strNull) : strAccount.Substring(0, strValueLen);

        //    return strAccount;
        //}
        //20180622 RC 線上投單CR修正 宏祥 update end

        /// <summary>
        /// 獲取 幣別代碼檔
        /// </summary>
        /// <returns></returns>
        public DataTable GetParmCodeCurrency()
        {
            string sqlSelect = @" select CodeNo,CodeDesc from PARMCode where  CodeType='CaseCust_CURRENCY' ";

            // 清空容器
            base.Parameter.Clear();
            return base.Search(sqlSelect);
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
        public void OutPutPDF()
        {
            // 查詢要列印PDF的案件編號(一個案件下有多個人，只有有一個人HTG、RFDM發查成功就產出PDF,歷史記錄功能用)
            DataTable dtPDFList = GetQueryPDFList();

            if (dtPDFList != null && dtPDFList.Rows.Count > 0)
            {
                ExportReportPDF _ExportReportPDF = new ExportReportPDF(m_fileLog);

                #region 獲取回文用到的代碼

                DataTable dtPARMCode = GetPARMCode();
                DataTable dtSendSettingRef = GetSendSettingRef();

                string Address = dtPARMCode.Rows.Count > 0 ? dtPARMCode.Select("CodeNo = 'Address'")[0].ItemArray[0].ToString() : "";
                string ButtomLine = dtPARMCode.Rows.Count > 0 ? dtPARMCode.Select("CodeNo = 'ButtomLine'")[0].ItemArray[0].ToString() : "";
                string ButtomLine2 = dtPARMCode.Rows.Count > 0 ? dtPARMCode.Select("CodeNo = 'ButtomLine2'")[0].ItemArray[0].ToString() : "";
                string Speed = dtPARMCode.Rows.Count > 0 ? dtPARMCode.Select("CodeNo = 'Speed'")[0].ItemArray[0].ToString() : "";
                string Security = dtPARMCode.Rows.Count > 0 ? dtPARMCode.Select("CodeNo = 'Security'")[0].ItemArray[0].ToString() : "";

                #endregion

                for (int i = 0; i < dtPDFList.Rows.Count; i++)
                {
                    string strPDFReturn = "";

                    dtPDFList.Rows[i]["Address"] = Address;
                    dtPDFList.Rows[i]["ButtomLine"] = ButtomLine;
                    dtPDFList.Rows[i]["ButtomLine2"] = ButtomLine2;
                    dtPDFList.Rows[i]["Speed"] = Speed;
                    dtPDFList.Rows[i]["Security"] = Security;
                    dtPDFList.Rows[i]["Subject"] = dtSendSettingRef.Rows[0]["Subject"].ToString();
                    dtPDFList.Rows[i]["Description"] = dtSendSettingRef.Rows[0]["Description"].ToString();

                    // 案件狀態為成功，PDF需要產生第一頁
                    if (dtPDFList.Rows[i]["Status"].ToString() == "03" || dtPDFList.Rows[i]["Status"].ToString() == "07")
                    {
                        // 查詢發文字號
                       string strSendNoNow = string.Empty;

                       if (string.IsNullOrEmpty(dtPDFList.Rows[i]["MessageNo"].ToString()))
                       {
                          // 首次取號
                          strSendNoNow = QuerySendNoNow(dtPDFList.Rows[i]["NewID"].ToString());
                       }
                       else
                       {
                          // 原案件編號重查，使用原回文文號，不重新取號
                          strSendNoNow = dtPDFList.Rows[i]["MessageNo"].ToString();
                       }

                        if (strSendNoNow != "")
                        {
                            strPDFReturn = _ExportReportPDF.SavePDF(dtPDFList.Rows[i], "Y", strSendNoNow);
                        }
                        else
                        {
                            // 將狀態更新為沒有發文字號
                            UpdatePDFStatus(dtPDFList.Rows[i]["NewID"].ToString(), "W", "77");
                        }
                    }
                    else
                    {
                        // 案件狀態不為成功，PDF不產生第一頁
                        strPDFReturn = _ExportReportPDF.SavePDF(dtPDFList.Rows[i], "N", "");
                    }

                    // 主檔的狀態為成功代表整個案件下所有人都發查成功，則更新PDF匯出狀態，下次不再產出PDF
                    if ((dtPDFList.Rows[i]["Status"].ToString() == "03" || dtPDFList.Rows[i]["Status"].ToString() == "07"
                        || dtPDFList.Rows[i]["Status"].ToString() == "66") && strPDFReturn == "Y")
                    {
                        // 將案件PDF產出狀態更新成Y
                        UpdatePDFStatus(dtPDFList.Rows[i]["NewID"].ToString(), "Y", "");
                    }
                }
            }
        }

        /// <summary>
        /// 查詢要列印PDF的案件編號
        /// </summary>
        /// <returns></returns>
        public DataTable GetQueryPDFList()
        {
            string sqlSelect = @"          
SELECT
                               CaseCust.CaseID as NewID
                              , CaseCust.Status
                              , CaseMaster.Govunit AS ComeFile -- '受文者'
                              , CaseMaster.CaseNo --   系統案件編號
                              -- , CaseMaster.ROpenFileName --附件1
                              -- , CaseMaster.RFileTransactionFileName --附件2
                              , LDAPEmployee.EmpID --承辦人ID
                              , LDAPEmployee.EmpName  --承辦人名字
                              , ISNULL(LDAPEmployee.TelNo, '') AS TelNo --電話
                              , ISNULL(LDAPEmployee.TelExt, '') AS TelExt --分機
                              , CaseMaster.Govunit--   來文機關
                              -- , CaseMaster.LetterDeptNo --   來文機關代碼
                              , CaseMaster.GovDate --   來文日期
                              , CaseMaster.GovNo --來文字號
                              , '' AS Address -- 地址
                              , '' AS ButtomLine 
                              , '' AS ButtomLine2
                              , CaseMaster.Speed AS Speed -- 速別
                              , '' AS Security -- 密等
                              , '' AS Subject -- 主旨，內容
                              , '' AS Description -- 主旨，內容
                              , CaseTrsQueryVersion.CustId + 
                               case 
	                              when  ISNULL(CaseTrsQueryVersion.countID, 0) > 0 then '等'
                              else  '' 
                              end CustIdNo  --S12XXXXX49等
                              , '' AS Remark -- 1061106 高院偵字第10637396號
                              , '' --版本號
                              , CaseMaster.AgentUser AS InCharge -- '承辦人員'
                              , '' AS MessageNo -- '原回文文號' 
                              FROM 
                              (
	                              SELECT 
		                              CaseMaster.CaseID, CaseMaster.Status 
	                              FROM CaseTrsQueryVersion 
	                              INNER JOIN CaseMaster
		                              ON CaseTrsQueryVersion.CaseTrsNewID = CaseMaster.CaseID
	                              WHERE (CaseMaster.HistoryStatus IS NULL OR CaseMaster.HistoryStatus = 'N') 
		                              AND CaseTrsQueryVersion.Status <> '01' 
		                              AND CaseTrsQueryVersion.Status <> '02' 
		                              AND CaseTrsQueryVersion.Status <> '06'
	                              GROUP BY CaseMaster.CaseID, CaseMaster.Status
                              ) CaseCust
                              INNER JOIN CaseMaster
	                              ON CaseCust.CaseID = CaseMaster.CaseID
                              LEFT JOIN LDAPEmployee
	                              ON CaseMaster.AgentUser = LDAPEmployee.EmpID
                              LEFT JOIN 
                              (
	                              SELECT CaseTrsNewID,  count(CaseTrsNewID) AS countID, MAX(CustId) AS CustId FROM CaseTrsQueryVersion 
	                              GROUP BY CaseTrsNewID
                              ) CaseTrsQueryVersion
                              ON CaseMaster.CaseID = CaseTrsQueryVersion.CaseTrsNewID
                              ORDER BY CaseMaster.DocNo --,CaseMaster.Version
                            ";

            // 清空容器
            base.Parameter.Clear();
            return base.Search(sqlSelect);
        }

 

        public DataTable GetPARMCode()
        {
            string sqlSelect = @" 
                                SELECT CodeDesc,CodeNo FROM PARMCode WHERE CodeType = 'REPORT_SETTING' 
                            ";

            // 清空容器
            base.Parameter.Clear();
            return base.Search(sqlSelect);
        }

        public DataTable Get401Currency(string VersionNewID ,string Acct_no)
        {
            string sqlSelect = @" 
select * from BOPS000401Recv where VersionNewid = '"+ VersionNewID+"'  and acct_no =  '"+Acct_no+"' ";
            // 清空容器
            base.Parameter.Clear();
            return base.Search(sqlSelect);
        }

        public DataTable GetSendSettingRef()
        {
            string sqlSelect = @" 
                                SELECT Subject, Description FROM SendSettingRef WHERE CaseKind = '外來文案件' AND CaseKind2 = '外來文回文'
                            ";

            // 清空容器
            base.Parameter.Clear();
            return base.Search(sqlSelect);
        }

        /// <summary>
        /// 將PDF列印狀態更新到系統中
        /// </summary>
        /// <param name="VersionNewID"></param>
        /// <param name="strPDFStatus">W: 沒有回文編號</param>
        /// <param name="strStatus">77：未獲取回文字號</param>
        /// <returns></returns>
        public int UpdatePDFStatus(string VersionNewID, string strPDFStatus, string strStatus)
        {
            string sql = @" Update CaseMaster 
                         set PDFStatus = @PDFStatus";

            if (strStatus != "")
            {
                sql += @" ,Status = '77' ";
            }

            sql += @" where NewID = @NewID ";

            // 清空容器
            base.Parameter.Clear();
            base.Parameter.Add(new CommandParameter("@NewID", VersionNewID));
            base.Parameter.Add(new CommandParameter("@PDFStatus", strPDFStatus));

            return base.ExecuteNonQuery(sql);
        }

        public string QuerySendNoNow(string strNewID)
        {
            string sqlSelect = @" 
                                DECLARE @SendNoId BIGINT;
                                DECLARE @SendNoNow BIGINT
                                SELECT @SendNoId = SendNoId, @SendNoNow = SendNoNow + 1 FROM SendNoTable WHERE SendNoYear = left(convert(varchar,getdate(),21),4)
                                AND SendNoNow < SendNoEnd
                                ORDER BY SendNoId ASC
                                UPDATE SendNoTable SET SendNoNow = @SendNoNow WHERE SendNoId = @SendNoId
                                SELECT @SendNoNow    as SendNoNow
                              ";

            DataTable dt = base.Search(sqlSelect);

            if (dt != null && dt.Rows.Count > 0)
            {
                // 20180326,PeteHsieh : 規格修改成，將取得的文號中[單位代碼]移除(4~9碼)
                Regex regex = new Regex(Regex.Escape("24839"));
                string NewSendNo = regex.Replace(dt.Rows[0]["SendNoNow"].ToString(), string.Empty, 1);

                string sql = @"UPDATE CaseMaster SET MessageNo = @SendNoNow, MessageDate = GETDATE() WHERE NewID = @NewID";

                // 清空容器
                base.Parameter.Clear();

                base.Parameter.Add(new CommandParameter("@SendNoNow", NewSendNo));
                base.Parameter.Add(new CommandParameter("@NewID", strNewID));

                base.ExecuteNonQuery(sql);

                return NewSendNo;
            }
            else
            {
                return "";
            }

        }

        #endregion
    }
}