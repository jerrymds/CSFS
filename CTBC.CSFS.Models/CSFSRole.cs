/// <summary>
/// 程式說明：CSFS Role 物件
/// 維護部門:資訊管理處
/// 中國信託銀行 版權所有  ©  All Rights Reserved.
/// </summary>


namespace CTBC.CSFS.Models
{
    public class CSFSRole : Entity
    {
        public string RoleID { get; set; }
        public string RoleName { get; set; }
        public string RoleGroupID { get; set; }
        public string RoleDesc { get; set; }

        /// <summary>
        /// 是否已授權給某個menu Y=是/N=否
        /// </summary>
        public string Checked { get; set; }
    }
}