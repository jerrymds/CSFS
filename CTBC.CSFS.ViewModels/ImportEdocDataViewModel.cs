using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CTBC.CSFS.Models;

namespace CTBC.CSFS.ViewModels
{
    public class ImportEdocDataViewModel
    {
        public ImportEdocData ImportEdocData { get; set; }
        public IList<ImportEdocData> ImportEdocDatalist { get; set; }
    }
}
