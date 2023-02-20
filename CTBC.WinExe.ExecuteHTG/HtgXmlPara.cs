using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;
using System.Web;
using System.Xml;
using System.Data;
using System.Collections;

namespace ExcuteHTG
{
    public partial class HtgXmlPara
    {



        #region 全域變數



        #endregion

        #region 私域變數

        #endregion 


        public HtgXmlPara()
        {

        }

        /// <summary>
        /// return session of connection to htg
        /// </summary>
        /// <returns></returns>
        public string EstablishSessionXml()
        {
            string strxml = @"<?xml version=""1.0""?>
                                <hostgateway>
                                  <header>
                                    <action type=""establishSession""/>
                                  </header>
                                  <body>
                                    <data id=""userId"" value=""#userId#""/>
                                    <data id=""passWord"" value=""#passWord#""/>
                                    <data id=""branchNo"" value=""#branchNo#""/>
                                    <data id=""racfId"" value=""#racfId#""/>
                                    <data id=""racfPassWord"" value=""#racfPassWord#""/>
                                    <data id=""signOnLU0"" value=""#signOnLU0#""/>
                                    <data id=""applicationId"" value=""#applicationId#""/>
                                    <data id=""ldapInfo"" value=""yes""/>
                                  </body>
                                </hostgateway>";

            return strxml;

        }

        /// <summary>
        /// return close session xml string for htg
        /// </summary>
        /// <returns></returns>
        public string CloseSessionXml()
        {
            string strxml = @"<?xml version=""1.0""?>
                                <hostgateway>
	                                <header>
		                                <action type=""closeSession""/>
	                                </header>
	                                <body>
		                                <data id=""sessionId"" value=""#sessionId#""/>
	                                </body>
                                </hostgateway>";

            return strxml;

        }

        /// <summary>
        /// 上行xml template,其他電文須override 此function 
        /// </summary>
        /// <returns></returns>
        public virtual string GetPostTransactionXml()
        {
            return "";
        }

        /// <summary>
        /// 將上行xml template 套入參數,其他電文若有特別的處理則override 此function
        /// </summary>
        /// <param name="parmht"></param>
        /// <returns></returns>
        public virtual string GetPostTransactionXml(Hashtable parmht)
        {
            string strxml = GetPostTransactionXml();
            XmlDocument xmldoc = new XmlDocument();
            XmlNodeList header;
            XmlNodeList body;

            //取代session 
            strxml = strxml.Replace("#sessionId#", parmht["sessionId"].ToString());
            parmht.Remove("sessionId");

            //傳入值
            xmldoc.LoadXml(strxml);
            header = xmldoc.GetElementsByTagName("header");
            body = xmldoc.GetElementsByTagName("body");


            foreach (XmlNode snode in body.Item(0).ChildNodes)
            {
                if (snode.Name == "data")
                {
                    string nodeid = snode.Attributes["id"].Value;
                    if (parmht.ContainsKey(nodeid))
                    {
                        snode.Attributes["value"].Value = parmht[nodeid].ToString();
                    }
                }

            }


            strxml = xmldoc.InnerXml;

            return strxml;
        }

        /// <summary>
        /// 將傳入的xmldata 套入參數,其他電文若有特別的處理則override 此function
        /// </summary>
        /// <param name="postxml"></param>
        /// <param name="parmht"></param>
        /// <returns></returns>
        public virtual string GetPostTransactionXml(string postxml, Hashtable parmht)
        {
            return "";
        }


        /// <summary>
        /// 20181214, 檢查是否有錯誤訊息
        /// </summary>
        /// <param name="xmldoc"></param>
        /// <param name="lineno"></param>
        /// <returns></returns>
        public string isErrorMessage(XmlDocument xmldoc, int lineCount)
        {
            string result = null;
            for (int i = 1; i <= lineCount; i++)
            {
                string lineno = i.ToString();
                try
                {
                    var _xmlnode = xmldoc.SelectSingleNode("/hostgateway/line[@no='" + lineno + "']/msgBody/data[@id='ERRORMESSAGETEXT_OC01']");
                    if (_xmlnode != null)
                        result = " 電文訊息 :" + _xmlnode.Attributes["value"].Value;

                    var _xmlnode1 = xmldoc.SelectSingleNode("/hostgateway/line[@no='" + lineno + "']/msgBody/data[@id='outputCode']");
                    if (_xmlnode1 != null)
                    {
                        var outputcode = _xmlnode1.Attributes["value"].Value;
                        System.Text.RegularExpressions.Regex re = new System.Text.RegularExpressions.Regex("[A-Z|a-z]$");
                        if (re.IsMatch(outputcode))
                        {
                            var _xmlnode2 = xmldoc.SelectSingleNode("/hostgateway/line[@no='" + lineno + "']/msgBody/data[@id='MSG']");
                            result = " 電文訊息 :" + _xmlnode2.Attributes["value"].Value;
                        }
                    }
                }
                catch
                {
                    result = "電文格式無法辨識";
                }
            }

            return result;
        }


        /// <summary>
        /// 檢查是否有下一頁
        /// </summary>
        /// <param name="xmldata"></param>
        /// <returns></returns>
        public virtual bool CheckIsHasNextPage(string xmldata)
        {
            return false;
        }

        /// <summary>
        /// 是否sing on lu0,視各電文而定
        /// </summary>
        /// <returns></returns>
        public virtual bool CheckIsSignOnlu0()
        {
            return true;
        }

        /// <summary>
        /// 將回傳的xml電文轉成DataSet , 其他電文請override 此function
        /// </summary>
        /// <param name="recxml"></param>
        /// <returns></returns>
        public virtual DataSet TransferXmlToDataSet(string recxml)
        {
            return new DataSet();
        }

        /// <summary>
        /// 將回傳的xml電文轉成DataSet ,適用於多頁處理, 其他電文請override 此function
        /// </summary>
        /// <param name="recxml"></param>
        /// <returns></returns>
        public virtual DataSet TransferXmlToDataSet(ArrayList recxml, out string strmessage)
        {
            strmessage = "";
            return new DataSet();
        }

        /// <summary>
        /// 數值轉換
        /// </summary>
        /// <param name="strn"></param>
        /// <returns></returns>
        public Decimal GetDecimal(string strn)
        {
            if (strn.Length == 0)
                return 0;
            Decimal result = 0;

            if (strn.LastIndexOf("+") != -1 || strn.LastIndexOf("-") != -1)
            {
                string sign = strn.Substring(strn.Length - 1, 1);
                if (sign == "+")
                {
                    result = Convert.ToDecimal(strn.Substring(0, strn.Length - 1));
                }
                else
                {
                    result = Convert.ToDecimal(strn.Substring(0, strn.Length - 1)) * -1;
                }

            }
            else
            {
                result = Convert.ToDecimal(strn);
            }


            return result;
        }


        /// <summary>
        /// mmddyyyy轉換成yyyymmdd
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public string GetdataYYYYMMDD(string str)
        {
            if (str.Length != 0)
                return "00000000";

            string strdate = "";
            strdate = str.Substring(4, 4) + str.Substring(2, 2) + str.Substring(0, 2);
            return strdate;

        }
    }
}
