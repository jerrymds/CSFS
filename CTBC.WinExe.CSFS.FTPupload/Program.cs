/// <summary>
/// 程式說明:4-將法務資料存入DB
/// </summary>

using CTBC.CSFS.BussinessLogic;
using CTBC.FrameWork.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace CTBC.WinExe.CSFS.FTPupload
{
    class Program
    {
        private static FileLog m_fileLog;
        private static string sendftpserver;
        private static string sendport;
        private static string sendusername;
        private static string sendpassword;
        private static string sendftpdir;
        private static string sendloaclFilePath;
        private static FtpClient sendftpClient;


        private static string reciveftpserver;
        private static string reciveport;
        private static string reciveusername;
        private static string recivepassword;
        private static string reciveftpdir;
        private static string reciveloaclFilePath;
        private static FtpClient reciveftpClient;


        static Program()
        {
            m_fileLog = new FileLog(ConfigurationManager.AppSettings["filelog"], AppDomain.CurrentDomain.FriendlyName.Replace(".vshost", "").Replace(".exe", ""));

            string dt = DateTime.Now.ToString("yyyyMMdd").Substring(4, 4);

            #region 收文設定

            reciveftpserver = ConfigurationManager.AppSettings["reciveftpserver"];
            reciveport = ConfigurationManager.AppSettings["reciveport"];
            reciveusername = ConfigurationManager.AppSettings["reciveusername"];
            recivepassword = ConfigurationManager.AppSettings["recivepassword"];


            reciveftpdir = ConfigurationManager.AppSettings["reciveftpdir"];
            reciveloaclFilePath = ConfigurationManager.AppSettings["reciveloaclFilePath"];
            reciveftpClient = new FtpClient(reciveftpserver, reciveusername, recivepassword, reciveport);

            #endregion

            #region 發文設定

            sendftpserver = ConfigurationManager.AppSettings["sendftpserver"];
            sendport = ConfigurationManager.AppSettings["sendport"];
            sendusername = ConfigurationManager.AppSettings["sendusername"];
            sendpassword = ConfigurationManager.AppSettings["sendpassword"];


            sendftpdir = ConfigurationManager.AppSettings["sendftpdir"];
            sendloaclFilePath = ConfigurationManager.AppSettings["sendloaclFilePath"];
            sendftpClient = new FtpClient(sendftpserver, sendusername, sendpassword, sendport);

            #endregion
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            Program mainProgram = new Program();
            mainProgram.Process();
        }

        private void Process()
        {
            // 上傳檔案到ftp
            SendFile();

            // 從ftp 收文
            ReciveFile();
        }


        /// <summary>
        /// 從ftp 收文
        /// </summary>
        public static bool ReciveFile()
        {
            try
            {
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息---------從MFTP下載檔案作業開始----------------");

                // 判斷路徑是否存在
                if (!Directory.Exists(reciveloaclFilePath))
                {
                    Directory.CreateDirectory(reciveloaclFilePath);
                }

                // 取得FTP指定目錄下的所有文件名稱
                ArrayList fileList = reciveftpClient.GetFileList(reciveftpdir);

                // 下載FTP指定目錄下的所有文件
                foreach (var file in fileList)
                {
                    //  ftp 文件
                    string remoteFile = reciveftpClient.SetRemotePath(reciveftpdir) + "//" + file;

                    // 本地文件
                    string localFile = reciveloaclFilePath.TrimEnd('\\') + "\\" + file;

                    // 若已經存在，則先刪除掉舊的文件
                    if (File.Exists(localFile))
                    {
                        File.Delete(localFile);
                    }

                    reciveftpClient.GetFiles(remoteFile, localFile);

                    m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------從MFTP下載檔案" + file + "----------------");

                    try { 
                       reciveftpClient.DeleteFile(remoteFile);

                       m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------刪除FTP文件" + file + "----------------");
                    }
                    catch (Exception ex)
                    {
                       m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------無法刪除FTP文件" + file + "，失敗原因：" + ex.Message + "----------------");

                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------從MFTP下載檔案作業失敗，失敗原因：" + ex.Message + "----------------");

                return false;
            }
        }

        /// <summary>
        /// 上傳檔案到ftp
        /// </summary>
        public static void SendFile()
        {
            try
            {
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息-------- 上傳檔案到MFTP作業開始----------------");

                // 取得本地文件清單
                string[] fileList = Directory.GetFiles(sendloaclFilePath);

                //  上傳本地文件到指定目錄
                foreach (string file in fileList)
                {
                    // 上傳檔案到FTP
                    sendftpClient.SendFile(sendftpdir, file);

                    m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------上傳檔案" + file + "到MFTP上----------------");

                    File.Delete(file);

                    m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------刪除本地文件" + file + "----------------");

                }            

            }
            catch (Exception ex)
            {
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------上傳檔案到MFTP作業失敗，失敗原因：" + ex.Message + "----------------");
            }
        }


    }
}
