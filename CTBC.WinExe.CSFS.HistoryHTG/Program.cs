/// <summary>
/// 程式說明:6-HTG發查
/// </summary>


using System;
using System.Data;
using System.Reflection;

namespace CTBC.WinExe.CSFS.HistoryHTG
{
    class Program
    {

        public string _taskname = "CTBC.WinExe.CSFS.HistoryHTG";

        HTGDBBIZ _HTGDBBIZ = new HTGDBBIZ();

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            Program mainProgram = new Program();
            mainProgram.Process();
        }

        private void Process()
        {
            #region 收發電文處理

            try
            {
                // log.config
                log4net.Config.XmlConfigurator.Configure(new System.IO.FileInfo(AppDomain.CurrentDomain.BaseDirectory + @"\Log.config"));

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
                                                                             where SendFlag = 'Y' and MsgType = 'T' order by SendOrder asc
                                                                            ");
                /// 新增一組只輸入帳號 沒有ID 電文順序
                /// 
                // 電文設定
                // adam 20191220
                DataTable p_dtApprMsgDefine = _HTGDBBIZ.OpenDataTable(@" SELECT
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
                                                                             where SendFlag = 'Y' and MsgType = 'P' order by SendOrder asc
                                                                            ");


                _HTGDBBIZ.WriteLog(_taskname + " ApprMsgDefine settings count = [" + m_dtApprMsgDefine.Rows.Count.ToString() + "]");

                // 無發送電文設定
                if (m_dtApprMsgDefine.Rows.Count == 0)
                {
                    _HTGDBBIZ.WriteLog("No ApprMsgDefine setting");
                    return;
                }
                // 備份
                DataTable backupDT = m_dtApprMsgDefine.Copy();
                DataTable backupP = p_dtApprMsgDefine.Copy();
                // 查詢要發送電文的案件編號
                // adam 20191220
                string strDataSql = "SELECT NewID as VersionNewID, Status, CustId,CustAccount,Currency,QDateS,QDateE  FROM CaseTrsQueryVersion WHERE 1 = 2   ";

                for (int i = 0; i < m_dtApprMsgDefine.Rows.Count; i++)
                {
                    strDataSql += " or NewID in ( select VersionNewID from " + m_dtApprMsgDefine.Rows[i]["SourceTableName"].ToString().Trim() + " where sendstatus = '02') ";
                }

                DataTable dtData = _HTGDBBIZ.OpenDataTable(strDataSql);
                dtData.TableName = "CaseTrsQueryVersion";

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
                    //bool bRet = true;
                    CTCB.NUMS.Library.HTG.HTGObjectMsg obj = new CTCB.NUMS.Library.HTG.HTGObjectMsg();
                    // adam 20191220
                    if (dtData.Rows[i]["Currency"].ToString().Length != 3)
                    {
                        dtData.Rows[i]["Currency"] = "TWD";
                    }

                    // 按電文設定，依次發送電文 ,有客戶ID及帳號 取  p_dtApprMsgDefine
                    if ((dtData.Rows[i]["CustId"].ToString().Length > 7) && (dtData.Rows[i]["CustAccount"].ToString().Length > 11))
                    {
                        m_dtApprMsgDefine.Clear();
                        m_dtApprMsgDefine = backupP.Copy();
                        for (int j = 0; j < m_dtApprMsgDefine.Rows.Count; j++)
                        {
                            _HTGDBBIZ.WriteLog(_taskname + " Msg name = [" + m_dtApprMsgDefine.Rows[j]["MsgName"].ToString() + "]" + " VersionNewID = [" + dtData.Rows[i]["VersionNewID"].ToString() + "]");

                            _HTGDBBIZ.SendMsgCase(obj, m_dtApprMsgDefine.Rows[j]["MsgName"].ToString(), m_dtApprMsgDefine.Rows[j], dtData.Rows[i]["VersionNewID"].ToString(), CurrencyList, dtData.Rows[i]["CustAccount"].ToString(), dtData.Rows[i]["CustId"].ToString(), dtData.Rows[i]["Currency"].ToString(), dtData.Rows[i]["QDateS"].ToString(), dtData.Rows[i]["QDateE"].ToString());
                        }
                        //m_dtApprMsgDefine.Clear();
                        //m_dtApprMsgDefine = backupDT.Copy();
                    }
                    else
                    {
                        // 按電文設定，依次發送電文 ,有客戶ID及無帳號 取 m_dtApprMsgDefine
                        if ((dtData.Rows[i]["CustId"].ToString().Length > 7) && (dtData.Rows[i]["CustAccount"].ToString().Length < 11))
                        {
                            m_dtApprMsgDefine.Clear();
                            m_dtApprMsgDefine = backupDT.Copy();
                            for (int j = 0; j < m_dtApprMsgDefine.Rows.Count; j++)
                            {
                                _HTGDBBIZ.WriteLog(_taskname + " Msg name = [" + m_dtApprMsgDefine.Rows[j]["MsgName"].ToString() + "]" + " VersionNewID = [" + dtData.Rows[i]["VersionNewID"].ToString() + "]");

                                _HTGDBBIZ.SendMsgCase(obj, m_dtApprMsgDefine.Rows[j]["MsgName"].ToString(), m_dtApprMsgDefine.Rows[j], dtData.Rows[i]["VersionNewID"].ToString(), CurrencyList, dtData.Rows[i]["CustAccount"].ToString(), dtData.Rows[i]["CustId"].ToString(), dtData.Rows[i]["Currency"].ToString(), dtData.Rows[i]["QDateS"].ToString(), dtData.Rows[i]["QDateE"].ToString());
                            }
                        }
                        else
                        {
                            // 按電文設定，依次發送電文 ,無客戶ID及有帳號 取 m_dtApprMsgDefine
                            if ((dtData.Rows[i]["CustId"].ToString().Length < 7) && (dtData.Rows[i]["CustAccount"].ToString().Length > 11))
                            {
                                m_dtApprMsgDefine.Clear();
                                m_dtApprMsgDefine = backupP.Copy();
                                for (int j = 0; j < m_dtApprMsgDefine.Rows.Count; j++)
                                {
                                    _HTGDBBIZ.WriteLog(_taskname + " Msg name = [" + m_dtApprMsgDefine.Rows[j]["MsgName"].ToString() + "]" + " VersionNewID = [" + dtData.Rows[i]["VersionNewID"].ToString() + "]");

                                    _HTGDBBIZ.SendMsgCase(obj, m_dtApprMsgDefine.Rows[j]["MsgName"].ToString(), m_dtApprMsgDefine.Rows[j], dtData.Rows[i]["VersionNewID"].ToString(), CurrencyList, dtData.Rows[i]["CustAccount"].ToString(), dtData.Rows[i]["CustId"].ToString(), dtData.Rows[i]["Currency"].ToString(), dtData.Rows[i]["QDateS"].ToString(), dtData.Rows[i]["QDateE"].ToString());
                                }
                                //m_dtApprMsgDefine.Clear();
                                //m_dtApprMsgDefine = backupDT.Copy(); ;
                            }
                        }
                    }
 


                    // 查詢案件下是否有異常的電文

                    DataTable dtErrorSend = _HTGDBBIZ.GetErrorSend(dtData.Rows[i]["VersionNewID"].ToString());

                    if (dtErrorSend != null && dtErrorSend.Rows.Count > 0)
                    {
                        // 案件下有異常的HTG發查,將主檔狀態改爲 失敗
                        if (dtData.Rows[i]["Status"].ToString() == "01" || dtData.Rows[i]["Status"].ToString() == "02" )
                        {
                            // 20200312 錯誤仍要產檔
                            //_HTGDBBIZ.UpdateQueryVersionStatus("04", dtData.Rows[i]["VersionNewID"].ToString());
                        }
                        // 案件如果是重查拋查中, 並且案件下有異常的HTG發查,將主檔狀態改爲 重查失敗
                        else if (dtData.Rows[i]["Status"].ToString() == "06")
                        {
                            // 20200312 錯誤仍要產檔
                            //_HTGDBBIZ.UpdateQueryVersionStatus("08", dtData.Rows[i]["VersionNewID"].ToString());
                        }
                    }
                    else
                    {
                        if (dtData.Rows[i]["Status"].ToString() == "01")
                        {
                            _HTGDBBIZ.UpdateQueryVersionStatus("02", dtData.Rows[i]["VersionNewID"].ToString());
                        }
                        // 查詢是否有02,03 的案件，若沒有代表都有案件都成功
                        DataTable dtSuccessSend = _HTGDBBIZ.GetSuccessSend(dtData.Rows[i]["VersionNewID"].ToString());

                        if (dtSuccessSend == null || dtSuccessSend.Rows.Count == 0)
                        {
                            // 將Version 檔 HTG的狀態更新為成功 //04 還是4
                            _HTGDBBIZ.EditCaseTrsQueryVersion(dtData.Rows[i]["VersionNewID"].ToString(), "4", "");
                        }
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
            string sqlSelect = @" select CodeNo,CodeDesc from PARMCode where  CodeType='CaseCust_CURRENCY' ";

            return _HTGDBBIZ.OpenDataTable(sqlSelect);
        }
    }
}
