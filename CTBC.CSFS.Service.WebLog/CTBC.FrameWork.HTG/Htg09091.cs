using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Xml;
using System.Linq;
using System.Linq.Expressions;

namespace CTBC.FrameWork.HTG
{
    public partial class Htg09091 : HtgXmlPara
    {
        #region 全域變數



        #endregion

        #region 私域變數

        #endregion


        /// <summary>
        /// get xml data for transaction 090012 
        /// </summary>
        /// <returns></returns>
        public override string GetPostTransactionXml()
        {
            string strxml = @"<?xml version=""1.0""?>
                                <hostgateway>
                                  <header>
                                    <action type=""transaction""/>
                                    <data id=""application"" value=""CSFS""/>
                                    <data id=""luType"" value=""LU62""/>
                                    <data id=""transactionId"" value=""009091""/>
                                    <data id=""sessionId"" value=""#sessionId#""/>
                                    <data id=""lowValue"" value=""""/>
                                  </header>
                                  <body>
                                    <data value=""CSFS"" id=""applicationId""/>
                                    <data value=""S"" id=""ACCT_NO""/>
                                    <data value=""TWD"" id=""CURRENCY""/>
                                    <data value=""S"" id=""DTSRC_DATE"" />
                                    <data value=""Memo"" id=""STOP_RESN_DESC"" />
                                    <data value=""0"" id=""FUNCTION"" />
                                    <data value=""4"" id=""STOP_RESN_CODE"" />
                                    <data id=""WRITTEN"" value="""" />
                                    <data id=""FILLER5"" value="""" />
                                    <data id=""FILLER6"" value="""" />
                                    <data id=""PROMO_NO"" value="""" />
                                    <data id=""VIRTUAL_ACCT_NO"" value="""" />
                                    <data id=""BULLETIN_TYPE"" value="""" />
                                    <data id=""DUE_DATE"" value="""" />
                                  </body>
                                </hostgateway>";



            return strxml;
        }




        /// <summary>
        /// get postxml data from receive xml data
        /// </summary>
        /// <param name="recxml"></param>
        /// <param name="parmht"></param>
        /// <returns></returns>
        public override string GetPostTransactionXml(string recxml, Hashtable parmht)
        {
            string strxml = "";
            XmlDocument recxmldoc = new XmlDocument();
            XmlDocument postxmldoc = new XmlDocument();
            XmlNode header;
            XmlNodeList line;
            string strheader = "";
            string strline = "";
            string lineno = "";

            //組傳送的xml及xmldocument
            recxmldoc.LoadXml(recxml);
            header = recxmldoc.SelectSingleNode("/hostgateway/header");
            strheader = recxmldoc.SelectSingleNode("/hostgateway/header").InnerXml;

            strheader += @"<data id=""lowValue"" value=""true""/>";

            line = recxmldoc.GetElementsByTagName("line");
            if (line.Count == 2)
            { lineno = "2"; }
            else if (line.Count == 1)
            {
                XmlNode data = recxmldoc.SelectSingleNode("/hostgateway/line[@no='1']/msgBody/data[@id='outputCode']");
                if (data.Attributes["value"].Value == "03")
                {
                    lineno = "1";
                }
            }

            strline = recxmldoc.SelectSingleNode("/hostgateway/line[@no='" + lineno + "']/msgBody").InnerXml;
            strxml = @"<?xml version=""1.0""?><hostgateway><header>" + strheader + "</header><body>" + strline + "</body></hostgateway>";
            postxmldoc.LoadXml(strxml);
            //改變交易代號
            XmlNode datanode = postxmldoc.SelectSingleNode("/hostgateway/header/data[@id='transactionId']");
            datanode.Attributes["value"].Value = "09091";

            //傳送000401 要設定以下的值<data id="ACTION" value="N"/>
            datanode = postxmldoc.SelectSingleNode("/hostgateway/body/data[@id='ACTION']");
            datanode.Attributes["value"].Value = "N";

            strxml = postxmldoc.OuterXml;

            return strxml;
        }

        /// <summary>
        /// transfer receive xml data to dataset
        /// </summary>
        /// <param name="recxmldatalist"></param>
        /// <param name="strmessage"></param>
        /// <returns></returns>
        public override DataSet TransferXmlToDataSet(ArrayList recxmldatalist, out string strmessage)
        {

            string recxmldata = "";
            strmessage = "";
            DataSet XmlData = new DataSet();
            XmlDocument xmldoc = new XmlDocument();
            XmlNodeList linedata;
            XmlNode _xmlnode;

            DataTable dt09091 = Createdt09091();

            string lineflag = "";

            //主機回傳碼
            string outputcode = "";

            string _currentpageno = "1";
            //string _currentpageseq="1";

            ////string CONTROL_AREA = "";

            string totallimit = "0";


            // 由於電文的欄位名稱與DB的欄位名稱 不太相同, 因此用以下來mapping 欄位名稱
            Dictionary<string, string> masterMapping = new Dictionary<string, string>() { 
                {"Account","Account"},{"RESPONSE_OC08","RESPONSE_OC08"},{"CURRENCY_OC08","CURRENCY_OC08"},{"CMTRASH_OC08","CMTRASH_OC08"},{"DESCRIP_OC08","DESCRIP_OC08"}
            };

            try
            {
                if (recxmldatalist.Count == 0)
                {
                    throw new Exception("No xml data");
                }

                #region 逐一拆解xml資料
                for (int i = 0; i < recxmldatalist.Count; i++)
                {
                    recxmldata = recxmldatalist[i].ToString();
                    xmldoc.LoadXml(recxmldata);
                    linedata = xmldoc.GetElementsByTagName("line");

                    #region 檢查第一頁是否有錯誤,或是否為最後一頁
                    if (linedata.Count == 1)
                    {
                        //表示第一頁只一筆,即為錯誤
                        _xmlnode = xmldoc.SelectSingleNode("/hostgateway/line[@no='1']/msgBody/data[@id='outputCode']");
                        outputcode = _xmlnode.Attributes["value"].Value;

                        if (outputcode != "03")
                        {
                            //不為03可能是一開始就沒資料,也有可能是最後一頁了
                            _xmlnode = xmldoc.SelectSingleNode("/hostgateway/line[@no='1']/msgBody/data[@id='ERRORMESSAGETEXT_OC01']");
                            try { strmessage += " message :" + _xmlnode.Attributes["value"].Value; }
                            catch { }
                            break;
                        }
                        lineflag = "1";
                    }
                    else
                    {
                        lineflag = "2";
                    }
                    #endregion

                    #region 若為第一頁則要insert 
                    if (linedata.Count >1)
                    //if (i == 0) // 表示第一頁
                    {
                        //寫入dt060491Master
                        for (int line = 2; line <= linedata.Count; line++) // 因為有時會在LINE=3 , 有時在LINE=4.. 所以就全掃描
                        {

                            var hasElment = xmldoc.SelectSingleNode("/hostgateway/line[@no='" + line.ToString() + "']/msgBody/data[@id='RESPONSE_OC08']");

                            if (hasElment == null)
                                continue;
                            DataRow _dr = dt09091.NewRow();
                            foreach (XmlNode _node in xmldoc.SelectSingleNode("/hostgateway/line[@no='" + line.ToString() + "']/msgBody").ChildNodes)
                            {
                                try
                                {
                                    var eFieldName = _node.Attributes["id"].Value.ToUpper().Trim();

                                    bool isMasterField = masterMapping.ContainsKey(eFieldName);
                                    if (isMasterField) // 找出對映DB的欄位名稱
                                    {
                                        string dbFieldName = masterMapping[eFieldName];
                                        _dr[dbFieldName] = _node.Attributes["value"].Value;
                                    }
                                }
                                catch (Exception ex1)
                                //欄位有錯誤則不處理
                                {
                                    throw new Exception(_node.Attributes["id"].ToString() + " 找不到!!!");
                                }
                            }
                            dt09091.Rows.Add(_dr);
                        }
                    }

                    #endregion


                }

                #endregion
            }
            catch (Exception exe)
            {
                //throw exe;
                strmessage += exe.ToString();
                //throw exe;
            }
            finally
            {
                #region 資料轉換
                try
                {

                    XmlData.Tables.Add(dt09091);


                }
                catch (Exception exe2)
                {
                    throw exe2;
                }

                #endregion

            }

            return XmlData;

        }

        private DataTable Createdt09091()
        {
            DataTable dt = new DataTable("TX_09091");
            dt.Columns.Add(new DataColumn("Account"));
            dt.Columns.Add(new DataColumn("RESPONSE_OC08"));
            dt.Columns.Add(new DataColumn("CURRENCY_OC08"));
            dt.Columns.Add(new DataColumn("CMTRASH_OC08"));
            dt.Columns.Add(new DataColumn("DESCRIP_OC08"));
            return dt;

            return dt;
        }


        /// <summary>
        /// 檢查是否有下一頁CheckIsHasNextPage
        /// </summary>
        /// <param name="xmldata"></param>
        /// <returns></returns>
        public override bool CheckIsHasNextPage(string xmldata)
        {
            bool result = false;
            string outputCode = "";
            XmlDocument xmldoc = new XmlDocument();
            //XmlNodeList header;
            XmlNodeList line;
            XmlNode data;
            xmldoc.LoadXml(xmldata);
            line = xmldoc.GetElementsByTagName("line");
            if (line.Count > 1)
            {
                data = xmldoc.SelectSingleNode("/hostgateway/line[@no='2']/msgBody/data[@id='outputCode']");
                outputCode = data.Attributes["value"].Value;
            }
            else
            {
                //只有一筆要檢查是第二次查還是第一次查
                data = xmldoc.SelectSingleNode("/hostgateway/line[@no='1']/msgBody/data[@id='outputCode']");
                outputCode = data.Attributes["value"].Value;
            }

            if (outputCode == "03")
                result = true;

            return result;
        }

        /// <summary>
        /// 是否sing on lu0,062072要sign lu0
        /// </summary>
        /// <returns></returns>
        public override bool CheckIsSignOnlu0()
        {
            return true;
        }
    }
}
