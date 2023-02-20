using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using CTBC.CSFS.Models;

namespace CTBC.CSFS.ViewModels
{
   public  class AgentDepartmentAccessViewModel
    {

        // 介面顯示
       public AgentDepartmentAccess AgentDeptAccess { get; set; }

        // 清單顯示
       public IList<AgentDepartmentAccess> AgentDeptAccessList { get; set; }
    }
}
