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
    public partial class Htg87016 : HtgXmlPara
    {
        public override string GetPostTransactionXml()
        {
            string strxml = @"<?xml version=""1.0""?>
                                <hostgateway>
                                  <header>
                                    <action type=""transaction""/>
                                    <data id=""application"" value=""CSFS""/>
                                    <data id=""luType"" value=""LU62""/>
                                    <data id=""transactionId"" value=""087016""/>
                                    <data id=""sessionId"" value=""#sessionId#""/>
                                    <data id=""lowValue"" value=""""/>
                                  </header>
                                  <body>
                                    <data value=""CSFS"" id=""applicationId""/>
                                    <data value=""T"" id=""INQ-CCY-TYPE""/>                                    
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
            string strxml = @"<?xml version=""1.0""?>
                                <hostgateway>
                                  <header>
                                    <action type=""transaction""/>
                                    <data id=""application"" value=""CSFS""/>
                                    <data id=""luType"" value=""LU62""/>
                                    <data id=""transactionId"" value=""087016""/>
                                    <data id=""sessionId"" value=""#sessionId#""/>
                                    <data id=""lowValue"" value=""""/>
                                  </header>
                                  <body>
                                    <data value=""CSFS"" id=""applicationId""/>
                                    <data value=""T"" id=""INQ-CCY-TYPE""/>    
                                    <data value=""USDTWDC"" id=""NEXT_KEY"" />
                                    <data id=""lowValue"" value=""true""/>
                                  </body>  
                                </hostgateway>";



            XmlDocument recxmldoc = new XmlDocument();
            XmlDocument postxmldoc = new XmlDocument();
            XmlNode header;
            XmlNodeList line;
            string strheader = "";
            string strline = "";
            string lineno = "";
            var seid = parmht["sessionId"].ToString();
            strxml = strxml.Replace("#sessionId#", seid);

            //組傳送的xml及xmldocument
            //recxmldoc.LoadXml(strxml);
            //header = recxmldoc.SelectSingleNode("/hostgateway/header");
            //strheader = recxmldoc.SelectSingleNode("/hostgateway/header").InnerXml;

            //strheader += @"<data id=""lowValue"" value=""true""/>";

            //line = recxmldoc.GetElementsByTagName("line");
            //if (line.Count == 2)
            //{ lineno = "2"; }
            //else if (line.Count == 1)
            //{
            //    XmlNode data = recxmldoc.SelectSingleNode("/hostgateway/line[@no='1']/msgBody/data[@id='outputCode']");
            //    if (data.Attributes["value"].Value == "03")
            //    {
            //        lineno = "1";
            //    }
            //}

            //strline = recxmldoc.SelectSingleNode("/hostgateway/line[@no='" + lineno + "']/msgBody").InnerXml;
            //strxml = @"<?xml version=""1.0""?><hostgateway><header>" + strheader + "</header><body>" + strline + "</body></hostgateway>";
            //postxmldoc.LoadXml(strxml);
            ////改變交易代號
            //XmlNode datanode = postxmldoc.SelectSingleNode("/hostgateway/header/data[@id='transactionId']");
            //datanode.Attributes["value"].Value = "087016";

            //傳送000401 要設定以下的值<data id="NEXT_KEY" value="USDTWDC"/>
            //datanode = postxmldoc.SelectSingleNode("/hostgateway/body/data[@id='NEXT_KEY']");
            //datanode.Attributes["value"].Value = "USDTWDC";

            //strxml = postxmldoc.OuterXml;

            return strxml;
        }

        public override DataSet TransferXmlToDataSet(ArrayList recxmldatalist, out string strmessage)
        {

            string recxmldata = "";
            strmessage = "";
            DataSet XmlData = new DataSet();
            XmlDocument xmldoc = new XmlDocument();
            XmlNodeList linedata;
            XmlNode _xmlnode;

            DataTable dt87016 = Createdt87016();

            string lineflag = "";

            //主機回傳碼
            string outputcode = "";

            string _currentpageno = "1";
            //string _currentpageseq="1";

            ////string CONTROL_AREA = "";

            string totallimit = "0";


            // 由於電文的欄位名稱與DB的欄位名稱 不太相同, 因此用以下來mapping 欄位名稱
            Dictionary<string, string> masterMapping = new Dictionary<string, string>() { 
                {"RESP_CUST_ID","RESP_CUST_ID"},{"RESP_NAME","RESP_NAME"}
            };

            try
            {
                if (recxmldatalist.Count == 0)
                {
                    throw new Exception("No xml data");
                }
                // 共有21個匯率
                // 第一頁
                Dictionary<string, string> ccyCode1 = new Dictionary<string,string>() {
                    {"2","AUD"},{"3","CAD"},{"4","CHF"},{"6","CNH"},{"8","CNY"},{"10","EUR"},{"11","GBP"},{"13","HKD"},{"14","IDR"},{"15","INR"},{"17","JPY"},{"18","KRW"},{"19","MYR"},{"20","NZD"},{"21","PHP"},{"22","SEK"},{"23","SGD"},{"24","THB"}
                              };

                // 第二頁, 只有三個匯率
                Dictionary<string, string> ccyCode2 = new Dictionary<string, string>() {
                    {"1","USD"},{"2","VND"},{"3","ZAR"}         };


                #region 逐一拆解xml資料
                for (int i = 0; i < recxmldatalist.Count; i++)
                {
                    recxmldata = recxmldatalist[i].ToString();
                    xmldoc.LoadXml(recxmldata);
                    linedata = xmldoc.GetElementsByTagName("line");
                    Dictionary<string, string> rateLookup = new Dictionary<string, string>();
                    if (i == 0)
                        rateLookup = ccyCode1;
                    if (i == 1)
                        rateLookup = ccyCode2;

                    #region 若為第一頁則要insert
                    //if (linedata.Count >1)
                    //if (i == 0) // 表示第一頁
                    {

                        string line = "1";
                        
                        foreach(KeyValuePair<string,string> ccy in rateLookup)
                        {
                            DataRow _dr = dt87016.NewRow();
                            string _ccy = ccy.Value;
                            decimal buyrate = 1.0m;
                            decimal sellrate = 1.0m;
                            string BuyRateNodeName = "BUY_RATE_" + ccy.Key;
                            string SellRateNodeName = "SELL_RATE_" + ccy.Key;
                            XmlNode buy = xmldoc.SelectSingleNode("/hostgateway/line[@no='" + line.ToString() + "']/msgBody/data[@id='" + BuyRateNodeName + "']");

                            XmlNode sell = xmldoc.SelectSingleNode("/hostgateway/line[@no='" + line.ToString() + "']/msgBody/data[@id='" + SellRateNodeName + "']");
                            if (buy != null)
                                buyrate = (decimal)(Int64.Parse(buy.Attributes["value"].Value) / 10000000.0);
                            if( sell != null)
                                sellrate = (decimal)(Int64.Parse(sell.Attributes["value"].Value) / 10000000.0);

                            _dr["CCY"] = _ccy;
                            _dr["Buy"] = buyrate;
                            _dr["Sell"] = sellrate;
                            dt87016.Rows.Add(_dr);
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
                    XmlData.Tables.Add(dt87016);
                }
                catch (Exception exe2)
                {
                    throw exe2;
                }

                #endregion

            }

            return XmlData;

        }

        /// <summary>
        /// 檢查是否有下一頁CheckIsHasNextPage
        /// </summary>
        /// <param name="xmldata"></param>
        /// <returns></returns>
        public override bool CheckIsHasNextPage(string xmldata)
        {

            // END_MARK = 'N' 表示有下一頁, 要用NEXT_KEY='USDTWDC' 再送
            bool result = false;
            string outputCode = "";
            XmlDocument xmldoc = new XmlDocument();
            //XmlNodeList header;
            XmlNodeList line;
            XmlNode data;
            xmldoc.LoadXml(xmldata);
            line = xmldoc.GetElementsByTagName("line");
            if (line.Count==1)
            {
                //只有一筆要檢查是第二次查還是第一次查
                data = xmldoc.SelectSingleNode("/hostgateway/line[@no='1']/msgBody/data[@id='END_MARK']");
                outputCode = data.Attributes["value"].Value;
            }

            if (outputCode == "N")
                result = true;

            return result;
        }


        private DataTable Createdt87016()
        {
            DataTable dt = new DataTable("TX_87016");
            dt.Columns.Add(new DataColumn("CCY"));
            //dt.Columns.Add(new DataColumn("TYPE"));
            dt.Columns.Add(new DataColumn("Buy", typeof(decimal)));
            dt.Columns.Add(new DataColumn("Sell", typeof(decimal)));


            //dt.Columns.Add(new DataColumn("TWD"));
            //dt.Columns.Add(new DataColumn("USD"));
            //dt.Columns.Add(new DataColumn("CNY"));
            //dt.Columns.Add(new DataColumn("HKD"));
            //dt.Columns.Add(new DataColumn("GBP"));
            //dt.Columns.Add(new DataColumn("JPY"));
            //dt.Columns.Add(new DataColumn("EUR"));
            //dt.Columns.Add(new DataColumn("CAD"));
            //dt.Columns.Add(new DataColumn("AUD"));
            //dt.Columns.Add(new DataColumn("NZD"));
            //dt.Columns.Add(new DataColumn("CHF"));
            //dt.Columns.Add(new DataColumn("IDR"));
            //dt.Columns.Add(new DataColumn("INR"));
            //dt.Columns.Add(new DataColumn("KRW"));
            //dt.Columns.Add(new DataColumn("MYR"));
            //dt.Columns.Add(new DataColumn("PHP"));
            //dt.Columns.Add(new DataColumn("SEK"));
            //dt.Columns.Add(new DataColumn("SGD"));
            //dt.Columns.Add(new DataColumn("THB"));
            //dt.Columns.Add(new DataColumn("ZAR"));
            //dt.Columns.Add(new DataColumn("ILS"));
            //dt.Columns.Add(new DataColumn("DKK"));
            //dt.Columns.Add(new DataColumn("PLN"));
            //dt.Columns.Add(new DataColumn("CZK"));
            //dt.Columns.Add(new DataColumn("HUF"));
            //dt.Columns.Add(new DataColumn("NOK"));
            return dt;
        }
    }
}
