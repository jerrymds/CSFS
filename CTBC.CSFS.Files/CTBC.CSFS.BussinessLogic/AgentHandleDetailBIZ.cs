using CTBC.CSFS.Models;
using CTBC.CSFS.Pattern;
using CTBC.FrameWork.Util;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CTBC.CSFS.ViewModels;

namespace CTBC.CSFS.BussinessLogic
{
    public class AgentHandleDetailBIZ : CommonBIZ
    {
        public AgentHandleDetailBIZ(AppController appController)
            : base(appController)
        { }

        public AgentHandleDetailBIZ()
        { }

        public List<AgentHandleDetail> GetQueryList(AgentHandleDetail AHD)
        {
            base.Parameter.Clear();
            base.Parameter.Add(new CommandParameter("@CaseId", AHD.CaseId));

            string strSql = @"select HistoryId,CaseId,(select CaseNo from CaseMaster A where A.CaseId=B.CaseId ) as CaseNo,
                             FromRole,FromUser,FromFolder,EventTime,Event,ToRole,ToUser,ToFolder from CaseHistory as B where CaseId=@CaseId order by B.EventTime";
            return base.SearchList<AgentHandleDetail>(strSql).ToList() ?? new List<AgentHandleDetail>();
        }
        public AgentHandleDetail Select(int HistoryId)
        {
            try
            {
                string sqlStr = @"select * from CaseHistory where HistoryId=@HistoryId";

                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@HistoryId", HistoryId));

                IList<AgentHandleDetail> list = base.SearchList<AgentHandleDetail>(sqlStr);
                if (list != null)
                {
                    if (list.Count > 0)
                    {
                        return list[0];
                    }
                    else
                    {
                        return new AgentHandleDetail();
                    }
                }
                else
                {
                    return new AgentHandleDetail();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
