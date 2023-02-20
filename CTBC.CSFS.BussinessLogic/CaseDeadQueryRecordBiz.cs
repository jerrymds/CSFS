using CTBC.CSFS.Models;
using CTBC.CSFS.Pattern;
using Microsoft.VisualBasic;
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
using NPOI.XSSF.UserModel;
namespace CTBC.CSFS.BussinessLogic
{
    public class CaseDeadQueryRecordBIZ : CommonBIZ
    {
        CaseDeadCommonBIZ pCaseDeadCommonBIZ = new CaseDeadCommonBIZ();

        CaseDeadBIZ pCaseDeadBIZ = new CaseDeadBIZ();

        /// <summary>
        /// 歷史記錄查詢與重送清單資料
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


                // 客戶ID
                if (!string.IsNullOrEmpty(model.HeirId))
                {
                    //sqlWhere += @" AND t.HeirId like @HeirId";
                    sqlWhere += @" AND t.HeirId like  '%" + model.HeirId+"%' ";
                    //base.Parameter.Add(new CommandParameter("@HeirId", "'%" + model.HeirId + "%'"));
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
   (Select  ROW_NUMBER() OVER ( ORDER BY DocNo desc ) AS RowNum,DocNo
	  ,SetStatus
	  ,SendStatus
      ,Excel_SETUP
      ,CreatedDate
      ,CreatedUser
      ,CaseTrsNewID 
	  ,NewID
	  ,(select top 1 CodeDesc from ParmCode where Codetype = 'CaseDeadSetStatus' and codeno = SetStatus) as statusname1
	  ,(select top 1 CodeDesc from ParmCode where Codetype = 'CaseDeadSendStatus' and codeno = SendStatus) as statusname2
	  ,(select top 1 CodeDesc from ParmCode where Codetype = 'CaseCustStatus' and codeno = Status) as statusname
	   From
	   (select 	   
	  t.DocNo
      ,t.Status
	  ,t.SetStatus
	  ,t.SendStatus
      ,(select top 1 FileName from CaseEdocFile where CaseId = m.CaseID and FileType = 'xlsx3' ) as Excel_SETUP
      ,t.CreatedDate
      ,t.CreatedUser
      ,t.CaseTrsNewID 
	  ,t.NewId
	,ROW_NUMBER() Over (Partition By t.DocNo,CONVERT(Varchar(10),t.CreatedDate,111) Order By CONVERT(Varchar(10),t.CreatedDate,111),t.status ) As Sort From  CaseDeadVersion
 t
	inner join
   CaseMaster m
  on  t.CaseTrsNewID = m.CaseID 
WHERE 1 =1 " + sqlWhere + @"
   ) as tabletemp  where Sort=1 ");

                // 判斷是否分頁
                sql.Append(@" ) as tmp WHERE  RowNum > " + PageSize * (pageNum - 1)
                                   + " AND RowNum < " + ((PageSize * pageNum) + 1)+ "   order by DocNo desc");

                #endregion

                // 資料總筆數
                string sqlCount = @"
                                select count(0)
                                from 
                                  ( select docno,CONVERT(Varchar(10),CaseDeadVersion
.CreatedDate,111) as CreatedDate,Status,HeirId,CreatedUser
,ROW_NUMBER() Over (Partition By CaseDeadVersion
.DocNo,CONVERT(Varchar(10),CaseDeadVersion
.CreatedDate,111) Order By CaseDeadVersion
.CreatedDate Desc) As Sort From  CaseDeadVersion
 ) as t  WHERE Sort =1  " + sqlWhere ;

   
                base.DataRecords = int.Parse(base.ExecuteScalar(sqlCount).ToString());

                // 查詢清單資料
                IList< CaseDeadVersion> _ilsit = base.SearchList< CaseDeadVersion> (sql.ToString());
                DataTable dt = new DataTable();
                string sw = ""; 
                if (_ilsit.Count > 0)
                {
                    for (int i=0;i< _ilsit.Count;i++)
                    {
                        sw = "";
                        dt=base.Search("Select SetStatus,Count(SetStatus) as ct  into #temp from CaseDeadVersion where DocNo='" + _ilsit[i].DocNo + "' group by SetStatus  order by SetStatus select * from #temp  drop table #temp ");
                        if (dt.Rows.Count > 0)
                        {
                            for (int j = 0; j < dt.Rows.Count; j++)
                            {
                                switch (dt.Rows[j][0].ToString().Trim())
                                {
                                    case "0":
                                        if (Convert.ToInt32(dt.Rows[j][1].ToString()) > 0)
                                        {
                                            sw = "0";
                                            _ilsit[i].SetStatus = "0";
                                            _ilsit[i].StatusName1 = "待處理";
                                        }
                                        break;
                                    case "1":
                                        if (Convert.ToInt32(dt.Rows[j][1].ToString()) > 0)
                                        {
                                                 sw = "1";
                                                _ilsit[i].SetStatus = "1";
                                                _ilsit[i].StatusName1 = "處理中";
                                        }
                                        break;
                                    case "3":
                                        if (Convert.ToInt32(dt.Rows[j][1].ToString()) > 0)
                                        {
                                            if (sw != "0" && sw != "1")
                                            {
                                                sw = "3";
                                                _ilsit[i].SetStatus = "3";
                                                _ilsit[i].StatusName1 = "失敗";
                                            }
                                        }
                                        break;
                                    case "2":
                                        if (Convert.ToInt32(dt.Rows[j][1].ToString()) > 0)
                                        {
                                            if (sw != "0" && sw != "1" && sw != "3")
                                            {
                                                sw = "2";
                                                _ilsit[i].SetStatus = "2";
                                                _ilsit[i].StatusName1 = "成功";
                                            }
                                        }
                                        break;
                                }
                         }
                     }

                   }
                    dt.Dispose();
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

        public int UpdateCaseDeadExcelSetup(string Content)
        {
            //string Edocpath = EdocFilePath;//檔案的目錄路徑
            //string pdfDocDirPath = Edocpath + "\\" + Content;
                    IDbConnection dbConnection = OpenConnection();
            IDbTransaction dbTrans = null;
            try
            {
                using (dbConnection)
                {
                    dbTrans = dbConnection.BeginTransaction();

                    string sql = "";
                    sql = " Update   CaseDeadVersion set EXCEL_SETUP = 'Y' , ModifiedDate = getdate()  where  DocNo = '" + Content + "' ; ";
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

        public int InsertCaseEdocFile(CaseEdocFile caseEdocFile)
        {
            try
            {
                int rtn = 0;
                string sql = @"insert into CaseEdocFile values(@CaseId,@Type,@FileType,@FileName,@FileObject,@SendNo)";
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@CaseId", caseEdocFile.CaseId));
                base.Parameter.Add(new CommandParameter("@Type", caseEdocFile.Type));
                base.Parameter.Add(new CommandParameter("@FileType", caseEdocFile.FileType));
                base.Parameter.Add(new CommandParameter("@FileName", caseEdocFile.FileName));
                base.Parameter.Add(new CommandParameter("@SendNo", caseEdocFile.SendNo));
                base.Parameter.Add(new CommandParameter("@FileObject", caseEdocFile.FileObject, SqlDbType.VarBinary, 0));
                rtn = base.ExecuteNonQuery(sql);
                return rtn;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public int DeleteCaseEdocFile(Guid caseid)
        {
            try
            {
                int rtn = 0;
                string sql = @"delete CaseEdocFile where CaseId=@CaseId and FileType='pdf' and Type='歷史'";
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@CaseId", caseid));
                rtn = base.ExecuteNonQuery(sql);
                return rtn;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public int UpdateCaseDeadSendStatus(string Content)
        {
            IDbConnection dbConnection = OpenConnection();
            IDbTransaction dbTrans = null;
            try
            {
                using (dbConnection)
                {
                    dbTrans = dbConnection.BeginTransaction();

                    string sql = "";
                    sql = " Update   CaseDeadVersion set SendStatus = '9' , ModifiedDate = getdate()  where  DocNo = '" + Content + "' ; ";
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
        public int EditCaseTrsQuery(string Content)
        {
            IDbConnection dbConnection = OpenConnection();
            IDbTransaction dbTrans = null;
            try
            {
                using (dbConnection)
                {
                    dbTrans = dbConnection.BeginTransaction();

                    string sql = "";
                    sql  = " Update CaseDeadVersion set SendStatus = '3' WHERE  SendStatus = '0' and CONVERT(VARCHAR(10),ModifiedDate,111) = CONVERT(VARCHAR(10),getdate(),111) ;Update CaseDeadVersion set SendStatus = '0' , ModifiedDate = getdate()  where   DocNo = '" + Content + "' ; ";
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
        /// 刪除清單資料
        /// </summary>
        /// <param name="Content"></param>
        /// <returns></returns>
        public int DeleteCaseTrsQuery(string Content)
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
                        //sql += " delete from CaseDeadVersion  where Status Not in   ('03')  and CaseTrsNewID = '" + arrayNewID[i].ToString() + "' ; ";
                        sql += " delete from CaseDeadVersion  where  CaseTrsNewID = '" + caseid + "' and  CONVERT(VARCHAR(10), CreatedDate,111) = '" + cdate + "'";
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

        /// <summary>Convierte un DataTable en un archivo de Excel (xls o Xlsx) y lo guarda en disco.</summary>
        /// <param name="pDatos">Datos de la Tabla a guardar. Usa el nombre de la tabla como nombre de la Hoja</param>
        /// <param name="pFilePath">Ruta del archivo donde se guarda.</param>
        private void DataTable_To_Excel(DataTable pDatos, string pFilePath)
        {
            try
            {
                if (pDatos != null && pDatos.Rows.Count > 0)
                {
                    IWorkbook workbook = null;
                    ISheet worksheet = null;

                    using (FileStream stream = new FileStream(pFilePath, FileMode.Create, FileAccess.ReadWrite))
                    {
                        string Ext = System.IO.Path.GetExtension(pFilePath); //<-Extension del archivo
                        switch (Ext.ToLower())
                        {
                            case ".xls":
                                HSSFWorkbook workbookH = new HSSFWorkbook();
                                NPOI.HPSF.DocumentSummaryInformation dsi = NPOI.HPSF.PropertySetFactory.CreateDocumentSummaryInformation();
                                dsi.Company = "中國信託"; dsi.Manager = "外來文系統";
                                workbookH.DocumentSummaryInformation = dsi;
                                workbook = workbookH;
                                break;

                            case ".xlsx": workbook = new XSSFWorkbook(); break;
                        }

                        worksheet = workbook.CreateSheet(pDatos.TableName); 

                        int iRow = 0;
                        if (pDatos.Columns.Count > 0)
                        {
                            int iCol = 0;
                            IRow fila = worksheet.CreateRow(iRow);
                            foreach (DataColumn columna in pDatos.Columns)
                            {
                                ICell cell = fila.CreateCell(iCol, CellType.String);
                                cell.SetCellValue(columna.ColumnName);
                                iCol++;
                            }
                            iRow++;
                        }

                        ICellStyle _doubleCellStyle = workbook.CreateCellStyle();
                        _doubleCellStyle.DataFormat = workbook.CreateDataFormat().GetFormat("#,##0.###");

                        ICellStyle _intCellStyle = workbook.CreateCellStyle();
                        _intCellStyle.DataFormat = workbook.CreateDataFormat().GetFormat("#,##0");

                        ICellStyle _boolCellStyle = workbook.CreateCellStyle();
                        _boolCellStyle.DataFormat = workbook.CreateDataFormat().GetFormat("BOOLEAN");

                        ICellStyle _dateCellStyle = workbook.CreateCellStyle();
                        _dateCellStyle.DataFormat = workbook.CreateDataFormat().GetFormat("yyyy-MM-dd");

                        ICellStyle _dateTimeCellStyle = workbook.CreateCellStyle();
                        _dateTimeCellStyle.DataFormat = workbook.CreateDataFormat().GetFormat("yyyy-MM-dd HH:mm:ss");

                        //AHORA CREAR UNA FILA POR CADA REGISTRO DE LA TABLA
                        foreach (DataRow row in pDatos.Rows)
                        {
                            IRow fila = worksheet.CreateRow(iRow);
                            int iCol = 0;
                            foreach (DataColumn column in pDatos.Columns)
                            {
                                ICell cell = null; //<-Representa la celda actual                               
                                object cellValue = row[iCol]; //<- El valor actual de la celda

                                switch (column.DataType.ToString())
                                {
                                    case "System.Boolean":
                                        if (cellValue != DBNull.Value)
                                        {
                                            cell = fila.CreateCell(iCol, CellType.Boolean);

                                            if (Convert.ToBoolean(cellValue)) { cell.SetCellFormula("TRUE()"); }
                                            else { cell.SetCellFormula("FALSE()"); }

                                            cell.CellStyle = _boolCellStyle;
                                        }
                                        break;

                                    case "System.String":
                                        if (cellValue != DBNull.Value)
                                        {
                                            cell = fila.CreateCell(iCol, CellType.String);
                                            cell.SetCellValue(Convert.ToString(cellValue));
                                        }
                                        break;

                                    case "System.Int32":
                                        if (cellValue != DBNull.Value)
                                        {
                                            cell = fila.CreateCell(iCol, CellType.Numeric);
                                            cell.SetCellValue(Convert.ToInt32(cellValue));
                                            cell.CellStyle = _intCellStyle;
                                        }
                                        break;
                                    case "System.Int64":
                                        if (cellValue != DBNull.Value)
                                        {
                                            cell = fila.CreateCell(iCol, CellType.Numeric);
                                            cell.SetCellValue(Convert.ToInt64(cellValue));
                                            cell.CellStyle = _intCellStyle;
                                        }
                                        break;
                                    case "System.Decimal":
                                        if (cellValue != DBNull.Value)
                                        {
                                            cell = fila.CreateCell(iCol, CellType.Numeric);
                                            cell.SetCellValue(Convert.ToDouble(cellValue));
                                            cell.CellStyle = _doubleCellStyle;
                                        }
                                        break;
                                    case "System.Double":
                                        if (cellValue != DBNull.Value)
                                        {
                                            cell = fila.CreateCell(iCol, CellType.Numeric);
                                            cell.SetCellValue(Convert.ToDouble(cellValue));
                                            cell.CellStyle = _doubleCellStyle;
                                        }
                                        break;

                                    case "System.DateTime":
                                        if (cellValue != DBNull.Value)
                                        {
                                            cell = fila.CreateCell(iCol, CellType.Numeric);
                                            cell.SetCellValue(Convert.ToDateTime(cellValue));

                                            //Si No tiene valor de Hora, usar formato dd-MM-yyyy
                                            DateTime cDate = Convert.ToDateTime(cellValue);
                                            if (cDate != null && cDate.Hour > 0) { cell.CellStyle = _dateTimeCellStyle; }
                                            else { cell.CellStyle = _dateCellStyle; }
                                        }
                                        break;
                                    default:
                                        break;
                                }
                                iCol++;
                            }
                            iRow++;
                        }

                        workbook.Write(stream);
                        stream.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public MemoryStream ExportExcel1(string strNewID)
        {
            IWorkbook workbook = new XSSFWorkbook();
            ISheet sheet = workbook.CreateSheet("設定回饋檔");

            // 查詢要匯出資料
            DataTable dt = GetCaseDeadDetailByCaseDeadNewId(strNewID);

            #region  style
            // 標題
            ICellStyle styleHead12 = workbook.CreateCellStyle();
            IFont font12 = workbook.CreateFont();
            font12.FontHeightInPoints = 10;
            font12.FontName = "新細明體";
            styleHead12.FillPattern = FillPattern.SolidForeground;
            styleHead12.BorderTop = BorderStyle.None;
            styleHead12.BorderLeft = BorderStyle.None;
            styleHead12.BorderRight = BorderStyle.None;
            styleHead12.BorderBottom = BorderStyle.None;
            styleHead12.WrapText = true;
            styleHead12.Alignment = HorizontalAlignment.Center;
            styleHead12.VerticalAlignment = VerticalAlignment.Center;
            styleHead12.SetFont(font12);

            // 居左
            ICellStyle styleHead8 = workbook.CreateCellStyle();
            IFont font8 = workbook.CreateFont();
            font8.FontHeightInPoints = 8;
            font8.FontName = "微軟正黑體";
            styleHead8.FillPattern = FillPattern.SolidForeground;
            styleHead8.BorderTop = BorderStyle.Thin;
            styleHead8.BorderLeft = BorderStyle.Thin;
            styleHead8.BorderRight = BorderStyle.Thin;
            styleHead8.BorderBottom = BorderStyle.Thin;
            styleHead8.WrapText = true;
            styleHead8.Alignment = HorizontalAlignment.Left;// 水平位置
            styleHead8.VerticalAlignment = VerticalAlignment.Center;
            styleHead8.SetFont(font8);

            ICellStyle styleHead10 = workbook.CreateCellStyle();
            IFont font10 = workbook.CreateFont();
            font10.FontHeightInPoints = 10;
            font10.FontName = "微軟正黑體";
            styleHead10.FillPattern = FillPattern.SolidForeground;
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
            sheet.SetColumnWidth(0, 100 * 50);
            sheet.SetColumnWidth(1, 100 * 70);
            sheet.SetColumnWidth(2, 100 * 10);
            sheet.SetColumnWidth(3, 100 * 50);
            sheet.SetColumnWidth(4, 100 * 30);
            sheet.SetColumnWidth(5, 100 * 30);
            sheet.SetColumnWidth(6, 100 * 30);
            sheet.SetColumnWidth(7, 100 * 30);
            sheet.SetColumnWidth(8, 100 * 30);
            sheet.SetColumnWidth(9, 100 * 70);
            sheet.SetColumnWidth(10, 100 * 30);
            sheet.SetColumnWidth(11, 100 * 30);
            sheet.SetColumnWidth(12, 100 * 30);
            #endregion

            #region title

           // 0      1            2      3       4         5      6           7            8      9               10      11              12        13              14         15          16          17              18              19              20              21              22              23              24  25      26      27      28       29
           // DOC_ID DOC_NAME    IS_DUP CDBC_ID CDBC_NAME IS_HAVE BRCI_STATUS BRCI_Message IS_BOX TX67050_STATUS  Account AccountStatus   PROD_CODE TX9091_STATUS   BOX_STATUS BOX_Message Gold_STATUS Gold_Message    TX67050_Message TX60628_STATUS  TX60628_Message TX60490_STATUS  TX60490_Message TX9091_Message  Ccy STATUS  REVERSE REMARK  BALANCE TYPE

            //* line2
            SetExcelCell(sheet, 0, 0, styleHead8, "來文ID");
            SetExcelCell(sheet, 0, 1, styleHead8, "來文戶名");
            SetExcelCell(sheet, 0, 2, styleHead8, "重號(TX60628)");
            SetExcelCell(sheet, 0, 3, styleHead8, "本行ID");
            SetExcelCell(sheet, 0, 4, styleHead8, "本行戶名");
            SetExcelCell(sheet, 0, 5, styleHead8, "有無存放款");
            SetExcelCell(sheet, 0, 6, styleHead8, "BRCI設定");
            SetExcelCell(sheet, 0, 7, styleHead8, "保管箱");
            SetExcelCell(sheet, 0, 8, styleHead8, "設定67050-5");
            SetExcelCell(sheet, 0, 9, styleHead8, "存款/黃金存摺帳號");
            SetExcelCell(sheet, 0, 10, styleHead8, "產品型態");
            SetExcelCell(sheet, 0, 11, styleHead8, "狀態");
            SetExcelCell(sheet, 0, 12, styleHead8, "設定9091");
            #endregion

            #region body


            //string QU = "";
            for (int iRow = 0; iRow < dt.Rows.Count; iRow++)
            {
                for (int iCol = 0; iCol < 14; iCol++)
                {
                    if (iCol == 7 )
                    {
                        continue;
                    }
                    if (iCol < 6 || iCol == 10 || iCol == 11 || iCol == 12)
                    {
                        if (iCol == 2) // 重號
                        {
                            if (dt.Rows[iRow][iCol].ToString() == "F")
                            {
                                SetExcelCell(sheet, iRow + 1, iCol, styleHead10, dt.Rows[iRow][20].ToString());
                            }
                            else
                            {
                                SetExcelCell(sheet, iRow + 1, iCol, styleHead10, dt.Rows[iRow][iCol].ToString());
                            }
                        }
                        if (iCol == 11)
                        {
                            SetExcelCell(sheet, iRow + 1, iCol-1, styleHead10, dt.Rows[iRow][12].ToString());
                        }
                        else
                        {
                            if (iCol == 12)
                            {
                                SetExcelCell(sheet, iRow + 1, iCol-1, styleHead10, dt.Rows[iRow][11].ToString());
                            }
                            else
                            {
                                if (iCol > 7)
                                {
                                    SetExcelCell(sheet, iRow + 1, iCol - 1, styleHead10, dt.Rows[iRow][iCol].ToString());
                                }
                                else
                                {
                                    SetExcelCell(sheet, iRow + 1, iCol, styleHead10, dt.Rows[iRow][iCol].ToString());
                                }
                            }
                        }

                    }
                    if (iCol == 6) // BRCI設定
                    {
                        if (dt.Rows[iRow][iCol].ToString() == "F")
                        {
                            SetExcelCell(sheet, iRow + 1, iCol, styleHead10, dt.Rows[iRow][7].ToString());
                        }
                        else
                        {
                            SetExcelCell(sheet, iRow + 1, iCol, styleHead10, dt.Rows[iRow][iCol].ToString());
                        }
                    }
                    if (iCol == 8) //保管箱
                    {
                        if (dt.Rows[iRow][iCol].ToString() == "Y" || dt.Rows[iRow][iCol].ToString() == "y")
                        {
                            SetExcelCell(sheet, iRow + 1, iCol-1, styleHead10, dt.Rows[iRow][iCol].ToString());
                        }
                        else
                        {
                            SetExcelCell(sheet, iRow + 1, iCol - 1, styleHead10, "N");
                            //SetExcelCell(sheet, iRow + 1, iCol-1, styleHead10, dt.Rows[iRow][15].ToString());
                        }
                    }
                    if (iCol == 9) // 67050
                    {
                        if (dt.Rows[iRow][iCol].ToString() == "1")
                        {
                            SetExcelCell(sheet, iRow + 1, iCol-1, styleHead10, "成功");
                        }
                        else
                        {
                            SetExcelCell(sheet, iRow + 1, iCol-1, styleHead10, dt.Rows[iRow][18].ToString());
                        }
                    }
                    if (iCol == 13) // 67050
                    {
                        if (dt.Rows[iRow][iCol].ToString() == "1")
                        {
                            SetExcelCell(sheet, iRow + 1, iCol - 1, styleHead10, "成功");
                        }
                        else
                        {
                            SetExcelCell(sheet, iRow + 1, iCol - 1, styleHead10, dt.Rows[iRow][23].ToString());
                        }
                    }
                }
            }

            //sheet.AddMergedRegion(new CellRangeAddress(1, 1, 7, 8));

            #endregion
            MemoryStream ms = new MemoryStream();
            MemoryStream tempStream = new MemoryStream();
            workbook.Write(tempStream);
            var byteArray = tempStream.ToArray();
            ms.Write(byteArray, 0, byteArray.Length);

            //workbook.Write(ms);
            //ms.Position = 0;
            ms.Flush();
            workbook = null;
            return ms;
        }
        /// <summary>
        /// 匯出Excel
        /// </summary>
        /// <param name="model">查詢條件</param>
        /// <returns></returns>
        public MemoryStream ExportExcel(string strNewID)
        {
            IWorkbook workbook = new XSSFWorkbook();
            ISheet sheet = workbook.CreateSheet("死亡回饋檔");

            // 查詢要匯出資料
            DataTable dt = GetCaseDeadVersionByCaseTrsNewId(strNewID);

            #region  style
            // 標題
            ICellStyle styleHead12 = workbook.CreateCellStyle();
            IFont font12 = workbook.CreateFont();
            font12.FontHeightInPoints = 10;
            font12.FontName = "新細明體";
            styleHead12.FillPattern = FillPattern.SolidForeground;
            styleHead12.BorderTop = BorderStyle.None;
            styleHead12.BorderLeft = BorderStyle.None;
            styleHead12.BorderRight = BorderStyle.None;
            styleHead12.BorderBottom = BorderStyle.None;
            styleHead12.WrapText = true;
            styleHead12.Alignment = HorizontalAlignment.Center;
            styleHead12.VerticalAlignment = VerticalAlignment.Center;
            styleHead12.SetFont(font12);

            // 居左
            ICellStyle styleHead8 = workbook.CreateCellStyle();
            IFont font8 = workbook.CreateFont();
            font8.FontHeightInPoints = 8;
            font8.FontName = "微軟正黑體";
            styleHead8.FillPattern = FillPattern.SolidForeground;
            styleHead8.BorderTop = BorderStyle.Thin;
            styleHead8.BorderLeft = BorderStyle.Thin;
            styleHead8.BorderRight = BorderStyle.Thin;
            styleHead8.BorderBottom = BorderStyle.Thin;
            styleHead8.WrapText = true;
            styleHead8.Alignment = HorizontalAlignment.Left;// 水平位置
            styleHead8.VerticalAlignment = VerticalAlignment.Center;
            styleHead8.SetFont(font8);

            ICellStyle styleHead10 = workbook.CreateCellStyle();
            IFont font10 = workbook.CreateFont();
            font10.FontHeightInPoints = 10;
            font10.FontName = "微軟正黑體";
            styleHead10.FillPattern = FillPattern.SolidForeground;
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
            sheet.SetColumnWidth(0, 100 * 60);
            sheet.SetColumnWidth(1, 100 * 10);
            sheet.SetColumnWidth(2, 100 * 10);
            sheet.SetColumnWidth(3, 100 * 30);
            sheet.SetColumnWidth(4, 100 * 30);
            sheet.SetColumnWidth(5, 100 * 30);
            sheet.SetColumnWidth(6, 100 * 50);
            sheet.SetColumnWidth(7, 100 * 35);
            sheet.SetColumnWidth(8, 100 * 35);
            sheet.SetColumnWidth(9, 100 * 35);
            sheet.SetColumnWidth(10, 100 * 20);
            sheet.SetColumnWidth(11, 100 * 70);
            sheet.SetColumnWidth(12, 100 * 30);
            sheet.SetColumnWidth(13, 100 * 30);
            sheet.SetColumnWidth(14, 100 * 30);
            sheet.SetColumnWidth(15, 100 * 70);
            sheet.SetColumnWidth(16, 100 * 30);
            sheet.SetColumnWidth(17, 100 * 30);
            sheet.SetColumnWidth(18, 100 * 30);
            sheet.SetColumnWidth(19, 100 * 30);
            sheet.SetColumnWidth(20, 100 * 120);
            sheet.SetColumnWidth(21, 100 * 200);
            sheet.SetColumnWidth(22, 100 * 30);
            sheet.SetColumnWidth(23, 100 * 30);
            sheet.SetColumnWidth(24, 100 * 30);
            sheet.SetColumnWidth(25, 100 * 30);
            sheet.SetColumnWidth(26, 100 * 30);
            sheet.SetColumnWidth(27, 100 * 30);
            sheet.SetColumnWidth(28, 100 * 30);
            #endregion

            #region title
            //* line0
            //SetExcelCell(sheet, 0, 0, styleHead12, "死亡回饋檔");
            //sheet.AddMergedRegion(new CellRangeAddress(0, 0, 0, 8));

            //* line2
            SetExcelCell(sheet, 0, 0, styleHead8, "金融遺產查詢單位");
            SetExcelCell(sheet, 0, 1, styleHead8, "縣市代號");
            SetExcelCell(sheet, 0, 2, styleHead8, "總分局處所代號");
            SetExcelCell(sheet, 0, 3, styleHead8, "申請日期");
            SetExcelCell(sheet, 0, 4, styleHead8, "流水號");
            SetExcelCell(sheet, 0, 5, styleHead8, "被繼承人身分證字號");
            SetExcelCell(sheet, 0, 6, styleHead8, "被繼承人姓名");
            SetExcelCell(sheet, 0, 7, styleHead8, "被繼承人出生日期");
            SetExcelCell(sheet, 0, 8, styleHead8, "被繼承人死亡日期");
            SetExcelCell(sheet, 0, 9, styleHead8, "申請人身分證字號");
            SetExcelCell(sheet, 0, 10, styleHead8, "申請人姓名");
            SetExcelCell(sheet, 0, 11, styleHead8, "申請人電話號碼");
            SetExcelCell(sheet, 0, 12, styleHead8, "申請人與被繼承人關係");
            SetExcelCell(sheet, 0, 13, styleHead8, "代理人身分證字號");
            SetExcelCell(sheet, 0, 14, styleHead8, "代理人姓名");
            SetExcelCell(sheet, 0, 15, styleHead8, "代理人電話號碼");
            SetExcelCell(sheet, 0, 16, styleHead8, "送達地址縣市名稱");
            SetExcelCell(sheet, 0, 17, styleHead8, "送達地址鄉鎮市區名稱");
            SetExcelCell(sheet, 0, 18, styleHead8, "送達地址村里名稱");
            SetExcelCell(sheet, 0, 19, styleHead8, "送達地址鄰");
            SetExcelCell(sheet, 0, 20, styleHead8, "送達地址街道門牌");
            SetExcelCell(sheet, 0, 21, styleHead8, "合併Q-U，轉半型");
            SetExcelCell(sheet, 0, 22, styleHead8, "存款");
            SetExcelCell(sheet, 0, 23, styleHead8, "放款");
            SetExcelCell(sheet, 0, 24, styleHead8, "投資型理財");
            SetExcelCell(sheet, 0, 25, styleHead8, "戶名不符");
            SetExcelCell(sheet, 0, 26, styleHead8, "保險箱");
            SetExcelCell(sheet, 0, 27, styleHead8, "信用卡");
            SetExcelCell(sheet, 0, 28, styleHead8, "會辦投資型理財");
            #endregion

            #region body


            string QU = "";
            for (int iRow = 0; iRow < dt.Rows.Count; iRow++)
            {
                QU = "";
                for (int iCol = 0; iCol < dt.Columns.Count; iCol++)
                {
                    {
                        // 居左
                        
                        if (iCol > 15 && iCol < 21)
                        {
                            QU = QU + dt.Rows[iRow][iCol].ToString();
                            SetExcelCell(sheet, iRow + 1, iCol, styleHead10, dt.Rows[iRow][iCol].ToString());
                        }
                        else
                        {
                            if (iCol == 21)
                            {
                                QU=ConvertToHalf(QU);
                                SetExcelCell(sheet, iRow + 1, iCol, styleHead10, QU);
                            }
                            else
                            {
                                SetExcelCell(sheet, iRow + 1, iCol, styleHead10, dt.Rows[iRow][iCol].ToString());
                            }
                        }
                    }
                }
            }

            //sheet.AddMergedRegion(new CellRangeAddress(1, 1, 7, 8));

            #endregion
            MemoryStream ms = new MemoryStream();
            MemoryStream tempStream = new MemoryStream();
            workbook.Write(tempStream);
            var byteArray = tempStream.ToArray();
            ms.Write(byteArray, 0, byteArray.Length);

            //workbook.Write(ms);
            //ms.Position = 0;
            ms.Flush();
            workbook = null;
            return ms;
        }
        public string ConvertToHalf(string wd)
        {
            // 全形轉半形
            string strReturn = Strings.StrConv(wd, VbStrConv.Narrow, 0).ToLower().Trim();
            // 小寫轉大寫
            strReturn = strReturn.ToUpper();
            return strReturn;
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
        ///  複製最大版本號的資料（CaseTrsQuery）
        /// </summary>
        /// <param name="pDocNo">案件編號</param>
        /// <param name="pVersion">版本號</param>
        /// <param name="LogonUser">當前登錄者</param>
        /// <param name="dbTransactionn"></param>
        public void InsertCaseTrsQuery(string pDocNo, string pVersion, string LogonUser, Guid pNewID, IDbTransaction dbTransactionn)
        {

            string sqlInsert = @"INSERT  INTO dbo.CaseTrsQuery
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
                                             FROM    dbo.CaseTrsQuery
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
        /// 根據案件編號&版本號更新CaseTrsQuery
        /// </summary>
        /// <param name="pDocNo">案件編號</param>
        /// <param name="pVersion">版本號</param>
        /// <param name="LogonUser">當前登錄者</param>
        /// <param name="dbTransactionn"></param>
        public void UpdateCaseTrsQuery(string pDocNo, string pVersion, string LogonUser, IDbTransaction dbTransactionn)
        {
            // 更新SQL
            string sqlUpdate = @"UPDATE  CaseTrsQuery
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

        public DataTable GetCaseDeadDetailByCaseDeadNewId(string strCaseDeadNewId)
        {
            string sqlSelect = @" 
SELECT [DOC_ID]
      ,[DOC_NAME]
      ,[IS_DUP]
      ,[CDBC_ID]
      ,[CDBC_NAME]
      ,[IS_HAVE]
      ,[BRCI_STATUS]
      ,[BRCI_Message]
      ,[IS_BOX]
      ,[TX67050_STATUS]
      ,[Account]
      ,[AccountStatus]
      ,[PROD_CODE]
      ,[TX9091_STATUS]
      ,[BOX_STATUS]
      ,[BOX_Message]
      ,[Gold_STATUS]
      ,[Gold_Message]
      ,[TX67050_Message]
      ,[TX60628_STATUS]
      ,[TX60628_Message]
      ,[TX60490_STATUS]
      ,[TX60490_Message]
      ,[TX9091_Message]
      ,[Ccy]
      ,[STATUS]
      ,[REVERSE]
      ,[REMARK]
      ,[BALANCE]
      ,[TYPE]
  FROM  CaseDeadDetail 
WHERE  CaseDeadNewId = @CaseDeadNewId
ORDER BY DetailId";

            base.Parameter.Clear();

            base.Parameter.Add(new CommandParameter("@CaseDeadNewId", strCaseDeadNewId));

            return base.Search(sqlSelect);

        }

        public DataTable GetCaseDeadVersionByCaseTrsNewId(string strCaseTrsNewId)
        {
            string sqlSelect = @" 
SELECT [QueryUnit]
      ,[CityNo]
      ,[BrigeNo]
      ,[AppDate]
      ,[SNo]
      ,[HeirId]
      ,[HeirName]
      ,[HeirBirthDay]
      ,[HeirDeadDate]
      ,[AppId]
      ,[AppName]
      ,[AppTel]
      ,[Relation]
      ,[AgentId]
      ,[AgentName]
      ,[AgentTel]
      ,[SendCity]
      ,[SendTown]
      ,[SendLe]
      ,[SendLin]
      ,[SendStreet]
      ,[MergeQU]
      ,[deposit]
      ,[Loan]
      ,[LoanMgr]
      ,[IsMatch]
      ,[Box]
      ,[CashCard]
      ,[CreditCard]
      ,[InvestMgr]
  FROM  CaseDeadVersion 
WHERE  CaseTrsNewId = @CaseTrsNewId
ORDER BY seq";

            base.Parameter.Clear();

            base.Parameter.Add(new CommandParameter("@CaseTrsNewId", strCaseTrsNewId));

            return base.Search(sqlSelect);

        }
        /// <summary>
        /// 根據案件編號查詢CaseTrsQuery
        /// </summary>
        /// <param name="strDocNo"></param>
        /// <returns></returns>
        public DataTable GetCaseTrsQueryByDocNo(string strDocNo)
        {
            string sqlSelect = @" 
SELECT 
    Status, DocNo, Version, ROpenFileName, RFileTransactionFileName, CloseDate 
FROM CaseTrsQuery
WHERE  DocNo = @DocNo
ORDER BY Version DESC";

            base.Parameter.Clear();

            base.Parameter.Add(new CommandParameter("@DocNo", strDocNo));

            return base.Search(sqlSelect);

        }


        #endregion

    }
}
