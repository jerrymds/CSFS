using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Xml;
using System.Linq;
using System.Linq.Expressions;

namespace ExcuteHTG
{
    public partial class Htg033401 : HtgXmlPara
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
                                    <data id=""transactionId"" value=""000401""/>
                                    <data id=""sessionId"" value=""#sessionId#""/>
                                    <data id=""lowValue"" value=""""/>
                                  </header>
                                  <body>
                                    <data value=""CSFS"" id=""applicationId""/>
                                    <data value=""S"" id=""ACCT_NO""/>
                                    <data value=""TWD"" id=""CURRENCY""/>
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
            datanode.Attributes["value"].Value = "000401";

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
        //public override DataSet TransferXmlToDataSet_OLD(ArrayList recxmldatalist, out string strmessage)
        //{

        //    string recxmldata = "";
        //    strmessage = "";
        //    DataSet XmlData = new DataSet();
        //    XmlDocument xmldoc = new XmlDocument();
        //    XmlNodeList linedata;
        //    XmlNode _xmlnode;

        //    DataTable dt33401 = Createdt33401();

        //    string lineflag = "";

        //    //主機回傳碼
        //    string outputcode = "";

        //    string _currentpageno = "1";
        //    //string _currentpageseq="1";

        //    ////string CONTROL_AREA = "";

        //    string totallimit = "0";


        //    // 由於電文的欄位名稱與DB的欄位名稱 不太相同, 因此用以下來mapping 欄位名稱
        //    Dictionary<string, string> masterMapping = new Dictionary<string, string>() { { "A_ACT_DEGREE", "AActDegree" }, { "A_RELATION_TYPE", "ARelationType" }, { "ACCT_NO", "Acct" }, { "ACCT_STATUS_1", "AcctStatus1" }, { "ACCT_STATUS_2", "AcctStatus2" }, { "ATM_HOLD_AMT", "AtmHoldAmt" }, { "ATM_WAIVE_EXP_DT", "AtmWaiveExpDt" }, { "BIRTHDAY", "Birthday" }, { "BRANCH_NO", "Branch" }, { "CLOSE_DATE", "CloseDate" }, { "CUR_BAL", "CurBal" }, { "CURRENCY", "Currency" }, { "CUSM_OR_A_AC_DEGREE", "CusmOrAAcDegree" }, { "CUST_ID_NO", "CustId" }, { "EX_OD_INT", "ExOdInt" }, { "FAX_NO", "FaxNo" }, { "FB_TELLER", "FB_TELLER" }, { "FILLER5", "Filler" }, { "GLOBAL_HEAD_CODE", "GlobalHeadCode" }, { "H_TEL_EXT", "HTelExt" }, { "HIGH_CONTR", "HighContr" }, { "HOLD_AMT", "HoldAmt" }, { "HOUSE_FLAG", "HouseFlag" }, { "INT_ACCT", "IntAcct" }, { "INT_AMT", "IntAmt" }, { "INT_DATE", "IntDate" }, { "INTER_BRANCH", "InterBranch" }, { "INVEST_HOLD_VAL", "InvestHoldVal" }, { "INVEST_TYPE", "InvestType" }, { "LAST_DATE", "LastDate" }, { "LGMB_FLAG", "LgmbFlag" }, { "LST_TAX", "LstTax" }, { "MIN_REPAY_AMT", "MinRepayAmt" }, { "MIN_REPAY_DATE", "MinRepayDate" }, { "OPEN_DATE", "OpenDate" }, { "OTHER_HOLD_AMT", "OtherHoldAmt" }, { "OVER_AMT", "OverAmt" }, { "OVER_TAX", "OverTax" }, { "PB_BAL", "PbBal" }, { "PORTFOLIO_NO", "PortfolioNo" }, { "PROCESS_STS_TEXT", "ProcessStsText" }, { "RATE", "Rate" }, { "ROLL_CNT", "RollCnt" }, { "SECURITY_COMP_NO", "SecurityCompNo" }, { "SLY_OTHER_TRF_CNT", "SlyOtherTrfCnt" }, { "SLY_OWN_TRF_CNT", "SlyOwnTrfCnt" }, { "SLY_STF_EXCHG_RATE", "SlyStfExchgRate" }, { "SLY_WAIVE_TRF_CNT", "SlyWaiveTrfCnt" }, { "SLY_WAIVE_WD_CNT", "SlyWaiveWdCnt" }, { "STOCK_HOLD_AMT", "StockHoldAmt" }, { "TD_AMT", "TdAmt" }, { "TERM_DAY", "TermDay" }, { "TODAY_CHECK", "TodayCheck" }, { "TRIAL_FLAG", "TrialFlag" }, { "TRUE_AMT", "TrueAmt" }, { "UNCL_CHQ_USED", "UnclChqUsed" }, { "VIP_CD_I", "VipCDI" }, { "VIP_CODE", "VipCode" }, { "VIP_DEGREE", "VipDegree" }, { "YEAR_AMT", "YearAmt" }, { "YEAR_TAX", "YearTax" }, { "YEAR_TAX_FCY", "YearTaxFcy" } };

        //   try
        //    {
        //        if (recxmldatalist.Count == 0)
        //        {
        //            throw new Exception("No xml data");
        //        }

        //        #region 逐一拆解xml資料
        //        for (int i = 0; i < recxmldatalist.Count; i++)
        //        {
        //            recxmldata = recxmldatalist[i].ToString();
        //            xmldoc.LoadXml(recxmldata);
        //            linedata = xmldoc.GetElementsByTagName("line");

        //            #region 檢查第一頁是否有錯誤,或是否為最後一頁
        //            if (linedata.Count == 1)
        //            {
        //                //表示第一頁只一筆,即為錯誤
        //                _xmlnode = xmldoc.SelectSingleNode("/hostgateway/line[@no='1']/msgBody/data[@id='outputCode']");
        //                outputcode = _xmlnode.Attributes["value"].Value;

        //                if (outputcode != "03")
        //                {
        //                    //不為03可能是一開始就沒資料,也有可能是最後一頁了
        //                    _xmlnode = xmldoc.SelectSingleNode("/hostgateway/line[@no='1']/msgBody/data[@id='ERRORMESSAGETEXT_OC01']");
        //                    try { strmessage += " message :" + _xmlnode.Attributes["value"].Value; }
        //                    catch { }
        //                    break;
        //                }
        //                lineflag = "1";
        //            }
        //            else
        //            {
        //                lineflag = "2";
        //            }
        //            #endregion

        //            #region 若為第一頁則要insert dt060491Master
        //            if (linedata.Count == 2)
        //            //if (i == 0) // 表示第一頁
        //            {
        //                //寫入dt060491Master
        //                DataRow _dr = dt33401.NewRow();
        //                foreach (XmlNode _node in xmldoc.SelectSingleNode("/hostgateway/line[@no='2']/msgBody").ChildNodes)
        //                {
        //                    try
        //                    {
        //                        var eFieldName = _node.Attributes["id"].Value.ToUpper().Trim();

        //                        bool isMasterField = masterMapping.ContainsKey(eFieldName);
        //                        if (isMasterField) // 找出對映DB的欄位名稱
        //                        {
        //                            string dbFieldName = masterMapping[eFieldName];
        //                            _dr[dbFieldName] = _node.Attributes["value"].Value;
        //                        }
        //                    }
        //                    catch (Exception ex1)
        //                    //欄位有錯誤則不處理
        //                    {
        //                        throw new Exception(_node.Attributes["id"].ToString() + " 找不到!!!");
        //                    }
        //                }
        //                dt33401.Rows.Add(_dr);

        //            }

        //            #endregion


        //        }

        //        #endregion
        //    }
        //    catch (Exception exe)
        //    {
        //        //throw exe;
        //        strmessage += exe.ToString();
        //        //throw exe;
        //    }
        //    finally
        //    {
        //        #region 資料轉換
        //        try
        //        {
        //            if (dt33401.Rows.Count > 0)
        //            {
        //                //主檔處理資料數值
        //                //dt060491Master.Rows[0]["CONTROL_AREA"] = "";
        //                dt33401.Rows[0]["CurBal"] = GetDecimal(dt33401.Rows[0]["CurBal"].ToString());
        //                dt33401.Rows[0]["PbBal"] = GetDecimal(dt33401.Rows[0]["PbBal"].ToString());

        //            }



        //            XmlData.Tables.Add(dt33401);


        //        }
        //        catch (Exception exe2)
        //        {
        //            throw exe2;
        //        }

        //        #endregion

        //    }

        //    return XmlData;

        //}


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

            DataTable dt33401 = Createdt33401();

            string lineflag = "";

            //主機回傳碼
            string outputcode = "";

            string _currentpageno = "1";
            //string _currentpageseq="1";

            ////string CONTROL_AREA = "";

            string totallimit = "0";


            // 由於電文的欄位名稱與DB的欄位名稱 不太相同, 因此用以下來mapping 欄位名稱
            Dictionary<string, string> masterMapping = new Dictionary<string, string>() { { "A_ACT_DEGREE", "AActDegree" }, { "NEXT_AMT", "NextAmt" }, { "A_RELATION_TYPE", "ARelationType" }, { "ACCT_NO", "Acct" }, { "ACCT_STATUS_1", "AcctStatus1" }, { "ACCT_STATUS_2", "AcctStatus2" }, { "ATM_HOLD_AMT", "AtmHoldAmt" }, { "ATM_WAIVE_EXP_DT", "AtmWaiveExpDt" }, { "BIRTHDAY", "Birthday" }, { "BRANCH_NO", "Branch" }, { "CLOSE_DATE", "CloseDate" }, { "CUR_BAL", "CurBal" }, { "CURRENCY", "Currency" }, { "CUSM_OR_A_AC_DEGREE", "CusmOrAAcDegree" }, { "CUST_ID_NO", "CustId" }, { "EX_OD_INT", "ExOdInt" }, { "FAX_NO", "FaxNo" }, { "FB_TELLER", "FB_TELLER" }, { "FILLER5", "Filler" }, { "GLOBAL_HEAD_CODE", "GlobalHeadCode" }, { "H_TEL_EXT", "HTelExt" }, { "HIGH_CONTR", "HighContr" }, { "HOLD_AMT", "HoldAmt" }, { "HOUSE_FLAG", "HouseFlag" }, { "INT_ACCT", "IntAcct" }, { "INT_AMT", "IntAmt" }, { "INT_DATE", "IntDate" }, { "INTER_BRANCH", "InterBranch" }, { "INVEST_HOLD_VAL", "InvestHoldVal" }, { "INVEST_TYPE", "InvestType" }, { "LAST_DATE", "LastDate" }, { "LGMB_FLAG", "LgmbFlag" }, { "LST_TAX", "LstTax" }, { "MIN_REPAY_AMT", "MinRepayAmt" }, { "MIN_REPAY_DATE", "MinRepayDate" }, { "OPEN_DATE", "OpenDate" }, { "OTHER_HOLD_AMT", "OtherHoldAmt" }, { "OVER_AMT", "OverAmt" }, { "OVER_TAX", "OverTax" }, { "PB_BAL", "PbBal" }, { "PORTFOLIO_NO", "PortfolioNo" }, { "PROCESS_STS_TEXT", "ProcessStsText" }, { "RATE", "Rate" }, { "ROLL_CNT", "RollCnt" }, { "SECURITY_COMP_NO", "SecurityCompNo" }, { "SLY_OTHER_TRF_CNT", "SlyOtherTrfCnt" }, { "SLY_OWN_TRF_CNT", "SlyOwnTrfCnt" }, { "SLY_STF_EXCHG_RATE", "SlyStfExchgRate" }, { "SLY_WAIVE_TRF_CNT", "SlyWaiveTrfCnt" }, { "SLY_WAIVE_WD_CNT", "SlyWaiveWdCnt" }, { "STOCK_HOLD_AMT", "StockHoldAmt" }, { "TD_AMT", "TdAmt" }, { "TERM_DAY", "TermDay" }, { "TODAY_CHECK", "TodayCheck" }, { "TRIAL_FLAG", "TrialFlag" }, { "TRUE_AMT", "TrueAmt" }, { "UNCL_CHQ_USED", "UnclChqUsed" }, { "VIP_CD_I", "VipCDI" }, { "VIP_CODE", "VipCode" }, { "VIP_DEGREE", "VipDegree" }, { "YEAR_AMT", "YearAmt" }, { "YEAR_TAX", "YearTax" }, { "YEAR_TAX_FCY", "YearTaxFcy" }, { "CUSTOMER_NAME", "name" }, { "LON_LIMIT", "lonamt" }, { "MD_HOLD_AMT", "MdHoldAmt" } };

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

                    #region 若為第一頁則要insert dt060491Master
                    if ((lineflag == "2" && linedata.Count == 2) || lineflag == "1")
                    //if (i == 0) // 表示第一頁
                    {
                        //寫入dt33401
                        DataRow _dr = dt33401.NewRow();
                        foreach (XmlNode _node in xmldoc.SelectSingleNode("/hostgateway/line[@no='" + lineflag + "']/msgBody").ChildNodes)
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
                        dt33401.Rows.Add(_dr);

                    }

                    #endregion

                    #region 有時, 會放在LineNo='3' 以上...
                    if (linedata.Count > 2)
                    {
                        for (int iLineNo = 3; iLineNo <= linedata.Count; iLineNo++)
                        {
                            string strLineFlag = iLineNo.ToString();
                            DataRow _dr = dt33401.NewRow();
                            foreach (XmlNode _node in xmldoc.SelectSingleNode("/hostgateway/line[@no='" + strLineFlag + "']/msgBody").ChildNodes)
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
                                catch (Exception ex1)//欄位有錯誤則不處理                                
                                {
                                    throw new Exception(_node.Attributes["id"].ToString() + " 找不到!!!");
                                }
                            }
                            dt33401.Rows.Add(_dr);
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
                    if (dt33401.Rows.Count > 0)
                    {
                        //主檔處理資料數值
                        //dt060491Master.Rows[0]["CONTROL_AREA"] = "";
                        for (int i = 0; i < dt33401.Rows.Count; i++)
                        {
                            if (dt33401.Rows[i]["Acct"].ToString().Length == 16)
                                dt33401.Rows[i]["Acct"] = "0" + dt33401.Rows[i]["Acct"].ToString();
                            dt33401.Rows[i]["CurBal"] = GetDecimal(dt33401.Rows[i]["CurBal"].ToString());
                            dt33401.Rows[i]["PbBal"] = GetDecimal(dt33401.Rows[i]["PbBal"].ToString());
                            dt33401.Rows[i]["nextamt"] = GetDecimal(dt33401.Rows[i]["nextamt"].ToString());
                        }
                    }



                    XmlData.Tables.Add(dt33401);


                }
                catch (Exception exe2)
                {
                    throw exe2;
                }

                #endregion

            }

            return XmlData;

        }


        private DataTable Createdt33401()
        {
            DataTable dt = new DataTable("TX_33401");
            dt.Columns.Add(new DataColumn("AActDegree"));
            dt.Columns.Add(new DataColumn("ARelationType"));
            dt.Columns.Add(new DataColumn("Acct"));
            dt.Columns.Add(new DataColumn("AcctStatus1"));
            dt.Columns.Add(new DataColumn("AcctStatus2"));
            dt.Columns.Add(new DataColumn("AtmHoldAmt"));
            dt.Columns.Add(new DataColumn("AtmWaiveExpDt"));
            dt.Columns.Add(new DataColumn("Birthday"));
            dt.Columns.Add(new DataColumn("Branch"));
            dt.Columns.Add(new DataColumn("CloseDate"));
            dt.Columns.Add(new DataColumn("CurBal"));
            dt.Columns.Add(new DataColumn("Currency"));
            dt.Columns.Add(new DataColumn("CusmOrAAcDegree"));
            dt.Columns.Add(new DataColumn("NextAmt"));
            dt.Columns.Add(new DataColumn("CustId"));
            dt.Columns.Add(new DataColumn("ExOdInt"));
            dt.Columns.Add(new DataColumn("FaxNo"));
            dt.Columns.Add(new DataColumn("FB_TELLER"));
            dt.Columns.Add(new DataColumn("Filler"));
            dt.Columns.Add(new DataColumn("GlobalHeadCode"));
            dt.Columns.Add(new DataColumn("HTelExt"));
            dt.Columns.Add(new DataColumn("HighContr"));
            dt.Columns.Add(new DataColumn("HoldAmt"));
            dt.Columns.Add(new DataColumn("HouseFlag"));
            dt.Columns.Add(new DataColumn("IntAcct"));
            dt.Columns.Add(new DataColumn("IntAmt"));
            dt.Columns.Add(new DataColumn("IntDate"));
            dt.Columns.Add(new DataColumn("InterBranch"));
            dt.Columns.Add(new DataColumn("InvestHoldVal"));
            dt.Columns.Add(new DataColumn("InvestType"));
            dt.Columns.Add(new DataColumn("LastDate"));
            dt.Columns.Add(new DataColumn("LgmbFlag"));
            dt.Columns.Add(new DataColumn("LstTax"));
            dt.Columns.Add(new DataColumn("MinRepayAmt"));
            dt.Columns.Add(new DataColumn("MinRepayDate"));
            dt.Columns.Add(new DataColumn("OpenDate"));
            dt.Columns.Add(new DataColumn("OtherHoldAmt"));
            dt.Columns.Add(new DataColumn("OverAmt"));
            dt.Columns.Add(new DataColumn("OverTax"));
            dt.Columns.Add(new DataColumn("PbBal"));
            dt.Columns.Add(new DataColumn("PortfolioNo"));
            dt.Columns.Add(new DataColumn("ProcessStsText"));
            dt.Columns.Add(new DataColumn("Rate"));
            dt.Columns.Add(new DataColumn("RollCnt"));
            dt.Columns.Add(new DataColumn("SecurityCompNo"));
            dt.Columns.Add(new DataColumn("SlyOtherTrfCnt"));
            dt.Columns.Add(new DataColumn("SlyOwnTrfCnt"));
            dt.Columns.Add(new DataColumn("SlyStfExchgRate"));
            dt.Columns.Add(new DataColumn("SlyWaiveTrfCnt"));
            dt.Columns.Add(new DataColumn("SlyWaiveWdCnt"));
            dt.Columns.Add(new DataColumn("StockHoldAmt"));
            dt.Columns.Add(new DataColumn("TdAmt"));
            dt.Columns.Add(new DataColumn("TermDay"));
            dt.Columns.Add(new DataColumn("TodayCheck"));
            dt.Columns.Add(new DataColumn("TrialFlag"));
            dt.Columns.Add(new DataColumn("TrueAmt"));
            dt.Columns.Add(new DataColumn("UnclChqUsed"));
            dt.Columns.Add(new DataColumn("VipCDI"));
            dt.Columns.Add(new DataColumn("VipCode"));
            dt.Columns.Add(new DataColumn("VipDegree"));
            dt.Columns.Add(new DataColumn("YearAmt"));
            dt.Columns.Add(new DataColumn("YearTax"));
            dt.Columns.Add(new DataColumn("YearTaxFcy"));
            dt.Columns.Add(new DataColumn("name"));
            dt.Columns.Add(new DataColumn("lonamt"));
            dt.Columns.Add(new DataColumn("MdHoldAmt"));

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
