using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CTBC.CSFS.Models;

namespace CTBC.CSFS.ViewModels
{
    public class WarningImportQueryDataViewModel
    {
        public WarningImportQueryData WarningImportQueryData { get; set; }
        public IList<WarningImportQueryData> WarningImportQueryDatalist { get; set; }
    }
}
