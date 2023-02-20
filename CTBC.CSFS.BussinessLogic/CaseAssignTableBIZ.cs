using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CTBC.CSFS.Models;
using CTBC.CSFS.Pattern;

namespace CTBC.CSFS.BussinessLogic
{
    public class CaseAssignTableBIZ : CommonBIZ
    {
        public bool insertCaseAssignTable(CaseAssignTable item, IDbTransaction dbTransaction = null)
        {
            string strSql = @"INSERT INTO [CaseAssignTable] ([CaseId],[EmpId],[AlreadyAssign],[CreatdUser],[CreatedDate])VALUES (@CaseId,@EmpId,0,@CreatdUser,@CreateDate);";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("@CaseId", item.CaseId));
            Parameter.Add(new CommandParameter("@EmpId", item.EmpId));
            Parameter.Add(new CommandParameter("@CreatdUser", item.CreatdUser));
            Parameter.Add(new CommandParameter("@CreateDate", item.CreatedDate));
            Parameter.Add(new CommandParameter("@ModifiedUser", item.ModifiedDate));
            return base.ExecuteNonQuery(strSql) > 0;
        }
    }
}
