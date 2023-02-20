using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using CTBC.FrameWork.HTG;
using System.Text;
using System.Threading.Tasks;
using CTBC.FrameWork.Util;
using System.IO;
using System.Timers;
using CTBC.CSFS.BussinessLogic;
using System.Xml;
using System.Configuration;
using CTBC.CSFS.Models;
using CTBC.CSFS.Pattern;
using log4net;
using System.Text.RegularExpressions;
using System.Globalization;

namespace CTBC.WinExe.GetEOS
{
   class Program
   {
      public static string eQueryStaff = null;
      public static string _branchNo = null;
      public static string cs = null;
      public static string LastAgentSetting = null;
      public static bool isDebugMode = false;

      public string initErrorMessage = "";
      static bool sendHTG = true;
      static object _lockLog = new object();
      //ILog log = LogManager.GetLogger("loginfo");
      public static ExecuteHTG objHTG;

      static void Main(string[] args)
      {
          log4net.Config.XmlConfigurator.Configure(new System.IO.FileInfo(AppDomain.CurrentDomain.BaseDirectory + @"\Log.config"));




          try
          {

              string sql = "SELECT * FROM PARMCode WHERE CodeType='eTabsQueryStaff' and Enable='1' Order by SortOrder ";
              CTBC.FrameWork.HTG.HostMsgGrpBIZ hostbiz = new CTBC.FrameWork.HTG.HostMsgGrpBIZ();

              DataTable gOrder = hostbiz.getDataTabe(sql);

              if( gOrder==null)
              {
                  WriteLog("**********************************************************************");
                  WriteLog("****************************無任何可用發查人**************************");
                  WriteLog("**********************************************************************");
                  return;

              }



              try
              {
                  objHTG = new ExecuteHTG();
              }
              catch (Exception ex)
              {

                  WriteLog("****ExecuteHTG 失敗 *" + ex.Message.ToString());
                  return;
              }

              // 刪除Example.log.2018*.*
              string sDir = AppDomain.CurrentDomain.BaseDirectory;
              DirectoryInfo di = new DirectoryInfo(sDir);
              foreach (var s in di.GetFiles("example.log.*"))
              {
                  File.Delete(s.FullName);
                  System.Threading.Thread.Sleep(100);
              }


              WriteLog("====================開始執行GetEOS====================");
              cs = System.Configuration.ConfigurationManager.ConnectionStrings["CSFS_ADO"].ToString();

              PARMCodeBIZ pbiz = new PARMCodeBIZ();
              // 先檢查 select * from [CSFS_SIT].[dbo].[PARMCode] where codetype='eTabsQueryStaff'中的人員的LDAP RACF有效性

              var eTabsStaffs = pbiz.GetParmCodeByCodeType("eTabsQueryStaff").Where(x => (bool)x.Enable).OrderBy(x => x.SortOrder).ToList();
              var eBranchNo = pbiz.GetParmCodeByCodeType("eTabsQueryStaffBranchNo").FirstOrDefault();

              if (eTabsStaffs.Count() == 0)
              {
                  WriteLog("**********************************************************************");
                  WriteLog("****************************目前沒有指定發查人**************************");
                  WriteLog("**********************************************************************");
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




              if (sendHTG)
              {

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
                      WriteLog("**************************** 使用員編: " + e.CodeNo + " 進行發查****************************");



                      if (up.Length == 4) // 一定要順序為
                      {
                          eQueryStaff = up[0].ToString();
                          // 分行別, 要去 select bracnchid from [LDAPEmployee] where empid=up[0]
                          bool isInit = objHTG.HTGInitialize(up[0], up[1], up[2], up[3], _branchNo);
                          if (!isInit)
                          {
                              string InitErrorMessage = objHTG.initErrorMessage;
                              WriteLog("**********************************************************************");
                              WriteLog("**************************** " + up[0].ToString() + " 無法登入, 換下一個發查人****************************");
                              WriteLog("**********************************************************************");
                              var eTabsStaffsMail1 = pbiz.GetParmCodeByCodeType("eTabsQueryStaffNoticeMail").FirstOrDefault();
                              //WriteLog("無法取得SessionID認證!!, 程式結束!!");
                              if (eTabsStaffsMail1 != null)
                              {
                                  string[] mailTo = eTabsStaffsMail1.CodeMemo.Split(',');
                                  //noticeMail(mailTo, InitErrorMessage);
                              }
                              // 若失敗, 則將此人的啟用狀態改成False, 以免下次進來後, 繼續用第一個人...
                              using (CSFSEntities ctx = new CSFSEntities())
                              {
                                  var aaa = ctx.PARMCode.Where(x => x.CodeType == "eTabsQueryStaff" && x.SortOrder == e.SortOrder).FirstOrDefault();
                                  if (aaa != null)
                                  {
                                      aaa.Enable = false;
                                      ctx.SaveChanges();
                                  }
                              }

                          }
                          else // 此人通過
                          {
                              loginStatus = 1;
                              //objSeiHTG = new ExecuteHTG(up[0], up[1], up[2], up[3], _branchNo);
                              break; // foreach 
                          }
                      }
                  }

                  if (loginStatus < 0) // 表示所有人都認證未過.. 
                  {
                      WriteLog("**********************************************************************");
                      WriteLog("****************************" + "所有發查人皆無法登入****************************");
                      WriteLog("**********************************************************************");
                      return;
                  }

                  //WriteLog("取得SessionID認證!!");
                  #endregion
              }

              // 20190716, 加入讀取參數檔的啟始日期, 跟結束日期, 若Enable未啟動, 則以自動用昨天日期去打電文
              var _sDate = pbiz.GetParmCodeByCodeType("getEOSstartDate").Where(x => (bool)x.Enable).FirstOrDefault();
              var _eDate = pbiz.GetParmCodeByCodeType("getEOSendDate").Where(x => (bool)x.Enable).FirstOrDefault();
              if (_sDate != null && _eDate != null)
              {
                  getEOS(_sDate.CodeNo, _eDate.CodeNo);
              }
              else
                  getEOS(null, null);

              WriteLog("====================結束GetEOS====================");
          }
          catch (Exception ex)
          {

              WriteLog("發生錯誤=================================> " + ex.Message.ToString());
          }



      }

      private static void getEOS(string sDate, string eDate)
      {

         string acc_nos = ConfigurationManager.AppSettings["Acc_No"].ToString();

         // 20190716, 目前已無用, 改去讀參數檔, getEOSstartDate
         //string sDate = ConfigurationManager.AppSettings["StartDate"].ToString();
         //目前已無用, 改去讀參數檔, getEOSendDate
         //string eDate = ConfigurationManager.AppSettings["EndDate"].ToString();

         List<string> acc = new List<string>();

         if (string.IsNullOrEmpty(acc_nos))
         {
            WriteLog(string.Format("\t未設定電文發查帳號, 請至App.Config 設定"));
            return;
         }
         else
         {
            WriteLog(string.Format("\t發查帳號 {0} ", acc_nos));
            acc = acc_nos.Split(',').ToList();
         }

         if (acc.Count() < 1)
         {
            WriteLog(string.Format("\t未設定電文發查帳號, 請至App.Config 設定"));
            return;
         }

         if (string.IsNullOrEmpty(sDate) && string.IsNullOrEmpty(eDate)) // 若二個是空的, 就拿昨天的日期
         {
            DateTime dYestoday = DateTime.Now.AddDays(-1);
            sDate = dYestoday.ToString("ddMMyyyy");
            eDate = dYestoday.ToString("ddMMyyyy");
         }

         DateTime WorkDate = new DateTime(1980, 1, 1);
         if (string.IsNullOrEmpty(sDate) && string.IsNullOrEmpty(eDate)) // 若沒有設定打電文日, 則設昨天
         {
            DateTime dYestoday = DateTime.Now.AddDays(-1);
            WorkDate = dYestoday;
         }
         else // 有設打電文日, 設為eDate
         {
            DateTime enddate = DateTime.ParseExact(eDate, "ddMMyyyy", CultureInfo.InvariantCulture);
            WorkDate = enddate;
         }

         WriteLog(string.Format("============================================ {0} ============================================", DateTime.Now.ToString()));
         Dictionary<string, string> dicAcc = new Dictionary<string, string>();
         foreach (string acc_no in acc.OrderBy(x => x))
         {

            WriteLog(string.Format("\t發查20450交易, 帳號{0}", acc_no.ToString()));
            Guid newGuid2 = Guid.NewGuid();
            string s20450 = objHTG.Send20450(acc_no, newGuid2.ToString(), sDate, eDate);
            WriteLog(string.Format("\t發查20450電文, 結果{0}", s20450));
            // 取得今日抓到的DB, 進一步取出銷帳碼
            // select * from TX_20450 where cCretDT = Today && Account in ('712','738')
            if (!s20450.StartsWith("0000|"))
            {
               WriteLog("發查20450失敗");
               return;
            }
            string trnnum = s20450.Replace("0000|", "").Trim();
            dicAcc.Add(acc_no, trnnum);
         }

         List<glTx20450> result20450 = new List<glTx20450>();


         bool isSuccess = true;

         Guid newGuid = Guid.NewGuid();

         List<glTx20450> Result = new List<glTx20450>();

         foreach (KeyValuePair<string, string> kvp in dicAcc)
         {
            using (CSFSEntities ctx = new CSFSEntities())
            {
               var acc712 = ctx.TX_20450.Where(x => (x.Account == kvp.Key) && x.TrnNum == kvp.Value).Where(x => x.DATA1 != "END OF TXN").ToList();

               if (acc712.Count() > 0)
               {
                  string last712JRNO = "";
                  var Result712 = ReFormat450(acc712, kvp.Key, ref last712JRNO);
                  WriteLog(string.Format("\t\t發查20450交易, 帳號{0}, 共有{1}", kvp.Key.ToString(), Result712.Count().ToString()));
                  #region 更新AccountBalance  712
                  if (Result712.Count() > 0)  // 如果當天有打到電文....
                  {
                     Result.AddRange(Result712);
                     if (!string.IsNullOrEmpty(last712JRNO)) // 若有最後一筆的last738JRNO
                     {
                        var las450 = Result712.Where(x => x.JRNL_NO == last712JRNO).FirstOrDefault();
                        if (las450 != null)
                        {
                           WriteLog(string.Format("\t\t 寫入帳號 {0}, 交易日 {1}, 餘額 {2}, JRNO {3} 至 WarningAcctBalance", kvp.Key, las450.txDate.ToString("yyyy/MM/dd"), las450.Balance450.ToString(), last712JRNO));
                           //deleteAccountBalance(las450.txDate, "880010090712"); // 一律刪除

                           var toDel = ctx.WarningAcctBalance.Where(x => x.Account == kvp.Key && x.TXDate == las450.txDate).ToList();
                           if (toDel.Count() > 0)
                           {
                              foreach (var del in toDel)
                              {
                                 ctx.WarningAcctBalance.Remove(del);
                              }
                           }
                           // 準備取當天打20450的最後一筆餘額, 來寫入WarningAcctBalance 
                           WarningAcctBalance lastBalance = new WarningAcctBalance()
                           {
                              Account = kvp.Key,
                              CaseId = newGuid,
                              CreatedId = "Sys",
                              CreatedTime = DateTime.Now,
                              TXDate = las450.txDate,
                              Balance = las450.Balance450 >= 0 ? las450.Balance450.ToString("+00000000000000.000") : las450.Balance450.ToString("00000000000000.000")
                           };
                           ctx.WarningAcctBalance.Add(lastBalance);
                        }

                     }

                  }
                  else // 20190527, 若沒有任何20450的交易, 則要找出上次最後一次的AcctBalance , 重覆新增一筆
                  {
                     var Prev712 = ctx.WarningAcctBalance.Where(x => x.Account == kvp.Key).OrderByDescending(x => x.TXDate).FirstOrDefault();
                     if (Prev712 != null)
                     {
                        WriteLog(string.Format("\t\t 寫入帳號(因為今天沒有交易, 複製上一次交易餘額) {0}, 交易日 {1}, 餘額 {2} 至 WarningAcctBalance", kvp.Key, ((DateTime)Prev712.TXDate).ToString("yyyy/MM/dd"), Prev712.Balance.ToString()));
                        //deleteAccountBalance(new DateTime().Date, "880010090712"); // 一律刪除

                        var toDel = ctx.WarningAcctBalance.Where(x => x.Account == "880010090712" && x.TXDate == WorkDate).ToList();
                        if (toDel.Count() > 0)
                        {
                           foreach (var del in toDel)
                           {
                              ctx.WarningAcctBalance.Remove(del);
                           }
                        }
                        WarningAcctBalance lastBalance = new WarningAcctBalance()
                        {
                           Account = kvp.Key,
                           CaseId = newGuid,
                           CreatedId = "Sys",
                           CreatedTime = DateTime.Now,
                           TXDate = WorkDate,
                           Balance = Prev712.Balance
                        };
                        ctx.WarningAcctBalance.Add(lastBalance);
                     }
                  }
                  #endregion

               }
               else
               {
                  #region (因為今天沒有交易, 複製上一次交易餘額
                  var Prev712 = ctx.WarningAcctBalance.Where(x => x.Account == kvp.Key).OrderByDescending(x => x.TXDate).FirstOrDefault();
                  if (Prev712 != null)
                  {
                     WriteLog(string.Format("\t\t 寫入帳號(因為今天沒有交易, 複製上一次交易餘額) {0}, 交易日 {1}, 餘額 {2} 至 WarningAcctBalance", kvp.Key, ((DateTime)Prev712.TXDate).ToString("yyyy/MM/dd"), Prev712.Balance.ToString()));
                     //deleteAccountBalance(new DateTime().Date, "880010090712"); // 一律刪除

                     var toDel = ctx.WarningAcctBalance.Where(x => x.Account == kvp.Key && x.TXDate == WorkDate).ToList();
                     if (toDel.Count() > 0)
                     {
                        foreach (var del in toDel)
                        {
                           ctx.WarningAcctBalance.Remove(del);
                        }
                     }
                     WarningAcctBalance lastBalance = new WarningAcctBalance()
                     {
                        Account = kvp.Key,
                        CaseId = newGuid,
                        CreatedId = "Sys",
                        CreatedTime = DateTime.Now,
                        TXDate = WorkDate,
                        Balance = Prev712.Balance
                     };
                     ctx.WarningAcctBalance.Add(lastBalance);
                  }
                  #endregion
               }
               ctx.SaveChanges();
            }


         }//end  foreach (KeyValuePair<string, string> kvp in dicAcc)

         WriteLog(string.Format("\t\t取得{0} 筆20450交易", Result.Count().ToString()));

         List<t480base> result480 = new List<t480base>();
         // 發查20480 ///
         #region 發查20480


         var grp = Result.Where(x => !string.IsNullOrEmpty(x.respCode)).DistinctBy(x => x.respCode).ToList();
         WriteLog(string.Format("\t\t發查20480電文 ========================, 共有 {0} 個銷帳碼, \r\n\t\t 分別為 : {1}", grp.Count().ToString(), string.Join(",", grp.Select(x => x.respCode))));
         using (CSFSEntities ctx = new CSFSEntities())
         {
            foreach (var r in grp)
            {
               WriteLog(string.Format("\t\t\t發查20480電文, 銷帳碼{0}", r.respCode));
               string s480 = objHTG.Send20480(r.Account, Guid.NewGuid().ToString(), r.respCode, sDate, eDate);
               if (s480.StartsWith("0000|"))
               {
                  string trnNum = s480.Replace("0000|", "");
                  var v480Lst = ctx.TX_20480.Where(x => x.TrnNum == trnNum && !x.DATA1.StartsWith("END OF TXN")).ToList();
                  int seq = 1;
                  foreach (var v480 in v480Lst)
                  {
                     string[] data480_1 = System.Text.RegularExpressions.Regex.Split(v480.DATA1, @"\s{1,}");
                     if (data480_1.Length > 4)
                     {
                        if (!string.IsNullOrEmpty(data480_1[4]))
                        {
                           decimal newBalance = decimal.Parse(data480_1[4]);
                           result480.Add(new t480base { repcode = r.respCode, bal = newBalance, seq = seq });
                           //result480.Add(r.respCode, newBalance);
                        }
                     }
                     seq++;
                  }
               }
               WriteLog(string.Format("\t\t\t發查20480電文, 結果{0}, 共有{1}", s480, result480.Count().ToString()));
            }
         }
         // 若有打到480的餘額, 則用480的
         foreach (var g450 in Result)
         {
            var r480 = result480.Where(x => x.repcode == g450.respCode).OrderBy(x => x.seq).FirstOrDefault();
            if (r480 != null)
            {

               g450.Balance = r480.bal;
               // 把result480 找到的那筆, 刪除
               result480.Remove(r480);
            }
         }

         #endregion





         WriteLog(string.Format("\t\t 寫入WarningGenAcct ...."));
         // 將Result 新增到回WarningGenAcct

         #region 寫入WarningGenAcct
         using (CSFSEntities ctx = new CSFSEntities())
         {


            foreach (var g in Result.OrderBy(x=>x.Account).ThenBy(x=>x.Seq))
            {

               // 20190527, 與宏祥討論後, 修改為, 若已存在, 採用更新模式, 若不存在, 採用新增模式
               string keyAcctNo = g.Account.PadLeft(16, '0');
               string keyTranDate = "";
               if (g.txDate == new DateTime(1900, 1, 1))
                  keyTranDate = "";
               else
                  keyTranDate = g.txDate.ToString("yyyyMMdd");
               string keyJRNO = g.JRNL_NO.PadLeft(7, '0');

               var isExit = ctx.WarningGenAcct.Where(x => x.TRAN_DATE == keyTranDate && x.JRNL_NO == keyJRNO && x.ACCT_NO == keyAcctNo).FirstOrDefault();
               if (isExit == null)
               {
                  #region 新增模式

                  WarningGenAcct ga = new WarningGenAcct();
                  if (!string.IsNullOrEmpty(g.transAccount))
                     ga.ACCOUNT_NO = g.transAccount.PadLeft(16, '0');
                  else
                     ga.ACCOUNT_NO = "".PadLeft(16, '0');
                  ga.ACCT_NO = g.Account.PadLeft(16, '0');
                  if (g.startDate == new DateTime(1900, 1, 1))
                  {
                     ga.ACT_CCYY = "";
                     ga.ACT_DATE = "";
                     ga.ACT_DATE_TIME = "";
                     ga.ACT_DD = "";
                     ga.ACT_MM = "";
                  }
                  else
                  {
                     ga.ACT_CCYY = g.startDate.ToString("yyyy");
                     ga.ACT_DATE = g.startDate.ToString("yyyyMMdd");
                     ga.ACT_DATE_TIME = g.startDate.ToString("yyyyMMdd") + "000000000000";
                     ga.ACT_DD = g.startDate.ToString("dd");
                     ga.ACT_MM = g.startDate.ToString("MM");
                  }
                  ga.ACT_TIME = "000000000000";
                  ga.AMOUNT = g.Amt >= 0 ? g.Amt.ToString("+00000000000000.000") : g.Amt.ToString("00000000000000.000");
                  ga.BALANCE = g.Balance >= 0 ? g.Balance.ToString("+00000000000000.000") : g.Balance.ToString("00000000000000.000");
                  ga.BALANCE450 = g.Balance450 >= 0 ? g.Balance450.ToString("+00000000000000.000") : g.Balance450.ToString("00000000000000.000");
                  ga.BRANCH_TERM = "";
                  ga.BRANCH = "0495";
                  ga.BTCH_NO_U = "";
                  ga.CHQ_PAYEE = g.respCode;
                  ga.CORRECTION = "";
                  ga.CreatedId = "Sys";
                  ga.CreatedTime = DateTime.Now;
                  ga.DEFER_DAYS = "";
                  ga.DESCR = "";
                  ga.FILLER = "";
                  ga.FOREIGN_FLAG = "";
                  ga.HOME_BRCH = "0495";
                  ga.INST_NO = "003";
                  ga.JRNL_NO = g.JRNL_NO.PadLeft(7, '0');
                  if (g.txDate == new DateTime(1900, 1, 1))
                     ga.POST_DATE = "";
                  else
                     ga.POST_DATE = g.txDate.ToString("yyyyMMdd");
                  ga.SYSTEM = "";
                  ga.TELLER = "";
                  ga.TRAN_CODE = g.txCode.PadLeft(7, '0');

                  if (g.txDate == new DateTime(1900, 1, 1))
                     ga.TRAN_DATE = "";
                  else
                     ga.TRAN_DATE = g.txDate.ToString("yyyyMMdd");
                  ga.TRAN_STATUS = "";
                  ga.TRAN_TYPE = "";
                  ctx.WarningGenAcct.Add(ga);

                  #endregion
               }
               else
               {
                  #region 更新模式
                  if (!string.IsNullOrEmpty(g.transAccount))
                     isExit.ACCOUNT_NO = g.transAccount.PadLeft(16, '0');
                  else
                     isExit.ACCOUNT_NO = "".PadLeft(16, '0');
                  isExit.ACCT_NO = g.Account.PadLeft(16, '0');
                  if (g.startDate == new DateTime(1900, 1, 1))
                  {
                     isExit.ACT_CCYY = "";
                     isExit.ACT_DATE = "";
                     isExit.ACT_DATE_TIME = "";
                     isExit.ACT_DD = "";
                     isExit.ACT_MM = "";
                  }
                  else
                  {
                     isExit.ACT_CCYY = g.startDate.ToString("yyyy");
                     isExit.ACT_DATE = g.startDate.ToString("yyyyMMdd");
                     isExit.ACT_DATE_TIME = g.startDate.ToString("yyyyMMdd") + "000000000000";
                     isExit.ACT_DD = g.startDate.ToString("dd");
                     isExit.ACT_MM = g.startDate.ToString("MM");
                  }
                  isExit.ACT_TIME = "000000000000";
                  isExit.AMOUNT = g.Amt >= 0 ? g.Amt.ToString("+00000000000000.000") : g.Amt.ToString("00000000000000.000");
                  isExit.BALANCE = g.Balance >= 0 ? g.Balance.ToString("+00000000000000.000") : g.Balance.ToString("00000000000000.000");
                  isExit.BALANCE450 = g.Balance450 >= 0 ? g.Balance450.ToString("+00000000000000.000") : g.Balance450.ToString("00000000000000.000");
                  isExit.BRANCH_TERM = "";
                  isExit.BRANCH = "0495";
                  isExit.BTCH_NO_U = "";
                  isExit.CHQ_PAYEE = g.respCode;
                  isExit.CORRECTION = "";
                  isExit.CreatedId = "Sys";
                  isExit.CreatedTime = DateTime.Now;
                  isExit.DEFER_DAYS = "";
                  isExit.DESCR = "";
                  isExit.FILLER = "";
                  isExit.FOREIGN_FLAG = "";
                  isExit.HOME_BRCH = "0495";
                  isExit.INST_NO = "003";
                  isExit.JRNL_NO = g.JRNL_NO.PadLeft(7, '0');
                  if (g.txDate == new DateTime(1900, 1, 1))
                     isExit.POST_DATE = "";
                  else
                     isExit.POST_DATE = g.txDate.ToString("yyyyMMdd");
                  isExit.SYSTEM = "";
                  isExit.TELLER = "";
                  isExit.TRAN_CODE = g.txCode.PadLeft(7, '0');

                  if (g.txDate == new DateTime(1900, 1, 1))
                     isExit.TRAN_DATE = "";
                  else
                     isExit.TRAN_DATE = g.txDate.ToString("yyyyMMdd");
                  isExit.TRAN_STATUS = "";
                  isExit.TRAN_TYPE = "";
                  #endregion
               }
            }
            try
            {
               ctx.SaveChanges();
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException dbEx)
            {
               Exception raise = dbEx;
               foreach (var validationErrors in dbEx.EntityValidationErrors)
               {
                  foreach (var validationError in validationErrors.ValidationErrors)
                  {
                     string message = string.Format("{0}:{1}",
                           validationErrors.Entry.Entity.ToString(),
                           validationError.ErrorMessage);
                     WriteLog(string.Format("\t\t Error ", message));
                  }
               }

               //throw raise;
            }

         }

         #endregion



         WriteLog(string.Format("\t\t 寫入WarningGenAcct, 共{0}筆", Result.Count().ToString()));


         WriteLog(string.Format("--------------------------------------------------------------------------------------------"));

      }


      //private static int deleteAccountBalance(DateTime dt, string account)
      //{
      //    int result = -1;
      //    try
      //    {
      //        using (CSFSEntities ctx = new CSFSEntities())
      //        {
      //            var toDel = ctx.WarningAcctBalance.Where(x => x.Account == account && x.TXDate == dt);
      //            if (toDel.Count() > 0)
      //            {
      //                ctx.WarningAcctBalance.RemoveRange(toDel);
      //                ctx.SaveChanges();
      //            }
      //        }
      //        result = 1;
      //    }
      //    catch(Exception ex )
      //    {
      //        WriteLog("Error " + ex.Message.ToString());
      //        result = -1;
      //    }
      //    return result;
      //}

      /// <summary>
      /// 20190426, 要把TX20450打平, 依照01 的.. 去搭配20, 26開頭的
      /// </summary>
      /// <param name="result450"></param>
      /// <param name="p"></param>
      private static List<glTx20450> ReFormat450(List<TX_20450> result450, string accNo, ref string LastJRNO)
      {
         List<string> r01 = new List<string>();
         List<string> r20 = new List<string>();
         List<string> r36 = new List<string>();

         foreach (var lst in result450)
         {
            if (lst.DATA1.StartsWith("01")) r01.Add(lst.DATA1);
            if (lst.DATA2.StartsWith("01")) r01.Add(lst.DATA2);
            if (lst.DATA3.StartsWith("01")) r01.Add(lst.DATA3);
            if (lst.DATA4.StartsWith("01")) r01.Add(lst.DATA4);
            if (lst.DATA5.StartsWith("01")) r01.Add(lst.DATA5);
            if (lst.DATA6.StartsWith("01")) r01.Add(lst.DATA6);
            if (lst.DATA1.StartsWith("20")) r20.Add(lst.DATA1);
            if (lst.DATA2.StartsWith("20")) r20.Add(lst.DATA2);
            if (lst.DATA3.StartsWith("20")) r20.Add(lst.DATA3);
            if (lst.DATA4.StartsWith("20")) r20.Add(lst.DATA4);
            if (lst.DATA5.StartsWith("20")) r20.Add(lst.DATA5);
            if (lst.DATA6.StartsWith("20")) r20.Add(lst.DATA6);
            if (lst.DATA1.StartsWith("36")) r36.Add(lst.DATA1);
            if (lst.DATA2.StartsWith("36")) r36.Add(lst.DATA2);
            if (lst.DATA3.StartsWith("36")) r36.Add(lst.DATA3);
            if (lst.DATA4.StartsWith("36")) r36.Add(lst.DATA4);
            if (lst.DATA5.StartsWith("36")) r36.Add(lst.DATA5);
            if (lst.DATA6.StartsWith("36")) r36.Add(lst.DATA6);
         }



         string lastJRNL_NO = "";

         List<glTx20450> Result = new List<glTx20450>();
        int seq = 1;
         foreach (var rData in r01)
         {
            string[] data1 = System.Text.RegularExpressions.Regex.Split(rData, @"\s{1,}");
            glTx20450 g = new glTx20450();
            if (data1.Length == 7)
            {
               g.startDate = DateTime.Parse(data1[1]);
               g.txCode = data1[2];
               g.JRNL_NO = data1[3].Trim();
               g.Amt = decimal.Parse(data1[4]);
               g.Balance450 = decimal.Parse(data1[5]);
               g.Balance = 0.0m;
               g.txDate = DateTime.Parse(data1[6]);               
            }
            else
            {
               g.startDate = new DateTime(1900, 1, 1);
               g.txCode = "";
               g.JRNL_NO = "";
               g.Amt = 0;
               g.Balance450 = 0.0m;
               g.Balance = 0.0m;
               g.txDate = new DateTime(1900, 1, 1);
            }
            g.Seq = seq++;
            g.Account = accNo;
            Result.Add(g);
            lastJRNL_NO = g.JRNL_NO;
         }


         foreach (var rData in r20)
         {
            string[] data3 = System.Text.RegularExpressions.Regex.Split(rData, @"\s{1,}");
            //切出JRNL_NO, START DATE, 來找出List<glTX20450> 的那一筆, 來更新
            // <data id="DATA3" value="20 2019/04/23        472400 B1080016"/>
            string jrno = "";
            DateTime dt = new DateTime(1900, 1, 1);
            if (data3.Length >= 4)
            {
               jrno = data3[2].Trim();
               dt = DateTime.Parse(data3[1]);
               var gl450 = Result.Where(x => x.JRNL_NO == jrno && x.startDate == dt && string.IsNullOrEmpty(x.respCode)).FirstOrDefault();
               if (gl450 != null)
               {
                  gl450.respCode = data3[3].Trim();
               }
               //else
               //{
               //   gl450.respCode = "";
               //}
            }
         }


         foreach (var rData in r36)
         {
            string[] data5 = System.Text.RegularExpressions.Regex.Split(rData, @"\s{1,}");
            //切出JRNL_NO, START DATE, 來找出List<glTX20450> 的那一筆, 來更新
            // <data id="DATA3" value="36 2019/04/23        542048 20190423   TRANSFER TO 107540326749"/>
            string jrno = "";
            DateTime dt = new DateTime(1900, 1, 1);
            if (data5.Length >= 3)
            {
               jrno = data5[2].Trim();
               dt = DateTime.Parse(data5[1]);
               var gl450 = Result.Where(x => x.JRNL_NO == jrno && x.startDate == dt && string.IsNullOrEmpty(x.transAccount)).FirstOrDefault();
               if (gl450 == null)
                  gl450.transAccount = "";
               else
               {
                  string tAccount = "";
                  string data5Account = "";
                  if (gl450 != null && rData.Length >= 40)
                  {
                     data5Account = rData.Substring(39);
                     if (data5Account.Length >= 12)
                     {
                        MatchCollection matches = Regex.Matches(data5Account, @"\d{12,16}$", RegexOptions.IgnoreCase);
                        foreach (Match match in matches)
                        {
                           tAccount = match.Groups[0].ToString();
                        }
                     }
                  } // end if (gl450 != null && rData.Length >=40)

                  if (gl450 != null && !string.IsNullOrEmpty(tAccount))
                  {
                     gl450.transAccount = tAccount;
                  }
               }
            }
         }

         var aaa = Result.Where(x => x.JRNL_NO == "472400" || x.JRNL_NO == "542048").ToList();

         LastJRNO = lastJRNL_NO; // 將當天打的最後一筆JRNO 回傳
         return Result;

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


         string subject = strEnv + "-- CTBC.WinExe.GetEOS 外來文系統 RACF 登入錯誤";
         string body = "外來文系統 RACF 登入錯誤，錯誤原因：" + InitMessage;
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

   }

   public class t480base
   {
      public string repcode { get; set; }
      public int seq { get; set; }

      public decimal bal { get; set; }
   }
   public class glTx20450
   {
      public DateTime startDate { get; set; }
      public string txCode { get; set; }
      public decimal Amt { get; set; }
      public decimal Balance { get; set; }
      public decimal Balance450 { get; set; }
      //public string branch { get; set; }
      //public string teller { get; set; }
      public string Account { get; set; }
      public string JRNL_NO { get; set; }
      public DateTime txDate { get; set; }
      public string respCode { get; set; }
      public string transAccount { get; set; }
      public int Seq { get; set; }
   }
}
