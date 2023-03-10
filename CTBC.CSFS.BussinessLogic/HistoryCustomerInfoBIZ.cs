using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CTBC.CSFS.Models;
using CTBC.CSFS.Pattern;
using CTBC.CSFS.Resource;
using CTBC.CSFS.ViewModels;
using NPOI.OpenXmlFormats.Dml;

namespace CTBC.CSFS.BussinessLogic
{
    public class HistoryCustomerInfoBIZ : CommonBIZ
    {
        public HistoryTxCustomerInfo GetData(string customerId,Guid caseId)
        {
            try
            {
                HistoryTxCustomerInfo rtn = new HistoryTxCustomerInfo
                {
                    HistoryTx60491_Grp = GetLatestTx60491Grp(customerId, caseId),
                    HistoryTx60491Detls = new List<HistroyTX_60491_Detl>(),
                    HistoryTx60491DetlIdDupDatas = new List<HistroyTX_60491_Detl>(),
                    HistoryTx67072_Grp = GetLatestTx67072Grp(customerId, caseId),
                    HistoryTx67072Detls = new List<HistoryTX_67072_Detl>(),
                    //20160122 RC --> 20150115 宏祥 add 新增67100電文
                    HistoryTx67100 = GetLatestTx67100(customerId),
                    //20180709 新增67002電文
                    //Tx67002 = GetLatestTx67002(caseId, rtn.Tx60491Grp.CustomerName)
                };

                rtn.HistoryTx67002 = GetLatestTx67002(caseId, rtn.HistoryTx60491Grp.CustomerName);
                if (rtn.HistoryTx60491Grp != null)
                    rtn.HistoryTx60491Detls = GetTx60491Detls(rtn.HistoryTx60491Grp.SNO);
                else
                    rtn.ErrMsg = Lang.csfs_no_data;
                #region 找ID重號的資料
                string customerIdnew = customerId.Length > 10 ? customerId.Substring(0, 10) : customerId;
                IList<HistoryTX_60491_Grp> Tx60491Grps = GetTx60491Grps(customerId, caseId, customerIdnew);
                if (Tx60491Grps != null && Tx60491Grps.Any())
                {
                    Tx60491Grps = Tx60491Grps.GroupBy(a => a.CustomerId).Select(g => g.First()).ToList();
                    IList<HistroyTX_60491_Detl> tx60491N = new List<HistroyTX_60491_Detl>();
                    if (Tx60491Grps != null && Tx60491Grps.Any())
                    {
                        foreach (var item in Tx60491Grps)
                        {
                            tx60491N = GetTx60491Detls(item.SNO);
                            if (tx60491N != null && tx60491N.Any())
                            {
                                foreach (HistroyTX_60491_Detl detail in tx60491N)
                                {
                                    rtn.HistoryTx60491DetlIdDupDatas.Add(detail);
                                }
                            }
                        }
                    }
                }
                #endregion

                #region 外幣活存要砍掉右邊3碼  IR-1019
                PARMCodeBIZ pbiz = new PARMCodeBIZ();
                string foreignCcy = pbiz.GetParmCodeByCodeType("SeizureSeqence").Where(m => m.CodeDesc == "外幣活存").FirstOrDefault().CodeMemo;//外幣活存的產品代碼
                foreach (HistroyTX_60491_Detl item in rtn.HistoryTx60491Detls)
                {
                    if (!string.IsNullOrEmpty(item.ProdCode.Trim()) && item.ProdCode.Length > 2)//TxtProdCode排空才能Substring
                    {
                        if (item.Ccy.ToUpper() != "TWD" && item.Account.Length >= 15 && foreignCcy.Contains(item.ProdCode.Substring(2)))
                        {
                            item.Account = item.Account.Substring(0, item.Account.Length - 3);
                        }
                    }
                }
                foreach (HistroyTX_60491_Detl item in rtn.HistoryTx60491DetlIdDupDatas)//ID重號資料,外幣活存要砍掉右邊3碼
                {
                    if (!string.IsNullOrEmpty(item.ProdCode.Trim()) && item.ProdCode.Length > 2)//TxtProdCode排空才能Substring
                    {
                        if (item.Ccy.ToUpper() != "TWD" && item.Account.Length >= 15 && foreignCcy.Contains(item.ProdCode.Substring(2)))
                        {
                            item.Account = item.Account.Substring(0, item.Account.Length - 3);
                        }
                    }
                }
                #endregion

                //ID重號查詢，如果Tx67072Grp無資料(IdDupFlag欄位為空，則顯示N)時，去查TX_60629，如果ID_DATA_2有值，表明有重號，顯示Y，否則顯示N
                if (rtn.HistoryTx67072_Grp != null)
                {
                    rtn.HistoryTx67072Detls = GetTx67072Detls(rtn.HistoryTx67072_Grp.SNO);
                    if(string.IsNullOrEmpty(rtn.HistoryTx67072_Grp.IdDupFlag))
                    {
                        rtn.HistoryTx67072_Grp.IdDupFlag = "N";
                    }
                }
                else
                {
                    rtn.HistoryTx67072_Grp = new HistoryTX_67072_Grp();
                    HistoryTX_60629 Tx60629 = GetLatestTx60629(caseId);
                    if (Tx60629 != null && !string.IsNullOrEmpty(Tx60629.ID_DATA_2))
                        rtn.HistoryTx67072_Grp.IdDupFlag = "Y";
                    else
                        rtn.HistoryTx67072_Grp.IdDupFlag = "N";
                }
                    

                //20160122 RC --> 20150115 宏祥 add 新增67100電文
                if (rtn.HistoryTx67100 == null)
                    rtn.HistoryTx67100 = new HistoryTX_67100();

                if (rtn.HistoryTx67002 == null)
                    rtn.HistoryTx67002 = new HistoryTX_67002();

                return rtn;
            }
            catch (Exception ex)
            {
                return new HistoryTxCustomerInfo {ErrMsg = ex.Message};
            }
        }
        public IList<HistoryTX_60491_Grp> GetTx60491Grps(string customerId, Guid caseId, string customerIdnew)
        {
            base.Parameter.Clear();
            base.Parameter.Add(new CommandParameter("@CodeType", "CheckDate"));
            DataTable dt = base.Search("select * from PARMCode where CodeType = @CodeType");
            string strCheckdate = "2016-03-31";
            if (dt.Rows.Count > 0)
            {
                strCheckdate = dt.Rows[0]["CodeDesc"].ToString();
            }

            string strSql = @"SELECT [SNO],[RspCode],[TrnNum],[ErrType],[RspMessage],[CustomerNo],[CustType],[RMNum],[TelDay],[TelDayExt],[CustomerId],[BirthDt],[TelNig],[TelNigExt],[CustomerName]
                            ,[Addr1],[TrustOneAppl],[TrustOneActual],[Addr2],[Rank],[Amt],[Addr3],[NetAsset],[DepTot],[NoOfCards],[MutTot],[LonTot],[CardLimit],[WmAssetAmt],[MobilNo],[Email],[OcpnDesc]
                            ,[SelectNo],[KeepSts],[KeepOpt],[Action],[KeepReadFlag],[MutltFlag],[CardFlag],[KeepEnqClsDate],[VIPCode],[Contrib],[VipDegree],[FbAoBranch],[FbTeller],[KeepCurrency]
                            ,[KeepRecno],[KeepWaIdx],[ServiceCode1],[FbAoCode],[RiskAttrib],[VipCdI],[VipCdH],[HighContr],[InputMsgType],[HouseholdFlag],[TrialFlag],[AssetVar],[LgmbFlag]
                            ,[FundCif],[EnqOpt],[MnthsSnc],[VipDegreeH],[OldFlag],[SboxFlag],[cCretDT],isnull(@CaseId,'00000000-0000-0000-0000-000000000000') as CaseId                              
                            FROM [History_TX_60491_Grp]
                            WHERE [CustomerId] like @CustomerIdnew and [CustomerId] != @CustomerId and [CaseId]= @CaseId and CONVERT(varchar(10), cCretDT, 23) >= @Checkdate
                            ORDER BY [SNO] DESC;";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("CustomerIdnew", "%" + customerIdnew + "%"));
            Parameter.Add(new CommandParameter("CustomerId", customerId));
            Parameter.Add(new CommandParameter("CaseId", caseId));
            Parameter.Add(new CommandParameter("Checkdate", strCheckdate));
            var rtn = SearchList<HistoryTX_60491_Grp>(strSql);
            if (rtn.Count == 0)
            {
                strSql = @"SELECT [SNO],[RspCode],[TrnNum],[ErrType],[RspMessage],[CustomerNo],[CustType],[RMNum],[TelDay],[TelDayExt],[CustomerId],[BirthDt],[TelNig],[TelNigExt],[CustomerName]
                            ,[Addr1],[TrustOneAppl],[TrustOneActual],[Addr2],[Rank],[Amt],[Addr3],[NetAsset],[DepTot],[NoOfCards],[MutTot],[LonTot],[CardLimit],[WmAssetAmt],[MobilNo],[Email],[OcpnDesc]
                            ,[SelectNo],[KeepSts],[KeepOpt],[Action],[KeepReadFlag],[MutltFlag],[CardFlag],[KeepEnqClsDate],[VIPCode],[Contrib],[VipDegree],[FbAoBranch],[FbTeller],[KeepCurrency]
                            ,[KeepRecno],[KeepWaIdx],[ServiceCode1],[FbAoCode],[RiskAttrib],[VipCdI],[VipCdH],[HighContr],[InputMsgType],[HouseholdFlag],[TrialFlag],[AssetVar],[LgmbFlag],[FundCif]
                            ,[EnqOpt],[MnthsSnc],[VipDegreeH],[OldFlag],[SboxFlag],[cCretDT]                          
                            FROM [History_TX_60491_Grp]
                            WHERE [CustomerId] like @CustomerIdnew and [CustomerId] != @CustomerId and CONVERT(varchar(10), cCretDT, 23) < @Checkdate
                            ORDER BY [SNO] DESC;";
                Parameter.Clear();
                Parameter.Add(new CommandParameter("CustomerIdnew", "%" + customerIdnew + "%"));
                Parameter.Add(new CommandParameter("CustomerId", customerId));
                Parameter.Add(new CommandParameter("Checkdate", strCheckdate));
                rtn = SearchList<HistoryTX_60491_Grp>(strSql);
            }
            return rtn == null || !rtn.Any() ? null : rtn;
        }
        public HistoryTX_60491_Grp GetLatestTx60491Grp(string customerId,Guid caseId)
        {
            base.Parameter.Clear();
            base.Parameter.Add(new CommandParameter("@CodeType", "CheckDate"));
            DataTable dt = base.Search("select * from PARMCode where CodeType = @CodeType");
            string  strCheckdate = "2016-03-31";
            if (dt.Rows.Count > 0)
            {
                strCheckdate = dt.Rows[0]["CodeDesc"].ToString();
            }

            string strSql = @"SELECT top 1 [SNO]
                                    ,[RspCode]
                                    ,[TrnNum]
                                    ,[ErrType]
                                    ,[RspMessage]
                                    ,[CustomerNo]
                                    ,[CustType]
                                    ,[RMNum]
                                    ,[TelDay]
                                    ,[TelDayExt]
                                    ,[CustomerId]
                                    ,[BirthDt]
                                    ,[TelNig]
                                    ,[TelNigExt]
                                    ,[CustomerName]
                                    ,[Addr1]
                                    ,[TrustOneAppl]
                                    ,[TrustOneActual]
                                    ,[Addr2]
                                    ,[Rank]
                                    ,[Amt]
                                    ,[Addr3]
                                    ,[NetAsset]
                                    ,[DepTot]
                                    ,[NoOfCards]
                                    ,[MutTot]
                                    ,[LonTot]
                                    ,[CardLimit]
                                    ,[WmAssetAmt]
                                    ,[MobilNo]
                                    ,[Email]
                                    ,[OcpnDesc]
                                    ,[SelectNo]
                                    ,[KeepSts]
                                    ,[KeepOpt]
                                    ,[Action]
                                    ,[KeepReadFlag]
                                    ,[MutltFlag]
                                    ,[CardFlag]
                                    ,[KeepEnqClsDate]
                                    ,[VIPCode]
                                    ,[Contrib]
                                    ,[VipDegree]
                                    ,[FbAoBranch]
                                    ,[FbTeller]
                                    ,[KeepCurrency]
                                    ,[KeepRecno]
                                    ,[KeepWaIdx]
                                    ,[ServiceCode1]
                                    ,[FbAoCode]
                                    ,[RiskAttrib]
                                    ,[VipCdI]
                                    ,[VipCdH]
                                    ,[HighContr]
                                    ,[InputMsgType]
                                    ,[HouseholdFlag]
                                    ,[TrialFlag]
                                    ,[AssetVar]
                                    ,[LgmbFlag]
                                    ,[FundCif]
                                    ,[EnqOpt]
                                    ,[MnthsSnc]
                                    ,[VipDegreeH]
                                    ,[OldFlag]
                                    ,[SboxFlag]
                                    ,[cCretDT]
                                    ,isnull(@CaseId,'00000000-0000-0000-0000-000000000000') as CaseId                              
                            FROM [History_TX_60491_Grp]
                            WHERE [CustomerId] = @CustomerId and [CaseId]= @CaseId and CONVERT(varchar(10), cCretDT, 23) >= @Checkdate
                            ORDER BY [SNO] DESC;";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("CustomerId", customerId));
            Parameter.Add(new CommandParameter("CaseId", caseId));// 20160324
            Parameter.Add(new CommandParameter("Checkdate", strCheckdate));
            var rtn = SearchList<HistoryTX_60491_Grp>(strSql);
            if (rtn.Count == 0)
            {
                strSql = @"SELECT top 1 [SNO]
                                    ,[RspCode]
                                    ,[TrnNum]
                                    ,[ErrType]
                                    ,[RspMessage]
                                    ,[CustomerNo]
                                    ,[CustType]
                                    ,[RMNum]
                                    ,[TelDay]
                                    ,[TelDayExt]
                                    ,[CustomerId]
                                    ,[BirthDt]
                                    ,[TelNig]
                                    ,[TelNigExt]
                                    ,[CustomerName]
                                    ,[Addr1]
                                    ,[TrustOneAppl]
                                    ,[TrustOneActual]
                                    ,[Addr2]
                                    ,[Rank]
                                    ,[Amt]
                                    ,[Addr3]
                                    ,[NetAsset]
                                    ,[DepTot]
                                    ,[NoOfCards]
                                    ,[MutTot]
                                    ,[LonTot]
                                    ,[CardLimit]
                                    ,[WmAssetAmt]
                                    ,[MobilNo]
                                    ,[Email]
                                    ,[OcpnDesc]
                                    ,[SelectNo]
                                    ,[KeepSts]
                                    ,[KeepOpt]
                                    ,[Action]
                                    ,[KeepReadFlag]
                                    ,[MutltFlag]
                                    ,[CardFlag]
                                    ,[KeepEnqClsDate]
                                    ,[VIPCode]
                                    ,[Contrib]
                                    ,[VipDegree]
                                    ,[FbAoBranch]
                                    ,[FbTeller]
                                    ,[KeepCurrency]
                                    ,[KeepRecno]
                                    ,[KeepWaIdx]
                                    ,[ServiceCode1]
                                    ,[FbAoCode]
                                    ,[RiskAttrib]
                                    ,[VipCdI]
                                    ,[VipCdH]
                                    ,[HighContr]
                                    ,[InputMsgType]
                                    ,[HouseholdFlag]
                                    ,[TrialFlag]
                                    ,[AssetVar]
                                    ,[LgmbFlag]
                                    ,[FundCif]
                                    ,[EnqOpt]
                                    ,[MnthsSnc]
                                    ,[VipDegreeH]
                                    ,[OldFlag]
                                    ,[SboxFlag]
                                    ,[cCretDT]                          
                            FROM [History_TX_60491_Grp]
                            WHERE [CustomerId] = @CustomerId and CONVERT(varchar(10), cCretDT, 23) < @Checkdate
                            ORDER BY [SNO] DESC;";
                Parameter.Clear();
                Parameter.Add(new CommandParameter("CustomerId", customerId));
                Parameter.Add(new CommandParameter("Checkdate", strCheckdate));
                rtn = SearchList<HistoryTX_60491_Grp>(strSql);
            }
            return rtn == null || !rtn.Any() ? null : rtn.FirstOrDefault();
        }

        public IList<HistroyTX_60491_Detl> GetTx60491Detls(int sno)
        {
            string strSql = @"SELECT [SNO]
                                    ,[FKSNO]
                                    ,[Account]
                                    ,[Branch]
                                    ,P.[CodeDesc] AS [BranchName]
                                    ,[StsDesc]
                                    ,[ProdCode]
                                    ,[ProdDesc]
                                    ,[Link]
                                    ,[Ccy]
                                    ,[Bal]
                                    ,[System]
                                    ,[SegmentCode]
                                    ,[CUST_ID]
                                    ,isnull([CaseId],'00000000-0000-0000-0000-000000000000') as CaseId
                                FROM [History_TX_60491_Detl] AS D
                                LEFT OUTER JOIN [PARMCode] AS P ON P.[CodeType]= 'RCAF_BRANCH' AND P.[CodeNo] = D.[Branch]
                                WHERE [FKSNO] = @FKSno";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("FKSno", sno));
            return SearchList<HistroyTX_60491_Detl>(strSql);
        }

        public HistoryTX_67072_Grp GetLatestTx67072Grp(string customerId, Guid caseId)             //adam 20160427
        {
            //adam 20160427
            base.Parameter.Clear();
            base.Parameter.Add(new CommandParameter("@CodeType", "CheckDate"));
            DataTable dt = base.Search("select * from PARMCode where CodeType = @CodeType");
            string strCheckdate = "2016-03-31";
            if (dt.Rows.Count > 0)
            {
                strCheckdate = dt.Rows[0]["CodeDesc"].ToString();
            }
            //adam 20160427
            string strSql = @"SELECT top 1 [SNO]
                                  ,[RspCode]
                                  ,[TrnNum]
                                  ,[ErrType]
                                  ,[RspMessage]
                                  ,[Action]
                                  ,[CustIdNo]
                                  ,[CustTitle]
                                  ,[CustNo]
                                  ,[CustomerName]
                                  ,[RelaId]
                                  ,[Filler1]
                                  ,[Filler2]
                                  ,[CorpGrp]
                                  ,[Rank]
                                  ,[Amount]
                                  ,[DisclosAgrmt]
                                  ,[CardLimit]
                                  ,[RestStatus]
                                  ,[CardStatus]
                                  ,[CardPay]
                                  ,[CardTmpLim]
                                  ,[CustLimit]
                                  ,[MaluHLim]
                                  ,[CardAvail]
                                  ,[CustAvail]
                                  ,[MaluHAvl]
                                  ,[CardBorrowAmt]
                                  ,[TotLimit]
                                  ,[LoanBal]
                                  ,[PageCnt]
                                  ,[Select]
                                  ,[ControlArea]
                                  ,[Time]
                                  ,[FxTr]
                                  ,[AmtOpt]
                                  ,[Option]
                                  ,[MailLoanAmt]
                                  ,[MailLoanBal]
                                  ,[CardConn]
                                  ,[WsmtStatus]
                                  ,[VipDegree]
                                  ,[PreAppr]
                                  ,[ContrbCode]
                                  ,[FbAoBranch]
                                  ,[FbAoName]
                                  ,[CashCardCode]
                                  ,[ConsultStatus]
                                  ,[KfFlag]
                                  ,[ClearStep]
                                  ,[IdDupFlag]
                                  ,[cCretDT]
                                  ,isnull(@CaseId,'00000000-0000-0000-0000-000000000000') as CaseId 
                              FROM [History_TX_67072_Grp]
                              WHERE [CustIdNo] = @CustNo and [CaseId]= @CaseId and CONVERT(varchar(10), cCretDT, 23) >= @Checkdate
                              ORDER BY [SNO] DESC";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("CustNo", customerId));
            //adam 20160427
            Parameter.Add(new CommandParameter("CaseId", caseId));
            Parameter.Add(new CommandParameter("Checkdate", strCheckdate));
            //adam 20160427
            var rtn = SearchList<HistoryTX_67072_Grp>(strSql);
            if (rtn.Count == 0)
            {
                strSql = @"SELECT top 1 [SNO]
                                  ,[RspCode]
                                  ,[TrnNum]
                                  ,[ErrType]
                                  ,[RspMessage]
                                  ,[Action]
                                  ,[CustIdNo]
                                  ,[CustTitle]
                                  ,[CustNo]
                                  ,[CustomerName]
                                  ,[RelaId]
                                  ,[Filler1]
                                  ,[Filler2]
                                  ,[CorpGrp]
                                  ,[Rank]
                                  ,[Amount]
                                  ,[DisclosAgrmt]
                                  ,[CardLimit]
                                  ,[RestStatus]
                                  ,[CardStatus]
                                  ,[CardPay]
                                  ,[CardTmpLim]
                                  ,[CustLimit]
                                  ,[MaluHLim]
                                  ,[CardAvail]
                                  ,[CustAvail]
                                  ,[MaluHAvl]
                                  ,[CardBorrowAmt]
                                  ,[TotLimit]
                                  ,[LoanBal]
                                  ,[PageCnt]
                                  ,[Select]
                                  ,[ControlArea]
                                  ,[Time]
                                  ,[FxTr]
                                  ,[AmtOpt]
                                  ,[Option]
                                  ,[MailLoanAmt]
                                  ,[MailLoanBal]
                                  ,[CardConn]
                                  ,[WsmtStatus]
                                  ,[VipDegree]
                                  ,[PreAppr]
                                  ,[ContrbCode]
                                  ,[FbAoBranch]
                                  ,[FbAoName]
                                  ,[CashCardCode]
                                  ,[ConsultStatus]
                                  ,[KfFlag]
                                  ,[ClearStep]
                                  ,[IdDupFlag]
                                  ,[cCretDT]
                              FROM [History_TX_67072_Grp]
                              WHERE [CustIdNo] = @CustNo and CONVERT(varchar(10), cCretDT, 23) < @Checkdate
                              ORDER BY [SNO] DESC";
                Parameter.Clear();
                Parameter.Add(new CommandParameter("CustNo", customerId));
                //adam 20160427
                Parameter.Add(new CommandParameter("Checkdate", strCheckdate));
                //adam 20160427
                rtn = SearchList<HistoryTX_67072_Grp>(strSql);
            }
            return rtn == null || !rtn.Any() ? null : rtn.FirstOrDefault();
        }

        public IList<HistoryTX_67072_Detl> GetTx67072Detls(int sno)
        {
            string strSql = @"SELECT [SNO]
                                    ,[FKSNO]
                                    ,[Branc]
                                    ,[LimNo]
                                    ,[Produ]
                                    ,[AppDat]
                                    ,[ExpDat]
                                    ,[Amt]
                                    ,[Curr]
                                    ,[BalAmt]
                                    ,[Sign]
                                    ,[DbbTyp]
                                    ,[Status]
                                    ,[StopCd]
                                    ,[HoldFlag]
                                    ,[APPLNO]
                                    ,[APPLNOB]
                                    ,[CUST_ID]
                                    ,isnull([CaseId],'00000000-0000-0000-0000-000000000000')
                                FROM [TX_67072_Detl]
                                WHERE [FKSNO] = @FKSNO";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("FKSNO", sno));
            return SearchList<HistoryTX_67072_Detl>(strSql);
        }

        public HistoryTX_33401 GetLatestTx33401(string account, Guid caseId, string ccy) //adam 20160427
        {
            string sqlWhere = "";
            //adam 20160427
            base.Parameter.Clear();
            base.Parameter.Add(new CommandParameter("@CodeType", "CheckDate"));
            DataTable dt = base.Search("select * from PARMCode where CodeType = @CodeType");
            string strCheckdate = "2016-03-31";
            if (dt.Rows.Count > 0)
            {
                strCheckdate = dt.Rows[0]["CodeDesc"].ToString();
            }
            //adam 20160427
            string strSql = @"SELECT [SNO]
                                  ,[RspCode]
                                  ,[TrnNum]
                                  ,[RspMsg]
                                  ,[Acct]
                                  ,[AcctType]
                                  ,[MacValue]
                                  ,[PassbookNo]
                                  ,[Filler]
                                  ,[Name]
                                  ,[Currency]
                                  ,[InterBranch]
                                  ,[CustId]
                                  ,[AcctType2]
                                  ,[AcctCat]
                                  ,[Branch]
                                  ,[Birthday]
                                  ,[Rate]
                                  ,[OpenDate]
                                  ,[HTel]
                                  ,[IntMeth]
                                  ,[CloseDate]
                                  ,[OTel]
                                  ,[TermDay]
                                  ,[IntDate]
                                  ,[IntAcct]
                                  ,[OverDate]
                                  ,[LastDate]
                                  ,[CurBal]
                                  ,[PbBal]
                                  ,[LstTax]
                                  ,[TodayCheck]
                                  ,[LonAmt]
                                  ,[YearAmt]
                                  ,[NextCheck]
                                  ,[TdAmt]
                                  ,[YearTax]
                                  ,[NextAmt]
                                  ,[HoldAmt]
                                  ,[OverAmt]
                                  ,[Micr]
                                  ,[IntAmt]
                                  ,[OverTax]
                                  ,[TrueAmt]
                                  ,[ExOdInt]
                                  ,[TermBasis]
                                  ,[AcctTypeDesc]
                                  ,[AtmHoldAmt]
                                  ,[RollCnt]
                                  ,[AcctStatus1]
                                  ,[AcctStatus2]
                                  ,[StockHoldAmt]
                                  ,[UnclChqUsed]
                                  ,[VipCode]
                                  ,[AssetBranch]
                                  ,[FB_TELLER]
                                  ,[ProcessStsText]
                                  ,[MinRepayDate]
                                  ,[MinRepayAmt]
                                  ,[HTelExt]
                                  ,[FaxNo]
                                  ,[InvestCode]
                                  ,[InvestType]
                                  ,[InvestHoldVal]
                                  ,[YearTaxFcy]
                                  ,[OtherHoldAmt]
                                  ,[VipDegree]
                                  ,[AtmWaiveExpDt]
                                  ,[AActDegree]
                                  ,[NbCusm]
                                  ,[CusmOrAAcDegree]
                                  ,[ARelationType]
                                  ,[SlyWaiveWdCnt]
                                  ,[SlyWaiveTrfCnt]
                                  ,[SlyStfExchgRate]
                                  ,[SlyOwnTrfCnt]
                                  ,[SlyOtherTrfCnt]
                                  ,[VipCDI]
                                  ,[HighContr]
                                  ,[HouseFlag]
                                  ,[TrialFlag]
                                  ,[GlobalHeadCode]
                                  ,[SecurityCompNo]
                                  ,[LgmbFlag]
                                  ,[OUTPUT_ERR_NO]
                                  ,[OUTPUT_ERR_OTHER_TXT]
                                  ,[BrchName]
                                  ,[PublishNo]
                                  ,[NcdAmt]
                                  ,[NpTfrNote]
                                  ,[MdHoldAmt]
                                  ,[PortfolioNo]
                                  ,[CUST_ID]
                                  ,[cCretDT]
                                  ,isnull(@CaseId,'00000000-0000-0000-0000-000000000000') as CaseId 
                              FROM [History_TX_33401]
                              WHERE [Acct] LIKE {0} and [CaseId]= @CaseId and CONVERT(varchar(10), cCretDT, 23) >= @Checkdate ";
            Parameter.Clear();

            string str =  account.PadLeft(12, '0') ;
            //str = str.Substring(str.Length - 12) ;
            //Parameter.Add(new CommandParameter("Acct", str));
            strSql = string.Format(strSql, "'%" + str + "%'");

            //adam 20160427
            Parameter.Add(new CommandParameter("CaseId", caseId)); 
            Parameter.Add(new CommandParameter("Checkdate", strCheckdate));
            //adam 20160427
            if(!string.IsNullOrEmpty(ccy))
            {
                sqlWhere = @" and [Currency] = @Currency ";
                Parameter.Add(new CommandParameter("Currency", ccy));
            }
            strSql += sqlWhere + @" ORDER BY SNO DESC ";
            var rtn = SearchList<HistoryTX_33401>(strSql);
            if (rtn.Count == 0)
            {
                 strSql = @"SELECT [SNO]
                                  ,[RspCode]
                                  ,[TrnNum]
                                  ,[RspMsg]
                                  ,[Acct]
                                  ,[AcctType]
                                  ,[MacValue]
                                  ,[PassbookNo]
                                  ,[Filler]
                                  ,[Name]
                                  ,[Currency]
                                  ,[InterBranch]
                                  ,[CustId]
                                  ,[AcctType2]
                                  ,[AcctCat]
                                  ,[Branch]
                                  ,[Birthday]
                                  ,[Rate]
                                  ,[OpenDate]
                                  ,[HTel]
                                  ,[IntMeth]
                                  ,[CloseDate]
                                  ,[OTel]
                                  ,[TermDay]
                                  ,[IntDate]
                                  ,[IntAcct]
                                  ,[OverDate]
                                  ,[LastDate]
                                  ,[CurBal]
                                  ,[PbBal]
                                  ,[LstTax]
                                  ,[TodayCheck]
                                  ,[LonAmt]
                                  ,[YearAmt]
                                  ,[NextCheck]
                                  ,[TdAmt]
                                  ,[YearTax]
                                  ,[NextAmt]
                                  ,[HoldAmt]
                                  ,[OverAmt]
                                  ,[Micr]
                                  ,[IntAmt]
                                  ,[OverTax]
                                  ,[TrueAmt]
                                  ,[ExOdInt]
                                  ,[TermBasis]
                                  ,[AcctTypeDesc]
                                  ,[AtmHoldAmt]
                                  ,[RollCnt]
                                  ,[AcctStatus1]
                                  ,[AcctStatus2]
                                  ,[StockHoldAmt]
                                  ,[UnclChqUsed]
                                  ,[VipCode]
                                  ,[AssetBranch]
                                  ,[FB_TELLER]
                                  ,[ProcessStsText]
                                  ,[MinRepayDate]
                                  ,[MinRepayAmt]
                                  ,[HTelExt]
                                  ,[FaxNo]
                                  ,[InvestCode]
                                  ,[InvestType]
                                  ,[InvestHoldVal]
                                  ,[YearTaxFcy]
                                  ,[OtherHoldAmt]
                                  ,[VipDegree]
                                  ,[AtmWaiveExpDt]
                                  ,[AActDegree]
                                  ,[NbCusm]
                                  ,[CusmOrAAcDegree]
                                  ,[ARelationType]
                                  ,[SlyWaiveWdCnt]
                                  ,[SlyWaiveTrfCnt]
                                  ,[SlyStfExchgRate]
                                  ,[SlyOwnTrfCnt]
                                  ,[SlyOtherTrfCnt]
                                  ,[VipCDI]
                                  ,[HighContr]
                                  ,[HouseFlag]
                                  ,[TrialFlag]
                                  ,[GlobalHeadCode]
                                  ,[SecurityCompNo]
                                  ,[LgmbFlag]
                                  ,[OUTPUT_ERR_NO]
                                  ,[OUTPUT_ERR_OTHER_TXT]
                                  ,[BrchName]
                                  ,[PublishNo]
                                  ,[NcdAmt]
                                  ,[NpTfrNote]
                                  ,[MdHoldAmt]
                                  ,[PortfolioNo]
                                  ,[CUST_ID]
                                  ,[cCretDT]
                              FROM [History_TX_33401]
                              WHERE RIGHT([Acct],12) = @Acct and CONVERT(varchar(10), cCretDT, 23) < @Checkdate";
                Parameter.Clear();

                str = account.PadLeft(12, '0');
                str = str.Substring(str.Length - 12);
                Parameter.Add(new CommandParameter("Acct", str));
                //adam 20160427
                Parameter.Add(new CommandParameter("Checkdate", strCheckdate));
                //adam 20160427
                if (!string.IsNullOrEmpty(ccy))
                {
                    sqlWhere = @" and [Currency] = @Currency ";
                    Parameter.Add(new CommandParameter("Currency", ccy));
                }
                strSql += sqlWhere + @" ORDER BY SNO DESC ";
                rtn = SearchList<HistoryTX_33401>(strSql);
            }
            return rtn == null || !rtn.Any() ? null : rtn.FirstOrDefault();
        }

        /// <summary>
        /// 取得67100電文資料
        /// </summary>
        /// <param name="customerId"></param>
        /// <returns></returns>
        /// <remarks>20160122 RC --> 20150115 宏祥 add 新增67100電文</remarks>
        public HistoryTX_67100 GetLatestTx67100(string customerId)
        {
            string strSql = @"SELECT [CifNo]
                                    ,[CustId]
                                    ,[IdType]
                                    ,[IdType01]
                                    ,[IdDesc01]
                                    ,[IdNo01]
                                    ,[EffcDateDay01]
                                    ,[ExpiDateDay01]
                                    ,[IdValu01]
                                    ,[Func01]
                                    ,[Ind01]
                                    ,[IdType02]
                                    ,[IdDesc02]
                                    ,[IdNo02]
                                    ,[EffcDateDay02]
                                    ,[ExpiDateDay02]
                                    ,[IdValu02]
                                    ,[Func02]
                                    ,[Ind02]
                                    ,[IdType03]
                                    ,[IdDesc03]
                                    ,[IdNo03]
                                    ,[EffcDateDay03]
                                    ,[ExpiDateDay03]
                                    ,[IdValu03]
                                    ,[Func03]
                                    ,[Ind03]
                                    ,[IdType04]
                                    ,[IdDesc04]
                                    ,[IdNo04]
                                    ,[EffcDateDay04]
                                    ,[ExpiDateDay04]
                                    ,[IdValu04]
                                    ,[Func04]
                                    ,[Ind04]
                                    ,[IdType05]
                                    ,[IdDesc05]
                                    ,[IdNo05]
                                    ,[EffcDateDay05]
                                    ,[ExpiDateDay05]
                                    ,[IdValu05]
                                    ,[Func05]
                                    ,[Ind05]
                                    ,[IdType06]
                                    ,[IdDesc06]
                                    ,[IdNo06]
                                    ,[EffcDateDay06]
                                    ,[ExpiDateDay06]
                                    ,[IdValu06]
                                    ,[Func06]
                                    ,[Ind06]
                                    ,[IdType07]
                                    ,[IdDesc07]
                                    ,[IdNo07]
                                    ,[EffcDateDay07]
                                    ,[ExpiDateDay07]
                                    ,[IdValu07]
                                    ,[Func07]
                                    ,[Ind07]
                                    ,[IdType08]
                                    ,[IdDesc08]
                                    ,[IdNo08]
                                    ,[EffcDateDay08]
                                    ,[ExpiDateDay08]
                                    ,[IdValu08]
                                    ,[Func08]
                                    ,[Ind08]
                                    ,[IdType09]
                                    ,[IdDesc09]
                                    ,[IdNo09]
                                    ,[EffcDateDay09]
                                    ,[ExpiDateDay09]
                                    ,[IdValu09]
                                    ,[Func09]
                                    ,[Ind09]
                                    ,[IdType10]
                                    ,[IdDesc10]
                                    ,[IdNo10]
                                    ,[EffcDateDay10]
                                    ,[ExpiDateDay10]
                                    ,[IdValu10]
                                    ,[Func10]
                                    ,[Ind10]
                                    ,[Pont]
                                    ,[CustName]
                                    ,[cCretDT]
                            FROM [History_TX_67100]
                            WHERE [CustId] = @CustomerId
                            ORDER BY [CifNo] DESC;";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("CustomerId", customerId));
            var rtn = SearchList<HistoryTX_67100>(strSql);
            return rtn == null || !rtn.Any() ? null : rtn.FirstOrDefault();
        }
        /// <summary>
        /// 取得TX00450電文資料.(事故資訊WXOption='30',他案扣押WXOption='31')
        /// </summary>
        /// <param name="Account">The account.</param>
        /// <param name="caseId">The caseId.</param>
        /// <param name="WXOption">The WXOption.</param>
        /// <returns></returns>
        public HistoryTX_00450 GetLatestTx00450(string Account, Guid caseId, string WXOption, string txid = null, string tailNum = null)
        {
            //adam 參數NULL 無法取得
            //string strSql = @"select  TOP 1 * from TX_00450 where  DATA1 like @Txid  and DATA2 like @tailNum and  caseid=@caseId and WXOption=@WXOption and Account like @Account order by SNO desc ";
            //string strSql = @"select  TOP 1 * from TX_00450 where  DATA1 like @Txid  and DATA2 like @tailNum and  caseid=@caseId and WXOption=@WXOption and Account like @Account order by SNO desc ";

            string strSql = @"select  TOP 1 * from History_TX_00450 where  caseid=@caseId and WXOption=@WXOption ";
            Parameter.Clear();
            if (!string.IsNullOrEmpty(Account))
            {
                strSql = strSql + string.Format(" and Account like '%{0}%'", Account);
                //Parameter.Add(new CommandParameter("Account", "%" + Account + "%"));
            }
            if (!string.IsNullOrEmpty(txid))
            {
                strSql = strSql + string.Format(" and DATA1 like '%{0}%'", txid);
                //Parameter.Add(new CommandParameter("Txid", "%" + txid + "%"));
            }
            if (!string.IsNullOrEmpty(tailNum))
            {
                strSql = strSql + string.Format(" and DATA2 like '%{0}%'", tailNum);
                //Parameter.Add(new CommandParameter("tailNum", "%" + tailNum + "%"));
            }
            Parameter.Add(new CommandParameter("caseId", caseId));
            Parameter.Add(new CommandParameter("WXOption", WXOption));
            strSql = strSql + " order by SNO desc ";
            var rtn = SearchList<HistoryTX_00450>(strSql);
            return rtn == null || !rtn.Any() ? null : rtn.FirstOrDefault();
        }
        public HistoryTX_67002 GetLatestTx67002(Guid caseId,string Customname)
        {
            string strSql = @"select * from History_TX_67002 where CaseId = @CaseId and CUST_NAME = @CustName order by SNO asc";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("CaseId", caseId));
            Parameter.Add(new CommandParameter("CustName", Customname));
            var rtn = SearchList<HistoryTX_67002>(strSql);
            return rtn == null || !rtn.Any() ? null : rtn.FirstOrDefault();
        }

        public IList<HistoryTX_00450> GetTx00450List(string Account, Guid caseId, string WXOption ,string ccy )
        {
            // adam 20180728 去除重複

           // string strSql = @"select [cCretDT],caseid,[WXOption],[Account],[DATA1],[DATA2],[DATA3]  into #temp FROM TX_00450  where  caseid=@caseId and WXOption=@WXOption and  DATA1 is not null and SUBSTRING(DATA1,1,11) <> 'END OF TXN' and DATA1 <> ''";
           // string strSql = @"select distinct [CaseId],[WXOption],[Account],[DATA1],[DATA2],[DATA3]  from TX_00450 where  caseid=@caseId and WXOption=@WXOption and  DATA1 is not null and DATA1 <> 'END OF TXN' and DATA1 <> ''";
           // string strSql = @"SELECT  ROW_NUMBER() OVER (PARTITION BY [WXOption],[Account],[DATA1],[DATA2],[DATA3]  ORDER BY [SNO] ASC) AS RowID, [SNO],[CaseId],[WXOption],[Account],[DATA1],[DATA2],[DATA3]
           //                 FROM TX_00450 where  caseid=@caseId and WXOption=@WXOption and  DATA1 is not null and DATA1 <> 'END OF TXN' and DATA1 <> ''";
            string strSql = @"declare @TrnNum varchar(21) select top 1 TrnNum  into #tmp  from History_tx_00450 where caseid=@caseId and  DATA1 is not null and SUBSTRING(DATA1,1,11) <> 'END OF TXN' and DATA1 <> '' and Account like @Account and WXOption=@WXOption  ";
            if (ccy == "TWD")
            {
                strSql = strSql + @"and (RepMessage = @ccy or RepMessage is NULL ) group by TrnNum order by TrnNum desc";
            }
            else
            {
                strSql = strSql + @"and (RepMessage = @ccy) group by TrnNum order by TrnNum desc";
            }

            Parameter.Clear();
            strSql = strSql + @" select @TrnNum = TrnNum from #tmp  select * from  History_tx_00450 where caseid=@caseId and TrnNum = @TrnNum and  DATA1 is not null and SUBSTRING(DATA1,1,11) <> 'END OF TXN' and DATA1 <> '' and Account like @Account and WXOption=@WXOption ";
            if (ccy == "TWD")
            {
                strSql = strSql + @" and (RepMessage = @ccy or RepMessage is NULL ) drop table #tmp";
            }
            else
            {
                strSql = strSql + @" and (RepMessage = @ccy ) drop table #tmp";
            }
            if (string.IsNullOrEmpty(Account))
            {
                Account = "";
            }
            if (string.IsNullOrEmpty(ccy))
            {
                Account = null;
            }
            Parameter.Add(new CommandParameter("Account", "%" + Account.Trim() + "%"));
            Parameter.Add(new CommandParameter("caseId", caseId));
            Parameter.Add(new CommandParameter("WXOption", WXOption));
            Parameter.Add(new CommandParameter("ccy", ccy));
            // adam 20180728 去除重複
            //strSql = strSql + " order by SNO select  [DATA1],[DATA2],[DATA3] from #temp group by [DATA2],[DATA1],[DATA3] drop table #temp ";
            var rtn = SearchList<HistoryTX_00450>(strSql);

            //if (rtn.Count > 0)
            //{
            //    var rtn1 = rtn.Distinct();
            //}
            return rtn == null || !rtn.Any() ? null : rtn;
        }
        public HistoryTX_60629 GetLatestTx60629(Guid caseId)
        {
            string strSql = @"select * from History_TX_60629 where caseid=@caseId order by cCretDT desc";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("caseid", caseId));
            var rtn = SearchList<HistoryTX_60629>(strSql);
            return rtn == null || !rtn.Any() ? null : rtn.FirstOrDefault();
        }
        /// <summary>
        /// 帳務資訊 ID重號、客戶等級、RM/理專. 20181102
        /// </summary>
        /// <param name="customerId">The customer identifier.</param>
        /// <param name="caseId">The case identifier.</param>
        /// <returns></returns>
        public HistoryTxCustomerInfo GetRMData(string customerId, Guid caseId)
        {
            try
            {
                HistoryTxCustomerInfo rtn = new HistoryTxCustomerInfo();
                string customerIdnew = string.Empty;
                if (customerId.Length > 10)
                {
                    customerIdnew = customerId.Substring(0, 10);
                }
                else if (customerId.Length > 8 && customerId.Length < 10)
                {
                    customerIdnew = customerId.Substring(0, 8);
                }
                else
                {
                    customerIdnew = customerId;
                }
                rtn.HistoryTx60491Grp = GetLatestTx60491Grp(customerIdnew, caseId);
                rtn.HistoryTx67002 = GetLatestTx67002(caseId, rtn.HistoryTx60491Grp.CustomerName);
                rtn.HistoryTx67072_Grp = GetLatestTx67072Grp(customerIdnew, caseId);
                
                if (rtn.HistoryTx67002 == null)
                    rtn.HistoryTx67002 = new HistoryTX_67002();

                //ID重號查詢，如果Tx67072Grp無資料(IdDupFlag欄位為空，則顯示N)時，去查TX_60629，如果ID_DATA_2有值，表明有重號，顯示Y，否則顯示N
                if (rtn.HistoryTx67072_Grp != null)
                {
                    if (string.IsNullOrEmpty(rtn.HistoryTx67072_Grp.IdDupFlag))
                    {
                        rtn.HistoryTx67072_Grp.IdDupFlag = "N";
                    }
                }
                else
                {
                    rtn.HistoryTx67072_Grp = new HistoryTX_67072_Grp();
                    HistoryTX_60629 Tx60629 = GetLatestTx60629(caseId);
                    if (Tx60629 != null && !string.IsNullOrEmpty(Tx60629.ID_DATA_2))
                        rtn.HistoryTx67072_Grp.IdDupFlag = "Y";
                    else
                        rtn.HistoryTx67072_Grp.IdDupFlag = "N";
                }

                return rtn;
            }
            catch (Exception ex)
            {
                return new HistoryTxCustomerInfo { ErrMsg = ex.Message };
            }
        }
    }
}
