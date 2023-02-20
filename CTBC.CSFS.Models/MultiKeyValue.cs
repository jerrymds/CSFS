using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTBC.CSFS.Models
{
    public class MultiKeyValue
    {
        public Guid Caseid { get; set; }
        public byte[] Content { get; set; }
    }
}
