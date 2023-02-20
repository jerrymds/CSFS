using CTBC.CSFS.Models;
using CTBC.CSFS.Pattern;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTBC.CSFS.BussinessLogic
{
    public class SendNoSettingBIZ : CommonBIZ
    {
        public SendNoSettingBIZ(AppController appController)
            : base(appController)
        { }

        public SendNoSettingBIZ()
        { }

        public IList<SendNoSetting> GetQueryList(SendNoSetting sns, int pageIndex, string strSortExpression, string strSortDirection)
        {
            try
            {
                base.PageIndex = pageIndex;
                string sqlStr = "";
                string sqlWhere = "";
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@pageS", (base.PageSize * (base.PageIndex - 1)) + 1));
                base.Parameter.Add(new CommandParameter("@pageE", base.PageSize * base.PageIndex));

                if (!string.IsNullOrEmpty(sns.SendNoYear))
                {
                    sqlWhere += @" and SendNoYear like @SendNoYear ";
                    base.Parameter.Add(new CommandParameter("@SendNoYear", "%" + sns.SendNoYear.Trim() + "%"));
                }
                sqlStr += @";with T1 
	                        as
	                        (
		                       SELECT [SendNoId]
                               ,[SendNoYear]
                               ,[SendNoStart]
                               ,[SendNoEnd]
                               ,[SendNoNow]
                               FROM [SendNoTable] where 1=1" + sqlWhere + @"
	                        ),T2 as
	                        (
		                        select *, row_number() over (order by " + strSortExpression + " " + strSortDirection + @" ) RowNum
		                        from T1
	                        ),T3 as 
	                        (
		                        select *,(select max(RowNum) from T2) maxnum from T2 
		                        where rownum between @pageS and @pageE
	                        )
	                        select a.* from T3 a ";


                IList<SendNoSetting> _ilsit = base.SearchList<SendNoSetting>(sqlStr);

                if (_ilsit.Count > 0)
                {
                    base.DataRecords = _ilsit[0].maxnum;
                }
                else
                {
                    base.DataRecords = 0;
                    _ilsit = new List<SendNoSetting>();
                }
                return _ilsit;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public int Create(SendNoSetting model)
        {
            int rtn = 0;
            IDbConnection dbConnection = base.OpenConnection();
            IDbTransaction dbTransaction = null;
            try
            {
                using (dbConnection)
                {
                    dbTransaction = dbConnection.BeginTransaction();
                    string sqlStr = @"insert into SendNoTable
                                      (SendNoYear,SendNoStart,SendNoEnd,SendNoNow)
                                      values(@SendNoYear,@SendNoStart,@SendNoEnd,@SendNoNow);";

                    base.Parameter.Clear();
                    base.Parameter.Add(new CommandParameter("@SendNoYear", model.SendNoYear));
                    base.Parameter.Add(new CommandParameter("@SendNoStart", model.SendNoStart));
                    base.Parameter.Add(new CommandParameter("@SendNoEnd", model.SendNoEnd));
                    base.Parameter.Add(new CommandParameter("@SendNoNow", model.SendNoNow));

                    rtn = base.ExecuteNonQuery(sqlStr, dbTransaction);
                    dbTransaction.Commit();
                }
                return rtn;
            }
            catch (Exception ex)
            {
                try
                {
                    dbTransaction.Rollback();
                }
                catch (Exception ex2)
                {
                }
                throw ex;
            }
        }
        public SendNoSetting Select(int SendNoId)
        {
            try
            {
                string sqlStr = @"select * from SendNoTable where SendNoId=@SendNoId";

                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@SendNoId", SendNoId));

                IList<SendNoSetting> list = base.SearchList<SendNoSetting>(sqlStr);
                if (list != null)
                {
                    if (list.Count > 0)
                    {
                        return list[0];
                    }
                    else
                    {
                        return new SendNoSetting();
                    }
                }
                else
                {
                    return new SendNoSetting();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public int Edit(SendNoSetting model)
        {
            int rtn = 0;
            IDbConnection dbConnection = base.OpenConnection();
            IDbTransaction dbTransaction = null;
            try
            {
                using (dbConnection)
                {
                    dbTransaction = dbConnection.BeginTransaction();
                    string sqlStr = @"update SendNoTable set 
                                            SendNoYear=@SendNoYear,
                                            SendNoStart=@SendNoStart,
                                            SendNoEnd=@SendNoEnd,
                                            SendNoNow=@SendNoNow
                                    where SendNoId=@SendNoId";

                    base.Parameter.Clear();
                    base.Parameter.Add(new CommandParameter("@SendNoId", model.SendNoId));
                    base.Parameter.Add(new CommandParameter("@SendNoYear", model.SendNoYear));
                    base.Parameter.Add(new CommandParameter("@SendNoStart", model.SendNoStart));
                    base.Parameter.Add(new CommandParameter("@SendNoEnd", model.SendNoEnd));
                    base.Parameter.Add(new CommandParameter("@SendNoNow", model.SendNoNow));
                    rtn = base.ExecuteNonQuery(sqlStr, dbTransaction);
                    dbTransaction.Commit();
                }
                return rtn;
            }
            catch (Exception ex)
            {
                try
                {
                    dbTransaction.Rollback();
                }
                catch (Exception ex2)
                {
                }
                throw ex;
            }
        }
        public int Delete(int SendNoId)
        {
            int rtn = 0;
            IDbConnection dbConnection = base.OpenConnection();
            IDbTransaction dbTransaction = null;
            try
            {
                using (dbConnection)
                {
                    dbTransaction = dbConnection.BeginTransaction();
                    string sqlStr = @"delete from SendNoTable where SendNoId=@SendNoId";

                    base.Parameter.Clear();
                    base.Parameter.Add(new CommandParameter("@SendNoId", SendNoId));
                    rtn = base.ExecuteNonQuery(sqlStr, dbTransaction);
                    dbTransaction.Commit();                   
                }
                return rtn;
            }
            catch (Exception ex)
            {
                try
                {
                    dbTransaction.Rollback();
                }
                catch (Exception ex2)
                {
                }
                throw ex;
            }
        }

       
    }
}
