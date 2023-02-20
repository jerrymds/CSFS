using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using CTBC.CSFS.Models;
using CTBC.CSFS.ViewModels;
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
    public class WarningAccountQueryBIZ : CommonBIZ
    {
        public WarningAccountQueryBIZ(AppController appController)
            : base(appController)
        { }

        public WarningAccountQueryBIZ()
        { }
        public IList<WarningAccountQuery> GetQueryAList(WarningAccountQuery wq)
        {
            try
            {
                string sqlStr = "";
                string sqlStrWhere = "";
                base.Parameter.Clear();
                //if (!string.IsNullOrEmpty(wq.CustId))
                //{
                //    sqlStrWhere += @" AND CustId like @CustId";
                //    base.Parameter.Add(new CommandParameter("@CustId", "%" + wq.CustId.Trim() + "%"));
                //}
                //if (!string.IsNullOrEmpty(wq.CustAccount))
                //{
                //    sqlStrWhere += @" AND CustAccount like @CustAccount";
                //    base.Parameter.Add(new CommandParameter("@CustAccount", "%" + wq.CustAccount.Trim() + "%"));
                //}
                //if (!string.IsNullOrEmpty(wq.DocNo))
                //{
                //    sqlStrWhere += @" AND wd.DocNo like @DocNo";
                //    base.Parameter.Add(new CommandParameter("@DocNo", "%" + wq.DocNo.Trim() + "%"));
                //}
                //if (!string.IsNullOrEmpty(wq.VictimName))
                //{
                //    sqlStrWhere += @" AND VictimName like @VictimName";
                //    base.Parameter.Add(new CommandParameter("@VictimName", "%" + wq.VictimName.Trim() + "%"));
                //}
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
                //Add by adam 20190325 start
                if (!string.IsNullOrEmpty(wq.AccountStatus))
                {
                    string AccountStatus = wq.AccountStatus;
                    sqlStrWhere += @" AND AccountStatus = @AccountStatus ";
                    base.Parameter.Add(new CommandParameter("@StateType", AccountStatus));
                }
                //Add by adam 20190325 start
                if (!string.IsNullOrEmpty(wq.AccountStatus))
                {
                    string AccountStatus = wq.AccountStatus;
                    sqlStrWhere += @" AND AccountStatus = @AccountStatus ";
                    base.Parameter.Add(new CommandParameter("@StateType", AccountStatus));
                }
                if (!string.IsNullOrEmpty(wq.HangAmount))
                {
                    if (wq.HangAmount == "1")
                    {
                        sqlStrWhere += @" AND HangAmount < @HangAmount";
                        base.Parameter.Add(new CommandParameter("@HangAmount", 1000));
                    }
                    if (wq.HangAmount == "2")
                    {
                        sqlStrWhere += @" AND HangAmount <= @HangAmount AND Original >= 1000";
                        base.Parameter.Add(new CommandParameter("@HangAmount", 50000));
                    }
                    if (wq.HangAmount == "3")
                    {
                        sqlStrWhere += @" AND HangAmount = @HangAmount";
                        base.Parameter.Add(new CommandParameter("@HangAmount", 50000));
                    }

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
                //adam20190218
                sqlStr += @"SELECT ROW_NUMBER() OVER(ORDER BY wm.DocNo asc) as SerialID, wm.DocNo,wm.currency,CustId,CustAccount,BankID,AccountStatus,
    Convert(Nvarchar(10),ForCDate,111 ) as ForCDate, NotificationUnit,CustName,ExtPhone,wd.NotificationSource,NotificationName,PoliceStation,
    	ISNULL(ws.NoClosed,0)
	 AS NoClosed,HappenDateTime,No_165,DocAddress,
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
case ISNULL(ws.NoClosed,0)
	when '1' then '結案'
	when '2' then '未結案'
	else '未結案'
	end AS NoClosedName,
    case AccountStatus
	when '1' then '未結清'
	when '2' then '結清'
	else '其他'
	end as AccountStatusName
    FROM WarningMaster wm
    left OUTER JOIN WarningDetails wd ON wm.DocNo=wd.DocNo left join [WarningState] wsta on wd.DocNo=wsta.DocNo and wd.StateCode =wsta.StateCode
    LEFT OUTER JOIN (SELECT [DocNo],COUNT(1) AS NoClosed FROM [WarningState] WHERE [RelieveDate] IS NULL GROUP BY [DocNo] ) AS ws ON wm.DocNo = ws.DocNo
    WHERE 1=1  " + sqlStrWhere + @"
    ORDER BY DocNo ASC,ForCDate ASC";
                //Add by zhangwei 20180315 end
                IList<WarningAccountQuery> _ilsit = base.SearchList<WarningAccountQuery>(sqlStr);

                return _ilsit;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public IList<WarningAccountQuery> GetQueryList(WarningAccountQuery wq, int pageIndex)
        {
            try
            {
                base.Parameter.Clear();
                base.PageIndex = pageIndex;
                base.Parameter.Clear();
                if (pageIndex == 999999)
                {
                    base.Parameter.Add(new CommandParameter("@pageS", 1));
                    base.Parameter.Add(new CommandParameter("@pageE", 999999));
                }
                else
                {
                    base.Parameter.Add(new CommandParameter("@pageS", (base.PageSize * (base.PageIndex - 1)) + 1));
                    base.Parameter.Add(new CommandParameter("@pageE", base.PageSize * base.PageIndex));
                }
                string sqlStr = "";
                string sqlStrWhere = "";
                if (!string.IsNullOrEmpty(wq.HangAmount))
                {
                    if (wq.HangAmount == "1")
                    {
                        sqlStrWhere += @" AND Convert(money,BALANCE) < @HangAmount";
                        base.Parameter.Add(new CommandParameter("@HangAmount", 1000));
                    }
                    if (wq.HangAmount == "2")
                    {
                        sqlStrWhere += @" AND Convert(money,BALANCE) <= @HangAmount AND Convert(money,BALANCE) >= 1000";
                        base.Parameter.Add(new CommandParameter("@HangAmount", 50000));
                    }
                    if (wq.HangAmount == "3")
                    {
                        sqlStrWhere += @" AND Convert(money,BALANCE) >= @HangAmount";
                        base.Parameter.Add(new CommandParameter("@HangAmount", 50000));
                    }

                }
                if (!string.IsNullOrEmpty(wq.ItemType))
                {
                    if (wq.ItemType == "1")
                    {
                        sqlStrWhere += @" AND Convert(money,Amount) = 0 ";
                    }
                    if (wq.ItemType == "2")
                    {
                        sqlStrWhere += @" AND Convert(money,Amount) > 0 ";
                    }
                    if (wq.ItemType == "3")
                    {
                        sqlStrWhere += @" AND Convert(money,Amount) < 0 ";
                    }
                }
                if (!string.IsNullOrEmpty(wq.AccountStatus))
                {
                    //sqlStrWhere += @" AND ws.NoClosed = @AccountStatus";
                    sqlStrWhere += @" AND AccountStatus = @AccountStatus";
                    base.Parameter.Add(new CommandParameter("@AccountStatus",  wq.AccountStatus.Trim()  ));
                }
                if (!string.IsNullOrEmpty(wq.NotificationSource))
                {
                    if (wq.NotificationSource == "1")
                    {
                        sqlStrWhere += @" AND wd.NotificationSource = '非電話詐財' ";
                    }
                    if (wq.NotificationSource == "2")
                    {
                        sqlStrWhere += @" AND wd.NotificationSource = '電話詐財' ";
                    }
                    if (wq.NotificationSource == "3")
                    {
                        sqlStrWhere += @" AND wd.NotificationSource = '刑事' ";
                    }
                }

                if (!string.IsNullOrEmpty(wq.ForCDateS))
                {
                    string ForCDateS = Convert.ToDateTime(wq.ForCDateS).AddDays(0).ToString("yyyyMMdd");
                    sqlStrWhere += @" AND TRAN_DATE >= @ForCDateS";
                    base.Parameter.Add(new CommandParameter("@ForCDateS", ForCDateS));
                }
                if (!string.IsNullOrEmpty(wq.ForCDateE))
                {
                    string ForCDateE = Convert.ToDateTime(wq.ForCDateE).AddDays(0).ToString("yyyyMMdd");
                    sqlStrWhere += @" AND TRAN_DATE <= @ForCDateE ";
                    base.Parameter.Add(new CommandParameter("@ForCDateE", ForCDateE));
                }

                //adam20190430 CHQ_PAYEE,ACCT_NO,PoliceStation,ACT_DATE_TIME
                sqlStr += @" ;with T1 
	                        as
	                        (
SELECT  ROW_NUMBER() OVER(ORDER BY id asc) as SerialID, wm.DocNo,wm.currency,CustId,CustAccount,BankID,AccountStatus,
    CustName,wd.NotificationSource,
    	ISNULL(ws.NoClosed,0) AS NoClosed,
 
    CONVERT(varchar(10),RelieveDate, 111) as RelieveDate,  

 CurBal,NotifyBal,ReleaseBal,
(case when Convert(money,BALANCE) < 1000  then '一仟以下'
	when (Convert(money,BALANCE) >= 1000 and Convert(money,BALANCE) < 50000) then '伍萬以下至一仟(含)'
	when ( Convert(money,BALANCE)>= 50000 ) then '伍萬以上(含)'
	else ''
	end ) AS HangAmountList,Balance450,
case ISNULL(ws.NoClosed,0)
	when '1' then '結案'
	when '2' then '未結案'
	else '未結案'
	end AS NoClosedName,
    case AccountStatus
	when '1' then '未結清'
	when '2' then '結清'
	else '其他'
	end as AccountStatusName,wg.id,wg.ACCT_NO,wg.TRAN_DATE,wg.ACT_DATE,wg.HangAmount,wg.Amount,wg.BALANCE,wg.POST_DATE
    FROM WarningMaster wm
   left OUTER JOIN (select  DISTINCT DocNo,NotificationContent,NotificationSource,StateCode from WarningDetails)as wd ON wm.DocNo=wd.DocNo 
    left join [WarningState] wsta on wd.DocNo=wsta.DocNo and wd.StateCode =wsta.StateCode
    LEFT OUTER JOIN (SELECT [DocNo],COUNT(1) AS NoClosed FROM [WarningState] WHERE [RelieveDate] IS NULL GROUP BY [DocNo] ) AS ws ON wm.DocNo = ws.DocNo
inner join WarningGenAcct wg
  on wm.DocNo like '%'+wg.CHQ_PAYEE+'%'
    WHERE 1=1  " + sqlStrWhere + @"
),T2 as
	                        (
		                        select *, row_number() over (ORDER BY id ASC" + @" ) RowNum
		                        from T1
	                        ),T3 as 
	                        (
		                        select *,(select max(RowNum) from T2) maxnum from T2 
		                        where rownum between @pageS and @pageE
	                        )
	                        select a.* from T3 a order by a.RowNum";

                //Add by adam 20190503 end
                IList<WarningAccountQuery> _ilsit = base.SearchList<WarningAccountQuery>(sqlStr);
                if ((_ilsit != null) && (_ilsit.Count > 0))
                {
                    foreach (var it in _ilsit)
                    {
                        decimal _trueamt = GetDecimal(it.HangAmount.ToString());
                        decimal _amt = GetDecimal(it.Amount.ToString());
                        decimal _balanceamt = GetDecimal(it.Balance.ToString());
                        decimal _balance450 = GetDecimal(it.Balance450.ToString());
                        if ((_amt > 0) || (_trueamt > 0))
                        {
                            if (_amt > 0)
                            {
                                it.HangAmount = _amt.ToString("###,###,###,###.##");
                            }
                            if (_trueamt > 0)
                            {
                                it.HangAmount = _trueamt.ToString("###,###,###,###.##");
                            }
                            it.Amount = "0";
                        }
                        else
                        {
                            it.HangAmount = "0";
                            it.Amount = _trueamt.ToString("###,###,###,###.##");
                        }
                        if ( _amt < 0 )
                        {
                            it.Amount = _amt.ToString("###,###,###,###.##");
                        } 
                        if (_balanceamt > 0)
                        {
                            it.Balance = _balanceamt.ToString("###,###,###,###.##");
                        }
                        if (_balance450 > 0)
                        {
                            it.Balance450 = _balance450.ToString("###,###,###,###.##");
                        }
                    }
                    base.DataRecords = _ilsit[0].maxnum;
                    return _ilsit;
                }
                else
                {
                    base.DataRecords = 0;
                    return _ilsit;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public IList<WarningAccountQuery> GetQueryList1(WarningAccountQuery wq, int pageIndex)
        {
            try
            {
                base.Parameter.Clear();
                base.PageIndex = pageIndex;
                //string sqlStr = "";
                //string sqlWhere = "";
                base.Parameter.Add(new CommandParameter("@pageS", (base.PageSize * (base.PageIndex - 1)) + 1));
                base.Parameter.Add(new CommandParameter("@pageE", base.PageSize * base.PageIndex));
                string sqlStr = "";
                string sqlStrWhere = "";
 
                if (!string.IsNullOrEmpty(wq.ForCDateE))
                {
                    string ForCDateE = UtlString.FormatDateString(Convert.ToDateTime(wq.ForCDateE).AddDays(0).ToString("yyyy/MM/dd"));
                    sqlStrWhere += @" AND  TXDate  = @ForCDateE ";
                    base.Parameter.Add(new CommandParameter("@ForCDateE", ForCDateE));
                }

                sqlStr += @";with T1 
	                        as
	                        (select  *    FROM WarningAcctBalance WHERE 1=1  "
                   + sqlStrWhere + @"
),T2 as
	                        (
		                        select *, row_number() over (ORDER BY TXDate,Account ASC" + @" ) RowNum
		                        from T1
	                        ),T3 as 
	                        (
		                        select *,(select max(RowNum) from T2) maxnum from T2 
		                        where rownum between @pageS and @pageE
	                        )
	                        select a.* from T3 a order by a.RowNum";

                //+ @" order by TXDate,Account ";
                IList<WarningAccountQuery> _ilsit = base.SearchList<WarningAccountQuery>(sqlStr);
                if ((_ilsit != null) && (_ilsit.Count > 0))
                {
                    foreach (var it in _ilsit)
                    {
                        decimal _balanceamt = GetDecimal(it.Balance.ToString());
                        if (_balanceamt > 0)
                        {
                            it.Balance = _balanceamt.ToString("###,###,###,###.##");
                        }
                    }
                    base.DataRecords = _ilsit[0].maxnum;
                    return _ilsit;
                }
                else
                {
                    base.DataRecords = 0;
                    return _ilsit;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public static Decimal GetDecimal(string strn)
        {
            if (strn.Length == 0)
                return 0;
            Decimal result = 0;

            if (strn.LastIndexOf("+") != -1 || strn.LastIndexOf("-") != -1)
            {
                // string sign = strn.Substring(strn.Length - 1, 1);
                string sign = strn.Substring(0, 1);
                if (sign == "+")
                {
                    result = Convert.ToDecimal(strn.Substring(0, strn.Length - 1));
                }
                else
                {
                    result = Convert.ToDecimal(strn.Substring(0, strn.Length - 1));
                }

            }
            else
            {
                result = Convert.ToDecimal(strn);
            }


            return result;
        }
        public DataTable ExcelList(WarningAccountQuery wq)
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
	                        inner join WarningGenAcct wg  on wm.DocNo like '%'+wg.CHQ_PAYEE+'%'
                            LEFT OUTER JOIN (SELECT [DocNo],COUNT(1) AS NoClosed FROM [WarningState] WHERE [RelieveDate] IS NULL GROUP BY [DocNo] ) AS ws ON wm.DocNo = ws.DocNo
							  where 1=1 " + sqlStrWhere;
            strSql += " ORDER BY wm.DocNo";
            DataTable dt = base.Search(strSql);
            if (dt != null && dt.Rows.Count > 0) return dt;
            else return new DataTable();
        }

        //public MemoryStream ParmCodeExcel_NPOI(WarningAccountQuery wq)
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

        public MemoryStream ImportXLSX_NPOI(string pathSource)
        {
            //string pathSource = @"C:\Users\mvmurthy\Downloads\VOExportTemplate.xlsx";
            DataTable dt = new DataTable();
            decimal ct = 0;
            decimal type50000 = 0;
            decimal bigThan50000 = 0;
            string amts1 = "";
            string amts2 = "";
            decimal amt1 = 0;
            decimal amt2 = 0;
            int iBig = 0;
            int iCt = 0;
            string typ1 = "五萬以上(含)";
            string typ2 = "五萬以上(含)";
            FileStream fs = new FileStream(pathSource, FileMode.Open, FileAccess.ReadWrite);
            HSSFWorkbook templateWorkbook = new HSSFWorkbook(fs, true);
            HSSFSheet sheet = (HSSFSheet)templateWorkbook.GetSheet("總明細");


            //dataRow.GetCell(1).SetCellValue("foo");

            MemoryStream ms = new MemoryStream();
            //templateWorkbook.Write(ms);

            //HSSFWorkbook workbook = null;
            //HSSFSheet sheet = null;

            try
            {
                #region 讀Excel檔，逐行寫入DataTable
                //workbook = new HSSFWorkbook(fs); //只能讀取 System.IO.Stream 
                //                                 //FileContent 屬性會取得指向要上載之檔案的 Stream 物件。這個屬性可以用於存取檔案的內容 (做為位元組)。 
                //                                 //   例如，您可以使用 FileContent 屬性傳回的 Stream 物件，將檔案的內容做為位元組進行讀取並將其以位元組陣列儲存。 
                //                                 //FileContent 屬性，型別：System.IO.Stream 

                //sheet = (HSSFSheet)workbook.GetSheetAt(0);   //0表示：第一個 worksheet工作表


                HSSFRow headerRow = (HSSFRow)sheet.GetRow(0);   //Excel 表頭列
                //for (int colIdx = 0; colIdx <= headerRow.LastCellNum; colIdx++) //表頭列，共有幾個 "欄位"?（取得最後一欄的數字） 
                for (int colIdx = 0; colIdx <= 7; colIdx++) //表頭列，共有幾個 "欄位"?（取得最後一欄的數字） 
                {
                    if (headerRow.GetCell(colIdx) != null)
                        dt.Columns.Add(new DataColumn(headerRow.GetCell(colIdx).StringCellValue));
                    //欄位名有折行時，只取第一行的名稱做法是headerRow.GetCell(colIdx).StringCellValue.Replace("\n", ",").Split(',')[0]
                }

                //For迴圈的「啟始值」為1，表示不包含 Excel表頭列
                for (int rowIdx = 1; rowIdx <= sheet.LastRowNum; rowIdx++)   //每一列做迴圈
                {
                    try
                    {
                        HSSFRow exlRow = (HSSFRow)sheet.GetRow(rowIdx); //不包含 Excel表頭列的 "其他資料列"
                        DataRow newDataRow = dt.NewRow();

                        for (int colIdx = exlRow.FirstCellNum; colIdx <= 7; colIdx++)   //exlRow.LastCellNum 每一個欄位做迴圈
                        {
                            if (exlRow.GetCell(colIdx) != null)
                            {
                                newDataRow[colIdx] = exlRow.GetCell(colIdx).ToString();
                            }
                            else
                            {
                                newDataRow[colIdx] = null;
                            }

                            //每一個欄位，都加入同一列 DataRow
                        }
                        typ1 = newDataRow[1].ToString().Trim();
                        amts1 = newDataRow[5].ToString().Trim();
                        if (amts1 != null && amts1.Trim().Length > 0)
                        {
                            amt1 = Convert.ToDecimal(amts1);
                        }
                        else
                        {
                            amt1 = 0;
                        }
                        if (typ2 == typ1)  // typ1 是實際金額,typ2 是固定5萬以上
                        {
                            bigThan50000 = bigThan50000 + amt1;
                            iBig = iBig + 1;
                        }
                        else
                        {
                            type50000 = type50000 + amt1;
                            iCt = iCt + 1;
                        }
                        dt.Rows.Add(newDataRow);
                    }
                    catch (Exception err)
                    {
                        iCt = iCt + 1;
                        throw err;
                    }

                }
                //update to excel total page
                HSSFSheet sheet0 = (HSSFSheet)templateWorkbook.GetSheet("總表");
                HSSFRow dataRow0 = (HSSFRow)sheet0.GetRow(13);//第5列
                dataRow0.GetCell(3).SetCellValue(iBig);//第14列  >= 500000 件數
                dataRow0.GetCell(4).SetCellValue(bigThan50000.ToString());//  >= 500000 金額
                dataRow0.GetCell(5).SetCellValue(iCt);//   < 500000 件數
                dataRow0.GetCell(6).SetCellValue(type50000.ToString());//  < 500000 金額
                                                                       //另存為Result.xls  
                var NewFile = pathSource.Replace(".xls", "_new.xls");
                using (var file = new FileStream(NewFile, FileMode.Open, FileAccess.ReadWrite))
                {
                    templateWorkbook.Write(file);
                }
                templateWorkbook.Write(ms);
                //using (FileStream fsOut = new FileStream(NewFile, FileMode.CreateNew))
                //{
                //    templateWorkbook.Write(fsOut);
                //    fsOut.Close();
                //}
                //GridView1.DataSource = dt;
                //GridView1.DataBind();
                #endregion 讀Excel檔，逐行寫入DataTable
            }
            catch (Exception err)
            {
                throw err;
            }
            finally
            {
                //釋放 NPOI的資源
                templateWorkbook = null;
                sheet = null;
            }
            
            return ms;           
        }


        public ICell SetExcelCell(ISheet sheet, int rowNum, int colNum, ICellStyle style, string value)
        {

            IRow row = sheet.GetRow(rowNum) ?? sheet.CreateRow(rowNum);
            ICell cell = row.GetCell(colNum) ?? row.CreateCell(colNum);
            cell.CellStyle = style;
            cell.SetCellValue(value);
            return cell;
        }
        public MemoryStream ParmCodeExcel_NPOI(WarningAccountQuery model)
        {
            //            < th > @Lang.csfs_seqnum </ th >
            //< th > @Lang.csfs_case_no </ th >
            //< th > @Lang.csfs_bankID </ th >
            //< th > @Lang.csfs_warningnum </ th >
            //< th > @Lang.csfs_warn_id </ th >
            //< th > 金額區間 </ th >
            //< th > 通報時餘額 </ th >
            //< th > VD / MD </ th >
            //< th > 掛帳日期 </ th >
            //< th > 掛帳金額 </ th >
            //< th > 還款日期 </ th >
            //< th > 還款金額 </ th >
            //< th > 目前餘額 </ th >
            //< th > 通報內容 </ th >
            //< th > 掛帳G / L </ th >
            //< th > 通報聯徵日期 </ th >
            //< th > 警局 / 被害人 </ th >
            //< th > 狀態 </ th >
            //< th > 最後付息日 </ th >
            //< th > 解除日期 </ th >
            //< th > 新增日期 </ th >
            //"類別",
            //"正本",
            //"設定/修改日期 9091",
            //"解除日期9092",
            //"案件編號",
            //"ID",
            //"帳號",
            //"分行別",
            //"通報聯徵日期",
            //"通報內容",
            //"單位",
            //"客戶姓名",
            //"通報警局(單位)",
            //"帳戶狀態",
            //"結案",
            //"幣別",
            //"通報餘額",
            //"解除餘額",
            //"案發時間",
            //"案發地點",
            //"165案號"
            int pageNum = 1;
            var ms = new MemoryStream();
            string[] headerColumns = new[]
                    {
                                "序號",
                                "案件編號",
                                "分行別",
                                "被通報帳號",
                                "被通報ID",
                                "金額區間",
                                "通報時餘額",
                                "VD / MD",
                                "掛帳日期",
                                "掛帳金額",
                                "還款日期",
                                "還款金額",
                                "目前餘額",
                                "G/L 餘額",
                                "通報內容",
                                "掛帳G / L",
                                "狀態",
                                "最後付息日",
                                "解除日期"
                    };
            string[] headerColumns1 = new[]
                    {
                                "帳號G/L",
                                "戶名",
                                "餘額"
                    };
            string[] headerColumns2 = new[]
                    {
                                "       掛帳合計     ",
                                "       還款合計     ",
                                "  掛帳合計-還款合計 "
                    };
            Decimal iHangAmount = 0;
            Decimal iAmount = 0;
            Decimal iBalance = 0;
            //
            Decimal iHangAmount712 = 0;
            Decimal iAmount712 = 0;
            Decimal iBalance712 = 0;
            //
            Decimal iHangAmount738 = 0;
            Decimal iAmount738 = 0;
            Decimal iBalance738 = 0;

            if (model.Other == "1")
            {
                pageNum = 999999;
                IList<WarningAccountQuery> ilst = GetQueryList(model, pageNum);

                if (ilst != null)
                {
                    foreach (var il in ilst)
                    {
                        if (il.NoClosed > 0)
                            il.NoClosedName = "未結案";
                        else
                            il.NoClosedName = "已結案";
                        if (il.HangAmount != null && il.HangAmount != "")
                        {
                            iHangAmount = iHangAmount + Convert.ToDecimal(il.HangAmount);
                            iBalance =  iBalance + Convert.ToDecimal(il.HangAmount);
                        }
                        // 712 ,738
                        if (il.HangAmount != null && il.HangAmount != "" && il.ACCT_NO == "0000880010090712")
                        {
                            iHangAmount712 = iHangAmount712 + Convert.ToDecimal(il.HangAmount);
                            iBalance712 = iBalance712 + Convert.ToDecimal(il.HangAmount);
                        }
                        if (il.HangAmount != null && il.HangAmount != "" && il.ACCT_NO == "0000880010090738")
                        {
                            iHangAmount738 = iHangAmount738 + Convert.ToDecimal(il.HangAmount);
                            iBalance738 = iBalance738 + Convert.ToDecimal(il.HangAmount);
                        }

                        if (il.Amount != null && il.Amount != "")
                        {
                            iAmount = iAmount + Convert.ToDecimal(il.Amount);
                            iBalance = iBalance + Convert.ToDecimal(il.Amount);
                        }
                        // 712,738
                        if (il.Amount != null && il.Amount != "" && il.ACCT_NO == "0000880010090712")
                        {
                            iAmount712 = iAmount712 + Convert.ToDecimal(il.Amount);
                            iBalance712 = iBalance712 + Convert.ToDecimal(il.Amount);
                        }
                        if (il.Amount != null && il.Amount != "" && il.ACCT_NO == "0000880010090738")
                        {
                            iAmount738 = iAmount738 + Convert.ToDecimal(il.Amount);
                            iBalance738 = iBalance738 + Convert.ToDecimal(il.Amount);
                        }

                    }
                    WarningAccountQuery mTotal = new WarningAccountQuery();
                    mTotal.ACCOUNT = "9999999999999990";
                    mTotal.HangAmount = iHangAmount.ToString("###,###,###,##0.##");
                    mTotal.Amount = iAmount.ToString("###,###,###,##0.##");
                    mTotal.Balance = iBalance.ToString("###,###,###,##0.##");
                    ilst.Add(mTotal);
                    WarningAccountQuery mTotal1 = new WarningAccountQuery();
                    //712
                    mTotal1.ACCOUNT = "9999999999999991";
                    mTotal1.HangAmount = iHangAmount712.ToString("###,###,###,##0.##");
                    mTotal1.Amount = iAmount712.ToString("###,###,###,##0.##");
                    mTotal1.Balance = iBalance712.ToString("###,###,###,##0.##");
                    ilst.Add(mTotal1);
                    //738
                    WarningAccountQuery mTotal2 = new WarningAccountQuery();
                    mTotal2.ACCOUNT = "9999999999999992";
                    mTotal2.HangAmount = iHangAmount738.ToString("###,###,###,##0.##");
                    mTotal2.Amount = iAmount738.ToString("###,###,###,##0.##");
                    mTotal2.Balance = iBalance738.ToString("###,###,###,##0.##");
                    ilst.Add(mTotal2);
                    ms = ExcelExport(ilst, headerColumns,
                    delegate (HSSFRow dataRow, WarningAccountQuery dataItem)
                    {
                        if( (dataItem.ACCOUNT != "9999999999999990") && (dataItem.ACCOUNT != "9999999999999991") && (dataItem.ACCOUNT != "9999999999999992"))
                        {
                            //* 這裡可以針對每一個欄位做額外處理.比如日期 (UtlString.FormatDateTw(dataItem.ForCDate));
                            dataRow.CreateCell(0).SetCellValue(dataItem.SerialID);// id 
                            dataRow.CreateCell(1).SetCellValue(dataItem.DocNo);
                            dataRow.CreateCell(2).SetCellValue(dataItem.BankID);
                            dataRow.CreateCell(3).SetCellValue(dataItem.CustAccount);
                            dataRow.CreateCell(4).SetCellValue(dataItem.CustId);
                            dataRow.CreateCell(5).SetCellValue(dataItem.HangAmountlist);
                            dataRow.CreateCell(6).SetCellValue(dataItem.NotifyBal);
                            dataRow.CreateCell(7).SetCellValue(dataItem.VDMD);
                            dataRow.CreateCell(8).SetCellValue(UtlString.FormatDateTw(dataItem.TRAN_Date));
                            dataRow.CreateCell(9).SetCellValue(dataItem.HangAmount);
                            dataRow.CreateCell(10).SetCellValue(UtlString.FormatDateTw(dataItem.TRAN_Date));
                            dataRow.CreateCell(11).SetCellValue(dataItem.Amount);
                            dataRow.CreateCell(12).SetCellValue(dataItem.Balance);
                            dataRow.CreateCell(13).SetCellValue(dataItem.Balance450);
                            dataRow.CreateCell(14).SetCellValue(dataItem.NotificationSource);
                            dataRow.CreateCell(15).SetCellValue(dataItem.ACCT_NO);
                            dataRow.CreateCell(16).SetCellValue(dataItem.NoClosedName);
                            dataRow.CreateCell(17).SetCellValue(dataItem.ACCT_DATE);
                            dataRow.CreateCell(18).SetCellValue(UtlString.FormatDateTw(dataItem.RelieveDate));
                            //dataRow.CreateCell(19).SetCellValue(UtlString.FormatDateTw(dataItem.ModifiedDate));
                        }
                        else
                        {
                            dataRow.CreateCell(0).SetCellValue("");
                            dataRow.CreateCell(1).SetCellValue("");
                            dataRow.CreateCell(2).SetCellValue("");
                            dataRow.CreateCell(3).SetCellValue("");
                            dataRow.CreateCell(4).SetCellValue("");
                            dataRow.CreateCell(5).SetCellValue("");
                            dataRow.CreateCell(6).SetCellValue("");
                            dataRow.CreateCell(7).SetCellValue("");
                            if (dataItem.ACCOUNT == "9999999999999990")
                            {
                                dataRow.CreateCell(8).SetCellValue("合計:");
                            }
                            if (dataItem.ACCOUNT == "9999999999999991")
                            {
                                dataRow.CreateCell(8).SetCellValue("712:");
                            }
                            if (dataItem.ACCOUNT == "9999999999999992")
                            {
                                dataRow.CreateCell(8).SetCellValue("738:");
                            }
                            dataRow.CreateCell(9).SetCellValue(dataItem.HangAmount);
                            dataRow.CreateCell(10).SetCellValue("");
                            dataRow.CreateCell(11).SetCellValue(dataItem.Amount);
                            dataRow.CreateCell(12).SetCellValue(dataItem.Balance);
                            dataRow.CreateCell(13).SetCellValue("");
                            dataRow.CreateCell(14).SetCellValue("");
                            dataRow.CreateCell(15).SetCellValue("");
                            dataRow.CreateCell(16).SetCellValue("");
                            dataRow.CreateCell(17).SetCellValue("");
                            dataRow.CreateCell(18).SetCellValue("");
                            //dataRow.CreateCell(19).SetCellValue("");

                        }
                    });
          
                }
                //ms = ExcelExport(ilst, headerColumns2,
                //   delegate (HSSFRow dataRow, WarningAccountQuery dataItem)
                //   {
                //        dataRow.CreateCell(0).SetCellValue(iHangAmount.ToString("###,###,###,###.##"));
                //        dataRow.CreateCell(1).SetCellValue(iAmount.ToString("###,###,###,###.##"));
                //        dataRow.CreateCell(2).SetCellValue(iBalance.ToString("###,###,###,###.##"));
                //   });
            }
            else
            {
                IList<WarningAccountQuery> ilst = GetQueryList1(model,pageNum);
                if (ilst != null)
                {
                    foreach (var il in ilst)
                    {
                        if (il.NoClosed > 0)
                            il.NoClosedName = "未結案";
                        else
                            il.NoClosedName = "已結案";
                    }
                    ms = ExcelExport(ilst, headerColumns1,
                    delegate (HSSFRow dataRow, WarningAccountQuery dataItem)
                    {
                        //* 這裡可以針對每一個欄位做額外處理.比如日期 (UtlString.FormatDateTw(dataItem.ForCDate));
                        dataRow.CreateCell(0).SetCellValue(dataItem.ACCOUNT);
                        if(dataItem.ACCOUNT == "0000880010090712")
                        {
                            dataRow.CreateCell(1).SetCellValue("其他應付警示帳戶剩餘款電話詐財");
                        }
                         else
                        {
                            dataRow.CreateCell(1).SetCellValue("其他應付警示帳戶剩餘款非電話詐財");
                        }
                        dataRow.CreateCell(2).SetCellValue(dataItem.Balance);  
                    });
                }
            }
            return ms;
        }
    }
}
