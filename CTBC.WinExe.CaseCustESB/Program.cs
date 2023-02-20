using log4net;
using System;
using System.Configuration;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using TIBCO.EMS;
//using CTBC.CSFS.BussinessLogic;
using CTBC.CSFS.Pattern;
using System.Data;
using System.Text;
using System.Xml;
using CTBC.CSFS.Models;

namespace CTBC.WinExe.CaseCustESB
{
    class Program : BBRule
    {
        private static FileLog m_fileLog = new FileLog(ConfigurationManager.AppSettings["filelog"], AppDomain.CurrentDomain.FriendlyName.Replace(".vshost", "").Replace(".exe", ""));
        //private static FileLog m_fileLog2 = new FileLog(ConfigurationManager.AppSettings["filelog2"], AppDomain.CurrentDomain.FriendlyName.Replace(".vshost", "").Replace(".exe", ""));
        // 是否開啟電文測試1為開啟其他為不開啟
        private static string IsnotTest = ConfigurationManager.AppSettings["IsnotTest"].ToString();
        private const string strHerader1 = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>";
        private const string strHerader2 = "<ns0:ServiceEnvelope xmlns:ns0=\"http://ns.chinatrust.com.tw/XSD/CTCB/ESB/Message/EMF/ServiceEnvelope\">";
        private const string strHerader3 = "<ns1:ServiceHeader xmlns:ns1=\"http://ns.chinatrust.com.tw/XSD/CTCB/ESB/Message/EMF/ServiceHeader\">";
        private static string strSourceID = ConfigurationManager.AppSettings["SourceID"].ToString();
        ILog log = LogManager.GetLogger("DebugLog");
        static object _lockLog = new object();

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        private static void Main()
        {
            Program mainProgram = new Program();

            int WaitTime = 0;
            if (ConfigurationManager.AppSettings["WaitTimeForTest"] != null)
            {
                WaitTime = int.Parse(ConfigurationManager.AppSettings["WaitTimeForTest"].ToString());
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.Information, string.Format("目前故意只是等待  {0}   秒!! ", WaitTime));
            }


            System.Threading.Thread.Sleep(1000 * WaitTime);



            // 20221202, 增加, 防止重覆執行....  CTBC.WinExe.CaseCustESB
            string exeFile = "CTBC.WinExe.CaseCustESB";
            if (mainProgram.IsProcessRunning(exeFile))
            {
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.Information, string.Format("前一支 {0} 正在執行中， 故本次不進行執行!! ", exeFile));
                return;
            }
            else
            {
                mainProgram.Process();
                mainProgram.Process2();
            }



        }
        private void Process()
        {
            try
            {
                // log.config
                log4net.Config.XmlConfigurator.Configure(new System.IO.FileInfo(AppDomain.CurrentDomain.BaseDirectory + @"\Log.config"));

                // 取得要發查的資料
                DataTable dt = GetCaseCustRFDMSend();


                if (dt.Rows.Count==0)
                {
                    // log 記錄
                    WriteLog("========== 目前無資料 ! 結束程式!!  ==========");
                    return;
                }

                // log 記錄
                WriteLog("==========  RFDM 發查開始 ==========");

                // 存在查詢中的資料，才進行發查
                if (dt != null && dt.Rows.Count > 0)
                {
                    #region 逐筆拋查

                    foreach (DataRow dtRow in dt.Rows)
                    {
                        try
                        {
                            // log 記錄
                            WriteLog("========== 交易序號: " + dtRow["TrnNum"].ToString() + " start  GenerateRequestXml ==========");

                            // 取得上行XML
                            string requestXml = GenerateRequestXml(dtRow);

                            // 上行XML若為空，則不再執行發查
                            if (requestXml == null)
                            {
                                WriteLog("==========  Error in GenerateRequestXml,交易序號為 " + dtRow["TrnNum"].ToString());
                                continue;
                            }

                            // log 記錄
                            WriteLog("========== 交易序號: " + dtRow["TrnNum"].ToString() + "  RequestXML:\r\n" + requestXml);
                            WriteLog("========== 交易序號: " + dtRow["TrnNum"].ToString() + "  end  GenerateRequestXml");
                            WriteLog("========== 交易序號: " + dtRow["TrnNum"].ToString() + "  start Send RequestXML");

                            // 取得下行XML
                            string bSendresult = SendESBData(dtRow["TrnNum"].ToString(), requestXml, "ESB");

                            if (bSendresult != "")
                            {

                                // log 記錄
                                WriteLog("========== 交易序號: " + dtRow["TrnNum"].ToString() + " ResponXML:\r\n");
                                WriteLog("========== 交易序號: " + dtRow["TrnNum"].ToString() + " end Send RequestXML ");
                                WriteLog("========== 交易序號: " + dtRow["TrnNum"].ToString() + " start AnalyticResponXMl ");

                                // 解析下行XML
                                bool _rowresult = AnalyticResponXMl(dtRow["TrnNum"].ToString(), dtRow["VersionNewID"].ToString(), bSendresult, dtRow["QueryStatus"].ToString());

                                // 如果有錯, 直接回錯
                                if (!_rowresult)
                                {
                                    WriteLog("========== 交易序號: " + dtRow["TrnNum"].ToString() + " AnalyticResponXMl has error ");
                                }

                                // log 記錄
                                WriteLog(" ==========  end AnalyticResponXMl ");
                            }
                            else
                            {
                                // log 記錄
                                WriteLog(string.Format(" ==========  連接ESB服務器失敗, 已押失敗 , 案件 {0} ", dtRow["TrnNum"].ToString()));

                                // 202212/8 若空白時.... 改狀態, 以免重覆砲.....並存入錯誤訊息.....將.. CaseCustNewRFDMSend.RFDMSendStatus='03' 設為失敗...
                                string updateFail = @"UPDATE CaseCustNewRFDMSend SET RFDMSendStatus='03' WHERE VersionNewID=@VersionNewID";
                                
                                Parameter.Clear();
                                Parameter.Add(new CommandParameter("@VersionNewID", dtRow["VersionNewID"].ToString()));
                                ExecuteNonQuery(updateFail);

                                
                            }
                        }
                        catch (Exception ex)
                        {
                            WriteLog("========== 交易序號: " + dtRow["TrnNum"].ToString() + "發查異常，錯誤信息：" + ex.Message);
                        }

                    }

                    #endregion
                }
                else
                {
                    WriteLog("========== 沒有可發查的資料 ==========");
                }

                // log 記錄
                WriteLog("==========  RFDM 發查結束 ==========");
            }
            catch (Exception ex)
            {
                WriteLog("程式異常，錯誤信息：" + ex.Message);
            }
        }

        private void Process2()
        {
            try
            {
                // log.config
                log4net.Config.XmlConfigurator.Configure(new System.IO.FileInfo(AppDomain.CurrentDomain.BaseDirectory + @"\Log.config"));

                // 取得要發查的資料
                DataTable dt2 = GetWarningHistorySend();

                // log 記錄
                WriteLog2("==========  WarningHistory 發查開始 ==========");

                // 存在查詢中的資料，才進行發查
                if (dt2 != null && dt2.Rows.Count > 0)
                {
                    #region 逐筆拋查

                    foreach (DataRow dt2Row in dt2.Rows)
                    {
                        try
                        {
                            // log 記錄
                            WriteLog2("========== 交易序號: " + dt2Row["TrnNum"].ToString() + " start  GenerateRequestXml ==========");

                            // 取得上行XML
                            string requestXml2 = GenerateRequestXml2(dt2Row);

                            // 上行XML若為空，則不再執行發查
                            if (requestXml2 == null)
                            {
                                WriteLog2("==========  Error in GenerateRequestXml,交易序號為 " + dt2Row["TrnNum"].ToString());
                                continue;
                            }

                            // log 記錄
                            WriteLog2("========== 交易序號: " + dt2Row["TrnNum"].ToString() + "  RequestXML:\r\n" + requestXml2);
                            WriteLog2("========== 交易序號: " + dt2Row["TrnNum"].ToString() + "  end  GenerateRequestXml");
                            WriteLog2("========== 交易序號: " + dt2Row["TrnNum"].ToString() + "  start Send RequestXML");

                            // 取得下行XML
                            string bSendresult = SendESBData(dt2Row["TrnNum"].ToString(), requestXml2, "ESB");

                            if (bSendresult != "")
                            {

                                // log 記錄
                                WriteLog2("========== 交易序號: " + dt2Row["TrnNum"].ToString() + " ResponXML:\r\n");
                                WriteLog2("========== 交易序號: " + dt2Row["TrnNum"].ToString() + " end Send RequestXML ");
                                WriteLog2("========== 交易序號: " + dt2Row["TrnNum"].ToString() + " start AnalyticResponXMl ");

                                // 解析下行XML
                                bool _rowresult = AnalyticResponXMl2(dt2Row["TrnNum"].ToString(), bSendresult);

                                // 如果有錯, 直接回錯
                                if (!_rowresult)
                                {
                                    WriteLog2("========== 交易序號: " + dt2Row["TrnNum"].ToString() + " AnalyticResponXMl has error ");
                                }

                                // log 記錄
                                WriteLog2(" ==========  end AnalyticResponXMl ");
                            }
                            else
                            {
                                // log 記錄
                                WriteLog2(" ==========  連接ESB服務器失敗 ");
                            }
                        }
                        catch (Exception ex)
                        {
                            WriteLog2("========== 交易序號: " + dt2Row["TrnNum"].ToString() + "發查異常，錯誤信息：" + ex.Message);
                        }

                    }

                    #endregion
                }
                else
                {
                    WriteLog2("========== 沒有可發查的資料 ==========");
                }

                // log 記錄
                WriteLog2("==========  WarningHistory  發查結束 ==========");
            }
            catch (Exception ex)
            {
                WriteLog2("程式異常，錯誤信息：" + ex.Message);
            }
        }


        public bool IsProcessRunning(string name)
        {
            List<System.Diagnostics.Process> res = new List<System.Diagnostics.Process>();

            foreach (System.Diagnostics.Process clsProcess in System.Diagnostics.Process.GetProcesses())
            {
                if (clsProcess.ProcessName.Contains(name))
                {
                    res.Add(clsProcess);
                }
            }

            //var c = res;如果大於1個, 代表排除自己後, 還有另一個在RUNING.. 
            return res.Count > 1;
        } 

        /// <summary>
        /// 解析下行數據XML
        /// </summary>
        /// <param name="strTrnNum"></param>
        /// <param name="xmlString"></param>
        /// <returns></returns>
        private bool AnalyticResponXMl2(string strTrnNum, string xmlString)
        {
            bool _result = true;
            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(xmlString);

                // xml中帶xmlns:，使用SelectNodes或SelectSingleNode去查找其下节点，需要添加对应的XmlNamespaceManager参数，才可以的
                XmlNamespaceManager m = new XmlNamespaceManager(xmlDoc.NameTable);
                m.AddNamespace("ns0", "http://ns.chinatrust.com.tw/XSD/CTCB/ESB/Message/EMF/ServiceEnvelope");
                m.AddNamespace("ns1", "http://ns.chinatrust.com.tw/XSD/CTCB/ESB/Message/EMF/ServiceBody");
                m.AddNamespace("ns2", "http://ns.chinatrust.com.tw/XSD/CTCB/ESB/Message/BSMF/ceDemDepTrnCntInqRs/01");
                XmlNode node1 = xmlDoc.SelectSingleNode("ns0:ServiceEnvelope/ns1:ServiceBody/ns2:ceDemDepTrnCntInqRs/ns2:RESHDR/ns2:RspCode", m);

                // 發查結果
                string sRspCode = (node1 != null ? node1.InnerText : "");

                //  取得錯誤信息
                XmlNode oRspMsgNode = xmlDoc.SelectSingleNode("ns0:ServiceEnvelope/ns1:ServiceBody/ns2:ceDemDepTrnCntInqRs/ns2:RESHDR/ns2:RspMsg", m);

                string sRspMsg = (oRspMsgNode != null ? oRspMsgNode.InnerText : "");

                //  發查成功
                if (!string.IsNullOrEmpty(sRspCode) && (sRspCode == "0000" || sRspCode == "C001"))
                {
                    WriteLog2("==========    發查成功， TrnNum=" + strTrnNum + "RspCode = " + sRspCode);
                    // 查無資料時，將狀態更新完02代表已解析完成，因為更新為01之後，還要要獲取excel的明細會在更新成02
                    if (sRspCode == "C001")
                    {
                        // 更新WarningQueryHistory，為成功
                        EditWarningQueryHistory(strTrnNum, "02", sRspCode, sRspMsg);
                    }
                    else
                    {
                        // WarningQueryHistory，為成功
                        EditWarningQueryHistory(strTrnNum, "01", sRspCode, sRspMsg);
                    }
                }
                else
                {

                    // WarningQueryHistory，為失敗
                    WriteLog2("==========  發查失敗， TrnNum=" + strTrnNum + "RspCode = " + sRspCode + "RspCode = " + sRspMsg);
                    EditWarningQueryHistory(strTrnNum, "03", sRspCode, sRspMsg);
                }
            }
            catch (Exception ex)
            {
                _result = false;
                WriteLog2(" ========== AnalyticResponXMl Has Error, Error is " + ex.ToString());
            }

            return _result;
        }

        /// <summary>
        /// 更新警示歷史記錄表
        /// </summary>
        /// <param name="strTrnNum"></param>
        /// <param name="strESBStatus"></param>
        /// <param name="strRspCode"></param>
        /// <param name="strRspMsg"></param>
        /// <returns></returns>
        public bool EditWarningQueryHistory(string strTrnNum, string strESBStatus, string strRspCode, string strRspMsg)
        {
            try
            {
                string sql = @"UPDATE [WarningQueryHistory] 
                            SET 
                                [ESBStatus] = @ESBStatus,
                                [RspCode] = @RspCode,
                                [RspMsg] = @RspMsg,
                                [ModifiedDate] = GETDATE(), 
                                [ModifiedUser] = 'SYSTEM' 
                            WHERE TrnNum = @TrnNum";

                Parameter.Clear();
                Parameter.Add(new CommandParameter("@TrnNum", strTrnNum));
                Parameter.Add(new CommandParameter("@ESBStatus", strESBStatus));
                Parameter.Add(new CommandParameter("@RspCode", strRspCode));
                Parameter.Add(new CommandParameter("@RspMsg", strRspMsg));

                return ExecuteNonQuery(sql) > 0;
            }
            catch (Exception ex)
            {
                WriteLog("\t 更新來文檔案查詢條件帳號檔 失敗, TrnNum = " + strTrnNum + "錯誤訊息" + ex.Message.ToString());
                return false;
            }
        }
        /// <summary>
        /// 組織上行XML
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private string GenerateRequestXml2(DataRow data)
        {
            if (data == null) { return null; }

            // 需要上送的數據
            string strTrnNum = data["TrnNum"].ToString();
            string strIDNo = data["CustId"].ToString();
            string strAcctDesc = data["AcctDesc"].ToString();
            string strAcctNo = data["CustAccount"].ToString();
            string strStart_Jnrst_Date = data["ForCDateS"].ToString().Substring(0, 4) + data["ForCDateS"].ToString().Substring(5, 2) + data["ForCDateS"].ToString().Substring(8, 2);
            string strEnd_Jnrst_Date = data["ForCDateE"].ToString().Substring(0, 4) + data["ForCDateE"].ToString().Substring(5, 2) + data["ForCDateE"].ToString().Substring(8, 2);

            string strHerader1 = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>";
            string strHerader2 = "<ns0:ServiceEnvelope xmlns:ns0=\"http://ns.chinatrust.com.tw/XSD/CTCB/ESB/Message/EMF/ServiceEnvelope\">";
            string strHerader3 = "<ns1:ServiceHeader xmlns:ns1=\"http://ns.chinatrust.com.tw/XSD/CTCB/ESB/Message/EMF/ServiceHeader\">";
            try
            {
                string strYear = Convert.ToInt16(DateTime.Now.Year - 1911).ToString("000");
                string strMonth = Convert.ToInt16(DateTime.Now.Month).ToString("00");
                string strDay = Convert.ToInt16(DateTime.Now.Day).ToString("00");
                string strHour = System.DateTime.Now.ToString("HHmmssfff");
                string strSno = "CSFS" + strYear.Substring(1, 2) + strMonth + strDay + strHour;
                string strSourceID = "CSFS";
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(strHerader1);
                sb.AppendLine(strHerader2);
                sb.AppendLine(strHerader3);
                sb.AppendLine("<ns1:StandardType>BSMF</ns1:StandardType>");
                sb.AppendLine("<ns1:StandardVersion>01</ns1:StandardVersion>");
                sb.AppendLine("<ns1:ServiceName>ceDemDepTrnCntInq</ns1:ServiceName>");
                sb.AppendLine("<ns1:ServiceVersion>01</ns1:ServiceVersion>");
                sb.AppendLine("<ns1:SourceID>" + strSourceID + "</ns1:SourceID>");
                sb.AppendLine("<ns1:TransactionID>" + strTrnNum + "</ns1:TransactionID>");
                sb.AppendLine("<ns1:RqTimestamp>" + System.DateTime.Now.ToString("yyyy-MM-dd") + "T" + System.DateTime.Now.ToString("HH:mm:ss.ff") + "+08:00" + "</ns1:RqTimestamp>");
                sb.AppendLine("</ns1:ServiceHeader>");
                sb.AppendLine("<ns1:ServiceBody xmlns:ns1=\"http://ns.chinatrust.com.tw/XSD/CTCB/ESB/Message/EMF/ServiceBody\">");
                sb.AppendLine("<ns2:ceDemDepTrnCntInqRq xmlns:ns2=\"http://ns.chinatrust.com.tw/XSD/CTCB/ESB/Message/BSMF/ceDemDepTrnCntInqRq/01\">");
                sb.AppendLine("<ns2:REQHDR>");
                sb.AppendLine("<ns2:TrnNum>" + strTrnNum + "</ns2:TrnNum>");
                sb.AppendLine("<ns2:TrnCode></ns2:TrnCode>");
                sb.AppendLine("</ns2:REQHDR><ns2:REQBDY>");
                sb.AppendLine("<ns2:CustId>" + strIDNo + "</ns2:CustId>");
                sb.AppendLine("<ns2:AcctNo>" + strAcctNo + "</ns2:AcctNo>");
                sb.AppendLine("<ns2:StartDate>" + strStart_Jnrst_Date + "</ns2:StartDate>");
                sb.AppendLine("<ns2:EndDate>" + strEnd_Jnrst_Date + "</ns2:EndDate>");
                sb.AppendLine("<ns2:Type>0</ns2:Type>");
                sb.AppendLine("<ns2:PromoCode></ns2:PromoCode>");
                sb.AppendLine("<ns2:AcctDesc>" + strAcctDesc + "</ns2:AcctDesc>");
                sb.AppendLine("<ns2:Curr></ns2:Curr>");
                sb.AppendLine("<ns2:Channel>CSFS</ns2:Channel>");
                sb.AppendLine("</ns2:REQBDY>");
                sb.AppendLine("</ns2:ceDemDepTrnCntInqRq>");
                sb.AppendLine("</ns1:ServiceBody>");
                sb.AppendLine("</ns0:ServiceEnvelope>");

                return sb.ToString();
            }
            catch( Exception ex)
            {
                WriteLog("\t 組織上行XML 失敗, trnnum = " + strTrnNum + "錯誤訊息" + ex.Message.ToString());
                return null;
            }
        }
        /// <summary>
        /// 產生上行XML
        /// </summary>
        /// <param name="data">上送的數據</param>
        /// <returns></returns>
        private string GenerateRequestXml(DataRow data)
        {
            if (data == null) { return null; }

            // 需要上送的數據
            string strTrnNum = data["TrnNum"].ToString();
            string strVersionNewID = data["VersionNewID"].ToString();
            string strIDNo = data["ID_No"].ToString();
            string strAcctNo = data["Acct_No"].ToString();
            string strStart_Jnrst_Date = data["Start_Jnrst_Date"].ToString();
            string strEnd_Jnrst_Date = data["End_Jnrst_Date"].ToString();
            string strType = data["Type"].ToString();
            string strAcctDesc = data["acctDesc"].ToString().Trim();

            try
            {
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
                sb.AppendLine("<ns1:ServiceName>ceDemDepTrnCntInq</ns1:ServiceName>");
                sb.AppendLine("<ns1:ServiceVersion>01</ns1:ServiceVersion>");
                sb.AppendLine("<ns1:SourceID>" + strSourceID + "</ns1:SourceID>");
                sb.AppendLine("<ns1:TransactionID>" + strTrnNum + "</ns1:TransactionID>");
                sb.AppendLine("<ns1:RqTimestamp>" + System.DateTime.Now.ToString("yyyy-MM-dd") + "T" + System.DateTime.Now.ToString("HH:mm:ss.ff") + "+08:00" + "</ns1:RqTimestamp>");
                sb.AppendLine("</ns1:ServiceHeader>");
                sb.AppendLine("<ns1:ServiceBody xmlns:ns1=\"http://ns.chinatrust.com.tw/XSD/CTCB/ESB/Message/EMF/ServiceBody\">");
                sb.AppendLine("<ns2:ceDemDepTrnCntInqRq xmlns:ns2=\"http://ns.chinatrust.com.tw/XSD/CTCB/ESB/Message/BSMF/ceDemDepTrnCntInqRq/01\">");
                sb.AppendLine("<ns2:REQHDR>");
                sb.AppendLine("<ns2:TrnNum>" + strTrnNum + "</ns2:TrnNum>");
                sb.AppendLine("<ns2:TrnCode></ns2:TrnCode>");
                sb.AppendLine("</ns2:REQHDR><ns2:REQBDY>");
                sb.AppendLine("<ns2:CustId>" + strIDNo + "</ns2:CustId>");
                sb.AppendLine("<ns2:AcctNo>" + strAcctNo + "</ns2:AcctNo>");
                sb.AppendLine("<ns2:StartDate>" + strStart_Jnrst_Date + "</ns2:StartDate>");
                sb.AppendLine("<ns2:EndDate>" + strEnd_Jnrst_Date + "</ns2:EndDate>");
                sb.AppendLine("<ns2:Type>" + strType + "</ns2:Type>");
                sb.AppendLine("<ns2:PromoCode></ns2:PromoCode>");
                sb.AppendLine("<ns2:AcctDesc>" + strAcctDesc + "</ns2:AcctDesc>");
                sb.AppendLine("<ns2:Curr></ns2:Curr>");
                sb.AppendLine("<ns2:Channel>CSFS</ns2:Channel>");
                sb.AppendLine("</ns2:REQBDY>");
                sb.AppendLine("</ns2:ceDemDepTrnCntInqRq>");
                sb.AppendLine("</ns1:ServiceBody>");
                sb.AppendLine("</ns0:ServiceEnvelope>");

                return sb.ToString();
            }
            catch (Exception ex)
            {
                WriteLog("\t 產生上行XML 失敗, Trnnum = " + strTrnNum + "錯誤訊息" + ex.Message.ToString());
                return null;
            }
        }

        /// <summary>
        /// 解析下行XML
        /// </summary>
        /// <param name="strTrnNum">交易序號</param>
        /// <param name="xmlString">下行XML</param>
        /// <returns></returns>
        private bool AnalyticResponXMl(string strTrnNum, string strVersionNewID, string xmlString, string strQueryStatus)
        {
            bool _result = true;
            try
            {

                XmlDocument xmlDoc = new XmlDocument();
                //if (xmlString == "")
                //{
                //    // 本段程式測試用, 只是為了模擬錯誤出現E803時的XML....

                //    xmlDoc.Load("test.xml");

                //}
                //else
                {
                    xmlDoc.LoadXml(xmlString);
                }
                

    
                
                

                // xml中帶xmlns:，使用SelectNodes或SelectSingleNode去查找其下节点，需要添加对应的XmlNamespaceManager参数，才可以的
                XmlNamespaceManager m = new XmlNamespaceManager(xmlDoc.NameTable);
                m.AddNamespace("ns0", "http://ns.chinatrust.com.tw/XSD/CTCB/ESB/Message/EMF/ServiceEnvelope");
                m.AddNamespace("ns1", "http://ns.chinatrust.com.tw/XSD/CTCB/ESB/Message/EMF/ServiceBody");
                m.AddNamespace("ns2", "http://ns.chinatrust.com.tw/XSD/CTCB/ESB/Message/BSMF/ceDemDepTrnCntInqRs/01");
                XmlNode node1 = xmlDoc.SelectSingleNode("ns0:ServiceEnvelope/ns1:ServiceBody/ns2:ceDemDepTrnCntInqRs/ns2:RESHDR/ns2:RspCode", m);

                // 發查結果
                string sRspCode = (node1 != null ? node1.InnerText : "");

                //  取得錯誤信息
                XmlNode oRspMsgNode = xmlDoc.SelectSingleNode("ns0:ServiceEnvelope/ns1:ServiceBody/ns2:ceDemDepTrnCntInqRs/ns2:RESHDR/ns2:RspMsg", m);

                string sRspMsg = (oRspMsgNode != null ? oRspMsgNode.InnerText : "");

                //  發查成功
                if (!string.IsNullOrEmpty(sRspCode) && (sRspCode == "0000" || sRspCode == "C001"))
                {
                    // log 記錄
                    WriteLog("==========    發查成功， TrnNum=" + strTrnNum + "RspCode = " + sRspCode);

                    // 查無資料時，將狀態更新完08代表案件處理完畢，因為更新為01之後，還要要獲取excel的明細會在更新成08
                    if (sRspCode == "C001")
                    {
                        // 更新CaseCustNewRFDMSend，為成功
                        EditCaseCustRFDMSend(strTrnNum, "08", sRspCode, sRspMsg);
                    }
                    else
                    {
                        // 更新CaseCustNewRFDMSend，為成功
                        EditCaseCustRFDMSend(strTrnNum, "01", sRspCode, sRspMsg);
                    }

                    // 查詢此賬號3種RFDM類型都返回C001，代表沒有明細所以不用跑產出明細排程
                    if (QueryRFDMC001(strVersionNewID))
                    {
                        //  將version檔的RFDM發查狀態更新為【產出回文文檔】
                        EditCaseCustDetails(strVersionNewID, "4", ""); // 20230109, 若全部無相關資料, 則視為成功. 所以將99改為4成功
                        //20230109, 新增, 若打完同一個案號下的所有RFDM, 都是C001, 無相關資料時, 就可以準備產檔了.. 不用等ATM下載....
                        // 因此, 將CaseCustDetails.Execl_file 押成'FTP', 即可產檔...
                        EditCaseCustDetailsExcel_file(strVersionNewID, "FTP");
                    }
                    else if (QueryRFDMStatus08(strVersionNewID))
                    {
                        //  將version檔的RFDM發查狀態更新為【產出回文文檔】
                        EditCaseCustDetails(strVersionNewID, "8", "");
                    }
                    // 判斷是否將Version檔更新為成功，因RFDMSend檔有3筆資料
                    else if (QueryRFDMSendByStatus(strVersionNewID))
                    {
                        //  將version檔的RFDM發查狀態更新為【成功】
                        EditCaseCustDetails(strVersionNewID, "4", "");
                    }
                }
                else
                {
                    // xml中帶xmlns:，使用SelectNodes或SelectSingleNode去查找其下节点，需要添加对应的XmlNamespaceManager参数，才可以的
                    XmlNamespaceManager m1 = new XmlNamespaceManager(xmlDoc.NameTable);
                    m1.AddNamespace("ns0", "http://ns.chinatrust.com.tw/XSD/CTCB/ESB/Message/EMF/ServiceEnvelope");
                    m1.AddNamespace("ns1", "http://ns.chinatrust.com.tw/XSD/CTCB/ESB/Message/EMF/ServiceError");
                    m1.AddNamespace("ns2", "http://ns.chinatrust.com.tw/XSD/CTCB/ESB/Message/EMF/Common");
                    XmlNode nodeCode = xmlDoc.SelectSingleNode("ns0:ServiceEnvelope/ns1:ServiceError/ns2:Error/ns2:ErrorCode", m1);
                    XmlNode nodeMess = xmlDoc.SelectSingleNode("ns0:ServiceEnvelope/ns1:ServiceError/ns2:Error/ns2:ErrorMessage", m1);
                    // log 記錄
                    WriteLog("==========  發查失敗， TrnNum=" + strTrnNum + "RspCode = " + nodeCode.InnerText + "RspCode = " + nodeMess.InnerText);

                    // 更新CaseCustNewRFDMSend，為失敗
                    EditCaseCustRFDMSend(strTrnNum, "03", nodeCode.InnerText, nodeMess.InnerText);
                    _result = false;
                    // 依據案件狀態
                    if (int.Parse(strQueryStatus) >= 6)
                    {
                        //  將version檔的RFDM發查狀態更新為【重查失敗】
                        EditCaseCustDetailsErr(strVersionNewID, "08", sRspMsg);
                    }
                    else
                    {
                        //  將version檔的RFDM發查狀態更新為【失敗】
                        EditCaseCustDetailsErr(strVersionNewID, "04", sRspMsg);
                    }
                }
            }
            catch (Exception ex)
            {
                _result = false;
                WriteLog(" ========== AnalyticResponXMl Has Error, Error is " + ex.ToString());
            }

            return _result;
        }

        /// <summary>
        /// 取得查詢中的來文檔案
        /// </summary>
        /// <returns></returns>
        public DataTable GetWarningHistorySend()
        {
            try
            {
                string sqlSelect = @"select [NewID],[DocNo],[CustAccount], CONVERT(varchar(10), [ForCDateS], 111) as [ForCDateS], 
CONVERT(varchar(10), [ForCDateE], 111) as [ForCDateE],TrnNum,CustId,AcctDesc,Curr,Channel from WarningQueryHistory where Esbstatus is NULL";

                return base.Search(sqlSelect);
            }
            catch (Exception ex)
            {
                WriteLog("\t 取得查詢中的來文檔案 失敗, 錯誤訊息" + ex.Message.ToString());
                return null;
            }
        }


        /// <summary>
        /// 取得查詢中的來文檔案
        /// </summary>
        /// <returns></returns>
        public DataTable GetCaseCustRFDMSend()
        {

            try
            {
                // 要改成讀取CaseCustNewRFDMSend .. 以兔跟線上投單的程式打架....
                // 2021-11-24 ... 記... 要HTG打完後, 才能打ESB ... 所以先加上...  AND CaseCustDetails.HTGSendStatus='04' 的條件...
                // 2021-11-24 ... 記... 因為Type 3, Type 4沒有給身份證字號, 所以在查以上SQL時, 會造成ESB 沒有身分字號而失敗..
                // 初步想法是當HTG打完時, 回補CaseCustNewRFDMSend.ID_NO 的 Typ3, 4 的CUST_ID_NO, 即可....


                string sqlSelect = @"SELECT CaseCustNewRFDMSend.TrnNum ,
                                        CaseCustNewRFDMSend.VersionNewID ,
                                        CaseCustNewRFDMSend.ID_No ,
                                        CaseCustNewRFDMSend.Acct_No ,
                                        CONVERT(VARCHAR(8),CaseCustNewRFDMSend.Start_Jnrst_Date ,112) AS  Start_Jnrst_Date,
                                        CONVERT(VARCHAR(8),CaseCustNewRFDMSend.End_Jnrst_Date ,112) AS  End_Jnrst_Date,
                                        CaseCustNewRFDMSend.Type ,
                                        CaseCustNewRFDMSend.AcctDesc ,
                                        CaseCustNewRFDMSend.RFDMSendStatus ,
                                        CaseCustNewRFDMSend.RspCode ,
                                        CaseCustNewRFDMSend.RspMsg ,
                                        CaseCustMaster.Status AS QueryStatus,
                                        CaseCustNewRFDMSend.FileName
                                    FROM    CaseCustNewRFDMSend
                                    INNER JOIN CaseCustDetails
	                                    ON CaseCustNewRFDMSend.VersionNewID = CaseCustDetails.Detailsid
                                    INNER JOIN CaseCustMaster
	                                    ON CaseCustDetails.CaseCustMasterId = CaseCustMaster.NewId
                                    WHERE   CaseCustNewRFDMSend.RFDMSendStatus = '02'   AND CaseCustDetails.HTGSendStatus='4' ";

                base.Parameter.Clear();

                return base.Search(sqlSelect);
            }
            catch (Exception ex)
            {
                WriteLog("\t 取得查詢中的來文檔案 失敗 , 錯誤訊息" + ex.Message.ToString());
                return null;
            }
        }

        /// <summary>
        ///  更新來文檔案
        /// </summary>
        /// <param name="strTrnNum">交易序號</param>
        /// <param name="strRFDMSendStatus">RFDM查詢狀態</param>
        /// <param name="strRspCode">發查結果</param>
        /// <param name="strRspMsg">錯誤信息</param>
        /// <returns></returns>
        public bool EditCaseCustRFDMSend(string strTrnNum, string strRFDMSendStatus, string strRspCode, string strRspMsg)
        {
            try
            {
                string sql = @"UPDATE [CaseCustNewRFDMSend] 
                            SET 
                                [RFDMSendStatus] = @RFDMSendStatus,
                                [RspCode] = @RspCode,
                                [RspMsg] = @RspMsg,
                                [ModifiedDate] = GETDATE(), 
                                [ModifiedUser] = 'SYSTEM' 
                            WHERE TrnNum = @TrnNum";

                Parameter.Clear();
                Parameter.Add(new CommandParameter("@TrnNum", strTrnNum));
                Parameter.Add(new CommandParameter("@RFDMSendStatus", strRFDMSendStatus));
                Parameter.Add(new CommandParameter("@RspCode", strRspCode));
                Parameter.Add(new CommandParameter("@RspMsg", strRspMsg));

                return ExecuteNonQuery(sql) > 0;
            }
            catch (Exception ex)
            {
                WriteLog("\t 更新來文檔案 失敗, TrnNum = " + strTrnNum + "錯誤訊息" + ex.Message.ToString());
                return false;
            }
        }

        /// <summary>
        /// 更新來文檔案查詢條件帳號檔
        /// </summary>
        /// <param name="strTrnNum">流水號，主鍵</param>
        /// <param name="strRFDMSendStatus">發查狀態</param>
        /// <param name="strRFDMQryMessage">RFDM狀態原因</param>
        /// <returns></returns>
        public bool EditCaseCustDetails(string strstrVersionNewID, string strRFDMSendStatus, string strRFDMQryMessage)
        {
            try
            {
                string sql = @"UPDATE [CaseCustDetails] 
                            SET 
                                [RFDMSendStatus] = @RFDMSendStatus,
                                [RFDMQryMessage] = @RFDMQryMessage,
                                [ModifiedDate] = GETDATE(), 
                                [ModifiedUser] = 'SYSTEM' 
                            WHERE DetailsId = @NewID";
                Parameter.Clear();
                Parameter.Add(new CommandParameter("@NewID", strstrVersionNewID));
                Parameter.Add(new CommandParameter("@RFDMSendStatus", strRFDMSendStatus));
                Parameter.Add(new CommandParameter("@RFDMQryMessage", strRFDMQryMessage));
                return ExecuteNonQuery(sql) > 0;
            }
            catch (Exception ex)
            {
                WriteLog("\t 更新來文檔案查詢條件帳號檔 失敗, DetailsID = " + strstrVersionNewID + "錯誤訊息" + ex.Message.ToString());
                return false;
            }
        }


        /// <summary>
        /// 更新案件無相關資料, 所以可以產檔...
        /// </summary>
        /// <param name="strTrnNum">流水號，主鍵</param>
        /// <param name="strRFDMSendStatus">發查狀態</param>
        /// <param name="strRFDMQryMessage">RFDM狀態原因</param>
        /// <returns></returns>
        public bool EditCaseCustDetailsExcel_file(string strstrVersionNewID, string excel_file)
        {
            try
            {
                string sql = @"UPDATE [CaseCustDetails] 
                            SET 
                                [EXCEL_FILE] = @excel_file,
                                [ModifiedDate] = GETDATE(), 
                                [ModifiedUser] = 'SYSTEM' 
                            WHERE DetailsId = @NewID";
                Parameter.Clear();
                Parameter.Add(new CommandParameter("@NewID", strstrVersionNewID));
                Parameter.Add(new CommandParameter("@excel_file", excel_file));
                return ExecuteNonQuery(sql) > 0;
            }
            catch (Exception ex)
            {
                WriteLog("\t 更新CaseCustDetails.EXCEL_FILE 失敗, DetailsID = " + strstrVersionNewID + "錯誤訊息" + ex.Message.ToString());
                return false;
            }
        }


        /// <summary>
        /// 更新來文檔案查詢條件帳號檔
        /// </summary>
        /// <param name="strTrnNum">流水號，主鍵</param>
        /// <param name="strRFDMSendStatus">發查狀態</param>
        /// <param name="strRFDMQryMessage">RFDM狀態原因</param>
        /// <returns></returns>
        public bool EditCaseCustDetailsErr(string strstrVersionNewID, string strStatus, string strRFDMQryMessage)
        {
            try
            {
                string sql = @"UPDATE [CaseCustDetails] 
                            SET 
                                [RFDMSendStatus] = '6',
                                [Status] = @Status,
                                [RFDMQryMessage] = @RFDMQryMessage,
                                [ModifiedDate] = GETDATE(), 
                                [ModifiedUser] = 'SYSTEM' 
                            WHERE DetailsId = @NewID;
 UPDATE CaseCustMaster SET Status = @Status WHERE NewID IN (SELECT CaseCustMasterId FROM CaseCustDetails WHERE NewID = @NewID)

";
                Parameter.Clear();
                Parameter.Add(new CommandParameter("@NewID", strstrVersionNewID));
                Parameter.Add(new CommandParameter("@Status", strStatus));
                Parameter.Add(new CommandParameter("@RFDMQryMessage", strRFDMQryMessage));

                return ExecuteNonQuery(sql) > 0;
            }
            catch (Exception ex)
            {
                WriteLog("\t 更新來文檔案查詢條件帳號檔 失敗, DetailsID = " + strstrVersionNewID + "錯誤訊息" + ex.Message.ToString());
                return false;
            }
        }

        /// <summary>
        /// 發送上行XML,並取得下行XML
        /// </summary>
        /// <param name="TrnNum">Transaction ID</param>
        /// <param name="strXML">上行XML</param>
        /// <param name="txtCode">ESB</param>
        /// <returns></returns>
        public string SendESBData(string TrnNum, string strXML, string txtCode)
        {
            string strResult = string.Empty;

            // 隊列有關設定
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
                path += "/" + txtCode + "U" + TrnNum + "_" + DateTime.Now.ToString("yyyyMMddhhmmssfff") + ".xml";
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
                using (StreamWriter sw = new StreamWriter(new FileStream(path, FileMode.Create)))
                {
                    sw.Write(strXML);
                }
                #endregion

                // 記錄上行XML
                WriteLog(txtCode + "\r\n" + strXML + "\r\n ------------------------------------------------------------\r\n\r\n");

                // 隊列有關設定
                ServerUrl = ConfigurationManager.AppSettings["ServerUrl"].ToString();
                ServerPort = ConfigurationManager.AppSettings["ServerPort"].ToString();
                UserName = ConfigurationManager.AppSettings["UserName"].ToString();
                Password = ConfigurationManager.AppSettings["Password"].ToString();
                ESBSendQueueName = ConfigurationManager.AppSettings["ESBSendQueueName"].ToString();
                ESBReceiveQueueName = ConfigurationManager.AppSettings["ESBReceiveQueueName"].ToString();
                ServerPortStandBy = ConfigurationManager.AppSettings["ServerPortStandBy"].ToString();

                // 鏈接隊列
                strResult = ConnESB(ServerUrl, ServerPort, UserName, Password, ESBSendQueueName, ESBReceiveQueueName, strXML, ref msgNull);

                //  若無回應，則再次呼叫
                if (msgNull)
                {
                    strResult = ConnESB(ServerUrl, ServerPortStandBy, UserName, Password, ESBSendQueueName, ESBReceiveQueueName, strXML, ref msgNull);
                }

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
                path += "/" + txtCode + "D" + TrnNum + "_" + DateTime.Now.ToString("yyyyMMddhhmmssfff") + ".xml";
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

        /// <summary>
        /// 連接到ESB 隊列方式
        /// </summary>
        /// <param name="ServerUrl"></param>
        /// <param name="ServerPort"></param>
        /// <param name="UserName"></param>
        /// <param name="Password"></param>
        /// <param name="ESBSendQueueName">發送隊列</param>
        /// <param name="ESBReceiveQueueName">接收隊列</param>
        /// <param name="strXML">上行XML</param>
        /// <param name="msgNull"></param>
        /// <returns></returns>
        public string ConnESB(string ServerUrl, string ServerPort, string UserName, string Password, string ESBSendQueueName, string ESBReceiveQueueName, string strXML, ref bool msgNull)
        {
            msgNull = false;
            string strResult = string.Empty;
            string _serverurl = string.Empty;
            string _messageid = string.Empty;
            // 20221104, 改成讀參數檔.."ESBConnectionTimeout "
            double _TransactionTimeout = 45; // 預設45秒, 比ESB 預設40秒要長....

            PARMCodeBIZ pBiz = new PARMCodeBIZ();
            List<PARMCode> ret = pBiz.GetParmCodeByCodeType("ESBConnectionTimeout").ToList();


            if (ret.Count() > 0)
            {
                _TransactionTimeout = double.Parse(ret.First().CodeNo);
            }


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
                message.ReplyTo = (TIBCO.EMS.Destination)session.CreateQueue(ESBReceiveQueueName);

                qsender.Send(message);

                _messageid = message.MessageID;

                //receive message
                String messageselector = null;
                messageselector = "JMSCorrelationID = '" + _messageid + "'";

                TIBCO.EMS.Queue receivequeue = session.CreateQueue(ESBReceiveQueueName);

                QueueReceiver receiver = session.CreateReceiver(receivequeue, messageselector);

                connection.Start();

                //set up timeout 
                WriteLog("發查ESB時間"+ DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff"));
                TIBCO.EMS.Message msg = receiver.Receive((long)_TransactionTimeout * 1000);
                WriteLog("接收ESB時間"+ DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff"));
                
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
                        TextMessage tm = (TextMessage)msg;
                        strResult = tm.Text;
                    }
                    else
                    {
                        strResult = msg.ToString();
                    }
                }
                connection.Close();
            }
            else
            {
                string tmpupfile = AppDomain.CurrentDomain.BaseDirectory + "RFDM發查_Recv.xml";
                strResult = File.ReadAllText(tmpupfile);
            }
            #endregion

            return strResult;
        }




        /// <summary>
        /// LOG 記錄
        /// </summary>
        /// <param name="msg"></param>
        /// 
        public void WriteLog2(string msg)
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory + "Log2";
            if (Directory.Exists(baseDir) == false)
            {
                Directory.CreateDirectory(baseDir);
            }
            string filename = DateTime.Now.ToString("yyyyMMdd");
            lock (_lockLog)
            {

                System.IO.File.AppendAllText(baseDir + "\\" + filename + ".log", msg);

            }
        }
        //public void WriteLog2(string msg)
        //{
        //    if (Directory.Exists(@".\Log2") == false)
        //    {
        //        Directory.CreateDirectory(@".\Log2");
        //    }

        //    LogManager.Exists("DebugLog").Debug(msg);
        //    m_fileLog2.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------- " + msg);
        //}

        /// <summary>
        /// LOG 記錄
        /// </summary>
        /// <param name="msg"></param>
        public void WriteLog(string msg)
        {
            if (Directory.Exists(@".\Log") == false)
            {
                Directory.CreateDirectory(@".\Log");
            }

            LogManager.Exists("DebugLog").Debug(msg);
            m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------- " + msg);
        }

        /// <summary>
        /// 查詢CaseCustNewRFDMSend檔，狀態是否都為01和08，若是返回true，否則返回false
        /// </summary>
        /// <param name="strVersionNewID"></param>
        /// <returns></returns>
        public bool QueryRFDMSendByStatus(string strVersionNewID)
        {
            try
            {
                string sqlSelect = @" 
                                SELECT TrnNum 
                                FROM CaseCustNewRFDMSend 
                                WHERE VersionNewID = '" + strVersionNewID + @"'
                                    AND RFDMSendStatus <> '01' AND RFDMSendStatus <> '08' 
                                ";

                base.Parameter.Clear();

                DataTable dt = base.Search(sqlSelect);

                // 有其它狀態的資料
                if (dt != null && dt.Rows.Count > 0)
                {
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                WriteLog("\t 查詢CaseCustNewRFDMSend檔，狀態是否都為01和08 失敗, DetailsID = " + strVersionNewID + "錯誤訊息" + ex.Message.ToString());
                return false;
            }
        }

        /// <summary>
        /// 查詢案件下發成成功，但是沒有明細的總筆數
        /// </summary>
        /// <param name="strVersionNewID">案件編號</param>
        /// <returns>true：查詢3種類型明細都無明細C001，否則有其它狀況</returns>
        public bool QueryRFDMC001(string strVersionNewID)
        {
            try
            {
                // 20230109, 要考慮重號的狀況下, 故要先查詢本案件, 需要有多少個C01才會回無相關資料...
                string sqlSelectAll = @" 
                                SELECT COUNT(VersionNewID)  AS CountRFDMSendAll 
                                FROM CaseCustNewRFDMSend 
                                WHERE VersionNewID = '" + strVersionNewID + @"' 
                                ";

                base.Parameter.Clear();

                DataTable dtAll = base.Search(sqlSelectAll);


                // 查詢案件下要發查RFDM的總資料筆數
                string sqlSelect = @" 
                                SELECT COUNT(VersionNewID)  AS CountRFDMSend 
                                FROM CaseCustNewRFDMSend 
                                WHERE VersionNewID = '" + strVersionNewID + @"' AND CaseCustNewRFDMSend.RspCode = 'C001'
                                ";

                base.Parameter.Clear();

                DataTable dt = base.Search(sqlSelect);



                // 有其它狀態的資料
                // 20230109, 要考慮重號的狀況下, 就不能像原來的寫死為3了..
                if (dt != null && dt.Rows.Count > 0 && dt.Rows[0]["CountRFDMSend"].ToString().Trim() == dtAll.Rows[0]["CountRFDMSendAll"].ToString().Trim())
                {
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                WriteLog("\t 查詢案件下發成成功，但是沒有明細的總筆數 失敗, DetailsID = " + strVersionNewID + "錯誤訊息" + ex.Message.ToString());
                return false;
            }
        }

        public bool QueryRFDMStatus08(string strVersionNewID)
        {
            try
            {
                // 20230109, 要考慮重號的狀況下, 故要先查詢本案件, 需要有多少個08
                string sqlSelectAll = @" 
                                SELECT COUNT(VersionNewID)  AS CountRFDMSendAll 
                                FROM CaseCustNewRFDMSend 
                                WHERE VersionNewID = '" + strVersionNewID + @"' 
                                ";

                base.Parameter.Clear();

                DataTable dtAll = base.Search(sqlSelectAll);

                string sqlSelect = @" 
                                SELECT COUNT(VersionNewID) AS CountRFDMSend 
                                FROM CaseCustNewRFDMSend 
                                WHERE VersionNewID = '" + strVersionNewID + @"'
                                    AND RFDMSendStatus = '08' 
                                ";

                base.Parameter.Clear();

                DataTable dt = base.Search(sqlSelect);

                // 有其它狀態的資料
                // 20230109, 要考慮重號的狀況下, 就不能像原來的寫死為3了..
                if (dt != null && dt.Rows.Count > 0 && dt.Rows[0]["CountRFDMSend"].ToString().Trim() == dtAll.Rows[0]["CountRFDMSendAll"].ToString().Trim())
                {
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                WriteLog("\t 檢查狀態碼08 失敗, DetailsID = " + strVersionNewID + "錯誤訊息" + ex.Message.ToString());
                return false;
            }
        }
    }
}
