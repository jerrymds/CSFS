using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTBC.CSFS.Models
{
    public class EDocTXT1
    {
        /// <summary>
        /// GUID
        /// </summary>
        public Guid CaseId { get; set; }
        /// <summary>
        /// 發文機關
        /// </summary>
        public string GovUnit { get; set; }
        /// <summary>
        /// 發文機關代碼
        /// </summary>
        public string GovUnitCode { get; set; }
        /// <summary>
        /// 發文日期
        /// </summary>
        public string GovDate { get; set; }
        /// <summary>
        /// 發文字號
        /// </summary>
        public string ReceiverNo { get; set; }
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
        /// 身分證統一編號
        /// </summary>
        public string ObligorNo { get; set; }
        /// <summary>
        /// 應執行金額總計
        /// </summary>
        public string Amount { get; set; }
        /// <summary>
        /// 執行必要費用
        /// </summary>
        public string Fee { get; set; }
        /// <summary>
        /// 合計
        /// </summary>
        public string Total { get; set; }
        /// <summary>
        /// 備註
        /// </summary>
        public string Memo { get; set; }
        /// <summary>
        /// 單位
        /// </summary>
        public string Unit { get; set; }
        /// <summary>
        /// 聯絡人
        /// </summary>
        public string Contact { get; set; }
        /// <summary>
        /// 電話
        /// </summary>
        public string Telephone { get; set; }
    }
}
