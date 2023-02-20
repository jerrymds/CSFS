using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Text;
using System.Web.Script.Serialization;
using System.Web.Script.Services;
using System.Web.Services;

namespace CTBC.CSFS.WebService
{

  /// <summary>
  /// Summary description for ProcessResult
  /// </summary>
  [WebService(Namespace = "http://OpenFile")]
  [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
  [System.ComponentModel.ToolboxItem(false)]
  // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
  // [System.Web.Script.Services.ScriptService]
  public class OpenFileStream : System.Web.Services.WebService
  {
    // log 路徑
     private static string _logPath = ConfigurationManager.AppSettings["OpenFile_log_path"];

    /// <summary>
    /// 定義回傳物件
    /// </summary>
    public class FileStreamObject
    {
      public string Code { get; set; }
      public byte[] Content { get; set; }
      public double FileSize { get; set; }
    }

     /// <summary>
     /// 檢查檔案是否存在
     /// </summary>
     /// <param name="FullFilename"></param>
     /// <returns></returns>
    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public string FileExist(string FullFilename)
    {
       FileStreamObject returnFile = new FileStreamObject();

       WriteLog("FileExist service start ~~~~");

       WriteLog("傳入參數:Filename：[" + FullFilename + "]");

       try
       {
          // 若要開啟的檔案是否存在，才能繼續執行，否則就傳錯誤
          if (File.Exists(@FullFilename))
          {
             // 檔案存在
             returnFile.Code = "0000";
             returnFile.Content = Encoding.ASCII.GetBytes("檔案存在！");
          }
          else
          {
             // 檔案不存在
             returnFile.Code = "9999";
             returnFile.Content = Encoding.ASCII.GetBytes(String.Format("所要開啟的回文檔案[{0}]並不存在！", FullFilename));
          }

          WriteLog("Return : " + returnFile.Code);
          WriteLog("FileExist service end ~~~~");

       }
       catch (Exception ex)
       {
          //* 單筆出錯儘量不影響全部的查詢
          WriteLog("程式異常，錯誤信息: " + ex.Source);
          WriteLog("程式異常，錯誤信息: " + ex.ToString());
          WriteLog("程式異常，錯誤信息: " + ex.StackTrace);

          returnFile.Code = "8888";
          returnFile.Content = Encoding.ASCII.GetBytes("程式異常");

          WriteLog("Return : " + returnFile.Code);
       }

       return new JavaScriptSerializer().Serialize(returnFile);
    }

    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public string DeleteFile(string FullFilename)
    {
       FileStreamObject returnFile = new FileStreamObject();

       WriteLog("DeleteFile service start ~~~~");

       WriteLog("傳入參數:Filename：[" + FullFilename + "]");

       try
       {
          if (File.Exists(@FullFilename))
          {
             // 檔案存在
             File.Delete(@FullFilename);

             returnFile.Code = "0000";
             returnFile.Content = Encoding.ASCII.GetBytes("檔案已刪除！");

             WriteLog("Return : " + returnFile.Code);
          }

          WriteLog("DeleteFile service end ~~~~");

       }
       catch (Exception ex)
       {
          //* 單筆出錯儘量不影響全部的查詢
          WriteLog("程式異常，錯誤信息: " + ex.Source);
          WriteLog("程式異常，錯誤信息: " + ex.ToString());
          WriteLog("程式異常，錯誤信息: " + ex.StackTrace);

          returnFile.Code = "8888";
          returnFile.Content = Encoding.ASCII.GetBytes("程式異常");

          WriteLog("Return : " + returnFile.Code);
       }

       return new JavaScriptSerializer().Serialize(returnFile);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="FullFilename">全路徑檔名</param>
    /// <returns></returns>
    [WebMethod]
    //[ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public byte[] OpenFile(string FullFilename)
    {
      WriteLog("OpenFile service start ~~~~");

      WriteLog("傳入參數:Filename：[" + FullFilename + "]");

      byte[] file = null;

      try
      {
        // 若要開啟的檔案是否存在，才能繼續執行，否則就傳錯誤
        if (File.Exists(@FullFilename))
        {
          // 檔案存在
          file = File.ReadAllBytes(@FullFilename);

          WriteLog("Return : " + file.Length);
        }
      }
      catch (Exception ex)
      {
        //* 單筆出錯儘量不影響全部的查詢
        WriteLog("程式異常，錯誤信息: " + ex.Source);
        WriteLog("程式異常，錯誤信息: " + ex.ToString());
        WriteLog("程式異常，錯誤信息: " + ex.StackTrace);
      }

      return file;
    }

   /// <summary>
   /// 開啟壓縮檔 (將所列的檔案壓縮後回傳)
   /// </summary>
   /// <param name="DocNo"></param>
   /// <param name="FilePath"></param>
   /// <param name="FilenameList"></param>
   /// <returns></returns>
    [WebMethod]
    public byte[] OpenZipFile(string DocNo, string FilePath, List<string> FilenameList)
    {
       WriteLog("OpenZipFile service start ~~~~");

       string fileList = string.Empty;

       foreach (string item in FilenameList)
       {
          fileList += string.Format("{0},", item);
       }

       WriteLog("傳入參數:FilenameList：[" + fileList.Substring(0, fileList.Length - 1) + "]");

       byte[] file = null;

       try
       {
          // 臨時文件
          string zipPath = Server.MapPath("~/ZipTemp/");
          string zipFilename = zipPath + string.Format("{0}_{1:yyyyMMddHHmmssfff}.zip", DocNo, DateTime.Now);

          if (!Directory.Exists(zipPath))
          {
             Directory.CreateDirectory(zipPath);
          }

          // 壓縮密碼
          string password = "822822" + DocNo.Substring(1, DocNo.Length - 1);

          WriteLog("ZipFilename : " + zipFilename);

          CreateZip(FilePath, zipFilename, FilenameList, password);

          // 若要開啟的檔案是否存在，才能繼續執行，否則就傳錯誤
          if (File.Exists(@zipFilename))
          {
             // 檔案存在
             file = File.ReadAllBytes(@zipFilename);

             WriteLog("Return : " + file.Length);

             // 刪除暫時檔案
             File.Delete(zipFilename);
             WriteLog("OpenZipFile service end ~~~~");
          }
       }
       catch (Exception ex)
       {
          //* 單筆出錯儘量不影響全部的查詢
          WriteLog("程式異常，錯誤信息: " + ex.Source);
          WriteLog("程式異常，錯誤信息: " + ex.ToString());
          WriteLog("程式異常，錯誤信息: " + ex.StackTrace);
       }

       return file;
    }

    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public string GetFileSize(List<string> FilenameList)
    {
       WriteLog("GetFileSize service start ~~~~");

       FileStreamObject returnFile = new FileStreamObject();

       string fileList = string.Empty;
       double fileSize = 0;

       try
       {
          foreach (string item in FilenameList)
          {
             fileList += string.Format("{0},", item);

             if (File.Exists(item))
             {
                fileSize += new FileInfo(item).Length;
             }
             else
             {
                fileSize += 0;
             }
          }

          WriteLog("傳入參數:FilenameList：[" + fileList.Substring(0, fileList.Length - 1) + "]");

          WriteLog("File size : " + fileSize.ToString());

          returnFile.Code = "0000";
          returnFile.FileSize = fileSize;
       }
       catch (Exception ex)
       {
          //* 單筆出錯儘量不影響全部的查詢
          WriteLog("程式異常，錯誤信息: " + ex.Source);
          WriteLog("程式異常，錯誤信息: " + ex.ToString());
          WriteLog("程式異常，錯誤信息: " + ex.StackTrace);

          returnFile.Code = "8888";
          returnFile.Content = Encoding.ASCII.GetBytes("程式異常");
          returnFile.FileSize = 0;

          WriteLog("Return : " + returnFile.Code);
       }

       WriteLog("GetFileSize service end ~~~~");

       return new JavaScriptSerializer().Serialize(returnFile);
    }

    /// <summary>
    /// 壓縮多個文件
    /// </summary>
    /// <param name="sourceFilePath"></param>
    /// <param name="destinationZipFilePath"></param>
    public void CreateZip(string sourceFilePath, string destinationZipFilePath, List<string> files, string password)
    {
       if (sourceFilePath[sourceFilePath.Length - 1] != System.IO.Path.DirectorySeparatorChar)
          sourceFilePath += System.IO.Path.DirectorySeparatorChar;

       ZipOutputStream zipStream = new ZipOutputStream(File.Create(destinationZipFilePath));

       zipStream.SetLevel(6);  // 压缩级别 0-9
       zipStream.Password = password;

       foreach (string file in files)
       {
          FileStream stream = File.OpenRead(file);

          byte[] buffer = new byte[stream.Length];

          stream.Read(buffer, 0, buffer.Length);

          string tempFile = file.Substring(sourceFilePath.LastIndexOf("\\") + 1);
          
          ZipEntry entry = new ZipEntry(Path.GetFileName(file));

          entry.DateTime = DateTime.Now;
          entry.Size = stream.Length;
          stream.Close();

          zipStream.PutNextEntry(entry);

          zipStream.Write(buffer, 0, buffer.Length);

       }

       zipStream.Finish();
       zipStream.Close();

       GC.Collect();
       GC.Collect(1);
    }

     /// <summary>
    /// Write log file
    /// </summary>
    /// <param name="Message"></param>
    public void WriteLog(string Message)
    {
      // 判斷路徑是否存在，不存在創建路徑
      if (!Directory.Exists(_logPath))
      {
        Directory.CreateDirectory(_logPath);
      }

      // 每天一個檔
      string logFilename = string.Format("OpenFile_{0:yyyyMMdd}.log", DateTime.Now);

      // 記錄信息
      StreamWriter sw = File.AppendText(_logPath + logFilename);
      sw.WriteLine(string.Format("{0:yyyy/MM/dd HH:mm:ss.sss} : {1}", DateTime.Now, Message));
      sw.Flush();
      sw.Close();
    }

  }
}
