/// <summary>
/// 程式說明:Db連接
/// 維護部門:資訊管理處
/// CSFS Version:v3.0
/// 中國信託銀行 版權所有  ©  All Rights Reserved. 
/// </summary>

using System.Data;
using System.Data.SqlClient;

namespace CTBC.CSFS.Pattern
{
	public class DBConnection
	{
		/// <summary>
		/// 取得DB連結
		/// </summary>
		/// <returns>DB連結</returns>
        /// 2014/1/16 horace 移除static
		public IDbConnection NewDBConnection(string connectionString)
        {
            SqlConnection sqlConn = new SqlConnection(connectionString);
            sqlConn.Open();

            return sqlConn;
        }
	}
}
