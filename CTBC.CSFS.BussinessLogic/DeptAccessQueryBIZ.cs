using CTBC.CSFS.Pattern;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using CTBC.FrameWork.Util;

namespace CTBC.CSFS.BussinessLogic
{
    public class DeptAccessQueryBIZ : CommonBIZ
    {
        public DeptAccessQueryBIZ(AppController AppController)
            : base(AppController)
        { }

        public DeptAccessQueryBIZ()
        { }
        /// <summary>
        /// 将字符串转成二进制
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public MemoryStream bianma(DataTable dt)
        {
            string s = string.Empty;
            string NewLine = "\r\n";
            if (dt.Rows.Count>0)
            {
                s = s + "日期:" + dt.Rows[0]["time"] + NewLine + NewLine;
            }
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                s = s + Convert.ToString(dt.Rows[i]["AccessData"]) + NewLine + NewLine;
            }
            //if (s.Length > 0)
            //{
            //    s = s.Replace("", "\r\n");
            //}
            MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(s));
            ms.Dispose();
            return ms;
        }

        public DataTable GetData(string AccessDataS, string AccessDataE)
        {
            try
            {
                AccessDataS = UtlString.FormatDateTwStringToAd(AccessDataS);
                AccessDataE = UtlString.FormatDateTwStringToAd(Convert.ToDateTime(UtlString.FormatDateString(AccessDataE)).AddDays(1).ToString("yyyy/MM/dd"));
                string strSql = @"select AccessData,(select CONVERT(varchar(100), GETDATE(), 111)) as time from CaseDepartmentAccess where CreatedDate>=@CreatedDateS and CreatedDate<@CreatedDateE";
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@CreatedDateS", AccessDataS));
                base.Parameter.Add(new CommandParameter("@CreatedDateE", AccessDataE));
                return base.Search(strSql);

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

    }
}
