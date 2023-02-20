using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CTBC.CSFS.Models;
using CTBC.CSFS.Pattern;
using Microsoft.Practices.EnterpriseLibrary.Logging;
using System.Data;

namespace CTBC.FrameWork.HTG
{
    public class HostMsgGrpBIZ : CommonBIZ
    {
        //public HostMsgGrpBIZ(AppController appController)  : base(appController)
        //{ }

        public HostMsgGrpBIZ()
        { }

        /// <summary>
        ///查詢HostMsgDetl設定（電文專用）
        /// </summary>
        /// <param name="txtCode">電文編碼</param>
        /// <param name="strType">上/下行</param>
        /// <returns></returns>
        public IList<HostMsgGrp> QueryHostMsgDetl(string txtCode, string strType)
        {
            try
            {
                string sqlStr = @"SELECT b.edata,b.cdata,b.dataorder,b.datatype,b.src_field,b.dest_table,b.dest_column
                                FROM HostMsgGrp t INNER JOIN HostMsgDetl b ON t.trans_id=b.trans_id
                                WHERE t.txcode=@txcode AND t.txkind=@strType AND t.is_use='Y' 
                                ORDER BY dest_table DESC,convert(int,b.dataorder) ASC";
                // 清空參數容器
                base.Parameter.Clear();

                // 添加參數
                base.Parameter.Add(new CommandParameter("@txcode", txtCode));
                base.Parameter.Add(new CommandParameter("@strType", strType));
                IList<HostMsgGrp> _ilsit = base.SearchList<HostMsgGrp>(sqlStr);
                return _ilsit;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        ///查詢最大流水號（電文專用）
        /// </summary>
        /// <param name="txtCode">電文編碼</param>
        /// <returns></returns>
        public string GetMaxSno(string txtCode)
        {
            try
            {
                string strDate = System.DateTime.Now.ToString("yyyyMMdd");
                string strHour = System.DateTime.Now.ToString("HHmmssfff");
                string strResult = "CSFS" + strDate + strHour;
                return strResult;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        ///查詢自增長欄位
        /// </summary>
        /// <param name="tablename">表名</param>
        /// <returns></returns>
        public string GetIdentityKey(string tablename)
        {
            try
            {
                string strResult = "1";
                string sql = "";
                sql = string.Format("select NEXT VALUE FOR SEQ{0}", tablename);
                object res = base.ExecuteScalar(sql);
                if (res != null)
                {
                    strResult = res.ToString();
                }
                return strResult;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public DataTable getDataTabe(string sqlStr)
        {
            try
            {
                //XMLHelper xml = new XMLHelper();
                //xml.LoadXml("HostMsgGrp.xml");
                //string sqlStr = xml.GetSingleNode("SQL//GetBranchId");
                //string sqlStr = "SELECT TOP 1 [CodeNo] FROM [PARMCode] WHERE [CodeType] = 'ESBSetting' AND [CodeDesc] = 'ESBBranchId';";
                var v = base.Search(sqlStr);

                //var v =  base.ExecuteScalar(sqlStr);
                if (v.Rows.Count != 0)
                {
                    return v;
                }
                return null;
            }
            catch (Exception)
            {
                throw;
            }

        }


        /// <summary>
        ///查詢銀電文登入TellerNo（電文專用）
        /// </summary>
        /// <returns></returns>
        public string GetTellerNo()
        {
            try
            {
                string strResult = "";
                string sqlStr = @"SELECT TOP 1 [CodeNo] FROM [PARMCode] WHERE [CodeType] = 'ESBSetting' AND [CodeDesc] = 'LOGINTELLERNO';";
                // 清空參數容器
                base.Parameter.Clear();
                object res = base.ExecuteScalar(sqlStr);
                if (res != null)
                {
                    strResult = res.ToString();
                }
                return strResult;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public DataTable LoginParm()
        {
            try
            {
                //XMLHelper xml = new XMLHelper();
                //xml.LoadXml("HostMsgGrp.xml");
                //string sqlStr = xml.GetSingleNode("SQL//LoginParm");
                string sqlStr = "SELECT [CodeDesc] AS prop_id,[CodeNo] AS prop_desc FROM [PARMCode] WHERE [CodeType] = 'ESBLoginParm' ORDER BY [SortOrder];";
                DataTable dt = base.Search(sqlStr);
                return dt;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public DataTable LogoutParm()
        {
            try
            {
                //XMLHelper xml = new XMLHelper();
                //xml.LoadXml("HostMsgGrp.xml");
                //string sqlStr = xml.GetSingleNode("SQL//LogoutParm");
                string sqlStr = "SELECT [CodeDesc] AS prop_id,[CodeNo] AS prop_desc FROM [PARMCode] WHERE [CodeType] = 'ESBLogoutParm' ORDER BY [SortOrder];";
                DataTable dt = base.Search(sqlStr);
                return dt;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public string GetBranchId()
        {
            try
            {
                //XMLHelper xml = new XMLHelper();
                //xml.LoadXml("HostMsgGrp.xml");
                //string sqlStr = xml.GetSingleNode("SQL//GetBranchId");
                string sqlStr = "SELECT TOP 1 [CodeNo] FROM [PARMCode] WHERE [CodeType] = 'ESBSetting' AND [CodeDesc] = 'ESBBranchId';";
                var v = base.ExecuteScalar(sqlStr);
                if (v != null)
                {
                    return v.ToString();
                }
                return "";
            }
            catch (Exception)
            {
                throw;
            }
        }
         
        public bool SaveESBData(ArrayList array)
        {
            bool flag = true;
            try
            {
                if (array.Count > 0)
                {
                    IDbConnection dbConnection = base.OpenConnection();
                    IDbTransaction dbTransaction = null;
                    using (dbConnection)
                    {
                        dbTransaction = dbConnection.BeginTransaction();
                        string strSql = "";
                        for (int i = 0; i < array.Count; i++)
                        {
                            strSql += array[i];
                        }
                        int rtn = base.ExecuteNonQuery(strSql, dbTransaction);
                        dbTransaction.Commit();
                    }
                }
            }
            catch
            {
                flag = false;
            }
            return flag;
        }
    }
}
