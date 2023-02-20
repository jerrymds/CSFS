using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Configuration;
using System.Xml;
using System.Xml.Serialization;
using System.Data.SqlClient;
using System.IO;

namespace CTBC.CSFS.WebService
{
    /// <summary>
    /// RespFromESB 的摘要说明
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // 若要允许使用 ASP.NET AJAX 从脚本中调用此 Web 服务，请取消注释以下行。 
    // [System.Web.Script.Services.ScriptService]
    public class RespFromESB : System.Web.Services.WebService
    {
        // 獲取數據庫連接對象
        private string connString = ConfigurationManager.ConnectionStrings["CSFS_ADO"].ConnectionString;

        // log 路徑
        private static string _logPath = ConfigurationManager.AppSettings["log_ESBpath"];

        /// <summary>
        /// 定義回傳物件
        /// </summary>
        public class ReturnResult
        {
            // Both types of attributes can be applied. Depending on which type  
            // the method used, either one will affect the call.  
            [XmlElement(ElementName = "Code")]
            public String Code;

            [XmlElement(ElementName = "Message")]
            public String Message;
        }
        [WebMethod]
        public ReturnResult RespFromESBService(string PreTrnNum, string ID_No, string ProcessCode, string FileName, string ProcessMsg)
        {
            ReturnResult rtn = new ReturnResult();

            try
            {
                // 記錄Log，被ESB呼叫，參數：
                string strMessage = "傳入參數，PreTrnNum：[" + PreTrnNum + "],ID_No：[" + ID_No + "],ProcessCode：[" + ProcessCode + "],FileName：[" + FileName + "],ProcessMsg：[" + ProcessMsg + "],isNull? " + (string.IsNullOrEmpty(ProcessMsg) ? "True" : "False");
                WriteLog("ProcessResult.txt", strMessage);

                // UPDATE  到 CaseCustRFDMSend， where TrnNum = @TrnNum
                string returnCode = EditWarningQueryHistory(PreTrnNum, ID_No, ProcessCode, FileName, ProcessMsg) ? "0000" : "9999";

                // 記錄LOG, update 影響的行數
                WriteLog("ProcessResult.txt", "update" + strMessage);

                rtn.Code = returnCode;
                rtn.Message = (returnCode == "0000" ? "成功" : "無對應資料存在(交易序號或客戶ID不存在)");

                WriteLog("ProcessResult.txt", "Return : " + rtn.Code);

            }
            catch (Exception ex)
            {
                //* 單筆出錯儘量不影響全部的查詢
                WriteLog("ProcessResult.txt", "程式異常，錯誤信息: " + ex.Source);
                WriteLog("ProcessResult.txt", "程式異常，錯誤信息: " + ex.Message);
                WriteLog("ProcessResult.txt", "程式異常，錯誤信息: " + ex.ToString());

                rtn.Code = "8888";
                rtn.Message = "程式異常";

                WriteLog("ProcessResult.txt", "Return : " + rtn.Code);
            }

            return rtn;
        }

        /// <summary>
        ///  根據傳進來的參數更新WarningQueryHistory
        /// </summary>
        /// <param name="strTrnNum">交易序號</param>
        /// <param name="strID_No">客戶ID</param>
        /// <param name="strProcessCode">發查結果</param>
        /// <param name="strProcessMsg">錯誤信息</param>
        /// <returns></returns>
        public bool EditWarningQueryHistory(string strTrnNum, string strID_No, string strProcessCode, string strFileName, string strProcessMsg)
        {
            string sql = @"UPDATE [WarningQueryHistory] 
                            SET 
                                [RspCode] = @ProcessCode,
                                [RspMsg] = @ProcessMsg,
                                [FileName] = @FileName,
                                [ModifiedDate] = GETDATE(), 
                                [ModifiedUser] = 'ESB' 
                            WHERE TrnNum = @TrnNum";


            // 獲得連接對象
            SqlConnection conn = new SqlConnection(connString);

            // 獲取命令對象
            SqlCommand command = new SqlCommand(sql, conn);

            // 清空參數集合
            command.Parameters.Clear();

            //給各@變量賦值 并添加到SQL中
            SqlParameter sP = new SqlParameter("@TrnNum", strTrnNum);
            command.Parameters.Add(sP);
            sP = new SqlParameter("@ProcessCode", strProcessCode);
            command.Parameters.Add(sP);
            sP = new SqlParameter("@FileName", strFileName);
            command.Parameters.Add(sP);
            sP = new SqlParameter("@ProcessMsg", (string.IsNullOrEmpty(strProcessMsg) ? "" : strProcessMsg));
            command.Parameters.Add(sP);

            // 打開連接
            conn.Open();

            // 獲取影響行數
            int i = command.ExecuteNonQuery();

            command.Dispose();

            // 關閉連接
            conn.Close();

            return i > 0;
        }

        /// <summary>
        /// log 記錄
        /// </summary>
        /// <param name="TrnNum">交易序號</param>
        /// <param name="ID_No">客戶ID</param>
        /// <param name="ProcessCode">發查結果</param>
        /// <param name="FileName">檔案名稱</param>
        /// <param name="ProcessMsg">錯誤信息</param>
        public void WriteLog(string strFileName, string strMesage)
        {
            // 判斷路徑是否存在，不存在創建路徑
            if (!Directory.Exists(_logPath))
            {
                Directory.CreateDirectory(_logPath);
            }

            // 記錄txt文件名稱每天每個排程一個檔
            string fileName = DateTime.Now.ToString("yyyyMMdd") + "-" + strFileName;

            // 記錄信息
            StreamWriter sw = File.AppendText(_logPath + fileName);
            sw.WriteLine(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"));
            sw.WriteLine(strMesage);
            sw.Flush();
            sw.Close();
        }
    }
}
