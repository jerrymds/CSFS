using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTBC.CSFS.Models
{
	public class SeizureOldDetails2
	{
		/// 義(債)務人戶名
		/// </summary>
		public string ObligorAccountName { get; set; }
		/// <summary>
		/// 義(債)務人統編
		/// </summary>
		public string ObligorCompanyId { get; set; }
		/// <summary>
		/// 存款分行
		/// </summary>
		public string BranchName { get; set; }
		/// <summary>
		/// 扣押金額
		/// </summary>
		public string SeizureSum { get; set; }
		/// <summary>
		/// 發文備註
		/// </summary>
		public string SendRemark { get; set; }
		/// <summary>
		/// 發文字號
		/// </summary>
		public string SendSeq { get; set; }
		public int maxnum { get; set; }
    }
}
