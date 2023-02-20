using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Data;
using System.Reflection;


namespace CTBC.WinExe.CaseCustHTG
{
    class Program 
    {
        public string _taskname = "CTBC.WinExe.CaseCustHTG";

        bool boolRacf = true; // 用於測試RACF帳密, 是否有效.. 若一但無效. 則停止所有發查動作....

        HTGDBBIZ _HTGDBBIZ = new HTGDBBIZ();

        static void Main(string[] args)
        {

            Program mainProgram = new Program();
            string dt = DateTime.Now.ToString("yyyy-MM-dd");
            if (args.Count() > 0)
                dt = args[0];


            mainProgram.Process(dt);
            //mainProgram.testInsert();
        }

//        private void testInsert()
//        {
//            string strDataSql = @"SELECT CaseCustMaster.DocNo, DetailsId as VersionNewID, CaseCustDetails.Status,CaseCustMasterId, QueryType, CustIdNo,QDateS,QDateE 
//	FROM CaseCustDetails inner join CaseCustMaster on CaseCustDetails.CaseCustMasterId = CaseCustMaster.NewId
//	where DetailsId='195B9802-1E5C-4E09-AF17-250F34AD9A19'";
//            DataTable dtData = _HTGDBBIZ.OpenDataTable(strDataSql);
            

//            var ret = _HTGDBBIZ.InsertOutputFormat(dtData.Rows[0]);
//        }


        private void Process(string pDate)
        {
            log4net.Config.XmlConfigurator.Configure(new System.IO.FileInfo(AppDomain.CurrentDomain.BaseDirectory + @"\Log.config"));
            // 先去找出今日要打的電文....
            _HTGDBBIZ.WriteLog("目前處理 " + pDate + "案件");




            // 20211216, 增加判斷, 是否前一次查HTG, 是否完成.....若未完成,則跳離....
            //string isRuning = @"SELECT Count(*) FROM CaseCustMaster m inner join CaseCustDetails d on m.NewID= d.CaseCustMasterId WHERE convert(varchar, m.ModifiedDate, 23)='" + pDate + "' AND m.STATUS='02' AND d.EXCEL_FILE='R';";
            //var dtIsRunning = _HTGDBBIZ.OpenDataTable(isRuning);
            //if (int.Parse(dtIsRunning.Rows[0][0].ToString()) > 0)
            //{
            //    _HTGDBBIZ.WriteLog("\r\n\r\n==========================   目前有前次HTG程式在處理中....  取消本次執行  ==========================\r\n\r\n");
            //    return;
            //}


            var sql = @"SELECT distinct d.QueryType FROM CaseCustMaster m  inner join CaseCustDetails d  on m.NewID = d.CaseCustMasterId
                            WHERE m.STATUS in ('02','06') AND d.Status in ('02','06') AND HTGSendStatus='2' AND QueryType in ('1','2','3','4') ;";  
            var details = _HTGDBBIZ.OpenDataTable(sql);
            if( details.Rows.Count >0 )
            {
                // CaseCustDetails.EXCEL_FILE 壓成'R'
//                var uSql = @"update CaseCustDetails Set EXCEL_FILE='R' where CaseCustMasterId in (
//	                        SELECT m.NewID FROM CaseCustMaster m  inner join CaseCustDetails d  on m.NewID = d.CaseCustMasterId
//	                        WHERE convert(varchar, m.ModifiedDate, 23)='" + pDate + "' AND m.STATUS in ('02','06') )";

                //var uSqlResult = _HTGDBBIZ.ExcuteSQL(uSql);

                //if( uSqlResult == 0)
                //{
                //    _HTGDBBIZ.WriteLog("\r\n\r\n==========================   目前無法標注執行中.......  取消本次執行  ==========================\r\n\r\n");
                //    return;
                //}


                foreach(DataRow dr in details.Rows)
                {
                    int qType = int.Parse(dr[0].ToString());
                    if( qType>=1 && qType < 5)
                        ProcessType(qType);
                }


                // CaseCustDetails.EXCEL_FILE 壓成 NULL, ==> 表示已執行完成
//                var uSql2 = @"update CaseCustDetails Set EXCEL_FILE=null where CaseCustMasterId in (
//	                        SELECT m.NewID FROM CaseCustMaster m  inner join CaseCustDetails d  on m.NewID = d.CaseCustMasterId
//	                        WHERE convert(varchar, m.ModifiedDate, 23)='" + pDate + "' AND m.STATUS in ('02','06','04','08') )";

//                var uSqlResult2 = _HTGDBBIZ.ExcuteSQL(uSql2);

            }
            else
            {
                _HTGDBBIZ.WriteLog("今日無案件");
            }
        }

        private void ProcessType(int iType)
        {
            #region 收發電文處理


            string strType = "A" + iType.ToString();
            try
            {
                // log.config
                

                // log 記錄
                _HTGDBBIZ.WriteLog(_taskname + " Service Start");

                // log 記錄
                _HTGDBBIZ.WriteLog(_taskname + " Get ApprMsgDefine setting");

                // 電文設定
                DataTable m_dtApprMsgDefine = _HTGDBBIZ.OpenDataTable(@" SELECT
            	                                                                  MsgName
                                                                                  ,MsgDesc
                                                                                  ,SendOrder
                                                                                  ,SendFlag
                                                                                  ,SourceTableName
                                                                                  ,SignOnLU0
                                                                                  ,MultiPages
                                                                                  ,PostTransactionXml
                                                                                  ,PostTransactionDataSql
                                                                                  ,PreMsgName
                                                                                  ,MsgType
                                                                                  ,PostMsgName
                                                                                  ,OkOutputCode
                                                                              FROM dbo.ApprMsgDefine
                                                                             where SendFlag = 'Y' and MSGType ='" + strType + "' order by SendOrder asc");


                _HTGDBBIZ.WriteLog(_taskname + " ApprMsgDefine settings count = [" + m_dtApprMsgDefine.Rows.Count.ToString() + "]");

                // 無發送電文設定
                if (m_dtApprMsgDefine.Rows.Count == 0)
                {
                    _HTGDBBIZ.WriteLog("No ApprMsgDefine setting");
                    return;
                }

                // 查詢要發送電文的案件編號
                //string strDataSql = "SELECT DetailsId as VersionNewID, Status FROM CaseCustDetails WHERE 1 = 2 ";
                string strDataSql = "SELECT DetailsId as VersionNewID, Status,CaseCustMasterId, QueryType, CustIdNo,QDateS,QDateE,DocNo,CustAccount, ModifiedUser FROM CaseCustDetails WHERE QueryType = '" + iType.ToString() + "'and HTGSendStatus='2'  and (";

                for (int i = 0; i < m_dtApprMsgDefine.Rows.Count; i++)
                {
                    strDataSql += " DetailsId in ( select VersionNewID from " + m_dtApprMsgDefine.Rows[i]["SourceTableName"].ToString().Trim() + " where sendstatus = '02') or";
                }

                strDataSql = strDataSql.Substring(0, strDataSql.Length - 2) + " )";

                DataTable dtData = _HTGDBBIZ.OpenDataTable(strDataSql);
                dtData.TableName = "CaseCustDetails";

                // 無案件需送電文
                if (dtData.Rows.Count == 0)
                {
                    _HTGDBBIZ.WriteLog("No data to send");
                    return;
                }

                // 查詢代碼檔的幣別， 401Send檔，依據賬號找到對應的幣別使用
                DataTable CurrencyList = GetParmCodeCurrency();

                
                // 案件逐筆發送電文
                for (int i = 0; i < dtData.Rows.Count; i++)
                {
                    //// 20220207, 若曾經有RACF 初始化失敗, 則都不打電文
                    //if (!boolRacf)
                    //{
                    //    _HTGDBBIZ.WriteLog(" DetailsId = [" + dtData.Rows[i]["VersionNewID"].ToString() + "] RACF 帳密初始化錯誤");
                    //    break;
                    //}
                    ////20220207, 先判斷這個RACF帳密/是否過期? 若過期, 則寫LOG, 並停止執行...以免被鎖住...
                    //var checkRacfResult = _HTGDBBIZ.checkRacf(dtData.Rows[i]["VersionNewID"].ToString());
                    //if( ! checkRacfResult)
                    //{
                    //    boolRacf = false;
                    //    _HTGDBBIZ.WriteLog("\r\n======================================RACF 帳密初始化錯誤===================================\r\n");
                    //    _HTGDBBIZ.WriteLog("==========================================直接不打本案其他案件================================\r\n");
                    //    break; // 離開本案
                    //}


                    //bool bRet = true;
                    CTCB.NUMS.Library.HTG.HTGObjectMsg obj = new CTCB.NUMS.Library.HTG.HTGObjectMsg();

                    // 按電文設定，依次發送電文
                    for (int j = 0; j < m_dtApprMsgDefine.Rows.Count; j++)
                    {
                        _HTGDBBIZ.WriteLog(_taskname + " Msg name = [" + m_dtApprMsgDefine.Rows[j]["MsgName"].ToString() + "]" + " DetailsId = [" + dtData.Rows[i]["VersionNewID"].ToString() + "]");

                        _HTGDBBIZ.SendMsgCase(obj, m_dtApprMsgDefine.Rows[j]["MsgName"].ToString(), m_dtApprMsgDefine.Rows[j], dtData.Rows[i]["VersionNewID"].ToString(), CurrencyList, j, dtData.Rows[i]["ModifiedUser"].ToString());
                    }

                    // 查詢案件下是否有異常的電文
                    DataTable dtErrorSend = _HTGDBBIZ.GetErrorSend(dtData.Rows[i]["VersionNewID"].ToString());

                    if (dtErrorSend != null && dtErrorSend.Rows.Count > 0)
                    {
                        // 案件如果是拋查中, 並且案件下有異常的HTG發查,將主檔狀態改爲 失敗
                        if (dtData.Rows[i]["Status"].ToString() == "02")
                        {                            
                            _HTGDBBIZ.UpdateCaseCustDetailStatus("04", dtData.Rows[i]["VersionNewID"].ToString());
                        }
                        // 案件如果是重查拋查中, 並且案件下有異常的HTG發查,將主檔狀態改爲 重查失敗
                        else if (dtData.Rows[i]["Status"].ToString() == "06")
                        {
                            _HTGDBBIZ.UpdateCaseCustDetailStatus("08", dtData.Rows[i]["VersionNewID"].ToString());
                        }

                        _HTGDBBIZ.EditCaseCustQueryVersion(dtData.Rows[i]["VersionNewID"].ToString(), "49", "");
                        // 2021-11-24, 將所有的錯誤訊息, 寫回CaseCustDetails.[ErrorMessage] 這欄位中
                        //20221004, 要將錯誤訊息.. distinct 
                        List<string> errMessages = new List<string>();
                        foreach (DataRow dr in dtErrorSend.Rows)
                        {
                            var res= errMessages.FirstOrDefault(x => x == dr["QueryErrMsg"].ToString());
                            if ( res==null)
                            {
                                errMessages.Add(dr["QueryErrMsg"].ToString());
                            }                            
                        }
                        if( errMessages.Count()>0)
                            _HTGDBBIZ.UpdateDetailsErrorMessage(dtData.Rows[i]["VersionNewID"].ToString(), string.Join(";", errMessages));
                    }
                    else
                    {
                        // 查詢是否有02,03 的案件，若沒有代表都有案件都成功
                        DataTable dtSuccessSend = _HTGDBBIZ.GetSuccessSend(dtData.Rows[i]["VersionNewID"].ToString(), iType);

                        if (dtSuccessSend == null || dtSuccessSend.Rows.Count == 0)
                        {
                            // 將Version 檔 HTG的狀態更新為成功
                            _HTGDBBIZ.EditCaseCustQueryVersion(dtData.Rows[i]["VersionNewID"].ToString(), "4", "");

                        }
                    }

                    //2022-02-10, 不管成功或失敗, 只要是QueryType = 3, or 4 , 都回寫到Details 中去....
                    // 2021-11-24, 新增, 當Type 3, Type 4 時, 只有給帳號, 但ESB打的時候要有身份證字號.....所以在此, 要在CastCustNewRFDMSend 那筆去補身份證字號..
                    if (iType == 3 || iType == 4)
                    {
                        _HTGDBBIZ.EditCaseCustNewRFDMSendIDNo(dtData.Rows[i]["VersionNewID"].ToString());
                    }
                    // 2021-12-17, 新增, 當Type 3, Type 4 時, 只有給帳號, 但產畫面時, 需要回補身份證字號到 CaseCustDetails.CustIdNo  那筆去補身份證字號..
                    if (iType == 3 || iType == 4)
                    {
                        _HTGDBBIZ.EditCaseCustDetailsIDNo(dtData.Rows[i]["VersionNewID"].ToString());
                    }

                    // 若是Type = 1 (A1) , 則儲存至CaseCustOutputF1 的TABLE
                    var ret = _HTGDBBIZ.InsertOutputFormat(dtData.Rows[i]);
                    if (ret)
                    {
                        _HTGDBBIZ.WriteLog("儲存成功!!" + dtData.Rows[i]["VersionNewID"] + " QueryType " + dtData.Rows[i]["Status"]);
                    }
                    else
                    {
                        _HTGDBBIZ.WriteLog("儲存失敗!!" + dtData.Rows[i]["VersionNewID"] + " QueryType " + dtData.Rows[i]["Status"]);
                    }

                }
            }
            catch (Exception ex)
            {
                _HTGDBBIZ.WriteLog("程式異常，錯誤信息：" + ex.Message);
            }

            #endregion
        }




        /// <summary>
        /// 獲取 幣別代碼檔
        /// </summary>
        /// <returns></returns>
        public DataTable GetParmCodeCurrency()
        {
            string sqlSelect = @" select CodeNo,CodeDesc from PARMCode where  CodeType='CaseCust_CURRENCY' Order By SortOrder ";

            return _HTGDBBIZ.OpenDataTable(sqlSelect);
        }


    }
}
