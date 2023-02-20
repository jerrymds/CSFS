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
    public partial class Htg20480 : HtgXmlPara
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
                                    <data id=""transactionId"" value=""020480""/>
                                    <data id=""sessionId"" value=""#sessionId#""/>
                                    <data id=""lowValue"" value=""""/>
                                  </header>
                                  <body>
                                    <data id=""applicationId"" value=""CSFS"" />
                                    <data value=""0000000000000000"" id=""ACCT_NO""/>
                                    <data value=""1"" id=""REF_CODE""/>
                                    <data value=""9"" id=""OPTION""/>
                                    <data value="""" id=""FROM_DATE"" />
                                    <data value="""" id=""TO_DATE""/>
                                    <data value="""" id=""ACTION"" />
                                    <data value="""" id=""CONN_INST_NO""/>
                                    <data value="""" id=""CONN_ACCT_NO""/>
                                    <data value="""" id=""CONN_REF_CODE""/>
                                    <data value="""" id=""CONN_DATE_TIME""/>

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
            datanode.Attributes["value"].Value = "20480";

            //傳送20480 要設定以下的值<data id="ACTION" value="N"/>
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

            DataTable dt20480Master = Createdt20480Master();
            DataTable dtNP = NextPageMaster();

            string lineflag = "";

            //主機回傳碼
            string outputcode = "";

            string _currentpageno = "1";
            string totallimit = "0";



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

                    //20181214, 若有任何錯誤Error, 則報錯
                    var errMess = isErrorMessage(xmldoc, linedata.Count);
                    if (!string.IsNullOrEmpty(errMess))
                    {
                        strmessage += errMess;
                        break;
                    }

                    #region 若為第一頁則要insert dt20480Master
                    // 每一頁, 回傳共6筆: 
                    // 一直找到FIELD_05 1~6 中間, 有任何一個<data id="FIELD_05" value="END OF T"/>, 即可停止




                    for (int ii = 1; ii <= linedata.Count; ii++)
                    {


                        var xPathEndLine = "/hostgateway/line[@no='" + ii.ToString() + "']/msgBody/data[@id='ENDDATA']";
                        XmlNode _node = xmldoc.SelectSingleNode(xPathEndLine);
                        if (_node != null && _node.Attributes["value"].Value.ToUpper().StartsWith("END OF TXN"))
                        {
                            DataRow _dr = dt20480Master.NewRow();
                            _dr["DATA1"] = "END OF TXN";
                            //_dr["DATA2"] = "END OF TXN";
                            //_dr["DATA3"] = "END OF TXN";
                            dt20480Master.Rows.Add(_dr);
                            break;
                        }

                        var xPathEndLine1 = "/hostgateway/line[@no='" + ii.ToString() + "']/msgBody/data[@id='HEADER_MESSAGE']";
                        XmlNode _node21 = xmldoc.SelectSingleNode(xPathEndLine1);
                        if (_node21 != null )
                        { continue; }
                        XmlNode _nodeNextPage = xmldoc.SelectSingleNode("/hostgateway/line[@no='" + ii.ToString() + "']/msgBody/data[@id='CINSTNO_OC07']");
                        XmlNode _nodeNextTime = xmldoc.SelectSingleNode("/hostgateway/line[@no='" + ii.ToString() + "']/msgBody/data[@id='CDTTIME_OC07']");
                        if (_nodeNextPage != null && _nodeNextTime != null)
                        {
                           DataRow _dr = dtNP.NewRow();
                           if (_nodeNextPage != null)
                              _dr["CINSTNO_OC07"] = _nodeNextPage.Attributes["value"].Value.ToUpper().Trim();
                           if (_nodeNextTime != null)
                              _dr["CDTTIME_OC07"] = _nodeNextTime.Attributes["value"].Value.ToUpper().Trim();
                           dtNP.Rows.Add(_dr);
                        }

                        
                        try
                        {
                            for(int ij=1;ij<=6;ij++)
                            {
                                string strNode1 =string.Format( "']/msgBody/data[@id='RECEIVE_DATA1_{0}']",ij.ToString());
                                string strNode2 = string.Format("']/msgBody/data[@id='RECEIVE_DATA2_{0}']", ij.ToString());
                                XmlNode _node1 = xmldoc.SelectSingleNode("/hostgateway/line[@no='" + ii.ToString() + strNode1);
                                XmlNode _node2 = xmldoc.SelectSingleNode("/hostgateway/line[@no='" + ii.ToString() + strNode2);

                                if (_node1 != null && _node2!=null)
                                {
                                    DataRow _dr = dt20480Master.NewRow();
                                    if (_node1 != null)
                                        _dr["DATA1"] = _node1.Attributes["value"].Value.ToUpper().Trim();
                                    if (_node2 != null)
                                        _dr["DATA2"] = _node2.Attributes["value"].Value.ToUpper().Trim();
                                    string aaab = _dr["DATA1"].ToString();
                                    if (! string.IsNullOrEmpty(aaab))
                                        dt20480Master.Rows.Add(_dr);
                                }


                            }
                            //XmlNode _node1_1 = xmldoc.SelectSingleNode("/hostgateway/line[@no='" + ii.ToString() + "']/msgBody/data[@id='RECEIVE_DATA1_1']");
                            //XmlNode _node2_1 = xmldoc.SelectSingleNode("/hostgateway/line[@no='" + ii.ToString() + "']/msgBody/data[@id='RECEIVE_DATA2_1']");
                            //XmlNode _node1_2 = xmldoc.SelectSingleNode("/hostgateway/line[@no='" + ii.ToString() + "']/msgBody/data[@id='RECEIVE_DATA1_2']");
                            //XmlNode _node2_2 = xmldoc.SelectSingleNode("/hostgateway/line[@no='" + ii.ToString() + "']/msgBody/data[@id='RECEIVE_DATA2_2']");
                            //{
                            //    DataRow _dr = dt20480Master.NewRow();
                            //    if (_node1_1 != null)
                            //        _dr["DATA1"] = _node1_1.Attributes["value"].Value.ToUpper().Trim();
                            //    if (_node2_1 != null)
                            //        _dr["DATA2"] = _node2_1.Attributes["value"].Value.ToUpper().Trim();
                            //    if( _node1_1!=null && _node2_1!=null)
                            //        dt20480Master.Rows.Add(_dr);
                            //}
                            //{
                            //    DataRow _dr = dt20480Master.NewRow();
                            //    if (_node1_2 != null)
                            //        _dr["DATA1"] = _node1_2.Attributes["value"].Value.ToUpper().Trim();
                            //    if (_node2_2 != null)
                            //        _dr["DATA2"] = _node2_2.Attributes["value"].Value.ToUpper().Trim();
                            //    if (_node1_2 != null && _node2_2 != null)
                            //        dt20480Master.Rows.Add(_dr);
                            //}
                        }
                        catch (Exception ex1)
                        //欄位有錯誤則不處理
                        {
                            throw new Exception(" 20480 找不到欄位的值!!!");
                            // Nothng to do ... 
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
                throw exe;
            }
            finally
            {
                #region 資料轉換
                try
                {
                    XmlData.Tables.Add(dt20480Master);
                    XmlData.Tables.Add(dtNP);
                }
                catch (Exception exe2)
                {
                    throw exe2;
                }

                #endregion

            }

            return XmlData;

        }



        private DataTable Createdt20480Master()
        {
            DataTable dt = new DataTable("TX_20480");
            dt.Columns.Add(new DataColumn("DATA1"));
            dt.Columns.Add(new DataColumn("DATA2"));
            return dt;
        }

        private DataTable NextPageMaster()
        {
            DataTable dt = new DataTable("TX_20480_NP");
            dt.Columns.Add(new DataColumn("CINSTNO_OC07"));
            dt.Columns.Add(new DataColumn("CDTTIME_OC07"));
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
