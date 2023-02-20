using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTBC.CSFS.Models
{
    public class CaseReturn
    {
        public Guid CaseId { get; set; }
        /// <summary>
        /// 公文文號
        /// </summary>
        public string DocNo { get; set; }
        /// <summary>
        /// 類別
        /// </summary>
        public string GovKind { get; set; }
        /// <summary>
        /// 案件編號
        /// </summary>
        public string CaseNo { get; set; }
        /// <summary>
        /// 速別
        /// </summary>
        public string Speed { get; set; }
        /// <summary>
        /// 來文機關
        /// </summary>
        public string GovUnit { get; set; }
        /// <summary>
        /// 來文字號
        /// </summary>
        public string GovNo { get; set; }
        /// <summary>
        /// 來文日期
        /// </summary>
        public string GovDate { get; set; }
        /// <summary>
        /// 限辦日期
        /// </summary>
        public string LimitDate { get; set; }
        /// <summary>
        /// 分行別
        /// </summary>
        public string Unit { get; set; }
        /// <summary>
        /// 創建人員
        /// </summary>
        public string CreateUser { get; set; }
        /// <summary>
        /// 案件類型
        /// </summary>
        public string CaseKind { get; set; }
        /// <summary>
        /// 案件類別2
        /// </summary>
        public string CaseKind2 { get; set; }
        /// <summary>
        /// 來文方式
        /// </summary>
        public string ReceiveKind { get; set; }
        /// <summary>
        /// 建黨人
        /// </summary>
        public string Person { get; set; }

        /// <summary>
        /// 排序欄位
        /// </summary>
        public string SortExpression { get; set; }

        /// <summary>
        /// 排序順序
        /// </summary>
        public string SortDirection { get; set; }
        public string Name { get; set; }
        public string CloseReason { get; set; }
        public string GovDateS { get; set; }
        public string GovDateE { get; set; }
        public int PageSize { get; set; }
        public int CurrentPage { get; set; }
        public int TotalItemCount { get; set; }
        public int maxnum { get; set; }
        public string ReturnReason { get; set; }
        public string ReturnAnswer { get; set; }
    }
}
