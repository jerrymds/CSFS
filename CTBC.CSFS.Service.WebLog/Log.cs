using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net.Mail;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Net;
using System.Reflection;

namespace CTBC.CSFS.Service.WebLog
{
    public class Log
    {
        string _MailFromDisplayName = "";
        string _MailSubject = "";
        string _MailFrom = "";
        string _MailToWho = "";
        string _SMTPServer = "";
        string _ConnectionString = "";
        string _LogFile = "";

        public Log()
        {
            _MailFromDisplayName = GetAppConfig("MailFromDisplayName",SectionType.AppSettings);
            _MailSubject = GetAppConfig("MailSubject",SectionType.AppSettings);
            _MailFrom = GetAppConfig("MailFrom",SectionType.AppSettings);
            _MailToWho = GetAppConfig("MailToWho",SectionType.AppSettings);
            _SMTPServer = GetAppConfig("SMTPServer",SectionType.AppSettings);
            _ConnectionString = GetAppConfig("CSFS_ADO",SectionType.ConnectionStrings);
            _LogFile = GetAppConfig("LogFile",SectionType.AppSettings);        
        }

        /// <summary>
        /// 透過寫Log到CSFSLog與發mail      
        /// </summary>
        /// <param name="msg">Log 訊息</param>
        /// <param name="msgType">Log Type</param>
        /// <param name="module">Log來源模組</param>
        /// 20130423 horace
        public void WriteLogByLogService(string msg, string msgType="", string module="")
        {
            try
            {
                if (string.IsNullOrEmpty(msg)) msg = "";
                if (string.IsNullOrEmpty(msgType)) msgType = "";
                if (string.IsNullOrEmpty(module)) module = "";
                SendMonitorEmail(msg);
                WriteLogToDB(msg, msgType, module);
            }
            catch (Exception ex)
            {
                WriteToFile("ErrDesc=" + ex.Message + ex.Source + ex.StackTrace);
            }
        }

        //透過exchange mail server發送alert mail
        public void SendMonitorEmail(string msg)
        {
            try
            {
                #region//.NET >= 2.0寫法
                char[] delimiterChars = { ',' };
                string[] whoList = _MailToWho.Split(delimiterChars);  //誰可收到批次執行狀況回報mail
                msg = (string.IsNullOrEmpty(msg)) ? "" : msg;
                if (whoList.Length >= 1)
                {
                    MailAddress mailFrom = new MailAddress(_MailFrom, _MailFromDisplayName);
                    MailAddress mailTo;
                    MailMessage message;
                    SmtpClient client = new SmtpClient(_SMTPServer);
                    for (int i = 0; i < whoList.Length; i++)
                    {
                        mailTo = new MailAddress(whoList[i]);
                        message = new MailMessage(mailFrom, mailTo);
                        message.Subject = _MailSubject;
                        message.IsBodyHtml = true;
                        message.Body = @"<html><head><title>新徵審系統</title></head><body><div style='font-size:13px;font-family:Verdana, Helvetica, Sans-Serif;'>" + @msg + @"</div></body></html>";
                        client.Send(message);
                    }

                    WriteToFile("Sending Batch Status Mail Success");
                }
                else
                {
                    WriteToFile("No List of Mail To !!");
                }
                #endregion
            }
            catch (Exception ex)
            {
                WriteToFile("ErrDesc=" + ex.Message + ex.Source + ex.StackTrace);
            }
        }

        /// <summary>
        /// 寫到CSFSLog中
        /// </summary>
        /// <param name="content">Save到CSFSLog.Message</param>
        /// <param name="msgType">Save到CSFSLog.TITLE</param>
        /// <param name="module">Save到CSFSLog.FnuctionID</param>
        public void WriteLogToDB(string content, string msgType, string module)
        {
            try
            {
                //string connStr = ConfigurationManager.ConnectionStrings["CSFS_ADO"].ToString();
                module = string.IsNullOrEmpty(module) ? "CTBC.CSFS.Service" : module;
                string bSvrName = System.Net.Dns.GetHostName();
                using (SqlConnection conn = new SqlConnection(_ConnectionString))
                {
                    // Create the Command and Parameter objects.
                    using (SqlCommand comm = conn.CreateCommand())
                    {
                        comm.CommandTimeout = conn.ConnectionTimeout;
                        comm.CommandType = CommandType.Text;
                        comm.CommandText = @"declare @LogID int
                                            declare @nowDateTime datetime
                                            set @nowDateTime = getdate()
                                            exec sp_CSFSLogWriteLog 1,2,'Critical',@MsgType,@nowDateTime,'NA','NA','NA','NA','NA','NA',@Message,'BatchServer|S|I|S|" + module + "|NA|NA|" + GetHostIPAddress() + @"|" + bSvrName + @"|',@LogID output
                                            exec sp_CSFSLogToCategory 4,@LogID";
                        comm.Parameters.AddWithValue("@Message", content);
                        comm.Parameters.AddWithValue("@MsgType", (string.IsNullOrEmpty(msgType) ? "Exception" : msgType));
                        try
                        {
                            conn.Open();
                            int rtn = comm.ExecuteNonQuery();
                        }
                        catch (Exception ex)
                        {
                            WriteToFile("ErrDesc=" + ex.Message + ex.Source + ex.StackTrace);
                            throw new Exception(ex.Message);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
            }
        }

        //取得執行時的Host IP
        private string GetHostIPAddress()
        {
            List<string> lstIPAddress = new List<string>();
            IPHostEntry IpEntry = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ipa in IpEntry.AddressList)
            {
                if (ipa.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    lstIPAddress.Add(ipa.ToString());

            }

            return lstIPAddress[0];
        }

        //寫到Local log file
        public void WriteToFile(string msg)
        {
            using (StreamWriter sw =  File.AppendText(_LogFile))
            {
                sw.WriteLine(DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString() + " " + msg);
            }
        }

/// <summary>
        /// 
        /// </summary>
        /// <param name="key">name or key</param>
        /// <param name="type">appSettings=connectionStrings</param>
        /// <returns></returns>
        public string GetAppConfig(string key,string type)
        {
            string result = string.Empty;
            var uri = new Uri(Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase));
            var fileMap = new ExeConfigurationFileMap { ExeConfigFilename = Path.Combine(uri.LocalPath, Assembly.GetExecutingAssembly().FullName.Split(',')[0] + ".dll.config") };
            var assemblyConfig = ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);
            if (assemblyConfig.HasFile)
            {
                if (type == SectionType.AppSettings)
                {
                    AppSettingsSection section = (assemblyConfig.GetSection(SectionType.AppSettings) as AppSettingsSection);
                    result = section.Settings[key].Value;
                }
                if (type == SectionType.ConnectionStrings)
                {
                    ConnectionStringsSection conStrSection = (assemblyConfig.GetSection(SectionType.ConnectionStrings) as ConnectionStringsSection);
                    result = conStrSection.ConnectionStrings[1].ConnectionString;
                }
            }
            return result;
        } 
    }
    public class SectionType
    {
        public static string AppSettings { get { return "appSettings"; } }
        public static string ConnectionStrings { get { return "connectionStrings"; } }
    }
}

