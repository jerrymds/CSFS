using System;
using Microsoft.VisualBasic;

namespace CTBC.FrameWork.Util
{
    public class UtilMathematics
    {
        /// <summary>
        /// 計算PMT
        /// </summary>
        /// <param name="decRate">利率</param>
        /// <param name="Num">期數</param>
        /// <param name="decRMoney">本金</param>
        /// <returns>回傳年支出</returns>
        public decimal getPMT(decimal decRate, int Num, decimal decRMoney)
        {
            try
            {
                double Rate = Convert.ToDouble(decRate);
                double RMoney = Convert.ToDouble(decRMoney);

                double pmtAmount = Financial.Pmt((Rate / 100) / 12, Num, RMoney, 0, DueDate.EndOfPeriod) * (-12);

                decimal decPMTAmount = Convert.ToDecimal(pmtAmount);
                return decPMTAmount;
            }
            catch
            {
                return 0;
            }
        }
    }
}
