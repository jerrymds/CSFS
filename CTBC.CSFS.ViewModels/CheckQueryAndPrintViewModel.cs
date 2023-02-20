using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CTBC.CSFS.Models;

namespace CTBC.CSFS.ViewModels
{
   public class CheckQueryAndPrintViewModel
    {
       public CheckQueryAndPrint CheckQueryAndPrint { get; set; }
       public IList<CheckQueryAndPrint> CheckQueryAndPrintlist { get; set; }
    }
}
