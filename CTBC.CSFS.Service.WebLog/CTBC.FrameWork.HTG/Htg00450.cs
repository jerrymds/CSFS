﻿using System;
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
    public partial class Htg00450 : HtgXmlPara
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
                                    <data id=""transactionId"" value=""000450""/>
                                    <data id=""sessionId"" value=""#sessionId#""/>
                                    <data id=""lowValue"" value=""""/>
                                  </header>
                                  <body>
                                    <data id=""applicationId"" value=""CSFS"" />
                                    <data value=""0000000000000000"" id=""ACCT_NO""/>
                                    <data value=""1"" id=""LIST_TX_FR_NO""/>
                                    <data value=""30"" id=""TX_TYPE""/>
                                    <data value=""TWD"" id=""CURRENCY"" />
                                    <data value=""01011991"" id=""LIST_TX_DT_FR""/>
                                    <data value=""#today#"" id=""LIST_TX_DT_TO""/>
                                  </body>
                                </hostgateway>";

            string strToday = DateTime.Now.ToString("ddMMyyyy");
            strxml = strxml.Replace("#today#", strToday);

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
            datanode.Attributes["value"].Value = "00450";

            //傳送060491 要設定以下的值<data id="ACTION" value="N"/>
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

            DataTable dt00450Master = Createdt00450Master();

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

                    #region 若為第一頁則要insert dt00450Master
                    // 每一頁, 回傳共5筆: 
                    // 一直找到LINE NO 1~6 中間, 有任何一個<data id="LAST_TXN_DATE" value="END OF T"/>, 即可停止

                    for (int ii = 1; ii <= linedata.Count; ii++)
                    {
                         
                        var xPathEndLine = "/hostgateway/line[@no='" + ii.ToString() + "']/msgBody/data[@id='LAST_TXN_DATE']";
                        XmlNode _node = xmldoc.SelectSingleNode(xPathEndLine);
                        if (_node != null && _node.Attributes["value"].Value.ToUpper().EndsWith("END OF T")) // 第6筆不會有資料, 但可能會有END OF T
                        {
                            DataRow _dr = dt00450Master.NewRow();
                            _dr["DATA1"] = "END OF TXN";
                            _dr["DATA2"] = "END OF TXN";
                            _dr["DATA3"] = "END OF TXN";
                            dt00450Master.Rows.Add(_dr);
                            break;
                        }

                        if (i == 6) continue; // 第6筆, 不會有資料, 所以直接跳走
                         
                        string xPath1 = "/hostgateway/line[@no='" + ii.ToString() + "']/msgBody/data[@id='DATA1']";
                        XmlNode _node1 = xmldoc.SelectSingleNode(xPath1);
                        XmlNode _node2 = xmldoc.SelectSingleNode("/hostgateway/line[@no='" + ii.ToString() + "']/msgBody/data[@id='DATA2']");
                        XmlNode _node3 = xmldoc.SelectSingleNode("/hostgateway/line[@no='" + ii.ToString() + "']/msgBody/data[@id='DATA3']"); 
                        try
                        {
                            
                            {      
                                DataRow _dr = dt00450Master.NewRow();
                                if( _node1!=null)
                                _dr["DATA1"] = _node1.Attributes["value"].Value.ToUpper().Trim();
                                if (_node2 != null)
                                    _dr["DATA2"] = _node2.Attributes["value"].Value.ToUpper();
                                if( _node3!=null)
                                _dr["DATA3"] = _node3.Attributes["value"].Value.ToUpper().Trim();
                                dt00450Master.Rows.Add(_dr);
                            }
                        }
                        catch (Exception ex1)
                        //欄位有錯誤則不處理
                        {
                            throw new Exception(_node1.Attributes["id"].ToString() + " 找不到!!!");
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
                    XmlData.Tables.Add(dt00450Master);
                }
                catch (Exception exe2)
                {
                    throw exe2;
                }

                #endregion

            }

            return XmlData;

        }



        private DataTable Createdt00450Master()
        {
            DataTable dt = new DataTable("TX_00450");
            dt.Columns.Add(new DataColumn("DATA1"));
            dt.Columns.Add(new DataColumn("DATA2"));
            dt.Columns.Add(new DataColumn("DATA3"));
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
