using CTBC.CSFS.Pattern;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTBC.WinExe.CSFS.QueryRFDM
{
    public class QueryRFDMBIZ : BaseBusinessRule
    {

        /// <summary>
        /// INSERT CaseCustRFDMSend
        /// </summary>
        /// <param name="strID_No">客戶ID</param>
        public string InsertCaseCustRFDMSend(string strID_No)
        {
           string VersionNewID = null;

            try
            {
                // 查詢本月最大的流水號
                string pMaxTrnNum = GetMaxTrnNum();

                // 流水號變量
                int pTrnNum1 = 0;

                // 截取流水號
                if (pMaxTrnNum != "")
                {
                    pTrnNum1 = Convert.ToInt32(pMaxTrnNum.Substring(8, 5));
                }

                // 流水號
                string pNewTrnNum1 = CalculateTrnNum(pTrnNum1);
                string pNewTrnNum2 = CalculateTrnNum(pTrnNum1 + 1);
                string pNewTrnNum3 = CalculateTrnNum(pTrnNum1 + 2);

                Guid pVersionNewID = Guid.NewGuid();

                #region insert sql

                string sqlCaseCustRFDMSend = @"INSERT  CaseCustRFDMSend
                                                            ( TrnNum ,
                                                              VersionNewID ,
                                                              ID_No ,
                                                              Type ,
                                                              Start_Jnrst_Date,
                                                              End_Jnrst_Date,
                                                              acctDesc ,
                                                              RFDMSendStatus ,
                                                              CreatedDate ,
                                                              CreatedUser ,
                                                              ModifiedDate ,
                                                              ModifiedUser
                                                            )
                                                    VALUES  ( @TrnNum1 , -- TrnNum - nvarchar(20)
                                                              @VersionNewID , -- VersionNewID - uniqueidentifier
                                                              @CustIdNo , -- ID_No - nvarchar(14)
                                                              '0' , -- Type - nvarchar(1)
                                                              '2017-01-01',
                                                              '2017-06-30',
                                                              @acctDesc1 ,
                                                              '02' , -- RFDMSendStatus - nvarchar(2)
                                                              @ModifiedDate , -- CreatedDate - datetime
                                                              @ModifiedUser , -- CreatedUser - nvarchar(20)
                                                              @ModifiedDate , -- ModifiedDate - datetime
                                                              @ModifiedUser  -- ModifiedUser - nvarchar(20)
                                                            )";

                sqlCaseCustRFDMSend += @"INSERT  CaseCustRFDMSend
                                                            ( TrnNum ,
                                                              VersionNewID ,
                                                              ID_No ,
                                                              Type ,
                                                              Start_Jnrst_Date,
                                                              End_Jnrst_Date,
                                                              acctDesc ,
                                                              RFDMSendStatus ,
                                                              CreatedDate ,
                                                              CreatedUser ,
                                                              ModifiedDate ,
                                                              ModifiedUser
                                                            )
                                                    VALUES  ( @TrnNum2 , -- TrnNum - nvarchar(20)
                                                              @VersionNewID , -- VersionNewID - uniqueidentifier
                                                              @CustIdNo , -- ID_No - nvarchar(14)
                                                              '0' , -- Type - nvarchar(1)
                                                              '2017-01-01',
                                                              '2017-06-30',
                                                              @acctDesc2 ,
                                                              '02' , -- RFDMSendStatus - nvarchar(2)
                                                              @ModifiedDate , -- CreatedDate - datetime
                                                              @ModifiedUser , -- CreatedUser - nvarchar(20)
                                                              @ModifiedDate , -- ModifiedDate - datetime
                                                              @ModifiedUser  -- ModifiedUser - nvarchar(20)
                                                            )";


                sqlCaseCustRFDMSend += @"INSERT  CaseCustRFDMSend
                                                            ( TrnNum ,
                                                              VersionNewID ,
                                                              ID_No ,
                                                              Type ,
                                                              Start_Jnrst_Date,
                                                              End_Jnrst_Date,
                                                              acctDesc ,
                                                              RFDMSendStatus ,
                                                              CreatedDate ,
                                                              CreatedUser ,
                                                              ModifiedDate ,
                                                              ModifiedUser
                                                            )
                                                    VALUES  ( @TrnNum3 , -- TrnNum - nvarchar(20)
                                                              @VersionNewID , -- VersionNewID - uniqueidentifier
                                                              @CustIdNo , -- ID_No - nvarchar(14)
                                                              '0' , -- Type - nvarchar(1)
                                                              '2017-01-01',
                                                              '2017-06-30',
                                                              @acctDesc3 ,
                                                              '02' , -- RFDMSendStatus - nvarchar(2)
                                                              @ModifiedDate , -- CreatedDate - datetime
                                                              @ModifiedUser , -- CreatedUser - nvarchar(20)
                                                              @ModifiedDate , -- ModifiedDate - datetime
                                                              @ModifiedUser  -- ModifiedUser - nvarchar(20)
                                                            )";

                #endregion

                base.Parameter.Clear();

                base.Parameter.Add(new CommandParameter("@TrnNum1", pNewTrnNum1));
                base.Parameter.Add(new CommandParameter("@TrnNum2", pNewTrnNum2));
                base.Parameter.Add(new CommandParameter("@TrnNum3", pNewTrnNum3));
                base.Parameter.Add(new CommandParameter("@VersionNewID", pVersionNewID));
                base.Parameter.Add(new CommandParameter("@CustIdNo", strID_No));
                base.Parameter.Add(new CommandParameter("@acctDesc1", "S"));
                base.Parameter.Add(new CommandParameter("@acctDesc2", "Q"));
                base.Parameter.Add(new CommandParameter("@acctDesc3", "T"));
                base.Parameter.Add(new CommandParameter("@ModifiedUser", "QueryRFDM"));
                base.Parameter.Add(new CommandParameter("@ModifiedDate", DateTime.Now));

                base.ExecuteNonQuery(sqlCaseCustRFDMSend);


                // Get VersionNewID
                sqlCaseCustRFDMSend = @"select distinct VersionNewID from CaseCustRFDMSend where RFDMSendStatus = '02' AND ID_No = @CustIdNo";

                base.Parameter.Clear();

                base.Parameter.Add(new CommandParameter("@CustIdNo", strID_No));

                VersionNewID = base.Search(sqlCaseCustRFDMSend).Rows[0][0].ToString();

               
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return VersionNewID;
        }

        /// <summary>
        /// 查詢來文檔案
        /// </summary>
        /// <param name="strID_No"></param>
        /// <returns></returns>
        public DataTable SelectCaseCustRFDMRecv(string strID_No)
        {
            try
            {
                #region sql

                string sqlSelect = @"SELECT  CONVERT(CHAR(10),DATA_DATE,111) AS DATA_DATE
                                        	,CaseCustRFDMRecv.ACCT_NO
                                        	,CONVERT(CHAR(10),JNRST_DATE,111) AS JNRST_DATE
                                        	,JNRST_TIME
                                        	,JNRST_TIME_SEQ
                                        	,CONVERT(CHAR(10),TRAN_DATE,111) AS TRAN_DATE
                                        	,CONVERT(CHAR(10),POST_DATE,111) AS POST_DATE
                                        	,TRANS_CODE
                                        	,JRNL_NO
                                        	,REVERSE
                                        	,PROMO_CODE
                                        	,REMARK
                                        	,TRAN_AMT
                                        	,BALANCE
                                        	,TRF_BANK
                                        	,TRF_ACCT
                                        	,NARRATIVE
                                        	,FISC_BANK
                                        	,FISC_SEQNO
                                        	,CHQ_NO
                                        	,ATM_NO
                                        	,TRAN_BRANCH
                                        	,TELLER
                                        	,FILLER
                                        	,TXN_DESC
                                        	,ACCT_P2
                                        	,FILE_NAME
                                        	,CaseCustRFDMRecv.TYPE
                                        
                                        FROM    dbo.CaseCustRFDMRecv
                                        LEFT JOIN 
                                                CaseCustRFDMSend ON CaseCustRFDMSend.TrnNum = CaseCustRFDMRecv.TrnNum
                                        AND
                                              CaseCustRFDMSend.VersionNewID = CaseCustRFDMRecv.VersionNewID
                                        WHERE ID_No=@ID_No";

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
                            from CaseCustRFDMSend
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
        public int CheckCaseCustRFDMRecv(string strID_No, string VersionNewID)
        {
            try
            {

                string sql = @"
                            select 
                            	count(*) as num 
                            from CaseCustRFDMSend
                            where 
                               RFDMSendStatus = '01'
                               AND RspCode = '0000'
                               AND FileName is not null
                               AND ID_NO = '" + strID_No + @"' 
                               AND VersionNewID = '" + VersionNewID + "' ";

                DataTable dt = base.Search(sql);

                if (dt != null && dt.Rows.Count > 0)
                {
                    return Convert.ToInt32(dt.Rows[0]["num"]);
                }
                else
                {
                    return 0;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }

}
