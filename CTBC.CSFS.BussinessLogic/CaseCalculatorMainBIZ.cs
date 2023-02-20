using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CTBC.CSFS.Models;
using CTBC.CSFS.Pattern;

namespace CTBC.CSFS.BussinessLogic
{
    public class CaseCalculatorMainBIZ : CommonBIZ
    {
        public int Edit(Guid caseId, int amount1, int amount2, int amount3, int amount4, int amount5)
        {
            string strSql = @"update CaseCalculatorMain set Amount1=@Amount1,Amount2=@Amount2,
                                       Amount3=@Amount3,Amount4=@Amount4,Amount5=@Amount5,CreatedDate=@CreatedDate
                                       where CaseId=@CaseId";

            base.Parameter.Clear();
            base.Parameter.Add(new CommandParameter("@Amount1", amount1));
            base.Parameter.Add(new CommandParameter("@Amount2", amount2));
            base.Parameter.Add(new CommandParameter("@Amount3", amount3));
            base.Parameter.Add(new CommandParameter("@Amount4", amount4));
            base.Parameter.Add(new CommandParameter("@Amount5", amount5));
            //base.Parameter.Add(new CommandParameter("@CreatedUser",CreatedUser));
            base.Parameter.Add(new CommandParameter("@CreatedDate",DateTime.Now));
            base.Parameter.Add(new CommandParameter("@CaseId", caseId));
            return base.ExecuteNonQuery(strSql);
        }

        public int Create(Guid caseId, int amount1, int amount2, int amount3, int amount4, int amount5)
        {
            string strSql = @"insert into CaseCalculatorMain (CaseId,Amount1,Amount2,Amount3,Amount4,Amount5,CreatedDate)
                                values(@CaseId,@Amount1,@Amount2,@Amount3,@Amount4,@Amount5,@CreatedDate)";

            base.Parameter.Clear();
            base.Parameter.Add(new CommandParameter("@CaseId", caseId));
            base.Parameter.Add(new CommandParameter("@Amount1", amount1));
            base.Parameter.Add(new CommandParameter("@Amount2", amount2));
            base.Parameter.Add(new CommandParameter("@Amount3", amount3));
            base.Parameter.Add(new CommandParameter("@Amount4", amount4));
            base.Parameter.Add(new CommandParameter("@Amount5", amount5));
            base.Parameter.Add(new CommandParameter("@CreatedDate", DateTime.Now));
            return base.ExecuteNonQuery(strSql);
        }

        public int Count(Guid caseId)
        {
            string strSql = @" select count(0) from CaseCalculatorMain where CaseId=@CaseId";
            base.Parameter.Clear();
            base.Parameter.Add(new CommandParameter("@CaseId", caseId));
            return (int)base.ExecuteScalar(strSql);
        }

        public CaseCalculatorMain GetCaseMainInfo(Guid caseId)
        {
            string strSql = @"select convert(int,Amount1) as Amount1,convert(int,Amount2) as Amount2,convert(int,Amount3) as Amount3,
                                        convert(int,Amount4) as Amount4, convert(int,Amount5) as Amount5,CreatedUser,CreatedDate from 
                                        CaseCalculatorMain where CaseId=@CaseId";

            base.Parameter.Clear();
            base.Parameter.Add(new CommandParameter("@CaseId", caseId));
            IList<CaseCalculatorMain> _list = base.SearchList<CaseCalculatorMain>(strSql);
            if (_list != null)
            {
                if (_list.Count > 0) return _list[0];
                else return new CaseCalculatorMain();
            }
            else return new CaseCalculatorMain();
        }
    }
}
