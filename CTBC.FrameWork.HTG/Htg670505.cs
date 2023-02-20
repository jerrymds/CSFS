using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace CTBC.FrameWork.HTG
{
    public partial class Htg670505 : HtgXmlPara
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
                                    <data id=""transactionId"" value=""067050""/>
                                    <data id=""sessionId"" value=""#sessionId#""/>
                                    <data id=""lowValue"" value=""""/>
                                  </header>
                                  <body>
                                    <data value=""CSFS"" id=""applicationId""/>
                                    <data value=""S"" id=""CUST_ID_NO""/>
                                    <data value=""5"" id=""OPTN""/>
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
            datanode.Attributes["value"].Value = "067050";

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

            DataTable dt67002 = Createdt67101();

            string lineflag = "";

            //主機回傳碼
            string outputcode = "";

            string _currentpageno = "1";
            //string _currentpageseq="1";

            ////string CONTROL_AREA = "";

            string totallimit = "0";


            // 由於電文的欄位名稱與DB的欄位名稱 不太相同, 因此用以下來mapping 欄位名稱
            Dictionary<string, string> masterMapping = new Dictionary<string, string>() { 
                {"CUSTOMER_NAME","CUST_NAME"},{"RMNAME","RM_NAME"},{"RMNO","RM_NO"},
                {"CUST_NAME","CUST_NAME"},{"RM_NAME","RM_NAME"},{"RM_NO","RM_NO"}
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

                    //20181214, 若有任何錯誤Error, 則報錯
                    var errMess = isErrorMessage(xmldoc, linedata.Count);
                    if (!string.IsNullOrEmpty(errMess))
                    {
                        strmessage += errMess;
                        break;
                    }

                    #region 若為第一頁則要insert
                    //if (linedata.Count >1)
                    //if (i == 0) // 表示第一頁
                    {

                        string line = "1";
                        {

                            DataRow _dr = dt67002.NewRow();
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
                            dt67002.Rows.Add(_dr);
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
                    XmlData.Tables.Add(dt67002);
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

                    var _xnode = xmldoc.SelectSingleNode("/hostgateway/line[@no='" + lineno + "']/msgBody/data[@id='MSG']");
                    if (_xnode != null)
                        if (!string.IsNullOrEmpty(_xnode.Attributes["value"].Value))
                            result = " 電文訊息 :" + _xnode.Attributes["value"].Value;

                    var _xnode1 = xmldoc.SelectSingleNode("/hostgateway/line[@no='" + lineno + "']/msgBody/data[@id='MSGTYPE_OC01']");
                    if (_xnode1 != null)
                        if (!string.IsNullOrEmpty(_xnode1.Attributes["value"].Value) && _xnode1.Attributes["value"].Value == "ERR")
                        {
                            var _xnode2 = xmldoc.SelectSingleNode("/hostgateway/line[@no='" + lineno + "']/msgBody/data[@id='ERRORMESSAGETEXT_OC01']");
                            result = " 電文訊息 :" + _xnode2.Attributes["value"].Value;
                        }
                }
                catch
                {
                    result = "電文格式無法辨識";
                }
            }

            return result;
        }

        private DataTable Createdt67101()
        {
            DataTable dt = new DataTable("TX_67101");
            dt.Columns.Add(new DataColumn("RM_NO"));
            dt.Columns.Add(new DataColumn("RM_NAME"));
            dt.Columns.Add(new DataColumn("CUST_NAME"));
            return dt;
        }
    }
}
