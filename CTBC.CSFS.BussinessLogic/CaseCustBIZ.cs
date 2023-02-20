﻿using CTBC.CSFS.Models;
using CTBC.CSFS.Pattern;
using CTBC.FrameWork.Platform;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
namespace CTBC.CSFS.BussinessLogic
{
    public class CaseCustBIZ : CommonBIZ
    {
        string TodayYYYMMDD = "";
        CaseCustCommonBIZ pCaseCustCommonBIZ = new CaseCustCommonBIZ();

        /// <summary>
        /// 查詢外文啟動查詢清單資料
        /// </summary>
        /// <param name="model"></param>
        /// <param name="pageNum"></param>
        /// <returns></returns>
        public IList<CaseCustQuery> GetQueryList(CaseCustQuery model, int pageNum, string strSortExpression, string strSortDirection)
        {
            try
            {
                base.PageIndex = pageNum;

                #region sql
                StringBuilder sql = new StringBuilder();

                sql.Append(@"
                             select
                            	RowNum
                            	,DocNo　
                            	,FileNo
                            	,Govement
                            	,CustIdNo
                            	,SearchProgram 
                            	,RecvDate　
                            	,CaseStatus 
                                ,CaseStatusName
                            	,StatusReason
                                ,LimitDate　--限辦日期(T+7) 
                            	,RecvDate5  --T+5
                                ,VersionKey
                                ,ShowDocNo
                            from (
                            	select 
                            		ROW_NUMBER() OVER ( ORDER BY " + strSortExpression + "  " + strSortDirection + @" ) AS RowNum
                            		,CaseCustQuery.DocNo　--案件編碼(E+12碼)
                                	,CaseCustQuery.LetterNo as FileNo    --來文字號
                                    ,CaseCustQuery.LetterDeptName as Govement    --來文機關
                                	,CaseCustQueryVersion.CustIdNo --客戶ID 
                                	,case when isnull(CaseCustQueryVersion.OpenFlag,'')='Y' then '1.基本資料<br />'　else '' end 
                                     +case when isnull(CaseCustQueryVersion.TransactionFlag,'')='Y' then '2.存款明細'　else '' end as SearchProgram  --查詢項目       
                                	,convert(nvarchar(10),CaseCustQuery.RecvDate,111) as RecvDate　--限辦日期(T+7) 
                                	,isnull(CaseCustQueryVersion.Status,'') as CaseStatus --案件狀態 
                                    , PARMCode.CodeDesc as  CaseStatusName
	                                ,case 
	                                    when CaseCustQueryVersion.Status = '01' then CaseCustQuery.ImportMessage
	                                    else  
										  case when isnull(CaseCustQuery.ImportMessage,'') <> '' 
	                                                then CaseCustQuery.ImportMessage +'<br />'   　
	                                    			else '' end 
	                                          + 
	                                           case when isnull(CaseCustQueryVersion.HTGQryMessage,'') <> '' 
	                                                then 'HTG錯誤：' + CaseCustQueryVersion.HTGQryMessage +'<br />'   　
	                                    			else '' end 
	                                          + 
	                                           case when isnull(CaseCustQueryVersion.RFDMQryMessage,'') <> '' 
	                                                then 'RFDM錯誤：' + CaseCustQueryVersion.RFDMQryMessage 　
	                                    	        else '' end
                                    end as StatusReason --狀態原因
	                                ,'' as LimitDate　--限辦日期(T+7) 
	                                ,'' as RecvDate5  --T+5
                                    ,CaseCustQueryVersion.NewID as VersionKey
                                	,CaseCustQuery.DocNo 
                                    + case when CaseCustQuery.Version = 0 then '' 
                                         else '-' + CONVERT(nvarchar(10), CaseCustQuery.Version) end  
                                    as ShowDocNo   --顯示在清單中案件編碼
                            from CaseCustQuery
                            inner join CaseCustQueryVersion
                            on CaseCustQueryVersion.CaseCustNewID = CaseCustQuery.NewID
                            left join PARMCode
                            on CaseCustQueryVersion.Status = PARMCode.CodeNo
                            and CodeType = 'CaseCustStatus'
                            WHERE ( isnull( CaseCustQueryVersion.Status,'') = '01'
                                or isnull( CaseCustQueryVersion.Status,'') = '02'
                            	or isnull( CaseCustQueryVersion.Status,'') = '04')
                            ) as tabletemp 
                           ");

                // 判斷是否分頁
                sql.Append(@" WHERE  RowNum > " + PageSize * (pageNum - 1)
                                   + " AND RowNum < " + ((PageSize * pageNum) + 1));
                #endregion

                // 資料總筆數
                string sqlCount = @"
                                select
                                    count(0)
                                from CaseCustQuery
                                inner join CaseCustQueryVersion
                                on CaseCustQueryVersion.CaseCustNewID = CaseCustQuery.NewID
                                left join PARMCode
                                on CaseCustQueryVersion.Status = PARMCode.CodeNo
                                and CodeType = 'CaseCustStatus'
                                WHERE ( isnull( CaseCustQueryVersion.Status,'') = '01'
                                    or isnull( CaseCustQueryVersion.Status,'') = '02'
                                	or isnull( CaseCustQueryVersion.Status,'') = '04') ";
                base.DataRecords = int.Parse(base.ExecuteScalar(sqlCount).ToString());

                // 查詢清單資料
                IList<CaseCustQuery> _ilsit = base.SearchList<CaseCustQuery>(sql.ToString());

                if (_ilsit != null)
                {
                    // 處理日期
                    for (int i = 0; i < _ilsit.Count; i++)
                    {
                        _ilsit[i].LimitDate = pCaseCustCommonBIZ.DateAdd(_ilsit[i].RecvDate, 7);
                    }
                    return _ilsit;
                }
                else
                {
                    return new List<CaseCustQuery>();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 刪除清單資料
        /// </summary>
        /// <param name="Content"></param>
        /// <returns></returns>
        public bool DeleteCaseCustQuery(string Content)
        {
            IDbConnection dbConnection = OpenConnection();
            IDbTransaction dbTrans = null;
            try
            {
                using (dbConnection)
                {
                    dbTrans = dbConnection.BeginTransaction();

                    // 將字串拆分成數組
                    string[] arrayNewID = Content.Split(',');

                    string sql = "";
                    base.Parameter.Clear();

                    // 遍歷，組刪除sql
                    for (int i = 0; i < arrayNewID.Length; i++)
                    {
                        sql += " delete from CaseCustQueryVersion where NewID = @NewID" + i.ToString() + "; ";

                        base.Parameter.Add(new CommandParameter("@NewID" + i.ToString(), arrayNewID[i]));
                    }

                    base.ExecuteNonQuery(sql);

                    // 若案件下所有待查ID都被刪除了，同步刪除主案件
                    sql = @"delete from CaseCustQuery
                            where (Status = '01')
                            AND NewID in 
                             (select CaseCustQuery.NewID as CaseCustNewID
                              from CaseCustQuery
                              left join CaseCustQueryVersion on CaseCustQuery.NewID = CaseCustQueryVersion.CaseCustNewID
                              group by CaseCustQuery.NewID
                              having count(CaseCustQueryVersion.CaseCustNewID) = 0
                             )";

                    base.Parameter.Clear();
                    base.ExecuteNonQuery(sql);

                    dbTrans.Commit();

                    return true;
                }
            }
            catch (Exception ex)
            {
                dbTrans.Rollback();

                return false;
            }
        }

        /// <summary>
        /// 檢查是否為外國人(新版)
        /// </summary>
        /// <param name="sei"></param>
        /// <returns></returns>
        private static bool isForeignID(string strID)
        {
            bool result = false;

            //Regex reHumanForeign = new Regex(@"^[A-Z]{2}\d{8,10}");
            // 20200828, 改變外國人規則為第2碼為... 8或9
            Regex reHumanForeign = new Regex(@"^[A-Z]{1}[A-Z|8-9]{1}\d+$");

            if (reHumanForeign.IsMatch(strID)) // 個人
            {
                result = true;
            }

            return result;
        }

        /// <summary>
        /// 啟動發查
        /// </summary>
        /// <returns></returns>
        public string StartSearch(User logonUser)
        {
            string pUserAccount = logonUser.Account;

            // 查詢案件狀態爲未處理資料
            DataTable dt = GetCaseCustQueryData();

            TodayYYYMMDD = DateTime.Now.ToString("yyyyMMdd");

            // 查詢本月最大的流水號
            string pMaxTrnNum = GetMaxTrnNum();

            // 流水號變量
            int pTrnNum = 0;

            // 截取流水號
            if (pMaxTrnNum != "")
            {
                pTrnNum = Convert.ToInt32(pMaxTrnNum.Substring(8, 5));
            }

            IDbConnection dbConnection = OpenConnection();
            IDbTransaction dbTrans = null;

            try
            {
                using (dbConnection)
                {
                    dbTrans = dbConnection.BeginTransaction();

                    base.Parameter.Clear();

                    if (dt != null && dt.Rows.Count > 0)
                    {
                        // 記錄CaseCustQuery主鍵值/sql語句變量
                        string strLastNewID = "";
                        string updateSql = "";

                        // 記錄不符合查詢迄日的案件編號
                        StringBuilder notWorkNoList = new StringBuilder("");

                        int diffDay = GetParmCodeEndDateDiff();

                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            // 20200903, 若是外國人, 則回status='04', [HTGQryMessage]='外國人身份證字號'
                            if (isForeignID(dt.Rows[i]["CustIdNo"].ToString().Trim()))
                            {
                                // 
                                string sql = string.Format("update [dbo].[CaseCustQueryVersion] set Status='55',HTGQryMessage='統一證號格式不符' where NewID='{0}';", dt.Rows[i]["NewID"].ToString());
                                sql += string.Format("update CaseCustQuery set Status='55' where NewID='{0}'", dt.Rows[i]["CaseCustNewID"].ToString());
                                base.ExecuteNonQuery(sql, dbTrans);
                                continue;
                            }

                            // 若已是不可發查案件的ID，則跳過
                            if (notWorkNoList.ToString().IndexOf(dt.Rows[i]["DocNo"].ToString()) > -1)
                            {
                                continue;

                            }

                            string dtQDateS = "";
                            string dtQDateE = "";

                            if (dt.Rows[i]["QDateS"] != null && dt.Rows[i]["QDateS"].ToString().Trim() != "")
                            {
                                dtQDateS = dt.Rows[i]["QDateS"].ToString().Substring(0, 4) + "/" + dt.Rows[i]["QDateS"].ToString().Substring(4, 2) + "/" + dt.Rows[i]["QDateS"].ToString().Substring(6, 2);
                            }

                            if (dt.Rows[i]["QDateE"] != null && dt.Rows[i]["QDateE"].ToString().Trim() != "")
                            {
                                dtQDateE = dt.Rows[i]["QDateE"].ToString().Substring(0, 4) + "/" + dt.Rows[i]["QDateE"].ToString().Substring(4, 2) + "/" + dt.Rows[i]["QDateE"].ToString().Substring(6, 2);
                            }

                            // 20180323,PeterHsieh : 中信增加規格，當查詢迄日 > (系統日-n)，則此筆不作發查 (因RFDM無法查到資料)
                            DateTime eDate = new DateTime();
                            DateTime eDateDiff = new DateTime();

                            Boolean processFlag = false;

                            if (string.IsNullOrEmpty(dtQDateS) || string.IsNullOrEmpty(dtQDateE))
                            {
                                // 不查交易明細，繼續作業
                                processFlag = true;
                            }
                            else
                            {
                                eDate = Convert.ToDateTime(dtQDateE);
                                eDateDiff = Convert.ToDateTime(DateTime.Today.AddDays(-diffDay).ToShortDateString());

                                if (eDate > eDateDiff)
                                {
                                    // 不可發查案件，確認案件編號是否已記錄，若尚未記錄才寫入
                                    if (notWorkNoList.ToString().IndexOf(dt.Rows[i]["DocNo"].ToString()) == -1)
                                    {
                                        notWorkNoList.AppendLine(dt.Rows[i]["DocNo"].ToString());
                                    }

                                    processFlag = false;
                                }
                                else
                                {
                                    // 符合發查規則，繼續作業
                                    processFlag = true;
                                }
                            }

                            if (processFlag)
                            {
                                updateSql += @" update BOPS000401Send 
                                                set sendstatus = '02'
	                                                ,QueryErrMsg = ''
                                                    ,ModifiedUser = @CreatedUser
                                                    ,ModifiedDate = getdate() 
                                            WHERE VersionNewID = @VersionNewID401" + i.ToString() + @" 
                                            and sendstatus = '03';
                                            update BOPS060490Send 
                                                set sendstatus = '02'
                                                    ,QueryErrMsg = ''
                                                    ,ModifiedUser = @CreatedUser
                                                    ,ModifiedDate = getdate() 
                                            WHERE VersionNewID = @VersionNewID401" + i.ToString() + @" 
                                            and sendstatus = '03'; ";

                                base.Parameter.Add(new CommandParameter("@VersionNewID401" + i.ToString(), dt.Rows[i]["NewID"]));

                                #region insert 或 update  BOPS060628Send/BOPS067050Send
                                // 當OpenFlag=“Y”時，更新BOPS060628Send
                                if (dt.Rows[i]["OpenFlag"] != null && dt.Rows[i]["OpenFlag"].ToString() == "Y")
                                {
                                    #region BOPS060628Send
                                    // 判斷資料是否存在與BOPS060628Send，存在就update，否則就insert
                                    if (dt.Rows[i]["SendNewID"] != null && dt.Rows[i]["SendNewID"].ToString() != "")
                                    {
                                        updateSql += @"
                                                update BOPS060628Send
                                                set SendStatus = '02'
                                                	,QueryErrMsg = ''
                                                    ,ModifiedUser = @CreatedUser
                                                    ,ModifiedDate = getdate()
                                                where NewID = @60628NewID" + i.ToString() +
                                                " and SendStatus = '03'; ";

                                        base.Parameter.Add(new CommandParameter("@60628NewID" + i.ToString(), dt.Rows[i]["SendNewID"]));
                                    }
                                    else
                                    {
                                        updateSql += @" insert BOPS060628Send
                                             (
                                             NewID
                                             ,VersionNewID
                                             , CustIdNo
                                             , CreatedUser
                                             , CreatedDate
                                             , ModifiedUser
                                             , ModifiedDate
                                             , SendStatus
                                             )
                                             values(
                                             newid()
                                             ,@60628VersionNewID" + i.ToString() + @"
                                             , @CustIdNo" + i.ToString() + @"
                                             , @CreatedUser
                                             , getdate()
                                             , @CreatedUser
                                             , getdate()
                                             , '02'
                                             ); ";
                                        base.Parameter.Add(new CommandParameter("@60628VersionNewID" + i.ToString(), dt.Rows[i]["NewID"]));
                                        base.Parameter.Add(new CommandParameter("@CustIdNo" + i.ToString(), dt.Rows[i]["CustIdNo"]));
                                    }

                                    #endregion

                                    #region BOPS067050Send

                                    // 判斷資料是否存在與BOPS060628Send，存在就update，否則就insert
                                    if (dt.Rows[i]["BOPS067050SendNewID"] != null && dt.Rows[i]["BOPS067050SendNewID"].ToString() != "")
                                    {
                                        updateSql += @"
                                                update BOPS067050Send
                                                set SendStatus = '02'
                                                	,QueryErrMsg = ''
                                                    ,ModifiedUser = @CreatedUser
                                                    ,ModifiedDate = getdate()
                                                where NewID = @67050SendNewID" + i.ToString() +
                                                " and SendStatus = '03'; ";
                                        base.Parameter.Add(new CommandParameter("@67050SendNewID" + i.ToString(), dt.Rows[i]["BOPS067050SendNewID"]));
                                    }
                                    else
                                    {
                                        updateSql += @" insert BOPS067050Send
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
                                             @BOPS067050SendNewID" + i.ToString() + @"
                                             , @67050VersionNewID" + i.ToString() + @"
                                             , @67050CustIdNo" + i.ToString() + @"
                                             , 'D'
                                             , @CreatedUser
                                             , getdate()
                                             , @CreatedUser
                                             , getdate()
                                             , '02'
                                             ); ";

                                        base.Parameter.Add(new CommandParameter("@BOPS067050SendNewID" + i.ToString(), Guid.NewGuid()));
                                        base.Parameter.Add(new CommandParameter("@67050VersionNewID" + i.ToString(), dt.Rows[i]["NewID"]));
                                        base.Parameter.Add(new CommandParameter("@67050CustIdNo" + i.ToString(), dt.Rows[i]["CustIdNo"]));
                                    }
                                    #endregion
                                }
                                #endregion

                                #region insert 或 update CaseCustRFDMSend
                                if (dt.Rows[i]["TransactionFlag"] != null && dt.Rows[i]["TransactionFlag"].ToString() == "Y")
                                {
                                    DataTable dtRFDMSend = QueryRFDMSend(dt.Rows[i]["NewID"].ToString());

                                    if (dtRFDMSend == null || dtRFDMSend.Rows.Count == 0)
                                    {
                                        updateSql += @" insert CaseCustRFDMSend
                                             (
                                             TrnNum, VersionNewID, ID_No, Type, RFDMSendStatus, CreatedUser, CreatedDate, ModifiedUser, ModifiedDate,acctDesc,curr,channel,Start_Jnrst_Date,End_Jnrst_Date
                                             )
                                             values(
                                             @TrnNumS" + i.ToString() + @"
                                             , @RFDMVersionNewIDS" + i.ToString() + @"
                                             , @RFDMCustIdNoS" + i.ToString() + @"
                                             , '0'
                                             , '02'
                                             , @CreatedUser
                                             , getdate()
                                             , @CreatedUser
                                             , getdate()
                                             ,'S','TWD','CSFS' ";

                                        if (dtQDateS != "")
                                        {
                                            updateSql += ",'" + dtQDateS + "'";
                                        }
                                        else
                                        {
                                            updateSql += ",NULL";
                                        }
                                        if (dtQDateE != "")
                                        {
                                            updateSql += ",'" + dtQDateE + "'";
                                        }
                                        else
                                        {
                                            updateSql += ",NULL";
                                        }

                                        updateSql += " ); ";

                                        updateSql += @" insert CaseCustRFDMSend
                                             (
                                             TrnNum, VersionNewID, ID_No, Type, RFDMSendStatus, CreatedUser, CreatedDate, ModifiedUser, ModifiedDate,acctDesc,curr,channel,Start_Jnrst_Date,End_Jnrst_Date
                                             )
                                             values(
                                             @TrnNumQ" + i.ToString() + @"
                                             , @RFDMVersionNewIDQ" + i.ToString() + @"
                                             , @RFDMCustIdNoQ" + i.ToString() + @"
                                             , '0'
                                             , '02'
                                             , @CreatedUser
                                             , getdate()
                                             , @CreatedUser
                                             , getdate()
                                             ,'Q','TWD','CSFS' ";

                                        if (dtQDateS != "")
                                        {
                                            updateSql += ",'" + dtQDateS + "'";
                                        }
                                        else
                                        {
                                            updateSql += ",NULL";
                                        }
                                        if (dtQDateE != "")
                                        {
                                            updateSql += ",'" + dtQDateE + "'";
                                        }
                                        else
                                        {
                                            updateSql += ",NULL";
                                        }

                                        updateSql += " ); ";

                                        updateSql += @" insert CaseCustRFDMSend
                                             (
                                             TrnNum, VersionNewID, ID_No, Type, RFDMSendStatus, CreatedUser, CreatedDate, ModifiedUser, ModifiedDate,acctDesc,curr,channel,Start_Jnrst_Date,End_Jnrst_Date
                                             )
                                             values(
                                             @TrnNumT" + i.ToString() + @"
                                             , @RFDMVersionNewIDT" + i.ToString() + @"
                                             , @RFDMCustIdNoT" + i.ToString() + @"
                                             , '0'
                                             , '02'
                                             , @CreatedUser
                                             , getdate()
                                             , @CreatedUser
                                             , getdate()
                                             ,'T','TWD','CSFS' ";

                                        if (dtQDateS != "")
                                        {
                                            updateSql += ",'" + dtQDateS + "'";
                                        }
                                        else
                                        {
                                            updateSql += ",NULL";
                                        }
                                        if (dtQDateE != "")
                                        {
                                            updateSql += ",'" + dtQDateE + "'";
                                        }
                                        else
                                        {
                                            updateSql += ",NULL";
                                        }

                                        updateSql += " ); ";

                                        base.Parameter.Add(new CommandParameter("@TrnNumS" + i.ToString(), CalculateTrnNum(pTrnNum)));
                                        base.Parameter.Add(new CommandParameter("@RFDMCustIdNoS" + i.ToString(), dt.Rows[i]["CustIdNo"]));
                                        base.Parameter.Add(new CommandParameter("@RFDMVersionNewIDS" + i.ToString(), dt.Rows[i]["NewID"]));
                                        pTrnNum++;

                                        base.Parameter.Add(new CommandParameter("@TrnNumQ" + i.ToString(), CalculateTrnNum(pTrnNum)));
                                        base.Parameter.Add(new CommandParameter("@RFDMCustIdNoQ" + i.ToString(), dt.Rows[i]["CustIdNo"]));
                                        base.Parameter.Add(new CommandParameter("@RFDMVersionNewIDQ" + i.ToString(), dt.Rows[i]["NewID"]));
                                        pTrnNum++;

                                        base.Parameter.Add(new CommandParameter("@TrnNumT" + i.ToString(), CalculateTrnNum(pTrnNum)));
                                        base.Parameter.Add(new CommandParameter("@RFDMCustIdNoT" + i.ToString(), dt.Rows[i]["CustIdNo"]));
                                        base.Parameter.Add(new CommandParameter("@RFDMVersionNewIDT" + i.ToString(), dt.Rows[i]["NewID"]));
                                        pTrnNum++;
                                    }
                                    else
                                    {
                                        DataRow[] drArr = dtRFDMSend.Select("acctDesc='S'");

                                        if (drArr.Length > 0)
                                        {
                                            updateSql += @"
                                                update CaseCustRFDMSend
                                                set RFDMSendStatus = '02'
                                                	,RspMsg = ''
                                                    ,ModifiedUser = @CreatedUser
                                                    ,ModifiedDate = getdate()
                                                where TrnNum = @TrnNumS" + i.ToString() +
                                                 " and RFDMSendStatus = '03'; ";

                                            base.Parameter.Add(new CommandParameter("@TrnNumS" + i.ToString(), drArr[0]["TrnNum"].ToString()));
                                        }
                                        else
                                        {
                                            updateSql += @" insert CaseCustRFDMSend
                                             (
                                             TrnNum, VersionNewID, ID_No, Type, RFDMSendStatus, CreatedUser, CreatedDate, ModifiedUser, ModifiedDate,acctDesc,curr,channel,Start_Jnrst_Date,End_Jnrst_Date
                                             )
                                             values(
                                             @TrnNumS" + i.ToString() + @"
                                             , @RFDMVersionNewIDS" + i.ToString() + @"
                                             , @RFDMCustIdNoS" + i.ToString() + @"
                                             , '0'
                                             , '02'
                                             , @CreatedUser
                                             , getdate()
                                             , @CreatedUser
                                             , getdate()
                                             ,'S' ,'TWD','CSFS' ";

                                            if (dtQDateS != "")
                                            {
                                                updateSql += ",'" + dtQDateS + "'";
                                            }
                                            else
                                            {
                                                updateSql += ",NULL";
                                            }
                                            if (dtQDateE != "")
                                            {
                                                updateSql += ",'" + dtQDateE + "'";
                                            }
                                            else
                                            {
                                                updateSql += ",NULL";
                                            }

                                            updateSql += " ); ";

                                            base.Parameter.Add(new CommandParameter("@TrnNumS" + i.ToString(), CalculateTrnNum(pTrnNum)));
                                            base.Parameter.Add(new CommandParameter("@RFDMCustIdNoS" + i.ToString(), dt.Rows[i]["CustIdNo"]));
                                            base.Parameter.Add(new CommandParameter("@RFDMVersionNewIDS" + i.ToString(), dt.Rows[i]["NewID"]));
                                            pTrnNum++;
                                        }

                                        DataRow[] drArrQ = dtRFDMSend.Select("acctDesc='Q'");

                                        if (drArrQ.Length > 0)
                                        {
                                            updateSql += @"
                                                update CaseCustRFDMSend
                                                set RFDMSendStatus = '02'
                                                	,RspMsg = ''
                                                    ,ModifiedUser = @CreatedUser
                                                    ,ModifiedDate = getdate()
                                                where TrnNum = @TrnNumQ" + i.ToString() +
                                                 " and RFDMSendStatus = '03'; ";

                                            base.Parameter.Add(new CommandParameter("@TrnNumQ" + i.ToString(), drArrQ[0]["TrnNum"].ToString()));
                                        }
                                        else
                                        {
                                            updateSql += @" insert CaseCustRFDMSend
                                             (
                                             TrnNum, VersionNewID, ID_No, Type, RFDMSendStatus, CreatedUser, CreatedDate, ModifiedUser, ModifiedDate,acctDesc,curr,channel,Start_Jnrst_Date,End_Jnrst_Date
                                             )
                                             values(
                                             @TrnNumQ" + i.ToString() + @"
                                             , @RFDMVersionNewIDQ" + i.ToString() + @"
                                             , @RFDMCustIdNoQ" + i.ToString() + @"
                                             , '0'
                                             , '02'
                                             , @CreatedUser
                                             , getdate()
                                             , @CreatedUser
                                             , getdate()
                                             ,'Q','TWD','CSFS' ";

                                            if (dtQDateS != "")
                                            {
                                                updateSql += ",'" + dtQDateS + "'";
                                            }
                                            else
                                            {
                                                updateSql += ",NULL";
                                            }
                                            if (dtQDateE != "")
                                            {
                                                updateSql += ",'" + dtQDateE + "'";
                                            }
                                            else
                                            {
                                                updateSql += ",NULL";
                                            }

                                            updateSql += " ); ";

                                            base.Parameter.Add(new CommandParameter("@TrnNumQ" + i.ToString(), CalculateTrnNum(pTrnNum)));
                                            base.Parameter.Add(new CommandParameter("@RFDMCustIdNoQ" + i.ToString(), dt.Rows[i]["CustIdNo"]));
                                            base.Parameter.Add(new CommandParameter("@RFDMVersionNewIDQ" + i.ToString(), dt.Rows[i]["NewID"]));
                                            pTrnNum++;
                                        }

                                        DataRow[] drArrT = dtRFDMSend.Select("acctDesc='T'");

                                        if (drArrT.Length > 0)
                                        {
                                            updateSql += @"
                                                update CaseCustRFDMSend
                                                set RFDMSendStatus = '02'
                                                	,RspMsg = ''
                                                    ,ModifiedUser = @CreatedUser
                                                    ,ModifiedDate = getdate()
                                                where TrnNum = @TrnNumT" + i.ToString() +
                                                 " and RFDMSendStatus = '03'; ";

                                            base.Parameter.Add(new CommandParameter("@TrnNumT" + i.ToString(), drArrT[0]["TrnNum"].ToString()));
                                        }
                                        else
                                        {

                                            updateSql += @" insert CaseCustRFDMSend
                                             (
                                             TrnNum, VersionNewID, ID_No, Type, RFDMSendStatus, CreatedUser, CreatedDate, ModifiedUser, ModifiedDate,acctDesc,curr,channel,Start_Jnrst_Date,End_Jnrst_Date
                                             )
                                             values(
                                             @TrnNumT" + i.ToString() + @"
                                             , @RFDMVersionNewIDT" + i.ToString() + @"
                                             , @RFDMCustIdNoT" + i.ToString() + @"
                                             , '0'
                                             , '02'
                                             , @CreatedUser
                                             , getdate()
                                             , @CreatedUser
                                             , getdate()
                                             ,'T','TWD','CSFS' ";

                                            if (dtQDateS != "")
                                            {
                                                updateSql += ",'" + dtQDateS + "'";
                                            }
                                            else
                                            {
                                                updateSql += ",NULL";
                                            }
                                            if (dtQDateE != "")
                                            {
                                                updateSql += ",'" + dtQDateE + "'";
                                            }
                                            else
                                            {
                                                updateSql += ",NULL";
                                            }

                                            updateSql += " ); ";

                                            base.Parameter.Add(new CommandParameter("@TrnNumT" + i.ToString(), CalculateTrnNum(pTrnNum)));
                                            base.Parameter.Add(new CommandParameter("@RFDMCustIdNoT" + i.ToString(), dt.Rows[i]["CustIdNo"]));
                                            base.Parameter.Add(new CommandParameter("@RFDMVersionNewIDT" + i.ToString(), dt.Rows[i]["NewID"]));
                                            pTrnNum++;
                                        }
                                    }

                                    #region CaseCustRFDMSend

                                    #endregion
                                }

                                #endregion

                                #region  update  CaseCustQueryVersion
                                updateSql += @"
                                    update  CaseCustQueryVersion
                                    set Status = '02' ";

                                // 更新HTGSendStatus欄位，'OpenFlag為Y AND (HTGSendStatus 為 0 或6) ,則存2，否則存空
                                if (dt.Rows[i]["OpenFlag"] != null && dt.Rows[i]["OpenFlag"].ToString() == "Y"
                                    && (dt.Rows[i]["HTGSendStatus"] != null &&
                                    (dt.Rows[i]["HTGSendStatus"].ToString() == "0" || dt.Rows[i]["HTGSendStatus"].ToString() == "6")))
                                {
                                    updateSql += @",HTGSendStatus = '2'
                                               ,HTGQryMessage = ''
                                               ,HTGModifiedDate = getdate() ";
                                }

                                // 更新RFDMSendStatus欄位，'TransactionFlag為Y AND (RFDMSendStatus 為 0 或6) ,則存2，否則存空
                                if (dt.Rows[i]["TransactionFlag"] != null && dt.Rows[i]["TransactionFlag"].ToString() == "Y"
                                    && (dt.Rows[i]["RFDMSendStatus"] != null
                                    && (dt.Rows[i]["RFDMSendStatus"].ToString() == "0" || dt.Rows[i]["RFDMSendStatus"].ToString() == "6")))
                                {
                                    updateSql += @",RFDMSendStatus = '2' 
                                               ,RFDMQryMessage = ''
                                               ,RFDModifiedDate = getdate() ";
                                }


                                updateSql += @"
                                            ,ModifiedDate = getdate()
                                            ,ModifiedUser = @CreatedUser 
                                    where NewID = @NewID" + i.ToString() + "; ";

                                base.Parameter.Add(new CommandParameter("@NewID" + i.ToString(), dt.Rows[i]["NewID"]));

                                #endregion

                                #region update  CaseCustQuery
                                // 判斷該筆資料與上筆資料是否屬於同一個案件，如果不屬於，就更新該資料所屬案件的案件狀態欄位
                                if (strLastNewID != dt.Rows[i]["CaseCustNewID"].ToString())
                                {
                                    // CaseCustQuery狀態改爲拋查中
                                    updateSql += @"
                                    update  CaseCustQuery
                                    set Status = '02'
                                        ,QueryUserID = @CreatedUser
                                        ,ModifiedDate = getdate()
                                        ,ModifiedUser = @CreatedUser
                                    where NewID = @CaseCustNewID" + i.ToString() + "; ";

                                    base.Parameter.Add(new CommandParameter("@CaseCustNewID" + i.ToString(), dt.Rows[i]["CaseCustNewID"]));
                                }
                                #endregion

                                strLastNewID = dt.Rows[i]["CaseCustNewID"].ToString();

                            }
                        }

                        // 若有可發查案件，才更新表格
                        if (!string.IsNullOrEmpty(updateSql))
                        {
                            base.Parameter.Add(new CommandParameter("@CreatedUser", pUserAccount));

                            base.ExecuteNonQuery(updateSql, dbTrans);

                            // insert或update ApprMsgKey表
                            SaveMsg(dt, logonUser, dbTrans);

                        }

                        dbTrans.Commit();

                        // 回傳全部都有發查，或是部份發查
                        if (string.IsNullOrEmpty(notWorkNoList.ToString()))
                        {
                            return "OK";

                        }
                        else
                        {
                            return string.Format("SubOK:{0}", notWorkNoList.ToString().Replace("\r", ""));

                        }

                    }
                    else
                    {
                        return "NoData";
                    }
                }
            }
            catch (Exception ex)
            {
                dbTrans.Rollback();
                return "Error";
            }
        }

        /// <summary>
        /// 查詢需要啟動發查資料
        /// </summary>
        /// <returns></returns>
        public DataTable GetCaseCustQueryData()
        {
            try
            {

                string sql = @"
                            select 
								         CaseCustQuery.DocNo
                                ,CaseCustQueryVersion.NewID
                            	,CaseCustQueryVersion.CaseCustNewID
                                ,CaseCustQueryVersion.CustIdNo
                            	,isnull(CaseCustQueryVersion.OpenFlag,'') as OpenFlag
                            	,isnull(CaseCustQueryVersion.TransactionFlag,'') as TransactionFlag
                            	,isnull(CaseCustQueryVersion.HTGSendStatus,'') as HTGSendStatus
                            	,isnull(CaseCustQueryVersion.RFDMSendStatus,'') as RFDMSendStatus
                            	,BOPS060628Send.NewID as SendNewID
                            	,BOPS060628Send.SendStatus
                            	,BOPS067050Send.NewID as BOPS067050SendNewID
                            	--,CaseCustRFDMSend.TrnNum as TrnNum
                                ,CaseCustQueryVersion.QDateS
                                ,CaseCustQueryVersion.QDateE
                            from CaseCustQueryVersion
                            inner join CaseCustQuery
                            on CaseCustQuery.NewID = CaseCustQueryVersion.CaseCustNewId
                            left join BOPS060628Send
                            on BOPS060628Send.VersionNewID = CaseCustQueryVersion.NewID
                            left join BOPS067050Send
                            on BOPS067050Send.VersionNewID = CaseCustQueryVersion.NewID
                            --left join CaseCustRFDMSend
                            --on CaseCustRFDMSend.VersionNewID = CaseCustQueryVersion.NewID
                            where CaseCustQueryVersion.Status IN ('04', '01')
                            order by CaseCustQuery.DocNo,CaseCustQueryVersion.CaseCustNewID,CaseCustQueryVersion.QDateE desc ";

                return base.Search(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public DataTable QueryRFDMSend(string strVersionNewID)
        {
            try
            {

                string sql = @"
                            SELECT 
                                CaseCustRFDMSend.TrnNum as TrnNum
                                ,ISNULL(acctDesc,'') as acctDesc
                            FROM CaseCustRFDMSend
                            WHERE VersionNewID = '" + strVersionNewID + "'";

                return base.Search(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 案件總筆數
        /// </summary>
        /// <returns></returns>
        public int GetDataCount()
        {
            try
            {
                int DataCount = 0;

                string sql = @"
                            select 
                            	CaseCustNewID
                            from CaseCustQuery
                            inner join CaseCustQueryVersion
                            on CaseCustQueryVersion.CaseCustNewID = CaseCustQuery.NewID
                            WHERE  isnull( CaseCustQueryVersion.Status,'') = '01'
                                or isnull( CaseCustQueryVersion.Status,'') = '02'
                            	or isnull( CaseCustQueryVersion.Status,'') = '04'
                            group by CaseCustNewID ";

                DataTable dt = base.Search(sql);

                if (dt != null && dt.Rows.Count > 0)
                {
                    DataCount = dt.Rows.Count;
                }

                return DataCount;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #region insert ApprMsgKey

        /// <summary>
        /// insert或update ApprMsgKey表
        /// </summary>
        /// <param name="dt">資料集</param>
        /// <param name="logonUser"><登錄人員信息/param>
        /// <param name="dbTrans">事務</param>
        public void SaveMsg(DataTable dt, User logonUser, IDbTransaction dbTrans)
        {
            for (int j = 0; j < dt.Rows.Count; j++)
            {
                // 當OpenFlag=“Y”時，更新BOPS060628Send
                if (dt.Rows[j]["OpenFlag"].ToString() == "Y")
                {
                    SaveMsgKey(new Guid(dt.Rows[j]["NewID"].ToString()), logonUser, dbTrans);
                }
            }
        }

        /// <summary>
        /// 保存ApprMsgKey資料
        /// </summary>
        /// <param name="strVersionNewID">VersionNewID</param>
        /// <param name="logonUser">登錄人員資料</param>
        /// <param name="dbTrans">事務</param>
        /// <returns></returns>
        public bool SaveMsgKey(Guid strVersionNewID, User logonUser, IDbTransaction dbTrans)
        {
            try
            {
                bool flag = false;

                // 獲取登錄人員資料
                ApprMsgKeyVO model = new ApprMsgKeyVO();
                model.MsgUID = logonUser.Account;
                model.MsgKeyLP = logonUser.LDAPPwd;
                model.MsgKeyLU = logonUser.Account;
                model.MsgKeyRU = logonUser.RCAFAccount;
                model.MsgKeyRP = logonUser.RCAFPs;
                model.MsgKeyRB = logonUser.RCAFBranch;

                // VersionNewID
                model.VersionNewID = strVersionNewID;

                // 判斷資料是否存在ApprMsgKey,如果不存在就可向ApprMsgKey增加資料
                if (!isExistInMsgKey(strVersionNewID, logonUser.Account, dbTrans))
                {
                    flag = InsertApprMsgKey(model, dbTrans);
                }
                return flag;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 判斷是否存在ApprMsgKey
        /// </summary>
        /// <param name="strVersionNewID"></param>
        /// <param name="logonUser">登錄人員ID</param>
        /// <param name="dbTrans">事務</param>
        /// <returns></returns>
        public bool isExistInMsgKey(Guid strVersionNewID, string logonUser, IDbTransaction dbTrans)
        {
            try
            {
                string strSql = @"SELECT  COUNT(*)
                                  FROM    dbo.ApprMsgKey
                                  WHERE   VersionNewID = @VersionNewID
                                          AND MsgUID = @MsgUID ";
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@VersionNewID", strVersionNewID));
                base.Parameter.Add(new CommandParameter("@MsgUID", Encode(logonUser)));
                int n = (int)base.ExecuteScalar(strSql, dbTrans);
                if (n > 0) return true;
                else return false;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// insert ApprMsgKey
        /// </summary>
        /// <param name="model">實體資料</param>
        /// <param name="dbTrans">事務</param>
        /// <returns></returns>
        public bool InsertApprMsgKey(ApprMsgKeyVO model, IDbTransaction dbTrans)
        {
            try
            {
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
                                                     @VersionNewID )";
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@MsgKeyLU", Encode(model.MsgKeyLU)));
                base.Parameter.Add(new CommandParameter("@MsgKeyLP", Encode(model.MsgKeyLP)));
                base.Parameter.Add(new CommandParameter("@MsgKeyRU", Encode(model.MsgKeyRU)));
                base.Parameter.Add(new CommandParameter("@MsgKeyRP", Encode(model.MsgKeyRP)));
                base.Parameter.Add(new CommandParameter("@MsgKeyRB", Encode(model.MsgKeyRB)));
                base.Parameter.Add(new CommandParameter("@MsgUID", Encode(model.MsgUID)));
                base.Parameter.Add(new CommandParameter("@VersionNewID", model.VersionNewID));
                return base.ExecuteNonQuery(sql, dbTrans) > 0 ? true : false;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 加密
        /// </summary>
        /// <param name="data">加密字串</param>
        /// <returns></returns>
        public string Encode(string data)
        {
            string KEY_64 = "VavicApp";
            string IV_64 = "VavicApp";

            byte[] byKey = System.Text.ASCIIEncoding.ASCII.GetBytes(KEY_64);
            byte[] byIV = System.Text.ASCIIEncoding.ASCII.GetBytes(IV_64);

            DESCryptoServiceProvider cryptoProvider = new DESCryptoServiceProvider();
            int i = cryptoProvider.KeySize;
            MemoryStream ms = new MemoryStream();
            CryptoStream cst = new CryptoStream(ms, cryptoProvider.CreateEncryptor(byKey, byIV), CryptoStreamMode.Write);
            StreamWriter sw = new StreamWriter(cst);
            sw.Write(data);
            sw.Flush();
            cst.FlushFinalBlock();
            sw.Flush();

            return Convert.ToBase64String(ms.GetBuffer(), 0, (int)ms.Length);
        }

        /// <summary>
        /// 解密
        /// </summary>
        /// <param name="data">解密字串</param>
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

        #endregion

        /// <summary>
        /// 查詢CaseCustRFDMSend中本月最大的流水號
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
        /// 計算流水號
        /// YYYMMDD+4位數流水號
        /// </summary>
        /// <param name="strMaxNo">根據最大流水號+1(純數字流水號)</param>
        /// <returns></returns>
        private string CalculateTrnNum(int strMaxNo)
        {
            return TodayYYYMMDD + String.Format("{0:D5}", strMaxNo + 1);
        }

        /// <summary>
        /// 取得查詢迄日要往前 n日
        /// </summary>
        /// <returns>往前 n日</returns>
        public int GetParmCodeEndDateDiff()
        {
            string sqlSelect = @"Select CodeDesc from PARMCode where CodeType='CSFSCode' and CodeNo='PreDay' ";

            // 清空容器
            base.Parameter.Clear();

            string day = base.Search(sqlSelect).Rows[0][0].ToString();

            return Convert.ToInt16(string.IsNullOrEmpty(day) ? "3" : day);
        }


    }
}
