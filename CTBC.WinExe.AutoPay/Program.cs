using CTBC.CSFS.BussinessLogic;
using CTBC.FrameWork.HTG;
using CTBC.CSFS.Models;
using log4net;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CTBC.CSFS.ViewModels;
using CTBC.CSFS.Resource;
using CTBC.FrameWork.Util;
using System.Collections;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.Globalization;
using Newtonsoft.Json;

namespace CTBC.WinExe.AutoPay
{

    class Program
    {
        public static string eQueryStaff = null;
        public static string _branchNo = null;
        public static string cs = null;
        public static string LastAgentSetting = null;
        public static bool isDebugMode = false;

        //CustomerInfoBIZ custBiz = new CustomerInfoBIZ();
        public static CTBC.FrameWork.HTG.HostMsgGrpBIZ hostbiz;
        public static CaseMasterBIZ caseMaster;
        public static CaseAccountBiz caseAccount;
        public static bool sendHTG = true;  // 本變數來決定, 是否要發電文, 還是只讀DB
        public static ExecuteHTG objSeiHTG;
        public static int HandleFee = 0;
        public static DateTime BreakDay;
        public static bool isAutoPayReply = false;
        public static string ITManagerEmail = "hunghsiang.chang@ctbcbank.com"; // 預設宏祥...
        //public static int delaySeconds = 1800; // 預設測試卡件, 1800 秒...

        public static Dictionary<string, decimal> gdicCurrency = new Dictionary<string, decimal>();
        ILog log = LogManager.GetLogger("DebugLog");

        static void Main(string[] args)
        {
            caseMaster = new CaseMasterBIZ();
            caseAccount = new CaseAccountBiz();
            hostbiz = new CTBC.FrameWork.HTG.HostMsgGrpBIZ();
            objSeiHTG = new ExecuteHTG();


            log4net.Config.XmlConfigurator.Configure(new System.IO.FileInfo(AppDomain.CurrentDomain.BaseDirectory + @"\Log.config"));
            //WriteLog("====================開始執行自動扣押====================");
            cs = ConfigurationManager.ConnectionStrings["CSFS_ADO"].ToString();


            if (System.Configuration.ConfigurationManager.AppSettings["ITManager"] != null)
            {
                ITManagerEmail = System.Configuration.ConfigurationManager.AppSettings["ITManager"].ToString();
            }

            //if (System.Configuration.ConfigurationManager.AppSettings["delaySeconds"] != null)
            //{
            //    delaySeconds = int.Parse(System.Configuration.ConfigurationManager.AppSettings["delaySeconds"].ToString());
            //}

            if (args.Length == 0) // 沒有參數，代表不按照來文號的尾碼進行分流發查, 照發文號來查
            {
                isDebugMode = false;
                doAutoPay(null);
            }
            else
            {
                isDebugMode = true;
                doAutoPay(args[0].ToString().Trim());
            }

        }




        private static void noticeMail(string[] mailFromTo, string InitMessage)
        {
            string mailFrom = ConfigurationManager.AppSettings["mailFrom"];
            //string[] mailFromTo = ConfigurationManager.AppSettings["mailTo"].ToString().Split(',');
            PARMCodeBIZ pbiz = new PARMCodeBIZ();
            var sysEnv = pbiz.GetParmCodeByCodeType("SysEnv");
            string strEnv = "";

            if (sysEnv.Count() > 0)
            {
                strEnv = sysEnv.First().CodeNo.Trim();
            }

            string subject = strEnv + " -- CTBC.WinExe.AutoPay 外來文系統 RACF 登入錯誤";
            string body = "外來文系統 RACF 登入錯誤，錯誤原因：" + InitMessage;
            string host = ConfigurationManager.AppSettings["mailHost"];
            UtlMail.SendEmail(mailFrom, mailFromTo, subject, body, host);
        }

        /// <summary>
        /// 自動支付
        /// 若是Null 則
        /// </summary>
        /// <param name="v"></param>
        private static void doAutoPay(string _CaseNo)
        {

            AutoPayBiz apBiz = new AutoPayBiz();
            // 先找, 是否有狀態是99 且尾數是tailNum, 若有, 表示, 目前前一次執行尚未完成, 等候下一輪, 在進行發查

            //IList<CaseMaster> allCaseMasters = caseMaster.getCaseMaster("支付", "997", _CaseNo);
            //20220411, 
            IList<CaseMaster> allCaseMasters = apBiz.getCaseMasterStillRuning();
            if (allCaseMasters.Count() > 0)
            {
                // 表示目前還在執行中, 直接Return;
                //WriteLog(string.Format("*********************************前次仍在執行中, 尚有 {0} 個正在排隊執行中******************************", allCaseMasters.Count().ToString()));
                //return;

                // ===============================================================================================
                // 20221128, 正達表示, 過去幾個月內, 並不會主動落人工..
                // 20221128, 改為. .在本批自動扣押完成後, 回頭看一下, 是否本批還有997, 若有.. 則落人工
                // ===============================================================================================




                // 20220328, 若在執行中, 可能是相同案件, 卡件, 所以要檢查, 是不是
                // 找出目前正在執行的第一筆...
                //var allCaseDoc = apBiz.getCaseMasterStillRuning();
                //var Firstcasedoc = allCaseDoc.First();
                ////20220328, 記錄目前處理的案件在ParmCode.CodeType='ProcAutoSeizure' , CodeNo='案號',  CodeTag='次數'

                ////20220706, 讀取第一筆上次案件的執行時間....
                //PARMCode lastProcCase = apBiz.getParmCodeByCodeType("ProcAutoPay");


                //// 若案件號碼相同且, 時間超過設定時間, 則落人工....
                //if (lastProcCase.CodeNo == Firstcasedoc.CaseNo)
                //{
                //    DateTime allowTime = ((DateTime)lastProcCase.ModifiedDate).AddSeconds(delaySeconds);
                //    DateTime thenow = DateTime.Now;
                //    //WriteLog(string.Format("*********************************前次仍在執行中, 案件{0} / 卡住次數{1} ******************************", Firstcasedoc.CaseNo.ToString(), iCount.ToString()));
                //    if (allowTime > thenow)
                //    {
                //        WriteLog(string.Format("*********************************目前案件{0} 執行時間{1}  仍在  執行時間仍在 {2} 秒內, 繼續等待下一次執行******************************", lastProcCase.CodeNo, lastProcCase.ModifiedDate, delaySeconds.ToString()));
                //        // 表示目前還在執行中, 直接Return;                
                //        return;
                //    }
                //    else // 相同案件, 重覆次數超過 retryNum .. 啟動 把此案件落人工, 其讓其他案件繼續跑
                //    {
                //        // 把第一個案號, 落人工C01
                //        WriteLog(string.Format("*********************************將案件 {0},  落人工 , 其餘繼續執行******************************", Firstcasedoc.CaseNo.ToString()));
                //        apBiz.setCaseMasterC01B01(Firstcasedoc.CaseId);
                //        List<string> memos = new List<string>() { "電文發查延滯" };
                //        apBiz.insertCaseMemo(Firstcasedoc.CaseId, "CaseSeizure", memos, "SYS");
                //    }
                //}
                //else
                {
                    //20220728, 
                    WriteLog(string.Format("\r\n\r\n\r\n\r\n*********************************前案執行中, 尚有{0} 筆待執行*********************************\r\n\r\n\r\n\r\n", allCaseMasters.Count().ToString()));
                    return;
                }
                


            }


            allCaseMasters = caseMaster.getCaseMaster("支付", "B01", _CaseNo);

            WriteLog(string.Format("\r\n\r\n\r\n\r\n取得共{0}筆案件", allCaseMasters.Count().ToString()));
            if (allCaseMasters.Count == 0)
            {
                WriteLog(string.Format("*********************************目前無任何支付案件, 結束自動支付******************************"));
                return;
            }

            // 取得人工處理的經辦的名單, 放入humanProcLists ... 中...
            AgentSettingBIZ agentSetting = new AgentSettingBIZ();
            //IList<AgentSetting> humanProcLists = agentSetting.getAgentSetting();


            // 確定是個人戶的案件後,  馬上把取得案件編號, 改成997
            caseMaster.setCaseMasterRunning(allCaseMasters.Select(x => x.CaseId).ToList(), "997");


            // 20200616, 計算開票日

            PARMCodeBIZ pbiz = new PARMCodeBIZ();

            int[] checkDateArray = pbiz.GetParmCodeByCodeType("CheckDate_Setup").OrderBy(x => x.SortOrder).Select(x => int.Parse(x.CodeNo)).ToArray();

            // 20200825, 不應該是當天日期, 而是建檔日期... CaseMaster.CreatedDate ...
            //BreakDay = UtlString.GetCheckDate(DateTime.Now, checkDateArray[0], checkDateArray[1], checkDateArray[2]);


            // 20200821, 需要讀取參數檔, 以決定是否要啟動新的支付回文電子化...
            var parmAutoReply = pbiz.GetParmCodeByCodeType("AutoPayReply").FirstOrDefault();

            if (parmAutoReply != null && parmAutoReply.Enable != null)
            {
                isAutoPayReply = (bool)parmAutoReply.Enable;
            }

            CheckQueryAndPrintBIZ CKP = new CheckQueryAndPrintBIZ();
            //開始遂筆執行自動支付
            foreach (var casedoc in allCaseMasters.OrderBy(x => x.CaseNo))
            {
                WriteLog(string.Format("開始支付CaseID={0} DocNo={1}", casedoc.CaseId.ToString(), casedoc.CaseNo.ToString()));

                //20220328, 記錄目前處理的案件在ParmCode.CodeType='ProcAutoPay' , CodeNo='案號',  CodeTag='次數'
                //var iCount = apBiz.UpdateProcCaseNo("ProcAutoPay", casedoc.CaseNo.ToString());
                //string message = apBiz.setExecuteCaseNo("ProcAutoPay", casedoc.CaseNo);
                //if (!string.IsNullOrEmpty(message))
                //{
                //    WriteLog("設定目前處理案件錯誤: " + message);
                //}

                updateCaseMasterAgentUser(casedoc.CaseId, "SYS");

                // 20200825, 不應該是當天日期, 而是建檔日期... CaseMaster.CreatedDate ...
                BreakDay = UtlString.GetCheckDate(DateTime.Parse(casedoc.CreatedDate), checkDateArray[0], checkDateArray[1], checkDateArray[2]);      
    
                // 20211221, 需要檢查, 開票日是否是上班日... 若不是.. 以下方法會自動延到下一個工作日....
                BreakDay = DateTime.Parse( CKP.GetWorkingDays(BreakDay.ToString("yyyy-MM-dd")));


                //20200611, 發查人, 不可固定某個人, 因此, 要去讀[dbo].[ApprMsgKey]  where versionnewid=@caseId
                var ldapInfo = new ApprMsgKeyBiz().getLdapInfo(casedoc.CaseId);


                //20220318, 檢查, 是否在ApprMsgKey中, 有啟動發查...
                if (string.IsNullOrEmpty(ldapInfo[0]) || string.IsNullOrEmpty(ldapInfo[1]) || string.IsNullOrEmpty( ldapInfo[2]) || string.IsNullOrEmpty( ldapInfo[3]))
                {
                    WriteLog("***************************使用者未啟動發查(ApprMsgKey, 沒有任何LDAP RACF 帳密)******************************");
                    noticeITMail(ITManagerEmail, "使用者未啟動發查(ApprMsgKey, 沒有任何LDAP RACF 帳密), 案件: " + casedoc.CaseNo );
                    continue;
                }

                objSeiHTG = new ExecuteHTG(ldapInfo[0], ldapInfo[1], ldapInfo[2], ldapInfo[3], ldapInfo[4]);

                // 20210604, ***************************  ********************************
                var isInit = objSeiHTG.HTGInitialize(ldapInfo[0], ldapInfo[1], ldapInfo[2], ldapInfo[3], ldapInfo[4]);
                if (!isInit)
                {
                    string InitErrorMessage = objSeiHTG.initErrorMessage;
                    var eTabsStaffsMail1 = pbiz.GetParmCodeByCodeType("eTabsQueryStaffNoticeMail").FirstOrDefault();
                    if (eTabsStaffsMail1 != null)
                    {
                        string[] mailTo = eTabsStaffsMail1.CodeMemo.Split(',');
                        noticeMail(mailTo, InitErrorMessage);
                        WriteLog("**********************************************************************");
                        WriteLog(string.Format("****************************目前發查人{0} 帳密錯誤**************************", ldapInfo[0].ToString()));
                        WriteLog("**********************************************************************");
                    }
                    return;
                }
                // 20210604, *************************** ********************************



                WriteLog(string.Format("\t取得由{0}來發查", ldapInfo[0]));
                //將取得的ldapID 設定給eQueryStaff 
                eQueryStaff = ldapInfo[0];

                #region 將CaseMaster的AssignPerson, 及AgentUser改為目前發查人


                caseMaster.updateCaseMasterAgentUser(casedoc.CaseId, eQueryStaff);
                WriteLog("\t指派發查人" + eQueryStaff);
                #endregion



                #region 新增一筆CaseHistory

                insertCaseHistory(casedoc, "自動支付", eQueryStaff, "收發-待分文", "收發分派", "經辦人員", eQueryStaff, "經辦-待辦理");
                insertCaseAssignTable(casedoc.CaseId, eQueryStaff, 0);
                WriteLog("\t新增一筆CaseHistory, CaseAssignTable");
                #endregion
                CaseMemoBiz cMemoBiz = new CaseMemoBiz();

                string result = "";

                try
                {

                    result = doPayByCaseId(casedoc, ldapInfo);

                    if (result.StartsWith("0000") || result.StartsWith("9900"))
                    {
                        WriteLog("\t\t支付完成!!");
                        updateCaseMasterStatus(casedoc.CaseId, "D01", eQueryStaff, "自動支付");
                        var cmBiz = new CaseMasterBIZ();
                        cmBiz.updateCaseMasterAgentUser(casedoc.CaseId, eQueryStaff);

                        insertCaseHistory(casedoc, "自動支付", eQueryStaff, "經辦-待辦理", "經辦呈核", "自動化處理", eQueryStaff, "主管-待核決");
                        // 以下二個動作, 都在ChengHe()中做完了...
                        //string mgrid = GetManagerID(eQueryStaff);
                        //insertCaseAssignTable(casedoc.CaseId, mgrid, 0);
                        // 進行呈核動作.. 參考AgentAccountInfoController 的第353行 子流程
                        ChengHe(casedoc.CaseId, eQueryStaff);
                    }
                    else if (result.StartsWith("9902")) // 支付>扣押, 落收發, 開票, 不取票
                    {
                        WriteLog("\t\t開票, 不取票, 落人工支付完成!!");
                        updateCaseMasterStatus(casedoc.CaseId, "B01", eQueryStaff, "自動支付");
                        var cmBiz = new CaseMasterBIZ();
                        cmBiz.updateCaseMasterAgentUser(casedoc.CaseId, "SYS");
                        insertCaseHistory(casedoc, "自動支付", eQueryStaff, "經辦-待辦理", "經辦待辦理", "自動支付退回人工", eQueryStaff, "經辦-待辦理");
                    }
                    else if (result.StartsWith("9905"))
                    {
                        WriteLog("\t\t支付失敗, 原因: !!" + result + "轉收發待辦理");
                        updateCaseMasterStatus(casedoc.CaseId, "B01", eQueryStaff, "自動支付");
                        var cmBiz = new CaseMasterBIZ();
                        cmBiz.updateCaseMasterAgentUser(casedoc.CaseId, "SYS");
                        insertCaseHistory(casedoc, "自動支付", eQueryStaff, "收發-待辦理", "收發待辦理", "自動支付退回人工", eQueryStaff, "收發-待辦理");
                        List<string> docmemo = new List<string>() { result.Replace("9905|", "") };
                        insertCaseMemo(casedoc.CaseId, "CaseMemo", docmemo, "SYS");
                    }
                    else if (result.StartsWith("9909"))
                    {
                        updateCaseMasterStatus(casedoc.CaseId, "B01", eQueryStaff, "自動支付");
                        WriteLog("\t\t支付失敗, 原因: !!" + result);
                        var cmBiz = new CaseMasterBIZ();
                        cmBiz.updateCaseMasterAgentUser(casedoc.CaseId, "SYS");
                        List<string> docmemo = new List<string>() { result.Replace("9909|","") };
                        insertCaseMemo(casedoc.CaseId, "CaseMemo", docmemo , "SYS");
                    }                    
                    else if( result.StartsWith("991")) // 991X開頭, 一定有其中的錯誤
                    {
                        updateCaseMasterStatus(casedoc.CaseId, "B01", eQueryStaff, "自動支付");
                        WriteLog("\t\t支付失敗, 原因: !!" + result);
                        var cmBiz = new CaseMasterBIZ();
                        cmBiz.updateCaseMasterAgentUser(casedoc.CaseId, "SYS");
                        List<string> docmemo = new List<string>();
                        foreach(var errorLine in Regex.Split(result,"\r\n"))
                        {
                            if (string.IsNullOrEmpty(errorLine))
                                continue;
                            string errorMessage = errorLine.Split('|')[1].ToString();
                            docmemo.Add(errorMessage);
                        }                       
                        
                        insertCaseMemo(casedoc.CaseId, "CaseMemo", docmemo, "SYS");
                    }
                    WriteLog(string.Format("@@@@@@@@@@@@結束支付:{0}\r\n\r\n\r\n\r\n\r\n", casedoc.CaseNo));
                }
                catch (Exception ex)
                {
                    //List<string> docmemo = new List<string>() { "未知的錯誤，落人工" };
                    WriteLog(string.Format("未知的錯誤 {0} {1}", casedoc.CaseNo, ex.Message.ToString()));
                    insertCaseMemo(casedoc.CaseId, "CaseMemo", new List<string>() { "未知的錯誤，落人工" }, "SYS");
                    noticeITMail(ITManagerEmail, "未知的錯誤，!, 案件編號: " + casedoc.CaseNo + "錯誤訊息: " + ex.Message.ToString());
                }



            }

            // 20221128, 正達建議, 在執行完成後, 去檢查一下, 是否有997的案件, 若有, 則落人工

            // 去檢查一下, 是否有997的案件, 若有, 則落人工
            WriteLog(string.Format("*********************************將案件共計 {0}, 已執行完成, 檢查是否有卡件******************************", allCaseMasters.Count().ToString()));
            List<CaseMaster> stillRunningCase = apBiz.getCaseMasterStillRuningCase();
            if (stillRunningCase.Count() > 0)
            {
                WriteLog(string.Format("*********************************發現仍有卡件 , 共計{0} ******************************", stillRunningCase.Count().ToString()));
                foreach (CaseMaster caseInfo in stillRunningCase)
                {
                    WriteLog(string.Format("*********************************案件 {0}, 已落人工******************************", caseInfo.CaseNo));
                    apBiz.setCaseMasterC01B01(caseInfo.CaseId);
                    List<string> memos = new List<string>() { "電文發查延滯" };
                    apBiz.insertCaseMemo(caseInfo.CaseId, "CaseSeizure", memos, "SYS");
                    noticeITMail(ITManagerEmail, "自動扣押卡件! 案件編號: " + caseInfo.CaseNo + "\t 已落人工");
                }

            }
            else
            {
                WriteLog(string.Format("*********************************目前本批次無卡件, 程式完成******************************"));
            }


        }




        private static bool insertCaseMemo(Guid caseid, string p, List<string> DocMemo, string eQueryStaff)
        {

            bool isResult = true;
            CaseMemoBiz cmbiz = new CaseMemoBiz();

            var newDocMemo = DocMemo.Distinct();
            foreach (var s in newDocMemo)
            {
                CaseMemo cm = new CaseMemo();
                cm.CaseId = caseid;
                cm.MemoType = p;
                cm.Memo = s;
                cm.MemoDate = DateTime.Now.ToShortDateString();
                cm.MemoUser = eQueryStaff;
                isResult = isResult & cmbiz.Create2(cm);
            }

            return isResult;
           
        }


        /// <summary>
        /// 20200508, 開始動作...
        /// </summary>
        /// <param name="casedoc"></param>
        /// <returns></returns>
        private static string doPayByCaseId(CaseMaster casedoc, string[] ldapInfo)
        {
            string retStr = string.Empty;
            bool isOverCancel = false;
            var cmBiz = new CaseMasterBIZ();
            var caBiz = new CaseAccountBiz();

            WriteLog(String.Format("\t\t step 1: 比對支付來文與原扣押案是否相符"));
            // Step 1: 比對支付來文與原扣押案是否相符
            // Step 1-1: 條件
            // 來文機關  CaseMaster.[GovUnit].
            // 來文日期  CaseMaster.[GovDate]
            // 來文字號  CaseMaster.[GovNo]
            // 扣押金額  CaseMaster.[ReceiveAmount]

            WriteLog(String.Format("\t\t處理支付案{0} / {1} / {2} ", casedoc.DocNo, casedoc.GovNo, casedoc.GovDate));

            // 找出支付案件所列之當初扣押案號... (EDocTXT3.SeizureIssueNo)

            var SeizureGovNo = cmBiz.getSeizureGovNo(casedoc.CaseId); // 取得支付案件中, 原扣押GovNo
            var SeizureGovDate = cmBiz.getSeizureGovDate(casedoc.CaseId); // 取得支付案件中, 原扣押GovDate

            WriteLog(String.Format("\t\t原扣押GovNo: {0} / GovDate: {1}  ", SeizureGovNo, SeizureGovDate));
            if (string.IsNullOrEmpty(SeizureGovNo))
            {
                return String.Format("9905|支付來文   {0}  之扣押命令發文字號, 找不到 ", casedoc.ReceiverNo);
            }


            // 20200727, 要CaseDoc, 找出義務人的ID, 來比對出來,....
            IList<CaseObligor> caseObligorsList = new CaseObligorBIZ().GetObligorsList(casedoc.CaseId);
            WriteLog(String.Format("\t\t義務人: {0} ", string.Join(" / ", caseObligorsList.Select(x=>x.ObligorName))));
            // Step 1: 發查TX450, 找到前案金額及備註
            // 先查450-31, 是否有該筆扣押金額, 若有, 才能支付(要注意, 同一帳戶, 設扣押及解扣押, 要沖銷)
            var SeizureCases = caseMaster.getCaseMasterByGovNo(casedoc.PreGovNo, casedoc.PreSubDate, caseObligorsList.ToList()); // 找出原扣押案件
            if (SeizureCases.Count() ==0 ) // 等於0, 都是錯誤的....
            {
                return String.Format("9905|扣押發文字號或日期有誤或ID不相符", SeizureGovNo);
            }

            

            CaseMaster SeizureCase = SeizureCases.FirstOrDefault(); // 預設第一個
            List<CaseMaster> CancelCases = new List<CaseMaster>(); // 記錄, 若有相同案號, 要撒銷的案.......
            List<CaseMaster> AllCases = new List<CaseMaster>(); 
            //Case 41, 26, 都是有二個扣押案... 26, 其中一個要撒銷... 41: 其中一個無往來, 
            if (SeizureCases.Count() > 1 ) // 大於等於2以上, 都是錯誤的....
            {

                WriteLog(String.Format("\t\t原扣押有一個以上, 分別是 {0} ", string.Join(" / ", SeizureCases.Select(x=>x.CaseNo))));
                WriteLog("\t\t試圖排除非合法的案件.......");
                foreach(var c in SeizureCases)
                {
                    var cSeizureList = new CaseAccountBiz().GetCaseSeizure(c.CaseId, null);
                    if (cSeizureList.Count() > 0 && cSeizureList.Sum(x => x.SeizureAmountNtd) > 0)
                        AllCases.Add(c);
                }

                if( AllCases.Count()==1)
                {
                    SeizureCase = AllCases.First();
                    WriteLog(string.Format("\t\t決定取出{0} 進行支付",SeizureCase.CaseNo));
                    CancelCases = null;
                }
                else // 大於1的, 只取第1筆, 其他的都列進CancelCases....
                {
                    SeizureCase = AllCases.First();
                    WriteLog(string.Format("\t\t決定取出{0} 進行支付", SeizureCase.CaseNo));
                    foreach(var c in AllCases)
                    {
                        if (c == SeizureCase)
                            continue;
                        CancelCases.Add(c);
                    }                    
                    WriteLog(string.Format("\t\t待撒銷案件 {0} ", string.Join(" / ", CancelCases.Select(x=>x.CaseNo))));
                    // 20200731, 還要開票....
                    string payMemo3 = getNewMemo(casedoc.GovUnit, casedoc.GovNo);
                    var caseSeizureLists222 = new CaseAccountBiz().GetCaseSeizure(SeizureCase.CaseId, null);
                    int CaseMasterHandleFee3 = getHandelFee(caseSeizureLists222.ToList());
                    WriteCheckWithOutNo_991X(caseSeizureLists222.ToList(), SeizureCase.CaseId, casedoc.CaseId, ldapInfo, payMemo3, CaseMasterHandleFee3);
                    
                    
                    
                    WriteLog("目前決議, 直接落人");



                    if( CancelCases.Count()>0)
                    {
                        return "9909|前扣押案有2筆以上，請確認";
                    }
                }
            }            
            

            if (SeizureCase != null)
            {
                WriteLog(String.Format("\t\t找到扣押案{0} / {1} / {2} ", SeizureCase.DocNo, SeizureCase.GovNo, SeizureCase.GovDate));


                // 比較是否與支付案的GovNo, GovDate相同
                DateTime paydt = DateTime.ParseExact(SeizureGovDate, "yyyMMdd", CultureInfo.InvariantCulture).AddYears(1911);
                DateTime Seidt = DateTime.Parse(SeizureCase.GovDate);
                if (!(SeizureCase.GovNo == SeizureGovNo && Seidt == paydt))
                {
                    return "9905|扣押發文字號或日期有誤";
                }

                //本案件, 準備發文的備註 .. 從來文字號, 取出 前三個字+ 最後6個字
                var govNo = SeizureGovNo.Substring(0, 3) + SeizureGovNo.Substring(SeizureGovNo.Length - 7, 6);

                //20200828, 加入新規則, 要能同時支援新舊規則
                var govNoNew = getNewMemo(SeizureCase.GovUnit, SeizureCase.GovNo);

                IList<CaseSeizure> caseSeizureLists = new CaseAccountBiz().GetCaseSeizure(SeizureCase.CaseId, null);
                // Case 22 : 扣押發文字號或日期有誤
                if (caseSeizureLists.Count() == 0)
                {
                    WriteLog(String.Format("\t\t查無原扣押案, GovNo: {0}的任何扣押帳號", casedoc.GovNo));
                    retStr = "9909|查無原扣押案";
                    return retStr;
                }


                var only1person = PreCalcNewPayAmount(caseSeizureLists.ToList(), casedoc.PreReceiveAmount);

                int CaseMasterHandleFee2 = getHandelFee(caseSeizureLists.ToList());

                if( CaseMasterHandleFee2!=only1person) // Case 35, 原500手續費, 因只支付一個, 手續費變250, 落人
                {
                    WriteLog(String.Format("\t\t來函收取二個義務人，僅須解繳一個義務人之帳戶即可足額支付，請確認手續費後人工作業"));
                    retStr = "9913|來函收取二個義務人，僅須解繳一個義務人之帳戶即可足額支付，請確認手續費後人工作業";

                    // 還要分配...
                    string payMemo3 = getNewMemo(casedoc.GovUnit, casedoc.GovNo);
                    WriteCheckWithOutNo_991X(new CaseAccountBiz().GetCaseSeizure(SeizureCase.CaseId, null).ToList(), SeizureCase.CaseId, casedoc.CaseId, ldapInfo, payMemo3, CaseMasterHandleFee2);

                    return retStr;
                }
                
                WriteLog(String.Format("\t\t手續費{0}", CaseMasterHandleFee2.ToString()));
                // Case 23 : 原扣押0元, 支付金額大於扣押金額
                if( caseSeizureLists.Sum(x=>x.SeizureAmountNtd)==0)
                {
                    WriteLog(String.Format("\t\t原扣押0元, 支付金額大於扣押金額"));
                    retStr = "9909|原扣押0元";
                    return retStr;

                }



                // 20200728,  取得是否有外國人, 若有, 則落B01, 且要分配...
                int ForeignIds = getForeignIDCount(new CaseAccountBiz().GetCaseSeizure(SeizureCase.CaseId).ToList());
                if (ForeignIds > 0)
                {
                    string payMemo3 = getNewMemo(casedoc.GovUnit, casedoc.GovNo);
                    WriteCheckWithOutNo_991X(new CaseAccountBiz().GetCaseSeizure(SeizureCase.CaseId, null).ToList(), SeizureCase.CaseId, casedoc.CaseId, ldapInfo, payMemo3, 250);
                    return "9911|有外國人，落人工";

                }



                // Case 24: 要比較支付來文的前案扣押金額與肚子中的總金額是否一致....
                if (caseSeizureLists.Sum(x => x.SeizureAmountNtd) != casedoc.PreSubAmount + CaseMasterHandleFee2)
                {
                    WriteLog(String.Format("\t\t來文扣押金額有誤"));
                    retStr = "9909|來文扣押金額有誤";
                    return retStr;
                }


                //Case 3: 比對收取金額跟扣押金額.. 若收取金額>扣押金額, 則落人, 不分配
                if( casedoc.PreReceiveAmount + CaseMasterHandleFee2 > casedoc.PreSubAmount + CaseMasterHandleFee2)
                {
                    WriteLog(String.Format("\t\t支付金額大於扣押金額, 原扣押{0}, 來文扣押{1}", caseSeizureLists.Sum(x => x.SeizureAmount).ToString(), casedoc.PreSubAmount));
                    retStr = "9909|支付金額大於扣押金額";
                    return retStr;
                }

                // Case 24 : 原扣押1700元, 來文要支付2700元
                if (caseSeizureLists.Sum(x => x.SeizureAmountNtd) < casedoc.PreReceiveAmount + CaseMasterHandleFee2)
                {
                    WriteLog(String.Format("\t\t來文扣押金額有誤, 原扣押{0}, 來文扣押{1}", caseSeizureLists.Sum(x => x.SeizureAmount).ToString(), casedoc.PreSubAmount));
                    retStr = "9909|來文扣押金額有誤";
                    return retStr;

                }

                
                List<bool> isAllMatch = new List<bool>();
                foreach (var acc in caseSeizureLists.Where(x=>x.SeizureAmount>0))
                {
                    WriteLog(String.Format("\t\t\t檢查帳戶{0} , 是否需要落人工", acc.Account));

                    try
                    {

                        // 20210604, ***************************  ********************************
                        var ret = checkSeizureStatus(acc.CustId, acc.CaseId.ToString(), acc.Account, acc.Currency, govNo, acc.SeizureAmount, casedoc, govNoNew);
                        //string ret = "0000|";
                        // 20210604, ***************************  ********************************

                        if (ret.StartsWith("991")) // 
                        {
                            isAllMatch.Add(false);

                            string payMemo3 = getNewMemo(casedoc.GovUnit, casedoc.GovNo);
                            WriteCheckWithOutNo_991X(caseSeizureLists.ToList(), SeizureCase.CaseId, casedoc.CaseId, ldapInfo, payMemo3, CaseMasterHandleFee2);
                            retStr = ret;
                            break;
                        }
                        if (ret.StartsWith("0000|"))
                        {
                            isAllMatch.Add(true);
                        }
                        else // 若有錯誤, 則return 那個帳戶不匹配, 用 0001|XXXXX^881234567890@0002|XXXXX^991234567890@     的格式
                        {
                            retStr += ret + "\r\n";
                            isAllMatch.Add(false);
                        }
                    }
                    catch (Exception ex)
                    {
                        WriteLog("*********發生發查TX33401, TX45030 或TX45031 有錯誤*****" + ex.Message.ToString());
                        noticeITMail(ITManagerEmail, "發生發查TX33401, TX45030 或TX45031 有錯誤!, 案件編號: " + casedoc.CaseNo + "錯誤訊息: " + ex.Message.ToString());
                        //retStr += ex.Message.ToString() + "\r\n";
                        isAllMatch.Add(false);
                    }                   

                }
                if (isAllMatch.Any(x => !x)) // 表示全部的扣押金, 文號, 跟目前的TX450-31, 有部分的不一致... 
                {
                    return retStr;
                }


               //20200928, 檢查, 開出支票的金額, 是否等於收取金額...
                var Payee222 = new CaseMasterBIZ().EDocTXT3_DetailByCaseId(casedoc.CaseId); 
               
                int TotalCheckAmount = 0;
                foreach(var dist in Payee222)
                {
                    int checkAmount = 0;
                    if (int.TryParse(dist.ReceiveAmount_Case, out checkAmount))
                    {
                        TotalCheckAmount += checkAmount;
                    }
                }

                // 若不相等則.. 開票, 不取票號( 收取金額≠分配金額)
                if(casedoc.PreReceiveAmount!=TotalCheckAmount )
                {
                    string payMemo3 = getNewMemo(casedoc.GovUnit, casedoc.GovNo);
                    WriteCheckWithOutNo_991X(new CaseAccountBiz().GetCaseSeizure(SeizureCase.CaseId, null).ToList(), SeizureCase.CaseId, casedoc.CaseId, ldapInfo, payMemo3, 250);
                    return "9911| 收取金額≠分配金額，落人工";
                }


                WriteLog(String.Format("\t\t\t以上帳戶均通過檢查... "));
            }
            else
            {
                WriteLog(String.Format("找不到原扣押案, GovNo: {0}", casedoc.GovNo));
                retStr = "9908|找不到原扣押案";
                return retStr;
            }


            WriteLog(String.Format("\t\t step 2: 比對扣押金額與支付金額"));
            // step 2: 比對扣押金額與支付金額
            // 找出原扣押案...  每個帳戶的扣押金額         
            IList<CaseSeizure> payLists = caBiz.GetCaseSeizure(SeizureCase.CaseId, null);



            




            int CaseMasterHandleFee = 0;
            int SurplusMoney = 0; // 若是多扣押的錢, 要填入此數字
            // 比對GovUnit, GovDate, SeizureAmount,  是否相同
            // 回覆  =, >, <            
            string AmountCompare = CompareFields(casedoc, SeizureCase, SeizureGovNo, ref CaseMasterHandleFee, ref SurplusMoney);
            switch (AmountCompare)
            {
                case ">":
                    WriteLog("\t支付金額 > 扣押金額  ==> 依TXT收取分配媒體檔資訊開立支票，不取票號後落收發作業/待辦理(經辦人SYS)且不分配");



                    retStr = "9909|支付金額 > 扣押金額, 落人工";
                    return retStr;
                case "=":
                    WriteLog("\t支付金額=原扣押案 金額相同");


                    // 準備解扣, 必須先把每個在CaseSeizure中的支付金額, 填入
                    foreach (var item in payLists)
                    {
                        bool retInt = caBiz.updateCaseSeizurePayAmount(item.SeizureId, casedoc.CaseId, item.SeizureAmount,"3");
                    }
                    // 再讀取一次CaseSeizure, 才有PayAmount的值, 因為每個帳戶解扣的金額, 是讀取PayAmont...
                    payLists = caBiz.GetCaseSeizure(SeizureCase.CaseId, null).Where(x=>x.SeizureAmount>0).ToList();

                    // 找出支付的文號, 並縮成備註....(由payGovNo中, 取)
                    string payMemo = getNewMemo(casedoc.GovUnit, casedoc.GovNo);
                    try
                    {
                        retStr = WriteCheckWithNo(payLists.ToList(), SeizureCase.CaseId, casedoc.CaseId, ldapInfo, payMemo, CaseMasterHandleFee);
                        if (retStr.StartsWith("9917|"))
                        {
                            // 要分配
                            string payMemo3 = getNewMemo(casedoc.GovUnit, casedoc.GovNo);
                            WriteCheckWithOutNo_991X(new CaseAccountBiz().GetCaseSeizure(SeizureCase.CaseId, null).ToList(), SeizureCase.CaseId, casedoc.CaseId, ldapInfo, payMemo3, CaseMasterHandleFee);
                        }
                    }
                    catch (Exception ex)
                    {
                        WriteLog("*****發查電文9099 異常" + ex.Message.ToString());
                        noticeITMail(ITManagerEmail, "*****發查電文9099 異常 案件編號: " + casedoc.CaseNo + "錯誤訊息: " + ex.Message.ToString());
                        return "9909|發查電文9099";
                    }

                    break;
                case "<":
                    WriteLog("\t支付金額 < 扣押金額");
                    // 要讀取CaseMaster.OverCancel欄位, 決定否要撒銷
                    isOverCancel = casedoc.OverCancel.Trim() == "Y" ? true : false;
                    if (isOverCancel)
                    {
                        // 餘下的錢, 要撒銷....
                        // 若扣押數個帳號，支付金額依外來文系統扣押排序填載支付金額，最後一個或數個扣押金額≠支付金額，解除扣押金額，在依支付金額扣押及設解扣日
                        // 若執行<TX:9093-1-1>回應訊息為「超額(待確認)」則落人工
                        // 撤銷完發現無法設解扣日期時需沖正

                        WriteLog(string.Format("\t\t支付金額 < 扣押金額 , 多餘的錢 共計{0} , 要自動撒銷!!", SurplusMoney.ToString()));
                        // 依據當初扣押的排序,來支付金額...

                        // 重新計算每個帳戶
                        List<CaseSeizure> cancelList = new List<CaseSeizure>();
                        IList<CaseSeizure> payLists_Temp = CalcNewPayAmount(payLists , casedoc.ReceiveAmount + CaseMasterHandleFee);
                        payLists_Temp.ToList().ForEach(x =>
                        {
                            WriteLog(string.Format("\t\t\t要支付的帳號: {0}, 支付金額: {1}", x.Account, x.PayAmount));
                        });
                        

                        // 準備解扣, 必須先把每個在CaseSeizure中的支付金額, 填入
                        foreach (var item in payLists_Temp)
                        {
                            bool retInt = caBiz.updateCaseSeizurePayAmount(item.SeizureId, casedoc.CaseId, item.PayAmount, "3");
                        }
                        // 再讀取一次CaseSeizure, 才有PayAmount的值, 因為每個帳戶解扣的金額, 是讀取PayAmont..., Case 36, 而且要有扣押的帳戶
                        payLists = caBiz.GetCaseSeizure(SeizureCase.CaseId, null).Where(x=>x.SeizureAmount>0).ToList();


                        // 找出支付的文號, 並縮成備註....(由payGovNo中, 取)
                        string payMemo2 = getNewMemo(casedoc.GovUnit, casedoc.GovNo);


                        try
                        {
                            retStr = WriteCheckWithNo(payLists.ToList(), SeizureCase.CaseId, casedoc.CaseId, ldapInfo, payMemo2, CaseMasterHandleFee);
                            if (retStr.StartsWith("9917|"))
                            {
                                // 要分配
                                string payMemo3 = getNewMemo(casedoc.GovUnit, casedoc.GovNo);
                                WriteCheckWithOutNo_991X(new CaseAccountBiz().GetCaseSeizure(SeizureCase.CaseId, null).ToList(), SeizureCase.CaseId, casedoc.CaseId, ldapInfo, payMemo3, CaseMasterHandleFee);
                            }
                            if (!retStr.StartsWith("9900|")) // 表示, 發電文期間, 有錯誤...直接Return
                            {
                                return retStr;
                            }
                        }
                        catch (Exception ex)
                        {
                            WriteLog("*****發查電文9099 異常1" + ex.Message.ToString());
                            noticeITMail(ITManagerEmail, "*****發查電文9099 異常1 案件編號: " + casedoc.CaseNo + "錯誤訊息: " + ex.Message.ToString());
                            return "9909|發查電文9099";
                        }
                        return "9900|支付成功";
                    }
                    else // 20200724, 芯瑜說, N, 一律落人工, 不打電文, 落收發
                    {
                        // 依TXT收取分配媒體檔資訊開立支票，不取票號後落收發作業/待辦理(經辦人SYS)
                        WriteLog(string.Format("\t\t支付金額 < 扣押金額 , 多餘的錢 共計{0} , 但不要自動撒銷, 開票不取票號, 落人工!!", SurplusMoney.ToString()));
                        // 準備解扣, 必須先把每個在CaseSeizure中的支付金額, 填入
                        foreach (var item in payLists)
                        {
                            bool retInt = caBiz.updateCaseSeizurePayAmount(item.SeizureId, casedoc.CaseId, item.SeizureAmount,"3");
                        }
                        // 再讀取一次CaseSeizure, 才有PayAmount的值, 因為每個帳戶解扣的金額, 是讀取PayAmont...
                        payLists = caBiz.GetCaseSeizure(SeizureCase.CaseId, null);

                        // 找出支付的文號, 並縮成備註....(由payGovNo中, 取)
                        //string payMemo2 = getNewMemo(casedoc.GovUnit, casedoc.GovNo);
                        //retStr = WriteCheckWithOutNo(payLists.ToList(), SeizureCase.CaseId, casedoc.CaseId, ldapInfo, payMemo2, CaseMasterHandleFee);

                        // 要分配
                        string payMemo3 = getNewMemo(casedoc.GovUnit, casedoc.GovNo);
                        WriteCheckWithOutNo_991X(new CaseAccountBiz().GetCaseSeizure(SeizureCase.CaseId, null).ToList(), SeizureCase.CaseId, casedoc.CaseId, ldapInfo, payMemo3, CaseMasterHandleFee);
   
                        return "9919|超過收取金額部分是否撤銷欄位為「否」，請人工作業";
                    }

                    break;
            }








            return retStr;
        }
        /// <summary>
        /// 撒銷帳戶, 若扣押>支付 的部分
        /// </summary>
        /// <param name="cancelList"></param>
        /// <param name="SeizureCase"></param>
        /// <param name="ldapInfo"></param>
        /// <returns></returns>
        private static string CancelCaseSeizure(List<CaseSeizure> cancelList, CaseMaster SeizureCase, string[] ldapInfo)
        {
            string retStr = string.Empty;
            bool retBool = true;
            foreach (var c in cancelList)
            {
                var reply = CancelTelegram(c, ldapInfo);
                if (!reply)
                {
                    retStr += string.Format("撒銷帳戶{0}, 有錯", c.Account);
                }
                retBool = retBool & reply;
            }
            return retBool ? "0000|撒銷成功" : "9911|撒銷有錯, " + retStr;
        }

        private static int PreCalcNewPayAmount(IList<CaseSeizure> list, int PayAmount)
        {
            List<CaseSeizure> PayResult = new List<CaseSeizure>();
            //List<CaseSeizure> CancelResult = new List<CaseSeizure>();
            list = list.OrderByDescending(x => x.SeizureAmountNtd).ToList(); //先按照當初扣押的順序,
            decimal total = 0;
            bool isEnough = false;
            foreach (var s in list)
            {
                if (isEnough) // 表示已累績足夠支付額, 所以一律都要撤銷
                {
                    CaseSeizure s1 = deepCopy(s);
                    s1.PayAmount = 0; // 表示此帳戶, 要撤銷(不支付任何錢)
                    PayResult.Add(s1);
                    continue;
                }

                total += s.SeizureAmountNtd;
                if (total < PayAmount && !isEnough) // 表示還沒有累積到PayAmount, 所以直接加入支付
                {
                    CaseSeizure s1 = deepCopy(s);
                    s1.PayAmount = s.SeizureAmount;
                    PayResult.Add(s1);
                }

                if (total > PayAmount && !isEnough) // 表示只到這個帳戶而已, 要把此帳戶扣押金額, 切成, 要支付, 跟要撤銷二種......
                {
                    CaseSeizure s1 = deepCopy(s);
                    s1.PayAmount = s.SeizureAmountNtd - (total - PayAmount);
                    PayResult.Add(s1);

                    //CaseSeizure s2 = deepCopy(s);
                    //s2.PayAmount = s.SeizureAmount - s1.PayAmount; // 預定支付的錢
                    //// 保留s2.SeizureAmout , 等於原案的錢, 以備先撒銷..
                    ////s2.SeizureAmount = s2.PayAmount;
                    //CancelResult.Add(s2);
                    isEnough = true;
                }


                if (total == PayAmount) // 表示就到這個帳戶
                {
                    CaseSeizure s1 = deepCopy(s);
                    s1.PayAmount = s.SeizureAmount;
                    PayResult.Add(s1);
                    isEnough = true;
                }

            }

            
            int HandleFee = getHandelFee(PayResult.Where(x => x.PayAmount > 0).ToList());

            return HandleFee;
        }


        private static IList<CaseSeizure> CalcNewPayAmount(IList<CaseSeizure> list, int PayAmount)
        {
            List<CaseSeizure> PayResult = new List<CaseSeizure>();
            //List<CaseSeizure> CancelResult = new List<CaseSeizure>();
            list = list.OrderByDescending(x => x.SeizureAmount).ToList(); //先按照當初扣押的順序,
            decimal total = 0;
            bool isEnough = false;
            foreach (var s in list)
            {
                if (isEnough) // 表示已累績足夠支付額, 所以一律都要撤銷
                {
                    CaseSeizure s1 = deepCopy(s);
                    s1.PayAmount = 0; // 表示此帳戶, 要撤銷(不支付任何錢)
                    PayResult.Add(s1);
                    continue;
                }

                total += s.SeizureAmount;
                if (total < PayAmount && !isEnough) // 表示還沒有累積到PayAmount, 所以直接加入支付
                {
                    CaseSeizure s1 = deepCopy(s);
                    s1.PayAmount = s.SeizureAmount;
                    PayResult.Add(s1);
                }

                if (total > PayAmount && !isEnough) // 表示只到這個帳戶而已, 要把此帳戶扣押金額, 切成, 要支付, 跟要撤銷二種......
                {
                    CaseSeizure s1 = deepCopy(s);
                    s1.PayAmount = s.SeizureAmount - (total - PayAmount);
                    PayResult.Add(s1);

                    //CaseSeizure s2 = deepCopy(s);
                    //s2.PayAmount = s.SeizureAmount - s1.PayAmount; // 預定支付的錢
                    //// 保留s2.SeizureAmout , 等於原案的錢, 以備先撒銷..
                    ////s2.SeizureAmount = s2.PayAmount;
                    //CancelResult.Add(s2);
                    isEnough = true;
                }


                if (total == PayAmount) // 表示就到這個帳戶
                {
                    CaseSeizure s1 = deepCopy(s);
                    s1.PayAmount = s.SeizureAmount;
                    PayResult.Add(s1);
                    isEnough = true;
                }


            }

            return PayResult;
        }


        private static CaseSeizure deepCopy(CaseSeizure source)
        {
            var serialized = JsonConvert.SerializeObject(source);
            return JsonConvert.DeserializeObject<CaseSeizure>(serialized);
        }



        /// <summary>
        /// 開立支票
        /// </summary>
        /// <param name="SeizureCase">當初扣押CaseSeizure</param>
        /// <param name="CaseId">扣押Caseid</param>
        /// <param name="payCaseId">支付CaseId</param>
        /// <param name="ldapInfo"></param>
        /// <param name="payGovNo">支付來文的文號</param>
        /// <returns></returns>
        private static string WriteCheckWithNo(List<CaseSeizure> SeizureCase, Guid CaseId, Guid payCaseId, string[] ldapInfo, string payMemo, int CaseMasterHandleFee)
        {
            CaseSendSettingBIZ sendBiz = new CaseSendSettingBIZ();
            var payCaseMaster = new CaseMasterBIZ().MasterModelNew(payCaseId);
            var caBiz = new CaseAccountBiz();
            //計算, 支付金額選定帳號邏輯為扣押金額大->小
            var paySequences = SeizureCase.OrderByDescending(x => x.SeizureAmountNtd).ThenBy(x=>x.CustId.Length).ToList();
            //List<CaseSeizure> success = new List<CaseSeizure>();
            //Dictionary<string, string> fails = new Dictionary<string, string>();
            foreach (var p in paySequences)
            {
                // 20210604, ***************************  ********************************
                var ret = PayTelegram(p, payMemo, payCaseId, BreakDay, "Pay", ldapInfo);
                //string ret = "true";
                // 20210604, ***************************  ********************************

                if (ret.StartsWith("true")) //表示打電文成功
                {
                    caBiz.updateCaseSeizurePayAmount(p.SeizureId, payCaseId, p.PayAmount, "1");
                }
                else // 失敗
                {
                    caBiz.updateCaseSeizurePayAmount(p.SeizureId, payCaseId, 0, "3"); // 20200803 當SeizureStatus='3'在網頁上支援時, 要改成'3'
                    return ret;
                }
            }

            // 20200807, 失敗的, 不必沖正, 是打完9093, 若失敗, 才要沖正, 若其他帳戶成功了, 就成了.. 留著.....
            //if (fails.Count() > 0) // 表示數個帳戶中, 其中有打9099失敗的...要沖正, 並且回復失敗
            //{
            //    // 把其中, 剛剛成功的, 沖正
            //    foreach (var p in success)
            //    {
            //        var ret = PayTelegram(p, payMemo, CaseId, BreakDay, "Reset", ldapInfo);
            //    }

            //    List<string> sb = new List<string>();
            //    foreach (var k in fails)
            //        sb.Add(k.Value);
            //    WriteLog("\t支付時, 發生錯誤:" + string.Join(" / ", sb));
                
            //    return "9917|" + string.Join(" \r\n ", sb);
            //}
            WriteLog("\t\t開始開立支票 .....");

            var cpsBiz = new CasePayeeSettingBIZ();
            var master = new CaseMasterBIZ().EDocTXT3ByCaseId(payCaseId);

            // 跟據要開票的對象.. 去湊出那個帳號, 要解扣多少錢....
            //var edoctxt3 = new CaseMasterBIZ().EDocTXT3ByCaseId(payCaseId);
            // 到edoctxt_detail找出開票對象, 存到payee中
            var Payee = new CaseMasterBIZ().EDocTXT3_DetailByCaseId(payCaseId); // 讀取分配媒體檔, 有822開頭的...
            IDbTransaction trans = null;

            bool firstPayee = true;

            // Bank, BankID, 要從CaseSeizure 被指定支付的帳戶裏面, 抓出分行別與分行ID, 並用"；"區隔....
            string _bank = string.Join("；", paySequences.Select(x => x.BranchName));
            string _bankid = string.Join("；", paySequences.Select(x => x.BranchNo));
            foreach (var dist in Payee )
            {
                int checkAmount = 0;

                if (int.TryParse(dist.ReceiveAmount_Case, out checkAmount))
                {
                    if (checkAmount <= 0)
                        continue;
                }
                else
                {
                    WriteLog(string.Format("\t\t\t支票金額有誤 {0} ", dist.ReceiveAmount_Case));
                    continue;
                };
                int checkAmout = int.Parse( dist.ReceiveAmount_Case);
   
                // 取出票號
                var cnsBiz = new CheckNoSettingBIZ();
                CheckUse check = cnsBiz.GetNoUseCheck(null);
                string checkNo = check.CheckNo.ToString();
                string checkIntervalId = check.CheckIntervalId.ToString();
                // 20200826, 發現有開票, 但沒有取出票號...

                // 設定上面 的支票, 已使用...
                cnsBiz.SettingForUseCheck(new CheckNoSetting { Kind = CheckNoUseKind.CasePay, IsUsed = 1, CheckNo = check.CheckNo, CheckIntervalID = check.CheckIntervalId }, trans);


                //20200617, 參考CasePayeeSettingBiz.cs 中的第188行.. 產出發文檔...
                // 先弄出一個CasePayeeSetting 的model... 這應該 , 就是從媒體分配檔讀進來...
                // Bank, BankID, 要從CaseSeizure 被指定支付的帳戶裏面, 抓出分行別與分行ID, 並用"；"區隔....

                var model = new CasePayeeSetting()
                {
                    Address = dist.CheckAddress,
                    Bank = _bank,
                    BankID = _bankid,
                    CaseId = payCaseId,
                    CaseKind = "支付",
                    CheckNo = checkNo,
                    CaseNo = payCaseMaster.CaseNo,
                    CheckIntervalId = checkIntervalId,
                    CCReceiver = new GovAddressBIZ().GetEnabledGovAddrByGovName(master.GovUnit), // eDocTXT3.GovUnit 的地址, 要去GovAddress查...
                    Currency = master.GovUnit, // eDocTXT3.GovUnit
                    ReceivePerson = dist.ReceiveName, // eDocTXT3_Detail.ReceiveName
                    Receiver = dist.ReceiveUnit, // eDocTXT3_Detail.ReceiveUnit
                    PayeeAction = 2, // 或1,2,3,4 // 取號存檔
                    Money = dist.ReceiveAmount_Case　,
                    PayDate = BreakDay,
                    //20220627, 後來說, 要用固定值250元... 
                    Fee = firstPayee ? CaseMasterHandleFee.ToString() : "0",
                    //Fee = CaseMasterHandleFee.ToString(),
                    MoneySum = cpsBiz.PayAmountSum(master.CaseId), //select ISNULL(Sum(Money + Fee),0) from CasePayeeSetting where CaseId=@CaseId AND [PayeeId] <> @PayeeId
                    PayAmountSum = cpsBiz.PayAmountSum(master.CaseId) // select ISNULL(Sum(PayAmount),0) from CaseSeizure where PayCaseId=@CaseId
                };

                firstPayee = false;

                WriteLog(string.Format("\t\t\t票號: {0}, 對象: {1}, $: {2} ", model.CheckNo, model.ReceivePerson, model.Money));


                cpsBiz.InsertCasePayeeSetting2(model, trans);




                //* 新增發文
                string errMsg;
                //* 取得初始的發文資訊資料
                // 2021-08-20要求以參數檔來切換新舊回文版面
                bool isAutopay = false;
                if (isAutoPayReply) // 全域變變, 再一開始, 就去取參數檔的值, 新舊回文版面
                    isAutopay = true;

                CaseSendSettingCreateViewModel caseSendModel = sendBiz.GetDefaultSendSetting(model, out errMsg, trans, ldapInfo[0], autopay: isAutopay);
                if (!string.IsNullOrEmpty(errMsg) || caseSendModel == null)
                {
                    //* 這裡面能出錯.也就發票號碼到最大或者沒設定了
                    trans.Rollback();
                    WriteLog("Error -->這裡面能出錯.也就發票號碼到最大或者沒設定了");
                    return "Error";
                }
                caseSendModel.SendDate = model.PayDate; // 看起來, 是去讀CASEMASTER.PayDate....
                caseSendModel.SendKind = "電子發文";                
                caseSendModel.GovName = dist.ReceiveUnit;
                caseSendModel.GovAddr = dist.CheckAddress;
                caseSendModel.GovNameCc = master.GovUnit;
                caseSendModel.GovAddrCc = model.CCReceiver;
                caseSendModel.Speed = "普通件";

                // 取得 ParmCode 中, CodeType='SendGovName' 的CodeDesc
                PARMCodeBIZ pbiz = new PARMCodeBIZ();
                var pz = pbiz.GetParmCodeByCodeType("SendGovName").FirstOrDefault();
                string strCodeDesc = "";
                if (pz != null)
                {
                    strCodeDesc = pz.CodeDesc;
                }
                caseSendModel.SendWord = caseSendModel.SendKind == "紙本" ? Lang.csfs_ctci_bank : strCodeDesc;



                //* 20150518 儲存時同時存發文設定
                bool rtn1 = sendBiz.SaveCreate2(caseSendModel, trans);
                //* 回填SendId
                bool rtn2 = cpsBiz.UpdateCasePayeeSettingSendNo(model.CheckIntervalId, model.CheckNo, caseSendModel.SerialId, trans);
                WriteLog(string.Format("\t\t\t新增發文, SendNo:  {0} ", caseSendModel.SendNo));
            }

            // 更新CaseMaster.AddCharge 欄位...
            new CaseMasterBIZ().UpdateCaseMasterAddCharge(payCaseId, CaseMasterHandleFee);

            return "9900|支付成功";
        }


        
        /// <summary>
        /// 取出備註
        /// </summary>
        /// <param name="payGovNo"></param>
        /// <returns></returns>
        private static string getNewMemo(string govUnit, string payGovNo)
        {
            //20200624, 新的方法
            // 1. 若是執行署, 則取(第一個中文字)+執+(執後第一個字) + 號(前面的六碼)
            // 2. 若是法院, 則取 (院前所有字)+(字前一個字)+號(前面六碼)

            if (govUnit.IndexOf("執行署") > 0)
            {

                var pos1 = payGovNo.IndexOf("執") + 1;
                var pos2 = payGovNo.LastIndexOf("號");
                string f1 = payGovNo.Substring(0, 1) + "執" + payGovNo.Substring(pos1, 1);
                string f2 = payGovNo.Substring(pos2 - 6, 6);
                return f1 + f2;
            }
            if (govUnit.IndexOf("地方法院") > 0)
            {
                var pos1 = payGovNo.IndexOf("院") + 1;
                var pos2 = payGovNo.LastIndexOf("字第");
                var pos3 = payGovNo.LastIndexOf("號");
                string f1 = payGovNo.Substring(0, pos1);
                string f2 = payGovNo.Substring(pos2 - 1, 1);
                string f3 = payGovNo.Substring(pos3 - 6, 6);
                return f1 + f2 + f3;

            }
            return "";
        }



        /// <summary>
        /// 支付電文
        /// </summary>
        /// <param name="item"></param>
        /// <param name="memo"></param>
        /// <param name="CaseId"></param>
        /// <param name="BreakDay">解扣日</param>
        /// <param name="kind">Pay /  Reset</param>
        /// <param name="LogonUser"></param>
        /// <returns></returns>
        public static string PayTelegram(CaseSeizure item, string memo, Guid CaseId, DateTime BreakDay, string kind, string[] ldapInfo)
        {
            AutoLogBIZ alBiz = new AutoLogBIZ();
            bool rtn = false;
            string result = "發查電文后,更新表失敗";
            ExecuteHTG objHTG = new ExecuteHTG(ldapInfo[0], ldapInfo[1], ldapInfo[2], ldapInfo[3], ldapInfo[4]);
            string retP = "";
            string BreakDayToTelegram = "";//發電文用的解扣日
            string BreakDayFormat = "";//更新支付時間用的日期



            //if (!string.IsNullOrEmpty(BreakDay))
            {
                BreakDayFormat = BreakDay.ToString("yyyy/MM/dd");
                //BreakDayToTelegram = Convert.ToDateTime(BreakDayFormat).Date.ToString("ddMMyyyy");//解扣日格式日月年13062018
                BreakDayToTelegram = BreakDay.ToString("ddMMyyyy");//解扣日格式日月年13062018
            }
            if (kind == "Pay")//支付
            {

                // 20200807, 不要9099打電文.. 一律先9095, 再9093
                //if (item.PayAmount == item.SeizureAmount)//支付金額=扣押金額
                //{
                //    WriteLog(string.Format("\t\t準備開始支付帳戶{0}, 金額{1}", item.Account, item.SeizureAmount.ToString()));
                    
                //    retP = objHTG.Send9099(item.Account, item.SeizureAmount, item.Currency, "66", memo, item.CustId, CaseId.ToString(), BreakDayToTelegram);
                //    alBiz.addAutoLog(CaseId, eQueryStaff, item.Account, item.CustId, "9099",1, retP);
                //    WriteLog(string.Format("\t\t支付完成, 回應碼{0}", retP));
                //}
                //else//支付金額≠扣押金額
                {
                    CaseMaster sei = new CaseMasterBIZ().MasterModelNew(item.CaseId, null);
                    string seiMemo = getNewMemo(sei.GovUnit, sei.GovNo);

                    
                    retP = objHTG.Send9095(item.Account, item.SeizureAmount, item.Currency, "66", seiMemo, item.CustId, item.CaseId.ToString(), ""); //-- 解扣
                    
                    //retP = objHTG.Send9092Or9095(item.Account, item.SeizureAmount, item.Currency, "", memo, item.CustId, CaseId.ToString(), "0", "");
                    if (retP.StartsWith("0000"))//IR-0164 上一個電文成功了才能打下一個電文
                    {
                        
                        alBiz.addAutoLog(CaseId, eQueryStaff, item.Account, item.CustId, "9095", 1, retP);
                        WriteLog(string.Format("\t\t撒銷帳號{0}成功, 回應碼{1}", item.Account, retP));
                        if (item.PayAmount > 0) //adam20181122 支付0元不發電文
                        {
                            
                            retP = objHTG.Send9091Or9093(item.Account, item.PayAmount, item.Currency, "66", memo, item.CustId, CaseId.ToString(), "0", BreakDayToTelegram);
                            if (retP.StartsWith("0000"))
                            {                                
                                alBiz.addAutoLog(CaseId, eQueryStaff, item.Account, item.CustId, "9093",1, retP);
                                WriteLog(string.Format("\t\t支付帳號{0}成功, 回應碼{1}", item.Account, retP));
                            }
                            else
                            {                                
                                alBiz.addAutoLog(CaseId, eQueryStaff, item.Account, item.CustId, "9093", 0, retP);
                                WriteLog(string.Format("\t\t支付帳號{0}失敗, 回應碼{1}", item.Account, retP));
                                WriteLog(string.Format("\t\t啟動沖正帳號{0} ..... ", item.Account, retP));
                                var retP1 = objHTG.Send9091Or9093(item.Account, item.SeizureAmount, item.Currency, "66", seiMemo, item.CustId, item.CaseId.ToString(), "0", "");
                                if (retP1.StartsWith("0000|"))
                                {
                                    WriteLog(string.Format("\t\t啟動沖正帳號{0} 成功 ", item.Account, retP));
                                }
                                else
                                {
                                    WriteLog(string.Format("\t\t啟動沖正帳號{0} 失敗 ", item.Account, retP));
                                }
                                //20220726, 發現, 若沖正成功, 會被視為支付成功, 回傳0000, 造成錯誤..
                                // 所以要把回傳的訊息, 改為991X 開頭, 才會視為失敗, 落人工....
                                if (retP.Contains("|"))
                                {
                                    retP = retP.Split('|')[1];
                                }
                                retP = "9919|" + retP;
                                return retP;
                            }
                        }
                    }
                    else
                    {
                        WriteLog(string.Format("\t\t撒銷帳號{0}失敗, 回應碼{1}", item.Account, retP));
                        alBiz.addAutoLog(CaseId, eQueryStaff, item.Account, item.CustId, "9095", 0, retP);
                        // 若是發查9095失敗.. 可能是備償帳戶, 則直接落人
                        return string.Format("9917|帳號{0}, 交易限制，請人工作業", item.Account);
                    }
                }
            }
            else if (kind == "Reset")//
            {
                #region 沖正


                //解扣日 IR-0144
                CaseMasterBIZ master = new CaseMasterBIZ();
                string ResetDay = master.GetPayDate(CaseId);//日期格式日月年12092018
                string oldmemo = string.Empty;//原扣押文號 IR-0144
                if (!string.IsNullOrEmpty(item.GovNo) && item.GovNo.Length >= 7)
                {
                    oldmemo = item.GovNo.Substring(0, 3) + item.GovNo.Substring(item.GovNo.Length - 7, 6);
                }
                else
                {
                    oldmemo = item.GovNo;
                }
                if (item.PayAmount == item.SeizureAmount)//支付金額=扣押金額
                {                    
                    retP = objHTG.Send9099Reset(item.Account, item.PayAmount, item.Currency, "66", oldmemo, item.CustId, CaseId.ToString(), ResetDay);//IR-0184
                    alBiz.addAutoLog(CaseId, eQueryStaff, item.Account, item.CustId, "9099Reset", 1, retP);
                }
                else//支付金額≠扣押金額
                {                    
                    retP = objHTG.Send9092Or9095(item.Account, item.PayAmount, item.Currency, "66", memo, item.CustId, CaseId.ToString(), "0", ResetDay);//9095(KEY解扣日)
                    if (retP.StartsWith("0000"))//IR-0164 上一個電文成功了才能打下一個電文
                    {
                        alBiz.addAutoLog(CaseId, eQueryStaff, item.Account, item.CustId, "9095", 1, retP);
                        if (item.PayAmount > 0) ////adam20181122 支付0元不發電文
                        {
                            retP = objHTG.Send9091Or9093(item.Account, item.SeizureAmount, item.Currency, "66", oldmemo, item.CustId, CaseId.ToString(), "0", "");//原扣押文號
                            alBiz.addAutoLog(CaseId, eQueryStaff, item.Account, item.CustId, "9093", 1, retP);
                        }
                    }
                }
                #endregion
            }




            if (retP.StartsWith("0000"))
            {
                //CaseSeizure csOld = GetCaseSeizureInfo(CaseId, item.SeizureId);
                var caBiz = new CaseAccountBiz();
                CaseSeizure csOld = caBiz.GetCaseSeizureInfoBySeizureId(item.SeizureId);
                if (kind == "Pay")
                {
                    if (item.PayAmount == item.SeizureAmount)//支付金額=扣押金額
                    {
                        item.TripAmount = item.SeizureAmount;//解扣金額,同扣押金額
                    }
                    else//支付金額≠扣押金額
                    {
                        
                        item.TripAmount = item.PayAmount;//解扣金額,同支付金額
                        // 20200730, 若有撒銷, 也要填入
                        //item.TripAmount = item.SeizureAmount - item.PayAmount;
                    }
                    CaseMasterBIZ masterBiz = new CaseMasterBIZ();
                    rtn = masterBiz.UpdateCaseMasterPayDate(CaseId, BreakDayFormat, "", null, "Pay");//更新CaseMaster表的支付日期PayDate
                    
                }
                else if (kind == "Reset")
                {
                    item.TripAmount = 0;//解扣金額,金額變為0
                    item.PayAmount = 0;//支付金額,金額變為0
                }
                rtn = caBiz.UpdatePaySetting(item);

                Guid TXSNO = Guid.NewGuid();
                DateTime TXDateTime = DateTime.Parse(System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")).AddMilliseconds(5);
                LdapEmployeeBiz empBiz = new LdapEmployeeBiz();
                LDAPEmployee empNow = empBiz.GetLdapEmployeeByEmpId(ldapInfo[0]);
                #region 支付電文Log記錄
                CaseDataLog log = new CaseDataLog();
                log.TXType = "發查支付電文";
                log.TXUser = empNow != null && !string.IsNullOrEmpty(empNow.EmpId) ? empNow.EmpId : " ";
                log.TXUserName = empNow != null && !string.IsNullOrEmpty(empNow.EmpName) ? empNow.EmpName : " ";
                log.md_FuncID = "Menu.CollectionToAgent";
                log.TITLE = "經辦作業-待辦理";

                #region 支付金額
                log.TXSNO = TXSNO;
                log.TXDateTime = TXDateTime;
                log.ColumnID = "PayAmount";
                log.ColumnName = "支付金額";
                log.ColumnValueBefore = csOld.PayAmount.ToString();
                log.ColumnValueAfter = item.PayAmount.ToString();
                log.TabID = "Tab2-1";
                log.TabName = "支付設定";
                log.TableName = "CaseSeizure";
                log.DispSrNo = 17;
                log.TableDispActive = "1";
                log.CaseId = CaseId.ToString();
                log.LinkDataKey = item.SeizureId.ToString();
                caBiz.InsertCaseDataLog(log);
                #endregion

                #region 解扣金額
                log.TXSNO = TXSNO;
                log.TXDateTime = TXDateTime;
                log.ColumnID = "TripAmount";
                log.ColumnName = "解扣金額";
                log.ColumnValueBefore = csOld.TripAmount.ToString();
                log.ColumnValueAfter = item.TripAmount.ToString();
                log.TabID = "Tab2-1";
                log.TabName = "支付設定";
                log.TableName = "CaseSeizure";
                log.DispSrNo = 18;
                log.TableDispActive = "1";
                log.CaseId = CaseId.ToString();
                log.LinkDataKey = item.SeizureId.ToString();
                caBiz.InsertCaseDataLog(log);
                #endregion
                #endregion
                if (rtn)
                {
                    result = "true";
                }
            }
            else
            {
                result = retP.Substring(5);
            }
            return result;
        }



        public static bool CancelTelegram(CaseSeizure item, string[] ldapInfo)
        {
            AutoLogBIZ alBiz = new AutoLogBIZ();
            ExecuteHTG objHTG = new ExecuteHTG(ldapInfo[0], ldapInfo[1], ldapInfo[2], ldapInfo[3], ldapInfo[4]);
            CaseMaster sei = new CaseMasterBIZ().MasterModelNew(item.CaseId, null);
            string seiMemo = getNewMemo(sei.GovUnit, sei.GovNo);

            string retP = objHTG.Send9095(item.Account, item.SeizureAmount, item.Currency, "66", seiMemo, item.CustId, item.CaseId.ToString(), ""); //-- 解扣
            alBiz.addAutoLog(item.CaseId, eQueryStaff, item.Account, item.CustId, "9095", 1, retP);
            return retP.StartsWith("0000") ? true : false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ObligorNo"></param>
        /// <param name="caseid"></param>
        /// <param name="Account"></param>
        /// <param name="Ccy"></param>
        /// <returns></returns>
        private static string checkSeizureStatus(string ObligorNo, string caseid, string Account, string Ccy, string govNo, decimal SeizureAmount, CaseMaster payCase, string govNoNew)
        {
            AutoLogBIZ alBiz = new AutoLogBIZ();
            // 若是1. 帳號號碼為前三碼006………打頭,  2. 帳號幣別為外幣
            if (Account.StartsWith("00000006"))
            {
                return String.Format("9914|扣押之帳戶中有定存帳戶", Account);
            }
            WriteLog(String.Format("\t\t\t\t檢查帳戶{0} 不是定存", Account));

            if (Ccy != "TWD")
            {
                return String.Format("9915|扣押之帳戶中有外幣帳戶", Account);
            }
            WriteLog(String.Format("\t\t\t\t檢查帳戶{0} 不是外幣", Account));



            #region 當扣押金額+VD / MD>目前餘額, 33401
            

            // 當扣押金額+VD / MD>目前餘額, 33401

            string s33401 = objSeiHTG.Send401(ObligorNo, payCase.CaseId.ToString(), Account, Ccy);
            if (s33401.StartsWith("0000|"))
            {
                string _trn = s33401.Replace("0000|", "");

                alBiz.addAutoLog(payCase.CaseId, eQueryStaff, Account, ObligorNo, "33401", 1, s33401);
                string strsql = @"select * from TX_33401 where Acct like '%{0}%' and TrnNum='{1}'  ";
                string formsql = string.Format(strsql, Account, _trn);
                DataTable dt = hostbiz.getDataTabe(formsql);
                if (dt != null)
                {
                    if (dt.Rows.Count > 0)
                    {
                       decimal VD = 0;
                       if(! string.IsNullOrEmpty(dt.Rows[0]["AtmHoldAmt"].ToString()) )
                           VD = decimal.Parse(dt.Rows[0]["AtmHoldAmt"] == null ? "0" : dt.Rows[0]["AtmHoldAmt"].ToString());

                       decimal MD=0;
                       if(! string.IsNullOrEmpty(dt.Rows[0]["MdHoldAmt"].ToString()) )
                          MD = decimal.Parse(dt.Rows[0]["MdHoldAmt"].ToString());

                       decimal holdAmt = 0;
                       if(! string.IsNullOrEmpty(dt.Rows[0]["HoldAmt"].ToString()) )
                         holdAmt = decimal.Parse(dt.Rows[0]["HoldAmt"] == null ? "0" : dt.Rows[0]["HoldAmt"].ToString());

                       decimal CurBal = 0;
                       if(! string.IsNullOrEmpty(dt.Rows[0]["CurBal"].ToString()) )
                         CurBal = decimal.Parse(dt.Rows[0]["CurBal"] == null ? "0" : dt.Rows[0]["CurBal"].ToString());//目前餘額 

                        string acctStatus1 = dt.Rows[0]["AcctStatus1"].ToString(); // 判斷是否被結清...
                        if (acctStatus1.StartsWith("結清"))
                        {
                            return string.Format("9913|帳號{0}, 扣押之帳戶中有帳戶已被結清",Account);
                        }

                        if (holdAmt + VD + MD > CurBal)
                        {
                            WriteLog(String.Format("9918|帳戶{0} 扣押金額({1})+VD({2}) + MD({3})>目前餘額({4})", Account, holdAmt.ToString(), VD.ToString(), MD.ToString(), CurBal.ToString()));
                            return string.Format("9918|帳號{0}, 扣押之帳戶中有餘額不足", Account);
                        }
                    }
                    else
                    {
                        return String.Format("9911|帳號{0} 無法取得33401資料!", Account);
                    }
                }
            }
            else
            {
                alBiz.addAutoLog(payCase.CaseId, eQueryStaff, Account, ObligorNo, "33401", 0, s33401);
                return "0013|33401發查失敗";
            }
            WriteLog(String.Format("\t\t\t\t檢查帳戶{0} 扣押金額+VD / MD < 目前餘額", Account));

            #endregion


            #region 發查450-31
            


            // 發查450-31


            string s45031 = objSeiHTG.Send45031(ObligorNo, payCase.CaseId.ToString(), Account, Ccy);
            if (s45031.StartsWith("0000|"))
            {

                #region 檢查目前450-31, 跟當初扣押的金額, 文號, 是否一致
                //alBiz.updateAutoLog(addLog, 1, s45031);
                alBiz.addAutoLog(payCase.CaseId, eQueryStaff, Account, ObligorNo,"45031",1, s45031);
                // 要檢核, 是否在今日, 在DATA1 中, 有 9093 及 金額 (##########.##)
                string strAmount2 = " " + SeizureAmount.ToString("#,0.00");
                string _trn = s45031.Replace("0000|", "");

                // 20200807, 要分是金額不對, 還是備註不對
                // 先比對金額不對的部分
                bool isSeizureAmount = false;
                string sql9093 = "SELECT * FROM TX_00450 WHERE TRNNUM='{0}' AND WXOPTION='31' AND DATA1 LIKE '% 9093 %' AND DATA1 LIKE '% {1}%'  ";
                string formsql = string.Format(sql9093, _trn, strAmount2, govNo);
                DataTable gCase9093 = hostbiz.getDataTabe(formsql);

                string sql9095 = "SELECT * FROM TX_00450 WHERE TRNNUM='{0}' AND WXOPTION='31' AND DATA1 LIKE '% 9095 %' AND DATA1 LIKE '% {1}%'  ";
                formsql = string.Format(sql9095, _trn, strAmount2, govNo);
                DataTable gCase9095 = hostbiz.getDataTabe(formsql);

                
                if (gCase9095 != null)
                {
                    if (gCase9095.Rows.Count > 0) //若有沖銷的話
                    {
                        int c9093 = gCase9093.Rows.Count;
                        int c9095 = gCase9095.Rows.Count;
                        if ((c9093 - c9095) % 2 == 0) // 表示沒有任何的扣押
                        {
                            isSeizureAmount = false;
                        }
                        else
                            isSeizureAmount = true;
                    }
                }
                else if(gCase9093!=null &&  gCase9093.Rows.Count>0)
                    isSeizureAmount = true;

                // 先比對備註不對的部分
                bool isSeizureMemo = false;
                sql9093 = "SELECT * FROM TX_00450 WHERE TRNNUM='{0}' AND WXOPTION='31' AND DATA1 LIKE '% 9093 %'  AND DATA1 LIKE '% {1}%'  AND (DATA2 LIKE '% {2}%' OR DATA2 LIKE '% {3}%') ";
                formsql = string.Format(sql9093, _trn, strAmount2, govNo, govNoNew);
                gCase9093 = hostbiz.getDataTabe(formsql);

                sql9095 = "SELECT * FROM TX_00450 WHERE TRNNUM='{0}' AND WXOPTION='31' AND DATA1 LIKE '% 9095 %'   AND DATA1 LIKE '% {1}%' AND (DATA2 LIKE '% {2}%' OR DATA2 LIKE '% {3}%') ";
                formsql = string.Format(sql9095, _trn, strAmount2, govNo, govNoNew);
                gCase9095 = hostbiz.getDataTabe(formsql);


                if (gCase9095 != null)
                {
                    if (gCase9095.Rows.Count > 0) //若有沖銷的話
                    {
                        int c9093 = gCase9093.Rows.Count;
                        int c9095 = gCase9095.Rows.Count;
                        if ((c9093 - c9095) % 2 == 0) // 表示沒有任何的扣押
                        {
                            isSeizureMemo = false;
                        }
                        else
                            isSeizureMemo = true;
                    }
                }
                else if (gCase9093 != null && gCase9093.Rows.Count > 0)
                    isSeizureMemo = true;


                // 再比對凍結碼
                bool isSeizureCode = false;

                sql9093 = "SELECT * FROM TX_00450 WHERE TRNNUM='{0}' AND WXOPTION='31' AND DATA1 LIKE '% 9093 %'  AND DATA1 LIKE '% {1}%'  AND  (DATA2 LIKE '% {2}%' OR DATA2 LIKE '% {3}%') AND DATA2 LIKE '%66' ";
                formsql = string.Format(sql9093, _trn, strAmount2, govNo, govNoNew);
                gCase9093 = hostbiz.getDataTabe(formsql);

                if (gCase9093 != null)
                    isSeizureCode = true;

                if( ! isSeizureAmount )
                    return string.Format("9913|帳號{0}, 扣押金額與ETABS系統不一致", Account);

                if (!isSeizureMemo)
                    return string.Format("9913|帳號{0}, 扣押備註與ETABS系統不一致", Account);

                if (!isSeizureCode)
                    return string.Format("9913|帳號{0}, 扣押凍結碼與ETABS系統不一致", Account);



                //if (!isSeizure)
                //    return string.Format("9913|帳號{0}, 查欲支付之前案扣押備註與ETBAS系統備註不同", Account);
                #endregion
            }
            else
            {
                alBiz.addAutoLog(payCase.CaseId, eQueryStaff, Account, ObligorNo, "45031", 0, s45031);                
                return "9912|450-31發查失敗";
            }
            WriteLog(String.Format("\t\t\t\t檢查帳戶{0} 與目前450-31, 跟當初扣押的金額, 文號 皆 一致", Account));



            #endregion



            #region 發查450-30, 檢查以下的事故代碼    事故代碼03~12、14、16、18、19、15
            


            // 發查450-30, 檢查以下的事故代碼    事故代碼03~12、14、16、18、19、15

            string codes = "03,04,05,06,07,08,09,10,11,12,14,15,16,18,19";
            List<string> a9091 = new List<string>();
            List<string> a9092 = new List<string>();


            string s45030 = objSeiHTG.Send45030(ObligorNo, payCase.CaseId.ToString(), Account, Ccy);
            if (s45030.StartsWith("0000|"))
            {
                string _trn = s45030.Replace("0000|", "");                
                alBiz.addAutoLog(payCase.CaseId, eQueryStaff, Account, ObligorNo, "45030", 1, s45030);
                // 先取9092的....
                string strsql2 = @"select * from TX_00450 where WXOPTION='30' AND Account like '%{0}%' and TrnNum='{1}' AND DATA1 LIKE '% 9092 %'  AND DATA2 not like 'END%' ";
                string formsq2 = string.Format(strsql2, Account, _trn);
                DataTable dtAcc2 = hostbiz.getDataTabe(formsq2);
                if (dtAcc2 != null)
                {
                    foreach (DataRow dr in dtAcc2.Rows)
                    {
                        var line = dr["DATA2"].ToString();
                        if (line.Length > 42)
                        {
                            var accNo = line.Substring(41, 2); //應該要抓到事故...  //   位置在41-42
                            if (codes.IndexOf(accNo) >= 0)
                                a9092.Add(accNo);
                        }
                    }
                }


                string strsql = @"select * from TX_00450 where WXOPTION='30' AND Account like '%{0}%' and TrnNum='{1}' AND DATA1 LIKE '% 9091 %'  AND DATA2 not like 'END%' ";
                string formsql = string.Format(strsql, Account, _trn);
                DataTable dtAcc1 = hostbiz.getDataTabe(formsql);
                if (dtAcc1 != null)
                {
                    foreach (DataRow dr in dtAcc1.Rows)
                    {
                        var line = dr["DATA2"].ToString();
                        if (line.Length > 42)
                        {
                            var accNo = line.Substring(41, 2); //應該要抓到事故...  //  位置在41-42
                            if (codes.IndexOf(accNo) >= 0 && a9092.Contains(accNo))
                            {
                                a9092.Remove(accNo);
                            }
                            else
                            {
                                if( codes.IndexOf(accNo)>=0)
                                    a9091.Add(accNo);
                            }
                        }
                    }
                }

                if( a9091.Count()>0)
                {
                    WriteLog(String.Format("9919|帳戶{0} 有事故 {1}", Account, string.Join("\r\n", a9091)));
                    return String.Format("9919|帳號{0}, 扣押之帳戶有特殊事故原因，請確認是否可支付", Account);
                }

            }
            else
            {
                alBiz.addAutoLog(payCase.CaseId, eQueryStaff, Account, ObligorNo, "45030", 0, s45030);
                return "0012|450-30發查失敗";
            }
            WriteLog(String.Format("\t\t\t\t檢查帳戶{0} 與目前450-30, 皆不是列舉的事故代碼(03~12、14、16、18、19、15)", Account));
            #endregion




            return "0000|符合扣押(自動支付的條件)";
        }




        /// <summary>
        /// 依TXT收取分配媒體檔資訊開立支票，不取票號，落收發待辦理
        /// </summary>
        /// <param name="casedoc"></param>
        private static string WriteCheckWithOutNo(List<CaseSeizure> SeizureCase, Guid CaseId, Guid payCaseId, string[] ldapInfo, string payMemo, int CaseMasterHandleFee)
        {
            WriteLog("\t\t開始開立支票, 但不取票號 .....");

            var cpsBiz = new CasePayeeSettingBIZ();
            var master = new CaseMasterBIZ().EDocTXT3ByCaseId(payCaseId);
            var payCaseMaster = new CaseMasterBIZ().MasterModelNew(payCaseId);
            // 跟據要開票的對象.. 去湊出那個帳號, 要解扣多少錢....
            //var edoctxt3 = new CaseMasterBIZ().EDocTXT3ByCaseId(payCaseId);
            // 到edoctxt_detail找出開票對象, 存到payee中
            var Payee = new CaseMasterBIZ().EDocTXT3_DetailByCaseId(payCaseId); // 讀取分配媒體檔, 有822開頭的...
            IDbTransaction trans = null;

            bool firstPayee = true;
            // Bank, BankID, 要從CaseSeizure 被指定支付的帳戶裏面, 抓出分行別與分行ID, 並用"；"區隔....
            string _bank = string.Join("；", SeizureCase.Select(x => x.BranchName));
            string _bankid = string.Join("；", SeizureCase.Select(x => x.BranchNo));
            foreach (var dist in Payee)
            {
                var model = new CasePayeeSetting()
                {
                    Address = dist.CheckAddress,
                    Bank = _bank,
                    BankID = _bankid,
                    CaseId = payCaseId,
                    CaseKind = "支付",
                    CheckNo = "",
                    CaseNo = payCaseMaster.CaseNo,
                    CheckIntervalId = "",
                    CCReceiver = new GovAddressBIZ().GetEnabledGovAddrByGovName(master.GovUnit), // eDocTXT3.GovUnit 的地址, 要去GovAddress查...
                    Currency = master.GovUnit, // eDocTXT3.GovUnit
                    ReceivePerson = dist.ReceiveName, // eDocTXT3_Detail.ReceiveUnit
                    Receiver = dist.ReceiveUnit, // eDocTXT3_Detail.ReceiveUnit
                    PayeeAction = 2, // 或1,2,3,4 // 取號存檔
                    Money = dist.ReceiveAmount_Case,
                    PayDate = BreakDay,
                    Fee = firstPayee ? CaseMasterHandleFee.ToString() : "0",
                    MoneySum = cpsBiz.PayAmountSum(master.CaseId), //select ISNULL(Sum(Money + Fee),0) from CasePayeeSetting where CaseId=@CaseId AND [PayeeId] <> @PayeeId
                    PayAmountSum = cpsBiz.PayAmountSum(master.CaseId) // select ISNULL(Sum(PayAmount),0) from CaseSeizure where PayCaseId=@CaseId
                };

                firstPayee = false;
                WriteLog(string.Format("\t\t\t票號: {0}, 對象: {1}, $: {2} ", model.CheckNo, model.ReceivePerson, model.Money));


                cpsBiz.InsertCasePayeeSetting2(model, trans);
            }
            // 更新CaseMaster.AddCharge 欄位...
            new CaseMasterBIZ().UpdateCaseMasterAddCharge(payCaseId, CaseMasterHandleFee);

            return "9902|支付金額 > 扣押金額 ==> 開立支票，不取票號，落收發待辦理 ";
        }

        /// <summary>
        /// 錯誤代碼是991X的.. 要分配(要開支票, 不取號)
        /// </summary>
        /// <param name="SeizureCase"></param>
        /// <param name="CaseId"></param>
        /// <param name="payCaseId"></param>
        /// <param name="ldapInfo"></param>
        /// <param name="payMemo"></param>
        /// <param name="CaseMasterHandleFee"></param>
        /// <returns></returns>
        private static void WriteCheckWithOutNo_991X(List<CaseSeizure> SeizureCase, Guid CaseId, Guid payCaseId, string[] ldapInfo, string payMemo, int CaseMasterHandleFee)
        {
            WriteLog("\t\t開始開立支票, 但不取票號 .....");

            var cpsBiz = new CasePayeeSettingBIZ();
            var master = new CaseMasterBIZ().EDocTXT3ByCaseId(payCaseId);

            var payCaseMaster = new CaseMasterBIZ().MasterModelNew(payCaseId);

            // 跟據要開票的對象.. 去湊出那個帳號, 要解扣多少錢....
            //var edoctxt3 = new CaseMasterBIZ().EDocTXT3ByCaseId(payCaseId);
            // 到edoctxt_detail找出開票對象, 存到payee中
            var Payee = new CaseMasterBIZ().EDocTXT3_DetailByCaseId(payCaseId); // 讀取分配媒體檔, 有822開頭的...
            IDbTransaction trans = null;

            bool firstPayee = true;
            // Bank, BankID, 要從CaseSeizure 被指定支付的帳戶裏面, 抓出分行別與分行ID, 並用"；"區隔....
            string _bank = string.Join("；", SeizureCase.Select(x => x.BranchName));
            string _bankid = string.Join("；", SeizureCase.Select(x => x.BranchNo));
            foreach (var dist in Payee)
            {
                var model = new CasePayeeSetting()
                {
                    Address = dist.CheckAddress,
                    Bank = _bank,
                    BankID = _bankid,
                    CaseId = payCaseId,
                    CaseKind = "支付",
                    CheckNo = "",  
                    CaseNo = payCaseMaster.CaseNo,
                    CheckIntervalId = "",
                    CCReceiver = new GovAddressBIZ().GetEnabledGovAddrByGovName(master.GovUnit), // eDocTXT3.GovUnit 的地址, 要去GovAddress查...
                    Currency = master.GovUnit, // eDocTXT3.GovUnit
                    ReceivePerson = dist.ReceiveName, // eDocTXT3_Detail.ReceiveUnit
                    Receiver = dist.ReceiveUnit, // eDocTXT3_Detail.ReceiveUnit
                    PayeeAction = 2, // 或1,2,3,4 // 取號存檔
                    Money = dist.ReceiveAmount_Case,
                    PayDate = BreakDay,
                    Fee = firstPayee ? CaseMasterHandleFee.ToString() : "0",
                    MoneySum = cpsBiz.PayAmountSum(master.CaseId), //select ISNULL(Sum(Money + Fee),0) from CasePayeeSetting where CaseId=@CaseId AND [PayeeId] <> @PayeeId
                    PayAmountSum = cpsBiz.PayAmountSum(master.CaseId) // select ISNULL(Sum(PayAmount),0) from CaseSeizure where PayCaseId=@CaseId
                };

                firstPayee = false;
                WriteLog(string.Format("\t\t\t票號: {0}, 對象: {1}, $: {2} ", model.CheckNo, model.ReceivePerson, model.Money));


                cpsBiz.InsertCasePayeeSetting2(model, trans);
            }
            // 更新CaseMaster.AddCharge 欄位...
            new CaseMasterBIZ().UpdateCaseMasterAddCharge(payCaseId, CaseMasterHandleFee);

            //return "9902|支付金額 > 扣押金額 ==> 開立支票，不取票號，落收發待辦理 ";
        }


        /// <summary>
        /// // 比對GovUnit, GovDate, SeizureAmount 是否相同
        /// </summary>
        /// <param name="casedoc"></param>
        /// <param name="seizureCase"></param>
        /// <returns></returns>
        private static string CompareFields(CaseMaster payCase, CaseMaster seizureCase, string SeiGovNo, ref int CaseMasterHandleFee, ref int SurplusMoney)
        {
            // 依3.1.2.3「本案件」區帳務資訊
            //檢視來文TXT檔與外來文系統扣押資訊
            //扣押金額合計：外來文系統前案所有帳戶加總金額
            //支付金額：來文「收取媒體檔(TXT)」之編號13「收取金額」+新外來文系統查到前案扣押到錢之ID數(重號不另計) * 參數設定手續費

            // 找出CaseSeizure中, 當初, 被扣押的ID有多少個
            var cSeizures = new CaseAccountBiz().GetCaseSeizure(seizureCase.CaseId);

            // 移除重號的ID...
            int Ids = getIDCount(cSeizures.Where(x=>x.SeizureAmount>0).ToList());

            // 找出自動支付手續費
            var parmCode = new PARMCodeBIZ().GetParmCodeByCodeType("AutoPayHandleFee").FirstOrDefault();

            if (parmCode == null)
            {
                WriteLog("ERROR --->    找不到ParmCode.CodeType='AutoPayHandleFee' 參數值");
                return null;
            }

            HandleFee = int.Parse(parmCode.CodeNo);

            int payFee = payCase.ReceiveAmount + Ids * HandleFee; // 

            CaseMasterHandleFee = Ids * HandleFee;

            if (seizureCase.GovNo == SeiGovNo && seizureCase.GovDate == payCase.PreSubDate)
            {
                if (seizureCase.ReceiveAmount == payFee)
                    return "=";
                else
                {
                    if (payFee > seizureCase.ReceiveAmount)
                        return ">";
                    else
                    {
                        SurplusMoney = seizureCase.ReceiveAmount - payFee;
                        return "<";
                    }
                }
            }
            else
            {
                return null; // 表示來文字號與日期不符...
            }
        }



        private static int getHandelFee(List<CaseSeizure> cSeizures)
        {
            //20200731, 還要考慮那些ID, 是否有金額可以扣.. Case 42,43, 44
            int Ids = getIDCount(cSeizures.Where(x=>x.SeizureAmount>0).ToList());

            // 找出自動支付手續費
            var parmCode = new PARMCodeBIZ().GetParmCodeByCodeType("AutoPayHandleFee").FirstOrDefault();
            HandleFee = int.Parse(parmCode.CodeNo);           

            return Ids * HandleFee;
        }

        /// <summary>
        /// 計算外國人的數量
        /// </summary>
        /// <param name="sei"></param>
        /// <returns></returns>
        private static int getForeignIDCount(List<CaseSeizure> sei)
        {
            
            List<string> humanList = new List<string>();
            //Regex reHumanForeign = new Regex(@"^[A-Z]{2}\d{8,10}");
            // 20200828, 改變外國人規則為第2碼為... 8或9
            Regex reHumanForeign = new Regex(@"^[A-Z]{1}[A-Z|8-9]{1}\d+$");
            foreach (var c in sei.Select(x=>x.CustId))
            {
                if (reHumanForeign.IsMatch(c)) // 個人
                {
                    if (c.Length > 10)
                        humanList.Add(c.Substring(0, 10));
                    else
                        humanList.Add(c);
                }
            }
            return humanList.Distinct().Count();
        }

        /// <summary>
        /// 去除重號, 計算被扣押的人數(包括公司)
        /// </summary>
        /// <param name="sei"></param>
        /// <returns></returns>
        private static int getIDCount(List<CaseSeizure> sei)
        {
            Regex reHuman = new Regex(@"^[A-Z]{1}\d{9,10}");
            Regex reCo = new Regex(@"^\d{8}");


            List<string> humanList = new List<string>();
            List<string> compaynList = new List<string>();

            var ids = sei.GroupBy(x => x.CustId).Select(x => x.Key).OrderBy(x => x.Length).ToList();

            foreach (var c in ids)
            {
                if (reCo.IsMatch(c)) // 公司
                {
                    if (c.Length > 8)
                        compaynList.Add(c.Substring(0, 8));
                    else
                        compaynList.Add(c);
                }
                if (reHuman.IsMatch(c)) // 個人
                {
                    if (c.Length > 10)
                        humanList.Add(c.Substring(0, 10));
                    else
                        humanList.Add(c);
                }
            }

            return compaynList.Distinct().Count() + humanList.Distinct().Count();
        }

        private static void ChengHe(Guid CaseId, string userId)
        {
            try
            {
                // 先找出MangerID
                var AToHBIZ = new AgentToHandleBIZ();
                var mangerID = AToHBIZ.GetManagerID(userId);


                AToHBIZ.AutoPay_ChenHe(new List<Guid>() { CaseId }, new List<string>() { mangerID }, userId);
            }
            catch (Exception ex)
            {
                    WriteLog("***************************呈核主管發生錯誤******************************");
                    noticeITMail(ITManagerEmail, "呈核主管發生!, 案號: " + CaseId.ToString() + "錯誤訊息: " +ex.Message.ToString());
            }

        }

        private static void noticeITMail(string mailTo, string Message)
        {
            string mailFrom = ConfigurationManager.AppSettings["mailFrom"];
            string[] mailFromTo = mailTo.Split(',');

            string subject = "外來文系統(AutoPay.exe) 發生錯誤";
            string body = "錯誤訊息：" + Message;
            string host = ConfigurationManager.AppSettings["mailHost"];
            UtlMail.SendEmail(mailFrom, mailFromTo, subject, body, host);
        }

        private static void ChengHe_OLD(Guid CaseId, string userId)
        {

            CaseSendSettingBIZ cssBIZ = new CaseSendSettingBIZ();
            AgentToHandleBIZ AToHBIZ = new AgentToHandleBIZ();
            SendSettingRefBiz refBiz = new SendSettingRefBiz();
            bool result = false;
            //* 案件主表信息

            CTBC.CSFS.Models.CaseMaster master = caseMaster.MasterModel(CaseId);

            //CaseMaster master = getCaseMasterByCaseID(CaseId);

            if (master.CaseKind == "扣押案件" && master.CaseKind2 == "撤銷")
            {
                result = true;//撤銷案件不會產生發文檔，直接呈核主管
            }
            else
            {
                #region 產生發文檔
                //* 讀取該案件以前發文儲存的資料
                IList<CaseSendSettingQueryResultViewModel> listRtn = cssBIZ.GetSendSettingList(CaseId);
                //* 用以返回的viewmodel
                CaseSendSettingCreateViewModel css = new CaseSendSettingCreateViewModel
                {
                    CaseId = CaseId,
                    ReceiveKind = master.ReceiveKind,
                    ReceiveList = new List<CaseSendSettingDetails>(),
                    CcList = new List<CaseSendSettingDetails>()
                };
                //* 取得以前沒有更新發文資訊的
                IList<CasePayeeSetting> cpsList = new CasePayeeSettingBIZ().GetPayeeSettingWhichNotSendSetting(CaseId);
                //來文機關資料.取資料
                string govAddr = new GovAddressBIZ().GetEnabledGovAddrByGovName(master.GovUnit);
                #region SendDate
                if (master.CaseKind2 == Lang.csfs_seizure)
                {
                    //* 扣押 (當天)
                    css.SendDate = Convert.ToDateTime(UtlString.FormatDateTw(DateTime.Today.ToString("yyyy/MM/dd")));
                }
                //else if (master.CaseKind2 == Lang.csfs_seizureandpay && master.AfterSeizureApproved != 1)
                //{
                //    //* 扣押並支付 的扣押 (當天)
                //    css.SendDate = Convert.ToDateTime(UtlString.FormatDateTw(DateTime.Today.ToString("yyyy/MM/dd")));
                //}
                //else if (ViewBag.CaseKind2 == Lang.csfs_Pay)
                //{
                //    //* 支付類 (看Master)
                //    css.SendDate = Convert.ToDateTime(UtlString.FormatDateTw(master.PayDate));
                //}
                //else if (master.CaseKind2 == Lang.csfs_seizureandpay && master.AfterSeizureApproved == 1)
                //{
                //    //* 扣押並支付 的支付(看Master)
                //    css.SendDate = Convert.ToDateTime(UtlString.FormatDateTw(master.PayDate));
                //}
                else
                {
                    //* 其他(當天)
                    css.SendDate = Convert.ToDateTime(UtlString.FormatDateTw(DateTime.Today.ToString("yyyy/MM/dd")));
                }
                #endregion
                if (master.CaseKind == Lang.csfs_receive_case)
                {
                    //3.外來文 發文正本預設為來文機關
                    css.ReceiveList.Add(new CaseSendSettingDetails { CaseId = CaseId, SendType = 1, GovName = master.GovUnit, GovAddr = govAddr });
                    if (master.CaseKind2 == Lang.csfs_165_reading)
                    {
                        css.Template = Lang.csfs_165_reading;
                    }
                    else if (master.CaseKind2 == Lang.csfs_property_declaration1)
                    {
                        css.Template = Lang.csfs_property_declaration1;
                    }
                    else
                    {
                        css.Template = Lang.csfs_not_165_reading;
                    }
                }
                if (master.CaseKind == Lang.csfs_menu_tit_caseseizure)
                {

                    if (master.CaseKind2 == Lang.csfs_seizure)
                    {
                        //1.扣押 發文正本預設為來文機關
                        css.ReceiveList.Add(new CaseSendSettingDetails { CaseId = CaseId, SendType = 1, GovName = master.GovUnit.Replace("(執)", "") + "(執)", GovAddr = govAddr });
                        css.Template = Lang.csfs_seizure;
                    }
                    else if (master.CaseKind2 == Lang.csfs_Pay)
                    {
                        //2.支付 發文副本預設為來文機關
                        css.CcList.Add(new CaseSendSettingDetails { CaseId = CaseId, SendType = 2, GovName = master.GovUnit, GovAddr = govAddr });
                        css.Template = Lang.csfs_Pay;
                    }
                    else if (master.CaseKind2 == Lang.csfs_seizureandpay)
                    {
                        //1.扣押 發文正本預設為來文機關
                        css.ReceiveList.Add(new CaseSendSettingDetails { CaseId = CaseId, SendType = 1, GovName = master.GovUnit, GovAddr = govAddr });
                        css.Template = master.AfterSeizureApproved == 1 ? Lang.csfs_Pay : Lang.csfs_seizure;
                    }
                    else
                    {
                        //1.扣押 發文正本預設為來文機關
                        css.ReceiveList.Add(new CaseSendSettingDetails { CaseId = CaseId, SendType = 1, GovName = master.GovUnit, GovAddr = govAddr });
                    }
                }
                //* 沒有發文的受款人正本.副本
                if (cpsList != null && cpsList.Any())
                {
                    foreach (CasePayeeSetting item in cpsList)
                    {
                        css.ReceiveList.Add(new CaseSendSettingDetails() { CaseId = CaseId, SendType = 1, GovName = item.Receiver, GovAddr = item.Address });
                        css.CcList.Add(new CaseSendSettingDetails() { CaseId = CaseId, SendType = 2, GovName = item.CCReceiver, GovAddr = item.Currency });
                    }
                }
                // 取得 ParmCode 中, CodeType='SendGovName' 的CodeDesc
                PARMCodeBIZ pbiz = new PARMCodeBIZ();
                var pz = pbiz.GetParmCodeByCodeType("SendGovName").FirstOrDefault();
                string strCodeDesc = "";
                if (pz != null)
                {
                    strCodeDesc = pz.CodeDesc;
                }


                css.SendKind = master.ReceiveKind == "紙本" ? "紙本發文" : "電子發文";
                css.SendWord = master.ReceiveKind == "紙本" ? Lang.csfs_ctci_bank : strCodeDesc;
                css.SendNo = master.SendNo;
                css.Speed = master.Speed;
                css.Security = Lang.csfs_security1;
                decimal edocTotal = 0.0m;

                // 取得支付金額
                edocTotal = new ImportEDocBiz().getEDocTxt3Total(CaseId);


                //@@@! 2018,0828 測試多義務人發文..
                SendSettingRef SSref = refBiz.AutoGetSubjectAndDescription(CaseId, css.Template, css.SendKind, edocTotal);
                css.Subject = SSref.Subject;
                css.Description = SSref.Description;
                #endregion

                #region 儲存發文檔
                css.SendDate = UtlString.FormatDateTwStringToAd(css.SendDate);
                //result = cssBIZ.SaveCreate(css);
                result = SaveCreate(css);

                #endregion
            }

            #region 呈核主管
            if (result)
            {

                string mgrid = AToHBIZ.GetManagerID(eQueryStaff);
                insertCaseAssignTable(CaseId, mgrid, 0);

                //string[] caseid = CaseId.ToString().Split(','); ;
                //List<Guid> aryCaseId = (from id in caseid where !string.IsNullOrEmpty(id) select new Guid(id)).ToList();
                //string agentIdList = AToHBIZ.GetManagerID(userId);
                //caseid = agentIdList.Split(',');
                //List<string> aryAgentId = (from id in caseid where !string.IsNullOrEmpty(id) select id).ToList();
                //JsonReturn Return = AToHBIZ.AgentSubmit(aryCaseId, aryAgentId, userId);
                //if (Return.ReturnCode == "1")
                //{
                //    result = true;
                //}
                //else
                //{
                //    result = false;
                //}
            }
            #endregion
        }


        private static void insertCaseHistory(CaseMaster casedoc, string FromRole, string FromUser, string FromFolder, string Event, string ToRole, string ToUser, string ToFolder)
        {
            CaseHistory ch = new CaseHistory()
            {
                CaseId = casedoc.CaseId,
                CreatedDate = DateTime.Now,
                CreatedUser = FromUser,
                Event = Event,
                EventTime = DateTime.Now,
                FromFolder = FromFolder,
                FromRole = FromRole,
                FromUser = FromUser,
                ToFolder = ToFolder,
                ToRole = ToRole,
                ToUser = ToUser
            };
            CaseHistoryBIZ chbiz = new CaseHistoryBIZ();
            chbiz.insertCaseHistory(ch);
        }

        private static void insertCaseAssignTable(Guid guid, string eQueryStaff, int p)
        {

            CaseAssignTable cat = new CaseAssignTable()
            {
                AlreadyAssign = p,
                CaseId = guid,
                CreatdUser = eQueryStaff,
                CreatedDate = DateTime.Now,
                EmpId = eQueryStaff,
                ModifiedDate = DateTime.Now,
                ModifiedUser = eQueryStaff
            };
            CaseAssignTableBIZ biz = new CaseAssignTableBIZ();
            biz.insertCaseAssignTable(cat);
        }

        //private static void insertCaseMemo(Guid caseid, string p, List<string> DocMemo, string eQueryStaff)
        //{

        //    CaseMemoBiz biz = new CaseMemoBiz();

        //    var newDocMemo = DocMemo.Distinct();
        //    foreach (var s in newDocMemo)
        //    {
        //        CaseMemo cm = new CaseMemo();
        //        cm.CaseId = caseid;
        //        cm.MemoType = p;
        //        cm.Memo = s;

        //        cm.MemoUser = eQueryStaff;
        //        biz.Create(cm);
        //    }

        //}

        private static void updateCaseMasterStatus(Guid caseid, string status, string eStaff, string returnReason)
        {

            CaseMasterBIZ biz = new CaseMasterBIZ();

            biz.UpdateCaseMasterStatus(caseid, status, eStaff, returnReason);
            if (status == "B01")
                biz.updateCaseMasterAgentUser(caseid, "SYS");
            else
                biz.updateCaseMasterAgentUser(caseid, eStaff);
        }


        private static void updateCaseMasterAgentUser(Guid caseid, string eQueryStaff)
        {
            // 先取得要進select AgentSection,AgentDeptId,AgentBranchId from [V_AgentAndDept] where [EmpID] = @EmpId 

            LdapEmployeeBiz agentBiz = new LdapEmployeeBiz();
            LDAPEmployee agent = agentBiz.GetAllEmployeeInEmployeeViewByEmpId(eQueryStaff);


            CaseMasterBIZ biz = new CaseMasterBIZ();
            var caseObj = biz.MasterModelNew(caseid);

            if (caseObj != null)
            {
                caseObj.AgentUser = eQueryStaff;
                caseObj.AssignPerson = eQueryStaff;
                if (agent != null)
                {
                    caseObj.AgentSection = agent.SectionName;
                    caseObj.AgentDeptId = agent.DepId;
                    caseObj.AgentBranchId = agent.BranchId;
                }
                biz.updateCaseMasterAgentUser(caseid, eQueryStaff);
            }


        }

        /// <summary>
        /// 取得列表中, 下一個使用者
        /// </summary>
        /// <param name="humanProcLists"></param>
        /// <param name="lastAgent"></param>
        /// <returns></returns>
        private static AgentSetting getNextAgent(List<AgentSetting> humanProcLists, string lastAgent)
        {
            AgentSetting result = new AgentSetting();
            var olastAgent = humanProcLists.Where(x => x.EmpId == lastAgent).FirstOrDefault();
            if (olastAgent == null) // 表示今日指定人, 沒有在上次的名單中, 從第一個開始
            {
                result = humanProcLists[0];
            }
            else // 表示今日指定人, 有在名單中, 找出下一個人
            {
                int index = humanProcLists.IndexOf(olastAgent);
                if (humanProcLists.Count > index + 1)
                    result = humanProcLists[index + 1];
                else
                    result = humanProcLists[0];
            }
            return result;
        }


        private static void updateLastAssign(AgentSetting nextAgent)
        {
            PARMCodeBIZ pbiz = new PARMCodeBIZ();


            var tobeUpdate = pbiz.GetParmCodeByCodeType("AssignLast").FirstOrDefault();
            if (tobeUpdate != null)
            {
                tobeUpdate.CodeDesc = nextAgent.EmpId;
                pbiz.Update(tobeUpdate);
            }

        }


        /// <summary>
        /// 儲存CaseSetting
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static bool SaveCreate(CaseSendSettingCreateViewModel model)
        {
            //simon 2016/09/29
            if (model.SendKind != "電子發文")
                model.SendKind = "紙本發文";

            //IDbConnection dbConnection = OpenConnection();
            bool rtn = true;
            bool needSubmit = false;
            try
            {
                //if (trans == null)
                //{
                //    needSubmit = true;
                //    trans = dbConnection.BeginTransaction();
                //}
                rtn = InsertCaseSendSetting(ref model);
                if (model.ReceiveList != null && model.ReceiveList.Any())
                {
                    foreach (CaseSendSettingDetails item in model.ReceiveList.Where(item => !string.IsNullOrEmpty(item.GovAddr) && !string.IsNullOrEmpty(item.GovAddr)))
                    {
                        item.CaseId = model.CaseId;
                        item.SerialID = model.SerialId;
                        item.SendType = CaseSettingDetailType.Receive;  //*正本
                        rtn = InsertCaseSendSettingDetials(item);
                    }
                }

                if (model.CcList != null && model.CcList.Any())
                {
                    foreach (CaseSendSettingDetails item in model.CcList.Where(item => !string.IsNullOrEmpty(item.GovAddr) && !string.IsNullOrEmpty(item.GovAddr)))
                    {
                        item.CaseId = model.CaseId;
                        item.SerialID = model.SerialId;
                        item.SendType = CaseSettingDetailType.Cc; //*副本
                        rtn = InsertCaseSendSettingDetials(item);
                    }
                }



                return rtn;
            }
            catch (Exception)
            {
                return false;
            }
        }

        static public bool InsertCaseSendSetting(ref CaseSendSettingCreateViewModel model)
        {
            bool result = true;
            FrameWork.HTG.HostMsgGrpBIZ hbiz = new FrameWork.HTG.HostMsgGrpBIZ();


            for (int i = 0; i < 3; i++)
            {
                try
                {
                    CaseSendSettingBIZ cssBIZ = new CaseSendSettingBIZ();
                    string sql = "";
                    sql = @"DECLARE @SendNoId bigint;
                    DECLARE @flag as timestamp;
                    SELECT TOP 1 @SendNoId=[SendNoId],@flag=[TimesFlag] FROM [SendNoTable] WHERE [SendNoYear] = @SendNoYear AND [SendNoNow] < [SendNoEnd] ORDER BY SendNoId;
                    UPDATE [SendNoTable] SET [SendNoNow] = [SendNoNow]+1 WHERE [SendNoId] = @SendNoId and [TimesFlag]=@flag;
                    INSERT INTO CaseSendSetting(CaseId,[Template],SendDate,SendWord,SendNo,Speed,Security,Subject,Description, Attachment,CreatedUser,CreatedDate,ModifiedUser,SendKind) 
                           VALUES (@CaseId,@Template,@SendDate,@SendWord,@SendNo1,@Speed,@Security,@Subject,@Description,@Attachment,@CreatedUser,GETDATE(),@ModifiedUser,@SendKind);
                    SELECT @@identity";
                    //string sql1 = "SELECT TOP 1 [SendNoId],[TimesFlag] FROM [SendNoTable] WHERE [SendNoYear] = '{0}' AND [SendNoNow] < [SendNoEnd] ORDER BY SendNoId";
                    //sql1 = string.Format(sql1, DateTime.Now.ToString("yyyy"));
                    //DataTable dt1 = hbiz.getDataTabe(sql1);
                    //string SendNoId = dt1.Rows[0][0].ToString();
                    //byte[] flag = (byte[] ) dt1.Rows[0][1];



                    //string flag = dt1.Rows[0][1].ToString();

                    ArrayList sqlArray = new ArrayList();
                    string sql2 = string.Format("UPDATE [SendNoTable] SET [SendNoNow] = [SendNoNow]+1 WHERE [SendNoId]   in (select TOP 1 SendNoId FROM [SendNoTable] WHERE [SendNoYear] ='{0}' AND [SendNoNow] < [SendNoEnd] ORDER BY SendNoId );", DateTime.Now.ToString("yyyy"));
                    sqlArray.Add(sql2); 

                    string sdate = model.SendDate.ToString("yyyy/MM/dd");

                    if (model.flag == "AgentAccountInfo")//帳務資訊儲存時不產生發文字號，呈核時才產生
                    {
                        model.SendNo = "";
                    }
                    else//發文資訊儲存時需要產生發文字號
                    {
                        //if (model.SendKind == "電子發文")
                        {
                            //第四碼固定為2 --simon 2016/08/05
                            //model.SendNo = model.SendDate.Year + "00" + model.SendNo.Substring(9);
                            model.SendNo = cssBIZ.SendNo();
                            //20170630 緊急 RQ-2015-019666-0?? 發文字號擴碼 宏祥 update start
                            //model.SendNo = model.SendDate.Year + "20" + model.SendNo.Substring(9);
                            model.SendNo = (DateTime.Now.Year - 1911) + "2" + model.SendNo.Substring(9);
                            //20170630 緊急 RQ-2015-019666-0?? 發文字號擴碼 宏祥 update end
                        }
                        //else
                        //{
                        //    model.SendNo = cssBIZ.SendNo();
                        //}
                    }


                    string sql3 = "INSERT INTO CaseSendSetting(CaseId,[Template],SendDate,SendWord,SendNo,Speed,Security,Subject,Description, Attachment,CreatedUser,CreatedDate,ModifiedUser,SendKind) VALUES ('{0}','{1}', Convert(datetime, '{2}',111),'{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}',GETDATE(),'{11}','{12}')";
                    sql3 = string.Format(sql3, model.CaseId, model.Template, sdate, model.SendWord, model.SendNo, model.Speed, model.Security, model.Subject, model.Description, model.Attachment, eQueryStaff, eQueryStaff, model.SendKind);
                    sqlArray.Add(sql3);
                    var ddd = hbiz.SaveESBData(sqlArray);

                    string sql4 = string.Format("SELECT TOP 1 SerialID FROM CaseSendSetting where CASEID='{0}' ", model.CaseId);
                    var dd2 = hbiz.getDataTabe(sql4);
                    int seriaid = 0;
                    if (dd2.Rows.Count > 0)
                    {
                        seriaid = int.Parse(dd2.Rows[0][0].ToString());
                    }
                    model.SerialId = seriaid;


                    //string cs = ConfigurationManager.AppSettings["CSFS_ADO"];
                    //using( SqlConnection cn = new SqlConnection(cs))
                    //{
                    //    cn.Open();
                    //    using( SqlCommand cmd = new SqlCommand(sql,cn))
                    //    {
                    //        cmd.Parameters.Clear();
                    //        //cmd.Parameters.Add(new SqlParameter());

                    //        if (model.SendKind == "電子發文")
                    //        {
                    //            //第四碼固定為2 --simon 2016/08/05
                    //            //model.SendNo = model.SendDate.Year + "00" + model.SendNo.Substring(9);
                    //            model.SendNo = cssBIZ.SendNo();
                    //            //20170630 緊急 RQ-2015-019666-0?? 發文字號擴碼 宏祥 update start
                    //            //model.SendNo = model.SendDate.Year + "20" + model.SendNo.Substring(9);
                    //            model.SendNo = (DateTime.Now.Year - 1911) + "2" + model.SendNo.Substring(9);
                    //            //20170630 緊急 RQ-2015-019666-0?? 發文字號擴碼 宏祥 update end
                    //        }
                    //        else
                    //        {
                    //            model.SendNo = cssBIZ.SendNo();
                    //        }

                    //        cmd.Parameters.Add("@CaseId", SqlDbType.UniqueIdentifier);
                    //        cmd.Parameters["@CaseId"].Value = model.CaseId;

                    //        cmd.Parameters.Add("@Template", SqlDbType.NVarChar);
                    //        cmd.Parameters["@Template"].Value = model.Template;

                    //        cmd.Parameters.Add("@SendDate", SqlDbType.DateTime);
                    //        cmd.Parameters["@SendDate"].Value = model.SendDate;

                    //        cmd.Parameters.Add("@SendWord", SqlDbType.NVarChar);
                    //        cmd.Parameters["@SendWord"].Value = model.SendWord;

                    //        cmd.Parameters.Add("@SendNo1", SqlDbType.NVarChar);
                    //        cmd.Parameters["@SendNo1"].Value = model.SendNo;

                    //        cmd.Parameters.Add("@Speed", SqlDbType.NVarChar);
                    //        cmd.Parameters["@Speed"].Value = model.Speed;

                    //        cmd.Parameters.Add("@Security", SqlDbType.NVarChar);
                    //        cmd.Parameters["@Security"].Value = model.Security;

                    //        cmd.Parameters.Add("@Subject", SqlDbType.NVarChar);
                    //        cmd.Parameters["@Subject"].Value = model.Subject;

                    //        cmd.Parameters.Add("@Description", SqlDbType.NVarChar);
                    //        cmd.Parameters["@Description"].Value = model.Description;

                    //        cmd.Parameters.Add("@Attachment", SqlDbType.NVarChar);
                    //        cmd.Parameters["@Attachment"].Value = model.Attachment;

                    //        cmd.Parameters.Add("@CreatedUser", SqlDbType.NVarChar);
                    //        cmd.Parameters["@CreatedUser"].Value = eQueryStaff;

                    //        cmd.Parameters.Add("@ModifiedUser", SqlDbType.NVarChar);
                    //        cmd.Parameters["@ModifiedUser"].Value = eQueryStaff;

                    //        cmd.Parameters.Add("@SendNoYear", SqlDbType.NVarChar);
                    //        cmd.Parameters["@SendNoYear"].Value = DateTime.Now.ToString("yyyy");

                    //        cmd.Parameters.Add("@GovName", SqlDbType.NVarChar);
                    //        cmd.Parameters["@GovName"].Value = model.GovName;

                    //        cmd.Parameters.Add("@GovAddr", SqlDbType.NVarChar);
                    //        cmd.Parameters["@GovAddr"].Value = model.GovAddr;

                    //        cmd.Parameters.Add("@GovNameCc", SqlDbType.NVarChar);
                    //        cmd.Parameters["@GovNameCc"].Value = model.GovNameCc;

                    //        cmd.Parameters.Add("@GovAddrCc", SqlDbType.NVarChar);
                    //        cmd.Parameters["@GovAddrCc"].Value = model.GovAddrCc;

                    //        cmd.Parameters.Add("@SendKind", SqlDbType.NVarChar);
                    //        cmd.Parameters["@SendKind"].Value = model.SendKind;

                    //        cmd.ExecuteScalar();
                    //    }
                    //    cn.Close();
                    //}
                    //model.SerialId = trans == null ? Convert.ToInt32(ExecuteScalar(sql)) : Convert.ToInt32(ExecuteScalar(sql, trans));
                    result = true;
                    break;
                }
                catch (Exception ex)
                {
                    i++;
                    result = false;
                }
            }
            return result;
        }
        static public bool InsertCaseSendSettingDetials(CaseSendSettingDetails model)
        {
            string strSql = @"insert into CaseSendSettingDetails([CaseId],[SerialID],[SendType],[GovName],[GovAddr])
                                    values(@CaseId, @SerialID, @SendType, @GovName,@GovAddr)";


            using (SqlConnection cn = new SqlConnection(cs))
            {
                cn.Open();
                using (SqlCommand cmd = new SqlCommand(strSql, cn))
                {
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add(new SqlParameter("@CaseId", model.CaseId));
                    cmd.Parameters.Add(new SqlParameter("@SerialID", model.SerialID));
                    cmd.Parameters.Add(new SqlParameter("@SendType", model.SendType));
                    cmd.Parameters.Add(new SqlParameter("@GovName", model.GovName));
                    cmd.Parameters.Add(new SqlParameter("@GovAddr", model.GovAddr));
                    cmd.ExecuteNonQuery();
                }
                cn.Close();
            }

            return true;

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
