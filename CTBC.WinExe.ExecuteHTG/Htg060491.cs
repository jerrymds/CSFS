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
    public partial class Htg060491 : HtgXmlPara
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
                                    <data id=""transactionId"" value=""060490""/>
                                    <data id=""sessionId"" value=""#sessionId#""/>
                                    <data id=""lowValue"" value=""""/>
                                  </header>
                                  <body>
                                    <data value=""0"" id=""CURRENCY_TYPE""/>
                                    <data value="""" id=""CUSTOMER_NO""/>
                                    <data value=""01"" id=""ID_TYPE""/>
                                    <data value=""05102016"" id=""CLOSED_DATE""/>
                                    <data value=""CSFS"" id=""applicationId""/>
                                    <data value=""S"" id=""ENQUIRY_OPTION""/>
                                    <data value=""B100233332"" id=""CUST_ID_NO""/>
                                    <data value=""1"" id=""ACCOUNT_STATUS""/>
                                    <data value=""ALL"" id=""PRODUCT_OPTION""/>
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
            datanode.Attributes["value"].Value = "060491";

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
            //DataTable dt067072Master = Createdt067072Master();
            //DataTable dt067072Detail = Createdt067072Detail();
            DataTable dt060491Master = Createdt060491Master();
            DataTable dt060491Detail = Createdt060491Detail();


            string lineflag = "";

            //主機回傳碼
            string outputcode = "";

            string _currentpageno = "1";
            //string _currentpageseq="1";

            ////string CONTROL_AREA = "";

            string totallimit = "0";


            // 由於電文的欄位名稱與DB的欄位名稱 不太相同, 因此用以下來mapping 欄位名稱
            Dictionary<string, string> masterMapping = new Dictionary<string, string>()  
            { 
                {"ACTION","Action"},{"ADD1","Addr1"},{"ADD2","Addr2"},{"ADD3","Addr3"},{"AMT","Amt"},{"ASSET_VAR","AssetVar"},{"BIR_DATE","BirthDt"},{"CARD_FLAG","CardFlag"},{"CARD_LIMIT","CardLimit"},{"CONTRIB","Contrib"},{"CUST_ID_NO","CustomerId"},{"CUSTOMER_NAME","CustomerName"},{"CUSTOMER_NO","CustomerNo"},{"CUST_TYPE","CustType"},{"DEP_TOT","DepTot"},{"EMAIL","Email"},{"ENQ_OPT","EnqOpt"},{"FB_AO_BRANCH","FbAoBranch"},{"FB_AO_CODE","FbAoCode"},{"FB_TELLER","FbTeller"},{"FUND_CIF","FundCif"},{"HIGH_CONTR","HighContr"},{"INPUT_MSG_TYPE","InputMsgType"},{"KEEP_CURRENCY","KeepCurrency"},{"KEEP_ENQ_CLS_DATE","KeepEnqClsDate"},{"KEEP_OPT","KeepOpt"},{"KEEP_READ_FLAG","KeepReadFlag"},{"KEEP_STS","KeepSts"},{"KEEP_WA_IDX","KeepWaIdx"},{"LGMB_FLAG","LgmbFlag"},{"LON_TOT","LonTot"},{"MOBIL_NO","MobilNo"},{"MUTLT_FLAG","MutltFlag"},{"MUT_TOT","MutTot"},{"NET_ASSET","NetAsset"},{"NO_OF_CARDS","NoOfCards"},{"OCPN_DESC","OcpnDesc"},{"OLD_FLAG","OldFlag"},{"RANK","Rank"},{"RISK_ATTRIB","RiskAttrib"},{"RM_NO","RMNum"},{"SELECT_NO","SelectNo"},{"SERVICE_CODE1","ServiceCode1"},{"TEL_DAY","TelDay"},{"TEL_DAY_EXT","TelDayExt"},{"TEL_NIG","TelNig"},{"TEL_NIG_EXT","TelNigExt"},{"TRIAL_FLAG","TrialFlag"},{"TRUST_ONE_ACTUAL","TrustOneActual"},{"TRUST_ONE_APPL","TrustOneAppl"},{"VIP_CD_H","VipCdH"},{"VIP_CD_I","VipCdI"},{"VIP_CODE","VIPCode"},{"VIP_DEGREE","VipDegree"},{"WM_ASSET_AMT","WmAssetAmt"}
            };


            string[] masfield = { "SNO", "RspCode", "ErrType", "RspMessage", "CustomerNo", "CustType", "RMNum", "TelDay", "TelDayExt", "CustomerId", "BirthDt", "TelNig", "TelNigExt", "CustomerName", "Addr1", "TrustOneAppl", "TrustOneActual", "Addr2", "Rank", "Amt", "Addr3", "NetAsset", "DepTot", "NoOfCards", "MutTot", "LonTot", "CardLimit", "WmAssetAmt", "MobilNo", "Email", "OcpnDesc", "SelectNo", "KeepSts", "KeepOpt", "Action", "KeepReadFlag", "MutltFlag", "CardFlag", "KeepEnqClsDate", "VIPCode", "Contrib", "VipDegree", "FbAoBranch", "FbTeller", "KeepCurrency", "KeepRecno", "KeepWaIdx", "ServiceCode1", "FbAoCode", "RiskAttrib", "VipCdI", "VipCdH", "HighContr", "InputMsgType", "HouseholdFlag", "TrialFlag", "AssetVar", "LgmbFlag", "FundCif", "EnqOpt", "MnthsSnc", "VipDegreeH", "OldFlag", "SboxFlag", "cCretDT", "CaseId" };
            string[] detfield = { "ACCOUNT_1", "BRANCH_1", "STS_DESC1", "PROD_CODE1", "PROD_DESC1", "LINK_1", "CCY_1", "BAL_1", "SYSTEM_1", "SEGMENT_CODE_1", "ACCOUNT_2", "BRANCH_2", "STS_DESC2", "PROD_CODE2", "PROD_DESC2", "LINK_2", "CCY_2", "BAL_2", "SYSTEM_2", "SEGMENT_CODE_2", "ACCOUNT_3", "BRANCH_3", "STS_DESC3", "PROD_CODE3", "PROD_DESC3", "LINK_3", "CCY_3", "BAL_3", "SYSTEM_3", "SEGMENT_CODE_3", "ACCOUNT_4", "BRANCH_4", "STS_DESC4", "PROD_CODE4", "PROD_DESC4", "LINK_4", "CCY_4", "BAL_4", "SYSTEM_4", "SEGMENT_CODE_4", "ACCOUNT_5", "BRANCH_5", "STS_DESC5", "PROD_CODE5", "PROD_DESC5", "LINK_5", "CCY_5", "BAL_5", "SYSTEM_5", "SEGMENT_CODE_5", "ACCOUNT_6", "BRANCH_6", "STS_DESC6", "PROD_CODE6", "PROD_DESC6", "LINK_6", "CCY_6", "BAL_6", "SYSTEM_6", "SEGMENT_CODE_6" };
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
                    lineflag = "0";
                    for (int lineno = 1; lineno <= linedata.Count; lineno++)
                    {
                        string st1= "/hostgateway/line[@no='" + lineno + "']/msgBody/data[@id='outputCode']";
                        XmlNode _node = xmldoc.SelectSingleNode("/hostgateway/line[@no='" + lineno + "']/msgBody/data[@id='outputCode']");
                        if (_node != null)
                            if (_node.Attributes["value"].Value.Trim().Equals("03"))
                                lineflag = lineno.ToString();
                    }

                    if(lineflag=="0") // 表示沒有outputCode=03的.. 電文有錯
                        throw new Exception("電文格式不正確!!!");

                    #region 若為第一頁則要insert dt060491Master

                        //寫入dt060491Master
                        DataRow _dr = dt060491Master.NewRow();
                        foreach (XmlNode _node in xmldoc.SelectSingleNode("/hostgateway/line[@no='" + lineflag+ "']/msgBody").ChildNodes)
                        //foreach (XmlNode _node in xmldoc.SelectSingleNode("/hostgateway/line[@no='1']/msgBody").ChildNodes)
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
                        dt060491Master.Rows.Add(_dr);


                    //if (linedata.Count == 1)           
                    //{
                    //    DataRow _dr = dt060491Master.NewRow();
                    //    foreach (XmlNode _node in xmldoc.SelectSingleNode("/hostgateway/line[@no='1']/msgBody").ChildNodes)
                    //    //foreach (XmlNode _node in xmldoc.SelectSingleNode("/hostgateway/line[@no='1']/msgBody").ChildNodes)
                    //    {
                    //        try
                    //        {
                    //            var eFieldName = _node.Attributes["id"].Value.ToUpper().Trim();

                    //            bool isMasterField = masterMapping.ContainsKey(eFieldName);
                    //            if (isMasterField) // 找出對映DB的欄位名稱
                    //            {
                    //                string dbFieldName = masterMapping[eFieldName];
                    //                _dr[dbFieldName] = _node.Attributes["value"].Value;
                    //            }
                    //        }
                    //        catch (Exception ex1)
                    //        //欄位有錯誤則不處理
                    //        {
                    //            throw new Exception(_node.Attributes["id"].ToString() + " 找不到!!!");
                    //        }
                    //    }
                    //    dt060491Master.Rows.Add(_dr);
                    //}



                    #region  每一頁有6筆,所以作6次

                    string[] detailFiled2 = { "ACCOUNT_", "BRANCH_", "STS_DESC", "PROD_CODE", "PROD_DESC", "LINK_", "CCY_", "BAL_", "SYSTEM_", "SEGMENT_CODE_" };
                    string[] detailFiled3 = { "Account", "Branch", "StsDesc", "ProdCode", "ProdDesc", "Link", "Ccy", "Bal", "System", "SegmentCode" };
                    
                    for (int idx = 1; idx <= 6; idx++)
                    {
                        DataRow _detr = dt060491Detail.NewRow();
                        for (int ii = 0; ii < detailFiled2.Length; ii++)
                        {
                            string sield = detailFiled2[ii];
                            string qield = detailFiled3[ii];
                            try
                            {
                                _detr[qield] = xmldoc.SelectSingleNode("/hostgateway/line[@no='"
                                + lineflag + "']/msgBody/data[@id='" + sield + idx.ToString() + "']").Attributes["value"].Value;
                            }
                            catch (Exception ex3)
                            { }
                        }
                        dt060491Detail.Rows.Add(_detr);                            
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
                    if (dt060491Master.Rows.Count > 0)
                    {
                        //主檔處理資料數值
                        //dt060491Master.Rows[0]["CONTROL_AREA"] = "";
                        dt060491Master.Rows[0]["AMT"] = GetDecimal(dt060491Master.Rows[0]["AMT"].ToString());
                    }



                    XmlData.Tables.Add(dt060491Master);
                    DataTable newDt060491Detail = Createdt060491Detail();
                    List<string> noSave = new List<string>() { "結清", "已貸", "啟用", "誤開", "新戶", "核淮", "婉拒", "作廢" };
                    //處理明細檔
                    //if (dt060491Detail.Rows.Count > 0)
                    //{
                        foreach (DataRow dr in dt060491Detail.Rows)
                        {

                            bool bfilter = true;
                            #region 判斷是否是現金卡等等
                            //// 若帳戶是00000000000000000
                            if (dr["Account"].ToString().StartsWith("000000000000"))
                                bfilter = false;
                            //// 若 prod_code = 0058, 或XX80 , 不用存                            
                            //if (dr["ProdCode"].ToString().Equals("0058") || dr["ProdCode"].ToString().EndsWith("80"))
                            //    bfilter = false;

                            //// 若  Link<>'JOIN' , 不用存
                            //if (dr["Link"].ToString().Equals("JOIN"))
                            //    bfilter = false;

                            //// 若 StsDesc='結清' AND  StsDesc='已貸' AND  StsDesc='啟用' AND  StsDesc='誤開'  AND  StsDesc='新戶', 也不用存
                            //string sdesc = dr["StsDesc"].ToString().Trim();
                            //if (noSave.Contains(sdesc))
                            //    bfilter = false;
                            #endregion

                            if (bfilter)
                            {
                                dr["BAL"] = GetDecimal(dr["BAL"].ToString());
                                #region  搬到新的TABLE上
                                DataRow dr1 = newDt060491Detail.NewRow();
                                string _account = dr["Account"].ToString().Trim();
                                
                                    dr1["Account"] = _account;
                         
                                dr1["Branch"]=dr["Branch"];
                                dr1["StsDesc"]=dr["StsDesc"];
                                dr1["ProdCode"]=dr["ProdCode"];
                                dr1["ProdDesc"]=dr["ProdDesc"];
                                dr1["Link"]=dr["Link"];
                                dr1["Ccy"]=dr["Ccy"];
                                dr1["Bal"]=dr["Bal"];
                                dr1["System"]=dr["System"];
                                dr1["SegmentCode"] = dr["SegmentCode"];
                                newDt060491Detail.Rows.Add(dr1);
                                #endregion
                            }
                            
                        }
                        XmlData.Tables.Add(newDt060491Detail);
                    //}
                    
                }
                catch (Exception exe2)
                {
                    throw exe2;
                }

                #endregion

            }

            return XmlData;

        }

        private DataTable Createdt060491Detail()
        {
            DataTable dt = new DataTable("TX_60491_Detl");
            //dt.Columns.Add(new DataColumn("SNO"));
            //dt.Columns.Add(new DataColumn("FKSNO"));
            dt.Columns.Add(new DataColumn("Account"));
            dt.Columns.Add(new DataColumn("Branch"));
            dt.Columns.Add(new DataColumn("StsDesc"));
            dt.Columns.Add(new DataColumn("ProdCode"));
            dt.Columns.Add(new DataColumn("ProdDesc"));
            dt.Columns.Add(new DataColumn("Link"));
            dt.Columns.Add(new DataColumn("Ccy"));
            dt.Columns.Add(new DataColumn("Bal"));
            dt.Columns.Add(new DataColumn("System"));
            dt.Columns.Add(new DataColumn("SegmentCode"));
            //dt.Columns.Add(new DataColumn("CUST_ID"));
            //dt.Columns.Add(new DataColumn("CaseId"));
            //資料是位於第n頁
            //dt.Columns.Add(new DataColumn("CurrentPageNo"));
            ////資料是位於該頁的第n筆
            //dt.Columns.Add(new DataColumn("CurrentPageSeqNo"));
            ////該帳號是否為F
            //dt.Columns.Add(new DataColumn("FACILITY"));

            return dt;
        }

        private DataTable Createdt060491Master()
        {
            //            string[] masfield = { "SNO", "RspCode", "ErrType", "RspMessage", "CustomerNo", "CustType", "RMNum", "TelDay", "TelDayExt", "CustomerId", "BirthDt", "TelNig", "TelNigExt", "CustomerName", "Addr1", "TrustOneAppl", "TrustOneActual", "Addr2", "Rank", "Amt", "Addr3", "NetAsset", "DepTot", "NoOfCards", "MutTot", "LonTot", "CardLimit", "WmAssetAmt", "MobilNo", "Email", "OcpnDesc", "SelectNo", "KeepSts", "KeepOpt", "Action", "KeepReadFlag", "MutltFlag", "CardFlag", "KeepEnqClsDate", "VIPCode", "Contrib", "VipDegree", "FbAoBranch", "FbTeller", "KeepCurrency", "KeepRecno", "KeepWaIdx", "ServiceCode1", "FbAoCode", "RiskAttrib", "VipCdI", "VipCdH", "HighContr", "InputMsgType", "HouseholdFlag", "TrialFlag", "AssetVar", "LgmbFlag", "FundCif", "EnqOpt", "MnthsSnc", "VipDegreeH", "OldFlag", "SboxFlag", "cCretDT", "CaseId" };
   
            DataTable dt = new DataTable("TX_60491_Grp");
            //dt.Columns.Add(new DataColumn("SNO"));
            dt.Columns.Add(new DataColumn("RspCode"));
            //dt.Columns.Add(new DataColumn("TrnNum"));
            dt.Columns.Add(new DataColumn("ErrType"));
            dt.Columns.Add(new DataColumn("RspMessage"));
            dt.Columns.Add(new DataColumn("CustomerNo"));
            dt.Columns.Add(new DataColumn("CustType"));
            dt.Columns.Add(new DataColumn("RMNum"));
            dt.Columns.Add(new DataColumn("TelDay"));
            dt.Columns.Add(new DataColumn("TelDayExt"));
            dt.Columns.Add(new DataColumn("CustomerId"));
            dt.Columns.Add(new DataColumn("BirthDt"));
            dt.Columns.Add(new DataColumn("TelNig"));
            dt.Columns.Add(new DataColumn("TelNigExt"));
            dt.Columns.Add(new DataColumn("CustomerName"));
            dt.Columns.Add(new DataColumn("Addr1"));
            dt.Columns.Add(new DataColumn("TrustOneAppl"));
            dt.Columns.Add(new DataColumn("TrustOneActual"));
            dt.Columns.Add(new DataColumn("Addr2"));
            dt.Columns.Add(new DataColumn("Rank"));
            dt.Columns.Add(new DataColumn("Amt"));
            dt.Columns.Add(new DataColumn("Addr3"));
            dt.Columns.Add(new DataColumn("NetAsset"));
            dt.Columns.Add(new DataColumn("DepTot"));
            dt.Columns.Add(new DataColumn("NoOfCards"));
            dt.Columns.Add(new DataColumn("MutTot"));
            dt.Columns.Add(new DataColumn("LonTot"));
            dt.Columns.Add(new DataColumn("CardLimit"));
            dt.Columns.Add(new DataColumn("WmAssetAmt"));
            dt.Columns.Add(new DataColumn("MobilNo"));
            dt.Columns.Add(new DataColumn("Email"));
            dt.Columns.Add(new DataColumn("OcpnDesc"));
            dt.Columns.Add(new DataColumn("SelectNo"));
            dt.Columns.Add(new DataColumn("KeepSts"));
            dt.Columns.Add(new DataColumn("KeepOpt"));
            dt.Columns.Add(new DataColumn("Action"));
            dt.Columns.Add(new DataColumn("KeepReadFlag"));
            dt.Columns.Add(new DataColumn("MutltFlag"));
            dt.Columns.Add(new DataColumn("CardFlag"));
            dt.Columns.Add(new DataColumn("KeepEnqClsDate"));
            dt.Columns.Add(new DataColumn("VIPCode"));
            dt.Columns.Add(new DataColumn("Contrib"));
            dt.Columns.Add(new DataColumn("VipDegree"));
            dt.Columns.Add(new DataColumn("FbAoBranch"));
            dt.Columns.Add(new DataColumn("FbTeller"));
            dt.Columns.Add(new DataColumn("KeepCurrency"));
            dt.Columns.Add(new DataColumn("KeepRecno"));
            dt.Columns.Add(new DataColumn("KeepWaIdx"));
            dt.Columns.Add(new DataColumn("ServiceCode1"));
            dt.Columns.Add(new DataColumn("FbAoCode"));
            dt.Columns.Add(new DataColumn("RiskAttrib"));
            dt.Columns.Add(new DataColumn("VipCdI"));
            dt.Columns.Add(new DataColumn("VipCdH"));
            dt.Columns.Add(new DataColumn("HighContr"));
            dt.Columns.Add(new DataColumn("InputMsgType"));
            dt.Columns.Add(new DataColumn("HouseholdFlag"));
            dt.Columns.Add(new DataColumn("TrialFlag"));
            dt.Columns.Add(new DataColumn("AssetVar"));
            dt.Columns.Add(new DataColumn("LgmbFlag"));
            dt.Columns.Add(new DataColumn("FundCif"));
            dt.Columns.Add(new DataColumn("EnqOpt"));
            dt.Columns.Add(new DataColumn("MnthsSnc"));
            dt.Columns.Add(new DataColumn("VipDegreeH"));
            dt.Columns.Add(new DataColumn("OldFlag"));
            dt.Columns.Add(new DataColumn("SboxFlag"));
            //dt.Columns.Add(new DataColumn("cCretDT"));
            //dt.Columns.Add(new DataColumn("CaseId"));
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
            DataTable dt = new DataTable("TX_60491_Detl");
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
            DataTable dt = new DataTable("TX_60491_Grp");
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
