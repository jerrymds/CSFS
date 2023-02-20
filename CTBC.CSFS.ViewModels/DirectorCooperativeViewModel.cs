using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CTBC.CSFS.Models;

namespace CTBC.CSFS.ViewModels
{
   public class DirectorCooperativeViewModel
    {
       public DirectorCooperative DirectorCooperative { get; set; }
       public IList<DirectorCooperative> DirectorCooperativelist { get; set; }
    }
}
