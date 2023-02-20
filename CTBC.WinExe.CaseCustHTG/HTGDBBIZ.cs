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

namespace CTBC.WinExe.CaseCustHTG
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
            int formatNum = int.Parse( dr["QueryType"].ToString() );
            string docNo = dr["DocNo"].ToString();

            switch (formatNum)
            {
                case 1:
                case 3:
                case 2:
                case 4:
                    dt = outputF1(dr);

                    if (dt.Rows.Count == 0) // 20220926, 發現若沒有BOPS00401, 則代表無往來.... 只要在CaseCustDetails.EXCEL_FILE='FTP' 標注, 即可產檔...
                    {
                        string detailsid = dr["VersionNewID"].ToString();
                        var sql = @"UPDATE  CaseCustDetails set EXCEL_FILE='FTP'  where DetailsId='" + detailsid + "';";
                        var det = base.ExecuteNonQuery(sql);
                    }
                    else
                    {
                        dt.TableName = "CaseCustOutputF1";
                        InsertIntoTableWithTransaction(dt, null);
                    }

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
            Guid masterid =Guid.Parse( dr["CaseCustMasterId"].ToString());
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
									,CASE WHEN substring(BOPS000401Recv.ACCT_NO,1,4)='0000'
									THEN RIGHT(BOPS000401Recv.ACCT_NO,12)
									ELSE
										Case WHEN BOPS000401Recv.CURRENCY='TWD' 
											THEN substring(BOPS000401Recv.ACCT_NO,LEN(BOPS000401Recv.ACCT_NO)-11 ,12)
											ELSE SUBSTRING(BOPS000401Recv.ACCT_NO,LEN(BOPS000401Recv.ACCT_NO)-14 ,12)  
										END 
									END
									as ACCT_NO --帳號
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
        public bool SendMsgCase(CTCB.NUMS.Library.HTG.HTGObjectMsg obj, string txid, DataRow drApprMsgDefine, string VersionNewID, DataTable CurrencyList,int seq, string ModifiedUser)
        {
            bool bRet = true;

            // log
            WriteLog("DetailsId = [" + VersionNewID + "], TxID=[" + txid + "]");

            try
            {
                #region 刪除舊的 BOPS081019

                // 20221219, 發現歷史交易 81019有時沒打到,是以下把非本案例的81019全部砍掉....

                //string sql = @"DELETE FROM BOPS081019Recv WHERE SendNewID NOT IN (SELECT distinct NewID FROM BOPS060490Send);";
                //sql += @"DELETE FROM BOPS081019Send WHERE BOPS060490SendNewID NOT IN (SELECT NewID FROM BOPS060490Send);";

                //Parameter.Clear();
                //ExecuteNonQuery(sql);

                #endregion

                // 撈取HTG參數
                DataTable dtHtgData = OpenDataTable("select * from ApprMsgKey where VersionNewID = '" + VersionNewID + "'");

                //20220923, 發現ApprMsgKey的VersionNewID, 會重覆, 造成抓錯人, 所以多加 發查人條件 來過濾, 並用最後一筆的發查人為主, 去打電文...
                if (dtHtgData.Rows.Count != 1)
                {
                    if (dtHtgData.Rows.Count <= 0)
                    {
                        WriteLog("DetailsId = [" + VersionNewID + "], TxID=[" + txid + "],無發查資料！");

                        return false;
                    }
                    else
                    {
                        if (dtHtgData.Rows.Count > 1) // 表示有多個, 要用ModifiedUser來過濾....
                        {
                            foreach (DataRow dr in dtHtgData.Rows)                            
                            {
                                var apprUser = Decode(dr["MsgKeyLU"].ToString()).Trim();
                                if (apprUser == ModifiedUser.Trim()) // 用相同使用者來過濾正確的發查人
                                {
                                    _ldapuid = Decode(dr["MsgKeyLU"].ToString());
                                    _ldappwd = Decode(dr["MsgKeyLP"].ToString());
                                    _racfuid = Decode(dr["MsgKeyRU"].ToString());
                                    _racfowd = Decode(dr["MsgKeyRP"].ToString());
                                    _racfbranch = Decode(dr["MsgKeyRB"].ToString());
                                }
                            }

                        }
                    }
                } 
                else // 只有一個VersionNewID 所以直接取row 0
                {
                    _ldapuid = Decode(dtHtgData.Rows[0]["MsgKeyLU"].ToString());
                    _ldappwd = Decode(dtHtgData.Rows[0]["MsgKeyLP"].ToString());
                    _racfuid = Decode(dtHtgData.Rows[0]["MsgKeyRU"].ToString());
                    _racfowd = Decode(dtHtgData.Rows[0]["MsgKeyRP"].ToString());
                    _racfbranch = Decode(dtHtgData.Rows[0]["MsgKeyRB"].ToString());
                }


                //WriteLog("======================================> " + txid + "\t使用帳戶" + _ldapuid);





                _htgurl = ConfigurationManager.AppSettings["HTGUrl"];
                _applicationid = ConfigurationManager.AppSettings["HTGApplication"];



                WriteLog("撈取下行儲存TABLE結構！");

                // 撈取下行儲存TABLE結構
                DataTable dtRecvData = OpenDataTable("select * from BOPS" + txid + "Recv where 1=2");

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

                    //  測試
                    string IsnotTest = ConfigurationManager.AppSettings["IsnotTest"].ToString();

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


                    //if (txid == "081019")
                    //{
                    //    result = true;
                    //}

                    // 20220211, 發現, 打67050時, 會提示.. "客戶尚未簽定存款開戶總約定書  "
                    if (txid == "067050" )
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
                    if (txid == "000401" && seq==0 )
                    {
                        //20230103, 若尚未開戶, 則會出現: "提示代碼:0169 提示原因:檢查碼錯誤", 則在輸出報表時, 要顯示... "非本行存款帳號:

                        if ((htreturn["PopMessage"].ToString().Contains("檢查碼錯誤")))
                        {
                            result = false;
                            // 刪除掉由啟動發查產生的BOPS067050Send, BOPS060628Send的那筆..
                            var ret = deleteBOPS67050_60628(VersionNewID);
                        }



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
                            EditCaseCustQueryVersion(VersionNewID, "6", strMsg);
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

                                if( seq==0 && txid=="000401")
                                {
                                    #region 若第一支是打401, 則... 要扣60750 及60628的 押回401所剛剛取得的 身份證字號


                                    // 先取得401產出的結果...
                                    try
                                    {
                                        string get401Recv = "select * from BOPS000401Recv where VersionNewID='" + VersionNewID.ToString() + "';";
                                        var Recv401 = base.Search(get401Recv);


                                        if (Recv401 != null)
                                        {
                                            //20220719, 發現若QueryType=4時, 打401若是回應重號, 則要把重號砍掉...
                                            string Cust_ID_No = Recv401.Rows[0]["CUST_ID_NO"].ToString();
                                            if (Cust_ID_No.Trim().Length > 10) // 表示個人
                                                Cust_ID_No = Cust_ID_No.Substring(0, 10);
                                            else
                                            {
                                                if (Cust_ID_No.Trim().Length < 10) // 表示法人...
                                                {
                                                    Cust_ID_No = Cust_ID_No.Substring(0, 8);
                                                }
                                            }

                                            Cust_ID_No = Cust_ID_No.Trim();

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
                                        WriteLog("在401回傳結果中, 找不到身份證字號    VersionNewID = [" + VersionNewID + "], TxID=[" + txid + "],發查失敗！" );
                                    }
                                    #endregion

                                }







                                // 060628 發查成功，將
                                if (txid == "060628")
                                {
                                    Insert60628Send(VersionNewID, dt, txid, dtData.Rows[n]["NewID"].ToString());
                                }
                                else if (txid == "060490")
                                {
                                    //insert BOPS000401Send （ACCOUNT_1_1 ~ ACCOUNT_1_6）
                                    Insert000401Send(VersionNewID, dt, txid, dtData.Rows[n]["NewID"].ToString(), CurrencyList, _racfbranch, _racfuid.Substring(1, _racfuid.Length - 1));

                                    if (dt.Columns.Contains("ACCOUNT_6") && dt.Rows[0]["ACCOUNT_6"].ToString().Trim() != "" && dt.Rows[0]["ACCOUNT_6"].ToString().Trim() != "00000000000000000")
                                    {
                                        // 查詢491的電文設定檔
                                        DataTable dtDefineBy60491 = GetApprMsgDefineBy60491();

                                        if (dtDefineBy60491 != null && dtDefineBy60491.Rows.Count > 0)
                                        {
                                            strReturn = Send60491Start(obj, dtDefineBy60491.Rows[0], VersionNewID, dtData.Rows[n]["NewID"].ToString(), CurrencyList, ModifiedUser);
                                        }
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
                                EditCaseCustQueryVersion(VersionNewID, "6", strMsg);

                                WriteLog("VersionNewID = [" + VersionNewID + "], TxID=[" + txid + "],發查失敗，信息：" + strMsg);
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
                                EditCaseCustQueryVersion(VersionNewID, "6", strMsg);

                                bRet = false;

                                WriteLog("VersionNewID = [" + VersionNewID + "], TxID=[" + txid + "],發查失敗，信息：" + strMsg);
                            }
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

            var uSql = @"delete from  BOPS067050Send where VersionNewID = '" + VersionNewId + "';delete  from  BOPS060628Send where  VersionNewID = '"+ VersionNewId + "' ";

            var uSqlResult = ExcuteSQL(uSql);


            return uSqlResult > 0 ? true :false;

        }

        public string Send60491Start(CTCB.NUMS.Library.HTG.HTGObjectMsg obj, DataRow drApprMsgDefine, string VersionNewID, string NewID060490, DataTable CurrencyList,string ModifiedUser)
        {
            string strReturn = Send60491MsgCase(obj, drApprMsgDefine, VersionNewID, NewID060490, CurrencyList, ModifiedUser);
            if (strReturn == "Next")
            {
                Send60491Start(obj, drApprMsgDefine, VersionNewID, NewID060490, CurrencyList, ModifiedUser);
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
        public string Send60491MsgCase(CTCB.NUMS.Library.HTG.HTGObjectMsg obj, DataRow drApprMsgDefine, string VersionNewID, string NewID060490, DataTable CurrencyList, string ModifiedUser)
        {
            string bRet = "";

            // log
            WriteLog("Send60491MsgCase：VersionNewID = [" + VersionNewID + "], TxID=[060491]");

            try
            {
                // 撈取HTG參數
                DataTable dtHtgData = OpenDataTable("select * from ApprMsgKey where VersionNewID = '" + VersionNewID + "'");

                //20220923, 發現ApprMsgKey的VersionNewID, 會重覆, 造成抓錯人, 所以多加 發查人條件 來過濾, 並用最後一筆的發查人為主, 去打電文...
                if (dtHtgData.Rows.Count != 1)
                {
                    if (dtHtgData.Rows.Count <= 0)
                    {
                        WriteLog("Send60491MsgCase：VersionNewID = [" + VersionNewID + "], TxID=[060491],無發查資料！");

                        return "ErrorSetting";
                    }
                    else
                    {
                        if (dtHtgData.Rows.Count > 1) // 表示有多個, 要用ModifiedUser來過濾....
                        {
                            foreach (DataRow dr in dtHtgData.Rows)
                            {
                                var apprUser = Decode(dr["MsgKeyLU"].ToString()).Trim();
                                if (apprUser == ModifiedUser.Trim()) // 用相同使用者來過濾正確的發查人
                                {
                                    _ldapuid = Decode(dr["MsgKeyLU"].ToString());
                                    _ldappwd = Decode(dr["MsgKeyLP"].ToString());
                                    _racfuid = Decode(dr["MsgKeyRU"].ToString());
                                    _racfowd = Decode(dr["MsgKeyRP"].ToString());
                                    _racfbranch = Decode(dr["MsgKeyRB"].ToString());
                                }
                            }

                        }
                    }
                }
                else // 只有一個VersionNewID 所以直接取row 0
                {
                    _ldapuid = Decode(dtHtgData.Rows[0]["MsgKeyLU"].ToString());
                    _ldappwd = Decode(dtHtgData.Rows[0]["MsgKeyLP"].ToString());
                    _racfuid = Decode(dtHtgData.Rows[0]["MsgKeyRU"].ToString());
                    _racfowd = Decode(dtHtgData.Rows[0]["MsgKeyRP"].ToString());
                    _racfbranch = Decode(dtHtgData.Rows[0]["MsgKeyRB"].ToString());
                }






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
                        EditCaseCustQueryVersion(VersionNewID, "6", strMsg);
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
                            EditCaseCustQueryVersion(VersionNewID, "6", strMsg);
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
                        EditCaseCustQueryVersion(VersionNewID, "6", strMsg);
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
                string sql = "SELECT  * FROM ApprMsgDefine WHERE MsgName = '060491' AND MsgType= 'A1';";
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
        public bool EditCaseCustQueryVersion(string strVersionNewID, string strHTGSendStatus, string strHTGQryMessage)
        {
            string sql = @"UPDATE [CaseCustDetails] 
                            SET 
                                [HTGSendStatus] = @HTGSendStatus,
                                [HTGQryMessage] = @HTGQryMessage,
                                [ModifiedDate] = GETDATE(), 
                                [ModifiedUser] = 'SYSTEM'
                             WHERE DetailsId = @NewID";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("@NewID", strVersionNewID));
            Parameter.Add(new CommandParameter("@HTGSendStatus", strHTGSendStatus));
            Parameter.Add(new CommandParameter("@HTGQryMessage", strHTGQryMessage));
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

        public void Insert60628Send(string VersionNewID, DataTable dt, string txid, string BOPS060628SendNewID)
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
                    string insert60628Sql = "";

                    base.Parameter.Clear();

                    List<string> doubleIDs = new List<string>();

                    for (int i = 0; i < 12; i++)
                    {
                        string strColumnName = "AC_MARK_" + (i + 1).ToString();

                        string CustIdNo = "ID_DATA_" + (i + 1).ToString();


                        // 20220826, 交易明細的電文, 不需管是否結清.. 都要打.. 所以不用管AC_MARK 這個欄位...
                        //if (dt.Columns.Contains(strColumnName) && dt.Rows[0][strColumnName].ToString().Trim() == "Y" && CustIdNo != "")
                        if (CustIdNo != "" && ! string.IsNullOrEmpty(dt.Rows[0][CustIdNo].ToString().Trim()))
                        {

                            // AC_MARK_1 ~AC_MARK_12 如果為Y,則
                            //insert BOPS060490Send （ID_DATA_1 ~ ID_DATA_12）
                            //SA:活存; CA:支存; TD:定存
                            // 20200719, 集作說, 打60490時, 要包括結清案件, 所以AccountSatus 要改為0,
                            #region 組sql
                            insert60628Sql += @"
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
                                                    ,'" + BOPS060628SendNewID + @"'
                                                    ,'" + dt.Rows[0][CustIdNo].ToString().Trim() + @"'
                                                    ,'01'
                                                    ,''
                                                    ,'S'
                                                    ,'0' 
                                                    ,(select top 1 QDateS from  CaseCustDetails where DetailsId ='" + VersionNewID + @"')   
                                                    ,'SA'
                                                    ,'0'
                                                    ,'CSFS.HTG'
                                                    ,GETDATE()
                                                    ,'CSFS.HTG'
                                                    ,GETDATE()
                                                    ,'02'
                                                    ); ";

                            insert60628Sql += @"
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
                                                    ,'" + BOPS060628SendNewID + @"'
                                                    ,'" + dt.Rows[0][CustIdNo].ToString().Trim() + @"'
                                                    ,'01'
                                                    ,''
                                                    ,'S'
                                                    ,'0'
                                                    ,(select top 1 QDateS from  CaseCustDetails where DetailsId ='" + VersionNewID + @"')
                                                    ,'CA'
                                                    ,'0'
                                                    ,'CSFS.HTG'
                                                    ,GETDATE()
                                                    ,'CSFS.HTG'
                                                    ,GETDATE()
                                                    ,'02'
                                                    ); ";

                            insert60628Sql += @"
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
                                                    ,'" + BOPS060628SendNewID + @"'
                                                    ,'" + dt.Rows[0][CustIdNo].ToString().Trim() + @"'
                                                    ,'01'
                                                    ,''
                                                    ,'S'
                                                    ,'0'
                                                    ,(select top 1 QDateS from  CaseCustDetails where DetailsId ='" + VersionNewID + @"')
                                                    ,'TD'
                                                    ,'0'
                                                    ,'CSFS.HTG'
                                                    ,GETDATE()
                                                    ,'CSFS.HTG'
                                                    ,GETDATE()
                                                    ,'02'
                                                    ); ";
                            #endregion


                            #region 若是重號, 則還要把重號的ID, Insert 到CaseCustNewRFDMSend ...
                            // 若是CUST_ID_NO = ID_DATA_1 或ID_DATA_2 ... 表示, 是本號, 則不新增....
                            if (dt.Rows[0]["CUST_ID_NO"].ToString() == dt.Rows[0][CustIdNo].ToString())
                            {
                                // 表示這個重號, 為本號, (在頁面上, 已經新增RFDM了, 所以不需新增
                            }
                            else
                            {
                                doubleIDs.Add(dt.Rows[0][CustIdNo].ToString().Trim());
                            }


                            #endregion

                        }
                    }

                    //20181228 固定變更 add start
                    if (!string.IsNullOrEmpty(insert60628Sql))
                    {
                        base.ExecuteNonQuery(insert60628Sql, dbTransaction, false);
                    }

                    dbTransaction.Commit();

                    if (insert60628Sql != "")
                    {
                        WriteLog("VersionNewID = [" + VersionNewID + "], TxID=[" + txid + "],資料寫入Table: " + "BOPS" + txid + "Send");
                    }

                    if (doubleIDs.Count() > 0)
                    {

                        // 取得TRNNum 最大號....
                        // 查詢本月最大的流水號
                        string pMaxTrnNum = GetMaxTrnNum();

                        // 流水號變量
                        int pTrnNum = 0;

                        // 截取流水號
                        if (pMaxTrnNum != "")
                        {
                            pTrnNum = Convert.ToInt32(pMaxTrnNum.Substring(12, 5));
                        }
                        
                        // 讀取.. CaseCustNewRFDMSend 那三筆, 把讀出來, 然後, 改TrnNum 跟 ID_NO
                        string sqlSQ = @"SELECT * FROM CaseCustNewRFDMSend WHERE VersionNewID=@VersionNewID";

                        Parameter.Clear();
                        Parameter.Add(new CommandParameter("@VersionNewID", VersionNewID));
                        DataTable dtNewRFDM = base.Search(sqlSQ);

                        if (dtNewRFDM.Rows.Count > 0)
                        {
                            string rfdmsql = "INSERT INTO CaseCustNewRFDMSend(TrnNum, VersionNewID, ID_No, Acct_No, Start_Jnrst_Date, End_Jnrst_Date, Type, RFDMSendStatus, RspCode, RspMsg, FileName, CreatedDate, CreatedUser, ModifiedDate, ModifiedUser, AcctDesc, Curr, Channel) VALUES (@TrnNum,@VersionNewID,@ID_No,@Acct_No,@Start_Jnrst_Date,@End_Jnrst_Date,@Type,@RFDMSendStatus,@RspCode,@RspMsg,@FileName,getdate(),@CreatedUser,getdate(),@ModifiedUser,@AcctDesc,@Curr,@Channel);";

                            dbTransaction = dbConnection.BeginTransaction();
                            foreach(var idNo in doubleIDs) {
                                foreach (DataRow dr in dtNewRFDM.Rows)
                                {
                                    string newTrnNum = "THPO" + CalculateTrnNum(pTrnNum);
                                    Parameter.Clear();
                                    Parameter.Add(new CommandParameter("@TrnNum", newTrnNum));
                                    Parameter.Add(new CommandParameter("@VersionNewID", dr["VersionNewID"]));
                                    Parameter.Add(new CommandParameter("@ID_No", idNo));
                                    Parameter.Add(new CommandParameter("@Acct_No", dr["Acct_No"]));
                                    Parameter.Add(new CommandParameter("@Start_Jnrst_Date", dr["Start_Jnrst_Date"]));
                                    Parameter.Add(new CommandParameter("@End_Jnrst_Date", dr["End_Jnrst_Date"]));
                                    Parameter.Add(new CommandParameter("@Type", dr["Type"]));
                                    Parameter.Add(new CommandParameter("@RFDMSendStatus", dr["RFDMSendStatus"]));
                                    Parameter.Add(new CommandParameter("@RspCode", dr["RspCode"]));
                                    Parameter.Add(new CommandParameter("@RspMsg", dr["RspMsg"]));
                                    Parameter.Add(new CommandParameter("@FileName", dr["FileName"]));
                                    Parameter.Add(new CommandParameter("@CreatedUser", dr["CreatedUser"]));
                                    Parameter.Add(new CommandParameter("@ModifiedUser", dr["ModifiedUser"]));
                                    Parameter.Add(new CommandParameter("@AcctDesc", dr["AcctDesc"]));
                                    Parameter.Add(new CommandParameter("@Curr", dr["Curr"]));
                                    Parameter.Add(new CommandParameter("@Channel", dr["Channel"]));
                                    base.ExecuteNonQuery(rfdmsql, dbTransaction, false);
                                    pTrnNum++;
                                }
                            }

                            dbTransaction.Commit();
                            WriteLog("VersionNewID = [" + VersionNewID + "], 有重號: " + string.Join(",",doubleIDs) + ", 新增寫入Table: CaseCustNewRFDMSend");
                            
                        }
                        
                    }



                    //20181228 固定變更 add end

                    

                }
                catch (Exception ex)
                {
                    dbTransaction.Rollback();

                    throw ex;
                }
            }

        }

        /// <summary>
        /// 計算流水號
        /// YYYMMDD+4位數流水號
        /// </summary>
        /// <param name="strMaxNo">根據最大流水號+1(純數字流水號)</param>
        /// <returns></returns>
        private string CalculateTrnNum(int strMaxNo)
        {
            return DateTime.Now.ToString("yyyyMMdd") + String.Format("{0:D5}", strMaxNo + 1);
        }

        public string GetMaxTrnNum()
        {
            try
            {

                string sql = @"
                            select 
                            	isnull(MAX(TrnNum),'') as TrnNumMax 
                            from CaseCustNewRFDMSend
                            where TrnNum like 'THPO" + DateTime.Now.ToString("yyyyMM") + "%' ";

                DataTable dt = base.Search(sql);

                if (dt != null && dt.Rows.Count > 0)
                {
                    return dt.Rows[0]["TrnNumMax"].ToString();
                }
                else
                {
                    return "";
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
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
                string strDataSql = @"
                    select NewID,QueryErrMsg from BOPS067050Send WHERE VersionNewID = @VersionNewID and  sendstatus='03'
                    union
                    select NewID,QueryErrMsg from BOPS060628Send WHERE VersionNewID = @VersionNewID and  sendstatus='03'
                    union
                    select NewID,QueryErrMsg from BOPS060490Send WHERE VersionNewID = @VersionNewID and  sendstatus='03'
                    union
                    select NewID,QueryErrMsg from BOPS000401Send WHERE VersionNewID = @VersionNewID and  sendstatus='03'
                    union
                    select NewID,QueryErrMsg from BOPS081019Send WHERE VersionNewID = @VersionNewID and  sendstatus in ('03','04') and ( QueryErrMsg<>'提示代碼:1574 提示原因:申請資料已存在，請至８１０２０查詢維護 ')
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
        public DataTable GetSuccessSend(string VersionNewID, int iType)
        {
            try
            {
                string strDataSql = @"
                    select NewID from BOPS067050Send WHERE VersionNewID = @VersionNewID and  sendstatus IN ('02', '03')
                    union
                    select NewID from BOPS060628Send WHERE VersionNewID = @VersionNewID and  sendstatus IN ('02', '03')
                    union
                    select NewID from BOPS060490Send WHERE VersionNewID = @VersionNewID and  sendstatus IN ('02', '03')
                    union
                    select NewID from BOPS000401Send WHERE VersionNewID = @VersionNewID and  sendstatus IN ('02', '03')                    
                ";

//                if( iType==2 || iType==4)
//                {
//                    strDataSql += @"union
//                    select NewID from BOPS081019Send WHERE VersionNewID = @VersionNewID and  (sendstatus IN ('02', '03', '04') or (sendstatus = '01' and ATMFlag = 'N'))";
//                }

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
            string idNo="";
            if (ret.Rows.Count >0 )
            {                
                idNo = ret.Rows[0][0].ToString().Trim();
                if (idNo.Length > 10)
                    idNo = idNo.Substring(0, 10);
                else
                    idNo = idNo.Trim();
                string sql = @"UPDATE CaseCustNewRFDMSend Set ID_NO=@idno WHERE VersionNewID=@caseId and isnull(ID_NO,'')=''";
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@caseId", caseId));
                base.Parameter.Add(new CommandParameter("@idno", idNo));

                base.ExecuteNonQuery(sql);
            }



        }

        internal void EditCaseCustDetailsIDNo(string caseId)
        {
            string retSql = "select TOP 1 CUST_ID_NO  from BOPS000401Recv where versionNewID=@caseId";
            base.Parameter.Clear();
            base.Parameter.Add(new CommandParameter("@caseId", caseId));
            var ret = base.Search(retSql);
            string idNo = "";
            if (ret.Rows.Count > 0 )
            {
                idNo = ret.Rows[0][0].ToString().Trim();
                if (idNo.Length > 10)
                    idNo = idNo.Substring(0, 10);
                else
                    idNo = idNo.Trim();
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
    }
}
