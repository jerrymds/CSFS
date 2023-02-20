using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Data;
using System.IO;
using CTBC.CSFS.BussinessLogic;
using CTBC.FrameWork.Util;

namespace CTBC.CSFS.APLOG
{
    class Program
    {
        private static string AppPath = AppDomain.CurrentDomain.BaseDirectory;

        static void Main(string[] args)
        {
            //定義log object
            UtlLog log = new UtlLog();

            ////util物件
            //NUMSUtil _sUtilObj;

            //是否使用FTP(1=使用,0=不使用)
            string UseFTP = ConfigurationManager.AppSettings["UseFTP"];
            DateTime ProcessNow = DateTime.Now;
            string NowString = ProcessNow.ToString("yyyyMMdd");

            Process _Process = new Process(ProcessNow, NowString);

            try
            {
                #region 全局變量

                // 獲得當前時間(判斷傳入之參數是否有傳入日期，若有，則使用傳入之日期做為資料日期，若沒有，則使用系統日做為資料日期")
                string dateString = ProcessNow.AddDays(-1).ToString("yyyyMMdd"); //取前一天的資料

                // 判斷是否傳入參數
                if (args.Length > 0)
                {
                    #region 獲取排程執行需要的日期參數
                    if (!string.IsNullOrEmpty(args[0].ToString()))
                    {
                        // 若有，則使用傳入之日期做為資料日期，若沒有，則使用系統日做為資料日期
                        dateString = args[0].ToString();
                    }
                    #endregion
                }

                log._defaultLogFile = System.Configuration.ConfigurationManager.AppSettings["LogUrl"] + "\\" + DateTime.Now.ToString("yyyyMM") + "\\" + DateTime.Now.ToString("yyyyMMdd");
                log.WriteLog(" ***************    開始執行APLOG批次 " + DateTime.Now.ToString("yyyy/MM/dd hh:mm:ss") + "  ***************\r\n", CTBC.FrameWork.Util.Common.LogType.Information);

                // 取得DB連接字符串
                //_sUtilObj = new NUMSUtil(ConfigurationManager.AppSettings["ServiceConfigUrl"]);
                // SQLDB設定
                //string dataSource = _sUtilObj.ConfigSetting["DataSource"].ToString();
                //string initCatalog = _sUtilObj.ConfigSetting["InitCatalog"].ToString();
                //string uid = _sUtilObj.ConfigSetting["UID"].ToString();
                //string password = _sUtilObj.ConfigSetting["Password"].ToString();
                string strr = ConfigurationManager.ConnectionStrings["CSFS_ADO"].ConnectionString;

                // 數據文件存放路徑
                string txtUrl = AppPath + "\\" + System.Configuration.ConfigurationManager.AppSettings["txtUrl"];

                // 數據備份文件存放路徑
                string BackUpUrl = AppPath + "\\" + System.Configuration.ConfigurationManager.AppSettings["BackUpUrl"] + "\\" + DateTime.Now.ToString("yyyyMM");

                //資料明細筆數
                int APLOGCount = 0;

                APLogBIZ APLogBIZ = new APLogBIZ(strr);
                
                //APLog Redis ader 2022-05-01 - START
                APLogBIZ.InitRedis();
                //APLog Redis ader 2022-05-01 - END
                #endregion

                #region 主程式

                #region 產出TXT

                bool TxtFaild = true;

                // 明細檔
                _Process.APLOG(log, APLogBIZ, dateString, txtUrl, BackUpUrl, ref  APLOGCount, ref TxtFaild);

                // Header檔
                _Process.Header(log, APLogBIZ, dateString, txtUrl, BackUpUrl, APLOGCount, ref TxtFaild);

                #endregion

                #region 上傳FTP及刪除已上傳檔案

                // 使用FTP
                if (UseFTP == "1")
                {
                    if (TxtFaild) // 生成文字檔沒有錯誤
                    {
                        bool Faild = true;

                        // 上傳明細檔
                        _Process.SendFileFTP(txtUrl + "\\CSFS_" + dateString + ".D", "CSFS_" + dateString + ".D", log, ref Faild);

                        // 上傳Header檔
                        _Process.SendFileFTP(txtUrl + "\\CSFS_" + dateString + ".H", "CSFS_" + dateString + ".H", log, ref Faild);

                        #region 刪除文字檔
                        // FTP上傳成功刪除文字檔(如果上傳失敗，則文字檔都保留)
                        if (Faild)
                        {
                            // 刪除明細檔
                            _Process.SendFileDelte(txtUrl + "\\CSFS_" + dateString + ".D", "CSFS_" + dateString + ".D", log);

                            // 刪除Header檔
                            _Process.SendFileDelte(txtUrl + "\\CSFS_" + dateString + ".H", "CSFS_" + dateString + ".H", log);
                        }
                        #endregion
                    }
                }
                else
                {
                    log.WriteLog(" ***************    目前設定不FTP上傳檔案 " + DateTime.Now.ToString("yyyy/MM/dd hh:mm:ss") + "  ***************\r\n", CTBC.FrameWork.Util.Common.LogType.Information);
                }
                #endregion

                log.WriteLog(" ***************    執行APLOG批次結束 " + DateTime.Now.ToString("yyyy/MM/dd hh:mm:ss") + "  ***************\r\n", CTBC.FrameWork.Util.Common.LogType.Information);
                #endregion
            }
            catch (Exception ex)
            {
                log.WriteLog("批次執行過程發生錯誤 :  " + ex.ToString(), CTBC.FrameWork.Util.Common.LogType.Error);
            }
        }
    }

    class Process
    {
        #region 變量
        DateTime _ProcessDate;
        string _ProcessdateString;
        #endregion

        public Process(DateTime _d, string _dstring)
        {
            _ProcessDate = _d;
            _ProcessdateString = _dstring;
        }
        /// <summary>
        /// 生成明細檔TXT
        /// </summary>
        /// <param name="log">記錄LOG的對象</param>
        /// <param name="APLogBIZ">訪問BIZ層對象</param>
        /// <param name="dateString">資料日期</param>
        /// <param name="txtUrl">txt文件存放路徑</param>
        /// <param name="BackUpUrl">txt備份文件存放路徑</param>
        /// <param name="APLOGCount">記錄明細檔筆數</param>
        /// <param name="TxtFaild">判斷生成明細檔TXT是否發生錯誤</param>
        public void APLOG(UtlLog log, APLogBIZ APLogBIZ, string dateString, string txtUrl, string BackUpUrl, ref int APLOGCount, ref bool TxtFaild)
        {
            string TXTName = "CSFS_" + dateString;
            try
            {
                log._defaultLogFile = System.Configuration.ConfigurationManager.AppSettings["LogUrl"] + "\\" + DateTime.Now.ToString("yyyyMM") + "\\" + DateTime.Now.ToString("yyyyMMdd");
                log.WriteLog(" ***************    開始產出文字檔 " + TXTName + ".D " + DateTime.Now.ToString("yyyy/MM/dd hh:mm:ss") + "  ***************\r\n", CTBC.FrameWork.Util.Common.LogType.Information);
                log.WriteLog(" ***************     開始獲得" + TXTName + ".D 數據      ***************", CTBC.FrameWork.Util.Common.LogType.Information);

                // 獲取ApLog檔資料
                DataTable dtAPLogRawData = APLogBIZ.GetAPLogRawData(dateString);

                StringBuilder str = new StringBuilder();

                // 明細資料筆數
                //APLOGCount = dtAPLogRawData.Rows.Count;//APLog Redis ader 2022-05-01 -DEL

                // 取得數據
                //System_Code	VARCHAR(20)	系統別
                //Login_Account_Nbr VARCHAR(10) 使用者帳號
                //Query_Datetime	yyyy-MM-dd HH:mm:ss.SSS	杳詢日期/時間
                //AP_Txn_Code	VARCHAR(30)	交易代號或畫面代碼
                //Server_Name	VARCHAR(20)	所連線Server Name
                //User_Terminal VARCHAR(20)	使用者終端設備Id
                //AP_Account_Nbr	VARCHAR(20)	使用之AP帳號
                //Txn_Type_Code	CHAR(1)	動作類別
                //Statement_Text	VARCHAR(1000)	執行參數/語法
                //Object_Name		VARCHAR(128)	所存取之檔案/物件名稱
                //Txn_Status_Code	CHAR(1)	執行狀態
                //Customer_Id	VARCHAR(14)	客戶ID
                //Account_Nbr	VARCHAR(16)	交易帳號/卡號
                //Branch_Nbr	VARCHAR(4)	帳務分行別
                //Role_Id	VARCHAR(50)	登入系統帳號之角色
                //Import_Source	VARCHAR(20)	資料來源
                //As_Of_Date	'YYYYMMDD'	資料日期
                for (int i = 0; i < dtAPLogRawData.Rows.Count; i++)
                {
                    #region 宣告變數
                    string System_Code = string.Empty;
                    string Login_Account_Nbr = string.Empty;
                    string Query_Datetime = string.Empty;
                    string AP_Txn_Code = string.Empty;
                    string Server_Name = string.Empty;
                    string User_Terminal = string.Empty;
                    string AP_Account_Nbr = string.Empty;
                    string Txn_Type_Code = string.Empty;
                    string Statement_Text = string.Empty;
                    string Object_Name = string.Empty;
                    string Txn_Status_Code = string.Empty;
                    string Customer_Id = string.Empty;
                    string Account_Nbr = string.Empty;
                    string Branch_Nbr = string.Empty;
                    string Role_Id = string.Empty;
                    string Import_Source = string.Empty;
                    string As_Of_Date = string.Empty;
                    #endregion

                    #region Assign變數
                    System_Code = "CSFS";
                    Login_Account_Nbr = dtAPLogRawData.Rows[i]["Account"].ToString();
                    Query_Datetime = dtAPLogRawData.Rows[i]["DataTimestamp"].ToString();
                    AP_Txn_Code = dtAPLogRawData.Rows[i]["Controller"].ToString() + "." + dtAPLogRawData.Rows[i]["Action"].ToString();
                    Server_Name = "";
                    User_Terminal = dtAPLogRawData.Rows[i]["IP"].ToString();
                    AP_Account_Nbr = "";
                    Txn_Type_Code = "";
                    Statement_Text = dtAPLogRawData.Rows[i]["Parameters"].ToString();
                    Statement_Text = Statement_Text.TrimEnd('|');
                    Object_Name = "";
                    Txn_Status_Code = "";
                    Customer_Id = dtAPLogRawData.Rows[i]["CusID"].ToString();
                    Account_Nbr = "";
                    Branch_Nbr = "";
                    Role_Id = "";
                    Import_Source = "CSFS_APLOG";
                    As_Of_Date = dateString;
                    #endregion

                    #region Replace逗號 20150828
                    Login_Account_Nbr = Login_Account_Nbr.Replace(',', '.');
                    AP_Txn_Code = AP_Txn_Code.Replace(',', '.');
                    User_Terminal = User_Terminal.Replace(',', '.');
                    Statement_Text = Statement_Text.Replace(',', '.');
                    Customer_Id = Customer_Id.Replace(',', '.');
                    #endregion

                    #region 欄位長度判斷
                    Login_Account_Nbr = StringHelper.Big5SubStr(Login_Account_Nbr, 0, 10);
                    AP_Txn_Code = StringHelper.Big5SubStr(AP_Txn_Code, 0, 30);
                    User_Terminal = StringHelper.Big5SubStr(User_Terminal, 0, 20);
                    Statement_Text = StringHelper.Big5SubStr(Statement_Text, 0, 1000);
                    Customer_Id = StringHelper.Big5SubStr(Customer_Id, 0, 14);
                    #endregion
                    
                    //APLog Redis ader 2022-05-01 - START
                    if (string.IsNullOrWhiteSpace(Customer_Id)
                        || CTCB.APLog.APLogUtil.IsDuplicate(System_Code, AP_Txn_Code, User_Terminal, Login_Account_Nbr, Customer_Id, System.Convert.ToDateTime(dtAPLogRawData.Rows[i]["DataTimestamp"]))
                        )
                    {
                        continue;//避免上傳客戶ID為空或重覆的資料
                    }
                    APLOGCount++;
                    //APLog Redis ader 2022-05-01 - END

                    // 拼接一筆明細檔資料
                    str.Append("CSFS" + "," + Login_Account_Nbr + "," + Query_Datetime + "," + AP_Txn_Code + "," + Server_Name + "," + User_Terminal + "," + AP_Account_Nbr + "," + Txn_Type_Code + "," + Statement_Text + "," + Object_Name + "," + Txn_Status_Code + "," + Customer_Id + "," + Account_Nbr + "," + Branch_Nbr + "," + Role_Id + "," + Import_Source + "," + dateString + "\r\n");
                }

                //APLog Redis ader 2022-05-01 - START
                log.WriteLog(" ***************     獲得" + TXTName + ".D redis 數據開始      ***************\r\n", CTBC.FrameWork.Util.Common.LogType.Information);
                string systemID = CTCB.APLog.APLogUtil.SYSID_CSFS;
                DateTime date = DateTime.ParseExact(dateString, "yyyyMMdd", null);
                int contentLength;
                string message;
                int redisCount = CTCB.APLog.APLogUtil.AppendAPLogDetail(systemID, date, ref str, out contentLength, out message);
                APLOGCount += redisCount;
                log.WriteLog(string.Format(" ***************     獲得{0}.D redis 數據，計{1:n0}筆客戶資料，總資料資料長度為 {2:n0}", TXTName, redisCount, contentLength), CTBC.FrameWork.Util.Common.LogType.Information);
                if (!string.IsNullOrWhiteSpace(message))
                {
                    log.WriteLog(message, CTBC.FrameWork.Util.Common.LogType.Error);
                }
                log.WriteLog(" ***************     獲得" + TXTName + ".D redis 數據完成      ***************\r\n", CTBC.FrameWork.Util.Common.LogType.Information);

                log.WriteLog(" ***************     刪除 redis 過期數據開始      ***************\r\n", CTBC.FrameWork.Util.Common.LogType.Information);
                CTCB.APLog.APLogUtil.CleanData(systemID, date, out message);
                if (!string.IsNullOrWhiteSpace(message))
                {
                    log.WriteLog(message, CTBC.FrameWork.Util.Common.LogType.Error);
                }
                log.WriteLog(" ***************     刪除 redis 過期數據完成      ***************\r\n", CTBC.FrameWork.Util.Common.LogType.Information);
                //APLog Redis ader 2022-05-01 - END

                // 調用生成文檔方法
                Output_TXT(TXTName, str, ".D", log, txtUrl, BackUpUrl, ref TxtFaild);

            }
            catch (Exception ex)
            {
                TxtFaild = false;
                log.WriteLog(TXTName + "產出文字檔過程發生錯誤 :  " + ex.ToString(), CTBC.FrameWork.Util.Common.LogType.Error);
            }
        }

        /// <summary>
        /// 生成Header檔TXT
        /// </summary>
        /// <param name="log">記錄LOG的對象</param>
        /// <param name="APLogBIZ">訪問BIZ層對象</param>
        /// <param name="dateString">資料日期</param>
        /// <param name="txtUrl">txt文件存放路徑</param>
        /// <param name="BackUpUrl">txt備份文件存放路徑</param>
        /// <param name="APLOGCount">記錄明細檔筆數</param>
        /// <param name="TxtFaild">判斷生成Header檔TXT是否發生錯誤</param>
        public void Header(UtlLog log, APLogBIZ APLogBIZ, string dateString, string txtUrl, string BackUpUrl, int APLOGCount, ref bool TxtFaild)
        {
            string TXTName = "CSFS_" + dateString;
            try
            {
                log._defaultLogFile = System.Configuration.ConfigurationManager.AppSettings["LogUrl"] + "\\" + DateTime.Now.ToString("yyyyMM") + "\\" + DateTime.Now.ToString("yyyyMMdd");
                log.WriteLog(" ***************    開始產出文字檔 " + TXTName + ".H " + DateTime.Now.ToString("yyyy/MM/dd hh:mm:ss") + "  ***************\r\n", CTBC.FrameWork.Util.Common.LogType.Information);
                log.WriteLog(" ***************     開始獲得" + TXTName + ".H 數據      ***************", CTBC.FrameWork.Util.Common.LogType.Information);

                StringBuilder str = new StringBuilder();

                // 拼接主檔資料
                str.Append("CSFS_" + dateString + "," + APLOGCount.ToString().PadLeft(10, ' ') + "," + dateString);
                Output_TXT(TXTName, str, ".H", log, txtUrl, BackUpUrl, ref TxtFaild);

            }
            catch (Exception ex)
            {
                TxtFaild = false;
                log.WriteLog(TXTName + "產出文字檔過程發生錯誤 :  " + ex.ToString(), CTBC.FrameWork.Util.Common.LogType.Error);
            }
        }

        /// <summary>
        /// 上傳 FTP Server 檔案
        /// </summary>
        /// <param name="FileName">檔案所在路徑</param>
        /// <param name="txtName">檔案名稱</param>
        /// <param name="log">記錄LOG對象</param>
        public void SendFileFTP(string FileName, string txtName, UtlLog log, ref bool Faild)
        {
            log._defaultLogFile = System.Configuration.ConfigurationManager.AppSettings["LogUrl"] + "\\" + DateTime.Now.ToString("yyyyMM") + "\\" + DateTime.Now.ToString("yyyyMMdd");
            log.WriteLog(" ***************    開始上傳" + txtName + "檔" + "  ***************\r\n", CTBC.FrameWork.Util.Common.LogType.Information);

            string strFtpUser = System.Configuration.ConfigurationManager.AppSettings["FtpUser"]; //FTP登入帳號
            string strFtpHost = System.Configuration.ConfigurationManager.AppSettings["FtpHost"]; //FTP HOST
            string strFtpPwd = System.Configuration.ConfigurationManager.AppSettings["FtpPwd"]; //FTP 登入密碼
			string strftpSendDir = System.Configuration.ConfigurationManager.AppSettings["ftpSendDir"]; //for MFTP 新增上傳路徑

            try
            {
                FTPFactory ff = new FTPFactory();
                ff.setDebug(true);
                ff.setRemoteHost(strFtpHost);
                ff.setRemoteUser(strFtpUser);
                ff.setRemotePass(strFtpPwd);
				    ff.setRemotePath(strftpSendDir); //for MFTP 新增上傳路徑。
                ff.login();
                ff.setBinaryMode(false);
                ff.upload(FileName);
                ff.close();

                log.WriteLog(" ***************    結束上傳" + txtName + "檔" + "  ***************\r\n", CTBC.FrameWork.Util.Common.LogType.Information);
            }
            catch (Exception ex)
            {
                Faild = false;
                log.WriteLog("上傳" + txtName + "檔過程發生錯誤 :  " + ex.ToString(), CTBC.FrameWork.Util.Common.LogType.Error);
            }
        }

        /// <summary>
        /// 刪除文字檔
        /// </summary>
        /// <param name="FileName">檔案所在路徑</param>
        /// <param name="txtName">檔案名稱</param>
        /// <param name="log">記錄LOG對象</param>
        public void SendFileDelte(string FileName, string txtName, UtlLog log)
        {
            log._defaultLogFile = System.Configuration.ConfigurationManager.AppSettings["LogUrl"] + "\\" + DateTime.Now.ToString("yyyyMM") + "\\" + DateTime.Now.ToString("yyyyMMdd");

            try
            {
                log.WriteLog(" ***************    開始刪除" + txtName + "檔" + "  ***************\r\n", CTBC.FrameWork.Util.Common.LogType.Information);

                // 刪除文字檔
                if (File.Exists(FileName))
                {
                    File.Delete(FileName);
                }

                log.WriteLog(" ***************    刪除" + txtName + "檔結束 " + " ***************\r\n", CTBC.FrameWork.Util.Common.LogType.Information);

            }
            catch (Exception ex)
            {
                log.WriteLog("刪除" + txtName + "檔過程發生錯誤 :  " + ex.ToString(), CTBC.FrameWork.Util.Common.LogType.Error);
            }
        }

        /// <summary>
        /// 生成txt公用
        /// </summary>
        /// <param name="TXTName">TXT名稱</param>
        /// <param name="str">要產出的資料</param>
        /// <param name="FileName">文件後綴名</param>
        /// <param name="log">記錄LOG的對象</param>
        /// <param name="txtUrl">txt文件存放路徑</param>
        /// <param name="BackUpUrl">txt備份文件存放路徑</param>
        public void Output_TXT(string TXTName, StringBuilder str, string FileName, UtlLog log, string txtUrl, string BackUpUrl, ref bool TxtFaild)
        {
            try
            {
                log.WriteLog(" ***************     獲得" + TXTName + FileName + "數據完成      ***************\r\n", CTBC.FrameWork.Util.Common.LogType.Information);

                // 判斷存放數據文件路徑是否存在,不存在則創建
                if (!Directory.Exists(txtUrl))
                {
                    Directory.CreateDirectory(txtUrl);
                }

                // 判斷存放數據文件備份路徑是否存在,不存在則創建
                if (!Directory.Exists(BackUpUrl))
                {
                    Directory.CreateDirectory(BackUpUrl);
                }

                log.WriteLog(" ***************  將數據寫入" + TXTName + FileName + "文檔Start  ***************", CTBC.FrameWork.Util.Common.LogType.Information);

                // 將數據寫入文檔
                WriteTextFile(txtUrl + "\\" + TXTName + FileName, str);
                log.WriteLog(" ***************  將數據寫入" + TXTName + FileName + "文檔End    ***************\r\n", CTBC.FrameWork.Util.Common.LogType.Information);

                log.WriteLog(" ***************  將數據備份寫入" + TXTName + FileName + "文檔Start  ***************", CTBC.FrameWork.Util.Common.LogType.Information);

                // 將數據備份寫入文檔
                WriteTextFile(BackUpUrl + "\\" + TXTName + FileName, str);

                log.WriteLog(" ***************  將數據備份寫入" + TXTName + FileName + "文檔End    ***************\r\n", CTBC.FrameWork.Util.Common.LogType.Information);
                log.WriteLog(" ***************    結束產出文字檔 " + TXTName + FileName + " " + DateTime.Now.ToString("yyyy/MM/dd hh:mm:ss") + "  ***************\r\n", CTBC.FrameWork.Util.Common.LogType.Information);
            }
            catch (Exception ex)
            {
                TxtFaild = false;
                log.WriteLog(TXTName + "產出文字檔過程發生錯誤 :  " + ex.ToString(), CTBC.FrameWork.Util.Common.LogType.Error);
            }
        }

        /// <summary>
        /// 生成txt文檔方法
        /// </summary>
        /// <param name="path">文件生成路徑</param>
        /// <param name="strTXT">要產出的內容</param>
        public void WriteTextFile(string path, StringBuilder strTXT)
        {
            try
            {
                // 文件流
                using (FileStream fsMyfile = new FileStream(path, FileMode.Create, FileAccess.Write))
                {
                    using (StreamWriter swMyfile = new StreamWriter(fsMyfile, Encoding.GetEncoding(950)))
                    {
                        swMyfile.Write(strTXT);
                        swMyfile.Flush();
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
