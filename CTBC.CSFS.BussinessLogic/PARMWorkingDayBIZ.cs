/// <summary>
/// 程式說明：維護PARMWorkingDay - 營業日
/// 維護部門:資訊管理處
/// CSFS Version:v3.0
/// 中國信託銀行 版權所有  ©  All Rights Reserved.
/// </summary>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Text;
using System.Data.SqlClient;
using CTBC.CSFS.BussinessLogic;
using CTBC.FrameWork.Platform;
using CTBC.CSFS.Pattern;

namespace CTBC.CSFS.Models
{
    public class PARMWorkingDayBIZ:CommonBIZ
    {
        public PARMWorkingDayBIZ()
        {}

        /// <summary>
        /// 自DB查詢每月營業日資訊
        /// </summary>
        /// <param name="dtime"></param>
        /// <returns></returns>
        public IEnumerable<PARMWorkingDay> SelectByMonth(DateTime dtime)
        {
            try
            {
                IList<PARMWorkingDay> list = new List<PARMWorkingDay>();
                string sql = @"select Date,Flag from PARMWorkingDay where Year(Date)=@Year and Month(Date)=@Month ";
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@Year", dtime.Year));
                base.Parameter.Add(new CommandParameter("@Month", dtime.Month));
                return base.SearchList<PARMWorkingDay>(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 儲存營業日設定資料to DB
        /// </summary>
        /// <param name="nonWKDay"></param>
        /// <param name="currDte"></param>
        /// <returns></returns>
        public int Save(string[] nonWKDay,DateTime currDte)
        {
            try
            {
                DateTime fd = new DateTime();
                if (nonWKDay == null)
                    fd = currDte;
                else
                    fd = Convert.ToDateTime(nonWKDay[0]);
                string sql = @"delete PARMWorkingDay where Year(Date)=@Year and Month(Date)=@Month; ";
                sql += ComposeSql(nonWKDay, fd);
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@Year", fd.Year));
                base.Parameter.Add(new CommandParameter("@Month", fd.Month));
                base.Parameter.Add(new CommandParameter("@User", Account));
                return base.ExecuteNonQuery(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 組合每個月每一天是營業日 or 非營業日,準備insert 到DB
        /// </summary>
        /// <param name="nonWKDay">非營業日</param>
        /// <param name="dte">所選擇月份日期</param>
        /// <returns></returns>
        private string ComposeSql(string[] nonWKDay, DateTime dte)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                //每月總天數
                int days = DateTime.DaysInMonth(dte.Year, dte.Month);

                //每月一日
                DateTime tmpDte = new DateTime(dte.Year, dte.Month, 1);

                //是否為營業日(true/false)
                string isWK="";

                if (nonWKDay == null)
                {
                    //該日為本週星期幾(0=星期日,6=星期六)
                    int dOfw = (int)tmpDte.DayOfWeek;
                     
                    for (int i = 0; i < days; i++)
                    {
                        //IR-61354 預設預設星期六和星期日的日期要打勾(非營業日)
                        if (dOfw == 0 || dOfw == 6) isWK = "false";
                        else isWK = "true";
                        sb.Append(@" insert into PARMWorkingDay([Date],[Flag],CreatedUser,ModifiedUser) values('" + tmpDte.ToString("yyyy-MM-dd") + "','" + isWK + "',@User,@User);");
                        tmpDte = tmpDte.AddDays(1);
                        dOfw = (int)tmpDte.DayOfWeek;
                    }
                }
                else
                {

                    //先轉會為日期格式陣列
                    DateTime[] aryDte = new DateTime[nonWKDay.Count()];
                    for (int j = 0; j < nonWKDay.Count(); j++)
                    {
                        aryDte[j] = Convert.ToDateTime(nonWKDay[j]);
                    }
                    bool isFind = false;
                    isWK = "true";
                    string insDte = "";
                    for (int i = 0; i < days; i++)
                    {
                        isFind = false;
                        for (int k = 0; k < aryDte.Count(); k++)
                        {
                            //如果user勾選為假日
                            if (aryDte[k] == tmpDte.Date)
                            {
                                isFind = true;
                                insDte = nonWKDay[k];
                                isWK = "false";
                                break;
                            }
                        }
                        if (!isFind)
                        {
                            //其餘為營業日
                            insDte = tmpDte.ToString("yyyy-MM-dd");
                            isWK = "true";
                        }
                        sb.Append(@" insert into PARMWorkingDay([Date],[Flag],CreatedUser,ModifiedUser) values('" + insDte + "','" + isWK + "',@User,@User);");
                        tmpDte = tmpDte.AddDays(1);
                    }
                }
                return sb.ToString();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 排列好每月日曆的viewModel
        /// </summary>
        /// <param name="list">營業日資訊</param>
        /// <param name="dte">所選擇月份日期</param>
        /// <returns></returns>
        public PARMWorkingDayVO FormatCalendar(IEnumerable<PARMWorkingDay> list,DateTime dte)
        {
            try
            {
                List<PARMWorkingDay> flist = new List<PARMWorkingDay>();
                if (list != null)
                {
                    if (list.Count() > 0)
                    {
                        var fday = (PARMWorkingDay)list.First();
                        dte = fday.Date;
                    }
                }

                //每月總天數
                int days = DateTime.DaysInMonth(dte.Year, dte.Month);

                //每月一日
                DateTime tmpDte = new DateTime(dte.Year, dte.Month, 1);
                System.Globalization.DateTimeFormatInfo cinfo = System.Globalization.DateTimeFormatInfo.CurrentInfo;

                //每月1日前之上個月日期(1日前之空白日期總數)
                int aa = (int)cinfo.FirstDayOfWeek;
                int emptyCells = ((int)tmpDte.DayOfWeek + 7 - (int)cinfo.FirstDayOfWeek) % 7;
                int i = 0; int j = 0;

                //每頁月曆上總天數
                int tot = ((days + emptyCells) > 35 ? 42 : 35);

                //每月1日前之上個月日期
                for (j = 0; j < emptyCells; j++)
                {
                    flist.Add(null);
                }

                //開始讀取本月每日資料-狀態:資料庫尚未設定
                if (list == null)
                {
                    int dayOfweek = 0;
                    for (i = j; i < days; i++)
                    {
                        dayOfweek = (int)tmpDte.DayOfWeek;
                        if (dayOfweek == 0 || dayOfweek == 6)                        
                            flist.Add(new PARMWorkingDay { Date = tmpDte, Flag = false });//預設星期日與星期六皆為非營業日                        
                        else
                            flist.Add(new PARMWorkingDay { Date = tmpDte, Flag = true });                        
                        tmpDte = tmpDte.AddDays(1);
                    }
                }
                else
                {
                    //開始讀取本月每日資料-狀態:本月資料庫至只少設定一天
                    bool isFind = false;
                    for (i = 0; i < days; i++)
                    {
                        isFind = false;
                        foreach (var item in list)
                        {
                            if (item.Date == tmpDte.Date)
                            {
                                isFind = true;
                                flist.Add(item);
                                break;
                            }
                        }
                        if (!isFind)
                        {
                            flist.Add(new PARMWorkingDay { Date = tmpDte, Flag = true });
                        }
                        tmpDte = tmpDte.AddDays(1);
                    }
                }

                //每月底後日曆上之下個月日期
                if (i < tot)
                {
                    for (int k = i; k < tot; k++)
                    {
                        flist.Add(null);
                    }
                }

                //組合最後ViewModel
                PARMWorkingDayVO model = new PARMWorkingDayVO
                {
                    WKList = flist,
                    OneCalendarTotDay = tot,
                    CurrentDate = new DateTime(dte.Year, dte.Month, 1)
                };
                return model;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public DataTable  GetWorkDate()
        {
            string strSql = @"SELECT CONVERT(nvarchar(20), [Date],111) as workdate
                                      FROM PARMWorkingDay
                                      where flag = 1 and [date] >= CONVERT(nvarchar(20), GETDATE(),23)";

            DataTable dt = base.Search(strSql);
            if (dt != null & dt.Rows.Count > 0)
            {
                return dt;
            }
            else
            {
                return new DataTable();
            }
        }
    }
}
