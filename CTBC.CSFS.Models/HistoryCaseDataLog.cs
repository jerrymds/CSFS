using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTBC.CSFS.Models
{
    public  class HistoryCaseDataLog:Entity
    {
		public Guid TXSNO { get; set; }
		public DateTime TXDateTime { get; set; }
		public string TXUser { get; set; }
		public string TXUserName { get; set; }
		public string TXType { get; set; }
		public string md_FuncID { get; set; }
		public string TITLE { get; set; }
		public string TabID { get; set; }
		public string TabName { get; set; }
		public string TableName { get; set; }
		public string TableDispActive { get; set; }
		public string ColumnID { get; set; }
		public string ColumnName { get; set; }
		public string ColumnValueBefore { get; set; }
		public string ColumnValueAfter { get; set; }
		public string CaseId { get; set; }
		public string CaseNo { get; set; }
		public int DispSrNo { get; set; }
		public string LinkDataKey { get; set; }

		public int maxnum { get; set; }
		public string idkey { get; set; }
    }
}

