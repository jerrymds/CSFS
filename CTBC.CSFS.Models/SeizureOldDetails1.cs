using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTBC.CSFS.Models
{
	public class SeizureOldDetails1
	{
		/// 收文ID
		/// </summary>
		public string ReceiptId { get; set; }
		/// <summary>
		/// 收文序號
		/// </summary>
		public string ReceiptSeq { get; set; }
		/// <summary>
		/// 案件處理狀態
		/// </summary>
		public string CaseProcessStatus { get; set; }
		/// <summary>
		/// 收文分行
		/// </summary>
		public string BranchId { get; set; }
		/// <summary>
		/// 收文分行
		/// </summary>
		public string BranchName { get; set; }
		/// <summary>
		/// 處理經辦
		/// </summary>
		public string Clerk { get; set; }
		/// <summary>
		/// 來函機關
		/// </summary>
		public string InstitutionName { get; set; }
		/// <summary>
		///地址
		/// </summary> 
		public string InstitutionAddress { get; set; }
		/// <summary>
		/// 來函機關發文字號
		/// </summary>
		public string InsDeptDispatchId { get; set; }
		/// <summary>
		/// 來函機關發文日期
		/// </summary>
		public string InsDispatchDate { get; set; }
		/// <summary>
		/// 副本
		/// </summary>
		public string CCReceiver { get; set; }
		/// <summary>
		/// 地址
		/// </summary>
		public string CCAddress { get; set; }
		/// <summary>
		/// 發文字號
		/// </summary>
		public string SendSeq { get; set; }
		/// <summary>
		/// 發文日期
		/// </summary>
		public string SendDate { get; set; }
		/// <summary>
		/// 結案備註
		/// </summary>
		public string EndCaseRemark { get; set; }
		/// <summary>
		/// 發文備註
		/// </summary>
		public string SendRemark { get; set; }

		public int maxnum { get; set; }
        public string SortExpression { get; set; }
        public string SortDirection { get; set; }
        public int TotalItemCount { get; set; }
        public int PageSize { get; set; }
        public int CurrentPage { get; set; }
    }
}
