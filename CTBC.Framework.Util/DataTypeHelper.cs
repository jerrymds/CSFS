/// <summary>
/// �{������:�ƾ������B�z
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
    /// ��ƫ��O�ഫ�u��C
    /// </summary>
    public class DataTypeHelper
    {
        #region ���@�ܼ�
        private static readonly string[] abbrMonths =
            new string[12] { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };
        #endregion

        /// <summary>
        /// �ഫ��UI�h��ܪ���Ʈ榡�C
        /// </summary>
        /// <param name="sValue">����ܪ���ơC</param>
        /// <returns>�ഫ�� yyyy/mm/dd ����Ʈ榡�C</returns>
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
        /// �ഫ��UI�h��ܪ��T���榡�C
        /// </summary>
        /// <param name="sValue">����ܪ���ơC</param>
        /// <returns>�ഫ�� yyyy/mm/dd ����Ʈ榡�C</returns>
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
        /// �ഫ��UI�h��ܪ��T���榡�C20140218 smallzhi
        /// </summary>
        /// <param name="sValue">����ܪ���ơC</param>
        /// <returns>�ഫ�� yyyy/mm/dd ����Ʈ榡�CBy longtime</returns>
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
        /// �ഫ�e����ܸ��, �Y�Ȭ�int.MinValue, �h�^�Ǫť�.
        /// </summary>
        /// <param name="sValue">���ഫ��ơC</param>
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
        /// �ഫ�e����ܸ��, �Y�Ȭ�int.MinValue, �h�^�Ǫť�.
        /// </summary>
        /// <param name="sValue">���ഫ��ơC</param>
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
        /// �ഫ�e����ܸ��, �Y�Ȭ�int.MinValue, �h�^�Ǫť�.
        /// </summary>
        /// <param name="sValue">���ഫ��ơC</param>
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
        /// �ഫ�e����ܸ��, �Y�Ȭ�int.MinValue, �h�^�Ǫť�.
        /// </summary>
        /// <param name="sValue">���ഫ��ơC</param>
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
        /// �ഫ�e����ܸ��, �Y���ഫ��Ƶ���ŭȸ�ơA�h�^�Ǫŭȵ��G�C
        /// </summary>
        /// <param name="sValue">���ഫ��ơC</param>
        /// <param name="emptyString">�ŭȵ��G�C</param>
        /// <param name="emptyValue">�ŭȸ�ơC</param>
        /// <returns></returns>
        public static string GetUIValue(int sValue, int emptyValue, string emptyString)
        {
            string rValue = sValue == emptyValue ? emptyString : sValue.ToString();

            return rValue;
        }

        /// <summary>
        /// �ഫ�e����ܸ��, �Y���ഫ��Ƶ���ŭȸ�ơA�h�^�Ǫŭȵ��G�C
        /// </summary>
        /// <param name="value">���ഫ��ơC</param>
        /// <param name="emptyString">�ŭȵ��G�C</param>
        /// <param name="emptyValue">�ŭȸ�ơC</param>
        /// <returns></returns>
        public static string GetUIValue(double value, double emptyValue, string emptyString)
        {
            string rValue = value == emptyValue ? emptyString : value.ToString();

            return rValue;
        }

        /// <summary>
        /// �ഫ��UI�h��ܪ���Ʈ榡�C
        /// </summary>
        /// <param name="value">���ഫ����ơC</param>
        /// <remarks>���Ƭ��ȵ���Adouble.MinValue�A�h�^�ǪťաC</remarks>
        /// <returns>�ഫ�᪺��Ʈ榡�C</returns>
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
        /// �ഫ��UI�h��ܪ���Ʈ榡�C
        /// </summary>
        /// <param name="value">���ഫ����ơC</param>
        /// <param name="longFormat">�O�_�]�t�ɶ���ơC</param>
        /// <remarks>longFormat = true�A�ഫ�榡��: yyyy/MM/dd HH:mm:ss�C�_�h�ഫ�榡��: yyyy/MM/dd�C</remarks>
        /// <returns>�ഫ�᪺��Ʈ榡�C</returns>
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
        /// �ഫ�e����ܸ��, �Y�Ȭ�0, �h�^�Ǫť�.
        /// </summary>
        /// <param name="sValue">���ഫ��ơC</param>
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
        /// �ഫ�e����ܸ��, �Y�Ȭ�0, �h�^�Ǫť�.
        /// </summary>
        /// <param name="sValue">���ഫ��ơC</param>
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
        /// �N�r�Ŧ������ഫ��Guid�C
        /// </summary>
        /// <param name="uID">���ഫ��ơC</param>
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
        /// �N�r�Ŧ������ഫ��Guid�C
        /// </summary>
        /// <param name="uID">���ഫ��ơC</param>
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
        /// �N����ഫ�� yyyy/MM/dd-HH:mm:ss �榡�r��C
        /// </summary>
        /// <param name="dateTime">�����ơC</param>
        /// <returns></returns>
        public static string ConvertToStringDateTime(DateTime dateTime)
        {
            return dateTime.ToString("yyyy/MM/dd HH:mm:ss", DateTimeFormatInfo.InvariantInfo);
        }

        /// <summary>
        /// �N����ഫ�� yyyy/MM/dd HH:mm:ss �榡�r��C
        /// </summary>
        /// <param name="dateTime">�����ơC</param>
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
        /// �N����ഫ�� yyyy/MM/dd �榡�r��C
        /// </summary>
        /// <param name="date">�����ơC</param>
        /// <returns></returns>
        public static string ConvertToStringDate(DateTime date)
        {
            return date.ToString("yyyy/MM/dd", DateTimeFormatInfo.InvariantInfo);
        }

        /// <summary>
        /// �N����ഫ�� yyyy/MM/dd �榡�r��C
        /// </summary>
        /// <param name="date">�����ơC</param>
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
        /// �N����ഫ�� yyyy/MM/dd �榡�r��C
        /// </summary>
        /// <param name="date">�����ơC</param>
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
        /// �N����ഫ��DateTime�榡�A�Y�ഫ���ѡA�h�^DateTime.MinValue�C
        /// </summary>
        /// <param name="value">���ഫ��ơC</param>
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
        /// �N����ഫ��DateTime�榡�A�Y�ഫ���ѡA�h�^null�C
        /// </summary>
        /// <param name="value">���ഫ��ơC</param>
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
        /// �N����ഫ��int�榡�A�Y�ഫ���ѡA�h�^double.MinValue�C
        /// </summary>
        /// <param name="value">���ഫ��ơC</param>
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
        /// �N����ഫ��double�榡�A�Y�ഫ���ѡA�h�^defaultValue�]�w�ȡC
        /// </summary>
        /// <param name="value">���ഫ��ơC</param>
        /// <param name="defaultValue">�ഫ���Ѫ��w�]�ȡC</param>
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
        /// �N����ഫ��Int64�榡�A�Y�ഫ���ѡA�h�^defaultValue�]�w�ȡC
        /// </summary>
        /// <param name="value">���ഫ��ơC</param>
        /// <param name="defaultValue">�ഫ���Ѫ��w�]�ȡC</param>
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
        /// �N����ഫ��int�榡�A�Y�ഫ���ѡA�h�^int.MinValue�C
        /// </summary>
        /// <param name="value">���ഫ��ơC</param>
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
        /// �N����ഫ��int�榡�A�Y�ഫ���ѡA�h�^defaultValue�]�w�ȡC
        /// </summary>
        /// <param name="value">���ഫ��ơC</param>
        /// <param name="defaultValue">�ഫ���Ѫ��w�]�ȡC</param>
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
        /// �N����ഫ����Ʈw�x�s�����ȡA�Y���ഫ��Ƶ���double.MinValue�A�h�^��DBNull.Value�C
        /// </summary>
        /// <param name="value">���ഫ��ơC</param>
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
        /// �N����ഫ����Ʈw�x�s�����ȡA�Y���ഫ��Ƶ���int.MinValue�A�h�^��DBNull.Value�C
        /// </summary>
        /// <param name="value">���ഫ��ơC</param>
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
        /// �N����ഫ����Ʈw�x�s�����ȡA�Y���ഫ��Ƶ���DateTime.MinValue�A�h�^��DBNull.Value�C
        /// </summary>
        /// <param name="value">���ഫ��ơC</param>
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
        /// �N����ഫ��DateTime�A�Y�ഫ���ѫh�^�ǭ�l�ȡC
        /// </summary>
        /// <param name="value">���ഫ��ơC</param>
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
        /// �ഫ�~��榡��yyyy/MM.
        /// </summary>
        /// <param name="year">�~��.</param>
        /// <param name="month">���.</param>
        /// <returns></returns>
        public static string ConvertToYYYYMM(int year, int month)
        {
            return string.Format("{0:0000}/{1:00}", year, month);
        }

        /// <summary>
        /// �N����ഫ��int�A�Y���ഫ��Ƶ���null�A�h�^�� 0.
        /// </summary>
        /// <param name="value">���ഫ��ơC</param>
        /// <returns></returns>
        public static int GetIntValue(object value)
        {
            return GetIntValue(value, 0);
        }

        /// <summary>
        /// �N����ഫ��int�A�Y���ഫ��Ƶ���null�A�h�^��nullValue���]�w�ȡC
        /// </summary>
        /// <param name="value">���ഫ��ơC</param>
        /// <param name="nullValue">����ഫ��Ƶ���null�ɪ��ഫ�ȡC</param>
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
        /// ���o����������^��W�r�C
        /// </summary>
        /// <param name="month">���, ��J�d��1~12�C</param>
        /// <remarks>�Y�����J�Ȥ��b�X�k�d�򤺡A�h�^�ǪťաC</remarks>
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
        /// ���o����������^��W�r�C
        /// </summary>
        /// <param name="month">���, ��J�d��1~12�C</param>
        /// <remarks>�Y�����J�Ȥ��b�X�k�d�򤺡A�h�^�ǪťաC</remarks>
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
        /// �ഫ�~��榡��yyyy/MM.
        /// </summary>
        /// <param name="dateTime">���ഫ��ơC</param>
        /// <returns></returns>
        public static string ConvertToYYYYMM(DateTime dateTime)
        {
            return ConvertToYYYYMM(dateTime.Year, dateTime.Month);
        }

        /// <summary>
        /// �ഫ�~��榡��yyyy/MM.
        /// </summary>
        /// <param name="dateTime">���ഫ��ơC</param>
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
        /// �ഫ�~��榡��yyyy/MM.
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
        /// ���oDataRow��, �Y�ȵ���DBNull, �h�^�Ǹ�int.MinValue.
        /// </summary>
        /// <param name="o">���ȡC</param>
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
        /// ���oDataRow��, �Y�ȵ���DBNull, �h�^�Ǹ�DateTime.MinValue.
        /// </summary>
        /// <param name="o">���ȡC</param>
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
        /// ���oDataRow��, �Y�ȵ���DBNull, �h�^�Ǹ�DateTime.MinValue.
        /// </summary>
        /// <param name="o">���ȡC</param>
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
        /// �h�������ƪ��ɶ���T�C
        /// </summary>
        /// <param name="dateTime">���ഫ��ơC</param>
        /// <returns></returns>
        public static DateTime GetDateValue(DateTime dateTime)
        {
            return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day);
        }

        /// <summary>
        /// ���oDataRow��, �Y�ȵ���DBNull, �h�^�Ǹ�double.MinValue.
        /// </summary>
        /// <param name="o">���ȡC</param>
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
        /// ���oDataRow��, �Y�ȵ���DBNull, �h�^��defaultValue.
        /// </summary>
        /// <param name="o">���ȡC</param>
        /// <param name="defaultValue">�����ȵ���DBNull�ɪ��ഫ�ȡC</param>
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
        /// �h���r��e��0.
        /// </summary>
        /// <param name="value">���ഫ��ơC</param>
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
        /// ���oDataRow��, �Y�ȵ���DBNull, �h�^�Ǹ�defaultValue.
        /// </summary>
        /// <param name="o">���ȡC</param>
        /// <param name="defaultValue">���Ȭ�DBNull�ɪ��ഫ�ȡC</param>
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
        /// �N��r�r���ഫ��decimal�榡�A�Y�ഫ���ѡA�h�^decimal.MinValue�C
        /// </summary>
        /// <param name="value">���ഫ��ơC</param>
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
        /// �N��r�r���ഫ��decimal�榡�A�Y�ഫ���ѡA�h�^�ǹw�]�ȡC
        /// </summary>
        /// <param name="value">���ഫ��ơC</param>
        /// <param name="defaultValue">�w�]�ȡC</param>
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
        /// �N��r�r���ഫ��datetime?�榡�A�Y�ഫ���ѡA�h�^�ǹw�]�ȡA�Y�r�ꬰnull�h�Ǧ^nullValue�����w�ȡC
        /// </summary>
        /// <param name="value">���ഫ��ơC</param>
        /// <param name="nullValue">���ഫ��Ƭ�null�ɪ��ഫ�ȡC</param>
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
        /// �|�ˤ��J�ܾ�Ʀ�.
        /// </summary>
        /// <param name="value">���ഫ��ơC</param>
        /// <returns></returns>
        public static int Round(double value)
        {
            return Convert.ToInt32(value.ToString("0"));
        }

        /// <summary>
        /// �|�ˤ��J�ܾ�Ʀ�.
        /// </summary>
        /// <param name="value">���ഫ��ơC</param>
        /// <returns></returns>
        public static int Round(decimal value)
        {
            return Convert.ToInt32(value.ToString("0"));
        }

        /// <summary>
        /// �|�ˤ��J�ܾ�Ʀ�.
        /// </summary>
        /// <param name="value">���ഫ��ơC</param>
        /// <returns></returns>
        public static int Round(decimal? value)
        {
            return value.ToString() == "" ? 0 : Convert.ToInt32(value);
        }

        /// <summary>
        /// �|�ˤ��J�ܾ�p�Ʀ�.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="digit"></param>
        /// <returns></returns>
        public static Decimal Round(decimal value, int digit)
        {
            return Decimal.Round(value, digit);
        }

        /// <summary>
        /// �|�ˤ��J�ܾ�p�Ʀ�.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="digit"></param>
        /// <returns></returns>
        public static Decimal Round(decimal? value, int digit)
        {
            return value.ToString() == "" ? 0 : Decimal.Round((decimal)value, digit);
        }

        /// <summary>
        /// �L����˥h
        /// </summary>
        /// <param name="number">�ƭ�</param>
        /// <param name="place">�p�Ʀ��</param>
        /// <returns></returns>
        ///<remarks>20131012 Tom</remarks>
        public static Decimal Floor(decimal number, int place)
        {
            decimal dc = Convert.ToDecimal (Math.Pow(10,place)) ;
            decimal Num=Decimal.Floor(number*dc);
            return Num / dc;
        }

        /// <summary>
        /// �P�r�q���������M�g
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

            // ����������MDataTable�������ݩʲM��
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
        /// IList��Json
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
        /// Json��IList
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
        /// �N���yyyymmmdd�ഫ��yyyy/dd/dd�C
        /// </summary>
        /// <param name="value">���ഫ��ơC</param>
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
        /// �N���ddmmyyyy�ഫ��yyyy/mm/dd�C
        /// </summary>
        /// <param name="value">���ഫ��ơC</param>
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
        /// �N���yyyymmmdd�ഫ��yyyy/dd/dd�C
        /// </summary>
        /// <param name="value">���ഫ��ơC</param>
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
		/// ��DataTable�ഫ��List
		/// </summary>
		/// <typeparam name="T">�x��</typeparam>
		/// <param name="dt">�ƾڷ�</param>
		/// <returns>�x�����X</returns>
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

			// �Ыؤ@�Ӫx�����X��H
			IList<T> list = new List<T>();

			// ����������MDataTable�������ݩʲM��
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

			// ��M�椤���������ݩʽ��
			foreach (DataRow row in dt.Rows)
			{
				// �Ыؤ@�Ӫx����H
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
        /// �N�Ʀr�ഫ���U��
        /// </summary>
        /// <param name="num">���ഫ���Ʀr</param>
        /// <returns>�ഫ�᪺�Ʀr</returns>
        public static string NumToChineseUnit(Decimal? num)
        {
            string result = "0";
            string Unit = "�U��";
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
        /// �N�Ʀr�ഫ���U��[�L����i���U��]
        /// </summary>
        /// <param name="num">���ഫ���Ʀr</param>
        /// <returns>�ഫ�᪺�Ʀr</returns>
        public static string NumToChineseUnitCeiling(Decimal? num)
        {
            string result = "0";
            string Unit = "�U��";
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
        /// �N���B�H�U�����
        /// </summary>
        /// <param name="num">���ഫ���Ʀr</param>
        /// <returns>�ഫ�᪺�Ʀr</returns>
        public static decimal NumToTenthousand(Decimal num)
        {
            decimal result = 0;
            decimal Divid = 10000;

            if (num == Decimal.MinValue || num == 0) { return result; }

            result = num / Divid;

            return result;
        }

        /// <summary>
        /// �N���B�H�U�����[�L����i���U��]
        /// </summary>
        /// <param name="num">���ഫ���Ʀr</param>
        /// <returns>�ഫ�᪺�Ʀr</returns>
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
        /// ����~(yyymmdd)�ন�褸�~(yyyy/mm/dd) 
        /// </summary>
        /// <param name="value">���ഫ������~</param>
        /// <returns>�ഫ�᪺�褸�~</returns>
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
        /// ����~(yyymmdd)�ন�褸�~(yyyy/mm/dd) �ǥX�r��
        /// </summary>
        /// <param name="value">���ഫ������~</param>
        /// <returns>�ഫ�᪺�褸�~</returns>
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
        /// �褸�~�����YYYMMDD�r��
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
        /// �褸�~�����YYYMMDD�r��
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
        /// ���Y�ӭȴN�e�{���r 20140313
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