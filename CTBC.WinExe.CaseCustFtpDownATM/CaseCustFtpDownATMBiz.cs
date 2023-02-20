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

namespace CTBC.WinExe.CaseCustFtpDownATM
{
    class CaseCustFtpDownATMBiz : BaseBusinessRule
    {
        private static FileLog m_fileLog;
        /// <summary>
        /// 產出CaseCustOutputF2, 即Type 2, Type 4 的報表
        /// 因應ATMRecv多了三個欄位, 可提升JOIN的效率
        /// </summary>
        /// <param name="DetailsId"></param>        
        /// <returns></returns>
        internal string outputF2(string DetailsId)
        {
            string strData_Date = DateTime.Now.ToString("yyyyMMdd");
            try
            {

                #region sql command


                string sql = @"
                            with 
                            cr as
                             (
                                SELECT 
                                    (Cast(IdNo as varchar)
	                                     + CustIdNo
	                                     + CaseCustNewRFDMRecv.ACCT_NO
	                                     + ISNULL(QDateS, '')
	                                     + ISNULL(QDateE, '')
                                    ) AS GroupId
									,CaseCustMaster.DocNo AS DocNo
									,CaseCustDetails.CaseCustMasterId AS MasterId
									,CaseCustDetails.DetailsId AS DetailsId
									,CaseCustDetails.CustIdNo AS Cust_ID_NO
                                    ,ISNULL(BOPS067050Recv.CUSTOMER_NAME, '') AS CUSTOMER_NAME --戶名
                                    ,CaseCustNewRFDMRecv.ACCT_NO --帳號	X(20)
                                	,CASE WHEN ISNULL(CaseCustNewRFDMRecv.FISC_SEQNO, '') = '' OR (CaseCustNewRFDMRecv.FISC_SEQNO = '00000000')
                                        THEN RIGHT('00000000'+JRNL_NO,8)
                                        ELSE CaseCustNewRFDMRecv.FISC_SEQNO
                                    END AS JRNL_NO--交易序號	9(08)
                                	,CONVERT(nvarchar(8),TRAN_DATE,112 ) as TRAN_DATE--交易日期	X(08)
                                	,CaseCustATMRecv.[YBTXLOG_TXN_HHMMSS]  as ATM_TIME
                                    ,CASE WHEN ISNULL(JNRST_TIME, '') = ''
                                        THEN ''
                                        ELSE 
                                        CASE WHEN (JNRST_TIME = JRNL_NO OR JNRST_TIME = CONVERT(varchar, JNRST_DATE, 112))
                                            THEN ''
                                            ELSE
                                            --CASE WHEN TRY_CONVERT(datetime, SUBSTRING(JNRST_TIME,1,2)+':'+SUBSTRING(JNRST_TIME,3,2)+':'+SUBSTRING(JNRST_TIME,5,2)) IS NULL
                                                --THEN ''
                                                SUBSTRING(JNRST_TIME,1,6)
                                            --END
                                        END
                                    END AS JNRST_TIME --交易時間	X(06)
                                    ,CASE WHEN isnull(TRAN_BRANCH,'') <> '' 
                                        THEN '822' + isnull(TRAN_BRANCH,'') 
                                        ELSE ''
                                    END TRAN_BRANCH --交易行(或所屬分行代號)	X(07)
                                	,isnull(TXN_DESC,'') as TXN_DESC--交易摘要	X(40)
                                	,TRAN_AMT--支出金額	X(16)
                                	,TRAN_AMT*(-1) as SaveAMT--存入金額	X(16)
                                	,BALANCE --餘額	X(16)
                                	,(
                                       SUBSTRING(CaseCustATMRecv.[YBTXLOG_SRC_ID],1,3) + CaseCustATMRecv.[YBTXLOG_SAFE_TMNL_ID]                                       
                                    ) as ATM_NO --ATM或端末機代號	X(20)
                                	,TELLER as TELLER --櫃員代號	X(20)
                                	,CASE WHEN CAST(isnull(TRF_ACCT,'0') AS NUMERIC) = 0
                                        THEN ''
                                        ELSE replace(replace(isnull(TRF_BANK,''),'448','822'),'000','822') + isnull(TRF_ACCT,'')
                                    END as TRF_BANK --轉出入行庫代碼及帳號 (RFDM)TRF_BANK+TRF_ACCT
                                    ,isnull(remark,'') as Remark -- 註記
                                	,isnull(CaseCustNewRFDMRecv.NARRATIVE,'') as NARRATIVE  --備註 (RFDM) NARRATIVE
                                	,(
                                       CaseCustATMRecv.Member_No                                       
                                    ) as MEMBER_NO --ATM會員編號	X(20)
                                    ,BOPS000401Recv.PD_TYPE_DESC, --產品別
                                    BOPS000401Recv.CURRENCY AS CURRENCY -- 幣別
								FROM 
                                    CaseCustNewRFDMRecv 
                                LEFT JOIN CaseCustDetails ON CaseCustDetails.DetailsId = CaseCustNewRFDMRecv.VersionNewID
                                LEFT JOIN CaseCustMaster ON CaseCustMaster.NewID = CaseCustDetails.CaseCustMasterId
                                LEFT JOIN BOPS000401Recv ON BOPS000401Recv.VersionNewID = CaseCustNewRFDMRecv.VersionNewID
	                                    and BOPS000401Recv.ACCT_NO = CaseCustNewRFDMRecv.ACCT_NO
                                LEFT JOIN BOPS067050Recv ON BOPS067050Recv.VersionNewID = CaseCustDetails.detailsid
	                                        and BOPS067050Recv.CUST_ID_NO = CaseCustDetails.CustIdNo
								LEFT JOIN CaseCustATMRecv ON CaseCustATMRecv.DATA_DATE=@Data_Date 
								            and CaseCustATMRecv.YBTXLOG_DATE=CaseCustNewRFDMRecv.TRAN_DATE
											and CaseCustATMRecv.FISC_SEQNO = CaseCustNewRFDMRecv.FISC_SEQNO
                                WHERE 
                                    CaseCustNewRFDMRecv.VersionNewID = @DetailsId
)
select GroupId,DocNo,MasterId,DetailsId,Cust_ID_NO,CUSTOMER_NAME,ACCT_NO,ROW_NUMBER() OVER (PARTITION BY GroupId,TRAN_DATE ORDER BY GroupId,TRAN_DATE) AS JRNL_NO,TRAN_DATE,TRAN_BRANCH ,TXN_DESC,TRAN_AMT,SaveAMT,BALANCE,ATM_NO,TELLER,TRF_BANK,Remark,NARRATIVE,MEMBER_NO,PD_TYPE_DESC,
(CASE WHEN ISNULL(ATM_TIME,'')<>'' THEN ATM_TIME ELSE JNRST_TIME END) as JNRST_TIME, CURRENCY from cr ORDER BY GroupId,TRAN_DATE,JNRST_TIME,JRNL_NO";
                //舊的排序
                //select GroupId,DocNo,MasterId,DetailsId,Cust_ID_NO,CUSTOMER_NAME,ACCT_NO,JRNL_NO,TRAN_DATE,TRAN_BRANCH ,TXN_DESC,TRAN_AMT,SaveAMT,BALANCE,ATM_NO,TELLER,TRF_BANK,NARRATIVE,PD_TYPE_DESC,
                //(CASE WHEN ISNULL(ATM_TIME,'')<>'' THEN ATM_TIME
                //ELSE JNRST_TIME END) as JNRST_TIME, CURRENCY from cr ORDER BY GroupId,TRAN_DATE,JNRST_TIME,JRNL_NO



                #endregion






                // 清空容器
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@DetailsId", DetailsId));
                // strData_Date, 必須是YYYYMMdd
                base.Parameter.Add(new CommandParameter("@Data_Date", strData_Date));

                


                DataTable dt = base.Search(sql);

                if (dt.Rows.Count == 0) // 表示真的沒有明細資料.... Details.EXCEL_FILE='FTP'  一筆, "所查詢期間查無往來明細"
                {
                    insertF2empty(DetailsId);
                }
                else
                {

                    #region 20221122 加工.. 會員編號


                    foreach (DataRow dr in dt.Rows)
                    {
                        List<string> allNarrative = new List<string>();
                        if (!string.IsNullOrEmpty(dr["NARRATIVE"].ToString()))
                            allNarrative.Add(dr["NARRATIVE"].ToString());
                        if (!string.IsNullOrEmpty(dr["Remark"].ToString()))
                            allNarrative.Add(dr["Remark"].ToString());
                        if (!string.IsNullOrEmpty(dr["MEMBER_NO"].ToString()))
                            allNarrative.Add(dr["MEMBER_NO"].ToString());

                        //若有NARRATIVE 有值, 則要加 / 區隔...


                        dr["NARRATIVE"] = string.Join(" / ", allNarrative);


                    }



                    #endregion

                    dt.TableName = "CaseCustOutputF2";

                    InsertIntoTableWithTransaction(dt, null);

                }







                return "";
            }
            catch (Exception ex)
            {

                return string.Format("-------產出F2檔錯誤------- 案號 {0} / {1}", DetailsId, ex.Message.ToString());

            }
        }

        private void insertF2empty(string DetailsId)
        {
            try
            {
                var sql = @"UPDATE  CaseCustDetails set EXCEL_FILE='FTP'  where DetailsId='" + DetailsId + "';";
                var det = base.ExecuteNonQuery(sql);
            }
            catch (Exception ex)
            {
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "錯誤--------取得基資資料錯誤" + DetailsId.ToString() + "錯誤訊息" + ex.Message.ToString());
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

            if (rtable.Columns.Contains("PD_TYPE_DESC"))
            {
                rtable.Columns.Remove("PD_TYPE_DESC");
            }

            if (rtable.Columns.Contains("GroupId"))
                rtable.Columns.Remove("GroupId");

            if (rtable.Columns.Contains("MEMBER_NO"))
                rtable.Columns.Remove("MEMBER_NO");

            if (rtable.Columns.Contains("Remark"))
                rtable.Columns.Remove("Remark");

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

                    //adam 2022/3/3 JRNL_NO 前補0 補滿8位
                    for (int i = 0; i < rtable.Rows.Count; i++)
                    {
                        rtable.Rows[i]["JRNL_NO"] = rtable.Rows[i]["JRNL_NO"].ToString().PadLeft(8, '0');
                    }
                    //

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

                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "錯誤--------bulk insert 錯誤" + "錯誤訊息" + ex.Message.ToString());
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
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "錯誤--------取得空TABLE 錯誤" + tablename.ToString() + "錯誤訊息" + ex.Message.ToString());
                return new DataTable(); ;
            }
        }

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
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "錯誤--------取得OpenDataTable 錯誤" + "錯誤訊息" + ex.Message.ToString());
                
            }
            return returnValue;
        }



    }

    public class trans
    {
        public string GroupId { get; set; }
        public string DocNo { get; set; }
        public string MasterId { get; set; }
        public string DetailsId { get; set; }
        public string Cust_ID_NO { get; set; }
        public string CUSTOMER_NAME { get; set; }
        public string ACCT_NO { get; set; }
        public string JRNL_NO { get; set; }
        public string TRAN_DATE { get; set; }
        public string ATM_TIME { get; set; }
        public string JNRST_TIME { get; set; }
        public string TRAN_BRANCH { get; set; }
        public string TXN_DESC { get; set; }
        public string TRAN_AMT { get; set; }
        public string BALANCE { get; set; }
        public string ATM_NO { get; set; }
        public string TELLER { get; set; }
        public string TRF_BANK { get; set; }
        public string NARRATIVE { get; set; }
        public string PD_TYPE_DESC { get; set; }
        public string CURRENCY { get; set; }
    }
}