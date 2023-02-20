using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CTBC.CSFS.Pattern;
using CTBC.CSFS.Models;
using CTBC.CSFS.ViewModels;

namespace CTBC.CSFS.BussinessLogic
{
    public class CaseReturnBIZ : CommonBIZ
    {
        /// <summary>
        /// 查詢出所有集作退回來的案件
        /// </summary>
        /// <param name="model"></param>
        /// <param name="pageNum"></param>
        /// <param name="strSortExpression"></param>
        /// <param name="strSortDirection"></param>
        /// <returns></returns>
        public IList<CaseReturn> Getlist(CaseReturn model, int pageNum, string strSortExpression, string strSortDirection)
        {
            try
            {
                string Sqlwhere = "";
                base.PageIndex = pageNum;
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@pageS", (base.PageSize * (base.PageIndex - 1)) + 1));
                base.Parameter.Add(new CommandParameter("@pageE", base.PageSize * base.PageIndex));
                if (!string.IsNullOrEmpty(model.GovUnit))
                {
                    Sqlwhere += " and GovUnit like @GovUnit";
                    base.Parameter.Add(new CommandParameter("@GovUnit", "%" + model.GovUnit + "%"));
                }
                if (!string.IsNullOrEmpty(model.GovDateS))
                {
                    Sqlwhere += " and cm.GovDate>=@GovDateS";
                    base.Parameter.Add(new CommandParameter("@GovDateS", model.GovDateS));
                }
                if (!string.IsNullOrEmpty(model.GovDateE))
                {
                    Sqlwhere += " and cm.GovDate<@GovDateE";
                    base.Parameter.Add(new CommandParameter("@GovDateE", model.GovDateE));
                }
                if (!string.IsNullOrEmpty(model.Speed))
                {
                    Sqlwhere += " and Speed=@Speed";
                    base.Parameter.Add(new CommandParameter("@Speed", model.Speed));
                }
                if (!string.IsNullOrEmpty(model.ReceiveKind))
                {
                    Sqlwhere += " and ReceiveKind like @ReceiveKind";
                    base.Parameter.Add(new CommandParameter("@ReceiveKind", "%" + model.ReceiveKind + "%"));
                }
                if (!string.IsNullOrEmpty(model.GovNo))
                {
                    Sqlwhere += " and GovNo like @GovNo";
                    base.Parameter.Add(new CommandParameter("@GovNo", "%" + model.GovNo + "%"));
                }
                if (!string.IsNullOrEmpty(model.CaseKind))
                {
                    Sqlwhere += " and CaseKind=@CaseKind";
                    base.Parameter.Add(new CommandParameter("@CaseKind", model.CaseKind));
                }
                if (!string.IsNullOrEmpty(model.CaseKind2))
                {
                    Sqlwhere += " and CaseKind2 =@CaseKind2";
                    base.Parameter.Add(new CommandParameter("@CaseKind2", model.CaseKind2));
                }
                
                if (!string.IsNullOrEmpty(model.Unit))
                {
                    Sqlwhere += " and Unit like @Unit";
                    base.Parameter.Add(new CommandParameter("@Unit", "%" + model.Unit + "%"));
                }
				//else
				//{
				//	Sqlwhere += " and Unit = '' ";
				//}
                if (!string.IsNullOrEmpty(model.CreateUser))
                {
                    Sqlwhere += " and cm.CreatedUser like @CreateUser";
                    base.Parameter.Add(new CommandParameter("@CreateUser", "%" + model.CreateUser + "%"));
                }

                string strSql = @"with T1 
	                        as
	                        (
		                       select isnull(cm.CreatedUser,'9999') as CreateUser,CaseId,GovUnit,(SELECT CONVERT(varchar(100), GovDate, 111)) as GovDate,ReceiverNo,Speed,CaseKind,
		                       CaseKind2,GovNo,Unit,Person,CaseNo,CloseReason,(SELECT CONVERT(varchar(100), LimitDate, 111)) as LimitDate,
                               ReceiveKind,Status,cm.CreatedDate,DocNo,ReturnReason from CaseMaster cm  where status=@status and isDelete='0' " + Sqlwhere + @" 
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

                base.Parameter.Add(new CommandParameter("@status", CaseStatus.CollectionReturn));
                IList <CaseReturn> ilst= base.SearchList<CaseReturn>(strSql);
                if (ilst != null)
                {
                    if (ilst.Count > 0)
                    {
                        base.DataRecords = ilst[0].maxnum;
                    }
                    else
                    {
                        base.DataRecords = 0;
                        ilst = new List<CaseReturn>();
                    }
                    return ilst;
                }
                else
                {
                    return new List<CaseReturn>();
                }

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        /// <summary>
        /// 建檔人員結案
        /// </summary>
        /// <param name="caseId"></param>
        /// <returns></returns>
        public bool KeyInpuCloseCase(Guid caseId, string ClosedReson, string ReturnAnswer)
        {
            bool rtn = true;
            IDbConnection dbConnection = OpenConnection();
            IDbTransaction dbTransaction = null;
            try
            {
                using (dbConnection)
                {
                    dbTransaction = dbConnection.BeginTransaction();
                    string strSql = @"UPDATE [CaseMaster] SET [CloseUser] = @CloseUser,[CloseDate] = GETDATE(),[CloseReason] = @CloseReason,[ReturnAnswer] = @ReturnAnswer WHERE [CaseId] = @CaseId;";
                    Parameter.Clear();
                    Parameter.Add(new CommandParameter("CaseId", caseId));
                    Parameter.Add(new CommandParameter("CloseUser", Account));
                    Parameter.Add(new CommandParameter("CloseReason", ClosedReson));
                    Parameter.Add(new CommandParameter("ReturnAnswer", ReturnAnswer));
                    
                    rtn = ExecuteNonQuery(strSql, dbTransaction) > 0;
                    rtn = rtn &&  new CaseMasterBIZ().UpdateCaseStatus(caseId, CaseStatus.InputCancelClose, Account, dbTransaction) > 0;
                    if(rtn)
                        dbTransaction.Commit();
                    else
                        dbTransaction.Rollback();
                    return rtn;
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
