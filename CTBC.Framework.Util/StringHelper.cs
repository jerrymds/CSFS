/// <summary>
/// �{������:�ƾ���ܳB�z
/// </summary>

using System.Text;
using Microsoft.VisualBasic;

namespace CTBC.FrameWork.Util
{
	/// <summary>
	/// �ťն�R��m.
	/// </summary>
	public enum SpaceFormat
	{
		/// <summary>
		/// ��ƫe��.
		/// </summary>
		Before,
		/// <summary>
		/// ��ƫ��.
		/// </summary>
		After
	}

	/// <summary>
	/// ��r���׭p������.
	/// </summary>
	public enum CHTCharType
	{
		/// <summary>
		/// ���r��(����r).
		/// </summary>
		DoubleByte,
		/// <summary>
		/// �����w, �̨t�Τ��w.
		/// </summary>
		None
	}

	/// <summary>
	/// �r��B�z�@�Τ���.
	/// </summary>
	public class StringHelper
	{
		/// <summary>
		/// ���o���w�ťխӼ�.
		/// </summary>
		/// <param name="count">�r���`����.</param>
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
		/// �b�r�Ŧ�᭱�l�[�ťաA�i��J���ťխӼ�=count-�r�Ŧ����
		/// </summary>
		/// <param name="data">��J�����.</param>
		/// <param name="count">�r���`����.</param>
		/// <returns></returns>
		public static string Space(string data, int count)
		{
			return Space(data, count, SpaceFormat.After, CHTCharType.DoubleByte);
		}

		/// <summary>
		/// ��J���w��m�ΰl�[�ťաA�i��J���ťխӼ�=count-�r�Ŧ����.
		/// </summary>
		/// <param name="data">��J�����.</param>
		/// <param name="count">�r���`����.</param>
		/// <param name="format">�ťն�R��m.</param>
		/// <returns></returns>
		public static string Space(string data, int count, SpaceFormat format)
		{
			return Space(data, count, format, CHTCharType.None);
		}

		/// <summary>
		/// �b�r�Ŧ�᭱�l�[�ťաA�i��J���ťխӼ�=count-�r�Ŧ����.
		/// </summary>
		/// <param name="data">��J�����.</param>
		/// <param name="count">�r���`����.</param>
		/// <param name="chType">��r���׭p������.</param>
		/// <returns></returns>
		public static string Space(string data, int count, CHTCharType chType)
		{
			return Space(data, count, SpaceFormat.After, chType);
		}

		/// <summary>
		/// �b���w��m��J�ť�, �i��J���ťռ�=count-�r�Ŧ����.
		/// </summary>
		/// <param name="data">��J�����.</param>
		/// <param name="count">�r���`����.</param>
		/// <param name="format">�ťն�R��m.</param>
		/// <param name="chType">��r���׭p������.</param>
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
		/// ��ƥ����0.
		/// </summary>
		/// <param name="data">��J�����.</param>
		/// <param name="length">�r���`����.</param>
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
		/// ��ƥ����0.
		/// </summary>
		/// <param name="data">��J�����.</param>
		/// <param name="length">�r���`����.</param>
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
		/// ���o���w��r���Ц��Ʀr��, ���Ц��Ʒ|�۰ʦ�����J����ƪ���.
		/// </summary>
		/// <param name="data">��J�����.</param>
		/// <param name="times">���Ц���.</param>
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
		/// �|�٤��J���ơA�Ū�^0
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
		/// �|�٤��J���ơA�Ū�^��
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
		/// �N�b���r���ഫ�������r��.
		/// </summary>
		/// <param name="data">�ӷ����.</param>
		/// <returns></returns>
		public static string ConvertToWideString(string data)
		{
			return Strings.StrConv(data, VbStrConv.Wide, 0);
		}

		/// <summary>
		/// �ഫ���r�Ŧ�.
		/// </summary>
		/// <param name="value">�ӷ����.</param>
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
		/// ���o���w����(BYTE)���.
		/// </summary>
		/// <param name="data">�ӷ����.</param>
		/// <param name="length">���w������.</param>
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
		/// ���o���w����(BYTE)���.
		/// </summary>
		/// <param name="data">�ӷ����.</param>
		/// <param name="length">���w������.</param>
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
		/// �h���r�ꤤ����,���޸�.
		/// </summary>
		/// <param name="s">�B�z�r��C</param>
		/// <returns></returns>
		public static string RemoveStringQoute(string s)
		{
			s = s.Replace("'", "");
			s = s.Replace("\"", "");

			return s;
		}

		/// <summary>
		/// �h���d����.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static string RemoveThousandNumber(string value)
		{
			return value.Replace(",", "");
		}

		/// <summary>
		/// �W�[�d����.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static string FormatThousandNumber(int value)
		{
			return value.ToString("N0");
		}

		/// <summary>
		/// �W�[�d����.
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
		/// �W�[�d����.(���p��)
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static string FormatThousandNumber(double value)
		{
			//return value.ToString("N0");
			return value.ToString("#,##0.##");
		}

		/// <summary>
		/// �[�d����,�å|�٤��J����
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static string FormatThousandNumber0(double value)
		{
			return System.Convert.ToDouble(value.ToString("F0")).ToString("#,##0");
		}

		/// <summary>
		/// �W�[�d����.(�O�d4��p��)
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static string FormatThousandNumberFour(decimal value)
		{
			return value.ToString("#,##0.####");
		}

		/// <summary>
		/// �W�[�d����.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static string FormatThousandNumberInt64(double value)
		{
			return value.ToString("N0");
		}

		/// <summary>
		/// �W�[�d����.
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
		/// �W�[�d����.���
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
		/// �W�[�d����.�@��p��
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
		/// �W�[�d����.�G��p��
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
		/// �W�[�d����.�G��p��
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
		/// �W�[�d����.�T��p��
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
		/// �W�[�d����.�|��p��
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
		/// �W�[�d����(�ǤJ�r�Ŧ�)�A���p��.
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
		/// �W�[�d�����O�d���p��.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static string FormatThousandNumberTwoDot(double value)
		{
			return value.ToString("N2");
		}

		/// <summary>
		/// �W�[�d����(�ǤJ�r�Ŧ�).
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
		/// �W�[�d����.(�O�d4��p��)
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static string FormatThousandNumberZero(decimal value)
		{
			return value.ToString("N0");
		}

		/// <summary>
		/// �W�[�d����(�ǤJ�r�Ŧ�A���p��).
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
        /// �ǤJ�r�Ŧ�A�O�d���p��.
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
		/// �W�[�d����,�å|�٤��J����
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
		/// �W�[�d����(�ǤJ�r�Ŧ�,���p��).
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
		/// �N���w���r�괡�J�ŦX�j�M�r�ꪺ�_�l��m.
		/// </summary>
		/// <param name="source">�ӷ��r��.</param>
		/// <param name="insert">�n���J���r��.</param>
		/// <param name="match">�j�M�r��.</param>
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
		/// �榡��15����(�h�d����,�h�̫e��0,��15��)
		/// </summary>
		/// <param name="insert"></param>
		public static string FormatInt15(string strNum)
		{
			//�h�d����
			if (strNum != "")
			{
				strNum = StringHelper.RemoveThousandNumber(strNum);
			}

			//�h�̫e��0
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

			//�W�L15��ɺI���e15��
			if (strNum != "" && strNum.Length > 15)
			{
				strNum = strNum.Substring(0, 15);
			}

			return strNum;
		}

        /// <summary>
        /// ��Byte�I����
        /// </summary>
        /// <param name="a_SrcStr"></param>
        /// <param name="a_StartIndex"></param>
        /// <param name="a_Cnt"></param>
        /// <returns></returns>
        /// <remarks>20151211 RC --> 20151110 ���� APLog add</remarks>
        public static string Big5SubStr(string a_SrcStr, int a_StartIndex, int a_Cnt)
        {
            Encoding l_Encoding = Encoding.GetEncoding("big5");
            byte[] l_byte = l_Encoding.GetBytes(a_SrcStr);
            if (a_Cnt <= 0)
                return "";
            //�ҭY����10 
            //�Ya_StartIndex�ǤJ9 -> ok, 10 ->���� 
            if (a_StartIndex + 1 > l_byte.Length)
                return "";
            else
            {
                //�Ya_StartIndex�ǤJ9 , a_Cnt �ǤJ2 -> ���� -> �令 9,1 
                if (a_StartIndex + a_Cnt > l_byte.Length)
                    a_Cnt = l_byte.Length - a_StartIndex;
            }
            return l_Encoding.GetString(l_byte, a_StartIndex, a_Cnt);
        }

	}
}