using CTBC.CSFS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CTBC.CSFS.Pattern;
using System.Data;

namespace CTBC.CSFS.BussinessLogic
{
    public class AgentDepartmentAccessBIZ : CommonBIZ
    {
        public AgentDepartmentAccessBIZ(AppController appController)
            : base(appController)
        { }

        public AgentDepartmentAccessBIZ()
        { }
        public IList<AgentDepartmentAccess> GetDataFromCaseDtAccess(Guid caseId)
        {
            try
            {
                string strSql = @"select A.*,p.EmpName from CaseDepartmentAccess A left join LDAPEmployee P on a.CreatedUser=p.EmpID where CaseId=@CaseId";
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@CaseId", caseId));
                IList<AgentDepartmentAccess> list = SearchList<AgentDepartmentAccess>(strSql);
                return list;
            }
            catch (Exception ex)
            {
                
                throw ex;
            }
           
        }
        public AgentDepartmentAccess GetDataByAccessId(int AccessId)
        {
            try
            {
                string strSql = @"select A.*,p.EmpName from CaseDepartmentAccess A left join LDAPEmployee P on a.CreatedUser=p.EmpID where AccessId=@AccessId";
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@AccessId", AccessId));
                IList<AgentDepartmentAccess> list = SearchList<AgentDepartmentAccess>(strSql);
                if (list.Count>0)
                {
                    return list[0];
                }
                else
                {
                    return new AgentDepartmentAccess();
                }
                
            }
            catch (Exception ex)
            {
                
                throw ex;
            }
        }

        public bool Create(AgentDepartmentAccess model)
        {
            try
            {
                string strSql = @"INSERT INTO CaseDepartmentAccess(CaseId ,AccessData,CreatedUser,CreatedDate,ModifiedUser,ModifiedDate) VALUES(@CaseId,@AccessData,@CreatedUser,GETDATE(),@CreatedUser,GETDATE())";
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@CaseId", model.CaseId));
                base.Parameter.Add(new CommandParameter("@AccessData", model.AccessData));
                base.Parameter.Add(new CommandParameter("@CreatedUser", model.CreatedUser));
                return base.ExecuteNonQuery(strSql) > 0;
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }
        public bool Edit(AgentDepartmentAccess model)
        {
            try
            {
                string strSql = @"update CaseDepartmentAccess set CaseId=@CaseId,AccessData=@AccessData,ModifiedUser=@ModifiedUser,ModifiedDate=GETDATE() where AccessId=@AccessId;";
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@CaseId", model.CaseId));
                base.Parameter.Add(new CommandParameter("@AccessData", model.AccessData));
                base.Parameter.Add(new CommandParameter("@ModifiedUser", model.ModifiedUser));
                base.Parameter.Add(new CommandParameter("@AccessId", model.AccessId));
                return base.ExecuteNonQuery(strSql) > 0;
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }

        public int Delete(int AccessId)
        {
            try
            {
                string strSql = @"delete CaseDepartmentAccess where AccessId=@AccessId";
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@AccessId", AccessId));
                return base.ExecuteNonQuery(strSql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public bool DeleteDepartmentAccess(Guid caseId, IDbTransaction dbtrans = null)
        {
            string strSql = "DELETE CaseDepartmentAccess WHERE  CaseId = @CaseId ";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("CaseId", caseId));
            return dbtrans == null ? ExecuteNonQuery(strSql) > 0 : ExecuteNonQuery(strSql, dbtrans) > 0;
        }

    }
}
