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
    public class CaseCustManagerBIZ : CommonBIZ
    {
        CaseCustCommonBIZ pCaseCustCommonBIZ = new CaseCustCommonBIZ();


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
                                        WHERE CaseCustQueryVersion.CaseCustNewID = CaseCustQuery.NewID 
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
	,CaseCustQuery.NewID
	,CaseCustQuery.Version
FROM 
(
	select *
	from (
		select 
			ROW_NUMBER() OVER ( ORDER BY CaseCustQuery.DocNo,CaseCustQuery.Version  ) AS RowNum
			,CaseCustQuery.DocNo,CaseCustQuery.Version,CaseCustQuery.NewID
		from CaseCustQuery
		inner join  CaseCustQueryVersion
		on CaseCustQueryVersion.CaseCustNewID = CaseCustQuery.NewID
		WHERE (CaseCustQuery.Status = '03' or CaseCustQuery.Status = '07') 
		and isnull(CaseCustQuery.UploadStatus,'') <> 'Y'  " + sqlWhere + @"
		GROUP BY CaseCustQuery.DocNo,CaseCustQuery.Version,CaseCustQuery.NewID
	) as tabletemp
");

                // 判斷是否分頁
                sql.Append(@" WHERE  RowNum > " + PageSize * (pageNum - 1) + " AND RowNum < " + ((PageSize * pageNum) + 1));

                sql.Append(@" 
)RESULT
INNER JOIN CaseCustQuery
	ON RESULT.NewID = CaseCustQuery.NewID
INNER JOIN CaseCustQueryVersion
	ON CaseCustQueryVersion.CaseCustNewID = CaseCustQuery.NewID " + sqlVersionWhere + @"
left join PARMCode
	ON CaseCustQuery.Status = PARMCode.CodeNo
	AND CodeType = 'CaseCustStatus'
ORDER BY CaseCustQuery.DocNo,CaseCustQuery.Version, CaseCustQueryVersion.CustIdNo");

                #endregion

                // 資料總筆數
                string sqlCount = @"
                            SELECT count(*)
                            FROM
                            (
                                select
                                    CaseCustQuery.NewID
                                from CaseCustQuery
                                inner join  CaseCustQueryVersion
                                on CaseCustQueryVersion.CaseCustNewID = CaseCustQuery.NewID
                                left join PARMCode
                                on CaseCustQuery.Status = PARMCode.CodeNo
                                and CodeType = 'CaseCustStatus'
                                WHERE (CaseCustQuery.Status = '03' or CaseCustQuery.Status = '07') 
                                and isnull(CaseCustQuery.UploadStatus,'') <> 'Y' " + sqlWhere + " GROUP BY CaseCustQuery.NewID)Result";
                base.DataRecords = int.Parse(base.ExecuteScalar(sqlCount).ToString());

                // 查詢清單資料
                IList<CaseCustQuery> _ilsit = base.SearchList<CaseCustQuery>(sql.ToString());

                if (_ilsit != null)
                {
                    for (int i = 0; i < _ilsit.Count; i++)
                    {
                        _ilsit[i].LimitDate = pCaseCustCommonBIZ.DateAdd(_ilsit[i].RecvDate, 7);
                        _ilsit[i].RecvDate5 = pCaseCustCommonBIZ.DateAdd(_ilsit[i].RecvDate, 5);
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
        public IList<CaseCustQuery> GetDetailQueryList(CaseCustCondition model, int pageNum, string strSortExpression, string strSortDirection)
        {
            try
            {
                base.PageIndex = pageNum;

                #region 拼接查詢條件
                string sqlWhere = "";

                // 主檔主鍵
                if (!string.IsNullOrEmpty(model.NewID.ToString()))
                {
                    sqlWhere += @" CaseCustQueryVersion.CaseCustNewID = @CaseCustNewID";
                    base.Parameter.Add(new CommandParameter("@CaseCustNewID", model.NewID));
                }
                #endregion

                #region sql
                StringBuilder sql = new StringBuilder();

                sql.Append(@"
SELECT 
CaseCustQueryVersion.RowNum
    ,CaseCustQueryVersion.CustIdNo    --統一編號
    ,case when isnull(CaseCustQueryVersion.OpenFlag,'')='Y' then '1.基本資料<br />' 　
        else '' end
    +case when isnull(CaseCustQueryVersion.TransactionFlag,'')='Y' 
        then '2.存款明細'　else '' end as SearchProgram --查詢項目
    ,case when isnull(CaseCustQueryVersion.TransactionFlag,'')='Y'
            then CASE WHEN LEN(CaseCustQueryVersion.QDateS)=8 
                    THEN substring(CaseCustQueryVersion.QDateS,1, 4) +'/'
                    + substring(CaseCustQueryVersion.QDateS,5,2)  +'/' 
                    + substring(CaseCustQueryVersion.QDateS, 7,2) ELSE '' END
            else '' end as QDateS --查詢期間(起)
    ,case when isnull(CaseCustQueryVersion.TransactionFlag,'')='Y'
            then CASE WHEN LEN(CaseCustQueryVersion.QDateE)=8
                    THEN substring(CaseCustQueryVersion.QDateE,1, 4) +'/'
                    + substring(CaseCustQueryVersion.QDateE,5,2)  +'/'
                    + substring(CaseCustQueryVersion.QDateE, 7,2) ELSE '' END
            else '' end as QDateE  --查詢期間(迄)
    , BOPS000401Recv.ACCT_NO as GoFileNo     --存款帳號
    , CASE
        WHEN LEN(BOPS000401Recv.OPEN_DATE) > 0
        THEN  right('000'+CONVERT(nvarchar(3),CONVERT(int, substring(BOPS000401Recv.OPEN_DATE,7,4))-1911),3)+'/'
        + substring(BOPS000401Recv.OPEN_DATE,4, 2) +'/'
        +substring(BOPS000401Recv.OPEN_DATE,1, 2) ELSE null END AS Govement    --開戶日   
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
        ELSE null END as LimitDate    --最後交易日
    ,PARMCode.CodeDesc CaseStatusName
from
(
	SELECT 
	* FROM
	(
		SELECT 
		ROW_NUMBER() OVER ( ORDER BY CustIDno,IdNo ) AS RowNum
		,*
		from  CaseCustQueryVersion
		WHERE   " + sqlWhere + @"
	) as CaseCustQueryVersion ");

                sql.Append(@" WHERE  RowNum > " + PageSize * (pageNum - 1) + " AND RowNum < " + ((PageSize * pageNum) + 1));

                sql.Append(@" )CaseCustQueryVersion
                            LEFT JOIN PARMCode
                                ON CaseCustQueryVersion.Status = PARMCode.CodeNo
                                AND PARMCode.CodeType = 'CaseCustStatus'
                            LEFT JOIN BOPS000401Recv
                                ON BOPS000401Recv.VersionNewID = CaseCustQueryVersion.NewID
ORDER BY CaseCustQueryVersion.CustIDno,IdNo 
                            ");
                #endregion

                // 資料總筆數
                string sqlCount = @"
                                select
                                    count(0)
                                from  CaseCustQueryVersion
                                left join PARMCode
                                on CaseCustQueryVersion.Status = PARMCode.CodeNo
                                and PARMCode.CodeType = 'CaseCustStatus'
                                WHERE " + sqlWhere;
                base.DataRecords = int.Parse(base.ExecuteScalar(sqlCount).ToString());

                // 查詢清單資料
                IList<CaseCustQuery> _ilsit = base.SearchList<CaseCustQuery>(sql.ToString());

                if (_ilsit != null)
                {
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
                            update CaseCustQuery
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
                            update CaseCustQuery
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
        public CaseCustCondition GetCaseCustQuery(string strPk)
        {

            string sqlSlect = @" SELECT  ROpenFileName ,
                                          RFileTransactionFileName,
                                          Version,DocNo,QFileName,QFileName2,AuditStatus,NewID
                                  FROM    CaseCustQuery
                                  WHERE   NewID=@NewID";

            base.Parameter.Clear();

            base.Parameter.Add(new CommandParameter("@NewID", strPk));

            return base.SearchList<CaseCustCondition>(sqlSlect)[0];
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
