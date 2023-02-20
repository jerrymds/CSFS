/// <summary>
/// 程式說明:數據顯示處理
/// </summary>

using System.Text;
using Microsoft.VisualBasic;

namespace CTBC.FrameWork.Util
{
	/// <summary>
	/// 空白填充位置.
	/// </summary>
	public enum SpaceFormat
	{
		/// <summary>
		/// 資料前方.
		/// </summary>
		Before,
		/// <summary>
		/// 資料後方.
		/// </summary>
		After
	}

	/// <summary>
	/// 文字長度計算類型.
	/// </summary>
	public enum CHTCharType
	{
		/// <summary>
		/// 雙字元(中文字).
		/// </summary>
		DoubleByte,
		/// <summary>
		/// 未指定, 依系統內定.
		/// </summary>
		None
	}

	/// <summary>
	/// 字串處理共用元件.
	/// </summary>
	public class StringHelper
	{
		/// <summary>
		/// 取得指定空白個數.
		/// </summary>
		/// <param name="count">字串總長度.</param>
		/// <returns></returns>
		public static string Space(int count)
		{
			StringBuilder sb = new StringBuilder();

			for (int i = 0; i < count; i++)
			{
				sb.Append(" ");
			}

			return sb.ToString();
		}

		/// <summary>
		/// 在字符串後面追加空白，可填入的空白個數=count-字符串長度
		/// </summary>
		/// <param name="data">填入的資料.</param>
		/// <param name="count">字串總長度.</param>
		/// <returns></returns>
		public static string Space(string data, int count)
		{
			return Space(data, count, SpaceFormat.After, CHTCharType.DoubleByte);
		}

		/// <summary>
		/// 填入指定位置及追加空白，可填入的空白個數=count-字符串長度.
		/// </summary>
		/// <param name="data">填入的資料.</param>
		/// <param name="count">字串總長度.</param>
		/// <param name="format">空白填充位置.</param>
		/// <returns></returns>
		public static string Space(string data, int count, SpaceFormat format)
		{
			return Space(data, count, format, CHTCharType.None);
		}

		/// <summary>
		/// 在字符串後面追加空白，可填入的空白個數=count-字符串長度.
		/// </summary>
		/// <param name="data">填入的資料.</param>
		/// <param name="count">字串總長度.</param>
		/// <param name="chType">文字長度計算類型.</param>
		/// <returns></returns>
		public static string Space(string data, int count, CHTCharType chType)
		{
			return Space(data, count, SpaceFormat.After, chType);
		}

		/// <summary>
		/// 在指定位置填入空白, 可填入的空白數=count-字符串長度.
		/// </summary>
		/// <param name="data">填入的資料.</param>
		/// <param name="count">字串總長度.</param>
		/// <param name="format">空白填充位置.</param>
		/// <param name="chType">文字長度計算類型.</param>
		/// <returns></returns>
		public static string Space(string data, int count, SpaceFormat format, CHTCharType chType)
		{
			int length;

			switch (chType)
			{
				case CHTCharType.DoubleByte:
					Encoding encoding = Encoding.GetEncoding("big5");
					length = count - encoding.GetByteCount(data);
					break;
				case CHTCharType.None:
				default:
					length = count - data.Length;
					break;
			}
			string space = Space(length);

			string result = "";

			switch (format)
			{
				case SpaceFormat.After:
					result = data + space;
					break;
				case SpaceFormat.Before:
					result = space + data;
					break;
			}

			return result;
		}

		/// <summary>
		/// 資料左方補0.
		/// </summary>
		/// <param name="data">填入的資料.</param>
		/// <param name="length">字串總長度.</param>
		/// <returns></returns>
		public static string Zero(int data, int length)
		{
			string afterCsfs = data.ToString();
			int subLength = length - afterCsfs.Length;

			StringBuilder sb = new StringBuilder();
			for (int i = 0; i < subLength; i++)
			{
				sb.Append("0");
			}

			return sb.ToString() + afterCsfs;
		}

		/// <summary>
		/// 資料左方補0.
		/// </summary>
		/// <param name="data">填入的資料.</param>
		/// <param name="length">字串總長度.</param>
		/// <returns></returns>
		public static string Zero(string data, int length)
		{
			int subLength = length - data.Length;

			StringBuilder sb = new StringBuilder();
			for (int i = 0; i < subLength; i++)
			{
				sb.Append("0");
			}

			return sb.ToString() + data;
		}

		/// <summary>
		/// 取得指定文字重覆次數字串, 重覆次數會自動扣除填入的資料長度.
		/// </summary>
		/// <param name="data">填入的資料.</param>
		/// <param name="times">重覆次數.</param>
		/// <returns></returns>
		public static string RepeatString(string data, int times)
		{
			StringBuilder sb = new StringBuilder();

			for (int i = 0; i < times; i++)
			{
				sb.Append(data);
			}

			return sb.ToString();
		}

		/// <summary>
		/// 四舍五入到整數，空返回0
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static string GetNum0(string value)
		{
			if (value.Trim() == "")
			{
				return "0";
			}
			else
			{
				return System.Convert.ToDouble(value).ToString("F0");
			}
		}

		/// <summary>
		/// 四舍五入到整數，空返回空
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static string GetNum(string value)
		{
			if (value.Trim() == "")
			{
				return "";
			}
			else
			{
				return System.Convert.ToDouble(value).ToString("F0");
			}
		}

		/// <summary>
		/// 將半型字串轉換為全型字串.
		/// </summary>
		/// <param name="data">來源資料.</param>
		/// <returns></returns>
		public static string ConvertToWideString(string data)
		{
			return Strings.StrConv(data, VbStrConv.Wide, 0);
		}

		/// <summary>
		/// 轉換成字符串.
		/// </summary>
		/// <param name="value">來源資料.</param>
		/// <returns></returns>
		public static string ConvertToString(object value)
		{
			if (value == null)
			{
				return "";
			}
			else
			{
				return System.Convert.ToString(value);
			}
		}

		/// <summary>
		/// 取得指定長度(BYTE)資料.
		/// </summary>
		/// <param name="data">來源資料.</param>
		/// <param name="length">指定的長度.</param>
		/// <returns></returns>
		public static string Left(string data, int length)
		{
			Encoding encoding = Encoding.GetEncoding("big5");

			int charCount = 0;
			StringBuilder sb = new StringBuilder();

			foreach (char c in data)
			{
				string temp = c.ToString();
				charCount += encoding.GetByteCount(temp);

				if (charCount <= length)
				{
					sb.Append(temp);
				}
				else
				{
					break;
				}
			}

			return sb.ToString();
		}

		/// <summary>
		/// 取得指定長度(BYTE)資料.
		/// </summary>
		/// <param name="data">來源資料.</param>
		/// <param name="length">指定的長度.</param>
		/// <returns></returns>
		public static string Right(string data, int length)
		{
			Encoding encoding = Encoding.GetEncoding("big5");

			int charCount = 0;
			StringBuilder sb = new StringBuilder();

			for (int index = data.Length - 1; index >= 0; index--)
			{
				string temp = data[index].ToString();
				charCount += encoding.GetByteCount(temp);

				if (charCount <= length)
				{
					sb.Insert(0, temp);
				}
				else
				{
					break;
				}
			}

			return sb.ToString();
		}

		/// <summary>
		/// 去除字串中的單,雙引號.
		/// </summary>
		/// <param name="s">處理字串。</param>
		/// <returns></returns>
		public static string RemoveStringQoute(string s)
		{
			s = s.Replace("'", "");
			s = s.Replace("\"", "");

			return s;
		}

		/// <summary>
		/// 去除千分號.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static string RemoveThousandNumber(string value)
		{
			return value.Replace(",", "");
		}

		/// <summary>
		/// 增加千分號.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static string FormatThousandNumber(int value)
		{
			return value.ToString("N0");
		}

		/// <summary>
		/// 增加千分號.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static string FormatThousandNumber(int? value)
		{
			if (value == null)
			{
				return "";
			}
			else
			{
				return System.Convert.ToInt64(value).ToString("N0");
			}
		}

		/// <summary>
		/// 增加千分號.(兩位小數)
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static string FormatThousandNumber(double value)
		{
			//return value.ToString("N0");
			return value.ToString("#,##0.##");
		}

		/// <summary>
		/// 加千分位,並四舍五入到整數
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static string FormatThousandNumber0(double value)
		{
			return System.Convert.ToDouble(value.ToString("F0")).ToString("#,##0");
		}

		/// <summary>
		/// 增加千分號.(保留4位小數)
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static string FormatThousandNumberFour(decimal value)
		{
			return value.ToString("#,##0.####");
		}

		/// <summary>
		/// 增加千分號.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static string FormatThousandNumberInt64(double value)
		{
			return value.ToString("N0");
		}

		/// <summary>
		/// 增加千分號.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static string FormatThousandNumber(double? value)
		{
			if (value == null)
			{
				return "";
			}
			else
			{
				return System.Convert.ToDouble(value).ToString("N0");
			}
		}

		/// <summary>
		/// 增加千分號.整數
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static string FormatThousandNumber(decimal? value)
		{
			if (value == null)
			{
				return "0";
			}
			else
			{
				return System.Convert.ToDecimal(value).ToString("N0");
			}
		}

		/// <summary>
		/// 增加千分號.一位小數
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static string FormatThousandNumberOne(decimal? value)
		{
			if (value == null)
			{
				return "0";
			}
			else
			{
				return System.Convert.ToDecimal(value).ToString("#,##0.#");
			}
		}

		/// <summary>
		/// 增加千分號.二位小數
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static string FormatThousandNumberTwo(decimal? value)
		{
			if (value == null)
			{
				return "0";
			}
			else
			{
				return System.Convert.ToDecimal(value).ToString("#,##0.##");
			}
		}

		/// <summary>
		/// 增加千分號.二位小數
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static string FormatThousandNumberTwo2(decimal? value)
		{
			if (value.GetValueOrDefault(0).ToString().Equals("0"))
			{
				return " ";
			}
			else
			{
				return System.Convert.ToDecimal(value).ToString("#,##0.##");
			}
		}

		/// <summary>
		/// 增加千分號.三位小數
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static string FormatThousandNumberThree(decimal? value)
		{
			if (value == null)
			{
				return "0";
			}
			else
			{
				return System.Convert.ToDecimal(value).ToString("#,##0.###");
			}
		}

		/// <summary>
		/// 增加千分號.四位小數
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static string FormatThousandNumberFour(decimal? value)
		{
			if (value == null)
			{
				return "0";
			}
			else
			{
				return System.Convert.ToDecimal(value).ToString("#,##0.####");
			}
		}

		/// <summary>
		/// 增加千分號(傳入字符串)，兩位小數.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static string FormatThousandNumberTwo(string value)
		{
			if (value.Trim() == "")
			{
				return "";
			}
			else
			{
				return System.Convert.ToDecimal(value).ToString("N2");
			}
		}

		/// <summary>
		/// 增加千分號保留兩位小數.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static string FormatThousandNumberTwoDot(double value)
		{
			return value.ToString("N2");
		}

		/// <summary>
		/// 增加千分號(傳入字符串).
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static string FormatThousandNumberZero(string value)
		{
			if (string.IsNullOrEmpty(value) || value.Trim() == "")
			{
				return "";
			}
			else
			{
				return System.Convert.ToDecimal(value).ToString("N0");
			}
		}

		/// <summary>
		/// 增加千分號.(保留4位小數)
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static string FormatThousandNumberZero(decimal value)
		{
			return value.ToString("N0");
		}

		/// <summary>
		/// 增加千分號(傳入字符串，兩位小數).
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static string getFarmatNum(string value)
		{
			if (value.Trim() == "")
			{
				return "";
			}
			else
			{
				return System.Convert.ToDouble(value).ToString("#,##0.##");
			}
		}

        /// <summary>
        /// 傳入字符串，保留兩位小數.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string getFarmatNums(string value)
        {
            if (value.Trim() == "")
            {
                return "";
            }
            else
            {
                return System.Convert.ToDouble(value).ToString("###0.##");
            }
        }

		/// <summary>
		/// 增加千分位,並四舍五入到整數
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static string getFarmatNum0(string value)
		{
			if (value.Trim() == "" || value.Trim() == null)
			{
				return "";
			}
			else
			{
				return System.Convert.ToDouble(System.Convert.ToDouble(value).ToString("F0")).ToString("#,##0");
			}
		}

		/// <summary>
		/// 增加千分號(傳入字符串,兩位小數).
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static string getFarmatNum2(string value)
		{
			if (value.Trim() == "")
			{
				return "";
			}
			else
			{
				return System.Convert.ToDecimal(value).ToString("#,##0.##");
			}
		}

		/// <summary>
		/// 將指定的字串插入符合搜尋字串的起始位置.
		/// </summary>
		/// <param name="source">來源字串.</param>
		/// <param name="insert">要插入的字串.</param>
		/// <param name="match">搜尋字串.</param>
		/// <returns></returns>
		public static string InsertString(string source, string insert, string match)
		{
			int index = source.IndexOf(match);

			if (index == 0)
			{
				source = source.Insert(index, insert);
			}
			else if (index > 0)
			{
				source = source.Insert(index - 1, insert);
			}

			return source;
		}

		/// <summary>
		/// 格式化15位整數(去千分位,去最前邊0,取15位)
		/// </summary>
		/// <param name="insert"></param>
		public static string FormatInt15(string strNum)
		{
			//去千分位
			if (strNum != "")
			{
				strNum = StringHelper.RemoveThousandNumber(strNum);
			}

			//去最前邊0
			if (strNum != "")
			{
				int n = 0;

				for (int i = 0; i < strNum.Length - 1; i++)
				{
					if (strNum[i].ToString() != "0")
					{
						n = i + 1;
						break;
					}
				}

				if (n != 0)
				{
					strNum = strNum.Substring(n - 1);
				}
			}

			//超過15位時截取前15位
			if (strNum != "" && strNum.Length > 15)
			{
				strNum = strNum.Substring(0, 15);
			}

			return strNum;
		}

        /// <summary>
        /// 用Byte截長度
        /// </summary>
        /// <param name="a_SrcStr"></param>
        /// <param name="a_StartIndex"></param>
        /// <param name="a_Cnt"></param>
        /// <returns></returns>
        /// <remarks>20151211 RC --> 20151110 宏祥 APLog add</remarks>
        public static string Big5SubStr(string a_SrcStr, int a_StartIndex, int a_Cnt)
        {
            Encoding l_Encoding = Encoding.GetEncoding("big5");
            byte[] l_byte = l_Encoding.GetBytes(a_SrcStr);
            if (a_Cnt <= 0)
                return "";
            //例若長度10 
            //若a_StartIndex傳入9 -> ok, 10 ->不行 
            if (a_StartIndex + 1 > l_byte.Length)
                return "";
            else
            {
                //若a_StartIndex傳入9 , a_Cnt 傳入2 -> 不行 -> 改成 9,1 
                if (a_StartIndex + a_Cnt > l_byte.Length)
                    a_Cnt = l_byte.Length - a_StartIndex;
            }
            return l_Encoding.GetString(l_byte, a_StartIndex, a_Cnt);
        }

	}
}