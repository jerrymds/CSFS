using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTBC.CSFS.Models
{
    public class EDocTXT2_Detail
    {
        /// <summary>
        /// GUID
        /// </summary>
        public Guid CaseId { get; set; }
        /// <summary>
        /// 執行案號
        /// </summary>
        public string ExecCaseNo { get; set; }
        /// <summary>
        /// 移送機關代碼
        /// </summary>
        public string TransferUnitID { get; set; }
        /// <summary>
        /// 移送案號
        /// </summary>
        public string TransferCaseNo { get; set; }
        /// <summary>
        /// 管理代號
        /// </summary>
        public string ManageID { get; set; }
    }
}
