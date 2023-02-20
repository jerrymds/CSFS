/// <summary>
/// 程式說明:電文上下行處理
/// 中國信託銀行 版權所有  ©  All Rights Reserved.
/// </summary>

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using System.Configuration;
using TIBCO.EMS;
using CTBC.CSFS.BussinessLogic;
using log4net;
using CTBC.CSFS.Models;
using CTBC.CSFS.Pattern;
using CTBC.FrameWork.Util;

namespace CTBC.FrameWork.ESB
{
    public class ExcuteESB
    {
        private const string strHerader1 = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>";
        private const string strHerader2 = "<ns0:ServiceEnvelope xmlns:ns0=\"http://ns.chinatrust.com.tw/XSD/CTCB/ESB/Message/EMF/ServiceEnvelope\">";
        private const string strHerader3 = "<ns1:ServiceHeader xmlns:ns1=\"http://ns.chinatrust.com.tw/XSD/CTCB/ESB/Message/EMF/ServiceHeader\">";
        //private static string ObligorID = "";       //歸戶ＩＤ
        //private static string MastID = "";
        //private static string TellerNo = "99605 ";          //銀電文登入TellerNo 
        private static string BranchID = ConfigurationManager.AppSettings["UserBranchId"].ToString();
        HostMsgGrpBIZ hostbiz = new HostMsgGrpBIZ();
        private static string IsnotTest = ConfigurationManager.AppSettings["IsnotTest"].ToString();//是否開啟電文測試1為開啟其他為不開啟
        private static string strSourceID = ConfigurationManager.AppSettings["SourceID"].ToString();
        private static string strUserBranchId = ConfigurationManager.AppSettings["UserBranchId"].ToString();
        //private static string BranchId = ConfigurationManager.AppSettings["UserBranchId"].ToString();
        private static string strTerminalId = ConfigurationManager.AppSettings["TerminalId"].ToString();
        private static string TellerNo = ConfigurationManager.AppSettings["ns2UserId"].ToString();  
        private static string ns2Password = ConfigurationManager.AppSettings["ns2Password"].ToString(); 
        private static int ReadCS = 1;

        /// <summary>
        /// 實際程序入口
        /// </summary>
        /// <param name="EsbNo">電文ID</param>
        /// <param name="ObligorID">義務人統編</param>
        /// <returns></returns>
        public string ESBMainFun(string EsbNo, string ObligorID,string CustomID,string CaseID)
        {
            Thread.Sleep(3000);
            string strResult = "";
            try
            {
                log4net.Config.XmlConfigurator.Configure(new System.IO.FileInfo(AppDomain.CurrentDomain.BaseDirectory + @"\Log.config"));
                
                //TellerNo = hostbiz.GetTellerNo();
                //BranchID = hostbiz.GetBranchId();
                
                switch (EsbNo)
                {
                    case "60491":
                        return P4_60491U(ObligorID,CaseID);
                        break;
                    case "67072":
                        return P4_67072U(ObligorID,CaseID);
                        break;
                    case "33401":
                        return P4_33401U(ObligorID, CustomID,CaseID);
                        break;
                    //20160122 RC --> 20150106 宏祥 add 新增67100電文
                    case "67100":
                        return P4_67100U(ObligorID,CaseID);
                        break;
                    case "009001":
                        return Login();
                        break;
                    case "":
                        return LogOut();
                        break;
                }                
            }
            catch (Exception ex)
            {
                WriteLog("程式異常，錯誤信息：" + ex.Message);
                strResult = "0002|程式異常";
            }
            return strResult;
        }
        /// 改用 CMS 67072
        /// 
        public string P4_67072U(string ObligorID,string CaseID)
        {
            string result = "";
            string strRspCode = "";
            string strXML = "";
            string strErrorType = "";
            string strErrorCode = "";
            string strErrorMessage = "";
            bool b67072 = true;
            bool bESBFlag = true;       //發送上行和接收下行電文是否有誤
            string strIdentityKey = "";
            DataTable dt = new DataTable();        //從表
            ArrayList array = new ArrayList();
            Dictionary<string, string> dicValue = new Dictionary<string, string>();   //主表
            IList<HostMsgGrp> resultlist = hostbiz.QueryHostMsgDetl("67072", "D");
            if (resultlist.Count > 0)
            {
                //2015-09-01
                //string strSno = hostbiz.GetMaxSno("67072").PadRight(19, ' ');
                //2015-09-01
                string strYear = Convert.ToInt16(DateTime.Now.Year - 1911).ToString("000");
                string strMonth = Convert.ToInt16(DateTime.Now.Month).ToString("00");
                string strDay = Convert.ToInt16(DateTime.Now.Day).ToString("00");
                string strHour = System.DateTime.Now.ToString("HHmmssfff");
                string strSno = "CSFS" + strYear.Substring(1,2) + strMonth + strDay + strHour;
                //strSno = strSno.Substring(0, strSno.Length - 2);
                Dictionary<string, DataTable> dic = new Dictionary<string, DataTable>();
                foreach (var list in resultlist)
                {
                    bool bExists = false;
                    string strTableName = list.dest_table;
                    string strEdata = list.edata.ToUpper();
                    DataTable dtNew = new DataTable();
                    if (dic.ContainsKey(strTableName))
                    {
                        bExists = true;
                        dtNew = dic[strTableName];
                    }
                    if (!dtNew.Columns.Contains(strEdata))
                    {
                        dtNew.Columns.Add(strEdata);
                    }
                    if (bExists)
                    {
                        dic[strTableName] = dtNew;
                    }
                    else
                    {
                        dic.Add(strTableName, dtNew);
                    }
                }
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(strHerader1);
                sb.AppendLine(strHerader2);
                sb.AppendLine(strHerader3);
                sb.AppendLine("<ns1:StandardType>BSMF</ns1:StandardType>");
                sb.AppendLine("<ns1:StandardVersion>01</ns1:StandardVersion>");
                sb.AppendLine("<ns1:ServiceName>ctLoanAcctListInq</ns1:ServiceName>");
                sb.AppendLine("<ns1:ServiceVersion>01</ns1:ServiceVersion>");
                sb.AppendLine("<ns1:SourceID>" + strSourceID + "</ns1:SourceID>");
                sb.AppendLine("<ns1:TransactionID>" + strSno + "</ns1:TransactionID>");
                sb.AppendLine("<ns1:RqTimestamp>" + System.DateTime.Now.ToString("yyyy-MM-dd") + "T" + System.DateTime.Now.ToString("HH:mm:ss.ff") + "+08:00" + "</ns1:RqTimestamp>");
                sb.AppendLine("</ns1:ServiceHeader>");
                sb.AppendLine("<ns1:ServiceBody xmlns:ns1=\"http://ns.chinatrust.com.tw/XSD/CTCB/ESB/Message/EMF/ServiceBody\">");
                sb.AppendLine("<ns2:ctLoanAcctListInqRq xmlns:ns2=\"http://ns.chinatrust.com.tw/XSD/CTCB/ESB/Message/BSMF/ctLoanAcctListInqRq/01\">");
                sb.AppendLine("<ns2:REQHDR>");
                sb.AppendLine("<ns2:TrnNum>" + strSno + "</ns2:TrnNum>");
                sb.AppendLine("<ns2:TrnCode>062072</ns2:TrnCode>");
                sb.AppendLine("<ns2:UserBranchId>" + strUserBranchId + "</ns2:UserBranchId>");
                sb.AppendLine("<ns2:UserId>" + TellerNo + "</ns2:UserId>");
                sb.AppendLine("</ns2:REQHDR><ns2:REQBDY>");
                sb.AppendLine("<ns2:Action></ns2:Action>");
                sb.AppendLine("<ns2:CustIdNo>" + ObligorID + "</ns2:CustIdNo>");
                sb.AppendLine("<ns2:CustNo></ns2:CustNo>");
                sb.AppendLine("<ns2:TxTr>N</ns2:TxTr>");
                sb.AppendLine("<ns2:Option>0</ns2:Option>");

                sb.AppendLine("</ns2:REQBDY>");
                sb.AppendLine("</ns2:ctLoanAcctListInqRq>");
                sb.AppendLine("</ns1:ServiceBody>");
                sb.AppendLine("</ns0:ServiceEnvelope>");

                string UXML = sb.ToString();
                while (b67072)
                {
                    try
                    {
                        strXML = UXML;
                        string bSendresult = SendESBData(strXML, "67072");
                        sb = new StringBuilder();
                        sb.AppendLine(strHerader1);
                        sb.AppendLine(strHerader2);
                        sb.AppendLine(strHerader3);
                        sb.AppendLine("<ns1:StandardType>BSMF</ns1:StandardType>");
                        sb.AppendLine("<ns1:StandardVersion>01</ns1:StandardVersion>");
                        sb.AppendLine("<ns1:ServiceName>ctLoanAcctListInq</ns1:ServiceName>");
                        sb.AppendLine("<ns1:ServiceVersion>01</ns1:ServiceVersion>");
                        sb.AppendLine("<ns1:SourceID>"+ strSourceID + "</ns1:SourceID>");
                        sb.AppendLine("<ns1:TransactionID>" + strSno + "</ns1:TransactionID>");
                        sb.AppendLine("<ns1:RqTimestamp>" + System.DateTime.Now.ToString("yyyy-MM-dd") + "T" + System.DateTime.Now.ToString("HH:mm:ss.ff") + "+08:00" + "</ns1:RqTimestamp>");
                        sb.AppendLine("</ns1:ServiceHeader>");

                        sb.AppendLine("<ns1:ServiceBody xmlns:ns1=\"http://ns.chinatrust.com.tw/XSD/CTCB/ESB/Message/EMF/ServiceBody\">");
                        sb.AppendLine("<ns2:ctLoanAcctListInqRq xmlns:ns2=\"http://ns.chinatrust.com.tw/XSD/CTCB/ESB/Message/BSMF/ctLoanAcctListInqRq/01\">");
                        sb.AppendLine("<ns2:REQHDR>");
                        sb.AppendLine("<ns2:TrnNum>" + strSno + "</ns2:TrnNum>");
                        sb.AppendLine("<ns2:TrnCode>067072</ns2:TrnCode>");
                        sb.AppendLine("<ns2:UserBranchId>" + strUserBranchId + "</ns2:UserBranchId>");
                        sb.AppendLine("<ns2:UserId>" + TellerNo + "</ns2:UserId>");
                        sb.AppendLine("</ns2:REQHDR><ns2:REQBDY>");

                        if (!string.IsNullOrEmpty(bSendresult))
                        {
                            XmlDocument xdoc = new XmlDocument();
                            xdoc.LoadXml(bSendresult);
                            XmlNamespaceManager mgr = new XmlNamespaceManager(xdoc.NameTable);
                            mgr.AddNamespace("ns0", "http://ns.chinatrust.com.tw/XSD/CTCB/ESB/Message/EMF/ServiceEnvelope");
                            mgr.AddNamespace("ns1", "http://ns.chinatrust.com.tw/XSD/CTCB/ESB/Message/EMF/ServiceBody");
                            mgr.AddNamespace("ns2", "http://ns.chinatrust.com.tw/XSD/CTCB/ESB/Message/BSMF/ctLoanAcctListInqRs/01");
                            XmlNode docNode = xdoc.SelectSingleNode("/ns0:ServiceEnvelope/ns1:ServiceBody/ns2:ctLoanAcctListInqRs/ns2:RESBDY/ns2:BDYREC", mgr);
                            if (docNode != null)
                            {
                                foreach (XmlNode item in docNode.ChildNodes)
                                {
                                    string strNodeName = string.Empty;
                                    if (item.Name.ToUpper().Equals("NS2:ACTION"))
                                    {
                                        strNodeName = "<" + item.Name + ">N</" + item.Name + ">";
                                    }
                                    else
                                    {
                                        strNodeName = "<" + item.Name + ">" + item.InnerText + "</" + item.Name + ">";
                                    }
                                    sb.AppendLine(strNodeName);
                                }
                            }
                        }

                        sb.AppendLine("</ns2:REQBDY>");
                        sb.AppendLine("</ns2:ctLoanAcctListInqRq>");
                        sb.AppendLine("</ns1:ServiceBody>");
                        sb.AppendLine("</ns0:ServiceEnvelope>");
                        UXML = sb.ToString();

                        if (bSendresult != "")
                        {
                            XmlDocument xmldoc = new XmlDocument();
                            xmldoc.LoadXml(bSendresult);
                            if (xmldoc != null)
                            {
                                XmlNamespaceManager mgr = new XmlNamespaceManager(xmldoc.NameTable);
                                mgr.AddNamespace("ns0", "http://ns.chinatrust.com.tw/XSD/CTCB/ESB/Message/EMF/ServiceEnvelope");
                                mgr.AddNamespace("ns1", "http://ns.chinatrust.com.tw/XSD/CTCB/ESB/Message/EMF/ServiceBody");
                                mgr.AddNamespace("ns2", "http://ns.chinatrust.com.tw/XSD/CTCB/ESB/Message/BSMF/ctLoanAcctListInqRs/01");
                                XmlNode nodenormal = xmldoc.SelectSingleNode("/ns0:ServiceEnvelope/ns1:ServiceBody/ns2:ctLoanAcctListInqRs/ns2:RESHDR/ns2:RspCode", mgr);
                                if (nodenormal != null)
                                {
                                    XmlNode node = xmldoc.SelectSingleNode("/ns0:ServiceEnvelope/ns1:ServiceBody/ns2:ctLoanAcctListInqRs/ns2:RESHDR", mgr);
                                    if (node != null)
                                    {
                                        for (int j = 0; j < node.ChildNodes.Count; j++)
                                        {
                                            string strColumnName = node.ChildNodes[j].Name.ToString().ToUpper().Replace("NS2:", "");
                                            string strValue = node.ChildNodes[j].InnerText;
                                            if (strColumnName == "RSPCODE")
                                            {
                                                strRspCode = strValue;   //獲取下行電文返回的狀態碼
                                            }
                                            if (!dicValue.ContainsKey(strColumnName))
                                            {
                                                IEnumerable<HostMsgGrp> m_List = resultlist.Where(m => m.datatype == "9" && m.edata.ToUpper().Equals(strColumnName));
                                                if (m_List != null && m_List.Count() > 0)
                                                {
                                                    strValue = ChangeTheFormat(strValue);
                                                }
                                                dicValue.Add(strColumnName, strValue);
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    b67072 = false;
                                    bESBFlag = false;
                                    XmlNode nodeerror = xmldoc.SelectSingleNode("/ns0:ServiceEnvelope/ns1:ServiceBody/ns2:ctLoanAcctListInqRs/ns2:RESHDR/ErrorType", mgr);
                                    if (nodeerror != null)
                                    {
                                        strErrorType = nodeerror.InnerText;
                                    }
                                    nodeerror = xmldoc.SelectSingleNode("/ns0:ServiceEnvelope/ns1:ServiceBody/ns2:ctLoanAcctListInqRs/ns2:RESHDR/ErrorCode", mgr);
                                    if (nodeerror != null)
                                    {
                                        strErrorCode = nodeerror.InnerText;
                                    }
                                    nodeerror = xmldoc.SelectSingleNode("/ns0:ServiceEnvelope/ns1:ServiceBody/ns2:ctLoanAcctListInqRs/ns2:RESHDR/ErrorMessage", mgr);
                                    if (nodeerror != null)
                                    {
                                        strErrorMessage = nodeerror.InnerText;
                                    }
                                }
                                //ESB回复正常且主机回复正常
                                if (strRspCode == "03")
                                {
                                    //hostbiz.Save67072_PAGEDATA(strApplNo, strApplNoB, bSendresult);
                                    XmlNodeList nodelists = xmldoc.SelectNodes("/ns0:ServiceEnvelope/ns1:ServiceBody/ns2:ctLoanAcctListInqRs/ns2:RESBDY/ns2:BDYREC", mgr);
                                    string strChild = ",BRANC1,LIMNO1,PRODU1,APPDAT1,EXPDAT1,AMT1,CURR1,BALAMT1,SIGN1,DBBTYP1,STATUS1,STOPCD1,HOLDFLAG1,BRANC2,LIMNO2,PRODU2,APPDAT2,EXPDAT2,AMT2,CURR2,BALAMT2,SIGN2,DBBTYP2,STATUS2,STOPCD2,HOLDFLAG2,BRANC3,LIMNO3,PRODU3,APPDAT3,EXPDAT3,AMT3,CURR3,BALAMT3,SIGN3,DBBTYP3,STATUS3,STOPCD3,HOLDFLAG3,BRANC4,LIMNO4,PRODU4,APPDAT4,EXPDAT4,AMT4,CURR4,BALAMT4,SIGN4,DBBTYP4,STATUS4,STOPCD4,HOLDFLAG4,BRANC5,LIMNO5,PRODU5,APPDAT5,EXPDAT5,AMT5,CURR5,BALAMT5,SIGN5,DBBTYP5,STATUS5,STOPCD5,HOLDFLAG5,BRANC6,LIMNO6,PRODU6,APPDAT6,EXPDAT6,AMT6,CURR6,BALAMT6,SIGN6,DBBTYP6,STATUS6,STOPCD6,HOLDFLAG6,BRANC7,LIMNO7,PRODU7,APPDAT7,EXPDAT7,AMT7,CURR7,BALAMT7,SIGN7,DBBTYP7,STATUS7,STOPCD7,HOLDFLAG7,";
                                    for (int i = 0; i < nodelists.Count; i++)
                                    {
                                        XmlNode PageCnt = xmldoc.SelectSingleNode("/ns0:ServiceEnvelope/ns1:ServiceBody/ns2:ctLoanAcctListInqRs/ns2:RESBDY/ns2:BDYREC/ns2:PageCnt", mgr);
                                        if (PageCnt == null)
                                        {
                                            b67072 = false;
                                            break;
                                        }
                                        else if (PageCnt != null && int.Parse(PageCnt.InnerText) < 7)
                                        {
                                            b67072 = false;
                                        }
                                        Dictionary<string, string> dicValueDetl = new Dictionary<string, string>();   //從表
                                        for (int j = 1; j <= int.Parse(PageCnt.InnerText); j++)
                                        {
                                            XmlNode Branc = nodelists[i].SelectSingleNode("ns2:Branc" + j.ToString(), mgr);
                                            XmlNode LimNo = nodelists[i].SelectSingleNode("ns2:LimNo" + j.ToString(), mgr);
                                            XmlNode Produ = nodelists[i].SelectSingleNode("ns2:Produ" + j.ToString(), mgr);
                                            XmlNode AppDat = nodelists[i].SelectSingleNode("ns2:AppDat" + j.ToString(), mgr);
                                            XmlNode ExpDat = nodelists[i].SelectSingleNode("ns2:ExpDat" + j.ToString(), mgr);
                                            XmlNode Amt = nodelists[i].SelectSingleNode("ns2:Amt" + j.ToString(), mgr);
                                            XmlNode Curr = nodelists[i].SelectSingleNode("ns2:Curr" + j.ToString(), mgr);
                                            XmlNode BalAmt = nodelists[i].SelectSingleNode("ns2:BalAmt" + j.ToString(), mgr);
                                            XmlNode Sign = nodelists[i].SelectSingleNode("ns2:Sign" + j.ToString(), mgr);
                                            XmlNode DbbTyp = nodelists[i].SelectSingleNode("ns2:DbbTyp" + j.ToString(), mgr);
                                            XmlNode Status = nodelists[i].SelectSingleNode("ns2:Status" + j.ToString(), mgr);
                                            XmlNode StopCd = nodelists[i].SelectSingleNode("ns2:StopCd" + j.ToString(), mgr);
                                            XmlNode HoldFlag = nodelists[i].SelectSingleNode("ns2:HoldFlag" + j.ToString(), mgr);

                                            Dictionary<string, XmlNode> dicnode = new Dictionary<string, XmlNode>();
                                            dicnode.Add("1", Branc);
                                            dicnode.Add("2", LimNo);
                                            dicnode.Add("3", Produ);
                                            dicnode.Add("4", AppDat);
                                            dicnode.Add("5", ExpDat);
                                            dicnode.Add("6", Amt);
                                            dicnode.Add("7", Curr);
                                            dicnode.Add("8", BalAmt);
                                            dicnode.Add("9", Sign);
                                            dicnode.Add("10", DbbTyp);
                                            dicnode.Add("11", Status);
                                            dicnode.Add("12", StopCd);
                                            dicnode.Add("13", HoldFlag);

                                            if (Branc == null && LimNo == null && Produ == null && AppDat == null && ExpDat == null && Amt == null && Curr == null && Sign == null && DbbTyp == null &&
                                                Status == null && StopCd == null && HoldFlag == null)
                                            {
                                                b67072 = false;
                                                break;
                                            }
                                            else if (string.IsNullOrEmpty(Branc.InnerText)
                                                && string.IsNullOrEmpty(LimNo.InnerText)
                                                && string.IsNullOrEmpty(Produ.InnerText)
                                                && string.IsNullOrEmpty(AppDat.InnerText)
                                                && string.IsNullOrEmpty(ExpDat.InnerText)
                                                && string.IsNullOrEmpty(Amt.InnerText)
                                                && string.IsNullOrEmpty(Curr.InnerText)
                                                && string.IsNullOrEmpty(Sign.InnerText)
                                                && string.IsNullOrEmpty(DbbTyp.InnerText)
                                                && string.IsNullOrEmpty(Status.InnerText)
                                                && string.IsNullOrEmpty(StopCd.InnerText)
                                                && string.IsNullOrEmpty(HoldFlag.InnerText))
                                            {
                                                continue;
                                            }
                                            else
                                            {
                                                foreach (XmlNode nodechild in dicnode.Values)
                                                {
                                                    string strColumnName = nodechild.Name.ToString().ToUpper().Replace("NS2:", "");
                                                    string strValue = nodechild.InnerText;
                                                    strColumnName = strColumnName.TrimEnd(new char[] { '1', '2', '3', '4', '5', '6', '7', '8', '9', '0' });
                                                    if (dicValueDetl.ContainsKey(strColumnName))
                                                    {
                                                        DataRow dr1 = dt.NewRow();
                                                        foreach (string key in dicValueDetl.Keys)
                                                        {
                                                            dr1[key] = dicValueDetl[key];
                                                        }
                                                        dt.Rows.Add(dr1);
                                                        dicValueDetl = new Dictionary<string, string>();
                                                    }
                                                    if (!dt.Columns.Contains(strColumnName))
                                                    {
                                                        dt.Columns.Add(strColumnName);
                                                    }
                                                    IEnumerable<HostMsgGrp> m_List = resultlist.Where(m => m.datatype == "9" && m.edata.ToUpper().Equals(strColumnName));
                                                    if (m_List != null && m_List.Count() > 0)
                                                    {
                                                        strValue = ChangeTheFormat(strValue);
                                                    }
                                                    dicValueDetl.Add(strColumnName, strValue);
                                                }
                                            }
                                        }
                                        if (dicValueDetl.Count > 0)
                                        {
                                            DataRow dr = dt.NewRow();
                                            foreach (string key in dicValueDetl.Keys)
                                            {
                                                dr[key] = dicValueDetl[key];
                                            }
                                            dt.Rows.Add(dr);
                                        }

                                        for (int k = 0; k < nodelists[i].ChildNodes.Count; k++)
                                        {
                                            string strColumnName = nodelists[i].ChildNodes[k].Name.ToString().ToUpper().Replace("NS2:", "");
                                            string strValue = nodelists[i].ChildNodes[k].InnerText;

                                            if (strChild.IndexOf("," + strColumnName + ",") >= 0)//明細表
                                            {
                                                continue;
                                            }
                                            else //主表
                                            {
                                                if (!dicValue.ContainsKey(strColumnName))
                                                {
                                                    IEnumerable<HostMsgGrp> m_List = resultlist.Where(m => m.datatype == "9" && m.edata.ToUpper().Equals(strColumnName));
                                                    if (m_List != null && m_List.Count() > 0)
                                                    {
                                                        strValue = ChangeTheFormat(strValue);
                                                    }
                                                    dicValue.Add(strColumnName, strValue);
                                                }

                                            }
                                        }

                                    }
                                }
                                else if (strRspCode == "1204" || strRspCode == "7003" )   //最後一頁
                                {
                                    b67072 = false;
                                }
                                else
                                {
                                    b67072 = false;
                                    bESBFlag = false;
                                }
                            }
                            else
                            {
                                b67072 = false;
                                bESBFlag = false;
                            }
                        }
                        else
                        {
                            result = "003|連接ESB服務器失敗";
                            b67072 = false;
                            bESBFlag = false;
                        }
                    }
                    catch
                    {
                        b67072 = false;    //出現異常跳出while循環防止死循環
                        bESBFlag = false;
                    }
                }
                if (bESBFlag)
                {
                    //循環拼SQL 
                    foreach (string key in dic.Keys)
                    {
                        string strTableName = key;
                        string strLastName = strTableName.Substring(strTableName.Length - 3).ToUpper();
                        if (strLastName == "GRP")   //主表
                        {
                            if (dicValue.Count > 0)
                            {
                                strIdentityKey = hostbiz.GetIdentityKey(strTableName);
                                string strSql = "insert into " + strTableName + " (";

                                //adam
                                strSql += "[SNO],[cCretDT],caseId,";
                                //strSql += "[SNO],[APPLNO],[APPLNOB],[cCretDT],";
                                DataTable dtNew = dic[key];
                                for (int i = 0; i < dtNew.Columns.Count; i++)
                                {
                                    strSql += "[" + dtNew.Columns[i].ColumnName + "],";
                                }
                                strSql = strSql.TrimEnd(',') + ") values(";
                                strSql += "'" + strIdentityKey + "'," + "GETDATE(),'" + CaseID + "',";
                                for (int i = 0; i < dtNew.Columns.Count; i++)
                                {
                                    if (dicValue.ContainsKey(dtNew.Columns[i].ColumnName))
                                    {
                                        strSql += "'" + dicValue[dtNew.Columns[i].ColumnName] + "',";
                                    }
                                    else
                                    {
                                        strSql += "'',";
                                    }
                                }
                                strSql = strSql.TrimEnd(',') + ");";
                                array.Add(strSql);
                            }
                        }
                        else         //從表
                        {
                            DataTable dtNew = dic[key];
                            for (int i = 0; i < dt.Rows.Count; i++)
                            {
                                string strSql = "insert into " + strTableName + " (";
                                strSql += "[SNO],[FKSNO],[CUST_ID],caseId,";
                                for (int j = 0; j < dtNew.Columns.Count; j++)
                                {
                                    strSql += "[" + dtNew.Columns[j].ColumnName + "],";
                                }
                                strSql = strSql.TrimEnd(',') + ") values(";
                                strSql += "NEXT VALUE FOR SEQ" + strTableName + ",'" + strIdentityKey + "','" + ObligorID + "','" + CaseID + "',";
                                for (int j = 0; j < dtNew.Columns.Count; j++)
                                {
                                    string strColumnName = dtNew.Columns[j].ColumnName;
                                    if (dt.Columns.Contains(strColumnName))
                                    {
                                        strSql += "'" + dt.Rows[i][strColumnName].ToString() + "',";
                                    }
                                    else
                                    {
                                        strSql += "'',";
                                    }
                                }
                                strSql = strSql.TrimEnd(',') + ");";
                                array.Add(strSql);
                            }
                        }
                    }
                    bool flagresult = hostbiz.SaveESBData(array);
                }
            }
            //返回值

            if (!string.IsNullOrEmpty(result))
            {
                return result;
            }
            else if (bESBFlag)
            {
                result = "0001|成功";
            }
            else
            {
                if (strErrorCode != "")
                {
                    result = strErrorCode + "|" + strErrorMessage;
                }
                else
                {
                    result = "|檔案未開啟或其它錯誤";
                }
            }
            return result;
        }


        /// 原本
        /// <summary>
        /// adam 拿掉strApplNo
        /// </summary>
        /// <param name="strApplNo"></param>
        /// <param name="strApplNoB"></param>
        /// <returns></returns>

        //public string P4_67072U(string ObligorID)
        //{
        //    string result = "";
        //    string strRspCode = "";
        //    string strXML = "";
        //    int iBegin = 0;   //查詢起始筆數
        //    int iPage = 1;    //查詢起始頁碼
        //    int iCurrCount = 0;   //主機返回資料筆數
        //    int iPageSzie = 20;    //每頁筆數
        //    string strErrorType = "";
        //    string strErrorCode = "";
        //    string strErrorMessage = "";
        //    bool b67072 = true;
        //    bool bESBFlag = true;       //發送上行和接收下行電文是否有誤
        //    string strIdentityKey = "";
        //    DataTable dt = new DataTable();        //從表
        //    ArrayList array = new ArrayList();
        //    Dictionary<string, string> dicValue = new Dictionary<string, string>();   //主表
        //    IList<HostMsgGrp> resultlist = hostbiz.QueryHostMsgDetl("67072", "D");
        //    if (resultlist.Count > 0)
        //    {
        //        string strSno = hostbiz.GetMaxSno("67072").PadRight(20, ' ');
        //        Dictionary<string, DataTable> dic = new Dictionary<string, DataTable>();
        //        foreach (var list in resultlist)
        //        {
        //            bool bExists = false;
        //            string strTableName = list.dest_table;
        //            string strEdata = list.edata.ToUpper();
        //            DataTable dtNew = new DataTable();
        //            if (dic.ContainsKey(strTableName))
        //            {
        //                bExists = true;
        //                dtNew = dic[strTableName];
        //            }
        //            if (!dtNew.Columns.Contains(strEdata))
        //            {
        //                dtNew.Columns.Add(strEdata);
        //            }
        //            if (bExists)
        //            {
        //                dic[strTableName] = dtNew;
        //            }
        //            else
        //            {
        //                dic.Add(strTableName, dtNew);
        //            }
        //        }
        //        try
        //        {
        //            StringBuilder sb = new StringBuilder();
        //            sb.AppendLine(strHerader1);
        //            sb.AppendLine(strHerader2);
        //            sb.AppendLine(strHerader3);
        //            sb.AppendLine("<ns1:StandardType>BSMF</ns1:StandardType>");
        //            sb.AppendLine("<ns1:StandardVersion>01</ns1:StandardVersion>");
        //            sb.AppendLine("<ns1:ServiceName>ctLoanAcctListInq</ns1:ServiceName>");
        //            sb.AppendLine("<ns1:ServiceVersion>01</ns1:ServiceVersion>");
        //            sb.AppendLine("<ns1:SourceID>" + strSourceID + "</ns1:SourceID>");
        //            //sb.AppendLine("<ns1:SourceID>TWCMS</ns1:SourceID>");
        //            sb.AppendLine("<ns1:TransactionID>" + strSno + "</ns1:TransactionID>");
        //            sb.AppendLine("<ns1:RqTimestamp>" + System.DateTime.Now.ToString("yyyy-MM-dd") + "T" + System.DateTime.Now.ToString("HH:mm:ss.ff") + "+08:00" + "</ns1:RqTimestamp>");
        //            sb.AppendLine("</ns1:ServiceHeader>");

        //            sb.AppendLine("<ns1:ServiceBody xmlns:ns1=\"http://ns.chinatrust.com.tw/XSD/CTCB/ESB/Message/EMF/ServiceBody\">");
        //            sb.AppendLine("<ns2:ctLoanAcctListInqRq xmlns:ns2=\"http://ns.chinatrust.com.tw/XSD/CTCB/ESB/Message/BSMF/ctLoanAcctListInqRq/01\">");
        //            sb.AppendLine("<ns2:REQHDR>");
        //            sb.AppendLine("<ns2:TrnNum>" + strSno + "</ns2:TrnNum>");
        //            sb.AppendLine("<ns2:TrnCode>062072</ns2:TrnCode>");
        //            sb.AppendLine("<ns2:UserBranchId>" + strUserBranchId + "</ns2:UserBranchId>");
        //            sb.AppendLine("<ns2:UserId>" + TellerNo + "</ns2:UserId>");
        //            sb.AppendLine("</ns2:REQHDR><ns2:REQBDY>");
        //            sb.AppendLine("<ns2:Action></ns2:Action>");
        //            sb.AppendLine("<ns2:CustIdNo>" + ObligorID + "</ns2:CustIdNo>");
        //            sb.AppendLine("<ns2:CustNo></ns2:CustNo>");
        //            sb.AppendLine("<ns2:TxTr>N</ns2:TxTr>");
        //            sb.AppendLine("<ns2:Option>0</ns2:Option>");

        //            sb.AppendLine("</ns2:REQBDY>");
        //            sb.AppendLine("</ns2:ctLoanAcctListInqRq>");
        //            sb.AppendLine("</ns1:ServiceBody>");
        //            sb.AppendLine("</ns0:ServiceEnvelope>");

        //            strXML = sb.ToString();

        //            string bSendresult = SendESBData(strXML, "67072" + iPage.ToString());

        //            if (bSendresult != "")
        //            {
        //                XmlDocument xmldoc = new XmlDocument();
        //                xmldoc.LoadXml(bSendresult);
        //                if (xmldoc != null)
        //                {
        //                    XmlNamespaceManager mgr = new XmlNamespaceManager(xmldoc.NameTable);
        //                    mgr.AddNamespace("ns0", "http://ns.chinatrust.com.tw/XSD/CTCB/ESB/Message/EMF/ServiceEnvelope");
        //                    mgr.AddNamespace("ns1", "http://ns.chinatrust.com.tw/XSD/CTCB/ESB/Message/EMF/ServiceBody");
        //                    mgr.AddNamespace("ns2", "http://ns.chinatrust.com.tw/XSD/CTCB/ESB/Message/BSMF/ctLoanAcctListInqRs/01");
        //                    XmlNode nodenormal = xmldoc.SelectSingleNode("/ns0:ServiceEnvelope/ns1:ServiceBody/ns2:ctLoanAcctListInqRs/ns2:RESHDR/ns2:RspCode", mgr);
        //                    if (nodenormal != null)
        //                    {
        //                        XmlNode node = xmldoc.SelectSingleNode("/ns0:ServiceEnvelope/ns1:ServiceBody/ns2:ctLoanAcctListInqRs/ns2:RESHDR", mgr);
        //                        if (node != null)
        //                        {
        //                            for (int j = 0; j < node.ChildNodes.Count; j++)
        //                            {
        //                                string strColumnName = node.ChildNodes[j].Name.ToString().ToUpper().Replace("NS2:", "");
        //                                string strValue = node.ChildNodes[j].InnerText;
        //                                if (strColumnName == "RSPCODE")
        //                                {
        //                                    strRspCode = strValue;   //獲取下行電文返回的狀態碼
        //                                }
        //                                if (!dicValue.ContainsKey(strColumnName))
        //                                {
        //                                    dicValue.Add(strColumnName, strValue);
        //                                }
        //                            }
        //                        }
        //                    }
        //                    else
        //                    {
        //                        b67072 = false;
        //                        bESBFlag = false;
        //                        XmlNode nodeerror = xmldoc.SelectSingleNode("/ns0:ServiceEnvelope/ns1:ServiceBody/ns2:ctLoanAcctListInqRs/ns2:RESHDR/ErrorType", mgr);
        //                        if (nodeerror != null)
        //                        {
        //                            strErrorType = nodeerror.InnerText;
        //                        }
        //                        nodeerror = xmldoc.SelectSingleNode("/ns0:ServiceEnvelope/ns1:ServiceBody/ns2:ctLoanAcctListInqRs/ns2:RESHDR/ErrorCode", mgr);
        //                        if (nodeerror != null)
        //                        {
        //                            strErrorCode = nodeerror.InnerText;
        //                        }
        //                        nodeerror = xmldoc.SelectSingleNode("/ns0:ServiceEnvelope/ns1:ServiceBody/ns2:ctLoanAcctListInqRs/ns2:RESHDR/ErrorMessage", mgr);
        //                        if (nodeerror != null)
        //                        {
        //                            strErrorMessage = nodeerror.InnerText;
        //                        }
        //                    }
        //                    //ESB回复正常且主机回复正常
        //                    if (strRspCode == "03")
        //                    {
        //                        XmlNodeList nodelists = xmldoc.SelectNodes("/ns0:ServiceEnvelope/ns1:ServiceBody/ns2:ctLoanAcctListInqRs/ns2:RESBDY/ns2:BDYREC", mgr);
        //                        for (int i = 0; i < nodelists.Count; i++)
        //                        {
        //                            Dictionary<string, string> dicValueDetl = new Dictionary<string, string>();   //從表
        //                            for (int k = 0; k < nodelists[i].ChildNodes.Count; k++)
        //                            {
        //                                string strColumnName = nodelists[i].ChildNodes[k].Name.ToString().ToUpper().Replace("NS2:", "");
        //                                string strValue = nodelists[i].ChildNodes[k].InnerText;
        //                                if (!dt.Columns.Contains(strColumnName))
        //                                {
        //                                    dt.Columns.Add(strColumnName);
        //                                }
        //                                dicValueDetl.Add(strColumnName, strValue);
        //                            }
        //                            DataRow dr = dt.NewRow();
        //                            foreach (string key in dicValueDetl.Keys)
        //                            {
        //                                dr[key] = dicValueDetl[key];
        //                            }
        //                            dt.Rows.Add(dr);
        //                        }
        //                    }
        //                    else
        //                    {
        //                        b67072 = false;
        //                        bESBFlag = false;
        //                    }
        //                }
        //                else
        //                {
        //                    b67072 = false;
        //                    bESBFlag = false;
        //                }
        //            }
        //            else
        //            {
        //                b67072 = false;
        //            }
        //        }
        //        catch
        //        {
        //            b67072 = false;    //出現異常跳出while循環防止死循環
        //        }
        //        if (bESBFlag)
        //        {
        //            //循環拼SQL 
        //            foreach (string key in dic.Keys)
        //            {
        //                string strTableName = key;
        //                string strLastName = strTableName.Substring(strTableName.Length - 3).ToUpper();
        //                if (strLastName == "GRP")   //主表
        //                {
        //                    if (dicValue.Count > 0)
        //                    {
        //                        strIdentityKey = hostbiz.GetIdentityKey(strTableName);
        //                        string strSql = "insert into " + strTableName + " (";
        //                        //adam
        //                        strSql += "[SNO],";
        //                        //strSql += "[SNO],[APPLNO],[APPLNOB],";
        //                        DataTable dtNew = dic[key];
        //                        for (int i = 0; i < dtNew.Columns.Count; i++)
        //                        {
        //                            strSql += "[" + dtNew.Columns[i].ColumnName + "],";
        //                        }
        //                        strSql = strSql.TrimEnd(',') + ") values(";
        //                        strSql += "'" + strIdentityKey + "',";
        //                        for (int i = 0; i < dtNew.Columns.Count; i++)
        //                        {
        //                            if (dicValue.ContainsKey(dtNew.Columns[i].ColumnName))
        //                            {
        //                                strSql += "'" + dicValue[dtNew.Columns[i].ColumnName] + "',";
        //                            }
        //                            else
        //                            {
        //                                strSql += "'',";
        //                            }
        //                        }
        //                        strSql = strSql.TrimEnd(',') + ");";
        //                        array.Add(strSql);
        //                    }
        //                }
        //                else         //從表
        //                {
        //                    DataTable dtNew = dic[key];
        //                    for (int i = 0; i < dt.Rows.Count; i++)
        //                    {
        //                        string strSql = "insert into " + strTableName + " (";
        //                        strSql += "[SNO],[FKSNO],";
        //                        for (int j = 0; j < dtNew.Columns.Count; j++)
        //                        {
        //                            strSql += "[" + dtNew.Columns[j].ColumnName + "],";
        //                        }
        //                        strSql = strSql.TrimEnd(',') + ") values(";
        //                        strSql += "NEXT VALUE FOR SEQ" + strTableName + ",'" + strIdentityKey + "',";
        //                        for (int j = 0; j < dtNew.Columns.Count; j++)
        //                        {
        //                            string strColumnName = dtNew.Columns[j].ColumnName;
        //                            if (dt.Columns.Contains(strColumnName))
        //                            {
        //                                strSql += "'" + dt.Rows[i][strColumnName].ToString() + "',";
        //                            }
        //                            else
        //                            {
        //                                strSql += "'',";
        //                            }
        //                        }
        //                        strSql = strSql.TrimEnd(',') + ");";
        //                        array.Add(strSql);
        //                    }
        //                }
        //            }
        //            bool flagresult = hostbiz.SaveESBData(array);
        //        }
        //    }
        //    //返回值
        //    if (bESBFlag)
        //    {
        //        result = "0001|成功";
        //    }
        //    else
        //    {
        //        if (strErrorCode != "")
        //        {
        //            result = strErrorCode + "|" + strErrorMessage;
        //        }
        //        else
        //        {
        //            result = "|檔案未開啟或其它錯誤";
        //        }
        //    }
        //    return result;
        //}
        /// <summary>
        /// 60491 通過統編查詢客戶的信息以及下屬銀行卡信息
        /// </summary>
        /// <param name="ObligorID"></param>
        /// <returns></returns>
        public string P4_60491U(string ObligorID,string CaseID)
        {
            string result = "";
            string strRspCode = "";
            int iPage = 1;    //查詢起始頁碼
            string strErrorType = "";
            string strErrorCode = "";
            string strErrorMessage = "";
            bool bESBFlag = true;       //發送上行和接收下行電文是否有誤
            bool b60491 = true;
            string strIdentityKey = "";
            bool bMast = false;
            DataTable dt = new DataTable();        //從表
            ArrayList array = new ArrayList();
            Dictionary<string, string> dicValue = new Dictionary<string, string>();   //主表
            IList<HostMsgGrp> resultlist = hostbiz.QueryHostMsgDetl("60491", "D");
            if (resultlist.Count > 0)
            {
                //* 流水號
                //2015-09-01
                //string strSno = hostbiz.GetMaxSno("60491").PadRight(20, ' ');
                //strSno = strSno.Substring(0, strSno.Length - 2);
                string strYear = Convert.ToInt16(DateTime.Now.Year - 1911).ToString("000");
                string strMonth = Convert.ToInt16(DateTime.Now.Month).ToString("00");
                string strDay = Convert.ToInt16(DateTime.Now.Day).ToString("00");
                string strHour = System.DateTime.Now.ToString("HHmmssfff");
                string strSno = "CSFS" + strYear.Substring(1,2) + strMonth + strDay + strHour;

                #region 根據DB設定初始化要收取的DataTable
                Dictionary<string, DataTable> dic = new Dictionary<string, DataTable>();
                foreach (var list in resultlist)
                {
                    bool bExists = false;
                    string strTableName = list.dest_table;  //* 目標 table 名
                    string strEdata = list.edata.ToUpper(); //* 目標 table 英文欄位名
                    DataTable dtNew = new DataTable();
                    if (dic.ContainsKey(strTableName))
                    {
                        bExists = true;
                        dtNew = dic[strTableName];          //* 回圈,如果dic裏有這個table 就取出這個table的DataTable
                    }
                    if (!dtNew.Columns.Contains(strEdata))
                    {
                        dtNew.Columns.Add(strEdata);        //* 目標DataTable裏沒有這個欄位就新增這個欄位
                    }
                    if (bExists)
                    {
                        dic[strTableName] = dtNew;          //* 更新或新增DataTable
                    }
                    else
                    {
                        dic.Add(strTableName, dtNew);
                    }
                }
                #endregion

                #region 生成上行電文 (使用了strSno,TellerNo,ObligorID)
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(strHerader1);
                sb.AppendLine(strHerader2);
                sb.AppendLine(strHerader3);
                sb.AppendLine("<ns1:StandardType>BSMF</ns1:StandardType>");
                sb.AppendLine("<ns1:StandardVersion>01</ns1:StandardVersion>");
                sb.AppendLine("<ns1:ServiceName>csCustProfileInq</ns1:ServiceName>");
                sb.AppendLine("<ns1:ServiceVersion>01</ns1:ServiceVersion>");
                //adam
                sb.AppendLine("<ns1:SourceID>" + ReplaceContent_Up(strSourceID) + "</ns1:SourceID>");
                sb.AppendLine("<ns1:TransactionID>" + ReplaceContent_Up(strSno) + "</ns1:TransactionID>");
                sb.AppendLine("<ns1:RqTimestamp>" + System.DateTime.Now.ToString("yyyy-MM-dd") + "T" + System.DateTime.Now.ToString("HH:mm:ss.ff") + "+08:00" + "</ns1:RqTimestamp>");
                sb.AppendLine("</ns1:ServiceHeader>");

                sb.AppendLine("<ns1:ServiceBody xmlns:ns1=\"http://ns.chinatrust.com.tw/XSD/CTCB/ESB/Message/EMF/ServiceBody\">");
                sb.AppendLine("<ns2:csCustProfileInqRq xmlns:ns2=\"http://ns.chinatrust.com.tw/XSD/CTCB/ESB/Message/BSMF/csCustProfileInqRq/01\">");
                sb.AppendLine("<ns2:REQHDR>");
                sb.AppendLine("<ns2:TrnNum>" + ReplaceContent_Up(strSno) + "</ns2:TrnNum>");       //* 流水號
                sb.AppendLine("<ns2:TrnCode>060490</ns2:TrnCode>");
                sb.AppendLine("<ns2:CtryCode></ns2:CtryCode>");
                sb.AppendLine("<ns2:UserBranchId>" + ReplaceContent_Up(BranchID) + "</ns2:UserBranchId>");
                sb.AppendLine("<ns2:UserId>" + ReplaceContent_Up(TellerNo) + "</ns2:UserId>");     //* 銀電文登入TellerNo ,寫死了99605?
                sb.AppendLine("</ns2:REQHDR><ns2:REQBDY>");
                sb.AppendLine("<ns2:CustType></ns2:CustType>");
                sb.AppendLine("<ns2:EnquiryOption>S</ns2:EnquiryOption>");
                sb.AppendLine("<ns2:TellerId></ns2:TellerId>");
                sb.AppendLine("<ns2:Currency>0</ns2:Currency>");
                sb.AppendLine("<ns2:ProductOption>ALL</ns2:ProductOption>");
                sb.AppendLine("<ns2:ProfileType></ns2:ProfileType>");
                sb.AppendLine("<ns2:CustomerNo>0</ns2:CustomerNo>");
                sb.AppendLine("<ns2:ClosedDate>18122014</ns2:ClosedDate>");
                sb.AppendLine("<ns2:MacValue></ns2:MacValue>");
                sb.AppendLine("<ns2:AccountStatus>1</ns2:AccountStatus>");
                sb.AppendLine("<ns2:BranchId></ns2:BranchId>");
                sb.AppendLine("<ns2:CustId>" + ReplaceContent_Up(ObligorID) + "</ns2:CustId>");    //* 義務人統編
                sb.AppendLine("</ns2:REQBDY>");
                sb.AppendLine("</ns2:csCustProfileInqRq>");
                sb.AppendLine("</ns1:ServiceBody>");
                sb.AppendLine("</ns0:ServiceEnvelope>");

                string strUpXML = sb.ToString();

                #endregion

                #region 回圈解析將電文的主表信息放入到dicValue,從表信息放入到dicValueDetl
                //* 回圈, b60491 = true 還有下一頁, false,沒有下一頁了
                while (b60491)
                {
                    try
                    {
                        //* 發送上行電文到ESB,取得返回值
                        string bSendresult = SendESBData(strUpXML, "60491");

                        #region 接收到的XML作為下次發送的上行電文
                        sb = new StringBuilder();
                        sb.AppendLine(strHerader1);
                        sb.AppendLine(strHerader2);
                        sb.AppendLine(strHerader3);
                        sb.AppendLine("<ns1:StandardType>BSMF</ns1:StandardType>");
                        sb.AppendLine("<ns1:StandardVersion>01</ns1:StandardVersion>");
                        sb.AppendLine("<ns1:ServiceName>csCustProfileInq</ns1:ServiceName>");
                        sb.AppendLine("<ns1:ServiceVersion>01</ns1:ServiceVersion>");
                        sb.AppendLine("<ns1:SourceID>" + strSourceID + "</ns1:SourceID>");
                        //sb.AppendLine("<ns1:SourceID>CSFS</ns1:SourceID>");
                        sb.AppendLine("<ns1:TransactionID>" + strSno + "</ns1:TransactionID>");
                        sb.AppendLine("<ns1:RqTimestamp>" + System.DateTime.Now.ToString("yyyy-MM-dd") + "T" + System.DateTime.Now.ToString("HH:mm:ss.ff") + "+08:00" + "</ns1:RqTimestamp>");
                        sb.AppendLine("</ns1:ServiceHeader>");

                        sb.AppendLine("<ns1:ServiceBody xmlns:ns1=\"http://ns.chinatrust.com.tw/XSD/CTCB/ESB/Message/EMF/ServiceBody\">");
                        sb.AppendLine("<ns2:csCustProfileInqRq xmlns:ns2=\"http://ns.chinatrust.com.tw/XSD/CTCB/ESB/Message/BSMF/csCustProfileInqRq/01\">");
                        sb.AppendLine("<ns2:REQHDR>");
                        sb.AppendLine("<ns2:TrnNum>" + strSno + "</ns2:TrnNum>");   //* 流水號
                        sb.AppendLine("<ns2:TrnCode>060491</ns2:TrnCode>");
                        sb.AppendLine("<ns2:CtryCode></ns2:CtryCode>");
                        sb.AppendLine("<ns2:UserBranchId>" + BranchID + "</ns2:UserBranchId>");
                        sb.AppendLine("<ns2:UserId>" + TellerNo + "</ns2:UserId>"); //* 系統編號.寫死了
                        sb.AppendLine("</ns2:REQHDR><ns2:REQBDY>");

                        if (!string.IsNullOrEmpty(bSendresult))
                        {
                            XmlDocument xdoc = new XmlDocument();
                            xdoc.LoadXml(bSendresult);
                            XmlNamespaceManager mgr = new XmlNamespaceManager(xdoc.NameTable);
                            mgr.AddNamespace("ns0", "http://ns.chinatrust.com.tw/XSD/CTCB/ESB/Message/EMF/ServiceEnvelope");
                            mgr.AddNamespace("ns1", "http://ns.chinatrust.com.tw/XSD/CTCB/ESB/Message/EMF/ServiceBody");
                            mgr.AddNamespace("ns2", "http://ns.chinatrust.com.tw/XSD/CTCB/ESB/Message/BSMF/csCustProfileInqRs/01");
                            XmlNode docNode = xdoc.SelectSingleNode("/ns0:ServiceEnvelope/ns1:ServiceBody/ns2:csCustProfileInqRs/ns2:RESBDY/ns2:BDYREC", mgr);
                            if (docNode != null)
                            {
                                foreach (XmlNode item in docNode.ChildNodes)
                                {
                                    string strNodeName = "<" + item.Name + ">" + item.InnerText + "</" + item.Name + ">";
                                    sb.AppendLine(strNodeName);
                                }
                            }
                        }

                        sb.AppendLine("</ns2:REQBDY>");
                        sb.AppendLine("</ns2:csCustProfileInqRq>");
                        sb.AppendLine("</ns1:ServiceBody>");
                        sb.AppendLine("</ns0:ServiceEnvelope>");

                        strUpXML = sb.ToString();   //接收到的XML作為下次發送的上行電文
                        #endregion

                        #region 測試用判斷
                        if (IsnotTest == "1")
                        {
                            bSendresult = "1";
                        }
                        #endregion

                        if (bSendresult != "")
                        {
                            XmlDocument xmldoc = new XmlDocument();

                            #region 解析下行的XML信息
                            if (IsnotTest != "1")
                            {
                                //* 如果不是調試模式.則解析下行電文.
                                xmldoc.LoadXml(bSendresult);
                            }
                            else
                            {
                                //* 調試模式讀取本地xml
                                string path = new DirectoryInfo("~/").Parent.FullName + @"\rtnTemplate\060490+067158+090006_060491+067159+090006\";
                                if (ReadCS == 1)
                                {
                                    xmldoc.Load(path + "client_rs(060491).xml");
                                    //xmldoc.Load(@"D:\Source\Phase 1\銀主機電文\060490+067158+090006_060491+067159+090006\client_rs(060491).xml");
                                }
                                else if (ReadCS == 2)
                                {
                                    xmldoc.Load(path + "client_rs(60491翻頁下行).xml");
                                    //xmldoc.Load(@"D:\Source\Phase 1\銀主機電文\060490+067158+090006_060491+067159+090006\client_rs(60491翻頁下行).xml");
                                }
                            }
                            #endregion

                            if (xmldoc != null)
                            {
                                #region 主表處理
                                XmlNamespaceManager mgr = new XmlNamespaceManager(xmldoc.NameTable);
                                mgr.AddNamespace("ns0", "http://ns.chinatrust.com.tw/XSD/CTCB/ESB/Message/EMF/ServiceEnvelope");
                                mgr.AddNamespace("ns1", "http://ns.chinatrust.com.tw/XSD/CTCB/ESB/Message/EMF/ServiceBody");
                                mgr.AddNamespace("ns2", "http://ns.chinatrust.com.tw/XSD/CTCB/ESB/Message/BSMF/csCustProfileInqRs/01");
                                XmlNode nodenormal = xmldoc.SelectSingleNode("/ns0:ServiceEnvelope/ns1:ServiceBody/ns2:csCustProfileInqRs/ns2:RESHDR/ns2:RspCode", mgr);
                                if (nodenormal != null)
                                {
                                    XmlNode node = xmldoc.SelectSingleNode("/ns0:ServiceEnvelope/ns1:ServiceBody/ns2:csCustProfileInqRs/ns2:RESHDR", mgr);
                                    if (node != null)
                                    {
                                        for (int j = 0; j < node.ChildNodes.Count; j++)
                                        {
                                            string strColumnName = node.ChildNodes[j].Name.ToString().ToUpper().Replace("NS2:", "");
                                            string strValue = node.ChildNodes[j].InnerText;
                                            if (strColumnName == "RSPCODE")
                                            {
                                                strRspCode = strValue;   //獲取下行電文返回的狀態碼
                                            }
                                            if (!dicValue.ContainsKey(strColumnName))
                                            {
                                                IEnumerable<HostMsgGrp> m_List = resultlist.Where(m => m.datatype == "9" && m.edata.ToUpper().Equals(strColumnName));
                                                if (m_List != null && m_List.Count() > 0)
                                                {
                                                    strValue = ChangeTheFormat(strValue);
                                                }
                                                dicValue.Add(strColumnName, strValue);
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    //* 出錯
                                    bESBFlag = false;
                                    XmlNode nodeerror = xmldoc.SelectSingleNode("/ns0:ServiceEnvelope/ns1:ServiceBody/ns2:csCustProfileInqRs/ns2:RESHDR/ErrorType", mgr);
                                    if (nodeerror != null)
                                    {
                                        strErrorType = nodeerror.InnerText;
                                    }
                                    nodeerror = xmldoc.SelectSingleNode("/ns0:ServiceEnvelope/ns1:ServiceBody/ns2:csCustProfileInqRs/ns2:RESHDR/ErrorCode", mgr);
                                    if (nodeerror != null)
                                    {
                                        strErrorCode = nodeerror.InnerText;
                                    }
                                    nodeerror = xmldoc.SelectSingleNode("/ns0:ServiceEnvelope/ns1:ServiceBody/ns2:csCustProfileInqRs/ns2:RESHDR/ErrorMessage", mgr);
                                    if (nodeerror != null)
                                    {
                                        strErrorMessage = nodeerror.InnerText;
                                    }
                                }
                                #endregion
                                
                                //ESB回复正常且主机回复正常
                                if (strRspCode == "03" || strRspCode == "70")
                                {
                                    #region 從表處理
                                    XmlNodeList nodelists = xmldoc.SelectNodes("/ns0:ServiceEnvelope/ns1:ServiceBody/ns2:csCustProfileInqRs/ns2:RESBDY/ns2:BDYREC", mgr);
                                    string strChild = ",ACCOUNT1,BRANCH1,STSDESC1,PRODCODE1,PRODDESC1,LINK1,CCY1,BAL1,SYSTEM1,SEGMENTCODE1,ACCOUNT2,BRANCH2,STSDESC2,PRODCODE2,PRODDESC2,LINK2,CCY2,BAL2,SYSTEM2,SEGMENTCODE2,ACCOUNT3,BRANCH3,STSDESC3,PRODCODE3,PRODDESC3,LINK3,CCY3,BAL3,SYSTEM3,SEGMENTCODE3,ACCOUNT4,BRANCH4,STSDESC4,PRODCODE4,PRODDESC4,LINK4,CCY4,BAL4,SYSTEM4,SEGMENTCODE4,ACCOUNT5,BRANCH5,STSDESC5,PRODCODE5,PRODDESC5,LINK5,CCY5,BAL5,SYSTEM5,SEGMENTCODE5,ACCOUNT6,BRANCH6,STSDESC6,PRODCODE6,PRODDESC6,LINK6,CCY6,BAL6,SYSTEM6,SEGMENTCODE6,";
                                    for (int i = 0; i < nodelists.Count; i++)
                                    {
                                        for (int j = 1; j <= 6; j++)
                                        {
                                            #region 獲取從表節點
                                            XmlNode Account = nodelists[i].SelectSingleNode("ns2:Account" + j.ToString(), mgr);
                                            if (string.IsNullOrEmpty(Account.InnerText))
                                                Account.InnerText = "";
                                            Account.InnerText = ReplaceContent_Down(Account.InnerText);

                                            XmlNode Branch = nodelists[i].SelectSingleNode("ns2:Branch" + j.ToString(), mgr);
                                            if (string.IsNullOrEmpty(Branch.InnerText))
                                                Branch.InnerText = "";
                                            Branch.InnerText = ReplaceContent_Down(Branch.InnerText);

                                            XmlNode StsDesc = nodelists[i].SelectSingleNode("ns2:StsDesc" + j.ToString(), mgr);
                                            if (string.IsNullOrEmpty(StsDesc.InnerText))
                                                StsDesc.InnerText = "";
                                            StsDesc.InnerText = ReplaceContent_Down(StsDesc.InnerText);

                                            XmlNode ProdCode = nodelists[i].SelectSingleNode("ns2:ProdCode" + j.ToString(), mgr);
                                            if (string.IsNullOrEmpty(ProdCode.InnerText))
                                                ProdCode.InnerText = "";
                                            ProdCode.InnerText = ReplaceContent_Down(ProdCode.InnerText);

                                            XmlNode ProdDesc = nodelists[i].SelectSingleNode("ns2:ProdDesc" + j.ToString(), mgr);
                                            if (string.IsNullOrEmpty(ProdDesc.InnerText))
                                                ProdDesc.InnerText = "";
                                            ProdDesc.InnerText = ReplaceContent_Down(ProdDesc.InnerText);

                                            XmlNode Link = nodelists[i].SelectSingleNode("ns2:Link" + j.ToString(), mgr);
                                            if (string.IsNullOrEmpty(Link.InnerText))
                                                Link.InnerText = "";
                                            Link.InnerText = ReplaceContent_Down(Link.InnerText);

                                            XmlNode Ccy = nodelists[i].SelectSingleNode("ns2:Ccy" + j.ToString(), mgr);
                                            if (string.IsNullOrEmpty(Ccy.InnerText))
                                                Ccy.InnerText = "";
                                            Ccy.InnerText = ReplaceContent_Down(Ccy.InnerText);

                                            XmlNode Bal = nodelists[i].SelectSingleNode("ns2:Bal" + j.ToString(), mgr);
                                            if (string.IsNullOrEmpty(Bal.InnerText))
                                                Bal.InnerText = "";
                                            Bal.InnerText = ReplaceContent_Down(Bal.InnerText);

                                            XmlNode SystemNode = nodelists[i].SelectSingleNode("ns2:System" + j.ToString(), mgr);
                                            if (string.IsNullOrEmpty(SystemNode.InnerText))
                                                SystemNode.InnerText = "";
                                            SystemNode.InnerText = ReplaceContent_Down(SystemNode.InnerText);

                                            XmlNode SegmentCode = nodelists[i].SelectSingleNode("ns2:SegmentCode" + j.ToString(), mgr);
                                            if (string.IsNullOrEmpty(SegmentCode.InnerText))
                                                SegmentCode.InnerText = "";
                                            SegmentCode.InnerText = ReplaceContent_Down(SegmentCode.InnerText);

                                            Dictionary<string, XmlNode> dicnode = new Dictionary<string, XmlNode>();
                                            dicnode.Add("1", Account);
                                            dicnode.Add("2", Branch);
                                            dicnode.Add("3", StsDesc);
                                            dicnode.Add("4", ProdCode);
                                            dicnode.Add("5", ProdDesc);
                                            dicnode.Add("6", Link);
                                            dicnode.Add("7", Ccy);
                                            dicnode.Add("8", Bal);
                                            dicnode.Add("9", SystemNode);
                                            dicnode.Add("10", SegmentCode);
                                            #endregion

                                            #region 測試用方法
                                            if (ReadCS == 2 && IsnotTest == "1")
                                            {
                                                Account = null;
                                                Branch = null;
                                                StsDesc = null;
                                                ProdCode = null;
                                                ProdDesc = null;
                                                Link = null;
                                                Ccy = null;
                                                Bal = null;
                                                SystemNode = null;
                                                SegmentCode = null;
                                            }
                                            #endregion

                                            if (Account == null && Branch == null && StsDesc == null && ProdCode == null && ProdDesc == null && Link == null
                                                  && Ccy == null && Bal == null && SystemNode == null && SegmentCode == null)
                                            {
                                                //如果是最後一頁
                                                b60491 = false;//結束循環
                                                break;
                                            }
                                            else
                                            {
                                                if (string.IsNullOrEmpty(Account.InnerText)
                                                    && string.IsNullOrEmpty(Branch.InnerText)
                                                    && string.IsNullOrEmpty(StsDesc.InnerText)
                                                    && string.IsNullOrEmpty(ProdCode.InnerText)
                                                    && string.IsNullOrEmpty(ProdDesc.InnerText)
                                                    && string.IsNullOrEmpty(Link.InnerText)
                                                    && string.IsNullOrEmpty(Ccy.InnerText)
                                                    && string.IsNullOrEmpty(Bal.InnerText)
                                                    && string.IsNullOrEmpty(SystemNode.InnerText)
                                                    && string.IsNullOrEmpty(SegmentCode.InnerText))
                                                {
                                                    b60491 = false;
                                                    break;
                                                }
                                                else if (Account.InnerText.Equals("00000000000000000"))
                                                {
                                                    b60491 = false;//結束循環
                                                    break;
                                                }
                                                else
                                                {
                                                    Dictionary<string, string> dicValueDetl = new Dictionary<string, string>();   //從表
                                                    #region 從表數據
                                                    foreach (XmlNode nodechild in dicnode.Values)
                                                    {
                                                        string strColumnName = nodechild.Name.ToString().ToUpper().Replace("NS2:", "");
                                                        string strValue = nodechild.InnerText;
                                                        if (strColumnName.StartsWith("ACCOUNT"))
                                                        {
                                                            if (!dt.Columns.Contains("ACCOUNT"))
                                                            {
                                                                dt.Columns.Add("ACCOUNT");
                                                            }
                                                            IEnumerable<HostMsgGrp> m_List = resultlist.Where(m => m.datatype == "9" && m.edata.ToUpper().Equals("ACCOUNT"));
                                                            if (m_List != null && m_List.Count() > 0)
                                                            {
                                                                strValue = ChangeTheFormat(strValue);
                                                            }
                                                            dicValueDetl.Add("ACCOUNT", strValue);
                                                        }
                                                        else if (strColumnName.StartsWith("BRANCH"))
                                                        {
                                                            if (!dt.Columns.Contains("BRANCH"))
                                                            {
                                                                dt.Columns.Add("BRANCH");
                                                            }
                                                            IEnumerable<HostMsgGrp> m_List = resultlist.Where(m => m.datatype == "9" && m.edata.ToUpper().Equals("BRANCH"));
                                                            if (m_List != null && m_List.Count() > 0)
                                                            {
                                                                strValue = ChangeTheFormat(strValue);
                                                            }
                                                            dicValueDetl.Add("BRANCH", strValue);
                                                        }
                                                        else if (strColumnName.StartsWith("STSDESC"))
                                                        {
                                                            if (!dt.Columns.Contains("STSDESC"))
                                                            {
                                                                dt.Columns.Add("STSDESC");
                                                            }
                                                            IEnumerable<HostMsgGrp> m_List = resultlist.Where(m => m.datatype == "9" && m.edata.ToUpper().Equals("STSDESC"));
                                                            if (m_List != null && m_List.Count() > 0)
                                                            {
                                                                strValue = ChangeTheFormat(strValue);
                                                            }
                                                            dicValueDetl.Add("STSDESC", strValue);
                                                        }
                                                        else if (strColumnName.StartsWith("PRODCODE"))
                                                        {
                                                            if (!dt.Columns.Contains("PRODCODE"))
                                                            {
                                                                dt.Columns.Add("PRODCODE");
                                                            }
                                                            IEnumerable<HostMsgGrp> m_List = resultlist.Where(m => m.datatype == "9" && m.edata.ToUpper().Equals("PRODCODE"));
                                                            if (m_List != null && m_List.Count() > 0)
                                                            {
                                                                strValue = ChangeTheFormat(strValue);
                                                            }
                                                            dicValueDetl.Add("PRODCODE", strValue);
                                                        }
                                                        else if (strColumnName.StartsWith("PRODDESC"))
                                                        {
                                                            if (!dt.Columns.Contains("PRODDESC"))
                                                            {
                                                                dt.Columns.Add("PRODDESC");
                                                            }
                                                            IEnumerable<HostMsgGrp> m_List = resultlist.Where(m => m.datatype == "9" && m.edata.ToUpper().Equals("PRODDESC"));
                                                            if (m_List != null && m_List.Count() > 0)
                                                            {
                                                                strValue = ChangeTheFormat(strValue);
                                                            }
                                                            dicValueDetl.Add("PRODDESC", strValue);
                                                        }
                                                        else if (strColumnName.StartsWith("LINK"))
                                                        {
                                                            if (!dt.Columns.Contains("LINK"))
                                                            {
                                                                dt.Columns.Add("LINK");
                                                            }
                                                            IEnumerable<HostMsgGrp> m_List = resultlist.Where(m => m.datatype == "9" && m.edata.ToUpper().Equals("LINK"));
                                                            if (m_List != null && m_List.Count() > 0)
                                                            {
                                                                strValue = ChangeTheFormat(strValue);
                                                            }
                                                            dicValueDetl.Add("LINK", strValue);
                                                        }
                                                        else if (strColumnName.StartsWith("CCY"))
                                                        {
                                                            if (!dt.Columns.Contains("CCY"))
                                                            {
                                                                dt.Columns.Add("CCY");
                                                            }
                                                            IEnumerable<HostMsgGrp> m_List = resultlist.Where(m => m.datatype == "9" && m.edata.ToUpper().Equals("CCY"));
                                                            if (m_List != null && m_List.Count() > 0)
                                                            {
                                                                strValue = ChangeTheFormat(strValue);
                                                            }
                                                            dicValueDetl.Add("CCY", strValue);
                                                        }
                                                        else if (strColumnName.StartsWith("BAL"))
                                                        {
                                                            if (!dt.Columns.Contains("BAL"))
                                                            {
                                                                dt.Columns.Add("BAL");
                                                            }
                                                            if (strValue.Length >= 16)
                                                            {
                                                                string sym = strValue.Substring(strValue.Length - 1, 1);
                                                                strValue = strValue.Substring(0, 14) + "." + strValue.Substring(14, 2) + sym;
                                                            }
                                                            IEnumerable<HostMsgGrp> m_List = resultlist.Where(m => m.datatype == "9" && m.edata.ToUpper().Equals("BAL"));
                                                            if (m_List != null && m_List.Count() > 0)
                                                            {
                                                                strValue = ChangeTheFormat(strValue);
                                                            }
                                                            dicValueDetl.Add("BAL", strValue);
                                                        }
                                                        else if (strColumnName.StartsWith("SYSTEM"))
                                                        {
                                                            if (!dt.Columns.Contains("SYSTEM"))
                                                            {
                                                                dt.Columns.Add("SYSTEM");
                                                            }
                                                            IEnumerable<HostMsgGrp> m_List = resultlist.Where(m => m.datatype == "9" && m.edata.ToUpper().Equals("SYSTEM"));
                                                            if (m_List != null && m_List.Count() > 0)
                                                            {
                                                                strValue = ChangeTheFormat(strValue);
                                                            }
                                                            dicValueDetl.Add("SYSTEM", strValue);
                                                        }
                                                        else if (strColumnName.StartsWith("SEGMENTCODE"))
                                                        {
                                                            if (!dt.Columns.Contains("SEGMENTCODE"))
                                                            {
                                                                dt.Columns.Add("SEGMENTCODE");
                                                            }
                                                            IEnumerable<HostMsgGrp> m_List = resultlist.Where(m => m.datatype == "9" && m.edata.ToUpper().Equals("SEGMENTCODE"));
                                                            if (m_List != null && m_List.Count() > 0)
                                                            {
                                                                strValue = ChangeTheFormat(strValue);
                                                            }
                                                            dicValueDetl.Add("SEGMENTCODE", strValue);
                                                            DataRow dr = dt.NewRow();
                                                            foreach (string key in dicValueDetl.Keys)
                                                            {
                                                                dr[key] = dicValueDetl[key];
                                                            }
                                                            dt.Rows.Add(dr);
                                                            dicValueDetl.Clear();
                                                        }
                                                    }
                                                    #endregion
                                                }
                                            }
                                        }
                                        for (int k = 0; k < nodelists[i].ChildNodes.Count; k++)
                                        {
                                            string strColumnName = nodelists[i].ChildNodes[k].Name.ToString().ToUpper().Replace("NS2:", "");
                                            string strValue = nodelists[i].ChildNodes[k].InnerText;

                                            if (strChild.IndexOf("," + strColumnName + ",") >= 0)//明細表
                                            {
                                                continue;
                                            }
                                            else //主表
                                            {
                                                if (!bMast)//未處理
                                                {
                                                    if (!dicValue.ContainsKey(strColumnName))
                                                    {
                                                        if (strColumnName.Equals("ASSETVAR") && strValue.Length >= 4)
                                                        {
                                                            string sym = strValue.Substring(strValue.Length - 1, 1);
                                                            strValue = strValue.Substring(0, 2) + "." + strValue.Substring(2, 2) + sym;
                                                        }
                                                        IEnumerable<HostMsgGrp> m_List = resultlist.Where(m => m.datatype == "9" && m.edata.ToUpper().Equals(strColumnName));
                                                        if (m_List != null && m_List.Count() > 0)
                                                        {
                                                            strValue = ChangeTheFormat(strValue);
                                                        }
                                                        dicValue.Add(strColumnName, strValue);
                                                    }
                                                }
                                                else
                                                {
                                                    continue;
                                                }
                                            }
                                        }
                                        bMast = true;
                                    }
                                    #endregion
                                }
                                else if (strRspCode == "80" || strRspCode == "1204" || strRspCode == "0188" || strRspCode == "7003" || strRspCode == "7005")   //翻到最後一頁的標識
                                {
                                    b60491 = false;
                                }
                                else
                                {
                                    //* 其他狀態出錯
                                    result = "傳回錯誤:" + strRspCode;
                                    b60491 = false;
                                    bESBFlag = false;
                                    break;
                                }
                            }
                        }
                        else
                        {
                            //* 連線出錯
                            result = "003|連接ESB服務器失敗";
                            b60491 = false;
                            bESBFlag = false;
                        }

                        #region 測試用方法
                        if (IsnotTest == "1")
                        {
                            ReadCS++;
                        }
                        #endregion
                    }
                    catch
                    {
                        //* 異常出錯
                        b60491 = false;    //出現異常跳出while循環防止死循環
                        bESBFlag = false;
                    }
                }
                #endregion

                #region 測試用方法
                if (IsnotTest == "1")
                {
                    ReadCS = 1;
                }
                #endregion

                #region 如果call esb的過程中沒有出錯,產生SQL儲存DB
                if (bESBFlag)
                {
                    //循環拼SQL 
                    foreach (string key in dic.Keys)
                    {
                        string strTableName = key;
                        string strLastName = strTableName.Substring(strTableName.Length - 3).ToUpper();
                        if (strLastName == "GRP")   //主表
                        {
                            #region 主表
                            if (dicValue.Count > 0)
                            {
                                strIdentityKey = hostbiz.GetIdentityKey(strTableName);
                                string strSql = "insert into " + strTableName + " (";
                                strSql += "[SNO],[cCretDT],caseId,";
                                DataTable dtNew = dic[key];
                                for (int i = 0; i < dtNew.Columns.Count; i++)
                                {
                                    strSql += "[" + dtNew.Columns[i].ColumnName + "],";
                                }
                                strSql = strSql.TrimEnd(',') + ") values(";
                                strSql += "'" + strIdentityKey + "',GETDATE(),'"+CaseID+"',";
                                for (int i = 0; i < dtNew.Columns.Count; i++)
                                {
                                    if (dicValue.ContainsKey(dtNew.Columns[i].ColumnName))
                                    {
                                        strSql += "'" + dicValue[dtNew.Columns[i].ColumnName] + "',";
                                    }
                                    else
                                    {
                                        strSql += "'',";
                                    }
                                }
                                strSql = strSql.TrimEnd(',') + ");";
                                array.Add(strSql);
                            }
                            #endregion
                        }
                        else         //從表
                        {
                            #region 從表
                            DataTable dtNew = dic[key];
                            for (int i = 0; i < dt.Rows.Count; i++)
                            {
                                string strSql = "insert into " + strTableName + " (";
                                strSql += "[SNO],[FKSNO],[CUST_ID],caseId,";
                                for (int j = 0; j < dtNew.Columns.Count; j++)
                                {
                                    strSql += "[" + dtNew.Columns[j].ColumnName + "],";
                                }
                                strSql = strSql.TrimEnd(',') + ") values(";
                                strSql += "NEXT VALUE FOR SEQ" + strTableName + ",'" + strIdentityKey + "','" + ObligorID + "','" + CaseID + "',";
                                for (int j = 0; j < dtNew.Columns.Count; j++)
                                {
                                    string strColumnName = dtNew.Columns[j].ColumnName;
                                    if (dt.Columns.Contains(strColumnName))
                                    {
                                        strSql += "'" + dt.Rows[i][strColumnName].ToString() + "',";
                                    }
                                    else
                                    {
                                        strSql += "'',";
                                    }
                                }
                                strSql = strSql.TrimEnd(',') + ");";
                                array.Add(strSql);
                            }
                            #endregion
                        }
                    }
                    //* 實際將sql 元組儲存
                    bool flagresult = hostbiz.SaveESBData(array);
                }
                #endregion
            }

            //* 返回值不為空代表之前已經出錯.直接返回
            if (!string.IsNullOrEmpty(result))
            {
                return result;
            }
            else if (bESBFlag)
            {
                //* 執行成功
                WriteLog("\r\n-----------------------------------------義務人統編為：" + ObligorID + "的電文讀取成功！---------------------------------------\r\n");
                result = "0001|成功";
            }
            else
            {
                //* 執行失敗
                if (strErrorCode != "")
                {
                    result = strErrorCode + "|" + strErrorMessage;
                }
                else
                {
                    WriteLog("\r\n-----------------------------------------9999|檔案未開啟或其它錯誤---------------------------------------\r\n");
                    result = "9999|檔案未開啟或其它錯誤";
                }
            }
            return result;
        }

        public string P4_33401U(string accountNo,string CustomID,string CaseID)
        {
            string result = "";
            bool bESBFlag = false;
            bool breakEach = true;
            string strXML = "";
            //DataTable AccountList = hostbiz.GetAccount("33401", strApplNo, strApplNoB);
            //foreach (DataRow Account in AccountList.Rows)
            //{
            //    if (!breakEach)
            //    {
            //        bESBFlag = false;
            //        break;
            //    }
              //2015-09-01
               // string strSno = hostbiz.GetMaxSno("33401").PadRight(19, ' ');
               // strSno = strSno.Substring(0, strSno.Length - 2);
            string strYear = Convert.ToInt16(DateTime.Now.Year - 1911).ToString("000");
            string strMonth = Convert.ToInt16(DateTime.Now.Month).ToString("00");
            string strDay = Convert.ToInt16(DateTime.Now.Day).ToString("00");
            string strHour = System.DateTime.Now.ToString("HHmmssfff");
            string strSno = "CSFS" + strYear.Substring(1,2) + strMonth + strDay + strHour;
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(strHerader1);
                sb.AppendLine(strHerader2);
                sb.AppendLine(strHerader3);
                sb.AppendLine("<ns1:StandardType>BSMF</ns1:StandardType>");
                sb.AppendLine("<ns1:StandardVersion>01</ns1:StandardVersion>");
                sb.AppendLine("<ns1:ServiceName>cmAcctProfileInq</ns1:ServiceName>");
                sb.AppendLine("<ns1:ServiceVersion>01</ns1:ServiceVersion>");
                sb.AppendLine("<ns1:SourceID>" + strSourceID + "</ns1:SourceID>");
                sb.AppendLine("<ns1:TransactionID>" + strSno + "</ns1:TransactionID>");
                sb.AppendLine("<ns1:RqTimestamp>" + System.DateTime.Now.ToString("yyyy-MM-dd") + "T" + System.DateTime.Now.ToString("HH:mm:ss.ff") + "+08:00" + "</ns1:RqTimestamp>");
                sb.AppendLine("</ns1:ServiceHeader>");
                sb.AppendLine("<ns1:ServiceBody xmlns:ns1=\"http://ns.chinatrust.com.tw/XSD/CTCB/ESB/Message/EMF/ServiceBody\">");
                sb.AppendLine("<ns2:cmAcctProfileInqRq xmlns:ns2=\"http://ns.chinatrust.com.tw/XSD/CTCB/ESB/Message/BSMF/cmAcctProfileInqRq/01\">");
                sb.AppendLine("<ns2:REQHDR>");
                sb.AppendLine("<ns2:TrnNum>" + strSno + "</ns2:TrnNum>");
                sb.AppendLine("<ns2:TrnCode>000401</ns2:TrnCode>");
                sb.AppendLine("<ns2:CtryCode></ns2:CtryCode>");
                sb.AppendLine("</ns2:REQHDR><ns2:REQBDY>");
                //sb.AppendLine("<ns2:BranchId>0880</ns2:BranchId>");
                sb.AppendLine("<ns2:BranchId>" + strUserBranchId + "</ns2:BranchId>");
                //sb.AppendLine("<ns2:TellerId>790107</ns2:TellerId>");
                sb.AppendLine("<ns2:TellerId>" + TellerNo + "</ns2:TellerId>");
                //sb.AppendLine("<ns2:Acct>" + Account["Account"] + "</ns2:Acct>");
                sb.AppendLine("<ns2:Acct>" + accountNo + "</ns2:Acct>");
                sb.AppendLine("<ns2:CurCode></ns2:CurCode>");
                sb.AppendLine("<ns2:MacValue></ns2:MacValue>");

                sb.AppendLine("</ns2:REQBDY>");
                sb.AppendLine("</ns2:cmAcctProfileInqRq>");
                sb.AppendLine("</ns1:ServiceBody>");
                sb.AppendLine("</ns0:ServiceEnvelope>");

                strXML = sb.ToString();
                string bSendresult = SendESBData(strXML, "33401");
                if (bSendresult != "")
                {
                    bESBFlag = P4_33401D(bSendresult, accountNo, CustomID,CaseID);
                    if (!bESBFlag)
                    {
                        breakEach = false;
                    }
                }
                else
                {
                    result = "003|連接ESB服務器失敗";
                    bESBFlag = false;
                    breakEach = false;
                }
            //}
            if (!string.IsNullOrEmpty(result))
            {
                return result;
            }
            else if (bESBFlag)
            {
                result = "0001|成功";
            }
            else
            {
                result = "0002|檔案未開啟或其它錯誤";
            }
            return result;
        }

        //public bool P4_33401D(string strXML, string strApplNo, string strApplNoB)
        public bool P4_33401D(string strXML, string accountNo, string CustomID,string CaseID)
        {
            bool b33401 = true;
            string strRspCode = string.Empty;
            XmlDocument xmldoc = new XmlDocument();
            xmldoc.LoadXml(strXML);
            if (xmldoc != null)
            {
                IList<HostMsgGrp> resultlist = hostbiz.QueryHostMsgDetl("33401", "D");
                WriteLog(string.Format("獲取HostMsgDetl設定成功，共{0}筆", resultlist.Count));
                if (resultlist.Count > 0)
                {
                    Dictionary<string, string> dicValue = new Dictionary<string, string>();
                    XmlNamespaceManager mgr = new XmlNamespaceManager(xmldoc.NameTable);
                    mgr.AddNamespace("ns0", "http://ns.chinatrust.com.tw/XSD/CTCB/ESB/Message/EMF/ServiceEnvelope");
                    mgr.AddNamespace("ns1", "http://ns.chinatrust.com.tw/XSD/CTCB/ESB/Message/EMF/ServiceBody");
                    mgr.AddNamespace("ns2", "http://ns.chinatrust.com.tw/XSD/CTCB/ESB/Message/BSMF/cmAcctProfileInqRs/01");
                    XmlNode node = xmldoc.SelectSingleNode("/ns0:ServiceEnvelope/ns1:ServiceBody/ns2:cmAcctProfileInqRs/ns2:RESHDR", mgr);
                    if (node != null)
                    {
                        for (int j = 0; j < node.ChildNodes.Count; j++)
                        {
                            string strColumnName = node.ChildNodes[j].Name.ToString().ToUpper().Replace("NS2:", "");
                            string strValue = node.ChildNodes[j].InnerText;
                            if (!dicValue.ContainsKey(strColumnName))
                            {
                                IEnumerable<HostMsgGrp> m_List = resultlist.Where(m => m.datatype == "9" && m.edata.ToUpper().Equals(strColumnName));
                                if (m_List != null && m_List.Count() > 0)
                                {
                                    strValue = ChangeTheFormat(strValue);
                                }
                                dicValue.Add(strColumnName, strValue);
                            }
                            if (strColumnName.Equals("RSPCODE"))
                            {
                                strRspCode = strValue;
                                WriteLog("獲取RspCode成功，RspCode:" + strValue);
                            }
                        }
                    }

                    if (strRspCode.Equals("0000"))
                    {
                        node = xmldoc.SelectSingleNode("/ns0:ServiceEnvelope/ns1:ServiceBody/ns2:cmAcctProfileInqRs/ns2:RESBDY/ns2:BDYREC", mgr);
                        if (node != null)
                        {
                            for (int j = 0; j < node.ChildNodes.Count; j++)
                            {
                                string strColumnName = node.ChildNodes[j].Name.ToString().ToUpper().Replace("NS2:", "");
                                string strValue = node.ChildNodes[j].InnerText;
                                if (!dicValue.ContainsKey(strColumnName))
                                {
                                    IEnumerable<HostMsgGrp> m_List = resultlist.Where(m => m.datatype == "9" && m.edata.ToUpper().Equals(strColumnName));
                                    if (m_List != null && m_List.Count() > 0)
                                    {
                                        strValue = ChangeTheFormat(strValue);
                                    }
                                    dicValue.Add(strColumnName, strValue);
                                }
                            }
                        }
                        Dictionary<string, DataTable> dic = new Dictionary<string, DataTable>();
                        foreach (var list in resultlist)
                        {
                            bool bExists = false;
                            string strTableName = list.dest_table;
                            string strEdata = list.edata.ToUpper();
                            DataTable dt = new DataTable();
                            if (dic.ContainsKey(strTableName))
                            {
                                bExists = true;
                                dt = dic[strTableName];
                            }
                            else
                            {
                                dt.Columns.Add("SrcName");
                                dt.Columns.Add("dest_table");
                                dt.Columns.Add("dest_column");
                                dt.Columns.Add("Value");
                            }
                            DataRow dr = dt.NewRow();
                            dr[0] = strEdata;
                            dr[1] = strTableName;
                            dr[2] = list.dest_column;
                            if (dicValue.ContainsKey(strEdata))
                            {
                                dr[3] = dicValue[strEdata];
                            }
                            dt.Rows.Add(dr);
                            if (bExists)
                            {
                                dic[strTableName] = dt;
                            }
                            else
                            {
                                dic.Add(strTableName, dt);
                            }
                        }
                        WriteLog("開始拼接SQL:\r\n");
                        ArrayList array = new ArrayList();
                        //循環拼SQL 
                        foreach (string key in dic.Keys)
                        {
                            string strTableName = key;
                            string strSql = "insert into " + strTableName + " (";
                            strSql += "[SNO],[CUST_ID],[cCretDT],caseId,";
                            DataTable dt = dic[key];
                            for (int i = 0; i < dt.Rows.Count; i++)
                            {
                                strSql += "[" + dt.Rows[i]["dest_column"].ToString() + "],";
                            }
                            strSql = strSql.TrimEnd(',') + ") values(";
                            strSql += "NEXT VALUE FOR SEQ" + strTableName + ",'" + CustomID + "',GETDATE(),'"+ CaseID +"',";
                            for (int i = 0; i < dt.Rows.Count; i++)
                            {
                                strSql += "'" + dt.Rows[i]["Value"].ToString() + "',";
                            }
                            strSql = strSql.TrimEnd(',') + ");";
                            array.Add(strSql);
                            WriteLog("SQL:" + strSql);
                        }
                        bool flag = hostbiz.SaveESBData(array);
                        if (!flag)
                        {
                            b33401 = false;
                        }
                    }
                    else if (strRspCode == "0108")
                    {
                        b33401 = true;
                    }
                    else
                    {
                        b33401 = false;
                    }
                }
            }
            return b33401;
        }

        /// <summary>
        /// 原本
        /// </summary>
        /// <param name="accountNo"></param>
        /// <returns></returns>

        //public string P4_33401U(string accountNo)
        //{
        //    string strSno = hostbiz.GetMaxSno("33401").PadRight(19, ' ');
        //    strSno = strSno.Substring(0, strSno.Length - 2);
        //    StringBuilder sb = new StringBuilder();
        //    sb.AppendLine(strHerader1);
        //    sb.AppendLine(strHerader2);
        //    sb.AppendLine(strHerader3);
        //    sb.AppendLine("<ns1:StandardType>BSMF</ns1:StandardType>");
        //    sb.AppendLine("<ns1:StandardVersion>01</ns1:StandardVersion>");
        //    sb.AppendLine("<ns1:ServiceName>cmAcctProfileInq</ns1:ServiceName>");
        //    sb.AppendLine("<ns1:ServiceVersion>01</ns1:ServiceVersion>");
        //    sb.AppendLine("<ns1:SourceID>" + strSourceID + "</ns1:SourceID>");
        //    //sb.AppendLine("<ns1:SourceID>TWCMS</ns1:SourceID>");
        //    sb.AppendLine("<ns1:TransactionID>" + strSno + "</ns1:TransactionID>");
        //    sb.AppendLine("<ns1:RqTimestamp>" + DateTime.Now.ToString("yyyy-MM-dd") + "T" + DateTime.Now.ToString("HH:mm:ss.ff") + "+08:00" + "</ns1:RqTimestamp>");
        //    sb.AppendLine("</ns1:ServiceHeader>");
        //    sb.AppendLine("<ns1:ServiceBody xmlns:ns1=\"http://ns.chinatrust.com.tw/XSD/CTCB/ESB/Message/EMF/ServiceBody\">");
        //    sb.AppendLine("<ns2:cmAcctProfileInqRq xmlns:ns2=\"http://ns.chinatrust.com.tw/XSD/CTCB/ESB/Message/BSMF/cmAcctProfileInqRq/01\">");
        //    sb.AppendLine("<ns2:REQHDR>");
        //    sb.AppendLine("<ns2:TrnNum>" + strSno + "</ns2:TrnNum>");
        //    sb.AppendLine("<ns2:TrnCode>000401</ns2:TrnCode>");
        //    sb.AppendLine("<ns2:CtryCode></ns2:CtryCode>");
        //    sb.AppendLine("</ns2:REQHDR><ns2:REQBDY>");
        //    //sb.AppendLine("<ns2:BranchId>0880</ns2:BranchId>");
        //    sb.AppendLine("<ns2:BranchId>" + strUserBranchId + "</ns2:BranchId>");
        //    //sb.AppendLine("<ns2:TellerId>790107</ns2:TellerId>");
        //    sb.AppendLine("<ns2:TellerId>" + TellerNo + "</ns2:TellerId>");
        //    sb.AppendLine("<ns2:Acct>" + accountNo + "</ns2:Acct>");
        //    sb.AppendLine("<ns2:CurCode></ns2:CurCode>");
        //    sb.AppendLine("<ns2:MacValue></ns2:MacValue>");
        //    sb.AppendLine("</ns2:REQBDY>");
        //    sb.AppendLine("</ns2:cmAcctProfileInqRq>");
        //    sb.AppendLine("</ns1:ServiceBody>");
        //    sb.AppendLine("</ns0:ServiceEnvelope>");
        //    string bSendresult = SendESBData(sb.ToString(), "33401");
        //    if (string.IsNullOrEmpty(bSendresult))
        //        return "003|連接ESB服務器失敗";

        //    return P4_33401D(bSendresult, accountNo) ? "0001|成功" : "0002|檔案未開啟或其它錯誤";
        //}

        //public bool P4_33401D(string strXml, string accountNo)
        //{
        //    bool b33401 = true;
        //    string strRspCode = string.Empty;
        //    XmlDocument xmldoc = new XmlDocument();
        //    xmldoc.LoadXml(strXml);
        //    IList<HostMsgGrp> resultlist = hostbiz.QueryHostMsgDetl("33401", "D");
        //    //WriteLog(string.Format("獲取HostMsgDetl設定成功，共{0}筆", resultlist.Count));
        //    if (resultlist.Count > 0)
        //    {
        //        Dictionary<string, string> dicValue = new Dictionary<string, string>();
        //        XmlNamespaceManager mgr = new XmlNamespaceManager(xmldoc.NameTable);
        //        mgr.AddNamespace("ns0", "http://ns.chinatrust.com.tw/XSD/CTCB/ESB/Message/EMF/ServiceEnvelope");
        //        mgr.AddNamespace("ns1", "http://ns.chinatrust.com.tw/XSD/CTCB/ESB/Message/EMF/ServiceBody");
        //        mgr.AddNamespace("ns2", "http://ns.chinatrust.com.tw/XSD/CTCB/ESB/Message/BSMF/cmAcctProfileInqRs/01");
        //        XmlNode node =
        //            xmldoc.SelectSingleNode("/ns0:ServiceEnvelope/ns1:ServiceBody/ns2:cmAcctProfileInqRs/ns2:RESHDR",
        //                mgr);
        //        if (node != null)
        //        {
        //            for (int j = 0; j < node.ChildNodes.Count; j++)
        //            {
        //                string strColumnName = node.ChildNodes[j].Name.ToUpper().Replace("NS2:", "");
        //                string strValue = node.ChildNodes[j].InnerText;
        //                if (!dicValue.ContainsKey(strColumnName))
        //                {
        //                    IEnumerable<HostMsgGrp> mList =
        //                        resultlist.Where(m => m.datatype == "9" && m.edata.ToUpper().Equals(strColumnName));
        //                    if (mList.Any())
        //                    {
        //                        strValue = ChangeTheFormat(strValue);
        //                    }
        //                    dicValue.Add(strColumnName, strValue);
        //                }
        //                if (strColumnName.Equals("RSPCODE"))
        //                {
        //                    strRspCode = strValue;
        //                    WriteLog("獲取RspCode成功，RspCode:" + strValue);
        //                }
        //            }
        //        }

        //        if (strRspCode.Equals("0000"))
        //        {
        //            node = xmldoc.SelectSingleNode("/ns0:ServiceEnvelope/ns1:ServiceBody/ns2:cmAcctProfileInqRs/ns2:RESBDY/ns2:BDYREC", mgr);
        //            if (node != null)
        //            {
        //                for (int j = 0; j < node.ChildNodes.Count; j++)
        //                {
        //                    string strColumnName = node.ChildNodes[j].Name.ToUpper().Replace("NS2:", "");
        //                    string strValue = node.ChildNodes[j].InnerText;
        //                    if (!dicValue.ContainsKey(strColumnName))
        //                    {
        //                        IEnumerable<HostMsgGrp> mList = resultlist.Where(m => m.datatype == "9" && m.edata.ToUpper().Equals(strColumnName));
        //                        if (mList.Any())
        //                        {
        //                            strValue = ChangeTheFormat(strValue);
        //                        }
        //                        dicValue.Add(strColumnName, strValue);
        //                    }
        //                }
        //            }
        //            Dictionary<string, DataTable> dic = new Dictionary<string, DataTable>();
        //            foreach (var list in resultlist)
        //            {
        //                bool bExists = false;
        //                string strTableName = list.dest_table;
        //                string strEdata = list.edata.ToUpper();
        //                DataTable dt = new DataTable();
        //                if (dic.ContainsKey(strTableName))
        //                {
        //                    bExists = true;
        //                    dt = dic[strTableName];
        //                }
        //                else
        //                {
        //                    dt.Columns.Add("SrcName");
        //                    dt.Columns.Add("dest_table");
        //                    dt.Columns.Add("dest_column");
        //                    dt.Columns.Add("Value");
        //                }
        //                DataRow dr = dt.NewRow();
        //                dr[0] = strEdata;
        //                dr[1] = strTableName;
        //                dr[2] = list.dest_column;
        //                if (dicValue.ContainsKey(strEdata))
        //                {
        //                    dr[3] = dicValue[strEdata];
        //                }
        //                dt.Rows.Add(dr);
        //                if (bExists)
        //                {
        //                    dic[strTableName] = dt;
        //                }
        //                else
        //                {
        //                    dic.Add(strTableName, dt);
        //                }
        //            }
        //            WriteLog("開始拼接SQL:\r\n");
        //            ArrayList array = new ArrayList();
        //            //循環拼SQL 
        //            foreach (string key in dic.Keys)
        //            {
        //                string strTableName = key;
        //                string strSql = "insert into " + strTableName + " (";
        //                strSql += "[SNO],[CUST_ID],[cCretDT],";
        //                DataTable dt = dic[key];
        //                for (int i = 0; i < dt.Rows.Count; i++)
        //                {
        //                    strSql += "[" + dt.Rows[i]["dest_column"] + "],";
        //                }
        //                strSql = strSql.TrimEnd(',') + ") values(";
        //                strSql += "NEXT VALUE FOR SEQ" + strTableName + ",'',GETDATE(),";
        //                for (int i = 0; i < dt.Rows.Count; i++)
        //                {
        //                    strSql += "'" + dt.Rows[i]["Value"] + "',";
        //                }
        //                strSql = strSql.TrimEnd(',') + ");";
        //                array.Add(strSql);
        //                WriteLog("SQL:" + strSql);
        //            }
        //            bool flag = hostbiz.SaveESBData(array);
        //            if (!flag)
        //            {
        //                b33401 = false;
        //            }
        //        }
        //        else if (strRspCode == "0108")
        //        {
        //            b33401 = true;
        //        }
        //        else
        //        {
        //            b33401 = false;
        //        }
        //    }
        //    return b33401;
        //}

        /// <summary>
        /// 67050/67100電文(上行)
        /// </summary>
        /// <param name="ObligorID"></param>
        /// <returns></returns>
        /// <remarks>20160122 RC --> 20150111 宏祥 add 新增67100電文</remarks>
        public string P4_67100U(string ObligorID,string CaseID)
        {
            string result = "";
            bool bESBFlag = false;
            bool breakEach = true;
            string strXML = "";
            string strYear = Convert.ToInt16(DateTime.Now.Year - 1911).ToString("000");
            string strMonth = Convert.ToInt16(DateTime.Now.Month).ToString("00");
            string strDay = Convert.ToInt16(DateTime.Now.Day).ToString("00");
            string strHour = System.DateTime.Now.ToString("HHmmssfff");
            string strSno = "CSFS" + strYear.Substring(1, 2) + strMonth + strDay + strHour;
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(strHerader1);
            sb.AppendLine(strHerader2);
            sb.AppendLine(strHerader3);
            sb.AppendLine("<ns1:StandardType>BSMF</ns1:StandardType>");
            sb.AppendLine("<ns1:StandardVersion>01</ns1:StandardVersion>");
            sb.AppendLine("<ns1:ServiceName>cpIdntyDocListMod</ns1:ServiceName>");
            sb.AppendLine("<ns1:ServiceVersion>01</ns1:ServiceVersion>");
            sb.AppendLine("<ns1:SourceID>" + strSourceID + "</ns1:SourceID>");
            sb.AppendLine("<ns1:TransactionID>" + strSno + "</ns1:TransactionID>");
            sb.AppendLine("<ns1:RqTimestamp>" + System.DateTime.Now.ToString("yyyy-MM-dd") + "T" + System.DateTime.Now.ToString("HH:mm:ss.ff") + "+08:00" + "</ns1:RqTimestamp>");
            sb.AppendLine("</ns1:ServiceHeader>");
            sb.AppendLine("<ns1:ServiceBody xmlns:ns1=\"http://ns.chinatrust.com.tw/XSD/CTCB/ESB/Message/EMF/ServiceBody\">");
            sb.AppendLine("<ns2:cpIdntyDocListModRq xmlns:ns2=\"http://ns.chinatrust.com.tw/XSD/CTCB/ESB/Message/BSMF/cpIdntyDocListModRq/01\">");
            sb.AppendLine("<ns2:REQHDR>");
            sb.AppendLine("<ns2:TrnNum>" + strSno + "</ns2:TrnNum>");
            sb.AppendLine("<ns2:TrnCode>067050</ns2:TrnCode>");
            sb.AppendLine("<ns2:UserBranchId>" + BranchID + "</ns2:UserBranchId>");
            sb.AppendLine("<ns2:UserId>" + TellerNo + "</ns2:UserId>");
            sb.AppendLine("</ns2:REQHDR><ns2:REQBDY>");
            sb.AppendLine("<ns2:CustomerNo>0</ns2:CustomerNo>");
            sb.AppendLine("<ns2:ProfileType>4</ns2:ProfileType>");
            sb.AppendLine("<ns2:CustId>" + ObligorID + "</ns2:CustId>");
            sb.AppendLine("<ns2:CustType></ns2:CustType>");
            sb.AppendLine("</ns2:REQBDY>");
            sb.AppendLine("</ns2:cpIdntyDocListModRq>");
            sb.AppendLine("</ns1:ServiceBody>");
            sb.AppendLine("</ns0:ServiceEnvelope>");

            strXML = sb.ToString();
            string bSendresult = SendESBData(strXML, "67100");
            if (bSendresult != "")
            {
                bESBFlag = P4_67100D(bSendresult, ObligorID,CaseID);
                if (!bESBFlag)
                {
                    breakEach = false;
                }
            }
            else
            {
                result = "003|連接ESB服務器失敗";
                bESBFlag = false;
                breakEach = false;
            }
            //}
            if (!string.IsNullOrEmpty(result))
            {
                return result;
            }
            else if (bESBFlag)
            {
                result = "0001|成功";
            }
            else
            {
                result = "0002|檔案未開啟或其它錯誤";
            }
            return result;
        }

        /// <summary>
        /// 67050/67100電文(下行)
        /// </summary>
        /// <param name="strXML"></param>
        /// <param name="CustomID"></param>
        /// <returns></returns>
        /// <remarks>20160122 RC --> 20150114 宏祥 add 新增67100電文</remarks>
        public bool P4_67100D(string strXML, string CustomID,string CaseID)
        {
            bool b67100 = true;
            string strRspCode = string.Empty;
            XmlDocument xmldoc = new XmlDocument();
            xmldoc.LoadXml(strXML);
            if (xmldoc != null)
            {
                IList<HostMsgGrp> resultlist = hostbiz.QueryHostMsgDetl("67100", "D");
                WriteLog(string.Format("獲取HostMsgDetl設定成功，共{0}筆", resultlist.Count));
                if (resultlist.Count > 0)
                {
                    Dictionary<string, string> dicValue = new Dictionary<string, string>();
                    XmlNamespaceManager mgr = new XmlNamespaceManager(xmldoc.NameTable);
                    mgr.AddNamespace("ns0", "http://ns.chinatrust.com.tw/XSD/CTCB/ESB/Message/EMF/ServiceEnvelope");
                    mgr.AddNamespace("ns1", "http://ns.chinatrust.com.tw/XSD/CTCB/ESB/Message/EMF/ServiceBody");
                    mgr.AddNamespace("ns2", "http://ns.chinatrust.com.tw/XSD/CTCB/ESB/Message/BSMF/cpIdntyDocListModRs/01");
                    XmlNode node = xmldoc.SelectSingleNode("/ns0:ServiceEnvelope/ns1:ServiceBody/ns2:cpIdntyDocListModRs/ns2:RESHDR", mgr);
                    if (node != null)
                    {
                        for (int j = 0; j < node.ChildNodes.Count; j++)
                        {
                            string strColumnName = node.ChildNodes[j].Name.ToString().ToUpper().Replace("NS2:", "");
                            string strValue = node.ChildNodes[j].InnerText;
                            if (!dicValue.ContainsKey(strColumnName))
                            {
                                IEnumerable<HostMsgGrp> m_List = resultlist.Where(m => m.datatype == "9" && m.edata.ToUpper().Equals(strColumnName));
                                if (m_List != null && m_List.Count() > 0)
                                {
                                    strValue = ChangeTheFormat(strValue);
                                }
                                dicValue.Add(strColumnName, strValue);
                            }
                            if (strColumnName.Equals("RSPCODE"))
                            {
                                strRspCode = strValue;
                                WriteLog("獲取RspCode成功，RspCode:" + strValue);
                            }
                        }
                    }

                    if (strRspCode.Equals("0000"))
                    {
                        node = xmldoc.SelectSingleNode("/ns0:ServiceEnvelope/ns1:ServiceBody/ns2:cpIdntyDocListModRs/ns2:RESBDY/ns2:BDYREC", mgr);
                        if (node != null)
                        {
                            for (int j = 0; j < node.ChildNodes.Count; j++)
                            {
                                string strColumnName = node.ChildNodes[j].Name.ToString().ToUpper().Replace("NS2:", "");
                                string strValue = node.ChildNodes[j].InnerText;
                                if (!dicValue.ContainsKey(strColumnName))
                                {
                                    IEnumerable<HostMsgGrp> m_List = resultlist.Where(m => m.datatype == "9" && m.edata.ToUpper().Equals(strColumnName));
                                    if (m_List != null && m_List.Count() > 0)
                                    {
                                        strValue = ChangeTheFormat(strValue);
                                    }
                                    dicValue.Add(strColumnName, strValue);
                                }
                            }
                        }
                        Dictionary<string, DataTable> dic = new Dictionary<string, DataTable>();
                        foreach (var list in resultlist)
                        {
                            bool bExists = false;
                            string strTableName = list.dest_table;
                            string strEdata = list.edata.ToUpper();
                            DataTable dt = new DataTable();

                            if (dic.ContainsKey(strTableName))
                            {
                                bExists = true;
                                dt = dic[strTableName];
                            }
                            else
                            {
                                dt.Columns.Add("SrcName");
                                dt.Columns.Add("dest_table");
                                dt.Columns.Add("dest_column");
                                dt.Columns.Add("Value");
                            }
                            DataRow dr = dt.NewRow();
                            dr[0] = strEdata;
                            dr[1] = strTableName;
                            dr[2] = list.dest_column;
                            if (dicValue.ContainsKey(strEdata))
                            {
                                dr[3] = dicValue[strEdata];
                            }
                            dt.Rows.Add(dr);
                            if (bExists)
                            {
                                dic[strTableName] = dt;
                            }
                            else
                            {
                                dic.Add(strTableName, dt);
                            }
                        }
                        WriteLog("開始拼接SQL:\r\n");
                        ArrayList array = new ArrayList();
                        //循環拼SQL 
                        foreach (string key in dic.Keys)
                        {
                            string strTableName = key;
                            string strSql = "insert into " + strTableName + " (";
                            strSql += "[CifNo],[cCretDT],caseId,";
                            DataTable dt = dic[key];
                            for (int i = 0; i < dt.Rows.Count; i++)
                            {
                                strSql += "[" + dt.Rows[i]["dest_column"].ToString() + "],";
                            }
                            strSql = strSql.TrimEnd(',') + ") values(";
                            strSql += "NEXT VALUE FOR SEQ" + strTableName + ",GETDATE(),'"+CaseID+"',";
                            for (int i = 0; i < dt.Rows.Count; i++)
                            {
                                strSql += "'" + dt.Rows[i]["Value"].ToString() + "',";
                            }
                            strSql = strSql.TrimEnd(',') + ");";
                            array.Add(strSql);
                            WriteLog("SQL:" + strSql);
                        }
                        bool flag = hostbiz.SaveESBData(array);
                        if (!flag)
                        {
                            b67100 = false;
                        }
                    }
                    else if (strRspCode == "0108")
                    {
                        b67100 = true;
                    }
                    else
                    {
                        b67100 = false;
                    }
                }
            }
            return b67100;
        }

        /// <summary>
        /// 發送ESB
        /// </summary>
        /// <param name="strXML"></param>
        /// <param name="txtCode"></param>
        /// <returns></returns>
        public string SendESBData(string strXML, string txtCode)
        {
            string strResult = string.Empty;
            int _TransactionTimeout = 30;
            string ServerUrl = string.Empty;
            string ServerPort = string.Empty;
            string UserName = string.Empty;
            string Password = string.Empty;
            string ESBSendQueueName = string.Empty;
            string ESBReceiveQueueName = string.Empty;
            string ServerPortStandBy = string.Empty;
            bool msgNull = false;
            try
            {
                #region 將上行電文寫入到UXML UyyyyMMddhhmmss.xml
                string path = new DirectoryInfo("~/").Parent.FullName + "\\UXML";
                path = path.Replace("\\", "/");//Xml文件夾路徑
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);//如果文件夾不存在則創建Xml目錄
                }
                path += "/" + txtCode + "U" + DateTime.Now.ToString("yyyyMMddhhmmss") + ".xml";
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
                using (StreamWriter sw = new StreamWriter(new FileStream(path, FileMode.Create)))
                {
                    sw.Write(strXML);
                }
                #endregion

                WriteLog(txtCode + "\r\n" + strXML + "\r\n ------------------------------------------------------------\r\n\r\n");
                ServerUrl = ConfigurationManager.AppSettings["ServerUrl"].ToString();
                ServerPort = ConfigurationManager.AppSettings["ServerPort"].ToString();
                UserName = ConfigurationManager.AppSettings["UserName"].ToString();
                Password = ConfigurationManager.AppSettings["Password"].ToString();
                //Password = UtlString.DecodeBase64(Password);
                ESBSendQueueName = ConfigurationManager.AppSettings["ESBSendQueueName"].ToString();
                ESBReceiveQueueName = ConfigurationManager.AppSettings["ESBReceiveQueueName"].ToString();
                ServerPortStandBy = ConfigurationManager.AppSettings["ServerPortStandBy"].ToString();

                
                strResult = ConnESB(ServerUrl, ServerPort, UserName, Password, ESBSendQueueName, ESBReceiveQueueName, strXML, ref msgNull);
                if (msgNull)
                {
                    strResult = ConnESB(ServerUrl, ServerPortStandBy, UserName, Password, ESBSendQueueName, ESBReceiveQueueName, strXML, ref msgNull);
                }

                //DataTable dt = hostbiz.GetESBConnPara();//獲取連接參數
                //if (dt != null && dt.Rows.Count > 0)
                //{
                //    ServerUrl = dt.Rows[0]["prop_desc"].ToString();
                //    ServerPort = dt.Rows[1]["prop_desc"].ToString();
                //    UserName = dt.Rows[2]["prop_desc"].ToString();
                //    Password = dt.Rows[3]["prop_desc"].ToString();
                //    ESBSendQueueName = dt.Rows[4]["prop_desc"].ToString();
                //    ESBReceiveQueueName = dt.Rows[5]["prop_desc"].ToString();
                //}

                //strResult = "";
                //_serverurl = "";
                //_serverurl = "tcp://" + ServerUrl + ":" + ServerPort;
                //#region //测试时不执行到这
                //if (IsnotTest != "1")
                //{
                //    ///* 方法二,直接使用QueueConnectionFactory */
                //    QueueConnectionFactory factory = new TIBCO.EMS.QueueConnectionFactory(_serverurl);

                //    QueueConnection connection = factory.CreateQueueConnection(UserName, Password);

                //    QueueSession session = connection.CreateQueueSession(false, Session.AUTO_ACKNOWLEDGE);

                //    TIBCO.EMS.Queue queue = session.CreateQueue(ESBSendQueueName);

                //    QueueSender qsender = session.CreateSender(queue);

                //    /* send messages */
                //    TextMessage message = session.CreateTextMessage();
                //    message.Text = strXML;

                //    //一定要設定要reply的queue,這樣才收得到
                //    message.ReplyTo = (TIBCO.EMS.Destination)session.CreateQueue(ESBReceiveQueueName);

                //    qsender.Send(message);

                //    _messageid = message.MessageID;

                //    //receive message
                //    String messageselector = null;
                //    messageselector = "JMSCorrelationID = '" + _messageid + "'";

                //    TIBCO.EMS.Queue receivequeue = session.CreateQueue(ESBReceiveQueueName);

                //    QueueReceiver receiver = session.CreateReceiver(receivequeue, messageselector);

                //    connection.Start();

                //    //set up timeout 
                //    TIBCO.EMS.Message msg = receiver.Receive(_TransactionTimeout * 1000);

                //    if (msg == null)
                //    {
                //        strResult = "";
                //    }
                //    else
                //    {
                //        msg.Acknowledge();

                //        if (msg is TextMessage)
                //        {
                //            TextMessage tm = (TextMessage)msg;
                //            strResult = tm.Text;
                //        }
                //        else
                //        {
                //            strResult = msg.ToString();
                //        }
                //    }
                //    connection.Close();
                //}
                //#endregion
            }
            catch (Exception ex)
            {
                WriteLog("連接失敗，錯誤信息：" + ex.Message);
                strResult = "";
            }
            finally
            {
                //記錄下行XML
                string path = new DirectoryInfo("~/").Parent.FullName + "\\DXML";
                path = path.Replace("\\", "/");//Xml文件夾路徑
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);//如果文件夾不存在則創建Xml目錄
                }
                path += "/" + txtCode + "D" + DateTime.Now.ToString("yyyyMMddhhmmss") + ".xml";
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
                using (StreamWriter sw = new StreamWriter(new FileStream(path, FileMode.Create)))
                {
                    sw.Write(strResult);
                }
                WriteLog(txtCode + "D" + "\r\n" + strResult + "\r\n ------------------------------------------------------------\r\n\r\n");
            }
            
            return strResult;
        }
        public string ConnESB(string ServerUrl, string ServerPort, string UserName, string Password, string ESBSendQueueName, string ESBReceiveQueueName, string strXML, ref bool msgNull)
        {
            msgNull = false;
            string strResult = string.Empty;
            string _serverurl = string.Empty;
            string _messageid = string.Empty;
            int _TransactionTimeout = 30;

            strResult = "";
            #region //测试时不执行到这

            if (IsnotTest != "1")
            {
                _serverurl = "";
                _serverurl = "tcp://" + ServerUrl + ":" + ServerPort;
                /* 方法二,直接使用QueueConnectionFactory */
                QueueConnectionFactory factory = new TIBCO.EMS.QueueConnectionFactory(_serverurl);

                QueueConnection connection = factory.CreateQueueConnection(UserName, Password);

                QueueSession session = connection.CreateQueueSession(false, Session.AUTO_ACKNOWLEDGE);

                TIBCO.EMS.Queue queue = session.CreateQueue(ESBSendQueueName);

                QueueSender qsender = session.CreateSender(queue);

                /* send messages */
                TextMessage message = session.CreateTextMessage();
                message.Text = strXML;

                //一定要設定要reply的queue,這樣才收得到
                message.ReplyTo = (TIBCO.EMS.Destination) session.CreateQueue(ESBReceiveQueueName);

                qsender.Send(message);

                _messageid = message.MessageID;

                //receive message
                String messageselector = null;
                messageselector = "JMSCorrelationID = '" + _messageid + "'";

                TIBCO.EMS.Queue receivequeue = session.CreateQueue(ESBReceiveQueueName);

                QueueReceiver receiver = session.CreateReceiver(receivequeue, messageselector);

                connection.Start();

                //set up timeout 
                TIBCO.EMS.Message msg = receiver.Receive(_TransactionTimeout*1000);

                if (msg == null)
                {
                    msgNull = true;
                    strResult = "";
                }
                else
                {
                    msg.Acknowledge();

                    if (msg is TextMessage)
                    {
                        TextMessage tm = (TextMessage) msg;
                        strResult = tm.Text;
                    }
                    else
                    {
                        strResult = msg.ToString();
                    }
                }
                connection.Close();
            }
            #endregion

            return strResult;
        }
        
        public string Login()
        {
            bool flag = false;
            try
            {
                string strXML = "";
                Dictionary<string, string> list = new Dictionary<string, string>();
                list.Add("SystemId", "CSFS");
                list.Add("TrnProg", "009001");
                list.Add("TerminalId", strTerminalId);
                list.Add("Language", "12");
                list.Add("Filler", "");
                list.Add("Amount", "00000000000000000");
                list.Add("Password", ns2Password);
                list.Add("Branch", BranchID);
                WriteLog("SystemId:" + "CSFS");
                WriteLog("TrnProg"+"009001");
               WriteLog("TerminalId"+strTerminalId);
                WriteLog("Language"+"12");
                WriteLog("Filler"+"");
                WriteLog("Amount"+"00000000000000000");
                WriteLog("Password"+ ns2Password);
                WriteLog("Branch"+ BranchID);

                //2015-09-01
                //string strSno = hostbiz.GetMaxSno("P4_SgnOn").PadRight(20, ' ');
                string strYear = Convert.ToInt16(DateTime.Now.Year - 1911).ToString("000");
                string strMonth = Convert.ToInt16(DateTime.Now.Month).ToString("00");
                string strDay = Convert.ToInt16(DateTime.Now.Day).ToString("00");
                string strHour = System.DateTime.Now.ToString("HHmmssfff");
                string strSno = "CSFS" + strYear.Substring(1,2) + strMonth + strDay + strHour;
                DataTable dt = hostbiz.LoginParm();
                                WriteLog("login Para"+ dt.Rows.Count.ToString());
                if (dt != null && dt.Rows.Count > 0)
                {
                    foreach (string key in list.Keys.ToArray())
                    {
                        DataRow[] drList = dt.Select("prop_id = '" + key + "'");
                        if (drList != null && drList.Count() > 0)
                        {
                            list[key] = drList[0][1].ToString();
                        }
                    }
                }

                StringBuilder sb = new StringBuilder();
                sb.AppendLine(strHerader1);
                sb.AppendLine(strHerader2);
                sb.AppendLine(strHerader3);
                sb.AppendLine("<ns1:StandardType>BSMF</ns1:StandardType>");
                sb.AppendLine("<ns1:StandardVersion>01</ns1:StandardVersion>");
                sb.AppendLine("<ns1:TrackingID>j9Xx0a9bAeI03kHj33/Vfkx-TCA</ns1:TrackingID>");
                sb.AppendLine("<ns1:ServiceName>amSgnOnBancsAdd</ns1:ServiceName>");
                sb.AppendLine("<ns1:ServiceVersion>01</ns1:ServiceVersion>");
                sb.AppendLine("<ns1:SourceID>" + strSourceID + "</ns1:SourceID>");
                //sb.AppendLine("<ns1:SourceID>CSFS</ns1:SourceID>");
                sb.AppendLine("<ns1:TransactionID>" + strSno + "</ns1:TransactionID>");
                sb.AppendLine("<ns1:RqTimestamp>" + System.DateTime.Now.ToString("yyyy-MM-dd") + "T" + System.DateTime.Now.ToString("HH:mm:ss.ff") + "+08:00" + "</ns1:RqTimestamp>");
                sb.AppendLine("</ns1:ServiceHeader>");
                sb.AppendLine("<ns1:ServiceBody xmlns:ns1=\"http://ns.chinatrust.com.tw/XSD/CTCB/ESB/Message/EMF/ServiceBody\">");
                sb.AppendLine("<ns2:amSgnOnBancsAddRq xmlns:ns2=\"http://ns.chinatrust.com.tw/XSD/CTCB/ESB/Message/BSMF/amSgnOnBancsAddRq/01\">");
                sb.AppendLine("<ns2:REQHDR>");
                sb.AppendLine("<ns2:TrnNum>" + strSno + "</ns2:TrnNum>");
                sb.AppendLine("<ns2:SystemId>" + list["SystemId"].ToString() + "</ns2:SystemId>");
                sb.AppendLine("<ns2:TrnProg>" + list["TrnProg"].ToString() + "</ns2:TrnProg>");
                sb.AppendLine("</ns2:REQHDR><ns2:REQBDY>");
                sb.AppendLine("<ns2:TerminalId>" + strTerminalId + "</ns2:TerminalId>");
                sb.AppendLine("<ns2:Language>" + list["Language"].ToString() + "</ns2:Language>");
                sb.AppendLine("<ns2:Filler>" + list["Filler"].ToString() + "</ns2:Filler>");
                sb.AppendLine("<ns2:TellerNo>" + TellerNo + "</ns2:TellerNo>");
                sb.AppendLine("<ns2:Amount>" + list["Amount"].ToString() + "</ns2:Amount>");
                sb.AppendLine("<ns2:Password>" + ns2Password + "</ns2:Password>");
                sb.AppendLine("<ns2:Branch>" + strUserBranchId + "</ns2:Branch>");
                sb.AppendLine("</ns2:REQBDY>");
                sb.AppendLine("</ns2:amSgnOnBancsAddRq>");
                sb.AppendLine("</ns1:ServiceBody>");
                sb.AppendLine("</ns0:ServiceEnvelope>");

                strXML = sb.ToString();
                 WriteLog("XML:Start"+ strXML );
                string result = "";
                result = SendESBData(strXML, "P4_SgnOn");   //發送電文

                if (result != "")
                {
                    XmlDocument xmldoc = new XmlDocument();
                    xmldoc.LoadXml(result);
                    if (xmldoc != null)
                    {
                        XmlNamespaceManager mgr = new XmlNamespaceManager(xmldoc.NameTable);
                        mgr.AddNamespace("ns0", "http://ns.chinatrust.com.tw/XSD/CTCB/ESB/Message/EMF/ServiceEnvelope");
                        mgr.AddNamespace("ns1", "http://ns.chinatrust.com.tw/XSD/CTCB/ESB/Message/EMF/ServiceBody");
                        mgr.AddNamespace("ns2", "http://ns.chinatrust.com.tw/XSD/CTCB/ESB/Message/BSMF/amSgnOnBancsAddRs/01");
                        XmlNode node = xmldoc.SelectSingleNode("/ns0:ServiceEnvelope/ns1:ServiceBody/ns2:amSgnOnBancsAddRs/ns2:RESHDR/ns2:RspCode", mgr);
                        if (node != null)
                        {
                            string strValue = node.InnerText;
                            if (strValue == "03")
                            {
                                flag = true;
                            }
                            else
                            {
                                WriteLog("Node:" + strValue);
                                flag = false;
                            }
                        }
                        else
                        {
                            string strValue1 = node.InnerText;
                            WriteLog("Node:" + strValue1);
                            flag = false;
                        }
                    }
                }
                else
                {
                    WriteLog("result:" + "");
                    flag = false;
                }
            }
            catch
            {
                flag = false;
            }
            if (flag)
            {
                return "0001|成功";
            }
            else
            {
                return "0002|失敗";
            }
        }

        public string LogOut()
        {
            bool flag = false;
            try
            {
                string strXML = "";
                Dictionary<string, string> list = new Dictionary<string, string>();
                list.Add("TrnCode", "009003");
                list.Add("UserBranchId", BranchID);
                list.Add("UserId", TellerNo);
                list.Add("ExpenseCnt", "0");
                list.Add("ExpenseAmt", "00000000000000000+");
                list.Add("Filler1", " ");
                list.Add("AuthCode", "11");
                list.Add("IncomeCnt", "0");
                list.Add("IncomeAmt", "00000000000000000+");

                DataTable dt = hostbiz.LogoutParm();
                if (dt != null && dt.Rows.Count > 0)
                {
                    foreach (string key in list.Keys.ToArray())
                    {
                        DataRow[] drList = dt.Select("prop_id = '" + key + "'");
                        if (drList != null && drList.Count() > 0)
                        {
                            list[key] = drList[0][1].ToString();
                        }
                    }
                }
                //2015-09-01
                //string strSno = hostbiz.GetMaxSno("P4_SgnOff").PadRight(20, ' ');
                string strYear = Convert.ToInt16(DateTime.Now.Year - 1911).ToString("000");
                string strMonth = Convert.ToInt16(DateTime.Now.Month).ToString("00");
                string strDay = Convert.ToInt16(DateTime.Now.Day).ToString("00");
                string strHour = System.DateTime.Now.ToString("HHmmssfff");
                string strSno = "CSFS" + strYear.Substring(1,2) + strMonth + strDay + strHour;
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(strHerader1);
                sb.AppendLine(strHerader2);
                sb.AppendLine(strHerader3);
                sb.AppendLine("<ns1:StandardType>BSMF</ns1:StandardType>");
                sb.AppendLine("<ns1:StandardVersion>01</ns1:StandardVersion>");
                sb.AppendLine("<ns1:ServiceName>amSgnOffBancsAdd</ns1:ServiceName>");
                sb.AppendLine("<ns1:ServiceVersion>01</ns1:ServiceVersion>");
                //sb.AppendLine("<ns1:SourceID>CSFS</ns1:SourceID>");
                sb.AppendLine("<ns1:SourceID>" + strSourceID + "</ns1:SourceID>");
                sb.AppendLine("<ns1:TransactionID>" + strSno + "</ns1:TransactionID>");
                sb.AppendLine("<ns1:RqTimestamp>" + System.DateTime.Now.ToString("yyyy-MM-dd") + "T" + System.DateTime.Now.ToString("HH:mm:ss.ff") + "+08:00" + "</ns1:RqTimestamp>");
                sb.AppendLine("</ns1:ServiceHeader>");
                sb.AppendLine("<ns1:ServiceBody xmlns:ns1=\"http://ns.chinatrust.com.tw/XSD/CTCB/ESB/Message/EMF/ServiceBody\">");
                sb.AppendLine("<ns2:amSgnOffBancsAddRq xmlns:ns2=\"http://ns.chinatrust.com.tw/XSD/CTCB/ESB/Message/BSMF/amSgnOffBancsAddRq/01\">");
                sb.AppendLine("<ns2:REQHDR>");
                sb.AppendLine("<ns2:TrnNum>" + strSno + "</ns2:TrnNum>");
                sb.AppendLine("<ns2:TrnCode>" + list["TrnCode"].ToString() + "</ns2:TrnCode>");
                sb.AppendLine("<ns2:UserBranchId>" + list["UserBranchId"].ToString() + "</ns2:UserBranchId>");
                sb.AppendLine("<ns2:UserId>" + list["UserId"].ToString() + "</ns2:UserId>");
                sb.AppendLine("</ns2:REQHDR><ns2:REQBDY>");
                sb.AppendLine("<ns2:ExpenseCnt>" + list["ExpenseCnt"].ToString() + "</ns2:ExpenseCnt>");
                sb.AppendLine("<ns2:ExpenseAmt>" + list["ExpenseAmt"].ToString() + "</ns2:ExpenseAmt>");
                sb.AppendLine("<ns2:Filler1>" + list["Filler1"].ToString() + "</ns2:Filler1>");
                sb.AppendLine("<ns2:AuthCode>" + list["AuthCode"].ToString() + "</ns2:AuthCode>");
                sb.AppendLine("<ns2:IncomeCnt>" + list["IncomeCnt"].ToString() + "</ns2:IncomeCnt>");
                sb.AppendLine("<ns2:IncomeAmt>" + list["IncomeAmt"].ToString() + "</ns2:IncomeAmt>");
                sb.AppendLine("</ns2:REQBDY>");
                sb.AppendLine("</ns2:amSgnOffBancsAddRq>");
                sb.AppendLine("</ns1:ServiceBody>");
                sb.AppendLine("</ns0:ServiceEnvelope>");

                strXML = sb.ToString();

                string result = "";

                result = SendESBData(strXML, "P4_SgnOff");   //發送電文

                if (result != "")
                {
                    XmlDocument xmldoc = new XmlDocument();
                    xmldoc.LoadXml(result);
                    if (xmldoc != null)
                    {
                        XmlNamespaceManager mgr = new XmlNamespaceManager(xmldoc.NameTable);
                        mgr.AddNamespace("ns0", "http://ns.chinatrust.com.tw/XSD/CTCB/ESB/Message/EMF/ServiceEnvelope");
                        mgr.AddNamespace("ns1", "http://ns.chinatrust.com.tw/XSD/CTCB/ESB/Message/EMF/ServiceBody");
                        mgr.AddNamespace("ns2", "http://ns.chinatrust.com.tw/XSD/CTCB/ESB/Message/BSMF/amSgnOffBancsAddRs/01");
                        XmlNode node = xmldoc.SelectSingleNode("/ns0:ServiceEnvelope/ns1:ServiceBody/ns2:amSgnOffBancsAddRs/ns2:RESHDR/ns2:RspCode", mgr);
                        if (node != null)
                        {
                            string strValue = node.InnerText;
                            if (strValue == "08")
                            {
                                flag = true;
                            }
                            else
                            {
                                flag = false;
                            }
                        }
                        else
                        {
                            flag = false;
                        }
                    }
                }
                else
                {
                    flag = false;
                }
            }
            catch
            {
                flag = false;
            }
            if (flag)
            {
                return "0001|成功";
            }
            else
            {
                return "0002|失敗";
            }
        }

        public void WriteLog(string msg)
        {
            if (Directory.Exists(@".\Log") == false)
            {
                Directory.CreateDirectory(@".\Log");
            }
            LogManager.Exists("DebugLog").Debug(msg);
        }

        /// <summary>
        /// 轉換格式
        /// </summary>
        /// <param name="Oldval"></param>
        /// <returns></returns>
        public static string ChangeTheFormat(string Oldval)
        {
            try
            {
                string Newval = Oldval.Trim();
                if (!string.IsNullOrEmpty(Newval))//不等於空
                {
                    decimal outval;
                    bool flag = Decimal.TryParse(Newval.Replace("-", "").Replace("+", ""), out outval);
                    if (flag)//如果是數字 不是字母或者中文
                    {
                        if (Newval.IndexOf("-") < 0 || outval == 0)
                        {
                            //正整數 與 0
                            Newval = outval.ToString();
                        }
                        else
                        {
                            //負數
                            Newval = "-" + outval.ToString();
                        }

                        if (Newval.IndexOf('.') > -1)//去除小數點后的0  decima類型格式 0.00
                        {
                            Newval = Newval.TrimEnd('0');
                        }

                        if (Newval.IndexOf('.') == Newval.Length - 1)//如果小數點后無數字去除小數點。
                        {
                            Newval = Newval.TrimEnd('.');
                        }
                    }
                }
                return Newval;
            }
            catch (Exception)
            {
                return Oldval;
            }
        }

        public string ReplaceContent_Up(string pContent)
        {
            string mContent = pContent;

            mContent = mContent.Replace("&", "&amp;");
            mContent = mContent.Replace("'", "&apos;");
            mContent = mContent.Replace(@"""", "&quot;");
            mContent = mContent.Replace("<", "&lt;");
            mContent = mContent.Replace(">", "&gt;");

            return mContent;
        }

        public string ReplaceContent_Down(string pContent)
        {
            string mContent = pContent;

            mContent = mContent.Replace("&apos;", "'");
            mContent = mContent.Replace("&quot;", @"""");
            mContent = mContent.Replace("&lt;", "<");
            mContent = mContent.Replace("&gt;", ">");
            mContent = mContent.Replace("&amp;", "&");

            return mContent;
        }
    }
}
