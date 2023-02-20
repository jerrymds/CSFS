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
	public class ExternalOldQueryBIZ : CommonBIZ
	{
		public ExternalOldQueryBIZ(AppController appController)
			: base(appController)
		{ }
		public ExternalOldQueryBIZ()
		{
		}

		public IList<ExternalOldQuery> GetData(ExternalOldQuery model, int pageNum, string strSortExpression, string strSortDirection, string UserId, ref string where)
		{
			try
			{
				string sqlWhere = "1=1";
				base.PageIndex = pageNum;
				base.Parameter.Clear();
				base.Parameter.Add(new CommandParameter("@pageS", (base.PageSize * (base.PageIndex - 1)) + 1));
				base.Parameter.Add(new CommandParameter("@pageE", base.PageSize * base.PageIndex));
				//收文開始序號
				if (!string.IsNullOrEmpty(model.ReceiverNbrS))
				{
					sqlWhere += @" AND A.ReceiveNbr >= @ReceiverNbrS";
					base.Parameter.Add(new CommandParameter("@ReceiverNbrS", model.ReceiverNbrS));
				}
				//收文結束序號
				if (!string.IsNullOrEmpty(model.ReceiverNbrE))
				{
					sqlWhere += @" AND A.ReceiveNbr <= @ReceiverNbrE ";
					base.Parameter.Add(new CommandParameter("@ReceiverNbrE", model.ReceiverNbrE));
				}
				//收件開始日期
				if (!string.IsNullOrEmpty(model.ReceiveDateS))
				{
					sqlWhere += @" AND A.ReceiveDate >= @ReceivedDateS";
					base.Parameter.Add(new CommandParameter("@ReceivedDateS", model.ReceiveDateS));
				}
				//收件結束日期
				if (!string.IsNullOrEmpty(model.ReceiveDateE))
				{
					sqlWhere += @" AND A.ReceiveDate <= @ReceivedDateE";
					base.Parameter.Add(new CommandParameter("@ReceivedDateE", model.ReceiveDateE));
				}
				//結案開始日期
				if (!string.IsNullOrEmpty(model.CloseDateS))
				{
					sqlWhere += @" AND A.CloseDate >= @CloseDateS";
					base.Parameter.Add(new CommandParameter("@CloseDateS", model.CloseDateS));
				}
				//結案結束日期
				if (!string.IsNullOrEmpty(model.CloseDateE))
				{
					sqlWhere += @" AND A.CloseDate <= @CloseDateE ";
					base.Parameter.Add(new CommandParameter("@CloseDateE", model.CloseDateE));
				}
				//發文字號
				if (!string.IsNullOrEmpty(model.ResponseCaseNbr))
				{
					sqlWhere += @" and A.ResponseCaseNbr like @ResponseCaseNbr ";
					base.Parameter.Add(new CommandParameter("@ResponseCaseNbr", "%" + model.ResponseCaseNbr.Trim() + "%"));
				}
				//來函機關發文字號
				if (!string.IsNullOrEmpty(model.OrgSendCaseNbr))
				{
					sqlWhere += @" and A.OrgSendCaseNbr like @OrgSendCaseNbr ";
					base.Parameter.Add(new CommandParameter("@OrgSendCaseNbr", "%" + model.OrgSendCaseNbr.Trim() + "%"));
				}
				//分行別(起)
				if (!string.IsNullOrEmpty(model.BranchCodeS))
				{
					sqlWhere += @" AND cast(A.BranchCode as int) >= @BranchCodeS";
					base.Parameter.Add(new CommandParameter("@BranchCodeS", Convert.ToInt32(model.BranchCodeS)));
				}
				//分行別(迄) 
				if (!string.IsNullOrEmpty(model.BranchCodeE))
				{
					sqlWhere += @" AND cast(A.BranchCode as int) >= @BranchIdE";
					base.Parameter.Add(new CommandParameter("@BranchIdE", Convert.ToInt32(model.BranchCodeE)));
				}

				string strSql = @";with T1 
	                        as
	                        (
                               select 
								ReceiveNbr,--收文序號
								ResponseCaseNbr,--發文字號
								AccountName,--戶名
								AccountID,--統一編號
								--StatusCode,--案件狀態
								case StatusCode when 0 then '收文處理中'
								when 1 then '發文處理中'
								when 3 then '已結案'
								when 4 then '刪除' else '' end as StatusCode, --案件狀態,
								SenderInstitutionClerk,--發文經辦
								B.Name as ReceiverInstitutionName,--受文者
								--cast(ResponseCaseNbr as varchar) +'字第'+ cast(ResponseCaseNbrYear as varchar)+ cast(ResponseCaseNbrSN as varchar)+'號' as ResponseCaseNbr1,--函覆文號
								OrgSendCaseNbr as ResponseCaseNbr1, --函覆文號,
								CONVERT(varchar(100), CloseDate, 111) as CloseDate--結案日
								from CaseExternal A
								left join SysAddressList B on A.ReceiverInstitutionCode = B.uid
								left join SysBranch C on A.BranchCode = C.BranchCode
                                WHERE " + sqlWhere + @" 
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

				IList<ExternalOldQuery> _ilsit = base.SearchList<ExternalOldQuery>(strSql);
				if (_ilsit.Count > 0)
				{
					base.DataRecords = _ilsit[0].maxnum;
				}
				else
				{
					base.DataRecords = 0;
					_ilsit = new List<ExternalOldQuery>();
				}
				return _ilsit;
			}
			catch (Exception ex)
			{

				throw ex;
			}
		}

		#region 獲取明細
		public List<ExternalOldDetails> GetExternalOldDetailList(string ReceiveNbr)
		{
			string sqlStr = @"select distinct
								ReceiveNbr, --收文序號,
								--StatusCode, --案件狀態,
								case StatusCode when 0 then '收文處理中'
								when 1 then '發文處理中'
								when 3 then '已結案'
								when 4 then '刪除' else '' end as StatusCode, --案件狀態,
								CONVERT(varchar(100), OrgSendDate, 111) as OrgSendDate, --來函日,
								B.Name as OrgInstitutionName,--來函機關,
								--cast(ResponseCaseNbr as varchar) +'字第'+ cast(ResponseCaseNbrYear as varchar)+ cast(ResponseCaseNbrSN as varchar)+'號' as ResponseCaseNbr1, --函覆文號,
								OrgSendCaseNbr as ResponseCaseNbr1, --函覆文號,
								SenderInstitutionClerk, --發文單位承辦人,
								C.Name as ReceiverInstitutionName,--受文者,
								D.Name as ReceiverInstitutionEctypeName,--副本,
								CONVERT(varchar(100), CloseDate, 111) as CloseDate, --結案日,
								ResponseCaseNbr, --發文字號,
								--ResponseEmp, --本行發文經辦,
								H.EmployeeName as ResponseEmp, --本行發文經辦,
								ResponseComment, --發文備註,
								AccountID, --統一編號,
								AccountName, --戶名,
								E.BranchName,--分行別,
								--PMStatus, --文件正本狀態,
								case isnull(PMStatus,'') 
								when '' then ''
								when '0' then '無須借出'
								when '2' then '借出中'
								when '4' then '已歸還' else '' end as PMStatus, --文件正本狀態,
								--PMReturnKey, --正本備查序號,
								PMReturnKey = case isnull(A.PMStatus,'') 
								when '' then ''
								when 0 then ''
								when 2 then A.PMKey
								when 4 then (case when isnull(A.PMKey,'') = '' then A.PMReturnKey else A.PMKey end) else PMReturnKey end, --正本備查序號,
								--isnull(G.Cost * F.Quantity,0) as OperationMoney, --手續費金額,
								isnull(F.Quantity,0) as OperationMoney, --手續費金額,
								DataProvider, --資料提供者,
								ResponseContent, --回覆資料內容
								I.BranchName as PMBranchNbr --借出分行
								from CaseExternal A
								left join SysAddressList B on A.OrgInstitutionCode = B.UID
								left join SysAddressList C on A.ReceiverInstitutionCode = C.UID
								left join SysAddressList D on A.ReceiverInstitutionEctypeCode = D.UID
								left join SysBranch E on A.BranchCode = E.BranchCode
								left join CaseExternalPAC F on A.UID = F.FID and F.PACId = (select uid from SysPAC where Name = '手續費金額')
								--left join SysPAC G on F.PACId = G.UID and G.Name = '手續費金額'
								left join SysEmployee H on A.ResponseEmp = H.EmployeeNbr
								left join SysBranch I on A.PMBranchNbr = I.BranchCode
								where ReceiveNbr = @ReceiveNbr ";
			base.Parameter.Clear();
			base.Parameter.Add(new CommandParameter("@ReceiveNbr", ReceiveNbr.Trim()));

			List<ExternalOldDetails> _ilsit = base.SearchList<ExternalOldDetails>(sqlStr).ToList();
			if (_ilsit != null)
			{
				return _ilsit;
			}
			else
			{
				return new List<ExternalOldDetails>();
			}
		}
		#endregion
	}
}
