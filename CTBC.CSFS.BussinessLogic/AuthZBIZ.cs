/// <summary>
/// 程式說明:維護授權Table:AuthZ
/// 維護部門:資訊管理處
/// 中國信託銀行 版權所有  ©  All Rights Reserved.
/// </summary>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CTBC.CSFS.Pattern;
using CTBC.CSFS.Models;



namespace CTBC.CSFS.BussinessLogic
{
    public class AuthZBIZ : CommonBIZ
    {
        public IEnumerable<AuthZ> selectAuthZ()
        {
            try
            {
                string appName = (string.IsNullOrEmpty(Config.GetValue("CUF_AppName"))) ? "CSFS" : Config.GetValue("CUF_AppName");
                string sql = @"select top 1 * from AuthZ where AppName='" + appName + "' ";
                base.Parameter.Clear();
                return base.SearchList<AuthZ>(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}