using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTBC.CSFS.Models
{
   public class CollectionToAgent
    {
       public Guid CaseId { get; set; }
       public int num { get; set; }
       public string CaseNo { get; set; }
       public string GovUnit { get; set; }
       public string AgentUser { get; set; }
       public string GovDate { get; set; }
       public string LimitDate { get; set; }
       public string GovNo { get; set; }
       public string CaseKind { get; set; }
       public string CaseKind2 { get; set; }
       public string Speed { get; set; }
       public string Unit { get; set; }
       public string CreatedUser { get; set; }
       public string SortExpression { get; set; }
       public string SortDirection { get; set; }
       public int TotalItemCount { get; set; }
       public int PageSize { get; set; }
       public int CurrentPage { get; set; }
       public int maxnum { get; set; }
       public string GovKind { get; set; }

       public string CreatedDateS { get; set; }
       public string CreatedDateE { get; set; }

       public string GovDateS { get; set; }
       public string GovDateE { get; set; }
       public string ReceiveKind { get;set;}
       public string OverDueMemo { get; set; }

       //20170421 固定 RQ-2015-019666-017 派件至跨單位 宏祥 add start
       public string AgentDepartment { get; set; }
       public string AgentDepartment2 { get; set; }
       public string AgentDepartmentUser { get; set; }       
       //20170421 固定 RQ-2015-019666-017 派件至跨單位 宏祥 add end
    }
}
