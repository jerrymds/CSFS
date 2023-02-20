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
    public class LendAttachmentBIZ : CommonBIZ
    {
        //新增
        public int Create(LendAttachment model, IDbTransaction trans = null)
        {
            string strSql = @" insert into LendAttachment  (LendId,CaseId,LendAttachName,LendAttachServerPath,LendAttachServerName,isDelete,CreatedUser,CreatedDate) 
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
            string strSql = @" delete from  LendAttachment where LendId=@LendId";
            Parameter.Clear();
            // 添加參數
            Parameter.Add(new CommandParameter("@LendId", LendId));
            return trans == null ? ExecuteNonQuery(strSql) : ExecuteNonQuery(strSql, trans);
        }

        //得到List
        public List<LendAttachment> getAttachList(string lendid)
        {
            string strSql = @" select l.LendAttachId,l.LendAttachServerPath,l.LendAttachName,l.LendAttachServerName from LendAttachment l
                                        where LendId=@LendId";
            Parameter.Clear();
            // 添加參數
            Parameter.Add(new CommandParameter("@LendId", lendid));
            IList<LendAttachment> _list = base.SearchList<LendAttachment>(strSql);
            List<LendAttachment> listItem = new List<LendAttachment>();
            foreach (var item in _list)
            {
                listItem.Add(item);
            }
            return listItem==null?null:listItem;
        }

        public LendAttachment GetAttachmentInfo(int attachId)
        {
            string strSql = "select l.LendAttachId,l.LendAttachServerPath,l.LendAttachName,l.LendAttachServerName from LendAttachment l " +
                            "WHERE [LendAttachId]=@LendAttachId";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("@LendAttachId", attachId));
            IList<LendAttachment> list = SearchList<LendAttachment>(strSql);
            return list.FirstOrDefault();
        }
        public HistoryLendAttachment GetHistoryAttachmentInfo(int attachId)
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
