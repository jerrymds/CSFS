using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CTBC.CSFS.Models;
using CTBC.CSFS.Pattern;

namespace CTBC.CSFS.BussinessLogic
{
    public class HistoryLendAttachmentBIZ : CommonBIZ
    {
        //新增
        public int Create(HistoryLendAttachment model, IDbTransaction trans = null)
        {
            string strSql = @" insert into History_LendAttachment  (LendId,CaseId,LendAttachName,LendAttachServerPath,LendAttachServerName,isDelete,CreatedUser,CreatedDate) 
                                        values (
                                        @LendId,@CaseId,@LendAttachName,@LendAttachServerPath,@LendAttachServerName,@isDelete,@CreatedUser,GETDATE());";

            base.Parameter.Clear();

            // 添加參數
            base.Parameter.Add(new CommandParameter("@LendId", model.LendId));
            base.Parameter.Add(new CommandParameter("@CaseId", model.CaseId));
            base.Parameter.Add(new CommandParameter("@LendAttachName", model.LendAttachName));
            base.Parameter.Add(new CommandParameter("@LendAttachServerPath", model.LendAttachServerPath));
            base.Parameter.Add(new CommandParameter("@LendAttachServerName", model.LendAttachServerName));
            base.Parameter.Add(new CommandParameter("@isDelete", model.isDelete));
            base.Parameter.Add(new CommandParameter("@CreatedUser", model.CreatedUser));

            try
            {
                if (trans != null)
                {
                    return base.ExecuteNonQuery(strSql, trans);
                }
                IDbConnection dbConnection = base.OpenConnection();
                IDbTransaction tans = dbConnection.BeginTransaction();
                return base.ExecuteNonQuery(strSql, trans);
            }
            catch (Exception ex)
            {
                // 拋出異常
                throw ex;
            }
        }

        //刪除
        public int DeleteAttatch(string LendId, IDbTransaction trans = null)
        {
            string strSql = @" delete from  History_LendAttachment where LendId=@LendId";
            Parameter.Clear();
            // 添加參數
            Parameter.Add(new CommandParameter("@LendId", LendId));
            return trans == null ? ExecuteNonQuery(strSql) : ExecuteNonQuery(strSql, trans);
        }

        //得到List
        public List<HistoryLendAttachment> getAttachList(string lendid)
        {
            string strSql = @" select l.LendAttachId,l.LendAttachServerPath,l.LendAttachName,l.LendAttachServerName from History_LendAttachment l
                                        where LendId=@LendId";
            Parameter.Clear();
            // 添加參數
            Parameter.Add(new CommandParameter("@LendId", lendid));
            IList<HistoryLendAttachment> _list = base.SearchList<HistoryLendAttachment>(strSql);
            List<HistoryLendAttachment> listItem = new List<HistoryLendAttachment>();
            foreach (var item in _list)
            {
                listItem.Add(item);
            }
            return listItem==null?null:listItem;
        }

        public HistoryLendAttachment GetAttachmentInfo(int attachId)
        {
            string strSql = "select l.LendAttachId,l.LendAttachServerPath,l.LendAttachName,l.LendAttachServerName from History_LendAttachment l " +
                            "WHERE [LendAttachId]=@LendAttachId";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("@LendAttachId", attachId));
            IList<HistoryLendAttachment> list = SearchList<HistoryLendAttachment>(strSql);
            return list.FirstOrDefault();
        }
    }
}
