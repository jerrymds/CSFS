using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using log4net;
using log4net.Config;
using System.IO;
using System.Configuration;
using CTBC.CSFS.BussinessLogic;
using log4net;
using CTBC.CSFS.Models;
using CTBC.CSFS.Pattern;

namespace CTBC.WinExe.HTG60491
{
    public partial class Service1 : ServiceBase
    {
        private Timer MyTimer;
        ILog log = LogManager.GetLogger("DebugLog");
        string exeFilename = "";
        //Dictionary<string, string> runningStatus = new Dictionary<string, string>();
        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            string strMInter = ConfigurationManager.AppSettings["m_nInterval"].ToString();
            MyTimer = new Timer();
            MyTimer.Elapsed += new ElapsedEventHandler(MyTimer_Elapsed);
            MyTimer.Interval = int.Parse(strMInter) * 1000; // 二分鐘執行一次
            MyTimer.Start();
            exeFilename = ConfigurationManager.AppSettings["HTGBatchEXE"].ToString();
           
            log4net.Config.XmlConfigurator.Configure(new System.IO.FileInfo(AppDomain.CurrentDomain.BaseDirectory + @"\Log.config"));
            WriteLog("開始啟動發查HTG服務");
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


            // Do SomeThing...
            {
                WriteLog("找出目前待發查案件");
                // 找出目前所有待發查的案號
                CaseObligorBIZ cobiz = new CaseObligorBIZ();
                CustomerInfoBIZ custBiz = new CustomerInfoBIZ();
                var sendList = cobiz.GetAllServiceObligorNo();
                if( sendList==null || sendList.Count()==0)
                {
                    WriteLog("目前沒有待發查案件");
                    return;
                }
                
                for (int tn = 0; tn < 10; tn++)
                {
                    System.Threading.Thread.Sleep(3000);
                    Process p = new Process();
                    p.StartInfo.FileName = exeFilename;
                    p.StartInfo.Arguments = tn.ToString();
                    p.Start();
                    WriteLog( string.Format("目前發查尾碼是{0}的電文", tn.ToString()));
                    //string tailNum = tn.ToString();
                    //DataTable runningTailNum = cobiz.GetObligorNoRunning(tailNum);
                    //if( runningTailNum==null)
                    //{ // 表示, 目前這個尾號, 已經沒有在跑了....
                    //    DataTable dtobligor = cobiz.GetObligorNo("60491", tailNum);
                    //    if( dtobligor!=null)
                    //    {
                    //        string batchQueueDocNo = dtobligor.Rows[0]["DocNo"].ToString();
                    //        Process p = new Process();
                    //        p.StartInfo.FileName = exeFilename;
                    //        // 額外的參數；給的是DocNo , 可以讓Console程式知道要執行那一個案件
                    //        p.StartInfo.Arguments = batchQueueDocNo;
                    //        p.Start();
                    //    }
                    //}
                    

                }


                // 讀取目前
                //string msg = "發電文";
                //log.Debug(msg);
                //LogManager.Exists("LogHTG").Debug(msg);
            }
        }

        public void WriteLog(string msg)
        {
            if (Directory.Exists(@".\Log") == false)
            {
                Directory.CreateDirectory(@".\Log" );
            }
            LogManager.Exists("DebugLog").Debug(msg);
        }

        protected override void OnStop()
        {
            WriteLog("結束發查HTG服務");
            MyTimer.Stop();
            MyTimer = null;
        }
    }


}
