using System;
using System.Dynamic;

namespace CTBC.CSFS.Models
{
    public class HistoryCaseSeizure
    {
        public int SeizureId { get; set; }
        public Guid CaseId { get; set; }
        public Guid? PayCaseId { get; set; }
        public string CaseNo { get; set; }
        public string CustId { get; set; }
        public string CustIdflag { get; set; }    
        public string CustName { get; set; }
        public string CustNameflag { get; set; }
        public string BranchNo { get; set; }
        public string BranchNoflag { get; set; }
        public string BranchName { get; set; }
        public string BranchNameflag { get; set; }
        public string Account { get; set; }
        public string Accountflag { get; set; }
        public string AccountStatus { get; set; }
        public string AccountStatusflag { get; set; }
        public string Currency { get; set; }
        public string Currencyflag { get; set; }
        public string Balance { get; set; }
        public string Balanceflag { get; set; }
        public decimal SeizureAmount { get; set; }
        public string SeizureAmountflag { get; set; }
        public decimal ExchangeRate { get; set; }
        public string ExchangeRateflag { get; set; }
        public decimal SeizureAmountNtd { get; set; }
        public string SeizureAmountNtdflag { get; set; }
        public decimal PayAmount { get; set; }
        public string PayAmountflag { get; set; }
        public string SeizureStatus { get; set; }
        public string CreatedUser { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedUser { get; set; }
        public DateTime ModifiedDate { get; set; }
        public Guid? CancelCaseId { get; set; }
        public string GovUnit { get; set; }
        public string GovDate { get; set; }
        public string GovNo { get; set; }
		public int IsFromZJ { get; set; }
		public bool IsAdd { get; set; }
		public bool IsUpdate { get; set; }
		public bool IsDelete { get; set; }
        //* 20150601 扣押新增三個欄位 , 產品形態,關係,管理區分
        /// <summary>
        /// 產品型態
        /// </summary>
        public string ProdCode { get; set; }
        /// <summary>
        /// 關係
        /// </summary>
        public string ProdCodeflag { get; set; }
        public string Link { get; set; }
        /// <summary>
        /// 管理區分
        /// </summary>
        public string Linkflag { get; set; }
        public string SegmentCode { get; set; }
        public string SegmentCodeflag { get; set; }

        //*電文狀態
        public string Status { get; set; }
        public string System { get; set; }
        public string InvestCode { get; set; }
        /// <summary>
        /// 他案扣押
        /// </summary>
        public string OtherSeizure { get; set; }
        public string OtherSeizureflag { get; set; }
        ///// <summary>
        ///// 修改狀態
        ///// </summary>
        //public int EditStatus { get; set; }
        /// <summary>
        /// 是否被勾選
        /// </summary>
        public string IsCheck { get; set; }
        /// <summary>
        /// 已撤銷金額
        /// </summary>
        public decimal CancelAmount { get; set; }
        public string CancelAmountflag { get; set; }
        /// <summary>
        /// 解扣金額
        /// </summary>
        public decimal TripAmount { get; set; }
        public string TripAmountflag { get; set; }
        /// <summary>
        /// 是否拋查電文 0 否 1 是 (成功)
        /// </summary>
        public string TxtStatus { get; set; }
        public int SeizureSeq { get; set; } // 扣押的順序
        public string TxtProdCode { get; set; } //TX_60491_Detl 的ProdCode
        public string TxtProdCodeflag { get; set; }
        public int SNO { get; set; }
        public int FKSNO { get; set; }
    }
}