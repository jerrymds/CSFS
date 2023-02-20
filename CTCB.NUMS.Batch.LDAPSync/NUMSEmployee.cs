using System;

namespace CTCB.NUMS.Batch.LDAPSync
{
    public class NUMSEmployee
    {
        /// <summary>
        /// 序號
        /// </summary>
        public int EmpUid { get; set; }

        /// <summary>
        /// 員工員編
        /// </summary>
        public string EmpID { get; set; }

        /// <summary>
        /// 員工姓名
        /// </summary>
        public string EmpName { get; set; }

        /// <summary>
        /// 員工描述
        /// </summary>
        public string EmDesc { get; set; }

        /// <summary>
        /// 員工郵件帳號
        /// </summary>
        public string EmpEMail { get; set; }

        /// <summary>
        /// 員工職稱
        /// </summary>
        public string EmpTitle { get; set; }

        /// <summary>
        /// 員工職級
        /// </summary>
        public string EmpLevel { get; set; }

        /// <summary>
        /// 是否在職
        /// </summary>
        public bool OnBoard { get; set; }

        /// <summary>
        /// 員工姓名
        /// </summary>
        public string EmpDesc { get; set; }

        /// <summary>
        /// 員工所屬角色 in NUMS
        /// </summary>
        public string EmpGroups { get; set; }
    }
}
