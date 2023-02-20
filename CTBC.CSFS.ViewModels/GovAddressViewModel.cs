using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CTBC.CSFS.Models;

namespace CTBC.CSFS.ViewModels
{
    public class GovAddressViewModel
    {
        public GovAddress GovAddress { get; set; }

        public IList<GovAddress> GovAddressList { get; set; }
    }
}
