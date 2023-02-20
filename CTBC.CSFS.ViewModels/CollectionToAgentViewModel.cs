using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CTBC.CSFS.Models;

namespace CTBC.CSFS.ViewModels
{
   public class CollectionToAgentViewModel
    {
       public CollectionToAgent CollectionToAgent { get; set; }
       public IList<CollectionToAgent> CollectionToAgentList { get; set; }
    }
}
