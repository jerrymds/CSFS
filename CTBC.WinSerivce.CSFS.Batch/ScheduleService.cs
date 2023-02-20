using System.ServiceProcess;
using System.IO;
using System.Configuration;
using System.Diagnostics;
using System;
using System.Timers;
using System.Collections.Generic;

namespace CTBC.WinSerivce.CSFS.Batch
{
   partial class ScheduleService : ServiceBase
   {
      Dictionary<int, ExecuteInfo> _source = new Dictionary<int, ExecuteInfo>();
      Timer _jobTimer = new Timer();

      int _activeTime = 0;

      public ScheduleService()
      {
         InitializeComponent();
      }

      protected override void OnStart(string[] args)
      {
         LogService("INFO", "OnStart() : Windows Service 正在被啟動執行中 ~~~~~~~~~~");

         Boolean errorFlag = false;

         // 取得各批次程式的程式名稱(含位置)及執行間隔時間
         try
         {
            LogService("INFO", "OnStart() : 開始取得環境參數資料 ~~~~~~~~~~");

            int i = 0;

            // 取得定時啟動Job的時間，以分鐘計
            _activeTime = Convert.ToInt16(ConfigurationManager.AppSettings["ActiveTime"]);

            LogService("INFO", string.Format("OnStart() : 定時啟動Job的時間(以分鐘計) = {0}分鐘", _activeTime));
            
            // 取得要被執行的批次程式資訊
            _source.Add(i++, new ExecuteInfo
            {
               FullFilename = ConfigurationManager.AppSettings["FTPdown.Filename"]
            });

            _source.Add(i++, new ExecuteInfo
            {
               FullFilename = ConfigurationManager.AppSettings["HTG.Filename"]
            });

            _source.Add(i++, new ExecuteInfo
            {
               FullFilename = ConfigurationManager.AppSettings["RFDMPut.Filename"]
            });

            _source.Add(i++, new ExecuteInfo
            {
               FullFilename = ConfigurationManager.AppSettings["RFDMGet.Filename"]
            });

            _source.Add(i++, new ExecuteInfo
            {
               FullFilename = ConfigurationManager.AppSettings["ReturnFile.Filename"]
            });

            LogService("INFO", "OnStart() : 本服務要被啟動的批次程式列表如下：");

            foreach (KeyValuePair<int, ExecuteInfo> item in _source)
            {
               LogService("INFO", string.Format("OnStart() : 批次程式檔名 : {0}", item.Value.FullFilename));

            }

            LogService("INFO", "OnStart() : 啟動批次排程作業");

            _jobTimer.Interval = 60000 * _activeTime;
            _jobTimer.Elapsed += new ElapsedEventHandler(this.JobTimer_Elapsed);
            _jobTimer.AutoReset = true;
            _jobTimer.Start();

         }
         catch (Exception ex)
         {
            LogService("ERROR", string.Format("程式執行錯誤，相關錯誤訊息如下 : \n\tSource: [{0}]\n\tException: [{1}]\n\tStackTrace: [{2}]", ex.Source, ex.InnerException, ex.StackTrace));

         }

         LogService("INFO", string.Format("OnStart() : Windows Service 已被正啟動{0} ~~~~~~~~~~", (errorFlag ? "，但執行有錯誤產生" : "")));

      }

      protected override void OnStop()
      {
         LogService("INFO", "OnStop() : Windows Service 將被終止執行中 ~~~~~~~~~~");

         _jobTimer.Stop();

         LogService("INFO", "OnStop() : Windows Service 已被終止執行 ~~~~~~~~~~");
      }

      /// <summary>
      /// 定時啟動各批次程式
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      void JobTimer_Elapsed(Object sender, ElapsedEventArgs e)
      {
         LogService("INFO", "JobTimer_Elapsed() : 正要啟動外部批次程式");

         try
         {
            foreach (KeyValuePair<int, ExecuteInfo> item in _source)
            {
               ExecuteInfo FileInfo = GetFileInfo(item.Value.FullFilename);

               LogService("INFO", string.Format("JobTimer_Elapsed() : 檢查批次執行檔[{0}]是否存在？ = [{1}]", FileInfo.FullFilename, File.Exists(FileInfo.FullFilename)));

               // 判斷外部批次程式是否正在執行中，若有就不執行
               if (!CheckProcess(FileInfo.Filename))
               {
                  // 執行外部批次程式
                  Process execute = new Process();

                  if (File.Exists(FileInfo.FullFilename))
                  {
                     LogService("INFO", string.Format("JobTimer_Elapsed() : 批次程式[{0}]將被啟動執行", FileInfo.FullFilename));

                     execute.StartInfo.FileName = FileInfo.FullFilename;
                     execute.StartInfo.WorkingDirectory = FileInfo.Path;
                     Boolean status = execute.Start();

                     LogService("INFO", string.Format("JobTimer_Elapsed() : 批次程式[{0}]是否正常啟動執行？ [{1}]", FileInfo.FullFilename, status));

                  }
                  else
                  {
                     LogService("ERROR", string.Format("JobTimer_Elapsed() : 無法找到批次執行程式檔案[{0}]所在目錄或檔案！", FileInfo.FullFilename));

                  }
               }
            }

         }
         catch (Exception ex) {
            LogService("ERROR", string.Format("程式執行錯誤，相關錯誤訊息如下 : \n\tSource: [{0}]\n\tException: [{1}]\n\tStackTrace: [{2}]", ex.Source, ex.InnerException, ex.StackTrace));

         }

         LogService("INFO", "JobTimer_Elapsed() : 結束啟動外部批次程式");

      }

      /// <summary>
      /// 檢查批次程式是否正在執行中
      /// </summary>
      /// <param name="executeName"></param>
      /// <returns></returns>
      Boolean CheckProcess(string executeName)
      {
         Process[] process = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(executeName));

         LogService("INFO", string.Format("CheckProcess() : 檢查外部批次程式[{0}]是否尚在執行中 ? {1}", executeName, (process.Length > 0).ToString()));

         return (process.Length > 0);
      }

      /// <summary>
      /// Logging
      /// </summary>
      /// <param name="level">層級名稱</param>
      /// <param name="content">訊息內容</param>
      private void LogService(string level, string content)
      {
         string logPath = ConfigurationManager.AppSettings["LogPath"];
         string logFilename = String.Format("{0}\\{1}_{2:yyyyMMdd}.log", logPath, Process.GetCurrentProcess().ProcessName, DateTime.Today);

         if (!Directory.Exists(logPath))
         {
            Directory.CreateDirectory(@logPath);
         }

         FileStream fs = new FileStream(@logFilename, FileMode.OpenOrCreate, FileAccess.Write);
         StreamWriter sw = new StreamWriter(fs);

         sw.BaseStream.Seek(0, SeekOrigin.End);
         sw.WriteLine(string.Format("{0:HH:mm:ss.sss} - {1} : {2}", DateTime.Now, level, content));
         sw.Flush();
         sw.Close();
      }

      /// <summary>
      /// 區分目錄與檔名
      /// </summary>
      /// <param name="filename">原始包含目錄與檔名</param>
      /// <returns>檔案資料物件</returns>
      private ExecuteInfo GetFileInfo(string source)
      {
         ExecuteInfo info = new ExecuteInfo();

         int position = source.LastIndexOf('\\');

         info.FullFilename = source;
         info.Path = source.Substring(0, position);
         info.Filename = source.Substring(position + 1);

         #region Output information for DEBUG
#if (DEBUG)
         LogService("DEBUG", string.Format("外部程式檔案所在目錄位置 : ({0})", info.Path));
         LogService("DEBUG", string.Format("外部程式檔案名稱 : ({0})", info.Filename));
#endif
         #endregion

         return info;
      }

   }

   /// <summary>
   /// 記錄批次程式檔啟動資訊
   /// </summary>
   class ExecuteInfo
   {
      public string FullFilename { get; set; }
      public string Path { get; set; }
      public string Filename { get; set; }

   }

   /// <summary>
   /// 繼承Timer元件，增加記錄要被啟動的程式資訊
   /// </summary>
   class NewTimer : Timer
   {
      public ExecuteInfo ExecuteInfo { get; set; }
   }

}
