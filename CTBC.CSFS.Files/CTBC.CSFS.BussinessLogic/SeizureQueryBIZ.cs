using CTBC.CSFS.Models;
using CTBC.CSFS.Pattern;
using CTBC.FrameWork.Util;
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
using CTBC.FrameWork.Util;

namespace CTBC.CSFS.BussinessLogic
{
    public class SeizureQueryBIZ : CommonBIZ
    {public SeizureQueryBIZ(AppController appController)
            : base(appController)
        { }
    public SeizureQueryBIZ()
        { }
        public MemoryStream Excel(string[] headerColumns,string where)
        {
            var ms = new MemoryStream();

            IList<SeizureQuery> ilst = GetDataFromCaseMaster(where);
            
            if (ilst != null)
            {
                ms = ExcelExport(ilst, headerColumns,
                                                   delegate(HSSFRow dataRow, SeizureQuery dataItem)
                                                   {
                                                       //* 這裡可以針對每一個欄位做額外處理.比如日期
                                                       dataRow.CreateCell(0).SetCellValue(dataItem.CaseNo);
                                                       dataRow.CreateCell(1).SetCellValue(UtlString.GetLastSix(dataItem.CustId));
                                                       dataRow.CreateCell(2).SetCellValue(UtlString.GetFirstLetter(dataItem.CustName));
                                                       dataRow.CreateCell(3).SetCellValue(dataItem.BranchNo);
                                                       dataRow.CreateCell(4).SetCellValue(dataItem.BranchName);
                                                       dataRow.CreateCell(5).SetCellValue(UtlString.GetLastSix(dataItem.Account));
                                                       dataRow.CreateCell(6).SetCellValue(dataItem.Currency);
                                                       dataRow.CreateCell(7).SetCellValue(dataItem.Balance);
                                                       dataRow.CreateCell(8).SetCellValue(dataItem.SeizureAmount);
                                                       dataRow.CreateCell(9).SetCellValue(dataItem.ExchangeRate);
                                                       dataRow.CreateCell(10).SetCellValue(dataItem.SeizureAmountNtd);
                                                       dataRow.CreateCell(11).SetCellValue(dataItem.CaseKind2);
                                                       dataRow.CreateCell(12).SetCellValue(dataItem.PayCaseNo);
                                                   });
            }
            return ms;
        }

        public IList<SeizureQuery> GetData(SeizureQuery model, int pageNum, string strSortExpression, string strSortDirection, string UserId, ref string where)
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
                    sqlWhere += @" and M.CaseNo like @CaseNo ";
                    where += @" and M.CaseNo like '%" + model.CaseNo.Trim() + "%'";
                    base.Parameter.Add(new CommandParameter("@CaseNo", "%" + model.CaseNo.Trim() + "%"));
                }
                if (!string.IsNullOrEmpty(model.GovUnit))
                {
                    sqlWhere += @" and M.GovUnit like @GovUnit ";
                    where += @" and M.GovUnit like '%" + model.GovUnit.Trim() + "%'";
                    base.Parameter.Add(new CommandParameter("@GovUnit", "%" + model.GovUnit.Trim() + "%"));
                }
                if (!string.IsNullOrEmpty(model.GovNo))
                {
                    sqlWhere += @" and M.GovNo like @GovNo ";
                    where += @" and M.GovNo like '%" + model.GovNo.Trim() + "%'";
                    base.Parameter.Add(new CommandParameter("@GovNo", "%" + model.GovNo.Trim() + "%"));
                }
                if (!string.IsNullOrEmpty(model.CreateUser))
                {
                    sqlWhere += @" and M.Person like @Person ";
                    where += @" and M.Person like '%" + model.CreateUser.Trim() + "%'";
                    base.Parameter.Add(new CommandParameter("@Person", "%" + model.CreateUser.Trim() + "%"));
                }
                if (!string.IsNullOrEmpty(model.Speed))
                {
                    sqlWhere += @" and M.Speed = @Speed ";
                    where += @" and M.Speed = '" + model.Speed.Trim() + "' ";
                    base.Parameter.Add(new CommandParameter("@Speed", model.Speed.Trim()));
                }
                if (!string.IsNullOrEmpty(model.ReceiveKind))
                {
                    sqlWhere += @" and M.ReceiveKind like @ReceiveKind ";
                    where += @" and M.ReceiveKind like '%" + model.ReceiveKind.Trim() + "%'";
                    base.Parameter.Add(new CommandParameter("@ReceiveKind", "%" + model.ReceiveKind.Trim() + "%"));
                }
                if (!string.IsNullOrEmpty(model.CaseKind))
                {
                    sqlWhere += @" and M.CaseKind = @CaseKind ";
                    where += @" and M.CaseKind = '" + model.CaseKind.Trim() + "' ";
                    base.Parameter.Add(new CommandParameter("@CaseKind", model.CaseKind.Trim()));
                }
                if (!string.IsNullOrEmpty(model.CaseKind2))
                {

                    sqlWhere += @" and M.CaseKind2 = @CaseKind2 ";
                    where += @" and M.CaseKind2 = '" + model.CaseKind2.Trim() + "' ";
                    base.Parameter.Add(new CommandParameter("@CaseKind2", model.CaseKind2.Trim()));
                }
                if (!string.IsNullOrEmpty(model.GovKind))
                {

                    sqlWhere += @" and M.GovKind = @GovKind ";
                    where += @" and M.GovKind = '" + model.GovKind.Trim() + "' ";
                    base.Parameter.Add(new CommandParameter("@GovKind", model.GovKind.Trim()));
                }
                if (!string.IsNullOrEmpty(model.GovDateS))
                {
                    sqlWhere += @" AND M.GovDate >= @GovDateS";
                    where += @" and M.GovDate >= '" + model.GovDateS + "' ";
                    base.Parameter.Add(new CommandParameter("@GovDateS", model.GovDateS));
                }
                if (!string.IsNullOrEmpty(model.GovDateE))
                {
                    sqlWhere += @" AND M.GovDate <= @GovDateE ";
                    where += @" and M.GovDate <= '" + model.GovDateE + "' ";
                    base.Parameter.Add(new CommandParameter("@GovDateE", model.GovDateE));
                }
                if (!string.IsNullOrEmpty(model.BranchNo))
                {
                    sqlWhere += @" and S.BranchNo like @BranchNo ";
                    where += @" and S.BranchNo like '%" + model.BranchNo.Trim() + "%'";
                    base.Parameter.Add(new CommandParameter("@BranchNo", "%" + model.BranchNo.Trim() + "%"));
                }
                if (!string.IsNullOrEmpty(model.CreatedDateS))
                {
                    sqlWhere += @" AND M.CreatedDate >= @CreatedDateS";
                    where += @" and M.CreatedDate >= '" + model.CreatedDateS + "' ";
                    base.Parameter.Add(new CommandParameter("@CreatedDateS", model.CreatedDateS));
                }
                if (!string.IsNullOrEmpty(model.CreatedDateE))
                {
                    sqlWhere += @" AND M.CreatedDate <= @CreatedDateE ";
                    where += @" and M.CreatedDate <= '" + model.CreatedDateE + "' ";
                    base.Parameter.Add(new CommandParameter("@CreatedDateE", model.CreatedDateE));
                }
                if (!string.IsNullOrEmpty(model.OverDateS))
                {
                    model.OverDateS = UtlString.FormatDateTwStringToAd(model.OverDateS);
                    sqlWhere += @" AND M.CloseDate >= @OverDateS ";
                    where += @" and M.CloseDate >= '" + model.OverDateS + "' ";
                    base.Parameter.Add(new CommandParameter("@OverDateS", model.OverDateS));
                }
                if (!string.IsNullOrEmpty(model.OverDateE))
                {
                    model.OverDateE = UtlString.FormatDateTwStringToAd(model.OverDateE);
                    sqlWhere += @" AND M.CloseDate <= @OverDateE ";
                    where += @" and M.CloseDate <= '" + model.OverDateE + "' ";
                    base.Parameter.Add(new CommandParameter("@OverDateE", model.OverDateE));
                }
                if (!string.IsNullOrEmpty(model.Status))
                {
                    sqlWhere += @" AND M.Status = @Status ";
                    where += @" and M.Status = '" + model.Status + "' ";
                    base.Parameter.Add(new CommandParameter("@Status", model.Status));
                }
                if (!string.IsNullOrEmpty(model.AgentUser))
                {
                    sqlWhere += @" AND M.AgentUser like @AgentUser ";
                    where += @" and M.AgentUser like '%" + model.AgentUser.Trim() + "%'";
                    base.Parameter.Add(new CommandParameter("@AgentUser", "%" + model.AgentUser + "%"));
                }
                if (!string.IsNullOrEmpty(model.CustName))
                {
                    sqlWhere += @" AND S.CustName = @CustName ";
                    where += @" and S.CustName = '" + model.CustName + "' ";
                    base.Parameter.Add(new CommandParameter("@CustName", model.CustName));
                }
                if (!string.IsNullOrEmpty(model.CustId))
                {
                    sqlWhere += @" AND S.CustId = @CustId ";
                    where += @" and S.CustId = '" + model.CustId + "' ";
                    base.Parameter.Add(new CommandParameter("@CustId", model.CustId));
                }
                if (!string.IsNullOrEmpty(model.Account))
                {
                    sqlWhere += @" AND S.Account = @Account ";
                    where += @" and S.Account = '" + model.Account + "' ";
                    base.Parameter.Add(new CommandParameter("@Account", model.Account));
                }

                //發文日期
                if (!string.IsNullOrEmpty(model.SendDateS) 
                    || !string.IsNullOrEmpty(model.SendDateE)
                    || !string.IsNullOrEmpty(model.SendNo))
                {
                    string sqlWhere1 = "";
                    string where1 = "";
                    if (!string.IsNullOrEmpty(model.SendDateS))
                    {
                        sqlWhere1 += @" AND SendDate >= @SendDateStart ";
                        where1 += @" AND SendDate >= '" + UtlString.FormatDateTwStringToAd(model.SendDateS) + "' ";
                        Parameter.Add(new CommandParameter("@SendDateStart", UtlString.FormatDateTwStringToAd(model.SendDateS)));
                    }
                    if (!string.IsNullOrEmpty(model.SendDateE))
                    {
                        string sendDateEnd = Convert.ToDateTime(UtlString.FormatDateTwStringToAd(model.SendDateE)).AddDays(1).ToString("yyyy/MM/dd");
                        sqlWhere1 += @" AND SendDate < @SendDateEnd ";
                        where1 += @" AND SendDate < '" + UtlString.FormatDateTwStringToAd(sendDateEnd) + "' ";
                        Parameter.Add(new CommandParameter("@SendDateEnd", UtlString.FormatDateTwStringToAd(sendDateEnd)));
                    }
                    if (!string.IsNullOrEmpty(model.SendNo))
                    {
                        sqlWhere1 += @" AND SendNo LIKE @SendNo ";
                        where1 += @" AND SendNo LIKE '%" + model.SendNo.Trim() + "%' ";
                        Parameter.Add(new CommandParameter("@SendNo", "%" + model.SendNo.Trim() + "%"));
                    }
                    sqlWhere += " AND S.CaseId IN (SELECT CaseId  FROM CaseSendSetting AS CSS where 1=1 " + sqlWhere1 + @") ";
                    where += " AND S.CaseId IN (SELECT CaseId  FROM CaseSendSetting AS CSS where 1=1 " + where1 + @") ";
                }


                sqlWhere += @" AND M.ApproveDate IS NOT NULL ";
                where += @" and M.ApproveDate IS NOT NULL ";

                string strSql = @";with T1 
	                        as
	                        (
                                SELECT 
                                    M.CaseId,
                                    M.CaseNo,
                                    S.CustId,
                                    S.CustName,
                                    S.BranchNo,
                                    S.BranchName,
                                    S.Account,
                                    S.Currency,
                                    S.Balance,
                                    S.SeizureAmount,
                                    S.ExchangeRate,
                                    S.SeizureAmountNtd,
                                    M2.CaseKind2,   
                                    M2.CaseNo AS PayCaseNo,
                                    (SELECT CONVERT(varchar(100), M.LimitDate, 111)) as LimitDate
                                FROM 
                                    [CaseSeizure] AS S
                                    INNER JOIN [CaseMaster] AS M  ON M.CaseId = S.CaseId
                                    LEFT OUTER JOIN (SELECT SeizureId,M.CaseKind2,M.CaseNo FROM [CaseMaster] AS M
                                                    INNER JOIN [CaseSeizure] AS S ON M.[CaseId] = S.[PayCaseId] OR M.[CaseId] = S.[CancelCaseId]
                                                    WHERE M.[Status] = 'Z01') AS M2 ON S.SeizureId = M2.SeizureId
                                WHERE S.SeizureAmount > 0 " + sqlWhere + @" 
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

                IList<SeizureQuery> _ilsit = base.SearchList<SeizureQuery>(strSql);
                if (_ilsit.Count > 0)
                {
                    base.DataRecords = _ilsit[0].maxnum;
                }
                else
                {
                    base.DataRecords = 0;
                    _ilsit = new List<SeizureQuery>();
                }
                return _ilsit;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public IList<SeizureQuery> GetStatusName(string colName)
        {
            try
            {
                string strSql = @" select CodeDesc,CodeNo from PARMCode where CodeType=@CodeType ORDER BY SortOrder";
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@CodeType", colName));
                return base.SearchList<SeizureQuery>(strSql);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public IList<SeizureQuery> GetDataFromCaseMaster(string Where)
        {
            try
            {
                base.Parameter.Clear();
                string strSql = @"SELECT 
                                    M.CaseId,
                                    M.CaseNo,
                                    S.CustId,
                                    S.CustName,
                                    S.BranchNo,
                                    S.BranchName,
                                    S.Account,
                                    S.Currency,
                                    S.Balance,
                                    S.SeizureAmount,
                                    S.ExchangeRate,
                                    S.SeizureAmountNtd,
                                    M2.CaseKind2,   
                                    M2.CaseNo AS PayCaseNo,
                                    (SELECT CONVERT(varchar(100), M.LimitDate, 111)) as LimitDate
                                FROM 
                                    [CaseSeizure] AS S
                                    INNER JOIN [CaseMaster] AS M  ON M.CaseId = S.CaseId
                                    LEFT OUTER JOIN (SELECT SeizureId,M.CaseKind2,M.CaseNo FROM [CaseMaster] AS M
                                                    INNER JOIN [CaseSeizure] AS S ON M.[CaseId] = S.[PayCaseId] OR M.[CaseId] = S.[CancelCaseId]
                                                    WHERE M.[Status] = 'Z01') AS M2 ON S.SeizureId = M2.SeizureId
                                WHERE S.SeizureAmount > 0 " + Where;
                strSql += " order by CaseId asc";
                IList<SeizureQuery> _ilsit = base.SearchList<SeizureQuery>(strSql);
                if (_ilsit.Count > 0)
                {
                    base.DataRecords = _ilsit[0].maxnum;
                }
                else
                {
                    base.DataRecords = 0;
                    _ilsit = new List<SeizureQuery>();
                }
                return _ilsit;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
    }
}
