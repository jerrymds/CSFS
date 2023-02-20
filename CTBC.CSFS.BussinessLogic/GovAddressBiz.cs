using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CTBC.CSFS.Models;
using CTBC.CSFS.Pattern;
using NPOI.OpenXmlFormats.Dml;

namespace CTBC.CSFS.BussinessLogic
{
    public class GovAddressBIZ : CommonBIZ
    {
        public int Create(GovAddress model)
        {
            string sql = @" insert into GovAddress  (GovKind,GovName,GovAddr,IsEnabled,CreatedUser,CreatedDate) 
                                        values (
                                        @GovKind,@GovName,@GovAddr,@IsEnabled,@CreatedUser,@CreatedDate);";

            Parameter.Clear();

            // 添加參數
            Parameter.Add(new CommandParameter("@GovKind", model.GovKind));
            Parameter.Add(new CommandParameter("@GovName", model.GovName));
            Parameter.Add(new CommandParameter("@GovAddr", model.GovAddr));
            Parameter.Add(new CommandParameter("@IsEnabled", model.IsEnabled));
            Parameter.Add(new CommandParameter("@CreatedUser", model.CreatedUser));
            Parameter.Add(new CommandParameter("@CreatedDate", model.CreatedDate));
            return ExecuteNonQuery(sql);
        }

        public IList<GovAddress> GetQueryList(GovAddress model, int pageIndex, string strSortExpression, string strSortDirection)
        {
            try
            {
                base.PageIndex = pageIndex;
                string sqlStr = "";
                string sqlWhere="";
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@pageS", (base.PageSize * (base.PageIndex - 1)) + 1));
                base.Parameter.Add(new CommandParameter("@pageE", base.PageSize * base.PageIndex));

                if (!string.IsNullOrEmpty(model.GovKind))
                {
                    sqlWhere += @" and GovKind like @GovKind ";
                    base.Parameter.Add(new CommandParameter("@GovKind", "%" + model.GovKind + "%"));
                }
                if (!string.IsNullOrEmpty(model.GovName))
                {
                    sqlWhere += @" and GovName like @GovName ";
                    base.Parameter.Add(new CommandParameter("@GovName", "%" + model.GovName + "%"));
                }
                sqlStr += @";with T1 
	                        as
	                        (
		                     select [GovAddrId],[GovKind],[GovName],[GovAddr],[IsEnabled] from [dbo].[GovAddress] where 1=1 " + sqlWhere + @"   
	                        ),T2 as
	                        (
		                        select *, row_number() over (order by " + strSortExpression + " " + strSortDirection + @" ) RowNum
		                        from T1
	                        ),T3 as 
	                        (
		                        select *,(select max(RowNum) from T2) maxnum from T2 
		                        where rownum between @pageS and @pageE
	                        )
	                        select a.* from T3 a order by a.RowNum";

                IList<GovAddress> _ilsit = base.SearchList<GovAddress>(sqlStr);

                if (_ilsit != null)
                {
                    if (_ilsit.Count > 0)
                    {
                        base.DataRecords = _ilsit[0].maxnum;
                    }
                    else
                    {
                        base.DataRecords = 0;
                        _ilsit = new List<GovAddress>();
                    }
                    return _ilsit;
                }
                else
                {
                    return new List<GovAddress>();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public GovAddress getGovAddressByGovId(int govID)
        {
            string strSql = @"select GovAddrId,GovKind,GovName,GovAddr,IsEnabled from GovAddress where GovAddrId=@GovAddrId";
            base.Parameter.Clear();
            base.Parameter.Add(new CommandParameter("@GovAddrId", govID));
            IList<GovAddress> _list = base.SearchList<GovAddress>(strSql);
            if (_list != null && _list.Count > 0) return _list[0];
            else return new GovAddress();
        }

        public int Update(GovAddress model)
        {
            string sql = @" update  GovAddress set  GovKind=@GovKind,GovName=@GovName,GovAddr=@GovAddr,IsEnabled=@IsEnabled,
                                    ModifiedUser=@ModifiedUser,ModifiedDate=@ModifiedDate where GovAddrId=@GovAddrId";

            Parameter.Clear();

            // 添加參數
            Parameter.Add(new CommandParameter("@GovKind", model.GovKind));
            Parameter.Add(new CommandParameter("@GovName", model.GovName));
            Parameter.Add(new CommandParameter("@GovAddr", model.GovAddr));
            Parameter.Add(new CommandParameter("@IsEnabled", model.IsEnabled));
            Parameter.Add(new CommandParameter("@ModifiedUser", model.ModifiedUser));
            Parameter.Add(new CommandParameter("@ModifiedDate", model.ModifiedDate));
            Parameter.Add(new CommandParameter("@GovAddrId", model.GovAddrId));
            return ExecuteNonQuery(sql);
        }

        /// <summary>
        /// 取得地址表中的大類(GovKind)
        /// </summary>
        /// <returns></returns>
        public List<string> GetAllEnabledGovKind()
        {
            string strSql = @"SELECT DISTINCT [GovKind] FROM [GovAddress] WHERE [IsEnabled] = 1";
            Parameter.Clear();
            IList<string> list = SearchList<string>(strSql);
            return list != null && list.Any() ? list.ToList() : new List<string>();
        }

        /// <summary>
        /// 取得某一大類下所有 有效的 的機關和地址信息
        /// </summary>
        /// <param name="govKind"></param>
        /// <returns></returns>
        public List<GovAddress> GetEnabledGovAddrByGovKind(string govKind)
        {
            List<GovAddress> list = GetGovAddrByGovKind(govKind);
            if (list != null && list.Any())
                list.RemoveAll(m=>m.IsEnabled  == false);
            return list;
        }

        /// <summary>
        /// 取得某一大類下所有的機關和地址信息(包括禁用的)
        /// </summary>
        /// <param name="govKind"></param>
        /// <returns></returns>
        public List<GovAddress> GetGovAddrByGovKind(string govKind)
        {
            string strSql = @"SELECT [GovAddrId]
                                  ,[GovKind]
                                  ,[GovName]
                                  ,[GovAddr]
                                  ,[IsEnabled]
                                  ,[CreatedUser]
                                  ,[CreatedDate]
                                  ,[ModifiedUser]
                                  ,[ModifiedDate]
                            FROM  [GovAddress]
                            WHERE ISNULL(@GovKind,'') = '' OR [GovKind] = @GovKind";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("GovKind", govKind));
            IList<GovAddress> list = SearchList<GovAddress>(strSql);
            return list != null && list.Any() ? list.ToList() : new List<GovAddress>();
        }
        
        public List<GovAddress> QueryGovAddrByGovKindAndName(string govKind, string govName)
        {
            string strSql = @"SELECT [GovAddrId]
                                  ,[GovKind]
                                  ,[GovName]
                                  ,[GovAddr]
                                  ,[IsEnabled]
                                  ,[CreatedUser]
                                  ,[CreatedDate]
                                  ,[ModifiedUser]
                                  ,[ModifiedDate]
                            FROM  [GovAddress]
                            WHERE [IsEnabled] = 1 AND [GovKind] LIKE @GovKind AND [GovName] LIKE @GovName ";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("GovKind", "%" + govKind + "%"));
            Parameter.Add(new CommandParameter("GovName", "%" + govName + "%"));
            IList<GovAddress> list = SearchList<GovAddress>(strSql);
            return list != null && list.Any() ? list.ToList() : new List<GovAddress>();
        }

        /// <summary>
        /// 通過來文機關得到這個機關的地址
        /// </summary>
        /// <param name="govName"></param>
        /// <returns></returns>
        public string GetEnabledGovAddrByGovName(string govName)
        {
            Parameter.Clear();
            Parameter.Add(new CommandParameter("GovName", govName));
            return ExecuteScalar("SELECT [GovAddr] FROM [GovAddress] WHERE [GovName] = @GovName AND [IsEnabled] = 1") as string;
        }
    }
}
