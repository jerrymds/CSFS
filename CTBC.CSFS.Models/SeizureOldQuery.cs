using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTBC.CSFS.Models
{
    public class SeizureOldQuery
    {
		/// <summary>
		/// 收文ID
		/// </summary>
		public string ReceiptId { get; set; }
		/// <summary>
		/// 收文序號
		/// </summary>
		public string ReceiptSeq { get; set; }
		/// <summary>
		/// 收文開始序號
		/// </summary>
		public string ReceiptSeqS { get; set; }
		/// <summary>
		/// 收文結束序號
		/// </summary>
		public string ReceiptSeqE { get; set; }
		/// <summary>
		/// 收件日期
		/// </summary>
		public string ReceivedDate { get; set; }
		/// <summary>
		/// 收件開始日期
		/// </summary>
		public string ReceivedDateS { get; set; }
		/// <summary>
		/// 收件結束日期
		/// </summary>
		public string ReceivedDateE { get; set; }
		/// <summary>
		/// 發文字號
		/// </summary>
		public string SendSeq { get; set; }
		/// <summary>
		/// 義(債)務人戶名
		/// </summary>
		public string ObligorAccountName { get; set; }
		/// <summary>
		/// 義(債)務人統編
		/// </summary>
		public string ObligorCompanyId { get; set; }
		/// <summary>
		/// 案件處理狀態
		/// </summary>
		public string CaseProcessStatus { get; set; }
		/// <summary>
		/// 發文日
		/// </summary>
		public string SendDate { get; set; }
		/// <summary>
		/// 結案備註
		/// </summary>
		public string EndCaseRemark { get; set; }
		/// <summary>
		/// 分行別
		/// </summary>
		public string BranchId { get; set; }
		/// <summary>
		/// 分行別起
		/// </summary>
		public string BranchIdS { get; set; }
		/// <summary>
		/// 分行別迄
		/// </summary>
		public string BranchIdE { get; set; }
        public int maxnum { get; set; }
        public string SortExpression { get; set; }
        public string SortDirection { get; set; }
        public int TotalItemCount { get; set; }
        public int PageSize { get; set; }
        public int CurrentPage { get; set; }
    }
}
