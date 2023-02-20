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
    public partial class Htg060629 : HtgXmlPara
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
                                    <data id=""transactionId"" value=""060628""/>
                                    <data id=""sessionId"" value=""#sessionId#""/>
                                    <data id=""lowValue"" value=""""/>
                                  </header>
                                  <body>
                                    <data value=""N"" id=""ACTION""/>
                                    <data value=""0000000000000000"" id=""CUST_ID_NO""/>
                                    <data value="""" id=""LIST_TX_FR_NO""/>
                                    <data value=""00"" id=""ITM_TYPE""/>
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
            datanode.Attributes["value"].Value = "060629";

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

            DataTable dt060629Master = Createdt060629Master();

            string lineflag = "";

            //主機回傳碼
            string outputcode = "";

            string _currentpageno = "1";
            //string _currentpageseq="1";

            ////string CONTROL_AREA = "";

            string totallimit = "0";


            // 由於電文的欄位名稱與DB的欄位名稱 不太相同, 因此用以下來mapping 欄位名稱
            //Dictionary<string, string> masterMapping = new Dictionary<string, string>() { { "CIF_NO", "CifNo" }, { "CUST_ID_NO", "CustId" }, { "CUSTOMER_NAME", "CustName" }, { "EFFC_DATE_DAY_01", "EffcDateDay01" }, { "EFFC_DATE_DAY_02", "EffcDateDay02" }, { "EFFC_DATE_DAY_03", "EffcDateDay03" }, { "EFFC_DATE_DAY_04", "EffcDateDay04" }, { "EFFC_DATE_DAY_05", "EffcDateDay05" }, { "EFFC_DATE_DAY_06", "EffcDateDay06" }, { "EFFC_DATE_DAY_07", "EffcDateDay07" }, { "EFFC_DATE_DAY_08", "EffcDateDay08" }, { "EFFC_DATE_DAY_09", "EffcDateDay09" }, { "EFFC_DATE_DAY_10", "EffcDateDay10" }, { "EXPI_DATE_DAY_01", "ExpiDateDay01" }, { "EXPI_DATE_DAY_02", "ExpiDateDay02" }, { "EXPI_DATE_DAY_03", "ExpiDateDay03" }, { "EXPI_DATE_DAY_04", "ExpiDateDay04" }, { "EXPI_DATE_DAY_05", "ExpiDateDay05" }, { "EXPI_DATE_DAY_06", "ExpiDateDay06" }, { "EXPI_DATE_DAY_07", "ExpiDateDay07" }, { "EXPI_DATE_DAY_08", "ExpiDateDay08" }, { "EXPI_DATE_DAY_09", "ExpiDateDay09" }, { "EXPI_DATE_DAY_10", "ExpiDateDay10" }, { "FUNC_01", "Func01" }, { "FUNC_02", "Func02" }, { "FUNC_03", "Func03" }, { "FUNC_04", "Func04" }, { "FUNC_05", "Func05" }, { "FUNC_06", "Func06" }, { "FUNC_07", "Func07" }, { "FUNC_08", "Func08" }, { "FUNC_09", "Func09" }, { "FUNC_10", "Func10" }, { "ID_DESC_01", "IdDesc01" }, { "ID_DESC_02", "IdDesc02" }, { "ID_DESC_03", "IdDesc03" }, { "ID_DESC_04", "IdDesc04" }, { "ID_DESC_05", "IdDesc05" }, { "ID_DESC_06", "IdDesc06" }, { "ID_DESC_07", "IdDesc07" }, { "ID_DESC_08", "IdDesc08" }, { "ID_DESC_09", "IdDesc09" }, { "ID_DESC_10", "IdDesc10" }, { "ID_NO_01", "IdNo01" }, { "ID_NO_02", "IdNo02" }, { "ID_NO_03", "IdNo03" }, { "ID_NO_04", "IdNo04" }, { "ID_NO_05", "IdNo05" }, { "ID_NO_06", "IdNo06" }, { "ID_NO_07", "IdNo07" }, { "ID_NO_08", "IdNo08" }, { "ID_NO_09", "IdNo09" }, { "ID_NO_10", "IdNo10" }, { "ID_TYPE", "IdType" }, { "ID_TYPE_01", "IdType01" }, { "ID_TYPE_02", "IdType02" }, { "ID_TYPE_03", "IdType03" }, { "ID_TYPE_04", "IdType04" }, { "ID_TYPE_05", "IdType05" }, { "ID_TYPE_06", "IdType06" }, { "ID_TYPE_07", "IdType07" }, { "ID_TYPE_08", "IdType08" }, { "ID_TYPE_09", "IdType09" }, { "ID_TYPE_10", "IdType10" }, { "ID_VALU_01", "IdValu01" }, { "ID_VALU_02", "IdValu02" }, { "ID_VALU_03", "IdValu03" }, { "ID_VALU_04", "IdValu04" }, { "ID_VALU_05", "IdValu05" }, { "ID_VALU_06", "IdValu06" }, { "ID_VALU_07", "IdValu07" }, { "ID_VALU_08", "IdValu08" }, { "ID_VALU_09", "IdValu09" }, { "ID_VALU_10", "IdValu10" }, { "IND_01", "Ind01" }, { "IND_02", "Ind02" }, { "IND_03", "Ind03" }, { "IND_04", "Ind04" }, { "IND_05", "Ind05" }, { "IND_06", "Ind06" }, { "IND_07", "Ind07" }, { "IND_08", "Ind08" }, { "IND_09", "Ind09" }, { "IND_10", "Ind10" }, { "PONT", "Pont" } };

            List<string> dbField = new List<string>() { "AC_MARK_1", "AC_MARK_10", "AC_MARK_11", "AC_MARK_12", "AC_MARK_2", "AC_MARK_3", "AC_MARK_4", "AC_MARK_5", "AC_MARK_6", "AC_MARK_7", "AC_MARK_8", "AC_MARK_9", "ACTION", "BIRTH_DT_1", "BIRTH_DT_10", "BIRTH_DT_11", "BIRTH_DT_12", "BIRTH_DT_2", "BIRTH_DT_3", "BIRTH_DT_4", "BIRTH_DT_5", "BIRTH_DT_6", "BIRTH_DT_7", "BIRTH_DT_8", "BIRTH_DT_9", "CUST_ID_NO", "CUST_NAME_1", "CUST_NAME_10", "CUST_NAME_11", "CUST_NAME_12", "CUST_NAME_2", "CUST_NAME_3", "CUST_NAME_4", "CUST_NAME_5", "CUST_NAME_6", "CUST_NAME_7", "CUST_NAME_8", "CUST_NAME_9", "ID_DATA_1", "ID_DATA_10", "ID_DATA_11", "ID_DATA_12", "ID_DATA_2", "ID_DATA_3", "ID_DATA_4", "ID_DATA_5", "ID_DATA_6", "ID_DATA_7", "ID_DATA_8", "ID_DATA_9" };
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



                    #region 若為第一頁則要insert dt060629Master
                    //if (linedata.Count == 2)  //表示第一頁
                    // (i == 0) // 表示第一頁
                    {
                        //寫入dt060491Master
                        DataRow _dr = dt060629Master.NewRow();
                        foreach (XmlNode _node in xmldoc.SelectSingleNode("/hostgateway/line[@no='1']/msgBody").ChildNodes)
                        {
                            try
                            {
                                var eFieldName = _node.Attributes["id"].Value.ToUpper().Trim();
                                if (dbField.Contains(eFieldName.ToUpper()))
                                {
                                    _dr[eFieldName] = _node.Attributes["value"].Value.Trim();
                                }
                                //bool isMasterField = masterMapping.ContainsKey(eFieldName);
                                //if (isMasterField) // 找出對映DB的欄位名稱
                                //{
                                //    string dbFieldName = masterMapping[eFieldName];
                                //    _dr[dbFieldName] = _node.Attributes["value"].Value;

                                //}
                            }
                            catch (Exception ex1)
                            //欄位有錯誤則不處理
                            {
                                throw new Exception(_node.Attributes["id"].ToString() + " 找不到!!!");
                            }
                        }
                        dt060629Master.Rows.Add(_dr);

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
                    //                    if (dt067100Master.Rows.Count > 0)
                    //                    {
                    //                        //主檔處理資料數值
                    //                        //dt060491Master.Rows[0]["CONTROL_AREA"] = "";
                    ////                        dt067100Master.Rows[0]["CifNo"] = GetDecimal(dt067100Master.Rows[0]["CifNo"].ToString());
                    //                        dt067100Master.Rows[0]["CifNo"] = Convert.ToInt32(dt067100Master.Rows[0]["CifNo"].ToString());

                    //                    }



                    XmlData.Tables.Add(dt060629Master);


                }
                catch (Exception exe2)
                {
                    throw exe2;
                }
                    
                #endregion

            }

            return XmlData;

        }



        private DataTable Createdt060629Master()
        {
            DataTable dt = new DataTable("TX_60629");
            dt.Columns.Add(new DataColumn("AC_MARK_1"));
            dt.Columns.Add(new DataColumn("AC_MARK_10"));
            dt.Columns.Add(new DataColumn("AC_MARK_11"));
            dt.Columns.Add(new DataColumn("AC_MARK_12"));
            dt.Columns.Add(new DataColumn("AC_MARK_2"));
            dt.Columns.Add(new DataColumn("AC_MARK_3"));
            dt.Columns.Add(new DataColumn("AC_MARK_4"));
            dt.Columns.Add(new DataColumn("AC_MARK_5"));
            dt.Columns.Add(new DataColumn("AC_MARK_6"));
            dt.Columns.Add(new DataColumn("AC_MARK_7"));
            dt.Columns.Add(new DataColumn("AC_MARK_8"));
            dt.Columns.Add(new DataColumn("AC_MARK_9"));
            dt.Columns.Add(new DataColumn("ACTION"));
            dt.Columns.Add(new DataColumn("BIRTH_DT_1"));
            dt.Columns.Add(new DataColumn("BIRTH_DT_10"));
            dt.Columns.Add(new DataColumn("BIRTH_DT_11"));
            dt.Columns.Add(new DataColumn("BIRTH_DT_12"));
            dt.Columns.Add(new DataColumn("BIRTH_DT_2"));
            dt.Columns.Add(new DataColumn("BIRTH_DT_3"));
            dt.Columns.Add(new DataColumn("BIRTH_DT_4"));
            dt.Columns.Add(new DataColumn("BIRTH_DT_5"));
            dt.Columns.Add(new DataColumn("BIRTH_DT_6"));
            dt.Columns.Add(new DataColumn("BIRTH_DT_7"));
            dt.Columns.Add(new DataColumn("BIRTH_DT_8"));
            dt.Columns.Add(new DataColumn("BIRTH_DT_9"));
            dt.Columns.Add(new DataColumn("CUST_ID_NO"));
            dt.Columns.Add(new DataColumn("CUST_NAME_1"));
            dt.Columns.Add(new DataColumn("CUST_NAME_10"));
            dt.Columns.Add(new DataColumn("CUST_NAME_11"));
            dt.Columns.Add(new DataColumn("CUST_NAME_12"));
            dt.Columns.Add(new DataColumn("CUST_NAME_2"));
            dt.Columns.Add(new DataColumn("CUST_NAME_3"));
            dt.Columns.Add(new DataColumn("CUST_NAME_4"));
            dt.Columns.Add(new DataColumn("CUST_NAME_5"));
            dt.Columns.Add(new DataColumn("CUST_NAME_6"));
            dt.Columns.Add(new DataColumn("CUST_NAME_7"));
            dt.Columns.Add(new DataColumn("CUST_NAME_8"));
            dt.Columns.Add(new DataColumn("CUST_NAME_9"));
            dt.Columns.Add(new DataColumn("ID_DATA_1"));
            dt.Columns.Add(new DataColumn("ID_DATA_10"));
            dt.Columns.Add(new DataColumn("ID_DATA_11"));
            dt.Columns.Add(new DataColumn("ID_DATA_12"));
            dt.Columns.Add(new DataColumn("ID_DATA_2"));
            dt.Columns.Add(new DataColumn("ID_DATA_3"));
            dt.Columns.Add(new DataColumn("ID_DATA_4"));
            dt.Columns.Add(new DataColumn("ID_DATA_5"));
            dt.Columns.Add(new DataColumn("ID_DATA_6"));
            dt.Columns.Add(new DataColumn("ID_DATA_7"));
            dt.Columns.Add(new DataColumn("ID_DATA_8"));
            dt.Columns.Add(new DataColumn("ID_DATA_9"));
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
