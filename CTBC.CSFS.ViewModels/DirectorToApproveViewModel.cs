using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CTBC.CSFS.Models;

namespace CTBC.CSFS.ViewModels
{
   public class DirectorToApproveViewModel
    {
        public DirectorToApprove DirectorToApprove { get; set; }
        public IList<DirectorToApprove> DirectorToApprovelist { get; set; }
    }
}
