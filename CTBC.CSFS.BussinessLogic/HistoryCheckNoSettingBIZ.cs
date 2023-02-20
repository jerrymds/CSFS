using CTBC.CSFS.Models;
using CTBC.CSFS.Pattern;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CTBC.CSFS.Resource;
using CTBC.CSFS.ViewModels;
using ICSharpCode.SharpZipLib.Core;
using NPOI.OpenXmlFormats.Dml;

namespace CTBC.CSFS.BussinessLogic
{
    public class HistoryCheckNoSettingBIZ : CommonBIZ
    {
        public HistoryCheckNoSettingBIZ(AppController appController)
            : base(appController)
        { }

        public HistoryCheckNoSettingBIZ()
        { }

        #region 支票本
        /// <summary>
        /// 查詢支票本
        /// </summary>
        /// <param name="cns"></param>
        /// <param name="pageIndex"></param>
        /// <param name="strSortExpression"></param>
        /// <param name="strSortDirection"></param>
        /// <returns></returns>
        public IList<HistoryCheckNoSetting> GetQueryList(HistoryCheckNoSetting cns, int pageIndex, string strSortExpression, string strSortDirection)
        {
            try
            {
                base.PageIndex = pageIndex;
                string sqlStr = "";
                string sqlWhere = "";
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@pageS", (base.PageSize * (base.PageIndex - 1)) + 1));
                base.Parameter.Add(new CommandParameter("@pageE", base.PageSize * base.PageIndex));

                if (cns.CheckIntervalID != 0)
                {
                    sqlWhere += @" and CheckIntervalID like @CheckIntervalID ";
                    base.Parameter.Add(new CommandParameter("@CheckIntervalID", "%" + cns.CheckIntervalID.ToString().Trim() + "%"));
                }
                sqlStr += @";with T1 
	                        as
	                        (
		                       SELECT [CheckIntervalID]
                               ,[StartNo] as [CheckNoStart]
                               ,[EndNo] as [CheckNoEnd]
                               ,[WeekTempAmount]
                               ,[UseStatus]
                               FROM [History_CheckInterval] where 1=1" + sqlWhere + @"
	                        ),T2 as
	                        (
		                        select *, row_number() over (order by " + strSortExpression + " " + strSortDirection + @" ) RowNum
		                        from T1
	                        ),T3 as 
	                        (
		                        select *,(select max(RowNum) from T2) maxnum from T2 
		                        where rownum between @pageS and @pageE
	                        )
	                        select a.* from T3 a ";


                IList<HistoryCheckNoSetting> _ilsit = base.SearchList<HistoryCheckNoSetting>(sqlStr);

                if (_ilsit.Count > 0)
                {
                    base.DataRecords = _ilsit[0].maxnum;
                }
                else
                {
                    base.DataRecords = 0;
                    _ilsit = new List<HistoryCheckNoSetting>();
                }
                return _ilsit;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 新增支票本
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public int Create(HistoryCheckNoSetting model)
        {
            int rtn = 0;
            IDbConnection dbConnection = base.OpenConnection();
            IDbTransaction dbTransaction = null;
            try
            {
                using (dbConnection)
                {
                    dbTransaction = dbConnection.BeginTransaction();
                    string sqlStr = @"DECLARE @ID BIGINT; 
                                      SELECT @ID=ISNULL(MAX(CheckIntervalID),0) +1 FROM History_CheckInterval;
                                      INSERT INTO History_CheckInterval
                                      (CheckIntervalID,StartNo,EndNo,WeekTempAmount,UseStatus)
                                      values(@ID,@CheckNoStart,@CheckNoEnd,@WeekTempAmount,@UseStatus);";

                    base.Parameter.Clear();
                    base.Parameter.Add(new CommandParameter("@CheckNoStart", model.CheckNoStart));
                    base.Parameter.Add(new CommandParameter("@CheckNoEnd", model.CheckNoEnd));
                    base.Parameter.Add(new CommandParameter("@WeekTempAmount", model.WeekTempAmount));
                    base.Parameter.Add(new CommandParameter("@UseStatus", model.UseStatus));

                    rtn = base.ExecuteNonQuery(sqlStr, dbTransaction);
                    dbTransaction.Commit();
                }
                return rtn;
            }
            catch (Exception ex)
            {
                try
                {
                    dbTransaction.Rollback();
                }
                catch (Exception ex2)
                {
                }
                throw ex;
            }
        }
        /// <summary>
        /// 取得支票本設定
        /// </summary>
        /// <param name="CheckIntervalID"></param>
        /// <returns></returns>
        public HistoryCheckNoSetting Select(Int64 CheckIntervalID)
        {
            try
            {
                string sqlStr = @"SELECT [CheckIntervalID]
                               ,[StartNo] as [CheckNoStart]
                               ,[EndNo] as [CheckNoEnd]
                               ,[WeekTempAmount]
                               ,[UseStatus] 
                                from History_CheckInterval where CheckIntervalID=@CheckIntervalID";

                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@CheckIntervalID", CheckIntervalID));

                IList<HistoryCheckNoSetting> list = base.SearchList<HistoryCheckNoSetting>(sqlStr);
                if (list != null)
                {
                    if (list.Count > 0)
                    {
                        return list[0];
                    }
                    else
                    {
                        return new HistoryCheckNoSetting();
                    }
                }
                else
                {
                    return new HistoryCheckNoSetting();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 修改支票本
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public int Edit(HistoryCheckNoSetting model)
        {
            int rtn = 0;
            IDbConnection dbConnection = base.OpenConnection();
            IDbTransaction dbTransaction = null;
            try
            {
                using (dbConnection)
                {
                    dbTransaction = dbConnection.BeginTransaction();
                    string sqlStr = @"update History_CheckInterval set 
                                            StartNo=@CheckNoStart,
                                            EndNo=@CheckNoEnd,
                                            WeekTempAmount=@WeekTempAmount
                                    where CheckIntervalID=@CheckIntervalID";

                    base.Parameter.Clear();
                    base.Parameter.Add(new CommandParameter("@CheckIntervalID", model.CheckIntervalID));
                    base.Parameter.Add(new CommandParameter("@CheckNoStart", model.CheckNoStart));
                    base.Parameter.Add(new CommandParameter("@CheckNoEnd", model.CheckNoEnd));
                    base.Parameter.Add(new CommandParameter("@WeekTempAmount", model.WeekTempAmount));
                    rtn = base.ExecuteNonQuery(sqlStr, dbTransaction);
                    dbTransaction.Commit();
                }
                return rtn;
            }
            catch (Exception ex)
            {
                try
                {
                    dbTransaction.Rollback();
                }
                catch (Exception ex2)
                {
                }
                throw ex;
            }
        }

        /// <summary>
        /// 刪除支票本
        /// </summary>
        /// <param name="CheckIntervalID"></param>
        /// <returns></returns>
        public int Delete(Int64 CheckIntervalID)
        {
            int rtn = 0;
            IDbConnection dbConnection = base.OpenConnection();
            IDbTransaction dbTransaction = null;
            try
            {
                using (dbConnection)
                {
                    dbTransaction = dbConnection.BeginTransaction();
                    string sqlStr = @"delete from History_CheckInterval where CheckIntervalID=@CheckIntervalID";

                    base.Parameter.Clear();
                    base.Parameter.Add(new CommandParameter("@CheckIntervalID", CheckIntervalID));
                    rtn = base.ExecuteNonQuery(sqlStr, dbTransaction);
                    dbTransaction.Commit();
                }
                return rtn;
            }
            catch (Exception ex)
            {
                try
                {
                    dbTransaction.Rollback();
                }
                catch (Exception ex2)
                {
                }
                throw ex;
            }
        }
        /// <summary>
        /// 啟用支票本
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public int Active(CheckNoSetting model)
        {
            int rtn = 0;
            IDbConnection dbConnection = base.OpenConnection();
            IDbTransaction dbTransaction = null;
            try
            {
                using (dbConnection)
                {
                    dbTransaction = dbConnection.BeginTransaction();
                    string sqlStr = @"update History_CheckInterval set UseStatus=@UseStatus where CheckIntervalID=@CheckIntervalID;";
                    base.Parameter.Clear();
                    base.Parameter.Add(new CommandParameter("@CheckIntervalID", model.CheckIntervalID));
                    base.Parameter.Add(new CommandParameter("@UseStatus", model.UseStatus));
                    base.ExecuteNonQuery(sqlStr, dbTransaction);

                    for (Int64 i = model.CheckNoStart; i <= model.CheckNoEnd; i++)
                    {
                        sqlStr = @"INSERT INTO [History_CheckUse] ([CheckIntervalID],[CheckNo],[Kind],[IsUsed],[IsPreserve])VALUES (@CheckIntervalID,@CheckNo,@Kind,@IsUsed,@IsPreserve);";

                        base.Parameter.Clear();
                        base.Parameter.Add(new CommandParameter("@CheckIntervalID", model.CheckIntervalID));
                        base.Parameter.Add(new CommandParameter("@UseStatus", model.UseStatus));
                        base.Parameter.Add(new CommandParameter("@CheckNo", i));
                        base.Parameter.Add(new CommandParameter("@Kind", model.Kind));
                        base.Parameter.Add(new CommandParameter("@IsUsed", model.IsUsed));
                        int IsPreserve = 0;
                        if (model.WeekTempAmount != 0)
                        {
                            if (i - model.CheckNoStart < model.WeekTempAmount)
                            {
                                IsPreserve = 1;
                            }
                        }
                        base.Parameter.Add(new CommandParameter("@IsPreserve", IsPreserve));
                        rtn = base.ExecuteNonQuery(sqlStr, dbTransaction);
                    }
                    dbTransaction.Commit();
                }
                return rtn;
            }
            catch (Exception ex)
            {
                try
                {
                    dbTransaction.Rollback();
                }
                catch (Exception ex2)
                {
                }
                //throw ex;
                return 0;
            }
        }


        /// <summary>
        /// 取得某支票本內的明細
        /// </summary>
        /// <param name="checkIntervalId"></param>
        /// <param name="checkNoS"></param>
        /// <param name="checkNoE"></param>
        /// <param name="pageIndex"></param>
        /// <param name="strSortExpression"></param>
        /// <param name="strSortDirection"></param>
        /// <returns></returns>
        public IList<HistoryCheckNoSetting> GetDetailList(Int64 checkIntervalId, Int32? checkNoS, Int32? checkNoE, int pageIndex, string strSortExpression, string strSortDirection)
        {
            try
            {
                base.PageIndex = pageIndex;
                string sqlStr = "";
                string sqlWhere = "";
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@pageS", (base.PageSize * (base.PageIndex - 1)) + 1));
                base.Parameter.Add(new CommandParameter("@pageE", base.PageSize * base.PageIndex));

                sqlWhere += @" and CheckIntervalID=@CheckIntervalID ";
                base.Parameter.Add(new CommandParameter("@CheckIntervalID", checkIntervalId));
                if (checkNoS.HasValue && checkNoS.Value != 0 )
                {
                    sqlWhere += @" and CheckNo >= @CheckNoS ";
                    Parameter.Add(new CommandParameter("@CheckNoS", checkNoS.Value));
                }
                if (checkNoE.HasValue && checkNoE.Value != 0)
                {
                    sqlWhere += @" and CheckNo <= @CheckNoE ";
                    Parameter.Add(new CommandParameter("@CheckNoE", checkNoE.Value));
                }
                sqlStr += @";with T1 
	                        as
	                        (
		                       SELECT [CheckIntervalID]
                               ,[CheckNo]
                               ,[Kind]
                               ,[IsUsed]
                               ,[IsPreserve]
                               FROM [History_CheckUse] where 1=1" + sqlWhere + @"
	                        ),T2 as
	                        (
		                        select *, row_number() over (order by " + strSortExpression + " " + strSortDirection + @" ) RowNum
		                        from T1
	                        ),T3 as 
	                        (
		                        select *,(select max(RowNum) from T2) maxnum from T2 
		                        where rownum between @pageS and @pageE
	                        )
	                        select a.* from T3 a ";


                IList<HistoryCheckNoSetting> _ilsit = base.SearchList<HistoryCheckNoSetting>(sqlStr);

                if (_ilsit.Count > 0)
                {
                    base.DataRecords = _ilsit[0].maxnum;
                }
                else
                {
                    base.DataRecords = 0;
                    _ilsit = new List<HistoryCheckNoSetting>();
                }
                return _ilsit;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        /// <summary>
        /// 取得第一個沒有使用的支票號碼
        /// </summary>
        /// <returns></returns>
        public HistoryCheckUse GetNoUseCheck(IDbTransaction trans = null)
        {
            string sql = @"SELECT 
	                            D.[CheckIntervalID],
	                            D.[CheckNo],
	                            D.[Kind],
	                            D.[IsUsed],
	                            D.[IsPreserve]
                            FROM [History_CheckInterval] AS M
                            LEFT OUTER JOIN [History_CheckUse] AS D ON M.CheckIntervalID = D.CheckIntervalID
                            WHERE 
	                            M.UseStatus = '已使用' 
	                            AND D.IsUsed = 0 
	                            AND D.IsPreserve = 0
                            ORDER BY M.CheckIntervalID ASC , D.CheckNo ASC ";
            Parameter.Clear();
            IList<HistoryCheckUse> list = trans == null ? SearchList<HistoryCheckUse>(sql) : SearchList<HistoryCheckUse>(sql, trans);
            if (list == null || !list.Any())
                return null;
            return list.FirstOrDefault();
        }
        /// <summary>
        /// 取得第一個沒有使用的保留支票號碼
        /// </summary>
        /// <returns></returns>
        public HistoryCheckUse GetPreserveCheck(IDbTransaction trans = null)
        {
            string sql = @"SELECT 
	                            D.[CheckIntervalID],
	                            D.[CheckNo],
	                            D.[Kind],
	                            D.[IsUsed],
	                            D.[IsPreserve]
                            FROM [History_CheckInterval] AS M
                            LEFT OUTER JOIN [History_CheckUse] AS D ON M.CheckIntervalID = D.CheckIntervalID
                            WHERE 
	                            M.UseStatus = '已使用' 
	                            AND D.IsUsed = 0 
	                            AND D.IsPreserve = 1
                            ORDER BY M.CheckIntervalID ASC , D.CheckNo ASC ";
            Parameter.Clear();
            IList<HistoryCheckUse> list = trans == null ? SearchList<HistoryCheckUse>(sql) : SearchList<HistoryCheckUse>(sql, trans);
            if (list == null || !list.Any())
                return null;
            return list.FirstOrDefault();
        }
        /// <summary>
        /// 使用支票號碼
        /// </summary>
        /// <param name="check"></param>
        /// <param name="trans"></param>
        /// <returns></returns>
        public JsonReturn UseCheck(ref HistoryCheckUse check, IDbTransaction trans = null)
        {
            //* 沒有支票號碼取一個支票號碼
            if (check == null || check == new HistoryCheckUse())
            {
                check = GetNoUseCheck(trans);
                if (check == null)
                {
                    return new JsonReturn { ReturnCode = "0", ReturnMsg = Lang.csfa_NoMax };
                }
            }

            return SettingForUseCheck(new HistoryCheckNoSetting { Kind = CheckNoUseKind.CasePay, IsUsed = 1, CheckNo = check.CheckNo, CheckIntervalID = check.CheckIntervalId }, trans)
                ? new JsonReturn { ReturnCode = "1" }
                : new JsonReturn { ReturnCode = "0", ReturnMsg = Lang.csfs_getticket + Lang.csfs_fail };
        }

        public JsonReturn CancelCheck(string checkIntervalId, string checkNo, string sendId, IDbTransaction trans = null)
        {
            //* 撤銷號碼存檔
            if (!string.IsNullOrEmpty(checkIntervalId) && !string.IsNullOrEmpty(checkNo))
            {
                if (!Setting(new HistoryCheckNoSetting { Kind = CheckNoUseKind.CasePay, IsUsed = 0, CheckNo = Convert.ToInt64(checkNo), CheckIntervalID = Convert.ToInt64(checkIntervalId) }, trans))
                    return new JsonReturn { ReturnCode = "0", ReturnMsg = Lang.csfs_cancelticket + Lang.csfs_fail };

            }
            //* 刪除發文
            if (!string.IsNullOrEmpty(sendId) && Convert.ToInt32(sendId) > 0)
            {
                HistoryCaseSendSettingBIZ sendBiz = new HistoryCaseSendSettingBIZ();
                sendBiz.DeleteCaseSendSetting(Convert.ToInt32(sendId), trans);
                sendBiz.DeleteCaseSendSettingDetails(Convert.ToInt32(sendId), trans);
            }

            //* 更新受款人
            CasePayeeSettingBIZ payeeBiz = new CasePayeeSettingBIZ();
            payeeBiz.UpdateCasePayeeSettingCheckNo(checkIntervalId, checkNo, null, null, trans);

            return new JsonReturn { ReturnCode = "1" };
        }
        /// <summary>
        /// 作廢 支票
        /// </summary>
        /// <param name="checkIntervalId"></param>
        /// <param name="checkNo"></param>
        /// <param name="sendId"></param>
        /// <param name="trans"></param>
        /// <returns></returns>
        public JsonReturn DisableCheck(string checkIntervalId, string checkNo, string sendId, IDbTransaction trans = null)
        {
            //* 撤銷號碼存檔
            if (!string.IsNullOrEmpty(checkIntervalId) && !string.IsNullOrEmpty(checkNo))
            {
                if (!Setting(new HistoryCheckNoSetting { Kind = "作廢", IsUsed = 1, CheckNo = Convert.ToInt64(checkNo), CheckIntervalID = Convert.ToInt64(checkIntervalId) }, trans))
                    return new JsonReturn { ReturnCode = "0", ReturnMsg = Lang.csfs_cancelticket + Lang.csfs_fail };

            }
            ////* 刪除發文
            if (!string.IsNullOrEmpty(sendId) && Convert.ToInt32(sendId) > 0)
            {
                CaseSendSettingBIZ sendBiz = new CaseSendSettingBIZ();
                sendBiz.DeleteCaseSendSetting(Convert.ToInt32(sendId), trans);
                sendBiz.DeleteCaseSendSettingDetails(Convert.ToInt32(sendId), trans);
            }

            ////* 更新受款人
            //CasePayeeSettingBIZ payeeBiz = new CasePayeeSettingBIZ();
            //payeeBiz.UpdateCasePayeeSettingCheckNo(checkIntervalId, checkNo, null, null, trans);

            return new JsonReturn { ReturnCode = "1" };
        }


        public JsonReturn OtherCheck(int payeeId)
        {
            HistoryCasePayeeSetting old = new HistoryCasePayeeSettingBIZ().Select(payeeId);
            if (old == null)
                return new JsonReturn { ReturnCode = "0", ReturnMsg = Lang.csfs_fail };
            IDbConnection dbConnection = OpenConnection();
            IDbTransaction dbTransaction = null;
            try
            {
                using (dbConnection)
                {
                    dbTransaction = dbConnection.BeginTransaction();
                    //* 取號
                    HistoryCheckUse check = GetPreserveCheck(dbTransaction);
                    //* 沒有新支票號了
                    if (check == null)
                    {
                        dbTransaction.Rollback();
                        return new JsonReturn { ReturnCode = "0", ReturnMsg = Lang.csfs_checkno_max1 };
                    }

                    //* 1.標記新號碼已用
                    JsonReturn rtn = UseCheck(ref check, dbTransaction);//*更改狀態
                    if (rtn.ReturnCode != "1")
                    {
                        dbTransaction.Rollback();
                        return rtn;
                    }

                    //* 2.作廢舊支票
                    rtn = DisableCheck(old.CheckIntervalId, old.CheckNo, old.SendId, dbTransaction);
                    if (rtn.ReturnCode != "1")
                    {
                        dbTransaction.Rollback();
                        return rtn;
                    }

                    //* 3.受款人存檔支票
                    HistoryCasePayeeSettingBIZ payeeBiz = new HistoryCasePayeeSettingBIZ();
                    if (payeeBiz.UpdateCasePayeeSettingCheckNo(payeeId, check.CheckIntervalId.ToString(), check.CheckNo.ToString(), dbTransaction))
                    {
                        //dbTransaction.Rollback();
                        //return new JsonReturn { ReturnCode = "0", ReturnMsg = Lang.csfs_save_fail };
                    }

                    //* 4.重新發文
                    //* 新增發文
                    old.CheckIntervalId = check.CheckIntervalId.ToString();
                    old.CheckNo = check.CheckNo.ToString();
                    string errMsg;
                    bool flag = true;
                    HistoryCaseSendSettingBIZ sendBiz = new HistoryCaseSendSettingBIZ();
                    //* 取得初始的發文資訊資料
                    HistoryCaseSendSettingCreateViewModel caseSendModel = sendBiz.GetDefaultSendSetting(old, out errMsg, dbTransaction);
                    if (!string.IsNullOrEmpty(errMsg) || caseSendModel == null)
                    {
                        //* 這裡面能出錯.也就發票號碼到最大或者沒設定了
                        dbTransaction.Rollback();
                        return new JsonReturn { ReturnCode = "0", ReturnMsg = errMsg };
                    }
                    //* 20150518 儲存時同時存發文設定
                    flag = sendBiz.SaveCreate(caseSendModel, dbTransaction);
                    //* 回填SendId
                    new CasePayeeSettingBIZ().UpdateCasePayeeSettingSendNo(old.CheckIntervalId, old.CheckNo, caseSendModel.SerialId, dbTransaction);
                    if (!flag)
                    {
                        dbTransaction.Rollback();
                        return new JsonReturn { ReturnCode = "0", ReturnMsg = Lang.csfs_save_fail };
                    }

                    dbTransaction.Commit();
                    return new JsonReturn { ReturnCode = "1" };
                }
            }
            catch (Exception ex)
            {
                try
                {
                    if (dbTransaction != null) dbTransaction.Rollback();
                }
                catch (Exception ex2)
                {
                }
                throw ex;
            }

        }
        /// <summary>
        /// Settings for use check.
        /// </summary>
        /// <param name="cns">The CNS.</param>
        /// <param name="trans">The trans.</param>
        /// <returns></returns>
        public bool SettingForUseCheck(HistoryCheckNoSetting cns, IDbTransaction trans = null)
        {
            string sqlStr = @"UPDATE History_CheckUse SET Kind=@Kind,IsUsed=@IsUsed WHERE CheckNo=@CheckNo AND CheckIntervalID = @CheckIntervalID and IsUsed=0";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("CheckNo", cns.CheckNo));
            Parameter.Add(new CommandParameter("CheckIntervalID", cns.CheckIntervalID));
            Parameter.Add(new CommandParameter("Kind", cns.Kind));
            Parameter.Add(new CommandParameter("IsUsed", cns.IsUsed));
            return trans == null ? ExecuteNonQuery(sqlStr) > 0 : ExecuteNonQuery(sqlStr, trans) > 0;
        }

        /// <summary>
        /// 將支票CheckUse狀態Update
        /// </summary>
        /// <param name="cns"></param>
        /// <param name="trans"></param>
        /// <returns></returns>
        public bool Setting(HistoryCheckNoSetting cns, IDbTransaction trans = null)
        {
            string sqlStr = @"UPDATE History_CheckUse SET Kind=@Kind,IsUsed=@IsUsed WHERE CheckNo=@CheckNo AND CheckIntervalID = @CheckIntervalID";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("CheckNo", cns.CheckNo));
            Parameter.Add(new CommandParameter("CheckIntervalID", cns.CheckIntervalID));
            Parameter.Add(new CommandParameter("Kind", cns.Kind));
            Parameter.Add(new CommandParameter("IsUsed", cns.IsUsed));
            return trans == null ? ExecuteNonQuery(sqlStr) > 0 : ExecuteNonQuery(sqlStr, trans) > 0;
        }


        /// <summary>
        /// 將支票號碼最小的Num筆設置為預留保護狀態
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public JsonReturn PreservCheck(int num)
        {
            //* 大於999 小於等於0不行
            if (num <= 0 || num > 999)
                return new JsonReturn { ReturnCode = "0", ReturnMsg = string.Format(Lang.csfs_plzInputNumberBetween0And1, "1", "999") };
            //* 已有保留不行
            int nowPreservNum = NoUsePreservCheckNoNum();
            if (nowPreservNum > 0)
            {
                //* 已預留{0}張,請執行還原功能或用完,才能繼續預留
                return new JsonReturn { ReturnCode = "0", ReturnMsg = string.Format(Lang.csfs_AlreadyPreservForbidden_0, nowPreservNum) };
            }
            //* 可用號碼不足.不行
            int nowCanUse = NoUseCheckNoNumber();
            if (num > nowCanUse)
                return new JsonReturn { ReturnCode = "0", ReturnMsg = Lang.csfs_DontHaveEnoughCheck };

            //* 開始更新保留
            return UpdatePreserv(num)
                ? new JsonReturn { ReturnCode = "1", ReturnMsg = Lang.csfs_save_ok }
                : new JsonReturn { ReturnCode = "0", ReturnMsg = Lang.csfs_save_fail };

        }

        /// <summary>
        /// 預留支票歸還
        /// </summary>
        /// <returns></returns>
        public JsonReturn ReturnPreservCheck()
        {
            if (NoUsePreservCheckNoNum() <= 0)
                return new JsonReturn { ReturnCode = "0", ReturnMsg = Lang.csfs_noPreservCheckNoUse };
            return UpdatePreservReturn()
                ? new JsonReturn { ReturnCode = "1", ReturnMsg = Lang.csfs_save_ok }
                : new JsonReturn { ReturnCode = "0", ReturnMsg = Lang.csfs_save_fail };
        }

        /// <summary>
        /// 取得目前可用支票張數
        /// </summary>
        /// <returns></returns>
        public int NoUseCheckNoNumber()
        {
            string strSql = "SELECT ISNULL(COUNT(1),0) FROM [History_CheckUse] WHERE IsUsed = 0 AND IsPreserve = 0 AND Kind = '支付'";
            Parameter.Clear();
            int? tmp = ExecuteScalar(strSql) as int?;
            return tmp ?? 0;
        }





        #region private

        /// <summary>
        /// 更新最小的Num筆為保護狀態
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        private bool UpdatePreserv(int num)
        {
            string strSql = @"UPDATE [History_CheckUse] SET IsPreserve = 1 
                                WHERE [CheckNo] IN 
                                (
	                                SELECT TOP " + num + @" [CheckNo] FROM [History_CheckUse]
	                                WHERE IsUsed = 0 AND IsPreserve = 0 AND Kind = '支付'
	                                ORDER BY CheckNo ASC 
                                )";
            Parameter.Clear();
            return ExecuteNonQuery(strSql) == num;
        }

        /// <summary>
        /// 將預留的保護狀態支票釋放
        /// </summary>
        /// <returns></returns>
        private bool UpdatePreservReturn()
        {
            string strSql = @"UPDATE [History_CheckUse] SET IsPreserve = 0 
                            WHERE IsUsed = 0 AND IsPreserve = 1 AND Kind = '支付'";
            Parameter.Clear();
            return ExecuteNonQuery(strSql) > 0;
        }

        /// <summary>
        /// 未使用的預留支票張數
        /// </summary>
        /// <returns></returns>
        private int NoUsePreservCheckNoNum()
        {
            string strSql =
                @"SELECT ISNULL(COUNT(1),0) FROM [History_CheckUse] WHERE IsUsed = 0 AND IsPreserve = 1 AND Kind = '支付'";
            Parameter.Clear();
            int? tmp = ExecuteScalar(strSql) as int?;
            return tmp ?? 0;
        }

        #endregion

    }
}
