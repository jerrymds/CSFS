using CTBC.FrameWork.HTG;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using System.IO;
using CTBC.CSFS.Models;
using CTBC.CSFS.Pattern;
using CTBC.CSFS.BussinessLogic;
using System.Threading;

namespace CTBC.WinExe.DeathQuerySetting
{
    class Program
    {
        static CTBC.FrameWork.HTG.HostMsgGrpBIZ hostbiz;
        //static ExecuteHTG objSeiHTG;
        //static string cs = null;
        ILog log = LogManager.GetLogger("DebugLog");
        private static Int32 intTask = 1;
        private static string Debug_RacfPassword = "";

        private static DeathQueryBiz dqBiz;

        static void Main(string[] args)
        {
            Program mainProgram = new Program();

            // 產生Excel
            mainProgram.Process();

        }

        private void Process()
        {
            log4net.Config.XmlConfigurator.Configure(new System.IO.FileInfo(AppDomain.CurrentDomain.BaseDirectory + @"\Log.config"));
            WriteLog("====================開始執行國稅局死亡查詢及設定====================");
            try
            {
                if (ConfigurationManager.AppSettings["iTask"] != null)
                    intTask = Convert.ToInt32(ConfigurationManager.AppSettings["iTask"]);

                if (ConfigurationManager.AppSettings["Debug_RacfPassword"] != null)
                    Debug_RacfPassword = ConfigurationManager.AppSettings["Debug_RacfPassword"].ToString().Trim();

                doDeathQuerySetting();
            }
            catch (Exception ex)
            {
                WriteLog("發生未知的錯誤" + ex.Message.ToString());

            }
            WriteLog("============================================================");
        }

        private void doDeathQuerySetting()
        {



            dqBiz = new DeathQueryBiz();

            // 20211208, 檢查, 前一次是否執行完成?
            int isRunning = dqBiz.isRunning();
            if( isRunning > 0)
            {
                WriteLog(string.Format("\t========================================  前一次排程，尚未執行完成, 本次執行取消!! ========================================"));
                return;
            }


            // Step 1. 讀取那一個CaseDeadVersion 案件, 要查詢 SetStatus=0的            
            List<CaseDeadVersion> cdvList = dqBiz.getDeathCase().ToList();

            if( cdvList.Count() <=0)
            {
                WriteLog(string.Format("\t無案件要發查設定"));
                return;
            }

            // 抓住目前要執行的案件, 把狀態更新成99... 若成功, 會押2, 失敗, 會押3
            dqBiz.setDeathCaseRunning();
            WriteLog(string.Format("\t找到共計{0}個案件 要需要發查設定", cdvList.Count().ToString()));

            // Step 2. 找出所有的承辦人的Ldap, Racf, 因為同一個案件, 會是同一個Agent.. 
            var grpCaseNo = cdvList.GroupBy(x => x.DocNo).Select(x => x.Key).ToList();
            foreach (var CaseNo in grpCaseNo)
            {
                WriteLog(string.Format("\t案件{0} 開始進行",CaseNo));

                // 修改CaseDeadVersion 的Status , 改為01----> 處理中
                dqBiz.updateCaseDeadVersionStatus(CaseNo, "01");


                //var NewID = cdvList.First().NewID;
                //string[] ldapInfo = new string[5];
                //ldapInfo = new ApprMsgKeyBiz().getLdapInfo(NewID);   // 找出承辦人的Racf, Ldap
                
                //var objSeiHTG = new ExecuteHTG(ldapInfo[0], ldapInfo[1], ldapInfo[2], ldapInfo[3], ldapInfo[4]);
                //objSeiHTG.HTGInitialize(ldapInfo[0], ldapInfo[1], ldapInfo[2], ldapInfo[3], ldapInfo[4]);
                
           
                var allSettings = cdvList.Where(x => x.DocNo == CaseNo).OrderBy(y => y.Seq).ToList();
                WriteLog(string.Format("\t\t先刪除CaseDeadDetail相同案號的資料"));
                dqBiz.deleteCaseDeadDetailByCaseNo(CaseNo);


                WriteLog(string.Format("\t\t共有 {0} 個名單, 需要進行設定", allSettings.Count().ToString()));




                //for (int i = 0; i < allSettings.Count;i++ )
                //{
                //    ThreadPool.SetMaxThreads(intTask, 1000);
                //    List<Task> TaskList = new List<Task>();
                //    for (int j = 0; j < 3; j++)
                //    {
                //        var cdv = allSettings[i * 3 + j];
                //        Task doSetting = Task.Run(() => DeathSetting2(cdv, CaseNo));
                //        TaskList.Add(doSetting);
                //        Thread.Sleep(3000);
                //    }
                //    Task.WaitAll(TaskList.ToArray());
                //}


                var min = ThreadPool.SetMinThreads(1, 2);
                var suc = ThreadPool.SetMaxThreads(intTask, 1000);
                List<Task> TaskList = new List<Task>();

                foreach (var cdv in allSettings) // 找出這個承辦人 負責的案件 
                {
                    Task doSetting = Task.Run(() => DeathSetting2(cdv, CaseNo));
                    TaskList.Add(doSetting);
                    Thread.Sleep(3000);
                    //DeathSetting2(cdv, CaseNo);
                }
                Task.WaitAll(TaskList.ToArray());


                //int a; int b;
                //ThreadPool.GetMinThreads(out a, out b);
                //var suc = ThreadPool.SetMaxThreads(4, 4);
                ////List<Task> TaskList = new List<Task>();

                //foreach (var cdv in allSettings) // 找出這個承辦人 負責的案件 
                //{
                //    ThreadPool.QueueUserWorkItem(new WaitCallback((o) => { DeathSetting2(cdv, CaseNo); }));


                //}




                WriteLog(string.Format("\t案件{0} 結束\n", CaseNo));
            }
            
        }

        void DeathSetting2(object objCDV, object objCaseNo)
        {
            try
            {
                CaseDeadVersion cdv = objCDV as CaseDeadVersion;
                string CaseNo = objCaseNo.ToString();

                string[] ldapInfo = new string[5];                

                ldapInfo = new ApprMsgKeyBiz().getLdapInfo(cdv.NewID);   // 找出承辦人的Racf, Ldap
                // 發現有可能ApprMsgKey的表格, 沒有新增LDAP, RACF 帳密, 造成錯誤.. 所以多加此判斷
                if (string.IsNullOrEmpty(ldapInfo[0]) || string.IsNullOrEmpty(ldapInfo[1]) || string.IsNullOrEmpty(ldapInfo[2]) || string.IsNullOrEmpty(ldapInfo[3]) )
                {
                    WriteLog(string.Format("\n\t\t =======================警告:  未設定LDAP, RACF 帳密: {0}=======================\n", cdv.NewID));
                    return;
                }

                var HeirID = cdv.HeirId;
                var HeirName = cdv.HeirName;
                var HeirBirthday = cdv.HeirBirthDay;

                int threadId = System.Threading.Thread.CurrentThread.ManagedThreadId;

                try
                {

                    // -----------------------------以下這行是測試用，若在Config中, 有設定Racf密碼, 則用此密碼覆蓋...-------------------
                    if (!string.IsNullOrEmpty(Debug_RacfPassword))
                    {
                        WriteLog(string.Format("\n\t\t =======================通知:  目前使用Config設定的RACF 密碼: {0}=======================\n", Debug_RacfPassword));
                        ldapInfo[3] = Debug_RacfPassword;
                    }
                    // -----------------------------以上這行是測試用，若在Config中, 有設定Racf密碼, 則用此密碼覆蓋...-------------------

                    var objSeiHTG = new ExecuteHTG(ldapInfo[0], ldapInfo[1], ldapInfo[2], ldapInfo[3], ldapInfo[4]);
                    objSeiHTG.HTGInitialize(ldapInfo[0], ldapInfo[1], ldapInfo[2], ldapInfo[3], ldapInfo[4]);



                    WriteLog(string.Format("\t\t\t 開始設定 Thread ID :{0} / {1} / {2} ", threadId, HeirID, HeirName));
                    // 更新CaseDeadVersion.SetStatus狀態為1
                    dqBiz.updateCaseDeadVersionSetStatus(CaseNo, HeirID, "1");


                    // 發查TX_60628, 找出是否有重號, 若有則加入CaseDeadDetailList
                    List<CaseDeadDetail> CaseDeadDetailByID = dqBiz.doQueryDeadDetail(objSeiHTG, cdv);
                    WriteLog(string.Format("\t\t\t\t Thread ID :{0} / 重號{1} ", threadId, string.Join(",", CaseDeadDetailByID.Select(x => x.CDBC_ID))));
                    // 檢查戶名是否一致..若姓名一致, 再檢查生日若一致, 就算一致了..
                    // 寫回TX9091_Status, TX9091_Mesaage 
                    dqBiz.checkSameName(cdv, CaseDeadDetailByID);


                    // 通常檢查都TX9091_Status == '01' 可以繼續跑一 TX60491, 找出所有帳號, 跟黃金存摺                    
                    List<CaseDeadDetail> CaseDeadDetailList = dqBiz.getAccounts(objSeiHTG, cdv, CaseDeadDetailByID);


                    if (CaseDeadDetailList.Count() > 0)
                        WriteLog(string.Format("\t\t\t\t Thread ID :{0} / 找到帳號{1} ", threadId, string.Join(",", CaseDeadDetailList.Select(x => x.Account))));
                    List<string> Setting9091Message = dqBiz.doSetting9091(objSeiHTG, CaseDeadDetailList);
                    WriteLog(string.Format("\t\t\t\t Thread ID :{0} / 9091交易完成{1} ", threadId, HeirID));

                    // 看起來是打67050-5, 回來後..修改
                    // DEAD_FLAG = 'Y' 
                    // 地址不要異動直接落失敗
                    List<string> Setting67050V5 = dqBiz.doSetting67101(objSeiHTG, CaseDeadDetailList);
                    WriteLog(string.Format("\t\t\t\t Thread ID :{0} / 67050交易完成{1} ", threadId, HeirID));

                    // 這裏要把Result 的結果, 寫回CaseDeadDetail 表中
                    //20220315, 加上BRCI_STATUS='F', 表示已打完電文, 接下來, 要看這個欄位, 來決定是否要打DeathBRCISettingESB
                    dqBiz.insertCaseDeadDetail(CaseDeadDetailList);
                    WriteLog(string.Format("\t\t\t\t Thread ID :{0} / 寫回CaseDeadDetail ", threadId, HeirID));
                }
                catch (Exception ex)
                {
                    WriteLog(string.Format("\t\t\t\t Thread ID :{0} / 發生錯誤  {1}", threadId, HeirID, ex.Message.ToString()));
                }
                finally
                {
                    // 檢查案件中, 是否有成功的 若成功, 修改CaseDeadVersion 的SetStatus , 改為2
                    //                          若全部失敗 才押CaseDeadVersion 的SetStatus , 改為3
                    dqBiz.updateSetStatus(CaseNo, HeirID);
                    WriteLog(string.Format("\t\t\t\t Thread ID :{0} / 處理完成 {1} ", threadId, HeirID));
                }
            }
            catch (Exception exUnk)            {
                
                WriteLog(string.Format("\t\t\t\t Thread ID :{0} / 發生錯誤  ", exUnk.Message.ToString()));
            }
        }

        void DeathSetting(object objCDV, object objCaseNo, object htg)
        {            
            try
            {
                CaseDeadVersion cdv = objCDV as CaseDeadVersion;
                string CaseNo = objCaseNo.ToString();
                ExecuteHTG objSeiHTG = htg as ExecuteHTG;

                var HeirID = cdv.HeirId;
                var HeirName = cdv.HeirName;
                var HeirBirthday = cdv.HeirBirthDay;

                int threadId = System.Threading.Thread.CurrentThread.ManagedThreadId;

                try
                {

                    WriteLog(string.Format("\t\t\t 開始設定 Thread ID :{0} / {1} / {2} ", threadId, HeirID, HeirName));
                    // 更新CaseDeadVersion.SetStatus狀態為1
                    dqBiz.updateCaseDeadVersionSetStatus(CaseNo, HeirID, "1");


                    // 發查TX_60628, 找出是否有重號, 若有則加入CaseDeadDetailList
                    List<CaseDeadDetail> CaseDeadDetailByID = dqBiz.doQueryDeadDetail(objSeiHTG, cdv);
                    WriteLog(string.Format("\t\t\t\t Thread ID :{0} / 重號{1} ", threadId, string.Join(",", CaseDeadDetailByID.Select(x => x.CDBC_ID))));
                    // 檢查戶名是否一致..若姓名一致, 再檢查生日若一致, 就算一致了..
                    // 寫回TX9091_Status, TX9091_Mesaage 
                    dqBiz.checkSameName(cdv, CaseDeadDetailByID);


                    // 通常檢查都TX9091_Status == '01' 可以繼續跑一 TX60491, 找出所有帳號, 跟黃金存摺                    
                    List<CaseDeadDetail> CaseDeadDetailList = dqBiz.getAccounts(objSeiHTG, cdv, CaseDeadDetailByID);


                    if (CaseDeadDetailList.Count() > 0)
                        WriteLog(string.Format("\t\t\t\t Thread ID :{0} / 找到帳號{1} ", threadId, string.Join(",", CaseDeadDetailList.Select(x => x.Account))));
                    List<string> Setting9091Message = dqBiz.doSetting9091(objSeiHTG, CaseDeadDetailList);
                    WriteLog(string.Format("\t\t\t\t Thread ID :{0} / 9091交易完成{1} ", threadId, HeirID));

                    // 看起來是打67050-5, 回來後..修改
                    // DEAD_FLAG = 'Y' 
                    // 地址不要異動直接落失敗
                    List<string> Setting67050V5 = dqBiz.doSetting67101(objSeiHTG, CaseDeadDetailList);
                    WriteLog(string.Format("\t\t\t\t Thread ID :{0} / 67050交易完成{1} ", threadId, HeirID));

                    // 這裏要把Result 的結果, 寫回CaseDeadDetail 表中
                    dqBiz.insertCaseDeadDetail(CaseDeadDetailList);
                    WriteLog(string.Format("\t\t\t\t Thread ID :{0} / 寫回CaseDeadDetail ", threadId, HeirID));
                }
                catch (Exception ex)
                {
                    WriteLog(string.Format("\t\t\t\t Thread ID :{0} / 發生錯誤  {1}", threadId, HeirID, ex.Message.ToString()));
                }
                finally
                {
                    // 檢查案件中, 是否有成功的 若成功, 修改CaseDeadVersion 的SetStatus , 改為2
                    //                          若全部失敗 才押CaseDeadVersion 的SetStatus , 改為3
                    dqBiz.updateSetStatus(CaseNo, HeirID);
                    WriteLog(string.Format("\t\t\t\t Thread ID :{0} / 處理完成 {1} ", threadId, HeirID));
                }
            }
            catch (Exception exUnk)
            {
                
                WriteLog(string.Format("\t\t\t\t Thread ID :{0} / 發生錯誤  ", exUnk.Message.ToString()));
            }
        }

        public static void test67101()
        {
            var objSeiHTG = new ExecuteHTG("Z00013771", "Z00013771", "Z13771", "8UHB8UHB", "0495");
            objSeiHTG.HTGInitialize("Z00013771", "Z00013771", "Z13771", "8UHB8UHB", "0495");
            var caseid = new Guid().ToString();
            //var ret = objSeiHTG.Send9091("00000163540119039", "TWD", "3", "亡外1234567890", "03077208", caseid);
            var ret = objSeiHTG.Send67101("F125693534", caseid);
            //var ret = objSeiHTG.Send9092("00000163540119039", "TWD", "3", "亡外1234567890", "03077208", caseid);
            //var ret450 = objSeiHTG.Send45030("03077208", caseid, "00000163540119039", "TWD");
            var dd = ret;

        }


        public static void test9091()
        {
            var objSeiHTG = new ExecuteHTG("Z00013771", "Z00013771", "Z13771", "8UHB8UHB", "0495");
            objSeiHTG.HTGInitialize("Z00013771", "Z00013771", "Z13771", "8UHB8UHB", "0495");
            var caseid = new Guid().ToString();
            var ret = objSeiHTG.Send9091("00000163540119039", "TWD", "3", "亡外1234567890", "03077208", caseid);
            //var ret = objSeiHTG.Send9092("00000163540119039", "TWD", "3", "亡外1234567890", "03077208", caseid);
            var ret450 = objSeiHTG.Send45030("03077208", caseid, "00000163540119039", "TWD");
            var dd= ret;
        }




        public static void WriteLog(string msg)
        {
            if (Directory.Exists(@".\Log") == false)
            {
                Directory.CreateDirectory(@".\Log");
            }
            LogManager.Exists("DebugLog").Debug(msg);
        }
    }
}
