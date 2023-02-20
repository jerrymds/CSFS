using CTBC.CSFS.BussinessLogic;
using CTBC.CSFS.Pattern;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CTBC.CSFS.Models;
using CTBC.FrameWork.HTG;
using System.Text.RegularExpressions;

namespace CTBC.WinExe.AutoSeizure
{
    public class AutoSeizureBiz : CTBC.CSFS.BussinessLogic.CommonBIZ
    {
        internal int getCaseMasterStillRuning()
        {
            //List<CaseMaster> Result = new List<CaseMaster>();

            string strsql = @"SELECT Count(*)  FROM [dbo].[CaseMaster] c inner join [dbo].EDocTXT1 e on c.CaseId=e.CaseId 
   where CaseKind2='扣押' and ReceiveKind='電子公文' and c.Status='999'  and (NULLIF(ReturnReason, '') IS NULL ) ";

            Parameter.Clear();
            DataTable Result = Search(strsql);
            //Result = SearchList<CaseMaster>(strsql).ToList();

            return int.Parse(Result.Rows[0][0].ToString());
        }

        internal List<CaseMaster> getCaseMasterStillRuningCase()
        {
            //List<CaseMaster> Result = new List<CaseMaster>();

            string strsql = @"SELECT c.*  FROM [dbo].[CaseMaster] c inner join [dbo].EDocTXT1 e on c.CaseId=e.CaseId 
   where CaseKind2='扣押' and ReceiveKind='電子公文' and c.Status='999'";

            Parameter.Clear();
            return SearchList<CaseMaster>(strsql).ToList();
            
        }



        internal List<CaseMaster> getCaseMaster(string status)
        {
            List<CaseMaster> Result = new List<CaseMaster>();

            string strsql = @"SELECT c.*  FROM [dbo].[CaseMaster] c inner join [dbo].EDocTXT1 e on c.CaseId=e.CaseId 
   where CaseKind2='扣押' and ReceiveKind='電子公文' and c.Status=@Status and (NULLIF(ReturnReason, '') IS NULL ) ORDER BY CASENO";

            Parameter.Clear();
            Parameter.Add(new CommandParameter("@Status", status));
            Result = SearchList<CaseMaster>(strsql).ToList();

            return Result;

        }

        /// <summary>
        /// 設定那些CaseID 正要執行
        /// </summary>
        /// <param name="caseids"></param>
        /// <param name="Status">999: 自動扣押    998: 自動撒銷     997: 自動支付</param>
        internal void setCaseMasterRunning(List<Guid> caseids, string Status)
        {
            string whereCondition = string.Join("','", caseids);
            string sql = string.Format("UPDATE [CaseMaster] SET [Status]=@Status WHERE [CaseId] in ('{0}')", whereCondition);
            Parameter.Clear();
            Parameter.Add(new CommandParameter("@Status", Status));
            ExecuteNonQuery(sql);
            return;
        }

        internal List<AgentSetting> getAgentSetting()
        {
            //ctx.AgentSetting.Where(x => (bool)x.IsSeizure).OrderBy(x => x.SettingId).ToList();
            string strsql = @"SELECT *  FROM [dbo].[AgentSetting] where IsSeizure='1'  ORDER BY SettingId";

            Parameter.Clear();
            var Result = SearchList<AgentSetting>(strsql).ToList();

            return Result;
        }


        internal string  setExecuteCaseNo(string strCodeType,string CaseNo)
        {
            string ret = "";
            try
            {
                string sql = "UPDATE PARMCode SET CodeNo=@CaseNo, ModifiedDate=getdate() WHERE CodeType=@CodeType ";
                Parameter.Clear();
                Parameter.Add(new CommandParameter("@CaseNo", CaseNo));
                Parameter.Add(new CommandParameter("@CodeType", strCodeType));
                ExecuteNonQuery(sql);
            }
            catch(Exception ex)
            {
                ret = ex.Message.ToString();
            }
            return ret;
        }


        internal string setAgentUserSYS(Guid CaseId)
        {
            string ret = "";
            try
            {
                string sql = string.Format("UPDATE [CaseMaster] SET [AgentUser]='SYS',AssignPerson='SYS' WHERE [CaseId]=@CaseId");
                Parameter.Clear();
                Parameter.Add(new CommandParameter("@CaseId", CaseId));
                ExecuteNonQuery(sql);
            }
            catch (Exception ex)
            {

                ret = ex.Message.ToString();
            }
            return ret;
        }

        internal string insertCaseHistory(CaseMaster casedoc, string FromRole, string FromUser, string FromFolder, string Event, string ToRole, string ToUser, string ToFolder)
        {
            string ret = "";
            try
            {
                string sql = @"INSERT INTO CaseHistory(CaseId, FromRole, FromUser, FromFolder, Event, EventTime, ToRole, ToUser, ToFolder, CreatedUser, CreatedDate) 
                                               VALUES (@CaseId, @FromRole, @FromUser, @FromFolder, @Event, getDate(), @ToRole, @ToUser, @ToFolder, @CreatedUser, getDate() )";
                Parameter.Clear();
                Parameter.Add(new CommandParameter("@CaseId", casedoc.CaseId));
                Parameter.Add(new CommandParameter("@FromRole", FromRole));
                Parameter.Add(new CommandParameter("@FromUser", FromUser));
                Parameter.Add(new CommandParameter("@FromFolder", FromFolder));
                Parameter.Add(new CommandParameter("@Event", Event));
                Parameter.Add(new CommandParameter("@ToRole", ToRole));
                Parameter.Add(new CommandParameter("@ToUser", ToUser));
                Parameter.Add(new CommandParameter("@ToFolder", ToFolder));
                Parameter.Add(new CommandParameter("@CreatedUser", FromUser));


                ExecuteNonQuery(sql);

            }
            catch (Exception ex)
            {

                ret = ex.Message.ToString();
            }
            return ret;

        }

        internal string insertCaseAssignTable(Guid guid, string eQueryStaff, int p)
        {

            string ret = "";
            try
            {
                string sql = @"INSERT INTO CaseAssignTable(CaseId, EmpId, AlreadyAssign, CreatdUser, CreatedDate, ModifiedUser, ModifiedDate) 
                                                   VALUES (@CaseId, @EmpId, @AlreadyAssign, @CreatdUser, getDate() , @ModifiedUser, getDate() )";
                Parameter.Clear();
                Parameter.Add(new CommandParameter("@CaseId", guid));
                Parameter.Add(new CommandParameter("@EmpId", eQueryStaff));
                Parameter.Add(new CommandParameter("@AlreadyAssign", p));
                Parameter.Add(new CommandParameter("@CreatdUser", eQueryStaff));
                Parameter.Add(new CommandParameter("@ModifiedUser", eQueryStaff));

                ExecuteNonQuery(sql);

            }
            catch (Exception ex)
            {

                ret = ex.Message.ToString();
            }
            return ret;
        }


        internal PARMCode getParmCodeByCodeType(string strCodeType)
        {
            PARMCode results = new PARMCode();
            string sql1 = "SELECT TOP 1 * from PARMCode WHERE CodeType='" + strCodeType + "'";
            results = SearchList<PARMCode>(sql1).ToList().First();

            return results;
        }


        internal int UpdateParmCode(string strCodeType, string strCaseNo)
        {
            int iCount = 1;


            try
            {
                string sql1 = "SELECT TOP 1 * from PARMCode WHERE CodeType='" + strCodeType + "'";
                var result = SearchList<PARMCode>(sql1).ToList();
                var cNo = result.Where(x => x.CodeType == strCodeType).FirstOrDefault();
                if (cNo != null)
                {
                    if (cNo.CodeNo == strCaseNo) // 如果這次, 跟上次一樣, 則把次數+1
                    {
                        iCount = int.Parse(cNo.CodeTag.ToString()) + 1;
                        cNo.CodeTag = iCount.ToString();
                        string sql2 = "UPDATE PARMCode SET CodeTag='" + iCount.ToString() + "' WHERE CodeType='" + strCodeType + "'";
                        Parameter.Clear();
                        ExecuteNonQuery(sql2);
                    }
                    else
                    {
                        cNo.CodeNo = strCaseNo;
                        cNo.CodeTag = "1";
                        iCount = 1;
                        string sql2 = "UPDATE PARMCode SET CodeTag='" + iCount.ToString() + "',CodeNo='" + strCaseNo + "' WHERE CodeType='" + strCodeType + "'";
                        Parameter.Clear();
                        ExecuteNonQuery(sql2);
                    }

                }
            }
            catch (Exception)
            {

            }

            return iCount;
        }

        internal void setCaseMasterC01B01(Guid first)
        {
            try
            {

                string sql = "UPDATE CaseMaster SET Status='B01' WHERE Status='999';UPDATE CaseMaster SET Status='C01',AgentUser='SYS' WHERE CaseId=@CaseId";
                Parameter.Clear();
                Parameter.Add(new CommandParameter("@CaseId", first));
                
                ExecuteNonQuery(sql);


            }
            catch (Exception)
            {


            }
            return;
        }

        internal bool UpdateProcCaseNo(string strCodeType, string strCaseNo)
        {
            bool result = false;

            try
            {
                string sql1 = string.Format("UPDATE PARMCode SET CodeNo='{0}', CodeTag='1', ModifiedDate=getdate() WHERE CodeType='{1}' ", strCaseNo, strCodeType);
                Parameter.Clear();
                ExecuteNonQuery(sql1);
                result = true;
            }
            catch (Exception)
            {
                result = false;
                throw;
            }


            return result;
        }

        internal string insertCaseMemo(Guid guid, string p, List<string> DocMemo, string eQueryStaff)
        {

            string ret = "";
            try
            {
                var newDocMemo = DocMemo.Distinct();
                foreach (var s in newDocMemo)
                {
                    string sql = @"INSERT INTO CaseMemo(CaseId, MemoType, Memo, MemoDate, MemoUser) 
                                                   VALUES (@CaseId, @MemoType, @Memo, getDate() , @MemoUser )";
                    Parameter.Clear();
                    Parameter.Add(new CommandParameter("@CaseId", guid));
                    Parameter.Add(new CommandParameter("@MemoType", p));
                    Parameter.Add(new CommandParameter("@Memo", s));
                    Parameter.Add(new CommandParameter("@MemoUser", eQueryStaff));

                    ExecuteNonQuery(sql);
                }

            }
            catch (Exception ex)
            {

                ret = ex.Message.ToString();
            }
            return ret;


        }

        internal void updateAutoLog(long id, int Status, string ErrorMsg)
        {
            try
            {   
                    string sql = @"UPDATE AutoLog SET Status=@Status,ErrorMsg=@ErrorMsg WHERE Id=@Id";
                    Parameter.Clear();
                    Parameter.Add(new CommandParameter("@Id", id));
                    Parameter.Add(new CommandParameter("@Status", Status));
                    Parameter.Add(new CommandParameter("@ErrorMsg", ErrorMsg));

                    ExecuteNonQuery(sql);  
            }
            catch (Exception ex)
            {
            }

        }

        internal long addAutoLog(Guid CaseID, string SendUser, string DocNo, string ObligorNo, string SeviceNo)
        {
            long returnid = 0;
            try
            {
                string sql = @"INSERT INTO  AutoLog (CaseId, SendUser, DocNo, ObligorNo, ServiceName, SendDate, Status, ErrorMsg, CreateDatetime) 
                                VALUES (@CaseId, @SendUser, @DocNo, @ObligorNo, @ServiceName, GETDATE(), '0', '', GETDATE());SELECT SCOPE_IDENTITY();";
                Parameter.Clear();
                Parameter.Add(new CommandParameter("@CaseId", CaseID));
                Parameter.Add(new CommandParameter("@SendUser", SendUser));
                Parameter.Add(new CommandParameter("@DocNo", DocNo));
                Parameter.Add(new CommandParameter("@ObligorNo", ObligorNo));
                Parameter.Add(new CommandParameter("@ServiceName", SeviceNo));

                var ret = Search(sql);
                if( ret.Rows.Count >0 )
                {
                    returnid = long.Parse(ret.Rows[0][0].ToString());
                }
                //ExecuteNonQuery(sql);
            }
            catch (Exception ex)
            {
            }

            return returnid;
        }

                /// <summary>
        /// 把扣押結果, 寫到CaseSeizure, 把全部跑到Step 6, 沒有中途跳走的
        /// /// </summary>
        /// <param name="caseid"></param> 
        internal void insertCaseSeizure(CaseMaster casedoc, List<ObligorAccount> AccLists, string userid, string SeizureResult)
        {
            //SeizureResult = "0001|完全沒有扣到";
            //SeizureResult = "0002|只扣到手續費, 但扣押金額金扣到部分";
            //SeizureResult = "0000|所有金額都有扣押";

            // 20180801, 要多insert 一個欄位, 叫他案扣押(OtherSeizure) , 若450-31有 "或者" 450-30, 04 有, 則設為'Y', ... 


            if (SeizureResult.StartsWith("0000")) // 只要寫回被扣押的帳戶
            {
                
                

                    foreach (var acc in AccLists.Where(x => x.showSeizure > 0))
                    {
                        string strSegmentCode = acc.SegmentCode.Trim().Equals("1") ? "個金" :
                            acc.SegmentCode.Trim().Equals("2") ? "法金" :
                            acc.SegmentCode.Trim().Equals("3") ? "SBG" :
                            "";

                        string strOtherSeizure = "N";
                        if (acc.message450Code.Contains("04") || acc.is45031OK)
                            strOtherSeizure = "Y";

                        #region add obj
                        string sql = @"INSERT INTO  CaseSeizure (CaseId, PayCaseId, CaseNo, CustId, CustName, BranchNo, BranchName, Account, AccountStatus, Currency, Balance, SeizureAmount, ExchangeRate, SeizureAmountNtd, PayAmount, SeizureStatus, CreatedUser, CreatedDate, ModifiedUser, ModifiedDate, CancelCaseId, ProdCode, Link, SegmentCode, CancelAmount, TripAmount, OtherSeizure, Seq, SeizureAmountSUB, TxtStatus, TxtProdCode) 
                                VALUES ( @CaseId, @PayCaseId, @CaseNo, @CustId, @CustName, @BranchNo, @BranchName, @Account, @AccountStatus, @Currency, @Balance, @SeizureAmount, @ExchangeRate,
                                        @SeizureAmountNtd, @PayAmount, @SeizureStatus, @CreatedUser, @CreatedDate, @ModifiedUser, @ModifiedDate, 
                                    @CancelCaseId, @ProdCode, @Link, @SegmentCode, @CancelAmount, @TripAmount, @OtherSeizure, @Seq, @SeizureAmountSUB, @TxtStatus, @TxtProdCode)";
                        Parameter.Clear();
                        Parameter.Add(new CommandParameter("@CaseId", casedoc.CaseId));
                        Parameter.Add(new CommandParameter("@PayCaseId", null));
                        Parameter.Add(new CommandParameter("@CaseNo", casedoc.CaseNo));
                        Parameter.Add(new CommandParameter("@CustId", acc.Id));
                        Parameter.Add(new CommandParameter("@CustName", acc.Name));
                        Parameter.Add(new CommandParameter("@BranchNo", acc.BranchNo));
                        Parameter.Add(new CommandParameter("@BranchName", acc.BranchName));
                        Parameter.Add(new CommandParameter("@Account", acc.Account));
                        Parameter.Add(new CommandParameter("@AccountStatus", acc.AccountStatus));
                        Parameter.Add(new CommandParameter("@Currency", acc.Ccy));
                        Parameter.Add(new CommandParameter("@Balance", acc.Bal.ToString()));
                        Parameter.Add(new CommandParameter("@SeizureAmount", acc.showSeizure));
                        Parameter.Add(new CommandParameter("@ExchangeRate", acc.Rate));
                        Parameter.Add(new CommandParameter("@SeizureAmountNtd", Math.Floor(acc.showSeizure * acc.Rate)));
                        Parameter.Add(new CommandParameter("@PayAmount", 0.0m));
                        Parameter.Add(new CommandParameter("@SeizureStatus", "0"));
                        Parameter.Add(new CommandParameter("@CreatedUser", userid));
                        Parameter.Add(new CommandParameter("@CreatedDate", DateTime.Now));
                        Parameter.Add(new CommandParameter("@ModifiedUser", userid));
                        Parameter.Add(new CommandParameter("@ModifiedDate", DateTime.Now));
                        Parameter.Add(new CommandParameter("@CancelCaseId", null));
                        Parameter.Add(new CommandParameter("@ProdCode", acc.ProdDesc));
                        Parameter.Add(new CommandParameter("@Link", acc.Link));
                        Parameter.Add(new CommandParameter("@SegmentCode", strSegmentCode));
                        Parameter.Add(new CommandParameter("@CancelAmount", null));
                        Parameter.Add(new CommandParameter("@TripAmount", null));
                        Parameter.Add(new CommandParameter("@OtherSeizure", strOtherSeizure));
                        Parameter.Add(new CommandParameter("@Seq", acc.SeizureSeq));
                        Parameter.Add(new CommandParameter("@SeizureAmountSUB", 0.0m));
                        Parameter.Add(new CommandParameter("@TxtStatus", "1"));
                        Parameter.Add(new CommandParameter("@TxtProdCode", acc.ProdCode));
                        ExecuteNonQuery(sql);  



                        #endregion

                    }




                    
                

            }
            else // 若是部分被扣押, 則, 全部帳戶寫回CaseSeizure
            {

                    foreach (var acc in AccLists)
                    {
                        string strSegmentCode = acc.SegmentCode.Trim().Equals("1") ? "個金" :
                            acc.SegmentCode.Trim().Equals("2") ? "法金" :
                            acc.SegmentCode.Trim().Equals("3") ? "SBG" :
                            "";
                        string strOtherSeizure = "N";
                        if (acc.message450Code.Contains("04") || acc.is45031OK)
                            strOtherSeizure = "Y";
                        string strTxtStatus = acc.showSeizure > 0.0m ? "1" : "";

                        #region add obj
                        string sql = @"INSERT INTO  CaseSeizure (CaseId, PayCaseId, CaseNo, CustId, CustName, BranchNo, BranchName, Account, AccountStatus, Currency, Balance, SeizureAmount, ExchangeRate, SeizureAmountNtd, PayAmount, SeizureStatus, CreatedUser, CreatedDate, ModifiedUser, ModifiedDate, CancelCaseId, ProdCode, Link, SegmentCode, CancelAmount, TripAmount, OtherSeizure, Seq, SeizureAmountSUB, TxtStatus, TxtProdCode) 
                                VALUES ( @CaseId, @PayCaseId, @CaseNo, @CustId, @CustName, @BranchNo, @BranchName, @Account, @AccountStatus, @Currency, @Balance, @SeizureAmount, @ExchangeRate,
                                        @SeizureAmountNtd, @PayAmount, @SeizureStatus, @CreatedUser, @CreatedDate, @ModifiedUser, @ModifiedDate, 
                                    @CancelCaseId, @ProdCode, @Link, @SegmentCode, @CancelAmount, @TripAmount, @OtherSeizure, @Seq, @SeizureAmountSUB, @TxtStatus, @TxtProdCode)";
                        Parameter.Clear();
                        Parameter.Add(new CommandParameter("@CaseId", casedoc.CaseId));
                        Parameter.Add(new CommandParameter("@PayCaseId", null));
                        Parameter.Add(new CommandParameter("@CaseNo", casedoc.CaseNo));
                        Parameter.Add(new CommandParameter("@CustId", acc.Id));
                        Parameter.Add(new CommandParameter("@CustName", acc.Name));
                        Parameter.Add(new CommandParameter("@BranchNo", acc.BranchNo));
                        Parameter.Add(new CommandParameter("@BranchName", acc.BranchName));
                        Parameter.Add(new CommandParameter("@Account", acc.Account));
                        Parameter.Add(new CommandParameter("@AccountStatus", acc.AccountStatus));
                        Parameter.Add(new CommandParameter("@Currency", acc.Ccy));
                        Parameter.Add(new CommandParameter("@Balance", acc.Bal.ToString()));
                        Parameter.Add(new CommandParameter("@SeizureAmount", acc.showSeizure));
                        Parameter.Add(new CommandParameter("@ExchangeRate", acc.Rate));
                        Parameter.Add(new CommandParameter("@SeizureAmountNtd", Math.Floor(acc.showSeizure * acc.Rate)));
                        Parameter.Add(new CommandParameter("@PayAmount", 0.0m));
                        Parameter.Add(new CommandParameter("@SeizureStatus", "0"));
                        Parameter.Add(new CommandParameter("@CreatedUser", userid));
                        Parameter.Add(new CommandParameter("@CreatedDate", DateTime.Now));
                        Parameter.Add(new CommandParameter("@ModifiedUser", userid));
                        Parameter.Add(new CommandParameter("@ModifiedDate", DateTime.Now));
                        Parameter.Add(new CommandParameter("@CancelCaseId", null));
                        Parameter.Add(new CommandParameter("@ProdCode", acc.ProdDesc));
                        Parameter.Add(new CommandParameter("@Link", acc.Link));
                        Parameter.Add(new CommandParameter("@SegmentCode", strSegmentCode));
                        Parameter.Add(new CommandParameter("@CancelAmount", null));
                        Parameter.Add(new CommandParameter("@TripAmount", null));
                        Parameter.Add(new CommandParameter("@OtherSeizure", strOtherSeizure));
                        Parameter.Add(new CommandParameter("@Seq", acc.SeizureSeq));
                        Parameter.Add(new CommandParameter("@SeizureAmountSUB", 0.0m));
                        Parameter.Add(new CommandParameter("@TxtStatus", strTxtStatus));
                        Parameter.Add(new CommandParameter("@TxtProdCode", acc.ProdCode));
                        ExecuteNonQuery(sql);



                        #endregion
                    }

            }


        }


                /// <summary>
        /// 把扣押結果, 寫到CaseSeizure, 中途跳走的案, 所以寫2
        /// /// </summary>
        /// <param name="caseid"></param> 
        internal void insertCaseSeizure2(CaseMaster casedoc, List<ObligorAccount> AccLists, string userid, string SeizureResult)
        {
            //SeizureResult = "0001|完全沒有扣到";
            //SeizureResult = "0002|只扣到手續費, 但扣押金額金扣到部分";
            //SeizureResult = "0000|所有金額都有扣押";

            // 20180801, 要多insert 一個欄位, 叫他案扣押(OtherSeizure) , 若450-31有 "或者" 450-30, 04 有, 則設為'Y', ... 


            if (SeizureResult.StartsWith("0000")) // 只要寫回被扣押的帳戶
            {



                foreach (var acc in AccLists.Where(x => x.showSeizure > 0))
                {
                    string strSegmentCode = acc.SegmentCode.Trim().Equals("1") ? "個金" :
                        acc.SegmentCode.Trim().Equals("2") ? "法金" :
                        acc.SegmentCode.Trim().Equals("3") ? "SBG" :
                        "";

                    string strOtherSeizure = "N";
                    if (acc.message450Code.Contains("04") || acc.is45031OK)
                        strOtherSeizure = "Y";

                    #region add obj
                    string sql = @"INSERT INTO  CaseSeizure (CaseId, PayCaseId, CaseNo, CustId, CustName, BranchNo, BranchName, Account, AccountStatus, Currency, Balance, SeizureAmount, ExchangeRate, SeizureAmountNtd, PayAmount, SeizureStatus, CreatedUser, CreatedDate, ModifiedUser, ModifiedDate, CancelCaseId, ProdCode, Link, SegmentCode, CancelAmount, TripAmount, OtherSeizure, Seq, SeizureAmountSUB, TxtStatus, TxtProdCode) 
                                VALUES ( @CaseId, @PayCaseId, @CaseNo, @CustId, @CustName, @BranchNo, @BranchName, @Account, @AccountStatus, @Currency, @Balance, @SeizureAmount, @ExchangeRate,
                                        @SeizureAmountNtd, @PayAmount, @SeizureStatus, @CreatedUser, @CreatedDate, @ModifiedUser, @ModifiedDate, 
                                    @CancelCaseId, @ProdCode, @Link, @SegmentCode, @CancelAmount, @TripAmount, @OtherSeizure, @Seq, @SeizureAmountSUB, @TxtStatus, @TxtProdCode)";
                    Parameter.Clear();
                    Parameter.Add(new CommandParameter("@CaseId", casedoc.CaseId));
                    Parameter.Add(new CommandParameter("@PayCaseId", null));
                    Parameter.Add(new CommandParameter("@CaseNo", casedoc.CaseNo));
                    Parameter.Add(new CommandParameter("@CustId", acc.Id));
                    Parameter.Add(new CommandParameter("@CustName", acc.Name));
                    Parameter.Add(new CommandParameter("@BranchNo", acc.BranchNo));
                    Parameter.Add(new CommandParameter("@BranchName", acc.BranchName));
                    Parameter.Add(new CommandParameter("@Account", acc.Account));
                    Parameter.Add(new CommandParameter("@AccountStatus", acc.AccountStatus));
                    Parameter.Add(new CommandParameter("@Currency", acc.Ccy));
                    Parameter.Add(new CommandParameter("@Balance", acc.Bal.ToString()));
                    Parameter.Add(new CommandParameter("@SeizureAmount", acc.planSeizure));
                    Parameter.Add(new CommandParameter("@ExchangeRate", acc.Rate));
                    Parameter.Add(new CommandParameter("@SeizureAmountNtd", Math.Floor(acc.planSeizure * acc.Rate)));
                    Parameter.Add(new CommandParameter("@PayAmount", 0.0m));
                    Parameter.Add(new CommandParameter("@SeizureStatus", "0"));
                    Parameter.Add(new CommandParameter("@CreatedUser", userid));
                    Parameter.Add(new CommandParameter("@CreatedDate", DateTime.Now));
                    Parameter.Add(new CommandParameter("@ModifiedUser", userid));
                    Parameter.Add(new CommandParameter("@ModifiedDate", DateTime.Now));
                    Parameter.Add(new CommandParameter("@CancelCaseId", null));
                    Parameter.Add(new CommandParameter("@ProdCode", acc.ProdDesc));
                    Parameter.Add(new CommandParameter("@Link", acc.Link));
                    Parameter.Add(new CommandParameter("@SegmentCode", strSegmentCode));
                    Parameter.Add(new CommandParameter("@CancelAmount", null));
                    Parameter.Add(new CommandParameter("@TripAmount", null));
                    Parameter.Add(new CommandParameter("@OtherSeizure", strOtherSeizure));
                    Parameter.Add(new CommandParameter("@Seq", acc.SeizureSeq));
                    Parameter.Add(new CommandParameter("@SeizureAmountSUB", 0.0m));
                    Parameter.Add(new CommandParameter("@TxtStatus", "1"));
                    Parameter.Add(new CommandParameter("@TxtProdCode", acc.ProdCode));
                    ExecuteNonQuery(sql);



                    #endregion

                }
            }
            else // 若是部分被扣押, 則, 全部帳戶寫回CaseSeizure
            {

                foreach (var acc in AccLists)
                {
                    string strSegmentCode = acc.SegmentCode.Trim().Equals("1") ? "個金" :
                        acc.SegmentCode.Trim().Equals("2") ? "法金" :
                        acc.SegmentCode.Trim().Equals("3") ? "SBG" :
                        "";
                    string strOtherSeizure = "N";
                    if (acc.message450Code.Contains("04") || acc.is45031OK)
                        strOtherSeizure = "Y";
                    string strTxtStatus = acc.showSeizure > 0.0m ? "1" : "";

                    #region add obj
                    string sql = @"INSERT INTO  CaseSeizure (CaseId, PayCaseId, CaseNo, CustId, CustName, BranchNo, BranchName, Account, AccountStatus, Currency, Balance, SeizureAmount, ExchangeRate, SeizureAmountNtd, PayAmount, SeizureStatus, CreatedUser, CreatedDate, ModifiedUser, ModifiedDate, CancelCaseId, ProdCode, Link, SegmentCode, CancelAmount, TripAmount, OtherSeizure, Seq, SeizureAmountSUB, TxtStatus, TxtProdCode) 
                                VALUES ( @CaseId, @PayCaseId, @CaseNo, @CustId, @CustName, @BranchNo, @BranchName, @Account, @AccountStatus, @Currency, @Balance, @SeizureAmount, @ExchangeRate,
                                        @SeizureAmountNtd, @PayAmount, @SeizureStatus, @CreatedUser, @CreatedDate, @ModifiedUser, @ModifiedDate, 
                                    @CancelCaseId, @ProdCode, @Link, @SegmentCode, @CancelAmount, @TripAmount, @OtherSeizure, @Seq, @SeizureAmountSUB, @TxtStatus, @TxtProdCode)";
                    Parameter.Clear();
                    Parameter.Add(new CommandParameter("@CaseId", casedoc.CaseId));
                    Parameter.Add(new CommandParameter("@PayCaseId", null));
                    Parameter.Add(new CommandParameter("@CaseNo", casedoc.CaseNo));
                    Parameter.Add(new CommandParameter("@CustId", acc.Id));
                    Parameter.Add(new CommandParameter("@CustName", acc.Name));
                    Parameter.Add(new CommandParameter("@BranchNo", acc.BranchNo));
                    Parameter.Add(new CommandParameter("@BranchName", acc.BranchName));
                    Parameter.Add(new CommandParameter("@Account", acc.Account));
                    Parameter.Add(new CommandParameter("@AccountStatus", acc.AccountStatus));
                    Parameter.Add(new CommandParameter("@Currency", acc.Ccy));
                    Parameter.Add(new CommandParameter("@Balance", acc.Bal.ToString()));
                    Parameter.Add(new CommandParameter("@SeizureAmount", acc.planSeizure));
                    Parameter.Add(new CommandParameter("@ExchangeRate", acc.Rate));
                    Parameter.Add(new CommandParameter("@SeizureAmountNtd", Math.Floor(acc.planSeizure * acc.Rate)));
                    Parameter.Add(new CommandParameter("@PayAmount", 0.0m));
                    Parameter.Add(new CommandParameter("@SeizureStatus", "0"));
                    Parameter.Add(new CommandParameter("@CreatedUser", userid));
                    Parameter.Add(new CommandParameter("@CreatedDate", DateTime.Now));
                    Parameter.Add(new CommandParameter("@ModifiedUser", userid));
                    Parameter.Add(new CommandParameter("@ModifiedDate", DateTime.Now));
                    Parameter.Add(new CommandParameter("@CancelCaseId", null));
                    Parameter.Add(new CommandParameter("@ProdCode", acc.ProdDesc));
                    Parameter.Add(new CommandParameter("@Link", acc.Link));
                    Parameter.Add(new CommandParameter("@SegmentCode", strSegmentCode));
                    Parameter.Add(new CommandParameter("@CancelAmount", null));
                    Parameter.Add(new CommandParameter("@TripAmount", null));
                    Parameter.Add(new CommandParameter("@OtherSeizure", strOtherSeizure));
                    Parameter.Add(new CommandParameter("@Seq", acc.SeizureSeq));
                    Parameter.Add(new CommandParameter("@SeizureAmountSUB", 0.0m));
                    Parameter.Add(new CommandParameter("@TxtStatus", strTxtStatus));
                    Parameter.Add(new CommandParameter("@TxtProdCode", acc.ProdCode));
                    ExecuteNonQuery(sql);



                    #endregion
                }

            }
        }

        internal void updateCaseMasterStatus(Guid caseid, string p)
        {

            try
            {

                if (p == "D01")
                {
                    string sql = @"UPDATE CaseMaster SET Status=@Status,ReturnReason='自動扣押',AgentSubmitDate=GETDATE()  WHERE CaseId=@CaseId";
                    Parameter.Clear();
                    Parameter.Add(new CommandParameter("@CaseId", caseid));
                    Parameter.Add(new CommandParameter("@Status", p));

                    ExecuteNonQuery(sql);

                }
                else
                {
                    string sql = @"UPDATE CaseMaster SET Status=@Status,ReturnReason='自動扣押' WHERE CaseId=@CaseId";
                    Parameter.Clear();
                    Parameter.Add(new CommandParameter("@CaseId", caseid));
                    Parameter.Add(new CommandParameter("@Status", p));

                    ExecuteNonQuery(sql);

                }

            }
            catch (Exception ex)
            {
            }

        }

        internal void updateCaseMasterAgentUser(Guid caseid, string eQueryStaff)
        {
            // 先取得要進select AgentSection,AgentDeptId,AgentBranchId from [V_AgentAndDept] where [EmpID] = @EmpId 

            LdapEmployeeBiz agentBiz = new LdapEmployeeBiz();
            LDAPEmployee agent = agentBiz.GetAllEmployeeInEmployeeViewByEmpId(eQueryStaff);

            try
            {
                if (agent != null)
                {
                    string sql = @"UPDATE CaseMaster SET AgentUser=@AgentUser,AssignPerson=@AgentUser,AgentSection=@AgentSection,AgentDeptId=@AgentDeptId,AgentBranchId=@AgentBranchId  WHERE CaseId=@CaseId";
                    Parameter.Clear();
                    Parameter.Add(new CommandParameter("@AgentUser", eQueryStaff));
                    Parameter.Add(new CommandParameter("@CaseId", caseid));
                    Parameter.Add(new CommandParameter("@AgentSection", agent.SectionName));
                    Parameter.Add(new CommandParameter("@AgentDeptId", agent.DepId));
                    Parameter.Add(new CommandParameter("@AgentBranchId", agent.BranchId));

                    ExecuteNonQuery(sql);
                }
                else
                {
                    string sql = @"UPDATE CaseMaster SET AgentUser=@AgentUser,AssignPerson=@AgentUser  WHERE CaseId=@CaseId";
                    Parameter.Clear();
                    Parameter.Add(new CommandParameter("@AgentUser", eQueryStaff));
                    Parameter.Add(new CommandParameter("@CaseId", caseid));

                    ExecuteNonQuery(sql);
                }


            }
            catch (Exception ex)
            {
            }

        }

        internal void delCaseSeizureNoSeizure(CaseMaster caseDoc, List<ObligorAccount> calculatedAccList)
        {
            if (calculatedAccList.Where(x => x.noSeizure).Count() > 0)
            {
                foreach (var c in calculatedAccList.Where(x => x.noSeizure))
                {
                    try 
	                {	        
		                        Guid gcaseid = Guid.Parse(c.CaseId);
                        string sql = @"DELETE FROM CaseSeizure WHERE CaseId=@CaseId AND Account=@Account AND SeizureStatus='0' AND SeizureAmount='0'";
                        Parameter.Clear();
                        Parameter.Add(new CommandParameter("@Account", c.Account));
                        Parameter.Add(new CommandParameter("@CaseId", gcaseid));
                        ExecuteNonQuery(sql);
	                }
	                catch (Exception)
	                {
		
		                throw;
	                }
                }
            }

            
        }

        /// <summary>
        /// 取得待扣押的義務人, 若同一個CaseID中, 有個人ID及公司ID, 則要多發查67102，驗證戶名是否相同
        /// 若不相同, 則移除該公司的ID
        /// </summary>
        /// <param name="allCaseMaster"></param>
        /// <returns></returns>
        internal List<CaseObligor> getObligor(Guid caseid, ref string message, ref bool isForeignID)
        {
            int iCompany = 0;
            int iHuman = 0;
            Regex reCo = new Regex(@"^\d{8}$");
            Regex reHuman = new Regex(@"^[A-Z]{1}[0-7]\d{8,10}$");

            //System.Text.RegularExpressions.Regex reOld = new Regex(@"^([A-Z]|[a-z]){2}\d+");
            // 20200828, 改變外國人規則為第2碼為... 8或9
            System.Text.RegularExpressions.Regex re = new Regex(@"^[A-Z]{1}[A-Z|8-9]{1}\d+$");
            //bool isForeignID = false;
            isForeignID = false;
            List<CaseObligor> Result = new List<CaseObligor>(); // 
            List<CaseObligor> CoIDs = new List<CaseObligor>(); // 若有公司統編, 則加入此LIST
               

                string sql = @"SELECT *  FROM CaseObligor WHERE CaseId=@CaseId ";
                Parameter.Clear();
                Parameter.Add(new CommandParameter("@CaseId", caseid));
                var obligorList = SearchList<CaseObligor>(sql);



                foreach (var o in obligorList)
                {
                    if (re.IsMatch(o.ObligorNo))
                    {
                        isForeignID = true;
                        return null;
                    }

                    if (reCo.IsMatch(o.ObligorNo))
                    {
                        iCompany++;
                        CoIDs.Add(o);
                    }
                    if (reHuman.IsMatch(o.ObligorNo))
                    {
                        iHuman++;
                        Result.Add(o); // 個人，直接加入義務人LIST
                    }
                }
                if (iCompany == 0 && iHuman > 0) // 只有個人ID, 
                    return Result;
                else
                {
                    if (iCompany > 0 && iHuman == 0) // 只有公司ID
                        return CoIDs;
                    else  // 表示同時有法人及個人, 則需要發查67102, 驗證戶名是否相符
                    {

                    }
                }

            return Result;
        }




        internal bool getIsPerson(CaseMaster casedoc)
        {
            Regex reHuman = new Regex(@"^[A-Z]{1}\d{9,10}");
            Regex reCo = new Regex(@"^\d{8}");
            int iCount = 0;
            int pCount = 0;
            bool result = false;
            
            {


                string sql = @"SELECT *  FROM CaseObligor WHERE CaseId=@CaseId ";
                Parameter.Clear();
                Parameter.Add(new CommandParameter("@CaseId", casedoc.CaseId));
                var obligorList = SearchList<CaseObligor>(sql);


                foreach (var o in obligorList)
                {
                    if (reCo.IsMatch(o.ObligorNo))
                    {
                        iCount++;
                    }
                    if (reHuman.IsMatch(o.ObligorNo))
                    {
                        pCount++;
                    }
                }
                if (pCount == obligorList.Count())
                    result = true;

            }
            return result;
        }

        internal void getTDOD(string _trn, string Acc_No, ref bool isTD, ref bool isOD, ref bool isLon, ref bool isHoldAmt)
        {
            
            {
                isOD = false;
                isTD = false;
                

                string sql = @"SELECT TOP 1 *  FROM TX_33401 WHERE Acct=@Acct AND TrnNum=@TrnNum ORDER BY TRNNUM DESC";
                Parameter.Clear();
                Parameter.Add(new CommandParameter("@Acct", Acc_No));
                Parameter.Add(new CommandParameter("@TrnNum", _trn));
                var ds401 = Search(sql);

                if (ds401.Rows.Count > 0)
                {
                    //result = decimal.Parse(ds401.TrueAmt);
                    if (!string.IsNullOrEmpty(ds401.Rows[0]["NextAmt"].ToString()))
                    {
                        var dec = decimal.Parse(ds401.Rows[0]["NextAmt"].ToString());
                        if (dec > 0)
                            isOD = true;
                    }

                    if (!string.IsNullOrEmpty(ds401.Rows[0]["TdAmt"].ToString()))
                    {
                        var dec = decimal.Parse(ds401.Rows[0]["TdAmt"].ToString());
                        if (dec > 0)
                            isTD = true;
                    }

                    if (!string.IsNullOrEmpty(ds401.Rows[0]["LonAmt"].ToString()))
                    {
                        var dec = decimal.Parse(ds401.Rows[0]["LonAmt"].ToString());
                        if (dec > 0)
                            isLon = true;
                    }

                    if (!string.IsNullOrEmpty(ds401.Rows[0]["HoldAmt"].ToString()))
                    {
                        var dec = decimal.Parse(ds401.Rows[0]["HoldAmt"].ToString());
                        if (dec > 0)
                            isHoldAmt = true;
                    }
                }
            }
        }

        internal decimal getBalance(string TrnNum, string Acc_No)
        {
            decimal result = 0;
            {
                string sql = @"SELECT TOP 1 *  FROM TX_33401 WHERE Acct=@Acct AND TrnNum=@TrnNum ";
                Parameter.Clear();
                Parameter.Add(new CommandParameter("@Acct", Acc_No));
                Parameter.Add(new CommandParameter("@TrnNum", TrnNum));
                var ds401 = Search(sql);

                
                if (ds401.Rows.Count >0 )
                {
                    result = decimal.Parse(ds401.Rows[0]["TrueAmt"].ToString());
                }
            }
            return result;
        }

        internal void updateLastAssign(AgentSetting nextAgent)
        {
            
            {
                string sql = @"UPDATE PARMCode SET CodeDesc=@CodeDesc WHERE CodeType='AssignLast'; ";
                Parameter.Clear();
                Parameter.Add(new CommandParameter("@CodeDesc", nextAgent.EmpId));

                ExecuteNonQuery(sql);

            }
        }

        internal string getODTDLonAmt(string acc, string type)
        {
            string result = "0";
            // type, 只接受OD, TD, Lon
            


                string sql = @"SELECT TOP 1 *  FROM TX_33401 WHERE Acct=@Acct AND TrnNum=@TrnNum ORDER BY SNO DESC";
                Parameter.Clear();
                Parameter.Add(new CommandParameter("@Acct", acc));

                var acc401 = Search(sql);
                
                if (acc401.Rows.Count >0 )
                {
                    if (type == "OD")
                        result = acc401.Rows[0]["NextAmt"].ToString();
                    if (type == "TD")
                        result = acc401.Rows[0]["TdAmt"].ToString();
                    if (type == "Lon")
                        result = acc401.Rows[0]["LonAmt"].ToString();
                }

            return result.Trim();
        }

        internal List<CaseObligor> Step1(CaseMaster caseDoc)
        {
            List<CaseObligor> Result = new List<CaseObligor>();
            
            {
                string sql = @"SELECT *  FROM CaseObligor WHERE CaseId=@CaseId ";
                Parameter.Clear();
                Parameter.Add(new CommandParameter("@CaseId", caseDoc.CaseId));

                Result = SearchList<CaseObligor>(sql).ToList();
            }
            return Result;
        }

        internal decimal getEDocTotal(Guid CaseId)
        {
            //using (CSFSEntities ctx = new CSFSEntities())
            {

                decimal SeizureTotal = 0.0m;


                

                    string sql = @"SELECT TOP 1 *  FROM EDocTXT1 WHERE CaseId=@CaseId ";
                    Parameter.Clear();
                    Parameter.Add(new CommandParameter("@CaseId", CaseId));
                    var edoc1 = Search(sql);

                    
                    if (edoc1.Rows.Count > 0 )
                    {
                        SeizureTotal = decimal.Parse(edoc1.Rows[0]["Total"].ToString());                        
                    }


                try
                {

                    string sql1 = @"UPDATE CaseMaster SET Receiver='8888',ReceiveAmount=@ReceiveAmount, NotSeizureAmount='450'   WHERE CaseId=@CaseId ";
                    Parameter.Clear();
                    Parameter.Add(new CommandParameter("@CaseId", CaseId));
                    Parameter.Add(new CommandParameter("@ReceiveAmount", (int)SeizureTotal));
                    ExecuteNonQuery(sql1);


                }
                catch (Exception ex)
                {

                }

                return SeizureTotal;

            }
        }

        internal string geteDocTxtMemo(Guid CaseId)
        {
            string eDocTxtMemo = "";
            string sql = @"SELECT TOP 1 *  FROM EDocTXT1 WHERE CaseId=@CaseId ";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("@CaseId", CaseId));
            var edoc1 = Search(sql);


            if (edoc1.Rows.Count > 0)
            {
                eDocTxtMemo = edoc1.Rows[0]["Memo"].ToString();
            }
            return eDocTxtMemo;
        }

        internal bool getTX_45031(string _trn)
        {
            bool result = false;

                string sql1 = @"SELECT Count(*)  FROM TX_00450 WHERE TrnNum=@TrnNum AND DATA1 like '% 9093 %' AND DATA2 like '% 66%' ";
                Parameter.Clear();
                Parameter.Add(new CommandParameter("@TrnNum", _trn));
                int c9093 = 0;
                var ret1 = Search(sql1);
                if (ret1.Rows.Count > 0)
                {
                    c9093 = int.Parse(ret1.Rows[0][0].ToString());
                }



                string sql2 = @"SELECT Count(*)  FROM TX_00450 WHERE TrnNum=@TrnNum AND DATA1 like '% 9095 %' AND DATA2 like '% 66%' ";
                Parameter.Clear();
                Parameter.Add(new CommandParameter("@TrnNum", _trn));
                int c9095 = 0;
                var ret2 = Search(sql2);
                if (ret2.Rows.Count > 0)
                {
                    c9095 = int.Parse(ret2.Rows[0][0].ToString());
                }



                //============== 450-31 , 他案扣押, 是指 電文, 要再打選項01, (未解涷)之, 若有, 則有, 若無, 則"無他案扣押"
                //


                if ((c9093 - c9095) % 2 == 0) // 表示有他案扣押
                {
                    result = false;
                }
                else
                    result = true;

            return result;
        }

        internal void disableQueryStaff(int SortOrder)
        {
            
            {
                string sql = @"UPDATE PARMCode SET Enable='0' WHERE CodeType='eTabsQueryStaff' AND SortOrder=@SortOrder";
                Parameter.Clear();
                Parameter.Add(new CommandParameter("@SortOrder", SortOrder));

                ExecuteNonQuery(sql);    
            }
        }
    }
}
