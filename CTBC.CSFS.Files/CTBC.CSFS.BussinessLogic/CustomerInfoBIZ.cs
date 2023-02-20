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
    public class CustomerInfoBIZ : CommonBIZ
    {
        public TxCustomerInfo GetData(string customerId,Guid caseId)
        {
            try
            {
                TxCustomerInfo rtn = new TxCustomerInfo
                {
                    Tx60491Grp = GetLatestTx60491Grp(customerId,caseId),
                    Tx60491Detls = new List<TX_60491_Detl>(),
                    Tx67072Grp = GetLatestTx67072Grp(customerId, caseId),
                    Tx67072Detls = new List<TX_67072_Detl>(),
                    //20160122 RC --> 20150115 宏祥 add 新增67100電文
                    Tx67100 = GetLatestTx67100(customerId)
                };
                if (rtn.Tx60491Grp != null)
                    rtn.Tx60491Detls = GetTx60491Detls(rtn.Tx60491Grp.SNO);
                else
                    rtn.ErrMsg = Lang.csfs_no_data;

                if (rtn.Tx67072Grp != null)
                    rtn.Tx67072Detls = GetTx67072Detls(rtn.Tx67072Grp.SNO);
                else
                    rtn.Tx67072Grp = new TX_67072_Grp();

                //20160122 RC --> 20150115 宏祥 add 新增67100電文
                if (rtn.Tx67100 == null)
                    rtn.Tx67100 = new TX_67100();
                
                return rtn;
            }
            catch (Exception ex)
            {
                return new TxCustomerInfo {ErrMsg = ex.Message};
            }
        }

        public TX_60491_Grp GetLatestTx60491Grp(string customerId,Guid caseId)
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
                            FROM [TX_60491_Grp]
                            WHERE [CustomerId] = @CustomerId and [CaseId]= @CaseId and CONVERT(varchar(10), cCretDT, 23) >= @Checkdate
                            ORDER BY [SNO] DESC;";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("CustomerId", customerId));
            Parameter.Add(new CommandParameter("CaseId", caseId));// 20160324
            Parameter.Add(new CommandParameter("Checkdate", strCheckdate));
            var rtn = SearchList<TX_60491_Grp>(strSql);
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
                            FROM [TX_60491_Grp]
                            WHERE [CustomerId] = @CustomerId and CONVERT(varchar(10), cCretDT, 23) < @Checkdate
                            ORDER BY [SNO] DESC;";
                Parameter.Clear();
                Parameter.Add(new CommandParameter("CustomerId", customerId));
                Parameter.Add(new CommandParameter("Checkdate", strCheckdate));
                rtn = SearchList<TX_60491_Grp>(strSql);
            }
            return rtn == null || !rtn.Any() ? null : rtn.FirstOrDefault();
        }

        public IList<TX_60491_Detl> GetTx60491Detls(int sno)
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
                                FROM [TX_60491_Detl] AS D
                                LEFT OUTER JOIN [PARMCode] AS P ON P.[CodeType]= 'RCAF_BRANCH' AND P.[CodeNo] = D.[Branch]
                                WHERE [FKSNO] = @FKSno";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("FKSno", sno));
            return SearchList<TX_60491_Detl>(strSql);
        }

        private TX_67072_Grp GetLatestTx67072Grp(string customerId, Guid caseId)             //adam 20160427
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
                              FROM [TX_67072_Grp]
                              WHERE [CustIdNo] = @CustNo and [CaseId]= @CaseId and CONVERT(varchar(10), cCretDT, 23) >= @Checkdate
                              ORDER BY [SNO] DESC";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("CustNo", customerId));
            //adam 20160427
            Parameter.Add(new CommandParameter("CaseId", caseId));
            Parameter.Add(new CommandParameter("Checkdate", strCheckdate));
            //adam 20160427
            var rtn = SearchList<TX_67072_Grp>(strSql);
            if ( rtn.Count == 0)
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
                              FROM [TX_67072_Grp]
                              WHERE [CustIdNo] = @CustNo and CONVERT(varchar(10), cCretDT, 23) < @Checkdate
                              ORDER BY [SNO] DESC";
                Parameter.Clear();
                Parameter.Add(new CommandParameter("CustNo", customerId));
                //adam 20160427
                Parameter.Add(new CommandParameter("Checkdate", strCheckdate));
                //adam 20160427
                rtn = SearchList<TX_67072_Grp>(strSql);
            }
            return rtn == null || !rtn.Any() ? null : rtn.FirstOrDefault();
        }

        private IList<TX_67072_Detl> GetTx67072Detls(int sno)
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
            return SearchList<TX_67072_Detl>(strSql);
        }

        public TX_33401 GetLatestTx33401(string account, Guid caseId) //adam 20160427
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
                              FROM [TX_33401]
                              WHERE RIGHT([Acct],12) = @Acct and [CaseId]= @CaseId and CONVERT(varchar(10), cCretDT, 23) >= @Checkdate
                              ORDER BY SNO DESC";
            Parameter.Clear();

            string str = account.PadLeft(12,'0');
            str = str.Substring(str.Length - 12);
            Parameter.Add(new CommandParameter("Acct", str));
            //adam 20160427
            Parameter.Add(new CommandParameter("CaseId", caseId)); 
            Parameter.Add(new CommandParameter("Checkdate", strCheckdate));
            //adam 20160427
            var rtn = SearchList<TX_33401>(strSql);
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
                              FROM [TX_33401]
                              WHERE RIGHT([Acct],12) = @Acct and CONVERT(varchar(10), cCretDT, 23) < @Checkdate
                              ORDER BY SNO DESC";
                Parameter.Clear();

                str = account.PadLeft(12, '0');
                str = str.Substring(str.Length - 12);
                Parameter.Add(new CommandParameter("Acct", str));
                //adam 20160427
                Parameter.Add(new CommandParameter("Checkdate", strCheckdate));
                //adam 20160427
                rtn = SearchList<TX_33401>(strSql);
            }
            return rtn == null || !rtn.Any() ? null : rtn.FirstOrDefault();
        }

        /// <summary>
        /// 取得67100電文資料
        /// </summary>
        /// <param name="customerId"></param>
        /// <returns></returns>
        /// <remarks>20160122 RC --> 20150115 宏祥 add 新增67100電文</remarks>
        public TX_67100 GetLatestTx67100(string customerId)
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
                            FROM [TX_67100]
                            WHERE [CustId] = @CustomerId
                            ORDER BY [CifNo] DESC;";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("CustomerId", customerId));
            var rtn = SearchList<TX_67100>(strSql);
            return rtn == null || !rtn.Any() ? null : rtn.FirstOrDefault();
        }
        /// <summary>
        /// 取得TX00450電文資料.(事故資訊WXOption='30',他案扣押WXOption='31')
        /// </summary>
        /// <param name="Account">The account.</param>
        /// <param name="caseId">The caseId.</param>
        /// <param name="WXOption">The WXOption.</param>
        /// <returns></returns>
        public TX_00450 GetLatestTx00450(string Account, Guid caseId, string WXOption)
        {
            string strSql = @"select * from TX_00450 where caseid=@caseId and WXOption=@WXOption and Account like @Account order by cCretDT desc ";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("Account", "%" + Account + "%"));
            Parameter.Add(new CommandParameter("caseId", caseId));
            Parameter.Add(new CommandParameter("WXOption", WXOption));
            var rtn = SearchList<TX_00450>(strSql);
            return rtn == null || !rtn.Any() ? null : rtn.FirstOrDefault();
        }
    }
}
