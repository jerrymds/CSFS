using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTBC.CSFS.BussinessLogic
{
    class CaseDeadCommonBIZ : CommonBIZ
    {
        // 工作日
        private DataTable dtWorkingDay = new DataTable();

        /// <summary>
        /// 指定日期加指定的天數
        /// </summary>
        /// <param name="pDate">指定日期</param>
        /// <param name="Days">指定的天數</param>
        /// <returns></returns>
        public string DateAdd(string pDate, int Days)
        {
            // 要返回的日期
            string returnDate = "";

            if (!string.IsNullOrEmpty(pDate))
            {
                dtWorkingDay = dtWorkingDay != null && dtWorkingDay.Rows.Count > 0 ? dtWorkingDay : GetWorkingDay();

                DataRow[] dr = dtWorkingDay.Select("Date>'" + pDate + "' and Flag='true'");
                if (dr != null && dr.Length > 0)
                {
                    // 符合條件的筆數
                    int pDateCount = 0;
                    for (int j = 0; j < dr.Length; j++)
                    {
                        // 如果時間不等於pDate且爲工作日，符合條件筆數+1
                        if (dr[j]["Date"].ToString() != pDate)
                        {
                            pDateCount++;
                        }

                        if (pDateCount == Days)
                        {
                            returnDate = dr[j]["Date"].ToString();
                            break;
                        }
                    }
                }
            }
            if (returnDate != "" && Days == 5)
            {
                // T+5與系統時間比較，T+5<系統時間:returnDate="1"
                returnDate = DateTime.Compare(Convert.ToDateTime(returnDate), Convert.ToDateTime(DateTime.Now.ToString("yyyy/MM/dd"))) > 0 ? "" : "1";
            }
            return returnDate;
        }

        /// <summary>
        /// 查詢工作日
        /// </summary>
        /// <returns></returns>
        public DataTable GetWorkingDay()
        {
            try
            {
                string sql = @"
                            select 
                                convert(nvarchar(10),Date,111) as Date
                                ,Flag
                            from PARMWorkingDay 
                            order by Date asc ";

                return base.Search(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
