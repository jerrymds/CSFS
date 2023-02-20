using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTBC.CSFS.Models
{
    public class HistoryBatchQueue : Entity
    {
        /// <summary>
        /// 電文狀態 結果 0 待查,1成功,2失敗
        /// </summary>
        public string Status { get; set; }
        /// <summary>
        /// 經辦人
        /// </summary>
        public string SendUser { get; set; }
        /// <summary>
        /// 義務人
        /// </summary>
        public string ObligorNo { get; set; }
        /// <summary>
        /// 電文編號
        /// </summary>
        public string ServiceName { get; set; }
        /// <summary>
        /// 錯誤訊息
        /// </summary>
        public string ErrorMsg { get; set; }
        /// <summary>
        /// 發查時間
        /// </summary>
        public DateTime? SendDate { get; set; }
        /// <summary>
        /// 建檔時間
        /// </summary>
        public DateTime? CreateDatetime { get; set; }
    }
}
