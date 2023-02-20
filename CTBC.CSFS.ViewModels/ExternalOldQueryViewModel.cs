using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CTBC.CSFS.Models;

namespace CTBC.CSFS.ViewModels
{
	public class ExternalOldQueryViewModel
    {
		public ExternalOldQuery ExternalOldQuery { get; set; }
		public IList<ExternalOldQuery> ExternalOldQueryList { get; set; }
		//明細
		public List<ExternalOldDetails> ExternalOldDetailsList { get; set; }
    }
}
