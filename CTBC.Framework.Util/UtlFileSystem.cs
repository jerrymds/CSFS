/// <summary>
/// 檔案系統類別
/// </summary>

using System.IO;

namespace CTBC.FrameWork.Util
{
    public class UtlFileSystem
    {
        #region 全域變數

        #endregion

        #region 屬性設置(Get,Set)

        #endregion

        #region Public Method

        /// <summary>
        /// 刪除檔案
        /// </summary>
        /// <param name="psFileLoc">刪除檔案路徑</param>
        /// <returns>True/False</returns>
        /// <remarks>add by sky 2011/12/08</remarks>
        public static bool DeleteFile(string psFileLoc)
        {
            try
            {
                File.Delete(psFileLoc);

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 取得檔案大小(Bytes)
        /// </summary>
        /// <param name="psFilePath">檔案全路徑</param>
        /// <returns>檔案大小</returns>
        /// <remarks>add by sky 2011/12/08</remarks>
        public static long GetFileSize(string psFilePath)
        {
            try
            {
                FileInfo oFileInfo = new FileInfo(psFilePath);

                return oFileInfo.Length;
            }
            catch
            {

                return 0;
            }
        }

        /// <summary>
        /// 由檔案名稱(或全路徑)取得副檔名(含.符號)
        /// </summary>
        /// <param name="psFilePath">檔案名稱</param>
        /// <returns>副檔名</returns>
        /// <remarks>add by sky 2011/12/08</remarks>
        public static string GetFileExt(string psFilePath)
        {
            return Path.GetExtension(psFilePath);
        }

        /// <summary>
        /// 由檔案名稱(或全路徑)取得主檔名(不含副檔名)
        /// </summary>
        /// <param name="psFilePath">檔案名稱</param>
        /// <returns>主檔名</returns>
        /// <remarks>add by sky 2011/12/08</remarks>
        public static string GetFileNameNoExt(string psFilePath)
        {
            return Path.GetFileNameWithoutExtension(psFilePath);
        }

        /// <summary>
        /// 由檔案或目錄路徑取得所在目錄全路徑
        /// </summary>
        /// <param name="psPath">全路徑</param>
        /// <returns>目錄全路徑</returns>
        /// <remarks>add by sky 2011/12/08</remarks>
        public static string GetFolderName(string psPath)
        {
            return Path.GetDirectoryName(psPath);
        }

        /// <summary>
        /// 連接路徑字串, 不論結尾是否含有目錄分隔符號
        /// </summary>
        /// <param name="psRootPath">根路徑</param>
        /// <param name="psSubPath">子路徑</param>
        /// <returns>完整路徑</returns>
        /// <remarks>add by sky 2011/12/08</remarks>
        public static string JoinPath(string psRootPath, string psSubPath)
        {
            // 如果最後一給是‘\’時
            if (psRootPath[psRootPath.Length - 1] == Path.DirectorySeparatorChar)
            {
                return psRootPath + psSubPath;
            }
            else
            {
                return psRootPath + Path.DirectorySeparatorChar + psSubPath;
            }
        }

        /// <summary>
        /// 檢查目錄是否存在
        /// </summary>
        /// <param name="psFolderPath">檢查目錄路徑</param>
        /// <returns>True/False (錯誤時回傳false)</returns>
        /// <remarks>add by sky 2011/12/08</remarks>
        public static bool FolderIsExist(string psFolderPath)
        {
            return Directory.Exists(psFolderPath);
        }

        /// <summary>
        /// 複製檔案
        /// </summary>
        /// <param name="psSource">來源檔案路徑</param>
        /// <param name="psTarget">目的檔案路徑</param>
        /// <param name="pbOverwrite">是否允許覆寫檔案</param>
        /// <returns>True/False</returns>
        /// <remarks>add by sky 2011/12/14</remarks>
        public static bool CopyFile(string psSource, string psTarget, bool pbOverwrite)
        {
            try
            {
                File.Copy(psSource, psTarget, pbOverwrite);

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 刪除目錄
        /// </summary>
        /// <param name="psFolderPath">目錄路徑</param>
        /// <param name="pbRecurive">是否刪除所有子目錄或及檔案(遞迴刪除)</param>
        /// <returns>True/False</returns>
        /// <remarks>add by sky 2011/12/08</remarks>
        public static bool DeleteFolder(string psFolderPath, bool pbRecurive)
        {
            try
            {
                Directory.Delete(psFolderPath, pbRecurive);

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 建立目錄
        /// </summary>
        /// <param name="psFolderPath">目錄路徑</param>
        /// <returns>True/False</returns>
        /// <remarks>add by sky 2011/12/13</remarks>
        public static bool CreateFolder(string psFolderPath)
        {
            try
            {
                Directory.CreateDirectory(psFolderPath);

                return true;
            }
            catch
            {
                return false;
            }
        }
        #endregion

        #region Private Method

        #endregion
    }
}
