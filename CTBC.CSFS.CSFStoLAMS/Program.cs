using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using CTBC.CSFS.BussinessLogic;
using CTBC.FrameWork.Util;
using CTBC.CSFS.Models;
using System.IO;
namespace CTBC.CSFS.CSFStoLAMS
{
    class Program
    {
        private static FileLog m_fileLog;
        private static string ftpserver;
        private static string ftpport;
        private static string ftpusername;
        private static string ftppassword;
        private static string ftpdir;
        private static string LoaclFilePath;
        private static FtpClient ftpClient;

        static Program()
        {
            m_fileLog = new FileLog(ConfigurationManager.AppSettings["filelog"], AppDomain.CurrentDomain.FriendlyName.Replace(".vshost", "").Replace(".exe", ""));

           
            #region FTP設定

            ftpserver = ConfigurationManager.AppSettings["ftpserver"];
            ftpport = ConfigurationManager.AppSettings["ftpport"];
            ftpusername = ConfigurationManager.AppSettings["ftpusername"];
            ftppassword = ConfigurationManager.AppSettings["ftppassword"];


            ftpdir = ConfigurationManager.AppSettings["ftpdir"];
            LoaclFilePath = ConfigurationManager.AppSettings["LoaclFilePath"];
            ftpClient = new FtpClient(ftpserver, ftpusername, ftppassword, ftpport);

            #endregion
        }
        static void Main(string[] args)
        {
            Program mainProgram = new Program();
            mainProgram.Process();
        }
        private void Process()
        {
            m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------開始執行批次----------------");
            GetDataAndToftp();
            m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------執行批次結束----------------");
        }
        public static bool GetDataAndToftp()
        {
            try
            {
                CaseMasterBIZ cmbiz = new CaseMasterBIZ();
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息-------- 開始從數據庫取出本周二到上週三的數據----------------");
                //得到需要寫TXT文檔的數據
                List<CaseMaster> datalist =cmbiz.CaseMasterSearchListForBatch();
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息-------- 從數據庫取出本周二到上週三的數據結束----------------");
                
                #region 開始寫TXT臨時文件

                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息-------- 本次從數據庫取出" + datalist.Count.ToString() + "條數據----------------");
                if (!Directory.Exists(LoaclFilePath))
                {
                    Directory.CreateDirectory(LoaclFilePath);
                }
                string strdate = DateTime.Now.ToString("yyyyMMdd");
                string strlocalpath = LoaclFilePath + @"/CSFStoLAMS_" + strdate + ".txt";
                if (File.Exists(strlocalpath))
                {
                    File.Delete(strlocalpath);
                }
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息-------- 創建本地臨時TXT文檔，路徑：" + strlocalpath + "----------------");
                FileStream fs = File.Create(strlocalpath);
                fs.Close();
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息-------- 開始往本地臨時TXT文檔寫數據----------------");
                StreamWriter mysr = new StreamWriter(strlocalpath, true, Encoding.Default);
                foreach(CaseMaster cmitem in datalist)
                {
                    string strwrite = cmitem.CaseNo + "," + cmitem.ObligorNo + "," + cmitem.GovNo + "," + cmitem.CreatedDate;
                    mysr.WriteLine(strwrite);
                }
                mysr.Flush();
                mysr.Dispose();
                mysr.Close();
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息-------- 往本地臨時TXT文檔寫數據結束----------------");
                #endregion
                //上傳文件到FTP上
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息-------- 開始上傳臨時TXT文檔到FTP----------------");
                ftpClient.SendFile(ftpdir, strlocalpath);
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息-------- 上傳臨時TXT文檔到FTP成功----------------");
                File.Delete(strlocalpath);
                return true;
            }
            catch(Exception ex)
            {
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.Error, "執行批次失敗，失敗原因：" + ex.Message + "----------------");
                return false;
            }
        }
    }
}
