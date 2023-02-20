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

namespace CTBC.WinExe.CSFS.HistoryRFDM
{
    class Program : BBRule
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
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "-------- 從MFTP下載交易查詢結果檔案開始----------------");

                // 取得需要匯入DB的文件清單
                IList<CaseTrsRFDMSend> listCaseTrsRFDMSend = GetCaseTrsRFDMSend();
                if (listCaseTrsRFDMSend.Count() < 1)
                {
                    m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "------------------- 無交易查詢結果接收 !!--------------- ");
                }
                else
                {
                    m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "----- 待接收交易查詢結果:  " + listCaseTrsRFDMSend.Count().ToString() + "  筆 ----------------");
                }
                // 本地路徑
                if (!Directory.Exists(localFilePath))
                {
                    Directory.CreateDirectory(localFilePath);
                }
                // 逐個下載&匯入DB
                foreach (CaseTrsRFDMSend item in listCaseTrsRFDMSend)
                {    
                    #region - Adam,  
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

                            m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "--------從MFTP下載檔案" + file + "----------------");

                            ftpClient.DeleteFile(remoteFile);

                            m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "--------刪除FTP文件" + file + "----------------");

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
                        m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "-------- 從MFTP成功下載交易查詢結果檔案:" + item.FileName + "----------------");

                        // 匯入DB
                        bool isSucess = InsertToDB(localFilePath, item);

                        // 成功匯入後，刪掉FTP上的檔案
                        if (isSucess)
                        {
                            // 刪除FTP上的檔案 -- 12/19,AdamHsieh,MFTP會自己被刪除，待確認原因
                            //ftpClient.DeleteFile(ftpdir + "/" + item.FileName);

                            // log 記錄
                            //m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息-------- 從MFTP成功刪除檔案:" + item.FileName + "----------------");

                            UpdateCaseTrsRFDMSend(item.TrnNum, item.VersionNewID, "08");

                            // 判斷是否將Version檔更新為成功，因RFDMSend檔有3筆資料
                            if (QueryRFDMSendByStatus(item.VersionNewID))
                            {
                                //  更新RFDM查詢狀態為8：文檔資料獲取成功,
                                UpdateQueryVersion(item.TrnNum, item.VersionNewID, "8");
                            }
                        }
                    }
                    else
                    {
                        // log 記錄
                        m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "-------- 從MFTP下載檔案:" + item.FileName + "失敗----------------");
                    }
                }
            }
            catch (Exception ex)
            {
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "-------- 程式錯誤，錯誤原因：" + ex.Message + "----------------");
            }
        }

        public bool QueryRFDMSendByStatus(string strVersionNewID)
        {
            string sqlSelect = @" 
                                SELECT COUNT(VersionNewID) AS CountRFDMSend 
                                FROM CaseTrsRFDMSend 
                                WHERE VersionNewID = '" + strVersionNewID + @"'
                                    AND ((RFDMSendStatus = '08') or (RspCode = '0000' and len(FileName) > 0)  or (RspCode = 'C001' ))
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
        public bool InsertToDB(string localFilePath, CaseTrsRFDMSend item)
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

                            //string sqlInsert = @"INSERT INTO CaseTrsRFDMRecv
                            //                                ( NewID ,
                            //                                  TrnNum ,
                            //                                  VersionNewID ,
                            //                                  DATA_DATE ,
                            //                                  ACCT_NO ,
                            //                                  JNRST_DATE ,
                            //                                  JNRST_TIME ,
                            //                                  JNRST_TIME_SEQ ,
                            //                                  TRAN_DATE ,
                            //                                  POST_DATE ,
                            //                                  TRANS_CODE ,
                            //                                  JRNL_NO ,
                            //                                  REVERSE ,
                            //                                  PROMO_CODE ,
                            //                                  REMARK ,
                            //                                  TRAN_AMT ,
                            //                                  BALANCE ,
                            //                                  TRF_BANK ,
                            //                                  TRF_ACCT ,
                            //                                  NARRATIVE ,
                            //                                  FISC_BANK ,
                            //                                  FISC_SEQNO ,
                            //                                  CHQ_NO ,
                            //                                  ATM_NO ,
                            //                                  TRAN_BRANCH ,
                            //                                  TELLER ,
                            //                                  FILLER ,
                            //                                  TXN_DESC ,
                            //                                  ACCT_P2 ,
                            //                                  FILE_NAME ,
                            //                                  TYPE
                            //                                )
                            //                        VALUES  ( NEWID() ,
                            //                                  '" + item.TrnNum + @"' , 
                            //                                  '" + item.VersionNewID + @"' , -- VersionNewID - uniqueidentifier
                            //                                  '" + columns[0].Trim() + @"' , -- DATA_DATE - date
                            //                                  '" + columns[1].Trim() + @"' , -- ACCT_NO - varchar(16)
                            //                                  '" + columns[2].Trim() + @"' , -- JNRST_DATE - date
                            //                                  '" + columns[3].Trim() + @"' , -- JNRST_TIME - varchar(8)
                            //                                  '" + columns[4].Trim() + @"' , -- JNRST_TIME_SEQ - varchar(2)
                            //                                  '" + columns[5].Trim() + @"' , -- TRAN_DATE - date
                            //                                  '" + columns[6].Trim() + @"' , -- POST_DATE - date
                            //                                  '" + columns[7].Trim() + @"' , -- TRANS_CODE - varchar(5)
                            //                                  '" + columns[8].Trim() + @"' , -- JRNL_NO - varchar(7)
                            //                                  '" + columns[9].Trim() + @"' , -- REVERSE - char(1)
                            //                                  '" + columns[10].Trim() + @"' , -- PROMO_CODE - varchar(2)
                            //                                  '" + columns[11].Trim() + @"' , -- REMARK - nvarchar(20)
                            //                                  '" + columns[12].Trim() + @"' , -- TRAN_AMT - decimal
                            //                                  '" + columns[13].Trim() + @"' , -- BALANCE - decimal
                            //                                  '" + columns[14].Trim() + @"' , -- TRF_BANK - varchar(3)
                            //                                  '" + columns[15].Trim() + @"' , -- TRF_ACCT - varchar(16)
                            //                                  '" + columns[16].Trim() + @"' , -- NARRATIVE - nvarchar(20)
                            //                                  '" + columns[17].Trim() + @"' , -- FISC_BANK - varchar(3)
                            //                                  '" + columns[18].Trim() + @"' , -- FISC_SEQNO - varchar(8)
                            //                                  '" + columns[19].Trim() + @"' , -- CHQ_NO - varchar(8)
                            //                                  '" + columns[20].Trim() + @"' , -- ATM_NO - varchar(4)
                            //                                  '" + columns[21].Trim() + @"' , -- TRAN_BRANCH - varchar(4)
                            //                                  '" + columns[22].Trim() + @"' , -- TELLER - varchar(5)
                            //                                  '" + columns[23].Trim() + @"' , -- FILLER - varchar(54)
                            //                                  '" + columns[24].Trim() + @"' , -- TXN_DESC - nvarchar(8)
                            //                                  '" + columns[25].Trim() + @"', -- ACCT_P2 - tinyint
                            //                                  '" + columns[26].Trim() + @"' , -- FILE_NAME - varchar(20)
                            //                                  '" + columns[27].Trim() + @"'  -- TYPE - varchar(1)

                            //                                 )  ";
                            //adam 20200721
                            string sqlInsert = @"INSERT INTO CaseTrsRFDMRecv
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
                                                    VALUES  ( @NewID ,
                                                              @TrnNum , 
                                                              @VersionNewID,  
                                                              @DATA_DATE ,
                                                              @ACCT_NO,
                                                              @JNRST_DATE,
                                                              @JNRST_TIME,
                                                              @JNRST_TIME_SEQ,
                                                              @TRAN_DATE,
                                                              @POST_DATE,
                                                              @TRANS_CODE,
                                                              @JRNL_NO,
                                                              @REVERSE,
                                                              @PROMO_CODE,
                                                              @REMARK,
                                                              @TRAN_AMT,
                                                              @BALANCE,
                                                              @TRF_BANK,
                                                              @TRF_ACCT ,
                                                              @NARRATIVE,
                                                              @FISC_BANK,
                                                              @FISC_SEQNO,
                                                              @CHQ_NO,
                                                              @ATM_NO,
                                                              @TRAN_BRANCH,
                                                              @TELLER,
                                                              @FILLER,
                                                              @TXN_DESC,
                                                              @ACCT_P2,
                                                              @FILE_NAME,
                                                              @TYPE
                                                             )  ";
                            #endregion
                            Parameter.Clear();
                            Parameter.Add(new CommandParameter("@NewID", Guid.NewGuid()));
                            Parameter.Add(new CommandParameter("@TrnNum", item.TrnNum));
                            Parameter.Add(new CommandParameter("@VersionNewID" 	,item.VersionNewID )); 
                            Parameter.Add(new CommandParameter("@DATA_DATE"     ,columns[0].Trim() ));            
                            Parameter.Add(new CommandParameter("@ACCT_NO"       ,columns[1].Trim() ));            
                            Parameter.Add(new CommandParameter("@JNRST_DATE" 	,columns[2].Trim() ));            
                            Parameter.Add(new CommandParameter("@JNRST_TIME" 	,columns[3].Trim() ));            
                            Parameter.Add(new CommandParameter("@JNRST_TIME_SEQ" ,columns[4].Trim() ));            
                            Parameter.Add(new CommandParameter("@TRAN_DATE" 	,columns[5].Trim() ));            
                            Parameter.Add(new CommandParameter("@POST_DATE" 	,columns[6].Trim() ));            
                            Parameter.Add(new CommandParameter("@TRANS_CODE"	,columns[7].Trim() ));            
                            Parameter.Add(new CommandParameter("@JRNL_NO" 		,columns[8].Trim() ));            
                            Parameter.Add(new CommandParameter("@REVERSE" 		,columns[9].Trim() ));            
                            Parameter.Add(new CommandParameter("@PROMO_CODE" 	,columns[10].Trim()));            
                            Parameter.Add(new CommandParameter("@REMARK" 		,columns[11].Trim()));            
                            Parameter.Add(new CommandParameter("@TRAN_AMT"		,columns[12].Trim()));            
                            Parameter.Add(new CommandParameter("@BALANCE"		,columns[13].Trim()));            
                            Parameter.Add(new CommandParameter("@TRF_BANK" 		,columns[14].Trim()));            
                            Parameter.Add(new CommandParameter("@TRF_ACCT"		,columns[15].Trim()));            
                            Parameter.Add(new CommandParameter("@NARRATIVE"		,columns[16].Trim()));            
                            Parameter.Add(new CommandParameter("@FISC_BANK"		,columns[17].Trim()));            
                            Parameter.Add(new CommandParameter("@FISC_SEQNO"	,columns[18].Trim()));            
                            Parameter.Add(new CommandParameter("@CHQ_NO" 		,columns[19].Trim()));            
                            Parameter.Add(new CommandParameter("@ATM_NO"		,columns[20].Trim()));            
                            Parameter.Add(new CommandParameter("@TRAN_BRANCH"	,columns[21].Trim()));            
                            Parameter.Add(new CommandParameter("@TELLER"		,columns[22].Trim()));            
                            Parameter.Add(new CommandParameter("@FILLER" 		,columns[23].Trim()));            
                            Parameter.Add(new CommandParameter("@TXN_DESC"		,columns[24].Trim()));            
                            Parameter.Add(new CommandParameter("@ACCT_P2"       ,columns[25].Trim()));            
                            Parameter.Add(new CommandParameter("@FILE_NAME"     ,columns[26].Trim()));            
                            Parameter.Add(new CommandParameter("@TYPE"          ,columns[27].Trim()));          
                            base.ExecuteNonQuery(sqlInsert, dbTransaction);
                        }
                    }

                    dbTransaction.Commit();

                    #endregion

                    m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "-------- 檔案" + item.FileName + "匯入DB成功----------------");

                    return true;
                }
                catch (Exception ex)
                {
                    dbTransaction.Rollback();

                    m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "-------- 檔案" + item.FileName + "匯入DB失敗，失敗原因：" + ex.Message + "----------------");

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
        public void UpdateCaseTrsRFDMSend(string strTrnNum, string strVersionNewID, string strRFDMSendStatus)
        {
            try
            {
                // 更新CaseTrsRFDMSend
                string sqlUpdate = @"UPDATE [CaseTrsRFDMSend] 
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
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, " 異常還原, 錯誤訊息: " + ex.ToString());

            }
        }

        public void UpdateQueryVersion(string strTrnNum, string strVersionNewID, string strRFDMSendStatus)
        {
            try
            {
                // 更新CaseCustQueryVersion
                string sqlUpdate = @"UPDATE [CaseTrsQueryVersion] 
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
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, " 異常還原, 錯誤訊息: " + ex.ToString());

            }
        }

        /// <summary>
        /// 取得需要吃檔的檔案名稱
        /// </summary>
        /// <returns></returns>
        public IList<CaseTrsRFDMSend> GetCaseTrsRFDMSend()
        {
            string sqlSelect = @" SELECT  FileName
                                            ,TrnNum
                                            ,VersionNewID
                                    FROM    CaseTrsRFDMSend
                                    WHERE   RFDMSendStatus = '01'
                                            AND ISNULL(FileName, '') <> '' ";
            Parameter.Clear();

            return base.SearchList<CaseTrsRFDMSend>(sqlSelect);
        }

        /// <summary>
        /// 來文檔案查詢條件
        /// </summary>
        public class CaseTrsRFDMSend
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
