using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CTBC.CSFS.Pattern;
using CTBC.CSFS.BussinessLogic;
using CTBC.FrameWork.Util;
using System.Configuration;
using System.IO;
using System.Collections;
using System.Data;
using System.Text.RegularExpressions;
namespace CTBC.WinExe.CSFS.resolvingESB
{
    class Program : CommonBIZ
    {
        #region ftp有關設定&本地路徑

        private static FileLog m_fileLog;
        private static string ftpserver;
        private static string port;
        private static string username;
        private static string password;
        private static string ftpdir;
        private static string localFilePath;
        private static FtpClient ftpClient;

        #endregion
        public Program()
        {
            m_fileLog = new FileLog(ConfigurationManager.AppSettings["filelog"], AppDomain.CurrentDomain.FriendlyName.Replace(".vshost", "").Replace(".exe", ""));

            #region 設定

            ftpserver = ConfigurationManager.AppSettings["ftpserver"];
            port = ConfigurationManager.AppSettings["port"];
            username = ConfigurationManager.AppSettings["username"];
            password = ConfigurationManager.AppSettings["password"];


            ftpdir = ConfigurationManager.AppSettings["ftpdir"];
            localFilePath = AppDomain.CurrentDomain.BaseDirectory + ConfigurationManager.AppSettings["localFilePath"];
            ftpClient = new FtpClient(ftpserver, username, password, port);

            #endregion
        }
        static void Main(string[] args)
        {
            Program mainProgram = new Program();
            mainProgram.Process();
        }
        /// <summary>
        /// 主程式
        /// </summary>
        private void Process()
        {
            // 從FTP上取得檔案&匯入DB,逐個文件處理
            GetFilesFromFTP();
        }
        /// <summary>
        /// 從FTP上取得檔案&匯入DB,逐個文件處理
        /// </summary>
        public void GetFilesFromFTP()
        {
            try
            {
                // log 記錄
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息-------- 從MFTP下載檔案作業開始----------------");

                // 取得需要匯入DB的文件清單
                IList<WarningQueryHistory> listWarningQueryHistory = GetWarningQueryHistory();

                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "檔案Count:" + listWarningQueryHistory.Count().ToString() + "----------------");

                // 本地路徑
                if (!Directory.Exists(localFilePath))
                {
                    Directory.CreateDirectory(localFilePath);
                }
                // 逐個下載&匯入DB
                foreach (WarningQueryHistory item in listWarningQueryHistory)
                {
                    #region bat 方式下載
                    /*

                        string uploadpath = localFilePath + "download.txt";

                        using (StreamWriter sr = new StreamWriter(File.Open(uploadpath, FileMode.Create), System.Text.Encoding.Default))
                        {
                            sr.WriteLine("open " + ftpserver);
                            sr.WriteLine(username);
                            sr.WriteLine(password);
                            sr.WriteLine("ascii");
                            sr.WriteLine("cd " + ftpdir);
                            sr.WriteLine("lcd " + localFilePath);
                            sr.WriteLine("get " + item.FileName + " " + localFilePath + item.FileName);
                            sr.WriteLine("quit");
                            sr.Close();
                        }

                        // BAT檔案路徑
                        string batpath = localFilePath + "download.bat";
                        using (StreamWriter src = new StreamWriter(File.Open(batpath, FileMode.Create), System.Text.Encoding.Default))
                        {
                            src.WriteLine("@echo off");
                            src.WriteLine("ftp -s:" + uploadpath);

                            src.WriteLine("del %0");
                        }

                        // 执行bat文件
                        Process process = new Process();

                        process.StartInfo.FileName = batpath;

                        //不显示闪烁窗口
                        process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;

                        process.Start();

                        process.WaitForExit();

                        if (File.Exists(uploadpath))
                        {
                            File.Delete(uploadpath);
                        }

                     */
                    #endregion

                    #region - Peter, 20171123
                    // 取得FTP指定目錄下的所有文件名稱
                    ArrayList fileList = ftpClient.GetFileList(ftpdir);

                    // 下載FTP指定目錄下的所有文件
                    foreach (var file in fileList)
                    {
                        if (item.FileName.Contains(file.ToString()))
                        {
                            //  ftp 文件
                            string remoteFile = ftpClient.SetRemotePath(ftpdir) + "//" + file;

                            // 本地文件
                            string localFile = localFilePath.TrimEnd('\\') + "\\" + file;

                            // 若已經存在，則先刪除掉舊的文件
                            if (File.Exists(localFile))
                            {
                                File.Delete(localFile);
                            }

                            ftpClient.GetFiles(remoteFile, localFile);

                            m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------從MFTP下載檔案" + file + "----------------");

                            ftpClient.DeleteFile(remoteFile);

                            m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------刪除FTP文件" + file + "----------------");

                            break;
                        }
                    }
                    #endregion

                    // 判斷是否成功下載
                    bool isExist = File.Exists(localFilePath + item.FileName);

                    FileInfo newfile = new FileInfo(localFilePath + item.FileName);

                    // 檔案的新增日期
                    string fileDt = newfile.LastWriteTime.ToString("yyyyMMdd");

                    // 已在本地存在,且新增日期為當下
                    if (isExist && fileDt == DateTime.Now.ToString("yyyyMMdd"))
                    {
                        // log 記錄
                        m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息-------- 從MFTP成功下載檔案:" + item.FileName + "----------------");

                        // 匯入DB
                        bool isSucess = InsertToDB(localFilePath, item);

                        // 成功匯入後，刪掉FTP上的檔案
                        if (isSucess)
                        {
                            // 刪除FTP上的檔案 -- 12/19,PeterHsieh,MFTP會自己被刪除，待確認原因
                            //ftpClient.DeleteFile(ftpdir + "/" + item.FileName);

                            // log 記錄
                            //m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息-------- 從MFTP成功刪除檔案:" + item.FileName + "----------------");

                            UpdateWarningQueryHistory(item.TrnNum,"02");
                        }
                    }
                    else
                    {
                        // log 記錄
                        m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息-------- 從MFTP下載檔案:" + item.FileName + "失敗----------------");
                    }
                }
            }
            catch (Exception ex)
            {
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息-------- 程式錯誤，錯誤原因：" + ex.Message + "----------------");
            }
        }

        /// <summary>
        /// 讀取檔案到FTP
        /// </summary>
        /// <param name="localFilePath">本地路徑</param>
        /// <param name="item">來文檔案查詢條件</param>
        /// <returns></returns>
        public bool InsertToDB(string localFilePath, WarningQueryHistory item)
        {
            // 取得連接并開放連接
            IDbConnection dbConnection = base.OpenConnection();

            // 定義事務
            IDbTransaction dbTransaction = null;

            using (dbConnection)
            {
                // 開啟事務
                dbTransaction = dbConnection.BeginTransaction();

                try
                {
                    FileStream fs = new FileStream(localFilePath + item.FileName, FileMode.Open);
                    StreamReader reader = new StreamReader(fs, Encoding.UTF8);

                    #region 讀取文件，寫入DB

                    string strLine = "";

                    // 不是文件尾
                    while (!reader.EndOfStream)
                    {
                        // 讀取一行
                        strLine = reader.ReadLine();

                        if (strLine != "")
                        {
                            // 欄位列之間用||隔開
                            string[] columns = Regex.Split(strLine, @"\|\|", RegexOptions.IgnoreCase);
                            string strYN = "";
                            if  ( columns[1].Trim()== columns[15].Trim() )
                            {
                                strYN = "Y";
                            }
                            else
                            {
                                strYN = "N";
                            }
                            #region insert SQL

                            string sqlInsert = @"INSERT INTO TransactionDetail
                                                            ( 
                                                              TrnNum ,
                                                              DATA_DATE ,
                                                              ACCT_NO ,
                                                              JNRST_DATE ,
                                                              JNRST_TIME ,
                                                              JNRST_TIME_SEQ ,
                                                              TRAN_DATE ,
                                                              POST_DATE ,
                                                              TRANS_CODE ,
                                                              JRNL_NO ,
                                                              REVERSE ,
                                                              PROMO_CODE ,
                                                              REMARK ,
                                                              TRAN_AMT ,
                                                              BALANCE ,
                                                              TRF_BANK ,
                                                              TRF_ACCT ,
                                                              NARRATIVE ,
                                                              FISC_BANK ,
                                                              FISC_SEQNO ,
                                                              CHQ_NO ,
                                                              ATM_NO ,
                                                              TRAN_BRANCH ,
                                                              TELLER ,
                                                              FILLER ,
                                                              TXN_DESC ,
                                                              ACCT_P2 ,
                                                              FILE_NAME ,
                                                              TYPE
                                                            )  VALUES  ( '" + item.TrnNum + @"' , 
                                                              '" + columns[0].Trim() + @"' , 
                                                              '" + columns[1].Trim() + @"' , 
                                                              '" + columns[2].Trim() + @"' , 
                                                              '" + columns[3].Trim() + @"' , 
                                                              '" + columns[4].Trim() + @"' , 
                                                              '" + columns[5].Trim() + @"' , 
                                                              '" + columns[6].Trim() + @"' , 
                                                              '" + columns[7].Trim() + @"' , 
                                                              '" + columns[8].Trim() + @"' , 
                                                              '" + columns[9].Trim() + @"' ,
                                                              '" + columns[10].Trim() + @"' , 
                                                              '" + columns[11].Trim() + @"' , 
                                                              '" + columns[12].Trim() + @"' , 
                                                              '" + columns[13].Trim() + @"' , 
                                                              '" + columns[14].Trim() + @"' ,
                                                              '" + columns[15].Trim() + @"' , 
                                                              '" + columns[16].Trim() + @"' , 
                                                              '" + columns[17].Trim() + @"' , 
                                                              '" + columns[18].Trim() + @"' , 
                                                              '" + columns[19].Trim() + @"' , 
                                                              '" + columns[20].Trim() + @"' , 
                                                              '" + columns[21].Trim() + @"' , 
                                                              '" + columns[22].Trim() + @"' ,
                                                              '" + columns[23].Trim() + @"' , 
                                                              '" + columns[24].Trim() + @"' ,
                                                              '" + columns[25].Trim() + @"', 
                                                              '" + columns[26].Trim() + @"' , 
                                                              '" + strYN + @"')  ";

                                                             // '" + columns[0].Trim() + @"' , -- DATA_DATE - date
                                                             // '" + columns[1].Trim() + @"' , -- ACCT_NO - varchar(16)
                                                             // '" + columns[2].Trim() + @"' , -- JNRST_DATE - date
                                                             // '" + columns[3].Trim() + @"' , -- JNRST_TIME - varchar(8)
                                                             // '" + columns[4].Trim() + @"' , -- JNRST_TIME_SEQ - varchar(2)
                                                             // '" + columns[5].Trim() + @"' , -- TRAN_DATE - date
                                                             // '" + columns[6].Trim() + @"' , -- POST_DATE - date
                                                             // '" + columns[7].Trim() + @"' , -- TRANS_CODE - varchar(5)
                                                             // '" + columns[8].Trim() + @"' , -- JRNL_NO - varchar(7)
                                                             // '" + columns[9].Trim() + @"' , -- REVERSE - char(1)
                                                             // '" + columns[10].Trim() + @"' , -- PROMO_CODE - varchar(2)
                                                             // '" + columns[11].Trim() + @"' , -- REMARK - nvarchar(20)
                                                             // '" + columns[12].Trim() + @"' , -- TRAN_AMT - decimal
                                                             // '" + columns[13].Trim() + @"' , -- BALANCE - decimal
                                                             // '" + columns[14].Trim() + @"' , -- TRF_BANK - varchar(3)
                                                             // '" + columns[15].Trim() + @"' , -- TRF_ACCT - varchar(16)
                                                             // '" + columns[16].Trim() + @"' , -- NARRATIVE - nvarchar(20)
                                                             // '" + columns[17].Trim() + @"' , -- FISC_BANK - varchar(3)
                                                             // '" + columns[18].Trim() + @"' , -- FISC_SEQNO - varchar(8)
                                                             // '" + columns[19].Trim() + @"' , -- CHQ_NO - varchar(8)
                                                             // '" + columns[20].Trim() + @"' , -- ATM_NO - varchar(4)
                                                             // '" + columns[21].Trim() + @"' , -- TRAN_BRANCH - varchar(4)
                                                             // '" + columns[22].Trim() + @"' , -- TELLER - varchar(5)
                                                             // '" + columns[23].Trim() + @"' , -- FILLER - varchar(54)
                                                             // '" + columns[24].Trim() + @"' , -- TXN_DESC - nvarchar(8)
                                                             // '" + columns[25].Trim() + @"', -- ACCT_P2 - tinyint
                                                             // '" + columns[26].Trim() + @"' , -- FILE_NAME - varchar(20)
                                                             // '" + strYN + @"'                -- TYPE - varchar(1)
                                                             //)  ";

                            #endregion

                            base.Parameter.Clear();

                            base.ExecuteNonQuery(sqlInsert, dbTransaction);
                        }
                    }

                    dbTransaction.Commit();

                    #endregion

                    m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息-------- 檔案" + item.FileName + "匯入DB成功----------------");

                    return true;
                }
                catch (Exception ex)
                {
                    dbTransaction.Rollback();

                    m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息-------- 檔案" + item.FileName + "匯入DB失敗，失敗原因：" + ex.Message + "----------------");

                    return false;
                }
            }

        }

        /// <summary>
        /// 更新RFDM查詢狀態
        /// </summary>
        /// <param name="strTrnNum">交易序號</param>
        /// <param name="strVersionNewID">來文檔案查詢條件帳號檔主鍵</param>
        /// <param name="strRFDMSendStatus">RFDM查詢狀態</param>
        public void UpdateWarningQueryHistory(string strTrnNum,  string strESBStatus)
        {
            try
            {
                // 更新CaseCustRFDMSend
                string sqlUpdate = @"UPDATE [WarningQueryHistory] 
                                                        SET 
                                                            [ESBStatus] = @ESBStatus,
                                                            [ModifiedDate] = GETDATE(), 
                                                            [ModifiedUser] = 'SYSTEM' 
                                                        WHERE TrnNum = @TrnNum;";

                base.Parameter.Clear();

                base.Parameter.Add(new CommandParameter("@TrnNum", strTrnNum));
                base.Parameter.Add(new CommandParameter("@ESBStatus", strESBStatus));

                base.ExecuteNonQuery(sqlUpdate);
            }
            catch (Exception ex)
            {
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "錯誤訊息: " + ex.ToString());

            }
        }

        /// <summary>
        /// 取得需要吃檔的檔案名稱
        /// </summary>
        /// <returns></returns>
        public IList<WarningQueryHistory> GetWarningQueryHistory()
        {
            string sqlSelect = @" SELECT  FileName
                                            ,TrnNum
                                            ,NewID
                                    FROM    WarningQueryHistory
                                    WHERE   ESBStatus = '01'
                                            AND ISNULL(FileName, '') <> '' ";
            Parameter.Clear();

            return base.SearchList<WarningQueryHistory>(sqlSelect);
        }

        /// <summary>
        /// 來文檔案查詢條件
        /// </summary>
        public class WarningQueryHistory
        {
            /// <summary>
            /// 交易序號
            /// </summary>
            public string TrnNum { get; set; }

            /// <summary>
            /// 來文檔案查詢條件帳號檔主鍵
            /// </summary>
            public string NewID { get; set; }

            /// <summary>
            /// 檔案名稱
            /// </summary>
            public string FileName { get; set; }
        }
    }
}
