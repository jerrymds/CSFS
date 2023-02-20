using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTBC.CSFS.Models
{
	public class SeizureOldDetails3
	{
		/// 受款人
		/// </summary>
		public string MoneyReceiverPay { get; set; }
		/// <summary>
		/// 本行支票號碼
		/// </summary>
		public string CBChequeNumPay { get; set; }
		/// <summary>
		/// 收取金額
		/// </summary>
		public string ChargeSumPay { get; set; }
		/// <summary>
		/// 受文者
		/// </summary>
		public string SendReceiver { get; set; }
		/// <summary>
		/// 地址
		/// </summary>
		public string ReceiverAddress { get; set; }
		/// <summary>
		/// 副本
		/// </summary>
		public string CCReceiver { get; set; }
		/// <summary>
		/// 地址
		/// </summary>
		public string CCAddress { get; set; }
		/// <summary>
		/// 說明
		/// </summary>
		public string SendRemark { get; set; }
		public int maxnum { get; set; }
    }
}
