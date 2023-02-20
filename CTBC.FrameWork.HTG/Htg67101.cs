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
    public class Htg67101 : HtgXmlPara
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
                                    <data id=""transactionId"" value=""067101""/>
                                    <data id=""sessionId"" value=""#sessionId#""/>
                                    <data id=""lowValue"" value=""""/>
                                  </header>
                                  <body>
                                    #body#
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
            datanode.Attributes["value"].Value = "067100";

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

            List<string> Fields = new List<string>() { "MRTL_STA", "IN_REC", "OCPN_DESC", "ML_BILL_PRT_D", "EMPR_NAME", "CHANNEL", "HLTH_WITHHOLD_IND", "ADULT_FLAG", "VI2_PI_FLAG", "SEGMENT_CODE", "EDUC_LEVEL", "INCOME_FREQ", "BILL_DATE", "OCPN_CODE", "HIRE_TYPE", "ML_BILL_TYPE", "OLD_CHILD_YYYYMM", "SIGN_FLAG", "FUND", "ASSET_SOURCE_8", "CUST_ID_TYP", "ASSET_SOURCE_7", "ASSET_SOURCE_6", "ASSET_SOURCE_5", "ASSET_SOURCE_4", "REGI_TEL_STS", "ASSET_SOURCE_3", "ASSET_SOURCE_2", "ASSET_SOURCE_1", "CUSM_IN_ATTR", "REGI_TEL_DATE", "INVEST_EXPER_16", "INVEST_EXPER_15", "INVEST_EXPER_14", "INVEST_EXPER_13", "INVEST_EXPER_12", "INVEST_EXPER_11", "AML_OCCUPATION_CODE", "INVEST_EXPER_10", "AUDIO_CODE", "OCPN_TYPE", "SICK_FLAG", "GRAD_YYMM", "WEALTH_REQ_8", "WEALTH_REQ_7", "WEALTH_REQ_6", "WEALTH_REQ_5", "WEALTH_REQ_4", "FINANC_GOODS_9", "WEALTH_REQ_3", "FINANC_GOODS_8", "WEALTH_REQ_2", "FINANC_GOODS_7", "WEALTH_REQ_1", "FINANC_GOODS_6", "FINANC_GOODS_5", "HIGH_ASSET_FLAG", "FINANC_GOODS_4", "FINANC_GOODS_3", "ERR_DAT", "ZIP_CODE", "LOAN_NOTIFY_DATE", "FINANC_GOODS_2", "FINANC_GOODS_1", "BILL_TYPE", "HIGH_OLD", "SI_AMT_REC", "VIP_DEGREE", "IN_AMT_REC", "REGI_TEL", "RISK_ATTR", "DOUBLE_SALARY_STS", "CONFIRM_DATA_DATE", "SI_REC", "CIF_NO", "INVEST_EXPER_9", "outputCode", "INVEST_EXPER_8", "INVEST_EXPER_7", "MAIL_IND", "SALARY_DAY", "SN_REC", "INVEST_EXPER_6", "INVEST_EXPER_5", "INVEST_EXPER_4", "INVEST_EXPER_3", "INVEST_EXPER_2", "NOF_DEPN", "INVEST_EXPER_1", "GRD_AMT_REC", "CTRY_RES_CODE", "FN_REC", "SBU_BILL_DT", "CUST_ADD4", "GRD_REC", "CUST_ADD3", "CUST_ADD2", "CUST_ADD1", "DOB", "GOV_ORG_CODE2", "GOV_ORG_CODE1", "DEAD_FLAG", "EMPR_ADRS", "LOAN_NOTIFY_TYPE", "ASSET_STATUS", "EMPY_DATE", "REG_NAT_CODE", "TAX_IND", "SN_AMT_REC", "REGI_ADDR_STS", "CAFLAG", "BILL_CYCLE", "HLTH_IND", "OPEN_AIM", "CUST_ID_NO", "CUSTOMER_TYPE", "FINANC_GOODS_19", "UCODE", "FINANC_GOODS_18", "FINANC_GOODS_17", "FINANC_GOODS_16", "FINANC_GOODS_15", "FINANC_GOODS_14", "FINANC_GOODS_13", "FINANC_GOODS_12", "INCOME_AMT", "FINANC_GOODS_11", "FINANC_GOODS_10", "PLAN_ASSET", "CUSTOMER_NAME", "FN_AMT_REC", "EDU_FLAG", "GNDR", "AML_INDUSTRY_CODE_NEW", "CONF_FLAG", "REGI_TEL_EXT", "AML_INDUSTRY_CODE", "BILL_FORCE", "FUND_CIF", "VI2_COUNT", "VI2_ACTIVE_FLAG" };

            string recxmldata = "";
            strmessage = "";
            DataSet XmlData = new DataSet();
            XmlDocument xmldoc = new XmlDocument();
            XmlNodeList linedata;
            XmlNode _xmlnode;

            DataTable dt067101Master = Createdt67101Master();

            string lineflag = "";

            //主機回傳碼
            string outputcode = "";

            string _currentpageno = "1";
            //string _currentpageseq="1";

            ////string CONTROL_AREA = "";

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

                    
                    var errMess = isErrorMessage(xmldoc, linedata.Count);
                    if (!string.IsNullOrEmpty(errMess))
                    {
                        strmessage += errMess;
                        break;
                    }
                    //LINE='2', 有沒有OK .... 若有任何錯誤Error, 則報錯, 
                    errMess = checkLine2OK(xmldoc, "2");
                    if (!string.IsNullOrEmpty(errMess))
                    {
                        strmessage += errMess;
                        break;
                    }


                    #region 若為第一頁則要insert dt067101Master
                    //if (linedata.Count == 2)  //表示第一頁
                    // (i == 0) // 表示第一頁
                    {
                        //寫入dt060491Master
                        DataRow _dr = dt067101Master.NewRow();
                        foreach (XmlNode _node in xmldoc.SelectSingleNode("/hostgateway/line[@no='1']/msgBody").ChildNodes)
                        {
                            try
                            {

                                var eFieldName = _node.Attributes["id"].Value.ToUpper().Trim();
                                if (Fields.Contains(eFieldName))
                                    _dr[eFieldName] = _node.Attributes["value"].Value;
                            }
                            catch (Exception ex1)
                            //欄位有錯誤則不處理
                            {
                                throw new Exception(_node.Attributes["id"].ToString() + " 找不到!!!");
                            }
                        }
                        dt067101Master.Rows.Add(_dr);

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
                    XmlData.Tables.Add(dt067101Master);
                }
                catch (Exception exe2)
                {
                    throw exe2;
                }

                #endregion

            }

            return XmlData;

        }

        private string checkLine2OK(XmlDocument xmldoc, string lineno)
        {
            string result = null;
            
            
            try
            {
                var Oknode = xmldoc.SelectSingleNode("/hostgateway/line[@no='" + lineno + "']/msgBody/data[@id='RESPONSE_OC08']");
                if (Oknode != null && Oknode.Attributes["value"].Value.Contains("O.K."))
                    return result;

                Oknode = xmldoc.SelectSingleNode("/hostgateway/line[@no='" + lineno + "']/msgBody/data[@id='outputCode']");
                if (Oknode != null && (Oknode.Attributes["value"].Value.Contains("08") || Oknode.Attributes["value"].Value.Contains("03")))
                    return result;


                var _xmlnode = xmldoc.SelectSingleNode("/hostgateway/line[@no='" + lineno + "']/msgBody/data[@id='ERRORMESSAGETEXT_OC01']");
                if (_xmlnode != null)
                    result = " 電文訊息 :" + _xmlnode.Attributes["value"].Value;

            }
            catch
            {
                result = "電文格式無法辨識";
            }
            

            return result;
        }



        private DataTable Createdt67101Master()
        {
            DataTable dt = new DataTable("TX_67101");

            dt.Columns.Add(new DataColumn("MRTL_STA"));
            dt.Columns.Add(new DataColumn("IN_REC"));
            dt.Columns.Add(new DataColumn("OCPN_DESC"));
            dt.Columns.Add(new DataColumn("ML_BILL_PRT_D"));
            dt.Columns.Add(new DataColumn("EMPR_NAME"));
            dt.Columns.Add(new DataColumn("CHANNEL"));
            dt.Columns.Add(new DataColumn("HLTH_WITHHOLD_IND"));
            dt.Columns.Add(new DataColumn("ADULT_FLAG"));
            dt.Columns.Add(new DataColumn("VI2_PI_FLAG"));
            dt.Columns.Add(new DataColumn("SEGMENT_CODE"));
            dt.Columns.Add(new DataColumn("EDUC_LEVEL"));
            dt.Columns.Add(new DataColumn("INCOME_FREQ"));
            dt.Columns.Add(new DataColumn("BILL_DATE"));
            dt.Columns.Add(new DataColumn("OCPN_CODE"));
            dt.Columns.Add(new DataColumn("HIRE_TYPE"));
            dt.Columns.Add(new DataColumn("ML_BILL_TYPE"));
            dt.Columns.Add(new DataColumn("OLD_CHILD_YYYYMM"));
            dt.Columns.Add(new DataColumn("SIGN_FLAG"));
            dt.Columns.Add(new DataColumn("FUND"));
            dt.Columns.Add(new DataColumn("ASSET_SOURCE_8"));
            dt.Columns.Add(new DataColumn("CUST_ID_TYP"));
            dt.Columns.Add(new DataColumn("ASSET_SOURCE_7"));
            dt.Columns.Add(new DataColumn("ASSET_SOURCE_6"));
            dt.Columns.Add(new DataColumn("ASSET_SOURCE_5"));
            dt.Columns.Add(new DataColumn("ASSET_SOURCE_4"));
            dt.Columns.Add(new DataColumn("REGI_TEL_STS"));
            dt.Columns.Add(new DataColumn("ASSET_SOURCE_3"));
            dt.Columns.Add(new DataColumn("ASSET_SOURCE_2"));
            dt.Columns.Add(new DataColumn("ASSET_SOURCE_1"));
            dt.Columns.Add(new DataColumn("CUSM_IN_ATTR"));
            dt.Columns.Add(new DataColumn("REGI_TEL_DATE"));
            dt.Columns.Add(new DataColumn("INVEST_EXPER_16"));
            dt.Columns.Add(new DataColumn("INVEST_EXPER_15"));
            dt.Columns.Add(new DataColumn("INVEST_EXPER_14"));
            dt.Columns.Add(new DataColumn("INVEST_EXPER_13"));
            dt.Columns.Add(new DataColumn("INVEST_EXPER_12"));
            dt.Columns.Add(new DataColumn("INVEST_EXPER_11"));
            dt.Columns.Add(new DataColumn("AML_OCCUPATION_CODE"));
            dt.Columns.Add(new DataColumn("INVEST_EXPER_10"));
            dt.Columns.Add(new DataColumn("AUDIO_CODE"));
            dt.Columns.Add(new DataColumn("OCPN_TYPE"));
            dt.Columns.Add(new DataColumn("SICK_FLAG"));
            dt.Columns.Add(new DataColumn("GRAD_YYMM"));
            dt.Columns.Add(new DataColumn("WEALTH_REQ_8"));
            dt.Columns.Add(new DataColumn("WEALTH_REQ_7"));
            dt.Columns.Add(new DataColumn("WEALTH_REQ_6"));
            dt.Columns.Add(new DataColumn("WEALTH_REQ_5"));
            dt.Columns.Add(new DataColumn("WEALTH_REQ_4"));
            dt.Columns.Add(new DataColumn("FINANC_GOODS_9"));
            dt.Columns.Add(new DataColumn("WEALTH_REQ_3"));
            dt.Columns.Add(new DataColumn("FINANC_GOODS_8"));
            dt.Columns.Add(new DataColumn("WEALTH_REQ_2"));
            dt.Columns.Add(new DataColumn("FINANC_GOODS_7"));
            dt.Columns.Add(new DataColumn("WEALTH_REQ_1"));
            dt.Columns.Add(new DataColumn("FINANC_GOODS_6"));
            dt.Columns.Add(new DataColumn("FINANC_GOODS_5"));
            dt.Columns.Add(new DataColumn("HIGH_ASSET_FLAG"));
            dt.Columns.Add(new DataColumn("FINANC_GOODS_4"));
            dt.Columns.Add(new DataColumn("FINANC_GOODS_3"));
            dt.Columns.Add(new DataColumn("ERR_DAT"));
            dt.Columns.Add(new DataColumn("ZIP_CODE"));
            dt.Columns.Add(new DataColumn("LOAN_NOTIFY_DATE"));
            dt.Columns.Add(new DataColumn("FINANC_GOODS_2"));
            dt.Columns.Add(new DataColumn("FINANC_GOODS_1"));
            dt.Columns.Add(new DataColumn("BILL_TYPE"));
            dt.Columns.Add(new DataColumn("HIGH_OLD"));
            dt.Columns.Add(new DataColumn("SI_AMT_REC"));
            dt.Columns.Add(new DataColumn("VIP_DEGREE"));
            dt.Columns.Add(new DataColumn("IN_AMT_REC"));
            dt.Columns.Add(new DataColumn("REGI_TEL"));
            dt.Columns.Add(new DataColumn("RISK_ATTR"));
            dt.Columns.Add(new DataColumn("DOUBLE_SALARY_STS"));
            dt.Columns.Add(new DataColumn("CONFIRM_DATA_DATE"));
            dt.Columns.Add(new DataColumn("SI_REC"));
            dt.Columns.Add(new DataColumn("CIF_NO"));
            dt.Columns.Add(new DataColumn("INVEST_EXPER_9"));
            dt.Columns.Add(new DataColumn("outputCode"));
            dt.Columns.Add(new DataColumn("INVEST_EXPER_8"));
            dt.Columns.Add(new DataColumn("INVEST_EXPER_7"));
            dt.Columns.Add(new DataColumn("MAIL_IND"));
            dt.Columns.Add(new DataColumn("SALARY_DAY"));
            dt.Columns.Add(new DataColumn("SN_REC"));
            dt.Columns.Add(new DataColumn("INVEST_EXPER_6"));
            dt.Columns.Add(new DataColumn("INVEST_EXPER_5"));
            dt.Columns.Add(new DataColumn("INVEST_EXPER_4"));
            dt.Columns.Add(new DataColumn("INVEST_EXPER_3"));
            dt.Columns.Add(new DataColumn("INVEST_EXPER_2"));
            dt.Columns.Add(new DataColumn("NOF_DEPN"));
            dt.Columns.Add(new DataColumn("INVEST_EXPER_1"));
            dt.Columns.Add(new DataColumn("GRD_AMT_REC"));
            dt.Columns.Add(new DataColumn("CTRY_RES_CODE"));
            dt.Columns.Add(new DataColumn("FN_REC"));
            dt.Columns.Add(new DataColumn("SBU_BILL_DT"));
            dt.Columns.Add(new DataColumn("CUST_ADD4"));
            dt.Columns.Add(new DataColumn("GRD_REC"));
            dt.Columns.Add(new DataColumn("CUST_ADD3"));
            dt.Columns.Add(new DataColumn("CUST_ADD2"));
            dt.Columns.Add(new DataColumn("CUST_ADD1"));
            dt.Columns.Add(new DataColumn("DOB"));
            dt.Columns.Add(new DataColumn("GOV_ORG_CODE2"));
            dt.Columns.Add(new DataColumn("GOV_ORG_CODE1"));
            dt.Columns.Add(new DataColumn("DEAD_FLAG"));
            dt.Columns.Add(new DataColumn("EMPR_ADRS"));
            dt.Columns.Add(new DataColumn("LOAN_NOTIFY_TYPE"));
            dt.Columns.Add(new DataColumn("ASSET_STATUS"));
            dt.Columns.Add(new DataColumn("EMPY_DATE"));
            dt.Columns.Add(new DataColumn("REG_NAT_CODE"));
            dt.Columns.Add(new DataColumn("TAX_IND"));
            dt.Columns.Add(new DataColumn("SN_AMT_REC"));
            dt.Columns.Add(new DataColumn("REGI_ADDR_STS"));
            dt.Columns.Add(new DataColumn("CAFLAG"));
            dt.Columns.Add(new DataColumn("BILL_CYCLE"));
            dt.Columns.Add(new DataColumn("HLTH_IND"));
            dt.Columns.Add(new DataColumn("OPEN_AIM"));
            dt.Columns.Add(new DataColumn("CUST_ID_NO"));
            dt.Columns.Add(new DataColumn("CUSTOMER_TYPE"));
            dt.Columns.Add(new DataColumn("FINANC_GOODS_19"));
            dt.Columns.Add(new DataColumn("UCODE"));
            dt.Columns.Add(new DataColumn("FINANC_GOODS_18"));
            dt.Columns.Add(new DataColumn("FINANC_GOODS_17"));
            dt.Columns.Add(new DataColumn("FINANC_GOODS_16"));
            dt.Columns.Add(new DataColumn("FINANC_GOODS_15"));
            dt.Columns.Add(new DataColumn("FINANC_GOODS_14"));
            dt.Columns.Add(new DataColumn("FINANC_GOODS_13"));
            dt.Columns.Add(new DataColumn("FINANC_GOODS_12"));
            dt.Columns.Add(new DataColumn("INCOME_AMT"));
            dt.Columns.Add(new DataColumn("FINANC_GOODS_11"));
            dt.Columns.Add(new DataColumn("FINANC_GOODS_10"));
            dt.Columns.Add(new DataColumn("PLAN_ASSET"));
            dt.Columns.Add(new DataColumn("CUSTOMER_NAME"));
            dt.Columns.Add(new DataColumn("FN_AMT_REC"));
            dt.Columns.Add(new DataColumn("EDU_FLAG"));
            dt.Columns.Add(new DataColumn("GNDR"));
            dt.Columns.Add(new DataColumn("AML_INDUSTRY_CODE_NEW"));
            dt.Columns.Add(new DataColumn("CONF_FLAG"));
            dt.Columns.Add(new DataColumn("REGI_TEL_EXT"));
            dt.Columns.Add(new DataColumn("AML_INDUSTRY_CODE"));
            dt.Columns.Add(new DataColumn("BILL_FORCE"));
            dt.Columns.Add(new DataColumn("FUND_CIF"));
            dt.Columns.Add(new DataColumn("VI2_COUNT"));
            dt.Columns.Add(new DataColumn("VI2_ACTIVE_FLAG"));
            return dt;
        }
        
    }
}
