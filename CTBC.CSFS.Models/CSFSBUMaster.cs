namespace CTBC.CSFS.Models
{
    public class CSFSBUMaster : Entity
    {

        /// <summary>
        /// 序號
        /// </summary>
        public int RowNum { get; set; }

        /// <summary>
        /// BUMaster ID
        /// </summary>
        public string BUMasterID { get; set; }

        /// <summary>
        /// BUMaster名稱
        /// </summary>
        public string BUMasterName { get; set; }

        /// <summary>
        /// BUMaster描述
        /// </summary>
        public string BUMasterDesc { get; set; }

        /// <summary>
        /// 統一編號
        /// </summary>
        public string CompanyID { get; set; }

        /// <summary>
        /// 公司代碼
        /// </summary>
        public string CompanyNum { get; set; }

        /// <summary>
        /// 公司中文簡稱
        /// </summary>
        public string CompanyNameCs { get; set; }

        /// <summary>
        /// 公司中文名稱
        /// </summary>
        public string CompanyNameCl { get; set; }

        /// <summary>
        /// 公司英文名稱
        /// </summary>
        public string CompanyNameEl { get; set; }

        /// <summary>
        /// 公司英文簡稱
        /// </summary>
        public string CompanyNameEs { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>add by mel 20130909 bu維護介面</remarks>
        public string BUMasterDesc2 { get; set; }


    }
}