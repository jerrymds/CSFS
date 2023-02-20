using CTBC.CSFS.Models;
using CTBC.CSFS.Pattern;
using CTBC.FrameWork.Util;
using CTBC.CSFS.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Data;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.SS.Util;
using NPOI.HSSF.Util;
using NPOI.HSSF.UserModel;
using System.Collections;

namespace CTBC.CSFS.BussinessLogic
{
    public class HistoryQueryBIZ : CommonBIZ
    {
        public HistoryQueryBIZ(AppController appController)
            : base(appController)
        { }
        public HistoryQueryBIZ()
        { }
        public MemoryStream Excel(HistoryQuery model)
        {
            #region
            //string[] headerColumns = new[]
            //        {
            //            "案件編號",//caseno
            //            "類別",//caseKind
            //            "細分類",//caseKind2
            //            "速別",//speed
            //            "來文機關",//GovUnit
            //            "來文字號",//GovNO
            //            "來文日期",//GovDate
            //            "限辦日期",//limitdate
            //            "經辦人員",//person
            //            "結案日期", //結案日起
            //            "狀態",//狀態
            //            "外來文類別備註",//PropertyDeclaration
            //            "逾期原因",//OverDueMemo
            //            "退件原因",//ReturnReason
            //            "結案原因",//CloseReason
            //            "帳務資訊備註"//Memo
            //        };

            //var ms = new MemoryStream();

            //List<HistoryQuery> ilst = GetDataFromCaseMaster(where);
            //#region
            //for (int i = 0; i < ilst.Count; i++)
            //{
            //    List<HistoryQuery> list = getDateyByCaseId(ilst[i].CaseId);
            //    if (list != null)
            //    {
            //        if (list.Count > 0)
            //        {
            //            var _ilist = GetCodeData("STATUS_NAME");
            //            foreach (HistoryQuery item in list)
            //            {
            //                var obj = _ilist.FirstOrDefault(a => a.CodeNo == item.Status);
            //                if (obj != null)
            //                {
            //                    item.StatusShow = obj.CodeDesc;
            //                }

            //                else
            //                {
            //                    item.StatusShow = item.Status;
            //                }
            //                ilst[i].StatusShow = item.StatusShow;
            //            }
            //        }
            //        else
            //        {
            //            ilst = new List<HistoryQuery>();
            //        }
            //    }
            //    else
            //    {
            //        new List<HistoryQuery>();
            //    }
            //}
            //#endregion
            //if (ilst != null)
            //{
            //    ms = ExcelExport(ilst, headerColumns,
            //                                       delegate(HSSFRow dataRow, HistoryQuery dataItem)
            //                                       {
            //                                           //* 這裡可以針對每一個欄位做額外處理.比如日期
            //                                           dataRow.CreateCell(0).SetCellValue(dataItem.CaseNo);
            //                                           dataRow.CreateCell(1).SetCellValue(dataItem.CaseKind);
            //                                           dataRow.CreateCell(2).SetCellValue(dataItem.CaseKind2);
            //                                           dataRow.CreateCell(3).SetCellValue(dataItem.Speed);
            //                                           dataRow.CreateCell(4).SetCellValue(dataItem.GovUnit);
            //                                           dataRow.CreateCell(5).SetCellValue(dataItem.GovNo);
            //                                           dataRow.CreateCell(6).SetCellValue(dataItem.GovDate);
            //                                           dataRow.CreateCell(7).SetCellValue(dataItem.LimitDate);
            //                                           dataRow.CreateCell(8).SetCellValue(dataItem.AgentUser);
            //                                           dataRow.CreateCell(9).SetCellValue(dataItem.CloseDate);
            //                                           dataRow.CreateCell(10).SetCellValue(dataItem.StatusShow);
            //                                           dataRow.CreateCell(11).SetCellValue(dataItem.PropertyDeclaration);
            //                                           dataRow.CreateCell(12).SetCellValue(dataItem.OverDueMemo);
            //                                           dataRow.CreateCell(13).SetCellValue(dataItem.ReturnReason);
            //                                           dataRow.CreateCell(14).SetCellValue(dataItem.CloseReason);
            //                                           dataRow.CreateCell(15).SetCellValue(dataItem.Memo);
            //                                       });
            //}
            //return ms;
            #endregion

            IWorkbook workbook = new HSSFWorkbook();
            ISheet sheet = workbook.CreateSheet("案件查詢");
            DataTable dt = GetDataFromCaseMasterForExcel(model);//獲取查詢集作一科的案件

            #region def style
            ICellStyle styleHead21 = workbook.CreateCellStyle();
            HSSFDataFormat dateformat = (HSSFDataFormat)workbook.CreateDataFormat();
            styleHead21.DataFormat = dateformat.GetFormat("yyyy/mm/dd");


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
            styleHead10.Alignment = HorizontalAlignment.Left;
            styleHead10.VerticalAlignment = VerticalAlignment.Center;
            styleHead10.SetFont(font10);
            #endregion

            #region Width
            sheet.SetColumnWidth(0, 100 * 40);
            sheet.SetColumnWidth(1, 100 * 40);
            sheet.SetColumnWidth(2, 100 * 30);
            sheet.SetColumnWidth(3, 100 * 30);
            sheet.SetColumnWidth(4, 100 * 30);
            sheet.SetColumnWidth(5, 100 * 80);
            sheet.SetColumnWidth(6, 100 * 50);
            sheet.SetColumnWidth(7, 100 * 30);
            sheet.SetColumnWidth(8, 100 * 40);
            sheet.SetColumnWidth(9, 100 * 30);
            sheet.SetColumnWidth(10, 100 * 30);
            sheet.SetColumnWidth(11, 100 * 50);
            sheet.SetColumnWidth(12, 100 * 70);
            sheet.SetColumnWidth(13, 100 * 70);
            sheet.SetColumnWidth(14, 100 * 70);
            sheet.SetColumnWidth(15, 100 * 70);
            sheet.SetColumnWidth(16, 100 * 70);
            sheet.SetColumnWidth(17, 100 * 30);
            sheet.SetColumnWidth(18, 100 * 50);
            sheet.SetColumnWidth(19, 100 * 50);
            sheet.SetColumnWidth(20, 100 * 30);
            sheet.SetColumnWidth(21, 100 * 50);
            sheet.SetColumnWidth(22, 100 * 30);
            sheet.SetColumnWidth(23, 100 * 50);
            sheet.SetColumnWidth(24, 100 * 50);
            sheet.SetColumnWidth(25, 100 * 50);
            #endregion

            #region title
            #region 廢
            //* line0
            //SetExcelCell(sheet, 0, 0, styleHead12, "類別");
            //sheet.AddMergedRegion(new CellRangeAddress(0, 0, 0, 0));
            //SetExcelCell(sheet, 0, 1, styleHead12, model.CaseKind + '-' + model.CaseKind2);
            //sheet.AddMergedRegion(new CellRangeAddress(0, 0, 1, 2));
            //SetExcelCell(sheet, 0, 3, styleHead12, "來文機關");
            //sheet.AddMergedRegion(new CellRangeAddress(0, 0, 3, 3));
            //SetExcelCell(sheet, 0, 4, styleHead12, model.GovUnit);
            //sheet.AddMergedRegion(new CellRangeAddress(0, 0, 4, 5));
            //SetExcelCell(sheet, 0, 6, styleHead12, "來文日期");
            //sheet.AddMergedRegion(new CellRangeAddress(0, 0, 6, 6));
            //SetExcelCell(sheet, 0, 7, styleHead12, model.GovDateS + '~' + model.GovDateE);
            //sheet.AddMergedRegion(new CellRangeAddress(0, 0, 7, 8));
            //SetExcelCell(sheet, 0, 9, styleHead12, "速別");
            //sheet.AddMergedRegion(new CellRangeAddress(0, 0, 9, 9));
            //SetExcelCell(sheet, 0, 10, styleHead12, model.Speed);
            //sheet.AddMergedRegion(new CellRangeAddress(0, 0, 10, 11));

            ////* line1
            //SetExcelCell(sheet, 1, 0, styleHead12, "來文方式");
            //sheet.AddMergedRegion(new CellRangeAddress(1, 1, 0, 0));
            //SetExcelCell(sheet, 1, 1, styleHead12, model.ReceiveKind);
            //sheet.AddMergedRegion(new CellRangeAddress(1, 1, 1, 2));
            //SetExcelCell(sheet, 1, 3, styleHead12, "來文字號");
            //sheet.AddMergedRegion(new CellRangeAddress(1, 1, 3, 3));
            //SetExcelCell(sheet, 1, 4, styleHead12, model.GovNo);
            //sheet.AddMergedRegion(new CellRangeAddress(1, 1, 4, 5));
            //SetExcelCell(sheet, 1, 6, styleHead12, "建檔日期");
            //sheet.AddMergedRegion(new CellRangeAddress(1, 1, 6, 6));
            //SetExcelCell(sheet, 1, 7, styleHead12, model.CreatedDateS + '~' + model.CreatedDateE);
            //sheet.AddMergedRegion(new CellRangeAddress(1, 1, 7, 8));
            //SetExcelCell(sheet, 1, 9, styleHead12, "分行別");
            //sheet.AddMergedRegion(new CellRangeAddress(1, 1, 9, 9));
            //SetExcelCell(sheet, 1, 10, styleHead12, model.Unit);
            //sheet.AddMergedRegion(new CellRangeAddress(1, 1, 10, 11));

            ////* line2
            //SetExcelCell(sheet, 2, 0, styleHead12, "建檔人員");
            //sheet.AddMergedRegion(new CellRangeAddress(2, 2, 0, 0));
            //SetExcelCell(sheet, 2, 1, styleHead12, model.CreateUser);
            //sheet.AddMergedRegion(new CellRangeAddress(2, 2, 1, 2));
            //SetExcelCell(sheet, 2, 3, styleHead12, "客戶ID");
            //sheet.AddMergedRegion(new CellRangeAddress(2, 2, 3, 3));
            //SetExcelCell(sheet, 2, 4, styleHead12, model.ObligorNo);
            //sheet.AddMergedRegion(new CellRangeAddress(2, 2, 4, 5));
            //SetExcelCell(sheet, 2, 6, styleHead12, "客戶姓名");
            //sheet.AddMergedRegion(new CellRangeAddress(2, 2, 6, 6));
            //SetExcelCell(sheet, 2, 7, styleHead12, model.ObligorName);
            //sheet.AddMergedRegion(new CellRangeAddress(2, 2, 7, 8));
            //SetExcelCell(sheet, 2, 9, styleHead12, "發文日期");
            //sheet.AddMergedRegion(new CellRangeAddress(2, 2, 9, 9));
            //SetExcelCell(sheet, 2, 10, styleHead12, model.SendDateS + '~' + model.SendDateE);
            //sheet.AddMergedRegion(new CellRangeAddress(2, 2, 10, 11));

            ////* line3
            //SetExcelCell(sheet, 3, 0, styleHead12, "發文字號");
            //sheet.AddMergedRegion(new CellRangeAddress(3, 3, 0, 0));
            //SetExcelCell(sheet, 3, 1, styleHead12, model.SendNo);
            //sheet.AddMergedRegion(new CellRangeAddress(3, 3, 1, 2));
            //SetExcelCell(sheet, 3, 3, styleHead12, "結案日期");
            //sheet.AddMergedRegion(new CellRangeAddress(3, 3, 3, 3));
            //SetExcelCell(sheet, 3, 4, styleHead12, model.OverDateS + '~' + model.OverDateE);
            //sheet.AddMergedRegion(new CellRangeAddress(3, 3, 4, 5));
            //SetExcelCell(sheet, 3, 6, styleHead12, "狀態");
            //sheet.AddMergedRegion(new CellRangeAddress(3, 3, 6, 6));
            //SetExcelCell(sheet, 3, 7, styleHead12, model.Status);
            //sheet.AddMergedRegion(new CellRangeAddress(3, 3, 7, 8));
            //SetExcelCell(sheet, 3, 9, styleHead12, "經辦人員");
            //sheet.AddMergedRegion(new CellRangeAddress(3, 3, 9, 9));
            //SetExcelCell(sheet, 3, 10, styleHead12, model.AgentUser);
            //sheet.AddMergedRegion(new CellRangeAddress(3, 3, 10, 11));
            #endregion
            //* line0
            SetExcelCell(sheet, 0, 0, styleHead12, "案件查詢");
            sheet.AddMergedRegion(new CellRangeAddress(0, 0, 0, 23));

            //* line1
            SetExcelCell(sheet, 1, 0, styleHead10, "發文方式");
            sheet.AddMergedRegion(new CellRangeAddress(1, 1, 0, 0));
            SetExcelCell(sheet, 1, 1, styleHead10, "案件編號");
            sheet.AddMergedRegion(new CellRangeAddress(1, 1, 1, 1));
            SetExcelCell(sheet, 1, 2, styleHead10, "類別");
            sheet.AddMergedRegion(new CellRangeAddress(1, 1, 2, 2));
            SetExcelCell(sheet, 1, 3, styleHead10, "細分類");
            sheet.AddMergedRegion(new CellRangeAddress(1, 1, 3, 3));
            SetExcelCell(sheet, 1, 4, styleHead10, "速別");
            sheet.AddMergedRegion(new CellRangeAddress(1, 1, 4, 4));
            SetExcelCell(sheet, 1, 5, styleHead10, "來文機關");
            sheet.AddMergedRegion(new CellRangeAddress(1, 1, 5, 5));
            SetExcelCell(sheet, 1, 6, styleHead10, "來文字號");
            sheet.AddMergedRegion(new CellRangeAddress(1, 1, 6, 6));
            SetExcelCell(sheet, 1, 7, styleHead10, "來文日期");
            sheet.AddMergedRegion(new CellRangeAddress(1, 1, 7, 7));
            SetExcelCell(sheet, 1, 8, styleHead10, "限辦日期");
            sheet.AddMergedRegion(new CellRangeAddress(1, 1, 8, 8));
            SetExcelCell(sheet, 1, 9, styleHead10, "經辦人員");
            sheet.AddMergedRegion(new CellRangeAddress(1, 1, 9, 9));
            SetExcelCell(sheet, 1, 10, styleHead10, "結案日期");
            sheet.AddMergedRegion(new CellRangeAddress(1, 1, 10, 10));
            SetExcelCell(sheet, 1, 11, styleHead10, "狀態");
            sheet.AddMergedRegion(new CellRangeAddress(1, 1, 11, 11));
            SetExcelCell(sheet, 1, 12, styleHead10, "外來文類別備註");
            sheet.AddMergedRegion(new CellRangeAddress(1, 1, 12, 12));
            SetExcelCell(sheet, 1, 13, styleHead10, "逾期原因");
            sheet.AddMergedRegion(new CellRangeAddress(1, 1, 13, 13));
            SetExcelCell(sheet, 1, 14, styleHead10, "收發退件原因");
            sheet.AddMergedRegion(new CellRangeAddress(1, 1, 14, 14));
            SetExcelCell(sheet, 1, 15, styleHead10, "分行回覆原因");
            sheet.AddMergedRegion(new CellRangeAddress(1, 1, 15, 15));
            SetExcelCell(sheet, 1, 16, styleHead10, "帳務資訊備註");
            sheet.AddMergedRegion(new CellRangeAddress(1, 1, 16, 16));
            SetExcelCell(sheet, 1, 17, styleHead10, "來文方式");
            sheet.AddMergedRegion(new CellRangeAddress(1, 1, 17, 17));
            SetExcelCell(sheet, 1, 18, styleHead10, "建檔日期");
            sheet.AddMergedRegion(new CellRangeAddress(1, 1, 18, 18));
            SetExcelCell(sheet, 1, 19, styleHead10, "分行別");
            sheet.AddMergedRegion(new CellRangeAddress(1, 1, 19, 19));
            SetExcelCell(sheet, 1, 20, styleHead10, "建檔人員");
            sheet.AddMergedRegion(new CellRangeAddress(1, 1, 20, 20));
            SetExcelCell(sheet, 1, 21, styleHead10, "電子發文/外來文結案上傳日");
            sheet.AddMergedRegion(new CellRangeAddress(1, 1, 21, 21));
            SetExcelCell(sheet, 1, 22, styleHead10, "客戶ID");
            sheet.AddMergedRegion(new CellRangeAddress(1, 1, 22, 22));
            SetExcelCell(sheet, 1, 23, styleHead10, "客戶姓名");
            sheet.AddMergedRegion(new CellRangeAddress(1, 1, 23, 23));
            SetExcelCell(sheet, 1, 24, styleHead10, "發文日期");
            sheet.AddMergedRegion(new CellRangeAddress(1, 1, 24, 24));
            SetExcelCell(sheet, 1, 25, styleHead10, "發文字號");
            sheet.AddMergedRegion(new CellRangeAddress(1, 1, 25, 25));
            
            #endregion

            #region body
            var ilist = GetCodeData("STATUS_NAME");
            for (int iRow = 0; iRow < dt.Rows.Count; iRow++)
            {
                var obj = ilist.FirstOrDefault(a => a.CodeNo == Convert.ToString(dt.Rows[iRow]["Status"]));
                string strStatus = obj != null ? obj.CodeDesc : Convert.ToString(dt.Rows[iRow]["Status"]);
                for (int iCol = 1; iCol < dt.Columns.Count; iCol++)
                {

                    if ((iCol != 12) && (iCol != 22))
                        SetExcelCell(sheet, iRow + 2, iCol - 1, styleHead10, dt.Rows[iRow][iCol].ToString());
                    else
                    {
                        if (iCol == 22)
                        {
                            string NewDate = string.IsNullOrEmpty(dt.Rows[iRow][iCol].ToString()) ? "" : DateTime.Parse(dt.Rows[iRow][iCol].ToString()).ToString("yyyy/MM/dd");
                            SetExcelCell(sheet, iRow + 2, iCol - 1, styleHead10, NewDate);
                        }
                        else
                        {
                            SetExcelCell(sheet, iRow + 2, iCol - 1, styleHead10, strStatus);
                        }
                    }
                }
            }
            #endregion

            MemoryStream ms = new MemoryStream();
            workbook.Write(ms);
            ms.Flush();
            ms.Position = 0;
            workbook = null;
            return ms;
        }
        public List<HistoryQuery> getDateyByCaseId(Guid caseId)
        {
            string strSql = @"SELECT distinct A.CaseId, B.ObligorName,B.ObligorNo,CaseNo,GovUnit,(SELECT CONVERT(varchar(100), CloseDate, 111)) as                             CloseDate,AgentUser,(SELECT CONVERT(varchar(100), GovDate, 111)) as GovDate, 
                              GovNo,Person,CaseKind,CaseKind2,A.Speed,Unit,(SELECT CONVERT(varchar(100), LimitDate, 111)) as LimitDate,
                              Status,(SELECT CONVERT(varchar(100), ApproveDate, 111)) as ApproveDate 
                              FROM History_CaseMaster A 
                              left join History_CaseObligor B on A.CaseId=b.CaseId left join History_CaseSendSetting S on A.CaseId=S.CaseId 
                              where A.CaseId=@CaseId";
            base.Parameter.Clear();
            base.Parameter.Add(new CommandParameter("@CaseId", caseId.ToString()));
            return base.SearchList<HistoryQuery>(strSql).ToList() ?? new List<HistoryQuery>();
        }
        public IList<HistoryQuery> GetData(HistoryQuery model, int pageNum, string strSortExpression, string strSortDirection, string UserId, ref string where)
        {
            try
            {
                where = string.Empty;
                string sqlWhere = "";
                base.PageIndex = pageNum;
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@pageS", (base.PageSize * (base.PageIndex - 1)) + 1));
                base.Parameter.Add(new CommandParameter("@pageE", base.PageSize * base.PageIndex));
                if (!string.IsNullOrEmpty(model.CaseNo))
                {

                    sqlWhere += @" and (A.CaseId in (select CaseId from History_CaseNoChangeHistory where OldCaseNo like @CaseNo) or A.CaseNo like @CaseNo) ";
                    where += @" and (A.CaseId in (select CaseId from History_CaseNoChangeHistory where '%" + model.CaseNo.Trim() + "%' like @CaseNo) or A.CaseNo like '%" + model.CaseNo.Trim() + "%') ";
                    base.Parameter.Add(new CommandParameter("@CaseNo", "%" + model.CaseNo.Trim() + "%"));
                }
                if (!string.IsNullOrEmpty(model.GovUnit))
                {
                    sqlWhere += @" and GovUnit like @GovUnit ";
                    where += @" and GovUnit like '%" + model.GovUnit.Trim() + "%'";
                    base.Parameter.Add(new CommandParameter("@GovUnit", "%" + model.GovUnit.Trim() + "%"));
                }
                if (!string.IsNullOrEmpty(model.GovNo))
                {
                    sqlWhere += @" and GovNo like @GovNo ";
                    where += @" and GovUnit like '%" + model.GovNo.Trim() + "%'";
                    base.Parameter.Add(new CommandParameter("@GovNo", "%" + model.GovNo.Trim() + "%"));
                }
                if (!string.IsNullOrEmpty(model.CreateUser))
                {
                    sqlWhere += @" and Person like @Person ";
                    where += @" and Person like '%" + model.CreateUser.Trim() + "%'";
                    base.Parameter.Add(new CommandParameter("@Person", "%" + model.CreateUser.Trim() + "%"));
                }
                if (!string.IsNullOrEmpty(model.Speed))
                {
                    sqlWhere += @" and A.Speed = @Speed ";
                    where += @" and A.Speed = '" + model.Speed.Trim() + "' ";
                    base.Parameter.Add(new CommandParameter("@Speed", model.Speed.Trim()));
                }
                if (!string.IsNullOrEmpty(model.ReceiveKind))
                {
                    sqlWhere += @" and ReceiveKind like @ReceiveKind ";
                    where += @" and ReceiveKind like '%" + model.ReceiveKind.Trim() + "%'";
                    base.Parameter.Add(new CommandParameter("@ReceiveKind", "%" + model.ReceiveKind.Trim() + "%"));
                }
                if (!string.IsNullOrEmpty(model.SendKind))
                {
                    sqlWhere += @" and S.SendKind like @SendKind ";
                    where += @" and S.SendKind like '%" + model.SendKind.Trim() + "%'";
                    base.Parameter.Add(new CommandParameter("@SendKind", "%" + model.SendKind.Trim() + "%"));
                }
                if (!string.IsNullOrEmpty(model.CaseKind))
                {
                    sqlWhere += @" and CaseKind = @CaseKind ";
                    where += @" and CaseKind = '" + model.CaseKind.Trim() + "' ";
                    base.Parameter.Add(new CommandParameter("@CaseKind", model.CaseKind.Trim()));
                }
                if (!string.IsNullOrEmpty(model.CaseKind2))
                {

                    sqlWhere += @" and CaseKind2 = @CaseKind2 ";
                    where += @" and CaseKind2 = '" + model.CaseKind2.Trim() + "' ";
                    base.Parameter.Add(new CommandParameter("@CaseKind2", model.CaseKind2.Trim()));
                }
                if (!string.IsNullOrEmpty(model.GovDateS))
                {
                    sqlWhere += @" AND GovDate >= @GovDateS";
                    where += @" and GovDate >= '" + model.GovDateS + "' ";
                    base.Parameter.Add(new CommandParameter("@GovDateS", model.GovDateS));
                }
                if (!string.IsNullOrEmpty(model.GovDateE))
                {
                    sqlWhere += @" AND GovDate <= @GovDateE ";
                    where += @" and GovDate <= '" + model.GovDateE + "' ";
                    base.Parameter.Add(new CommandParameter("@GovDateE", model.GovDateE));
                }
                if (!string.IsNullOrEmpty(model.Unit))
                {
                    sqlWhere += @" and Unit like @Unit ";
                    where += @" and Unit like '%" + model.Unit.Trim() + "%'";
                    base.Parameter.Add(new CommandParameter("@Unit", "%" + model.Unit.Trim() + "%"));
                }
                if (!string.IsNullOrEmpty(model.CreatedDateS))
                {
                    sqlWhere += @" AND A.CreatedDate >= @CreatedDateS";
                    where += @" and A.CreatedDate >= '" + model.CreatedDateS + "' ";
                    base.Parameter.Add(new CommandParameter("@CreatedDateS", model.CreatedDateS));
                }
                if (!string.IsNullOrEmpty(model.CreatedDateE))
                {
                    sqlWhere += @" AND A.CreatedDate <= @CreatedDateE ";
                    where += @" and A.CreatedDate <= '" + model.CreatedDateE + "' ";
                    base.Parameter.Add(new CommandParameter("@CreatedDateE", model.CreatedDateE));
                }
                if (!string.IsNullOrEmpty(model.OverDateS))
                {
                    model.OverDateS = UtlString.FormatDateTwStringToAd(model.OverDateS);
                    sqlWhere += @" AND A.CloseDate >= @OverDateS ";
                    where += @" and A.CloseDate >= '" + model.OverDateS + "' ";
                    base.Parameter.Add(new CommandParameter("@OverDateS", model.OverDateS));
                }
                if (!string.IsNullOrEmpty(model.OverDateE))
                {
                    model.OverDateE = Convert.ToDateTime(UtlString.FormatDateTwStringToAd(model.OverDateE)).AddDays(1).ToString("yyyy/MM/dd");
                    sqlWhere += @" AND A.CloseDate < @OverDateE ";
                    where += @" and A.CloseDate < '" + model.OverDateE + "' ";
                    base.Parameter.Add(new CommandParameter("@OverDateE", model.OverDateE));
                }
                if (!string.IsNullOrEmpty(model.Status))
                {
                    sqlWhere += @" AND Status = @Status ";
                    where += @" and Status = '" + model.Status + "' ";
                    base.Parameter.Add(new CommandParameter("@Status", model.Status));
                }
                //20170630 緊急 RQ-2015-019666-018 派件至跨單位 宏祥 add start
                //if (!string.IsNullOrEmpty(model.AgentUser))
                //{
                //    sqlWhere += @" AND AgentUser like @AgentUser ";
                //    where += @" and AgentUser like '%" + model.AgentUser.Trim() + "%'";
                //    base.Parameter.Add(new CommandParameter("@AgentUser", "%" + model.AgentUser + "%"));
                //}
                string[] aryAgentDepartmentUser;
                string strAgentDepartmentUser = "";
                if (!string.IsNullOrEmpty(model.AgentDepartmentUser))
                {
                   aryAgentDepartmentUser = model.AgentDepartmentUser.Split('-');
                   strAgentDepartmentUser = aryAgentDepartmentUser.GetValue(0).ToString().Trim();
                   sqlWhere += @" AND AgentUser like @AgentUser ";
                   base.Parameter.Add(new CommandParameter("@AgentUser", "%" + strAgentDepartmentUser + "%"));
                }
                else if (!string.IsNullOrEmpty(model.AgentDepartment2))
                {
                   AgentSettingBIZ agentsettingBiz = new AgentSettingBIZ();
                   aryAgentDepartmentUser = agentsettingBiz.GetAgentSetting(model.AgentDepartment2).Split(',');
                   if (aryAgentDepartmentUser.Length > 0)
                   {
                      for (int i = 0; i < aryAgentDepartmentUser.Length; i++)
                      {
                         strAgentDepartmentUser += "'" + aryAgentDepartmentUser.GetValue(i).ToString().Trim() + "',";
                      }
                      strAgentDepartmentUser = strAgentDepartmentUser.Trim(',');

                      sqlWhere += @" AND AgentUser in (" + strAgentDepartmentUser + ")";
                   }
                }
                else if (!string.IsNullOrEmpty(model.AgentDepartment))
                {
                   AgentSettingBIZ agentsettingBiz = new AgentSettingBIZ();
                   IList<AgentSetting> list = agentsettingBiz.GetAgentDepartmentUserView(model.AgentDepartment);
                   foreach (AgentSetting item in list)
                   {
                      strAgentDepartmentUser += "'" + item.EmpId.ToString().Trim() + "',";
                   }
                   strAgentDepartmentUser = strAgentDepartmentUser.Trim(',');

                   sqlWhere += @" AND AgentUser in (" + strAgentDepartmentUser + ")";
                }
                //20170714 固定 RQ-2015-019666-019 派件至跨單位 宏祥 update start
                //else if (model.IsBranchDirector == "1")
                //{
                //   sqlWhere += @" AND AgentUser in (select EmpID from V_AgentAndDept where ManagerID like @ManagerID) ";
                //   base.Parameter.Add(new CommandParameter("@ManagerID", "%" + UserId + "%"));
                //}
                //20170714 固定 RQ-2015-019666-019 派件至跨單位 宏祥 update end
                //20170630 緊急 RQ-2015-019666-018 派件至跨單位 宏祥 add end
                if (!string.IsNullOrEmpty(model.ObligorName))
                {
                    sqlWhere += @" AND B.ObligorName = @ObligorName ";
                    where += @" and B.ObligorName = '" + model.ObligorName + "' ";
                    base.Parameter.Add(new CommandParameter("@ObligorName", model.ObligorName));
                }
                if (!string.IsNullOrEmpty(model.ObligorNo))
                {
                    sqlWhere += @" AND B.ObligorNo = @ObligorNo ";
                    where += @" and B.ObligorNo = '" + model.ObligorNo + "' ";
                    base.Parameter.Add(new CommandParameter("@ObligorNo", model.ObligorNo));
                }


                if (!string.IsNullOrEmpty(model.SendDateS))
                {
                    model.SendDateS = UtlString.FormatDateTwStringToAd(model.SendDateS);
                    sqlWhere += @" AND S.SendDate >= @SendDateS ";
                    where += @" and S.SendDate >= '" + model.SendDateS + "' ";
                    base.Parameter.Add(new CommandParameter("@SendDateS", model.SendDateS));
                }
                if (!string.IsNullOrEmpty(model.SendDateE))
                {
                    model.SendDateE = UtlString.FormatDateTwStringToAd(model.SendDateE);
                    sqlWhere += @" AND S.SendDate <= @SendDateE ";
                    where += @" and S.SendDate <= '" + model.SendDateE + "' ";
                    base.Parameter.Add(new CommandParameter("@SendDateE", model.SendDateE));
                }
                //AgentUser
                if (!string.IsNullOrEmpty(model.SendNo))
                {
                    sqlWhere += @" AND S.SendNo like @SendNo ";
                    where += @" and S.SendNo like '%" + model.SendNo.Trim() + "%'";
                    base.Parameter.Add(new CommandParameter("@SendNo", "%" + model.SendNo.Trim() + "%"));
                }
                //Add by zhangwei 20180315 start
                if (!string.IsNullOrEmpty(model.RMType))
                {
                    if (model.RMType == "1")
                    {
                        //sqlWhere += @" AND isnull(C.RMNum,'')<>'' and C.RMNum<>'00000' and len(C.customerid )=8";
                        //where += @" and isnull(C.RMNum,'')<>'' and C.RMNum<>'00000' and len(C.customerid )=8";
                        sqlWhere += @" AND ((isnull(C.RMNum,'')<>'' and C.RMNum<>'00000' and len(C.customerid )>=8) or (len(TX.RM_NO + TX.RM_NAME) > 0 and TX.RM_NO<>'00000' and isnull(TX.RM_NAME,'')<>'')) ";
                        where += @" and ((isnull(C.RMNum,'')<>'' and C.RMNum<>'00000' and len(C.customerid )>=8) or (len(TX.RM_NO + TX.RM_NAME) > 0 and TX.RM_NO<>'00000' and isnull(TX.RM_NAME,'')<>'')) ";
                    }
                    else if (model.RMType == "2")
                    {
                        sqlWhere += @" AND (isnull(C.RMNum,'')='' or C.RMNum='00000') and len(C.customerid )>=8";
                        where += @" and (isnull(C.RMNum,'')='' or C.RMNum='00000') and len(C.customerid )>=8";
                    }
                }
                //Add by zhangwei 20180315 end
                string strSql = @";with T1 
	                        as
	                        (
                                SELECT distinct A.CaseId, CaseNo,GovUnit,(SELECT CONVERT(varchar(100), CloseDate, 111)) as CloseDate,AgentUser,(SELECT CONVERT(varchar(100), GovDate, 111)) as GovDate, 
                                GovNo,Person,CaseKind,CaseKind2,A.Speed,Unit,(SELECT CONVERT(varchar(100), LimitDate, 111)) as LimitDate,
                                Status,(SELECT CONVERT(varchar(100), ApproveDate, 111)) as ApproveDate 
                                FROM History_CaseMaster A 
                                left join History_CaseObligor B on A.CaseId=b.CaseId left join History_CaseSendSetting S on A.CaseId=S.CaseId 
                                left join History_TX_60491_Grp C on A.CaseId=C.CaseId left join History_TX_67002 TX on A.CaseId=TX.CaseId 
                                where 0=0 " + sqlWhere + @" 
	                        ),T2 as
	                        (
		                        select *, row_number() over (order by " + strSortExpression + " " + strSortDirection + @" ) RowNum
		                        from T1
	                        ),T3 as 
	                        (
		                        select *,(select max(RowNum) from T2) maxnum from T2 
		                        where rownum between @pageS and @pageE
	                        )
	                        select a.* from T3 a order by a.RowNum";

                IList<HistoryQuery> ilst = base.SearchList<HistoryQuery>(strSql);
                if (ilst != null)
                {
                    if (ilst.Count > 0)
                    {
                        var ilist = GetCodeData("STATUS_NAME");
                        foreach (HistoryQuery item in ilst)
                        {
                            var obj = ilist.FirstOrDefault(a => a.CodeNo == item.Status);
                            if (obj != null)
                                item.StatusShow = obj.CodeDesc;
                            else
                                item.StatusShow = item.Status;
                        }
                        base.DataRecords = ilst[0].maxnum;
                    }
                    else
                    {
                        base.DataRecords = 0;
                        ilst = new List<HistoryQuery>();
                    }
                    return ilst;
                }
                else
                {
                    return new List<HistoryQuery>();
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public IList<HistoryQuery> GetDataForSendAgain(HistoryQuery model, int pageNum, string strSortExpression, string strSortDirection)
        {
            try
            {
                string sqlWhere = "";
                base.PageIndex = pageNum;
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@pageS", (base.PageSize * (base.PageIndex - 1)) + 1));
                base.Parameter.Add(new CommandParameter("@pageE", base.PageSize * base.PageIndex));

                if (!string.IsNullOrEmpty(model.CaseNo))
                {
                    sqlWhere += @" and CaseNo like @CaseNo ";
                    base.Parameter.Add(new CommandParameter("@CaseNo", "%" + model.CaseNo.Trim() + "%"));
                }
                if (!string.IsNullOrEmpty(model.GovUnit))
                {
                    sqlWhere += @" and GovUnit like @GovUnit ";
                    base.Parameter.Add(new CommandParameter("@GovUnit", "%" + model.GovUnit.Trim() + "%"));
                }
                if (!string.IsNullOrEmpty(model.GovNo))
                {
                    sqlWhere += @" and GovNo like @GovNo ";
                    base.Parameter.Add(new CommandParameter("@GovNo", "%" + model.GovNo.Trim() + "%"));
                }
                if (!string.IsNullOrEmpty(model.CreateUser))
                {
                    sqlWhere += @" and Person like @Person ";
                    base.Parameter.Add(new CommandParameter("@Person", "%" + model.CreateUser.Trim() + "%"));
                }
                if (!string.IsNullOrEmpty(model.Speed))
                {
                    sqlWhere += @" and A.Speed = @Speed ";
                    base.Parameter.Add(new CommandParameter("@Speed", model.Speed.Trim()));
                }
                if (!string.IsNullOrEmpty(model.ReceiveKind))
                {
                    sqlWhere += @" and ReceiveKind like @ReceiveKind ";
                    base.Parameter.Add(new CommandParameter("@ReceiveKind", "%" + model.ReceiveKind.Trim() + "%"));
                }
                if (!string.IsNullOrEmpty(model.SendKind))
                {
                    sqlWhere += @" and S.SendKind like @SendKind ";
                    base.Parameter.Add(new CommandParameter("@SendKind", "%" + model.SendKind.Trim() + "%"));
                }
                if (!string.IsNullOrEmpty(model.CaseKind))
                {
                    sqlWhere += @" and CaseKind = @CaseKind ";
                    base.Parameter.Add(new CommandParameter("@CaseKind", model.CaseKind.Trim()));
                }
                if (!string.IsNullOrEmpty(model.CaseKind2))
                {

                    sqlWhere += @" and CaseKind2 = @CaseKind2 ";
                    base.Parameter.Add(new CommandParameter("@CaseKind2", model.CaseKind2.Trim()));
                }
                if (!string.IsNullOrEmpty(model.GovDateS))
                {
                    sqlWhere += @" AND GovDate >= @GovDateS";
                    base.Parameter.Add(new CommandParameter("@GovDateS", model.GovDateS));
                }
                if (!string.IsNullOrEmpty(model.GovDateE))
                {
                    sqlWhere += @" AND GovDate <= @GovDateE ";
                    base.Parameter.Add(new CommandParameter("@GovDateE", model.GovDateE));
                }
                if (!string.IsNullOrEmpty(model.Unit))
                {
                    sqlWhere += @" and Unit like @Unit ";
                    base.Parameter.Add(new CommandParameter("@Unit", "%" + model.Unit.Trim() + "%"));
                }
                if (!string.IsNullOrEmpty(model.CreatedDateS))
                {
                    sqlWhere += @" AND A.CreatedDate >= @CreatedDateS";
                    base.Parameter.Add(new CommandParameter("@CreatedDateS", model.CreatedDateS));
                }
                if (!string.IsNullOrEmpty(model.CreatedDateE))
                {
                    sqlWhere += @" AND A.CreatedDate <= @CreatedDateE ";
                    base.Parameter.Add(new CommandParameter("@CreatedDateE", model.CreatedDateE));
                }
                if (!string.IsNullOrEmpty(model.OverDateS))
                {
                    model.OverDateS = UtlString.FormatDateTwStringToAd(model.OverDateS);
                    sqlWhere += @" AND A.CloseDate >= @OverDateS ";
                    base.Parameter.Add(new CommandParameter("@OverDateS", model.OverDateS));
                }
                if (!string.IsNullOrEmpty(model.OverDateE))
                {
                    model.OverDateE = UtlString.FormatDateTwStringToAd(model.OverDateE);
                    sqlWhere += @" AND A.CloseDate <= @OverDateE ";
                    base.Parameter.Add(new CommandParameter("@OverDateE", model.OverDateE));
                }
                if (!string.IsNullOrEmpty(model.Status))
                {
                    sqlWhere += @" AND Status = @Status ";
                    base.Parameter.Add(new CommandParameter("@Status", model.Status));
                }
                //20170811 RC RQ-2015-019666-020 派件至跨單位 宏祥 add start
                //if (!string.IsNullOrEmpty(model.AgentUser))
                //{
                //    sqlWhere += @" AND AgentUser like @AgentUser ";
                //    base.Parameter.Add(new CommandParameter("@AgentUser", "%" + model.AgentUser + "%"));
                //}
                string[] aryAgentDepartmentUser;
                string strAgentDepartmentUser = "";
                if (!string.IsNullOrEmpty(model.AgentDepartmentUser))
                {
                   aryAgentDepartmentUser = model.AgentDepartmentUser.Split('-');
                   strAgentDepartmentUser = aryAgentDepartmentUser.GetValue(0).ToString().Trim();
                   sqlWhere += @" AND AgentUser like @AgentUser ";
                   base.Parameter.Add(new CommandParameter("@AgentUser", "%" + strAgentDepartmentUser + "%"));
                }
                else if (!string.IsNullOrEmpty(model.AgentDepartment2))
                {
                   AgentSettingBIZ agentsettingBiz = new AgentSettingBIZ();
                   aryAgentDepartmentUser = agentsettingBiz.GetAgentSetting(model.AgentDepartment2).Split(',');
                   if (aryAgentDepartmentUser.Length > 0)
                   {
                      for (int i = 0; i < aryAgentDepartmentUser.Length; i++)
                      {
                         strAgentDepartmentUser += "'" + aryAgentDepartmentUser.GetValue(i).ToString().Trim() + "',";
                      }
                      strAgentDepartmentUser = strAgentDepartmentUser.Trim(',');

                      sqlWhere += @" AND AgentUser in (" + strAgentDepartmentUser + ")";
                   }
                }
                else if (!string.IsNullOrEmpty(model.AgentDepartment))
                {
                   AgentSettingBIZ agentsettingBiz = new AgentSettingBIZ();
                   IList<AgentSetting> list = agentsettingBiz.GetAgentDepartmentUserView(model.AgentDepartment);
                   foreach (AgentSetting item in list)
                   {
                      strAgentDepartmentUser += "'" + item.EmpId.ToString().Trim() + "',";
                   }
                   strAgentDepartmentUser = strAgentDepartmentUser.Trim(',');

                   sqlWhere += @" AND AgentUser in (" + strAgentDepartmentUser + ")";
                }
                //20170811 RC RQ-2015-019666-020 派件至跨單位 宏祥 add end
                if (!string.IsNullOrEmpty(model.ObligorName))
                {
                    sqlWhere += @" AND B.ObligorName = @ObligorName ";
                    base.Parameter.Add(new CommandParameter("@ObligorName", model.ObligorName));
                }
                if (!string.IsNullOrEmpty(model.ObligorNo))
                {
                    sqlWhere += @" AND B.ObligorNo = @ObligorNo ";
                    base.Parameter.Add(new CommandParameter("@ObligorNo", model.ObligorNo));
                }
                if (!string.IsNullOrEmpty(model.SendDateS))
                {
                    model.SendDateS = UtlString.FormatDateTwStringToAd(model.SendDateS);
                    sqlWhere += @" AND S.SendDate >= @SendDateS ";
                    base.Parameter.Add(new CommandParameter("@SendDateS", model.SendDateS));
                }
                if (!string.IsNullOrEmpty(model.SendDateE))
                {
                    model.SendDateE = UtlString.FormatDateTwStringToAd(model.SendDateE);
                    sqlWhere += @" AND S.SendDate <= @SendDateE ";
                    base.Parameter.Add(new CommandParameter("@SendDateE", model.SendDateE));
                }
                if (!string.IsNullOrEmpty(model.SendNo))
                {
                    sqlWhere += @" AND S.SendNo like @SendNo ";
                    base.Parameter.Add(new CommandParameter("@SendNo", "%" + model.SendNo.Trim() + "%"));
                }

                sqlWhere += @" AND (a.Status = @Status1  or a.Status = @Status2)  ";
                base.Parameter.Add(new CommandParameter("@Status1", CaseStatus.DirectorApprove));
                base.Parameter.Add(new CommandParameter("@Status2", CaseStatus.DirectorApproveSeizureAndPay));
                string strSql = @";with T1 
	                        as
	                        (
                                SELECT distinct A.CaseId, CaseNo,GovUnit,(SELECT CONVERT(varchar(100), CloseDate, 111)) as CloseDate,AgentUser,(SELECT CONVERT(varchar(100), GovDate, 111)) as GovDate, 
                                GovNo,Person,CaseKind,CaseKind2,A.Speed,Unit,(SELECT CONVERT(varchar(100), LimitDate, 111)) as LimitDate,
                                Status,(SELECT CONVERT(varchar(100), ApproveDate, 111)) as ApproveDate 
                                FROM History_CaseMaster A 
                                left join History_CaseObligor B on A.CaseId=b.CaseId left join History_CaseSendSetting S on A.CaseId=S.CaseId where 0=0 " + sqlWhere + @" 
	                        ),T2 as
	                        (
		                        select *, row_number() over (order by  CaseNo  asc ) RowNum
		                        from T1
	                        ),T3 as 
	                        (
		                        select *,(select max(RowNum) from T2) maxnum from T2 
		                        where rownum between @pageS and @pageE
	                        )
	                        select a.* from T3 a order by a.RowNum";

                IList<HistoryQuery> ilst = base.SearchList<HistoryQuery>(strSql);
                if (ilst != null)
                {
                    if (ilst.Count > 0)
                    {
                        var ilist = GetCodeData("STATUS_NAME");
                        foreach (HistoryQuery item in ilst)
                        {
                            var obj = ilist.FirstOrDefault(a => a.CodeNo == item.Status);
                            if (obj != null)
                                item.StatusShow = obj.CodeDesc;
                            else
                                item.StatusShow = item.Status;
                        }
                        base.DataRecords = ilst[0].maxnum;
                    }
                    else
                    {
                        base.DataRecords = 0;
                        ilst = new List<HistoryQuery>();
                    }
                    return ilst;
                }
                else
                {
                    return new List<HistoryQuery>();
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public JsonReturn SaveSendAgain(Guid caseId, string userId)
        {
            CaseHistoryBIZ historyBiz = new CaseHistoryBIZ();
            CaseMasterBIZ masterBiz = new CaseMasterBIZ();
            CaseObligorBIZ caseobligor = new CaseObligorBIZ();
            CaseAttachmentBIZ attachment = new CaseAttachmentBIZ();
            CaseMaster newMaster = new CaseMaster();
            Guid newcaseId = Guid.NewGuid();
            IDbConnection dbConnection = OpenConnection();
            IDbTransaction dbTrans = null;
            bool bFlag = true;
            try
            {
                using (dbConnection)
                {
                    dbTrans = dbConnection.BeginTransaction();
                    CaseMaster master = masterBiz.MasterModel(caseId, dbTrans);
                    newMaster = master;//找到原来的model，赋值再重新新增
                    int pos = master.CaseNo.IndexOf("_", 0);
                    if (pos>8)
                    {
                        master.CaseNo = master.CaseNo.Substring(0, pos);
                    }
                    string sendCaseNo = IsExistSendAgain(master.CaseNo + "_1");//*判斷是否已有再次發文
                    string maxCaseNo = MaxCaseNoSendAgain(master.CaseNo);//*找再次發文的最大案件編號

                    if (sendCaseNo != "")//*已有再次發文
                    {
                        //*找到最大次數的再次發文
                        if (Convert.ToInt32(maxCaseNo.Substring(maxCaseNo.Length - 1, 1)) < 9)//*再次發文最多執行9次
                        {
                            newMaster.CaseNo = maxCaseNo.Substring(0, maxCaseNo.Length - 1) + (Convert.ToInt32(maxCaseNo.Substring(maxCaseNo.Length - 1, 1)) + 1).ToString();
                        }
                        else
                        {
                            dbTrans.Commit();
                            return new JsonReturn { ReturnCode = "2", ReturnMsg = "" };
                        }
                    }
                    else//*沒有再次發文
                    {
                        newMaster.CaseNo = master.CaseNo + "_1";
                    }

                    #region 公文资讯
                    #region 主表
                    newMaster.CaseId = newcaseId;
                    newMaster.Status = CaseStatus.AgentEdit;
                    newMaster.ReceiveDate = Convert.ToDateTime(newMaster.ReceiveDate).ToString("yyyy/MM/dd");
                    newMaster.AgentUser = userId;
                    newMaster.CreatedUser = userId;
                    newMaster.CreatedDate = DateTime.Now.ToString("yyyy/MM/dd");
                    newMaster.ModifiedUser = userId;
                    newMaster.ModifiedDate = DateTime.Now.ToString("yyyy/MM/dd");
                    //* 限辦日期
                    if (newMaster.CaseKind == CaseKind.CASE_SEIZURE)
                    {
                        newMaster.LimitDate = DateTime.Now.AddDays(Convert.ToInt32(new PARMCodeBIZ().GetCodeNoByCodeDesc("LIMITDATE1"))).ToString("yyyy/MM/dd");
                    }
                    else if (newMaster.CaseKind == CaseKind.CASE_EXTERNAL)
                    {
                        PARMWorkingDayBIZ workDate = new PARMWorkingDayBIZ();
                        PARMCodeBIZ para = new PARMCodeBIZ();
                        DataTable dt = workDate.GetWorkDate();
                        if (dt != null && dt.Rows.Count > 0)
                        {
                            newMaster.LimitDate = dt.Rows[Convert.ToInt32(para.GetCodeNoByCodeDesc("LIMITDATE2"))]["workdate"].ToString();
                        }
                    }
                    //* PayDay


                    bFlag = bFlag && masterBiz.Create(newMaster, dbTrans) > 0;
                    #endregion

                    #region 責任人
                    List<CaseObligor> list = caseobligor.ObligorModel(caseId);
                    if (list != null && list.Count > 0)
                    {
                        foreach (CaseObligor obligor in list)
                        {
                            obligor.CaseId = newcaseId;
                            obligor.CreatedUser = userId;
                            obligor.CreatedDate = DateTime.Now;
                        }
                        bFlag = bFlag && caseobligor.Create(list, dbTrans) > 0;
                        bFlag = bFlag && caseobligor.CreateLog(list, newMaster, dbTrans) > 0;
                    }
                    #endregion

                    #region 附件
                    List<CaseAttachment> listAttachment = attachment.AttachmentList(caseId, dbTrans);
                    if (listAttachment != null && listAttachment.Count > 0)
                    {
                        foreach (CaseAttachment attach in listAttachment)
                        {
                            attach.CaseId = newcaseId;
                            attach.CreatedUser = userId;
                            attach.CreatedDate = DateTime.Now;
                            bFlag = bFlag && attachment.Create(attach, dbTrans) > 0;
                        }
                    }
                    #endregion

                    #region 歷史記錄
                    historyBiz.insertCaseHistory(newcaseId, CaseStatus.CaseInput, userId, dbTrans);
                    #endregion
                    #endregion

                    //#region 賬務資訊(待補充)
                    //#region 扣押資訊
                    //CaseAccountBiz caseAccountbiz = new CaseAccountBiz();
                    //List<CaseSeizure> seizureList = caseAccountbiz.GetCaseSeizure(caseId, dbTrans).ToList();
                    //if (seizureList != null && seizureList.Count > 0)
                    //{
                    //    foreach (CaseSeizure item in seizureList)
                    //    {
                    //        item.CaseId = newcaseId;
                    //        if (item.PayCaseId != null)
                    //        {
                    //            item.PayCaseId = newcaseId;
                    //        }
                    //        item.CaseNo = newMaster.CaseNo;
                    //        item.CreatedUser = userId;
                    //        bFlag = bFlag && caseAccountbiz.InsertSeizureSetting(item, dbTrans);
                    //    }
                    //}
                    //#endregion

                    //#region 外來文
                    //List<CaseAccountExternal> externalList = caseAccountbiz.GetDataFromCAEnal(caseId).ToList();
                    //if (externalList != null && externalList.Count > 0)
                    //{
                    //    int a = 0;
                    //    foreach (CaseAccountExternal item in externalList)
                    //    {
                    //        a++;
                    //        item.CaseId = newcaseId;
                    //        bFlag = bFlag && caseAccountbiz.InsertCAEnal(item, a, dbTrans) > 0;
                    //    }
                    //}
                    ////casemo
                    //#endregion

                    //#region 收款人

                    //#endregion
                    //#endregion

                    //#region 會辦資訊
                    //#region 主表
                    //CaseMeetBiz casemeetBiz = new CaseMeetBiz();
                    //CaseMeetMaster meetmaster = casemeetBiz.GetCaseMeetMaster(caseId);
                    //if (meetmaster != null)
                    //{
                    //    meetmaster.CaseId = newcaseId;
                    //    bFlag = bFlag && casemeetBiz.InsertCaseMeetMaster(meetmaster, dbTrans);
                    //}
                    //#endregion

                    //#region 從表
                    //List<CaseMeetDetails> meetDetailList = casemeetBiz.GetCaseMeetDetails(caseId);
                    //if (meetDetailList != null && meetDetailList.Count > 0)
                    //{
                    //    foreach (CaseMeetDetails item in meetDetailList)
                    //    {
                    //        item.CaseId = newcaseId;
                    //        bFlag = bFlag && casemeetBiz.InsertCaseMeetDetails(item, dbTrans);
                    //    }
                    //}
                    //#endregion
                    //#endregion

                    //#region 正本備查
                    //LendDataBIZ lendData = new LendDataBIZ();
                    //string lendid = string.Empty;
                    //List<LendData> lendDataList = lendData.GetLendListByCaseId(caseId);
                    //if (lendDataList != null && lendDataList.Count > 0)
                    //{
                    //    foreach (LendData item in lendDataList)
                    //    {
                    //        item.CaseId = newcaseId;
                    //        LendAttachment attamodel = new LendAttachmentBIZ().GetAttachByCaseIdandLendID(caseId, item.LendID.ToString());
                    //        bFlag=bFlag&&  lendData.Create(item, ref lendid, dbTrans)>0;
                    //        if (attamodel != null)//*附件
                    //        {
                    //            attamodel.CaseId = newcaseId;
                    //            attamodel.LendId = Convert.ToInt32(lendid);
                    //            bFlag = bFlag && new LendAttachmentBIZ().Create(attamodel, dbTrans) > 0;
                    //        }
                    //    }
                    //}
                    //#endregion

                    //#region 資訊部調閱
                    //List<AgentDepartmentAccess> departMentList = new AgentDepartmentAccessBIZ().GetDataFromCaseDtAccess(caseId).ToList();
                    //if (departMentList != null && departMentList.Count > 0)
                    //{
                    //    foreach (AgentDepartmentAccess item in departMentList)
                    //    {
                    //        item.CaseId = newcaseId;
                    //        item.CreatedUser = userId;
                    //        item.CreatedDate = DateTime.Now;
                    //        bFlag = bFlag && new AgentDepartmentAccessBIZ().Create(item, dbTrans);
                    //    }
                    //}
                    //#endregion

                    //#region 發文資訊
                    //List<CaseSendSetting> caseSendSetList = new CaseSendSettingBIZ().GetSendSettingByCaseIdList(caseId);

                    //if (caseSendSetList != null && caseSendSetList.Count > 0)
                    //{
                    //    foreach (CaseSendSetting item in caseSendSetList)
                    //    {
                    //        item.CaseId = newcaseId;
                    //        item.CreatedUser = userId;
                    //        item.CreatedDate = DateTime.Now;

                    //        //bFlag = bFlag && new AgentDepartmentAccessBIZ().Create(item, dbTrans);
                    //    }
                    //    CaseSendSettingCreateViewModel sendModel = new CaseSendSettingCreateViewModel()
                    //    {
                    //        ReceiveList = caseSendSetList
                    //    };
                    //}
                    //#region 發文明細
                    //IList<CaseSendSettingDetails> sendSetDetailList = new CaseSendSettingBIZ().GetSendSettingDetails(caseId);
                    //if (sendSetDetailList != null && sendSetDetailList.Count > 0)
                    //{
                    //    foreach (CaseSendSettingDetails item in sendSetDetailList)
                    //    {
                    //        item.CaseId = newcaseId;
                    //        //bFlag = bFlag && new AgentDepartmentAccessBIZ().Create(item, dbTrans);
                    //    }
                    //}
                    //#endregion
                    //#endregion

                    //#region 流程記錄
                    //List<CaseHistory> historyList = new CaseHistoryBIZ().getHistoryByCaseId(caseId);
                    //if (historyList != null && historyList.Count > 0)
                    //{
                    //    foreach (CaseHistory item in historyList)
                    //    {
                    //        item.CaseId = newcaseId;
                    //        item.CreatedUser = userId;
                    //        item.CreatedDate = DateTime.Now;
                    //        bFlag = bFlag && new CaseMemoBiz().Create(item, dbTrans);
                    //    }
                    //}
                    //#endregion

                    //#region 利息計算
                    //#region 主表
                    //CaseCalculatorMain mainModel = new CaseCalculatorMainBIZ().GetCaseMainInfo(caseId);
                    //mainModel.CaseId = newcaseId;
                    //mainModel.CreatedUser = userId;
                    //mainModel.CreatedDate = DateTime.Now;
                    //bFlag = bFlag &&  new CaseCalculatorMainBIZ().Create(newcaseId, Convert.ToInt32(mainModel.Amount1),  Convert.ToInt32(mainModel.Amount2),
                    // Convert.ToInt32(mainModel.Amount3),  Convert.ToInt32(mainModel.Amount4),  Convert.ToInt32(mainModel.Amount5), dbTrans)>0;
                    //#endregion

                    //#region 從表
                    //List<CaseCalculatorDetails> detailList = new CaseCalculatorDetailsBIZ().DetailModel(caseId);
                    //if (detailList != null && detailList.Count > 0)
                    //{
                    //    foreach (CaseCalculatorDetails item in detailList)
                    //    {
                    //        item.CaseId = newcaseId;
                    //        bFlag = bFlag && new CaseCalculatorDetailsBIZ().Create(item, dbTrans)>0;
                    //    }
                    //}
                    //#endregion
                    //#endregion

                    //#region 內部註記
                    //List<CaseMemo> casememoList = new CaseMemoBiz().GetQueryListByCaseId(caseId);
                    //if (casememoList != null && casememoList.Count > 0)
                    //{
                    //    foreach (CaseMemo item in casememoList)
                    //    {
                    //        item.CaseId = newcaseId;
                    //        item.MemoUser = userId;
                    //        item.MemoDate = DateTime.Now.ToString("yyyy/MM/dd");
                    //        bFlag = bFlag && new CaseMemoBiz().Create(item, dbTrans);
                    //    }
                    //}
                    //#endregion

                    if (bFlag)
                    {
                        dbTrans.Commit();
                        return new JsonReturn { ReturnCode = "1", ReturnMsg = "" };
                    }
                    dbTrans.Rollback();
                    return new JsonReturn { ReturnCode = "0", ReturnMsg = "" };
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
                return new JsonReturn { ReturnCode = "0", ReturnMsg = "" };
            }
        }

        //判斷是否該案件是否存在二次回文
        public string IsExistSendAgain(string caseNo)
        {
            string strSql = "select c.CaseNo from History_CaseMaster c where  c.CaseNo =@CaseNo";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("@CaseNo", caseNo));

            try
            {
                object caseno = base.ExecuteScalar(strSql);
                if (caseno != null && caseno != "")
                {
                    return caseno.ToString();//有重複
                }
                return "";//無重複
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        //找到最大一筆再次發文案件編號
        public string MaxCaseNoSendAgain(string caseNo)
        {
            string strSql = "select top 1 CaseNo from History_CaseMaster where CaseNo like @CaseNo order by CaseNo desc";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("@CaseNo", "%" + caseNo + "%"));           
            try
            {
                object caseno = base.ExecuteScalar(strSql);
                if (caseno != null && caseno != "")
                {
                    return caseno.ToString();
                }
                return "";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public IList<HistoryQuery> GetStatusName(string colName)
        {
            try
            {
                string strSql = @" select CodeDesc,CodeNo from PARMCode where CodeType=@CodeType ORDER BY SortOrder";
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@CodeType", colName));
                return base.SearchList<HistoryQuery>(strSql);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        //public MemoryStream Exportlist(string caseId, string GovDateS, string GovDateE)
        //{
        //   string[] headerColumns = new[]
        //            {
        //                "序號",
        //                "案件編號",//caseno
        //                "類別",//caseKind
        //                "細分類",//caseKind2
        //                "速別",//speed
        //                "來文機關",//GovUnit
        //                "來文字號",//GovNO
        //                "來文日期",//GovDate
        //                "限辦日期",//limitdate
        //                "經辦人員",//person
        //                "結案日期", //結案日起
        //                "狀態"//狀態
        //            };
        //   string[] ColumnsName = new[]
        //            {
        //                "Num",
        //                "CaseNo",//caseno
        //                "CaseKind",//caseKind
        //                "CaseKind2",//caseKind2
        //                "Speed",//speed
        //                "GovUnit",//GovUnit
        //                "GovNO",//GovNO
        //                "GovDate",//GovDate
        //                "LimitDate",//limitdate
        //                "AgentUser",//person
        //                "CloseDate", //結案日起
        //                "Status"//狀態
        //            };
        //   string[] caseid = caseId.Split(',');
        //   DataTable dt = new DataTable();
        //   DataTable dt2 = new DataTable();
        //   DataTable dt3 = new DataTable();
        //   List<Guid> aryCaseId = (from id in caseid where !string.IsNullOrEmpty(id) select new Guid(id)).ToList();
        //   for (int i = 0; i < aryCaseId.Count; i++)
        //   {
        //      DataTable list = getCaseId(aryCaseId[i], "分行集作一科");
        //      DataTable list2 = getCaseId(aryCaseId[i], "分行集作二科");
        //      DataTable list3 = getCaseId(aryCaseId[i], "集作三科");
        //      if (i == 0)
        //      {
        //         dt = list.Clone();
        //         dt2 = list2.Clone();
        //         dt3 = list3.Clone();
        //      }
        //      DataRow dr = dt.NewRow();
        //      if (list != null && list.Rows.Count > 0)
        //      {
        //         var _ilist = GetCodeData("STATUS_NAME");
        //         for (int j = 0; j < list.Rows.Count; j++)
        //         {
        //            var obj = _ilist.FirstOrDefault(a => a.CodeNo == list.Rows[j]["Status"]);
        //            if (obj != null)
        //            {
        //               list.Rows[j]["Status"] = obj.CodeDesc;
        //            }
        //            else
        //            {
        //               list.Rows[j]["Status"] = list.Rows[j]["Status"];
        //            }

        //            //dr.ItemArray = list.Rows[j].ItemArray;
        //            dt.Rows.Add(list.Rows[j].ItemArray);
        //         }
        //      }
        //      else
        //      {
        //         new DataTable();
        //      }

        //      //DataRow dr2 = dt2.NewRow();
        //      if (list2 != null && list2.Rows.Count > 0)
        //      {
        //         var _ilist = GetCodeData("STATUS_NAME");
        //         for (int j = 0; j < list2.Rows.Count; j++)
        //         {
        //            var obj = _ilist.FirstOrDefault(a => a.CodeNo == list2.Rows[j]["Status"]);
        //            if (obj != null)
        //            {
        //               list2.Rows[j]["Status"] = obj.CodeDesc;
        //            }
        //            else
        //            {
        //               list2.Rows[j]["Status"] = list2.Rows[j]["Status"];
        //            }

        //            //dr2.ItemArray = list2.Rows[j].ItemArray;
        //            dt2.Rows.Add(list2.Rows[j].ItemArray);
        //         }
        //      }
        //      else
        //      {
        //         new DataTable();
        //      }

        //      //DataRow dr3 = dt3.NewRow();
        //      if (list3 != null && list3.Rows.Count > 0)
        //      {
        //         var _ilist = GetCodeData("STATUS_NAME");
        //         for (int j = 0; j < list3.Rows.Count; j++)
        //         {
        //            var obj = _ilist.FirstOrDefault(a => a.CodeNo == list3.Rows[j]["Status"]);
        //            if (obj != null)
        //            {
        //               list3.Rows[j]["Status"] = obj.CodeDesc;
        //            }
        //            else
        //            {
        //               list3.Rows[j]["Status"] = list3.Rows[j]["Status"];
        //            }
        //            //dr3.ItemArray = list3.Rows[j].ItemArray;
        //            dt3.Rows.Add(list3.Rows[j].ItemArray);
        //         }
        //      }
        //      else
        //      {
        //         new DataTable();
        //      }
        //   }
        //   IWorkbook workbook = new HSSFWorkbook();
        //   ISheet sheet = workbook.CreateSheet("分行集作一科");
        //   ISheet sheet2 = workbook.CreateSheet("分行集作二科");
        //   ISheet sheet3 = workbook.CreateSheet("集作三科");

        //   #region def style

        //   ICellStyle styleHead10 = workbook.CreateCellStyle();
        //   IFont font10 = workbook.CreateFont();
        //   font10.FontHeightInPoints = 10;
        //   font10.FontName = "新細明體";
        //   styleHead10.FillPattern = FillPattern.SolidForeground;
        //   styleHead10.FillForegroundColor = HSSFColor.White.Index;
        //   styleHead10.BorderTop = BorderStyle.Thin;
        //   styleHead10.BorderLeft = BorderStyle.Thin;
        //   styleHead10.BorderRight = BorderStyle.Thin;
        //   styleHead10.BorderBottom = BorderStyle.Thin;
        //   styleHead10.WrapText = true;
        //   styleHead10.Alignment = HorizontalAlignment.Left;
        //   styleHead10.VerticalAlignment = VerticalAlignment.Center;
        //   styleHead10.SetFont(font10);


        //   ICellStyle styleHead0 = workbook.CreateCellStyle();
        //   IFont font0 = workbook.CreateFont();
        //   font0.FontHeightInPoints = 10;
        //   font0.FontName = "新細明體";
        //   styleHead0.FillPattern = FillPattern.SolidForeground;
        //   styleHead0.FillForegroundColor = HSSFColor.White.Index;
        //   styleHead0.BorderTop = BorderStyle.Thin;
        //   styleHead0.BorderLeft = BorderStyle.Thin;
        //   styleHead0.BorderRight = BorderStyle.Thin;
        //   styleHead0.BorderBottom = BorderStyle.Thin;
        //   styleHead0.WrapText = true;
        //   styleHead0.Alignment = HorizontalAlignment.Center;
        //   styleHead0.VerticalAlignment = VerticalAlignment.Center;
        //   styleHead0.SetFont(font0);

        //   ICellStyle styleBody = workbook.CreateCellStyle();
        //   IFont fontBody = workbook.CreateFont();
        //   fontBody.FontHeightInPoints = 10;
        //   fontBody.FontName = "新細明體";
        //   styleBody.WrapText = true;
        //   styleBody.Alignment = HorizontalAlignment.Center;
        //   styleBody.VerticalAlignment = VerticalAlignment.Center;
        //   styleBody.BorderTop = BorderStyle.Thin;
        //   styleBody.BorderLeft = BorderStyle.Thin;
        //   styleBody.BorderRight = BorderStyle.Thin;
        //   styleBody.BorderBottom = BorderStyle.Thin;
        //   styleBody.TopBorderColor = HSSFColor.Grey25Percent.Index;
        //   styleBody.LeftBorderColor = HSSFColor.Grey25Percent.Index;
        //   styleBody.RightBorderColor = HSSFColor.Grey25Percent.Index;
        //   styleBody.BottomBorderColor = HSSFColor.Grey25Percent.Index;
        //   styleBody.SetFont(fontBody);

        //   ICellStyle styleBody1 = workbook.CreateCellStyle();
        //   IFont fontBody1 = workbook.CreateFont();
        //   fontBody1.FontHeightInPoints = 11;
        //   fontBody1.FontName = "新細明體";
        //   fontBody1.Boldweight = (short)FontBoldWeight.Bold;
        //   styleBody1.WrapText = true;
        //   styleBody1.Alignment = HorizontalAlignment.Center;
        //   styleBody1.VerticalAlignment = VerticalAlignment.Center;
        //   styleBody1.BorderTop = BorderStyle.Thin;
        //   styleBody1.BorderLeft = BorderStyle.Thin;
        //   styleBody1.BorderRight = BorderStyle.Thin;
        //   styleBody1.BorderBottom = BorderStyle.Thin;
        //   styleBody1.TopBorderColor = HSSFColor.Grey25Percent.Index;
        //   styleBody1.LeftBorderColor = HSSFColor.Grey25Percent.Index;
        //   styleBody1.RightBorderColor = HSSFColor.Grey25Percent.Index;
        //   styleBody1.BottomBorderColor = HSSFColor.Grey25Percent.Index;
        //   styleBody1.SetFont(fontBody1);


        //   #endregion

        //   #region body
        //   Dictionary<string, ArrayList> dc = new Dictionary<string, ArrayList>();
        //   foreach (DataRow item in dt.Rows)
        //   {
        //      ArrayList arrlist = new ArrayList();
        //      if (!dc.ContainsKey(item["EmpName"].ToString()))
        //      {
        //         arrlist.Add(item["CaseNo"]);
        //         dc.Add(item["EmpName"].ToString(), arrlist);
        //      }
        //      else
        //      {
        //         arrlist = dc[item["EmpName"].ToString()];
        //         arrlist.Add(item["CaseNo"]);
        //         dc[item["EmpName"].ToString()] = arrlist;
        //      }
        //   }
        //   if (dc.Count > 0)
        //   {
        //      int x = dc.Count / 2 + 1;
        //      #region title
        //      SetExcelCell(sheet, 0, 0, styleBody1, "收文清單");

        //      sheet.AddMergedRegion(new CellRangeAddress(0, 0, 0, dc.Count));

        //      SetExcelCell(sheet, 1, 0, styleBody, "收文日期");
        //      sheet.AddMergedRegion(new CellRangeAddress(1, 1, 0, 0));
        //      SetExcelCell(sheet, 1, 1, styleBody, GovDateS + "~" + GovDateE);
        //      sheet.AddMergedRegion(new CellRangeAddress(1, 1, 1, 1));

        //      if (dc.Count > 2)
        //      {
        //         SetExcelCell(sheet, 1, x, styleBody, "部门别/科别");
        //         sheet.AddMergedRegion(new CellRangeAddress(1, 1, x, x));
        //         SetExcelCell(sheet, 1, dc.Count, styleBody, "分行集作一科");
        //         sheet.AddMergedRegion(new CellRangeAddress(1, 1, dc.Count, dc.Count));
        //      }
        //      else
        //      {
        //         SetExcelCell(sheet, 1, 2, styleBody, "部门别/科别");
        //         sheet.AddMergedRegion(new CellRangeAddress(1, 1, 2, 2));
        //         SetExcelCell(sheet, 1, 3, styleBody, "分行集作一科");
        //         sheet.AddMergedRegion(new CellRangeAddress(1, 1, 3, 3));
        //      }

        //      #endregion
        //      int a = 0;
        //      int b = 0;
        //      int c = 0;
        //      foreach (KeyValuePair<string, ArrayList> item in dc)
        //      {
        //         c = item.Value.Count > c ? item.Value.Count : c;
        //      }
        //      b = c - c + 1;
        //      foreach (KeyValuePair<string, ArrayList> item in dc)
        //      {
        //         sheet.SetColumnWidth(0, 100 * 30);
        //         sheet.SetColumnWidth(a + 1, 100 * 30);
        //         for (int i = 0; i < item.Value.Count; i++)
        //         {
        //            SetExcelCell(sheet, 2, 0, styleHead10, "序號");
        //            SetExcelCell(sheet, i + 3, 0, styleHead10, Convert.ToString(b + i));
        //            if (item.Value.Count < c)
        //            {
        //               SetExcelCell(sheet, 2, a + 1, styleHead10, Convert.ToString(item.Key));
        //               for (int j = 0; j < c; j++)
        //               {
        //                  if (j < item.Value.Count)
        //                  {
        //                     SetExcelCell(sheet, j + 3, a + 1, styleHead10, Convert.ToString(item.Value[j]));
        //                  }
        //                  else
        //                  {
        //                     SetExcelCell(sheet, j + 3, a + 1, styleHead10, "");
        //                  }

        //               }
        //            }
        //            else
        //            {
        //               SetExcelCell(sheet, 2, a + 1, styleHead10, Convert.ToString(item.Key));
        //               SetExcelCell(sheet, i + 3, a + 1, styleHead10, Convert.ToString(item.Value[i]));
        //            }

        //         }
        //         a++;
        //      }
        //   }
        //   #endregion

        //   #region body2
        //   Dictionary<string, ArrayList> dc2 = new Dictionary<string, ArrayList>();
        //   foreach (DataRow item in dt2.Rows)
        //   {
        //      ArrayList arrlist = new ArrayList();
        //      if (!dc2.ContainsKey(item["EmpName"].ToString()))
        //      {
        //         arrlist.Add(item["CaseNo"]);
        //         dc2.Add(item["EmpName"].ToString(), arrlist);
        //      }
        //      else
        //      {
        //         arrlist = dc2[item["EmpName"].ToString()];
        //         arrlist.Add(item["CaseNo"]);
        //         dc2[item["EmpName"].ToString()] = arrlist;
        //      }
        //   }
        //   if (dc2.Count > 0)
        //   {
        //      int x = dc2.Count / 2 + 1;
        //      #region title
        //      SetExcelCell(sheet2, 0, 0, styleBody1, "收文清單");

        //      sheet2.AddMergedRegion(new CellRangeAddress(0, 0, 0, dc2.Count));

        //      SetExcelCell(sheet2, 1, 0, styleBody, "收文日期");
        //      sheet2.AddMergedRegion(new CellRangeAddress(1, 1, 0, 0));
        //      SetExcelCell(sheet2, 1, 1, styleBody, GovDateS + "~" + GovDateE);
        //      sheet2.AddMergedRegion(new CellRangeAddress(1, 1, 1, 1));


        //      if (dc2.Count > 2)
        //      {
        //         if (x == 1) x = x + 1;
        //         SetExcelCell(sheet2, 1, x, styleBody, "部门别/科别");
        //         sheet2.AddMergedRegion(new CellRangeAddress(1, 1, x, x));
        //         SetExcelCell(sheet2, 1, dc2.Count, styleBody, "分行集作二科");
        //         sheet2.AddMergedRegion(new CellRangeAddress(1, 1, dc2.Count, dc2.Count));
        //      }
        //      else
        //      {
        //         if (x == 1) x = x + 1;
        //         SetExcelCell(sheet2, 1, x, styleBody, "部门别/科别");
        //         sheet2.AddMergedRegion(new CellRangeAddress(1, 1, x, x));
        //         SetExcelCell(sheet2, 1, x + 1, styleBody, "分行集作二科");
        //         sheet2.AddMergedRegion(new CellRangeAddress(1, 1, dc2.Count, dc2.Count));
        //      }

        //      #endregion
        //      int a = 0;
        //      int b = 0;
        //      int c = 0;
        //      foreach (KeyValuePair<string, ArrayList> item in dc2)
        //      {
        //         c = item.Value.Count > c ? item.Value.Count : c;
        //      }
        //      b = c - c + 1;
        //      foreach (KeyValuePair<string, ArrayList> item in dc2)
        //      {
        //         sheet2.SetColumnWidth(0, 100 * 30);
        //         sheet2.SetColumnWidth(a + 1, 100 * 30);
        //         for (int i = 0; i < item.Value.Count; i++)
        //         {
        //            SetExcelCell(sheet2, 2, 0, styleHead10, "序號");
        //            SetExcelCell(sheet2, i + 3, 0, styleHead10, Convert.ToString(b + i));
        //            if (item.Value.Count < c)
        //            {
        //               SetExcelCell(sheet2, 2, a + 1, styleHead10, Convert.ToString(item.Key));
        //               for (int j = 0; j < c; j++)
        //               {
        //                  if (j < item.Value.Count)
        //                  {
        //                     SetExcelCell(sheet2, j + 3, a + 1, styleHead10, Convert.ToString(item.Value[j]));
        //                  }
        //                  else
        //                  {
        //                     SetExcelCell(sheet2, j + 3, a + 1, styleHead10, "");
        //                  }

        //               }
        //            }
        //            else
        //            {
        //               SetExcelCell(sheet2, 2, a + 1, styleHead10, Convert.ToString(item.Key));
        //               SetExcelCell(sheet2, i + 3, a + 1, styleHead10, Convert.ToString(item.Value[i]));
        //            }
        //         }
        //         a++;
        //      }
        //   }
        //   #endregion

        //   #region body3
        //   Dictionary<string, ArrayList> dc3 = new Dictionary<string, ArrayList>();
        //   foreach (DataRow item in dt3.Rows)
        //   {
        //      ArrayList arrlist = new ArrayList();
        //      if (!dc3.ContainsKey(item["EmpName"].ToString()))
        //      {
        //         arrlist.Add(item["CaseNo"]);
        //         dc3.Add(item["EmpName"].ToString(), arrlist);
        //      }
        //      else
        //      {
        //         arrlist = dc3[item["EmpName"].ToString()];
        //         arrlist.Add(item["CaseNo"]);
        //         dc3[item["EmpName"].ToString()] = arrlist;
        //      }
        //   }
        //   if (dc3.Count > 0)
        //   {
        //      int x = dc3.Count / 2 + 1;
        //      #region title
        //      SetExcelCell(sheet3, 0, 0, styleBody1, "收文清單");

        //      sheet3.AddMergedRegion(new CellRangeAddress(0, 0, 0, dc3.Count));

        //      SetExcelCell(sheet3, 1, 0, styleBody, "收文日期");
        //      sheet3.AddMergedRegion(new CellRangeAddress(1, 1, 0, 0));
        //      SetExcelCell(sheet3, 1, 1, styleBody, GovDateS + "~" + GovDateE);
        //      sheet3.AddMergedRegion(new CellRangeAddress(1, 1, 1, 1));

        //      if (dc3.Count > 2)
        //      {
        //         if (x == 1) x = x + 1;
        //         SetExcelCell(sheet3, 1, x, styleBody, "部门别/科别");
        //         sheet3.AddMergedRegion(new CellRangeAddress(1, 1, x, x));
        //         SetExcelCell(sheet3, 1, dc3.Count, styleBody, "集作三科");
        //         sheet3.AddMergedRegion(new CellRangeAddress(1, 1, dc3.Count, dc3.Count));
        //      }
        //      else
        //      {
        //         if (x == 1) x = x + 1;
        //         SetExcelCell(sheet3, 1, x, styleBody, "部门别/科别");
        //         sheet3.AddMergedRegion(new CellRangeAddress(1, 1, x, x));
        //         SetExcelCell(sheet3, 1, x + 1, styleBody, "集作三科");
        //         sheet3.AddMergedRegion(new CellRangeAddress(1, 1, dc3.Count, dc3.Count));
        //      }
        //      #endregion

        //      int a = 0;
        //      int b = 0;
        //      int c = 0;
        //      foreach (KeyValuePair<string, ArrayList> item in dc3)
        //      {
        //         c = item.Value.Count > c ? item.Value.Count : c;
        //      }
        //      b = c - c + 1;
        //      foreach (KeyValuePair<string, ArrayList> item in dc3)
        //      {
        //         sheet3.SetColumnWidth(0, 100 * 30);
        //         sheet3.SetColumnWidth(a + 1, 100 * 30);
        //         for (int i = 0; i < item.Value.Count; i++)
        //         {
        //            SetExcelCell(sheet3, 2, 0, styleHead10, "序號");
        //            SetExcelCell(sheet3, i + 3, 0, styleHead10, Convert.ToString(b + i));
        //            if (item.Value.Count < c)
        //            {
        //               SetExcelCell(sheet3, 2, a + 1, styleHead10, Convert.ToString(item.Key));
        //               for (int j = 0; j < c; j++)
        //               {
        //                  if (j < item.Value.Count)
        //                  {
        //                     SetExcelCell(sheet3, j + 3, a + 1, styleHead10, Convert.ToString(item.Value[j]));
        //                  }
        //                  else
        //                  {
        //                     SetExcelCell(sheet3, j + 3, a + 1, styleHead10, "");
        //                  }

        //               }
        //            }
        //            else
        //            {
        //               SetExcelCell(sheet3, 2, a + 1, styleHead10, Convert.ToString(item.Key));
        //               SetExcelCell(sheet3, i + 3, a + 1, styleHead10, Convert.ToString(item.Value[i]));
        //            }
        //         }
        //         a++;
        //      }
        //   }
        //   #endregion

        //   MemoryStream ms = new MemoryStream();
        //   workbook.Write(ms);
        //   ms.Flush();
        //   ms.Position = 0;
        //   workbook = null;
        //   return ms;
        //}

        //20171103 RC RQ-2015-019666-003 系統功能修正 宏祥 update start
        public MemoryStream Exportlist(string caseId, string GovDateS, string GovDateE, IList<PARMCode> listCode)
        {
           string[] headerColumns = new[]
                    {
                        "序號",
                        "案件編號",//caseno
                        "類別",//caseKind
                        "細分類",//caseKind2
                        "速別",//speed
                        "來文機關",//GovUnit
                        "來文字號",//GovNO
                        "來文日期",//GovDate
                        "限辦日期",//limitdate
                        "經辦人員",//person
                        "結案日期", //結案日起
                        "狀態"//狀態
                    };
           string[] ColumnsName = new[]
                    {
                        "Num",
                        "CaseNo",//caseno
                        "CaseKind",//caseKind
                        "CaseKind2",//caseKind2
                        "Speed",//speed
                        "GovUnit",//GovUnit
                        "GovNO",//GovNO
                        "GovDate",//GovDate
                        "LimitDate",//limitdate
                        "AgentUser",//person
                        "CloseDate", //結案日起
                        "Status"//狀態
                    };
           string[] caseid = caseId.Split(',');
           //DataTable dt = new DataTable();
           //DataTable dt2 = new DataTable();
           //DataTable dt3 = new DataTable();
           List<Guid> aryCaseId = (from id in caseid where !string.IsNullOrEmpty(id) select new Guid(id)).ToList();

           IWorkbook workbook = new HSSFWorkbook();
           int Departmentcount = listCode.Count();

           for (int k = 0; k < Departmentcount; k++)
           {
              ISheet sheet = null;
              DataTable dt = new DataTable();

              for (int i = 0; i < aryCaseId.Count; i++)
              {
                 DataTable list = getCaseId(aryCaseId[i], listCode[k].CodeDesc);
                 //DataTable list2 = getCaseId(aryCaseId[i], "分行集作二科");
                 //DataTable list3 = getCaseId(aryCaseId[i], "集作三科");
                 if (i == 0)
                 {
                    dt = list.Clone();
                    //dt2 = list2.Clone();
                    //dt3 = list3.Clone();
                 }
                 DataRow dr = dt.NewRow();
                 if (list != null && list.Rows.Count > 0)
                 {
                    var _ilist = GetCodeData("STATUS_NAME");
                    for (int j = 0; j < list.Rows.Count; j++)
                    {
                       var obj = _ilist.FirstOrDefault(a => a.CodeNo == list.Rows[j]["Status"]);
                       if (obj != null)
                       {
                          list.Rows[j]["Status"] = obj.CodeDesc;
                       }
                       else
                       {
                          list.Rows[j]["Status"] = list.Rows[j]["Status"];
                       }

                       //dr.ItemArray = list.Rows[j].ItemArray;
                       dt.Rows.Add(list.Rows[j].ItemArray);
                    }
                 }
                 else
                 {
                    new DataTable();
                 }

                 //DataRow dr2 = dt2.NewRow();
                 //if (list2 != null && list2.Rows.Count > 0)
                 //{
                 //   var _ilist = GetCodeData("STATUS_NAME");
                 //   for (int j = 0; j < list2.Rows.Count; j++)
                 //   {
                 //      var obj = _ilist.FirstOrDefault(a => a.CodeNo == list2.Rows[j]["Status"]);
                 //      if (obj != null)
                 //      {
                 //         list2.Rows[j]["Status"] = obj.CodeDesc;
                 //      }
                 //      else
                 //      {
                 //         list2.Rows[j]["Status"] = list2.Rows[j]["Status"];
                 //      }

                 //      //dr2.ItemArray = list2.Rows[j].ItemArray;
                 //      dt2.Rows.Add(list2.Rows[j].ItemArray);
                 //   }
                 //}
                 //else
                 //{
                 //   new DataTable();
                 //}

                 //DataRow dr3 = dt3.NewRow();
                 //if (list3 != null && list3.Rows.Count > 0)
                 //{
                 //   var _ilist = GetCodeData("STATUS_NAME");
                 //   for (int j = 0; j < list3.Rows.Count; j++)
                 //   {
                 //      var obj = _ilist.FirstOrDefault(a => a.CodeNo == list3.Rows[j]["Status"]);
                 //      if (obj != null)
                 //      {
                 //         list3.Rows[j]["Status"] = obj.CodeDesc;
                 //      }
                 //      else
                 //      {
                 //         list3.Rows[j]["Status"] = list3.Rows[j]["Status"];
                 //      }
                 //      //dr3.ItemArray = list3.Rows[j].ItemArray;
                 //      dt3.Rows.Add(list3.Rows[j].ItemArray);
                 //   }
                 //}
                 //else
                 //{
                 //   new DataTable();
                 //}
              }
              //IWorkbook workbook = new HSSFWorkbook();
              sheet = workbook.CreateSheet(listCode[k].CodeDesc);
              //ISheet sheet2 = workbook.CreateSheet("分行集作二科");
              //ISheet sheet3 = workbook.CreateSheet("集作三科");

              #region def style

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
              styleHead10.Alignment = HorizontalAlignment.Left;
              styleHead10.VerticalAlignment = VerticalAlignment.Center;
              styleHead10.SetFont(font10);


              ICellStyle styleHead0 = workbook.CreateCellStyle();
              IFont font0 = workbook.CreateFont();
              font0.FontHeightInPoints = 10;
              font0.FontName = "新細明體";
              styleHead0.FillPattern = FillPattern.SolidForeground;
              styleHead0.FillForegroundColor = HSSFColor.White.Index;
              styleHead0.BorderTop = BorderStyle.Thin;
              styleHead0.BorderLeft = BorderStyle.Thin;
              styleHead0.BorderRight = BorderStyle.Thin;
              styleHead0.BorderBottom = BorderStyle.Thin;
              styleHead0.WrapText = true;
              styleHead0.Alignment = HorizontalAlignment.Center;
              styleHead0.VerticalAlignment = VerticalAlignment.Center;
              styleHead0.SetFont(font0);

              ICellStyle styleBody = workbook.CreateCellStyle();
              IFont fontBody = workbook.CreateFont();
              fontBody.FontHeightInPoints = 10;
              fontBody.FontName = "新細明體";
              styleBody.WrapText = true;
              styleBody.Alignment = HorizontalAlignment.Center;
              styleBody.VerticalAlignment = VerticalAlignment.Center;
              styleBody.BorderTop = BorderStyle.Thin;
              styleBody.BorderLeft = BorderStyle.Thin;
              styleBody.BorderRight = BorderStyle.Thin;
              styleBody.BorderBottom = BorderStyle.Thin;
              styleBody.TopBorderColor = HSSFColor.Grey25Percent.Index;
              styleBody.LeftBorderColor = HSSFColor.Grey25Percent.Index;
              styleBody.RightBorderColor = HSSFColor.Grey25Percent.Index;
              styleBody.BottomBorderColor = HSSFColor.Grey25Percent.Index;
              styleBody.SetFont(fontBody);

              ICellStyle styleBody1 = workbook.CreateCellStyle();
              IFont fontBody1 = workbook.CreateFont();
              fontBody1.FontHeightInPoints = 11;
              fontBody1.FontName = "新細明體";
              fontBody1.Boldweight = (short)FontBoldWeight.Bold;
              styleBody1.WrapText = true;
              styleBody1.Alignment = HorizontalAlignment.Center;
              styleBody1.VerticalAlignment = VerticalAlignment.Center;
              styleBody1.BorderTop = BorderStyle.Thin;
              styleBody1.BorderLeft = BorderStyle.Thin;
              styleBody1.BorderRight = BorderStyle.Thin;
              styleBody1.BorderBottom = BorderStyle.Thin;
              styleBody1.TopBorderColor = HSSFColor.Grey25Percent.Index;
              styleBody1.LeftBorderColor = HSSFColor.Grey25Percent.Index;
              styleBody1.RightBorderColor = HSSFColor.Grey25Percent.Index;
              styleBody1.BottomBorderColor = HSSFColor.Grey25Percent.Index;
              styleBody1.SetFont(fontBody1);


              #endregion

              #region body
              Dictionary<string, ArrayList> dc = new Dictionary<string, ArrayList>();
              foreach (DataRow item in dt.Rows)
              {
                 ArrayList arrlist = new ArrayList();
                 if (!dc.ContainsKey(item["EmpName"].ToString()))
                 {
                    arrlist.Add(item["CaseNo"]);
                    dc.Add(item["EmpName"].ToString(), arrlist);
                 }
                 else
                 {
                    arrlist = dc[item["EmpName"].ToString()];
                    arrlist.Add(item["CaseNo"]);
                    dc[item["EmpName"].ToString()] = arrlist;
                 }
              }
              if (dc.Count > 0)
              {
                 int x = dc.Count / 2 + 1;
                 #region title
                 SetExcelCell(sheet, 0, 0, styleBody1, "收文清單");

                 sheet.AddMergedRegion(new CellRangeAddress(0, 0, 0, dc.Count));

                 SetExcelCell(sheet, 1, 0, styleBody, "收文日期");
                 sheet.AddMergedRegion(new CellRangeAddress(1, 1, 0, 0));
                 SetExcelCell(sheet, 1, 1, styleBody, GovDateS + "~" + GovDateE);
                 sheet.AddMergedRegion(new CellRangeAddress(1, 1, 1, 1));

                 if (dc.Count > 2)
                 {
                    SetExcelCell(sheet, 1, x, styleBody, "部门别/科别");
                    sheet.AddMergedRegion(new CellRangeAddress(1, 1, x, x));
                    SetExcelCell(sheet, 1, dc.Count, styleBody, listCode[k].CodeDesc);
                    sheet.AddMergedRegion(new CellRangeAddress(1, 1, dc.Count, dc.Count));
                 }
                 else
                 {
                    SetExcelCell(sheet, 1, 2, styleBody, "部门别/科别");
                    sheet.AddMergedRegion(new CellRangeAddress(1, 1, 2, 2));
                    SetExcelCell(sheet, 1, 3, styleBody, listCode[k].CodeDesc);
                    sheet.AddMergedRegion(new CellRangeAddress(1, 1, 3, 3));
                 }

                 #endregion
                 int a = 0;
                 int b = 0;
                 int c = 0;
                 foreach (KeyValuePair<string, ArrayList> item in dc)
                 {
                    c = item.Value.Count > c ? item.Value.Count : c;
                 }
                 b = c - c + 1;
                 foreach (KeyValuePair<string, ArrayList> item in dc)
                 {
                    sheet.SetColumnWidth(0, 100 * 30);
                    sheet.SetColumnWidth(a + 1, 100 * 30);
                    for (int i = 0; i < item.Value.Count; i++)
                    {
                       SetExcelCell(sheet, 2, 0, styleHead10, "序號");
                       SetExcelCell(sheet, i + 3, 0, styleHead10, Convert.ToString(b + i));
                       if (item.Value.Count < c)
                       {
                          SetExcelCell(sheet, 2, a + 1, styleHead10, Convert.ToString(item.Key));
                          for (int j = 0; j < c; j++)
                          {
                             if (j < item.Value.Count)
                             {
                                SetExcelCell(sheet, j + 3, a + 1, styleHead10, Convert.ToString(item.Value[j]));
                             }
                             else
                             {
                                SetExcelCell(sheet, j + 3, a + 1, styleHead10, "");
                             }

                          }
                       }
                       else
                       {
                          SetExcelCell(sheet, 2, a + 1, styleHead10, Convert.ToString(item.Key));
                          SetExcelCell(sheet, i + 3, a + 1, styleHead10, Convert.ToString(item.Value[i]));
                       }

                    }
                    a++;
                 }
              }
              #endregion

              //#region body2
              //Dictionary<string, ArrayList> dc2 = new Dictionary<string, ArrayList>();
              //foreach (DataRow item in dt2.Rows)
              //{
              //   ArrayList arrlist = new ArrayList();
              //   if (!dc2.ContainsKey(item["EmpName"].ToString()))
              //   {
              //      arrlist.Add(item["CaseNo"]);
              //      dc2.Add(item["EmpName"].ToString(), arrlist);
              //   }
              //   else
              //   {
              //      arrlist = dc2[item["EmpName"].ToString()];
              //      arrlist.Add(item["CaseNo"]);
              //      dc2[item["EmpName"].ToString()] = arrlist;
              //   }
              //}
              //if (dc2.Count > 0)
              //{
              //   int x = dc2.Count / 2 + 1;
              //   #region title
              //   SetExcelCell(sheet2, 0, 0, styleBody1, "收文清單");

              //   sheet2.AddMergedRegion(new CellRangeAddress(0, 0, 0, dc2.Count));

              //   SetExcelCell(sheet2, 1, 0, styleBody, "收文日期");
              //   sheet2.AddMergedRegion(new CellRangeAddress(1, 1, 0, 0));
              //   SetExcelCell(sheet2, 1, 1, styleBody, GovDateS + "~" + GovDateE);
              //   sheet2.AddMergedRegion(new CellRangeAddress(1, 1, 1, 1));


              //   if (dc2.Count > 2)
              //   {
              //      if (x == 1) x = x + 1;
              //      SetExcelCell(sheet2, 1, x, styleBody, "部门别/科别");
              //      sheet2.AddMergedRegion(new CellRangeAddress(1, 1, x, x));
              //      SetExcelCell(sheet2, 1, dc2.Count, styleBody, "分行集作二科");
              //      sheet2.AddMergedRegion(new CellRangeAddress(1, 1, dc2.Count, dc2.Count));
              //   }
              //   else
              //   {
              //      if (x == 1) x = x + 1;
              //      SetExcelCell(sheet2, 1, x, styleBody, "部门别/科别");
              //      sheet2.AddMergedRegion(new CellRangeAddress(1, 1, x, x));
              //      SetExcelCell(sheet2, 1, x + 1, styleBody, "分行集作二科");
              //      sheet2.AddMergedRegion(new CellRangeAddress(1, 1, dc2.Count, dc2.Count));
              //   }

              //   #endregion
              //   int a = 0;
              //   int b = 0;
              //   int c = 0;
              //   foreach (KeyValuePair<string, ArrayList> item in dc2)
              //   {
              //      c = item.Value.Count > c ? item.Value.Count : c;
              //   }
              //   b = c - c + 1;
              //   foreach (KeyValuePair<string, ArrayList> item in dc2)
              //   {
              //      sheet2.SetColumnWidth(0, 100 * 30);
              //      sheet2.SetColumnWidth(a + 1, 100 * 30);
              //      for (int i = 0; i < item.Value.Count; i++)
              //      {
              //         SetExcelCell(sheet2, 2, 0, styleHead10, "序號");
              //         SetExcelCell(sheet2, i + 3, 0, styleHead10, Convert.ToString(b + i));
              //         if (item.Value.Count < c)
              //         {
              //            SetExcelCell(sheet2, 2, a + 1, styleHead10, Convert.ToString(item.Key));
              //            for (int j = 0; j < c; j++)
              //            {
              //               if (j < item.Value.Count)
              //               {
              //                  SetExcelCell(sheet2, j + 3, a + 1, styleHead10, Convert.ToString(item.Value[j]));
              //               }
              //               else
              //               {
              //                  SetExcelCell(sheet2, j + 3, a + 1, styleHead10, "");
              //               }

              //            }
              //         }
              //         else
              //         {
              //            SetExcelCell(sheet2, 2, a + 1, styleHead10, Convert.ToString(item.Key));
              //            SetExcelCell(sheet2, i + 3, a + 1, styleHead10, Convert.ToString(item.Value[i]));
              //         }
              //      }
              //      a++;
              //   }
              //}
              //#endregion

              //#region body3
              //Dictionary<string, ArrayList> dc3 = new Dictionary<string, ArrayList>();
              //foreach (DataRow item in dt3.Rows)
              //{
              //   ArrayList arrlist = new ArrayList();
              //   if (!dc3.ContainsKey(item["EmpName"].ToString()))
              //   {
              //      arrlist.Add(item["CaseNo"]);
              //      dc3.Add(item["EmpName"].ToString(), arrlist);
              //   }
              //   else
              //   {
              //      arrlist = dc3[item["EmpName"].ToString()];
              //      arrlist.Add(item["CaseNo"]);
              //      dc3[item["EmpName"].ToString()] = arrlist;
              //   }
              //}
              //if (dc3.Count > 0)
              //{
              //   int x = dc3.Count / 2 + 1;
              //   #region title
              //   SetExcelCell(sheet3, 0, 0, styleBody1, "收文清單");

              //   sheet3.AddMergedRegion(new CellRangeAddress(0, 0, 0, dc3.Count));

              //   SetExcelCell(sheet3, 1, 0, styleBody, "收文日期");
              //   sheet3.AddMergedRegion(new CellRangeAddress(1, 1, 0, 0));
              //   SetExcelCell(sheet3, 1, 1, styleBody, GovDateS + "~" + GovDateE);
              //   sheet3.AddMergedRegion(new CellRangeAddress(1, 1, 1, 1));

              //   if (dc3.Count > 2)
              //   {
              //      if (x == 1) x = x + 1;
              //      SetExcelCell(sheet3, 1, x, styleBody, "部门别/科别");
              //      sheet3.AddMergedRegion(new CellRangeAddress(1, 1, x, x));
              //      SetExcelCell(sheet3, 1, dc3.Count, styleBody, "集作三科");
              //      sheet3.AddMergedRegion(new CellRangeAddress(1, 1, dc3.Count, dc3.Count));
              //   }
              //   else
              //   {
              //      if (x == 1) x = x + 1;
              //      SetExcelCell(sheet3, 1, x, styleBody, "部门别/科别");
              //      sheet3.AddMergedRegion(new CellRangeAddress(1, 1, x, x));
              //      SetExcelCell(sheet3, 1, x + 1, styleBody, "集作三科");
              //      sheet3.AddMergedRegion(new CellRangeAddress(1, 1, dc3.Count, dc3.Count));
              //   }
              //   #endregion

              //   int a = 0;
              //   int b = 0;
              //   int c = 0;
              //   foreach (KeyValuePair<string, ArrayList> item in dc3)
              //   {
              //      c = item.Value.Count > c ? item.Value.Count : c;
              //   }
              //   b = c - c + 1;
              //   foreach (KeyValuePair<string, ArrayList> item in dc3)
              //   {
              //      sheet3.SetColumnWidth(0, 100 * 30);
              //      sheet3.SetColumnWidth(a + 1, 100 * 30);
              //      for (int i = 0; i < item.Value.Count; i++)
              //      {
              //         SetExcelCell(sheet3, 2, 0, styleHead10, "序號");
              //         SetExcelCell(sheet3, i + 3, 0, styleHead10, Convert.ToString(b + i));
              //         if (item.Value.Count < c)
              //         {
              //            SetExcelCell(sheet3, 2, a + 1, styleHead10, Convert.ToString(item.Key));
              //            for (int j = 0; j < c; j++)
              //            {
              //               if (j < item.Value.Count)
              //               {
              //                  SetExcelCell(sheet3, j + 3, a + 1, styleHead10, Convert.ToString(item.Value[j]));
              //               }
              //               else
              //               {
              //                  SetExcelCell(sheet3, j + 3, a + 1, styleHead10, "");
              //               }

              //            }
              //         }
              //         else
              //         {
              //            SetExcelCell(sheet3, 2, a + 1, styleHead10, Convert.ToString(item.Key));
              //            SetExcelCell(sheet3, i + 3, a + 1, styleHead10, Convert.ToString(item.Value[i]));
              //         }
              //      }
              //      a++;
              //   }
              //}
              //#endregion
           }           

           MemoryStream ms = new MemoryStream();
           workbook.Write(ms);
           ms.Flush();
           ms.Position = 0;
           workbook = null;
           return ms;
        }
        //20171103 RC RQ-2015-019666-003 系統功能修正 宏祥 update end

        public ICell SetExcelCell(ISheet sheet, int rowNum, int colNum, ICellStyle style, string value)
        {

            IRow row = sheet.GetRow(rowNum) ?? sheet.CreateRow(rowNum);
            ICell cell = row.GetCell(colNum) ?? row.CreateCell(colNum);
            cell.CellStyle = style;
            cell.SetCellValue(value);
            return cell;
        }

        public DataTable getCaseId(Guid caseId, string depart)
        {
            string strSql = @"SELECT distinct A.CaseId, CaseNo,GovUnit,(SELECT CONVERT(varchar(100), CloseDate, 111)) as 
                                CloseDate,AgentUser,(SELECT CONVERT(varchar(100), GovDate, 111)) as GovDate, 
                              GovNo,Person,CaseKind,CaseKind2,A.Speed,Unit,(SELECT CONVERT(varchar(100), LimitDate, 111)) as LimitDate,
                              Status,(SELECT CONVERT(varchar(100), ApproveDate, 111)) as ApproveDate,l.EmpName 
                              FROM CaseMaster A 
                              left join CaseObligor B on A.CaseId=b.CaseId left join CaseSendSetting S on A.CaseId=S.CaseId 
                              left join LDAPEmployee l on l.EmpID=a.AgentUser
                              where A.CaseId=@CaseId and AgentUser in 
                            (SELECT P.EmpID FROM  [LDAPDepartment] d inner join ldapEmployee p on P.DepDN LIKE '%'+d.depid + '%'
                            where  d.DepName in (@Depart))";
            base.Parameter.Clear();
            base.Parameter.Add(new CommandParameter("@CaseId", caseId.ToString()));
            base.Parameter.Add(new CommandParameter("@Depart", depart));
            return base.Search(strSql) == null ? new DataTable() : base.Search(strSql);
        }

        #region
        //        public List<HistoryQuery> GetDataFromCaseMaster(string Where)
        //        {
        //            try
        //            {
        //                //* 20150522新增財產申報欄位
        //                base.Parameter.Clear();
        //                string strSql = @"SELECT distinct A.CaseId, CaseNo,GovUnit,(SELECT CONVERT(varchar(100), CloseDate, 111)) as CloseDate,AgentUser,
        //                                  (SELECT CONVERT(varchar(100), GovDate, 111)) as GovDate, 
        //                                  GovNo,Person,CaseKind,CaseKind2,A.Speed,Unit,(SELECT CONVERT(varchar(100), LimitDate, 111)) as LimitDate,
        //                                  Status,(SELECT CONVERT(varchar(100), ApproveDate, 111)) as ApproveDate,
        //                                  PropertyDeclaration,OverDueMemo,ReturnReason,CloseReason,
        //                                  CASE WHEN A.CaseKind = '扣押案件' 
        //                                  	THEN (select top 1 CONVERT(NVARCHAR(MAX),memo) from CaseMemo where MemoType='CaseSeizure' and CaseMemo.CaseId=A.CaseId) 
        //                                  	ELSE (select top 1 CONVERT(NVARCHAR(MAX),memo) from CaseMemo where MemoType='CaseExternal' and CaseMemo.CaseId=A.CaseId) 
        //                                  END AS Memo
        //                                  FROM CaseMaster A 
        //                                  left join CaseObligor B on A.CaseId=b.CaseId 
        //                                  left join CaseSendSetting S on A.CaseId=S.CaseId where 0=0 " + Where;
        //                return base.SearchList<HistoryQuery>(strSql).ToList() ?? new List<HistoryQuery>();
        //            }
        //            catch (Exception ex)
        //            {

        //                throw ex;
        //            }
        //        }
        #endregion
        public DataTable GetDataFromCaseMaster(HistoryQuery model)
        {
            string sqlWhere = "";
            string sqlStr = "";
            base.Parameter.Clear();

            if (!string.IsNullOrEmpty(model.CaseNo))
            {
                sqlWhere += @" and CaseNo like @CaseNo ";
                base.Parameter.Add(new CommandParameter("@CaseNo", "%" + model.CaseNo.Trim() + "%"));
            }
            if (!string.IsNullOrEmpty(model.GovUnit))
            {
                sqlWhere += @" and GovUnit like @GovUnit ";
                base.Parameter.Add(new CommandParameter("@GovUnit", "%" + model.GovUnit.Trim() + "%"));
            }
            if (!string.IsNullOrEmpty(model.GovNo))
            {
                sqlWhere += @" and GovNo like @GovNo ";
                base.Parameter.Add(new CommandParameter("@GovNo", "%" + model.GovNo.Trim() + "%"));
            }
            if (!string.IsNullOrEmpty(model.CreateUser))
            {
                sqlWhere += @" and Person like @Person ";
                base.Parameter.Add(new CommandParameter("@Person", "%" + model.CreateUser.Trim() + "%"));
            }
            if (!string.IsNullOrEmpty(model.Speed))
            {
                sqlWhere += @" and A.Speed = @Speed ";
                base.Parameter.Add(new CommandParameter("@Speed", model.Speed.Trim()));
            }
            if (!string.IsNullOrEmpty(model.ReceiveKind))
            {
                sqlWhere += @" and ReceiveKind like @ReceiveKind ";
                base.Parameter.Add(new CommandParameter("@ReceiveKind", "%" + model.ReceiveKind.Trim() + "%"));
            }
            if (!string.IsNullOrEmpty(model.SendKind))
            {
                sqlWhere += @" and S.SendKind like @SendKind ";
                base.Parameter.Add(new CommandParameter("@SendKind", "%" + model.SendKind.Trim() + "%"));
            }
            if (!string.IsNullOrEmpty(model.CaseKind))
            {
                sqlWhere += @" and CaseKind = @CaseKind ";
                base.Parameter.Add(new CommandParameter("@CaseKind", model.CaseKind.Trim()));
            }
            if (!string.IsNullOrEmpty(model.CaseKind2))
            {

                sqlWhere += @" and CaseKind2 = @CaseKind2 ";
                base.Parameter.Add(new CommandParameter("@CaseKind2", model.CaseKind2.Trim()));
            }
            if (!string.IsNullOrEmpty(model.GovDateS))
            {
                sqlWhere += @" AND GovDate >= @GovDateS";
                base.Parameter.Add(new CommandParameter("@GovDateS", model.GovDateS));
            }
            if (!string.IsNullOrEmpty(model.GovDateE))
            {
                sqlWhere += @" AND GovDate <= @GovDateE ";
                base.Parameter.Add(new CommandParameter("@GovDateE", model.GovDateE));
            }
            if (!string.IsNullOrEmpty(model.Unit))
            {
                sqlWhere += @" and Unit like @Unit ";
                base.Parameter.Add(new CommandParameter("@Unit", "%" + model.Unit.Trim() + "%"));
            }
            if (!string.IsNullOrEmpty(model.CreatedDateS))
            {
                sqlWhere += @" AND A.CreatedDate >= @CreatedDateS";
                base.Parameter.Add(new CommandParameter("@CreatedDateS", model.CreatedDateS));
            }
            if (!string.IsNullOrEmpty(model.CreatedDateE))
            {
                sqlWhere += @" AND A.CreatedDate <= @CreatedDateE ";
                base.Parameter.Add(new CommandParameter("@CreatedDateE", model.CreatedDateE));
            }
            if (!string.IsNullOrEmpty(model.OverDateS))
            {
                model.OverDateS = UtlString.FormatDateTwStringToAd(model.OverDateS);
                sqlWhere += @" AND A.CloseDate >= @OverDateS ";
                base.Parameter.Add(new CommandParameter("@OverDateS", model.OverDateS));
            }
            if (!string.IsNullOrEmpty(model.OverDateE))
            {
                model.OverDateE = Convert.ToDateTime(UtlString.FormatDateTwStringToAd(model.OverDateE)).AddDays(1).ToString("yyyy/MM/dd");
                sqlWhere += @" AND A.CloseDate < @OverDateE ";
                base.Parameter.Add(new CommandParameter("@OverDateE", model.OverDateE));
            }
            if (!string.IsNullOrEmpty(model.Status))
            {
                sqlWhere += @" AND Status = @Status ";
                base.Parameter.Add(new CommandParameter("@Status", model.Status));
            }
            if (!string.IsNullOrEmpty(model.AgentUser))
            {
                sqlWhere += @" AND AgentUser like @AgentUser ";
                base.Parameter.Add(new CommandParameter("@AgentUser", "%" + model.AgentUser + "%"));
            }
            if (!string.IsNullOrEmpty(model.ObligorName))
            {
                sqlWhere += @" AND B.ObligorName = @ObligorName ";
                base.Parameter.Add(new CommandParameter("@ObligorName", model.ObligorName));
            }
            if (!string.IsNullOrEmpty(model.ObligorNo))
            {
                sqlWhere += @" AND B.ObligorNo = @ObligorNo ";
                base.Parameter.Add(new CommandParameter("@ObligorNo", model.ObligorNo));
            }

            if (!string.IsNullOrEmpty(model.SendDateS))
            {
                model.SendDateS = UtlString.FormatDateTwStringToAd(model.SendDateS);
                sqlWhere += @" AND S.SendDate >= @SendDateS ";
                base.Parameter.Add(new CommandParameter("@SendDateS", model.SendDateS));
            }
            if (!string.IsNullOrEmpty(model.SendDateE))
            {
                model.SendDateE = UtlString.FormatDateTwStringToAd(model.SendDateE);
                sqlWhere += @" AND S.SendDate <= @SendDateE ";
                base.Parameter.Add(new CommandParameter("@SendDateE", model.SendDateE));
            }
            //AgentUser
            if (!string.IsNullOrEmpty(model.SendNo))
            {
                sqlWhere += @" AND S.SendNo like @SendNo ";
                base.Parameter.Add(new CommandParameter("@SendNo", "%" + model.SendNo.Trim() + "%"));
            }
            //Add by zhangwei 20180315 start
            if (!string.IsNullOrEmpty(model.RMType))
            {
                if (model.RMType == "1")
                {
                    //sqlWhere += @" AND isnull(C.RMNum,'')<>'' and C.RMNum<>'00000' and len(C.customerid )=8";
                    sqlWhere += @" AND ((isnull(C.RMNum,'')<>'' and C.RMNum<>'00000' and len(C.customerid )>=8) or (len(TX.RM_NO + TX.RM_NAME) > 0))";
                }
                else if (model.RMType == "2")
                {
                    sqlWhere += @" AND (isnull(C.RMNum,'')='' or C.RMNum='00000') and len(C.customerid )>=8";
                }
            }
            //Add by zhangwei 20180315 end
            sqlStr = @"with
                        cte1 as
                        (
                        	SELECT * FROM
	                        (
		                        SELECT 
		                        ROW_NUMBER() OVER (PARTITION BY CaseMemo.[CaseId] ORDER BY CaseMemo.[MemoDate] DESC) AS RowID, 
		                        CaseMemo.[CaseId], Convert(nvarchar(max),memo) as memo FROM CaseMaster  
		                        INNER JOIN CaseMemo ON CaseMaster.CaseId = CaseMemo.CaseId
		                        WHERE (CaseKind = '扣押案件' AND CaseMemo.MemoType='CaseSeizure') OR (CaseKind <> '扣押案件' AND CaseMemo.MemoType='CaseExternal')
	                        ) M 
	                        WHERE M.RowID = 1
                        )
                        SELECT distinct 
                        A.CaseId,S.SendKind,CaseNo,CaseKind,CaseKind2,A.Speed,GovUnit,GovNo,CONVERT(varchar(100), GovDate, 111) as GovDate,
                        CONVERT(varchar(100), LimitDate, 111) as LimitDate,AgentUser,CONVERT(varchar(100), CloseDate, 111) as CloseDate,Status,
                        PropertyDeclaration,OverDueMemo,ReturnReason,ReturnAnswer,
                        cte1.memo ,ReceiveKind,CONVERT(varchar(100), A.CreatedDate, 111) AS CreatedDate,Unit,Person,S.SendUpDate
                        FROM CaseMaster A 
                        left join CaseObligor B on A.CaseId=b.CaseId 
                        left join CaseSendSetting S on A.CaseId=S.CaseId 
                        left join TX_60491_Grp C on A.CaseId=C.CaseId
                        left join TX_67002 TX on A.CaseId=TX.CaseId 
                        LEFT JOIN cte1 ON a.caseId = cte1.caseId where 0=0 " + sqlWhere + "  order by CaseNo asc";
            DataTable dt = base.Search(sqlStr);
            dt.Columns.Add("ObligorNo");
            dt.Columns.Add("ObligorName");
            dt.Columns.Add("SendDate");
            dt.Columns.Add("SendNo");
            string strObligorNo = string.Empty;
            string strObligorName = string.Empty;
            string strSendDate = string.Empty;
            string strSendNo = string.Empty;
            if (dt != null && dt.Rows.Count > 0)
            {
                string str = "";
                foreach (DataRow row in dt.Rows)
                {
                    if (row["GovDate"].ToString() == "2001/01/01")
                    {
                        row["GovDate"] = "扣押補建";
                    }
                    if (row["LimitDate"].ToString() == "2001/01/01")
                    {
                        row["LimitDate"] = "扣押補建";
                    }
                    if (row["CreatedDate"].ToString() == "2001/01/01")
                    {
                        row["CreatedDate"] = "扣押補建";
                    }
                    str += "'" + Convert.ToString(row["CaseId"]) + "',";
                }
                str = str.TrimEnd(',');
                sqlStr = @"select CaseId,ObligorNo,ObligorName from History_CaseObligor where CaseId in (" + str + ");";
                IList<CaseObligor> listObli = base.SearchList<CaseObligor>(sqlStr);
                if (listObli != null && listObli.Any())
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        string strNo = "";
                        string strName = "";
                        foreach (CaseObligor obj in listObli.Where(m => m.CaseId.ToString() == Convert.ToString(row["CaseId"])))
                        {
                            strNo += UtlString.GetLastSix(obj.ObligorNo) + ";";
                            strName += UtlString.GetFirstLetter(obj.ObligorName) + ";";
                        }
                        //if (strNo.Length > 0)
                        //    strNo = strNo.Substring(1);
                        //if (strName.Length > 0)
                        //    strName = strName.Substring(1);

                        row["ObligorNo"] = strNo.TrimEnd(';');
                        row["ObligorName"] = strName.TrimEnd(';');
                    }
                }

                sqlStr = @"select CaseId,Convert(Nvarchar(10),SendDate,111) as SendDate,SendNo from History_CaseSendSetting  where CaseId in (" + str + ");";
                IList<CaseSendSetting> listSend = base.SearchList<CaseSendSetting>(sqlStr);
                if (listSend != null && listSend.Any())
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        string strNo = "";
                        string strDate = "";
                        foreach (CaseSendSetting obj in listSend.Where(m => m.CaseId.ToString() == Convert.ToString(row["CaseId"])))
                        {
                            strNo += obj.SendNo + ";";
                            strDate += obj.SendDate.ToString("yyyy/MM/dd") + ";";
                        }

                        //if (strNo.Length > 0)
                        //    strNo = strNo.Substring(1);
                        //if (strDate.Length > 0)
                        //    strDate = strDate.Substring(1);

                        row["SendNo"] = strNo.TrimEnd(';');
                        row["SendDate"] = strDate.TrimEnd(';');
                    }
                }
            }
            return dt;
        }
        #region 20180903 IR-6007 匯出內容與查詢結果一致
        public DataTable GetDataFromCaseMasterForExcel(HistoryQuery model)
        {
            string sqlWhere = "";
            string sqlStr = "";
            base.Parameter.Clear();
            #region CaseNo 20180903 IR-6007
            //if (!string.IsNullOrEmpty(model.CaseNo))
            //{
            //    sqlWhere += @" and CaseNo like @CaseNo ";
            //    base.Parameter.Add(new CommandParameter("@CaseNo", "%" + model.CaseNo.Trim() + "%"));
            //}
            if (!string.IsNullOrEmpty(model.CaseNo))
            {
                sqlWhere += @" and (A.CaseId in (select CaseId from CaseNoChangeHistory where OldCaseNo like @CaseNo) or A.CaseNo like @CaseNo) 
                               and (A.CaseId in (select CaseId from CaseNoChangeHistory where @CaseNo like @CaseNo) or A.CaseNo like @CaseNo) ";
                base.Parameter.Add(new CommandParameter("@CaseNo", "%" + model.CaseNo.Trim() + "%"));
            }
            #endregion
            if (!string.IsNullOrEmpty(model.GovUnit))
            {
                sqlWhere += @" and GovUnit like @GovUnit ";
                base.Parameter.Add(new CommandParameter("@GovUnit", "%" + model.GovUnit.Trim() + "%"));
            }
            if (!string.IsNullOrEmpty(model.GovNo))
            {
                sqlWhere += @" and GovNo like @GovNo ";
                base.Parameter.Add(new CommandParameter("@GovNo", "%" + model.GovNo.Trim() + "%"));
            }
            if (!string.IsNullOrEmpty(model.CreateUser))
            {
                sqlWhere += @" and Person like @Person ";
                base.Parameter.Add(new CommandParameter("@Person", "%" + model.CreateUser.Trim() + "%"));
            }
            if (!string.IsNullOrEmpty(model.Speed))
            {
                sqlWhere += @" and A.Speed = @Speed ";
                base.Parameter.Add(new CommandParameter("@Speed", model.Speed.Trim()));
            }
            if (!string.IsNullOrEmpty(model.ReceiveKind))
            {
                sqlWhere += @" and ReceiveKind like @ReceiveKind ";
                base.Parameter.Add(new CommandParameter("@ReceiveKind", "%" + model.ReceiveKind.Trim() + "%"));
            }
            if (!string.IsNullOrEmpty(model.SendKind))
            {
                sqlWhere += @" and S.SendKind like @SendKind ";
                base.Parameter.Add(new CommandParameter("@SendKind", "%" + model.SendKind.Trim() + "%"));
            }
            if (!string.IsNullOrEmpty(model.CaseKind))
            {
                sqlWhere += @" and CaseKind = @CaseKind ";
                base.Parameter.Add(new CommandParameter("@CaseKind", model.CaseKind.Trim()));
            }
            if (!string.IsNullOrEmpty(model.CaseKind2))
            {

                sqlWhere += @" and CaseKind2 = @CaseKind2 ";
                base.Parameter.Add(new CommandParameter("@CaseKind2", model.CaseKind2.Trim()));
            }
            if (!string.IsNullOrEmpty(model.GovDateS))
            {
                sqlWhere += @" AND GovDate >= @GovDateS";
                base.Parameter.Add(new CommandParameter("@GovDateS", model.GovDateS));
            }
            if (!string.IsNullOrEmpty(model.GovDateE))
            {
                sqlWhere += @" AND GovDate <= @GovDateE ";
                base.Parameter.Add(new CommandParameter("@GovDateE", model.GovDateE));
            }
            if (!string.IsNullOrEmpty(model.Unit))
            {
                sqlWhere += @" and Unit like @Unit ";
                base.Parameter.Add(new CommandParameter("@Unit", "%" + model.Unit.Trim() + "%"));
            }
            if (!string.IsNullOrEmpty(model.CreatedDateS))
            {
                sqlWhere += @" AND A.CreatedDate >= @CreatedDateS";
                base.Parameter.Add(new CommandParameter("@CreatedDateS", model.CreatedDateS));
            }
            if (!string.IsNullOrEmpty(model.CreatedDateE))
            {
                sqlWhere += @" AND A.CreatedDate <= @CreatedDateE ";
                base.Parameter.Add(new CommandParameter("@CreatedDateE", model.CreatedDateE));
            }
            if (!string.IsNullOrEmpty(model.OverDateS))
            {
                model.OverDateS = UtlString.FormatDateTwStringToAd(model.OverDateS);
                sqlWhere += @" AND A.CloseDate >= @OverDateS ";
                base.Parameter.Add(new CommandParameter("@OverDateS", model.OverDateS));
            }
            if (!string.IsNullOrEmpty(model.OverDateE))
            {
                model.OverDateE = Convert.ToDateTime(UtlString.FormatDateTwStringToAd(model.OverDateE)).AddDays(1).ToString("yyyy/MM/dd");
                sqlWhere += @" AND A.CloseDate < @OverDateE ";
                base.Parameter.Add(new CommandParameter("@OverDateE", model.OverDateE));
            }
            if (!string.IsNullOrEmpty(model.Status))
            {
                sqlWhere += @" AND Status = @Status ";
                base.Parameter.Add(new CommandParameter("@Status", model.Status));
            }
            #region AgentUser 20180903 IR-6007
            //if (!string.IsNullOrEmpty(model.AgentUser))
            //{
            //    sqlWhere += @" AND AgentUser like @AgentUser ";
            //    base.Parameter.Add(new CommandParameter("@AgentUser", "%" + model.AgentUser + "%"));
            //}
            string[] aryAgentDepartmentUser;
            string strAgentDepartmentUser = "";
            if (!string.IsNullOrEmpty(model.AgentDepartmentUser))
            {
                aryAgentDepartmentUser = model.AgentDepartmentUser.Split('-');
                strAgentDepartmentUser = aryAgentDepartmentUser.GetValue(0).ToString().Trim();
                sqlWhere += @" AND AgentUser like @AgentUser ";
                base.Parameter.Add(new CommandParameter("@AgentUser", "%" + strAgentDepartmentUser + "%"));
            }
            else if (!string.IsNullOrEmpty(model.AgentDepartment2))
            {
                AgentSettingBIZ agentsettingBiz = new AgentSettingBIZ();
                aryAgentDepartmentUser = agentsettingBiz.GetAgentSetting(model.AgentDepartment2).Split(',');
                if (aryAgentDepartmentUser.Length > 0)
                {
                    for (int i = 0; i < aryAgentDepartmentUser.Length; i++)
                    {
                        strAgentDepartmentUser += "'" + aryAgentDepartmentUser.GetValue(i).ToString().Trim() + "',";
                    }
                    strAgentDepartmentUser = strAgentDepartmentUser.Trim(',');

                    sqlWhere += @" AND AgentUser in (" + strAgentDepartmentUser + ")";
                }
            }
            else if (!string.IsNullOrEmpty(model.AgentDepartment))
            {
                AgentSettingBIZ agentsettingBiz = new AgentSettingBIZ();
                IList<AgentSetting> list = agentsettingBiz.GetAgentDepartmentUserView(model.AgentDepartment);
                foreach (AgentSetting item in list)
                {
                    strAgentDepartmentUser += "'" + item.EmpId.ToString().Trim() + "',";
                }
                strAgentDepartmentUser = strAgentDepartmentUser.Trim(',');

                sqlWhere += @" AND AgentUser in (" + strAgentDepartmentUser + ")";
            }
            #endregion
            if (!string.IsNullOrEmpty(model.ObligorName))
            {
                sqlWhere += @" AND B.ObligorName = @ObligorName ";
                base.Parameter.Add(new CommandParameter("@ObligorName", model.ObligorName));
            }
            if (!string.IsNullOrEmpty(model.ObligorNo))
            {
                sqlWhere += @" AND B.ObligorNo = @ObligorNo ";
                base.Parameter.Add(new CommandParameter("@ObligorNo", model.ObligorNo));
            }

            if (!string.IsNullOrEmpty(model.SendDateS))
            {
                model.SendDateS = UtlString.FormatDateTwStringToAd(model.SendDateS);
                sqlWhere += @" AND S.SendDate >= @SendDateS ";
                base.Parameter.Add(new CommandParameter("@SendDateS", model.SendDateS));
            }
            if (!string.IsNullOrEmpty(model.SendDateE))
            {
                model.SendDateE = UtlString.FormatDateTwStringToAd(model.SendDateE);
                sqlWhere += @" AND S.SendDate <= @SendDateE ";
                base.Parameter.Add(new CommandParameter("@SendDateE", model.SendDateE));
            }
            //AgentUser
            if (!string.IsNullOrEmpty(model.SendNo))
            {
                sqlWhere += @" AND S.SendNo like @SendNo ";
                base.Parameter.Add(new CommandParameter("@SendNo", "%" + model.SendNo.Trim() + "%"));
            }
            //Add by zhangwei 20180315 start
            if (!string.IsNullOrEmpty(model.RMType))
            {
                if (model.RMType == "1")
                {
                    //sqlWhere += @" AND isnull(C.RMNum,'')<>'' and C.RMNum<>'00000' and len(C.customerid )=8";
                    sqlWhere += @" AND ((isnull(C.RMNum,'')<>'' and C.RMNum<>'00000' and len(C.customerid )>=8) or (len(TX.RM_NO + TX.RM_NAME) > 0))";
                }
                else if (model.RMType == "2")
                {
                    sqlWhere += @" AND (isnull(C.RMNum,'')='' or C.RMNum='00000') and len(C.customerid )>=8";
                }
            }
            //Add by zhangwei 20180315 end
            sqlStr = @"with
                        cte1 as
                        (
                        	SELECT * FROM
	                        (
		                        SELECT 
		                        ROW_NUMBER() OVER (PARTITION BY CaseMemo.[CaseId] ORDER BY CaseMemo.[MemoDate] DESC) AS RowID, 
		                        CaseMemo.[CaseId], Convert(nvarchar(max),memo) as memo FROM CaseMaster  
		                        INNER JOIN CaseMemo ON CaseMaster.CaseId = CaseMemo.CaseId
		                        WHERE (CaseKind = '扣押案件' AND CaseMemo.MemoType='CaseSeizure') OR (CaseKind <> '扣押案件' AND CaseMemo.MemoType='CaseExternal')
	                        ) M 
	                        WHERE M.RowID = 1
                        )
                        SELECT distinct 
                        A.CaseId,S.SendKind,CaseNo,CaseKind,CaseKind2,A.Speed,GovUnit,GovNo,CONVERT(varchar(100), GovDate, 111) as GovDate,
                        CONVERT(varchar(100), LimitDate, 111) as LimitDate,AgentUser,CONVERT(varchar(100), CloseDate, 111) as CloseDate,Status,
                        PropertyDeclaration,OverDueMemo,ReturnReason,ReturnAnswer,
                        cte1.memo ,ReceiveKind,CONVERT(varchar(100), A.CreatedDate, 111) AS CreatedDate,Unit,Person,S.SendUpDate
                        FROM CaseMaster A 
                        left join CaseObligor B on A.CaseId=b.CaseId 
                        left join CaseSendSetting S on A.CaseId=S.CaseId 
                        left join TX_60491_Grp C on A.CaseId=C.CaseId
                        left join TX_67002 TX on A.CaseId=TX.CaseId 
                        LEFT JOIN cte1 ON a.caseId = cte1.caseId where 0=0 " + sqlWhere + "  order by CaseNo asc";
            DataTable dt = base.Search(sqlStr);
            dt.Columns.Add("ObligorNo");
            dt.Columns.Add("ObligorName");
            dt.Columns.Add("SendDate");
            dt.Columns.Add("SendNo");
            string strObligorNo = string.Empty;
            string strObligorName = string.Empty;
            string strSendDate = string.Empty;
            string strSendNo = string.Empty;
            if (dt != null && dt.Rows.Count > 0)
            {
                string str = "";
                foreach (DataRow row in dt.Rows)
                {
                    if (row["GovDate"].ToString() == "2001/01/01")
                    {
                        row["GovDate"] = "扣押補建";
                    }
                    if (row["LimitDate"].ToString() == "2001/01/01")
                    {
                        row["LimitDate"] = "扣押補建";
                    }
                    if (row["CreatedDate"].ToString() == "2001/01/01")
                    {
                        row["CreatedDate"] = "扣押補建";
                    }
                    str += "'" + Convert.ToString(row["CaseId"]) + "',";
                }
                str = str.TrimEnd(',');
                sqlStr = @"select CaseId,ObligorNo,ObligorName from History_CaseObligor where CaseId in (" + str + ");";
                IList<CaseObligor> listObli = base.SearchList<CaseObligor>(sqlStr);
                if (listObli != null && listObli.Any())
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        string strNo = "";
                        string strName = "";
                        foreach (CaseObligor obj in listObli.Where(m => m.CaseId.ToString() == Convert.ToString(row["CaseId"])))
                        {
                            strNo += UtlString.GetLastSix(obj.ObligorNo) + ";";
                            strName += UtlString.GetFirstLetter(obj.ObligorName) + ";";
                        }
                        //if (strNo.Length > 0)
                        //    strNo = strNo.Substring(1);
                        //if (strName.Length > 0)
                        //    strName = strName.Substring(1);

                        row["ObligorNo"] = strNo.TrimEnd(';');
                        row["ObligorName"] = strName.TrimEnd(';');
                    }
                }

                sqlStr = @"select CaseId,Convert(Nvarchar(10),SendDate,111) as SendDate,SendNo from History_CaseSendSetting  where CaseId in (" + str + ");";
                IList<CaseSendSetting> listSend = base.SearchList<CaseSendSetting>(sqlStr);
                if (listSend != null && listSend.Any())
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        string strNo = "";
                        string strDate = "";
                        foreach (CaseSendSetting obj in listSend.Where(m => m.CaseId.ToString() == Convert.ToString(row["CaseId"])))
                        {
                            strNo += obj.SendNo + ";";
                            strDate += obj.SendDate.ToString("yyyy/MM/dd") + ";";
                        }

                        //if (strNo.Length > 0)
                        //    strNo = strNo.Substring(1);
                        //if (strDate.Length > 0)
                        //    strDate = strDate.Substring(1);

                        row["SendNo"] = strNo.TrimEnd(';');
                        row["SendDate"] = strDate.TrimEnd(';');
                    }
                }
                if (dt.Rows[0]["CaseKind"].ToString() == "外來文案件")
                {
                    string sqlStr2 = @"select CaseId,Convert(Nvarchar(10),SendDate,111) as SendDate  from GssDoc_Detail  where SendStatus = 1 and CaseId in (" + str + ") ;";
                    IList<GssDoc_Detail> listGssDoc_Detail = base.SearchList<GssDoc_Detail>(sqlStr2);
                    if (listGssDoc_Detail != null && listGssDoc_Detail.Any())
                    {
                        foreach (DataRow row in dt.Rows)
                        {
                            string sDate = "";
                            foreach (GssDoc_Detail obj in listGssDoc_Detail.Where(m => m.CaseId.ToString() == Convert.ToString(row["CaseId"])))
                            {
                                sDate += obj.SendDate.ToString("yyyy/MM/dd") + ";";
                            }
                            sDate= sDate.TrimEnd(';');
                            if (sDate != "" && sDate != null)
                            {
                                row["SendUpDate"] = sDate;
                            }
                        }
                    }
                }

            }
            return dt;
        }
        #endregion

        public IList<HistoryQuery> GetData20Days(HistoryQuery model, int pageNum, string strSortExpression, string strSortDirection, string UserId, ref string where)
        {
            try
            {
                //DateTime nextDate = UtlString.GetDate();
                //int[] aryNextDays = { 3, 2, 1, 7, 6, 5, 4, 3 };
                //DateTime nextDate = DateTime.Today.AddDays(aryNextDays[Convert.ToInt32(DateTime.Today.DayOfWeek.ToString("d"))]);

                where = string.Empty;
                string sqlWhere = "";
                PageIndex = pageNum;
                Parameter.Clear();
                Parameter.Add(new CommandParameter("@pageS", (base.PageSize * (base.PageIndex - 1)) + 1));
                Parameter.Add(new CommandParameter("@pageE", base.PageSize * base.PageIndex));

                where += @" AND A.CaseId IN (select caseid from [History_CaseSeizure] where [SeizureStatus] = 0 OR PayCaseId =CaseId) ";
                sqlWhere += @" AND A.CaseId IN (select caseid from [History_CaseSeizure] where [SeizureStatus] = 0 OR PayCaseId =CaseId) ";

                sqlWhere += @" and CaseKind2  =  @CaseKind2 ";
                //where += @" and CaseKind2  = '" + CaseKind2.CaseSeizureAndPay + "'";
                Parameter.Add(new CommandParameter("@CaseKind2", CaseKind2.CaseSeizureAndPay));

                sqlWhere += @" AND A.PayDate = @NextDate ";
                //where += @" AND DATEADD(day,20,A.CreatedDate) < '" + model.Date + "' ";
                Parameter.Add(new CommandParameter("@NextDate", model.Date));
                //* 扣押並支付中間狀態 or 退件且曾經有這個中間狀態的
                sqlWhere += @" AND ([Status]=@Status1 OR ([Status]=@Status2 AND A.CaseId IN (SELECT CaseId FROM PARMCODE AS A,History_CaseHistory AS B WHERE A.CODETYPE = 'EVENT_NAME' AND A.CODENO = @Status1 AND A.CodeDesc = B.Event))) ";
                //where += @" and ([Status]=@Status1) ";

                Parameter.Add(new CommandParameter("Status1", CaseStatus.DirectorApproveSeizureAndPay));
                Parameter.Add(new CommandParameter("Status2", CaseStatus.DirectorReturn));


                string strSql = @";with T1 
	                        as
	                        (
                                SELECT distinct A.CaseId, CaseNo,GovUnit,(SELECT CONVERT(varchar(100), CloseDate, 111)) as CloseDate,AgentUser,(SELECT CONVERT(varchar(100), GovDate, 111)) as GovDate, 
                                GovNo,Person,CaseKind,CaseKind2,A.Speed,Unit,(SELECT CONVERT(varchar(100), LimitDate, 111)) as LimitDate,
                                Status,(SELECT CONVERT(varchar(100), ApproveDate, 111)) as ApproveDate 
                                FROM History_CaseMaster A 
                                left join History_CaseObligor B on A.CaseId=b.CaseId left join History_CaseSendSetting S on A.CaseId=S.CaseId where 0=0 " + sqlWhere + @" 
	                        ),T2 as
	                        (
		                        select *, row_number() over (order by " + strSortExpression + " " + strSortDirection + @" ) RowNum
		                        from T1
	                        ),T3 as 
	                        (
		                        select *,(select max(RowNum) from T2) maxnum from T2 
		                        where rownum between @pageS and @pageE
	                        )
	                        select a.* from T3 a order by a.RowNum";

                IList<HistoryQuery> ilst = base.SearchList<HistoryQuery>(strSql);
                if (ilst != null)
                {
                    if (ilst.Count > 0)
                    {
                        var ilist = GetCodeData("STATUS_NAME");
                        foreach (HistoryQuery item in ilst)
                        {
                            var obj = ilist.FirstOrDefault(a => a.CodeNo == item.Status);
                            if (obj != null)
                                item.StatusShow = obj.CodeDesc;
                            else
                                item.StatusShow = item.Status;
                        }
                        base.DataRecords = ilst[0].maxnum;
                    }
                    else
                    {
                        base.DataRecords = 0;
                        ilst = new List<HistoryQuery>();
                    }
                    return ilst;
                }
                else
                {
                    return new List<HistoryQuery>();
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        //20170630 緊急 RQ-2015-019666-018 派件至跨單位 宏祥 add start
        public int checkAgentDepartment(string DepartmentID, string EmpID)
        {
           try
           {
              string strSql = @" select count(0) from LDAPEmployee where DepDN like @DepartmentID and EmpID=@EmpID";
              base.Parameter.Clear();
              base.Parameter.Add(new CommandParameter("@DepartmentID", "%" + DepartmentID + "%"));
              base.Parameter.Add(new CommandParameter("@EmpID", EmpID));
              return (int)base.ExecuteScalar(strSql);
           }
           catch (Exception ex)
           {
              throw ex;
           }
        }
       //20170630 緊急 RQ-2015-019666-018 派件至跨單位 宏祥 add end

    }
}
