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
//using CTBC.CSFS.Service;
using System.Configuration;
using System.Net;
using CTBC.FrameWork.Util;
using System.IO;
using System.Collections;
using System.Xml;
using System.Text.RegularExpressions;
using System.Data;
using Microsoft.Reporting.WinForms;
using iTextSharp.text.pdf;
using iTextSharp.text;
using ICSharpCode.SharpZipLib.Zip;
using System.Globalization;



namespace CTBC.WinExe.ImportDeath
{
    class Program
    {
        private static int systime;
        //private static FileLog m_fileLog;
        private static int timeSection1;
        private static int timeSection2;
        private static int timeSection3;
        private static ImportEDocBiz _ImportEDocBiz;
        private static string ftpserver;
        private static string port;
        private static string username;
        private static string password;
        private static string ftpdir;
        private static string localFilePath;
        private static FtpClient ftpClient;
        private static string[] fileTypes;
        private static bool isFtp;

        private static string brprefix;
        private static string in10prefix;
        private static string in11prefix;

        private static ImportDeathBiz idb;

        static Program()
        {
            systime = Convert.ToInt32(DateTime.Now.Hour.ToString().PadLeft(2, '0') + DateTime.Now.Minute.ToString().PadLeft(2, '0'));
            //m_fileLog = new FileLog(ConfigurationManager.AppSettings["filelog"], AppDomain.CurrentDomain.FriendlyName.Replace(".vshost", "").Replace(".exe", ""));
            log4net.Config.XmlConfigurator.Configure(new System.IO.FileInfo(AppDomain.CurrentDomain.BaseDirectory + @"\Log.config"));

            _ImportEDocBiz = new ImportEDocBiz();
            ftpserver = ConfigurationManager.AppSettings["ftpserver"];
            port = ConfigurationManager.AppSettings["port"];
            username = ConfigurationManager.AppSettings["username"];
            password = ConfigurationManager.AppSettings["password"];

            brprefix = ConfigurationManager.AppSettings["prefix_br"];
            in10prefix = ConfigurationManager.AppSettings["prefix_in10"];
            in11prefix = ConfigurationManager.AppSettings["prefix_in11"];

            ftpdir = ConfigurationManager.AppSettings["ftpdir"];
            localFilePath = ConfigurationManager.AppSettings["loaclFilePath"];
            ftpClient = new FtpClient(ftpserver, username, password, port);
            isFtp = bool.Parse(ConfigurationManager.AppSettings["isFtp"].ToString());


            idb = new ImportDeathBiz();
        }


        static void Main(string[] args)
        {
            try
            {

                idb.WriteLog("信息-------- 被繼承人收檔作業開始----------------");
                ImportDeath(args);
                idb.WriteLog("信息--------被繼承人收檔作業結束----------------");

                
            }
            catch (Exception ex)
            {
                idb.WriteLog("信息--------被繼承人收檔作業失敗，失敗原因：" + ex.Message + "----------------");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="timesection"></param>
        private static void ImportDeath(string[] args)
        {
            // CaseDeadVersion , SendStatus = 0 , 只是上傳, 1: 畫面上確定要查主機了, 2: 成功 , 3: 失敗



            DateTime thenow = DateTime.Now;
            // ************************************************************************************************************************
            // 以下這一行記得拿掉....thenow = new DateTime(2020, 11, 19);
            // 以下這一行記得拿掉....thenow = new DateTime(2020, 11, 19);
            // 以下這一行記得拿掉....thenow = new DateTime(2020, 11, 19);
            // 以下這一行記得拿掉....thenow = new DateTime(2020, 11, 19);
            // 以下這一行記得拿掉....thenow = new DateTime(2020, 11, 19);
            // 以下這一行記得拿掉....thenow = new DateTime(2020, 11, 19);
            // 以下這一行記得拿掉....thenow = new DateTime(2020, 11, 19);
            // ************************************************************************************************************************
            
            if( args.Count() >0)
                thenow = new DateTime(2020, 12, 31);


            //20201120, 要避開營業日
            bool? isWorkDay = idb.getWorkDay(thenow); // 應該要讀取前一天的工作日才對
            if( isWorkDay==null)
            {
                idb.WriteLog(string.Format("無法判定是否為工作日{0}", thenow.ToShortDateString()));
            }
            else
            {
                if(! (bool)isWorkDay) //假日
                {
                    idb.WriteLog(string.Format("非工作日, 直接離開 {0} ", thenow.ToShortDateString()));
                    return;
                }
            }

            DateTime lastWorkDay = idb.getLastWorkDay(thenow);

            // Step 1, 取出SendStatus=1 或9 (只產檔, 不FTP)
            idb.WriteLog(string.Format("開始取出案件, 前一工作日{0}", lastWorkDay.ToString("yyyy-MM-dd")));

            var caseDeath = idb.getLastCaseDeadVersion(lastWorkDay); // 取出當天上傳的最後一筆

            if( caseDeath==null)
            {
                idb.WriteLog(string.Format("目前無案件"));
                return;
            }
            idb.WriteLog(string.Format("\t取出案件{0}", caseDeath.DocNo));


            idb.updateStatus(caseDeath, "1"); // 表示畫面中, 顯示處理中.../
            //if (caseDeath.SendStatus == "1") // 狀態SendStatus=1, 是要先FTP,再產PDF .... 若是SendStatus=9, 直接產PDF
            {
                idb.WriteLog(string.Format("\t開始讀取FTP"));             
                List<string> fileNames = getFilesFromFTP(new string[] { brprefix, in10prefix, in11prefix }, caseDeath.ModifiedDate.ToString("yyyMMdd"));
                idb.WriteLog(string.Format("\t共取得 {0} 個檔案 ", fileNames.Count()));
            }


            //20201119, 先檢查, 是否長度是規定的長度..若沒有資料, 會給空檔
            bool isBr = idb.checkFileLength(caseDeath, caseDeath.ModifiedDate, localFilePath, brprefix, 172);
            bool is10 = idb.checkFileLength(caseDeath, caseDeath.ModifiedDate, localFilePath, in10prefix, 167);
            bool is11 = idb.checkFileLength(caseDeath, caseDeath.ModifiedDate, localFilePath, in11prefix, 198);

            if(!( isBr && is10 && is11)) // 若有三個檔案中, 有任何一個ID的長度不一致, 則丟出錯誤, 或沒有檔案....
            {
                idb.updateStatus(caseDeath, "3","檔案長度有問題, 或是沒有檔案"); // 表示表示失敗
                idb.WriteLog(string.Format("\t檔案長度有問題 !!! "));
                idb.setCaseDeadVersionStatus(caseDeath, "03");
                return;
            }

            //if ( (caseDeath.SendStatus == "1"))  // 一律重新從FTP抓下來....
            //{
                // Step 2 , 由File目錄, 開始找BR0966_yyyyMMdd (放款), INHIS11_yyyyMMdd (投資), INHIS10_yyyyMMdd (存款) 
                //idb.deleteBR(caseDeath.DocNo); // 刪除原先有的DocNo
                bool brResult = idb.importBR(caseDeath, caseDeath.ModifiedDate, localFilePath, brprefix);
                //idb.deleteIn10(caseDeath.DocNo); // 刪除原先有的DocNo
                bool in10Result = idb.importIn10(caseDeath, caseDeath.ModifiedDate, localFilePath, in10prefix);
                //idb.deleteIn11(caseDeath.DocNo); // 刪除原先有的DocNo
                bool in11Result = idb.importIn11(caseDeath, caseDeath.ModifiedDate, localFilePath, in11prefix);
                idb.WriteLog(string.Format("\t匯入完成, 案件編號{0}", caseDeath.DocNo));

                bool allImportSuccess = brResult & in10Result & in11Result;
                if (!allImportSuccess)
                {
                    idb.WriteLog(string.Format("匯入檔案, 發生錯誤...日期 {0}", caseDeath.ModifiedDate.ToString("yyyyMMdd")));
                    idb.updateStatus(caseDeath, "3", "匯入主機檔案進資料表失敗"); // 表示表示失敗
                    idb.setCaseDeadVersionStatus(caseDeath, "03");
                    return;
                }
            //}

            bool bSuccess = true;

            
            string ouputPDF =  localFilePath + "\\" + caseDeath.DocNo + ".pdf";
            string outputZIP = localFilePath + "\\" + caseDeath.DocNo + ".zip";
            try
            {
                // Step 3, 開始產出報表.. .用RDLC
                idb.WriteLog(string.Format("\t準備產生PDF檔"));
                CreatePdf(caseDeath.DocNo, ouputPDF);
                idb.WriteLog(string.Format("\t產生PDF檔: {0}完成", ouputPDF));
            }
            catch (Exception ex)
            {                
                idb.updateStatus(caseDeath, "3","產生PDF失敗"); // 表示表示失敗
                idb.WriteLog(string.Format("\t產生PDF檔: 失敗 {0} ", ex.Message.ToString()));
                idb.setCaseDeadVersionStatus(caseDeath, "03");
                return;
            }

            // Step 3.5, 壓縮成ZIP
            try
            {
                if (System.IO.File.Exists(outputZIP))
                    System.IO.File.Delete(outputZIP);
                ICSharpCode.SharpZipLib.Zip.ZipFile zipFile = ICSharpCode.SharpZipLib.Zip.ZipFile.Create(outputZIP);
                zipFile.NameTransform = new ZipNameTransform(localFilePath);
                zipFile.BeginUpdate();
                zipFile.Add(ouputPDF);
                zipFile.CommitUpdate();                
                zipFile.Close();
                zipFile = null;
                GC.Collect();
                GC.WaitForPendingFinalizers();
                
            }
            catch (Exception ex)
            {
                idb.updateStatus(caseDeath, "3", "產生ZIP失敗"); // 表示表示失敗
                idb.WriteLog(string.Format("\t產生ZIP檔: 失敗 {0} ", ex.Message.ToString()));
                idb.setCaseDeadVersionStatus(caseDeath, "03");
                return;
            }

            // Step 4, 插入至CaseEDocFfile
            try
            {
                idb.WriteLog(string.Format("\t準備插入CaseEdocFile "));
                var insertResult = idb.importCaseEdocFile(caseDeath, outputZIP, caseDeath.DocNo);
                idb.WriteLog(string.Format("\t插入CaseEdocFile完成"));

                // Step 5, 押回成功
                idb.WriteLog(string.Format("\t準備押回狀態碼 "));
                if (insertResult == "0")
                {
                    idb.updateStatus(caseDeath, "2"); // 表示成功
                    idb.setCaseDeadVersionStatus(caseDeath, "02");
                }
                else
                {
                    idb.updateStatus(caseDeath, "3", "新增檔案至資料表失敗"); // 表示表示失敗
                    idb.setCaseDeadVersionStatus(caseDeath, "03");
                }               


                idb.WriteLog(string.Format("\t押回狀態碼完成 "));
            }
            catch (Exception ex)
            {                
                idb.updateStatus(caseDeath, "3", "新增檔案至資料表失敗"); // 表示表示失敗
                idb.WriteLog(string.Format("\t插入CaseEdocFile : 失敗 {0} ", ex.Message.ToString()));
                idb.setCaseDeadVersionStatus(caseDeath, "03");
                return;
            }

            GC.Collect();
            GC.WaitForPendingFinalizers();
            // Step 5.5 刪除PDF, ZIP 檔
            try
            {

                //20201228, 要保留7天內的主機檔案....

                FileInfo[] files = new DirectoryInfo(localFilePath).GetFiles("*.txt", SearchOption.TopDirectoryOnly);
                List<string> filesname2del = new List<string>();
                foreach(var f in files)
                {
                    if( f.CreationTime < DateTime.Now.AddDays(-7))
                    {
                        f.Delete();
                        filesname2del.Add(f.Name);
                    }
                }
                idb.WriteLog(string.Format("\t刪除7天前的檔案 : {0} ", string.Join(",",filesname2del)));

                //var fn = Path.Combine(localFilePath, brprefix + lastWorkDay.ToString("yyyyMMdd") + ".txt");
                //System.IO.File.Delete(fn);
                //fn = Path.Combine(localFilePath, in10prefix + lastWorkDay.ToString("yyyyMMdd") + ".txt");
                //System.IO.File.Delete(fn);
                //fn = Path.Combine(localFilePath, in11prefix + lastWorkDay.ToString("yyyyMMdd") + ".txt");
                //System.IO.File.Delete(fn);

                
                // 20201225, ZIP檔跟PDF, 當天就砍掉
                System.IO.File.Delete(ouputPDF);
                //System.IO.File.Move(outputZIP, outputZIP + "temp");                
                System.IO.File.Delete(outputZIP);
            }
            catch (Exception ex)
            {
                idb.WriteLog(string.Format("\t刪除檔案失敗 : {0} ", ex.Message.ToString()));
                return;
            }



            // Step 6. 完成

            //20201218, 押回CaseDeadVersion. 存款, 放款, 理財, 有無存款及保險箱 欄位
            idb.WriteLog(string.Format("\t回寫  CaseDeadVersion. 存款, 放款, 理財, 有無存款及保險箱  "));
            idb.updateCaseDeadVersion(caseDeath.DocNo);
            
            idb.WriteLog("");
            idb.WriteLog(string.Format("案件處理完成 "));
        }


        private static void CreatePdf(string DocNo, string strFileName)
        {

            var _deadDeposit = idb.getDeathDeposit(DocNo);
            var _deadInvest = idb.getDeathInvest(DocNo);
            var _deadLoan = idb.getDeathLoan(DocNo);

            // 找出所有的ID...得到DeathViewModel
            List<DeathViewModel> DeathInfo = idb.getUnionInfo(DocNo, _deadDeposit, _deadInvest, _deadLoan);

            SavePDFByLocalReport("DeadReports.rdlc", strFileName, DeathInfo);


        }


        public static void SavePDFByLocalReport(string pReportName, string pFileName, List<DeathViewModel> DataSource, List<ReportParameter> listParm = null)
        {
            try
            {
                int i = 1;
                List<byte[]> result = new List<byte[]>();
                foreach(var dvm in DataSource)
                {
                    try
                    {
                        #region MyRegion 產生單頁的PDF
                        LocalReport localReport = new LocalReport { ReportPath = AppDomain.CurrentDomain.BaseDirectory + @"Report1.rdlc" };
                        DataTable main = CreateMain(DataSource.Where(x => x.IDNO == dvm.IDNO).ToList());
                        //DataTable dt = CreateMain(DataSource);
                        ReportDataSource rds = new ReportDataSource() { Name = "DataSet1", Value = main };
                        ReportDataSource rdsDeposit = new ReportDataSource() { Name = "DataSet2", Value = dvm.Deposits };
                        ReportDataSource rdsInvest = new ReportDataSource() { Name = "DataSet3", Value = dvm.Invests };
                        ReportDataSource rdsLoan = new ReportDataSource() { Name = "DataSet4", Value = dvm.Loans };
                        localReport.DataSources.Add(rds);
                        localReport.DataSources.Add(rdsDeposit);
                        localReport.DataSources.Add(rdsInvest);
                        localReport.DataSources.Add(rdsLoan);

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
                        #endregion
                        result.Add(renderedBytes);
                    }
                    catch (Exception ex1)
                    {
                        idb.WriteLog(string.Format("產生 PDF文件報錯: {0}  {1}", dvm.IDNO, ex1.Message));
                    }

                }

                byte[] allResult = mergePdfs(result);


                using (FileStream fs = new FileStream(pFileName, FileMode.Create, FileAccess.Write))
                {
                    fs.Write(allResult, 0, allResult.Length);
                }  

            }
            catch (Exception ex)
            {
                idb.WriteLog("LocalReport 產生 PDF文件報錯: " + ex.Message);
                //throw ex;
            }
        }

        internal static byte[] mergePdfs(List<byte[]> pdfs)
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

        private static List<string> getFilesFromFTP(string[] prefix, string modifydate)
        {
            if (!Directory.Exists(localFilePath))
                Directory.CreateDirectory(localFilePath);

            List<string> fileNames = new List<string>();

            if (isFtp)
            {
                ArrayList fileList = new ArrayList();
                //獲取FTP文件清單
                idb.WriteLog("信息--------正在獲取FTP文件清單----------------");
                try
                {
                    fileList = ftpClient.GetFileList(ftpdir);
                    //下載FTP指定目錄下的所有文件
                    foreach (var file in fileList)
                    {
                        if (file.ToString().EndsWith("_" + modifydate + ".txt"))
                        {
                            // 若是開頭是BR09666_, IN10.. IN11 才要抓下來
                            if (file.ToString().ToUpper().StartsWith(prefix[0]) || file.ToString().ToUpper().StartsWith(prefix[1]) || file.ToString().ToUpper().StartsWith(prefix[2]))
                            {
                                string remoteFile = ftpClient.SetRemotePath(ftpdir) + "//" + file;
                                string localFile = localFilePath.TrimEnd('\\') + "\\" + file;
                                ftpClient.GetFiles(remoteFile, localFile);
                                fileNames.Add(localFile);
                            }
                        }
                    }
                }
                catch (Exception ex1)
                {
                    idb.WriteLog("錯誤--------取得FTP檔案錯誤----------------" + ex1.Message.ToString());
                }
                idb.WriteLog("信息--------獲取FTP文件清單結束----------------");

                // 刪除FTP

                try
                {
                    //ftpClient.DeleteFiles(ftpdir, (string[])fileList.ToArray(typeof(string)));
                    // 只刪除有抓下來的檔案
                    List<string> delFile = new List<string>();
                    fileNames.ForEach(x => delFile.Add( x.Replace(localFilePath, "")));
                    ftpClient.DeleteFiles(ftpdir, delFile.ToArray());
                    idb.WriteLog("錯誤--------刪除FTP成功----------------");
                }
                catch (Exception ex2)
                {

                    idb.WriteLog("錯誤--------刪除FTP錯誤----------------" + ex2.Message.ToString());
                }
            }
            else
            {
                var fileNamesTemp = Directory.GetFiles(localFilePath, "*.txt").ToList();                
                fileNamesTemp.ForEach(f => fileNames.Add(f));
            }

            return fileNames;
        }


        private static DataTable CreateMain(List<DeathViewModel> dv)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("IDNO", typeof(string));
            dt.Columns.Add("Name", typeof(string));
            dt.Columns.Add("BaseDate", typeof(string));
            dt.Columns.Add("SBox_Flag", typeof(string));
            foreach(var d in dv)
            {
                DataRow dr = dt.NewRow();
                dr["IDNO"] = d.IDNO;
                dr["Name"] = d.Name;
                dr["BaseDate"] = d.BaseDate;
                dr["SBox_Flag"] = d.SBox_Flag;
                dt.Rows.Add(dr);
            }

            return dt;
        }
    }
}
