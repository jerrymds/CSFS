using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;
using System.Web;
using System.Xml;
using System.Collections;
using System.Data;


namespace CTCB.NUMS.Library.HTG
{

    public class HTGObject
    {

        #region 全域變數

        //timeout value
        public int TimeOut = 130000;
        //htg cookies value
        private string Cookies = "";
        //htg session key
        private string SessionKey = "";
        //htg return code
        private Hashtable _returncode;
        
        /// <summary>
        /// Htg and mainframe Return code/message
        /// </summary>
        public Hashtable ReturnCode
        {
            get { return _returncode; }
        }


        /// <summary>
        /// data of transaction
        /// </summary>
        public DataSet HtgDataSet
        {
            get { return htgdata; }
        }

        /// <summary>
        /// transaction log
        /// </summary>
        public string  MessageLog
        {
            get { return messagelog; }
        }

        #endregion 

        #region 私域變數

        //htg url
        private string _htgurl;
        //htg applicationId
        private string _applicationId = "";
        //login ladp id
        private string _userId = "";
        //login ladp pwd
        private string _passWord = "";
        //login racf id
        private string _racfId = "";
        //login racf pwd
        private string _racfPassWord = "";
        //login branch no 
        private string _branchNo = "";
        //is sing on lu0, default false
        private bool _islogonlu0 =false;
        int _maxresendtime = 20;

        //transaction data
        private DataSet htgdata;
        //transaction log
        private string messagelog;

        #endregion 

        #region Public Method

        /// <summary>
        /// htg login object
        /// </summary>
        /// <param name="HtgUrl">htg url</param>
        /// <param name="ApplicationId">htg applicationId</param>
        /// <param name="LdapId">login ladp id</param>
        /// <param name="LdapPwd">login ladp pwd</param>
        /// <param name="RacfId">login racf id</param>
        /// <param name="RacfPwd">login racf pwd</param>
        /// <param name="BranchNo">login branch no </param>
        public HTGObject
                        (
                        string HtgUrl,
                        string ApplicationId,
                        string LdapId,
                        string LdapPwd,
                        string RacfId,
                        string RacfPwd,
                        string BranchNo
                        )
        {
            _htgurl = HtgUrl;

            _applicationId = ApplicationId;

            //_islogonlu0 = IsSignLu0;

            _userId = LdapId;

            _passWord = LdapPwd;

            _racfId = RacfId;

            _racfPassWord = RacfPwd;

            _branchNo = BranchNo;

            //initial return code
            _returncode = new Hashtable();
            //htg return code 
            _returncode.Add("HtgReturnCode", "");
            //htg return message
            _returncode.Add("HtgMessage", "");
            //main frame return message
            _returncode.Add("HGExceptionMessage", "");

            htgdata = new DataSet();

            messagelog = "";

        }

        /// <summary>
        /// 電文執行
        /// </summary>
        /// <param name="txid">電文代號</param>
        /// <param name="htparm">傳送參數</param>
        /// <returns>true/false</returns>
        public bool QueryHtg(string txid, Hashtable htparm)
        {
            bool result = false;

            htgdata = new DataSet();
            messagelog = "";
            //string recxmldata = "";
            StringBuilder sblog = new StringBuilder();

            string sendstr = "";
            string recstr = "";

            string sessionexpstr = "";
            string sessionmessage = "";
            string sessionreturn = "";

            try
            {
                //1,getcookie
                Cookies = GetCookie();
                if (Cookies.Trim().Length == 0)
                    throw new Exception("No Cookie value!!");
                sblog.AppendLine(System.DateTime.Now.ToString() + " || " + "\r\nGet Cookie value:" + Cookies + "\r\n" );

                //2,getsessionid
                _islogonlu0 = CheckedIsSignLu0(txid);

                sendstr = GetEstablishSessionXml();
                sblog.AppendLine(System.DateTime.Now.ToString() + " || " + "Get Session key ,\r\nSend xml :" + sendstr + "\r\n");
                
                recstr = PostHttp(sendstr);

                sessionreturn = CheckSession(recstr, out SessionKey, out sessionmessage, out sessionexpstr);

                sblog.AppendLine(System.DateTime.Now.ToString() + " || " + "\r\nGet Session key:" + SessionKey + " ,\r\nReceive xml :" + recstr + "\r\n");


                if (sessionmessage != "700 SESSION 建立成功")
                {
                    throw new Exception("Error in EstablishSession message : " + sessionmessage + "\r\n" + "Exception :" + sessionexpstr);
                }
                //switch (sessionreturn)
                //{ 
                //    case "700":
                //        break;
                //    default:
                //        throw new Exception("Error in EstablishSession message : " + sessionmessage + "\r\n" + "Exception :" + sessionexpstr);
                //        break;

                //}

                //if (!sessionreturn.Trim().Equals("700"))
                //{
                //    throw new Exception("Error in EstablishSession message : " + sessionmessage + "\r\n" + "Exception :" + sessionexpstr);
                //}
                //if (sessionreturn == "700")
                //{
                //    //do nothing
                //}
                //else
                //{
                //    throw new Exception("Error in EstablishSession message : " + sessionmessage + "\r\n" + "Exception :" + sessionexpstr);
                //}

                try
                {
                    sendstr = "";
                    recstr = "";
                   
                    switch (txid)
                    {
                            //客制化電文查詢,此處為多頁電文查詢作業,其他若有多頁電文交易者請修改此處
                        case "060490":
                        case "062072":
                            TransactionByMultiPages(sendstr,txid, htparm, ref sblog, ref htgdata);
                            break;

                        default:
                            //預設的查法,適用於一來一往,單頁電文
                            
                            TransactionBySingle(txid, htparm, ref sblog, ref htgdata);
                            break;
                    }

                    result = true;

                }
                catch (Exception queryexe)
                {
                    sblog.AppendLine("Error in Transaction to htg , error :" + queryexe.ToString());
                    result = false;
                }

                sblog.AppendLine(System.DateTime.Now.ToString() + " || " + "close Session  ,\r\nSend xml :" + sendstr + "\r\n");

                sendstr = GetCloseSessionXml();

                recstr = PostHttp(sendstr,false); //20140805smallzhi

                sessionreturn = CheckSession(recstr, out SessionKey, out sessionmessage, out sessionexpstr);

                sblog.AppendLine(System.DateTime.Now.ToString() + " || " + "close Session :" + SessionKey + " ,\r\nReceive xml :" + recstr + "\r\n");

 
                if (sessionreturn != "702")
                {
                    throw new Exception("Error in close Session message : " + sessionmessage + "\r\n" + "Exception :" + sessionexpstr + "\r\n");
                }

            }
            catch (Exception exe)
            {
                result = false;

                messagelog += System.DateTime.Now.ToString() + " || " + exe.ToString();
            }

            messagelog += sblog.ToString();

            return result;

        }

        public bool CheckHtgLogin()
        {
            /*
                 * racf id 錯誤
                    +		["HtgMessage"]	"799 SESSION 建立失敗 (SIGN ON 失敗)"	
                    +		["HGExceptionMessage"]	"01 GN00找不到該USERID"	
                    +		["HtgReturnCode"]	"799"	                
                 * racf pwd 錯誤
                    +		["HtgMessage"]	"704 SESSION 建立失敗 (RACF Sign on 失敗)"	
                    +		["HGExceptionMessage"]	"密碼錯誤"	
                    +		["HtgReturnCode"]	"704"	   
             */
            bool result = false;

            htgdata = new DataSet();
            messagelog = "";
            //string recxmldata = "";
            StringBuilder sblog = new StringBuilder();

            string sendstr = "";
            string recstr = "";

            string sessionexpstr = "";
            string sessionmessage = "";
            string sessionreturn = "";

            try
            {
                //1,getcookie
                Cookies = GetCookie();
                if (Cookies.Trim().Length == 0)
                    throw new Exception("No Cookie value!!");
                sblog.AppendLine(System.DateTime.Now.ToString() + " || " + "\r\nGet Cookie value:" + Cookies + "\r\n");

                //2,getsessionid
                _islogonlu0 = (new HtgXmlPara()).CheckIsSignOnlu0();

                sendstr = GetEstablishSessionXml();
                sblog.AppendLine(System.DateTime.Now.ToString() + " || " + "Get Session key ,\r\nSend xml :" + sendstr + "\r\n");

                recstr = PostHttp(sendstr);

                sessionreturn = CheckSession(recstr, out SessionKey, out sessionmessage, out sessionexpstr);

                sblog.AppendLine(System.DateTime.Now.ToString() + " || " + "\r\nGet Session key:" + SessionKey + " ,\r\nReceive xml :" + recstr + "\r\n");


                if (sessionmessage != "700 SESSION 建立成功")
                {
                    throw new Exception("Error in EstablishSession message : " + sessionmessage + "\r\n" + "Exception :" + sessionexpstr);
                }

                sendstr = GetCloseSessionXml();

                recstr = PostHttp(sendstr,false); //20140805 smallzhi

                sessionreturn = CheckSession(recstr, out SessionKey, out sessionmessage, out sessionexpstr);

                sblog.AppendLine(System.DateTime.Now.ToString() + " || " + "close Session :" + SessionKey + " ,\r\nReceive xml :" + recstr + "\r\n");


                if (sessionreturn != "702")
                {
                    throw new Exception("Error in close Session message : " + sessionmessage + "\r\n" + "Exception :" + sessionexpstr + "\r\n");
                }
                result = true;

            }
            catch (Exception exe)
            {
                result = false;

                messagelog += System.DateTime.Now.ToString() + " || " + exe.ToString();
            }

            messagelog += sblog.ToString();

            return result;

        }


        /// <summary>
        /// test
        /// </summary>
        /// <param name="parmht"></param>
        public void test(Hashtable parmht)
        {
            string recxml;
            recxml = "";
            DataSet ds = new DataSet();
        }

        #endregion 

 
        #region Private Method

        /// <summary>
        /// 轉換接收到的xml 成data
        /// </summary>
        /// <param name="recxmldata">接收到的xml</param>
        /// <param name="txid">交易代號</param>
        /// <returns></returns>
        private DataSet GetRecXmlData(ArrayList recxmllist, string txid, out string msg)
        {
            DataSet ds = new DataSet();
            msg = "";
            string strnamespace = "CTCB.NUMS.Library.HTG.Htg" + txid;

            object child = Activator.CreateInstance(Type.GetType(strnamespace, true, true));
            ds = ((HtgXmlPara)child).TransferXmlToDataSet(recxmllist,out msg);

            return ds;
          
        }

        /// <summary>
        /// 取得close session xml
        /// </summary>
        /// <returns></returns>
        private string GetCloseSessionXml()
        {
            string result = "";
            result = (new HtgXmlPara()).CloseSessionXml();
            result = result.Replace("#sessionId#", SessionKey);
            return result;
        }

        ///// <summary>
        ///// 由上行電文xml templage 及傳入參數取出組合好的xml
        ///// </summary>
        ///// <param name="txid"></param>
        ///// <param name="htparm"></param>
        ///// <returns></returns>
        private string GetPostXml(string txid, Hashtable htparm)
        {
            string result = "";
            string strnamespace = "";


            try
            {
                if (txid.Length == 0)
                    throw new Exception("No transaction id !!");

                if (!htparm.ContainsKey("sessionId"))
                    htparm.Add("sessionId", SessionKey);

                strnamespace = "CTCB.NUMS.Library.HTG.Htg" + txid;
                //dynamic child = Activator.CreateInstance(Type.GetType(strnamespace, true, true));
                //result = child.GetPostTransactionXml(htparm);

                object child = Activator.CreateInstance(Type.GetType(strnamespace, true, true));
                result = ((HtgXmlPara)child).GetPostTransactionXml(htparm);


            }
            catch (Exception exe)
            {
                throw exe;
            }

            return result;

        }

        ///// <summary>
        ///// 保留function ,適用於以第一次電文的下行再發查第二次電文的xml 字串組合
        ///// </summary>
        ///// <param name="txid"></param>
        ///// <param name="postxml"></param>
        ///// <param name="htparm"></param>
        ///// <returns></returns>
        private string GetPostXml(string txid, string postxml, Hashtable htparm)
        {
            string result = "";
            string strnamespace = "";


            try
            {
                if (txid.Length == 0)
                    throw new Exception("No transaction id !!");

                if (!htparm.ContainsKey("sessionId"))
                    htparm.Add("sessionId", SessionKey);

                strnamespace = "CTCB.NUMS.Library.HTG.Htg" + txid;

                object child = Activator.CreateInstance(Type.GetType(strnamespace, true, true));
                result = ((HtgXmlPara)child).GetPostTransactionXml(postxml, htparm);
            }
            catch (Exception exe)
            {
                throw exe;
            }

            return result;
        }

        /// <summary>
        /// get xml of getting Establish 
        /// </summary>
        /// <returns></returns>
        private string GetEstablishSessionXml()
        {
            string result = "";
            result = (new HtgXmlPara()).EstablishSessionXml();
            result = result.Replace("#userId#", _userId);
            result = result.Replace("#passWord#", _passWord);
            result = result.Replace("#branchNo#", _branchNo);
            result = result.Replace("#racfId#", _racfId);
            result = result.Replace("#racfPassWord#", _racfPassWord);
            result = result.Replace("#signOnLU0#", (_islogonlu0 == true) ? "yes" : "no");
            result = result.Replace("#applicationId#", _applicationId);

            return result;
        }

        /// <summary>
        /// 檢查電文是否sign on lu0
        /// </summary>
        /// <param name="txid"></param>
        /// <returns></returns>
        private bool CheckedIsSignLu0(string txid)
        {
            bool result = false ;
            string strnamespace = "";


            try
            {
                if (txid.Length == 0)
                    throw new Exception("No transaction id !!");

                strnamespace = "CTCB.NUMS.Library.HTG.Htg" + txid;

                object child = Activator.CreateInstance(Type.GetType(strnamespace, true, true));
                result = ((HtgXmlPara)child).CheckIsSignOnlu0();


            }
            catch (Exception exe)
            {
                throw exe;
            }

            return result;
        }


        /// <summary>
        /// get cookie
        /// </summary>
        /// <returns></returns>
        private string GetCookie()
        {
            Uri _uri;
            HttpWebRequest req;
            HttpWebResponse rsp;
            Stream rspsm;
            string strcookie = "";

            string result = "";
            try
            {
                _uri = new Uri(_htgurl);
                req = (HttpWebRequest)WebRequest.Create(_uri);
                req.Timeout = TimeOut;
                req.Method = "POST";
                req.CookieContainer = new CookieContainer(); //20140925 smallzhi
                byte[] bytes = Encoding.UTF8.GetBytes("");

                req.ContentType = "text/plain";

                using (Stream reqStream = req.GetRequestStream())
                {
                    reqStream.Write(bytes, 0, bytes.Length);
                }

                rsp = (HttpWebResponse)req.GetResponse();
                rspsm = rsp.GetResponseStream();
                foreach (Cookie _c in rsp.Cookies)
                {//20140925 smallzhi
                    strcookie = strcookie + ((!string.IsNullOrEmpty(strcookie))?";":"") + string.Format("{0}={1}", _c.Name, _c.Value);
                    if (!string.IsNullOrEmpty(_c.Path))
                    {
                        strcookie = strcookie + ";" + string.Format("path={0}", _c.Path);
                    }
                }
                //strcookie = rsp.GetResponseHeader("Set-Cookie");
                //strcookie = strcookie.Substring(0, strcookie.LastIndexOf(@"path=/")) + @"path=/";
                result = strcookie;
                req.Abort();
                rsp.Close();
            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {

                req = null;
                rsp = null;
                _uri = null;
            }


            return result;
        }

        /// <summary>
        /// post data over http
        /// </summary>
        /// <param name="sendstr"></param>
        /// <returns></returns>
        /// 20140805 smallzhi add persistent
        private string PostHttp(string sendstr, bool persistent = true)
        {
            Uri _uri;
            HttpWebRequest req;
            HttpWebResponse rsp;
            Stream rspsm;
            string strResponse = "";

            string result = "";
            try
            {
                _uri = new Uri(_htgurl);
                req = (HttpWebRequest)WebRequest.Create(_uri);
                req.Timeout = TimeOut;
                req.Method = "POST";
                //if (!persistent) { req.KeepAlive = false; } //test} //20140925 
                byte[] bytes = Encoding.UTF8.GetBytes(sendstr);

                req.ContentType = "text/plain";

                if (Cookies.Length != 0)
                {
                    req.Headers.Set("Cookie", Cookies);
                }

                using (Stream reqStream = req.GetRequestStream())
                {
                    reqStream.Write(bytes, 0, bytes.Length);
                }

                rsp = (HttpWebResponse)req.GetResponse();
                rspsm = rsp.GetResponseStream();
                strResponse = GetResponseString(rspsm);
                result = strResponse;
                req.Abort();
                rsp.Close();
            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
                req = null;
                rsp = null;
                _uri = null;
            }


            return result;
        }

        /// <summary>
        /// 取得web 回應
        /// </summary>
        /// <param name="sm"></param>
        /// <returns></returns>
        private string GetResponseString(Stream sm)
        {
            List<byte> bytes = new List<byte>(10);

            try
            {
                int i = 0;

                while ((i = sm.ReadByte()) != -1)
                {
                    bytes.Add((byte)i);
                }
            }
            catch (System.IO.IOException e)
            {
                // 當作成功
            }
            catch (Exception e)
            {
                throw e;
            }

            sm.Close();

            return Encoding.UTF8.GetString(bytes.ToArray());
        }

        /// <summary>
        /// check htg session value
        /// </summary>
        /// <param name="strxml"></param>
        /// <param name="sessionkey"></param>
        /// <param name="message"></param>
        /// <param name="exp"></param>
        /// <returns></returns>
        private string CheckSession(string strxml, out string sessionkey, out string message, out string exp)
        {
            string result = "";
            sessionkey = "";
            message = "";
            exp = "";
            XmlDocument xmldoc = new XmlDocument();
            XmlNodeList header;
            XmlNodeList body;


            try
            {
                xmldoc.LoadXml(strxml);

                header = xmldoc.GetElementsByTagName("header");
                body = xmldoc.GetElementsByTagName("body");

                //判斷結果
                //XmlNodeList rc = xmldoc.GetElementsByTagName("rc");
                result = xmldoc.GetElementsByTagName("rc")[0].Attributes["value"].Value.Trim();
                message = xmldoc.GetElementsByTagName("msg")[0].Attributes["value"].Value.Trim();

                _returncode["HtgReturnCode"] = result;
                _returncode["HtgMessage"] = message;


                switch (result)
                {
                    case "700":
                        //建立session 成功
                        foreach (XmlElement xmle in xmldoc.GetElementsByTagName("data"))
                        {
                            if (xmle.GetAttribute("id") == "sessionId")
                            {
                                sessionkey = xmle.GetAttribute("value").Trim();
                                break;
                            }
                        }

                        break;
                    case "702":
                        //close session 成功
                        break;
                    default:

                        if (xmldoc.SelectSingleNode("/hostgateway/body/data[@id='HG_EXCEPTION_MSG']") != null)
                        {
                            exp = xmldoc.SelectSingleNode("/hostgateway/body/data[@id='HG_EXCEPTION_MSG']").Attributes["value"].Value;
                            _returncode["HGExceptionMessage"] = exp;
                        }
                

                        //foreach (XmlElement xmle in xmldoc.GetElementsByTagName("data"))
                        //{
                        //    if (xmle.GetAttribute("id") == "HG_EXCEPTION_MSG")
                        //    {
                        //        exp = xmle.GetAttribute("value").Trim();
                        //        break;
                        //    }
                        //}
                        break;
                }

            }
            catch (Exception exe)
            {
                //throw exe;
                result = "Err";
                message = "Exception in function 'CheckSession'";
                exp = exe.ToString();
            }


            return result;
        }

        /// <summary>
        /// single page transaction on htg 
        /// </summary>
        /// <param name="txid"></param>
        /// <param name="htparm"></param>
        /// <param name="sendstr"></param>
        /// <param name="recstr"></param>
        /// <param name="sblog"></param>
        /// <param name="htgdata"></param>
        private void TransactionBySingle(string txid, Hashtable htparm,  ref StringBuilder sblog, ref DataSet htgdata)
        {
            string transferdatamsg = "";
            try
            {
                ArrayList recstrlist = new ArrayList();
                string sendstr="";
                string recstr="";
                //get post xml data
                sendstr = GetPostXml(txid, htparm);
                sblog.AppendLine(System.DateTime.Now.ToString() + " || " + "query transaction  ,\r\nSend xml :" + sendstr);

                //receive xml data
                recstr = PostHttp(sendstr);
                sblog.AppendLine(System.DateTime.Now.ToString() + " || " + "query transaction  ,\r\nreceive xml :" + recstr);

                recstrlist.Add(recstr);
                htgdata = GetRecXmlData(recstrlist, txid, out transferdatamsg);

                if (transferdatamsg.Length > 0)
                {
                    sblog.AppendLine(System.DateTime.Now.ToString() + " || " + "\r\nTransfer Xml to Data :" + transferdatamsg);
                    _returncode["HGExceptionMessage"] = transferdatamsg;
                }
                    


            }
            catch (Exception exe)
            {
                _returncode["HGExceptionMessage"] = exe.Message;
                throw exe;
            }

        }

        /// <summary>
        /// multiple page transaction on htg 
        /// </summary>
        /// <param name="txid"></param>
        /// <param name="htparm"></param>
        /// <param name="sblog"></param>
        /// <param name="htgdata"></param>
        private void TransactionByMultiPages(string sendstr ,string txid, Hashtable htparm, ref StringBuilder sblog, ref DataSet htgdata)
        {
            string transferdatamsg = "";
            ArrayList recxmllist = new ArrayList();
            bool resend = true;

            int currentresendtime = 0;

            try
            {
                //string sendstr="";
                string recstr="";
                //get first page post xml data
                if (sendstr.Trim().Length == 0)
                {
                    sendstr = GetPostXml(txid, htparm);
                }
                else
                {
                    sendstr = GetPostXml(txid,sendstr, htparm);
                }
               

                sblog.AppendLine(System.DateTime.Now.ToString() + " || " + "first query transaction  ,\r\nSend xml :" + sendstr);

                //receive xml data
                recstr = PostHttp(sendstr);
                recxmllist.Add(recstr);
                sblog.AppendLine(System.DateTime.Now.ToString() + " || " + "first query transaction  ,\r\nreceive xml :" + recstr);

                //check whether it has next page 
                resend = CheckIsHasNextPage(txid,recstr);

                //Second time send 
                
                while (resend)
                {

                    if (currentresendtime == _maxresendtime)
                    {
                        //己達最大重送次數了
                        resend = false;
                        break;
                    }
                    //put last receive xml to tranfer into post xml
                    sendstr = GetPostXml(txid, recstr, htparm);
                    sblog.AppendLine(System.DateTime.Now.ToString() + " || " + "the " + (currentresendtime +1) + "times resend query transaction  ,\r\nSend xml :" + sendstr);

                    //receive xml data
                    recstr = PostHttp(sendstr);
                    recxmllist.Add(recstr);
                    sblog.AppendLine(System.DateTime.Now.ToString() + " || " + "the " + (currentresendtime + 1) + "times resend query transaction  ,\r\nreceive xml :" + recstr);

                    //傳送完加1次
                    currentresendtime += 1;

                    resend = CheckIsHasNextPage(txid,recstr);

                }


                htgdata = GetRecXmlData(recxmllist, txid, out transferdatamsg);
                if (transferdatamsg.Length > 0)
                    sblog.AppendLine(System.DateTime.Now.ToString() + " || " + "\r\nTransfer Xml to Data :" + transferdatamsg);

                switch (txid)
                { 
                    case "062072":
                        //執行是否要發查67073
                        DoSend067073(recxmllist, ref sblog, ref htgdata);

                        break;
                }

            }
            catch (Exception exe)
            {
                throw exe;
            }

        }


        /// <summary>
        /// customization function for 067073 after sending 06272
        /// </summary>
        /// <param name="recxmllist"></param>
        /// <param name="sblog"></param>
        /// <param name="htgdata"></param>
        private void DoSend067073(ArrayList recxmllist, ref  StringBuilder sblog, ref DataSet htgdata)
        {
            
            int currentpageno =0;
            string currentpageseqno = "";
            string currentsendstr ="";
            DataSet tx067073 =new DataSet();
            Hashtable htparm=new Hashtable();

            try
            {
                #region ,檢查是否要發067073
                //有facility值即為要發查
                DataRow[] _facilitydr = htgdata.Tables["dt067072Detail"].Select("FACILITY ='F'");
                if (_facilitydr.Length == 0)
                    return;

                foreach (DataRow _dr in _facilitydr)
                {

                    currentpageno = Convert.ToInt16(_dr["CurrentPageNo"].ToString());
                    currentpageseqno = _dr["CurrentPageSeqNo"].ToString();
                    currentsendstr = recxmllist[currentpageno-1].ToString();
                    tx067073 =new DataSet();
                    htparm = new Hashtable();
                    htparm.Add("SELECT", "0" + currentpageseqno);
                    htparm.Add("ACTION", " ");

                    //發查067073並取回資料
                    sblog.AppendLine(System.DateTime.Now.ToString() + " || " + "\r\nStart to Get 067073 ,CurretnPage="+ currentpageno.ToString() +", CurretnPageSeqNo=" + currentpageseqno );
                    TransactionByMultiPages(currentsendstr, "067073", htparm, ref sblog, ref tx067073);
 
                    #region 將067073的資料合併到67072的dataset之內
                    try
                    {
                         foreach (DataTable _dt in tx067073.Tables)
                        {
                            if (htgdata.Tables.Contains(_dt.TableName))
                            {htgdata.Tables[_dt.TableName].Merge(_dt);}
                            else
                            {htgdata.Tables.Add(_dt.Copy());}
                        }

                    }
                    catch (Exception ex3)
                    {
                        sblog.AppendLine(System.DateTime.Now.ToString() + " || " + "\r\n transfer   067073 xml to data err,Error:" + ex3.ToString());
                    }

                    #endregion 

                    sblog.AppendLine(System.DateTime.Now.ToString() + " || " + "\r\nFinished in  Getting 067073 ,CurretnPage="+ currentpageno.ToString() +", CurretnPageSeqNo=" + currentpageseqno );
                }

                #endregion 

            }
            catch (Exception exe)
            {
                sblog.AppendLine(System.DateTime.Now.ToString() + " || " + "\r\nError in  DoSend067073 :" + exe.ToString());
            }

        }

        /// <summary>
        /// check the response xml does has next page 
        /// </summary>
        /// <param name="txid"></param>
        /// <param name="recstr"></param>
        /// <returns></returns>
        private bool CheckIsHasNextPage(string txid,string recstr)
        {
            bool result =false;
            string strnamespace = "";


            try
            {
                strnamespace = "CTCB.NUMS.Library.HTG.Htg" + txid;
                object child = Activator.CreateInstance(Type.GetType(strnamespace, true, true));
                result = ((HtgXmlPara)child).CheckIsHasNextPage(recstr);
            }
            catch (Exception exe)
            {
                throw exe;
            }

            return result;
        }
        

        #endregion





    }
}
