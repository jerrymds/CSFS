using CTBC.CSFS.Models;
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
    public class NewCaseCustManagerBIZ : CommonBIZ
    {
        NewCaseCustCommonBIZ NewpCaseCustCommonBIZ = new NewCaseCustCommonBIZ();


        /// <summary>
        /// 查詢主管檢視放行清單資料
        /// </summary>
        /// <param name="model">查詢條件</param>
        /// <param name="pageNum">當前頁</param>
        /// <param name="strSortExpression">排序欄位</param>
        /// <param name="strSortDirection">排序方式</param>
        /// <returns></returns>
        public IList<CaseCustMaster> GetManagerQueryList(NewCaseCustCondition model, int pageNum, string strSortExpression, string strSortDirection)
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
                    sqlWhere += @" AND CaseCustMaster.LetterDeptName like @LetterDeptName";
                    base.Parameter.Add(new CommandParameter("@LetterDeptName", "%" + model.FileGovenment + "%"));
                }

                // 來文字號   FileNo
                if (!string.IsNullOrEmpty(model.FileNo))
                {
                    sqlWhere += @" AND CaseCustMaster.LetterNo like @LetterNo";
                    base.Parameter.Add(new CommandParameter("@LetterNo", "%" + model.FileNo + "%"));
                }

                // 來文日期S FileDateStart
                if (!string.IsNullOrEmpty(model.FileDateStart))
                {
                    sqlWhere += @" AND convert(nvarchar(10),CaseCustMaster.LetterDate,111) >= @LetterDateStart ";
                    base.Parameter.Add(new CommandParameter("@LetterDateStart", model.FileDateStart));
                }

                // 來文日期E FileDateEnd
                if (!string.IsNullOrEmpty(model.FileDateEnd))
                {
                    sqlWhere += @" AND convert(nvarchar(10),CaseCustMaster.LetterDate,111) <= @LetterDateEnd ";
                    base.Parameter.Add(new CommandParameter("@LetterDateEnd", model.FileDateEnd));
                }

                // 案件狀態 CaseStatus
                if (!string.IsNullOrEmpty(model.CaseStatus))
                {
                    // 格式錯誤
                    //if (model.CaseStatus == "99")
                    //{
                    //    sqlWhere += @" AND CaseCustMaster.ImportFormFlag ='Y'";
                    //}
                    //else
                    {
                        sqlWhere += @" AND CaseCustMaster.Status = @CaseStatus ";
                        base.Parameter.Add(new CommandParameter("@CaseStatus", model.CaseStatus));
                    }
                }
                //else
                //{
                //    sqlWhere += @" and (CaseCustMaster.Status = '03' or CaseCustMaster.Status = '07') ";
                //}

                // 是否重號
                if (!string.IsNullOrEmpty(model.Double))
                {
                    // 格式錯誤
                    if (model.Double == "Y")
                    {
                        if (!string.IsNullOrEmpty(model.DocNo))
                        {
                            sqlWhere += @" and CaseCustMaster.DocNo in (Select  docno from CaseCustOutputF1  where DocNo = '" + model.DocNo + "' and Len(Cust_ID_NO) = 9 or  Len(Cust_ID_NO) = 11  group by docno) ";
                        }
                        else
                        {
                            sqlWhere += @" and CaseCustMaster.DocNo in (Select  docno from CaseCustOutputF1  where Len(Cust_ID_NO) = 9 or  Len(Cust_ID_NO) = 11  group by docno) ";
                        }
                    }
                }
                // 案件編號DocNo
                if (!string.IsNullOrEmpty(model.DocNo))
                {
                    sqlWhere += @" AND CaseCustMaster.DocNo like @DocNo";
                    base.Parameter.Add(new CommandParameter("@DocNo", "%" + model.DocNo + "%"));
                }

                // 客戶ID
                if (!string.IsNullOrEmpty(model.CustIdNo))
                {
                    sqlWhere += @" AND CaseCustDetails.CustIdNo like @CustIdNo";
                    base.Parameter.Add(new CommandParameter("@CustIdNo", "%" + model.CustIdNo + "%"));
                }

                // 客戶帳號
                if (!string.IsNullOrEmpty(model.CustAccount))
                {
                    sqlWhere += @" AND CaseCustDetails.CustAccount like @CustAccount";
                    base.Parameter.Add(new CommandParameter("@CustAccount", "%" + model.CustAccount + "%"));
                }

                // 建檔日期S DateStart ~ 建檔日期E DateEnd
                if (!string.IsNullOrEmpty(model.DateStart) && !string.IsNullOrEmpty(model.DateEnd))
                {
                    sqlWhere += @"  
AND 
(
	(
            convert(nvarchar(10),CaseCustMaster.RecvDate,111) >= @DateStart
        AND convert(nvarchar(10),CaseCustMaster.RecvDate,111) <= @DateEnd
    )
	--OR
	--EXISTS
	--( 
	--    SELECT DocNo FROM CaseCustQueryHistory 
	--    WHERE CaseCustQueryHistory.DocNo = CaseCustMaster.DocNo
	--    AND convert(nvarchar(10),CaseCustQueryHistory.RecvDate,111) >= @DateStart
	--    AND convert(nvarchar(10),CaseCustQueryHistory.RecvDate,111) <= @DateEnd
	--)
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
    convert(nvarchar(10),CaseCustMaster.RecvDate,111) >= @DateStart
	--OR
	--EXISTS
	--( 
	--    SELECT DocNo FROM CaseCustQueryHistory 
	--    WHERE CaseCustQueryHistory.DocNo = CaseCustMaster.DocNo
	--    AND convert(nvarchar(10),CaseCustQueryHistory.RecvDate,111) >= @DateStart
	--)
)
";
                    base.Parameter.Add(new CommandParameter("@DateStart", model.DateStart));
                }
                else if (!string.IsNullOrEmpty(model.DateEnd))
                {
                    sqlWhere += @"  
AND 
(
    convert(nvarchar(10),CaseCustMaster.RecvDate,111) <= @DateEnd
	OR
	--EXISTS
	--( 
	--    SELECT DocNo FROM CaseCustQueryHistory 
	--    WHERE CaseCustQueryHistory.DocNo = CaseCustMaster.DocNo
	--    AND convert(nvarchar(10),CaseCustQueryHistory.RecvDate,111) <= @DateEnd
	--)
)
";
                    base.Parameter.Add(new CommandParameter("@DateEnd", model.DateEnd));
                }

                // 查詢項目 SearchProgram
                if (!string.IsNullOrEmpty(model.SearchProgram))
                {
                    sqlWhere += @" AND EXISTS
                                    (
                                        SELECT CaseCustMasterId
                                         FROM
                                         (
                                        SELECT 
	                                        CaseCustMasterId,QueryType
                                        FROM CaseCustDetails 
                                        GROUP BY CaseCustMasterId, CaseCustDetails.QueryType
                                        ) CaseCustDetails
                                        WHERE CaseCustDetails.CaseCustMasterId = CaseCustMaster.NewID 
                                       
							     ";

                    // 1.基本資料
                    if (model.SearchProgram == "1" )
                    {
                        sqlWhere += @" and QueryType = '1' GROUP BY CaseCustMasterId ) ";
                    }

                    // 2.存款明細 
                    else if (model.SearchProgram == "2" )
                    {
                        sqlWhere += @" and QueryType = '2' GROUP BY CaseCustMasterId ) ";
                    }

                    // 3.基本資料
                    else if (model.SearchProgram == "3")
                    {
                        sqlWhere += @" and QueryType = '3' GROUP BY CaseCustMasterId ) ";
                    }

                    // 4.存款明細
                    else if (model.SearchProgram == "4")
                    {
                        sqlWhere += @" and QueryType = '4' GROUP BY CaseCustMasterId ) ";
                    }

                    // 5.保管箱
                    else if (model.SearchProgram == "5")
                    {
                        sqlWhere += @" and QueryType = '5' GROUP BY CaseCustMasterId ) ";
                    }
                }

                // 拋查結果 Result
                if (!string.IsNullOrEmpty(model.Result))
                {
                    switch (model.Result)
                    {
                        case "04":
                            sqlWhere += @" AND CaseCustMaster.Status  in ('04','08','77','86','87') ";//少2個
                            break;
                        case "03":
                            sqlWhere += @" AND CaseCustMaster.Status  in ('03','07','55','98','99','88') ";
                            break;
                        case "01":
                            sqlWhere += @" AND CaseCustMaster.Status  in ('00','01','02','06') ";
                            break;
                        default: /* 可选的 */
                            break;
                    }
                    //sqlWhere += @" AND CaseCustMaster.Status = @Status";
                    //base.Parameter.Add(new CommandParameter("@Status", model.Result));
                }

                // 審核狀態 Status
                if (!string.IsNullOrEmpty(model.Status))
                {
                    sqlWhere += @" AND CaseCustMaster.AuditStatus = @AuditStatus ";
                    base.Parameter.Add(new CommandParameter("@AuditStatus", model.Status));
                }
                #endregion

                #region sql
                StringBuilder sql = new StringBuilder();
                if (!string.IsNullOrEmpty(model.CaseStatus))
                {
                   sql.Append(@" 
                    SELECT 
	                    --RowNum
                        ROW_NUMBER() OVER ( ORDER BY CaseCustMaster.DocNo,CaseCustMaster.Version,CaseCustDetails.CustIdNo,CaseCustDetails.CustAccount  ) AS RowNum
	                    ,CaseCustMaster.DocNo    --案件編碼
	                    ,CaseCustDetails.CustIdNo
	                    ,CaseCustDetails.CustAccount
	                    ,isNull(CaseCustMaster.LetterNo,' ') as FileNo    --來文字號
	                    ,CaseCustMaster.LetterDeptName as Govement    --來文機關
	                    ,CaseCustMaster.MessageNo as GoFileNo    --回文字號
                        ,CASE Rtrim(Ltrim([QueryType]))  
                            WHEN '1' THEN '1.基本資料(ID)'  
                            WHEN '2' THEN '2.存款明細(ID)'  
                            WHEN '3' THEN '3.基本資料(帳號)'  
                            WHEN '4' THEN '4.存款明細(帳號)'  
		                    WHEN '5' THEN '5.保管箱' 
                             ELSE ''  
                        END  as SearchProgram --查詢項目
	                    ,PARMCode.CodeDesc as Result    --拋查結果
	                    ,case when isnull(CaseCustMaster.AuditStatus,'N') = 'Y' and isnull (CaseCustMaster.UploadStatus,'') <> 'Y'
	                    then 'Y'
	                    when isnull(CaseCustMaster.AuditStatus,'N') <> 'Y' and isnull(CaseCustMaster.UploadStatus,'') <> 'Y' 
	                    then 'N' end as AuditStatus    --審核狀態
	                    ,convert(nvarchar(10),CaseCustMaster.RecvDate,111) as RecvDate 
	                    ,DATEADD(day,10,RecvDate) as LimitDate --限辦日期(T+10)
	                    ,DATEADD(day,5,RecvDate) as RecvDate5 --(T+5)
	                    ,CaseCustMaster.QFileName
	                    ,CaseCustMaster.QFileName2
	                    ,CaseCustMaster.QFileName3
	                    ,CaseCustMaster.Status as CaseStatus
	                    ,CaseCustMaster.NewID
	                    ,CaseCustMaster.Version
                    FROM 
                    (
	                    select *
	                    from (
		                    select 
			                    ROW_NUMBER() OVER ( ORDER BY CaseCustMaster.DocNo,CaseCustMaster.Version  ) AS RowNum
			                    ,CaseCustMaster.DocNo,CaseCustMaster.Version,CaseCustMaster.NewID
		                    from CaseCustMaster
		                    left join  CaseCustDetails
		                    on CaseCustDetails.CaseCustMasterID = CaseCustMaster.NewID
		                    WHERE isnull(CaseCustMaster.UploadStatus,'') <> 'Y'  " + sqlWhere + @"
		                    GROUP BY CaseCustMaster.DocNo,CaseCustMaster.Version,CaseCustMaster.NewID
	                    ) as tabletemp
                    ");
                }
                else
                {
                    sql.Append(@" 
                    SELECT 
	                    --RowNum
                        ROW_NUMBER() OVER ( ORDER BY CaseCustMaster.DocNo,CaseCustMaster.Version,CaseCustDetails.CustIdNo,CaseCustDetails.CustAccount  ) AS RowNum
	                    ,CaseCustMaster.DocNo    --案件編碼
	                    ,CaseCustDetails.CustIdNo
	                    ,CaseCustDetails.CustAccount
	                    ,isNull(CaseCustMaster.LetterNo,' ') as FileNo    --來文字號
	                    ,CaseCustMaster.LetterDeptName as Govement    --來文機關
	                    ,CaseCustMaster.MessageNo as GoFileNo    --回文字號
                        ,CASE Rtrim(Ltrim([QueryType]))  
                            WHEN '1' THEN '1.基本資料(ID)'  
                            WHEN '2' THEN '2.存款明細(ID)'  
                            WHEN '3' THEN '3.基本資料(帳號)'  
                            WHEN '4' THEN '4.存款明細(帳號)'  
		                    WHEN '5' THEN '5.保管箱' 
                                ELSE ''  
                        END  as SearchProgram --查詢項目
	                    ,PARMCode.CodeDesc as Result    --拋查結果
	                    ,case when isnull(CaseCustMaster.AuditStatus,'N') = 'Y' and isnull (CaseCustMaster.UploadStatus,'') <> 'Y'
	                    then 'Y'
	                    when isnull(CaseCustMaster.AuditStatus,'N') <> 'Y' and isnull(CaseCustMaster.UploadStatus,'') <> 'Y' 
	                    then 'N' end as AuditStatus    --審核狀態
	                    ,convert(nvarchar(10),CaseCustMaster.RecvDate,111) as RecvDate 
	                    ,DATEADD(day,10,RecvDate) as LimitDate --限辦日期(T+10)
	                    ,DATEADD(day,5,RecvDate) as RecvDate5 --(T+5)
	                    ,CaseCustMaster.QFileName
	                    ,CaseCustMaster.QFileName2
	                    ,CaseCustMaster.QFileName3
	                    ,CaseCustMaster.Status as CaseStatus
	                    ,CaseCustMaster.NewID
	                    ,CaseCustMaster.Version
                    FROM 
                    (
	                    select *
	                    from (
		                    select 
			                    ROW_NUMBER() OVER ( ORDER BY CaseCustMaster.DocNo,CaseCustMaster.Version  ) AS RowNum
			                    ,CaseCustMaster.DocNo,CaseCustMaster.Version,CaseCustMaster.NewID
		                    from CaseCustMaster
		                    left join  CaseCustDetails
		                    on CaseCustDetails.CaseCustMasterID = CaseCustMaster.NewID
		                    WHERE 1=1 -- (CaseCustMaster.Status = '03' or CaseCustMaster.Status = '07') 
		                    and isnull(CaseCustMaster.UploadStatus,'') <> 'Y'  " + sqlWhere + @"
		                    GROUP BY CaseCustMaster.DocNo,CaseCustMaster.Version,CaseCustMaster.NewID
	                    ) as tabletemp
                    ");
                }

                // 判斷是否分頁
                sql.Append(@" WHERE  RowNum > " + PageSize * (pageNum - 1) + " AND RowNum < " + ((PageSize * pageNum) + 1));

                sql.Append(@" 
)RESULT
INNER JOIN CaseCustMaster
	ON RESULT.NewID = CaseCustMaster.NewID

left JOIN CaseCustDetails
	ON CaseCustDetails.CaseCustMasterID = CaseCustMaster.NewID  

" + sqlVersionWhere + @"
left join PARMCode
	ON CaseCustMaster.Status = PARMCode.CodeNo
	AND CodeType = 'CaseCustStatus'
ORDER BY CaseCustMaster.DocNo,CaseCustMaster.Version, CaseCustDetails.CustIdNo, CaseCustDetails.CustAccount");

                #endregion

                // 資料總筆數
                string sqlCount = @"
                            SELECT count(*)
                            FROM
                            (
                                select
                                    CaseCustMaster.NewID
                                from CaseCustMaster
                                left join  CaseCustDetails
                                on CaseCustDetails.CaseCustMasterID = CaseCustMaster.NewID
                                left join PARMCode
                                on CaseCustMaster.Status = PARMCode.CodeNo
                                and CodeType = 'CaseCustStatus'
                                WHERE 1=1 -- (CaseCustMaster.Status = '03' or CaseCustMaster.Status = '07') 
                                and isnull(CaseCustMaster.UploadStatus,'') <> 'Y' " + sqlWhere + " GROUP BY CaseCustMaster.NewID)Result";
                base.DataRecords = int.Parse(base.ExecuteScalar(sqlCount).ToString());

                // 查詢清單資料
                IList<CaseCustMaster> _ilsit = base.SearchList<CaseCustMaster>(sql.ToString());

                if (_ilsit != null)
                {
                    for (int i = 0; i < _ilsit.Count; i++)
                    {
                        _ilsit[i].LimitDate = NewpCaseCustCommonBIZ.DateAdd(_ilsit[i].RecvDate, 7);
                        _ilsit[i].RecvDate5 = NewpCaseCustCommonBIZ.DateAdd(_ilsit[i].RecvDate, 5);
                    }
                    return _ilsit;
                }
                else
                {
                    return new List<CaseCustMaster>();
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
        public IList<CaseCustMaster> GetDetailQueryList(NewCaseCustCondition model, int pageNum, string strSortExpression, string strSortDirection)
        {
            try
            {
                base.PageIndex = pageNum;

                #region 拼接查詢條件
                string sqlWhere = "";

                // 主檔主鍵
                if (!string.IsNullOrEmpty(model.NewID.ToString()))
                {
                    sqlWhere += @" CaseCustDetails.CaseCustMasterID = @CaseCustMasterID";
                    base.Parameter.Add(new CommandParameter("@CaseCustMasterID", model.NewID));
                }
                #endregion

                #region sql
                StringBuilder sql = new StringBuilder();

                sql.Append(@"
SELECT 
CaseCustDetails.RowNum
    --,CaseCustOutputF1.CUST_ID_NO as CustIdNo    --統一編號
    --,CaseCustOutputF1.ACCT_NO as CustAccount -- 帳號
    ,isnull(CaseCustDetails.CustIdNo,' ') as CustIdNo   --統一編號
    ,CaseCustOutputF1.CUST_ID_NO as CustIdNo1
    ,(CaseCustDetails.QueryType+'-'+isnull(SUBSTRING(CaseCustOutputF1.CUST_ID_NO,1,10),' ')) as CustIdNo2
    ,case when isnull(CaseCustOutputF1.ACCT_NO,'')=' ' then isNull(CaseCustDetails.CustAccount,' ') else isNull(CaseCustOutputF1.ACCT_NO,' ') end as CustAccount   -- 帳號
    ,CASE Rtrim(Ltrim([QueryType]))  
         WHEN '1' THEN '1.基本資料(ID'  
         WHEN '2' THEN '2.存款明細(ID)'  
         WHEN '3' THEN '3.基本資料(帳號)'  
         WHEN '4' THEN '4.存款明細(帳號)'  
		 WHEN '5' THEN '5.保管箱'  
         ELSE ' '  
      END  as SearchProgram --查詢項目
    ,case when isnull(CaseCustDetails.TransactionFlag,'')='Y'
            then CASE WHEN LEN(CaseCustDetails.QDateS)=8 
                    THEN substring(CaseCustDetails.QDateS,1, 4) +'/'
                    + substring(CaseCustDetails.QDateS,5,2)  +'/' 
                    + substring(CaseCustDetails.QDateS, 7,2) ELSE '' END
            else '' end as QDateS --查詢期間(起)
    ,case when isnull(CaseCustDetails.TransactionFlag,'')='Y'
            then CASE WHEN LEN(CaseCustDetails.QDateE)=8
                    THEN substring(CaseCustDetails.QDateE,1, 4) +'/'
                    + substring(CaseCustDetails.QDateE,5,2)  +'/'
                    + substring(CaseCustDetails.QDateE, 7,2) ELSE '' END
            else '' end as QDateE  --查詢期間(迄)
    , isNull(CaseCustOutputF1.ACCT_NO,' ') as GoFileNo     --存款帳號
    , isNull(CaseCustDetails.SBoxNo,' ') as SBoxNo
    , isnull(CaseCustDetails.HTGQryMessage,'')+isnull(CaseCustDetails.RFDMQryMessage,'')+isnull(CaseCustDetails.SboxQryMessage,'')+isnull(CaseCustDetails.ErrorMessage,'') as RFDMQryMessage
    , CaseCustDetails.CaseCustMasterId as NewID
    , CaseCustDetails.DetailsId as VersionKey
    , CASE
        WHEN LEN(CaseCustOutputF1.OPEN_DATE) > 0
        THEN  right('000'+CONVERT(nvarchar(3),CONVERT(int, substring(CaseCustOutputF1.OPEN_DATE,1,4))-1911),3)+'/'
        + substring(CaseCustOutputF1.OPEN_DATE,5, 2) +'/'
        +substring(CaseCustOutputF1.OPEN_DATE,7, 2) ELSE null END AS Govement    --開戶日   
    , CASE
        WHEN LEN(CaseCustOutputF1.LAST_DATE) > 0
        THEN  
						CASE
							WHEN substring(CaseCustOutputF1.LAST_DATE,7,4) = '9999'
							THEN '999/99/99'
							ELSE
								right('000'+CONVERT(nvarchar(3),CONVERT(int, substring(CaseCustOutputF1.LAST_DATE,7,4))-1911),3)+'/'
									+ substring(CaseCustOutputF1.LAST_DATE,4, 2) +'/'
									+substring(CaseCustOutputF1.LAST_DATE,1, 2)
						END
        ELSE null END as LimitDate    --最後交易日
    ,PARMCode.CodeDesc CaseStatusName
from
(
	SELECT 
	* FROM
	(
		SELECT 
		ROW_NUMBER() OVER ( ORDER BY QueryType,CustIdno,CustAccount) AS RowNum
		,*
		from  CaseCustDetails
		WHERE   " + sqlWhere + @"
	) as CaseCustDetails ");

                sql.Append(@" WHERE  RowNum > " + PageSize * (pageNum - 1) + " AND RowNum < " + ((PageSize * pageNum) + 1));

                sql.Append(@" )CaseCustDetails
                            LEFT JOIN PARMCode
                                ON CaseCustDetails.Status = PARMCode.CodeNo
                                AND PARMCode.CodeType = 'CaseCustStatus'
                            LEFT JOIN CaseCustOutputF1
                                ON CaseCustOutputF1.DetailsId = CaseCustDetails.DetailsId
ORDER BY RowNum,QueryType,CustIdno,CustAccount
                            ");
                #endregion

                // 資料總筆數
                string sqlCount = @"
                                select
                                    count(0)
                                from  CaseCustDetails
                                left join PARMCode
                                on CaseCustDetails.Status = PARMCode.CodeNo
                                and PARMCode.CodeType = 'CaseCustStatus'
                                WHERE " + sqlWhere;
                base.DataRecords = int.Parse(base.ExecuteScalar(sqlCount).ToString());

                // 查詢清單資料
                IList<CaseCustMaster> _ilsit = base.SearchList<CaseCustMaster>(sql.ToString());

                if (_ilsit != null)
                {
                    foreach (var item in _ilsit)
                    {
                        if (!string.IsNullOrEmpty(item.CustIdNo.ToString()) && item.SearchProgram == "5.保管箱")
                        {
                            DataTable dt = GetSBoxNo(item.NewID.ToString(), item.VersionKey.ToString(),item.CustIdNo.ToString());
                            if (dt.Rows.Count > 0)
                            {
                                item.SBoxNo = dt.Rows[0]["BOX_NO"].ToString();
                                item.Govement = dt.Rows[0]["RENT_START"].ToString();
                                item.LimitDate = dt.Rows[0]["RENT_END"].ToString();
                            }
                        }
                    }
                    return _ilsit;
                }
                else
                {
                    return new List<CaseCustMaster>();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public DataTable GetSBoxNo(string CaseCustMasterId, string DetailsId, string CUST_ID_NO)
        {
            string sqlSelect = @" select * from CaseCustOutputF3 where  MasterId=@CaseCustMasterId and DetailsId=@DetailsId and CUST_ID_NO=@CUST_ID_NO ";

            // 清空容器
            base.Parameter.Clear();
            base.Parameter.Add(new CommandParameter("@CaseCustMasterId", CaseCustMasterId));
            base.Parameter.Add(new CommandParameter("@DetailsId", DetailsId));
            base.Parameter.Add(new CommandParameter("@CUST_ID_NO", CUST_ID_NO));
            return base.Search(sqlSelect);
        }
        /// <summary>
        /// 審核完成
        /// </summary>
        /// <param name="pDocNo">主檔主鍵字串</param>
        /// <param name="LogonUser">登錄人員ID</param>
        /// <param name="UserName">登錄人員姓名</param>
        /// <returns></returns>
        public bool AuditFinishs(string strKey, string flag, string LogonUser, string UserName)
        {
            IDbConnection dbConnection = OpenConnection();
            IDbTransaction dbTrans = null;
            try
            {
                using (dbConnection)
                {
                    dbTrans = dbConnection.BeginTransaction();

                    string[] arrayKey = strKey.Split(',');

                    string Sql = "";
                    base.Parameter.Clear();

                    // 主管放行[審核]
                    if (flag == "1")
                    {
                        for (int i = 0; i < arrayKey.Length; i++)
                        {
                            Sql += @" 
                            update CaseCustMaster
                            set AuditStatus = 'Y'
                              
                                ,CearanceUserID = @CearanceUserID
                                ,CearanceUserName = @CearanceUserName
                                ,ModifiedDate = getdate()
                                ,ModifiedUser = @ModifiedUser
                            where NewID = @NewID" + i.ToString() + "; ";
                            base.Parameter.Add(new CommandParameter("@NewID" + i.ToString(), arrayKey[i]));
                        }
                    }
                    else
                    {
                        // 主管放行明細頁面[審核完成]
                        Sql = @" 
                            update CaseCustMaster
                            set AuditStatus = 'Y'
                              
                                ,CearanceUserID = @CearanceUserID
                                ,CearanceUserName = @CearanceUserName
                                ,ModifiedDate = getdate()
                                ,ModifiedUser = @ModifiedUser
                            where NewID = @NewID ";
                        base.Parameter.Add(new CommandParameter("@NewID", arrayKey[0]));
                    }

                    base.Parameter.Add(new CommandParameter("@CearanceUserID", LogonUser));
                    base.Parameter.Add(new CommandParameter("@CearanceUserName", UserName));
                    base.Parameter.Add(new CommandParameter("@ModifiedUser", LogonUser));

                    int result = base.ExecuteNonQuery(Sql);

                    // 如果執行結果=資料筆數，就返回true，否則返回false
                    if (result == arrayKey.Length)
                    {
                        dbTrans.Commit();
                        return true;
                    }
                    else
                    {
                        dbTrans.Rollback();
                        return false;
                    }
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
                            update CaseCustMaster
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
        public CaseCustMaster GetReturnFilesByPk(string strPk)
        {

            string sqlSlect = @" SELECT  ROpenFileName ,
                                          RFileTransactionFileName,
                                          Version
                                  FROM    CaseCustMaster
                                  WHERE   NewID=@NewID";

            base.Parameter.Clear();

            base.Parameter.Add(new CommandParameter("@NewID", strPk));

            return base.SearchList<CaseCustMaster>(sqlSlect)[0];
        }

        public CaseCustMaster GetReceiveZipByPk(string strPk)
        {

            string sqlSlect = @" SELECT  QFileName3 ,Version
                                  FROM    CaseCustMaster
                                  WHERE   NewID=@NewID";

            base.Parameter.Clear();

            base.Parameter.Add(new CommandParameter("@NewID", strPk));

            return base.SearchList<CaseCustMaster>(sqlSlect)[0];
        }

        /// <summary>
        /// 根據主鍵取得回文檔案名稱
        /// </summary>
        /// <param name="strPk">主鍵</param>
        /// <returns></returns>
        public NewCaseCustCondition GetCaseCustQuery(string strPk)
        {

            string sqlSlect = @" SELECT  ROpenFileName ,
                                          RFileTransactionFileName,
                                          Version,DocNo,QFileName,QFileName2,QFileName3,AuditStatus,NewID  FROM    CaseCustMaster
                                  WHERE   NewID=@NewID";

            base.Parameter.Clear();

            base.Parameter.Add(new CommandParameter("@NewID", strPk));

            return base.SearchList<NewCaseCustCondition>(sqlSlect)[0];
        }

        /// <summary>
        /// 查詢勾選案件的回文
        /// </summary>
        /// <param name="strDocNo">案件編號</param>
        /// <returns></returns>
        public IList<CaseCustMaster> GedReturnFile(string strNewID)
        {

            string sqlSelect = @"  SELECT  DocNo + '_'+ CONVERT(NVARCHAR, Version)  + '.pdf'  AS PDFFileName
                                    FROM    CaseCustMaster
                                    WHERE   @NewID LIKE '%' + CONVERT(NVARCHAR(50), [NewID]) + '%'
                                    ORDER BY DocNo";

            base.Parameter.Clear();

            base.Parameter.Add(new CommandParameter("@NewID", strNewID));

            return base.SearchList<CaseCustMaster>(sqlSelect);
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
