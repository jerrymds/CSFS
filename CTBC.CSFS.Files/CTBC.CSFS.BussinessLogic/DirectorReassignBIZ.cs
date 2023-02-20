using CTBC.CSFS.Models;
using CTBC.CSFS.Pattern;
using CTBC.CSFS.ViewModels;
using CTBC.FrameWork.Util;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CTBC.FrameWork.Platform; //20170421 固定 RQ-2015-019666-017 派件至跨單位 宏祥 add

namespace CTBC.CSFS.BussinessLogic
{
    public class DirectorReassignBIZ : CommonBIZ
    {
        public DirectorReassignBIZ(AppController appController)
            : base(appController)
        { }

        public DirectorReassignBIZ()
        { }
        /// <summary>
        /// 主管改派查詢
        /// </summary>
        /// <param name="model"></param>
        /// <param name="pageNum"></param>
        /// <param name="strSortExpression"></param>
        /// <param name="strSortDirection"></param>
        /// <param name="UserId"></param>
        /// <param name="where"></param>
        /// <returns></returns>
        public IList<CaseQuery> GetData(CaseQuery model, int pageNum, string strSortExpression, string strSortDirection, string UserId, ref string where)
        {
            try
            {
                where = string.Empty;
                string sqlWhere = "";
                base.PageIndex = pageNum;
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@pageS", (base.PageSize * (base.PageIndex - 1)) + 1));
                base.Parameter.Add(new CommandParameter("@pageE", base.PageSize * base.PageIndex));
                base.Parameter.Add(new CommandParameter("StatusMiddle", CaseStatus.DirectorApproveSeizureAndPay));
                if (!string.IsNullOrEmpty(model.CaseNo))
                {
                    sqlWhere += @" and CaseNo like @CaseNo ";
                    where += @" and CaseNo like '%" + model.CaseNo.Trim() + "%'";
                    base.Parameter.Add(new CommandParameter("@CaseNo", "%" + model.CaseNo.Trim() + "%"));
                }
                if (!string.IsNullOrEmpty(model.GovUnit))
                {
                    sqlWhere += @" and GovUnit like @GovUnit ";
                    where += @" and GovUnit like '%" + model.GovUnit.Trim() + "%'";
                    base.Parameter.Add(new CommandParameter("@GovUnit", "%" + model.GovUnit.Trim() + "%"));
                }
                if (!string.IsNullOrEmpty(model.GovNo))
                {
                    sqlWhere += @" and GovNo like @GovNo ";
                    where += @" and GovUnit like '%" + model.GovNo.Trim() + "%'";
                    base.Parameter.Add(new CommandParameter("@GovNo", "%" + model.GovNo.Trim() + "%"));
                }
                if (!string.IsNullOrEmpty(model.CreateUser))
                {
                    sqlWhere += @" and Person like @Person ";
                    where += @" and Person like '%" + model.CreateUser.Trim() + "%'";
                    base.Parameter.Add(new CommandParameter("@Person", "%" + model.CreateUser.Trim() + "%"));
                }
                if (!string.IsNullOrEmpty(model.SendKind))
                {
                    sqlWhere += @" and SendKind = @SendKind ";
                    Parameter.Add(new CommandParameter("@SendKind", model.SendKind.Trim()));
                }
                if (!string.IsNullOrEmpty(model.Speed))
                {
                    sqlWhere += @" and A.Speed = @Speed ";
                    where += @" and A.Speed = '" + model.Speed.Trim() + "' ";
                    base.Parameter.Add(new CommandParameter("@Speed", model.Speed.Trim()));
                }
                if (!string.IsNullOrEmpty(model.ReceiveKind))
                {
                    sqlWhere += @" and ReceiveKind like @ReceiveKind ";
                    where += @" and ReceiveKind like '%" + model.ReceiveKind.Trim() + "%'";
                    base.Parameter.Add(new CommandParameter("@ReceiveKind", "%" + model.ReceiveKind.Trim() + "%"));
                }
                if (!string.IsNullOrEmpty(model.CaseKind))
                {
                    sqlWhere += @" and CaseKind = @CaseKind ";
                    where += @" and CaseKind = '" + model.CaseKind.Trim() + "' ";
                    base.Parameter.Add(new CommandParameter("@CaseKind", model.CaseKind.Trim()));
                }
                if (!string.IsNullOrEmpty(model.CaseKind2))
                {

                    sqlWhere += @" and CaseKind2 = @CaseKind2 ";
                    where += @" and CaseKind2 = '" + model.CaseKind2.Trim() + "' ";
                    base.Parameter.Add(new CommandParameter("@CaseKind2", model.CaseKind2.Trim()));
                }
                if (!string.IsNullOrEmpty(model.GovDateS))
                {
                    sqlWhere += @" AND GovDate >= @GovDateS";
                    where += @" and GovDate >= '" + model.GovDateS + "' ";
                    base.Parameter.Add(new CommandParameter("@GovDateS", model.GovDateS));
                }
                if (!string.IsNullOrEmpty(model.GovDateE))
                {
                    sqlWhere += @" AND GovDate <= @GovDateE ";
                    where += @" and GovDate <= '" + model.GovDateE + "' ";
                    base.Parameter.Add(new CommandParameter("@GovDateE", model.GovDateE));
                }
                if (!string.IsNullOrEmpty(model.Unit))
                {
                    sqlWhere += @" and Unit like @Unit ";
                    where += @" and Unit like '%" + model.Unit.Trim() + "%'";
                    base.Parameter.Add(new CommandParameter("@Unit", "%" + model.Unit.Trim() + "%"));
                }
                if (!string.IsNullOrEmpty(model.CreatedDateS))
                {
                    sqlWhere += @" AND A.CreatedDate >= @CreatedDateS";
                    where += @" and A.CreatedDate >= '" + model.CreatedDateS + "' ";
                    base.Parameter.Add(new CommandParameter("@CreatedDateS", model.CreatedDateS));
                }
                if (!string.IsNullOrEmpty(model.CreatedDateE))
                {
                    sqlWhere += @" AND A.CreatedDate <= @CreatedDateE ";
                    where += @" and A.CreatedDate <= '" + model.CreatedDateE + "' ";
                    base.Parameter.Add(new CommandParameter("@CreatedDateE", model.CreatedDateE));
                }
                if (!string.IsNullOrEmpty(model.OverDateS))
                {
                    model.OverDateS = UtlString.FormatDateTwStringToAd(model.OverDateS);
                    sqlWhere += @" AND A.CloseDate >= @OverDateS ";
                    where += @" and A.CloseDate >= '" + model.OverDateS + "' ";
                    base.Parameter.Add(new CommandParameter("@OverDateS", model.OverDateS));
                }
                if (!string.IsNullOrEmpty(model.OverDateE))
                {
                    model.OverDateE = UtlString.FormatDateTwStringToAd(model.OverDateE);
                    sqlWhere += @" AND A.CloseDate <= @OverDateE ";
                    where += @" and A.CloseDate <= '" + model.OverDateE + "' ";
                    base.Parameter.Add(new CommandParameter("@OverDateE", model.OverDateE));
                }
                if (!string.IsNullOrEmpty(model.Status))
                {
                    sqlWhere += @" AND Status = @Status ";
                    where += @" and Status = '" + model.Status + "' ";
                    base.Parameter.Add(new CommandParameter("@Status", model.Status));
                }
                //20170421 固定 RQ-2015-019666-017 派件至跨單位 宏祥 add start
                //if (!string.IsNullOrEmpty(model.AgentUser))
                //{
                //    sqlWhere += @" AND AgentUser like @AgentUser ";
                //    where += @" and AgentUser like '%" + model.AgentUser.Trim() + "%'";
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
                else if (model.IsBranchDirector == "1")
                {
                   sqlWhere += @" AND AgentUser in (select EmpID from V_AgentAndDept where ManagerID like @ManagerID) ";
                   base.Parameter.Add(new CommandParameter("@ManagerID", "%" + UserId + "%"));
                }
                //20170421 固定 RQ-2015-019666-017 派件至跨單位 宏祥 add end
                if (!string.IsNullOrEmpty(model.ObligorName))
                {
                    sqlWhere += @" AND B.ObligorName = @ObligorName ";
                    where += @" and B.ObligorName = '" + model.ObligorName + "' ";
                    base.Parameter.Add(new CommandParameter("@ObligorName", model.ObligorName));
                }
                if (!string.IsNullOrEmpty(model.ObligorNo))
                {
                    sqlWhere += @" AND B.ObligorNo = @ObligorNo ";
                    where += @" and B.ObligorNo = '" + model.ObligorNo + "' ";
                    base.Parameter.Add(new CommandParameter("@ObligorNo", model.ObligorNo));
                }


                if (!string.IsNullOrEmpty(model.SendDateS))
                {
                    model.SendDateS = UtlString.FormatDateTwStringToAd(model.SendDateS);
                    sqlWhere += @" AND S.SendDate >= @SendDateS ";
                    where += @" and S.SendDate >= '" + model.SendDateS + "' ";
                    base.Parameter.Add(new CommandParameter("@SendDateS", model.SendDateS));
                }
                if (!string.IsNullOrEmpty(model.SendDateE))
                {
                    model.SendDateE = UtlString.FormatDateTwStringToAd(model.SendDateE);
                    sqlWhere += @" AND S.SendDate <= @SendDateE ";
                    where += @" and S.SendDate <= '" + model.SendDateE + "' ";
                    base.Parameter.Add(new CommandParameter("@SendDateE", model.SendDateE));
                }
                //AgentUser
                if (!string.IsNullOrEmpty(model.SendNo))
                {
                    sqlWhere += @" AND S.SendNo like @SendNo ";
                    where += @" and S.SendNo like '%" + model.SendNo.Trim() + "%'";
                    base.Parameter.Add(new CommandParameter("@SendNo", "%" + model.SendNo.Trim() + "%"));
                }
                string strSql = @";with T1 
	                        as
	                        (
                                SELECT distinct A.CaseId, CaseNo,GovUnit,(SELECT CONVERT(varchar(100), CloseDate, 111)) as CloseDate,AgentUser,(SELECT CONVERT(varchar(100), GovDate, 111)) as GovDate, 
                                GovNo,Person,CaseKind,CaseKind2,A.Speed,Unit,(SELECT CONVERT(varchar(100), LimitDate, 111)) as LimitDate,
                                Status,(SELECT CONVERT(varchar(100), ApproveDate, 111)) as ApproveDate 
                                FROM CaseMaster A 
                                left join CaseObligor B on A.CaseId=b.CaseId left join CaseSendSetting S on A.CaseId=S.CaseId where Status not like '%Z%' and Status <> @StatusMiddle " + sqlWhere + @" 
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

                IList<CaseQuery> ilst = base.SearchList<CaseQuery>(strSql);
                if (ilst != null)
                {
                    if (ilst.Count > 0)
                    {
                        var ilist = GetCodeData("STATUS_NAME");
                        foreach (CaseQuery item in ilst)
                        {
                            var obj = ilist.FirstOrDefault(a => a.CodeNo == item.Status);
                            if (obj != null)
                                item.StatusShow = obj.CodeDesc;
                            else
                                item.StatusShow = item.Status;
                        }
                        base.DataRecords = ilst[0].maxnum;
                    }
                    else
                    {
                        base.DataRecords = 0;
                        ilst = new List<CaseQuery>();
                    }
                    return ilst;
                }
                else
                {
                    return new List<CaseQuery>();
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public IList<CaseQuery> GetStatusName(string colName)
        {
            try
            {
                string strSql = @" select CodeDesc,CodeNo from PARMCode where CodeType=@CodeType ORDER BY SortOrder";
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@CodeType", colName));
                return base.SearchList<CaseQuery>(strSql);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        /// <summary>
        /// 主管改派
        /// </summary>
        /// <param name="aryCaseId"></param>
        /// <param name="aryAgentId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public JsonReturn DirectReassign(List<Guid> aryCaseId, List<string> aryAgentId, string userId)
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
                        Parameter.Add(new CommandParameter("Status", CaseStatus.DirectorReassign));
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
                            /// 2015-06-01 修改改分不同科才另外取號
                            string dep = "C1";
                            string depno = strCaseNo.Substring(8, 1);
                            switch (depno)
                            {
                                case "0":
                                    dep = "C1";
                                    break;
                                case "1":
                                    dep = "C1";
                                    break;
                                case "2":
                                    dep = "C1";
                                    break;
                                case "3":
                                    dep = "C1";
                                    break;
                                case "4":
                                    dep = "C1";
                                    break;
                                case "5":
                                    dep = "C1";
                                    break;
                                case "6":
                                    dep = "C1";
                                    break;
                                case "7":
                                    dep = "C1";
                                    break;
                                case "8":
                                    dep = "C3";
                                    break;
                                case "9":
                                    dep = "C2";
                                    break;
                                default:
                                    dep = "C1";
                                    break;
                            }
                            if (dep != type)
                            //if (strCaseNo.Substring(0, 2) != type)
                            {
                                string caseNo = new CaseNoTableBIZ().GetCaseNo(type, dbTransaction);
                                strSql = "UPDATE [CaseMaster] SET [CaseNo] = @CaseNo WHERE [CaseId] = @CaseId";
                                Parameter.Clear();
                                Parameter.Add(new CommandParameter("CaseId", aryCaseId[i]));
                                Parameter.Add(new CommandParameter("CaseNo", caseNo));
                                bResult = bResult && ExecuteNonQuery(strSql, dbTransaction) > 0;
                            }
                        }
                        //* 寫History
                        bResult = bResult && historyBiz.insertCaseHistory(aryCaseId[i], CaseStatus.DirectorReassign, userId, dbTransaction);
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

        //20170421 固定 RQ-2015-019666-017 派件至跨單位 宏祥 add start
        public int checkAgentDepartment(string DepartmentID, string EmpID)
        {
           try
           {
              string strSql = @" select count(0) from LDAPEmployee where DepDN like @DepartmentID and EmpID=@EmpID";
              base.Parameter.Clear();
              base.Parameter.Add(new CommandParameter("@DepartmentID", "%" + DepartmentID + "%"));
              base.Parameter.Add(new CommandParameter("@EmpID", EmpID));
              return (int)base.ExecuteScalar(strSql);
           }
           catch (Exception ex)
           {
              throw ex;
           }
        }
       //20170421 固定 RQ-2015-019666-017 派件至跨單位 宏祥 add end
    }
}
