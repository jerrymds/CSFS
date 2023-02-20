/// <summary>
/// 程式說明：虛擬組織@Lang.CSFS_detail檔
/// </summary>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using CTBC.FrameWork.Util;

namespace CTBC.CSFS.Models
{
    public class CSFSBU : Entity
    {
        # region 案件轉派功能
        /// <summary>
        /// 序號
        /// </summary>
        public int RowNum { get; set; }

        /// <summary>
        /// 部門編號
        /// </summary>
        public int BUID { get; set; }

        /// <summary>
        /// 應用系統代號
        /// </summary>
        public string APPID { get; set; }

        /// <summary>
        /// 虛擬組織主檔代號
        /// </summary>
        public string BUMasterID { get; set; }

        /// <summary>
        /// 自訂部門編號
        /// </summary>
        [Required]
        [Display(Name = "parm_bunumber")]
        public string BUNumber { get; set; }

        /// <summary>
        /// 部門名稱
        /// </summary>
        [Required]
        [Display(Name = "parm_buname")]
        public string BUName { get; set; }

        /// <summary>
        /// 上層部門序號
        /// </summary>
        public string BUParent { get; set; }

        /// <summary>
        /// 上層部門名稱
        /// </summary>
        public string BUParentName { get; set; }

        /// <summary>
        /// 是否有下屬部門
        /// </summary>
        [Required]
        [Display(Name = "parm_node")]
        public bool Node { get; set; }

        /// <summary>
        /// 部門顯示順序
        /// </summary>
        /// 20130225 hroace改成int
        [Required]
        [Display(Name = "parm_sort")]
        public int Sort { get; set; }

        /// <summary>
        /// 組別代碼
        /// </summary>
        public string BUDivType { get; set; }

        /// <summary>
        /// 部門地址
        /// </summary>
        public string BUAddress { get; set; }

        /// <summary>
        /// 部門郵遞區號
        /// </summary>
        public string BUZip { get; set; }

        /// <summary>
        /// 部門電話區碼
        /// </summary>
        public string BUTelArea { get; set; }

        /// <summary>
        /// 部門電話號碼
        /// </summary>
        public string BUTelNo { get; set; }

        /// <summary>
        /// 部門傳真區碼
        /// </summary>
        public string BUFaxArea { get; set; }

        /// <summary>
        /// 部門傳真號碼
        /// </summary>
        public string BUtFaxNo { get; set; }

        /// <summary>
        /// 部門傳真號碼聯絡人
        /// </summary>
        public string BUCaller { get; set; }

        /// <summary>
        /// 部門主管
        /// </summary>
        public string BUBoss { get; set; }

        /// <summary>
        /// 部門主管姓名
        /// </summary>
        /// 20130513 horace
        public string BUBossName { get; set; }

        /// <summary>
        /// 管理員角色
        /// </summary>
        /// 20130225 hroace改成string
        public string RoleID { get; set; }

        /// <summary>
        /// 審核主管群組
        /// </summary>
        public string ApproveGroupID { get; set; }

        /// <summary>
        /// 啟用狀態
        /// </summary>
        [Required]
        public bool Enable { get; set; }

        #endregion

        /// <summary>
        /// 統一編號
        /// </summary>
        public string CompanyID { get; set; }

        /// <summary>
        /// 公司代碼
        /// </summary>
        [Required]
        [Display(Name = "recv_companynum")]
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
        [Display(Name = "recv_companynameel")]
        [Letter]
        public string CompanyNameEl { get; set; }

        /// <summary>
        /// 公司英文簡稱
        /// </summary>
        [Display(Name = "recv_companynamees")]
        [Letter]
        public string CompanyNameEs { get; set; }

        /// <summary>
        /// 負責人-姓名
        /// </summary>
        public string OwnerName { get; set; }

        /// <summary>
        /// 負責人-職稱
        /// </summary>
        public string OwnerTitle { get; set; }

        /// <summary>
        /// 負責人-電話
        /// </summary>
        public string OwnerTEL { get; set; }

        /// <summary>
        /// 負責人-傳真
        /// </summary>
        public string OwnerFAX { get; set; }

        /// <summary>
        /// 聯絡人1-姓名
        /// </summary>
        public string Contact1Name { get; set; }

        /// <summary>
        /// 聯絡人1-職稱
        /// </summary>
        public string Contact1Title { get; set; }

        /// <summary>
        /// 聯絡人1-電話1
        /// </summary>
        public string Contact1TEL1 { get; set; }

        /// <summary>
        /// 聯絡人1-分機1
        /// </summary>
        public string Contact1EXT1 { get; set; }

        /// <summary>
        /// 聯絡人1-電話2
        /// </summary>
        public string Contact1TEL2 { get; set; }

        /// <summary>
        /// 聯絡人1-分機2
        /// </summary>
        public string Contact1EXT2 { get; set; }

        /// <summary>
        /// 聯絡人1-手機
        /// </summary>
        public string Contact1Mobile { get; set; }

        /// <summary>
        /// 聯絡人1-傳真
        /// </summary>
        public string Contact1FAX { get; set; }

        /// <summary>
        /// 聯絡人1-E-mail
        /// </summary>
        [Display(Name = "recv_contact1email")]
        [Email]
        public string Contact1Email { get; set; }

        /// <summary>
        /// 聯絡人2-姓名
        /// </summary>
        public string Contact2Name { get; set; }

        /// <summary>
        /// 聯絡人2-職稱
        /// </summary>
        public string Contact2Title { get; set; }

        /// <summary>
        /// 聯絡人2-電話1
        /// </summary>
        public string Contact2TEL1 { get; set; }

        /// <summary>
        /// 聯絡人2-分機1
        /// </summary>
        public string Contact2EXT1 { get; set; }

        /// <summary>
        /// 聯絡人2-電話2
        /// </summary>
        public string Contact2TEL2 { get; set; }

        /// <summary>
        /// 聯絡人2-分機2
        /// </summary>
        public string Contact2EXT2 { get; set; }

        /// <summary>
        /// 聯絡人2-手機
        /// </summary>
        public string Contact2Mobile { get; set; }

        /// <summary>
        /// 聯絡人2-傳真
        /// </summary>
        public string Contact2FAX { get; set; }

        /// <summary>
        /// 聯絡人2-E-mail
        /// </summary>
        [Display(Name = "recv_contact2email")]
        [Email]
        public string Contact2Email { get; set; }

        /// <summary>
        /// 聯絡地址-郵區
        /// </summary>
        public string ContactZip { get; set; }

        /// <summary>
        /// 聯絡地址-地址
        /// </summary>
        public string ContactAddress { get; set; }

        /// <summary>
        /// 合約郵區
        /// </summary>
        public string ContractZip { get; set; }

        /// <summary>
        /// 合約地址
        /// </summary>
        public string ContractAddress { get; set; }

        //bunumber +buname 
        public string BuNumberName { get; set; }

        //推廣位單位代碼
        public string PromotionUnit { get; set; }
    }
}