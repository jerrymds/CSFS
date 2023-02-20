using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using CTBC.CSFS.Models;
using CTBC.CSFS.Pattern;
using CTBC.FrameWork.Platform;

namespace CTBC.FrameWork.HTG
{
    public class CommonBIZ : BaseBusinessRule
    {
        //public AppController _applController;
        // 策略物件    		


        #region 構造函數

        public CommonBIZ()
            : base()
        {

        }

        public CommonBIZ(string connectstring)
            : base(connectstring)
        {

        }

        //public CommonBIZ(AppController appController)             : base(appController)
        //{
        //    _applController = appController;
        //}

        #endregion

        #region Common共用Function

        /// <summary>
        /// 取得某個CodeType之下的所有(啟用+未啟用)的PARMCode資料
        /// </summary>
        /// <param name="codeType">CodeType欄位</param>
        public IList<PARMCode> GetCodeData(string codeType)
        {
            //可從Cache中將參數資料,透過ParmCode這個Key值直接取出
            var parmCodeList = (IEnumerable<PARMCode>)AppCache.Get("PARMCode");

            var query = from item in parmCodeList
                        where item.CodeType == codeType
                        orderby item.SortOrder
                        select item;

            return query.ToList();
        }


        public IList<PARMCode> GetPARMCodeByCodeType(string CodeType, string codeNo)
        {
            try
            {
                string sql = "select * from PARMCode where CodeType = @CodeType and CodeNo = @CodeNo";
                Parameter.Clear();
                Parameter.Add(new CommandParameter("@CodeType", CodeType));
                Parameter.Add(new CommandParameter("@CodeNo", codeNo));
                IList<PARMCode> list = SearchList<PARMCode>(sql);
                return list;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        ///  表單類型
        /// </summary>
        /// <param name="codeType">代碼類別</param>
        /// <param name="codeNo">代碼</param>
        /// <returns></returns>
        /// <remarks>Add By Niking 2011/12/05</remarks>

        public DateTime GetNowDateTime()
        {
            DateTime dtRet = DateTime.Now;

            try
            {
                string sql = @"SELECT convert(varchar(100),getdate(),121)";
                // 清空容器
                base.Parameter.Clear();
                DataTable dtResult = base.Search(sql);
                if (dtResult.Rows.Count > 0)
                {
                    dtRet = DateTime.Parse(dtResult.Rows[0][0].ToString());
                }
            }
            catch (Exception)
            {
                return dtRet;
            }

            return dtRet;
        }


        /// <summary>
        /// 讀取人員資料
        /// </summary>
        /// <returns>人員資料</returns>
        /// <remarks> add by mel 2013/09/10</remarks>
        public IList<CSFSEmployee> GetEmployeeList()
        {
            try
            {
                string sql = "";

                sql = @"SELECT 
	                        CSFSEmployee.EmpID
	                        ,CSFSEmployee.EmpName  
	                        ,CSFSEmployee.EmpID + '/' + CSFSEmployee.EmpName  AS EmDesc
                        From 
	                        CSFSEmployee   order by CSFSEmployee.EmpID  ";

                // 清空容器
                base.Parameter.Clear();

                return base.SearchList<CSFSEmployee>(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion



    }
}
