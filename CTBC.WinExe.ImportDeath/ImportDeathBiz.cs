using CTBC.CSFS.BussinessLogic;
using CTBC.CSFS.Models;
using CTBC.CSFS.Pattern;
using log4net;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTBC.WinExe.ImportDeath
{
    public class ImportDeathBiz : BaseBusinessRule
    {
        public CaseDeadVersion getLastCaseDeadVersion(DateTime dt)
        {
            string SQL = string.Format("SELECT TOP 1 * FROM [dbo].[CaseDeadVersion] where SendStatus='0' and CONVERT(varchar(20),ModifiedDate, 23)='{0}' order by ModifiedDate desc", dt.ToString("yyyy-MM-dd"));
            return base.SearchList<CaseDeadVersion>(SQL).FirstOrDefault();
        }

        public List<CaseDeadVersion> getCaseDeadVersionByCaseNo(string CaseNo)
        {
            string SQL = "SELECT * FROM [dbo].[CaseDeadVersion] where DocNo='{0}' order by Seq ";
            SQL = string.Format(SQL, CaseNo);
            return base.SearchList<CaseDeadVersion>(SQL).ToList();
        }


        /// <summary>
        /// 當天是否為工作日, 若是, 才可以執行本程式
        /// </summary>
        /// <param name="BizDate"></param>
        /// <returns></returns>
        internal bool? getWorkDay(DateTime BizDate)
        {
            string SQL = "SELECT TOP 1 * FROM [dbo].[PARMWorkingDay] where date ='{0}' ";
            SQL = string.Format(SQL, BizDate.ToString("yyyy-MM-dd"));
            var d = base.SearchList<PARMWorkingDay>(SQL).FirstOrDefault();
            if( d!=null)
            {
                return d.Flag;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 取得前一天的工作日
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        internal DateTime getLastWorkDay(DateTime BizDate)
        {
            string SQL = "SELECT TOP 1 * FROM [dbo].[PARMWorkingDay] where  Flag=1 and  date<'{0}' order by [date] desc";
            SQL = string.Format(SQL, BizDate.ToString("yyyy-MM-dd"));
            var d = base.SearchList<PARMWorkingDay>(SQL).FirstOrDefault();
            if (d != null)
            {
                return d.Date;
            }
            else
            {
                return BizDate.AddDays(-1);
            }
        }


        internal bool importIn11(CaseDeadVersion caseDeath, DateTime lastWorkDay, string baseDir, string prefix)
        {
            // 放款檔(BR0966_YYYYMMDD.txt檔), 的切割位置
            List<int> Br_Pos = new List<int>() {
                        10,60,14,8,30,20,6,20,30
                    };

            try
            {
                string fullName = Path.Combine(baseDir, prefix + lastWorkDay.ToString("yyyyMMdd") + ".txt");
                if (File.Exists(fullName))
                {
                    string[] lines = File.ReadAllLines(fullName, Encoding.Default);

                    WriteLog(string.Format("\t處理檔案 {0} ", fullName));

                    int Seq = 1; int succ = 0; int fail = 0;
                    foreach (string line in lines)
                    {
                        if (string.IsNullOrEmpty(line) || line.Length < 20)
                            continue;

                        string[] re = splitByPos(line, Br_Pos);
                        int rtn = InsertIn11DB(caseDeath, re, Seq++);
                        if (rtn == 1)
                            succ++;
                        else
                        {
                            WriteLog(string.Format("\t\t存入 ID :  {0} 失敗", re[0]));
                            fail++;
                        }
                    }
                    WriteLog(string.Format("\t\t存入 ID : 成功 {0} 筆, 失敗{1}", succ.ToString(), fail.ToString()));
                }
                else
                {
                    WriteLog(string.Format("\t檔案不存在 {0} ", fullName));
                }
                
            }
            catch (Exception ex)
            {
                WriteLog(string.Format("匯入INHIS11資料庫發生錯誤,  訊息: {0}", ex.Message.ToString()));
                return false;
            }
            return true;
        }

        private int InsertIn11DB(CaseDeadVersion caseDeath, string[] r, int Seq)
        {
            DateTime thenow = DateTime.Now;
            string sql = @"INSERT INTO [dbo].[CaseDeathInvestment] ([ID_NO], [UTIDNO], [BANK_NAME], [PROVIDE_DATE], [DESCRIPTION], [UNIT], [CURRENCY], [BAL], [MEMO], [CaseDeadVersionNewID], [CaseNo], [Seq], [CreatedDate]) Values (@ID_NO,@UTIDNO,@BANK_NAME,@PROVIDE_DATE,@DESCRIPTION,@UNIT,@CURRENCY,@BAL,@MEMO,@CaseDeadVersionNewID,@CaseNo,@Seq,@CreatedDate)";


            try
            {
                base.Parameter.Clear();

                base.Parameter.Add(new CommandParameter("@ID_NO", r[0]));
                base.Parameter.Add(new CommandParameter("@UTIDNO",  r[1]));
                base.Parameter.Add(new CommandParameter("@BANK_NAME",  r[2]));
                base.Parameter.Add(new CommandParameter("@PROVIDE_DATE",  r[3]));
                base.Parameter.Add(new CommandParameter("@DESCRIPTION",  r[4]));
                base.Parameter.Add(new CommandParameter("@UNIT",  r[5]));
                base.Parameter.Add(new CommandParameter("@CURRENCY",  r[6]));
                base.Parameter.Add(new CommandParameter("@BAL",  r[7]));
                base.Parameter.Add(new CommandParameter("@MEMO", r[8]));
                base.Parameter.Add(new CommandParameter("@CaseDeadVersionNewID", caseDeath.CaseTrsNewID));
                base.Parameter.Add(new CommandParameter("@CaseNo", caseDeath.DocNo));
                base.Parameter.Add(new CommandParameter("@Seq", Seq++));
                base.Parameter.Add(new CommandParameter("@CreatedDate", thenow));
                base.ExecuteNonQuery(sql);
            }
            catch (Exception ex)
            {

                WriteLog(string.Format("新增INHIS11資料庫發生錯誤, ID : {0} 訊息: {1}", r[0], ex.Message.ToString()));
                return 2;
            }

            return 1;

        }


        internal bool importIn10(CaseDeadVersion caseDeath, DateTime lastWorkDay, string baseDir, string prefix)
        {
            // 放款檔(BR0966_YYYYMMDD.txt檔), 的切割位置
            List<int> Br_Pos = new List<int>() {
                        10,60,14,8,16,16,3,3,7,1,13,16
                    };

            try
            {
                string fullName = Path.Combine(baseDir, prefix + lastWorkDay.ToString("yyyyMMdd") + ".txt");
                if (File.Exists(fullName))
                {
                    string[] lines = File.ReadAllLines(fullName, Encoding.Default);

                    WriteLog(string.Format("\t處理檔案 {0} ", fullName));

                    int Seq = 1; int succ = 0; int fail = 0;
                    foreach (string line in lines)
                    {
                        if (string.IsNullOrEmpty(line) || line.Length < 20)
                            continue;

                        string[] re = splitByPos(line, Br_Pos);
                        int rtn = InsertIn10DB(caseDeath, re, Seq++);
                        if (rtn == 1)
                            succ++;
                        else
                        {
                            WriteLog(string.Format("\t\t存入 ID :  {0} 失敗", re[0]));
                            fail++;
                        }
                    }
                    WriteLog(string.Format("\t\t存入 ID : 成功 {0} 筆, 失敗{1}", succ.ToString(), fail.ToString()));
                }
                else
                {
                    WriteLog(string.Format("\t檔案不存在 {0} ", fullName));
                }
            }
            catch (Exception ex)
            {
                WriteLog(string.Format("匯入INHIS10資料庫發生錯誤,  訊息: {0}", ex.Message.ToString()));
                return false;
            }
            return true;
        }

        private int InsertIn10DB(CaseDeadVersion caseDeath, string[] r, int Seq)
        {
            DateTime thenow = DateTime.Now;
            string sql = @"INSERT INTO [dbo].[CaseDeathDeposit] ([ID_NO], [UTIDNO], [BANK_NAME], [PROVIDE_DATE], [DEP_KIND], [ACCT_NO], [CURRENCY_TWD], [CURRENCY_FITAS], [RATE], [SIGN], [BAL], [MEMO], [CaseDeadVersionNewID], [CaseNo], [Seq], [CreatedDate]) Values (@ID_NO, @UTIDNO, @BANK_NAME, @PROVIDE_DATE, @DEP_KIND, @ACCT_NO, @CURRENCY_TWD, @CURRENCY_FITAS, @RATE, @SIGN, @BAL, @MEMO, @CaseDeadVersionNewID, @CaseNo, @Seq, @CreatedDate)";


            try
            {
                base.Parameter.Clear();
                decimal _rate = 0.0m;
                string strRate = r[8].Substring(0, 2) + "." + r[8].Substring(2);
                decimal.TryParse(strRate, out _rate);

                base.Parameter.Add(new CommandParameter("@ID_NO",r[0]));
                base.Parameter.Add(new CommandParameter("@UTIDNO",r[1]));
                base.Parameter.Add(new CommandParameter("@BANK_NAME",r[2]));
                base.Parameter.Add(new CommandParameter("@PROVIDE_DATE",r[3]));
                base.Parameter.Add(new CommandParameter("@DEP_KIND",r[4]));
                base.Parameter.Add(new CommandParameter("@ACCT_NO",r[5]));
                base.Parameter.Add(new CommandParameter("@CURRENCY_TWD",r[6]));
                base.Parameter.Add(new CommandParameter("@CURRENCY_FITAS",r[7]));
                base.Parameter.Add(new CommandParameter("@RATE", _rate));
                base.Parameter.Add(new CommandParameter("@SIGN",r[9]));
                base.Parameter.Add(new CommandParameter("@BAL",r[10]));
                base.Parameter.Add(new CommandParameter("@MEMO", r[11]));
                base.Parameter.Add(new CommandParameter("@CaseDeadVersionNewID",caseDeath.CaseTrsNewID));
                base.Parameter.Add(new CommandParameter("@CaseNo", caseDeath.DocNo));
                base.Parameter.Add(new CommandParameter("@Seq", Seq++));
                base.Parameter.Add(new CommandParameter("@CreatedDate", thenow));
                base.ExecuteNonQuery(sql);
            }
            catch (Exception ex)
            {

                WriteLog(string.Format("新增INHIS10資料庫發生錯誤, ID : {0} 訊息: {1}", r[0], ex.Message.ToString()));
                return 2;
            }

            return 1;

        }




        internal bool importBR(CaseDeadVersion caseDeath, DateTime lastWorkDay, string baseDir, string prefix)
        {
            // 放款檔(BR0966_YYYYMMDD.txt檔), 的切割位置
            List<int> Br_Pos = new List<int>() {
                        10,60,16,8,8,8,8,8,30,16
                    };

            try
            {
                string fullName = Path.Combine(baseDir, prefix + lastWorkDay.ToString("yyyyMMdd") + ".txt");
                if (File.Exists(fullName))
                {
                    string[] lines = File.ReadAllLines(fullName, Encoding.Default);

                    WriteLog(string.Format("\t處理檔案 {0} ", fullName));

                    int succ = 0; int fail = 0;
                    int Seq = 1;
                    foreach (string line in lines)
                    {
                        if (string.IsNullOrEmpty(line) || line.Length < 10)
                            continue;

                        string[] re = splitByPos(line, Br_Pos);
                        int rtn = InsertBr2DB(caseDeath, re, Seq++);
                        if (rtn == 1)
                            succ++;
                        else
                        {
                            WriteLog(string.Format("\t\t存入 ID :  {0} 失敗", re[0]));
                            fail++;
                        }
                    }
                    WriteLog(string.Format("\t\t存入 ID : 成功 {0} 筆, 失敗{1}", succ.ToString(), fail.ToString()));
                }
                else
                {
                    WriteLog(string.Format("\t檔案不存在 {0} ", fullName));
                }
            }
            catch (Exception ex)
            {
                WriteLog(string.Format("匯入BR0966資料庫發生錯誤,  訊息: {0}", ex.Message.ToString()));
                return false;
            }      
            return true;
        }

        private int InsertBr2DB(CaseDeadVersion caseDeath, string[] r, int Seq)
        {
            //20201204, 要先判斷, 是TABLE中是否已有相同帳號的資料.. 若有代表是理債檔上傳的.. 
            // 以理債檔為主, 主機的資料為輔

            string qSql = @"SELECT * from [CaseDeathLoan] WHERE CaseNo=@CaseNo AND ACC_NO=@ACC_NO";
            try
            {
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@ACC_NO", r[2]));
                base.Parameter.Add(new CommandParameter("@CaseNo", caseDeath.DocNo));
                var ret = base.SearchList<CaseDeathLoan>(qSql).ToList();
                if (ret.Count() > 0)
                    return 1;
            }
            catch (Exception ex)
            {
                WriteLog(string.Format("新增BR0966資料庫發生錯誤, ID : {0} 訊息: {1}", r[0], ex.Message.ToString()));
                return 2;
            }



            DateTime thenow = DateTime.Now;            
            string sql = @"INSERT INTO [dbo].[CaseDeathLoan] ([IDHIS_IDNO_ACNO], [UTIDNO], [ACC_NO], [BANK], [BASE_DATE], [PROD_TYPE], [CONTRACT_S_DT], [CONTRACT_E_DT], [USE_TYPE], [BAL], [CaseDeadVersionNewID], [CaseNo], [Seq], [CreatedDate]) Values (@IDHIS_IDNO_ACNO, @UTIDNO, @ACC_NO, @BANK, @BASE_DATE, @PROD_TYPE, @CONTRACT_S_DT, @CONTRACT_E_DT, @USE_TYPE, @BAL, @CaseDeadVersionNewID, @CaseNo, @Seq, @CreatedDate)";


            try
            {
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@IDHIS_IDNO_ACNO", r[0]));
                base.Parameter.Add(new CommandParameter("@UTIDNO", r[1]));
                base.Parameter.Add(new CommandParameter("@ACC_NO", r[2]));
                base.Parameter.Add(new CommandParameter("@BANK", r[3]));
                base.Parameter.Add(new CommandParameter("@BASE_DATE", r[4]));
                base.Parameter.Add(new CommandParameter("@PROD_TYPE", r[5]));
                base.Parameter.Add(new CommandParameter("@CONTRACT_S_DT", r[6]));
                base.Parameter.Add(new CommandParameter("@CONTRACT_E_DT", r[7]));
                base.Parameter.Add(new CommandParameter("@USE_TYPE", r[8]));
                base.Parameter.Add(new CommandParameter("@BAL", r[9]));
                base.Parameter.Add(new CommandParameter("@CaseDeadVersionNewID", caseDeath.CaseTrsNewID));
                base.Parameter.Add(new CommandParameter("@CaseNo", caseDeath.DocNo));
                base.Parameter.Add(new CommandParameter("@Seq", Seq++));
                base.Parameter.Add(new CommandParameter("@CreatedDate", thenow));
                base.ExecuteNonQuery(sql);                
            }
            catch (Exception ex)
            {

                WriteLog(string.Format("新增BR0966資料庫發生錯誤, ID : {0} 訊息: {1}", r[0], ex.Message.ToString()));
                return 2;
            }

            return 1;
            
        }


        private static string[] splitByPos(string OrginTxt, List<int> Pos)
        {
            List<string> result = new List<string>();
            int TotalLen = getLength(OrginTxt);
            int strLength = 0;

            //if (TotalLen <= 900)
            //{
            //    OrginTxt = OrginTxt.PadRight(900, ' ');
            //}

            char[] Temp = OrginTxt.ToCharArray();

            string txt = OrginTxt;
            int prePos = 0;
            int realPos = 0;

            foreach (var p in Pos)
            {
                for (int i = 0; i != Temp.Length; i++)
                {
                    if (((int)Temp[i]) < 255) //大於255的都是漢字或者特殊字符
                    {
                        strLength++;
                        realPos++;
                    }
                    else
                    {
                        strLength = strLength + 2;
                        realPos++;
                    }
                    if (strLength == p) // 斷開
                    {
                        result.Add(txt.Substring(0, realPos));
                        prePos = p;

                        txt = txt.Substring(realPos);
                        realPos = 0;
                        strLength = 0;
                        break;
                    }
                }
                Temp = txt.ToCharArray();
            }
            return result.ToArray();
        }


        private static int getLength(string txt)
        {
            int strLength = 0;
            int realPos = 0;
            char[] Temp = txt.ToCharArray();
            for (int i = 0; i != Temp.Length; i++)
            {
                if (((int)Temp[i]) < 255) //大於255的都是漢字或者特殊字符
                {
                    strLength++;
                    realPos++;
                }
                else
                {
                    strLength = strLength + 2;
                    realPos++;
                }
            }
            return strLength;
        }

        public void WriteLog(string msg)
        {
            if (Directory.Exists(@".\Log") == false)
            {
                Directory.CreateDirectory(@".\Log");
            }
            LogManager.Exists("DebugLog").Debug(msg);
        }

        internal List<CaseDeathDeposit> getDeathDeposit(string CaseNo)
        {

            string sql = @"SELECT * FROM [dbo].[CaseDeathDeposit] where CaseNO=@CaseNO";
            try
            {
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@CaseNO", CaseNo));
                return base.SearchList<CaseDeathDeposit>(sql).ToList();
            }
            catch (Exception ex)
            {
                WriteLog(string.Format("查詢CaseDeathDeposit 資料庫發生錯誤, ID : {0} 訊息: {1}", CaseNo, ex.Message.ToString()));
                return null;
            }
        }

        internal List<CaseDeathInvestment> getDeathInvest(string CaseNo)
        {
            string sql = @"SELECT * FROM [dbo].[CaseDeathInvestment] where CaseNO=@CaseNO";
            try
            {
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@CaseNO", CaseNo));
                return base.SearchList<CaseDeathInvestment>(sql).ToList();
            }
            catch (Exception ex)
            {
                WriteLog(string.Format("查詢CaseDeathInvestment 資料庫發生錯誤, ID : {0} 訊息: {1}", CaseNo, ex.Message.ToString()));
                return null;
            }
        }

        internal List<CaseDeathLoan> getDeathLoan(string CaseNo)
        {
            string sql = @"SELECT  *  FROM [dbo].[CaseDeathLoan] where CaseNO=@CaseNO";
            try
            {
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@CaseNO", CaseNo));
                return base.SearchList<CaseDeathLoan>(sql).ToList();
            }
            catch (Exception ex)
            {
                WriteLog(string.Format("查詢CaseDeathLoan 資料庫發生錯誤, ID : {0} 訊息: {1}", CaseNo, ex.Message.ToString()));
                return null;
            }
        }

        /// <summary>
        /// 找出所有ID
        /// </summary>
        /// <param name="_deadDeposit"></param>
        /// <param name="_deadInvest"></param>
        /// <param name="_deadLoan"></param>
        /// <returns></returns>
        internal List<DeathViewModel> getUnionInfo(string CaseNo, List<CaseDeathDeposit> _deadDeposit, List<CaseDeathInvestment> _deadInvest, List<CaseDeathLoan> _deadLoan)
        {
            var CaseDeathVersion = getCaseDeadVersionByCaseNo(CaseNo);
            
            // 若某一個ID, 沒有投資. 也要在第一欄位, 塞入"無資料"
            List<DeathViewModel> output = new List<DeathViewModel>();
            int Sno = 1;
            foreach (var id in CaseDeathVersion)
            {
                //20201126, IR-5 基準日93年12月31日以前, 資料不應該產回
                string baseDate = id.HeirDeadDate.Trim();
                if(! string.IsNullOrEmpty(baseDate))
                {
                    if (baseDate.Length == 6)
                        baseDate = "0" + baseDate;

                    if (baseDate.Length == 7)
                    {
                        try
                        {
                            int iYear = int.Parse(baseDate.Substring(0, 3)) + 1911;
                            int iMonth = int.Parse(baseDate.Substring(3, 2));
                            int iDay = int.Parse(baseDate.Substring(5, 2));
                            DateTime dt = new DateTime(iYear, iMonth, iDay);
                            if (dt < new DateTime(2005, 1, 1)) // 小於2005/01/01 , 即民國94年1月1日
                                continue;
                        }
                        catch (Exception)
                        {
                        }
                    }
                }


                var o = new DeathViewModel();

                //20201224, 針對重覆申請人, 申請同一個被繼承人時, 主機會產生二筆一樣的資料, 所以要Group by 帳號
                var deposit = _deadDeposit.Where(x => x.ID_NO == id.HeirId).GroupBy(x=>x.ACCT_NO).Select(g=>g.First()).ToList();
                var invest = _deadInvest.Where(x => x.ID_NO == id.HeirId).GroupBy(x => new {x.DESCRIPTION, x.BAL} ).Select(g => g.First()).ToList();
                var loan = _deadLoan.Where(x => x.IDHIS_IDNO_ACNO == id.HeirId).GroupBy(x => x.ACC_NO).Select(g => g.First()).ToList();
                // 20201126, IR-6 報表資訊及保管箱、理債檔皆無，則無需產出該ID之PDF。
                if (deposit.Count() == 0 && invest.Count() == 0 && loan.Count() == 0)
                    continue;



                o.Sno = Sno;
                o.IDNO = id.HeirId;
                o.Name = id.HeirName;
                o.BaseDate = id.HeirDeadDate;

                // 20201218, 到TX60490.sBoxFlag去找保管箱, 若有重號, 則其中只要有一個有保管箱, 就設為Y , N及空白, 都是N
                string sBox = getSafeBox(id.CaseTrsNewID,o.IDNO);
                o.SBox_Flag = sBox;

                List<DepositViewModel> deResult = new List<DepositViewModel>();
                if (deposit.Count() > 0)
                {
                    foreach (var de in deposit)
                    {
                        // 放款類的餘額, 是最後2位為小數點
                        // 加上千分逗號
                        string strBal = "0.00";
                        decimal iBal = 0;
                        
                        if (decimal.TryParse(de.BAL, out iBal))
                        {
                            // 20210511, 要求Deposit 的餘額, 若是"-"號, 才要秀... 也要加上符號
                            if (de.SIGN.Trim().Equals("-"))
                                strBal = de.SIGN + String.Format("{0:N2}", (iBal / 100));
                            else
                                strBal = String.Format("{0:N2}", (iBal / 100));
                        }
                        if (!string.IsNullOrEmpty(de.CURRENCY_TWD.Trim()))
                            deResult.Add(new DepositViewModel() { BANK_NAME = de.BANK_NAME, DEP_KIND = de.DEP_KIND, ACCT_NO = de.ACCT_NO, CURRENCY_TWD = de.CURRENCY_TWD, BAL = strBal });
                        else
                            deResult.Add(new DepositViewModel() { BANK_NAME = de.BANK_NAME, DEP_KIND = de.DEP_KIND, ACCT_NO = de.ACCT_NO, CURRENCY_TWD = de.CURRENCY_FITAS, BAL = strBal });
                    }
                }
                else
                {
                    deResult.Add(new DepositViewModel() { BANK_NAME = "無資料" });
                }
                o.Deposits = deResult.ToArray();



                List<InvestViewModel> invResult = new List<InvestViewModel>();
                if (invest.Count() > 0)
                {
                    foreach (var inv in invest)
                    {
                        string strBal = "0";
                        if (inv.BAL.StartsWith("+") || inv.BAL.StartsWith("-"))
                        {
                            string sign = inv.BAL.Substring(0, 1);                            
                            int iBal = 0;
                            // 20210511, 要求Invest 的餘額, 若是"-"號, 才要秀... 也要加上符號
                            if (int.TryParse(inv.BAL.Substring(1), out iBal))
                            {
                                if( sign.Equals("-"))
                                    strBal = sign + String.Format("{0:N0}", (iBal));
                                else
                                    strBal = String.Format("{0:N0}", (iBal));
                            }
                        }
                        else
                        {
                            int iBal = 0;
                            if (int.TryParse(inv.BAL, out iBal))
                                strBal = String.Format("{0:N0}", (iBal));
                        }
                        invResult.Add(new InvestViewModel() { BANK_NAME = inv.BANK_NAME, DESCRIPTION = inv.DESCRIPTION, UNIT = inv.UNIT, CURRENCY = inv.CURRENCY, BAL = strBal });
                    }
                }
                else
                {
                    invResult.Add(new InvestViewModel() { BANK_NAME = "無資料" });
                }
                o.Invests = invResult.ToArray();



                List<LoanViewModel> loResult = new List<LoanViewModel>();
                if (loan.Count() > 0)
                {
                    foreach (var lo in loan)
                    {
                        string strBal = "0.00";                        
                        if (lo.BAL.StartsWith("+") || lo.BAL.StartsWith("-"))
                        {
                            string sign = lo.BAL.Substring(0, 1);
                            decimal iBal = 0;

                            if (decimal.TryParse(lo.BAL.Substring(1), out iBal))
                            {
                                //strBal = sign + String.Format("{0:N2}", (iBal / 100));
                                // 20210511, 要求Loan 的餘額, 若是"-"號, 才要秀... 也要加上符號
                                if( sign.Equals("-"))
                                    strBal = sign + String.Format("{0:N2}", (iBal / 100));
                                else
                                    strBal = String.Format("{0:N2}", (iBal / 100));

                            }
                        }
                        else
                        {
                            decimal iBal = 0;
                            if (decimal.TryParse(lo.BAL, out iBal))
                                strBal = String.Format("{0:N2}", (iBal / 100));
                        }
                        loResult.Add(new LoanViewModel() { PROD_TYPE = lo.PROD_TYPE, ACC_NO = lo.ACC_NO, BAL = strBal });
                    }
                }
                else
                {
                    loResult.Add(new LoanViewModel() { PROD_TYPE = "無資料" });
                }
                o.Loans = loResult.ToArray();



                output.Add(o);
                Sno++;
            }
            return output;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        private string getSafeBox(Guid caseid, string id)
        {
            //到TX_60491_Grp 中, 去找, 在重號中, 若有一個"Y", 就是Y, 
            // N 跟空白, 都視為N 
            string SQL = string.Format("SELECT * FROM [dbo].[TX_60491_Grp] where CustomerId like '" + id + "%' and CaseId='{0}' and SboxFlag='Y'", caseid.ToString());
            var allGrp = base.SearchList<TX_60491_Grp>(SQL).ToList();
            if (allGrp.Count() > 0)
                return "Y";
            else
                return "N";
        }



        internal string importCaseEdocFile(CaseDeadVersion caseDeath, string ouputPDF, string filename)
        {
            string result = "0";
            try
            {

                using (FileStream fs = File.OpenRead(ouputPDF))
                {
                    ImportEDocBiz _ImportEDocBiz = new ImportEDocBiz(); ;

                    StreamReader sr = new StreamReader(ouputPDF, Encoding.UTF8);

                    CaseEdocFile caseEdocFile = new CaseEdocFile();
                    caseEdocFile.CaseId = caseDeath.CaseTrsNewID;
                    caseEdocFile.Type = "死亡";

                    caseEdocFile.FileType = "pdf";
                    caseEdocFile.FileName = filename + ".zip";

                    byte[] bytes = new byte[fs.Length];
                    fs.Position = 0;
                    fs.Read(bytes, 0, bytes.Length);

                    caseEdocFile.FileObject = bytes;
                    caseEdocFile.SendNo = caseDeath.DocNo;

                    //若有, 則先行刪除
                    {
                        string deleteSQL = @"DELETE from CaseEdocFile where CaseId = @CaseId and Type = @Type and FileType = @FileType and SendNo = @SendNo";
                        base.Parameter.Clear();
                        base.Parameter.Add(new CommandParameter("@CaseId", caseEdocFile.CaseId));
                        base.Parameter.Add(new CommandParameter("@Type", caseEdocFile.Type));
                        base.Parameter.Add(new CommandParameter("@FileType", caseEdocFile.FileType));
                        base.Parameter.Add(new CommandParameter("@SendNo", caseEdocFile.SendNo));
                        base.ExecuteNonQuery(deleteSQL);
                    }

                    string sql = @"insert into CaseEdocFile values(@CaseId,@Type,@FileType,@FileName,@FileObject,@SendNo)";
                    base.Parameter.Clear();
                    base.Parameter.Add(new CommandParameter("@CaseId", caseEdocFile.CaseId));
                    base.Parameter.Add(new CommandParameter("@Type", caseEdocFile.Type));
                    base.Parameter.Add(new CommandParameter("@FileType", caseEdocFile.FileType));
                    base.Parameter.Add(new CommandParameter("@FileName", caseEdocFile.FileName));
                    base.Parameter.Add(new CommandParameter("@SendNo", caseEdocFile.SendNo));
                    base.Parameter.Add(new CommandParameter("@FileObject", caseEdocFile.FileObject, SqlDbType.VarBinary, 0));
                    base.ExecuteNonQuery(sql);
                }
            }
            catch (Exception ex)
            {
                WriteLog(string.Format("插入CaseEdocFile 資料庫發生錯誤, ID : {0} 訊息: {1}", caseDeath.DocNo, ex.Message.ToString()));
                result = "1";
            }
            return result;
        }

        internal void updateStatus(CaseDeadVersion caseDeath, string SendStatus, string SendMessage="")
        {
            //string sql = @"SELECT * FROM [dbo].[CaseDeathLoan] where CaseNO=@CaseNO";
            string sql = @"update [CaseDeadVersion] set SendStatus=@SendStatus, SendMessage=@SendMessage where DocNo=@DocNo";
            try
            {
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@DocNo", caseDeath.DocNo));
                base.Parameter.Add(new CommandParameter("@SendMessage", SendMessage));                
                base.Parameter.Add(new CommandParameter("@SendStatus", SendStatus));
                base.ExecuteNonQuery(sql);                
            }
            catch (Exception ex)
            {
                WriteLog(string.Format("更新CaseDeadVersion 資料庫狀態發生錯誤, ID : {0} 訊息: {1}", caseDeath.DocNo, ex.Message.ToString()));                
            }
        }

        internal void deleteBR(string DocNo)
        {
            string sql = @"DELETE FROM [dbo].[CaseDeathLoan] WHERE CaseNo=@DocNo";
            try
            {
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@DocNo", DocNo));
                base.ExecuteNonQuery(sql);
            }
            catch (Exception ex)
            {
                WriteLog(string.Format("刪除CaseDeathLoan 資料庫狀態發生錯誤, ID : {0} 訊息: {1}", DocNo, ex.Message.ToString()));
            }
        }

        internal void deleteIn10(string DocNo)
        {
            string sql = @"DELETE FROM [dbo].[CaseDeathDeposit] WHERE CaseNo=@DocNo";
            try
            {
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@DocNo", DocNo));
                base.ExecuteNonQuery(sql);
            }
            catch (Exception ex)
            {
                WriteLog(string.Format("刪除CaseDeathDeposit 資料庫狀態發生錯誤, ID : {0} 訊息: {1}", DocNo, ex.Message.ToString()));
            }
        }

        internal void deleteIn11(string DocNo)
        {
            string sql = @"DELETE FROM [dbo].[CaseDeathInvestment] WHERE CaseNo=@DocNo";
            try
            {
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@DocNo", DocNo));
                base.ExecuteNonQuery(sql);
            }
            catch (Exception ex)
            {
                WriteLog(string.Format("刪除CaseDeathInvestment 資料庫狀態發生錯誤, ID : {0} 訊息: {1}", DocNo, ex.Message.ToString()));
            }
        }

        internal bool checkFileLength(CaseDeadVersion caseDeath, DateTime lastWorkDay, string baseDir, string prefix, int TotalLen)
        {
            try
            {
                string fullName = Path.Combine(baseDir, prefix + lastWorkDay.ToString("yyyyMMdd") + ".txt");
                if (File.Exists(fullName))
                {
                    string[] lines = File.ReadAllLines(fullName, Encoding.Default);

                    WriteLog(string.Format("\t檢查檔案 {0} 的長度 ", fullName));

                    
                    bool isPass = true;
                    foreach (string line in lines)
                    {
                        if (string.IsNullOrEmpty(line) || line.Length < 10)
                            continue;

                        if (getLength(line) == TotalLen)
                        {

                        }
                        else
                        {
                            isPass = isPass & false;
                            WriteLog(string.Format("\t\t行 {0} , 長度不一致", line.Substring(0,10) ));
                        };
                    }
                    if (isPass)
                        return true;
                    else
                        return false;
                }
                else
                {
                    WriteLog(string.Format("\t檔案不存在 {0} ", fullName));
                    return false;
                }
            }
            catch (Exception ex)
            {
                WriteLog(string.Format("發生錯誤,  訊息: {0}", ex.Message.ToString()));
                return false;
            }
            return true;
        }

        internal void setCaseDeadVersionStatus(CaseDeadVersion caseDeath,string Status)
        {
            string sql = @"Update [dbo].[CaseDeadVersion] set [Status]=@Status WHERE [CaseTrsNewID]=@CaseId";
            try
            {
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@Status", Status));
                base.Parameter.Add(new CommandParameter("@CaseId", caseDeath.CaseTrsNewID));
                base.ExecuteNonQuery(sql);
            }
            catch (Exception ex)
            {
                WriteLog(string.Format("更新CaseDeadVersion 資料庫狀態發生錯誤, ID : {0} 訊息: {1}", caseDeath.DocNo, ex.Message.ToString()));
            }
        }

        internal void updateCaseDeadVersion(string CaseNo)
        {
            try
            {
                List<CaseDeadVersion> allDead = new List<CaseDeadVersion>();
                List<CaseDeadDetail> CaseDeadDetailList = new List<CaseDeadDetail>();
                using (IDbConnection dbConnection = OpenConnection())
                {
                    string sqlStr = @"SELECT * FROM CaseDeadVersion WHERE DocNo=@CaseNo order by Seq";
                    base.Parameter.Clear();
                    base.Parameter.Add(new CommandParameter("@CaseNo", CaseNo));
                    allDead = base.SearchList<CaseDeadVersion>(sqlStr).ToList();

                    sqlStr = @"SELECT * FROM CaseDeadDetail WHERE CaseNo=@CaseNo";
                    base.Parameter.Clear();
                    base.Parameter.Add(new CommandParameter("@CaseNo", CaseNo));
                    CaseDeadDetailList = base.SearchList<CaseDeadDetail>(sqlStr).ToList();
                };

                foreach (var cdv in allDead)
                {
                    List<CaseDeadDetail> CaseDeadDetailListByID = CaseDeadDetailList.Where(x => x.DOC_ID == cdv.HeirId).ToList();
                    updateCaseDeadVersion(cdv, CaseDeadDetailListByID);
                }

            }
            catch (Exception ex)
            {
                WriteLog("Error " + ex.Message.ToString());                
            }

        }

        internal void updateCaseDeadVersion(CaseDeadVersion cdv, List<CaseDeadDetail> CaseDeadDetailList)
        {
            // 存回死亡回饋檔的W-AA欄
            // 分別為[deposit] [Loan]  [LoanMgr] [IsMatch] [Box]
            string isDeposit = "N";
            string isLoan = "N";
            string isLoanMgr = "N";
            string isMatch = "N";
            string isBox = "N";
            using (IDbConnection dbConnection = OpenConnection())
            {
                string sqlStr = @"SELECT COUNT(*) FROM CaseDeathDeposit WHERE CaseNo=@CaseNo AND ID_NO=@HeirId";
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@CaseNo", cdv.DocNo));
                base.Parameter.Add(new CommandParameter("@HeirId", cdv.HeirId));
                var result = base.Search(sqlStr);
                if (result.Rows.Count > 0)
                    isDeposit = int.Parse(result.Rows[0][0].ToString()) > 0 ? "Y" : "N";
            };

            using (IDbConnection dbConnection = OpenConnection())
            {
                string sqlStr = @"SELECT COUNT(*) FROM CaseDeathLoan WHERE CaseNo=@CaseNo AND IDHIS_IDNO_ACNO=@HeirId AND PROD_TYPE<>'信用卡' ";
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@CaseNo", cdv.DocNo));
                base.Parameter.Add(new CommandParameter("@HeirId", cdv.HeirId));
                var result = base.Search(sqlStr);
                if (result.Rows.Count > 0)
                    isLoan = int.Parse(result.Rows[0][0].ToString()) > 0 ? "Y" : "N";
            };

            using (IDbConnection dbConnection = OpenConnection())
            {
                string sqlStr = @"SELECT COUNT(*) FROM CaseDeathInvestment WHERE CaseNo=@CaseNo AND ID_NO=@HeirId";
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@CaseNo", cdv.DocNo));
                base.Parameter.Add(new CommandParameter("@HeirId", cdv.HeirId));
                var result = base.Search(sqlStr);
                if (result.Rows.Count > 0)
                    isLoanMgr = int.Parse(result.Rows[0][0].ToString()) > 0 ? "Y" : "N";
            };

            using (IDbConnection dbConnection = OpenConnection())
            {
                string sqlStr = @"SELECT * FROM CaseDeadDetail WHERE CaseNo=@CaseNo AND DOC_ID=@HeirId";
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@CaseNo", cdv.DocNo));
                base.Parameter.Add(new CommandParameter("@HeirId", cdv.HeirId));
                var result = base.SearchList<CaseDeadDetail>(sqlStr).ToList();


                //保險箱.. 是若有任何一個重號有Y, 就是Y...
                isBox = result.Any(x => !string.IsNullOrEmpty(x.IS_BOX) && x.IS_BOX == "Y") ? "Y" : "N";

                // 戶名不符, 是指若有任一個戶名不符的.. 就是Y
                isMatch = result.Any(x => x.TX60628_Message == "戶名不符" ) ? "Y" : "N";
            };

            using (IDbConnection dbConnection = OpenConnection())
            {
                string sqlStr = @"Update CaseDeadVersion  SET deposit=@isDeposit, Loan=@isLoan, LoanMgr=@isLoanMgr, IsMatch=@isMatch, Box=@isBox WHERE  [NewID]=@NewID";
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@NewID", cdv.NewID));                
                base.Parameter.Add(new CommandParameter("@isDeposit", isDeposit));
                base.Parameter.Add(new CommandParameter("@isLoan", isLoan));
                base.Parameter.Add(new CommandParameter("@isLoanMgr", isLoanMgr));
                base.Parameter.Add(new CommandParameter("@isMatch", isMatch));
                base.Parameter.Add(new CommandParameter("@isBox", isBox));
                base.ExecuteNonQuery(sqlStr);
            };

            WriteLog(string.Format("\t\t 亡者{0}  [deposit] [Loan]  [LoanMgr] [IsMatch] [Box] 狀態  {1} {2} {3} {4} {5} ", cdv.HeirId + " / " + cdv.HeirName, isDeposit, isLoan, isLoanMgr, isMatch, isBox));

        }
    }
}
