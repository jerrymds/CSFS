using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Data;
using System.Reflection;

namespace CTBC.WinExe.WarningQuerySetting
{
    class Program
    {
        public string _taskname = "CTBC.WinExe.WarningQuerySetting";

        bool boolRacf = true; // 用於測試RACF帳密, 是否有效.. 若一但無效. 則停止所有發查動作....

        HTGDBBIZ _HTGDBBIZ = new HTGDBBIZ();

        static void Main(string[] args)
        {
            Program mainProgram = new Program();
            //mainProgram.test();
            mainProgram.Process();
        }


        private void Process()
        {
            try
            {
                log4net.Config.XmlConfigurator.Configure(new System.IO.FileInfo(AppDomain.CurrentDomain.BaseDirectory + @"\Log.config"));



                // 先判斷是否執行中....
                var isExist = @"select Count(*) from WarningDetails d inner join WarningMaster m on d.docno=m.docno where d.[Set]='9'";
                var vExist = _HTGDBBIZ.OpenDataTable(isExist);
                if (int.Parse(vExist.Rows[0][0].ToString()) > 0)
                {
                    _HTGDBBIZ.WriteLog("前次執行中, 還有" + vExist.Rows[0][0].ToString() + "個案件, 尚未處理!，本次取消執行");
                    return;
                }


                // 先去找出要打的電文.... 20221212, 決定多加 未結案(status<>Z99) , 及排除部分已結案...(d.release is null)

                var sql = @"select d.NewID, d.SerialID,d.DocNo,d.Status,m.CustId, m.CustName, m.CustAccount,m.Currency from WarningDetails d 
                            inner join WarningMaster m on d.docno=m.docno 
                            inner join WarningState s on s.docno= d.docno  and s.NotificationSource= d.NotificationSource
                            where d.Status='C01' AND d.[Set] is NULL AND d.SetDate is NULL AND isnull(s.Status,'')<>'Z99' AND d.release is null";

                var details = _HTGDBBIZ.OpenDataTable(sql);
                if (details.Rows.Count > 0)
                {
                    _HTGDBBIZ.WriteLog("目前處理共 " + details.Rows.Count.ToString() + "個案件");

                    // Set 押成9, 代表執行中...
                    var uSql = @"update WarningDetails SET [Set]='9' where Status='C01' AND [Set] is NULL AND SetDate is NULL";
                    var uSqlResult = _HTGDBBIZ.ExcuteSQL(uSql);


                    foreach (DataRow dr in details.Rows)
                    {
                        try
                        {
                            _HTGDBBIZ.WriteLog("\t目前處理 SerialID :  " + dr["SerialID"] + " / " + dr["DocNo"] + " 案件");
                            ProcessHTG(dr);
                            _HTGDBBIZ.WriteLog("\t處理完成 SerialID :  " + dr["SerialID"] + " / " + dr["DocNo"] + " 案件");
                        }
                        catch (System.Exception ex)
                        {
                            _HTGDBBIZ.WriteLog("\t目前處理 " + dr["SerialID"] + " / " + dr["DocNo"] + " 案件, 失敗!!!!!" + ex.Message.ToString());
                        }
                        finally
                        {
                            // 將Set =1 , SetDate

                            _HTGDBBIZ.UpdateWarningDetailsSet_Date(dr);

                        }
                    }

                    // 20221208, 將本批次, 不知什麼原因.. 造成卡件.. (即[Set]='9', Status='C01' )的案件, 一律, 壓成'2', 以免卡件...
                    var uSql3 = @"update WarningDetails SET [Set]='2',SetDate=getdate() where [Set]='9' AND Status='C01' ";
                    var uSqlResult3 = _HTGDBBIZ.ExcuteSQL(uSql3);





                    // CaseCustDetails.EXCEL_FILE 壓成 NULL, ==> 表示已執行完成
                    //                    var uSql2 = @"update CaseCustDetails Set EXCEL_FILE=null where CaseCustMasterId in (
                    //	                        SELECT m.NewID FROM CaseCustMaster m  inner join CaseCustDetails d  on m.NewID = d.CaseCustMasterId
                    //	                        WHERE convert(varchar, m.ModifiedDate, 23)='" + pDate + "' AND m.STATUS in ('02','06','04','08') )";

                    //                    var uSqlResult2 = _HTGDBBIZ.ExcuteSQL(uSql2);

                }
                else
                {
                    _HTGDBBIZ.WriteLog("目前無案件");
                }
            }
            catch (System.Exception ex)
            {
                _HTGDBBIZ.WriteLog("發生錯誤, 錯誤訊息" + ex.Message.ToString());
            }
        }

        private void ProcessHTG(DataRow dr)
        {
            string strType = "W1";
            try
            {
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

                // 查詢代碼檔的幣別， 401Send檔，依據賬號找到對應的幣別使用
                DataTable CurrencyList = GetParmCodeCurrency();




                // 20220624, 檢查, 若不是外國人, 則不要打... 67050-4 , 把m_dtApprMsgDefine.Rows[0], 刪除
                bool isForeign = true;
                System.Text.RegularExpressions.Regex re = new System.Text.RegularExpressions.Regex("[A-Z|a-z]$");
                string eng = "";
                string cID = dr["CustId"].ToString();
                if (cID != null && cID.Length > 7)
                {
                    eng = cID.Substring(cID.Length - 3, 3);
                    if (!re.IsMatch(eng))
                    {
                        DataRow dr3 = m_dtApprMsgDefine.Rows[0];
                        dr3.Delete();
                        m_dtApprMsgDefine.AcceptChanges();
                        isForeign = false;
                    }
                    else
                        isForeign = true;
                }


                // 20220624 , 新增 [dbo].[BOPS067050V4Send], [dbo].[BOPS067050Send], [dbo].[BOPS060628Send]
                if (isForeign) // 是外國人, 所以要新增BOPS067050V4Send
                {
                    _HTGDBBIZ.Insert67050V4Send(dr["NewID"].ToString(), dr["CustId"].ToString());
                }

                // 新增[BOPS067050Send]
                _HTGDBBIZ.insert67050Send(dr, "SYSTEM");
                //新增  [dbo].[BOPS060628Send]
                //_HTGDBBIZ.insert60628Send(dr, "SYSTEM");





                //string strDataSql = "SELECT DetailsId as VersionNewID, Status,CaseCustMasterId, QueryType, CustIdNo,QDateS,QDateE,DocNo,CustAccount FROM CaseCustDetails WHERE QueryType = '" + iType.ToString() + "'  and (";
                string strDataSql = "SELECT NewID as VersionNewID, SerialID , DocNo FROM WarningDetails WHERE NEWID='" + dr["NewID"] + "'  and (";
                for (int i = 0; i < m_dtApprMsgDefine.Rows.Count; i++)
                {
                    strDataSql += " NEWID in ( select VersionNewID from " + m_dtApprMsgDefine.Rows[i]["SourceTableName"].ToString().Trim() + " where sendstatus = '02') or";
                }

                strDataSql = strDataSql.Substring(0, strDataSql.Length - 2) + " )";
                DataTable dtData = _HTGDBBIZ.OpenDataTable(strDataSql);




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
                    //if (!checkRacfResult)
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

                        _HTGDBBIZ.SendMsgCase(obj, m_dtApprMsgDefine.Rows[j]["MsgName"].ToString(), m_dtApprMsgDefine.Rows[j], dtData.Rows[i]["VersionNewID"].ToString(), CurrencyList, j, dr["CustAccount"].ToString().Trim());
                    }

                    // 查詢案件下是否有異常的電文
                    DataTable dtErrorSend = _HTGDBBIZ.GetErrorSend(dtData.Rows[i]["VersionNewID"].ToString());

                    if (dtErrorSend != null && dtErrorSend.Rows.Count > 0)
                    {
                        StringBuilder sb = new StringBuilder();
                        foreach (DataRow d in dtErrorSend.Rows)
                        {
                            sb.Append(d["QueryErrMsg"].ToString() + " ");
                        }
                        _HTGDBBIZ.WriteLog("TX_9091失敗: 錯誤訊息" + sb.ToString());
                        _HTGDBBIZ.EditWarningDetails(dtData.Rows[i]["VersionNewID"].ToString(), "0");
                    }
                    else
                    {
                        _HTGDBBIZ.EditWarningDetails(dtData.Rows[i]["VersionNewID"].ToString(), "1");
                        // 查詢是否有02,03 的案件，若沒有代表都有案件都成功
                        //DataTable dtSuccessSend = _HTGDBBIZ.GetSuccessSend(dtData.Rows[i]["VersionNewID"].ToString());
                        //if (dtSuccessSend == null || dtSuccessSend.Rows.Count == 0)
                        //{                            
                        //    _HTGDBBIZ.EditWarningDetails(dtData.Rows[i]["VersionNewID"].ToString(), "1");
                        //}                   

                    }


                    //2022-05-18, 將BOPS 的資料回寫到TX中...

                    //_HTGDBBIZ.BOPS2TX(dtData.Rows[i]["VersionNewID"].ToString(), dtErrorSend);
                    _HTGDBBIZ.BOPS2TX(dtData.Rows[i]["VersionNewID"].ToString(), dtData.Rows[i]["DocNo"].ToString());


                    // 2022-05-25, 
                    //// 若是Type = 1 (A1) , 則儲存至CaseCustOutputF1 的TABLE
                    //var ret = _HTGDBBIZ.InsertOutputFormat(dtData.Rows[i]);
                    //if (ret)
                    //{
                    //    _HTGDBBIZ.WriteLog("儲存成功!!" + dtData.Rows[i]["VersionNewID"] + " QueryType " + dtData.Rows[i]["Status"]);
                    //}
                    //else
                    //{
                    //    _HTGDBBIZ.WriteLog("儲存失敗!!" + dtData.Rows[i]["VersionNewID"] + " QueryType " + dtData.Rows[i]["Status"]);
                    //}

                }









            }
            catch (Exception ex)
            {
                _HTGDBBIZ.WriteLog("程式異常，錯誤信息：" + ex.Message);
            }
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