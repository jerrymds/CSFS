using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CTBC.CSFS.Pattern;
using CTBC.CSFS.Models;
using CTBC.CSFS.ViewModels;
using System.Data;

namespace CTBC.CSFS.BussinessLogic
{
    public class CollectionToAgentBIZ : CommonBIZ
    {
        public CollectionToAgentBIZ(AppController AppController)
            : base(AppController)
        { }
        public CollectionToAgentBIZ() { }

        public IList<CollectionToAgent>  GetData(CollectionToAgent model, int pageNum, string strSortExpression, string strSortDirection, string UserId)
        {
            try
            {
                string sqlWhere = "";
                base.PageIndex = pageNum;
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@pageS", (base.PageSize * (base.PageIndex - 1)) + 1));
                base.Parameter.Add(new CommandParameter("@pageE", base.PageSize * base.PageIndex));
                if (!string.IsNullOrEmpty(model.CaseNo))
                {
                    sqlWhere += @" and CaseNo like @CaseNo ";
                    base.Parameter.Add(new CommandParameter("@CaseNo", "%" + model.CaseNo.Trim() + "%"));
                }
                if (!string.IsNullOrEmpty(model.GovUnit))
                {
                    sqlWhere += @" and GovUnit like @GovUnit ";
                    base.Parameter.Add(new CommandParameter("@GovUnit", "%" + model.GovUnit.Trim() + "%"));
                }
                if (!string.IsNullOrEmpty(model.GovNo))
                {
                    sqlWhere += @" and GovNo like @GovNo ";
                    base.Parameter.Add(new CommandParameter("@GovNo", "%" + model.GovNo.Trim() + "%"));
                }
                if (!string.IsNullOrEmpty(model.GovKind))
                {
                    sqlWhere += @" and GovKind like @GovKind ";
                    base.Parameter.Add(new CommandParameter("@GovKind", "%" + model.GovKind.Trim() + "%"));
                }
                if (!string.IsNullOrEmpty(model.CreatedUser))
                {
                    sqlWhere += @" and Person like @Person ";
                    base.Parameter.Add(new CommandParameter("@Person", "%" + model.CreatedUser.Trim() + "%"));
                }
                if (!string.IsNullOrEmpty(model.Speed))
                {
                    sqlWhere += @" and Speed = @Speed ";
                    base.Parameter.Add(new CommandParameter("@Speed", model.Speed.Trim()));
                }
                if (!string.IsNullOrEmpty(model.ReceiveKind))
                {
                    sqlWhere += @" and ReceiveKind like @ReceiveKind ";
                    base.Parameter.Add(new CommandParameter("@ReceiveKind", "%" + model.ReceiveKind.Trim() + "%"));
                }
                if (!string.IsNullOrEmpty(model.CaseKind))
                {
                    sqlWhere += @" and CaseKind = @CaseKind ";
                    base.Parameter.Add(new CommandParameter("@CaseKind", model.CaseKind.Trim()));
                }
                if (!string.IsNullOrEmpty(model.CaseKind2))
                {

                    sqlWhere += @" and CaseKind2 = @CaseKind2 ";
                    base.Parameter.Add(new CommandParameter("@CaseKind2", model.CaseKind2.Trim()));
                }
                if (!string.IsNullOrEmpty(model.GovDateS))
                {
                    sqlWhere += @" AND GovDate >= @GovDateS";
                    base.Parameter.Add(new CommandParameter("@GovDateS", model.GovDateS));
                }
                if (!string.IsNullOrEmpty(model.GovDateE))
                {
                    sqlWhere += @" AND GovDate < @GovDateE ";
                    base.Parameter.Add(new CommandParameter("@GovDateE", model.GovDateE));
                }
                if (!string.IsNullOrEmpty(model.Unit))
                {
                    sqlWhere += @" and Unit like @Unit ";
                    base.Parameter.Add(new CommandParameter("@Unit", "%" + model.Unit.Trim() + "%"));
                }
                if (!string.IsNullOrEmpty(model.CreatedDateS))
                {
                    sqlWhere += @" AND CreatedDate >= @CreatedDateS";
                    base.Parameter.Add(new CommandParameter("@CreatedDateS", model.CreatedDateS));
                }
                if (!string.IsNullOrEmpty(model.CreatedDateE))
                {
                    string approveDateEnd = Convert.ToDateTime(model.CreatedDateE).AddDays(1).ToString("yyyy/MM/dd");
                    sqlWhere += @" AND CreatedDate < @CreatedDateE ";
                    base.Parameter.Add(new CommandParameter("@CreatedDateE", approveDateEnd));
                }
                //20170421 固定 RQ-2015-019666-017 派件至跨單位 宏祥 update start
                //if (!string.IsNullOrEmpty(model.AgentUser))
                //{
                //   sqlWhere += @" AND AgentUser like @AgentUser ";
                //   base.Parameter.Add(new CommandParameter("@AgentUser", "%" + model.AgentUser + "%"));
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
                //20170421 固定 RQ-2015-019666-017 派件至跨單位 宏祥 update end
                string strSql = @";with T1 
	                        as
	                        (
                               SELECT distinct CaseId,row_number() over (order by CaseId asc ) num,CaseNo,GovUnit,AgentUser,(SELECT CONVERT(varchar(100), GovDate, 111)) as GovDate,(SELECT CONVERT(varchar(100), LimitDate, 111)) as LimitDate, 
                                GovNo,CaseKind,CaseKind2,Speed,Unit,isnull(CreatedUser,'9999') CreatedUser,OverDueMemo
                                FROM CaseMaster  where 0=0 and (Status=@Status1 or Status=@Status2 )" + sqlWhere + @" 
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
                base.Parameter.Add(new CommandParameter("@Status1", CaseStatus.AgentToCollection));//AgentToCollection
                base.Parameter.Add(new CommandParameter("@Status2", CaseStatus.CollectionEdit));
                IList<CollectionToAgent> ilst = base.SearchList<CollectionToAgent>(strSql);
                if (ilst != null)
                {
                    if (ilst.Count > 0)
                    {
                        base.DataRecords = ilst[0].maxnum;
                    }
                    else
                    {
                        base.DataRecords = 0;
                        ilst = new List<CollectionToAgent>();
                    }
                    return ilst;
                }
                else
                {
                    return new List<CollectionToAgent>();
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public JsonReturn SendAgents(List<Guid> aryCaseId, string userId)
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
                        //* 更新Master
                        string strSql = @"UPDATE [CaseMaster] 
                                    SET [Status] = @Status,
	                                    [ModifiedUser] = @ModifiedUser,
	                                    [ModifiedDate] = GETDATE()
                                    WHERE [CaseId] = @CaseId";
                        Parameter.Clear();
                        Parameter.Add(new CommandParameter("CaseId", aryCaseId[i]));
                        Parameter.Add(new CommandParameter("Status", CaseStatus.CollectionToAgent));
                        Parameter.Add(new CommandParameter("ModifiedUser", userId));
                        bResult = bResult && ExecuteNonQuery(strSql, dbTransaction) > 0;
                        //* 寫History
                        bResult = bResult && historyBiz.insertCaseHistory(aryCaseId[i], CaseStatus.CollectionToAgent, userId, dbTransaction);
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
        public string GetAgent(Guid caseId)
        {
            try
            {
                string strSql = @"SELECT  AgentUser from CaseMaster where CaseId in (@CaseId)";
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("CaseId", caseId));
                return ExecuteScalar(strSql) as string;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public bool AgentSubmit(List<Guid> aryCaseId, List<string> aryAgentId, string userId)
        {
            if (aryCaseId == null || !aryCaseId.Any() || aryAgentId == null || !aryAgentId.Any()) return false;

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
                        Parameter.Add(new CommandParameter("@CaseId", caseId));
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
                throw;
            }
        }
        public bool OverDue(AgentToHandle model, List<Guid> aryCaseId, List<string> aryAgentId, string userId)
        {
            if (aryCaseId == null || !aryCaseId.Any() || aryAgentId == null || !aryAgentId.Any()) return false;

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
                        Parameter.Add(new CommandParameter("@CaseId", caseId));
                        Parameter.Add(new CommandParameter("@ModifiedUser", userId));
                        ExecuteNonQuery(strSql, dbTransaction);

                        foreach (string empId in aryAgentId)
                        {
                            PARMCodeBIZ para = new PARMCodeBIZ();
                            string limite = limitDate(caseId);
                            DateTime nowdate = GetNowDateTime().AddDays((Convert.ToInt32(para.GetCASE_END_TIME("OVERDUE_DAYS"))) * 1);

                            if (Convert.ToDateTime(limite) < nowdate)
                            {
                                OverDueMemo(model, caseId, userId, dbTransaction);
                            }

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

        //20170421 固定 RQ-2015-019666-017 派件至跨單位 宏祥 add start
        /// <summary>
        /// 取得經辦人員下拉選單(科組)
        /// </summary>
        /// <param name="DepartmentID"></param>
        /// <returns></returns>
        public IList<AgentSetting> GetAgentDepartment2View(string DepartmentID)
        {
           string strSql = @"select distinct SectionName,DepartmentID from AgentSetting where DepartmentID=@DepartmentID";
           base.Parameter.Clear();
           Parameter.Add(new CommandParameter("@DepartmentID", DepartmentID));
           return base.SearchList<AgentSetting>(strSql);
        }
        /// <summary>
        /// 取得經辦人員下拉選單(人)
        /// </summary>
        /// <param name="DepartmentID"></param>
        /// <returns></returns>
        public IList<LDAPEmployee> GetAgentDepartmentUserView(string DepartmentID)
        {
           string strSql = @"SELECT  A.[EmpID]
                                  ,[EmpName]
                                  ,A.[SectionName]
                                  ,[DepName]
                                  ,[UpperDepID]
                                  ,[UpDepName]
                                  ,[EmpTitle]
                                  ,[EmpBusinessCategory]
                                  ,[IsManager]
                                  ,[EMail]
                                  ,[DepDN]
                                  ,[DepID]
                                  ,[ManagerID]
                                  ,[BranchID]
                                  ,[BranchName]
                                  ,[CreatedUser]
                                  ,[CreatedDate]
                                  ,[ModifiedUser]
                                  ,[ModifiedDate]
                                  ,A.[EmpID] + ' - ' + [EmpName] AS [EmpIdAndName]
                             FROM [AgentSetting] AS A LEFT OUTER JOIN 
							        [V_AgentAndDept] AS V ON A.[EmpID] = V.[EmpID]
							        WHERE A.[SectionName] = @DepartmentID
                             ORDER BY [SectionName]";
           base.Parameter.Clear();
           Parameter.Add(new CommandParameter("@DepartmentID", DepartmentID));
           return base.SearchList<LDAPEmployee>(strSql);
        }
        //20170421 固定 RQ-2015-019666-017 派件至跨單位 宏祥 add end
    }
}
