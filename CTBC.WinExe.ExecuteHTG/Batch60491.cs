using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using CTBC.FrameWork.ESB;
using System.IO;
using System.Timers;
using CTBC.CSFS.BussinessLogic;
using System.Xml;
using System.Configuration;
using log4net;
using CTBC.CSFS.Models;
using CTBC.CSFS.Pattern;


namespace ExcuteESB
{
    public partial class Batch60491 : ServiceBase
    {

        private Timer m_timer = new Timer();        // 定時器
        private FileLog m_filelog = null;
        private int m_nInterval = 1;                // 間隔
        CTBC.FrameWork.ESB.ExcuteESB esb = new CTBC.FrameWork.ESB.ExcuteESB();
        string Interval = ConfigurationSettings.AppSettings["m_nInterval"].ToString();

        public Batch60491()
        {
            log4net.Config.XmlConfigurator.Configure(new System.IO.FileInfo(AppDomain.CurrentDomain.BaseDirectory + @"\Log.config"));
            InitializeComponent();

            
            ////m_timer.AutoReset = false;
            ////m_timer.Elapsed += new ElapsedEventHandler(OnTimer);
        }

        public void test()
        {
            OnStart(null);
        }
        protected override void OnStart(string[] args)
        {
            log4net.Config.XmlConfigurator.Configure(new System.IO.FileInfo(AppDomain.CurrentDomain.BaseDirectory + @"\Log.config"));
            try
            {
                m_timer.Stop();
                WriteLog("========== 讀取電文 Service 60491 服務啟動 ==========");
                WriteLog(Interval);
                GetEsb();
                m_timer.Interval = m_nInterval * int.Parse(Interval);
                m_timer.Start();
            }
            catch (Exception exp)
            {
                WriteLog("OnStart exp: " + exp.Message);
                //throw exp;
            }
        }

        protected override void OnStop()
        {
            log4net.Config.XmlConfigurator.Configure(new System.IO.FileInfo(AppDomain.CurrentDomain.BaseDirectory + @"\Log.config"));
            m_timer.Stop();
            WriteLog("========== 讀取電文 Service 60941 服務停止 ==========");
        }

        private void OnTimer(Object source, ElapsedEventArgs e)
        {
            try
            {
                GetEsb();
            }
            catch (Exception exp)
            {
                WriteLog("OnTimer exp: " + exp.Message);

                //throw exp;
            }

            if (m_nInterval == 0)
            {
                if (this.CanStop)
                {
                    this.Stop();
                }
                return;
            }

            m_timer.Start();
        }

        public void WriteLog(string msg)
        {
            if (Directory.Exists(@".\Log") == false)
            {
                Directory.CreateDirectory(@".\Log");
            }
            LogManager.Exists("DebugLog").Debug(msg);
        }

        /// <summary>
        /// 呼叫電文
        /// </summary>
        public void GetEsb()
        {
            CaseObligorBIZ cobiz = new CaseObligorBIZ();
            CustomerInfoBIZ custBiz = new CustomerInfoBIZ();
            #region 電文60491
            IList<CaseObligor> obligorList = cobiz.GetObligorNo("60491");
            foreach (CaseObligor obligorno in obligorList)
            {
                try
                {
                  	String caseid = cobiz.GetBatchQueueID(obligorno.ObligorId.ToString());
                    string result = esb.ESBMainFun("60491", obligorno.ObligorNo,"",caseid);
                    string[] mes = result.Split('|');
                    if (mes[0] == "0001")
                    {
                        cobiz.EditCaseObligor(obligorno.ObligorId.ToString(), "1", "");
                        Guid gcaseId = new Guid(caseid);
                        TX_60491_Grp main = custBiz.GetLatestTx60491Grp(obligorno.ObligorNo, gcaseId);
                        if (main != null && main.SNO > 0)
                        {
                            IList<TX_60491_Detl> detls = custBiz.GetTx60491Detls(main.SNO);
                            if (detls != null && detls.Any())
                            {
                                foreach (TX_60491_Detl detl in detls)
                                {
                                    esb.ESBMainFun("33401", detl.Account,detl.CUST_ID,caseid);
                                }   
                            }
                        }
                    }
                    else
                    {
                        cobiz.EditCaseObligor(obligorno.ObligorId.ToString(), "2", mes[1]);
                    }
                }
                catch (Exception ex)
                {
                    //* 單筆出錯儘量不影響全部的查詢
                    WriteLog("OnTimer exp: " + ex.Message);
                }
            }
            #endregion

            #region 電文67072
            obligorList = cobiz.GetObligorNo("67072");
            foreach (CaseObligor obligorno in obligorList)
            {
                try
                {
                	  string caseid = cobiz.GetBatchQueueID(obligorno.ObligorId.ToString());
                    string result = esb.ESBMainFun("67072", obligorno.ObligorNo,"",caseid);
                    string[] mes = result.Split('|');
                    if (mes[0] == "0001")
                    {
                        cobiz.EditCaseObligor(obligorno.ObligorId.ToString(), "1", "");
                    }
                    else
                    {
                        cobiz.EditCaseObligor(obligorno.ObligorId.ToString(), "2", mes[1]);
                    }
                }
                catch (Exception ex)
                {
                    //* 單筆出錯儘量不影響全部的查詢
                    WriteLog("OnTimer exp: " + ex.Message);
                }
            }
            #endregion

            //20160122 RC --> 20150111 宏祥 add 新增67100電文
            #region 電文67100
            obligorList = cobiz.GetObligorNo("67100");
            foreach (CaseObligor obligorno in obligorList)
            {
                try
                {
                	  string caseid = cobiz.GetBatchQueueID(obligorno.ObligorId.ToString());
                    string result = esb.ESBMainFun("67100", obligorno.ObligorNo, "",caseid);
                    string[] mes = result.Split('|');
                    if (mes[0] == "0001")
                    {
                        cobiz.EditCaseObligor(obligorno.ObligorId.ToString(), "1", "");
                    }
                    else
                    {
                        cobiz.EditCaseObligor(obligorno.ObligorId.ToString(), "2", mes[1]);
                    }
                }
                catch (Exception ex)
                {
                    //* 單筆出錯儘量不影響全部的查詢
                    WriteLog("OnTimer exp: " + ex.Message);
                }
            }
            #endregion
        }



    }
}
