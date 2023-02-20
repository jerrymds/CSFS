/// <summary>
/// 程式說明：員工主檔
/// 維護部門:資訊管理處
/// 中國信託銀行 版權所有  ©  All Rights Reserved.
/// </summary>

namespace CTBC.CSFS.Models
{
    public class CSFSEmployee : Entity
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

        //20130726 Tom 新增電謄所需欄位
        /// <summary>
        /// 電謄科別
        /// </summary>
        public string LandDept { get; set; }

        /// <summary>
        /// 電謄組別
        /// </summary>
        public string LandGroup { get; set; }

        /// <summary>
        /// 電謄角色
        /// </summary>
        public string LandRole { get; set; }

        /// <summary>
        /// 層級
        /// </summary>
        public string CreditLevel { get; set; }

        /// <summary>
        /// 層級順序
        /// </summary>
        public int Seq { get; set; }

        //20140205 Tom 新增序號
        /// <summary>
        /// 序號
        /// </summary>
        public int RowNum { get; set; }

        //20140205 Tom 新增員工部門
        /// <summary>
        /// 員工部門
        /// </summary>
        public string EmpDept { get; set; }

        //20140205 Tom 新增總筆數
        /// <summary>
        /// 總筆數
        /// </summary>
        public int maxnum { get; set; }
    }
}
