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
    public partial class HtgXmlParaMsg
    {
        #region 全域變數
        private DataRow m_drApprMsgDefine;
        #endregion

        #region 私域變數

        #endregion


        public HtgXmlParaMsg(DataRow drApprMsgDefine)
        {
            m_drApprMsgDefine = drApprMsgDefine;
        }

        /// <summary>
        /// return session of connection to htg
        /// </summary>
        /// <returns></returns>
        public string EstablishSessionXml()
        {
            string strxml = @"<?xml version=""1.0""?>
                                <hostgateway>
                                  <header>
                                    <action type=""P8EstablishSession""/>
                                  </header>
                                  <body>
                                    <data id=""userId"" value=""#userId#""/>
                                    <data id=""passWord"" value=""#passWord#""/>
                                    <data id=""branchNo"" value=""#branchNo#""/>
                                    <data id=""racfId"" value=""#racfId#""/>
                                    <data id=""racfPassWord"" value=""#racfPassWord#""/>
                                    <data id=""signOnLU0"" value=""#signOnLU0#""/>
                                    <data id=""applicationId"" value=""#applicationId#""/>
                                    <data id=""ldapInfo"" value=""yes""/>
                                  </body>
                                </hostgateway>";

            return strxml;

        }

        /// <summary>
        /// return close session xml string for htg
        /// </summary>
        /// <returns></returns>
        public string CloseSessionXml()
        {
            string strxml = @"<?xml version=""1.0""?>
                                <hostgateway>
	                                <header>
		                                <action type=""closeSession""/>
	                                </header>
	                                <body>
		                                <data id=""sessionId"" value=""#sessionId#""/>
	                                </body>
                                </hostgateway>";

            return strxml;

        }

        /// <summary>
        /// 上行xml template,其他電文須override 此function 
        /// </summary>
        /// <returns></returns>
        public string GetPostTransactionXml(string strTxId)
        {
            /*
            string strxml = @"<?xml version=""1.0""?>
                                <hostgateway>
                                 <header>
                                  <action type=""transaction""/>
                                  <data id=""sessionId"" value=""#sessionId#""/>
                                  <data id=""applicationId"" value=""default""/>
                                  <data id=""luType"" value=""LU62""/>
                                  <data id=""transactionId"" value=""090012""/>
                                 </header>
                                 <body>
                                  <data id=""WX-CUSTOMER-ID"" value=""#WX-CUSTOMER-ID#""/>
                                 </body>
                                </hostgateway>";*/
            string strxml = m_drApprMsgDefine["PostTransactionXml"].ToString().Trim();

            return strxml;
        }

        /// <summary>
        /// 將上行xml template 套入參數,其他電文若有特別的處理則override 此function
        /// </summary>
        /// <param name="parmht"></param>
        /// <returns></returns>
        public string GetPostTransactionXml(string strTxId, Hashtable parmht)
        {
            string strxml = GetPostTransactionXml(strTxId);
            XmlDocument xmldoc = new XmlDocument();
            XmlNodeList header;
            XmlNodeList body;

            //取代session 
            strxml = strxml.Replace("#sessionId#", parmht["sessionId"].ToString());
            parmht.Remove("sessionId");

            //傳入值
            xmldoc.LoadXml(strxml);
            header = xmldoc.GetElementsByTagName("header");
            body = xmldoc.GetElementsByTagName("body");

            // header
            foreach (XmlNode snode in header.Item(0).ChildNodes)
            {
                if (snode.Name == "data")
                {
                    string nodeid = snode.Attributes["id"].Value;
                    if (parmht.ContainsKey(nodeid))
                    {
                        snode.Attributes["value"].Value = parmht[nodeid].ToString();
                    }
                }

            }

            // body
            foreach (XmlNode snode in body.Item(0).ChildNodes)
            {
                if (snode.Name == "data")
                {
                    string nodeid = snode.Attributes["id"].Value;
                    if (parmht.ContainsKey(nodeid))
                    {
                        snode.Attributes["value"].Value = parmht[nodeid].ToString();
                    }
                }

            }


            strxml = xmldoc.InnerXml;

            return strxml;
        }

        /// <summary>
        /// 將傳入的xmldata 套入參數,其他電文若有特別的處理則override 此function
        /// </summary>
        /// <param name="postxml"></param>
        /// <param name="parmht"></param>
        /// <returns></returns>
        public string GetPostTransactionXml(string strTxId, string postxml, Hashtable parmht)
        {
            return "";
        }

        /// <summary>
        /// 檢查是否有下一頁
        /// </summary>
        /// <param name="xmldata"></param>
        /// <returns></returns>
        public bool CheckIsHasNextPage(string strTxId, string xmldata)
        {
            return false;
        }

        /// <summary>
        /// 是否sing on lu0,視各電文而定
        /// </summary>
        /// <returns></returns>
        public bool CheckIsSignOnlu0(string txid)
        {
            string strRet = m_drApprMsgDefine["SignOnLU0"].ToString().Trim();
            if (strRet == "Y")
                return true;
            else
                return false;
        }

        /*
        /// <summary>
        /// 將回傳的xml電文轉成DataSet , 其他電文請override 此function
        /// </summary>
        /// <param name="recxml"></param>
        /// <returns></returns>
        public virtual DataSet TransferXmlToDataSet(string recxml)
        {
            return new DataSet();
        }
        */

        /// <summary>
        /// 將回傳的xml電文轉成DataSet ,適用於多頁處理, 其他電文請override 此function
        /// </summary>
        /// <param name="recxml"></param>
        /// <returns></returns>
        public DataSet TransferXmlToDataSet(DataTable dtRecvData, ArrayList recxmldatalist, string SessionKey, out string strmessage, out string strPopMsg)
        {
            string recxmldata = "";
            strmessage = "";
            strPopMsg = "";
            DataSet XmlData = new DataSet();
            XmlDocument xmldoc = new XmlDocument();
            XmlNodeList linedata;
            //string _class = "", _name = "", _originvesamtsign = "", _refcurramtsign = "", _refinvestnetsign = "", _refreturnratesign = "";
            //Decimal _originvesamt = 0, _refcurramt = 0, _refinvestnet = 0, _refreturnrate = 0;

            //主機回傳碼
            string outputcode = "";
            //主機回傳訊息
            string lineMsg = string.Empty;
            //string msg = "";

            /*
            // workstationNo
            string workstationNo = string.Empty;
            */

            try
            {
                #region 建立資料表
                /*
                //建立NUMSBank90012Master table
                DataTable dt090012master = new DataTable("dt090012Master");
                //資產類別
                dt090012master.Columns.Add(new DataColumn("Class"));
                //資產名稱
                dt090012master.Columns.Add(new DataColumn("ClassName"));
                //原始投資收益 OUT-90012-S-BAL
                dt090012master.Columns.Add(new DataColumn("OrigInvestAmt"));
                //目前參考市值 OUT-90012-S-BAL-2 
                dt090012master.Columns.Add(new DataColumn("RefCurrAmt"));
                //參考投資損益 OUT-90012-S-BAL-3 
                dt090012master.Columns.Add(new DataColumn("RefInvestNet"));
                //參考報酬% OUT-90012-S-BAL-4 
                dt090012master.Columns.Add(new DataColumn("RefReturnRate"));
                */
                //dtRecvData.Clear();
                DataTable dtTemp = new DataTable();
                dtTemp = dtRecvData.Clone();
                dtTemp.TableName = dtRecvData.TableName;
                #endregion

                //逐一拆解xml資料
                for (int i = 0; i < recxmldatalist.Count; i++)
                {
                    recxmldata = recxmldatalist[i].ToString();
                    xmldoc.LoadXml(recxmldata);
                    linedata = xmldoc.GetElementsByTagName("line");
                    XmlNode _lineData = null;
                    List<XmlNode> _xmlNodeList = new List<XmlNode>();

                    #region 先檢查是否有資料
                    foreach (XmlNode _line in linedata)
                    {
                        outputcode = "";
                        lineMsg = string.Empty;

                        //檢核line no="1"的outputCode
                        //if (_line.Attributes["no"].Value == "1")
                        //{

                        // msgbody
                        foreach (XmlNode datanode in _line.ChildNodes[1])
                        {
                            switch (datanode.Attributes["id"].Value)
                            {
                                case "outputCode":
                                    outputcode = datanode.Attributes["value"].Value;
                                    break;
                                case "ERRORCODE_OC01":
                                    lineMsg += "提示代碼:" + datanode.Attributes["value"].Value + " ";
                                    break;
                                case "ERRORMESSAGETEXT_OC01":
                                    lineMsg += "提示原因:" + datanode.Attributes["value"].Value + " ";
                                    break;
                                case "DATAMSG1_OC30":
                                    lineMsg += datanode.Attributes["value"].Value + " ";
                                    break;
                                case "DATAMSG2_OC30":
                                    lineMsg += datanode.Attributes["value"].Value + " ";
                                    break;
                                case "MSG":
                                    lineMsg += datanode.Attributes["value"].Value + " ";
                                    break;
                                default:
                                    break;
                            }
                        }

                        // 依設定判斷
                        string strOkOutputCodes = m_drApprMsgDefine["OkOutputCode"].ToString().Trim();
                        if (strOkOutputCodes.Contains(outputcode))
                        {
                            if (m_drApprMsgDefine["MsgName"].ToString().Trim() == "67108")
                            {
                                _xmlNodeList.Add(_line);
                            }
                            else
                            {
                                _xmlNodeList = new List<XmlNode>();
                                _xmlNodeList.Add(_line);
                            }
                        }

                        // 提示訊息都記
                        strPopMsg += lineMsg;

                        //}
                    }
                    #endregion


                    #region 建立資料
                    //foreach (XmlNode _line in linedata)
                    if (_xmlNodeList != null && _xmlNodeList.Count > 0)
                    {
                        DataRow _dr = dtTemp.NewRow();

                        // Session id
                        if (dtTemp.Columns.Contains("sessionId"))
                        {
                            _dr["sessionId"] = SessionKey;
                        }

                        foreach (XmlNode _lineXmlNode in _xmlNodeList)
                        {
                            XmlNode _line = _lineXmlNode;

                            // header
                            foreach (XmlNode _data in _line.ChildNodes[0].ChildNodes)
                            {
                                // NUMS有的欄位才需要記錄下來
                                if (dtTemp.Columns.Contains(_data.Attributes["id"].Value))
                                {
                                    if (m_drApprMsgDefine["MsgName"].ToString().Trim() == "67108")
                                    {
                                        if (_data.Attributes["value"].Value.Trim() != "")
                                        {
                                            _dr[_data.Attributes["id"].Value] = _data.Attributes["value"].Value.Trim();
                                        }
                                    }
                                    else
                                    {
                                        _dr[_data.Attributes["id"].Value] = _data.Attributes["value"].Value.Trim();
                                    }

                                }
                            }

                            // body
                            foreach (XmlNode _data in _line.ChildNodes[1].ChildNodes)
                            {
                                // NUMS有的欄位才需要記錄下來
                                if (dtTemp.Columns.Contains(_data.Attributes["id"].Value))
                                {
                                    if (m_drApprMsgDefine["MsgName"].ToString().Trim() == "67108")
                                    {
                                        if (_data.Attributes["value"].Value.Trim() != "")
                                        {
                                            _dr[_data.Attributes["id"].Value] = _data.Attributes["value"].Value.Trim();
                                        }
                                    }
                                    else
                                    {
                                        _dr[_data.Attributes["id"].Value] = _data.Attributes["value"].Value.Trim();
                                    }
                                }
                            }
                        }

                        dtTemp.Rows.Add(_dr);
                    }

                    #endregion
                }

                XmlData.Tables.Add(dtTemp);


            }
            catch (Exception exe)
            {
                throw exe;
            }

            return XmlData;
        }

        /// <summary>
        /// 數值轉換
        /// </summary>
        /// <param name="strn"></param>
        /// <returns></returns>
        public Decimal GetDecimal(string strn)
        {
            if (strn.Length == 0)
                return 0;
            Decimal result = 0;

            if (strn.LastIndexOf("+") != -1 || strn.LastIndexOf("-") != -1)
            {
                string sign = strn.Substring(strn.Length - 1, 1);
                if (sign == "+")
                {
                    result = Convert.ToDecimal(strn.Substring(0, strn.Length - 1));
                }
                else
                {
                    result = Convert.ToDecimal(strn.Substring(0, strn.Length - 1)) * -1;
                }

            }
            else
            {
                result = Convert.ToDecimal(strn);
            }


            return result;
        }


        /// <summary>
        /// mmddyyyy轉換成yyyymmdd
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public string GetdataYYYYMMDD(string str)
        {
            if (str.Length != 0)
                return "00000000";

            string strdate = "";
            strdate = str.Substring(4, 4) + str.Substring(2, 2) + str.Substring(0, 2);
            return strdate;

        }
    }


}
