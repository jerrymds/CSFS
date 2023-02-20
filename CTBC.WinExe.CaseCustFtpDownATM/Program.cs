using CTBC.CSFS.BussinessLogic;
using CTBC.CSFS.Models;
using CTBC.CSFS.Pattern;
using CTBC.FrameWork.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

namespace CTBC.WinExe.CaseCustFtpDownATM
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



            #region 測試用, 故意等待秒數
            if (ConfigurationManager.AppSettings["WaitTimeForTest"] != null)
            {
                int WaitTime = 0;
                WaitTime = int.Parse(ConfigurationManager.AppSettings["WaitTimeForTest"].ToString());
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.Information, string.Format("目前故意只是等待  {0}   秒!! ", WaitTime));
                System.Threading.Thread.Sleep(1000 * WaitTime);

                // 只是測試

                //CaseCustFtpDownATMBiz c = new CaseCustFtpDownATMBiz();
                //c.outputF2_2023("F7088456-5E25-4F71-9317-BE078787F38C","20230117");
                //m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.Information, string.Format("目前只是測試, 要拿掉!!!!!!! "));
                //return;
            } 
            #endregion


            #region 防止重覆執行, 不設參數就不阻檔重覆執行
            if (ConfigurationManager.AppSettings["exeFile"] != null)
            {
                string exeFile = ConfigurationManager.AppSettings["exeFile"].ToString();
                if (IsProcessRunning(exeFile))
                {
                    m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.Information, string.Format("前一支 {0} 正在執行中， 故本次不進行執行!! ", exeFile));
                    return;
                }
            } 
            #endregion



            string processCaseId = string.Empty;
            DateTime thenow = DateTime.Now;
            DateTime lastWorkDay = getLastWorkDay(thenow); // 應該要讀取前一天的工作日才對

            try
            {
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.Information, "-------處理開始-------");

                // 系統當前時數大於config設定的時數，則將昨天前發送成功的電文狀態更新為 【未收到ATM資料檔】

                if (!checkReciveTime(lastWorkDay, ref processCaseId))
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

        public bool IsProcessRunning(string name)
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

        /// <summary>
        /// 檢查批次程式是否正在執行中
        /// </summary>
        /// <param name="executeName"></param>
        /// <returns></returns>
        public Boolean CheckProcess(string executeName)
        {
            System.Diagnostics.Process[] process = System.Diagnostics.Process.GetProcessesByName(Path.GetFileNameWithoutExtension(executeName));

            m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.Information, string.Format("CheckProcess() : 檢查外部批次程式[{0}]是否尚在執行中 ? {1}", executeName, (process.Length > 0).ToString()));
            return (process.Length > 0);




        }


        /// <summary>
        /// 系統當前時數大於config設定的時數，則將昨天前發送成功的電文狀態更新為 【未收到ATM資料檔】
        /// </summary>
        public Boolean checkReciveTime(DateTime lastWorkDay, ref string versionNewId)
        {
            string strTime = ConfigurationManager.AppSettings["NoReciveFileTime"];
            Boolean processFlag = false;

            try
            {
                // 系統當前時數大於config設定的時數，則將昨天前發送成功的電文狀態更新為 【未收到ATM資料檔】
                if (DateTime.Now.Hour >= int.Parse(strTime))
                {

                    m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------系統當前時數大於config設定的時數：" + strTime + "----------------");

                    processFlag = true;


                    //取得目前處理中的CaseId (即CaseCustDetails.DetailsId )
                    try
                    {
                        string pSql = "SELECT TOP 1 * FROM BOPS081019Send WHERE SendStatus='02' AND ATMFlag='N' AND CONVERT(VARCHAR, ModifiedDate, 111) = '" + lastWorkDay.ToString("yyyy/MM/dd") + "'";
                        var ret = base.Search(pSql);
                        if (ret.Rows.Count > 0)
                        {
                            versionNewId = ret.Rows[0]["VersionNewID"].ToString();
                        }
                        else
                            versionNewId = null;
                    }
                    catch (Exception ex)
                    {
                        m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, " 在BOPS081019Send 找不到 昨日的案件   " + ex.ToString());
                    }



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
                                                        AND CONVERT(VARCHAR, ModifiedDate, 111) = '" + lastWorkDay.ToString("yyyy/MM/dd") + "' ";
                            // 2021-02-04

                            base.ExecuteNonQuery(strUpdate, dbTransaction);

                            // 將Master, Version 檔 Status的狀態更新為失敗
                            m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------將Master, Version 檔 Status的狀態更新為失敗]作業開始----------------");

                            strUpdate = @"UPDATE [CaseCustDetails] 
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
                                    WHERE DetailsId in (
                                        SELECT distinct VersionNewID 
                                        FROM BOPS081019Send
                                        WHERE SENDSTATUS = '01' 
                                            AND ATMFlag = 'E' 
                                            AND CONVERT(VARCHAR, ModifiedDate, 111) = '" + lastWorkDay.ToString("yyyy/MM/dd") + "' );";
                            strUpdate = strUpdate + @"   UPDATE [CaseCustMaster] 
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
                                        SELECT distinct CaseCustMasterId
                                        FROM CaseCustDetails
                                        LEFT JOIN BOPS081019Send ON (CaseCustDetails.DetailsId = BOPS081019Send.VersionNewID)
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
            }
            catch (Exception ex)
            {
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "錯誤-------- 系統當前時數大於config設定的時數  錯誤"  + "錯誤訊息" + ex.Message.ToString());
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
            public string MEMBER_NO { get; set; }

        }

        /// <summary>
        /// 將FTP檔案拷貝到本機
        /// </summary>
        /// <returns></returns>
        public static bool ReciveFile()
        {
            bool boolFlag = false;
            bool isFtp = bool.Parse(ConfigurationManager.AppSettings["isFtp"].ToString());
            try
            {
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息---------從MFTP下載檔案作業開始----------------");

                // 判斷路徑是否存在
                if (!Directory.Exists(reciveloaclFilePath))
                {
                    Directory.CreateDirectory(reciveloaclFilePath);
                }


                if (isFtp) // 由Ftp 中, 取得檔案
                {
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
                else // 直接取File目錄裏的檔案
                {
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
            bool isFtp = bool.Parse(ConfigurationManager.AppSettings["isFtp"].ToString());
            try
            {
                // 判斷 Backup路徑是否存在
                if (!Directory.Exists(backupFilePath))
                {
                    Directory.CreateDirectory(backupFilePath);
                }

                // 取得本地文件清單
                string[] fileList = Directory.GetFiles(reciveloaclFilePath);

                // 20220609, 只要鎖定前一個工作日所產生的檔, 來Insert OutputF2 .....
                string[] fileListWorkingDate = filterWorkingDate(fileList, lastWorkDay);


                //  上傳本地文件到指定目錄
                foreach (string file in fileListWorkingDate)
                {
                    if (string.IsNullOrEmpty(file))
                    {
                        m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, " 目前無前一個工作日的檔案 ");
                        break;
                    }

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
                                        // 20221122, 將原來NARRATIVE, 由原來40, 改為16長度.
                                        CaseCustATMRecv.NARRATIVE = Big5SubStr(strLine, 16).Trim();
                                        // 20221121, 新增會員編號
                                        CaseCustATMRecv.MEMBER_NO = Big5SubStr(strLine, 20).Trim();

                                        //20230105, 新增三個欄位
                                        string strDATA_DATE = DateTime.Now.ToString("yyyyMMdd");
                                        string strFISC_SEQNO = "0" + CaseCustATMRecv.YBTXLOG_STAND_NO;

                                        //string strYBTXLOG_DATE = "";
                                        DateTime dYBTXLOG_DATE;
                                        if (!DateTime.TryParseExact(CaseCustATMRecv.YBTXLOG_YYYYMMDD, "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out dYBTXLOG_DATE))
                                        {
                                            //TryParse轉換失敗. 則用字串方式拼接
                                            int iYear = int.Parse(CaseCustATMRecv.YBTXLOG_YYYYMMDD.Substring(0, 4));
                                            int iMonth = int.Parse(CaseCustATMRecv.YBTXLOG_YYYYMMDD.Substring(4, 2));
                                            int iday = int.Parse(CaseCustATMRecv.YBTXLOG_YYYYMMDD.Substring(6, 2));
                                            dYBTXLOG_DATE = new DateTime(iYear, iMonth, iday);
                                            //strYBTXLOG_DATE = dYBTXLOG_DATE.ToString("yyyy-MM-dd");
                                        }

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
                                                ,MEMBER_NO
										        ,DATA_DATE
                                                ,FISC_SEQNO
                                                ,YBTXLOG_DATE
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
                                           ,'" + CaseCustATMRecv.MEMBER_NO + @"'
                                           ,'" + strDATA_DATE + @"'
                                           ,'" + strFISC_SEQNO + @"'
                                           ,@yb_date
                                        )";

                                        base.Parameter.Clear();
                                        base.Parameter.Add(new CommandParameter("@yb_date", dYBTXLOG_DATE));	
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
                                //                                string strUpdate = @" UPDATE BOPS081019Send SET ATMFlag = 'Y' 
                                //                                                      WHERE SendStatus = '01' 
                                //                                                        AND ATMFlag = 'N' 
                                //                                                        AND CONVERT(VARCHAR, ModifiedDate, 111) = '" + lastWorkDay.ToString("yyyy/MM/dd") + "' ";
                                //adam 2022/03/01
                                string strUpdate = @" UPDATE BOPS081019Send SET ATMFlag = 'Y' 
                                                      WHERE  ATMFlag = 'N' 
                                                        AND CONVERT(VARCHAR, ModifiedDate, 111) = '" + lastWorkDay.ToString("yyyy/MM/dd") + "'; ";

                                base.ExecuteNonQuery(strUpdate, dbTransaction);

                                // 將Version 檔 HTG的狀態更新為成功
                                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------[將Version 檔 HTG的狀態更新為成功]作業開始----------------");

                                strUpdate = @"UPDATE [CaseCustDetails] 
                                                SET 
                                                    [HTGSendStatus] = '4',
                                                    [ModifiedDate] = GETDATE(), 
                                                    [ModifiedUser] = 'SYSTEM'
                                                WHERE DetailsId in (
                                                    SELECT distinct VersionNewID FROM BOPS081019Send
                                                    WHERE SENDSTATUS = '01' 
                                                        AND ATMFlag = 'Y' 
                                                        AND CONVERT(VARCHAR, ModifiedDate, 111) = '" + lastWorkDay.ToString("yyyy/MM/dd") + "' ); ";
                                strUpdate = strUpdate + @"  UPDATE [CaseTrsQueryVersion] 
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


                    #region 將今日的DetailsId 回傳, 以產生CaseCustOutputF2
                    try
                    {

                        CaseCustFtpDownATMBiz biz = new CaseCustFtpDownATMBiz();

                        //                        var sql2 = @"SELECT distinct VersionNewID FROM BOPS081019Send
                        //                                                    WHERE SENDSTATUS = '01' 
                        //                                                        AND ATMFlag = 'Y' 
                        //                                                        AND CONVERT(VARCHAR, ModifiedDate, 111) = '" + lastWorkDay.ToString("yyyy/MM/dd") + "'";
                        // adam 2022/03/01 取消BOPS081019Send 條件01 保留 ATMFlag 確保己下載更新
                        var sql2 = @"SELECT distinct VersionNewID FROM BOPS081019Send
                                                                            WHERE  ATMFlag = 'Y' 
                                                                                AND CONVERT(VARCHAR, ModifiedDate, 111) = '" + lastWorkDay.ToString("yyyy/MM/dd") + "'";

                        var detailsList = base.Search(sql2);

                        foreach (DataRow dr in detailsList.Rows)
                        {
                            var processCaseId = dr[0].ToString();
                            // 產生[dbo].[CaseCustOutputF2] 的結果...
                            if (!string.IsNullOrEmpty(processCaseId))
                            {

                                string dt = biz.outputF2(processCaseId);
                                if (!string.IsNullOrEmpty(dt))
                                {
                                    m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.Error, "處理錯誤: " + dt);
                                }

                            }
                        }
                    }
                    catch (Exception ex)
                    {

                        m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.Error, " 產生CaseCustOutputF2 發生錯誤, 錯誤訊息: " + ex.ToString());
                    }


                    #endregion

                    #region 刪除FTP上的檔案

                    if (isFtp)
                    {

                        m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------[刪除FTP上的檔案]作業開始----------------");

                        string remoteFile = reciveftpClient.SetRemotePath(reciveftpdir) + "//";

                        int intName = file.LastIndexOf('\\') + 1;

                        reciveftpClient.DeleteFile(remoteFile + file.Substring(intName, file.Length - intName));

                        m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------刪除FTP文件" + file + "----------------");
                    }
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

        private string[] filterWorkingDate(string[] fileList, DateTime lastwd)
        {
            // 檔名為.... YBTXLOG_YYYYMMDD.TXT 
            string myFile = "";
            string lwd = lastwd.ToString("yyyyMMdd");
            foreach (string file in fileList)
            {
                if (file.Contains(lwd))
                {
                    myFile = file;
                    break;
                }
            }
            return new[] { myFile }; ;
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