using CTBC.CSFS.Models;
using CTBC.CSFS.ViewModels;
using CTBC.CSFS.Pattern;
using ICSharpCode.SharpZipLib.Checksums;
using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;

namespace CTBC.CSFS.BussinessLogic
{
    public class eTrsHisRecordBIZ : CommonBIZ
    {
        CaseTrsCommonBIZ pCaseTrsCommonBIZ = new CaseTrsCommonBIZ();


        /// <summary>
        /// 查詢主管檢視放行清單資料
        /// </summary>
        /// <param name="model">查詢條件</param>
        /// <param name="pageNum">當前頁</param>
        /// <param name="strSortExpression">排序欄位</param>
        /// <param name="strSortDirection">排序方式</param>
        /// <returns></returns>
        public IList<CaseCustQuery> GetManagerQueryList(CaseCustCondition model, int pageNum, string strSortExpression, string strSortDirection)
        {
            try
            {
                base.PageIndex = pageNum;

                #region 拼接查詢條件

                string sqlWhere = "";
                string sqlVersionWhere = "";

                // 來文機關 FileGovenment
                if (!string.IsNullOrEmpty(model.FileGovenment))
                {
                    sqlWhere += @" AND CaseCustQuery.LetterDeptName like @LetterDeptName";
                    base.Parameter.Add(new CommandParameter("@LetterDeptName", "%" + model.FileGovenment + "%"));
                }

                // 來文字號   FileNo
                if (!string.IsNullOrEmpty(model.FileNo))
                {
                    sqlWhere += @" AND CaseCustQuery.LetterNo like @LetterNo";
                    base.Parameter.Add(new CommandParameter("@LetterNo", "%" + model.FileNo + "%"));
                }

                // 來文日期S FileDateStart
                if (!string.IsNullOrEmpty(model.FileDateStart))
                {
                    sqlWhere += @" AND convert(nvarchar(10),CaseCustQuery.LetterDate,111) >= @LetterDateStart ";
                    base.Parameter.Add(new CommandParameter("@LetterDateStart", model.FileDateStart));
                }

                // 來文日期E FileDateEnd
                if (!string.IsNullOrEmpty(model.FileDateEnd))
                {
                    sqlWhere += @" AND convert(nvarchar(10),CaseCustQuery.LetterDate,111) <= @LetterDateEnd ";
                    base.Parameter.Add(new CommandParameter("@LetterDateEnd", model.FileDateEnd));
                }

                // 案件編號DocNo
                if (!string.IsNullOrEmpty(model.DocNo))
                {
                    sqlWhere += @" AND CaseCustQuery.DocNo like @DocNo";
                    base.Parameter.Add(new CommandParameter("@DocNo", "%" + model.DocNo + "%"));
                }

                // 建檔日期S DateStart ~ 建檔日期E DateEnd
                if (!string.IsNullOrEmpty(model.DateStart) && !string.IsNullOrEmpty(model.DateEnd))
                {
                    sqlWhere += @"  
AND 
(
	(
            convert(nvarchar(10),CaseCustQuery.RecvDate,111) >= @DateStart
        AND convert(nvarchar(10),CaseCustQuery.RecvDate,111) <= @DateEnd
    )
	OR
	EXISTS
	( 
	    SELECT DocNo FROM CaseCustQueryHistory 
	    WHERE CaseCustQueryHistory.DocNo = CaseCustQuery.DocNo
	    AND convert(nvarchar(10),CaseCustQueryHistory.RecvDate,111) >= @DateStart
	    AND convert(nvarchar(10),CaseCustQueryHistory.RecvDate,111) <= @DateEnd
	)
)
";

                    base.Parameter.Add(new CommandParameter("@DateStart", model.DateStart));
                    base.Parameter.Add(new CommandParameter("@DateEnd", model.DateEnd));
                }
                else if (!string.IsNullOrEmpty(model.DateStart))
                {
                    sqlWhere += @"  
AND 
(
    convert(nvarchar(10),CaseCustQuery.RecvDate,111) >= @DateStart
	OR
	EXISTS
	( 
	    SELECT DocNo FROM CaseCustQueryHistory 
	    WHERE CaseCustQueryHistory.DocNo = CaseCustQuery.DocNo
	    AND convert(nvarchar(10),CaseCustQueryHistory.RecvDate,111) >= @DateStart
	)
)
";
                    base.Parameter.Add(new CommandParameter("@DateStart", model.DateStart));
                }
                else if (!string.IsNullOrEmpty(model.DateEnd))
                {
                    sqlWhere += @"  
AND 
(
    convert(nvarchar(10),CaseCustQuery.RecvDate,111) <= @DateEnd
	OR
	EXISTS
	( 
	    SELECT DocNo FROM CaseCustQueryHistory 
	    WHERE CaseCustQueryHistory.DocNo = CaseCustQuery.DocNo
	    AND convert(nvarchar(10),CaseCustQueryHistory.RecvDate,111) <= @DateEnd
	)
)
";
                    base.Parameter.Add(new CommandParameter("@DateEnd", model.DateEnd));
                }

                // 查詢項目 SearchProgram
                if (!string.IsNullOrEmpty(model.SearchProgram))
                {
                    sqlWhere += @" AND EXISTS
                                    (
                                        SELECT CaseCustNewID
                                         FROM
                                         (
                                        SELECT 
	                                        CaseCustNewID,
	                                        CASE 
		                                        WHEN isnull(CaseCustQueryVersion.OpenFlag,'N') = 'Y' THEN 1
		                                        ELSE 0
	                                        END OpenFlag, 
	                                        CASE 
		                                        WHEN isnull(CaseCustQueryVersion.TransactionFlag,'N') = 'Y' THEN 1
		                                        ELSE 0
	                                        END TransactionFlag
                                        FROM CaseCustQueryVersion 
                                        GROUP BY CaseCustNewID, CaseCustQueryVersion.OpenFlag, CaseCustQueryVersion.TransactionFlag
                                        ) CaseCustQueryVersion
                                        WHERE CaseCustQueryVersion.CaseTrsNewID = CaseMaster.CaseID 
                                        GROUP BY CaseCustNewID
							     ";

                    // 1.基本資料
                    if (model.SearchProgram == "1")
                    {
                        sqlWhere += @" having  MAX(OpenFlag) > 0 AND MAX(TransactionFlag) = 0 ) ";
                    }

                    // 2.存款明細 
                    else if (model.SearchProgram == "2")
                    {
                        sqlWhere += @" having  MAX(CaseCustQueryVersion.OpenFlag) = 0 AND MAX(CaseCustQueryVersion.TransactionFlag) > 0 ) ";
                    }

                    // 3.基本資料+存款明細 
                    else if (model.SearchProgram == "3")
                    {
                        sqlWhere += @" having  MAX(OpenFlag) > 0 AND MAX(TransactionFlag) > 0 ) ";
                    }
                }

                // 拋查結果 Result
                if (!string.IsNullOrEmpty(model.Result))
                {
                    sqlWhere += @" AND CaseCustQuery.Status = @Status";
                    base.Parameter.Add(new CommandParameter("@Status", model.Result));
                }

                // 審核狀態 Status
                if (!string.IsNullOrEmpty(model.Status))
                {
                    sqlWhere += @" AND CaseCustQuery.AuditStatus = @AuditStatus ";
                    base.Parameter.Add(new CommandParameter("@AuditStatus", model.Status));
                }
                #endregion

                #region sql
                StringBuilder sql = new StringBuilder();

                sql.Append(@" 
SELECT 
	RowNum
	,CaseCustQuery.DocNo    --案件編碼
	,CaseCustQueryVersion.CustIdNo
	,CaseCustQuery.LetterNo as FileNo    --來文字號
	,CaseCustQuery.LetterDeptName as Govement    --來文機關
	,CaseCustQuery.MessageNo as GoFileNo    --回文字號
	,case when isnull(CaseCustQueryVersion.OpenFlag,'')='Y' 
	then '1.基本資料<br />'　else '' end 
	+ case when isnull(CaseCustQueryVersion.TransactionFlag,'')='Y' 
	then '2.存款明細'　else '' end as SearchProgram--查詢項目
	,PARMCode.CodeDesc as Result    --拋查結果
	,case when isnull(CaseCustQuery.AuditStatus,'N') = 'Y' and isnull (CaseCustQuery.UploadStatus,'') <> 'Y'
	then 'Y'
	when isnull(CaseCustQuery.AuditStatus,'N') <> 'Y' and isnull(CaseCustQuery.UploadStatus,'') <> 'Y' 
	then 'N' end as AuditStatus    --審核狀態
	,convert(nvarchar(10),CaseCustQuery.RecvDate,111) as RecvDate 
	,'' as LimitDate --限辦日期(T+7)
	,'' as RecvDate5 --(T+5)
	,CaseCustQuery.QFileName
	,CaseCustQuery.QFileName2
	,CaseCustQuery.Status as CaseStatus
	,CaseMaster.CaseID
	,CaseCustQuery.Version
FROM 
(
	select *
	from (
		select 
			ROW_NUMBER() OVER ( ORDER BY CaseCustQuery.DocNo,CaseCustQuery.Version  ) AS RowNum
			,CaseCustQuery.DocNo,CaseCustQuery.Version,CaseMaster.CaseID
		from CaseCustQuery
		inner join  CaseCustQueryVersion
		on CaseCustQueryVersion.CaseTrsNewID = CaseMaster.CaseID
		WHERE (CaseCustQuery.Status = '03' or CaseCustQuery.Status = '07') 
		and isnull(CaseCustQuery.UploadStatus,'') <> 'Y'  " + sqlWhere + @"
		GROUP BY CaseCustQuery.DocNo,CaseCustQuery.Version,CaseMaster.CaseID
	) as tabletemp
");

                // 判斷是否分頁
                sql.Append(@" WHERE  RowNum > " + PageSize * (pageNum - 1) + " AND RowNum < " + ((PageSize * pageNum) + 1));

                sql.Append(@" 
)RESULT
INNER JOIN CaseCustQuery
	ON RESULT.NewID = CaseMaster.CaseID
INNER JOIN CaseCustQueryVersion
	ON CaseCustQueryVersion.CaseTrsNewID = CaseMaster.CaseID " + sqlVersionWhere + @"
left join PARMCode
	ON CaseCustQuery.Status = PARMCode.CodeNo
	AND CodeType = 'CaseCustStatus'
ORDER BY CaseCustQuery.DocNo,CaseCustQuery.Version, CaseCustQueryVersion.CustIdNo 

");

                #endregion

                // 資料總筆數
                string sqlCount = @"
                            SELECT count(*)
                            FROM
                            (
                                select
                                    CaseMaster.CaseID
                                from CaseCustQuery
                                inner join  CaseCustQueryVersion
                                on CaseCustQueryVersion.CaseTrsNewID = CaseMaster.CaseID
                                left join PARMCode
                                on CaseCustQuery.Status = PARMCode.CodeNo
                                and CodeType = 'CaseCustStatus'
                                WHERE (CaseCustQuery.Status = '03' or CaseCustQuery.Status = '07') 
                                and isnull(CaseCustQuery.UploadStatus,'') <> 'Y' " + sqlWhere + " GROUP BY CaseMaster.CaseID)Result";
                base.DataRecords = int.Parse(base.ExecuteScalar(sqlCount).ToString());

                // 查詢清單資料
                IList<CaseCustQuery> _ilsit = base.SearchList<CaseCustQuery>(sql.ToString());

                if (_ilsit != null)
                {
                    for (int i = 0; i < _ilsit.Count; i++)
                    {
                        _ilsit[i].LimitDate = pCaseTrsCommonBIZ.DateAdd(_ilsit[i].RecvDate, 7);
                        _ilsit[i].RecvDate5 = pCaseTrsCommonBIZ.DateAdd(_ilsit[i].RecvDate, 5);
                    }
                    return _ilsit;
                }
                else
                {
                    return new List<CaseCustQuery>();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 主管檢視放行明細頁面-清單資料查詢
        /// </summary>
        /// <param name="model">實體類</param>
        /// <param name="pageNum">當前頁</param>
        /// <param name="strSortExpression">排序欄位</param>
        /// <param name="strSortDirection">排序方式</param>
        /// <returns></returns>
        public IList<CaseTrsQueryVersion> GetDetailQueryList(CaseHisCondition model, int pageNum, string strSortExpression, string strSortDirection)
        {
            try
            {
                base.PageIndex = pageNum;

                #region 拼接查詢條件
                string sqlWhere = "";

                // 主檔主鍵
                //if (!string.IsNullOrEmpty(model.CaseId))
                //{
                //    sqlWhere += @" CaseTrsQueryVersion.CaseTrsNewID = @CaseTrsNewID";
                //    base.Parameter.Add(new CommandParameter("@CaseTrsNewID", model.CaseId));
                //}
                // 主檔主鍵
                if (!string.IsNullOrEmpty(model.CaseId))
                {
                    //sqlWhere += @" CaseTrsQueryVersion.NewID = @NewID";
                    //base.Parameter.Add(new CommandParameter("@NewID", model.NewID));
                    sqlWhere += @" CaseTrsQueryVersion.CaseTrsNewID = @CaseID";
                    base.Parameter.Add(new CommandParameter("@CaseID", model.CaseId));
                }
                #endregion

                #region sql
                StringBuilder sqlNotPress = new StringBuilder();
                sqlNotPress.Append(@"");
                StringBuilder sql = new StringBuilder();

                sql.Append(@"

Select   * into #401 from
 (select  distinct 
    BOPS000401Recv.acct_no
  as acctno401 ,CaseTrsRFDMRecv.Acct_No as Recv_acct,Case when isnull(CaseTrsRFDMRecv.Acct_No,'') ='' then '查無相關資料' else '' end as RFDMQryMessage ,CaseTrsQueryVersion.NewID,CaseTrsQueryVersion.CustID from CaseTrsQueryVersion 
 inner join  BOPS000401Recv
on CaseTrsQueryVersion.Newid = BOPS000401Recv.VersionNewID  
left join CaseTrsRFDMRecv
on BOPS000401Recv.VersionNewID  = CaseTrsRFDMRecv.VersionNewID and substring(BOPS000401Recv.Acct_no,1,16) = CaseTrsRFDMRecv.Acct_No 
where CaseTrsQueryVersion.CaseTrsNewID = @CaseId    ) as table401 


SELECT  
CaseTrsQueryVersion.RowNum
    ,CaseTrsQueryVersion.CustId   ,CaseTrsQueryVersion.CustAccount 
    ,case when isnull(CaseTrsQueryVersion.OpenFlag,'')='Y' then '1.基本資料<br />' 　
        else '' end
    +case when isnull(CaseTrsQueryVersion.TransactionFlag,'')='Y' 
        then '2.存款明細'　else '' end as SearchProgram --查詢項目
    ,case when isnull(CaseTrsQueryVersion.TransactionFlag,'')='Y'
            then 
			case when  LEN(CaseTrsQueryVersion.QDateS)=8 and CaseTrsQueryVersion.QDateS <> '19110101' 
                then     substring(CaseTrsQueryVersion.QDateS,1, 4) +'/'
                    + substring(CaseTrsQueryVersion.QDateS,5,2)  +'/' 
                    + substring(CaseTrsQueryVersion.QDateS, 7,2) 	
            WHEN CaseTrsQueryVersion.QDateS = '19110101' and LEN(BOPS000401Recv.OPEN_DATE) > 0
        THEN  substring(BOPS000401Recv.OPEN_DATE,7,4)+'/'
        + substring(BOPS000401Recv.OPEN_DATE,4, 2) +'/' +substring(BOPS000401Recv.OPEN_DATE,1, 2) 
		ELSE '' END 
    ELSE '' End  as  QDateS --查詢期間(起)
    ,case when isnull(CaseTrsQueryVersion.TransactionFlag,'')='Y'
            then CASE WHEN LEN(CaseTrsQueryVersion.QDateE)=8
                    THEN substring(CaseTrsQueryVersion.QDateE,1, 4) +'/'
                    + substring(CaseTrsQueryVersion.QDateE,5,2)  +'/'
                    + substring(CaseTrsQueryVersion.QDateE, 7,2) ELSE '' END
            else '' end as QDateE  --查詢期間(迄)
    , BOPS000401Send.AcctNO  as GoFileNo     --存款帳號
  , ( CASE WHEN isnull(BOPS000401Send.QueryErrMsg,'') ='' then ' ' else '401:'+rtrim(ltrim(BOPS000401Send.QueryErrMsg)) +'  <br/>'  end )
  + ( CASE WHEN isnull(BOPS081019Send.QueryErrMsg,'') ='' then '' else '81019:'+BOPS081019Send.QueryErrMsg  end ) 
   + ( CASE WHEN isnull(BOPS067050Send.QueryErrMsg,'') ='' then '' else '67050:'+BOPS067050Send.QueryErrMsg  end ) 
  + ( CASE WHEN isnull(#401.RFDMQryMessage,'') ='' then '' else 'RFDM:'+#401.RFDMQryMessage  +'  <br/>'  end ) as RFDMQryMessage
    , CASE
        WHEN LEN(BOPS000401Recv.OPEN_DATE) > 0
        THEN  right('000'+CONVERT(nvarchar(3),CONVERT(int, substring(BOPS000401Recv.OPEN_DATE,7,4))-1911),3)+'/'
        + substring(BOPS000401Recv.OPEN_DATE,4, 2) +'/'
        +substring(BOPS000401Recv.OPEN_DATE,1, 2) ELSE null END AS OpenDate    --開戶日   
    , CASE
        WHEN LEN(BOPS000401Recv.LAST_DATE) > 0
        THEN  
						CASE
							WHEN substring(BOPS000401Recv.LAST_DATE,7,4) = '9999'
							THEN '999/99/99'
							ELSE
								right('000'+CONVERT(nvarchar(3),CONVERT(int, substring(BOPS000401Recv.LAST_DATE,7,4))-1911),3)+'/'
									+ substring(BOPS000401Recv.LAST_DATE,4, 2) +'/'
									+substring(BOPS000401Recv.LAST_DATE,1, 2)
						END
        ELSE null END as LastDate    --最後交易日
    ,PARMCode.CodeDesc CaseStatusName
from
(
	SELECT distinct
	* FROM
	(
		SELECT 
		ROW_NUMBER() OVER ( ORDER BY CustID ) AS RowNum
		,*
		from  CaseTrsQueryVersion
		WHERE   " + sqlWhere + @"
	) as CaseTrsQueryVersion ");

                sql.Append(@" WHERE  RowNum > " + PageSize * (pageNum - 1) + " AND RowNum < " + ((PageSize * pageNum) + 1));

                sql.Append(@" )CaseTrsQueryVersion
                            LEFT JOIN PARMCode
                                ON CaseTrsQueryVersion.Status = PARMCode.CodeNo
                                AND PARMCode.CodeType = 'CaseCustStatus'
 							LEFT JOIN BOPS000401Send
                                ON BOPS000401Send.VersionNewID = CaseTrsQueryVersion.NewID
							LEFT JOIN BOPS067050Send
                                ON BOPS067050Send.VersionNewID = CaseTrsQueryVersion.NewID
                            LEFT JOIN BOPS000401Recv
                                ON BOPS000401Recv.SendNewID = BOPS000401Send.NewID 
                            left join  #401							
                                ON BOPS000401Recv.Acct_No = #401.acctno401 
                           LEFT JOIN BOPS081019Send
                                ON BOPS081019Send.VersionNewID  = BOPS000401Send.VersionNewID and BOPS000401Send.Currency=BOPS081019Send.Currency  and BOPS000401Send.AcctNo like '%'+substring(BOPS081019Send.acctNo,6,12)+'%'
ORDER BY ROWNUM,CaseTrsQueryVersion.CustID,CaseTrsQueryVersion.CustAccount,CaseTrsQueryVersion.idno,BOPS000401Send.acctno
drop table #401

                            ");


                //// 判斷是否分頁
                //sql.Append(@" WHERE  RowNum > " + PageSize * (pageNum - 1)
                //                   + " AND RowNum < " + ((PageSize * pageNum) + 1));

                #endregion

                // 資料總筆數
                string sqlCount = @"
                                select
                                    count(0)
                                from  CaseTrsQueryVersion
                                left join PARMCode
                                on CaseTrsQueryVersion.Status = PARMCode.CodeNo
                                and PARMCode.CodeType = 'CaseCustStatus'
                                WHERE " + sqlWhere;
                base.DataRecords = int.Parse(base.ExecuteScalar(sqlCount).ToString());
                //model.CaseTrsQueryVersion.PageSize = base.PageSize;
                //model.CaseTrsQueryVersion.CurrentPage = pageNum;
                //model.CaseTrsQueryVersion.TotalItemCount = base.DataRecords;
                //model.CaseTrsQueryVersion.SortExpression = strSortExpression;
                //model.CaseTrsQueryVersion.SortDirection = strSortDirection;
                // 查詢清單資料
                IList<CaseTrsQueryVersion> _ilsit = base.SearchList<CaseTrsQueryVersion>(sql.ToString());

                if (_ilsit != null)
                {
                    return _ilsit;
                }
                else
                {
                    return new List<CaseTrsQueryVersion>();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public bool Save(string strKey, string CustID, string CustAccount,string DateS ,string DateE,string LogonUser)
        {
            string Sql = "";
            IDbConnection dbConnection = OpenConnection();
            IDbTransaction dbTrans = null;
            try
            {
                using (dbConnection)
                {
                    dbTrans = dbConnection.BeginTransaction();

                    base.Parameter.Clear();

                    Sql += @" 
                            update CaseTrsQueryVersion
                            set CustId = @CustId
                             ,CustAccount = @CustAccount
                             ,QDateS = @QDateS
                             ,QDateE = @QDateE
                             ,ModifiedDate = getdate()
                             ,ModifiedUser = @ModifiedUser 
                            where NewID = @NewID ; ";
                    base.Parameter.Add(new CommandParameter("@NewID", strKey));
                    base.Parameter.Add(new CommandParameter("@CustID", CustID));
                    base.Parameter.Add(new CommandParameter("@CustAccount", CustAccount));
                    base.Parameter.Add(new CommandParameter("@QDateS", DateS));
                    base.Parameter.Add(new CommandParameter("@QDateE", DateE));
                    base.Parameter.Add(new CommandParameter("@ModifiedUser", LogonUser));
                    int result = base.ExecuteNonQuery(Sql);

                    // 如果執行結果=資料筆數，就返回true，否則返回false
                    dbTrans.Commit();
                    return true;
                }
            }
            catch (Exception ex)
            {
                dbTrans.Rollback();

                return false;
            }
        }


        /// <summary>
        /// 上傳
        /// </summary>
        /// <param name="strKey">主檔主鍵</param>
        /// <param name="strAuditStatus">主檔審核狀態</param>
        /// <param name="LogonUser">登錄人員ID</param>
        /// <returns></returns>
        public bool Upload(string strKey, string strAuditStatus, string LogonUser)
        {
            IDbConnection dbConnection = OpenConnection();
            IDbTransaction dbTrans = null;
            try
            {
                using (dbConnection)
                {
                    dbTrans = dbConnection.BeginTransaction();

                    string[] arrayKey = strKey.Split(',');
                    string[] arrayAuditStatus = strAuditStatus.Split(',');

                    string Sql = "";
                    base.Parameter.Clear();

                    // 遍歷主鍵數組,組sql
                    for (int i = 0; i < arrayKey.Length; i++)
                    {
                        Sql += @" 
                            update CaseCustQuery
                            set 
                                UploadStatus = 'Y'
,UploadUserID = @ModifiedUser 
,CloseDate = getdate()
                                ,ModifiedDate = getdate()
                                ,ModifiedUser = @ModifiedUser 
                            where NewID = @NewID" + i.ToString() + "; ";
                        base.Parameter.Add(new CommandParameter("@NewID" + i.ToString(), arrayKey[i]));
                    }
                    base.Parameter.Add(new CommandParameter("@ModifiedUser", LogonUser));

                    int result = base.ExecuteNonQuery(Sql);

                    // 如果執行結果=資料筆數，就返回true，否則返回false
                    dbTrans.Commit();
                    return true;
                }
            }
            catch (Exception ex)
            {
                dbTrans.Rollback();

                return false;
            }
        }


        #region 回文檢視

        /// <summary>
        /// 根據主鍵取得回文檔案名稱
        /// </summary>
        /// <param name="strPk">主鍵</param>
        /// <returns></returns>
        public CaseCustQuery GetReturnFilesByPk(string strPk)
        {

            string sqlSlect = @" SELECT  ROpenFileName ,
                                          RFileTransactionFileName,
                                          Version
                                  FROM    CaseCustQuery
                                  WHERE   NewID=@NewID";

            base.Parameter.Clear();

            base.Parameter.Add(new CommandParameter("@NewID", strPk));

            return base.SearchList<CaseCustQuery>(sqlSlect)[0];
        }

        /// <summary>
        /// 根據主鍵取得回文檔案名稱
        /// </summary>
        /// <param name="strPk">主鍵</param>
        /// <returns></returns>


        public CaseMaster GetCaseMaster(string caseId, IDbTransaction trans = null)
        {
            string strSql = @" SELECT  *                                       
                                  FROM    CaseMaster
                                  WHERE   caseid=@CaseId";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("@CaseId", caseId));
            IList<CaseMaster> list = trans == null ? SearchList<CaseMaster>(strSql) : SearchList<CaseMaster>(strSql, trans);
            if (list != null)
            {
                return list.Count > 0 ? list[0] : new CaseMaster();
            }
            return new CaseMaster();
        }

        public CaseHisCondition GetCaseHisCondition(string NewID, IDbTransaction trans = null)
        {
            string strSql = @" SELECT  *                                       
                                  FROM    CaseTrsQueryVersion
                                  WHERE   NewID=@CaseId";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("@CaseId", NewID));
            IList<CaseHisCondition> list = trans == null ? SearchList<CaseHisCondition>(strSql) : SearchList<CaseHisCondition>(strSql, trans);
            if (list != null)
            {
                return list.Count > 0 ? list[0] : new CaseHisCondition();
            }
            return new CaseHisCondition();
        }

        public CaseTrsQueryVersion GetCaseTrsQueryVersion(string NewID, IDbTransaction trans = null)
        {
            string strSql = @" SELECT  *                                       
                                  FROM    CaseTrsQueryVersion
                                  WHERE   NewID=@CaseId";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("@CaseId", NewID));
            IList<CaseTrsQueryVersion> list = trans == null ? SearchList<CaseTrsQueryVersion>(strSql) : SearchList<CaseTrsQueryVersion>(strSql, trans);
            if (list != null)
            {
                return list.Count > 0 ? list[0] : new CaseTrsQueryVersion();
            }
            return new CaseTrsQueryVersion();
        }
        /// <summary>
        /// 查詢勾選案件的回文
        /// </summary>
        /// <param name="strDocNo">案件編號</param>
        /// <returns></returns>
        public IList<CaseCustQuery> GedReturnFile(string strNewID)
        {

            string sqlSelect = @"  SELECT  DocNo + '_'+ CONVERT(NVARCHAR, Version)  + '.pdf'  AS PDFFileName
                                    FROM    CaseCustQuery
                                    WHERE   @NewID LIKE '%' + CONVERT(NVARCHAR(50), [NewID]) + '%'
                                    ORDER BY DocNo";

            base.Parameter.Clear();

            base.Parameter.Add(new CommandParameter("@NewID", strNewID));

            return base.SearchList<CaseCustQuery>(sqlSelect);
        }

        /// <summary>
        /// 压缩多個文件
        /// </summary>
        /// <param name="sourceFilePath"></param>
        /// <param name="destinationZipFilePath"></param>
        public void CreateZip(string sourceFilePath, string destinationZipFilePath, List<string> files, string strPassWord)
        {
            if (sourceFilePath[sourceFilePath.Length - 1] != System.IO.Path.DirectorySeparatorChar)
                sourceFilePath += System.IO.Path.DirectorySeparatorChar;

            ZipOutputStream zipStream = new ZipOutputStream(File.Create(destinationZipFilePath));

            zipStream.SetLevel(6);  // 压缩级别 0-9
            zipStream.Password = strPassWord;
            foreach (string file in files)
            {
                FileStream fileStream = File.OpenRead(file);

                byte[] buffer = new byte[fileStream.Length];


                fileStream.Read(buffer, 0, buffer.Length);
                string tempFile = file.Substring(sourceFilePath.LastIndexOf("\\") + 1);
                ZipEntry entry = new ZipEntry(Path.GetFileName(file));

                entry.DateTime = DateTime.Now;
                entry.Size = fileStream.Length;
                fileStream.Close();

                zipStream.PutNextEntry(entry);

                zipStream.Write(buffer, 0, buffer.Length);

            }

            zipStream.Finish();
            zipStream.Close();

            GC.Collect();
            GC.Collect(1);
        }

        #endregion
    }
}
