using CTBC.CSFS.Pattern;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTBC.WinExe.CSFS.QueryHTG
{
    public class QueryHTGBIZ : BaseBusinessRule
    {

       // 案件編號
       public static string pDocNoPrefix;

       public static bool isContinue = true;

       /// <summary>
        /// INSERT CaseCustHTGSend
        /// </summary>
        /// <param name="strID_No">客戶ID</param>
        public void InsertCaseCustHTGSend(string strID_No)
        {
                 // 取得連接并開放連接
                 IDbConnection dbConnection = base.OpenConnection();

                 // 定義事務
                 IDbTransaction dbTransaction = null;

              using (dbConnection)
              {
                 // 開啟事務
                 dbTransaction = dbConnection.BeginTransaction();

                    string strLine = "";

                    string erroMessage = "";

                    // 主表主鍵
                    string strMainPK = Guid.NewGuid().ToString();

                    #region 案件編號

                    // 取得最大收編
                    string strMaxDocNo = GetMaxDocNo(dbTransaction);

                    // 取得流水號
                    int intMaxDocNo = 0;

                    if (strMaxDocNo == "")
                    {
                       intMaxDocNo = 0;
                    }
                    else
                    {
                       intMaxDocNo = Convert.ToInt32(strMaxDocNo.Substring(8, 5));
                    }

                    // 案件編號 規則：E+民國106+月份10+日16+00001(五碼流水編號)
                    string strDocNo = GetDocNo(intMaxDocNo);

                    #endregion

                    // 資料格式是否正確
                    bool isErro = true;

                    #region  逐筆取得檔案資料

                    // 身分證統一編號
                    string strCustIdNo = strID_No;

                    // 開戶個人資料標記
                    string strOpenFlag = "Y";

                    // 特定期間之交易明細往來（限新臺幣）標記
                    string strTransactionFlag = "N";

                    // 查詢期間起日
                    string strQDateS = "";

                    // 查詢期間訖日
                    string strQDateE = "";

                    string strVersionStatus = "";

                    strVersionStatus = "01";

                    isErro = false;

                    // HTG查詢狀態
                    string strHTGSendStatus = strOpenFlag == "Y" ? "0" : null;

                    // RFDM查詢狀態
                    string strRFDMSendStatus = strTransactionFlag == "Y" ? "0" : null;

                    // INSERT CaseCustQueryVersion
                    string sqlCaseCustQueryVersion = @" INSERT INTO CaseCustQueryVersion
                                                                                         ( NewID ,
                                                                                           CaseCustNewID ,
                                                                                           CustIdNo ,
                                                                                           OpenFlag ,
                                                                                           TransactionFlag ,
                                                                                           QDateS ,
                                                                                           QDateE ,
                                                                                           Status ,
                                                                                           HTGSendStatus ,
                                                                                           RFDMSendStatus ,
                                                                                           CreatedDate ,
                                                                                           CreatedUser ,
                                                                                           ModifiedDate ,
                                                                                           ModifiedUser
                                                                                         )
                                                                                 VALUES  ( NEWID() ,
                                                                                           '" + strMainPK + @"' ,
                                                                                           '" + strCustIdNo + @"', 
                                                                                           '" + strOpenFlag + @"' ,
                                                                                           '" + strTransactionFlag + @"' ,
                                                                                          CASE WHEN '" + strQDateS + @"' = '' THEN '        ' ELSE '" + strQDateS + @"' END , 
                                                                                          CASE WHEN '" + strQDateE + @"' = '' THEN '        ' ELSE '" + strQDateE + @"' END , 
                                                                                           '" + strVersionStatus + @"' ,
                                                                                           '" + strHTGSendStatus + @"' , 
                                                                                            '" + strRFDMSendStatus + @"' , 
                                                                                           GETDATE() ,
                                                                                           'SYSTEM' , 
                                                                                           GETDATE() ,
                                                                                           'SYSTEM' 
                                                                                         )";


                    base.ExecuteNonQuery(sqlCaseCustQueryVersion, dbTransaction);


                    #endregion

                 #region  Insert主表

                 string strMainStatus = "99";


                 string sqlCaseCustQuery = @" INSERT CaseCustQuery
                                                                ( NewID ,
                                                                  DocNo ,
                                                                  Version ,
                                                                  RecvDate ,
                                                                  QFileName ,
                                                                  Status ,
                                                                  CreatedDate ,
                                                                  CreatedUser ,
                                                                  ModifiedDate ,
                                                                  ModifiedUser
                                                                )
                                                        VALUES  ( '" + strMainPK + @"' ,
                                                                  '" + strDocNo + @"' , 
                                                                  0 , 
                                                                  GETDATE() , 
                                                                  '" + "" + @"' ,                       
                                                                  '" + strMainStatus + @"' , 
                                                                  GETDATE() , 
                                                                  'SYSTEM' ,
                                                                  GETDATE() , 
                                                                  'SYSTEM' 
                                                                )";


                 base.ExecuteNonQuery(sqlCaseCustQuery, dbTransaction);

                 // Insert ApprMsgKey
                 string sql = @"INSERT INTO dbo.ApprMsgKey
                                                    ( MsgKeyLU ,
                                                      MsgKeyLP ,
                                                      MsgKeyRU ,
                                                      MsgKeyRP ,
                                                      MsgKeyRB ,
                                                      MsgUID ,
                                                      VersionNewID
                                                    )
                                            VALUES  ( @MsgKeyLU, 
                                                     @MsgKeyLP ,
                                                     @MsgKeyRU ,
                                                     @MsgKeyRP ,
                                                     @MsgKeyRB ,
                                                     @MsgUID ,
                                                     (select NewID from CaseCustQueryVersion where CaseCustNewID = '" + strMainPK + @"') )";
                 base.Parameter.Clear();
                 base.Parameter.Add(new CommandParameter("@MsgKeyLU", "cs9YNi65qmhlrZrTtyHdRg=="));
                 base.Parameter.Add(new CommandParameter("@MsgKeyLP", "2s1AL83pWawjetgMsys4qg=="));
                 base.Parameter.Add(new CommandParameter("@MsgKeyRU", "hzyHRpUXX4Q="));
                 base.Parameter.Add(new CommandParameter("@MsgKeyRP", "5sUffpY92gWzpQQOAINjMA=="));
                 base.Parameter.Add(new CommandParameter("@MsgKeyRB", "09FhuxWlgXE="));
                 base.Parameter.Add(new CommandParameter("@MsgUID", "cs9YNi65qmhlrZrTtyHdRg=="));

                 base.ExecuteNonQuery(sql, dbTransaction);


                 // Insert BOPS067050Send
                 sql = @" INSERT INTO BOPS067050Send
           ( NewID ,
VersionNewID ,
CustIdNO ,
Optn ,
SendStatus ,
           CreatedDate ,
           CreatedUser ,
           ModifiedDate ,
           ModifiedUser
           )
           VALUES  ( NEWID() ,
(select NewID from CaseCustQueryVersion where CaseCustNewID = '" + strMainPK + @"') ,
'" + strID_No + @"',
   'D' ,
             '02' ,
                                                                                           GETDATE() ,
                                                                                           'QueryHTG' , 
                                                                                           GETDATE() ,
                                                                                           'QueryHTG' 
                                                                                         )";


                 base.ExecuteNonQuery(sql, dbTransaction);

                 // Insert BOPS060628Send
                 sql = @" INSERT INTO BOPS060628Send
           ( NewID ,
VersionNewID ,
CustIdNO ,
SendStatus ,
           CreatedDate ,
           CreatedUser ,
           ModifiedDate ,
           ModifiedUser
           )
           VALUES  ( NEWID() ,
(select NewID from CaseCustQueryVersion where CaseCustNewID = '" + strMainPK + @"') ,
'" + strID_No + @"',
             '02' ,
                                                                                           GETDATE() ,
                                                                                           'QueryHTG' , 
                                                                                           GETDATE() ,
                                                                                           'QueryHTG' 
                                                                                         )";

                 base.ExecuteNonQuery(sql, dbTransaction);

/*
                 // Insert BOPS060490Send
                 sql = @" INSERT INTO BOPS060490Send
           ( NewID ,
VersionNewID ,
CustIdNO ,
IDTYPE ,
CURRENCYTYPE ,
SendStatus ,
           CreatedDate ,
           CreatedUser ,
           ModifiedDate ,
           ModifiedUser
           )
           VALUES  ( NEWID() ,
(select NewID from CaseCustQueryVersion where CaseCustNewID = '" + strMainPK + @"') ,
'" + strID_No + @"',
   '01' ,
'0' ,
             '02' ,
                                                                                           GETDATE() ,
                                                                                           'QueryHTG' , 
                                                                                           GETDATE() ,
                                                                                           'QueryHTG' 
                                                                                         )";


                 base.ExecuteNonQuery(sql, dbTransaction);

                 // Insert BOPS000401Send
*/

                 dbTransaction.Commit();

                 #endregion
              }
        }


        /// <summary>
        /// 取得最大案件編號
        /// </summary>
        /// <returns></returns>
        public string GetMaxDocNo(IDbTransaction dbTransaction)
        {

           string sqlSelect = @"SELECT	
                                   ISNULL(MAX(DocNo),'')
                                FROM 
                                    CaseCustQuery
                                WHERE
                                    Substring(DocNo,2,7) = @DocNo";

           // 清空容器
           base.Parameter.Clear();

           // 添加參數
           base.Parameter.Add(new CommandParameter("@DocNo", pDocNoPrefix));

           return base.ExecuteScalar(sqlSelect, dbTransaction).ToString();

        }

        /// <summary>
        /// 獲取案件編號
        /// 規則：E+民國106+月份10+日16+00001(五碼流水編號)
        /// </summary>
        /// <param name="strMaxDocNo"></param>
        /// <returns></returns>
        private string GetDocNo(int strMaxDocNo)
        {
           string strReturn = "";

           strReturn = "E" + pDocNoPrefix + String.Format("{0:D5}", strMaxDocNo + 1);

           return strReturn;
        }

        /// <summary>
        /// 轉換日期格式(西元年轉換成民國年) YYYMMDD
        /// </summary>
        /// <param name="dateDb">日期</param>
        /// <returns>轉換後的日期</returns>
        /// <remarks>判斷年份是否為1911年以前日期,若為1911以前,頁面預示為"0"年份</remarks>
        public static string ConvertYYYMMDD()
        {
           string dateDb = DateTime.Today.ToString();

           // 轉換後的日期
           string dateValue = "";

           if (dateDb != "" && dateDb.Length > 8)
           {
              DateTime dt = Convert.ToDateTime(dateDb);

              // 判斷年份是否為1911年以前日期,若為1911以前,頁面預示為"0"年份 add by Stephen 03/23
              if (Convert.ToInt32(dt.Year) <= 1911)
              {
                 dateValue = string.Format("{0}{1}{2}", "0", dt.Month.ToString("00"), dt.Day.ToString("00"));
              }
              else
              {
                 dateValue = string.Format("{0}{1}{2}", dt.AddYears(-1911).Year.ToString("000"), dt.Month.ToString("00"), dt.Day.ToString("00"));
              }
           }

           return dateValue;
        }

       /// <summary>
        /// 查詢來文檔案
        /// </summary>
        /// <param name="strID_No"></param>
        /// <returns></returns>
        public DataTable SelectCaseCustHTGRecv(string strID_No)
        {
            try
            {
                #region sql

               string sqlSelect = @"SELECT NewID as VersionNewID, Status 
                        FROM CaseCustQueryVersion 
                        WHERE (
                           NewID in ( select VersionNewID from BOPS067050Send where sendstatus = '02' AND CustIdNo=@ID_No)  
                           or NewID in ( select VersionNewID from BOPS060628Send where sendstatus = '02' AND CustIdNo=@ID_No)  
                           or NewID in ( select VersionNewID from BOPS060490Send where sendstatus = '02' AND CustIdNo=@ID_No)  
                         )";

                #endregion

                base.Parameter.Clear();

                base.Parameter.Add(new CommandParameter("@ID_No", strID_No));

                return base.Search(sqlSelect);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        /// <summary>
        /// 取得最大流水號
        /// </summary>
        /// <returns></returns>
        public string GetMaxTrnNum()
        {
            try
            {

                string sql = @"
                            select 
                            	isnull(MAX(TrnNum),'') as TrnNumMax 
                            from CaseCustHTGSend
                            where TrnNum like '" + DateTime.Now.ToString("yyyyMM") + "%' ";

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
        /// 
        /// </summary>
        /// <param name="strMaxNo"></param>
        /// <returns></returns>
        private string CalculateTrnNum(int strMaxNo)
        {
            return DateTime.Now.ToString("yyyyMMdd") + String.Format("{0:D5}", strMaxNo + 1);
        }


        /// <summary>
        /// 查詢來文檔案
        /// </summary>
        /// <param name="strID_No"></param>
        /// <returns></returns>
        public DataTable CheckCaseCustHTGRecv(string strID_No)
        {
           DataTable dt = null;

            try
            {

               string VersionNewID = null;

               string sql = @"select distinct NewID from CaseCustQueryVersion where CustIdNo = @CustIdNo";

               base.Parameter.Clear();

               base.Parameter.Add(new CommandParameter("@CustIdNo", strID_No));

               VersionNewID = base.Search(sql).Rows[0][0].ToString();

               // 檢查是否都已發本完畢
               sql = @"SELECT count(*) num
                        FROM BOPS000401Send 
                        WHERE SendStatus = '02' and VersionNewID=@VersionNewID
                  ";

               base.Parameter.Clear();

               base.Parameter.Add(new CommandParameter("@VersionNewID", VersionNewID));

               dt = base.Search(sql);

               if (dt != null && dt.Rows.Count > 0) {
                  if (Convert.ToInt32(dt.Rows[0]["num"]) == 0)
                  { 

               // 取得各回傳檔的筆數
               sql = @"SELECT 'BOPS067050Recv' name, count(*) num
                        FROM BOPS067050Recv 
                        WHERE VersionNewID=@VersionNewID
                        UNION
                        SELECT 'BOPS060628Recv' name, count(*) num
                        FROM BOPS060628Recv 
                        WHERE VersionNewID=@VersionNewID
                        UNION
                        SELECT 'BOPS060490Recv' name, count(*) num
                        FROM BOPS060490Recv 
                        WHERE VersionNewID=@VersionNewID
                        UNION
                        SELECT 'BOPS000401Recv' name, count(*) num
                        FROM BOPS000401Recv 
                        WHERE VersionNewID=@VersionNewID
                  ";

                base.Parameter.Clear();

                base.Parameter.Add(new CommandParameter("@VersionNewID", VersionNewID));

                return dt = base.Search(sql);
                  }
                  else
                  {
                     return null;
                  }
               }
               else
               {
                  return null;
               }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }

}
