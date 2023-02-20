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
    public class HistoryAgentHandleDetailBIZ : CommonBIZ
    {
        public HistoryAgentHandleDetailBIZ(AppController appController)
            : base(appController)
        { }

        public HistoryAgentHandleDetailBIZ()
        { }

        public List<HistoryAgentHandleDetail> GetQueryList(HistoryAgentHandleDetail AHD)
        {
            base.Parameter.Clear();
            base.Parameter.Add(new CommandParameter("@CaseId", AHD.CaseId));

            string strSql = @"select HistoryId,CaseId,(select CaseNo from CaseMaster A where A.CaseId=B.CaseId ) as CaseNo,
                             FromRole,FromUser,FromFolder,EventTime,Event,ToRole,ToUser,ToFolder from History_CaseHistory as B where CaseId=@CaseId order by B.EventTime";
            return base.SearchList<HistoryAgentHandleDetail>(strSql).ToList() ?? new List<HistoryAgentHandleDetail>();
        }
        public HistoryAgentHandleDetail Select(int HistoryId)
        {
            try
            {
                string sqlStr = @"select * from History_CaseHistory where HistoryId=@HistoryId";

                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@HistoryId", HistoryId));

                IList<HistoryAgentHandleDetail> list = base.SearchList<HistoryAgentHandleDetail>(sqlStr);
                if (list != null)
                {
                    if (list.Count > 0)
                    {
                        return list[0];
                    }
                    else
                    {
                        return new HistoryAgentHandleDetail();
                    }
                }
                else
                {
                    return new HistoryAgentHandleDetail();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
