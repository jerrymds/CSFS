using CTBC.CSFS.Models;
using CTBC.CSFS.Pattern;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NPOI.HSSF.UserModel;

namespace CTBC.CSFS.BussinessLogic
{
    public class HistoryCaseHistoryBIZ : CommonBIZ
    {
		public DataTable getCaseStatus(Guid caseId)
		{
			string strSql = @"select top 1 Event from CaseHistory where CaseId = @CaseId order by CreatedDate desc";
			base.Parameter.Clear();
			base.Parameter.Add(new CommandParameter("@CaseId", caseId.ToString()));
			return base.Search(strSql) == null ? new DataTable() : base.Search(strSql);
		}

        public List<HistoryCaseHistory> getHistoryByCaseId(Guid caseId)
        {
            string strSql = "select * from CaseHistory where caseId = @CaseId";
            base.Parameter.Clear();
            base.Parameter.Add(new CommandParameter("@CaseId", caseId.ToString()));
            return base.SearchList<HistoryCaseHistory>(strSql).ToList() ?? new List<HistoryCaseHistory>();
        }

        public bool insertCaseHistory(HistoryCaseHistory item,IDbTransaction dbTransaction = null)
        {
            string strSql = "insert into HistoryCaseHistory (CaseId,FromRole,FromUser,FromFolder,Event,EventTime,ToRole,ToUser,ToFolder,CreatedUser,CreatedDate) values "+
                "(@CaseId,@FromRole,@FromUser,@FromFolder,@Event,@EventTime,@ToRole,@ToUser,@ToFolder,@CreatedUser,@CreatedDate) ";
            base.Parameter.Clear();
            base.Parameter.Add(new CommandParameter("@CaseId",item.CaseId));
            base.Parameter.Add(new CommandParameter("@FromRole",item.FromRole));
            base.Parameter.Add(new CommandParameter("@FromUser",item.FromUser));
            base.Parameter.Add(new CommandParameter("@FromFolder",item.FromFolder));
            base.Parameter.Add(new CommandParameter("@Event",item.Event));
            base.Parameter.Add(new CommandParameter("@EventTime",item.EventTime));
            base.Parameter.Add(new CommandParameter("@ToRole",item.ToRole));
            base.Parameter.Add(new CommandParameter("@ToUser",item.ToUser));
            base.Parameter.Add(new CommandParameter("@ToFolder",item.ToFolder));
            base.Parameter.Add(new CommandParameter("@CreatedUser",item.CreatedUser));
            base.Parameter.Add(new CommandParameter("@CreatedDate",item.CreatedDate));
            return base.ExecuteNonQuery(strSql) > 0;
        }

        public bool insertCaseHistory(Guid caseId, string action, string userId, IDbTransaction dbTransaction = null)
        {

            HistoryCaseHistory old = getHistoryByCaseId(caseId).OrderByDescending(m => m.HistoryId).FirstOrDefault();

            PARMCode obj = GetCodeData("EVENT_NAME").FirstOrDefault(m => m.CodeNo == action.ToString());
            string eventName = obj != null ? obj.CodeDesc : "";

            obj = GetCodeData("STATUS_NAME").FirstOrDefault(m => m.CodeNo == action.ToString());
            string toFolder = obj != null ? obj.CodeDesc : "";

            obj = GetCodeData("EVENT_ROLE").FirstOrDefault(m => m.CodeNo == action.ToString());
            string toRole = obj != null ? obj.CodeDesc : "";

            HistoryCaseHistory newHistory = new HistoryCaseHistory()
            {
                CaseId = caseId,
                FromRole = old != null ? old.ToRole : "",
                FromUser = old != null ? old.ToUser : "",
                FromFolder = old != null ? old.ToFolder : "",
                Event = eventName,
                EventTime = GetNowDateTime(),
                ToRole = toRole,
                ToUser = userId,
                ToFolder = toFolder,
                CreatedUser = userId,
                CreatedDate = GetNowDateTime()

            };

            return insertCaseHistory(newHistory, dbTransaction);
        }
        public bool insertCaseHistory2(Guid caseId, string action, string userId, IDbTransaction dbTransaction = null)
        {

            HistoryCaseHistory old = getHistoryByCaseId(caseId).OrderByDescending(m => m.HistoryId).FirstOrDefault();

            PARMCode obj = GetCodeData("EVENT_NAME").FirstOrDefault(m => m.CodeNo == action.ToString());
            string eventName = obj != null ? obj.CodeDesc : "";

            obj = GetCodeData("STATUS_NAME").FirstOrDefault(m => m.CodeNo == action.ToString());
            string toFolder = obj != null ? obj.CodeDesc : "";

            obj = GetCodeData("EVENT_ROLE").FirstOrDefault(m => m.CodeNo == action.ToString());
            string toRole = obj != null ? obj.CodeDesc : "";

            HistoryCaseHistory newHistory = new HistoryCaseHistory()
            {
                CaseId = caseId,
                FromRole = "自動化處理",
                FromUser = userId,
                FromFolder = "結案-上傳",
                Event = eventName,
                EventTime = GetNowDateTime(),
                ToRole = "自動化處理",
                ToUser = userId,
                ToFolder = "結案-重傳",
                CreatedUser = userId,
                CreatedDate = GetNowDateTime()

            };

            return insertCaseHistory(newHistory, dbTransaction);
        }

        public MemoryStream SimpleSampleExcel(Guid caseId)
        {
            List<HistoryCaseHistory> list = getHistoryByCaseId(caseId);
            string[] headerColumns = new[]
                    {
                        "案件編號",
                        "前步驟角色",
                        "前步骤人員",
                        "前步驟",
                        "事件",
                        "時間",
                        "角色",
                        "人員",
                        "步驟"
                    };
            var ms = new MemoryStream();

            if (list != null)
            {
                ms = ExcelExport(list, headerColumns,
                                               delegate(HSSFRow dataRow, HistoryCaseHistory dataItem)
                                               {
                                                   //* 這裡可以針對每一個欄位做額外處理.比如日期
                                                   dataRow.CreateCell(0).SetCellValue(dataItem.CaseId.ToString());
                                                   dataRow.CreateCell(1).SetCellValue(dataItem.FromRole);
                                                   dataRow.CreateCell(2).SetCellValue(dataItem.FromUser);
                                                   dataRow.CreateCell(3).SetCellValue(dataItem.FromFolder);
                                                   dataRow.CreateCell(4).SetCellValue(dataItem.Event);
                                                   dataRow.CreateCell(5).SetCellValue(dataItem.EventTime.ToString("yyyy/MM/dd"));
                                                   dataRow.CreateCell(6).SetCellValue(dataItem.ToRole);
                                                   dataRow.CreateCell(7).SetCellValue(dataItem.ToUser);
                                                   dataRow.CreateCell(8).SetCellValue(dataItem.ToFolder);
                                               });
            }
            return ms;
        }

    }
}
