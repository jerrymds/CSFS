using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTBC.CSFS.Models
{
    public class EDocTXT3
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
        /// 扣押命令發文日期
        /// </summary>
        public string SeizureIssueDate { get; set; }
        /// <summary>
        /// 扣押命令發文字號
        /// </summary>
        public string SeizureIssueNo { get; set; }
        /// <summary>
        /// 收取銀行代號(總行)
        /// </summary>
        //public string ReceiveBankId { get; set; }


    }
}
