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
using System.IO;

namespace CTBC.WinExe.AutoCancel
{
    class Program
    {
        //CTBC.FrameWork.HTG.HostMsgGrpBIZ hostbiz = new CTBC.FrameWork.HTG.HostMsgGrpBIZ();
        //CustomerInfoBIZ custBiz = new CustomerInfoBIZ();
        public static CaseMasterBIZ caseMaster = new CaseMasterBIZ();
        //public static CaseAccountBiz caseAccount = new CaseAccountBiz();
        //public static bool sendHTG = true;  // 本變數來決定, 是否要發電文, 還是只讀DB
        public static ExecuteHTG objSeiHTG = null;
        public static string eQueryStaff = null;
        public static string _branchNo = null;
        public static string cs = null;
        public static string LastAgentSetting = null;
        public static string ITManagerEmail = "hunghsiang.chang@ctbcbank.com"; // 預設宏祥...
        //public static int delaySeconds = 1800; // 預設測試卡件, 1800 秒...
        public static AutoCancelBiz asBiz;

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
            }


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
            asBiz = new AutoCancelBiz();
            // 先檢查 select * from [CSFS_SIT].[dbo].[PARMCode] where codetype='eTabsQueryStaff'中的人員的LDAP RACF有效性

            var eTabsStaffs = pbiz.GetParmCodeByCodeType("eTabsQueryStaff").Where(x => (bool)x.Enable).OrderBy(x => x.SortOrder).ToList();
            var eBranchNo = pbiz.GetParmCodeByCodeType("eTabsQueryStaffBranchNo").FirstOrDefault();
            if (eBranchNo != null)
                _branchNo = eBranchNo.CodeDesc;

            if (eTabsStaffs.Count() == 0)
            {
                WriteLog("**********************************************************************");
                WriteLog("****************************目前沒有指定發查人**************************");
                WriteLog("**********************************************************************");
                noticeITMail(ITManagerEmail, "參數檔中, 沒有啟動任何一個電文發查人!");
                return;
            }

            objSeiHTG = new ExecuteHTG();
            // 讀取上一次, 指派到那一位承辦人
            var _lastAgetSetting = pbiz.GetParmCodeByCodeType("AssignLast").FirstOrDefault();
            if (_lastAgetSetting == null)
                LastAgentSetting = eQueryStaff;
            else
            {
                LastAgentSetting = _lastAgetSetting.CodeDesc;
            }

            if (eBranchNo != null)
                _branchNo = eBranchNo.CodeDesc;


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
                        if (eTabsStaffsMail1 != null)
                        {
                            string[] mailTo = eTabsStaffsMail1.CodeMemo.Split(',');
                            noticeMail(mailTo, InitErrorMessage);
                            WriteLog("**********************************************************************");
                            WriteLog(string.Format("****************************目前發查人{0} 帳密錯誤**************************", eQueryStaff));
                            WriteLog("**********************************************************************");
                        }
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



            if (args.Length == 0) // 沒有參數，代表不按照來文號的尾碼進行分流發查, 照發文號來查
            {
                doCancel(null);
            }
            else
            {
                doCancel(args[0].ToString().Trim());
            }
        }

        private static void doCancel(string _CaseNo)
        {


            // 先找, 是否有狀態是998 且尾數是tailNum, 若有, 表示, 目前前一次執行尚未完成, 等候下一輪, 在進行發查
            //var oList = cobiz.GetAllServiceObligorNo(tailNum);

            //int RunningCaseNum = getStillRunningNumber(_CaseNo);
            int RunningCaseNum = asBiz.getCaseMasterStillRuning();

            if (RunningCaseNum > 0)
            {
                // ===============================================================================================
                // 20221128, 正達表示, 過去幾個月內, 並不會主動落人工..
                // 20221128, 改為. .在本批自動撒銷完成後, 回頭看一下, 是否本批還有998, 若有.. 則落人工
                // ===============================================================================================




                // 20220328, 若在執行中, 可能是相同案件, 卡件, 所以要檢查, 是不是
                // 找出目前正在執行的第一筆...
                //var allCaseDoc = asBiz.getCaseMaster("998");
                //var Firstcasedoc = allCaseDoc.First();
                ////20220328, 記錄目前處理的案件在ParmCode.CodeType='ProcAutoCancel' , CodeNo='案號',  ModifiedDate='執行時間'
                ////20220706, 讀取第一筆上次案件的執行時間....
                //PARMCode lastProcCase = asBiz.getParmCodeByCodeType("ProcAutoCancel");
                
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




            //找出發文號尾碼
            List<CaseMaster> CaseMasterLists = asBiz.getCaseMaster("B01");
            WriteLog(string.Format("\r\n\r\n\r\n\r\n取得共{0}筆案件", CaseMasterLists.Count().ToString()));

            if (CaseMasterLists.Count() == 0)
            {
                WriteLog(string.Format("\r\n\r\n\r\n\r\n目前無案件可發查"));
                return;
            }


            List<AgentSetting> humanProcLists = asBiz.getAgentSetting();

            LDAPEmployee agent = (new LdapEmployeeBiz()).GetAllEmployeeInEmployeeViewByEmpId(eQueryStaff);


            // 確定是個人戶的案件後,  馬上把取得案件編號, 改成998
            //setCaseMasterRunning(CaseMasterLists.Select(x => x.CaseId).ToList());
            asBiz.setCaseMasterRunning(CaseMasterLists.Select(x => x.CaseId).ToList(), "998");

            foreach (var casedoc in CaseMasterLists.OrderBy(x=>x.CaseNo))
            {
                try
                {
                    WriteLog(string.Format("\r\n\r\n開始撤銷CaseID={0} DocNo={1}\r\n", casedoc.CaseId.ToString(), casedoc.CaseNo.ToString()));


                    //20220328, 記錄目前處理的案件在ParmCode.CodeType='ProcAutoCancel' , CodeNo='案號',  ModifiedDate='執行時間'                    
                    string message = asBiz.setExecuteCaseNo("ProcAutoCancel", casedoc.CaseNo);
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

                    asBiz.insertCaseHistory(casedoc, "自動撤銷", eQueryStaff, "收發-待分文", "收發分派", "經辦人員", eQueryStaff, "經辦-待辦理");
                    asBiz.insertCaseAssignTable(casedoc.CaseId, eQueryStaff, 0);
                    WriteLog("新增一筆CaseHistory");
                    #endregion


                }
                catch (Exception ex)
                {
                    
                    WriteLog(string.Format("\r\n\r\n\r\n\r\n未知的錯誤 {0}", ex.Message.ToString()));
                    List<string> docmemo = new List<string>() { "修改發查人錯誤，落人工" };
                    //insertCaseMemo(casedoc.CaseId, "CaseSeizure", docmemo, eQueryStaff);
                    asBiz.insertCaseMemo(casedoc.CaseId, "CaseSeizure", docmemo, eQueryStaff);
                    noticeITMail(ITManagerEmail, "修改發查人錯誤，落人工!, 案件編號: " + casedoc.CaseNo + "錯誤訊息: " + ex.Message.ToString());
                    continue;
                }

                try
                {
                    var result = doCancelByCaseId(casedoc);
                    if (result.StartsWith("0000"))
                    {
                        WriteLog("撤銷完成!!");
                        asBiz.updateCaseMasterStatus(casedoc.CaseId, "D01");
                        asBiz.updateCaseMasterStatus(casedoc.CaseId, "D01");                        
                        asBiz.updateCaseMasterAgentUser(casedoc.CaseId, eQueryStaff);
                        asBiz.insertCaseHistory(casedoc, "自動撤銷", eQueryStaff, "經辦-待辦理", "經辦呈核", "自動化處理", eQueryStaff, "主管-待核決");
                        ChengHe(casedoc.CaseId, eQueryStaff);
                    }
                    else
                    {
                        WriteLog("撤銷失敗, 原因: !!" + result);
                        #region 撤銷失敗
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
                                WriteLog(string.Format("自動撤銷失敗，指派承辦人{0} ", nextAgent.EmpId.ToString()));
                                asBiz.insertCaseHistory(casedoc, "自動撤銷", eQueryStaff, "收發-待分文", "收發分派", "自動撤銷退回人工", nextAgent.EmpId.ToString(), "經辦-待辦理");
                                LastAgentSetting = nextAgent.EmpId.ToString();
                                asBiz.updateLastAssign(nextAgent); // 存入ParmCode 的AssignLast

                            }
                            else // 若無, 則指定eQueryStaff
                            {
                                asBiz.updateCaseMasterAgentUser(casedoc.CaseId, "SYS");
                                asBiz.updateCaseMasterStatus(casedoc.CaseId, "B01");
                                //insertCaseHistory(casedoc, "自動扣押", eQueryStaff, "經辦-辦理", "主管放行結案", "自動扣押", eQueryStaff, "經辦-待辦理");
                                WriteLog(string.Format("自動撤銷失敗，指派承辦人{0} ", eQueryStaff.ToString()));
                                asBiz.insertCaseHistory(casedoc, "自動撤銷", eQueryStaff, "收發-待分文", "收發分派", "自動撤銷退回人工", eQueryStaff, "經辦-待辦理");
                            }
                            //updateCaseMasterStatus(casedoc.CaseId, "C01");
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
                            asBiz.insertCaseHistory(casedoc, "自動撤銷", eQueryStaff, "收發-待分文", "收發分派", "自動撤銷退回人工", eQueryStaff, "收發-待分文");
                            #endregion
                        }
                        #endregion
                    }
                }
                catch(Exception ex)
                {
                    WriteLog("********************************************發生錯誤********************************************");
                    WriteLog(string.Format("{0} \t {1}", DateTime.Now.ToString("MM/dd hh:mm:ss.ffff"), ex.Message.ToString()));
                    WriteLog("********************************************發生錯誤********************************************");
                    WriteLog("強制派件   撤銷失敗 !!" );
                    List<string> docmemo = new List<string>() { "未知的錯誤，落人工" };
                    asBiz.insertCaseMemo(casedoc.CaseId, "CaseSeizure", docmemo, eQueryStaff);
                    noticeITMail(ITManagerEmail, "未知的錯誤!, 案件編號: " + casedoc.CaseNo + "錯誤訊息: " + ex.Message.ToString());
                    #region 撤銷失敗
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
                            WriteLog(string.Format("自動撤銷失敗，指派承辦人{0} ", nextAgent.EmpId.ToString()));
                            asBiz.insertCaseHistory(casedoc, "自動撤銷", eQueryStaff, "收發-待分文", "收發分派", "自動撤銷退回人工", nextAgent.EmpId.ToString(), "經辦-待辦理");
                            LastAgentSetting = nextAgent.EmpId.ToString();
                            asBiz.updateLastAssign(nextAgent); // 存入ParmCode 的AssignLast

                        }
                        else // 若無, 則指定eQueryStaff
                        {
                            asBiz.updateCaseMasterAgentUser(casedoc.CaseId, "SYS");
                            asBiz.updateCaseMasterStatus(casedoc.CaseId, "B01");
                            //insertCaseHistory(casedoc, "自動扣押", eQueryStaff, "經辦-辦理", "主管放行結案", "自動扣押", eQueryStaff, "經辦-待辦理");
                            WriteLog(string.Format("自動撤銷失敗，指派承辦人{0} ", eQueryStaff.ToString()));
                            asBiz.insertCaseHistory(casedoc, "自動撤銷", eQueryStaff, "收發-待分文", "收發分派", "自動撤銷退回人工", eQueryStaff, "經辦-待辦理");
                        }
                        //updateCaseMasterStatus(casedoc.CaseId, "C01");
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
                        asBiz.insertCaseHistory(casedoc, "自動撤銷", eQueryStaff, "收發-待分文", "收發分派", "自動撤銷退回人工", eQueryStaff, "收發-待分文");
                        #endregion
                    }
                    #endregion
                }
            }

            // 20221128, 正達建議, 在執行完成後, 去檢查一下, 是否有998的案件, 若有, 則落人工

            // 去檢查一下, 是否有999的案件, 若有, 則落人工
            WriteLog(string.Format("*********************************將案件共計 {0}, 已執行完成, 檢查是否有卡件******************************", CaseMasterLists.Count().ToString()));
            List<CaseMaster> stillRunningCase = asBiz.getCaseMasterStillRuningCase();
            if (stillRunningCase.Count() > 0)
            {
                WriteLog(string.Format("*********************************發現仍有卡件 , 共計{0} ******************************", stillRunningCase.Count().ToString()));
                foreach (CaseMaster caseInfo in stillRunningCase)
                {
                    WriteLog(string.Format("*********************************案件 {0}, 已落人工******************************", caseInfo.CaseNo));
                    asBiz.setCaseMasterC01B01(caseInfo.CaseId);
                    List<string> memos = new List<string>() { "電文發查延滯" };
                    asBiz.insertCaseMemo(caseInfo.CaseId, "CaseSeizure", memos, "SYS");
                    noticeITMail(ITManagerEmail, "自動扣押卡件! 案件編號: " + caseInfo.CaseNo + "\t 已落人工");
                }

            }
            else
            {
                WriteLog(string.Format("*********************************目前本批次無卡件, 程式完成******************************"));
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





        private static string doCancelByCaseId(CaseMaster caseDoc)
        {
            #region 案件層級的私用變數

            PARMCodeBIZ pbiz = new PARMCodeBIZ();

            // 外來文, 加註的所有備註
            List<string> DocMemo = new List<string>();

            // 本案件, 是否要落人工
            bool Doc2Human = false;

            // 本案件，執行的結果
            var SeizureResult = "0000|撤銷成功";

            // 本案件的CaseID
            Guid caseid = caseDoc.CaseId;

            Guid OrginalCaseid = new Guid();

            string OrginalGovNo = "";
            DateTime OrginalGovDate;
            // 本案件, 合法的ID
            List<CaseObligor> VaildObligors = new List<CaseObligor>();

            // 本案件, 因為以上的ID, 所帶出來的帳戶
            List<ObligorAccount> AccLists = new List<ObligorAccount>();
            CaseMaster OrginCaseInfo = new CaseMaster();
            decimal TotalSeizure = 0.0m;
            // 本案件, 來文的扣押總金額
            //decimal SeizureTotal = 0;
            #region   本案件, 取得原發文字號ReceiverNo2  從來文字號,  找到當初扣押的CaseID, 寫入OrginalCaseid

            //using (CSFS1Entities ctx = new CSFS1Entities())
            {
                //var edoc2 = ctx.EDocTXT2.Where(x => x.CaseId == caseDoc.CaseId).FirstOrDefault();

                EDocTXT2 edoc2 = asBiz.getEDocTXT2(caseDoc.CaseId);

                if (edoc2 != null) // 表示, 找到原來的發文字號
                {
                    // 20191119, 新增檢查, 若義務人(只要任何一個)是空白或NULL, 即落人工-----> START
                    if (string.IsNullOrEmpty(edoc2.ObligorName))
                    {
                        Doc2Human = true;
                        List<string> iMemo = new List<string>() { "義務人姓名空白或NULL值。" };
                        asBiz.insertCaseMemo(caseDoc.CaseId, "CaseMemo", iMemo, "SYS");
                        asBiz.insertCaseMemo(caseDoc.CaseId, "CaseSeizure", iMemo, "SYS");
                        return "0010|義務人姓名空白或NULL值，落人工";
                    }
                    // 20191119, 新增檢查, 若義務人(只要任何一個)是空白或NULL, 即落人工-----> END

                    OrginalGovNo = edoc2.ReceiverNo2;
                    OrginalGovDate = DateTime.Parse( edoc2.GovDate2);
                    if (long.Parse( edoc2.Amount) > 0)
                    {
                        Doc2Human = true;
                        //DocMemo.Add("撤銷有保留扣押金額，落人工");
                        //asBiz.insertCaseMemo(caseDoc.CaseId, "CaseSeizure", DocMemo, eQueryStaff);
                        List<string> iMemo = new List<string>() { "有保留餘額，落人工" };
                        asBiz.insertCaseMemo(caseDoc.CaseId, "CaseMemo", iMemo, "SYS");
                        asBiz.insertCaseMemo(caseDoc.CaseId, "CaseSeizure", iMemo, "SYS");
                        return "0010|有保留餘額，落人工";
                    }
                    if (!string.IsNullOrEmpty(edoc2.Memo)) // 20181030, 來文有備註, 落人工
                    {
                        Doc2Human = true;
                        //DocMemo.Add("撤銷有備註文字，落人工");
                        //insertCaseMemo(caseDoc.CaseId, "CaseSeizure", DocMemo, eQueryStaff);
                        List<string> iMemo = new List<string>() { "撤銷有備註，落人工" };
                        asBiz.insertCaseMemo(caseDoc.CaseId, "CaseMemo", iMemo, "SYS");
                        asBiz.insertCaseMemo(caseDoc.CaseId, "CaseSeizure", iMemo, "SYS");
                        return "0011|撤銷有備註，落人工";

                    }

                }
                else // 找不到原發文字號
                {
                    Doc2Human = true;
                    //DocMemo.Add("無前案資料，落人工");
                    //insertCaseMemo(caseDoc.CaseId, "CaseSeizure", DocMemo, eQueryStaff);
                    List<string> iMemo = new List<string>() { "找不到原發文字號" };
                    asBiz.insertCaseMemo(caseDoc.CaseId, "CaseMemo", iMemo, "SYS");
                    asBiz.insertCaseMemo(caseDoc.CaseId, "CaseSeizure", iMemo, "SYS");
                    return "0011|無前案資料，落人工";
                }

                if (!string.IsNullOrEmpty(OrginalGovNo))
                {
                    //var oCaseAll = ctx.CaseMaster.Where(x => x.GovNo == OrginalGovNo && x.GovDate == OrginalGovDate).ToList();
                    List<CaseMaster> oCaseAll = asBiz.getCaseMasterByGovNoNDate(OrginalGovNo, OrginalGovDate);
                    if( oCaseAll.Count()==0)
                    {
                        WriteLog(string.Format("前案找不到 !!!，落人工"));
                        Doc2Human = true;
                        //DocMemo.Add("無前案資料，落人工");
                        //insertCaseMemo(caseDoc.CaseId, "CaseSeizure", DocMemo, eQueryStaff);
                        List<string> iMemo = new List<string>() { "無前案扣押，落人工" };
                        asBiz.insertCaseMemo(caseDoc.CaseId, "CaseMemo", iMemo, "SYS");
                        asBiz.insertCaseMemo(caseDoc.CaseId, "CaseSeizure", iMemo, "SYS");
                        return "0011|無前案扣押，落人工";
                    }

                    if (oCaseAll.Count() > 1)
                    {
                        WriteLog(string.Format("前案扣押有2筆 !!!，落人工"));
                        Doc2Human = true;
                        //DocMemo.Add("無前案資料，落人工");
                        //insertCaseMemo(caseDoc.CaseId, "CaseSeizure", DocMemo, eQueryStaff);
                        string caseNo = string.Join(",", oCaseAll.Select(x=>x.CaseNo));
                        List<string> iMemo = new List<string>() { "前案扣押有1筆以上，落人工，案號 : "  + caseNo};
                        asBiz.insertCaseMemo(caseDoc.CaseId, "CaseMemo", iMemo, "SYS");
                        asBiz.insertCaseMemo(caseDoc.CaseId, "CaseSeizure", iMemo, "SYS");
                        return "0018|前案扣押有1筆以上，落人工";
                    }
                    var oCase = oCaseAll.FirstOrDefault();
                    if (oCase != null)
                    {
                        WriteLog(string.Format("前案已找到: CaseID : {0} / {1}", oCase.CaseId, oCase.CaseNo));
                        OrginalCaseid = oCase.CaseId;
                        OrginCaseInfo = oCase;
                    }
                    else
                    {
                        WriteLog(string.Format("前案找不到 !!!，落人工"));
                        Doc2Human = true;
                        //DocMemo.Add("無前案資料，落人工");
                        //asBiz.insertCaseMemo(caseDoc.CaseId, "CaseSeizure", DocMemo, eQueryStaff);
                        List<string> iMemo = new List<string>() { "無前案扣押，落人工" };
                        asBiz.insertCaseMemo(caseDoc.CaseId, "CaseMemo", iMemo, "SYS");
                        asBiz.insertCaseMemo(caseDoc.CaseId, "CaseSeizure", iMemo, "SYS");
                        return "0011|無前案扣押，落人工";
                    }
                }
                else
                {
                    Doc2Human = true;
                    WriteLog(string.Format("前案找不到 !!!，落人工"));
                    //DocMemo.Add("無前案資料，落人工");
                    //asBiz.insertCaseMemo(caseDoc.CaseId, "CaseSeizure", DocMemo, eQueryStaff);
                    List<string> iMemo = new List<string>() { "無前案扣押，落人工" };
                    asBiz.insertCaseMemo(caseDoc.CaseId, "CaseMemo", iMemo, "SYS");
                    asBiz.insertCaseMemo(caseDoc.CaseId, "CaseSeizure", iMemo, "SYS");
                    return "0011|無前案扣押，落人工 ";
                }

            }
            #endregion

            // Step 3 自動比對TXT檔的「扣押命令發文日期」=該撤銷案結果區的「來文日期」;
            //「扣押命令發文字號」=該撤銷案結果區的「來文字號」欄位的前三個字+最後6個字。

            CaseMaster OrginalCase = new CaseMaster();

            OrginalCase = asBiz.getCaseMaster(OrginalCaseid);
            //using (CSFS1Entities ctx = new CSFS1Entities())
            //{
            //    OrginalCase = ctx.CaseMaster.Where(x => x.CaseId == OrginalCaseid).FirstOrDefault();
            //}

            if (OrginalCase == null)
            {
                Doc2Human = true;
                WriteLog(string.Format("前案找不到 !!!，落人工"));
                //DocMemo.Add("無前案資料，落人工");
                //insertCaseMemo(caseDoc.CaseId, "CaseSeizure", DocMemo, eQueryStaff);
                List<string> iMemo = new List<string>() { "無前案扣押，落人工" };
                asBiz.insertCaseMemo(caseDoc.CaseId, "CaseMemo", iMemo, "SYS");
                asBiz.insertCaseMemo(caseDoc.CaseId, "CaseSeizure", iMemo, "SYS");
                return "0011|無前案扣押，落人工 ";
            }

            if (DateTime.Parse(OrginalCase.GovDate) != OrginalGovDate || OrginalCase.GovNo != OrginalGovNo)
            {
                Doc2Human = true;
                WriteLog(string.Format("前案資料日期與字號不符，落人工"));
                //DocMemo.Add("前案資料日期與字號不符，落人工");
                //insertCaseMemo(caseDoc.CaseId, "CaseSeizure", DocMemo, eQueryStaff);
                List<string> iMemo = new List<string>() { "前案資料日期與字號不符，落人工" };
                asBiz.insertCaseMemo(caseDoc.CaseId, "CaseMemo", iMemo, "SYS");
                asBiz.insertCaseMemo(caseDoc.CaseId, "CaseSeizure", iMemo, "SYS");
                return "0012|前案資料日期與字號不符，落人工";
            }



            // 檢查來文戶名ID是否與原案相同
            bool isNoCaseSeizure = false;
            bool isSameNameID = checkNameNID2(OrginalCase, caseDoc, ref isNoCaseSeizure);
            if (!isSameNameID && ! isNoCaseSeizure)
            {
                Doc2Human = true;
                WriteLog(string.Format("撤銷來文戶名/ID  與  扣押戶名/ID  不符，落人工"));
                //DocMemo.Add("撤銷來文戶名/ID  與  扣押戶名/ID  不符，落人工");
                //insertCaseMemo(caseDoc.CaseId, "CaseSeizure", DocMemo, eQueryStaff);
                List<string> iMemo = new List<string>() { "原扣押案號" + OrginalCase.CaseNo + "撤銷來文戶名/ID與扣押戶名/ID不符" };
                asBiz.insertCaseMemo(caseDoc.CaseId, "CaseMemo", iMemo, "SYS");
                asBiz.insertCaseMemo(caseDoc.CaseId, "CaseSeizure", iMemo, "SYS");
                return "0013|撤銷來文戶名/ID  與  扣押戶名/ID  不符，落人工";
            }

            #region step 1  20181129, 新增一項功能, 若找到原執行署扣押後, 要去SCAN 建檔日期後, 是否有法院來扣押的文, 若有則落人工

            bool isYuan = false; // 建檔日期後, 有沒有法院來的扣押.... 
            
            
                
            //VaildObligors = ctx.CaseObligor.Where(x => x.CaseId == OrginalCase.CaseId).ToList();
            VaildObligors = asBiz.getValidObligor(OrginalCase.CaseId).ToList();
            foreach(var v in VaildObligors)
            {
                //var caseall = (from p in ctx.CaseMaster join q in ctx.CaseObligor on p.CaseId equals q.CaseId
                //                where q.ObligorNo == v.ObligorNo && p.CreatedDate>= OrginalCase.CreatedDate && p.GovUnit.Contains("地方法院")
                //                select p).ToList();


                var caseall = asBiz.getOtherSeizure(v.ObligorNo, OrginalCase.CreatedDate);



                if( caseall.Count()>0) // 表示有, 需落人工
                {
                    Doc2Human = true;
                    WriteLog(string.Format("前案中有法院扣押，落人工"));
                    //DocMemo.Add("前案中有法院扣押，落人工");
                    //insertCaseMemo(caseDoc.CaseId, "CaseSeizure", DocMemo, eQueryStaff);
                    List<string> iMemo = new List<string>() { "原扣押案件案號 " + OrginalCase.CaseNo +" 有地院扣押案件編號為 " + caseall.First().CaseNo };
                    asBiz.insertCaseMemo(caseDoc.CaseId, "CaseMemo", iMemo, "SYS");
                    asBiz.insertCaseMemo(caseDoc.CaseId, "CaseSeizure", iMemo, "SYS");
                    //var caseSei = ctx.CaseSeizure.Where(x => x.SeizureStatus == "0" && x.CaseId == OrginalCase.CaseId).ToList();
                    List<CaseSeizure> caseSei = asBiz.getCaseSeizure(OrginalCase.CaseId);
                    asBiz.insertCaseSeizure(OrginalCase, caseDoc, caseSei, eQueryStaff, "0005|前案中有法院扣押，落人工");
                    return "0012|前案資料日期與字號不符，落人工";

                }
            }
            


            

         



            #endregion





            var govNoBrief = caseDoc.GovNo.Substring(0, 3) + caseDoc.GovNo.Substring(caseDoc.GovNo.Length - 7, 6);
            //20200828, 改成新規則
            var govNoBriefNew = getNewMemo(caseDoc.GovUnit, caseDoc.GovNo);


            var OrinalgovNoBrief = OrginalGovNo.Substring(0, 3) + OrginalGovNo.Substring(OrginalGovNo.Length - 7, 6); // 原案在主機上的備註


            // Step 4 : 撤銷該扣押案之前案是否有法院扣押案

            // 所有的訊息
            List<string> SeizureMessage = new List<string>();

            #endregion
            string SeizureResultAll = null;

            //using (CSFS1Entities ctx = new CSFS1Entities())
            {

                //var SeiListAll = ctx.CaseSeizure.Where(x => x.CaseId == OrginalCase.CaseId).ToList();

                List<CaseSeizure> SeiListAll = asBiz.getCaseSeizureByCaseId(OrginalCase.CaseId);


                if (SeiListAll.Count() == 0) // 表示無存款往來
                {
                    WriteLog(string.Format("\t撤銷失敗-->無存款往來 或戶名不符 直接給主管 "));


                    // 20181211, 若有戶名不符備註, 也要加入內部註記
                    //var caMemo = ctx.CaseMemo.Where(x => x.CaseId == OrginalCase.CaseId).ToList();
                    List<CaseMemo> caMemo = asBiz.getCaseMemoById(OrginalCase.CaseId);
                    List<string> pMemo = new List<string>();
                    foreach(var m in caMemo)
                    {
                        if( m.MemoType==CaseMemoType.CaseSeizureMemo && m.Memo.Contains("戶名不符") )
                        {
                            pMemo.Add("原扣押案號 " + OrginalCase.CaseNo + "，回函：戶名不符");                            
                        }
                        // 20190730, 扣押案就有錯... 若是ID無法辨識, 就落人工....
                        if (m.MemoType == CaseMemoType.CaseSeizureMemo && m.Memo.Contains("ID無法辨識"))
                        {
                            pMemo.Add("原扣押案號 " + OrginalCase.CaseNo + "，回函：ID無法辨識");
                        }
                    }
                    if( pMemo.Count()>0)
                    {
                        asBiz.insertCaseMemo(caseDoc.CaseId, "CaseMemo", pMemo, "SYS");
                        asBiz.insertCaseMemo(caseDoc.CaseId, "CaseSeizure", pMemo, "SYS");
                    }
                    else
                    {
                        List<string> iMemo = new List<string>() { "原扣押案號 " + OrginalCase.CaseNo + "，回函：與本行無存款往來" };
                        asBiz.insertCaseMemo(caseDoc.CaseId, "CaseMemo", iMemo, "SYS");
                        asBiz.insertCaseMemo(caseDoc.CaseId, "CaseSeizure", iMemo, "SYS");
                    }
                    SeizureResultAll = "0000|撤銷失敗，無存款往來，直接給主管";
                    return SeizureResultAll;
                }



                if (SeiListAll.Sum(x=>x.SeizureAmount) == 0) // 表示之前扣押0元 ... 但在CaseSeizure中, 有資料
                {
                    WriteLog(string.Format("\t撤銷成功-->之前扣押0元 "));
                    List<string> iMemo = new List<string>() { "前案扣押0元扣押編號為 " + OrginalCase.CaseNo +"。 "};
                    asBiz.insertCaseMemo(caseDoc.CaseId, "CaseMemo", iMemo, "SYS");
                    asBiz.insertCaseMemo(caseDoc.CaseId, "CaseSeizure", iMemo, "SYS");

                    SeizureResultAll = "0000|撤銷成功";
                    // IR-2012, 之前扣押0元的, 本案件區, 要有...
                    var newCaseSeiList = SeiListAll.Where(x => x.SeizureStatus == "0").ToList();
                    asBiz.insertCaseSeizure(OrginCaseInfo, caseDoc, newCaseSeiList, eQueryStaff, "0000|撤銷成功");
                    return SeizureResultAll;
                }
                


                // 若之前有部分金額撤銷, 則集作會將原案整個撤銷, 再新建一個扣押案.. 所以不可能有部分撤銷的情形發生
                // 20181114, 若以前曾經撤銷過, 則不可以再撤銷.. (落人工)
                if (SeiListAll.Any(x => x.CancelCaseId != null))
                {
                    // 計算之前撤銷的金額...到totalCanclAmt
                    decimal totalCanclAmt = 0.0m;
                    foreach (var c in SeiListAll)
                    {
                        if (c.CancelAmount != null)
                            totalCanclAmt += (decimal)c.CancelAmount;
                    }

                    var oneSei = SeiListAll.Where(x => x.CancelCaseId != null).FirstOrDefault();
                    if (totalCanclAmt == 0)
                    {
                        string preCanCaseNo="";
                        CaseMaster preCancelCase = new CaseMaster();
                        if (oneSei != null)
                        {
                            //preCancelCase = ctx.CaseMaster.Where(x => x.CaseId == oneSei.CancelCaseId).OrderByDescending(x=>x.CreatedDate).FirstOrDefault();
                            preCancelCase = asBiz.getPayCase(oneSei.CancelCaseId);
                            if (preCancelCase != null)
                                preCanCaseNo = preCancelCase.CaseNo;
                        }
                        // ===> 條件--> SeiListAll.CancelCaseID 若有填, 且CancelAmt is NULL ...表示是2018,7月份以前的撤銷, .. 一定要落人工
                        WriteLog(string.Format("\t撤銷失敗-->之前有撤銷過, 直接呈主管 "));
                        //DocMemo.Add(string.Format("本扣押案曾經撤銷過，落人工"));
                        //insertCaseMemo(caseDoc.CaseId, "CaseSeizure", DocMemo, eQueryStaff);
                        List<string> iMemo = new List<string>() { "原扣押案號 " + OrginalCase.CaseNo + " 已撤銷，前撤銷案件編號為" + preCanCaseNo };
                        asBiz.insertCaseMemo(caseDoc.CaseId, "CaseMemo", iMemo, "SYS");
                        asBiz.insertCaseMemo(caseDoc.CaseId, "CaseSeizure", iMemo, "SYS");
                        SeizureResultAll = "0000|撤銷成功 直接呈主管";
                        return SeizureResultAll;
                    }
                    else
                    {
                        string preCanCaseNo = "";
                        CaseMaster preCancelCase = new CaseMaster();
                        if (oneSei != null)
                        {
                            //preCancelCase = ctx.CaseMaster.Where(x => x.CaseId == oneSei.CancelCaseId).OrderByDescending(x => x.CreatedDate).FirstOrDefault();
                            preCancelCase = asBiz.getCaseMemoById_lastest(oneSei.CancelCaseId);
                            if (preCancelCase != null)
                                preCanCaseNo = preCancelCase.CaseNo;
                        }
                        // 另外, 若-->SeiListAll.CancelCaseID 有填, 且CancelAmt > 0 時,  只要檢核原扣押金額A=之前撤銷金(B) + 本次撤銷金(C)
                        // 若撤銷只能全部帳號一起撤銷
                        WriteLog(string.Format("\t撤銷失敗-->之前有撤銷過, 直接呈主管  "));
                        //DocMemo.Add(string.Format("本扣押案曾經撤銷過，之前撤銷金額為{0}，落人工", totalCanclAmt.ToString()));
                        //insertCaseMemo(caseDoc.CaseId, "CaseSeizure", DocMemo, eQueryStaff);
                        List<string> iMemo = new List<string>() { "原扣押案號 " + OrginalCase.CaseNo + " 已撤銷，前撤銷案件編號為" + preCanCaseNo };
                        asBiz.insertCaseMemo(caseDoc.CaseId, "CaseMemo", iMemo, "SYS");
                        asBiz.insertCaseMemo(caseDoc.CaseId, "CaseSeizure", iMemo, "SYS");
                        SeizureResultAll = "0000|撤銷成功 直接呈主管";
                        return SeizureResultAll;
                    }


                }








                // 20180919, 
                // 若在CaseMaster中, PayDate=null, 系統日小於PayDate 就可以 代表此扣押案, 
                // 有設付款日, 發9095一定會失敗.. 因此芯瑜說, 直接落人工
                //
                DateTime thenoew = DateTime.Now;

                // if (!(OrginalCase.PayDate == null || thenoew < OrginalCase.PayDate))
                if (! string.IsNullOrEmpty(OrginalCase.PayDate  ))
                {
                    string payCaseNo = "";
                    var oPay = SeiListAll.Where(x => x.PayCaseId != null).FirstOrDefault();
                    if( oPay!=null)
                    {
                        //var payCase = ctx.CaseMaster.Where(x => x.CaseId == oPay.PayCaseId).OrderByDescending(x => x.CreatedDate).FirstOrDefault();
                        CaseMaster payCase = asBiz.getPayCase(oPay.PayCaseId);
                        if (payCase != null)
                            payCaseNo = payCase.CaseNo;
                    }

                    WriteLog(string.Format("\t撤銷失敗-->已設定付款日，落人工 "));
                    //DocMemo.Add(string.Format("已設定付款日，落人工"));
                    //insertCaseMemo(caseDoc.CaseId, "CaseSeizure", DocMemo, eQueryStaff);
                    List<string> iMemo = new List<string>() { "原扣押案號 " + OrginalCase.CaseNo + " 已設定付款日 " + OrginalCase.PayDate.ToString() + " 支付案件編號 " + payCaseNo };
                    asBiz.insertCaseMemo(caseDoc.CaseId, "CaseMemo", iMemo, "SYS");
                    asBiz.insertCaseMemo(caseDoc.CaseId, "CaseSeizure", iMemo, "SYS");

                    SeizureResultAll = "0003|撤銷失敗，已設定付款日，落人工";
                    return SeizureResultAll;
                }


                
                








                // 20181115, 若原有支付案件
                if (SeiListAll.Any(x => x.PayCaseId != null && x.PayAmount > 0))
                {
                    string payCaseNo = "";
                    var oPay = SeiListAll.Where(x => x.PayCaseId != null).FirstOrDefault();
                    if (oPay != null)
                    {
                        //var payCase = ctx.CaseMaster.Where(x => x.CaseId == oPay.PayCaseId).OrderByDescending(x => x.CreatedDate).FirstOrDefault();
                        CaseMaster payCase = asBiz.getPayCase(oPay.PayCaseId);
                        if (payCase != null)
                            payCaseNo = payCase.CaseNo;
                    }
                    // ===> 條件--> SeiListAll.CancelCaseID 若有填, 且CancelAmt is NULL ...表示是2018,7月份以前的撤銷, .. 一定要落人工
                    WriteLog(string.Format("\t撤銷失敗-->之前有支付案 "));
                    //DocMemo.Add(string.Format("本扣押案曾經支付過，落人工"));
                    //insertCaseMemo(caseDoc.CaseId, "CaseSeizure", DocMemo, eQueryStaff);
                    List<string> iMemo = new List<string>() { "原扣押案號 " + OrginalCase.CaseNo + " 已設定付款日 " + OrginalCase.PayDate.ToString() + " 支付案件編號 " + payCaseNo };
                    asBiz.insertCaseMemo(caseDoc.CaseId, "CaseMemo", iMemo, "SYS");
                    asBiz.insertCaseMemo(caseDoc.CaseId, "CaseSeizure", iMemo, "SYS");
                    SeizureResultAll = "0001|撤銷失敗";
                    return SeizureResultAll;
                }






                //var SeiList = SeiListAll.GroupBy(x => x.Account).Select(x => x.OrderByDescending(y => y.CreatedDate).FirstOrDefault()).ToList();



                //if (SeiList.Count() == 0) // 表示之前沒有扣押任何帳戶(或無存款往來), 落人工
                //{

                //    WriteLog(string.Format("\t撤銷失敗-->之前沒有扣押任何帳戶 或 無存款往來 "));
                //    DocMemo.Add(string.Format("沒有扣押任何帳戶 或 無存款往來，落人工"));
                //    insertCaseMemo(caseDoc.CaseId, "CaseSeizure", DocMemo, eQueryStaff);
                //    var noAccouts = ctx.CaseSeizure.Where(x => x.CaseId == OrginalCase.CaseId).ToList();
                //    insertCaseSeizure(OrginCaseInfo, caseDoc, noAccouts, eQueryStaff, "0005|沒有扣押任何帳戶 或 無存款往來");

                //    SeizureResultAll = "0000|撤銷成功，扣押0元";
                //    return SeizureResultAll;

                //}







                var caseSeizureList = SeiListAll.Where(x => x.SeizureAmount > 0 && x.SeizureStatus=="0").ToList();
                WriteLog("======================================================================");
                WriteLog(string.Format("找到原案原扣押帳戶, 共扣押了 {0} 個帳戶", caseSeizureList.Count().ToString()));
                WriteLog(string.Format(" 帳號 / 幣別 / 原幣 / 台幣"));
                foreach (var c in caseSeizureList)
                {
                    WriteLog(string.Format(" {0} / {1} / {2} / {3}", c.Account, c.Currency, c.SeizureAmount.ToString(), c.SeizureAmountNtd.ToString()));
                }
                WriteLog("======================================================================");



                if (caseSeizureList.Count() == 0) // 表示之前扣押0元 ... 但在CaseSeizure中, 有資料
                {
                    WriteLog(string.Format("\t撤銷成功-->之前扣押0元 "));
                    //DocMemo.Add(string.Format("沒有扣押任何帳戶 或 無存款往來，落人工"));
                    //insertCaseMemo(caseDoc.CaseId, "CaseSeizure", DocMemo, eQueryStaff);

                    SeizureResultAll = "0000|撤銷成功";
                    // IR-2012, 之前扣押0元的, 本案件區, 要有...
                    var newCaseSeiList = SeiListAll.Where(x => x.SeizureStatus == "0").ToList();
                    asBiz.insertCaseSeizure(OrginCaseInfo, caseDoc, newCaseSeiList, eQueryStaff, "0000|撤銷成功");
                    return SeizureResultAll;
                }



                bool isFixAccount = false;
                var fixAccount = caseSeizureList.Where(x => x.SeizureAmount > 0 && x.SeizureStatus=="0").ToList();

                foreach (var f in fixAccount)
                {
                    string lastAccount12 = "";
                    if (f.Account.Length >= 12)
                        lastAccount12 = f.Account.Substring(f.Account.Length - 12);
                    if (lastAccount12.StartsWith("006"))
                        isFixAccount = true;
                }

                if (isFixAccount) // 需要撤銷到定存類帳號, 落人工
                {
                    WriteLog(string.Format("\t撤銷失敗-->需要撤銷到定存類帳號，落人工。 "));
                    DocMemo.Add(string.Format("前案扣押有定存，扣押編號為 {0}，落人工。", OrginalCase.CaseNo));
                    //insertCaseMemo(caseDoc.CaseId, "CaseSeizure", DocMemo, eQueryStaff);
                    asBiz.insertCaseMemo(caseDoc.CaseId, "CaseMemo", DocMemo, "SYS");
                    asBiz.insertCaseMemo(caseDoc.CaseId, "CaseSeizure", DocMemo, "SYS");
                    //20181019, 落人工, 也不需要Insert CaseSeizure
                    //insertCaseSeizure(OrginalCase, caseDoc, fixAccount, eQueryStaff, "0003|需要撤銷到定存類帳號, 落人工");
                    SeizureResultAll = "0003|需要撤銷到定存類帳號, 落人工。";
                    return SeizureResultAll;
                }


                int failUnSeizureCount = 0;
                //var OrginalSeizureAccouts = ctx.CaseSeizure.Where(x => x.CaseId == OrginalCase.CaseId).ToList();
                List<CaseSeizure> OrginalSeizureAccouts = asBiz.getCaseSeizureByCaseId(OrginalCase.CaseId);
                List<CaseSeizure> successUnSieuzre = new List<CaseSeizure>();
                foreach (var c in caseSeizureList)
                {
                    WriteLog(string.Format("找到原案原扣押  帳戶 : {0} 原幣 : {1} 台幣 : {2} ", c.Account, c.SeizureAmount, c.SeizureAmountNtd));

                    WriteLog(string.Format("撤銷備註 : {0} 或是 {1}  ", govNoBrief, govNoBriefNew));
                    // 逐一撤銷
                    decimal cSeiAmt = (decimal)c.SeizureAmount;

                    // ---- Start 20181011----------------------------------------------------------------------------
                    var pkid = asBiz.addAutoLog(caseid, eQueryStaff, caseDoc.DocNo, c.CustId, "9095");


                    // 20200828, 先試新規則備註, 若失敗，才試著用舊規則來跑...
                    SeizureResult = objSeiHTG.Send9092Or9095(c.Account, cSeiAmt, c.Currency, "66", govNoBriefNew, c.CustId, c.CaseId.ToString());
                    if (SeizureResult.StartsWith("0000|"))
                    {
                        asBiz.updateAutoLog(pkid, 1, SeizureResult);
                    }
                    else // 用新備註失敗, 試舊備註看看...
                    {
                        SeizureResult = objSeiHTG.Send9092Or9095(c.Account, cSeiAmt, c.Currency, "66", govNoBrief, c.CustId, c.CaseId.ToString());
                        if (SeizureResult.StartsWith("0000|"))
                            asBiz.updateAutoLog(pkid, 1, SeizureResult);
                        else
                        {
                            asBiz.updateAutoLog(pkid, 2, SeizureResult);
                            WriteLog("發查失敗 " + SeizureResult);
                            DocMemo.Add("發查9095失敗，落人工");
                            //asBiz.insertCaseMemo(caseid, "CaseMemo", DocMemo, eQueryStaff);
                            asBiz.insertCaseMemo(caseDoc.CaseId, "CaseMemo", DocMemo, "SYS");
                            asBiz.insertCaseMemo(caseDoc.CaseId, "CaseSeizure", DocMemo, "SYS");
                        }
                    }
                    



                    // 休息1秒, 以免發生錯誤
                    System.Threading.Thread.Sleep(1000);



                    if (SeizureResult.StartsWith("0000|"))
                    {// 撤銷成功
                        WriteLog(string.Format("\t撤銷成功 帳戶 A/C : {0} 原幣 : {1} 台幣 : {2} ", c.Account, c.SeizureAmount, c.SeizureAmountNtd));
                        successUnSieuzre.Add(c);
                    }
                    else // 發查電文失敗.. 也要寫CaseSeizure
                    {
                        if (SeizureResult.Contains("|"))
                        {
                            string[] err = SeizureResult.Split('|');
                            Doc2Human = true;
                            WriteLog(string.Format("\t撤銷失敗帳戶 A/C : {0} 原因{1} ", c.Account, err[1].ToString()));
                            DocMemo.Add(string.Format("帳戶{0} 撤銷失敗, 原因{1}，落人工", c.Account, err[1].ToString()));
                            failUnSeizureCount++;
                        }
                        else
                        {
                            WriteLog(string.Format("\t撤銷失敗帳戶 A/C : {0} ", c.Account));
                            DocMemo.Add(string.Format("帳戶{0} 撤銷失敗，落人工", c.Account));
                            failUnSeizureCount++;
                        }
                        break;
                    }
                }

                // 2. 寫回CaseMemo.. 把備註寫回去... MemoType='CaseMemo'
                //insertCaseMemo(caseid, "CaseSeizure", DocMemo, eQueryStaff);
                asBiz.insertCaseMemo(caseDoc.CaseId, "CaseMemo", DocMemo, "SYS");
                asBiz.insertCaseMemo(caseDoc.CaseId, "CaseSeizure", DocMemo, "SYS");

                if (failUnSeizureCount > 0) // 有失敗帳戶
                {
                    SeizureResultAll = "0001|撤銷失敗";
                    asBiz.insertCaseSeizure(OrginCaseInfo, caseDoc, successUnSieuzre, eQueryStaff, "0000|撤銷有成功的帳戶");
                    var othersAcc = OrginalSeizureAccouts.Except(successUnSieuzre).ToList();
                    if( othersAcc.Count()>0)
                        asBiz.insertCaseSeizure(OrginCaseInfo, caseDoc, othersAcc, eQueryStaff, "0001|撤銷其他的帳戶");
                }
                else
                { // 全部成功
                    SeizureResultAll = "0000|撤銷成功";
                    asBiz.insertCaseSeizure(OrginCaseInfo, caseDoc, successUnSieuzre, eQueryStaff, "0000|撤銷成功");
                    var othersAcc = OrginalSeizureAccouts.Except(successUnSieuzre).ToList();
                    if (othersAcc.Count() > 0)
                        asBiz.insertCaseSeizure(OrginCaseInfo, caseDoc, othersAcc, eQueryStaff, "0001|撤銷成功");
                }

            }


            return SeizureResultAll;
        }







        /// <summary>
        /// // 檢查來文戶名ID是否與原案相同
        /// </summary>
        /// <param name="OrginalCase">原扣押案</param>
        /// <param name="CancelCase">撤銷案</param>
        /// <returns></returns>
        private static bool checkNameNID(CaseMaster OrginalCase, CaseMaster CancelCase)
        {


            
            bool result = true;
            Dictionary<string, string> OrginalObligor = new Dictionary<string, string>();
            Dictionary<string, string> CanelObligor = new Dictionary<string, string>();          



            OrginalObligor = asBiz.getObligor(OrginalCase.CaseId);

            CanelObligor = asBiz.getObligor(CancelCase.CaseId);

            if (OrginalObligor.Count() != CanelObligor.Count())
            {
                result = false;
                return result;
            }

            // 以cancel 為主, 若任何一個找不到, 就false;
            foreach (var c in CanelObligor)
            {
                var o = OrginalObligor.Where(x => x.Key == c.Key && x.Value == c.Value);
                if (o.Count() == 0) // 找不到
                {
                    result = false;
                    break;
                }
            }
            return result;
        }

        /// <summary>
        /// 20181122, 要比對撒銷的來文姓名ID與當初扣押的主機上的姓名與ID
        /// </summary>
        /// <param name="OrginalCase"></param>
        /// <param name="CancelCase"></param>
        /// <returns></returns>
        private static bool checkNameNID2(CaseMaster OrginalCase, CaseMaster CancelCase, ref bool isNoSeiAccount)
        {

            // 20181122, 要比對自動撒銷的來文姓名A  , 與 自動扣押 實際的帳務姓名B ... 而不是 自動扣押來文的姓名C
            // if( A=B ) 才回TRUE

            bool result = true;
            Dictionary<string, string> OrginalObligor = new Dictionary<string, string>();
            Dictionary<string, string> CanelObligor = new Dictionary<string, string>();



            //using (CSFS1Entities ctx = new CSFS1Entities())
            //{
            //    var cs = ctx.CaseSeizure.Where(x => x.CaseId == OrginalCase.CaseId).Count();
            //    if (cs == 0)
            //    {
            //        isNoSeiAccount = true;
            //        return true;
            //    }
            //    else
            //        isNoSeiAccount = false;
            //    OrginalObligor = (from p in ctx.CaseSeizure
            //                      where p.CaseId == OrginalCase.CaseId
            //                      group p by new { p.CustId, p.CustName } into g
            //                      select new { g.Key.CustId, g.Key.CustName }).ToDictionary(x => x.CustId, x => x.CustName);


            //    CanelObligor = ctx.CaseObligor.Where(x => x.CaseId == CancelCase.CaseId).ToDictionary(x => x.ObligorNo.Trim(), x => x.ObligorName.Trim());
            //}


            var cs = asBiz.getCaseSeizureCount(OrginalCase.CaseId);
            if (cs == 0)
            {
                isNoSeiAccount = true;
                return true;
            }
            else
                isNoSeiAccount = false;

            OrginalObligor = asBiz.getOrginalObligor(OrginalCase.CaseId);
            CanelObligor = asBiz.getObligor(CancelCase.CaseId);


            if (CanelObligor.Count()==0)
            {
                result = false;
                return result;
            }

            if (OrginalObligor.Count() != CanelObligor.Count())
            {
                result = false;
                return result;
            }

            // 以cancel 為主, 若任何一個找不到, 就false;
            foreach (var c in CanelObligor)
            {
                var aKey = c.Key.Trim();
                var aVaue = c.Value.Trim();
                var o = OrginalObligor.Where(x => x.Key.Trim() == aKey && x.Value.Trim() == aVaue);
                if (o.Count() == 0) // 找不到
                {
                    result = false;
                    break;
                }
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
                            css.ReceiveList.Add(new CaseSendSettingDetails { CaseId = CaseId, SendType = 1, GovName = master.GovUnit, GovAddr = govAddr });
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
                    SendSettingRef SSref = refBiz.GetSubjectAndDescription(CaseId, css.Template, css.SendKind);
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
                    noticeITMail(ITManagerEmail, "產生發文檔錯誤!, 案件編號: " + master.CaseNo + "錯誤訊息: " + ex.Message.ToString());
                }
            }
            try
            {

                #region 呈核主管
                if (result)
                {

                    string mgrid = AToHBIZ.GetManagerID(eQueryStaff);                    
                    asBiz.insertCaseAssignTable(CaseId, mgrid, 0);
                }
                #endregion

            }
            catch (Exception ex)
            {
                WriteLog("***************************呈核主管錯誤******************************");
                noticeITMail(ITManagerEmail, "呈核主管錯誤!, 案件編號: " + master.CaseNo + "錯誤訊息: " + ex.Message.ToString());
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


        private static bool InsertCaseSendSetting(ref CaseSendSettingCreateViewModel model)
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

                    string sql3 = "INSERT INTO CaseSendSetting(CaseId,[Template],SendDate,SendWord,SendNo,Speed,Security,Subject,Description, Attachment,CreatedUser,CreatedDate,ModifiedUser,SendKind) VALUES ('{0}','{1}', Convert(datetime, '{2}',111),'{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}',GETDATE(),'{11}','{12}')";
                    sql3 = string.Format(sql3, model.CaseId, model.Template, sdate, model.SendWord, model.SendNo, model.Speed, model.Security, model.Subject, model.Description, model.Attachment, eQueryStaff, eQueryStaff, model.SendKind);
                    sqlArray.Add(sql3);
                    var ddd = hbiz.SaveESBData(sqlArray);

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


        private static bool InsertCaseSendSettingDetials(CaseSendSettingDetails model)
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







        private static void noticeITMail(string mailTo, string Message)
        {
            string mailFrom = ConfigurationManager.AppSettings["mailFrom"];
            string[] mailFromTo = mailTo.Split(',');

            string subject = "外來文系統(AutoCancel.exe) 發生錯誤";
            string body = "錯誤訊息：" + Message;
            string host = ConfigurationManager.AppSettings["mailHost"];
            UtlMail.SendEmail(mailFrom, mailFromTo, subject, body, host);
        }





        public static void WriteLog(string msg)
        {
            if (Directory.Exists(@".\Log") == false)
            {
                Directory.CreateDirectory(@".\Log");
            }
            LogManager.Exists("DebugLog").Debug(msg);
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

            string subject =strEnv +  " -- CTBC.WinExe.AutoCancel 外來文系統 RACF 登入錯誤";
            string body = "外來文系統 RACF 登入錯誤，錯誤原因：" + InitMessage;
            string host = ConfigurationManager.AppSettings["mailHost"];
            UtlMail.SendEmail(mailFrom, mailFromTo, subject, body, host);


        }
    }
}
