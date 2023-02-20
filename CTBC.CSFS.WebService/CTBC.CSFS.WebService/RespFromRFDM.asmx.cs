using System;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Web.Services;
using System.Xml;
using System.Xml.Serialization;

namespace CTBC.CSFS.WebService
{

  /// <summary>
  /// Summary description for ProcessResult
  /// </summary>
  [WebService(Namespace = "http://RespFromRFDM")]
  [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
  [System.ComponentModel.ToolboxItem(false)]
  // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
  // [System.Web.Script.Services.ScriptService]
  public class ProcessResult : System.Web.Services.WebService
  {
    // 獲取數據庫連接對象
    private string connString = ConfigurationManager.ConnectionStrings["CSFS_ADO"].ConnectionString;

    // log 路徑
    private static string _logPath = ConfigurationManager.AppSettings["log_path"];

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

    /// <summary>
    /// ESB發查完成，將發查信息返回給系統，我們去抓取
    /// </summary>
    /// <param name="TrnNum">1.交易序號</param>
    /// <param name="ID_No">2.客戶ID</param>
    /// <param name="ProcessCode">3.發查結果</param>
    /// <param name="FileName">4.檔案名稱</param>
    /// <param name="ProcessMsg">5.錯誤信息</param>
    /// <returns></returns>
    [WebMethod]
    public ReturnResult RespFromRFDM(string PreTrnNum, string ID_No, string ProcessCode, string FileName, string ProcessMsg)
    {
      ReturnResult rtn = new ReturnResult();

      try
      {
        // 記錄Log，被ESB呼叫，參數：
         string strMessage = "傳入參數，PreTrnNum：[" + PreTrnNum + "],ID_No：[" + ID_No + "],ProcessCode：[" + ProcessCode + "],FileName：[" + FileName + "],ProcessMsg：[" + ProcessMsg + "],isNull? " + (string.IsNullOrEmpty(ProcessMsg) ? "True" : "False");
         WriteLog("ProcessResult.txt", strMessage);
         string returnCode="";

         if (PreTrnNum.Trim().Substring(0, 4) == "THPO") // 為高檢署案子... 打ESB開頭的代號
         {
             WriteLog("ProcessResult.txt", "\t進入THPO: " + PreTrnNum);
             string SQLErrorMessage = "";
             returnCode = EditCaseCustRFDMSend2(PreTrnNum, ID_No, ProcessCode, FileName, ProcessMsg,ref SQLErrorMessage) ? "0000" : "9999";

             WriteLog("ProcessResult.txt", "\t執行結果: "+ returnCode + "影響(更新)筆數: " + SQLErrorMessage);

         }
         else if (PreTrnNum.Trim().Substring(0, 4) == "CSFS") // 為歷史交易... 打ESB開頭的代號 
         {
             returnCode = EditCaseTrsRFDMSend(PreTrnNum, ID_No, ProcessCode, FileName, ProcessMsg) ? "0000" : "9999";
             // UPDATE  到 CaseCustRFDMSend， where TrnNum = @TrnNum
             //returnCode = EditWarningQueryHistory(PreTrnNum, ID_No, ProcessCode, FileName, ProcessMsg) ? "0000" : "9999";returnCode = EditCaseCustRFDMSend2(PreTrnNum, ID_No, ProcessCode, FileName, ProcessMsg) ? "0000" : "9999";

         }
         else  // 為原廠商... 打ESB沒有任開頭符的.... 
         {
             // UPDATE  到 CaseCustRFDMSend， where TrnNum = @TrnNum
             returnCode = EditCaseCustRFDMSend(PreTrnNum, ID_No, ProcessCode, FileName, ProcessMsg) ? "0000" : "9999";
         }


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
    ///  根據傳進來的參數更新CaseCustRFDMSend
    /// </summary>
    /// <param name="strTrnNum">交易序號</param>
    /// <param name="strID_No">客戶ID</param>
    /// <param name="strProcessCode">發查結果</param>
    /// <param name="strProcessMsg">錯誤信息</param>
    /// <returns></returns>
    public bool EditCaseCustRFDMSend(string strTrnNum, string strID_No, string strProcessCode, string strFileName, string strProcessMsg)
    {
      string sql = @"UPDATE [CaseCustRFDMSend] 
                            SET 
                                [ID_No] = @ID_No,
                                [RspCode] = @ProcessCode,
                                [RspMsg] = @ProcessMsg,
                                [FileName] = @FileName,
                                [ModifiedDate] = GETDATE(), 
                                [ModifiedUser] = 'RFDM' 
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
      sP = new SqlParameter("@ID_No", strID_No);
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
      /// 高檢署.. 開頭碼為"THPO" 
      /// </summary>
      /// <param name="strTrnNum"></param>
      /// <param name="strID_No"></param>
      /// <param name="strProcessCode"></param>
      /// <param name="strFileName"></param>
      /// <param name="strProcessMsg"></param>
      /// <returns></returns>
    public bool EditCaseCustRFDMSend2(string strTrnNum, string strID_No, string strProcessCode, string strFileName, string strProcessMsg, ref string SQLErrorMessage)
    {
        int i = 0;
        try
        {
            string sql = @"UPDATE [CaseCustNewRFDMSend] 
                            SET 
                                [ID_No] = @ID_No,
                                [RspCode] = @ProcessCode,
                                [RspMsg] = @ProcessMsg,
                                [FileName] = @FileName,
                                [ModifiedDate] = GETDATE(), 
                                [ModifiedUser] = 'RFDM' 
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
            sP = new SqlParameter("@ID_No", strID_No);
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
            i = command.ExecuteNonQuery();

            command.Dispose();

            // 關閉連接
            conn.Close();
            SQLErrorMessage = i.ToString();
        }
        catch (Exception ex)
        {
            i = -1;
            SQLErrorMessage = ex.Message.ToString();
        }


        return i > 0;
    }


    public bool EditCaseTrsRFDMSend(string strTrnNum, string strID_No, string strProcessCode, string strFileName, string strProcessMsg)
    {
        string sql = @"UPDATE [CaseTrsRFDMSend] 
                            SET 
                                [ID_No] = @ID_No,
                                [RspCode] = @ProcessCode,
                                [RspMsg] = @ProcessMsg,
                                [FileName] = @FileName,
                                [ModifiedDate] = GETDATE(), 
                                [ModifiedUser] = 'RFDM' 
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
        sP = new SqlParameter("@ID_No", strID_No);
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
