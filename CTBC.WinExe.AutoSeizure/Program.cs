using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CTBC.FrameWork.HTG;
using CTBC.FrameWork.Util;
using CTBC.CSFS.BussinessLogic;
using CTBC.CSFS.Models;
using CTBC.CSFS.Pattern;
using CTBC.CSFS.ViewModels;
using CTBC.CSFS.Resource;
using System.Collections;
using System.Configuration;
using log4net;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.IO;

namespace CTBC.WinExe.AutoSeizure
{
    /// <summary>
    /// 自動扣押
    /// Written By Patrick Yang , patrickyang2@wistronits.com
    /// 2018/4/24 
    /// </summary>
    class Program
    {

        public static string eQueryStaff = null;
        public static string _branchNo = null;
        public static string cs = null;
        public static string LastAgentSetting = null;
        public static bool isDebugMode = false;
        public static string ITManagerEmail = "chengda.chen@ctbcbank.com"; 
        //public static int delaySeconds = 1800; // 預設測試卡件, 1800 秒...

        CustomerInfoBIZ custBiz = new CustomerInfoBIZ();
        CTBC.FrameWork.HTG.HostMsgGrpBIZ hostbiz = new CTBC.FrameWork.HTG.HostMsgGrpBIZ();
        public static CaseMasterBIZ caseMaster = new CaseMasterBIZ();
        public static CaseAccountBiz caseAccount = new CaseAccountBiz();
        public static bool sendHTG = true;  // 本變數來決定, 是否要發電文, 還是只讀DB
        public static ExecuteHTG objSeiHTG;
        public static AutoSeizureBiz asBiz;

        public static Dictionary<string, decimal> gdicCurrency;
        ILog log = LogManager.GetLogger("DebugLog");
        static void Main(string[] args)
        {
            log4net.Config.XmlConfigurator.Configure(new System.IO.FileInfo(AppDomain.CurrentDomain.BaseDirectory + @"\Log.config"));



            // 刪除Example.log.2018*.*
            string sDir = AppDomain.CurrentDomain.BaseDirectory;
            DirectoryInfo di = new DirectoryInfo(sDir);
            foreach (var s in di.GetFiles("example.log.*"))
            {
                File.Delete(s.FullName);
                System.Threading.Thread.Sleep(100);
            };

            
            gdicCurrency = new Dictionary<string, decimal>();

            //WriteLog("====================開始執行自動扣押====================");
            cs = System.Configuration.ConfigurationManager.ConnectionStrings["CSFS_ADO"].ToString();

            if (System.Configuration.ConfigurationManager.AppSettings["ITManager"] != null)
            {
                ITManagerEmail = System.Configuration.ConfigurationManager.AppSettings["ITManager"].ToString();
            }
            //if (System.Configuration.ConfigurationManager.AppSettings["delaySeconds"] != null)
            //{
            //    delaySeconds = int.Parse(System.Configuration.ConfigurationManager.AppSettings["delaySeconds"].ToString());
            //}

            PARMCodeBIZ pbiz = new PARMCodeBIZ();
            asBiz = new AutoSeizureBiz();
            // 先檢查 select * from [CSFS_SIT].[dbo].[PARMCode] where codetype='eTabsQueryStaff'中的人員的LDAP RACF有效性

            var eTabsStaffs = pbiz.GetParmCodeByCodeType("eTabsQueryStaff").Where(x => (bool)x.Enable).OrderBy(x => x.SortOrder).ToList();
            var eBranchNo = pbiz.GetParmCodeByCodeType("eTabsQueryStaffBranchNo").FirstOrDefault();

            if( eTabsStaffs.Count()==0)
            {
                WriteLog("**********************************************************************");
                WriteLog("****************************目前沒有指定發查人**************************");
                WriteLog("**********************************************************************");
                noticeITMail(ITManagerEmail, "參數檔中, 沒有啟動任何一個電文發查人!");
                return;
            }

            // 讀取上一次, 指派到那一位承辦人
            var _lastAgetSetting = pbiz.GetParmCodeByCodeType("AssignLast").FirstOrDefault();
            if (_lastAgetSetting == null)
                LastAgentSetting = eQueryStaff;
            else
            {
                LastAgentSetting = _lastAgetSetting.CodeDesc;
            }


            objSeiHTG = new ExecuteHTG();

            if (eBranchNo != null)
                _branchNo = eBranchNo.CodeDesc;
            //WriteLog("取得發查人員的LDAP,RACF....");
            #region 前處理, 若LDAP, RACF 無法登入, 則停下來, 發mail通知
            int loginStatus = -1;
            foreach (var e in eTabsStaffs)
            {

                // 先認證是否OK, 發查HTGInitialize();
                // 解碼 
                var base64EncodedBytes = System.Convert.FromBase64String(e.CodeMemo);
                string[] up = System.Text.Encoding.UTF8.GetString(base64EncodedBytes).Split(',');
                if (up.Length == 4) // 一定要順序為
                {
                    eQueryStaff = up[0].ToString();
                    // 分行別, 要去 select bracnchid from [LDAPEmployee] where empid=up[0]
                    bool isInit = objSeiHTG.HTGInitialize(up[0], up[1], up[2], up[3], _branchNo);
                    if (!isInit)
                    {
                        string InitErrorMessage = objSeiHTG.initErrorMessage;
                        var eTabsStaffsMail1 = pbiz.GetParmCodeByCodeType("eTabsQueryStaffNoticeMail").FirstOrDefault();
                        //WriteLog("無法取得SessionID認證!!, 程式結束!!");
                        if (eTabsStaffsMail1 != null)
                        {
                            string[] mailTo = eTabsStaffsMail1.CodeMemo.Split(',');
                            noticeMail(mailTo, InitErrorMessage);
                            WriteLog("**********************************************************************");
                            WriteLog(string.Format("****************************目前發查人{0} 帳密錯誤**************************", eQueryStaff));
                            WriteLog("**********************************************************************");
                        }
                        // 若失敗, 則將此人的啟用狀態改成False, 以免下次進來後, 繼續用第一個人...
                        asBiz.disableQueryStaff(e.SortOrder);


                    }
                    else // 此人通過
                    {
                        loginStatus = 1;
                        break; // foreach 
                    }
                }
            }

            if (loginStatus < 0) // 表示所有人都認證未過.. 
            {
                return;
            }

            #endregion




            try
            {
                if (args.Length == 0) // 沒有參數，代表不按照來文號的尾碼進行分流發查, 照發文號來查
                {
                    isDebugMode = false;
                    doSeizure(null);
                }
                else
                {
                    isDebugMode = true;
                    doSeizure(args[0].ToString().Trim());
                }
            }
            catch (Exception ex)
            {
                WriteLog(string.Format("\r\n\r\n\r\n\r\n未知的錯誤 {0}", ex.Message.ToString()));
                noticeITMail(ITManagerEmail, "未知的錯誤!" + ex.Message.ToString());
                
            }
        }



        private static void doSeizure(string _CaseNo)
        {


            int RunningCaseNum = asBiz.getCaseMasterStillRuning();

            if (RunningCaseNum > 0)
            {
                // ===============================================================================================
                // 20221128, 正達表示, 過去幾個月內, 並不會主動落人工..
                // 20221128, 改為. .在本批自動扣押完成後, 回頭看一下, 是否本批還有999, 若有.. 則落人工
                // ===============================================================================================


                // 20220328, 若在執行中, 可能是相同案件, 卡件, 所以要檢查, 是不是
                // 找出目前正在執行的第一筆...
                //var allCaseDoc = asBiz.getCaseMaster("999");
                //var Firstcasedoc = allCaseDoc.First();


                ////20220706, 讀取第一筆上次案件的執行時間....
                ////PARMCode lastProcCase = asBiz.getParmCodeByCodeType("ProcAutoSeizure");
                
                //// 若案件號碼相同且, 時間超過設定時間, 則落人工....

                //if (lastProcCase.CodeNo == Firstcasedoc.CaseNo)
                //{
                //    DateTime allowTime = ((DateTime)lastProcCase.ModifiedDate).AddSeconds(delaySeconds);
                //    DateTime thenow = DateTime.Now;
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
                //        //setCaseMasterC01B01(allCaseDoc.Select(x => x.CaseId).ToList(), Firstcasedoc.CaseId);
                //        asBiz.setCaseMasterC01B01(Firstcasedoc.CaseId);
                //        List<string> memos = new List<string>() { "電文發查延滯" };
                //        asBiz.insertCaseMemo(Firstcasedoc.CaseId, "CaseSeizure", memos, "SYS");

                //    }
                //}
                //else
                {
                    //20220728, 
                    WriteLog(string.Format("\r\n\r\n\r\n\r\n*********************************前案執行中, 尚有 {0} 筆資料執行中*********************************\r\n\r\n\r\n\r\n", RunningCaseNum.ToString()));
                    return;
                }

            }

            List<CaseMaster> CaseMasterLists = asBiz.getCaseMaster("B01");
            WriteLog(string.Format("\r\n\r\n\r\n\r\n取得共{0}筆案件", CaseMasterLists.Count().ToString()));

            if (CaseMasterLists.Count() == 0)
            {
                WriteLog(string.Format("\r\n\r\n\r\n\r\n目前無案件可發查"));
                return;
            }

            // 人工處理的人
            //List<AgentSetting> humanProcLists = new List<AgentSetting>();
            //using (CSFSEntities ctx = new CSFSEntities())
            //{
            //    humanProcLists = ctx.AgentSetting.Where(x => (bool)x.IsSeizure).OrderBy(x => x.SettingId).ToList();
            //}
            List<AgentSetting> humanProcLists = asBiz.getAgentSetting();
            
            LDAPEmployee agent = (new LdapEmployeeBiz()).GetAllEmployeeInEmployeeViewByEmpId(eQueryStaff);



            // 確定是個人戶的案件後,  馬上把取得案件編號, 改成999
            //setCaseMasterRunning(CaseMasterLists.Select(x => x.CaseId).ToList());
            asBiz.setCaseMasterRunning(CaseMasterLists.Select(x => x.CaseId).ToList(), "999");

            foreach (var casedoc in CaseMasterLists)
            {



                try
                {
                    WriteLog(string.Format("開始扣押CaseID={0} DocNo={1}", casedoc.CaseId.ToString(), casedoc.CaseNo.ToString()));

                    //20220328, 記錄目前處理的案件在ParmCode.CodeType='ProcAutoSeizure' , CodeNo='案號',  ModifiedDate='執行時間'                    
                    string message = asBiz.setExecuteCaseNo("ProcAutoSeizure", casedoc.CaseNo);
                    if (!string.IsNullOrEmpty(message))
                    {
                        WriteLog("設定目前處理案件錯誤: " + message);
                    }


                    #region 將CaseMaster的AssignPerson, 及AgentUser改為目前發查人

                    // 20181221, 改成"SYS"                    
                    asBiz.setAgentUserSYS(casedoc.CaseId);
                    WriteLog("指派發查人" + eQueryStaff);
                    #endregion

                    #region 新增一筆CaseHistory
                    asBiz.insertCaseHistory(casedoc, "自動扣押", eQueryStaff, "收發-待分文", "收發分派", "經辦人員", eQueryStaff, "經辦-待辦理");
                    asBiz.insertCaseAssignTable(casedoc.CaseId, eQueryStaff, 0);
                    //insertCaseHistory(casedoc, "自動扣押", eQueryStaff, "收發-待分文", "收發分派", "經辦人員", eQueryStaff, "經辦-待辦理");
                    //insertCaseAssignTable(casedoc.CaseId, eQueryStaff, 0);
                    WriteLog("新增一筆CaseHistory");
                    #endregion
                }
                catch (Exception ex)
                {
                    WriteLog(string.Format("\r\n\r\n\r\n\r\n未知的錯誤 {0}", ex.Message.ToString()));
                    List<string> docmemo = new List<string>() { "修改發查人錯誤，落人工" };
                    //asBiz.insertCaseMemo(casedoc.CaseId, "CaseSeizure", docmemo, eQueryStaff);
                    asBiz.insertCaseMemo(casedoc.CaseId, "CaseSeizure", docmemo, eQueryStaff);
                    noticeITMail(ITManagerEmail, "修改發查人錯誤，落人工!, 案件編號: " + casedoc.CaseNo + "錯誤訊息: " + ex.Message.ToString());
                    continue;
                }

                try
                {

                    // 判斷是純10碼, 或不是, 若純10碼, 走個人. 
                    
                    bool isPerson = asBiz.getIsPerson(casedoc);
                    string result = "";
                    int CaseType = 1;
                    if (isPerson) // 純10碼
                    {
                        result = doSeizureByCaseId(casedoc);
                        CaseType = 1;
                    }
                    else
                    {
                        result = doSeizureByCaseId_Co(casedoc, ref CaseType);
                    }
                    //var result = doSeizureByCaseId(casedoc);
                    if (
                        result.StartsWith("0000") || result.StartsWith("0001") || result.StartsWith("0002") || result.StartsWith("0006")
                        || result.StartsWith("0009") || result.StartsWith("0007") || result.StartsWith("0008") || result.StartsWith("0013")
                        || result.StartsWith("0023") || result.StartsWith("0024") || result.StartsWith("0038") || result.StartsWith("0027") 
                        )
                    {
                        WriteLog("扣押完成!!");                        
                        asBiz.updateCaseMasterStatus(casedoc.CaseId, "D01");                        
                        asBiz.updateCaseMasterAgentUser(casedoc.CaseId, eQueryStaff);                        
                        asBiz.insertCaseHistory(casedoc, "自動扣押", eQueryStaff, "經辦-待辦理", "經辦呈核", "自動化處理", eQueryStaff, "主管-待核決");
                        // 以下二個動作, 都在ChengHe()中做完了...
                        //string mgrid = GetManagerID(eQueryStaff);
                        //insertCaseAssignTable(casedoc.CaseId, mgrid, 0);
                        // 進行呈核動作.. 參考AgentAccountInfoController 的第353行 子流程
                        ChengHe(casedoc.CaseId, eQueryStaff);
                    }
                    else
                    {

                        WriteLog("扣押失敗, 原因: !!" + result);

                        if (! ( CaseType == 1) )
                        {
                            CaseMemoBiz cmb = new CaseMemoBiz();
                            var a = new CaseMemoBiz().Update2(new CTBC.CSFS.Models.CaseMemo() { CaseId = casedoc.CaseId, MemoType = CaseMemoType.CaseSeizureMemo }, null);
                        }
                        #region 20181101, 若有落人工, 且為行號, 則告知負責人ID　及姓名 (A107103000058), 寫在CaseMemo中
                        if( CaseType==5)
                        {
                            string sql = "SELECT TOP 1 * FROM TX_67102 WHERE caseid='{0}'  ORDER BY SNO DESC";
                            string formsql = string.Format(sql, casedoc.CaseId.ToString());

                            CTBC.FrameWork.HTG.HostMsgGrpBIZ hostbiz = new CTBC.FrameWork.HTG.HostMsgGrpBIZ();
                            DataTable gCase = hostbiz.getDataTabe(formsql);
                            //var nums = ctx.TX_60629.Where(x => x.TrnNum == trnnum).OrderByDescending(x => x.SNO).FirstOrDefault();
                            if (gCase == null)
                            { }
                            else
                            {
                                if (gCase.Rows.Count > 0)
                                {
                                    var name = gCase.Rows[0]["RESP_NAME"].ToString().Trim();
                                    var id = gCase.Rows[0]["RESP_CUST_ID"].ToString().Trim();
                                    string msg= string.Format("經查於本行負責人戶名{0} / ID {1} 。" ,name, id);
                                    List<string> newMemo = new List<string>() { msg };
                                    asBiz.insertCaseMemo(casedoc.CaseId, CaseMemoType.CaseSeizureMemo, newMemo, eQueryStaff);
                                }
                            }
                        }




                        #endregion


                        #region 扣押失敗
                        // 失敗將CaseMaster的AgentUser清空
                        //updateCaseMasterAgentUser(casedoc.CaseId, "");

                        // 要加 CodeType = 'AutoDispatch'
                        PARMCodeBIZ pbiz = new PARMCodeBIZ();

                        bool bAutoDisp = false;
                        var autoDisp = pbiz.GetParmCodeByCodeType("AutoDispatch").FirstOrDefault();

                        if (autoDisp != null)
                        {
                            if ((bool)autoDisp.Enable)
                                bAutoDisp = true;
                            else
                                bAutoDisp = false;
                        }

                        if (bAutoDisp)
                        {
                            #region 若是自動派件, 則採取隨機派件

                            if (humanProcLists.Count() > 0)
                            {
                                AgentSetting nextAgent = getNextAgent(humanProcLists, LastAgentSetting);                                
                                asBiz.updateCaseMasterAgentUser(casedoc.CaseId, nextAgent.EmpId);                                
                                asBiz.updateCaseMasterStatus(casedoc.CaseId, "C01");
                                WriteLog(string.Format("扣押失敗，指派承辦人{0} ", nextAgent.EmpId.ToString()));
                                asBiz.insertCaseHistory(casedoc, "自動扣押", eQueryStaff, "收發-待分文", "收發分派", "自動扣押退回人工", nextAgent.EmpId.ToString(), "經辦-待辦理");
                                LastAgentSetting = nextAgent.EmpId.ToString();                                
                                asBiz.updateLastAssign(nextAgent); // 存入ParmCode 的AssignLast

                            }
                            else // 若無, 則指定eQueryStaff
                            {
                                asBiz.updateCaseMasterAgentUser(casedoc.CaseId, "SYS");
                                asBiz.updateCaseMasterStatus(casedoc.CaseId, "B01");
                                //insertCaseHistory(casedoc, "自動扣押", eQueryStaff, "經辦-辦理", "主管放行結案", "自動扣押", eQueryStaff, "經辦-待辦理");
                                WriteLog(string.Format("扣押失敗，指派承辦人{0} ", eQueryStaff.ToString()));
                                asBiz.insertCaseHistory(casedoc, "自動扣押", eQueryStaff, "收發-待分文", "收發分派", "自動扣押退回人工", eQueryStaff, "經辦-待辦理");
                            }

                            #endregion

                            //CaseMasterBIZ cmb = new CaseMasterBIZ();
                            //string assignAgent = cmb.GetAutoEmployee(CaseKind.CASE_SEIZURE, true, false);
                            //updateCaseMasterAgentUser(casedoc.CaseId, assignAgent);
                            //WriteLog(string.Format("扣押失敗，指派承辦人{0} ", assignAgent.ToString()));
                        }
                        else
                        {
                            #region  要派到收發待分文的階段
                            asBiz.updateCaseMasterStatus(casedoc.CaseId, "B01");
                            asBiz.updateCaseMasterAgentUser(casedoc.CaseId, "SYS");
                            asBiz.insertCaseHistory(casedoc, "自動扣押", eQueryStaff, "收發-待分文", "收發分派", "自動扣押退回人工", eQueryStaff, "收發-待分文");
                            #endregion
                        }
                        #endregion
                    }
                    WriteLog(string.Format("@@@@@@@@@@@@結束扣押:{0}\r\n\r\n\r\n\r\n\r\n", casedoc.CaseNo));

                }
                catch (Exception ex)
                {
                    List<string> docmemo = new List<string>() { "未知的錯誤，落人工" };
                    asBiz.insertCaseMemo(casedoc.CaseId, "CaseSeizure", docmemo, eQueryStaff);
                    WriteLog(string.Format("\r\n\r\n\r\n\r\n未知的錯誤{0}", ex.Message.ToString()));
                    noticeITMail(ITManagerEmail, "未知的錯誤!, 案件編號: " + casedoc.CaseNo +"\t 錯誤訊息:" + ex.Message.ToString());
                }
            } //END  foreach (var casedoc in CaseMasterLists)


            // 20221128, 正達建議, 在執行完成後, 去檢查一下, 是否有999的案件, 若有, 則落人工

            // 去檢查一下, 是否有999的案件, 若有, 則落人工
            WriteLog(string.Format("*********************************將案件共計 {0}, 已執行完成, 檢查是否有卡件******************************", CaseMasterLists.Count().ToString()));
            List<CaseMaster> stillRunningCase = asBiz.getCaseMasterStillRuningCase();
            if (stillRunningCase.Count() > 0)
            {
                WriteLog(string.Format("*********************************發現仍有卡件 , 共計{0} ******************************", stillRunningCase.Count().ToString()));
                foreach(CaseMaster caseInfo in stillRunningCase) 
                {
                    WriteLog(string.Format("*********************************案件 {0}, 已落人工******************************", caseInfo.CaseNo));
                    asBiz.setCaseMasterC01B01(caseInfo.CaseId);
                    List<string> memos = new List<string>() { "電文發查延滯" };
                    asBiz.insertCaseMemo(caseInfo.CaseId, "CaseSeizure", memos, "SYS");
                    noticeITMail(ITManagerEmail, "自動扣押卡件! 案件編號: " + caseInfo.CaseNo + "\t 已落人工" );
                }

            }
            else
            {
                WriteLog(string.Format("*********************************目前本批次無卡件, 程式完成******************************"));
            }





            //WriteLog("====================自動扣押結束====================\r\n\r\n");
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

        private static void ChengHe(Guid CaseId, string userId)
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

                try
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


                    AutoSeizureBiz azBiz = new AutoSeizureBiz();
                    edocTotal = azBiz.getEDocTotal(CaseId);


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
                catch (Exception ex)
                {
                    WriteLog("***************************產生發文檔錯誤******************************");
                    noticeITMail(ITManagerEmail, "產生發文檔錯誤!, 案件編號: " + master.CaseNo + "錯誤訊息: " +ex.Message.ToString());
                }

            }

            #region 呈核主管
            if (result)
            {

                try
                {
                    string mgrid = AToHBIZ.GetManagerID(eQueryStaff);
                    asBiz.insertCaseAssignTable(CaseId, mgrid, 0);
                }
                catch (Exception ex)
                {
                    WriteLog("***************************呈核主管錯誤******************************");
                    noticeITMail(ITManagerEmail, "呈核主管錯誤!, 案件編號: " + master.CaseNo + "錯誤訊息: " + ex.Message.ToString());
                }

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
                        if (model.SendKind == "電子發文")
                        {
                            //第四碼固定為2 --simon 2016/08/05
                            //model.SendNo = model.SendDate.Year + "00" + model.SendNo.Substring(9);
                            model.SendNo = cssBIZ.SendNo();
                            //20170630 緊急 RQ-2015-019666-0?? 發文字號擴碼 宏祥 update start
                            //model.SendNo = model.SendDate.Year + "20" + model.SendNo.Substring(9);
                            model.SendNo = (DateTime.Now.Year - 1911) + "2" + model.SendNo.Substring(9);
                            //20170630 緊急 RQ-2015-019666-0?? 發文字號擴碼 宏祥 update end
                        }
                        else
                        {
                            model.SendNo = cssBIZ.SendNo();
                        }
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








        /// <summary>
        /// 更新發查的人員
        /// </summary>




        private static string doSeizureByCaseId_Co(CaseMaster caseDoc, ref int CaseType)
        {

            WriteLog(string.Format("\r\n\r\n@@@@@@@@@@@@開始扣押:{0}", caseDoc.CaseNo));
            #region 案件層級的私用變數

            PARMCodeBIZ pbiz = new PARMCodeBIZ();

            // 外來文, 加註的所有備註
            List<string> DocMemo = new List<string>();

            // 戶名不符, 仍需要查60491, 33401, 取得餘額, 才能決定發文內容
            bool AccountNameNOTConsistent = false;


            // Case 16, 18 , 若原號的帳戶不足額, 但重號的名稱與來不不一樣.. 不能扣, 只能輸出 依足額與否, 輸出
            // 另查，該身分證統一編號於本行尚有另一帳戶開立之戶名與來函戶名不符。
            //另查，該身分證統一編號於本行尚有另一帳戶開立之戶名與來函戶名不符且存款餘額未達新臺幣200元。
            bool doubleIDNameNotConsistent = false;
            bool isRename = false;
            bool isAmountBelow450 = false;
            string doubleID = "";

            // 本案件, 是否要落人工
            bool Doc2Human = false;

            // 本案件，執行的結果
            var SeizureResult = "0000|扣押成功";

            // 本案件的CaseID
            Guid caseid = caseDoc.CaseId;

            //本案件, 準備發文的備註 .. 從來文字號, 取出 前三個字+ 最後6個字
            //var govNo = caseDoc.GovNo.Substring(0, 3) + caseDoc.GovNo.Substring(caseDoc.GovNo.Length - 7, 6);
            //20200828, 改成新規則
            var govNo = getNewMemo(caseDoc.GovUnit, caseDoc.GovNo);

            // 本案件, 合法的ID
            List<CaseObligor> VaildObligors = new List<CaseObligor>();

            // 本案件, 因為以上的ID, 所帶出來的帳戶
            List<ObligorAccount> AccLists = new List<ObligorAccount>();

            // 本案件, 執行單位
            string GovUnit = "執行署";
            #region 執行單位
            if (caseDoc.GovUnit.IndexOf("執行署") < 0)
                GovUnit = "法院";
            #endregion

            // 本案件, 取得手續費法院1250, 執行署450... 目前沒有法院的電子來文....
            int HandleFee = 450;
            #region 決定手續費 ...
            //var lstFee = pbiz.GetParmCodeByCodeType("MinAmountReq").FirstOrDefault();

            //CSFS.Models.PARMCode oFee = new CSFS.Models.PARMCode();
            //if (GovUnit.Equals("執行署"))
            //    oFee = lstFee.Where(x => x.CodeNo == "pDocExeOffice").FirstOrDefault();
            //else
            //    oFee = lstFee.Where(x => x.CodeNo == "pDocCourtReq").FirstOrDefault();

            //if (oFee != null)
            //    HandleFee = int.Parse(oFee.CodeDesc);           

            #endregion





            // 本案件, 來文的扣押總金額
            decimal SeizureTotal = 0;
            SeizureTotal = asBiz.getEDocTotal(caseDoc.CaseId);

            #region 取得來文扣押金額
            
            string eDocTxtMemo = "";
            eDocTxtMemo = asBiz.geteDocTxtMemo(caseDoc.CaseId);
            
            #endregion


            #region 檢查來文的金額
            // 若有來文, 有小數點, 則落人工
            int iSeizureTotal = (int)SeizureTotal;
            if (SeizureTotal != (decimal)iSeizureTotal)
            {
                //SeizureResult = "0030|來文扣押金額有小數點，落人工";
                //DocMemo.Add("來函扣押金額異常(請查明)。");
                //asBiz.insertCaseMemo(caseid, "CaseSeizure", DocMemo, eQueryStaff);
                isAmountBelow450 = true;
                //return SeizureResult; // 不可直接回文, 要查完帳戶後, 才能回文
            }

            // 若有來文, 有負數, 則落人工            
            if (SeizureTotal < 0)
            {
                //SeizureResult = "0031|來文扣押金額為負數，落人工";
                //DocMemo.Add("來函扣押金額異常(請查明)。");
                //asBiz.insertCaseMemo(caseid, "CaseSeizure", DocMemo, eQueryStaff);
                isAmountBelow450 = true;
                //return SeizureResult; // 不可直接回文, 要查完帳戶後, 才能回文
            }


            // 若有來文, 有負數, 則落人工            
            if (SeizureTotal < HandleFee)
            {
                //SeizureResult = "0032|來文扣押金額低於" + HandleFee.ToString() + "元，落人工";
                //DocMemo.Add("來函扣押金額異常(請查明)。");
                //asBiz.insertCaseMemo(caseid, "CaseSeizure", DocMemo, eQueryStaff);
                isAmountBelow450 = true;
                //return SeizureResult; // 不可直接回文, 要查完帳戶後, 才能回文
            }

            WriteLog(string.Format("來文扣押的總金額: {0}", SeizureTotal.ToString()));

            #endregion


            //  此人的加總的可用餘額, 必須折算回台幣
            decimal TotalTrueAmt = 0.0m;

            // 所有的訊息
            List<string> SeizureMessage = new List<string>();


            // 讀取參數檔中的優先順序..
            // SELECT *   FROM [CSFS_SIT].[dbo].[PARMCode] where codetype='SeizureSeqence'
            var seizureSequenceAll = pbiz.GetParmCodeByCodeType("SeizureSeqence");
            Dictionary<string, string> BranchInfo = pbiz.GetParmCodeByCodeType("RCAF_BRANCH").ToDictionary(x => x.CodeNo, x => x.CodeDesc);
            // 5.1 正向表列, 可能被扣押的帳戶類型
            Dictionary<string, List<string>> SeizureOrder = getSeizureOrder(seizureSequenceAll);


            // 5.2 負向表列, 絕對不可被扣押的帳戶類型  2018/05/09  包括DCI, SI 理財定存, 產品代碼
            // 0080, 表示台幣  // XX80, 非00, 表示外幣, 
            Dictionary<string, List<string>> SeizureNOTOrder = getSeizureNOTOrder(seizureSequenceAll);
            Dictionary<string, string> SeizureOrderTable = getReverseSeizureOrder(seizureSequenceAll);

            #endregion

            #region 開始把CaseMaster的Status 改成C02
            asBiz.updateCaseMasterStatus(caseid, "C02");
            #endregion

            # region 若來文有備註文字, 人工
            if (!string.IsNullOrEmpty(eDocTxtMemo))
            {
                SeizureResult = "0019|來文有備註文字，落人工";
                DocMemo.Add("來文有備註文字。");
                asBiz.insertCaseMemo(caseid, "CaseSeizure", DocMemo, eQueryStaff);
                //insertBatchQuene(caseDoc.CaseId, eQueryStaff, caseDoc.DocNo,caseDoc. d.Key);
                return SeizureResult;
            }
            #endregion

            #region Step 1：取出 allCaseMaster 中的CaseID之義務人 到_caseObligors


            
            var _caseObligors = asBiz.Step1(caseDoc);


            if (_caseObligors == null)
            {
                SeizureResult = "0014|ID無法辨識";
                DocMemo.Add("ID無法辨識");
                asBiz.insertCaseMemo(caseid, "CaseSeizure", DocMemo, eQueryStaff);

                return SeizureResult;
            }

            if (_caseObligors.Count() == 0)
            {
                SeizureResult = "0014|ID無法辨識";
                DocMemo.Add("ID無法辨識");
                asBiz.insertCaseMemo(caseid, "CaseSeizure", DocMemo, eQueryStaff);
                return SeizureResult;
            }

            { // 若來函的姓名, 為空白, 則落人工
                bool isEmptyName = false;
                foreach(var c in _caseObligors)
                {
                    if (string.IsNullOrEmpty(c.ObligorName))
                        isEmptyName = true;
                }
                if( isEmptyName)
                {
                    SeizureResult = "0014|來函的姓名, 為空白, 則落人工";
                    DocMemo.Add("來函的姓名, 為空白, 則落人工");
                    asBiz.insertCaseMemo(caseid, "CaseSeizure", DocMemo, eQueryStaff);
                    foreach (var c in _caseObligors)
                    {
                        insertBatchQuene(caseDoc.CaseId, eQueryStaff, caseDoc.DocNo, c.ObligorNo);
                    }
                    return SeizureResult;
                }
            }



            WriteLog(string.Format("Step 1 : 取得義務人ID = {0} / {1} 的結果", string.Join(",", _caseObligors.Select(x => x.ObligorNo)), string.Join(",", _caseObligors.Select(x => x.ObligorName))));

            #endregion

            #region Step 2 : 找出重號, 填入VaildObligors

            List<string> DiffLists = new List<string>() { "*", "?", "" };

            var strDiffWord = pbiz.GetParmCodeByCodeType("DifficultWord").FirstOrDefault();
            if (strDiffWord != null)
            {
                DiffLists = strDiffWord.CodeDesc.Split(',').ToList();
            }

            bool isDiffWordAll = false;
            string strMessage = null;

            //List<CaseObligor> ValidObligors = Step2(caseDoc, _caseObligors, ref strMessage, DiffLists, ref DocMemo);

            int CaseTypeID = getCaseType(_caseObligors);
            if( CaseTypeID <0)
            {
                WriteLog(string.Format("\r\n\r\n無法判斷企業型態, 案件編號 : {0}", caseDoc.CaseNo));
                return "0032|無法判斷企業型態";
            }

            List<CaseObligor> ValidObligors = new List<CaseObligor>();

            CaseType = CaseTypeID;

            switch (CaseTypeID)
            {
                case 2: // 純8碼 + 公司
                    ValidObligors = Case2(caseDoc, _caseObligors, ref strMessage, DiffLists, ref DocMemo);
                    break;
                case 3: // 純8碼 + 行號
                    ValidObligors = Case3(caseDoc, _caseObligors, ref strMessage, DiffLists, ref DocMemo);
                    break;
                case 4:// 8+10碼 + 公司
                    ValidObligors = Case4(caseDoc, _caseObligors, ref strMessage, DiffLists, ref DocMemo);
                    break;
                case 5:// 8+10碼 + 行號
                    ValidObligors = Case5(caseDoc, _caseObligors, ref strMessage, DiffLists, ref DocMemo);
                    break;
            }

            if( ! strMessage.StartsWith("0000|"))
            {
                if (ValidObligors == null) // 20181001,  表示來文8碼, 戶名不符, 直接落人工
                    return strMessage;
            }


            if (VaildObligors != null)
            {
                VaildObligors = ValidObligors.DistinctBy(x => x.ObligorNo).ToList();
                WriteLog("======================================================================================================");
                WriteLog("Step 2 :          ObligorNo / ObligorName / 型態 / 名稱相同 / 有更名(新名字) / 負責人資訊相同");
                foreach (var v in VaildObligors)
                {
                    if (v.isDoubleAcc)
                        WriteLog(string.Format("Step 2 : 重號 = {0} / {1} / {2} / {3} / {4} ({5}) / {6}", v.ObligorNo, v.ObligorName, v.type == "C" ? "公司" : "行號", v.isSame.ToString(), v.isChangeName.ToString(), v.newName, v.isRespInfoSame.ToString()));
                    else
                        WriteLog(string.Format("Step 2 : 本號 = {0} / {1} / {2} / {3} / {4} ({5}) / {6}", v.ObligorNo, v.ObligorName, v.type == "C" ? "公司" : "行號", v.isSame.ToString(), v.isChangeName.ToString(), v.newName, v.isRespInfoSame.ToString()));
                }
            }
            //WriteLog(string.Format("Step 2 : 找出重號及所有義務人ID = {0} / {1} 的結果", string.Join(",", VaildObligors.Select(x => x.ObligorNo)), string.Join(",", VaildObligors.Select(x => x.ObligorName))));








            // 20180910 , 若有任何落人工的動作, 一律InsertBatchQuene 把帳務的部分, 都查完
            // 原因是, 某人, 有設立籌備處, 因此有重號, 但 籌備處 的戶名 > 3 , 因此, 會直接落人工, 而本號, 確沒有去查 所造成

            #region 落人工的原因, 要查到全部重號
            if (strMessage.StartsWith("0002|")  )
            {
                WriteLog("\t\tStep2-1-1 : 戶名有難字，請確認");
                DocMemo.Add("戶名有難字，請確認");
                Doc2Human = true;
                SeizureResult = "0015|戶名有難字，請確認";
                asBiz.insertCaseMemo(caseid, "CaseSeizure", DocMemo, eQueryStaff);
                foreach (var s in VaildObligors)
                {
                    insertBatchQuene(caseDoc.CaseId, eQueryStaff, caseDoc.DocNo, s.ObligorNo);
                }
                return SeizureResult;
            }

            if (strMessage.StartsWith("0028|0003|"))
            {
                WriteLog("\t\tStep2-1-1 : 戶名長度超過18個字，落人工");
                //DocMemo.Add("戶名長度超過18個字，落人工");
                Doc2Human = true;
                SeizureResult = "0033|戶名長度超過18個字，落人工";
                //發查60629失敗, 原因 : 0003|戶名長度超過18個字，落人工
                List<string> newDM = new List<string>();
                foreach(var a in DocMemo)
                {
                    if (a.Contains("發查60629失敗, 原因 : 0003|"))
                        newDM.Add("戶名長度超過18個字，落人工");
                    else
                        newDM.Add(a);
                }
                asBiz.insertCaseMemo(caseid, "CaseSeizure", newDM, eQueryStaff);

                CaseMemoBiz cmb = new CaseMemoBiz();
                var abb = cmb.Delete3(caseDoc.CaseId, CaseMemoType.CaseSeizureMemo, "發查60629失敗, 原因 : 0003|", null);


                foreach (var s in VaildObligors)
                {
                    insertBatchQuene(caseDoc.CaseId, eQueryStaff, caseDoc.DocNo, s.ObligorNo);
                }
                return SeizureResult;
            }

            //if (!strMessage.StartsWith("0000|"))
            //{
            //    WriteLog("\t\tStep2-1-1 : 找不到合法義務人，請確認");
            //    DocMemo.Add("找不到合法義務人，請確認");
            //    Doc2Human = true;
            //    SeizureResult = "0015|找不到合法義務人，請確認";
            //    asBiz.insertCaseMemo(caseid, "CaseSeizure", DocMemo, eQueryStaff);
            //    foreach (var s in VaildObligors)
            //    {
            //        insertBatchQuene(caseDoc.CaseId, eQueryStaff, caseDoc.DocNo, s.ObligorNo);
            //    }
            //    return SeizureResult;
            //}

            #endregion



            #endregion

            #region Step 3 : 找出所有這些義務人的帳號, 打60491

            // 此變數準備 不在 ProdCode 中(正向可扣押, 負向不可扣押)的帳務, 判斷是否有漏掉的帳戶, 若還扣不足額, 需要落人工提示
            List<ObligorAccount> allAccLists = new List<ObligorAccount>();

            if (VaildObligors==null || VaildObligors.Count() == 0)
            {
                //DocMemo.Add("於本行無存款往來。");
                Doc2Human = true;
                SeizureResult = "0027|於本行無存款往來。";
                asBiz.insertCaseMemo(caseid, "CaseSeizure", DocMemo, eQueryStaff);
                //insertBatchQuene(caseDoc.CaseId, eQueryStaff, caseDoc.DocNo, d.Key);
                WriteLog("\tStep3-1 : 找不到合法的ID");
                return SeizureResult;
            }


            foreach (var v in VaildObligors)
            {
                var pkid = asBiz.addAutoLog(caseid, eQueryStaff, caseDoc.DocNo, v.ObligorNo, "60491");
                string retMessage = "";

                //20180928 
                // 若是行號, 且負責人姓名與來文不同, 則要把該Account的NoSeizure 設為True, 表示不能扣押
                bool isNoSeizure = false;
                if ( v.type == "D"  && v.ObligorNo.Length<10)
                {
                    bool isResNot = false;
                    foreach(var a in DocMemo)
                    {
                        if (a.Contains("!@!經查於本行負責人") || a.Contains("!@!戶名不符。") || a.Contains("8碼行號的戶名不符"))
                        {
                            isResNot = true;
                        }
                            
                    }                    
                    if (isResNot)
                        isNoSeizure = true;
                }
                else
                    isNoSeizure = false;

                if( v.type=="C" && v.ObligorNo.Length >=10) // Case 4 , 10碼, 才會有戶名不符的問題
                {
                    bool isResNot = false;
                    foreach (var a in DocMemo)
                    {
                        if ( a.Contains("!@!戶名不符。"))
                            isResNot = true;
                    }
                    if (isResNot)
                        isNoSeizure = true;
                }

                List<ObligorAccount> oaList = objSeiHTG.Send60491_Co(v.ObligorNo, v.CaseId, BranchInfo, ref retMessage, v.isChangeName, v.isSame, v.newName, v.isDoubleAcc, v.type, isNoSeizure).Where(x => !x.Account.Contains("000000000000")).ToList();
                if (retMessage.StartsWith("0000|"))
                    asBiz.updateAutoLog(pkid, 1, retMessage);
                else
                {
                    retMessage = "0099|發查60491失敗";
                    asBiz.updateAutoLog(pkid, 2, retMessage);
                    WriteLog("發查失敗 " + retMessage);
                    DocMemo.Add("發查60491失敗");
                    asBiz.insertCaseMemo(caseid, "CaseSeizure", DocMemo, eQueryStaff);
                    return "0028|" + retMessage;
                }

                // 20180814, 每個ID, 都要打RM(67050-8), 並存到TX_67002 中, 即可
                #region 每個ID, 都要打RM, 並存到TX_67002 中, 即可
                try
                {
                    var pkid333 = asBiz.addAutoLog(caseid, eQueryStaff, caseDoc.DocNo, v.ObligorNo, "67050-8");
                    string retMessage333 = objSeiHTG.Send67002(v.ObligorNo, v.CaseId.ToString());
                    if (retMessage333.StartsWith("0000|"))
                        asBiz.updateAutoLog(pkid333, 1, retMessage333);
                    else
                    {
                        retMessage = "0099|發查67505-8失敗";
                        asBiz.updateAutoLog(pkid333, 2, retMessage333);
                        WriteLog("發查失敗 " + retMessage333);
                        DocMemo.Add("發查67505-8失敗");

                        asBiz.insertCaseMemo(caseid, "CaseSeizure", DocMemo, eQueryStaff);
                        return "0028|" + retMessage;
                    }
                }
                catch (Exception ex)
                { }
                #endregion


                // 要排除 ("關係別") TX_60491_Detl.Link = 'JOIN'
                // 要排除 TX_60491_Detl.StsDesc = '結清' or '放款' or '現金卡'

                List<ObligorAccount> newOaList = getSeizureAccount(oaList); // 將TX_60491_Detl.StsDesc = '結清' or '放款' or '現金卡', ("關係別") TX_60491_Detl.Link = 'JOIN' 排除

                AccLists.AddRange(newOaList);
                WriteLog("======================================================================================================");
                WriteLog(string.Format("\tStep 3-2-1 : ID = {0}, 共有{1} 帳號, 分別為{2}", v.ObligorNo, oaList.Count().ToString(), string.Join(",", oaList.Select(x => x.Account))));
                WriteLog("\t排除 結清  放款 現金卡JOIN後");
                WriteLog(string.Format("\tStep 3-2-2 : ID = {0}, 共有{1} 帳號, 分別為{2}", v.ObligorNo, newOaList.Count().ToString(), string.Join(",", newOaList.Select(x => x.Account))));

            }

            WriteLog(string.Format("Step 3 : 本案件共有{0} 帳號", AccLists.Count().ToString()));

            #endregion

            #region Step 4 根據AccLists中所有的帳戶，去查歷史上的扣押交易, 順便把查401的餘額填入Bal

            //20181023, A10710190002, 發現, 若重號若無往來, 則什麼都不說
           
            foreach(var v in VaildObligors)
            {
                var pAcc = AccLists.Where(x=>x.Id==v.ObligorNo).ToList();
                if( pAcc.Count==0 || v.isDiffWord) // 若無存款往來, 則什麼也不說
                {

                }
                else
                {
                    if(! string.IsNullOrEmpty(v.docMemo))
                        DocMemo.Add(v.ObligorNo + "!@!" + v.docMemo);
                }

            }



            if (AccLists.Count() == 0)
            {
                //DocMemo.Add("於本行無存款往來。");
                Doc2Human = true;
                SeizureResult = "0008|於本行無存款往來。";
                //asBiz.insertCaseMemo(caseid, "CaseSeizure", DocMemo, eQueryStaff);
                return SeizureResult;
            }

            foreach (var item in AccLists)
            {

                bool isForeign = false;
                bool bisOD = false;
                bool bisTD = false;
                bool bisLon = false;
                bool bisHoldAmt = false; ; // 他案扣押金額
                string errMessage = null;
                item.AccountType = lookUpAccountType(item.ProdCode, SeizureOrderTable);
                if (item.AccountType.StartsWith("外幣"))
                    isForeign = true;
                item.Bal = getBalance(item.Id, item.CaseId, item.Account, ref bisOD, ref bisTD, ref bisLon, ref bisHoldAmt, caseDoc.CaseNo, item.Ccy, ref errMessage, isForeign);


                if (string.IsNullOrEmpty(errMessage))
                { }
                else
                {
                    //0001|電文00450 發查失敗01|交易限制
                    //errMessage = "0028|發查401失敗";
                    DocMemo.Add("帳戶:" + item.Account + "發查失敗, 原因:" + errMessage);
                    asBiz.insertCaseMemo(caseid, "CaseSeizure", DocMemo, eQueryStaff);
                    return "0028|" + errMessage;
                }



                item.isOD = bisOD;
                item.isTD = bisTD;
                item.isLon = bisLon;

                // 20180802, 電子的部分, 450-31有他案扣押, 也照樣扣, 
                if (bisHoldAmt)
                {
                    //item.message45031 = "帳戶因他案已扣押在案。(請查明)";
                    item.is45031OK = true;
                }
                else
                {
                    //item.message45031 = "";
                    item.is45031OK = false;
                }

                var pkid = asBiz.addAutoLog(caseid, eQueryStaff, caseDoc.DocNo, item.Id, "45030");
                string retMessage = "";
                string message450 = "";
                string message450Code = "";

                // 
                item.is450OK = false; item.message450 = ""; item.message450Code = "";

                if (item.StsDesc.Trim().Equals("事故"))
                {

                    Dictionary<string, string> dic450Result = objSeiHTG.getAccident2(item.Id, item.CaseId, item.Account, item.Ccy, ref retMessage);
                    if (dic450Result == null)
                    {
                        item.is450OK = false; item.message450 = ""; item.message450Code = "";
                    }
                    else
                    {
                        if (dic450Result.Count() > 0)
                        {
                            item.is450OK = true;
                            item.message450Code = string.Join("@", dic450Result.Keys);
                            item.message450 = string.Join("@", dic450Result.Values);
                        }
                        else
                        {
                            item.is450OK = false; item.message450 = ""; item.message450Code = "";
                        }
                    }


                    if (retMessage.StartsWith("0000|"))
                        asBiz.updateAutoLog(pkid, 1, retMessage);
                    else
                    {
                        asBiz.updateAutoLog(pkid, 2, retMessage);

                        //0001|電文00450 發查失敗01|交易限制
                        DocMemo.Add("帳戶:" + item.Account + "發查失敗, 原因:" + retMessage.Replace("0001|電文00450 發查失敗", ""));
                        asBiz.insertCaseMemo(caseid, "CaseSeizure", DocMemo, eQueryStaff);
                        return "0028|" + retMessage;
                    }
                }
                WriteLog(string.Format("\tStep 4-3 : ID {0},  帳戶{1}, ProdCode {2},  餘額 {3}, 幣別 {4}", item.Id, item.Account, item.ProdCode, item.Bal.ToString(), item.Ccy));
                WriteLog(string.Format("\tStep 4-4 : OD={0}, TD={1}, 放款={2}, 他案={3}, 不可扣押={4}", bisOD.ToString(), bisTD.ToString(), bisLon.ToString(), bisHoldAmt.ToString(), item.noSeizure.ToString()));
                WriteLog(string.Format("\tStep 4-5 : 事故代號={0}, 事故訊息={1}", item.message450Code, item.message450));
            }


            WriteLog("Step 4: 查詢33401 及450-30, 450-31 相關帳戶資訊!!");
            #endregion

            #region Step 5: 針對AccLists的排除不可扣押的類型, 並在扣押前, 及查401



            // 5.1 先從AccLists中, 拿掉, 絕對不可被扣押的帳戶類型
            #region 排除絕對不可被扣押的帳戶類型

            var delAccList = AccLists.Where(x => x.AccountType == "確定不用扣押").ToList();
            AccLists = AccLists.Except(delAccList).ToList();

            #endregion


            // 5.2 排除 "結清", "已貸", "啟用", "誤開", "新戶" 的帳戶



            #region  排除 "結清", "已貸", "啟用", "誤開", "新戶" 的帳戶
            List<ObligorAccount> newAccList = new List<ObligorAccount>();
            List<string> noSave = new List<string>() { "結清", "已貸", "啟用", "誤開", "新戶","核淮","婉拒","作廢" };
            //
            foreach (var acc in AccLists)
            {
                bool bfilter = true;
                if (acc.Account.StartsWith("000000000000"))
                    bfilter = false;

                #region 判斷是否是現金卡等等
                // 若 prod_code = 0058, 或XX80 , 不用存
                if (acc.ProdCode.ToString().Equals("0058") || acc.ProdCode.ToString().EndsWith("80"))
                    bfilter = false;

                // 若  Link<>'JOIN' , 不用存
                if (acc.Link.ToString().Equals("JOIN"))
                    bfilter = false;

                // 若 StsDesc='結清' AND  StsDesc='已貸' AND  StsDesc='啟用' AND  StsDesc='誤開'  AND  StsDesc='新戶', 也不用存
                string sdesc = acc.StsDesc.ToString().Trim();
                if (noSave.Contains(sdesc))
                    bfilter = false;

                #endregion

                if (bfilter)
                    newAccList.Add(acc);
            }

            AccLists = newAccList;
            #endregion


            // 記錄在排序之前, 目前可能可以被扣押的帳戶, (一定包括可扣押的帳戶, 也包括, 未列出的)
            foreach (var acc in AccLists)
            {
                allAccLists.Add(acc);
            }






            // 5.3 開始排順序, 
            // 要分二種, 第一種是, 只有"個人"或"公司"
            //                  第二種是, "個人及公司"都有的...


            //List<ObligorAccount> OrderedAccLists = new List<ObligorAccount>();




            //int pCount = getPersonCount(AccLists); // AccLists.Count(x => x.Id.Length >= 10); // 個人的帳戶數
            //int cCount = getComCount(AccLists); // AccLists.Count(x => x.Id.Length < 10); // 公司的帳戶數
            //WriteLog(string.Format("Step 5: 待扣押帳戶 個人戶{0}, 公司戶{1} ", pCount.ToString(), cCount.ToString()));


            #endregion

            #region Step 6: 開始扣押, 8碼的 公司 優先, 其次個人

            string strCalcResult = null; // 本變數儲存, 計算後扣押的結果
            #region 同時有個人ID與公司ID, 都要扣押,公司 優先, 其次個人

            WriteLog(string.Format("\tStep 6-1: 扣押個人戶及公司戶, 以公司戶優先扣,其次個人 "));
                //先個人, 再公司, 但右有扣到公司, 則另外需要針對公司帳號去扣450元..

                // 針對這些個人帳戶, 去扣押  
                decimal realSeizureAmt = 0.0m; // 個人戶, 實際扣押金額
                //SeizureResult = PersonSeizure(caseDoc, AccLists, SeizureOrder, pbiz, HandleFee, SeizureTotal, govNo, ref DocMemo, doubleIDNameNotConsistent, AccountNameNOTConsistent, isRename, isAmountBelow450, allAccLists, doubleID, VaildObligors, ref realSeizureAmt);
                SeizureResult = CompanySeizure2(caseDoc, AccLists, SeizureOrder, pbiz, HandleFee, SeizureTotal, govNo, ref DocMemo,  isAmountBelow450, allAccLists, doubleID, VaildObligors, ref realSeizureAmt);


                #region

                #endregion
                WriteLog(string.Format("Step 6 : 扣押結果{0}", SeizureResult));


            #endregion
            #endregion


            return SeizureResult;

        }

        private static int getCaseType(List<CaseObligor> _caseObligors)
        {


            var CoObligors = _caseObligors.Where(x => x.ObligorNo.Length == 8).ToList();
            var PeObligors = _caseObligors.Where(x => x.ObligorNo.Length > 8).ToList();

            if (CoObligors.Count()==0)
            {
                //strMesage = "0001|有錯, 來文義務人，沒有包括8碼統編";
                return -1;
            }
            else
            {
                var firCo = CoObligors.FirstOrDefault();
                if(PeObligors.Count()>0) // 8碼+10碼, Case 4 Case 5
                {                    
                    if (firCo.ObligorName.Contains("有限公司") || firCo.ObligorName.Contains("股份有限公司"))
                        return 4;
                    else
                        return 5;
                }
                else // 8碼而已, Case 2, Case 3
                {
                    if (firCo.ObligorName.Contains("有限公司") || firCo.ObligorName.Contains("股份有限公司"))
                        return 2;
                    else
                        return 3;
                }

            }




        }



        /// <summary>
        /// Step 2 : 找到合法的義務人
        /// </summary>
        /// <param name="docObligors"></param>
        /// <returns>
        /// 0000|成功, 無錯誤
        /// 0001|有錯, 來文義務人，沒有包括8碼統編
        /// 0002|有難字
        /// 
        /// </returns>
        public static List<CaseObligor> Step2(CaseMaster caseDoc, List<CaseObligor> docObligors, ref string strMesage, List<string> DiffLists, ref List<string> DocMemo)
        {

            #region 檢查難字

            bool isDifficult = false;
            foreach (var d in docObligors)
            {
                foreach (var s in DiffLists)
                {
                    if (d.ObligorName.Contains(s))
                        isDifficult = true;
                }
            }
            if (isDifficult) //戶名有*號的, 表示有難字, 落人工處理                    
            {
                strMesage = "0002|有難字";
                return null;
            }
            #endregion
            
             List<CaseObligor> ValidObligors = new List<CaseObligor>();

            #region 找出是公司戶(C)或是行號(D)

            var strType = "";
           
            var CoObligors = docObligors.Where(x => x.ObligorNo.Length == 8).FirstOrDefault();

            if (CoObligors == null)
            {
                strMesage = "0001|有錯, 來文義務人，沒有包括8碼統編";
                return null;
            }
            else
            {
                if (CoObligors.ObligorName.Contains("有限公司") || CoObligors.ObligorName.Contains("股份有限公司"))
                    strType = "C";
                else
                    strType = "D";
            }

            #endregion


            WriteLog(string.Format("\t\tStep 2-1-1: 判斷為 {0} ", strType=="C" ? "公司" : "行號" ));





            foreach (var v in docObligors)
            {
                v.type = strType;



                string respName = "";
                string respId = "";
                if( strType=="D") // 如果是行號, 要找出負責人
                {
                    #region 如果是行號, 要打67050-6, 找到負責人
                    if (strType == "D" && v.ObligorNo.Length == 8)
                    {
                        var pkid22 = asBiz.addAutoLog(caseDoc.CaseId, eQueryStaff, caseDoc.DocNo, v.ObligorNo, "67050-8");
                        string retMessage22 = "";
                        string respInfo = objSeiHTG.Send67102(v.ObligorNo, v.CaseId, ref retMessage22);
                        if (retMessage22.StartsWith("0000"))
                        {
                            if (!string.IsNullOrEmpty(respInfo))
                            {
                                string[] nameInfo = respInfo.Split('@');
                                respId = nameInfo[0];
                                respName = nameInfo[1];
                            }
                            else
                            {
                                WriteLog("查不到負責人!!");
                            }
                        }
                        else
                        {
                            WriteLog("發查67050-6失敗");
                        }
                    }
                    #endregion  
                }





                #region 發查重號 60628 , 存到doubleIDs
                Dictionary<string, string> doubleIDs = new Dictionary<string, string>();

                var pkid = asBiz.addAutoLog(caseDoc.CaseId, eQueryStaff, caseDoc.DocNo, v.ObligorNo, "60629");
                string retMessage = "";
                doubleIDs = objSeiHTG.Send60629(v.ObligorNo, v.CaseId, ref retMessage);
                if (doubleIDs.Count() == 0)
                {
                    doubleIDs.Add(v.ObligorNo, v.ObligorName);
                }

                if (retMessage.StartsWith("0000|"))
                    asBiz.updateAutoLog(pkid, 1, retMessage);
                else
                {
                    asBiz.updateAutoLog(pkid, 2, retMessage);
                    //0001|電文00450 發查失敗01|交易限制
                    //DocMemo.Add("發查60629失敗, 原因 : " + retMessage);
                    //asBiz.insertCaseMemo(caseDoc.CaseId, "CaseSeizure", DocMemo, eQueryStaff);
                    strMesage = "0028|" + retMessage;
                    return null;
                }
                WriteLog(string.Format("\t\tStep 2-1-2:  查詢重號結果 {0} / {1}", string.Join(", ", doubleIDs.Select(x => x.Key)), string.Join(", ", doubleIDs.Select(x => x.Value))));
                #endregion

                #region 檢查重號, 是否是合於扣押的義務人, 若合於, 則加入ValidObligors中
                int i = 1;
                foreach (var d in doubleIDs)
                {
                    bool isDoubleAcc = i == 1 ? false : true;
                    if (v.ObligorName.Trim().Equals(d.Value.Trim())) // 姓名相符, 加入ValidObligors
                    {
                        #region Insert VaildObligor
                        CaseObligor c = new CaseObligor();
                        c.ObligorNo = d.Key;
                        c.CaseId = caseDoc.CaseId;
                        c.CreatedDate = DateTime.Now;
                        c.ObligorName = d.Value;
                        c.isChangeName = false; // 沒有改名
                        c.isDoubleAcc = isDoubleAcc;
                        c.isSame = true; // 原來名字
                        c.newName = "";
                        c.type = strType;
                        ValidObligors.Add(c);
                        #endregion
                        WriteLog(string.Format("\t\t\tStep 2-1-2-2: 戶名相符 來文戶名: {0} = 主機戶名:{1}", v.ObligorName, d.Value));
                    }
                    else // 姓名不相符, 去發查60600, 找出是否曾經相符過(過去有沒有改名字過)
                    {
                        WriteLog(string.Format("\t\t\tStep 2-1-2-2: 戶名不相符 來文戶名: {0} / 主機戶名:{1}", v.ObligorName, d.Value));
                        #region 打60600, 確認是否有更名, 若是公司戶, 不用管名字對不對, 若是行號, 才要管                        
                        List<string> anoNames = new List<string>();

                        var pkid222 = asBiz.addAutoLog(caseDoc.CaseId, eQueryStaff, caseDoc.DocNo, v.ObligorNo, "60600");
                        string retMessage222 = "";
                        anoNames = objSeiHTG.Send60600(d.Key, caseDoc.CaseId, ref retMessage222);
                        if (retMessage222.StartsWith("0000|"))
                            asBiz.updateAutoLog(pkid222, 1, retMessage222);
                        else
                        {
                            asBiz.updateAutoLog(pkid222, 2, retMessage222);
                            //asBiz.updateAutoLog(pkid, 2, retMessage);
                            DocMemo.Add("發查60600失敗, 原因 : " + retMessage);
                            asBiz.insertCaseMemo(caseDoc.CaseId, "CaseSeizure", DocMemo, eQueryStaff);
                            strMesage =  "0028|" + retMessage;
                            return null;
                        }

                        #endregion
                        //if( v.ObligorNo=="F223596365")
                        //    anoNames = new List<string>() {{"徐大智            CHANGE      徐小智"}};

                        List<CaseObligor> vObligors = changeName(caseDoc, anoNames, v.ObligorName, v.ObligorNo, ref DocMemo, strType, isDoubleAcc);
                        ValidObligors.AddRange(vObligors);
                    }
                    i++;
                }


                #endregion
            }
            strMesage = "0000|";
            return ValidObligors;
        }



        /// <summary>
        /// 8碼, 並為行號
        /// </summary>
        /// <param name="caseDoc"></param>
        /// <param name="docObligors"></param>
        /// <param name="strMesage"></param>
        /// <param name="DiffLists"></param>
        /// <param name="DocMemo"></param>
        /// <returns></returns>
        public static List<CaseObligor> Case2(CaseMaster caseDoc, List<CaseObligor> docObligors, ref string strMesage, List<string> DiffLists, ref List<string> DocMemo)
        {
            //在此案件中, 只要扣8碼的..不用管負責人, 但要比對戶名一致性
            #region 檢查難字

            bool isDifficult = false;
            foreach (var d in docObligors)
            {
                foreach (var s in DiffLists)
                {
                    if (d.ObligorName.Contains(s))
                        isDifficult = true;
                }
            }
            if (isDifficult) //戶名有*號的, 表示有難字, 落人工處理                    
            {
                strMesage = "0002|有難字";
                return null;
            }
            #endregion

            List<CaseObligor> ValidObligors = new List<CaseObligor>();

            string strType = "C";

            WriteLog(string.Format("\t\tStep 2-1-1: 判斷為 {0} / Case 2 ", strType == "C" ? "公司" : "行號"));

            // 先找出負責人是否相符, 用公司ID找
            var v = docObligors.Where(x => x.ObligorNo.Length == 8).FirstOrDefault();
            {
                v.type = strType;
                // 發查60628, 把二者都加入ValidObligors
                Dictionary<string, string> doubleIDs = new Dictionary<string, string>();
                #region 發查v的重號 60628 , 存到doubleIDs


                var pkid = asBiz.addAutoLog(caseDoc.CaseId, eQueryStaff, caseDoc.DocNo, v.ObligorNo, "60629");
                string retMessage = "";
                doubleIDs = objSeiHTG.Send60629(v.ObligorNo, v.CaseId, ref retMessage);
                if (doubleIDs.Count() == 0)
                {
                    doubleIDs.Add(v.ObligorNo, v.ObligorName);
                }

                if (retMessage.StartsWith("0000|"))
                    asBiz.updateAutoLog(pkid, 1, retMessage);
                else
                {
                    if (retMessage.EndsWith("電文訊息 :找不到相關資料"))
                    {
                        WriteLog("\t\t **** 發查60629, 找不到相關資料, 表示無存款往來");
                        //20190107, 表示無存款往來, 直接回文
                        asBiz.updateAutoLog(pkid, 1, retMessage);
                        return ValidObligors;
                    }
                    else
                    {


                        asBiz.updateAutoLog(pkid, 2, retMessage);
                        //0001|電文00450 發查失敗01|交易限制
                        DocMemo.Add("發查60629失敗, 原因 : " + retMessage);
                        asBiz.insertCaseMemo(caseDoc.CaseId, "CaseSeizure", DocMemo, eQueryStaff);
                        strMesage = "0028|" + retMessage;
                        var resC1 = addObligor(doubleIDs, caseDoc, strType, v.ObligorName, v.ObligorNo, ref DocMemo, ref strMesage);
                        ValidObligors.AddRange(resC1);
                        return ValidObligors;
                    }
                }
                WriteLog(string.Format("\t\tStep 2-1-2:  查詢重號結果 {0} / {1}", string.Join(", ", doubleIDs.Select(x => x.Key)), string.Join(", ", doubleIDs.Select(x => x.Value))));
                #endregion

                #region 發查67072
                {
                    var pkid72 = asBiz.addAutoLog(caseDoc.CaseId, eQueryStaff, caseDoc.DocNo, v.ObligorNo, "67072");
                    string retMessage72 = "";
                    var d67072 = objSeiHTG.Send67072(v.ObligorNo, v.CaseId, ref retMessage72);


                    if (d67072.StartsWith("0000|"))
                        asBiz.updateAutoLog(pkid72, 1, retMessage72);
                    else
                    {
                        asBiz.updateAutoLog(pkid72, 2, retMessage72);
                        //0001|電文00450 發查失敗01|交易限制
                        DocMemo.Add("發查67072失敗, 原因 : " + retMessage72);
                        asBiz.insertCaseMemo(caseDoc.CaseId, "CaseSeizure", DocMemo, eQueryStaff);
                        //return "0028|" + retMessage72;
                    }
                }

                #endregion

                var resC = addObligor(doubleIDs, caseDoc, strType, v.ObligorName, v.ObligorNo, ref DocMemo, ref strMesage);
                ValidObligors.AddRange(resC);
            }

            if (strType == "C") // 公司型態, 不管戶名符不符, 都要扣
            {
                foreach (var a in ValidObligors)
                    a.isSame = true;
                WriteLog("\t\t公司型態, 不管戶名符不符, 都要扣");
            }

            strMesage = "0000|";
            return ValidObligors;
        }



        /// <summary>
        /// 8碼, 並為行號
        /// </summary>
        /// <param name="caseDoc"></param>
        /// <param name="docObligors"></param>
        /// <param name="strMesage"></param>
        /// <param name="DiffLists"></param>
        /// <param name="DocMemo"></param>
        /// <returns></returns>
        public static List<CaseObligor> Case3(CaseMaster caseDoc, List<CaseObligor> docObligors, ref string strMesage, List<string> DiffLists, ref List<string> DocMemo)
        {

            string respName = "";
            string respId = "";
            //在此案件中, 只要扣8碼的..不用管負責人, 但要比對戶名一致性
            #region 檢查難字

            bool isDifficult = false;
            foreach (var d in docObligors)
            {
                foreach (var s in DiffLists)
                {
                    if (d.ObligorName.Contains(s))
                        isDifficult = true;
                }
            }
            if (isDifficult) //戶名有*號的, 表示有難字, 落人工處理                    
            {
                strMesage = "0002|有難字";
                return null;
            }
            #endregion

            List<CaseObligor> ValidObligors = new List<CaseObligor>();

            string strType = "D";

            WriteLog(string.Format("\t\tStep 2-1-1: 判斷為 {0}  / Case 3 ", strType == "C" ? "公司" : "行號"));

            // 先找出負責人是否相符, 用公司ID找
            var v = docObligors.Where(x => x.ObligorNo.Length == 8).FirstOrDefault();
            {
                v.type = strType;



                if (strType == "D") // 如果是行號, 要找出負責人
                {
                    #region 如果是行號, 要打67050-6, 找到負責人
                    if (strType == "D" && v.ObligorNo.Length == 8)
                    {
                        var pkid22 = asBiz.addAutoLog(caseDoc.CaseId, eQueryStaff, caseDoc.DocNo, v.ObligorNo, "67050-8");
                        string retMessage22 = "";
                        string respInfo = objSeiHTG.Send67102(v.ObligorNo, v.CaseId, ref retMessage22);
                        if (retMessage22.StartsWith("0000"))
                        {
                            if (!string.IsNullOrEmpty(respInfo))
                            {
                                string[] nameInfo = respInfo.Split('@');
                                respId = nameInfo[0];
                                respName = nameInfo[1];
                            }
                            else
                            {
                                WriteLog("查不到負責人!!");
                                strMesage = "查不到負責人!!";
                            }
                        }
                        else
                        {
                            WriteLog("發查67050-6失敗, 但只有行號, 故直接回文");
                            strMesage = "發查67050-6失敗";
                        }
                    }
                    #endregion

                    WriteLog(string.Format("\t\tStep 2-1-2:  行號 {0} , 負責人 {1} / {2} ", v.ObligorName, respId, respName));

                }



                // 發查60628, 把二者都加入ValidObligors
                Dictionary<string, string> doubleIDsq = new Dictionary<string, string>();
                #region 發查v的重號 60628 , 存到doubleIDs


                var pkidq = asBiz.addAutoLog(caseDoc.CaseId, eQueryStaff, caseDoc.DocNo, v.ObligorNo, "60629");
                string retMessageq = "";
                doubleIDsq = objSeiHTG.Send60629(v.ObligorNo, v.CaseId, ref retMessageq);
                if (doubleIDsq.Count() == 0)
                {
                    doubleIDsq.Add(v.ObligorNo, v.ObligorName);
                }

                if (retMessageq.StartsWith("0000|"))
                    asBiz.updateAutoLog(pkidq, 1, retMessageq);
                else
                {
                    if (retMessageq.EndsWith("電文訊息 :找不到相關資料"))
                    {
                        WriteLog("\t\t **** 發查60629, 找不到相關資料, 表示無存款往來");
                        //20190107, 表示無存款往來, 直接回文
                        asBiz.updateAutoLog(pkidq, 1, retMessageq);
                        return ValidObligors;
                    }
                    else
                    {
                        asBiz.updateAutoLog(pkidq, 2, retMessageq);
                        //0001|電文00450 發查失敗01|交易限制
                        DocMemo.Add("發查60629失敗, 原因 : " + retMessageq);
                        asBiz.insertCaseMemo(caseDoc.CaseId, "CaseSeizure", DocMemo, eQueryStaff);
                        strMesage = "0028|" + retMessageq;
                        var resC1 = addObligor(doubleIDsq, caseDoc, strType, v.ObligorName, v.ObligorNo, ref DocMemo, ref strMesage);
                        ValidObligors.AddRange(resC1);
                        return ValidObligors;
                    }
                }
                WriteLog(string.Format("\t\tStep 2-1-2:  查詢重號結果 {0} / {1}", string.Join(", ", doubleIDsq.Select(x => x.Key)), string.Join(", ", doubleIDsq.Select(x => x.Value))));
                #endregion

                var resCq = addObligor(doubleIDsq, caseDoc, strType, v.ObligorName, v.ObligorNo, ref DocMemo, ref strMesage);

                #region 發查67072
                {
                    var pkid72 = asBiz.addAutoLog(caseDoc.CaseId, eQueryStaff, caseDoc.DocNo, v.ObligorNo, "67072");
                    string retMessage72 = "";
                    var d67072 = objSeiHTG.Send67072(v.ObligorNo, v.CaseId, ref retMessage72);


                    if (d67072.StartsWith("0000|"))
                        asBiz.updateAutoLog(pkid72, 1, retMessage72);
                    else
                    {
                        asBiz.updateAutoLog(pkid72, 2, retMessage72);
                        //0001|電文00450 發查失敗01|交易限制
                        DocMemo.Add("發查67072失敗, 原因 : " + retMessage72);
                        asBiz.insertCaseMemo(caseDoc.CaseId, "CaseSeizure", DocMemo, eQueryStaff);
                        //return "0028|" + retMessage72;
                    }
                }

                #endregion

                #region 若8碼戶名不符的處理

                if (resCq.Any(x => !x.isSame))
                {
                    WriteLog("\t\tStep2-1-1 : 若Case 5的狀況下,   8碼的戶名不符時, 直接落人工");
                    DocMemo.Add("8碼行號的戶名不符，請確認!");

                    strMesage = "0015|8碼行號的戶名不符，請確認!";


                    { // 塞入BatchQuene, 幫忙查帳務資訊
                        insertBatchQuene(caseDoc.CaseId, eQueryStaff, caseDoc.DocNo, v.ObligorNo);
                    }

                    #region 檢查負責人ID是否相同, 若若不同, 也要提示出來
                    if (respId != v.ObligorNo)
                    {
                        DocMemo.Add("經查來函ID與本行留存負責人ID : " + respId + " 不同。");
                    }

                    //ValidObligors.AddRange(resC);
                    #endregion

                    asBiz.insertCaseMemo(caseDoc.CaseId, "CaseSeizure", DocMemo, eQueryStaff);
                    //ValidObligors.AddRange(resP);
                    return null;
                }
                #endregion

                ValidObligors.AddRange(resCq);
            }



            strMesage = "0000|";
            return ValidObligors;
        }


        /// <summary>
        /// 來文10+8碼, 並為公司
        /// </summary>
        /// <param name="caseDoc"></param>
        /// <param name="docObligors"></param>
        /// <param name="strMesage"></param>
        /// <param name="DiffLists"></param>
        /// <param name="DocMemo"></param>
        /// <returns></returns>
        public static List<CaseObligor> Case4(CaseMaster caseDoc, List<CaseObligor> docObligors, ref string strMesage, List<string> DiffLists, ref List<string> DocMemo)
        {



            List<CaseObligor> ValidObligors = new List<CaseObligor>();


            string strType = "C";


            WriteLog(string.Format("\t\tStep 2-1-1: 判斷為 {0} / Case 4 ", strType == "C" ? "公司" : "行號"));

            // 來文的個人
            var p = docObligors.Where(x => x.ObligorNo.Length > 8).FirstOrDefault();
            var v = docObligors.Where(x => x.ObligorNo.Length == 8).FirstOrDefault();

            #region 20181023 判斷難字與否
            {
                bool isDifficult = false;
                foreach (var s in DiffLists)
                {
                    if (p.ObligorName.Contains(s))
                        isDifficult = true;
                }
                if (isDifficult)
                    p.isDiffWord = true;
                else
                    p.isDiffWord = false;
            }
            {
                bool isDifficult = false;
                foreach (var s in DiffLists)
                {
                    if (v.ObligorName.Contains(s))
                        isDifficult = true;
                }
                if (isDifficult)
                    v.isDiffWord = true;
                else
                    v.isDiffWord = false;
            }

            #endregion

            #region 若來文10碼戶名>3, 落人工
            if (p.ObligorName.Length > 3)
            {

                WriteLog("\t\tStep2-1-1 : 若Case 5的狀況下,   來文8+10碼， 但10碼戶名長度>3，落人工");
                DocMemo.Add("10碼戶名長度>3，請確認!");

                strMesage = "0015|10碼戶名長度>3，請確認!";
                insertBatchQuene(caseDoc.CaseId, eQueryStaff, caseDoc.DocNo, v.ObligorNo);
                insertBatchQuene(caseDoc.CaseId, eQueryStaff, caseDoc.DocNo, p.ObligorNo);
                asBiz.insertCaseMemo(caseDoc.CaseId, "CaseSeizure", DocMemo, eQueryStaff);
                return null;
            }



            #endregion


            // 先找出負責人是否相符, 用公司ID找

            {
                v.type = strType;
                {
                    // 發查60628, 把二者都加入ValidObligors

                    Dictionary<string, string> doubleIDs = new Dictionary<string, string>();
                    #region 發查v的重號 60628 , 存到doubleIDs

                    bool hasAccount = true;
                    var pkid = asBiz.addAutoLog(caseDoc.CaseId, eQueryStaff, caseDoc.DocNo, v.ObligorNo, "60629");
                    string retMessage = "";
                    doubleIDs = objSeiHTG.Send60629(v.ObligorNo, v.CaseId, ref retMessage);
                    if (doubleIDs.Count() == 0)
                    {
                        doubleIDs.Add(v.ObligorNo, v.ObligorName);
                    }

                    if (retMessage.StartsWith("0000|"))
                        asBiz.updateAutoLog(pkid, 1, retMessage);
                    else
                    {
                        if (retMessage.EndsWith("電文訊息 :找不到相關資料"))
                        {
                            WriteLog("\t\t **** 發查60629, 找不到相關資料, 表示無存款往來");
                            hasAccount = false;
                        }
                        else
                        {
                            asBiz.updateAutoLog(pkid, 2, retMessage);
                            //0001|電文00450 發查失敗01|交易限制
                            DocMemo.Add("發查60629失敗, 原因 : " + retMessage);
                            asBiz.insertCaseMemo(caseDoc.CaseId, "CaseSeizure", DocMemo, eQueryStaff);
                            strMesage = "0028|" + retMessage;
                            var resC1 = addObligor(doubleIDs, caseDoc, strType, v.ObligorName, v.ObligorNo, ref DocMemo, ref strMesage);
                            ValidObligors.AddRange(resC1);
                            return ValidObligors;
                        }
                    }
                    WriteLog(string.Format("\t\tStep 2-1-2:  查詢重號結果 {0} / {1}", string.Join(", ", doubleIDs.Select(x => x.Key)), string.Join(", ", doubleIDs.Select(x => x.Value))));
                    #endregion




                    #region 發查67072
                    {
                        var pkid72 = asBiz.addAutoLog(caseDoc.CaseId, eQueryStaff, caseDoc.DocNo, v.ObligorNo, "67072");
                        string retMessage72 = "";
                        var d67072 = objSeiHTG.Send67072(v.ObligorNo, v.CaseId, ref retMessage72);


                        if (d67072.StartsWith("0000|"))
                            asBiz.updateAutoLog(pkid72, 1, retMessage72);
                        else
                        {
                            asBiz.updateAutoLog(pkid72, 2, retMessage72);
                            //0001|電文00450 發查失敗01|交易限制
                            DocMemo.Add("發查67072失敗, 原因 : " + retMessage72);
                            asBiz.insertCaseMemo(caseDoc.CaseId, "CaseSeizure", DocMemo, eQueryStaff);
                            //return "0028|" + retMessage72;
                        }
                    }
                    {
                        var pkid72 = asBiz.addAutoLog(caseDoc.CaseId, eQueryStaff, caseDoc.DocNo, p.ObligorNo, "67072");
                        string retMessage72 = "";
                        var d67072 = objSeiHTG.Send67072(p.ObligorNo, v.CaseId, ref retMessage72);


                        if (d67072.StartsWith("0000|"))
                            asBiz.updateAutoLog(pkid72, 1, retMessage72);
                        else
                        {
                            asBiz.updateAutoLog(pkid72, 2, retMessage72);
                            //0001|電文00450 發查失敗01|交易限制
                            DocMemo.Add("發查67072失敗, 原因 : " + retMessage72);
                            asBiz.insertCaseMemo(caseDoc.CaseId, "CaseSeizure", DocMemo, eQueryStaff);
                            //return "0028|" + retMessage72;
                        }
                    }

                    #endregion

                    List<CaseObligor> resC = new List<CaseObligor>();

                    if(  hasAccount)
                         resC = addObligor(doubleIDs, caseDoc, strType, v.ObligorName, v.ObligorNo, ref DocMemo, ref strMesage);

                    //if (doubleIDs.Count() > 1)
                    //{
                    //    foreach (var r in resC)
                    //        r.docMemo = "來文8碼有重號，請確認! ";
                    //}

                    if (strType == "C") // 公司型態, 不管戶名符不符, 都要扣
                    {
                        foreach (var a in resC)
                            a.isSame = true;
                        WriteLog("\t\t公司型態, 不管戶名符不符, 都要扣");
                    }
                    ValidObligors.AddRange(resC);

                    #region 發查p的重號 60628 , 存到doubleIDs


                    var pkidp = asBiz.addAutoLog(caseDoc.CaseId, eQueryStaff, caseDoc.DocNo, p.ObligorNo, "60629");
                    string retMessagep = "";
                    Dictionary<string, string> doubleIDsp = objSeiHTG.Send60629(p.ObligorNo, p.CaseId, ref retMessagep);
                    if (doubleIDsp.Count() == 0)
                    {
                        doubleIDsp.Add(p.ObligorNo, p.ObligorName);
                    }

                    if (retMessagep.StartsWith("0000|"))
                        asBiz.updateAutoLog(pkidp, 1, retMessagep);
                    else
                    {
                        if (retMessagep.EndsWith("電文訊息 :找不到相關資料"))
                        {
                            WriteLog("\t\t **** 發查60629, 找不到相關資料, 表示無存款往來");
                            //20190107, 表示無存款往來, 直接回文
                            asBiz.updateAutoLog(pkid, 1, retMessagep);
                            return ValidObligors;
                        }
                        else
                        {
                            asBiz.updateAutoLog(pkidp, 2, retMessagep);
                            //0001|電文00450 發查失敗01|交易限制
                            DocMemo.Add("發查60629失敗, 原因 : " + retMessage);
                            asBiz.insertCaseMemo(caseDoc.CaseId, "CaseSeizure", DocMemo, eQueryStaff);
                            strMesage = "0028|" + retMessage;
                            var resC1 = addObligor(doubleIDs, caseDoc, strType, v.ObligorName, v.ObligorNo, ref DocMemo, ref strMesage);
                            ValidObligors.AddRange(resC1);
                            return ValidObligors;
                        }
                    }
                    WriteLog(string.Format("\t\tStep 2-1-2:  查詢重號結果 {0} / {1}", string.Join(", ", doubleIDsp.Select(x => x.Key)), string.Join(", ", doubleIDsp.Select(x => x.Value))));

                    #endregion

                    var resP = addObligor(doubleIDsp, caseDoc, strType, p.ObligorName, p.ObligorNo, ref DocMemo, ref strMesage);
                    foreach (var a in resP)
                    {
                        if (!a.isSame)
                        {
                            DocMemo.Add(p.ObligorNo + "!@!" + "戶名不符。");
                        }
                    }
                    ValidObligors.AddRange(resP);
                }
            }
            strMesage = "0000|";
            return ValidObligors;
        }


        public static DictionaryEntry getRespInfo(string ObligorNo, string ObligorName, CaseMaster caseDoc)
        {
            DictionaryEntry d = new DictionaryEntry();
            #region 如果是行號, 要打67050-6, 找到負責人
            //if (ObligorNo.Length == 8)
            {
                var pkid22 = asBiz.addAutoLog(caseDoc.CaseId, eQueryStaff, caseDoc.DocNo, ObligorNo, "67050-8");
                string retMessage22 = "";
                string respInfo = objSeiHTG.Send67102(ObligorNo, caseDoc.CaseId, ref retMessage22);
                if (retMessage22.StartsWith("0000"))
                {
                    if (!string.IsNullOrEmpty(respInfo))
                    {
                        string[] nameInfo = respInfo.Split('@');
                        d.Key = nameInfo[0];
                        d.Value = nameInfo[1];
                    }
                    else
                    {
                        WriteLog("查不到負責人!!");
                    }
                }
                else
                {
                    WriteLog("發查67050-6失敗");
                }
            }
            #endregion
            WriteLog(string.Format("\t\tStep 2-1-2:  行號 {0} , 負責人 {1} / {2} ", ObligorName, d.Key, d.Value));
            return d;
        }




        public static Dictionary<string, string> getDoubleId(CaseMaster caseDoc, string ObligorNo, string ObligorName , ref List<string> DocMemo)
        {
            Dictionary<string, string> doubleIDs2 = new Dictionary<string, string>();
            #region 發查v的重號 60628 , 存到doubleIDs


            var pkid2 = asBiz.addAutoLog(caseDoc.CaseId, eQueryStaff, caseDoc.DocNo, ObligorNo, "60629");
            string retMessage2 = "";
            doubleIDs2 = objSeiHTG.Send60629(ObligorNo, caseDoc.CaseId, ref retMessage2);
            if (doubleIDs2.Count() == 0)
            {
                doubleIDs2.Add(ObligorNo, ObligorName);
            }

            if (retMessage2.StartsWith("0000|"))
                asBiz.updateAutoLog(pkid2, 1, retMessage2);
            else
            {
                asBiz.updateAutoLog(pkid2, 2, retMessage2);
                DocMemo.Add("發查60629失敗, 原因 : " + retMessage2);
                asBiz.insertCaseMemo(caseDoc.CaseId, "CaseSeizure", DocMemo, eQueryStaff);
                //strMesage = "0028|" + retMessage2;
                //var resC1 = addObligor(doubleIDs2, caseDoc, strType, v.ObligorName, v.ObligorNo, ref DocMemo, ref strMesage);
                //ValidObligors.AddRange(resC1);
                //return ValidObligors;
            }
            WriteLog(string.Format("\t\tStep 2-1-2:  查詢重號結果 {0} / {1}", string.Join(", ", doubleIDs2.Select(x => x.Key)), string.Join(", ", doubleIDs2.Select(x => x.Value))));
            #endregion

            return doubleIDs2;
        }



        /// <summary>
        /// 來文10+8碼, 並為行號, 20181016之前版本(備份版本)
        /// 20181016, 決議, 若8碼有重號, 也落人工
        /// </summary>
        /// <param name="caseDoc"></param>
        /// <param name="docObligors"></param>
        /// <param name="strMesage"></param>
        /// <param name="DiffLists"></param>
        /// <param name="DocMemo"></param>
        /// <returns></returns>
        public static List<CaseObligor> Case5(CaseMaster caseDoc, List<CaseObligor> docObligors, ref string strMesage, List<string> DiffLists, ref List<string> DocMemo)
        {


            List<CaseObligor> ValidObligors = new List<CaseObligor>();


            string strType = "D";
            string respName = "";
            string respId = "";

            WriteLog(string.Format("\t\tStep 2-1-1: 判斷為 {0} / Case 5 ", strType == "C" ? "公司" : "行號"));

            // 來文的個人
            var p = docObligors.Where(x => x.ObligorNo.Length > 8).FirstOrDefault();
            // 先找出負責人是否相符, 用公司ID找
            var v = docObligors.Where(x => x.ObligorNo.Length == 8).FirstOrDefault();


            #region 20181023 判斷難字與否
            {
                bool isDifficult = false;
                foreach (var s in DiffLists)
                {
                    if (p.ObligorName.Contains(s))
                        isDifficult = true;
                }
                if (isDifficult)
                    p.isDiffWord = true;
                else
                    p.isDiffWord = false;
            }
            {
                bool isDifficult = false;
                foreach (var s in DiffLists)
                {
                    if (v.ObligorName.Contains(s))
                        isDifficult = true;
                }
                if (isDifficult)
                    v.isDiffWord = true;
                else
                    v.isDiffWord = false;
            }

            #endregion


            {
                v.type = strType;




                if (strType == "D") // 如果是行號, 要找出負責人
                {
                    #region 如果是行號, 要打67050-6, 找到負責人
                    if (strType == "D" && v.ObligorNo.Length == 8)
                    {
                        var pkid22 = asBiz.addAutoLog(caseDoc.CaseId, eQueryStaff, caseDoc.DocNo, v.ObligorNo, "67050-8");
                        string retMessage22 = "";
                        string respInfo = objSeiHTG.Send67102(v.ObligorNo, v.CaseId, ref retMessage22);
                        if (retMessage22.StartsWith("0000"))
                        {
                            if (!string.IsNullOrEmpty(respInfo))
                            {
                                string[] nameInfo = respInfo.Split('@');
                                respId = nameInfo[0];
                                respName = nameInfo[1];
                            }
                            else
                            {
                                WriteLog("查不到負責人!!");
                                strMesage = "查不到負責人!!";
                            }
                        }
                        else
                        {
                            WriteLog("發查67050-6失敗");
                            strMesage = "發查67050-6失敗";
                        }
                    }
                    #endregion
                    WriteLog(string.Format("\t\tStep 2-1-2:  行號 {0} , 負責人 {1} / {2} ", v.ObligorName, respId, respName));
                }



                #region 20181001, 若Case 5的狀況下,   8碼的戶名不符時, 直接落人工
                bool hasRespID = true;

                if (string.IsNullOrEmpty(respId) || string.IsNullOrEmpty(respName)) // 表示本行無任何往來, 就不要查60629
                {
                    hasRespID = false;
                    WriteLog("\t\t********找不到負責人.. 表示此行號無任何往來... 只扣押個人即可");
                }

                if (true)
                {
                    Dictionary<string, string> doubleIDs = new Dictionary<string, string>();

                    if (hasRespID) // 有找到負責人ID
                    {
                        #region 發查v的重號 60628 , 存到doubleIDs


                        var pkid = asBiz.addAutoLog(caseDoc.CaseId, eQueryStaff, caseDoc.DocNo, v.ObligorNo, "60629");
                        string retMessage = "";
                        doubleIDs = objSeiHTG.Send60629(v.ObligorNo, v.CaseId, ref retMessage);
                        if (doubleIDs.Count() == 0)
                        {
                            doubleIDs.Add(v.ObligorNo, v.ObligorName);
                        }

                        if (retMessage.StartsWith("0000|"))
                            asBiz.updateAutoLog(pkid, 1, retMessage);
                        else
                        {
                            asBiz.updateAutoLog(pkid, 2, retMessage);
                            //0001|電文00450 發查失敗01|交易限制
                            DocMemo.Add("發查60629失敗, 原因 : " + retMessage);
                            asBiz.insertCaseMemo(caseDoc.CaseId, "CaseSeizure", DocMemo, eQueryStaff);
                            strMesage = "0028|" + retMessage;
                            var resC1 = addObligor(doubleIDs, caseDoc, strType, v.ObligorName, v.ObligorNo, ref DocMemo, ref strMesage);
                            ValidObligors.AddRange(resC1);
                            return ValidObligors;
                        }
                        WriteLog(string.Format("\t\tStep 2-1-2:  查詢重號結果 {0} / {1}", string.Join(", ", doubleIDs.Select(x => x.Key)), string.Join(", ", doubleIDs.Select(x => x.Value))));
                        #endregion
                    }


                    #region 發查10碼p的重號 60628 , 存到doubleIDsp


                    var pkidp = asBiz.addAutoLog(caseDoc.CaseId, eQueryStaff, caseDoc.DocNo, p.ObligorNo, "60629");
                    string retMessagep = "";
                    Dictionary<string, string> doubleIDsp = objSeiHTG.Send60629(p.ObligorNo, p.CaseId, ref retMessagep);
                    if (doubleIDsp.Count() == 0)
                    {
                        doubleIDsp.Add(p.ObligorNo, p.ObligorName);
                    }



                    if (retMessagep.StartsWith("0000|"))
                        asBiz.updateAutoLog(pkidp, 1, retMessagep);
                    else
                    {
                        // 20190107
                        if (retMessagep.EndsWith("電文訊息 :找不到相關資料"))
                        {
                            WriteLog("\t\t **** 發查60629, 找不到相關資料, 表示無存款往來");
                            //20190107, 表示無存款往來, 直接回文
                            asBiz.updateAutoLog(pkidp, 1, retMessagep);
                            return ValidObligors;
                        }
                        else
                        {
                            asBiz.updateAutoLog(pkidp, 2, retMessagep);
                            //0001|電文00450 發查失敗01|交易限制
                            DocMemo.Add("發查60629失敗, 原因 : " + retMessagep);
                            asBiz.insertCaseMemo(caseDoc.CaseId, "CaseSeizure", DocMemo, eQueryStaff);
                            strMesage = "0028|" + retMessagep;
                            var resC1 = addObligor(doubleIDsp, caseDoc, strType, v.ObligorName, v.ObligorNo, ref DocMemo, ref strMesage);
                            ValidObligors.AddRange(resC1);
                            return ValidObligors;
                        }
                    }
                    WriteLog(string.Format("\t\tStep 2-1-2:  查詢重號結果 {0} / {1}", string.Join(", ", doubleIDsp.Select(x => x.Key)), string.Join(", ", doubleIDsp.Select(x => x.Value))));


                    #endregion


                    #region 發查67072
                    {
                        var pkid72 = asBiz.addAutoLog(caseDoc.CaseId, eQueryStaff, caseDoc.DocNo, v.ObligorNo, "67072");
                        string retMessage72 = "";
                        var d67072 = objSeiHTG.Send67072(v.ObligorNo, v.CaseId, ref retMessage72);


                        if (d67072.StartsWith("0000|"))
                            asBiz.updateAutoLog(pkid72, 1, retMessage72);
                        else
                        {
                            asBiz.updateAutoLog(pkid72, 2, retMessage72);
                            //0001|電文00450 發查失敗01|交易限制
                            DocMemo.Add("發查67072失敗, 原因 : " + retMessage72);
                            asBiz.insertCaseMemo(caseDoc.CaseId, "CaseSeizure", DocMemo, eQueryStaff);
                            //return "0028|" + retMessage72;
                        }
                    }
                    {
                        var pkid72 = asBiz.addAutoLog(caseDoc.CaseId, eQueryStaff, caseDoc.DocNo, p.ObligorNo, "67072");
                        string retMessage72 = "";
                        var d67072 = objSeiHTG.Send67072(p.ObligorNo, v.CaseId, ref retMessage72);


                        if (d67072.StartsWith("0000|"))
                            asBiz.updateAutoLog(pkid72, 1, retMessage72);
                        else
                        {
                            asBiz.updateAutoLog(pkid72, 2, retMessage72);
                            //0001|電文00450 發查失敗01|交易限制
                            DocMemo.Add("發查67072失敗, 原因 : " + retMessage72);
                            asBiz.insertCaseMemo(caseDoc.CaseId, "CaseSeizure", DocMemo, eQueryStaff);
                            //return "0028|" + retMessage72;
                        }
                    }

                    #endregion


                    bool is8double = false;
                    bool is10double = false;

                    #region 20181016, 決議, 若8碼有重號, 也落人工,,, 10碼有重號, 且戶名不符, 才落下
                    if (doubleIDs.Count() > 1)
                    {
                        DocMemo.Add("來文8碼有重號，請確認! ");
                        is8double = true;
                    }



                    if(doubleIDsp.Count()>1) // 10碼有重號, 且戶名不符, 才落下
                    {
                        //DocMemo.Add("來文10碼有重號，請確認! ");
                        is10double = true;
                    }

                    if (is8double) // 20181029,  有8碼重號, 落人工 ,    10碼有重號, 且戶名不符, 才落下
                    {
                        asBiz.insertCaseMemo(caseDoc.CaseId, "CaseSeizure", DocMemo, eQueryStaff);
                        foreach (var d in doubleIDs)
                        {
                            insertBatchQuene(caseDoc.CaseId, eQueryStaff, caseDoc.DocNo, d.Key);
                        }
                        //再查出個人, 然後去查帳戶
                        foreach (var d in doubleIDsp)
                        {
                            insertBatchQuene(caseDoc.CaseId, eQueryStaff, caseDoc.DocNo, d.Key);
                        }
                        strMesage = "0029|來文8碼有重號，請確認";
                        return null;
                    }





                    #endregion


                    var resC = addObligor(doubleIDs, caseDoc, strType, v.ObligorName, v.ObligorNo, ref DocMemo, ref strMesage);

                    #region 若8碼戶名不符的處理
                    
                    if (resC.Any(x => !x.isSame))                    
                    {
                        WriteLog("\t\tStep2-1-1 : 若Case 5的狀況下,   8碼的戶名不符時, 直接落人工");
                        DocMemo.Add("8碼行號的戶名不符，請確認!");

                        strMesage = "0015|8碼行號的戶名不符，請確認!";


                        { // 塞入BatchQuene, 幫忙查帳務資訊
                            insertBatchQuene(caseDoc.CaseId, eQueryStaff, caseDoc.DocNo, v.ObligorNo);
                            insertBatchQuene(caseDoc.CaseId, eQueryStaff, caseDoc.DocNo, p.ObligorNo);
                        }

                        #region 檢查負責人ID是否相同, 若若不同, 也要提示出來
                        if (respId != p.ObligorNo)
                        {
                            DocMemo.Add("經查來函ID與本行留存負責人ID : " + respId + " 不同。");
                        }

                        //ValidObligors.AddRange(resC);
                        #endregion



                        // 比對10碼主機戶名與
                        var resP = addObligor(doubleIDsp, caseDoc, strType, p.ObligorName, p.ObligorNo, ref DocMemo, ref strMesage);
                        if (resP.Any(x => !x.isSame)) // 20181003 目前尚未考慮,10碼的有重號
                        {
                            DocMemo.Add("10碼行號的戶名不符，請確認!");
                        }

                        asBiz.insertCaseMemo(caseDoc.CaseId, "CaseSeizure", DocMemo, eQueryStaff);
                        //ValidObligors.AddRange(resP);
                        return null;
                    }
                    #endregion
                }
                #endregion


                #region 若來文10碼戶名>3, 落人工
                if (p.ObligorName.Length > 3)
                {

                    WriteLog("\t\tStep2-1-1 : 若Case 5的狀況下,   來文8+10碼， 但10碼戶名長度>3，落人工");
                    DocMemo.Add("10碼戶名長度>3，請確認!");

                    strMesage = "0015|10碼戶名長度>3，請確認!";
                    insertBatchQuene(caseDoc.CaseId, eQueryStaff, caseDoc.DocNo, v.ObligorNo);
                    insertBatchQuene(caseDoc.CaseId, eQueryStaff, caseDoc.DocNo, p.ObligorNo);
                    asBiz.insertCaseMemo(caseDoc.CaseId, "CaseSeizure", DocMemo, eQueryStaff);
                    return null;
                }
                #endregion

                if (!string.IsNullOrEmpty(respId))
                {
                    if (respId == p.ObligorNo) // 表示來文ID, 與負責人ID相同
                    {

                        #region 表示來文ID, 與負責人ID相同

                        Dictionary<string, string> doubleIDs = new Dictionary<string, string>();
                        Dictionary<string, string> doubleIDsp = new Dictionary<string, string>();
                        string retMessage = "";
                        #region 發查p的重號 60628 , 存到doubleIDsp


                        var pkidp = asBiz.addAutoLog(caseDoc.CaseId, eQueryStaff, caseDoc.DocNo, p.ObligorNo, "60629");
                        string retMessagep = "";
                        doubleIDsp = objSeiHTG.Send60629(p.ObligorNo, p.CaseId, ref retMessagep);
                        if (doubleIDsp.Count() == 0)
                        {
                            doubleIDsp.Add(p.ObligorNo, p.ObligorName);
                        }

                        if (retMessagep.StartsWith("0000|"))
                            asBiz.updateAutoLog(pkidp, 1, retMessagep);
                        else
                        {
                            asBiz.updateAutoLog(pkidp, 2, retMessagep);
                            //0001|電文00450 發查失敗01|交易限制
                            DocMemo.Add("發查60629失敗, 原因 : " + retMessage);
                            asBiz.insertCaseMemo(caseDoc.CaseId, "CaseSeizure", DocMemo, eQueryStaff);
                            strMesage = "0028|" + retMessage;
                            var resC1 = addObligor(doubleIDs, caseDoc, strType, v.ObligorName, v.ObligorNo, ref DocMemo, ref strMesage);
                            ValidObligors.AddRange(resC1);
                            return ValidObligors;
                        }
                        WriteLog(string.Format("\t\tStep 2-1-2:  查詢重號結果 {0} / {1}", string.Join(", ", doubleIDsp.Select(x => x.Key)), string.Join(", ", doubleIDsp.Select(x => x.Value))));

                        //if (doubleIDsp.Count() > 1)
                        //{
                        //    DocMemo.Add("來文10碼有重號，請確認! ");
                        //}


                        #endregion
                        #region 發查v的重號 60628 , 存到doubleIDs


                        var pkid = asBiz.addAutoLog(caseDoc.CaseId, eQueryStaff, caseDoc.DocNo, v.ObligorNo, "60629");

                        doubleIDs = objSeiHTG.Send60629(v.ObligorNo, v.CaseId, ref retMessage);
                        if (doubleIDs.Count() == 0)
                        {
                            doubleIDs.Add(v.ObligorNo, v.ObligorName);
                        }

                        if (retMessage.StartsWith("0000|"))
                            asBiz.updateAutoLog(pkid, 1, retMessage);
                        else
                        {
                            asBiz.updateAutoLog(pkid, 2, retMessage);
                            //0001|電文00450 發查失敗01|交易限制
                            DocMemo.Add("發查60629失敗, 原因 : " + retMessage);
                            asBiz.insertCaseMemo(caseDoc.CaseId, "CaseSeizure", DocMemo, eQueryStaff);
                            strMesage = "0028|" + retMessage;
                            var resC1 = addObligor(doubleIDs, caseDoc, strType, v.ObligorName, v.ObligorNo, ref DocMemo, ref strMesage);
                            ValidObligors.AddRange(resC1);
                            return ValidObligors;
                        }
                        WriteLog(string.Format("\t\tStep 2-1-2:  查詢重號結果 {0} / {1}", string.Join(", ", doubleIDs.Select(x => x.Key)), string.Join(", ", doubleIDs.Select(x => x.Value))));
                        #endregion

                        string resNewName = p.ObligorName;
                        var resP = addObligor(doubleIDsp, caseDoc, strType, p.ObligorName, p.ObligorNo, ref DocMemo, ref strMesage);
                        if (resP.Any(x => x.isChangeName && x.isSame))
                        {
                            resNewName = resP.Where(x => x.isChangeName && x.isSame).First().newName;
                        }
                        // 發查60628, 把二者都加入ValidObligors




                        var resC = addObligor(doubleIDs, caseDoc, strType, v.ObligorName, v.ObligorNo, ref DocMemo, ref strMesage);
                        // 20181004, 檢查負責人姓名與10碼是否相同, 若不同resC要回負責人不符
                        bool isResSame = false;
                        foreach (var r in resC)
                        {
                            if (resNewName != respName)
                            {
                                isResSame = true;
                                // 查有沒有存款往來
                                DocMemo.Add(v.ObligorNo + "!@!" + "經查於本行負責人戶名不符，故無執行扣押。");
                                r.isSame = false;
                            }
                        }



                        if (resP.Any(x => !x.isSame)) // 20181003 目前尚未考慮,10碼的有重號
                        {
                            DocMemo.Add(p.ObligorNo + "!@!" + "戶名不符。");
                        }
                        ValidObligors.AddRange(resC);
                        ValidObligors.AddRange(resP);


                        #region 20181029, 10碼有重號, 且戶名不符, 才要落人工
                        if (doubleIDsp.Count() > 1 && resP.Any(x => !x.isSame))
                        {
                            DocMemo.Add("來文10碼有重號且戶名不符，請確認。");
                            asBiz.insertCaseMemo(caseDoc.CaseId, "CaseSeizure", DocMemo, eQueryStaff);
                            foreach (var d in doubleIDs)
                            {
                                insertBatchQuene(caseDoc.CaseId, eQueryStaff, caseDoc.DocNo, d.Key);
                            }
                            //再查出個人, 然後去查帳戶
                            foreach (var d in doubleIDsp)
                            {
                                insertBatchQuene(caseDoc.CaseId, eQueryStaff, caseDoc.DocNo, d.Key);
                            }
                            strMesage = "0029|來文10碼有重號且戶名不符落人工，請確認";
                            return null;

                        }
                        #endregion


                        #endregion
                    }
                    else // 表示來文ID, 與負責人ID不相同
                    {
                        #region  表示來文ID, 與負責人ID不相同
                        Dictionary<string, string> doubleIDs = new Dictionary<string, string>();
                        Dictionary<string, string> doubleIDsp = new Dictionary<string, string>();
                        string retMessage = "";

                        #region 發查v的重號 60628 , 存到doubleIDs


                        var pkid = asBiz.addAutoLog(caseDoc.CaseId, eQueryStaff, caseDoc.DocNo, v.ObligorNo, "60629");

                        doubleIDs = objSeiHTG.Send60629(v.ObligorNo, v.CaseId, ref retMessage);
                        if (doubleIDs.Count() == 0)
                        {
                            doubleIDs.Add(v.ObligorNo, v.ObligorName);
                        }

                        if (retMessage.StartsWith("0000|"))
                            asBiz.updateAutoLog(pkid, 1, retMessage);
                        else
                        {
                            asBiz.updateAutoLog(pkid, 2, retMessage);
                            //0001|電文00450 發查失敗01|交易限制
                            DocMemo.Add("發查60629失敗, 原因 : " + retMessage);
                            asBiz.insertCaseMemo(caseDoc.CaseId, "CaseSeizure", DocMemo, eQueryStaff);
                            strMesage = "0028|" + retMessage;
                            var resC1 = addObligor(doubleIDs, caseDoc, strType, v.ObligorName, v.ObligorNo, ref DocMemo, ref strMesage);
                            ValidObligors.AddRange(resC1);
                            return ValidObligors;
                        }
                        WriteLog(string.Format("\t\tStep 2-1-2:  查詢重號結果 {0} / {1}", string.Join(", ", doubleIDs.Select(x => x.Key)), string.Join(", ", doubleIDs.Select(x => x.Value))));
                        #endregion

                        var resC = addObligor(doubleIDs, caseDoc, strType, v.ObligorName, v.ObligorNo, ref DocMemo, ref strMesage);
                        ValidObligors.AddRange(resC);

                        #region 發查10碼p的重號 60628 , 存到doubleIDs


                        var pkidp = asBiz.addAutoLog(caseDoc.CaseId, eQueryStaff, caseDoc.DocNo, p.ObligorNo, "60629");
                        string retMessagep = "";
                        doubleIDsp = objSeiHTG.Send60629(p.ObligorNo, p.CaseId, ref retMessagep);
                        if (doubleIDsp.Count() == 0)
                        {
                            doubleIDsp.Add(p.ObligorNo, p.ObligorName);
                        }

                        if (retMessagep.StartsWith("0000|"))
                            asBiz.updateAutoLog(pkidp, 1, retMessagep);
                        else
                        {
                            asBiz.updateAutoLog(pkidp, 2, retMessagep);
                            //0001|電文00450 發查失敗01|交易限制
                            DocMemo.Add("發查60629失敗, 原因 : " + retMessagep);
                            asBiz.insertCaseMemo(caseDoc.CaseId, "CaseSeizure", DocMemo, eQueryStaff);
                            strMesage = "0028|" + retMessagep;
                            var resC1 = addObligor(doubleIDsp, caseDoc, strType, v.ObligorName, v.ObligorNo, ref DocMemo, ref strMesage);
                            ValidObligors.AddRange(resC1);
                            return ValidObligors;
                        }
                        WriteLog(string.Format("\t\tStep 2-1-2:  查詢重號結果 {0} / {1}", string.Join(", ", doubleIDsp.Select(x => x.Key)), string.Join(", ", doubleIDsp.Select(x => x.Value))));



                        #endregion

                        // 比對10碼主機戶名與
                        var resP = addObligor(doubleIDsp, caseDoc, strType, p.ObligorName, p.ObligorNo, ref DocMemo, ref strMesage);
                        if (resP.Any(x => !x.isSame)) // 20181003 目前尚未考慮,10碼的有重號
                        {
                            DocMemo.Add(p.ObligorNo + "!@!" + "戶名不符。");
                        }

                        DocMemo.Add(v.ObligorNo + "!@!" + "經查於本行負責人不同，故無執行扣押。");

                        ValidObligors.AddRange(resP);



                        #region 20181029, 10碼有重號, 且戶名不符, 才要落人工
                        if (doubleIDsp.Count() > 1 && resP.Any(x => !x.isSame))
                        {
                            DocMemo.Add("來文10碼有重號且戶名不符，請確認。");
                            asBiz.insertCaseMemo(caseDoc.CaseId, "CaseSeizure", DocMemo, eQueryStaff);
                            foreach (var d in doubleIDs)
                            {
                                insertBatchQuene(caseDoc.CaseId, eQueryStaff, caseDoc.DocNo, d.Key);
                            }
                            //再查出個人, 然後去查帳戶
                            foreach (var d in doubleIDsp)
                            {
                                insertBatchQuene(caseDoc.CaseId, eQueryStaff, caseDoc.DocNo, d.Key);
                            }
                            strMesage = "0029|來文10碼有重號且戶名不符落人工，請確認";
                            return null;

                        }
                        #endregion



                        #endregion
                    }
                }
                else
                {
                    #region 若找不到負責人ID, 則只加入10碼個人

                    Dictionary<string, string> doubleIDsp = new Dictionary<string, string>();
                    string retMessage = "";
                    #region 發查p的重號 60628 , 存到doubleIDsp


                    var pkidp = asBiz.addAutoLog(caseDoc.CaseId, eQueryStaff, caseDoc.DocNo, p.ObligorNo, "60629");
                    string retMessagep = "";
                    doubleIDsp = objSeiHTG.Send60629(p.ObligorNo, p.CaseId, ref retMessagep);
                    if (doubleIDsp.Count() == 0)
                    {
                        doubleIDsp.Add(p.ObligorNo, p.ObligorName);
                    }

                    if (retMessagep.StartsWith("0000|"))
                        asBiz.updateAutoLog(pkidp, 1, retMessagep);
                    else
                    {
                        asBiz.updateAutoLog(pkidp, 2, retMessagep);
                        //0001|電文00450 發查失敗01|交易限制
                        DocMemo.Add("發查60629失敗, 原因 : " + retMessage);
                        asBiz.insertCaseMemo(caseDoc.CaseId, "CaseSeizure", DocMemo, eQueryStaff);
                        strMesage = "0028|" + retMessage;
                        return ValidObligors;
                    }
                    WriteLog(string.Format("\t\tStep 2-1-2:  查詢重號結果 {0} / {1}", string.Join(", ", doubleIDsp.Select(x => x.Key)), string.Join(", ", doubleIDsp.Select(x => x.Value))));

                    //if (doubleIDsp.Count() > 1)
                    //{
                    //    DocMemo.Add("來文10碼有重號，請確認! ");
                    //}


                    #endregion


                    var resP = addObligor(doubleIDsp, caseDoc, strType, p.ObligorName, p.ObligorNo, ref DocMemo, ref strMesage);
                    ValidObligors.AddRange(resP);


                    #endregion
                }



            }


            foreach (var v1 in ValidObligors)
            {
                v1.RespId = respId;
                v1.RespName = respName;
            }


            if (string.IsNullOrEmpty(respId) || string.IsNullOrEmpty(respName)) // 表示本行無任何往來, 所以直接
            {
                ValidObligors = ValidObligors.Where(x => x.ObligorNo.Length != 8).ToList();
            }


            strMesage = "0000|";
            return ValidObligors;
        }


        public static List<CaseObligor> addObligor(Dictionary<string, string> doubleIDs, CaseMaster caseDoc, string strType, string ObligorName, string ObligorNo, ref List<string> DocMemo, ref string strMesage)
        {
            List<CaseObligor> ValidObligors = new List<CaseObligor>();
            #region 檢查重號, 是否是合於扣押的義務人, 若合於, 則加入ValidObligors中
            int i = 1;
            foreach (var d in doubleIDs)
            {
                bool isDoubleAcc = i == 1 ? false : true;
                if (ObligorName.Trim().Equals(d.Value.Trim())) // 姓名相符, 加入ValidObligors
                {
                    #region Insert VaildObligor
                    CaseObligor c = new CaseObligor();
                    c.ObligorNo = d.Key;
                    c.CaseId = caseDoc.CaseId;
                    c.CreatedDate = DateTime.Now;
                    c.ObligorName = d.Value;
                    c.isChangeName = false; // 沒有改名
                    c.isDoubleAcc = isDoubleAcc;
                    c.isSame = true; // 原來名字
                    c.newName = "";
                    c.type = strType;
                    ValidObligors.Add(c);
                    #endregion
                    WriteLog(string.Format("\t\t\tStep 2-1-2-2: 戶名相符 來文戶名: {0} = 主機戶名:{1}", ObligorName, d.Value));
                }
                else // 姓名不相符, 去發查60600, 找出是否曾經相符過(過去有沒有改名字過)
                {
                    WriteLog(string.Format("\t\t\tStep 2-1-2-2: 戶名不相符 來文戶名: {0} / 主機戶名:{1}", ObligorName, d.Value));
                    #region 打60600, 確認是否有更名, 若是公司戶, 不用管名字對不對, 若是行號, 才要管
                    List<string> anoNames = new List<string>();

                    var pkid222 = asBiz.addAutoLog(caseDoc.CaseId, eQueryStaff, caseDoc.DocNo, ObligorNo, "60600");
                    string retMessage222 = "";
                    anoNames = objSeiHTG.Send60600(d.Key, caseDoc.CaseId, ref retMessage222);
                    if (retMessage222.StartsWith("0000|"))
                        asBiz.updateAutoLog(pkid222, 1, retMessage222);
                    else
                    {
                        asBiz.updateAutoLog(pkid222, 2, retMessage222);
                        //asBiz.updateAutoLog(pkid, 2, retMessage);
                        DocMemo.Add("發查60600失敗, 原因 : " + retMessage222);
                        asBiz.insertCaseMemo(caseDoc.CaseId, "CaseSeizure", DocMemo, eQueryStaff);
                        strMesage = "0028|" + retMessage222;
                        return null;
                    }

                    #endregion

                    List<CaseObligor> vObligors = changeName(caseDoc, anoNames, ObligorName, ObligorNo, ref DocMemo, strType, isDoubleAcc, d.Value, d.Key);
                    var aaa = DocMemo;
                    ValidObligors.AddRange(vObligors);
                }
                i++;
            }


            #endregion
            return ValidObligors;
        }




        /// <summary>
        /// 找出可能的義務人
        /// </summary>
        /// <param name="anoNames"></param>
        /// <param name="docName">來文姓名</param>
        /// <param name="docId">來文ID</param>
        /// <param name="oldName"></param>
        /// <param name="newName"></param>
        /// <returns></returns>
        private static List<CaseObligor> changeName(CaseMaster caseDoc, List<string> anoNames, string docName, string docId, ref List<string> DocMemo, string strType, bool isDoubleAcc, string accName=null, string accId=null)
        {
            string oldName="";
            string newName = "";
            bool isFoundChaneName = false;
            List<CaseObligor> VaildObligors = new List<CaseObligor>();
            #region 找60600, 若有的話isSameName = true
            if (anoNames.Count() > 0)
            {
                foreach (var ano in anoNames)
                {
                    WriteLog(string.Format("\t\t\t\tStep 2-1-2-2-1: 查到60600的資料 {0}", ano));

                    // 20180911, 先要抓出舊名字跟新名字

                    int iPos = ano.IndexOf("CHANGE");
                    if (iPos > 0)
                    {
                        oldName = ano.Substring(0, iPos).Trim();
                        newName = ano.Substring(iPos + 6).Trim();

                        // 要分舊名字跟新名字.. 若是來文舊名字, 扣押到新名字, 則要加備註, "本行戶名已更名為XXX。"

                        if (docName == oldName)
                        {

                            isFoundChaneName = true;
                            docName = newName;
                        }
                        else
                        {
                            #region Insert VaildObligor
                            CaseObligor c = new CaseObligor();
                            c.ObligorNo = docId;
                            c.CaseId = caseDoc.CaseId;
                            c.CreatedDate = DateTime.Now;
                            c.ObligorName = docName;
                            c.newName = "";
                            c.isChangeName = true; // 有改過名
                            c.isSame = false; // 更名後, 正確
                            c.type = strType;
                            c.isDoubleAcc = isDoubleAcc;
                            // 20181003, 沒有比對到, 不應該加入
                            //VaildObligors.Add(c);
                            WriteLog(string.Format("\t\t\t\t2-1-2-2-1 ===> 沒有新名字"));
                            #endregion
                        }
                        

                    }

                } // end for

                if(isFoundChaneName)
                {
                    #region Insert VaildObligor
                    CaseObligor c = new CaseObligor();
                    c.ObligorNo = docId;
                    c.CaseId = caseDoc.CaseId;
                    c.CreatedDate = DateTime.Now;
                    c.ObligorName = docName;
                    c.newName = newName;
                    c.isChangeName = true; // 有改過名
                    c.isSame = true; // 更名後, 正確
                    c.isDoubleAcc = isDoubleAcc;
                    c.type = strType;
                    
                    #endregion
                    WriteLog(string.Format("\t\t\tStep 2-1-2-4: 查到新名字 {0}", newName));
                    //if (strType == "D")
                        DocMemo.Add(docId + "!@!" + "本行戶名已更名為" + newName + "。");
                    //else
                    //{
                    //    DocMemo.Add("本行戶名已更名為" + newName + "。");
                    //}

                    VaildObligors.Add(c);
                }
                else
                {
                    #region Insert VaildObligor
                    CaseObligor c = new CaseObligor();
                    c.ObligorNo = docId;
                    c.CaseId = caseDoc.CaseId;
                    c.CreatedDate = DateTime.Now;
                    c.ObligorName = docName;
                    c.newName = "";
                    c.isChangeName = true; // 有改過名
                    c.isSame = false; // 更名後, 正確
                    c.type = strType;
                    c.isDoubleAcc = isDoubleAcc;
                   // c.docMemo = "本行戶名為" + accName;
                    // 20181003, 沒有比對到, 不應該加入
                    VaildObligors.Add(c);
                    //WriteLog(string.Format("\tStep2-4 : 查到來文名字 {0} = 新名字 (無)", docName));
                    WriteLog(string.Format("\t\t\t\t2-1-2-2-1 ===> 沒有新名字, 因為是公司, 所以直接告知本行戶名"));
                    #endregion
                }



            }
            else // 若沒有更名, 還是要查他的餘額
            {
                #region Insert VaildObligor
                CaseObligor c = new CaseObligor();
                c.ObligorNo = accId;
                c.CaseId = caseDoc.CaseId;
                c.CreatedDate = DateTime.Now;
                c.ObligorName = accName;
                c.newName = "";
                c.isChangeName = false; // 有改過名
                c.isSame = false; // 更名後, 正確
                c.isDoubleAcc = isDoubleAcc;
                c.type = strType;
                //c.docMemo = "本行戶名為" + accName;
                VaildObligors.Add(c);

                #endregion
            }
            #endregion

            if( ! isFoundChaneName && strType=="C") // 如果都沒有更名的話
            {
                VaildObligors = new List<CaseObligor>();
                #region Insert VaildObligor
                CaseObligor c = new CaseObligor();
                if (isDoubleAcc)
                    c.ObligorNo = accId;
                else
                    c.ObligorNo = docId;
                c.CaseId = caseDoc.CaseId;
                c.CreatedDate = DateTime.Now;
                c.ObligorName = docName;
                c.newName = "";
                c.isChangeName = false; // 有改過名
                c.isSame = false; // 更名後, 正確
                c.isDoubleAcc = isDoubleAcc;
                c.type = strType;
                // 若是公司戶, 且更名都不對時, 要提供主機的名字
                {
                    if( ! isDoubleAcc)
                        c.docMemo = "本行戶名為" + accName;
                }

                //DocMemo.Add("本行戶名為" + accName);

                VaildObligors.Add(c);

                #endregion


            }

            return VaildObligors;
        }


        private static string doSeizureByCaseId(CaseMaster caseDoc)
        {

            WriteLog(string.Format("\r\n\r\n@@@@@@@@@@@@開始扣押:{0}", caseDoc.CaseNo));
            #region 案件層級的私用變數

            PARMCodeBIZ pbiz = new PARMCodeBIZ();

            // 外來文, 加註的所有備註
            List<string> DocMemo = new List<string>();

            // 戶名不符, 仍需要查60491, 33401, 取得餘額, 才能決定發文內容
            //bool AccountNameNOTConsistent = false;


            // Case 16, 18 , 若原號的帳戶不足額, 但重號的名稱與來不不一樣.. 不能扣, 只能輸出 依足額與否, 輸出
            // 另查，該身分證統一編號於本行尚有另一帳戶開立之戶名與來函戶名不符。
            //另查，該身分證統一編號於本行尚有另一帳戶開立之戶名與來函戶名不符且存款餘額未達新臺幣200元。
            //bool doubleIDNameNotConsistent = false;
            bool isRename = false;
            bool isAmountBelow450 = false;
            string doubleID = "";

            // 本案件, 是否要落人工
            bool Doc2Human = false;

            // 本案件，執行的結果
            var SeizureResult = "0000|扣押成功";

            // 本案件的CaseID
            Guid caseid = caseDoc.CaseId;

            //本案件, 準備發文的備註 .. 從來文字號, 取出 前三個字+ 最後6個字
            //var govNo = caseDoc.GovNo.Substring(0, 3) + caseDoc.GovNo.Substring(caseDoc.GovNo.Length - 7, 6);
            var govNo = getNewMemo(caseDoc.GovUnit, caseDoc.GovNo);

            // 本案件, 合法的ID
            List<CaseObligor> VaildObligors = new List<CaseObligor>();

            // 本案件, 因為以上的ID, 所帶出來的帳戶
            List<ObligorAccount> AccLists = new List<ObligorAccount>();

            // 本案件, 執行單位
            string GovUnit = "執行署";
            #region 執行單位
            if (caseDoc.GovUnit.IndexOf("執行署") < 0)
                GovUnit = "法院";
            #endregion

            // 本案件, 取得手續費法院1250, 執行署450... 目前沒有法院的電子來文....
            int HandleFee = 450;
            #region 決定手續費 ...
            //var lstFee = pbiz.GetParmCodeByCodeType("MinAmountReq").FirstOrDefault();

            //CSFS.Models.PARMCode oFee = new CSFS.Models.PARMCode();
            //if (GovUnit.Equals("執行署"))
            //    oFee = lstFee.Where(x => x.CodeNo == "pDocExeOffice").FirstOrDefault();
            //else
            //    oFee = lstFee.Where(x => x.CodeNo == "pDocCourtReq").FirstOrDefault();

            //if (oFee != null)
            //    HandleFee = int.Parse(oFee.CodeDesc);           

            #endregion





            // 本案件, 來文的扣押總金額
            decimal SeizureTotal = 0;
            SeizureTotal = asBiz.getEDocTotal(caseDoc.CaseId);
            #region 取得來文扣押金額

            string eDocTxtMemo = asBiz.geteDocTxtMemo(caseDoc.CaseId);

            #endregion


            #region 檢查來文的金額
            // 若有來文, 有小數點, 則落人工
            int iSeizureTotal = (int)SeizureTotal;
            if (SeizureTotal != (decimal)iSeizureTotal)
            {
                //SeizureResult = "0030|來文扣押金額有小數點，落人工";
                //DocMemo.Add("來函扣押金額異常(請查明)。");
                //asBiz.insertCaseMemo(caseid, "CaseSeizure", DocMemo, eQueryStaff);
                isAmountBelow450 = true;
                //return SeizureResult; // 不可直接回文, 要查完帳戶後, 才能回文
            }

            // 若有來文, 有負數, 則落人工            
            if (SeizureTotal < 0)
            {
                //SeizureResult = "0031|來文扣押金額為負數，落人工";
                //DocMemo.Add("來函扣押金額異常(請查明)。");
                //asBiz.insertCaseMemo(caseid, "CaseSeizure", DocMemo, eQueryStaff);
                isAmountBelow450 = true;
                //return SeizureResult; // 不可直接回文, 要查完帳戶後, 才能回文
            }


            // 若有來文, 有負數, 則落人工            
            if (SeizureTotal < HandleFee)
            {
                //SeizureResult = "0032|來文扣押金額低於" + HandleFee.ToString() + "元，落人工";
                //DocMemo.Add("來函扣押金額異常(請查明)。");
                //asBiz.insertCaseMemo(caseid, "CaseSeizure", DocMemo, eQueryStaff);
                isAmountBelow450 = true;
                //return SeizureResult; // 不可直接回文, 要查完帳戶後, 才能回文
            }

            WriteLog(string.Format("來文扣押的總金額: {0}", SeizureTotal.ToString()));

            #endregion


            //  此人的加總的可用餘額, 必須折算回台幣
            decimal TotalTrueAmt = 0.0m;

            // 所有的訊息
            List<string> SeizureMessage = new List<string>();


            // 讀取參數檔中的優先順序..
            // SELECT *   FROM [CSFS_SIT].[dbo].[PARMCode] where codetype='SeizureSeqence'
            var seizureSequenceAll = pbiz.GetParmCodeByCodeType("SeizureSeqence");
            Dictionary<string, string> BranchInfo = pbiz.GetParmCodeByCodeType("RCAF_BRANCH").ToDictionary(x => x.CodeNo, x => x.CodeDesc);
            // 5.1 正向表列, 可能被扣押的帳戶類型
            Dictionary<string, List<string>> SeizureOrder = getSeizureOrder(seizureSequenceAll);


            // 5.2 負向表列, 絕對不可被扣押的帳戶類型  2018/05/09  包括DCI, SI 理財定存, 產品代碼
            // 0080, 表示台幣  // XX80, 非00, 表示外幣, 
            Dictionary<string, List<string>> SeizureNOTOrder = getSeizureNOTOrder(seizureSequenceAll);
            Dictionary<string, string> SeizureOrderTable = getReverseSeizureOrder(seizureSequenceAll);

            #endregion

            #region 開始把CaseMaster的Status 改成C02
            asBiz.updateCaseMasterStatus(caseid, "C02");
            #endregion

            # region 若來文有備註文字, 人工
            if (!string.IsNullOrEmpty(eDocTxtMemo))
            {
                SeizureResult = "0019|來文有備註文字，落人工";
                DocMemo.Add("來文有備註文字。");
                asBiz.insertCaseMemo(caseid, "CaseSeizure", DocMemo, eQueryStaff);
                //insertBatchQuene(caseDoc.CaseId, eQueryStaff, caseDoc.DocNo,caseDoc. d.Key);
                return SeizureResult;
            }
            #endregion

            #region Step 1：取出 allCaseMaster 中的CaseID之義務人 到ValidObligors

            bool isForeignID = false;
            string resp_name_message = "";
            
            var _caseObligors = asBiz.getObligor(caseid, ref resp_name_message, ref isForeignID); // 取得該案件，合法的義務人ID

            //// 20191119, 新增檢查, 若義務人(只要任何一個)是空白或NULL, 即落人工-----> START
            //bool isEmptyObligorName = false;
            //foreach(var obligor in _caseObligors)
            //{
            //    if (string.IsNullOrEmpty(obligor.ObligorName)) isEmptyObligorName = true;
            //}

            //if( isEmptyObligorName)
            //{
            //    SeizureResult = "0005|義務人姓名空白或NULL值，落人工";
            //    DocMemo.Add("義務人姓名空白或NULL值。");
            //    asBiz.insertCaseMemo(caseid, "CaseSeizure", DocMemo, eQueryStaff);

            //    return SeizureResult;
            //}
            //// 20191119, 新增檢查, 若義務人(只要任何一個)是空白或NULL, 即落人工-----> END

            // 是外國人ID
            if (isForeignID)
            {
                SeizureResult = "0005|有外國人ID，落人工";
                DocMemo.Add("ID無法辨識。");
                asBiz.insertCaseMemo(caseid, "CaseSeizure", DocMemo, eQueryStaff);

                return SeizureResult;
            }

            if (_caseObligors == null)
            {
                SeizureResult = "0014|ID無法辨識";
                DocMemo.Add("ID無法辨識");
                asBiz.insertCaseMemo(caseid, "CaseSeizure", DocMemo, eQueryStaff);

                return SeizureResult;
            }

            if (_caseObligors.Count() == 0)
            {
                SeizureResult = "0014|ID無法辨識";
                DocMemo.Add("ID無法辨識");
                asBiz.insertCaseMemo(caseid, "CaseSeizure", DocMemo, eQueryStaff);
                return SeizureResult;
            }

            // 獨資戶負責人不同
            //若負責人的姓名與ID, 與 個人不符, 寫到備註
            if (!string.IsNullOrEmpty(resp_name_message))
            {
                SeizureResult = "0009|負責人的姓名與ID不同 : " + resp_name_message;
                DocMemo.Add(resp_name_message);
                asBiz.insertCaseMemo(caseid, "CaseSeizure", DocMemo, eQueryStaff);
                return SeizureResult;
            }



            WriteLog(string.Format("Step 1 : 取得義務人ID = {0} / {1} 的結果", string.Join(",", _caseObligors.Select(x => x.ObligorNo)), string.Join(",", _caseObligors.Select(x => x.ObligorName))));



            List<string> DiffLists = new List<string>() { "*", "?", "" };

            var strDiffWord = pbiz.GetParmCodeByCodeType("DifficultWord").FirstOrDefault();
            if (strDiffWord != null)
            {
                DiffLists = strDiffWord.CodeDesc.Split(',').ToList();
            }


                #region 檢查難字

                bool isDifficult = false;
                foreach (var d in _caseObligors)
                {
                    foreach (var s in DiffLists)
                    {
                        if (d.ObligorName.Contains(s))
                            isDifficult = true;
                    }
                }
                if (isDifficult) //戶名有*號的, 表示有難字, 落人工處理                    
                {    
                    WriteLog("\t\tStep2-1-1 : 戶名有難字，請確認");
                    DocMemo.Add("戶名有難字，請確認");
                    Doc2Human = true;
                    SeizureResult = "0015|戶名有難字，請確認";
                    asBiz.insertCaseMemo(caseid, "CaseSeizure", DocMemo, eQueryStaff);
                    foreach (var s in _caseObligors)
                    {
                        insertBatchQuene(caseDoc.CaseId, eQueryStaff, caseDoc.DocNo, s.ObligorNo);
                    }
                    return SeizureResult;
                }
                #endregion



            #endregion

            #region Step 2 : 找出重號, 填入VaildObligors


            WriteLog(string.Format("\tStep 2-1-1: 判斷為 個人 / Case 1"));
            
            //bool isDiffWordAll = false;
            //bool isLongerGT3 = false;

            foreach (var v in _caseObligors)
            {
                WriteLog(string.Format("\tStep 2-1:  處理義務人 {0} / {1}", v.ObligorNo, v.ObligorName));

                #region 來文戶名長度>3的, 落人工
                if (v.ObligorName.Length > 3 && v.ObligorNo.Length >= 10) // 個人戶
                {
                    DocMemo.Add("戶名長度>3，落人工處理。");
                    WriteLog(string.Format("\t\t\tStep2-1-1 : 長度>3的的戶名, 決議不查, 落人工  戶名ID  {0} / {1}", v.ObligorNo, v.ObligorName));
                    Doc2Human = true;
                    SeizureResult = "0020|戶名長度>3，落人工處理。";
                    asBiz.insertCaseMemo(caseid, "CaseSeizure", DocMemo, eQueryStaff);
                    // 要查RM, 等所以直接Insert 一筆BatchQuene 
                    insertBatchQuene(caseDoc.CaseId, eQueryStaff, caseDoc.DocNo, v.ObligorNo);

                    return SeizureResult;
                }

                #endregion 

                Dictionary<string, string> doubleIDs = new Dictionary<string, string>();

                #region 發查60629
                var pkid = asBiz.addAutoLog(caseid, eQueryStaff, caseDoc.DocNo, v.ObligorNo, "60629");
                string retMessage = "";
                doubleIDs = objSeiHTG.Send60629(v.ObligorNo, v.CaseId, ref retMessage);
                if (doubleIDs.Count() == 0)
                {
                    doubleIDs.Add(v.ObligorNo, v.ObligorName);
                }

                if (retMessage.StartsWith("0000|"))
                    asBiz.updateAutoLog(pkid, 1, retMessage);
                else
                {
                    if (retMessage.EndsWith("電文訊息 :找不到相關資料"))
                    {
                        WriteLog("\t\t **** 發查60629, 找不到相關資料, 表示無存款往來");
                        //20190107, 表示無存款往來, 直接回文
                        asBiz.updateAutoLog(pkid,1, retMessage);
                        return "0038|" + retMessage;
                    }
                    else
                    {
                        asBiz.updateAutoLog(pkid, 2, retMessage);
                        //0001|電文00450 發查失敗01|交易限制
                        DocMemo.Add("發查60629失敗, 原因 : " + retMessage);
                        asBiz.insertCaseMemo(caseid, "CaseSeizure", DocMemo, eQueryStaff);
                        return "0028|" + retMessage;
                    }
                }
                WriteLog(string.Format("\t\tStep 2-1-2:  查詢重號結果 {0} / {1}", string.Join(", ", doubleIDs.Select(x => x.Key)), string.Join(", ", doubleIDs.Select(x => x.Value))));


                #endregion

                #region 發查67072
                {
                    var pkid72 = asBiz.addAutoLog(caseid, eQueryStaff, caseDoc.DocNo, v.ObligorNo, "67072");
                    string retMessage72 = "";
                    var d67072 = objSeiHTG.Send67072(v.ObligorNo, v.CaseId, ref retMessage72);


                    if (d67072.StartsWith("0000|"))
                        asBiz.updateAutoLog(pkid72, 1, retMessage72);
                    else
                    {
                        asBiz.updateAutoLog(pkid72, 2, retMessage72);
                        //0001|電文00450 發查失敗01|交易限制
                        DocMemo.Add("發查67072失敗, 原因 : " + retMessage72);
                        asBiz.insertCaseMemo(caseid, "CaseSeizure", DocMemo, eQueryStaff);
                        return "0028|" + retMessage72;
                    }
                    //WriteLog(string.Format("\t\tStep 2-1-2:  查詢重號結果 {0} / {1}", string.Join(", ", doubleIDs.Select(x => x.Key)), string.Join(", ", doubleIDs.Select(x => x.Value))));


                }

                #endregion

                #region 檢查是否全部是個人戶


                bool pAccount = false;
                bool cAccount = false;

                int pC = 0;
                int cC = 0;
                // 20180905 , 判斷是純個人, 或是公司
                foreach (var d in doubleIDs)
                {
                    if (d.Key.Length < 10)
                        cC++;
                    if (d.Key.Length >= 10)
                        pC++;
                }

                if (pC == doubleIDs.Count())
                    pAccount = true;
                if (cC == doubleIDs.Count())
                    cAccount = true;


                if (pC == 0 && cC == 0)
                {
                    WriteLog("錯誤, 不是公司戶,也不是個人戶");
                    return "0028|不是公司戶,也不是個人戶";
                }

                #endregion

                // 若查到重號，則加入VaildObligors
                foreach (var d in doubleIDs)
                {
                    bool isDiffWord = false; //檢查是否有難字

                    WriteLog(string.Format("\t\t\tStep 2-1-2-1:  查詢重號結果 {0} / {1}", d.Key, d.Value));
                    #region 檢查戶名有難字

                    foreach (var s in DiffLists)
                    {
                        if (d.Value.Contains(s))
                            isDifficult = true;
                    }
                    if (isDifficult) //戶名有*號的, 表示有難字, 落人工處理                    
                    {
                        isDiffWord = true;
                    }
                    #endregion

                    // 20180721, 長度超過3個字, 落人工

                    if (v.ObligorName.Trim().Equals(d.Value.Trim())) // 姓名相符, 加入ValidObligors
                    {
                        //AllDoubleNameCount++;
                        #region Insert VaildObligor
                        CaseObligor c = new CaseObligor();
                        c.ObligorNo = d.Key;
                        c.CaseId = caseid;
                        c.CreatedDate = DateTime.Now;
                        c.ObligorName = d.Value;
                        c.isChangeName = false; // 沒有改名
                        c.isSame = true; // 原來名字
                        c.newName = "";                        
                        c.isDiffWord = isDiffWord;
                        VaildObligors.Add(c);
                        #endregion
                        WriteLog(string.Format("\t\t\tStep 2-1-2-2: 戶名相符 來文戶名: {0} / 主機戶名:{1}", v.ObligorName, d.Value));
                    }
                    else // 姓名不相符, 去發查60600, 找出是否曾經相符過(過去有沒有改名字過)
                    {
                        WriteLog(string.Format("\t\t\tStep 2-1-2-2: 戶名不相符 來文戶名: {0} / 主機戶名:{1}", v.ObligorName, d.Value));
                        List<string> anoNames = new List<string>();

                         // 個人戶
                        {
                            pAccount = true;
                            #region 個人戶
                            // 20180605, 決議, 若姓名超過3個字, 也不用發查60600
                            if (v.ObligorName.Trim().Length <= 3)
                            {
                                var pkid222 = asBiz.addAutoLog(caseid, eQueryStaff, caseDoc.DocNo, v.ObligorNo, "60600");
                                string retMessage222 = "";
                                anoNames = objSeiHTG.Send60600(d.Key, caseid, ref retMessage222);
                                if (retMessage222.StartsWith("0000|"))
                                    asBiz.updateAutoLog(pkid222, 1, retMessage222);
                                else
                                {
                                    asBiz.updateAutoLog(pkid222, 2, retMessage222);
                                    //asBiz.updateAutoLog(pkid, 2, retMessage);
                                    DocMemo.Add("發查60600失敗, 原因 : " + retMessage);
                                    asBiz.insertCaseMemo(caseid, "CaseSeizure", DocMemo, eQueryStaff);
                                    return "0028|" + retMessage;
                                }


                                //bool isSameName = false;

                                #region 找60600, 若有的話isSameName = true
                                if (anoNames.Count() > 0)
                                {
                                    string allNewName = v.ObligorName;                                    
                                    string qNewName = "";
                                    bool isCName = false;
                                    foreach (var ano in anoNames)
                                    {
                                        WriteLog(string.Format("\t\t\tStep 2-1-2-3: 查到60600的資料 {0}", ano));

                                        #region 20180911, 先要抓出舊名字跟新名字
                                        int iPos = ano.IndexOf("CHANGE");
                                        if (iPos > 0)
                                        {
                                            string oldName = ano.Substring(0, iPos).Trim();
                                            string newName = ano.Substring(iPos + 6).Trim();

                                            // 要分舊名字跟新名字.. 若是來文舊名字, 扣押到新名字, 則要加備註, "本行戶名已更名為XXX。"

                                            if (allNewName == oldName)
                                            {
                                                
                                                if (!string.IsNullOrEmpty(newName))
                                                {
                                                        qNewName = "本行戶名已更名為" + newName + "。";
                                                        WriteLog(string.Format("\t\t\tStep 2-1-2-4: 查到新名字 {0}", newName));
                                                }
                                                allNewName = newName;
                                                isCName = true;
                                                WriteLog(string.Format("\tStep2-4 : 查到來文名字 {0} = 新名字 {1}", v.ObligorName, newName));
                                            }

                                        }
                                        #endregion
                                    }


                                    CaseObligor c1 = new CaseObligor();

                                    #region Insert VaildObligor
                                    
                                    c1.ObligorNo = d.Key;
                                    c1.CaseId = caseid;
                                    c1.CreatedDate = DateTime.Now;
                                    c1.ObligorName = d.Value;
                                    c1.newName = "";
                                    c1.isChangeName = false; // 有改過名
                                    c1.isSame = false; // 更名後, 正確
                                    c1.isDiffWord = isDiffWord;
                                    c1.docMemo = qNewName;
                                    
                                    #endregion


                                    if( isCName) // 有更名, 則要用新的名字
                                    {
                                        c1.newName = allNewName;
                                        c1.isChangeName = true;
                                        c1.isSame = true;
                                    }

                                    VaildObligors.Add(c1);

                                }
                                else // 若沒有更名, 還是要查他的餘額
                                {
                                    #region Insert VaildObligor
                                    CaseObligor c = new CaseObligor();
                                    c.ObligorNo = d.Key;
                                    c.CaseId = caseid;
                                    c.CreatedDate = DateTime.Now;
                                    c.ObligorName = d.Value;
                                    c.newName = "";
                                    c.isChangeName = false; // 有改過名
                                    c.isSame = false; // 更名後, 正確
                                    c.isDiffWord = isDiffWord;
                                    VaildObligors.Add(c);

                                    #endregion
                                }
                                #endregion
                            }
                            else // 長度>3的的戶名, 決議不查, 落人工
                            {
                                DocMemo.Add("戶名長度>3，落人工處理。");
                                WriteLog(string.Format("\t\t\tStep2-1-2-1 : 長度>3的的戶名, 決議不查, 落人工  戶名ID  {0} / {1}", v.ObligorNo, v.ObligorName));
                                Doc2Human = true;
                                SeizureResult = "0020|戶名長度>3，落人工處理。";
                                asBiz.insertCaseMemo(caseid, "CaseSeizure", DocMemo, eQueryStaff);
                                // 要查RM, 等所以直接Insert 一筆BatchQuene 
                                insertBatchQuene(caseDoc.CaseId, eQueryStaff, caseDoc.DocNo, d.Key);

                                return SeizureResult;
                            }
                            #endregion
                        }

                    } // End if (v.ObligorName.Trim().Equals(d.Value.Trim())) // 姓名相符, 加入ValidObligors
                } // End foreach (var d in doubleIDs)
            } // END foreach (var v in _caseObligors)


            //VaildObligors = VaildObligors.DistinctBy(x => x.ObligorNo).ToList();
            WriteLog(string.Format("Step 2 : 義務人ID  / Name / 相同  / 新名字"));
            foreach(var v in VaildObligors)
            {
                WriteLog(string.Format("Step 2 :  {0} / {1}  / {2} / {3}",  v.ObligorNo, v.ObligorName, v.isSame.ToString(), v.newName));
            }
            

            // 20180910 , 若有任何落人工的動作, 一律InsertBatchQuene 把帳務的部分, 都查完
            // 原因是, 某人, 有設立籌備處, 因此有重號, 但 籌備處 的戶名 > 3 , 因此, 會直接落人工, 而本號, 確沒有去查 所造成



            #endregion

            #region Step 3 : 找出所有這些義務人的帳號, 打60491

            // 此變數準備 不在 ProdCode 中(正向可扣押, 負向不可扣押)的帳務, 判斷是否有漏掉的帳戶, 若還扣不足額, 需要落人工提示
            List<ObligorAccount> allAccLists = new List<ObligorAccount>();

            if (VaildObligors.Count() == 0)
            {
                //DocMemo.Add("於本行無存款往來。");
                Doc2Human = true;
                SeizureResult = "0027|於本行無存款往來。";
                asBiz.insertCaseMemo(caseid, "CaseSeizure", DocMemo, eQueryStaff);
                //insertBatchQuene(caseDoc.CaseId, eQueryStaff, caseDoc.DocNo, d.Key);
                WriteLog("\tStep3-1 : 找不到合法的ID");
                return SeizureResult;
            }

            bool isDoubleAcc = false;

            foreach (var v in VaildObligors.OrderBy(x=>x.ObligorNo.Length))
            {
                var pkid = asBiz.addAutoLog(caseid, eQueryStaff, caseDoc.DocNo, v.ObligorNo, "60491");
                string retMessage = "";
                List<ObligorAccount> oaList = objSeiHTG.Send60491(v.ObligorNo, v.CaseId, BranchInfo, ref retMessage).Where(x => !x.Account.Contains("000000000000")).ToList();
                if( v.isSame) // 若戶名相同, 則可扣押
                {
                    foreach(var a in oaList)
                    {
                        a.noSeizure = false;
                    }
                }
                else
                {
                    foreach (var a in oaList)
                    {
                        a.noSeizure = true;
                    }
                }

                foreach (var a in oaList)
                {
                    a.isDoubleAcc = isDoubleAcc;
                    a.isSame = v.isSame;
                }


                if( !isDoubleAcc)
                {
                    foreach(var a in oaList)
                    {
                        a.isDoubleAcc = isDoubleAcc;
                    }
                    isDoubleAcc = true;
                }



                if (retMessage.StartsWith("0000|"))
                    asBiz.updateAutoLog(pkid, 1, retMessage);
                else
                {
                    retMessage = "0099|發查60491失敗";
                    asBiz.updateAutoLog(pkid, 2, retMessage);
                    WriteLog("發查失敗 " + retMessage);
                    DocMemo.Add("發查60491失敗");
                    asBiz.insertCaseMemo(caseid, "CaseSeizure", DocMemo, eQueryStaff);
                    return "0028|" + retMessage;
                }

                // 20180814, 每個ID, 都要打RM(67050-8), 並存到TX_67002 中, 即可
                #region 每個ID, 都要打RM, 並存到TX_67002 中, 即可
                try
                {
                    var pkid333 = asBiz.addAutoLog(caseid, eQueryStaff, caseDoc.DocNo, v.ObligorNo, "67050-8");
                    string retMessage333 = objSeiHTG.Send67002(v.ObligorNo, v.CaseId.ToString());
                    if (retMessage333.StartsWith("0000|"))
                        asBiz.updateAutoLog(pkid333, 1, retMessage333);
                    else
                    {
                        retMessage = "0099|發查67505-8失敗";
                        asBiz.updateAutoLog(pkid333, 2, retMessage333);
                        WriteLog("發查失敗 " + retMessage333);
                        DocMemo.Add("發查67505-8失敗");

                        asBiz.insertCaseMemo(caseid, "CaseSeizure", DocMemo, eQueryStaff);
                        return "0028|" + retMessage;
                    }
                }
                catch (Exception ex)
                { }
                #endregion


                // 要排除 ("關係別") TX_60491_Detl.Link = 'JOIN'
                // 要排除 TX_60491_Detl.StsDesc = '結清' or '放款' or '現金卡'

                List<ObligorAccount> newOaList = getSeizureAccount(oaList); // 將TX_60491_Detl.StsDesc = '結清' or '放款' or '現金卡', ("關係別") TX_60491_Detl.Link = 'JOIN' 排除

                AccLists.AddRange(newOaList);
                if( v.isSame)
                    WriteLog("=====================可扣押========================");
                else
                    WriteLog("==================不可扣押, 因為戶名不符=====================");
                WriteLog(string.Format("\tStep 3-2-1 : ID = {0}, 共有{1} 帳號, 分別為{2}", v.ObligorNo, oaList.Count().ToString(), string.Join(",", oaList.Select(x => x.Account))));
                WriteLog("\t排除 結清  放款 現金卡JOIN後");
                WriteLog(string.Format("\tStep 3-2-2 : ID = {0}, 共有{1} 帳號, 分別為{2}", v.ObligorNo, newOaList.Count().ToString(), string.Join(",", newOaList.Select(x => x.Account))));

            }


            WriteLog(string.Format("\r\nStep 3 : 本案件共有{0} 帳號\r\n", AccLists.Count().ToString()));

            #endregion

            #region Step 4 根據AccLists中所有的帳戶，去查歷史上的扣押交易, 順便把查401的餘額填入Bal



            //20181023, A10710190002, 發現, 若重號若無往來, 則什麼都不說

            foreach (var v in VaildObligors)
            {
                var pAcc = AccLists.Where(x => x.Id == v.ObligorNo).ToList();
                if (pAcc.Count == 0 || v.isDiffWord) // 若無存款往來, 則什麼也不說
                {

                }
                else
                {
                    if( ! string.IsNullOrEmpty(v.docMemo))
                        DocMemo.Add(v.docMemo);
                }

            }

            if (AccLists.Count() == 0)
            {
                //DocMemo.Add("於本行無存款往來。");
                Doc2Human = true;
                SeizureResult = "0008|於本行無存款往來。";
                asBiz.insertCaseMemo(caseid, "CaseSeizure", DocMemo, eQueryStaff);
                return SeizureResult;
            }

            foreach (var item in AccLists)
            {

                bool isForeign = false;
                bool bisOD = false;
                bool bisTD = false;
                bool bisLon = false;
                bool bisHoldAmt = false; ; // 他案扣押金額
                
                string errMessage = null;
                item.AccountType = lookUpAccountType(item.ProdCode, SeizureOrderTable);
                if (item.AccountType.StartsWith("外幣"))
                    isForeign = true;
                item.Bal = getBalance(item.Id, item.CaseId, item.Account, ref bisOD, ref bisTD, ref bisLon, ref bisHoldAmt, caseDoc.CaseNo, item.Ccy, ref errMessage, isForeign);


                if (string.IsNullOrEmpty(errMessage))
                { }
                else
                {
                    //0001|電文00450 發查失敗01|交易限制
                    //errMessage = "0028|發查401失敗";
                    DocMemo.Add("帳戶:" + item.Account + "發查失敗, 原因:" + errMessage);
                    asBiz.insertCaseMemo(caseid, "CaseSeizure", DocMemo, eQueryStaff);
                    return "0028|" + errMessage;
                }



                item.isOD = bisOD;
                item.isTD = bisTD;
                item.isLon = bisLon;

                // 20180802, 電子的部分, 450-31有他案扣押, 也照樣扣, 
                if (bisHoldAmt)
                {
                    //item.message45031 = "帳戶因他案已扣押在案。(請查明)";
                    item.is45031OK = true;
                }
                else
                {
                    //item.message45031 = "";
                    item.is45031OK = false;
                }

                var pkid = asBiz.addAutoLog(caseid, eQueryStaff, caseDoc.DocNo, item.Id, "45030");
                string retMessage = "";
                string message450 = "";
                string message450Code = "";

                // 
                item.is450OK = false; item.message450 = ""; item.message450Code = "";

                if (item.StsDesc.Trim().Equals("事故"))
                {

                    Dictionary<string, string> dic450Result = objSeiHTG.getAccident2(item.Id, item.CaseId, item.Account, item.Ccy, ref retMessage);
                    if (dic450Result == null)
                    {
                        item.is450OK = false; item.message450 = ""; item.message450Code = "";
                    }
                    else
                    {
                        if (dic450Result.Count() > 0)
                        {
                            item.is450OK = true;
                            item.message450Code = string.Join("@", dic450Result.Keys);
                            item.message450 = string.Join("@", dic450Result.Values);
                        }
                        else
                        {
                            item.is450OK = false; item.message450 = ""; item.message450Code = "";
                        }
                    }


                    if (retMessage.StartsWith("0000|"))
                        asBiz.updateAutoLog(pkid, 1, retMessage);
                    else
                    {
                        asBiz.updateAutoLog(pkid, 2, retMessage);

                        //0001|電文00450 發查失敗01|交易限制
                        DocMemo.Add("帳戶:" + item.Account + "發查失敗, 原因:" + retMessage.Replace("0001|電文00450 發查失敗", ""));
                        asBiz.insertCaseMemo(caseid, "CaseSeizure", DocMemo, eQueryStaff);
                        return "0028|" + retMessage;
                    }
                }
                WriteLog(string.Format("\tStep 4-3 : ID {0},  帳戶{1}, ProdCode {2},  餘額 {3}, 幣別 {4}, 不可扣押 {5}", item.Id, item.Account, item.ProdCode, item.Bal.ToString(), item.Ccy, item.noSeizure.ToString()));
                WriteLog(string.Format("\tStep 4-4 : OD={0}, TD={1}, 放款={2}, 他案={3}", bisOD.ToString(), bisTD.ToString(), bisLon.ToString(), bisHoldAmt.ToString()));
                WriteLog(string.Format("\tStep 4-5 : 事故代號={0}, 事故訊息={1}", item.message450Code, item.message450));
            }


            WriteLog("Step 4: 查詢33401 及450-30, 450-31 相關帳戶資訊!!");
            #endregion

            #region Step 5: 針對AccLists的排除不可扣押的類型, 並在扣押前, 及查401



            // 5.1 先從AccLists中, 拿掉, 絕對不可被扣押的帳戶類型
            #region 排除絕對不可被扣押的帳戶類型

            var delAccList = AccLists.Where(x => x.AccountType == "確定不用扣押").ToList();
            AccLists = AccLists.Except(delAccList).ToList();

            #endregion


            // 5.2 排除 "結清", "已貸", "啟用", "誤開", "新戶" 的帳戶



            #region  排除 "結清", "已貸", "啟用", "誤開", "新戶" 的帳戶
            List<ObligorAccount> newAccList = new List<ObligorAccount>();
            List<string> noSave = new List<string>() { "結清", "已貸", "啟用", "誤開", "新戶", "核准", "婉拒", "作廢" };
            //
            foreach (var acc in AccLists)
            {
                bool bfilter = true;
                if (acc.Account.StartsWith("000000000000"))
                    bfilter = false;

                #region 判斷是否是現金卡等等
                // 若 prod_code = 0058, 或XX80 , 不用存
                if (acc.ProdCode.ToString().Equals("0058") || acc.ProdCode.ToString().EndsWith("80"))
                    bfilter = false;

                // 若  Link<>'JOIN' , 不用存
                if (acc.Link.ToString().Equals("JOIN"))
                    bfilter = false;

                // 若 StsDesc='結清' AND  StsDesc='已貸' AND  StsDesc='啟用' AND  StsDesc='誤開'  AND  StsDesc='新戶', 也不用存
                string sdesc = acc.StsDesc.ToString().Trim();
                if (noSave.Contains(sdesc))
                    bfilter = false;

                #endregion

                if (bfilter)
                    newAccList.Add(acc);
            }

            AccLists = newAccList;
            #endregion


            // 記錄在排序之前, 目前可能可以被扣押的帳戶, (一定包括可扣押的帳戶, 也包括, 未列出的)
            foreach (var acc in AccLists)
            {
                allAccLists.Add(acc);
            }




            // 5.3 開始排順序, 
            // 要分二種, 第一種是, 只有"個人"或"公司"
            //                  第二種是, "個人及公司"都有的...


            List<ObligorAccount> OrderedAccLists = new List<ObligorAccount>();




            int pCount = getPersonCount(AccLists); // AccLists.Count(x => x.Id.Length >= 10); // 個人的帳戶數
            int cCount = getComCount(AccLists); // AccLists.Count(x => x.Id.Length < 10); // 公司的帳戶數
            WriteLog(string.Format("Step 5: 待扣押帳戶 個人戶{0}, 公司戶{1} ", pCount.ToString(), cCount.ToString()));


            #endregion

            #region Step 6: 開始扣押,  要分三種狀況來執行, 先排優先順序, 再真正扣押

            string strCalcResult = null; // 本變數儲存, 計算後扣押的結果

            #region 只有個人ID要扣押
            if (pCount > 0 && cCount == 0)
            {
                decimal realSeizureAmt = 0.0m;
                SeizureResult = PersonSeizure(caseDoc, AccLists, SeizureOrder, pbiz, HandleFee, SeizureTotal, govNo, ref DocMemo,  isRename, isAmountBelow450, allAccLists, doubleID, VaildObligors, ref realSeizureAmt);
            }
            #endregion

            #endregion




            return SeizureResult;

        }

        private static int getPersonCount(List<ObligorAccount> AccLists)
        {
            Regex reHuman = new Regex(@"^[A-Z]{1}\d{9,10}");
            int iCount = 0;
            foreach (var a in AccLists)
            {
                if (reHuman.IsMatch(a.Id))
                {
                    iCount++;
                }
            }
            return iCount;
        }

        private static int getComCount(List<ObligorAccount> AccLists)
        {
            Regex reCo = new Regex(@"^\d{8}");
            int iCount = 0;
            foreach (var a in AccLists)
            {
                if (reCo.IsMatch(a.Id))
                {
                    iCount++;
                }
            }
            return iCount;
        }


        //                        insertBatchQuene(caseDoc.CaseId, eQueryStaff, caseDoc.DocNo, d.Key, "60491", DateTime.Now, "0");
        private static void insertBatchQuene(Guid guid, string eQueryStaff, string p1, string p2)
        {
            CTBC.FrameWork.HTG.HostMsgGrpBIZ hostbiz = new CTBC.FrameWork.HTG.HostMsgGrpBIZ();

            string strSql1 = string.Format("INSERT INTO [dbo].[BatchQueue]([CaseId],[SendUser],[DocNo],[ObligorNo],[ServiceName],[SendDate],[Status],[CreateDatetime]) VALUES ('{0}','{1}','{2}','{3}','{4}',GetDate(),'0',GetDate());", guid.ToString(), eQueryStaff, p1, p2, "60491");
            string strSql2 = string.Format("INSERT INTO [dbo].[BatchQueue]([CaseId],[SendUser],[DocNo],[ObligorNo],[ServiceName],[SendDate],[Status],[CreateDatetime]) VALUES ('{0}','{1}','{2}','{3}','{4}',GetDate(),'0',GetDate());", guid.ToString(), eQueryStaff, p1, p2, "67072");
            string strSql3 = string.Format("INSERT INTO [dbo].[BatchQueue]([CaseId],[SendUser],[DocNo],[ObligorNo],[ServiceName],[SendDate],[Status],[CreateDatetime]) VALUES ('{0}','{1}','{2}','{3}','{4}',GetDate(),'0',GetDate());", guid.ToString(), eQueryStaff, p1, p2, "67100");
            ArrayList array = new ArrayList();
            array.Add(strSql1); array.Add(strSql2); array.Add(strSql3);

            //WriteLog("寫入參數檔....");
            var ret = hostbiz.SaveESBData(array);

        }

        private static string PersonSeizure(CaseMaster caseDoc, List<ObligorAccount> AccLists, Dictionary<string, List<string>> SeizureOrder, PARMCodeBIZ pbiz, int HandleFee, decimal SeizureTotal, string govNo, ref List<string> DocMemo,   bool isRename, bool isAmountBelow450, List<ObligorAccount> allAccLists, string doubleID, List<CaseObligor> VaildObligors, ref decimal realSeizureAmt)
        {
            WriteLog(string.Format("\tStep 6-2: 只扣押個人戶"));
            List<ObligorAccount> OrderedAccLists = new List<ObligorAccount>();
            string strCalcResult = "";
            string SeizureResult = "";


            var pOrder = getPersonOrder(AccLists, SeizureOrder);

            // 因為發生了, 0019(台幣), 跟XX19(外幣), 會造成二筆, 所以要刪除那一筆
            pOrder = distinctPOrder(pOrder);
            OrderedAccLists.AddRange(pOrder);

            // 針對這些個人帳戶, 去扣押
            realSeizureAmt = 0.0m;
            List<ObligorAccount> calculatedAccList = CalcSeizureAmt(OrderedAccLists, pbiz, HandleFee, SeizureTotal, govNo, VaildObligors, ref realSeizureAmt, ref strCalcResult);

            #region 若來文金額異常, 落人工, 但要InsertCaseSeizre
            if (isAmountBelow450)
            {
                SeizureResult = "0030|來函扣押金額異常(請查明)。";
                DocMemo.Add("來函扣押金額異常(請查明)。");
                asBiz.insertCaseMemo(caseDoc.CaseId, "CaseSeizure", DocMemo, eQueryStaff);
                foreach (var s in calculatedAccList)
                {
                    s.planSeizure = 0.0m;
                    s.showSeizure = 0.0m;
                }
                asBiz.insertCaseSeizure2(caseDoc, calculatedAccList, eQueryStaff, SeizureResult);
                return SeizureResult;
            }


            #endregion
            #region 如果有事故11,12,13,14 , 要加備註, 會不要落人工

            if (calculatedAccList.Any(x => x.planSeizure > 0  && x.is450OK && (x.message450Code.Trim().Contains("11") || x.message450Code.Trim().Contains("12") || x.message450Code.Trim().Contains("13") || x.message450Code.Trim().Contains("14"))))
            {
                Dictionary<string, string> mess45030 = new Dictionary<string, string>()
                    {
                        {"11", "帳戶已設定為警示帳戶。"},
                        {"12", "帳戶已設定為警示帳戶。"},
                        {"13", "帳戶已設定為衍生警示帳戶。"},
                        {"14", "帳戶已設定為警示帳戶。"}
                    };
                WriteLog("\tStep 6 : 帳戶有事故 11,12,13,14 (不需落人工)，請確認");

                var seiTot = calculatedAccList.Where(x => x.planSeizure > 0).Sum(x => x.Twd);

                if (seiTot >= 450) //扣押要大於450元, 才要寫事故
                {
                    foreach (var s in calculatedAccList.Where(x => x.planSeizure > 0 && x.is450OK))
                    {
                        if (s.message450Code.Contains("@"))
                        {
                            string[] qqq = s.message450Code.Split('@');
                            foreach (var q in qqq)
                                if (mess45030.ContainsKey(q)) DocMemo.Add(mess45030[q]);
                        }
                        else
                            DocMemo.Add(s.message450);
                    }
                    //Doc2Human = true;
                    SeizureResult = "0022|帳戶有事故11,12,13,14 ，請確認";
                    //asBiz.insertCaseMemo(caseid, "CaseSeizure", DocMemo, eQueryStaff);
                    WriteLog("備註文字: " + string.Join(",", DocMemo.Select(x => x)));
                }



            }



            #endregion


            bool isfixaccount = calculatedAccList.Any(x => x.planSeizure > 0 && (x.AccountType.Contains("定存") || x.AccountType.Contains("綜定")));


            #region 如果有事故, 4, 5,6,7,8, 10 , 要落人工
            if (calculatedAccList.Any(x => x.planSeizure > 0  && x.is450OK && (x.message450Code.Trim().Contains("04")
                || x.message450Code.Trim().Contains("05") || x.message450Code.Trim().Contains("06")
                || x.message450Code.Trim().Contains("07") || x.message450Code.Trim().Contains("08") || x.message450Code.Trim().Contains("10"))))
            {

                var seiTot = calculatedAccList.Where(x => x.planSeizure > 0).Sum(x => x.Twd);

                if (seiTot < 450)
                {

                    if (!isfixaccount)
                    {
                        //低於450元, 什麼DocMemo都不用說
                        List<string> newDocMemo = new List<string>() {"扣除手續費存款餘額未達200元，不予扣押。"};
                        asBiz.insertCaseMemo(caseDoc.CaseId, "CaseSeizure", newDocMemo, eQueryStaff);
                        foreach (var s in calculatedAccList)
                        {
                            s.planSeizure = 0.0m;
                            s.showSeizure = 0.0m;
                        }
                        
                        asBiz.insertCaseSeizure2(caseDoc, calculatedAccList, eQueryStaff, "0024|有事故, 但扣押金額低於450, 所以直接回文");
                        string SeizureResult222 = "0024|有事故, 但扣押金額低於450, 所以直接回文";
                        return SeizureResult222;
                    }
                }


                Dictionary<string, string> mess45030 = new Dictionary<string, string>()
                    {
                        {"04", "帳戶因他案已凍結在案。(請查明)"},
                        {"05", "帳戶因他案已設定質權在案。(請查明)"},
                        {"06", "帳戶因他案已設定質權在案。(請查明)"},
                        {"07", "帳戶已設為支存拒往戶。(請查明)"},
                        {"08", "帳戶因本票中止契約。(請查明)"},
                        {"10", "帳戶因其他原因已凍結在案。(請查明)"}
                    };
                WriteLog("\tStep 6 : 帳戶有事故4, 5,6,7,8, 10 , 要落人工，請確認");
                foreach (var s in calculatedAccList.Where(x => x.planSeizure > 0 && x.is450OK))
                {
                    if (s.message450Code.Contains("@"))
                    {
                        string[] qqq = s.message450Code.Split('@');
                        foreach (var q in qqq)
                            if (mess45030.ContainsKey(q)) DocMemo.Add(mess45030[q]);
                    }
                    else
                        DocMemo.Add(s.message450);
                }
                //Doc2Human = true;
                SeizureResult = "0022|帳戶有事故4, 5,6,7,8, 10，請確認";

                if (!isfixaccount)
                {
                    asBiz.insertCaseMemo(caseDoc.CaseId, "CaseSeizure", DocMemo, eQueryStaff);
                    foreach (var s in calculatedAccList)
                    {
                        s.planSeizure = 0.0m;
                        s.showSeizure = 0.0m;
                    }
                    WriteLog("備註文字: " + string.Join(",", DocMemo.Select(x => x)));
                    asBiz.insertCaseSeizure2(caseDoc, calculatedAccList, eQueryStaff, SeizureResult);
                    return SeizureResult;
                }
            }

            #endregion


            #region 如果, 有扣要定存類, 則要落人工
            // 綜定 定存 外幣定存
            if (calculatedAccList.Any(x => x.planSeizure > 0  && (x.AccountType.Contains("定存") || x.AccountType.Contains("綜定"))))
            {
                WriteLog("\tStep 6 : 有扣到定存類，請確認");
                var fixacc = calculatedAccList.Where(x => x.planSeizure > 0 && (x.AccountType.Contains("定存") || x.AccountType.Contains("綜定"))).First();
                DocMemo.Add(string.Format("定存{0} 需要被扣押到，請確認", fixacc.Account));
                //Doc2Human = true;
                SeizureResult = "0021|有扣到定存類，請確認";
                asBiz.insertCaseMemo(caseDoc.CaseId, "CaseSeizure", DocMemo, eQueryStaff);
                foreach (var s in calculatedAccList)
                {
                    s.planSeizure = 0.0m;
                    s.showSeizure = 0.0m;
                }
                WriteLog("備註文字: " + string.Join(",", DocMemo.Select(x => x)));
                asBiz.insertCaseSeizure2(caseDoc, calculatedAccList, eQueryStaff, SeizureResult);
                return SeizureResult;
            }


            #endregion


            #region 重號的戶名與來文名不同, 要提示 足額, 或不足額 ( 20181127 , 不應該只有重號, 才要考慮, 反之, 也要提示)

            var idCount = calculatedAccList.GroupBy(x => x.Id).Count();
            
            if (calculatedAccList.Any(x=> !x.isSame) && idCount>=2) // 表示, 有本號, 或重號, 有一個不同
            {
                // 要測試 20181031 --------------
                SeizureResult = "0013|重號的戶名不符。";
                List<string> sameIds = VaildObligors.Where(x => x.isSame).Select(x => x.ObligorNo).ToList();
                List<string> notsameIds = VaildObligors.Where(x => !x.isSame).Select(x => x.ObligorNo).ToList();

                bool isEnough = false;

                #region 戶名相同的處理
                var totalBal = calculatedAccList.Where(x => sameIds.Contains(x.Id)).Sum(x => x.Twd);
                if( totalBal >=450)
                {
                    if (SeizureTotal <= totalBal) // 足額
                    {
                        isEnough = true;
                        WriteLog("有重號, 一個同, 另一個不同，足額扣押");
                        // 若是足額, 則不需要"另查XXXX" 
                        //var newMemo = DocMemo.Where(x => !x.Contains("另查，該身分")).ToList();
                        //asBiz.insertCaseMemo(caseDoc.CaseId, "CaseSeizure", newMemo, eQueryStaff);
                        //asBiz.insertCaseSeizure(caseDoc, OrderedAccLists, eQueryStaff, "0000|足額扣押");
                    }
                    else // 不足額
                    {
                        WriteLog("有重號, 一個同, 另一個不同，不足額扣押");
                        //asBiz.insertCaseMemo(caseDoc.CaseId, "CaseSeizure", DocMemo, eQueryStaff);
                        //asBiz.insertCaseSeizure(caseDoc, OrderedAccLists, eQueryStaff, "0001|不足額扣押");
                    }
                }
                else // 小於450元, 將samids中的帳務設成0
                {
                    var removeItems = calculatedAccList.Where(x => sameIds.Contains(x.Id)).ToList();
                    foreach (var s in removeItems)
                    {                        
                        s.planSeizure = 0.0m;
                        s.showSeizure = 0.0m;                        
                    }
                }


                #endregion 



                #region 戶名不同的處理
                {
                    var totalBalNotSame = calculatedAccList.Where(x => notsameIds.Contains(x.Id)).Sum(x => x.Twd);

                    if (! isEnough) // 不足額時, 才要寫
                    {
                        if (totalBalNotSame >= 450)
                            DocMemo.Add("另查，該身分證統一編號於本行尚有另一帳戶開立之戶名與來函戶名不符。");
                        else
                            DocMemo.Add("另查，該身分證統一編號於本行尚有另一帳戶開立之戶名與來函戶名不符且存款餘額未達新臺幣200元。");
                    }

                    // 20181031, 當有此情形時, 要把重號(戶名不符) 的, 在帳務資訊中刪除, 否則回文時, 會出現重號的帳務出來
                    var removeItems = calculatedAccList.Where(x => notsameIds.Contains(x.Id)).ToList();
                    foreach (var s in removeItems)
                    {
                        //calculatedAccList.Remove(r);
                        s.planSeizure = 0.0m;
                        s.showSeizure = 0.0m;
                    }
                }
                #endregion 

                asBiz.insertCaseMemo(caseDoc.CaseId, "CaseSeizure", DocMemo, eQueryStaff);
                if( isEnough)
                    asBiz.insertCaseSeizure(caseDoc, calculatedAccList, eQueryStaff, "0000|足額扣押");                    
                else
                    asBiz.insertCaseSeizure(caseDoc, calculatedAccList, eQueryStaff, "0001|不足額扣押");
                string errorAccount = null;

                
                
                // ------------------------------------- 20181011 -------------------------
                // 目前測試版本, 先不扣押
                var result22 = SeizureSetting(calculatedAccList, govNo, ref errorAccount);
                //WriteLog("**********提醒目前並未執行真正扣押動作**************");
                
                //-------------------------------------------------------------------------------

                //20181207, 直接回文, 所以把isNoSeizure=True砍掉...
                asBiz.delCaseSeizureNoSeizure(caseDoc, calculatedAccList);


                return SeizureResult;

                //var totalBal = calculatedAccList.Where(x => sameIds.Contains(x.Id)).Sum(x => x.Twd);
                //if (totalBal >= 450)
                //{

                //    foreach (var s in calculatedAccList.Where(x => !sameIds.Contains(x.Id))) // 把不可扣押的ID, 設為0元
                //    {
                //        s.planSeizure = 0.0m;
                //        s.showSeizure = 0.0m;
                //    }


                //    string errorAccount = null;
                //    var result = SeizureSetting(calculatedAccList, govNo, ref errorAccount);

                //    #region 0002扣押中, 有錯

                //    if (result.StartsWith("0002|"))
                //    {
                //        string retMessage = result.Replace("0002|", "");
                //        DocMemo.Add(retMessage);
                //        // 寫入CaseMemo, 然後return
                //        asBiz.insertCaseMemo(caseDoc.CaseId, "CaseSeizure", DocMemo, eQueryStaff);

                //        // 要把已扣押的, 寫入CaseSeizure,
                //        bool startToZero = false;
                //        foreach (var acc in OrderedAccLists.Where(x => x.planSeizure != 0))
                //        {
                //            if (acc.Account == errorAccount)
                //            {
                //                startToZero = true;
                //            }
                //            if (startToZero)
                //            {
                //                acc.planSeizure = 0.0m;
                //                acc.showSeizure = 0.0m;
                //            }
                //        }
                //    }
                //    #endregion

                //    // 要分足額扣押, 或是部分扣押
                //    if (SeizureTotal <= totalBal && totalBal >= 450) // 足額
                //    {
                //        WriteLog("備註文字: " + string.Join(",", DocMemo.Select(x => x)));
                //        // 若是足額, 則不需要"另查XXXX" 
                //        var newMemo = DocMemo.Where(x => !x.Contains("另查，該身分")).ToList();
                //        asBiz.insertCaseMemo(caseDoc.CaseId, "CaseSeizure", newMemo, eQueryStaff);
                //        asBiz.insertCaseSeizure(caseDoc, OrderedAccLists, eQueryStaff, "0000|足額扣押");
                //    }
                //    else
                //    {
                //        WriteLog("備註文字: " + string.Join(",", DocMemo.Select(x => x)));
                //        asBiz.insertCaseMemo(caseDoc.CaseId, "CaseSeizure", DocMemo, eQueryStaff);
                //        asBiz.insertCaseSeizure(caseDoc, OrderedAccLists, eQueryStaff, "0001|不足額扣押");
                //    }
                //    return result;
                //}
                //else
                //{

                //    List<string> newDocMemo = new List<string>();
                //    foreach (var m in DocMemo)
                //    {
                //        if (!m.Contains("本行戶名已更名為"))
                //        {
                //            newDocMemo.Add(m);
                //        }
                //    }
                //    foreach (var s in calculatedAccList)
                //    {
                //        s.planSeizure = 0.0m;
                //        s.showSeizure = 0.0m;
                //    }
                //    WriteLog("備註文字: " + string.Join(",", DocMemo.Select(x => x)));
                //    asBiz.insertCaseMemo(caseDoc.CaseId, "CaseSeizure", newDocMemo, eQueryStaff);
                //    asBiz.insertCaseSeizure2(caseDoc, calculatedAccList, eQueryStaff, SeizureResult);
                //    return SeizureResult;
                //}
            }

            #endregion


            #region 若戶名不符, 則要決定, 餘額超過450或低於450元
            if ( calculatedAccList.Any(x=>!x.isSame && !x.isDoubleAcc)) // 戶名不符
            {
                // 要測試 20181031 --------------
                SeizureResult = "0013|戶名不符。";
                var totalBal = calculatedAccList.Sum(x => x.Twd);
                if (totalBal >= 450)
                    DocMemo.Add("戶名不符。");
                else
                    DocMemo.Add("戶名不符且存款餘額未達200元，不予扣押。");

                foreach (var s in calculatedAccList)
                {
                    s.planSeizure = 0.0m;
                    s.showSeizure = 0.0m;
                }
                WriteLog("備註文字: " + string.Join(",", DocMemo.Select(x => x)));
                asBiz.insertCaseMemo(caseDoc.CaseId, "CaseSeizure", DocMemo, eQueryStaff);
                asBiz.insertCaseSeizure2(caseDoc, calculatedAccList, eQueryStaff, SeizureResult);


                return SeizureResult;
            }

            #endregion

            #region 若有新名子, 且金額大於450元, 要提示新名字
            if (isRename) // 有新名字, 要提示
            {

                var totalBal1 = calculatedAccList.Sum(x => x.Twd);
                if (totalBal1 < 450)
                {
                    // 拿掉 本行戶名已更名XXX
                    List<string> newDocMemo = new List<string>();
                    foreach (var s in DocMemo)
                    {
                        if (!s.Contains("本行戶名已更名為"))
                            newDocMemo.Add(s);
                    }
                    if (totalBal1 >= 0) // 若等0, 則不秀此訊息
                        newDocMemo.Add("扣除手續費存款餘額未達200元，不予扣押。");
                    asBiz.insertCaseMemo(caseDoc.CaseId, "CaseSeizure", newDocMemo, eQueryStaff);
                    foreach (var s in calculatedAccList)
                    {
                        s.planSeizure = 0.0m;
                        s.showSeizure = 0.0m;
                    }
                    WriteLog("備註文字: " + string.Join(",", DocMemo.Select(x => x)));
                    asBiz.insertCaseSeizure2(caseDoc, calculatedAccList, eQueryStaff, strCalcResult);
                    SeizureResult = "0000|扣除手續費存款餘額未達200元，不予扣押。";


                    return SeizureResult;
                }


            }

            #endregion

            #region 若有任一個帳戶 TD,OD, 放款 , 需落人工, 則落人工

            if (calculatedAccList.Any(x => x.planSeizure > 0 && (x.isOD || x.isTD || x.isLon)))
            {
                foreach (var a in calculatedAccList.Where(x => x.planSeizure > 0 && (x.isOD || x.isTD || x.isLon)))
                {
                    if (a.isOD)
                    {
                        //--- 找出透支額度
                        string amt = getODTDLonAmt( a.Account, "OD");
                        DocMemo.Add("帳戶" + a.Account.ToString() + "有透支餘額 " + amt.ToString() + "，請確認。");
                    }
                    if (a.isTD)
                    {
                        //-- 找出透支額度
                        string amt = getODTDLonAmt( a.Account, "TD");
                        DocMemo.Add("帳戶" + a.Account.ToString() + "有質借餘額 " + amt.ToString() + "，請確認。");
                    }
                    if (a.isLon)
                    {
                        //-- 找出透支額度
                        string amt = getODTDLonAmt( a.Account, "Lon");
                        DocMemo.Add("帳戶" + a.Account.ToString() + "有放款額度 " + amt.ToString() + "，請確認。");
                    }
                }
                foreach (var s in OrderedAccLists)
                {
                    s.planSeizure = 0.0m;
                    s.showSeizure = 0.0m;
                }
                SeizureResult = "0025|有OD, TD, 放款帳戶, 要扣押, 落人工";
                asBiz.insertCaseMemo(caseDoc.CaseId, "CaseSeizure", DocMemo, eQueryStaff);
                asBiz.insertCaseSeizure2(caseDoc, OrderedAccLists, eQueryStaff, "0025|有OD, TD, 放款帳戶, 要扣押, 落人工");
                WriteLog("備註文字: " + string.Join(",", DocMemo.Select(x => x)));
                return SeizureResult;
            }
            #endregion


            var totalBal2 = calculatedAccList.Sum(x => x.Twd);

            #region 若不在可扣押的帳戶ProdCode, 且未扣押足額,  但還有其他帳戶有錢, 則要落人工

            // 若可被扣押的帳戶, 都扣完了, 且, 還有 其他帳戶, 則才需要落人工
            // 若所有帳戶數量<> 可被扣押的數量+ 絕對不可被扣押的數量，則落人工
            // 如果exceptAccountList.count() > 0 , 表示有一些帳戶, 非正向及負向表列的帳戶, 一律落人工
            // exceptAccountList , 表示正面表列選到的留下的其他帳戶
            var exceptAccountList = allAccLists.Except(OrderedAccLists).ToList();
            if (exceptAccountList.Count() > 0)
            {
                WriteLog("還有其他帳戶可能可以扣押(非正面列表的ProdCode) : " + string.Join(",", exceptAccountList.Select(x => x.Account)));
                decimal tot2 = exceptAccountList.Where(x => x.Bal > 0).Sum(x => x.Bal);
                if (tot2 > 0 && totalBal2 < SeizureTotal)
                {
                    SeizureResult = "0026|還有其他帳戶可能可以扣押，請確認。";

                    foreach (var s in OrderedAccLists)
                    {
                        s.planSeizure = 0.0m;
                        s.showSeizure = 0.0m;
                    }

                    string accno = string.Join(",", exceptAccountList.Select(x => x.Account));
                    string prodcode = string.Join(",", exceptAccountList.Select(x => x.ProdCode));
                    // 還要把被排除的帳戶加回去OrderedAccLists...
                    foreach (var a in exceptAccountList)
                    {
                        a.ProdCode = "ZZZZ";
                        a.Rate = gdicCurrency[a.Ccy];
                        OrderedAccLists.Add(a);
                    }

                    DocMemo.Add("還有其他帳戶 " + accno + " 可能可以扣押, 代碼: (" + prodcode + ") ，請將產品代碼加入參數設定。");
                    asBiz.insertCaseMemo(caseDoc.CaseId, "CaseSeizure", DocMemo, eQueryStaff);
                    asBiz.insertCaseSeizure2(caseDoc, OrderedAccLists, eQueryStaff, "0026|還有其他帳戶可能可以扣押，請確認。");
                    WriteLog("備註文字: " + string.Join(",", DocMemo.Select(x => x)));
                    return SeizureResult;
                }
            }


            #endregion

            #region 若總扣押金額<450 元, 則不進行扣押, 若大於450, 則扣押

            DocMemo.AddRange(calculatedAccList.Where(x => x.planSeizure>0 && !string.IsNullOrEmpty(x.Memo)).Select(x => x.Memo));

            if (totalBal2 < 450)
            {
                foreach (var s in calculatedAccList)
                {
                    s.planSeizure = 0.0m;
                    s.showSeizure = 0.0m;
                }
                List<string> newDocMemo = new List<string>() { "扣除手續費存款餘額未達200元，不予扣押。" };
                asBiz.insertCaseMemo(caseDoc.CaseId, "CaseSeizure", newDocMemo, eQueryStaff);

                asBiz.insertCaseSeizure(caseDoc, calculatedAccList, eQueryStaff, "0023|總扣押金額<450 元");
                SeizureResult = "0023|總扣押金額<450 元";
            }
            else
            {
                // 扣押動作
                //DocMemo.AddRange(calculatedAccList.Where(x => !string.IsNullOrEmpty(x.Memo)).Select(x => x.Memo));

                string errorAccount = null;  

                // ------------------------------------- 20181011 -------------------------
                // 目前測試版本, 先不扣押
                var result = SeizureSetting(calculatedAccList, govNo, ref errorAccount);
                //WriteLog("**********提醒目前並未執行真正扣押動作**************");
                //string result = "0000|";
                //-------------------------------------------------------------------------------

                if (result.StartsWith("0002|"))
                {
                    string retMessage = result.Replace("0002|", "");
                    DocMemo.Add(retMessage);
                    // 寫入CaseMemo, 然後return
                    asBiz.insertCaseMemo(caseDoc.CaseId, "CaseSeizure", DocMemo, eQueryStaff);

                    // 要把已扣押的, 寫入CaseSeizure,
                    bool startToZero = false;
                    foreach (var acc in OrderedAccLists.Where(x => x.planSeizure != 0))
                    {
                        if (acc.Account == errorAccount)
                        {
                            startToZero = true;
                        }
                        if (startToZero)
                        {
                            acc.planSeizure = 0.0m;
                            acc.showSeizure = 0.0m;
                        }
                    }
                    asBiz.insertCaseSeizure(caseDoc, OrderedAccLists, eQueryStaff, result);

                    return "0028|" + retMessage;
                }

                if (result.StartsWith("0033|案例特殊")) // "0033|案例特殊, 若A無往來, B戶名不符, 則都不扣押, 要直接回文"
                {
                    // 要Inser B戶名不符的帳戶
                    //asBiz.insertCaseSeizure(caseDoc, OrderedAccLists, eQueryStaff, "0003|部分扣押, 則寫入全部帳戶");
                    // 要把目前所有OrderedAccList清空
                    OrderedAccLists = new List<ObligorAccount>();
                    // 改為成功, 才會直接回文
                    result = "0000|扣押成功";
                }
                



                SeizureResult = result;
                #region 寫回CaseMaster

                WriteLog(string.Format("Step 6-5 : 新增CaseSeizure及CaseMemo"));
                // 1. 要寫回CaseSeizre.Seq 扣押順序 // 0, 初始, 1:扣押完成 2. 支付完成                    
                //strCalcResult

                if (totalBal2 < SeizureTotal) // 若只是部分扣押, 則寫入全部帳戶
                {
                    asBiz.insertCaseSeizure(caseDoc, OrderedAccLists, eQueryStaff, "0003|部分扣押, 則寫入全部帳戶");
                }
                else
                {
                    asBiz.insertCaseSeizure(caseDoc, OrderedAccLists, eQueryStaff, SeizureResult);
                }



                realSeizureAmt = OrderedAccLists.Where(x => x.planSeizure > 0).Sum(x => (decimal)x.Twd);
                WriteLog(string.Format("Step 6-6 : 個人戶扣押總金額, 折合台幣{0}", realSeizureAmt.ToString("###,###,###")));
                // 2. 寫回CaseMemo.. 把備註寫回去... MemoType='CaseMemo'
                asBiz.insertCaseMemo(caseDoc.CaseId, "CaseSeizure", DocMemo, eQueryStaff);
                #endregion
            }
            #endregion
            WriteLog("備註文字: " + string.Join(",", DocMemo.Select(x => x)));
            WriteLog(string.Format("Step 7 : 扣押結果{0}", SeizureResult));


            return SeizureResult;
        }

        private static string getODTDLonAmt(string acc, string type)
        {
            string result = "0";
            // type, 只接受OD, TD, Lon
            result = asBiz.getODTDLonAmt(acc, type);

            return result.Trim();
        }






        //@@@! 公司戶扣押... 20180808 
        private static string CompanySeizure(CaseMaster caseDoc, List<ObligorAccount> AccLists, Dictionary<string, List<string>> SeizureOrder, PARMCodeBIZ pbiz, int HandleFee, decimal SeizureTotal, string govNo, ref List<string> DocMemo, bool doubleIDNameNotConsistent, bool AccountNameNOTConsistent, bool isRename, bool isAmountBelow450, List<ObligorAccount> allAccLists, string doubleID, List<CaseObligor> VaildObligors, ref decimal realSeizureAmt)
        {
            WriteLog(string.Format("\tStep 6-3: 只扣押公司戶"));

            List<ObligorAccount> OrderedAccLists = new List<ObligorAccount>();
            string strCalcResult = "";
            string SeizureResult = "";
            var cOrder = getCompanyOrder(AccLists, SeizureOrder);

            // 因為發生了, 0019(台幣), 跟XX19(外幣), 會造成二筆, 所以要刪除那一筆
            cOrder = distinctPOrder(cOrder);
            OrderedAccLists.AddRange(cOrder);

            // 針對這些個人帳戶, 去扣押
            realSeizureAmt = 0.0m;
            List<ObligorAccount> calculatedAccList = CalcSeizureAmt_Company(OrderedAccLists, pbiz, HandleFee, SeizureTotal, govNo, VaildObligors, ref realSeizureAmt, ref strCalcResult);


            #region 若來文金額異常, 落人工, 但要InsertCaseSeizre
            if (isAmountBelow450)
            {
                SeizureResult = "0030|來函扣押金額異常(請查明)。";
                DocMemo.Add("來函扣押金額異常(請查明)。");
                asBiz.insertCaseMemo(caseDoc.CaseId, "CaseSeizure", DocMemo, eQueryStaff);
                foreach (var s in calculatedAccList)
                {
                    s.planSeizure = 0.0m;
                    s.showSeizure = 0.0m;
                }
                asBiz.insertCaseSeizure2(caseDoc, calculatedAccList, eQueryStaff, SeizureResult);
                return SeizureResult;
            }


            #endregion
            #region 如果有事故11,12,13,14 , 要加備註, 會不要落人工

            if (calculatedAccList.Any(x => x.planSeizure > 0 && x.is450OK && (x.message450Code.Trim().Contains("11") || x.message450Code.Trim().Contains("12") || x.message450Code.Trim().Contains("13") || x.message450Code.Trim().Contains("14"))))
            {
                Dictionary<string, string> mess45030 = new Dictionary<string, string>()
                    {
                        {"11", "帳戶已設定為警示帳戶。"},
                        {"12", "帳戶已設定為警示帳戶。"},
                        {"13", "帳戶已設定為衍生警示帳戶。"},
                        {"14", "帳戶已設定為警示帳戶。"}
                    };
                WriteLog("\tStep 6 : 帳戶有事故 11,12,13,14 (不需落人工)，請確認");
                foreach (var s in calculatedAccList.Where(x => x.planSeizure > 0 && x.is450OK))
                {
                    if (s.message450Code.Contains("@"))
                    {
                        string[] qqq = s.message450Code.Split('@');
                        foreach (var q in qqq)
                            if (mess45030.ContainsKey(q)) DocMemo.Add(mess45030[q]);
                    }
                    else
                        DocMemo.Add(s.message450);
                }
                //Doc2Human = true;
                SeizureResult = "0022|帳戶有事故11,12,13,14 ，請確認";
                //asBiz.insertCaseMemo(caseid, "CaseSeizure", DocMemo, eQueryStaff);
                WriteLog("備註文字: " + string.Join(",", DocMemo.Select(x => x)));
            }



            #endregion


            bool isfixaccount = calculatedAccList.Any(x => x.planSeizure > 0 && (x.AccountType.Contains("定存") || x.AccountType.Contains("綜定")));


            #region 如果有事故, 4, 5,6,7,8, 10 , 要落人工
            if (calculatedAccList.Any(x => x.planSeizure > 0 && x.is450OK && (x.message450Code.Trim().Contains("04")
                || x.message450Code.Trim().Contains("05") || x.message450Code.Trim().Contains("06")
                || x.message450Code.Trim().Contains("07") || x.message450Code.Trim().Contains("08") || x.message450Code.Trim().Contains("10"))))
            {

                var seiTot = calculatedAccList.Where(x => x.planSeizure > 0).Sum(x => x.Twd);

                if (seiTot < 450)
                {

                    if (!isfixaccount)
                    {
                        //低於450元, 什麼DocMemo都不用說
                        List<string> newDocMemo = new List<string>();
                        asBiz.insertCaseMemo(caseDoc.CaseId, "CaseSeizure", newDocMemo, eQueryStaff);
                        foreach (var s in calculatedAccList)
                        {
                            s.planSeizure = 0.0m;
                            s.showSeizure = 0.0m;
                        }
                        asBiz.insertCaseSeizure2(caseDoc, calculatedAccList, eQueryStaff, "0024|有事故, 但扣押金額低於450, 所以直接回文");
                        string SeizureResult222 = "0024|有事故, 但扣押金額低於450, 所以直接回文";
                        return SeizureResult222;
                    }
                }


                Dictionary<string, string> mess45030 = new Dictionary<string, string>()
                    {
                        {"04", "帳戶因他案已凍結在案。(請查明)"},
                        {"05", "帳戶因他案已設定質權在案。(請查明)"},
                        {"06", "帳戶因他案已設定質權在案。(請查明)"},
                        {"07", "帳戶已設為支存拒往戶。(請查明)"},
                        {"08", "帳戶因本票中止契約。(請查明)"},
                        {"10", "帳戶因其他原因已凍結在案。(請查明)"}
                    };
                WriteLog("\tStep 6 : 帳戶有事故4, 5,6,7,8, 10 , 要落人工，請確認");
                foreach (var s in calculatedAccList.Where(x => x.planSeizure > 0 && x.is450OK))
                {
                    if (s.message450Code.Contains("@"))
                    {
                        string[] qqq = s.message450Code.Split('@');
                        foreach (var q in qqq)
                            if (mess45030.ContainsKey(q)) DocMemo.Add(mess45030[q]);
                    }
                    else
                        DocMemo.Add(s.message450);
                }
                //Doc2Human = true;
                SeizureResult = "0022|帳戶有事故4, 5,6,7,8, 10，請確認";

                if (!isfixaccount)
                {
                    asBiz.insertCaseMemo(caseDoc.CaseId, "CaseSeizure", DocMemo, eQueryStaff);
                    foreach (var s in calculatedAccList)
                    {
                        s.planSeizure = 0.0m;
                        s.showSeizure = 0.0m;
                    }
                    WriteLog("備註文字: " + string.Join(",", DocMemo.Select(x => x)));
                    asBiz.insertCaseSeizure2(caseDoc, calculatedAccList, eQueryStaff, SeizureResult);
                    return SeizureResult;
                }
            }

            #endregion


            #region 如果, 有扣要定存類, 則要落人工
            // 綜定 定存 外幣定存
            if (calculatedAccList.Any(x => x.planSeizure > 0 && (x.AccountType.Contains("定存") || x.AccountType.Contains("綜定"))))
            {
                WriteLog("\tStep 6 : 有扣到定存類，請確認");
                var fixacc = calculatedAccList.Where(x => x.planSeizure > 0 && (x.AccountType.Contains("定存") || x.AccountType.Contains("綜定"))).First();
                DocMemo.Add(string.Format("定存{0} 需要被扣押到，請確認", fixacc.Account));
                //Doc2Human = true;
                SeizureResult = "0021|有扣到定存類，請確認";
                asBiz.insertCaseMemo(caseDoc.CaseId, "CaseSeizure", DocMemo, eQueryStaff);
                foreach (var s in calculatedAccList)
                {
                    s.planSeizure = 0.0m;
                    s.showSeizure = 0.0m;
                }
                WriteLog("備註文字: " + string.Join(",", DocMemo.Select(x => x)));
                asBiz.insertCaseSeizure2(caseDoc, calculatedAccList, eQueryStaff, SeizureResult);
                return SeizureResult;
            }


            #endregion


            #region 重號的戶名與來文名不同, 要提示 足額, 或不足額
            if (doubleIDNameNotConsistent)
            {
                SeizureResult = "0013|重號的戶名不符。";
                List<string> sameIds = VaildObligors.Where(x => x.isSame).Select(x => x.ObligorNo).ToList();


                var totalBalNotSame = calculatedAccList.Where(x => !sameIds.Contains(x.Id)).Sum(x => x.Twd);
                if (totalBalNotSame >= 450)
                    DocMemo.Add("另查，該身分證統一編號於本行尚有另一帳戶開立之戶名與來函戶名不符。");
                else
                    DocMemo.Add("另查，該身分證統一編號於本行尚有另一帳戶開立之戶名與來函戶名不符且存款餘額未達新臺幣200元。");


                var totalBal = calculatedAccList.Where(x => sameIds.Contains(x.Id)).Sum(x => x.Twd);
                if (totalBal >= 450)
                {

                    foreach (var s in calculatedAccList.Where(x => !sameIds.Contains(x.Id))) // 把不可扣押的ID, 設為0元
                    {
                        s.planSeizure = 0.0m;
                        s.showSeizure = 0.0m;
                    }


                    string errorAccount = null;
                    var result = SeizureSetting(calculatedAccList, govNo, ref errorAccount);

                    #region 0002扣押中, 有錯

                    if (result.StartsWith("0002|"))
                    {
                        string retMessage = result.Replace("0002|", "");
                        DocMemo.Add(retMessage);
                        // 寫入CaseMemo, 然後return
                        asBiz.insertCaseMemo(caseDoc.CaseId, "CaseSeizure", DocMemo, eQueryStaff);

                        // 要把已扣押的, 寫入CaseSeizure,
                        bool startToZero = false;
                        foreach (var acc in OrderedAccLists.Where(x => x.planSeizure != 0))
                        {
                            if (acc.Account == errorAccount)
                            {
                                startToZero = true;
                            }
                            if (startToZero)
                            {
                                acc.planSeizure = 0.0m;
                                acc.showSeizure = 0.0m;
                            }
                        }
                    }
                    #endregion

                    // 要分足額扣押, 或是部分扣押
                    if (SeizureTotal <= totalBal && totalBal >= 450) // 足額
                    {
                        WriteLog("備註文字: " + string.Join(",", DocMemo.Select(x => x)));
                        // 若是足額, 則不需要"另查XXXX" 
                        var newMemo = DocMemo.Where(x => !x.Contains("另查，該身分")).ToList();
                        asBiz.insertCaseMemo(caseDoc.CaseId, "CaseSeizure", newMemo, eQueryStaff);
                        asBiz.insertCaseSeizure(caseDoc, OrderedAccLists, eQueryStaff, "0000|足額扣押");
                    }
                    else
                    {
                        WriteLog("備註文字: " + string.Join(",", DocMemo.Select(x => x)));
                        asBiz.insertCaseMemo(caseDoc.CaseId, "CaseSeizure", DocMemo, eQueryStaff);
                        asBiz.insertCaseSeizure(caseDoc, OrderedAccLists, eQueryStaff, "0001|不足額扣押");
                    }
                    return result;
                }
                else
                {

                    List<string> newDocMemo = new List<string>();
                    foreach (var m in DocMemo)
                    {
                        if (!m.Contains("本行戶名已更名為"))
                        {
                            newDocMemo.Add(m);
                        }
                    }
                    foreach (var s in calculatedAccList)
                    {
                        s.planSeizure = 0.0m;
                        s.showSeizure = 0.0m;
                    }
                    WriteLog("備註文字: " + string.Join(",", DocMemo.Select(x => x)));
                    asBiz.insertCaseMemo(caseDoc.CaseId, "CaseSeizure", newDocMemo, eQueryStaff);
                    asBiz.insertCaseSeizure2(caseDoc, calculatedAccList, eQueryStaff, SeizureResult);
                    return SeizureResult;
                }
            }

            #endregion


            #region 若戶名不符, 則要決定, 餘額超過450或低於450元
            if (AccountNameNOTConsistent) // 戶名不符
            {
                SeizureResult = "0013|戶名不符。";
                var totalBal = calculatedAccList.Sum(x => x.Twd);
                if (totalBal >= 450)
                    DocMemo.Add("戶名不符。");
                else
                    DocMemo.Add("戶名不符且存款餘額未達200元，不予扣押。");

                foreach (var s in calculatedAccList)
                {
                    s.planSeizure = 0.0m;
                    s.showSeizure = 0.0m;
                }
                WriteLog("備註文字: " + string.Join(",", DocMemo.Select(x => x)));
                asBiz.insertCaseMemo(caseDoc.CaseId, "CaseSeizure", DocMemo, eQueryStaff);
                asBiz.insertCaseSeizure2(caseDoc, calculatedAccList, eQueryStaff, SeizureResult);
                return SeizureResult;






            }

            #endregion

            #region 若有新名子, 且金額大於450元, 要提示新名字
            if (isRename) // 有新名字, 要提示
            {

                var totalBal1 = calculatedAccList.Sum(x => x.Twd);
                if (totalBal1 < 450)
                {
                    // 拿掉 本行戶名已更名XXX
                    List<string> newDocMemo = new List<string>();
                    foreach (var s in DocMemo)
                    {
                        if (!s.Contains("本行戶名已更名為"))
                            newDocMemo.Add(s);
                    }
                    if (totalBal1 >= 0) // 若等0, 則不秀此訊息
                        newDocMemo.Add("扣除手續費存款餘額未達200元，不予扣押。");
                    asBiz.insertCaseMemo(caseDoc.CaseId, "CaseSeizure", newDocMemo, eQueryStaff);
                    foreach (var s in calculatedAccList)
                    {
                        s.planSeizure = 0.0m;
                        s.showSeizure = 0.0m;
                    }
                    WriteLog("備註文字: " + string.Join(",", DocMemo.Select(x => x)));
                    asBiz.insertCaseSeizure2(caseDoc, calculatedAccList, eQueryStaff, strCalcResult);
                    SeizureResult = "0000|扣除手續費存款餘額未達200元，不予扣押。";
                    return SeizureResult;
                }


            }

            #endregion

            #region 若有任一個帳戶 TD,OD, 放款 , 需落人工, 則落人工

            if (calculatedAccList.Any(x => x.planSeizure > 0 && (x.isOD || x.isTD || x.isLon)))
            {
                foreach (var a in calculatedAccList.Where(x => x.planSeizure > 0 && (x.isOD || x.isTD || x.isLon)))
                {
                    if (a.isOD) DocMemo.Add("帳戶有透支餘額，請確認。");
                    if (a.isTD) DocMemo.Add("帳戶有質借餘額，請確認。");
                    if (a.isLon) DocMemo.Add("帳戶有放款，請確認。");
                }
                foreach (var s in OrderedAccLists)
                {
                    s.planSeizure = 0.0m;
                    s.showSeizure = 0.0m;
                }
                SeizureResult = "0025|有OD, TD, 放款帳戶, 要扣押, 落人工";
                asBiz.insertCaseMemo(caseDoc.CaseId, "CaseSeizure", DocMemo, eQueryStaff);
                asBiz.insertCaseSeizure2(caseDoc, OrderedAccLists, eQueryStaff, "0025|有OD, TD, 放款帳戶, 要扣押, 落人工");
                WriteLog("備註文字: " + string.Join(",", DocMemo.Select(x => x)));
                return SeizureResult;
            }
            #endregion


            var totalBal2 = calculatedAccList.Sum(x => x.Twd);

            #region 若不在可扣押的帳戶ProdCode, 且未扣押足額,  但還有其他帳戶有錢, 則要落人工

            // 若可被扣押的帳戶, 都扣完了, 且, 還有 其他帳戶, 則才需要落人工
            // 若所有帳戶數量<> 可被扣押的數量+ 絕對不可被扣押的數量，則落人工
            // 如果exceptAccountList.count() > 0 , 表示有一些帳戶, 非正向及負向表列的帳戶, 一律落人工
            // exceptAccountList , 表示正面表列選到的留下的其他帳戶
            var exceptAccountList = allAccLists.Except(OrderedAccLists).ToList();
            if (exceptAccountList.Count() > 0 && SeizureTotal > totalBal2) // 並且要不足額, 才可以提示
            {
                WriteLog("還有其他帳戶可能可以扣押(非正面列表的ProdCode) : " + string.Join(",", exceptAccountList.Select(x => x.Account)));
                decimal tot2 = exceptAccountList.Where(x => x.Bal > 0).Sum(x => x.Bal);
                if (tot2 > 0 && totalBal2 < SeizureTotal)
                {
                    SeizureResult = "0026|還有其他帳戶可能可以扣押，請確認。";

                    foreach (var s in OrderedAccLists)
                    {
                        s.planSeizure = 0.0m;
                        s.showSeizure = 0.0m;
                    }

                    string accno = string.Join(",", exceptAccountList.Select(x => x.Account));
                    string prodcode = string.Join(",", exceptAccountList.Select(x => x.ProdCode));
                    // 還要把被排除的帳戶加回去OrderedAccLists...
                    foreach (var a in exceptAccountList)
                    {
                        a.ProdCode = "ZZZZ";                        
                        a.Rate = gdicCurrency[a.Ccy];
                        OrderedAccLists.Add(a);
                    }

                    DocMemo.Add("還有其他帳戶 " + accno + " 可能可以扣押, 代碼: (" + prodcode + ") ，請將產品代碼加入參數設定。");
                    asBiz.insertCaseMemo(caseDoc.CaseId, "CaseSeizure", DocMemo, eQueryStaff);
                    asBiz.insertCaseSeizure2(caseDoc, OrderedAccLists, eQueryStaff, "0026|還有其他帳戶可能可以扣押，請確認。");
                    WriteLog("備註文字: " + string.Join(",", DocMemo.Select(x => x)));
                    return SeizureResult;
                }
            }


            #endregion

            #region 若總扣押金額<450 元, 則不進行扣押, 若大於450, 則扣押

            DocMemo.AddRange(calculatedAccList.Where(x => x.planSeizure>0 &&  !string.IsNullOrEmpty(x.Memo)).Select(x => x.Memo));

            if (totalBal2 < 450)
            {
                foreach (var s in calculatedAccList)
                {
                    s.planSeizure = 0.0m;
                    s.showSeizure = 0.0m;
                }
                List<string> newDocMemo = new List<string>() { "扣除手續費存款餘額未達200元，不予扣押。" };
                asBiz.insertCaseMemo(caseDoc.CaseId, "CaseSeizure", newDocMemo, eQueryStaff);

                asBiz.insertCaseSeizure(caseDoc, calculatedAccList, eQueryStaff, "0023|總扣押金額<450 元");
                SeizureResult = "0023|總扣押金額<450 元";
            }
            else
            {
                // 扣押動作
                //DocMemo.AddRange(calculatedAccList.Where(x => !string.IsNullOrEmpty(x.Memo)).Select(x => x.Memo));

                string errorAccount = null;
                var result = SeizureSetting(calculatedAccList, govNo, ref errorAccount);



                if (result.StartsWith("0002|"))
                {
                    string retMessage = result.Replace("0002|", "");
                    DocMemo.Add(retMessage);
                    // 寫入CaseMemo, 然後return
                    asBiz.insertCaseMemo(caseDoc.CaseId, "CaseSeizure", DocMemo, eQueryStaff);

                    // 要把已扣押的, 寫入CaseSeizure,
                    bool startToZero = false;
                    foreach (var acc in OrderedAccLists.Where(x => x.planSeizure != 0))
                    {
                        if (acc.Account == errorAccount)
                        {
                            startToZero = true;
                        }
                        if (startToZero)
                        {
                            acc.planSeizure = 0.0m;
                            acc.showSeizure = 0.0m;
                        }
                    }
                    asBiz.insertCaseSeizure(caseDoc, OrderedAccLists, eQueryStaff, result);

                    return "0028|" + retMessage;
                }


                SeizureResult = result;
                #region 寫回CaseMaster

                WriteLog(string.Format("Step 6-5 : 新增CaseSeizure及CaseMemo"));
                // 1. 要寫回CaseSeizre.Seq 扣押順序 // 0, 初始, 1:扣押完成 2. 支付完成                    
                //strCalcResult

                if (totalBal2 < SeizureTotal) // 若只是部分扣押, 則寫入全部帳戶
                {
                    asBiz.insertCaseSeizure(caseDoc, OrderedAccLists, eQueryStaff, "0003|部分扣押, 則寫入全部帳戶");
                }
                else
                {
                    asBiz.insertCaseSeizure(caseDoc, OrderedAccLists, eQueryStaff, SeizureResult);
                }

                realSeizureAmt = OrderedAccLists.Where(x => x.planSeizure > 0).Sum(x => (decimal)x.Twd);
                WriteLog(string.Format("Step 6-6 : 個人戶扣押總金額, 折合台幣{0}", realSeizureAmt.ToString("###,###,###")));
                // 2. 寫回CaseMemo.. 把備註寫回去... MemoType='CaseMemo'
                asBiz.insertCaseMemo(caseDoc.CaseId, "CaseSeizure", DocMemo, eQueryStaff);
                #endregion
            }
            #endregion
            WriteLog("備註文字: " + string.Join(",", DocMemo.Select(x => x)));
            WriteLog(string.Format("Step 7 : 扣押結果{0}", SeizureResult));



            return SeizureResult;

        }

        /// <summary>
        /// 當落人工時, 要把!@!的備註, 換成USER看的懂的文字
        /// </summary>
        /// <param name="DocMemo"></param>
        private static List<string> detectTypeDifHumanProc(List<string> DocMemo)
        {
            List<string> NewMemo = new List<string>();
            foreach(var a in DocMemo)
            {
                if (a.Contains("!@!"))
                {
                    if (a.Contains("!@!經查於本行負責人戶名不符"))
                    { // DocMemo.Add("經查來函ID與本行留存負責人ID : " + c.RespId + " 不同。");
                        int ipos = a.IndexOf("!@!經查於本行負責人戶名不符");
                        if( ipos>0 )
                        {
                            string ids = a.Substring(0, ipos);
                            NewMemo.Add("經查來函ID與本行留存負責人ID :" + ids + " 不同。");
                        }
                        

                    }
                    if (a.Contains("!@!經查於本行負責人不同，故無執行扣押"))
                    {
                        int ipos = a.IndexOf("!@!經查於本行負責人不同，故無執行扣押");
                        if (ipos > 0)
                        {
                            string ids = a.Substring(0, ipos);
                            NewMemo.Add("經查於本行負責人不同，故無執行扣押。");
                        }
                    }
                    if (a.Contains("!@!戶名不符"))
                    {
                        NewMemo.Add("戶名不符。");
                    }
                }
                else
                    NewMemo.Add(a);
            }
            return NewMemo;
        }

        // 20180921 新版
        private static string CompanySeizure2(CaseMaster caseDoc, List<ObligorAccount> AccLists, Dictionary<string, List<string>> SeizureOrder, PARMCodeBIZ pbiz, int HandleFee, decimal SeizureTotal, string govNo, ref List<string> DocMemo,  bool isAmountBelow450, List<ObligorAccount> allAccLists, string doubleID, List<CaseObligor> VaildObligors, ref decimal realSeizureAmt)
        {
            WriteLog(string.Format("\tStep 6-3: 只扣押公司戶"));

            List<ObligorAccount> OrderedAccLists = new List<ObligorAccount>();
            string strCalcResult = "";
            string SeizureResult = "";
            var cOrder = getCompanyOrder(AccLists, SeizureOrder);

            // 因為發生了, 0019(台幣), 跟XX19(外幣), 會造成二筆, 所以要刪除那一筆
            cOrder = distinctPOrder(cOrder);
            OrderedAccLists.AddRange(cOrder);

            // 針對這些個人帳戶, 去扣押
            realSeizureAmt = 0.0m;
            List<ObligorAccount> calculatedAccList = CalcSeizureAmt_Company(OrderedAccLists, pbiz, HandleFee, SeizureTotal, govNo, VaildObligors, ref realSeizureAmt, ref strCalcResult);



            //#region 20181015, 若是行號, 又是重號, 要把AccLists 中ID合併成本號
            //if (calculatedAccList.Any(x => x.CoType == "D"))
            //{
            //    var gid = (from p in calculatedAccList.Where(x => !x.isDoubleAcc) group p by p.Id into g select g.Key).ToList();
            //    foreach (var g in gid)
            //    {
            //        foreach (var acc in calculatedAccList.Where(x => x.Id.StartsWith(g)))
            //        {
            //            acc.Id = g;
            //        }
            //    }
            //}
            //#endregion



            bool isHumanProc = false;

            var writeToCaseSeizure = calculatedAccList;

            #region 若來文金額異常, 落人工, 但要InsertCaseSeizre
            if (isAmountBelow450)
            {
                SeizureResult = "0030|來函扣押金額異常(請查明)。";
                DocMemo.Add("來函扣押金額異常(請查明)。");

                DocMemo = detectTypeDifHumanProc(DocMemo);

                asBiz.insertCaseMemo(caseDoc.CaseId, "CaseSeizure", DocMemo, eQueryStaff);
                foreach (var s in calculatedAccList)
                {
                    s.planSeizure = 0.0m;
                    s.showSeizure = 0.0m;
                }
                asBiz.insertCaseSeizure2(caseDoc, calculatedAccList, eQueryStaff, SeizureResult);
                return SeizureResult;
            }


            #endregion
            #region 如果有事故11,12,13,14 , 要加備註, 會不要落人工

            if (calculatedAccList.Any(x => x.planSeizure > 0 && x.is450OK && (x.message450Code.Trim().Contains("11") || x.message450Code.Trim().Contains("12") || x.message450Code.Trim().Contains("13") || x.message450Code.Trim().Contains("14"))))
            {
                Dictionary<string, string> mess45030 = new Dictionary<string, string>()
                    {
                        {"11", "帳戶已設定為警示帳戶。"},
                        {"12", "帳戶已設定為警示帳戶。"},
                        {"13", "帳戶已設定為衍生警示帳戶。"},
                        {"14", "帳戶已設定為警示帳戶。"}
                    };
                WriteLog("\tStep 6 : 帳戶有事故 11,12,13,14 (不需落人工)，請確認");
                foreach (var s in calculatedAccList.Where(x => x.planSeizure > 0 && x.is450OK))
                {
                    if (s.message450Code.Contains("@"))
                    {
                        string[] qqq = s.message450Code.Split('@');
                        foreach (var q in qqq)
                            if (mess45030.ContainsKey(q)) DocMemo.Add(mess45030[q]);
                    }
                    else
                        DocMemo.Add(s.message450);
                }
                //Doc2Human = true;
                SeizureResult = "0022|帳戶有事故11,12,13,14 ，請確認";
                //asBiz.insertCaseMemo(caseid, "CaseSeizure", DocMemo, eQueryStaff);
                WriteLog("備註文字: " + string.Join(",", DocMemo.Select(x => x)));
            }



            #endregion


            bool isfixaccount = calculatedAccList.Any(x => x.planSeizure > 0 && (x.AccountType.Contains("定存") || x.AccountType.Contains("綜定")));

            #region 若有任一個帳戶 TD,OD, 放款 , 需落人工, 則落人工

            if (calculatedAccList.Any(x => x.planSeizure > 0 && (x.isOD || x.isTD || x.isLon)))
            {
                foreach (var a in calculatedAccList.Where(x => x.planSeizure > 0 && (x.isOD || x.isTD || x.isLon)))
                {
                    if (a.isOD) DocMemo.Add("帳戶有透支餘額，請確認。");
                    if (a.isTD) DocMemo.Add("帳戶有質借餘額，請確認。");
                    if (a.isLon) DocMemo.Add("帳戶有放款，請確認。");
                }
                //foreach (var s in calculatedAccList)
                //{
                //    s.planSeizure = 0.0m;
                //    s.showSeizure = 0.0m;
                //}
                SeizureResult = "0025|有OD, TD, 放款帳戶, 要扣押, 落人工";

                DocMemo = detectTypeDifHumanProc(DocMemo);
                asBiz.insertCaseMemo(caseDoc.CaseId, "CaseSeizure", DocMemo, eQueryStaff);
                WriteLog("備註文字: " + string.Join(",", DocMemo.Select(x => x)));
                //asBiz.insertCaseSeizure2(caseDoc, calculatedAccList, eQueryStaff, "0025|有OD, TD, 放款帳戶, 要扣押, 落人工");
                isHumanProc = true;
                //return SeizureResult;
            }
            #endregion

            #region 如果有事故, 4, 5,6,7,8, 10 , 要落人工
            if (calculatedAccList.Any(x => x.planSeizure > 0 && x.is450OK && (x.message450Code.Trim().Contains("04")
                || x.message450Code.Trim().Contains("05") || x.message450Code.Trim().Contains("06")
                || x.message450Code.Trim().Contains("07") || x.message450Code.Trim().Contains("08") || x.message450Code.Trim().Contains("10"))))
            {

                var seiTot = calculatedAccList.Where(x => x.planSeizure > 0).Sum(x => x.Twd);

                if (seiTot < 450)
                {

                    if (!isfixaccount)
                    {
                        //低於450元, 什麼DocMemo都不用說
                        List<string> newDocMemo = new List<string>();
                        asBiz.insertCaseMemo(caseDoc.CaseId, "CaseSeizure", newDocMemo, eQueryStaff);
                        foreach (var s in calculatedAccList)
                        {
                            s.planSeizure = 0.0m;
                            s.showSeizure = 0.0m;
                        }
                        asBiz.insertCaseSeizure2(caseDoc, calculatedAccList, eQueryStaff, "0024|有事故, 但扣押金額低於450, 所以直接回文");
                        string SeizureResult222 = "0024|有事故, 但扣押金額低於450, 所以直接回文";
                        return SeizureResult222;
                    }
                }


                Dictionary<string, string> mess45030 = new Dictionary<string, string>()
                    {
                        {"04", "帳戶因他案已凍結在案。(請查明)"},
                        {"05", "帳戶因他案已設定質權在案。(請查明)"},
                        {"06", "帳戶因他案已設定質權在案。(請查明)"},
                        {"07", "帳戶已設為支存拒往戶。(請查明)"},
                        {"08", "帳戶因本票中止契約。(請查明)"},
                        {"10", "帳戶因其他原因已凍結在案。(請查明)"}
                    };
                WriteLog("\tStep 6 : 帳戶有事故4, 5,6,7,8, 10 , 要落人工，請確認");
                foreach (var s in calculatedAccList.Where(x => x.planSeizure > 0 && x.is450OK))
                {
                    if (s.message450Code.Contains("@"))
                    {
                        string[] qqq = s.message450Code.Split('@');
                        foreach (var q in qqq)
                            if (mess45030.ContainsKey(q)) DocMemo.Add(mess45030[q]);
                    }
                    else
                        DocMemo.Add(s.message450);
                }
                //Doc2Human = true;
                SeizureResult = "0022|帳戶有事故4, 5,6,7,8, 10，請確認";

                if (!isfixaccount)
                {
                    DocMemo = detectTypeDifHumanProc(DocMemo);

                    asBiz.insertCaseMemo(caseDoc.CaseId, "CaseSeizure", DocMemo, eQueryStaff);
                    //foreach (var s in calculatedAccList)
                    //{
                    //    s.planSeizure = 0.0m;
                    //    s.showSeizure = 0.0m;
                    //}
                    WriteLog("備註文字: " + string.Join(",", DocMemo.Select(x => x)));
                    //asBiz.insertCaseSeizure2(caseDoc, calculatedAccList, eQueryStaff, SeizureResult);
                    //return SeizureResult;
                    isHumanProc = true;
                }
            }

            #endregion


            #region 如果, 有扣要定存類, 則要落人工
            // 綜定 定存 外幣定存
            if (calculatedAccList.Any(x => x.planSeizure > 0 && (x.AccountType.Contains("定存") || x.AccountType.Contains("綜定"))))
            {
                WriteLog("\tStep 6 : 有扣到定存類，請確認");
                var fixacc = calculatedAccList.Where(x => x.planSeizure > 0 && (x.AccountType.Contains("定存") || x.AccountType.Contains("綜定"))).First();
                DocMemo.Add(string.Format("定存{0} 需要被扣押到，請確認", fixacc.Account));
                //Doc2Human = true;
                SeizureResult = "0021|有扣到定存類，請確認";

                DocMemo = detectTypeDifHumanProc(DocMemo);

                asBiz.insertCaseMemo(caseDoc.CaseId, "CaseSeizure", DocMemo, eQueryStaff);
                WriteLog("備註文字: " + string.Join(",", DocMemo.Select(x => x)));
                //foreach (var s in calculatedAccList)
                //{
                //    s.planSeizure = 0.0m;
                //    s.showSeizure = 0.0m;
                //}
                
                //asBiz.insertCaseSeizure2(caseDoc, calculatedAccList, eQueryStaff, SeizureResult);
                //return SeizureResult;
                isHumanProc = true;
            }


            #endregion


            if( isHumanProc)
            {
                foreach (var s in calculatedAccList)
                {
                    s.planSeizure = 0.0m;
                    s.showSeizure = 0.0m;
                }
                asBiz.insertCaseSeizure2(caseDoc, calculatedAccList, eQueryStaff, SeizureResult);
                return SeizureResult;
            }


            #region 重號的戶名與來文名不同, 要提示 足額, 或不足額
            if ( calculatedAccList.Any(x=>x.isDoubleAcc && !x.isSame))
            {
                SeizureResult = "0013|重號的戶名不符。";
                List<string> sameIds = VaildObligors.Where(x => x.isSame).Select(x => x.ObligorNo).ToList();


                var totalBalNotSame = calculatedAccList.Where(x => !sameIds.Contains(x.Id)).Sum(x => x.Twd);
                if (totalBalNotSame >= 450)
                    DocMemo.Add("另查，該身分證統一編號於本行尚有另一帳戶開立之戶名與來函戶名不符。");
                else
                    DocMemo.Add("另查，該身分證統一編號於本行尚有另一帳戶開立之戶名與來函戶名不符且存款餘額未達新臺幣200元。");


                var totalBal = calculatedAccList.Where(x => sameIds.Contains(x.Id)).Sum(x => x.Twd);
                if (totalBal >= 450)
                {

                    foreach (var s in calculatedAccList.Where(x => !sameIds.Contains(x.Id))) // 把不可扣押的ID, 設為0元
                    {
                        s.planSeizure = 0.0m;
                        s.showSeizure = 0.0m;
                    }


                    string errorAccount = null;
                    var result = SeizureSetting(calculatedAccList, govNo, ref errorAccount);

                    #region 0002扣押中, 有錯

                    if (result.StartsWith("0002|"))
                    {
                        string retMessage = result.Replace("0002|", "");
                        DocMemo.Add(retMessage);
                        // 寫入CaseMemo, 然後return
                        asBiz.insertCaseMemo(caseDoc.CaseId, "CaseSeizure", DocMemo, eQueryStaff);

                        // 要把已扣押的, 寫入CaseSeizure,
                        bool startToZero = false;
                        foreach (var acc in OrderedAccLists.Where(x => x.planSeizure != 0))
                        {
                            if (acc.Account == errorAccount)
                            {
                                startToZero = true;
                            }
                            if (startToZero)
                            {
                                acc.planSeizure = 0.0m;
                                acc.showSeizure = 0.0m;
                            }
                        }
                    }
                    #endregion

                    // 要分足額扣押, 或是部分扣押
                    if (SeizureTotal <= totalBal && totalBal >= 450) // 足額
                    {
                        WriteLog("備註文字: " + string.Join(",", DocMemo.Select(x => x)));
                        // 若是足額, 則不需要"另查XXXX" 
                        var newMemo = DocMemo.Where(x => !x.Contains("另查，該身分")).ToList();
                        asBiz.insertCaseMemo(caseDoc.CaseId, "CaseSeizure", newMemo, eQueryStaff);
                        asBiz.insertCaseSeizure(caseDoc, OrderedAccLists, eQueryStaff, "0000|足額扣押");
                    }
                    else
                    {
                        WriteLog("備註文字: " + string.Join(",", DocMemo.Select(x => x)));
                        asBiz.insertCaseMemo(caseDoc.CaseId, "CaseSeizure", DocMemo, eQueryStaff);
                        asBiz.insertCaseSeizure(caseDoc, OrderedAccLists, eQueryStaff, "0001|不足額扣押");
                    }
                    return result;
                }
                else
                {

                    List<string> newDocMemo = new List<string>();
                    foreach (var m in DocMemo)
                    {
                        if (!m.Contains("本行戶名已更名為"))
                        {
                            newDocMemo.Add(m);
                        }
                    }
                    foreach (var s in calculatedAccList)
                    {
                        s.planSeizure = 0.0m;
                        s.showSeizure = 0.0m;
                    }
                    WriteLog("備註文字: " + string.Join(",", DocMemo.Select(x => x)));
                    asBiz.insertCaseMemo(caseDoc.CaseId, "CaseSeizure", newDocMemo, eQueryStaff);
                    asBiz.insertCaseSeizure2(caseDoc, calculatedAccList, eQueryStaff, SeizureResult);
                    return SeizureResult;
                }
            }

            #endregion


            #region 若戶名不符, 則要決定, 餘額超過450或低於450元
            if (calculatedAccList.Any(x =>! x.isSame && x.CoType!="C") ) // 戶名不符
            {
                SeizureResult = "0013|戶名不符。";




                var ids2 = (from p in calculatedAccList group p by p.Id into g select g.Key).ToList();
                foreach(var id in ids2)
                {
                    var totalBal = calculatedAccList.Where(x=>x.Id==id).Sum(x => x.Twd);
                    if (calculatedAccList.Any(x => x.CoType != "D"))
                    {
                        if (totalBal >= 450)
                            DocMemo.Add("戶名不符。");
                        else
                            DocMemo.Add("戶名不符且存款餘額未達200元，不予扣押。");
                    }
                    foreach (var s in calculatedAccList.Where(x=>x.Id==id))
                    {
                        s.planSeizure = 0.0m;
                        s.showSeizure = 0.0m;
                    }
                }

                WriteLog("備註文字: " + string.Join(",", DocMemo.Select(x => x)));
                asBiz.insertCaseMemo(caseDoc.CaseId, "CaseSeizure", DocMemo, eQueryStaff);
                asBiz.insertCaseSeizure2(caseDoc, calculatedAccList, eQueryStaff, SeizureResult);
                return SeizureResult;






            }

            #endregion

            #region 若有新名子, 且金額大於450元, 要提示新名字
            if ((calculatedAccList.Any(x => x.isRename))) // 有新名字, 要提示
            {
                #region 20181003, 新版的
                
                decimal allTotalBal = 0.0m;

                #region 有更名的
                List<string> newDocMemo = DocMemo.Where(x => !x.Contains("本行戶名已更名為")).ToList();
                var ids = (from p in calculatedAccList.Where(x => x.isRename) group p by p.Id into g select g.Key).ToList();
                foreach (var id in ids)
                {
                    var calbyId = calculatedAccList.Where(x => x.Id == id).ToList();
                    if( calbyId.Sum(x=>x.planSeizure) >0)
                    {
                        var totalBal1 = calbyId.Sum(x => x.Twd);
                        allTotalBal += totalBal1;
                        if (totalBal1 >= 450)
                        {
                            var f1 = calculatedAccList.Where(x => x.Id == id).First();
                            newDocMemo.Add(id + "!@!"+ "本行戶名已更名為" + f1.newName + "。");
                        }
                        else
                        {
                            foreach (var s in calculatedAccList.Where(x => x.Id == id))
                            {
                                s.planSeizure = 0.0m;
                                s.showSeizure = 0.0m;
                            }
                        }

                    }
                }

                DocMemo = newDocMemo;
                #endregion

                if( calculatedAccList.Any(x=>x.CoType=="D"))
                {}
                else
                {
                    // 20181026, IR161 , 拿掉了
                    //if (allTotalBal >= 0 && allTotalBal < 450 ) // 若等0, 則不秀此訊息
                    //{
                    //    asBiz.insertCaseMemo(caseDoc.CaseId, "CaseSeizure", newDocMemo, eQueryStaff);
                    //    newDocMemo.Add("扣除手續費存款餘額未達200元，不予扣押。");
                    //    SeizureResult = "0000|扣除手續費存款餘額未達200元，不予扣押。";
                    //    //return SeizureResult;
                    //}
                }

#endregion

                #region 20181003/  舊版的
                //var totalBal1 = calculatedAccList.Sum(x => x.Twd);
                //if (totalBal1 < 450)
                //{
                //    // 拿掉 本行戶名已更名XXX
                //    List<string> newDocMemo = new List<string>();
                //    foreach (var s in DocMemo)
                //    {
                //        if (!s.Contains("本行戶名已更名為"))
                //            newDocMemo.Add(s);
                //    }
                //    if (totalBal1 >= 0) // 若等0, 則不秀此訊息
                //        newDocMemo.Add("扣除手續費存款餘額未達200元，不予扣押。");
                //    asBiz.insertCaseMemo(caseDoc.CaseId, "CaseSeizure", newDocMemo, eQueryStaff);
                //    foreach (var s in calculatedAccList)
                //    {
                //        s.planSeizure = 0.0m;
                //        s.showSeizure = 0.0m;
                //    }
                //    WriteLog("備註文字: " + string.Join(",", DocMemo.Select(x => x)));
                //    asBiz.insertCaseSeizure2(caseDoc, calculatedAccList, eQueryStaff, strCalcResult);
                //    SeizureResult = "0000|扣除手續費存款餘額未達200元，不予扣押。";
                //    return SeizureResult;
                //}

                #endregion
            }

            #endregion



            // 20181005要被扣押的, 才要計算最後總金額...
            var totalBal2 = calculatedAccList.Where(x=>x.planSeizure>0).Sum(x => x.Twd);

            #region 若不在可扣押的帳戶ProdCode, 且未扣押足額,  但還有其他帳戶有錢, 則要落人工

            // 若可被扣押的帳戶, 都扣完了, 且, 還有 其他帳戶, 則才需要落人工
            // 若所有帳戶數量<> 可被扣押的數量+ 絕對不可被扣押的數量，則落人工
            // 如果exceptAccountList.count() > 0 , 表示有一些帳戶, 非正向及負向表列的帳戶, 一律落人工
            // exceptAccountList , 表示正面表列選到的留下的其他帳戶
            var exceptAccountList = allAccLists.Except(OrderedAccLists).ToList();
            if (exceptAccountList.Count() > 0 && SeizureTotal > totalBal2) // 並且要不足額, 才可以提示
            {
                WriteLog("還有其他帳戶可能可以扣押(非正面列表的ProdCode) : " + string.Join(",", exceptAccountList.Select(x => x.Account)));
                decimal tot2 = exceptAccountList.Where(x => x.Bal > 0).Sum(x => x.Bal);
                if (tot2 > 0 && totalBal2 < SeizureTotal)
                {
                    SeizureResult = "0026|還有其他帳戶可能可以扣押，請確認。";

                    foreach (var s in OrderedAccLists)
                    {
                        s.planSeizure = 0.0m;
                        s.showSeizure = 0.0m;
                    }

                    string accno = string.Join(",", exceptAccountList.Select(x => x.Account));
                    string prodcode = string.Join(",", exceptAccountList.Select(x => x.ProdCode));
                    // 還要把被排除的帳戶加回去OrderedAccLists...
                    foreach (var a in exceptAccountList)
                    {
                        a.ProdCode = "ZZZZ";
                        a.Rate = gdicCurrency[a.Ccy];
                        OrderedAccLists.Add(a);
                    }

                    DocMemo.Add("還有其他帳戶 " + accno + " 可能可以扣押, 代碼: (" + prodcode + ") ，請將產品代碼加入參數設定。");
                    asBiz.insertCaseMemo(caseDoc.CaseId, "CaseSeizure", DocMemo, eQueryStaff);
                    asBiz.insertCaseSeizure2(caseDoc, OrderedAccLists, eQueryStaff, "0026|還有其他帳戶可能可以扣押，請確認。");
                    WriteLog("備註文字: " + string.Join(",", DocMemo.Select(x => x)));
                    return SeizureResult;
                }
            }


            #endregion

            #region 若總扣押金額<450 元, 則不進行扣押, 若大於450, 則扣押

           
            DocMemo.AddRange(calculatedAccList.Where(x => x.planSeizure>0 && !string.IsNullOrEmpty(x.Memo)).Select(x => x.Memo));

            // 20181025, by ID 沒有扣押的ID, 若planSeizure  ,要把DocMemo中的相關文字, 都拿掉
            var g2 = (from p in calculatedAccList group p by p.Id into g select g.Key).ToList();

            // 要保留     "!@!經查於本行負責人"      "!@!戶名不符"      "扣除手續費存款餘額未達200元"


            // 2018,1029, 其他的備註會不見, 不可這樣做
            List<string> resMemo = new List<string>();
            foreach (var a in DocMemo)
            {
                if (a.Contains("!@!經查於本行負責人") || a.Contains("!@!戶名不符") || a.Contains("扣除手續費存款餘額未達200元") || a.Contains("帳戶已設定為") || a.Contains("帳戶有"))
                    resMemo.Add(a);
            }
            
            List<string> gNewDocMemo = DocMemo.Where(x => !x.Contains("!@!")).ToList();
            foreach(var g22 in g2 )
            {
                var aAcc = calculatedAccList.Where(x => x.Id == g22).Sum(x => x.planSeizure);
                if (aAcc > 0) // 表示此ID不扣押, 要把相關這個ID的拿DocMemo文字拿掉
                {                   
                    foreach(var d in DocMemo )
                    {
                        if (d.StartsWith(g22))
                            gNewDocMemo.Add(d);
                    }
                }
            }
            DocMemo = gNewDocMemo;
            DocMemo.AddRange(resMemo);




            //if (totalBal2 < 450)
            {
                #region 總扣押<450
                // 20181029, 因為IR-182的原因, 要用DeepCopy來能實現,因此, 寫一個新方法, 只加總每個重號的總金額
                var totalSeiById = AccSum(calculatedAccList);
                foreach (var id in totalSeiById)
                {
                    if (id.Value < 450)
                    {
                        foreach (var s in calculatedAccList.Where(x => x.Id.StartsWith(id.Key)))
                        {
                            s.planSeizure = 0.0m;
                            s.showSeizure = 0.0m;
                        }
                        if (totalBal2 < SeizureTotal) // 如果總扣押金額, 不足額,  才會出來
                        {
                            DocMemo.Add(id.Key + "!@!" + "扣除手續費存款餘額未達200元，不予扣押。");
                            //asBiz.insertCaseMemo(caseDoc.CaseId, "CaseSeizure", DocMemo, eQueryStaff);
                            //asBiz.insertCaseSeizure(caseDoc, calculatedAccList.Where(x => x.Id.StartsWith(id.Key)).ToList(), eQueryStaff, "0023|總扣押金額<450 元");
                        }
                    }
                }




                if (totalBal2 == 0) // 總扣押==0
                {
                    asBiz.insertCaseSeizure(caseDoc, OrderedAccLists, eQueryStaff, "0001|");
                    asBiz.insertCaseMemo(caseDoc.CaseId, "CaseSeizure", DocMemo, eQueryStaff);
                    return "0000|" + "總扣押金額為0";
                }
                //var orginAccLists = AccLists;
                //var combinAcc = NormalAccount(AccLists);

                //var ids3 = (from p in combinAcc group p by p.Id into g select g.Key).ToList();
                //foreach (var id in ids3)
                //{
                //    var cAccList = combinAcc.Where(x => x.Id == id).ToList();
                //    decimal totalBal3 = cAccList.Sum(x => x.Twd);
                //    if (totalBal3 < 450)
                //    {
                //        foreach (var s in combinAcc.Where(x => x.Id == id))
                //        {
                //            s.planSeizure = 0.0m;
                //            s.showSeizure = 0.0m;
                //        }
                //        if (totalBal2 < SeizureTotal) // 如果總扣押金額, 不足額,  才會出來
                //        {
                //            List<string> newDocMemo = new List<string>() { id + "!@!" + "扣除手續費存款餘額未達200元，不予扣押。" };
                //            foreach (var a in DocMemo)
                //            {
                //                if (a.Contains(id + "!@!"))
                //                    newDocMemo.AddRange(DocMemo);
                //            }

                //            asBiz.insertCaseMemo(caseDoc.CaseId, "CaseSeizure", newDocMemo, eQueryStaff);
                //            asBiz.insertCaseSeizure(caseDoc, calculatedAccList.Where(x => x.Id == id).ToList(), eQueryStaff, "0023|總扣押金額<450 元");
                //        }
                //        //asBiz.insertCaseSeizure(caseDoc, cAccList, eQueryStaff, "0023|總扣押金額<450 元");
                //        SeizureResult = "0023|總扣押金額<450 元";
                //    }
                //}
                //// 恢復原來帳戶
                //AccLists = orginAccLists;
                #endregion
            }

            if (totalBal2 >= 450)
            {
                // 扣押動作
                string errorAccount = null;
                // ------------------------------------- 20181011 -------------------------
                // 目前測試版本, 先不扣押
                var result = SeizureSetting(calculatedAccList, govNo, ref errorAccount);
                //WriteLog("**********提醒目前並未執行真正扣押動作**************");
                //string result = "0000|";
                //-------------------------------------------------------------------------------

                if (result.StartsWith("0002|"))
                {
                    string retMessage = result.Replace("0002|", "");
                    DocMemo.Add(retMessage);
                    // 寫入CaseMemo, 然後return
                    asBiz.insertCaseMemo(caseDoc.CaseId, "CaseSeizure", DocMemo, eQueryStaff);

                    // 要把已扣押的, 寫入CaseSeizure,
                    bool startToZero = false;
                    foreach (var acc in OrderedAccLists.Where(x => x.planSeizure != 0))
                    {
                        if (acc.Account == errorAccount)
                        {
                            startToZero = true;
                        }
                        if (startToZero)
                        {
                            acc.planSeizure = 0.0m;
                            acc.showSeizure = 0.0m;
                        }
                    }
                    asBiz.insertCaseSeizure(caseDoc, OrderedAccLists, eQueryStaff, result);

                    return "0028|" + retMessage;
                }


                SeizureResult = result;
                #region 寫回CaseMaster

                WriteLog(string.Format("Step 6-5 : 新增CaseSeizure及CaseMemo"));
                // 1. 要寫回CaseSeizre.Seq 扣押順序 // 0, 初始, 1:扣押完成 2. 支付完成                    
                //strCalcResult

                if (totalBal2 < SeizureTotal) // 若只是部分扣押, 則寫入全部帳戶
                {
                    asBiz.insertCaseSeizure(caseDoc, OrderedAccLists, eQueryStaff, "0003|部分扣押, 則寫入全部帳戶");
                    realSeizureAmt = OrderedAccLists.Where(x => x.planSeizure > 0).Sum(x => (decimal)x.Twd);
                    WriteLog(string.Format("Step 6-6 : 扣押總金額, 折合台幣{0}", realSeizureAmt.ToString("###,###,###")));
                }
                else
                {
                    // 要判斷若是來文8+10碼, 只扣10碼時就足額, 也要把8碼的寫入CaseSeizure
                    var gid = (from p in OrderedAccLists group p by p.Id into g select g.Key).ToList();
                    foreach(var g in gid)
                    {
                        var NewAccList = OrderedAccLists.Where(x => x.Id == g).ToList();
                        decimal totsum = NewAccList.Sum(x => x.showSeizure);
                        if( totsum==0)
                            asBiz.insertCaseSeizure(caseDoc, NewAccList, eQueryStaff, "0001|要寫CASESeizure");
                        else
                            asBiz.insertCaseSeizure(caseDoc, NewAccList, eQueryStaff, SeizureResult);
                    }

                    
                    WriteLog(string.Format("Step 6-6 : 扣押總金額, 折合台幣{0}", SeizureTotal.ToString("###,###,###")));
                }


                // 2. 寫回CaseMemo.. 把備註寫回去... MemoType='CaseMemo'
                asBiz.insertCaseMemo(caseDoc.CaseId, "CaseSeizure", DocMemo, eQueryStaff);
                #endregion
            }
            #endregion
            WriteLog("備註文字: " + string.Join(",", DocMemo.Select(x => x)));
            WriteLog(string.Format("Step 7 : 扣押結果{0}", SeizureResult));



            return SeizureResult;

        }


        private static Dictionary<string, decimal> AccSum(List<ObligorAccount> AccLists)
        {
            Dictionary<string, decimal> Result = new Dictionary<string, decimal>();
            var gid = AccLists.Where(x => x.Id.Length == 8).DistinctBy(x => x.Id).Select(x => x.Id).ToList();
            foreach (var g in gid)
            {
                decimal totalSum = 0.0m;
                foreach (var acc in AccLists.Where(x => x.Id.StartsWith(g)))
                {
                    totalSum += acc.Twd;
                }
                Result.Add(g, totalSum);
            }

            var gid2 = AccLists.Where(x => x.Id.Length == 10).DistinctBy(x => x.Id).Select(x => x.Id).ToList();
            foreach (var g in gid2)
            {
                decimal totalSum = 0.0m;
                foreach (var acc in AccLists.Where(x => x.Id.StartsWith(g)))
                {
                    totalSum += acc.Twd;                    
                }
                Result.Add(g, totalSum);
            }
            return Result;

        }

        private static List<ObligorAccount>  NormalAccount(List<ObligorAccount> AccLists)
        {
            List<ObligorAccount> result = new List<ObligorAccount>();
            var gid = AccLists.Where(x => x.Id.Length == 8).DistinctBy(x => x.Id).Select(x => x.Id).ToList();
            foreach (var g in gid)
            {
                foreach (var acc in AccLists.Where(x => x.Id.StartsWith(g)))
                {
                    acc.Id = g;
                }
            }

            var gid2 = AccLists.Where(x => x.Id.Length == 10).DistinctBy(x => x.Id).Select(x => x.Id).ToList();
            foreach (var g in gid2)
            {
                foreach (var acc in AccLists.Where(x => x.Id.StartsWith(g)))
                {
                    acc.Id = g;
                }
            }
            result = AccLists;
            return result;
        }



        private static List<ObligorAccount> distinctPOrder(List<ObligorAccount> pOrder)
        {
            List<ObligorAccount> result = new List<ObligorAccount>();
            pOrder = pOrder.DistinctBy(x => x.Account).ToList();
            foreach (var s in pOrder)
            {
                if (s.ProdCode.EndsWith("19"))
                {
                    if (s.ProdCode == "0019")
                    {
                        s.AccountType = "活期";
                        result.Add(s);
                    }
                    else
                    {
                        // 若是其他XX19 , 非0019的.. 就忽略
                    }
                }
                else
                {
                    result.Add(s);
                }
            }
            return result;
        }


        /// <summary>
        ///  要排除 ("關係別") TX_60491_Detl.Link = 'JOIN'
        ///  要排除 TX_60491_Detl.StsDesc = '結清' or '放款' or '現金卡'
        /// </summary>
        /// <param name="oaList"></param>
        /// <returns></returns>
        private static List<ObligorAccount> getSeizureAccount(List<ObligorAccount> AccLists)
        {
            List<ObligorAccount> newAccList = new List<ObligorAccount>();
            List<string> noSave = new List<string>() { "結清", "已貸", "啟用", "誤開", "新戶", "核准", "婉拒", "作廢" };
            //
            foreach (var acc in AccLists)
            {
                bool bfilter = true;
                if (acc.Account.StartsWith("000000000000"))
                    bfilter = false;

                #region 判斷是否是現金卡等等
                // 若 prod_code = 0058, 或XX80 , 不用存
                if (acc.ProdCode.ToString().Equals("0058") || acc.ProdCode.ToString().EndsWith("80"))
                    bfilter = false;

                // 若  Link<>'JOIN' , 不用存
                if (acc.Link.ToString().Equals("JOIN"))
                    bfilter = false;

                // 若 StsDesc='結清' AND  StsDesc='已貸' AND  StsDesc='啟用' AND  StsDesc='誤開'  AND  StsDesc='新戶', 也不用存
                string sdesc = acc.StsDesc.ToString().Trim();
                if (noSave.Contains(sdesc))
                    bfilter = false;

                #endregion

                if (bfilter)
                    newAccList.Add(acc);
            }

            return newAccList;
        }








        /// <summary>
        /// 將1對多, 反轉為多對一的對映
        /// </summary>
        /// <param name="SeizureOrder"></param>
        /// <param name="SeizureNOTOrder"></param>
        /// <returns></returns>
        private static Dictionary<string, string> getReverseSeizureOrder(IList<CTBC.CSFS.Models.PARMCode> AllSeq)
        {
            Dictionary<string, string> Result = new Dictionary<string, string>();
            foreach (var s in AllSeq)
            {
                if (!string.IsNullOrEmpty(s.CodeMemo))
                {
                    string[] _pCodes = s.CodeMemo.Split(',');
                    if (_pCodes.Count() > 0)
                    {
                        foreach (var a in _pCodes)
                        {
                            if (!Result.ContainsKey(a)) // 因為, 綜定, 跟定存,是一樣的ProdCode , 以綜定優先
                                Result.Add(a, s.CodeDesc);
                        }
                    }
                }
            }
            return Result;
        }




        /// <summary>
        /// 輸入ProdCode 產生中文的帳戶類型
        /// </summary>
        /// <param name="p"></param>
        /// <param name="seizureSequenceAll"></param>
        /// <returns></returns>
        private static string lookUpAccountType(string prodCode, Dictionary<string, string> LookUpTable)
        {
            string accType = null;
            if (LookUpTable.ContainsKey(prodCode))
                accType = LookUpTable[prodCode];
            else // 找不到, 代表可能是XX開頭
            {
                string NewProdCode = "XX" + prodCode.Substring(2, 2);
                if (LookUpTable.ContainsKey(NewProdCode))
                    accType = LookUpTable[NewProdCode];
                else
                    accType = "未知帳戶類型";
            }
            return accType;
        }


        /// <summary> 
        ///         此功能的目的, 即為為決定.. 這些帳戶, 要扣多少錢
        ///         但要注意, 若noSeizure = True, 表示該帳戶不得進行扣押
        ///         前題是因為. 若有定存(連結到活儲的).. 必須扣在活儲, 但 該該戶需要註記.. 因為.. 以下三個欄位, 一定要在這裏填完:
        ///         public decimal planSeizure { get; set; } // 原計劃應該要扣押多少金額
        //          public decimal realSeizure { get; set; }  實際扣押到的金額( 活儲 扣押的總金額) , 例如可用餘額10,000 , 但連結定存 共有50,000 ... 則實際要打9093 .. 60,000元
        //          public decimal showSeizure { get; set; } // 顯示(有連結定存類, 實際打9093(活儲 扣押的總金額) , 但在畫面上, 要連結類, 扣了多少) , 例如前例 , 要留下50,000元
        /// </summary>
        /// <param name="AccList"></param>
        /// <param name="pbiz">滙率檔</param>
        /// <param name="HandleFee">手續費</param>
        /// <param name="SeizureTotal">折合台幣，準備要扣少錢</param>
        /// <param name="govNo">來文字號</param>
        /// <param name="realSeizureAmt">折合台幣, 總共要扣多少錢</param>
        /// <returns></returns>
        private static List<ObligorAccount> CalcSeizureAmt(List<ObligorAccount> AccList, PARMCodeBIZ pbiz, int HandleFee, decimal SeizureTotal, string govNo, List<CaseObligor> VaildObligors, ref decimal realSeizureAmt, ref string SeizureResult)
        {
            List<string> Cur = new List<string>() { "TWD", "JPY", "DEN", "FRF", "HUF", "IDR", "ITL", "KRW" };


            List<string> SeizureMessage = new List<string>();

            decimal TotalTrueAmt = 0.0m;

            #region Step:  1: 若有外幣, 折算台幣
            var dicCurrency = pbiz.GetParmCodeByCodeType("Currency").ToDictionary(x => x.CodeNo, x => decimal.Parse(x.CodeMemo));
            gdicCurrency = dicCurrency;

            foreach (var item in AccList)
            {
                item.SeizureStatus = 0; // 設定扣押狀態為 未扣押
                if (!item.Ccy.Equals("TWD"))
                //if (item.AccountType.StartsWith("外幣"))
                {
                    item.Rate = dicCurrency[item.Ccy] * 0.95m;
                    item.Twd = Math.Floor((decimal)((item.Bal * item.Rate)));
                }
                else
                {
                    item.Rate = 1;
                    item.Twd = item.Bal;
                }
                TotalTrueAmt += item.Twd;
            }
            #endregion

            #region Step 2 : 若不足手續費, 直接回復
            if (TotalTrueAmt < HandleFee)
            {
                SeizureMessage.Add("0001|扣除手續費存款餘額未達(200或1,000元)，不予扣押。");
                realSeizureAmt = 0.0m;

                //return null;      
            }
            #endregion


            bool Stage1 = false;
            bool Stage2 = false;


            //全部要扣押旳錢 = 手續費(HandleFee) 
            decimal Total = SeizureTotal;
            decimal QueryBal = 0; //累計扣押金額
            decimal RemainingBal = Total; // 累計還有多少錢未扣




            //個人戶,  若有戶名不符的.. 完全不能扣押

            foreach (var v in VaildObligors.Where(x => !x.isSame))
            {
                var a = AccList.Where(x => x.Id == v.ObligorNo).ToList();
                foreach (var b in a)
                    b.noSeizure = true;
            }

            // 1. 預先掃描是否要 扣到連結戶
            //List<ObligorAccount> linkAccountList = new List<ObligorAccount>();

            foreach (var acc in AccList.Where(x => x.Twd >= 1)) // 金額大於等於1元, 才能扣
            {
                decimal SeiAmt = 0; // 本帳戶準備要扣押的金額
                acc.isHuman = false;
                if (acc.noSeizure)
                {
                    acc.planSeizure = 0.0m;
                    acc.showSeizure = 0.0m;
                    continue;
                }

                //if( acc.isTD || acc.isOD || acc.isLon)
                //{
                //    acc.planSeizure = SeiAmt; // 計劃要扣這麼多錢
                //    acc.showSeizure = SeiAmt;
                //    acc.SeizureStatus = 0; // 預設為不扣押
                //    continue;
                //}


                #region 計算這個帳戶, 要扣押多少錢
                if (acc.Twd <= RemainingBal)
                {
                    if (acc.Ccy != "TWD")
                        SeiAmt = acc.Bal;
                    else
                        SeiAmt = Math.Floor(acc.Twd);

                    RemainingBal -= Math.Floor(acc.Twd);
                    QueryBal += Math.Floor(acc.Twd);
                }
                else
                {
                    if (acc.Ccy != "TWD") // 表示外幣折算台幣後, 高於扣押 金額, 需要折算回外幣金額來扣押
                    {
                        SeiAmt = System.Math.Ceiling((RemainingBal / acc.Rate) * 100.0m) / 100.0m;
                        if (Cur.Contains(acc.Ccy))
                        {
                            SeiAmt = Math.Ceiling(SeiAmt);
                        }
                        RemainingBal -= Math.Floor(SeiAmt * acc.Rate);
                        QueryBal += Math.Floor(SeiAmt * acc.Rate);
                    }
                    else
                    {
                        SeiAmt = RemainingBal;
                        RemainingBal -= Math.Floor(SeiAmt);
                        QueryBal += Math.Floor(SeiAmt);
                    }

                }

                acc.planSeizure = SeiAmt; // 計劃要扣這麼多錢
                acc.showSeizure = SeiAmt;
                acc.SeizureStatus = 0; // 預設為不扣押

                if (SeiAmt < acc.Bal)
                    acc.SeizureStatus = 1; // 部分扣押
                if (SeiAmt == acc.Bal)
                    acc.SeizureStatus = 2; // 全部扣押
                if (SeiAmt > acc.Bal)
                    acc.SeizureStatus = 3; // 超額扣押
                #endregion




                #region  若有事故代號 04, 05, 06, 07, 10 , 落人工
                if ((bool)acc.is450OK)
                {
                    if (acc.message450Code == "04" || acc.message450Code == "05" || acc.message450Code == "06" || acc.message450Code == "07" || acc.message450Code == "08" || acc.message450Code == "10")
                    {
                        acc.isHuman = true;
                        acc.Memo = acc.message450;
                    }
                }
                #endregion
                #region 若事故代號, 11, 12,13,14, 直接發文, 但要加備註
                if ((bool)acc.is450OK)
                {
                    if (acc.message450Code == "11" || acc.message450Code == "12" || acc.message450Code == "13" || acc.message450Code == "14")
                    {
                        acc.isHuman = false;
                        acc.Memo = acc.message450;
                    }
                }
                #endregion
                #region  若有TD,OD, 則落人工
                if (acc.isTD || acc.isOD || acc.isLon)
                {
                    acc.isHuman = true;
                    acc.Memo = "帳戶有透支餘額，請確認。";
                }

                #endregion
                #region 如果要扣到定存，一律落人工
                if (acc.AccountType == "定存" || acc.AccountType == "綜定")
                {
                    acc.isHuman = true;
                }
                #endregion
                #region 只要要扣到外幣定存, 一律落人工
                if (acc.AccountType == "外幣定存") // 只要要扣到外幣定存, 一律落人工
                {
                    acc.isHuman = true;
                }
                #endregion



                #region 設定該帳戶的扣押成功與否, 與扣押到Stage1 或 Stage2

                #region Step 6-1 :  if( QueryBal>= HandleFee) ,  Stage 1 = True, 表示手續費，至少夠了
                if (QueryBal >= HandleFee)
                    Stage1 = true;
                #endregion

                #region Step 6-2 :  if( QueryBal>=  SeizureTotal) ,  Stage 2 = True, 表示全部扣足了... 跳出廻圈
                if (QueryBal >= SeizureTotal && RemainingBal <= 0)
                {
                    Stage2 = true;
                    break;
                }

                #endregion

                #endregion

                if (Stage1 && Stage2) // 都扣押到了.. 離開廻圈
                    break;
            }


            if (!Stage1 && !Stage2)
            {
                SeizureResult = "0001|完全沒有扣到";
            }

            if (Stage1 && !Stage2)
            {
                SeizureResult = "0002|只扣到手續費, 但扣押金額金扣到部分";

            }

            if (Stage1 && Stage2)
            {
                SeizureResult = "0000|所有金額都有扣押";
            }

            realSeizureAmt = QueryBal; //回傳總共扣了多少錢.                     

            // 寫入準備扣押的金額

            foreach (var a in AccList)
            {
                WriteLog(string.Format("\t Step 6-2-1 ID  {0} 帳戶{1} ProdCode {2} 準備扣押{3}, 折合台幣{4}", a.Id, a.Account, a.ProdCode, a.planSeizure.ToString(), ((decimal)a.planSeizure * a.Rate).ToString("###,###,##.##")));
            }

            return AccList;
        }




        /// <summary> 
        ///          公司戶
        ///         此功能的目的, 即為為決定.. 這些帳戶, 要扣多少錢
        ///         但要注意, 若noSeizure = True, 表示該帳戶不得進行扣押
        ///         前題是因為. 若有定存(連結到活儲的).. 必須扣在活儲, 但 該該戶需要註記.. 因為.. 以下三個欄位, 一定要在這裏填完:
        ///         public decimal planSeizure { get; set; } // 原計劃應該要扣押多少金額
        //          public decimal realSeizure { get; set; }  實際扣押到的金額( 活儲 扣押的總金額) , 例如可用餘額10,000 , 但連結定存 共有50,000 ... 則實際要打9093 .. 60,000元
        //          public decimal showSeizure { get; set; } // 顯示(有連結定存類, 實際打9093(活儲 扣押的總金額) , 但在畫面上, 要連結類, 扣了多少) , 例如前例 , 要留下50,000元
        /// </summary>
        /// <param name="AccList"></param>
        /// <param name="pbiz">滙率檔</param>
        /// <param name="HandleFee">手續費</param>
        /// <param name="SeizureTotal">折合台幣，準備要扣少錢</param>
        /// <param name="govNo">來文字號</param>
        /// <param name="realSeizureAmt">折合台幣, 總共要扣多少錢</param>
        /// <returns></returns>
        private static List<ObligorAccount> CalcSeizureAmt_Company(List<ObligorAccount> AccList, PARMCodeBIZ pbiz, int HandleFee, decimal SeizureTotal, string govNo, List<CaseObligor> VaildObligors, ref decimal realSeizureAmt, ref string SeizureResult)
        {
            List<string> Cur = new List<string>() { "TWD", "JPY", "DEN", "FRF", "HUF", "IDR", "ITL", "KRW" };


            List<string> SeizureMessage = new List<string>();

            decimal TotalTrueAmt = 0.0m;

            #region Step:  1: 若有外幣, 折算台幣
            var dicCurrency = pbiz.GetParmCodeByCodeType("Currency").ToDictionary(x => x.CodeNo, x => decimal.Parse(x.CodeMemo));
            gdicCurrency = dicCurrency;

            foreach (var item in AccList)
            {
                item.SeizureStatus = 0; // 設定扣押狀態為 未扣押
                if (!item.Ccy.Equals("TWD"))
                //if (item.AccountType.StartsWith("外幣"))
                {
                    item.Rate = dicCurrency[item.Ccy] * 0.95m;
                    item.Twd = Math.Floor((decimal)((item.Bal * item.Rate)));
                }
                else
                {
                    item.Rate = 1;
                    item.Twd = item.Bal;
                }
                TotalTrueAmt += item.Twd;
            }
            #endregion

            //#region Step 2 : 若不足手續費, 直接回復
            //if (TotalTrueAmt < HandleFee)
            //{
            //    SeizureMessage.Add("0001|扣除手續費存款餘額未達(200或1,000元)，不予扣押。");
            //    realSeizureAmt = 0.0m;

            //    //return null;      
            //}
            //#endregion


            #region Step 2 : 若不足手續費, 直接回復(要依個別ID來看, 是否足夠450元)
            
            // 分8碼, 跟10碼
            var gid1 = AccList.Where(x=>x.Id.Length==8).DistinctBy(x=>x.Id).Select(x => x.Id).ToList();

            foreach(var g in gid1)
            {
                decimal totalTrueAmt = AccList.Where(x => x.Id.StartsWith(g)).Sum(x => x.Twd);
                if( totalTrueAmt < HandleFee)
                {
                    foreach(var a in AccList.Where(x=>x.Id==g))
                    {
                        a.noSeizure = true;
                    }
                    SeizureMessage.Add("0001|扣除手續費存款餘額未達(200或1,000元)，不予扣押。");
                    realSeizureAmt = 0.0m;
                }
            }

            var gid2 = AccList.Where(x => x.Id.Length == 10).DistinctBy(x => x.Id).Select(x => x.Id).ToList();

            foreach (var g in gid2)
            {
                decimal totalTrueAmt = AccList.Where(x => x.Id.StartsWith(g)).Sum(x => x.Twd);
                if (totalTrueAmt < HandleFee)
                {
                    foreach (var a in AccList.Where(x => x.Id == g))
                    {
                        a.noSeizure = true;
                    }
                    SeizureMessage.Add("0001|扣除手續費存款餘額未達(200或1,000元)，不予扣押。");
                    realSeizureAmt = 0.0m;
                }
            }



            #endregion

            bool Stage1 = false;
            bool Stage2 = false;


            //全部要扣押旳錢 = 手續費(HandleFee) 
            decimal Total = SeizureTotal;
            decimal QueryBal = 0; //累計扣押金額
            decimal RemainingBal = Total; // 累計還有多少錢未扣




            //個人戶,  若有戶名不符的.. 完全不能扣押
            // 公司戶, 不管

            //foreach (var v in VaildObligors.Where(x => !x.isSame))
            //{
            //    var a = AccList.Where(x => x.Id == v.ObligorNo).ToList();
            //    foreach (var b in a)
            //        b.noSeizure = true;
            //}

            // 1. 預先掃描是否要 扣到連結戶
            //List<ObligorAccount> linkAccountList = new List<ObligorAccount>();

            foreach (var acc in AccList.Where(x => x.Twd >= 1)) // 金額大於等於1元, 才能扣
            {
                decimal SeiAmt = 0; // 本帳戶準備要扣押的金額
                acc.isHuman = false;
                if (acc.noSeizure)
                {
                    acc.planSeizure = 0.0m;
                    acc.showSeizure = 0.0m;
                    continue;
                }

                //if( acc.isTD || acc.isOD || acc.isLon)
                //{
                //    acc.planSeizure = SeiAmt; // 計劃要扣這麼多錢
                //    acc.showSeizure = SeiAmt;
                //    acc.SeizureStatus = 0; // 預設為不扣押
                //    continue;
                //}


                #region 計算這個帳戶, 要扣押多少錢
                if (acc.Twd <= RemainingBal)
                {
                    if (acc.Ccy != "TWD")
                        SeiAmt = acc.Bal;
                    else
                        SeiAmt = Math.Floor(acc.Twd);

                    RemainingBal -= Math.Floor(acc.Twd);
                    QueryBal += Math.Floor(acc.Twd);
                }
                else
                {
                    if (acc.Ccy != "TWD") // 表示外幣折算台幣後, 高於扣押 金額, 需要折算回外幣金額來扣押
                    {
                        SeiAmt = System.Math.Ceiling((RemainingBal / acc.Rate) * 100.0m) / 100.0m;
                        if (Cur.Contains(acc.Ccy))
                        {
                            SeiAmt = Math.Ceiling(SeiAmt);
                        }
                        RemainingBal -= Math.Floor(SeiAmt * acc.Rate);
                        QueryBal += Math.Floor(SeiAmt * acc.Rate);
                    }
                    else
                    {
                        SeiAmt = RemainingBal;
                        RemainingBal -= Math.Floor(SeiAmt);
                        QueryBal += Math.Floor(SeiAmt);
                    }

                }

                acc.planSeizure = SeiAmt; // 計劃要扣這麼多錢
                acc.showSeizure = SeiAmt;
                acc.SeizureStatus = 0; // 預設為不扣押

                if (SeiAmt < acc.Bal)
                    acc.SeizureStatus = 1; // 部分扣押
                if (SeiAmt == acc.Bal)
                    acc.SeizureStatus = 2; // 全部扣押
                if (SeiAmt > acc.Bal)
                    acc.SeizureStatus = 3; // 超額扣押
                #endregion




                #region  若有事故代號 04, 05, 06, 07, 10 , 落人工
                if ((bool)acc.is450OK)
                {
                    if (acc.message450Code == "04" || acc.message450Code == "05" || acc.message450Code == "06" || acc.message450Code == "07" || acc.message450Code == "08" || acc.message450Code == "10")
                    {
                        acc.isHuman = true;
                        acc.Memo = acc.message450;
                    }
                }
                #endregion
                #region 若事故代號, 11, 12,13,14, 直接發文, 但要加備註
                if ((bool)acc.is450OK)
                {
                    if (acc.message450Code == "11" || acc.message450Code == "12" || acc.message450Code == "13" || acc.message450Code == "14")
                    {
                        acc.isHuman = false;
                        acc.Memo = acc.message450;
                    }
                }
                #endregion
                #region  若有TD,OD, 則落人工
                if (acc.isTD || acc.isOD || acc.isLon)
                {
                    acc.isHuman = true;
                    acc.Memo = "帳戶有透支餘額，請確認。";
                }

                #endregion
                #region 如果要扣到定存，一律落人工
                if (acc.AccountType == "定存" || acc.AccountType == "綜定")
                {
                    acc.isHuman = true;
                }
                #endregion
                #region 只要要扣到外幣定存, 一律落人工
                if (acc.AccountType == "外幣定存") // 只要要扣到外幣定存, 一律落人工
                {
                    acc.isHuman = true;
                }
                #endregion



                #region 設定該帳戶的扣押成功與否, 與扣押到Stage1 或 Stage2

                #region Step 6-1 :  if( QueryBal>= HandleFee) ,  Stage 1 = True, 表示手續費，至少夠了
                if (QueryBal >= HandleFee)
                    Stage1 = true;
                #endregion

                #region Step 6-2 :  if( QueryBal>=  SeizureTotal) ,  Stage 2 = True, 表示全部扣足了... 跳出廻圈
                if (QueryBal >= SeizureTotal && RemainingBal <= 0)
                {
                    Stage2 = true;
                    break;
                }

                #endregion

                #endregion

                if (Stage1 && Stage2) // 都扣押到了.. 離開廻圈
                    break;
            }


            if (!Stage1 && !Stage2)
            {
                SeizureResult = "0001|完全沒有扣到";
            }

            if (Stage1 && !Stage2)
            {
                SeizureResult = "0002|只扣到手續費, 但扣押金額金扣到部分";

            }

            if (Stage1 && Stage2)
            {
                SeizureResult = "0000|所有金額都有扣押";
            }

            realSeizureAmt = QueryBal; //回傳總共扣了多少錢.                     

            #region 20181016, 若Case 4,5, 若8碼扣剩下低於450元, 則10碼的, 就不扣了...
            var gid = AccList.DistinctBy(x => x.Id).Select(x => x.Id).ToList();
            if(  gid.Any(x=>x.Length==8) && gid.Any(x=>x.Length==10)) //有8碼跟10碼的
            {
                string a8code = gid.Where(x=>x.Length==8).First();
                string a10code = gid.Where(x=>x.Length==10).First();
                var coSum = AccList.Where(x => x.Id.StartsWith(a8code) && x.planSeizure > 0).Sum(x => x.Twd);
                if (SeizureTotal - coSum <450) //剩下低於450元, 10 碼, 就不扣了
                {
                    WriteLog("\t\tCase 4,5, 若8碼扣剩下低於450元, 則10碼的, 就不扣了...");
                    foreach(var a in AccList.Where(x=>x.Id.StartsWith(a10code)))
                    {
                        a.planSeizure = 0.0m;
                        a.showSeizure = 0.0m;
                    }

                }

            }




            #endregion

            // 寫入準備扣押的金額

            foreach (var a in AccList)
            {
                WriteLog(string.Format("\t Step 6-2-1 ID  {0} 帳戶{1} ProdCode {2} 準備扣押{3}, 折合台幣{4}", a.Id, a.Account, a.ProdCode, a.planSeizure.ToString(), ((decimal)a.planSeizure * a.Rate).ToString("###,###,##.##")));
            }

            return AccList;
        }




        /// <summary>
        /// 要先計算完CalcSeizureAmt後, 取得planSeizure, 進行扣押        /// 
        /// </summary>
        /// <param name="AccList"></param>
        /// <param name="govNo"></param>
        /// <param name="realSeizureAmt"></param>
        /// <returns></returns>
        private static string SeizureSetting(List<ObligorAccount> AccList, string govNo, ref string errorAccount)
        {
            string SeizureResult = "0000|扣押成功";
            if (AccList == null)
            {
                return "0001|加總後可用餘額不足支付手續費";
            }

            // 20181206, A107112300003, 案例特殊, 若A無往來, B戶名不符, 則都不扣押, 要直接回文... ";
            decimal seiSum = AccList.Where(x => x.planSeizure != 0).Sum(x=>x.planSeizure);
            if( seiSum==0)
            {
                return "0033|案例特殊, 若A無往來, B戶名不符, 則都不扣押, 要直接回文";
            }

            List<string> SeizureMessage = new List<string>();



            bool isSeizure = false;

            #region  開始扣押
            string boolSeizure = "";

            foreach (var acc in AccList.Where(x => x.planSeizure != 0))
            {
                WriteLog(string.Format("\t\tStep 6-2-1 : 帳戶{0}, 扣押{1}, 幣別{2}", acc.Account, acc.planSeizure.ToString(), acc.Ccy));

                if (acc.noSeizure) // 有他案扣押, 此案不扣押, 落人工
                    continue;

                if (acc.AccountType.StartsWith("定存"))
                {
                    if (string.IsNullOrEmpty(acc.LinkAccount)) // 無連結 , 發查9091-1, 回9091
                    {
                        //boolSeizure = Send9091("1", acc.Account, acc.Ccy, "4", govNo,  "Y", acc.Id, acc.CaseId);
                        var pkid = asBiz.addAutoLog(Guid.Parse(acc.CaseId), eQueryStaff, "", acc.Id, "09091");

                        boolSeizure = objSeiHTG.SettingSeizure9091(acc, govNo);
                        if (!boolSeizure.StartsWith("0000"))
                            SeizureResult = "0001|扣押 9091 失敗";
                        if (SeizureResult.StartsWith("0000|"))
                            asBiz.updateAutoLog(pkid, 1, SeizureResult);
                        else
                        {
                            asBiz.updateAutoLog(pkid, 2, SeizureResult);
                            //asBiz.updateAutoLog(pkid, 2, retMessage);
                            List<string> DocMemo = new List<string>() { "扣押 9091 失敗" };
                            //DocMemo.Add("發查60629失敗, 原因 : " + retMessage);
                            asBiz.insertCaseMemo(Guid.Parse(acc.CaseId), "CaseSeizure", DocMemo, eQueryStaff);
                            return "0028|扣押 9091 失敗";
                        }
                    }
                    else // 有連結, 發查9093-1, 回9093
                    {
                        var pkid = asBiz.addAutoLog(Guid.Parse(acc.CaseId), eQueryStaff, "", acc.Id, "09093");
                        boolSeizure = objSeiHTG.SettingSeizure9093(acc, acc.planSeizure, govNo);
                        if (!boolSeizure.StartsWith("0000"))
                            SeizureResult = "0001|扣押 9093 失敗";
                        if (SeizureResult.StartsWith("0000|"))
                            asBiz.updateAutoLog(pkid, 1, SeizureResult);
                        else
                        {
                            asBiz.updateAutoLog(pkid, 2, SeizureResult);
                            List<string> DocMemo = new List<string>() { "扣押 9093 失敗" };
                            //DocMemo.Add("發查60629失敗, 原因 : " + retMessage);
                            asBiz.insertCaseMemo(Guid.Parse(acc.CaseId), "CaseSeizure", DocMemo, eQueryStaff);
                            return "0028|扣押 9093 失敗";
                        }
                    }
                }
                else // 其他類, 發查9093, 回9093
                {
                    var pkid = asBiz.addAutoLog(Guid.Parse(acc.CaseId), eQueryStaff, "", acc.Id, "09093");


                    boolSeizure = objSeiHTG.SettingSeizure9093(acc, acc.planSeizure, govNo);
                    //boolSeizure = "0000|測不實際執行扣押, 只是測試";

                    if (!boolSeizure.StartsWith("0000"))
                    {
                        SeizureResult = "0001|帳號:" + acc.Account + "扣押失敗, 原因:" + boolSeizure.Replace("0001|電文09093 發查失敗01|", "");

                        errorAccount = acc.Account;
                    }
                    if (boolSeizure.StartsWith("0000|"))
                    {
                        asBiz.updateAutoLog(pkid, 1, boolSeizure);
                        isSeizure = true;
                    }
                    else
                    {
                        asBiz.updateAutoLog(pkid, 2, boolSeizure);
                        isSeizure = false;
                    }
                }
                if (SeizureResult.StartsWith("0001|"))
                {
                    break;
                }

            }

            #endregion

            if (!isSeizure) // 若有某帳戶沒有扣押到, 則回訊息
            {
                SeizureResult = "0002|" + SeizureResult.Replace("0001|", "");
            }

            return SeizureResult;
        }



        /// <summary>
        /// 要扣押的帳戶, 舊版, 已不用, 2018/05/29
        /// </summary>
        /// <param name="AccList">要扣押的帳戶順序</param>
        /// <param name="Fee">最低手續費</param>
        /// <returns></returns>
        //private static string SeizureSetting(List<ObligorAccount> AccList, PARMCodeBIZ pbiz, int HandleFee, decimal SeizureTotal, string govNo, ref decimal realSeizureAmt)
        //{
        //    string SeizureResult = "0000|扣押成功";
        //    List<string> SeizureMessage = new List<string>();

        //    decimal TotalTrueAmt = 0.0m;

        //    #region Step:  1: 若有外幣, 折算台幣
        //    var dicCurrency = pbiz.GetParmCodeByCodeType("Currency").ToDictionary(x => x.CodeNo, x => decimal.Parse(x.CodeMemo));

        //    foreach (var item in AccList)
        //    {
        //        item.SeizureStatus = 0; // 設定扣押狀態為 未扣押
        //        if (item.AccountType.StartsWith("外幣"))
        //        {
        //            item.Rate = dicCurrency[item.Ccy] * 0.95m;
        //            item.Twd = (decimal)((item.Bal * item.Rate));
        //        }
        //        else
        //        {
        //            item.Rate = 1;
        //            item.Twd = item.Bal;
        //        }
        //        TotalTrueAmt += item.Twd;
        //    }
        //    #endregion

        //    #region Step 2 : 若不足手續費, 直接回復
        //    if (TotalTrueAmt < HandleFee)
        //    {
        //        SeizureMessage.Add("0001|加總後可用餘額不足支付手續費");
        //        realSeizureAmt = 0.0m;
        //        return "0001|加總後可用餘額不足支付手續費";
        //    }
        //    #endregion


        //    bool Stage1 = false;
        //    bool Stage2 = false;


        //    //全部要扣押旳錢 = 手續費(HandleFee) + 扣押總金額(SeizureTotal)
        //    decimal Total = SeizureTotal + (decimal)HandleFee;
        //    decimal QueryBal = 0; //累計扣押金額
        //    decimal RemainingBal = Total; // 累計還有多少錢未扣
        //    foreach (var acc in AccList)
        //    {
        //        decimal SeiAmt = 0; // 本帳戶準備要扣押的金額

        //        #region 計算這個帳戶, 要扣押多少錢
        //        if (acc.Twd <= RemainingBal)
        //        {
        //            if (acc.Ccy != "TWD")
        //                SeiAmt = acc.Bal;
        //            else
        //                SeiAmt = acc.Twd;

        //            RemainingBal -= acc.Twd;
        //            QueryBal += acc.Twd;
        //        }
        //        else
        //        {
        //            if (acc.Ccy != "TWD") // 表示外幣折算台幣後, 高於扣押 金額, 需要折算回外幣金額來扣押
        //            {
        //                SeiAmt = System.Math.Round(RemainingBal / dicCurrency[acc.Ccy], 2, MidpointRounding.AwayFromZero);
        //                RemainingBal -= SeiAmt * dicCurrency[acc.Ccy];
        //                QueryBal += SeiAmt * dicCurrency[acc.Ccy];
        //            }
        //            else
        //            {
        //                SeiAmt = RemainingBal;
        //                RemainingBal -= SeiAmt;
        //                QueryBal += SeiAmt;
        //            }

        //        }
        //        #endregion

        //        #region  開始扣押
        //        string boolSeizure = "";



        //        if (SeiAmt > 0 && acc.Twd > 0)
        //        {
        //            if (acc.AccountType.StartsWith("定存"))
        //            {
        //                if (string.IsNullOrEmpty(acc.LinkAccount)) // 無連結 , 發查9091-1, 回9091
        //                {

        //                    //boolSeizure = Send9091("1", acc.Account, acc.Ccy, "4", govNo,  "Y", acc.Id, acc.CaseId);
        //                    boolSeizure = objSeiHTG.SettingSeizure9091(acc, govNo);
        //                }
        //                else // 有連結, 發查9093-1, 回9093
        //                {
        //                    //boolSeizure = "0000|";
        //                    boolSeizure = objSeiHTG.SettingSeizure9093(acc, SeiAmt, govNo);
        //                    // boolSeizure =Send9093(acc.LinkAccount, SeiAmt, acc.Ccy, "66", govNo, acc.Id, acc.CaseId);
        //                }
        //            }
        //            else // 其他類, 發查9093, 回9093
        //            {
        //                //boolSeizure = "0000|";
        //                boolSeizure = objSeiHTG.SettingSeizure9093(acc, SeiAmt, govNo);
        //                //boolSeizure = Send9093(acc.Account, SeiAmt, acc.Ccy, "66", govNo, acc.Id, acc.CaseId);
        //            }
        //        }

        //        #endregion

        //        #region 設定該帳戶的扣押成功與否, 與扣押到Stage1 或 Stage2

        //        if (boolSeizure.StartsWith("0000")) // 有扣押到, boolSeizure=true
        //        {
        //            acc.SeizureStatus = 1; // 設定扣押狀態為 成功
        //            #region Step 6-1 :  if( QueryBal>= HandleFee) ,  Stage 1 = True, 表示手續費，至少夠了
        //            if (QueryBal >= HandleFee)
        //                Stage1 = true;


        //            #endregion

        //            #region Step 6-2 :  if( QueryBal>= HandleFee + SeizureTotal) ,  Stage 2 = True, 表示全部扣足了... 跳出廻圈
        //            if (QueryBal >= HandleFee + SeizureTotal && RemainingBal <= 0)
        //            {
        //                Stage2 = true;
        //                break;
        //            }

        //            #endregion
        //        }
        //        else
        //        {
        //            acc.SeizureStatus = 2; // 設定扣押狀態為 失敗
        //        }

        //        #endregion

        //        if (Stage1 && Stage2) // 都扣押到了.. 離開廻圈
        //            break;
        //    }


        //    if (!Stage1 && !Stage2)
        //    {
        //        SeizureResult = "0001|完全沒有扣到";
        //    }

        //    if (Stage1 && !Stage2)
        //    {
        //        SeizureResult = "0002|只扣到手續費, 但扣押金額金扣到部分";

        //    }

        //    if (Stage1 && Stage2)
        //    {
        //        SeizureResult = "0000|所有金額都有扣押";
        //    }
        //    return SeizureResult;
        //}




        /// <summary>
        /// 找出公司ID的扣押順序
        /// </summary>
        /// <param name="AccLists"></param>
        /// <param name="SeizureOrder"></param>
        /// <returns></returns>
        private static List<ObligorAccount> getCompanyOrder(List<ObligorAccount> AccLists, Dictionary<string, List<string>> SeizureOrder)
        {
            int _SeiSeq = 1;
            List<ObligorAccount> OrderedAccLists = new List<ObligorAccount>();

            // 第一層為公司戶，可能有多個ID, 因此要一個人一個人的來排順序
            // 找出有多少人
            var pids = (from p in AccLists group p by p.Id into g select g.Key).OrderBy(x=>x.Length).ToList(); 

            foreach (var id in pids)
            {

                var TDODList = AccLists.Where(x => x.isTD || x.isOD || x.isLon).ToList();
                var thisAccLists = AccLists.Except(TDODList).Where(x => x.Id == id).ToList(); // 先排除TD, OD, Lon 

                foreach (var s in SeizureOrder)
                {
                    if (s.Key == "綜定" || s.Key == "定存")
                    {
                        #region 要發電文 417確認, 有回傳有值, 代表綜存
                        var accTemp = thisAccLists.Where(x => s.Value.Contains(x.ProdCode)).ToList();
                        foreach (var acc in accTemp)
                        {
                            if (!string.IsNullOrEmpty(acc.LinkAccount)) // 20180530 表示有連結到活儲.. 要檢查是否該活儲, 有被扣押, 若被他案扣押, 則不得再扣, 落人工
                            {
                                bool bOther = false;
                                var res = isOtherCaseSeizure(acc, ref bOther); // 檢查, 是否他案扣押 
                                if (bOther)
                                {
                                    acc.noSeizure = true;
                                    acc.isHuman = true;
                                    var dd = thisAccLists.Where(x => x.Account == acc.LinkAccount).FirstOrDefault();
                                    if (dd != null)
                                    {
                                        dd.noSeizure = true;
                                        acc.isHuman = true;
                                    }
                                }
                            }
                            acc.SeizureSeq = _SeiSeq++;
                        }
                        OrderedAccLists.AddRange(accTemp);
                        #endregion
                    }
                    else // 非定存類
                    {
                        var accTemp = thisAccLists.Where(x => s.Value.Contains(x.ProdCode)).ToList();
                        // 填寫帳戶的類型
                        foreach (var at in accTemp)
                        {
                            at.AccountType = s.Key;
                            at.SeizureSeq = _SeiSeq++;
                        }

                        OrderedAccLists.AddRange(accTemp);
                        #region 開始處理有XX頭的ProdCode
                        List<string> XXStartWith = s.Value.Where(x => x.StartsWith("XX")).ToList();
                        if (XXStartWith.Count() > 0) // // 表示有XX頭, 則必須要用EndWith遂一過濾, 
                        {
                            foreach (var xx in XXStartWith)
                            {
                                var tail = xx.Substring(2);
                                var lst = thisAccLists.Where(x => x.Id == id && x.ProdCode.EndsWith(tail));
                                foreach (var at in lst)
                                {
                                    at.AccountType = s.Key;
                                    at.SeizureSeq = _SeiSeq++;
                                }
                                OrderedAccLists.AddRange(lst);
                            }
                        }
                        #endregion
                    }
                }

                // 若有TD ,OD, Lon的, 才加入...
                if (TDODList.Count() > 0)
                {
                    foreach (var acc in TDODList)
                    {
                        acc.SeizureSeq = _SeiSeq++;
                        OrderedAccLists.Add(acc);
                    }
                }

            }



            return OrderedAccLists;
        }

        /// <summary>
        /// 找出個人ID的扣押順序
        /// </summary>
        /// <param name="AccLists"></param>
        /// <param name="SeizureOrder"></param>
        /// <returns></returns>
        private static List<ObligorAccount> getPersonOrder(List<ObligorAccount> AccLists, Dictionary<string, List<string>> SeizureOrder)
        {
            List<ObligorAccount> OrderedAccLists = new List<ObligorAccount>();

            int _SeiSeq = 1;
            // 第一層為個人戶，可能有多個人, 因此要一個人一個人的來排順序
            // 找出有多少人
            var pids = (from p in AccLists group p by p.Id into g select g.Key).ToList();
            foreach (string id in pids)
            {
                //個人, ID的長度>8
                // 第二層， 活儲->XXX->定存, 比對ProdCode
                // 20180531 , 若有綜定(有連結到活儲的帳戶), 該帳戶不得被扣押, 要落人工

                var TDODList = AccLists.Where(x => x.isTD || x.isOD || x.isLon).ToList();
                var thisAccLists = AccLists.Except(TDODList).Where(x => x.Id == id).ToList(); // 先排除TD, OD, Lon 

                foreach (var s in SeizureOrder)
                {
                    if (s.Key == "綜定" || s.Key == "定存")
                    {
                        #region 要發電文 417確認, 有回傳有值, 代表綜存, 無值, 代表定存
                        //var accTemp = AccLists.Where(x => x.Id.Length > 8 && x.Id == id && s.Value.Contains(x.ProdCode)).ToList();
                        var accTemp = thisAccLists.Where(x => s.Value.Contains(x.ProdCode)).ToList();
                        foreach (var acc in accTemp)
                        {
                            if (!string.IsNullOrEmpty(acc.LinkAccount)) // 20180530 表示有連結到活儲.. 要檢查是否該活儲, 有被扣押, 若被他案扣押, 則不得再扣, 落人工
                            {
                                bool bOther = false;
                                var res = isOtherCaseSeizure(acc, ref bOther); // 檢查, 是否他案扣押 
                                if (bOther)
                                {
                                    acc.noSeizure = true;
                                    acc.isHuman = true;
                                    var dd = AccLists.Where(x => x.Account == acc.LinkAccount).FirstOrDefault();
                                    if (dd != null)
                                    {
                                        dd.noSeizure = true;
                                        dd.isHuman = true;
                                    }
                                }
                            }
                            acc.SeizureSeq = _SeiSeq++;
                        }
                        OrderedAccLists.AddRange(accTemp);
                        #endregion
                    }
                    else // 非定存類
                    {
                        //var accTemp = AccLists.Where(x => x.Id.Length > 8 && x.Id == id && s.Value.Contains(x.ProdCode)).ToList();
                        var accTemp = thisAccLists.Where(x => s.Value.Contains(x.ProdCode)).ToList();
                        // 填寫帳戶的類型
                        foreach (var at in accTemp)
                        {
                            at.AccountType = s.Key;
                            at.SeizureSeq = _SeiSeq++;
                        }
                        OrderedAccLists.AddRange(accTemp);
                        #region 開始處理有XX頭的ProdCode
                        List<string> XXStartWith = s.Value.Where(x => x.StartsWith("XX")).ToList();
                        if (XXStartWith.Count() > 0) // // 表示有XX頭, 則必須要用EndWith遂一過濾, 
                        {
                            foreach (var xx in XXStartWith)
                            {

                                var tail = xx.Substring(2);
                                var lst = thisAccLists.Where(x => x.Id.Length > 8 && x.Id == id && x.ProdCode.EndsWith(tail));

                                foreach (var at in lst)
                                {
                                    at.AccountType = s.Key;
                                    at.SeizureSeq = _SeiSeq++;
                                }
                                OrderedAccLists.AddRange(lst);
                            }
                        }
                        #endregion
                    }
                }

                // 若有TD ,OD, Lon的, 才加入...
                if (TDODList.Count() > 0)
                {
                    foreach (var acc in TDODList)
                    {
                        acc.SeizureSeq = _SeiSeq++;
                        OrderedAccLists.Add(acc);
                    }
                }
            }




            return OrderedAccLists;
        }
        /// <summary>
        /// 發電文, 找出是否他案有扣押
        /// </summary>
        /// <param name="acc"></param>
        private static string isOtherCaseSeizure(ObligorAccount acc, ref bool result)
        {
            var pkid = asBiz.addAutoLog(Guid.Parse(acc.CaseId), eQueryStaff, "", acc.Id, "45031");

            var re45031 = objSeiHTG.Send45031(acc.Id, acc.CaseId, acc.Account, acc.Ccy);
            if (re45031.StartsWith("0000|"))
            {
                string _trn = re45031.Replace("0000|", "");

                result = asBiz.getTX_45031(_trn);
            }
            if (re45031.StartsWith("0000|"))
                asBiz.updateAutoLog(pkid, 1, re45031);
            else
            {
                asBiz.updateAutoLog(pkid, 2, re45031);
                List<string> DocMemo = new List<string>() { "發查450-31失敗" };
                WriteLog("發查450-31失敗");
                asBiz.insertCaseMemo(Guid.Parse(acc.CaseId), "CaseSeizure", DocMemo, eQueryStaff);
                return "0028|發查450-31失敗";
            }

            return re45031;
        }

        private static Dictionary<string, List<string>> getSeizureNOTOrder(IList<CSFS.Models.PARMCode> seizureSequenceAll)
        {
            var seizureNOT_Sequence = seizureSequenceAll.Where(x => x.CodeNo == "99").ToList();
            Dictionary<string, List<string>> SeizureNOTOrder = new Dictionary<string, List<string>>();
            foreach (var s in seizureNOT_Sequence)
            {
                if (!string.IsNullOrEmpty(s.CodeMemo))
                {
                    SeizureNOTOrder.Add(s.CodeDesc, s.CodeMemo.Split(',').ToList());
                }
            }
            return SeizureNOTOrder;
        }

        private static Dictionary<string, List<string>> getSeizureOrder(IList<CSFS.Models.PARMCode> seizureSequenceAll)
        {
            var seizureSequence = seizureSequenceAll.Where(x => x.CodeNo != "99").ToList();
            Dictionary<string, List<string>> SeizureOrder = new Dictionary<string, List<string>>();
            foreach (var s in seizureSequence)
            {
                if (!string.IsNullOrEmpty(s.CodeMemo))
                {
                    SeizureOrder.Add(s.CodeDesc, s.CodeMemo.Split(',').ToList());
                }
            }
            return SeizureOrder;
        }






        /// <summary>
        /// 取得33401的餘額
        /// </summary>

        /// <returns></returns>
        private static decimal getBalance(string ObligorID, string CaseID, string Acc_No, ref bool isOD, ref bool isTD, ref bool isLon, ref bool isHoldAmt, string DocNo, string ccy, ref string errMessage, bool isForeign = false)
        {
            decimal result = 0;
            Guid _caseid = Guid.Parse(CaseID);


            var pkid = asBiz.addAutoLog(Guid.Parse(CaseID), eQueryStaff, DocNo, ObligorID, "401");



            var str401 = objSeiHTG.Send401(ObligorID, CaseID, Acc_No, ccy);
            if (str401.StartsWith("0000|"))
            {
                string _trn = str401.Replace("0000|", "");
                result = getBalance(_trn, Acc_No, isForeign);                
                asBiz.getTDOD(_trn, Acc_No, ref isTD, ref isOD, ref isLon, ref isHoldAmt);
                asBiz.updateAutoLog(pkid, 1, str401);
            }
            else
            {
                asBiz.updateAutoLog(pkid, 2, str401);
                List<string> DocMemo = new List<string>() { "發查401失敗" +  str401.Replace("0001|", "") };
                WriteLog("發查401失敗");
                asBiz.insertCaseMemo(Guid.Parse(CaseID), "CaseSeizure", DocMemo, eQueryStaff);
                errMessage = "0028|發查401失敗";
                //return "0028|發查401失敗";
            }




            return result;
        }



        private static decimal getBalance(string TrnNum, string Acc_No, bool isForeign)
        {
            decimal result = 0;
            //if( isForeign)
            //{
            //    Acc_No = Acc_No.Substring(0,Acc_No.Length - 3);
            //}

            //Acc_No = Acc_No.Substring(Acc_No.Length - 12);
            result = asBiz.getBalance(TrnNum, Acc_No);

            return result;
        }


        public static Decimal GetDecimal(string strn)
        {
            if (strn.Length == 0)
                return 0;
            Decimal result = 0;

            if (strn.LastIndexOf("+") != -1 || strn.LastIndexOf("-") != -1)
            {
                string sign = strn.Substring(strn.Length - 1, 1);
                if (sign == "+")
                {
                    result = Convert.ToDecimal(strn.Substring(0, strn.Length - 1));
                }
                else
                {
                    result = Convert.ToDecimal(strn.Substring(0, strn.Length - 1)) * -1;
                }

            }
            else
            {
                result = Convert.ToDecimal(strn);
            }


            return result;
        }














        private static void testCalcSeizureAmt(PARMCodeBIZ pbiz)
        {
            decimal realSeizureAmt111 = 0.0m;
            List<ObligorAccount> accList111 = new List<ObligorAccount>();
            ObligorAccount a1 = new ObligorAccount() { Account = "1", LinkAccount = "", Bal = 10000m, Ccy = "TWD", AccountType = "活儲" };
            ObligorAccount a2 = new ObligorAccount() { Account = "2", LinkAccount = "1", Bal = 20000m, Ccy = "TWD", AccountType = "綜定" };
            ObligorAccount a3 = new ObligorAccount() { Account = "3", LinkAccount = "1", Bal = 30000m, Ccy = "TWD", AccountType = "綜定" };
            ObligorAccount a4 = new ObligorAccount() { Account = "4", LinkAccount = "", Bal = 10000m, Ccy = "TWD", AccountType = "定存" };
            accList111.Add(a1); accList111.Add(a2); accList111.Add(a3); accList111.Add(a4);

            //var a33k3 = CalcSeizureAmt(accList111, pbiz, 450, 100000.00m, "adsakfja", ref realSeizureAmt111);


        }

        private static void noticeITMail(string mailTo, string Message)
        {
            string mailFrom = ConfigurationManager.AppSettings["mailFrom"];
            string[] mailFromTo = mailTo.Split(',');

            PARMCodeBIZ pbiz = new PARMCodeBIZ();
            var sysEnv = pbiz.GetParmCodeByCodeType("SysEnv");
            string strEnv = "";

            if (sysEnv.Count() > 0)
            {
                strEnv = sysEnv.First().CodeNo.Trim();
            }


            string subject = strEnv + "外來文系統(AutoSeizure.exe) 發生錯誤";
            string body = "錯誤訊息：" + Message;
            string host = ConfigurationManager.AppSettings["mailHost"];
            UtlMail.SendEmail(mailFrom, mailFromTo, subject, body, host);
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


            string subject = strEnv + " -- CTBC.WinExe.AutoSeizure 外來文系統 RACF 登入錯誤";
            string body = "外來文系統 RACF 登入錯誤，錯誤原因：" + InitMessage;
            string host = ConfigurationManager.AppSettings["mailHost"];
            UtlMail.SendEmail(mailFrom, mailFromTo, subject, body, host);
        }
        /// <summary>
        /// Just for Test for 9093 & 9091 API
        /// 2018/05/22
        /// </summary>
        private static void test90919093()
        {
            ExecuteHTG objHTG = new ExecuteHTG();

            //var s33401 = objHTG.Send401("B101063165", "a4b59d8c-9add-478d-955d-b099437d65d3", "0000107546000069","TWD");

            // var s45030 = objHTG.Send45030("B101063165", "a4b59d8c-9add-478d-955d-b099437d65d3", "0000006135179541");

            //var s45031 = objHTG.Send45031("B101063165", "a4b59d8c-9add-478d-955d-b099437d65d3", "0000006135179541");

            //var s09092 = objHTG.Send9092("0000006135179541", "TWD", "4", "就是扣押", "B101063165", "a4b59d8c-9add-478d-955d-b099437d65d3");
            //var s09091 = objHTG.Send9091Or9093("00000495540203503", 1000, "TWD", "66", "就是扣押", "B101063165", "a4b59d8c-9add-478d-955d-b099437d65d3");

            //var s09092 = objHTG.Send9092("00000495540203503", "TWD", "11", "就是詐騙", "B101063165", "A4B59D8C-9ADD-478D-955D-B099437D65D3");
            var s09091 = objHTG.Send9091("00000495540203503", "TWD", "11", "就是詐騙", "B101063165", "A4B59D8C-9ADD-478D-955D-B099437D65D3");
            //var s45030 = objHTG.Send45030("B101063165", "a4b59d8c-9add-478d-955d-b099437d65d3", "00000495540203503");


            string aa = "test";


            return;
        }

        private static void test9099()
        {
            ExecuteHTG objHTG = new ExecuteHTG();

            //var s33401 = objHTG.Send401("B101063165", "a4b59d8c-9add-478d-955d-b099437d65d3", "0000107546000069","TWD");

            // var s45030 = objHTG.Send45030("B101063165", "a4b59d8c-9add-478d-955d-b099437d65d3", "0000006135179541");

            //var s45031 = objHTG.Send45031("B101063165", "a4b59d8c-9add-478d-955d-b099437d65d3", "0000006135179541");

            //var s09092 = objHTG.Send9092("0000006135179541", "TWD", "4", "就是扣押", "B101063165", "a4b59d8c-9add-478d-955d-b099437d65d3");
            //var s09091 = objHTG.Send9091Or9093("00000495540203503", 1000, "TWD", "66", "就是扣押", "B101063165", "a4b59d8c-9add-478d-955d-b099437d65d3");

            //var s09092 = objHTG.Send9092("00000495540203503", "TWD", "11", "就是詐騙", "B101063165", "A4B59D8C-9ADD-478D-955D-B099437D65D3");
            //send9099(string Account, decimal SeizureAmount, string Ccy, string Code, string Memo, string ObligorNo, string caseid, string DTSRC_Date = "")
            //var s09099 = objHTG.Send9099("0000495540468924", 500.0m, "TWD", "66", "就是支付", "C2908067731", "3f016fb5-4e30-43b1-b8ad-0795a7a82d80", "12092018");
            var s09099Reset = objHTG.Send9099Reset("0000495540468924", 500.0m, "TWD", "66", "就是支付", "C2908067731", "3f016fb5-4e30-43b1-b8ad-0795a7a82d80", "12092018");


            string aa = "test";


            return;
        }

        /// <summary>
        /// 取出備註, 新規則
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

        public static void WriteLog(string msg)
        {
            if (Directory.Exists(@".\Log") == false)
            {
                Directory.CreateDirectory(@".\Log");
            }
            LogManager.Exists("DebugLog").Debug(msg);
        }
    }

    public class doubleAccount
    {
        public string id { set; get; } // 打回來的ID
        public string name { get; set; } // 打回來的Name
        public bool isConsist1 { get; set; } // 是否與來文名字相同
        public string newName { get; set; } // 打完60600 後的
        public bool isConsist2 { get; set; } // 是否新名字與來文名字相同

    }
}
