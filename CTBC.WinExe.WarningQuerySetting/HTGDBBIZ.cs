using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CTBC.CSFS.BussinessLogic;
using CTBC.CSFS.Pattern;
using CTBC.FrameWork.Util;
using System.Data;
using System.Collections;
using System.Data.SqlClient;
using System.Configuration;
using System.IO;
using log4net;
using System.Security.Cryptography;
using System.Xml;
using CTBC.FrameWork.HTG;

namespace CTBC.WinExe.WarningQuerySetting
{
    public class HTGDBBIZ : BaseBusinessRule
    {
        string _ldapuid = "";
        string _ldappwd = "";
        string _racfuid = "";
        string _racfowd = "";
        string _racfbranch = "";
        string _htgurl = "";
        string _applicationid = "";

        /// <summary>
        /// 修改要發送電文主檔的發送狀態
        /// </summary>
        /// <param name="TableName"></param>
        /// <param name="SendStatus"></param>
        /// <param name="Applno"></param>
        /// <param name="ApplnoB"></param>
        /// <returns></returns>
        public int UpdateSendStatus(string TableName, string SendStatus, string Applno, string ApplnoB)
        {
            try
            {
                string sql = @"UPDATE  " + TableName + @"
                                            SET     SendStatus = @SendStatus ,
                                                    ModifiedDate=GETDATE()
                                            WHERE   ApplNo = @ApplNo
                                                    AND ApplNoB = @ApplNoB 
                                                     ";

                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@SendStatus", SendStatus));
                base.Parameter.Add(new CommandParameter("@ApplNo", Applno));
                base.Parameter.Add(new CommandParameter("@ApplNoB", ApplnoB));

                return base.ExecuteNonQuery(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        /// <summary>
        /// dr 的欄位..DocNo, DetailsId as VersionNewID, Status,CaseCustMasterId, QueryType, CustIdNo,QDateS,QDateE
        /// </summary>
        /// <param name="dr"></param>
        /// <returns></returns>
        public bool InsertOutputFormat(DataRow dr)
        {
            bool ret = true;
            DataTable dt = new DataTable();
            int formatNum = int.Parse(dr["QueryType"].ToString());
            string docNo = dr["DocNo"].ToString();

            switch (formatNum)
            {
                case 1:
                case 3:
                case 2:
                case 4:
                    dt = outputF1(dr);

                    //dt.Columns.Add("DocNo", typeof(string));
                    //foreach(DataRow dr2 in dt.Rows)
                    //{
                    //    dr2["DocNo"] = docNo;
                    //}

                    dt.TableName = "CaseCustOutputF1";
                    InsertIntoTableWithTransaction(dt, null);
                    break;

                //dt = outputF2(dr);
                //dt.TableName = "CaseCustOutputF2";
                //InsertIntoTableWithTransaction(dt, null);
                //break;
                case 5:
                    dt = null;
                    //dt.TableName = "CaseCustOutputF3";
                    //InsertIntoTableWithTransaction(dt, null);
                    break;
            }

            return ret;
        }


        /// <summary>
        /// 輸出格式 1   (即, Type 1, Type 3的資料格式)
        /// </summary>
        /// <param name="dr"></param>
        /// <returns></returns>
        public DataTable outputF1(DataRow dr)
        {
            DataTable dt = new DataTable();

            string caseid = dr["VersionNewID"].ToString();
            string docno = dr["DocNo"].ToString();
            Guid masterid = Guid.Parse(dr["CaseCustMasterId"].ToString());
            Guid detailsid = Guid.Parse(dr["VersionNewID"].ToString());


            #region 找出待儲存的資料格式, 目前是Type 1


            string dataSql = @" SELECT  @docNo 
                                    ,CaseCustDetails.CaseCustMasterId as MasterId
									,CaseCustDetails.DetailsId
                                	,BOPS000401Recv.CUST_ID_NO -- 統一編號
                                	,CASE 
                                        WHEN ISNULL(BOPS000401Recv.BRANCH_NO,'') <> '' THEN '822' + BOPS000401Recv.BRANCH_NO 
                                        ELSE ''
                                    END BRANCH_NO --分行別
                                	,BOPS000401Recv.PD_TYPE_DESC--產品別
                                	,BOPS000401Recv.CURRENCY --幣別
                                	,BOPS067050Recv.CUSTOMER_NAME --戶名
                                	,BOPS067050Recv.NIGTEL_NO --晚上電話
                                	,BOPS067050Recv.MOBIL_NO --手機號碼
                                	,isnull(BOPS067050Recv.POST_CODE,'') + ' ' 
                                	+isnull(BOPS067050Recv.COMM_ADDR1,'')
                                	+isnull(BOPS067050Recv.COMM_ADDR2,'')
                                	+isnull(BOPS067050Recv.COMM_ADDR3,'') as COMM_ADDR--通訊地址
                                	,isnull(BOPS067050Recv.ZIP_CODE,'') + ' ' + 
                                	isnull(BOPS067050Recv.CUST_ADD1,'') +
                                	isnull(BOPS067050Recv.CUST_ADD2,'') +
                                	isnull(BOPS067050Recv.CUST_ADD3,'') as CUST_ADD--戶籍/證照地址
									,CASE WHEN BOPS000401Recv.CURRENCY='TWD' THEN substring(BOPS000401Recv.ACCT_NO,LEN(BOPS000401Recv.ACCT_NO)-11 ,12)
									ELSE SUBSTRING(BOPS000401Recv.ACCT_NO,LEN(BOPS000401Recv.ACCT_NO)-14 ,12)  END as ACCT_NO --帳號
                                	-- ,BOPS000401Recv.ACCT_NO --帳號
                                    ,GETDATE() as HTGModifiedDate
                                	,CASE
                                      WHEN LEN(BOPS000401Recv.OPEN_DATE) > 0 
                                           and BOPS000401Recv.OPEN_DATE <>'00/00/0000'
                                      THEN substring(BOPS000401Recv.OPEN_DATE,7,4)
                                      + substring(BOPS000401Recv.OPEN_DATE,4, 2)
                                      +substring(BOPS000401Recv.OPEN_DATE,1, 2) ELSE '' END as OPEN_DATE --開戶日
                                	,CASE
                                		 WHEN LEN(BOPS000401Recv.CLOSE_DATE) > 0 
                                            and BOPS000401Recv.CLOSE_DATE <> '00/00/0000'
                                		 THEN substring(BOPS000401Recv.CLOSE_DATE,7,4)
                                		 + substring(BOPS000401Recv.CLOSE_DATE,4, 2)
                                		 +substring(BOPS000401Recv.CLOSE_DATE,1, 2) ELSE '' END as CLOSE_DATE --結清日
									,substring(BOPS000401Recv.CUR_BAL,LEN(BOPS000401Recv.CUR_BAL),1) + substring(BOPS000401Recv.CUR_BAL,1,LEN(BOPS000401Recv.CUR_BAL)-1) as CUR_BAL --目前餘額
                                	--,BOPS000401Recv.CUR_BAL --目前餘額
                                	,BOPS000401Recv.LAST_DATE  
                                FROM BOPS000401Recv
                                LEFT JOIN BOPS067050Recv
                                    ON BOPS000401Recv.VersionNewID = BOPS067050Recv.VersionNewID
                                    --AND BOPS000401Recv.ACCT_NO = BOPS067050Recv.CIF_NO  --待客戶環境測試
                                LEFT JOIN CaseCustDetails
                                 ON CaseCustDetails.DetailsId = BOPS000401Recv.VersionNewID
                                WHERE BOPS000401Recv.VersionNewID =@VersionNewID
                                order by CaseCustDetails.IdNo,BOPS000401Recv.CUST_ID_NO,OPEN_DATE ";

            dataSql = dataSql.Replace(" @docNo ", "'" + docno + "' AS DocNo");
            base.Parameter.Clear();
            base.Parameter.Add(new CommandParameter("@VersionNewID", caseid));

            dt = base.Search(dataSql);
            #endregion

            return dt;

        }



        public DataTable outputF2(DataRow dr)
        {





            return null;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="strSQL"></param>
        /// <returns></returns>
        public DataTable OpenDataTable(string strSQL)
        {
            DataTable returnValue = new DataTable();

            try
            {
                base.Parameter.Clear();
                returnValue = base.Search(strSQL);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return returnValue;
        }

        public int ExcuteSQL(string sql)
        {
            IDbConnection dbConnection = base.OpenConnection();
            IDbTransaction dbTransaction = null;
            using (dbConnection)
            {
                try
                {
                    // 開始事務
                    dbTransaction = dbConnection.BeginTransaction();

                    base.ExecuteNonQuery(sql, dbTransaction, false);

                    dbTransaction.Commit();
                }
                catch (Exception ex)
                {
                    dbTransaction.Rollback();

                    return 0;
                }
            }

            return 1;
        }

        /// <summary>
        /// Update DataRow
        /// </summary>
        /// <param name="tablename"></param>
        /// <param name="dr"></param>
        public void UpdateDataRow(string tablename, DataRow dr)
        {
            // 連接數據庫
            IDbConnection dbConnection = base.OpenConnection();
            IDbTransaction dbTransaction = null;
            string sql = "";

            if (tablename == "")
                throw new Exception("no TableName!!");

            // DB連接
            using (dbConnection)
            {
                try
                {
                    // 開始事務
                    dbTransaction = dbConnection.BeginTransaction();

                    CommandParameterCollection cmdparm = new CommandParameterCollection();
                    CreateUpdateSql(tablename, dr, ref sql, ref cmdparm);
                    base.Parameter.Clear();
                    foreach (CommandParameter cmp in cmdparm)
                    {
                        base.Parameter.Add(cmp);
                    }

                    base.ExecuteNonQuery(sql, dbTransaction, false);

                    dbTransaction.Commit();
                }
                catch (Exception ex)
                {
                    dbTransaction.Rollback();

                    throw ex;
                }
            }
        }

        /// <summary>
        /// create insert sql command
        /// </summary>
        /// <param name="strTableName"></param>
        /// <param name="dr"></param>
        /// <param name="strInsert"></param>
        /// <param name="cmdparm"></param>
        private void CreateUpdateSql(string strTableName, DataRow dr, ref string strInsert, ref CommandParameterCollection cmdparm)
        {
            //產生 Insert SQL
            string strUpdateCol = "";

            foreach (DataColumn clmData in dr.Table.Columns)
            {
                string strColumnName = clmData.ColumnName.ToUpper();

                switch (strColumnName)
                {
                    case "NEWID":
                        cmdparm.Add(new CommandParameter("@" + strColumnName, dr[strColumnName]));
                        break;
                    default:
                        strUpdateCol += "," + strColumnName + "=@" + strColumnName;
                        cmdparm.Add(new CommandParameter("@" + strColumnName, dr[strColumnName]));
                        break;
                }
            }

            strUpdateCol = strUpdateCol.Substring(1);

            //產生命令
            strInsert = "Update " + strTableName + " Set " + strUpdateCol + " Where NewId = @NewId";
        }

        /// <summary>
        /// 解密
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public string Decode(string data)
        {
            string KEY_64 = "VavicApp";
            string IV_64 = "VavicApp";

            byte[] byKey = System.Text.ASCIIEncoding.ASCII.GetBytes(KEY_64);
            byte[] byIV = System.Text.ASCIIEncoding.ASCII.GetBytes(IV_64);
            byte[] byEnc;

            try
            {
                byEnc = Convert.FromBase64String(data);
            }
            catch
            {
                return null;
            }

            DESCryptoServiceProvider cryptoProvider = new DESCryptoServiceProvider();
            MemoryStream ms = new MemoryStream(byEnc);
            CryptoStream cst = new CryptoStream(ms, cryptoProvider.CreateDecryptor(byKey, byIV), CryptoStreamMode.Read);
            StreamReader sr = new StreamReader(cst);

            return sr.ReadToEnd();
        }

        /// <summary>
        /// //20220207, 先判斷這個RACF帳密/是否過期? 若過期, 則寫LOG, 並停止執行...以免被鎖住...
        /// </summary>
        /// <param name="VersionNewID"></param>
        /// <returns></returns>
        public bool checkRacf(string VersionNewID)
        {
            DataTable dtHtgData = OpenDataTable("select * from ApprMsgKey where VersionNewID = '" + VersionNewID + "'");

            if (dtHtgData.Rows.Count <= 0)
            {
                WriteLog("DetailsId = [" + VersionNewID + "] 無發查資料！");

                return false;
            }

            _ldapuid = Decode(dtHtgData.Rows[0]["MsgKeyLU"].ToString());
            _ldappwd = Decode(dtHtgData.Rows[0]["MsgKeyLP"].ToString());
            _racfuid = Decode(dtHtgData.Rows[0]["MsgKeyRU"].ToString());
            _racfowd = Decode(dtHtgData.Rows[0]["MsgKeyRP"].ToString());
            _racfbranch = Decode(dtHtgData.Rows[0]["MsgKeyRB"].ToString());

            _htgurl = ConfigurationManager.AppSettings["HTGUrl"];
            _applicationid = ConfigurationManager.AppSettings["HTGApplication"];



            //20220207, 先判斷這個RACF帳密/是否過期? 若過期, 則寫LOG, 並停止執行...以免被鎖住...
            var objSeiHTG = new CTBC.FrameWork.HTG.ExecuteHTG(_ldapuid, _ldappwd, _racfuid, _racfowd, _racfbranch);
            var testResut = objSeiHTG.HTGInitialize(_ldapuid, _ldappwd, _racfuid, _racfowd, _racfbranch);
            if (!testResut)
            {
                string InitErrorMessage = objSeiHTG.initErrorMessage;
                WriteLog("\r\n============初始化錯誤===================\r\n");
                WriteLog("DetailsId = [" + VersionNewID + "] 發生初始化錯誤" + InitErrorMessage);
                WriteLog("\r\n============初始化錯誤===================\r\n");
                return false;
            }
            else
                return true;
        }

        /// <summary>
        /// 發送電文
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="txid"></param>
        /// <param name="drApprMsgDefine"></param>
        /// <param name="applNo"></param>
        /// <param name="applNoB"></param>
        /// <returns></returns>
        public bool SendMsgCase(CTCB.NUMS.Library.HTG.HTGObjectMsg obj, string txid, DataRow drApprMsgDefine, string VersionNewID, DataTable CurrencyList, int seq, string CustAccount)
        {

            //  測試
            string IsnotTest = ConfigurationManager.AppSettings["IsnotTest"].ToString();

            bool bRet = true;

            if (txid == "009091")
                bRet = true;



            // log
            WriteLog("DetailsId = [" + VersionNewID + "], TxID=[" + txid + "]");

            try
            {

                // 撈取HTG參數
                DataTable dtHtgData = OpenDataTable("select * from ApprMsgKey where VersionNewID = '" + VersionNewID + "'");

                if (dtHtgData.Rows.Count <= 0)
                {
                    WriteLog("DetailsId = [" + VersionNewID + "], TxID=[" + txid + "],無發查資料！");

                    return false;
                }

                _ldapuid = Decode(dtHtgData.Rows[0]["MsgKeyLU"].ToString());
                _ldappwd = Decode(dtHtgData.Rows[0]["MsgKeyLP"].ToString());
                _racfuid = Decode(dtHtgData.Rows[0]["MsgKeyRU"].ToString());
                _racfowd = Decode(dtHtgData.Rows[0]["MsgKeyRP"].ToString());
                _racfbranch = Decode(dtHtgData.Rows[0]["MsgKeyRB"].ToString());


                if (ConfigurationManager.AppSettings["racfPassword"] != null)
                    _racfowd = ConfigurationManager.AppSettings["racfPassword"].ToString();

                _htgurl = ConfigurationManager.AppSettings["HTGUrl"];
                _applicationid = ConfigurationManager.AppSettings["HTGApplication"];



                WriteLog("撈取下行儲存TABLE結構！");

                // 撈取下行儲存TABLE結構
                DataTable dtRecvData = OpenDataTable("select * from BOPS" + txid + "Recv where 1=2");
                //DataTable dtRecvData = new DataTable();
                //dtRecvData.Columns.Add("NEWID", typeof(Guid));
                //dtRecvData.Columns.Add("VERSIONNEWID", typeof(Guid));
                //dtRecvData.Columns.Add("RESPONSE_OC08", typeof(String));
                //dtRecvData.Columns.Add("CURRENCY_OC08", typeof(String));
                //dtRecvData.Columns.Add("CMTRASH_OC08", typeof(String));
                //dtRecvData.Columns.Add("DESCRIP_OC08", typeof(String)); 
                //dtRecvData.Columns.Add("outputCode", typeof(String));
                //dtRecvData.Columns.Add("SendNewID", typeof(Guid));



                // 撈取電文資料
                string strPostTransactionDataSql = drApprMsgDefine["PostTransactionDataSql"].ToString();

                strPostTransactionDataSql = strPostTransactionDataSql.Replace("@VersionNewID", "'" + VersionNewID + "'");

                DataTable dtData = OpenDataTable(strPostTransactionDataSql);

                // 一筆一筆發電文
                for (int n = 0; n < dtData.Rows.Count; n++)
                {
                    WriteLog("Case = [" + n.ToString() + "] begin QueryHtg");

                    //執行過程的log
                    string messagelog = "";

                    //執行交易後所得到的資料
                    DataSet htgdata = new DataSet();
                    Hashtable htparm = new Hashtable();
                    Hashtable htreturn = new Hashtable();

                    //查詢htg 
                    if (obj == null)
                    {
                        obj = new CTCB.NUMS.Library.HTG.HTGObjectMsg();
                    }

                    obj.Init(drApprMsgDefine, dtRecvData, _htgurl, _applicationid, _ldapuid, _ldappwd, _racfuid, _racfowd, _racfbranch);

                    // 前1個欄位為資料定位欄位
                    string strFilter = " 1=1 ";
                    for (int i = 0; i < 1; i++)
                    {
                        strFilter += " and " + dtData.Columns[i].ColumnName + "='" + dtData.Rows[n][dtData.Columns[i].ColumnName].ToString().Trim() + "' ";
                    }

                    // 從第2個欄位開始為上行電文資料欄位
                    for (int i = 1; i < dtData.Columns.Count; i++)
                    {
                        //發送電文的參數,其參數名稱須與上送電文欄位名稱一致
                        htparm.Add(dtData.Columns[i].ColumnName, dtData.Rows[n][dtData.Columns[i].ColumnName].ToString());
                    }

                    WriteLog("DetailsId = [" + VersionNewID + "], TxID=[" + txid + "],開始發查");



                    bool result = false;


                    // 濟南測試，沒有辦法發查
                    if (IsnotTest == "1")
                    {
                        result = true;
                    }
                    else
                    {
                        //查詢htg 
                        result = obj.QueryHtg(txid, htparm);
                    }
                    htreturn = obj.ReturnCode;


                    // 20220211, 發現, 打67050時, 會提示.. "客戶尚未簽定存款開戶總約定書  "
                    if (txid == "067050")
                    {
                        // 20220215, 由obj.htgdataSet 來判斷是否有資料, 若無資料, 則判斷為失敗...
                        htgdata = obj.HtgDataSet;
                        if (htgdata.Tables[0].Rows.Count == 0)
                        {
                            result = false;
                        }
                    }
                    if (txid == "060628")
                    {
                        // 20220215, 由obj.htgdataSet 來判斷是否有資料, 若無資料, 則判斷為失敗...
                        htgdata = obj.HtgDataSet;
                        if (htgdata.Tables[0].Rows.Count == 0)
                        {
                            result = false;
                        }
                    }

                    // 20220211, 發現有可能出現... "提示代碼:0169 提示原因:檢查碼錯誤 "
                    // 代表本身帳號就不符合正確的規範.. 直接落失敗...
                    if (txid == "000401" && seq == 0)
                    {
                        // 20220215, 由obj.htgdataSet 來判斷是否有資料, 若無資料, 則判斷為失敗...

                        if (txid == "000401" && (htreturn["PopMessage"].ToString().Contains("無此帳戶") || htreturn["PopMessage"].ToString().Contains("交易幣別不一致")))
                        {
                            // 換下一個幣別....
                            // 取得目前要發查的幣別, 從1開始...
                            for (int i = 1; i < CurrencyList.Rows.Count; i++)
                            {
                                string strCurrency = CurrencyList.Rows[i][1].ToString();
                                htparm["CURRENCY"] = strCurrency;
                                var res401 = obj.QueryHtg(txid, htparm);
                                result = res401;
                                if (!res401) //若出現Exception 則直接Break;
                                    break;
                                htreturn = obj.ReturnCode;
                                // 如果沒有出現"無此帳戶", 代表幣別正確, 則Break;
                                if (!htreturn["PopMessage"].ToString().Contains("無此帳戶") && !htreturn["PopMessage"].ToString().Contains("交易幣別不一致"))
                                {
                                    break;
                                }
                            }
                            htgdata = obj.HtgDataSet;
                            if (htgdata.Tables[0].Rows.Count == 0)
                            {
                                result = false;
                                // 刪除掉由啟動發查產生的BOPS067050Send, BOPS060628Send的那筆..
                                var ret = deleteBOPS67050_60628(VersionNewID);
                            }

                        }
                    }

                    else
                    {
                        // 20211028.... 
                        // 若是幣別錯誤.. 會在 obj.ReturnCode["PopMessage"] = "提示代碼:0108 提示原因:無此帳戶 "
                        if (txid == "000401" && (htreturn["PopMessage"].ToString().Contains("無此帳戶") || htreturn["PopMessage"].ToString().Contains("交易幣別不一致")))
                        {
                            // 換下一個幣別....
                            // 取得目前要發查的幣別, 從1開始...
                            for (int i = 1; i < CurrencyList.Rows.Count; i++)
                            {
                                string strCurrency = CurrencyList.Rows[i][1].ToString();
                                htparm["CURRENCY"] = strCurrency;
                                var res401 = obj.QueryHtg(txid, htparm);
                                result = res401;
                                if (!res401) //若出現Exception 則直接Break;
                                    break;
                                htreturn = obj.ReturnCode;
                                // 如果沒有出現"無此帳戶", 代表幣別正確, 則Break;
                                if (!htreturn["PopMessage"].ToString().Contains("無此帳戶") && !htreturn["PopMessage"].ToString().Contains("交易幣別不一致"))
                                {
                                    break;
                                }
                            }
                        }

                    }



                    //執行log
                    messagelog = obj.MessageLog;

                    WriteLog("VersionNewID = [" + VersionNewID + "]" + messagelog);

                    // 電文發查失敗
                    if (!result)
                    {
                        #region 電文發查失敗
                        string strMsg = htreturn["PopMessage"].ToString();

                        if (strMsg == null || strMsg == "")
                        {
                            strMsg = "呼叫QueryHtg方法發查失敗";
                        }

                        DataTable dtTemp = new DataTable();
                        dtTemp.Columns.Add("NewID");
                        dtTemp.Columns.Add("SendStatus");
                        dtTemp.Columns.Add("QueryErrMsg");
                        dtTemp.Columns.Add("ModifiedDate");

                        DataRow drSend = dtTemp.NewRow();
                        drSend["NewID"] = dtData.Rows[n]["NewID"].ToString();
                        drSend["QueryErrMsg"] = strMsg;
                        drSend["ModifiedDate"] = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");

                        WriteLog("VersionNewID = [" + VersionNewID + "], TxID=[" + txid + "],更新Table: " + "BOPS" + txid + "Send");

                        /*
                         * 當HTG下行電文訊息有包含下列代碼時，是歸屬於正常狀況。
                         * 1.電文(67050) : 0188 - 找不到相關資料
                         * 2.電文(67028) : 0188 - 找不到相關資料
                         */
                        if (strMsg.Trim().Contains("0188"))
                        {
                            drSend["SendStatus"] = "01";

                            UpdateDataRow("BOPS" + txid + "Send", drSend);
                        }
                        else
                        {
                            drSend["SendStatus"] = "03";
                            UpdateDataRow("BOPS" + txid + "Send", drSend);

                            // 將CaseCustQueryVersion表狀態更新為異常
                            //EditCaseCustQueryVersion(VersionNewID, "6", strMsg);
                            bRet = false;

                            WriteLog("VersionNewID = [" + VersionNewID + "], TxID=[" + txid + "],發查失敗，信息：" + strMsg);
                        }
                        #endregion
                    }
                    // 電文發查成功
                    else
                    {
                        //  測試用，直接讀取本地XML
                        if (IsnotTest == "1")
                        {
                            #region 測試

                            string c = "";
                            string xmlName = "";

                            switch (txid)
                            {
                                case "067050":
                                    xmlName = "067050_Receive.xml";
                                    break;
                                case "060628":
                                    xmlName = "060628_Receive.xml";
                                    break;
                                case "060490":
                                    xmlName = "060490_Receive.xml";
                                    break;
                                case "000401":
                                    xmlName = "000401_Receive.xml";
                                    break;
                                case "081019":
                                    xmlName = "081019_Recv.xml";
                                    break;
                            }


                            htgdata = getDataSetByXMLUser(txid, xmlName, out c);

                            htreturn["PopMessage"] = c;

                            #endregion
                        }
                        else
                        {
                            //電文資料集
                            htgdata = obj.HtgDataSet;
                        }

                        string strMsg = htreturn["PopMessage"].ToString();
                        string strReturn = "";

                        if (htgdata.Tables[0].Rows.Count > 0)
                        {
                            try
                            {
                                #region 如果發送的是490電文，則check此電文下recv和401檔是否有冗餘資料。需要先清乾淨。

                                if (txid == "060490")
                                {
                                    // 刪除490Recv
                                    DeleteHTGBy60490SendID(dtData.Rows[n]["NewID"].ToString());
                                }

                                #endregion

                                #region 將主機返回資料insert到Recv檔

                                DataTable dt = htgdata.Tables[0];
                                dt.TableName = "BOPS" + txid + "Recv";

                                if (!dt.Columns.Contains("VersionNewID")) dt.Columns.Add("VersionNewID");
                                if (!dt.Columns.Contains("SendNewID")) dt.Columns.Add("SendNewID");

                                for (int i = 0; i < dt.Rows.Count; i++)
                                {
                                    dt.Rows[i]["VersionNewID"] = VersionNewID;
                                    dt.Rows[i]["SendNewID"] = dtData.Rows[n]["NewID"].ToString();
                                }

                                WriteLog("VersionNewID = [" + VersionNewID + "], TxID=[" + txid + "],資料寫入Table: " + dt.TableName);

                                InsertIntoTable(dt);

                                #endregion

                                #region 依據Recv檔的資料重新新增其它 send Table檔



                                // 備註因為先打Type 3, Type 4 時, 要先打401, ==> 找到 身份證字號, 才能把Web 界面產生的BOPS0067050 及 BOPS0060628 押上身份證字號.. 
                                // 因為有這段程式

                                if (seq == 0 && txid == "000401")
                                {
                                    #region 若第一支是打401, 則... 要扣60750 及60628的 押回401所剛剛取得的 身份證字號


                                    // 先取得401產出的結果...
                                    try
                                    {
                                        string get401Recv = "select * from BOPS000401Recv where VersionNewID='" + VersionNewID.ToString() + "';";
                                        var Recv401 = base.Search(get401Recv);


                                        if (Recv401 != null)
                                        {

                                            string Cust_ID_No = Recv401.Rows[0]["CUST_ID_NO"].ToString();

                                            string update67050 = "UPDATE BOPS067050Send SET CustIdNo = @Cust_ID_No WHERE VersionNewID= @VersionNewID;";
                                            base.Parameter.Clear();
                                            base.Parameter.Add(new CommandParameter("@Cust_ID_No", Cust_ID_No));
                                            base.Parameter.Add(new CommandParameter("@VersionNewID", VersionNewID));
                                            base.ExecuteNonQuery(update67050);
                                            string update60628 = "UPDATE BOPS060628Send SET CustIdNo = @Cust_ID_No WHERE VersionNewID= @VersionNewID;";
                                            base.Parameter.Clear();
                                            base.Parameter.Add(new CommandParameter("@Cust_ID_No", Cust_ID_No));
                                            base.Parameter.Add(new CommandParameter("@VersionNewID", VersionNewID));
                                            base.ExecuteNonQuery(update60628);

                                            // 還要把第一次打401的那一筆, 刪除, 才不會輸出結果時, 多一筆重覆的....

                                            string del401Recv = "delete from BOPS000401Recv where VersionNewID= @VersionNewID;";
                                            base.Parameter.Clear();
                                            base.Parameter.Add(new CommandParameter("@VersionNewID", VersionNewID));
                                            base.ExecuteNonQuery(del401Recv);

                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        WriteLog("在401回傳結果中, 找不到身份證字號    VersionNewID = [" + VersionNewID + "], TxID=[" + txid + "],發查失敗！");
                                    }
                                    #endregion

                                }



                                //// 060628 發查成功，將
                                //if (txid == "060628")
                                //{
                                //    Insert60628Send(VersionNewID, dt, txid, dtData.Rows[n]["NewID"].ToString());
                                //}


                                // 20221213, 集作說, 不要把60628, 因此, 要從67050, 直接跳到Insert 60490Send ...
                                if (txid == "067050")
                                {
                                    Insert60490Send(VersionNewID, dt, txid, dtData.Rows[n]["NewID"].ToString());
                                } 
                                else if (txid == "060490")
                                {
                                    //insert BOPS000401Send （ACCOUNT_1_1 ~ ACCOUNT_1_6）
                                    Insert000401Send(VersionNewID, dt, txid, dtData.Rows[n]["NewID"].ToString(), CurrencyList, _racfbranch, _racfuid.Substring(1, _racfuid.Length - 1));

                                    // insert BOPS009091Send ( 因為要連同401一樣的電文...)
                                    //2022-05-25, 要排除通報的那個帳號... 

                                    Insert9091_13Send(VersionNewID, dt, txid, dtData.Rows[n]["NewID"].ToString(), CustAccount, CurrencyList, _racfbranch, _racfuid.Substring(1, _racfuid.Length - 1));


                                    if (dt.Columns.Contains("ACCOUNT_6") && dt.Rows[0]["ACCOUNT_6"].ToString().Trim() != "" && dt.Rows[0]["ACCOUNT_6"].ToString().Trim() != "00000000000000000")
                                    {
                                        // 查詢491的電文設定檔
                                        DataTable dtDefineBy60491 = GetApprMsgDefineBy60491();

                                        if (dtDefineBy60491 != null && dtDefineBy60491.Rows.Count > 0)
                                        {
                                            strReturn = Send60491Start(obj, dtDefineBy60491.Rows[0], VersionNewID, dtData.Rows[n]["NewID"].ToString(), CurrencyList);
                                        }
                                    }
                                }

                                #endregion


                                #region 若是打67050-4, 要回寫WarningMaster.ForeignId
                                //20220624, 

                                if (seq == 0 && txid == "067050V4")
                                {
                                    // 外國證號....
                                    string fid = dt.Rows[0]["ID_NO_03"].ToString();
                                    string u674 = "SELECT Top 1 CaseId from WarningDetails where NewID='" + VersionNewID + "'";
                                    var res674 = OpenDataTable(u674);
                                    if (res674.Rows.Count > 0)
                                    {
                                        string masterCaseId = res674.Rows[0][0].ToString();
                                        UpdateWarningMasterForeignId(masterCaseId, fid);
                                    }
                                }


                                #endregion


                                #region 更新WarningMaster後面四個欄位, 生日, 電話, 行動, 地址...
                                if (txid == "067050")
                                {
                                    string u674 = "SELECT Top 1 CaseId from WarningDetails where NewID='" + VersionNewID + "'";
                                    var res674 = OpenDataTable(u674);
                                    if (res674.Rows.Count > 0)
                                    {
                                        DataRow dr5 = dt.Rows[0];
                                        string masterCaseId = res674.Rows[0][0].ToString();
                                        UpdateWarningMasterBasicInfo(masterCaseId, dr5);
                                    }

                                }
                                #endregion

                                if (strReturn != "ERROR")
                                {
                                    #region 更新Version和send檔狀態為成功

                                    DataTable dtTemp = new DataTable();
                                    dtTemp.Columns.Add("NewID");
                                    dtTemp.Columns.Add("SendStatus");
                                    dtTemp.Columns.Add("QueryErrMsg");
                                    dtTemp.Columns.Add("ModifiedDate");
                                    DataRow drSend = dtTemp.NewRow();
                                    drSend["NewID"] = dtData.Rows[n]["NewID"].ToString();
                                    drSend["SendStatus"] = "01";
                                    drSend["QueryErrMsg"] = strMsg;
                                    drSend["ModifiedDate"] = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");

                                    WriteLog("VersionNewID = [" + VersionNewID + "], TxID=[" + txid + "],更新Table: " + "BOPS" + txid + "Send");

                                    UpdateDataRow("BOPS" + txid + "Send", drSend);

                                    #endregion
                                }


                                WriteLog("VersionNewID = [" + VersionNewID + "], TxID=[" + txid + "],發查成功！" + strMsg);
                            }
                            catch (Exception ex)
                            {
                                #region MyRegion
                                strMsg = "QueryHtg Exception = [" + ex.Message + "]";

                                DataTable dtTemp = new DataTable();
                                dtTemp.Columns.Add("NewID");
                                dtTemp.Columns.Add("SendStatus");
                                dtTemp.Columns.Add("QueryErrMsg");
                                dtTemp.Columns.Add("ModifiedDate");
                                DataRow drSend = dtTemp.NewRow();
                                drSend["NewID"] = dtData.Rows[n]["NewID"].ToString();
                                drSend["SendStatus"] = "03";
                                drSend["QueryErrMsg"] = strMsg;
                                drSend["ModifiedDate"] = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");

                                WriteLog("VersionNewID = [" + VersionNewID + "], TxID=[" + txid + "],更新Table: " + "BOPS" + txid + "Send");

                                UpdateDataRow("BOPS" + txid + "Send", drSend);

                                bRet = false;

                                // 將CaseCustQueryVersion表狀態更新為異常
                                //EditCaseCustQueryVersion(VersionNewID, "6", strMsg);

                                WriteLog("VersionNewID = [" + VersionNewID + "], TxID=[" + txid + "],發查失敗，信息：" + strMsg);
                                #endregion
                            }

                            //receivedata = "查詢成功!";
                            WriteLog("QueryHtg success");
                        }
                        else
                        {
                            DataTable dtTemp = new DataTable();
                            dtTemp.Columns.Add("NewID");
                            dtTemp.Columns.Add("SendStatus");
                            dtTemp.Columns.Add("QueryErrMsg");
                            dtTemp.Columns.Add("ModifiedDate");

                            DataRow drSend = dtTemp.NewRow();
                            drSend["NewID"] = dtData.Rows[n]["NewID"].ToString();
                            drSend["QueryErrMsg"] = strMsg;
                            drSend["ModifiedDate"] = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");

                            WriteLog("VersionNewID = [" + VersionNewID + "], TxID=[" + txid + "],更新Table: " + "BOPS" + txid + "Send");

                            /*
                             * 當HTG下行電文訊息有包含下列代碼時，是歸屬於正常狀況。
                             * 1.電文(67050) : 0188 - 找不到相關資料
                             * 2.電文(67028) : 0188 - 找不到相關資料
                             */
                            #region MyRegion
                            if (strMsg.Trim().Contains("0188"))
                            {
                                drSend["SendStatus"] = "01";

                                UpdateDataRow("BOPS" + txid + "Send", drSend);
                            }
                            else
                            {
                                drSend["SendStatus"] = "03";

                                UpdateDataRow("BOPS" + txid + "Send", drSend);

                                // 將CaseCustQueryVersion表狀態更新為異常
                                //EditCaseCustQueryVersion(VersionNewID, "6", strMsg);

                                bRet = false;

                                WriteLog("VersionNewID = [" + VersionNewID + "], TxID=[" + txid + "],發查失敗，信息：" + strMsg);
                            }
                            #endregion
                        }
                    }
                }

                return bRet;
            }
            catch (Exception exp)
            {
                bRet = false;
                WriteLog("SendMsgCase Exception = [" + exp.Message + "]");
            }

            return bRet;
        }


        public bool deleteBOPS67050_60628(string VersionNewId)
        {
            bool ret = false;

            var uSql = @"delete from  BOPS067050Send where VersionNewID = '" + VersionNewId + "';delete  from  BOPS060628Send where  VersionNewID = '" + VersionNewId + "' ";

            var uSqlResult = ExcuteSQL(uSql);


            return uSqlResult > 0 ? true : false;

        }

        public string Send60491Start(CTCB.NUMS.Library.HTG.HTGObjectMsg obj, DataRow drApprMsgDefine, string VersionNewID, string NewID060490, DataTable CurrencyList)
        {
            string strReturn = Send60491MsgCase(obj, drApprMsgDefine, VersionNewID, NewID060490, CurrencyList);
            if (strReturn == "Next")
            {
                Send60491Start(obj, drApprMsgDefine, VersionNewID, NewID060490, CurrencyList);
            }

            return strReturn;
        }

        /// <summary>
        /// 發送60491電文
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="txid"></param>
        /// <param name="drApprMsgDefine"></param>
        /// <param name="VersionNewID"></param>
        /// <returns>ErrorSetting:設定錯誤；Error:發查失敗；Next：需要發查下一筆 </returns>
        public string Send60491MsgCase(CTCB.NUMS.Library.HTG.HTGObjectMsg obj, DataRow drApprMsgDefine, string VersionNewID, string NewID060490, DataTable CurrencyList)
        {
            string bRet = "";

            // log
            WriteLog("Send60491MsgCase：VersionNewID = [" + VersionNewID + "], TxID=[060491]");

            try
            {
                // 撈取HTG參數
                DataTable dtHtgData = OpenDataTable("select * from ApprMsgKey where VersionNewID = '" + VersionNewID + "'");

                if (dtHtgData.Rows.Count <= 0)
                {
                    WriteLog("Send60491MsgCase：VersionNewID = [" + VersionNewID + "], TxID=[060491],無發查資料！");

                    return "ErrorSetting";
                }

                _ldapuid = Decode(dtHtgData.Rows[0]["MsgKeyLU"].ToString());
                _ldappwd = Decode(dtHtgData.Rows[0]["MsgKeyLP"].ToString());
                _racfuid = Decode(dtHtgData.Rows[0]["MsgKeyRU"].ToString());
                _racfowd = Decode(dtHtgData.Rows[0]["MsgKeyRP"].ToString());
                _racfbranch = Decode(dtHtgData.Rows[0]["MsgKeyRB"].ToString());

                _htgurl = ConfigurationManager.AppSettings["HTGUrl"];
                _applicationid = ConfigurationManager.AppSettings["HTGApplication"];

                WriteLog("Send60491MsgCase：撈取下行儲存TABLE結構！");

                // 撈取下行儲存TABLE結構
                DataTable dtRecvData = OpenDataTable("select * from BOPS060490Recv where 1=2");

                // 撈取電文資料
                string strPostTransactionDataSql = drApprMsgDefine["PostTransactionDataSql"].ToString();

                strPostTransactionDataSql = strPostTransactionDataSql.Replace("@VersionNewID", "'" + VersionNewID + "'");

                DataTable dtData = OpenDataTable(strPostTransactionDataSql);

                //--------------------執行發查動作------------------------------------------------------

                //執行交易後所得到的資料
                DataSet htgdata = new DataSet();
                Hashtable htparm = new Hashtable();
                Hashtable htreturn = new Hashtable();

                //查詢htg 
                if (obj == null)
                {
                    obj = new CTCB.NUMS.Library.HTG.HTGObjectMsg();
                }

                obj.Init(drApprMsgDefine, dtRecvData, _htgurl, _applicationid, _ldapuid, _ldappwd, _racfuid, _racfowd, _racfbranch);

                // 前1個欄位為資料定位欄位
                string strFilter = " 1=1 ";
                for (int i = 0; i < 1; i++)
                {
                    strFilter += " and " + dtData.Columns[i].ColumnName + "='" + dtData.Rows[0][dtData.Columns[i].ColumnName].ToString().Trim() + "' ";
                }

                // 從第2個欄位開始為上行電文資料欄位
                for (int i = 1; i < dtData.Columns.Count; i++)
                {
                    //發送電文的參數,其參數名稱須與上送電文欄位名稱一致
                    htparm.Add(dtData.Columns[i].ColumnName, dtData.Rows[0][dtData.Columns[i].ColumnName].ToString());
                }

                WriteLog("Send60491MsgCase：VersionNewID = [" + VersionNewID + "], TxID=[060491],開始發查");

                bool result = false;

                //  測試
                string IsnotTest = ConfigurationManager.AppSettings["IsnotTest"].ToString();

                // 濟南測試，沒有辦法發查
                if (IsnotTest == "1")
                {
                    result = true;
                }
                else
                {
                    //查詢htg
                    result = obj.QueryHtg("060491", htparm);
                }

                htreturn = obj.ReturnCode;

                WriteLog("Send60491MsgCase：VersionNewID = [" + VersionNewID + "]" + obj.MessageLog);

                // 電文發查失敗
                if (!result)
                {
                    string strMsg = htreturn["PopMessage"].ToString();

                    if (strMsg == null || strMsg == "")
                    {
                        strMsg = "呼叫QueryHtg方法發查失敗";
                    }

                    // 當HTG下行電文訊息的代碼為'1204'-查無資料時，是歸屬於正常狀況。
                    if (strMsg.Trim().Contains("1204"))
                    {
                        return "CLOSE";
                    }
                    else
                    {
                        DataTable dtTemp = new DataTable();
                        dtTemp.Columns.Add("NewID");
                        dtTemp.Columns.Add("SendStatus");
                        dtTemp.Columns.Add("QueryErrMsg");
                        dtTemp.Columns.Add("ModifiedDate");
                        DataRow drSend = dtTemp.NewRow();
                        drSend["NewID"] = NewID060490;
                        drSend["SendStatus"] = "03";
                        drSend["QueryErrMsg"] = strMsg;
                        drSend["ModifiedDate"] = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                        UpdateDataRow("BOPS060490Send", drSend);

                        // 將CaseCustQueryVersion表狀態更新為異常
                        //EditCaseCustQueryVersion(VersionNewID, "6", strMsg);
                        bRet = "ERROR";
                    }

                    WriteLog("Send60491MsgCase：VersionNewID = [" + VersionNewID + "], TxID=[060491],發查失敗，信息：" + strMsg);
                }
                // 電文發查成功
                else
                {
                    //  測試用，直接讀取本地XML
                    if (IsnotTest == "1")
                    {
                        #region 測試

                        string c = "";

                        htgdata = getDataSetByXMLUser("060490", "60491_Receive_Page3.xml", out c);

                        htreturn["PopMessage"] = c;

                        #endregion
                    }
                    else
                    {
                        //電文資料集
                        htgdata = obj.HtgDataSet;
                    }

                    string strMsg = htreturn["PopMessage"].ToString();

                    // 當HTG下行電文訊息的代碼為'1204'-查無資料時，是歸屬於正常狀況。
                    if (strMsg.Trim().Contains("1204"))
                    {
                        return "CLOSE";
                    }

                    if (htgdata.Tables[0].Rows.Count > 0)
                    {
                        try
                        {
                            DataTable dt = htgdata.Tables[0];
                            dt.TableName = "BOPS060490Recv";

                            if (!dt.Columns.Contains("VersionNewID")) dt.Columns.Add("VersionNewID");

                            for (int i = 0; i < dt.Rows.Count; i++)
                            {
                                dt.Rows[i]["VersionNewID"] = VersionNewID;
                            }

                            WriteLog("Send60491MsgCase：VersionNewID = [" + VersionNewID + "], TxID=[060491],資料寫入Table: " + dt.TableName);

                            InsertIntoTable(dt);

                            //insert BOPS000401Send （ACCOUNT_1_1 ~ ACCOUNT_1_6）
                            Insert000401Send(VersionNewID, dt, "060491", NewID060490, CurrencyList, _racfbranch, _racfuid.Substring(1, _racfuid.Length - 1));

                            WriteLog("Send60491MsgCase：VersionNewID = [" + VersionNewID + "], TxID=[060491],發查成功！" + strMsg);

                            // ACCOUNT_6<> 00000000000000000  and  ACCOUNT_6<>  '', 則代表需要再次發查
                            if (dt.Columns.Contains("ACCOUNT_6") && dt.Rows[0]["ACCOUNT_6"].ToString().Trim() != "" && dt.Rows[0]["ACCOUNT_6"].ToString().Trim() != "00000000000000000")
                            {
                                return "Next";
                            }
                            else
                            {
                                return "CLOSE";
                            }
                        }
                        catch (Exception ex)
                        {
                            strMsg = "QueryHtg Exception = [" + ex.Message + "]";

                            DataTable dtTemp = new DataTable();
                            dtTemp.Columns.Add("NewID");
                            dtTemp.Columns.Add("SendStatus");
                            dtTemp.Columns.Add("QueryErrMsg");
                            dtTemp.Columns.Add("ModifiedDate");
                            DataRow drSend = dtTemp.NewRow();
                            drSend["NewID"] = NewID060490;
                            drSend["SendStatus"] = "03";
                            drSend["QueryErrMsg"] = strMsg;
                            drSend["ModifiedDate"] = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                            UpdateDataRow("BOPS060490Send", drSend);

                            // 將CaseCustQueryVersion表狀態更新為異常
                            //EditCaseCustQueryVersion(VersionNewID, "6", strMsg);
                            bRet = "ERROR";

                            WriteLog("Send60491MsgCase：VersionNewID = [" + VersionNewID + "], TxID=[060491],發查失敗，信息：" + strMsg);
                        }
                    }
                    else
                    {
                        DataTable dtTemp = new DataTable();
                        dtTemp.Columns.Add("NewID");
                        dtTemp.Columns.Add("SendStatus");
                        dtTemp.Columns.Add("QueryErrMsg");
                        dtTemp.Columns.Add("ModifiedDate");
                        DataRow drSend = dtTemp.NewRow();
                        drSend["NewID"] = NewID060490;
                        drSend["SendStatus"] = "03";
                        drSend["QueryErrMsg"] = strMsg;
                        drSend["ModifiedDate"] = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                        UpdateDataRow("BOPS060490Send", drSend);

                        // 將CaseCustQueryVersion表狀態更新為異常
                        //EditCaseCustQueryVersion(VersionNewID, "6", strMsg);
                        bRet = "ERROR";

                        WriteLog("Send60491MsgCase：VersionNewID = [" + VersionNewID + "], TxID=[060491],發查失敗，信息：" + strMsg);
                    }
                }
            }
            catch (Exception exp)
            {
                bRet = "ERROR";
                WriteLog("Send60491MsgCase：SendMsgCase Exception = [" + exp.Message + "]");
            }

            return bRet;
        }

        public DataTable GetApprMsgDefineBy60491()
        {
            try
            {
                DataTable dt = new DataTable();
                string sql = "SELECT  * FROM ApprMsgDefine WHERE MsgName = '060491'";
                dt = OpenDataTable(sql);
                return dt;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void InsertIntoTable(DataTable rtable, IDbTransaction dbTransaction = null)
        {
            if (dbTransaction != null)
            {
                InsertIntoTableWithTransaction(rtable, dbTransaction);
                return;
            }

            bool innertransaction = false;
            IDbConnection dbConnection;
            if (dbTransaction == null)
            {
                innertransaction = true;
                dbConnection = base.OpenConnection();
            }
            else
            {
                dbConnection = dbTransaction.Connection;
            }

            if (rtable.TableName.Trim() == "" || rtable.TableName.ToUpper() == "TABLE")
                throw new Exception("DataTable has no TableName!!");

            if (rtable.Columns.Contains("NewId"))
            {
                rtable.Columns.Remove("NewId");
            }

            using (dbConnection)
            {
                try
                {
                    // 開始事務
                    if (innertransaction == true)
                        dbTransaction = dbConnection.BeginTransaction();

                    using (SqlBulkCopy bulkinsert = new SqlBulkCopy((SqlConnection)dbConnection, SqlBulkCopyOptions.FireTriggers, (SqlTransaction)dbTransaction))
                    {
                        bulkinsert.BatchSize = 1000;
                        bulkinsert.BulkCopyTimeout = 60;
                        bulkinsert.DestinationTableName = rtable.TableName;

                        var arrayNames = (from DataColumn x in rtable.Columns
                                          select x.ColumnName).ToArray();

                        //取出該talbe 空的資料表
                        DataTable desdt = new DataTable();
                        desdt = GetEmptyDataTable(rtable.TableName);
                        string destColName = "";

                        bulkinsert.ColumnMappings.Clear();
                        for (int i = 0; i < arrayNames.Length; i++)
                        {
                            if (desdt.Columns[arrayNames[i]].ColumnName.ToUpper() == arrayNames[i].ToUpper())
                            {
                                destColName = desdt.Columns[arrayNames[i]].ColumnName;
                            }
                            else
                            {
                                //比對不到欄位則由bulkinsert來回error
                                destColName = arrayNames[i];
                            }
                            bulkinsert.ColumnMappings.Add(arrayNames[i], destColName);
                        }

                        //寫入
                        bulkinsert.WriteToServer(rtable);
                        bulkinsert.Close();
                    }

                    if (innertransaction == true)
                        dbTransaction.Commit();


                }
                catch (Exception ex)
                {
                    if (innertransaction == true)
                        dbTransaction.Rollback();

                    throw ex;
                }
            }
        }

        public void InsertIntoTableWithTransaction(DataTable rtable, IDbTransaction dbTransaction)
        {
            bool innertransaction = false;
            IDbConnection dbConnection;
            if (dbTransaction == null)
            {
                innertransaction = true;
                dbConnection = base.OpenConnection();
            }
            else
            {
                dbConnection = dbTransaction.Connection;
            }

            if (rtable.TableName.Trim() == "" || rtable.TableName.ToUpper() == "TABLE")
                throw new Exception("DataTable has no TableName!!");

            if (rtable.Columns.Contains("NewId"))
            {
                rtable.Columns.Remove("NewId");
            }

            try
            {
                // 開始事務
                if (innertransaction == true)
                    dbTransaction = dbConnection.BeginTransaction();

                using (SqlBulkCopy bulkinsert = new SqlBulkCopy((SqlConnection)dbConnection, SqlBulkCopyOptions.FireTriggers, (SqlTransaction)dbTransaction))
                {
                    bulkinsert.BatchSize = 1000;
                    bulkinsert.BulkCopyTimeout = 60;
                    bulkinsert.DestinationTableName = rtable.TableName;

                    var arrayNames = (from DataColumn x in rtable.Columns
                                      select x.ColumnName).ToArray();

                    //取出該talbe 空的資料表
                    DataTable desdt = new DataTable();
                    desdt = GetEmptyDataTable(rtable.TableName);
                    string destColName = "";

                    bulkinsert.ColumnMappings.Clear();
                    for (int i = 0; i < arrayNames.Length; i++)
                    {
                        if (desdt.Columns[arrayNames[i]].ColumnName.ToUpper() == arrayNames[i].ToUpper())
                        {
                            destColName = desdt.Columns[arrayNames[i]].ColumnName;
                        }
                        else
                        {
                            //比對不到欄位則由bulkinsert來回error
                            destColName = arrayNames[i];
                        }
                        bulkinsert.ColumnMappings.Add(arrayNames[i], destColName);
                    }

                    //寫入
                    bulkinsert.WriteToServer(rtable);
                    bulkinsert.Close();
                }

                if (innertransaction == true)
                    dbTransaction.Commit();
            }
            catch (Exception ex)
            {
                if (innertransaction == true)
                    dbTransaction.Rollback();

                throw ex;
            }
        }

        /// <summary>
        /// 取出空的資料集
        /// </summary>
        /// <param name="tablename"></param>
        /// <returns></returns>
        public DataTable GetEmptyDataTable(string tablename)
        {
            try
            {
                DataTable dt = new DataTable();
                string sql = "select * from " + tablename + "  Where  1=2";
                dt = OpenDataTable(sql);
                dt.TableName = tablename;
                return dt;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 更新HTG發查狀態
        /// </summary>
        /// <param name="strVersionNewID"></param>
        /// <param name="strHTGSendStatus"></param>
        /// <param name="strHTGQryMessage"></param>
        /// <returns></returns>
        public bool EditWarningDetails(string strVersionNewID, string flag9091)
        {
            string sql = @"UPDATE [WarningDetails] 
                            SET 
                                [Set] = @flag9091,                                
                                [SetDate] = getdate()
                             WHERE NewID = @NewID";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("@NewID", strVersionNewID));
            Parameter.Add(new CommandParameter("@flag9091", flag9091));


            return ExecuteNonQuery(sql) > 0;
        }


        public bool UpdateWarningMasterForeignId(string CaseId, string Fid)
        {

            string sql = @"UPDATE [WarningMaster] 
                            SET 
                                [ForeignId] = @fid
                             WHERE CaseID = @CaseID";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("@fid", Fid));
            Parameter.Add(new CommandParameter("@CaseID", CaseId));


            return ExecuteNonQuery(sql) > 0;

        }

        public bool UpdateWarningMasterBasicInfo(string CaseId, DataRow dr)
        {

            string bDate = dr["DATE_OF_BIRTH"].ToString();
            // 要把bDate 的格式.. ddMMyyyy 改成.. yyyy-MM-dd 
            string dd = bDate.Substring(0, 2);
            string mm = bDate.Substring(2, 2);
            string yy = bDate.Substring(4, 4);
            string newdate = yy + "-" + mm + "-" + dd;


            string tel = "";
            // NIGTEL_NO > DAYTEL_NO > REGTEL_NO
            if (!string.IsNullOrEmpty(dr["NIGTEL_NO"].ToString()))
                tel = dr["NIGTEL_NO"].ToString();
            else
                if (!string.IsNullOrEmpty(dr["DAYTEL_NO"].ToString()))
                    tel = dr["DAYTEL_NO"].ToString();
                else
                    if (!string.IsNullOrEmpty(dr["REGTEL_NO"].ToString()))
                        tel = dr["REGTEL_NO"].ToString();
                    else
                        tel = "";

            string addr = dr["COMM_ADDR1"].ToString() + dr["COMM_ADDR2"].ToString() + dr["COMM_ADDR3"].ToString() + dr["COMM_ADDR4"].ToString();
            string mobile = dr["MOBIL_NO"].ToString();

            string sql = @"UPDATE [WarningMaster] 
                            SET 
                                [BirthDay] = @bDate,
                                [Tel] = @tel,
                                [Address] = @addr,
                                [Mobile] = @mobile
                             WHERE CaseID = @CaseID";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("@bDate", newdate));
            Parameter.Add(new CommandParameter("@tel", tel));
            Parameter.Add(new CommandParameter("@addr", addr));
            Parameter.Add(new CommandParameter("@mobile", mobile));
            Parameter.Add(new CommandParameter("@CaseID", CaseId));


            return ExecuteNonQuery(sql) > 0;

        }


        /// <summary>
        /// 依據60490SendID，刪除BOPS060490Recv、BOPS000401Recv、BOPS000401Send檔資料。怕DB存到冗餘資料。
        /// </summary>
        /// <param name="BOPS060490SendNewID">BOPS060490Send主鍵</param>
        /// <returns></returns>
        public bool DeleteHTGBy60490SendID(string BOPS060490SendNewID)
        {
            string sql = @"
                    DELETE FROM BOPS060490Recv WHERE SendNewID = @SendNewID;
                    DELETE FROM BOPS000401Recv WHERE SendNewID IN (SELECT NewID FROM BOPS000401Send WHERE BOPS060490SendNewID = @SendNewID);
                    DELETE FROM BOPS000401Send WHERE BOPS060490SendNewID = @SendNewID;
                    DELETE FROM BOPS081019Recv WHERE SendNewID IN (SELECT NewID FROM BOPS081019Send WHERE BOPS060490SendNewID = @SendNewID);
                    DELETE FROM BOPS081019Send WHERE BOPS060490SendNewID = @SendNewID;
                    ";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("@SendNewID", BOPS060490SendNewID));
            return ExecuteNonQuery(sql) > 0;
        }


        /// <summary>
        /// log 記錄
        /// </summary>
        /// <param name="msg"></param>
        public void WriteLog(string msg)
        {
            if (Directory.Exists(@".\Log") == false)
            {
                Directory.CreateDirectory(@".\Log");
            }
            LogManager.Exists("DebugLog").Debug(msg);
        }

        public DataSet getDataSetByXMLUser(string txid, string xml, out string c)
        {
            // 撈取下行儲存TABLE結構
            DataTable dtBig = base.Search("select * from BOPS" + txid + "Recv where 1=2");

            string strInsPath = AppDomain.CurrentDomain.BaseDirectory + xml;

            XmlDocument xmlDocument = new XmlDocument();

            xmlDocument.Load(strInsPath);

            ArrayList recstrlist = new ArrayList();
            recstrlist.Add(xmlDocument.InnerXml);

            DataTable dtApprMsgDefine = base.Search("select * from ApprMsgDefine where MsgName = '" + txid + "'");
            DataRow drApprMsgDefine = dtApprMsgDefine.Select("MsgName='" + txid + "'")[0];

            CTCB.NUMS.Library.HTG.HtgXmlParaMsg msg = new CTCB.NUMS.Library.HTG.HtgXmlParaMsg(drApprMsgDefine);

            string a = "";
            string b = "";

            DataSet htgdata = msg.TransferXmlToDataSet(dtBig, recstrlist, a, out b, out c);

            return htgdata;
        }

        /// <summary>
        /// 將發查結果新增到401電文檔
        /// </summary>
        /// <param name="VersionNewID"></param>
        /// <param name="dt"></param>
        /// <param name="txid"></param>
        /// <param name="BOPS060490SendNewID"></param>
        /// <param name="CurrencyList"></param>
        /// <param name="parApplyBranch"></param>
        /// <param name="parTellerNo"></param>
        public void Insert000401Send(string VersionNewID, DataTable dt, string txid, string BOPS060490SendNewID, DataTable CurrencyList, string parApplyBranch, string parTellerNo)
        {

            // 連接數據庫
            IDbConnection dbConnection = base.OpenConnection();
            IDbTransaction dbTransaction = null;

            // DB連接
            using (dbConnection)
            {
                try
                {
                    // 開始事務
                    dbTransaction = dbConnection.BeginTransaction();

                    CommandParameterCollection cmdparm = new CommandParameterCollection();
                    string insert000401Sql = "";
                    string insert081019Sql = "";

                    base.Parameter.Clear();

                    /*
                     * 20180730,PeterHsieh : 規格調整如下
                     * 	01.只能針對 系統碼[SYSTEM] in ('S','O','C')的帳號進行發查。
                     *	02.判斷 幣別[CCY]，若為 'TWD'則以帳號全碼進行發查，
                     *	   其它幣別則剔除帳號後三碼，並前補三碼 '0'進行發查。
                    */
                    for (int i = 0; i < 6; i++)
                    {
                        string strColumnName = "ACCOUNT_" + (i + 1).ToString();
                        string strColumnName_CCY = "CCY_" + (i + 1).ToString();

                        // 20180730,PeterHsieh : 增加取得欄位 [SYSTEM_?]
                        string strColumnName_SYSTEM = "SYSTEM_" + (i + 1).ToString();

                        if (dt.Columns.Contains(strColumnName) && dt.Rows[0][strColumnName].ToString().Trim() != ""
                            && dt.Rows[0][strColumnName].ToString().Trim() != "00000000000000000")
                        {
                            // AC_MARK_1 ~AC_MARK_12 如果為Y,則
                            //insert BOPS060490Send （ID_DATA_1 ~ ID_DATA_12）
                            #region 組BOPS060490Send sql
                            insert000401Sql += @"
                            insert BOPS000401Send
                                (
                                NewID
                                ,VersionNewID
                                ,BOPS060490SendNewID
                                ,AcctNo
                                ,Currency
                                ,CreatedUser
                                ,CreatedDate
                                ,ModifiedUser
                                ,ModifiedDate
                                ,SendStatus
                                )
                                values(
                                NewID()
                                ,@VersionNewID" + (i + 1).ToString() + @"
                                ,'" + BOPS060490SendNewID + @"'
                                ,'" + dt.Rows[0][strColumnName].ToString().Trim() + @"'
                                ,@Currency" + (i + 1).ToString() + @"
                                ,'CSFS.HTG'
                                ,GETDATE()
                                ,'CSFS.HTG'
                                ,GETDATE()
                                ,'02'
                                ) ";
                            #endregion

                            #region 組BOPS081019Send sql
                            if ("S,O,C".Contains(dt.Rows[0][strColumnName_SYSTEM].ToString().Trim()))
                            {
                                // 依據幣別取帳號
                                string account = dt.Rows[0][strColumnName].ToString().Trim();

                                if (dt.Rows[0][strColumnName_CCY].ToString().Trim() != "TWD")
                                {
                                    account = "000" + account.Substring(0, account.Length - 3);
                                }

                                insert081019Sql += @"
								insert BOPS081019Send
									(
									NewID
									,VersionNewID
									,BOPS060490SendNewID
									,ApplyBranch
									,TellerNo
									,AcctNo
									,Currency
									,CreatedUser
									,CreatedDate
									,ModifiedUser
									,ModifiedDate
									,SendStatus
									,ATMFlag
									)
									values(
									NewID()
									,'" + VersionNewID + @"'
									,'" + BOPS060490SendNewID + @"'
									,'" + parApplyBranch + @"'
									,'" + parTellerNo + @"'
									,'" + account + @"'
									,'" + dt.Rows[0][strColumnName_CCY].ToString().Trim() + @"'
									,'CSFS.HTG'
									,GETDATE()
									,'CSFS.HTG'
									,GETDATE()
									,'02'
									,'N'
									) ";
                            }
                            #endregion

                            base.Parameter.Add(new CommandParameter("@VersionNewID" + (i + 1).ToString(), VersionNewID));

                            // 依據賬號比對代碼檔查詢出幣別
                            //string strCurrency = GetCurrency(dt.Rows[0][strColumnName].ToString().Trim(), CurrencyList);
                            string strCurrency = dt.Rows[0][strColumnName_CCY].ToString().Trim();

                            base.Parameter.Add(new CommandParameter("@Currency" + (i + 1).ToString(), strCurrency));
                        }
                    }

                    if (!string.IsNullOrEmpty(insert000401Sql))
                    {
                        // 新增401Send
                        base.ExecuteNonQuery(insert000401Sql, dbTransaction, false);
                        // Adam 2022/03/01 檢查QueryType 2,4 才需要81019
                        // 新增81019Send
                        string CheckQueryTypesql = "Select QueryType from CaseCustDetails where DetailsId =  '" + VersionNewID + "' ";   // add
                        DataTable dtCaseCustDetails = base.Search(CheckQueryTypesql);                                                    // add
                        if (dtCaseCustDetails.Rows.Count > 0)                                                                            // add
                        {                                                                                                                // add
                            if (dtCaseCustDetails.Rows[0][0].ToString() == "2" || dtCaseCustDetails.Rows[0][0].ToString() == "4")        // add
                            {                                                                                                            // add
                                if (!string.IsNullOrEmpty(insert081019Sql))
                                {
                                    base.ExecuteNonQuery(insert081019Sql, dbTransaction, false);
                                }
                            }                                                                                                            // add
                        }                                                                                                                // add
                    }

                    dbTransaction.Commit();

                    if (insert000401Sql != "")
                    {
                        WriteLog("VersionNewID = [" + VersionNewID + "], TxID=[" + txid + "],資料寫入Table: " + "BOPS000401Send");
                    }
                }
                catch (Exception ex)
                {
                    dbTransaction.Rollback();

                    throw ex;
                }
            }

        }


        /// <summary>
        /// 將發查結果新增到9091-13電文檔
        /// Written by Patrick
        /// </summary>
        /// <param name="VersionNewID"></param>
        /// <param name="dt"></param>
        /// <param name="txid"></param>
        /// <param name="BOPS060490SendNewID"></param>
        /// <param name="CurrencyList"></param>
        /// <param name="parApplyBranch"></param>
        /// <param name="parTellerNo"></param>
        public void Insert9091_13Send(string VersionNewID, DataTable dt, string txid, string BOPS060490SendNewID, string CustAccount, DataTable CurrencyList, string parApplyBranch, string parTellerNo)
        {

            // 連接數據庫
            IDbConnection dbConnection = base.OpenConnection();
            IDbTransaction dbTransaction = null;

            // DB連接
            //using (dbConnection)
            {
                try
                {
                    // 開始事務
                    //dbTransaction = dbConnection.BeginTransaction();

                    //CommandParameterCollection cmdparm = new CommandParameterCollection();
                    //string insert000401Sql = "";


                    base.Parameter.Clear();


                    for (int i = 0; i < 6; i++)
                    {
                        string strColumnName = "ACCOUNT_" + (i + 1).ToString();
                        string strColumnName_CCY = "CCY_" + (i + 1).ToString();

                        // 20180730,PeterHsieh : 增加取得欄位 [SYSTEM_?]
                        string strColumnName_SYSTEM = "SYSTEM_" + (i + 1).ToString();

                        if (dt.Columns.Contains(strColumnName) && dt.Rows[0][strColumnName].ToString().Trim() != ""
                            && dt.Rows[0][strColumnName].ToString().Trim() != "00000000000000000")
                        {

                            string strAccount = dt.Rows[0][strColumnName].ToString().Trim();
                            string strCurrency = dt.Rows[0][strColumnName_CCY].ToString().Trim();
                            if (strAccount != CustAccount) // 2022-05-25,  要通報的衍生帳號, 不可包含.. 通報的帳號....
                                insert9091row(Guid.Parse(VersionNewID), strAccount, strCurrency, "SYS");

                        }
                    }



                    //dbTransaction.Commit();

                    //if (insert000401Sql != "")
                    {
                        WriteLog("VersionNewID = [" + VersionNewID + "], TxID=[" + txid + "],資料寫入Table: " + "BOPS000401Send");
                    }
                }
                catch (Exception ex)
                {
                    //dbTransaction.Rollback();
                    WriteLog("VersionNewID = [" + VersionNewID + "], TxID=[" + txid + "], 發生錯誤" + ex.Message.ToString());
                }
            }

        }



        internal void insert9091row(Guid vNewId, string custAccount, string currency, string userid)
        {
            string updateSql = @" insert BOPS009091Send
                                             (       
                                             NewID                                      
                                             ,VersionNewID
                                             , ACCT_NO
                                             , CURRENCY
                                             , DTSRC_DATE
                                             , STOP_RESN_CODE
                                             , STOP_RESN_DESC
                                             , [FUNCTION]
                                             , WRITTEN
                                             , CreatedUser
                                             , CreatedDate
                                             , ModifiedUser
                                             , ModifiedDate
                                             , SendStatus
                                             )
                                             values(                                             
                                             NEWID()
                                             ,@VersionNewID
                                             , @ACCT_NO
                                             , @CURRENCY
                                             , @DTSRC_DATE
                                             , @STOP_RESN_CODE
                                             , @STOP_RESN_DESC
                                             , @FUNCTION2
                                             , @WRITTEN
                                             , @CreatedUser
                                             , getdate()
                                             , @CreatedUser
                                             , getdate()
                                             , '02'
                                             ); ";
            base.Parameter.Clear();
            base.Parameter.Add(new CommandParameter("@VersionNewID", vNewId));
            base.Parameter.Add(new CommandParameter("@ACCT_NO", custAccount));
            base.Parameter.Add(new CommandParameter("@CURRENCY", currency));
            base.Parameter.Add(new CommandParameter("@DTSRC_DATE", ""));
            base.Parameter.Add(new CommandParameter("@STOP_RESN_CODE", "13"));
            base.Parameter.Add(new CommandParameter("@STOP_RESN_DESC", ""));
            base.Parameter.Add(new CommandParameter("@FUNCTION2", "0"));
            base.Parameter.Add(new CommandParameter("@WRITTEN", ""));
            base.Parameter.Add(new CommandParameter("@CreatedUser", userid));

            base.ExecuteNonQuery(updateSql);

        }



        /// <summary>
        /// 根據賬號中的標誌位獲取幣別
        /// </summary>
        /// <param name="strValue"></param>
        /// <returns></returns>
        public string GetCurrency(string strValue, DataTable CurrencyList)
        {
            // 截取標誌位
            string strFlag = strValue.Length > 4 ? strValue.Substring(0, 4) : "";

            // strFlag != "0000",標誌位爲賬號的後3位
            strFlag = strFlag == "0000" ? strFlag : strValue.Substring(strValue.Length - 3, 3);

            // 獲取幣別
            DataRow[] dr = CurrencyList.Select("CodeNo ='" + strFlag + "'");
            string strCurrency = dr != null && dr.Length > 0 ? dr[0]["CodeDesc"].ToString() : "";

            return strCurrency;
        }



        /// <summary>
        /// 將案件主檔和Version檔狀態更新為失敗
        /// </summary>
        /// <param name="Status"></param>
        /// <param name="NewID"></param>
        /// <returns></returns>
        public int UpdateCaseCustDetailStatus(string Status, string NewID)
        {
            try
            {
                //                string sql = @"
                //UPDATE CaseCustDetails SET Status = @Status  WHERE   DetailsId = @NewID;
                //UPDATE CaseCustMaster SET Status = @Status WHERE NewID IN (SELECT CaseCustMasterId FROM CaseCustDetails WHERE DetailsId = @NewID)
                //";


                // 20220124,  不修改 CaseCustMaster.Status, 等 ReturnFile 產檔後再押'03' 或 '04'
                string sql = @"UPDATE CaseCustDetails SET Status = @Status  WHERE   DetailsId = @NewID;";

                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@Status", Status));
                base.Parameter.Add(new CommandParameter("@NewID", NewID));

                return base.ExecuteNonQuery(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 查詢發查人員是否有異常的案件
        /// </summary>
        /// <param name="VersionNewID"></param>
        /// <returns></returns>
        public DataTable GetErrorSend(string VersionNewID)
        {
            try
            {
                //                string strDataSql = @"
                //select NewID,QueryErrMsg,'BOPS067050Send' as txno  from BOPS067050Send WHERE VersionNewID = @VersionNewID and  sendstatus='03'
                //union
                //select NewID,QueryErrMsg,'BOPS060628Send' as txno  from BOPS060628Send WHERE VersionNewID = @VersionNewID and  sendstatus='03'
                //union
                //select NewID,QueryErrMsg,'BOPS060490Send' as txno  from BOPS060490Send WHERE VersionNewID = @VersionNewID and  sendstatus='03'
                //union
                //select NewID,QueryErrMsg,'BOPS000401Send' as txno from BOPS000401Send WHERE VersionNewID = @VersionNewID and  sendstatus='03'
                //union
                //select NewID,QueryErrMsg,'BOPS009091Send' as txno from BOPS009091Send WHERE VersionNewID = @VersionNewID and  sendstatus='03'  and ( QueryErrMsg<>'帳戶已經結清' OR QueryErrMsg<>'此事故代號或凍結碼重複設定')
                //";

                string strDataSql = @"
select NewID,QueryErrMsg,'BOPS009091Send' as txno from BOPS009091Send WHERE VersionNewID = @VersionNewID and  sendstatus='03'  and NOT ( QueryErrMsg='提示代碼:2881 提示原因:此事故代號或凍結碼重複設定' OR QueryErrMsg='帳戶已經結清' ) 
";

                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@VersionNewID", VersionNewID));

                return base.Search(strDataSql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 查詢發查人員是否都成功的案件
        /// </summary>
        /// <param name="VersionNewID"></param>
        /// <returns></returns>
        public DataTable GetSuccessSend(string VersionNewID)
        {
            try
            {
                //                string strDataSql = @"
                //                    select NewID from BOPS067050Send WHERE VersionNewID = @VersionNewID and  sendstatus IN ('02', '03')
                //                    union
                //                    select NewID from BOPS060628Send WHERE VersionNewID = @VersionNewID and  sendstatus IN ('02', '03')
                //                    union
                //                    select NewID from BOPS060490Send WHERE VersionNewID = @VersionNewID and  sendstatus IN ('02', '03')
                //                    union
                //                    select NewID from BOPS000401Send WHERE VersionNewID = @VersionNewID and  sendstatus IN ('02', '03')                    
                //                    union
                //                    select NewID from BOPS009091Send WHERE VersionNewID = @VersionNewID and  sendstatus IN ('02', '03') 
                //                ";

                string strDataSql = @"select NewID from BOPS009091Send WHERE VersionNewID = @VersionNewID and  sendstatus IN ('02', '03')";



                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@VersionNewID", VersionNewID));

                return base.Search(strDataSql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        internal void EditCaseCustNewRFDMSendIDNo(string caseId)
        {
            string retSql = "select TOP 1  CustIdNo from BOPS060628Send WHERE VersionNewID= @caseId";
            base.Parameter.Clear();
            base.Parameter.Add(new CommandParameter("@caseId", caseId));
            var ret = base.Search(retSql);
            string idNo = "";
            if (ret.Rows.Count > 0)
            {
                idNo = ret.Rows[0][0].ToString();

                string sql = @"UPDATE CaseCustNewRFDMSend Set ID_NO=@idno WHERE VersionNewID=@caseId";
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@caseId", caseId));
                base.Parameter.Add(new CommandParameter("@idno", idNo));

                base.ExecuteNonQuery(sql);
            }



        }

        internal void EditCaseCustDetailsIDNo(string caseId)
        {
            string retSql = "select TOP 1  CustIdNo from BOPS060628Send WHERE VersionNewID= @caseId";
            base.Parameter.Clear();
            base.Parameter.Add(new CommandParameter("@caseId", caseId));
            var ret = base.Search(retSql);
            string idNo = "";
            if (ret.Rows.Count > 0)
            {
                idNo = ret.Rows[0][0].ToString();
                string sql = @"UPDATE CaseCustDetails Set CustIdNo=@idno WHERE DetailsId=@caseId";
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@caseId", caseId));
                base.Parameter.Add(new CommandParameter("@idno", idNo));

                base.ExecuteNonQuery(sql);
            }



        }


        internal void UpdateDetailsErrorMessage(string caseid, string errorMessage)
        {
            if (errorMessage.Length > 1980)
                errorMessage = errorMessage.Substring(0, 1980);
            string sql = @"UPDATE CaseCustDetails Set ErrorMessage=@errorMessage WHERE DetailsId=@caseId";
            base.Parameter.Clear();
            base.Parameter.Add(new CommandParameter("@caseId", caseid));
            base.Parameter.Add(new CommandParameter("@errorMessage", errorMessage));

            base.ExecuteNonQuery(sql);
        }

        internal void insert67050Send(DataRow dr, string userid)
        {
            string updateSql = @" insert BOPS067050Send
                                             (
                                             NewID
                                             ,VersionNewID
                                             , CustIdNo
                                             , Optn
                                             , CreatedUser
                                             , CreatedDate
                                             , ModifiedUser
                                             , ModifiedDate
                                             ,SendStatus
                                             )
                                             values(
                                             NEWID()
                                             , @VersionNewID
                                             , @CustIdNo
                                             , 'D'
                                             , @CreatedUser
                                             , getdate()
                                             , @CreatedUser
                                             , getdate()
                                             , '02'
                                             ); ";
            base.Parameter.Clear();
            base.Parameter.Add(new CommandParameter("@VersionNewID", dr["NewID"]));
            base.Parameter.Add(new CommandParameter("@CustIdNo", dr["CustId"]));
            base.Parameter.Add(new CommandParameter("@CreatedUser", userid));

            base.ExecuteNonQuery(updateSql);

        }

        public void Insert67050V4Send(string VersionNewID, string CustId)
        {

            // 連接數據庫
            IDbConnection dbConnection = base.OpenConnection();
            IDbTransaction dbTransaction = null;

            // DB連接
            using (dbConnection)
            {
                try
                {
                    // 開始事務
                    dbTransaction = dbConnection.BeginTransaction();

                    CommandParameterCollection cmdparm = new CommandParameterCollection();
                    string insert67050V4Sql = "";

                    // base.Parameter.Clear();

                    insert67050V4Sql = @" insert BOPS067050V4Send
                                             (
                                             NewID
                                             ,VersionNewID
                                             , CustIdNo
                                             , Optn
                                             , CreatedUser
                                             , CreatedDate
                                             , ModifiedUser
                                             , ModifiedDate
                                             ,SendStatus
                                             )
                                            values(
                                                    NewID()
                                                    ,'" + VersionNewID + @"'
                                                    ,'" + CustId + @"'
                                             , '4'
                                             , 'SYSTEM'
                                             , getdate()
                                             , 'SYSTEM'
                                             , getdate()
                                             , '02'
                                             ); ";

                    string chksql = @"select NewID  from BOPS067050V4Send where  SendStatus = '02'  and  VersionNewID = '" + VersionNewID + @"'  and CustIdNo = '" + CustId + @"' ";
                    DataTable dtExist = OpenDataTable(chksql);
                    if (!string.IsNullOrEmpty(insert67050V4Sql) && (dtExist.Rows.Count < 1))
                    {
                        base.ExecuteNonQuery(insert67050V4Sql, dbTransaction, false);
                    }
                    dbTransaction.Commit();

                }
                catch (Exception ex)
                {
                    dbTransaction.Rollback();
                    WriteLog("error:" + ex.Message.ToString());
                    throw ex;
                }
            }

        }

        public void Insert60490Send(string VersionNewID, DataTable dt, string txid, string BOPS060490SendNewID)
        {

            // 連接數據庫
            IDbConnection dbConnection = base.OpenConnection();
            IDbTransaction dbTransaction = null;

            // DB連接
            using (dbConnection)
            {
                try
                {
                    // 開始事務
                    dbTransaction = dbConnection.BeginTransaction();

                    CommandParameterCollection cmdparm = new CommandParameterCollection();
                    string insert60490Sql = "";

                    base.Parameter.Clear();

                    //for (int i = 0; i < 12; i++)
                    //{
                    //   string strColumnName = "AC_MARK_" + (i + 1).ToString();

                    //   string CustIdNo = "ID_DATA_" + (i + 1).ToString();

                    //if (dt.Columns.Contains(strColumnName) && dt.Rows[0][strColumnName].ToString().Trim() == "Y" && CustIdNo != "")
                    //{

                    // AC_MARK_1 ~AC_MARK_12 如果為Y,則
                    //insert BOPS060490Send （ID_DATA_1 ~ ID_DATA_12）
                    //SA:活存; CA:支存; TD:定存
                    #region 組sql
                    insert60490Sql += @"
                                                    INSERT BOPS060490Send
                                                    (
                                                    NewID
                                                    ,VersionNewID
                                                    ,BOPS060628SendNewID
                                                    ,CustIdNo
                                                    ,IDType
                                                    ,CustomerNo
                                                    ,EnquiryOption
                                                    ,AccountStatus
                                                    ,ClosedDate
                                                    ,ProductOption
                                                    ,CurrencyType
                                                    ,CreatedUser
                                                    ,CreatedDate
                                                    ,ModifiedUser
                                                    ,ModifiedDate
                                                    ,SendStatus
                                                    )
                                                    values(
                                                    NewID()
                                                    ,'" + VersionNewID + @"'
                                                    ,'" + BOPS060490SendNewID + @"'
                                                    ,'" + dt.Rows[0]["CUST_ID_NO"].ToString().Trim() + @"'
                                                    ,'01'
                                                    ,''
                                                    ,'S'
                                                    ,'1'
                                                    ,(select top 1 QDateS from  CaseTrsQueryVersion where NewID ='" + VersionNewID + @"')
                                                    ,'SA'
                                                    ,'0'
                                                    ,'CSFS.HTG'
                                                    ,GETDATE()
                                                    ,'CSFS.HTG'
                                                    ,GETDATE()
                                                    ,'02'
                                                    ); ";

                    insert60490Sql += @"
                                                    INSERT BOPS060490Send
                                                    (
                                                    NewID
                                                    ,VersionNewID
                                                    ,BOPS060628SendNewID
                                                    ,CustIdNo
                                                    ,IDType
                                                    ,CustomerNo
                                                    ,EnquiryOption
                                                    ,AccountStatus
                                                    ,ClosedDate
                                                    ,ProductOption
                                                    ,CurrencyType
                                                    ,CreatedUser
                                                    ,CreatedDate
                                                    ,ModifiedUser
                                                    ,ModifiedDate
                                                    ,SendStatus
                                                    )
                                                    values(
                                                    NewID()
                                                    ,'" + VersionNewID + @"'
                                                    ,'" + BOPS060490SendNewID + @"'
                                                    ,'" + dt.Rows[0]["CUST_ID_NO"].ToString().Trim() + @"'
                                                    ,'01'
                                                    ,''
                                                    ,'S'
                                                    ,'1'
                                                    ,(select top 1 QDateS from  CaseTrsQueryVersion where NewID ='" + VersionNewID + @"')
                                                    ,'CA'
                                                    ,'0'
                                                    ,'CSFS.HTG'
                                                    ,GETDATE()
                                                    ,'CSFS.HTG'
                                                    ,GETDATE()
                                                    ,'02'
                                                    ); ";

                    insert60490Sql += @"
                                                    INSERT BOPS060490Send
                                                    (
                                                    NewID
                                                    ,VersionNewID
                                                    ,BOPS060628SendNewID
                                                    ,CustIdNo
                                                    ,IDType
                                                    ,CustomerNo
                                                    ,EnquiryOption
                                                    ,AccountStatus
                                                    ,ClosedDate
                                                    ,ProductOption
                                                    ,CurrencyType
                                                    ,CreatedUser
                                                    ,CreatedDate
                                                    ,ModifiedUser
                                                    ,ModifiedDate
                                                    ,SendStatus
                                                    )
                                                    values(
                                                    NewID()
                                                    ,'" + VersionNewID + @"'
                                                    ,'" + BOPS060490SendNewID + @"'
                                                    ,'" + dt.Rows[0]["CUST_ID_NO"].ToString().Trim() + @"'
                                                    ,'01'
                                                    ,''
                                                    ,'S'
                                                    ,'1'
                                                    ,(select top 1 QDateS from  CaseTrsQueryVersion where NewID ='" + VersionNewID + @"')
                                                    ,'TD'
                                                    ,'0'
                                                    ,'CSFS.HTG'
                                                    ,GETDATE()
                                                    ,'CSFS.HTG'
                                                    ,GETDATE()
                                                    ,'02'
                                                    ); ";
                    #endregion

                    //}
                    //}

                    //20181228 固定變更 add start
                    if (!string.IsNullOrEmpty(insert60490Sql))
                    {
                        base.ExecuteNonQuery(insert60490Sql, dbTransaction, false);
                    }
                    //20181228 固定變更 add end

                    dbTransaction.Commit();

                    if (insert60490Sql != "")
                    {
                        WriteLog("VersionNewID = [" + VersionNewID + "], TxID=[" + txid + "],資料寫入Table: " + "BOPS" + txid + "Send");
                    }
                }
                catch (Exception ex)
                {
                    dbTransaction.Rollback();
                    WriteLog("error:" + ex.Message.ToString());
                    throw ex;
                }
            }

        }



        internal void insert9091Send(DataRow dr, string userid)
        {
            string updateSql = @" insert BOPS009091Send
                                             (                                             
                                             VersionNewID
                                             , ACCT_NO
                                             , CURRENCY
                                             , DTSRC_DATE
                                             , STOP_RESN_CODE
                                             , STOP_RESN_DESC
                                             , [FUNCTION]
                                             , WRITTEN
                                             , CreatedUser
                                             , CreatedDate
                                             , ModifiedUser
                                             , ModifiedDate
                                             , SendStatus
                                             )
                                             values(                                             
                                             @VersionNewID
                                             , @ACCT_NO
                                             , @CURRENCY
                                             , @DTSRC_DATE
                                             , @STOP_RESN_CODE
                                             , @STOP_RESN_DESC
                                             , @FUNCTION2
                                             , @WRITTEN
                                             , @CreatedUser
                                             , getdate()
                                             , @CreatedUser
                                             , getdate()
                                             , '02'
                                             ); ";
            base.Parameter.Clear();
            base.Parameter.Add(new CommandParameter("@VersionNewID", dr["NewID"]));
            base.Parameter.Add(new CommandParameter("@ACCT_NO", dr["CustAccount"]));
            base.Parameter.Add(new CommandParameter("@CURRENCY", dr["Currency"]));
            base.Parameter.Add(new CommandParameter("@DTSRC_DATE", ""));
            base.Parameter.Add(new CommandParameter("@STOP_RESN_CODE", "13"));
            base.Parameter.Add(new CommandParameter("@STOP_RESN_DESC", ""));
            base.Parameter.Add(new CommandParameter("@FUNCTION2", "0"));
            base.Parameter.Add(new CommandParameter("@WRITTEN", ""));
            base.Parameter.Add(new CommandParameter("@CreatedUser", userid));

            base.ExecuteNonQuery(updateSql);

        }


        internal DataTable getHTGtobeHit(Guid newid)
        {

            string sql = "SELECT * FROM BOPS067050Send WHERE VersionNewID='@caseId' UNION select * from BOPS060628Send where VersionNewID = '@caseId'";
            base.Parameter.Clear();
            base.Parameter.Add(new CommandParameter("@caseId", newid));

            return base.Search(sql);

        }

        internal void BOPS2TX(string VersionNewID, string docNo)
        {
            // 若有錯誤  dtErrorSend.Row > 0 ,  2022-05-18,回寫到TX 
            // 有三個欄位... dr["NewID"], dr["QueryErrMsg"], dr["txno"]
            // 共有...BOPS067050Recv   =====> 不手轉到TX, 
            //        BOPS060628Recv([dbo].[TX_60629])  
            //        BOPS060490Recv   [dbo].[TX_60491_Grp]  [dbo].[TX_60491_Detl]
            //        BOPS000401Recv  [dbo].[TX_33401]
            //        BOPS009091Recv [dbo].[TX_09091]




            try
            {
                string ch60628 = @"Select count(*) from BOPS060628Recv WHERE VersionNewID='" + VersionNewID + "'";
                var r60628 = base.Search(ch60628);

                if (int.Parse(r60628.Rows[0][0].ToString()) > 0)
                {
                    #region TX60628

                    string s60628 = @"INSERT INTO TX_60629 (CaseId,[cCretDT], ACTION, ID_DATA_1, BIRTH_DT_1, CUST_NAME_1, AC_MARK_1, ID_DATA_2, BIRTH_DT_2, CUST_NAME_2, AC_MARK_2, ID_DATA_3, BIRTH_DT_3, CUST_NAME_3, AC_MARK_3, ID_DATA_4, BIRTH_DT_4, CUST_NAME_4, AC_MARK_4, ID_DATA_5, BIRTH_DT_5, CUST_NAME_5, AC_MARK_5, ID_DATA_6, BIRTH_DT_6, CUST_NAME_6, AC_MARK_6, ID_DATA_7, BIRTH_DT_7, CUST_NAME_7, AC_MARK_7, ID_DATA_8, BIRTH_DT_8, CUST_NAME_8, AC_MARK_8, ID_DATA_9, BIRTH_DT_9, CUST_NAME_9, AC_MARK_9, ID_DATA_10, BIRTH_DT_10, CUST_NAME_10, AC_MARK_10, ID_DATA_11, BIRTH_DT_11, CUST_NAME_11, AC_MARK_11, ID_DATA_12, BIRTH_DT_12, CUST_NAME_12, AC_MARK_12, DocNo)
SELECT  VersionNewID AS CaseId,getdate(), ACTION, ID_DATA_1, BIRTH_DT_1, CUST_NAME_1, AC_MARK_1, ID_DATA_2, BIRTH_DT_2, CUST_NAME_2, AC_MARK_2, ID_DATA_3, BIRTH_DT_3, CUST_NAME_3, AC_MARK_3, ID_DATA_4, BIRTH_DT_4, CUST_NAME_4, AC_MARK_4, ID_DATA_5, BIRTH_DT_5, CUST_NAME_5, AC_MARK_5, ID_DATA_6, BIRTH_DT_6, CUST_NAME_6, AC_MARK_6, ID_DATA_7, BIRTH_DT_7, CUST_NAME_7, AC_MARK_7, ID_DATA_8, BIRTH_DT_8, CUST_NAME_8, AC_MARK_8, ID_DATA_9, BIRTH_DT_9, CUST_NAME_9, AC_MARK_9, ID_DATA_10, BIRTH_DT_10, CUST_NAME_10, AC_MARK_10, ID_DATA_11, BIRTH_DT_11, CUST_NAME_11, AC_MARK_11, ID_DATA_12, BIRTH_DT_12, CUST_NAME_12, AC_MARK_12,'" + docNo + "' from BOPS060628Recv WHERE VersionNewID='" + VersionNewID + "'";
                    base.Parameter.Clear();
                    base.ExecuteNonQuery(s60628);

                    #endregion
                }



            }
            catch (Exception ex)
            {
                WriteLog("Move TX60628 Error " + ex.Message.ToString());

            }

            try
            {
                string ch60491 = @"Select count(*) from BOPS060490Recv WHERE VersionNewID='" + VersionNewID + "'";
                var r604918 = base.Search(ch60491);

                if (int.Parse(r604918.Rows[0][0].ToString()) > 0)
                {
                    #region 搬至TX_60491_Grp , TX_60491_Detl

                    // 先搬共用的欄位.....

                    #region 取出欄位....SQL



                    string sql60491 = @"SELECT 
TOP 1 
ACTION AS Action,
ADD1 AS Addr1,
ADD2 AS Addr2,
ADD3 AS Addr3,
AMT AS Amt,
ASSET_VAR AS AssetVar,
BIR_DATE AS BirthDt,
CARD_FLAG AS CardFlag,
CARD_LIMIT AS CardLimit,
CONTRIB AS Contrib,
CUST_ID_NO AS CustomerId,
CUSTOMER_NAME AS CustomerName,
CUSTOMER_NO AS CustomerNo,
CUST_TYPE AS CustType,
DEP_TOT AS DepTot,
EMAIL AS Email,
ENQ_OPT AS EnqOpt,
FB_AO_BRANCH AS FbAoBranch,
FB_AO_CODE AS FbAoCode,
FB_TELLER AS FbTeller,
FUND_CIF AS FundCif,
HIGH_CONTR AS HighContr,
HOUSE_FLAG AS HouseholdFlag,
INPUT_MSG_TYPE AS InputMsgType,
KEEP_CURRENCY AS KeepCurrency,
KEEP_ENQ_CLS_DATE AS KeepEnqClsDate,
KEEP_OPT AS KeepOpt,
KEEP_READ_FLAG AS KeepReadFlag,
KEEP_RECNO AS KeepRecno,
KEEP_STS AS KeepSts,
KEEP_WA_IDX AS KeepWaIdx,
LGMB_FLAG AS LgmbFlag,
LON_TOT AS LonTot,
MNTHS_SNC AS MnthsSnc,
MOBIL_NO AS MobilNo,
MUTLT_FLAG AS MutltFlag,
MUT_TOT AS MutTot,
NET_ASSET AS NetAsset,
NO_OF_CARDS AS NoOfCards,
OCPN_DESC AS OcpnDesc,
OLD_FLAG AS OldFlag,
RANK AS Rank,
RISK_ATTRIB AS RiskAttrib,
RM_NO AS RMNum,
SBOX_FLAG AS SboxFlag,
SELECT_NO AS SelectNo,
SERVICE_CODE1 AS ServiceCode1,
TEL_DAY AS TelDay,
TEL_DAY_EXT AS TelDayExt,
TEL_NIG AS TelNig,
TEL_NIG_EXT AS TelNigExt,
TRIAL_FLAG AS TrialFlag,
TRUST_ONE_ACTUAL AS TrustOneActual,
TRUST_ONE_APPL AS TrustOneAppl,
VIP_CD_H AS VipCdH,
VIP_CD_I AS VipCdI,
VIP_CODE AS VIPCode,
VIP_DEGREE AS VipDegree,
VIP_DEGREE_H AS VipDegreeH,
WM_ASSET_AMT AS WmAssetAmt,
CUST_ID_NO AS CustomerId
FROM [dbo].[BOPS060490Recv]
WHERE  VERSIONNEWID='" + VersionNewID + "'";

                    #endregion

                    var s60491 = base.Search(sql60491);
                    DataRow dr = s60491.Rows[0];
                    // 插入TX_60491_Grp
                    string i60491 = @"INSERT INTO TX_60491_Grp(SNO, CaseId,cCretDT, Action,Addr1,Addr2,Addr3,Amt,AssetVar,BirthDt,CardFlag,CardLimit,Contrib,CustomerId,CustomerName,CustomerNo,CustType,DepTot,Email,EnqOpt,FbAoBranch,FbAoCode,FbTeller,FundCif,HighContr,HouseholdFlag,InputMsgType,KeepCurrency,KeepEnqClsDate,KeepOpt,KeepReadFlag,KeepSts,KeepWaIdx,LgmbFlag,LonTot,MnthsSnc,MobilNo,MutltFlag,MutTot,NetAsset,NoOfCards,OcpnDesc,OldFlag,Rank,RiskAttrib,RMNum,SboxFlag,SelectNo,ServiceCode1,TelDay,TelDayExt,TelNig,TelNigExt,TrialFlag,TrustOneActual,TrustOneAppl,VipCdH,VipCdI,VIPCode,VipDegree,VipDegreeH,WmAssetAmt,DocNo) 
                            VALUES ( NEXT VALUE FOR dbo.SEQTX_60491_Grp, @VersionNewID,getdate(), @Action,@Addr1,@Addr2,@Addr3,@Amt,@AssetVar,@BirthDt,@CardFlag,@CardLimit,@Contrib,@CustomerId,@CustomerName,@CustomerNo,@CustType,@DepTot,@Email,@EnqOpt,@FbAoBranch,@FbAoCode,@FbTeller,@FundCif,@HighContr,@HouseholdFlag,@InputMsgType,@KeepCurrency,@KeepEnqClsDate,@KeepOpt,@KeepReadFlag,@KeepSts,@KeepWaIdx,@LgmbFlag,@LonTot,@MnthsSnc,@MobilNo,@MutltFlag,@MutTot,@NetAsset,@NoOfCards,@OcpnDesc,@OldFlag,@Rank,@RiskAttrib,@RMNum,@SboxFlag,@SelectNo,@ServiceCode1,@TelDay,@TelDayExt,@TelNig,@TelNigExt,@TrialFlag,@TrustOneActual,@TrustOneAppl,@VipCdH,@VipCdI,@VIPCode,@VipDegree,@VipDegreeH,@WmAssetAmt,@docNo);";
                    base.Parameter.Clear();

                    //  dr['Action'],dr['Addr1'],dr['Addr2'],dr['Addr3'],dr['Amt'],dr['AssetVar'],dr['BirthDt'],dr['CardFlag'],dr['CardLimit'],dr['Contrib'],dr['CustomerId'],dr['CustomerName'],dr['CustomerNo'],dr['CustType'],dr['DepTot'],dr['Email'],dr['EnqOpt'],dr['FbAoBranch'],dr['FbAoCode'],dr['FbTeller'],dr['FundCif'],dr['HighContr'],dr['HouseholdFlag'],dr['InputMsgType'],dr['KeepCurrency'],dr['KeepEnqClsDate'],dr['KeepOpt'],dr['KeepReadFlag'],dr['KeepRecno'],dr['KeepSts'],dr['KeepWaIdx'],dr['LgmbFlag'],dr['LonTot'],dr['MnthsSnc'],dr['MobilNo'],dr['MutltFlag'],dr['MutTot'],dr['NetAsset'],dr['NoOfCards'],dr['OcpnDesc'],dr['OldFlag'],dr['Rank'],dr['RiskAttrib'],dr['RMNum'],dr['SboxFlag'],dr['SelectNo'],dr['ServiceCode1'],dr['TelDay'],dr['TelDayExt'],dr['TelNig'],dr['TelNigExt'],dr['TrialFlag'],dr['TrustOneActual'],dr['TrustOneAppl'],dr['VipCdH'],dr['VipCdI'],dr['VIPCode'],dr['VipDegree'],dr['VipDegreeH'],dr['WmAssetAmt']
                    #region base.parameter
                    base.Parameter.Add(new CommandParameter("@VersionNewID", VersionNewID));
                    base.Parameter.Add(new CommandParameter("@Action", dr["Action"]));
                    base.Parameter.Add(new CommandParameter("@Addr1", dr["Addr1"]));
                    base.Parameter.Add(new CommandParameter("@Addr2", dr["Addr2"]));
                    base.Parameter.Add(new CommandParameter("@Addr3", dr["Addr3"]));
                    base.Parameter.Add(new CommandParameter("@Amt", dr["Amt"]));
                    base.Parameter.Add(new CommandParameter("@AssetVar", dr["AssetVar"]));
                    base.Parameter.Add(new CommandParameter("@BirthDt", dr["BirthDt"]));
                    base.Parameter.Add(new CommandParameter("@CardFlag", dr["CardFlag"]));
                    base.Parameter.Add(new CommandParameter("@CardLimit", dr["CardLimit"]));
                    base.Parameter.Add(new CommandParameter("@Contrib", dr["Contrib"]));
                    base.Parameter.Add(new CommandParameter("@CustomerId", dr["CustomerId"]));
                    base.Parameter.Add(new CommandParameter("@CustomerName", dr["CustomerName"]));
                    base.Parameter.Add(new CommandParameter("@CustomerNo", dr["CustomerNo"]));
                    base.Parameter.Add(new CommandParameter("@CustType", dr["CustType"]));
                    base.Parameter.Add(new CommandParameter("@DepTot", dr["DepTot"]));
                    base.Parameter.Add(new CommandParameter("@Email", dr["Email"]));
                    base.Parameter.Add(new CommandParameter("@EnqOpt", dr["EnqOpt"]));
                    base.Parameter.Add(new CommandParameter("@FbAoBranch", dr["FbAoBranch"]));
                    base.Parameter.Add(new CommandParameter("@FbAoCode", dr["FbAoCode"]));
                    base.Parameter.Add(new CommandParameter("@FbTeller", dr["FbTeller"]));
                    base.Parameter.Add(new CommandParameter("@FundCif", dr["FundCif"]));
                    base.Parameter.Add(new CommandParameter("@HighContr", dr["HighContr"]));
                    base.Parameter.Add(new CommandParameter("@HouseholdFlag", dr["HouseholdFlag"]));
                    base.Parameter.Add(new CommandParameter("@InputMsgType", dr["InputMsgType"]));
                    base.Parameter.Add(new CommandParameter("@KeepCurrency", dr["KeepCurrency"]));
                    base.Parameter.Add(new CommandParameter("@KeepEnqClsDate", dr["KeepEnqClsDate"]));
                    base.Parameter.Add(new CommandParameter("@KeepOpt", dr["KeepOpt"]));
                    base.Parameter.Add(new CommandParameter("@KeepReadFlag", dr["KeepReadFlag"]));
                    //base.Parameter.Add(new CommandParameter("@KeepRecno", dr["KeepRecno"]));
                    base.Parameter.Add(new CommandParameter("@KeepSts", dr["KeepSts"]));
                    base.Parameter.Add(new CommandParameter("@KeepWaIdx", dr["KeepWaIdx"]));
                    base.Parameter.Add(new CommandParameter("@LgmbFlag", dr["LgmbFlag"]));
                    base.Parameter.Add(new CommandParameter("@LonTot", dr["LonTot"]));
                    base.Parameter.Add(new CommandParameter("@MnthsSnc", dr["MnthsSnc"]));
                    base.Parameter.Add(new CommandParameter("@MobilNo", dr["MobilNo"]));
                    base.Parameter.Add(new CommandParameter("@MutltFlag", dr["MutltFlag"]));
                    base.Parameter.Add(new CommandParameter("@MutTot", dr["MutTot"]));
                    base.Parameter.Add(new CommandParameter("@NetAsset", dr["NetAsset"]));
                    base.Parameter.Add(new CommandParameter("@NoOfCards", dr["NoOfCards"]));
                    base.Parameter.Add(new CommandParameter("@OcpnDesc", dr["OcpnDesc"]));
                    base.Parameter.Add(new CommandParameter("@OldFlag", dr["OldFlag"]));
                    base.Parameter.Add(new CommandParameter("@Rank", dr["Rank"]));
                    base.Parameter.Add(new CommandParameter("@RiskAttrib", dr["RiskAttrib"]));
                    base.Parameter.Add(new CommandParameter("@RMNum", dr["RMNum"]));
                    base.Parameter.Add(new CommandParameter("@SboxFlag", dr["SboxFlag"]));
                    base.Parameter.Add(new CommandParameter("@SelectNo", dr["SelectNo"]));
                    base.Parameter.Add(new CommandParameter("@ServiceCode1", dr["ServiceCode1"]));
                    base.Parameter.Add(new CommandParameter("@TelDay", dr["TelDay"]));
                    base.Parameter.Add(new CommandParameter("@TelDayExt", dr["TelDayExt"]));
                    base.Parameter.Add(new CommandParameter("@TelNig", dr["TelNig"]));
                    base.Parameter.Add(new CommandParameter("@TelNigExt", dr["TelNigExt"]));
                    base.Parameter.Add(new CommandParameter("@TrialFlag", dr["TrialFlag"]));
                    base.Parameter.Add(new CommandParameter("@TrustOneActual", dr["TrustOneActual"]));
                    base.Parameter.Add(new CommandParameter("@TrustOneAppl", dr["TrustOneAppl"]));
                    base.Parameter.Add(new CommandParameter("@VipCdH", dr["VipCdH"]));
                    base.Parameter.Add(new CommandParameter("@VipCdI", dr["VipCdI"]));
                    base.Parameter.Add(new CommandParameter("@VIPCode", dr["VIPCode"]));
                    base.Parameter.Add(new CommandParameter("@VipDegree", dr["VipDegree"]));
                    base.Parameter.Add(new CommandParameter("@VipDegreeH", dr["VipDegreeH"]));
                    base.Parameter.Add(new CommandParameter("@WmAssetAmt", dr["WmAssetAmt"]));
                    base.Parameter.Add(new CommandParameter("@docNo", docNo));
                    #endregion

                    var s333 = base.ExecuteNonQuery(i60491);





                    // 找出Grp的SNO
                    string sno = "Select SNO,CustomerId from TX_60491_Grp WHERE CaseId='" + VersionNewID + "'";
                    var vsno = base.Search(sno);



                    string sTemplate = @"INSERT INTO TX_60491_Detl (SNO, FKSNO, Account, Branch, StsDesc, ProdCode, ProdDesc, Link, Ccy, Bal, System, SegmentCode, CUST_ID, CaseId,DocNo)  
                                VALUEs (NEXT VALUE FOR dbo.SEQTX_60491_Detl, @FKSNO,@Account, @Branch, @StsDesc, @ProdCode, @ProdDesc, @Link, @Ccy, @Bal, @System, @SegmentCode, @CUST_ID, @CaseId,@docNo);";

                    // 再來取出Details 一筆一筆Insert ...
                    string sql60detail = "Select * from BOPS060490Recv WHERE  VERSIONNEWID='" + VersionNewID + "'";
                    base.Parameter.Clear();
                    var details = base.Search(sql60detail);



                    foreach (DataRow r in details.Rows)
                    {
                        for (int i = 1; i <= 6; i++)
                        {
                            string Account = r["ACCOUNT_" + i].ToString();

                            if (Account != "00000000000000000")
                            {
                                base.Parameter.Clear();
                                base.Parameter.Add(new CommandParameter("@FKSNO", vsno.Rows[0][0]));
                                base.Parameter.Add(new CommandParameter("@CUST_ID", vsno.Rows[0][1]));
                                base.Parameter.Add(new CommandParameter("@CaseId", VersionNewID));
                                base.Parameter.Add(new CommandParameter("@docNo", docNo));
                                base.Parameter.Add(new CommandParameter("@Account", Account));
                                base.Parameter.Add(new CommandParameter("@Branch", r["BRANCH_" + i].ToString()));
                                base.Parameter.Add(new CommandParameter("@StsDesc", r["STS_DESC" + i].ToString()));
                                base.Parameter.Add(new CommandParameter("@ProdCode", r["PROD_CODE" + i].ToString()));
                                base.Parameter.Add(new CommandParameter("@ProdDesc", r["PROD_DESC" + i].ToString()));
                                base.Parameter.Add(new CommandParameter("@Link", r["Link_" + i].ToString()));
                                base.Parameter.Add(new CommandParameter("@Ccy", r["Ccy_" + i].ToString()));
                                base.Parameter.Add(new CommandParameter("@Bal", r["Bal_" + i].ToString()));
                                base.Parameter.Add(new CommandParameter("@System", r["System_" + i].ToString()));
                                base.Parameter.Add(new CommandParameter("@SegmentCode", r["SEGMENT_CODE_" + i].ToString()));

                                base.ExecuteNonQuery(sTemplate);
                            }

                        }

                    }




                    #endregion
                }

            }
            catch (Exception ex)
            {

                WriteLog("Move TX606491 Error " + ex.Message.ToString());
            }
            try
            {

                #region BOPS 搬到TX 401
                // 若有失敗, 則不會寫入BOPS009091Recv .....
                string ch401 = @"Select count(*) from BOPS000401Recv WHERE VersionNewID='" + VersionNewID + "'";
                var r401 = base.Search(ch401);

                if (int.Parse(r401.Rows[0][0].ToString()) > 0)
                {


                    #region sql401 的欄位....
                    string sql401 = @"insert into TX_33401 (
SNO,
CaseId,
AActDegree,
Acct,
AcctStatus1,
AcctStatus2,
ARelationType,
AtmHoldAmt,
AtmWaiveExpDt,
Birthday,
Branch,
BrchName,
CloseDate,
CurBal,
Currency,
CusmOrAAcDegree,
CustId,
ExOdInt,
FaxNo,
FB_TELLER,
GlobalHeadCode,
HighContr,
HoldAmt,
HouseFlag,
HTel,
HTelExt,
IntAcct,
IntAmt,
IntDate,
InterBranch,
InvestCode,
InvestHoldVal,
InvestType,
LastDate,
LgmbFlag,
LonAmt,
LstTax,
MdHoldAmt,
Micr,
MinRepayAmt,
MinRepayDate,
Name,
NbCusm,
NextAmt,
NextCheck,
NpTfrNote,
OpenDate,
OTel,
OtherHoldAmt,
OverAmt,
OverTax,
PbBal,
PortfolioNo,
ProcessStsText,
PublishNo,
Rate,
RollCnt,
SecurityCompNo,
SlyOtherTrfCnt,
SlyOwnTrfCnt,
SlyStfExchgRate,
SlyWaiveTrfCnt,
SlyWaiveWdCnt,
StockHoldAmt,
TdAmt,
TermBasis,
TermDay,
TodayCheck,
TrialFlag,
TrueAmt,
UnclChqUsed,
VipCDI,
VipCode,
VipDegree,
YearAmt,
YearTax,
YearTaxFcy,
cCretDT,
DocNo
) 

Select 
( NEXT VALUE FOR DBO.SEQTX_33401) as sno,
VersionNewID AS CaseId,
A_ACT_DEGREE AS AActDegree,
ACCT_NO AS Acct,
ACCT_STATUS_1 AS AcctStatus1,
ACCT_STATUS_2 AS AcctStatus2,
A_RELATION_TYPE AS ARelationType,
ATM_HOLD_AMT AS AtmHoldAmt,
ATM_WAIVE_EXP_DT AS AtmWaiveExpDt,
BIRTHDAY AS Birthday,
BRANCH_NO AS Branch,
BRCH_NAME AS BrchName,
CLOSE_DATE AS CloseDate,
CUR_BAL AS CurBal,
CURRENCY AS Currency,
CUSM_OR_A_AC_DEGREE AS CusmOrAAcDegree,
CUST_ID_NO AS CustId,
EX_OD_INT AS ExOdInt,
FAX_NO AS FaxNo,
FB_BRANCH AS FB_TELLER,
GLOBAL_HEAD_CODE AS GlobalHeadCode,
HIGH_CONTR AS HighContr,
HOLD_AMT AS HoldAmt,
HOUSE_FLAG AS HouseFlag,
TEL_NIG AS HTel,
H_TEL_EXT AS HTelExt,
INT_ACCT AS IntAcct,
INT_AMT AS IntAmt,
INT_DATE AS IntDate,
INTER_BRANCH AS InterBranch,
INVEST_DESC AS InvestCode,
INVEST_HOLD_VAL AS InvestHoldVal,
INVEST_TYPE AS InvestType,
LAST_DATE AS LastDate,
LGMB_FLAG AS LgmbFlag,
LON_LIMIT AS LonAmt,
LST_TAX AS LstTax,
MD_HOLD_AMT AS MdHoldAmt,
STOCK_MICR_AMT AS Micr,
MIN_REPAY_AMT AS MinRepayAmt,
MIN_REPAY_DATE AS MinRepayDate,
CUSTOMER_NAME AS Name,
NB_CUSM AS NbCusm,
NEXT_AMT AS NextAmt,
NEXTDAY_CHECK AS NextCheck,
NP_TFR_NOTE AS NpTfrNote,
OPEN_DATE AS OpenDate,
TEL_DAY AS OTel,
OTHER_HOLD_AMT AS OtherHoldAmt,
OVER_AMT AS OverAmt,
OVER_TAX AS OverTax,
PB_BAL AS PbBal,
PORTFOLIO_NO AS PortfolioNo,
PROCESS_STS_TEXT AS ProcessStsText,
PUBLISH_NO AS PublishNo,
RATE AS Rate,
ROLL_CNT AS RollCnt,
SECURITY_COMP_NO AS SecurityCompNo,
SLY_OTHER_TRF_CNT AS SlyOtherTrfCnt,
SLY_OWN_TRF_CNT AS SlyOwnTrfCnt,
SLY_STF_EXCHG_RATE AS SlyStfExchgRate,
SLY_WAIVE_TRF_CNT AS SlyWaiveTrfCnt,
SLY_WAIVE_WD_CNT AS SlyWaiveWdCnt,
STOCK_HOLD_AMT AS StockHoldAmt,
TD_AMT AS TdAmt,
TERM_BASIS AS TermBasis,
TERM_DAY AS TermDay,
TODAY_CHECK AS TodayCheck,
TRIAL_FLAG AS TrialFlag,
TRUE_AMT AS TrueAmt,
UNCL_CHQ_USED AS UnclChqUsed,
VIP_CD_I AS VipCDI,
VIP_CODE AS VipCode,
VIP_DEGREE AS VipDegree,
YEAR_AMT AS YearAmt,
YEAR_TAX AS YearTax,
YEAR_TAX_FCY AS YearTaxFcy,
getdate() as cCretDT ,'" +
        docNo
        + "' from BOPS000401Recv WHERE VersionNewID=@VersionNewID";

                    #endregion
                    base.Parameter.Clear();
                    base.Parameter.Add(new CommandParameter("@VersionNewID", VersionNewID));
                    base.ExecuteNonQuery(sql401);


                    //            var sqlErr401 = dtErrorSend.Select("txno='BOPS000401Send' ");
                    //            foreach (DataRow dr3 in sqlErr401)
                    //            {
                    //                string i9091 = @"INSERT INTO TX_33401(cCretDT, CaseId, RepMessage) VALUES 
                    //                    (getDate(),@CaseId, @RepMessage ); ";
                    //                base.Parameter.Clear();
                    //                base.Parameter.Add(new CommandParameter("@CaseId", dr3["NewID"]));
                    //                base.Parameter.Add(new CommandParameter("@RepMessage", dr3["QueryErrMsg"]));

                    //                base.ExecuteNonQuery(i9091);
                    //            }
                }
                #endregion

            }
            catch (Exception ex)
            {
                WriteLog("Move TX33401 Error " + ex.Message.ToString());
            }
            try
            {

                string ch9091 = @"Select count(*) from BOPS009091Recv WHERE VersionNewID='" + VersionNewID + "'";
                var r9091 = base.Search(ch9091);
                 if (int.Parse(r9091.Rows[0][0].ToString()) > 0)
                 {
                     #region BOPS 搬到TX 9091
                     // 若有失敗, 則不會寫入BOPS009091Recv .....
                     string sql9091 = @"SELECT * FROM BOPS009091Recv WHERE VersionNewID='" + VersionNewID + "';";
                     var res9091 = base.Search(sql9091);
                     foreach (DataRow dr1 in res9091.Rows)
                     {
                         string i9091 = @"INSERT INTO TX_09091(cCretDT, CaseId, Account, RESPONSE_OC08, CURRENCY_OC08, CMTRASH_OC08, DESCRIP_OC08, TrnNum, RepMessage,DocNo) VALUES 
                    (getDate(),@CaseId,@Account2,@RESPONSE_OC08, @CURRENCY_OC08, @CMTRASH_OC08, @DESCRIP_OC08, @TrnNum, @RepMessage,@docNo ); ";
                         base.Parameter.Clear();
                         base.Parameter.Add(new CommandParameter("@CaseId", dr1["VersionNewID"]));
                         base.Parameter.Add(new CommandParameter("@Account2", ""));
                         base.Parameter.Add(new CommandParameter("@RESPONSE_OC08", dr1["RESPONSE_OC08"]));
                         base.Parameter.Add(new CommandParameter("@CURRENCY_OC08", dr1["CURRENCY_OC08"]));
                         base.Parameter.Add(new CommandParameter("@CMTRASH_OC08", dr1["CMTRASH_OC08"]));
                         base.Parameter.Add(new CommandParameter("@DESCRIP_OC08", dr1["DESCRIP_OC08"]));
                         base.Parameter.Add(new CommandParameter("@TrnNum", ""));
                         base.Parameter.Add(new CommandParameter("@RepMessage", ""));
                         base.Parameter.Add(new CommandParameter("@docNo", docNo));
                         base.ExecuteNonQuery(i9091);
                     }



                     #endregion
                 }




            }
            catch (Exception ex)
            {
                WriteLog("Move TX 9091 Error " + ex.Message.ToString());
            }




        }

        internal void UpdateWarningDetailsSet_Date(DataRow dr)
        {
            string updateSql = @" UPDate WarningDetails SET SetDate=getdate() where NewID=@NewID";
            base.Parameter.Clear();
            base.Parameter.Add(new CommandParameter("@NewID", dr["NewID"].ToString()));

            base.ExecuteNonQuery(updateSql);
        }
    }
}