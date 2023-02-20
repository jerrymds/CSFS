using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Timers;
using CTBC.CSFS.BussinessLogic;
using System.Xml;
using System.Configuration;
using log4net;
using CTBC.CSFS.Models;
using CTBC.CSFS.Pattern;

namespace ExcuteHTG
{
    public partial class HtgBatch60491 //: ServiceBase  
    {
        private System.ComponentModel.IContainer components = null;
        //private Timer m_timer = new Timer();        // 定時器
        private FileLog m_filelog = null;
        private int m_nInterval = 1;                // 間隔
        //CTBC.FrameWork.ESB.ExcuteESB esb = new CTBC.FrameWork.ESB.ExcuteESB();
        ExecuteHTG htg = new ExecuteHTG();
        //string Interval = ConfigurationManager.AppSettings["m_nInterval"].ToString();
        public HtgBatch60491()
        {
            log4net.Config.XmlConfigurator.Configure(new System.IO.FileInfo(AppDomain.CurrentDomain.BaseDirectory + @"\Log.config"));
            InitializeComponent();


            //m_timer.AutoReset = false;
            //m_timer.Elapsed += new ElapsedEventHandler(OnTimer);
        }

        public void StartProgram(string tailNum)
        {

            GetHtg(tailNum);
            //OnStart(null);    
        }


        //protected override void OnStart(string[] args)        
        //{
        //    log4net.Config.XmlConfigurator.Configure(new System.IO.FileInfo(AppDomain.CurrentDomain.BaseDirectory + @"\Log.config"));
        //    try
        //    {
        //        m_timer.Stop();
        //        WriteLog("========== 讀取電文 Service 60491 服務啟動 ==========");
        //        WriteLog(Interval);
        //        //GetEsb();
        //        GetHtg();
        //        m_timer.Interval = m_nInterval * int.Parse(Interval);
        //        m_timer.Start();
        //    }
        //    catch (Exception exp)
        //    {
        //        WriteLog("OnStart exp: " + exp.Message);
        //        //throw exp;
        //    }
        //}


        //protected override void OnStop()        
        //{
        //    log4net.Config.XmlConfigurator.Configure(new System.IO.FileInfo(AppDomain.CurrentDomain.BaseDirectory + @"\Log.config"));
        //    m_timer.Stop();
        //    WriteLog("========== 讀取電文 Service 60941 服務停止 ==========");
        //}

        //private void OnTimer(Object source, ElapsedEventArgs e)
        //{
        //    try
        //    {
        //        //GetEsb();
        //        //WriteLog( DateTime.Now.ToString("MM/dd hh:mm:ss") + "\t Start");
        //        GetHtg();
        //        //WriteLog(DateTime.Now.ToString("MM/dd hh:mm:ss") + "\t Ending");
        //    }
        //    catch (Exception exp)
        //    {
        //        WriteLog("OnTimer exp: " + exp.Message);

        //        //throw exp;
        //    }

        //    if (m_nInterval == 0)
        //    {
        //        if (this.CanStop)
        //        {
        //            this.Stop();
        //        }
        //        return;
        //    }

        //    m_timer.Start();
        //}

        public void WriteLog(string msg)
        {
            if (Directory.Exists(@".\Log") == false)
            {
                Directory.CreateDirectory(@".\Log");
            }
            LogManager.Exists("DebugLog").Debug(msg);
        }

        /// <summary>
        /// 呼叫HTG電文
        /// </summary>
        private void GetHtg(string tailNum)
        {

            CaseObligorBIZ cobiz = new CaseObligorBIZ();
            CustomerInfoBIZ custBiz = new CustomerInfoBIZ();
            KDSql kdsql = new KDSql();
            // 先找, 是否有狀態是99 且尾數是tailNum, 若有, 表示, 目前前一次執行尚未完成, 等候下一輪, 在進行發查
            //var oList = cobiz.GetAllServiceObligorNo(tailNum);
             var obligorListExit =cobiz.GetObligorNo99("60491", tailNum);
             if (obligorListExit == null || obligorListExit.Count() > 0)
            {
                WriteLog(string.Format("前次執行中, 等候下一次執行 , 尾碼是: {0}", tailNum));
                return;
            }

            // 若有0的, 先把狀態改成99, 以免下一次, 又再執行同義的電文...  先把此時, 尾數tailNum 改成99
            bool bresult = cobiz.EditAllServiceCaseObligor("99",tailNum);



            // 開始發電文了...

            #region 電文60491           
            var obligorList = cobiz.GetObligorNo99("60491", tailNum);
            //DataTable dtobligorList60491 = cobiz.GetObligorNo("60491", DocNo);
            //string tailNum = DocNo.Substring(DocNo.Length-1); 
            //if (dtobligorList60491 != null)

            


                //DataRow dr = dtobligorList60491.Rows[0];
                //IList<CaseObligor> 
                WriteLog(string.Format("{0}\t開始跑批次, 共計{1}筆  ", DateTime.Now.ToString("MM/dd hh:mm:ss"), obligorList.Count().ToString()));
                WriteLog("**********START**********");

                List<CaseObligor> DoubleIDs = new List<CaseObligor>();
                Dictionary<string, bool> isAllSuccess = new Dictionary<string,bool>();
                StringBuilder failMessage = new StringBuilder();

                foreach (CaseObligor obligornoOrg in obligorList)
                {
                    try
                    {
                        String caseid = cobiz.GetBatchQueueID(obligornoOrg.ObligorId.ToString());
                        cobiz.EditCaseObligor(obligornoOrg.ObligorId.ToString(), "99", "執行中");

                        string retMessage = null;
                        List<CaseObligor> doubleIDs = Send60628(obligornoOrg, tailNum, ref retMessage);
                        bool isSuccess = true;
                        if ( !string.IsNullOrEmpty(retMessage))
                        {
                            //--------------表示60629 失敗
                            WriteLog(string.Format("*****發查60629失敗, 原因 {0}", retMessage));
                            cobiz.EditCaseObligor(obligornoOrg.ObligorId.ToString(), "2", "發查60629失敗, 原因" + retMessage);
                            isSuccess = isSuccess & false;
                        }
                        if( doubleIDs.Count()==0)
                        {
                            //--------------表示60629 失敗
                            WriteLog(string.Format("*****發查60629失敗, 原因 {0}  ", retMessage));
                            cobiz.EditCaseObligor(obligornoOrg.ObligorId.ToString(), "2", "發查60629失敗, 原因" + retMessage);
                            isSuccess = isSuccess & false;
                        }

                        
                        string mess = "";
                        foreach (var obligorno in doubleIDs)
                        {

                            #region 
                            string result670508 = htg.HTGMainFun("67050", obligorno.ObligorNo.ToString(), "", caseid, tailNum);
                            if( ! result670508.StartsWith("0000|"))
                            {
                                WriteLog(string.Format("*****發查67050-8失敗  "));
                                cobiz.EditCaseObligor(obligornoOrg.ObligorId.ToString(), "2", "發查67050-8失敗, 有部分失敗");
                                isSuccess = isSuccess & false;
                            }


                            // 打60491...
                            string result = htg.HTGMainFun("60491", obligorno.ObligorNo.ToString(), "", caseid, tailNum);
                            string[] mes = result.Split('|');
                            WriteLog(string.Format("\t發查60491, ID={0}, 結果={1}", obligorno.ObligorNo.ToString(), mes[0]));
                            if ((mes[0] == "0001" || mes[0] == "0002") && mes.Length>=2)
                            {
                                isSuccess = isSuccess & false;
                                mess += " " + mes[1] + " ";
                                //isAllSuccess.Add(obligornoOrg..ObligorId.ToString(), false);
                                //failMessage.Append(mes[1] + "\r\n");
                            }
                            else
                            {
                                isSuccess = isSuccess & true;
                                //isAllSuccess.Add(obligornoOrg.ObligorId.ToString(), true);
                            }
                            #endregion

                        }


                        if( isSuccess)
                        {
                            WriteLog(string.Format("\t發查60491, 全部成功, Status=1"));
                            cobiz.EditCaseObligor(obligornoOrg.ObligorId.ToString(), "1", "");
                        }
                        else
                        {
                            WriteLog(string.Format("\t發查60491, 有部分失敗, Status=2, 訊息" +mess + retMessage));
                            cobiz.EditCaseObligor(obligornoOrg.ObligorId.ToString(), "2", "發查60491失敗, 訊息" + mess + retMessage);
                        }

                        if (doubleIDs.Count() > 1) //表示有重號, 要改變TX_60491_GRP.MultFlag = "Y", 
                        {
                            updatCaseMaterMultFlag(caseid, "Y");
                        }
                        else //表示沒有重號, 要改變TX_60491_GRP.MultFlag = "N", 
                        {
                            updatCaseMaterMultFlag(caseid, "N");
                        }
                        //isAllSuccess.Add(obligornoOrg.ObligorId.ToString(), isSuccess);
                    }
                    catch (Exception ex)
                    {
                        //* 單筆出錯儘量不影響全部的查詢
                        WriteLog("發生錯誤 電文60491 : " + ex.Message);
                        cobiz.EditCaseObligor(obligornoOrg.ObligorId.ToString(), "2", "發查60491, 有部分失敗, 訊息" + ex.Message);
                    }
                }

            //var failCount = isAllSuccess.Count(x => !x.Value);
            //if( failCount>0)
            //{
            //    WriteLog(string.Format("\t發查60491, 有部分失敗, Status=2"));
            //    foreach(var s in isAllSuccess.Where(x=>!x.Value))
            //        cobiz.EditCaseObligor(s.Key.ToString(), "2", failMessage.ToString());
            //}
            //else
            //{
            //    WriteLog(string.Format("\t發查60491, 全部成功, Status=1"));
            //    foreach (var s in isAllSuccess.Where(x => x.Value))
            //        cobiz.EditCaseObligor(s.Key.ToString(), "1", "");
            //}


            #endregion



            #region 電文62072
            obligorList = cobiz.GetObligorNo99("67072",tailNum);
            //DataTable dtobligorList62072 = cobiz.GetObligorNo("62072", DocNo);
            //if (dtobligorList62072 != null)
            {
                //DataRow dr = dtobligorList62072.Rows[0];
                //obligorList = cobiz.GetObligorNo("67072", tailNum);
                foreach (CaseObligor obligorno in obligorList)
                {
                    try
                    {
                        string caseid = cobiz.GetBatchQueueID(obligorno.ObligorId.ToString());
                        cobiz.EditCaseObligor(obligorno.ObligorId.ToString().ToString(), "99", "執行中");
                        string result = htg.HTGMainFun("67072", obligorno.ObligorNo.ToString(), "", caseid, tailNum);
                        string[] mes = result.Split('|');
                        if (mes[0] == "0001")
                        {
                            cobiz.EditCaseObligor(obligorno.ObligorId.ToString(), "2", mes[1]);
                        }
                        else
                        {
                            cobiz.EditCaseObligor(obligorno.ObligorId.ToString(), "1", "");
                        }
                    }
                    catch (Exception ex)
                    {
                        //* 單筆出錯儘量不影響全部的查詢
                        WriteLog("發生錯誤 電文67072 : " + ex.Message);
                        cobiz.EditCaseObligor(obligorno.ObligorId.ToString(), "2", "發查電文62072, 有部分失敗, 訊息" + ex.Message);
                    }
                }
            }
            #endregion

            //20160122 RC --> 20150111 宏祥 add 新增67100電文
            #region 電文67100

            obligorList = cobiz.GetObligorNo99("67100", tailNum);
            //DataTable dtobligorList67100 = cobiz.GetObligorNo("67100", DocNo);
            //if (dtobligorList67100 != null)
            {
                //DataRow dr = dtobligorList67100.Rows[0];
                //obligorList = cobiz.GetObligorNo("67100", tailNum);
                foreach (CaseObligor obligorno in obligorList)
                {
                    try
                    {
                        string caseid = cobiz.GetBatchQueueID(obligorno.ObligorId.ToString());
                        cobiz.EditCaseObligor(obligorno.ObligorId.ToString(), "99", "執行中");
                        string result = htg.HTGMainFun("67100", obligorno.ObligorNo.ToString(), "", caseid, tailNum);
                        string[] mes = result.Split('|');
                        if (mes[0] == "0001")
                        {
                            cobiz.EditCaseObligor(obligorno.ObligorId.ToString(), "2", mes[1]);
                        }
                        else
                        {
                            cobiz.EditCaseObligor(obligorno.ObligorId.ToString(), "1", "");
                        }
                    }
                    catch (Exception ex)
                    {
                        //* 單筆出錯儘量不影響全部的查詢
                        WriteLog("發生錯誤 電文67100: " + ex.Message);
                        cobiz.EditCaseObligor(obligorno.ObligorId.ToString(), "2", "發查電文67100, 有部分失敗, 訊息" + ex.Message);
                    }
                }
            }
            #endregion



            WriteLog(string.Format("{0}\t完成批次 ", DateTime.Now.ToString("MM/dd hh:mm:ss")));
            WriteLog("**********END**********");

        }

        private void updatCaseMaterMultFlag(string caseid, string multFlag)
        {
            KDSql kdsql = new KDSql();
            string sql = string.Format("UPDATE [dbo].[TX_60491_Grp] SET MutltFlag='{0}' WHERE CASEID='{1}'", multFlag, caseid.ToString());

            int aaa = kdsql.SQLServGetInt(sql);
            int bbb = aaa;
        }


        private List<CaseObligor> Send60628(CaseObligor obligorno, string tailNum, ref string retMessage)
        {
            KDSql kdsql = new KDSql();
            #region 若有重號的.. 要抓出來
            List<CaseObligor> newObligor = new List<CaseObligor>();
            var d = obligorno;
            {
                //var result60629 = htg.HTGMainFun("60629", d.ObligorNo , "", d.CaseId.ToString() , tailNum);
                string result60629 = htg.HTGMainFun("60629", d.ObligorNo, "", d.CaseId.ToString(), tailNum);

                Dictionary<string, string> Res = new Dictionary<string, string>();
                // 再去DB把重號的部分查出來.
                if (result60629.StartsWith("0000"))
                {
                    string trnnum = result60629.Split('|')[1].Trim();
                    string _trn = result60629.Replace("0000|", "");
                    string sql = "SELECT TOP 1 * FROM TX_60629 WHERE TRNNUM='{0}'  ORDER BY SNO DESC";
                    string formsql = string.Format(sql, _trn);
                    #region 取得重號的ID, 至Res
                    DataTable gCase = kdsql.getDataTable(formsql);
                    //var nums = ctx.TX_60629.Where(x => x.TrnNum == trnnum).OrderByDescending(x => x.SNO).FirstOrDefault();
                    if (gCase == null)
                    {
                        //result = "0001|扣押失敗(原因450-31查不到該筆)";
                    }
                    else
                    {
                        if (gCase.Rows.Count > 0)
                        {
                            if (!string.IsNullOrEmpty(gCase.Rows[0]["ID_DATA_1"].ToString())) Res.Add(gCase.Rows[0]["ID_DATA_1"].ToString(), gCase.Rows[0]["CUST_NAME_1"].ToString());
                            if (!string.IsNullOrEmpty(gCase.Rows[0]["ID_DATA_2"].ToString())) Res.Add(gCase.Rows[0]["ID_DATA_2"].ToString(), gCase.Rows[0]["CUST_NAME_2"].ToString());
                            if (!string.IsNullOrEmpty(gCase.Rows[0]["ID_DATA_3"].ToString())) Res.Add(gCase.Rows[0]["ID_DATA_3"].ToString(), gCase.Rows[0]["CUST_NAME_3"].ToString());
                            if (!string.IsNullOrEmpty(gCase.Rows[0]["ID_DATA_4"].ToString())) Res.Add(gCase.Rows[0]["ID_DATA_4"].ToString(), gCase.Rows[0]["CUST_NAME_4"].ToString());
                            if (!string.IsNullOrEmpty(gCase.Rows[0]["ID_DATA_5"].ToString())) Res.Add(gCase.Rows[0]["ID_DATA_5"].ToString(), gCase.Rows[0]["CUST_NAME_5"].ToString());
                            if (!string.IsNullOrEmpty(gCase.Rows[0]["ID_DATA_6"].ToString())) Res.Add(gCase.Rows[0]["ID_DATA_6"].ToString(), gCase.Rows[0]["CUST_NAME_6"].ToString());
                            if (!string.IsNullOrEmpty(gCase.Rows[0]["ID_DATA_7"].ToString())) Res.Add(gCase.Rows[0]["ID_DATA_7"].ToString(), gCase.Rows[0]["CUST_NAME_7"].ToString());
                            if (!string.IsNullOrEmpty(gCase.Rows[0]["ID_DATA_8"].ToString())) Res.Add(gCase.Rows[0]["ID_DATA_8"].ToString(), gCase.Rows[0]["CUST_NAME_8"].ToString());
                            if (!string.IsNullOrEmpty(gCase.Rows[0]["ID_DATA_9"].ToString())) Res.Add(gCase.Rows[0]["ID_DATA_9"].ToString(), gCase.Rows[0]["CUST_NAME_9"].ToString());
                            if (!string.IsNullOrEmpty(gCase.Rows[0]["ID_DATA_10"].ToString())) Res.Add(gCase.Rows[0]["ID_DATA_10"].ToString(), gCase.Rows[0]["CUST_NAME_10"].ToString());
                            if (!string.IsNullOrEmpty(gCase.Rows[0]["ID_DATA_11"].ToString())) Res.Add(gCase.Rows[0]["ID_DATA_11"].ToString(), gCase.Rows[0]["CUST_NAME_11"].ToString());
                            if (!string.IsNullOrEmpty(gCase.Rows[0]["ID_DATA_12"].ToString())) Res.Add(gCase.Rows[0]["ID_DATA_12"].ToString(), gCase.Rows[0]["CUST_NAME_12"].ToString());
                        }
                    }

                    #endregion
                }
                else
                {
                    retMessage = result60629;
                }

                

                if (Res.Count() > 0)
                {
                    foreach (var r in Res)
                    {

                            CaseObligor c = new CaseObligor();
                            c.ObligorNo = r.Key;
                            c.ObligorName = r.Value;
                            c.CaseId = d.CaseId;
                            c.ObligorId = d.ObligorId;
                            newObligor.Add(c);
                            WriteLog(string.Format("\t發查60628 , ID={0}, 取得重號={1}", d.ObligorNo.ToString(), r.Key));

                    }
                }
                else
                {
                    retMessage = retMessage.Replace("0001|", "0002");
                }
            } // END foreach(var d in DoubleIDs)
            #endregion
            return newObligor;
        }


        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        //protected override void Dispose(bool disposing)
        //protected  void Dispose(bool disposing)
        //{
        //    if (disposing && (components != null))
        //    {
        //        components.Dispose();
        //    }
        //    base.Dispose(disposing);
        //}

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            // 
            // ExcuteESB
            // 
            //this.ServiceName = "ExcuteHTG";

        }

        #endregion

    }
}
