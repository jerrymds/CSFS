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
    public class CaseDeadQueryBIZ : CommonBIZ
    {
        CaseDeadCommonBIZ pCaseDeadCommonBIZ = new CaseDeadCommonBIZ();

        CaseDeadBIZ pCaseDeadBIZ = new CaseDeadBIZ();

        /// <summary>
        /// 死亡記錄查詢與重送清單資料
        /// </summary>
        /// <param name="model"></param>
        /// <param name="pageNum"></param>
        /// <returns></returns>

        public IList<CaseDeadVersion> GetHisQueryList(CaseRecordCondition model, int pageNum, string strSortExpression, string strSortDirection)
        {
            try
            {
                base.PageIndex = pageNum;

                #region 拼接查詢條件
                string sqlWhere = "";

                #region 待補充欄位


                // 統一編號
                if (!string.IsNullOrEmpty(model.HeirId))
                {
                    //sqlWhere += @" AND t.HeirId like @HeirId";
                    sqlWhere += @" AND t.HeirId like  '%" + model.HeirId+"%' ";
                    //base.Parameter.Add(new CommandParameter("@HeirId", "'%" + model.HeirId + "%'"));
                }

                // 帳號  
                if (!string.IsNullOrEmpty(model.CustAccount))
                {
                    sqlWhere += @" AND t.CustAccount  like  '%" + model.CustAccount + "%' ";
                    //base.Parameter.Add(new CommandParameter("@CustAccount", "'%" + model.CustAccount + "%'"));
                }

                #endregion

                // 案件編號DocNo
                if (!string.IsNullOrEmpty(model.DocNo))
                {
                    sqlWhere += @" AND t.DocNo like '%" + model.DocNo + "%' ";
                    //base.Parameter.Add(new CommandParameter("@DocNo", "'%" + model.DocNo + "%'"));
                }

                // 查詢日期S FinishDateStart
                if (!string.IsNullOrEmpty(model.ForCDateS))
                {
                    sqlWhere += @" AND convert(nvarchar(10),t.CreatedDate,111) >= '" + model.ForCDateS+"' ";
                    //base.Parameter.Add(new CommandParameter("@ForCDateS", model.ForCDateS));
                }

                // 查詢日期E FinishDateEnd
                if (!string.IsNullOrEmpty(model.ForCDateE))
                {
                    sqlWhere += @" AND convert(nvarchar(10),t.CreatedDate,111) <= '" + model.ForCDateE+"' ";
                   // base.Parameter.Add(new CommandParameter("@ForCDateE", model.ForCDateE));
                }

                //// 案件狀態 CaseStatus
                if (!string.IsNullOrEmpty(model.CaseStatus))
                {
                    sqlWhere += @" AND t.Status = '" + model.CaseStatus+"' ";
                       // base.Parameter.Add(new CommandParameter("@CaseStatus", model.CaseStatus));
                }

                //// 使用者
                if (!string.IsNullOrEmpty(model.AgentDepartmentUser))
                {
                    int intCreatedUser = model.AgentDepartmentUser.IndexOf("-");
                    if (intCreatedUser > 0)
                    {

                        sqlWhere += @" AND t.CreatedUser = '" + model.AgentDepartmentUser.Substring(0, intCreatedUser) +"' ";
                    }
                    else
                    {
                        sqlWhere += @" AND t.CreatedUser = '" + model.AgentDepartmentUser + "' ";
                    }
                    //base.Parameter.Add(new CommandParameter("@AgentDepartmentUser", model.AgentDepartmentUser));
                }



                #endregion

                #region sql
                StringBuilder sql = new StringBuilder();
                
                //adam 20200219 改為案件編號+日期唯一
                sql.Append(@"Select * from 
   (Select  ROW_NUMBER() OVER ( ORDER BY DocNo ) AS RowNum,DocNo
      ,Status
      ,CreatedDate
      ,CreatedUser
      ,CaseTrsNewID 
	  ,NewID
	  ,(select top 1 CodeDesc from ParmCode where Codetype = 'CaseCustStatus' and codeno = Status) as statusname
	   From
	   (select 	   
	  t.DocNo
      ,t.Status
      ,t.CreatedDate
      ,t.CreatedUser
      ,t.CaseTrsNewID 
	  ,t.NewId
	,ROW_NUMBER() Over (Partition By t.DocNo,CONVERT(Varchar(10),t.CreatedDate,111) Order By CONVERT(Varchar(10),t.CreatedDate,111),t.status ) As Sort From  CaseDeadVersion t
	inner join
   CaseMaster m
  on  t.CaseTrsNewID = m.CaseID 
WHERE 1 =1 " + sqlWhere + @"
   ) as tabletemp  where Sort=1 ");

                // 判斷是否分頁
                sql.Append(@" ) as tmp WHERE  RowNum > " + PageSize * (pageNum - 1)
                                   + " AND RowNum < " + ((PageSize * pageNum) + 1));

                #endregion

                // 資料總筆數
                string sqlCount = @"
                                select count(0)
                                from 
                                  ( select docno,CONVERT(Varchar(10),CaseDeadVersion.CreatedDate,111) as CreatedDate,Status,HeirId,CustAccount,CreatedUser
,ROW_NUMBER() Over (Partition By CaseDeadVersion.DocNo,CONVERT(Varchar(10),CaseDeadVersion.CreatedDate,111) Order By CaseDeadVersion.CreatedDate Desc) As Sort From  CaseDeadVersion ) as t  WHERE Sort =1  " + sqlWhere ;


                base.DataRecords = int.Parse(base.ExecuteScalar(sqlCount).ToString());

                // 查詢清單資料
                IList< CaseDeadVersion> _ilsit = base.SearchList< CaseDeadVersion> (sql.ToString());

                if (_ilsit.Count > 0)
                {
                    return _ilsit;
                }
                else
                {
                    return new List<CaseDeadVersion>();
                }
                
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        /// <summary>
        /// 刪除清單資料
        /// </summary>
        /// <param name="Content"></param>
        /// <returns></returns>
        public int DeleteCaseDeadQuery(string Content)
        {
            IDbConnection dbConnection = OpenConnection();
            IDbTransaction dbTrans = null;
            try
            {
                using (dbConnection)
                {
                    dbTrans = dbConnection.BeginTransaction();

                    // 將字串拆分成數組
                    string[] arrayNewID = Content.Split(',');

                    string sql = "";
                    base.Parameter.Clear();
                    string caseid = "";
                    string createdate = "";
                    string cdate = "";         
                    // 遍歷，組刪除sql
                    for (int i = 0; i < arrayNewID.Length; i++)
                    {
                        int index1 = arrayNewID[i].ToString().IndexOf('|'); 

                        if (arrayNewID[i].ToString().Length > 22)
                        {
                            caseid = arrayNewID[i].ToString().Substring(0, index1);
                            createdate =  arrayNewID[i].ToString().Substring(index1 + 1, arrayNewID[i].ToString().Length - (index1 + 2)) ;
                            DateTime sourceDate = DateTime.Parse(createdate);
                             cdate = sourceDate.ToString("yyyy/MM/dd");
                        }
                        else
                        {
                            caseid = arrayNewID[i].ToString();
                        }
                        //sql += " delete from CaseDeadVersion where Status Not in   ('03')  and CaseTrsNewID = '" + arrayNewID[i].ToString() + "' ; ";
                        sql += " delete from CaseDeadVersion where  CaseTrsNewID = '" + caseid + "' and  CONVERT(VARCHAR(10), CreatedDate,111) = '" + cdate + "'";
                        //base.Parameter.Add(new CommandParameter("@NewID" + i.ToString(), arrayNewID[i]));
                    }

                    int ok = base.ExecuteNonQuery(sql);

                    dbTrans.Commit();
                    return ok;
                }
            }
            catch (Exception ex)
            {
                dbTrans.Rollback();

                return 0;
            }
        }
        /// <summary>
        ///  查詢匯出資料
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        //public DataTable GetHistoryQueryDdata(string strNewID)
        //{
        //    try
        //    {
        //        #region 拼接查詢條件
        //        string sqlWhere = "";
        //        string[] NewIDArray = strNewID.Split(',');
        //        for (int i = 0; i < NewIDArray.Length; i++)
        //        {
        //            sqlWhere += "'" + NewIDArray[i] + "',";
        //        }
        //        sqlWhere = sqlWhere.Substring(0, sqlWhere.Length - 1);
        //        #endregion

        //        #region sql
        //        StringBuilder sql = new StringBuilder();

        //        sql.Append(@"
        //                    	select 
        //                        	ROW_NUMBER() OVER ( ORDER BY CaseTrsQuery.DocNo, CaseTrsQuery.Version, CaseDeadVersion.HeirIdNo asc ) AS RowNum
        //                            ,CASE 
        //                             WHEN CaseTrsQuery.Version = 0 THEN CaseTrsQuery.DocNo
        //                             ELSE CaseTrsQuery.DocNo +'-'+ CONVERT(nvarchar, CaseTrsQuery.Version) 
        //                            END DocNo    --案件編碼 
        //                            ,CaseTrsQuery.LetterNo as FileNo    --來文字號 
        //                            ,CaseTrsQuery.LetterDeptName as Govement    --來文機關 
        //                            ,CaseTrsQuery.MessageNo as GoFileNo    --回文字號
        //                            ,case when  LEN(CaseDeadVersion.HeirIdNo) > 3
        //                                  then SUBSTRING(CaseDeadVersion.HeirIdNo,1,LEN(CaseDeadVersion.HeirIdNo) -4) +  '***'     
        //                            	  + SUBSTRING(CaseDeadVersion.HeirIdNo,LEN(CaseDeadVersion.HeirIdNo),1)
        //                                  else CaseDeadVersion.HeirIdNo
        //                                  end as HeirIdNo --統一編號
        //                            ,isnull(CaseTrsQuery.CearanceUserID,'') + ' ' + isnull(CaseTrsQuery.CearanceUserName,'') as  SearchProgram   -- 放行主管
        //                            ,case when isnull (CaseTrsQuery.Status,'') = '66'
        //                            then N'已處理\n'+ CaseTrsQuery.ProcessingMode
        //                            else case when isnull(UploadStatus,'') = 'Y' then '已上傳' else '' end
        //                            end as Result    --處理方式
        //                            ,convert(nvarchar(10),CaseTrsQuery.CloseDate,111) as FinishDate    --結案日期
        //                        from CaseTrsQuery
        //                        left join CaseDeadVersion
        //                        on CaseDeadVersion.CaseTrsNewID = CaseTrsQuery.NewID
        //                        left join PARMCode
        //                        on CaseDeadVersion.Status = PARMCode.CodeNo
        //                        and CodeType = 'CaseTrsStatus'
        //                        WHERE 1=1  
        //                        and  CaseTrsQuery.NewID in(" + sqlWhere + ")  ");
        //        #endregion

        //        return base.Search(sql.ToString());
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}

        /// <summary>
        /// 匯出Excel
        /// </summary>
        /// <param name="model">查詢條件</param>
        /// <returns></returns>
        //public MemoryStream ExportExcel(string strNewID)
        //{
        //    IWorkbook workbook = new HSSFWorkbook();
        //    ISheet sheet = workbook.CreateSheet("歷史紀錄查詢與重送");

        //    // 查詢要匯出資料
        //    DataTable dt = GetHistoryQueryDdata(strNewID);

        //    #region  style
        //    // 標題
        //    ICellStyle styleHead12 = workbook.CreateCellStyle();
        //    IFont font12 = workbook.CreateFont();
        //    font12.FontHeightInPoints = 10;
        //    font12.FontName = "新細明體";
        //    styleHead12.FillPattern = FillPattern.SolidForeground;
        //    styleHead12.FillForegroundColor = HSSFColor.White.Index;
        //    styleHead12.BorderTop = BorderStyle.None;
        //    styleHead12.BorderLeft = BorderStyle.None;
        //    styleHead12.BorderRight = BorderStyle.None;
        //    styleHead12.BorderBottom = BorderStyle.None;
        //    styleHead12.WrapText = true;
        //    styleHead12.Alignment = HorizontalAlignment.Center;
        //    styleHead12.VerticalAlignment = VerticalAlignment.Center;
        //    styleHead12.SetFont(font12);

        //    // 居左
        //    ICellStyle styleHead10 = workbook.CreateCellStyle();
        //    IFont font10 = workbook.CreateFont();
        //    font10.FontHeightInPoints = 10;
        //    font10.FontName = "新細明體";
        //    styleHead10.FillPattern = FillPattern.SolidForeground;
        //    styleHead10.FillForegroundColor = HSSFColor.White.Index;
        //    styleHead10.BorderTop = BorderStyle.Thin;
        //    styleHead10.BorderLeft = BorderStyle.Thin;
        //    styleHead10.BorderRight = BorderStyle.Thin;
        //    styleHead10.BorderBottom = BorderStyle.Thin;
        //    styleHead10.WrapText = true;
        //    styleHead10.Alignment = HorizontalAlignment.Left;// 水平位置
        //    styleHead10.VerticalAlignment = VerticalAlignment.Center;
        //    styleHead10.SetFont(font10);

        //    // 居中
        //    ICellStyle styleCenter = workbook.CreateCellStyle();
        //    styleCenter.FillPattern = FillPattern.SolidForeground;
        //    styleCenter.FillForegroundColor = HSSFColor.White.Index;
        //    styleCenter.BorderTop = BorderStyle.Thin;
        //    styleCenter.BorderLeft = BorderStyle.Thin;
        //    styleCenter.BorderRight = BorderStyle.Thin;
        //    styleCenter.BorderBottom = BorderStyle.Thin;
        //    styleCenter.WrapText = true;
        //    styleCenter.Alignment = HorizontalAlignment.Center;// 水平位置
        //    styleCenter.VerticalAlignment = VerticalAlignment.Center;
        //    styleCenter.SetFont(font10);
        //    #endregion

        //    #region 設置單元格Width
        //    sheet.SetColumnWidth(0, 100 * 20);
        //    sheet.SetColumnWidth(1, 100 * 40);
        //    sheet.SetColumnWidth(2, 100 * 30);
        //    sheet.SetColumnWidth(3, 100 * 30);
        //    sheet.SetColumnWidth(4, 100 * 30);
        //    sheet.SetColumnWidth(5, 100 * 40);
        //    sheet.SetColumnWidth(6, 100 * 50);
        //    sheet.SetColumnWidth(7, 100 * 35);
        //    sheet.SetColumnWidth(8, 100 * 35);
        //    #endregion

        //    #region title
        //    //* line0
        //    SetExcelCell(sheet, 0, 0, styleHead12, "歷史紀錄查詢與重送");
        //    sheet.AddMergedRegion(new CellRangeAddress(0, 0, 0, 8));

        //    //* line2
        //    SetExcelCell(sheet, 2, 0, styleCenter, "序號");
        //    sheet.AddMergedRegion(new CellRangeAddress(2, 2, 0, 0));
        //    SetExcelCell(sheet, 2, 1, styleCenter, "案件編號");
        //    sheet.AddMergedRegion(new CellRangeAddress(2, 2, 1, 1));
        //    SetExcelCell(sheet, 2, 2, styleCenter, "來文字號");
        //    sheet.AddMergedRegion(new CellRangeAddress(2, 2, 2, 2));
        //    SetExcelCell(sheet, 2, 3, styleCenter, "來文機關");
        //    sheet.AddMergedRegion(new CellRangeAddress(2, 2, 3, 3));
        //    SetExcelCell(sheet, 2, 4, styleCenter, "回文字號");
        //    sheet.AddMergedRegion(new CellRangeAddress(2, 2, 4, 4));
        //    SetExcelCell(sheet, 2, 5, styleCenter, "統一編號");
        //    sheet.AddMergedRegion(new CellRangeAddress(2, 2, 5, 5));
        //    SetExcelCell(sheet, 2, 6, styleCenter, "放行主管");
        //    sheet.AddMergedRegion(new CellRangeAddress(2, 2, 6, 6));
        //    SetExcelCell(sheet, 2, 7, styleCenter, "處理方式");
        //    sheet.AddMergedRegion(new CellRangeAddress(2, 2, 7, 7));
        //    SetExcelCell(sheet, 2, 8, styleCenter, "結案日期");
        //    sheet.AddMergedRegion(new CellRangeAddress(2, 2, 8, 8));
        //    #endregion

        //    #region body

        //    string DocNo = String.Empty;
        //    int CountDocNo = 0;

        //    for (int iRow = 0; iRow < dt.Rows.Count; iRow++)
        //    {
        //      if (DocNo != dt.Rows[iRow]["DocNo"].ToString())
        //      {
        //        DocNo = dt.Rows[iRow]["DocNo"].ToString();
        //        CountDocNo++;
        //      }

        //      for (int iCol = 0; iCol < dt.Columns.Count; iCol++)
        //      {
        //          if (dt.Columns[iCol].ToString() == "RowNum" || dt.Columns[iCol].ToString() == "DocNo" || dt.Columns[iCol].ToString() == "FinishDate" || dt.Columns[iCol].ToString() == "LimitDate" || dt.Columns[iCol].ToString() == "HeirIdNo")
        //          {
        //              // 居中
        //              SetExcelCell(sheet, iRow + 3, iCol, styleCenter, dt.Rows[iRow][iCol].ToString());
        //          }
        //          else
        //          {
        //              // 居左
        //              SetExcelCell(sheet, iRow + 3, iCol, styleHead10, dt.Rows[iRow][iCol].ToString());
        //          }
        //      }
        //    }

        //    //* line1
        //    SetExcelCell(sheet, 1, 7, styleHead12, String.Format("案件總筆數：{0} 筆", CountDocNo));
        //    sheet.AddMergedRegion(new CellRangeAddress(1, 1, 7, 8));

        //    #endregion

        //    MemoryStream ms = new MemoryStream();
        //    workbook.Write(ms);
        //    ms.Flush();
        //    ms.Position = 0;
        //    workbook = null;
        //    return ms;
        //}

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
        //public bool EndCase(string strKEY, string LogonUser, string ProcessingMode)
        //{
        //    IDbConnection dbConnection = OpenConnection();
        //    IDbTransaction dbTrans = null;
        //    try
        //    {
        //        using (dbConnection)
        //        {
        //            dbTrans = dbConnection.BeginTransaction();

        //            string[] arrayKey = strKEY.Split(',');

        //            string delSql = "";
        //            base.Parameter.Clear();
        //            for (int i = 0; i < arrayKey.Length; i++)
        //            {
        //                #region  更新主檔
        //                delSql += @" 
        //                    update CaseTrsQuery
        //                    set 
        //                        Status = '66'
        //                        ,ProcessingMode = @ProcessingMode
        //                        ,CloseDate = getdate()
        //                        ,ModifiedDate = getdate()
        //                        ,ModifiedUser = @ModifiedUser
        //                    where NewID = @NewID" + i.ToString() + "; ";
        //                base.Parameter.Add(new CommandParameter("@NewID" + i.ToString(), arrayKey[i]));
        //                #endregion

        //                #region  更新子檔
        //                delSql += @" 
        //                    update CaseDeadVersion
        //                    set 
        //                        Status = '66'
        //                        ,ModifiedDate = getdate()
        //                        ,ModifiedUser = @ModifiedUser
        //                    where CaseTrsNewID = @CaseTrsNewID" + i.ToString() + "; ";
        //                base.Parameter.Add(new CommandParameter("@CaseTrsNewID" + i.ToString(), arrayKey[i]));
        //                #endregion
        //            }
        //            base.Parameter.Add(new CommandParameter("@ModifiedUser", LogonUser));
        //            base.Parameter.Add(new CommandParameter("@ProcessingMode", ProcessingMode));

        //            base.ExecuteNonQuery(delSql);

        //            dbTrans.Commit();
        //            return true;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        dbTrans.Rollback();

        //        return false;
        //    }
        //}

        /// <summary>
        /// 重查檢核
        /// </summary>
        /// <param name="pDocNo">案件編號</param>
        /// <param name="pVersion">版本號</param>
        /// <param name="pCaseStatus">案件狀態</param>
        /// <returns></returns>
 
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
                        from CaseDead Query
                        left join PARMCode
                        on CaseDead Query.Status = PARMCode.CodeNo
                        and CodeType = 'CaseTrsStatus'
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
        //public bool SearchAgain(string pDocNo, string pVersion, string pCaseStatus, string pCountDocNo, User logonUser, string strFilePath)
        //{
        //    // 當前登錄者
        //    string pUserAccount = logonUser.Account;

        //    IDbConnection dbConnection = OpenConnection();
        //    IDbTransaction dbTrans = null;
        //    try
        //    {
        //        using (dbConnection)
        //        {
        //            // 將數據塞進DataTable中
        //            DataTable dt = ChangeDataTable(pDocNo, pVersion, pCaseStatus, pCountDocNo);

        //            // 根據案件編號篩選重複資料
        //            DataView dataView = dt.DefaultView;
        //            DataTable dtDistinct = dataView.ToTable(true, "DocNo");

        //            // 清空參數
        //            base.Parameter.Clear();

        //            for (int i = 0; i < dtDistinct.Rows.Count; i++)
        //            {
        //               DataTable dtCaseTrsQuery = GetCaseTrsQueryByDocNo(dtDistinct.Rows[i]["DocNo"].ToString());

        //                // 取同案件下最大版本號
        //                DataRow dR = dtCaseTrsQuery.Select("DocNo ='" + dtDistinct.Rows[i]["DocNo"] + "'", "Version DESC")[0];

        //                // 案件狀態
        //                string strCaseStatus = dR["Status"].ToString();
        //                string strCloseDate = dR["CloseDate"].ToString();

        //                dbTrans = dbConnection.BeginTransaction();

        //                // 01：未處理
        //                // 02：拋查中
        //                // 03：成功
        //                // 04：失敗
        //                // 06：重查拋查中
        //                // 07：重查成功
        //                // 08：重查失敗
        //                // 66：已處理 / 強制結案
        //                // 成功案件且已結案之重查，所有發查table資料重新insert
        //                if ((strCaseStatus == "03" || strCaseStatus == "07" || strCaseStatus == "66") && !string.IsNullOrEmpty(strCloseDate))
        //                {
        //                    Guid pkNewID = Guid.NewGuid();

        //                    // 成功和已處理案件重查,版本加1
        //                    InsertCaseTrsQuery(dR["DocNo"].ToString(), dR["Version"].ToString(), pUserAccount, pkNewID, dbTrans);

        //                    // 取得要INSERT 到CaseDeadVersion的資料
        //                    IList<CaseDeadVersion> ilistCaseDeadVersion = GetCaseDeadVersion(dR["DocNo"].ToString(), dR["Version"].ToString(), pUserAccount, pkNewID, dbTrans);

        //                    // Insert 資料到CaseDeadVersion&CaseTrsRFDMSend&BOPS067050Send&BOPS060628Send
        //                    InsertVersionAndSend(ilistCaseDeadVersion, logonUser, dbTrans);
        //                }
        //                else
        //                {
        //                    #region 以原案件編號重查

        //                    // 更新CaseTrsQuery
        //                    UpdateCaseTrsQuery(dR["DocNo"].ToString(), dR["Version"].ToString(), pUserAccount, dbTrans);

        //                    // 更新CaseDeadVersion
        //                    UpdateCaseDeadVersion(dR["DocNo"].ToString(), dR["Version"].ToString(), pUserAccount, dbTrans);

        //                    //  刪除有關Send &Recv&ApprMsgKey Table 資料
        //                    DeleteTables(dR["DocNo"].ToString(), dR["Version"].ToString(), dbTrans);

        //                    //  Insert有關Send &ApprMsgKey Table 資料
        //                    InsertTables(dR["DocNo"].ToString(), dR["Version"].ToString(), logonUser, dbTrans);

        //                    #endregion

        //                    #region 刪除產出的回文資料

        //                   /*
        //                    // 回文檔名稱（存款帳戶開戶資料）
        //                    if (System.IO.File.Exists(strFilePath + dR["ROpenFileName"].ToString()))
        //                    {
        //                        FileInfo filedi = new FileInfo(strFilePath + dR["ROpenFileName"].ToString());
        //                        filedi.Delete();
        //                    }

        //                    // 回文檔名稱（存款往來明細資料）
        //                    if (System.IO.File.Exists(strFilePath + dR["RFileTransactionFileName"].ToString()))
        //                    {
        //                        FileInfo filedi = new FileInfo(strFilePath + dR["RFileTransactionFileName"].ToString());
        //                        filedi.Delete();
        //                    }

        //                    // 回文首頁（第一頁PDF）
        //                    if (System.IO.File.Exists(strFilePath + dR["DocNo"].ToString() + "_" + dR["Version"].ToString() + "_001.pdf"))
        //                    {
        //                        FileInfo filedi = new FileInfo(strFilePath + dR["DocNo"].ToString() + "_" + dR["Version"].ToString() + "_001.pdf");
        //                        filedi.Delete();
        //                    }

        //                    // 回文首頁(ALL PDF)
        //                    if (System.IO.File.Exists(strFilePath + dR["DocNo"].ToString() + "_" + dR["Version"].ToString() + ".pdf"))
        //                    {
        //                        FileInfo filedi = new FileInfo(strFilePath + dR["DocNo"].ToString() + "_" + dR["Version"].ToString() + ".pdf");
        //                        filedi.Delete();
        //                    }
        //                    */

        //                    #endregion
        //                }

        //                dbTrans.Commit();
        //            }

        //            return true;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        dbTrans.Rollback();

        //        return false;
        //    }
        //}

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
                            from  CaseDeadVersion
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

        public int Delete(String NewId)
        {
            try
            {

                string sql = @"Delete from CaseDeadVersion
                            where NewId   ='" + NewId + "' ";

                DataTable dt = base.Search(sql);

                if (dt != null && dt.Rows.Count > 0)
                {
                    return dt.Rows.Count;
                }
                else
                {
                    return 0;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        #region 重查

        /// <summary>
        ///  複製最大版本號的資料（CaseDead Query）
        /// </summary>
        /// <param name="pDocNo">案件編號</param>
        /// <param name="pVersion">版本號</param>
        /// <param name="LogonUser">當前登錄者</param>
        /// <param name="dbTransactionn"></param>
        public void InsertCaseDeadQuery(string pDocNo, string pVersion, string LogonUser, Guid pNewID, IDbTransaction dbTransactionn)
        {

            string sqlInsert = @"INSERT  INTO dbo.CaseDeadQuery
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
                                             FROM    dbo.CaseDead Query
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
        /// 
        /// </summary>
        /// <param name="strMaxNo"></param>
        /// <returns></returns>
        private string CalculateTrnNum(int strMaxNo)
        {
            return DateTime.Now.ToString("yyyyMMdd") + String.Format("{0:D5}", strMaxNo + 1);
        }

        /// <summary>
        /// 根據案件編號&版本號更新CaseDead Query
        /// </summary>
        /// <param name="pDocNo">案件編號</param>
        /// <param name="pVersion">版本號</param>
        /// <param name="LogonUser">當前登錄者</param>
        /// <param name="dbTransactionn"></param>
        public void UpdateCaseDeadQuery(string pDocNo, string pVersion, string LogonUser, IDbTransaction dbTransactionn)
        {
            // 更新SQL
            string sqlUpdate = @"UPDATE  CaseDead Query
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
        //public void UpdateCaseDeadVersion(string pDocNo, string pVersion, string LogonUser, IDbTransaction dbTransactionn)
        //{
        //    // 更新SQL
        //    string sqlUpdate = @"UPDATE  CaseDeadVersion
        //                           SET     Status = '06' ,
        //                                   HTGSendStatus = ( CASE WHEN OpenFlag = 'Y' THEN '2' ELSE NULL END ) ,
        //                                   HTGQryMessage = ( CASE WHEN OpenFlag = 'Y' THEN '' ELSE NULL END ) ,
        //                                   HTGModifiedDate = (CASE WHEN OpenFlag = 'Y' THEN GETDATE()  ELSE NULL END ),
        //                                   RFDMSendStatus = ( CASE WHEN TransactionFlag = 'Y' THEN '2' ELSE NULL END ) ,
        //                                   RFDMQryMessage = ( CASE WHEN TransactionFlag = 'Y' THEN '2' ELSE NULL END ) ,
        //                                   RFDModifiedDate = ( CASE WHEN TransactionFlag = 'Y' THEN GETDATE() ELSE NULL END ) ,
        //                                   ModifiedDate = GETDATE(),
        //                                   ModifiedUser = @ModifiedUser
        //                           WHERE
        //                                  CaseTrsNewID = (SELECT NEWID FROM CaseTrsQuery WHERE DocNo = @DocNo AND Version = @Version)";

        //    base.Parameter.Clear();

        //    base.Parameter.Add(new CommandParameter("@ModifiedUser", LogonUser));
        //    base.Parameter.Add(new CommandParameter("@DocNo", pDocNo));
        //    base.Parameter.Add(new CommandParameter("@Version", pVersion));

        //    base.ExecuteNonQuery(sqlUpdate, dbTransactionn);
        //}

        /// <summary>
        /// 刪除有關Table 資料
        /// </summary>
        /// <param name="pDocNo">案件編號</param>
        /// <param name="pVersion">版本號</param>
        /// <param name="dbTransactionn"></param>
        public void DeleteTables(string pDocNo, string pVersion, IDbTransaction dbTransactionn)
        {
            #region CaseTrsRFDM

            string sqlDelete = @" DELETE FROM CaseTrsRFDMSend
                                             WHERE  VersionNewID IN (
                                                    SELECT  NewID
                                                    FROM    CaseDeadVersion
                                                    WHERE   CaseTrsNewID = ( SELECT    NEWID
                                                                              FROM      CaseTrsQuery
                                                                              WHERE     DocNo = @DocNo
                                                                                        AND Version = @Version
                                                                            ) )";

            sqlDelete += @"DELETE FROM CaseTrsRFDMRecv
                                             WHERE  VersionNewID IN (
                                                    SELECT  NewID
                                                    FROM    CaseDeadVersion
                                                    WHERE   CaseTrsNewID = ( SELECT    NEWID
                                                                              FROM      CaseTrsQuery
                                                                              WHERE     DocNo = @DocNo
                                                                                        AND Version = @Version
                                                                            ) )";

            #endregion

            #region BOPS067050

            sqlDelete += @"DELETE FROM BOPS067050Send
                                             WHERE  VersionNewID IN (
                                                    SELECT  NewID
                                                    FROM    CaseDeadVersion
                                                    WHERE   CaseTrsNewID = ( SELECT    NEWID
                                                                              FROM      CaseTrsQuery
                                                                              WHERE     DocNo = @DocNo
                                                                                        AND Version = @Version
                                                                            ) )";

            sqlDelete += @"DELETE FROM BOPS067050Recv
                                             WHERE  VersionNewID IN (
                                                    SELECT  NewID
                                                    FROM    CaseDeadVersion
                                                    WHERE   CaseTrsNewID = ( SELECT    NEWID
                                                                              FROM      CaseTrsQuery
                                                                              WHERE     DocNo = @DocNo
                                                                                        AND Version = @Version
                                                                            ) )";

            #endregion

            #region BOPS060628

            sqlDelete += @"DELETE FROM BOPS060628Send
                                             WHERE  VersionNewID IN (
                                                    SELECT  NewID
                                                    FROM    CaseDeadVersion
                                                    WHERE   CaseTrsNewID = ( SELECT    NEWID
                                                                              FROM      CaseTrsQuery
                                                                              WHERE     DocNo = @DocNo
                                                                                        AND Version = @Version
                                                                            ) )";

            sqlDelete += @"DELETE FROM BOPS060628Recv
                                             WHERE  VersionNewID IN (
                                                    SELECT  NewID
                                                    FROM    CaseDeadVersion
                                                    WHERE   CaseTrsNewID = ( SELECT    NEWID
                                                                              FROM      CaseTrsQuery
                                                                              WHERE     DocNo = @DocNo
                                                                                        AND Version = @Version
                                                                            ) )";

            #endregion

            #region BOPS060490

            sqlDelete += @"DELETE FROM BOPS060490Send
                                             WHERE  VersionNewID IN (
                                                    SELECT  NewID
                                                    FROM    CaseDeadVersion
                                                    WHERE   CaseTrsNewID = ( SELECT    NEWID
                                                                              FROM      CaseTrsQuery
                                                                              WHERE     DocNo = @DocNo
                                                                                        AND Version = @Version
                                                                            ) )";

            sqlDelete += @"DELETE FROM BOPS060490Recv
                                             WHERE  VersionNewID IN (
                                                    SELECT  NewID
                                                    FROM    CaseDeadVersion
                                                    WHERE   CaseTrsNewID = ( SELECT    NEWID
                                                                              FROM      CaseTrsQuery
                                                                              WHERE     DocNo = @DocNo
                                                                                        AND Version = @Version
                                                                            ) )";

            #endregion

            #region BOPS000401

            sqlDelete += @"DELETE FROM BOPS000401Send
                                             WHERE  VersionNewID IN (
                                                    SELECT  NewID
                                                    FROM    CaseDeadVersion
                                                    WHERE   CaseTrsNewID = ( SELECT    NEWID
                                                                              FROM      CaseTrsQuery
                                                                              WHERE     DocNo = @DocNo
                                                                                        AND Version = @Version
                                                                            ) )";

            sqlDelete += @"DELETE FROM BOPS000401Recv
                                             WHERE  VersionNewID IN (
                                                    SELECT  NewID
                                                    FROM    CaseDeadVersion
                                                    WHERE   CaseTrsNewID = ( SELECT    NEWID
                                                                              FROM      CaseTrsQuery
                                                                              WHERE     DocNo = @DocNo
                                                                                        AND Version = @Version
                                                                            ) )";

            #endregion

            #region ApprMsgKey

            sqlDelete += @"DELETE FROM ApprMsgKey
                                             WHERE  VersionNewID IN (
                                                    SELECT  NewID
                                                    FROM    CaseDeadVersion
                                                    WHERE   CaseTrsNewID = ( SELECT    NEWID
                                                                              FROM      CaseTrsQuery
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
                                         HeirIdNo ,
                                         OpenFlag ,
QDateS,
QDateE,
                                         TransactionFlag
                                 FROM    CaseDeadVersion
                                 WHERE   CaseTrsNewID = ( SELECT    NEWID
                                                           FROM      CaseTrsQuery
                                                           WHERE     DocNo = @DocNo
                                                                     AND Version = @Version
                                                         )";

            base.Parameter.Add(new CommandParameter("@DocNo", pDocNo));
            base.Parameter.Add(new CommandParameter("@Version", pVersion));

            IList<CaseDeadVersion> list = base.SearchList<CaseDeadVersion>(sqlSelect.ToString(), dbTransactionn);

            #endregion

            //  取得當前登錄者
            string pUserAccount = logonUser.Account;

            if (list != null && list.Count > 0)
            {
                foreach (CaseDeadVersion item in list)
                {
                    base.Parameter.Clear();

                    base.Parameter.Add(new CommandParameter("@ModifiedUser", pUserAccount));
                    base.Parameter.Add(new CommandParameter("@VersionNewID", item.NewID));
                    base.Parameter.Add(new CommandParameter("@CustID", item.HeirId));

                    //if (item.TransactionFlag == "Y")
                    //{
                    //    #region insert CaseTrsRFDMSend

                    //    string dtQDateS = "";
                    //    string dtQDateE = "";

                    //    if (item.QDateS != null && item.QDateS.Trim() != "")
                    //    {
                    //        dtQDateS = item.QDateS.Substring(0, 4) + "/" + item.QDateS.Substring(4, 2) + "/" + item.QDateS.Substring(6, 2);
                    //    }

                    //    if (item.QDateE != null && item.QDateE.Trim() != "")
                    //    {
                    //        dtQDateE = item.QDateE.Substring(0, 4) + "/" + item.QDateE.Substring(4, 2) + "/" + item.QDateE.Substring(6, 2);
                    //    }

                    //    // 查詢本月最大的流水號
                    //    string pMaxTrnNum = GetMaxTrnNum(dbTransactionn);

                    //    // 流水號變量
                    //    int pTrnNum = 0;

                    //    // 截取流水號
                    //    if (pMaxTrnNum != "")
                    //    {
                    //        pTrnNum = Convert.ToInt32(pMaxTrnNum.Substring(8, 5));
                    //    }

                    //    string sqlCaseTrsRFDMSend = "";

                    //    #region Insert RFDMSend

                    //    sqlCaseTrsRFDMSend += @" insert CaseTrsRFDMSend
                    //                         (
                    //                         TrnNum, VersionNewID, ID_No, Type, RFDMSendStatus, CreatedUser, CreatedDate, ModifiedUser, ModifiedDate,acctDesc,curr,channel,Start_Jnrst_Date,End_Jnrst_Date
                    //                         )
                    //                         VALUES  (
                    //                            @TrnNumS , 
                    //                            @VersionNewID , 
                    //                            @HeirIdNo 
                    //                         , '0'
                    //                         , '02'
                    //                         , @ModifiedUser
                    //                         , getdate()
                    //                         , @ModifiedUser
                    //                         , getdate()
                    //                         ,'S','TWD','CSFS' ";

                    //    if (dtQDateS != "")
                    //    {
                    //        sqlCaseTrsRFDMSend += ",'" + dtQDateS + "'";
                    //    }
                    //    else
                    //    {
                    //        sqlCaseTrsRFDMSend += ",NULL";
                    //    }
                    //    if (dtQDateE != "")
                    //    {
                    //        sqlCaseTrsRFDMSend += ",'" + dtQDateE + "'";
                    //    }
                    //    else
                    //    {
                    //        sqlCaseTrsRFDMSend += ",NULL";
                    //    }

                    //    sqlCaseTrsRFDMSend += " ); ";

                    //    sqlCaseTrsRFDMSend += @" insert CaseTrsRFDMSend
                    //                         (
                    //                         TrnNum, VersionNewID, ID_No, Type, RFDMSendStatus, CreatedUser, CreatedDate, ModifiedUser, ModifiedDate,acctDesc,curr,channel,Start_Jnrst_Date,End_Jnrst_Date
                    //                         )
                    //                             VALUES  (
                    //                            @TrnNumQ , 
                    //                            @VersionNewID , 
                    //                            @HeirIdNo 
                    //                         , '0'
                    //                         , '02'
                    //                         , @ModifiedUser
                    //                         , getdate()
                    //                         , @ModifiedUser
                    //                         , getdate()
                    //                         ,'Q','TWD','CSFS' ";

                    //    if (dtQDateS != "")
                    //    {
                    //        sqlCaseTrsRFDMSend += ",'" + dtQDateS + "'";
                    //    }
                    //    else
                    //    {
                    //        sqlCaseTrsRFDMSend += ",NULL";
                    //    }
                    //    if (dtQDateE != "")
                    //    {
                    //        sqlCaseTrsRFDMSend += ",'" + dtQDateE + "'";
                    //    }
                    //    else
                    //    {
                    //        sqlCaseTrsRFDMSend += ",NULL";
                    //    }

                    //    sqlCaseTrsRFDMSend += " ); ";

                    //    sqlCaseTrsRFDMSend += @" insert CaseTrsRFDMSend
                    //                         (
                    //                         TrnNum, VersionNewID, ID_No, Type, RFDMSendStatus, CreatedUser, CreatedDate, ModifiedUser, ModifiedDate,acctDesc,curr,channel,Start_Jnrst_Date,End_Jnrst_Date
                    //                         )
                    //                            VALUES  (
                    //                            @TrnNumT , 
                    //                            @VersionNewID , 
                    //                            @HeirIdNo 
                    //                         , '0'
                    //                         , '02'
                    //                         , @ModifiedUser
                    //                         , getdate()
                    //                         , @ModifiedUser
                    //                         , getdate()
                    //                         ,'T','TWD','CSFS' ";

                    //    if (dtQDateS != "")
                    //    {
                    //        sqlCaseTrsRFDMSend += ",'" + dtQDateS + "'";
                    //    }
                    //    else
                    //    {
                    //        sqlCaseTrsRFDMSend += ",NULL";
                    //    }
                    //    if (dtQDateE != "")
                    //    {
                    //        sqlCaseTrsRFDMSend += ",'" + dtQDateE + "'";
                    //    }
                    //    else
                    //    {
                    //        sqlCaseTrsRFDMSend += ",NULL";
                    //    }

                    //    sqlCaseTrsRFDMSend += " ); ";

                    //    base.Parameter.Add(new CommandParameter("@TrnNumS", CalculateTrnNum(pTrnNum)));
                    //    pTrnNum++;

                    //    base.Parameter.Add(new CommandParameter("@TrnNumQ", CalculateTrnNum(pTrnNum)));
                    //    pTrnNum++;

                    //    base.Parameter.Add(new CommandParameter("@TrnNumT", CalculateTrnNum(pTrnNum)));
                    //    pTrnNum++;

                    //    #endregion


                    //    base.ExecuteNonQuery(sqlCaseTrsRFDMSend, dbTransactionn);

                    //    #endregion
                    //}

                    //if (item.OpenFlag == "Y")
                    //{
                    //    #region Insert BOPS067050Send

                    //    string sqlBOPS067050Send = @" INSERT INTO dbo.BOPS067050Send
                    //                                        ( NewID ,
                    //                                          VersionNewID ,
                    //                                          HeirIdNo ,
                    //                                          Optn ,
                    //                                          CreatedUser ,
                    //                                          CreatedDate ,
                    //                                          ModifiedUser ,
                    //                                          ModifiedDate ,
                    //                                          SendStatus 
                    //                                        )
                    //                                VALUES  (  NEWID() , -- NewID - uniqueidentifier
                    //                                          @VersionNewID , -- VersionNewID - uniqueidentifier
                    //                                          @HeirIdNo , -- HeirIdNo - nvarchar(14)
                    //                                          'D' , -- Optn - nvarchar(1)
                    //                                          @ModifiedUser , -- CreatedUser - nvarchar(50)
                    //                                          GETDATE() , -- CreatedDate - datetime
                    //                                          @ModifiedUser , -- ModifiedUser - nvarchar(50)
                    //                                          GETDATE() , -- ModifiedDate - datetime
                    //                                          '02'  -- SendStatus - char(2)
                    //                                        )";

                    //    base.ExecuteNonQuery(sqlBOPS067050Send, dbTransactionn);

                    //    #endregion

                    //    #region Insert BOPS060628Send

                    //    string sqlBOPS060628Send = @" INSERT INTO dbo.BOPS060628Send
                    //                                        ( NewID ,
                    //                                          HeirIdNo ,
                    //                                          CreatedUser ,
                    //                                          CreatedDate ,
                    //                                          ModifiedUser ,
                    //                                          ModifiedDate ,
                    //                                          SendStatus ,
                    //                                          VersionNewID
                    //                                        )
                    //                                VALUES  ( NEWID() , -- NewID - uniqueidentifier
                    //                                          @HeirIdNo , -- HeirIdNo - nvarchar(14)
                    //                                          @ModifiedUser, -- CreatedUser - nvarchar(50)
                    //                                           GETDATE() , -- CreatedDate - datetime
                    //                                          @ModifiedUser , -- ModifiedUser - nvarchar(50)
                    //                                          GETDATE() , -- ModifiedDate - datetime
                    //                                          '02' , -- SendStatus - char(2)
                    //                                          @VersionNewID  -- VersionNewID - uniqueidentifier
                    //                                        )";

                    //    base.ExecuteNonQuery(sqlBOPS060628Send, dbTransactionn);

                    //    #endregion}
                    //}

                    #region Insert ApprMsgKey

                    ApprMsgKeyVO model = new ApprMsgKeyVO();
                    model.MsgUID = logonUser.Account;
                    model.MsgKeyLP = logonUser.LDAPPwd;
                    model.MsgKeyLU = logonUser.Account;
                    model.MsgKeyRU = logonUser.RCAFAccount;
                    model.MsgKeyRP = logonUser.RCAFPs;
                    model.MsgKeyRB = logonUser.RCAFBranch;
                    model.VersionNewID = item.NewID;

                    pCaseDeadBIZ.InsertApprMsgKey(model, dbTransactionn);

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
                            from CaseTrsRFDMSend
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

 

        #endregion

    }
}
