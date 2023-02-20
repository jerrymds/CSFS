using CTBC.CSFS.BussinessLogic;
using CTBC.CSFS.Models;
using CTBC.CSFS.Pattern;
using CTBC.FrameWork.Util;
using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

namespace CTBC.WinExe.CSFS.FTPdownATM
{
    class Program : BaseBusinessRule
    {
        #region 全局變量

        private static FileLog m_fileLog;

        private static string reciveftpserver;
        private static string reciveport;
        private static string reciveusername;
        private static string recivepassword;
        private static string reciveftpdir;
        private static string reciveloaclFilePath;
        private static string backupFilePath;
        private static FtpClient reciveftpClient;

        // 截取txt字符串位置
        public static int intIndex = 0;

        public static bool boolAtmFlag = false;

        #endregion

        /// <summary>
        /// 獲取參數配置
        /// </summary>
        static Program()
        {
            m_fileLog = new FileLog(ConfigurationManager.AppSettings["filelog"], AppDomain.CurrentDomain.FriendlyName.Replace(".vshost", "").Replace(".exe", ""));

            #region 獲取FTP檔案參數配置

            reciveftpserver = ConfigurationManager.AppSettings["reciveftpserver"];
            reciveport = ConfigurationManager.AppSettings["reciveport"];
            reciveusername = ConfigurationManager.AppSettings["reciveusername"];
            recivepassword = ConfigurationManager.AppSettings["recivepassword"];

            reciveftpdir = ConfigurationManager.AppSettings["reciveftpdir"];
            reciveloaclFilePath = AppDomain.CurrentDomain.BaseDirectory + ConfigurationManager.AppSettings["reciveloaclFilePath"];
            backupFilePath = AppDomain.CurrentDomain.BaseDirectory + ConfigurationManager.AppSettings["backupFilePath"];

            reciveftpClient = new FtpClient(reciveftpserver, reciveusername, recivepassword, reciveport);

            #endregion
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            Program mainProgram = new Program();
            mainProgram.Process();
        }

        /// <summary>
        /// 排程開始方法
        /// </summary>
        private void Process()
        {
            DateTime thenow = DateTime.Now;
            DateTime lastWorkDay= getLastWorkDay(thenow); // 應該要讀取前一天的工作日才對
            try
            {
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.Information, "-------處理開始-------");

                // 系統當前時數大於config設定的時數，則將昨天前發送成功的電文狀態更新為 【未收到ATM資料檔】
                if (!checkReciveTime(lastWorkDay))
                {
                    // 將FTP檔案拷貝到本機;-
                    bool isExsit = ReciveFile();

                    // 成功下載到本地
                    if (isExsit)
                    {
                        m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.Information, "-------Insert DB-------");

                        // 將下載的檔案解析insert到Table中
                        InsertDB(lastWorkDay);

                    }
                }
            }
            catch (Exception ex)
            {
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.Information, "處理錯誤: " + ex.Message);
                throw ex;
            }
            finally
            {
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.Information, "-------處理結束-------");
            }

        }

        internal DateTime getLastWorkDay(DateTime BizDate)
        {
            string SQL = "SELECT TOP 1 * FROM [dbo].[PARMWorkingDay] where  Flag=1 and  date<'{0}' order by [date] desc";
            SQL = string.Format(SQL, BizDate.ToString("yyyy-MM-dd"));
            var d = base.SearchList<PARMWorkingDay>(SQL).FirstOrDefault();
            if (d != null)
            {
                return d.Date;
            }
            else
            {
                return BizDate.AddDays(-1);
            }
        }

        /// <summary>
        /// 系統當前時數大於config設定的時數，則將昨天前發送成功的電文狀態更新為 【未收到ATM資料檔】
        /// </summary>
        public Boolean checkReciveTime(DateTime lastWorkDay)
        {
            string strTime = ConfigurationManager.AppSettings["NoReciveFileTime"];
            Boolean processFlag = false;

            // 系統當前時數大於config設定的時數，則將昨天前發送成功的電文狀態更新為 【未收到ATM資料檔】
            if (DateTime.Now.Hour >= int.Parse(strTime))
            {

                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------系統當前時數大於config設定的時數：" + strTime + "----------------");

                processFlag = true;

                // 取得連接并開放連接
                IDbConnection dbConnection = base.OpenConnection();

                // 定義事務
                IDbTransaction dbTransaction = null;

                using (dbConnection)
                {
                    dbTransaction = dbConnection.BeginTransaction();

                    try
                    {
                        m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------[將昨天發送成功的電文，ATM資料回傳註記更新為 超過時間未收到]作業開始----------------");

                        // 將昨天發送成功的電文，ATM資料回傳註記更新為 超過時間未收到
                        string strUpdate = @" UPDATE BOPS081019Send SET ATMFlag = 'E' 
                                                      WHERE SendStatus = '01' 
                                                        AND ATMFlag = 'N' 
                                                        AND CONVERT(VARCHAR, ModifiedDate, 111) = '" + lastWorkDay.ToString("yyyy/MM/dd")+"' ";
                        // 2021-02-04

                        base.ExecuteNonQuery(strUpdate, dbTransaction);

                        // 將Master, Version 檔 Status的狀態更新為失敗
                        m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------將Master, Version 檔 Status的狀態更新為失敗]作業開始----------------");

                        strUpdate = @"UPDATE [CaseCustQueryVersion] 
                                    SET 
                                        STATUS = 
                                            CASE WHEN (Status = '02') 
                                                THEN '04' 
                                                ELSE 
                                                    CASE WHEN (STATUS = '06')
                                                        THEN '08'
                                                        ELSE STATUS
                                                    END
                                            END,
                                        [ModifiedDate] = GETDATE(), 
                                        [ModifiedUser] = 'SYSTEM'
                                    WHERE NewID in (
                                        SELECT distinct VersionNewID 
                                        FROM BOPS081019Send
                                        WHERE SENDSTATUS = '01' 
                                            AND ATMFlag = 'E' 
                                            AND CONVERT(VARCHAR, ModifiedDate, 111) = '" + lastWorkDay.ToString("yyyy/MM/dd") + "' );";
                        strUpdate= strUpdate+ @"   UPDATE [CaseCustQuery] 
                                    SET 
                                        STATUS = 
                                            CASE WHEN (STATUS = '02') 
                                                THEN '04' 
                                                ELSE 
                                                    CASE WHEN (STATUS = '06')
                                                        THEN '08'
                                                        ELSE STATUS
                                                    END
                                            END,
                                        [ModifiedDate] = GETDATE(), 
                                        [ModifiedUser] = 'SYSTEM'
                                    WHERE NewID in (
                                        SELECT distinct CaseCustNewID 
                                        FROM CaseCustQueryVersion
                                        LEFT JOIN BOPS081019Send ON (CaseCustQueryVersion.NewID = BOPS081019Send.VersionNewID)
                                        WHERE BOPS081019Send.SENDSTATUS = '01' 
                                            AND BOPS081019Send.ATMFlag = 'E' 
                                            AND CONVERT(VARCHAR, BOPS081019Send.ModifiedDate, 111) = '" + lastWorkDay.ToString("yyyy/MM/dd") + "' )";

                        base.ExecuteNonQuery(strUpdate, dbTransaction);

                        dbTransaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        dbTransaction.Rollback();

                        m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, " 事務回滾, 錯誤訊息: " + ex.ToString());
                    }
                }
            }

            if (!processFlag)
            {
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------系統當前時數未大於config設定的時數：" + strTime + "----------------");
            }

            return processFlag;
        }

        public class CaseCustATMRecv
        {
            public string YBTXLOG_YYYYMMDD { get; set; }
            public string YBTXLOG_SRC_ID { get; set; }
            public string YBTXLOG_STAND_NO { get; set; }
            public string YBTXLOG_TXN_HHMMSS { get; set; }
            public string YBTXLOG_TXN_CODE { get; set; }
            public string YBTXLOG_TRNFER_BANK { get; set; }
            public string YBTXLOG_TRNFER_ACT { get; set; }
            public string YBTXLOG_TRNFEE_BANK { get; set; }
            public string YBTXLOG_TRNFEE_ACT { get; set; }
            public string YBTXLOG_AMT { get; set; }
            public string YBTXLOG_SAFE_TMNL_ID { get; set; }
            public string YBTXLOG_IC_MEMO_CARDNO { get; set; }
            public string NARRATIVE { get; set; }

        }

        /// <summary>
        /// 將FTP檔案拷貝到本機
        /// </summary>
        /// <returns></returns>
        public static bool ReciveFile()
        {
            bool boolFlag = false;

            try
            {
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息---------從MFTP下載檔案作業開始----------------");

                // 判斷路徑是否存在
                if (!Directory.Exists(reciveloaclFilePath))
                {
                    Directory.CreateDirectory(reciveloaclFilePath);
                }

                // 取得FTP指定目錄下的所有文件名稱
                ArrayList fileList = reciveftpClient.GetFileList(reciveftpdir);

                // 下載FTP指定目錄下的所有文件
                foreach (var file in fileList)
                {
                    //  ftp 文件
                    string remoteFile = reciveftpClient.SetRemotePath(reciveftpdir) + "//" + file;

                    // 本地文件
                    string localFile = reciveloaclFilePath.TrimEnd('\\') + "\\" + file;

                    // 若已經存在，則先刪除掉舊的文件
                    if (File.Exists(localFile))
                    {
                        File.Delete(localFile);
                    }

                    reciveftpClient.GetFiles(remoteFile, localFile);

                    m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------從MFTP下載檔案" + file + "----------------");

                    boolFlag = true;
                }
            }
            catch (Exception ex)
            {
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------從MFTP下載檔案作業失敗，失敗原因：" + ex.Message + "----------------");

                boolFlag = false;
            }
            finally
            {
                if (!boolFlag)
                {
                    m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------無資料下載----------------");
                }
            }

            return boolFlag;
        }

        /// <summary>
        /// 將下載的檔案解析insert到Table中
        /// </summary>
        public void InsertDB(DateTime lastWorkDay)
        {
            try
            {
                // 判斷 Backup路徑是否存在
                if (!Directory.Exists(backupFilePath))
                {
                    Directory.CreateDirectory(backupFilePath);
                }

                // 取得本地文件清單
                string[] fileList = Directory.GetFiles(reciveloaclFilePath);

                //  上傳本地文件到指定目錄
                foreach (string file in fileList)
                {
                    // 取得連接并開放連接
                    IDbConnection dbConnection = base.OpenConnection();

                    // 定義事務
                    IDbTransaction dbTransaction = null;

                    using (dbConnection)
                    {
                        dbTransaction = dbConnection.BeginTransaction();

                        #region 讀取TXT 檔案

                        try
                        {
                            m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------讀取檔案" + file + "到DB作業開始----------------");

                            string strLine = "";

                            #region  逐筆取得檔案資料

                            //讀取資料文件
                            using (StreamReader sr = new StreamReader(file, Encoding.GetEncoding(950)))
                            {
                                while (sr.Peek() >= 0)
                                {
                                    // 讀取一行
                                    strLine = sr.ReadLine().Trim();

                                    // 主機返回ATM文檔，但沒有明細
                                    if (strLine == "NODATA")
                                    {
                                        boolAtmFlag = true;
                                    }
                                    else if (strLine != "" && strLine != "NODATA")
                                    {
                                        // 索引值設為0
                                        intIndex = 0;

                                        CaseCustATMRecv CaseCustATMRecv = new CaseCustATMRecv();

                                        CaseCustATMRecv.YBTXLOG_YYYYMMDD = Big5SubStr(strLine, 8).Trim();
                                        CaseCustATMRecv.YBTXLOG_SRC_ID = Big5SubStr(strLine, 7).Trim();
                                        CaseCustATMRecv.YBTXLOG_STAND_NO = Big5SubStr(strLine, 7).Trim();
                                        CaseCustATMRecv.YBTXLOG_TXN_HHMMSS = Big5SubStr(strLine, 6).Trim();
                                        CaseCustATMRecv.YBTXLOG_TXN_CODE = Big5SubStr(strLine, 6).Trim();
                                        CaseCustATMRecv.YBTXLOG_TRNFER_BANK = Big5SubStr(strLine, 3).Trim();
                                        CaseCustATMRecv.YBTXLOG_TRNFER_ACT = Big5SubStr(strLine, 16).Trim();
                                        CaseCustATMRecv.YBTXLOG_TRNFEE_BANK = Big5SubStr(strLine, 3).Trim();
                                        CaseCustATMRecv.YBTXLOG_TRNFEE_ACT = Big5SubStr(strLine, 16).Trim();
                                        CaseCustATMRecv.YBTXLOG_AMT = Big5SubStr(strLine, 11).Trim();
                                        //adam Add 附言
                                        /*
                                        string strAmtL = Big5SubStr(strLine, 11).Trim();
                                        string strAmtR = strAmtL;

                                        if (strAmtL != "" && strAmtL.Length > 9)
                                        {
                                            CaseCustATMRecv.YBTXLOG_AMT = strAmtL.Substring(0, 9) + "." + strAmtR.Substring(9, strAmtR.Length - 9);
                                        }
                                        else
                                        {
                                            CaseCustATMRecv.YBTXLOG_AMT = Big5SubStr(strLine, 11).Trim();
                                        }
                                        */

                                        CaseCustATMRecv.YBTXLOG_SAFE_TMNL_ID = Big5SubStr(strLine, 8).Trim();
                                        CaseCustATMRecv.YBTXLOG_IC_MEMO_CARDNO = Big5SubStr(strLine, 16).Trim();
                                        CaseCustATMRecv.NARRATIVE = Big5SubStr(strLine, 40).Trim();
                                        // INSERT CaseCustATMRecv
                                        string sqlCaseCustATMRecv = @" INSERT INTO CaseCustATMRecv
                                            ( 
                                                NewID
                                                ,YBTXLOG_YYYYMMDD
                                                ,YBTXLOG_SRC_ID
                                                ,YBTXLOG_STAND_NO
                                                ,YBTXLOG_TXN_HHMMSS
                                                ,YBTXLOG_TXN_CODE
                                                ,YBTXLOG_TRNFER_BANK
                                                ,YBTXLOG_TRNFER_ACT
                                                ,YBTXLOG_TRNFEE_BANK
                                                ,YBTXLOG_TRNFEE_ACT
                                                ,YBTXLOG_AMT
                                                ,YBTXLOG_SAFE_TMNL_ID
                                                ,YBTXLOG_IC_MEMO_CARDNO
                                                ,CreatedDate
                                                ,NARRATIVE
                                             )
                                        VALUES  ( 
                                            NEWID()
                                            ,'" + CaseCustATMRecv.YBTXLOG_YYYYMMDD + @"'
                                            ,'" + CaseCustATMRecv.YBTXLOG_SRC_ID + @"'
                                            ,'" + CaseCustATMRecv.YBTXLOG_STAND_NO + @"'
                                            ,'" + CaseCustATMRecv.YBTXLOG_TXN_HHMMSS + @"'
                                            ,'" + CaseCustATMRecv.YBTXLOG_TXN_CODE + @"'
                                            ,'" + CaseCustATMRecv.YBTXLOG_TRNFER_BANK + @"'
                                            ,'" + CaseCustATMRecv.YBTXLOG_TRNFER_ACT + @"'
                                            ,'" + CaseCustATMRecv.YBTXLOG_TRNFEE_BANK + @"'
                                            ,'" + CaseCustATMRecv.YBTXLOG_TRNFEE_ACT + @"'
                                            ,'" + CaseCustATMRecv.YBTXLOG_AMT + @"'
                                            ,'" + CaseCustATMRecv.YBTXLOG_SAFE_TMNL_ID + @"'
                                            ,'" + CaseCustATMRecv.YBTXLOG_IC_MEMO_CARDNO + @"'
                                            ,GETDATE() 
                                           ,'" + CaseCustATMRecv.NARRATIVE + @"'
                                        )";

                                        base.ExecuteNonQuery(sqlCaseCustATMRecv, dbTransaction);

                                        // 讀完一行，索引值設為0
                                        intIndex = 0;

                                        // 主機返回ATM文檔，有明細
                                        boolAtmFlag = true;
                                    }
                                }
                            }

                            #endregion

                            // 將昨天發查成功HTG電文，狀態改為獲取到 ATM 文檔
                            if (boolAtmFlag)
                            {
                                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------[將昨天發查成功HTG電文，狀態改為獲取到 ATM 文檔]作業開始----------------");
                                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "更新ATMFlag為Y----------------");
                                string strUpdate = @" UPDATE BOPS081019Send SET ATMFlag = 'Y' 
                                                      WHERE SendStatus = '01' 
                                                        AND ATMFlag = 'N' 
                                                        AND CONVERT(VARCHAR, ModifiedDate, 111) = '" + lastWorkDay.ToString("yyyy/MM/dd") + "' ";

                                base.ExecuteNonQuery(strUpdate, dbTransaction);

                                // 將Version 檔 HTG的狀態更新為成功
                                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------[將Version 檔 HTG的狀態更新為成功]作業開始----------------");

                                strUpdate = @"UPDATE [CaseCustQueryVersion] 
                                                SET 
                                                    [HTGSendStatus] = '4',
                                                    [ModifiedDate] = GETDATE(), 
                                                    [ModifiedUser] = 'SYSTEM'
                                                WHERE NewID in (
                                                    SELECT distinct VersionNewID FROM BOPS081019Send
                                                    WHERE SENDSTATUS = '01' 
                                                        AND ATMFlag = 'Y' 
                                                        AND CONVERT(VARCHAR, ModifiedDate, 111) = '" + lastWorkDay.ToString("yyyy/MM/dd") + "' ); ";
                                 strUpdate = strUpdate+ @"  UPDATE [CaseTrsQueryVersion] 
                                                SET 
                                                    [HTGSendStatus] = '4',
                                                    [ModifiedDate] = GETDATE(), 
                                                    [ModifiedUser] = 'SYSTEM'
                                                WHERE NewID in (
                                                    SELECT distinct VersionNewID FROM BOPS081019Send
                                                    WHERE SENDSTATUS = '01' 
                                                        AND ATMFlag = 'Y' 
                                                        AND CONVERT(VARCHAR, ModifiedDate, 111) = '" + lastWorkDay.ToString("yyyy/MM/dd") + "' )";

                                base.ExecuteNonQuery(strUpdate, dbTransaction);
                            }

                            dbTransaction.Commit();
                        }
                        catch (Exception ex)
                        {
                            dbTransaction.Rollback();

                            m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, " 事務回滾, 錯誤訊息: " + ex.ToString());
                        }

                        #endregion
                    }

                    #region 刪除FTP上的檔案

                    m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------[刪除FTP上的檔案]作業開始----------------");

                    string remoteFile = reciveftpClient.SetRemotePath(reciveftpdir) + "//";

                    int intName = file.LastIndexOf('\\') + 1;

                    reciveftpClient.DeleteFile(remoteFile + file.Substring(intName, file.Length - intName));

                    m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------刪除FTP文件" + file + "----------------");

                    #endregion

                    // 將已完成處理的檔案，搬移到備份目錄之下
                    File.Copy(file, file.Replace(reciveloaclFilePath, backupFilePath), true);
                    File.Delete(file);
                }
            }
            catch (Exception ex)
            {
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, " 事務回滾, 錯誤訊息: " + ex.ToString());
            }
        }

        /// <summary>
        /// 用Byte截長度
        /// </summary>
        /// <param name="a_SrcStr"></param>
        /// <param name="a_Cnt"></param>
        /// <returns></returns>
        public static string Big5SubStr(string a_SrcStr, int a_Cnt)
        {
            Encoding l_Encoding = Encoding.GetEncoding("big5");
            byte[] l_byte = l_Encoding.GetBytes(a_SrcStr);
            if (a_Cnt <= 0)
                return "";
            //例若長度10 
            //若a_StartIndex傳入9 -> ok, 10 ->不行 
            if (intIndex + 1 > l_byte.Length)
                return "";
            else
            {
                //若a_StartIndex傳入9 , a_Cnt 傳入2 -> 不行 -> 改成 9,1 
                if (intIndex + a_Cnt > l_byte.Length)
                    a_Cnt = l_byte.Length - intIndex;
            }

            string strResult = l_Encoding.GetString(l_byte, intIndex, a_Cnt);
            intIndex = intIndex + a_Cnt;
            //if (a_Cnt <= 0)
            //    return "";
            ////例若長度10 
            ////若a_StartIndex傳入9 -> ok, 10 ->不行 
            //if (intIndex + 1 > a_SrcStr.Length)
            //    return "";
            //else
            //{
            //    //若a_StartIndex傳入9 , a_Cnt 傳入2 -> 不行 -> 改成 9,1 
            //    if (intIndex + a_Cnt > a_SrcStr.Length)
            //        a_Cnt = a_SrcStr.Length - intIndex;
            //}
            //string strResult = a_SrcStr.Substring( intIndex, a_Cnt);

            return strResult;
        }
    }
}