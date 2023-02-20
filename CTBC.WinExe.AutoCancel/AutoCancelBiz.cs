using CTBC.CSFS.BussinessLogic;
using CTBC.CSFS.Pattern;
using CTBC.CSFS.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTBC.WinExe.AutoCancel
{
    public class AutoCancelBiz : CTBC.CSFS.BussinessLogic.CommonBIZ
    {
        internal int getCaseMasterStillRuning()
        {
            //List<CaseMaster> Result = new List<CaseMaster>();

            string strsql = @"SELECT Count(*)  FROM [dbo].[CaseMaster] c inner join [dbo].EDocTXT2 e on c.CaseId=e.CaseId 
   where CaseKind2='撤銷' and ReceiveKind='電子公文' and c.Status='998'  and (NULLIF(ReturnReason, '') IS NULL ) ";

            Parameter.Clear();
            DataTable Result = Search(strsql);
            //Result = SearchList<CaseMaster>(strsql).ToList();

            return int.Parse(Result.Rows[0][0].ToString());
        }

        internal List<CaseMaster> getCaseMasterStillRuningCase()
        {
            //List<CaseMaster> Result = new List<CaseMaster>();

            string strsql = @"SELECT c.*  FROM [dbo].[CaseMaster] c inner join [dbo].EDocTXT2 e on c.CaseId=e.CaseId 
   where CaseKind2='撤銷' and ReceiveKind='電子公文' and c.Status='998' ";

            Parameter.Clear();
            return SearchList<CaseMaster>(strsql).ToList();
        }


        internal List<CaseMaster> getCaseMaster(string status)
        {
            List<CaseMaster> Result = new List<CaseMaster>();

            string strsql = @"SELECT c.*  FROM [dbo].[CaseMaster] c inner join [dbo].EDocTXT2 e on c.CaseId=e.CaseId 
   where CaseKind2='撤銷' and ReceiveKind='電子公文' and c.Status=@Status and (NULLIF(ReturnReason, '') IS NULL ) ORDER BY CASENO";

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


        internal string setExecuteCaseNo(string strCodeType, string CaseNo)
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
            catch (Exception ex)
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

                string sql = "UPDATE CaseMaster SET Status='B01' WHERE Status='998';UPDATE CaseMaster SET Status='C01',AgentUser='SYS' WHERE CaseId=@CaseId";
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

        internal void insertCaseSeizure(CaseMaster OrginCaseDoc, CaseMaster casedoc, List<CaseSeizure> AccLists, string userid, string SeizureResult)
        {
            if (SeizureResult.StartsWith("0000")) // 只要寫回被扣押的帳戶
            {

                
                    foreach (var acc in AccLists)
                    {
                        try
                        {
                        #region add caseSeizure
                        string sql = @"INSERT INTO  CaseSeizure (CaseId, PayCaseId, CaseNo, CustId, CustName, BranchNo, BranchName, Account, AccountStatus, Currency, Balance, SeizureAmount, ExchangeRate, SeizureAmountNtd, PayAmount, SeizureStatus, CreatedUser, CreatedDate, ModifiedUser, ModifiedDate, CancelCaseId, ProdCode, Link, SegmentCode, CancelAmount, TripAmount, OtherSeizure, Seq, SeizureAmountSUB, TxtStatus, TxtProdCode) 
                                VALUES ( @CaseId, @PayCaseId, @CaseNo, @CustId, @CustName, @BranchNo, @BranchName, @Account, @AccountStatus, @Currency, @Balance, @SeizureAmount, @ExchangeRate,
                                        @SeizureAmountNtd, @PayAmount, @SeizureStatus, @CreatedUser, @CreatedDate, @ModifiedUser, @ModifiedDate, 
                                    @CancelCaseId, @ProdCode, @Link, @SegmentCode, @CancelAmount, @TripAmount, @OtherSeizure, @Seq, @SeizureAmountSUB, @TxtStatus, @TxtProdCode)";
                        Parameter.Clear();
                        Parameter.Add(new CommandParameter("@CaseId", casedoc.CaseId));
                        Parameter.Add(new CommandParameter("@PayCaseId", null));
                        Parameter.Add(new CommandParameter("@CaseNo", casedoc.CaseNo));
                        Parameter.Add(new CommandParameter("@CustId", acc.CustId));
                        Parameter.Add(new CommandParameter("@CustName", acc.CustName));
                        Parameter.Add(new CommandParameter("@BranchNo", acc.BranchNo));
                        Parameter.Add(new CommandParameter("@BranchName", acc.BranchName));
                        Parameter.Add(new CommandParameter("@Account", acc.Account));
                        Parameter.Add(new CommandParameter("@AccountStatus", acc.AccountStatus));
                        Parameter.Add(new CommandParameter("@Currency", acc.Currency));
                        Parameter.Add(new CommandParameter("@Balance", acc.Balance.ToString()));
                        Parameter.Add(new CommandParameter("@SeizureAmount", acc.SeizureAmount));
                        Parameter.Add(new CommandParameter("@ExchangeRate", acc.ExchangeRate));
                        Parameter.Add(new CommandParameter("@SeizureAmountNtd", acc.SeizureAmountNtd));
                        Parameter.Add(new CommandParameter("@PayAmount", 0.0m));
                        Parameter.Add(new CommandParameter("@SeizureStatus", "2"));
                        Parameter.Add(new CommandParameter("@CreatedUser", userid));
                        Parameter.Add(new CommandParameter("@CreatedDate", DateTime.Now));
                        Parameter.Add(new CommandParameter("@ModifiedUser", userid));
                        Parameter.Add(new CommandParameter("@ModifiedDate", DateTime.Now));
                        Parameter.Add(new CommandParameter("@CancelCaseId", null));
                        Parameter.Add(new CommandParameter("@ProdCode", acc.ProdCode));
                        Parameter.Add(new CommandParameter("@Link", acc.Link));
                        Parameter.Add(new CommandParameter("@SegmentCode", acc.SegmentCode));
                        Parameter.Add(new CommandParameter("@CancelAmount", acc.SeizureAmount));
                        Parameter.Add(new CommandParameter("@TripAmount", null));
                        Parameter.Add(new CommandParameter("@OtherSeizure", acc.OtherSeizure));
                        Parameter.Add(new CommandParameter("@Seq", acc.Seq));
                        Parameter.Add(new CommandParameter("@SeizureAmountSUB", 0.0m));
                        Parameter.Add(new CommandParameter("@TxtStatus", "1"));
                        Parameter.Add(new CommandParameter("@TxtProdCode", acc.ProdCode));
                        ExecuteNonQuery(sql);  
                        #endregion
                        }
                        catch (Exception ex)
                        {
                            
                            throw;
                        }




                    }
                    string sql1 = @"UPDATE CaseSeizure SET CancelCaseId=@CancelCaseId,CancelAmount=SeizureAmount,SeizureStatus='2'  WHERE CaseId=@CaseId";
                    Parameter.Clear();
                    Parameter.Add(new CommandParameter("@CancelCaseId", casedoc.CaseId));                    
                    Parameter.Add(new CommandParameter("@CaseId", OrginCaseDoc.CaseId));
                    ExecuteNonQuery(sql1); 
         

            }
            else // 
            {
                    foreach (var acc in AccLists)
                    {

                        #region add caseSeizure
                        string sql = @"INSERT INTO  CaseSeizure (CaseId, PayCaseId, CaseNo, CustId, CustName, BranchNo, BranchName, Account, AccountStatus, Currency, Balance, SeizureAmount, ExchangeRate, SeizureAmountNtd, PayAmount, SeizureStatus, CreatedUser, CreatedDate, ModifiedUser, ModifiedDate, CancelCaseId, ProdCode, Link, SegmentCode, CancelAmount, TripAmount, OtherSeizure, Seq, SeizureAmountSUB, TxtStatus, TxtProdCode) 
                                VALUES ( @CaseId, @PayCaseId, @CaseNo, @CustId, @CustName, @BranchNo, @BranchName, @Account, @AccountStatus, @Currency, @Balance, @SeizureAmount, @ExchangeRate,
                                        @SeizureAmountNtd, @PayAmount, @SeizureStatus, @CreatedUser, @CreatedDate, @ModifiedUser, @ModifiedDate, 
                                    @CancelCaseId, @ProdCode, @Link, @SegmentCode, @CancelAmount, @TripAmount, @OtherSeizure, @Seq, @SeizureAmountSUB, @TxtStatus, @TxtProdCode)";
                        Parameter.Clear();
                        Parameter.Add(new CommandParameter("@CaseId", casedoc.CaseId));
                        Parameter.Add(new CommandParameter("@PayCaseId", null));
                        Parameter.Add(new CommandParameter("@CaseNo", casedoc.CaseNo));
                        Parameter.Add(new CommandParameter("@CustId", acc.CustId));
                        Parameter.Add(new CommandParameter("@CustName", acc.CustName));
                        Parameter.Add(new CommandParameter("@BranchNo", acc.BranchNo));
                        Parameter.Add(new CommandParameter("@BranchName", acc.BranchName));
                        Parameter.Add(new CommandParameter("@Account", acc.Account));
                        Parameter.Add(new CommandParameter("@AccountStatus", acc.AccountStatus));
                        Parameter.Add(new CommandParameter("@Currency", acc.Currency));
                        Parameter.Add(new CommandParameter("@Balance", acc.Balance.ToString()));
                        Parameter.Add(new CommandParameter("@SeizureAmount", acc.SeizureAmount));
                        Parameter.Add(new CommandParameter("@ExchangeRate", acc.ExchangeRate));
                        Parameter.Add(new CommandParameter("@SeizureAmountNtd", acc.SeizureAmountNtd));
                        Parameter.Add(new CommandParameter("@PayAmount", 0.0m));
                        Parameter.Add(new CommandParameter("@SeizureStatus", "2"));
                        Parameter.Add(new CommandParameter("@CreatedUser", userid));
                        Parameter.Add(new CommandParameter("@CreatedDate", DateTime.Now));
                        Parameter.Add(new CommandParameter("@ModifiedUser", userid));
                        Parameter.Add(new CommandParameter("@ModifiedDate", DateTime.Now));
                        Parameter.Add(new CommandParameter("@CancelCaseId", null));
                        Parameter.Add(new CommandParameter("@ProdCode", acc.ProdCode));
                        Parameter.Add(new CommandParameter("@Link", acc.Link));
                        Parameter.Add(new CommandParameter("@SegmentCode", acc.SegmentCode));
                        Parameter.Add(new CommandParameter("@CancelAmount",  0.0m));
                        Parameter.Add(new CommandParameter("@TripAmount", null));
                        Parameter.Add(new CommandParameter("@OtherSeizure", acc.OtherSeizure));
                        Parameter.Add(new CommandParameter("@Seq", acc.Seq));
                        Parameter.Add(new CommandParameter("@SeizureAmountSUB", 0.0m));
                        Parameter.Add(new CommandParameter("@TxtStatus", "0"));
                        Parameter.Add(new CommandParameter("@TxtProdCode", acc.ProdCode));
                        ExecuteNonQuery(sql);
                        #endregion

                    }


                    string sql1 = @"UPDATE CaseSeizure SET CancelCaseId=@CancelCaseId,CancelAmount=SeizureAmount,SeizureStatus='2'  WHERE CaseId=@CaseId";
                    Parameter.Clear();
                    Parameter.Add(new CommandParameter("@CancelCaseId", casedoc.CaseId));                    
                    Parameter.Add(new CommandParameter("@CaseId", OrginCaseDoc.CaseId));
                    ExecuteNonQuery(sql1); 



            }
        }

        internal void updateLastAssign(AgentSetting nextAgent)
        {
            string sql = @"UPDATE PARMCode SET CodeDesc=@CodeDesc WHERE CodeType='AssignLast'; ";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("@CodeDesc", nextAgent.EmpId));

            ExecuteNonQuery(sql);
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
                if (ret.Rows.Count > 0)
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




        internal CaseMaster getCaseMaster(Guid OrginalCaseid)
        {

            try
            {
                string sql1 = "SELECT TOP 1 * from CaseMaster WHERE CaseId=@CaseId;";

                Parameter.Clear();
                Parameter.Add(new CommandParameter("@CaseId", OrginalCaseid));

                return SearchList<CaseMaster>(sql1).FirstOrDefault();
            }
            catch (Exception)
            {
                
                throw;
            }       
        }

        internal Dictionary<string, string> getObligor(Guid CaseID)
        {

            try
            {
                string sql1 = "SELECT * from CaseObligor WHERE CaseId=@CaseId;";
                Parameter.Clear();
                Parameter.Add(new CommandParameter("@CaseId", CaseID));
                var ret = SearchList<CaseObligor>(sql1);
                if (ret.Count>0)
                    return ret.ToDictionary(x => x.ObligorNo, x => x.ObligorName);
                else
                    return null;
            }
            catch (Exception)
            {

                throw;
            }  
        }

        internal void updateCaseMasterStatus(Guid caseid, string p)
        {
            string sql = @"UPDATE CaseMaster SET Status=@Status,ReturnReason='自動撤銷' WHERE CaseId=@CaseId; ";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("@Status", p));
            Parameter.Add(new CommandParameter("@CaseId", caseid));

            ExecuteNonQuery(sql);

        }

        internal void updateCaseMasterAgentUser(Guid CaseId, string eQueryStaff)
        {

            string sql = @"UPDATE CaseMaster SET AgentUser=@AgentUser,AssignPerson=@AssignPerson WHERE CaseId=@CaseId; ";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("@AgentUser", eQueryStaff));
            Parameter.Add(new CommandParameter("@AssignPerson", eQueryStaff));
            Parameter.Add(new CommandParameter("@CaseId", CaseId));

            ExecuteNonQuery(sql);

        }

        internal List<CaseObligor> getValidObligor(Guid CaseID)
        {
            try
            {
                string sql1 = "SELECT * from CaseObligor WHERE CaseId=@CaseId;";
                Parameter.Clear();
                Parameter.Add(new CommandParameter("@CaseId", CaseID));
                var ret = SearchList<CaseObligor>(sql1);

                return ret.ToList();
            }
            catch (Exception)
            {

                throw;
            }  
        }

        //建檔日期後, 有沒有法院來的扣押.... 
        internal List<CaseMaster> getOtherSeizure(string obligorNo, string CreatedDate)
        {
            //var caseall = (from p in ctx.CaseMaster join q in ctx.CaseObligor on p.CaseId equals q.CaseId
            //                where q.ObligorNo == v.ObligorNo && p.CreatedDate>= OrginalCase.CreatedDate && p.GovUnit.Contains("地方法院")
            //                select p).ToList();
            string sql = @"Select m.* from CaseMaster m INNER JOIN CaseObligor o on m.CaseId=o.Caseid
                               WHERE o.ObligorNo=@obligorNo AND m.CreatedDate >=@CreatedDate AND m.GovUnit like '%地方法院%';";

            Parameter.Clear();
            Parameter.Add(new CommandParameter("@obligorNo", obligorNo));
            Parameter.Add(new CommandParameter("@CreatedDate", DateTime.Parse( CreatedDate)));

            return SearchList<CaseMaster>(sql).ToList();

        }

        internal List<CaseSeizure> getCaseSeizure(Guid CaseId)
        {
            //ctx.CaseSeizure.Where(x => x.SeizureStatus == "0" && x.CaseId == OrginalCase.CaseId).ToList();
            string sql = @"Select * from CaseSeizure WHERE CaseId=@CaseId AND SeizureStatuso='0';";

            Parameter.Clear();
            Parameter.Add(new CommandParameter("@CaseId", CaseId));


            return SearchList<CaseSeizure>(sql).ToList();

        }

        internal Dictionary<string, string> getOrginalObligor(Guid CaseId)
        {
            //OrginalObligor = (from p in ctx.CaseSeizure
            //                  where p.CaseId == OrginalCase.CaseId
            //                  group p by new { p.CustId, p.CustName } into g
            //                  select new { g.Key.CustId, g.Key.CustName }).ToDictionary(x => x.CustId, x => x.CustName);

            string sql = @"Select CustId,CustName from CaseSeizure WHERE CaseId=@CaseId GROUP BY CustId,CustName";

            Parameter.Clear();
            Parameter.Add(new CommandParameter("@CaseId", CaseId));
            var ret = Search(sql);

            if (ret.Rows.Count > 0)
            {
                Dictionary<string, string> result = new Dictionary<string, string>();
                foreach(DataRow dr in ret.Rows)
                {
                    result.Add(dr[0].ToString(), dr[1].ToString());
                }
                return result;
            }
            else
                return null;


            
        }

        internal int getCaseSeizureCount(Guid CaseId)
        {
            //ctx.CaseSeizure.Where(x => x.CaseId == OrginalCase.CaseId).Count();
            string sql = @"Select count(*) from CaseSeizure WHERE CaseId=@CaseId";

            Parameter.Clear();
            Parameter.Add(new CommandParameter("@CaseId", CaseId));
            var ret = Search(sql);
            if (ret.Rows.Count > 0)
            {
                return int.Parse(ret.Rows[0][0].ToString());
            }
            else
                return 0;

        }

        internal EDocTXT2 getEDocTXT2(Guid CaseId)
        {
            // ctx.EDocTXT2.Where(x => x.CaseId == caseDoc.CaseId).FirstOrDefault();
            string sql = @"Select TOP 1 *  from EDocTXT2 WHERE CaseId=@CaseId";

            Parameter.Clear();
            Parameter.Add(new CommandParameter("@CaseId", CaseId));
            var ret = SearchList<EDocTXT2>(sql).FirstOrDefault();
            return ret;

        }

        internal List<CaseMaster> getCaseMasterByGovNoNDate(string OrginalGovNo, DateTime OrginalGovDate)
        {
            //ctx.CaseMaster.Where(x => x.GovNo == OrginalGovNo && x.GovDate == OrginalGovDate).ToList();
            string sql = @"Select *  from CaseMaster WHERE GovNo=@GovNo AND GovDate=@GovDate";

            Parameter.Clear();
            Parameter.Add(new CommandParameter("@GovNo", OrginalGovNo));
            Parameter.Add(new CommandParameter("@GovDate", OrginalGovDate));
            var ret = SearchList<CaseMaster>(sql).ToList();
            return ret;
        }

        internal List<CaseSeizure> getCaseSeizureByCaseId(Guid CaseId)
        {
            //ctx.CaseSeizure.Where(x => x.CaseId == OrginalCase.CaseId).ToList();
            string sql = @"Select *  from CaseSeizure WHERE CaseId=@CaseId";

            Parameter.Clear();
            Parameter.Add(new CommandParameter("@CaseId", CaseId));

            var ret = SearchList<CaseSeizure>(sql).ToList();
            return ret;

        }

        internal List<CaseMemo> getCaseMemoById(Guid CaseId)
        {
            //ctx.CaseMemo.Where(x => x.CaseId == OrginalCase.CaseId).ToList();
            string sql = @"Select *  from CaseMemo WHERE CaseId=@CaseId";

            Parameter.Clear();
            Parameter.Add(new CommandParameter("@CaseId", CaseId));

            var ret = SearchList<CaseMemo>(sql).ToList();
            return ret;

        }

        internal CaseMaster getCaseMemoById_lastest(Guid? CaseId)
        {
            //ctx.CaseMaster.Where(x => x.CaseId == oneSei.CancelCaseId).OrderByDescending(x => x.CreatedDate).FirstOrDefault();

            string sql = @"Select TOP 1 *  from CaseMaster WHERE CaseId=@CaseId ORDER BY CreatedDate DESC";

            Parameter.Clear();
            Parameter.Add(new CommandParameter("@CaseId", CaseId));

            var ret = SearchList<CaseMaster>(sql).FirstOrDefault();
            return ret;
        }

        internal void updateAutoLog(long Id, int Status, string ErrorMessage)
        {
            string sql = @"UPDATE AutoLog SET Status=@Status,ErrorMsg=@ErrorMsg WHERE Id=@Id; ";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("@Id", Id));
            Parameter.Add(new CommandParameter("@Status", Status));
            Parameter.Add(new CommandParameter("@ErrorMsg", ErrorMessage));

            ExecuteNonQuery(sql);

        }

        internal CaseMaster getPayCase(Guid? CaseId)
        {
            //ctx.CaseMaster.Where(x => x.CaseId == oPay.PayCaseId).OrderByDescending(x => x.CreatedDate).FirstOrDefault();
            string sql = @"Select TOP 1 *  from CaseMaster WHERE CaseId=@CaseId ORDER BY CreatedDate DESC";

            Parameter.Clear();
            Parameter.Add(new CommandParameter("@CaseId", CaseId));

            var ret = SearchList<CaseMaster>(sql).FirstOrDefault();
            return ret;

        }
    }
}
