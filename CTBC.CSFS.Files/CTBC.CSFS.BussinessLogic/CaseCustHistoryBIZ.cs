using CTBC.CSFS.Models;
using CTBC.CSFS.Pattern;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Data;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.HSSF.Util;
using NPOI.HSSF.UserModel;
using CTBC.FrameWork.Platform;

namespace CTBC.CSFS.BussinessLogic
{
    public class CaseCustHistoryBIZ : CommonBIZ
    {
        CaseCustCommonBIZ pCaseCustCommonBIZ = new CaseCustCommonBIZ();

        CaseCustBIZ pCaseCustBIZ = new CaseCustBIZ();

        /// <summary>
        /// 歷史記錄查詢與重送清單資料
        /// </summary>
        /// <param name="model"></param>
        /// <param name="pageNum"></param>
        /// <returns></returns>
        public IList<CaseCustQuery> GetHistoryQueryList(CaseCustCondition model, int pageNum, string strSortExpression, string strSortDirection)
        {
            try
            {
                base.PageIndex = pageNum;

                #region 拼接查詢條件
                string sqlWhere = "";

                #region 待補充欄位

                // 來文機關 FileGovenment
                if (!string.IsNullOrEmpty(model.FileGovenment))
                {
                    sqlWhere += @" AND CaseCustQuery.LetterDeptName like @LetterDeptName";
                    base.Parameter.Add(new CommandParameter("@LetterDeptName", "%" + model.FileGovenment + "%"));
                }

                // 來文字號 FileNo
                if (!string.IsNullOrEmpty(model.FileNo))
                {
                    sqlWhere += @" AND CaseCustQuery.LetterNo like @LetterNo";
                    base.Parameter.Add(new CommandParameter("@LetterNo", "%" + model.FileNo + "%"));
                }

                // 回文字號  GoFileNo
                if (!string.IsNullOrEmpty(model.GoFileNo))
                {
                    sqlWhere += @" AND CaseCustQuery.MessageNo like @MessageNo";
                    base.Parameter.Add(new CommandParameter("@MessageNo", "%" + model.GoFileNo + "%"));
                }

                #endregion

                // 案件編號DocNo
                if (!string.IsNullOrEmpty(model.DocNo))
                {
                    sqlWhere += @" AND CaseCustQuery.DocNo like @DocNo ";
                    base.Parameter.Add(new CommandParameter("@DocNo", "%" + model.DocNo + "%"));
                }

                // 結案日期S FinishDateStart
                if (!string.IsNullOrEmpty(model.FinishDateStart))
                {
                    sqlWhere += @" AND convert(nvarchar(10),CaseCustQuery.CloseDate,111) >= @FinishDateStart ";
                    base.Parameter.Add(new CommandParameter("@FinishDateStart", model.FinishDateStart));
                }

                // 結案日期E FinishDateEnd
                if (!string.IsNullOrEmpty(model.FinishDateEnd))
                {
                    sqlWhere += @" AND convert(nvarchar(10),CaseCustQuery.CloseDate,111) <= @FinishDateEnd ";
                    base.Parameter.Add(new CommandParameter("@FinishDateEnd", model.FinishDateEnd));
                }

                // 案件狀態 CaseStatus
                if (!string.IsNullOrEmpty(model.CaseStatus))
                {
                    // 格式錯誤
                   if (model.CaseStatus == "99")
                    {
                        sqlWhere += @" AND CaseCustQuery.ImportFormFlag ='Y'";
                    }
                    else
                    {
                        sqlWhere += @" AND CaseCustQuery.Status = @CaseStatus ";
                        base.Parameter.Add(new CommandParameter("@CaseStatus", model.CaseStatus));
                    }
                }

                // 處理方式 ProcessingMethod
                if (!string.IsNullOrEmpty(model.ProcessingMethod))
                {
                    if (model.ProcessingMethod == "01")
                    {
                        // 未處理 : 除了"成功+重查成功+已上傳"之外的都算是
                        sqlWhere += @" AND NOT CaseCustQuery.Status in ('03','07','66') ";
                    }
                    else if (model.ProcessingMethod == "66")
                    {
                        // 已處理 : 成功+重查成功
                       sqlWhere += @" AND CaseCustQuery.Status in ('03','07','66') 
                                      AND (CaseCustQuery.UploadStatus = 'N' OR  CaseCustQuery.UploadStatus IS NULL)";
                    }
                    else
                    {
                        // 已上傳
                        sqlWhere += @" AND CaseCustQuery.UploadStatus IS NOT NULL AND CaseCustQuery.UploadStatus = 'Y' ";
                    }
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

                // 統一編號   CustIdNo
                if (!string.IsNullOrEmpty(model.CustIdNo))
                {
                    sqlWhere += @" AND EXISTS
                                    (
                                      SELECT NewID FROM CaseCustQueryVersion
                                       WHERE CaseCustQueryVersion.CaseCustNewID = CaseCustQuery.NewID
                                        and CaseCustQueryVersion.CustIdNo like @CustIdNo 
                                    )
							     ";

                    base.Parameter.Add(new CommandParameter("@CustIdNo", "%" + model.CustIdNo + "%"));
                }

                #endregion

                #region sql
                StringBuilder sql = new StringBuilder();

                sql.Append(@"
                             select
                            	RowNum
                            	,DocNo　
                            	,FileNo
                            	,Govement
                            	,GoFileNo
                                ,CaseStatus
                            	,CaseStatusName 
                            	,Result
                                ,FinishDate
                            	,RecvDate
                                ,LimitDate
                                ,NewID
                                ,Version
                                ,CountDocNo
                                ,ShowDocNo
                                ,QFileName
                                ,QFileName2
,Repeat
                            from (
                            	select 
                            		ROW_NUMBER() OVER ( ORDER BY " + strSortExpression + " " + strSortDirection + @" ) AS
RowNum
                            		,CaseCustQuery.DocNo    --案件編碼
                                    ,CaseCustQuery.LetterNo as FileNo    --來文字號
                                    ,CaseCustQuery.LetterDeptName as Govement    --來文機關
                                    ,CaseCustQuery.MessageNo as GoFileNo    --回文字號
                                    ,CaseCustQuery.Status as CaseStatus
                                    ,CASE WHEN ISNULL(ImportFormFlag,'')='Y'  THEN '格式錯誤' ELSE PARMCode.CodeDesc END as CaseStatusName
,CASE WHEN ISNULL(CaseCustQueryHistory.SUMDocNo,'') <> ''  THEN '是' ELSE '否' END as Repeat 
                                	,case when isnull (CaseCustQuery.Status,'') = '66'
                                		then N'已處理<br />'+ CaseCustQuery.ProcessingMode
                                		else case when isnull(UploadStatus,'') = 'Y' then '已上傳' else '' end
                                		end as Result    --處理方式
                                    ,convert(nvarchar(10),CaseCustQuery.CloseDate,111) as FinishDate    --結案日期
                                    ,convert(nvarchar(10),CaseCustQuery.RecvDate,111) as RecvDate
                                	,'' as LimitDate     --限辦日期(T+7)
                                    ,CaseCustQuery.NewID
                                	,CaseCustQuery.Version --當前案件的版本號
                                	,COUNTCaseCustQuery.CountDocNo --案件編號下資料筆數
                                	,CaseCustQuery.DocNo 
                                    + case when CaseCustQuery.Version = 0 then '' 
                                         else '-' + CONVERT(nvarchar(10), CaseCustQuery.Version) end  
                                    as ShowDocNo   --顯示在清單中案件編碼
                                    ,QFileName
                                    ,QFileName2
                                from CaseCustQuery
LEFT JOIN 
(
	SELECT DocNo, COUNT(DocNo) AS SUMDocNo FROM CaseCustQueryHistory
	GROUP BY DocNo
)CaseCustQueryHistory
	ON CaseCustQueryHistory.DocNo = CaseCustQuery.DocNo
                                LEFT JOIN PARMCode
                                    on CaseCustQuery.Status = PARMCode.CodeNo
                                    and PARMCode.CodeType = 'CaseCustStatus'
                                LEFT JOIN 
                                (
                                 SELECT DocNo, COUNT(DocNo) AS CountDocNo FROM CaseCustQuery GROUP BY DocNo
                                ) COUNTCaseCustQuery
                                    ON CaseCustQuery.DocNo = COUNTCaseCustQuery.DocNo
                                WHERE 1=1 " + sqlWhere + @"
                            ) as tabletemp ");

                // 判斷是否分頁
                sql.Append(@" WHERE  RowNum > " + PageSize * (pageNum - 1)
                                   + " AND RowNum < " + ((PageSize * pageNum) + 1));

                #endregion

                // 資料總筆數
                string sqlCount = @"
                                select
                                    count(0)
                                from CaseCustQuery
                                left join PARMCode
                                on CaseCustQuery.Status = PARMCode.CodeNo
                                and CodeType = 'CaseCustStatus'
                                LEFT JOIN 
                                (
                                 SELECT DocNo, COUNT(DocNo) AS COUNTDocNo FROM CaseCustQuery GROUP BY DocNo
                                ) COUNTCaseCustQuery
                                ON CaseCustQuery.DocNo = COUNTCaseCustQuery.DocNo
                                WHERE 1=1  " + sqlWhere;
                base.DataRecords = int.Parse(base.ExecuteScalar(sqlCount).ToString());

                // 查詢清單資料
                IList<CaseCustQuery> _ilsit = base.SearchList<CaseCustQuery>(sql.ToString());

                if (_ilsit != null)
                {
                    // 處理日期
                    for (int i = 0; i < _ilsit.Count; i++)
                    {
                        _ilsit[i].LimitDate = pCaseCustCommonBIZ.DateAdd(_ilsit[i].RecvDate, 7);
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
        ///  查詢匯出資料
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public DataTable GetHistoryQueryDdata(string strNewID)
        {
            try
            {
                #region 拼接查詢條件
                string sqlWhere = "";
                string[] NewIDArray = strNewID.Split(',');
                for (int i = 0; i < NewIDArray.Length; i++)
                {
                    sqlWhere += "'" + NewIDArray[i] + "',";
                }
                sqlWhere = sqlWhere.Substring(0, sqlWhere.Length - 1);
                #endregion

                #region sql
                StringBuilder sql = new StringBuilder();

                sql.Append(@"
                            	select 
                                	ROW_NUMBER() OVER ( ORDER BY CaseCustQuery.DocNo, CaseCustQuery.Version, CaseCustQueryVersion.CustIdNo asc ) AS RowNum
                                    ,CASE 
	                                    WHEN CaseCustQuery.Version = 0 THEN CaseCustQuery.DocNo
	                                    ELSE CaseCustQuery.DocNo +'-'+ CONVERT(nvarchar, CaseCustQuery.Version) 
                                    END DocNo    --案件編碼 
                                    ,CaseCustQuery.LetterNo as FileNo    --來文字號 
                                    ,CaseCustQuery.LetterDeptName as Govement    --來文機關 
                                    ,CaseCustQuery.MessageNo as GoFileNo    --回文字號
                                    ,case when  LEN(CaseCustQueryVersion.CustIdNo) > 3
                                          then SUBSTRING(CaseCustQueryVersion.CustIdNo,1,LEN(CaseCustQueryVersion.CustIdNo) -4) +  '***'     
	                                   	  + SUBSTRING(CaseCustQueryVersion.CustIdNo,LEN(CaseCustQueryVersion.CustIdNo),1)
                                          else CaseCustQueryVersion.CustIdNo
                                          end as CustIdNo --統一編號
                                    ,isnull(CaseCustQuery.CearanceUserID,'') + ' ' + isnull(CaseCustQuery.CearanceUserName,'') as  SearchProgram   -- 放行主管
                                    ,case when isnull (CaseCustQuery.Status,'') = '66'
	                                   then N'已處理\n'+ CaseCustQuery.ProcessingMode
	                                   else case when isnull(UploadStatus,'') = 'Y' then '已上傳' else '' end
	                                   end as Result    --處理方式
                                    ,convert(nvarchar(10),CaseCustQuery.CloseDate,111) as FinishDate    --結案日期
                                from CaseCustQuery
                                left join CaseCustQueryVersion
                                on CaseCustQueryVersion.CaseCustNewID = CaseCustQuery.NewID
                                left join PARMCode
                                on CaseCustQueryVersion.Status = PARMCode.CodeNo
                                and CodeType = 'CaseCustStatus'
                                WHERE 1=1  
                                and  CaseCustQuery.NewID in(" + sqlWhere + ")  ");
                #endregion

                return base.Search(sql.ToString());
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 匯出Excel
        /// </summary>
        /// <param name="model">查詢條件</param>
        /// <returns></returns>
        public MemoryStream ExportExcel(string strNewID)
        {
            IWorkbook workbook = new HSSFWorkbook();
            ISheet sheet = workbook.CreateSheet("歷史紀錄查詢與重送");

            // 查詢要匯出資料
            DataTable dt = GetHistoryQueryDdata(strNewID);

            #region  style
            // 標題
            ICellStyle styleHead12 = workbook.CreateCellStyle();
            IFont font12 = workbook.CreateFont();
            font12.FontHeightInPoints = 10;
            font12.FontName = "新細明體";
            styleHead12.FillPattern = FillPattern.SolidForeground;
            styleHead12.FillForegroundColor = HSSFColor.White.Index;
            styleHead12.BorderTop = BorderStyle.None;
            styleHead12.BorderLeft = BorderStyle.None;
            styleHead12.BorderRight = BorderStyle.None;
            styleHead12.BorderBottom = BorderStyle.None;
            styleHead12.WrapText = true;
            styleHead12.Alignment = HorizontalAlignment.Center;
            styleHead12.VerticalAlignment = VerticalAlignment.Center;
            styleHead12.SetFont(font12);

            // 居左
            ICellStyle styleHead10 = workbook.CreateCellStyle();
            IFont font10 = workbook.CreateFont();
            font10.FontHeightInPoints = 10;
            font10.FontName = "新細明體";
            styleHead10.FillPattern = FillPattern.SolidForeground;
            styleHead10.FillForegroundColor = HSSFColor.White.Index;
            styleHead10.BorderTop = BorderStyle.Thin;
            styleHead10.BorderLeft = BorderStyle.Thin;
            styleHead10.BorderRight = BorderStyle.Thin;
            styleHead10.BorderBottom = BorderStyle.Thin;
            styleHead10.WrapText = true;
            styleHead10.Alignment = HorizontalAlignment.Left;// 水平位置
            styleHead10.VerticalAlignment = VerticalAlignment.Center;
            styleHead10.SetFont(font10);

            // 居中
            ICellStyle styleCenter = workbook.CreateCellStyle();
            styleCenter.FillPattern = FillPattern.SolidForeground;
            styleCenter.FillForegroundColor = HSSFColor.White.Index;
            styleCenter.BorderTop = BorderStyle.Thin;
            styleCenter.BorderLeft = BorderStyle.Thin;
            styleCenter.BorderRight = BorderStyle.Thin;
            styleCenter.BorderBottom = BorderStyle.Thin;
            styleCenter.WrapText = true;
            styleCenter.Alignment = HorizontalAlignment.Center;// 水平位置
            styleCenter.VerticalAlignment = VerticalAlignment.Center;
            styleCenter.SetFont(font10);
            #endregion

            #region 設置單元格Width
            sheet.SetColumnWidth(0, 100 * 20);
            sheet.SetColumnWidth(1, 100 * 40);
            sheet.SetColumnWidth(2, 100 * 30);
            sheet.SetColumnWidth(3, 100 * 30);
            sheet.SetColumnWidth(4, 100 * 30);
            sheet.SetColumnWidth(5, 100 * 40);
            sheet.SetColumnWidth(6, 100 * 50);
            sheet.SetColumnWidth(7, 100 * 35);
            sheet.SetColumnWidth(8, 100 * 35);
            #endregion

            #region title
            //* line0
            SetExcelCell(sheet, 0, 0, styleHead12, "歷史紀錄查詢與重送");
            sheet.AddMergedRegion(new CellRangeAddress(0, 0, 0, 8));

            //* line2
            SetExcelCell(sheet, 2, 0, styleCenter, "序號");
            sheet.AddMergedRegion(new CellRangeAddress(2, 2, 0, 0));
            SetExcelCell(sheet, 2, 1, styleCenter, "案件編號");
            sheet.AddMergedRegion(new CellRangeAddress(2, 2, 1, 1));
            SetExcelCell(sheet, 2, 2, styleCenter, "來文字號");
            sheet.AddMergedRegion(new CellRangeAddress(2, 2, 2, 2));
            SetExcelCell(sheet, 2, 3, styleCenter, "來文機關");
            sheet.AddMergedRegion(new CellRangeAddress(2, 2, 3, 3));
            SetExcelCell(sheet, 2, 4, styleCenter, "回文字號");
            sheet.AddMergedRegion(new CellRangeAddress(2, 2, 4, 4));
            SetExcelCell(sheet, 2, 5, styleCenter, "統一編號");
            sheet.AddMergedRegion(new CellRangeAddress(2, 2, 5, 5));
            SetExcelCell(sheet, 2, 6, styleCenter, "放行主管");
            sheet.AddMergedRegion(new CellRangeAddress(2, 2, 6, 6));
            SetExcelCell(sheet, 2, 7, styleCenter, "處理方式");
            sheet.AddMergedRegion(new CellRangeAddress(2, 2, 7, 7));
            SetExcelCell(sheet, 2, 8, styleCenter, "結案日期");
            sheet.AddMergedRegion(new CellRangeAddress(2, 2, 8, 8));
            #endregion

            #region body
            
            string DocNo = String.Empty;
            int CountDocNo = 0;

            for (int iRow = 0; iRow < dt.Rows.Count; iRow++)
            {
              if (DocNo != dt.Rows[iRow]["DocNo"].ToString())
              {
                DocNo = dt.Rows[iRow]["DocNo"].ToString();
                CountDocNo++;
              }
              
              for (int iCol = 0; iCol < dt.Columns.Count; iCol++)
              {
                  if (dt.Columns[iCol].ToString() == "RowNum" || dt.Columns[iCol].ToString() == "DocNo" || dt.Columns[iCol].ToString() == "FinishDate" || dt.Columns[iCol].ToString() == "LimitDate" || dt.Columns[iCol].ToString() == "CustIdNo")
                  {
                      // 居中
                      SetExcelCell(sheet, iRow + 3, iCol, styleCenter, dt.Rows[iRow][iCol].ToString());
                  }
                  else
                  {
                      // 居左
                      SetExcelCell(sheet, iRow + 3, iCol, styleHead10, dt.Rows[iRow][iCol].ToString());
                  }
              }
            }

            //* line1
            SetExcelCell(sheet, 1, 7, styleHead12, String.Format("案件總筆數：{0} 筆", CountDocNo));
            sheet.AddMergedRegion(new CellRangeAddress(1, 1, 7, 8));

            #endregion

            MemoryStream ms = new MemoryStream();
            workbook.Write(ms);
            ms.Flush();
            ms.Position = 0;
            workbook = null;
            return ms;
        }

        /// <summary>
        /// 創建單元格
        /// </summary>
        /// <param name="sheet">sheet</param>
        /// <param name="rowNum">行號</param>
        /// <param name="colNum">列號</param>
        /// <param name="style">單元格樣式</param>
        /// <param name="value">單元格值</param>
        /// <returns>返回單元格</returns>
        public ICell SetExcelCell(ISheet sheet, int rowNum, int colNum, ICellStyle style, string value)
        {
            // 創建或得到行
            IRow row = sheet.GetRow(rowNum) ?? sheet.CreateRow(rowNum);

            // 創建或得到單元格
            ICell cell = row.GetCell(colNum) ?? row.CreateCell(colNum);

            // 樣式
            cell.CellStyle = style;

            // 賦值
            cell.SetCellValue(value.Replace("\\n", Environment.NewLine));

            // 返回單元格
            return cell;
        }

        /// <summary>
        /// 強制結案/重查失敗
        /// </summary>
        /// <param name="pDocNo"></param>
        /// <param name="LogonUser">登錄人員ID</param>
        /// <param name="pProcessingMode">要更新的欄位名</param>
        /// <returns></returns>
        public bool EndCase(string strKEY, string LogonUser, string ProcessingMode)
        {
            IDbConnection dbConnection = OpenConnection();
            IDbTransaction dbTrans = null;
            try
            {
                using (dbConnection)
                {
                    dbTrans = dbConnection.BeginTransaction();

                    string[] arrayKey = strKEY.Split(',');

                    string delSql = "";
                    base.Parameter.Clear();
                    for (int i = 0; i < arrayKey.Length; i++)
                    {
                        #region  更新主檔
                        delSql += @" 
                            update CaseCustQuery
                            set 
                                Status = '66'
                                ,ProcessingMode = @ProcessingMode
                                ,CloseDate = getdate()
                                ,ModifiedDate = getdate()
                                ,ModifiedUser = @ModifiedUser
                            where NewID = @NewID" + i.ToString() + "; ";
                        base.Parameter.Add(new CommandParameter("@NewID" + i.ToString(), arrayKey[i]));
                        #endregion

                        #region  更新子檔
                        delSql += @" 
                            update CaseCustQueryVersion
                            set 
                                Status = '66'
                                ,ModifiedDate = getdate()
                                ,ModifiedUser = @ModifiedUser
                            where CaseCustNewID = @CaseCustNewID" + i.ToString() + "; ";
                        base.Parameter.Add(new CommandParameter("@CaseCustNewID" + i.ToString(), arrayKey[i]));
                        #endregion
                    }
                    base.Parameter.Add(new CommandParameter("@ModifiedUser", LogonUser));
                    base.Parameter.Add(new CommandParameter("@ProcessingMode", ProcessingMode));

                    base.ExecuteNonQuery(delSql);

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
        /// 重查檢核
        /// </summary>
        /// <param name="pDocNo">案件編號</param>
        /// <param name="pVersion">版本號</param>
        /// <param name="pCaseStatus">案件狀態</param>
        /// <returns></returns>
        public string SearchAgainCheck(string pDocNo, string pVersion, string pCaseStatus, string pCountDocNo)
        {
            try
            {
                string Msg = "";

                // 將數據塞進DataTable中
                DataTable dt = ChangeDataTable(pDocNo, pVersion, pCaseStatus, pCountDocNo);

                // 根據案件編號篩選重複資料
                DataView dataView = dt.DefaultView;
                DataTable dtDistinct = dataView.ToTable(true, "DocNo");

                for (int i = 0; i < dtDistinct.Rows.Count; i++)
                {
                    // 根據案件編碼取得版本號最大的案件狀態
                    CaseCustQuery PCaseCustQuery = GetStatusByDocNo(dtDistinct.Rows[i]["DocNo"].ToString());

                    string strCaseStatus = PCaseCustQuery.CaseStatus;
                    string strImportFormFlag = PCaseCustQuery.ImportFormFlag;

                    // 03：成功
                    // 04：失敗
                    // 07：重查成功
                    // 08：重查失敗
                    // 66：已處理 / 強制結案" 且格式正確的才可以重查
                    if ((strCaseStatus == "03" || strCaseStatus == "07" || strCaseStatus == "66" || strCaseStatus == "04" || strCaseStatus == "08" || strCaseStatus == "77") && strImportFormFlag == "N")
                    {
                        continue;
                    }
                    else
                    {
                        // 拼接提示內容
                       Msg += dtDistinct.Rows[i]["DocNo"].ToString() + "不可重查\r\n";
                    }

                }

                return Msg;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 查詢案件編號下所有版本的案件狀態
        /// </summary>
        /// <param name="pDocNo">案件編號</param>
        /// <returns></returns>
        public DataTable CheckOneData(string pDocNo)
        {
            try
            {
                // 查詢案件狀態不是'成功''已處理''失敗'的資料
                string sql = @"
                            select 
                        	Status
                        	,Version
                        	,DocNo
                        	,PARMCode.CodeDesc
                        from CaseCustQuery
                        left join PARMCode
                        on CaseCustQuery.Status = PARMCode.CodeNo
                        and CodeType = 'CaseCustStatus'
                        WHERE DocNo = @DocNo
                        and Status <> '03'
                        and Status <> '04'
                        and Status <> '07'
                        and Status <> '08'
                        and Status <> '66'
                        ORDER BY Version asc ";

                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@DocNo", pDocNo));

                return base.Search(sql.ToString());
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 重查
        /// </summary>
        /// <param name="pDocNo">案件編號</param>
        /// <param name="pVersion">版本號</param>
        /// <param name="pCaseStatus">案件狀態</param>
        /// <returns></returns>
        public bool SearchAgain(string pDocNo, string pVersion, string pCaseStatus, string pCountDocNo, User logonUser, string strFilePath)
        {
            // 當前登錄者
            string pUserAccount = logonUser.Account;

            IDbConnection dbConnection = OpenConnection();
            IDbTransaction dbTrans = null;
            try
            {
                using (dbConnection)
                {
                    // 將數據塞進DataTable中
                    DataTable dt = ChangeDataTable(pDocNo, pVersion, pCaseStatus, pCountDocNo);

                    // 根據案件編號篩選重複資料
                    DataView dataView = dt.DefaultView;
                    DataTable dtDistinct = dataView.ToTable(true, "DocNo");

                    // 清空參數
                    base.Parameter.Clear();

                    for (int i = 0; i < dtDistinct.Rows.Count; i++)
                    {
                       DataTable dtCaseCustQuery = GetCaseCustQueryByDocNo(dtDistinct.Rows[i]["DocNo"].ToString());

                        // 取同案件下最大版本號
                        DataRow dR = dtCaseCustQuery.Select("DocNo ='" + dtDistinct.Rows[i]["DocNo"] + "'", "Version DESC")[0];

                        // 案件狀態
                        string strCaseStatus = dR["Status"].ToString();
                        string strCloseDate = dR["CloseDate"].ToString();

                        dbTrans = dbConnection.BeginTransaction();

                        // 01：未處理
                        // 02：拋查中
                        // 03：成功
                        // 04：失敗
                        // 06：重查拋查中
                        // 07：重查成功
                        // 08：重查失敗
                        // 66：已處理 / 強制結案
                        // 成功案件且已結案之重查，所有發查table資料重新insert
                        if ((strCaseStatus == "03" || strCaseStatus == "07" || strCaseStatus == "66") && !string.IsNullOrEmpty(strCloseDate))
                        {
                            Guid pkNewID = Guid.NewGuid();

                            // 成功和已處理案件重查,版本加1
                            InsertCaseCustQuery(dR["DocNo"].ToString(), dR["Version"].ToString(), pUserAccount, pkNewID, dbTrans);

                            // 取得要INSERT 到CaseCustQueryVersion的資料
                            IList<CaseCustQueryVersion> ilistCaseCustQueryVersion = GetCaseCustQueryVersion(dR["DocNo"].ToString(), dR["Version"].ToString(), pUserAccount, pkNewID, dbTrans);

                            // Insert 資料到CaseCustQueryVersion&CaseCustRFDMSend&BOPS067050Send&BOPS060628Send
                            InsertVersionAndSend(ilistCaseCustQueryVersion, logonUser, dbTrans);
                        }
                        else
                        {
                            #region 以原案件編號重查

                            // 更新CaseCustQuery
                            UpdateCaseCustQuery(dR["DocNo"].ToString(), dR["Version"].ToString(), pUserAccount, dbTrans);

                            // 更新CaseCustQueryVersion
                            UpdateCaseCustQueryVersion(dR["DocNo"].ToString(), dR["Version"].ToString(), pUserAccount, dbTrans);

                            //  刪除有關Send &Recv&ApprMsgKey Table 資料
                            DeleteTables(dR["DocNo"].ToString(), dR["Version"].ToString(), dbTrans);

                            //  Insert有關Send &ApprMsgKey Table 資料
                            InsertTables(dR["DocNo"].ToString(), dR["Version"].ToString(), logonUser, dbTrans);

                            #endregion

                            #region 刪除產出的回文資料

                           /*
                            // 回文檔名稱（存款帳戶開戶資料）
                            if (System.IO.File.Exists(strFilePath + dR["ROpenFileName"].ToString()))
                            {
                                FileInfo filedi = new FileInfo(strFilePath + dR["ROpenFileName"].ToString());
                                filedi.Delete();
                            }

                            // 回文檔名稱（存款往來明細資料）
                            if (System.IO.File.Exists(strFilePath + dR["RFileTransactionFileName"].ToString()))
                            {
                                FileInfo filedi = new FileInfo(strFilePath + dR["RFileTransactionFileName"].ToString());
                                filedi.Delete();
                            }

                            // 回文首頁（第一頁PDF）
                            if (System.IO.File.Exists(strFilePath + dR["DocNo"].ToString() + "_" + dR["Version"].ToString() + "_001.pdf"))
                            {
                                FileInfo filedi = new FileInfo(strFilePath + dR["DocNo"].ToString() + "_" + dR["Version"].ToString() + "_001.pdf");
                                filedi.Delete();
                            }

                            // 回文首頁(ALL PDF)
                            if (System.IO.File.Exists(strFilePath + dR["DocNo"].ToString() + "_" + dR["Version"].ToString() + ".pdf"))
                            {
                                FileInfo filedi = new FileInfo(strFilePath + dR["DocNo"].ToString() + "_" + dR["Version"].ToString() + ".pdf");
                                filedi.Delete();
                            }
                            */

                            #endregion
                        }

                        dbTrans.Commit();
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                dbTrans.Rollback();

                return false;
            }
        }

        public DataTable SearchData(string[] arrayDocNo)
        {
            try
            {
                string strDocNo = "'";
                for (int n = 0; n < arrayDocNo.Length; n++)
                {
                    strDocNo = strDocNo + arrayDocNo[n] + "',";
                }
                strDocNo = strDocNo.Substring(0, strDocNo.Length - 1);

                string sql = @"
                            select 
                                max(Version)
                                ,DocNo
                            from  CaseCustQueryVersion
                            where DocNo in (" + strDocNo + @")
                            group by DocNo ";

                return base.Search(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 將數據塞進DataTable中
        /// </summary>
        /// <param name="pDocNo"></param>
        /// <param name="pVersion"></param>
        /// <param name="pCaseStatus"></param>
        /// <param name="pCountDocNo"></param>
        /// <returns></returns>
        public DataTable ChangeDataTable(string pDocNo, string pVersion, string pCaseStatus, string pCountDocNo)
        {
            // 案件編號&版本號&案件狀態&案件筆數
            string[] arrayDocNo = pDocNo.Split(',');
            string[] arrayVersion = pVersion.Split(',');
            string[] arrayCaseStatus = pCaseStatus.Split(',');
            string[] arrayCountDocNo = pCountDocNo.Split(',');

            DataTable dt = new DataTable();
            dt.Columns.Add("DocNo");
            dt.Columns.Add("Version");
            dt.Columns.Add("CaseStatus");
            dt.Columns.Add("CountDocNo");

            for (int i = 0; i < arrayDocNo.Length; i++)
            {
                DataRow drRow = dt.NewRow();

                // 案件編號 & 版本號 & 案件狀態 & 案件筆數
                drRow["DocNo"] = arrayDocNo[i];
                drRow["Version"] = arrayVersion[i];
                drRow["CaseStatus"] = arrayCaseStatus[i];
                drRow["CountDocNo"] = arrayCountDocNo[i];

                dt.Rows.Add(drRow);
            }

            return dt;
        }

        #region 重查

        /// <summary>
        ///  複製最大版本號的資料（CaseCustQuery）
        /// </summary>
        /// <param name="pDocNo">案件編號</param>
        /// <param name="pVersion">版本號</param>
        /// <param name="LogonUser">當前登錄者</param>
        /// <param name="dbTransactionn"></param>
        public void InsertCaseCustQuery(string pDocNo, string pVersion, string LogonUser, Guid pNewID, IDbTransaction dbTransactionn)
        {

            string sqlInsert = @"INSERT  INTO dbo.CaseCustQuery
                                             ( NewID ,
                                               DocNo ,
                                               Version ,
                                               RecvDate ,
                                               QFileName ,
                                               QFileName2 ,
                                               Status ,
                                               FileStatus ,
                                               AuditStatus ,
                                               UploadStatus ,
                                               ProcessingMode ,
                                               CloseDate ,
                                               CreatedDate ,
                                               CreatedUser ,
                                               ModifiedDate ,
                                               ModifiedUser ,
                                               CearanceUserID ,
                                               CearanceUserName ,
                                               ROpenFileName ,
                                               RFileTransactionFileName ,
                                               ImportFormFlag
                                                ,InCharge
                                                ,PDFStatus
                                                ,UploadUserID
                                                ,QueryUserID
                                                ,LetterDate
                                                ,LetterDeptName
                                                ,LetterDeptNo
                                                ,LetterNo
                                                ,Recipient
                                                ,MessageNo
                                                ,MessageDate

                                             )
                                             SELECT  @NEWID ,
                                                     @DocNo ,
                                                     @NewVersion ,
                                                     RecvDate ,
                                                     QFileName ,
                                                     QFileName2 ,
                                                     '02' ,
                                                     'N' ,
                                                     'N' ,
                                                     NULL ,
                                                     NULL ,
                                                     NULL ,
                                                     GETDATE() ,
                                                     @ModifiedUser ,
                                                     GETDATE() ,
                                                     @ModifiedUser ,
                                                     NULL ,
                                                     NULL ,
                                                     @ROpenFileName ,
                                                     @RFileTransactionFileName ,
                                                     ImportFormFlag
                                                      ,InCharge
                                                      ,'N'
                                                      ,NULL
                                                      ,@ModifiedUser
                                                      ,LetterDate
                                                      ,LetterDeptName
                                                      ,LetterDeptNo
                                                      ,LetterNo
                                                      ,Recipient
                                                      ,NULL
                                                      ,NULL
                                             FROM    dbo.CaseCustQuery
                                             WHERE   DocNo = @DocNo
                                                     AND Version = @Version";

            base.Parameter.Clear();

            base.Parameter.Add(new CommandParameter("@ModifiedUser", LogonUser));
            base.Parameter.Add(new CommandParameter("@NEWID", pNewID));
            base.Parameter.Add(new CommandParameter("@DocNo", pDocNo));
            base.Parameter.Add(new CommandParameter("@Version", pVersion));

            // 版本號
            int pNewVersion = Convert.ToInt32(pVersion) + 1;

            // 回文檔名稱
            string fileName = pDocNo + "_" + pNewVersion.ToString() + "_";

            base.Parameter.Add(new CommandParameter("@NewVersion", pNewVersion));
            base.Parameter.Add(new CommandParameter("@ROpenFileName", fileName + "Base.txt"));
            base.Parameter.Add(new CommandParameter("@RFileTransactionFileName", fileName + "Detail.txt"));

            base.ExecuteNonQuery(sqlInsert, dbTransactionn);

        }

        /// <summary>
        /// Insert 資料到CaseCustQueryVersion&CaseCustRFDMSend&BOPS067050Send&BOPS060628Send
        /// </summary>
        /// <param name="pDocNo">案件編號</param>
        /// <param name="pVersion">版本號</param>
        /// <param name="LogonUser"></param>
        /// <param name="pNewID">當前登錄者</param>
        /// <param name="dbTransactionn"></param>
        public void InsertVersionAndSend(IList<CaseCustQueryVersion> listCaseCustQueryVersion, User logonUser, IDbTransaction dbTransactionn)
        {
            // 存在CaseCustQueryVersion
            if (listCaseCustQueryVersion != null && listCaseCustQueryVersion.Count > 0)
            {
                // 逐筆Insert
                foreach (CaseCustQueryVersion item in listCaseCustQueryVersion)
                {
                    base.Parameter.Clear();

                    #region Insert CaseCustQueryVersion

                    string sqlCaseCustQueryVersion = @" INSERT INTO dbo.CaseCustQueryVersion
                                                                    ( NewID ,
                                                                      CaseCustNewID ,
                                                                      CustIdNo ,
                                                                      OpenFlag ,
                                                                      TransactionFlag ,
                                                                      QDateS ,
                                                                      QDateE ,
                                                                      Status ,
                                                                      HTGSendStatus ,
                                                                      HTGQryMessage ,
                                                                      HTGModifiedDate ,
                                                                      RFDMSendStatus ,
                                                                      RFDMQryMessage ,
                                                                      RFDModifiedDate ,
                                                                      CreatedDate ,
                                                                      CreatedUser ,
                                                                      ModifiedDate ,
                                                                      ModifiedUser
                                                                    )
                                                            VALUES  ( @VersionNewID , -- NewID - uniqueidentifier
                                                                      @CaseCustNewID , -- CaseCustNewID - uniqueidentifier
                                                                      @CustIdNo , -- CustIdNo - nvarchar(10)
                                                                      @OpenFlag , -- OpenFlag - nvarchar(1)
                                                                      @TransactionFlag , -- TransactionFlag - nvarchar(1)
                                                                      @QDateS , -- QDateS - nvarchar(8)
                                                                      @QDateE , -- QDateE - nvarchar(8)
                                                                      @Status , -- Status - nvarchar(2)
                                                                      @HTGSendStatus , -- HTGSendStatus - nvarchar(2)
                                                                      @HTGQryMessage , -- HTGQryMessage - nvarchar(1000)
                                                                      @HTGModifiedDate , -- HTGModifiedDate - datetime
                                                                      @RFDMSendStatus , -- RFDMSendStatus - nvarchar(2)
                                                                      @RFDMQryMessage , -- RFDMQryMessage - nvarchar(1000)
                                                                      @RFDModifiedDate , -- RFDModifiedDate - datetime
                                                                      @CreatedDate , -- CreatedDate - datetime
                                                                      @CreatedUser , -- CreatedUser - nvarchar(20)
                                                                      @ModifiedDate , -- ModifiedDate - datetime
                                                                      @ModifiedUser  -- ModifiedUser - nvarchar(20)
                                                                    ) ";


                    base.Parameter.Add(new CommandParameter("@VersionNewID", item.NewID));
                    base.Parameter.Add(new CommandParameter("@CaseCustNewID", item.CaseCustNewID));
                    base.Parameter.Add(new CommandParameter("@CustIdNo", item.CustIdNo));
                    base.Parameter.Add(new CommandParameter("@OpenFlag", item.OpenFlag));
                    base.Parameter.Add(new CommandParameter("@TransactionFlag", item.TransactionFlag));
                    base.Parameter.Add(new CommandParameter("@QDateS", item.QDateS != null ? item.QDateS.Trim() : item.QDateS));
                    base.Parameter.Add(new CommandParameter("@QDateE", item.QDateE != null ? item.QDateE.Trim() : item.QDateE));
                    base.Parameter.Add(new CommandParameter("@Status", item.Status));
                    base.Parameter.Add(new CommandParameter("@HTGSendStatus", item.HTGSendStatus));
                    base.Parameter.Add(new CommandParameter("@HTGQryMessage", item.HTGQryMessage));
                    base.Parameter.Add(new CommandParameter("@HTGModifiedDate", item.HTGModifiedDate));
                    base.Parameter.Add(new CommandParameter("@RFDMSendStatus", item.RFDMSendStatus));
                    base.Parameter.Add(new CommandParameter("@RFDMQryMessage", item.RFDMQryMessage));
                    base.Parameter.Add(new CommandParameter("@RFDModifiedDate", item.RFDModifiedDate));
                    base.Parameter.Add(new CommandParameter("@CreatedDate", item.CreatedDate));
                    base.Parameter.Add(new CommandParameter("@CreatedUser", item.CreatedUser));
                    base.Parameter.Add(new CommandParameter("@ModifiedDate", item.ModifiedDate));
                    base.Parameter.Add(new CommandParameter("@ModifiedUser", item.ModifiedUser));

                    base.ExecuteNonQuery(sqlCaseCustQueryVersion, dbTransactionn);

                    #endregion

                    #region insert CaseCustRFDMSend

                    if (item.TransactionFlag == "Y")
                    {
                        string dtQDateS = "";
                        string dtQDateE = "";

                        if (item.QDateS != null && item.QDateS.Trim() != "")
                        {
                            dtQDateS = item.QDateS.Substring(0, 4) + "/" + item.QDateS.Substring(4, 2) + "/" + item.QDateS.Substring(6, 2);
                        }

                        if (item.QDateE != null && item.QDateE.Trim() != "")
                        {
                            dtQDateE = item.QDateE.Substring(0, 4) + "/" + item.QDateE.Substring(4, 2) + "/" + item.QDateE.Substring(6, 2);
                        }

                        // 查詢本月最大的流水號
                        string pMaxTrnNum = GetMaxTrnNum(dbTransactionn);

                        // 流水號變量
                        int pTrnNum = 0;

                        // 截取流水號
                        if (pMaxTrnNum != "")
                        {
                            pTrnNum = Convert.ToInt32(pMaxTrnNum.Substring(8, 5));
                        }

                        string sqlCaseCustRFDMSend = "";

                        #region Insert RFDMSend

                        sqlCaseCustRFDMSend += @" insert CaseCustRFDMSend
                                             (
                                             TrnNum, VersionNewID, ID_No, Type, RFDMSendStatus, CreatedUser, CreatedDate, ModifiedUser, ModifiedDate,acctDesc,curr,channel,Start_Jnrst_Date,End_Jnrst_Date
                                             )
                                             VALUES  (
                                                @TrnNumS , 
                                                @VersionNewID , 
                                                @CustIdNo 
                                             , '0'
                                             , '02'
                                             , @CreatedUser
                                             , getdate()
                                             , @CreatedUser
                                             , getdate()
                                             ,'S','TWD','CSFS' ";

                        if (dtQDateS != "")
                        {
                            sqlCaseCustRFDMSend += ",'" + dtQDateS + "'";
                        }
                        else
                        {
                            sqlCaseCustRFDMSend += ",NULL";
                        }
                        if (dtQDateE != "")
                        {
                            sqlCaseCustRFDMSend += ",'" + dtQDateE + "'";
                        }
                        else
                        {
                            sqlCaseCustRFDMSend += ",NULL";
                        }

                        sqlCaseCustRFDMSend += " ); ";

                        sqlCaseCustRFDMSend += @" insert CaseCustRFDMSend
                                             (
                                             TrnNum, VersionNewID, ID_No, Type, RFDMSendStatus, CreatedUser, CreatedDate, ModifiedUser, ModifiedDate,acctDesc,curr,channel,Start_Jnrst_Date,End_Jnrst_Date
                                             )
                                                 VALUES  (
                                                @TrnNumQ , 
                                                @VersionNewID , 
                                                @CustIdNo 
                                             , '0'
                                             , '02'
                                             , @CreatedUser
                                             , getdate()
                                             , @CreatedUser
                                             , getdate()
                                             ,'Q','TWD','CSFS' ";

                        if (dtQDateS != "")
                        {
                            sqlCaseCustRFDMSend += ",'" + dtQDateS + "'";
                        }
                        else
                        {
                            sqlCaseCustRFDMSend += ",NULL";
                        }
                        if (dtQDateE != "")
                        {
                            sqlCaseCustRFDMSend += ",'" + dtQDateE + "'";
                        }
                        else
                        {
                            sqlCaseCustRFDMSend += ",NULL";
                        }

                        sqlCaseCustRFDMSend += " ); ";

                        sqlCaseCustRFDMSend += @" insert CaseCustRFDMSend
                                             (
                                             TrnNum, VersionNewID, ID_No, Type, RFDMSendStatus, CreatedUser, CreatedDate, ModifiedUser, ModifiedDate,acctDesc,curr,channel,Start_Jnrst_Date,End_Jnrst_Date
                                             )
                                                VALUES  (
                                                @TrnNumT , 
                                                @VersionNewID , 
                                                @CustIdNo 
                                             , '0'
                                             , '02'
                                             , @CreatedUser
                                             , getdate()
                                             , @CreatedUser
                                             , getdate()
                                             ,'T','TWD','CSFS' ";

                        if (dtQDateS != "")
                        {
                            sqlCaseCustRFDMSend += ",'" + dtQDateS + "'";
                        }
                        else
                        {
                            sqlCaseCustRFDMSend += ",NULL";
                        }
                        if (dtQDateE != "")
                        {
                            sqlCaseCustRFDMSend += ",'" + dtQDateE + "'";
                        }
                        else
                        {
                            sqlCaseCustRFDMSend += ",NULL";
                        }

                        sqlCaseCustRFDMSend += " ); ";

                        base.Parameter.Add(new CommandParameter("@TrnNumS", CalculateTrnNum(pTrnNum)));
                        pTrnNum++;

                        base.Parameter.Add(new CommandParameter("@TrnNumQ", CalculateTrnNum(pTrnNum)));
                        pTrnNum++;

                        base.Parameter.Add(new CommandParameter("@TrnNumT", CalculateTrnNum(pTrnNum)));
                        pTrnNum++;

                        #endregion

                        base.ExecuteNonQuery(sqlCaseCustRFDMSend, dbTransactionn);
                    }

                    #endregion

                    if (item.OpenFlag == "Y")
                    {
                        #region Insert BOPS067050Send

                        string sqlBOPS067050Send = @" INSERT INTO dbo.BOPS067050Send
                                                            ( NewID ,
                                                              VersionNewID ,
                                                              CustIdNo ,
                                                              Optn ,
                                                              CreatedUser ,
                                                              CreatedDate ,
                                                              ModifiedUser ,
                                                              ModifiedDate ,
                                                              SendStatus 
                                                            )
                                                    VALUES  (  NEWID() , -- NewID - uniqueidentifier
                                                              @VersionNewID , -- VersionNewID - uniqueidentifier
                                                              @CustIdNo , -- CustIdNo - nvarchar(14)
                                                              'D' , -- Optn - nvarchar(1)
                                                              @CreatedUser , -- CreatedUser - nvarchar(50)
                                                              @CreatedDate , -- CreatedDate - datetime
                                                              @ModifiedUser , -- ModifiedUser - nvarchar(50)
                                                              @ModifiedDate , -- ModifiedDate - datetime
                                                              '02'  -- SendStatus - char(2)
                                                            )";

                        base.ExecuteNonQuery(sqlBOPS067050Send, dbTransactionn);

                        #endregion

                        #region Insert BOPS060628Send

                        string sqlBOPS060628Send = @" INSERT INTO dbo.BOPS060628Send
                                                            ( NewID ,
                                                              CustIdNo ,
                                                              CreatedUser ,
                                                              CreatedDate ,
                                                              ModifiedUser ,
                                                              ModifiedDate ,
                                                              SendStatus ,
                                                              VersionNewID
                                                            )
                                                    VALUES  ( NEWID() , -- NewID - uniqueidentifier
                                                              @CustIdNo , -- CustIdNo - nvarchar(14)
                                                              @CreatedUser, -- CreatedUser - nvarchar(50)
                                                              @CreatedDate , -- CreatedDate - datetime
                                                              @ModifiedUser , -- ModifiedUser - nvarchar(50)
                                                              @ModifiedDate , -- ModifiedDate - datetime
                                                              '02' , -- SendStatus - char(2)
                                                              @VersionNewID  -- VersionNewID - uniqueidentifier
                                                            )";

                        base.ExecuteNonQuery(sqlBOPS060628Send, dbTransactionn);

                        #endregion
                    }

                    #region Insert ApprMsgKey

                    ApprMsgKeyVO model = new ApprMsgKeyVO();
                    model.MsgUID = logonUser.Account;
                    model.MsgKeyLP = logonUser.LDAPPwd;
                    model.MsgKeyLU = logonUser.Account;
                    model.MsgKeyRU = logonUser.RCAFAccount;
                    model.MsgKeyRP = logonUser.RCAFPs;
                    model.MsgKeyRB = logonUser.RCAFBranch;
                    model.VersionNewID = item.NewID;

                    pCaseCustBIZ.InsertApprMsgKey(model, dbTransactionn);

                    #endregion

                }
            }

        }

        /// <summary>
        /// 根據案件編號&版本號來文檔案查詢條件帳號檔
        /// </summary>
        /// <param name="pDocNo">案件編號</param>
        /// <param name="pVersion">版本號</param>
        /// <param name="LogonUser">當前登錄者</param>
        /// <param name="pNewID">來文檔案查詢條件檔主鍵</param>
        /// <param name="dbTransactionn"></param>
        /// <returns></returns>
        public IList<CaseCustQueryVersion> GetCaseCustQueryVersion(string pDocNo, string pVersion, string LogonUser, Guid pNewID, IDbTransaction dbTransactionn)
        {
            string sqlSelect = @" SELECT  NEWID() AS NewID ,
                                                     @CaseCustNewID AS CaseCustNewID,
                                                     CustIdNo AS CustIdNo,
                                                     OpenFlag AS OpenFlag,
                                                     TransactionFlag AS TransactionFlag,
                                                     QDateS AS QDateS,
                                                     QDateE AS QDateE,
                                                     '02' AS Status,
                                                     HTGSendStatus ,
                                                     HTGQryMessage ,
                                                     HTGModifiedDate ,
                                                     RFDMSendStatus ,
                                                     RFDMQryMessage ,
                                                     RFDModifiedDate ,
                                                     GETDATE()  AS CreatedDate ,
                                                     @ModifiedUser  AS CreatedUser ,
                                                     GETDATE()   AS ModifiedDate,
                                                     @ModifiedUser  AS ModifiedUser
                                             FROM    ( SELECT    
IdNo,
CustIdNo ,
                                                                 OpenFlag ,
                                                                 TransactionFlag ,
                                                                 QDateS ,
                                                                 QDateE ,
                                                                 ( CASE WHEN ISNULL(OpenFlag, '') = 'Y' THEN '2' ELSE NULL END ) AS HTGSendStatus ,
                                                                 ( CASE WHEN ISNULL(OpenFlag, '') = 'Y' THEN '' ELSE NULL END ) AS HTGQryMessage ,
                                                                 ( CASE WHEN ISNULL(OpenFlag, '') = 'Y' THEN GETDATE()ELSE NULL END ) AS HTGModifiedDate ,
                                                                 ( CASE WHEN ISNULL(TransactionFlag, '') = 'Y' THEN '2' ELSE NULL END ) AS RFDMSendStatus ,
                                                                 ( CASE WHEN ISNULL(TransactionFlag, '') = 'Y' THEN '' ELSE NULL END ) AS RFDMQryMessage ,
                                                                 ( CASE WHEN ISNULL(TransactionFlag, '') = 'Y' THEN GETDATE() ELSE NULL END ) AS RFDModifiedDate
                                                       FROM      CaseCustQueryVersion
                                                       WHERE     CaseCustNewID = ( SELECT    NEWID
                                                                                   FROM      CaseCustQuery
                                                                                   WHERE     DocNo = @DocNo
                                                                                             AND Version = @Version
                                                                                 )
                                                     ) A
ORDER BY IdNo ASC
";


            base.Parameter.Clear();

            base.Parameter.Add(new CommandParameter("@ModifiedUser", LogonUser));
            base.Parameter.Add(new CommandParameter("@CaseCustNewID", pNewID));
            base.Parameter.Add(new CommandParameter("@DocNo", pDocNo));
            base.Parameter.Add(new CommandParameter("@Version", pVersion));

            return base.SearchList<CaseCustQueryVersion>(sqlSelect.ToString(), dbTransactionn);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="strMaxNo"></param>
        /// <returns></returns>
        private string CalculateTrnNum(int strMaxNo)
        {
            return DateTime.Now.ToString("yyyyMMdd") + String.Format("{0:D5}", strMaxNo + 1);
        }

        /// <summary>
        /// 根據案件編號&版本號更新CaseCustQuery
        /// </summary>
        /// <param name="pDocNo">案件編號</param>
        /// <param name="pVersion">版本號</param>
        /// <param name="LogonUser">當前登錄者</param>
        /// <param name="dbTransactionn"></param>
        public void UpdateCaseCustQuery(string pDocNo, string pVersion, string LogonUser, IDbTransaction dbTransactionn)
        {
            // 更新SQL
            string sqlUpdate = @"UPDATE  CaseCustQuery
                                 SET     Status = '06' ,
                                         FileStatus = 'N' ,
                                         PDFStatus = 'N' ,
                                         AuditStatus = 'N' ,
                                         CearanceUserID = NULL ,
                                         CearanceUserName = NULL ,
                                         UploadStatus = 'N' ,
                                         UploadUserID = NULL ,
                                         ProcessingMode = NULL ,
                                         CloseDate = NULL ,
                                         --MessageNo = NULL ,
                                         MessageDate = NULL ,
                                         ModifiedDate = GETDATE() ,
                                         ModifiedUser = @ModifiedUser,
                                         QueryUserID = @ModifiedUser

                                 WHERE   DocNo = @DocNo
                                  AND 
                                        Version = @Version";

            base.Parameter.Clear();

            base.Parameter.Add(new CommandParameter("@ModifiedUser", LogonUser));
            base.Parameter.Add(new CommandParameter("@DocNo", pDocNo));
            base.Parameter.Add(new CommandParameter("@Version", pVersion));

            base.ExecuteNonQuery(sqlUpdate, dbTransactionn);
        }

        /// <summary>
        ///  更新
        /// </summary>
        /// <param name="pDocNo"></param>
        /// <param name="pVersion"></param>
        /// <param name="LogonUser"></param>
        /// <param name="dbTransactionn"></param>
        public void UpdateCaseCustQueryVersion(string pDocNo, string pVersion, string LogonUser, IDbTransaction dbTransactionn)
        {
            // 更新SQL
            string sqlUpdate = @"UPDATE  CaseCustQueryVersion
                                   SET     Status = '06' ,
                                           HTGSendStatus = ( CASE WHEN OpenFlag = 'Y' THEN '2' ELSE NULL END ) ,
                                           HTGQryMessage = ( CASE WHEN OpenFlag = 'Y' THEN '' ELSE NULL END ) ,
                                           HTGModifiedDate = (CASE WHEN OpenFlag = 'Y' THEN GETDATE()  ELSE NULL END ),
                                           RFDMSendStatus = ( CASE WHEN TransactionFlag = 'Y' THEN '2' ELSE NULL END ) ,
                                           RFDMQryMessage = ( CASE WHEN TransactionFlag = 'Y' THEN '2' ELSE NULL END ) ,
                                           RFDModifiedDate = ( CASE WHEN TransactionFlag = 'Y' THEN GETDATE() ELSE NULL END ) ,
                                           ModifiedDate = GETDATE(),
                                           ModifiedUser = @ModifiedUser
                                   WHERE
                                          CaseCustNewID = (SELECT NEWID FROM CaseCustQuery WHERE DocNo = @DocNo AND Version = @Version)";

            base.Parameter.Clear();

            base.Parameter.Add(new CommandParameter("@ModifiedUser", LogonUser));
            base.Parameter.Add(new CommandParameter("@DocNo", pDocNo));
            base.Parameter.Add(new CommandParameter("@Version", pVersion));

            base.ExecuteNonQuery(sqlUpdate, dbTransactionn);
        }

        /// <summary>
        /// 刪除有關Table 資料
        /// </summary>
        /// <param name="pDocNo">案件編號</param>
        /// <param name="pVersion">版本號</param>
        /// <param name="dbTransactionn"></param>
        public void DeleteTables(string pDocNo, string pVersion, IDbTransaction dbTransactionn)
        {
            #region CaseCustRFDM

            string sqlDelete = @" DELETE FROM CaseCustRFDMSend
                                             WHERE  VersionNewID IN (
                                                    SELECT  NewID
                                                    FROM    CaseCustQueryVersion
                                                    WHERE   CaseCustNewID = ( SELECT    NEWID
                                                                              FROM      CaseCustQuery
                                                                              WHERE     DocNo = @DocNo
                                                                                        AND Version = @Version
                                                                            ) )";

            sqlDelete += @"DELETE FROM CaseCustRFDMRecv
                                             WHERE  VersionNewID IN (
                                                    SELECT  NewID
                                                    FROM    CaseCustQueryVersion
                                                    WHERE   CaseCustNewID = ( SELECT    NEWID
                                                                              FROM      CaseCustQuery
                                                                              WHERE     DocNo = @DocNo
                                                                                        AND Version = @Version
                                                                            ) )";

            #endregion

            #region BOPS067050

            sqlDelete += @"DELETE FROM BOPS067050Send
                                             WHERE  VersionNewID IN (
                                                    SELECT  NewID
                                                    FROM    CaseCustQueryVersion
                                                    WHERE   CaseCustNewID = ( SELECT    NEWID
                                                                              FROM      CaseCustQuery
                                                                              WHERE     DocNo = @DocNo
                                                                                        AND Version = @Version
                                                                            ) )";

            sqlDelete += @"DELETE FROM BOPS067050Recv
                                             WHERE  VersionNewID IN (
                                                    SELECT  NewID
                                                    FROM    CaseCustQueryVersion
                                                    WHERE   CaseCustNewID = ( SELECT    NEWID
                                                                              FROM      CaseCustQuery
                                                                              WHERE     DocNo = @DocNo
                                                                                        AND Version = @Version
                                                                            ) )";

            #endregion

            #region BOPS060628

            sqlDelete += @"DELETE FROM BOPS060628Send
                                             WHERE  VersionNewID IN (
                                                    SELECT  NewID
                                                    FROM    CaseCustQueryVersion
                                                    WHERE   CaseCustNewID = ( SELECT    NEWID
                                                                              FROM      CaseCustQuery
                                                                              WHERE     DocNo = @DocNo
                                                                                        AND Version = @Version
                                                                            ) )";

            sqlDelete += @"DELETE FROM BOPS060628Recv
                                             WHERE  VersionNewID IN (
                                                    SELECT  NewID
                                                    FROM    CaseCustQueryVersion
                                                    WHERE   CaseCustNewID = ( SELECT    NEWID
                                                                              FROM      CaseCustQuery
                                                                              WHERE     DocNo = @DocNo
                                                                                        AND Version = @Version
                                                                            ) )";

            #endregion

            #region BOPS060490

            sqlDelete += @"DELETE FROM BOPS060490Send
                                             WHERE  VersionNewID IN (
                                                    SELECT  NewID
                                                    FROM    CaseCustQueryVersion
                                                    WHERE   CaseCustNewID = ( SELECT    NEWID
                                                                              FROM      CaseCustQuery
                                                                              WHERE     DocNo = @DocNo
                                                                                        AND Version = @Version
                                                                            ) )";

            sqlDelete += @"DELETE FROM BOPS060490Recv
                                             WHERE  VersionNewID IN (
                                                    SELECT  NewID
                                                    FROM    CaseCustQueryVersion
                                                    WHERE   CaseCustNewID = ( SELECT    NEWID
                                                                              FROM      CaseCustQuery
                                                                              WHERE     DocNo = @DocNo
                                                                                        AND Version = @Version
                                                                            ) )";

            #endregion

            #region BOPS000401

            sqlDelete += @"DELETE FROM BOPS000401Send
                                             WHERE  VersionNewID IN (
                                                    SELECT  NewID
                                                    FROM    CaseCustQueryVersion
                                                    WHERE   CaseCustNewID = ( SELECT    NEWID
                                                                              FROM      CaseCustQuery
                                                                              WHERE     DocNo = @DocNo
                                                                                        AND Version = @Version
                                                                            ) )";

            sqlDelete += @"DELETE FROM BOPS000401Recv
                                             WHERE  VersionNewID IN (
                                                    SELECT  NewID
                                                    FROM    CaseCustQueryVersion
                                                    WHERE   CaseCustNewID = ( SELECT    NEWID
                                                                              FROM      CaseCustQuery
                                                                              WHERE     DocNo = @DocNo
                                                                                        AND Version = @Version
                                                                            ) )";

            #endregion

            #region ApprMsgKey

            sqlDelete += @"DELETE FROM ApprMsgKey
                                             WHERE  VersionNewID IN (
                                                    SELECT  NewID
                                                    FROM    CaseCustQueryVersion
                                                    WHERE   CaseCustNewID = ( SELECT    NEWID
                                                                              FROM      CaseCustQuery
                                                                              WHERE     DocNo = @DocNo
                                                                                        AND Version = @Version
                                                                            ) )";

            #endregion

            base.Parameter.Clear();

            base.Parameter.Add(new CommandParameter("@DocNo", pDocNo));
            base.Parameter.Add(new CommandParameter("@Version", pVersion));

            base.ExecuteNonQuery(sqlDelete, dbTransactionn);
        }

        /// <summary>
        ///  insert 資料到send &ApprMsgKey
        /// </summary>
        /// <param name="pDocNo">案件編碼</param>
        /// <param name="pVersion">版本號</param>
        /// <param name="logonUser">當前登錄者</param>
        /// <param name="dbTransactionn"></param>
        public void InsertTables(string pDocNo, string pVersion, User logonUser, IDbTransaction dbTransactionn)
        {
            #region 取得來文檔案查詢條件帳號檔

            base.Parameter.Clear();

            string sqlSelect = @"SELECT  NEWID AS NEWID ,
                                         CustIdNo ,
                                         OpenFlag ,
QDateS,
QDateE,
                                         TransactionFlag
                                 FROM    CaseCustQueryVersion
                                 WHERE   CaseCustNewID = ( SELECT    NEWID
                                                           FROM      CaseCustQuery
                                                           WHERE     DocNo = @DocNo
                                                                     AND Version = @Version
                                                         )";

            base.Parameter.Add(new CommandParameter("@DocNo", pDocNo));
            base.Parameter.Add(new CommandParameter("@Version", pVersion));

            IList<CaseCustQueryVersion> list = base.SearchList<CaseCustQueryVersion>(sqlSelect.ToString(), dbTransactionn);

            #endregion

            //  取得當前登錄者
            string pUserAccount = logonUser.Account;

            if (list != null && list.Count > 0)
            {
                foreach (CaseCustQueryVersion item in list)
                {
                    base.Parameter.Clear();

                    base.Parameter.Add(new CommandParameter("@ModifiedUser", pUserAccount));
                    base.Parameter.Add(new CommandParameter("@VersionNewID", item.NewID));
                    base.Parameter.Add(new CommandParameter("@CustIdNo", item.CustIdNo));

                    if (item.TransactionFlag == "Y")
                    {
                        #region insert CaseCustRFDMSend

                        string dtQDateS = "";
                        string dtQDateE = "";

                        if (item.QDateS != null && item.QDateS.Trim() != "")
                        {
                            dtQDateS = item.QDateS.Substring(0, 4) + "/" + item.QDateS.Substring(4, 2) + "/" + item.QDateS.Substring(6, 2);
                        }

                        if (item.QDateE != null && item.QDateE.Trim() != "")
                        {
                            dtQDateE = item.QDateE.Substring(0, 4) + "/" + item.QDateE.Substring(4, 2) + "/" + item.QDateE.Substring(6, 2);
                        }

                        // 查詢本月最大的流水號
                        string pMaxTrnNum = GetMaxTrnNum(dbTransactionn);

                        // 流水號變量
                        int pTrnNum = 0;

                        // 截取流水號
                        if (pMaxTrnNum != "")
                        {
                            pTrnNum = Convert.ToInt32(pMaxTrnNum.Substring(8, 5));
                        }

                        string sqlCaseCustRFDMSend = "";

                        #region Insert RFDMSend

                        sqlCaseCustRFDMSend += @" insert CaseCustRFDMSend
                                             (
                                             TrnNum, VersionNewID, ID_No, Type, RFDMSendStatus, CreatedUser, CreatedDate, ModifiedUser, ModifiedDate,acctDesc,curr,channel,Start_Jnrst_Date,End_Jnrst_Date
                                             )
                                             VALUES  (
                                                @TrnNumS , 
                                                @VersionNewID , 
                                                @CustIdNo 
                                             , '0'
                                             , '02'
                                             , @ModifiedUser
                                             , getdate()
                                             , @ModifiedUser
                                             , getdate()
                                             ,'S','TWD','CSFS' ";

                        if (dtQDateS != "")
                        {
                            sqlCaseCustRFDMSend += ",'" + dtQDateS + "'";
                        }
                        else
                        {
                            sqlCaseCustRFDMSend += ",NULL";
                        }
                        if (dtQDateE != "")
                        {
                            sqlCaseCustRFDMSend += ",'" + dtQDateE + "'";
                        }
                        else
                        {
                            sqlCaseCustRFDMSend += ",NULL";
                        }

                        sqlCaseCustRFDMSend += " ); ";

                        sqlCaseCustRFDMSend += @" insert CaseCustRFDMSend
                                             (
                                             TrnNum, VersionNewID, ID_No, Type, RFDMSendStatus, CreatedUser, CreatedDate, ModifiedUser, ModifiedDate,acctDesc,curr,channel,Start_Jnrst_Date,End_Jnrst_Date
                                             )
                                                 VALUES  (
                                                @TrnNumQ , 
                                                @VersionNewID , 
                                                @CustIdNo 
                                             , '0'
                                             , '02'
                                             , @ModifiedUser
                                             , getdate()
                                             , @ModifiedUser
                                             , getdate()
                                             ,'Q','TWD','CSFS' ";

                        if (dtQDateS != "")
                        {
                            sqlCaseCustRFDMSend += ",'" + dtQDateS + "'";
                        }
                        else
                        {
                            sqlCaseCustRFDMSend += ",NULL";
                        }
                        if (dtQDateE != "")
                        {
                            sqlCaseCustRFDMSend += ",'" + dtQDateE + "'";
                        }
                        else
                        {
                            sqlCaseCustRFDMSend += ",NULL";
                        }

                        sqlCaseCustRFDMSend += " ); ";

                        sqlCaseCustRFDMSend += @" insert CaseCustRFDMSend
                                             (
                                             TrnNum, VersionNewID, ID_No, Type, RFDMSendStatus, CreatedUser, CreatedDate, ModifiedUser, ModifiedDate,acctDesc,curr,channel,Start_Jnrst_Date,End_Jnrst_Date
                                             )
                                                VALUES  (
                                                @TrnNumT , 
                                                @VersionNewID , 
                                                @CustIdNo 
                                             , '0'
                                             , '02'
                                             , @ModifiedUser
                                             , getdate()
                                             , @ModifiedUser
                                             , getdate()
                                             ,'T','TWD','CSFS' ";

                        if (dtQDateS != "")
                        {
                            sqlCaseCustRFDMSend += ",'" + dtQDateS + "'";
                        }
                        else
                        {
                            sqlCaseCustRFDMSend += ",NULL";
                        }
                        if (dtQDateE != "")
                        {
                            sqlCaseCustRFDMSend += ",'" + dtQDateE + "'";
                        }
                        else
                        {
                            sqlCaseCustRFDMSend += ",NULL";
                        }

                        sqlCaseCustRFDMSend += " ); ";

                        base.Parameter.Add(new CommandParameter("@TrnNumS", CalculateTrnNum(pTrnNum)));
                        pTrnNum++;

                        base.Parameter.Add(new CommandParameter("@TrnNumQ", CalculateTrnNum(pTrnNum)));
                        pTrnNum++;

                        base.Parameter.Add(new CommandParameter("@TrnNumT", CalculateTrnNum(pTrnNum)));
                        pTrnNum++;

                        #endregion


                        base.ExecuteNonQuery(sqlCaseCustRFDMSend, dbTransactionn);

                        #endregion
                    }

                    if (item.OpenFlag == "Y")
                    {
                        #region Insert BOPS067050Send

                        string sqlBOPS067050Send = @" INSERT INTO dbo.BOPS067050Send
                                                            ( NewID ,
                                                              VersionNewID ,
                                                              CustIdNo ,
                                                              Optn ,
                                                              CreatedUser ,
                                                              CreatedDate ,
                                                              ModifiedUser ,
                                                              ModifiedDate ,
                                                              SendStatus 
                                                            )
                                                    VALUES  (  NEWID() , -- NewID - uniqueidentifier
                                                              @VersionNewID , -- VersionNewID - uniqueidentifier
                                                              @CustIdNo , -- CustIdNo - nvarchar(14)
                                                              'D' , -- Optn - nvarchar(1)
                                                              @ModifiedUser , -- CreatedUser - nvarchar(50)
                                                              GETDATE() , -- CreatedDate - datetime
                                                              @ModifiedUser , -- ModifiedUser - nvarchar(50)
                                                              GETDATE() , -- ModifiedDate - datetime
                                                              '02'  -- SendStatus - char(2)
                                                            )";

                        base.ExecuteNonQuery(sqlBOPS067050Send, dbTransactionn);

                        #endregion

                        #region Insert BOPS060628Send

                        string sqlBOPS060628Send = @" INSERT INTO dbo.BOPS060628Send
                                                            ( NewID ,
                                                              CustIdNo ,
                                                              CreatedUser ,
                                                              CreatedDate ,
                                                              ModifiedUser ,
                                                              ModifiedDate ,
                                                              SendStatus ,
                                                              VersionNewID
                                                            )
                                                    VALUES  ( NEWID() , -- NewID - uniqueidentifier
                                                              @CustIdNo , -- CustIdNo - nvarchar(14)
                                                              @ModifiedUser, -- CreatedUser - nvarchar(50)
                                                               GETDATE() , -- CreatedDate - datetime
                                                              @ModifiedUser , -- ModifiedUser - nvarchar(50)
                                                              GETDATE() , -- ModifiedDate - datetime
                                                              '02' , -- SendStatus - char(2)
                                                              @VersionNewID  -- VersionNewID - uniqueidentifier
                                                            )";

                        base.ExecuteNonQuery(sqlBOPS060628Send, dbTransactionn);

                        #endregion}
                    }

                    #region Insert ApprMsgKey

                    ApprMsgKeyVO model = new ApprMsgKeyVO();
                    model.MsgUID = logonUser.Account;
                    model.MsgKeyLP = logonUser.LDAPPwd;
                    model.MsgKeyLU = logonUser.Account;
                    model.MsgKeyRU = logonUser.RCAFAccount;
                    model.MsgKeyRP = logonUser.RCAFPs;
                    model.MsgKeyRB = logonUser.RCAFBranch;
                    model.VersionNewID = item.NewID;

                    pCaseCustBIZ.InsertApprMsgKey(model, dbTransactionn);

                    #endregion

                }
            }


        }

        public string GetMaxTrnNum(IDbTransaction dbTransactionn)
        {
            try
            {

                string sql = @"
                            select 
                            	isnull(MAX(TrnNum),'') as TrnNumMax 
                            from CaseCustRFDMSend
                            where TrnNum like '" + DateTime.Now.ToString("yyyyMM") + "%' ";

                DataTable dt = base.Search(sql, dbTransactionn);

                if (dt != null && dt.Rows.Count > 0)
                {
                    return dt.Rows[0]["TrnNumMax"].ToString();
                }
                else
                {
                    return "";
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 根據案件編碼取得版本號最大的案件狀態
        /// </summary>
        /// <param name="strDocNo">案件</param>
        /// <returns></returns>
        public CaseCustQuery GetStatusByDocNo(string strDocNo)
        {
            string sqlSelect = @" SELECT TOP ( 1 )
                                            Status AS CaseStatus
                                            ,Version
                                            ,ImportFormFlag
                                    FROM    CaseCustQuery
                                    WHERE   DocNo = @DocNo
                                    ORDER BY Version DESC";

            base.Parameter.Clear();

            base.Parameter.Add(new CommandParameter("@DocNo", strDocNo));

            return base.SearchList<CaseCustQuery>(sqlSelect)[0];
        }

        /// <summary>
        /// 根據案件編號查詢CaseCustQuery
        /// </summary>
        /// <param name="strDocNo"></param>
        /// <returns></returns>
        public DataTable GetCaseCustQueryByDocNo(string strDocNo)
        {
            string sqlSelect = @" 
SELECT 
    Status, DocNo, Version, ROpenFileName, RFileTransactionFileName, CloseDate 
FROM CaseCustQuery
WHERE  DocNo = @DocNo
ORDER BY Version DESC";

            base.Parameter.Clear();

            base.Parameter.Add(new CommandParameter("@DocNo", strDocNo));

            return base.Search(sqlSelect);

        }


        #endregion

    }
}
