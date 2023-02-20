using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using CTBC.FrameWork.Platform;
using CTBC.CSFS.Pattern;
using System.Text;
using CTBC.CSFS.Models;

namespace CTBC.CSFS.BussinessLogic
{
    public class CSFSBUBIZ : CommonBIZ
    {
        public CSFSBUBIZ(AppController appController)
            : base(appController)
        { }

        public CSFSBUBIZ()
        { }

        /// <summary>
        /// 獲取所有代碼檔資料
        /// </summary>
        /// <returns></returns>
        public IEnumerable<CSFSBU> SelectAllCSFSBU()
        {
            try
            {
                string sql = @"select * from CSFSBU where Enable='1'";
                base.Parameter.Clear();
                return base.SearchList<CSFSBU>(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<CSFSBUMaster> SelectAllCSFSBUMaster()
        {
            try
            {
                string sql = @"select * from CSFSBUMaster";
                base.Parameter.Clear();
                return base.SearchList<CSFSBUMaster>(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IList<CSFSBUMaster> GetAllCSFSBUMaster()
        {
            try
            {
                string sql = @"select a.* , a.BUMasterName+'-'+a.BUMasterDesc as BUMasterDesc2 from CSFSBUMaster a order by BUMasterID";
                base.Parameter.Clear();
                return base.SearchList<CSFSBUMaster>(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public IList<CSFSBUMaster> GetCSFSBUMaster(string bumasterid)
        {
            try
            {
                string sql = @"select * from CSFSBUMaster where BUMasterID=@BUMasterID";
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@BUMasterID", bumasterid));
                return base.SearchList<CSFSBUMaster>(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IList<CSFSBU> GetBUData(string buid)
        {
            try
            {
                string sql = @"select * from CSFSBU where BUID=@BUID";
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@BUID", buid));
                return base.SearchList<CSFSBU>(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IList<CSFSBU> GetBUDataByParent(string buparent, string bumasterid)
        {
            try
            {
                string sql = @"select a.* ,a.BUNumber + '-' + a.BUName as BuNumberName  from CSFSBU a  
                            where BUParent=@BUParent  and 
                            BUMasterID =@BUMasterID and Enable='1' order by Sort";
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@BUParent", buparent));
                base.Parameter.Add(new CommandParameter("@BUMasterID", bumasterid));
                return base.SearchList<CSFSBU>(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_obj"></param>
        /// <param name="pageIndex"></param>
        /// <returns></returns>
        /// <remarks>add by mel 20130911</remarks>
        public IList<CSFSBUToEmployee> GetBuToEmployeeUnion(CSFSBUToEmployee _obj, int pageIndex)
        {
            try
            {
                base.PageIndex = pageIndex;
                base.Parameter.Clear();

                if (!string.IsNullOrEmpty(_obj.Level4BUID))
                    _obj.BUID = Convert.ToInt16(_obj.Level4BUID);
                else if (!string.IsNullOrEmpty(_obj.Level3BUID))
                    _obj.BUID = Convert.ToInt16(_obj.Level3BUID);
                else if (!string.IsNullOrEmpty(_obj.Level2BUID))
                    _obj.BUID = Convert.ToInt16(_obj.Level2BUID);
                else if (!string.IsNullOrEmpty(_obj.Level1BUID))
                    _obj.BUID = Convert.ToInt16(_obj.Level1BUID);
                else
                    _obj.BUParent = 0;



                StringBuilder sqlParameter1 = new StringBuilder("");
                StringBuilder sqlParameter2 = new StringBuilder("");
                StringBuilder sqlParameter3 = new StringBuilder("");
                sqlParameter3.Append(@"DECLARE @BTCtreeName VARCHAR(MAX)
                                       DECLARE @BTCtreeNumBer VARCHAR(MAX)");

                #region 查詢條件
                if (string.IsNullOrEmpty(_obj.Level4BUID) &&
                    string.IsNullOrEmpty(_obj.Level3BUID) &&
                    string.IsNullOrEmpty(_obj.Level2BUID) &&
                    string.IsNullOrEmpty(_obj.Level1BUID)
                    )
                {

                    sqlParameter1.Append(" AND BUParent = @BUParent ");
                    base.Parameter.Add(new CommandParameter("@BUParent", _obj.BUParent));
                    sqlParameter3.Append(@"SET @BTCtreeName = ''
                                         SET @BTCtreeNumBer='' ");
                }
                else
                {
                    sqlParameter1.Append(" AND BUID = @BUID ");
                    base.Parameter.Add(new CommandParameter("@BUID", _obj.BUID));

                    string substr = "";
                    if (!string.IsNullOrEmpty(_obj.Level1BUID) && _obj.Level1BUID != _obj.BUID.ToString())
                    {
                        substr += ("or BUID = @Level1BUID ");
                        base.Parameter.Add(new CommandParameter("@Level1BUID", _obj.Level1BUID));
                    }
                    if (!string.IsNullOrEmpty(_obj.Level2BUID) && _obj.Level2BUID != _obj.BUID.ToString())
                    {
                        substr += ("or BUID = @Level2BUID ");
                        base.Parameter.Add(new CommandParameter("@Level2BUID", _obj.Level2BUID));
                    }

                    if (!string.IsNullOrEmpty(_obj.Level3BUID) && _obj.Level3BUID != _obj.BUID.ToString())
                    {
                        substr += ("or BUID = @Level3BUID ");
                        base.Parameter.Add(new CommandParameter("@Level3BUID", _obj.Level3BUID));
                    }

                    if (!string.IsNullOrEmpty(_obj.Level4BUID) && _obj.Level4BUID != _obj.BUID.ToString())
                    {
                        substr += ("or BUID = @Level4BUID ");
                        base.Parameter.Add(new CommandParameter("@Level4BUID", _obj.Level4BUID));
                    }


                    if (substr.Length > 0)
                    {
                        substr = " where (" + substr.Substring(2) + ")";
                        //sqlParameter1.Append(" where (" + substr + ")");
                        sqlParameter3.Append(@"SET @BTCtreeName = ( SELECT STUFF((SELECT BUName + '->' FROM CSFSBU ");
                        sqlParameter3.Append(substr);
                        sqlParameter3.Append(@"FOR XML PATH(''),type).value('.','NVARCHAR(max)'),1,1,'') AS [BUName] ) ");

                        sqlParameter3.Append(@"SET @BTCtreeNumBer = (SELECT STUFF( (SELECT BUNumber + '->'  FROM CSFSBU ");
                        sqlParameter3.Append(substr);
                        sqlParameter3.Append(@" FOR XML PATH(''),type).value('.','NVARCHAR(max)'),1,1,'') AS [BUName] )  ");

                    }
                    else
                    {
                        sqlParameter3.Append(@"SET @BTCtreeName = ''
                                         SET @BTCtreeNumBer='' ");
                    }

                }

                //if (!string.IsNullOrEmpty(_obj.Level4BUID))
                //{
                //    sqlParameter1.Append(" AND BUID = @BUID ");
                //    base.Parameter.Add(new CommandParameter("@BUID", _obj.Level4BUID));
                //}

                if (!string.IsNullOrEmpty(_obj.EmpID))
                {
                    sqlParameter2.Append(" AND BuTree_CTE2.EmpID = @EmpID ");
                    base.Parameter.Add(new CommandParameter("@EmpID", _obj.EmpID));
                }
                #endregion



                StringBuilder sql = new StringBuilder("");
                StringBuilder sql_count = new StringBuilder("");

                #region sql
                sql = new StringBuilder("");
                sql.Append(sqlParameter3);
                sql.Append(";");
                sql.Append(@"with BuTree_CTE (BUID,BuMasterID,BUNumber,BUName,BUParent,Node,Sort,Enable,CteLevel,CteSort,CteSortName)
			                    as 
			                    (
					                    Select 
						                    BUID,BuMasterID,BUNumber,BUName,BUParent,Node,Sort,Enable ,1 
						                    ,CONVERT(varchar(500), '-' + CAST(Sort+100000 as varchar(10))+ BUNumber)
                                            ,CONVERT(varchar(500), BUName )
						                    from CSFSBU 
						                    where  
						                    BUMasterID=@BUMasterID and Enable='1'
						                     ");
                sql.Append(sqlParameter1);
                sql.Append(@"           union all
					                    select 
						                    b.BUID,b.BuMasterID,b.BUNumber,b.BUName,b.BUParent,b.Node,b.Sort,b.Enable ,CteLevel+1 
						                    ,CONVERT(varchar(500),ctesort + '-' + CAST(b.Sort+100000 as varchar(10))+b.BUNumber)
                                            ,CONVERT(varchar(500),CteSortName + '->' + b.BUName )
						                    from CSFSBU b 
						                    join BuTree_CTE on b.BUParent=BuTree_CTE.BUID
			                    )
			                    , BuTree_CTE2 as 
			                    (select a1.* ,a2.BUMasterName,a2.BUMasterDesc,a3.EmpID ,a4.EmpName,a3.UID
				                    from BuTree_CTE a1 
				                    left outer join CSFSBUMaster a2 on a1.BuMasterID =a2.BUMasterID  
				                    left outer join CSFSBUToEmployee a3 on a1.BUID=a3.BUID
				                    left outer join CSFSEmployee a4 on a3.EmpID=a4.EmpID 
				                    where a3.EmpID is not null
			                    )
                                select * from 
                                (
			                        select 
			                        ROW_NUMBER() OVER
                                                      ( 
                                                      order by EmpID
                                                      ) AS RowNum
			                        ,BUMasterName +'|' + REPLICATE('-',CteLevel*2)+'>' + BUName as BuNameList
                                    ,BUMasterName + '->' +isnull(@BTCtreeName,'') + CteSortName  CteSortName
			                        ,BUID,BuMasterID,BUNumber,BUName,BUParent,isnull(Node,'0') Node,Sort,isnull(Enable,'0') Enable
                                    ,CteLevel,CteSort
			                        ,EmpID,EmpName ,UID
			                        from BuTree_CTE2 Where EmpID is not null ");
                sql.Append(sqlParameter2);
                sql.Append(@"    ) PAGE ");

                // 判斷是否分頁
                sql.Append(@" WHERE 
                                   RowNum > " + base.PageSize * (base.PageIndex - 1)
                                   + " AND RowNum < " + ((base.PageSize * base.PageIndex) + 1));
                #endregion



                base.Parameter.Add(new CommandParameter("@BUMasterID", _obj.BUMasterID));
                //base.Parameter.Add(new CommandParameter("@BUParent", _obj.BUParent));

                #region 計算筆數
                if (_obj.TotalItemCount.Equals(0) || _obj == null)
                {
                    sql_count = new StringBuilder(@"with BuTree_CTE (BUID,BuMasterID,BUNumber,BUName,BUParent,Node,Sort,Enable,CteLevel,CteSort,CteSortName)
			                    as 
			                    (
					                    Select 
						                    BUID,BuMasterID,BUNumber,BUName,BUParent,Node,Sort,Enable ,1 
						                    ,CONVERT(varchar(500), '-' + CAST(Sort+100000 as varchar(10))+ BUNumber)
                                            ,CONVERT(varchar(500), BUName )
						                    from CSFSBU 
						                    where  
						                    BUMasterID=@BUMasterID  and Enable='1'
						                    
                                            ");
                    sql_count.Append(sqlParameter1);
                    sql_count.Append(@"           union all
					                    select 
						                    b.BUID,b.BuMasterID,b.BUNumber,b.BUName,b.BUParent,b.Node,b.Sort,b.Enable ,CteLevel+1 
						                    ,CONVERT(varchar(500),ctesort + '-' + CAST(b.Sort+100000 as varchar(10))+b.BUNumber)
                                            ,CONVERT(varchar(500),CteSortName + '->' + b.BUName )
						                    from CSFSBU b 
						                    join BuTree_CTE on b.BUParent=BuTree_CTE.BUID
			                    )
			                    , BuTree_CTE2 as 
			                    (select a1.* ,a2.BUMasterName,a2.BUMasterDesc,a3.EmpID ,a4.EmpName
				                    from BuTree_CTE a1 
				                    left outer join CSFSBUMaster a2 on a1.BuMasterID =a2.BUMasterID  
				                    left outer join CSFSBUToEmployee a3 on a1.BUID=a3.BUID
				                    left outer join CSFSEmployee a4 on a3.EmpID=a4.EmpID 
				                    where a3.EmpID is not null
			                    )
                                select count(0) from 
                                (
			                        select 
			                        ROW_NUMBER() OVER
                                                      ( 
                                                      ORDER BY CteSort
                                                      ) AS RowNum
			                        ,BUMasterName +'|' + REPLICATE('-',CteLevel*2)+'>' + BUName as BuNameList,CteSortName
			                        ,BUID,BuMasterID,BUNumber,BUName,BUParent,Node,Sort,Enable,CteLevel,CteSort
			                        ,EmpID,EmpName 
			                        from BuTree_CTE2 Where EmpID is not null ");
                    sql_count.Append(sqlParameter2);
                    sql_count.Append(@"    ) PAGE ");

                    //base.DataRecords = int.Parse(base.ExecuteScalar(sql_count).ToString());
                    base.DataRecords = int.Parse(base.ExecuteScalar(sql_count.ToString()).ToString());

                }
                else
                {
                    base.DataRecords = _obj.TotalItemCount;
                }



                #endregion


                return base.SearchList<CSFSBUToEmployee>(sql.ToString());
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_obj"></param>
        /// <param name="pageIndex"></param>
        /// <returns></returns>
        /// <remarks>add by mel 20131022  IR-61602</remarks>
        public IList<CSFSBUToEmployee> GetBuToEmployeeUnionByRole(CSFSBUToEmployee _obj, int pageIndex)
        {
            try
            {
                base.PageIndex = pageIndex;
                base.Parameter.Clear();

                if (!string.IsNullOrEmpty(_obj.Level4BUID))
                    _obj.BUID = Convert.ToInt16(_obj.Level4BUID);
                else if (!string.IsNullOrEmpty(_obj.Level3BUID))
                    _obj.BUID = Convert.ToInt16(_obj.Level3BUID);
                else if (!string.IsNullOrEmpty(_obj.Level2BUID))
                    _obj.BUID = Convert.ToInt16(_obj.Level2BUID);
                else if (!string.IsNullOrEmpty(_obj.Level1BUID))
                    _obj.BUID = Convert.ToInt16(_obj.Level1BUID);
                else
                    _obj.BUParent = 0;



                StringBuilder sqlParameter1 = new StringBuilder("");
                StringBuilder sqlParameter2 = new StringBuilder("");
                StringBuilder sqlParameter3 = new StringBuilder("");

                sqlParameter3.Append(@"DECLARE @BTCtreeName VARCHAR(MAX)
                                       DECLARE @BTCtreeNumBer VARCHAR(MAX)");

                #region 查詢條件

                if (!string.IsNullOrEmpty(_obj.BUMasterID))
                {
                    sqlParameter1.Append(" AND BUMasterID = @BUMasterID ");
                    base.Parameter.Add(new CommandParameter("@BUMasterID", _obj.BUMasterID));
                }

                if (string.IsNullOrEmpty(_obj.Level4BUID) &&
                    string.IsNullOrEmpty(_obj.Level3BUID) &&
                    string.IsNullOrEmpty(_obj.Level2BUID) &&
                    string.IsNullOrEmpty(_obj.Level1BUID)
                    )
                {

                    sqlParameter1.Append(" AND BUParent = @BUParent ");
                    base.Parameter.Add(new CommandParameter("@BUParent", _obj.BUParent));
                    sqlParameter3.Append(@"SET @BTCtreeName = ''
                                         SET @BTCtreeNumBer='' ");
                }
                else
                {
                    sqlParameter1.Append(" AND BUID = @BUID ");
                    base.Parameter.Add(new CommandParameter("@BUID", _obj.BUID));

                    string substr = "";
                    if (!string.IsNullOrEmpty(_obj.Level1BUID) && _obj.Level1BUID != _obj.BUID.ToString())
                    {
                        substr += ("or BUID = @Level1BUID ");
                        base.Parameter.Add(new CommandParameter("@Level1BUID", _obj.Level1BUID));
                    }
                    if (!string.IsNullOrEmpty(_obj.Level2BUID) && _obj.Level2BUID != _obj.BUID.ToString())
                    {
                        substr += ("or BUID = @Level2BUID ");
                        base.Parameter.Add(new CommandParameter("@Level2BUID", _obj.Level2BUID));
                    }

                    if (!string.IsNullOrEmpty(_obj.Level3BUID) && _obj.Level3BUID != _obj.BUID.ToString())
                    {
                        substr += ("or BUID = @Level3BUID ");
                        base.Parameter.Add(new CommandParameter("@Level3BUID", _obj.Level3BUID));
                    }

                    if (!string.IsNullOrEmpty(_obj.Level4BUID) && _obj.Level4BUID != _obj.BUID.ToString())
                    {
                        substr += ("or BUID = @Level4BUID ");
                        base.Parameter.Add(new CommandParameter("@Level4BUID", _obj.Level4BUID));
                    }


                    if (substr.Length > 0)
                    {
                        substr = " where (" + substr.Substring(2) + ")";
                        //sqlParameter1.Append(" where (" + substr + ")");
                        sqlParameter3.Append(@"SET @BTCtreeName = ( SELECT STUFF((SELECT BUName + '->' FROM CSFSBU ");
                        sqlParameter3.Append(substr);
                        sqlParameter3.Append(@"FOR XML PATH(''),type).value('.','NVARCHAR(max)'),1,1,'') AS [BUName] ) ");

                        sqlParameter3.Append(@"SET @BTCtreeNumBer = (SELECT STUFF( (SELECT BUNumber + '->'  FROM CSFSBU ");
                        sqlParameter3.Append(substr);
                        sqlParameter3.Append(@" FOR XML PATH(''),type).value('.','NVARCHAR(max)'),1,1,'') AS [BUName] )  ");

                    }
                    else
                    {
                        sqlParameter3.Append(@"SET @BTCtreeName = ''
                                         SET @BTCtreeNumBer='' ");
                    }

                }




                if (!string.IsNullOrEmpty(_obj.EmpID))
                {
                    sqlParameter2.Append(" AND BuTree_CTE2.EmpID = @EmpID ");
                    base.Parameter.Add(new CommandParameter("@EmpID", _obj.EmpID));
                }

                if (!string.IsNullOrEmpty(_obj.EmpName))
                {
                    sqlParameter2.Append(" AND BuTree_CTE2.EmpName = @EmpName ");
                    base.Parameter.Add(new CommandParameter("@EmpName", _obj.EmpName));
                }

                if (!string.IsNullOrEmpty(_obj.RoleID))
                {
                    sqlParameter2.Append(" AND BuTree_CTE2.RoleID = @RoleID ");
                    base.Parameter.Add(new CommandParameter("@RoleID", _obj.RoleID));
                }

                #endregion



                StringBuilder sql = new StringBuilder("");
                StringBuilder sql_count = new StringBuilder("");

                #region sql
                sql = new StringBuilder("");
                sql.Append(sqlParameter3);
                sql.Append(";");
                sql.Append(@"with BuTree_CTE (BUID,BuMasterID,BUNumber,BUName,BUParent,Node,Sort,Enable,CteLevel,CteSort,CteSortName,BUBoss)
			                    as 
			                    (
					                    Select 
						                    BUID,BuMasterID,BUNumber,BUName,BUParent,Node,Sort,Enable ,1 
						                    ,CONVERT(varchar(500), '-' + CAST(Sort+100000 as varchar(10))+ BUNumber)
                                            ,CONVERT(varchar(500), BUName )
                                            ,BUBoss
						                    from CSFSBU 
						                    where  
						                     Enable='1'
						                     ");
                sql.Append(sqlParameter1);
                sql.Append(@"           union all
					                    select 
						                    b.BUID,b.BuMasterID,b.BUNumber,b.BUName,b.BUParent,b.Node,b.Sort,b.Enable ,CteLevel+1 
						                    ,CONVERT(varchar(500),ctesort + '-' + CAST(b.Sort+100000 as varchar(10))+b.BUNumber)
                                            ,CONVERT(varchar(500),CteSortName + '->' + b.BUName )
                                            ,b.BUBoss
						                    from CSFSBU b 
						                    join BuTree_CTE on b.BUParent=BuTree_CTE.BUID
			                    )
			                    , BuTree_CTE2 as 
			                    (select a1.* ,a2.BUMasterName,a2.BUMasterDesc,a3.EmpID ,a4.EmpName
                                    ,a5.RoleID,a6.RoleName,a7.EmpName BUBossName
				                    from BuTree_CTE a1 
				                    left outer join CSFSBUMaster a2 on a1.BuMasterID =a2.BUMasterID  
				                    left outer join CSFSBUToEmployee a3 on a1.BUID=a3.BUID
				                    left outer join CSFSEmployee a4 on a3.EmpID=a4.EmpID 
				                    left outer join CSFSEmployeeToRole a5 on a3.EmpID=a5.EmpID 
				                    left outer join CSFSRole a6 on a5.RoleID=a6.RoleID
				                    left outer join CSFSEmployee a7 on a1.BUBoss=a7.EmpID 
				                    where a3.EmpID is not null
			                    )
                                select * from 
                                (
			                        select 
			                        ROW_NUMBER() OVER
                                                      ( 
                                                       order by EmpID
                                                      ) AS RowNum
			                        ,BUMasterName +'|' + REPLICATE('-',CteLevel*2)+'>' + BUName as BuNameList
                                    ,BUMasterName + '->' +isnull(@BTCtreeName,'') + CteSortName  CteSortName
			                        ,BUID,BuMasterID,BUNumber,BUName,BUParent,isnull(Node,'0') Node,Sort,isnull(Enable,'0') Enable
                                    ,CteLevel,CteSort
			                        ,EmpID,EmpName ,RoleID,RoleName,BUBoss,BUBossName
			                        from BuTree_CTE2 Where EmpID is not null ");
                sql.Append(sqlParameter2);

                sql.Append(@"    ) PAGE ");

                // 判斷是否分頁
                sql.Append(@" WHERE 
                                   RowNum > " + base.PageSize * (base.PageIndex - 1)
                                   + " AND RowNum < " + ((base.PageSize * base.PageIndex) + 1));
                //加入排序
                //sql.Append(@" order by EmpID");

                #endregion



                //base.Parameter.Add(new CommandParameter("@BUMasterID", _obj.BUMasterID));
                //base.Parameter.Add(new CommandParameter("@BUParent", _obj.BUParent));

                #region 計算筆數
                if (_obj.TotalItemCount.Equals(0) || _obj == null)
                {
                    sql_count = new StringBuilder(@"with BuTree_CTE (BUID,BuMasterID,BUNumber,BUName,BUParent,Node,Sort,Enable,CteLevel,CteSort,CteSortName,BUBoss)
			                    as 
			                    (
					                    Select 
						                    BUID,BuMasterID,BUNumber,BUName,BUParent,Node,Sort,Enable ,1 
						                    ,CONVERT(varchar(500), '-' + CAST(Sort+100000 as varchar(10))+ BUNumber)
                                            ,CONVERT(varchar(500), BUName )
                                            ,BUBoss
						                    from CSFSBU 
						                    where  
						                     Enable='1'
						                    
                                            ");
                    sql_count.Append(sqlParameter1);
                    sql_count.Append(@"           union all
					                    select 
						                    b.BUID,b.BuMasterID,b.BUNumber,b.BUName,b.BUParent,b.Node,b.Sort,b.Enable ,CteLevel+1 
						                    ,CONVERT(varchar(500),ctesort + '-' + CAST(b.Sort+100000 as varchar(10))+b.BUNumber)
                                            ,CONVERT(varchar(500),CteSortName + '->' + b.BUName )
                                            ,b.BUBoss
						                    from CSFSBU b 
						                    join BuTree_CTE on b.BUParent=BuTree_CTE.BUID
			                    )
			                    , BuTree_CTE2 as 
			                    (select a1.* ,a2.BUMasterName,a2.BUMasterDesc,a3.EmpID ,a4.EmpName
                                    ,a5.RoleID,a6.RoleName,a7.EmpName BUBossName
				                    from BuTree_CTE a1 
				                    left outer join CSFSBUMaster a2 on a1.BuMasterID =a2.BUMasterID  
				                    left outer join CSFSBUToEmployee a3 on a1.BUID=a3.BUID
				                    left outer join CSFSEmployee a4 on a3.EmpID=a4.EmpID 
				                    left outer join CSFSEmployeeToRole a5 on a3.EmpID=a5.EmpID 
				                    left outer join CSFSRole a6 on a5.RoleID=a6.RoleID
				                    left outer join CSFSEmployee a7 on a1.BUBoss=a7.EmpID 
				                    where a3.EmpID is not null
			                    )
                                select count(0) from 
                                (
			                        select 
			                        ROW_NUMBER() OVER
                                                      ( 
                                                      ORDER BY CteSort
                                                      ) AS RowNum
			                        ,BUMasterName +'|' + REPLICATE('-',CteLevel*2)+'>' + BUName as BuNameList,BUMasterName + '->' + CteSortName CteSortName
			                        ,BUID,BuMasterID,BUNumber,BUName,BUParent,Node,Sort,Enable,CteLevel,CteSort
			                        ,EmpID,EmpName ,RoleID,RoleName,BUBoss,BUBossName
			                        from BuTree_CTE2 Where EmpID is not null ");
                    sql_count.Append(sqlParameter2);
                    sql_count.Append(@"    ) PAGE ");

                    //base.DataRecords = int.Parse(base.ExecuteScalar(sql_count).ToString());
                    base.DataRecords = int.Parse(base.ExecuteScalar(sql_count.ToString()).ToString());

                }
                else
                {
                    base.DataRecords = _obj.TotalItemCount;
                }



                #endregion


                return base.SearchList<CSFSBUToEmployee>(sql.ToString());
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public IEnumerable<CSFSBUToEmployee> SelectAllCSFSBUToEmployee()
        {
            try
            {
                string sql = @"select * from CSFSBUToEmployee";
                base.Parameter.Clear();
                return base.SearchList<CSFSBUToEmployee>(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //某bossID下,所有員工清單
        public IList<CSFSEmployee> GetDepartmentEmployee(string bossID, string buMasterID)
        {
            try
            {
                string sql = @"select EmpID from dbo.GetEmpListByEmpOrBoss(@LogOnUser)";
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@LogOnUser", Account));
                return base.SearchList<CSFSEmployee>(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 產生Bu menutree xml
        /// </summary>
        /// <param name="bumasterid"></param>
        /// <returns></returns>
        /// <remarks>add by mel 20130906 IR-62550 </remarks>
        public string GetBUListToXml(string bumasterid)
        {
            string result = "";
            DataTable dt = new DataTable();
            try
            {

                //使用sql 產生xml
                string sql = @"  select 
                                  link_1.BUName as [@linkName]
                                  ,link_1.BUID as [@BUID]
                                  ,CONVERT(varchar(10),link_1.BUID ) as [@linkClick]
                                  ,'parent' as [@type]
                                  ,BUParent as [@parentId]
                                  ,case when link_1.Node =1  then 'folderIcon' else 'fileIcon' end as  [@parentIcon]
                                  ,case when link_1.Node =0  then 'fileIcon' else '' end as  [@childIcon]
                                  ,link_1.BUNumber as [@BUNumber]
                                  ,link_1.Node as [@Node]
                                  ,link_1.Enable as [@Enable]
                                  ,link_1.APPID as [@APPID]
                                  ,link_1.BUParent as [@BUParent]
                                  ,link_1.Sort as [@Sort]
                                  ,1 as [@BULevel]
                                  ,(
	                                   select 
	                                  link_2.BUName as [@linkName]
	                                  ,link_2.BUID as [@BUID]
	                                  ,CONVERT(varchar(10),link_2.BUID) as [@linkClick]
	                                  ,case when link_2.Node =1  then 'parent' else 'child' end as [@type]
	                                  ,link_2.BUParent as [@parentId]
	                                  ,case when link_2.Node =1  then 'folderIcon' else '' end as  [@parentIcon]
	                                  ,case when link_2.Node =0  then 'fileIcon' else '' end as  [@childIcon]
	                                  ,link_2.BUNumber as [@BUNumber]
	                                  ,link_2.Node as [@Node]
	                                  ,link_2.Enable as [@Enable]
	                                  ,link_2.APPID as [@APPID]
	                                  ,link_2.BUParent as [@BUParent]
	                                  ,link_2.Sort as [@Sort]  
	                                  ,2 as [@BULevel]
	                                  ,(
		                                   select 
		                                  link_3.BUName as [@linkName]
		                                  ,link_3.BUID as [@BUID]
		                                  ,CONVERT(varchar(10),link_3.BUID) as [@linkClick]
		                                  ,case when link_3.Node =1  then 'parent' else 'child' end as [@type]
		                                  ,link_3.BUParent as [@parentId]
		                                  ,case when link_3.Node =1  then 'folderIcon' else '' end as  [@parentIcon]
		                                  ,case when link_3.Node =0  then 'fileIcon' else '' end as  [@childIcon]
		                                  ,link_3.BUNumber as [@BUNumber]
		                                  ,link_3.Node as [@Node]
		                                  ,link_3.Enable as [@Enable]
		                                  ,link_3.APPID as [@APPID]
		                                  ,link_3.BUParent as [@BUParent]
		                                  ,link_3.Sort as [@Sort]  
		                                  ,3 as [@BULevel]
		                                  ,(
				                                   select 
				                                  link_4.BUName as [@linkName]
				                                  ,link_4.BUID as [@BUID]
				                                  ,CONVERT(varchar(10),link_4.BUID) as [@linkClick]
				                                  ,case when link_4.Node =1  then 'parent' else 'child' end as [@type]
				                                  ,link_4.BUParent as [@parentId]
				                                  ,case when link_4.Node =1  then 'folderIcon' else '' end as  [@parentIcon]
				                                  ,case when link_4.Node =0  then 'fileIcon' else '' end as  [@childIcon]
				                                  ,link_4.BUNumber as [@BUNumber]
				                                  ,link_4.Node as [@Node]
				                                  ,link_4.Enable as [@Enable]
				                                  ,link_4.APPID as [@APPID]
				                                  ,link_4.BUParent as [@BUParent]
				                                  ,link_4.Sort as [@Sort]  
				                                  ,99 as [@BULevel] 		--99  mean the last level	  
				                                  from csfsbu link_4
				                                  where link_4.BUParent=link_3.BUID order by sort
				                                  for xml path('link'), type
		                                  ) as [*]		  		  
		                                  from csfsbu link_3
		                                  where link_3.BUParent=link_2.BUID order by sort
		                                  for xml path('link'), type
	                                  ) as [*]
  	  
	                                  from csfsbu link_2
	                                  where link_2.BUParent=link_1.BUID  order by sort
	                                  for xml path('link'), type
                                  ) as [*]
                                  from csfsbu link_1
                                  where link_1.BUMasterID=@BUMasterID and  link_1.BUParent=0   order by sort
                                 for xml path('link') , root('menuTree') ";
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@BUMasterID", bumasterid));
                dt = base.Search(sql);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow _dr in dt.Rows)
                    {
                        result += _dr[0].ToString();
                    }

                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bunumber"></param>
        /// <returns></returns>
        /// <remarks>addi by mel 20130906 IR-62550</remarks>
        public int CheckCSFSBUBUNumber(string bunumber)
        {
            try
            {
                int count = 0;
                string sql = @"select count(0) from CSFSBU where BUNumber=@BUNumber ";
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@BUNumber", bunumber));
                count = (int)base.ExecuteScalar(sql);
                return count;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_obj"></param>
        /// <returns></returns>
        /// <remarks>add by mel 20130906 IR-62550 </remarks>
        public bool UpdateCSFSBU(CSFSBU _obj)
        {
            bool result = false;

            IDbConnection dbConnection = base.OpenConnection();
            IDbTransaction dbTransaction = null;
            using (dbConnection)
            {
                dbTransaction = dbConnection.BeginTransaction();
                try
                {
                    //更新CSFSBU
                    #region
                    string sqlStr = @"UPDATE 
                                            CSFSBU
                                        SET 
                                            BUNumber = @BUNumber,
                                            BUName = @BUName,
                                            Node = @Node,
                                            Sort=@Sort,
                                            Enable = @Enable,
                                            BUBoss = @BUBoss,
                                            PromotionUnit = @PromotionUnit,
                                            ModifiedUser = @ModifiedUser,
                                            ModifiedDate = GETDATE() 
                                        WHERE 
                                            BUID = @BUID";


                    base.Parameter.Clear();

                    base.Parameter.Add(new CommandParameter("@BUID", _obj.BUID));
                    base.Parameter.Add(new CommandParameter("@BUNumber", _obj.BUNumber));
                    base.Parameter.Add(new CommandParameter("@BUName", _obj.BUName));
                    base.Parameter.Add(new CommandParameter("@Node", (_obj.Node) ? "1" : "0"));//false=0=enable;true=1=disable
                    base.Parameter.Add(new CommandParameter("@Enable", (_obj.Enable) ? "1" : "0"));//false=0=enable;true=1=disable
                    base.Parameter.Add(new CommandParameter("@Sort", _obj.Sort));
                    base.Parameter.Add(new CommandParameter("@BUBoss", _obj.BUBoss));
                    base.Parameter.Add(new CommandParameter("@PromotionUnit", _obj.PromotionUnit));
                    base.Parameter.Add(new CommandParameter("@ModifiedUser", Account));

                    base.ExecuteNonQuery(sqlStr, dbTransaction);

                    #endregion


                    //若Enable 為0,則要同步更新其下層也Enable 也為0
                    #region
                    if (!_obj.Enable)
                    {
                        sqlStr = @"with BuTree_CTE (BUID,BuMasterID,BUNumber,BUName,BUParent,Node,Sort,Enable,ctelevel,ctesort)
                                    as 
                                    (
                                    Select 
                                        BUID,BuMasterID,BUNumber,BUName,BUParent,Node,Sort,Enable ,1 ,CONVERT(varchar(255),BUNumber)
                                        from CSFSBU where BUMasterID=@BUMasterID and BUParent=@BUParent and BUID=@BUID
                                    union all
                                    select 
                                        b.BUID,b.BuMasterID,b.BUNumber,b.BUName,b.BUParent,b.Node,b.Sort,b.Enable ,ctelevel+1 ,CONVERT(varchar(255),ctesort + '-' + b.BUNumber)
                                        from CSFSBU b 
                                        join BuTree_CTE on b.BUParent=BuTree_CTE.BUID
                                    )
                                    update CSFSBU set Enable =0 where BUID in 
                                    (select buid  from BuTree_CTE  ) ";

                        base.Parameter.Clear();
                        base.Parameter.Add(new CommandParameter("@BUID", _obj.BUID));
                        base.Parameter.Add(new CommandParameter("@BUParent", _obj.BUParent));
                        base.Parameter.Add(new CommandParameter("@BUMasterID", _obj.BUMasterID));
                        base.ExecuteNonQuery(sqlStr, dbTransaction);
                    }

                    #endregion

                    dbTransaction.Commit();
                    result = true;

                }
                catch (Exception ex)
                {
                    dbTransaction.Rollback();
                    // 拋出異常
                    throw ex;
                }
            }

            return result;
            //
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_obj"></param>
        /// <returns></returns>
        /// <remarks>add by mel 20130911 Bu維護介面</remarks>
        public bool CreateCSFSBU(CSFSBU _obj)
        {
            bool result = false;

            IDbConnection dbConnection = base.OpenConnection();
            IDbTransaction dbTransaction = null;
            using (dbConnection)
            {
                dbTransaction = dbConnection.BeginTransaction();
                try
                {
                    //更新CSFSBU
                    //Edit by mel 20140107 增加buboss , PromotionUnit
                    #region
                    string sqlStr = @"INSERT INTO 
                                    CSFSBU
                                    (
                                        APPID,
                                        BUMasterID,
                                        BUNumber,
                                        BUName,
                                        BUParent,
                                        Node,
                                        Sort,
                                        Enable,
                                        BUBoss,
                                        PromotionUnit,
                                        CreatedUser,
                                        CreatedDate,
                                        ModifiedUser,
                                        ModifiedDate
                                    ) 
                                    VALUES
                                    (
                                        @APPID,
                                        @BUMasterID,
                                        @BUNumber,
                                        @BUName,
                                        @BUParent,
                                        @Node,
                                        (select isnull(MAX(sort),0) from CSFSBU where BUParent=@BUParent ) + 1,
                                        @Enable,
                                        @BUBoss,
                                        @PromotionUnit,
                                        @CreatedUser,
                                        GETDATE(),
                                        @ModifiedUser,
                                        GETDATE()
                                    )";



                    base.Parameter.Clear();

                    base.Parameter.Add(new CommandParameter("@APPID", "CSFS"));
                    base.Parameter.Add(new CommandParameter("@BUMasterID", _obj.BUMasterID));
                    base.Parameter.Add(new CommandParameter("@BUNumber", _obj.BUNumber));
                    base.Parameter.Add(new CommandParameter("@BUName", _obj.BUName));
                    base.Parameter.Add(new CommandParameter("@BUParent", _obj.BUParent));
                    base.Parameter.Add(new CommandParameter("@Node", (_obj.Node) ? "1" : "0"));//false=0=enable;true=1=disable
                    base.Parameter.Add(new CommandParameter("@Sort", _obj.Sort));
                    base.Parameter.Add(new CommandParameter("@Enable", (_obj.Enable) ? "1" : "0"));//false=0=enable;true=1=disable
                    base.Parameter.Add(new CommandParameter("@BUBoss", _obj.BUBoss));
                    base.Parameter.Add(new CommandParameter("@PromotionUnit", _obj.PromotionUnit));
                    base.Parameter.Add(new CommandParameter("@CreatedUser", Account));
                    base.Parameter.Add(new CommandParameter("@ModifiedUser", Account));

                    base.ExecuteNonQuery(sqlStr, dbTransaction);
                    #endregion

                    dbTransaction.Commit();
                    result = true;

                }
                catch (Exception ex)
                {
                    dbTransaction.Rollback();
                    // 拋出異常
                    throw ex;
                }
            }

            return result;
            //
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_obj"></param>
        /// <returns></returns>
        /// <remarks>add by mel 20130911 Bu維護介面</remarks>
        public int CheckCSFSBUToEmployee(CSFSBUToEmployee _obj)
        {

            try
            {
                int count = 0;
                string sql = @"select count(0) from CSFSBUToEmployee where BUID=@BUID and EmpID=@EmpID and BUMasterID=@BUMasterID ";
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@BUID", _obj.BUID));
                base.Parameter.Add(new CommandParameter("@EmpID", _obj.EmpID));
                base.Parameter.Add(new CommandParameter("@BUMasterID", _obj.BUMasterID));
                count = (int)base.ExecuteScalar(sql);
                return count;
            }
            catch (Exception ex)
            {
                throw ex;
            }



        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_obj"></param>
        /// <returns></returns>
        /// <remarks>add by mel 20130911 Bu維護介面</remarks>
        public bool UpdateCSFSBUToEmployee(CSFSBUToEmployee _obj)
        {
            bool result = false;

            IDbConnection dbConnection = base.OpenConnection();
            IDbTransaction dbTransaction = null;
            using (dbConnection)
            {
                dbTransaction = dbConnection.BeginTransaction();
                try
                {
                    //更新CSFSBU
                    #region
                    string sqlStr = @"UPDATE 
                                            CSFSBUToEmployee
                                        SET 
                                            BUID = @BUID,
                                            EmpID = @EmpID,
                                            BUMasterID = @BUMasterID,
                                            ModifiedUser = @ModifiedUser,
                                            ModifiedDate = GETDATE() 
                                        WHERE 
                                            UID = @UID";


                    base.Parameter.Clear();

                    base.Parameter.Add(new CommandParameter("@BUID", _obj.BUID));
                    base.Parameter.Add(new CommandParameter("@EmpID", _obj.EmpID));
                    base.Parameter.Add(new CommandParameter("@BUMasterID", _obj.BUMasterID));
                    base.Parameter.Add(new CommandParameter("@ModifiedUser", Account));
                    base.Parameter.Add(new CommandParameter("@UID", _obj.UID));

                    base.ExecuteNonQuery(sqlStr, dbTransaction);

                    #endregion


                    dbTransaction.Commit();
                    result = true;

                }
                catch (Exception ex)
                {
                    dbTransaction.Rollback();
                    // 拋出異常
                    throw ex;
                }
            }

            return result;
            //

        }

        public bool CreateCSFSBUToEmployee(CSFSBUToEmployee _obj)
        {
            bool result = false;

            IDbConnection dbConnection = base.OpenConnection();
            IDbTransaction dbTransaction = null;
            using (dbConnection)
            {
                dbTransaction = dbConnection.BeginTransaction();
                try
                {
                    //更新CSFSBU
                    #region
                    string sqlStr = @"INSERT INTO 
                                    CSFSBUToEmployee
                                    (
                                        BUID,
                                        EmpID,
                                        BUMasterID,
                                        CreatedUser,
                                        CreatedDate,
                                        ModifiedUser,
                                        ModifiedDate
                                    ) 
                                    VALUES
                                    (
                                        @BUID,
                                        @EmpID,
                                        @BUMasterID,
                                        @CreatedUser,
                                        GETDATE(),
                                        @ModifiedUser,
                                        GETDATE()
                                    )";



                    base.Parameter.Clear();

                    base.Parameter.Add(new CommandParameter("@BUID", _obj.BUID));
                    base.Parameter.Add(new CommandParameter("@EmpID", _obj.EmpID));
                    base.Parameter.Add(new CommandParameter("@BUMasterID", _obj.BUMasterID));
                    base.Parameter.Add(new CommandParameter("@CreatedUser", Account));
                    base.Parameter.Add(new CommandParameter("@ModifiedUser", Account));

                    base.ExecuteNonQuery(sqlStr, dbTransaction);
                    #endregion

                    dbTransaction.Commit();
                    result = true;

                }
                catch (Exception ex)
                {
                    dbTransaction.Rollback();
                    // 拋出異常
                    throw ex;
                }
            }

            return result;

        }

    }
}