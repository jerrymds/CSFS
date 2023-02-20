/// <summary>
/// 常用對字符串操作
/// </summary>

using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Sockets;
using CTBC.CSFS.Resource;

namespace CTBC.FrameWork.Util
{
    public class UtlString
    {
        #region 全域變數

        #endregion

        #region 屬性設置(Get,Set)

        #endregion

        #region Public Method

        /// <summary>
        /// 取得字串左邊部分
        /// </summary>
        /// <param name="psSource">來源字串</param>
        /// <param name="piLength">取出長度</param>
        /// <returns>回傳字串</returns>
        /// <remarks>add by sky 2011/12/08</remarks>
        public static string Left(string psSource, int piLength)
        {
            // 是否大於字串的長度
            if (piLength > psSource.Length)
            {
                piLength = psSource.Length;
            }

            return psSource.Substring(0, piLength);
        }

        /// <summary>
        /// 取得字符串
        /// </summary>
        /// <param name="objValue">源</param>
        /// <returns>字符串</returns>
        /// <remarks>add by sky 2011/12/08</remarks>
        public static string GetString(object objValue)
        {
            string strValue = "";

            // 如果非空
            if (objValue != null)
            {
                // 不為數據空時
                if (objValue != DBNull.Value)
                {
                    strValue = objValue.ToString().Trim();
                }
            }

            return strValue;
        }

        /// <summary>
        /// 獲取某個obj對象，非空的時候轉換成空對象
        /// </summary>
        /// <param name="obj">object對象</param>
        /// <returns></returns>
        public static string GetStrEverNull(object obj)
        {
            if (obj == null)
            {
                return string.Empty;
            }
            else
            {
                return obj.ToString();
            }
        }
        /// <summary>
        /// 連接路徑字串, 不論結尾是否含有目錄分隔符號
        /// </summary>
        /// <param name="psRootPath">根路徑</param>
        /// <param name="psSubPath">子路徑</param>
        /// <returns>完整路徑</returns>
        /// <remarks>add by sky 2011/12/08</remarks>
        public static string JoinPath(string psRootPath, string psSubPath)
        {
            // 如果有‘\’就返回
            if (psRootPath[psRootPath.Length - 1] == Path.DirectorySeparatorChar)
            {
                return psRootPath + psSubPath;
            }
            else
            {
                return psRootPath + Path.DirectorySeparatorChar + psSubPath;
            }
        }

        /// <summary>
        /// 組合字符串
        /// </summary>
        /// <param name="strings">字符串數組</param>
        /// <returns>字符串</returns>
        /// <remarks>add by sky 2011/12/08</remarks>
        public static String JoinString(params string[] strings)
        {
            return String.Join(String.Empty, strings);
        }

        public static double GetDouble(object objValue)
        {
            double dblValue = 0;

            double.TryParse(GetString(objValue), out dblValue);
            return dblValue;
        }

        /// <summary>
        /// 依分隔符號切割字串
        /// </summary>
        /// <param name="psSource">來源字串</param>
        /// <param name="psSeparator">分隔符號</param>
        /// <returns>回傳分割後的字串陣列</returns>
        public static string[] Split(string psSource, string psSeparator)
        {
            char[] delimiter = psSeparator.ToCharArray();
            return psSource.Split(delimiter);
        }

        /// <summary>
        /// 取得字串右邊部分
        /// </summary>
        /// <param name="psSource">來源字串</param>
        /// <param name="piLength">取出長度</param>
        /// <returns>回傳字串</returns>
        public static string Right(string psSource, int piLength)
        {
            int iStart = psSource.Length - piLength;
            if (iStart < 1) iStart = 0;
            if (piLength > psSource.Length) piLength = psSource.Length;
            return psSource.Substring(iStart, piLength);
        }

        /// <summary>
        /// 取得字串的後半段文字
        /// </summary>
        /// <param name="psSource">來源字串</param>
        /// <param name="piStartPos">起始字元位置(由1開始)</param>
        /// <returns>回傳字串</returns>
        public static string Mid(string psSource, int piStartPos)
        {
            int iLength = psSource.Length - piStartPos + 1;
            return Mid(psSource, piStartPos, iLength);
        }

        /// <summary>
        /// 取得字串的某段文字
        /// </summary>
        /// <param name="psSource">來源字串</param>
        /// <param name="piStartPos">起始字元位置(由1開始)</param>
        /// <param name="piLength">長度</param>
        /// <returns>回傳字串</returns>
        public static string Mid(string psSource, int piStartPos, int piLength)
        {
            if (psSource.Length == 0) return "";
            if (piStartPos > psSource.Length) return "";
            if (piLength > psSource.Length - piStartPos) piLength = psSource.Length - piStartPos + 1;
            if (piStartPos < 1) piStartPos = 1;
            return psSource.Substring(piStartPos - 1, piLength);
        }

        /// <summary>
        /// 格式化字串
        /// </summary>
        /// <param name="poSource">數值或字串</param>
        /// <param name="psFormat">格式字串</param>
        /// <returns>回值格式化字串</returns>
        public static string Format(object poSource, string psFormat)
        {
            return String.Format(@"{0:" + psFormat + "}", poSource);
        }

        /// <summary>
        /// 格式化日期時間字串
        /// </summary>
        /// <param name="poDateTime">日期時間物件</param>
        /// <param name="psFormat">格式字串</param>
        /// <returns>回值格式化日期時間字串</returns>
        public static string Format(DateTime poDateTime, string psFormat)
        {
            return poDateTime.ToString(psFormat, DateTimeFormatInfo.InvariantInfo);
        }

        /// <summary>
        /// 格式化今日為西元年日期 (yyyy/MM/dd)
        /// </summary>
        /// <returns>西元年日期字串</returns>
        public static string FormatDate()
        {
            return FormatDate(DateTime.Now);
        }

        /// <summary>
        /// 格式化西元年日期 (yyyy/MM/dd)
        /// </summary>
        /// <param name="poDateTime">日期時間物件</param>
        /// <returns>西元年日期字串</returns>
        public static string FormatDate(DateTime poDateTime)
        {
            return Format(poDateTime, "yyyy/MM/dd");
        }

        /// <summary>
        /// 格式化今日為西元年日期 (yyyy?MM?dd)
        /// </summary>
        /// <param name="psSeparator">分隔符號 (可為空白)</param>
        /// <returns>西元年日期字串</returns>
        public static string FormatDate(string psSeparator)
        {
            return FormatDate(DateTime.Now, psSeparator);
        }

        /// <summary>
        /// 格式化西元年日期 (yyyy?MM?dd)
        /// </summary>
        /// <param name="poDateTime">日期時間物件</param>
        /// <param name="psSeparator">分隔符號 (可為空白)</param>
        /// <returns>西元年日期字串</returns>
        public static string FormatDate(DateTime poDateTime, string psSeparator)
        {
            if (psSeparator.Length == 0)
                return Format(poDateTime, "yyyyMMdd");
            else
                return Format(poDateTime, "yyyy" + psSeparator + "MM" + psSeparator + "dd");
        }

        /// <summary>
        /// 格式化目前時間 (HH:mm:ss)
        /// </summary>
        /// <returns>時間字串(24小時制)</returns>
        public static string FormatTime()
        {
            return FormatTime(DateTime.Now);
        }

        /// <summary>
        /// 格式化時間 (HH:mm:ss)
        /// </summary>
        /// <param name="poDateTime">日期時間物件</param>
        /// <returns>時間字串(24小時制)</returns>
        public static string FormatTime(DateTime poDateTime)
        {
            return Format(poDateTime, "HH:mm:ss");
        }

        /// <summary>
        /// 格式化目前時間為西元日期時間 (yyyy/MM/dd HH:mm:ss)
        /// </summary>
        /// <returns>西元日期時間字串</returns>
        public static string FormatDateTime()
        {
            return FormatDateTime(DateTime.Now);
        }

        /// <summary>
        /// 格式化西元日期時間 (yyyy/MM/dd HH:mm:ss)
        /// </summary>
        /// <param name="poDateTime">日期時間物件</param>
        /// <returns>西元日期時間字串</returns>
        public static string FormatDateTime(DateTime poDateTime)
        {
            return Format(poDateTime, "yyyy/MM/dd HH:mm:ss");
        }

        /// <summary>
        /// 格式化今日為西元日期時間
        /// </summary>
        /// <param name="psSeparator">日期分隔符號 (可為空白)</param>
        /// <returns>西元日期時間字串</returns>
        public static string FormatDateTime(string psSeparator)
        {
            return FormatDateTime(DateTime.Now, psSeparator);
        }

        /// <summary>
        /// 格式化西元日期時間
        /// </summary>
        /// <param name="poDateTime">日期時間物件</param>
        /// <param name="psSeparator">日期分隔符號 (可為空白)</param>
        /// <returns>西元日期時間字串</returns>
        public static string FormatDateTime(DateTime poDateTime, string psSeparator)
        {
            return FormatDate(poDateTime, psSeparator) + " " + FormatTime(poDateTime);
        }

        /// <summary>
        /// 格式化目前時間為西元日期時間 (無分隔符號 yyyyMMddHHmmss)
        /// </summary>
        /// <returns>西元日期時間字串(無間隔)</returns>
        public static string FormatDateTimeNoSep()
        {
            return FormatDateTimeNoSep(DateTime.Now);
        }

        /// <summary>
        /// 格式化西元日期時間 (無分隔符號 yyyyMMddHHmmss)
        /// </summary>
        /// <param name="poDateTime">日期時間物件</param>
        /// <returns></returns>
        public static string FormatDateTimeNoSep(DateTime poDateTime)
        {
            return Format(poDateTime, "yyyyMMddHHmmss");
        }

        /// <summary>
        /// 格式化今日為民國年日期 (yyy/MM/dd)
        /// </summary>
        /// <returns>民國年日期字串</returns>
        public static string FormatLocalDate()
        {
            return FormatLocalDate(DateTime.Now);
        }

        /// <summary>
        /// 格式化民國年日期 (yyy/MM/dd)
        /// </summary>
        /// <param name="poDateTime">日期時間物件</param>
        /// <returns>民國年日期字串</returns>
        public static string FormatLocalDate(DateTime poDateTime)
        {
            return FormatLocalDate(poDateTime, "/");
        }

        /// <summary>
        /// 格式化今日為民國年日期 (yyy?MM?dd)
        /// </summary>
        /// <param name="psSeparator">分隔符號(可為空白)</param>
        /// <returns>民國年日期字串</returns>
        public static string FormatLocalDate(string psSeparator)
        {
            return FormatLocalDate(DateTime.Now, psSeparator);
        }

        /// <summary>
        /// 格式化民國年日期 (yyy?MM?dd)
        /// </summary>
        /// <param name="poDateTime">日期時間物件</param>
        /// <param name="psSeparator">分隔符號(可為空白)</param>
        /// <returns>民國年日期字串</returns>
        public static string FormatLocalDate(DateTime poDateTime, string psSeparator)
        {
            return
            String.Format(@"{0:000}", poDateTime.Year - 1911) + psSeparator +
            String.Format(@"{0:00}", poDateTime.Month) + psSeparator +
            String.Format(@"{0:00}", poDateTime.Day);
        }

        /// <summary>
        /// 將20140925格式化為2014/09/25
        /// </summary>
        /// <param name="date">8位長度的日期</param>
        /// <param name="sperator">分隔符，默認為/</param>
        /// <returns></returns>
        public static string FormatDateString(string date, string sperator = "/")
        {
            if (!string.IsNullOrEmpty(date) && date.Trim().Length == 8)
            {
                return date.Substring(0, 4) + sperator + date.Substring(4, 2) + sperator + date.Substring(6);
            }
            else
                return date;
        }

        /// <summary>
        /// 將2014/09/25的公元時間轉換成民國日期(yyy/MM/dd)
        /// </summary>
        /// <returns></returns>
        public static string FormatDateTw(string dateStr)
        {
            dateStr = dateStr.Replace("/", "");
            if (dateStr.Length == 8)
            {
                string year = (Convert.ToInt32(dateStr.Substring(0, 4)) - 1911).ToString();
                string monthDay = dateStr.Substring(4, 4);
                return dateStr = year + "/" + monthDay.Substring(0, 2) + "/" + monthDay.Substring(2, 2);
            }
            return dateStr;
        }

        /// <summary>
        /// 將104/08/01 格式化為西元年
        /// </summary>
        /// <param name="dateTw"></param>
        /// <param name="sperator"></param>
        /// <returns></returns>
        public static string FormatDateTwStringToAd(string dateTw, char sperator = '/')
        {
            if (string.IsNullOrEmpty(dateTw))
                return "";
            string[] list = dateTw.Split(sperator);
            int year = Convert.ToInt32(list[0]);
            if (year <= 200)
                year = year + 1911;
            list[0] = Convert.ToString(year);
            list[1] = ("00" + list[1]).Substring(list[1].Length + 2 - 2);
            list[2] = ("00" + list[2]).Substring(list[2].Length + 2 - 2);
            return list[0] + sperator + list[1] + sperator + list[2];
        }
        /// <summary>
        /// 將民國年 轉換為西元年日期
        /// </summary>
        /// <param name="dtTw"></param>
        /// <returns></returns>
        public static DateTime FormatDateTwStringToAd(DateTime dtTw)
        {
            if (dtTw.Year < 200)
            {
                dtTw = dtTw.AddYears(1911);
            }
            return dtTw;
        }

        public static string GetLastSix(string hideString)
        {
            if (hideString.Length < 2) return hideString;
            string hideSymbol = string.Empty;
            string strResult = string.Empty;
            if (hideString.Length < 6)
            {
                for (int i = 0; i < hideString.Length - 2; i++)
                {
                    hideSymbol += "*";
                }
                strResult = hideSymbol + hideString.Substring(hideString.Length - 2, 2);
            }
            else
            {
                string strSub = hideString.Substring(hideString.Length - 6, 6);
                strSub = strSub.Replace(strSub.Substring(0, 4), "****");
                strResult = hideString.Substring(0, hideString.Length - 6) + strSub;
            }
            return strResult;
        }

        public static string GetFirstLetter(string hideString)
        {
            if (hideString.Length <= 0) return "";
            string hideSymbol = string.Empty;
            string strHide = hideString.Substring(0, 1);
            if (hideString.Length - 1 > 0)
            {
                for (int i = 0; i < hideString.Length - 1; i++)
                {
                    hideSymbol += "*";
                }
            }
            return strHide + hideSymbol;
        }

        /// <summary>
        /// 將包含AM\PM的時間轉換為24小時時間
        /// </summary>
        /// <param name="date">8位長度的日期</param>
        /// <param name="sperator">分隔符，默認為/</param>
        /// <returns></returns>
        public static string FormatAMPMTimeString(string time)
        {
            if (!string.IsNullOrEmpty(time))
            {
                string time2 = time.Trim();
                if (time2.EndsWith("AM"))
                {
                    time2 = time2.TrimEnd(new char[] { ' ', 'A', 'M' });
                }
                else if (time2.EndsWith("PM"))
                {
                    time2 = time2.TrimEnd(new char[] { ' ', 'P', 'M' });
                    time2 = (Convert.ToInt16(time2.Split(':')[0]) + 12) + ":" + time2.Split(':')[1];
                }
                return time2;
            }
            return time;
        }

        /// <summary>
        /// 檢查字串是否為數值
        /// </summary>
        /// <param name="psCheckString">檢查字串</param>
        /// <returns>True/False</returns>
        /// <remarks>字串為空白或是Null時回傳False</remarks>
        public static bool IsNumeric(string psCheckString)
        {
            if (psCheckString == null)
                return false;
            if (psCheckString.Length == 0)
                return false;
            char[] cTemp = psCheckString.ToCharArray();
            for (int k = 0; k < cTemp.Length; k++)
                if (!Char.IsNumber(cTemp[k]))
                    return false;
            return true;
        }

        public static string EncodeBase64(string result)
        {
            string strValue = "";
            try
            {
                if (result != "")
                {
                    byte[] bytes = System.Text.Encoding.UTF8.GetBytes(result);
                    strValue = Convert.ToBase64String(bytes);
                    strValue = strValue.Replace("+", "%2B");
                }
            }
            catch
            {
                strValue = "";
            }
            return strValue;
        }

        public static string DecodeBase64(string result)
        {
            string strValue = "";
            try
            {
                if (result != "")
                {
                    result = result.Replace("%2B", "+");
                    byte[] bytes = Convert.FromBase64String(result);
                    strValue = System.Text.Encoding.UTF8.GetString(bytes);
                    if (strValue.Length == 0)
                        strValue = result;
                }
            }
            catch
            {
                strValue = result;
            }
            return strValue;
        }

        /// <summary>
        /// 產生隨機連接字符串
        /// </summary>
        /// <param name="codeCount"></param>
        /// <returns></returns>
        public static string CreateRandomCode(int codeCount)
        {
            string allChar = "0,1,2,3,4,5,6,7,8,9,a,b,c,d,e,f,g,h,i,j,k,l,m,n,o,p,q,r,s,t,u,v,w,x,y,z,A,B,C,D,E,F,G,H,I,J,K,L,M,N,O,P,Q,R,S,T,U,V,W,X,Y,Z";
            string[] allCharArray = allChar.Split(',');
            int len = allCharArray.Length;
            string randomCode = "";
            int temp = -1;

            Random rand = new Random();
            for (int i = 0; i < codeCount; i++)
            {
                if (temp != -1)
                {
                    rand = new Random(i * temp * ((int)DateTime.Now.Ticks));
                }
                int t = rand.Next(len);
                if (temp == t)
                {
                    return CreateRandomCode(codeCount);
                }
                temp = t;
                randomCode += allCharArray[t];
            }
            return randomCode;

        }


        /// <summary>
        /// 對字符串按字節進行截取與填補
        /// </summary>
        /// <param name="i">要求字節總長</param>
        /// <param name="str">目標字符串</param>
        /// <param name="space">填補字符</param>
        /// <returns>返回要求字符串</returns>
        public static string GetEnCodeStr(int i, string str, char space = ' ')
        {
            if (string.IsNullOrEmpty(str))
            {
                return " ".PadRight(i);
            }
            byte[] bs = System.Text.Encoding.Default.GetBytes(str);
            if (bs.Length >= i)
            {
                return System.Text.Encoding.Default.GetString(bs, 0, i);
            }
            else
            {
                return str.PadRight(i - bs.Length + str.Length, space);
            }
        }

        /// <summary>
        ///將字符串格式化為帶飛千分位的字符串 (eg. "9785.335"→"9,785.33")
        /// </summary>
        /// <param name="strPriceValue">Digital</param>
        /// <param name="FractionalDigit">Decimal places of Rounding</param>
        /// <returns></returns>
        public static string FormatCurrency(Decimal PriceValue, int fractionalDigit)
        {
            try
            {
                string strPriceValue = PriceValue.ToString("F" + fractionalDigit.ToString());
                string retValue = "";
                if (strPriceValue.Split('.').Length > 1)
                {
                    retValue = "." + strPriceValue.Split('.')[1];
                }
                strPriceValue = strPriceValue.Split('.')[0];
                if (strPriceValue.IndexOf("-") < 0)
                {
                    while (strPriceValue.Length > 3)
                    {
                        retValue = "," + strPriceValue.Substring(strPriceValue.Length - 3, 3) + retValue;
                        strPriceValue = strPriceValue.Substring(0, strPriceValue.Length - 3);
                    }
                }
                else
                {
                    strPriceValue = strPriceValue.Substring(1);
                    while (strPriceValue.Length > 3)
                    {
                        retValue = "," + strPriceValue.Substring(strPriceValue.Length - 3, 3) + retValue;
                        strPriceValue = strPriceValue.Substring(0, strPriceValue.Length - 3);
                    }
                    strPriceValue = "-" + strPriceValue;
                }
                retValue = strPriceValue + retValue;
                return retValue;
            }
            catch (Exception)
            {
                return PriceValue.ToString();
            }
        }

        /// <summary>
        ///將字符串格式化為帶飛千分位的字符串 (eg. "9785.335"→"9,78.33")
        /// </summary>
        /// <param name="strPriceValue">Digital string</param>
        /// <param name="FractionalDigit">Decimal places of Rounding</param>
        /// <returns></returns>
        public static string FormatCurrency(string strPriceValue, int fractionalDigit)
        {
            try
            {
                Decimal PriceValue;
                if (!Decimal.TryParse(strPriceValue, out PriceValue))
                {
                    return strPriceValue;
                }
                strPriceValue = PriceValue.ToString("F" + fractionalDigit.ToString());
                string retValue = "";
                if (strPriceValue.Split('.').Length > 1)
                {
                    retValue = "." + strPriceValue.Split('.')[1];
                }
                strPriceValue = strPriceValue.Split('.')[0];
                if (strPriceValue.IndexOf("-") < 0)
                {
                    while (strPriceValue.Length > 3)
                    {
                        retValue = "," + strPriceValue.Substring(strPriceValue.Length - 3, 3) + retValue;
                        strPriceValue = strPriceValue.Substring(0, strPriceValue.Length - 3);
                    }
                }
                else
                {
                    strPriceValue = strPriceValue.Substring(1);
                    while (strPriceValue.Length > 3)
                    {
                        retValue = "," + strPriceValue.Substring(strPriceValue.Length - 3, 3) + retValue;
                        strPriceValue = strPriceValue.Substring(0, strPriceValue.Length - 3);
                    }
                    strPriceValue = "-" + strPriceValue;
                }
                retValue = strPriceValue + retValue;
                return retValue;
            }
            catch (Exception)
            {
                return strPriceValue;
            }
        }

        //獲取IP地址
        public static string GetHostIP()
        {
            string ipAddress = "";
            IPHostEntry myEntry = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in myEntry.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetworkV6)
                {
                    continue;
                }
                else
                {
                    ipAddress = ip.ToString();
                }
            }
            return ipAddress;
        }

        /// <summary>
        /// 從字節數組中獲取篤定長度字節的字符串
        /// </summary>
        /// <param name="data"></param>
        /// <param name="index"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string GetByteString(byte[] data, int index, int length)
        {
            if (length >= data.Length || index < 0 || index > data.Length - 1)
            {
                return string.Empty;
            }
            else
            {
                return System.Text.Encoding.Default.GetString(data, index, length);
            }
        }

        /// <summary>
        /// 通過19930301這樣的生日日期求年齡
        /// </summary>
        /// <param name="birthday">生日形如19930301</param>
        /// <returns></returns>
        public static int GetAge(string birthday)
        {
            if (birthday.Length == 8)
            {
                int biryear = Convert.ToInt32(birthday.Substring(0, 4));
                int birmonth = Convert.ToInt32(birthday.Substring(4, 2));
                int birday = Convert.ToInt32(birthday.Substring(6, 2));
                int nowyear = Convert.ToInt32(DateTime.Now.Year);
                int nowmonth = Convert.ToInt32(DateTime.Now.Month);
                int nowday = Convert.ToInt32(DateTime.Now.Day);
                int age = nowyear - biryear;
                if (nowmonth < birmonth || (nowmonth == birmonth && nowday < birday))
                {
                    age = age - 1;
                }
                return age;
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// 對decimal,float,double數據類型進行補空白
        /// </summary>
        /// <param name="decimaldata"></param>
        /// <returns></returns>
        public static string PointNumFillPad(string decimaldata, int totalLen)
        {
            if (decimaldata.Length > totalLen)
            {
                return decimaldata.Substring(0, totalLen);
            }
            else
            {
                decimaldata = decimaldata.Trim();
                if (decimaldata.IndexOf('-') == 0 || decimaldata.IndexOf('+') == 0)
                {
                    char mark = Convert.ToChar(decimaldata.Substring(0, 1));
                    decimaldata.Substring(1, decimaldata.Length - 1);
                    decimaldata = DecimalFillingPad(decimaldata, totalLen - 1);
                    decimaldata = mark + decimaldata;
                }
                else
                {
                    decimaldata = DecimalFillingPad(decimaldata, totalLen);
                }
                return decimaldata;
            }
        }

        public static string DecimalFillingPad(string decimaldata, int totolLen)
        {
            if (decimaldata.IndexOf('.') == 2 && Convert.ToChar(decimaldata.Substring(0, 1)) == '0')
            {
                decimaldata = decimaldata.PadRight(totolLen - decimaldata.Length, '0');
            }
            else
            {
                decimaldata = decimaldata.PadLeft(totolLen - decimaldata.Length, '0');
            }
            return decimaldata;
        }

        #endregion

        #region Private Method

        #endregion

        /// <summary>
        /// 檢核身份證字號是否合法
        /// </summary>
        /// <param name="sID">身份證字號</param>
        /// <param name="sMsg">空字符串表示驗證通過，不為空表示錯誤信息</param>
        /// <returns></returns>
        public static bool CheckID(string sID, ref string sMsg)
        {

            int[] aryTmp = new int[9];
            int iResult = 0;
            string sArea = "";
            string sTmpID = "";
            int iChkSum = 0;
            string tmpID = "";
            DateTime tmpDT;
            //check 長度
            if (string.IsNullOrEmpty(sID) || sID.Length != 10)
            {
                sMsg = Lang.csfs_ID + Lang.csfs_length_error;
                return false;
            }


            switch (sID.Substring(0, 1))
            {
                case "1":
                    //外國人,檢查前碼為有效年月日;後碼為英文
                    tmpID = sID.Substring(0, 4) + "/" + sID.Substring(4, 2) + "/" + sID.Substring(6, 2);
                    if (!DateTime.TryParse(tmpID, out tmpDT))
                    {
                        sMsg = Lang.csfs_id_foreigner_error;
                        return false;
                    }
                    if (char.Parse(sID.Substring(8, 1)) < 65 || char.Parse(sID.Substring(8, 1)) > 90 || char.Parse(sID.Substring(9, 1)) < 65 || char.Parse(sID.Substring(9, 1)) > 90)
                    {
                        sMsg = Lang.csfs_id_foreigner_error2;
                        return false;
                    }
                    return true;
                case "2":
                    //外國人,檢查前碼為有效年月日;後碼為英文
                    tmpID = sID.Substring(0, 4) + "/" + sID.Substring(4, 2) + "/" + sID.Substring(6, 2);
                    if (!DateTime.TryParse(tmpID, out tmpDT))
                    {
                        sMsg = Lang.csfs_id_foreigner_error;
                        return false;
                    }
                    if (char.Parse(sID.Substring(8, 1)) < 65 || char.Parse(sID.Substring(8, 1)) > 90 || char.Parse(sID.Substring(9, 1)) < 65 || char.Parse(sID.Substring(9, 1)) > 90)
                    {
                        sMsg = Lang.csfs_id_foreigner_error2;
                        return false;
                    }
                    return true;

                default:
                    //本國人
                    if (sID.Substring(1, 1) != "1" && sID.Substring(1, 1) != "2")
                    {
                        sMsg = Lang.csfs_id_sex_error;
                        return false;
                    }

                    if ("ABCDEFGHJKLMNPQRSTUVXYWZIO".IndexOf(sID.Substring(0, 1)) < 0)
                    {
                        sMsg = Lang.csfs_id_error;
                        return false;
                    }

                    if (!int.TryParse(sID.Substring(1), out iResult))
                    {
                        sMsg = Lang.csfs_id_error2;
                        return false;
                    }

                    //英文轉數字
                    int i = "ABCDEFGHJKLMNPQRSTUVXYWZIO".IndexOf(sID.Substring(0, 1)) + 10;
                    sArea = i.ToString();
                    sTmpID = sArea + sID.Substring(1);

                    //將第碼-第碼轉依序* 權重
                    iChkSum = int.Parse(sTmpID.Substring(0, 1));
                    for (int n = 1; n <= 9; n++)
                    {
                        iChkSum = iChkSum + int.Parse(sTmpID.Substring(n, 1)) * (10 - n);
                    }
                    iChkSum = (10 - iChkSum % 10) % 10;

                    if (iChkSum.ToString() == sTmpID.Substring(sTmpID.Length - 1, 1))
                    {
                        sMsg = "";
                        return true;
                    }
                    else
                    {
                        sMsg = Lang.csfs_id_error3;
                        return false;
                    }
            }

        }


        /// <summary>
        /// 得到本周三
        /// 週一~週三 = 本週三
        /// 週四~     = 下週三
        /// </summary>
        /// <returns></returns>
        public static DateTime GetWednesday()
        {
            int[] aryNextDays = { 3, 2, 1, 0, 6, 5, 4, 3 };
            DateTime nextDate = DateTime.Today.AddDays(aryNextDays[Convert.ToInt32(DateTime.Today.DayOfWeek.ToString("d"))]);
            return nextDate;
        }

        /// <summary>
        /// 得到PayDay本周三
        /// 週一~週三 = 本週三
        /// 週四~     = 下週三
        /// 這種算法主要用於 "扣押並支付" 的週三算法
        /// </summary>
        /// <returns></returns>
        public static DateTime GetCheckDateForSeizureAndPay(DateTime baseDate)
        {
            baseDate = baseDate.Date;
            int[] aryNextDays = { 3, 2, 1, 0, 6, 5, 4, 3 };
            DateTime nextDate = baseDate.AddDays(aryNextDays[Convert.ToInt32(baseDate.DayOfWeek.ToString("d"))]);
            return nextDate;
        }
        /// <summary>
        /// 取得當前paydate的支票日期
        /// 週一  = 本周三
        /// 週二~ = 下週三
        /// 這種算法主要用於 "支付" 的週三算法
        /// </summary>
        /// <param name="baseDate"></param>
        /// <returns></returns>
        public static DateTime GetCheckDate(DateTime baseDate)
        {
            baseDate = baseDate.Date;
            int[] aryNextDays = { 3, 2, 8, 7, 6, 5, 4, 3 };
            DateTime nextDate = baseDate.AddDays(aryNextDays[Convert.ToInt32(baseDate.DayOfWeek.ToString("d"))]);
            return nextDate;
        }
        public static DateTime GetCheckDate(DateTime baseDate,int st ,int en ,int NextD)
        {
            //select * FROM PARMCode  where CodeType = 'CheckDate_Setup'
 
            baseDate = baseDate.Date;
            int[] aryNextDays = { 3, 9, 8, 7, 6, 5, 4, 3 };
            for (int i=0;i<8;i++)
            {
                aryNextDays[i] = NextD+7-i;
            }
            // 設定每周幾開票

            DateTime nextDate = baseDate.AddDays(aryNextDays[Convert.ToInt32(baseDate.DayOfWeek.ToString("d"))]);
            return nextDate;
        }

    }
}
