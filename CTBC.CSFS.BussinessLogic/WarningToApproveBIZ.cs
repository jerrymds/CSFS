using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using CTBC.CSFS.Models;
using CTBC.FrameWork.Platform;
using CTBC.CSFS.Pattern;
using CTBC.CSFS.ViewModels;
using NPOI.SS.UserModel;
using NPOI.HSSF.Util;
using NPOI.SS.Util;
using System.IO;
using NPOI.HSSF.UserModel;
using CTBC.FrameWork.Util;

namespace CTBC.CSFS.BussinessLogic
{
    public class WarningToApproveBIZ : CommonBIZ
    {
        private static List<Guid> staticAryCaseId = new List<Guid>();
        public WarningToApproveBIZ(AppController appController)
            : base(appController)
        { }

        public WarningToApproveBIZ()
        { }

        public IList<WarningToApprove> GetQueryList(WarningToApprove wq)
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
                //if (!string.IsNullOrEmpty(wq.Kind))
                //{
                //    sqlStrWhere += @" AND Kind like @Kind";
                //    base.Parameter.Add(new CommandParameter("@Kind", "%" + wq.Kind.Trim() + "%"));
                //}
                if (!string.IsNullOrEmpty(wq.Kind))
                {
                    switch (wq.Kind)
                    {
                        case "通報聯徵":
                            sqlStrWhere += @" AND Kind like @Kind";
                            base.Parameter.Add(new CommandParameter("@Kind", "%" + wq.Kind.Trim() + "%"));
                            break;
                        case "解除":
                            sqlStrWhere += @" AND ( Release = '1' or RelieveDate is Not Null) ";
                            //base.Parameter.Add(new CommandParameter("@Kind", "%" + wq.Kind.Trim() + "%"));
                            break;
                        case "修改通報":
                            sqlStrWhere += @" AND Kind like @Kind";
                            base.Parameter.Add(new CommandParameter("@Kind", "%" + wq.Kind.Trim() + "%"));
                            break;
                        case "ID變更":
                            sqlStrWhere += @" AND (wm.CustId_Old is Not Null or wm.ForeignId_Old is Not Null) ";
                            break;
                        case "延長":
                            sqlStrWhere += @" AND ( Extend = '1' ) ";
                            break;
                        default: /* 可选的 */
                            break;
                    }
                }
                if (!string.IsNullOrEmpty(wq.No_165))
                {
                    sqlStrWhere += @" AND No_165 like @No_165";
                    base.Parameter.Add(new CommandParameter("@No_165", "%" + wq.No_165.Trim() + "%"));
                }
                if (!string.IsNullOrEmpty(wq.Set))
                {
                    if (wq.Set == "Y")
                    {
                        sqlStrWhere += @" AND ( [Set] = '1' or [Set] = 'Y')  ";
                    }
                    else
                    {
                        sqlStrWhere += @" AND ( [Set] <> '1' or [Set] is Null ) ";
                    }
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
                if (!string.IsNullOrEmpty(wq.StateType))
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
                sqlStr += @";with T1 
	                        as
	                        (
		                       select ROW_NUMBER() OVER (PARTITION BY  docNo ORDER BY  docno ASC) as SerialNo, SerialID,  DocNo from WarningDetails   where  CreatedDate > '2022-01-01'
	                        ),T2 as
	                        (
SELECT ROW_NUMBER() OVER(ORDER BY wm.DocNo asc) as SerialNo1,wd.SerialID, wm.DocNo,wm.currency,CustId,CustAccount,BankID,AccountStatus,Retry,
    Convert(Nvarchar(10),ForCDate,111 ) as ForCDate, NotificationUnit,CustName,ExtPhone,wd.NotificationSource,NotificationName,PoliceStation,wd.NewId,wm.CaseId,
    	ISNULL(ws.NoClosed,0)
	 AS NoClosed,HappenDateTime,wd.No_165,DocAddress,[Flag_909113]
      ,[Release]
      ,[ReleaseDate]
      ,[Extend]
      ,[Kind]
      ,[ExtendNo]
      ,[FIX]
      ,[FIXSEND]
      ,[Set]
      ,[SetDate],
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
    WHERE  wd.Status = 'D01'  " + sqlStrWhere + @") select T1.SerialNo,t2.* from t2 inner join T1 
	on t2.SerialID = t1.SerialID
	ORDER BY t2.DocNo ASC,ForCDate ASC  ";
                //Add by zhangwei 20180315 end
                IList<WarningToApprove> _ilsit = base.SearchList<WarningToApprove>(sqlStr);

                return _ilsit;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public JsonReturn DirectorReturn(List<String> aryStatus, List<Guid> aryCaseId, string userId)
        {
            if (aryCaseId == null || !aryCaseId.Any()) return new JsonReturn() { ReturnCode = "0" };

            //并發驗證
            foreach (var item in aryCaseId)
            {

                if (staticAryCaseId != null && staticAryCaseId.Contains(item))
                {
                    return new JsonReturn { ReturnCode = "0", ReturnMsg = "(" + item + ")" + "此案件正在處理中，請重新查詢！" };
                }
                else { staticAryCaseId.Add(item); }
            }

            bool bRtn = true;
            IDbConnection dbConnection = OpenConnection();
            IDbTransaction dbTransaction = null;
            WarningToApproveBIZ wpBiz = new WarningToApproveBIZ();
            int i = 0;
            try
            {
                using (dbConnection)
                {
                    dbTransaction = dbConnection.BeginTransaction();
                    foreach (Guid caseId in aryCaseId)
                    {
                        string NewId = caseId.ToString();
                        bRtn = bRtn && wpBiz.UpdateCaseStatus(caseId, "C01", userId, dbTransaction) > 0;
                        i++;
                    }

                    if (bRtn)
                    {
                        dbTransaction.Commit();
                        return new JsonReturn() { ReturnCode = "1" };
                    }
                    else
                    {
                        dbTransaction.Rollback();
                        return new JsonReturn() { ReturnCode = "0" };
                    }
                }
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
                throw;
            }
            finally
            {
                staticAryCaseId.Clear();
            }
        }

        //public JsonReturn DirectorReturn(WarningToApprove model, List<String> aryCaseId,string aryStatus, string userId)
        //{
        //    if (aryCaseId == null || !aryCaseId.Any()) return new JsonReturn() { ReturnCode = "0" };
        //    bool bRtn = true;

        //    //WarningToApproveBIZ wpBiz = new WarningToApproveBIZ();
        //    //CaseHistoryBIZ history = new CaseHistoryBIZ();

        //    IDbConnection dbConnection = OpenConnection();
        //    IDbTransaction dbTransaction = null;
        //    int num = 0;
        //    try
        //    {
        //        using (dbConnection)
        //        {
        //            dbTransaction = dbConnection.BeginTransaction();
        //            foreach (String caseId in aryCaseId)
        //            {

        //                    string sqlStr = "";
        //                    sqlStr = sqlStr + @" UPDATE [WarningDetails] SET 
        //                                        [Status] = @Status, 
        //                                        [ModifiedUser] = @ModifiedUser,
        //                                        [ModifiedDate] = GETDATE()
        //                                WHERE [NewId] = @NewId";

        //                    Parameter.Clear();
        //                    Parameter.Add(new CommandParameter("NewId", caseId));
        //                    Parameter.Add(new CommandParameter("Status", "C01"));
        //                    Parameter.Add(new CommandParameter("ModifiedUser", userId));
        //                    bRtn = bRtn && ExecuteNonQuery(sqlStr, dbTransaction) > 0;


        //                    if (sqlStr != "")
        //                    {
        //                        int i = ExecuteNonQuery(sqlStr);
        //                    }

        //                    num++;
        //                }
        //                //else
        //                //{
        //                //    return new JsonReturn { ReturnCode = "0", ReturnMsg = "(" + masterobj.CaseNo + ")" + "此案件狀態已變更，請重新查詢！" };
        //                //}
        //            }
        //            if (bRtn)
        //            {
        //                dbTransaction.Commit();
        //                return new JsonReturn() { ReturnCode = "1" };
        //            }
        //            else
        //            {
        //                dbTransaction.Rollback();
        //                return new JsonReturn() { ReturnCode = "0" };
        //            }

        //    }
        //    catch (Exception)
        //    {
        //        try
        //        {
        //            if (dbTransaction != null) dbTransaction.Rollback();
        //        }
        //        catch (Exception)
        //        {
        //            // ignored
        //        }
        //        throw;
        //    }
        //}


        public WarningDetails GetWarningDetailsByNewId(Guid caseid)
        {
            string strSql = @"select * from  [WarningDetails]  WHERE [CaseId] = @CaseId";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("@CaseId", caseid));
            IList<WarningDetails> list = SearchList<WarningDetails>(strSql);
            if (list != null)
            {
                return list.Count > 0 ? list[0] : new WarningDetails();
            }
            return new WarningDetails();
        }
        public int UpdateCaseStatus(Guid caseid, string newStatus, string userId, IDbTransaction trans = null)
        {
            DateTime dtNow = GetNowDateTime();
            //CaseHistoryBIZ history = new CaseHistoryBIZ();
            string strSql = @"UPDATE [WarningDetails] SET [Status] = @Status,
                [ModifiedUser] =@UpdateUserId, [ModifiedDate]=@UpdateDate WHERE [NewId] = @NewId";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("@NewId", caseid));
            Parameter.Add(new CommandParameter("@Status", newStatus));
            Parameter.Add(new CommandParameter("@UpdateUserId", userId));
            Parameter.Add(new CommandParameter("@UpdateDate", dtNow));
            if (trans != null)
            {
                // 執行新增返回是否成功
                if (ExecuteNonQuery(strSql, trans) > 0)
                {                    
                    return 1;
                }
            }
            return 0;
        }

        public WarningDetails WarningDetailsModel(string caseId)
        {
            string strSql = @"Select * from WarningDetails 	where NewId=@NewId) ";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("@NewId", caseId));
            IList<WarningDetails> list = SearchList<WarningDetails>(strSql) ;
            if (list != null)
            {
                return list.Count > 0 ? list[0] : new WarningDetails();
            }
            return new WarningDetails();
        }

        public DataTable ExcelList(WarningToApprove wq)
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

        public JsonReturn DirectorApprove(List<String> aryStatus, List<Guid> aryCaseId, string userId)
        {
            if (aryCaseId == null || !aryCaseId.Any()) return new JsonReturn() { ReturnCode = "0" };

            //并發驗證
            foreach (var item in aryCaseId)
            {
                //增加外國人ID 是否電文己執行完成
                if (staticAryCaseId != null && staticAryCaseId.Contains(item))
                {
                    TX_67100 Tx67100 = new TX_67100();
                    CustomerInfoBIZ biz = new CustomerInfoBIZ();
                     Tx67100 = biz.GetLatestTx67100byCaseId(item);
                    return new JsonReturn { ReturnCode = "0", ReturnMsg = "(" + item + ")" + "統一證號批次查詢尚未完成！" };
                }

                if (staticAryCaseId != null && staticAryCaseId.Contains(item))
                {
                    return new JsonReturn { ReturnCode = "0", ReturnMsg = "(" + item + ")" + "此案件正在處理中，請重新查詢！" };
                }
                else { staticAryCaseId.Add(item); }
            }

            bool bRtn = true;
            IDbConnection dbConnection = OpenConnection();
            IDbTransaction dbTransaction = null;
            WarningToApproveBIZ wpBiz = new WarningToApproveBIZ();
            int i = 0;
            try
            {
                using (dbConnection)
                {
                    dbTransaction = dbConnection.BeginTransaction();
                    foreach (Guid caseId in aryCaseId)
                    {
                        //*20150609 扣押並結案 不會直接結案
                        string NewId = caseId.ToString();
                        bRtn = bRtn && wpBiz.UpdateCaseStatus(caseId, CaseStatus.DirectorApprove, userId, dbTransaction) > 0;
                        i++;
                    }

                    if (bRtn)
                    {
                        dbTransaction.Commit();
                        return new JsonReturn() { ReturnCode = "1" };
                    }
                    else
                    {
                        dbTransaction.Rollback();
                        return new JsonReturn() { ReturnCode = "0" };
                    }
                }
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
                throw;
            }
            finally
            {
                staticAryCaseId.Clear();
            }
        }


        //public MemoryStream ParmCodeExcel_NPOI(WarningToApprove wq)
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

        #region Adam 2019/03/19 匯出內容與查詢結果一致
        public DataTable GetDataFromWarningGenAcctForExcel(string DocNo)
        {
            string sqlStr = "";
            base.Parameter.Clear();
            sqlStr += @" select wm.docno,wg.[Id],wg.tran_date,wg.[HangAmount],wg.[Amount],wg.[Balance],wg.[eTabs],wg.[Memo],wg.[TimeLog] from  WarningMaster wm 
  inner join WarningGenAcct wg
  on wm.DocNo like '%'+wg.CHQ_PAYEE+'%' where wm.docno = @DocNo ";
            base.Parameter.Clear();
            base.Parameter.Add(new CommandParameter("@DocNo", DocNo));
            DataTable dt = base.Search(sqlStr);
            //dt.Columns.Add("ObligorNo");
            //dt.Columns.Add("ObligorName");
            //dt.Columns.Add("SendDate");
            //dt.Columns.Add("SendNo");
            //string strObligorNo = string.Empty;
            //string strObligorName = string.Empty;
            //string strSendDate = string.Empty;
            //string strSendNo = string.Empty;
            if (dt != null && dt.Rows.Count > 0)
            {
                //string str = "";
                foreach (DataRow row in dt.Rows)
                {
                    decimal _trueamt = GetDecimal(row["HangAmount"].ToString());
                    decimal _amt = GetDecimal(row["AMOUNT"].ToString());
                    decimal _balanceamt = GetDecimal(row["Balance"].ToString());
                    //
                    if ((_amt > 0) || (_trueamt > 0))
                    {
                        if (_amt > 0)
                        {
                            row["HangAmount"] = _amt.ToString("###,###,###,###.##");
                        }
                        if (_trueamt > 0)
                        {
                            row["HangAmount"] = _trueamt.ToString("###,###,###,###.##");
                        }
                        row["Amount"] = "0";
                    }
                    else
                    {
                        row["HangAmount"] = "0";
                        row["Amount"] = _trueamt.ToString("###,###,###,###.##");
                    }
                    if (_amt < 0)
                    {
                        row["Amount"] = _amt.ToString("###,###,###,###.##");
                    }
                    if (_balanceamt > 0)
                    {
                        row["Balance"] = _balanceamt.ToString("###,###,###,###.##");
                    }
                    //

                }
            }
            //    str = str.TrimEnd(',');
            //    sqlStr = @"select CaseId,ObligorNo,ObligorName from CaseObligor where CaseId in (" + str + ");";
            //    IList<CaseObligor> listObli = base.SearchList<CaseObligor>(sqlStr);
            //    if (listObli != null && listObli.Any())
            //    {
            //        foreach (DataRow row in dt.Rows)
            //        {
            //            string strNo = "";
            //            string strName = "";
            //            foreach (CaseObligor obj in listObli.Where(m => m.CaseId.ToString() == Convert.ToString(row["CaseId"])))
            //            {
            //                strNo += UtlString.GetLastSix(obj.ObligorNo) + ";";
            //                strName += UtlString.GetFirstLetter(obj.ObligorName) + ";";
            //            }
            //            //if (strNo.Length > 0)
            //            //    strNo = strNo.Substring(1);
            //            //if (strName.Length > 0)
            //            //    strName = strName.Substring(1);

            //            row["ObligorNo"] = strNo.TrimEnd(';');
            //            row["ObligorName"] = strName.TrimEnd(';');
            //        }
            //    }

            //    sqlStr = @"select CaseId,Convert(Nvarchar(10),SendDate,111) as SendDate,SendNo from CaseSendSetting  where CaseId in (" + str + ");";
            //    IList<CaseSendSetting> listSend = base.SearchList<CaseSendSetting>(sqlStr);
            //    if (listSend != null && listSend.Any())
            //    {
            //        foreach (DataRow row in dt.Rows)
            //        {
            //            string strNo = "";
            //            string strDate = "";
            //            foreach (CaseSendSetting obj in listSend.Where(m => m.CaseId.ToString() == Convert.ToString(row["CaseId"])))
            //            {
            //                strNo += obj.SendNo + ";";
            //                strDate += obj.SendDate.ToString("yyyy/MM/dd") + ";";
            //            }

            //            //if (strNo.Length > 0)
            //            //    strNo = strNo.Substring(1);
            //            //if (strDate.Length > 0)
            //            //    strDate = strDate.Substring(1);

            //            row["SendNo"] = strNo.TrimEnd(';');
            //            row["SendDate"] = strDate.TrimEnd(';');
            //        }
            //    }
            //}
            return dt;
        }
        #endregion

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
        public MemoryStream Excel(string DocNo)
        {
            IWorkbook workbook = new HSSFWorkbook();
            ISheet sheet = workbook.CreateSheet("警示帳務LOG");
            //WarningGenAcctList = warn.WarningGenAcctSearchList(DocNo)
            DataTable dt = GetDataFromWarningGenAcctForExcel(DocNo);//獲取查詢集作一科的案件

            #region def style
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
            sheet.SetColumnWidth(0, 100 * 60);
            sheet.SetColumnWidth(1, 100 * 60);
            sheet.SetColumnWidth(2, 100 * 60);
            sheet.SetColumnWidth(3, 100 * 60);
            sheet.SetColumnWidth(4, 100 * 60);
            sheet.SetColumnWidth(5, 100 * 60);
            sheet.SetColumnWidth(6, 100 * 60);
            sheet.SetColumnWidth(7, 100 * 120);
            sheet.SetColumnWidth(8, 100 * 120);
            #endregion

            #region header
            //* line0
            SetExcelCell(sheet, 0, 0, styleHead12, "警示帳務LOG");
            sheet.AddMergedRegion(new CellRangeAddress(0, 0, 0, 9));

            //* line1
            SetExcelCell(sheet, 1, 0, styleHead10, "案件編號");
            sheet.AddMergedRegion(new CellRangeAddress(1, 1, 0, 0));
            SetExcelCell(sheet, 1, 1, styleHead10, "序號");
            sheet.AddMergedRegion(new CellRangeAddress(1, 1, 1, 1));
            SetExcelCell(sheet, 1, 2, styleHead10, "交易日");
            sheet.AddMergedRegion(new CellRangeAddress(1, 1, 2, 2));
            SetExcelCell(sheet, 1, 3, styleHead10, "掛帳金額");
            sheet.AddMergedRegion(new CellRangeAddress(1, 1, 3, 3));
            SetExcelCell(sheet, 1, 4, styleHead10, "還款金額");
            sheet.AddMergedRegion(new CellRangeAddress(1, 1, 4, 4));
            SetExcelCell(sheet, 1, 5, styleHead10, "餘額");
            sheet.AddMergedRegion(new CellRangeAddress(1, 1, 5, 5));
            SetExcelCell(sheet, 1, 6, styleHead10, "eTabs");
            sheet.AddMergedRegion(new CellRangeAddress(1, 1, 6, 6));
            SetExcelCell(sheet, 1, 7, styleHead10, "備註");
            sheet.AddMergedRegion(new CellRangeAddress(1, 1, 7, 7));
            SetExcelCell(sheet, 1, 8, styleHead10, "時程LOG");
            sheet.AddMergedRegion(new CellRangeAddress(1, 1, 8, 8));


            #endregion

            #region body
            //var ilist = GetCodeData("STATUS_NAME");
            for (int iRow = 0; iRow < dt.Rows.Count; iRow++)
            {
                //var obj = ilist.FirstOrDefault(a => a.CodeNo == Convert.ToString(dt.Rows[iRow]["Status"]));
                // string strStatus = obj != null ? obj.CodeDesc : Convert.ToString(dt.Rows[iRow]["Status"]);
                for (int iCol = 1; iCol < dt.Columns.Count; iCol++)
                {

                    if (iCol != 1)
                        SetExcelCell(sheet, iRow + 2, iCol, styleHead10, dt.Rows[iRow][iCol].ToString());
                    else
                    {
                        SetExcelCell(sheet, iRow + 2, iCol - 1, styleHead10, DocNo.ToString());
                        SetExcelCell(sheet, iRow + 2, iCol, styleHead10, dt.Rows[iRow][iCol].ToString());
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
        public ICell SetExcelCell(ISheet sheet, int rowNum, int colNum, ICellStyle style, string value)
        {

            IRow row = sheet.GetRow(rowNum) ?? sheet.CreateRow(rowNum);
            ICell cell = row.GetCell(colNum) ?? row.CreateCell(colNum);
            cell.CellStyle = style;
            cell.SetCellValue(value);
            return cell;
        }
        public MemoryStream ParmCodeExcel_NPOI(WarningToApprove model)
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
                        "通報內容",
                        "單位",
                        "客戶姓名",
                        "通報警局(單位)",
                        "帳戶狀態",
                        "結案",
                        "幣別",
                        "通報餘額",
                        "解除餘額",
                        "案發時間",
                        "案發地點",
                        "165案號"
                    };
            IList<WarningToApprove> ilst = GetQueryList(model);

            // 20190102, 匯出與晝面上不同, 原因是在頁面上有加工.. 把 
            // if( NoClosed > 0 ) Then "未結案" else "已結案"
            foreach (var il in ilst)
            {
                if (il.NoClosed > 0)
                    il.NoClosedName = "未結案";
                else
                    il.NoClosedName = "已結案";
            }

            if (ilst != null)
            {
                ms = ExcelExport(ilst, headerColumns,
                                                   delegate (HSSFRow dataRow, WarningToApprove dataItem)
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
                                                       dataRow.CreateCell(9).SetCellValue(dataItem.NotificationSource);
                                                       dataRow.CreateCell(10).SetCellValue(dataItem.NotificationUnit);
                                                       dataRow.CreateCell(11).SetCellValue(dataItem.CustName);
                                                       dataRow.CreateCell(12).SetCellValue(dataItem.PoliceStation);
                                                       dataRow.CreateCell(13).SetCellValue(dataItem.AccountStatusName);
                                                       dataRow.CreateCell(14).SetCellValue(dataItem.NoClosedName);//結案
                                                       dataRow.CreateCell(15).SetCellValue(dataItem.currency);//幣別
                                                       dataRow.CreateCell(16).SetCellValue(dataItem.NotifyBal);
                                                       dataRow.CreateCell(17).SetCellValue(dataItem.ReleaseBal);
                                                       dataRow.CreateCell(18).SetCellValue(dataItem.HappenDateTime);//案發時間
                                                       dataRow.CreateCell(19).SetCellValue(dataItem.DocAddress);//地點
                                                       dataRow.CreateCell(20).SetCellValue(dataItem.No_165);//165案號
                                                   });
            }
            return ms;
        }
    }
}
