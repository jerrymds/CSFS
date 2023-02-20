using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CTBC.CSFS.Models;

namespace CTBC.CSFS.ViewModels
{
   public class CaseReturnViewModel
    {
       public CaseReturn CaseReturn { get; set; }

        // 清單顯示
       public IList<CaseReturn> CaseReturnList { get; set; }
    }
}
