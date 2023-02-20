/// <summary>
/// 程式說明：維護系統參數
/// 維護部門:資訊管理處
/// 中國信託銀行 版權所有  ©  All Rights Reserved.
/// </summary>

using System;
using System.Web;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CTBC.CSFS.Pattern;
using Microsoft.Practices.EnterpriseLibrary.Logging;
using System.Data;
using CTBC.CSFS.Models;


namespace CTBC.CSFS.BussinessLogic
{
    public class ParameterSettingBIZ : CommonBIZ
    {
        public ParameterSettingBIZ(AppController appController)
            : base(appController)
        { }

        public ParameterSettingBIZ()
        { }

        public IList<ParameterSetting> GetQueryList(ParameterSetting parasetting, int pageIndex, string strSortExpression, string strSortDirection)
        {
            try
            {
                base.PageIndex = pageIndex;
                string sqlStr = "";
                string sqlWhere = "";
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@pageS", (base.PageSize * (base.PageIndex - 1)) + 1));
                base.Parameter.Add(new CommandParameter("@pageE", base.PageSize * base.PageIndex));

                if (!string.IsNullOrEmpty(parasetting.QuickSearchCon))
                {
                    sqlWhere += @" and param_desc like @QuickSearchCon OR param_id like @QuickSearchCon";
                    base.Parameter.Add(new CommandParameter("@QuickSearchCon", "%" + parasetting.QuickSearchCon.Trim() + "%"));
                }
                if (!string.IsNullOrEmpty(parasetting.param_type))
                {
                    sqlWhere += @" and param_type like @param_type ";
                    base.Parameter.Add(new CommandParameter("@param_type", "%" + parasetting.param_type.Trim() + "%"));
                }
                sqlStr += @";with T1 
	                        as
	                        (
		                       select sno,param_id,param_desc,param_type as param_typeid,(select prop_desc from csParaDetl where param_id='Type' and prop_id=csParaGrp.param_type) as param_type from csParaGrp with(nolock)
			                   where 1=1 " + sqlWhere + @" 
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

                IList<ParameterSetting> _ilsit = base.SearchList<ParameterSetting>(sqlStr);

                if (_ilsit != null)
                {
                    if (_ilsit.Count > 0)
                    {
                        base.DataRecords = _ilsit[0].maxnum;
                    }
                    else
                    {
                        base.DataRecords = 0;
                        _ilsit = new List<ParameterSetting>();
                    }
                    return _ilsit;
                }
                else
                {
                    return new List<ParameterSetting>();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public IList<ParameterSetting> GetDetailDataList(ParameterSetting parasetting, int pageIndex, string strSortExpression, string strSortDirection)
        {
            try
            {
                base.PageIndex = pageIndex;
                string sqlStr = "";
                string sqlWhere = "";
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@pageS", (base.PageSize * (base.PageIndex - 1)) + 1));
                base.Parameter.Add(new CommandParameter("@pageE", base.PageSize * base.PageIndex));

                sqlStr += @";with T1 
	                        as
	                        (
		                       select sno,prop_id,param_id,prop_desc,(select prop_desc from csParaDetl where param_id='CULTURE_NAME' and prop_id='" + parasetting.language + @"') as language,(select prop_id from csParaDetl where param_id='CULTURE_NAME' and prop_id='zh-tw') as detlLanguage,sort,
is_show,parent_param_id,parent_prop_id from csParaDetl where param_id='" + parasetting.param_id + @"'" + sqlWhere + @" 
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
                //base.Parameter.Add(new CommandParameter("@language", parasetting.language));
                //base.Parameter.Add(new CommandParameter("@param_id", parasetting.param_id));

                IList<ParameterSetting> _ilsit = base.SearchList<ParameterSetting>(sqlStr);
                if (_ilsit != null)
                {
                    if (_ilsit.Count > 0)
                    {
                        base.DataRecords = _ilsit[0].maxnum;
                    }
                    else
                    {
                        base.DataRecords = 0;
                        _ilsit = new List<ParameterSetting>();
                    }
                    return _ilsit;
                }
                else
                {
                    return new List<ParameterSetting>();
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public IList<ParameterSetting> GetParaDetlList(string strParamId, string strLanguage)
        {
            try
            {
                string sqlStr = "";
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@ParamId", strParamId));
                base.Parameter.Add(new CommandParameter("@Language", strLanguage));

                sqlStr += @"select t.param_id,t.prop_id,prop_name,prop_desc from csParaDetl t inner join csParaGrp b on t.param_id=b.param_id
where t.param_id=@ParamId and t.[language]=@Language and t.is_show='Y' order by t.sort";
                IList<ParameterSetting> _ilsit = base.SearchList<ParameterSetting>(sqlStr);
                return _ilsit;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public IList<ParameterSetting> GetUnionDownInfo(string param_id, string language = "zh-tw")
        {

            try
            {
                string sqlStr = "";
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@param_id", param_id));
                base.Parameter.Add(new CommandParameter("@language", language));

                sqlStr += @"select prop_desc ,prop_id from csParaDetl where param_id='CardType' and language='zh-tw' and is_show='Y'
                            and prop_id in( select distinct CARDTYPE from FieldTable) order by sort";
                IList<ParameterSetting> _ilsit = base.SearchList<ParameterSetting>(sqlStr);
                return _ilsit;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IList<ParameterSetting> GetParaDetlList_Uni(string strParamId, string strLanguage)
        {
            try
            {
                string sqlStr = "";
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@param_id", strParamId));
                base.Parameter.Add(new CommandParameter("@language", strLanguage));

                sqlStr += @"select prop_desc,prop_id from csParaDetl t where param_id=@param_id  and prop_id not in (
                            select distinct FieldGroup from csfsFieldTable 
                            ) and t.is_show='Y'  and t.language=@language order by t.sort ";
                IList<ParameterSetting> _ilsit = base.SearchList<ParameterSetting>(sqlStr);
                return _ilsit;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        /// <summary>
        /// 資料是否重複
        /// </summary>
        /// <param name="codeNo">參數KEY</param>
        /// <returns></returns>
        public int Count(string codeNo)
        {
            try
            {
                int count = 0;
                string sql = @"select count(0) from csParaGrp where param_id=@CodeNo";
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@CodeNo", codeNo));
                count = (int)base.ExecuteScalar(sql);
                return count;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public int DtCount(string propid, string paramid)
        {
            try
            {
                string sql = @"select count(*) from csParaDetl where prop_id=@prop_id and param_id=@param_id";
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@prop_id", propid));
                base.Parameter.Add(new CommandParameter("@param_id", paramid));
                int count = (int)base.ExecuteScalar(sql);
                return count;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public bool DtCreate(ParameterSetting model)
        {

            string sql = @"insert into 
                                     csParaDetl
                                     (
                                        SNO,         
                                        param_id,
                                        prop_id,
                                        prop_desc,
                                        language,
                                        sort,
                                        is_show,
                                        parent_param_id,
                                        parent_prop_id,
                                        cCretMebrNo,
                                        cCretDT,
                                        cMantMebrNo,
                                        cMantDT
                                      )
                                     values
                                     (
                                        NEXT VALUE FOR SEQcsParaDetl,    
                                        @param_id,
                                        @prop_id,
                                        @prop_desc,
                                        @language,
                                        @sort,
                                        @is_show,
                                        @parent_param_id,
                                        @parent_prop_id,                
                                        @UserId,
                                        GETDATE(),
                                        @UserId,
                                        GETDATE()
                                     )";
            base.Parameter.Clear();
            base.Parameter.Add(new CommandParameter("@param_id", model.param_id));
            base.Parameter.Add(new CommandParameter("@prop_id", model.prop_id));
            base.Parameter.Add(new CommandParameter("@prop_desc", model.prop_desc));
            base.Parameter.Add(new CommandParameter("@language", model.language));
            base.Parameter.Add(new CommandParameter("@sort", model.sort));
            base.Parameter.Add(new CommandParameter("@is_show", model.is_show));
            base.Parameter.Add(new CommandParameter("@parent_param_id", model.parent_param_id));
            base.Parameter.Add(new CommandParameter("@parent_prop_id", model.parent_prop_id));
            base.Parameter.Add(new CommandParameter("@UserId", model.cCretMebrNo));
            try
            {
                return base.ExecuteNonQuery(sql) > 0;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public int DTDelete(string param_id, string prop_id)
        {
            string sql = string.Empty;
            string Bwhere = string.Empty;
            string[] popid = prop_id.Split(',');
            foreach (string Bitem in popid)
            {
                sql += @"delete from csParaDetl where param_id='" + param_id + "' and prop_id='" + Bitem + "';";

            }
            base.Parameter.Clear();
            base.Parameter.Add(new CommandParameter("@param_id", param_id));
            base.Parameter.Add(new CommandParameter("@prop_id", prop_id));
            try
            {
                return base.ExecuteNonQuery(sql);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public bool DTEdit(ParameterSetting model)
        {
            string sql = @"update csParaDetl set prop_id=@prop_id,prop_desc=@prop_desc,language=@language,sort=@sort,is_show=@is_show,parent_param_id=@parent_param_id,parent_prop_id=@parent_prop_id,cMantMebrNo=@UserId,
                                        cMantDT =GETDATE() where param_id=@param_id and sno=@son";
            base.Parameter.Clear();
            base.Parameter.Add(new CommandParameter("@prop_id", model.prop_id));
            base.Parameter.Add(new CommandParameter("@prop_desc", model.prop_desc));
            base.Parameter.Add(new CommandParameter("@language", model.language));
            base.Parameter.Add(new CommandParameter("@sort", model.sort));
            base.Parameter.Add(new CommandParameter("@is_show", model.is_show));
            base.Parameter.Add(new CommandParameter("@parent_param_id", model.parent_param_id));
            base.Parameter.Add(new CommandParameter("@parent_prop_id", model.parent_prop_id));
            base.Parameter.Add(new CommandParameter("@UserId", model.cMantMebrNo));
            base.Parameter.Add(new CommandParameter("@param_id", model.param_id));
            base.Parameter.Add(new CommandParameter("@son", model.SNO));
            try
            {
                return base.ExecuteNonQuery(sql) > 0;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        /// <summary>
        /// 新增csParaGrp資料
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool Create(ParameterSetting model)
        {
            // 插入SQL語句
            string sqlStr = @"insert into 
                                    csParaGrp
                                    (
                                        SNO,
                                        param_id,
                                        param_desc,
                                        param_type,
                                        cCretMebrNo,
                                        cCretDT,
                                        cMantMebrNo,
                                        cMantDT
                                    ) 
                                    values
                                    (
                                        NEXT VALUE FOR SEQcsParaGrp,
                                        @param_id,
                                        @param_desc,
                                        @param_type,
                                        @UserId,
                                        GETDATE(),
                                        @UserId,
                                        GETDATE()
                                    )";

            // 清空參數容器
            base.Parameter.Clear();

            // 添加參數
            base.Parameter.Add(new CommandParameter("@param_id", model.param_id));
            base.Parameter.Add(new CommandParameter("@param_desc", model.param_desc));
            base.Parameter.Add(new CommandParameter("@param_type", model.param_type));
            base.Parameter.Add(new CommandParameter("@UserId", model.cCretMebrNo));
            try
            {
                // 執行新增返回是否成功
                return base.ExecuteNonQuery(sqlStr) > 0;
            }
            catch (Exception ex)
            {
                // 拋出異常
                throw ex;
            }
        }

        /// <summary>
        /// 修改csParaGrp資料
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool Edit(ParameterSetting model)
        {
            // 插入SQL語句
            string sqlStr = @"update csParaGrp set 
                                        param_desc=@param_desc,
                                        param_type=@param_type,
                                        cMantMebrNo=@UserId,
                                        cMantDT =GETDATE() 
                                        where sno=@sno";

            // 清空參數容器
            base.Parameter.Clear();

            // 添加參數
            base.Parameter.Add(new CommandParameter("@sno", model.SNO));
            base.Parameter.Add(new CommandParameter("@param_id", model.param_id));
            base.Parameter.Add(new CommandParameter("@param_desc", model.param_desc));
            base.Parameter.Add(new CommandParameter("@param_type", model.param_type));
            base.Parameter.Add(new CommandParameter("@UserId", model.cMantMebrNo));

            try
            {
                // 執行修改返回是否成功
                return base.ExecuteNonQuery(sqlStr) > 0;
            }
            catch (Exception ex)
            {
                // 拋出異常
                throw ex;
            }
        }

        /// <summary>
        /// 刪除一筆csParaGrp設定
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public int Delete(string id)
        {
            int rtn = 0;
            IDbConnection dbConnection = base.OpenConnection();
            IDbTransaction dbTransaction = null;
            try
            {
                using (dbConnection)
                {
                    dbTransaction = dbConnection.BeginTransaction();
                    string sqlStr = string.Empty;
                    string[] a = id.Split(',');
                    foreach (string item in a)
                    {
                        sqlStr += @"delete csParaGrp where sno=" + item + ";";
                    }
                    base.Parameter.Clear();
                    rtn = base.ExecuteNonQuery(sqlStr, dbTransaction);
                    dbTransaction.Commit();

                    //動態記錄至CSFSLog Sample
                    CSFSLogBIZ.WriteLog(new CSFSLog()
                    {
                        Title = "ParameterSetting",
                        Message = "ParameterSetting sno=" + id + " deleted"
                    });
                }
                return rtn;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}