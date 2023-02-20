using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTBC.WinExe.WarningQuerySettingService
{
    class KDSql
    {
        #region 宣告
        public System.Data.SqlClient.SqlConnection SQLCon;
        public System.Data.SqlClient.SqlConnection CountCon;
        public System.Data.SqlClient.SqlDataAdapter AD;
        public System.Data.DataSet DS;
        public int Count = 0;
        public double dobCount = 0;
        public string strSQL = "";
        public string strErr = "";
        public string strReturn = "";
        public string strSQLCon = "";
        //		public string strSQLCECon = @"Data Source=.\Program Files\SMS\TPEMS.sdf";
        //		public string FileName = @".\Program Files\SMS\Host.ini";
        #endregion

        #region TODO: 在此加入建構函式的程式碼
        public KDSql(string _conn)
        {


            strSQLCon = _conn;
            //
            // TODO: 在此加入建構函式的程式碼
            //
        }
        #endregion


        #region SQLServExec(string _strSQL)，執行一 SQL 命令，若成功傳回1
        //執行一 SQL 命令，若成功傳回1
        public int SQLServExec(string _strSQL)
        {
            try
            {
                //Cursor.Current = Cursors.WaitCursor;
                SQLCon = new SqlConnection(strSQLCon);
                SQLCon.Open();
                System.Data.SqlClient.SqlCommand cmd = new SqlCommand(_strSQL, SQLCon);
                Count = cmd.ExecuteNonQuery();
                SQLCon.Close();
            }
            catch (System.Data.SqlClient.SqlException Err)
            {
                strErr = "執行 SQL 命令[" + _strSQL + "] 失敗" + "\n" + Err.Message.ToString();
                //MessageBox.Show( strErr ,  "執行SQL Command 錯誤" );
            }
            finally
            {
                //Cursor.Current = Cursors.Default;
            }
            return Count;
        }

        #endregion


        #region SQLServGetInt(string _strSQL)，執行 SQL 命令並傳回一 Int
        //執行 SQL 命令並傳回一 int
        public int SQLServGetInt(string _strSQL)
        {
            try
            {
                //Cursor.Current = Cursors.WaitCursor;
                SQLCon = new SqlConnection(strSQLCon);
                SQLCon.Open();
                System.Data.SqlClient.SqlCommand cecmd = new SqlCommand(_strSQL, SQLCon);
                Count = Convert.ToUInt16(cecmd.ExecuteScalar());
                SQLCon.Close();
            }
            catch (System.Data.SqlClient.SqlException Err)
            {
                strErr = "執行 SQL 命令[" + _strSQL + "] 失敗" + "\n" + Err.Message.ToString();
                //MessageBox.Show( strErr ,  "執行SQL Command 錯誤" );
            }
            finally
            {
                //Cursor.Current = Cursors.Default;
            }
            return Count;
        }
        #endregion

        #region SQLServGetInt2(string _strSQL,_strConn)，執行 SQL 命令並傳回一 Int
        //執行 SQL 命令並傳回一 int
        public int SQLServGetInt2(string _strSQL, string _strConn)
        {
            try
            {
                //Cursor.Current = Cursors.WaitCursor;
                SQLCon = new SqlConnection(_strConn);
                SQLCon.Open();
                System.Data.SqlClient.SqlCommand cecmd = new SqlCommand(_strSQL, SQLCon);
                Count = Convert.ToInt32(cecmd.ExecuteScalar());
                SQLCon.Close();
            }
            catch (System.Data.SqlClient.SqlException Err)
            {
                strErr = "執行 SQL 命令[" + _strSQL + "] 失敗" + "\n" + Err.Message.ToString();
                //MessageBox.Show( strErr ,  "執行SQL Command 錯誤" );
            }
            finally
            {
                //Cursor.Current = Cursors.Default;
            }
            return Count;
        }
        #endregion


        #region SQLServGetDouble(string _strSQL)，執行 SQL 命令並傳回一 Double
        //執行 SQL 命令並傳回一 Double
        public double SQLServGetDouble(string _strSQL)
        {
            //object myObject;
            try
            {
                //Cursor.Current = Cursors.WaitCursor;
                SQLCon = new SqlConnection(strSQLCon);
                SQLCon.Open();
                System.Data.SqlClient.SqlCommand cmd = new SqlCommand(_strSQL, SQLCon);
                object myObject = cmd.ExecuteScalar();
                if (myObject != null)
                {
                    if (myObject.GetType().ToString() != "System.DBNull")
                        dobCount = (double)myObject;
                    else
                        dobCount = 0;
                }
                else
                    dobCount = 0;

                //string strTemp =myObject.GetType().ToString();
                //dobCount = Convert.ToDouble(cecmd.ExecuteScalar());
                //dobCount = (double)cecmd.ExecuteScalar();
                SQLCon.Close();
            }
            catch (System.Data.SqlClient.SqlException Err)
            {
                strErr = "執行 SQL 命令[" + _strSQL + "] 失敗" + "\n" + Err.Message.ToString();
                //MessageBox.Show( strErr ,  "執行SQL Command 錯誤" );
            }
            finally
            {
                //Cursor.Current = Cursors.Default;
            }
            return dobCount;
        }
        #endregion

        #region SQLServGetStr(string _strSQL)，執行 SQL 命令並傳回一 String
        //執行 SQL 命令並傳回一 string
        public string SQLServGetStr(string _strSQL)
        {
            try
            {
                //Cursor.Current = Cursors.WaitCursor;
                SQLCon = new SqlConnection(strSQLCon);
                SQLCon.Open();
                System.Data.SqlClient.SqlCommand cmd = new SqlCommand(_strSQL, SQLCon);
                //strReturn = (string)(cmd.ExecuteScalar());
                object myObject = cmd.ExecuteScalar();
                if (myObject != null)
                {
                    if (myObject.GetType().ToString() != "System.DBNull")
                        strReturn = (string)myObject;
                    else
                        strReturn = "";
                }
                else
                    strReturn = "";

                SQLCon.Close();
            }
            catch (System.Data.SqlClient.SqlException Err)
            {
                strErr = "執行 SQL 命令[" + _strSQL + "] 失敗" + "\n" + Err.Message.ToString();
                //MessageBox.Show( strErr ,  "執行SQL Command 錯誤" );
            }
            finally
            {
                //Cursor.Current = Cursors.Default;
            }
            return strReturn;
        }
        #endregion
        #region SQLServGetStr2(string _strSQL,_strConn)，執行 SQL 命令並傳回一 String
        //執行 SQL 命令並傳回一 string
        public string SQLServGetStr2(string _strSQL, string _strConn)
        {
            try
            {
                //Cursor.Current = Cursors.WaitCursor;
                SQLCon = new SqlConnection(_strConn);
                SQLCon.Open();
                System.Data.SqlClient.SqlCommand cmd = new SqlCommand(_strSQL, SQLCon);
                //strReturn = (string)(cmd.ExecuteScalar());
                object myObject = cmd.ExecuteScalar();
                if (myObject != null)
                {
                    if (myObject.GetType().ToString() != "System.DBNull")
                        strReturn = (string)myObject;
                    else
                        strReturn = "";
                }
                else
                    strReturn = "";

                SQLCon.Close();
            }
            catch (System.Data.SqlClient.SqlException Err)
            {
                strErr = "執行 SQL 命令[" + _strSQL + "] 失敗" + "\n" + Err.Message.ToString();
                //MessageBox.Show( strErr ,  "執行SQL Command 錯誤" );
            }
            finally
            {
                //Cursor.Current = Cursors.Default;
            }
            return strReturn;
        }
        #endregion
        #region SQLServGetDataSet(string _strSQL)，執行 SQL 命令並傳回一 DataSet
        //執行一 SQL Serv 命令並傳回一 DataSet
        public System.Data.DataSet SQLServGetDataSet(string _strSQL)
        {
            try
            {
                //Cursor.Current = Cursors.WaitCursor;
                SQLCon = new SqlConnection(strSQLCon);
                SQLCon.Open();
                AD = new SqlDataAdapter(_strSQL, SQLCon);
                DS = new DataSet();
                AD.Fill(DS, "Result");
                AD.Dispose();
                SQLCon.Close();
            }
            catch (System.Data.SqlClient.SqlException Err)
            {
                strErr = "執行 SQL 命令[" + _strSQL + "] 失敗" + "\n" + Err.Message.ToString();
                //MessageBox.Show( strErr ,  "執行SQL Command 錯誤" );
            }
            finally
            {
                //Cursor.Current = Cursors.Default;
            }
            return DS;
        }


        public System.Data.DataTable getDataTable(string _strSQL)
        {
            DataTable Result = new DataTable();
            try
            {

                //Cursor.Current = Cursors.WaitCursor;
                SQLCon = new SqlConnection(strSQLCon);
                SQLCon.Open();
                AD = new SqlDataAdapter(_strSQL, SQLCon);
                DS = new DataSet();
                AD.Fill(DS, "Result");
                if (DS.Tables.Count > 0)
                {
                    Result = DS.Tables[0];
                    if (Result == null)
                        Result = null;
                }
                else
                    Result = null;
                AD.Dispose();
                SQLCon.Close();
            }
            catch (System.Data.SqlClient.SqlException Err)
            {
                strErr = "執行 SQL 命令[" + _strSQL + "] 失敗" + "\n" + Err.Message.ToString();
                //MessageBox.Show( strErr ,  "執行SQL Command 錯誤" );
            }
            finally
            {
                //Cursor.Current = Cursors.Default;
            }
            return Result;
        }

        #endregion

        #region SQLServ2Trans
        public void SQLServ2Trans(string strSQL1, string strSQL2)
        {
            SQLCon = new SqlConnection(strSQLCon);
            SQLCon.Open();

            System.Data.SqlClient.SqlCommand cmd = SQLCon.CreateCommand();
            System.Data.SqlClient.SqlTransaction SqlTrans;

            // Start a local transaction
            SqlTrans = SQLCon.BeginTransaction("KDTransaction");
            // Must assign both transaction object and connection
            // to Command object for a pending local transaction
            cmd.Connection = SQLCon;
            cmd.Transaction = SqlTrans;

            try
            {
                cmd.CommandText = strSQL1;
                cmd.ExecuteNonQuery();
                cmd.CommandText = strSQL2;
                cmd.ExecuteNonQuery();
                SqlTrans.Commit();
                SQLCon.Close();
            }
            catch (Exception Err)
            {
                try
                {
                    SqlTrans.Rollback("KDTransaction");
                }
                catch (SqlException Err2)
                {
                    if (SqlTrans.Connection != null)
                    {
                        strErr = Err2.GetType().ToString();
                    }
                }
                //strErr = Err.GetType().ToString();
                strErr = "執行 SQL 命令[" + strSQL1 + "] 失敗" + "\n" + Err.Message.ToString();
                //MessageBox.Show( strErr ,  "執行SQL Command 錯誤" );
            }
        }
        #endregion

        #region FillError
        //攔截Fill發生錯誤時的事件，強迫忽略錯誤並繼續執行
        protected static void FillError(object sender, FillErrorEventArgs args)
        {
            if (args.Errors.GetType() == typeof(System.OverflowException))
            {
                //Code to handle Precision Loss
                args.Continue = true;
            }
        }
        #endregion


        #region GetSQLNumber
        //轉換資料成 SQL COMMAND 格式之字串
        public string GetSQLString(string strValue)
        {
            string strReturn;

            if (strValue == "")
                strReturn = "NULL";
            else
                strReturn = "'" + strValue + "'";
            return strReturn;

        }

        public string GetSQLString(object objData)
        {
            string strReturn;

            if ((objData == null) || (objData is DBNull))
                strReturn = "NULL";
            else
                strReturn = "'" + objData.ToString() + "'";
            return strReturn;

        }


        public string GetSQLNumber(string strValue)
        {
            string strReturn;

            if (strValue == "")
                strReturn = "NULL";
            else
                strReturn = strValue;
            return strReturn;

        }

        public string GetSQLNumber(object objData)
        {
            string strReturn;

            if ((objData == null) || (objData is DBNull))
                strReturn = "NULL";
            else
                strReturn = objData.ToString();
            return strReturn;

        }

        #endregion

    }
}
