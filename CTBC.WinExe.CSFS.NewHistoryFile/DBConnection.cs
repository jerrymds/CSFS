using System.Data;
using System.Data.SqlClient;

namespace CTBC.WinExe.CSFS.NewHistoryFile
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
