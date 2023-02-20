/// <summary>
/// 程式說明:數據類型處理
/// </summary>

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Data;
using System.Web; //20140313

namespace CTBC.FrameWork.Util
{
    /// <summary>
    /// 資料型別轉換工具。
    /// </summary>
    public class DataTypeHelper
    {
        #region 公共變數
        private static readonly string[] abbrMonths =
            new string[12] { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };
        #endregion

        /// <summary>
        /// 轉換為UI層顯示的資料格式。
        /// </summary>
        /// <param name="sValue">欲顯示的資料。</param>
        /// <returns>轉換為 yyyy/mm/dd 的資料格式。</returns>
        public static string GetUIValue(DateTime sValue)
        {
            string rValue;

            if (sValue == DateTime.MinValue)
            {
                rValue = "";
            }
            else
            {
                rValue = ConvertToStringDate(sValue);
            }

            return rValue;
        }

        /// <summary>
        /// 轉換為UI層顯示的訊息格式。
        /// </summary>
        /// <param name="sValue">欲顯示的資料。</param>
        /// <returns>轉換為 yyyy/mm/dd 的資料格式。</returns>
        public static string GetUIValue(DateTime? sValue)
        {
            string rValue;

            if (sValue == null)
            {
                rValue = "";
            }
            else if (sValue == DateTime.MinValue)
            {
                rValue = "";
            }
            else
            {
                rValue = ConvertToStringDate(sValue);
            }

            return rValue;
        }

        /// <summary>
        /// 轉換為UI層顯示的訊息格式。20140218 smallzhi
        /// </summary>
        /// <param name="sValue">欲顯示的資料。</param>
        /// <returns>轉換為 yyyy/mm/dd 的資料格式。By longtime</returns>
        public static string GetUIValue(DateTime? sValue, bool longFormat)
        {
            string rValue;

            if (sValue == null)
            {
                rValue = "";
            }
            else if (sValue == DateTime.MinValue)
            {
                rValue = "";
            }
            else
            {
                if (longFormat)
                {
                    rValue = ConvertToStringDateTime(sValue);
                }
                else
                {
                    rValue = ConvertToStringDate(sValue);
                }
            }

            return rValue;
        }

        /// <summary>
        /// 轉換前端顯示資料, 若值為int.MinValue, 則回傳空白.
        /// </summary>
        /// <param name="sValue">待轉換資料。</param>
        /// <returns></returns>
        public static string GetUIValue(int sValue)
        {
            string rValue;

            if (sValue == int.MinValue)
            {
                rValue = "";
            }
            else
            {
                rValue = sValue.ToString();
            }

            return rValue;
        }

        /// <summary>
        /// 轉換前端顯示資料, 若值為int.MinValue, 則回傳空白.
        /// </summary>
        /// <param name="sValue">待轉換資料。</param>
        /// <returns></returns>
        public static string GetUIValue(int? sValue)
        {
            string rValue;

            if (sValue == int.MinValue)
            {
                rValue = "";
            }
            else
            {
                rValue = sValue.ToString();
            }

            return rValue;
        }

        /// <summary>
        /// 轉換前端顯示資料, 若值為int.MinValue, 則回傳空白.
        /// </summary>
        /// <param name="sValue">待轉換資料。</param>
        /// <returns></returns>
        public static string GetUIValue(decimal sValue)
        {
            string rValue;

            if (sValue == decimal.MinValue)
            {
                rValue = "";
            }
            else
            {
                rValue = sValue.ToString();
            }

            return rValue;
        }

        /// <summary>
        /// 轉換前端顯示資料, 若值為int.MinValue, 則回傳空白.
        /// </summary>
        /// <param name="sValue">待轉換資料。</param>
        /// <returns></returns>
        public static string GetUIValue(decimal? sValue)
        {
            string rValue;

            if (sValue == decimal.MinValue)
            {
                rValue = "";
            }
            else
            {
                rValue = sValue.ToString();
            }

            return rValue;
        }

        /// <summary>
        /// 轉換前端顯示資料, 若待轉換資料等於空值資料，則回傳空值結果。
        /// </summary>
        /// <param name="sValue">待轉換資料。</param>
        /// <param name="emptyString">空值結果。</param>
        /// <param name="emptyValue">空值資料。</param>
        /// <returns></returns>
        public static string GetUIValue(int sValue, int emptyValue, string emptyString)
        {
            string rValue = sValue == emptyValue ? emptyString : sValue.ToString();

            return rValue;
        }

        /// <summary>
        /// 轉換前端顯示資料, 若待轉換資料等於空值資料，則回傳空值結果。
        /// </summary>
        /// <param name="value">待轉換資料。</param>
        /// <param name="emptyString">空值結果。</param>
        /// <param name="emptyValue">空值資料。</param>
        /// <returns></returns>
        public static string GetUIValue(double value, double emptyValue, string emptyString)
        {
            string rValue = value == emptyValue ? emptyString : value.ToString();

            return rValue;
        }

        /// <summary>
        /// 轉換為UI層顯示的資料格式。
        /// </summary>
        /// <param name="value">欲轉換的資料。</param>
        /// <remarks>當資料為值等於，double.MinValue，則回傳空白。</remarks>
        /// <returns>轉換後的資料格式。</returns>
        public static string GetUIValue(double value)
        {
            string rValue;

            if (value == double.MinValue)
            {
                rValue = "";
            }
            else
            {
                rValue = value.ToString();
            }

            return rValue;
        }

        /// <summary>
        /// 轉換為UI層顯示的資料格式。
        /// </summary>
        /// <param name="value">欲轉換的資料。</param>
        /// <param name="longFormat">是否包含時間資料。</param>
        /// <remarks>longFormat = true，轉換格式為: yyyy/MM/dd HH:mm:ss。否則轉換格式為: yyyy/MM/dd。</remarks>
        /// <returns>轉換後的資料格式。</returns>
        public static string GetUIValue(DateTime value, bool longFormat)
        {
            string rValue;

            if (value == DateTime.MinValue)
            {
                rValue = "";
            }
            else
            {
                if (longFormat)
                {
                    rValue = ConvertToStringDateTime(value);
                }
                else
                {
                    rValue = ConvertToStringDate(value);
                }
            }

            return rValue;
        }

        /// <summary>
        /// 轉換前端顯示資料, 若值為0, 則回傳空白.
        /// </summary>
        /// <param name="sValue">待轉換資料。</param>
        /// <returns></returns>
        public static string GetUIValueZeroToEmpty(int sValue)
        {
            string rValue;

            if (sValue == 0)
            {
                rValue = "";
            }
            else
            {
                rValue = sValue.ToString();
            }

            return rValue;
        }

        /// <summary>
        /// 轉換前端顯示資料, 若值為0, 則回傳空白.
        /// </summary>
        /// <param name="sValue">待轉換資料。</param>
        /// <returns></returns>
        public static string GetUIValueZeroToEmpty(decimal sValue)
        {
            string rValue;

            if (sValue == 0)
            {
                rValue = "";
            }
            else
            {
                rValue = sValue.ToString();
            }

            return rValue;
        }

        /// <summary>
        /// 將字符串類型轉換為Guid。
        /// </summary>
        /// <param name="uID">待轉換資料。</param>
        /// <returns></returns>
        public static Guid ConvertToGuid(string uID)
        {
            Guid guid;

            if (string.IsNullOrEmpty(uID))
            {
                guid = Guid.NewGuid();
            }
            else
            {
                guid = new Guid(uID);
            }
            return guid;
        }

        /// <summary>
        /// 將字符串類型轉換為Guid。
        /// </summary>
        /// <param name="uID">待轉換資料。</param>
        /// <returns></returns>
        public static Guid GetToGuid(string uID)
        {
            Guid guid;

            if (string.IsNullOrEmpty(uID))
            {
                guid = Guid.Empty;
            }
            else
            {
                guid = new Guid(uID);
            }
            return guid;
        }

        /// <summary>
        /// 將日期轉換為 yyyy/MM/dd-HH:mm:ss 格式字串。
        /// </summary>
        /// <param name="dateTime">日期資料。</param>
        /// <returns></returns>
        public static string ConvertToStringDateTime(DateTime dateTime)
        {
            return dateTime.ToString("yyyy/MM/dd HH:mm:ss", DateTimeFormatInfo.InvariantInfo);
        }

        /// <summary>
        /// 將日期轉換為 yyyy/MM/dd HH:mm:ss 格式字串。
        /// </summary>
        /// <param name="dateTime">日期資料。</param>
        /// <returns></returns>
        public static string ConvertToStringDateTime(DateTime? dateTime)
        {
            string rValue;

            if (dateTime == null)
            {
                rValue = "";
            }
            else if (dateTime == DateTime.MinValue)
            {
                rValue = "";
            }
            else
            {
                DateTime value = (DateTime)dateTime;

                rValue = value.ToString("yyyy/MM/dd HH:mm:ss", DateTimeFormatInfo.InvariantInfo);
            }

            return rValue;
        }

        /// <summary>
        /// 將日期轉換為 yyyy/MM/dd 格式字串。
        /// </summary>
        /// <param name="date">日期資料。</param>
        /// <returns></returns>
        public static string ConvertToStringDate(DateTime date)
        {
            return date.ToString("yyyy/MM/dd", DateTimeFormatInfo.InvariantInfo);
        }

        /// <summary>
        /// 將日期轉換為 yyyy/MM/dd 格式字串。
        /// </summary>
        /// <param name="date">日期資料。</param>
        /// <returns></returns>
        public static string ConvertToStringDate(string date)
        {
            if (string.IsNullOrEmpty(date))
            {
                return date;
            }

            DateTime datetime;

            try
            {
                datetime = Convert.ToDateTime(date);
            }
            catch
            {
                return date;
            }

            return ConvertToStringDate(datetime);
        }

        /// <summary>
        /// 將日期轉換為 yyyy/MM/dd 格式字串。
        /// </summary>
        /// <param name="date">日期資料。</param>
        /// <returns></returns>
        public static string ConvertToStringDate(object date)
        {
            if (date == null || date.ToString().Length == 0)
            {
                return "";
            }

            string sdate = date.ToString();

            DateTime datetime;

            try
            {
                datetime = Convert.ToDateTime(date);
            }
            catch
            {
                return date.ToString();
            }

            return ConvertToStringDate(datetime);
        }

        /// <summary>
        /// 將資料轉換為DateTime格式，若轉換失敗，則回DateTime.MinValue。
        /// </summary>
        /// <param name="value">待轉換資料。</param>
        /// <returns></returns>
        public static DateTime GetBLDateTimeValue(string value)
        {
            DateTime rValue;

            try
            {
                rValue = Convert.ToDateTime(value);
            }
            catch
            {
                rValue = DateTime.MinValue;
            }

            return rValue;
        }

        /// <summary>
        /// 將資料轉換為DateTime格式，若轉換失敗，則回null。
        /// </summary>
        /// <param name="value">待轉換資料。</param>
        /// <returns></returns>
        public static DateTime? GetBLDateTimeNullableValue(string value)
        {
            DateTime? rValue;

            try
            {
                if (value != null)
                {
                    rValue = Convert.ToDateTime(value);
                }
                else
                {
                    rValue = null;
                }
            }
            catch
            {
                rValue = null;
            }

            return rValue;
        }

        /// <summary>
        /// 將資料轉換為int格式，若轉換失敗，則回double.MinValue。
        /// </summary>
        /// <param name="value">待轉換資料。</param>
        /// <returns></returns>
        public static double GetBLDoubleValue(string value)
        {
            double rValue;

            try
            {
                rValue = Convert.ToDouble(value);
            }
            catch
            {
                rValue = double.MinValue;
            }

            return rValue;
        }

        /// <summary>
        /// 將資料轉換為double格式，若轉換失敗，則回defaultValue設定值。
        /// </summary>
        /// <param name="value">待轉換資料。</param>
        /// <param name="defaultValue">轉換失敗的預設值。</param>
        /// <returns></returns>
        public static double GetBLDoubleValue(string value, double defaultValue)
        {
            double rValue;

            try
            {
                rValue = Convert.ToDouble(value);
            }
            catch
            {
                rValue = defaultValue;
            }

            return rValue;
        }

        /// <summary>
        /// 將資料轉換為Int64格式，若轉換失敗，則回defaultValue設定值。
        /// </summary>
        /// <param name="value">待轉換資料。</param>
        /// <param name="defaultValue">轉換失敗的預設值。</param>
        /// <returns></returns>
        public static Int64 GetBLLongValue(string value, Int64 defaultValue)
        {
            Int64 rValue;

            try
            {
                rValue = Convert.ToInt64(Convert.ToDouble(value).ToString("F0"));
            }
            catch
            {
                rValue = defaultValue;
            }

            return rValue;
        }

        /// <summary>
        /// 將資料轉換為int格式，若轉換失敗，則回int.MinValue。
        /// </summary>
        /// <param name="value">待轉換資料。</param>
        /// <returns></returns>
        public static int GetBLIntValue(string value)
        {
            int rValue;

            try
            {
                rValue = Convert.ToInt32(value);
            }
            catch
            {
                rValue = int.MinValue;
            }

            return rValue;
        }

        /// <summary>
        /// 將資料轉換為int格式，若轉換失敗，則回defaultValue設定值。
        /// </summary>
        /// <param name="value">待轉換資料。</param>
        /// <param name="defaultValue">轉換失敗的預設值。</param>
        /// <returns></returns>
        public static int GetBLIntValue(string value, int defaultValue)
        {
            int rValue;

            try
            {
                rValue = Convert.ToInt32(value);
            }
            catch
            {
                rValue = defaultValue;
            }

            return rValue;
        }

        /// <summary>
        /// 將資料轉換為資料庫儲存對應值，若待轉換資料等於double.MinValue，則回傳DBNull.Value。
        /// </summary>
        /// <param name="value">待轉換資料。</param>
        /// <returns></returns>
        public static object GetDBValue(double value)
        {
            object rValue;

            if (value == double.MinValue)
            {
                rValue = DBNull.Value;
            }
            else
            {
                rValue = value;
            }

            return rValue;
        }

        /// <summary>
        /// 將資料轉換為資料庫儲存對應值，若待轉換資料等於int.MinValue，則回傳DBNull.Value。
        /// </summary>
        /// <param name="value">待轉換資料。</param>
        /// <returns></returns>
        public static object GetDBValue(int value)
        {
            object rValue;

            if (value == int.MinValue)
            {
                rValue = DBNull.Value;
            }
            else
            {
                rValue = value;
            }

            return rValue;
        }

        /// <summary>
        /// 將資料轉換為資料庫儲存對應值，若待轉換資料等於DateTime.MinValue，則回傳DBNull.Value。
        /// </summary>
        /// <param name="value">待轉換資料。</param>
        /// <returns></returns>
        public static object GetDBValue(DateTime value)
        {
            object rValue;

            if (value == DateTime.MinValue)
            {
                rValue = DBNull.Value;
            }
            else
            {
                rValue = value;
            }

            return rValue;
        }

        /// <summary>
        /// 將資料轉換為DateTime，若轉換失敗則回傳原始值。
        /// </summary>
        /// <param name="value">待轉換資料。</param>
        /// <returns></returns>
        public static string ConvertToStringDateTime(string value)
        {
            try
            {
                DateTime d = Convert.ToDateTime(value);

                return ConvertToStringDateTime(d);
            }
            catch
            {
                return value;
            }
        }

        /// <summary>
        /// 轉換年月格式為yyyy/MM.
        /// </summary>
        /// <param name="year">年份.</param>
        /// <param name="month">月份.</param>
        /// <returns></returns>
        public static string ConvertToYYYYMM(int year, int month)
        {
            return string.Format("{0:0000}/{1:00}", year, month);
        }

        /// <summary>
        /// 將資料轉換為int，若待轉換資料等於null，則回傳 0.
        /// </summary>
        /// <param name="value">待轉換資料。</param>
        /// <returns></returns>
        public static int GetIntValue(object value)
        {
            return GetIntValue(value, 0);
        }

        /// <summary>
        /// 將資料轉換為int，若待轉換資料等於null，則回傳nullValue的設定值。
        /// </summary>
        /// <param name="value">待轉換資料。</param>
        /// <param name="nullValue">當待轉換資料等於null時的轉換值。</param>
        /// <returns></returns>
        public static int GetIntValue(object value, int nullValue)
        {
            int retValue = nullValue;

            if (value != null)
            {
                string temp = value.ToString();

                if (temp.Length > 0)
                {
                    try
                    {
                        retValue = Convert.ToInt32(value);
                    }
                    catch
                    {
                        return retValue;
                    }

                }
            }

            return retValue;
        }

        /// <summary>
        /// 取得對應月份的英文名字。
        /// </summary>
        /// <param name="month">月份, 輸入範圍為1~12。</param>
        /// <remarks>若月份輸入值不在合法範圍內，則回傳空白。</remarks>
        /// <returns></returns>
        public static string GetMonthEnglishName(int month)
        {
            if (month < 1 || month > 12)
            {
                return "";
            }

            int index = month - 1;

            return abbrMonths[index];
        }

        /// <summary>
        /// 取得對應月份的英文名字。
        /// </summary>
        /// <param name="month">月份, 輸入範圍為1~12。</param>
        /// <remarks>若月份輸入值不在合法範圍內，則回傳空白。</remarks>
        /// <returns></returns>
        public static string GetMonthEnglishName(string month)
        {
            int m;

            try
            {
                m = Convert.ToInt32(month);
            }
            catch
            {
                return "";
            }

            return GetMonthEnglishName(m);
        }

        /// <summary>
        /// 轉換年月格式為yyyy/MM.
        /// </summary>
        /// <param name="dateTime">待轉換資料。</param>
        /// <returns></returns>
        public static string ConvertToYYYYMM(DateTime dateTime)
        {
            return ConvertToYYYYMM(dateTime.Year, dateTime.Month);
        }

        /// <summary>
        /// 轉換年月格式為yyyy/MM.
        /// </summary>
        /// <param name="dateTime">待轉換資料。</param>
        /// <returns></returns>
        public static string ConvertToYYYYMM(DateTime? dateTime)
        {

            string rValue;

            if (dateTime == null)
            {
                rValue = "";
            }
            else if (dateTime == DateTime.MinValue)
            {
                rValue = "";
            }
            else
            {
                rValue = ConvertToYYYYMM(((DateTime)dateTime).Year, ((DateTime)dateTime).Month);
            }

            return rValue;
        }

        /// <summary>
        /// 轉換年月格式為yyyy/MM.
        /// </summary>
        /// <param name="dateTime">yyyymm</param>
        /// <returns></returns>
        public static string ConvertToYYYYMM(string dateTime)
        {
            string rValue;

            if (string.IsNullOrEmpty(dateTime))
            {
                rValue = "";
            }
            else if (dateTime.Length < 6)
            {
                rValue = dateTime;
            }
            else
            {
                try
                {
                    rValue = ConvertToYYYYMM(Convert.ToInt32(dateTime.Substring(0, 4)), Convert.ToInt32(dateTime.Substring(4)));
                }
                catch
                {
                    rValue = dateTime;
                }
            }

            return rValue;
        }

        /// <summary>
        /// 取得DataRow值, 若值等於DBNull, 則回傳該int.MinValue.
        /// </summary>
        /// <param name="o">欄位值。</param>
        /// <returns></returns>
        public static int GetDataRowIntValue(object o)
        {
            if (o == DBNull.Value)
            {
                return int.MinValue;
            }
            else
            {
                return Convert.ToInt32(o);
            }
        }

        /// <summary>
        /// 取得DataRow值, 若值等於DBNull, 則回傳該DateTime.MinValue.
        /// </summary>
        /// <param name="o">欄位值。</param>
        /// <returns></returns>
        public static DateTime GetDataRowDateTimeValue(object o)
        {
            if (o == DBNull.Value)
            {
                return DateTime.MinValue;
            }
            else
            {
                return Convert.ToDateTime(o);
            }
        }

        /// <summary>
        /// 取得DataRow值, 若值等於DBNull, 則回傳該DateTime.MinValue.
        /// </summary>
        /// <param name="o">欄位值。</param>
        /// <returns></returns>
        public static DateTime GetDataRowDateValue(object o)
        {
            if (o == DBNull.Value)
            {
                return DateTime.MinValue;
            }
            else
            {
                DateTime temp = Convert.ToDateTime(o);

                return GetDateValue(temp);
            }
        }

        /// <summary>
        /// 去除日期資料的時間資訊。
        /// </summary>
        /// <param name="dateTime">待轉換資料。</param>
        /// <returns></returns>
        public static DateTime GetDateValue(DateTime dateTime)
        {
            return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day);
        }

        /// <summary>
        /// 取得DataRow值, 若值等於DBNull, 則回傳該double.MinValue.
        /// </summary>
        /// <param name="o">欄位值。</param>
        /// <returns></returns>
        public static double GetDataRowDoubleValue(object o)
        {
            if (o == DBNull.Value)
            {
                return double.MinValue;
            }
            else
            {
                return Convert.ToDouble(o);
            }
        }

        /// <summary>
        /// 取得DataRow值, 若值等於DBNull, 則回傳defaultValue.
        /// </summary>
        /// <param name="o">欄位值。</param>
        /// <param name="defaultValue">當欄位值等於DBNull時的轉換值。</param>
        /// <returns></returns>
        public static double GetDataRowDoubleValue(object o, double defaultValue)
        {
            if (o == DBNull.Value)
            {
                return defaultValue;
            }
            else
            {
                return Convert.ToDouble(o);
            }
        }

        /// <summary>
        /// 去掉字串前方0.
        /// </summary>
        /// <param name="value">待轉換資料。</param>
        /// <returns></returns>
        public static string RemoveFrontZero(string value)
        {
            while (value.StartsWith("0"))
            {
                value = value.Substring(1);
            }

            return value;
        }

        /// <summary>
        /// 取得DataRow值, 若值等於DBNull, 則回傳該defaultValue.
        /// </summary>
        /// <param name="o">欄位值。</param>
        /// <param name="defaultValue">欄位值為DBNull時的轉換值。</param>
        /// <returns></returns>
        public static int GetDataRowIntValue(object o, int defaultValue)
        {
            if (o == DBNull.Value)
            {
                return defaultValue;
            }
            else
            {
                return Convert.ToInt32(o);
            }
        }

        /// <summary>
        /// 將文字字串轉換為decimal格式，若轉換失敗，則回decimal.MinValue。
        /// </summary>
        /// <param name="value">待轉換資料。</param>
        /// <returns></returns>
        public static decimal GetBLDecimalValue(string value)
        {
            decimal rValue;

            try
            {
                rValue = Convert.ToDecimal(value);
            }
            catch
            {
                rValue = decimal.MinValue;
            }

            return rValue;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static decimal FormatDecimal(decimal value, int len)
        {
            if (len == 0)
            {
                return System.Convert.ToDecimal(value.ToString("F0"));
            }
            else
            {
                return System.Convert.ToDecimal(value.ToString("F2"));
            }
        }

        /// <summary>
        /// 將文字字串轉換為decimal格式，若轉換失敗，則回傳預設值。
        /// </summary>
        /// <param name="value">待轉換資料。</param>
        /// <param name="defaultValue">預設值。</param>
        /// <returns></returns>
        public static decimal GetBLDecimalValue(string value, decimal defaultValue)
        {
            decimal rValue;

            try
            {
                rValue = Convert.ToDecimal(value);
            }
            catch
            {
                rValue = defaultValue;
            }

            return rValue;
        }

        /// <summary>
        /// 將文字字串轉換為datetime?格式，若轉換失敗，則回傳預設值，若字串為null則傳回nullValue的指定值。
        /// </summary>
        /// <param name="value">待轉換資料。</param>
        /// <param name="nullValue">等轉換資料為null時的轉換值。</param>
        /// <returns></returns>
        public static DateTime? GetBLDateTimeNullableValue(string value, string nullValue)
        {
            DateTime? rValue = null;

            if (value == nullValue)
            {
                return rValue;
            }

            rValue = Convert.ToDateTime(value);

            return rValue;
        }

        /// <summary>
        /// 四捨五入至整數位.
        /// </summary>
        /// <param name="value">待轉換資料。</param>
        /// <returns></returns>
        public static int Round(double value)
        {
            return Convert.ToInt32(value.ToString("0"));
        }

        /// <summary>
        /// 四捨五入至整數位.
        /// </summary>
        /// <param name="value">待轉換資料。</param>
        /// <returns></returns>
        public static int Round(decimal value)
        {
            return Convert.ToInt32(value.ToString("0"));
        }

        /// <summary>
        /// 四捨五入至整數位.
        /// </summary>
        /// <param name="value">待轉換資料。</param>
        /// <returns></returns>
        public static int Round(decimal? value)
        {
            return value.ToString() == "" ? 0 : Convert.ToInt32(value);
        }

        /// <summary>
        /// 四捨五入至整小數位.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="digit"></param>
        /// <returns></returns>
        public static Decimal Round(decimal value, int digit)
        {
            return Decimal.Round(value, digit);
        }

        /// <summary>
        /// 四捨五入至整小數位.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="digit"></param>
        /// <returns></returns>
        public static Decimal Round(decimal? value, int digit)
        {
            return value.ToString() == "" ? 0 : Decimal.Round((decimal)value, digit);
        }

        /// <summary>
        /// 無條件捨去
        /// </summary>
        /// <param name="number">數值</param>
        /// <param name="place">小數位數</param>
        /// <returns></returns>
        ///<remarks>20131012 Tom</remarks>
        public static Decimal Floor(decimal number, int place)
        {
            decimal dc = Convert.ToDecimal (Math.Pow(10,place)) ;
            decimal Num=Decimal.Floor(number*dc);
            return Num / dc;
        }

        /// <summary>
        /// 同字段的實體類映射
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="L"></typeparam>
        /// <param name="t"></param>
        /// <returns></returns>
        public static L SetProperties<T, L>(T t) where L : new()
        {
            if (t == null)
            {
                return default(L);
            }

            // 獲取實體類和DataTable對應的屬性清單
            Dictionary<string, PropertyInfo> proT = new Dictionary<string, PropertyInfo>();
            Dictionary<string, PropertyInfo> proL = new Dictionary<string, PropertyInfo>();

            Array.ForEach<PropertyInfo>(typeof(T).GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public), property =>
            {
                proT.Add(property.Name, property);
            });

            L setT = new L();

            Array.ForEach<PropertyInfo>(typeof(L).GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.DeclaredOnly), property =>
            {
                if (property.Name.Contains("Query_"))
                {
                    return;
                }
                var value = proT[property.Name].GetValue(t, null);
                string fullName = (property.PropertyType).FullName;

                if (fullName.Contains("System.Nullable"))
                {
                    if (value == null)
                    {
                        property.SetValue(setT, null, null);
                    }
                    else
                    {
                        if (fullName.Contains("System.Int32"))
                        {
                            property.SetValue(setT, Convert.ToInt32(value), null);
                        }
                        else if (fullName.Contains("System.String"))
                        {
                            property.SetValue(setT, value.ToString(), null);
                        }
                        else if (fullName.Contains("System.DateTime"))
                        {
                            property.SetValue(setT, Convert.ToDateTime(value), null);
                        }
                        else if (fullName.Contains("System.Decimal"))
                        {
                            property.SetValue(setT, Convert.ToDecimal(value), null);
                        }
                        else if (fullName.Contains("System.Char"))
                        {
                            property.SetValue(setT, Convert.ToChar(value), null);
                        }
                        else if (fullName.Contains("System.Boolean"))
                        {
                            property.SetValue(setT, Convert.ToBoolean(value), null);
                        }
                        else
                        {
                            property.SetValue(setT, Convert.ToString(value), null);
                        }
                    }
                }
                else
                {
                    switch (fullName)
                    {
                        case "System.Int32":
                            property.SetValue(setT, Convert.ToInt32(value), null);
                            break;
                        case "System.String":
                            if ((property.PropertyType).FullName.Contains("DateTime"))
                            {
                                property.SetValue(setT, ConvertToStringDate(value), null);
                            }
                            else
                            {
                                property.SetValue(setT, value == null ? "" : value.ToString(), null);
                            }

                            break;
                        case "System.DateTime":
                            property.SetValue(setT, Convert.ToDateTime(value), null);
                            break;
                        case "System.Decimal":
                            property.SetValue(setT, Convert.ToDecimal(value), null);
                            break;
                        case "System.Char":
                            property.SetValue(setT, Convert.ToChar(value), null);
                            break;
                        case "System.Boolean":
                            property.SetValue(setT, Convert.ToBoolean(value), null);
                            break;
                        default:
                            property.SetValue(setT, value == null ? "" : value.ToString(), null);
                            break;
                    }
                }
            });

            return setT;
        }

        /// <summary>
        /// IList轉Json
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <remarks>Add By Sam 2011/12/21</remarks>
        public static string ObjToJson<T>(T data)
        {
            try
            {
                System.Runtime.Serialization.Json.DataContractJsonSerializer serializer = new System.Runtime.Serialization.Json.DataContractJsonSerializer(data.GetType());
                using (MemoryStream ms = new MemoryStream())
                {
                    serializer.WriteObject(ms, data);
                    return Encoding.UTF8.GetString(ms.ToArray());
                }
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Json轉IList
        /// </summary>
        /// <param name="json"></param>
        /// <param name="t"></param>
        /// <remarks>Add By Sam 2011/12/21</remarks>
        public static Object JsonToObj(String json, Type t)
        {
            try
            {
                System.Runtime.Serialization.Json.DataContractJsonSerializer serializer = new System.Runtime.Serialization.Json.DataContractJsonSerializer(t);
                using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(json)))
                {

                    return serializer.ReadObject(ms);
                }
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 將資料yyyymmmdd轉換為yyyy/dd/dd。
        /// </summary>
        /// <param name="value">待轉換資料。</param>
        /// <returns></returns>
        public static string ConvertToDateTimeFormat(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return "";
            }
            else if (value.Length < 8)
            {
                return value;
            }
            else
            {
                string returnValue = value.Substring(0, 4) + "/" + value.Substring(4, 2) + "/" + value.Substring(6, 2);

                return returnValue;
            }
        }

        /// <summary>
        /// 將資料ddmmyyyy轉換為yyyy/mm/dd。
        /// </summary>
        /// <param name="value">待轉換資料。</param>
        /// <returns></returns>
        /// 20130515 horace
        public static string ConvertddmmyyyyToDateTimeFormat(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return "";
            }
            else if (value.Length < 8)
            {
                return value;
            }
            else
            {
                string returnValue = value.Substring(4, 4) + "/" + value.Substring(2, 2) + "/" + value.Substring(0, 2);

                return returnValue;
            }
        }

        /// <summary>
        /// 將資料yyyymmmdd轉換為yyyy/dd/dd。
        /// </summary>
        /// <param name="value">待轉換資料。</param>
        /// <returns></returns>
        public static DateTime ConvertToDateTime(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return DateTime.MinValue;
            }
            else if (value.Length < 8)
            {
                return DateTime.MinValue;
            }
            else
            {
                string returnValue = value.Substring(0, 4) + "/" + value.Substring(4, 2) + "/" + value.Substring(6, 2);

                try
                {
                    return Convert.ToDateTime(returnValue);
                }
                catch
                {
                    return DateTime.MinValue;
                }
            }
        }

		/// <summary>
		/// 把DataTable轉換成List
		/// </summary>
		/// <typeparam name="T">泛型</typeparam>
		/// <param name="dt">數據源</param>
		/// <returns>泛型集合</returns>
		public static IList<T> DataSetToList<T>(DataTable dt)
		{
			if (dt == null)
			{
				return null;
			}

			if (dt.Rows.Count < 0)
			{
				return null;
			}

			// 創建一個泛型集合對象
			IList<T> list = new List<T>();

			// 獲取實體類和DataTable對應的屬性清單
			Dictionary<PropertyInfo, string> pro = new Dictionary<PropertyInfo, string>();

			Array.ForEach<PropertyInfo>(typeof(T).GetProperties(), property =>
			{
				foreach (DataColumn column in dt.Columns)
				{
					if (string.Compare(column.ColumnName.Replace("_", ""), property.Name, true) == 0)
					{
						pro.Add(property, column.ColumnName);
					}
				}
			});

			// 對清單中的實體類屬性賦值
			foreach (DataRow row in dt.Rows)
			{
				// 創建一個泛型對象
				T t = Activator.CreateInstance<T>();

				foreach (PropertyInfo property in pro.Keys)
				{

					object value = row[pro[property]];
					string fullName = property.PropertyType.ToString();
					if (fullName.Contains("System.Nullable"))
					{
						value = string.IsNullOrEmpty(value.ToString()) ? null : value;

						if (value == null)
						{
							property.SetValue(t, null, null);
						}
						else
						{
							if (fullName.Contains("System.Int32"))
							{
								property.SetValue(t, Convert.ToInt32(value.ToString()), null);
							}
							else if (fullName.Contains("System.String"))
							{
								property.SetValue(t, value.ToString(), null);
							}
							else if (fullName.Contains("System.DateTime"))
							{
								property.SetValue(t, Convert.ToDateTime(value.ToString()), null);
							}
							else if (fullName.Contains("System.Decimal"))
							{
								property.SetValue(t, Convert.ToDecimal(value.ToString()), null);
							}
							else if (fullName.Contains("System.Char"))
							{
								property.SetValue(t, Convert.ToChar(value.ToString()), null);
							}
							else if (fullName.Contains("System.Boolean"))
							{
								property.SetValue(t, Convert.ToBoolean(value.ToString()), null);
							}
							else if (fullName.Contains("System.Guid"))
							{
								property.SetValue(t, new Guid(value.ToString()), null);
							}
							else
							{
								property.SetValue(t, Convert.ToString(value), null);
							}
						}
					}
					else
					{
						switch (fullName)
						{
							case "System.Int32":
								property.SetValue(t, Convert.ToInt32(string.IsNullOrEmpty(value.ToString()) ? "0" : value), null);
								break;
							case "System.String":
								if ((property.PropertyType).FullName.Contains("DateTime"))
								{
									property.SetValue(t, Convert.ToDateTime(value.ToString()), null);
								}
								else
								{
									property.SetValue(t, value == null ? "" : value.ToString(), null);
								}

								break;
							case "System.DateTime":
								property.SetValue(t, string.IsNullOrEmpty(value.ToString()) ? DateTime.MinValue : Convert.ToDateTime(value.ToString()), null);
								break;
							case "System.Decimal":
								property.SetValue(t, string.IsNullOrEmpty(value.ToString()) ? 0 : Convert.ToDecimal(value.ToString()), null);
								break;
							case "System.Char":
								property.SetValue(t, string.IsNullOrEmpty(value.ToString()) ? ' ' : Convert.ToChar(value.ToString()), null);
								break;
							case "System.Boolean":
								property.SetValue(t, Convert.ToBoolean(value.ToString()), null);
								break;
							case "System.Guid":
								property.SetValue(t, new Guid(value.ToString()), null);
								break;
							case "System.Byte[]":
								property.SetValue(t, value, null);
								break;
							default:
								property.SetValue(t, value == null ? "" : value.ToString(), null);
								break;
						}
					}
				}

				list.Add(t);
			}

			return list;
		}

        /// <summary>
        /// 將數字轉換成萬元
        /// </summary>
        /// <param name="num">欲轉換的數字</param>
        /// <returns>轉換後的數字</returns>
        public static string NumToChineseUnit(Decimal? num)
        {
            string result = "0";
            string Unit = "萬元";
            int Divid = 10000;

            if (num == null || num == Decimal.MinValue || num == 0) { return "0" + Unit; }

            if (num > Divid)
            {
                result = Convert.ToInt32((num / Divid)).ToString() + Unit;
            }
            else
            {
                result = Convert.ToDecimal((num / Divid)).ToString("0.##") + Unit;
            }

            return result;
        }

        /// <summary>
        /// 將數字轉換成萬元[無條件進位到萬元]
        /// </summary>
        /// <param name="num">欲轉換的數字</param>
        /// <returns>轉換後的數字</returns>
        public static string NumToChineseUnitCeiling(Decimal? num)
        {
            string result = "0";
            string Unit = "萬元";
            decimal temp = 0;
            int Divid = 10000;

            if (num == null || num == Decimal.MinValue || num == 0) { return "0" + Unit; }

            if (num > Divid)
            {
                temp = num.Value;
                result = Convert.ToInt32(Math.Ceiling(temp / Divid)).ToString() + Unit;
            }
            else
            {
                temp = num.Value;
                result = Convert.ToDecimal(Math.Ceiling(temp / Divid)).ToString("0.##") + Unit;
            }

            return result;
        }

        /// <summary>
        /// 將金額以萬為單位
        /// </summary>
        /// <param name="num">欲轉換的數字</param>
        /// <returns>轉換後的數字</returns>
        public static decimal NumToTenthousand(Decimal num)
        {
            decimal result = 0;
            decimal Divid = 10000;

            if (num == Decimal.MinValue || num == 0) { return result; }

            result = num / Divid;

            return result;
        }

        /// <summary>
        /// 將金額以萬為單位[無條件進位到萬元]
        /// </summary>
        /// <param name="num">欲轉換的數字</param>
        /// <returns>轉換後的數字</returns>
        public static decimal NumToTenthousandCeiling(Decimal? num)
        {
            decimal result = 0;
            decimal Divid = 10000;
            decimal _num = 0;

            if (num == null || num == 0) { return result; }

            _num = (num == null) ? 0 : num.Value;

            result = Math.Ceiling(_num / Divid);

            return result;
        }

        /// <summary>
        /// 民國年(yyymmdd)轉成西元年(yyyy/mm/dd) 
        /// </summary>
        /// <param name="value">欲轉換的民國年</param>
        /// <returns>轉換後的西元年</returns>
        /// added by smallzhi
        /// IR-59310
        public static DateTime? ConvertToDate(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return null;
            }
            else if (value.Length < 7)
            {
                return null;
            }
            else
            {
                try
                {
                    string year = (Convert.ToInt32(value.Substring(0, 3)) + 1911).ToString();
                    string returnValue = year + "/" + value.Substring(3, 2) + "/" + value.Substring(5, 2);
                    return Convert.ToDateTime(returnValue);
                }
                catch
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// 民國年(yyymmdd)轉成西元年(yyyy/mm/dd) 傳出字串
        /// </summary>
        /// <param name="value">欲轉換的民國年</param>
        /// <returns>轉換後的西元年</returns>
        /// added by smallzhi
        /// IR-59310
        public static string ConvertToDateString(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return "";
            }
            else if (value.Length < 7)
            {
                return "";
            }
            else
            {
                try
                {
                    string year = (Convert.ToInt32(value.Substring(0, 3)) + 1911).ToString();
                    string returnValue = year + "/" + value.Substring(3, 2) + "/" + value.Substring(5, 2);
                    DateTime d = Convert.ToDateTime(returnValue);
                    return d.ToString("yyyy/MM/dd");
                }
                catch
                {
                    return "";
                }
            }
        }

        /// <summary>
        /// 西元年轉民國YYYMMDD字串
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string ConvertToROCDate(DateTime value)
        {
            try
            {
                string yyyymmdd = value.ToString("yyyy/MM/dd");
                int yyy = Convert.ToInt32(yyyymmdd.Substring(0, 4)) - 1911;
                string yyyt = yyy.ToString();
                if (yyyt.Length == 2) yyyt = "0" + yyyt;
                else if (yyyt.Length == 1) yyyt = "00" + yyyt;
                string yyymmdd = yyyt + yyyymmdd.Substring(5, 2) + yyyymmdd.Substring(8, 2);
                return yyymmdd;
            }
            catch {
                return null;
            }
        }

        /// <summary>
        /// 西元年轉民國YYYMMDD字串
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        /// Added by smallzhi on 2013-08-14
        public static string ConvertToROCDate(DateTime? value)
        {
            try
            {
                if (value != null)
                {
                    DateTime t = value.Value;
                    string yyyymmdd = t.ToString("yyyy/MM/dd");
                    int yyy = Convert.ToInt32(yyyymmdd.Substring(0, 4)) - 1911;
                    string yyyt = yyy.ToString();
                    if (yyyt.Length == 2) yyyt = "0" + yyyt;
                    else if (yyyt.Length == 1) yyyt = "00" + yyyt;
                    string yyymmdd = yyyt + yyyymmdd.Substring(5, 2) + yyyymmdd.Substring(8, 2);
                    return yyymmdd;
                }
                else
                {
                    return "";
                }
            }
            catch
            {
                return "";
            }
        }

        /// <summary>
        /// 為某個值就呈現紅字 20140313
        /// </summary>
        /// <param name="_t"></param>
        /// <returns></returns>
        public static HtmlString translate(string _t,string _u)
        {
            if (_t == _u)
            {
                return new HtmlString("<span style='color:Red'>" + _t + "</span>");
            }
            else
            {
                return new HtmlString(_t);
            }
        }
    }
}