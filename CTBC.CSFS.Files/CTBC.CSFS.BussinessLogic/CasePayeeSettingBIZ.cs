using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CTBC.CSFS.Models;
using CTBC.CSFS.Pattern;
using CTBC.CSFS.Resource;
using CTBC.CSFS.ViewModels;

namespace CTBC.CSFS.BussinessLogic
{
    public class CasePayeeSettingBIZ : CommonBIZ
    {
        public CasePayeeSettingBIZ(AppController appController)
            : base(appController)
        { }

        public CasePayeeSettingBIZ()
        { }

        public List<CasePayeeSetting> GetQueryList(CasePayeeSetting cps, IDbTransaction trans = null)
        {
            base.Parameter.Clear();
            base.Parameter.Add(new CommandParameter("@CaseId", cps.CaseId));

            string strSql = @"select CaseId,PayeeId,ReceivePerson,Receiver,Money,Fee,CheckNo,Bank,
                                              Memo,SendId,(select CaseKind2 from CaseMaster A where A.CaseId=B.CaseId ) as CaseKind 
                                             from CasePayeeSetting as B where CaseId=@CaseId ";
            List<CasePayeeSetting> list = trans == null ? SearchList<CasePayeeSetting>(strSql).ToList() : SearchList<CasePayeeSetting>(strSql, trans).ToList();
            if (list != null && list.Any())
            {
                string sql = "SELECT BranchNo,PayCaseId FROM CaseSeizure WHERE PayCaseId =@CaseId ORDER BY BRANCHNO ASC";
                List<CaseSeizure> listBranchNo = base.SearchList<CaseSeizure>(sql).ToList();
                if (listBranchNo != null && listBranchNo.Any())
                {
                    string strBankId = string.Empty;
                    foreach (CaseSeizure items in listBranchNo)
                    {
                        strBankId += items.BranchNo + "；";
                    }
                    strBankId = strBankId.TrimEnd('；');
                    foreach (CasePayeeSetting item in list)
                    {
                        item.BankID = strBankId;
                    }
                    //foreach (CasePayeeSetting item in list)
                    //{
                    //    string strBankId = string.Empty;
                    //    foreach (CaseSeizure items in listBranchNo.Where(m => m.PayCaseId == item.CaseId))
                    //    {
                    //        strBankId += items.BranchNo + "；";
                    //    }
                    //    strBankId = strBankId.TrimEnd('；');
                    //    item.BankID = strBankId;
                    //}
                }
            }
            return list;
        }

        /// <summary>
        /// 新增受款人
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public JsonReturn Create(CasePayeeSetting model)
        {
            CaseMaster master = new CaseMasterBIZ().MasterModel(model.CaseId);
            try
            {
                if (master != null && master.PayDate != null && master.PayDate != "")
                {
                    model.PayDate = Convert.ToDateTime(master.PayDate);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            CheckNoSettingBIZ checkBiz = new CheckNoSettingBIZ();
            CaseSendSettingBIZ sendBiz = new CaseSendSettingBIZ();
            IDbConnection dbConnection = OpenConnection();
            IDbTransaction trans = null;
            try
            {
                using (dbConnection)
                {
                    trans = dbConnection.BeginTransaction();
                    JsonReturn rtnObj;
                    switch (model.PayeeAction)
                    {
                        case 1:
                            //* 正常存檔
                            model.CheckNo = null;
                            model.CheckNo = null;
                            break;
                        case 2:
                            //* 取號存檔
                            //* 取號如果之前有號(雖然在UI上攔截了但是後臺還是要預防萬一)
                            if (!string.IsNullOrEmpty(model.CheckNo) || !string.IsNullOrEmpty(model.CheckIntervalId))
                                return new JsonReturn { ReturnCode = "0", ReturnMsg = Lang.csfs_CheckNoExist };

                            //* 取號
                            CheckUse check = null;
                            rtnObj = checkBiz.UseCheck(ref check, trans);
                            if (rtnObj.ReturnCode != "1") { trans.Rollback(); return rtnObj; }

                            model.CheckNo = check.CheckNo.ToString();
                            model.CheckIntervalId = check.CheckIntervalId.ToString();
                            break;
                        case 3:
                            //* 撤銷號碼存檔(新增時理論上不用
                            rtnObj = checkBiz.CancelCheck(model.CheckIntervalId, model.CheckNo, model.SendId, trans);
                            if (rtnObj.ReturnCode != "1")
                            { trans.Rollback(); return rtnObj; }
                            model.CheckNo = null;
                            model.CheckNo = null;
                            break;
                    }

                    //* 新增受款人
                    bool rtn = InsertCasePayeeSetting(model, trans);
                    if (!rtn)
                    {
                        trans.Rollback();
                        return new JsonReturn { ReturnCode = "0", ReturnMsg = Lang.csfs_receive_set + Lang.csfs_fail };
                    }

                    if (model.PayeeAction == 2)
                    {
                        //* 新增發文
                        string errMsg;
                        //* 取得初始的發文資訊資料
                        CaseSendSettingCreateViewModel caseSendModel = sendBiz.GetDefaultSendSetting(model, out errMsg, trans);
                        if (!string.IsNullOrEmpty(errMsg) || caseSendModel == null)
                        {
                            //* 這裡面能出錯.也就發票號碼到最大或者沒設定了
                            trans.Rollback();
                            return new JsonReturn { ReturnCode = "0", ReturnMsg = errMsg };
                        }
                        caseSendModel.SendDate = model.PayDate;
                        //* 20150518 儲存時同時存發文設定
                        rtn = rtn & sendBiz.SaveCreate(caseSendModel, trans);
                        //* 回填SendId
                        rtn = rtn & UpdateCasePayeeSettingSendNo(model.CheckIntervalId, model.CheckNo, caseSendModel.SerialId, trans);
                    }


                    if (rtn)
                    {
                        trans.Commit();
                        return new JsonReturn { ReturnCode = "1" };
                    }
                    trans.Rollback();
                    return new JsonReturn { ReturnCode = "0", ReturnMsg = Lang.csfs_add_fail };
                }

            }
            catch (Exception ex)
            {
                try
                {
                    if (trans != null) trans.Rollback();
                }
                catch (Exception ex2)
                {
                }
                throw ex;
            }
        }




        /// <summary>
        /// 實際新增 受款人 表
        /// </summary>
        /// <param name="model"></param>
        /// <param name="trans"></param>
        /// <returns></returns>
        private bool InsertCasePayeeSetting(CasePayeeSetting model, IDbTransaction trans = null)
        {
            string sql = @"INSERT INTO CasePayeeSetting
                            (CaseId,ReceivePerson,Receiver,Address,Currency,CCReceiver,Money,Fee,CheckNo,CaseKind,Bank,Memo,CheckDate,CheckIntervalID) 
                            VALUES
                            (@CaseId,@ReceivePerson,@Receiver,@Address,@Currency,@CCReceiver,@Money,@Fee,@CheckNo,@CaseKind,@Bank,@Memo,GETDATE(),@CheckIntervalID);";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("@CaseId", model.CaseId));
            Parameter.Add(new CommandParameter("@ReceivePerson", model.ReceivePerson));
            Parameter.Add(new CommandParameter("@Receiver", model.Receiver));
            Parameter.Add(new CommandParameter("@Address", model.Address));
            Parameter.Add(new CommandParameter("@Currency", model.Currency));
            Parameter.Add(new CommandParameter("@CCReceiver", model.CCReceiver));
            Parameter.Add(new CommandParameter("@Money", model.Money ?? "0"));
            Parameter.Add(new CommandParameter("@Fee", model.Fee ?? "0"));
            Parameter.Add(new CommandParameter("@CheckNo", string.IsNullOrEmpty(model.CheckNo) ? null : model.CheckNo));
            Parameter.Add(new CommandParameter("@CaseKind", model.CaseKind));
            Parameter.Add(new CommandParameter("@Bank", model.Bank));
            Parameter.Add(new CommandParameter("@Memo", model.Memo));
            Parameter.Add(new CommandParameter("@CheckIntervalID", model.CheckIntervalId));
            return trans == null ? ExecuteNonQuery(sql) > 0 : ExecuteNonQuery(sql, trans) > 0;
        }

        /// <summary>
        /// 更新 受款人 的發文ID
        /// </summary>
        /// <param name="checkIntervalId"></param>
        /// <param name="checkNo"></param>
        /// <param name="sendId"></param>
        /// <param name="trans"></param>
        /// <returns></returns>
        public bool UpdateCasePayeeSettingSendNo(string checkIntervalId, string checkNo, int sendId, IDbTransaction trans = null)
        {
            //* 這裡理論上要用PayeeId但是取出來太麻煩哦
            string sql = @"UPDATE [CasePayeeSetting] SET [SendId] = @SendId WHERE [CheckIntervalID] = @CheckIntervalID AND [CheckNo] = @CheckNo";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("SendId", sendId));
            Parameter.Add(new CommandParameter("CheckIntervalID", checkIntervalId));
            Parameter.Add(new CommandParameter("CheckNo", checkNo));
            return trans == null ? ExecuteNonQuery(sql) > 0 : ExecuteNonQuery(sql, trans) > 0;
        }

        /// <summary>
        /// 更新 受款人 的支票號碼
        /// </summary>
        /// <param name="oldCheckIntervalId"></param>
        /// <param name="oldCheckNo"></param>
        /// <param name="newCheckIntervalId"></param>
        /// <param name="newCheckNo"></param>
        /// <param name="trans"></param>
        /// <returns></returns>
        public bool UpdateCasePayeeSettingCheckNo(string oldCheckIntervalId, string oldCheckNo, string newCheckIntervalId, string newCheckNo, IDbTransaction trans = null)
        {
            string sql = @"UPDATE [CasePayeeSetting] 
                                SET [CheckNo] = @NewCheckNo,
                                    [CheckIntervalID] = @NewCheckIntervalID 
                           WHERE [CheckIntervalID] = @OldCheckIntervalId AND [CheckNo] = @OldCheckNo";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("NewCheckIntervalID", string.IsNullOrEmpty(newCheckIntervalId) ? null : newCheckIntervalId));
            Parameter.Add(new CommandParameter("NewCheckNo", string.IsNullOrEmpty(newCheckNo) ? null : newCheckNo));
            Parameter.Add(new CommandParameter("OldCheckIntervalId", oldCheckIntervalId));
            Parameter.Add(new CommandParameter("OldCheckNo", oldCheckNo));
            return trans == null ? ExecuteNonQuery(sql) > 0 : ExecuteNonQuery(sql, trans) > 0;
        }
        public bool UpdateCasePayeeSettingCheckNo(int payeeId, string newCheckIntervalId, string newCheckNo, IDbTransaction trans = null)
        {
            string sql = @"UPDATE [CasePayeeSetting] 
                                SET [CheckNo] = @NewCheckNo,
                                    [CheckIntervalID] = @NewCheckIntervalID 
                           WHERE [PayeeId] = @payeeId ";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("NewCheckIntervalID", string.IsNullOrEmpty(newCheckIntervalId) ? null : newCheckIntervalId));
            Parameter.Add(new CommandParameter("NewCheckNo", string.IsNullOrEmpty(newCheckNo) ? null : newCheckNo));
            Parameter.Add(new CommandParameter("payeeId", payeeId));
            return trans == null ? ExecuteNonQuery(sql) > 0 : ExecuteNonQuery(sql, trans) > 0;
        }

        /// <summary>
        /// 編輯受款人
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public JsonReturn Edit(CasePayeeSetting model)
        {
            CaseMasterBIZ masterBiz = new CaseMasterBIZ();
            CaseMaster master = masterBiz.MasterModel(model.CaseId);
            if (master != null && master.PayDate != null)
            {
                model.PayDate = Convert.ToDateTime(master.PayDate);
            }
            JsonReturn rtnObj;
            CheckNoSettingBIZ checkBiz = new CheckNoSettingBIZ();
            CaseSendSettingBIZ sendBiz = new CaseSendSettingBIZ();
            CasePayeeSetting old = Select(model.PayeeId);
            model.CaseId = old.CaseId;
            bool rtn;
            IDbConnection dbConnection = base.OpenConnection();
            IDbTransaction trans = null;
            try
            {
                using (dbConnection)
                {
                    trans = dbConnection.BeginTransaction();
                    if (model.PayeeAction == 2)
                    {
                        //* 取號存檔
                        //* 取號如果之前有號(雖然在UI上攔截了但是後臺還是要預防萬一)
                        if (!string.IsNullOrEmpty(old.CheckNo) || !string.IsNullOrEmpty(old.CheckIntervalId))
                            return new JsonReturn { ReturnCode = "0", ReturnMsg = Lang.csfs_CheckNoExist };

                        //* 取號
                        CheckUse check = null;
                        rtnObj = checkBiz.UseCheck(ref check, trans);
                        if (rtnObj.ReturnCode != "1")
                        { trans.Rollback(); return rtnObj; }

                        model.CheckNo = check.CheckNo.ToString();
                        model.CheckIntervalId = check.CheckIntervalId.ToString();
                    }
                    if (model.PayeeAction == 3)
                    {
                        //* 撤銷號碼存檔(新增時理論上不用
                        rtnObj = checkBiz.CancelCheck(old.CheckIntervalId, old.CheckNo, old.SendId, trans);
                        if (rtnObj.ReturnCode != "1")
                        { trans.Rollback(); return rtnObj; }
                        model.CheckNo = null;
                        model.CheckIntervalId = null;
                    }

                    //* 更新受款人
                    rtn = UpdateCasePayeeSetting(model, trans);
                    if (!rtn)
                    {
                        trans.Rollback();
                        return new JsonReturn { ReturnCode = "0", ReturnMsg = Lang.csfs_receive_set + Lang.csfs_fail };
                    }

                    if (model.PayeeAction == 2)
                    {
                        //* 新增發文
                        string errMsg;
                        //* 取得初始的發文資訊資料
                        CaseSendSettingCreateViewModel caseSendModel = sendBiz.GetDefaultSendSetting(model, out errMsg, trans);
                        if (!string.IsNullOrEmpty(errMsg) || caseSendModel == null)
                        {
                            //* 這裡面能出錯.也就發票號碼到最大或者沒設定了
                            trans.Rollback();
                            return new JsonReturn { ReturnCode = "0", ReturnMsg = errMsg };
                        }
                        //* 20150518 儲存時同時存發文設定
                        rtn = rtn & sendBiz.SaveCreate(caseSendModel, trans);
                        //* 回填SendId
                        rtn = rtn & UpdateCasePayeeSettingSendNo(model.CheckIntervalId, model.CheckNo, caseSendModel.SerialId, trans);
                    }
                    if (rtn)
                    {
                        trans.Commit();
                        return new JsonReturn { ReturnCode = "1" };
                    }
                    trans.Rollback();
                    return new JsonReturn { ReturnCode = "0", ReturnMsg = Lang.csfs_save_fail };
                }
            }
            catch (Exception ex)
            {
                try
                {
                    if (trans != null)
                        trans.Rollback();
                }
                catch (Exception ex2)
                {
                }
                throw ex;
            }
        }

        /// <summary>
        /// 修改CasePayeeSetting表
        /// </summary>
        /// <param name="model"></param>
        /// <param name="trans"></param>
        /// <returns></returns>
        public bool UpdateCasePayeeSetting(CasePayeeSetting model, IDbTransaction trans = null)
        {
            string strSql = @"update CasePayeeSetting set 
                                            ReceivePerson=@ReceivePerson,
                                            Receiver=@Receiver,
                                            Address=@Address,
                                            Currency=@Currency,
                                            CCReceiver=@CCReceiver,
                                            Money=@Money,
                                            Fee=@Fee,
                                            Bank=@Bank,
                                            Memo=@Memo,
                                            CheckNo = @CheckNo,
                                            CheckDate = GETDATE(),
                                            CheckIntervalID = @CheckIntervalID
                                    where PayeeId=@PayeeId";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("@PayeeId", model.PayeeId));
            Parameter.Add(new CommandParameter("@ReceivePerson", model.ReceivePerson));
            Parameter.Add(new CommandParameter("@Receiver", model.Receiver));
            Parameter.Add(new CommandParameter("@Address", model.Address));
            Parameter.Add(new CommandParameter("@Currency", model.Currency));
            Parameter.Add(new CommandParameter("@CCReceiver", model.CCReceiver));
            Parameter.Add(new CommandParameter("@Money", model.Money ?? "0"));
            Parameter.Add(new CommandParameter("@Fee", model.Fee ?? "0"));
            Parameter.Add(new CommandParameter("@Bank", model.Bank));
            Parameter.Add(new CommandParameter("@Memo", model.Memo));
            Parameter.Add(new CommandParameter("@CheckNo", string.IsNullOrEmpty(model.CheckNo) ? null : model.CheckNo));
            Parameter.Add(new CommandParameter("@CheckIntervalID", model.CheckIntervalId));
            return trans == null ? ExecuteNonQuery(strSql) > 0 : ExecuteNonQuery(strSql, trans) > 0;
        }

        /// <summary>
        /// 刪除受款人
        /// </summary>
        /// <param name="payeeId"></param>
        /// <returns></returns>
        public JsonReturn Delete(int payeeId)
        {
            CasePayeeSetting old = Select(payeeId);
            IDbConnection connection = OpenConnection();
            IDbTransaction transaction = null;
            try
            {
                using (connection)
                {
                    transaction = connection.BeginTransaction();
                    //* 撤銷支票號碼
                    JsonReturn rtn = new CheckNoSettingBIZ().CancelCheck(old.CheckIntervalId, old.CheckNo, old.SendId, transaction);
                    if (rtn.ReturnCode != "1") { transaction.Rollback(); return rtn; }

                    if (DeleteCasePayeeSetting(payeeId, transaction))
                    {
                        transaction.Commit();
                        return new JsonReturn { ReturnCode = "1" };
                    }
                    transaction.Rollback();
                    return new JsonReturn { ReturnCode = "0", ReturnMsg = Lang.csfs_del_fail };
                }
            }
            catch (Exception ex)
            {
                if (transaction != null) transaction.Rollback();
                return new JsonReturn { ReturnCode = "0", ReturnMsg = Lang.csfs_del_fail };
            }
        }

        public string Address(string GovName)
        {
            try
            {
                string StrSql = @"select GovAddr from GovAddress where GovName=@GovName";
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@GovName", GovName));
                return base.ExecuteScalar(StrSql) == null ? "" : base.ExecuteScalar(StrSql).ToString();
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        /// <summary>
        /// 實際 刪除 受款人
        /// </summary>
        /// <param name="payeeId"></param>
        /// <param name="trans"></param>
        /// <returns></returns>
        private bool DeleteCasePayeeSetting(int payeeId, IDbTransaction trans = null)
        {
            string strSql = @"DELETE from CasePayeeSetting where PayeeId=@PayeeId";
            base.Parameter.Clear();
            base.Parameter.Add(new CommandParameter("@PayeeId", payeeId));
            return trans == null ? base.ExecuteNonQuery(strSql) > 0 : base.ExecuteNonQuery(strSql, trans) > 0;
        }

        /// <summary>
        /// 刪除CaseId 下所有的受款人
        /// </summary>
        /// <param name="caseId"></param>
        /// <param name="dbtrans"></param>
        /// <returns></returns>
        public bool DeleteCasePayeeSetting(Guid caseId, IDbTransaction dbtrans = null)
        {
            string strSql = "DELETE CasePayeeSetting WHERE  CaseId = @CaseId ";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("CaseId", caseId));
            return dbtrans == null ? ExecuteNonQuery(strSql) > 0 : ExecuteNonQuery(strSql, dbtrans) > 0;
        }


        #region 頁面傳值
        /// <summary>
        /// 取得某一指定的受款人
        /// </summary>
        /// <param name="payeeId"></param>
        /// <returns></returns>
        public CasePayeeSetting Select(int payeeId)
        {
            try
            {
//                string sqlStr = @"select B.CaseId,ReceivePerson,Receiver,Money,Fee,CheckNo,SendId,CheckIntervalID,
//		                       (select distinct [dbo].[CaseSeizureBranchName](b.CaseId) from CaseSeizure) as BankID,Bank,Memo,Address,Currency,CCReceiver,
//		                       (select CaseKind2 from CaseMaster A where A.CaseId=B.CaseId ) as CaseKind from CasePayeeSetting as B where PayeeId=@PayeeId";

                string sqlStr = @"SELECT B.[CaseId],[ReceivePerson],B.[Receiver],[Money],[Fee],[CheckNo],[SendId],[CheckIntervalID],
                    [Bank],[Memo],[Address],[Currency],[CCReceiver], A.[CaseKind2]
                    FROM [CasePayeeSetting] AS B
                    INNER JOIN [CaseMaster] AS A ON B.[CaseId] = A.[CaseId] WHERE PayeeId=@PayeeId";
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@PayeeId", payeeId));

                IList<CasePayeeSetting> list = base.SearchList<CasePayeeSetting>(sqlStr);
                return list != null && list.Any() ? list.FirstOrDefault() : new CasePayeeSetting();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string CaseKind(Guid caseId)
        {
            try
            {
                string sqlStr = @"select CaseKind2 from CaseMaster where CaseId=@CaseId";

                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@CaseId", caseId));

                string result = base.ExecuteScalar(sqlStr).ToString();
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string BankID(Guid caseId)
        {
            try
            {
                //string sqlStr = @"select distinct [dbo].[CaseSeizureBranchNo](@CaseId) from CaseSeizure";

                string sqlStr = @"SELECT BranchNo FROM CaseSeizure WHERE PayCaseId=@CaseId";
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@CaseId", caseId));
                string result = string.Empty;
                List<CaseSeizure> list = base.SearchList<CaseSeizure>(sqlStr).ToList();
                if (list != null && list.Any())
                {
                    foreach (CaseSeizure item in list)
                    {
                        result += item.BranchNo + "；";
                    }
                    result = result.TrimEnd('；');
                }

                //string result = Convert.ToString(base.ExecuteScalar(sqlStr));
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        //public string Bank(Guid caseId)
        //{
        //    try
        //    {
        //        string sqlStr = @"select distinct [dbo].[CaseSeizureBranchName](@CaseId) from CaseSeizure";

        //        base.Parameter.Clear();
        //        base.Parameter.Add(new CommandParameter("@CaseId", caseId));

        //        string result = Convert.ToString(base.ExecuteScalar(sqlStr));
        //        return result;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}

        public string BankForPay(Guid caseId)
        {
            try
            {
                //string sqlStr = @"select distinct [dbo].[CaseSeizureBranchNameForPay](@CaseId) from CaseSeizure";

                string sqlStr = @"select distinct  BranchName from CaseSeizure where PayCaseId=@CaseId";
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@CaseId", caseId));
                string result = string.Empty;
                List<CaseSeizure> list = base.SearchList<CaseSeizure>(sqlStr).ToList();
                if (list != null && list.Any())
                {
                    foreach (var item in list)
                    {
                        result += item.BranchName + "；";
                    }
                    result = result.TrimEnd('；');
                }

                //string result = Convert.ToString(base.ExecuteScalar(sqlStr));
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string PayAmountSum(Guid caseId)
        {
            try
            {
                string sqlStr = @"select ISNULL(Sum(PayAmount),0) from CaseSeizure where PayCaseId=@CaseId";

                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@CaseId", caseId));

                string result = string.Format("{0:N0}", Convert.ToDecimal(base.ExecuteScalar(sqlStr)));
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public string MoneySum(Guid caseId, int payeeId = 0)
        {
            try
            {
                string sqlStr = @"select ISNULL(Sum(Money + Fee),0) from CasePayeeSetting where CaseId=@CaseId AND [PayeeId] <> @PayeeId";

                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@CaseId", caseId));
                base.Parameter.Add(new CommandParameter("@PayeeId", payeeId));

                string result = string.Format("{0:N0}", Convert.ToDecimal(base.ExecuteScalar(sqlStr)));
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion


        /// <summary>
        /// 帶出還沒發文的支票的受款人信息
        /// </summary>
        /// <param name="caseId"></param>
        /// <returns></returns>
        public IList<CasePayeeSetting> GetPayeeSettingWhichNotSendSetting(Guid caseId)
        {
            string sql = @"SELECT 
	            CPS.[PayeeId],
	            CPS.[CaseId],
	            CPS.[ReceivePerson],
	            CPS.[Address],
	            CPS.[Receiver],
	            CPS.[CCReceiver],
	            CPS.[CheckNo],
	            CPS.[CaseKind],
	            CPS.[BankID],
	            CPS.[Bank],
	            CPS.[Currency],
	            CPS.[Money],
	            CPS.[Fee],
	            CPS.[Memo],
	            CPS.[CheckDate] 
            FROM [CasePayeeSetting] AS CPS
            WHERE CPS.CaseId = @CaseId AND ISNULL(CPS.SendId,'') = ''";
            base.Parameter.Clear();
            base.Parameter.Add(new CommandParameter("CaseId", caseId));
            return SearchList<CasePayeeSetting>(sql);
        }


    }
}
