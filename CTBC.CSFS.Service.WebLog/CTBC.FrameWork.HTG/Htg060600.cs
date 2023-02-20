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
    public partial class Htg060600 : HtgXmlPara
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
                                    <data id=""transactionId"" value=""060600""/>
                                    <data id=""sessionId"" value=""#sessionId#""/>
                                    <data id=""lowValue"" value=""""/>
                                  </header>
                                  <body>
                                    <data value=""N"" id=""ACTION""/>
                                    <data value=""0000000000000000"" id=""ACCT_NO""/>
                                    <data value="""" id=""LIST_TX_FR_NO""/>
                                    <data value=""01"" id=""ITM_TYPE""/>
                                    <data value=""01011991"" id=""LIST_TX_FR_DT""/>
                                    <data value=""01012030"" id=""LIST_TX_TO_DT""/>
                                    <data value=""I"" id=""ACCT_TYPE""/>
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
            datanode.Attributes["value"].Value = "060600";

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

            DataTable dt060600Master = Createdt060600Master();

            string lineflag = "";

            //主機回傳碼
            string outputcode = "";

            string _currentpageno = "1";
            //string _currentpageseq="1";

            ////string CONTROL_AREA = "";

            string totallimit = "0";


            // 由於電文的欄位名稱與DB的欄位名稱 不太相同, 因此用以下來mapping 欄位名稱
            //List<string> masterMapping = new List<string>() { "BR_NO", "BR_NO_1", "BR_NO_2", "DATA", "DATA_1", "DATA_2", "FIELD_NAME", "FIELD_NAME_1", "FIELD_NAME_2", "FILLER10_1", "FILLER10_2", "FILLER11", "FILLER12", "FILLER13", "FILLER14", "FILLER15", "FILLER16", "FILLER5_1", "FILLER5_2", "FILLER6_1", "FILLER6_2", "FILLER7_1", "FILLER7_2", "FILLER8_1", "FILLER8_2", "FILLER9_1", "FILLER9_2", "ITM_TYPE_1", "ITM_TYPE_2", "ITM_TYPE_S", "TELLER_ID", "TELLER_ID_1", "TELLER_ID_2", "TX_DATE", "TX_DATE_1", "TX_DATE_2", "TXN_NUM", "TXN_NUM_1", "TXN_NUM_2" };

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



                    //先判斷是不是最後一頁, 若是, 則新增一筆 END OF TXN
                    //可能會發生在LINE No = 4, 5, 6 所以寫了三個判斷
                    var xPathEndLine = "/hostgateway/line[@no='4']/msgBody/data[@id='DATA']";
                    XmlNode _node = xmldoc.SelectSingleNode(xPathEndLine);
                    if( _node!=null && _node.Attributes["value"].Value.ToUpper().Trim()=="END OF TXN" )
                    {
                        DataRow _dr = dt060600Master.NewRow();
                        _dr["FIELD_NAME"] = "最後一筆";
                        _dr["DATA"] = "END OF TXN";
                        dt060600Master.Rows.Add(_dr);
                    }
                    xPathEndLine = "/hostgateway/line[@no='5']/msgBody/data[@id='DATA']";
                    _node = xmldoc.SelectSingleNode(xPathEndLine);
                    if (_node != null && _node.Attributes["value"].Value.ToUpper().Trim() == "END OF TXN")
                    {
                        DataRow _dr = dt060600Master.NewRow();
                        _dr["FIELD_NAME"] = "最後一筆";
                        _dr["DATA"] = "END OF TXN";
                        dt060600Master.Rows.Add(_dr);
                    }
                    xPathEndLine = "/hostgateway/line[@no='6']/msgBody/data[@id='DATA']";
                    _node = xmldoc.SelectSingleNode(xPathEndLine);
                    if (_node != null && _node.Attributes["value"].Value.ToUpper().Trim() == "END OF TXN")
                    {
                        DataRow _dr = dt060600Master.NewRow();
                        _dr["FIELD_NAME"] = "最後一筆";
                        _dr["DATA"] = "END OF TXN";
                        dt060600Master.Rows.Add(_dr);
                    }



                    #region 若為第一頁則要insert dt060600Master
                    // 每一頁, 回傳共7筆: (不知那個人定義的規則... 真是天才...唉.)
                    // 規則是LINE=4(Type1)時, 回傳Field_NAME_1 (DATA_1), Field_NAME_2(DATA_2), Field_NAME
                    //             LINE=5(Type2)時, 回傳DATA(補充前面的值), Field_NAME_1 (DATA_1), Field_NAME_2(DATA_2)
                    //             LINE=6(Type1)時, 回傳Field_NAME_1 (DATA_1), Field_NAME_2(DATA_2)
                    Dictionary<string, string> xPathMapping = new Dictionary<string, string>(){
                        {"/hostgateway/line[@no='4']/msgBody/data[@id='FIELD_NAME']","/hostgateway/line[@no='5']/msgBody/data[@id='DATA']"},
                        {"/hostgateway/line[@no='4']/msgBody/data[@id='FIELD_NAME_1']","/hostgateway/line[@no='4']/msgBody/data[@id='DATA_1']"},
                        {"/hostgateway/line[@no='4']/msgBody/data[@id='FIELD_NAME_2']","/hostgateway/line[@no='4']/msgBody/data[@id='DATA_2']"},                        
                        {"/hostgateway/line[@no='5']/msgBody/data[@id='FIELD_NAME_1']","/hostgateway/line[@no='5']/msgBody/data[@id='DATA_1']"},
                        {"/hostgateway/line[@no='5']/msgBody/data[@id='FIELD_NAME_2']","/hostgateway/line[@no='5']/msgBody/data[@id='DATA_2']"},
                        {"/hostgateway/line[@no='6']/msgBody/data[@id='FIELD_NAME_1']","/hostgateway/line[@no='6']/msgBody/data[@id='DATA_1']"},
                        {"/hostgateway/line[@no='6']/msgBody/data[@id='FIELD_NAME_2']","/hostgateway/line[@no='6']/msgBody/data[@id='DATA_2']"}
                    };

                    

                    foreach(var x in xPathMapping)
                    {
                        

                        XmlNode _node1 = xmldoc.SelectSingleNode(x.Key);
                        XmlNode _node2 = xmldoc.SelectSingleNode(x.Value);

                        //string val = _node1.Attributes["value"].Value.ToUpper().Trim();
                        //if (!val.Trim().Equals("中文姓名")) // 如果不是中文姓名, 則不儲存在DB中
                        //    continue;                        

                        if (_node1 == null && _node2 == null) // 可能根本就沒有這個LINE行
                            continue;

                        try
                        {
                            string ke = "";
                            string val = "";

                            if( _node1!=null && _node1.Attributes["value"].Value!=null)
                                ke = _node1.Attributes["value"].Value.ToUpper().Trim();
                            if (_node2!= null && _node2.Attributes["value"].Value != null)
                                val = _node2.Attributes["value"].Value.ToUpper().Trim();

                            //if (ke.Trim().Equals("中文姓名"))    
                            {
                                DataRow _dr = dt060600Master.NewRow();
                                _dr["FIELD_NAME"] = ke;
                                _dr["DATA"] = val;
                                dt060600Master.Rows.Add(_dr);
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
                    XmlData.Tables.Add(dt060600Master);
                }
                catch (Exception exe2)
                {
                    throw exe2;
                }

                #endregion

            }

            return XmlData;

        }



        private DataTable Createdt060600Master()
        {
            DataTable dt = new DataTable("TX_60600");
            //dt.Columns.Add(new DataColumn("SNO"));
            //dt.Columns.Add(new DataColumn("cCretDT"));
            //dt.Columns.Add(new DataColumn("CaseId"));
            dt.Columns.Add(new DataColumn("BR_NO"));
            dt.Columns.Add(new DataColumn("BR_NO_1"));
            dt.Columns.Add(new DataColumn("BR_NO_2"));
            dt.Columns.Add(new DataColumn("DATA"));
            dt.Columns.Add(new DataColumn("DATA_1"));
            dt.Columns.Add(new DataColumn("DATA_2"));
            dt.Columns.Add(new DataColumn("FIELD_NAME"));
            dt.Columns.Add(new DataColumn("FIELD_NAME_1"));
            dt.Columns.Add(new DataColumn("FIELD_NAME_2"));
            dt.Columns.Add(new DataColumn("FILLER10_1"));
            dt.Columns.Add(new DataColumn("FILLER10_2"));
            dt.Columns.Add(new DataColumn("FILLER11"));
            dt.Columns.Add(new DataColumn("FILLER12"));
            dt.Columns.Add(new DataColumn("FILLER13"));
            dt.Columns.Add(new DataColumn("FILLER14"));
            dt.Columns.Add(new DataColumn("FILLER15"));
            dt.Columns.Add(new DataColumn("FILLER16"));
            dt.Columns.Add(new DataColumn("FILLER5_1"));
            dt.Columns.Add(new DataColumn("FILLER5_2"));
            dt.Columns.Add(new DataColumn("FILLER6_1"));
            dt.Columns.Add(new DataColumn("FILLER6_2"));
            dt.Columns.Add(new DataColumn("FILLER7_1"));
            dt.Columns.Add(new DataColumn("FILLER7_2"));
            dt.Columns.Add(new DataColumn("FILLER8_1"));
            dt.Columns.Add(new DataColumn("FILLER8_2"));
            dt.Columns.Add(new DataColumn("FILLER9_1"));
            dt.Columns.Add(new DataColumn("FILLER9_2"));
            dt.Columns.Add(new DataColumn("ITM_TYPE_1"));
            dt.Columns.Add(new DataColumn("ITM_TYPE_2"));
            dt.Columns.Add(new DataColumn("ITM_TYPE_S"));
            dt.Columns.Add(new DataColumn("TELLER_ID"));
            dt.Columns.Add(new DataColumn("TELLER_ID_1"));
            dt.Columns.Add(new DataColumn("TELLER_ID_2"));
            dt.Columns.Add(new DataColumn("TX_DATE"));
            dt.Columns.Add(new DataColumn("TX_DATE_1"));
            dt.Columns.Add(new DataColumn("TX_DATE_2"));
            dt.Columns.Add(new DataColumn("TXN_NUM"));
            dt.Columns.Add(new DataColumn("TXN_NUM_1"));
            dt.Columns.Add(new DataColumn("TXN_NUM_2"));
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
