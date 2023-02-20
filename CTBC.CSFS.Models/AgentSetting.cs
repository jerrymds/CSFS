using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTBC.CSFS.Models
{
   public  class AgentSetting
    {
       public int SettingId { get; set; }
        /// <summary>
        /// 扣押是否啟用自動派件
        /// </summary>
       public bool IsAutoDispatch { get; set; }
       /// <summary>
       /// 外來文是否啟用自動派件
       /// </summary>
       public bool IsAutoDispatchFS { get; set; }
        /// <summary>
        /// 是否為扣押經辦
        /// </summary>
       public bool IsSeizure { get; set; }
       /// <summary>
       /// 是否為外來文經辦
       /// </summary>
       public bool IsCase { get; set; }
       public string Department { get; set; }
       public string EmpId { get; set; }
       /// <summary>
       /// AgentSetting(EmpId)
       /// </summary>
       public string AsempId { get; set; }
       /// <summary>
       /// AgentSetting(AsSname)
       /// </summary>
       public string AsSname { get; set; }
       //20170421 固定 RQ-2015-019666-017 派件至跨單位 宏祥 add start
       public string EmpName { get; set; }
       public string DeptId { get; set; }
       public string SectionName { get; set; }
       public string DepartmentID { get; set; }
       public string EmpIdAndName { get; set; }
       //20170421 固定 RQ-2015-019666-017 派件至跨單位 宏祥 add end
    }
}
