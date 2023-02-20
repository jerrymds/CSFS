/// <summary>
/// 程式說明：PARMWorkingDay - 營業日物件
/// 維護部門:資訊管理處
/// CSFS Version:v3.0
/// 中國信託銀行 版權所有  ©  All Rights Reserved.
/// </summary>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CTBC.CSFS.Models
{
    public class PARMWorkingDay
    {
        public DateTime Date { get; set; }
        public bool Flag { get; set; }
    }
}
