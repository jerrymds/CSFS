using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTBC.CSFS.Models
{
	public class ExternalOldQuery
    {
		/// <summary>
		/// 收文序號
		/// </summary>
		public string ReceiveNbr { get; set; }
		/// <summary>
		/// 收文開始序號
		/// </summary>
		public string ReceiverNbrS { get; set; }
		/// <summary>
		/// 收文結束序號
		/// </summary>
		public string ReceiverNbrE { get; set; }
		/// <summary>
		/// 收件開始日期
		/// </summary>
		public string ReceiveDateS { get; set; }
		/// <summary>
		/// 收件結束日期
		/// </summary>
		public string ReceiveDateE { get; set; }
		/// <summary>
		/// 結案開始日期
		/// </summary>
		public string CloseDateS { get; set; }
		/// <summary>
		/// 結案結束日期
		/// </summary>
		public string CloseDateE { get; set; }
		/// <summary>
		/// 來文機關發文字號
		/// </summary>
		public string OrgSendCaseNbr { get; set; }
		/// <summary>
		/// 發文字號
		/// </summary>
		public string ResponseCaseNbr { get; set; }
		/// <summary>
		/// 案件狀態
		/// </summary>
		public string StatusCode { get; set; }
		/// <summary>
		/// 來函日
		/// </summary>
		public string OrgSendDate { get; set; }
		/// <summary>
		/// 來函機關
		/// </summary>
		public string OrgInstitutionName { get; set; }
		/// <summary>
		/// 發文單位承辦人
		/// </summary>
		public string SenderInstitutionClerk { get; set; }
		/// <summary>
		/// 本行發文經辦
		/// </summary>
		public string ResponseEmp { get; set; }
		/// <summary>
		/// 受文者
		/// </summary>
		public string ReceiverInstitutionName { get; set; }
		/// <summary>
		/// 副本
		/// </summary>
		public string ReceiverInstitutionEctypeName { get; set; }
		/// <summary>
		/// 結案日
		/// </summary>
		public string CloseDate { get; set; }
		/// <summary>
		/// 發文備註
		/// </summary>
		public string ResponseComment { get; set; }
		/// <summary>
		/// 統一編號
		/// </summary>
		public string AccountID { get; set; }
		/// <summary>
		/// 戶名
		/// </summary>
		public string AccountName { get; set; }
		/// <summary>
		/// 分行別代碼
		/// </summary>
		public string BranchCode { get; set; }
		/// <summary>
		/// 分行別
		/// </summary>
		public string BranchName { get; set; }
		/// <summary>
		/// 文件正本狀態
		/// </summary>
		public string PMStatus { get; set; }
		/// <summary>
		/// 正本備查序號
		/// </summary>
		public string PMReturnKey { get; set; }
		/// <summary>
		/// 資料提供者
		/// </summary>
		public string DataProvider { get; set; }
		/// <summary>
		/// 分行別起
		/// </summary>
		public string BranchCodeS { get; set; }
		/// <summary>
		/// 分行別迄
		/// </summary>
		public string BranchCodeE { get; set; }
		/// <summary>
		/// 函覆文號
		/// </summary>
		public string ResponseCaseNbr1 { get; set; }
        public int maxnum { get; set; }
        public string SortExpression { get; set; }
        public string SortDirection { get; set; }
        public int TotalItemCount { get; set; }
        public int PageSize { get; set; }
        public int CurrentPage { get; set; }
    }
}
