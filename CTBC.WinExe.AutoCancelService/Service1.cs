using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using log4net;
using log4net.Config;
using System.Timers;
using System.Configuration;
using System.IO;

namespace CTBC.WinExe.AutoCancelService
{
    public partial class Service1 : ServiceBase
    {
        private Timer MyTimer;
        ILog log = LogManager.GetLogger("DebugLog");
        string exeFilename = "";
        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            /// Adam 20191107 考慮原本有檢查半夜不能做,加了這一段就會直接執行,再每2分鐘一次
            exeFilename = ConfigurationManager.AppSettings["AutoCancelEXE"].ToString();
            log4net.Config.XmlConfigurator.Configure(new System.IO.FileInfo(AppDomain.CurrentDomain.BaseDirectory + @"\Log.config"));
            WriteLog("開始執行自動撤銷...");
            Timer MyTimer1 = new Timer();
            MyTimer1.Elapsed += new ElapsedEventHandler(MyTimer_Elapsed1);
            MyTimer1.Interval = 1; // 執行一次
            MyTimer1.AutoReset = false;
            MyTimer1.Start();
            WriteLog("結束自動撤銷");
            MyTimer1.Stop();
            MyTimer1 = null;
            /// 
            string strMInter = ConfigurationManager.AppSettings["m_nInterval"].ToString();
            MyTimer = new Timer();
            MyTimer.Elapsed += new ElapsedEventHandler(MyTimer_Elapsed);
            MyTimer.Interval = int.Parse(strMInter) * 1000; // 二分鐘執行一次
            MyTimer.AutoReset = true;
            MyTimer.Start();
            exeFilename = ConfigurationManager.AppSettings["AutoCancelEXE"].ToString();
            log4net.Config.XmlConfigurator.Configure(new System.IO.FileInfo(AppDomain.CurrentDomain.BaseDirectory + @"\Log.config"));
            WriteLog("開始啟動自動撤銷服務");
        }


        private void MyTimer_Elapsed1(object sender, ElapsedEventArgs e)
        {
            // 20181212, 每日凌晨不進行任何電文發查
            DateTime dtFrom = DateTime.Parse(ConfigurationManager.AppSettings["StopServiceFrom"].ToString());
            DateTime dtTo = DateTime.Parse(ConfigurationManager.AppSettings["StopServiceTo"].ToString());



            //ExecuteHTG objHtg = new ExecuteHTG();
            DateTime dt = DateTime.Now;

            if (dt >= dtFrom && dt <= dtTo)
            {
                WriteLog("目前為 夜間停止發查期間.... 不進行任何發查");
                return;
            }
            else
            {
                WriteLog("進行發查...");
            }

            WriteLog("開始執行自動撤銷...");

            System.Threading.Thread.Sleep(3000);
            Process p = new Process();
            p.StartInfo.FileName = exeFilename;
            //p.StartInfo.Arguments = tn.ToString();
            p.Start();
            //WriteLog(string.Format("目前發查尾碼是{0}的電文", tn.ToString()));
            WriteLog("執行完成自動撤銷...");
            // 讀取目前
            //string msg = "發電文";
            //log.Debug(msg);
            //LogManager.Exists("LogHTG").Debug(msg);

        }
        private void MyTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            // 20181212, 每日凌晨不進行任何電文發查
            DateTime dtFrom = DateTime.Parse(ConfigurationManager.AppSettings["StopServiceFrom"].ToString());
            DateTime dtTo = DateTime.Parse(ConfigurationManager.AppSettings["StopServiceTo"].ToString());



            //ExecuteHTG objHtg = new ExecuteHTG();
            DateTime dt = DateTime.Now;

            if (dt >= dtFrom && dt <= dtTo)
            {
                WriteLog("目前為 夜間停止發查期間.... 不進行任何發查");
                return;
            }
            else
            {
                WriteLog("進行發查...");
            }

            WriteLog("開始執行自動撤銷...");

            System.Threading.Thread.Sleep(3000);
            Process p = new Process();
            p.StartInfo.FileName = exeFilename;
            //p.StartInfo.Arguments = tn.ToString();
            p.Start();
            //WriteLog(string.Format("目前發查尾碼是{0}的電文", tn.ToString()));
            WriteLog("執行完成自動撤銷...");
            // 讀取目前
            //string msg = "發電文";
            //log.Debug(msg);
            //LogManager.Exists("LogHTG").Debug(msg);

        }


        public void WriteLog(string msg)
        {
            if (Directory.Exists(@".\Log") == false)
            {
                Directory.CreateDirectory(@".\Log");
            }
            LogManager.Exists("DebugLog").Debug(msg);
        }

        protected override void OnStop()
        {
            WriteLog("結束發查自動撤銷服務");
            MyTimer.Stop();
            MyTimer = null;
        }
    }
}
