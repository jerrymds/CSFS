using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CTBC.CSFS.Models;
using CTBC.CSFS.Pattern;
using CTBC.CSFS.ViewModels;

namespace CTBC.CSFS.BussinessLogic
{
    public class HistoryCaseMemoBiz :CommonBIZ
    {
        public bool SaveMemo(HistoryCaseMemo memo, IDbTransaction trans = null)
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
        public bool Create(HistoryCaseMemo model, IDbTransaction trans = null)
        {
            string strSql = @"INSERT INTO [History_CaseMemo] ([CaseId],[MemoType],[Memo],[MemoUser],[MemoDate]) VALUES (@CaseId, @MemoType, @Memo,@MemoUser,GETDATE());";
            base.Parameter.Clear();
            base.Parameter.Add(new CommandParameter("@CaseId", model.CaseId));
            base.Parameter.Add(new CommandParameter("@MemoType", model.MemoType));
            base.Parameter.Add(new CommandParameter("@Memo", model.Memo,  CTBC.CSFS.Pattern.FieldType.NVarchar));
            base.Parameter.Add(new CommandParameter("@MemoUser", Account));
            return trans == null ? ExecuteNonQuery(strSql) > 0 : ExecuteNonQuery(strSql, trans) > 0;
        }
        
         /// <summary>
        /// CaseMemo 新增 Edit by Patrick 20180731
        /// </summary>
        /// <param name="model"></param>
        /// <param name="trans"></param>
        /// <returns></returns>
        public bool Create2(HistoryCaseMemo model, IDbTransaction trans = null)
        {
            string strSql = @"INSERT INTO [History_CaseMemo] ([CaseId],[MemoType],[Memo],[MemoUser],[MemoDate]) VALUES (@CaseId, @MemoType, @Memo,@MemoUser,GETDATE());";
            base.Parameter.Clear();
            base.Parameter.Add(new CommandParameter("@CaseId", model.CaseId));
            base.Parameter.Add(new CommandParameter("@MemoType", model.MemoType));
            base.Parameter.Add(new CommandParameter("@Memo", model.Memo, CTBC.CSFS.Pattern.FieldType.NVarchar));
            base.Parameter.Add(new CommandParameter("@MemoUser", model.MemoUser));
            return trans == null ? ExecuteNonQuery(strSql) > 0 : ExecuteNonQuery(strSql, trans) > 0;
        }


        public bool Delete(HistoryCaseMemo model, IDbTransaction trans = null)
        {
            string strSql = @"delete from History_CaseMemo where CaseId=@CaseId and MemoType=@MemoType";
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
        public bool Edit(HistoryCaseMemo model, IDbTransaction trans = null)
        {
            string strSql = @"UPDATE History_CaseMemo SET Memo=@Memo,MemoUser=@MemoUser,MemoDate=GETDATE() where CaseId=@CaseId AND [MemoType] = @MemoType ";
            base.Parameter.Clear();
            base.Parameter.Add(new CommandParameter("@CaseId", model.CaseId));
            base.Parameter.Add(new CommandParameter("@MemoType", model.MemoType));
            base.Parameter.Add(new CommandParameter("@Memo", model.Memo, CTBC.CSFS.Pattern.FieldType.NVarchar));
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
        public HistoryCaseMemo Memo(Guid caseId, string caseMemoType, IDbTransaction trans = null)
        {
            string sqlStr = @"SELECT top 1 [MemoId],[CaseId],[MemoType],convert(nvarchar(4000), [Memo]) as [Memo] FROM [History_CaseMemo] WHERE CaseId=@CaseId AND [MemoType] = @MemoType order by MemoId desc";
            base.Parameter.Clear();
            base.Parameter.Add(new CommandParameter("@CaseId", caseId));
            base.Parameter.Add(new CommandParameter("@MemoType", caseMemoType));
            IList<HistoryCaseMemo> list = trans == null ? SearchList<HistoryCaseMemo>(sqlStr) : SearchList<HistoryCaseMemo>(sqlStr, trans);
            return list != null && list.Any() ? list[0] : new HistoryCaseMemo();
        }
        /// <summary>
        /// 20180802, Written by Patrick 
        /// </summary>
        /// <param name="caseId"></param>
        /// <param name="caseMemoType"></param>
        /// <param name="trans"></param>
        /// <returns></returns>
        public List<HistoryCaseMemo> MemoList2(Guid caseId, string caseMemoType, IDbTransaction trans = null)
        {
            string sqlStr = @"SELECT distinct [CaseId],[MemoType],convert(nvarchar(4000), [Memo]) as [Memo] FROM [History_CaseMemo] WHERE CaseId=@CaseId AND [MemoType] = @MemoType";
            base.Parameter.Clear();
            base.Parameter.Add(new CommandParameter("@CaseId", caseId));
            base.Parameter.Add(new CommandParameter("@MemoType", caseMemoType));
            IList<HistoryCaseMemo> list = trans == null ? SearchList<HistoryCaseMemo>(sqlStr) : SearchList<HistoryCaseMemo>(sqlStr, trans);
            return list != null && list.Any() ? list.ToList() : new List<HistoryCaseMemo>();
        }

        /// <summary>
        /// 20181003, Written by Patrick , 主要刪除行號中, 備註欄位, 另外多記錄的訊息
        /// </summary>
        /// <param name="model"></param>
        /// <param name="trans"></param>
        /// <returns></returns>
        public bool Delete2(HistoryCaseMemo model, IDbTransaction trans = null)
        {
            string strSql = @"delete from History_CaseMemo where CaseId=@CaseId and MemoType=@MemoType and Memo like '%!@!%' ";
            base.Parameter.Clear();
            base.Parameter.Add(new CommandParameter("@CaseId", model.CaseId));
            base.Parameter.Add(new CommandParameter("@MemoType", model.MemoType));
            return trans == null ? ExecuteNonQuery(strSql) > 0 : ExecuteNonQuery(strSql, trans) > 0;
        }

        /// <summary>
        /// 20181128, Written by Patrick , 主要變把行號中, 備註欄位, 原來有!@!, 改成 "其中 ID ，XXXXX" 
        /// </summary>
        /// <param name="model"></param>
        /// <param name="trans"></param>
        /// <returns></returns>
        public bool Update2(HistoryCaseMemo model, IDbTransaction trans = null)
        {
            string strSql = @"update  History_CaseMemo set Memo='其中統編：' + REPLACE(cast( Memo as nvarchar(max)),'!@!','，') where caseid=@CaseId and MemoType=@MemoType and Memo not like '其中統編%' AND Memo like '%!@!%' ";

            //string strSql = @"delete from CaseMemo where CaseId=@CaseId and MemoType=@MemoType and Memo like '%!@!%' ";
            base.Parameter.Clear();
            base.Parameter.Add(new CommandParameter("@CaseId", model.CaseId));
            base.Parameter.Add(new CommandParameter("@MemoType", model.MemoType));
            return trans == null ? ExecuteNonQuery(strSql) > 0 : ExecuteNonQuery(strSql, trans) > 0;
        }

        /// <summary>
        /// 20181003, Written by Patrick , 主要刪除行號中, 備註欄位, 另外多記錄的訊息
        /// </summary>
        /// <param name="model"></param>
        /// <param name="trans"></param>
        /// <returns></returns>
        public bool Delete3(Guid CaseId, string MemoType, string delString , IDbTransaction trans = null)
        {
            string strSql = @"delete from History_CaseMemo where CaseId=@CaseId and MemoType=@MemoType and Memo like '%" + delString + "%' ";
            base.Parameter.Clear();
            base.Parameter.Add(new CommandParameter("@CaseId", CaseId));
            base.Parameter.Add(new CommandParameter("@MemoType", MemoType));
            //base.Parameter.Add(new CommandParameter("@delString", delString));
            return trans == null ? ExecuteNonQuery(strSql) > 0 : ExecuteNonQuery(strSql, trans) > 0;
        }


        public List<HistoryCaseMemo> MemoList(string strCaseId, IDbTransaction trans = null)
        {
            string sqlStr = @"SELECT [CaseId],convert(nvarchar(4000), [Memo]) as [Memo]
                                            FROM [History_CaseMemo] 
                                        WHERE CaseId IN (" + strCaseId + ") AND [MemoType] = 'CaseExternal' ORDER BY CaseId ASC";
            base.Parameter.Clear();
            List<HistoryCaseMemo> list = trans == null ? SearchList<HistoryCaseMemo>(sqlStr).ToList() : SearchList<HistoryCaseMemo>(sqlStr, trans).ToList();
            return list != null && list.Any() ? list : new List<HistoryCaseMemo>();
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
            string sqlStr = @"SELECT count(*) FROM History_CaseMemo WHERE CaseId=@CaseId AND [MemoType] = @MemoType";
            base.Parameter.Clear();
            base.Parameter.Add(new CommandParameter("@CaseId", caseId));
            base.Parameter.Add(new CommandParameter("@MemoType", caseMemoType));
            return trans == null ? Convert.ToInt32(ExecuteScalar(sqlStr)) : Convert.ToInt32(ExecuteScalar(sqlStr, trans));
        }

        public List<HistoryCaseMemo> GetQueryList(HistoryCaseMemo memo, string caseMemoType)
        {
            base.Parameter.Clear();
            base.Parameter.Add(new CommandParameter("@CaseId", memo.CaseId));
            base.Parameter.Add(new CommandParameter("@MemoType", caseMemoType));
            string strSql = @"SELECT [MemoId],[CaseId],[MemoType],convert(nvarchar(4000), [Memo]) as [Memo],[MemoDate],(select EmpName from LDAPEmployee where [MemoUser]=EmpID) as [MemoUser] FROM [History_CaseMemo] where CaseId=@CaseId and [MemoType] = @MemoType";
            return base.SearchList<HistoryCaseMemo>(strSql).ToList() ?? new List<HistoryCaseMemo>();
        }
    }
}