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
using System.Configuration;
using log4net;


namespace CTBC.FrameWork.HTG
{
    
    public class ExecuteHTG
    {

        string HTGUrl;
        string HTGApplication;
        string userId;
        string passWord;
        string branchNo;
        string racfId;
        string racfPassWord;
        public string initErrorMessage="";
        static bool sendHTG = true;
        static object _lockLog = new object();

        ILog log = LogManager.GetLogger("loginfo");
        //log4net.ILog log = log4net.LogManager.GetLogger("loginfo");

        HostMsgGrpBIZ hostbiz = new HostMsgGrpBIZ();
        
        public ExecuteHTG(string ldapUser=null,string ldapPassword=null, string racfUser=null,string racfPassword=null, string branchno=null)
        {
            log4net.Config.XmlConfigurator.Configure();
            //log.Info(message);

            HTGUrl = ConfigurationManager.AppSettings["HTGUrl"].ToString();
            HTGApplication = ConfigurationManager.AppSettings["HTGApplication"].ToString();

            if( ! string.IsNullOrEmpty(ldapUser) && ! string.IsNullOrEmpty(ldapPassword) && ! string.IsNullOrEmpty(racfUser) && ! string.IsNullOrEmpty(racfPassword) && ! string.IsNullOrEmpty(branchno))
            {
                userId = ldapUser.Trim();
                passWord = ldapPassword.Trim();
                branchNo = branchno.Trim();
                racfId = racfUser.Trim();
                racfPassWord = racfPassword.Trim();
            }
            else // 若沒有參數, 則, 去PARMCode 找eTabsQueryStaffBranchNo
            {
                string brancno = "SELECT TOP 1 CodeDesc FROM PARMCode WHERE CodeType='eTabsQueryStaffBranchNo' Order by SortOrder ";
                DataTable gBranchNo = hostbiz.getDataTabe(brancno);
                string _branchNo = gBranchNo.Rows[0][0].ToString();

                string sql = "SELECT * FROM PARMCode WHERE CodeType='eTabsQueryStaff' and Enable='1' Order by SortOrder ";
                //string formsql = string.Format(sql, _trn);

                DataTable gOrder = hostbiz.getDataTabe(sql);
                foreach (DataRow g in gOrder.Rows)
                {
                    var base64EncodedBytes = System.Convert.FromBase64String(g["CodeMemo"].ToString());
                    string[] up = System.Text.Encoding.UTF8.GetString(base64EncodedBytes).Split(',');
                    if (up.Length == 4) // 一定要順序為
                    {
                        userId = up[0].ToString();
                        passWord = up[1].ToString();
                        branchNo = _branchNo;
                        racfId = up[2].ToString();
                        racfPassWord = up[3].ToString();
                        break;
                    }
                }

            }


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


        /// <summary>
        /// 實際程序入口
        /// </summary>
        /// <param name="EsbNo">電文ID</param>
        /// <param name="ObligorID">義務人統編</param>
        /// <returns></returns>
        public string HTGMainFun(string HTGNo, string ObligorID, string CustomID, string CaseID, string Acc_No = null, Hashtable htparm=null)
        {
            Thread.Sleep(100);
            string strResult = "";
            try
            {
                log4net.Config.XmlConfigurator.Configure(new System.IO.FileInfo(AppDomain.CurrentDomain.BaseDirectory + @"\Log.config"));

                //TellerNo = hostbiz.GetTellerNo();
                //BranchID = hostbiz.GetBranchId();

                string strYear = Convert.ToInt16(DateTime.Now.Year - 1911).ToString("000");
                string strMonth = Convert.ToInt16(DateTime.Now.Month).ToString("00");
                string strDay = Convert.ToInt16(DateTime.Now.Day).ToString("00");
                string strHour = System.DateTime.Now.ToString("HHmmssfff");
                string strSno = "CSFS" + strYear.Substring(1, 2) + strMonth + strDay + strHour;
                string ccy="TWD";
                if( htparm!=null)
                {
                    if (htparm["CURRENCY"] != null)
                        ccy = htparm["CURRENCY"].ToString();
                }   
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
                    case "60600":
                        return P4_60600U(ObligorID,  CaseID, strSno);
                        break;
                    case "00450":
                        return P4_00450U(ObligorID, CaseID, htparm, strSno);
                        break;
                    case "67050":
                        return P4_67050U(ObligorID, CaseID, strSno);
                        break;
                    case "67050_8":
                        return P4_67050_8U(ObligorID, CaseID,htparm, strSno);
                        break;
                    case "09091":
                        return P4_9091U(ObligorID, CaseID, htparm, strSno);
                        break;
                    case "09092":
                        return P4_9092U(ObligorID, CaseID, htparm, strSno);
                        break;
                    case "09093":
                        return P4_9093U(ObligorID, CaseID, htparm, strSno);
                        break;
                    case "09095":
                        return P4_9095U(ObligorID, CaseID, htparm, strSno);
                        break;
                    case "09099":
                        return P4_9099U(ObligorID, CaseID, htparm, strSno);
                        break;
                    case "09099R":
                        return P4_9099R(ObligorID, CaseID, htparm, strSno);
                        break;
                    case "33401":
                        return P4_00401U(ObligorID, CaseID, Acc_No, ccy, strSno);
                        break;
                    case "00417":
                        return P4_00417U(ObligorID, CaseID, Acc_No, strSno);
                        break;
                    //20160122 RC --> 20150106 宏祥 add 新增67100電文
                    case "67100":
                        return P4_67100U(ObligorID, CaseID, strSno);
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



        private bool  P4_Initialize()
        {
            HTGObject obj = new HTGObject(HTGUrl, HTGApplication, userId, passWord, racfId, racfPassWord, branchNo);

            Hashtable htparm = new Hashtable();
            bool result = obj.QueryHtg("00000", htparm);
            if (!result)
            {
                initErrorMessage = obj.ReturnCode["HGExceptionMessage"].ToString();
            }
            else
                initErrorMessage = "";
            return result;

        }




        /// <summary>
        /// 發查9091, 需要分二次, 第一次.. 不要塞 FLAG1, FLAG2, 第二次, 才要FLAG1=2, FLAG2=2
        /// </summary>
        /// <param name="Option"></param>
        /// <param name="Account"></param>
        /// <param name="Ccy"></param>
        /// <param name="Code"></param>
        /// <param name="Memo"></param>
        /// <param name="ObligorNo"></param>
        /// <param name="caseid"></param>
        /// <param name="DTSRC_Date">被通報日期, 預設當天, 不用傳參數.  格式: ddMMyyyy</param>
        /// <returns></returns>
        public string Send9091(string Account, string Ccy, string Code, string Memo, string ObligorNo, string caseid, string DTSRC_Date = null)
        {

            string result = "0000|";
            if (string.IsNullOrEmpty(DTSRC_Date))
                DTSRC_Date = DateTime.Now.ToString("ddMMyyyy");
            Hashtable htparm = new Hashtable();
            //ExecuteHTG objHTG = new ExecuteHTG();
            // 先發9091
            htparm.Add("ACCT_CUR_CODE", Ccy);
            htparm.Add("STOP_RESN_CODE", Code);
            htparm.Add("ACCT_NO", Account);
            htparm.Add("DTSRC_DATE", DTSRC_Date);
            htparm.Add("STOP_RESN_DESC", Memo);

            string ret = "0000|";
            if (sendHTG)
                ret = HTGMainFun("09091", ObligorNo, "", caseid, "", htparm: htparm);
            else
                ret = "0000|"; // 測試用.. 不用每次DEBUG都要發電文

           
                return ret;
            // 20180706, 開會決定, 扣押完後, 不需要再去打450-31確認
            //System.Threading.Thread.Sleep(2000);
            //// 還要再發查一次450-30, 看是不是有扣押到
            //ret = "0000|";
            //if (sendHTG)
            //    ret = objHTG.HTGMainFun("00450", ObligorNo, "", caseid, Account);
            //else
            //    ret = "0000|"; // 測試用.. 不用每次DEBUG都要發電文

            //if (!ret.StartsWith("0000"))
            //    return ret;
            
            //if( ret.StartsWith("0000"))
            //{
            //    HostMsgGrpBIZ hostbiz1 = new HostMsgGrpBIZ();
            //    string _trn = ret.Replace("0000|", "");

            //    string sql = "SELECT * FROM TX_00450 WHERE TRNNUM='{0}' AND WXOPTION='30' AND DATA1 LIKE '% 9091 %' AND ACCOUNT = '{1}' AND DATA2 LIKE '% 04 '";
            //    string formsql = string.Format(sql, _trn, Account );

            //    DataTable gCase = hostbiz1.getDataTabe(formsql);

            //    if (gCase.Rows.Count > 0)
            //        result = "0000|9091扣押成功";
            //    else
            //        result = "0001|9091扣押失敗(原因450-30)找不到該筆";
            //    //var ds450 = ctx.TX_00450.Where(x => x.Account == Account && x.DATA1.Contains(" 9091 ") && x.DATA2.Contains(" 04 ")).FirstOrDefault();
            //    //if (ds450 == null)
            //    //    result = "0001|";
            //}
            //return result;
        }

        public  string Send9092(string Account, string Ccy, string Code, string Memo, string ObligorNo, string caseid)
        {

            //string result = "0000|";
            Hashtable htparm = new Hashtable();
            //ExecuteHTG objHTG = new ExecuteHTG();
            // 先發9092
            htparm.Add("ACCT-CUR-CODE", Ccy);
            htparm.Add("STOP_RESN_CODE", Code);
            htparm.Add("ACCT_NO", Account);
            htparm.Add("STOP_RESN_DESC", Memo);

            string ret = "0000|";
            if (sendHTG)
                ret = HTGMainFun("09092", ObligorNo, "", caseid, "", htparm: htparm);
            else
                ret = "0000|"; // 測試用.. 不用每次DEBUG都要發電文
            
            return ret;
            // 20180706, 開會決定, 扣押完後, 不需要再去打450-31確認
        }


        public string SettingSeizure9093(ObligorAccount acc, decimal SeiAmt, string govNo)
        {
            string Resut = "0000|9093扣押成功";
            if (SeiAmt <= 0)
                return "0002|無扣押金額";
            #region  開始扣押
            string boolSeizure = "";
            if (acc.Twd > 0)
            {
                if (acc.AccountType.StartsWith("定存"))
                {
                    if (string.IsNullOrEmpty(acc.LinkAccount)) // 無連結 , 發查9091-1, 回9091
                    {
                        
                        boolSeizure = Send9091(acc.Account, acc.Ccy, "4", govNo,  "Y", acc.Id, acc.CaseId);
                    }
                    else // 有連結, 發查9093-1, 回9093
                    {
                        //boolSeizure = "0000|";
                        boolSeizure = Send9093(acc.LinkAccount, SeiAmt, acc.Ccy, "66", govNo, acc.Id, acc.CaseId);
                    }
                }
                else // 其他類, 發查9093, 回9093
                {
                    //boolSeizure = "0000|";
                    boolSeizure = Send9093(acc.Account, SeiAmt, acc.Ccy, "66", govNo, acc.Id, acc.CaseId);
                }
            }


            #endregion
            Resut = boolSeizure;
            return Resut;
        }

        public string CancelSeizure9093(ObligorAccount acc, decimal SeiAmt, string govNo)
        {
            string Resut = "";

            #region  開始扣押
            Resut = Send9095(acc.Account, SeiAmt, acc.Ccy, "66", govNo, acc.Id, acc.CaseId);

            #endregion
            
            return Resut;
        }


        public string SettingSeizure9091(ObligorAccount acc,  string govNo)
        {
            string Resut = "";

            #region  開始扣押
            string boolSeizure = "";
            //if (acc.Twd > 0)
            {
                boolSeizure = Send9091("0", acc.Account, acc.Ccy, "4", govNo, acc.Id, acc.CaseId);
                                         
            }


            #endregion
            Resut = boolSeizure;
            return Resut;

        }


        public string CancelSeizure9091(ObligorAccount acc, decimal SeiAmt, string govNo)
        {
            string Resut = "";

            #region  開始扣押
            string boolSeizure = "";
            if (acc.Twd > 0)
            {
                boolSeizure = Send9091("1", acc.Account, acc.Ccy, "66", govNo, acc.Id, acc.CaseId);
            }


            #endregion
            Resut = boolSeizure;
            return Resut;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Account">帳戶</param>
        /// <param name="SeizureAmount">扣押金額</param>
        /// <param name="Ccy">幣別</param>
        /// <param name="Code">代碼</param>
        /// <param name="Memo">來文字號</param>
        /// <param name="ObligorNo">義務人ID</param>
        /// <param name="caseid">案件編號</param>
        /// <param name="Option">0表示 設定扣押 , 1表示 沖正沖正 </param>
        /// <param name="DTSRC_Date">解扣日(預設當日)</param>
        /// <returns></returns>
        public string Send9091Or9093(string Account, decimal SeizureAmount, string Ccy, string Code, string Memo, string ObligorNo, string caseid, string Option="0", string DTSRC_Date="")
        {
            string lastAccount12="";
            if (Account.Length >= 12)
                lastAccount12 = Account.Substring(Account.Length - 12);
            else
                return "0001|帳戶長度不對";
            
            
            string boolSeizure = "";

            if( Option.Equals("1")) // 支付要用的電文
            {
                if (lastAccount12.StartsWith("006")) //表示定存
                {
                    // 不可能存在, USER會在Etabs打
                }
                else
                {
                    // 先查450-31, 是否有該筆扣押金額, 若有, 才能支付
                    bool is45031hasSeizure=false;
                    string s45031 = Send45031(ObligorNo, caseid, Account, Ccy);
                    if( s45031.StartsWith("0000|"))
                    {                       
                        // 要檢核, 是否在今日, 在DATA1 中, 有 9093 及 金額 (##########.##)
                        string strAmount2 = " " + SeizureAmount.ToString("#,#.00");
                        string _trn = s45031.Replace("0000|", "");
                        string sql = "SELECT * FROM TX_00450 WHERE TRNNUM='{0}' AND WXOPTION='31' AND DATA1 LIKE '% 9093 %' AND ACCOUNT = '{1}' AND DATA1 LIKE '%{2}'";
                        string formsql = string.Format(sql, _trn, Account, strAmount2);
                        DataTable gCase = hostbiz.getDataTabe(formsql);
                        if (gCase == null)
                            return "0003|此筆帳戶目前沒有扣押金額或金額不對";
                        is45031hasSeizure=true;
                    }
                    else
                    {
                        return  "0002|450-31發查失敗";
                    }
                    // 有連結, 打9093
                    if(is45031hasSeizure)
                        boolSeizure = Send9093(Account, SeizureAmount, Ccy, "66", Memo, ObligorNo, caseid, "1", DTSRC_Date);
                }
            }


            if (Option.Equals("0"))
            {
                if (lastAccount12.StartsWith("006")) //表示定存
                {
                    // 發查417
                    var result417 = Send417(ObligorNo, caseid, Account);
                    if (result417.StartsWith("0000|定存"))
                    {
                        // 沒有連結, 故打9091                        
                        boolSeizure = Send9091(Account, Ccy, "4", Memo,  ObligorNo, caseid, DTSRC_Date);
                        
                    }

                    
                    if (result417.StartsWith("0000|綜定"))
                    {
                        // 有連結, 打9093
                        // 20180613, 若畫面是連結帳戶, 回0001|本帳號為綜定，有連結帳戶
                        //boolSeizure = Send9093(Account, SeizureAmount, Ccy, "66", Memo, ObligorNo, caseid);
                        boolSeizure = "0001|本帳號為綜定，有連結帳戶";
                    }
                }
                else
                {
                    // 有連結, 打9093
                    //boolSeizure = Send9093(Account, SeizureAmount, Ccy, "66", Memo, ObligorNo, caseid);
                    boolSeizure = Send9093(Account, SeizureAmount, Ccy, "66", Memo, ObligorNo, caseid,"0",DTSRC_Date);
                }
            }
            else if ( Option.Equals("2")) // 表示沖正
            {
                if (lastAccount12.StartsWith("006")) //表示定存
                {
                    // 發查417
                    var result417 = Send417(ObligorNo, caseid, Account);
                    if (result417.StartsWith("0000|定存"))
                    {
                        // 沒有連結, 故打9091                        
                        boolSeizure = Send9092(Account, Ccy, "4", Memo, ObligorNo, caseid);
                    }
                    if (result417.StartsWith("0000|綜定"))
                    {
                        // 有連結, 打9093
                        // 20180613, 若畫面是連結帳戶, 回0001|本帳號為綜定，有連結帳戶
                        boolSeizure = "0001|本帳號為綜定，有連結帳戶";
                        //boolSeizure = Send9095(Account, SeizureAmount, Ccy, "66", Memo, ObligorNo, caseid);
                    }
                }
                else
                {
                    // 有連結, 打9093
                    boolSeizure = Send9095(Account, SeizureAmount, Ccy, "66", Memo, ObligorNo, caseid);
                }
            }

            return boolSeizure;
        }

        public string Send9092Or9095(string Account, decimal SeizureAmount, string Ccy, string Code, string Memo, string ObligorNo, string caseid, string Option="0", string DTSRC_Date="")
        {
            string lastAccount12 = "";
            if (Account.Length >= 12)
                lastAccount12 = Account.Substring(Account.Length - 12);
            else
                return "0001|帳戶長度不對";
            string boolSeizure = "";

            if (lastAccount12.StartsWith("006")) //表示定存
            {
                boolSeizure = Send9092(Account, Ccy, "4", Memo, ObligorNo, caseid);
            }
            else
            {
                boolSeizure = Send9095(Account, SeizureAmount, Ccy, "66", Memo, ObligorNo, caseid, DTSRC_Date);
            }
            return boolSeizure;
        }


        public string Send9095(string Account, decimal SeizureAmount, string Ccy, string Code, string Memo, string ObligorNo, string caseid, string DTSRC_Date="")
        {
            string result = "";
            // 先發9095
            //ExecuteHTG objHTG = new ExecuteHTG();

            Hashtable htparm = new Hashtable();
            //            上送主機時小數補為3位
            //"12345678901234560-"
            //"12345678901234560+"
            string strAmount = "0+";
            if (SeizureAmount > 0)
                strAmount = (Convert.ToInt32(SeizureAmount * 1000)).ToString() + "+";
            else
                strAmount = (Convert.ToInt32(SeizureAmount * 1000)).ToString() + "-";

            htparm.Add("ACCT_NO", Account);
            htparm.Add("AMOUNT", strAmount);
            htparm.Add("HOLD_RESN_DESC", Memo);
            htparm.Add("PRMO_NO", "");
            htparm.Add("LOAN_ACCT_NO", "0000000000000000");
            htparm.Add("STOP_REASON_NO", Code);
            htparm.Add("CURRENCY", Ccy);
            htparm.Add("ACCT_CUR_CODE", Ccy);
            htparm.Add("NO_RECEIPT", "");
            htparm.Add("PRESET_REMOVAL_DT", DTSRC_Date);

            string ret = "0000|";
            if (sendHTG)
                ret = HTGMainFun("09095", ObligorNo, "", caseid, "", htparm: htparm);
            else
                ret = "0000|此帳戶解扣押"; // 測試用.. 不用每次DEBUG都要發電文

           
                return ret;

            // 20180706, 開會決定, 扣押完後, 不需要再去打450-31確認
            //System.Threading.Thread.Sleep(2000);
            //// 還要再發查一次450-31, 看是不是有扣押到
            //Hashtable htparm1 = new Hashtable();
            //htparm1.Add("ACCT_NO", Account);
            //htparm1.Add("LIST_TX_FR_NO", "1");
            //htparm1.Add("LIST_TX_DT_FR", DateTime.Now.ToString("ddMMyyyy"));
            //htparm1.Add("LIST_TX_DT_TO", DateTime.Now.ToString("ddMMyyyy"));
            //htparm1.Add("TX_TYPE", "31");
            //htparm1.Add("OPTION", "00"); // 要查全部, 所以要00

            //ret = objHTG.HTGMainFun("00450", ObligorNo, "", caseid, htparm: htparm1);

            //if (ret.StartsWith("0000"))
            //{
                
            //    {
            //        // 要檢核, 是否在今日, 在DATA1 中, 有 9095 及 金額 (##########.##)
            //        string strAmount2 = " " + SeizureAmount.ToString("#,#.00");
            //        string _trn = ret.Replace("0000|", "");

            //        string sql = "SELECT * FROM TX_00450 WHERE TRNNUM='{0}' AND WXOPTION='31' AND DATA1 LIKE '% 9095 %' AND ACCOUNT = '{1}' AND DATA2 LIKE '%解扣押%'";
            //        string formsql = string.Format(sql, _trn, Account, strAmount2);

            //        DataTable gCase = hostbiz.getDataTabe(formsql);
            //        //var gCase = ctx.TX_00450.Where(x => x.CaseId == gCaseid && x.WXOption == "31" && x.Account == Account && x.DATA1.Contains(" 9095 ") && x.DATA2.Contains("解扣押 ")).ToList();
            //        //var ds450 = gCase.Where(x => x.DATA1.Contains(strAmount2)).LastOrDefault();
            //        if (gCase == null)
            //            result = "0001|扣押失敗(原因450-31查不到該筆)";
            //        else
            //        {
            //            if (gCase.Rows.Count > 0)
            //                result = "0000|扣押成功";
            //            else
            //                result = "0001|扣押失敗(原因450-31查不到該筆)";
            //        }
            //    }
            //}
            //else
            //{
            //    result = "0002|450-31發查失敗";
            //}
            //return result;
        }

        private string Send9093(string Account, decimal SeizureAmount, string Ccy, string Code, string Memo, string ObligorNo, string caseid, string Function="0",string DTSRC_Date="")
        {

            string result = "";
            // 先發9093-1
            //ExecuteHTG objHTG = new ExecuteHTG();

            Hashtable htparm = new Hashtable();

            //上送主機時小數補為3位
            //"12345678901234560-"
            //"12345678901234560+"

            // 2018 /7/ 16 若是台幣, 日幣, 不可以有   分角....無條件進位
            List<string> Cur = new List<string>() { "TWD", "JPY", "DEN", "FRF", "HUF", "IDR", "ITL", "KRW" };

            if(  Cur.Contains(Ccy) )
            {
                SeizureAmount = Math.Ceiling(SeizureAmount);
            }

            string strAmount = "0+";
            if (SeizureAmount > 0)
                strAmount = (Convert.ToInt32(SeizureAmount * 1000)).ToString() + "+";
            else
                strAmount = (Convert.ToInt32(SeizureAmount * 1000)).ToString() + "-";

            htparm.Add("ACCT_NO", Account);
            htparm.Add("AMOUNT", strAmount);
            htparm.Add("HOLD_RESN_DESC", Memo);
            htparm.Add("PRMO_NO", "");
            htparm.Add("LOAN_ACCT_NO", "0000000000000000");
            htparm.Add("STOP_REASON_NO", Code);
            htparm.Add("CURRENCY", Ccy);
            htparm.Add("NO_RECEIPT", "");
            htparm.Add("PRESET_REMOVAL_DT", DTSRC_Date);
            htparm.Add("FUNCTION", Function);

            string ret = "0000|";


            if (sendHTG)
                ret = HTGMainFun("09093", ObligorNo, "", caseid, "", htparm: htparm);
            else
                ret = "0000|此帳戶扣押了"; // 測試用.. 不用每次DEBUG都要發電文

            
            return ret;

            // 20180706, 開會決定, 扣押完後, 不需要再去打450-31確認
            //System.Threading.Thread.Sleep(2000);
            //// 還要再發查一次450-31, 看是不是有扣押到
            //Hashtable htparm1 = new Hashtable();
            //htparm1.Add("ACCT_NO", Account);
            //htparm1.Add("LIST_TX_FR_NO", "1");
            //htparm1.Add("LIST_TX_DT_FR", DateTime.Now.ToString("ddMMyyyy"));
            //htparm1.Add("LIST_TX_DT_TO", DateTime.Now.ToString("ddMMyyyy"));
            //htparm1.Add("TX_TYPE", "31");
            //htparm1.Add("OPTION", "01");

            //ret = objHTG.HTGMainFun("00450", ObligorNo, "", caseid, htparm: htparm1);

            //if( ret.StartsWith("0000"))
            //{
                
            //    {
            //        // 要檢核, 是否在今日, 在DATA1 中, 有 9093 及 金額 (##########.##)
            //        string strAmount2 = " " + SeizureAmount.ToString("#,#.00");
            //        string _trn = ret.Replace("0000|","");
            //        //Guid gCaseid = Guid.Parse(caseid);
                    
            //        string sql = "SELECT * FROM TX_00450 WHERE TRNNUM='{0}' AND WXOPTION='31' AND DATA1 LIKE '% 9093 %' AND ACCOUNT = '{1}' AND DATA1 LIKE '%{2}'";
            //        string formsql = string.Format(sql, _trn, Account, strAmount2);

            //        DataTable gCase = hostbiz.getDataTabe(formsql);
                    
            //        //var gCase = ctx.TX_00450.Where(x => x.TrnNum == _trn && x.WXOption == "31" && x.DATA1.Contains(" 9093 ") && x.Account == Account && x.DATA1.Contains(strAmount2)).LastOrDefault();
            //        if (gCase == null)
            //            result = "0001|扣押失敗(原因450-31查不到該筆)";
            //        else
            //        {
            //            if (gCase.Rows.Count > 0)
            //                result = "0000|扣押成功";
            //            else
            //                result = "0001|扣押不成功(原因450-31查不到該筆)";
            //        }
            //    }
            //}
            //else
            //{
            //    result = "0002|450-31發查失敗";
            //}
            //return result;
        }


        public string Send9099(string Account, decimal SeizureAmount, string Ccy, string Code, string Memo, string ObligorNo, string caseid, string DTSRC_Date = "")
        {

            string result = "";
            // 先發9099
            //ExecuteHTG objHTG = new ExecuteHTG();

            Hashtable htparm = new Hashtable();

            //上送主機時小數補為3位
            //"12345678901234560-"
            //"12345678901234560+"

            // 2018 /7/ 16 若是台幣, 日幣, 不可以有   分角....無條件進位
            List<string> Cur = new List<string>() { "TWD", "JPY", "DEN", "FRF", "HUF", "IDR", "ITL", "KRW" };

            if (Cur.Contains(Ccy))
            {
                SeizureAmount = Math.Ceiling(SeizureAmount);
            }

            string strAmount = "0+";
            if (SeizureAmount > 0)
                strAmount = (Convert.ToInt32(SeizureAmount * 1000)).ToString() + "+";
            else
                strAmount = (Convert.ToInt32(SeizureAmount * 1000)).ToString() + "-";


            // 20180903 打9099, 必須先打9093, Function=1, 然後拿回來後, 再打9099
            string Function = "1";

            htparm.Add("ACCT_NO", Account);
            htparm.Add("AMOUNT", strAmount);
            htparm.Add("HOLD_RESN_DESC", Memo);
            htparm.Add("PRMO_NO", "");
            htparm.Add("LOAN_ACCT_NO", "0000000000000000");
            htparm.Add("STOP_REASON_NO", Code);
            htparm.Add("CURRENCY", Ccy);
            htparm.Add("NO_RECEIPT", "");
            htparm.Add("PRESET_REMOVAL_DT", DTSRC_Date);
            htparm.Add("FUNCTION", Function);

            string ret = "0000|";


            if (sendHTG)
                ret = HTGMainFun("09099", ObligorNo, "", caseid, "", htparm: htparm);
            else
                ret = "0000|此帳戶扣押了"; // 測試用.. 不用每次DEBUG都要發電文


            return ret;

        }


        public string Send9099Reset(string Account, decimal SeizureAmount, string Ccy, string Code, string Memo, string ObligorNo, string caseid, string DTSRC_Date = "")
        {

            string result = "";
            // 先發9099
            //ExecuteHTG objHTG = new ExecuteHTG();

            Hashtable htparm = new Hashtable();

            //上送主機時小數補為3位
            //"12345678901234560-"
            //"12345678901234560+"

            // 2018 /7/ 16 若是台幣, 日幣, 不可以有   分角....無條件進位
            List<string> Cur = new List<string>() { "TWD", "JPY", "DEN", "FRF", "HUF", "IDR", "ITL", "KRW" };

            if (Cur.Contains(Ccy))
            {
                SeizureAmount = Math.Ceiling(SeizureAmount);
            }

            string strAmount = "0+";
            if (SeizureAmount > 0)
                strAmount = (Convert.ToInt32(SeizureAmount * 1000)).ToString() + "+";
            else
                strAmount = (Convert.ToInt32(SeizureAmount * 1000)).ToString() + "-";


            // 20180903 打9099, 必須先打9093, Function=1, 然後拿回來後, 再打9099
            string Function = "1";

            htparm.Add("ACCT_NO", Account);
            htparm.Add("AMOUNT", strAmount);
            htparm.Add("HOLD_RESN_DESC", Memo);
            htparm.Add("PRMO_NO", "");
            htparm.Add("LOAN_ACCT_NO", "0000000000000000");
            htparm.Add("STOP_REASON_NO", Code);
            htparm.Add("CURRENCY", Ccy);
            htparm.Add("NO_RECEIPT", "");
            htparm.Add("PRESET_REMOVAL_DT", DTSRC_Date);
            htparm.Add("FUNCTION", Function);

            string ret = "0000|";


            if (sendHTG)
                ret = HTGMainFun("09099R", ObligorNo, "", caseid, "", htparm: htparm);
            else
                ret = "0000|此帳戶扣押了"; // 測試用.. 不用每次DEBUG都要發電文


            return ret;

        }
        

        public Dictionary<string, decimal> Send87106()
        {
            Dictionary<string, decimal> rate = new Dictionary<string, decimal>();

            HTGObject obj = new HTGObject(HTGUrl, HTGApplication, userId, passWord, racfId, racfPassWord, branchNo);

            Hashtable htparm = new Hashtable();
           
            bool result = obj.QueryHtg("87016", htparm);
            //執行過程的log
            string log = obj.MessageLog;

            if (result) //此為程式發生Exception或電文發送回應錯誤
            {

                
                //電文資料集
                DataSet returnDS = obj.HtgDataSet;



                var masterCount = returnDS.Tables["TX_87016"].Rows.Count;
                foreach(DataRow dr in returnDS.Tables["TX_87016"].Rows)
                {
                    rate.Add(dr["CCY"].ToString(), decimal.Parse( dr["Buy"].ToString()));
                }

            }

            WriteLog(log);
            return rate;
        }

        public string Send417(string ObligorNo, string caseid, string Acc_No)
        {
            ExecuteHTG objHTG = new ExecuteHTG();
            // ----------------------------------------DEBUG用----------------------------
            //  要帶頁數.. <data id="LIST_TX_FR_NO" value="1" /> , 直到看到<data id="DATA" value="END OF TXN"/>
            // 目前程式還沒有自動帶
            string ret = objHTG.HTGMainFun("00417", ObligorNo, "", caseid.ToString(), Acc_No);
            //string ret = "0000|"; // 測試用.. 不用每次DEBUG都要發電文
            // ----------------------------------------DEBUG用----------------------------

            if (ret.StartsWith("0000"))
            {
                
                {
                    string _trn = ret.Split('|')[1];
                    string sql = "SELECT TOP 1* FROM TX_00417 WHERE TRNNUM='{0}' ";
                    string formsql = string.Format(sql, _trn);

                    DataTable gCase = hostbiz.getDataTabe(formsql);
                    if (gCase == null)
                        return  "0001|發查失敗(原因417查不到該筆)";
                    else
                    {

                        if (gCase.Rows.Count > 0)
                        {
                            string res = gCase.Rows[0]["LinkAccount"].ToString();
                            if (string.IsNullOrEmpty(res))
                                return "0000|定存";
                            else
                                return "0000|綜定" + "|" + res;
                        }
                        else
                        {
                            return "00001|發查417, 但無資料";
                        }
                    }
                }
            }
            else
            {
                return "0001| 發電文00417失敗";
            }
        }

        public string Send401(string ObligorNo, string caseid, string Acc_No,string ccy)
        {
            Hashtable ht = new Hashtable();
            ht.Add("CURRENCY", ccy);
            return HTGMainFun("33401", ObligorNo, "", caseid, Acc_No: Acc_No,htparm:ht);
            //return P4_00401U(ObligorNo, caseid, Acc_No);
        }

        public Dictionary<string, string> Send60629(string ObligorNo, Guid caseid, ref string RetMessage)
        {
            ExecuteHTG objHTG = new ExecuteHTG();
            string result = objHTG.HTGMainFun("60629", ObligorNo, "", caseid.ToString());

            
            Dictionary<string, string> Res = new Dictionary<string, string>();
            // 再去DB把重號的部分查出來.
            if (result.StartsWith("0000"))
            {
                string trnnum = result.Split('|')[1].Trim();
               
                {
                    //string strAmount2 = " " + SeizureAmount.ToString("#,#.00");
                    string _trn = result.Replace("0000|", "");

                    string sql = "SELECT TOP 1 * FROM TX_60629 WHERE TRNNUM='{0}'  ORDER BY SNO DESC";
                    string formsql = string.Format(sql, _trn);

                    DataTable gCase = hostbiz.getDataTabe(formsql);
                    //var nums = ctx.TX_60629.Where(x => x.TrnNum == trnnum).OrderByDescending(x => x.SNO).FirstOrDefault();
                    if (gCase == null)
                        result = "0001|扣押失敗(原因450-31查不到該筆)";
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

                }


            }

            bool isLength18 = Res.Any(x => x.Value.Length > 18);

            if (isLength18)
                RetMessage = "0003|戶名長度超過18個字，落人工";
            else
                RetMessage = result;
            return Res;
        }


        public string Send67102(string ObligorNo, Guid caseid, ref string RetMessage)
        {
            ExecuteHTG objHTG = new ExecuteHTG();
            string ret = "";      

            string result = objHTG.HTGMainFun("67050", ObligorNo, "", caseid.ToString());



            if (result.StartsWith("0000"))
            {
                string trnnum = result.Split('|')[1].Trim();

                {
                    //string strAmount2 = " " + SeizureAmount.ToString("#,#.00");
                    string _trn = result.Replace("0000|", "");

                    string sql = "SELECT TOP 1 * FROM TX_67102 WHERE TRNNUM='{0}'  ORDER BY SNO DESC";
                    string formsql = string.Format(sql, _trn);

                    DataTable gCase = hostbiz.getDataTabe(formsql);
                    //var nums = ctx.TX_60629.Where(x => x.TrnNum == trnnum).OrderByDescending(x => x.SNO).FirstOrDefault();
                    if (gCase == null)
                        result = "0001|67050失敗(原因67050-6查不到該筆)";
                    else
                    {
                        if (gCase.Rows.Count > 0)
                        {
                            var name =  gCase.Rows[0]["RESP_NAME"].ToString().Trim();
                            var id = gCase.Rows[0]["RESP_CUST_ID"].ToString().Trim();
                            ret = id + "@" + name;
                        }
                    }

                }
                RetMessage = "0000|";
            }
            else
            {
                RetMessage = "0001|67050-6失敗";
            }

            return ret;
        }





        public List<ObligorAccount> Send60491_Co(string ObligorNo, Guid caseid, Dictionary<string, string> BranchInfo, ref string retMessage, bool isRename, bool isSame, string newName, bool isDoubleAcc, string coType = "P", bool bNoSeizure=false)
        {


            ExecuteHTG objHTG = new ExecuteHTG();
            string ret = "0000|";

            if (sendHTG)
                ret = objHTG.HTGMainFun("60491", ObligorNo, "", caseid.ToString());
            else
                ret = "0000|"; // 測試用.. 不用每次DEBUG都要發電文
            // ----------------------------------------DEBUG用----------------------------

            retMessage = ret;

            List<ObligorAccount> Result = new List<ObligorAccount>();
            if (ret.StartsWith("0000"))
            {
                
                {
                    string _name = null;
                    //var master1 = ctx.TX_60491_Grp.Where(x => x.CaseId == caseid);
                    //if (master1 != null)
                    //    _name = master1.First().CustomerName; 
                    
                    // 要排除 ("關係別") TX_60491_Detl.Link = 'JOIN'
                    // 要排除 TX_60491_Detl.StsDesc = '結清' or '放款' or '現金卡'



                    string _trn = ret.Replace("0000|", "");


                    string sql = "SELECT * FROM TX_60491_Detl a inner join TX_60491_Grp b on a.fksno=b.sno WHERE b.TRNNUM='{0}'  ";
                    string formsql = string.Format(sql, _trn);
                    DataTable gCase = hostbiz.getDataTabe(formsql);
                    if (gCase != null)
                    {
                        foreach (DataRow dr in gCase.Rows)
                        {
                            ObligorAccount n = new ObligorAccount()
                            {
                                Account = dr["Account"].ToString(),
                                CaseId = dr["CaseId"].ToString(),
                                Ccy = dr["Ccy"].ToString(),
                                Id = dr["CUST_ID"].ToString(),
                                ProdCode = dr["ProdCode"].ToString(),
                                Name = dr["CustomerName"].ToString(),
                                BranchNo = dr["Branch"].ToString(),
                                BranchName = BranchInfo[dr["Branch"].ToString()],
                                AccountStatus = dr["StsDesc"].ToString(),
                                ProdDesc = dr["ProdDesc"].ToString(),
                                Link = dr["Link"].ToString(),
                                SegmentCode = dr["SegmentCode"].ToString(),
                                StsDesc = dr["StsDesc"].ToString().Trim(),
                                CoType = coType,
                                isSame = isSame,
                                isRename = isRename,
                                newName = newName,
                                isDoubleAcc = isDoubleAcc,
                                noSeizure = bNoSeizure
                            };
                            

                            Result.Add(n);
                        }
                    }
                    //Result = ctx.TX_60491_Detl.Where(x => x.CaseId == caseid && (x.Link.Trim().ToUpper() != "JOIN" || x.StsDesc.Trim() != "結清" || x.StsDesc.Trim() != "放款" || x.StsDesc.Trim() != "現金卡")).Select(x => new ObligorAccount
                    //{
                    //    Account = x.Account,
                    //    CaseId = x.CaseId.ToString(),
                    //    Ccy = x.Ccy,
                    //    Id = x.CUST_ID,
                    //    ProdCode = x.ProdCode,
                    //    Name = _name
                    //}).ToList();
                }
            }
            return Result;
        }


        public List<ObligorAccount> Send60491(string ObligorNo, Guid caseid, Dictionary<string, string> BranchInfo, ref string retMessage)
        {


            ExecuteHTG objHTG = new ExecuteHTG();
            string ret = "0000|";

            if (sendHTG)
                ret = objHTG.HTGMainFun("60491", ObligorNo, "", caseid.ToString());
            else
                ret = "0000|"; // 測試用.. 不用每次DEBUG都要發電文
            // ----------------------------------------DEBUG用----------------------------

            retMessage = ret;

            List<ObligorAccount> Result = new List<ObligorAccount>();
            if (ret.StartsWith("0000"))
            {

                {
                    string _name = null;
                    //var master1 = ctx.TX_60491_Grp.Where(x => x.CaseId == caseid);
                    //if (master1 != null)
                    //    _name = master1.First().CustomerName; 

                    // 要排除 ("關係別") TX_60491_Detl.Link = 'JOIN'
                    // 要排除 TX_60491_Detl.StsDesc = '結清' or '放款' or '現金卡'



                    string _trn = ret.Replace("0000|", "");


                    string sql = "SELECT * FROM TX_60491_Detl a inner join TX_60491_Grp b on a.fksno=b.sno WHERE b.TRNNUM='{0}'  ";
                    string formsql = string.Format(sql, _trn);
                    DataTable gCase = hostbiz.getDataTabe(formsql);
                    if (gCase != null)
                    {
                        foreach (DataRow dr in gCase.Rows)
                        {
                            ObligorAccount n = new ObligorAccount()
                            {
                                Account = dr["Account"].ToString(),
                                CaseId = dr["CaseId"].ToString(),
                                Ccy = dr["Ccy"].ToString(),
                                Id = dr["CUST_ID"].ToString(),
                                ProdCode = dr["ProdCode"].ToString(),
                                Name = dr["CustomerName"].ToString(),
                                BranchNo = dr["Branch"].ToString(),
                                BranchName = BranchInfo[dr["Branch"].ToString()],
                                AccountStatus = dr["StsDesc"].ToString(),
                                ProdDesc = dr["ProdDesc"].ToString(),
                                Link = dr["Link"].ToString(),
                                SegmentCode = dr["SegmentCode"].ToString(),
                                StsDesc = dr["StsDesc"].ToString().Trim()
                            };

                            Result.Add(n);
                        }
                    }
                    //Result = ctx.TX_60491_Detl.Where(x => x.CaseId == caseid && (x.Link.Trim().ToUpper() != "JOIN" || x.StsDesc.Trim() != "結清" || x.StsDesc.Trim() != "放款" || x.StsDesc.Trim() != "現金卡")).Select(x => new ObligorAccount
                    //{
                    //    Account = x.Account,
                    //    CaseId = x.CaseId.ToString(),
                    //    Ccy = x.Ccy,
                    //    Id = x.CUST_ID,
                    //    ProdCode = x.ProdCode,
                    //    Name = _name
                    //}).ToList();
                }
            }
            return Result;
        }



        public List<string> Send60600(string ObligorNo, Guid caseid,  ref string retMessage)
        {
            ExecuteHTG objHTG = new ExecuteHTG();
            // ----------------------------------------DEBUG用----------------------------
            //  要帶頁數.. <data id="LIST_TX_FR_NO" value="1" /> , 直到看到<data id="DATA" value="END OF TXN"/>
            // 目前程式還沒有自動帶
            string ret = "0000|";
            if (sendHTG)
                ret = objHTG.HTGMainFun("60600", ObligorNo, "", caseid.ToString());
            else
                ret = "0000|";
            //string ret = "0000|"; // 測試用.. 不用每次DEBUG都要發電文
            // ----------------------------------------DEBUG用----------------------------
            //回傳的電文, 只要比較FIELD_NAME, FIELD_NAME_1, FIELD_NAME_2 等於"中文姓名"
            // 才需要去比對DATA, DATA_1, DATA_2
            retMessage = ret;

            List<string> Result = new List<string>();
            if (ret.StartsWith("0000"))
            {

                string _trn = ret.Replace("0000|", "");

                string sql = "SELECT DATA FROM TX_60600 WHERE TRNNUM='{0}' AND FIELD_NAME='中文姓名' ";
                string formsql = string.Format(sql, _trn);

                DataTable gCase = hostbiz.getDataTabe(formsql);

                if (gCase != null)
                {
                    foreach (DataRow dr in gCase.Rows)
                    {
                        Result.Add(dr[0].ToString());
                        //Result = ctx.TX_60600.Where(x => x.CaseId == caseid && (x.FIELD_NAME == "中文姓名")).Select(x => x.DATA).ToList();
                    }
                }
            }
            return Result;

        }

        public bool? is405Accident(string ObligorNo, string caseid, string Acc_No, string Ccy)
        {
            return  is450Accident(ObligorNo,  caseid,  Acc_No, Ccy);
        }



        public Dictionary<string, string> getAccident2(string ObligorNo, string caseid, string Acc_No, string Ccy, ref string retMessage)
        {

            Dictionary<string, string> newResult = new Dictionary<string, string>();
            Dictionary<string, string> mess45030 = new Dictionary<string, string>()
            {
                {"04", "帳戶因他案已凍結在案。(請查明)"},
                {"05", "帳戶因他案已設定質權在案。(請查明)"},
                {"06", "帳戶因他案已設定質權在案。(請查明)"},
                {"07", "帳戶已設為支存拒往戶。(請查明)"},
                {"08", "帳戶因本票中止契約。(請查明)"},
                {"10", "帳戶因其他原因已凍結在案。(請查明)"},
                {"11", "帳戶已設定為警示帳戶。"},
                {"12", "帳戶已設定為警示帳戶。"},
                {"13", "帳戶已設定為衍生警示帳戶。"},
                {"14", "帳戶已設定為警示帳戶。"},
            };
            var sendResult = Send45030(ObligorNo, caseid, Acc_No, Ccy);
            retMessage = sendResult;

            if (sendResult.StartsWith("0000|"))
            {
                string _trn = sendResult.Replace("0000|", "");

                string sql1 = "SELECT * FROM TX_00450 WHERE TRNNUM='{0}'  AND ACCOUNT = '{1}' AND (DATA1 LIKE '% 9091 %' OR DATA1 LIKE '% 9092 %' OR DATA1 LIKE '% 9098 %' ) ORDER BY SNO ";
                string formsql1 = string.Format(sql1, _trn, Acc_No);
                DataTable gCase1 = hostbiz.getDataTabe(formsql1);
                if (gCase1 == null) // 若都沒有這些, 直接回
                    return null;
                int i = 1;
                Dictionary<int, string> a9091 = new Dictionary<int, string>();
                Dictionary<int, string> a9092 = new Dictionary<int, string>();
                foreach (DataRow dr in gCase1.Rows)
                {
                    string accid = "";
                    if (dr["DATA2"].ToString().Length >= 42)
                    {
                        accid = dr["DATA2"].ToString().Substring(41, 2);
                        if (dr["DATA1"].ToString().Contains(" 9091 ") || dr["DATA1"].ToString().Contains(" 9098 "))
                            a9091.Add(i, accid);
                        if (dr["DATA1"].ToString().Contains(" 9092 "))
                            a9092.Add(i, accid);
                        i++;
                    }
                }


                
                foreach (var reason in mess45030)
                {
                    string accid = reason.Key.Trim();
                    if (a9092.ContainsValue(accid) && a9091.ContainsValue(accid))
                    {
                        //找出最後一筆的位置
                        int last9092 = a9092.Where(x => x.Value == accid).Last().Key;
                        int last9091 = a9091.Where(x => x.Value == accid).Last().Key;
                        if (last9092 > last9091)
                        { }
                        else
                            newResult.Add(accid, mess45030[accid]);
                    }
                    if (a9091.ContainsValue(accid) && !a9092.ContainsValue(accid))
                    {
                        newResult.Add(accid, mess45030[accid]);
                    }
                } // end for

            }
            else
            {
                // 發查失敗... 回應格式為   0001|電文00450 發查失敗0001|交易限制

                retMessage = retMessage.Replace("0001|電文00450 發查失敗", "");

            }

            return newResult;
        }

        public bool getAccident(string ObligorNo, string caseid, string Acc_No, string Ccy, ref string message450, ref string message450Code,  ref string retMessage)
        {
            bool result = false;
            Dictionary<string, string> mess45030 = new Dictionary<string, string>()
            {
                {" 04 ", "帳戶因他案已凍結在案。(請查明)"},
                {" 05 ", "帳戶因他案已設定質權在案。(請查明)"},
                {" 06 ", "帳戶因他案已設定質權在案。(請查明)"},
                {" 07 ", "帳戶已設為支存拒往戶。(請查明)"},
                {" 08 ", "帳戶因本票中止契約。(請查明)"},
                {" 10 ", "帳戶因其他原因已凍結在案。(請查明)"},
                {" 11 ", "帳戶已設定為警示帳戶。"},
                {" 12 ", "帳戶已設定為警示帳戶。"},
                {" 13 ", "帳戶已設定為衍生警示帳戶。"},
                {" 14 ", "帳戶已設定為警示帳戶。"},
            };
            var sendResult = Send45030(ObligorNo, caseid, Acc_No, Ccy);
            retMessage = sendResult;
            if (sendResult.StartsWith("0000|"))
            {
                string _trn = sendResult.Replace("0000|", "");

                string sql1 = "SELECT * FROM TX_00450 WHERE TRNNUM='{0}'  AND ACCOUNT = '{1}' AND ( DATA2 LIKE '%{2}%'  OR DATA2 LIKE '%{3}%'  OR DATA2 LIKE '%{4}%'  OR DATA2 LIKE '%{5}%'  OR DATA2 LIKE '%{6}%'  OR DATA2 LIKE '%{7}%' OR DATA2 LIKE '%{8}%' OR DATA2 LIKE '%{9}%' OR DATA2 LIKE '%{10}%' OR DATA2 LIKE '%{11}%' )";
                string formsql1 = string.Format(sql1, _trn, Acc_No, " 04 ", " 05 ", " 06 ", " 07 ", " 08 ", " 10 ", " 11 ", " 12 ", " 13 ", " 14 ");
                DataTable gCase1 = hostbiz.getDataTabe(formsql1);
                if (gCase1 == null) // 若都沒有這些, 直接回
                    return false;



                string sql = "SELECT * FROM TX_00450 WHERE TRNNUM='{0}'  AND ACCOUNT = '{1}' AND DATA2 LIKE '%{2}%' AND NOT DATA1 LIKE '%END OF TXN%'  ORDER BY SNO DESC";
                //string sql2 = "SELECT * FROM TX_00450 WHERE TRNNUM='{0}'  AND ACCOUNT = '{1}' AND DATA2 LIKE '%{2}' AND NOT DATA1 LIKE '%END OF TXN%'  ORDER BY SNO DESC";

                foreach (var reason in mess45030)
                {
                    string formsql = string.Format(sql, _trn, Acc_No, reason.Key);
                    DataTable gCase = hostbiz.getDataTabe(formsql);
                    if (gCase != null)
                    {
                        if ((gCase.Rows.Count % 2) == 1) // 若是奇數, 表示目前有事故.. 若是偶數, 表示已解事故
                        {
                            //result += mess45030[reason.Key];
                            message450Code = reason.Key.Trim();
                            message450 = mess45030[reason.Key].Trim();
                            //if (reason.Key == " 11 " || reason.Key == " 12 " || reason.Key == " 13 " || reason.Key == " 14 ")
                            //    accType = "自動回文";
                            //else
                            //    accType = "落人工";
                            result = true;
                        }
                    }
                }

            }
            return result;
        }


        public bool? is450Accident(string ObligorNo, string caseid, string Acc_No, string Ccy)
        {
            bool? result = null;
            // P20頁, 要04, .. 05,06, , 10,       11, 12,        13,      14
            //回傳的電文, 只要比較FIELD_NAME, FIELD_NAME_1, FIELD_NAME_2 等於"中文姓名"
            // 才需要去比對DATA, DATA_1, DATA_2
            // 若
            //SELECT *   FROM [CSFS_SIT].[dbo].[TX_00450] where Data1 like '% 9091 %' and data2 like '% 04 %' and Account=@Acc_No
            
            var sendResult = Send45030(ObligorNo, caseid, Acc_No, Ccy);

            if (sendResult.StartsWith("0000|"))
                return true;
            else
                return false;
            //if( sendResult.StartsWith("0000|"))
            //{
            //    //string trnnum = sendResult.Split('|')[1].ToString();

            //    //string _trn = sendResult.Replace("0000|", "");

            //    ////string sql = "SELECT TOP 1 * FROM TX_00450 WHERE TRNNUM='{0}' AND  DATA1 LIKE '% 9091 %' AND ACCOUNT = '{1}' AND ( DATA2 LIKE '%{2}'  OR DATA2 LIKE '%{3}'  OR DATA2 LIKE '%{4}'  OR DATA2 LIKE '%{5}'  OR DATA2 LIKE '%{6}'  OR DATA2 LIKE '%{7}' OR DATA2 LIKE '%{8}' OR DATA2 LIKE '%{9}' )";
                
            //    //string sql = "SELECT * FROM TX_00450 WHERE TRNNUM='{0}'  AND WXOPTION='30'  AND ACCOUNT = '{1}' AND ( DATA2 LIKE '%{2}'  OR DATA2 LIKE '%{3}'  OR DATA2 LIKE '%{4}'  OR DATA2 LIKE '%{5}'  OR DATA2 LIKE '%{6}'  OR DATA2 LIKE '%{7}' OR DATA2 LIKE '%{8}' OR DATA2 LIKE '%{9}' )";
            //    //string formsql = string.Format(sql, _trn, Acc_No, " 04", " 05", " 06", " 10", " 11", " 12", " 13", " 14");

            //    //DataTable gCase = hostbiz.getDataTabe(formsql);                
            //    //{
            //    //    //var ds450 = ctx.TX_00450.Where(x => x.TrnNum==trnnum &&  x.Account == Acc_No && x.DATA1.Contains(" 9091 ") && 
            //    //    //    (x.DATA2.Contains(" 04 ") || x.DATA2.Contains(" 05 ") || x.DATA2.Contains(" 06 ") || x.DATA2.Contains(" 10 ")  || x.DATA2.Contains(" 11 ")  || x.DATA2.Contains(" 12 ")  || x.DATA2.Contains(" 13 ")  || x.DATA2.Contains(" 14 ")  )                        
            //    //    //    ).FirstOrDefault();
            //    //    if (gCase != null)
            //    //    {
            //    //        if (gCase.Rows.Count == 0)
            //    //            result = false;
            //    //        else
            //    //            result = true;
            //    //    }
            //    //    else
            //    //        result = false;
            //    //}
            //}

            return result;
        }

        public string Send45030(string ObligorNo, string caseid, string Acc_No, string Ccy)
        {
            ExecuteHTG objHTG = new ExecuteHTG();            

            Hashtable htparm1 = new Hashtable();
            htparm1.Add("ACCT_NO", Acc_No);
            htparm1.Add("LIST_TX_FR_NO", "1");
            htparm1.Add("LIST_TX_DT_FR", "01011991");
            htparm1.Add("LIST_TX_DT_TO", DateTime.Now.ToString("ddMMyyyy"));
            htparm1.Add("TX_TYPE", "30");
            htparm1.Add("CURRENCY", Ccy);
            htparm1.Add("OPTION", "00");

            // ----------------------------------------DEBUG用----------------------------
            //  要帶頁數.. <data id="LIST_TX_FR_NO" value="1" /> , 直到看到<data id="DATA" value="END OF TXN"/>
            string ret = "0000|";
            if (sendHTG)
                ret = objHTG.HTGMainFun("00450", ObligorNo, "", caseid, htparm: htparm1);
            else
                ret = "0000|"; // 測試用.. 不用每次DEBUG都要發電文
            // ----------------------------------------DEBUG用----------------------------
            
            return ret;
        }

        public string Send45031(string ObligorNo, string caseid, string Acc_No, string Ccy)
        {
            ExecuteHTG objHTG = new ExecuteHTG();

            Hashtable htparm1 = new Hashtable();
            htparm1.Add("ACCT_NO", Acc_No);
            htparm1.Add("LIST_TX_FR_NO", "1");
            htparm1.Add("LIST_TX_DT_FR", "01011991");
            htparm1.Add("LIST_TX_DT_TO", DateTime.Now.ToString("ddMMyyyy"));
            htparm1.Add("TX_TYPE", "31");
            htparm1.Add("CURRENCY", Ccy);
            htparm1.Add("OPTION", "01");

            // ----------------------------------------DEBUG用----------------------------
            //  要帶頁數.. <data id="LIST_TX_FR_NO" value="1" /> , 直到看到<data id="DATA" value="END OF TXN"/>
            string ret = "0000|";
            if (sendHTG)
                ret = objHTG.HTGMainFun("00450", ObligorNo, "", caseid, htparm: htparm1);
            else
                ret = "0000|"; // 測試用.. 不用每次DEBUG都要發電文
            // ----------------------------------------DEBUG用----------------------------

            return ret;
        }

        public string Send67002(string ObligorNo, string caseid)
        {
            ExecuteHTG objHTG = new ExecuteHTG();

            Hashtable htparm1 = new Hashtable();
            htparm1.Add("CUST_ID_NO", ObligorNo);
            htparm1.Add("OPTN", "8");
            htparm1.Add("CUST_NO", "0000000000000000");

            // ----------------------------------------DEBUG用----------------------------
            //  要帶頁數.. <data id="LIST_TX_FR_NO" value="1" /> , 直到看到<data id="DATA" value="END OF TXN"/>
            string ret = "0000|";
            if (sendHTG)
                ret = objHTG.HTGMainFun("67050_8", ObligorNo, "", caseid, htparm: htparm1);
            else
                ret = "0000|"; // 測試用.. 不用每次DEBUG都要發電文
            // ----------------------------------------DEBUG用----------------------------

            return ret;
        }

        private string P4_9095U(string ObligorID, string CaseID, Hashtable htparm, string strSno)
        {
            HTGObject obj = new HTGObject(HTGUrl, HTGApplication, userId, passWord, racfId, racfPassWord, branchNo);

            bool result = obj.QueryHtg("09095", htparm);
            if (!result) // 執行第2次
            {
                System.Threading.Thread.Sleep(300);
                result = obj.QueryHtg("09095", htparm);
            }

            if (!result) // 執行第3次
            {
                System.Threading.Thread.Sleep(300);
                result = obj.QueryHtg("09095", htparm);
            }

            string strMessage = obj.ReturnCode["HGExceptionMessage"].ToString();
            if( ! string.IsNullOrEmpty(strMessage))
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
                return "0001|電文09095 發查失敗 " + strMessage  ;
            }
            else // 與主機交易執行結果成功,取回交易結果的資料集, 註: Result = true並不代表一定有資料傳,因為可能在所傳入的查詢條件下並無符合的資料,但交易是成功的
            {
                //執行過程的log  
                log = obj.MessageLog;
                //電文資料集
                DataSet returnDS = obj.HtgDataSet;
                #region 開始儲存至DB
                // 要把Account也存回TABLE中
                var masterCount = returnDS.Tables["TX_09095"].Rows.Count;
                foreach (DataRow dr in returnDS.Tables["TX_09095"].Rows)
                {
                    dr["Account"] = htparm["ACCT_NO"].ToString();
                }
                bool isSuccess = P4_SinglePage_SaveToDB(returnDS, CaseID, ObligorID, "09095", strSno);
                if (!isSuccess)
                {
                    return "0002|儲存09095失敗";
                }
                #endregion
                //return code
                Hashtable returnHT = obj.ReturnCode;
            }
            WriteLog(log);
            return "0000|" + strSno;
        }

        private string P4_9093U(string ObligorID, string CaseID, Hashtable htparm, string strSno)
        {
            HTGObject obj = new HTGObject(HTGUrl, HTGApplication, userId, passWord, racfId, racfPassWord, branchNo);

            bool result = obj.QueryHtg("09093", htparm);
            if (!result) // 執行第2次
            {
                System.Threading.Thread.Sleep(300);
                result = obj.QueryHtg("09093", htparm);
            }

            if (!result) // 執行第3次
            {
                System.Threading.Thread.Sleep(300);
                result = obj.QueryHtg("09093", htparm);
            }

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
                return "0001|電文09093 發查失敗" + strMessage;
            }
            else // 與主機交易執行結果成功,取回交易結果的資料集, 註: Result = true並不代表一定有資料傳,因為可能在所傳入的查詢條件下並無符合的資料,但交易是成功的
            {
                //執行過程的log  
                log = obj.MessageLog;
                //電文資料集
                DataSet returnDS = obj.HtgDataSet;
                #region 開始儲存至DB
                // 要把Account也存回TABLE中
                var masterCount = returnDS.Tables["TX_09093"].Rows.Count;
                foreach(DataRow dr in returnDS.Tables["TX_09093"].Rows)
                {
                    dr["Account"] = htparm["ACCT_NO"].ToString();
                }
                bool isSuccess = P4_SinglePage_SaveToDB(returnDS, CaseID, ObligorID, "09093", strSno);
                if (!isSuccess)
                {
                    return "0002|儲存09093失敗";
                }
                #endregion
                //return code
                Hashtable returnHT = obj.ReturnCode;
            }
            WriteLog(log);
            return "0000|" + strSno;
        }


        private string P4_9099U(string ObligorID, string CaseID, Hashtable htparm, string strSno)
        {
            HTGObject obj = new HTGObject(HTGUrl, HTGApplication, userId, passWord, racfId, racfPassWord, branchNo);

            bool result = obj.QueryHtg("09099", htparm);
            if (!result) // 執行第2次
            {
                System.Threading.Thread.Sleep(300);
                result = obj.QueryHtg("09099", htparm);
            }

            if (!result) // 執行第3次
            {
                System.Threading.Thread.Sleep(300);
                result = obj.QueryHtg("09099", htparm);
            }

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
                return "0001|電文09099 發查失敗" + strMessage;
            }
            else // 與主機交易執行結果成功,取回交易結果的資料集, 註: Result = true並不代表一定有資料傳,因為可能在所傳入的查詢條件下並無符合的資料,但交易是成功的
            {
                //執行過程的log  
                log = obj.MessageLog;
                //電文資料集
                DataSet returnDS = obj.HtgDataSet;
                #region 開始儲存至DB
                // 要把Account也存回TABLE中
                var masterCount = returnDS.Tables["TX_09099"].Rows.Count;
                foreach (DataRow dr in returnDS.Tables["TX_09099"].Rows)
                {
                    dr["Account"] = htparm["ACCT_NO"].ToString();
                }
                bool isSuccess = P4_SinglePage_SaveToDB(returnDS, CaseID, ObligorID, "09099", strSno);
                if (!isSuccess)
                {
                    return "0002|儲存09099失敗";
                }
                #endregion
                //return code
                Hashtable returnHT = obj.ReturnCode;
            }
            WriteLog(log);
            return "0000|" + strSno;
        }

        private string P4_9099R(string ObligorID, string CaseID, Hashtable htparm, string strSno)
        {
            HTGObject obj = new HTGObject(HTGUrl, HTGApplication, userId, passWord, racfId, racfPassWord, branchNo);
           
            bool result = obj.QueryHtg("09099R", htparm);
            if (!result) // 執行第2次
            {
                System.Threading.Thread.Sleep(300);
                result = obj.QueryHtg("09099R", htparm);
            }

            if (!result) // 執行第3次
            {
                System.Threading.Thread.Sleep(300);
                result = obj.QueryHtg("09099R", htparm);
            }

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
                return "0001|電文09099 發查失敗" + strMessage;
            }
            else // 與主機交易執行結果成功,取回交易結果的資料集, 註: Result = true並不代表一定有資料傳,因為可能在所傳入的查詢條件下並無符合的資料,但交易是成功的
            {
                //執行過程的log  
                log = obj.MessageLog;
                //電文資料集
                DataSet returnDS = obj.HtgDataSet;
                #region 開始儲存至DB
                // 要把Account也存回TABLE中
                var masterCount = returnDS.Tables["TX_09099"].Rows.Count;
                foreach (DataRow dr in returnDS.Tables["TX_09099"].Rows)
                {
                    dr["Account"] = htparm["ACCT_NO"].ToString();
                }
                bool isSuccess = P4_SinglePage_SaveToDB(returnDS, CaseID, ObligorID, "09099", strSno);
                if (!isSuccess)
                {
                    return "0002|儲存09099失敗";
                }
                #endregion
                //return code
                Hashtable returnHT = obj.ReturnCode;
            }
            WriteLog(log);
            return "0000|" + strSno;
        }



        private string P4_9091U(string ObligorID, string CaseID, Hashtable htparm, string strSno)
        {
            HTGObject obj = new HTGObject(HTGUrl, HTGApplication, userId, passWord, racfId, racfPassWord, branchNo);
            bool result = obj.QueryHtg("09091", htparm);

            if (!result) // 執行第2次
            {
                System.Threading.Thread.Sleep(300);
                result = obj.QueryHtg("09091", htparm);
            }

            if (!result) // 執行第3次
            {
                System.Threading.Thread.Sleep(300);
                result = obj.QueryHtg("09091", htparm);
            }

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
                return "0001|電文09091 發查失敗" + strMessage;
            }
            else // 與主機交易執行結果成功,取回交易結果的資料集, 註: Result = true並不代表一定有資料傳,因為可能在所傳入的查詢條件下並無符合的資料,但交易是成功的
            {
                //執行過程的log  
                log = obj.MessageLog;
                //電文資料集
                DataSet returnDS = obj.HtgDataSet;
                #region 開始儲存至DB
                var masterCount = returnDS.Tables["TX_09091"].Rows.Count;
                bool isSuccess = P4_SinglePage_SaveToDB(returnDS, CaseID, ObligorID, "09091", strSno);
                if (!isSuccess)
                {
                    return "0002|儲存09091失敗";
                }
                #endregion
                //return code
                Hashtable returnHT = obj.ReturnCode;
            }
            WriteLog(log);
            return "0000|" + strSno;
        }

        private string P4_9092U(string ObligorID, string CaseID, Hashtable htparm, string strSno)
        {
                 
            HTGObject obj = new HTGObject(HTGUrl, HTGApplication, userId, passWord, racfId, racfPassWord, branchNo);
            bool result = obj.QueryHtg("09092", htparm);
            if (!result) // 執行第2次
            {
                System.Threading.Thread.Sleep(300);
                result = obj.QueryHtg("09092", htparm);
            }

            if (!result) // 執行第3次
            {
                System.Threading.Thread.Sleep(300);
                result = obj.QueryHtg("09092", htparm);
            }


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
                return "0001|電文09092 發查失敗" + strMessage;
            }
            else // 與主機交易執行結果成功,取回交易結果的資料集, 註: Result = true並不代表一定有資料傳,因為可能在所傳入的查詢條件下並無符合的資料,但交易是成功的
            {
                //執行過程的log  
                log = obj.MessageLog;
                //電文資料集
                DataSet returnDS = obj.HtgDataSet;
                #region 開始儲存至DB
                var masterCount = returnDS.Tables["TX_09092"].Rows.Count;
                bool isSuccess = P4_SinglePage_SaveToDB(returnDS, CaseID, ObligorID, "09092", strSno);
                if (!isSuccess)
                {
                    return "0002|儲存09092失敗";
                }
                #endregion
                //return code
                Hashtable returnHT = obj.ReturnCode;
            }
            WriteLog(log);
            return "0000|" + strSno;
        }


        private string P4_67050U(string ObligorID, string CaseID, string strSno)
        {
            HTGObject obj = new HTGObject(HTGUrl, HTGApplication, userId, passWord, racfId, racfPassWord, branchNo);

            Hashtable htparm = new Hashtable();
            htparm.Add("CUST_ID_NO", ObligorID);
            bool result = obj.QueryHtg("67050", htparm);

            if (!result) // 執行第2次
            {
                System.Threading.Thread.Sleep(300);
                result = obj.QueryHtg("67050", htparm);
            }

            if (!result) // 執行第3次
            {
                System.Threading.Thread.Sleep(300);
                result = obj.QueryHtg("67050", htparm);
            }


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
                return "0001|電文67050 發查失敗" + strMessage;
            }
            else // 與主機交易執行結果成功,取回交易結果的資料集, 註: Result = true並不代表一定有資料傳,因為可能在所傳入的查詢條件下並無符合的資料,但交易是成功的
            {
                //執行過程的log  
                log = obj.MessageLog;
                //電文資料集
                DataSet returnDS = obj.HtgDataSet;
                #region 開始儲存至DB
                var masterCount = returnDS.Tables["TX_67102"].Rows.Count;
                bool isSuccess = P4_SinglePage_SaveToDB(returnDS, CaseID, ObligorID, "67102", strSno);
                if (!isSuccess)
                {
                    return "0002|儲存67102失敗";
                }
                #endregion
                //return code
                Hashtable returnHT = obj.ReturnCode;
            }
            WriteLog(log);
            return "0000|" + strSno;
        }

        private string P4_67050_8U(string ObligorID, string CaseID, Hashtable htparm, string strSno)
        {
            HTGObject obj = new HTGObject(HTGUrl, HTGApplication, userId, passWord, racfId, racfPassWord, branchNo);

            //Hashtable htparm = new Hashtable();
            //htparm.Add("CUST_ID_NO", ObligorID);
            bool result = obj.QueryHtg("670508", htparm);

            if (!result) // 執行第2次
            {
                System.Threading.Thread.Sleep(300);
                result = obj.QueryHtg("670508", htparm);
            }

            if (!result) // 執行第3次
            {
                System.Threading.Thread.Sleep(300);
                result = obj.QueryHtg("670508", htparm);
            }


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
                return "0001|電文67050 發查失敗" + strMessage;
            }
            else // 與主機交易執行結果成功,取回交易結果的資料集, 註: Result = true並不代表一定有資料傳,因為可能在所傳入的查詢條件下並無符合的資料,但交易是成功的
            {
                //執行過程的log  
                log = obj.MessageLog;
                //電文資料集
                DataSet returnDS = obj.HtgDataSet;
                #region 開始儲存至DB
                var masterCount = returnDS.Tables["TX_67002"].Rows.Count;
                bool isSuccess = P4_SinglePage_SaveToDB(returnDS, CaseID, ObligorID, "67002", strSno);
                if (!isSuccess)
                {
                    return "0002|儲存67002失敗";
                }
                #endregion
                //return code
                Hashtable returnHT = obj.ReturnCode;
            }
            WriteLog(log);
            return "0000|" + strSno;
        }

        private string P4_00401U(string ObligorID, string CaseID, string Acc_No,string ccy, string strSno)
        {
            
            HTGObject obj = new HTGObject(HTGUrl, HTGApplication, userId, passWord, racfId, racfPassWord, branchNo);




            Hashtable htparm = new Hashtable();
            htparm.Add("ACCT_NO", Acc_No);
            htparm.Add("CURRENCY", ccy);
            bool result = obj.QueryHtg("033401", htparm);

            if (!result) // 執行第2次
            {
                System.Threading.Thread.Sleep(300);
                result = obj.QueryHtg("033401", htparm);
            }

            if (!result) // 執行第3次
            {
                System.Threading.Thread.Sleep(300);
                result = obj.QueryHtg("033401", htparm);
            }


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
                return "0001|電文00417 發查失敗" + strMessage;
            }
            else // 與主機交易執行結果成功,取回交易結果的資料集, 註: Result = true並不代表一定有資料傳,因為可能在所傳入的查詢條件下並無符合的資料,但交易是成功的
            {
                //執行過程的log  
                log = obj.MessageLog;
                //電文資料集
                DataSet returnDS = obj.HtgDataSet;
                #region 開始儲存至DB
                var masterCount = returnDS.Tables["TX_33401"].Rows.Count;
                bool isSuccess = P4_33401_SaveToDB(returnDS, CaseID, ObligorID, strSno);
                if (!isSuccess)
                {
                    return "0002|儲存33401DB失敗";
                }
                #endregion
                Hashtable returnHT = obj.ReturnCode;
            }
            WriteLog(log);
            return "0000|" + strSno;
        }

        private string P4_00417U(string ObligorID, string CaseID, string Acc_No, string strSno)
        {
            
            HTGObject obj = new HTGObject(HTGUrl, HTGApplication, userId, passWord, racfId, racfPassWord, branchNo);

            Hashtable htparm = new Hashtable();

            htparm.Add("ACCT_NO", Acc_No);
            htparm.Add("STS_SEL", "0");

            bool result = obj.QueryHtg("00417", htparm);

            if (!result) // 執行第2次
            {
                System.Threading.Thread.Sleep(300);
                result = obj.QueryHtg("00417", htparm);
            }

            if (!result) // 執行第3次
            {
                System.Threading.Thread.Sleep(300);
                result = obj.QueryHtg("00417", htparm);
            }

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

                DataSet returnDS = obj.HtgDataSet;
                #region 開始儲存至DB
                var master = returnDS.Tables["TX_00417"];
                foreach (DataRow z in master.Rows)
                {                    
                        z["RepMessage"] = "0001|電文00417 發查失敗";
                }
                bool isSuccess = P4_SinglePage_SaveToDB(returnDS, CaseID, ObligorID, "00417", strSno);
                return  "0001|電文00417 發查失敗" + strMessage;
                #endregion
            }
            else // 與主機交易執行結果成功,取回交易結果的資料集, 註: Result = true並不代表一定有資料傳,因為可能在所傳入的查詢條件下並無符合的資料,但交易是成功的
            {
                //執行過程的log  
                log = obj.MessageLog;
                //電文資料集
                DataSet returnDS = obj.HtgDataSet;
                #region 開始儲存至DB
                var master = returnDS.Tables["TX_00417"];
                
                foreach( DataRow z in master.Rows)
                {
                    if( string.IsNullOrEmpty(z["Account"].ToString()))
                        z["Account"] = Acc_No;
                    if( ! string.IsNullOrEmpty(z["LinkAccount"].ToString()))
                        z["RepMessage"] = "0000|綜定, 連結帳號為:" + z["LinkAccount"].ToString();
                }
                bool isSuccess = P4_SinglePage_SaveToDB(returnDS, CaseID, ObligorID, "00417", strSno);
                if (!isSuccess)
                {
                    return  "0002|儲存00417DB失敗";
                }
                #endregion
                //return code
                Hashtable returnHT = obj.ReturnCode;
            }

            WriteLog(log);
            return "0000|" + strSno;
        }

        private string P4_00450U(string ObligorID, string CaseID, Hashtable htparm, string strSno)
        {
            HTGObject obj = new HTGObject(HTGUrl, HTGApplication, userId, passWord, racfId, racfPassWord, branchNo);



            bool result = obj.QueryHtg("00450", htparm);

            if (!result) // 執行第2次
            {
                System.Threading.Thread.Sleep(300);
                result = obj.QueryHtg("00450", htparm);
            }

            if (!result) // 執行第3次
            {
                System.Threading.Thread.Sleep(300);
                result = obj.QueryHtg("00450", htparm);
            }
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
                return "0001|電文00450 發查失敗" + strMessage;
            }
            else // 與主機交易執行結果成功,取回交易結果的資料集, 註: Result = true並不代表一定有資料傳,因為可能在所傳入的查詢條件下並無符合的資料,但交易是成功的
            {
                //執行過程的log  
                log = obj.MessageLog;
                //電文資料集
                string Acc_No = htparm["ACCT_NO"].ToString();
                DataSet returnDS = obj.HtgDataSet;
                #region 開始儲存至DB
                var masterCount = returnDS.Tables["TX_00450"].Rows.Count;
                string wxoption = htparm["TX_TYPE"].ToString();
                bool isSuccess = P4_00450_SaveToDB(returnDS, CaseID, ObligorID, wxoption, Acc_No, strSno);
                if (!isSuccess)
                {
                    return "0002|儲存00450DB失敗";
                }
                #endregion
                Hashtable returnHT = obj.ReturnCode;
            }
            WriteLog(log);
            return "0000|" + strSno;
        }

        private string P4_60629U(string ObligorID, string CaseID, string strSno)
        {
            HTGObject obj = new HTGObject(HTGUrl, HTGApplication, userId, passWord, racfId, racfPassWord, branchNo);

            Hashtable htparm = new Hashtable();

            htparm.Add("CUST_ID_NO", ObligorID);

            bool result = obj.QueryHtg("060629", htparm);
            if (!result) // 執行第2次
            {
                System.Threading.Thread.Sleep(300);
                result = obj.QueryHtg("060629", htparm);
            }

            if (!result) // 執行第3次
            {
                System.Threading.Thread.Sleep(300);
                result = obj.QueryHtg("060629", htparm);
            }


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
                return "0001|電文60629 發查失敗" +strMessage;
            }
            else // 與主機交易執行結果成功,取回交易結果的資料集, 註: Result = true並不代表一定有資料傳,因為可能在所傳入的查詢條件下並無符合的資料,但交易是成功的
            {
                //執行過程的log
                log = obj.MessageLog;
                //電文資料集
                DataSet returnDS = obj.HtgDataSet;


                #region 開始儲存至DB

                var masterCount = returnDS.Tables["TX_60629"].Rows.Count;
                bool isSuccess = P4_SinglePage_SaveToDB(returnDS, CaseID, ObligorID,"60629", strSno);                
                if (!isSuccess)
                {
                    return "0002|儲存60629DB失敗";
                }
                #endregion
                Hashtable returnHT = obj.ReturnCode;

            }
            WriteLog(log);
            return "0000|" + strSno;
        }

        private string P4_60600U(string ObligorID, string CaseID, string strSno)
        {
            HTGObject obj = new HTGObject(HTGUrl, HTGApplication, userId, passWord, racfId, racfPassWord, branchNo);

            Hashtable htparm = new Hashtable();

            htparm.Add("ACCT_NO", ObligorID);   
            htparm.Add("LIST_TX_FR_NO", "1");
            htparm.Add("LIST_TX_FR_DT", "01011991");
            htparm.Add("LIST_TX_TO_DT", DateTime.Now.ToString("ddMMyyyy"));           
            //htparm.Add("LIST_TX_TO_DT", "18042005");
            htparm.Add("ITM_TYPE","01");
            htparm.Add("ACCT_TYPE", "I");

            bool result = obj.QueryHtg("060600", htparm);
            if (!result) // 執行第2次
            {
                System.Threading.Thread.Sleep(300);
                result = obj.QueryHtg("060600", htparm);
            }

            if (!result) // 執行第3次
            {
                System.Threading.Thread.Sleep(300);
                result = obj.QueryHtg("060600", htparm);
            }



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
                return "0001|電文60600 發查失敗" + strMessage;
            }
            else // 與主機交易執行結果成功,取回交易結果的資料集, 註: Result = true並不代表一定有資料傳,因為可能在所傳入的查詢條件下並無符合的資料,但交易是成功的
            {
                //執行過程的log
                log = obj.MessageLog;
                //電文資料集
                DataSet returnDS = obj.HtgDataSet;

                #region 開始儲存至DB
                var masterCount = returnDS.Tables["TX_60600"].Rows.Count;
                bool isSuccess = P4_60600_SaveToDB(returnDS, CaseID, ObligorID, strSno);
                if (!isSuccess)
                {
                    return "0002|儲存60600DB失敗";
                }
                #endregion
                Hashtable returnHT = obj.ReturnCode;

            }
            WriteLog(log);
            return "0000|" + strSno;
        }

        private string P4_67100U(string obligorID, string caseID, string strSno)
        {

            HTGObject obj = new HTGObject(HTGUrl, HTGApplication, userId, passWord, racfId, racfPassWord, branchNo);

            Hashtable htparm = new Hashtable();

            htparm.Add("CUST_ID_NO", obligorID);
            bool result = obj.QueryHtg("067100", htparm);

            if (!result) // 執行第2次
            {
                System.Threading.Thread.Sleep(300);
                result = obj.QueryHtg("067100", htparm);
            }

            if (!result) // 執行第3次
            {
                System.Threading.Thread.Sleep(300);
                result = obj.QueryHtg("067100", htparm);
            }

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
                return "0001|電文67100 發查失敗" + strMessage;
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
                    return "0002|儲存607100DB失敗";
                }




                #endregion
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
            if (!result) // 執行第2次
            {
                System.Threading.Thread.Sleep(300);
                result = obj.QueryHtg("067072", htparm);
            }

            if (!result) // 執行第3次
            {
                System.Threading.Thread.Sleep(300);
                result = obj.QueryHtg("067072", htparm);
            }

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
                return "0001|電文67072 發查失敗" + strMessage;
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
                //bool isSuccess = P4_60491_SaveToDB(returnDS, caseID, obligorID);
                bool isSuccess = P4_MultiPage_SaveToDB(returnDS, caseID, obligorID, "67072", strSno);
                if (!isSuccess)
                {
                    return "0002|儲存60491DB失敗";
                }




                #endregion                

                Hashtable returnHT = obj.ReturnCode;
                
            }
            WriteLog(log);
            return "0000|" + strSno;
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
            if( !result) // 執行第2次
            {
                System.Threading.Thread.Sleep(300);
                result = obj.QueryHtg("060491", htparm);
            }

            if (!result) // 執行第3次
            {
                System.Threading.Thread.Sleep(300);
                result = obj.QueryHtg("060491", htparm);
            }

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
                return "0001|電文60491 發查失敗" + strMessage;
            }
            else // 與主機交易執行結果成功,取回交易結果的資料集, 註: Result = true並不代表一定有資料傳,因為可能在所傳入的查詢條件下並無符合的資料,但交易是成功的
            {
                //執行過程的log
                log = obj.MessageLog;
                //電文資料集
                DataSet returnDS = obj.HtgDataSet;


                #region 開始儲存至DB

                //var masterCount = returnDS.Tables["TX_60491_Grp"].Rows.Count;
                //var detailCount = returnDS.Tables["TX_60491_detl"].Rows.Count;
                bool isSuccess = P4_MultiPage_SaveToDB(returnDS, caseID, obligorID, "60491", strSno);
                if (!isSuccess)
                {
                    return "0002|儲存60491DB失敗";
                }




                #endregion
                // 20181012, 看來不用在60491中, 發查
                //#region 發查33401 , 並更新returnDS.Detail


                ////var detailCount = returnDS.Tables["TX_60491_detl"].Rows.Count;

                //foreach (DataRow dr in returnDS.Tables["TX_60491_Detl"].Rows)
                //{
                //    if (dr["System"].ToString().Trim() == "B" || dr["System"].ToString().Trim() == "T"  ) // 若是法金, 個金, 及放款的.. 皆是B, T, 要排除
                //        continue;
                //    Hashtable htparm401 = new Hashtable();
                //    htparm401.Add("CURRENCY", dr["Ccy"].ToString());
                //    htparm401.Add("applicationId", "CSFS");
                //    htparm401.Add("ACCT_NO", dr["Account"].ToString());
                //    //var aaa = dr["Account"].ToString();
                //    //string bbb = aaa;
                //    bool result401 = obj.QueryHtg("033401", htparm401);
                //    if (result401)
                //    {
                //        Hashtable errormessage = obj.ReturnCode;
                //        var errorlog = obj.MessageLog;
                //        DataSet return401 = obj.HtgDataSet;
                //        int rowCount = return401.Tables["TX_33401"].Rows.Count;
                //        bool isSuccess1 = P4_33401_SaveToDB(return401, caseID, obligorID,strSno);
                //        if (!isSuccess1)
                //        {
                //            return "0002|儲存33401DB失敗";
                //        }
                //    }
                //    else
                //    {
                //        return "0001|發查401失敗";
                //    }
                //}


                //#endregion



                //return code
                Hashtable returnHT = obj.ReturnCode;
                WriteLog(log);
                return "0000|" + strSno;
            }


        }


        private bool P4_60600_SaveToDB(DataSet htgdata, string CaseID, string ObligorID, string strSno)
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
                        //string strIdentityKey = hostbiz.GetIdentityKey(strTableName);
                        foreach (DataRow dr in dt.Rows)
                        {
                            string strSql = "insert into " + strTableName + " (";
                            strSql += "[cCretDT],caseId, TrnNum,";
                            //strSql += "[SNO],[cCretDT],caseId,";
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
                            strSql += "[CifNo],[cCretDT],caseId, TrnNum,";
                            //strSql += "[SNO],[cCretDT],caseId,";
                            //DataTable dtNew = dic[key];
                            for (int i = 0; i < dt.Columns.Count; i++)
                            {
                                strSql += "[" + dt.Columns[i].ColumnName + "],";
                            }
                            strSql = strSql.TrimEnd(',') + ") values(";
                            //strSql += "GETDATE(),'" + CaseID + "','" + strSno + "'," ;
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

        private bool P4_00450_SaveToDB(DataSet htgdata, string CaseID, string ObligorID,string WXOption, string account, string strSno)
        {

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
                            strSql += "GETDATE(),'" + CaseID + "','" + WXOption +"','" + account + "','" + strSno + "',";
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


        private bool P4_SinglePage_SaveToDB(DataSet htgdata, string CaseID, string ObligorID,string TxID, string strSno)
        {

            HostMsgGrpBIZ hostbiz = new HostMsgGrpBIZ();
            ArrayList array = new ArrayList();
            //foreach (DataTable dt in htgdata.Tables)
            {
                string strTN = "TX_" + TxID ;

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
                            strSql += "GETDATE(),'" + CaseID + "','" + strSno + "'," ;
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
                            if (string.IsNullOrEmpty(CaseID))
                                strSql += "'" + strIdentityKey + "',GETDATE(), null,'" + strSno + "',";
                            else
                                strSql += "'" + strIdentityKey + "',GETDATE(),'" + CaseID + "','" + strSno +"',";
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
            string baseDir = AppDomain.CurrentDomain.BaseDirectory + "XML";
            if (Directory.Exists(baseDir) == false)
            {
                Directory.CreateDirectory(baseDir);
            }
            string filename = DateTime.Now.ToString("yyyyMMdd");
            lock (_lockLog){

                System.IO.File.AppendAllText(baseDir + "\\" + filename + ".log", msg);

            }
            
            //LogManager.Exists("DebugLog").Debug(msg);
            //log.Info(msg);
        }
    }
}
