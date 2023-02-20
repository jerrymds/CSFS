using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CTBC.CSFS.Models;

namespace CTBC.CSFS.ViewModels
{
    public class CancelSeizureViewModel
    {
        /// <summary>
        /// 來文字號
        /// </summary>
        public string GovNo { get; set; }

        public Guid CaseId { get; set; }
        /// <summary>
        /// 客戶ID
        /// </summary>
        public string CustId { get; set; }

        /// <summary>
        /// 客戶姓名
        /// </summary>
        public string CustName { get; set; }

        /// <summary>
        /// 客戶帳號
        /// </summary>
        public string Account { get; set; }

        /// <summary>
        /// 來文機關
        /// </summary>
        public string GoveUnit { get; set; }

        /// <summary>
        /// 已儲存
        /// </summary>
        public IList<CaseSeizure> ListSaved { get; set; }

        /// <summary>
        /// 查詢結果
        /// </summary>
        public IList<CaseSeizure> QueryResult { get; set; }
    }
}
