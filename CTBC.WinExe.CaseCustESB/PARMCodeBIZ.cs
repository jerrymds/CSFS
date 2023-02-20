/// <summary>
/// 程式說明：維護PARMCode參數檔
/// 維護部門:資訊管理處
/// 中國信託銀行 版權所有  ©  All Rights Reserved.
/// </summary>
/// 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using CTBC.CSFS.Models;
using CTBC.CSFS.Pattern;
using System.IO;

namespace CTBC.WinExe.CaseCustESB
{
    public class PARMCodeBIZ : BBRule
    {

        /// <summary>
        /// 2018/04/24 Written by Patrick Yang
        /// </summary>
        /// <param name="codeType"></param>
        /// <returns></returns>
        public IList<PARMCode> GetParmCodeByCodeType(string codeType)
        {
            try
            {
                string sql = @"select * from PARMCode where CodeType=@CodeType order by codeno";
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@CodeType", codeType));
                return base.SearchList<PARMCode>(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
