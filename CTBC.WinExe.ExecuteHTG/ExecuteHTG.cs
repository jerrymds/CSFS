using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using System.Configuration;
using CTBC.FrameWork.Util;
using CTBC.CSFS.BussinessLogic;
using log4net;
using CTBC.CSFS.Models;
using CTBC.CSFS.Pattern;


namespace ExcuteHTG
{
    class ExecuteHTG
    {

        string HTGUrl;
        string HTGApplication;
        string userId;
        string passWord;
        string branchNo;
        string racfId;
        string racfPassWord;
        HostMsgGrpBIZ hostbiz = new HostMsgGrpBIZ();
        CustomerInfoBIZ custBiz = new CustomerInfoBIZ();
        public static string eQueryStaff = null;
        public static string _branchNo = null;
        public static string _tailNum = "";
        public ExecuteHTG()
        {
            HTGUrl = ConfigurationManager.AppSettings["HTGUrl"].ToString();
            HTGApplication = ConfigurationManager.AppSettings["HTGApplication"].ToString();
            PARMCodeBIZ pbiz = new PARMCodeBIZ();

            var eTabsStaffs = pbiz.GetParmCodeByCodeType("eTabsQueryStaff").Where(x=> (bool)x.Enable).OrderBy(x => x.SortOrder).ToList();
            var eBranchNo = pbiz.GetParmCodeByCodeType("eTabsQueryStaffBranchNo").FirstOrDefault();
            if (eBranchNo != null)
                _branchNo = eBranchNo.CodeDesc;

            if (eTabsStaffs.Count() == 0)
            {
                WriteLog("**********************************************************************");
                WriteLog("****************************目前沒有指定發查人**************************");
                WriteLog("**********************************************************************");
                return;
            }

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
                    bool isInit = HTGInitialize(up[0], up[1], up[2], up[3], _branchNo);
                    if (!isInit)
                    {
                        var eTabsStaffsMail1 = pbiz.GetParmCodeByCodeType("eTabsQueryStaffNoticeMail").FirstOrDefault();
                        if (eTabsStaffsMail1 != null)
                        {
                            string[] mailTo = eTabsStaffsMail1.CodeMemo.Split(',');
                            noticeMail(mailTo);
                        }

                        // 若失敗, 則將此人的啟用狀態改成False, 以免下次進來後, 繼續用第一個人...
                        KDSql kd = new KDSql();
                        string strSQL  ="update [dbo].[PARMCode] set enable=0 where codeuid='" + e.CodeUid.ToString() +"'";
                        kd.SQLServExec(strSQL);


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


            //userId = ConfigurationManager.AppSettings["userId"].ToString();
            //passWord = ConfigurationManager.AppSettings["passWord"].ToString();
            //branchNo = ConfigurationManager.AppSettings["branchNo"].ToString();
            //racfId = ConfigurationManager.AppSettings["racfId"].ToString();
            //racfPassWord = ConfigurationManager.AppSettings["racfPassWord"].ToString();
        }


        private static void noticeMail(string[] mailFromTo)
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

            string subject = strEnv +  "-- CTBC.WinExe.ExecuteHTG 外來文系統 RACF 密碼錯誤";
            string body = "外來文系統 RACF 密碼錯誤";
            string host = ConfigurationManager.AppSettings["mailHost"];
            UtlMail.SendEmail(mailFrom, mailFromTo, subject, body, host);
        }

        public bool HTGInitialize(string _ldapuserId, string _ldappassWord, string _racfId, string _racfPassWord, string _branchNo)
        {
            userId = _ldapuserId;
            passWord = _ldappassWord;
            branchNo = _branchNo;
            racfId = _racfId;
            racfPassWord = _racfPassWord;
            return P4_Initialize();
        }

        private bool P4_Initialize()
        {
            HTGObject obj = new HTGObject(HTGUrl, HTGApplication, userId, passWord, racfId, racfPassWord, branchNo);

            Hashtable htparm = new Hashtable();
            bool result = obj.QueryHtg("00000", htparm);

            string strMessage = obj.ReturnCode["HGExceptionMessage"].ToString();
            if (!string.IsNullOrEmpty(strMessage))
            {
                result = false;
            }
            return result;

        }
        /// <summary>
        /// 實際程序入口
        /// </summary>
        /// <param name="EsbNo">電文ID</param>
        /// <param name="ObligorID">義務人統編</param>
        /// <returns></returns>
        public string HTGMainFun(string HTGNo, string ObligorID, string CustomID, string CaseID, string tailNum)
        {
            Thread.Sleep(300);
            string strResult = "";
            _tailNum = tailNum;
            try
            {
                log4net.Config.XmlConfigurator.Configure(new System.IO.FileInfo(AppDomain.CurrentDomain.BaseDirectory + @"\Log" + tailNum + ".config"));


                //TellerNo = hostbiz.GetTellerNo();
                //BranchID = hostbiz.GetBranchId();
                string strYear = Convert.ToInt16(DateTime.Now.Year - 1911).ToString("000");
                string strMonth = Convert.ToInt16(DateTime.Now.Month).ToString("00");
                string strDay = Convert.ToInt16(DateTime.Now.Day).ToString("00");
                string strHour = System.DateTime.Now.ToString("HHmmssfff");
                string strSno = "CSFS" + strYear.Substring(1, 2) + strMonth + strDay + strHour;

                WriteLog(string.Format("{0}\t電文:{1}\t 義務人:{2} : 批號: {3} ",DateTime.Now.ToString("MM/dd hh:mm:ss") , HTGNo, ObligorID, strSno));
                WriteLog("----------START----------");
                switch (HTGNo)
                {
                    case "60491":                        
                        return P4_60491U(ObligorID, CaseID, strSno);                        
                        break;
                    case "67072":
                        return P4_67072U(ObligorID, CaseID, strSno);
                        break;
                    case "60629":
                        return P4_60629U(ObligorID, CaseID, strSno);
                        break;
                    //case "33401":
                    //    //return P4_33401U(ObligorID, CustomID, CaseID);
                    //    break;
                    //20160122 RC --> 20150106 宏祥 add 新增67100電文
                    case "67100":
                        return P4_67100U(ObligorID, CaseID, strSno);
                        break;
                    case "67050":
                        return P4_67050U(ObligorID, CaseID, strSno);
                        break;
                    case "009001":
                        return Login();
                        break;
                    case "":
                        return LogOut();

                }
            }
            catch (Exception ex)
            {
                WriteLog("程式異常，錯誤信息：" + ex.Message);
                strResult = "0002|程式異常";
            }
            return strResult;
        }

        private string P4_60629U(string ObligorID, string CaseID,string strSno)
        {

            HTGObject obj = new HTGObject(HTGUrl, HTGApplication, userId, passWord, racfId, racfPassWord, branchNo);

            Hashtable htparm = new Hashtable();

            htparm.Add("CUST_ID_NO", ObligorID);

            bool result = obj.QueryHtg("060629", htparm);

            string strMessage = obj.ReturnCode["HGExceptionMessage"].ToString();
            if (!string.IsNullOrEmpty(strMessage))
            {
                result = false;
            }

            string log;

            if (!result) //此為程式發生Exception或電文發送回應錯誤
            {
                //執行過程的log
                log = obj.MessageLog;
                //[HTG 的回應碼] htg return code 
                string ErrorCode = obj.ReturnCode["HtgReturnCode"].ToString();
                //[HTG 的回應訊息] htg return message
                string ErrorMessage = obj.ReturnCode["HtgMessage"].ToString();
                //[主機回應訊息]main frame return message 
                string MFMessage = obj.ReturnCode["HGExceptionMessage"].ToString();
                WriteLog(log);
                return "0001|發查60629電文失敗" + MFMessage;
            }
            else // 與主機交易執行結果成功,取回交易結果的資料集, 註: Result = true並不代表一定有資料傳,因為可能在所傳入的查詢條件下並無符合的資料,但交易是成功的
            {
                //執行過程的log
                log = obj.MessageLog;
                //電文資料集
                DataSet returnDS = obj.HtgDataSet;


                #region 開始儲存至DB

                var masterCount = returnDS.Tables["TX_60629"].Rows.Count;
                bool isSuccess = P4_SinglePage_SaveToDB(returnDS, CaseID, ObligorID, "60629", strSno);
                if (!isSuccess)
                {
                    WriteLog(log);
                    return "0002|儲存60629DB失敗";
                }
                #endregion
                Hashtable returnHT = obj.ReturnCode;
                WriteLog(log);
                return "0000|" + strSno;
            }
        }


        private string P4_67100U(string obligorID, string caseID,string strSno)
        {

            HTGObject obj = new HTGObject(HTGUrl, HTGApplication, userId, passWord, racfId, racfPassWord, branchNo);

            Hashtable htparm = new Hashtable();

            htparm.Add("CUST_ID_NO", obligorID);



            bool result = obj.QueryHtg("067100", htparm);
            string strMessage = obj.ReturnCode["HGExceptionMessage"].ToString();
            if (!string.IsNullOrEmpty(strMessage))
            {
                result = false;
            }

            string log;

            if (!result) //此為程式發生Exception或電文發送回應錯誤
            {
                //執行過程的log
                log = obj.MessageLog;
                //[HTG 的回應碼] htg return code 
                string ErrorCode = obj.ReturnCode["HtgReturnCode"].ToString();
                //[HTG 的回應訊息] htg return message
                string ErrorMessage = obj.ReturnCode["HtgMessage"].ToString();
                //[主機回應訊息]main frame return message 
                string MFMessage = obj.ReturnCode["HGExceptionMessage"].ToString();
                WriteLog(log);
                return "0001|電文67100 發查失敗, 原因" + MFMessage;
            }
            else // 與主機交易執行結果成功,取回交易結果的資料集, 註: Result = true並不代表一定有資料傳,因為可能在所傳入的查詢條件下並無符合的資料,但交易是成功的
            {
                //執行過程的log
                log = obj.MessageLog;
                //電文資料集
                DataSet returnDS = obj.HtgDataSet;


                #region 開始儲存至DB

                var masterCount = returnDS.Tables["TX_67100"].Rows.Count;
                bool isSuccess = P4_67100_SaveToDB(returnDS, caseID, obligorID, strSno);
                if (!isSuccess)
                {
                    WriteLog(log);
                    return "0002|儲存607100DB失敗";
                }




                #endregion


                


                //return code
                Hashtable returnHT = obj.ReturnCode;

            }
            WriteLog(log);
            return "0000|" + strSno;
        }


        private string P4_67050U(string obligorID, string caseID, string strSno)
        {

            HTGObject obj = new HTGObject(HTGUrl, HTGApplication, userId, passWord, racfId, racfPassWord, branchNo);

            Hashtable htparm = new Hashtable();

            htparm.Add("CUST_ID_NO", obligorID);
             bool result = obj.QueryHtg("067050", htparm);
             string strMessage = obj.ReturnCode["HGExceptionMessage"].ToString();
             if (!string.IsNullOrEmpty(strMessage))
             {
                 result = false;
             }
            string log;

            if (!result) //此為程式發生Exception或電文發送回應錯誤
            {
                //執行過程的log
                log = obj.MessageLog;
                //[HTG 的回應碼] htg return code 
                string ErrorCode = obj.ReturnCode["HtgReturnCode"].ToString();
                //[HTG 的回應訊息] htg return message
                string ErrorMessage = obj.ReturnCode["HtgMessage"].ToString();
                //[主機回應訊息]main frame return message 
                string MFMessage = obj.ReturnCode["HGExceptionMessage"].ToString();
                WriteLog(log);
                return "0001|電文67050 發查失敗, 原因" + MFMessage;
            }
            else // 與主機交易執行結果成功,取回交易結果的資料集, 註: Result = true並不代表一定有資料傳,因為可能在所傳入的查詢條件下並無符合的資料,但交易是成功的
            {
                //執行過程的log
                log = obj.MessageLog;
                //電文資料集
                DataSet returnDS = obj.HtgDataSet;


                #region 開始儲存至DB

                var masterCount = returnDS.Tables["TX_67002"].Rows.Count;
                //bool isSuccess = P4_67100_SaveToDB(returnDS, caseID, obligorID);
                bool isSuccess = P4_SinglePage_SaveToDB(returnDS, caseID, obligorID, "67002", strSno);
                if (!isSuccess)
                {
                    WriteLog(log);
                    return "0002|儲存607100DB失敗";
                }




                #endregion





                //return code
                Hashtable returnHT = obj.ReturnCode;

            }
            WriteLog(log);
            return "0000|" + strSno;
        }


        private string P4_67072U(string obligorID, string caseID, string strSno)
        {

            HTGObject obj = new HTGObject(HTGUrl, HTGApplication, userId, passWord, racfId, racfPassWord, branchNo);

            Hashtable htparm = new Hashtable();
            htparm.Add("TXTR", "N");
            htparm.Add("OPTION", "0");
            htparm.Add("CUST_NO", "0000000000000000");
            htparm.Add("CUST_ID_NO", obligorID);
            htparm.Add("ACTION", "N");

            bool result = obj.QueryHtg("067072", htparm);
            string strMessage = obj.ReturnCode["HGExceptionMessage"].ToString();
            if (!string.IsNullOrEmpty(strMessage))
            {
                result = false;
            }

            string log;

            if (!result) //此為程式發生Exception或電文發送回應錯誤
            {
                //執行過程的log
                log = obj.MessageLog;
                //[HTG 的回應碼] htg return code 
                string ErrorCode = obj.ReturnCode["HtgReturnCode"].ToString();
                //[HTG 的回應訊息] htg return message
                string ErrorMessage = obj.ReturnCode["HtgMessage"].ToString();
                //[主機回應訊息]main frame return message 
                string MFMessage = obj.ReturnCode["HGExceptionMessage"].ToString();
                WriteLog(log);
                return "0001|電文67072 發查失敗, 原因" + MFMessage;
            }
            else // 與主機交易執行結果成功,取回交易結果的資料集, 註: Result = true並不代表一定有資料傳,因為可能在所傳入的查詢條件下並無符合的資料,但交易是成功的
            {
                //執行過程的log
                log = obj.MessageLog;
                //電文資料集
                DataSet returnDS = obj.HtgDataSet;


                #region 開始儲存至DB

                var masterCount = returnDS.Tables["TX_67072_Grp"].Rows.Count;
                var detailCount = returnDS.Tables["TX_67072_detl"].Rows.Count;
                bool isSuccess = P4_MultiPage_SaveToDB(returnDS, caseID, obligorID, "67072", strSno);
                
                if (!isSuccess)
                {
                    WriteLog(log);
                    return "0002|儲存60491DB失敗";
                }


                

                #endregion





                //return code
                Hashtable returnHT = obj.ReturnCode;
                WriteLog(log);
                return "0000|" + strSno;

            }



            //using (StreamWriter sw = new StreamWriter("MessageLog" + DateTime.Now.ToString("MMdd"), false, Encoding.UTF8))
            //{
            //    sw.WriteLine(log);
            //}

            WriteLog(log);
            return "0000|";
        }

        private string P4_60491U(string obligorID, string caseID, string strSno)
        {
            HTGObject obj = new HTGObject(HTGUrl, HTGApplication, userId, passWord, racfId, racfPassWord, branchNo);

            Hashtable htparm = new Hashtable();
            htparm.Add("CURRENCY_TYPE", "0");
            htparm.Add("CUSTOMER_NO", "");
            htparm.Add("ID_TYPE", "01");
            htparm.Add("CLOSED_DATE", "05102016");
            htparm.Add("applicationId", "CSFS");
            htparm.Add("ENQUIRY_OPTION", "S");
            htparm.Add("CUST_ID_NO", obligorID);
            htparm.Add("ACCOUNT_STATUS", "1");
            htparm.Add("PRODUCT_OPTION", "ALL");


            bool result = obj.QueryHtg("060491", htparm);

            string strMessage = obj.ReturnCode["HGExceptionMessage"].ToString();
            if (!string.IsNullOrEmpty(strMessage))
            {
                result = false;
            }
                
            string log;

            if (!result) //此為程式發生Exception或電文發送回應錯誤
            {
                //執行過程的log
                log = obj.MessageLog;
                //[HTG 的回應碼] htg return code 
                string ErrorCode = obj.ReturnCode["HtgReturnCode"].ToString();
                //[HTG 的回應訊息] htg return message
                string ErrorMessage = obj.ReturnCode["HtgMessage"].ToString();
                //[主機回應訊息]main frame return message 
                string MFMessage = obj.ReturnCode["HGExceptionMessage"].ToString();
                WriteLog(log);
                return "0001|電文60491 發查失敗, 原因" + MFMessage;
            }
            else // 與主機交易執行結果成功,取回交易結果的資料集, 註: Result = true並不代表一定有資料傳,因為可能在所傳入的查詢條件下並無符合的資料,但交易是成功的
            {
                //執行過程的log
                log = obj.MessageLog;
                //電文資料集
                DataSet returnDS = obj.HtgDataSet;


                #region 開始儲存至DB

                var masterCount = returnDS.Tables["TX_60491_Grp"].Rows.Count;
                var detailCount = returnDS.Tables["TX_60491_detl"].Rows.Count;
                
                bool isSuccess = P4_MultiPage_SaveToDB(returnDS, caseID, obligorID,"60491", strSno);
                if (!isSuccess)
                {
                    WriteLog(log);
                    return "0002|儲存60491DB失敗";
                }

                


                #endregion

                WriteLog(log);




                #region 發查33401, 45030 , 並更新returnDS.Detail


                var detailCount2 = returnDS.Tables["TX_60491_detl"].Rows.Count;
                WriteLog("開始發查33401, 共" + detailCount2.ToString() + "筆帳戶");

                foreach(DataRow dr in returnDS.Tables["TX_60491_Detl"].Rows)
                {
                    // 20190104, 未來, 可能要過濾一些帳戶類型,不用打33401 ...
                    // 可以呼叫isValidAccount() 方法, 來確認是否要打電文....


                    #region 發查33401
                    Hashtable htparm401 = new Hashtable();
                    htparm401.Add("CURRENCY", dr["Ccy"].ToString());
                    htparm401.Add("applicationId", "CSFS");
                    htparm401.Add("ACCT_NO", dr["Account"].ToString());
                    //var aaa = dr["Account"].ToString();
                    //string bbb = aaa;
                    bool result401  = obj.QueryHtg("033401", htparm401);
                    string strMessage1 = obj.ReturnCode["HGExceptionMessage"].ToString();
                    if (!string.IsNullOrEmpty(strMessage1))
                    {
                        result401 = false;
                    }

                    string errorlog = obj.MessageLog;
                    if (result401)
                    {
                        Hashtable errormessage = obj.ReturnCode;
                        //var errorlog = obj.MessageLog;
                        DataSet return401 = obj.HtgDataSet;
                        int rowCount = return401.Tables["TX_33401"].Rows.Count;
                        bool isSuccess1 = P4_33401_SaveToDB(return401, caseID, obligorID, strSno);
                        if (!isSuccess1)
                        {
                            WriteLog(errorlog);
                            return "0002|儲存33401DB失敗";
                        }                        
                    }
                    else
                    {
                        WriteLog(errorlog);
                        return "0001|發查33401失敗, 原因" + strMessage1;
                    }
                    WriteLog(errorlog);
                    #endregion



                    //---20190104 ---
                    dr["StsDesc"] = "事故";
                    if (dr["StsDesc"].ToString().Trim() == "事故")
                    {
                        #region 發查45030
                        Hashtable htparm450 = new Hashtable();
                        htparm450.Add("ACCT_NO", dr["Account"].ToString());
                        htparm450.Add("LIST_TX_FR_NO", "1");
                        htparm450.Add("LIST_TX_DT_FR", "01011991");
                        htparm450.Add("LIST_TX_DT_TO", DateTime.Now.ToString("ddMMyyyy"));
                        htparm450.Add("TX_TYPE", "30");
                        htparm450.Add("CURRENCY", dr["Ccy"].ToString());
                        htparm450.Add("OPTION", "00");
                        bool result450 = obj.QueryHtg("00450", htparm450);
                        string strMessage2 = obj.ReturnCode["HGExceptionMessage"].ToString();
                        if (!string.IsNullOrEmpty(strMessage2))
                        {
                            result450 = false;
                        }

                        string errorlog2 = obj.MessageLog;
                        if (result450)
                        {
                            Hashtable errormessage = obj.ReturnCode;
                            //var errorlog = obj.MessageLog;
                            DataSet return450 = obj.HtgDataSet;
                            int rowCount = return450.Tables["TX_00450"].Rows.Count;
                            
                            bool isSuccess1 = P4_00450_SaveToDB(return450, caseID, obligorID, dr["Account"].ToString(), strSno, "30");
                            if (!isSuccess1)
                            {
                                WriteLog(errorlog2);
                                return "0002|儲存00450DB失敗";
                            }
                        }
                        else
                        {
                            WriteLog(errorlog2);
                            return "0001|發查45030失敗, 原因" + strMessage2;
                        }
                        WriteLog(errorlog2);
                        #endregion
                    }
                }

                
                #endregion


                        
                //return code
                Hashtable returnHT = obj.ReturnCode;
                WriteLog(log);
                return "0000|" + strSno;
            }



            //using (StreamWriter sw = new StreamWriter("MessageLog" + DateTime.Now.ToString("MMdd"), false, Encoding.UTF8))
            //{
            //    sw.WriteLine(log);
            //}


            return "0000|";
        }

        /// <summary>
        /// 20190104, 備用, 準備過濾不需要打電文的帳號 ( 排除 "結清", "已貸", "啟用", "誤開", "新戶","核淮","婉拒","作廢"  的帳戶)
        /// </summary>
        /// <param name="Account"></param>
        /// <param name="ProdCode"></param>
        /// <param name="Link"></param>
        /// <param name="StsDesc"></param>
        /// <returns></returns>
        private bool isValidAccount(string Account, string ProdCode, string Link, string StsDesc)
        {
            #region  排除 "結清", "已貸", "啟用", "誤開", "新戶" 的帳戶
            
            List<string> noSave = new List<string>() { "結清", "已貸", "啟用", "誤開", "新戶","核淮","婉拒","作廢" };

            {
                bool bfilter = true;
                if (Account.StartsWith("000000000000"))
                    bfilter = false;

                #region 判斷是否是現金卡等等
                // 若 prod_code = 0058, 或XX80 , 不用存
                if (ProdCode.ToString().Equals("0058") || ProdCode.ToString().EndsWith("80"))
                    bfilter = false;

                // 若  Link<>'JOIN' , 不用存
                if (Link.ToString().Equals("JOIN"))
                    bfilter = false;

                // 若 StsDesc='結清' AND  StsDesc='已貸' AND  StsDesc='啟用' AND  StsDesc='誤開'  AND  StsDesc='新戶', 也不用存
                string sdesc = StsDesc.ToString().Trim();
                if (noSave.Contains(sdesc))
                    bfilter = false;

                #endregion

                return bfilter;
            }


            #endregion
        }


        private bool P4_00450_SaveToDB(DataSet htgdata, string CaseID, string ObligorID, string account,string strSno, string WXOption="30" )
        {
            //(DataSet htgdata, string CaseID, string ObligorID,string WXOption, string account, string strSno)
            HostMsgGrpBIZ hostbiz = new HostMsgGrpBIZ();
            ArrayList array = new ArrayList();

            {
                DataTable dt = htgdata.Tables["TX_00450"];
                string strTableName = dt.TableName;

                {
                    #region 主表
                    if (dt.Rows.Count > 0)
                    {
                        //string strIdentityKey = hostbiz.GetIdentityKey(strTableName);
                        foreach (DataRow dr in dt.Rows)
                        {
                            string strSql = "insert into " + strTableName + " (";
                            strSql += "[cCretDT],caseId,[WXOption],[Account],TrnNum,";
                            //DataTable dtNew = dic[key];
                            for (int i = 0; i < dt.Columns.Count; i++)
                            {
                                strSql += "[" + dt.Columns[i].ColumnName + "],";
                            }
                            strSql = strSql.TrimEnd(',') + ") values(";
                            strSql += "GETDATE(),'" + CaseID + "','" + WXOption + "','" + account + "','" + strSno + "',";
                            for (int i = 0; i < dt.Columns.Count; i++)
                            {
                                string strColumnName = dt.Columns[i].ColumnName;
                                strSql += "'" + dr[strColumnName].ToString() + "',";
                            }
                            strSql = strSql.TrimEnd(',') + ");";
                            array.Add(strSql);
                        }
                    }
                    #endregion
                }
            }
            //* 實際將sql 元組儲存
            bool flagresult = hostbiz.SaveESBData(array);

            return flagresult;
        }




        private bool P4_67100_SaveToDB(DataSet htgdata, string CaseID, string ObligorID, string strSno)
        {

            HostMsgGrpBIZ hostbiz = new HostMsgGrpBIZ();
            ArrayList array = new ArrayList();
            foreach (DataTable dt in htgdata.Tables)
            {
                string strTableName = dt.TableName; //eg .  TX_33401

                //string strLastName = strTableName.Substring(strTableName.Length - 3).ToUpper();
                //
                {
                    #region 主表
                    if (dt.Rows.Count > 0)
                    {
                        string strIdentityKey = hostbiz.GetIdentityKey(strTableName);
                        foreach (DataRow dr in dt.Rows)
                        {
                            string strSql = "insert into " + strTableName + " (";
                            strSql += "[CifNo],[cCretDT],caseId,";
                            //strSql += "[SNO],[cCretDT],caseId,";
                            //strSql += "[SNO],[cCretDT],caseId,";
                            //DataTable dtNew = dic[key];
                            for (int i = 0; i < dt.Columns.Count; i++)
                            {
                                strSql += "[" + dt.Columns[i].ColumnName + "],";
                            }
                            strSql = strSql.TrimEnd(',') + ") values(";
                            //strSql += "'" + strIdentityKey + "',GETDATE(),'" + CaseID + "',";
                            //strSql += "'" + strIdentityKey + "',GETDATE(),'" + CaseID + "',";
                            strSql += "'" + strIdentityKey + "',GETDATE(),'" + CaseID + "','";
                            for (int i = 0; i < dt.Columns.Count; i++)
                            {
                                string strColumnName = dt.Columns[i].ColumnName;
                                strSql += "'" + dr[strColumnName].ToString() + "',";
                            }
                            strSql = strSql.TrimEnd(',') + ");";
                            array.Add(strSql);
                            //using (StreamWriter sw = new StreamWriter("33401-" + DateTime.Now.ToString("MMdd") + ".csv", true, Encoding.UTF8))
                            //{
                            //    //string sendstr =
                            //    sw.WriteLine(dr["Acct"].ToString() + "\t" + dr["CurBal"].ToString() + "\t" + dr["Currency"].ToString() + "\t" + dr["CustId"].ToString());
                            //}
                        }
                    }
                    #endregion
                }
            }
            //* 實際將sql 元組儲存
            bool flagresult = hostbiz.SaveESBData(array);

            return flagresult;

        }


        private bool P4_MultiPage_SaveToDB(DataSet htgdata, string CaseID, string ObligorID, string TxID, string strSno)
        {

            HostMsgGrpBIZ hostbiz = new HostMsgGrpBIZ();
            ArrayList array = new ArrayList();
            string strTN = "TX_" + TxID + "_Grp";

            DataTable dt = htgdata.Tables[strTN];
            string strIdentityKey = null;
            #region 主表
            if (dt.Rows.Count > 0)
            {
                string strTableName = dt.TableName;
                strIdentityKey = hostbiz.GetIdentityKey(strTableName);
                string strSql = "insert into " + strTableName + " (";
                strSql += "[SNO],[cCretDT],caseId,TrnNum,";
                //DataTable dtNew = dic[key];
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    strSql += "[" + dt.Columns[i].ColumnName + "],";
                }
                strSql = strSql.TrimEnd(',') + ") values(";
                strSql += "'" + strIdentityKey + "',GETDATE(),'" + CaseID + "','" + strSno + "',";
                for (int i = 0; i < dt.Columns.Count; i++)
                {

                    string strColumnName = dt.Columns[i].ColumnName;
                    string aaa = dt.Rows[0][strColumnName].ToString();
                    strSql += "'" + dt.Rows[0][strColumnName].ToString() + "',";
                    // strSql += "'" + dt.Columns[i].ColumnName + "',";

                }
                strSql = strSql.TrimEnd(',') + ");";
                array.Add(strSql);
            }
            #endregion


            strTN = "TX_" + TxID + "_Detl";
            dt = htgdata.Tables[strTN];

            #region 從表

            string strTableName1 = dt.TableName;
            //string strIdentityKey = hostbiz.GetIdentityKey(strTableName);
            // strIndentityKey 淮備給Detail填入FKSNO那個欄位用
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                string strSql = "insert into " + strTableName1 + " (";
                strSql += "[SNO],[FKSNO],[CUST_ID],caseId,";
                for (int j = 0; j < dt.Columns.Count; j++)
                {
                    strSql += "[" + dt.Columns[j].ColumnName + "],";
                }
                strSql = strSql.TrimEnd(',') + ") values(";
                strSql += "NEXT VALUE FOR SEQ" + strTableName1 + ",'" + strIdentityKey + "','" + ObligorID + "','" + CaseID + "',";
                for (int j = 0; j < dt.Columns.Count; j++)
                {
                    string strColumnName = dt.Columns[j].ColumnName;

                    strSql += "'" + dt.Rows[i][strColumnName].ToString() + "',";

                }
                strSql = strSql.TrimEnd(',') + ");";
                array.Add(strSql);
            }
            #endregion


            //* 實際將sql 元組儲存
            bool flagresult = hostbiz.SaveESBData(array);

            return flagresult;

        }


        private bool P4_33401_SaveToDB(DataSet htgdata, string CaseID, string ObligorID, string strSno)
        {

            HostMsgGrpBIZ hostbiz = new HostMsgGrpBIZ();
            ArrayList array = new ArrayList();
            foreach (DataTable dt in htgdata.Tables)
            {
                string strTableName = dt.TableName; //eg .  TX_33401

                //string strLastName = strTableName.Substring(strTableName.Length - 3).ToUpper();
                //
                {
                    #region 主表
                    if (dt.Rows.Count > 0)
                    {
                        string strIdentityKey = hostbiz.GetIdentityKey(strTableName);
                        foreach (DataRow dr in dt.Rows)
                        {
                            string strSql = "insert into " + strTableName + " (";
                            strSql += "[SNO],[cCretDT],caseId,TrnNum,";
                            //DataTable dtNew = dic[key];
                            for (int i = 0; i < dt.Columns.Count; i++)
                            {
                                strSql += "[" + dt.Columns[i].ColumnName + "],";
                            }
                            strSql = strSql.TrimEnd(',') + ") values(";
                            strSql += "'" + strIdentityKey + "',GETDATE(),'" + CaseID + "','" + strSno + "',";
                            for (int i = 0; i < dt.Columns.Count; i++)
                            {
                                string strColumnName = dt.Columns[i].ColumnName;
                                strSql += "'" + dr[strColumnName].ToString() + "',";
                            }
                            strSql = strSql.TrimEnd(',') + ");";
                            array.Add(strSql);
                            //using (StreamWriter sw = new StreamWriter("33401-" + DateTime.Now.ToString("MMdd") + ".csv", true, Encoding.UTF8))
                            //{
                            //    //string sendstr =
                            //    sw.WriteLine(dr["Acct"].ToString() + "\t" + dr["CurBal"].ToString() + "\t" + dr["Currency"].ToString() + "\t" + dr["CustId"].ToString());
                            //}
                        }
                    }
                    #endregion
                }
            }
            //* 實際將sql 元組儲存
            bool flagresult = hostbiz.SaveESBData(array);

            return flagresult;

        }

        private bool P4_SinglePage_SaveToDB(DataSet htgdata, string CaseID, string ObligorID, string TxID, string strSno)
        {

            HostMsgGrpBIZ hostbiz = new HostMsgGrpBIZ();
            ArrayList array = new ArrayList();
            //foreach (DataTable dt in htgdata.Tables)
            {
                string strTN = "TX_" + TxID;

                DataTable dt = htgdata.Tables[strTN];
                string strTableName = dt.TableName; //eg .  TX_33401

                //string strLastName = strTableName.Substring(strTableName.Length - 3).ToUpper();
                //
                {
                    #region 主表
                    if (dt.Rows.Count > 0)
                    {
                        //string strIdentityKey = hostbiz.GetIdentityKey(strTableName);
                        foreach (DataRow dr in dt.Rows)
                        {
                            string strSql = "insert into " + strTableName + " (";
                            strSql += "[cCretDT],caseId, TrnNum,";
                            //DataTable dtNew = dic[key];
                            for (int i = 0; i < dt.Columns.Count; i++)
                            {
                                strSql += "[" + dt.Columns[i].ColumnName + "],";
                            }
                            strSql = strSql.TrimEnd(',') + ") values(";
                            strSql += "GETDATE(),'" + CaseID + "','" + strSno + "',";
                            for (int i = 0; i < dt.Columns.Count; i++)
                            {
                                string strColumnName = dt.Columns[i].ColumnName;
                                strSql += "'" + dr[strColumnName].ToString() + "',";
                            }
                            strSql = strSql.TrimEnd(',') + ");";
                            array.Add(strSql);
                        }
                    }
                    #endregion
                }
            }
            //* 實際將sql 元組儲存
            bool flagresult = hostbiz.SaveESBData(array);

            return flagresult;

        }


        public string Login()
        {
            return null;
        }

        public string LogOut()
        {
            return null;
        }


        public void WriteLog(string msg)
        {
            if (Directory.Exists(@".\Log" + _tailNum) == false)
            {
                Directory.CreateDirectory(@".\Log" + _tailNum);
            }
            LogManager.Exists("DebugLog").Debug(msg);
        }
    }
}
