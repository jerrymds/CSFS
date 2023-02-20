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
    public partial class Htg00417 : HtgXmlPara
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
                                    <data id=""transactionId"" value=""000417""/>
                                    <data id=""sessionId"" value=""#sessionId#""/>
                                    <data id=""lowValue"" value=""""/>
                                  </header>
                                  <body>
                                    <data value=""CSFS"" id=""applicationId""/>
                                    <data value="""" id=""ACCT_NO""/>
                                    <data value=""0"" id=""STS_SEL""/>
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

            if (line.Count > 2)
                lineno = line.Count.ToString();

            strline = recxmldoc.SelectSingleNode("/hostgateway/line[@no='" + lineno + "']/msgBody").InnerXml;
            strxml = @"<?xml version=""1.0""?><hostgateway><header>" + strheader + "</header><body>" + strline + "</body></hostgateway>";
            postxmldoc.LoadXml(strxml);
            //改變交易代號
            XmlNode datanode = postxmldoc.SelectSingleNode("/hostgateway/header/data[@id='transactionId']");
            datanode.Attributes["value"].Value = "000417";

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

            DataTable dt00417 = Createdt00417();

            string lineflag = "";

            //主機回傳碼
            string outputcode = "";

            string _currentpageno = "1";
            //string _currentpageseq="1";

            ////string CONTROL_AREA = "";

            string totallimit = "0";
            bool hasLinkAccount = true;


            // 由於電文的欄位名稱與DB的欄位名稱 不太相同, 因此用以下來mapping 欄位名稱
            Dictionary<string, string> masterMapping = new Dictionary<string, string>() { 
                {"ACCT_NO_2", "Account"},{"ACCT_NO_1", "LinkAccount"}
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

                        if (outputcode == "01") // 表示沒有LinkAccount
                        {
                            _xmlnode = xmldoc.SelectSingleNode("/hostgateway/line[@no='1']/msgBody/data[@id='ERRORMESSAGETEXT_OC01']");
                            try
                            {
                                strmessage += " message :" + _xmlnode.Attributes["value"].Value; // 應該是"無此帳戶"

                                DataRow _dr = dt00417.NewRow();
                                _dr["Account"] = "";
                                _dr["LinkAccount"] = "";
                                _dr["RepMessage"] = _xmlnode.Attributes["value"].Value;
                                dt00417.Rows.Add(_dr);
                                
                            }
                            catch { }
                            
                        }

                        //if (outputcode != "03")
                        //{
                        //    //不為03可能是一開始就沒資料,也有可能是最後一頁了
                        //    _xmlnode = xmldoc.SelectSingleNode("/hostgateway/line[@no='1']/msgBody/data[@id='ERRORMESSAGETEXT_OC01']");
                        //    try {
                        //        strmessage += " message :" + _xmlnode.Attributes["value"].Value;
                        //        return null;
                        //    }
                        //    catch { }
                        //    break;
                        //}
                        lineflag = "1";
                    }
                    else
                    {
                        lineflag = "2";
                    }
                    #endregion

                    #region 



                    if (linedata.Count >= 8 && lineflag=="2")
                    //if (i == 0) // 表示第一頁
                    {
                        //寫入dt060491Master
                        DataRow _dr = dt00417.NewRow();
                        foreach (XmlNode _node in xmldoc.SelectSingleNode("/hostgateway/line[@no='8']/msgBody").ChildNodes)
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
                        dt00417.Rows.Add(_dr);

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
                    if (dt00417.Rows.Count > 0)
                    {
                        //主檔處理資料數值
                        //dt060491Master.Rows[0]["CONTROL_AREA"] = "";
                        //dt33401.Rows[0]["CurBal"] = GetDecimal(dt33401.Rows[0]["CurBal"].ToString());
                        //dt33401.Rows[0]["PbBal"] = GetDecimal(dt33401.Rows[0]["PbBal"].ToString());

                    }



                    XmlData.Tables.Add(dt00417);


                }
                catch (Exception exe2)
                {
                    throw exe2;
                }

                #endregion

            }

            return XmlData;

        }

        private DataTable Createdt00417()
        {
            DataTable dt = new DataTable("TX_00417");
            dt.Columns.Add(new DataColumn("Account"));
            dt.Columns.Add(new DataColumn("LinkAccount"));
            dt.Columns.Add(new DataColumn("RepMessage"));
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
