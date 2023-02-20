using System;
using System.Collections;
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
using CTBC.CSFS.Models;
using CTBC.CSFS.Pattern;
using CTBC.FrameWork.HTG;

namespace CTBC.WinExe.GetRate
{
    public partial class Service1 : ServiceBase
    {
        private Timer MyTimer;
        ILog log = LogManager.GetLogger("DebugLog");
        public static ExecuteHTG objSeiHTG = new ExecuteHTG();
        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            MyTimer = new Timer();
            MyTimer.Elapsed += new ElapsedEventHandler(MyTimer_Elapsed);
            int inter = int.Parse(ConfigurationManager.AppSettings["m_Interval"].ToString());

            MyTimer.Interval = inter * 1000; // 二分鐘執行一次
            MyTimer.Start();            
            log4net.Config.XmlConfigurator.Configure(new System.IO.FileInfo(AppDomain.CurrentDomain.BaseDirectory + @"\Log.config"));
            WriteLog("開始啟動查詢外匯服務");
            getRate();
        }

        public void getTest()
        {
            // 20181212, 每日凌晨不進行任何電文發查
            DateTime dtFrom = DateTime.Parse(ConfigurationManager.AppSettings["StopServiceFrom"].ToString());
            DateTime dtTo = DateTime.Parse(ConfigurationManager.AppSettings["StopServiceTo"].ToString());
            DateTime dt = DateTime.Now;
         

        }

        public void getRate()
        {
            // 20181212, 每日凌晨不進行任何電文發查
            DateTime dtFrom = DateTime.Parse(ConfigurationManager.AppSettings["StopServiceFrom"].ToString());
            DateTime dtTo = DateTime.Parse(ConfigurationManager.AppSettings["StopServiceTo"].ToString());


            // 查詢時間, 只需要在9:15 ~ 16:15 之間
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


            var frDt = new DateTime(dt.Year, dt.Month, dt.Day, 9, 15, 0);
            var enDt = new DateTime(dt.Year, dt.Month, dt.Day, 16, 16, 0);
            // 20180906, 不再限制一定要9:15~16:15 之間, 才發送
            //if( dt>frDt && dt<enDt)
            //if (dt.Hour > 9 && dt.Hour < 17 && dt.Minute > 15)
            {
                //WriteLog("找出目前待發查案件");
                // 找出目前所有待發查的案號
                WriteLog("重新取Ldap, Racf 帳密....");
                objSeiHTG = new ExecuteHTG();


                WriteLog("取得目前外匯匯率....");
                var Result = objSeiHTG.Send87106();
                if (Result.Count() > 0)
                    WriteLog("發查外匯電文成功....");
                else
                {
                    WriteLog("發查外匯電文失敗....");
                    return;
                }
                //CaseObligorBIZ cobiz = new CaseObligorBIZ();
                //CustomerInfoBIZ custBiz = new CustomerInfoBIZ();
                CTBC.FrameWork.HTG.HostMsgGrpBIZ hostbiz = new CTBC.FrameWork.HTG.HostMsgGrpBIZ();
                PARMCodeBIZ pbiz = new PARMCodeBIZ();

                var currList = pbiz.GetParmCodeByCodeType("Currency").OrderBy(x => x.SortOrder).ToList();
                ArrayList array = new ArrayList();
                string dt1 = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");
                foreach (var c in Result)
                {
                    string strSQL = string.Format("Update PARMCode SET CodeMemo='{0}', ModifiedDate=CONVERT(datetime, '{1}',120) where CodeNo='{2}' AND  CodeType='CURRENCY' ;", c.Value.ToString(), dt1, c.Key);
                    array.Add(strSQL);
                }


                WriteLog("寫入參數檔....");
                var ret = hostbiz.SaveESBData(array);
                if (ret)
                {
                    WriteLog("已寫入參數檔中....");
                }
                else
                {
                    WriteLog("已寫入參數檔失敗....");
                    return;
                }

                // 讀取目前
                //string msg = "發電文";
                //log.Debug(msg);
                //LogManager.Exists("LogHTG").Debug(msg);

                WriteLog("結束外匯匯率查詢....");
            }
            //else
            //{
            //    WriteLog("目前不在發查電文時間內....");
            //}
        }


        private void MyTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            getRate();
        }


        protected override void OnStop()
        {
            WriteLog("結束查詢外匯服務");
            MyTimer.Stop();
            MyTimer = null;
        }

        public void WriteLog(string msg)
        {
            if (Directory.Exists(@".\Log") == false)
            {
                Directory.CreateDirectory(@".\Log");
            }
            LogManager.Exists("DebugLog").Debug(msg);
        }
    }
}
