using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;


namespace CTBC.FrameWork.Util
{
    public class FtpClient
    {
        public string ftpHost;
        public string ftpAccount;
        public string ftpPassword;
        private FtpWebRequest ftpRequest = null;
        private FtpWebResponse ftpResponse = null;
        private Stream ftpStream = null;
        private int bufferSize = 2048;

        //string FTP_Server = string.Empty, FTP_Port = string.Empty, FTP_UserID = string.Empty, FTP_Password = string.Empty, FTP_FilePath = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sHost">主機</param>
        /// <param name="sAccount">用戶名</param>
        /// <param name="sPassword">密碼</param>
        /// <param name="sPort">端口號</param>
        public FtpClient(string sHost, string sAccount, string sPassword, string sPort)
        {
            this.ftpHost = "FTP://" + sHost + ":" + sPort;
            this.ftpAccount = sAccount;
            this.ftpPassword = sPassword;
            //this.ftpPassword = UtlString.DecodeBase64(sPassword);
        }

        /// <summary>
        /// 獲取FTP單一文件
        /// </summary>
        /// <param name="sRemoteFile">遠端文檔名(含路徑)</param>
        /// <param name="sLocalFile">本地文檔名(含路徑)</param>
        /// <returns></returns>
        public void GetFiles(string sRemoteFile, string sLocalFile)
        {
            ftpRequest = (FtpWebRequest)FtpWebRequest.Create(ftpHost + SetRemotePath(sRemoteFile));
            ftpRequest.Credentials = new NetworkCredential(ftpAccount, ftpPassword);
            ftpRequest.UseBinary = true;
            ftpRequest.UsePassive = true;
            ftpRequest.KeepAlive = true;
            ftpRequest.Method = WebRequestMethods.Ftp.DownloadFile;
            ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();
            ftpStream = ftpResponse.GetResponseStream();
            FileStream localFileStream = new FileStream(sLocalFile, FileMode.Create, FileAccess.Write);
            byte[] byteBuffer = new byte[bufferSize];
            int bytesRead = ftpStream.Read(byteBuffer, 0, bufferSize);
            try
            {
                while (bytesRead > 0)
                {
                    localFileStream.Write(byteBuffer, 0, bytesRead);
                    bytesRead = ftpStream.Read(byteBuffer, 0, bufferSize);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                localFileStream.Close();
                ftpStream.Close();
                ftpResponse.Close();
                ftpRequest = null;
            }
        }

        public void SendFile(string sRemoteFile, string fileName, byte[] file)
        {
            try
            {
                ftpRequest = (FtpWebRequest)FtpWebRequest.Create(ftpHost + SetRemotePath(sRemoteFile) + "//" + fileName);
                ftpRequest.Credentials = new NetworkCredential(ftpAccount, ftpPassword);
                ftpRequest.UseBinary = true;
                ftpRequest.UsePassive = true;
                ftpRequest.KeepAlive = true;
                ftpRequest.Method = WebRequestMethods.Ftp.UploadFile;
                ftpStream = ftpRequest.GetRequestStream();
                try
                {
                    ftpStream.Write(file, 0, file.Length);
                    ftpStream.Flush();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    ftpStream.Close();
                    ftpRequest = null;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 將本地指定文檔傳送到FTP指定目錄
        /// </summary>
        /// <param name="sLocalFile">本地文檔名(含全路徑)</param>
        /// <param name="sRemoteFile">遠端目錄名稱</param>
        /// <returns></returns>
        public void SendFile(string sRemoteFile, string sLocalFile)
        {
            try
            {
                ftpRequest = (FtpWebRequest)FtpWebRequest.Create(ftpHost + SetRemotePath(sRemoteFile) + "//" + new FileInfo(sLocalFile).Name);
                ftpRequest.Credentials = new NetworkCredential(ftpAccount, ftpPassword);
                ftpRequest.UseBinary = true;
                ftpRequest.UsePassive = true;
                ftpRequest.KeepAlive = true;
                ftpRequest.Method = WebRequestMethods.Ftp.UploadFile;
                ftpStream = ftpRequest.GetRequestStream();
                FileStream localFileStream = new FileStream(sLocalFile, FileMode.Open);
                byte[] byteBuffer = new byte[bufferSize];
                int bytesSent = localFileStream.Read(byteBuffer, 0, bufferSize);
                try
                {
                    while (bytesSent != 0)
                    {
                        ftpStream.Write(byteBuffer, 0, bytesSent);
                        bytesSent = localFileStream.Read(byteBuffer, 0, bufferSize);
                    }
                    ftpStream.Flush();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    localFileStream.Close();
                    ftpStream.Close();
                    ftpRequest = null;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 將本地指定目錄中所有文檔傳到FTP
        /// </summary>
        /// <param name="sRemoteFile"></param>
        /// <param name="sLocalDir"></param>
        /// <returns></returns>
        public void SendFiles(string sRemoteFile, string sLocalDir)
        {
            try
            {
                string[] fileList = Directory.GetFiles(sLocalDir);
                foreach (string file in fileList)
                {
                    SendFile(sRemoteFile, file);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 刪除FTP上的文檔
        /// </summary>
        /// <param name="sRemoteFile">遠端文檔名(含路徑)</param>
        public void DeleteFile(string sRemoteFile)
        {
            try
            {
                ftpRequest = (FtpWebRequest)WebRequest.Create(ftpHost + SetRemotePath(sRemoteFile));
                ftpRequest.Credentials = new NetworkCredential(ftpAccount, ftpPassword);
                ftpRequest.UseBinary = true;
                ftpRequest.UsePassive = true;
                ftpRequest.KeepAlive = true;
                ftpRequest.Method = WebRequestMethods.Ftp.DeleteFile;
                ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();
                ftpResponse.Close();
                ftpRequest = null;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 刪除指定FileList所有文檔
        /// </summary>
        /// <param name="sRemoteFile"></param>
        public void DeleteFiles(string sDir,string[] sRemoteFile)
        {
            //ArrayList dir = GetFileList(sDir);
            foreach (string file in sRemoteFile)
            {
                DeleteFile(sDir + "//" + file);
            }
        }

        /// <summary>
        /// 刪除指定目錄下所有文檔
        /// </summary>
        /// <param name="sRemoteFile"></param>
        public void DeleteFiles(string sRemoteFile)
        {
            ArrayList dir = GetFileList(sRemoteFile);
            foreach (string file in dir)
            {
                DeleteFile(sRemoteFile + "//" + file);
            }
        }

        /// <summary>
        /// 建立FTP目錄
        /// </summary>
        /// <param name="newDirectory"></param>
        public void CreateDir(string sRemoteDirName)
        {
            try
            {
                ftpRequest = (FtpWebRequest)WebRequest.Create(ftpHost + SetRemotePath(sRemoteDirName));
                ftpRequest.Credentials = new NetworkCredential(ftpAccount, ftpPassword);
                ftpRequest.UseBinary = true;
                ftpRequest.UsePassive = true;
                ftpRequest.KeepAlive = true;
                ftpRequest.Method = WebRequestMethods.Ftp.MakeDirectory;
                ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();
                ftpResponse.Close();
                ftpRequest = null;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 取得指定目錄下的目錄清單
        /// </summary>
        /// <param name="sRemotePath"></param>
        /// <returns></returns>
        public ArrayList GetDirList(string sRemotePath)
        {
            try
            {
                ftpRequest = (FtpWebRequest)FtpWebRequest.Create(ftpHost + SetRemotePath(sRemotePath));
                ftpRequest.Credentials = new NetworkCredential(ftpAccount, ftpPassword);
                ftpRequest.UseBinary = true;
                ftpRequest.UsePassive = true;
                ftpRequest.KeepAlive = true;
                ftpRequest.Method = WebRequestMethods.Ftp.ListDirectory;
                ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();
                ftpStream = ftpResponse.GetResponseStream();
                StreamReader ftpReader = new StreamReader(ftpStream);
                ArrayList arrList = new ArrayList();
                try
                {
                    while (ftpReader.Peek() != -1)
                    {
                        arrList.Add(ftpReader.ReadLine());
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    ftpReader.Close();
                    ftpStream.Close();
                    ftpResponse.Close();
                    ftpRequest = null;
                }
                return arrList;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 獲取根目錄下目錄清單
        /// </summary>
        /// <param name="sRemotePath"></param>
        /// <returns></returns>
        public ArrayList GetDirList()
        {
            return GetDirList("");
        }

        /// <summary>
        /// 取得指定目錄下的文件清單
        /// </summary>
        /// <param name="sRemotePath"></param>
        /// <returns></returns>
        public ArrayList GetFileList(string sRemotePath)
        {
            try
            {
                ftpRequest = (FtpWebRequest)FtpWebRequest.Create(ftpHost + SetRemotePath(sRemotePath));
                ftpRequest.Credentials = new NetworkCredential(ftpAccount, ftpPassword);
                ftpRequest.UseBinary = true;
                ftpRequest.UsePassive = true;
                ftpRequest.KeepAlive = true;
                ftpRequest.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
                ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();
                ftpStream = ftpResponse.GetResponseStream();
                StreamReader ftpReader = new StreamReader(ftpStream);
                ArrayList arrList = new ArrayList();
                try
                {
                    while (ftpReader.Peek() != -1)
                    {
                        for (string Buffer = ftpReader.ReadLine(); Buffer != null; Buffer = ftpReader.ReadLine())
                        {
                            int TrimCount = Buffer[0] >= '0' && Buffer[0] <= '9' ? 3 : Buffer[0] == '-' ? 8 : 0;
                            for (int j = 0; j < TrimCount; j++)
                            {
                                int Length = 0;
                                int StartIndex = 0;
                                for (int k = StartIndex; k < Buffer.Length; k++)
                                    if (Buffer[k] != ' ' && Buffer[k] != '\t')
                                        Length++;
                                    else
                                        break;
                                if (TrimCount == 3 && j == 2 && Buffer.Substring(StartIndex, Length) == "<DIR>")
                                    TrimCount = 0;
                                for (int k = StartIndex + Length; k < Buffer.Length; k++)
                                    if (Buffer[k] == ' ' || Buffer[k] == '\t')
                                        Length++;
                                    else
                                        break;
                                Buffer = Buffer.Substring(StartIndex + Length);
                            }
                            if (TrimCount != 0)
                                arrList.Add(Buffer);
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    ftpReader.Close();
                    ftpStream.Close();
                    ftpResponse.Close();
                    ftpRequest = null;
                }
                return arrList;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void Rename(string currentFileNameAndPath, string newFileName)
        {
            try
            {
                ftpRequest = (FtpWebRequest)WebRequest.Create(ftpHost + SetRemotePath(currentFileNameAndPath));
                ftpRequest.Credentials = new NetworkCredential(ftpAccount, ftpPassword);
                ftpRequest.UseBinary = true;
                ftpRequest.UsePassive = true;
                ftpRequest.KeepAlive = true;
                ftpRequest.Method = WebRequestMethods.Ftp.Rename;
                ftpRequest.RenameTo = newFileName;
                ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();
                ftpResponse.Close();
                ftpRequest = null;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public string GetFileCreatedDateTime(string fileName)
        {
            try
            {
                ftpRequest = (FtpWebRequest)FtpWebRequest.Create(ftpHost + SetRemotePath(fileName));
                ftpRequest.Credentials = new NetworkCredential(ftpAccount, ftpPassword);
                ftpRequest.UseBinary = true;
                ftpRequest.UsePassive = true;
                ftpRequest.KeepAlive = true;
                ftpRequest.Method = WebRequestMethods.Ftp.GetDateTimestamp;
                ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();
                ftpStream = ftpResponse.GetResponseStream();
                StreamReader ftpReader = new StreamReader(ftpStream);
                string fileInfo = null;
                try
                {
                    fileInfo = ftpReader.ReadToEnd();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    ftpReader.Close();
                    ftpStream.Close();
                    ftpResponse.Close();
                    ftpRequest = null;
                }
                return fileInfo;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public string GetFileSize(string fileName)
        {
            try
            {
                ftpRequest = (FtpWebRequest)FtpWebRequest.Create(ftpHost + SetRemotePath(fileName));
                ftpRequest.Credentials = new NetworkCredential(ftpAccount, ftpPassword);
                ftpRequest.UseBinary = true;
                ftpRequest.UsePassive = true;
                ftpRequest.KeepAlive = true;
                ftpRequest.Method = WebRequestMethods.Ftp.GetFileSize;
                ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();
                ftpStream = ftpResponse.GetResponseStream();
                StreamReader ftpReader = new StreamReader(ftpStream);
                string fileInfo = null;
                try
                {
                    while (ftpReader.Peek() != -1)
                    {
                        fileInfo = ftpReader.ReadToEnd();
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    ftpReader.Close();
                    ftpStream.Close();
                    ftpResponse.Close();
                    ftpRequest = null;
                }
                return fileInfo;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string SetRemotePath(string sRemotePath)
        {
            string NewPath = sRemotePath.TrimStart('/').TrimEnd('/');
            if (!string.IsNullOrEmpty(sRemotePath))
            {
                NewPath = NewPath.Replace("//", "/").Replace("\\\\", "/").Replace("\\", "/").Replace("/", "//");
                NewPath = NewPath.TrimStart('/').TrimEnd('/');
                if (!string.IsNullOrEmpty(NewPath))
                {
                    NewPath = "//" + NewPath;
                }
            }
            return NewPath;
        }

        /// <summary>
        /// 判斷文件是否存在。
        /// </summary>
        /// <param name="fileName">文件名稱。</param>
        /// <returns></returns>
        public bool ValidateFileExist(string ftpServer, string fileName, string userid, string password)
        {
            string[] allFiles = GetFileList(ftpServer, WebRequestMethods.Ftp.ListDirectory, userid, password);
            if (allFiles == null)
                return false;
            string result = Array.Find(allFiles, delegate(String list)
            {
                return list.Equals(fileName) ? true : false;
            });
            if (!string.IsNullOrEmpty(result))
                return true;
            else
                return false;
        }
        private static string[] GetFileList(string ftpServer, string ftpMethod, string userid, string password)
        {
            string[] downloadFiles;
            StringBuilder result = new StringBuilder();
            try
            {
                FtpWebRequest request = (FtpWebRequest)FtpWebRequest.Create(ftpServer);
                request.Credentials = new NetworkCredential(userid, UtlString.DecodeBase64(password));
                request.KeepAlive = true;
                request.UseBinary = true;
                request.Method = ftpMethod;
                WebResponse response = request.GetResponse();
                StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                string line = reader.ReadLine();
                while (line != null)
                {
                    result.Append(line);
                    result.Append("\n");
                    line = reader.ReadLine();
                }
                if (result.ToString() != "")
                    result.Remove(result.ToString().LastIndexOf("\n", StringComparison.OrdinalIgnoreCase), 1);
                reader.Close();
                response.Close();
                return result.ToString().Split('\n');
            }
            catch (Exception ex)
            {
                downloadFiles = null;
                return downloadFiles;
                throw ex;
            }
        }
    }
}
