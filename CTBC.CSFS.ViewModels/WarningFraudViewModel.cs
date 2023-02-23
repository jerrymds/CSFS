using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CTBC.CSFS.Models;

namespace CTBC.CSFS.ViewModels
{
    public class WarningFraudViewModel : Entity
    {
        public WarningFraud WarningFraud { get; set; }

        public IList<WarningFraud> WarningFraudList { get; set; }
    }
}
