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
using System.Timers;
using System.Configuration;
using System.IO;

namespace CTBC.WinExe.WarningQuerySettingService
{
    public partial class Service1 : ServiceBase
    {
        private Timer MyTimer;
        ILog log = LogManager.GetLogger("DebugLog");
        string exeFilename = "";
        private KDSql db;

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
            string conn = ConfigurationManager.ConnectionStrings["CSFS_ADO"].ToString();

            db = new KDSql(conn);

            log4net.Config.XmlConfigurator.Configure(new System.IO.FileInfo(AppDomain.CurrentDomain.BaseDirectory + @"\Log.config"));
            WriteLog("開始啟動WarningQuerySetting 發查服務");
        }

        protected override void OnStop()
        {
            WriteLog("結束WarningQuerySetting 發查服務");
            MyTimer.Stop();
            MyTimer = null;
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


            
            {
                WriteLog("找出目前待發查案件");
                string sql = string.Format(@"select count(*) from WarningDetails d inner join WarningMaster m on d.docno=m.docno where (d.Status='C01' OR d.Status='D01' ) AND (d.SetDate is NULL) ");
                DataTable dtResult = db.getDataTable(sql);

                if( int.Parse( dtResult.Rows[0][0].ToString()) ==0 )
                {
                    WriteLog("目前沒有待發查案件");
                    return;
                }
                

                // 找出目前所有待發查的案號

                //for (int tn = 0; tn < 10; tn++)
                {
                    System.Threading.Thread.Sleep(3000);
                    Process p = new Process();
                    p.StartInfo.FileName = exeFilename;
                    //p.StartInfo.Arguments = tn.ToString();
                    p.Start();
                    //WriteLog(string.Format("目前發查尾碼是{0}的電文", tn.ToString()));
                    WriteLog(string.Format("目前已啟動發查"));
                }
            }
        }



        public void WriteLog(string msg)
        {
            if (Directory.Exists(@".\Log") == false)
            {
                Directory.CreateDirectory(@".\Log");
            }
            LogManager.Exists("DebugLog").Debug(msg);
        }

        public void test()
        {
            string strMInter = ConfigurationManager.AppSettings["m_nInterval"].ToString();
            MyTimer = new Timer();
            MyTimer.Elapsed += new ElapsedEventHandler(MyTimer_Elapsed);
            MyTimer.Interval = int.Parse(strMInter) * 1000; // 二分鐘執行一次
            MyTimer.Start();
            exeFilename = ConfigurationManager.AppSettings["HTGBatchEXE"].ToString();
            string conn = ConfigurationManager.ConnectionStrings["CSFS_ADO"].ToString();

            db = new KDSql(conn);

            log4net.Config.XmlConfigurator.Configure(new System.IO.FileInfo(AppDomain.CurrentDomain.BaseDirectory + @"\Log.config"));
            WriteLog("開始啟動WarningQuerySetting 發查服務");

            MyTimer_Elapsed(null, null);
        }
    }
}
