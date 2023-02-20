using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CTBC.CSFS.Models;

namespace CTBC.CSFS.ViewModels
{
    public class SeizureOldQueryViewModel
    {
        public SeizureOldQuery SeizureOldQuery { get; set; }
        public IList<SeizureOldQuery> SeizureOldQueryList { get; set; }
		//類型List
		public List<SeizureOldDetails1> SeizureOldDetails1List { get; set; }
		//類型List1
		public List<SeizureOldDetails1> SeizureOldDetails1_1List { get; set; }
		//扣押及撤銷List1
		public List<SeizureOldDetails2> SeizureOldDetails2_1List { get; set; }
		//扣押及撤銷List2
		public List<SeizureOldDetails2> SeizureOldDetails2_2List { get; set; }
		//支付List
		public List<SeizureOldDetails3> SeizureOldDetails3List { get; set; }
    }
}
