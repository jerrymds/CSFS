using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CTBC.CSFS.Models;

namespace CTBC.CSFS.ViewModels
{
    public class SeizureQueryViewModel
    {
        public SeizureQuery SeizureQuery { get; set; }
        public IList<SeizureQuery> SeizureQueryList { get; set; }
    }
}
