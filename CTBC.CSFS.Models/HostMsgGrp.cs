/// <summary>
/// 程式說明:DB Entity--參數@Lang.cms_detail檔
/// 維護部門:資訊管理處
/// 中國信託銀行 版權所有  ©  All Rights Reserved.
/// </summary>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using CTBC.FrameWork.Util;


namespace CTBC.CSFS.Models
{
    /// <summary>
    /// DB 實體類
    /// </summary>
    public class HostMsgGrp : Entity
    {
        /// <summary>
        /// 流水號
        /// </summary>
        public string grp_id { get; set; }
        /// <summary>
        /// 電文編號
        /// </summary>
        public string trans_id { get; set; }
        /// <summary>
        /// 交易代碼
        /// </summary>
        public string txcode { get; set; }
        /// <summary>
        /// 資料來源
        /// </summary>
        public string src_id { get; set; }
        /// <summary>
        /// 上/下行
        /// </summary>
        public string txkind { get; set; }
        /// <summary>
        /// 交易名稱
        /// </summary>
        public string txname { get; set; }
        /// <summary>
        /// 是否使用
        /// </summary>
        public string is_use { get; set; }
        /// <summary>
        /// 建立人員編號
        /// </summary>
        public string cCretMebrNo { get; set; }
        /// <summary>
        /// 建立日期
        /// </summary>
        public DateTime cCretDT { get; set; }
        /// <summary>
        /// 異動人員編號
        /// </summary>
        public string cMantMebrNo { get; set; }
        /// <summary>
        /// 異動日期
        /// </summary>
        public DateTime cMantDT { get; set; }
        /// <summary>
        /// 總筆數
        /// </summary>
        public int detl_id { get; set; }
        public string edata { get; set; }
        public string cdata { get; set; }
        public string dataorder { get; set; }
        public string datatype { get; set; }
        public string src_field { get; set; }
        public string dest_table { get; set; }
        public string dest_column { get; set; }
        public string datalength { get; set; }
        public int maxnum { get; set; }
        public string prod_id { get; set; }
        public string ProductTypes { get; set; }
        public string Cis_use { get; set; }
        public string Ctxkind { get; set; }
        /// <summary>
        /// 業務別
        /// </summary>
        public string BUSINESS_TYPE { get; set; }
        /// <summary>
        /// 產品別
        /// </summary>
        public string PRODUCT_TYPE { get; set; }
    }
}
