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
    public partial class Htg067073 : HtgXmlPara
    {
        #region 全域變數



        #endregion

        #region 私域變數

        #endregion



        /// <summary>
        /// get postxml data from receive xml data
        /// </summary>
        /// <param name="postxml"></param>
        /// <param name="parmht"></param>
        /// <returns></returns>
        public override string GetPostTransactionXml(string recxml, Hashtable parmht)
        {
            //第一次發送時要更改SELECT 及ACTION=' '
            string strxml = "";
            XmlDocument recxmldoc = new XmlDocument();
            XmlDocument postxmldoc = new XmlDocument();
            XmlNode header;
            XmlNodeList line;
            string strheader = "";
            string strline = "";
            string lineno = "";
            string innertxid = "";

            //組傳送的xml及xmldocument
            recxmldoc.LoadXml(recxml);
            header = recxmldoc.SelectSingleNode("/hostgateway/header");
            strheader = recxmldoc.SelectSingleNode("/hostgateway/header").InnerXml;

            strheader += @"<data id=""lowValue"" value=""true""/>";

            line = recxmldoc.GetElementsByTagName("line");

            if (line.Count == 2)
            { lineno = "2"; }
            else
            {
                lineno = "1";
            }



            strline = recxmldoc.SelectSingleNode("/hostgateway/line[@no='" + lineno + "']/msgBody").InnerXml;
            strxml = @"<?xml version=""1.0""?><hostgateway><header>" + strheader + "</header><body>" + strline + "</body></hostgateway>";
            postxmldoc.LoadXml(strxml);
            //改變交易代號
            XmlNode datanode = postxmldoc.SelectSingleNode("/hostgateway/header/data[@id='transactionId']");
            innertxid = datanode.Attributes["value"].Value;
            if (innertxid == "062072")
            { datanode.Attributes["value"].Value = "067072"; }
            else
            { datanode.Attributes["value"].Value = "067073"; }
            

            //傳送067072要取回067073 須設定以下的值<data id="ACTION" value=" "/> <data id='SELECT' value='01'>
            if (innertxid == "062072")
            {
                //表示第一次發送要取回67073
                datanode = postxmldoc.SelectSingleNode("/hostgateway/body/data[@id='ACTION']");
                datanode.Attributes["value"].Value = " ";

                datanode = postxmldoc.SelectSingleNode("/hostgateway/body/data[@id='SELECT']");
                datanode.Attributes["value"].Value = parmht["SELECT"].ToString();
            }
            else
            {
                datanode = postxmldoc.SelectSingleNode("/hostgateway/body/data[@id='ACTION']");
                datanode.Attributes["value"].Value = "N";
            }


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
            string totacctblan = "0"; ;

            DataTable dt067073Detail = Createdt067073Detail();

            DataTable dt067073Master = Createdt067073Master(); 

            string lineflag = "";

            //主機回傳碼
            string outputcode = "";

            string facilityno = "";

            int _currentpageno =0;
  
            string[] detfield = { "ACCOUNT_NO", "ACCT_BALANCE", "ACCT_STUS", "ACCT_WORST", "ADV_DATE", "AVAIL_LIMIT", "BRANCH_NO", "INT_RATE", "LIMIT", "MAT_DATE", "PRODUCT" };

            try
            {
                if (recxmldatalist.Count == 0)
                {
                    throw new Exception("No xml data");
                }

                #region 逐一拆解xml資料
                for (int i = 0; i < recxmldatalist.Count; i++)
                {
                    _currentpageno = i + 1;
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


                    #region 若為第一頁則要insert dt067073Master
                    if (i == 0)
                    {
                        //寫入dt67072master
                        DataRow _dr = dt067073Master.NewRow();
                        foreach (XmlNode _node in xmldoc.SelectSingleNode("/hostgateway/line[@no='" + lineflag +"']/msgBody").ChildNodes)
                        {
                            try
                            {
                                _dr[_node.Attributes["id"].Value] = _node.Attributes["value"].Value;
                            }
                            catch (Exception ex1)//欄位有錯誤則不處理
                            { }
                        }
                        dt067073Master.Rows.Add(_dr);
                        facilityno = _dr["FACILITY_NO"].ToString();
                    }

                    //取出最後的TOT_ACCT_BLAN 並更新至dt067073Master
                    _xmlnode = xmldoc.SelectSingleNode("/hostgateway/line[@no='" + lineflag + "']/msgBody/data[@id='TOT_ACCT_BLAN']");
                    totacctblan = (_xmlnode == null) ? "0" : _xmlnode.Attributes["value"].Value;
                    dt067073Master.Rows[0]["TOT_ACCT_BLAN"] = GetDecimal(totacctblan);
                    #endregion


                    #region 處理dt67073的資料
                    #region  每一頁有7筆,所以作7次

                    for (int idx = 1; idx <= 7; idx++)
                    {
                        DataRow _detr = dt067073Detail.NewRow();
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

                        _detr["FACILITY_NO"] = facilityno;
                        _detr["CurrentPageNo"] = _currentpageno.ToString();
                        _detr["CurrentPageSeqNo"] = idx;

                        if (_detr["ACCOUNT_NO"].ToString() != "000000000000")
                        {
                            dt067073Detail.Rows.Add(_detr);
                        }
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

                    if (dt067073Master.Rows.Count > 0)
                    {
                        //主檔處理資料數值

                        dt067073Master.Rows[0]["LOAN_BAL"] = GetDecimal(dt067073Master.Rows[0]["LOAN_BAL"].ToString());
                        dt067073Master.Rows[0]["OPT_AMT"] = GetDecimal(dt067073Master.Rows[0]["OPT_AMT"].ToString());
                        dt067073Master.Rows[0]["TOT_ACCT_BLAN"] = GetDecimal(dt067073Master.Rows[0]["TOT_ACCT_BLAN"].ToString());
                        dt067073Master.Rows[0]["TOT_LIMIT"] = GetDecimal(dt067073Master.Rows[0]["TOT_LIMIT"].ToString());
                    }

                    XmlData.Tables.Add(dt067073Master);

                    //處理明細檔
                    if (dt067073Detail.Rows.Count > 0)
                    {
                        for (int idn = 0; idn < dt067073Detail.Rows.Count; idn++)
                        {
                            dt067073Detail.Rows[idn]["ACCT_BALANCE"] = GetDecimal(dt067073Detail.Rows[idn]["ACCT_BALANCE"].ToString());
                            dt067073Detail.Rows[idn]["LIMIT"] = GetDecimal(dt067073Detail.Rows[idn]["LIMIT"].ToString());
                            dt067073Detail.Rows[idn]["INT_RATE"] = GetDecimal(dt067073Detail.Rows[idn]["INT_RATE"].ToString());
                            dt067073Detail.Rows[idn]["LIMIT"] = GetDecimal(dt067073Detail.Rows[idn]["LIMIT"].ToString());
                            dt067073Detail.Rows[idn]["ADV_DATE"] = GetdataYYYYMMDD(dt067073Detail.Rows[idn]["ADV_DATE"].ToString());
                            dt067073Detail.Rows[idn]["MAT_DATE"] = GetdataYYYYMMDD(dt067073Detail.Rows[idn]["MAT_DATE"].ToString());
                        }
                    }
                    XmlData.Tables.Add(dt067073Detail);                
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
        /// set up data container
        /// </summary>
        /// <returns></returns>
        private DataTable Createdt067073Detail()
        {
            DataTable dt = new DataTable("dt067073Detail");
            dt.Columns.Add(new DataColumn("FACILITY_NO"));
            dt.Columns.Add(new DataColumn("ACCOUNT_NO"));
            dt.Columns.Add(new DataColumn("ACCT_BALANCE"));
            dt.Columns.Add(new DataColumn("ACCT_STUS"));
            dt.Columns.Add(new DataColumn("ACCT_WORST"));
            dt.Columns.Add(new DataColumn("ADV_DATE"));
            dt.Columns.Add(new DataColumn("BRANCH_NO"));
            dt.Columns.Add(new DataColumn("INT_RATE"));
            dt.Columns.Add(new DataColumn("LIMIT"));
            dt.Columns.Add(new DataColumn("MAT_DATE"));
            dt.Columns.Add(new DataColumn("PRODUCT"));
     
            //資料是位於第n頁
            dt.Columns.Add(new DataColumn("CurrentPageNo"));
            //資料是位於該頁的第n筆
            dt.Columns.Add(new DataColumn("CurrentPageSeqNo"));

            return dt;

        }

        /// <summary>
        /// set up data container
        /// </summary>
        /// <returns></returns>
        private DataTable Createdt067073Master()
        {
            DataTable dt =new DataTable("dt067073Master");

            dt.Columns.Add(new DataColumn("CUST_ID_NO"));
            dt.Columns.Add(new DataColumn("CUST_NO"));
            dt.Columns.Add(new DataColumn("CUST_NO1"));
            dt.Columns.Add(new DataColumn("CUSTOMER_NAME"));
            dt.Columns.Add(new DataColumn("LOAN_BAL"));
            dt.Columns.Add(new DataColumn("OPT_AMT"));
            dt.Columns.Add(new DataColumn("TENOR999"));
            dt.Columns.Add(new DataColumn("TIME"));
            dt.Columns.Add(new DataColumn("FACILITY_NO"));
            dt.Columns.Add(new DataColumn("FACM_NO1"));
            dt.Columns.Add(new DataColumn("PAGE_LINE"));
            dt.Columns.Add(new DataColumn("PROD_TYPE"));
            dt.Columns.Add(new DataColumn("TOT_ACCT_BLAN"));
            dt.Columns.Add(new DataColumn("TOT_LIMIT"));
            return dt;

        }

    }


}
