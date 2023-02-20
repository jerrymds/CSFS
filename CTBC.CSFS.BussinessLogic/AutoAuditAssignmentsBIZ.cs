using System;
using System.Collections.Generic;
using System.Linq;
using CTBC.CSFS.Models;
using CTBC.CSFS.Pattern;
using CTBC.CSFS.ViewModels;
using CTBC.FrameWork.Util;
using CTBC.CSFS.Resource;
using System.Data;
using System.IO;
using NPOI.HSSF.UserModel;
namespace CTBC.CSFS.BussinessLogic
{
    public class AutoAuditAssignmentsBIZ : CommonBIZ
    {
        private static List<Guid> staticAryCaseId = new List<Guid>();
        public AutoAuditAssignmentsBIZ(AppController appController)
            : base(appController)
        { }

        public AutoAuditAssignmentsBIZ()
        { }
        /// <summary>
        /// 取得主管核決的結果集
        /// </summary>
        /// <param name="model"></param>
        /// <param name="pageIndex"></param>
        /// <param name="strSortExpression"></param>
        /// <param name="strSortDirection"></param>
        /// <param name="EmpId"></param>
        /// <returns></returns>
        public IList<DirectorToApprove> GetDirectorToApproveData(DirectorToApprove model, int pageIndex, string strSortExpression, string strSortDirection, string EmpId)
        {
            try
            {
                string sqlWhere = "";
                PageIndex = pageIndex;
                Parameter.Clear();
                /// Adam 20180302 ///
                /// 修改主管每頁30筆 ///
                Parameter.Add(new CommandParameter("@pageS", (PageSize * (PageIndex - 1)) + 1));
                Parameter.Add(new CommandParameter("@pageE", PageSize * PageIndex));
                if (!string.IsNullOrEmpty(model.CaseNo))
                {
                    sqlWhere += @" and CaseNo like @CaseNo ";
                    Parameter.Add(new CommandParameter("@CaseNo", "%" + model.CaseNo.Trim() + "%"));
                }
                if (!string.IsNullOrEmpty(model.GovUnit))
                {
                    sqlWhere += @" and GovUnit like @GovUnit ";
                    Parameter.Add(new CommandParameter("@GovUnit", "%" + model.GovUnit.Trim() + "%"));
                }
                if (!string.IsNullOrEmpty(model.GovNo))
                {
                    sqlWhere += @" and GovNo like @GovNo ";
                    Parameter.Add(new CommandParameter("@GovNo", "%" + model.GovNo.Trim() + "%"));
                }
                if (!string.IsNullOrEmpty(model.CreateUser))
                {
                    sqlWhere += @" and Person like @Person ";
                    Parameter.Add(new CommandParameter("@Person", "%" + model.CreateUser.Trim() + "%"));
                }
                if (!string.IsNullOrEmpty(model.SendKind))
                {
                    sqlWhere += @" and SendKind like @SendKind ";
                    Parameter.Add(new CommandParameter("@SendKind", model.SendKind.Trim()));
                }
                if (!string.IsNullOrEmpty(model.Speed))
                {
                    sqlWhere += @" and B.Speed = @Speed ";
                    Parameter.Add(new CommandParameter("@Speed", model.Speed.Trim()));
                }
                if (!string.IsNullOrEmpty(model.ReceiveKind))
                {
                    sqlWhere += @" and ReceiveKind like @ReceiveKind ";
                    Parameter.Add(new CommandParameter("@ReceiveKind", "%" + model.ReceiveKind.Trim() + "%"));
                }
                if (!string.IsNullOrEmpty(model.CaseKind))
                {
                    sqlWhere += @" and CaseKind like @CaseKind ";
                    Parameter.Add(new CommandParameter("@CaseKind", "%" + model.CaseKind.Trim() + "%"));
                }
                if (!string.IsNullOrEmpty(model.CaseKind2))
                {
                    sqlWhere += @" and CaseKind2 like @CaseKind2 ";
                    Parameter.Add(new CommandParameter("@CaseKind2", "%" + model.CaseKind2.Trim() + "%"));
                }
                if (!string.IsNullOrEmpty(model.GovDateS))
                {
                    sqlWhere += @" AND GovDate >= @GovDateS";
                    Parameter.Add(new CommandParameter("@GovDateS", model.GovDateS));
                }
                if (!string.IsNullOrEmpty(model.GovDateE))
                {
                    string qDateE = Convert.ToDateTime(UtlString.FormatDateString(model.GovDateE)).AddDays(1).ToString("yyyyMMdd");
                    sqlWhere += @" AND GovDate < @GovDateE ";
                    Parameter.Add(new CommandParameter("@GovDateE", qDateE));
                }

                //20170630 緊急 RQ-2015-019666-018 派件至跨單位 宏祥 add start
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
                //--modify by zhangwei 20180713 start
                //else if (model.IsBranchDirector == "1")
                //{
                //    sqlWhere += @" AND AgentUser in (select EmpID from V_AgentAndDept where ManagerID like @ManagerID) ";
                //    base.Parameter.Add(new CommandParameter("@ManagerID", "%" + EmpId + "%"));
                //}
                //--modify by zhangwei 20180713 end
                //20170630 緊急 RQ-2015-019666-018 派件至跨單位 宏祥 add end

                if (!string.IsNullOrEmpty(model.SendDateS) || !string.IsNullOrEmpty(model.SendDateE))
                {
                    string sqlWhere1 = "";
                    if (!string.IsNullOrEmpty(model.SendDateS))
                    {
                        model.SendDateS = UtlString.FormatDateTwStringToAd(model.SendDateS);
                        sqlWhere1 += @" AND SendDate >= @SendDateStart";
                        Parameter.Add(new CommandParameter("@SendDateStart", model.SendDateS));
                    }
                    if (!string.IsNullOrEmpty(model.SendDateE))
                    {
                        string sendDateEnd = Convert.ToDateTime(UtlString.FormatDateString(model.SendDateE)).AddDays(1).ToString("yyyyMMdd");
                        sqlWhere1 += @" AND SendDate < @SendDateEnd ";
                        Parameter.Add(new CommandParameter("@SendDateEnd", sendDateEnd));
                    }
                    sqlWhere += " AND B.CaseId IN (SELECT CaseId  FROM CaseSendSetting AS CSS where 1=1 " + sqlWhere1 + @") ";
                }
                if (!string.IsNullOrEmpty(model.Unit))
                {
                    sqlWhere += @" and Unit like @Unit ";
                    Parameter.Add(new CommandParameter("@Unit", "%" + model.Unit.Trim() + "%"));
                }
                if (!string.IsNullOrEmpty(model.CreatedDateS))
                {
                    sqlWhere += @" AND B.CreatedDate >= @CreatedDateS";
                    Parameter.Add(new CommandParameter("@CreatedDateS", model.CreatedDateS));
                }
                if (!string.IsNullOrEmpty(model.CreatedDateE))
                {
                    string createdDateE = Convert.ToDateTime(UtlString.FormatDateString(model.CreatedDateE)).AddDays(1).ToString("yyyy/MM/dd");
                    sqlWhere += @" AND B.CreatedDate < @CreatedDateE ";
                    Parameter.Add(new CommandParameter("@CreatedDateE", createdDateE));
                }
                //--add by zhangwei 20180713 start
                sqlWhere += @" AND AgentUser <> @AgentUserManage ";
                Parameter.Add(new CommandParameter("@AgentUserManage", EmpId));
                //--add by zhangwei 20180713 start
                //自動化審核查詢時,要排除自動扣押退回人工的案件,並且要是自動扣押、自動撤銷的案件
                sqlWhere += @" and B.CaseId not in (select CaseId from CaseHistory ch1 where ToRole like '自動%退回人工' and convert(varchar(10),CreatedDate,23) = (select convert(varchar(10),max(CreatedDate),23) from CaseHistory ch2 where  ch1.CaseId = ch2.CaseId)) ";
                sqlWhere += @" and B.CaseId in (select CaseId from CaseHistory ch1 where FromRole like '自動%' and CreatedDate = (select  max(CreatedDate) from CaseHistory ch2 where  ch1.CaseId = ch2.CaseId)) ";
                string strSql = @";with T1 
	                        as
	                        (
		                       SELECT distinct B.Status,B.CaseId,B.CaseNo,B.GovUnit,(SELECT CONVERT(varchar(100), B.GovDate, 111)) as GovDate,B.GovNo,B.Person,B.AgentUser,B.CaseKind,B.CaseKind2,B.Speed, (SELECT TOP 1 CONVERT(varchar(12),m.SendDate,111) FROM CaseSendSetting m where m.CaseId = B.CaseId ORDER BY CreatedDate desc) AS SendDate,(SELECT CONVERT(varchar(100), B.LimitDate, 111)) as LimitDate FROM CaseAssignTable AS A
                                LEFT OUTER JOIN CaseMaster AS B ON A.CaseId=B.CaseId left join CaseSendSetting CSS on CSS.CaseId=B.CaseId
                                WHERE --A.EmpId=@EmpId AND A.AlreadyAssign=0 AND 
                                (B.Status=@Status1 OR B.Status=@Status2) AND B.isDelete='0' " + sqlWhere + @" 
	                        ),T2 as
	                        (
		                        select *, row_number() over (order by LimitDate ASC , CaseNo ASC ) RowNum
		                        from T1
	                        ),T3 as 
	                        (
		                        select *,(select max(RowNum) from T2) maxnum from T2 
		                        where rownum between @pageS and @pageE
	                        )
	                        select a.* from T3 a order by a.RowNum";

                Parameter.Add(new CommandParameter("EmpId", EmpId));
                Parameter.Add(new CommandParameter("Status1", CaseStatus.AgentSubmit));
                Parameter.Add(new CommandParameter("Status2", CaseStatus.DirectorSubmit));

                IList<DirectorToApprove> ilst = SearchList<DirectorToApprove>(strSql);
                if (ilst != null)
                {
                    if (ilst.Count > 0)
                    {
                        DataRecords = ilst[0].maxnum;
                    }
                    else
                    {
                        DataRecords = 0;
                        ilst = new List<DirectorToApprove>();
                    }
                    return ilst;
                }
                return new List<DirectorToApprove>();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        /// <summary>
        /// 取得主管協同作業的結果集
        /// </summary>
        /// <param name="model"></param>
        /// <param name="pageIndex"></param>
        /// <param name="strSortExpression"></param>
        /// <param name="strSortDirection"></param>
        /// <param name="EmpId"></param>
        /// <returns></returns>
        public IList<DirectorCooperative> GetDirectorCooperativeData(DirectorCooperative model, int pageIndex, string strSortExpression, string strSortDirection, string EmpId)
        {
            try
            {
                string sqlWhere = "";
                PageIndex = pageIndex;
                Parameter.Clear();
                Parameter.Add(new CommandParameter("@pageS", (PageSize * (PageIndex - 1)) + 1));
                Parameter.Add(new CommandParameter("@pageE", PageSize * PageIndex));
                if (!string.IsNullOrEmpty(model.CaseNo))
                {
                    sqlWhere += @" and CaseNo like @CaseNo ";
                    Parameter.Add(new CommandParameter("@CaseNo", "%" + model.CaseNo.Trim() + "%"));
                }
                if (!string.IsNullOrEmpty(model.GovUnit))
                {
                    sqlWhere += @" and GovUnit like @GovUnit ";
                    Parameter.Add(new CommandParameter("@GovUnit", "%" + model.GovUnit.Trim() + "%"));
                }
                if (!string.IsNullOrEmpty(model.GovNo))
                {
                    sqlWhere += @" and GovNo like @GovNo ";
                    Parameter.Add(new CommandParameter("@GovNo", "%" + model.GovNo.Trim() + "%"));
                }
                if (!string.IsNullOrEmpty(model.CreateUser))
                {
                    sqlWhere += @" and Person like @Person ";
                    Parameter.Add(new CommandParameter("@Person", "%" + model.CreateUser.Trim() + "%"));
                }
                if (!string.IsNullOrEmpty(model.SendKind))
                {
                    sqlWhere += @" and SendKind like @SendKind ";
                    Parameter.Add(new CommandParameter("@SendKind", model.SendKind.Trim()));
                }
                if (!string.IsNullOrEmpty(model.Speed))
                {
                    sqlWhere += @" and cm.Speed = @Speed ";
                    Parameter.Add(new CommandParameter("@Speed", model.Speed.Trim()));
                }
                if (!string.IsNullOrEmpty(model.ReceiveKind))
                {
                    sqlWhere += @" and ReceiveKind like @ReceiveKind ";
                    Parameter.Add(new CommandParameter("@ReceiveKind", "%" + model.ReceiveKind.Trim() + "%"));
                }
                if (!string.IsNullOrEmpty(model.CaseKind))
                {
                    sqlWhere += @" and CaseKind = @CaseKind ";
                    Parameter.Add(new CommandParameter("@CaseKind", model.CaseKind.Trim()));
                }
                if (!string.IsNullOrEmpty(model.CaseKind2))
                {
                    sqlWhere += @" and CaseKind2 = @CaseKind2 ";
                    Parameter.Add(new CommandParameter("@CaseKind2", model.CaseKind2.Trim()));
                }
                if (!string.IsNullOrEmpty(model.GovDateS))
                {
                    sqlWhere += @" AND GovDate >= @GovDateS";
                    Parameter.Add(new CommandParameter("@GovDateS", model.GovDateS));
                }
                if (!string.IsNullOrEmpty(model.GovDateE))
                {
                    string qDateE = Convert.ToDateTime(UtlString.FormatDateString(model.GovDateE)).AddDays(1).ToString("yyyyMMdd");
                    sqlWhere += @" AND GovDate < @GovDateE ";
                    Parameter.Add(new CommandParameter("@GovDateE", qDateE));
                }
                //20170714 固定 RQ-2015-019666-019 派件至跨單位 宏祥 add start
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
                //20170714 固定 RQ-2015-019666-019 派件至跨單位 宏祥 add end

                if (!string.IsNullOrEmpty(model.SendDateS) || !string.IsNullOrEmpty(model.SendDateE))
                {
                    string sqlWhere1 = "";
                    if (!string.IsNullOrEmpty(model.SendDateS))
                    {
                        model.SendDateS = UtlString.FormatDateTwStringToAd(model.SendDateS);
                        sqlWhere1 += @" AND SendDate >= @SendDateStart";
                        Parameter.Add(new CommandParameter("@SendDateStart", model.SendDateS));
                    }
                    if (!string.IsNullOrEmpty(model.SendDateE))
                    {
                        string sendDateEnd = Convert.ToDateTime(UtlString.FormatDateString(model.SendDateE)).AddDays(1).ToString("yyyyMMdd");
                        sqlWhere1 += @" AND SendDate < @SendDateEnd ";
                        Parameter.Add(new CommandParameter("@SendDateEnd", sendDateEnd));
                    }
                    sqlWhere += " AND cm.CaseId IN (SELECT CaseId  FROM CaseSendSetting AS CSS where 1=1 " + sqlWhere1 + @") ";
                }
                if (!string.IsNullOrEmpty(model.Unit))
                {
                    sqlWhere += @" and Unit like @Unit ";
                    Parameter.Add(new CommandParameter("@Unit", "%" + model.Unit.Trim() + "%"));
                }
                if (!string.IsNullOrEmpty(model.CreatedDateS))
                {
                    sqlWhere += @" AND cm.CreatedDate >= @CreatedDateS";
                    Parameter.Add(new CommandParameter("@CreatedDateS", model.CreatedDateS));
                }
                if (!string.IsNullOrEmpty(model.CreatedDateE))
                {
                    string createdDateE = Convert.ToDateTime(UtlString.FormatDateString(model.CreatedDateE)).AddDays(1).ToString("yyyyMMdd");
                    sqlWhere += @" AND cm.CreatedDate < @CreatedDateE ";
                    Parameter.Add(new CommandParameter("@CreatedDateE", createdDateE));
                }
                sqlWhere += @" AND AgentUser <> @AgentUserManage ";
                Parameter.Add(new CommandParameter("@AgentUserManage", EmpId));
                string strSql = @";with T1 
	                        as
	                        (
                               SELECT distinct cm.Status,cm.CaseId,cm.CaseNo,cm.GovUnit,(SELECT CONVERT(varchar(100), cm.GovDate, 111)) as GovDate,
                                (SELECT TOP 1 CONVERT(varchar(12),m.SendDate,111) FROM CaseSendSetting m where m.CaseId = cm.CaseId ORDER BY cm.CreatedDate desc) AS SendDate,
                               cm.GovNo,cm.Person,cm.AgentUser,cm.CaseKind,cm.CaseKind2,cm.Speed,(SELECT CONVERT(varchar(100), cm.LimitDate, 111)) as LimitDate
                                FROM CaseMaster cm left join CaseSendSetting css on cm.caseId=css.caseId where (cm.Status=@Status1 OR cm.Status=@Status2) AND cm.isDelete='0' " + sqlWhere + @" 
	                        ),T2 as
	                        (
		                        select *, row_number() over (order by LimitDate ASC , CaseNo ASC ) RowNum
		                        from T1
	                        ),T3 as 
	                        (
		                        select *,(select max(RowNum) from T2) maxnum from T2 
		                        where rownum between @pageS and @pageE
	                        )
	                        select a.* from T3 a order by a.RowNum";

                Parameter.Add(new CommandParameter("EmpId", EmpId));
                Parameter.Add(new CommandParameter("Status1", CaseStatus.AgentSubmit));
                Parameter.Add(new CommandParameter("Status2", CaseStatus.DirectorSubmit));

                IList<DirectorCooperative> ilst = SearchList<DirectorCooperative>(strSql);
                if (ilst != null)
                {
                    if (ilst.Count > 0)
                    {
                        DataRecords = ilst[0].maxnum;
                    }
                    else
                    {
                        DataRecords = 0;
                        ilst = new List<DirectorCooperative>();
                    }
                    return ilst;
                }
                return new List<DirectorCooperative>();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 獲取還沒有上傳的檔案
        /// </summary>
        /// <param name="caseIdarr"></param>
        /// <returns></returns>
        public IList<CaseEdocFile> GetBatchControlNotUp(string caseIdarr)
        {
            try
            {
                string sql = @"select * from CaseEdocFile where Type='發文' and caseId in (select caseId from BatchControl where STATUS_Create=1 and STATUS_Transfer=0 and caseId in (" + caseIdarr + "))";
                //string sql = @"select * from CaseEdocFile";
                IList<CaseEdocFile> list = SearchList<CaseEdocFile>(sql);
                return list;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public bool UpdateBatchControlUp(List<Guid> aryCaseId, string userId)
        {
            if (aryCaseId == null || !aryCaseId.Any())
            { return false; }
            bool bRtn = true;
            IDbConnection dbConnection = OpenConnection();
            IDbTransaction dbTransaction = null;
            try
            {
                using (dbConnection)
                {
                    dbTransaction = dbConnection.BeginTransaction();
                    foreach (Guid caseId in aryCaseId)
                    {
                        string sql = @"update BatchControl set STATUS_Transfer = 1,UPDATE_TIME=getdate() where caseId = @CaseId;update CaseSendSetting set SendUpDate=getdate() where SerialID in (select SerialID from CaseSendSetting where CaseId=@CaseId and SendKind='電子發文');
                                       Insert into CaseHistory(CaseId,Event,EventTime,ToRole,ToUser,ToFolder) values(@CaseId,'電子發文上傳',getdate(),'主管人員',@userId,'主管-上傳')";
                        Parameter.Clear();
                        Parameter.Add(new CommandParameter("@CaseId", caseId));
                        Parameter.Add(new CommandParameter("@userId", userId));
                        bRtn = bRtn && ExecuteNonQuery(sql) > 0;
                    }
                    if (bRtn)
                    {
                        dbTransaction.Commit();
                        return bRtn;
                    }
                    dbTransaction.Rollback();
                    return bRtn;
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

        /// <summary>
        /// 主管 放行
        /// </summary>
        /// <param name="aryCaseId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public JsonReturn DirectorApprove(List<String> aryStatus, List<Guid> aryCaseId, string userId)
        {
            if (aryCaseId == null || !aryCaseId.Any()) return new JsonReturn() { ReturnCode = "0" };
            //并發驗證
            foreach (var item in aryCaseId)
            {
                if (staticAryCaseId != null && staticAryCaseId.Contains(item))
                {
                    return new JsonReturn { ReturnCode = "0", ReturnMsg = "(" + item + ")" + "此案件正在處理中，請重新查詢！" };
                }
                else { staticAryCaseId.Add(item); }
            }
            bool bRtn = true;
            IDbConnection dbConnection = OpenConnection();
            IDbTransaction dbTransaction = null;
            CaseMasterBIZ master = new CaseMasterBIZ();
            CaseHistoryBIZ history = new CaseHistoryBIZ();
            int i = 0;
            try
            {
                using (dbConnection)
                {
                    dbTransaction = dbConnection.BeginTransaction();
                    foreach (Guid caseId in aryCaseId)
                    {
                        //*20150609 扣押並結案 不會直接結案
                        CaseMaster masterobj = master.MasterModel(caseId, dbTransaction);
                        if (masterobj.Status == aryStatus[i])
                        {
                            if (masterobj.CaseKind2 == CaseKind2.CaseSeizureAndPay && masterobj.AfterSeizureApproved > 0)
                            {
                                //* 扣押並支付的支付
                                string strSql = @"UPDATE [CaseMaster] 
                                                SET	[ApproveUser2] = @ModifiedUser,
	                                                [ApproveDate2] = GETDATE(),
	                                                [CloseUser] = @ModifiedUser,
	                                                [CloseDate] = GETDATE(),
	                                                [CloseReason] = @CloseReason,
	                                                [ModifiedUser] = @ModifiedUser,
	                                                [ModifiedDate] = GETDATE()
                                                WHERE [CaseId] = @CaseId;";
                                Parameter.Clear();
                                Parameter.Add(new CommandParameter("CaseId", caseId));
                                Parameter.Add(new CommandParameter("ModifiedUser", userId));
                                Parameter.Add(new CommandParameter("CloseReason", "主管放行結案"));

                                bRtn = bRtn && ExecuteNonQuery(strSql, dbTransaction) > 0;
                                bRtn = bRtn && master.UpdateCaseStatus(caseId, CaseStatus.DirectorApprove, userId, dbTransaction) > 0;
                            }
                            else if (masterobj.CaseKind2 == CaseKind2.CaseSeizureAndPay)
                            {
                                //* 扣押並支付案件.的扣押 就會update為一個中間狀態
                                string strSql = @"UPDATE [CaseMaster] 
                                                SET [ApproveUser] = @ModifiedUser,
	                                                [ApproveDate] = GETDATE(),
                                                    [AfterSeizureApproved] = 1,
                                                    [ModifiedUser] = @ModifiedUser,
                                                    [ModifiedDate] = GETDATE()
                                                WHERE [CaseId] = @CaseId";
                                Parameter.Clear();
                                Parameter.Add(new CommandParameter("@CaseId", caseId));
                                Parameter.Add(new CommandParameter("@Status", CaseStatus.DirectorApproveSeizureAndPay));
                                Parameter.Add(new CommandParameter("@ModifiedUser", userId));
                                bRtn = bRtn && ExecuteNonQuery(strSql, dbTransaction) > 0;
                                bRtn = bRtn && master.UpdateCaseStatus(caseId, CaseStatus.DirectorApproveSeizureAndPay, userId, dbTransaction) > 0;

                            }
                            else
                            {
                                //* 其他的一次結案
                                //* 結案
                                var strSql = @"UPDATE [CaseMaster] 
                                    SET	[ApproveUser] = @ModifiedUser,
	                                    [ApproveDate] = GETDATE(),
	                                    [CloseUser] = @ModifiedUser,
	                                    [CloseDate] = GETDATE(),
	                                    [CloseReason] = @CloseReason,
	                                    [ModifiedUser] = @ModifiedUser,
	                                    [ModifiedDate] = GETDATE()
                                    WHERE [CaseId] = @CaseId;";
                                Parameter.Clear();
                                Parameter.Add(new CommandParameter("CaseId", caseId));
                                Parameter.Add(new CommandParameter("ModifiedUser", userId));
                                Parameter.Add(new CommandParameter("CloseReason", "主管放行結案"));

                                bRtn = bRtn && ExecuteNonQuery(strSql, dbTransaction) > 0;
                                bRtn = bRtn && master.UpdateCaseStatus(caseId, CaseStatus.DirectorApprove, userId, dbTransaction) > 0;
                            }
                            string sql = @"if not exists (select CaseId from BatchControl where CaseId = @CaseId) Insert into BatchControl(CaseId,CREATE_TIME) values(@CaseId,getdate())";
                            Parameter.Clear();
                            Parameter.Add(new CommandParameter("@CaseId", caseId));
                            ExecuteNonQuery(sql, dbTransaction);
                            //bRtn = bRtn && ExecuteNonQuery(sql) > 0;
                            i++;
                        }
                        else
                        {
                            return new JsonReturn { ReturnCode = "0", ReturnMsg = "(" + masterobj.CaseNo + ")" + "此案件狀態已變更，請重新查詢！" };
                        }
                    }

                    if (bRtn)
                    {
                        dbTransaction.Commit();
                        return new JsonReturn() { ReturnCode = "1" };
                    }
                    dbTransaction.Rollback();
                    //數據rollback時，刪除CaseHistory記錄，避免案件狀態與流程記錄不一致
                    foreach (Guid caseId in aryCaseId)
                    {
                        var sql = @"delete from CaseHistory where CaseId = @CaseId and Event = @Event and CreatedDate between DATEADD( minute,-30,GETDATE()) and getdate();";
                        Parameter.Clear();
                        Parameter.Add(new CommandParameter("@CaseId", caseId));
                        Parameter.Add(new CommandParameter("@Event", "主管放行結案"));
                        ExecuteNonQuery(sql);
                    }
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
            finally
            {
                staticAryCaseId.Clear();
            }
        }

        /// <summary>
        /// 主管呈核
        /// </summary>
        /// <param name="aryCaseId"></param>
        /// <param name="aryAgentId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public JsonReturn DirectorSubmit(List<String> aryStatus, List<Guid> aryCaseId, List<string> aryAgentId, string userId)
        {
            if (aryCaseId == null || !aryCaseId.Any() || aryAgentId == null || !aryAgentId.Any()) return new JsonReturn() { ReturnCode = "0" };

            DateTime dtNow = GetNowDateTime();
            IDbConnection dbConnection = OpenConnection();
            IDbTransaction dbTransaction = null;
            CaseMasterBIZ master = new CaseMasterBIZ();
            int i = 0;
            try
            {
                using (dbConnection)
                {
                    dbTransaction = dbConnection.BeginTransaction();
                    var strSql = @" UPDATE [CaseAssignTable] SET [AlreadyAssign] = 2, [ModifiedUser] = @ModifiedUser, [ModifiedDate] = GETDATE() WHERE [CaseId] = @CaseId;";
                    foreach (Guid caseId in aryCaseId)
                    {
                        CaseMaster masterobj = master.MasterModel(caseId, dbTransaction);
                        if (masterobj.Status == aryStatus[i])
                        {
                            foreach (string empId in aryAgentId)
                            {
                                strSql += @"INSERT INTO [CaseAssignTable] ([CaseId],[EmpId],[AlreadyAssign],[CreatdUser],[CreatedDate])VALUES (@CaseId,@EmpId,0,@UserId,@CreateDate);";
                                Parameter.Clear();

                                Parameter.Add(new CommandParameter("@CaseId", caseId));
                                Parameter.Add(new CommandParameter("@EmpId", empId));
                                Parameter.Add(new CommandParameter("@UserId", userId));
                                Parameter.Add(new CommandParameter("@CreateDate", dtNow));
                                Parameter.Add(new CommandParameter("@ModifiedUser", userId));
                                ExecuteNonQuery(strSql, dbTransaction);
                            }
                            master.UpdateCaseStatus(caseId, CaseStatus.DirectorSubmit, userId, dbTransaction);
                            i++;
                        }
                        else
                        {
                            return new JsonReturn { ReturnCode = "0", ReturnMsg = "(" + masterobj.CaseNo + ")" + "此案件狀態已變更，請重新查詢！" };
                        }
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

        //20170421 固定 RQ-2015-019666-017 派件至跨單位 宏祥 add start
        /// <summary>
        /// 收發代辦
        /// </summary>
        /// <param name="model"></param>
        /// <param name="aryCaseId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public JsonReturn AssignSet(DirectorToApprove model, List<Guid> aryCaseId, string userId)
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

        /// <summary>
        /// 主管退件
        /// </summary>
        /// <param name="aryCaseId"></param>
        /// <returns></returns>
        public JsonReturn DirectorReturn(DirectorToApprove model, List<Guid> aryCaseId, List<String> aryStatus, string userId)
        {
            if (aryCaseId == null || !aryCaseId.Any()) return new JsonReturn() { ReturnCode = "0" };
            bool bRtn = true;

            CaseMasterBIZ masterBiz = new CaseMasterBIZ();
            CaseHistoryBIZ history = new CaseHistoryBIZ();

            IDbConnection dbConnection = OpenConnection();
            IDbTransaction dbTransaction = null;
            int num = 0;
            try
            {
                using (dbConnection)
                {
                    dbTransaction = dbConnection.BeginTransaction();
                    foreach (Guid caseId in aryCaseId)
                    {
                        CaseMaster masterobj = masterBiz.MasterModel(caseId, dbTransaction);
                        //* >1 說明是扣押並支付的支付 所以把二次呈核的日期置為空

                        DataTable dtCaseStatus = history.getCaseStatus(caseId);
                        string mLastHistory = dtCaseStatus.Rows[0][0].ToString().Trim();

                        string sqlStr = "";
                        if (masterobj.Status == aryStatus[num])
                        {
                            if (masterobj.AfterSeizureApproved > 0 && mLastHistory == "主管放行結案")
                            {
                                sqlStr = "UPDATE [CaseMaster] SET [AgentSubmitDate2] = NULL,[AgentUser2]=NULL,[ApproveUser2]=null,[ApproveDate2]=null WHERE [CaseId] = @CaseId; ";
                            }
                            else
                            {
                                sqlStr = "UPDATE [CaseMaster] SET [AgentSubmitDate] = NULL WHERE [CaseId] = @CaseId; ";
                                sqlStr = sqlStr + @" UPDATE [CaseMaster] set [ApproveDate] = NULL,[CloseDate] = NULL where CaseId=@CaseId ";
                                sqlStr = sqlStr + @" Delete from BatchControl where CaseId=@CaseId";
                                sqlStr = sqlStr + @" Delete from CaseEdocFile where CaseId=@CaseId and Type ='發文'";
                            }

                            sqlStr = sqlStr + @" UPDATE [CaseMaster] SET 
                                                [Status] = @Status, 
                                                [ReturnReason] = @ReturnReason, 
                                                [ModifiedUser] = @ModifiedUser,
                                                [ModifiedDate] = GETDATE()
                                        WHERE [CaseId] = @CaseId";

                            Parameter.Clear();
                            Parameter.Add(new CommandParameter("CaseId", caseId));
                            Parameter.Add(new CommandParameter("Status", CaseStatus.DirectorReturn));
                            Parameter.Add(new CommandParameter("ReturnReason", model.CloseReason));
                            Parameter.Add(new CommandParameter("ModifiedUser", userId));
                            bRtn = bRtn && ExecuteNonQuery(sqlStr, dbTransaction) > 0;



                            //獲取案件狀態

                            string strUpdate = "";
                            if (mLastHistory == "主管放行結案")
                            {
                                strUpdate = " update CaseHistory set Event = '主管放行結案(取消)' where HistoryId = (select top 1 HistoryId from CaseHistory where CaseId = @CaseId order by CreatedDate desc) ";
                            }
                            else if (mLastHistory == "主管-待20日支付")
                            {
                                strUpdate = " update CaseHistory set Event = '主管-待20日支付(取消)' where HistoryId = (select top 1 HistoryId from CaseHistory where CaseId = @CaseId order by CreatedDate desc) ";
                            }
                            Parameter.Clear();
                            Parameter.Add(new CommandParameter("CaseId", caseId));
                            if (strUpdate != "")
                            {
                                int i = ExecuteNonQuery(strUpdate);
                            }

                            if (dtCaseStatus.Rows[0][0].ToString().Trim() == "主管-待20日支付")
                            {
                                Update2(caseId, dbTransaction);
                            }

                            //* 案件歷程表
                            history.insertCaseHistory(caseId, CaseStatus.DirectorReturn, userId, dbTransaction);
                            //* 主管退回原因曆史表

                            string retrunToEmpId = masterobj.AfterSeizureApproved > 0
                                ? masterobj.AgentUser2
                                : masterobj.AgentUser;

                            DirectorReturnHistory(model, caseId, userId, masterobj.AfterSeizureApproved, retrunToEmpId, dbTransaction);
                            num++;
                        }
                        else
                        {
                            return new JsonReturn { ReturnCode = "0", ReturnMsg = "(" + masterobj.CaseNo + ")" + "此案件狀態已變更，請重新查詢！" };
                        }
                    }
                    if (bRtn)
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

        public bool DirectorReturnHistory(DirectorToApprove model, Guid caseId, string userId, int afterSeizureApproved, string returnAgentId, IDbTransaction trans = null)
        {
            string sql = @"  insert into DirectorReturnHistory (CaseId,DirectorId,ReturnDate,ReturnReason,[AfterSeizureApproved],[AgentUser])values(@CaseId,@DirectorId,GetDate(),@ReturnReason,@AfterSeizureApproved,@AgentUser)";
            base.Parameter.Clear();
            base.Parameter.Add(new CommandParameter("CaseId", caseId));
            base.Parameter.Add(new CommandParameter("ReturnReason", model.CloseReason));
            base.Parameter.Add(new CommandParameter("DirectorId", userId));
            base.Parameter.Add(new CommandParameter("AfterSeizureApproved", afterSeizureApproved));
            base.Parameter.Add(new CommandParameter("AgentUser", returnAgentId));
            return trans == null ? ExecuteNonQuery(sql) > 0 : ExecuteNonQuery(sql, trans) > 0;
        }

        public bool Update2(Guid caseId, IDbTransaction trans = null)
        {
            string sql = @" update CaseMaster set AfterSeizureApproved=0 where CaseId=@CaseId";
            base.Parameter.Clear();
            base.Parameter.Add(new CommandParameter("CaseId", caseId));
            return trans == null ? ExecuteNonQuery(sql) > 0 : ExecuteNonQuery(sql, trans) > 0;
        }

        /// <summary>
        /// 獲取電子發文上傳查詢結果
        /// </summary>
        /// <param name="model"></param>
        /// <param name="pageIndex"></param>
        /// <param name="strSortExpression"></param>
        /// <param name="strSortDirection"></param>
        /// <param name="EmpId"></param>
        /// <returns></returns>
        public IList<DirectorToApprove> GetCaseSendUpLoad(DirectorToApprove model, int pageIndex, string strSortExpression, string strSortDirection, string EmpId)
        {
            try
            {
                string sqlWhere = "";
                PageIndex = pageIndex;
                Parameter.Clear();
                Parameter.Add(new CommandParameter("@pageS", (PageSize * (PageIndex - 1)) + 1));
                Parameter.Add(new CommandParameter("@pageE", PageSize * PageIndex));
                if (!string.IsNullOrEmpty(model.CaseNo))
                {
                    sqlWhere += @" and CaseNo like @CaseNo ";
                    Parameter.Add(new CommandParameter("@CaseNo", "%" + model.CaseNo.Trim() + "%"));
                }
                if (!string.IsNullOrEmpty(model.GovUnit))
                {
                    sqlWhere += @" and GovUnit like @GovUnit ";
                    Parameter.Add(new CommandParameter("@GovUnit", "%" + model.GovUnit.Trim() + "%"));
                }
                if (!string.IsNullOrEmpty(model.GovNo))
                {
                    sqlWhere += @" and GovNo like @GovNo ";
                    Parameter.Add(new CommandParameter("@GovNo", "%" + model.GovNo.Trim() + "%"));
                }
                if (!string.IsNullOrEmpty(model.CreateUser))
                {
                    sqlWhere += @" and Person like @Person ";
                    Parameter.Add(new CommandParameter("@Person", "%" + model.CreateUser.Trim() + "%"));
                }
                if (!string.IsNullOrEmpty(model.Speed))
                {
                    sqlWhere += @" and CSS.Speed = @Speed ";
                    Parameter.Add(new CommandParameter("@Speed", model.Speed.Trim()));
                }
                if (!string.IsNullOrEmpty(model.ReceiveKind))
                {
                    sqlWhere += @" and ReceiveKind like @ReceiveKind ";
                    Parameter.Add(new CommandParameter("@ReceiveKind", "%" + model.ReceiveKind.Trim() + "%"));
                }
                if (!string.IsNullOrEmpty(model.CaseKind))
                {
                    sqlWhere += @" and CaseKind like @CaseKind ";
                    Parameter.Add(new CommandParameter("@CaseKind", "%" + model.CaseKind.Trim() + "%"));
                }
                if (!string.IsNullOrEmpty(model.CaseKind2))
                {
                    sqlWhere += @" and CaseKind2 like @CaseKind2 ";
                    Parameter.Add(new CommandParameter("@CaseKind2", "%" + model.CaseKind2.Trim() + "%"));
                }
                if (!string.IsNullOrEmpty(model.GovDateS))
                {
                    sqlWhere += @" AND GovDate >= @GovDateS";
                    Parameter.Add(new CommandParameter("@GovDateS", model.GovDateS));
                }
                if (!string.IsNullOrEmpty(model.GovDateE))
                {
                    string qDateE = Convert.ToDateTime(UtlString.FormatDateString(model.GovDateE)).AddDays(1).ToString("yyyyMMdd");
                    sqlWhere += @" AND GovDate < @GovDateE ";
                    Parameter.Add(new CommandParameter("@GovDateE", qDateE));
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
                if (!string.IsNullOrEmpty(model.SendDateS) || !string.IsNullOrEmpty(model.SendDateE))
                {
                    string sqlWhere1 = "";
                    if (!string.IsNullOrEmpty(model.SendDateS))
                    {
                        model.SendDateS = UtlString.FormatDateTwStringToAd(model.SendDateS);
                        sqlWhere1 += @" AND SendDate >= @SendDateStart";
                        Parameter.Add(new CommandParameter("@SendDateStart", model.SendDateS));
                    }
                    if (!string.IsNullOrEmpty(model.SendDateE))
                    {
                        string sendDateEnd = Convert.ToDateTime(UtlString.FormatDateString(model.SendDateE)).AddDays(1).ToString("yyyyMMdd");
                        sqlWhere1 += @" AND SendDate < @SendDateEnd ";
                        Parameter.Add(new CommandParameter("@SendDateEnd", sendDateEnd));
                    }
                    sqlWhere += " AND CM.CaseId IN (SELECT CaseId  FROM CaseSendSetting AS CSS where 1=1 " + sqlWhere1 + @") ";
                }
                if (!string.IsNullOrEmpty(model.Unit))
                {
                    sqlWhere += @" and Unit like @Unit ";
                    Parameter.Add(new CommandParameter("@Unit", "%" + model.Unit.Trim() + "%"));
                }
                if (!string.IsNullOrEmpty(model.CreatedDateS))
                {
                    sqlWhere += @" AND CM.CreatedDate >= @CreatedDateS";
                    Parameter.Add(new CommandParameter("@CreatedDateS", model.CreatedDateS));
                }
                if (!string.IsNullOrEmpty(model.CreatedDateE))
                {
                    string createdDateE = Convert.ToDateTime(UtlString.FormatDateString(model.CreatedDateE)).AddDays(1).ToString("yyyy/MM/dd");
                    sqlWhere += @" AND CM.CreatedDate < @CreatedDateE ";
                    Parameter.Add(new CommandParameter("@CreatedDateE", createdDateE));
                }
                string strSql = @";with T1 
	                        as
	                        (
		                       SELECT distinct CM.CaseId,CM.CaseNo,CM.GovUnit,(SELECT CONVERT(varchar(100), CM.GovDate, 111)) as GovDate,CM.GovNo,CM.Person,CM.AgentUser,CM.CaseKind,CM.CaseKind2,CSS.Speed, (SELECT TOP 1 CONVERT(varchar(12),SendDate,111) FROM CaseSendSetting where CaseId = CM.CaseId ORDER BY CreatedDate desc) AS SendDate,(SELECT CONVERT(varchar(100), CM.LimitDate, 111)) as LimitDate FROM CaseMaster AS CM
                                inner join CaseSendSetting AS CSS ON CM.CaseId=CSS.CaseId
                                WHERE CSS.SendKind='電子發文' and CM.CaseId in (select CaseId from BatchControl where STATUS_Transfer=0 and STATUS_Create=1) " + sqlWhere + @" 
	                        ),T2 as
	                        (
		                        select *, row_number() over (order by LimitDate ASC , CaseNo ASC ) RowNum
		                        from T1
	                        ),T3 as 
	                        (
		                        select *,(select max(RowNum) from T2) maxnum from T2 
		                        where rownum between @pageS and @pageE
	                        )
	                        select a.* from T3 a order by a.RowNum";

                IList<DirectorToApprove> ilst = SearchList<DirectorToApprove>(strSql);
                if (ilst != null)
                {
                    if (ilst.Count > 0)
                    {
                        DataRecords = ilst[0].maxnum;
                    }
                    else
                    {
                        DataRecords = 0;
                        ilst = new List<DirectorToApprove>();
                    }
                    return ilst;
                }
                return new List<DirectorToApprove>();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 放行案件查詢
        /// </summary>
        /// <param name="model"></param>
        /// <param name="pageIndex"></param>
        /// <param name="strSortExpression"></param>
        /// <param name="strSortDirection"></param>
        /// <param name="EmpId"></param>
        /// <returns></returns>
        public IList<DirectorToApprove> GetApprovedCase(DirectorToApprove model, int pageIndex, string strSortExpression, string strSortDirection, string EmpId)
        {
            try
            {
                string sqlWhere = "";
                PageIndex = pageIndex;
                Parameter.Clear();
                Parameter.Add(new CommandParameter("@pageS", (PageSize * (PageIndex - 1)) + 1));
                Parameter.Add(new CommandParameter("@pageE", PageSize * PageIndex));
                if (!string.IsNullOrEmpty(model.CaseNo))
                {
                    sqlWhere += @" and CaseNo like @CaseNo ";
                    Parameter.Add(new CommandParameter("@CaseNo", "%" + model.CaseNo.Trim() + "%"));
                }
                if (!string.IsNullOrEmpty(model.GovUnit))
                {
                    sqlWhere += @" and GovUnit like @GovUnit ";
                    Parameter.Add(new CommandParameter("@GovUnit", "%" + model.GovUnit.Trim() + "%"));
                }
                if (!string.IsNullOrEmpty(model.GovNo))
                {
                    sqlWhere += @" and GovNo like @GovNo ";
                    Parameter.Add(new CommandParameter("@GovNo", "%" + model.GovNo.Trim() + "%"));
                }
                if (!string.IsNullOrEmpty(model.CreateUser))
                {
                    sqlWhere += @" and Person like @Person ";
                    Parameter.Add(new CommandParameter("@Person", "%" + model.CreateUser.Trim() + "%"));
                }
                if (!string.IsNullOrEmpty(model.Speed))
                {
                    sqlWhere += @" and A.Speed = @Speed ";
                    Parameter.Add(new CommandParameter("@Speed", model.Speed.Trim()));
                }
                if (!string.IsNullOrEmpty(model.ReceiveKind))
                {
                    sqlWhere += @" and ReceiveKind like @ReceiveKind ";
                    Parameter.Add(new CommandParameter("@ReceiveKind", "%" + model.ReceiveKind.Trim() + "%"));
                }
                if (!string.IsNullOrEmpty(model.CaseKind))
                {
                    sqlWhere += @" and CaseKind like @CaseKind ";
                    Parameter.Add(new CommandParameter("@CaseKind", "%" + model.CaseKind.Trim() + "%"));
                }
                if (!string.IsNullOrEmpty(model.CaseKind2))
                {
                    sqlWhere += @" and CaseKind2 like @CaseKind2 ";
                    Parameter.Add(new CommandParameter("@CaseKind2", "%" + model.CaseKind2.Trim() + "%"));
                }
                if (!string.IsNullOrEmpty(model.GovDateS))
                {
                    sqlWhere += @" AND GovDate >= @GovDateS";
                    Parameter.Add(new CommandParameter("@GovDateS", model.GovDateS));
                }
                if (!string.IsNullOrEmpty(model.GovDateE))
                {
                    string qDateE = Convert.ToDateTime(UtlString.FormatDateString(model.GovDateE)).AddDays(1).ToString("yyyy/MM/dd");
                    sqlWhere += @" AND GovDate < @GovDateE ";
                    Parameter.Add(new CommandParameter("@GovDateE", qDateE));
                }
                if (!string.IsNullOrEmpty(model.SendKind))
                {
                    sqlWhere += @" and SendKind like @SendKind ";
                    base.Parameter.Add(new CommandParameter("@SendKind", "%" + model.SendKind + "%"));
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
                if (!string.IsNullOrEmpty(model.SendDateS))
                {
                    sqlWhere += @" AND SendDate >= @SendDateS";
                    Parameter.Add(new CommandParameter("@SendDateS", model.SendDateS));
                }
                if (!string.IsNullOrEmpty(model.SendDateE))
                {
                    string qDateE = Convert.ToDateTime(UtlString.FormatDateString(model.SendDateE)).AddDays(1).ToString("yyyy/MM/dd");
                    sqlWhere += @" AND SendDate < @SendDateE ";
                    Parameter.Add(new CommandParameter("@SendDateE", qDateE));
                }
                //if (!string.IsNullOrEmpty(model.SendDateS) || !string.IsNullOrEmpty(model.SendDateE))
                //{
                //    string sqlWhere1 = "";
                //    if (!string.IsNullOrEmpty(model.SendDateS))
                //    {
                //        model.SendDateS = UtlString.FormatDateTwStringToAd(model.SendDateS);
                //        sqlWhere1 += @" AND SendDate >= @SendDateStart";
                //        Parameter.Add(new CommandParameter("@SendDateStart", model.SendDateS));
                //    }
                //    if (!string.IsNullOrEmpty(model.SendDateE))
                //    {
                //        string sendDateEnd = Convert.ToDateTime(UtlString.FormatDateString(model.SendDateE)).AddDays(1).ToString("yyyyMMdd");
                //        sqlWhere1 += @" AND SendDate < @SendDateEnd ";
                //        Parameter.Add(new CommandParameter("@SendDateEnd", sendDateEnd));
                //    }
                //    sqlWhere += " AND A.CaseId IN (SELECT CaseId  FROM CaseSendSetting AS CSS where 1=1 " + sqlWhere1 + @") ";
                //}
                if (!string.IsNullOrEmpty(model.Unit))
                {
                    sqlWhere += @" and Unit like @Unit ";
                    Parameter.Add(new CommandParameter("@Unit", "%" + model.Unit.Trim() + "%"));
                }
                //主管放行日
                if (!string.IsNullOrEmpty(model.ApproveDateS))
                {
                    sqlWhere += @" AND ApproveDate2 >= @ApproveDateStart";
                    Parameter.Add(new CommandParameter("@ApproveDateStart", model.ApproveDateS));
                }
                if (!string.IsNullOrEmpty(model.ApproveDateE))
                {
                    string approveDateEnd = Convert.ToDateTime(UtlString.FormatDateString(model.ApproveDateE)).AddDays(1).ToString("yyyy/MM/dd");
                    sqlWhere += @" AND ApproveDate2 < @ApproveDateEnd ";
                    Parameter.Add(new CommandParameter("@ApproveDateEnd", approveDateEnd));
                }
                //放行主管
                if (!string.IsNullOrEmpty(model.ApproveManager))
                {
                    sqlWhere += @" and (ApproveUser = @ApproveManager or ApproveUser2 = @ApproveManager)";
                    base.Parameter.Add(new CommandParameter("@ApproveManager", model.ApproveManager));
                }
                string strSql = @"  ;with T1 
	                        as
	                        (
		                       SELECT DISTINCT
                                        B.SerialID,
                                        A.CaseId,
	                                    A.CaseNo,
	                                    A.GovUnit,
	                                    CONVERT(varchar(100), A.GovDate, 111) as GovDate,
	                                    A.GovNo,
                                       --20170714 固定 RQ-2015-019666-019 派件至跨單位(廠商bug一併修正) 宏祥 add start--
                                       A.Status,
                                       --20170714 固定 RQ-2015-019666-019 派件至跨單位(廠商bug一併修正) 宏祥 add end--
                                        C.EmpName AS AgentUser,
	                                    CASE WHEN (A.CaseKind2 = '扣押並支付' AND B.Template = '支付') THEN E.EmpName ELSE D.EmpName END AS ApproveManager,
	                                    CONVERT(varchar(100), B.SendDate, 111) as SendDate,
                                        A.CaseKind,
                                        A.CaseKind2,
	                                    A.Speed,
                                        CONVERT(varchar(100), B.SendUpDate, 111) as SendUpDate,
                                        --(select top 1 MailNo from MailInfo where CaseId=A.CaseId) as MailNo,
										M.MailNo,
										convert(varchar(8),A.LimitDate,112) as LimitDate
                                    FROM CaseMaster AS A
                                    INNER JOIN CaseSendSetting AS B ON A.CaseId = B.CaseId
                                    LEFT OUTER JOIN [V_AgentAndDept] AS C ON C.EmpID = A.AgentUser
                                    LEFT OUTER JOIN [V_AgentAndDept] AS D ON D.EmpID = A.ApproveUser
                                    LEFT OUTER JOIN [V_AgentAndDept] AS E ON E.EmpID = A.ApproveUser2
									LEFT OUTER JOIN CaseSendSettingDetails AS F ON B.SerialID = F.SerialID
									LEFT OUTER JOIN MailInfo AS M on F.DetailsId = M.SendDetailId
                            WHERE  A.Status in ('D03','Z01')
	                            AND
                            (    
                                (
                                    (A.CaseKind2 <> '扣押並支付' OR (A.CaseKind2 = '扣押並支付' AND B.Template = '扣押'))
                                    AND A.ApproveDate IS NOT NULL 
                                    " + sqlWhere.Replace("ApproveDate2", "ApproveDate") + @"
                                )
                                OR
                                (
                                    A.CaseKind2 = '扣押並支付' 
                                    AND B.Template = '支付'
                                    AND A.ApproveDate IS NOT NULL 
                                    AND A.ApproveDate2 IS NOT NULL 
                                    " + sqlWhere + @"
                                )  
                            ) 
	                        ),T2 as
	                        (
		                        select *, row_number() over (order by CaseNo ASC ) RowNum
		                        from T1
	                        ),T3 as 
	                        (
		                        select *,(select max(RowNum) from T2) maxnum from T2 
		                        where rownum between @pageS and @pageE
	                        )
	                        select a.* from T3 a order by a.RowNum  	
                            ";
                IList<DirectorToApprove> ilst = SearchList<DirectorToApprove>(strSql);
                if (ilst != null)
                {
                    if (ilst.Count > 0)
                    {
                        DataRecords = ilst[0].maxnum;
                    }
                    else
                    {
                        DataRecords = 0;
                        ilst = new List<DirectorToApprove>();
                    }
                    return ilst;
                }
                return new List<DirectorToApprove>();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //20170630 緊急 RQ-2015-019666-018 派件至跨單位 宏祥 add start
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
        //20170630 緊急 RQ-2015-019666-018 派件至跨單位 宏祥 add end
        /// <summary>
        /// 整批放行
        /// </summary>
        /// <param name="aryStatus"></param>
        /// <param name="aryCaseId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public JsonReturn DirectorBatchApprove(List<String> aryStatus, List<Guid> aryCaseId, string userId)
        {
            if (aryCaseId == null || !aryCaseId.Any()) return new JsonReturn() { ReturnCode = "0" };
            //并發驗證
            foreach (var item in aryCaseId)
            {
                if (staticAryCaseId != null && staticAryCaseId.Contains(item))
                {
                    return new JsonReturn { ReturnCode = "0", ReturnMsg = "(" + item + ")" + "此案件正在處理中，請重新查詢！" };
                }
                else { staticAryCaseId.Add(item); }
            }
            bool bRtn = true;
            IDbConnection dbConnection = OpenConnection();
            IDbTransaction dbTransaction = null;
            CaseMasterBIZ master = new CaseMasterBIZ();
            CaseHistoryBIZ history = new CaseHistoryBIZ();
            int i = 0;
            try
            {
                using (dbConnection)
                {
                    dbTransaction = dbConnection.BeginTransaction();
                    foreach (Guid caseId in aryCaseId)
                    {
                        //*20150609 扣押並結案 不會直接結案
                        CaseMaster masterobj = master.MasterModel(caseId, dbTransaction);
                        if (masterobj.Status == aryStatus[i])
                        {
                            if (masterobj.CaseKind2 == CaseKind2.CaseSeizureAndPay && masterobj.AfterSeizureApproved > 0)
                            {
                                //* 扣押並支付的支付
                                string strSql = @"UPDATE [CaseMaster] 
                                                SET	[ApproveUser2] = @ModifiedUser,
	                                                [ApproveDate2] = GETDATE(),
	                                                [CloseUser] = @ModifiedUser,
	                                                [CloseDate] = GETDATE(),
	                                                [CloseReason] = @CloseReason,
	                                                [ModifiedUser] = @ModifiedUser,
	                                                [ModifiedDate] = GETDATE()
                                                WHERE [CaseId] = @CaseId;";
                                Parameter.Clear();
                                Parameter.Add(new CommandParameter("CaseId", caseId));
                                Parameter.Add(new CommandParameter("ModifiedUser", userId));
                                Parameter.Add(new CommandParameter("CloseReason", "主管放行結案"));

                                bRtn = bRtn && ExecuteNonQuery(strSql, dbTransaction) > 0;
                                bRtn = bRtn && UpdateCaseStatus(caseId, CaseStatus.DirectorApprove, userId, dbTransaction) > 0;
                            }
                            else if (masterobj.CaseKind2 == CaseKind2.CaseSeizureAndPay)
                            {
                                //* 扣押並支付案件.的扣押 就會update為一個中間狀態
                                string strSql = @"UPDATE [CaseMaster] 
                                                SET [ApproveUser] = @ModifiedUser,
	                                                [ApproveDate] = GETDATE(),
                                                    [AfterSeizureApproved] = 1,
                                                    [ModifiedUser] = @ModifiedUser,
                                                    [ModifiedDate] = GETDATE()
                                                WHERE [CaseId] = @CaseId";
                                Parameter.Clear();
                                Parameter.Add(new CommandParameter("@CaseId", caseId));
                                Parameter.Add(new CommandParameter("@Status", CaseStatus.DirectorApproveSeizureAndPay));
                                Parameter.Add(new CommandParameter("@ModifiedUser", userId));
                                bRtn = bRtn && ExecuteNonQuery(strSql, dbTransaction) > 0;
                                bRtn = bRtn && UpdateCaseStatus(caseId, CaseStatus.DirectorApproveSeizureAndPay, userId, dbTransaction) > 0;

                            }
                            else
                            {
                                //* 其他的一次結案
                                //* 結案
                                var strSql = @"UPDATE [CaseMaster] 
                                    SET	[ApproveUser] = @ModifiedUser,
	                                    [ApproveDate] = GETDATE(),
	                                    [CloseUser] = @ModifiedUser,
	                                    [CloseDate] = GETDATE(),
	                                    [CloseReason] = @CloseReason,
	                                    [ModifiedUser] = @ModifiedUser,
	                                    [ModifiedDate] = GETDATE()
                                    WHERE [CaseId] = @CaseId;";
                                Parameter.Clear();
                                Parameter.Add(new CommandParameter("CaseId", caseId));
                                Parameter.Add(new CommandParameter("ModifiedUser", userId));
                                Parameter.Add(new CommandParameter("CloseReason", "主管放行結案"));

                                bRtn = bRtn && ExecuteNonQuery(strSql, dbTransaction) > 0;
                                bRtn = bRtn && UpdateCaseStatus(caseId, CaseStatus.DirectorApprove, userId, dbTransaction) > 0;
                            }
                            string sql = @"if not exists (select CaseId from BatchControl where CaseId = @CaseId) Insert into BatchControl(CaseId,CREATE_TIME) values(@CaseId,getdate())";
                            Parameter.Clear();
                            Parameter.Add(new CommandParameter("@CaseId", caseId));
                            ExecuteNonQuery(sql, dbTransaction);
                            //bRtn = bRtn && ExecuteNonQuery(sql) > 0;
                            i++;
                        }
                        else
                        {
                            return new JsonReturn { ReturnCode = "0", ReturnMsg = "(" + masterobj.CaseNo + ")" + "此案件狀態已變更，請重新查詢！" };
                        }
                    }

                    if (bRtn)
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
            finally
            {
                staticAryCaseId.Clear();
            }
        }
        public MemoryStream Excel(string[] headerColumns, DirectorToApprove model, string EmpId)
        {
            var ms = new MemoryStream();

            IList<DirectorToApprove> ilst = GetPrintData(model,EmpId);

            if (ilst != null)
            {
                ms = ExcelExport(ilst, headerColumns,
                                                   delegate (HSSFRow dataRow, DirectorToApprove dataItem)
                                                   {
                                                       //* 這裡可以針對每一個欄位做額外處理.比如日期
                                                       dataRow.CreateCell(0).SetCellValue(dataItem.CaseNo);
                                                       dataRow.CreateCell(1).SetCellValue(dataItem.GovUnit);
                                                       dataRow.CreateCell(2).SetCellValue(dataItem.GovDate);
                                                       dataRow.CreateCell(3).SetCellValue(dataItem.GovNo);
                                                       dataRow.CreateCell(4).SetCellValue(dataItem.AgentUser);
                                                       dataRow.CreateCell(5).SetCellValue(dataItem.SendDate);
                                                       dataRow.CreateCell(6).SetCellValue(dataItem.CaseKind);
                                                       dataRow.CreateCell(7).SetCellValue(dataItem.CaseKind2);
                                                       dataRow.CreateCell(8).SetCellValue(dataItem.Speed);
                                                       dataRow.CreateCell(9).SetCellValue(dataItem.LimitDate);
                                                   });
            }
            return ms;
        }
        public MemoryStream ExcelForSeizure(string[] headerColumns, DirectorToApprove model, string EmpId,string strManageType)
        {
            var ms = new MemoryStream();

            IList<RemitSeizure> ilst = GetPrintDataForSeizure(model, EmpId, strManageType);

            //1. 只有10碼ID-->ID顯示在「身份證統一編號」、 戶名顯示在「義務人」
            //2. 只有8碼ID-->ID顯示在「營利事業編號」、戶名顯示在「義務人」;
            //3. 同時有8碼ID 及10碼ID-->8碼ID顯示在「營利事業編號」、戶名顯示在「義務人」;  10碼ID顯示在「身份證統一編號」、戶名顯示在「法定代理人」
            if(ilst != null && ilst.Any())
            {
                foreach(var item in ilst)
                {
                    if(!string.IsNullOrEmpty(item.ObligorNo) && item.ObligorNo.Trim().Length == 8)
                    {
                        item.RegistrationNo = item.ObligorNo;
                        item.ObligorNo = "";
                    }
                }
            }

            if (ilst != null)
            {
                ms = ExcelExport(ilst, headerColumns,
                                                   delegate (HSSFRow dataRow, RemitSeizure dataItem)
                                                   {
                                                       //* 這裡可以針對每一個欄位做額外處理.比如日期
                                                       dataRow.CreateCell(0).SetCellValue(dataItem.CaseKind2);
                                                       dataRow.CreateCell(1).SetCellValue(dataItem.CaseNo);
                                                       dataRow.CreateCell(2).SetCellValue(dataItem.ObligorName);//義務人
                                                       dataRow.CreateCell(3).SetCellValue(dataItem.Agent);//法定代理人
                                                       dataRow.CreateCell(4).SetCellValue(dataItem.ObligorNo);//身份證統一編號
                                                       dataRow.CreateCell(5).SetCellValue(dataItem.RegistrationNo);//營利事業編號
                                                       dataRow.CreateCell(6).SetCellValue(dataItem.ReceiveAmount);
                                                       dataRow.CreateCell(7).SetCellValue(!string.IsNullOrEmpty(dataItem.Account) ? dataItem.Account : "於本行無存款往來");
                                                       dataRow.CreateCell(8).SetCellValue(dataItem.Currency);
                                                       dataRow.CreateCell(9).SetCellValue(dataItem.SeizureAmount);
                                                       dataRow.CreateCell(10).SetCellValue(dataItem.AvailBalance);
                                                       dataRow.CreateCell(11).SetCellValue(dataItem.AgentUser);
                                                       dataRow.CreateCell(12).SetCellValue(dataItem.CreatedDate);
                                                       dataRow.CreateCell(13).SetCellValue(!string.IsNullOrEmpty(dataItem.Account) ? dataItem.Memo : dataItem.Memo + " 於本行無存款往來");//IR-6037
                                                   });
            }
            return ms;
        }

        public MemoryStream ExcelForUndo(string[] headerColumns, DirectorToApprove model, string EmpId, string strManageType)
        {
            var ms = new MemoryStream();

            IList<RemitSeizure> ilst = GetPrintDataForUndo(model, EmpId,strManageType);

            if (ilst != null)
            {
                ms = ExcelExport(ilst, headerColumns,
                                                   delegate (HSSFRow dataRow, RemitSeizure dataItem)
                                                   {
                                                       //* 這裡可以針對每一個欄位做額外處理.比如日期
                                                       dataRow.CreateCell(0).SetCellValue(dataItem.CaseKind2);
                                                       dataRow.CreateCell(1).SetCellValue(dataItem.CaseNo);
                                                       dataRow.CreateCell(2).SetCellValue(!string.IsNullOrEmpty(dataItem.Account) ? dataItem.Account : "查無資訊");//IR-6035
                                                       dataRow.CreateCell(3).SetCellValue(!string.IsNullOrEmpty(dataItem.Currency) ? dataItem.Currency : "查無資訊");//IR-6035
                                                       dataRow.CreateCell(4).SetCellValue(dataItem.SendNo);
                                                       dataRow.CreateCell(5).SetCellValue(!string.IsNullOrEmpty(dataItem.SeizureAmount) ? dataItem.SeizureAmount : "0");//IR-6035
                                                       dataRow.CreateCell(6).SetCellValue(!string.IsNullOrEmpty(dataItem.CancelAmount) ? dataItem.CancelAmount : "0");//IR-6035
                                                       dataRow.CreateCell(7).SetCellValue(dataItem.AgentUser);
                                                       dataRow.CreateCell(8).SetCellValue(dataItem.CreatedDate);
                                                       dataRow.CreateCell(9).SetCellValue(dataItem.Memo);
                                                   });
            }
            return ms;
        }
        /// <summary>
        /// 得到畫面上數據
        /// </summary>
        /// <param name="model"></param>
        /// <param name="EmpId"></param>
        /// <returns></returns>
        public IList<DirectorToApprove> GetPrintData(DirectorToApprove model, string EmpId)
        {
            try
            {
                string sqlWhere = "";
                Parameter.Clear();

                if (!string.IsNullOrEmpty(model.CaseNo))
                {
                    sqlWhere += @" and CaseNo like @CaseNo ";
                    Parameter.Add(new CommandParameter("@CaseNo", "%" + model.CaseNo.Trim() + "%"));
                }
                if (!string.IsNullOrEmpty(model.GovUnit))
                {
                    sqlWhere += @" and GovUnit like @GovUnit ";
                    Parameter.Add(new CommandParameter("@GovUnit", "%" + model.GovUnit.Trim() + "%"));
                }
                if (!string.IsNullOrEmpty(model.GovNo))
                {
                    sqlWhere += @" and GovNo like @GovNo ";
                    Parameter.Add(new CommandParameter("@GovNo", "%" + model.GovNo.Trim() + "%"));
                }
                if (!string.IsNullOrEmpty(model.CreateUser))
                {
                    sqlWhere += @" and Person like @Person ";
                    Parameter.Add(new CommandParameter("@Person", "%" + model.CreateUser.Trim() + "%"));
                }
                if (!string.IsNullOrEmpty(model.SendKind))
                {
                    sqlWhere += @" and SendKind like @SendKind ";
                    Parameter.Add(new CommandParameter("@SendKind", model.SendKind.Trim()));
                }
                if (!string.IsNullOrEmpty(model.Speed))
                {
                    sqlWhere += @" and B.Speed = @Speed ";
                    Parameter.Add(new CommandParameter("@Speed", model.Speed.Trim()));
                }
                if (!string.IsNullOrEmpty(model.ReceiveKind))
                {
                    sqlWhere += @" and ReceiveKind like @ReceiveKind ";
                    Parameter.Add(new CommandParameter("@ReceiveKind", "%" + model.ReceiveKind.Trim() + "%"));
                }
                if (!string.IsNullOrEmpty(model.CaseKind))
                {
                    sqlWhere += @" and CaseKind like @CaseKind ";
                    Parameter.Add(new CommandParameter("@CaseKind", "%" + model.CaseKind.Trim() + "%"));
                }
                if (!string.IsNullOrEmpty(model.CaseKind2))
                {
                    sqlWhere += @" and CaseKind2 like @CaseKind2 ";
                    Parameter.Add(new CommandParameter("@CaseKind2", "%" + model.CaseKind2.Trim() + "%"));
                }
                if (!string.IsNullOrEmpty(model.GovDateS))
                {
                    sqlWhere += @" AND GovDate >= @GovDateS";
                    Parameter.Add(new CommandParameter("@GovDateS", model.GovDateS));
                }
                if (!string.IsNullOrEmpty(model.GovDateE))
                {
                    string qDateE = Convert.ToDateTime(UtlString.FormatDateString(model.GovDateE)).AddDays(1).ToString("yyyyMMdd");
                    sqlWhere += @" AND GovDate < @GovDateE ";
                    Parameter.Add(new CommandParameter("@GovDateE", qDateE));
                }
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
                    base.Parameter.Add(new CommandParameter("@ManagerID", "%" + EmpId + "%"));
                }

                if (!string.IsNullOrEmpty(model.SendDateS) || !string.IsNullOrEmpty(model.SendDateE))
                {
                    string sqlWhere1 = "";
                    if (!string.IsNullOrEmpty(model.SendDateS))
                    {
                        model.SendDateS = UtlString.FormatDateTwStringToAd(model.SendDateS);
                        sqlWhere1 += @" AND SendDate >= @SendDateStart";
                        Parameter.Add(new CommandParameter("@SendDateStart", model.SendDateS));
                    }
                    if (!string.IsNullOrEmpty(model.SendDateE))
                    {
                        string sendDateEnd = Convert.ToDateTime(UtlString.FormatDateString(model.SendDateE)).AddDays(1).ToString("yyyyMMdd");
                        sqlWhere1 += @" AND SendDate < @SendDateEnd ";
                        Parameter.Add(new CommandParameter("@SendDateEnd", sendDateEnd));
                    }
                    sqlWhere += " AND B.CaseId IN (SELECT CaseId  FROM CaseSendSetting AS CSS where 1=1 " + sqlWhere1 + @") ";
                }
                if (!string.IsNullOrEmpty(model.Unit))
                {
                    sqlWhere += @" and Unit like @Unit ";
                    Parameter.Add(new CommandParameter("@Unit", "%" + model.Unit.Trim() + "%"));
                }
                if (!string.IsNullOrEmpty(model.CreatedDateS))
                {
                    sqlWhere += @" AND B.CreatedDate >= @CreatedDateS";
                    Parameter.Add(new CommandParameter("@CreatedDateS", model.CreatedDateS));
                }
                if (!string.IsNullOrEmpty(model.CreatedDateE))
                {
                    string createdDateE = Convert.ToDateTime(UtlString.FormatDateString(model.CreatedDateE)).AddDays(1).ToString("yyyy/MM/dd");
                    sqlWhere += @" AND B.CreatedDate < @CreatedDateE ";
                    Parameter.Add(new CommandParameter("@CreatedDateE", createdDateE));
                }
                string strSql = @";with T1 
	                        as
	                        (
		                       SELECT distinct B.Status,B.CaseId,B.CaseNo,B.GovUnit,(SELECT CONVERT(varchar(100), B.GovDate, 111)) as GovDate,B.GovNo,B.Person,B.AgentUser,B.CaseKind,B.CaseKind2,B.Speed, (SELECT TOP 1 CONVERT(varchar(12),m.SendDate,111) FROM CaseSendSetting m where m.CaseId = B.CaseId ORDER BY CreatedDate desc) AS SendDate,(SELECT CONVERT(varchar(100), B.LimitDate, 111)) as LimitDate FROM CaseAssignTable AS A
                                LEFT OUTER JOIN CaseMaster AS B ON A.CaseId=B.CaseId left join CaseSendSetting CSS on CSS.CaseId=B.CaseId
                                WHERE A.AlreadyAssign=0 AND 
                                (B.Status=@Status1 OR B.Status=@Status2) AND B.isDelete='0' " + sqlWhere + @" 
	                        ),T2 as
	                        (
		                        select *, row_number() over (order by LimitDate ASC , CaseNo ASC ) RowNum
		                        from T1
	                        ),T3 as 
	                        (
		                        select *,(select max(RowNum) from T2) maxnum from T2 
	                        )
	                        select a.* from T3 a order by a.RowNum";

                //Parameter.Add(new CommandParameter("EmpId", EmpId));
                Parameter.Add(new CommandParameter("Status1", CaseStatus.AgentSubmit));
                Parameter.Add(new CommandParameter("Status2", CaseStatus.DirectorSubmit));

                IList<DirectorToApprove> ilst = SearchList<DirectorToApprove>(strSql);
                if (ilst != null)
                {
                    if (ilst.Count > 0)
                    {
                        DataRecords = ilst[0].maxnum;
                    }
                    else
                    {
                        DataRecords = 0;
                        ilst = new List<DirectorToApprove>();
                    }
                    return ilst;
                }
                return new List<DirectorToApprove>();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <param name="EmpId"></param>
        /// <param name="strManageType">0待核決1協同作業2自動化審核</param>
        /// <returns></returns>
        public IList<RemitSeizure> GetPrintDataForSeizure(DirectorToApprove model, string EmpId, string strManageType)
        {
            try
            {
                string sqlWhere = "";
                string sqlwhere1 = "";
                Parameter.Clear();

                if (!string.IsNullOrEmpty(model.CaseNo))
                {
                    sqlWhere += @" and B.CaseNo like @CaseNo ";
                    Parameter.Add(new CommandParameter("@CaseNo", "%" + model.CaseNo.Trim() + "%"));
                }
                if (!string.IsNullOrEmpty(model.GovUnit))
                {
                    sqlWhere += @" and GovUnit like @GovUnit ";
                    Parameter.Add(new CommandParameter("@GovUnit", "%" + model.GovUnit.Trim() + "%"));
                }
                if (!string.IsNullOrEmpty(model.GovNo))
                {
                    sqlWhere += @" and GovNo like @GovNo ";
                    Parameter.Add(new CommandParameter("@GovNo", "%" + model.GovNo.Trim() + "%"));
                }
                if (!string.IsNullOrEmpty(model.CreateUser))
                {
                    sqlWhere += @" and Person like @Person ";
                    Parameter.Add(new CommandParameter("@Person", "%" + model.CreateUser.Trim() + "%"));
                }
                if (!string.IsNullOrEmpty(model.SendKind))
                {
                    sqlWhere += @" and ReceiveKind like @SendKind ";
                    Parameter.Add(new CommandParameter("@SendKind", model.SendKind.Trim()));
                }
                if (!string.IsNullOrEmpty(model.Speed))
                {
                    sqlWhere += @" and B.Speed = @Speed ";
                    Parameter.Add(new CommandParameter("@Speed", model.Speed.Trim()));
                }
                //if (!string.IsNullOrEmpty(model.ReceiveKind))
                //{
                //    sqlwhere1 += @" and ReceiveKind like @ReceiveKind ";
                //    Parameter.Add(new CommandParameter("@ReceiveKind", "%" + model.ReceiveKind.Trim() + "%"));
                //}
                if (!string.IsNullOrEmpty(model.CaseKind))
                {
                    sqlWhere += @" and CaseKind like @CaseKind ";
                    Parameter.Add(new CommandParameter("@CaseKind", "%" + model.CaseKind.Trim() + "%"));
                }
                if (!string.IsNullOrEmpty(model.CaseKind2))//扣押和扣押并支付要區分開 IR-0087
                {
                    sqlWhere += @" and CaseKind2 = @CaseKind2 ";
                    Parameter.Add(new CommandParameter("@CaseKind2", model.CaseKind2.Trim()));
                }
                if (!string.IsNullOrEmpty(model.GovDateS))
                {
                    sqlWhere += @" AND B.GovDate >= @GovDateS";
                    Parameter.Add(new CommandParameter("@GovDateS", model.GovDateS));
                }
                if (!string.IsNullOrEmpty(model.GovDateE))
                {
                    string qDateE = Convert.ToDateTime(UtlString.FormatDateString(model.GovDateE)).AddDays(1).ToString("yyyyMMdd");
                    sqlWhere += @" AND B.GovDate < @GovDateE ";
                    Parameter.Add(new CommandParameter("@GovDateE", qDateE));
                }
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
                    base.Parameter.Add(new CommandParameter("@ManagerID", "%" + EmpId + "%"));
                }

                if (!string.IsNullOrEmpty(model.SendDateS) || !string.IsNullOrEmpty(model.SendDateE))
                {
                    string sqlWhere1 = "";
                    if (!string.IsNullOrEmpty(model.SendDateS))
                    {
                        model.SendDateS = UtlString.FormatDateTwStringToAd(model.SendDateS);
                        sqlWhere1 += @" AND SendDate >= @SendDateStart";
                        Parameter.Add(new CommandParameter("@SendDateStart", model.SendDateS));
                    }
                    if (!string.IsNullOrEmpty(model.SendDateE))
                    {
                        string sendDateEnd = Convert.ToDateTime(UtlString.FormatDateString(model.SendDateE)).AddDays(1).ToString("yyyyMMdd");
                        sqlWhere1 += @" AND SendDate < @SendDateEnd ";
                        Parameter.Add(new CommandParameter("@SendDateEnd", sendDateEnd));
                    }
                    sqlWhere += " AND B.CaseId IN (SELECT CaseId  FROM CaseSendSetting AS CSS where 1=1 " + sqlWhere1 + @") ";
                }
                if (!string.IsNullOrEmpty(model.Unit))
                {
                    sqlWhere += @" and Unit like @Unit ";
                    Parameter.Add(new CommandParameter("@Unit", "%" + model.Unit.Trim() + "%"));
                }
                if (!string.IsNullOrEmpty(model.CreatedDateS))
                {
                    sqlWhere += @" AND B.CreatedDate >= @CreatedDateS";
                    Parameter.Add(new CommandParameter("@CreatedDateS", model.CreatedDateS));
                }
                if (!string.IsNullOrEmpty(model.CreatedDateE))
                {
                    string createdDateE = Convert.ToDateTime(UtlString.FormatDateString(model.CreatedDateE)).AddDays(1).ToString("yyyy/MM/dd");
                    sqlWhere += @" AND B.CreatedDate < @CreatedDateE ";
                    Parameter.Add(new CommandParameter("@CreatedDateE", createdDateE));
                }
                if (strManageType == "0")//0待核決
                {
                    sqlWhere += @" and A.EmpId=@EmpId";
                    Parameter.Add(new CommandParameter("EmpId", EmpId));
                    //待核決查詢時,要排除自動扣押、自動撤銷的案件(此類案件已成功會進入自動化審核作業)
                    sqlWhere += @" and B.CaseId not in (select CaseId from CaseHistory ch1 where FromRole like '自動%' and CreatedDate = (select max(CreatedDate) from CaseHistory ch2 where  ch1.CaseId = ch2.CaseId)) ";
                }
                if (strManageType == "1")//1協同作業
                {
                    sqlWhere += @" AND AgentUser <> @AgentUserManage ";
                    Parameter.Add(new CommandParameter("@AgentUserManage", EmpId));
                    //協同作業查詢時,要排除自動扣押、自動撤銷的案件(此類案件已成功會進入自動化審核作業)
                    sqlWhere += @" and  B.CaseId not in (select CaseId from CaseHistory ch1 where FromRole like '自動%' and CreatedDate = (select max(CreatedDate) from CaseHistory ch2 where  ch1.CaseId = ch2.CaseId)) ";
                }
                if (strManageType == "2")//2自動化審核
                {
                    sqlWhere += @" AND AgentUser <> @AgentUserManage ";
                    Parameter.Add(new CommandParameter("@AgentUserManage", EmpId));
                    //自動化審核查詢時,要排除自動扣押退回人工的案件,並且要是自動扣押、自動撤銷的案件
                    sqlWhere += @" and B.CaseId not in (select CaseId from CaseHistory ch1 where ToRole like '自動%退回人工' and convert(varchar(10),CreatedDate,23) = (select convert(varchar(10),max(CreatedDate),23) from CaseHistory ch2 where  ch1.CaseId = ch2.CaseId)) ";
                    sqlWhere += @" and B.CaseId in (select CaseId from CaseHistory ch1 where FromRole like '自動%' and CreatedDate = (select max(CreatedDate) from CaseHistory ch2 where  ch1.CaseId = ch2.CaseId)) ";
                }
                //協同的sql不同，待核決、自動化sql同原先的 IR-6037
                string strSql = "";
                if(strManageType == "0" || strManageType == "2")
                {
                    if (model.ReceiveKind == "紙本")
                    {
                        strSql = @"select* into #tmp from(  (select NT.*,convert(nvarchar(max),F.Memo) As memo from ( SELECT distinct B.CaseKind2,B.CaseId,B.CaseNo,E.ObligorName,'' as Agent,''  as RegistrationNo,E.ObligorNo ,B.ReceiveAmount,D.Account,D.Currency,D.SeizureAmount,D.Balance-D.SeizureAmount as AvailBalance,B.AgentUser,B.CreatedDate,B.ReceiveKind FROM CaseAssignTable AS A
                               LEFT  JOIN CaseMaster AS B ON A.CaseId=B.CaseId 
                               left join CaseSeizure D on D.CaseId=B.CaseId left join CaseObligor E on B.CaseId=E.CaseId and (D.CustId=E.ObligorNo or left(D.CustId,10)=left(E.ObligorNo,10))
                               WHERE B.ReceiveKind='紙本'  
                               AND A.AlreadyAssign=0 
                               AND (B.Status=@Status1 OR B.Status=@Status2) AND B.isDelete='0' " + sqlWhere + @" 
                               ) NT left join  CaseMemo F on F.CaseId=NT.CaseId  where MemoDate is null or MemoDate = (select max(MemoDate) from CaseMemo where CaseId = NT.CaseId))
                               ) SeizureTable where 1=1 " + sqlwhere1 + @" 
                               order by CaseNo ASC   select distinct CaseKind2,  CaseNo, ObligorName, Agent,  RegistrationNo,  ObligorNo , ReceiveAmount, Account, Currency, SeizureAmount,  SeizureAmount, AvailBalance,AgentUser,CreatedDate,ReceiveKind,memo  from #tmp drop table #tmp";
                    }
                    else if (model.ReceiveKind == "電子公文")
                    {
                        strSql = @"select* into #tmp from(  
                               ( select NT.*,convert(nvarchar(max),F.Memo) As memo from (SELECT distinct B.CaseKind2,B.CaseID,B.CaseNo,E.ObligorName,E.Agent,E.RegistrationNo,E.ObligorNo,E.Total as ReceiveAmount,D.Account,D.Currency,D.SeizureAmount,D.Balance-D.SeizureAmount as AvailBalance,B.AgentUser,B.CreatedDate,B.ReceiveKind FROM CaseAssignTable AS A
                               LEFT  JOIN CaseMaster AS B ON A.CaseId=B.CaseId 
                               left join CaseSeizure D on D.CaseId=B.CaseId left join EDocTXT1 E on B.CaseId=E.CaseId and (D.CustId=E.RegistrationNo or D.CustId=E.ObligorNo or left(D.CustId,10)=left(E.ObligorNo,10) or D.CustId=E.ObligorNo or left(D.CustId,10)=left(E.RegistrationNo,10))
                               WHERE B.ReceiveKind='電子公文'  
                               AND A.AlreadyAssign=0 
                               AND (B.Status=@Status1 OR B.Status=@Status2) AND B.isDelete='0' " + sqlWhere + @" 
                               ) NT left join  CaseMemo F on F.CaseId=NT.CaseId  where MemoDate is null or MemoDate = (select max(MemoDate) from CaseMemo where CaseId = NT.CaseId)) ) SeizureTable where 1=1 " + sqlwhere1 + @" 
                               order by CaseNo ASC   select distinct CaseKind2,  CaseNo, ObligorName, Agent,  RegistrationNo,  ObligorNo , ReceiveAmount, Account, Currency, SeizureAmount,  SeizureAmount, AvailBalance,AgentUser,CreatedDate,ReceiveKind,memo  from #tmp drop table #tmp";
                    }
                    else
                    {
                        strSql = @"select* into #tmp from(  (select NT.*,convert(nvarchar(max),F.Memo) As memo from ( SELECT distinct B.CaseKind2,B.CaseId,B.CaseNo,E.ObligorName,'' as Agent,''  as RegistrationNo,E.ObligorNo ,B.ReceiveAmount,D.Account,D.Currency,D.SeizureAmount,D.Balance-D.SeizureAmount as AvailBalance,B.AgentUser,B.CreatedDate,B.ReceiveKind FROM CaseAssignTable AS A
                               LEFT  JOIN CaseMaster AS B ON A.CaseId=B.CaseId 
                               left join CaseSeizure D on D.CaseId=B.CaseId left join CaseObligor E on B.CaseId=E.CaseId and (D.CustId=E.ObligorNo or left(D.CustId,10)=left(E.ObligorNo,10))
                               WHERE B.ReceiveKind='紙本'  
                               AND A.AlreadyAssign=0 
                               AND (B.Status=@Status1 OR B.Status=@Status2) AND B.isDelete='0' " + sqlWhere + @" 
                               ) NT left join  CaseMemo F on F.CaseId=NT.CaseId  where MemoDate is null or MemoDate = (select max(MemoDate) from CaseMemo where CaseId = NT.CaseId))
                               union all
                               ( select NT.*,convert(nvarchar(max),F.Memo) As memo from (SELECT distinct B.CaseKind2,B.CaseID,B.CaseNo,E.ObligorName,E.Agent,E.RegistrationNo,E.ObligorNo,E.Total as ReceiveAmount,D.Account,D.Currency,D.SeizureAmount,D.Balance-D.SeizureAmount as AvailBalance,B.AgentUser,B.CreatedDate,B.ReceiveKind FROM CaseAssignTable AS A
                               LEFT  JOIN CaseMaster AS B ON A.CaseId=B.CaseId 
                               left join CaseSeizure D on D.CaseId=B.CaseId left join EDocTXT1 E on B.CaseId=E.CaseId and (D.CustId=E.RegistrationNo or D.CustId=E.ObligorNo or left(D.CustId,10)=left(E.ObligorNo,10) or left(D.CustId,10)=left(E.RegistrationNo,10) )
                               WHERE B.ReceiveKind='電子公文'  
                               AND A.AlreadyAssign=0 
                               AND (B.Status=@Status1 OR B.Status=@Status2) AND B.isDelete='0' " + sqlWhere + @" 
                               ) NT left join  CaseMemo F on F.CaseId=NT.CaseId  where MemoDate is null or MemoDate = (select max(MemoDate) from CaseMemo where CaseId = NT.CaseId)) ) SeizureTable where 1=1 " + sqlwhere1 + @" 
                               order by CaseNo ASC   select distinct CaseKind2,  CaseNo, ObligorName, Agent,  RegistrationNo,  ObligorNo , ReceiveAmount, Account, Currency, SeizureAmount,  SeizureAmount, AvailBalance,AgentUser,CreatedDate,ReceiveKind,memo  from #tmp drop table #tmp";
                    }
                }
                else if(strManageType == "1")
                {
                    if (model.ReceiveKind == "紙本")
                    {
                        strSql = @"select* into #tmp from(  (select NT.*,convert(nvarchar(max),F.Memo) As memo from ( SELECT distinct B.CaseKind2,B.CaseId,B.CaseNo,E.ObligorName,'' as Agent,''  as RegistrationNo,E.ObligorNo ,B.ReceiveAmount,D.Account,D.Currency,D.SeizureAmount,D.Balance-D.SeizureAmount as AvailBalance,B.AgentUser,B.CreatedDate,B.ReceiveKind 
                               from CaseMaster AS B  
                               left join CaseSeizure D on D.CaseId=B.CaseId left join CaseObligor E on B.CaseId=E.CaseId and (D.CustId=E.ObligorNo or left(D.CustId,10)=left(E.ObligorNo,10))
                               WHERE B.ReceiveKind='紙本'
                               AND (B.Status=@Status1 OR B.Status=@Status2) AND B.isDelete='0' " + sqlWhere + @" 
                               ) NT left join  CaseMemo F on F.CaseId=NT.CaseId  where MemoDate is null or MemoDate = (select max(MemoDate) from CaseMemo where CaseId = NT.CaseId))
                               ) SeizureTable where 1=1 " + sqlwhere1 + @" 
                               order by CaseNo ASC   select distinct CaseKind2,  CaseNo, ObligorName, Agent,  RegistrationNo,  ObligorNo , ReceiveAmount, Account, Currency, SeizureAmount,  SeizureAmount, AvailBalance,AgentUser,CreatedDate,ReceiveKind,memo  from #tmp drop table #tmp";
                    }
                    else if (model.ReceiveKind == "電子公文")
                    {
                        strSql = @"select* into #tmp from(  
                               ( select NT.*,convert(nvarchar(max),F.Memo) As memo from (SELECT distinct B.CaseKind2,B.CaseID,B.CaseNo,E.ObligorName,E.Agent,E.RegistrationNo,E.ObligorNo,E.Total as ReceiveAmount,D.Account,D.Currency,D.SeizureAmount,D.Balance-D.SeizureAmount as AvailBalance,B.AgentUser,B.CreatedDate,B.ReceiveKind 
                               FROM CaseMaster AS B 
                               left join CaseSeizure D on D.CaseId=B.CaseId left join EDocTXT1 E on B.CaseId=E.CaseId and (D.CustId=E.RegistrationNo or D.CustId=E.ObligorNo or left(D.CustId,10)=left(E.ObligorNo,10)  or left(D.CustId,10)=left(E.RegistrationNo,10))
                               WHERE B.ReceiveKind='電子公文'
                               AND (B.Status=@Status1 OR B.Status=@Status2) AND B.isDelete='0' " + sqlWhere + @" 
                               ) NT left join  CaseMemo F on F.CaseId=NT.CaseId  where MemoDate is null or MemoDate = (select max(MemoDate) from CaseMemo where CaseId = NT.CaseId)) ) SeizureTable where 1=1 " + sqlwhere1 + @" 
                               order by CaseNo ASC   select distinct CaseKind2,  CaseNo, ObligorName, Agent,  RegistrationNo,  ObligorNo , ReceiveAmount, Account, Currency, SeizureAmount,  SeizureAmount, AvailBalance,AgentUser,CreatedDate,ReceiveKind,memo  from #tmp drop table #tmp";
                    }
                    else
                    {
                        strSql = @"select* into #tmp from(  (select NT.*,convert(nvarchar(max),F.Memo) As memo from ( SELECT distinct B.CaseKind2,B.CaseId,B.CaseNo,E.ObligorName,'' as Agent,''  as RegistrationNo,E.ObligorNo ,B.ReceiveAmount,D.Account,D.Currency,D.SeizureAmount,D.Balance-D.SeizureAmount as AvailBalance,B.AgentUser,B.CreatedDate,B.ReceiveKind 
                               from CaseMaster AS B  
                               left join CaseSeizure D on D.CaseId=B.CaseId left join CaseObligor E on B.CaseId=E.CaseId and (D.CustId=E.ObligorNo or left(D.CustId,8)=left(E.ObligorNo,8) or left(D.CustId,10)=left(E.ObligorNo,10))
                               WHERE B.ReceiveKind='紙本'
                               AND (B.Status=@Status1 OR B.Status=@Status2) AND B.isDelete='0' " + sqlWhere + @" 
                               ) NT left join  CaseMemo F on F.CaseId=NT.CaseId  where MemoDate is null or MemoDate = (select max(MemoDate) from CaseMemo where CaseId = NT.CaseId))
                               union all
                               ( select NT.*,convert(nvarchar(max),F.Memo) As memo from (SELECT distinct B.CaseKind2,B.CaseID,B.CaseNo,E.ObligorName,E.Agent,E.RegistrationNo,E.ObligorNo,E.Total as ReceiveAmount,D.Account,D.Currency,D.SeizureAmount,D.Balance-D.SeizureAmount as AvailBalance,B.AgentUser,B.CreatedDate,B.ReceiveKind 
                               FROM CaseMaster AS B 
                               left join CaseSeizure D on D.CaseId=B.CaseId left join EDocTXT1 E on B.CaseId=E.CaseId and (D.CustId=E.RegistrationNo or D.CustId=E.ObligorNo or left(D.CustId,10)=left(E.RegistrationNo,10) or left(D.CustId,10)=left(E.ObligorNo,10))
                               WHERE B.ReceiveKind='電子公文'
                               AND (B.Status=@Status1 OR B.Status=@Status2) AND B.isDelete='0' " + sqlWhere + @" 
                               ) NT left join  CaseMemo F on F.CaseId=NT.CaseId  where MemoDate is null or MemoDate = (select max(MemoDate) from CaseMemo where CaseId = NT.CaseId)) ) SeizureTable where 1=1 " + sqlwhere1 + @" 
                               order by CaseNo ASC   select distinct CaseKind2,  CaseNo, ObligorName, Agent,  RegistrationNo,  ObligorNo , ReceiveAmount, Account, Currency, SeizureAmount,  SeizureAmount, AvailBalance,AgentUser,CreatedDate,ReceiveKind,memo  from #tmp drop table #tmp";
                    }
                }
                
                Parameter.Add(new CommandParameter("Status1", CaseStatus.AgentSubmit));
                Parameter.Add(new CommandParameter("Status2", CaseStatus.DirectorSubmit));

                IList<RemitSeizure> ilst = SearchList<RemitSeizure>(strSql);
                if (ilst != null)
                {
                    return ilst;
                }
                return new List<RemitSeizure>();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        /// <summary>
        /// 得到撤銷數據
        /// </summary>
        /// <param name="model"></param>
        /// <param name="EmpId"></param>
        /// <returns></returns>
        public IList<RemitSeizure> GetPrintDataForUndo(DirectorToApprove model, string EmpId, string strManageType)
        {
            try
            {
                string sqlWhere = "";
                string sqlwhere1 = "";
                Parameter.Clear();

                if (!string.IsNullOrEmpty(model.CaseNo))
                {
                    sqlWhere += @" and B.CaseNo like @CaseNo ";
                    Parameter.Add(new CommandParameter("@CaseNo", "%" + model.CaseNo.Trim() + "%"));
                }
                if (!string.IsNullOrEmpty(model.GovUnit))
                {
                    sqlWhere += @" and GovUnit like @GovUnit ";
                    Parameter.Add(new CommandParameter("@GovUnit", "%" + model.GovUnit.Trim() + "%"));
                }
                if (!string.IsNullOrEmpty(model.GovNo))
                {
                    sqlWhere += @" and GovNo like @GovNo ";
                    Parameter.Add(new CommandParameter("@GovNo", "%" + model.GovNo.Trim() + "%"));
                }
                if (!string.IsNullOrEmpty(model.CreateUser))
                {
                    sqlWhere += @" and Person like @Person ";
                    Parameter.Add(new CommandParameter("@Person", "%" + model.CreateUser.Trim() + "%"));
                }
                if (!string.IsNullOrEmpty(model.SendKind))
                {
                    sqlWhere += @" and SendKind like @SendKind ";
                    Parameter.Add(new CommandParameter("@SendKind", model.SendKind.Trim()));
                }
                if (!string.IsNullOrEmpty(model.Speed))
                {
                    sqlWhere += @" and B.Speed = @Speed ";
                    Parameter.Add(new CommandParameter("@Speed", model.Speed.Trim()));
                }
                if (!string.IsNullOrEmpty(model.ReceiveKind))
                {
                    sqlwhere1 += @" and ReceiveKind like @ReceiveKind ";
                    Parameter.Add(new CommandParameter("@ReceiveKind", "%" + model.ReceiveKind.Trim() + "%"));
                }
                if (!string.IsNullOrEmpty(model.CaseKind))
                {
                    sqlWhere += @" and CaseKind like @CaseKind ";
                    Parameter.Add(new CommandParameter("@CaseKind", "%" + model.CaseKind.Trim() + "%"));
                }
                if (!string.IsNullOrEmpty(model.CaseKind2))
                {
                    sqlWhere += @" and CaseKind2 like @CaseKind2 ";
                    Parameter.Add(new CommandParameter("@CaseKind2", "%" + model.CaseKind2.Trim() + "%"));
                }
                if (!string.IsNullOrEmpty(model.GovDateS))
                {
                    sqlWhere += @" AND B.GovDate >= @GovDateS";
                    Parameter.Add(new CommandParameter("@GovDateS", model.GovDateS));
                }
                if (!string.IsNullOrEmpty(model.GovDateE))
                {
                    string qDateE = Convert.ToDateTime(UtlString.FormatDateString(model.GovDateE)).AddDays(1).ToString("yyyyMMdd");
                    sqlWhere += @" AND B.GovDate < @GovDateE ";
                    Parameter.Add(new CommandParameter("@GovDateE", qDateE));
                }
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
                    base.Parameter.Add(new CommandParameter("@ManagerID", "%" + EmpId + "%"));
                }

                if (!string.IsNullOrEmpty(model.SendDateS) || !string.IsNullOrEmpty(model.SendDateE))
                {
                    string sqlWhere1 = "";
                    if (!string.IsNullOrEmpty(model.SendDateS))
                    {
                        model.SendDateS = UtlString.FormatDateTwStringToAd(model.SendDateS);
                        sqlWhere1 += @" AND SendDate >= @SendDateStart";
                        Parameter.Add(new CommandParameter("@SendDateStart", model.SendDateS));
                    }
                    if (!string.IsNullOrEmpty(model.SendDateE))
                    {
                        string sendDateEnd = Convert.ToDateTime(UtlString.FormatDateString(model.SendDateE)).AddDays(1).ToString("yyyyMMdd");
                        sqlWhere1 += @" AND SendDate < @SendDateEnd ";
                        Parameter.Add(new CommandParameter("@SendDateEnd", sendDateEnd));
                    }
                    sqlWhere += " AND B.CaseId IN (SELECT CaseId  FROM CaseSendSetting AS CSS where 1=1 " + sqlWhere1 + @") ";
                }
                if (!string.IsNullOrEmpty(model.Unit))
                {
                    sqlWhere += @" and Unit like @Unit ";
                    Parameter.Add(new CommandParameter("@Unit", "%" + model.Unit.Trim() + "%"));
                }
                if (!string.IsNullOrEmpty(model.CreatedDateS))
                {
                    sqlWhere += @" AND B.CreatedDate >= @CreatedDateS";
                    Parameter.Add(new CommandParameter("@CreatedDateS", model.CreatedDateS));
                }
                if (!string.IsNullOrEmpty(model.CreatedDateE))
                {
                    string createdDateE = Convert.ToDateTime(UtlString.FormatDateString(model.CreatedDateE)).AddDays(1).ToString("yyyy/MM/dd");
                    sqlWhere += @" AND B.CreatedDate < @CreatedDateE ";
                    Parameter.Add(new CommandParameter("@CreatedDateE", createdDateE));
                }
                #region IR-0162 撤銷案件-匯出 IR-0163 
                if (strManageType == "0")//0待核決
                {
                    sqlWhere += @" and A.EmpId=@EmpId";
                    Parameter.Add(new CommandParameter("EmpId", EmpId));
                    //待核決查詢時,要排除自動扣押、自動撤銷的案件(此類案件已成功會進入自動化審核作業)
                    sqlWhere += @" and B.CaseId not in (select CaseId from CaseHistory ch1 where FromRole like '自動%' and CreatedDate = (select max(CreatedDate) from CaseHistory ch2 where  ch1.CaseId = ch2.CaseId)) ";
                }
                if (strManageType == "1")//1協同作業
                {
                    sqlWhere += @" AND AgentUser <> @AgentUserManage ";
                    Parameter.Add(new CommandParameter("@AgentUserManage", EmpId));
                    //協同作業查詢時,要排除自動扣押、自動撤銷的案件(此類案件已成功會進入自動化審核作業)
                    sqlWhere += @" and  B.CaseId not in (select CaseId from CaseHistory ch1 where FromRole like '自動%' and CreatedDate = (select max(CreatedDate) from CaseHistory ch2 where  ch1.CaseId = ch2.CaseId)) ";
                }
                if (strManageType == "2")//2自動化審核
                {
                    sqlWhere += @" AND AgentUser <> @AgentUserManage ";
                    Parameter.Add(new CommandParameter("@AgentUserManage", EmpId));
                    //自動化審核查詢時,要排除自動撤銷退回人工的案件,並且要是自動扣押、自動撤銷的案件
                    sqlWhere += @" and B.CaseId not in (select CaseId from CaseHistory ch1 where ToRole like '自動%退回人工' and convert(varchar(10),CreatedDate,23) = (select convert(varchar(10),max(CreatedDate),23) from CaseHistory ch2 where  ch1.CaseId = ch2.CaseId)) ";
                    sqlWhere += @" and B.CaseId in (select CaseId from CaseHistory ch1 where FromRole like '自動%' and CreatedDate = (select max(CreatedDate) from CaseHistory ch2 where  ch1.CaseId = ch2.CaseId)) ";
                }
                //協同的sql不同，待核決、自動化sql同原先的
                string strSql = "";
                if (strManageType == "0" || strManageType == "2")
                {
                    if (model.ReceiveKind == "紙本")
                    {
                        strSql = @"select * into #tmp from(  (select NT.*,convert(nvarchar(max),F.Memo) As Memo from ( SELECT distinct B.CaseKind2,B.CaseId,B.CaseNo,D.Account,D.Currency,'查無資訊' as SendNo,D.SeizureAmount,D.CancelAmount,B.AgentUser,B.CreatedDate,B.ReceiveKind FROM CaseAssignTable AS A
                                LEFT  JOIN CaseMaster AS B ON A.CaseId=B.CaseId 
                                left join CaseSeizure D on D.cancelcaseid=B.CaseId left join CaseObligor E on B.CaseId=E.CaseId and (D.CustId=E.ObligorNo or left(D.CustId,10)=left(E.ObligorNo,10)) 
                                WHERE B.ReceiveKind='紙本' 
                                AND A.AlreadyAssign=0 
                                AND (B.Status=@Status1 OR B.Status=@Status2) AND B.isDelete='0' " + sqlWhere + @" 
                                ) NT left join  CaseMemo F on F.CaseId=NT.CaseId  where MemoDate is null or MemoDate = (select max(MemoDate) from CaseMemo where CaseId = NT.CaseId)) 
                                ) SeizureTable where 1=1 " + sqlwhere1 + @" 
                                order by CaseNo ASC select distinct CaseKind2,CaseNo,Account,Currency,SendNo,SeizureAmount,CancelAmount,AgentUser,CreatedDate,ReceiveKind,Memo from #tmp  drop table #tmp";
                    }
                    else if (model.ReceiveKind == "電子公文")
                    {
                        strSql = @"select * into #tmp from( 
                                ( select NT.*,convert(nvarchar(max),F.Memo) As Memo from (SELECT distinct B.CaseKind2,B.CaseId,B.CaseNo,D.Account,D.Currency,E.ReceiverNo2 as SendNo,D.SeizureAmount,D.CancelAmount,B.AgentUser,B.CreatedDate,B.ReceiveKind FROM CaseAssignTable AS A                           
                                LEFT  JOIN CaseMaster AS B ON A.CaseId=B.CaseId 
                                left join CaseSeizure D on D.cancelcaseid=B.CaseId left join EDocTXT2 E on B.CaseId=E.CaseId and (D.CustId=E.RegistrationNo or D.CustId=E.ObligorNo or left(D.CustId,10)=left(E.ObligorNo,10) or left(D.CustId,10)=left(E.RegistrationNo,10) ) 
                                WHERE B.ReceiveKind='電子公文' 
                                AND A.AlreadyAssign=0 
                                AND (B.Status=@Status1 OR B.Status=@Status2) AND B.isDelete='0' " + sqlWhere + @" 
                                ) NT left join CaseMemo F on F.CaseId=NT.CaseId  where MemoDate is null or MemoDate = (select max(MemoDate) from CaseMemo where CaseId = NT.CaseId)) ) SeizureTable where 1=1 " + sqlwhere1 + @" 
                                order by CaseNo ASC update #tmp set SendNo = ET.ReceiverNo2 from EDocTXT2 ET where ET.CaseId = #tmp.CaseId 
								select distinct CaseKind2,CaseNo,Account,Currency,SendNo,SeizureAmount,CancelAmount,AgentUser,CreatedDate,ReceiveKind,Memo from #tmp drop table #tmp";
                    }
                    else
                    {
                        strSql = @"select * into #tmp from(  (select NT.*,convert(nvarchar(max),F.Memo) As Memo from ( SELECT distinct B.CaseKind2,B.CaseId,B.CaseNo,D.Account,D.Currency,'查無資訊' as SendNo,D.SeizureAmount,D.CancelAmount,B.AgentUser,B.CreatedDate,B.ReceiveKind FROM CaseAssignTable AS A
                                LEFT  JOIN CaseMaster AS B ON A.CaseId=B.CaseId 
                                left join CaseSeizure D on D.cancelcaseid=B.CaseId left join CaseObligor E on B.CaseId=E.CaseId and (D.CustId=E.ObligorNo or left(D.CustId,10)=left(E.ObligorNo,10)) 
                                WHERE B.ReceiveKind='紙本' 
                                AND A.AlreadyAssign=0 
                                AND (B.Status=@Status1 OR B.Status=@Status2) AND B.isDelete='0' " + sqlWhere + @" 
                                ) NT left join  CaseMemo F on F.CaseId=NT.CaseId  where MemoDate is null or MemoDate = (select max(MemoDate) from CaseMemo where CaseId = NT.CaseId)) 
                                union all
                                ( select NT.*,convert(nvarchar(max),F.Memo) As Memo from (SELECT distinct B.CaseKind2,B.CaseId,B.CaseNo,D.Account,D.Currency,E.ReceiverNo2 as SendNo,D.SeizureAmount,D.CancelAmount,B.AgentUser,B.CreatedDate,B.ReceiveKind FROM CaseAssignTable AS A                           
                                LEFT  JOIN CaseMaster AS B ON A.CaseId=B.CaseId 
                                left join CaseSeizure D on D.cancelcaseid=B.CaseId left join EDocTXT2 E on B.CaseId=E.CaseId and (D.CustId=E.RegistrationNo or D.CustId=E.ObligorNo or left(D.CustId,10)=left(E.ObligorNo,10) or left(D.CustId,10)=left(E.RegistrationNo,10) ) 
                                WHERE B.ReceiveKind='電子公文' 
                                AND A.AlreadyAssign=0 
                                AND (B.Status=@Status1 OR B.Status=@Status2) AND B.isDelete='0' " + sqlWhere + @" 
                                ) NT left join CaseMemo F on F.CaseId=NT.CaseId  where MemoDate is null or MemoDate = (select max(MemoDate) from CaseMemo where CaseId = NT.CaseId)) ) SeizureTable where 1=1 " + sqlwhere1 + @" 
                                order by CaseNo ASC update #tmp set SendNo = ET.ReceiverNo2 from EDocTXT2 ET where ET.CaseId = #tmp.CaseId 
								select distinct CaseKind2,CaseNo,Account,Currency,SendNo,SeizureAmount,CancelAmount,AgentUser,CreatedDate,ReceiveKind,Memo from #tmp drop table #tmp";
                    }
                }
                else if (strManageType == "1")
                {
                    if (model.ReceiveKind == "紙本")
                    {
                        strSql = @"select * into #tmp from(  (select NT.*,convert(nvarchar(max),F.Memo) As Memo from ( SELECT distinct B.CaseKind2,B.CaseId,B.CaseNo,D.Account,D.Currency,'查無資訊' as SendNo,D.SeizureAmount,D.CancelAmount,B.AgentUser,B.CreatedDate,B.ReceiveKind 
                                from CaseMaster AS B  
                                left join CaseSeizure D on D.cancelcaseid=B.CaseId left join CaseObligor E on B.CaseId=E.CaseId and (D.CustId=E.ObligorNo or left(D.CustId,10)=left(E.ObligorNo,10)) 
                                WHERE B.ReceiveKind='紙本' 
                                AND (B.Status=@Status1 OR B.Status=@Status2) AND B.isDelete='0' " + sqlWhere + @" 
                                ) NT left join  CaseMemo F on F.CaseId=NT.CaseId  where MemoDate is null or MemoDate = (select max(MemoDate) from CaseMemo where CaseId = NT.CaseId)) 
                                ) SeizureTable where 1=1 " + sqlwhere1 + @" 
                                order by CaseNo ASC select distinct CaseKind2,CaseNo,Account,Currency,SendNo,SeizureAmount,CancelAmount,AgentUser,CreatedDate,ReceiveKind,Memo from #tmp  drop table #tmp";
                    }
                    else if (model.ReceiveKind == "電子公文")
                    {
                        strSql = @"select * into #tmp from(
                                ( select NT.*,convert(nvarchar(max),F.Memo) As Memo from (SELECT distinct B.CaseKind2,B.CaseId,B.CaseNo,D.Account,D.Currency,E.ReceiverNo2 as SendNo,D.SeizureAmount,D.CancelAmount,B.AgentUser,B.CreatedDate,B.ReceiveKind  
                                from CaseMaster AS B  
                                left join CaseSeizure D on D.cancelcaseid=B.CaseId left join EDocTXT2 E on B.CaseId=E.CaseId and (D.CustId=E.RegistrationNo or D.CustId=E.ObligorNo or left(D.CustId,10)=left(E.ObligorNo,10) or left(D.CustId,10)=left(E.RegistrationNo,10) ) 
                                WHERE B.ReceiveKind='電子公文' 
                                AND (B.Status=@Status1 OR B.Status=@Status2) AND B.isDelete='0' " + sqlWhere + @" 
                                ) NT left join CaseMemo F on F.CaseId=NT.CaseId  where MemoDate is null or MemoDate = (select max(MemoDate) from CaseMemo where CaseId = NT.CaseId)) ) SeizureTable where 1=1 " + sqlwhere1 + @" 
                                order by CaseNo ASC update #tmp set SendNo = ET.ReceiverNo2 from EDocTXT2 ET where ET.CaseId = #tmp.CaseId 
								select distinct CaseKind2,CaseNo,Account,Currency,SendNo,SeizureAmount,CancelAmount,AgentUser,CreatedDate,ReceiveKind,Memo from #tmp drop table #tmp";
                    }
                    else
                    {
                        strSql = @"select * into #tmp from(  (select NT.*,convert(nvarchar(max),F.Memo) As Memo from ( SELECT distinct B.CaseKind2,B.CaseId,B.CaseNo,D.Account,D.Currency,'查無資訊' as SendNo,D.SeizureAmount,D.CancelAmount,B.AgentUser,B.CreatedDate,B.ReceiveKind 
                                from CaseMaster AS B  
                                left join CaseSeizure D on D.cancelcaseid=B.CaseId left join CaseObligor E on B.CaseId=E.CaseId and (D.CustId=E.ObligorNo or left(D.CustId,10)=left(E.ObligorNo,10)) 
                                WHERE B.ReceiveKind='紙本' 
                                AND (B.Status=@Status1 OR B.Status=@Status2) AND B.isDelete='0' " + sqlWhere + @" 
                                ) NT left join  CaseMemo F on F.CaseId=NT.CaseId  where MemoDate is null or MemoDate = (select max(MemoDate) from CaseMemo where CaseId = NT.CaseId)) 
                                union all
                                ( select NT.*,convert(nvarchar(max),F.Memo) As Memo from (SELECT distinct B.CaseKind2,B.CaseId,B.CaseNo,D.Account,D.Currency,E.ReceiverNo2 as SendNo,D.SeizureAmount,D.CancelAmount,B.AgentUser,B.CreatedDate,B.ReceiveKind  
                                from CaseMaster AS B  
                                left join CaseSeizure D on D.cancelcaseid=B.CaseId left join EDocTXT2 E on B.CaseId=E.CaseId and (D.CustId=E.RegistrationNo or D.CustId=E.ObligorNo or left(D.CustId,10)=left(E.ObligorNo,10) or left(D.CustId,10)=left(E.RegistrationNo,10) ) 
                                WHERE B.ReceiveKind='電子公文' 
                                AND (B.Status=@Status1 OR B.Status=@Status2) AND B.isDelete='0' " + sqlWhere + @" 
                                ) NT left join CaseMemo F on F.CaseId=NT.CaseId  where MemoDate is null or MemoDate = (select max(MemoDate) from CaseMemo where CaseId = NT.CaseId)) ) SeizureTable where 1=1 " + sqlwhere1 + @" 
                                order by CaseNo ASC update #tmp set SendNo = ET.ReceiverNo2 from EDocTXT2 ET where ET.CaseId = #tmp.CaseId 
								select distinct CaseKind2,CaseNo,Account,Currency,SendNo,SeizureAmount,CancelAmount,AgentUser,CreatedDate,ReceiveKind,Memo from #tmp drop table #tmp";
                    }
                }
                #endregion
   
                Parameter.Add(new CommandParameter("Status1", CaseStatus.AgentSubmit));
                Parameter.Add(new CommandParameter("Status2", CaseStatus.DirectorSubmit));

                IList<RemitSeizure> ilst = SearchList<RemitSeizure>(strSql);
                if (ilst != null)
                {
                    return ilst;
                }
                return new List<RemitSeizure>();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #region 修改CaseMaster的狀態,並且記錄Log
        /// <summary>
        /// 修改CaseMaster的狀態,並且記錄Log
        /// </summary>
        /// <param name="caseid"></param>
        /// <param name="newStatus"></param>
        /// <param name="userId"></param>
        /// <param name="trans"></param>
        /// <returns></returns>
        public int UpdateCaseStatus(Guid caseid, string newStatus, string userId, IDbTransaction trans = null)
        {
            DateTime dtNow = GetNowDateTime();
            CaseHistoryBIZ history = new CaseHistoryBIZ();
            string strSql = @"UPDATE [CaseMaster] SET [Status] = @Status,
                [ModifiedUser] =@UpdateUserId, [ModifiedDate]=@UpdateDate WHERE [CaseId] = @CaseId";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("@CaseId", caseid));
            Parameter.Add(new CommandParameter("@Status", newStatus));
            Parameter.Add(new CommandParameter("@UpdateUserId", userId));
            Parameter.Add(new CommandParameter("@UpdateDate", dtNow));
            if (trans != null)
            {
                // 執行新增返回是否成功
                if (ExecuteNonQuery(strSql, trans) > 0)
                {
                    insertCaseHistoryForBatch(caseid, newStatus, userId, trans);
                    return 1;
                }
            }
            IDbConnection dbConnection = OpenConnection();
            IDbTransaction tans = dbConnection.BeginTransaction();
            if (ExecuteNonQuery(strSql, tans) > 0)
            {
                insertCaseHistoryForBatch(caseid, newStatus, userId, tans);
                tans.Commit();
                return 1;
            }
            tans.Rollback();
            return 0;
        }
        #endregion
        public bool insertCaseHistoryForBatch(Guid caseId, string action, string userId, IDbTransaction dbTransaction = null)
        {
            CaseHistoryBIZ history = new CaseHistoryBIZ();
            //CaseHistory old = history.getHistoryByCaseId(caseId).OrderByDescending(m => m.HistoryId).FirstOrDefault();

            //PARMCode obj = GetCodeData("EVENT_NAME").FirstOrDefault(m => m.CodeNo == action.ToString());
            //string eventName = obj != null ? obj.CodeDesc : "";
            string eventName = "主管整批放行結案";
            //obj = GetCodeData("STATUS_NAME").FirstOrDefault(m => m.CodeNo == action.ToString());
            //string toFolder = obj != null ? obj.CodeDesc : "";
            string toFolder = "主管-整批放行結案";
            //obj = GetCodeData("EVENT_ROLE").FirstOrDefault(m => m.CodeNo == action.ToString());
            //string toRole = obj != null ? obj.CodeDesc : "";
            string toRole = "主管人員";
            CaseHistory newHistory = new CaseHistory()
            {
                CaseId = caseId,
                FromRole = "經辦人員",//old != null ? old.ToRole : "",
                FromUser = userId,//old != null ? old.ToUser : "",
                FromFolder ="主管-整批待核決", //old != null ? old.ToFolder : "",
                Event = eventName,
                EventTime = GetNowDateTime(),
                ToRole = toRole,
                ToUser = userId,
                ToFolder = toFolder,
                CreatedUser = userId,
                CreatedDate = GetNowDateTime()

            };

            return history.insertCaseHistory(newHistory, dbTransaction);
        }
    }
}
