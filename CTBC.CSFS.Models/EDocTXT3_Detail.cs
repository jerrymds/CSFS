using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTBC.CSFS.Models
{
    public class EDocTXT3_Detail
    {
        /// <summary>
        /// GUID
        /// </summary>
        public Guid CaseId { get; set; }

        /// ----------------以下是   收取媒體檔(txt檔)  的欄位
        /// <summary>
        /// 收取銀行代號(總行)
        /// </summary>
        public string ReceiveBankId { get; set; }
        /// <summary>
        /// 扣押金額
        /// </summary>
        public string SeizureAmount { get; set; }
        /// <summary>
        /// 收取金額
        /// </summary>
        public string ReceiveAmount { get; set; }
        /// <summary>
        /// 收取金額_執行必要費用
        /// </summary>
        public string ReceiveFee { get; set; }
        /// <summary>
        /// 超過收取金額部分是否撤銷
        /// </summary>
        public string OverPayCancel { get; set; }


        /// ----------------以下是   收取分配媒體檔(txt檔)  的欄位

        /// <summary>
        /// 收取單位
        /// </summary>
        public string ReceiveUnit { get; set; }
        /// <summary>
        /// 收取金額_案款
        /// </summary>
        public string ReceiveAmount_Case { get; set; }
        /// <summary>
        /// 收取金額_執行必要費用
        /// </summary>
        public string ReceiveFee_Case { get; set; }
        /// <summary>
        /// 支票寄送地址
        /// </summary>
        public string CheckAddress { get; set; }
        /// <summary>
        /// 單位
        /// </summary>
        public string Unit { get; set; }
        /// <summary>
        /// 存摺摘要編號
        /// </summary>
        public string PassbookAbsNo { get; set; }
        /// <summary>
        /// 銷帳編號
        /// </summary>
        public string WriteOffNo { get; set; }
        /// <summary>
        /// 受款人名稱
        /// </summary>
        public string ReceiveName { get; set; }
    }
}
