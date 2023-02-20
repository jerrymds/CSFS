/// <summary>
/// 程式說明:ADO公用方法
/// 維護部門:資訊管理處
/// CSFS Version:v3.0
/// 中國信託銀行 版權所有  ©  All Rights Reserved.
/// </summary>

using System;
using System.Text;
using System.Web;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using Microsoft.Practices.EnterpriseLibrary.Logging;
using System.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;

namespace CTBC.CSFS.Pattern
{
    public class BaseBusinessRule
    {
        // 連接字串
        private string _strConnectionString = Config.ConnectionString;//2014/1/16 horace,移除static

        private AppController _apCtrl;

        #region 屬性

        /// <summary>
        /// 資料清單之當前頁頁碼
        /// </summary>
        public int PageIndex { set; get; }

        /// <summary>
        /// 資料清單之每頁資料數
        /// </summary>
        public int PageSize { get { return Config.GetPerPageRows(); } }

        /// <summary>
        /// 當前條件所檢索出數據量
        /// </summary>
        public int DataRecords { set; get; }

        /// <summary>
        /// 可獨立代入ConnectionString
        /// </summary>
        public string ConnectionString
        {
            set { _strConnectionString = value; }
        }

        /// <summary>
        /// 參數集合
        /// </summary>
        protected CommandParameterCollection Parameter = new CommandParameterCollection();

        #endregion

        #region 方法

        public BaseBusinessRule(string connectstring)
        {
            _strConnectionString = connectstring;
        }

        public BaseBusinessRule()
        {
            _strConnectionString = Config.ConnectionString;
        }

        public BaseBusinessRule(AppController appController)
        {
            _apCtrl = appController;
        }

        /// <summary>
        /// 取得連接并開啟連接
        /// </summary>
        /// <returns></returns>
        /// 2014/1/16 horace 移除static
        protected SqlConnection OpenConnection()
        {
            DBConnection dbCon = new CTBC.CSFS.Pattern.DBConnection();
            return (SqlConnection)dbCon.NewDBConnection(_strConnectionString);
        }

        /// <summary>
        /// 關閉已開啟的連接
        /// </summary>
        protected void CloseConnection(IDbConnection AConnection)
        {
            if (AConnection != null && AConnection.State == ConnectionState.Open)
            {
                AConnection.Close();
            }
        }

        /// <summary>
        /// 開啟連接的事務
        /// </summary>
        /// <param name="AConnection">連接物件</param>
        /// <returns></returns>
        protected IDbTransaction GetTransaction(IDbConnection AConnection)
        {
            return AConnection.BeginTransaction();
        }

        /// <summary>
        /// 執行Sql，返回影響的行數
        /// </summary>
        /// <param name="sSql">SQL</param>
        /// <returns></returns>
        protected int ExecuteNonQuery(string sSql)
        {
            return this.ExecuteNonQuery(sSql, false);
        }

        /// <summary>
        /// 執行Sql，返回影響的行數
        /// </summary>
        /// <param name="sSql">SQL</param>
        /// <returns></returns>
        protected int ExecuteNonQueryByAtt(string Sql)
        {
            int rtn = 0;
            SqlParameter newOP;
            IDbCommand NewCommand = new SqlCommand(Sql);
            foreach (CommandParameter NewParameter in this.Parameter)
            {
                if (Sql.ToUpper().Contains(NewParameter.ColumnName.Replace("@", "").ToUpper()))
                {
                    newOP = new SqlParameter(NewParameter.ColumnName, NewParameter.Value ?? DBNull.Value);
                    NewCommand.Parameters.Add(newOP);
                }
            }

            using (IDbConnection AConnection = this.OpenConnection())
            {
                NewCommand.CommandText = Sql;
                NewCommand.Connection = AConnection;
                rtn = NewCommand.ExecuteNonQuery();
            }
            return rtn;
        }

        /// <summary>
        /// 執行Sql，返回影響的行數
        /// </summary>
        /// <param name="Sql">Sql語句</param>
        /// <param name="bPrepare">是否要預編譯</param>
        /// <returns></returns>
        /// 20140317 add using statement
        protected int ExecuteNonQuery(string Sql, bool bPrepare)
        {
            int rtn = 0;
            SqlParameter newOP;

            using (SqlConnection AConnection = this.OpenConnection())
            {
                using (SqlCommand NewCommand = new SqlCommand(Sql, AConnection))
                {
                    foreach (CommandParameter NewParameter in this.Parameter)
                    {
                        if (Sql.ToUpper().Contains(NewParameter.ColumnName.Replace("@", "").ToUpper()))
                        {
                            newOP = new SqlParameter(NewParameter.ColumnName, NewParameter.Value ?? DBNull.Value);
                            NewCommand.Parameters.Add(newOP);
                        }
                    }
                    NewCommand.CommandText = Sql;
                    rtn = NewCommand.ExecuteNonQuery();
                }
            }
            return rtn;
        }

        /// <summary>
        /// 執行Delete 的Sql，返回影響的行數
        /// </summary>
        /// <param name="Sql"></param>
        /// <param name="TableName"></param>
        /// <returns></returns>
        protected int ExecuteNonQueryDelete(string Sql, string TableName)
        {
            //Save Delete Log

            return ExecuteNonQuery(Sql);
        }

        /// <summary>
        /// 執行Sql，返回影響的行數
        /// </summary>
        /// <param name="Sql">SQL語句</param>
        /// <param name="ATans">事務</param>
        /// <returns></returns>
        /// 20140317 add using statement
        protected int ExecuteNonQuery(string Sql, IDbTransaction ATans)
        {
            #region 舊版程式
            int rtn = 0;
            SqlTransaction sqlTransaction = (SqlTransaction)ATans;
            SqlParameter newOP;
            using (SqlCommand NewCommand = new SqlCommand(Sql, sqlTransaction.Connection, sqlTransaction))
            {
                foreach (CommandParameter NewParameter in this.Parameter)
                {
                    if (Sql.ToUpper().Contains(NewParameter.ColumnName.Replace("@", "").ToUpper()))
                    {
                        newOP = new SqlParameter(NewParameter.ColumnName, NewParameter.Value ?? DBNull.Value);
                        NewCommand.Parameters.Add(newOP);
                    }
                }
                rtn = NewCommand.ExecuteNonQuery();
            }
            return rtn;
            #endregion
        }

        /// <summary>
        /// 執行Delete Sql，返回影響的行數
        /// </summary>
        /// <param name="Sql"></param>
        /// <param name="ATans"></param>
        /// <param name="TableName"></param>
        /// <returns></returns>
        protected int ExecuteNonQueryDelete(string Sql, IDbTransaction ATans, string TableName)
        {
            return ExecuteNonQuery(Sql, ATans);
        }

        #region 舊版程式
        /// <summary>
        /// 執行Sql，返回影響的行數,舊版程式
        /// </summary>
        /// <param name="Sql">SQL語句</param>
        /// <param name="ATans">事務</param>
        /// <param name="LogFlag">是否寫ap log</param>
        /// <returns></returns>
        /// 20140307 add using statement
        protected int ExecuteNonQuery(string Sql, IDbTransaction ATans, bool LogFlag)
        {
            if (LogFlag) return ExecuteNonQuery(Sql, ATans);
            int rtn = 0;
            SqlTransaction sqlTransaction = (SqlTransaction)ATans;
            SqlParameter newOP;
            using (SqlCommand NewCommand = new SqlCommand(Sql, sqlTransaction.Connection, sqlTransaction))
            {
                foreach (CommandParameter NewParameter in this.Parameter)
                {
                    if (Sql.ToUpper().Contains(NewParameter.ColumnName.Replace("@", "").ToUpper()))
                    {
                        newOP = new SqlParameter(NewParameter.ColumnName, NewParameter.Value ?? DBNull.Value);
                        NewCommand.Parameters.Add(newOP);
                    }
                }
                rtn = NewCommand.ExecuteNonQuery();
            }
            return rtn;
        }
        #endregion

        /// <summary>
        /// 查詢結果集
        /// </summary>
        /// <param name="spName">SP名稱</param>
        /// <returns></returns>
        protected int ExecuteNonQuerySP(string spName)
        {
            int rtn = 0;
            SqlParameter newOP;
            using (SqlConnection AConnection = this.OpenConnection())
            {
                using (SqlCommand NewCommand = new SqlCommand())
                {
                    NewCommand.Connection = AConnection;
                    NewCommand.CommandType = CommandType.StoredProcedure;
                    NewCommand.CommandText = spName;

                    foreach (CommandParameter NewParameter in this.Parameter)
                    {
                        newOP = new SqlParameter(NewParameter.ColumnName, NewParameter.Value ?? DBNull.Value);
                        NewCommand.Parameters.Add(newOP);
                    }
                    rtn = NewCommand.ExecuteNonQuery();
                }
            }
            return rtn;
        }

        /// <summary>
        /// 查詢結果集
        /// </summary>
        /// <param name="spName">SP名稱</param>
        /// <returns></returns>
        protected object ExecuteScalarSP(string spName)
        {
            object rtn;
            IDbCommand NewCommand = new SqlCommand();
            using (IDbConnection AConnection = this.OpenConnection())
            {
                NewCommand.Connection = (SqlConnection)AConnection;
                NewCommand.CommandType = CommandType.StoredProcedure;
                NewCommand.CommandText = spName;
                SqlParameter newOP;
                foreach (CommandParameter NewParameter in this.Parameter)
                {
                    newOP = new SqlParameter(NewParameter.ColumnName, NewParameter.Value ?? DBNull.Value);
                    NewCommand.Parameters.Add(newOP);
                }
                rtn = Convert.ToInt16(NewCommand.ExecuteScalar());
            }
            return rtn;
        }


        /// <summary>
        /// 查詢結果集-SP for Transaction,added by smallzhi 20130620
        /// </summary>
        /// <param name="spName">SP名稱</param>
        /// <returns></returns>
        protected int ExecuteNonQuerySP(string spName, IDbTransaction trans)
        {
            int rtn = 0;
            SqlTransaction sqlTransaction = (SqlTransaction)trans;
            using (SqlCommand NewCommand = new SqlCommand(spName, sqlTransaction.Connection, sqlTransaction))
            {
                NewCommand.CommandType = CommandType.StoredProcedure;
                SqlParameter newOP;
                foreach (CommandParameter NewParameter in this.Parameter)
                {
                    newOP = new SqlParameter(NewParameter.ColumnName, NewParameter.Value ?? DBNull.Value);
                    NewCommand.Parameters.Add(newOP);
                }
                rtn = NewCommand.ExecuteNonQuery();
            }
            return rtn;
        }

        /// <summary>
        /// 執行Sql，返回物件
        /// </summary>
        /// <param name="sSql">SQL</param>
        /// <returns></returns>
        protected object ExecuteScalar(string Sql)
        {
            return this.ExecuteScalar(Sql, false);
        }

        /// <summary>
        /// 執行Sql，返回物件
        /// </summary>
        /// <param name="Sql">Sql語句</param>
        /// <param name="bPrepare">是否要預編譯</param>
        /// <returns></returns>
        /// 20140317 add using statement
        protected object ExecuteScalar(string Sql, bool bPrepare)
        {
            SqlParameter newOP;
            using (SqlConnection AConnection = this.OpenConnection())
            {
                using (SqlCommand NewCommand = new SqlCommand(Sql, AConnection))
                {
                    foreach (CommandParameter NewParameter in this.Parameter)
                    {
                        if (Sql.ToUpper().Contains(NewParameter.ColumnName.Replace("@", "").ToUpper()))
                        {
                            newOP = new SqlParameter(NewParameter.ColumnName, NewParameter.Value ?? DBNull.Value);
                            NewCommand.Parameters.Add(newOP);
                        }
                    }
                    return NewCommand.ExecuteScalar();
                }
            }
        }

        /// <summary>
        /// 執行Sql，返回物件
        /// </summary>
        /// <param name="Sql">Sql語句</param>
        /// <param name="bPrepare">是否要預編譯</param>
        /// <returns></returns>
        protected object ExecuteScalar(string Sql, IDbTransaction ATans)
        {
            object result = "";
            SqlTransaction sqlTransaction = (SqlTransaction)ATans;
            using (IDbCommand NewCommand = new SqlCommand(Sql, sqlTransaction.Connection, sqlTransaction))
            {
                SqlParameter newOP;
                foreach (CommandParameter NewParameter in this.Parameter)
                {
                    if (Sql.ToUpper().Contains(NewParameter.ColumnName.Replace("@", "").ToUpper()))
                    {
                        newOP = new SqlParameter(NewParameter.ColumnName, NewParameter.Value ?? DBNull.Value);
                        NewCommand.Parameters.Add(newOP);
                    }
                }
                result = NewCommand.ExecuteScalar();
            }
            return result;
        }

        /// <summary>
        /// 執行Sql，返回Reader物件
        /// </summary>
        /// <param name="Sql">SQL語句</param>
        /// <returns></returns>
        protected IDataReader ExecuteReader(string Sql)
        {
            return this.ExecuteReader(Sql, false);
        }

        /// <summary>
        /// 執行Sql，返回Reader物件
        /// </summary>
        /// <param name="sSql">Sql</param>
        /// <param name="bPrepare">是否要預編譯</param>
        /// <returns></returns>
        protected IDataReader ExecuteReader(string Sql, bool bPrepare)
        {
            IDbConnection AConnection = this.OpenConnection();
            IDbCommand NewCommand = new SqlCommand(Sql, (SqlConnection)AConnection);
            SqlParameter newOP;
            foreach (CommandParameter NewParameter in this.Parameter)
            {
                newOP = new SqlParameter(NewParameter.ColumnName, NewParameter.Value ?? DBNull.Value);
                NewCommand.Parameters.Add(newOP);
            }
            return NewCommand.ExecuteReader(CommandBehavior.CloseConnection);
        }

        /// <summary>
        /// 查詢結果集
        /// </summary>
        /// <param name="Sql">SQL語句</param>
        /// <returns></returns>
        /// 20140317 加using statement
        public DataTable Search(string Sql)
        {
            DataSet dsResult = new DataSet();
            using (SqlConnection AConnection = this.OpenConnection())
            {
                using (SqlCommand NewCommand = new SqlCommand(Sql, AConnection))
                {
                    SqlParameter newOP;
                    NewCommand.CommandTimeout = 900;
                    foreach (CommandParameter NewParameter in this.Parameter)
                    {
                        newOP = new SqlParameter(NewParameter.ColumnName, NewParameter.Value ?? DBNull.Value);
                        NewCommand.Parameters.Add(newOP);
                    }
                    using (SqlDataAdapter Adapter = new SqlDataAdapter(NewCommand))
                    {
                        Adapter.Fill(dsResult);
                    }
                }
            }
            return dsResult.Tables[0];
        }

        /// <summary>
        /// 查詢結果集
        /// </summary>
        /// <param name="Sql">SQL語句</param>
        /// <returns></returns>
        /// 20140317 add using statement
        protected DataSet SearchToDataSet(string Sql)
        {
            SqlParameter newOP;
            DataSet dsResult = new DataSet();
            using (SqlConnection AConnection = this.OpenConnection())
            {
                using (SqlCommand NewCommand = new SqlCommand(Sql, AConnection))
                {
                    foreach (CommandParameter NewParameter in this.Parameter)
                    {
                        newOP = new SqlParameter(NewParameter.ColumnName, NewParameter.Value ?? DBNull.Value);
                        NewCommand.Parameters.Add(newOP);
                    }
                    using (SqlDataAdapter Adapter = new SqlDataAdapter(NewCommand))
                    {
                        Adapter.Fill(dsResult);
                    }
                }
            }
            return dsResult;
        }

        /// <summary>
        /// 查詢結果集
        /// </summary>
        /// <param name="sSql">SQL語句</param>
        /// <param name="tran">tran事務</param>
        /// <returns></returns>
        /// <remarks></remarks>
        /// 20140317 add using statement
        protected DataTable Search(string Sql, IDbTransaction tran)
        {
            SqlParameter newOP;
            DataSet dsResult = new DataSet();
            using (SqlCommand NewCommand = new SqlCommand(Sql, (SqlConnection)tran.Connection))
            {
                NewCommand.Transaction = (SqlTransaction)tran;
                foreach (CommandParameter NewParameter in this.Parameter)
                {
                    newOP = new SqlParameter(NewParameter.ColumnName, NewParameter.Value ?? DBNull.Value);
                    NewCommand.Parameters.Add(newOP);
                }
                using (SqlDataAdapter Adapter = new SqlDataAdapter(NewCommand))
                {
                    Adapter.Fill(dsResult);
                }
            }
            return dsResult.Tables[0];
        }

        /// <summary>
        /// 查詢結果集
        /// </summary>
        /// <param name="spName">SP名稱</param>
        /// <returns></returns>
        /// 20140317 add using statement
        protected DataTable ExecuteSP(string spName)
        {
            DataSet dt = new DataSet();
            SqlParameter newOP;
            using (SqlConnection AConnection = this.OpenConnection())
            {
                using (SqlCommand NewCommand = new SqlCommand())
                {
                    NewCommand.Connection = AConnection;
                    NewCommand.CommandType = CommandType.StoredProcedure;
                    NewCommand.CommandText = spName;
                    foreach (CommandParameter NewParameter in this.Parameter)
                    {
                        newOP = new SqlParameter(NewParameter.ColumnName, NewParameter.Value ?? DBNull.Value);
                        NewCommand.Parameters.Add(newOP);
                    }
                    using (SqlDataAdapter Adapter = new SqlDataAdapter(NewCommand))
                    {
                        Adapter.Fill(dt);
                    }
                }
            }
            return dt.Tables[0];
        }

        /// <summary>
        /// 把DataTable轉換成List
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="dt">數據源</param>
        /// <returns>泛型集合</returns>
        /// 20140317 add using statement
        protected IList<T> DataSetToList<T>(DataTable dt)
        {
            IList<T> list;

            using (dt)
            {
                if (dt == null)
                {
                    return null;
                }
                if (dt.Rows.Count < 0)
                {
                    return null;
                }
                list = new List<T>();
                Dictionary<PropertyInfo, string> pro = new Dictionary<PropertyInfo, string>();

                Array.ForEach<PropertyInfo>(typeof(T).GetProperties(), property =>
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        if (string.Compare(column.ColumnName, property.Name, true) == 0)
                        {
                            pro.Add(property, column.ColumnName);
                        }
                    }
                });
                foreach (DataRow row in dt.Rows)
                {
                    T t = Activator.CreateInstance<T>();
                    foreach (PropertyInfo property in pro.Keys)
                    {
                        object value = row[pro[property]];
                        string fullName = property.PropertyType.ToString();
                        if (fullName.Contains("System.Nullable"))
                        {
                            value = string.IsNullOrEmpty(value.ToString()) ? null : value;
                            if (value == null)
                            {
                                property.SetValue(t, null, null);
                            }
                            else
                            {
                                if (fullName.Contains("System.Int64"))
                                {
                                    property.SetValue(t, Convert.ToInt64(value.ToString()), null);
                                }
                                if (fullName.Contains("System.Int32"))
                                {
                                    property.SetValue(t, Convert.ToInt32(value.ToString()), null);
                                }
                                else if (fullName.Contains("System.String"))
                                {
                                    property.SetValue(t, value.ToString(), null);
                                }
                                else if (fullName.Contains("System.DateTime"))
                                {
                                    property.SetValue(t, Convert.ToDateTime(value.ToString()), null);
                                }
                                else if (fullName.Contains("System.Decimal"))
                                {
                                    property.SetValue(t, Convert.ToDecimal(value.ToString()), null);
                                }
                                else if (fullName.Contains("System.Char"))
                                {
                                    property.SetValue(t, Convert.ToChar(value.ToString()), null);
                                }
                                else if (fullName.Contains("System.Boolean"))
                                {
                                    property.SetValue(t, Convert.ToBoolean(value.ToString()), null);
                                }
                                else if (fullName.Contains("System.Guid"))
                                {
                                    property.SetValue(t, new Guid(value.ToString()), null);
                                }
                                else
                                {
                                    property.SetValue(t, Convert.ToString(value), null);
                                }
                            }
                        }
                        else
                        {
                            switch (fullName)
                            {
                                case "System.Int64":
                                    property.SetValue(t, Convert.ToInt64(string.IsNullOrEmpty(value.ToString()) ? "0" : value), null);
                                    break;
                                case "System.Int32":
                                    property.SetValue(t, Convert.ToInt32(string.IsNullOrEmpty(value.ToString()) ? "0" : value), null);
                                    break;
                                case "System.String":
                                    if ((property.PropertyType).FullName.Contains("DateTime"))
                                    {
                                        property.SetValue(t, Convert.ToDateTime(value.ToString()), null);
                                    }
                                    else
                                    {
                                        property.SetValue(t, value == null ? "" : value.ToString(), null);
                                    }
                                    break;
                                case "System.DateTime":
                                    property.SetValue(t, string.IsNullOrEmpty(value.ToString()) ? DateTime.MinValue : Convert.ToDateTime(value.ToString()), null);
                                    break;
                                case "System.Decimal":
                                    property.SetValue(t, string.IsNullOrEmpty(value.ToString()) ? 0 : Convert.ToDecimal(value.ToString()), null);
                                    break;
                                case "System.Char":
                                    property.SetValue(t, string.IsNullOrEmpty(value.ToString()) ? ' ' : Convert.ToChar(value.ToString()), null);
                                    break;
                                case "System.Boolean":
                                    property.SetValue(t, Convert.ToBoolean(value.ToString()), null);
                                    break;
                                case "System.Guid":
                                    property.SetValue(t, new Guid(value.ToString()), null);
                                    break;
                                case "System.Byte[]":
                                    property.SetValue(t, value, null);
                                    break;
                                default:
                                    property.SetValue(t, value == null ? "" : value.ToString(), null);
                                    break;
                            }
                        }
                    }
                    list.Add(t);
                }
            }
            return list;
        }

        /// <summary>
        /// 查詢結果集  轉換成對應List 
        /// <param name="Sql">SQL語句</param>
        /// <returns>泛型集合</returns>   
        public IList<T> SearchList<T>(string Sql)
        {
            return DataSetToList<T>(Search(Sql));
        }

        /// <summary>
        /// 查詢結果集  轉換成對應List 
        /// <param name="Sql">SQL語句</param>
        /// <param name="tran">事務</param>
        /// <returns>泛型集合</returns>    	 
        public IList<L> SearchList<L>(string Sql, IDbTransaction tran)
        {
            return DataSetToList<L>(Search(Sql, tran));
        }

        /// <summary>
        /// Added by smallzhi
        /// 查詢結果集  轉換成對應List For SP 
        /// <param name="SPName">SP Name</param>
        /// <returns>泛型集合</returns>   
        public IList<T> SearchListSP<T>(string SPNname)
        {
            return DataSetToList<T>(ExecuteSP(SPNname));
        }

        /// <summary>
        /// 反射创建泛型集合
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="reader"></param>
        /// <returns></returns>
        private IList<T> ToList<T>(IDataReader reader)
        {
            Type type = typeof(T);
            IList<T> list = null;
            if (type.IsValueType || type == typeof(string))
            {
                list = CreateValue<T>(reader, type);
            }
            else
            {
                list = CreateObject<T>(reader, type);
            }
            reader.Dispose();
            reader.Close();
            return list;
        }

        private IList<T> CreateObject<T>(IDataReader reader, Type type)
        {
            IList<T> list = new List<T>();
            PropertyInfo[] properties = type.GetProperties();
            string name = string.Empty;

            while (reader.Read())
            {
                T local = Activator.CreateInstance<T>();

                for (int i = 0; i < reader.FieldCount; i++)
                {
                    name = reader.GetName(i);

                    foreach (PropertyInfo info in properties)
                    {
                        if (name.Equals(info.Name))
                        {
                            info.SetValue(local, Convert.ChangeType(reader[info.Name], info.PropertyType), null);
                            break;
                        }
                    }
                }
                list.Add(local);
            }
            return list;
        }

        private IList<T> CreateValue<T>(IDataReader reader, Type type)
        {
            IList<T> list = new List<T>();
            while (reader.Read())
            {
                T local = (T)Convert.ChangeType(reader[0], type, null);
                list.Add(local);
            }
            return list;
        }

        /// <summary>
        /// 20150729 APLOG儲存
        /// </summary>
        public void ExecuteAPLOGSave(string _user, string _controller, string _action, string _ip, string _parameters, string _cusid)
        {
            try
            {
                StringBuilder strsql = new StringBuilder(@"  
                                                                 INSERT INTO APLogRawData
                                                                       (
                                                                        DataTimestamp
                                                                       ,Controller
                                                                       ,[Action]
                                                                       ,[IP]
                                                                       ,[Parameters]
                                                                       ,[CusID]
                                                                       ,[LogonUser]
                                                                        )
                                                                 VALUES
                                                                       (
			                                                            getdate()
			                                                            ,@Controller
			                                                            ,@Action
                                                                        ,@IP
			                                                            ,@Parameters
                                                                        ,@CusID
			                                                            ,@LogonUser
                                                                       )");

                // 執行單獨的，不需要再次記錄Log的方法
                CommandParameterCollection para = new CommandParameterCollection();
                para.Add(new CommandParameter("@Controller", (string.IsNullOrEmpty(_controller)) ? "NoController" : _controller));
                para.Add(new CommandParameter("@Action", (string.IsNullOrEmpty(_action)) ? "NoAction" : _action));
                para.Add(new CommandParameter("@IP", _ip));
                para.Add(new CommandParameter("@Parameters", _parameters));
                if (_cusid.Trim().Length > 11)
                {
                    _cusid = _cusid.Substring(0, 11);
                }
                para.Add(new CommandParameter("@CusID", _cusid));
                //para.Add(new CommandParameter("@ApplNo", _applno));
                para.Add(new CommandParameter("@LogonUser", (string.IsNullOrEmpty(_user)) ? "NoUser" : _user));

                using (SqlConnection AConnection = this.OpenConnection())
                {
                    using (SqlCommand NewCommand = new SqlCommand(strsql.ToString(), AConnection))
                    {
                        foreach (CommandParameter NewParameter in para)
                        {
                            if (strsql.ToString().ToUpper().Contains(NewParameter.ColumnName.Replace("@", "").ToUpper()))
                            {
                                SqlParameter newSParam = new SqlParameter(NewParameter.ColumnName, NewParameter.Value ?? DBNull.Value);
                                NewCommand.Parameters.Add(newSParam);
                            }
                        }
                        NewCommand.CommandText = strsql.ToString();
                        if (_cusid.Trim().Length > 0)
                        {
                            NewCommand.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }
        #endregion
    }
}
