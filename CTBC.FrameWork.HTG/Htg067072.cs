using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;
using System.Web;
using System.Xml;
using System.Data;
using System.Collections;


namespace CTBC.FrameWork.HTG
{
    public partial class Htg067072 : HtgXmlPara
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

            if (line.Count > 2)
                lineno = line.Count.ToString();

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

            // TX67072_Grp .. 有開欄位, 但電文沒有欄位的.. 如下: 
            //                              APPLNO,APPLNOB,CaseId,cCretDT,ErrType,Filler2,KfFlag,RspCode,RspMessage,SNO,TrnNum
            Dictionary<string, string> masterMapping = new Dictionary<string, string>() { { "ACTION", "Action" }, { "AMOUNT", "Amount" }, { "AMT_OPT", "AmtOpt" }, { "CARD_AVAIL", "CardAvail" }, { "CARD_BORROW_AMT", "CardBorrowAmt" }, { "CARD_CONN", "CardConn" }, { "CARD_LIMIT", "CardLimit" }, { "CARD_PAY", "CardPay" }, { "CARD_STATUS", "CardStatus" }, { "CARD_TMP_LIM", "CardTmpLim" }, { "CASH_CARD_CODE", "CashCardCode" }, { "CLEAR_STEP", "ClearStep" }, { "CONSULT_STATUS", "ConsultStatus" }, { "CONTRB_CODE", "ContrbCode" }, { "CONTROL_AREA", "ControlArea" }, { "CORP_GRP", "CorpGrp" }, { "CUST_AVAIL", "CustAvail" }, { "CUST_ID_NO", "CustIdNo" }, { "CUST_LIMIT", "CustLimit" }, { "CUST_NO", "CustNo" }, { "CUSTOMER_NAME", "CustomerName" }, { "CUST_TITLE", "CustTitle" }, { "DISCLOS_AGRMT", "DisclosAgrmt" }, { "FB_AO_BRANCH", "FbAoBranch" }, { "FB_AO_NAME", "FbAoName" }, { "FILLER", "Filler1" }, { "FX_TR", "FxTr" }, { "ID_DUP_FLAG", "IdDupFlag" }, { "LOAN_BAL", "LoanBal" }, { "MAIL_LOAN_AMT", "MailLoanAmt" }, { "MAIL_LOAN_BAL", "MailLoanBal" }, { "MALU_H_AVL", "MaluHAvl" }, { "MALU_H_LIM", "MaluHLim" }, { "OPTION", "Option" }, { "PAGE_CNT", "PageCnt" }, { "PRE_APPR", "PreAppr" }, { "RANK", "Rank" }, { "RELA_ID", "RelaId" }, { "REST_STATUS", "RestStatus" }, { "SELECT", "Select" }, { "TIME", "Time" }, { "TOT_LIMIT", "TotLimit" }, { "VIP_DEGREE", "VipDegree" }, { "WSMT_STATUS", "WsmtStatus" } };
            Dictionary<string, string> detailMapping = new Dictionary<string, string>() { { "AMT", "Amt" }, { "APPDAT", "AppDat" }, { "BALAMT", "BalAmt" }, { "BRANC", "Branc" }, { "CURR", "Curr" }, { "DBBTYP", "DbbTyp" }, { "EXPDAT", "ExpDat" }, { "HOLD_FLAG", "HoldFlag" }, { "LIMNO", "LimNo" }, { "PRODU", "Produ" }, { "SIGN", "Sign" }, { "STATUS", "Status" }, { "STOPCD", "StopCd" } };
            //string[] detfield = { "AMT", "APPDAT", "BALAMT", "BRANC", "CURR", "DBBTYP", "EXPDAT", "HOLD_FLAG", "KF_FLAG", "LIMNO", "PRODU", "SIGN", "STATUS", "STOPCD" };

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

                    #region 若為第一頁則要insert dt067072Master
                    if ((lineflag == "2" && linedata.Count == 2) || lineflag == "1")
                    //if(i==0)
                    {
                        //寫入dt067072Master
                        DataRow _dr = dt067072Master.NewRow();
                        //foreach (XmlNode _node in xmldoc.SelectSingleNode("/hostgateway/line[@no='1']/msgBody").ChildNodes)
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
                                //_dr[_node.Attributes["id"].Value] = _node.Attributes["value"].Value;
                            }
                            catch (Exception ex1)//欄位有錯誤則不處理
                            {

                            }
                        }
                        dt067072Master.Rows.Add(_dr);

                    }
                    #endregion

                    #region 處理dt67072的資料


                    //
                    #region  每一頁有7筆,所以作7次

                    for (int idx = 1; idx <= 7; idx++)
                    {
                        DataRow _detr = dt067072Detail.NewRow();
                        foreach (var k in detailMapping)
                        {
                            string sield = k.Key;
                            string qield = k.Value;
                            try
                            {
                                _detr[qield] = xmldoc.SelectSingleNode("/hostgateway/line[@no='"
                                + lineflag + "']/msgBody/data[@id='" + sield + idx.ToString() + "']").Attributes["value"].Value;
                            }
                            catch (Exception ex3)
                            { }
                        }
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
                        dt067072Master.Rows[0]["CustAvail"] = GetDecimal(dt067072Master.Rows[0]["CustAvail"].ToString());
                        dt067072Master.Rows[0]["MailLoanAmt"] = GetDecimal(dt067072Master.Rows[0]["MailLoanAmt"].ToString());
                        dt067072Master.Rows[0]["MaluHAvl"] = GetDecimal(dt067072Master.Rows[0]["MaluHAvl"].ToString());
                        dt067072Master.Rows[0]["LoanBal"] = GetDecimal(dt067072Master.Rows[0]["LoanBal"].ToString());
                        dt067072Master.Rows[0]["Amount"] = GetDecimal(dt067072Master.Rows[0]["Amount"].ToString());
                        dt067072Master.Rows[0]["CardLimit"] = GetDecimal(dt067072Master.Rows[0]["CardLimit"].ToString());
                        dt067072Master.Rows[0]["CardAvail"] = GetDecimal(dt067072Master.Rows[0]["CardAvail"].ToString());
                        dt067072Master.Rows[0]["CardTmpLim"] = GetDecimal(dt067072Master.Rows[0]["CardTmpLim"].ToString());
                        dt067072Master.Rows[0]["MaluHLim"] = GetDecimal(dt067072Master.Rows[0]["MaluHLim"].ToString());
                        dt067072Master.Rows[0]["CardBorrowAmt"] = GetDecimal(dt067072Master.Rows[0]["CardBorrowAmt"].ToString());
                        dt067072Master.Rows[0]["MailLoanBal"] = GetDecimal(dt067072Master.Rows[0]["MailLoanBal"].ToString());
                        dt067072Master.Rows[0]["TotLimit"] = GetDecimal(dt067072Master.Rows[0]["TotLimit"].ToString());
                    }

                    XmlData.Tables.Add(dt067072Master);

                    //處理明細檔
                    if (dt067072Detail.Rows.Count > 0)
                    {
                        for (int idn = 0; idn < dt067072Detail.Rows.Count; idn++)
                        {
                            dt067072Detail.Rows[idn]["Amt"] = GetDecimal(dt067072Detail.Rows[idn]["Amt"].ToString());
                            dt067072Detail.Rows[idn]["BalAmt"] = GetDecimal(dt067072Detail.Rows[idn]["BalAmt"].ToString());
                            //dt067072Detail.Rows[idn]["AppDat"] = GetdataYYYYMMDD(dt067072Detail.Rows[idn]["AppDat"].ToString());
                            //dt067072Detail.Rows[idn]["ExpDat"] = GetdataYYYYMMDD(dt067072Detail.Rows[idn]["ExpDat"].ToString());
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
            string outputCode = "";
            XmlDocument xmldoc = new XmlDocument();
            //XmlNodeList header;
            XmlNodeList line;
            XmlNode data;
            xmldoc.LoadXml(xmldata);
            line = xmldoc.GetElementsByTagName("line");
            for (int linno = line.Count; linno >= 1; linno--)
            {
                try
                {
                    data = xmldoc.SelectSingleNode("/hostgateway/line[@no='" + linno.ToString() + "']/msgBody/data[@id='outputCode']");
                    if (data != null)
                    {
                        outputCode = data.Attributes["value"].Value;
                        break;
                    }
                }
                catch { }

            }


            //if (line.Count > 1)
            //{
            //    data = xmldoc.SelectSingleNode("/hostgateway/line[@no='2']/msgBody/data[@id='outputCode']");
            //    outputCode = data.Attributes["value"].Value;
            //}
            //else
            //{
            //    //只有一筆要檢查是第二次查還是第一次查
            //    data = xmldoc.SelectSingleNode("/hostgateway/line[@no='1']/msgBody/data[@id='outputCode']");
            //    outputCode = data.Attributes["value"].Value;
            //}

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
            DataTable dt = new DataTable("TX_67072_Detl");
            //dt.Columns.Add(new DataColumn("SNO"));
            //dt.Columns.Add(new DataColumn("FKSNO"));
            dt.Columns.Add(new DataColumn("Branc"));
            dt.Columns.Add(new DataColumn("LimNo"));
            dt.Columns.Add(new DataColumn("Produ"));
            dt.Columns.Add(new DataColumn("AppDat"));
            dt.Columns.Add(new DataColumn("ExpDat"));
            dt.Columns.Add(new DataColumn("Amt"));
            dt.Columns.Add(new DataColumn("Curr"));
            dt.Columns.Add(new DataColumn("BalAmt"));
            dt.Columns.Add(new DataColumn("Sign"));
            dt.Columns.Add(new DataColumn("DbbTyp"));
            dt.Columns.Add(new DataColumn("Status"));
            dt.Columns.Add(new DataColumn("StopCd"));
            dt.Columns.Add(new DataColumn("HoldFlag"));
            //dt.Columns.Add(new DataColumn("APPLNO"));
            //dt.Columns.Add(new DataColumn("APPLNOB"));
            //dt.Columns.Add(new DataColumn("CUST_ID"));
            //dt.Columns.Add(new DataColumn("CaseId"));

            return dt;

        }

        /// <summary>
        /// set up data container
        /// </summary>
        /// <returns></returns>
        private DataTable Createdt067072Master()
        {
            DataTable dt = new DataTable("TX_67072_Grp");
            //dt.Columns.Add(new DataColumn("SNO"));
            //dt.Columns.Add(new DataColumn("APPLNO"));
            //dt.Columns.Add(new DataColumn("APPLNOB"));
            //dt.Columns.Add(new DataColumn("RspCode"));
            //dt.Columns.Add(new DataColumn("TrnNum"));
            dt.Columns.Add(new DataColumn("ErrType"));
            dt.Columns.Add(new DataColumn("RspMessage"));
            dt.Columns.Add(new DataColumn("Action"));
            dt.Columns.Add(new DataColumn("CustIdNo"));
            dt.Columns.Add(new DataColumn("CustTitle"));
            dt.Columns.Add(new DataColumn("CustNo"));
            dt.Columns.Add(new DataColumn("CustomerName"));
            dt.Columns.Add(new DataColumn("RelaId"));
            dt.Columns.Add(new DataColumn("Filler1"));
            dt.Columns.Add(new DataColumn("Filler2"));
            dt.Columns.Add(new DataColumn("CorpGrp"));
            dt.Columns.Add(new DataColumn("Rank"));
            dt.Columns.Add(new DataColumn("Amount"));
            dt.Columns.Add(new DataColumn("DisclosAgrmt"));
            dt.Columns.Add(new DataColumn("CardLimit"));
            dt.Columns.Add(new DataColumn("RestStatus"));
            dt.Columns.Add(new DataColumn("CardStatus"));
            dt.Columns.Add(new DataColumn("CardPay"));
            dt.Columns.Add(new DataColumn("CardTmpLim"));
            dt.Columns.Add(new DataColumn("CustLimit"));
            dt.Columns.Add(new DataColumn("MaluHLim"));
            dt.Columns.Add(new DataColumn("CardAvail"));
            dt.Columns.Add(new DataColumn("CustAvail"));
            dt.Columns.Add(new DataColumn("MaluHAvl"));
            dt.Columns.Add(new DataColumn("CardBorrowAmt"));
            dt.Columns.Add(new DataColumn("TotLimit"));
            dt.Columns.Add(new DataColumn("LoanBal"));
            dt.Columns.Add(new DataColumn("PageCnt"));
            dt.Columns.Add(new DataColumn("Select"));
            dt.Columns.Add(new DataColumn("ControlArea"));
            dt.Columns.Add(new DataColumn("Time"));
            dt.Columns.Add(new DataColumn("FxTr"));
            dt.Columns.Add(new DataColumn("AmtOpt"));
            dt.Columns.Add(new DataColumn("Option"));
            dt.Columns.Add(new DataColumn("MailLoanAmt"));
            dt.Columns.Add(new DataColumn("MailLoanBal"));
            dt.Columns.Add(new DataColumn("CardConn"));
            dt.Columns.Add(new DataColumn("WsmtStatus"));
            dt.Columns.Add(new DataColumn("VipDegree"));
            dt.Columns.Add(new DataColumn("PreAppr"));
            dt.Columns.Add(new DataColumn("ContrbCode"));
            dt.Columns.Add(new DataColumn("FbAoBranch"));
            dt.Columns.Add(new DataColumn("FbAoName"));
            dt.Columns.Add(new DataColumn("CashCardCode"));
            dt.Columns.Add(new DataColumn("ConsultStatus"));
            dt.Columns.Add(new DataColumn("KfFlag"));
            dt.Columns.Add(new DataColumn("ClearStep"));
            dt.Columns.Add(new DataColumn("IdDupFlag"));
            //dt.Columns.Add(new DataColumn("cCretDT"));
            //dt.Columns.Add(new DataColumn("CaseId"));
            return dt;

        }
    }
}
