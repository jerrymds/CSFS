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
    public class CollectionToSignBIZ : CommonBIZ
    {
        public CollectionToSignBIZ(AppController appController)
            : base(appController)
        { }

        public CollectionToSignBIZ()
        { }
        public IList<CollectionToSign> GetQueryList(CollectionToSign CToS, int pageIndex, string strSortExpression, string strSortDirection)
        {
            try
            {
                base.PageIndex = pageIndex;
                string sqlStr = "";
                string sqlWhere = "";
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@pageS", (base.PageSize * (base.PageIndex - 1)) + 1));
                base.Parameter.Add(new CommandParameter("@pageE", base.PageSize * base.PageIndex));
                if (!string.IsNullOrEmpty(CToS.CaseNo))
                {
                    sqlWhere += @" and CaseNo like @CaseNo ";
                    base.Parameter.Add(new CommandParameter("@CaseNo", "%" + CToS.CaseNo.Trim() + "%"));
                }
                if (!string.IsNullOrEmpty(CToS.GovUnit))
                {
                    sqlWhere += @" and GovUnit like @GovUnit ";
                    base.Parameter.Add(new CommandParameter("@GovUnit", "%" + CToS.GovUnit.Trim() + "%"));
                }
                if (!string.IsNullOrEmpty(CToS.GovNo))
                {
                    sqlWhere += @" and GovNo like @GovNo ";
                    base.Parameter.Add(new CommandParameter("@GovNo", "%" + CToS.GovNo.Trim() + "%"));
                }
                if (!string.IsNullOrEmpty(CToS.Person))
                {
                    sqlWhere += @" and Person like @Person ";
                    base.Parameter.Add(new CommandParameter("@Person", "%" + CToS.Person.Trim() + "%"));
                }
                if (!string.IsNullOrEmpty(CToS.Speed))
                {
                    sqlWhere += @" and Speed like @Speed ";
                    base.Parameter.Add(new CommandParameter("@Speed", "" + CToS.Speed.Trim() + ""));
                }
                if (!string.IsNullOrEmpty(CToS.ReceiveKind))
                {
                    sqlWhere += @" and ReceiveKind like @ReceiveKind ";
                    base.Parameter.Add(new CommandParameter("@ReceiveKind", "" + CToS.ReceiveKind.Trim() + ""));
                }
                if (!string.IsNullOrEmpty(CToS.CaseKind))
                {
                    sqlWhere += @" and CaseKind like @CaseKind ";
                    base.Parameter.Add(new CommandParameter("@CaseKind", "" + CToS.CaseKind.Trim() + ""));
                }
                if (!string.IsNullOrEmpty(CToS.CaseKind2))
                {
                    sqlWhere += @" and CaseKind2 like @CaseKind2 ";
                    base.Parameter.Add(new CommandParameter("@CaseKind2", "" + CToS.CaseKind2.Trim() + ""));
                }
                if (!string.IsNullOrEmpty(CToS.GovDateS))
                {
                    sqlWhere += @" AND GovDate >= @GovDateS";
                    base.Parameter.Add(new CommandParameter("@GovDateS", CToS.GovDateS));
                }
                if (!string.IsNullOrEmpty(CToS.GovDateE))
                {
                    string GovDateE = UtlString.FormatDateString(Convert.ToDateTime(CToS.GovDateE.Replace('/', ' ').ToString()).AddDays(1).ToString("yyyyMMdd"));
                    sqlWhere += @" AND GovDate < @GovDateE ";
                    base.Parameter.Add(new CommandParameter("@GovDateE", GovDateE));
                }
                if (!string.IsNullOrEmpty(CToS.CreatedDateS))
                {
                    sqlWhere += @" AND CreatedDate >= @CreatedDateS";
                    base.Parameter.Add(new CommandParameter("@CreatedDateS", CToS.CreatedDateS));
                }
                if (!string.IsNullOrEmpty(CToS.CreatedDateE))
                {
                    string CreatedDateE = UtlString.FormatDateString(Convert.ToDateTime(CToS.CreatedDateE.Replace('/', ' ').ToString()).AddDays(1).ToString("yyyyMMdd"));
                    sqlWhere += @" AND CreatedDate < @CreatedDateE ";
                    base.Parameter.Add(new CommandParameter("@CreatedDateE", CreatedDateE));
                }
                if (!string.IsNullOrEmpty(CToS.Unit))
                {
                    sqlWhere += @" AND Unit like @Unit ";
                    base.Parameter.Add(new CommandParameter("@Unit", CToS.Unit));
                }
                sqlStr += @";with T1 
	                        as
	                        (
		                       select m.CaseId,GovUnit,(SELECT CONVERT(varchar(100), GovDate, 111)) as GovDate,ReceiverNo,Speed,CaseKind,
		                       CaseKind2,GovNo,Unit,Person,CaseNo,(SELECT CONVERT(varchar(100), LimitDate, 111)) as LimitDate,
                               ReceiveKind,Status,m.CreatedDate,ReturnReason,
                               (select top 1 obligorNo from CaseObligor o where m.CaseId=o.CaseId) as ObligorNo ,docno
                               from CaseMaster m
			                   where (Status='" + CaseStatus.CaseInput + @"' OR Status='" + CaseStatus.AgentReturnClose + @"' ) 
                                    and isDelete='0' " + sqlWhere + @" 
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

                IList<CollectionToSign> _ilsit = base.SearchList<CollectionToSign>(sqlStr);

                if (_ilsit != null)
                {
                    if (_ilsit.Count > 0)
                    {
                        var list = GetCodeData("STATUS_NAME");
                        foreach (CollectionToSign item in _ilsit)
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
                        _ilsit = new List<CollectionToSign>();
                    }
                    return _ilsit;
                }
                else
                {
                    return new List<CollectionToSign>();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //public int Sign(List<Guid> listId,string userId)
        //{
        //    int rtn = 0;
        //    IDbConnection dbConnection = base.OpenConnection();
        //    IDbTransaction dbTransaction = null;
        //    CaseHistoryBIZ history = new CaseHistoryBIZ();
        //    try
        //    {
        //        using (dbConnection)
        //        {
        //            dbTransaction = dbConnection.BeginTransaction();
        //            foreach (Guid id in listId)
        //            {
        //                string sqlStr = @"update CaseMaster set Status=@Status,ModifiedUser=@ModifiedUser,ModifiedDate=GETDATE() where CaseId = @CaseId";
        //                history.insertCaseHistory(id, CaseStatus.CollectionSign, userId, dbTransaction);
        //                base.Parameter.Clear();
        //                base.Parameter.Add(new CommandParameter("@CaseId", id));
        //                base.Parameter.Add(new CommandParameter("@Status", CaseStatus.CollectionSign));
        //                base.Parameter.Add(new CommandParameter("@ModifiedUser", userId));
        //                rtn = base.ExecuteNonQuery(sqlStr, dbTransaction);                       
        //            }              
        //            dbTransaction.Commit();
        //        }
        //        return rtn;
        //    }
        //    catch (Exception ex)
        //    {
        //        try
        //        {
        //            dbTransaction.Rollback();
        //        }
        //        catch (Exception ex2)
        //        {
        //        }
        //        throw ex;
        //    }
        //}

        public int Return(List<Guid> listId, string userId, string returnReason)
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
                        string sqlStr = @"update CaseMaster set Status=@Status,ReturnReason=@ReturnReason,ModifiedUser=@ModifiedUser,ModifiedDate=GETDATE() where CaseId = @CaseId";
                        history.insertCaseHistory(id, CaseStatus.CollectionReturn, userId, dbTransaction);
                        base.Parameter.Clear();
                        base.Parameter.Add(new CommandParameter("@CaseId", id));
                        base.Parameter.Add(new CommandParameter("@Status", CaseStatus.CollectionReturn));
                        base.Parameter.Add(new CommandParameter("@ReturnReason", returnReason));
                        base.Parameter.Add(new CommandParameter("@ModifiedUser", userId));
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
                throw ex;
            }
        }


        /// <summary>
        /// 集作分派
        /// </summary>
        /// <param name="aryCaseId"></param>
        /// <param name="aryAgentId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public JsonReturn CollectionAssign(List<Guid> aryCaseId, List<string> aryAgentId, string userId)
        {
            if (aryCaseId == null || !aryCaseId.Any() || aryAgentId == null || !aryAgentId.Any()) return new JsonReturn(){ReturnCode = "0"};

            DateTime dtNow = GetNowDateTime();
            IDbConnection dbConnection = OpenConnection();
            IDbTransaction dbTransaction = null;
            CaseMasterBIZ masterBiz = new CaseMasterBIZ();
            LdapEmployeeBiz agentBiz = new LdapEmployeeBiz();
            CaseHistoryBIZ historyBiz = new CaseHistoryBIZ();
            bool bResult = true;
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
                        Parameter.Add(new CommandParameter("Status", CaseStatus.CollectionSubmit));
                        Parameter.Add(new CommandParameter("ModifiedUser", userId));
                        bResult = bResult && ExecuteNonQuery(strSql, dbTransaction) > 0;

                        if (master.CaseKind == CaseKind.CASE_EXTERNAL)
                        {
                            //*	外來文案件: C+民國年7碼+KXX (K:0~7 一科，K:9 二科，K:8 三科) (於集作收件派案後，系統自動編列)
                            string type = agent == null? "C"
                                                        : agent.SectionName.Contains("一") ? "C1"
                                                        : agent.SectionName.Contains("二") ? "C2"
                                                        : agent.SectionName.Contains("三") ? "C3"
                                                        : "C";
                            string caseNo = new CaseNoTableBIZ().GetCaseNo(type, dbTransaction);
                            strSql = "UPDATE [CaseMaster] SET [CaseNo] = @CaseNo WHERE [CaseId] = @CaseId";
                            Parameter.Clear();
                            Parameter.Add(new CommandParameter("CaseId", aryCaseId[i]));
                            Parameter.Add(new CommandParameter("CaseNo", caseNo));
                            bResult = bResult && ExecuteNonQuery(strSql, dbTransaction) > 0;
                            //* 想了下.這CaseNoChangHistory.OldCaseNo如果為空寫進去了那麼查詢時會無視其他條件
                            //CaseMemoBiz casememo = new CaseMemoBiz();
                            //CaseMemo memoModel = new CaseMemo();
                            //memoModel.CaseId = aryCaseId[i];
                            //memoModel.MemoType = "CaseMemo";
                            //memoModel.Memo = "外來文集作分派,案件編號從: " + master.CaseNo + " 變為:" + caseNo;
                            //memoModel.MemoDate = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                            //memoModel.MemoUser = userId;
                            //casememo.Create(memoModel, dbTransaction);
                            //masterBiz.CreateCaseNoChangHistory(aryCaseId[i], master.CaseNo, caseNo, userId, dbTransaction);
                        }

                        //* 寫History
                        bResult = bResult && historyBiz.insertCaseHistory(aryCaseId[i], CaseStatus.CollectionSubmit, userId, dbTransaction);
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
    }
}
