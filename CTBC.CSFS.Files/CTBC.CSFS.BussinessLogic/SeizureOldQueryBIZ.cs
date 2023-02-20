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
	public class SeizureOldQueryBIZ : CommonBIZ
	{
		public SeizureOldQueryBIZ(AppController appController)
			: base(appController)
		{ }
		public SeizureOldQueryBIZ()
		{
		}

		public IList<SeizureOldQuery> GetData(SeizureOldQuery model, int pageNum, string strSortExpression, string strSortDirection, string UserId, ref string where)
		{
			try
			{
				where = string.Empty;
				string sqlWhere = "1=1";
				base.PageIndex = pageNum;
				base.Parameter.Clear();
				base.Parameter.Add(new CommandParameter("@pageS", (base.PageSize * (base.PageIndex - 1)) + 1));
				base.Parameter.Add(new CommandParameter("@pageE", base.PageSize * base.PageIndex));
				//收文開始序號
				if (!string.IsNullOrEmpty(model.ReceiptSeqS))
				{
					sqlWhere += @" AND A.RECEIPT_SEQ >= @ReceiptSeqS";
					where += @" and A.RECEIPT_SEQ >= '" + model.ReceiptSeqS + "' ";
					base.Parameter.Add(new CommandParameter("@ReceiptSeqS", model.ReceiptSeqS));
				}
				//收文結束序號
				if (!string.IsNullOrEmpty(model.ReceiptSeqE))
				{
					sqlWhere += @" AND A.RECEIPT_SEQ <= @ReceiptSeqE ";
					where += @" and A.RECEIPT_SEQ <= '" + model.ReceiptSeqE + "' ";
					base.Parameter.Add(new CommandParameter("@ReceiptSeqE", model.ReceiptSeqE));
				}
				//收件開始日期
				if (!string.IsNullOrEmpty(model.ReceivedDateS))
				{
					sqlWhere += @" AND A.RECEIVED_DATE >= @ReceivedDateS";
					where += @" and A.RECEIVED_DATE >= '" + model.ReceivedDateS + "' ";
					base.Parameter.Add(new CommandParameter("@ReceivedDateS", model.ReceivedDateS));
				}
				//收件結束日期
				if (!string.IsNullOrEmpty(model.ReceivedDateE))
				{
					sqlWhere += @" AND A.RECEIVED_DATE <= @ReceivedDateE ";
					where += @" and A.RECEIVED_DATE <= '" + model.ReceivedDateE + "' ";
					base.Parameter.Add(new CommandParameter("@ReceivedDateE", model.ReceivedDateE));
				}
				//發文字號
				if (!string.IsNullOrEmpty(model.SendSeq))
				{
					sqlWhere += @" and B.SEND_SEQ like @SendSeq ";
					where += @" and B.SEND_SEQ like '%" + model.SendSeq.Trim() + "%'";
					base.Parameter.Add(new CommandParameter("@SendSeq", "%" + model.SendSeq.Trim() + "%"));
				}
				//統一編號
				if (!string.IsNullOrEmpty(model.ObligorCompanyId))
				{
					sqlWhere += @" and C.OBLIGOR_COMPANY_ID like @ObligorCompanyId ";
					where += @" and C.OBLIGOR_COMPANY_ID like '%" + model.ObligorCompanyId.Trim() + "%'";
					base.Parameter.Add(new CommandParameter("@ObligorCompanyId", "%" + model.ObligorCompanyId.Trim() + "%"));
				}
				//分行別(起)
				if (!string.IsNullOrEmpty(model.BranchIdS))
				{
					sqlWhere += @" AND E.BRANCH_CODE >= @BranchIdS";
					where += @" and E.BRANCH_CODE >= '" + model.BranchIdS + "' ";
					base.Parameter.Add(new CommandParameter("@BranchIdS", model.BranchIdS));
				}
				//分行別(迄) 
				if (!string.IsNullOrEmpty(model.BranchIdE))
				{
					sqlWhere += @" AND E.BRANCH_CODE <= @BranchIdE";
					where += @" and E.BRANCH_CODE <= '" + model.BranchIdE + "' ";
					base.Parameter.Add(new CommandParameter("@BranchIdE", model.BranchIdE));
				}

				if (strSortExpression == "RECEIPT_SEQ")
				{
					strSortExpression = "ReceiptSeq";
				}
				else if (strSortExpression == "RECEIVED_DATE")
				{
					strSortExpression = "ReceivedDate";
				}
				else if (strSortExpression == "BRANCH_ID")
				{
					strSortExpression = "BranchId";
				}
				else if (strSortExpression == "OBLIGOR_ACCOUNT_NAME")
				{
					strSortExpression = "ObligorAccountName";
				}
				else if (strSortExpression == "OBLIGOR_COMPANY_ID")
				{
					strSortExpression = "ObligorCompanyId";
				}
				else if (strSortExpression == "CASE_PROCESS_STATUS")
				{
					strSortExpression = "CaseProcessStatus";
				}
				else if (strSortExpression == "SEND_DATE")
				{
					strSortExpression = "SendDate";
				}
				else if (strSortExpression == "SEND_SEQ")
				{
					strSortExpression = "SendSeq";
				}
				else if (strSortExpression == "END_CASE_REMARK")
				{
					strSortExpression = "EndCaseRemark";
				}

				string strSql = @";with T1 
	                        as
	                        (
                                select 
									A.RECEIPT_ID as ReceiptId,
									A.RECEIPT_SEQ as ReceiptSeq, --收文序號
									A.RECEIVED_DATE as ReceivedDate, --收件日期
									A.BRANCH_ID as BranchId, --分行別
									C.OBLIGOR_ACCOUNT_NAME as ObligorAccountName, --義(債)務人戶名
									C.OBLIGOR_COMPANY_ID as ObligorCompanyId, --義(債)務人統編
									D.CASE_PROCESS_STATUS as CaseProcessStatus, --案件處理狀態
									CONVERT(varchar(100), B.SEND_DATE, 111) as SendDate, --發文日
									B.SEND_SEQ as SendSeq,--發文字號
									A.END_CASE_REMARK as EndCaseRemark --結案備註
									from dbo.RECEIPT A 
									left join dbo.WRITEBACK B on A.RECEIPT_ID = B.RECEIPT_ID
									left join dbo.OBLIGOR C on A.RECEIPT_ID = C.RECEIPT_ID
									left join dbo.CASE_STATUS D on A.CASE_STATUS_ID = D.CASE_STATUS_ID
									left join dbo.BRANCH E on A.BRANCH_ID = E.BRANCH_ID
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

				IList<SeizureOldQuery> _ilsit = base.SearchList<SeizureOldQuery>(strSql);
				if (_ilsit.Count > 0)
				{
					base.DataRecords = _ilsit[0].maxnum;
				}
				else
				{
					base.DataRecords = 0;
					_ilsit = new List<SeizureOldQuery>();
				}
				return _ilsit;
			}
			catch (Exception ex)
			{

				throw ex;
			}
		}

		#region 獲取類型List
		public List<SeizureOldDetails1> GetSeizureOldDetail1List(string ReceiptId)
		{
			string sqlStr = @"select 
A.RECEIPT_SEQ as ReceiptSeq, --本行收文序號
B.CASE_PROCESS_STATUS as CaseProcessStatus, --案件處理狀態
--A.BRANCH_ID as BranchId, --收文分行
D.BRANCH_NAME as BranchName, --收文分行
C.INSTITUTION_NAME as InstitutionName, --來函機關
C.INSTITUTION_ADDRESS as InstitutionAddress, --地址
A.INS_DEPT_DISPATCH_ID as InsDeptDispatchId, --來函機關發文字號
CONVERT(varchar(100), A.INS_DISPATCH_DATE, 111) as InsDispatchDate, --來函機關發文日期
A.END_CASE_REMARK as EndCaseRemark --結案備註
from dbo.RECEIPT A 
left join dbo.CASE_STATUS B on A.CASE_STATUS_ID = B.CASE_STATUS_ID
left join dbo.INSTITUTION C on A.INSTITUTION_ID = C.INSTITUTION_ID
left join dbo.BRANCH D on A.BRANCH_ID = D.BRANCH_ID
where A.RECEIPT_ID = @ReceiptId ";
			base.Parameter.Clear();
			base.Parameter.Add(new CommandParameter("@ReceiptId", ReceiptId));

			List<SeizureOldDetails1> _ilsit = base.SearchList<SeizureOldDetails1>(sqlStr).ToList();
			if (_ilsit != null)
			{
				return _ilsit;
			}
			else
			{
				return new List<SeizureOldDetails1>();
			}
		}

		public List<SeizureOldDetails1> GetSeizureOldDetail1_1List(string ReceiptId)
		{
			string sqlStr = @"select 
A.RECEIPT_SEQ as ReceiptSeq, --本行收文序號
D.CLERK as Clerk, --處理經辦
D.SEND_SEQ as SendSeq, --發文字號
CONVERT(varchar(100), D.SEND_DATE, 111) as SendDate --發文日期
from dbo.RECEIPT A 
left join dbo.WRITEBACK D on A.RECEIPT_ID = D.RECEIPT_ID
where A.RECEIPT_ID = @ReceiptId ";
			base.Parameter.Clear();
			base.Parameter.Add(new CommandParameter("@ReceiptId", ReceiptId));

			List<SeizureOldDetails1> _ilsit = base.SearchList<SeizureOldDetails1>(sqlStr).ToList();
			if (_ilsit != null)
			{
				return _ilsit;
			}
			else
			{
				return new List<SeizureOldDetails1>();
			}
		}
		#endregion

		#region 獲取扣押及撤銷List1
		public List<SeizureOldDetails2> GetSeizureOldDetail2_1List(string ReceiptId)
		{
			try
			{
				string sqlStr = "";
				sqlStr += @" select 
B.OBLIGOR_ACCOUNT_NAME as ObligorAccountName, --義(債)務人戶名
B.OBLIGOR_COMPANY_ID as ObligorCompanyId, --義(債)務人統編
B.OBL_SEIZURE_REMARK as SendRemark --發文備註
from dbo.RECEIPT A
left join dbo.OBLIGOR B on A.RECEIPT_ID = B.RECEIPT_ID
where A.RECEIPT_ID = @ReceiptId ";
				base.Parameter.Clear();
				base.Parameter.Add(new CommandParameter("@ReceiptId", ReceiptId));
				List<SeizureOldDetails2> _ilsit = base.SearchList<SeizureOldDetails2>(sqlStr).ToList();

				if (_ilsit != null)
				{
					return _ilsit;
				}
				else
				{
					return new List<SeizureOldDetails2>();
				}
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}
		#endregion

		#region 獲取扣押及撤銷List2
		public List<SeizureOldDetails2> GetSeizureOldDetail2_2List(string ReceiptId)
		{
			try
			{
				string sqlStr = "";
				sqlStr += @"  select B.RECEIPT_ID,C.WRITEBACK_ID,E.WRITEBACK_ID,
B.OBLIGOR_ACCOUNT_NAME as ObligorAccountName, --義(債)務人戶名
D.BRANCH_NAME as BranchName, --存款分行
C.SEIZURE_SUM as SeizureSum, --扣押金額
E.SEND_SEQ as SendSeq --發文字號
from dbo.RECEIPT A
left join dbo.OBLIGOR B on A.RECEIPT_ID = B.RECEIPT_ID
left join dbo.OBLIGOR_SEIZURE C on B.OBLIGOR_ID = C.OBLIGOR_ID
left join dbo.BRANCH D on C.SAVING_BRANCH_ID = D.BRANCH_ID
left join dbo.WRITEBACK E on C.WRITEBACK_ID = E.WRITEBACK_ID
where A.RECEIPT_ID = @ReceiptId ";
				base.Parameter.Clear();
				base.Parameter.Add(new CommandParameter("@ReceiptId", ReceiptId));
				List<SeizureOldDetails2> _ilsit = base.SearchList<SeizureOldDetails2>(sqlStr).ToList();

				if (_ilsit != null)
				{
					return _ilsit;
				}
				else
				{
					return new List<SeizureOldDetails2>();
				}
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}
		#endregion

		#region 獲取支付List
		public List<SeizureOldDetails3> GetSeizureOldDetail3List(string ReceiptId)
		{
			try
			{
				string sqlStr = "";
            sqlStr += @" select 
MONEY_RECEIVER_PAY as MoneyReceiverPay, --受款人
C_B_CHEQUE_NUM_PAY as CBChequeNumPay, --本行支票號碼
CHARGE_SUM_PAY as ChargeSumPay,  --收取金額
SEND_RECEIVER as SendReceiver,   --受文者
RECEIVER_ADDRESS as ReceiverAddress, --地址
CC_RECEIVER as CCReceiver, --副本
CC_ADDRESS as CCAddress, --地址
SEND_REMARK as SendRemark --說明
from dbo.WRITEBACK
where RECEIPT_ID = @ReceiptId ";
				base.Parameter.Clear();
				base.Parameter.Add(new CommandParameter("@ReceiptId", ReceiptId));

				List<SeizureOldDetails3> _ilsit = base.SearchList<SeizureOldDetails3>(sqlStr).ToList();
				if (_ilsit != null)
				{
					return _ilsit;
				}
				else
				{
					return new List<SeizureOldDetails3>();
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
