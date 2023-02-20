using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTBC.CSFS.Models
{
    public class AgentDepartmentAccess
    {
        public string CodeMemo { get; set; }
        public string EmpName { get; set; }
        public int AccessId { get; set; }
        public Guid CaseId { get;set;}
        public string AccessData { get; set; }
        public string CreatedUser { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedUser { get; set; }
        public DateTime ModifiedDate { get; set; }
    }
}
