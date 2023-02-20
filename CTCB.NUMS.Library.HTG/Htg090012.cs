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
    public partial class Htg090012 : HtgXmlPara
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
                                  <data id=""sessionId"" value=""#sessionId#""/>
                                  <data id=""applicationId"" value=""default""/>
                                  <data id=""luType"" value=""LU62""/>
                                  <data id=""transactionId"" value=""090012""/>
                                 </header>
                                 <body>
                                  <data id=""WX-CUSTOMER-ID"" value=""#WX-CUSTOMER-ID#""/>
                                 </body>
                                </hostgateway>";

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

            string recxmldata ="";
            strmessage = "";
            DataSet XmlData = new DataSet();
            XmlDocument xmldoc = new XmlDocument();
            XmlNodeList linedata;
            string _class = "", _name = "", _originvesamtsign = "", _refcurramtsign = "", _refinvestnetsign = "", _refreturnratesign = "";
            Decimal _originvesamt = 0, _refcurramt = 0, _refinvestnet = 0, _refreturnrate = 0;

            //主機回傳碼
            string outputcode = "";
            //主機回傳訊息
            string msg = "";
            try
            {
                #region 建立資料表
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
                #endregion 

                //逐一拆解xml資料
                for (int i = 0; i < recxmldatalist.Count; i++)
                {
                    recxmldata = recxmldatalist[i].ToString() ;
                    xmldoc.LoadXml(recxmldata);
                    linedata = xmldoc.GetElementsByTagName("line");

                    #region 先檢查是否有資料

                    foreach (XmlNode _line in linedata)
                    {
                        //檢核line no="1"的outputCode
                        if (_line.Attributes["no"].Value == "1")
                        {
                            //
                            foreach (XmlNode datanode in _line.ChildNodes[1])
                            {
                                switch (datanode.Attributes["id"].Value)
                                {
                                    case "outputCode":
                                        outputcode = datanode.Attributes["value"].Value;
                                        break;
                                    case "msg":
                                        msg = datanode.Attributes["value"].Value;
                                        break;
                                }

                            }
                        }
                    }

                    //不是03就不用拆解了,但結果是正確的
                    if (outputcode != "03")
                    {
                        strmessage = "OutPutCode:" + outputcode + " ,Message:" + msg;
                        break;
                    }
                    #endregion


                    #region 建立資料


                    #region
                    foreach (XmlNode _line in linedata)
                    {
                        _class = _line.Attributes["no"].Value;

                        //有資料的部分只到第7筆
                        if (Convert.ToInt16(_class) <= 7)
                        {
                            _name = ""; _originvesamtsign = ""; _refcurramtsign = ""; _refinvestnetsign = ""; _refreturnratesign = "";
                            _originvesamt = 0; _refcurramt = 0; _refinvestnet = 0; _refreturnrate = 0;

                            DataRow _dr = dt090012master.NewRow();
                            foreach (XmlNode _data in _line.ChildNodes[1].ChildNodes)
                            {
                                #region
                                switch (_data.Attributes["id"].Value)
                                {
                                    case "CNAME":
                                        _name = _data.Attributes["value"].Value.Trim();
                                        break;
                                    case "BAL":
                                        _originvesamt = Convert.ToDecimal(_data.Attributes["value"].Value.Trim());
                                        break;
                                    case "BALSIGN":
                                    case "BAL-SIGN":
                                        _originvesamtsign = _data.Attributes["value"].Value.Trim();
                                        break;
                                    case "BAL2":
                                        _refcurramt = Convert.ToDecimal(_data.Attributes["value"].Value.Trim());
                                        break;
                                    case "BAL2SIGN":
                                        _refcurramtsign = _data.Attributes["value"].Value.Trim();
                                        break;
                                    case "BAL3":
                                        _refinvestnet = Convert.ToDecimal(_data.Attributes["value"].Value.Trim());
                                        break;
                                    case "BALSIGN3":
                                        _refinvestnetsign = _data.Attributes["value"].Value.Trim();
                                        break;
                                    case "BAL4":
                                        _refreturnrate = Convert.ToDecimal(_data.Attributes["value"].Value.Trim());
                                        break;
                                    case "BALSIGN4":
                                        _refreturnratesign = _data.Attributes["value"].Value.Trim();
                                        break;
                                }

                                #endregion
                            }

                            //數值轉換
                            _originvesamt = _originvesamt * ((_originvesamtsign == "+") ? 1 : -1);
                            _refcurramt = _refcurramt * ((_refcurramtsign == "+") ? 1 : -1);
                            _refinvestnet = _refinvestnet * ((_refinvestnetsign == "+") ? 1 : -1);
                            _refreturnrate = _refreturnrate * ((_refreturnratesign == "+") ? 1 : -1);
                            _dr["Class"] = _class;
                            _dr["ClassName"] = _name;
                            _dr["OrigInvestAmt"] = _originvesamt;
                            _dr["RefCurrAmt"] = _refcurramt;
                            _dr["RefInvestNet"] = _refinvestnet;
                            _dr["RefReturnRate"] = _refreturnrate;

                            dt090012master.Rows.Add(_dr);

                        }

                    }
                    #endregion



                    #endregion


                }

                XmlData.Tables.Add(dt090012master);

   
            }
            catch (Exception exe)
            {
                throw exe;
            }

            return XmlData;

        }


        /// <summary>
        /// 是否sing on lu0,090012無須sign lu0
        /// </summary>
        /// <returns></returns>
        public override bool CheckIsSignOnlu0()
        {
            return false;
        }


    }


}
