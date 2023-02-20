using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTBC.CSFS.LDAPDataImport
{
    public class LDAPEmployee
    {
        /// <summary>
        /// 員編
        /// </summary>
        public string EmpID { get; set; }

        /// <summary>
        /// 員工姓名
        /// </summary>
        public string EmpName { get; set; }

        /// <summary>
        /// 職稱(Title)
        /// </summary>
        public string EmpTitle { get; set; }

        /// <summary>
        /// 業務別(BusinessCategory)
        /// </summary>
        public string EmpBusinessCategory { get; set; }

        /// <summary>
        /// 是否為主管(身分別/ctcbEmployeeRole有值)
        /// </summary>
        public string IsManager { get; set; }

        /// <summary>
        /// eMail
        /// </summary>
        public string EMail { get; set; }

        /// <summary>
        /// 部門DN
        /// </summary>
        public string DepDN { get; set; }

        /// <summary>
        /// 部門代號
        /// </summary>
        public string DepID { get; set; }

        /// <summary>
        /// 主管員編
        /// </summary>
        public string ManagerID { get; set; }

        /// <summary>
        /// 員工公司電話
        /// </summary>
        public string TelNo { get; set; }

        /// <summary>
        /// 員工公司分機
        /// </summary>
        public string TelExt { get; set; }

        /// <summary>
        /// 建立者
        /// </summary>
        public string CreatedUser { get; set; }

        /// <summary>
        /// 建立日
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// 修改者
        /// </summary>
        public string ModifiedUser { get; set; }

        /// <summary>
        /// 修改日
        /// </summary>
        public DateTime ModifiedDate { get; set; }
    }
}
