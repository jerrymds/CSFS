using System;

using System.Diagnostics;
using System.IO;
using System.Configuration;
using System.Windows.Forms;

namespace CTBC.FrameWork.Util
{
	/// <summary>
	/// 常用函式類別
	/// </summary>
	/// <remarks>版本日期 : 2006/08/12</remarks>
	public class UtlMain
	{

		/// <summary>
		/// 將視窗位置置中
		/// </summary>
		/// <param name="frmObject">視窗物件</param>
        //public static void CenterForm(Form frmObject)
        //{
        //    int iXPos=(Screen.PrimaryScreen.Bounds.Width-frmObject.Size.Width)/2;
        //    int iYPos=(Screen.PrimaryScreen.Bounds.Height-frmObject.Size.Height)/2;
        //    frmObject.Location = new System.Drawing.Point(iXPos, iYPos);
        //}

		/// <summary>
		/// 連接路徑字串, 不論結尾是否含有目錄分隔符號
		/// </summary>
		/// <param name="psRootPath">根路徑</param>
		/// <param name="psSubPath">子路徑</param>
		/// <returns>完整路徑</returns>
		public static string JoinPath(string psRootPath, string psSubPath)
		{
			if (psRootPath[psRootPath.Length-1] == Path.DirectorySeparatorChar) 
				return psRootPath + psSubPath;
			else
				return psRootPath + Path.DirectorySeparatorChar + psSubPath;
		}

		/// <summary>
		/// 取得應用程式名稱(不含副檔名)
		/// </summary>
		/// <returns>應用程式名稱</returns>
		public static string GetAppName()
		{
			string sAppName = AppDomain.CurrentDomain.FriendlyName;
			return UtlString.Left(sAppName, sAppName.Length - 4);
		}

		/// <summary>
		/// 取得應用程式路徑(1)
		/// </summary>
		/// <returns>應用程式路徑(不含執行檔名)</returns>
		public static string GetAppPath()
		{
            //固定服務紀錄的位置
            if (Application.StartupPath.Contains("CTCB.WII.Service"))
            {
                System.IO.DirectoryInfo startUp = new DirectoryInfo(Application.StartupPath);

                string path = startUp.Parent.FullName + Path.DirectorySeparatorChar + "LOGS" + Path.DirectorySeparatorChar + GetAppName();

                if (!System.IO.Directory.Exists(path)) { System.IO.Directory.CreateDirectory(path); }

                return path + Path.DirectorySeparatorChar;
            }
            else
            {
                return Application.StartupPath + Path.DirectorySeparatorChar;
            }
		}
		/// <summary>
		/// 取得應用程式路徑(2)
		/// </summary>
		/// <param name="pbWithExeName">是否含執行檔名</param>
		/// <returns>應用程式路徑(執行檔名)</returns>
		public static string GetAppPath(bool pbWithExeName)
		{
            if (GetString(AppDomain.CurrentDomain.DynamicDirectory).ToUpper().Contains("ASP.NET"))
            {
                string[] strFriendlyName = AppDomain.CurrentDomain.FriendlyName.Split(new string[] { "/" }, StringSplitOptions.None);
                string[] strAppName = strFriendlyName[strFriendlyName.Length - 1].Split(new string[] { "-" }, StringSplitOptions.None);

                return AppDomain.CurrentDomain.BaseDirectory + "\\LOG\\" + strAppName[0] + "123";
            }
            else 
            {
                if (pbWithExeName)
                {
                    return GetAppPath() + AppDomain.CurrentDomain.FriendlyName;
                }
                else
                {
                    return GetAppPath();
                }
            }
		}

        public static string GetString(object objValue)
        {
            string strValue = "";

            if (objValue != null)
            {
                if (objValue != DBNull.Value)
                {
                    strValue = objValue.ToString().Trim();
                }
            }

            return strValue;
        }

		/// <summary>
		/// 取得應用程式路徑(3)
		/// </summary>
		/// <param name="psExtName">自定副檔名</param>
		/// <returns>應用程式路徑+自定副檔名</returns>
        /// 判斷是這個月是否有檔案
		public static string GetAppPath(string psExtName)
		{
            DateTime time = DateTime.Now;
			string sTemp = GetAppPath(true);
            string fileName = UtlString.Left(sTemp, sTemp.Length - 3) + "_" + time.ToString("dd") + "." + psExtName;
            FileInfo info = new FileInfo(fileName);

            if (info.Exists)
            {
                if (info.LastWriteTime.Month != time.Month) { info.Delete(); }
            }

            return fileName;
		}

		/// <summary>
		/// 列印除錯訊息(1)
		/// </summary>
		/// <param name="poPrintObject">訊息物件</param>
		public static void DebugPrint(object poPrintObject)
		{
			// 改用Listeners[0]在其他程式呼叫時才會正常顯示於Debug視窗
			Debug.Listeners[0].WriteLine(poPrintObject);
		}
		/// <summary>
		/// 列印除錯訊息(2)
		/// </summary>
		/// <param name="poPrintObject">訊息物件</param>
		/// <param name="pbPrintTime">是否顯示時間</param>
		public static void DebugPrint(object poPrintObject, bool pbPrintTime)
		{
			if (pbPrintTime)
				DebugPrint(DateTime.Now + " " + poPrintObject.ToString());
			else
				DebugPrint(poPrintObject);
		}

		/// <summary>
		/// 執行外部應用程式(1)
		/// </summary>
		/// <param name="psExeFilePath">執行檔位置</param>
        public static void Shell(string psExeFilePath)
        {
            try
            {
                Process.Start(psExeFilePath);
            }
            catch (Exception e)
            {
                throw e;
                //UtlDialog.ShowErrorMsg(e, "< 執行檔 : > " + psExeFilePath);
            }
        }
		/// <summary>
		/// 執行外部應用程式(2)
		/// </summary>
		/// <param name="psExeFilePath">執行檔位置</param>
		/// <param name="psArguments">參數字串</param>
        public static void Shell(string psExeFilePath, string psArguments)
        {
            try
            {
                Process.Start(psExeFilePath, psArguments);
            }
            catch (Exception e)
            {
                throw e;
                //UtlDialog.ShowErrorMsg(e, "< 執行檔 : > " + psExeFilePath + " < 參數 : > " + psArguments);
            }
        }

		/// <summary>
		/// 檢查相同的應用程式是否已啟動
		/// </summary>
		/// <returns>True/False</returns>
		public static bool PreInstance()
		{
			Process oProcess = Process.GetCurrentProcess();
			string sProcName = oProcess.ProcessName;

			if (Process.GetProcessesByName(sProcName).Length > 1)
				return true;
			else
				return false;
		}

		/// <summary>
		/// 讀取應用程式設定值
		/// </summary>
		/// <param name="psKey">設定鍵值</param>
		/// <returns>設定值</returns>
		/// <remarks>讀取App.config檔案中[configuration][appSettings]的設定值(與Dynamic Properties共用設定區塊)</remarks>
		public static string GetAppSetting(string psKey)
		{
			return ConfigurationSettings.AppSettings.Get(psKey);
		}

		/// <summary>
		/// 由Exception物件組成錯誤訊息字串
		/// </summary>
		/// <param name="e">Exception物件</param>
		/// <returns>錯誤訊息</returns>
		public static string ComposeErrMsg(Exception e)
		{
			return e.GetHashCode().ToString() + " : " + e.ToString();
		}

	}


}