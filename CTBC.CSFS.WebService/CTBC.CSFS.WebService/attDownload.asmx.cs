using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Configuration;

namespace CTBC.CSFS.WebService
{
    /// <summary>
    ///attDownload 的摘要描述
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // 若要允許使用 ASP.NET AJAX 從指令碼呼叫此 Web 服務，請取消註解下列一行。
    // [System.Web.Script.Services.ScriptService]
    public class attDownload : System.Web.Services.WebService
    {
        // 下載路徑要與原本web 路徑一致
        private static string _DownFile_RootPath = ConfigurationManager.AppSettings["DownFile_Root_path"];
        // log 路徑 與 OpenFile 共用
        private static string _logPath = ConfigurationManager.AppSettings["log_path"];

        [WebMethod]
        public string UrlDownloadFile(string AttachmentServerPath, string FName)
        {
            WriteLog("DownloadFileExist service start ~~~~");

            WriteLog("傳入參數:ServerPath：" + AttachmentServerPath + " Filename:"+ FName + "]");
            System.IO.FileStream fs1 = null;
            var rootPath = Server.MapPath("~");
            string fl = _DownFile_RootPath+AttachmentServerPath.TrimEnd().Replace("/","\\").Replace("~","") + "\\" + FName;
            WriteLog("下載實體檔名：[" + fl + "]");
            // 若要開啟的檔案是否存在，才能繼續執行，否則就傳錯誤
            try
            {
                if (File.Exists(fl))
                {
                    // 檔案存在
                    fs1 = System.IO.File.Open(fl, FileMode.Open, FileAccess.Read);
                    byte[] b1 = new byte[fs1.Length];
                    fs1.Read(b1, 0, (int)fs1.Length);
                    string base64String = Convert.ToBase64String(b1, 0, b1.Length);
                    fs1.Close();
                    return base64String;
                }
                else
                {
                    return "";
                }
            }
            catch (Exception ex)
            {
                //* 單筆出錯儘量不影響全部的查詢
                WriteLog("程式異常，錯誤信息: " + ex.ToString());
                return "";
            }
        }
        public void WriteLog(string Message)
        {
            // 判斷路徑是否存在，不存在創建路徑
            if (!Directory.Exists(_logPath))
            {
                Directory.CreateDirectory(_logPath);
            }

            // 每天一個檔
            string logFilename = string.Format("DownLoadFile_{0:yyyyMMdd}.log", DateTime.Now);

            // 記錄信息
            StreamWriter sw = File.AppendText(_logPath + logFilename);
            sw.WriteLine(string.Format("{0:yyyy/MM/dd HH:mm:ss.sss} : {1}", DateTime.Now, Message));
            sw.Flush();
            sw.Close();
        }
    }
}
