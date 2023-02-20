using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTBC.CSFS.Models
{
    public class RemitSeizure
    {
        /// <summary>
        /// 細分類
        /// </summary>
        public string CaseKind2  { get; set; }
        /// <summary>
        /// 案件ID
        /// </summary>
        public Guid CaseId { get; set; }
        /// <summary>
        /// 案件編號
        /// </summary>
        public string CaseNo { get; set; }
        /// <summary>
        /// 義務人
        /// </summary>
        public string ObligorName { get; set; }
        /// <summary>
        /// 法定代理人
        /// </summary>
        public string Agent { get; set; }
        /// <summary>
        /// 營利事業編號
        /// </summary>
        public string RegistrationNo { get; set; }
        /// <summary>
        /// 義務人統編
        /// </summary>
        public string ObligorNo { get; set; }
        /// <summary>
        /// 來函扣押總金額
        /// </summary>
        public string ReceiveAmount { get; set; }
        /// <summary>
        /// 存款帳號
        /// </summary>
        public string Account { get; set; }
        /// <summary>
        /// 幣別
        /// </summary>
        public string Currency { get; set; }
        /// <summary>
        /// 扣押金額
        /// </summary>
        public string SeizureAmount { get; set; }
        /// <summary>
        /// 扣完的可用餘額
        /// </summary>
        public string AvailBalance { get; set; }
        /// <summary>
        /// 呈核經辦
        /// </summary>
        public string AgentUser { get; set; }
        /// <summary>
        /// 鍵檔日期
        /// </summary>
        public string CreatedDate { get; set; }
        /// <summary>
        /// 備註
        /// </summary>
        public string Memo { get; set; }
        /// <summary>
        /// 扣押命令發文字號(撤銷)
        /// </summary>
        public string SendNo { get; set; }
        /// <summary>
        /// 已撤銷金額(撤銷)
        /// </summary>
        public string CancelAmount { get; set; }

    }
}
