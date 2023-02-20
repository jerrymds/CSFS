using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;
using System.Web;
using System.Xml;
using System.Data;
using System.Collections;


namespace CTCB.NUMS.Library.HTG 
{
    public partial class Htg062072 : HtgXmlPara
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
                                    <data id=""applicationId"" value=""default""/>
                                    <data id=""luType"" value=""LU0""/>
                                    <data id=""transactionId"" value=""062072""/>
                                    <data id=""sessionId"" value=""#sessionId#""/>
                                  </header>
                                  <body>
                                    <!-- ACTION -->
                                    <data id=""ACTION"" value="" ""/>
                                    <!-- 統一編號 -->
                                    <data id=""CUST_ID_NO"" value=""A123456789""/>
                                    <!-- 客戶編號 -->
                                    <data id=""CUST_NO"" value=""0000000000000000""/>
                                    <!-- 外匯選項 -->
                                    <data id=""TXTR"" value=""N""/>
                                    <!-- 查詢類別 -->
                                    <data id=""OPTION"" value=""0""/>
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
            datanode.Attributes["value"].Value = "067072";

            //傳送067072 要設定以下的值<data id="ACTION" value="N"/>
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
            DataTable dt067072Master = Createdt067072Master();
            DataTable dt067072Detail = Createdt067072Detail();

            string lineflag = "";

            //主機回傳碼
            string outputcode = "";

            string _currentpageno = "1";
            //string _currentpageseq="1";

            string CONTROL_AREA = "";

            string totallimit = "0";

            string[] detfield = { "AMT", "APPDAT", "BALAMT", "BRANC", "CURR", "DBBTYP", "EXPDAT", "HOLD_FLAG", "KF_FLAG", "LIMNO", "PRODU", "SIGN", "STATUS", "STOPCD" };

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

                    #region 若為第一頁則要insert dt067072Master
                    if (linedata.Count == 2)
                    {
                        //寫入dt067072Master
                        DataRow _dr = dt067072Master.NewRow();
                        foreach (XmlNode _node in xmldoc.SelectSingleNode("/hostgateway/line[@no='2']/msgBody").ChildNodes)
                        {
                            try
                            {
                                _dr[_node.Attributes["id"].Value] = _node.Attributes["value"].Value;
                            }
                            catch (Exception ex1)//欄位有錯誤則不處理
                            { }
                        }
                        dt067072Master.Rows.Add(_dr);

                    }
                    #endregion

                    #region 處理dt67072的資料

                    //取出最後的total limit amt
                    _xmlnode = xmldoc.SelectSingleNode("/hostgateway/line[@no='" + lineflag + "']/msgBody/data[@id='TOT_LIMIT']");
                    totallimit = (_xmlnode == null) ? "0" : _xmlnode.Attributes["value"].Value;

                    //_xmlnode = xmldoc.SelectSingleNode("/hostgateway/line[@no='" + lineflag + "']/msgBody");

                    _xmlnode = xmldoc.SelectSingleNode("/hostgateway/line[@no='" + lineflag + "']/msgBody/data[@id='CONTROL_AREA']");
                    //取出目前是第幾頁
                    _currentpageno = (i + 1).ToString();
                    //判斷CONTROL_AREA
                    CONTROL_AREA = (_xmlnode == null) ? "" : _xmlnode.Attributes["value"].Value;
                    //記銀是否有F
                    string[] controlf = new string[7];

                    #region 取出每一筆的CONTROL_AREA
                    if (CONTROL_AREA.Length > 0)
                    {

                        for (int idx = 0; idx < controlf.Length; idx++)
                        {
                            try
                            {
                                controlf[idx] = CONTROL_AREA.Substring(idx * 2, 1);
                            }
                            catch (Exception ex2)
                            { controlf[idx] = ""; }

                        }

                    }

                    #endregion

                    //
                    #region  每一頁有7筆,所以作7次

                    for (int idx = 1; idx <= 7; idx++)
                    {
                        DataRow _detr = dt067072Detail.NewRow();
                        foreach (string sield in detfield)
                        {
                            try
                            {
                                _detr[sield] = xmldoc.SelectSingleNode("/hostgateway/line[@no='"
                                + lineflag + "']/msgBody/data[@id='" + sield + idx.ToString() + "']").Attributes["value"].Value;
                            }
                            catch (Exception ex3)
                            { }
                        }

                        _detr["CurrentPageNo"] = _currentpageno;
                        _detr["CurrentPageSeqNo"] = idx;
                        _detr["FACILITY"] = controlf[idx - 1];
                        if (_detr["AMT"].ToString().Trim().Length != 0 && _detr["BALAMT"].ToString().Trim().Length != 0)
                            dt067072Detail.Rows.Add(_detr);
                    }

                    #endregion





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
                    if (dt067072Master.Rows.Count > 0)
                    {
                        //主檔處理資料數值
                        dt067072Master.Rows[0]["CONTROL_AREA"] = "";
                        dt067072Master.Rows[0]["CARD_AVAIL"] = GetDecimal(dt067072Master.Rows[0]["CARD_AVAIL"].ToString());
                        dt067072Master.Rows[0]["AMOUNT"] = GetDecimal(dt067072Master.Rows[0]["AMOUNT"].ToString());
                        dt067072Master.Rows[0]["CARD_BORROW_AMT"] = GetDecimal(dt067072Master.Rows[0]["CARD_BORROW_AMT"].ToString());
                        dt067072Master.Rows[0]["CARD_LIMIT"] = GetDecimal(dt067072Master.Rows[0]["CARD_LIMIT"].ToString());
                        dt067072Master.Rows[0]["CARD_TMP_LIM"] = GetDecimal(dt067072Master.Rows[0]["CARD_TMP_LIM"].ToString());
                        dt067072Master.Rows[0]["CUST_AVAIL"] = GetDecimal(dt067072Master.Rows[0]["CUST_AVAIL"].ToString());
                        dt067072Master.Rows[0]["CUST_LIMIT"] = GetDecimal(dt067072Master.Rows[0]["CUST_LIMIT"].ToString());
                        dt067072Master.Rows[0]["LOAN_BAL"] = GetDecimal(dt067072Master.Rows[0]["LOAN_BAL"].ToString());
                        dt067072Master.Rows[0]["MAIL_LOAN_AMT"] = GetDecimal(dt067072Master.Rows[0]["MAIL_LOAN_AMT"].ToString());
                        dt067072Master.Rows[0]["LOAN_BAL"] = GetDecimal(dt067072Master.Rows[0]["LOAN_BAL"].ToString());
                        dt067072Master.Rows[0]["MAIL_LOAN_BAL"] = GetDecimal(dt067072Master.Rows[0]["MAIL_LOAN_BAL"].ToString());
                        dt067072Master.Rows[0]["MALU_H_AVL"] = GetDecimal(dt067072Master.Rows[0]["MALU_H_AVL"].ToString());
                        dt067072Master.Rows[0]["MALU_H_LIM"] = GetDecimal(dt067072Master.Rows[0]["MALU_H_LIM"].ToString());
                        dt067072Master.Rows[0]["TOT_LIMIT"] = GetDecimal(dt067072Master.Rows[0]["TOT_LIMIT"].ToString());
                    }

                    XmlData.Tables.Add(dt067072Master);

                    //處理明細檔
                    if (dt067072Detail.Rows.Count > 0)
                    {
                        for (int idn = 0; idn < dt067072Detail.Rows.Count; idn++)
                        {
                            dt067072Detail.Rows[idn]["AMT"] = GetDecimal(dt067072Detail.Rows[idn]["AMT"].ToString());
                            dt067072Detail.Rows[idn]["BALAMT"] = GetDecimal(dt067072Detail.Rows[idn]["BALAMT"].ToString());
                            dt067072Detail.Rows[idn]["APPDAT"] = GetdataYYYYMMDD(dt067072Detail.Rows[idn]["APPDAT"].ToString());
                            dt067072Detail.Rows[idn]["EXPDAT"] = GetdataYYYYMMDD(dt067072Detail.Rows[idn]["EXPDAT"].ToString());
                        }


                    }
                    XmlData.Tables.Add(dt067072Detail);                
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
            bool result = false;
            string outputCode="";
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

        /// <summary>
        /// set up data container
        /// </summary>
        /// <returns></returns>
        private DataTable Createdt067072Detail()
        {
            DataTable dt = new DataTable("dt067072Detail");
            dt.Columns.Add(new DataColumn("AMT"));
            dt.Columns.Add(new DataColumn("APPDAT"));
            dt.Columns.Add(new DataColumn("BALAMT"));
            dt.Columns.Add(new DataColumn("BRANC"));
            dt.Columns.Add(new DataColumn("CURR"));
            dt.Columns.Add(new DataColumn("DBBTYP"));
            dt.Columns.Add(new DataColumn("EXPDAT"));
            dt.Columns.Add(new DataColumn("HOLD_FLAG"));
            dt.Columns.Add(new DataColumn("KF_FLAG"));
            dt.Columns.Add(new DataColumn("LIMNO"));
            dt.Columns.Add(new DataColumn("PRODU"));
            dt.Columns.Add(new DataColumn("SIGN"));
            dt.Columns.Add(new DataColumn("STATUS"));
            dt.Columns.Add(new DataColumn("STOPCD"));
            //資料是位於第n頁
            dt.Columns.Add(new DataColumn("CurrentPageNo"));
            //資料是位於該頁的第n筆
            dt.Columns.Add(new DataColumn("CurrentPageSeqNo"));
            //該帳號是否為F
            dt.Columns.Add(new DataColumn("FACILITY"));

            return dt;

        }

        /// <summary>
        /// set up data container
        /// </summary>
        /// <returns></returns>
        private DataTable Createdt067072Master()
        {
            DataTable dt = new DataTable("dt067072Master");
            dt.Columns.Add(new DataColumn("ACTION"));
            dt.Columns.Add(new DataColumn("AMOUNT"));
            dt.Columns.Add(new DataColumn("AMT_OPT"));
            dt.Columns.Add(new DataColumn("CARD_AVAIL"));
            dt.Columns.Add(new DataColumn("CARD_BORROW_AMT"));
            dt.Columns.Add(new DataColumn("CARD_CONN"));
            dt.Columns.Add(new DataColumn("CARD_LIMIT"));
            dt.Columns.Add(new DataColumn("CARD_PAY"));
            dt.Columns.Add(new DataColumn("CARD_STATUS"));
            dt.Columns.Add(new DataColumn("CARD_TMP_LIM"));
            dt.Columns.Add(new DataColumn("CASH_CARD_CODE"));
            dt.Columns.Add(new DataColumn("CLEAR_STEP"));
            dt.Columns.Add(new DataColumn("CONSULT_STATUS"));
            dt.Columns.Add(new DataColumn("CONTRB_CODE"));
            dt.Columns.Add(new DataColumn("CONTROL_AREA"));
            dt.Columns.Add(new DataColumn("CORP_GRP"));
            dt.Columns.Add(new DataColumn("CUST_AVAIL"));
            dt.Columns.Add(new DataColumn("CUST_ID_NO"));
            dt.Columns.Add(new DataColumn("CUST_LIMIT"));
            dt.Columns.Add(new DataColumn("CUST_NO"));
            dt.Columns.Add(new DataColumn("CUST_TITLE"));
            dt.Columns.Add(new DataColumn("CUSTOMER_NAME"));
            dt.Columns.Add(new DataColumn("DISCLOS_AGRMT"));
            dt.Columns.Add(new DataColumn("FB_AO_BRANCH"));
            dt.Columns.Add(new DataColumn("FB_AO_NAME"));
            dt.Columns.Add(new DataColumn("FX_TR"));
            dt.Columns.Add(new DataColumn("LOAN_BAL"));
            dt.Columns.Add(new DataColumn("MAIL_LOAN_AMT"));
            dt.Columns.Add(new DataColumn("MAIL_LOAN_BAL"));
            dt.Columns.Add(new DataColumn("MALU_H_AVL"));
            dt.Columns.Add(new DataColumn("MALU_H_LIM"));
            dt.Columns.Add(new DataColumn("OPTION"));
            dt.Columns.Add(new DataColumn("outputCode"));
            dt.Columns.Add(new DataColumn("PAGE_CNT"));
            dt.Columns.Add(new DataColumn("PRE_APPR"));
            dt.Columns.Add(new DataColumn("RANK"));
            dt.Columns.Add(new DataColumn("REL2_ID"));
            dt.Columns.Add(new DataColumn("REL2_NAME"));
            dt.Columns.Add(new DataColumn("REL2_TITLE"));
            dt.Columns.Add(new DataColumn("RELA_ID"));
            dt.Columns.Add(new DataColumn("RELA_NAME"));
            dt.Columns.Add(new DataColumn("RELA_TITLE"));
            dt.Columns.Add(new DataColumn("REST_STATUS"));
            dt.Columns.Add(new DataColumn("SELECT"));
            dt.Columns.Add(new DataColumn("TIME"));
            dt.Columns.Add(new DataColumn("TOT_LIMIT"));
            dt.Columns.Add(new DataColumn("VIP_DEGREE"));
            dt.Columns.Add(new DataColumn("WSMT_STATUS"));
            return dt;

        }

    }


}
