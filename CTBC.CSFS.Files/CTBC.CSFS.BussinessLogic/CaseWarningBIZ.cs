using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Transactions;
using System.Threading.Tasks;
using CTBC.CSFS.Models;
using CTBC.FrameWork.Platform;
using CTBC.FrameWork.Util;
using CTBC.CSFS.Pattern;
using CTBC.CSFS.ViewModels;
using System.Data;
using CTBC.FrameWork.HTG;
using TIBCO.EMS;
using System.IO;
using System.Xml;
using NPOI.HSSF.UserModel;

namespace CTBC.CSFS.BussinessLogic
{
    public class CaseWarningBIZ : CommonBIZ
    {
        #region 根據被通報賬號獲取警示通報資料
        public List<WarningMaster> GetWarnMasterListByCustAccount(string custAccount)
        {
            string sql = @"SELECT [DocNo],[CustId],[CustName],[CustAccount]
                                ,[AccountStatus],[BankID],[BankName],[IsRelease],[CreatedUser],[CreatedDate],[ModifiedUser],[ModifiedDate]
                                ,ClosedDate,Currency,NotifyBal,CurBal,ReleaseBal,VD,MD FROM [WarningMaster]  where CustAccount = @CustAccount ";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("@CustAccount", custAccount));
            IList<WarningMaster> list = SearchList<WarningMaster>(sql);
            if (list != null && list.Count > 0) 
                return list.ToList();
            return null;
        }
        #endregion

        #region 根據案件編號獲取警示通報資料
        public WarningMaster GetWarnMasterListByDocNo(string DocNo)
        {
            string sql = @"SELECT [DocNo],[CustId],[CustName],[CustAccount]
                                ,[AccountStatus],[BankID],[BankName],[IsRelease],[CreatedUser],[CreatedDate],[ModifiedUser],[ModifiedDate]
                                 ,[ClosedDate],[Currency],[NotifyBal],[CurBal],[ReleaseBal],[VD],[MD] FROM [WarningMaster]  where DocNo = @DocNo ";
            base.Parameter.Clear();
            base.Parameter.Add(new CommandParameter("@DocNo", DocNo));
            IList<WarningMaster> list = base.SearchList<WarningMaster>(sql);
            if (list != null && list.Count > 0) return list[0];
            else return new WarningMaster();
        }
        #endregion

        #region 建檔一個警示案件
        /// <summary>
        /// 建檔一個警示案件
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool CreateWarnCase(ref CaseWarningViewModel model)
        {
            CaseNoTableBIZ noBiz = new CaseNoTableBIZ();
            CaseHistoryBIZ historyBiz = new CaseHistoryBIZ();
            IDbConnection dbConnection = OpenConnection();
            IDbTransaction dbTransaction = null;
            try
            {
                using (dbConnection)
                {
                    dbTransaction = dbConnection.BeginTransaction();

                    #region 主表
                    CaseNoTableBIZ casenotable = new CaseNoTableBIZ();
                    model.WarningMaster.DocNo = casenotable.GetCaseNoForWarn("B", dbTransaction);
                    Create(model.WarningMaster, dbTransaction);
                    #endregion

                    #region 附件
                    foreach (WarningAttachment attach in model.WarningAttachmentList)
                    {
                        attach.CreatedUser = model.WarningMaster.CreatedUser;
                        attach.DocNo = model.WarningMaster.DocNo;
                        CreateAttatchment(attach, dbTransaction);
                    }
                    #endregion

                    dbTransaction.Commit();
                }
                return true;
            }
            catch (Exception ex)
            {
                try
                {
                    if (dbTransaction != null) dbTransaction.Rollback();
                }
                catch (Exception)
                {
                    // ignored

                }
                throw ex;
                return false;
            }
        }
        #endregion

        #region 新增一筆附件
        public int CreateAttatchment(WarningAttachment model, IDbTransaction trans = null)
        {
            string strSql = @" insert into WarningAttachment  (DocNo,AttachmentName,AttachmentServerPath,AttachmentServerName,CreatedUser,CreatedDate) 
                                        values (
                                        @DocNo,@AttachmentName,@AttachmentServerPath,@AttachmentServerName,@CreatedUser,GETDATE());";

            base.Parameter.Clear();

            // 添加參數
            base.Parameter.Add(new CommandParameter("@DocNo", model.DocNo));
            base.Parameter.Add(new CommandParameter("@AttachmentName", model.AttachmentName));
            base.Parameter.Add(new CommandParameter("@AttachmentServerPath", model.AttachmentServerPath));
            base.Parameter.Add(new CommandParameter("@AttachmentServerName", model.AttachmentServerName));
            base.Parameter.Add(new CommandParameter("@CreatedUser", model.CreatedUser));
            return trans == null ? base.ExecuteNonQuery(strSql) : base.ExecuteNonQuery(strSql, trans);
        }
        #endregion

        #region 編輯一個警示案件
        public bool EditWarnCase(CaseWarningViewModel model)
        {
            IDbConnection dbConnection = OpenConnection();
            IDbTransaction dbTransaction = null;
            try
            {
                using (dbConnection)
                {
                    dbTransaction = dbConnection.BeginTransaction();
                    #region 主表
                    Edit(model.WarningMaster, dbTransaction);
                    #endregion

                    #region 附件
                    foreach (WarningAttachment attach in model.WarningAttachmentList)
                    {
                        attach.CreatedUser = model.WarningMaster.CreatedUser;
                        attach.DocNo = model.WarningMaster.DocNo;
                        CreateAttatchment(attach, dbTransaction);
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

        #region 刪除一筆附件
        public int DeleteAttatch(string AttatchId)
        {
            string strSql = " delete from WarningAttachment where AttachmentId=@AttachmentId ";
            base.Parameter.Clear();

            // 添加參數
            base.Parameter.Add(new CommandParameter("@AttachmentId", AttatchId));

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

        #region 根據AttachID 獲取一筆附件資料
        public WarningAttachment GetAttachDetailInfo(int attachId)
        {
            string strSql = @"SELECT [AttachmentId],[DocNo],[AttachmentName]
                                            ,[AttachmentServerPath],[AttachmentServerName],[CreatedUser],[CreatedDate] FROM [WarningAttachment]
                                            WHERE [AttachmentId]=@AttatchDetailId";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("@AttatchDetailId", attachId));
            IList<WarningAttachment> list = SearchList<WarningAttachment>(strSql);
            return list.FirstOrDefault();
        }
        #endregion

        #region 警示案件實際Create
        public int Create(WarningMaster model, IDbTransaction trans = null)
        {//Add by zhangwei 20180315 start
            string sql = @" insert into WarningMaster  ([DocNo],[CustId],[CustName],[CustAccount]
                                    ,[AccountStatus],[BankID],[BankName],[IsRelease],[CreatedUser],[CreatedDate],[ClosedDate],[Currency],[NotifyBal],[CurBal],[ReleaseBal],[VD],[MD]) 
                                        values (
                                        @DocNo,@CustId,@CustName,@CustAccount
                                    ,@AccountStatus,@BankID,@BankName,@IsRelease,@CreatedUser,GETDATE(),@ClosedDate,@Currency,@NotifyBal,@CurBal,@ReleaseBal,@VD,@MD);";

            Parameter.Clear();

            // 添加參數
            Parameter.Add(new CommandParameter("@DocNo", model.DocNo));
            Parameter.Add(new CommandParameter("@CustId", model.CustId));
            Parameter.Add(new CommandParameter("@CustName", model.CustName));
            Parameter.Add(new CommandParameter("@CustAccount", model.CustAccount));
            Parameter.Add(new CommandParameter("@AccountStatus", model.AccountStatus));
            Parameter.Add(new CommandParameter("@BankID", model.BankID));
            Parameter.Add(new CommandParameter("@BankName", model.BankName));
            Parameter.Add(new CommandParameter("@IsRelease", model.IsRelease));
            Parameter.Add(new CommandParameter("@CreatedUser", model.CreatedUser));
            Parameter.Add(new CommandParameter("@ClosedDate", model.ClosedDate));
            Parameter.Add(new CommandParameter("@Currency", model.Currency));
            Parameter.Add(new CommandParameter("@NotifyBal", model.NotifyBal));
            Parameter.Add(new CommandParameter("@CurBal", model.CurBal));
            Parameter.Add(new CommandParameter("@ReleaseBal", model.ReleaseBal));
            Parameter.Add(new CommandParameter("@VD", model.VD));
            Parameter.Add(new CommandParameter("@MD", model.MD));
            return trans == null ? ExecuteNonQuery(sql) : ExecuteNonQuery(sql, trans);
            //Add by zhangwei 20180315 end
        }
        #endregion

        #region 警示案件實際Edit
        public int Edit(WarningMaster model, IDbTransaction trans = null)
        {
            string sql = @" UPDATE  WarningMaster   SET [CustId]=@CustId,[CustName]=@CustName,[CustAccount]=@CustAccount
                                    ,[AccountStatus]=@AccountStatus,[BankID]=@BankID,[BankName]=@BankName,[IsRelease]=@IsRelease,
                                    [ModifiedUser]=@ModifiedUser,[ModifiedDate]=GETDATE(),
                                    [ClosedDate]=@ClosedDate,[Currency]=@Currency,[NotifyBal]=@NotifyBal,[CurBal]=@CurBal,[ReleaseBal]=@ReleaseBal,[VD]=@VD,[MD]=@MD
                                    WHERE [DocNo]=@DocNo";

            Parameter.Clear();

            // 添加參數
            Parameter.Add(new CommandParameter("@DocNo", model.DocNo));
            Parameter.Add(new CommandParameter("@CustId", model.CustId));
            Parameter.Add(new CommandParameter("@CustName", model.CustName));
            Parameter.Add(new CommandParameter("@CustAccount", model.CustAccount));
            Parameter.Add(new CommandParameter("@AccountStatus", model.AccountStatus));
            Parameter.Add(new CommandParameter("@BankID", model.BankID));
            Parameter.Add(new CommandParameter("@BankName", model.BankName));
            Parameter.Add(new CommandParameter("@IsRelease", model.IsRelease));
            Parameter.Add(new CommandParameter("@ModifiedUser", model.ModifiedUser));
            Parameter.Add(new CommandParameter("@ClosedDate", model.ClosedDate));
            Parameter.Add(new CommandParameter("@Currency", model.Currency));
            Parameter.Add(new CommandParameter("@NotifyBal", model.NotifyBal));
            Parameter.Add(new CommandParameter("@CurBal", model.CurBal));
            Parameter.Add(new CommandParameter("@ReleaseBal", model.ReleaseBal));
            Parameter.Add(new CommandParameter("@VD", model.VD));
            Parameter.Add(new CommandParameter("@MD", model.MD));
            return trans == null ? ExecuteNonQuery(sql) : ExecuteNonQuery(sql, trans);
        }
        #endregion

        #region 獲取警示調閱歷史List
        public List<CaseCustRFDMRecv> GetCaseCustRFDMRecvList(string DocNo)
        {
            try
            {
                string sqlStr = "";
                sqlStr += @" SELECT  [TrnNum]
      ,[VersionNewID]
      ,[DATA_DATE]
      ,[ACCT_NO]
      ,[JNRST_DATE]
      ,[JNRST_TIME]
      ,[JNRST_TIME_SEQ]
      ,[TRAN_DATE]
      ,[POST_DATE]
      ,[TRANS_CODE]
      ,[JRNL_NO]
      ,[REVERSE]
      ,[PROMO_CODE]
      ,[REMARK]
      ,[TRAN_AMT]
      ,[BALANCE]
      ,[TRF_BANK]
      ,[TRF_ACCT]
      ,[NARRATIVE]
      ,[FISC_BANK]
      ,[FISC_SEQNO]
      ,[CHQ_NO]
      ,[ATM_NO]
      ,[TRAN_BRANCH]
      ,[TELLER]
      ,[FILLER]
      ,[TXN_DESC]
      ,[ACCT_P2]
      ,[FILE_NAME]
      ,[TYPE]
      ,w.ForCDateS,w.ForCDateE
                                      FROM [CaseCustRFDMRecv] r 
    inner join WarningQueryHistory w on r.NewID = w.NewID
where w.DocNo=@DocNo";

                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@DocNo", DocNo));
                List<CaseCustRFDMRecv> _ilsit = base.SearchList<CaseCustRFDMRecv>(sqlStr).ToList();

                if (_ilsit != null)
                {
                    return _ilsit;
                }
                else
                {
                    return new List<CaseCustRFDMRecv>();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public List<WarningQueryHistory> GetWarningHistoryList(string DocNo)
        {
            try
            {
                string sqlStr = @"select [NewID],[DocNo],[CustAccount], CONVERT(varchar(10), [ForCDateS], 111) as [ForCDateS], CONVERT(varchar(10), [ForCDateE], 111) as [ForCDateE],TrnNum,FileName,ESBStatus from WarningQueryHistory where DocNo=@DocNo";

                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@DocNo", DocNo));
                List<WarningQueryHistory> _ilsit = base.SearchList<WarningQueryHistory>(sqlStr).ToList();

                if (_ilsit != null)
                {
                    return _ilsit;
                }
                else
                {
                    return new List<WarningQueryHistory>();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region 獲取警示附件List
        public List<WarningAttachment> GetWarnAttatchmentList(string DocNo)
        {
            try
            {
                string sqlStr = "";
                sqlStr += @" SELECT  [AttachmentId],[DocNo],[AttachmentName]
                                                 ,[AttachmentServerPath],[AttachmentServerName],[CreatedUser],[CreatedDate]
                                      FROM [WarningAttachment] where DocNo=@DocNo";

                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@DocNo", DocNo));
                List<WarningAttachment> _ilsit = base.SearchList<WarningAttachment>(sqlStr).ToList();

                if (_ilsit != null)
                {
                    return _ilsit;
                }
                else
                {
                    return new List<WarningAttachment>();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region 獲取警示狀態List
        public List<WarningState> GetWarnStateQueryList(string DocNo)
        {
            try
            {
                string sqlStr = "";
                sqlStr += @" SELECT  [DocNo],[NotificationSource]
                                               , Convert(nvarchar(10),[RelieveDate],111) as RelieveDate,[RelieveReason],[OtherReason],[CreatedUser],[CreatedDate],[ModifiedUser],[ModifiedDate],[EtabsNo]
                                  FROM [WarningState] where DocNo=@DocNo";

                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@DocNo", DocNo));

                List<WarningState> _ilsit = base.SearchList<WarningState>(sqlStr).ToList();
                if (_ilsit != null)
                {
                    return _ilsit;
                }
                else
                {
                    return new List<WarningState>();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region 獲取警示明細List
        public List<WarningDetails> WarningDetailsSearchList(string DocNo)
        {
            try
            {
                string sqlStr = "";
                string sqlWhere = "";


                //if (!string.IsNullOrEmpty(model.GovKind))
                //{
                //    sqlWhere += @" and GovKind like @GovKind ";
                //    base.Parameter.Add(new CommandParameter("@GovKind", "%" + model.GovKind + "%"));
                //}
                sqlStr += @" SELECT [SerialID],[DocNo], Convert(Nvarchar(10), [HappenDateTime],111) as [HappenDateTime],[No_165]
                                         ,[No_e],[NotificationContent],[NotificationSource],Convert(Nvarchar(10), [ForCDate],111) as [ForCDate],Convert(Nvarchar(10), [EtabsDatetime],111) as [EtabsDatetime],[NotificationUnit]
                                         ,[NotificationName],[ExtPhone],[PoliceStation],[VictimName],[CreatedUser],[CreatedDate],[ModifiedUser],[ModifiedDate],[Original]
                                  FROM [WarningDetails] where DocNo =@DocNo";
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@DocNo", DocNo));

                List<WarningDetails> _ilsit = base.SearchList<WarningDetails>(sqlStr).ToList();
                if (_ilsit != null)
                {
                    return _ilsit;
                }
                else
                {
                    return new List<WarningDetails>();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region 警示明細Create(同時新增通報來源資訊)
        public bool CreateWarnContent(WarningDetails model)
        {
            IDbConnection dbConnection = OpenConnection();
            IDbTransaction dbTransaction = null;
            bool flag = true;
            try
            {
                using (dbConnection)
                {
                    dbTransaction = dbConnection.BeginTransaction();

                    #region 案發內容
                    flag = flag && CreateContent(model, dbTransaction) > 0;
                    #endregion

                    #region 警示狀態
                    WarningState statemodel = new WarningState();
                    statemodel.DocNo = model.DocNo;
                    statemodel.NotificationSource = model.NotificationSource;

                    if (IsExistStatus(model.DocNo, model.NotificationSource) == 0)//*新增时，如果已存在该类通报来源则不新增，没有才新增
                    {
                        statemodel.CreatedUser = model.CreatedUser;
                        flag = flag && CreateStatus(statemodel, dbTransaction) > 0;
                    }

                    if (IsAllStatus(statemodel, dbTransaction) > 0)//*该案件尚未结案
                    {
                        if (IsExistStatus(model.DocNo, model.NotificationSource, dbTransaction) > 0)//*且警示状态中存在该笔资料，则将该笔通报来源设为空白
                        {
                            flag = flag && EditStatus(statemodel, dbTransaction)>0;
                        }
                    }
                    #endregion

                    if (flag)
                    {
                        dbTransaction.Commit();
                        return true;
                    }
                    dbTransaction.Rollback();
                    return false;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region 警示明細CreateContent
        public int CreateContent(WarningDetails model, IDbTransaction trans = null)
        {
            string sql = @" insert into WarningDetails  ([DocNo],[HappenDateTime],[No_165]
                                         ,[No_e],[NotificationContent],[NotificationSource],[ForCDate],[EtabsDatetime],[NotificationUnit]
                                         ,[NotificationName],[ExtPhone],[PoliceStation],[VictimName],[CreatedUser],[CreatedDate],[DocAddress],[Original],[StateType]) 
                                        values (
                                        @DocNo,@HappenDateTime,@No_165
                                         ,@No_e,@NotificationContent,@NotificationSource,@ForCDate,@EtabsDatetime,@NotificationUnit
                                         ,@NotificationName,@ExtPhone,@PoliceStation,@VictimName,@CreatedUser,GETDATE(),@DocAddress,@Original,@StateType);";

            Parameter.Clear();

            // 添加參數
            Parameter.Add(new CommandParameter("@DocNo", model.DocNo));
            Parameter.Add(new CommandParameter("@HappenDateTime", model.HappenDateTime));
            Parameter.Add(new CommandParameter("@No_165", model.No_165));
            Parameter.Add(new CommandParameter("@No_e", model.No_e));
            Parameter.Add(new CommandParameter("@NotificationContent", model.NotificationContent));
            Parameter.Add(new CommandParameter("@NotificationSource", model.NotificationSource));
            Parameter.Add(new CommandParameter("@ForCDate", model.ForCDate));
            Parameter.Add(new CommandParameter("@EtabsDatetime", model.EtabsDatetime));
            Parameter.Add(new CommandParameter("@NotificationUnit", model.NotificationUnit));
            Parameter.Add(new CommandParameter("@NotificationName", model.NotificationName));
            Parameter.Add(new CommandParameter("@ExtPhone", model.ExtPhone));
            Parameter.Add(new CommandParameter("@PoliceStation", model.PoliceStation));
            Parameter.Add(new CommandParameter("@VictimName", model.VictimName));
            Parameter.Add(new CommandParameter("@CreatedUser", model.CreatedUser));
            Parameter.Add(new CommandParameter("@DocAddress", model.DocAddress));
            Parameter.Add(new CommandParameter("@Original", model.Original));
            Parameter.Add(new CommandParameter("@StateType", string.IsNullOrEmpty(model.Original)?0:1));//新增警示案件狀態，電文發送成功則1否則就是普通新增0
            return trans == null ? ExecuteNonQuery(sql) : ExecuteNonQuery(sql, trans);
        }
        #endregion

        #region 警示狀態CreateStatus
        public int CreateStatus(WarningState model, IDbTransaction trans = null)
        {
            string sql = @" insert into WarningState  ([DocNo],[NotificationSource],[RelieveDate],[RelieveReason]
                                          ,[OtherReason],[CreatedUser],[CreatedDate]) 
                                    values (
                                        @DocNo,@NotificationSource,@RelieveDate,@RelieveReason
                                           ,@OtherReason,@CreatedUser,GETDATE());";

            Parameter.Clear();

            // 添加參數
            Parameter.Add(new CommandParameter("@DocNo", model.DocNo));
            Parameter.Add(new CommandParameter("@NotificationSource", model.NotificationSource));
            Parameter.Add(new CommandParameter("@RelieveDate", model.RelieveDate));
            Parameter.Add(new CommandParameter("@RelieveReason", model.RelieveReason));
            Parameter.Add(new CommandParameter("@OtherReason", model.OtherReason));
            Parameter.Add(new CommandParameter("@CreatedUser", model.CreatedUser));

            return trans == null ? ExecuteNonQuery(sql) : ExecuteNonQuery(sql, trans);
        }
        #endregion

        #region 判斷是否已存在警示狀態
        public int IsExistStatus(string DocNo, string NotificationSource, IDbTransaction trans = null)
        {
            string sql = @" select COUNT(*) from WarningState where DocNo=@DocNo and NotificationSource=@NotificationSource";

            Parameter.Clear();
            // 添加參數
            Parameter.Add(new CommandParameter("@DocNo", DocNo));
            Parameter.Add(new CommandParameter("@NotificationSource", NotificationSource));
            return trans == null ? (int)ExecuteScalar(sql) : (int)ExecuteScalar(sql, trans);
        }
        #endregion

        #region 判斷警示明細是否已存在警示狀態
        public int IsExistStatusInDetails(string DocNo, string NotificationSource, IDbTransaction trans = null)
        {
            string sql = @" select COUNT(*) from WarningDetails where DocNo=@DocNo and NotificationSource=@NotificationSource";

            Parameter.Clear();
            // 添加參數
            Parameter.Add(new CommandParameter("@DocNo", DocNo));
            Parameter.Add(new CommandParameter("@NotificationSource", NotificationSource));
            return trans == null ? (int)ExecuteScalar(sql) : (int)ExecuteScalar(sql, trans);
        }
        #endregion

        #region 判斷警示狀態是否要刪除
        public int IsDeleteStatus(string DocNo, string NotificationSource, IDbTransaction trans = null)
        {
            string sql = @" select COUNT(*) from WarningDetails where DocNo=@DocNo and NotificationSource=@NotificationSource";

            Parameter.Clear();
            // 添加參數
            Parameter.Add(new CommandParameter("@DocNo", DocNo));
            Parameter.Add(new CommandParameter("@NotificationSource", NotificationSource));
            return trans == null ? (int)ExecuteScalar(sql) : (int)ExecuteScalar(sql, trans);
        }
        #endregion

        #region 設定警示狀態
        public JsonReturn SetStatus(WarningState model)
        {
            IDbConnection dbConnection = OpenConnection();
            IDbTransaction dbTransaction = null;
            bool flag = true;
            try
            {
                using (dbConnection)
                {
                    dbTransaction = dbConnection.BeginTransaction();
                    //Add by zhangwei 20180315 start
                    string strStateType = "2";
                    //解除警示日期和電文都有值的情況下案發內容狀態為解除狀態
                    if (!string.IsNullOrEmpty(model.RelieveDate.Trim())&& !string.IsNullOrEmpty(model.EtabsTrnNum))
                    {
                        strStateType = "5";
                    }
                    else if(string.IsNullOrEmpty(model.RelieveDate.Trim()) && !string.IsNullOrEmpty(model.EtabsTrnNum))//為取消解除狀態
                    {
                        strStateType = "6";
                    }
                    else
                    {
                        strStateType = "2";//否則就是普通的修改
                    }
                    
                    #region 更新警示狀態
                    //flag = flag && EditStatus(model, dbTransaction) > 0;
                    flag = flag && EditStatus(model, dbTransaction) > 0 && EditStateType(model, strStateType, dbTransaction) > 0;
                    #endregion
                    //Add by zhangwei 20180315 end
                    //*如果該案件全部設定解除 ,即提示結案
                    if (IsAllStatus(model, dbTransaction) == 0)
                    {
                        if (flag)
                        {
                            dbTransaction.Commit();
                            return new JsonReturn() { ReturnCode = "2" };
                        }
                    }

                    if (flag)
                    {
                        dbTransaction.Commit();
                        return new JsonReturn() { ReturnCode = "1" };
                    }
                    dbTransaction.Rollback();
                    return new JsonReturn() { ReturnCode = "0" };
                }
            }
            catch (Exception ex)
            {
                dbTransaction.Rollback();
                throw ex;
            }
        }
        #endregion

        #region 判斷該案件的警示狀態是否全部解除時(即是否結案)
        public int IsAllStatus(WarningState model, IDbTransaction trans = null)
        {
            string sql = @" select COUNT(*) from WarningState where DocNo=@DocNo and ModifiedUser is  null";

            Parameter.Clear();
            // 添加參數
            Parameter.Add(new CommandParameter("@DocNo", model.DocNo));
            return trans == null ? (int)ExecuteScalar(sql) : (int)ExecuteScalar(sql, trans);
        }

        public int IsEmptyStatus(WarningState model, IDbTransaction trans = null)
        {
            string sql = @" select COUNT(*) from WarningState where DocNo=@DocNo ";

            Parameter.Clear();
            // 添加參數
            Parameter.Add(new CommandParameter("@DocNo", model.DocNo));
            return trans == null ? (int)ExecuteScalar(sql) : (int)ExecuteScalar(sql, trans);
        }
        #endregion

        #region 警示狀態EditStatus
        public int EditStatus(WarningState model, IDbTransaction trans = null)
        {
            string sql = @" UPDATE WarningState   SET [RelieveDate]=@RelieveDate,[RelieveReason]=@RelieveReason
                                          ,[OtherReason]=@OtherReason,[ModifiedUser]=@ModifiedUser,[ModifiedDate]=GETDATE(),[EtabsNo]=@EtabsNo
                                         WHERE [DocNo]=@DocNo AND [NotificationSource]=@NotificationSource;";

            Parameter.Clear();

            // 添加參數
            Parameter.Add(new CommandParameter("@DocNo", model.DocNo));
            Parameter.Add(new CommandParameter("@NotificationSource", model.NotificationSource));
            Parameter.Add(new CommandParameter("@RelieveDate", model.RelieveDate));
            Parameter.Add(new CommandParameter("@RelieveReason", model.RelieveReason));
            Parameter.Add(new CommandParameter("@OtherReason", model.OtherReason));
            Parameter.Add(new CommandParameter("@ModifiedUser", model.ModifiedUser));
            Parameter.Add(new CommandParameter("@EtabsNo", model.EtabsNo));
            return trans == null ? ExecuteNonQuery(sql) : ExecuteNonQuery(sql, trans);
        }
        #endregion

        #region 獲取警示狀態資料
        public WarningState GetWarnStateInfo(string DocNo, string NotificationSource)
        {
            string strSql = @"SELECT [DocNo],[NotificationSource],[RelieveDate],[RelieveReason]
                                          ,[OtherReason],[CreatedUser],[CreatedDate],[ModifiedUser],[ModifiedDate],[EtabsNo] FROM [WarningState]
                                            WHERE [DocNo]=@DocNo and [NotificationSource]=@NotificationSource";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("@DocNo", DocNo));
            Parameter.Add(new CommandParameter("@NotificationSource", NotificationSource));
            IList<WarningState> list = SearchList<WarningState>(strSql);
            return list.FirstOrDefault();
        }
        #endregion

        #region 警示明細Edit(同時編輯通報來源資訊)
        public bool EditWarnContent(WarningDetails model)
        {
            IDbConnection dbConnection = OpenConnection();
            IDbTransaction dbTransaction = null;
            bool flag = true;
            try
            {
                using (dbConnection)
                {
                    dbTransaction = dbConnection.BeginTransaction();
                   

                    #region 警示狀態
                    WarningState stateModel=new WarningState();
                    WarningDetails detailModelOld = GetWarnDetailBySerialID(model.SerialID.ToString());
                    string strDocNo = detailModelOld.DocNo;
                    string strSource = detailModelOld.NotificationSource;
                    if (detailModelOld.NotificationSource != model.NotificationSource)//*改變了通報來源
                    {
                        if (IsExistStatus(model.DocNo, model.NotificationSource, dbTransaction) == 0)//*不存在該通報來源，則新增該來源
                        {
                            stateModel.DocNo=model.DocNo;
                            stateModel.NotificationSource=model.NotificationSource;
                            flag = flag && CreateStatus(stateModel, dbTransaction)>0;
                        }
                    }
                    #endregion
                    //Add by zhangwei 20180315 start
                    //對比之前的設定日期和正本有無變化，有變化則為人為修改
                    //如果有修改設定時間並且沒有發送電文則狀態設置為4
                    detailModelOld.EtabsDatetime = string.IsNullOrEmpty(detailModelOld.EtabsDatetime) ? "" : DateTime.Parse(detailModelOld.EtabsDatetime).ToString("yyyy/MM/dd HH:mm");
                    if ((model.EtabsDatetime != detailModelOld.EtabsDatetime || model.Original != detailModelOld.Original) && string.IsNullOrEmpty(model.EtabsTrnNum))
                    {
                        model.StateType = "4";
                    }
                    else if (!string.IsNullOrEmpty(model.EtabsTrnNum))
                    {
                        model.StateType = "3";//有電文的修改
                    }
                    else
                    {
                         model.StateType = "2";//無電文的修改
                    }
                    //Add by zhangwei 20180315 end
                    #region 案發內容
                    flag = flag && EditWarnContent(model, dbTransaction) > 0;
                    #endregion


                    if (IsExistStatusInDetails(strDocNo, strSource, dbTransaction) == 0)//警示明細里已無改來源，則刪除警示狀態的資料
                    {
                        flag = flag && DeleteStatus(detailModelOld.DocNo, detailModelOld.NotificationSource, dbTransaction) > 0;
                    }

                    if (flag)
                    {
                        dbTransaction.Commit();
                        return true;
                    }
                    dbTransaction.Rollback();
                    return false;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region 警示明細EditWarnContent
        public int EditWarnContent(WarningDetails model, IDbTransaction trans = null)
        {
            string sql = @" UPDATE WarningDetails  SET [DocNo]=@DocNo,[HappenDateTime]=@HappenDateTime,[No_165]=@No_165
                                         ,[No_e]=@No_e,[NotificationContent]=@NotificationContent,[NotificationSource]=@NotificationSource,
                                         [ForCDate]=@ForCDate,[EtabsDatetime]=@EtabsDatetime,[NotificationUnit]=@NotificationUnit
                                         ,[NotificationName]=@NotificationName,[ExtPhone]=@ExtPhone,[PoliceStation]=@PoliceStation,
                                        [VictimName]=@VictimName,[ModifiedUser]=@ModifiedUser,[ModifiedDate]=GETDATE(),[StateType]=@StateType,
                                        [Original]=@Original
                                   FROM [WarningDetails] where   SerialID=@SerialID";
            base.Parameter.Clear();
            base.Parameter.Add(new CommandParameter("SerialID", model.SerialID));
            Parameter.Add(new CommandParameter("@DocNo", model.DocNo));
            Parameter.Add(new CommandParameter("@HappenDateTime", model.HappenDateTime));
            Parameter.Add(new CommandParameter("@No_165", model.No_165));
            Parameter.Add(new CommandParameter("@No_e", model.No_e));
            Parameter.Add(new CommandParameter("@NotificationContent", model.NotificationContent));
            Parameter.Add(new CommandParameter("@NotificationSource", model.NotificationSource));
            Parameter.Add(new CommandParameter("@ForCDate", model.ForCDate));
            Parameter.Add(new CommandParameter("@EtabsDatetime", model.EtabsDatetime));
            Parameter.Add(new CommandParameter("@NotificationUnit", model.NotificationUnit));
            Parameter.Add(new CommandParameter("@NotificationName", model.NotificationName));
            Parameter.Add(new CommandParameter("@ExtPhone", model.ExtPhone));
            Parameter.Add(new CommandParameter("@PoliceStation", model.PoliceStation));
            Parameter.Add(new CommandParameter("@VictimName", model.VictimName));
            Parameter.Add(new CommandParameter("@ModifiedUser", model.ModifiedUser));
            Parameter.Add(new CommandParameter("@StateType", model.StateType));
            Parameter.Add(new CommandParameter("@Original", model.Original));
            return trans == null ? ExecuteNonQuery(sql) : ExecuteNonQuery(sql, trans);
        }
        #endregion

        #region 根據案件編號獲取最後一筆警示通報明細資料
        public WarningDetails GetLastWarningDetails(string DocNo)
        {
            string sql = @"SELECT TOP 1 [SerialID],[DocNo], HappenDateTime,[No_165]
                                         ,[No_e],[NotificationContent],[NotificationSource],Convert(Nvarchar(10), [ForCDate],111) as [ForCDate],[EtabsDatetime],[NotificationUnit]
                                         ,[NotificationName],[ExtPhone],[PoliceStation],[VictimName],[CreatedUser],[CreatedDate],[ModifiedUser],[ModifiedDate],[Original]
                                  FROM [WarningDetails] where DocNo = @DocNo order by CreatedDate DESC";
            base.Parameter.Clear();
            base.Parameter.Add(new CommandParameter("@DocNo", DocNo));
            IList<WarningDetails> list = base.SearchList<WarningDetails>(sql);
            if (list != null && list.Count > 0) return list[0];
            else return new WarningDetails();
        }
        #endregion

        #region 根據SerialID獲取警示明細資料
        public WarningDetails GetWarnDetailBySerialID(string serialId, IDbTransaction trans = null)
        {
            string strSql = @" SELECT [SerialID],[DocNo], [HappenDateTime],[No_165]
                                         ,[No_e],[NotificationContent],[NotificationSource],Convert(Nvarchar(10), [ForCDate],111) as [ForCDate],[EtabsDatetime],[NotificationUnit]
                                         ,[NotificationName],[ExtPhone],[PoliceStation],[VictimName],[CreatedUser],[CreatedDate],[ModifiedUser],[ModifiedDate],[Original]
                                  FROM [WarningDetails] where   SerialID=@SerialID";
            base.Parameter.Clear();
            base.Parameter.Add(new CommandParameter("SerialID", serialId));
            IList<WarningDetails> list = trans == null ? base.SearchList<WarningDetails>(strSql) : base.SearchList<WarningDetails>(strSql, trans);
            if (list != null && list.Count > 0) return list[0];
            else return new WarningDetails();
        }
        #endregion

        #region 警示明細Delete(同時删除通報來源資訊)
        public bool DeleteWarnContents(string serialID, string DocNo, string Source)
        {
            IDbConnection dbConnection = OpenConnection();
            IDbTransaction dbTransaction = null;
            bool flag = true;
            try
            {
                using (dbConnection)
                {
                    dbTransaction = dbConnection.BeginTransaction();

                    #region 删除警示狀態
                    if (IsDeleteStatus(DocNo, Source, dbTransaction) == 1)
                    {
                        flag = flag && DeleteStatus(DocNo, Source, dbTransaction) > 0;
                    }
                    #endregion

                    #region 删除案發內容
                    flag = flag && DeleteWarnContent(serialID, dbTransaction) > 0;
                    #endregion

                    if (flag)
                    {
                        dbTransaction.Commit();
                        return true;
                    }
                    dbTransaction.Rollback();
                    return false;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region 根據刪除一筆警示状态
        public int DeleteStatus(string DocNo, string Source,IDbTransaction trans = null)
        {
            string sql = @" Delete  FROM [WarningState]   WHERE [DocNo]=@DocNo AND [NotificationSource]=@NotificationSource";
            base.Parameter.Clear();
            Parameter.Add(new CommandParameter("@DocNo", DocNo));
            Parameter.Add(new CommandParameter("@NotificationSource", Source));
            return trans == null ? ExecuteNonQuery(sql) : ExecuteNonQuery(sql, trans);
        }
        #endregion

        #region 根據serialID刪除一筆警示明細
        public int DeleteWarnContent(string serialID, IDbTransaction trans = null)
        {
            string sql = @" Delete  FROM [WarningDetails] where   SerialID=@SerialID";
            base.Parameter.Clear();
            base.Parameter.Add(new CommandParameter("SerialID", serialID));
            return trans == null ? ExecuteNonQuery(sql) : ExecuteNonQuery(sql, trans);
        }
        #endregion

        public DataTable GetCaseWarningBySerialID(string SerialID)
        {
            string strsql = @"select PoliceStation,HappenDateTime,ForCDate,No_165,No_e,CustName,CustAccount,EtabsDatetime from WarningDetails wd,WarningMaster wm where wd.DocNo=wm.DocNo and SerialID=@SerialID ";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("@SerialID", SerialID));
            DataTable dt = Search(strsql);
            return dt.Columns.Count > 0 ? dt : new DataTable();
        }
        public int ProduceGenDetail(string DocNo, string CustId, string CustAccount, string ForCDateS, string ForCDateE,string UserID, ref string strTrnNum, IDbTransaction trans = null)
        {
            try
            {
                strTrnNum = "CSFS" + DateTime.Now.ToString("yyyyMMddHHmmssf");
                string strsql = @"insert into WarningQueryHistory([NewID],[DocNo],[CustId],[CustAccount],[ForCDateS],[ForCDateE],[CreatedUser],[CreatedDate],[Status],TrnNum)
	values(@NewID,@DocNo,@CustId,@CustAccount,@ForCDateS,@ForCDateE,@CreatedUser,GETDATE(),@Status,@TrnNum)";
                Parameter.Clear();
                Parameter.Add(new CommandParameter("@NewID", Guid.NewGuid()));
                Parameter.Add(new CommandParameter("@DocNo", DocNo));
                Parameter.Add(new CommandParameter("@CustId", CustId));
                Parameter.Add(new CommandParameter("@CustAccount", CustAccount));
                Parameter.Add(new CommandParameter("@ForCDateS", ForCDateS));
                Parameter.Add(new CommandParameter("@ForCDateE", ForCDateE));
                Parameter.Add(new CommandParameter("@CreatedUser", UserID));
                Parameter.Add(new CommandParameter("@Status", "0"));
                Parameter.Add(new CommandParameter("@TrnNum", strTrnNum));
                return trans == null ? ExecuteNonQuery(strsql) : ExecuteNonQuery(strsql, trans);
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }
        /// <summary>
        /// 更改案發內容狀態
        /// </summary>
        /// <param name="StateType"></param>
        /// <param name="trans"></param>
        /// <returns></returns>
        public int EditStateType(WarningState model,string StateType, IDbTransaction trans = null)
        {
            string sql = @" UPDATE WarningDetails   SET [StateType]=@StateType
                                         WHERE [DocNo]=@DocNo AND [NotificationSource]=@NotificationSource;";

            Parameter.Clear();

            // 添加參數
            Parameter.Add(new CommandParameter("@DocNo", model.DocNo));
            Parameter.Add(new CommandParameter("@NotificationSource", model.NotificationSource));
            Parameter.Add(new CommandParameter("@StateType", StateType));
            return trans == null ? ExecuteNonQuery(sql) : ExecuteNonQuery(sql, trans);
        }
        /// <summary>
        /// 發送電文33401
        /// </summary>
        /// <param name="model"></param>
        public void Require33401(WarningMaster model,ref string strErrorCodeAndMsg)
        {
            string strResult = "";
            ExecuteHTG execHtg = new ExecuteHTG();
            strResult = execHtg.Send401("", "", model.CustAccount);
            if (strResult!="")
            {
                string[] strlist = strResult.Split('|');
                if (strlist[0].Trim() == "0000")
                {
                    //如果電文回應成功，則去數據庫TX_00401取數據
                    string strsql = @"select * from TX_33401 where Acct like @Acct and TrnNum=@TrnNum";
                    Parameter.Clear();
                    Parameter.Add(new CommandParameter("@Acct", "%"+ model.CustAccount+ "%"));
                    Parameter.Add(new CommandParameter("@TrnNum", strlist[1].Trim()));
                    DataTable dt = Search(strsql);
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        model.BankID = dt.Rows[0]["Branch"].ToString();//分行別
                        model.BankName = dt.Rows[0]["BrchName"].ToString();//分行名稱
                        model.ClosedDate = dt.Rows[0]["CloseDate"].ToString();//結清日
                        model.CurBal = dt.Rows[0]["CurBal"].ToString();//目前餘額
                        model.CustId = dt.Rows[0]["CustId"].ToString();//被通報ID
                        model.DocNo = dt.Rows[0]["CUST_ID"].ToString();//案件編號
                        model.CustName = dt.Rows[0]["Name"].ToString();//被通報姓名
                        model.Currency = dt.Rows[0]["Currency"].ToString();//幣別
                        model.NotifyBal = dt.Rows[0]["CurBal"].ToString();//通報時餘額目前等於目前餘額
                        model.VD=dt.Rows[0]["AtmHoldAmt"].ToString();
                        model.MD=dt.Rows[0]["MdHoldAmt"].ToString();
                    }
                }
                else
                {
                    strErrorCodeAndMsg = strResult;
                }
            }
            
        }
        /// <summary>
        /// 發送電文9091
        /// </summary>
        public string Require9091(WarningDetails Wdetails,string option,string docno,string ccy,string code,string memo,string obligorno,string caseid,string strSetTime)
        {
            string strResult = "";
            //先通過案件編號查找被通報帳號和幣別
            string strsql = @"select * from WarningMaster where DocNo=@DocNo";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("@DocNo", docno));
            DataTable dt = Search(strsql);
            string strCustAccount = "";
            string strCurrency = "";
            if (dt!=null && dt.Rows.Count>0)
            {
                strCustAccount = dt.Rows[0]["CustAccount"].ToString();
                strCurrency = dt.Rows[0]["Currency"].ToString();
            }
            ExecuteHTG execHtg = new ExecuteHTG();
            strResult = execHtg.Send9091Or9093(strCustAccount,0,strCurrency,code,memo,obligorno,caseid,option,strSetTime);
            if (strResult != "")
            {
                string[] strlist = strResult.Split('|');
                if (strlist[0].Trim() == "0000")
                {
                    //如果電文回應成功，則去數據庫TX_00401取數據
                    strsql = @"select cCretDT from TX_09091 where Account like @Account and TrnNum=@TrnNum";
                    Parameter.Clear();
                    Parameter.Add(new CommandParameter("@Account", "%"+ strCustAccount+ "%"));
                    Parameter.Add(new CommandParameter("@TrnNum", strlist[1].Trim()));
                    DataTable dt1 = Search(strsql);
                    if (dt1 != null && dt1.Rows.Count > 0)
                    {
                        string strTime = dt1.Rows[0]["cCretDT"].ToString();
                        Wdetails.EtabsDatetime = UtlString.FormatDateTw(Convert.ToDateTime(strTime).ToString("yyyy/MM/dd"));
                        Wdetails.EtabsDatetimeHour = Convert.ToDateTime(strTime).ToString("HH:mm");
                    }
                }
            }
            return strResult;
        }
        public string Require9091ForRemove(WarningState wstate, string option, string docno, string ccy, string code, string memo, string obligorno, string caseid, string strSetTime)
        {
            string strResult = "";
            //先通過案件編號查找被通報帳號和幣別
            string strsql = @"select * from WarningMaster where DocNo=@DocNo";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("@DocNo", docno));
            DataTable dt = Search(strsql);
            string strCustAccount = "";
            string strCurrency = "";
            if (dt != null && dt.Rows.Count > 0)
            {
                strCustAccount = dt.Rows[0]["CustAccount"].ToString();
                strCurrency = dt.Rows[0]["Currency"].ToString();
            }
            ExecuteHTG execHtg = new ExecuteHTG();
            strResult = execHtg.Send9091Or9093(strCustAccount, 0, strCurrency, code, memo, obligorno, caseid, option);
            if (strResult != "")
            {
                string[] strlist = strResult.Split('|');
                if (strlist[0].Trim() == "0000")
                {
                    //如果電文回應成功，則去數據庫TX_00401取數據
                    strsql = @"select cCretDT from TX_09091 where Account like @Account and TrnNum=@TrnNum";
                    Parameter.Clear();
                    Parameter.Add(new CommandParameter("@Account", "%"+ strCustAccount+ "%"));
                    Parameter.Add(new CommandParameter("@TrnNum", strlist[1].Trim()));
                    DataTable dt1 = Search(strsql);
                    if (dt1 != null && dt1.Rows.Count > 0)
                    {
                        string strTime = dt1.Rows[0]["cCretDT"].ToString();
                        wstate.RelieveDate = UtlString.FormatDateTw(Convert.ToDateTime(strTime).ToString("yyyy/MM/dd"));
                    }
                }
            }
            return strResult;
        }
        /// <summary>
        /// 警示解除成功后拋查33401得到可用餘額
        /// </summary>
        /// <param name="CustAccount"></param>
        /// <param name="strErrorCodeAndMsg"></param>
        /// <returns></returns>
        public string  Require33401ForRemove(string DocNo, ref string strErrorCodeAndMsg)
        {
            string strResult = "";
            string strReleaseBal = "";//解除餘額
            string strsql = @"select * from WarningMaster where DocNo=@DocNo";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("@DocNo", DocNo));
            DataTable dt = Search(strsql);
            string strCustAccount = "";
            string strCurrency = "";
            if (dt != null && dt.Rows.Count > 0)
            {
                strCustAccount = dt.Rows[0]["CustAccount"].ToString();
                strCurrency = dt.Rows[0]["Currency"].ToString();
            }
            ExecuteHTG execHtg = new ExecuteHTG();
            strResult = execHtg.Send401("", "", strCustAccount);
            if (strResult != "")
            {
                string[] strlist = strResult.Split('|');
                if (strlist[0].Trim() == "0000")
                {
                    //如果電文回應成功，則去數據庫TX_00401取數據
                    strsql = @"select * from TX_33401 where Acct like @Acct and TrnNum=@TrnNum";
                    Parameter.Clear();
                    Parameter.Add(new CommandParameter("@Acct", "%"+strCustAccount+ "%"));
                    Parameter.Add(new CommandParameter("@TrnNum", strlist[1].Trim()));
                    DataTable dt1 = Search(strsql);
                    if (dt1 != null && dt1.Rows.Count > 0)
                    {
                        strReleaseBal = dt1.Rows[0]["trueAMT"].ToString();//得到可用餘額
                    }
                }
                else
                {
                    strErrorCodeAndMsg = strResult;
                }
            }
            return strReleaseBal;
        }
        public int WriteReleaseBal(string DocNo,string strReleaseBal)
        {
            string strsql = @"update WarningMaster set ReleaseBal=@ReleaseBal where DocNo=@DocNo";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("@DocNo", DocNo));
            Parameter.Add(new CommandParameter("@ReleaseBal", strReleaseBal));
            return ExecuteNonQuery(strsql);
        }
        public bool GenDetailESB(string DocNo, string CustId, string CustAccount, string ForCDateS, string ForCDateE,string strTrnNum)
        {
            bool _rowresult = false;
          //  log4net.Config.XmlConfigurator.Configure(new System.IO.FileInfo(AppDomain.CurrentDomain.BaseDirectory + @"\Log.config"));
            string IsnotTest ="0";
            
            
            DataTable dt = GetWarningHistorySend(DocNo,CustId,CustAccount,ForCDateS,ForCDateE,strTrnNum);
            if(dt !=null && dt.Rows.Count>0)
            {
                string requestXml = GenerateRequestXml(dt.Rows[0]);
                // 取得下行XML
                string bSendresult = SendESBData(dt.Rows[0]["TrnNum"].ToString(), requestXml, "ESB",IsnotTest);
                if(bSendresult!="")
                {
                    // 解析下行XML
                    _rowresult = AnalyticResponXMl(dt.Rows[0]["TrnNum"].ToString(), bSendresult);
                }
            }
            return _rowresult;
        }
        /// <summary>
        /// 根據案件編號、被通報ID、被通報帳號、調閱日期起訖得到警示歷史記錄
        /// </summary>
        /// <param name="DocNo"></param>
        /// <param name="CustId"></param>
        /// <param name="CustAccount"></param>
        /// <param name="ForCDateS"></param>
        /// <param name="ForCDateE"></param>
        /// <returns></returns>
        public DataTable GetWarningHistorySend(string DocNo, string CustId, string CustAccount, string ForCDateS, string ForCDateE,string strTrnNum)
        {
            string sqlSelect = @"select [NewID],[DocNo],[CustAccount], CONVERT(varchar(10), [ForCDateS], 111) as [ForCDateS], 
CONVERT(varchar(10), [ForCDateE], 111) as [ForCDateE],TrnNum,CustId from WarningQueryHistory where DocNo=@DocNo
and [CustAccount]=@CustAccount and [CustId]=@CustId and TrnNum=@TrnNum
and CONVERT(varchar(10), [ForCDateS], 111)=@ForCDateS and CONVERT(varchar(10), [ForCDateE], 111)=@ForCDateE";

            base.Parameter.Clear();
            Parameter.Add(new CommandParameter("@DocNo", DocNo));
            Parameter.Add(new CommandParameter("@CustAccount", CustAccount));
            Parameter.Add(new CommandParameter("@CustId", CustId));
            Parameter.Add(new CommandParameter("@ForCDateS", ForCDateS));
            Parameter.Add(new CommandParameter("@ForCDateE", ForCDateE));
            Parameter.Add(new CommandParameter("@TrnNum", strTrnNum));
            return base.Search(sqlSelect);
        }
        /// <summary>
        /// 組織上行XML
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private string GenerateRequestXml(DataRow data)
        {
            if (data == null) { return null; }

            // 需要上送的數據
            string strTrnNum = data["TrnNum"].ToString();
            string strIDNo = data["CustId"].ToString();
            string strAcctNo = data["CustAccount"].ToString();
            string strStart_Jnrst_Date = data["ForCDateS"].ToString();
            string strEnd_Jnrst_Date = data["ForCDateE"].ToString();

            string strHerader1 = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>";
            string strHerader2 = "<ns0:ServiceEnvelope xmlns:ns0=\"http://ns.chinatrust.com.tw/XSD/CTCB/ESB/Message/EMF/ServiceEnvelope\">";
            string strHerader3 = "<ns1:ServiceHeader xmlns:ns1=\"http://ns.chinatrust.com.tw/XSD/CTCB/ESB/Message/EMF/ServiceHeader\">";
            try
            {
                string strYear = Convert.ToInt16(DateTime.Now.Year - 1911).ToString("000");
                string strMonth = Convert.ToInt16(DateTime.Now.Month).ToString("00");
                string strDay = Convert.ToInt16(DateTime.Now.Day).ToString("00");
                string strHour = System.DateTime.Now.ToString("HHmmssfff");
                string strSno = "CSFS" + strYear.Substring(1, 2) + strMonth + strDay + strHour;
                string strSourceID = "CSFS";
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(strHerader1);
                sb.AppendLine(strHerader2);
                sb.AppendLine(strHerader3);
                sb.AppendLine("<ns1:StandardType>BSMF</ns1:StandardType>");
                sb.AppendLine("<ns1:StandardVersion>01</ns1:StandardVersion>");
                sb.AppendLine("<ns1:ServiceName>ceDemDepTrnCntInq</ns1:ServiceName>");
                sb.AppendLine("<ns1:ServiceVersion>01</ns1:ServiceVersion>");
                sb.AppendLine("<ns1:SourceID>" + strSourceID + "</ns1:SourceID>");
                sb.AppendLine("<ns1:TransactionID>" + strTrnNum + "</ns1:TransactionID>");
                sb.AppendLine("<ns1:RqTimestamp>" + System.DateTime.Now.ToString("yyyy-MM-dd") + "T" + System.DateTime.Now.ToString("HH:mm:ss.ff") + "+08:00" + "</ns1:RqTimestamp>");
                sb.AppendLine("</ns1:ServiceHeader>");
                sb.AppendLine("<ns1:ServiceBody xmlns:ns1=\"http://ns.chinatrust.com.tw/XSD/CTCB/ESB/Message/EMF/ServiceBody\">");
                sb.AppendLine("<ns2:ceDemDepTrnCntInqRq xmlns:ns2=\"http://ns.chinatrust.com.tw/XSD/CTCB/ESB/Message/BSMF/ceDemDepTrnCntInqRq/01\">");
                sb.AppendLine("<ns2:REQHDR>");
                sb.AppendLine("<ns2:TrnNum>" + strTrnNum + "</ns2:TrnNum>");
                sb.AppendLine("<ns2:TrnCode></ns2:TrnCode>");
                sb.AppendLine("</ns2:REQHDR><ns2:REQBDY>");
                sb.AppendLine("<ns2:CustId>" + strIDNo + "</ns2:CustId>");
                sb.AppendLine("<ns2:AcctNo>" + strAcctNo + "</ns2:AcctNo>");
                sb.AppendLine("<ns2:StartDate>" + strStart_Jnrst_Date + "</ns2:StartDate>");
                sb.AppendLine("<ns2:EndDate>" + strEnd_Jnrst_Date + "</ns2:EndDate>");
                sb.AppendLine("<ns2:Type>0</ns2:Type>");
                sb.AppendLine("<ns2:PromoCode>GS</ns2:PromoCode>");
                sb.AppendLine("</ns2:REQBDY>");
                sb.AppendLine("</ns2:ceDemDepTrnCntInqRq>");
                sb.AppendLine("</ns1:ServiceBody>");
                sb.AppendLine("</ns0:ServiceEnvelope>");

                return sb.ToString();
            }
            catch
            {
                return null;
            }
        }
        /// <summary>
        /// 將上行XML發送到主機得到下行數據XML
        /// </summary>
        /// <param name="TrnNum"></param>
        /// <param name="strXML"></param>
        /// <param name="txtCode"></param>
        /// <param name="IsnotTest"></param>
        /// <returns></returns>
        public string SendESBData(string TrnNum, string strXML, string txtCode,string IsnotTest)
        {
            string strResult = string.Empty;

            // 隊列有關設定
            string ServerUrl = string.Empty;
            string ServerPort = string.Empty;
            string UserName = string.Empty;
            string Password = string.Empty;
            string ESBSendQueueName = string.Empty;
            string ESBReceiveQueueName = string.Empty;
            string ServerPortStandBy = string.Empty;
            bool msgNull = false;

            try
            {
                //#region 將上行電文寫入到UXML UyyyyMMddhhmmss.xml

                //string path = new DirectoryInfo("~/").Parent.FullName + "\\UXML";
                //path = path.Replace("\\", "/");//Xml文件夾路徑
                //if (!Directory.Exists(path))
                //{
                //    Directory.CreateDirectory(path);//如果文件夾不存在則創建Xml目錄
                //}
                //path += "/" + txtCode + "U" + TrnNum + "_" + DateTime.Now.ToString("yyyyMMddhhmmssfff") + ".xml";
                //if (File.Exists(path))
                //{
                //    File.Delete(path);
                //}
                //using (StreamWriter sw = new StreamWriter(new FileStream(path, FileMode.Create)))
                //{
                //    sw.Write(strXML);
                //}
                //#endregion

                //// 記錄上行XML
                //WriteLog(txtCode + "\r\n" + strXML + "\r\n ------------------------------------------------------------\r\n\r\n");

                // 隊列有關設定
                //ServerUrl = ConfigurationManager.AppSettings["ServerUrl"].ToString();
                //ServerPort = ConfigurationManager.AppSettings["ServerPort"].ToString();
                //UserName = ConfigurationManager.AppSettings["UserName"].ToString();
                //Password = ConfigurationManager.AppSettings["Password"].ToString();
                //ESBSendQueueName = ConfigurationManager.AppSettings["ESBSendQueueName"].ToString();
                //ESBReceiveQueueName = ConfigurationManager.AppSettings["ESBReceiveQueueName"].ToString();
                //ServerPortStandBy = ConfigurationManager.AppSettings["ServerPortStandBy"].ToString();
                //從數據庫PARMCODE取主機參數
                IList<PARMCode> ESBServerList = GetCodeData("ESBSERVERCONFIG");
                if(ESBServerList !=null && ESBServerList.Count>0)
                {
                    foreach (PARMCode code in ESBServerList)
                    {
                        if(code.CodeNo== "ServerUrl")
                        {
                            ServerUrl = code.CodeDesc;
                        }
                        else if(code.CodeNo== "ServerPort")
                        {
                            ServerPort = code.CodeDesc;
                        }
                        else if(code.CodeNo == "UserName")
                        {
                            UserName = code.CodeDesc;
                        }
                        else if(code.CodeNo == "Password")
                        {
                            Password = code.CodeDesc;
                        }
                        else if(code.CodeNo== "ServerPortStandBy")
                        {
                            ServerPortStandBy = code.CodeDesc;
                        }
                        else if(code.CodeNo== "ESBSendQueueName")
                        {
                            ESBSendQueueName = code.CodeDesc;
                        }
                        else if(code.CodeNo== "ESBReceiveQueueName")
                        {
                            ESBReceiveQueueName = code.CodeDesc;
                        }
                    }
                    // 鏈接隊列
                    strResult = ConnESB(ServerUrl, ServerPort, UserName, Password, ESBSendQueueName, ESBReceiveQueueName, strXML, IsnotTest, ref msgNull);
                    //  若無回應，則再次呼叫
                    if (msgNull)
                    {
                        strResult = ConnESB(ServerUrl, ServerPortStandBy, UserName, Password, ESBSendQueueName, ESBReceiveQueueName, strXML, IsnotTest, ref msgNull);
                    }
                }
            }
            catch (Exception ex)
            {
                strResult = "";
            }
            //finally
            //{
            //    //記錄下行XML
            //    string path = new DirectoryInfo("~/").Parent.FullName + "\\DXML";

            //    path = path.Replace("\\", "/");//Xml文件夾路徑
            //    if (!Directory.Exists(path))
            //    {
            //        Directory.CreateDirectory(path);//如果文件夾不存在則創建Xml目錄
            //    }
            //    path += "/" + txtCode + "D" + TrnNum + "_" + DateTime.Now.ToString("yyyyMMddhhmmssfff") + ".xml";
            //    if (File.Exists(path))
            //    {
            //        File.Delete(path);
            //    }
            //    using (StreamWriter sw = new StreamWriter(new FileStream(path, FileMode.Create)))
            //    {
            //        sw.Write(strResult);
            //    }

            //    WriteLog(txtCode + "D" + "\r\n" + strResult + "\r\n ------------------------------------------------------------\r\n\r\n");
            //}

            return strResult;
        }
        public string ConnESB(string ServerUrl, string ServerPort, string UserName, string Password, string ESBSendQueueName, string ESBReceiveQueueName, string strXML,string IsnotTest, ref bool msgNull)
        {
            msgNull = false;
            string strResult = string.Empty;
            string _serverurl = string.Empty;
            string _messageid = string.Empty;
            int _TransactionTimeout = 30;

            strResult = "";
            #region //测试时不执行到这

            if (IsnotTest != "1")
            {
                _serverurl = "";
                _serverurl = "tcp://" + ServerUrl + ":" + ServerPort;
                /* 方法二,直接使用QueueConnectionFactory */
                QueueConnectionFactory factory = new TIBCO.EMS.QueueConnectionFactory(_serverurl);

                QueueConnection connection = factory.CreateQueueConnection(UserName, Password);

                QueueSession session = connection.CreateQueueSession(false, Session.AUTO_ACKNOWLEDGE);

                TIBCO.EMS.Queue queue = session.CreateQueue(ESBSendQueueName);

                QueueSender qsender = session.CreateSender(queue);

                /* send messages */
                TextMessage message = session.CreateTextMessage();
                message.Text = strXML;

                //一定要設定要reply的queue,這樣才收得到
                message.ReplyTo = (TIBCO.EMS.Destination)session.CreateQueue(ESBReceiveQueueName);

                qsender.Send(message);

                _messageid = message.MessageID;

                //receive message
                String messageselector = null;
                messageselector = "JMSCorrelationID = '" + _messageid + "'";

                TIBCO.EMS.Queue receivequeue = session.CreateQueue(ESBReceiveQueueName);

                QueueReceiver receiver = session.CreateReceiver(receivequeue, messageselector);

                connection.Start();

                //set up timeout 
                TIBCO.EMS.Message msg = receiver.Receive(_TransactionTimeout * 1000);

                if (msg == null)
                {
                    msgNull = true;
                    strResult = "";
                }
                else
                {
                    msg.Acknowledge();

                    if (msg is TextMessage)
                    {
                        TextMessage tm = (TextMessage)msg;
                        strResult = tm.Text;
                    }
                    else
                    {
                        strResult = msg.ToString();
                    }
                }
                connection.Close();
            }
            else
            {
                string tmpupfile = AppDomain.CurrentDomain.BaseDirectory + "交易明細發查_Recv.xml";
                strResult = File.ReadAllText(tmpupfile);
            }
            #endregion

            return strResult;
        }
        /// <summary>
        /// 解析下行數據XML
        /// </summary>
        /// <param name="strTrnNum"></param>
        /// <param name="xmlString"></param>
        /// <returns></returns>
        private bool AnalyticResponXMl(string strTrnNum,string xmlString)
        {
            bool _result = true;
            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(xmlString);

                // xml中帶xmlns:，使用SelectNodes或SelectSingleNode去查找其下节点，需要添加对应的XmlNamespaceManager参数，才可以的
                XmlNamespaceManager m = new XmlNamespaceManager(xmlDoc.NameTable);
                m.AddNamespace("ns0", "http://ns.chinatrust.com.tw/XSD/CTCB/ESB/Message/EMF/ServiceEnvelope");
                m.AddNamespace("ns1", "http://ns.chinatrust.com.tw/XSD/CTCB/ESB/Message/EMF/ServiceBody");
                m.AddNamespace("ns2", "http://ns.chinatrust.com.tw/XSD/CTCB/ESB/Message/BSMF/ceDemDepTrnCntInqRs/01");
                XmlNode node1 = xmlDoc.SelectSingleNode("ns0:ServiceEnvelope/ns1:ServiceBody/ns2:ceDemDepTrnCntInqRs/ns2:RESHDR/ns2:RspCode", m);

                // 發查結果
                string sRspCode = (node1 != null ? node1.InnerText : "");

                //  取得錯誤信息
                XmlNode oRspMsgNode = xmlDoc.SelectSingleNode("ns0:ServiceEnvelope/ns1:ServiceBody/ns2:ceDemDepTrnCntInqRs/ns2:RESHDR/ns2:RspMsg", m);

                string sRspMsg = (oRspMsgNode != null ? oRspMsgNode.InnerText : "");

                //  發查成功
                if (!string.IsNullOrEmpty(sRspCode) && (sRspCode == "0000" || sRspCode == "C001"))
                {

                    // 查無資料時，將狀態更新完02代表已解析完成，因為更新為01之後，還要要獲取excel的明細會在更新成02
                    if (sRspCode == "C001")
                    {
                        // 更新WarningQueryHistory，為成功
                        EditWarningQueryHistory(strTrnNum, "02", sRspCode, sRspMsg);
                    }
                    else
                    {
                        // WarningQueryHistory，為成功
                        EditWarningQueryHistory(strTrnNum, "01", sRspCode, sRspMsg);
                    }
                }
                else
                {

                    // WarningQueryHistory，為失敗
                    EditWarningQueryHistory(strTrnNum, "03", sRspCode, sRspMsg);
                }
            }
            catch (Exception ex)
            {
                _result = false;
            }

            return _result;
        }
        /// <summary>
        /// 更新警示歷史記錄表
        /// </summary>
        /// <param name="strTrnNum"></param>
        /// <param name="strESBStatus"></param>
        /// <param name="strRspCode"></param>
        /// <param name="strRspMsg"></param>
        /// <returns></returns>
        public bool EditWarningQueryHistory(string strTrnNum, string strESBStatus, string strRspCode, string strRspMsg)
        {
            string sql = @"UPDATE [WarningQueryHistory] 
                            SET 
                                [ESBStatus] = @ESBStatus,
                                [RspCode] = @RspCode,
                                [RspMsg] = @RspMsg,
                                [ModifiedDate] = GETDATE(), 
                                [ModifiedUser] = 'SYSTEM' 
                            WHERE TrnNum = @TrnNum";

            Parameter.Clear();
            Parameter.Add(new CommandParameter("@TrnNum", strTrnNum));
            Parameter.Add(new CommandParameter("@ESBStatus", strESBStatus));
            Parameter.Add(new CommandParameter("@RspCode", strRspCode));
            Parameter.Add(new CommandParameter("@RspMsg", strRspMsg));

            return ExecuteNonQuery(sql) > 0;
        }
        public MemoryStream ExcelForTransactionDetail(string NewID, string TrnNum)
        {
            try
            {
                var ms = new MemoryStream();

                IList<TransactionDetail> ilst = GetPrintDataForTransactionDetail(TrnNum);
                DataTable dt = GetHeaderDataForTranDetail(NewID);//獲取表頭數據
                string[] headerColumns = new[]
                       {
                        "交易日期",
                        "帳務日期",
                        "交易代號",
                        "交易時間",
                        "交易分行",
                        "交易櫃員",
                        "摘要",
                        "支出",
                        "存入",
                        "餘額",
                        "轉出入帳號",
                        "金姿序號",
                        "備註"
                    };
                if (ilst != null)
                {
                    ms = ExcelExportForTranDetail(ilst, dt, headerColumns,
                                                       delegate (HSSFRow dataRow, TransactionDetail dataItem)
                                                       {
                                                       //* 這裡可以針對每一個欄位做額外處理.比如日期
                                                       dataRow.CreateCell(0).SetCellValue(dataItem.JNRST_DATE);
                                                           dataRow.CreateCell(1).SetCellValue(dataItem.POST_DATE);
                                                           dataRow.CreateCell(2).SetCellValue(dataItem.TRANS_CODE);
                                                           dataRow.CreateCell(3).SetCellValue(dataItem.JNRST_TIME);
                                                           dataRow.CreateCell(4).SetCellValue(dataItem.TRAN_BRANCH);
                                                           dataRow.CreateCell(5).SetCellValue(dataItem.TELLER);
                                                           dataRow.CreateCell(6).SetCellValue(dataItem.TXN_DESC);
                                                           dataRow.CreateCell(7).SetCellValue(dataItem.TRAN_AMT);
                                                           dataRow.CreateCell(8).SetCellValue(dataItem.TRAN_AMT);
                                                           dataRow.CreateCell(9).SetCellValue(dataItem.BALANCE);
                                                           dataRow.CreateCell(10).SetCellValue(dataItem.TRF_ACCT);
                                                           dataRow.CreateCell(11).SetCellValue(dataItem.FISC_SEQNO);
                                                           dataRow.CreateCell(12).SetCellValue(dataItem.NARRATIVE);
                                                       });
                }
                return ms;
            }
            catch(Exception ex)
            {
                return null;
            }
        }
        public IList<TransactionDetail> GetPrintDataForTransactionDetail(string TrnNum)
        {
            try
            {
                string strSql = @"select * from TransactionDetail where TrnNum=@TrnNum";
                Parameter.Clear();
                Parameter.Add(new CommandParameter("@TrnNum", TrnNum));
                IList<TransactionDetail> ilst = SearchList<TransactionDetail>(strSql);
                if (ilst != null)
                {
                    return ilst;
                }
                return new List<TransactionDetail>();
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }
        public DataTable GetHeaderDataForTranDetail(string strNewID)
        {
            try
            {
                string strsql = @"select a.[CustAccount],b.CustName,b.Currency, CONVERT(varchar(10), [ForCDateS], 111) as [ForCDateS], CONVERT(varchar(10), [ForCDateE], 111) as [ForCDateE]
 from WarningQueryHistory a inner join WarningMaster b on a.DocNo=b.DocNo
 and a.[NewID]=@NewID";
                Parameter.Clear();
                Parameter.Add(new CommandParameter("@NewID", strNewID));
                return base.Search(strsql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public  MemoryStream ExcelExportForTranDetail<TType>(IList<TType> list,DataTable dt, string[] headerColumns,
                                                     Action<HSSFRow, TType> setExcelRow)
        {
            return ExcelExportForDetail(new HSSFWorkbook(), list,dt, headerColumns, setExcelRow);
        }


        /// <summary>
        /// excel export.
        /// </summary>
        /// <typeparam name="TType"></typeparam>
        /// <param name="workbook"></param>
        /// <param name="list"></param>
        /// <param name="headerColumns"></param>
        /// <param name="setExcelRow"></param>
        /// <returns></returns>
        public  MemoryStream ExcelExportForDetail<TType>(HSSFWorkbook workbook, IList<TType> list,DataTable dt, string[] headerColumns, Action<HSSFRow, TType> setExcelRow)
        {
            var ms = new MemoryStream();

            #region set column style
            var headerStyle = workbook.CreateCellStyle() as HSSFCellStyle;
            var fonts = workbook.CreateFont() as HSSFFont;
            fonts.Boldweight = (short)NPOI.SS.UserModel.FontBoldWeight.Bold;
            headerStyle.SetFont(fonts);
            #endregion

            int iPage = 0;
            int iTotal = 0;
            int pageSize = 65534;

            if (!list.Any())
            {
                string strCustName = "";
                string strCustAccount = "";
                string strProductType = "";
                string strCurrency = "";
                string strForCDateS = "";
                string strForCDateE = "";
                if (dt.Rows.Count>0)
                {
                    strCustName = dt.Rows[0]["CustName"].ToString().Trim();
                    strCustAccount = dt.Rows[0]["CustAccount"].ToString().Trim();
                    strCurrency = dt.Rows[0]["Currency"].ToString().Trim();
                    strForCDateS = string.IsNullOrEmpty(dt.Rows[0]["ForCDateS"].ToString()) ? "" : UtlString.FormatDateTw(dt.Rows[0]["ForCDateS"].ToString().Trim());
                    strForCDateE = string.IsNullOrEmpty(dt.Rows[0]["ForCDateE"].ToString()) ? "" : UtlString.FormatDateTw(dt.Rows[0]["ForCDateE"].ToString().Trim());
                }
                var sheet = workbook.CreateSheet() as HSSFSheet;
                var dataHeaderRow1 = sheet.CreateRow(0) as HSSFRow;
                dataHeaderRow1.CreateCell(0).SetCellValue("戶名");
                dataHeaderRow1.GetCell(0).CellStyle = headerStyle;
                dataHeaderRow1.CreateCell(1).SetCellValue(strCustName);//待確認
                var dataHeaderRow2 = sheet.CreateRow(1) as HSSFRow;
                dataHeaderRow2.CreateCell(0).SetCellValue("帳號");
                dataHeaderRow2.GetCell(0).CellStyle = headerStyle;
                dataHeaderRow2.CreateCell(1).SetCellValue(strCustAccount);//待確認
                dataHeaderRow2.CreateCell(3).SetCellValue("產品別");
                dataHeaderRow2.GetCell(3).CellStyle = headerStyle;
                dataHeaderRow2.CreateCell(4).SetCellValue(strProductType);
                dataHeaderRow2.CreateCell(5).SetCellValue("幣別");
                dataHeaderRow2.GetCell(5).CellStyle = headerStyle;
                dataHeaderRow2.CreateCell(6).SetCellValue(strCurrency);
                dataHeaderRow2.CreateCell(8).SetCellValue("查詢起日");
                dataHeaderRow2.GetCell(8).CellStyle = headerStyle;
                dataHeaderRow2.CreateCell(9).SetCellValue(strForCDateS);
                dataHeaderRow2.CreateCell(10).SetCellValue("查詢訖日");
                dataHeaderRow2.GetCell(10).CellStyle = headerStyle;
                dataHeaderRow2.CreateCell(11).SetCellValue(strForCDateE);

                var dataRow = sheet.CreateRow(2) as HSSFRow;
                //set header
                for (var j = 0; j < headerColumns.Length; j++)
                {
                    dataRow.CreateCell(j).SetCellValue(headerColumns[j]);
                    dataRow.GetCell(j).CellStyle = headerStyle;
                }
            }
            else
            {
                while (iTotal < list.Count)
                {
                    iTotal = iPage * pageSize;
                    int iCurrentCount = 0;
                    if (list.Count - iTotal > pageSize)
                    {
                        iCurrentCount = pageSize;
                    }
                    else
                    {
                        iCurrentCount = list.Count - iTotal;

                    }

                    var sheet = workbook.CreateSheet() as HSSFSheet;
                    var dataHeaderRow1 = sheet.CreateRow(0) as HSSFRow;
                    dataHeaderRow1.CreateCell(0).SetCellValue("戶名");
                    dataHeaderRow1.GetCell(0).CellStyle = headerStyle;
                    dataHeaderRow1.CreateCell(1).SetCellValue("");//待確認
                    var dataHeaderRow2 = sheet.CreateRow(1) as HSSFRow;
                    dataHeaderRow2.CreateCell(0).SetCellValue("帳號");
                    dataHeaderRow2.GetCell(0).CellStyle = headerStyle;
                    dataHeaderRow2.CreateCell(1).SetCellValue("");//待確認
                    dataHeaderRow2.CreateCell(3).SetCellValue("產品別");
                    dataHeaderRow2.GetCell(3).CellStyle = headerStyle;
                    dataHeaderRow2.CreateCell(4).SetCellValue("");
                    dataHeaderRow2.CreateCell(5).SetCellValue("幣別");
                    dataHeaderRow2.GetCell(5).CellStyle = headerStyle;
                    dataHeaderRow2.CreateCell(6).SetCellValue("");
                    dataHeaderRow2.CreateCell(8).SetCellValue("查詢起日");
                    dataHeaderRow2.GetCell(8).CellStyle = headerStyle;
                    dataHeaderRow2.CreateCell(9).SetCellValue("");
                    dataHeaderRow2.CreateCell(10).SetCellValue("查詢訖日");
                    dataHeaderRow2.GetCell(10).CellStyle = headerStyle;
                    dataHeaderRow2.CreateCell(11).SetCellValue("");
                    var dataRow = sheet.CreateRow(2) as HSSFRow;
                    //set header
                    for (var j = 0; j < headerColumns.Length; j++)
                    {
                        dataRow.CreateCell(j).SetCellValue(headerColumns[j]);
                        dataRow.GetCell(j).CellStyle = headerStyle;
                    }

                    for (var i = iTotal; i < iTotal + iCurrentCount; i++)
                    {
                        //*set row data
                        dataRow = (HSSFRow)sheet.CreateRow(i - iTotal + 3);
                        setExcelRow(dataRow, list[i]);

                    }
                    sheet = null;
                    iTotal += iCurrentCount;
                    iPage++;
                }
            }

            workbook.Write(ms);
            ms.Flush();
            ms.Position = 0;
            workbook = null;
            return ms;
        }
    }
}
