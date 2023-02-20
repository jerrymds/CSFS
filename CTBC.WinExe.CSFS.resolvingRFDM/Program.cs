using CTBC.CSFS.BussinessLogic;
using CTBC.CSFS.Pattern;
using CTBC.FrameWork.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CTBC.WinExe.CSFS.resolvingRFDM
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
            // log 
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

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        private static void Main()
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
                IList<CaseCustRFDMSend> listCaseCustRFDMSend = GetCaseCustRFDMSend();

                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "檔案Count:" + listCaseCustRFDMSend.Count().ToString() + "----------------");

                // 本地路徑
                if (!Directory.Exists(localFilePath))
                {
                    Directory.CreateDirectory(localFilePath);
                }
                // 逐個下載&匯入DB
                foreach (CaseCustRFDMSend item in listCaseCustRFDMSend)
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

                            UpdateCaseCustRFDMSend(item.TrnNum, item.VersionNewID, "08");

                            // 判斷是否將Version檔更新為成功，因RFDMSend檔有3筆資料
                            if (QueryRFDMSendByStatus(item.VersionNewID))
                            {
                                //  更新RFDM查詢狀態為8：文檔資料獲取成功
                                UpdateQueryVersion(item.TrnNum, item.VersionNewID, "8");
                            }
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

        public bool QueryRFDMSendByStatus(string strVersionNewID)
        {
            string sqlSelect = @" 
                                SELECT COUNT(VersionNewID) AS CountRFDMSend 
                                FROM CaseCustRFDMSend 
                                WHERE VersionNewID = '" + strVersionNewID + @"'
                                    AND RFDMSendStatus = '08' 
                                ";

            base.Parameter.Clear();

            DataTable dt = base.Search(sqlSelect);

            // 有其它狀態的資料
            if (dt != null && dt.Rows.Count > 0 && dt.Rows[0]["CountRFDMSend"].ToString().Trim() == "3")
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 讀取檔案到FTP
        /// </summary>
        /// <param name="localFilePath">本地路徑</param>
        /// <param name="item">來文檔案查詢條件</param>
        /// <returns></returns>
        public bool InsertToDB(string localFilePath, CaseCustRFDMSend item)
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

                            //20181228 固定變更 add start
                            int count = columns[18].Trim().Length;

                            if (count == 7)
                            {
                               columns[18] = "0" + columns[18].Trim();
                            }
                            //20181228 固定變更 add end

                            #region insert SQL

                            string sqlInsert = @"INSERT INTO CaseCustRFDMRecv
                                                            ( NewID ,
                                                              TrnNum ,
                                                              VersionNewID ,
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
                                                            )
                                                    VALUES  ( NEWID() ,
                                                              '" + item.TrnNum + @"' , 
                                                              '" + item.VersionNewID + @"' , -- VersionNewID - uniqueidentifier
                                                              '" + columns[0].Trim() + @"' , -- DATA_DATE - date
                                                              '" + columns[1].Trim() + @"' , -- ACCT_NO - varchar(16)
                                                              '" + columns[2].Trim() + @"' , -- JNRST_DATE - date
                                                              '" + columns[3].Trim() + @"' , -- JNRST_TIME - varchar(8)
                                                              '" + columns[4].Trim() + @"' , -- JNRST_TIME_SEQ - varchar(2)
                                                              '" + columns[5].Trim() + @"' , -- TRAN_DATE - date
                                                              '" + columns[6].Trim() + @"' , -- POST_DATE - date
                                                              '" + columns[7].Trim() + @"' , -- TRANS_CODE - varchar(5)
                                                              '" + columns[8].Trim() + @"' , -- JRNL_NO - varchar(7)
                                                              '" + columns[9].Trim() + @"' , -- REVERSE - char(1)
                                                              '" + columns[10].Trim() + @"' , -- PROMO_CODE - varchar(2)
                                                              '" + columns[11].Trim() + @"' , -- REMARK - nvarchar(20)
                                                              '" + columns[12].Trim() + @"' , -- TRAN_AMT - decimal
                                                              '" + columns[13].Trim() + @"' , -- BALANCE - decimal
                                                              '" + columns[14].Trim() + @"' , -- TRF_BANK - varchar(3)
                                                              '" + columns[15].Trim() + @"' , -- TRF_ACCT - varchar(16)
                                                              '" + columns[16].Trim() + @"' , -- NARRATIVE - nvarchar(20)
                                                              '" + columns[17].Trim() + @"' , -- FISC_BANK - varchar(3)
                                                              '" + columns[18].Trim() + @"' , -- FISC_SEQNO - varchar(8)
                                                              '" + columns[19].Trim() + @"' , -- CHQ_NO - varchar(8)
                                                              '" + columns[20].Trim() + @"' , -- ATM_NO - varchar(4)
                                                              '" + columns[21].Trim() + @"' , -- TRAN_BRANCH - varchar(4)
                                                              '" + columns[22].Trim() + @"' , -- TELLER - varchar(5)
                                                              '" + columns[23].Trim() + @"' , -- FILLER - varchar(54)
                                                              '" + columns[24].Trim() + @"' , -- TXN_DESC - nvarchar(8)
                                                              '" + columns[25].Trim() + @"', -- ACCT_P2 - tinyint
                                                              '" + columns[26].Trim() + @"' , -- FILE_NAME - varchar(20)
                                                              '" + columns[27].Trim() + @"'  -- TYPE - varchar(1)
                                                             )  ";

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
        public void UpdateCaseCustRFDMSend(string strTrnNum, string strVersionNewID, string strRFDMSendStatus)
        {
            try
            {
                // 更新CaseCustRFDMSend
                string sqlUpdate = @"UPDATE [CaseCustRFDMSend] 
                                                        SET 
                                                            [RFDMSendStatus] = @RFDMSendStatus,
                                                            [ModifiedDate] = GETDATE(), 
                                                            [ModifiedUser] = 'SYSTEM' 
                                                        WHERE TrnNum = @TrnNum;";

                base.Parameter.Clear();

                base.Parameter.Add(new CommandParameter("@TrnNum", strTrnNum));
                base.Parameter.Add(new CommandParameter("@RFDMSendStatus", strRFDMSendStatus));

                base.ExecuteNonQuery(sqlUpdate);
            }
            catch (Exception ex)
            {
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, " 事務回滾, 錯誤訊息: " + ex.ToString());

            }
        }

        public void UpdateQueryVersion(string strTrnNum, string strVersionNewID, string strRFDMSendStatus)
        {
            try
            {
                // 更新CaseCustQueryVersion
                string sqlUpdate = @"UPDATE [CaseCustQueryVersion] 
                                                        SET 
                                                            [RFDMSendStatus] = @RFDMSendStatus,
                                                            [ModifiedDate] = GETDATE(), 
                                                            [ModifiedUser] = 'SYSTEM' 
                                                        WHERE NewID = @NewID; ";

                base.Parameter.Clear();

                base.Parameter.Add(new CommandParameter("@NewID", strVersionNewID));
                base.Parameter.Add(new CommandParameter("@RFDMSendStatus", strRFDMSendStatus));

                base.ExecuteNonQuery(sqlUpdate);
            }
            catch (Exception ex)
            {
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, " 事務回滾, 錯誤訊息: " + ex.ToString());

            }
        }

        /// <summary>
        /// 取得需要吃檔的檔案名稱
        /// </summary>
        /// <returns></returns>
        public IList<CaseCustRFDMSend> GetCaseCustRFDMSend()
        {
            string sqlSelect = @" SELECT  FileName
                                            ,TrnNum
                                            ,VersionNewID
                                    FROM    CaseCustRFDMSend
                                    WHERE   RFDMSendStatus = '01'
                                            AND ISNULL(FileName, '') <> '' ";
            Parameter.Clear();

            return base.SearchList<CaseCustRFDMSend>(sqlSelect);
        }

        /// <summary>
        /// 來文檔案查詢條件
        /// </summary>
        public class CaseCustRFDMSend
        {
            /// <summary>
            /// 交易序號
            /// </summary>
            public string TrnNum { get; set; }

            /// <summary>
            /// 來文檔案查詢條件帳號檔主鍵
            /// </summary>
            public string VersionNewID { get; set; }

            /// <summary>
            /// 檔案名稱
            /// </summary>
            public string FileName { get; set; }
        }

    }
}
