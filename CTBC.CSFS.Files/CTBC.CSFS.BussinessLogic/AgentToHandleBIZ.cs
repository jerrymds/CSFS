using CTBC.CSFS.Models;
using CTBC.CSFS.Pattern;
using CTBC.FrameWork.Util;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CTBC.CSFS.ViewModels;

namespace CTBC.CSFS.BussinessLogic
{
    public class AgentToHandleBIZ : CommonBIZ
    {
        public AgentToHandleBIZ(AppController appController)
            : base(appController)
        { }

        public AgentToHandleBIZ()
        { }
        /// <summary>
        /// 查詢出經辦需要處理的案件
        /// </summary>
        /// <param name="AToH"></param>
        /// <param name="pageIndex"></param>
        /// <param name="strSortExpression"></param>
        /// <param name="strSortDirection"></param>
        /// <param name="EmpId"></param>
        /// <returns></returns>
        public IList<AgentToHandle> GetQueryList(AgentToHandle AToH, int pageIndex, string strSortExpression, string strSortDirection, string EmpId)
        {
            try
            {
                base.PageIndex = pageIndex;
                string sqlStr = "";
                string sqlWhere = "";
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@pageS", (base.PageSize * (base.PageIndex - 1)) + 1));
                base.Parameter.Add(new CommandParameter("@pageE", base.PageSize * base.PageIndex));
                base.Parameter.Add(new CommandParameter("@EmpId", EmpId));

                if (!string.IsNullOrEmpty(AToH.CaseNo))
                {
                    sqlWhere += @" and (B.CaseId in (select CaseId from CaseNoChangeHistory where  OldCaseNo like @CaseNo) or CaseNo like @CaseNo) ";
                    base.Parameter.Add(new CommandParameter("@CaseNo", "%" + AToH.CaseNo.Trim() + "%"));
                }
                if (!string.IsNullOrEmpty(AToH.GovKind))
                {
                    sqlWhere += @" and GovKind like @GovKind ";
                    base.Parameter.Add(new CommandParameter("@GovKind", "%" + AToH.GovKind.Trim() + "%"));
                }
                if (!string.IsNullOrEmpty(AToH.GovUnit))
                {
                    sqlWhere += @" and GovUnit like @GovUnit ";
                    base.Parameter.Add(new CommandParameter("@GovUnit", "%" + AToH.GovUnit.Trim() + "%"));
                }
                if (!string.IsNullOrEmpty(AToH.GovNo))
                {
                    sqlWhere += @" and GovNo like @GovNo ";
                    base.Parameter.Add(new CommandParameter("@GovNo", "%" + AToH.GovNo.Trim() + "%"));
                }
                if (!string.IsNullOrEmpty(AToH.Person))
                {
                    sqlWhere += @" and Person like @Person ";
                    base.Parameter.Add(new CommandParameter("@Person", "%" + AToH.Person.Trim() + "%"));
                }
                if (!string.IsNullOrEmpty(AToH.Speed))
                {
                    sqlWhere += @" and Speed like @Speed ";
                    base.Parameter.Add(new CommandParameter("@Speed", "" + AToH.Speed.Trim() + ""));
                }
                if (!string.IsNullOrEmpty(AToH.ReceiveKind))
                {
                    sqlWhere += @" and ReceiveKind like @ReceiveKind ";
                    base.Parameter.Add(new CommandParameter("@ReceiveKind", "" + AToH.ReceiveKind.Trim() + ""));
                }
                if (!string.IsNullOrEmpty(AToH.CaseKind))
                {
                    sqlWhere += @" and CaseKind like @CaseKind ";
                    base.Parameter.Add(new CommandParameter("@CaseKind", "" + AToH.CaseKind.Trim() + ""));
                }
                if (!string.IsNullOrEmpty(AToH.CaseKind2))
                {
                    sqlWhere += @" and CaseKind2 like @CaseKind2 ";
                    base.Parameter.Add(new CommandParameter("@CaseKind2", "" + AToH.CaseKind2.Trim() + ""));
                }
                if (!string.IsNullOrEmpty(AToH.GovDateS))
                {
                    sqlWhere += @" AND GovDate >= @GovDateS";
                    base.Parameter.Add(new CommandParameter("@GovDateS", AToH.GovDateS));
                }
                if (!string.IsNullOrEmpty(AToH.GovDateE))
                {
                    string GovDateE = UtlString.FormatDateString(Convert.ToDateTime(AToH.GovDateE.Replace('/', ' ').ToString()).AddDays(1).ToString("yyyyMMdd"));
                    sqlWhere += @" AND GovDate < @GovDateE ";
                    base.Parameter.Add(new CommandParameter("@GovDateE", GovDateE));
                }
                if (!string.IsNullOrEmpty(AToH.Unit))
                {
                    sqlWhere += @" and Unit like @Unit ";
                    base.Parameter.Add(new CommandParameter("@Unit", "%" + AToH.Unit.Trim() + "%"));
                }
                if (!string.IsNullOrEmpty(AToH.CreatedDateS))
                {
                    sqlWhere += @" AND B.CreatedDate >= @CreatedDateS";
                    base.Parameter.Add(new CommandParameter("@CreatedDateS", AToH.CreatedDateS));
                }
                if (!string.IsNullOrEmpty(AToH.CreatedDateE))
                {
                    string CreatedDateE = UtlString.FormatDateString(Convert.ToDateTime(AToH.CreatedDateE.Replace('/', ' ').ToString()).AddDays(1).ToString("yyyyMMdd"));
                    sqlWhere += @" AND B.CreatedDate < @CreatedDateE ";
                    base.Parameter.Add(new CommandParameter("@CreatedDateE", CreatedDateE));
                }
                sqlStr += @" select row_number() over (partition by CaseId order by caseid ,MemoDate desc ) pid ,MemoDate,CaseId  into #memo from CaseMemo where  MemoType='CaseSeizure'
                            ;with T1 
	                        as
	                        (
		                       SELECT B.CaseId,GovUnit,(SELECT CONVERT(varchar(100), GovDate, 111)) as GovDate,ReceiverNo,Speed,CaseKind,PropertyDeclaration,
		                       CaseKind2,GovNo,Unit,Person,CaseNo,
							   (SELECT CONVERT(varchar(100), LimitDate, 111)) as LimitDate,
                               ReceiveKind,Status,(SELECT CONVERT(varchar(100), B.CreatedDate, 111)) as CreatedDate,MemoDate,ReturnReason,OverDueMemo
                               FROM [CaseMaster] AS B
                               left join #memo as memo on b.CaseId=memo.CaseId and pid = 1
			                   WHERE B.[isDelete] = 0 AND [AgentUser] = @EmpId --AND ISNULL(B.[AfterSeizureApproved],0) = 0 
                                    AND ([Status] = @Status1 OR [Status] = @Status2 OR [Status] = @Status3 OR [Status] = @Status4 OR [Status] = @Status5 OR [Status] = @Status6) " + sqlWhere + @"
	                        ),T2 as
	                        (
		                        select *, row_number() over (order by   
                                    CaseKind DESC,
                                    CASE CaseKind2 WHEN '扣押' THEN '1' WHEN '扣押並支付' THEN '2' WHEN '撤銷' THEN '3' WHEN '支付' THEN '4' ELSE '' END,
                                    CASE WHEN CaseKind = '扣押案件' AND ISNULL(MemoDate,'') = '' THEN 0 ELSE 1 END ASC,
                                    CaseNo  DESC) RowNum
		                        from T1
	                        ),T3 as 
	                        (
		                        select *,(select max(RowNum) from T2) maxnum from T2 
		                        where rownum between @pageS and @pageE
	                        )
	                        select a.* from T3 a 
                            drop table #memo ";
                /*
                sqlStr += @";with T1 
	                        as
	                        (
		                       SELECT B.CaseId,GovUnit,(SELECT CONVERT(varchar(100), GovDate, 111)) as GovDate,ReceiverNo,Speed,CaseKind,
		                       CaseKind2,GovNo,Unit,Person,CaseNo,
							   (SELECT CONVERT(varchar(100), LimitDate, 111)) as LimitDate,
                               ReceiveKind,Status,(SELECT CONVERT(varchar(100), B.CreatedDate, 111)) as CreatedDate,MemoDate,ReturnReason,OverDueMemo
                               FROM [CaseMaster] AS B
                               left join ( select top 1 MemoDate,CaseId from CaseMemo where  MemoType='CaseSeizure' ) as memo on b.CaseId=memo.CaseId 
			                   WHERE B.[isDelete] = 0 AND [AgentUser] = @EmpId AND ISNULL(B.[AfterSeizureApproved],0) = 0 
                                    AND ([Status] = @Status1 OR [Status] = @Status2 OR [Status] = @Status3 OR [Status] = @Status4 OR [Status] = @Status5 OR [Status] = @Status6) " + sqlWhere + @"
	                        ),T2 as
	                        (
		                        select *, row_number() over (order by   
                                    CaseKind DESC,
                                    CASE CaseKind2 WHEN '扣押' THEN '1' WHEN '扣押並支付' THEN '2' WHEN '撤銷' THEN '3' WHEN '支付' THEN '4' ELSE '' END,
                                    CASE WHEN CaseKind = '扣押案件' AND ISNULL(MemoDate,'') = '' THEN 0 ELSE 1 END ASC,
                                    CaseNo  DESC) RowNum
		                        from T1
	                        ),T3 as 
	                        (
		                        select *,(select max(RowNum) from T2) maxnum from T2 
		                        where rownum between @pageS and @pageE
	                        )
	                        select a.* from T3 a ";
                */
                //* 只能看到 集作分派,經辦修改,經辦改派,主管退件的案子
                Parameter.Add(new CommandParameter("Status1", CaseStatus.CollectionSubmit));
                Parameter.Add(new CommandParameter("Status2", CaseStatus.AgentReassign));
                Parameter.Add(new CommandParameter("Status3", CaseStatus.AgentEdit));
                Parameter.Add(new CommandParameter("Status4", CaseStatus.DirectorReturn));
                Parameter.Add(new CommandParameter("Status5", CaseStatus.CollectionToAgent));
                Parameter.Add(new CommandParameter("Status6", CaseStatus.DirectorReassign));

                IList<AgentToHandle> _ilsit = base.SearchList<AgentToHandle>(sqlStr);

                if (_ilsit != null)
                {
                    if (_ilsit.Count > 0)
                    {
                        var list = GetCodeData("STATUS_NAME");
                        foreach (AgentToHandle item in _ilsit)
                        {
                            var obj = list.FirstOrDefault(a => a.CodeNo == item.Status);
                            if (obj != null)
                                item.StatusShow = obj.CodeDesc;
                            else
                                item.StatusShow = item.Status;
                        }
                        base.DataRecords = _ilsit[0].maxnum;
                    }
                    else
                    {
                        base.DataRecords = 0;
                        _ilsit = new List<AgentToHandle>();
                    }
                    return _ilsit;
                }
                else
                {
                    return new List<AgentToHandle>();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        //退件結案
        public int ReturnClose(AgentToHandle model, List<Guid> listId, string userId)
        {
            int rtn = 0;
            IDbConnection dbConnection = OpenConnection();
            IDbTransaction dbTransaction = null;
            CaseHistoryBIZ history = new CaseHistoryBIZ();
            try
            {
                using (dbConnection)
                {
                    dbTransaction = dbConnection.BeginTransaction();
                    foreach (Guid id in listId)
                    {
                        string sqlStr = @"UPDATE [CaseMaster] SET 
                                                [Status] = @Status, 
                                                [ReturnReason] = @ReturnReason, 
                                                [ModifiedUser] = @ModifiedUser,
                                                [ModifiedDate] = GETDATE()
                                        WHERE [CaseId] = @CaseId";
                        history.insertCaseHistory(id, CaseStatus.AgentReturnClose, userId, dbTransaction);
                        base.Parameter.Clear();
                        base.Parameter.Add(new CommandParameter("CaseId", id));
                        base.Parameter.Add(new CommandParameter("Status", CaseStatus.AgentReturnClose));
                        base.Parameter.Add(new CommandParameter("ModifiedUser", userId));
                        base.Parameter.Add(new CommandParameter("ReturnReason", model.ReturnReason));
                        base.ExecuteNonQuery(sqlStr, dbTransaction);

                        #region 賬務資訊
                        new CaseAccountBiz().DeleteSeizureSetting(id, dbTransaction);//扣押
                        new CaseAccountBiz().DeleteCAENalByCaseID(id, dbTransaction);//外來文
                        new CasePayeeSettingBIZ().DeleteCasePayeeSetting(id, dbTransaction);//受款人
                        #endregion

                        #region  會辦咨詢
                        new CaseMeetBiz().DeleteCaseMeetMaster(id, dbTransaction);
                        new CaseMeetBiz().DeleteCaseMeetDetails(id, dbTransaction);
                        #endregion

                        #region  資訊部調閱
                        new AgentDepartmentAccessBIZ().DeleteDepartmentAccess(id, dbTransaction);
                        #endregion

                        #region  發文資訊
                        new CaseSendSettingBIZ().DeleteCaseSendSetting(id, dbTransaction);
                        new CaseSendSettingBIZ().DeleteCaseSendSettingDetails(id, dbTransaction);
                        #endregion

                        #region  正本備查
                        new LendDataBIZ().DeleteLendData(id, dbTransaction);
                        new LendDataBIZ().DeleteLendDataAttatchment(id, dbTransaction);
                        new LendDataBIZ().UpdateReturnCaseId(id, dbTransaction);
                        #endregion

                        #region  利息計算
                        new CaseCalculatorDetailsBIZ().DeleteCaseCalculatorMain(id, dbTransaction);
                        new CaseCalculatorDetailsBIZ().DeleteCaseCalculatorDetails(id, dbTransaction);
                        #endregion

                        #region  內部註記
                        CaseMemo casememo = new CaseMemo();
                        casememo.CaseId = id;
                        casememo.MemoType = "CaseMemo";
                        new CaseMemoBiz().Delete(casememo, dbTransaction);
                        #endregion
                    }
                    dbTransaction.Commit();
                }
                return 1;
            }
            catch (Exception ex)
            {
                try
                {
                    dbTransaction.Rollback();
                    return 0;
                }
                catch (Exception ex2)
                {
                }
                throw ex;
            }
        }

        /// <summary>
        /// 集作改派
        /// </summary>
        /// <param name="aryCaseId"></param>
        /// <param name="aryAgentId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public JsonReturn AgentReassign(List<Guid> aryCaseId, List<string> aryAgentId, string userId)
        {
            if (aryCaseId == null || !aryCaseId.Any() || aryAgentId == null || !aryAgentId.Any()) return new JsonReturn() { ReturnCode = "0" };

            bool bResult = true;
            DateTime dtNow = GetNowDateTime();
            IDbConnection dbConnection = OpenConnection();
            IDbTransaction dbTransaction = null;
            CaseMasterBIZ masterBiz = new CaseMasterBIZ();
            LdapEmployeeBiz agentBiz = new LdapEmployeeBiz();
            CaseHistoryBIZ historyBiz = new CaseHistoryBIZ();
            try
            {
                using (dbConnection)
                {
                    dbTransaction = dbConnection.BeginTransaction();
                    for (int i = 0; i < aryCaseId.Count; i++)
                    {
                        int iAgentNo = i % aryAgentId.Count;
                        string empId = aryAgentId[iAgentNo];
                        LDAPEmployee agent = agentBiz.GetAllEmployeeInEmployeeViewByEmpId(empId);
                        CaseMaster master = masterBiz.MasterModel(aryCaseId[i]);
                        string strCaseNo = master.CaseNo;
                        //* 刪除舊
                        string strSql = @" UPDATE [CaseAssignTable] SET [AlreadyAssign] = 2, [ModifiedUser] = @ModifiedUser, [ModifiedDate] = GETDATE() WHERE [CaseId] = @CaseId AND [AlreadyAssign] <> 2 ;";
                        Parameter.Clear();
                        Parameter.Add(new CommandParameter("@CaseId", aryCaseId[i]));
                        Parameter.Add(new CommandParameter("@ModifiedUser", userId));
                        ExecuteNonQuery(strSql, dbTransaction);
                        //* 插入新
                        strSql = @"INSERT INTO [CaseAssignTable] ([CaseId],[EmpId],[AlreadyAssign],[CreatdUser],[CreatedDate])VALUES (@CaseId,@EmpId,0,@UserId,@CreateDate);";
                        Parameter.Clear();
                        Parameter.Add(new CommandParameter("@CaseId", aryCaseId[i]));
                        Parameter.Add(new CommandParameter("@EmpId", empId));
                        Parameter.Add(new CommandParameter("@UserId", userId));
                        Parameter.Add(new CommandParameter("@CreateDate", dtNow));
                        Parameter.Add(new CommandParameter("@ModifiedUser", userId));
                        bResult = bResult && ExecuteNonQuery(strSql, dbTransaction) > 0;
                        //* 更新Master
                        strSql = @"UPDATE [CaseMaster] 
                                    SET [AgentUser] = @AgentUser,
	                                    [AgentBranchId] = @AgentBranchId,
	                                    [AgentDeptId] = @AgentDeptId,
                                        [AgentSection] = @AgentSection,
	                                    [Status] = @Status,
	                                    [ModifiedUser] = @ModifiedUser,
	                                    [ModifiedDate] = GETDATE()
                                    WHERE [CaseId] = @CaseId";
                        Parameter.Clear();
                        Parameter.Add(new CommandParameter("CaseId", aryCaseId[i]));
                        Parameter.Add(new CommandParameter("AgentUser", empId));
                        Parameter.Add(new CommandParameter("AgentBranchId", agent == null ? "" : agent.BranchId));
                        Parameter.Add(new CommandParameter("AgentDeptId", agent == null ? "" : agent.DepId));
                        Parameter.Add(new CommandParameter("AgentSection", agent == null ? "" : agent.SectionName));
                        Parameter.Add(new CommandParameter("Status", CaseStatus.AgentReassign));
                        Parameter.Add(new CommandParameter("ModifiedUser", userId));
                        bResult = bResult && ExecuteNonQuery(strSql, dbTransaction) > 0;

                        if (master.CaseKind == "外來文案件")
                        {
                            //*	外來文案件: C+民國年7碼+KXX (K:0~7 一科，K:9 二科，K:8 三科) (於集作收件派案後，系統自動編列)
                            string type = agent == null ? "C"
                                                        : agent.SectionName.Contains("一") ? "C1"
                                                        : agent.SectionName.Contains("二") ? "C2"
                                                        : agent.SectionName.Contains("三") ? "C3"
                                                        : "C";
                            if (strCaseNo.Substring(0, 2) != type)
                            {
                                string caseNo = new CaseNoTableBIZ().GetCaseNo(type, dbTransaction);
                                if (strCaseNo.Substring(0, 2) == caseNo)
                                    strSql = "UPDATE [CaseMaster] SET [CaseNo] = @CaseNo WHERE [CaseId] = @CaseId";
                                Parameter.Clear();
                                Parameter.Add(new CommandParameter("CaseId", aryCaseId[i]));
                                Parameter.Add(new CommandParameter("CaseNo", caseNo));
                                bResult = bResult && ExecuteNonQuery(strSql, dbTransaction) > 0;
                            }
                        }
                        //* 寫History
                        bResult = bResult && historyBiz.insertCaseHistory(aryCaseId[i], CaseStatus.AgentReassign, userId, dbTransaction);
                    }
                    if (bResult)
                    {
                        dbTransaction.Commit();
                        return new JsonReturn() { ReturnCode = "1" };
                    }
                    dbTransaction.Rollback();
                    return new JsonReturn() { ReturnCode = "0" };
                }
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
                throw;
            }
        }
        public JsonReturn AssignSet(AgentToHandle model, List<Guid> aryCaseId, string userId)
        {
            if (aryCaseId == null || !aryCaseId.Any()) return new JsonReturn() { ReturnCode = "0" };

            bool bResult = true;
            DateTime dtNow = GetNowDateTime();
            IDbConnection dbConnection = OpenConnection();
            IDbTransaction dbTransaction = null;
            CaseMasterBIZ masterBiz = new CaseMasterBIZ();
            LdapEmployeeBiz agentBiz = new LdapEmployeeBiz();
            CaseHistoryBIZ historyBiz = new CaseHistoryBIZ();
            try
            {
                using (dbConnection)
                {
                    dbTransaction = dbConnection.BeginTransaction();
                    for (int i = 0; i < aryCaseId.Count; i++)
                    {
                        //int iAgentNo = i % aryAgentId.Count;
                        //string empId = aryAgentId[iAgentNo];
                        //LDAPEmployee agent = agentBiz.GetAllEmployeeInEmployeeViewByEmpId(empId);
                        //CaseMaster master = masterBiz.MasterModel(aryCaseId[i]);

                        //* 刪除舊
                        //string strSql = @" UPDATE [CaseAssignTable] SET [AlreadyAssign] = 2, [ModifiedUser] = @ModifiedUser, [ModifiedDate] = GETDATE() WHERE [CaseId] = @CaseId AND [AlreadyAssign] <> 2 ;";
                        //Parameter.Clear();
                        //Parameter.Add(new CommandParameter("@CaseId", aryCaseId[i]));
                        //Parameter.Add(new CommandParameter("@ModifiedUser", userId));
                        //ExecuteNonQuery(strSql, dbTransaction);
                        //* 插入新
                        //strSql = @"INSERT INTO [CaseAssignTable] ([CaseId],[AlreadyAssign],[CreatdUser],[CreatedDate])VALUES (@CaseId,0,@UserId,@CreateDate);";
                        //Parameter.Clear();
                        //Parameter.Add(new CommandParameter("@CaseId", aryCaseId[i]));
                        ////Parameter.Add(new CommandParameter("@EmpId", empId));
                        //Parameter.Add(new CommandParameter("@UserId", userId));
                        //Parameter.Add(new CommandParameter("@CreateDate", dtNow));
                        //Parameter.Add(new CommandParameter("@ModifiedUser", userId));
                        //bResult = bResult && ExecuteNonQuery(strSql, dbTransaction) > 0;
                        //* 更新Master
                        string strSql = @"UPDATE [CaseMaster] 
                                    SET [Status] = @Status,
                                        OverDueMemo=@OverDueMemo,
	                                    [ModifiedUser] = @ModifiedUser,
	                                    [ModifiedDate] = GETDATE()
                                    WHERE [CaseId] = @CaseId";
                        Parameter.Clear();
                        Parameter.Add(new CommandParameter("CaseId", aryCaseId[i]));
                        Parameter.Add(new CommandParameter("Status", CaseStatus.AgentToCollection));
                        base.Parameter.Add(new CommandParameter("OverDueMemo", model.OverDueMemo));
                        Parameter.Add(new CommandParameter("ModifiedUser", userId));
                        bResult = bResult && ExecuteNonQuery(strSql, dbTransaction) > 0;
                        //* 寫History
                        bResult = bResult && historyBiz.insertCaseHistory(aryCaseId[i], CaseStatus.AgentToCollection, userId, dbTransaction);
                    }
                    if (bResult)
                    {
                        dbTransaction.Commit();
                        return new JsonReturn() { ReturnCode = "1" };
                    }
                    dbTransaction.Rollback();
                    return new JsonReturn() { ReturnCode = "0" };
                }
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
                throw;
            }
        }

        #region 呈核
        public string GetManagerID(string userId)
        {
            try
            {
                string strSql = @"SELECT ManagerID FROM LdapEmployee where EmpID=@EmpID";
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@EmpID", userId));
                return base.ExecuteScalar(strSql) != null ? base.ExecuteScalar(strSql).ToString() : "";
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        /// <summary>
        /// 經辦-呈核
        /// </summary>
        /// <param name="aryCaseId"></param>
        /// <param name="aryAgentId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public JsonReturn AgentSubmit(List<Guid> aryCaseId, List<string> aryAgentId, string userId)
        {
            if (aryCaseId == null || !aryCaseId.Any() || aryAgentId == null || !aryAgentId.Any()) return new JsonReturn() { ReturnCode = "0" };

            DateTime dtNow = GetNowDateTime();
            IDbConnection dbConnection = OpenConnection();
            IDbTransaction dbTransaction = null;
            CaseMasterBIZ master = new CaseMasterBIZ();
            bool bFlag = true;
            try
            {
                using (dbConnection)
                {
                    dbTransaction = dbConnection.BeginTransaction();
                    string strSql = string.Empty;
                    foreach (Guid caseId in aryCaseId)
                    {
                        strSql = @" UPDATE [CaseAssignTable] SET [AlreadyAssign] = 2, [ModifiedUser] = @ModifiedUser, [ModifiedDate] = GETDATE() WHERE [CaseId] = @CaseId;";
                        Parameter.Clear();
                        Parameter.Add(new CommandParameter("CaseId", caseId));
                        Parameter.Add(new CommandParameter("@ModifiedUser", userId));
                        ExecuteNonQuery(strSql, dbTransaction);

                        foreach (string empId in aryAgentId)
                        {
                            strSql = @"INSERT INTO [CaseAssignTable] ([CaseId],[EmpId],[AlreadyAssign],[CreatdUser],[CreatedDate])VALUES (@CaseId,@EmpId,0,@UserId,@CreateDate);";
                            Parameter.Clear();
                            Parameter.Add(new CommandParameter("@CaseId", caseId));
                            Parameter.Add(new CommandParameter("@EmpId", empId));
                            Parameter.Add(new CommandParameter("@UserId", userId));
                            Parameter.Add(new CommandParameter("@CreateDate", dtNow));
                            Parameter.Add(new CommandParameter("@ModifiedUser", userId));
                            bFlag = bFlag && ExecuteNonQuery(strSql, dbTransaction)>0;
                        }
                        CaseMaster masterObj = master.MasterModel(caseId, dbTransaction);
                        strSql = masterObj.AfterSeizureApproved > 0
                            ? "UPDATE [CaseMaster] SET [AgentSubmitDate2] = GETDATE(),[AgentUser2]=@UserId WHERE [CaseId] = @CaseId; "
                            : "UPDATE [CaseMaster] SET [AgentSubmitDate] = GETDATE() WHERE [CaseId] = @CaseId; ";

                        Parameter.Clear();
                        Parameter.Add(new CommandParameter("CaseId", caseId));
                        Parameter.Add(new CommandParameter("UserId", userId));
                        bFlag = bFlag && ExecuteNonQuery(strSql, dbTransaction)>0;

                        bFlag = bFlag && master.UpdateCaseStatus(caseId, CaseStatus.AgentSubmit, userId, dbTransaction)>0;
                    }
                    if (bFlag)
                    {
                        dbTransaction.Commit();
                        return new JsonReturn() { ReturnCode = "1" };
                    }
                    dbTransaction.Rollback();
                    return new JsonReturn() { ReturnCode = "0" };
                }
            }
            catch (Exception)
            {
                try
                {
                    if (dbTransaction != null) dbTransaction.Rollback();
                    return new JsonReturn() { ReturnCode = "0" };
                }
                catch (Exception)
                {
                    // ignored
                }
                throw;
            }
        }

        /// <summary>
        /// 經辦呈核逾期
        /// </summary>
        /// <param name="model"></param>
        /// <param name="aryCaseId"></param>
        /// <param name="aryAgentId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public JsonReturn OverDue(AgentToHandle model, List<Guid> aryCaseId, List<string> aryAgentId, string userId)
        {
            if (aryCaseId == null || !aryCaseId.Any() || aryAgentId == null || !aryAgentId.Any()) return new JsonReturn() { ReturnCode = "0" };

            DateTime dtNow = GetNowDateTime();
            IDbConnection dbConnection = OpenConnection();
            IDbTransaction dbTransaction = null;
            CaseMasterBIZ master = new CaseMasterBIZ();
            try
            {
                using (dbConnection)
                {
                    dbTransaction = dbConnection.BeginTransaction();

                    foreach (Guid caseId in aryCaseId)
                    {
                        string strSql = string.Empty;
                        strSql = @" UPDATE [CaseAssignTable] SET [AlreadyAssign] = 2, [ModifiedUser] = @ModifiedUser, [ModifiedDate] = GETDATE() WHERE [CaseId] = @CaseId;";
                        Parameter.Clear();
                        Parameter.Add(new CommandParameter("CaseId", caseId));
                        Parameter.Add(new CommandParameter("@ModifiedUser", userId));
                        ExecuteNonQuery(strSql, dbTransaction);

                        //20170630 緊急 RQ-2015-019666-018 派件至跨單位 宏祥 update start
                        PARMCodeBIZ para = new PARMCodeBIZ();
                        string limite = limitDate(caseId);
                        DateTime nowdate = GetNowDateTime().AddDays((Convert.ToInt32(para.GetCASE_END_TIME("OVERDUE_DAYS"))) * 1);

                        if (Convert.ToDateTime(limite) < nowdate)
                        {
                           OverDueMemo(model, caseId, userId, dbTransaction);
                        }     
                   
                        foreach (string empId in aryAgentId)
                        {
                            //PARMCodeBIZ para = new PARMCodeBIZ();
                            //string limite = limitDate(caseId);
                            //DateTime nowdate = GetNowDateTime().AddDays((Convert.ToInt32(para.GetCASE_END_TIME("OVERDUE_DAYS"))) * 1);

                            //if (Convert.ToDateTime(limite) < nowdate)
                            //{
                            //    OverDueMemo(model, caseId, userId, dbTransaction);
                            //}
                            //20170630 緊急 RQ-2015-019666-018 派件至跨單位 宏祥 update end

                            strSql = @"INSERT INTO [CaseAssignTable] ([CaseId],[EmpId],[AlreadyAssign],[CreatdUser],[CreatedDate])VALUES (@CaseId,@EmpId,0,@UserId,@CreateDate);";
                            Parameter.Clear();

                            Parameter.Add(new CommandParameter("@CaseId", caseId));
                            Parameter.Add(new CommandParameter("@EmpId", empId));
                            Parameter.Add(new CommandParameter("@UserId", userId));
                            Parameter.Add(new CommandParameter("@CreateDate", dtNow));
                            Parameter.Add(new CommandParameter("@ModifiedUser", userId));
                            Parameter.Add(new CommandParameter("OverDueMemo", model.OverDueMemo));
                            ExecuteNonQuery(strSql, dbTransaction);
                        }
                        CaseMaster masterObj = master.MasterModel(caseId, dbTransaction);
                        strSql = masterObj.AfterSeizureApproved > 0
                            ? "UPDATE [CaseMaster] SET [AgentSubmitDate2] = GETDATE(),[AgentUser2]=@UserId WHERE [CaseId] = @CaseId; "
                            : "UPDATE [CaseMaster] SET [AgentSubmitDate] = GETDATE() WHERE [CaseId] = @CaseId; ";

                        Parameter.Clear();
                        Parameter.Add(new CommandParameter("CaseId", caseId));
                        Parameter.Add(new CommandParameter("UserId", userId));
                        ExecuteNonQuery(strSql, dbTransaction);

                        master.UpdateCaseStatus(caseId, CaseStatus.AgentSubmit, userId, dbTransaction);
                    }
                    dbTransaction.Commit();
                }
                return new JsonReturn() { ReturnCode = "1" };
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
                throw;
            }
        }

        public bool OverDueMemo(AgentToHandle model, Guid caseId, string userId, IDbTransaction trans = null)
        {
            string sql = @"UPDATE [CaseMaster] SET 
                                                [OverDueMemo] = @OverDueMemo, 
                                                [ModifiedUser] = @ModifiedUser,
                                                [ModifiedDate] = GETDATE()
                                                WHERE [CaseId] = @CaseId;";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("@CaseId", caseId));
            Parameter.Add(new CommandParameter("@ModifiedUser", userId));
            base.Parameter.Add(new CommandParameter("OverDueMemo", model.OverDueMemo));
            return trans == null ? ExecuteNonQuery(sql) > 0 : ExecuteNonQuery(sql, trans) > 0;
        }
        public string limitDate(Guid caseId, IDbTransaction trans = null)
        {
            string sql = @"select limitDate from CaseMaster where CaseId=@CaseId";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("@CaseId", caseId));
            return trans == null ? Convert.ToString(ExecuteScalar(sql)) : Convert.ToString(ExecuteScalar(sql, trans));
        }
        #endregion

        public JsonReturn CancelSendAgain(string strCaseNo)
        {
            CaseQueryBIZ caseQueryBiz = new CaseQueryBIZ();
            IDbConnection dbConnection = OpenConnection();
            IDbTransaction dbTrans = null;
            bool bFlag = true;
            try
            {
                using (dbConnection)
                {
                    dbTrans = dbConnection.BeginTransaction();

                    if (strCaseNo.Substring(strCaseNo.Length - 2, 1) == "_")
                    {
                        if (caseQueryBiz.MaxCaseNoSendAgain(strCaseNo.Substring(0, strCaseNo.Length - 2)) == strCaseNo)//屬於最大案件好
                        {
                            bFlag = bFlag && DeleteSendAgain(strCaseNo, dbTrans) > 0;
                        }
                        else
                        {
                            dbTrans.Commit();
                            return new JsonReturn { ReturnCode = "3", ReturnMsg = "" };//所選擇的並非是最大子案件
                        }
                    }
                    else
                    {
                        dbTrans.Commit();
                        return new JsonReturn { ReturnCode = "2", ReturnMsg = "" };//所選擇的並非是子案件
                    }

                    if (bFlag)
                    {
                        dbTrans.Commit();
                        return new JsonReturn { ReturnCode = "1", ReturnMsg = "" };
                    }
                    dbTrans.Rollback();
                    return new JsonReturn { ReturnCode = "0", ReturnMsg = "" };
                }
            }
            catch (Exception ex)
            {
                try
                {
                    if (dbTrans != null)
                        dbTrans.Rollback();
                }
                catch (Exception ex2)
                {

                }
                return new JsonReturn { ReturnCode = "0", ReturnMsg = "" };
            }
        }

        public int DeleteSendAgain(string CaseNo, IDbTransaction trans = null)
        {
            string strSql = "Delete from CaseMaster where CaseNo=@CaseNo";

            base.Parameter.Clear();
            // 添加參數
            base.Parameter.Add(new CommandParameter("@CaseNo", CaseNo));

            try
            {
                return trans == null ? base.ExecuteNonQuery(strSql) : base.ExecuteNonQuery(strSql, trans);
            }
            catch (Exception ex)
            {
                // 拋出異常
                throw ex;
            }
        }                
    }
}
