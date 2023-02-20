using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CTBC.CSFS.BussinessLogic;
using CTBC.CSFS.Models;
using CTBC.CSFS.Pattern;
using CTBC.CSFS.Resource;
using CTBC.CSFS.ViewModels;
using CTBC.CSFS.Service;
using System.Configuration;
using System.Net;
using CTBC.FrameWork.Util;
using System.IO;
using System.Collections;
using System.Xml;
using System.Text.RegularExpressions;
using System.Data;

namespace CTBC.WinExe.ImportEDocClear
{
    class Program
    {
        private static int systime;
        private static FileLog m_fileLog;
        private static int timeSection1;
        private static int timeSection2;
        private static int timeSection3;
        private static ImportEDocBiz _ImportEDocBiz;
        private static string ftpserver;
        private static string port;
        private static string username;
        private static string password;
        private static string ftpdir;
        private static string loaclFilePath;
        private static FtpClient ftpClient;
        private static string[] fileTypes;
        static Program()
        {
            systime = Convert.ToInt32(DateTime.Now.Hour.ToString().PadLeft(2, '0') + DateTime.Now.Minute.ToString().PadLeft(2, '0'));
            m_fileLog = new FileLog(ConfigurationManager.AppSettings["filelog"], AppDomain.CurrentDomain.FriendlyName.Replace(".vshost", "").Replace(".exe", ""));
            _ImportEDocBiz = new ImportEDocBiz();
            ftpserver = ConfigurationManager.AppSettings["ftpserver"];
            port = ConfigurationManager.AppSettings["port"];
            username = ConfigurationManager.AppSettings["username"];
            password = ConfigurationManager.AppSettings["password"];
            //20160729不加密--simon
            //由於framework中會先做解密
            //password = UtlString.EncodeBase64(password);

            ftpdir = ConfigurationManager.AppSettings["ftpdir"];
            loaclFilePath = ConfigurationManager.AppSettings["loaclFilePath"];
            ftpClient = new FtpClient(ftpserver, username, password, port);
            fileTypes = ConfigurationManager.AppSettings["fileTypes"].Split(',');
        }
        static void Main(string[] args)
        {
            try
            {
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "--------收文目錄清除開始----------------");
                ImportEDocClear();
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "--------收文目錄清除結束----------------");
            }
            catch (Exception ex)
            {
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "--------收文目錄清除失敗，失敗原因：" + ex.Message + "----------------");
            }
        }

        /// <summary>
        /// 執行電子收文作業
        /// </summary>
        private static void ImportEDocClear()
        {
                ftpClient.DeleteFiles(ftpdir);
        }     
      

    }
}
