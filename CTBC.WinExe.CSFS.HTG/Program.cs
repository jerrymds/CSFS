/// <summary>
/// 程式說明:6-HTG發查
/// </summary>


using System;
using System.Data;
using System.Reflection;

namespace CTBC.WinExe.CSFS.HTG
{
    class Program
    {

        public string _taskname = "CTBC.WinExe.CSFS.HTG";

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
                                                                             where SendFlag = 'Y' order by SendOrder asc
                                                                            ");


                _HTGDBBIZ.WriteLog(_taskname + " ApprMsgDefine settings count = [" + m_dtApprMsgDefine.Rows.Count.ToString() + "]");

                // 無發送電文設定
                if (m_dtApprMsgDefine.Rows.Count == 0)
                {
                    _HTGDBBIZ.WriteLog("No ApprMsgDefine setting");
                    return;
                }

                // 查詢要發送電文的案件編號
                string strDataSql = "SELECT NewID as VersionNewID, Status FROM CaseCustQueryVersion WHERE 1 = 2 ";

                for (int i = 0; i < m_dtApprMsgDefine.Rows.Count; i++)
                {
                    strDataSql += " or NewID in ( select VersionNewID from " + m_dtApprMsgDefine.Rows[i]["SourceTableName"].ToString().Trim() + " where sendstatus = '02') ";
                }

                DataTable dtData = _HTGDBBIZ.OpenDataTable(strDataSql);
                dtData.TableName = "CaseCustQueryVersion";

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

                    // 按電文設定，依次發送電文
                    for (int j = 0; j < m_dtApprMsgDefine.Rows.Count; j++)
                    {
                        _HTGDBBIZ.WriteLog(_taskname + " Msg name = [" + m_dtApprMsgDefine.Rows[j]["MsgName"].ToString() + "]" + " VersionNewID = [" + dtData.Rows[i]["VersionNewID"].ToString() + "]");

                        _HTGDBBIZ.SendMsgCase(obj, m_dtApprMsgDefine.Rows[j]["MsgName"].ToString(), m_dtApprMsgDefine.Rows[j], dtData.Rows[i]["VersionNewID"].ToString(), CurrencyList);
                    }

                    // 查詢案件下是否有異常的電文
                    DataTable dtErrorSend = _HTGDBBIZ.GetErrorSend(dtData.Rows[i]["VersionNewID"].ToString());

                    if (dtErrorSend != null && dtErrorSend.Rows.Count > 0)
                    {
                        // 案件如果是拋查中, 並且案件下有異常的HTG發查,將主檔狀態改爲 失敗
                        if (dtData.Rows[i]["Status"].ToString() == "02")
                        {
                            _HTGDBBIZ.UpdateQueryVersionStatus("04", dtData.Rows[i]["VersionNewID"].ToString());
                        }
                        // 案件如果是重查拋查中, 並且案件下有異常的HTG發查,將主檔狀態改爲 重查失敗
                        else if (dtData.Rows[i]["Status"].ToString() == "06")
                        {
                            _HTGDBBIZ.UpdateQueryVersionStatus("08", dtData.Rows[i]["VersionNewID"].ToString());
                        }
                    }
                    else
                    {
                        // 查詢是否有02,03 的案件，若沒有代表都有案件都成功
                        DataTable dtSuccessSend = _HTGDBBIZ.GetSuccessSend(dtData.Rows[i]["VersionNewID"].ToString());

                        if (dtSuccessSend == null || dtSuccessSend.Rows.Count == 0)
                        {
                            // 將Version 檔 HTG的狀態更新為成功
                            _HTGDBBIZ.EditCaseCustQueryVersion(dtData.Rows[i]["VersionNewID"].ToString(), "4", "");
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
