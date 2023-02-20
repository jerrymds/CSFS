using CTBC.CSFS.BussinessLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using log4net;
using CTBC.CSFS.Models;
using System.Configuration;
using TIBCO.EMS;
using System.Xml;
using System.Data;
using CTBC.CSFS.Pattern;

namespace CTBC.WinExe.DeathBRCISettingESB
{
    public class BRCIBiz : CommonBIZ
    {
        private FileLog m_fileLog;
        private string strHerader1;
        private string strHerader2;
        private string strHerader3;
        private string strSourceID;
        public BRCIBiz()
        {
            m_fileLog = new FileLog(ConfigurationManager.AppSettings["filelog"], AppDomain.CurrentDomain.FriendlyName.Replace(".vshost", "").Replace(".exe", ""));            
            strHerader1 = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>";
            strHerader2 = "<ns0:ServiceEnvelope xmlns:ns0=\"http://ns.chinatrust.com.tw/XSD/CTCB/ESB/Message/EMF/ServiceEnvelope\">";
            strHerader3 = "<ns1:ServiceHeader xmlns:ns1=\"http://ns.chinatrust.com.tw/XSD/CTCB/ESB/Message/EMF/ServiceHeader\">";
            strSourceID = ConfigurationManager.AppSettings["SourceID"].ToString();
        }

        internal List<CaseDeadDetail> getDeathLists(string CaseNo)
        {
            

            //     CaseDeadDetail.67050_Status=1 or 2 and BRCI_Status=null 時  and CaseNo=@CaseNo , 就打...

            List<CaseDeadDetail> result = new List<CaseDeadDetail>();
            using (IDbConnection dbConnection = OpenConnection())
            {
                string sqlStr = string.Format( @"SELECT * FROM [CaseDeadDetail] WHERE (TX67050_STATUS='1' OR TX67050_STATUS='2') and BRCI_STATUS is null and CaseNo='{0}' ", CaseNo);
                result = base.SearchList<CaseDeadDetail>(sqlStr).ToList();
            };

            return result;
        }

        internal string GenerateRequestXml(CaseDeadDetail name, string AgentId, string AgentBranch, ref string strSno)
        {
            if (name == null) { return null; }

            try
            {
                string strYear = Convert.ToInt16(DateTime.Now.Year - 1911).ToString("000");
                string strMonth = Convert.ToInt16(DateTime.Now.Month).ToString("00");
                string strDay = Convert.ToInt16(DateTime.Now.Day).ToString("00");
                string strHour = System.DateTime.Now.ToString("HHmmssfff");
                strSno = "CSFS" + DateTime.Now.ToString("yyyyMMddHHmmssff");
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(strHerader1);
                sb.AppendLine(strHerader2);
                sb.AppendLine(strHerader3);
                sb.AppendLine("<ns1:StandardType>BSMF</ns1:StandardType>");
                sb.AppendLine("<ns1:StandardVersion>01</ns1:StandardVersion>");
                sb.AppendLine("<ns1:ServiceName>csCstmrDcsdNoteAdd</ns1:ServiceName>");
                sb.AppendLine("<ns1:ServiceVersion>01</ns1:ServiceVersion>");
                sb.AppendLine("<ns1:SourceID>" + strSourceID + "</ns1:SourceID>");
                sb.AppendLine("<ns1:TransactionID>" + strSno + "</ns1:TransactionID>");
                sb.AppendLine("<ns1:RqTimestamp>" + System.DateTime.Now.ToString("yyyy-MM-dd") + "T" + System.DateTime.Now.ToString("HH:mm:ss.ff") + "+08:00" + "</ns1:RqTimestamp>");
                sb.AppendLine("</ns1:ServiceHeader>");
                sb.AppendLine("<ns1:ServiceBody xmlns:ns1=\"http://ns.chinatrust.com.tw/XSD/CTCB/ESB/Message/EMF/ServiceBody\">");
                sb.AppendLine("<ns2:csCstmrDcsdNoteAddRq xmlns:ns2=\"http://ns.chinatrust.com.tw/XSD/CTCB/ESB/Message/BSMF/csCstmrDcsdNoteAddRq/01\">");
                sb.AppendLine("<ns2:REQHDR>");
                sb.AppendLine("<ns2:TrnNum>" + strSno + "</ns2:TrnNum>");
                sb.AppendLine("<ns2:SourceID>" + strSourceID + "</ns2:SourceID>");
                sb.AppendLine("<ns2:UserID></ns2:UserID>");
                sb.AppendLine("</ns2:REQHDR><ns2:REQBDY>");
                sb.AppendLine("<ns2:CustId>" + name.CDBC_ID + "</ns2:CustId>");
                sb.AppendLine("<ns2:UpdateUser>" + AgentId + "</ns2:UpdateUser>");
                sb.AppendLine("<ns2:UpdateBranch>" + AgentBranch + "</ns2:UpdateBranch>");
                sb.AppendLine("<ns2:DeadFlag>" + "Y" + "</ns2:DeadFlag>");
                sb.AppendLine("</ns2:REQBDY>");
                sb.AppendLine("</ns2:csCstmrDcsdNoteAddRq>");
                sb.AppendLine("</ns1:ServiceBody>");
                sb.AppendLine("</ns0:ServiceEnvelope>");

                return sb.ToString();
            }
            catch
            {
                return null;
            }
        }

        internal string SendESBData(CaseDeadDetail name, string TrnNum, string strXML, string txtCode)
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
                //WriteLog(txtCode + "\r\n" + strXML + "\r\n ------------------------------------------------------------\r\n\r\n");

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

                //WriteLog(txtCode + "D" + "\r\n" + strResult + "\r\n ------------------------------------------------------------\r\n\r\n");
            }

            return strResult;
        }

        internal string ParseResponXMl(CaseDeadDetail name, string TrnNum, string xmlString)
        {
            //bool _result = true;
            string sRspCode = "";
            string sRspMsg = "";
            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(xmlString);

                // xml中帶xmlns:，使用SelectNodes或SelectSingleNode去查找其下节点，需要添加对应的XmlNamespaceManager参数，才可以的
                XmlNamespaceManager m = new XmlNamespaceManager(xmlDoc.NameTable);
                m.AddNamespace("ns0", "http://ns.chinatrust.com.tw/XSD/CTCB/ESB/Message/EMF/ServiceEnvelope");
                m.AddNamespace("ns1", "http://ns.chinatrust.com.tw/XSD/CTCB/ESB/Message/EMF/ServiceBody");
                //m.AddNamespace("ns2", "http://ns.chinatrust.com.tw/XSD/CTCB/ESB/Message/BSMF/ceDemDepTrnCntInqRs/01");
                m.AddNamespace("ns2", "http://ns.chinatrust.com.tw/XSD/CTCB/ESB/Message/BSMF/csCstmrDcsdNoteAddRs/01");
                XmlNode node1 = xmlDoc.SelectSingleNode("ns0:ServiceEnvelope/ns1:ServiceBody/ns2:csCstmrDcsdNoteAddRs/ns2:RESHDR/ns2:RspCode", m);

                // 發查結果
                sRspCode = (node1 != null ? node1.InnerText : "");

                //  取得錯誤信息
                XmlNode oRspMsgNode = xmlDoc.SelectSingleNode("ns0:ServiceEnvelope/ns1:ServiceBody/ns2:csCstmrDcsdNoteAddRs/ns2:RESBDY/ns2:BDYREC/ns2:RspMsg", m);

                sRspMsg = (oRspMsgNode != null ? oRspMsgNode.InnerText : "");

            }
            catch (Exception ex)
            {
                //_result = false;
                WriteLog(" ========== Parse XML 錯誤, Error is " + ex.ToString());
                sRspCode = "4444";
                sRspMsg = ex.ToString().Length <= 450 ? ex.ToString() : ex.ToString().Substring(0, 450);                
            }

            return sRspCode+"|"+sRspMsg;
        }

        public void updateCaseDeadDetailBRCI(CaseDeadDetail name, string TrnNum, string BRCI_Status, string sRspMsg)
        {
            try
            {
                using (IDbConnection dbConnection = OpenConnection())
                {

                    string sql = @"Update CaseDeadDetail Set TrnNum = @TrnNum, BRCI_STATUS = @BRCI_STATUS, BRCI_Message = @BRCI_Message WHERE CaseDeadNewID=@CaseDeadNewID AND CDBC_ID=@CDBC_ID";
                    base.Parameter.Clear();
                    base.Parameter.Add(new CommandParameter("@TrnNum", TrnNum));
                    base.Parameter.Add(new CommandParameter("@BRCI_STATUS", BRCI_Status));
                    base.Parameter.Add(new CommandParameter("@BRCI_Message", sRspMsg));
                    base.Parameter.Add(new CommandParameter("@CaseDeadNewID", name.CaseDeadNewID));
                    base.Parameter.Add(new CommandParameter("@CDBC_ID", name.CDBC_ID));
                    base.ExecuteNonQuery(sql);
                }
            }
            catch (Exception ex)
            {
                WriteLog("\t\t更新 CaseDeadDetail 失敗 " + ex.Message.ToString());
            }
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
            int _TransactionTimeout = 30;
            string IsnotTest = ConfigurationManager.AppSettings["IsnotTest"].ToString();
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
                TIBCO.EMS.Message msg = receiver.Receive(_TransactionTimeout * 1000);

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



        public void WriteLog(string msg)
        {
            if (Directory.Exists(@".\Log") == false)
            {
                Directory.CreateDirectory(@".\Log");
            }

            LogManager.Exists("DebugLog").Debug(msg);
            m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------- " + msg);
        }

        internal List<string> getDeadVersion(string theday)
        {
            if (string.IsNullOrEmpty(theday))
                theday = DateTime.Now.ToString("yyyy-MM-dd");
            List<string> caseNos = new List<string>();
            string CaseNo = string.Empty;

            using (IDbConnection dbConnection = OpenConnection())
            {
                string sqlStr = string.Format(@"SELECT DocNo FROM [CaseDeadVersion] WHERE FORMAT(ModifiedDate, 'yyyy-MM-dd') = '{0}' group by DocNo", theday);
                var result2 = base.Search(sqlStr);
                if( result2.Rows.Count >0 )
                {
                    foreach(DataRow dr in result2.Rows)
                    {
                        caseNos.Add(dr[0].ToString());
                    }
                }
            };

            if (caseNos.Count==0)
            {
                WriteLog("今日 " + theday + "無批號進來! ");
                return null;
            }
            return caseNos;
        }
    }
}
