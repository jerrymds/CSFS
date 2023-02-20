using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CTBC.CSFS.Models;
using CTBC.CSFS.Pattern;
using CTBC.CSFS.ViewModels;

namespace CTBC.CSFS.BussinessLogic
{
    public class CaseMemoBiz :CommonBIZ
    {
        public bool SaveMemo(CaseMemo memo, IDbTransaction trans = null)
        {
            //return Count(memo.CaseId, memo.MemoType, trans) > 0 ? Edit(memo, trans) : Create(memo, trans);
            return Create(memo, trans);
        }

        /// <summary>
        /// CaseMemo 新增
        /// </summary>
        /// <param name="model"></param>
        /// <param name="trans"></param>
        /// <returns></returns>
        public bool Create(CaseMemo model, IDbTransaction trans = null)
        {
            string strSql = @"INSERT INTO [CaseMemo] ([CaseId],[MemoType],[Memo],[MemoUser],[MemoDate]) VALUES (@CaseId, @MemoType, @Memo,@MemoUser,GETDATE());";
            base.Parameter.Clear();
            base.Parameter.Add(new CommandParameter("@CaseId", model.CaseId));
            base.Parameter.Add(new CommandParameter("@MemoType", model.MemoType));
            base.Parameter.Add(new CommandParameter("@Memo", model.Memo));
            base.Parameter.Add(new CommandParameter("@MemoUser", Account));
            return trans == null ? ExecuteNonQuery(strSql) > 0 : ExecuteNonQuery(strSql, trans) > 0;
        }

        public bool Delete(CaseMemo model, IDbTransaction trans = null)
        {
            string strSql = @"delete from CaseMemo where CaseId=@CaseId and MemoType=@MemoType";
            base.Parameter.Clear();
            base.Parameter.Add(new CommandParameter("@CaseId", model.CaseId));
            base.Parameter.Add(new CommandParameter("@MemoType", model.MemoType));
            return trans == null ? ExecuteNonQuery(strSql) > 0 : ExecuteNonQuery(strSql, trans) > 0;
        }
        /// <summary>
        /// CaseMemo 修改
        /// </summary>
        /// <param name="model"></param>
        /// <param name="trans"></param>
        /// <returns></returns>
        public bool Edit(CaseMemo model, IDbTransaction trans = null)
        {
            string strSql = @"UPDATE CaseMemo SET Memo=@Memo,MemoUser=@MemoUser,MemoDate=GETDATE() where CaseId=@CaseId AND [MemoType] = @MemoType ";
            base.Parameter.Clear();
            base.Parameter.Add(new CommandParameter("@CaseId", model.CaseId));
            base.Parameter.Add(new CommandParameter("@MemoType", model.MemoType));
            base.Parameter.Add(new CommandParameter("@Memo", model.Memo));
            base.Parameter.Add(new CommandParameter("@MemoUser", model.MemoUser));
            return trans == null ? ExecuteNonQuery(strSql) > 0 : ExecuteNonQuery(strSql, trans) > 0;
        }

        /// <summary>
        /// 取得CaseMemo
        /// </summary>
        /// <param name="caseId"></param>
        /// <param name="caseMemoType"></param>
        /// <param name="trans"></param>
        /// <returns></returns>
        public CaseMemo Memo(Guid caseId, string caseMemoType, IDbTransaction trans = null)
        {
            string sqlStr = @"SELECT top 1 [MemoId],[CaseId],[MemoType],[Memo] FROM [CaseMemo] WHERE CaseId=@CaseId AND [MemoType] = @MemoType order by MemoId desc";
            base.Parameter.Clear();
            base.Parameter.Add(new CommandParameter("@CaseId", caseId));
            base.Parameter.Add(new CommandParameter("@MemoType", caseMemoType));
            IList<CaseMemo> list = trans == null ? SearchList<CaseMemo>(sqlStr) : SearchList<CaseMemo>(sqlStr, trans);
            return list != null && list.Any() ? list[0] : new CaseMemo();
        }

        public List<CaseMemo> MemoList(string strCaseId, IDbTransaction trans = null)
        {
            string sqlStr = @"SELECT [CaseId],[Memo] 
                                            FROM [CaseMemo] 
                                        WHERE CaseId IN (" + strCaseId + ") AND [MemoType] = 'CaseExternal' ORDER BY CaseId ASC";
            base.Parameter.Clear();
            List<CaseMemo> list = trans == null ? SearchList<CaseMemo>(sqlStr).ToList() : SearchList<CaseMemo>(sqlStr, trans).ToList();
            return list != null && list.Any() ? list : new List<CaseMemo>();
        }

        /// <summary>
        /// 判斷CaseMemo是否存在
        /// </summary>
        /// <param name="caseId"></param>
        /// <param name="caseMemoType"></param>
        /// <param name="trans"></param>
        /// <returns></returns>
        public int Count(Guid caseId, string caseMemoType, IDbTransaction trans = null)
        {
            string sqlStr = @"SELECT count(*) FROM CaseMemo WHERE CaseId=@CaseId AND [MemoType] = @MemoType";
            base.Parameter.Clear();
            base.Parameter.Add(new CommandParameter("@CaseId", caseId));
            base.Parameter.Add(new CommandParameter("@MemoType", caseMemoType));
            return trans == null ? Convert.ToInt32(ExecuteScalar(sqlStr)) : Convert.ToInt32(ExecuteScalar(sqlStr, trans));
        }

        public List<CaseMemo> GetQueryList(CaseMemo memo, string caseMemoType)
        {
            base.Parameter.Clear();
            base.Parameter.Add(new CommandParameter("@CaseId", memo.CaseId));
            base.Parameter.Add(new CommandParameter("@MemoType", caseMemoType));
            string strSql = @"SELECT [MemoId],[CaseId],[MemoType],[Memo],[MemoDate],(select EmpName from LDAPEmployee where [MemoUser]=EmpID) as [MemoUser] FROM [CaseMemo] where CaseId=@CaseId and [MemoType] = @MemoType";
            return base.SearchList<CaseMemo>(strSql).ToList() ?? new List<CaseMemo>();
        }
    }
}