/// <summary>
/// 程式說明：PARMWorkingDay View Object
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
    public class PARMWorkingDayVO
    {
        public IEnumerable<PARMWorkingDay> WKList { get; set; }
        public int OneCalendarTotDay { get; set; }
        public DateTime CurrentDate { get; set; }
        public string[] CkBox { get; set; }
    }
}
