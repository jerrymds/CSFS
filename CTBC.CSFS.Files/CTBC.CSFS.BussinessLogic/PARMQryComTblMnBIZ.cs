using System;
using System.Data;
using CTBC.CSFS.Pattern;

namespace CTBC.CSFS.BussinessLogic
{
    public class PARMQryComTblMnBIZ : CommonBIZ
    {
        #region 構造函數

        public PARMQryComTblMnBIZ(AppController appController)
            : base(appController)
        {

        }

        public PARMQryComTblMnBIZ()
        {

        }

        #endregion

        /// <summary>
        /// 根據User介面輸入的QryCondition
        /// </summary>
        /// <param name="sqlEntity">執行QryCondition</param>     
        /// <returns>執行結果</returns>
        public object Query(string qryStr)
        {
            try
            {
                // 分解執行QryCondition
                string query = qryStr.ToUpper();
                string[] strWhere = query.Replace("\t", " ").Replace("\r\n", " ").Split(' ');

                // 查詢
                if (strWhere[0] == "SELECT" || strWhere[0] == "DECLARE" || strWhere[0] == "WITH")
                {
                    //執行輸入的QryCondition
                    return base.Search(qryStr);
                }

                // 執行更新操作 
                else
                {
                    // 執行輸入的QryCondition
                    return base.ExecuteNonQuery(qryStr);
                }
            }
            catch (Exception ex)
            {              
                // 拋出異常
                return (object)ex.Message;
            }
        }
    }
}
