using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Transactions;
using System.Text;
using System.Threading.Tasks;
using CTBC.CSFS.Models;
using CTBC.CSFS.Pattern;
using CTBC.CSFS.ViewModels;
using CTBC.FrameWork.Util;

namespace CTBC.CSFS.BussinessLogic
{
    public class LendDataBIZ : CommonBIZ
    {
        public LendDataBIZ(AppController appController)
            : base(appController)
        { }

        public LendDataBIZ()
        { }

        #region 新增一筆正本調閱資料(資料與附件)
        /// <summary>
        /// 新增一筆正本調閱資料(資料與附件)
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool CreateLend(AgentOriginalInfoViewModel model)
        {
            CaseMasterBIZ caseBiz = new CaseMasterBIZ();
            IDbConnection dbConnection = OpenConnection();
            IDbTransaction dbTransaction = null;
            try
            {
                using (dbConnection)
                {
                    dbTransaction = dbConnection.BeginTransaction();
                    string lendid = "0";
                    #region 主表
                    LendDataBIZ lenddata = new LendDataBIZ();
                    CaseMaster casemodel = caseBiz.MasterModel(model.LendDataInfo.CaseId);
                    model.LendDataInfo.DocNo = casemodel.DocNo;
                    model.LendDataInfo.LendStatus = LendStatus.LendStatusLendSetting;
                    model.LendDataInfo.BankID = new PARMCodeBIZ().GetCodeNoByCodeDesc(model.LendDataInfo.BankID);
                    lenddata.Create(model.LendDataInfo, ref lendid, dbTransaction);
                    #endregion

                    #region 附件
                    LendAttachmentBIZ lendAttach = new LendAttachmentBIZ();
                    foreach (LendAttachment attach in model.LendAttachmentInfoList)
                    {
                        attach.CaseId = model.LendDataInfo.CaseId;
                        attach.LendId = Convert.ToInt32(lendid);
                        attach.CreatedUser = "";
                        lendAttach.Create(attach, dbTransaction);
                    }
                    #endregion

                    dbTransaction.Commit();
                }
                return true;
            }
            catch (Exception)
            {
                try
                {
                    if (dbTransaction != null) dbTransaction.Rollback();
                }
                catch (Exception)
                {
                    // ignored
                }
                return false;
            }
        }
        #endregion

        #region 新增一筆正本調閱
        /// <summary>
        /// 新增一筆正本調閱
        /// </summary>
        /// <param name="model">LendData</param>
        /// <param name="lendid">返回自增的id</param>
        /// <param name="trans">事務</param>
        /// <returns></returns>
        public int Create(LendData model, ref string lendid, IDbTransaction trans = null)
        {
            string sql = @"insert into LendData  (CaseId,DocNo,ClientID,Name,BankID,Bank,Account,Memo,Phone,LendStatus) 
                                        values (
                                        @CaseId,@DocNo,@ClientID,@Name,@BankID,@Bank,@Account,@Memo,@Phone,@LendStatus);
                                        select @@identity";

            Parameter.Clear();

            // 添加參數
            Parameter.Add(new CommandParameter("@CaseId", model.CaseId.ToString()));
            Parameter.Add(new CommandParameter("@DocNo", model.DocNo));
            Parameter.Add(new CommandParameter("@ClientID", model.ClientID));
            Parameter.Add(new CommandParameter("@Name", model.Name));
            Parameter.Add(new CommandParameter("@BankID", model.BankID));
            Parameter.Add(new CommandParameter("@Bank", model.Bank));
            Parameter.Add(new CommandParameter("@Account", model.Account));
            Parameter.Add(new CommandParameter("@Memo", model.Memo));
            Parameter.Add(new CommandParameter("@Phone", model.Phone));
            Parameter.Add(new CommandParameter("@LendStatus", LendStatus.LendStatusSetting));
            lendid = trans == null ? Convert.ToString(ExecuteScalar(sql)) : Convert.ToString(ExecuteScalar(sql, trans));
            return 1;
        }
        #endregion

        #region 正本調閱查詢結果集
        /// <summary>
        /// 得到正本調閱查詢結果
        /// </summary>
        /// <param name="model"></param>
        /// <param name="pageIndex"></param>
        /// <param name="strSortExpression"></param>
        /// <param name="strSortDirection"></param>
        /// <returns></returns>
        public IList<LendData> GetQueryList(AgentOriginalInfoViewModel model, int pageIndex, string strSortExpression, string strSortDirection)
        {
            try
            {
                base.PageIndex = pageIndex;
                string sqlStr = "";
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@pageS", (base.PageSize * (base.PageIndex - 1)) + 1));
                base.Parameter.Add(new CommandParameter("@pageE", base.PageSize * base.PageIndex));
                base.Parameter.Add(new CommandParameter("@CaseId", model.LendDataInfo.CaseId));
                base.Parameter.Add(new CommandParameter("@LendStatus", LendStatus.LendStatusSetting));
                /* 修改
                sqlStr = @" select ld.[LendID],ld.[CaseId],ld.[DocNo],ld.[ClientID],ld.[Name],
                                  ld.[BankID],ld.[Bank],ld.[Account],ld.[Memo],ld.[Phone]
                                 from LendData  ld where ld.LendStatus=@LendStatus and ld.CaseId=@CaseId ";
                */
                sqlStr = @" select ld.[LendID],ld.[CaseId],ld.[DocNo],ld.[ClientID],ld.[Name],
                                  ld.[BankID],ld.[Bank],ld.[Account],ld.[Memo],ld.[Phone]
                                 from LendData  ld where  ld.CaseId=@CaseId ";

                IList<LendData> _ilsit = base.SearchList<LendData>(sqlStr);

                if (_ilsit != null && _ilsit.Count > 0)
                {
                    string strID = string.Empty;
                    foreach (LendData item in _ilsit)
                    {
                        strID+="'"+item.CaseId+"',";
                    }
                    strID = strID.TrimEnd(',');
                    string strName = string.Empty;
                    string sql = "SELECT LendAttachName,CaseId FROM LendAttachment WHERE CaseId In (" + strID + ")";
                    List<LendAttachment> listAttach = base.SearchList<LendAttachment>(sql).ToList();
                    if (listAttach != null && listAttach.Any())
                    {
                        foreach (LendData item in _ilsit)
                        {
                            foreach (LendAttachment items in listAttach.Where(m=>m.CaseId==item.CaseId))
                            {
                                strName += items.LendAttachName + ",";
                            }
                            strName = strName.TrimEnd(',');
                            item.AttachNames = strName;
                        }
                    }
                    return _ilsit;
                }
                else
                {
                    return new List<LendData>();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region 查詢一筆正本調閱資料
        /// <summary>
        /// 得到一筆正本調閱資料
        /// </summary>
        /// <param name="lendId"></param>
        /// <returns></returns>
        public LendData getModel(string lendId)
        {
            string strSql = @"select ld.[LendID],ld.[CaseId],ld.[DocNo],ld.[ClientID],ld.[Name],ld.[BankID],ld.[Bank],ld.[Account],ld.[Memo],ld.[Phone]
                                        from LendData  ld where ld.LendID=@LendID";

            Parameter.Clear();

            // 添加參數
            Parameter.Add(new CommandParameter("@LendID", lendId));

            IList<LendData> _list = base.SearchList<LendData>(strSql);

            if (_list != null)
            {
                if (_list.Count > 0)
                {
                    return _list[0];
                }
                return new LendData();
            }
            return new LendData();
        }
        #endregion

        #region 刪除一筆正本調閱資料(資料與附件)
        /// <summary>
        /// 刪除一筆正本調閱資料
        /// </summary>
        /// <param name="lendId"></param>
        /// <returns></returns>
        public int DeleteLendData(string lendId)
        {
            IDbConnection dbConnection = OpenConnection();
            IDbTransaction dbTransaction = null;
            try
            {
                using (dbConnection)
                {
                    dbTransaction = dbConnection.BeginTransaction();
                    #region 主表
                    LendDataBIZ lenddata = new LendDataBIZ();
                    lenddata.DeleteLend(lendId, dbTransaction);
                    #endregion

                    #region 附件
                    LendAttachmentBIZ lendAttach = new LendAttachmentBIZ();
                    lendAttach.DeleteAttatch(lendId, dbTransaction);
                    #endregion

                    dbTransaction.Commit();
                }
                return 1;
            }
            catch (Exception)
            {
                try
                {
                    if (dbTransaction != null) dbTransaction.Rollback();
                }
                catch (Exception)
                {
                    // ignored
                }
                return 0;
            }
        }

        /// <summary>
        /// 刪除一筆附件
        /// </summary>
        /// <param name="attachid">附件id</param>
        /// <returns></returns>
        public int DeleteAttatch(string attachid)
        {
            string strSql = " delete from LendAttachment where LendAttachId=@LendAttachId ";
            base.Parameter.Clear();

            // 添加參數
            base.Parameter.Add(new CommandParameter("@LendAttachId", attachid));

            try
            {
                return base.ExecuteNonQuery(strSql);
            }
            catch (Exception ex)
            {
                // 拋出異常
                throw ex;
            }
        }
        #endregion

        #region 刪除一筆正本調閱
        /// <summary>
        /// 刪除一筆正本調閱
        /// </summary>
        /// <param name="lendId"></param>
        /// <returns></returns>
        public int DeleteLend(string lendId, IDbTransaction trans = null)
        {
            string strSql = @" delete from  LendData where LendID=@LendID";
            Parameter.Clear();
            // 添加參數
            Parameter.Add(new CommandParameter("@LendID", lendId));
            return trans == null ? ExecuteNonQuery(strSql) : ExecuteNonQuery(strSql, trans);
        }
        #endregion

        #region 修改一筆正本調閱(資料與附件)
        public bool UpdateLend(AgentOriginalInfoViewModel model)
        {
            IDbConnection dbConnection = OpenConnection();
            IDbTransaction dbTransaction = null;
            try
            {
                using (dbConnection)
                {
                    dbTransaction = dbConnection.BeginTransaction();
                    #region 主表
                    LendDataBIZ lenddata = new LendDataBIZ();
                    lenddata.Update(model.LendDataInfo, dbTransaction);
                    #endregion

                    #region 附件
                    LendAttachmentBIZ lendAttach = new LendAttachmentBIZ();
                    foreach (LendAttachment attach in model.LendAttachmentInfoList)
                    {
                        attach.CaseId = model.LendDataInfo.CaseId;
                        attach.LendId = model.LendDataInfo.LendID;
                        attach.CreatedUser = "";
                        lendAttach.Create(attach, dbTransaction);
                    }
                    #endregion

                    dbTransaction.Commit();
                }
                return true;
            }
            catch (Exception)
            {
                try
                {
                    if (dbTransaction != null) dbTransaction.Rollback();
                }
                catch (Exception)
                {
                    // ignored
                }
                return false;
            }
        }
        #endregion

        #region 修改一筆正本調閱資料
        /// <summary>
        /// 修改一筆正本調閱資料
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public int Update(LendData model, IDbTransaction trans = null)
        {
            string strSql = @" update LendData set ClientID=@ClientID,Name=@Name,BankID=@BankID,Bank=@Bank,
                                        Account=@Account,Memo=@Memo,Phone=@Phone
                                        where LendID=@LendID";

            Parameter.Clear();
            // 添加參數
            Parameter.Add(new CommandParameter("@ClientID", model.ClientID));
            Parameter.Add(new CommandParameter("@Name", model.Name));
            Parameter.Add(new CommandParameter("@BankID", model.BankID));
            Parameter.Add(new CommandParameter("@Bank", model.Bank));
            Parameter.Add(new CommandParameter("@Account", model.Account));
            Parameter.Add(new CommandParameter("@Memo", model.Memo));
            Parameter.Add(new CommandParameter("@Phone", model.Phone));
            Parameter.Add(new CommandParameter("@LendID", model.LendID));

            return trans == null ? base.ExecuteNonQuery(strSql) : base.ExecuteNonQuery(strSql, trans);
        }
        #endregion

        #region 查詢本案件所有已歸還的結果
        /// <summary>
        /// 查詢本案件所有已歸還的結果
        /// </summary>
        /// <param name="model"></param>
        /// <param name="pageIndex"></param>
        /// <param name="strSortExpression"></param>
        /// <param name="strSortDirection"></param>
        /// <returns></returns>
        public IList<LendData> GetLendCaseQueryList(AgentOriginalInfoViewModel model, int pageIndex, string strSortExpression, string strSortDirection)
        {
            try
            {
                base.PageIndex = pageIndex;
                string sqlStr = "";
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@pageS", (base.PageSize * (base.PageIndex - 1)) + 1));
                base.Parameter.Add(new CommandParameter("@pageE", base.PageSize * base.PageIndex));
                base.Parameter.Add(new CommandParameter("@CaseId", model.LendDataInfo.CaseId));
                base.Parameter.Add(new CommandParameter("@LendStatus", LendStatus.LendStatusLendSetting));

                sqlStr = @"select ld.[LendID],ld.[CaseId],ld.[DocNo],ld.[ClientID],ld.[Name],M.[CaseNo],ld.[BankID],ld.[Bank],
                              ld.[RetrunBankID],ld.[ReturnBank],ld.[Account],CONVERT(varchar(100),ld.[ReturnDate],111) as [ReturnDate],ld.LendStatus,
                              CONVERT(varchar(100),ld.[ReturnBankDate],111) as [ReturnBankDate],ld.[ReturnPostNo],ld.[BankReceiver],ld.[ReturnMemo]  
                              from LendData ld LEFT OUTER JOIN CaseMaster AS M ON LD.CaseId = M.CaseId
                              where 1=1 and LendStatus = @LendStatus and ReturnCaseId = @CaseId";

                IList<LendData> _ilsit = base.SearchList<LendData>(sqlStr);

                if (_ilsit != null && _ilsit.Count > 0)
                {
                    foreach (var item in _ilsit)
                    {
                        item.Bank = item.Bank.ToString();
                        item.CaseNo = item.CaseNo.ToString();
                        item.ReturnBankDate = UtlString.FormatDateTw(item.ReturnBankDate);
                        item.ReturnDate = UtlString.FormatDateTw(item.ReturnDate);
                    }
                    return _ilsit;
                }
                else
                {
                    return new List<LendData>();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region 查詢所有案件正本未歸還查詢結果lendstatus=0
        /// <summary>
        /// 得到所有案件正本未歸還查詢結果lendstatus=0
        /// </summary>
        /// <param name="model"></param>
        /// <param name="pageIndex"></param>
        /// <param name="strSortExpression"></param>
        /// <param name="strSortDirection"></param>
        /// <returns></returns>
        public IList<LendData> GetLendQueryList(AgentOriginalInfoViewModel model, int pageIndex, string strSortExpression, string strSortDirection)
        {
            try
            {
                base.PageIndex = pageIndex;
                string sqlStr = "";
                string sqlWhere = "";
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@pageS", (base.PageSize * (base.PageIndex - 1)) + 1));
                base.Parameter.Add(new CommandParameter("@pageE", base.PageSize * base.PageIndex));
                base.Parameter.Add(new CommandParameter("@CaseId", model.LendDataInfo.CaseId));
                base.Parameter.Add(new CommandParameter("@LendStatus", LendStatus.LendStatusSetting));

                if (!string.IsNullOrEmpty(model.LendDataInfo.DocNo))
                {
                    sqlWhere += @" and cm.CaseNo like @DocNo ";
                    base.Parameter.Add(new CommandParameter("@DocNo", "%" + model.LendDataInfo.DocNo + "%"));
                }
                if (!string.IsNullOrEmpty(model.LendDataInfo.GovNo))
                {
                    sqlWhere += @" and cm.GovNo like @GovNo ";
                    base.Parameter.Add(new CommandParameter("@GovNo", "%" + model.LendDataInfo.GovNo + "%"));
                }
                if (!string.IsNullOrEmpty(model.LendDataInfo.ClientID))
                {
                    sqlWhere += @" and ld.ClientID like @ClientID ";
                    base.Parameter.Add(new CommandParameter("@ClientID", "%" + model.LendDataInfo.ClientID + "%"));
                }
                if (!string.IsNullOrEmpty(model.LendDataInfo.Name))
                {
                    sqlWhere += @" and ld.Name like @Name ";
                    base.Parameter.Add(new CommandParameter("@Name", "%" + model.LendDataInfo.Name + "%"));
                }
                sqlWhere += @" and LendStatus = @LendStatus ";

                sqlStr = @" select ld.[LendID],ld.[CaseId],ld.[DocNo],ld.[ClientID],ld.[Name],cm.[CaseNo],ld.[BankID],ld.[Bank],
                              ld.[RetrunBankID],ld.[ReturnBank],ld.[Account],CONVERT(varchar(100),ld.[ReturnDate],111) as [ReturnDate],ld.LendStatus,
                              CONVERT(varchar(100),ld.[ReturnBankDate],111) as [ReturnBankDate],ld.[ReturnPostNo],ld.[BankReceiver],ld.[ReturnMemo]  
                              ,cm.GovNo
                              from LendData  ld left join CaseMaster cm on ld.CaseId=cm.CaseId
                              where 1=1 " + sqlWhere;

                IList<LendData> _ilsit = base.SearchList<LendData>(sqlStr);

                if (_ilsit != null && _ilsit.Count > 0)
                {
                    foreach (var item in _ilsit)
                    {
                        item.ReturnBankDate = UtlString.FormatDateTw(item.ReturnBankDate);
                        item.ReturnDate = UtlString.FormatDateTw(item.ReturnDate);
                    }
                    return _ilsit;
                }
                else
                {
                    return new List<LendData>();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region 查詢一筆正本歸還資料
        /// <summary>
        /// 得到一筆正本歸還資料
        /// </summary>
        /// <param name="lendId"></param>
        /// <returns></returns>
        public LendData getLendModel(string lendId)
        {
            string strSql = @"select ld.[LendID],ld.[CaseId],ld.[DocNo],m.[CaseNo],ld.[ClientID],ld.[Name],
                                       ld.[RetrunBankID],ld.[BankID],ld.[Bank],ld.[ReturnBank],ld.[Account],CONVERT(varchar(100),ld.[ReturnDate],111) as [ReturnDate],
                                       CONVERT(varchar(100),ld.[ReturnBankDate],111) as [ReturnBankDate],ld.[ReturnPostNo],ld.[BankReceiver],ld.[ReturnMemo]  
                                       from LendData  ld left join CaseMaster m on ld.CaseId=m.CaseId where ld.LendID=@LendID";

            Parameter.Clear();

            // 添加參數
            Parameter.Add(new CommandParameter("@LendID", lendId));

            IList<LendData> _list = base.SearchList<LendData>(strSql);

            if (_list != null)
            {
                if (_list.Count > 0)
                {
                    return _list[0];
                }
                return new LendData();
            }
            return new LendData();
        }
        #endregion

        #region 修改一筆正本調閱資料
        /// <summary>
        /// 修改一筆正本調閱資料
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public int UpdateLend(LendData model)
        {
            string strSql = @" update LendData set DocNo=@DocNo,ClientID=@ClientID,Name=@Name,Account=@Account,
                                        RetrunBankID=@RetrunBankID,ReturnBank=@ReturnBank,ReturnDate=@ReturnDate,
                                        ReturnBankDate=@ReturnBankDate,ReturnPostNo=@ReturnPostNo,
                                        BankReceiver=@BankReceiver,ReturnMemo=@ReturnMemo,LendStatus=@LendStatus,ReturnCaseId=@ReturnCaseId
                                        where LendID=@LendID";

            Parameter.Clear();

            // 添加參數
            Parameter.Add(new CommandParameter("@DocNo", model.DocNo));
            Parameter.Add(new CommandParameter("@ClientID", model.ClientID));
            Parameter.Add(new CommandParameter("@Name", model.Name));
            Parameter.Add(new CommandParameter("@Account", model.Account));
            Parameter.Add(new CommandParameter("@RetrunBankID", model.RetrunBankID));
            Parameter.Add(new CommandParameter("@ReturnBank", model.ReturnBank));
            Parameter.Add(new CommandParameter("@ReturnDate", model.ReturnDate));
            Parameter.Add(new CommandParameter("@ReturnBankDate", model.ReturnBankDate));
            Parameter.Add(new CommandParameter("@ReturnPostNo", model.ReturnPostNo));
            Parameter.Add(new CommandParameter("@BankReceiver", model.BankReceiver));
            Parameter.Add(new CommandParameter("@ReturnMemo", model.ReturnMemo));
            Parameter.Add(new CommandParameter("@LendStatus", model.LendStatus));
            Parameter.Add(new CommandParameter("@ReturnCaseId", model.ReturnCaseId));
            Parameter.Add(new CommandParameter("@LendID", model.LendID));

            int n = base.ExecuteNonQuery(strSql);
            if (n > 0)
            {
                return 1;
            }
            return 0;
        }
        #endregion

        #region 將正本歸還的狀態改為正本調閱
        /// <summary>
        /// 將正本歸還的狀態改為正本調閱
        /// </summary>
        /// <returns></returns>
        public int UpdateLendStatus(string LendId)
        {
            string strSql = @" update  LendData set LendStatus=@LendStatus,ReturnDate=null,ReturnCaseId=null,ReturnBankDate=null,RetrunBankID=null,
                                        ReturnBank=null, ReturnPostNo=null,BankReceiver=null,ReturnMemo=null where LendID=@LendID";
            Parameter.Clear();
            // 添加參數
            Parameter.Add(new CommandParameter("@LendID", LendId));
            Parameter.Add(new CommandParameter("@LendStatus", LendStatus.LendStatusSetting));

            return ExecuteNonQuery(strSql);
        }
        #endregion

        #region 正本備查查詢送件
        public IList<LendData> LendDataSearchList(LendData model, int pageIndex, string strSortExpression, string strSortDirection)
        {
            try
            {
                base.PageIndex = pageIndex;
                string sqlStr = "";
                string sqlWhere = "";
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@pageS", (base.PageSize * (base.PageIndex - 1)) + 1));
                base.Parameter.Add(new CommandParameter("@pageE", base.PageSize * base.PageIndex));

                if (!string.IsNullOrEmpty(model.CaseKind) && string.IsNullOrEmpty(model.CaseKind2))
                {
                    sqlWhere += @" and CaseKind = @CaseKind ";
                    base.Parameter.Add(new CommandParameter("@CaseKind",  model.CaseKind ));
                }
                else if (!string.IsNullOrEmpty(model.CaseKind) && !string.IsNullOrEmpty(model.CaseKind2))
                {
                    sqlWhere += @" and (CaseKind+'-'+CaseKind2) = @CaseKind2 ";
                    base.Parameter.Add(new CommandParameter("@CaseKind2",  model.CaseKind + "-"+model.CaseKind2));
                }
                if (!string.IsNullOrEmpty(model.GovKind))
                {
                    sqlWhere += @" and GovKind like @GovKind ";
                    base.Parameter.Add(new CommandParameter("@GovKind", "%" + model.GovKind + "%"));
                }
                if (!string.IsNullOrEmpty(model.GovUnit))
                {
                    sqlWhere += @" and GovUnit like @GovUnit ";
                    base.Parameter.Add(new CommandParameter("@GovUnit", "%" + model.GovUnit + "%"));
                }
                if (!string.IsNullOrEmpty(model.CaseNo))
                {
                    sqlWhere += @" and c.CaseNo like @CaseNo ";
                    base.Parameter.Add(new CommandParameter("@CaseNo", "%" + model.CaseNo + "%"));
                }
                if (!string.IsNullOrEmpty(model.GovDateStart))
                {
                    sqlWhere += @" and GovDate >= @DocNoStart";
                    base.Parameter.Add(new CommandParameter("@DocNoStart", model.GovDateStart));
                }
                if (!string.IsNullOrEmpty(model.GovDateEnd))
                {
                    string QDateE = Convert.ToDateTime(model.GovDateEnd).AddDays(1).ToString("yyyyMMdd");
                    sqlWhere += @" and GovDate < @DocNoEND ";
                    base.Parameter.Add(new CommandParameter("@DocNoEND", QDateE));
                }
                //if (!string.IsNullOrEmpty(model.GovDateStart) && !string.IsNullOrEmpty(model.GovDateEnd))
                //{
                //    sqlWhere += @" and GovDate between  @DocNoStart  and @DocNoEND";
                //    base.Parameter.Add(new CommandParameter("@DocNoStart", model.GovDateStart));
                //    base.Parameter.Add(new CommandParameter("@DocNoEND", model.GovDateEnd));
                //}
                if (!string.IsNullOrEmpty(model.Speed))
                {
                    sqlWhere += @" and Speed like @Speed ";
                    base.Parameter.Add(new CommandParameter("@Speed", "%" + model.Speed + "%"));
                }
                if (!string.IsNullOrEmpty(model.ReceiveKind))
                {
                    sqlWhere += @" and ReceiveKind like @ReceiveKind ";
                    base.Parameter.Add(new CommandParameter("@ReceiveKind", "%" + model.ReceiveKind + "%"));
                }
                if (!string.IsNullOrEmpty(model.GovNo))
                {
                    sqlWhere += @" and GovNo like @GovNo ";
                    base.Parameter.Add(new CommandParameter("@GovNo", "%" + model.GovNo + "%"));
                }
                //20170811 RC RQ-2015-019666-020 派件至跨單位 宏祥 add start
                //if (!string.IsNullOrEmpty(model.AgentUser))
                //{
                //    sqlWhere += @" and AgentUser like @AgentUser ";
                //    base.Parameter.Add(new CommandParameter("@AgentUser", "%" + model.AgentUser + "%"));
                //}
                string[] aryAgentDepartmentUser;
                string strAgentDepartmentUser = "";
                if (!string.IsNullOrEmpty(model.AgentDepartmentUser))
                {
                   aryAgentDepartmentUser = model.AgentDepartmentUser.Split('-');
                   strAgentDepartmentUser = aryAgentDepartmentUser.GetValue(0).ToString().Trim();
                   sqlWhere += @" AND AgentUser like @AgentUser ";
                   base.Parameter.Add(new CommandParameter("@AgentUser", "%" + strAgentDepartmentUser + "%"));
                }
                else if (!string.IsNullOrEmpty(model.AgentDepartment2))
                {
                   AgentSettingBIZ agentsettingBiz = new AgentSettingBIZ();
                   aryAgentDepartmentUser = agentsettingBiz.GetAgentSetting(model.AgentDepartment2).Split(',');
                   if (aryAgentDepartmentUser.Length > 0)
                   {
                      for (int i = 0; i < aryAgentDepartmentUser.Length; i++)
                      {
                         strAgentDepartmentUser += "'" + aryAgentDepartmentUser.GetValue(i).ToString().Trim() + "',";
                      }
                      strAgentDepartmentUser = strAgentDepartmentUser.Trim(',');

                      sqlWhere += @" AND AgentUser in (" + strAgentDepartmentUser + ")";
                   }
                }
                else if (!string.IsNullOrEmpty(model.AgentDepartment))
                {
                   AgentSettingBIZ agentsettingBiz = new AgentSettingBIZ();
                   IList<AgentSetting> list = agentsettingBiz.GetAgentDepartmentUserView(model.AgentDepartment);
                   foreach (AgentSetting item in list)
                   {
                      strAgentDepartmentUser += "'" + item.EmpId.ToString().Trim() + "',";
                   }
                   strAgentDepartmentUser = strAgentDepartmentUser.Trim(',');

                   sqlWhere += @" AND AgentUser in (" + strAgentDepartmentUser + ")";
                }
                //20170811 RC RQ-2015-019666-020 派件至跨單位 宏祥 add end
                if (!string.IsNullOrEmpty(model.ClientID))
                {
                    sqlWhere += @" and ClientID like @ClientID ";
                    base.Parameter.Add(new CommandParameter("@ClientID", "%" + model.ClientID + "%"));
                }
                if (model.LendStatus=="0")//*未歸還
                {
                    sqlWhere += @" and l.[ReturnCaseId]  is  null ";
                }
                if (model.LendStatus == "1")//*已歸還
                {
                    sqlWhere += @" and l.[ReturnCaseId]  is not null ";
                }
                sqlStr += @";with T1 
	                        as
	                        (
		                    select ROW_NUMBER() OVER(ORDER BY LendId asc) as Sno,l.[LendID],l.[CaseId],l.[DocNo],l.[ClientID],l.[Name],l.[BankID],l.[Bank],l.[Account],l.[Memo]
                                    ,l.[Phone],CONVERT(varchar(12),l.[ReturnDate],111) as [ReturnDate],CONVERT(varchar(12),l.[ReturnBankDate],111) as [ReturnBankDate],l.[RetrunBankID],l.[ReturnBank],l.[ReturnPostNo],l.[BankReceiver]
                                    ,l.[ReturnMemo],l.[LendStatus],(select CaseNo from CaseMaster m where m.CaseId=l.[ReturnCaseId]) AS returnCaseNo,c.[Status],c.[CaseNo],c.[GovKind],c.[GovUnit]
                                    ,c.[GovDate],c.[Speed],c.[ReceiveKind],c.[GovNo],c.[CaseKind],c.[CaseKind2],c.[AgentUser]
                                    ,(SELECT TOP 1 CONVERT(varchar(12),m.SendDate,111) FROM CaseSendSetting m where m.CaseId = l.CaseId ORDER BY SendDate desc) AS SendDate,
                                    (SELECT TOP 1 SendNo FROM CaseSendSetting m where m.CaseId = l.CaseId ORDER BY SendDate desc) AS SendNo,
                                    (select MAX( MailNo) from MailInfo mi where l.CaseId=mi.CaseId) as MailNo,
                                    (select top 1 CONVERT(nvarchar(10), mi.MailDate,111) from MailInfo mi where l.CaseId=mi.CaseId order by MailNo) as MailDate
                                    from LendData l left join CaseMaster c on l.CaseId=c.CaseId 
                                    where 1=1  " + sqlWhere + @"   
	                        ),T2 as
	                        (
		                        select *, row_number() over (order by " + strSortExpression + " " + strSortDirection + @" ) RowNum
		                        from T1
	                        ),T3 as 
	                        (
		                        select *,(select max(RowNum) from T2) maxnum from T2 
		                        where rownum between @pageS and @pageE
	                        )
	                        select a.* from T3 a order by a.RowNum";

                IList<LendData> _ilsit = base.SearchList<LendData>(sqlStr);
                if (_ilsit != null)
                {
                    if (_ilsit.Count > 0)
                    {
                        base.DataRecords = _ilsit[0].maxnum;
                    }
                    else
                    {
                        base.DataRecords = 0;
                        _ilsit = new List<LendData>();
                    }
                    return _ilsit;
                }
                else
                {
                    return new List<LendData>();
                }
                //if (_ilsit != null && _ilsit.Count > 0)
                //{
                //    foreach (var item in _ilsit)
                //    {
                //        list.Add(item);
                //    }
                //    return list;
                //}
                //else
                //{
                //    return new List<LendData>();
                //}
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        public bool DeleteLendData(Guid caseId, IDbTransaction dbtrans = null)
        {
            string strSql = "DELETE LendData WHERE  CaseId = @CaseId ";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("CaseId", caseId));
            return dbtrans == null ? ExecuteNonQuery(strSql) > 0 : ExecuteNonQuery(strSql, dbtrans) > 0;
        }

        public bool DeleteLendDataAttatchment(Guid caseId, IDbTransaction dbtrans = null)
        {
            string strSql = "DELETE LendAttachment WHERE  CaseId = @CaseId ";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("CaseId", caseId));
            return dbtrans == null ? ExecuteNonQuery(strSql) > 0 : ExecuteNonQuery(strSql, dbtrans) > 0;
        }

        public int UpdateReturnCaseId(Guid CaseId, IDbTransaction trans = null)
        {
            string strSql = @" update LendData set ReturnCaseId=NULL  where ReturnCaseId=@ReturnCaseId";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("@ReturnCaseId", CaseId));
            return trans == null ? base.ExecuteNonQuery(strSql) : base.ExecuteNonQuery(strSql, trans);
        }
    }
}
