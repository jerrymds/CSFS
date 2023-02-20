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
    public partial class Htg67002 : HtgXmlPara
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
                                    <data id=""transactionId"" value=""67050""/>
                                    <data id=""sessionId"" value=""#sessionId#""/>
                                    <data id=""lowValue"" value=""""/>
                                  </header>
                                  <body>
                                    <data value=""CSFS"" id=""applicationId""/>
                                    <data value="""" id=""CUST_ID_NO""/>
                                    <data value=""OPTN"" id=""8""/>
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
            datanode.Attributes["value"].Value = "67002";

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

            DataTable dt67002 = Createdt67002();

            string lineflag = "";

            //主機回傳碼
            string outputcode = "";

            string _currentpageno = "1";
            //string _currentpageseq="1";

            ////string CONTROL_AREA = "";

            string totallimit = "0";


            // 由於電文的欄位名稱與DB的欄位名稱 不太相同, 因此用以下來mapping 欄位名稱
            Dictionary<string, string> masterMapping = new Dictionary<string, string>() { 
                {"ANU_RPMT_AMT","ANU_RPMT_AMT"},{"ANU_RPMT_CHK","ANU_RPMT_CHK"},{"ANU_TOVR_AMT","ANU_TOVR_AMT"},{"ANU_TOVR_CHK","ANU_TOVR_CHK"},{"AUDT_NAME","AUDT_NAME"},{"BASEL_LIST","BASEL_LIST"},{"CIF_NO","CIF_NO"},{"CLN_CAPT_AMT","CLN_CAPT_AMT"},{"CLN_CAPT_CHK","CLN_CAPT_CHK"},{"CR_RATG","CR_RATG"},{"CREDIT_SCORE","CREDIT_SCORE"},{"CUST_NAME","CUST_NAME"},{"EXTN_DPOS_AMT","EXTN_DPOS_AMT"},{"FG_CONTRACT","FG_CONTRACT"},{"FRWD_CMIT_AMT","FRWD_CMIT_AMT"},{"ID_TYPE","ID_TYPE"},{"LONBUS_DATE","LONBUS_DATE"},{"ORR_EDATE","ORR_EDATE"},{"OTH_BANK_DESC","OTH_BANK_DESC"},{"OTH_BANK_OTSD_BAL","OTH_BANK_OTSD_BAL"},{"PAL_AMT_YY_01","PAL_AMT_YY_01"},{"PAL_AMT_YY_01_CHK","PAL_AMT_YY_01_CHK"},{"PAL_AMT_YY_02","PAL_AMT_YY_02"},{"PAL_AMT_YY_02_CHK","PAL_AMT_YY_02_CHK"},{"PAL_AMT_YY_03","PAL_AMT_YY_03"},{"PAL_AMT_YY_03_CHK","PAL_AMT_YY_03_CHK"},{"RM_NAME","RM_NAME"},{"RM_NO","RM_NO"},{"SBG_NXT_RV_DT","SBG_NXT_RV_DT"},{"SBG_PRV_RV_DT","SBG_PRV_RV_DT"},{"SHDR_EQTY_AMT","SHDR_EQTY_AMT"},{"SHDR_EQTY_CHK","SHDR_EQTY_CHK"},{"SMART_FLAG","SMART_FLAG"},{"SORD_DEBT_AMT","SORD_DEBT_AMT"},{"SORD_DEBT_CHK","SORD_DEBT_CHK"},{"VIP_CODE","VIP_CODE"},{"VIP_DEGREE","VIP_DEGREE"},{"VIP_DEGREE_FLG","VIP_DEGREE_FLG"}
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

                    #region 若為第一頁則要insert dt67002Master
                    //if (linedata.Count == 2)
                    //if (i == 0) // 表示第一頁
                    {
                        //寫入dt060491Master
                        DataRow _dr = dt67002.NewRow();
                        foreach (XmlNode _node in xmldoc.SelectSingleNode("/hostgateway/line[@no='1']/msgBody").ChildNodes)
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
                    if (dt67002.Rows.Count > 0)
                    {
                        //主檔處理資料數值
                        //dt060491Master.Rows[0]["CONTROL_AREA"] = "";
                        //dt67002.Rows[0]["CurBal"] = GetDecimal(dt67002.Rows[0]["CurBal"].ToString());
                        //dt67002.Rows[0]["PbBal"] = GetDecimal(dt67002.Rows[0]["PbBal"].ToString());

                    }



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

        private DataTable Createdt67002()
        {
            DataTable dt = new DataTable("TX_67002");
            dt.Columns.Add(new DataColumn("ANU_RPMT_AMT"));
            dt.Columns.Add(new DataColumn("ANU_RPMT_CHK"));
            dt.Columns.Add(new DataColumn("ANU_TOVR_AMT"));
            dt.Columns.Add(new DataColumn("ANU_TOVR_CHK"));
            dt.Columns.Add(new DataColumn("AUDT_NAME"));
            dt.Columns.Add(new DataColumn("BASEL_LIST"));
            dt.Columns.Add(new DataColumn("CIF_NO"));
            dt.Columns.Add(new DataColumn("CLN_CAPT_AMT"));
            dt.Columns.Add(new DataColumn("CLN_CAPT_CHK"));
            dt.Columns.Add(new DataColumn("CR_RATG"));
            dt.Columns.Add(new DataColumn("CREDIT_SCORE"));
            dt.Columns.Add(new DataColumn("CUST_NAME"));
            dt.Columns.Add(new DataColumn("EXTN_DPOS_AMT"));
            dt.Columns.Add(new DataColumn("FG_CONTRACT"));
            dt.Columns.Add(new DataColumn("FRWD_CMIT_AMT"));
            dt.Columns.Add(new DataColumn("ID_TYPE"));
            dt.Columns.Add(new DataColumn("LONBUS_DATE"));
            dt.Columns.Add(new DataColumn("ORR_EDATE"));
            dt.Columns.Add(new DataColumn("OTH_BANK_DESC"));
            dt.Columns.Add(new DataColumn("OTH_BANK_OTSD_BAL"));
            dt.Columns.Add(new DataColumn("PAL_AMT_YY_01"));
            dt.Columns.Add(new DataColumn("PAL_AMT_YY_01_CHK"));
            dt.Columns.Add(new DataColumn("PAL_AMT_YY_02"));
            dt.Columns.Add(new DataColumn("PAL_AMT_YY_02_CHK"));
            dt.Columns.Add(new DataColumn("PAL_AMT_YY_03"));
            dt.Columns.Add(new DataColumn("PAL_AMT_YY_03_CHK"));
            dt.Columns.Add(new DataColumn("RM_NAME"));
            dt.Columns.Add(new DataColumn("RM_NO"));
            dt.Columns.Add(new DataColumn("SBG_NXT_RV_DT"));
            dt.Columns.Add(new DataColumn("SBG_PRV_RV_DT"));
            dt.Columns.Add(new DataColumn("SHDR_EQTY_AMT"));
            dt.Columns.Add(new DataColumn("SHDR_EQTY_CHK"));
            dt.Columns.Add(new DataColumn("SMART_FLAG"));
            dt.Columns.Add(new DataColumn("SORD_DEBT_AMT"));
            dt.Columns.Add(new DataColumn("SORD_DEBT_CHK"));
            dt.Columns.Add(new DataColumn("VIP_CODE"));
            dt.Columns.Add(new DataColumn("VIP_DEGREE"));
            dt.Columns.Add(new DataColumn("VIP_DEGREE_FLG"));
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
