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

namespace CTBC.WinExe.CSFS.HTG
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
      /// 發送電文
      /// </summary>
      /// <param name="obj"></param>
      /// <param name="txid"></param>
      /// <param name="drApprMsgDefine"></param>
      /// <param name="applNo"></param>
      /// <param name="applNoB"></param>
      /// <returns></returns>
      public bool SendMsgCase(CTCB.NUMS.Library.HTG.HTGObjectMsg obj, string txid, DataRow drApprMsgDefine, string VersionNewID, DataTable CurrencyList)
      {
         bool bRet = true;

         // log
         WriteLog("VersionNewID = [" + VersionNewID + "], TxID=[" + txid + "]");

         try
         {
            #region 刪除舊的 BOPS081019

            string sql = @"DELETE FROM BOPS081019Recv WHERE SendNewID NOT IN (SELECT distinct NewID FROM BOPS060490Send);";
            sql += @"DELETE FROM BOPS081019Send WHERE BOPS060490SendNewID NOT IN (SELECT NewID FROM BOPS060490Send);";

            Parameter.Clear();
            ExecuteNonQuery(sql);

            #endregion

            // 撈取HTG參數
            DataTable dtHtgData = OpenDataTable("select * from ApprMsgKey where VersionNewID = '" + VersionNewID + "'");

            if (dtHtgData.Rows.Count <= 0)
            {
               WriteLog("VersionNewID = [" + VersionNewID + "], TxID=[" + txid + "],無發查資料！");

               return false;
            }

            _ldapuid = Decode(dtHtgData.Rows[0]["MsgKeyLU"].ToString());
            _ldappwd = Decode(dtHtgData.Rows[0]["MsgKeyLP"].ToString());
            _racfuid = Decode(dtHtgData.Rows[0]["MsgKeyRU"].ToString());
            _racfowd = Decode(dtHtgData.Rows[0]["MsgKeyRP"].ToString());
            _racfbranch = Decode(dtHtgData.Rows[0]["MsgKeyRB"].ToString());

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

               WriteLog("VersionNewID = [" + VersionNewID + "], TxID=[" + txid + "],開始發查");

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
                  result = obj.QueryHtg(txid, htparm);
               }

               htreturn = obj.ReturnCode;

               //執行log
               messagelog = obj.MessageLog;

               WriteLog("VersionNewID = [" + VersionNewID + "]" + messagelog);

               // 電文發查失敗
               if (!result)
               {
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
                                 strReturn = Send60491Start(obj, dtDefineBy60491.Rows[0], VersionNewID, dtData.Rows[n]["NewID"].ToString(), CurrencyList);
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
      public bool EditCaseCustQueryVersion(string strVersionNewID, string strHTGSendStatus, string strHTGQryMessage)
      {
         string sql = @"UPDATE [CaseCustQueryVersion] 
                            SET 
                                [HTGSendStatus] = @HTGSendStatus,
                                [HTGQryMessage] = @HTGQryMessage,
                                [ModifiedDate] = GETDATE(), 
                                [ModifiedUser] = 'SYSTEM'
                             WHERE NewID = @NewID";
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

               for (int i = 0; i < 12; i++)
               {
                  string strColumnName = "AC_MARK_" + (i + 1).ToString();

                  string CustIdNo = "ID_DATA_" + (i + 1).ToString();

                  if (dt.Columns.Contains(strColumnName) && dt.Rows[0][strColumnName].ToString().Trim() == "Y" && CustIdNo != "")
                  {

                     // AC_MARK_1 ~AC_MARK_12 如果為Y,則
                     //insert BOPS060490Send （ID_DATA_1 ~ ID_DATA_12）
                     //SA:活存; CA:支存; TD:定存
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
                                                    ,'1'
                                                    ,(select top 1 QDateS from  CaseCustQueryVersion where NewID ='" + VersionNewID + @"')
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
                                                    ,'1'
                                                    ,(select top 1 QDateS from  CaseCustQueryVersion where NewID ='" + VersionNewID + @"')
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
                                                    ,'1'
                                                    ,(select top 1 QDateS from  CaseCustQueryVersion where NewID ='" + VersionNewID + @"')
                                                    ,'TD'
                                                    ,'0'
                                                    ,'CSFS.HTG'
                                                    ,GETDATE()
                                                    ,'CSFS.HTG'
                                                    ,GETDATE()
                                                    ,'02'
                                                    ); ";
                     #endregion

                  }
               }

               //20181228 固定變更 add start
               if (!string.IsNullOrEmpty(insert60628Sql))
               {
                  base.ExecuteNonQuery(insert60628Sql, dbTransaction, false);
               }
               //20181228 固定變更 add end

               dbTransaction.Commit();

               if (insert60628Sql != "")
               {
                  WriteLog("VersionNewID = [" + VersionNewID + "], TxID=[" + txid + "],資料寫入Table: " + "BOPS" + txid + "Send");
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

                  // 新增81019Send
                  if (!string.IsNullOrEmpty(insert081019Sql))
                  {
                     base.ExecuteNonQuery(insert081019Sql, dbTransaction, false);
                  }
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
      public int UpdateQueryVersionStatus(string Status, string NewID)
      {
         try
         {
            string sql = @"
UPDATE CaseCustQueryVersion SET Status = @Status  WHERE   NewID = @NewID;
UPDATE CaseCustQuery SET Status = @Status WHERE NewID IN (SELECT CaseCustNewID FROM CaseCustQueryVersion WHERE NewID = @NewID)
";

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
                    select NewID from BOPS067050Send WHERE VersionNewID = @VersionNewID and  sendstatus='03'
                    union
                    select NewID from BOPS060628Send WHERE VersionNewID = @VersionNewID and  sendstatus='03'
                    union
                    select NewID from BOPS060490Send WHERE VersionNewID = @VersionNewID and  sendstatus='03'
                    union
                    select NewID from BOPS000401Send WHERE VersionNewID = @VersionNewID and  sendstatus='03'
                    union
                    select NewID from BOPS081019Send WHERE VersionNewID = @VersionNewID and  sendstatus in ('03','04')
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
            string strDataSql = @"
                    select NewID from BOPS067050Send WHERE VersionNewID = @VersionNewID and  sendstatus IN ('02', '03')
                    union
                    select NewID from BOPS060628Send WHERE VersionNewID = @VersionNewID and  sendstatus IN ('02', '03')
                    union
                    select NewID from BOPS060490Send WHERE VersionNewID = @VersionNewID and  sendstatus IN ('02', '03')
                    union
                    select NewID from BOPS000401Send WHERE VersionNewID = @VersionNewID and  sendstatus IN ('02', '03')
                    union
                    select NewID from BOPS081019Send WHERE VersionNewID = @VersionNewID and  (sendstatus IN ('02', '03', '04') or (sendstatus = '01' and ATMFlag = 'N'))
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
   }
}
