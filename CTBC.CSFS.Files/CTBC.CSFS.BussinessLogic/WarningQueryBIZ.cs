using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using CTBC.CSFS.Models;
using CTBC.FrameWork.Platform;
using CTBC.CSFS.Pattern;
using NPOI.SS.UserModel;
using NPOI.HSSF.Util;
using NPOI.SS.Util;
using System.IO;
using NPOI.HSSF.UserModel;
using CTBC.FrameWork.Util;

namespace CTBC.CSFS.BussinessLogic
{
    public class WarningQueryBIZ : CommonBIZ
    {
        public WarningQueryBIZ(AppController appController)
            : base(appController)
        { }

        public WarningQueryBIZ()
        { }

        public IList<WarningQuery> GetQueryList(WarningQuery wq)
        {
            try
            {
                string sqlStr = "";
                string sqlStrWhere = "";
                base.Parameter.Clear();
                if (!string.IsNullOrEmpty(wq.CustId))
                {
                    sqlStrWhere += @" AND CustId like @CustId";
                    base.Parameter.Add(new CommandParameter("@CustId", "%" + wq.CustId.Trim() + "%"));
                }
                if (!string.IsNullOrEmpty(wq.CustAccount))
                {
                    sqlStrWhere += @" AND CustAccount like @CustAccount";
                    base.Parameter.Add(new CommandParameter("@CustAccount", "%" + wq.CustAccount.Trim() + "%"));
                }
                if (!string.IsNullOrEmpty(wq.DocNo))
                {
                    sqlStrWhere += @" AND wd.DocNo like @DocNo";
                    base.Parameter.Add(new CommandParameter("@DocNo", "%" + wq.DocNo.Trim() + "%"));
                }
                if (!string.IsNullOrEmpty(wq.VictimName))
                {
                    sqlStrWhere += @" AND VictimName like @VictimName";
                    base.Parameter.Add(new CommandParameter("@VictimName", "%" + wq.VictimName.Trim() + "%"));
                }
                if (!string.IsNullOrEmpty(wq.ForCDate))
                {
                    sqlStrWhere += @" AND ForCDate like @ForCDate";
                    base.Parameter.Add(new CommandParameter("@ForCDate", "%" + wq.ForCDate.Trim() + "%"));
                }
                if (!string.IsNullOrEmpty(wq.ForCDateS))
                {
                    sqlStrWhere += @" AND ForCDate >= @ForCDateS";
                    base.Parameter.Add(new CommandParameter("@ForCDateS", wq.ForCDateS));
                }
                if (!string.IsNullOrEmpty(wq.ForCDateE))
                {
                    string ForCDateE = UtlString.FormatDateString(Convert.ToDateTime(wq.ForCDateE.Replace('/', ' ').ToString()).AddDays(1).ToString("yyyyMMdd"));
                    sqlStrWhere += @" AND ForCDate < @ForCDateE ";
                    base.Parameter.Add(new CommandParameter("@ForCDateE", ForCDateE));
                }
                //Add by zhangwei 20180315 start
                if (!string.IsNullOrEmpty(wq.StateType) )
                {
                    string StateType = wq.StateType;
                    sqlStrWhere += @" AND StateType = @StateType ";
                    base.Parameter.Add(new CommandParameter("@StateType", StateType));
                }
                if (!string.IsNullOrEmpty(wq.RelieveDateS))
                {
                    sqlStrWhere += @" AND RelieveDate >= @RelieveDateS";
                    base.Parameter.Add(new CommandParameter("@RelieveDateS", wq.RelieveDateS));
                }
                if (!string.IsNullOrEmpty(wq.RelieveDateE))
                {
                    sqlStrWhere += @" AND RelieveDate <= @RelieveDateE";
                    base.Parameter.Add(new CommandParameter("@RelieveDateE", wq.RelieveDateE));
                }
                if (!string.IsNullOrEmpty(wq.Original))
                {
                    sqlStrWhere += @" AND Original = @Original";
                    base.Parameter.Add(new CommandParameter("@Original", wq.Original));
                }
                if (!string.IsNullOrEmpty(wq.ModifyDateS))
                {
                    sqlStrWhere += @" AND wd.ModifiedDate >= @ModifyDateS";
                    base.Parameter.Add(new CommandParameter("@ModifyDateS", wq.ModifyDateS));
                }
                if (!string.IsNullOrEmpty(wq.ModifyDateE))
                {
                    sqlStrWhere += @" AND wd.ModifiedDate >= @ModifyDateE";
                    base.Parameter.Add(new CommandParameter("@ModifyDateE", wq.ModifyDateE));
                }
                //sqlStr += @"SELECT 
                //            ROW_NUMBER() OVER(ORDER BY wm.DocNo asc) as SerialID,
                //            wm.DocNo,CustId,CustAccount,BankID,AccountStatus,
                //            Convert(Nvarchar(10),ForCDate,111 ) as ForCDate,
                //            NotificationUnit,CustName,ExtPhone,NotificationName,PoliceStation,
                //            ISNULL(ws.NoClosed,0) AS NoClosed
                //            FROM WarningMaster wm
                //            left OUTER JOIN WarningDetails wd ON wm.DocNo=wd.DocNo
                //            LEFT OUTER JOIN (SELECT [DocNo],COUNT(1) AS NoClosed FROM [WarningState] WHERE [RelieveDate] IS NULL GROUP BY [DocNo] ) AS ws ON wm.DocNo = ws.DocNo
                //            WHERE 1=1  " + sqlStrWhere + @"
                //            ORDER BY DocNo ASC,ForCDate ASC";
                sqlStr += @"SELECT ROW_NUMBER() OVER(ORDER BY wm.DocNo asc) as SerialID, wm.DocNo,CustId,CustAccount,BankID,AccountStatus,
    Convert(Nvarchar(10),ForCDate,111 ) as ForCDate, NotificationUnit,CustName,ExtPhone,NotificationName,PoliceStation,
    ISNULL(ws.NoClosed,0) AS NoClosed,
    case StateType 
    when  '1' then '新增' 
    when '4' then '修改'
    when '5' then '撤銷'
    else ''
    end  as StateType,
    Original,CONVERT(varchar(10),RelieveDate, 111) as RelieveDate,
    case StateType
    when '1'  then CONVERT(varchar(10),EtabsDatetime, 111)
    when '3'  then CONVERT(varchar(10),EtabsDatetime, 111)
    else null
    end as TX9091,
    case StateType
    when '5'  then CONVERT(varchar(10),RelieveDate, 111)
    else null
    end as TX9092,
	CONVERT(varchar(10),wd.ModifiedDate, 111) as ModifiedDate,
 CurBal,NotifyBal,ReleaseBal,
    case AccountStatus
	when '1' then '未結清'
	when '2' then '結清'
	else '其他'
	end as AccountStatusName
    FROM WarningMaster wm
    left OUTER JOIN WarningDetails wd ON wm.DocNo=wd.DocNo left join [WarningState] wsta on wd.DocNo=wsta.DocNo
    LEFT OUTER JOIN (SELECT [DocNo],COUNT(1) AS NoClosed FROM [WarningState] WHERE [RelieveDate] IS NULL GROUP BY [DocNo] ) AS ws ON wm.DocNo = ws.DocNo
    WHERE 1=1  " + sqlStrWhere + @"
    ORDER BY DocNo ASC,ForCDate ASC";
                //Add by zhangwei 20180315 end
                IList<WarningQuery> _ilsit = base.SearchList<WarningQuery>(sqlStr);

                return _ilsit;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public DataTable ExcelList(WarningQuery wq)
        {
            string sqlStrWhere = "";
            if (!string.IsNullOrEmpty(wq.CustId))
            {
                sqlStrWhere += @" AND CustId like @CustId";
                base.Parameter.Add(new CommandParameter("@CustId", "%" + wq.CustId.Trim() + "%"));
            }
            if (!string.IsNullOrEmpty(wq.CustAccount))
            {
                sqlStrWhere += @" AND CustAccount like @CustAccount";
                base.Parameter.Add(new CommandParameter("@CustAccount", "%" + wq.CustAccount.Trim() + "%"));
            }
            if (!string.IsNullOrEmpty(wq.DocNo))
            {
                sqlStrWhere += @" AND wd.DocNo like @DocNo";
                base.Parameter.Add(new CommandParameter("@DocNo", "%" + wq.DocNo.Trim() + "%"));
            }
            if (!string.IsNullOrEmpty(wq.VictimName))
            {
                sqlStrWhere += @" AND VictimName like @VictimName";
                base.Parameter.Add(new CommandParameter("@VictimName", "%" + wq.VictimName.Trim() + "%"));
            }
            if (!string.IsNullOrEmpty(wq.ForCDate))
            {
                sqlStrWhere += @" AND ForCDate like @ForCDate";
                base.Parameter.Add(new CommandParameter("@ForCDate", "%" + wq.ForCDate.Trim() + "%"));
            }
            if (!string.IsNullOrEmpty(wq.ForCDateS))
            {
                sqlStrWhere += @" AND ForCDate >= @ForCDateS";
                base.Parameter.Add(new CommandParameter("@ForCDateS", wq.ForCDateS));
            }
            if (!string.IsNullOrEmpty(wq.ForCDateE))
            {
                string ForCDateE = UtlString.FormatDateString(Convert.ToDateTime(wq.ForCDateE.Replace('/', ' ').ToString()).AddDays(1).ToString("yyyyMMdd"));
                sqlStrWhere += @" AND ForCDate < @ForCDateE ";
                base.Parameter.Add(new CommandParameter("@ForCDateE", ForCDateE));
            }
            string strSql = @"select wm.DocNo,CustId,CustAccount,BankID,BankName,NotificationContent,NotificationSource,
                              Convert(Nvarchar(10),ForCDate,111) as ForCDate,CustName,PoliceStation,VictimName,wm.AccountStatus,ISNULL(ws.NoClosed,0) AS NoClosed
                              from WarningMaster 				  
							  wm
                            left OUTER JOIN WarningDetails wd ON wm.DocNo=wd.DocNo
                            LEFT OUTER JOIN (SELECT [DocNo],COUNT(1) AS NoClosed FROM [WarningState] WHERE [RelieveDate] IS NULL GROUP BY [DocNo] ) AS ws ON wm.DocNo = ws.DocNo
							  where 1=1 " + sqlStrWhere;
            strSql += " ORDER BY wm.DocNo";
            DataTable dt = base.Search(strSql);
            if (dt != null && dt.Rows.Count > 0) return dt;
            else return new DataTable();
        }

        //public MemoryStream ParmCodeExcel_NPOI(WarningQuery wq)
        //{
        //    IWorkbook workbook = new HSSFWorkbook();
        //    ISheet sheet = workbook.CreateSheet("警示案件查詢");

        //    #region 獲取數據源
        //    DataTable dt = ExcelList(wq);
        //    #endregion

        //    #region def style
        //    ICellStyle styleHead12 = workbook.CreateCellStyle();
        //    IFont font12 = workbook.CreateFont();
        //    font12.FontHeightInPoints = 12;
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
        //    styleHead10.Alignment = HorizontalAlignment.Center;
        //    styleHead10.VerticalAlignment = VerticalAlignment.Center;
        //    styleHead10.SetFont(font10);
        //    #endregion

        //    #region width
        //    sheet.SetColumnWidth(0, 100 * 30);
        //    sheet.SetColumnWidth(1, 100 * 30);
        //    sheet.SetColumnWidth(2, 100 * 30);
        //    sheet.SetColumnWidth(3, 100 * 30);
        //    sheet.SetColumnWidth(4, 100 * 30);
        //    sheet.SetColumnWidth(5, 100 * 30);
        //    sheet.SetColumnWidth(6, 100 * 30);
        //    sheet.SetColumnWidth(7, 100 * 50);
        //    sheet.SetColumnWidth(8, 100 * 30);
        //    sheet.SetColumnWidth(9, 100 * 100);
        //    sheet.SetColumnWidth(10, 100 * 50);
        //    sheet.SetColumnWidth(11, 100 * 50);
        //    #endregion

        //    #region title
        //    //*大標題 line0
        //    SetExcelCell(sheet, 0, 0, styleHead12, "警示案件查詢");
        //    sheet.AddMergedRegion(new CellRangeAddress(0, 0, 0, 11));

        //    //*查詢條件 line1
        //    SetExcelCell(sheet, 1, 0, styleHead10, "案件編號");
        //    sheet.AddMergedRegion(new CellRangeAddress(1, 1, 0, 0));
        //    SetExcelCell(sheet, 1, 1, styleHead10, "被通報ID");
        //    sheet.AddMergedRegion(new CellRangeAddress(1, 1, 1, 1));
        //    SetExcelCell(sheet, 1, 2, styleHead10, "被通報帳號");
        //    sheet.AddMergedRegion(new CellRangeAddress(1, 1, 2, 2));
        //    SetExcelCell(sheet, 1, 3, styleHead10, "分行代號");
        //    sheet.AddMergedRegion(new CellRangeAddress(1, 1, 3, 3));
        //    SetExcelCell(sheet, 1, 4, styleHead10, "分行名稱");
        //    sheet.AddMergedRegion(new CellRangeAddress(1, 1, 4, 4));
        //    SetExcelCell(sheet, 1, 5, styleHead10, "通報內容");
        //    sheet.AddMergedRegion(new CellRangeAddress(1, 1, 5, 5));
        //    SetExcelCell(sheet, 1, 6, styleHead10, "通報內容");
        //    sheet.AddMergedRegion(new CellRangeAddress(1, 1, 6, 6));
        //    SetExcelCell(sheet, 1, 7, styleHead10, "通報聯徵日期");
        //    sheet.AddMergedRegion(new CellRangeAddress(1, 1, 7, 7));
        //    SetExcelCell(sheet, 1, 8, styleHead10, "被通報姓名");
        //    sheet.AddMergedRegion(new CellRangeAddress(1, 1, 8, 8));
        //    SetExcelCell(sheet, 1, 9, styleHead10, "警局/被害人");
        //    sheet.AddMergedRegion(new CellRangeAddress(1, 1, 9, 9));
        //    SetExcelCell(sheet, 1, 10, styleHead10, "帳戶狀態");
        //    sheet.AddMergedRegion(new CellRangeAddress(1, 1, 10, 10));
        //    SetExcelCell(sheet, 1, 11, styleHead10, "結案");
        //    sheet.AddMergedRegion(new CellRangeAddress(1, 1, 11, 11));
        //    #endregion

        //    #region body
        //    for (int iRow = 0; iRow < dt.Rows.Count; iRow++)
        //    {
        //        for (int iCol = 0; iCol < dt.Columns.Count - 2; iCol++)
        //        {
        //                 SetExcelCell(sheet, iRow + 2, iCol, styleHead10, Convert.ToString(dt.Rows[iRow][iCol])); 
    
        //        }
        //        string polition = string.Empty;
        //        if (dt.Rows[iRow]["PoliceStation"].ToString() != "" && dt.Rows[iRow]["VictimName"].ToString() != "")
        //        {
        //            polition = Convert.ToString(dt.Rows[iRow]["PoliceStation"]) + ";被害人:" + Convert.ToString(dt.Rows[iRow]["VictimName"]);
        //        }
        //        if (dt.Rows[iRow]["PoliceStation"].ToString() == "" && dt.Rows[iRow]["VictimName"].ToString() != "")
        //        {
        //            polition = "被害人:" + Convert.ToString(dt.Rows[iRow]["VictimName"]);
        //        }
        //        if (dt.Rows[iRow]["PoliceStation"].ToString() != "" && dt.Rows[iRow]["VictimName"].ToString() == "")
        //        {
        //            polition = Convert.ToString(dt.Rows[iRow]["PoliceStation"]) + ";";
        //        }

        //        SetExcelCell(sheet, iRow + 2, 9, styleHead10, polition);
        //        string strAccountStatus = "";
        //        if (dt.Rows[iRow]["AccountStatus"].ToString() == "1")
        //        {
        //            strAccountStatus = "未結清";
        //        }
        //        if (dt.Rows[iRow]["AccountStatus"].ToString() == "2")
        //        {
        //            strAccountStatus = "結清";
        //        }
        //        if (dt.Rows[iRow]["AccountStatus"].ToString() != "2" && dt.Rows[iRow]["AccountStatus"].ToString() != "1")
        //        {
        //            strAccountStatus = "其他";
        //        }
        //        SetExcelCell(sheet, iRow + 2, 10, styleHead10, strAccountStatus);
        //        string strNoClosed = "";
        //        if (Convert.ToInt16(dt.Rows[iRow]["NoClosed"].ToString()) > 0)
        //        {
        //            strNoClosed = "未結案";
        //        }
        //        else
        //        {
        //            strNoClosed = "已結案";
        //        }
        //        SetExcelCell(sheet, iRow + 2, 11, styleHead10, strNoClosed);
        //    }
        //    #endregion

        //    MemoryStream ms = new MemoryStream();
        //    workbook.Write(ms);
        //    ms.Flush();
        //    ms.Position = 0;
        //    workbook = null;
        //    return ms;
        //}

        public ICell SetExcelCell(ISheet sheet, int rowNum, int colNum, ICellStyle style, string value)
        {

            IRow row = sheet.GetRow(rowNum) ?? sheet.CreateRow(rowNum);
            ICell cell = row.GetCell(colNum) ?? row.CreateCell(colNum);
            cell.CellStyle = style;
            cell.SetCellValue(value);
            return cell;
        }
        public MemoryStream ParmCodeExcel_NPOI(WarningQuery model)
        {
            var ms = new MemoryStream();
            string[] headerColumns = new[]
                    {
                        "類別",
                        "正本",
                        "設定/修改日期 9091",
                        "解除日期9092",
                        "案件編號",
                        "ID",
                        "帳號",
                        "分行別",
                        "通報聯徵日期",
                        "單位",
                        "人員/分機",
                        "客戶姓名",
                        "通報警局(單位)",
                        "帳戶狀態",
                        "目前餘額",
                        "通報餘額",
                        "解除餘額"
                    };
            IList<WarningQuery> ilst = GetQueryList(model);

            if (ilst != null)
            {
                ms = ExcelExport(ilst, headerColumns,
                                                   delegate (HSSFRow dataRow, WarningQuery dataItem)
                                                   {
                                                       //* 這裡可以針對每一個欄位做額外處理.比如日期
                                                       dataRow.CreateCell(0).SetCellValue(dataItem.StateType);
                                                       dataRow.CreateCell(1).SetCellValue(dataItem.Original);
                                                       dataRow.CreateCell(2).SetCellValue(UtlString.FormatDateTw(dataItem.TX9091));
                                                       dataRow.CreateCell(3).SetCellValue(UtlString.FormatDateTw(dataItem.TX9092));
                                                       dataRow.CreateCell(4).SetCellValue(dataItem.DocNo);
                                                       dataRow.CreateCell(5).SetCellValue(dataItem.CustId);
                                                       dataRow.CreateCell(6).SetCellValue(dataItem.CustAccount);
                                                       dataRow.CreateCell(7).SetCellValue(dataItem.BankID);
                                                       dataRow.CreateCell(8).SetCellValue(UtlString.FormatDateTw(dataItem.ForCDate));
                                                       dataRow.CreateCell(9).SetCellValue(dataItem.NotificationUnit);
                                                       dataRow.CreateCell(10).SetCellValue(dataItem.NotificationName + "/" + dataItem.ExtPhone);
                                                       dataRow.CreateCell(11).SetCellValue(dataItem.CustName);
                                                       dataRow.CreateCell(12).SetCellValue(dataItem.PoliceStation);
                                                       dataRow.CreateCell(13).SetCellValue(dataItem.AccountStatus);
                                                       dataRow.CreateCell(14).SetCellValue(dataItem.CurBal);
                                                       dataRow.CreateCell(15).SetCellValue(dataItem.NotifyBal);
                                                       dataRow.CreateCell(16).SetCellValue(dataItem.ReleaseBal);
                                                   });
            }
            return ms;
        }
    }
}
