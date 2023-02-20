using CTBC.CSFS.Models;
using CTBC.CSFS.Pattern;
using CTBC.FrameWork.HTG;
using log4net;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTBC.WinExe.DeathQuerySetting
{
    public class DeathQueryBiz : BaseBusinessRule
    {
        /// <summary>
        /// 讀取那一個CaseDeadVersion 案件, 要查詢 status=00的     
        /// </summary>
        /// <returns></returns>
        public List<CaseDeadVersion> getDeathCase()
        {
            List<CaseDeadVersion> result = new List<CaseDeadVersion>();
            using (IDbConnection dbConnection = OpenConnection())
            {
                string sqlStr = @"SELECT * FROM [CaseDeadVersion] WHERE SetStatus='0' ";
                result = base.SearchList<CaseDeadVersion>(sqlStr).ToList();
            };

            return result;
        }

        /// <summary>
        /// 20211208, 發生連續2個小時內執行, 發現第一批次未執行完, 第二次啟動, 會把第一次的結果砍掉, 所以設置, 若啟動時, 有SetStatus狀態為99, 表示
        /// 目前正在執行中...., 所以就先不執行.
        /// </summary>
        /// <returns></returns>
        public int setDeathCaseRunning()
        {
            int ret = 0;
            using (IDbConnection dbConnection = OpenConnection())
            {
                string sqlStr = @"UPDATE [CaseDeadVersion] SET SetStatus='99' WHERE SetStatus='0'  ";
                ret = base.ExecuteNonQuery(sqlStr);                
            };
            return ret;
        }


        public int isRunning()
        {
            
            List<CaseDeadVersion> result = new List<CaseDeadVersion>();
            using (IDbConnection dbConnection = OpenConnection())
            {
                string sqlStr = @"SELECT * FROM [CaseDeadVersion] WHERE SetStatus='99' ";
                result = base.SearchList<CaseDeadVersion>(sqlStr).ToList();
            };
            return result.Count();
        }
        /// <summary>
        /// 發查電文.. 
        /// </summary>
        /// <param name="cdv"></param>
        /// <returns></returns>
        internal List<CaseDeadDetail> doQueryDeadDetail(ExecuteHTG objSeiHTG, CaseDeadVersion cdv)
        {
            List<CaseDeadDetail> result = new List<CaseDeadDetail>();

            // Step 1, 發查60628 , 找到所有的重號
            string mess = string.Empty;


            var doubleIDs = getDoubleId(objSeiHTG, cdv.HeirId.Trim(), cdv.NewID, ref mess);

            if( !string.IsNullOrEmpty(mess))
            {
                #region 表示發查電文抓敗....
                // 
                CaseDeadDetail caseDetail = new CaseDeadDetail();
                caseDetail.CaseDeadNewID = cdv.CaseTrsNewID.ToString();
                caseDetail.CaseNo = cdv.DocNo;
                caseDetail.DATA_DATE = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                caseDetail.DOC_ID = cdv.HeirId;
                caseDetail.DOC_NAME = cdv.HeirName;
                caseDetail.IS_DUP = "N";
                caseDetail.CDBC_ID = "CIF無資料";
                caseDetail.CDBC_NAME = "CIF無資料";
                caseDetail.CDBC_Birthday = new DateTime(1900, 1, 1);
                caseDetail.IS_HAVE = "CIF無資料";
                caseDetail.IS_BOX = "CIF無資料";
                caseDetail.TX60628_Message = mess;
                caseDetail.TX60628_STATUS = "3";
                caseDetail.TX67050_Message = "CIF無資料";
                caseDetail.TX67050_STATUS = "2";
                caseDetail.Account = "CIF無資料";
                caseDetail.AccountStatus = "CIF無資料";
                caseDetail.PROD_CODE = "CIF無資料";
                caseDetail.TX9091_Message = "CIF無資料";
                caseDetail.TX9091_STATUS = "2";
                #endregion
                result.Add(caseDetail);
                return result;
            }

            if (doubleIDs.Count() == 0)
            {
                #region 表示沒有往來....
                // 
                CaseDeadDetail caseDetail = new CaseDeadDetail();
                caseDetail.CaseDeadNewID = cdv.CaseTrsNewID.ToString();
                caseDetail.CaseNo = cdv.DocNo;
                caseDetail.DATA_DATE = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                caseDetail.DOC_ID = cdv.HeirId;
                caseDetail.DOC_NAME = cdv.HeirName;
                caseDetail.IS_DUP = "N";
                caseDetail.CDBC_ID = "CIF無資料";
                caseDetail.CDBC_NAME = "CIF無資料";
                caseDetail.CDBC_Birthday = new DateTime(1900, 1, 1);
                caseDetail.IS_HAVE = "CIF無資料";
                caseDetail.IS_BOX = "CIF無資料";
                caseDetail.TX60628_Message = "CIF無資料";
                caseDetail.TX60628_STATUS = "3";
                caseDetail.TX67050_Message = "CIF無往來";
                caseDetail.TX67050_STATUS = "2";
                caseDetail.Account = "CIF無資料";
                caseDetail.AccountStatus = "CIF無資料";
                caseDetail.PROD_CODE = "CIF無資料";
                caseDetail.TX9091_Message = "CIF無往來";
                caseDetail.TX9091_STATUS = "2";
                #endregion
                result.Add(caseDetail);
            }

            foreach (var d in doubleIDs)
            {
                try
                {
                    CaseDeadDetail caseDetail = new CaseDeadDetail();
                    caseDetail.CaseDeadNewID = cdv.CaseTrsNewID.ToString();
                    caseDetail.CaseNo = cdv.DocNo;
                    caseDetail.DATA_DATE = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                    caseDetail.DOC_ID = cdv.HeirId;
                    caseDetail.DOC_NAME = cdv.HeirName;
                    caseDetail.IS_DUP = doubleIDs.Count() > 1 ? "Y" : "N";
                    caseDetail.CDBC_ID = d.ID;
                    caseDetail.CDBC_NAME = d.Name;
                    DateTime dt = new DateTime(1900, 1, 1);
                    if (!string.IsNullOrEmpty(d.Birthday))
                    {
                        dt = new DateTime(int.Parse(d.Birthday.Substring(4)), int.Parse(d.Birthday.Substring(2, 2)), int.Parse(d.Birthday.Substring(0, 2)));
                    }
                    caseDetail.CDBC_Birthday = dt;
                    caseDetail.IS_HAVE = d.ACMark;
                    caseDetail.TX60628_Message = d.Message;
                    caseDetail.TX60628_STATUS = "1";
                    result.Add(caseDetail);
                }
                catch (Exception ex)
                {

                    WriteLog("\t\t打60628發生錯誤 " + ex.Message.ToString());
                }
            }

            return result;
        }



        internal List<TX60629Model> getDoubleId(ExecuteHTG objHTG, string Id, Guid NewID, ref string Message)
        {
            List<TX60629Model> resultSet = new List<TX60629Model>();
            string result = objHTG.HTGMainFun("60629", Id, "", NewID.ToString());
            if (result.StartsWith("0000"))
            {
                string trnnum = result.Split('|')[1].Trim();

                {
                    //string strAmount2 = " " + SeizureAmount.ToString("#,#.00");
                    string _trn = result.Replace("0000|", "");

                    string sql = "SELECT TOP 1 * FROM TX_60629 WHERE TRNNUM='{0}' AND CASEID='{1}'  ORDER BY SNO DESC";
                    string formsql = string.Format(sql, _trn, NewID.ToString());

                    DataTable gCase = base.Search(formsql);

                    if (gCase == null)
                        result = "0001|扣押失敗(原因450-31查不到該筆)";
                    else
                    {
                        if (gCase.Rows.Count > 0)
                        {
                            for (int i = 1; i < 13; i++)
                            {
                                string fieldname1 = "ID_DATA_" + i.ToString();
                                string fieldname2 = "CUST_NAME_" + i.ToString();
                                string fieldname3 = "AC_MARK_" + i.ToString();
                                string fieldname4 = "BIRTH_DT_" + i.ToString();
                                if (!string.IsNullOrEmpty(gCase.Rows[0][fieldname1].ToString()))
                                {
                                    var model = new TX60629Model()
                                    {
                                        ID = gCase.Rows[0][fieldname1].ToString(),
                                        Name = gCase.Rows[0][fieldname2].ToString(),
                                        ACMark = gCase.Rows[0][fieldname3].ToString(),
                                        Birthday = gCase.Rows[0][fieldname4].ToString(),
                                        Message = gCase.Rows[0]["RepMessage"].ToString()
                                    };
                                    resultSet.Add(model);
                                }

                            }
                        }
                    }

                }


            }
            else
            {
                Message = result.Split('|')[1];                
            }
            return resultSet;
        }



        /// <summary>
        /// TX60628存放款帳號欄為Y之戶名是否與EXCEL被繼承人相符
        /// 若不一致, 再檢查EXCEL被繼人出生日期是否相同
        /// </summary>
        /// <param name="cdv"></param>
        /// <param name="CaseDeadDetailList"></param>
        /// <returns></returns>
        internal void checkSameName(CaseDeadVersion cdv, List<CaseDeadDetail> CaseDeadDetailList)
        {
            

            foreach (var x in CaseDeadDetailList.Where(x=>x.CDBC_NAME!="CIF無資料"))
            {
                bool output = true;

                if (x.CDBC_NAME != cdv.HeirName)
                {
                    x.TX60628_Message = "戶名不符";
                    x.TX60628_STATUS = "2";
                }

                output = x.CDBC_NAME == cdv.HeirName ? output & true : output & false;
                if (!output) // 若不符, 才去比對生日
                {
                    DateTime dt = new DateTime(1800,1,1);
                    output = true;
                    // 也要判是否為負值                    
                    if( cdv.HeirBirthDay.StartsWith("-"))
                    {
                        int minusYear = int.Parse(cdv.HeirBirthDay.Substring(1, 2));
                        string hbd = cdv.HeirBirthDay;
                        dt = new DateTime(1911 - minusYear, int.Parse(hbd.Substring(3, 2)), int.Parse(hbd.Substring(5, 2)));
                    }
                    else
                    {
                        string hbd = cdv.HeirBirthDay.PadLeft(7, '0');                    
                        dt = new DateTime(int.Parse(hbd.Substring(0, 3)) + 1911 , int.Parse(hbd.Substring(3, 2)), int.Parse(hbd.Substring(5, 2)));
                    }
                    output = x.CDBC_Birthday == dt ? output & true : output & false;
                }

                if (output)
                {
                    x.TX9091_STATUS = "1";
                    WriteLog(string.Format("\t\t\t\t\t 重號{0} / {1} 戶名正確  ", x.CDBC_ID , x.CDBC_NAME));
                }
                else
                {
                    x.TX9091_STATUS = "2";
                    x.TX9091_Message = "戶名及生日不符";
                    x.TX67050_STATUS = "2";
                    x.TX67050_Message = "戶名及生日不符";
                    WriteLog(string.Format("\t\t\t\t\t 重號{0} / {1} 戶名不一致  ", x.CDBC_ID, x.CDBC_NAME));
                }
            }
        }

        /// <summary>
        /// 打60491, 排除TX9091_STATUS<>1
        /// </summary>
        /// <param name="cdv"></param>
        /// <param name="CaseDeadDetailByID"></param>
        /// <returns></returns>
        internal List<CaseDeadDetail> getAccounts(ExecuteHTG objSeiHTG, CaseDeadVersion cdv, List<CaseDeadDetail> lstCaseDeadDetail)
        {
            List<CaseDeadDetail> output = new List<CaseDeadDetail>();

            // 若是TX9091_Status <> '01', 表示戶名有錯. .也要寫一筆進去
            foreach (var cdd in lstCaseDeadDetail.Where(x => x.TX9091_STATUS != "1"))
            {
                output.Add(cdd);
            }  


            foreach (var cdd in lstCaseDeadDetail.Where(x => x.TX9091_STATUS == "1" ))
            {
                string sBoxFlag = string.Empty;
                var accts = getAccountByID(objSeiHTG, cdd.CDBC_ID, cdd.CaseDeadNewID, ref sBoxFlag);
                WriteLog(string.Format("\t\t\t\t\t 帳號{0} , 共有{1}個帳號", cdd.CDBC_ID, accts.Count().ToString()));
                accts.ForEach(x =>
                {
                    CaseDeadDetail o = new CaseDeadDetail() { TX60628_STATUS=cdd.TX60628_STATUS, TX60628_Message=cdd.TX60628_Message, CaseDeadNewID = cdd.CaseDeadNewID, DATA_DATE = cdd.DATA_DATE, DOC_ID = cdd.DOC_ID, DOC_NAME = cdd.DOC_NAME, IS_DUP = cdd.IS_DUP, IS_HAVE = cdd.IS_HAVE, IS_BOX = sBoxFlag, CDBC_ID = cdd.CDBC_ID, CDBC_NAME = cdd.CDBC_NAME, CDBC_Birthday = cdd.CDBC_Birthday };
                    o.TX60490_STATUS = "1";
                    o.TX60490_Message = "";
                    o.Account = x.Account;
                    o.CaseNo = cdv.DocNo;
                    o.Ccy = x.Ccy;
                    o.AccountStatus = x.AccountStatus;
                    o.PROD_CODE = x.ProdDesc;                    
                    output.Add(o);
                });



                
                if( accts.Count()==0) // 若60628說有存放款, 但實際打60491沒有時, 也要新增一筆
                {
                    cdd.TX9091_STATUS = "2";
                    cdd.TX9091_Message = " ";

                    /// 20210409, 發現有案例是無存放款, 但還是要保管箱的資料.. 可能要改在這裏...
                    /// 因為, 搞不到這類案件, 所以先不改... 特此注記... 應該是補以下一行
                    cdd.IS_BOX = sBoxFlag;
                    output.Add(cdd);
                }

            }
            // 若是無存款款的重號, 要也加一筆
            foreach (var cdd in lstCaseDeadDetail.Where(x => x.IS_HAVE == "N" && x.TX9091_STATUS=="1"))
            {
                cdd.TX9091_STATUS = "2";
                cdd.TX9091_Message = "無存放款";
                output.Add(cdd);
            }
         

            return output;
        }


        internal List<ObligorAccount> getAccountByID(ExecuteHTG objSeiHTG, string ObligorNo, string caseid, ref string sBoxflag)
        {

            string ret = objSeiHTG.HTGMainFun("60491N", ObligorNo, "", caseid.ToString());
            List<ObligorAccount> Result = new List<ObligorAccount>();
            if (ret.StartsWith("0000"))
            {
                string _name = null;
                string _trn = ret.Replace("0000|", "");


                using (IDbConnection dbConnection = OpenConnection())
                {
                    string grp = "SELECT * FROM TX_60491_Grp WHERE TRNNUM='{0}'  AND CASEID='{1}'  ";
                    string sql2 = string.Format(grp, _trn, caseid.ToString());
                    DataTable grpdt = base.Search(sql2);
                    if (grpdt != null)
                    {
                        sBoxflag = grpdt.Rows[0]["SboxFlag"].ToString();
                    }

                    string sql = "SELECT * FROM TX_60491_Detl a inner join TX_60491_Grp b on a.fksno=b.sno WHERE b.TRNNUM='{0}'  AND b.CASEID='{1}'   ";
                    string formsql = string.Format(sql, _trn, caseid.ToString());
                    DataTable gCase = base.Search(formsql);

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
                                AccountStatus = dr["StsDesc"].ToString(),
                                ProdDesc = dr["ProdDesc"].ToString(),
                                Link = dr["Link"].ToString(),
                                SegmentCode = dr["SegmentCode"].ToString(),
                                StsDesc = dr["StsDesc"].ToString().Trim()
                            };
                            Result.Add(n);
                        }
                    }
                };

            }
            else
            {
                WriteLog(string.Format("\t\t\t\t\t 發送電文60491失敗, ID: {0} ,  錯誤訊息{1}", ObligorNo, ret ));
            }
            return Result;
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

        internal int insertCaseDeadDetail(List<CaseDeadDetail> Result)
        {

            try
            {
               
                using (SqlConnection dbConnection = OpenConnection())
                {
                    using (IDbTransaction tran = dbConnection.BeginTransaction())
                    {
                        foreach (var d in Result)
                        {
                            try
                            {


                                #region MyRegion
                                d.BALANCE = d.BALANCE == null ? "0" : d.BALANCE.ToString();
                                string sql = @"insert into CaseDeadDetail ([CaseDeadNewID], [TrnNum], [DATA_DATE], [DOC_ID], [DOC_NAME], [IS_DUP], [CDBC_ID], [CDBC_NAME], [IS_HAVE], [IS_BOX], [TX67050_STATUS], [TX67050_Message], [TX60628_STATUS], [TX60628_Message], [TX60490_STATUS], [TX60490_Message], [TX9091_STATUS], [TX9091_Message], [Account], [Ccy], [AccountStatus], [PROD_CODE], [STATUS], [REVERSE], [REMARK], [BALANCE], [TYPE],[CaseNo],BRCI_STATUS) 
                                values (@CaseDeadNewID, @TrnNum, @DATA_DATE, @DOC_ID, @DOC_NAME, @IS_DUP, @CDBC_ID, @CDBC_NAME, @IS_HAVE, @IS_BOX, @TX67050_STATUS, @TX67050_Message, @TX60628_STATUS, @TX60628_Message, @TX60490_STATUS, @TX60490_Message, @TX9091_STATUS, @TX9091_Message, @Account,@Ccy, @AccountStatus, @PROD_CODE, @STATUS, @REVERSE, @REMARK, @BALANCE, @TYPE,@CaseNo,'F') ";
                                //sql += string.Format(" Values ('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}','{12}','{13}','{14}','{15}','{16}','{17}','{18}','{19}','{20}','{21}','{22}','{23}','{24}','{25}')", "6acb8909-68bf-4d17-b2eb-bba60117f377", null, "2020-12-02", "B101063165", "測試祖另外請記住不要勾選查詢印表機並自", "Y", "B1010631651", "海有愁日號                 X S V", "Y", "", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null,null);

                                base.Parameter.Clear();
                                base.Parameter.Add(new CommandParameter("@CaseDeadNewID", d.CaseDeadNewID));
                                base.Parameter.Add(new CommandParameter("@TrnNum", d.TrnNum));
                                base.Parameter.Add(new CommandParameter("@DATA_DATE", d.DATA_DATE));
                                base.Parameter.Add(new CommandParameter("@DOC_ID", d.DOC_ID));
                                base.Parameter.Add(new CommandParameter("@DOC_NAME", d.DOC_NAME));
                                base.Parameter.Add(new CommandParameter("@IS_DUP", d.IS_DUP));
                                base.Parameter.Add(new CommandParameter("@CDBC_ID", d.CDBC_ID));
                                base.Parameter.Add(new CommandParameter("@CDBC_NAME", d.CDBC_NAME));
                                base.Parameter.Add(new CommandParameter("@IS_HAVE", d.IS_HAVE));
                                base.Parameter.Add(new CommandParameter("@IS_BOX", d.IS_BOX));
                                base.Parameter.Add(new CommandParameter("@TX67050_STATUS", d.TX67050_STATUS));
                                base.Parameter.Add(new CommandParameter("@TX67050_Message", d.TX67050_Message));
                                base.Parameter.Add(new CommandParameter("@TX60628_STATUS", d.TX60628_STATUS));
                                base.Parameter.Add(new CommandParameter("@TX60628_Message", d.TX60628_Message));
                                base.Parameter.Add(new CommandParameter("@TX60490_STATUS", d.TX60490_STATUS));
                                base.Parameter.Add(new CommandParameter("@TX60490_Message", d.TX60490_Message));
                                base.Parameter.Add(new CommandParameter("@TX9091_STATUS", d.TX9091_STATUS));
                                base.Parameter.Add(new CommandParameter("@TX9091_Message", d.TX9091_Message));
                                base.Parameter.Add(new CommandParameter("@Account", d.Account));
                                base.Parameter.Add(new CommandParameter("@Ccy", d.Ccy));
                                base.Parameter.Add(new CommandParameter("@AccountStatus", d.AccountStatus));
                                base.Parameter.Add(new CommandParameter("@PROD_CODE", d.PROD_CODE));
                                base.Parameter.Add(new CommandParameter("@STATUS", d.STATUS));
                                base.Parameter.Add(new CommandParameter("@REVERSE", d.REVERSE));
                                base.Parameter.Add(new CommandParameter("@REMARK", d.REMARK));
                                base.Parameter.Add(new CommandParameter("@BALANCE", d.BALANCE));
                                base.Parameter.Add(new CommandParameter("@TYPE", d.TYPE));
                                base.Parameter.Add(new CommandParameter("@CaseNo", d.CaseNo));
                                #endregion

                                //base.ExecuteNonQuery(sql);
                                base.ExecuteNonQuery(sql, tran);

  
                            }
                            catch (Exception ex)
                            {
                                WriteLog("\t\t新增CaseDeadDetail 222 發生錯誤 " + ex.Message.ToString());
                                //tran.Rollback();
                            }
                        }
                        tran.Commit();
                    };
                };
            }
            catch (Exception ex)
            {

                WriteLog("\t\t新增CaseDeadDetail 發生錯誤 " + ex.Message.ToString());
            }

            return 1;
        }

        internal List<string> doSetting9091(ExecuteHTG objSeiHTG, List<CaseDeadDetail> CaseDeadDetailList)
        {
            List<string> errorList = new List<string>();
            // 以下.. 直接回寫成功
            // 1. 帳號之「產品型態：現金卡」或「狀態：已結清」無須設定死亡，回寫成功
            //      [CaseDeadDetail] where   AccountStatus='結清' or PROD_CODE='現金卡'        
            // 2. 9091顯示凍結碼重複回寫成功, 要試著打打看, 才知
            //      ret = "0001|電文09091 發查失敗 電文訊息 :此事故代號或凍結碼重複設定" .. 則視為成功
            //  
            // 3. 9091顯示綜定存…回寫成功, 要試著打打看, 才知
            //      [看回應碼]               
            foreach (var acc in CaseDeadDetailList)
            {
                if (string.IsNullOrEmpty(acc.Account) || acc.Account=="CIF無資料") // 若是沒有Account 則, 還是不打9091
                    continue;
                // 1. 帳號之「產品型態：現金卡」或「狀態：已結清」無須設定死亡，回寫成功
                //      [CaseDeadDetail] where   AccountStatus='結清' or PROD_CODE='現金卡'        
                if (acc.AccountStatus.Trim().Equals("結清") || acc.PROD_CODE.Trim().Equals("現金卡"))
                {
                    acc.TX9091_STATUS = "1";
                    acc.TX9091_Message = "成功";
                    WriteLog(string.Format("\t\t\t\t\t 帳號{0} 設定成功  ", acc.Account));
                    continue;
                }
                // 2. 其餘的帳戶, 都要打電文9091-03 .. 備註要填.. 發查電文之備註：亡外(固定值)+案件編號(刪C)，例如：亡外109101800001
                try
                {
                    string memo = "亡外" + acc.CaseNo.Substring(1);
                    var ret = objSeiHTG.Send9091(acc.Account, acc.Ccy, "3", memo, acc.CDBC_ID, acc.CaseDeadNewID);
                    if (ret.StartsWith("0000") || ret.EndsWith("此事故代號或凍結碼重複設定") || ret.Contains("綜定存") || ret.Contains("結清") || ret.Contains("凍結碼重複設定"))
                    {
                        acc.TX9091_STATUS = "1";
                        acc.TX9091_Message = "成功";
                        WriteLog(string.Format("\t\t\t\t\t 帳號{0} 設定成功  ", acc.Account));
                    }
                    else
                    {
                        acc.TX9091_STATUS = "2";
                        string[] sp = ret.Split('|');
                        acc.TX9091_Message = sp[1];
                        errorList.Add(string.Format("{0}/{1}", acc.Account, sp[1]));
                        WriteLog(string.Format("\t\t\t\t\t 帳號{0} 設定失敗 原因{1}  ", acc.Account, sp[1]));
                    }
                }
                catch (Exception ex)
                {
                    acc.TX9091_STATUS = "2";
                    acc.TX9091_Message = ex.Message.ToString();
                    errorList.Add(string.Format("{0}/{1}", acc.Account, ex.Message.ToString()));

                    WriteLog("\t\t打9091發生錯誤 " + ex.Message.ToString());
                }


            }
            return errorList;
        }

        internal List<string> doSetting67101(ExecuteHTG objSeiHTG, List<CaseDeadDetail> CaseDeadDetailList)
        {
            // 在打67050-5 後, 會有 "DEAD_FLAG", 要上傳...
            //                      INCOME_AMT, 年收
            //                      ASSET_SOURCE_8,  收入及資產主要來源選擇==> 其他
            //                        <data id="GNDR" value="1"/>
            //                      <data id="MRTL_STA" value="1"/>
            //

            if (CaseDeadDetailList.Count() <= 0)
                return null;

            // Step 1, 要從CaseDeadDetailList中, 找到所有的ID, 包括重號
            var ids = CaseDeadDetailList.Where(x=>x.TX67050_STATUS!="2").GroupBy(x => x.CDBC_ID).Select(x => x.Key).ToList();
            var caseid = CaseDeadDetailList.First().CaseDeadNewID;
            foreach (var id in ids)
            {
                // Step 2. 逐一去打67101 ..
                try
                {
                    var ret = objSeiHTG.Send67101(id, caseid);
                    if (ret.StartsWith("0000|"))
                    {
                        CaseDeadDetailList.Where(x => x.CDBC_ID == id).ToList().ForEach(x => x.TX67050_STATUS = "1");
                        WriteLog(string.Format("\t\t\t\t\t 死亡註記{0} 設定成功  ", id));
                    }
                    else
                    {
                        CaseDeadDetailList.Where(x => x.CDBC_ID == id).ToList().ForEach(x => { x.TX67050_STATUS = "2"; x.TX67050_Message = ret.Replace("0000|", ""); });
                        WriteLog(string.Format("\t\t\t\t\t 死亡註記{0} 設定失敗 , 原因{1}  ", id, ret));
                    }
                }
                catch (Exception ex)
                {

                    WriteLog("\t\t打67101發生錯誤 " + ex.Message.ToString());
                }
            }




            return null;
        }

        internal void updateCaseDeadVersionStatus(string CaseNo, string Status)
        {
            try
            {
                using (IDbConnection dbConnection = OpenConnection())
                {

                    string sql = @"Update CaseDeadVersion Set Status = @Status WHERE DocNo=@DocNo";
                    base.Parameter.Clear();
                    base.Parameter.Add(new CommandParameter("@Status", Status));
                    base.Parameter.Add(new CommandParameter("@DocNo", CaseNo));
                    base.ExecuteNonQuery(sql);
                }
            }
            catch (Exception ex)
            {
                WriteLog("\t\t更新 CaseDeadVersion 失敗1 " + ex.Message.ToString());
            }
        }

        internal void updateCaseDeadVersionSetStatus(string CaseNo, string Id, string Status)
        {
            try
            {
                using (IDbConnection dbConnection = OpenConnection())
                {

                    string sql = @"Update CaseDeadVersion Set SetStatus = @Status WHERE DocNo=@DocNo AND HeirId=@Id";
                    base.Parameter.Clear();
                    base.Parameter.Add(new CommandParameter("@Status", Status));
                    base.Parameter.Add(new CommandParameter("@DocNo", CaseNo));
                    base.Parameter.Add(new CommandParameter("@Id", Id));
                    base.ExecuteNonQuery(sql);
                }
            }
            catch (Exception ex)
            {
                WriteLog("\t\t更新 CaseDeadVersion 失敗2 " + ex.Message.ToString());
            }
        }


        public static void WriteLog(string msg)
        {
            if (Directory.Exists(@".\Log") == false)
            {
                Directory.CreateDirectory(@".\Log");
            }
            LogManager.Exists("DebugLog").Debug(msg);
        }


        // 檢查案件中, 是否有成功的 若成功, 修改CaseDeadVersion 的SetStatus , 改為2
        //                          若全部失敗 才押CaseDeadVersion 的SetStatus , 改為3
        internal void updateSetStatus(string CaseNo, string HeirID)
        {
            try
            {
                using (IDbConnection dbConnection = OpenConnection())
                {

                    string sql = @"Select * from  CaseDeadDetail WHERE CDBC_ID like '%" +HeirID.Trim() + "%' AND CaseNo=@CaseNo AND TX67050_STATUS='1' ";
                    base.Parameter.Clear();
                    base.Parameter.Add(new CommandParameter("@CaseNo", CaseNo));
                    base.Parameter.Add(new CommandParameter("@Id", HeirID));
                    var result = base.SearchList<CaseDeadDetail>(sql);

                    if( result.Count()>0) // 如果有一筆, 代表都成功, 則CaseDeadVersion.SetStatus 押2
                    {
                        updateCaseDeadVersionSetStatus(CaseNo, HeirID, "2");                        
                    }
                    else
                    {
                        updateCaseDeadVersionSetStatus(CaseNo, HeirID, "3");                        
                    }
                }
            }
            catch (Exception ex)
            {
                WriteLog("\t\t更新 CaseDeadVersion 失敗3 " + ex.Message.ToString());
            }
        }



        internal void deleteCaseDeadDetailByCaseNo(string CaseNo)
        {
            try
            {
                using (IDbConnection dbConnection = OpenConnection())
                {
                    string sql = @"Delete from  CaseDeadDetail WHERE [CaseNo]=@CaseNo";
                    base.Parameter.Clear();
                    base.Parameter.Add(new CommandParameter("@CaseNo", CaseNo));
                    var ret = base.ExecuteNonQuery(sql);
                    WriteLog("\t\t共計刪除 CaseDeadDetail " + ret.ToString()+ " 筆");
                }
            }
            catch (Exception ex)
            {
                WriteLog("\t\t更新 CaseDeadDetail 失敗!! " + ex.Message.ToString());
            }
        }
    }
}
