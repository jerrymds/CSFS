using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CTBC.CSFS.Models;
using CTBC.CSFS.Pattern;
using CTBC.CSFS.ViewModels;
using CTBC.FrameWork.HTG;
using CTBC.FrameWork.Util;
using System.Web.Mvc;
using CTBC.FrameWork.Platform;

namespace CTBC.CSFS.BussinessLogic
{
    public class CaseAccountBiz : CommonBIZ
    {
        #region 扣押

		private string userId;
		public string UserId
		{
			set { userId = value; }
			get { return userId; }
		}

		private string pageFrom;
		public string PageFrom
		{
			set { pageFrom = value; }
			get { return pageFrom; }
		}

		/// <summary>
		/// 通過CaseId取得案件下所有的扣押
		/// </summary>
		/// <param name="caseId"></param>
		/// <param name="trans"></param>
		/// <returns></returns>
		public IList<CaseSeizure> GetCaseSeizure(Guid caseId, IDbTransaction trans = null)
		{
            //string strSql = @";with  T1 as(SELECT s.[SeizureId],s.[CaseId],s.[PayCaseId],s.[CaseNo],s.[CustId],s.[CustName],s.[BranchNo],s.[BranchName],s.[Account],s.[AccountStatus],s.[Currency],s.[Balance],
            //                         s.[SeizureAmount],s.[ExchangeRate],s.[SeizureAmountNtd],s.[PayAmount],s.[SeizureStatus],s.[CreatedUser],s.[CreatedDate],s.[ModifiedUser],s.[ModifiedDate],s.[CancelCaseId]
            //                         ,Convert(Nvarchar(10), m.GovDate,111) as GovDate,m.GovNo,m.GovUnit,s.[ProdCode],s.[Link],s.[SegmentCode],s.[OtherSeizure],s.[TxtStatus],s.Seq,s.[TxtProdCode],ISNULL(d.fksno,9999999) as fksno,ISNULL(d.SNO,9999999) as sno--,g.ObligorId
            //                         from CaseSeizure s  
            //                         left join CaseMaster m on s.CaseId=m.CaseId
            //                         left join  (SELECT top 1 
            //                         ROW_NUMBER() OVER (PARTITION BY [CustomerId] ORDER BY [SNO] DESC) AS RowID, [SNO],[CustomerId],[CustomerName]
            //                         FROM [TX_60491_Grp]
            //                         WHERE TX_60491_Grp.CaseID = @CaseId 
            //                         --and [CustomerId] IN (SELECT [ObligorNo] FROM [CaseObligor] WHERE [CaseId] = @CaseId)
            //                         ) h on h.CustomerId = s.CustId                             
            //                         left join TX_60491_Detl d on s.CustId = d.CUST_ID and s.Account = d.Account and d.FKSNO = h.SNO and d.CaseId = m.CaseId
            //				--inner join CaseObligor g on g.CaseId =m.CaseId and g.ObligorNo = s.CustId
            //                         WHERE s.[CaseId] = @CaseId),
            //				T2 as (select distinct  TXType,ColumnValueBefore,ColumnValueAfter,ColumnID,LinkDataKey from CaseDataLog c1,CaseSeizure
            //				where c1.CaseId=@CaseId and TabName='扣押設定' and TableName='CaseSeizure' and LinkDataKey=cast(SeizureId as nvarchar(100) )
            //					and TXDateTime=(select max(TXDateTime) from CaseDataLog c2 where c1.CaseId=c2.CaseId and c1.TabName=c2.TabName and c1.TableName=c2.TableName and c1.ColumnID=c2.ColumnID and c1.LinkDataKey=c2.LinkDataKey)),
            //				T3 as (select T1.*,
            //				(select case  when ColumnValueBefore<>CustId then 'true' else 'false' end from T2 where ColumnID='CustId' and LinkDataKey=cast(SeizureId as nvarchar(100) )) as CustIdflag,
            //				(select case  when ColumnValueBefore<>CustName then 'true' else 'false' end from T2 where ColumnID='CustName' and LinkDataKey=cast(SeizureId as nvarchar(100)) ) as CustNameflag,
            //				(select case  when ColumnValueBefore<>BranchNo then 'true' else 'false' end from T2 where ColumnID='BranchNo' and LinkDataKey=cast(SeizureId as nvarchar(100))  ) as BranchNoflag,
            //				(select case  when ColumnValueBefore<>BranchName then 'true' else 'false' end from T2 where ColumnID='BranchName' and LinkDataKey=cast(SeizureId as nvarchar(100)) ) as BranchNameflag,
            //				(select case  when ColumnValueBefore<>Account then 'true' else 'false' end from T2 where ColumnID='Account' and LinkDataKey=cast(SeizureId as nvarchar(100)) ) as Accountflag,
            //				(select case  when ColumnValueBefore<>AccountStatus then 'true' else 'false' end from T2 where ColumnID='AccountStatus' and LinkDataKey=cast(SeizureId as nvarchar(100))  ) as AccountStatusflag,
            //				(select case  when ColumnValueBefore<>Currency then 'true' else 'false' end from T2 where ColumnID='Currency' and LinkDataKey=cast(SeizureId as nvarchar(100)) ) as Currencyflag,
            //				(select case  when ColumnValueBefore<>Balance then 'true' else 'false' end from T2 where ColumnID='Balance' and LinkDataKey=cast(SeizureId as nvarchar(100)) ) as Balanceflag,
            //				(select case  when ColumnValueBefore<>cast(SeizureAmount as nvarchar(100)) then 'true' else 'false' end from T2 where ColumnID='SeizureAmount' and LinkDataKey=cast(SeizureId as nvarchar(100))  ) as SeizureAmountflag,
            //				(select case  when ColumnValueBefore<>cast(ExchangeRate as nvarchar(100)) then 'true' else 'false' end from T2 where ColumnID='ExchangeRate' and LinkDataKey=cast(SeizureId as nvarchar(100)) ) as ExchangeRateflag,
            //				(select case  when ColumnValueBefore<>cast(SeizureAmountNtd as nvarchar(100)) then 'true' else 'false' end from T2 where ColumnID='SeizureAmountNtd' and LinkDataKey=cast(SeizureId as nvarchar(100))  ) as SeizureAmountNtdflag,
            //				(select case  when ColumnValueBefore<>ProdCode then 'true' else 'false' end from T2 where ColumnID='ProdCode' and LinkDataKey=cast(SeizureId as nvarchar(100)) ) as ProdCodeflag,
            //                         (select case  when ColumnValueBefore<>TxtProdCode then 'true' else 'false' end from T2 where ColumnID='TxtProdCode' and LinkDataKey=cast(SeizureId as nvarchar(100)) ) as TxtProdCodeflag,
            //				(select case  when ColumnValueBefore<>Link then 'true' else 'false' end from T2 where ColumnID='Link' and LinkDataKey=cast(SeizureId as nvarchar(100)) ) as Linkflag,
            //                         (select case  when ColumnValueBefore<>SegmentCode then 'true' else 'false' end from T2 where ColumnID='SegmentCode' and LinkDataKey=cast(SeizureId as nvarchar(100))  ) as SegmentCodeflag,
            //				(select case  when ColumnValueBefore<>OtherSeizure then 'true' else 'false' end from T2 where ColumnID='OtherSeizure' and LinkDataKey=cast(SeizureId as nvarchar(100))  ) as OtherSeizureflag
            //				from T1)
            //				select * from T3 order by Seq asc";
            string strSql = @";WITH #h  (RowID,SNO,CustomerId,CustomerName) 
            AS 
            ( SELECT top 1 ROW_NUMBER() OVER (PARTITION BY [CustomerId] ORDER BY [SNO] DESC) AS RowID, [SNO],[CustomerId],[CustomerName]
                                        FROM [TX_60491_Grp]
                                        WHERE TX_60491_Grp.CaseID = @CaseId),

            #T2 (TXType,ColumnValueBefore,ColumnValueAfter,ColumnID,LinkDataKey ) 
            AS
            (select distinct  TXType,ColumnValueBefore,ColumnValueAfter,ColumnID,LinkDataKey from CaseDataLog c1,CaseSeizure
            							where c1.CaseId=@CaseId and TabName='扣押設定' and TableName='CaseSeizure' and LinkDataKey=cast(SeizureId as nvarchar(100) )
            								and TXDateTime=(select max(TXDateTime) from CaseDataLog c2 where c1.CaseId=c2.CaseId and c1.TabName=c2.TabName and c1.TableName=c2.TableName and c1.ColumnID=c2.ColumnID and c1.LinkDataKey=c2.LinkDataKey)) 
            ,
            #T1([SeizureId],[CaseId],[PayCaseId],[CaseNo],[CustId],[CustName],[BranchNo],[BranchName],[Account],[AccountStatus],[Currency],[Balance],[SeizureAmount],[ExchangeRate],[SeizureAmountNtd],[PayAmount],[SeizureStatus],[CreatedUser],[CreatedDate],[ModifiedUser],[ModifiedDate],[CancelCaseId]
                                        ,GovDate,GovNo,GovUnit,ProdCode,Link,SegmentCode,OtherSeizure,TxtStatus,Seq,TxtProdCode,fksno,sno)
            AS
            (SELECT s.[SeizureId],s.[CaseId],s.[PayCaseId],s.[CaseNo],s.[CustId],s.[CustName],s.[BranchNo],s.[BranchName],s.[Account],s.[AccountStatus],s.[Currency],s.[Balance],
                                        s.[SeizureAmount],s.[ExchangeRate],s.[SeizureAmountNtd],s.[PayAmount],s.[SeizureStatus],s.[CreatedUser],s.[CreatedDate],s.[ModifiedUser],s.[ModifiedDate],s.[CancelCaseId]
                                        ,Convert(Nvarchar(10), m.GovDate,111) as GovDate,m.GovNo,m.GovUnit,s.[ProdCode],s.[Link],s.[SegmentCode],s.[OtherSeizure],s.[TxtStatus],s.Seq,s.[TxtProdCode],ISNULL(d.fksno,9999999) as fksno,ISNULL(d.SNO,9999999) as sno--,g.ObligorId
                                        from CaseSeizure s  
                                        left join CaseMaster m on s.CaseId=m.CaseId
                                        left join #h on #h.CustomerId = s.CustId                             
                                        left join TX_60491_Detl d on s.CustId = d.CUST_ID and s.Account = d.Account and d.FKSNO = #h.SNO and d.CaseId = m.CaseId
                                        WHERE s.[CaseId] = @CaseId),

            #T3 

            AS 
            (select #T1.*,
            							(select case  when ColumnValueBefore<>CustId then 'true' else 'false' end from #T2 where ColumnID='CustId' and LinkDataKey=cast(SeizureId as nvarchar(100) )) as CustIdflag,
            							(select case  when ColumnValueBefore<>CustName then 'true' else 'false' end from #T2 where ColumnID='CustName' and LinkDataKey=cast(SeizureId as nvarchar(100)) ) as CustNameflag,
            							(select case  when ColumnValueBefore<>BranchNo then 'true' else 'false' end from #T2 where ColumnID='BranchNo' and LinkDataKey=cast(SeizureId as nvarchar(100))  ) as BranchNoflag,
            							(select case  when ColumnValueBefore<>BranchName then 'true' else 'false' end from #T2 where ColumnID='BranchName' and LinkDataKey=cast(SeizureId as nvarchar(100)) ) as BranchNameflag,
            							(select case  when ColumnValueBefore<>Account then 'true' else 'false' end from #T2 where ColumnID='Account' and LinkDataKey=cast(SeizureId as nvarchar(100)) ) as Accountflag,
            							(select case  when ColumnValueBefore<>AccountStatus then 'true' else 'false' end from #T2 where ColumnID='AccountStatus' and LinkDataKey=cast(SeizureId as nvarchar(100))  ) as AccountStatusflag,
            							(select case  when ColumnValueBefore<>Currency then 'true' else 'false' end from #T2 where ColumnID='Currency' and LinkDataKey=cast(SeizureId as nvarchar(100)) ) as Currencyflag,
            							(select case  when ColumnValueBefore<>Balance then 'true' else 'false' end from #T2 where ColumnID='Balance' and LinkDataKey=cast(SeizureId as nvarchar(100)) ) as Balanceflag,
            							(select case  when ColumnValueBefore<>cast(SeizureAmount as nvarchar(100)) then 'true' else 'false' end from #T2 where ColumnID='SeizureAmount' and LinkDataKey=cast(SeizureId as nvarchar(100))  ) as SeizureAmountflag,
            							(select case  when ColumnValueBefore<>cast(ExchangeRate as nvarchar(100)) then 'true' else 'false' end from #T2 where ColumnID='ExchangeRate' and LinkDataKey=cast(SeizureId as nvarchar(100)) ) as ExchangeRateflag,
            							(select case  when ColumnValueBefore<>cast(SeizureAmountNtd as nvarchar(100)) then 'true' else 'false' end from #T2 where ColumnID='SeizureAmountNtd' and LinkDataKey=cast(SeizureId as nvarchar(100))  ) as SeizureAmountNtdflag,
            							(select case  when ColumnValueBefore<>ProdCode then 'true' else 'false' end from #T2 where ColumnID='ProdCode' and LinkDataKey=cast(SeizureId as nvarchar(100)) ) as ProdCodeflag,
                                        (select case  when ColumnValueBefore<>TxtProdCode then 'true' else 'false' end from #T2 where ColumnID='TxtProdCode' and LinkDataKey=cast(SeizureId as nvarchar(100)) ) as TxtProdCodeflag,
            							(select case  when ColumnValueBefore<>Link then 'true' else 'false' end from #T2 where ColumnID='Link' and LinkDataKey=cast(SeizureId as nvarchar(100)) ) as Linkflag,
                                        (select case  when ColumnValueBefore<>SegmentCode then 'true' else 'false' end from #T2 where ColumnID='SegmentCode' and LinkDataKey=cast(SeizureId as nvarchar(100))  ) as SegmentCodeflag,
            							(select case  when ColumnValueBefore<>OtherSeizure then 'true' else 'false' end from #T2 where ColumnID='OtherSeizure' and LinkDataKey=cast(SeizureId as nvarchar(100))  ) as OtherSeizureflag
            							from #T1) 

            select * from #T3 order by Seq asc";
            Parameter.Clear();
			Parameter.Add(new CommandParameter("@CaseId", caseId));
			return trans == null ? SearchList<CaseSeizure>(strSql) : SearchList<CaseSeizure>(strSql, trans);
		}

		public CaseSeizure GetCaseSeizureInfo(Guid caseId, int seizureId)
		{
			string strSql = @"select * from CaseSeizure where CaseId = @CaseId and SeizureId=@SeizureId";
			Parameter.Clear();
			Parameter.Add(new CommandParameter("@CaseId", caseId));
			Parameter.Add(new CommandParameter("@SeizureId", seizureId));
			return SearchList<CaseSeizure>(strSql)[0];
		}

		//新增操作
        public void Add(Guid CaseId, CaseSeizure formItem, Guid TXSNO, DateTime TXDateTime, string TXType = null)
		{
			CaseDataLog log = new CaseDataLog();

			#region 新增
            if (!string.IsNullOrEmpty(TXType))
            {
                log.TXType = TXType;
            }
            else
            {
                log.TXType = "新增";
            }
			LdapEmployeeBiz empBiz = new LdapEmployeeBiz();
			LDAPEmployee empNow = empBiz.GetLdapEmployeeByEmpId(UserId);
			log.TXUser = empNow != null && !string.IsNullOrEmpty(empNow.EmpId) ? empNow.EmpId : " ";
			log.TXUserName = empNow != null && !string.IsNullOrEmpty(empNow.EmpName) ? empNow.EmpName : " ";
			if (PageFrom == "1")
			{
				log.md_FuncID = "Menu.CollectionToAgent";
				log.TITLE = "收發作業-收發代辦";
			}
			else if (PageFrom == "2")
			{
				log.md_FuncID = "Menu.CollectionToAgent";
				log.TITLE = "經辦作業-待辦理";
			}

			#region ID
			log.TXSNO = TXSNO;
			log.TXDateTime = TXDateTime;
			log.ColumnID = "CustId";
			log.ColumnName = "ID";
			log.ColumnValueBefore = "";
			log.ColumnValueAfter = formItem.CustId;
			log.TabID = "Tab2-1";
			log.TabName = "扣押設定";
			log.TableName = "CaseSeizure";
			log.DispSrNo = 1;
			log.TableDispActive = "1";
			log.CaseId = CaseId.ToString();
			log.LinkDataKey = GetMaxSeizureId(CaseId).ToString();
			InsertCaseDataLog(log);
			#endregion

			#region 姓名
			log.TXSNO = TXSNO;
			log.TXDateTime = TXDateTime;
			log.ColumnID = "CustName";
			log.ColumnName = "姓名";
			log.ColumnValueBefore = "";
			log.ColumnValueAfter = formItem.CustName;
			log.TabID = "Tab2-1";
			log.TabName = "扣押設定";
			log.TableName = "CaseSeizure";
			log.DispSrNo = 2;
			log.TableDispActive = "1";
			log.CaseId = CaseId.ToString();
			log.LinkDataKey = GetMaxSeizureId(CaseId).ToString();
			InsertCaseDataLog(log);
			#endregion

			#region 分行別
			log.TXSNO = TXSNO;
			log.TXDateTime = TXDateTime;
			log.ColumnID = "BranchNo";
			log.ColumnName = "分行別";
			log.ColumnValueBefore = "";
			log.ColumnValueAfter = formItem.BranchNo;
			log.TabID = "Tab2-1";
			log.TabName = "扣押設定";
			log.TableName = "CaseSeizure";
			log.DispSrNo = 3;
			log.TableDispActive = "1";
			log.CaseId = CaseId.ToString();
			log.LinkDataKey = GetMaxSeizureId(CaseId).ToString();
			InsertCaseDataLog(log);
			#endregion

			#region 分行名稱
			log.TXSNO = TXSNO;
			log.TXDateTime = TXDateTime;
			log.ColumnID = "BranchName";
			log.ColumnName = "分行名稱";
			log.ColumnValueBefore = "";
			log.ColumnValueAfter = formItem.BranchName;
			log.TabID = "Tab2-1";
			log.TabName = "扣押設定";
			log.TableName = "CaseSeizure";
			log.DispSrNo = 4;
			log.TableDispActive = "1";
			log.CaseId = CaseId.ToString();
			log.LinkDataKey = GetMaxSeizureId(CaseId).ToString();
			InsertCaseDataLog(log);
			#endregion

			#region 存款帳號
			log.TXSNO = TXSNO;
			log.TXDateTime = TXDateTime;
			log.ColumnID = "Account";
			log.ColumnName = "存款帳號 (12碼)";
			log.ColumnValueBefore = "";
			log.ColumnValueAfter = formItem.Account.ToString();
			log.TabID = "Tab2-1";
			log.TabName = "扣押設定";
			log.TableName = "CaseSeizure";
			log.DispSrNo = 5;
			log.TableDispActive = "1";
			log.CaseId = CaseId.ToString();
			log.LinkDataKey = GetMaxSeizureId(CaseId).ToString();
			InsertCaseDataLog(log);
			#endregion

			#region 狀態
			log.TXSNO = TXSNO;
			log.TXDateTime = TXDateTime;
			log.ColumnID = "AccountStatus";
			log.ColumnName = "狀態";
			log.ColumnValueBefore = "";
			log.ColumnValueAfter = formItem.AccountStatus.ToString();
			log.TabID = "Tab2-1";
			log.TabName = "扣押設定";
			log.TableName = "CaseSeizure";
			log.DispSrNo = 6;
			log.TableDispActive = "1";
			log.CaseId = CaseId.ToString();
			log.LinkDataKey = GetMaxSeizureId(CaseId).ToString();
			InsertCaseDataLog(log);
			#endregion

			#region 幣別
			log.TXSNO = TXSNO;
			log.TXDateTime = TXDateTime;
			log.ColumnID = "Currency";
			log.ColumnName = "幣別";
			log.ColumnValueBefore = "";
			log.ColumnValueAfter = formItem.Currency.ToString();
			log.TabID = "Tab2-1";
			log.TabName = "扣押設定";
			log.TableName = "CaseSeizure";
			log.DispSrNo = 10;
			log.TableDispActive = "1";
			log.CaseId = CaseId.ToString();
			log.LinkDataKey = GetMaxSeizureId(CaseId).ToString();
			InsertCaseDataLog(log);
			#endregion

			#region 可用餘額
			log.TXSNO = TXSNO;
			log.TXDateTime = TXDateTime;
			log.ColumnID = "Balance";
			log.ColumnName = "可用餘額";
			log.ColumnValueBefore = "";
			log.ColumnValueAfter = formItem.Balance.ToString();
			log.TabID = "Tab2-1";
			log.TabName = "扣押設定";
			log.TableName = "CaseSeizure";
			log.DispSrNo = 11;
			log.TableDispActive = "1";
			log.CaseId = CaseId.ToString();
			log.LinkDataKey = GetMaxSeizureId(CaseId).ToString();
			InsertCaseDataLog(log);
			#endregion

			#region 扣押金額
			log.TXSNO = TXSNO;
			log.TXDateTime = TXDateTime;
			log.ColumnID = "SeizureAmount";
            log.ColumnName = "ETABS扣押金額";// "扣押金額";
			log.ColumnValueBefore = "";
			log.ColumnValueAfter = formItem.SeizureAmount.ToString();
			log.TabID = "Tab2-1";
			log.TabName = "扣押設定";
			log.TableName = "CaseSeizure";
			log.DispSrNo = 12;
			log.TableDispActive = "1";
			log.CaseId = CaseId.ToString();
			log.LinkDataKey = GetMaxSeizureId(CaseId).ToString();
			InsertCaseDataLog(log);
			#endregion

			#region 匯率
			log.TXSNO = TXSNO;
			log.TXDateTime = TXDateTime;
			log.ColumnID = "ExchangeRate";
			log.ColumnName = "匯率";
			log.ColumnValueBefore = "";
			log.ColumnValueAfter = formItem.ExchangeRate.ToString();
			log.TabID = "Tab2-1";
			log.TabName = "扣押設定";
			log.TableName = "CaseSeizure";
			log.DispSrNo = 13;
			log.TableDispActive = "1";
			log.CaseId = CaseId.ToString();
			log.LinkDataKey = GetMaxSeizureId(CaseId).ToString();
			InsertCaseDataLog(log);
			#endregion

			#region 台幣金額
			log.TXSNO = TXSNO;
			log.TXDateTime = TXDateTime;
			log.ColumnID = "SeizureAmountNtd";
            log.ColumnName = "台幣";// "台幣金額";
			log.ColumnValueBefore = "";
			log.ColumnValueAfter = formItem.SeizureAmountNtd.ToString();
			log.TabID = "Tab2-1";
			log.TabName = "扣押設定";
			log.TableName = "CaseSeizure";
			log.DispSrNo = 14;
			log.TableDispActive = "1";
			log.CaseId = CaseId.ToString();
			log.LinkDataKey = GetMaxSeizureId(CaseId).ToString();
			InsertCaseDataLog(log);
			#endregion

			#region 產品型態
			log.TXSNO = TXSNO;
			log.TXDateTime = TXDateTime;
			log.ColumnID = "ProdCode";
			log.ColumnName = "產品型態";
			log.ColumnValueBefore = "";
			log.ColumnValueAfter = string.IsNullOrEmpty(formItem.ProdCode) ? "" : formItem.ProdCode.ToString();
			log.TabID = "Tab2-1";
			log.TabName = "扣押設定";
			log.TableName = "CaseSeizure";
			log.DispSrNo = 7;
			log.TableDispActive = "1";
			log.CaseId = CaseId.ToString();
			log.LinkDataKey = GetMaxSeizureId(CaseId).ToString();
			InsertCaseDataLog(log);
			#endregion

            #region 產品代碼
            log.TXSNO = TXSNO;
            log.TXDateTime = TXDateTime;
            log.ColumnID = "TxtProdCode";
            log.ColumnName = "產品代碼";
            log.ColumnValueBefore = "";
            log.ColumnValueAfter = string.IsNullOrEmpty(formItem.TxtProdCode) ? "" : formItem.TxtProdCode.ToString();
            log.TabID = "Tab2-1";
            log.TabName = "扣押設定";
            log.TableName = "CaseSeizure";
            log.DispSrNo = 7;
            log.TableDispActive = "1";
            log.CaseId = CaseId.ToString();
            log.LinkDataKey = GetMaxSeizureId(CaseId).ToString();
            InsertCaseDataLog(log);
            #endregion

			#region 關係
			log.TXSNO = TXSNO;
			log.TXDateTime = TXDateTime;
			log.ColumnID = "Link";
			log.ColumnName = "關係";
			log.ColumnValueBefore = "";
			log.ColumnValueAfter = string.IsNullOrEmpty(formItem.Link) ? "" : formItem.Link.ToString();
			log.TabID = "Tab2-1";
			log.TabName = "扣押設定";
			log.TableName = "CaseSeizure";
			log.DispSrNo = 8;
			log.TableDispActive = "1";
			log.CaseId = CaseId.ToString();
			log.LinkDataKey = GetMaxSeizureId(CaseId).ToString();
			InsertCaseDataLog(log);
			#endregion

			#region 管理
			log.TXSNO = TXSNO;
			log.TXDateTime = TXDateTime;
			log.ColumnID = "SegmentCode";
			log.ColumnName = "管理";
			log.ColumnValueBefore = "";
			log.ColumnValueAfter = string.IsNullOrEmpty(formItem.SegmentCode) ? "" : formItem.SegmentCode.ToString();
			log.TabID = "Tab2-1";
			log.TabName = "扣押設定";
			log.TableName = "CaseSeizure";
			log.DispSrNo = 9;
			log.TableDispActive = "1";
			log.CaseId = CaseId.ToString();
			log.LinkDataKey = GetMaxSeizureId(CaseId).ToString();
			InsertCaseDataLog(log);
			#endregion

            #region 他案扣押
            log.TXSNO = TXSNO;
            log.TXDateTime = TXDateTime;
            log.ColumnID = "OtherSeizure";
            log.ColumnName = "他案扣押";
            log.ColumnValueBefore = "";
            log.ColumnValueAfter = string.IsNullOrEmpty(formItem.OtherSeizure) ? "" : formItem.OtherSeizure.ToString();
            log.TabID = "Tab2-1";
            log.TabName = "扣押設定";
            log.TableName = "CaseSeizure";
            log.DispSrNo = 18;
            log.TableDispActive = "1";
            log.CaseId = CaseId.ToString();
            log.LinkDataKey = GetMaxSeizureId(CaseId).ToString();
            InsertCaseDataLog(log);
            #endregion

			#endregion
		}

		//刪除操作
		public void Delete(Guid CaseId, CaseSeizure formItem, Guid TXSNO, DateTime TXDateTime)
		{ 
			CaseDataLog log = new CaseDataLog();

			#region 刪除
			log.TXType = "刪除";
			LdapEmployeeBiz empBiz = new LdapEmployeeBiz();
			LDAPEmployee empNow = empBiz.GetLdapEmployeeByEmpId(UserId);
			log.TXUser = empNow != null && !string.IsNullOrEmpty(empNow.EmpId) ? empNow.EmpId : " ";
			log.TXUserName = empNow != null && !string.IsNullOrEmpty(empNow.EmpName) ? empNow.EmpName : " ";
			if (PageFrom == "1")
			{
				log.md_FuncID = "Menu.CollectionToAgent";
				log.TITLE = "收發作業-收發代辦";
			}
			else if (PageFrom == "2")
			{
				log.md_FuncID = "Menu.CollectionToAgent";
				log.TITLE = "經辦作業-待辦理";
			}

			#region ID
			log.TXSNO = TXSNO;
			log.TXDateTime = TXDateTime;
			log.ColumnID = "CustId";
			log.ColumnName = "ID";
			log.ColumnValueBefore = formItem.CustId.ToString();
			log.ColumnValueAfter = "";
			log.TabID = "Tab2-1";
			log.TabName = "扣押設定";
			log.TableName = "CaseSeizure";
			log.DispSrNo = 1;
			log.TableDispActive = "1";
			log.CaseId = CaseId.ToString();
			log.LinkDataKey = formItem.SeizureId.ToString();
			InsertCaseDataLog(log);
			#endregion

			#region 姓名
			log.TXSNO = TXSNO;
			log.TXDateTime = TXDateTime;
			log.ColumnID = "CustName";
			log.ColumnName = "姓名";
			log.ColumnValueBefore = formItem.CustName.ToString();
			log.ColumnValueAfter = "";
			log.TabID = "Tab2-1";
			log.TabName = "扣押設定";
			log.TableName = "CaseSeizure";
			log.DispSrNo = 2;
			log.TableDispActive = "1";
			log.CaseId = CaseId.ToString();
			log.LinkDataKey = formItem.SeizureId.ToString();
			InsertCaseDataLog(log);
			#endregion

			#region 分行別
			log.TXSNO = TXSNO;
			log.TXDateTime = TXDateTime;
			log.ColumnID = "BranchNo";
			log.ColumnName = "分行別";
			log.ColumnValueBefore = formItem.BranchNo.ToString();
			log.ColumnValueAfter = "";
			log.TabID = "Tab2-1";
			log.TabName = "扣押設定";
			log.TableName = "CaseSeizure";
			log.DispSrNo = 3;
			log.TableDispActive = "1";
			log.CaseId = CaseId.ToString();
			log.LinkDataKey = formItem.SeizureId.ToString();
			InsertCaseDataLog(log);
			#endregion

			#region 分行名稱
			log.TXSNO = TXSNO;
			log.TXDateTime = TXDateTime;
			log.ColumnID = "BranchName";
			log.ColumnName = "分行名稱";
			log.ColumnValueBefore = formItem.BranchName.ToString();
			log.ColumnValueAfter = "";
			log.TabID = "Tab2-1";
			log.TabName = "扣押設定";
			log.TableName = "CaseSeizure";
			log.DispSrNo = 4;
			log.TableDispActive = "1";
			log.CaseId = CaseId.ToString();
			log.LinkDataKey = formItem.SeizureId.ToString();
			InsertCaseDataLog(log);
			#endregion

			#region 存款帳號
			log.TXSNO = TXSNO;
			log.TXDateTime = TXDateTime;
			log.ColumnID = "Account";
			log.ColumnName = "存款帳號 (12碼)";
			log.ColumnValueBefore = formItem.Account.ToString();
			log.ColumnValueAfter = "";
			log.TabID = "Tab2-1";
			log.TabName = "扣押設定";
			log.TableName = "CaseSeizure";
			log.DispSrNo = 5;
			log.TableDispActive = "1";
			log.CaseId = CaseId.ToString();
			log.LinkDataKey = formItem.SeizureId.ToString();
			InsertCaseDataLog(log);
			#endregion

			#region 狀態
			log.TXSNO = TXSNO;
			log.TXDateTime = TXDateTime;
			log.ColumnID = "AccountStatus";
			log.ColumnName = "狀態";
			log.ColumnValueBefore = formItem.AccountStatus.ToString();
			log.ColumnValueAfter = "";
			log.TabID = "Tab2-1";
			log.TabName = "扣押設定";
			log.TableName = "CaseSeizure";
			log.DispSrNo = 6;
			log.TableDispActive = "1";
			log.CaseId = CaseId.ToString();
			log.LinkDataKey = formItem.SeizureId.ToString();
			InsertCaseDataLog(log);
			#endregion

			#region 幣別
			log.TXSNO = TXSNO;
			log.TXDateTime = TXDateTime;
			log.ColumnID = "Currency";
			log.ColumnName = "幣別";
			log.ColumnValueBefore = formItem.Currency.ToString();
			log.ColumnValueAfter = "";
			log.TabID = "Tab2-1";
			log.TabName = "扣押設定";
			log.TableName = "CaseSeizure";
			log.DispSrNo = 10;
			log.TableDispActive = "1";
			log.CaseId = CaseId.ToString();
			log.LinkDataKey = formItem.SeizureId.ToString();
			InsertCaseDataLog(log);
			#endregion

			#region 可用餘額
			log.TXSNO = TXSNO;
			log.TXDateTime = TXDateTime;
			log.ColumnID = "Balance";
			log.ColumnName = "可用餘額";
			log.ColumnValueBefore = formItem.Balance.ToString();
			log.ColumnValueAfter = "";
			log.TabID = "Tab2-1";
			log.TabName = "扣押設定";
			log.TableName = "CaseSeizure";
			log.DispSrNo = 11;
			log.TableDispActive = "1";
			log.CaseId = CaseId.ToString();
			log.LinkDataKey = formItem.SeizureId.ToString();
			InsertCaseDataLog(log);
			#endregion

			#region 扣押金額
			log.TXSNO = TXSNO;
			log.TXDateTime = TXDateTime;
			log.ColumnID = "SeizureAmount";
            log.ColumnName = "ETABS扣押金額";// "扣押金額";
			log.ColumnValueBefore = formItem.SeizureAmount.ToString();
			log.ColumnValueAfter = "";
			log.TabID = "Tab2-1";
			log.TabName = "扣押設定";
			log.TableName = "CaseSeizure";
			log.DispSrNo = 12;
			log.TableDispActive = "1";
			log.CaseId = CaseId.ToString();
			log.LinkDataKey = formItem.SeizureId.ToString();
			InsertCaseDataLog(log);
			#endregion

			#region 匯率
			log.TXSNO = TXSNO;
			log.TXDateTime = TXDateTime;
			log.ColumnID = "ExchangeRate";
			log.ColumnName = "匯率";
			log.ColumnValueBefore = formItem.ExchangeRate.ToString();
			log.ColumnValueAfter = "";
			log.TabID = "Tab2-1";
			log.TabName = "扣押設定";
			log.TableName = "CaseSeizure";
			log.DispSrNo = 13;
			log.TableDispActive = "1";
			log.CaseId = CaseId.ToString();
			log.LinkDataKey = formItem.SeizureId.ToString();
			InsertCaseDataLog(log);
			#endregion

			#region 台幣金額
			log.TXSNO = TXSNO;
			log.TXDateTime = TXDateTime;
			log.ColumnID = "SeizureAmountNtd";
            log.ColumnName = "台幣";// "台幣金額";
			log.ColumnValueBefore = formItem.SeizureAmountNtd.ToString();
			log.ColumnValueAfter = "";
			log.TabID = "Tab2-1";
			log.TabName = "扣押設定";
			log.TableName = "CaseSeizure";
			log.DispSrNo = 14;
			log.TableDispActive = "1";
			log.CaseId = CaseId.ToString();
			log.LinkDataKey = formItem.SeizureId.ToString();
			InsertCaseDataLog(log);
			#endregion

			#region 產品型態
			log.TXSNO = TXSNO;
			log.TXDateTime = TXDateTime;
			log.ColumnID = "ProdCode";
			log.ColumnName = "產品型態";
			log.ColumnValueBefore = formItem.ProdCode.ToString();
			log.ColumnValueAfter = "";
			log.TabID = "Tab2-1";
			log.TabName = "扣押設定";
			log.TableName = "CaseSeizure";
			log.DispSrNo = 7;
			log.TableDispActive = "1";
			log.CaseId = CaseId.ToString();
			log.LinkDataKey = formItem.SeizureId.ToString();
			InsertCaseDataLog(log);
			#endregion

            #region 產品代碼
            log.TXSNO = TXSNO;
            log.TXDateTime = TXDateTime;
            log.ColumnID = "TxtProdCode";
            log.ColumnName = "產品代碼";
            log.ColumnValueBefore = formItem.TxtProdCode.ToString();
            log.ColumnValueAfter = "";
            log.TabID = "Tab2-1";
            log.TabName = "扣押設定";
            log.TableName = "CaseSeizure";
            log.DispSrNo = 16;
            log.TableDispActive = "1";
            log.CaseId = CaseId.ToString();
            log.LinkDataKey = formItem.SeizureId.ToString();
            InsertCaseDataLog(log);
            #endregion

			#region 關係
			log.TXSNO = TXSNO;
			log.TXDateTime = TXDateTime;
			log.ColumnID = "Link";
			log.ColumnName = "關係";
			log.ColumnValueBefore = formItem.Link.ToString();
			log.ColumnValueAfter = "";
			log.TabID = "Tab2-1";
			log.TabName = "扣押設定";
			log.TableName = "CaseSeizure";
			log.DispSrNo = 8;
			log.TableDispActive = "1";
			log.CaseId = CaseId.ToString();
			log.LinkDataKey = formItem.SeizureId.ToString();
			InsertCaseDataLog(log);
			#endregion

			#region 管理
			log.TXSNO = TXSNO;
			log.TXDateTime = TXDateTime;
			log.ColumnID = "SegmentCode";
			log.ColumnName = "管理";
			log.ColumnValueBefore = formItem.SegmentCode.ToString();
			log.ColumnValueAfter = "";
			log.TabID = "Tab2-1";
			log.TabName = "扣押設定";
			log.TableName = "CaseSeizure";
			log.DispSrNo = 9;
			log.TableDispActive = "1";
			log.CaseId = CaseId.ToString();
			log.LinkDataKey = formItem.SeizureId.ToString();
			InsertCaseDataLog(log);
			#endregion

            #region 他案扣押
            log.TXSNO = TXSNO;
            log.TXDateTime = TXDateTime;
            log.ColumnID = "OtherSeizure";
            log.ColumnName = "他案扣押";
            log.ColumnValueBefore = formItem.OtherSeizure.ToString();
            log.ColumnValueAfter = "";
            log.TabID = "Tab2-1";
            log.TabName = "扣押設定";
            log.TableName = "CaseSeizure";
            log.DispSrNo = 18;
            log.TableDispActive = "1";
            log.CaseId = CaseId.ToString();
            log.LinkDataKey = formItem.SeizureId.ToString();
            InsertCaseDataLog(log);
            #endregion

			#endregion
		}

		public IList<CaseSeizure> GetCaseSeizureFromTx(Guid caseId)
		{


            string strSql = @";with  T1 as(SELECT @CaseId AS [CaseId],M.[CustomerId] AS [CustId],M.[CustomerName] AS [CustName],
							D.[Branch] AS [BranchNo],P.[CodeDesc] AS [BranchName],D.[Account] AS [Account],D.[StsDesc] AS [AccountStatus],
							D.[Ccy] AS [Currency],D.[Bal] AS [Balance],D.[ProdDesc] as [ProdCode],D.[ProdCode] as [TxtProdCode],D.[Link],D.[System],
							CASE d.[SegmentCode] WHEN '1' THEN '個金' WHEN '2' THEN '法金' WHEN '3' THEN 'SBG' ELSE '' END AS [SegmentCode],D.[SNO],D.[FKSNO],
							SeizureId,SeizureAmount,ExchangeRate,SeizureAmountNtd
							FROM 
							(
							SELECT 
							ROW_NUMBER() OVER (PARTITION BY [CustomerId] ORDER BY [SNO] DESC) AS RowID, [SNO],[CustomerId],[CustomerName]
							FROM [TX_60491_Grp] 
							WHERE TX_60491_Grp.CaseID = @CaseId 
                            --and [CustomerId] IN (SELECT [ObligorNo] FROM [CaseObligor] WHERE [CaseId] = @CaseId)
							) AS M
							LEFT OUTER JOIN [TX_60491_Detl] AS D ON M.[SNO] = D.[FKSNO]
							LEFT OUTER JOIN [PARMCode] AS P ON P.[CodeType]= 'RCAF_BRANCH' AND P.[CodeNo] = D.[Branch]
							LEFT OUTER JOIN CaseSeizure as S on D.CaseId=S.CaseId
							--inner join CaseObligor g on g.CaseId =@CaseId and g.ObligorNo = M.CustomerId
							WHERE M.RowID = 1) ,
							T2 as (select distinct ColumnValueBefore,ColumnValueAfter,ColumnID,LinkDataKey from CaseDataLog c1,CaseSeizure
							where c1.CaseId=@CaseId and TabName='扣押設定' and TableName='CaseSeizure' and LinkDataKey=cast(SeizureId as nvarchar(100) )
                            and TXDateTime=(select max(TXDateTime) from CaseDataLog c2 where c1.CaseId=c2.CaseId and c1.TabName=c2.TabName and c1.TableName=c2.TableName and c1.ColumnID=c2.ColumnID and c1.LinkDataKey=c2.LinkDataKey)),
							T3 as (select T1.*,
							(select case  when ColumnValueBefore<>CustId then 'true' else 'false' end from T2 where ColumnID='CustId' and LinkDataKey=cast(SeizureId as nvarchar(100) )) as CustIdflag,
							(select case  when ColumnValueBefore<>CustName then 'true' else 'false' end from T2 where ColumnID='CustName' and LinkDataKey=cast(SeizureId as nvarchar(100)) ) as CustNameflag,
							(select case  when ColumnValueBefore<>BranchNo then 'true' else 'false' end from T2 where ColumnID='BranchNo' and LinkDataKey=cast(SeizureId as nvarchar(100))  ) as BranchNoflag,
							(select case  when ColumnValueBefore<>BranchName then 'true' else 'false' end from T2 where ColumnID='BranchName' and LinkDataKey=cast(SeizureId as nvarchar(100)) ) as BranchNameflag,
							(select case  when ColumnValueBefore<>Account then 'true' else 'false' end from T2 where ColumnID='Account' and LinkDataKey=cast(SeizureId as nvarchar(100)) ) as Accountflag,
							(select case  when ColumnValueBefore<>AccountStatus then 'true' else 'false' end from T2 where ColumnID='AccountStatus' and LinkDataKey=cast(SeizureId as nvarchar(100))  ) as AccountStatusflag,
							(select case  when ColumnValueBefore<>Currency then 'true' else 'false' end from T2 where ColumnID='Currency' and LinkDataKey=cast(SeizureId as nvarchar(100)) ) as Currencyflag,
							(select case  when ColumnValueBefore<>Balance then 'true' else 'false' end from T2 where ColumnID='Balance' and LinkDataKey=cast(SeizureId as nvarchar(100)) ) as Balanceflag,
							(select case  when ColumnValueBefore<>cast(SeizureAmount as nvarchar(100)) then 'true' else 'false' end from T2 where ColumnID='SeizureAmount' and LinkDataKey=cast(SeizureId as nvarchar(100))  ) as SeizureAmountflag,
							(select case  when ColumnValueBefore<>cast(ExchangeRate as nvarchar(100)) then 'true' else 'false' end from T2 where ColumnID='ExchangeRate' and LinkDataKey=cast(SeizureId as nvarchar(100)) ) as ExchangeRateflag,
							(select case  when ColumnValueBefore<>cast(SeizureAmountNtd as nvarchar(100)) then 'true' else 'false' end from T2 where ColumnID='SeizureAmountNtd' and LinkDataKey=cast(SeizureId as nvarchar(100))  ) as SeizureAmountNtdflag,
							(select case  when ColumnValueBefore<>ProdCode then 'true' else 'false' end from T2 where ColumnID='ProdCode' and LinkDataKey=cast(SeizureId as nvarchar(100)) ) as ProdCodeflag,
                            (select case  when ColumnValueBefore<>TxtProdCode then 'true' else 'false' end from T2 where ColumnID='TxtProdCode' and LinkDataKey=cast(SeizureId as nvarchar(100)) ) as TxtProdCodeflag,
							(select case  when ColumnValueBefore<>Link then 'true' else 'false' end from T2 where ColumnID='Link' and LinkDataKey=cast(SeizureId as nvarchar(100)) ) as Linkflag,
							(select case  when ColumnValueBefore<>SegmentCode then 'true' else 'false' end from T2 where ColumnID='SegmentCode' and LinkDataKey=cast(SeizureId as nvarchar(100))  ) as SegmentCodeflag
							from T1)
							select *,1 as IsFromZJ from T3";
                            //CASE D.[Ccy] WHEN 'TWD' THEN '0' WHEN 'USD' THEN '1' ELSE '2' END ASC;";
			Parameter.Clear();
			Parameter.Add(new CommandParameter("@CaseId", caseId));
			IList<CaseSeizure> list = base.SearchList<CaseSeizure>(strSql);


			CaseMaster master = new CaseMasterBIZ().MasterModel(caseId);
			CustomerInfoBIZ custBiz = new CustomerInfoBIZ();
            PARMCodeBIZ pbiz = new PARMCodeBIZ();
            IList<PARMCode> CurrencyList = pbiz.GetCodeData("CURRENCY");//抓取所有外幣匯率
            string foreignCcy = pbiz.GetParmCodeByCodeType("SeizureSeqence").Where(m => m.CodeDesc == "外幣活存").FirstOrDefault().CodeMemo;//外幣活存的產品代碼
			if (list != null && list.Any())
			{
				//* 至少主機下來有查到資料
				foreach (CaseSeizure seizure in list)
				{
					seizure.CaseId = master.CaseId;
					seizure.CaseNo = master.CaseNo;
					seizure.Balance = "0";
					if (seizure.System.Trim().ToUpper() != "B" || seizure.System.Trim().ToUpper() != "T")
					{
                        CTBC.CSFS.Models.TX_33401 tx33401 = custBiz.GetLatestTx33401(seizure.Account, caseId, seizure.Currency);//adam 20160427
						if (tx33401 != null)
						{
							seizure.InvestCode = !string.IsNullOrEmpty(tx33401.InvestCode) ? "*" : "";
							seizure.Balance = tx33401.TrueAmt;
						}
					}
                    //seizure.ExchangeRate = seizure.Currency.ToUpper() == "TWD" ? 1 : 0;
                    string currencyRate = "";//外幣匯率
                    if (seizure.Currency.ToUpper() == "TWD")
                    {
                        seizure.ExchangeRate = 1;
                    }
                    else
                    {
                        IList<PARMCode> currencySelect = CurrencyList.Where(cc => cc.CodeNo == seizure.Currency.ToUpper()).ToList();
                        if (currencySelect != null && currencySelect.Any())
                        {
                            currencyRate = currencySelect.FirstOrDefault().CodeMemo;
                        }
                        else
                        {
                            currencyRate = "0";
                        }
                        seizure.ExchangeRate = Convert.ToDecimal(currencyRate) * Convert.ToDecimal(0.95);//幣別為外幣，匯率以中信現鈔買匯*0.95(匯率需參數設定) IR-0047
                    }
                    if (!string.IsNullOrEmpty(seizure.TxtProdCode.Trim()) && seizure.TxtProdCode.Length > 2)//TxtProdCode排空才能Substring
                    {
                        if (seizure.Currency.ToUpper() != "TWD" && seizure.Account.Length >= 15 && foreignCcy.Contains(seizure.TxtProdCode.Substring(2)))//外幣活存才砍掉右邊3碼  IR-1019
                        {
                            seizure.Account = seizure.Account.Substring(0, seizure.Account.Length - 3);
                        }
                    }
				}
			}

			return list;

		}

        public List<CaseMeetMaster> GetBranchVipFromTx(string obligorNo)
        {
            string strSql = @" SELECT BranchViptext
                                             FROM 
                                             (
                                                SELECT  
                                                ROW_NUMBER() OVER (PARTITION BY [CustomerId] ORDER BY [SNO] DESC) AS RowID, 
                                                VipCdI AS BranchViptext FROM TX_60491_Grp
                                                 WHERE
                                                 CustomerId IN (" + obligorNo+@") 
                                             ) AS M 
                                            WHERE M.RowID=1 ";

            Parameter.Clear();
            List<CaseMeetMaster> list = base.SearchList<CaseMeetMaster>(strSql).ToList();
            return list;
        }

		public int InsertCaseDataLog(CaseDataLog log, IDbTransaction trans = null)
		{
			CaseObligorBIZ caseobligor = new CaseObligorBIZ();
			return caseobligor.InsertCaseDataLog(log);
		}

		public int GetMaxSeizureId(Guid caseId)
		{
			string sql = "select max(SeizureId) from CaseSeizure where CaseId=@CaseId ";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("@CaseId", caseId));
			int result = Convert.ToInt32(base.ExecuteScalar(sql));
			return result;
		}

        public bool SaveSeizureSetting(List<CaseSeizure> listForm, string memo, Guid CaseId)
        {
            string str = base.Account;
            string str2 = base.AccountName;
            IDbConnection dbConnection = OpenConnection();
            IDbTransaction dbTrans = null;
            bool bFlag = true;
            try
            {
                using (dbConnection)
                {
                    dbTrans = dbConnection.BeginTransaction();
                    IList<CaseSeizure> listOld = GetCaseSeizure(CaseId, null);
					Guid TXSNO = Guid.NewGuid();
					DateTime TXDateTime = DateTime.Parse(System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
					LdapEmployeeBiz empBiz = new LdapEmployeeBiz();
					LDAPEmployee empNow = empBiz.GetLdapEmployeeByEmpId(UserId);
					List<int> list = new List<int>();
					if (listForm != null)
					{
						foreach (CaseSeizure ss in listForm)
						{
							list.Add(ss.SeizureId);
							if (ss.SeizureId == 0)
							{
								ss.IsAdd = true;
							}
						}
					}
					else
					{
						listForm = new List<CaseSeizure>();
					}
					if (listForm.Count == 0 && listOld.Count > 0)
					{
						for (int i = 0; i < listOld.Count; i++)
						{
							listOld[i].IsDelete = true;
							listForm.Add(listOld[i]);
						}
					}
					else if (listForm.Count == 0 && listOld.Count == 0)
					{
						for (int i = 0; i < listOld.Count; i++)
						{
							listForm.Add(listOld[i]);
						}
					}
					else
					{
						for (int i = 0; i < listOld.Count; i++)
						{
							for (int j = 0; j < listForm.Count; j++)
							{
								if (listOld[i].SeizureId != listForm[j].SeizureId)
								{
									if (!list.Contains(listOld[i].SeizureId))
									{
										listOld[i].IsDelete = true;
										list.Add(listOld[i].SeizureId);
										listForm.Add(listOld[i]);
									}
								}
							}
						}
                    }
                    #region 產品代碼
                    PARMCodeBIZ pbiz = new PARMCodeBIZ();
                    var seizureSequenceAll = pbiz.GetParmCodeByCodeType("SeizureSeqence");
                    var seizureSequence = seizureSequenceAll.ToList();
                    List<string> ProdCodeList = new List<string>();
                    string ProdCodeAll = "";//所有的產品代碼
                    string XXProdCode = "";//包含XX的產品代碼
                    string NoXXProdCode = "";//不包含XX的產品代碼
                    foreach (var s in seizureSequence)
                    {
                        if (!string.IsNullOrEmpty(s.CodeMemo))
                        {
                            ProdCodeAll = ProdCodeAll + s.CodeMemo + ",";
                        }
                    }
                    if(!string.IsNullOrEmpty(ProdCodeAll))
                    {
                        ProdCodeList = ProdCodeAll.TrimEnd(',').Split(',').ToList();
                    }
                    foreach(string p in ProdCodeList)
                    {
                        if(p.StartsWith("XX"))
                        {
                            XXProdCode = XXProdCode + p + ",";
                        }
                        else
                        {
                            NoXXProdCode = NoXXProdCode + p + ",";
                        }
                    }
                    #endregion
                    foreach (CaseSeizure formItem in listForm)
					{
                        //產品代碼 如果在 參數設定中沒有被找到 則直接將產品代碼改為ZZZZ儲存
                        if (string.IsNullOrEmpty(formItem.TxtProdCode) || (!NoXXProdCode.Contains(formItem.TxtProdCode) && !XXProdCode.Contains(formItem.TxtProdCode.Substring(2))))
                        {
                            formItem.TxtProdCode = "ZZZZ";
                        }

						if (formItem.SeizureId > 0 && formItem.IsDelete == false)
						{
							CaseSeizure cs = GetCaseSeizureInfo(formItem.CaseId, formItem.SeizureId);
							bFlag = bFlag && UpdateSeizureSetting(formItem, null);

							if (formItem.IsFromZJ == 0)
							{
								CaseDataLog log = new CaseDataLog();

								#region 修改
								log.TXType = "修改";
								log.TXUser = empNow != null && !string.IsNullOrEmpty(empNow.EmpId) ? empNow.EmpId : " ";
								log.TXUserName = empNow != null && !string.IsNullOrEmpty(empNow.EmpName) ? empNow.EmpName : " ";
								if (PageFrom == "1")
								{
									log.md_FuncID = "Menu.CollectionToAgent";
									log.TITLE = "收發作業-收發代辦";
								}
								else if (PageFrom == "2")
								{
									log.md_FuncID = "Menu.CollectionToAgent";
									log.TITLE = "經辦作業-待辦理";
								}

								if (listOld.Count > 0)
								{
									#region 扣押金額
									if (cs.SeizureAmount != formItem.SeizureAmount)
									{
										log.TXSNO = TXSNO;
										log.TXDateTime = TXDateTime;
										log.ColumnID = "SeizureAmount";
                                        log.ColumnName = "ETABS扣押金額";// "扣押金額";
										log.ColumnValueBefore = cs.SeizureAmount.ToString();
										log.ColumnValueAfter = formItem.SeizureAmount.ToString();
										log.TabID = "Tab2-1";
										log.TabName = "扣押設定";
										log.TableName = "CaseSeizure";
										log.DispSrNo = 1;
										log.TableDispActive = "1";
										log.CaseId = CaseId.ToString();
										log.LinkDataKey = formItem.SeizureId.ToString();
										InsertCaseDataLog(log);
									}
									#endregion

									#region 匯率
									if (cs.ExchangeRate != formItem.ExchangeRate)
									{
										log.TXSNO = TXSNO;
										log.TXDateTime = TXDateTime;
										log.ColumnID = "ExchangeRate";
										log.ColumnName = "匯率";
										log.ColumnValueBefore = cs.ExchangeRate.ToString();
										log.ColumnValueAfter = formItem.ExchangeRate.ToString();
										log.TabID = "Tab2-1";
										log.TabName = "扣押設定";
										log.TableName = "CaseSeizure";
										log.DispSrNo = 2;
										log.TableDispActive = "1";
										log.CaseId = CaseId.ToString();
										log.LinkDataKey = formItem.SeizureId.ToString();
										InsertCaseDataLog(log);
									}
									#endregion

									#region 台幣金額
									if (cs.SeizureAmountNtd != formItem.SeizureAmountNtd)
									{
										log.TXSNO = TXSNO;
										log.TXDateTime = TXDateTime;
										log.ColumnID = "SeizureAmountNtd";
                                        log.ColumnName = "台幣";// "台幣金額";
										log.ColumnValueBefore = cs.SeizureAmountNtd.ToString();
										log.ColumnValueAfter = formItem.SeizureAmountNtd.ToString();
										log.TabID = "Tab2-1";
										log.TabName = "扣押設定";
										log.TableName = "CaseSeizure";
										log.DispSrNo = 3;
										log.TableDispActive = "1";
										log.CaseId = CaseId.ToString();
										log.LinkDataKey = formItem.SeizureId.ToString();
										InsertCaseDataLog(log);
									}
									#endregion
								}
								#endregion
							}
						}
						//新增
						if (formItem.SeizureId == 0)
						{
							bFlag = bFlag && InsertSeizureSetting(formItem, null);

							if (formItem.IsFromZJ == 0)
							{
								Add(CaseId, formItem, TXSNO, TXDateTime);
							}
						}
						//刪除
						if (formItem.SeizureId > 0 && formItem.IsDelete == true)
						{
							DeleteSeizureSetting(CaseId, formItem.SeizureId, null);

							if (formItem.IsFromZJ == 0)
							{
								Delete(CaseId, formItem, TXSNO, TXDateTime);
							}
						}
					}

                    var memoObj = new CaseMemo { CaseId = CaseId, MemoType = CaseMemoType.CaseSeizureMemo, Memo = memo };
					CaseMemo oldMemoObj = new CaseMemoBiz().Memo(CaseId, CaseMemoType.CaseSeizureMemo);
					new CaseMemoBiz().Delete(memoObj, null);
					
                    bFlag = bFlag && new CaseMemoBiz().SaveMemo(memoObj, null);

					CaseDataLog logMemo = new CaseDataLog();

					#region 修改
					logMemo.TXType = "修改";
					logMemo.TXUser = empNow != null && !string.IsNullOrEmpty(empNow.EmpId) ? empNow.EmpId : " ";
					logMemo.TXUserName = empNow != null && !string.IsNullOrEmpty(empNow.EmpName) ? empNow.EmpName : " ";
					if (PageFrom == "1")
					{
						logMemo.md_FuncID = "Menu.CollectionToAgent";
						logMemo.TITLE = "收發作業-收發代辦";
					}
					else if (PageFrom == "2")
					{
						logMemo.md_FuncID = "Menu.CollectionToAgent";
						logMemo.TITLE = "經辦作業-待辦理";
					}

					CaseMasterBIZ masterBiz = new CaseMasterBIZ();
					CaseMaster master = masterBiz.MasterModelNew(CaseId);
					string oldMemo = oldMemoObj == null ? "" : oldMemoObj.Memo;
					if (oldMemo != memoObj.Memo)
					{
						#region Memo
						logMemo.TXSNO = TXSNO;
						logMemo.TXDateTime = TXDateTime;
						logMemo.ColumnID = "Memo";
						logMemo.ColumnName = "備註內容";
						logMemo.ColumnValueBefore = oldMemo;
						logMemo.ColumnValueAfter = memoObj.Memo;
						logMemo.TabID = "Tab2-1";
						logMemo.TabName = "扣押設定";
						logMemo.TableName = "CaseMemo";
						logMemo.DispSrNo = 1;
						logMemo.TableDispActive = "0";
						logMemo.CaseId = CaseId.ToString();
						logMemo.CaseNo = master.CaseNo.ToString();
						logMemo.LinkDataKey = CaseId.ToString();
						InsertCaseDataLog(logMemo);
						#endregion

						#region MemoDate
						logMemo.TXSNO = TXSNO;
						logMemo.TXDateTime = TXDateTime;
						logMemo.ColumnID = "MemoDate";
						logMemo.ColumnName = "備註時間";
						logMemo.ColumnValueBefore = oldMemoObj.MemoDate;
						logMemo.ColumnValueAfter = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
						logMemo.TabID = "Tab2-1";
						logMemo.TabName = "扣押設定";
						logMemo.TableName = "CaseMemo";
						logMemo.DispSrNo = 1;
						logMemo.TableDispActive = "0";
						logMemo.CaseId = CaseId.ToString();
						logMemo.CaseNo = master.CaseNo.ToString();
						logMemo.LinkDataKey = CaseId.ToString();
						InsertCaseDataLog(logMemo);
						#endregion

						#region MemoUser
						logMemo.TXSNO = TXSNO;
						logMemo.TXDateTime = TXDateTime;
						logMemo.ColumnID = "MemoUser";
						logMemo.ColumnName = "備註人";
						logMemo.ColumnValueBefore = oldMemoObj.MemoUser;
						logMemo.ColumnValueAfter = logMemo.TXUserName;
						logMemo.TabID = "Tab2-1";
						logMemo.TabName = "扣押設定";
						logMemo.TableName = "CaseMemo";
						logMemo.DispSrNo = 1;
						logMemo.TableDispActive = "0";
						logMemo.CaseId = CaseId.ToString();
						logMemo.CaseNo = master.CaseNo.ToString();
						logMemo.LinkDataKey = CaseId.ToString();
						InsertCaseDataLog(logMemo);
						#endregion
					}
					#endregion 

					if (bFlag)
					{
						dbTrans.Commit();
					}
					else
					{
						dbTrans.Rollback();
						
					}
                    return bFlag;
                }
            }
            catch (Exception ex)
            {
                try
                {
                    if (dbTrans != null)
                        dbTrans.Rollback();
                }
                catch (Exception ex2)
                {

                }
                return false;
            }
        }

        public int ResetStatus(Guid caseId)
        {
            CaseMasterBIZ masterBiz = new CaseMasterBIZ();
            CaseMaster master = masterBiz.MasterModel(caseId);
            if (master == null)
                return 0;

            CaseObligorBIZ obligorBiz = new CaseObligorBIZ();
            List<CaseObligor> list = obligorBiz.ObligorModel(caseId);
            return obligorBiz.CreateLog(list, master);
        }

        public CaseEdocFile OpenExcel(Guid caseId)
        {
            //caseId = new Guid("635FEDA7-59DA-478F-99DE-77F28FFF1F63");
            string strSql = @"select * from CaseEdocFile where CaseId=@CaseId and FileType='xlsx' and Type='歷史'";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("@CaseId", caseId));
            List<CaseEdocFile> list = base.SearchList<CaseEdocFile>(strSql).ToList();
            return list.FirstOrDefault();
        }
        public CaseEdocFile OpenDeadExcel3(Guid caseId)
        {
            //caseId = new Guid("635FEDA7-59DA-478F-99DE-77F28FFF1F63");
            string strSql = @"select * from CaseEdocFile where CaseId=@CaseId and FileType='xlsx3' and Type='死亡'";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("@CaseId", caseId));
            List<CaseEdocFile> list = base.SearchList<CaseEdocFile>(strSql).ToList();
            return list.FirstOrDefault();
        }
        public CaseEdocFile OpenPdf(Guid caseId)
        {
            //caseId = new Guid("635FEDA7-59DA-478F-99DE-77F28FFF1F63");
            string strSql = @"select * from CaseEdocFile where CaseId=@CaseId and FileType='pdf' and Type='歷史'";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("@CaseId", caseId));
            List<CaseEdocFile> list = base.SearchList<CaseEdocFile>(strSql).ToList();
            return list.FirstOrDefault();
        }
        public CaseEdocFile OpenDeadXlsx3(Guid caseId)
        {
            //caseId = new Guid("635FEDA7-59DA-478F-99DE-77F28FFF1F63");
            string strSql = @"select * from CaseEdocFile where CaseId=@CaseId and FileType='xlsx3' and Type='死亡'";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("@CaseId", caseId));
            List<CaseEdocFile> list = base.SearchList<CaseEdocFile>(strSql).ToList();
            return list.FirstOrDefault();
        }
        public CaseEdocFile OpenDeadPdf(Guid caseId)
        {
            //caseId = new Guid("635FEDA7-59DA-478F-99DE-77F28FFF1F63");
            string strSql = @"select * from CaseEdocFile where CaseId=@CaseId and FileType='pdf' and Type='死亡'";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("@CaseId", caseId));
            List<CaseEdocFile> list = base.SearchList<CaseEdocFile>(strSql).ToList();
            return list.FirstOrDefault();
        }
        public CaseEdocFile OpenTxtDoc(Guid caseId)
        {
            //caseId = new Guid("635FEDA7-59DA-478F-99DE-77F28FFF1F63");
            string strSql = @"select * from CaseEdocFile where CaseId=@CaseId and FileType='txt' and Type='收文'";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("@CaseId", caseId));
            List<CaseEdocFile> list = base.SearchList<CaseEdocFile>(strSql).ToList();
            return list.FirstOrDefault();
        }
        public CaseEdocFile OpenTxtDoc(string caseId,string fName)
        {
            string strSql = @"select * from CaseEdocFile where CaseId=@CaseId  and fileName=@filename";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("@CaseId", caseId));
            Parameter.Add(new CommandParameter("@filename", fName));
            List<CaseEdocFile> list = base.SearchList<CaseEdocFile>(strSql).ToList();
            return list.FirstOrDefault();
        }

        public CaseEdocFile OpenHisDoc(string caseId, string fName,int rownumber)
        {
            string strSql = @"WITH downfile AS  
(  
select CaseId, Type, FileType, FileName, FileObject, SendNo,ROW_NUMBER() OVER (ORDER BY CaseId) AS RowNumber from CaseEdocFile where CaseId=@CaseId   
)
select CaseId, Type, FileType, FileName, FileObject, SendNo from downfile   where CaseId=@CaseId  and fileName=@filename and RowNumber=@RowNumber ";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("@CaseId", caseId));
            Parameter.Add(new CommandParameter("@filename", fName));
            Parameter.Add(new CommandParameter("@RowNumber", rownumber));
            List<CaseEdocFile> list = base.SearchList<CaseEdocFile>(strSql).ToList();
            return list.FirstOrDefault();
        }



        public CaseEdocFile OpenPdfDoc(string caseId)
		{
			//caseId = "08F4A94D-5AA7-4010-9304-6AB97D011413";
			string strSql = @"select * from CaseEdocFile where CaseId=@CaseId and FileType='pdf' and Type='收文'";
			Parameter.Clear();
			Parameter.Add(new CommandParameter("@CaseId", caseId));
			List<CaseEdocFile> list = base.SearchList<CaseEdocFile>(strSql).ToList();
			return list.FirstOrDefault();
		}
        public CaseEdocFile OpenPayPdfDoc1(string caseId)
        {
            //caseId = "08F4A94D-5AA7-4010-9304-6AB97D011413";
            string strSql = @"select * from CaseEdocFile where CaseId=@CaseId and FileType='pdf' and Type='收文' and UPPER(filename) not like '%ATTCH%' ";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("@CaseId", caseId));
            List<CaseEdocFile> list = base.SearchList<CaseEdocFile>(strSql).ToList();
            return list.FirstOrDefault();
        }
        public CaseEdocFile OpenPayPdfDoc2(string caseId)
        {
            //caseId = "08F4A94D-5AA7-4010-9304-6AB97D011413";
            string strSql = @"select * from CaseEdocFile where CaseId=@CaseId and FileType='pdf' and Type='收文' and UPPER(filename) like '%ATTCH1%'";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("@CaseId", caseId));
            List<CaseEdocFile> list = base.SearchList<CaseEdocFile>(strSql).ToList();
            return list.FirstOrDefault();
        }

        public List<BatchQueue> StatusList(Guid CaseId)
        {
            //string strSql = @" select Status from BatchQueue where CaseId=@CaseId";
            string strSql = @" select * from BatchQueue where CaseId=@CaseId 
                                  union  
                                select * from autolog where CaseId=@CaseId ";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("@CaseId", CaseId));
            List<BatchQueue> list = base.SearchList<BatchQueue>(strSql).ToList();
            if (list != null && list.Count > 0) return list;
            else return new List<BatchQueue>();

            //string strSql = @" select Status from BatchQueue where CaseId=@CaseId";
            //Parameter.Clear();
            //Parameter.Add(new CommandParameter("@CaseId", CaseId.ToString()));
            //DataTable list = base.Search(strSql);
            //if (list != null && list.Rows.Count > 0) return list;
            //else return new DataTable();
        }

        public decimal ResetList(Guid CaseId)
        {
            //string strSql = @" select Status from BatchQueue where CaseId=@CaseId";
            string strsql = @" SELECT  sum(SeizureAmountNtd)   FROM  CaseSeizure  where CaseId=@CaseId ";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("@CaseId", CaseId));

            string str = base.ExecuteScalar(strsql).ToString();
            if (!string.IsNullOrEmpty(str))
                return Convert.ToDecimal(base.ExecuteScalar(strsql));
            else
                return 0;
        }

        //*新增案件及新增扣押维护
        public JsonReturn SaveSeizureSetting(List<CaseSeizure> listForm)
        {
            Guid strCaseId = new Guid();
            CaseNoTableBIZ noBiz = new CaseNoTableBIZ();
            CaseMasterBIZ master = new CaseMasterBIZ();
            IDbConnection dbConnection = OpenConnection();
            IDbTransaction dbTrans = null;
            bool bFlag = true;
            try
            {
                using (dbConnection)
                {
                    dbTrans = dbConnection.BeginTransaction();

                    #region 判断案件编号是否重复
                    if (listForm != null && listForm.Any())
                    {
                        foreach (CaseSeizure formItem in listForm)
                        {
                            if (master.IsCaseNoExist(formItem.CaseNo, dbTrans) == "1")
                            {
                                dbTrans.Rollback();
                                return new JsonReturn() { ReturnCode = "2" };
                            }
                        }
                    }
                    #endregion

                    #region 新增扣押案件及案件
                    string strCaseNo = string.Empty;
                    if (listForm != null && listForm.Any())
                    {
                        CaseMaster model = new CaseMaster();
                        model.GovKind = "法院";
                        model.GovUnit = "扣押補建";
                        model.ReceiverNo = "扣押補建";
                        model.GovNo = "扣押補建";
                        model.GovDate = "2001/01/01";
                        model.Speed = "最速件";
                        model.ReceiveKind = "紙本";
                        model.Status = CaseStatus.DirectorApprove;   //* 结案
                        model.LimitDate = "2001/01/01";
                        model.CaseKind = CaseKind.CASE_SEIZURE;
                        model.CaseKind2 = "扣押";
                        model.ReceiveDate = "2001/01/01";
                        model.Unit = "扣押補建";
                        model.Person = listForm[0].CreatedUser;         //* 建檔人
                        model.CreatedUser = listForm[0].CreatedUser;
                        model.CreatedDate = "2001/01/01";

                        //*新增扣押案件
                        foreach (CaseSeizure formItem in listForm)
                        {
                            if (strCaseNo != formItem.CaseNo)
                            {
                                Guid caseId = Guid.NewGuid();
                                strCaseId = caseId;
                                model.CaseId = caseId;
                                model.CaseNo = formItem.CaseNo;
                                model.DocNo = noBiz.GetDocNoMaintain(dbTrans);
                                bFlag = bFlag && master.Create(model, dbTrans) > 0;
                                strCaseNo = formItem.CaseNo;
                            }

                            formItem.CaseId = strCaseId;
                            //adam 直接commit
                            bFlag = bFlag && InsertSeizureSetting(formItem);
                            //bFlag = bFlag && InsertSeizureSetting(formItem, dbTrans);
                            //*新增案件

                        }
                    }
                    #endregion

                    if (bFlag)
                    {
                        dbTrans.Commit();
                        return new JsonReturn() { ReturnCode = "1" };
                    }
                    dbTrans.Rollback();
                    return new JsonReturn() { ReturnCode = "0" };
                }
            }
            
            
            catch (Exception ex)
            {
                dbTrans.Rollback();
                return new JsonReturn() { ReturnCode = "0" };
            }
        }

        public List<CaseSeizure> QuerySeizureMaintainList(Guid caseId)
        {
            string strSql = @" select s.SeizureId,s.CaseId,s.CaseNo,s.Currency,s.CustId,s.CustName,s.BranchNo,s.BranchName,s.Account,s.SeizureAmount,
                                        s.ExchangeRate,s.SeizureAmountNtd from  CaseSeizure s
                                        left join CaseMaster  m on m.CaseId=s.CaseId
                                        where LEN(m.CaseNo)<11 and s.SeizureStatus=0
                                        and s.CustId in(select ObligorNo from CaseObligor where CaseId=@CaseId)";

            Parameter.Clear();
            Parameter.Add(new CommandParameter("CaseId", caseId));
            return base.SearchList<CaseSeizure>(strSql).ToList();
        }

        public CaseSeizure GetCaseSeizure(string SeizureId, IDbTransaction dbtrans = null)
        {
            string strSql = @" select s.SeizureId,s.CaseId,s.CaseNo,s.Currency,s.CustId,s.CustName,s.BranchNo,s.BranchName,s.Account,s.SeizureAmount,s.CancelAmount,s.AccountStatus,s.PayAmount,s.TripAmount,
                                        s.ExchangeRate,s.SeizureAmountNtd from  CaseSeizure s
                                        where  SeizureId=@SeizureId";

            Parameter.Clear();
            Parameter.Add(new CommandParameter("SeizureId", SeizureId));
            //IList<CaseSeizure> list = base.SearchList<CaseSeizure>(strSql);
            //return list.FirstOrDefault();
            return dbtrans == null ? SearchList<CaseSeizure>(strSql).FirstOrDefault() : SearchList<CaseSeizure>(strSql, dbtrans)[0];
        }

        public bool SeizureMaintainEdit(CaseSeizure model)
        {
            return UpdateSeizureSetting(model);
        }

        public bool DeleteSeizureMaintain(Guid caseId, int seizureId, string caseno)
        {
            IDbConnection dbConnection = OpenConnection();
            IDbTransaction dbTransaction = null;
            bool flag = true;
            try
            {
                using (dbConnection)
                {
                    dbTransaction = dbConnection.BeginTransaction();
                    if (IsCaseNoExist(caseno) == "1")//扣押案件
                    {
                        flag = flag && new CaseMasterBIZ().DeleteCaseMaster(caseId, dbTransaction);
                    }

                    flag = flag && DeleteSeizureSetting(caseId, seizureId, dbTransaction);

                    dbTransaction.Commit();
                }
                return true;
            }
            catch (Exception)
            {
                try
                {
                    if (dbTransaction != null) dbTransaction.Rollback();
                }
                catch (Exception)
                {
                    // ignored
                }
                return false;
            }
        }

        public bool DeleteSeizureSetting(Guid caseId, int seizureId, IDbTransaction dbtrans = null)
        {
            string strSql = "DELETE CaseSeizure WHERE " +
                            "CaseId = @CaseId " +
                            " AND (ISNULL(@SeizureId,'') = '' OR SeizureId = @SeizureId)";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("CaseId", caseId));
            Parameter.Add(new CommandParameter("SeizureId", seizureId));
            return dbtrans == null ? ExecuteNonQuery(strSql) > 0 : ExecuteNonQuery(strSql, dbtrans) > 0;
        }

        public bool DeleteSeizureSetting(Guid caseId, IDbTransaction dbtrans = null)
        {
            string strSql = "DELETE CaseSeizure WHERE  CaseId = @CaseId ";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("CaseId", caseId));
            return dbtrans == null ? ExecuteNonQuery(strSql) > 0 : ExecuteNonQuery(strSql, dbtrans) > 0;
        }

        #region 檢查此CaseNo是否存在
        /// <summary>
        /// 檢查此CaseNo是否存在
        /// </summary>
        /// <param name="txtGovNo"></param>
        /// <returns></returns>
        public string IsCaseNoExist(string CaseNo)
        {
            string strSql = "select count(0) from CaseSeizure where  CaseNo=@CaseNo";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("@CaseNo", CaseNo));

            try
            {
                int a = (int)ExecuteScalar(strSql);
                if (a == 1)
                {
                    return "1";//只有一條
                }
                return "0";//無重複
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        #endregion
        private int GetMaxSeqByCaseId(Guid caseId)
        {
            string selectSql = @"select MAX(Seq) from CaseSeizure where CaseId =@CaseId";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("@CaseId", caseId));

            var aaa = base.ExecuteScalar(selectSql).ToString();
            int result = base.ExecuteScalar(selectSql).ToString() == "" ? 0 : Convert.ToInt32(base.ExecuteScalar(selectSql).ToString());
            return result + 1;
        }
        public bool InsertSeizureSetting(CaseSeizure item, IDbTransaction trans = null)
        {
            item.SeizureSeq = GetMaxSeqByCaseId(item.CaseId);
            string strSql = @"INSERT INTO [CaseSeizure]
                            ([CaseId],[CaseNo],[CustId],[CustName],[BranchNo],[BranchName],[Account],[AccountStatus]
                            ,[Currency],[Balance],[SeizureAmount],[ExchangeRate],[SeizureAmountNtd],[PayAmount],[SeizureStatus]
                            ,[CreatedUser],[CreatedDate],[ProdCode],[Link],[SegmentCode],[OtherSeizure],[TxtStatus],[Seq],[TxtProdCode])VALUES
                            (@CaseId,@CaseNo,@CustId,@CustName,@BranchNo,@BranchName,@Account,@AccountStatus
                            ,@Currency,@Balance,@SeizureAmount,@ExchangeRate,@SeizureAmountNtd,0,@SeizureStatus,@CreatedUser,GETDATE(),@ProdCode
                            ,@Link,@SegmentCode,@OtherSeizure,@TxtStatus,@Seq,@TxtProdCode)";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("@CaseId", item.CaseId));
            Parameter.Add(new CommandParameter("@CaseNo", item.CaseNo));
            Parameter.Add(new CommandParameter("@CustId", item.CustId));
            Parameter.Add(new CommandParameter("@CustName", item.CustName));
            Parameter.Add(new CommandParameter("@BranchNo", item.BranchNo));
            Parameter.Add(new CommandParameter("@BranchName", item.BranchName));
            Parameter.Add(new CommandParameter("@Account", item.Account));
            Parameter.Add(new CommandParameter("@AccountStatus", item.AccountStatus));
            Parameter.Add(new CommandParameter("@Currency", item.Currency));
            Parameter.Add(new CommandParameter("@Balance", item.Balance.Trim()));
            Parameter.Add(new CommandParameter("@SeizureAmount", item.SeizureAmount));
            Parameter.Add(new CommandParameter("@ExchangeRate", item.ExchangeRate));
            Parameter.Add(new CommandParameter("@SeizureAmountNtd", item.SeizureAmountNtd));
            Parameter.Add(new CommandParameter("@SeizureStatus", SeizureStatus.AfterSeizureSetting));
            Parameter.Add(new CommandParameter("@CreatedUser", item.CreatedUser));
            Parameter.Add(new CommandParameter("@ProdCode", item.ProdCode));
            Parameter.Add(new CommandParameter("@Link", item.Link)); ;
            Parameter.Add(new CommandParameter("@SegmentCode", item.SegmentCode));
            Parameter.Add(new CommandParameter("@OtherSeizure", item.OtherSeizure));
            Parameter.Add(new CommandParameter("@TxtStatus", item.TxtStatus));
            Parameter.Add(new CommandParameter("@Seq", item.SeizureSeq));
            Parameter.Add(new CommandParameter("@TxtProdCode", item.TxtProdCode));
            return trans == null ? ExecuteNonQuery(strSql) > 0 : ExecuteNonQuery(strSql, trans) > 0;
        }
        public bool UpdateSeizureBalance(CaseSeizure item, IDbTransaction trans = null)
        {
            string strSql = @"UPDATE [CaseSeizure]
                            SET 
                                [CustId]=@CustId,
                                [CustName]=@CustName,
                                [BranchNo]=@BranchNo,
                                [BranchName]=@BranchName,
                                [Account]=@Account,
                                [Currency]=@Currency,
	                            [SeizureAmount] = @SeizureAmount ,
	                            [ExchangeRate] = @ExchangeRate ,
	                            [SeizureAmountNtd] = @SeizureAmountNtd ,
	                            [ModifiedUser] = @ModifiedUser ,
	                            [ModifiedDate] = GETDATE(),
                                [ProdCode] = @ProdCode,
                                [Link] = @Link,
                                [SegmentCode] = @SegmentCode,
                                [OtherSeizure] = @OtherSeizure,
                                [TxtStatus] = @TxtStatus,
                                [CancelAmount] = @CancelAmount,
                                [TripAmount] = @TripAmount,
                                [PayAmount] = @PayAmount,
                                [TxtProdCode] = @TxtProdCode,
                                [Balance] = @Balance
                            WHERE [SeizureId] = @SeizureId";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("@SeizureId", item.SeizureId));
            //*扣押维护新增
            Parameter.Add(new CommandParameter("@CustId", item.CustId));
            Parameter.Add(new CommandParameter("@CustName", item.CustName));
            Parameter.Add(new CommandParameter("@BranchNo", item.BranchNo));
            Parameter.Add(new CommandParameter("@BranchName", item.BranchName));
            Parameter.Add(new CommandParameter("@Account", item.Account));
            Parameter.Add(new CommandParameter("@Currency", item.Currency));
            Parameter.Add(new CommandParameter("@SeizureAmount", item.SeizureAmount));
            Parameter.Add(new CommandParameter("@ExchangeRate", item.ExchangeRate));
            Parameter.Add(new CommandParameter("@SeizureAmountNtd", item.SeizureAmountNtd));
            Parameter.Add(new CommandParameter("@ModifiedUser", item.ModifiedUser));
            Parameter.Add(new CommandParameter("@ProdCode", item.ProdCode));
            Parameter.Add(new CommandParameter("@Link", item.Link)); ;
            Parameter.Add(new CommandParameter("@SegmentCode", item.SegmentCode));
            Parameter.Add(new CommandParameter("@OtherSeizure", item.OtherSeizure));
            Parameter.Add(new CommandParameter("@TxtStatus", item.TxtStatus));
            Parameter.Add(new CommandParameter("@CancelAmount", item.CancelAmount));//已撤銷金額
            Parameter.Add(new CommandParameter("@TripAmount", item.TripAmount));//解扣金額
            Parameter.Add(new CommandParameter("@PayAmount", item.PayAmount));//支付金額
            Parameter.Add(new CommandParameter("@TxtProdCode", item.TxtProdCode));//產品代碼
            Parameter.Add(new CommandParameter("@Balance", item.Balance));//可用餘額
            return trans == null ? ExecuteNonQuery(strSql) > 0 : ExecuteNonQuery(strSql, trans) > 0;
        }

        public bool UpdateSeizureSetting(CaseSeizure item, IDbTransaction trans = null)
        {
            string strSql = @"UPDATE [CaseSeizure]
                            SET 
                                [CustId]=@CustId,
                                [CustName]=@CustName,
                                [BranchNo]=@BranchNo,
                                [BranchName]=@BranchName,
                                [Account]=@Account,
                                [Currency]=@Currency,
	                            [SeizureAmount] = @SeizureAmount ,
	                            [ExchangeRate] = @ExchangeRate ,
	                            [SeizureAmountNtd] = @SeizureAmountNtd ,
	                            [ModifiedUser] = @ModifiedUser ,
	                            [ModifiedDate] = GETDATE(),
                                [ProdCode] = @ProdCode,
                                [Link] = @Link,
                                [SegmentCode] = @SegmentCode,
                                [OtherSeizure] = @OtherSeizure,
                                [TxtStatus] = @TxtStatus,
                                [CancelAmount] = @CancelAmount,
                                [TripAmount] = @TripAmount,
                                [PayAmount] = @PayAmount,
                                [TxtProdCode] = @TxtProdCode
                            WHERE [SeizureId] = @SeizureId";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("@SeizureId", item.SeizureId));
            //*扣押维护新增
            Parameter.Add(new CommandParameter("@CustId", item.CustId));
            Parameter.Add(new CommandParameter("@CustName", item.CustName));
            Parameter.Add(new CommandParameter("@BranchNo", item.BranchNo));
            Parameter.Add(new CommandParameter("@BranchName", item.BranchName));
            Parameter.Add(new CommandParameter("@Account", item.Account));
            Parameter.Add(new CommandParameter("@Currency", item.Currency));
            Parameter.Add(new CommandParameter("@SeizureAmount", item.SeizureAmount));
            Parameter.Add(new CommandParameter("@ExchangeRate", item.ExchangeRate));
            Parameter.Add(new CommandParameter("@SeizureAmountNtd", item.SeizureAmountNtd));
            Parameter.Add(new CommandParameter("@ModifiedUser", item.ModifiedUser));
            Parameter.Add(new CommandParameter("@ProdCode", item.ProdCode));
            Parameter.Add(new CommandParameter("@Link", item.Link)); ;
            Parameter.Add(new CommandParameter("@SegmentCode", item.SegmentCode));
            Parameter.Add(new CommandParameter("@OtherSeizure", item.OtherSeizure));
            Parameter.Add(new CommandParameter("@TxtStatus", item.TxtStatus));
            Parameter.Add(new CommandParameter("@CancelAmount", item.CancelAmount));//已撤銷金額
            Parameter.Add(new CommandParameter("@TripAmount", item.TripAmount));//解扣金額
            Parameter.Add(new CommandParameter("@PayAmount", item.PayAmount));//支付金額
            Parameter.Add(new CommandParameter("@TxtProdCode", item.TxtProdCode));//產品代碼
            return trans == null ? ExecuteNonQuery(strSql) > 0 : ExecuteNonQuery(strSql, trans) > 0;
        }
        #endregion

        #region 支付
        /// <summary>
        /// 通過CaseId取得案件下所有還沒設定過支付的扣押
        /// </summary>
        /// <param name="caseId"></param>
        /// <returns></returns>
        public IList<CaseSeizure> GetCaseSeizureWithoutPaySetting(Guid caseId)
        {
            IList<CaseSeizure> list = GetCaseSeizure(caseId);
            List<CaseSeizure> list2 = list != null ? list.ToList() : new List<CaseSeizure>();
            list2.RemoveAll(m => m.SeizureStatus != SeizureStatus.AfterSeizureSetting);
            return list2;
        }

        /// <summary>
        /// 通過CaseId取得案件下所有已經設定過支付的扣押
        /// </summary>
        /// <param name="caseId"></param>
        /// <param name="trans"></param>
        /// <returns></returns>
        public IList<CaseSeizure> GetCaseSeizureWithPaySetting(Guid caseId, IDbTransaction trans = null)
        {
            string strSql = ";with  T1 as(SELECT " +
                            "s.[SeizureId],s.[CaseId],s.[PayCaseId],s.[CaseNo],s.[CustId],s.[CustName],s.[BranchNo],s.[BranchName],s.[Account],s.[AccountStatus],s.[Currency],s.[Balance], " +
                            "s.[SeizureAmount],s.[ExchangeRate],s.[SeizureAmountNtd],s.[PayAmount],s.[TripAmount],s.[SeizureStatus],s.[CreatedUser],s.[CreatedDate],s.[ModifiedUser],s.[ModifiedDate] " +
                            ", Convert(Nvarchar(10),m.GovDate,111) as GovDate,m.GovNo,m.GovUnit ,ISNULL(d.FKSNO,9999999) as FKSNO,ISNULL(d.SNO,9999999) as SNO " +
                            "FROM [CaseSeizure] s left join CaseMaster m on s.CaseId=m.CaseId " +
                            "left join  (SELECT top 1 ROW_NUMBER() OVER (PARTITION BY [CustomerId] ORDER BY [SNO] DESC) AS RowID, [SNO],[CustomerId],[CustomerName] FROM [TX_60491_Grp] " +
                            "WHERE TX_60491_Grp.CaseID = @CaseId) h on h.CustomerId = s.CustId " +                          
                            "left join TX_60491_Detl d on s.CustId = d.CUST_ID and s.Account = d.Account and d.FKSNO = h.SNO " +
                            "WHERE s.[PayCaseId] = @CaseId AND s.[SeizureStatus] = @SeizureStatus " +
                            " and ((len(s.[CustId]) >= 10 and SUBSTRING(s.[CustId],1,10) IN (SELECT [ObligorNo] FROM [CaseObligor] WHERE CaseId = @CaseId)) or " +
                            "(len(s.[CustId]) < 10 and SUBSTRING(s.[CustId],1,8) IN (SELECT [ObligorNo] FROM [CaseObligor] WHERE CaseId = @CaseId))) " +
                            ") ,T2 as (select ColumnValueBefore,ColumnValueAfter,ColumnID,LinkDataKey from CaseDataLog c1,CaseSeizure " +
                            "where c1.CaseId=@CaseId and TabName='支付設定' and TableName='CaseSeizure' and LinkDataKey=cast(SeizureId as nvarchar(100) ) " +
                            "and TXDateTime=(select max(TXDateTime) from CaseDataLog c2 where c1.CaseId=c2.CaseId and c1.TabName=c2.TabName and c1.TableName=c2.TableName and c1.LinkDataKey=c2.LinkDataKey))," +
                            "T3 as (select T1.*, " +
                            "(select case  when ColumnValueBefore<>AccountStatus then 'true' else 'false' end from T2 where ColumnID='AccountStatus' and LinkDataKey=cast(SeizureId as nvarchar(100) )) as AccountStatusflag," +
                            "(select case  when ColumnValueBefore<>PayAmount then 'true' else 'false' end from T2 where ColumnID='PayAmount' and LinkDataKey=cast(SeizureId as nvarchar(100) )) as PayAmountflag," +
                            "(select case  when ColumnValueBefore<>TripAmount then 'true' else 'false' end from T2 where ColumnID='TripAmount' and LinkDataKey=cast(SeizureId as nvarchar(100) )) as TripAmountflag from T1)" +
                            "select *,1 as IsFromZJ from T3  order by  CaseNo asc, GovDate,GovUnit,Account ";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("@CaseId", caseId));
            Parameter.Add(new CommandParameter("@SeizureStatus", SeizureStatus.AfterPaySetting));
            return trans == null ? SearchList<CaseSeizure>(strSql) : SearchList<CaseSeizure>(strSql, trans);

        }

        public IList<CaseSeizure> GetCaseSeizureWithEPaySetting(Guid caseId, IDbTransaction trans = null)
        {
            string strSql = ";with  T1 as(SELECT " +
                            "s.[SeizureId],s.[CaseId],s.[PayCaseId],s.[CaseNo],s.[CustId],s.[CustName],s.[BranchNo],s.[BranchName],s.[Account],s.[AccountStatus],s.[Currency],s.[Balance], " +
                            "s.[SeizureAmount],s.[ExchangeRate],s.[SeizureAmountNtd],s.[PayAmount],s.[TripAmount],s.[SeizureStatus],s.[CreatedUser],s.[CreatedDate],s.[ModifiedUser],s.[ModifiedDate] " +
                            ", Convert(Nvarchar(10),m.GovDate,111) as GovDate,m.GovNo,m.GovUnit ,ISNULL(d.FKSNO,9999999) as FKSNO,ISNULL(d.SNO,9999999) as SNO " +
                            "FROM [CaseSeizure] s left join CaseMaster m on s.CaseId=m.CaseId " +
                            "left join  (SELECT top 1 ROW_NUMBER() OVER (PARTITION BY [CustomerId] ORDER BY [SNO] DESC) AS RowID, [SNO],[CustomerId],[CustomerName] FROM [TX_60491_Grp] " +
                            "WHERE TX_60491_Grp.CaseID = @CaseId) h on h.CustomerId = s.CustId " +
                            "left join TX_60491_Detl d on s.CustId = d.CUST_ID and s.Account = d.Account and d.FKSNO = h.SNO " +
                            "WHERE s.[PayCaseId] = @CaseId AND ( s.[SeizureStatus] = '1' or  s.[SeizureStatus] = '3')  " +
                            " and ((len(s.[CustId]) >= 10 and SUBSTRING(s.[CustId],1,10) IN (SELECT [ObligorNo] FROM [CaseObligor] WHERE CaseId = @CaseId)) or " +
                            "(len(s.[CustId]) < 10 and SUBSTRING(s.[CustId],1,8) IN (SELECT [ObligorNo] FROM [CaseObligor] WHERE CaseId = @CaseId))) " +
                            ") ,T2 as (select ColumnValueBefore,ColumnValueAfter,ColumnID,LinkDataKey from CaseDataLog c1,CaseSeizure " +
                            "where c1.CaseId=@CaseId and TabName='支付設定' and TableName='CaseSeizure' and LinkDataKey=cast(SeizureId as nvarchar(100) ) " +
                            "and TXDateTime=(select max(TXDateTime) from CaseDataLog c2 where c1.CaseId=c2.CaseId and c1.TabName=c2.TabName and c1.TableName=c2.TableName and c1.LinkDataKey=c2.LinkDataKey))," +
                            "T3 as (select T1.*, " +
                            "(select case  when ColumnValueBefore<>AccountStatus then 'true' else 'false' end from T2 where ColumnID='AccountStatus' and LinkDataKey=cast(SeizureId as nvarchar(100) )) as AccountStatusflag," +
                            "(select case  when ColumnValueBefore<>PayAmount then 'true' else 'false' end from T2 where ColumnID='PayAmount' and LinkDataKey=cast(SeizureId as nvarchar(100) )) as PayAmountflag," +
                            "(select case  when ColumnValueBefore<>TripAmount then 'true' else 'false' end from T2 where ColumnID='TripAmount' and LinkDataKey=cast(SeizureId as nvarchar(100) )) as TripAmountflag from T1)" +
                            "select *,1 as IsFromZJ from T3  order by  CaseNo asc, GovDate,GovUnit,Account ";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("@CaseId", caseId));
            return trans == null ? SearchList<CaseSeizure>(strSql) : SearchList<CaseSeizure>(strSql, trans);

        }
        /// <summary>
        /// 通過CaseId得到這個案件下客戶,所有沒有支付的扣押
        /// </summary>
        /// <param name="caseId"></param>
        /// <returns></returns>
        public IList<CaseSeizure> GetCaseSeizureWithoutPaySettingByCustomerList(Guid caseId)
        {
            //string strSql = "SELECT " +
            //                "s.[SeizureId],s.[CaseId],s.[PayCaseId],s.[CaseNo],s.[CustId],s.[CustName],s.[BranchNo],s.[BranchName],s.[Account],s.[AccountStatus],s.[Currency],s.[Balance], " +
            //                "s.[SeizureAmount],s.[ExchangeRate],s.[SeizureAmountNtd],s.[PayAmount],s.[SeizureStatus],s.[CreatedUser],s.[CreatedDate],s.[ModifiedUser],s.[ModifiedDate],s.[CancelAmount] " +
            //                ",Convert(Nvarchar(10), m.GovDate,111) as GovDate,m.GovNo,m.GovUnit,ISNULL(d.FKSNO,9999999) as FKSNO,ISNULL(d.SNO,9999999) as SNO from CaseSeizure s left join CaseMaster m on s.CaseId=m.CaseId " +
            //                "left join  (SELECT top 1 ROW_NUMBER() OVER (PARTITION BY [CustomerId] ORDER BY [SNO] DESC) AS RowID, [SNO],[CustomerId],[CustomerName] FROM [TX_60491_Grp] " +
            //                "WHERE TX_60491_Grp.CaseID = @CaseId) h on h.CustomerId = s.CustId " +
            //                "left join TX_60491_Detl d on s.CustId = d.CUST_ID and s.Account = d.Account and d.FKSNO = h.SNO " +
            //                "WHERE ((len(s.[CustId]) >= 10 and SUBSTRING(s.[CustId],1,10) IN (SELECT [ObligorNo] FROM [CaseObligor] WHERE CaseId = @CaseId)) or " +
            //                "(len(s.[CustId]) < 10 and substring(s.[CustId],1,8) IN (SELECT [ObligorNo] FROM [CaseObligor] WHERE CaseId = @CaseId))) " +
            //                "AND s.[SeizureStatus] = @SeizureStatus   order by  m.CaseNo,m.GovDate,m.GovUnit,s.Account";
            // 更新外國人ID
            string strSql = "SELECT " +
                            "s.[SeizureId],s.[CaseId],s.[PayCaseId],s.[CaseNo],s.[CustId],s.[CustName],s.[BranchNo],s.[BranchName],s.[Account],s.[AccountStatus],s.[Currency],s.[Balance], " +
                            "s.[SeizureAmount],s.[ExchangeRate],s.[SeizureAmountNtd],s.[PayAmount],s.[SeizureStatus],s.[CreatedUser],s.[CreatedDate],s.[ModifiedUser],s.[ModifiedDate],s.[CancelAmount] " +
                            ",Convert(Nvarchar(10), m.GovDate,111) as GovDate,m.GovNo,m.GovUnit,ISNULL(d.FKSNO,9999999) as FKSNO,ISNULL(d.SNO,9999999) as SNO from CaseSeizure s left join CaseMaster m on s.CaseId=m.CaseId " +
                            "left join  (SELECT top 1 ROW_NUMBER() OVER (PARTITION BY [CustomerId] ORDER BY [SNO] DESC) AS RowID, [SNO],[CustomerId],[CustomerName] FROM [TX_60491_Grp] " +
                            "WHERE TX_60491_Grp.CaseID = @CaseId) h on h.CustomerId = s.CustId " +
                            "left join TX_60491_Detl d on s.CustId = d.CUST_ID and s.Account = d.Account and d.FKSNO = h.SNO " +
                            "WHERE  s.Custid IN (SELECT [ObligorNo] FROM [CaseObligor] WHERE CaseId = @CaseId) " +
                            "AND s.[SeizureStatus] = @SeizureStatus   order by  m.CaseNo,m.GovDate,m.GovUnit,s.Account";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("@CaseId", caseId));
            Parameter.Add(new CommandParameter("@SeizureStatus", SeizureStatus.AfterSeizureSetting));
            return SearchList<CaseSeizure>(strSql);
        }

        /// <summary>
        /// 儲存支付設定
        /// </summary>
        /// <param name="saveList"></param>
        /// <returns></returns>
        public bool SavePaySetting(List<CaseSeizure> saveList, Guid CaseId)
        {
            IDbConnection dbConnection = OpenConnection();
            IDbTransaction dbTransaction = null;
            bool rtn = true;
            try
            {
                using (dbConnection)
                {
                    //* 這裡支付設定是基於扣押的.所以就不分新增和修改啦
                    dbTransaction = dbConnection.BeginTransaction();
                    IList<CaseSeizure> listOld = GetCaseSeizureWithEPaySetting(CaseId, dbTransaction);
                    if (saveList != null && saveList.Any())
                    {
                        foreach (CaseSeizure formItem in saveList)
                        {
                            if ( formItem.Account == null || formItem.Account.ToString().Length < 8)
                            {
                                continue;
                            }
                            CaseSeizure cs = GetCaseSeizure(formItem.SeizureId.ToString(), dbTransaction);
                            rtn = rtn && UpdateNewPaySetting(formItem, dbTransaction);
                            if (listOld.Count > 0)
                            {
                                #region 狀態
                                if (cs.AccountStatus != formItem.AccountStatus)
                                {
                                    CaseDataLog log = new CaseDataLog();
                                    Guid TXSNO = Guid.NewGuid();
                                    DateTime TXDateTime = DateTime.Parse(System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                                    LdapEmployeeBiz empBiz = new LdapEmployeeBiz();
                                    log.TXType = "修改";
                                    log.TXUser = empBiz.Account != null && !string.IsNullOrEmpty(empBiz.Account) ? empBiz.Account : " ";
                                    log.TXUserName = empBiz.AccountName != null && !string.IsNullOrEmpty(empBiz.AccountName) ? empBiz.AccountName : " ";
                                    if (PageFrom == "1")
                                    {
                                        log.md_FuncID = "Menu.CollectionToAgent";
                                        log.TITLE = "收發作業-收發代辦";
                                    }
                                    else if (PageFrom == "2")
                                    {
                                        log.md_FuncID = "Menu.CollectionToAgent";
                                        log.TITLE = "經辦作業-待辦理";
                                    }

                                    log.TXSNO = TXSNO;
                                    log.TXDateTime = TXDateTime;
                                    log.ColumnID = "AccountStatus";
                                    log.ColumnName = "狀態";
                                    log.ColumnValueBefore = string.IsNullOrEmpty(cs.AccountStatus) ? "" : cs.AccountStatus.ToString();
                                    log.ColumnValueAfter = formItem.AccountStatus.ToString();
                                    log.TabID = "Tab2-1";
                                    log.TabName = "支付設定";
                                    log.TableName = "CaseSeizure";
                                    log.DispSrNo = 6;
                                    log.TableDispActive = "1";
                                    log.CaseId = CaseId.ToString();
                                    log.LinkDataKey = formItem.SeizureId.ToString();
                                    InsertCaseDataLog(log);
                                }
                                #endregion

                                #region 支付金額
                                if (cs.PayAmount != formItem.PayAmount)
                                {
                                    CaseDataLog log = new CaseDataLog();
                                    Guid TXSNO = Guid.NewGuid();
                                    DateTime TXDateTime = DateTime.Parse(System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                                    LdapEmployeeBiz empBiz = new LdapEmployeeBiz();
                                    log.TXType = "修改";
                                    log.TXUser = empBiz.Account != null && !string.IsNullOrEmpty(empBiz.Account) ? empBiz.Account : " ";
                                    log.TXUserName = empBiz.AccountName != null && !string.IsNullOrEmpty(empBiz.AccountName) ? empBiz.AccountName : " ";
                                    if (PageFrom == "1")
                                    {
                                        log.md_FuncID = "Menu.CollectionToAgent";
                                        log.TITLE = "收發作業-收發代辦";
                                    }
                                    else if (PageFrom == "2")
                                    {
                                        log.md_FuncID = "Menu.CollectionToAgent";
                                        log.TITLE = "經辦作業-待辦理";
                                    }

                                    log.TXSNO = TXSNO;
                                    log.TXDateTime = TXDateTime;
                                    log.ColumnID = "PayAmount";
                                    log.ColumnName = "支付金額";
                                    log.ColumnValueBefore = cs.PayAmount.ToString();
                                    log.ColumnValueAfter = formItem.PayAmount.ToString();
                                    log.TabID = "Tab2-1";
                                    log.TabName = "支付設定";
                                    log.TableName = "CaseSeizure";
                                    log.DispSrNo = 17;
                                    log.TableDispActive = "1";
                                    log.CaseId = CaseId.ToString();
                                    log.LinkDataKey = formItem.SeizureId.ToString();
                                    InsertCaseDataLog(log);
                                }
                                #endregion

                                #region 解扣金額
                                if (cs.TripAmount != formItem.TripAmount)
                                {
                                    CaseDataLog log = new CaseDataLog();
                                    Guid TXSNO = Guid.NewGuid();
                                    DateTime TXDateTime = DateTime.Parse(System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                                    LdapEmployeeBiz empBiz = new LdapEmployeeBiz();
                                    log.TXType = "修改";
                                    log.TXUser = empBiz.Account != null && !string.IsNullOrEmpty(empBiz.Account) ? empBiz.Account : " ";
                                    log.TXUserName = empBiz.AccountName != null && !string.IsNullOrEmpty(empBiz.AccountName) ? empBiz.AccountName : " ";
                                    if (PageFrom == "1")
                                    {
                                        log.md_FuncID = "Menu.CollectionToAgent";
                                        log.TITLE = "收發作業-收發代辦";
                                    }
                                    else if (PageFrom == "2")
                                    {
                                        log.md_FuncID = "Menu.CollectionToAgent";
                                        log.TITLE = "經辦作業-待辦理";
                                    }

                                    log.TXSNO = TXSNO;
                                    log.TXDateTime = TXDateTime;
                                    log.ColumnID = "TripAmount";
                                    log.ColumnName = "解扣金額";
                                    log.ColumnValueBefore = cs.TripAmount.ToString();
                                    log.ColumnValueAfter = formItem.TripAmount.ToString();
                                    log.TabID = "Tab2-1";
                                    log.TabName = "支付設定";
                                    log.TableName = "CaseSeizure";
                                    log.DispSrNo = 18;
                                    log.TableDispActive = "1";
                                    log.CaseId = CaseId.ToString();
                                    log.LinkDataKey = formItem.SeizureId.ToString();
                                    InsertCaseDataLog(log);
                                }
                                #endregion
                            }
                        }
                    }
                    if (listOld != null && listOld.Any())
                    {
                        foreach (CaseSeizure oldItem in listOld.Where(a => saveList == null || !saveList.Exists(b => b.SeizureId == a.SeizureId)))
                        {
                            rtn = rtn && ClearPaySetting(oldItem, dbTransaction);
                        }
                    }

                    if (rtn)
                        dbTransaction.Commit();
                    else
                        dbTransaction.Rollback();
                    return rtn;
                }
            }
            catch (Exception)
            {
                try
                {
                    if (dbTransaction != null) dbTransaction.Rollback();
                }
                catch (Exception ex)
                {
                    // ignored
                }
                return false;
            }
        }
        /// <summary>
        /// 實際針對某一個支付設定進行儲存
        /// </summary>
        /// <param name="item"></param>
        /// <param name="trans"></param>
        /// <returns></returns>
        public bool UpdatePaySetting(CaseSeizure item, IDbTransaction trans = null)
        {
            string strSql = "UPDATE [CaseSeizure] " +
                            " SET [PayCaseId] = @PayCaseId , " +
                            "     [AccountStatus] = @AccountStatus , " +
                            "     [PayAmount] = @PayAmount , " +
                            "     [TripAmount] = @TripAmount , " +
                            "     [SeizureStatus] = @SeizureStatus , " +
                            "     [ModifiedUser] = @ModifiedUser, " +
                            "     [ModifiedDate] = GETDATE() " +
                            "WHERE [SeizureId] = @SeizureId ";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("@PayCaseId", item.PayCaseId));
            Parameter.Add(new CommandParameter("@AccountStatus", item.AccountStatus));
            Parameter.Add(new CommandParameter("@PayAmount", item.PayAmount));
            Parameter.Add(new CommandParameter("@TripAmount", item.TripAmount));
            Parameter.Add(new CommandParameter("@SeizureStatus", SeizureStatus.AfterPaySetting));
            Parameter.Add(new CommandParameter("@ModifiedUser", item.ModifiedUser));
            Parameter.Add(new CommandParameter("@SeizureId", item.SeizureId));
            return trans == null ? ExecuteNonQuery(strSql) > 0 : ExecuteNonQuery(strSql, trans) > 0;
        }


        /// <summary>
        /// 實際針對某一個支付設定進行儲存
        /// </summary>
        /// <param name="item"></param>
        /// <param name="trans"></param>
        /// <returns></returns>
        public bool UpdateNewPaySetting(CaseSeizure item, IDbTransaction trans = null)
        {
            string strSql1 = @"SELECT SeizureStatus from CaseSeizure where  [SeizureId] = "+ item.SeizureId.ToString()  ;
            DataTable dt =  Search(strSql1);
            if (dt.Rows.Count > 0)
            {
                if (dt.Rows[0]["SeizureStatus"].ToString().Trim() == "1" || dt.Rows[0]["SeizureStatus"].ToString().Trim() == "3")
                {
                    item.SeizureStatus = dt.Rows[0]["SeizureStatus"].ToString().Trim();
                }
                else
                {
                    item.SeizureStatus = "3";
                }
            }
            else
            {
                item.SeizureStatus = "3";
            }
           
            string strSql = "UPDATE [CaseSeizure] " +
                            " SET [PayCaseId] = @PayCaseId , " +
                            "     [AccountStatus] = @AccountStatus , " +
                            "     [PayAmount] = @PayAmount , " +
                            "     [TripAmount] = @TripAmount , " +
                            "     [SeizureStatus] = @SeizureStatus , " +
                            "     [ModifiedUser] = @ModifiedUser, " +
                            "     [ModifiedDate] = GETDATE() " +
                            "WHERE [SeizureId] = @SeizureId ";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("@PayCaseId", item.PayCaseId));
            Parameter.Add(new CommandParameter("@AccountStatus", item.AccountStatus));
            Parameter.Add(new CommandParameter("@PayAmount", item.PayAmount));
            Parameter.Add(new CommandParameter("@TripAmount", item.TripAmount));
            //Parameter.Add(new CommandParameter("@SeizureStatus", SeizureStatus.AfterPaySetting));
            if ((item.SeizureStatus == null ) ||(item.SeizureStatus.Trim() == ""))
            {
                item.SeizureStatus = "3";
            }
            Parameter.Add(new CommandParameter("@SeizureStatus", item.SeizureStatus));
            Parameter.Add(new CommandParameter("@ModifiedUser", item.ModifiedUser));
            Parameter.Add(new CommandParameter("@SeizureId", item.SeizureId));
            return trans == null ? ExecuteNonQuery(strSql) > 0 : ExecuteNonQuery(strSql, trans) > 0;
        }

        public bool UpdateResetPaySetting(CaseSeizure item, IDbTransaction trans = null)
        {
            string strSql = "UPDATE [CaseSeizure] " +
                            " SET [PayCaseId] = @PayCaseId , " +
                            "     [AccountStatus] = @AccountStatus , " +
                            "     [PayAmount] = @PayAmount , " +
                            "     [TripAmount] = @TripAmount , " +
                            "     [SeizureStatus] = @SeizureStatus , " +
                            "     [ModifiedUser] = @ModifiedUser, " +
                            "     [ModifiedDate] = GETDATE() " +
                            "WHERE [SeizureId] = @SeizureId ";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("@PayCaseId", item.PayCaseId));
            Parameter.Add(new CommandParameter("@AccountStatus", item.AccountStatus));
            Parameter.Add(new CommandParameter("@PayAmount", item.PayAmount));
            Parameter.Add(new CommandParameter("@TripAmount", item.TripAmount));
            Parameter.Add(new CommandParameter("@SeizureStatus", "3"));
            Parameter.Add(new CommandParameter("@ModifiedUser", item.ModifiedUser));
            Parameter.Add(new CommandParameter("@SeizureId", item.SeizureId));
            return trans == null ? ExecuteNonQuery(strSql) > 0 : ExecuteNonQuery(strSql, trans) > 0;
        }

        public bool ClearPaySetting(CaseSeizure item, IDbTransaction trans = null)
        {
            string strSql = "UPDATE [CaseSeizure] " +
                            " SET [PayCaseId] = @PayCaseId , " +
                            "     [CancelCaseId] = @PayCaseId , " +
                            "     [PayAmount] = @PayAmount , " +
                            "     [SeizureStatus] = @SeizureStatus , " +
                            "     [ModifiedUser] = @ModifiedUser, " +
                            "     [ModifiedDate] = GETDATE() " +
                            "WHERE [SeizureId] = @SeizureId ";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("@PayCaseId", null));
            Parameter.Add(new CommandParameter("@PayAmount", "0"));
            Parameter.Add(new CommandParameter("@SeizureStatus", SeizureStatus.AfterSeizureSetting));
            Parameter.Add(new CommandParameter("@ModifiedUser", item.ModifiedUser));
            Parameter.Add(new CommandParameter("@SeizureId", item.SeizureId));
            return trans == null ? ExecuteNonQuery(strSql) > 0 : ExecuteNonQuery(strSql, trans) > 0;
        }
        #endregion

        #region 取消扣押
        /// <summary>
        /// 通過CaseId取得案件下所有已經設定過取消的扣押
        /// </summary>
        /// <param name="caseId"></param>
        /// <returns></returns>
        public IList<CaseSeizure> GetCaseSeizureWithCancel(Guid caseId, IDbTransaction trans = null)
        {
            string strSql = ";with  T1 as(SELECT " +
                            "s.[SeizureId],s.[CaseId],s.[PayCaseId],s.[CaseNo],s.[CustId],s.[CustName],s.[BranchNo],s.[BranchName],s.[Account],s.[AccountStatus],s.[Currency],s.[Balance],s.[CancelAmount], " +
                            "s.[SeizureAmount],s.[ExchangeRate],s.[SeizureAmountNtd],s.[PayAmount],s.[SeizureStatus],s.[CreatedUser],s.[CreatedDate],s.[ModifiedUser],s.[ModifiedDate],s.[CancelCaseId], " +
                            "s.[TxtStatus],Convert(Nvarchar(10), m.GovDate,111) as GovDate,m.GovNo,m.GovUnit FROM [CaseSeizure]  s left join CaseMaster m on s.CaseId=m.CaseId " +
                            "WHERE s.[CancelCaseId] = @CaseId AND s.[SeizureStatus] = @SeizureStatus) ,T2 as (select ColumnValueBefore,ColumnValueAfter,ColumnID,LinkDataKey from CaseDataLog c1,CaseSeizure " +
                            "where c1.CaseId=@CaseId and TabName='撤銷設定' and TableName='CaseSeizure' and LinkDataKey=cast(SeizureId as nvarchar(100) ) " +
                            "and TXDateTime=(select max(TXDateTime) from CaseDataLog c2 where c1.CaseId=c2.CaseId and c1.TabName=c2.TabName and c1.TableName=c2.TableName and c1.LinkDataKey=c2.LinkDataKey))," +
                            "T3 as (select T1.*, " +
                            "(select case  when ColumnValueBefore<>CancelAmount then 'true' else 'false' end from T2 where ColumnID='CancelAmount' and LinkDataKey=cast(SeizureId as nvarchar(100) )) as CancelAmountflag from T1)" +
                            "select *,1 as IsFromZJ from T3  order by GovDate,GovUnit,Account";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("@CaseId", caseId));
            Parameter.Add(new CommandParameter("@SeizureStatus", SeizureStatus.AfterCancel));
            return trans == null ? SearchList<CaseSeizure>(strSql) : SearchList<CaseSeizure>(strSql, trans);
        }
        /// <summary>
        /// 查詢
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public IList<CaseSeizure> GetCaseSeizureByQuery(CancelSeizureViewModel model)
        {
            string strSql = @"SELECT [SeizureId]
                                  ,[CaseId]
                                  ,[PayCaseId]
                                  ,[CaseNo]
                                  ,[CustId]
                                  ,[CustName]
                                  ,[BranchNo]
                                  ,[BranchName]
                                  ,[Account]
                                  ,[AccountStatus]
                                  ,[Currency]
                                  ,[Balance]
                                  ,[SeizureAmount]
                                  ,[ExchangeRate]
                                  ,[SeizureAmountNtd]
                                  ,[PayAmount]
                                  ,[SeizureStatus]
                                  ,[CreatedUser]
                                  ,[CreatedDate]
                                  ,[ModifiedUser]
                                  ,[ModifiedDate]
                                  ,[CancelCaseId]
                              FROM [CaseSeizure]
                              WHERE [SeizureStatus] = @SeizureStatus
                            AND (ISNULL(@CustId,'') = '' OR [CustId] = @CustId)
                            AND (ISNULL(@CustName,'') = '' OR [CustName] = @CustName)
                            AND (ISNULL(@Account,'') = '' OR [Account] = @Account)
                            AND (ISNULL(@GovUnit,'') = '' OR [CaseId] IN (SELECT CaseId FROM CaseMaster WHERE GovUnit = @GovUnit AND isDelete = 0))";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("@SeizureStatus", SeizureStatus.AfterSeizureSetting));
            Parameter.Add(new CommandParameter("@CustId", model.CustId));
            Parameter.Add(new CommandParameter("@CustName", model.CustName));
            Parameter.Add(new CommandParameter("@Account", model.Account));
            Parameter.Add(new CommandParameter("@GovUnit", model.GoveUnit));
            return SearchList<CaseSeizure>(strSql);

        }


        //public bool updateCaseSeizurePayAmount(int SeizureId, Guid payCaseId, decimal _PayAmount)
        //{
        //    IDbTransaction trans = null;
        //    string strSql = @"UPDATE [CaseSeizure]
        //                       SET [PayAmount] = @PayAmount,
        //                                [PayCaseId] = @PayCaseId
        //                     WHERE  [SeizureId] = @SeizureId";
        //    base.Parameter.Clear();
        //    base.Parameter.Add(new CommandParameter("@PayAmount", _PayAmount));
        //    base.Parameter.Add(new CommandParameter("@PayCaseId", payCaseId));
        //    base.Parameter.Add(new CommandParameter("@SeizureId", SeizureId));
        //    return trans == null ? ExecuteNonQuery(strSql) > 0 : ExecuteNonQuery(strSql, trans) > 0;
        //}

        public bool updateCaseSeizurePayAmount(int SeizureId, Guid payCaseId, decimal _PayAmount, string SeizureStatus = "0")
        {
            IDbTransaction trans = null;
            string strSql = @"UPDATE [CaseSeizure]
                               SET [PayAmount] = @PayAmount,
                                        [PayCaseId] = @PayCaseId
                             WHERE  [SeizureId] = @SeizureId";

            base.Parameter.Clear();
            if (SeizureStatus == "1" || SeizureStatus == "3")
            {
                strSql = @"UPDATE [CaseSeizure]
                               SET [PayAmount] = @PayAmount, [SeizureStatus] = @SeizureStatus,
                                        [PayCaseId] = @PayCaseId
                             WHERE  [SeizureId] = @SeizureId";
                base.Parameter.Add(new CommandParameter("@SeizureStatus", SeizureStatus));
            }

            base.Parameter.Add(new CommandParameter("@PayAmount", _PayAmount));
            base.Parameter.Add(new CommandParameter("@PayCaseId", payCaseId));
            base.Parameter.Add(new CommandParameter("@SeizureId", SeizureId));

            return trans == null ? ExecuteNonQuery(strSql) > 0 : ExecuteNonQuery(strSql, trans) > 0;
        }
        /// <summary>
        /// 儲存扣押撤回
        /// </summary>
        /// <param name="caseId"></param>
        /// <param name="seizureIdlist"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public bool SaveCaseSeizureCancel(Guid caseId, List<CaseSeizure> saveList)
        {
            IDbConnection dbConnection = OpenConnection();
            IDbTransaction dbTransaction = null;
            bool rtn = true;
            try
            {
                dbTransaction = dbConnection.BeginTransaction();
                IList<CaseSeizure> listOld = GetCaseSeizureWithCancel(caseId, dbTransaction);
                if (saveList != null && saveList.Any())
                {
                    foreach (CaseSeizure formItem in saveList)
                    {
                        CaseSeizure cs = GetCaseSeizure(formItem.SeizureId.ToString(), dbTransaction);
                        rtn = rtn && UpdateCaseSeizureToCancel(formItem, dbTransaction);
                        if (listOld.Count > 0)
                        {
                            #region 已撤銷金額
                            if (cs.CancelAmount != formItem.CancelAmount)
                            {
                                CaseDataLog log = new CaseDataLog();
                                Guid TXSNO = Guid.NewGuid();
                                DateTime TXDateTime = DateTime.Parse(System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                                LdapEmployeeBiz empBiz = new LdapEmployeeBiz();
                                log.TXType = "修改";
                                log.TXUser = empBiz.Account != null && !string.IsNullOrEmpty(empBiz.Account) ? empBiz.Account : " ";
                                log.TXUserName = empBiz.AccountName != null && !string.IsNullOrEmpty(empBiz.AccountName) ? empBiz.AccountName : " ";
                                if (PageFrom == "1")
                                {
                                    log.md_FuncID = "Menu.CollectionToAgent";
                                    log.TITLE = "收發作業-收發代辦";
                                }
                                else if (PageFrom == "2")
                                {
                                    log.md_FuncID = "Menu.CollectionToAgent";
                                    log.TITLE = "經辦作業-待辦理";
                                }

                                log.TXSNO = TXSNO;
                                log.TXDateTime = TXDateTime;
                                log.ColumnID = "CancelAmount";
                                log.ColumnName = "已撤銷金額";
                                log.ColumnValueBefore = cs.CancelAmount.ToString();
                                log.ColumnValueAfter = formItem.CancelAmount.ToString();
                                log.TabID = "Tab2-1";
                                log.TabName = "撤銷設定";
                                log.TableName = "CaseSeizure";
                                log.DispSrNo = 15;
                                log.TableDispActive = "1";
                                log.CaseId = caseId.ToString();
                                log.LinkDataKey = formItem.SeizureId.ToString();
                                InsertCaseDataLog(log);
                            }
                            #endregion
                        }
                    }
                }
                if (listOld != null && listOld.Any())
                {
                    foreach (CaseSeizure oldItem in listOld.Where(a => saveList == null || !saveList.Exists(b => b.SeizureId == a.SeizureId)))
                    {
                        rtn = rtn && ClearPaySetting(oldItem, dbTransaction);
                    }
                }

                if (rtn)
                    dbTransaction.Commit();
                else
                    dbTransaction.Rollback();
                return rtn;

                //if (seizureIdlist == null || !seizureIdlist.Any())
                //{
                //    return true;
                //}
                //using (dbConnection)
                //{
                //    dbTrans = dbConnection.BeginTransaction();
                //    foreach (string strId in seizureIdlist)
                //    {
                //        bFlag = bFlag && UpdateCaseSeizureToCancel(caseId, strId, userId, dbTrans);
                //    }
                //    dbTrans.Commit();
                //}
            }
            catch (Exception ex)
            {
                try
                {
                    if (dbTransaction != null)
                        dbTransaction.Rollback();
                }
                catch (Exception ex2)
                {

                }
                return false;
            }
        }

        public bool UpdateCaseSeizureToCancel(CaseSeizure model, IDbTransaction trans = null)
        {
            string strSql = @"UPDATE [CaseSeizure]
                               SET [SeizureStatus] = @SeizureStatus
                                  ,[TxtStatus] = @TxtStatus
                                  ,[CancelAmount] = @CancelAmount
                                  ,[ModifiedUser] = @ModifiedUser
                                  ,[ModifiedDate] = GETDATE()
                                  ,[CancelCaseId] = @CancelCaseId
                             WHERE  [SeizureId] = @SeizureId";
            base.Parameter.Clear();
            base.Parameter.Add(new CommandParameter("@CancelAmount", model.CancelAmount));
            base.Parameter.Add(new CommandParameter("@TxtStatus", model.TxtStatus));
            base.Parameter.Add(new CommandParameter("@SeizureStatus", SeizureStatus.AfterCancel));
            base.Parameter.Add(new CommandParameter("@ModifiedUser", model.ModifiedUser));
            base.Parameter.Add(new CommandParameter("@CancelCaseId", model.PayCaseId));
            base.Parameter.Add(new CommandParameter("@SeizureId", model.SeizureId));
            return trans == null ? ExecuteNonQuery(strSql) > 0 : ExecuteNonQuery(strSql, trans) > 0;
        }

        #endregion

        public int CreateCase(List<CaseAccountExternal> listResult, string memo)
        {
            IDbConnection dbConnection = OpenConnection();
            IDbTransaction dbTransaction = null;
            try
            {
                using (dbConnection)
                {
                    dbTransaction = dbConnection.BeginTransaction();
                    //DeleteCAEnalByCaseID
                    CaseAccountBiz cab = new CaseAccountBiz();
                    cab.DeleteCAENalByCaseID(listResult[0].CaseId, dbTransaction);
                    int a = 0;
                    foreach (CaseAccountExternal item in listResult)
                    {

                        if (string.IsNullOrEmpty(item.Description))
                        {
                            continue;
                        }
                        else
                        {
                            a++;
                            cab.InsertCAEnal(item, a, dbTransaction);
                        }
                    }
                    CaseMemo memoObj = new CaseMemo { CaseId = listResult[0].CaseId, MemoType = CaseMemoType.CaseExternalMemo, Memo = memo };
                    new CaseMemoBiz().Delete(memoObj, dbTransaction);
                    new CaseMemoBiz().SaveMemo(memoObj, dbTransaction);
                    dbTransaction.Commit();
                }
                return 1;
            }
            catch (Exception)
            {
                try
                {
                    if (dbTransaction != null) dbTransaction.Rollback();
                }
                catch (Exception)
                {
                    // ignored
                }
                return 0;
            }
        }
        public int DeleteCAENalByCaseID(Guid caseId, IDbTransaction trans = null)
        {
            try
            {
                string strSql = @"DELETE [CaseAccountExternal] WHERE [CaseId]=@CaseId;";
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@CaseId", caseId));
                if (trans != null)
                {
                    return base.ExecuteNonQuery(strSql, trans);
                }
                else
                {
                    IDbConnection dbConnection = base.OpenConnection();
                    IDbTransaction tans = dbConnection.BeginTransaction();
                    return base.ExecuteNonQuery(strSql, tans);
                }

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public int InsertCAEnal(CaseAccountExternal listResult, int a, IDbTransaction trans = null)
        {
            string strSql = string.Empty;
            strSql = @"INSERT INTO CaseAccountExternal(CaseId,FirstCate,SecondCate,Description,UnitPrice,SortOrder,Quantity,Amount) values(@CaseId,@FirstCate,@SecondCate,@Description,@UnitPrice," + a + ",@Quantity,@Amount);";
            base.Parameter.Clear();
            base.Parameter.Add(new CommandParameter("@CaseId", listResult.CaseId));
            base.Parameter.Add(new CommandParameter("@FirstCate", listResult.FirstCate));
            base.Parameter.Add(new CommandParameter("@SecondCate", listResult.SecondCate));
            base.Parameter.Add(new CommandParameter("@Description", listResult.Description));
            base.Parameter.Add(new CommandParameter("@UnitPrice", listResult.UnitPrice));
            base.Parameter.Add(new CommandParameter("@Quantity", listResult.Quantity));
            base.Parameter.Add(new CommandParameter("@Amount", listResult.Amount));
            try
            {
                if (trans != null)
                {
                    return base.ExecuteNonQuery(strSql, trans);
                }
                else
                {
                    IDbConnection dbConnection = base.OpenConnection();
                    IDbTransaction tans = dbConnection.BeginTransaction();
                    return base.ExecuteNonQuery(strSql, tans);
                }
            }
            catch (Exception ex)
            {
                // 拋出異常
                throw ex;
            }

        }


        public IList<CaseAccountExternal> GetDataFromCAEnal(Guid caseId)
        {
            string strSql = @"SELECT [CaseId],[FirstCate],[SecondCate],[Description],[UnitPrice],[SortOrder],[Quantity],[Amount] FROM [CaseAccountExternal] WHERE [CaseId]=@CaseId ORDER BY [SortOrder] ASC";
            try
            {
                Parameter.Clear();
                Parameter.Add(new CommandParameter("@CaseId", caseId));
                IList<CaseAccountExternal> list = base.SearchList<CaseAccountExternal>(strSql);
                return list;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public IList<CaseAccountExternal> GetDataFromCAESetting()
        {
            string strSql = @"SELECT [FirstCate],[SecondCate],[Description],[UnitPrice],[SortOrder] FROM [CaseAccountExternalSetting] ORDER BY [SortOrder] ASC";
            try
            {
                Parameter.Clear();
                IList<CaseAccountExternal> ilst = base.SearchList<CaseAccountExternal>(strSql);
                return ilst;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public DataTable GetCaseAccountExternalByCaseIdList(List<string> caseIdList)
        {
            string strsql = @"SELECT [CaseId]
                                    ,[FirstCate]
                                    ,[SecondCate]
                                    ,[Description]
                                    ,[UnitPrice]
                                    ,[SortOrder]
                                    ,[Quantity]
                                    ,[Amount]
                                FROM [CaseAccountExternal] AS M
                                WHERE 1=2 ";
            Parameter.Clear();
            for (int i = 0; i < caseIdList.Count; i++)
            {
                strsql = strsql + " OR M.[CaseId] = @CaseId" + i + " ";
                Parameter.Add(new CommandParameter("@CaseId" + i, caseIdList[i]));
            }
            DataTable dt = Search(strsql);
            return dt.Columns.Count > 0 ? dt : new DataTable();
        }

        public DataTable GetCaseReceiptByCaseIdList(List<string> caseIdList)
        {
            /* adam 20151108
            string strsql = @"SELECT 
	                            CaseId,
	                            DocNo,
	                            CaseNo,
	                            GovNo,
								CASE WHEN CaseKind = '外來文案件' THEN GovUnit ELSE (SELECT TOP 1 ReceivePerson FROM [CasePayeeSetting] AS CPS WHERE CPS.CaseId =M.CaseId) END GovUnit,
	                            CaseKind,(select Memo from CaseMemo cm where cm.CaseId=M.CaseId and MemoType='CaseExternal') as Memo,
	                            CASE WHEN CaseKind = '外來文案件' THEN 
	                            (SELECT ISNULL(SUM(Amount),0) FROM [CaseAccountExternal] AS CAE WHERE CAE.CaseId = M.CaseId)
	                            ELSE 
	                            (SELECT ISNULL(SUM(Fee),0) FROM [CasePayeeSetting] AS CPS WHERE CPS.CaseId =M.CaseId)
	                             END Fee,
                                
                            FROM CaseMaster AS M
                            WHERE CASE WHEN CaseKind = '外來文案件' THEN 
	                        (SELECT ISNULL(SUM(Amount),0) FROM [CaseAccountExternal] AS CAE WHERE CAE.CaseId = M.CaseId)
	                        ELSE 
	                        (SELECT ISNULL(SUM(Fee),0) FROM [CasePayeeSetting] AS CPS WHERE CPS.CaseId =M.CaseId)
	                        END  >0 and ( 1=2 ";
             * */
            string strsql = @"SELECT 
	                            M.CaseId,
	                            DocNo,
	                            CaseNo,
	                            GovNo,								
								CASE 
								 WHEN M.CaseKind = '外來文案件' THEN GovUnit
								 ELSE C.ReceivePerson 
								  END GovUnit,
	                            M.CaseKind,(select Memo from CaseMemo cm where cm.CaseId=M.CaseId and MemoType='CaseExternal') as Memo,
	                            CASE 
								WHEN M.CaseKind = '外來文案件' THEN (SELECT ISNULL(SUM(Amount),0) FROM [CaseAccountExternal] AS CAE WHERE CAE.CaseId = M.CaseId)
								ELSE 
	                            ISNULL(Fee,0)
	                             END Fee,
                                --isNUll( Convert(NvarChar(10),S.SendDate,111),'') as SendDate
                                ISNULL((SELECT TOP 1 Convert(NvarChar(10),[SendDate],111)  FROM [CaseSendSetting] AS CSS WHERE [CaseId] =M.CaseId ORDER BY [Template] ASC),'') AS SendDate
                            FROM CaseMaster AS M
							left join [CasePayeeSetting] C on M.CaseId = C.CaseId and C.fee > 0  
							left join  [CaseSendSetting] S on M.CaseId = S.CaseId and S.Template = '支付' and (M.CaseKind2 = '支付' or M.CaseKind2 = '扣押並支付') 
                            WHERE
							CASE 
							WHEN M.CaseKind = '外來文案件' THEN (SELECT ISNULL(SUM(Amount),0) FROM [CaseAccountExternal] AS CAE WHERE CAE.CaseId = M.CaseId)
							WHEN M.CaseKind = '支付' THEN (SELECT ISNULL(SUM(Fee),0) FROM [CasePayeeSetting] AS CPS WHERE CPS.CaseId =M.CaseId )
	                        ELSE 
	                        (SELECT ISNULL(SUM(Fee),0) FROM [CasePayeeSetting] AS CPS WHERE CPS.CaseId =M.CaseId)
	                        END  >0 and ( 1=2 ";
                            // OR M.[CaseId] = '7c6587f4-b21d-4f1d-9b52-c0d66070a53b' )
							// group by M.CaseID,DocNo,CaseNo,GovNo,M.GovUnit,C.ReceivePerson,M.CaseKind,Memo,Fee,SendDate";
            Parameter.Clear();
            for (int i = 0; i < caseIdList.Count; i++)
            {
                strsql = strsql + " OR M.[CaseId] = @CaseId" + i + " ";
                Parameter.Add(new CommandParameter("@CaseId" + i, caseIdList[i]));
            }
            strsql = strsql + ") group by M.CaseID,DocNo,CaseNo,GovNo,M.GovUnit,C.ReceivePerson,M.CaseKind,Memo,Fee,SendDate ";
            //strsql = strsql + ")";
            DataTable dt = Search(strsql);
            return dt.Columns.Count > 0 ? dt : new DataTable();
        }

        /// <summary>
        /// 20201007, 因為輸出收據的數量是跟著手續費, 若手續費是250, 則1張, 500是2張.. 
        /// 所以不能以SendDate來Group by , 把原本的程式, 拿掉group by SendDate...
        /// </summary>
        /// <param name="caseIdList"></param>
        /// <returns></returns>
        public DataTable GetCaseReceiptByCaseIdList_2(List<string> caseIdList)
        {
            string strsql = @"SELECT 
	                            M.CaseId,
	                            DocNo,
	                            CaseNo,
	                            GovNo,								
								CASE 
								 WHEN M.CaseKind = '外來文案件' THEN GovUnit
								 ELSE C.ReceivePerson 
								  END GovUnit,
	                            M.CaseKind,(select Memo from CaseMemo cm where cm.CaseId=M.CaseId and MemoType='CaseExternal') as Memo,
	                            CASE 
								WHEN M.CaseKind = '外來文案件' THEN (SELECT ISNULL(SUM(Amount),0) FROM [CaseAccountExternal] AS CAE WHERE CAE.CaseId = M.CaseId)
								ELSE 
	                            ISNULL(Fee,0)
	                             END Fee,
                                --isNUll( Convert(NvarChar(10),S.SendDate,111),'') as SendDate
                                ISNULL((SELECT TOP 1 Convert(NvarChar(10),[SendDate],111)  FROM [CaseSendSetting] AS CSS WHERE [CaseId] =M.CaseId ORDER BY [Template] ASC),'') AS SendDate
                            FROM CaseMaster AS M
							left join [CasePayeeSetting] C on M.CaseId = C.CaseId and C.fee > 0  
							left join  [CaseSendSetting] S on M.CaseId = S.CaseId and S.Template = '支付' and (M.CaseKind2 = '支付' or M.CaseKind2 = '扣押並支付') 
                            WHERE
							CASE 
							WHEN M.CaseKind = '外來文案件' THEN (SELECT ISNULL(SUM(Amount),0) FROM [CaseAccountExternal] AS CAE WHERE CAE.CaseId = M.CaseId)
							WHEN M.CaseKind = '支付' THEN (SELECT ISNULL(SUM(Fee),0) FROM [CasePayeeSetting] AS CPS WHERE CPS.CaseId =M.CaseId )
	                        ELSE 
	                        (SELECT ISNULL(SUM(Fee),0) FROM [CasePayeeSetting] AS CPS WHERE CPS.CaseId =M.CaseId)
	                        END  >0 and ( 1=2 ";
            Parameter.Clear();
            for (int i = 0; i < caseIdList.Count; i++)
            {
                strsql = strsql + " OR M.[CaseId] = @CaseId" + i + " ";
                Parameter.Add(new CommandParameter("@CaseId" + i, caseIdList[i]));
            }
            strsql = strsql + ") group by M.CaseID,DocNo,CaseNo,GovNo,M.GovUnit,C.ReceivePerson,M.CaseKind,Memo,Fee";
            //strsql = strsql + ")";
            DataTable dt = Search(strsql);
            return dt.Columns.Count > 0 ? dt : new DataTable();
        }

        public string CaseNo(string caseid)
        {
            try
            {
                string StrSql = @"select CaseNo from CaseMaster where CaseId=@CaseId";
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@CaseId", caseid));
                return base.ExecuteScalar(StrSql) == null ? "" : base.ExecuteScalar(StrSql).ToString();
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public IList<CaseObligor> CheckCaseSeizure(Guid caseId)
        {
            string strSql = @"select ObligorNo,ObligorName from CaseObligor where CaseId=@CaseId and ObligorNo not in (select CustId from CaseSeizure where CaseId=@CaseId)";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("@CaseId", caseId));
            return SearchList<CaseObligor>(strSql);
        }

        #region 發查電文
        /// <summary>
        /// 扣押電文發查.
        /// </summary>
        /// <returns></returns>
        public string SeizureTelegram(CaseSeizure item, string memo, Guid CaseId, string CaseKind2, string BreakDay, string kind, User LogonUser)
        {
            bool rtn = false;
            string result = "發查電文后,更新表失敗";
            string newBreakday="";
            if (BreakDay.Length > 0)
            {
                DateTime dt = DateTime.Parse(BreakDay);
                newBreakday = dt.ToString("ddMMyyyy");
            }
            ExecuteHTG objHTG = new ExecuteHTG(LogonUser.Account, LogonUser.LDAPPwd, LogonUser.RCAFAccount, LogonUser.RCAFPs, LogonUser.RCAFBranch);
            //CustomerInfoBIZ CIBZ = new CustomerInfoBIZ();
            List<string> Cur = new List<string>() { "TWD", "JPY", "DEN", "FRF", "HUF", "IDR", "ITL", "KRW" };//這幾種幣別的扣押金額要舍去小數
            if (Cur.Contains(item.Currency))
            {
                int amt = (int)(item.SeizureAmount);
                item.SeizureAmount = Convert.ToDecimal(amt);
            }
            string retS = objHTG.Send9091Or9093(item.Account, item.SeizureAmount, item.Currency, "", memo, item.CustId, CaseId.ToString(), "0", newBreakday);
            if (retS.StartsWith("0000"))
            {
                Guid TXSNO = Guid.NewGuid();
                DateTime TXDateTime = DateTime.Parse(System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")).AddMilliseconds(5);
                LdapEmployeeBiz empBiz = new LdapEmployeeBiz();
                LDAPEmployee empNow = empBiz.GetLdapEmployeeByEmpId(UserId);
                CaseDataLog log = new CaseDataLog();
                log.TXUser = empNow != null && !string.IsNullOrEmpty(empNow.EmpId) ? empNow.EmpId : " ";
                log.TXUserName = empNow != null && !string.IsNullOrEmpty(empNow.EmpName) ? empNow.EmpName : " ";
                log.md_FuncID = "Menu.CollectionToAgent";
                log.TITLE = "經辦作業-待辦理";
                log.TXType = "發查扣押電文";
                CaseSeizure csOld = new CaseSeizure();
                if(item.SeizureId > 0)
                {
                    csOld = GetCaseSeizureInfoBySeizureId(item.SeizureId);
                }
                if(kind == "Seizure")
                {
                    #region 扣押要更新的欄位
                    // adam 20181127
                    string strSql = @"select * from TX_09093 where TrnNum = @TrnNum and CaseId = @CaseId ";
                    Parameter.Clear();
                    Parameter.Add(new CommandParameter("@TrnNum", retS.Trim().Substring(5)));//批號
                    Parameter.Add(new CommandParameter("@CaseId", CaseId));
                    CTBC.CSFS.Models.TX_09093 tx09093 = SearchList<CTBC.CSFS.Models.TX_09093>(strSql).FirstOrDefault();
                    if(item.Currency.ToUpper() == "TWD")//幣別為台幣，匯率為1
                    {
                        item.ExchangeRate = 1;
                    }
                    item.TxtStatus = "1";//是否拋查電文標記

                    Decimal Bal = 0;                   
                    string strn = tx09093.RAMOUNT_OC57;
                    if (tx09093.RAMOUNT_OC57.LastIndexOf("+") != -1 || strn.LastIndexOf("-") != -1)
                    {
                        string sign = strn.Substring(strn.Length - 1, 1);
                        if (sign == "+")
                        {
                            Bal = Convert.ToDecimal(strn.Substring(0, strn.Length - 1));
                        }
                        else
                        {
                            Bal = Convert.ToDecimal(strn.Substring(0, strn.Length - 1)) * -1;
                        }

                    }
                    else
                    {
                        Bal = Convert.ToDecimal(strn);
                    }
                    if (Bal > 0)
                    {
                        Bal = Bal / 1000;
                    }
                    if (tx09093.Account != "")
                    { 
                       item.Balance = Bal.ToString();//可用餘額
                    }
                    item.SeizureAmount = item.SeizureAmount;//ETABS扣押金額
                    item.SeizureAmountNtd = item.SeizureAmount * item.ExchangeRate;//台幣
                    #region //不寫備註回文
                    //CaseMemo oldMemoObj = new CaseMemoBiz().Memo(CaseId, CaseMemoType.CaseSeizureMemo);
                    //string oldMemo = oldMemoObj == null ? "" : oldMemoObj.Memo;
                    //string newMemo = "";//備註回文
                    //CTBC.CSFS.Models.TX_00450 CMOther = CIBZ.GetLatestTx00450(item.Account, CaseId, "31", " 9093 "," 66");
                    //if (CMOther != null && CMOther.DATA1.Contains(" 9093 ") && CMOther.DATA2.Trim().Contains(" 66"))
                    //{
                    //    item.OtherSeizure = "Y";//他案扣押
                    //    newMemo = oldMemo + "<br/>" + "帳戶因他案已扣押在案。(請查明)";//他案扣押的備註回文
                    //}
                    //CTBC.CSFS.Models.TX_00450 CM = CIBZ.GetLatestTx00450(item.Account, CaseId, "30", " 9091 "," 04");
                    //if (CM != null)
                    //{
                    //    if (CM.DATA1.Contains(" 9091 ") && CM.DATA2.Trim().Contains(" 04"))
                    //    {
                    //        item.OtherSeizure = "Y";//他案扣押
                    //    }
                    //    //凍結 備註回文
                    //    if (CM.DATA2.Trim().Contains(" 04"))
                    //    {
                    //        newMemo = oldMemo + "<br/>" + "帳戶因他案已凍結在案。(請查明)";
                    //    }
                    //    //定存 備註回文
                    //    if (CM.DATA2.Trim().Contains(" 05") || CM.DATA2.Trim().Contains(" 06"))
                    //    {
                    //        newMemo = oldMemo + "<br/>" + "帳戶因他案已設定質權在案。(請查明)";
                    //    }
                    //    //其它 備註回文
                    //    if (CM.DATA2.Trim().Contains(" 10"))
                    //    {
                    //        newMemo = oldMemo + "<br/>" + "帳戶因其他原因已凍結在案。(請查明)";
                    //    }
                    //    //警示 備註回文
                    //    if (CM.DATA2.Trim().Contains(" 11") || CM.DATA2.Trim().Contains(" 12"))
                    //    {
                    //        newMemo = oldMemo + "<br/>" + "帳戶已設定為警示帳戶。";
                    //    }
                    //    if (CM.DATA2.Trim().Contains(" 13 "))
                    //    {
                    //        newMemo = oldMemo + "<br/>" + "帳戶已設定為衍生警示帳戶。";
                    //    }
                    //    if (CM.DATA2.Trim().Contains(" 14"))
                    //    {
                    //        newMemo = oldMemo + "<br/>" + "帳戶因偵辦刑事案件設定為警示帳戶。";
                    //    }
                    //}
                    //var memoObj = new CaseMemo { CaseId = CaseId, MemoType = CaseMemoType.CaseSeizureMemo, Memo = newMemo };
                    //new CaseMemoBiz().Delete(memoObj, null);
                    //new CaseMemoBiz().SaveMemo(memoObj, null);
                    #endregion
                    if (item.SeizureId > 0)
                    {
                        if (Bal > 0)
                        {
                            rtn = UpdateSeizureBalance(item);
                        }
                        else
                        {
                            rtn = UpdateSeizureSetting(item);
                        }
                        #region 可用餘額
                        log.TXSNO = TXSNO;
                        log.TXDateTime = TXDateTime;
                        log.ColumnID = "Balance";
                        log.ColumnName = "可用餘額";
                        log.ColumnValueBefore = csOld.Balance.ToString();
                        log.ColumnValueAfter = item.Balance.ToString();
                        log.TabID = "Tab2-1";
                        log.TabName = "扣押設定";
                        log.TableName = "CaseSeizure";
                        log.DispSrNo = 11;
                        log.TableDispActive = "1";
                        log.CaseId = CaseId.ToString();
                        log.LinkDataKey = item.SeizureId.ToString();
                        InsertCaseDataLog(log);
                        #endregion

                        #region ETABS扣押金額
                        log.TXSNO = TXSNO;
                        log.TXDateTime = TXDateTime;
                        log.ColumnID = "SeizureAmount";
                        log.ColumnName = "ETABS扣押金額";
                        log.ColumnValueBefore = csOld.SeizureAmount.ToString();
                        log.ColumnValueAfter = item.SeizureAmount.ToString();
                        log.TabID = "Tab2-1";
                        log.TabName = "扣押設定";
                        log.TableName = "CaseSeizure";
                        log.DispSrNo = 12;
                        log.TableDispActive = "1";
                        log.CaseId = CaseId.ToString();
                        log.LinkDataKey = item.SeizureId.ToString();
                        InsertCaseDataLog(log);
                        #endregion

                        #region 台幣
                        log.TXSNO = TXSNO;
                        log.TXDateTime = TXDateTime;
                        log.ColumnID = "SeizureAmountNtd";
                        log.ColumnName = "台幣";
                        log.ColumnValueBefore = csOld.SeizureAmountNtd.ToString();
                        log.ColumnValueAfter = item.SeizureAmountNtd.ToString();
                        log.TabID = "Tab2-1";
                        log.TabName = "扣押設定";
                        log.TableName = "CaseSeizure";
                        log.DispSrNo = 14;
                        log.TableDispActive = "1";
                        log.CaseId = CaseId.ToString();
                        log.LinkDataKey = item.SeizureId.ToString();
                        InsertCaseDataLog(log);
                        #endregion

                        #region 他案扣押
                        log.TXSNO = TXSNO;
                        log.TXDateTime = TXDateTime;
                        log.ColumnID = "OtherSeizure";
                        log.ColumnName = "他案扣押";
                        log.ColumnValueBefore = csOld.OtherSeizure;
                        log.ColumnValueAfter = item.OtherSeizure;
                        log.TabID = "Tab2-1";
                        log.TabName = "扣押設定";
                        log.TableName = "CaseSeizure";
                        log.DispSrNo = 18;
                        log.TableDispActive = "1";
                        log.CaseId = CaseId.ToString();
                        log.LinkDataKey = item.SeizureId.ToString();
                        InsertCaseDataLog(log);
                        #endregion
                    }
                    else
                    {
                        rtn = InsertSeizureSetting(item);
                        Add(CaseId, item, TXSNO, TXDateTime, log.TXType);
                    }
                    CaseMasterBIZ masterBiz = new CaseMasterBIZ();
                    CaseMaster master = masterBiz.MasterModelNew(CaseId);
                    if (CaseKind2 == "扣押並支付" && !string.IsNullOrEmpty(BreakDay))//更新CaseMaster表的支付日期PayDate
                    {
                        string BreakDayFormat = UtlString.FormatDateTwStringToAd(BreakDay);
                        rtn = masterBiz.UpdateCaseMasterPayDate(CaseId, BreakDayFormat, "", null, "CaseSeizureAndPay");
                    }
                    #endregion
                    #region //不寫備註回文
                    //#region Memo
                    //log.TXSNO = TXSNO;
                    //log.TXDateTime = TXDateTime;
                    //log.ColumnID = "Memo";
                    //log.ColumnName = "備註內容";
                    //log.ColumnValueBefore = oldMemo;
                    //log.ColumnValueAfter = memoObj.Memo;
                    //log.TabID = "Tab2-1";
                    //log.TabName = "扣押設定";
                    //log.TableName = "CaseMemo";
                    //log.DispSrNo = 19;
                    //log.TableDispActive = "1";
                    //log.CaseId = CaseId.ToString();
                    //log.CaseNo = master.CaseNo.ToString();
                    //log.LinkDataKey = CaseId.ToString();
                    //InsertCaseDataLog(log);
                    //#endregion

                    //#region MemoDate
                    //log.TXSNO = TXSNO;
                    //log.TXDateTime = TXDateTime;
                    //log.ColumnID = "MemoDate";
                    //log.ColumnName = "備註時間";
                    //log.ColumnValueBefore = oldMemoObj.MemoDate;
                    //log.ColumnValueAfter = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    //log.TabID = "Tab2-1";
                    //log.TabName = "扣押設定";
                    //log.TableName = "CaseMemo";
                    //log.DispSrNo = 20;
                    //log.TableDispActive = "1";
                    //log.CaseId = CaseId.ToString();
                    //log.CaseNo = master.CaseNo.ToString();
                    //log.LinkDataKey = CaseId.ToString();
                    //InsertCaseDataLog(log);
                    //#endregion

                    //#region MemoUser
                    //log.TXSNO = TXSNO;
                    //log.TXDateTime = TXDateTime;
                    //log.ColumnID = "MemoUser";
                    //log.ColumnName = "備註人";
                    //log.ColumnValueBefore = oldMemoObj.MemoUser;
                    //log.ColumnValueAfter = log.TXUserName;
                    //log.TabID = "Tab2-1";
                    //log.TabName = "扣押設定";
                    //log.TableName = "CaseMemo";
                    //log.DispSrNo = 21;
                    //log.TableDispActive = "1";
                    //log.CaseId = CaseId.ToString();
                    //log.CaseNo = master.CaseNo.ToString();
                    //log.LinkDataKey = CaseId.ToString();
                    //InsertCaseDataLog(log);
                    //#endregion
                    #endregion
                }
                else if(kind == "Cancel")//撤銷案件的沖正
                {
                    item.CancelAmount = 0;//已撤銷金額,金額變為0
                    item.TxtStatus = "0";//撤銷案件沖正后，已撤銷金額變為0且反白可輸入 IR-1020
                    //item.TxtStatus = "1";
                    rtn = UpdateCaseSeizureToCancel(item);
                    //rtn = UpdateSeizureStatus(item);//做完沖正電文之後，將SeizureStatus 寫為0
                    log.TXType = "發查沖正電文";
                    #region 已撤銷金額
                    log.TXSNO = TXSNO;
                    log.TXDateTime = TXDateTime;
                    log.ColumnID = "CancelAmount";
                    log.ColumnName = "已撤銷金額";
                    log.ColumnValueBefore = csOld.CancelAmount.ToString();
                    log.ColumnValueAfter = item.CancelAmount.ToString();
                    log.TabID = "Tab2-1";
                    log.TabName = "撤銷設定";
                    log.TableName = "CaseSeizure";
                    log.DispSrNo = 15;
                    log.TableDispActive = "1";
                    log.CaseId = CaseId.ToString();
                    log.LinkDataKey = item.SeizureId.ToString();
                    InsertCaseDataLog(log);
                    #endregion
                }
                if(rtn)
                {
                    result = "true";
                }
            }
            else
            {
                result = retS.Substring(5);
            }
            return result;
        }
        /// <summary>
        /// 沖正電文發查.
        /// </summary>
        /// <returns></returns>
        public string ResetTelegram(CaseSeizure item, string memo, Guid CaseId, string kind, User LogonUser, string BreakDay)
        {
            string newBreakday = "";
            if (BreakDay.Length > 0)
            {
                DateTime dt = DateTime.Parse(BreakDay);
                newBreakday = dt.ToString("ddMMyyyy");
            }
            bool rtn = false;
            string result = "發查電文后,更新表失敗";
            ExecuteHTG objHTG = new ExecuteHTG(LogonUser.Account, LogonUser.LDAPPwd, LogonUser.RCAFAccount, LogonUser.RCAFPs, LogonUser.RCAFBranch);
            List<string> Cur = new List<string>() { "TWD", "JPY", "DEN", "FRF", "HUF", "IDR", "ITL", "KRW" };//這幾種幣別的扣押金額要舍去小數
            if(Cur.Contains(item.Currency))
            {
                int amt = (int)(item.SeizureAmount);
                item.SeizureAmount = Convert.ToDecimal(amt);
            }
            string retS = objHTG.Send9092Or9095(item.Account, item.SeizureAmount, item.Currency, "66", memo, item.CustId, CaseId.ToString(), "0", newBreakday);
            if (retS.StartsWith("0000"))
            {
                Guid TXSNO = Guid.NewGuid();
                DateTime TXDateTime = DateTime.Parse(System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")).AddMilliseconds(5);
                LdapEmployeeBiz empBiz = new LdapEmployeeBiz();
                LDAPEmployee empNow = empBiz.GetLdapEmployeeByEmpId(UserId);
                #region 沖正電文Log記錄
                CaseDataLog log = new CaseDataLog();
                log.TXUser = empNow != null && !string.IsNullOrEmpty(empNow.EmpId) ? empNow.EmpId : " ";
                log.TXUserName = empNow != null && !string.IsNullOrEmpty(empNow.EmpName) ? empNow.EmpName : " ";
                log.md_FuncID = "Menu.CollectionToAgent";
                log.TITLE = "經辦作業-待辦理";
                log.TXType = "發查沖正電文";
                CaseSeizure csOld = new CaseSeizure();
                if (item.SeizureId > 0)
                {
                    csOld = GetCaseSeizureInfoBySeizureId(item.SeizureId);
                }
                if (kind == "Seizure")//扣押案件的沖正
                {
                    item.SeizureAmount = 0;//ETABS扣押金額,金額變為0,人工可鍵新扣押金額
                    item.SeizureAmountNtd = 0;//台幣,金額變為0
                    item.TxtStatus = "0";//沖正后,ETABS扣押金額可輸入
                    if(item.SeizureId > 0)
                    {
                        rtn = UpdateSeizureSetting(item);
                        #region ETABS扣押金額
                        log.TXSNO = TXSNO;
                        log.TXDateTime = TXDateTime;
                        log.ColumnID = "SeizureAmount";
                        log.ColumnName = "ETABS扣押金額";
                        log.ColumnValueBefore = csOld.SeizureAmount.ToString();
                        log.ColumnValueAfter = item.SeizureAmount.ToString();
                        log.TabID = "Tab2-1";
                        log.TabName = "扣押設定";
                        log.TableName = "CaseSeizure";
                        log.DispSrNo = 12;
                        log.TableDispActive = "1";
                        log.CaseId = CaseId.ToString();
                        log.LinkDataKey = item.SeizureId.ToString();
                        InsertCaseDataLog(log);
                        #endregion

                        #region 台幣
                        log.TXSNO = TXSNO;
                        log.TXDateTime = TXDateTime;
                        log.ColumnID = "SeizureAmountNtd";
                        log.ColumnName = "台幣";
                        log.ColumnValueBefore = csOld.SeizureAmountNtd.ToString();
                        log.ColumnValueAfter = item.SeizureAmountNtd.ToString();
                        log.TabID = "Tab2-1";
                        log.TabName = "扣押設定";
                        log.TableName = "CaseSeizure";
                        log.DispSrNo = 14;
                        log.TableDispActive = "1";
                        log.CaseId = CaseId.ToString();
                        log.LinkDataKey = item.SeizureId.ToString();
                        InsertCaseDataLog(log);
                        #endregion
                    }
                    else
                    {
                        rtn = InsertSeizureSetting(item);
                        Add(CaseId, item, TXSNO, TXDateTime, log.TXType);
                    }
                    rtn = UpdateSeizureStatus(item);//做完沖正電文之後，將SeizureStatus 寫為0
                }
                else if (kind == "Cancel")//撤銷案件的撤銷
                {
                    item.CancelAmount = item.SeizureAmount;//已撤銷金額,同扣押金額
                    //item.TxtStatus = "0";//已撤銷金額可輸入
                    item.TxtStatus = "1";//撤銷成功後，左方<已撤銷金額>顯示實際撤銷金額，且反灰不可修改 IR-1020
                    rtn = UpdateCaseSeizureToCancel(item);
                    log.TXType = "發查撤銷電文";
                    #region 已撤銷金額
                    log.TXSNO = TXSNO;
                    log.TXDateTime = TXDateTime;
                    log.ColumnID = "CancelAmount";
                    log.ColumnName = "已撤銷金額";
                    log.ColumnValueBefore = csOld.CancelAmount.ToString();
                    log.ColumnValueAfter = item.CancelAmount.ToString();
                    log.TabID = "Tab2-1";
                    log.TabName = "撤銷設定";
                    log.TableName = "CaseSeizure";
                    log.DispSrNo = 15;
                    log.TableDispActive = "1";
                    log.CaseId = CaseId.ToString();
                    log.LinkDataKey = item.SeizureId.ToString();
                    InsertCaseDataLog(log);
                    #endregion
                }
                #endregion
                if (rtn)
                {
                    result = "true";
                }
            }
            else
            {
                result = retS.Substring(5);
            }
            return result;
        }
        /// <summary>
        /// 事故電文發查.
        /// </summary>
        /// <returns></returns>
        public bool AccidentTelegram(CaseSeizure item, string kind, User LogonUser, Guid CaseId)
        {
            bool rtn = false;
            string retP = "";
            bool? retA = null;
            CustomerInfoBIZ custBiz = new CustomerInfoBIZ();
            ExecuteHTG objHTG = new ExecuteHTG(LogonUser.Account, LogonUser.LDAPPwd, LogonUser.RCAFAccount, LogonUser.RCAFPs, LogonUser.RCAFBranch);
            //bool? retA = objHTG.is405Accident(item.CustId, CaseId.ToString(), item.Account, item.Currency);
            retP = objHTG.Send401(item.CustId, item.CaseId.ToString(), item.Account, item.Currency);
            if (retP.StartsWith("0000"))
            {  
                #region 更新 TX_60491_Detl的StsDesc欄位 SNO+FKSNO+CaseId+Account --> 改為以CaseId+Account+Currency來更新 20181101
                CTBC.CSFS.Models.TX_60491_Detl tx60491Detl = new CTBC.CSFS.Models.TX_60491_Detl();
                //tx60491Detl.SNO = item.SNO;
                //tx60491Detl.FKSNO = item.FKSNO;
                tx60491Detl.Ccy = item.Currency;
                tx60491Detl.CaseId = CaseId;
                tx60491Detl.Account = item.Account;
                
                if (retP.StartsWith("0000"))//IR-8002 先查33401 狀態
                {
                    CTBC.CSFS.Models.TX_33401 tx33401 = custBiz.GetLatestTx33401(item.Account, item.CaseId, item.Currency);//adam 20160427
                    if (tx33401 != null)
                    {
                        if ((tx33401.AcctStatus1.Trim() == "事故") || (tx33401.AcctStatus1.Trim() == "STOP"))
                        {
                            item.AccountStatus = "事故";
                             retA = objHTG.is405Accident(item.CustId, CaseId.ToString(), item.Account, item.Currency);                            
                        }
                        else
                        {
                            item.AccountStatus = "正常";
                        }
                    }
                }
                //events = GetStsDesc(tx60491Detl);
                //if (events == true)
                //{
                //    item.AccountStatus = "事故";
                //}
                //else
                //{
                //    item.AccountStatus = "正常";
                //}

                tx60491Detl.StsDesc = item.AccountStatus;
                rtn = UpdateStsDesc(tx60491Detl);
                #endregion
                #region 事故電文不寫CaseSeizure表
                ////CaseSeizure cs = GetCaseSeizureInfo(item.CaseId, item.SeizureId);
                //CaseSeizure cs = GetCaseSeizureInfoBySeizureId(item.SeizureId);
                rtn = UpdateAccountStatus(item);
                //Guid TXSNO = Guid.NewGuid();
                //DateTime TXDateTime = DateTime.Parse(System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                //LdapEmployeeBiz empBiz = new LdapEmployeeBiz();
                //LDAPEmployee empNow = empBiz.GetLdapEmployeeByEmpId(UserId);
                //CaseDataLog log = new CaseDataLog();
                //log.TXType = "發查事故電文";
                //log.TXUser = empNow != null && !string.IsNullOrEmpty(empNow.EmpId) ? empNow.EmpId : " ";
                //log.TXUserName = empNow != null && !string.IsNullOrEmpty(empNow.EmpName) ? empNow.EmpName : " ";
                //log.md_FuncID = "Menu.CollectionToAgent";
                //log.TITLE = "經辦作業-待辦理";

                //#region 帳號狀態
                //log.TXSNO = TXSNO;
                //log.TXDateTime = TXDateTime;
                //log.ColumnID = "AccountStatus";
                //log.ColumnName = "狀態";
                //log.ColumnValueBefore = cs.AccountStatus.ToString();
                //log.ColumnValueAfter = item.AccountStatus.ToString();
                //log.TabID = "Tab2-1";
                //if (kind == "Seizure")
                //{
                //    log.TabName = "扣押設定";
                //}
                //else if (kind == "Pay")
                //{
                //    log.TabName = "支付設定";
                //}
                //log.TableName = "CaseSeizure";
                //log.DispSrNo = 6;
                //log.TableDispActive = "1";
                //log.CaseId = item.CaseId.ToString();
                //log.LinkDataKey = item.SeizureId.ToString();
                //InsertCaseDataLog(log);
                //#endregion
                #endregion
            }
            return retP.StartsWith("0000") ? true : false;
        }

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
        /// <summary>
        /// 支付電文發查.
        /// </summary>
        /// <returns></returns>
        public string PayTelegram(CaseSeizure item, string memo, Guid CaseId, string BreakDay, string kind, User LogonUser)
        {
            bool rtn = false;
            string result = "發查電文后,更新表失敗";
            ExecuteHTG objHTG = new ExecuteHTG(LogonUser.Account, LogonUser.LDAPPwd, LogonUser.RCAFAccount, LogonUser.RCAFPs, LogonUser.RCAFBranch);
            string retP = "";
            string BreakDayToTelegram = "";//發電文用的解扣日
            string BreakDayFormat = "";//更新支付時間用的日期
            if (!string.IsNullOrEmpty(BreakDay))
            {
                BreakDayFormat = UtlString.FormatDateTwStringToAd(BreakDay);
                BreakDayToTelegram = Convert.ToDateTime(BreakDayFormat).Date.ToString("ddMMyyyy");//解扣日格式日月年13062018
            }
            if (kind == "Pay")//支付
            {
                if (item.PayAmount == item.SeizureAmount)//支付金額=扣押金額
                {
                    retP = objHTG.Send9099(item.Account, item.SeizureAmount, item.Currency, "66", memo, item.CustId, CaseId.ToString(), BreakDayToTelegram);
                }
                else//支付金額≠扣押金額
                {
                    retP = objHTG.Send9092Or9095(item.Account, item.SeizureAmount, item.Currency, "", memo, item.CustId, CaseId.ToString(), "0", "");
                    if (retP.StartsWith("0000"))//IR-0164 上一個電文成功了才能打下一個電文
                    {
                        if (item.PayAmount > 0) //adam20181122 支付0元不發電文
                        {
                            retP = objHTG.Send9091Or9093(item.Account, item.PayAmount, item.Currency, "66", memo, item.CustId, CaseId.ToString(), "0", BreakDayToTelegram);
                        }
                    }
                }
            }
            else if (kind == "Reset")//沖正
            {
                //解扣日 IR-0144
                CaseMasterBIZ master = new CaseMasterBIZ();
                string ResetDay = master.GetPayDate(CaseId);//日期格式日月年12092018
                string oldmemo = string.Empty;//原扣押文號 IR-0144
                if (!string.IsNullOrEmpty(item.GovNo) && item.GovNo.Length >= 7)
                {
                    oldmemo = item.GovNo.Substring(0, 3) + item.GovNo.Substring(item.GovNo.Length - 7, 6);
                }
                else
                {
                    oldmemo = item.GovNo;
                }
                if (item.PayAmount == item.SeizureAmount)//支付金額=扣押金額
                {
                    retP = objHTG.Send9099Reset(item.Account, item.PayAmount, item.Currency, "66", oldmemo, item.CustId, CaseId.ToString(), ResetDay);//IR-0184
                }
                else//支付金額≠扣押金額
                {
                    retP = objHTG.Send9092Or9095(item.Account, item.PayAmount, item.Currency, "66", memo, item.CustId, CaseId.ToString(), "0", ResetDay);//9095(KEY解扣日)
                    if (retP.StartsWith("0000"))//IR-0164 上一個電文成功了才能打下一個電文
                    {
                        if (item.PayAmount > 0) ////adam20181122 支付0元不發電文
                        {
                            retP = objHTG.Send9091Or9093(item.Account, item.SeizureAmount, item.Currency, "66", oldmemo, item.CustId, CaseId.ToString(), "0", "");//原扣押文號
                        }
                    }
                }
            }
            if (retP.StartsWith("0000"))
            {
                //CaseSeizure csOld = GetCaseSeizureInfo(CaseId, item.SeizureId);
                CaseSeizure csOld = GetCaseSeizureInfoBySeizureId(item.SeizureId);
                if (kind == "Pay")
                {
                    if (item.PayAmount == item.SeizureAmount)//支付金額=扣押金額
                    {
                        item.TripAmount = item.SeizureAmount;//解扣金額,同扣押金額
                    }
                    else//支付金額≠扣押金額
                    {
                        item.TripAmount = item.PayAmount;//解扣金額,同支付金額
                    }
                    CaseMasterBIZ masterBiz = new CaseMasterBIZ();
                    rtn = masterBiz.UpdateCaseMasterPayDate(CaseId, BreakDayFormat, "", null, "Pay");//更新CaseMaster表的支付日期PayDate
                }
                else if (kind == "Reset")
                {
                    item.TripAmount = 0;//解扣金額,金額變為0
                    item.PayAmount = 0;//支付金額,金額變為0
                    item.SeizureStatus = SeizureStatus.AfterPayCancel;//
                }
                rtn = UpdatePaySetting(item);
                //
                if (kind == "Reset")
                {
                    item.SeizureStatus = SeizureStatus.AfterPayCancel;//
                    //上一個update更新主數據，這個更新SeizureStatus,順序不能反
                    rtn = UpdateSeizureStatus(item);//做完沖正電文之後，將SeizureStatus 寫為3
                }
                Guid TXSNO = Guid.NewGuid();
                DateTime TXDateTime = DateTime.Parse(System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")).AddMilliseconds(5);
                LdapEmployeeBiz empBiz = new LdapEmployeeBiz();
                LDAPEmployee empNow = empBiz.GetLdapEmployeeByEmpId(UserId);
                #region 支付電文Log記錄
                CaseDataLog log = new CaseDataLog();
                log.TXType = "發查支付電文";
                log.TXUser = empNow != null && !string.IsNullOrEmpty(empNow.EmpId) ? empNow.EmpId : " ";
                log.TXUserName = empNow != null && !string.IsNullOrEmpty(empNow.EmpName) ? empNow.EmpName : " ";
                log.md_FuncID = "Menu.CollectionToAgent";
                log.TITLE = "經辦作業-待辦理";

                #region 支付金額
                log.TXSNO = TXSNO;
                log.TXDateTime = TXDateTime;
                log.ColumnID = "PayAmount";
                log.ColumnName = "支付金額";
                log.ColumnValueBefore = csOld.PayAmount.ToString();
                log.ColumnValueAfter = item.PayAmount.ToString();
                log.TabID = "Tab2-1";
                log.TabName = "支付設定";
                log.TableName = "CaseSeizure";
                log.DispSrNo = 17;
                log.TableDispActive = "1";
                log.CaseId = CaseId.ToString();
                log.LinkDataKey = item.SeizureId.ToString();
                InsertCaseDataLog(log);
                #endregion

                #region 解扣金額
                log.TXSNO = TXSNO;
                log.TXDateTime = TXDateTime;
                log.ColumnID = "TripAmount";
                log.ColumnName = "解扣金額";
                log.ColumnValueBefore = csOld.TripAmount.ToString();
                log.ColumnValueAfter = item.TripAmount.ToString();
                log.TabID = "Tab2-1";
                log.TabName = "支付設定";
                log.TableName = "CaseSeizure";
                log.DispSrNo = 18;
                log.TableDispActive = "1";
                log.CaseId = CaseId.ToString();
                log.LinkDataKey = item.SeizureId.ToString();
                InsertCaseDataLog(log);
                #endregion
                #endregion
                if (rtn)
                {
                    result = "true";
                }
            }
            else
            {
                result = retP.Substring(5);
            }
            return result;
        }
        //public string PayTelegram(CaseSeizure item, string memo, Guid CaseId, string BreakDay, string kind, User LogonUser)
        //{
        //    bool rtn = false;
        //    string result = "發查電文后,更新表失敗";
        //    ExecuteHTG objHTG = new ExecuteHTG(LogonUser.Account, LogonUser.LDAPPwd, LogonUser.RCAFAccount, LogonUser.RCAFPs, LogonUser.RCAFBranch);
        //    string retP = "";
        //    string BreakDayToTelegram = "";//發電文用的解扣日
        //    string BreakDayFormat ="";//更新支付時間用的日期
        //    if(!string.IsNullOrEmpty(BreakDay))
        //    {
        //        BreakDayFormat = UtlString.FormatDateTwStringToAd(BreakDay);
        //        BreakDayToTelegram = Convert.ToDateTime(BreakDayFormat).Date.ToString("ddMMyyyy");//解扣日格式日月年13062018
        //    }
        //    if (kind == "Pay")//支付
        //    {
        //        //if (item.PayAmount == item.SeizureAmount)//支付金額=扣押金額
        //        //{
        //        //    retP = objHTG.Send9099(item.Account, item.SeizureAmount, item.Currency, "66", memo, item.CustId, CaseId.ToString(), BreakDayToTelegram);
        //        //}
        //        //else//支付金額≠扣押金額
        //        //{
        //        //    retP = objHTG.Send9092Or9095(item.Account, item.SeizureAmount, item.Currency, "", memo, item.CustId, CaseId.ToString(), "0", "");
        //        //    if (retP.StartsWith("0000"))//IR-0164 上一個電文成功了才能打下一個電文
        //        //    {
        //        //        if (item.PayAmount > 0) //adam20181122 支付0元不發電文
        //        //        {
        //        //            retP = objHTG.Send9091Or9093(item.Account, item.PayAmount, item.Currency, "66", memo, item.CustId, CaseId.ToString(), "0", BreakDayToTelegram);
        //        //        }
        //        //    }
        //        //}
        //        //Adam 20200928
        //        CaseMaster sei = new CaseMasterBIZ().MasterModelNew(item.CaseId, null);
        //        string seiMemo = getNewMemo(sei.GovUnit, sei.GovNo);


        //        retP = objHTG.Send9095(item.Account, item.SeizureAmount, item.Currency, "66", seiMemo, item.CustId, item.CaseId.ToString(), ""); //-- 解扣

        //        //retP = objHTG.Send9092Or9095(item.Account, item.SeizureAmount, item.Currency, "", memo, item.CustId, CaseId.ToString(), "0", "");
        //        if (retP.StartsWith("0000"))//IR-0164 上一個電文成功了才能打下一個電文
        //        {
        //            if (item.PayAmount > 0) //adam20181122 支付0元不發電文
        //            {

        //                retP = objHTG.Send9091Or9093(item.Account, item.PayAmount, item.Currency, "66", memo, item.CustId, CaseId.ToString(), "0", BreakDayToTelegram);
        //                if (retP.StartsWith("0000"))
        //                {
        //                }
        //                else
        //                {
        //                    retP = objHTG.Send9091Or9093(item.Account, item.SeizureAmount, item.Currency, "66", seiMemo, item.CustId, item.CaseId.ToString(), "0", "");
        //                }
        //            }
        //        }

        //    }
        //    else if (kind == "Reset")//沖正
        //    {
        //        //解扣日 IR-0144
        //        CaseMasterBIZ master = new CaseMasterBIZ();
        //        string ResetDay = master.GetPayDate(CaseId);//日期格式日月年12092018
        //        string oldmemo = string.Empty;//原扣押文號 IR-0144
        //        //if (!string.IsNullOrEmpty(item.GovNo) && item.GovNo.Length >= 7)
        //        //{
        //        //    oldmemo = item.GovNo.Substring(0, 3) + item.GovNo.Substring(item.GovNo.Length - 7, 6);
        //        //}
        //        //else
        //        //{
        //        //    oldmemo = item.GovNo;
        //        //}


        //        retP = objHTG.Send9095(item.Account, item.SeizureAmount, item.Currency, "66", memo, item.CustId, item.CaseId.ToString(), ResetDay); //-- 解扣
        //        if (retP.StartsWith("0000"))//IR-0164 上一個電文成功了才能打下一個電文
        //        {
        //            CaseMaster sei = new CaseMasterBIZ().MasterModelNew(item.CaseId, null);
        //            string seiMemo = getNewMemo(sei.GovUnit, sei.GovNo);
        //            retP = objHTG.Send9091Or9093(item.Account, item.SeizureAmount, item.Currency, "66", seiMemo, item.CustId, CaseId.ToString(), "0", "");//原扣押文號
        //        }


        //        //if (item.PayAmount == item.SeizureAmount)//支付金額=扣押金額
        //        //{
        //        //    retP = objHTG.Send9099Reset(item.Account, item.PayAmount, item.Currency, "66", oldmemo, item.CustId, CaseId.ToString(), ResetDay);//IR-0184
        //        //}
        //        //else//支付金額≠扣押金額
        //        //{
        //        //    retP = objHTG.Send9092Or9095(item.Account, item.PayAmount, item.Currency, "66", memo, item.CustId, CaseId.ToString(), "0", ResetDay);//9095(KEY解扣日)
        //        //    if (retP.StartsWith("0000"))//IR-0164 上一個電文成功了才能打下一個電文
        //        //    {
        //        //        if (item.PayAmount > 0) ////adam20181122 支付0元不發電文
        //        //        {
        //        //            retP = objHTG.Send9091Or9093(item.Account, item.SeizureAmount, item.Currency, "66", oldmemo, item.CustId, CaseId.ToString(), "0", "");//原扣押文號
        //        //        }
        //        //    }
        //        //}
        //    }
        //    if (retP.StartsWith("0000"))
        //    {
        //        //CaseSeizure csOld = GetCaseSeizureInfo(CaseId, item.SeizureId);
        //        CaseSeizure csOld = GetCaseSeizureInfoBySeizureId(item.SeizureId);
        //        if (kind == "Pay")
        //        {
        //            if (item.PayAmount == item.SeizureAmount)//支付金額=扣押金額
        //            {
        //                item.TripAmount = item.SeizureAmount;//解扣金額,同扣押金額
        //            }
        //            else//支付金額≠扣押金額
        //            {
        //                item.TripAmount = item.PayAmount;//解扣金額,同支付金額
        //            }
        //            CaseMasterBIZ masterBiz = new CaseMasterBIZ();
        //            rtn = masterBiz.UpdateCaseMasterPayDate(CaseId, BreakDayFormat, "", null, "Pay");//更新CaseMaster表的支付日期PayDate
        //        }
        //        else if (kind == "Reset")
        //        {
        //            item.TripAmount = 0;//解扣金額,金額變為0
        //            item.PayAmount = 0;//支付金額,金額變為0
        //            item.SeizureStatus = SeizureStatus.AfterPayCancel;//
        //        }
        //        rtn = UpdatePaySetting(item);
        //        //
        //        if (kind == "Reset")
        //        {
        //            item.SeizureStatus = SeizureStatus.AfterPayCancel;//
        //            //上一個update更新主數據，這個更新SeizureStatus,順序不能反
        //            rtn = UpdateSeizureStatus(item);//做完沖正電文之後，將SeizureStatus 寫為3
        //        }
        //        Guid TXSNO = Guid.NewGuid();
        //        DateTime TXDateTime = DateTime.Parse(System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")).AddMilliseconds(5);
        //        LdapEmployeeBiz empBiz = new LdapEmployeeBiz();
        //        LDAPEmployee empNow = empBiz.GetLdapEmployeeByEmpId(UserId);
        //        #region 支付電文Log記錄
        //        CaseDataLog log = new CaseDataLog();
        //        log.TXType = "發查支付電文";
        //        log.TXUser = empNow != null && !string.IsNullOrEmpty(empNow.EmpId) ? empNow.EmpId : " ";
        //        log.TXUserName = empNow != null && !string.IsNullOrEmpty(empNow.EmpName) ? empNow.EmpName : " ";
        //        log.md_FuncID = "Menu.CollectionToAgent";
        //        log.TITLE = "經辦作業-待辦理";

        //        #region 支付金額
        //        log.TXSNO = TXSNO;
        //        log.TXDateTime = TXDateTime;
        //        log.ColumnID = "PayAmount";
        //        log.ColumnName = "支付金額";
        //        log.ColumnValueBefore = csOld.PayAmount.ToString();
        //        log.ColumnValueAfter = item.PayAmount.ToString();
        //        log.TabID = "Tab2-1";
        //        log.TabName = "支付設定";
        //        log.TableName = "CaseSeizure";
        //        log.DispSrNo = 17;
        //        log.TableDispActive = "1";
        //        log.CaseId = CaseId.ToString();
        //        log.LinkDataKey = item.SeizureId.ToString();
        //        InsertCaseDataLog(log);
        //        #endregion

        //        #region 解扣金額
        //        log.TXSNO = TXSNO;
        //        log.TXDateTime = TXDateTime;
        //        log.ColumnID = "TripAmount";
        //        log.ColumnName = "解扣金額";
        //        log.ColumnValueBefore = csOld.TripAmount.ToString();
        //        log.ColumnValueAfter = item.TripAmount.ToString();
        //        log.TabID = "Tab2-1";
        //        log.TabName = "支付設定";
        //        log.TableName = "CaseSeizure";
        //        log.DispSrNo = 18;
        //        log.TableDispActive = "1";
        //        log.CaseId = CaseId.ToString();
        //        log.LinkDataKey = item.SeizureId.ToString();
        //        InsertCaseDataLog(log);
        //        #endregion
        //        #endregion
        //        if (rtn)
        //        {
        //            result = "true";
        //        }
        //    }
        //    else
        //    {
        //        result = retP.Substring(5);
        //    }
        //    return result;
        //}
        #endregion
        /// <summary>
        /// 判斷CaseSeizure表是否有資料
        /// </summary>
        /// <param name="caseId"></param>
        /// <returns></returns>
        public bool IsExistCaseSeizureInfo(Guid caseId)
        {
            string strSql = @"select * from CaseSeizure where CaseId = @CaseId";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("@CaseId", caseId));
            IList<CaseSeizure> list = SearchList<CaseSeizure>(strSql);
            bool rtn = list != null && list.Any() ? true : false;
            return rtn;
        }
        #region old 事故電文只更新狀態欄位
        /// <summary>
        /// 事故電文只更新狀態欄位
        /// </summary>
        public bool UpdateAccountStatus(CaseSeizure item, IDbTransaction trans = null)
        {
            string strSql = @"UPDATE [CaseSeizure]
                            SET 
                                [AccountStatus]=@AccountStatus,
	                            [ModifiedUser] = @ModifiedUser ,
	                            [ModifiedDate] = GETDATE()
                            WHERE [CaseId] = @CaseId and [Currency] = @ccy and [Account] =@Account";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("@Account", item.Account));
            Parameter.Add(new CommandParameter("@ccy", item.Currency));
            Parameter.Add(new CommandParameter("@CaseId", item.CaseId));
            Parameter.Add(new CommandParameter("@AccountStatus", item.AccountStatus));
            Parameter.Add(new CommandParameter("@ModifiedUser", item.ModifiedUser));
            return trans == null ? ExecuteNonQuery(strSql) > 0 : ExecuteNonQuery(strSql, trans) > 0;
        }
        #endregion
        /// <summary>
        /// 事故電文 更新TX_60491_Detl的StsDesc欄位
        /// </summary>
        public bool UpdateStsDesc(CTBC.CSFS.Models.TX_60491_Detl item, IDbTransaction trans = null)
        {
            string strSql = @"UPDATE [TX_60491_Detl]
                            SET 
                                [StsDesc]=@StsDesc
                            WHERE [Ccy] = @Ccy and CaseId = @CaseId and Account like @Account";
                          //WHERE [SNO] = @SNO and FKSNO = @FKSNO and CaseId = @CaseId and Account like @Account";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("@StsDesc", item.StsDesc));
            Parameter.Add(new CommandParameter("@Ccy", item.Ccy));
            //Parameter.Add(new CommandParameter("@SNO", item.SNO));
            //Parameter.Add(new CommandParameter("@FKSNO", item.FKSNO));
            Parameter.Add(new CommandParameter("@CaseId", item.CaseId));
            Parameter.Add(new CommandParameter("@Account", "%" + item.Account + "%"));
            return trans == null ? ExecuteNonQuery(strSql) > 0 : ExecuteNonQuery(strSql, trans) > 0;
        }
        /// <summary>
        /// 事故電文 取出TX_00450是否有事故欄位
        /// </summary>
        public bool GetStsDesc(CTBC.CSFS.Models.TX_60491_Detl item, IDbTransaction trans = null)
        {
            string strSql = @"declare @TrnNum varchar(21) select top 1 TrnNum  into #tmp  from tx_00450 where caseid=@caseId and  DATA1 is not null and SUBSTRING(DATA1,1,11) <> 'END OF TXN' and DATA1 <> '' and Account like @Account and WXOption=@WXOption group by TrnNum order by TrnNum desc";
            Parameter.Clear();
            strSql = strSql + @" select @TrnNum = TrnNum from #tmp  select * from  tx_00450 where caseid=@caseId and TrnNum = @TrnNum and  DATA1 is not null and SUBSTRING(DATA1,1,11) <> 'END OF TXN' and DATA1 <> '' and Account like @Account and WXOption=@WXOption 
drop table #tmp";
            if (string.IsNullOrEmpty(item.Account))
            {
                item.Account = "";
            }
            Parameter.Add(new CommandParameter("Account", "%" + item.Account + "%"));
            Parameter.Add(new CommandParameter("caseId", item.CaseId));
            Parameter.Add(new CommandParameter("WXOption", "30"));
            return trans == null ? ExecuteNonQuery(strSql) > 0 : ExecuteNonQuery(strSql, trans) > 0;
        }
        /// <summary>
        /// 通過SeizureId查CaseSeizure資料
        /// </summary>
        public CaseSeizure GetCaseSeizureInfoBySeizureId(int seizureId)
        {
            string strSql = @"select * from CaseSeizure where SeizureId=@SeizureId";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("@SeizureId", seizureId));
            return SearchList<CaseSeizure>(strSql)[0];
        }

        /// <summary>
        /// 做完沖正電文之後，將SeizureStatus 寫為0
        /// </summary>
        public bool UpdateSeizureStatus(CaseSeizure item, IDbTransaction trans = null)
        {
            string strSql = @"UPDATE [CaseSeizure]
                            SET 
                                [SeizureStatus]=@SeizureStatus,
	                            [ModifiedUser] = @ModifiedUser ,
	                            [ModifiedDate] = GETDATE()
                            WHERE [SeizureId] = @SeizureId";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("@SeizureId", item.SeizureId));
            Parameter.Add(new CommandParameter("@SeizureStatus", SeizureStatus.AfterSeizureSetting));
            Parameter.Add(new CommandParameter("@ModifiedUser", item.ModifiedUser));
            return trans == null ? ExecuteNonQuery(strSql) > 0 : ExecuteNonQuery(strSql, trans) > 0;
        }
    }
}
