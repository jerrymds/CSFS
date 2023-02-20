/// <summary>
/// 記錄LOG
/// </summary>

using System;
using System.IO;
using System.Windows.Forms;
using System.Web;//20130418 hroace
using CTBC.CSFS.Resource;
using Microsoft.Practices.EnterpriseLibrary.Logging;//20130418 hroace
//using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;//20130330 hroace
using CTBC.CSFS.Pattern;//20130330 hroace
using CTBC.CSFS.Service.WebLog;//20130423 hroace
//using CTBC.FrameWork.Platform;

namespace CTBC.FrameWork.Util
{
	public class UtlLog
	{
		#region 全域變數

		// 預設記錄檔位置
		public string _defaultLogFile;

		#region add by mel 20120301

		/// <summary>
		/// 預設記錄檔位置
		/// </summary>
		public static string DefaultLogFile;

		#endregion

		#endregion

		#region 屬性設置(Get,Set)

		#endregion

		#region Public Method

		/// <summary>
		/// 寫入記錄檔
		/// </summary>
		/// <param name="psMessage">訊息內容</param>
		/// <param name="peMsgType">訊息種類</param>
		/// <remarks>add by sky 2011/12/08</remarks>
		public void WriteLog(string psMessage, CTBC.FrameWork.Util.Common.LogType peMsgType)
		{
			// 如果不存在地址
            if (_defaultLogFile == null || _defaultLogFile == Lang.csfs_notfind)
			{
				_defaultLogFile = Application.StartupPath + "\\Logs.log";
			}

			// 目錄
			string logDirectory = _defaultLogFile.Replace(_defaultLogFile.Substring(_defaultLogFile.LastIndexOf("\\")), "");

			string filename = _defaultLogFile.Substring(_defaultLogFile.LastIndexOf("\\")).Replace("\\", "");

			// 如果不包括後綴
			if (!filename.ToUpper().Contains(".LOG"))
			{
				_defaultLogFile = _defaultLogFile + ".LOG";
			}

			// 如果不存在該目錄則建立
			if (!UtlFileSystem.FolderIsExist(logDirectory))
			{
				Directory.CreateDirectory(logDirectory);
			}

			// 建立文件
            //FileInfo info = new FileInfo(_defaultLogFile);

            // 如果不存在
            //if (!info.Exists)
            //{
            //    info.Create();
            //}
			WriteLog(psMessage, peMsgType, _defaultLogFile);
            if (peMsgType == Common.LogType.Error)
            {
                WriteLogByLogService(psMessage, "Error", _defaultLogFile);//20130424 horace 寫入DB Table:CSFSLog 與發alert mail
            }
        }

        /// <summary>
        /// 透過Log Service 寫Log到CSFSLog
        /// </summary>
        /// <param name="msg">Log 訊息</param>
        /// <param name="msgType">Log Type</param>
        /// <param name="module">Log來源模組</param>
        /// 20130423 horace
        public void WriteLogByLogService(string psMessage, string peMsgType, string _defaultLogFile)
        {
            Log _wLog = new Log();
            _wLog.WriteLogByLogService(psMessage, peMsgType, _defaultLogFile);//20130423 horace 寫入DB Table:CSFSLog 與發alert mail    
        }

        /// <summary>
        /// 轉換MsgType to String
        /// </summary>
        /// <param name="peMsgType"></param>
        /// <returns></returns>
        /// 20130423 horace
        public string conType(Common.LogType peMsgType)
        {
            switch (peMsgType)
            {
                case CTBC.FrameWork.Util.Common.LogType.Information: return "Information";
                    break;
                case CTBC.FrameWork.Util.Common.LogType.Warning: return "Warning";
                    break;
                case CTBC.FrameWork.Util.Common.LogType.Error: return "Error";
                    break;
                default: return "Information";
                    break;
            }        
        }
		/// <summary>
		/// 寫入記錄檔
		/// </summary>
		/// <param name="psMessage">訊息內容</param>
		/// <param name="peMsgType">訊息種類</param>
		/// <param name="psLogFile">記錄檔全路徑</param>
		/// <remarks>add by sky 2011/12/08</remarks>
		public static void WriteLog(string psMessage, CTBC.FrameWork.Util.Common.LogType peMsgType, string psLogFile)
		{
			string sMsgType;

			switch (peMsgType)
			{
                case CTBC.FrameWork.Util.Common.LogType.Information: sMsgType = "----";
					break;
                case CTBC.FrameWork.Util.Common.LogType.Warning: sMsgType = Lang.csfs_warning;
					break;
                case CTBC.FrameWork.Util.Common.LogType.Error: sMsgType = Lang.csfs_err;
					break;
				default: sMsgType = "----";
					break;
			}

			// 向文件中寫LOG信息
			for (int intReTry = 0; intReTry < 10; intReTry++)
			{
				try
				{
					using (StreamWriter oLogFile = File.AppendText(psLogFile))
					{
						DateTime dtmNow = DateTime.Now;
						oLogFile.WriteLine("{0} {1} {2}", dtmNow.ToString("yyyy/MM/dd HH:mm:ss") + ":" + dtmNow.Millisecond.ToString("000"), sMsgType, psMessage);

						oLogFile.Flush();

						oLogFile.Close();
					}

					break;
				}
				catch
				{
				}
			}
		}

        /// <summary>
        /// 寫到DB
        /// </summary>
        /// <param name="psMessage"></param>
        /// <param name="peMsgType"></param>
        /// 20130418 hroace
        public static void WriteLogToDB(string msg)
        {
            try
            {
                CSFSLog log = new CSFSLog();
                log.Categories.Add("CSFS");
                log.Message = msg;
                log.TimeStamp = DateTime.Now;
                log.Title = "CSFS";
                log.Priority = 1;
                log.EventId = 105;
                log.Severity = System.Diagnostics.TraceEventType.Information;
                if (HttpContext.Current != null)
                {
                    log.dic["UserId"] = (HttpContext.Current.Session["UserAccount"] != null) ? HttpContext.Current.Session["UserAccount"].ToString() : "Anonymous"; ;
                    log.dic["SessionId"] = HttpContext.Current.Session.SessionID;
                    log.dic["URL"] = HttpContext.Current.Request.RawUrl;
                    log.dic["IP"] = HttpContext.Current.Request.UserHostAddress;
                    log.dic["MachineName"] = HttpContext.Current.Request.UserHostName;
                    log.dic["Result"] = Result.Success;
                }
                else {
                    string tm = "Lose Session";
                    log.dic["UserId"] = "Anonymous";
                    log.dic["SessionId"] = tm;
                    log.dic["URL"] = tm;
                    log.dic["IP"] = tm;
                    log.dic["MachineName"] = tm;
                    log.dic["Result"] = Result.Failure;
                }
                log.dic["ActionCode"] = ActionCode.Update;
                log.dic["TranFlag"] = TranFlag.After;
                log.dic["FunctionId"] = "";
                log.ExtendedProperties = log.dic;
                Logger.Write(log);
                log.Categories.Remove("CSFS");
            }
            catch (Exception ex) { throw ex; }
        }

        #region add by mel 20120301

        /// <summary>
		/// web method 使用
		/// </summary>
		/// <param name="webservicename"></param>
		/// <param name="log_str"></param>
		public static void WebWritelog(string webservicename, string log_str)
		{
			string log_path = "";
			string log_name = "";
			DirectoryInfo di;

			try
			{
				log_path = System.Web.HttpContext.Current.Request.PhysicalApplicationPath + @"log\" + String.Format(@"{0:yyyy\\MM\\dd}\\", System.DateTime.Now);
				log_name = log_path + webservicename + "_" + string.Format("{0:yyyyMMdd}", System.DateTime.Now) + ".log";
				if (!Directory.Exists(log_path))
				{
					di = Directory.CreateDirectory(log_path);

				}

				StreamWriter ologfile = File.AppendText(log_name);
				ologfile.WriteLine(String.Format("{0:yyyy-MM-dd HH:mm:ss.ffff}  {1} ", System.DateTime.Now, log_str));
				ologfile.Flush();
				ologfile.Close();



			}
			catch (Exception e)
			{
				throw e;
			}

		}

		/// <summary>
		/// 寫入一行文字至文字檔中
		/// </summary>
		/// <param name="psText">寫入文字</param>
		/// <param name="psLogFile">文字檔位置</param>
		public static void WriteTextFile(string psText, string psLogFile)
		{
			try
			{
				using (StreamWriter oLogFile = File.AppendText(psLogFile))
				{
					oLogFile.WriteLine(psText);
					oLogFile.Flush();	// Update the underlying file.
					oLogFile.Close();
				}
			}
			catch (Exception e)
			{
				throw e;

			}
		}

		/// <summary>
		/// 寫入多行文字至文字檔中
		/// </summary>
		/// <param name="psLineArray">行字串陣列</param>
		/// <param name="psLogFile">文字檔位置</param>
		public static void WriteTextFile(string[] psLineArray, string psLogFile)
		{
			try
			{
				using (StreamWriter oLogFile = File.AppendText(psLogFile))
				{
					for (int k = 0; k < psLineArray.Length; k++)
					{
						oLogFile.WriteLine(psLineArray[k]);
						oLogFile.Flush();	// Update the underlying file.
					}
					oLogFile.Close();
				}
			}
			catch (Exception e)
			{
				throw e;

			}
		}

		/// <summary>
		/// 呼叫作業系統預設的程式(Ex:Notepad.exe)顯示預設的記錄檔案內容
		/// </summary>
		public static void DisplayLog()
		{
			if (DefaultLogFile == null) DefaultLogFile = GetDefLogFile();
			DisplayLog(DefaultLogFile);
		}

		/// <summary>
		/// 呼叫作業系統預設的程式(Ex:Notepad.exe)顯示記錄檔案內容
		/// </summary>
		/// <param name="psLogFile">記錄檔完整路徑</param>
		public static void DisplayLog(string psLogFile)
		{
			UtlMain.Shell(psLogFile);
		}

		/// <summary>
		/// 取得預設記錄檔位置
		/// </summary>
		/// <returns>記錄檔全路徑</returns>
		public static string GetDefLogFile()
		{
			return UtlMain.GetAppPath("LOG");
		}

		#endregion

		#endregion

		#region Private Method

		#endregion
	}
}
