using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Timers;
using System.Threading;
using System.Configuration;
using System.Net;
using CTCB.NUMS.Service.WebLog;//20130423 hroace

namespace CTBC.CSFS.ScheduleService
{
    public class PARMScheduleSetting
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public string Arguments { get; set; }
        public string Path { get; set; }
        public string Enabled { get; set; }
        public string Status { get; set; }
        public DateTime NextRunDate { get; set; }
    }

    /// <summary>
    /// 啟動各exe執行檔
    /// </summary>
    public class RunScheduleJob
    {
        private PARMScheduleSetting _schedule;
        ProcessData log = new ProcessData();
        public RunScheduleJob(PARMScheduleSetting schedule)
        {
            _schedule = schedule;
        }

        public void StartProcess()
        {
            System.Diagnostics.Process ps = new System.Diagnostics.Process();
            try
            {
                //暫時不執行
                //_schedule.Status = "R";//20150628 horace 設定此項Schedule已經開始啟動,不可再重複啟動,直到Status='W'
                //UpdateScheduleStatus(_schedule);

                if (!string.IsNullOrEmpty(_schedule.Path.Trim()))
                {
                    //System.Diagnostics.Process ps = new System.Diagnostics.Process();
                    ps.StartInfo.FileName = @_schedule.Path.Trim(); //@"D:\CTCB.LAMS.Schedule\bin\CTCB.LAMS.Schedule.Prepare.A01.exe";
                    if (!string.IsNullOrEmpty(_schedule.Arguments.Trim()))
                    {

                        //執行檔後面加上執行時的參數
                        ps.StartInfo.Arguments = @_schedule.Arguments.Trim();
                    }
                    ps.StartInfo.UseShellExecute = false;
                    ps.Start();
                    //設定要等待相關的處理序結束的時間，並且阻止目前的執行緒執行，直到等候時間耗盡或者處理序已經結束為止。
                    ps.WaitForExit();
                    if (ps.HasExited)
                    {
                        log.WriteLog("ScheduleName=" + _schedule.Name + ";Status=STOP;ScheduleID=" + _schedule.ID);

                        //暫時不執行
                        //_schedule.Status = "W";//20150628 horace 設定此項Schedule已經停止,可再啟動
                        //UpdateScheduleStatus(_schedule);
                    }
                }
            }
            catch (Exception ex)
            {
                ps.Close();
                ps = null;
                log.WriteLogToAlertMail("ScheduleName=" + _schedule.Name + ";ErrDesc=" + ex.Message + ex.Source + ex.StackTrace + ";ScheduleID=" + _schedule.ID);
                throw new Exception(ex.Message);
                //暫時不執行
                //_schedule.Status = "W";
                //UpdateScheduleStatus(_schedule);    
            }
        }

        //20150628 horace W=Waiting/R=Running,Running中的排程不可再重複執行,直到Status=W,後才可再被執行
        public bool UpdateScheduleStatus(PARMScheduleSetting oneSch)
        {
            try
            {
                string connString = ConfigurationManager.ConnectionStrings["NUMS_ADO"].ToString();
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    conn.Open();
                    using (SqlCommand comm = new SqlCommand())
                    {
                        comm.Connection = conn;
                        comm.CommandText = "update PARMScheduleSetting set Status=@Status where ID=@GUID";
                        comm.Parameters.Clear();
                        comm.Parameters.Add("@GUID", SqlDbType.UniqueIdentifier).Value = oneSch.ID;
                        comm.Parameters.Add("@Status", SqlDbType.Char, 1).Value = oneSch.Status;
                        comm.ExecuteNonQuery();
                    }
                    log.WriteLog("Update Schedule.Status=" + oneSch.Status);
                }
                return true;
            }
            catch (Exception ex)
            {
                log.WriteLogToAlertMail("ErrDesc=" + ex.Message + ex.Source + ex.StackTrace);
                return false;
            }
        }
    }


    public partial class MainServiceCSFS : ServiceBase
    {
        ProcessData log = new ProcessData();
        int interval = Convert.ToInt32(ConfigurationManager.AppSettings["Interval"].ToString());
        System.Timers.Timer timer = new System.Timers.Timer();
        public MainServiceCSFS()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 啟動本windows service
        /// </summary>
        /// <param name="args"></param>
        protected override void OnStart(string[] args)
        {
            timer.Elapsed += new ElapsedEventHandler(OnElapsedTime);
            timer.Interval = interval;
            timer.Enabled = true;
        }

        /// <summary>
        /// 執行本windows service
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        private void OnElapsedTime(object source, ElapsedEventArgs e)
        {
            PARMScheduleSetting logSch = new PARMScheduleSetting();
            try
            {
                List<PARMScheduleSetting> list = GetSchedule();
                int count = list.Count;
                if (count > 0)
                {
                    log.WriteLog("Total " + count + " schedules prepare to execute.");
                    foreach (PARMScheduleSetting sch in list)
                    {
                        logSch = sch;
                        RunScheduleJob runJob = new RunScheduleJob(sch);
                        Thread t = new Thread(new ThreadStart(runJob.StartProcess));

                        // Start the thread.
                        t.Start();
                        log.WriteLog("ScheduleName=" + sch.Name + ";Status=START;ScheduleID=" + sch.ID);
                    }
                }
            }
            catch (Exception ex)
            {
                log.WriteLogToAlertMail("ScheduleName=" + logSch.Name + ";ErrDesc=" + ex.Message + ex.Source + ex.StackTrace + ";ScheduleID=" + logSch.ID);
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// 取得本時間可執行的名單排程
        /// </summary>
        /// <returns></returns>
        public List<PARMScheduleSetting> GetSchedule()
        {
            List<PARMScheduleSetting> list = new List<PARMScheduleSetting>();

            string connString = ConfigurationManager.ConnectionStrings["NUMS_ADO"].ToString();
            using (SqlConnection conn = new SqlConnection(connString))
            {
                conn.Open();
                using (SqlCommand comm = new SqlCommand())
                {
                    comm.Connection = conn;
                    comm.CommandText = @"with d1
                                        as
                                        (
                                        select ID from PARMScheduleSetting where Enabled = 'Y' 
                                        )
                                        ,d2
                                        as
                                        (
                                        select distinct a.ID,b.Name,b.Path,b.Enabled,b.Status,b.Arguments from d1 a
                                        left join PARMScheduleSetting b on a.ID = b.ID
                                        where 
                                        (
                                            --onetime job
	                                        convert(char(8),OneTime,112) = convert(char(8),GETDATE(),112)
	                                        and datepart(hour,OneTime) = datepart(hour,GETDATE())
	                                        and datepart(minute,OneTime) = datepart(minute,GETDATE())
                                            )
                                            or
                                            (
                                            --regular job
	                                        RegularHour = datepart(hour,GETDATE()) 
	                                        and RegularMinute = datepart(minute,GETDATE())
                                            )
                                        )
                                        select ID,Name,Path,Enabled,Status,Arguments from d2 ";

                    SqlDataReader reader = comm.ExecuteReader();
                    try
                    {
                        while (reader.Read())
                        {
                            list.Add(new PARMScheduleSetting
                            {
                                ID = reader[0].ToString(),
                                Name = reader[1].ToString(),
                                Path = reader[2].ToString(),
                                Enabled = reader[3].ToString(),
                                Status = reader[4].ToString(),
                                Arguments = reader[5].ToString()
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        log.WriteLogToAlertMail("ErrDesc=" + ex.Message + ex.Source + ex.StackTrace);
                        throw new Exception(ex.Message);
                    }
                    finally
                    {
                        // Always call Close when done reading.
                        reader.Close();
                    }
                }
            }
            return list;
        }

        protected override void OnStop()
        {
            timer.Enabled = false;
        }
    }

    /// <summary>
    /// 紀錄Log
    /// </summary>
    public class ProcessData
    {
        public void WriteLogToAlertMail(string content)
        {
            try
            {
                WriteLogToFile(content);
                WriteLogToDB(content);
                WriteLogByLogService(content, "Schedule", "");

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
            }
        }

        public void WriteLog(string content)
        {
            try
            {
                WriteLogToFile(content);
                WriteLogToDB(content);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
            }
        }

        public void WriteLogToFile(string content)
        {
            try
            {
                string logPath = ConfigurationManager.AppSettings["LogPath"].ToString();
                //FileStream fs = new FileStream(@logPath, FileMode.OpenOrCreate, FileAccess.Write);
                //StreamWriter sw = new StreamWriter(fs);
                //sw.BaseStream.Seek(0, SeekOrigin.End);
                //sw.WriteLine(DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString() + " " + content);
                //sw.Flush();
                //sw.Close();
                using (StreamWriter writer = new StreamWriter(@logPath, true))
                {
                    writer.Write(DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString() + " " + content);
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

        public void WriteLogToDB(string content)
        {
            try
            {
                string connStr = ConfigurationManager.ConnectionStrings["NUMS_ADO"].ToString();
                string bSvrName = System.Net.Dns.GetHostName();
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    // Create the Command and Parameter objects.
                    using (SqlCommand comm = conn.CreateCommand())
                    {
                        comm.CommandTimeout = conn.ConnectionTimeout;
                        comm.CommandType = CommandType.Text;
                        comm.CommandText = @"declare @LogID int
                                            declare @nowDateTime datetime
                                            set @nowDateTime = getdate()
                                            exec sp_CSFSLogWriteLog 1,2,'Critical','Schedule',@nowDateTime,'NA','NA','NA','NA','NA','NA',@Message,'ScheduleService|S|I|S|CTBC.CSFS.ScheduleService|NA|NA|" + GetHostIPAddress() + @"|" + bSvrName + @"|',@LogID output
                                            exec sp_CSFSLogToCategory 4,@LogID";
                        comm.Parameters.AddWithValue("@Message", content);
                        try
                        {
                            conn.Open();
                            int rtn = comm.ExecuteNonQuery();
                        }
                        catch (Exception ex)
                        {
                            WriteLogToAlertMail("ErrDesc=" + ex.Message + ex.Source + ex.StackTrace);
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

        public void WriteLogByLogService(string psMessage, string peMsgType, string _defaultLogFile)
        {
            Log _wLog = new Log();
            _wLog.WriteLogByLogService(psMessage, peMsgType, _defaultLogFile);//20130423 horace 寫入DB Table:NUMSLog 與發alert mail    
        }

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

    }
}
