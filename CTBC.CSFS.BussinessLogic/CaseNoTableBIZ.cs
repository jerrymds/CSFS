using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CTBC.CSFS.Pattern;
using CTBC.CSFS.Models;
using System.Data;
using NPOI.HSSF.Record;

namespace CTBC.CSFS.BussinessLogic
{
    public class CaseNoTableBIZ : CommonBIZ
    {
        public int GetNo(string caseType, string caseDate,IDbTransaction trans = null)
        {
            if (string.IsNullOrEmpty(caseDate)) caseDate = DateTime.Today.ToString("yyyyMMdd");
            string sql = @"select * from [CaseNoTable] WITH (NOLOCK) where caseType=@CaseType and CaseDate=@CaseDate";
            base.Parameter.Clear();
            base.Parameter.Add(new CommandParameter("@CaseType", caseType));
            base.Parameter.Add(new CommandParameter("@CaseDate", caseDate));
            IList<CaseNoTable> list = trans == null
                                                    ? base.SearchList<CaseNoTable>(sql)
                                                    : base.SearchList<CaseNoTable>(sql, trans);
            //* 有當天資料,修改
            if (list != null && list.Any()) return UpdateNoAdd1(caseType, caseDate, trans);

            //* 沒有當天資料,新增
            CaseNoTable model = new CaseNoTable() {CaseType = caseType, CaseDate = caseDate, CaseNo = 1};
            return Create(model, trans);
            
        }

        public string GetDocNo(IDbTransaction trans = null)
        {
            return GetDocNo( DateTime.Today.ToString("yyyyMMdd"), trans);
        }

        //扣押维护
        public string GetDocNoMaintain(IDbTransaction trans = null)
        {
            string code = "00000" + Convert.ToString(GetNo("SYSTEM", "20010101", trans));
            return "20010101" + code.Substring(code.Length - 5);
            //return GetDocNo("20010101", trans);
        }

        public string GetDocNo(string caseDate, IDbTransaction trans = null)
        {
            if (string.IsNullOrEmpty(caseDate)) caseDate = DateTime.Today.ToString("yyyyMMdd");
            string code = "00000" + Convert.ToString(GetNo("SYSTEM", caseDate, trans));
            return caseDate + code.Substring(code.Length - 5);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="caseType">A,C1,C2,C3</param>
        /// <param name="trans"></param>
        /// <returns></returns>
        public string GetCaseNo(string caseType, IDbTransaction trans = null)
        {
            return GetCaseNo(caseType, DateTime.Today.ToString("yyyyMMdd"), trans);
        }
        public string GetCaseNo(string caseType, string caseDate, IDbTransaction trans = null)
        {
            if (string.IsNullOrEmpty(caseDate)) caseDate = DateTime.Today.ToString("yyyyMMdd");
            string code = "";
            switch (caseType)
            {
            //案件編號3碼擴5碼 20170216 宏祥 update start
                case "A":
                    //* 扣押案件的案件編號：A+民國年7碼+XXXXX
                    code = "00000" + Convert.ToString(GetNo(caseType, caseDate, trans));
                    break;
                case "C1":
                    //* 外來文案件: C+民國年7碼+KXXXX (K:0~7 一科，K:9 二科，K:8 三科) 
                    code = "00000" + Convert.ToString(GetNo(caseType, caseDate, trans));
                    break;
                case "C2":
                    code = "00000" + Convert.ToString(GetNo(caseType, caseDate, trans));
                    code = "9" + code.Substring(code.Length - 4);
                    break;
                case "C3":
                    code = "00000" + Convert.ToString(GetNo(caseType, caseDate, trans));
                    code = "8" + code.Substring(code.Length - 4);
                    break;
            }
            return caseType.Substring(0,1) + FrameWork.Util.UtlString.FormatDateTw(caseDate).Replace("/","") + code.Substring(code.Length - 5);
            //案件編號3碼擴5碼 20170216 宏祥 update end
        }

        public string GetCaseNoForWarn(string caseType, IDbTransaction trans = null)
        {
            string caseDate = DateTime.Today.ToString("yyyy");
            string code = "";
            int MaxCode = 0;
            switch (caseType)
            {
                case "B":
                    //* 警示通報的案件編號：B+民國年3碼+流水號4碼
                    MaxCode = GetNo(caseType, caseDate, trans);
                    code = MaxCode.ToString("00000");
                    break;
            }

            return caseType.Substring(0, 1) + FrameWork.Util.UtlString.FormatDateTw(DateTime.Today.ToString("yyyyMMdd")).Replace("/", "").Substring(0, 3) + code;
        }

        public int Count(string CaseType,string CaseDate, IDbTransaction trans = null)
        {
            string sql = @"select count(0) from [dbo].[CaseNoTable] where CaseType=@CaseType and CaseDate=@CaseDate";

            base.Parameter.Clear();
            base.Parameter.Add(new CommandParameter("@CaseType", CaseType));
            base.Parameter.Add(new CommandParameter("@CaseDate", CaseDate));
            try
            {
                int n = trans == null ? base.ExecuteNonQuery(sql) : base.ExecuteNonQuery(sql, trans);                 
                if (n > 0)
                {
                    return 1;//有重複
                }
                else
                {
                    return 0;//無重複
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public CaseNoTable CaseNoTable(string CaseType,string CaseDate)
        {
            string sql = @"select top 1 * from [dbo].[CaseNoTable] where CaseType=@CaseType and CaseDate=@CaseDate order by CaseNo DESC";

            base.Parameter.Clear();

            // 添加參數
            base.Parameter.Add(new CommandParameter("@CaseType", CaseType));
            base.Parameter.Add(new CommandParameter("@CaseDate", CaseDate));
            try
            {
                IList<CaseNoTable> list = base.SearchList<CaseNoTable>(sql);
                if (list != null)
                {
                    if (list.Count > 0)
                    {
                        return list[0];
                    }
                    else
                    {
                        return new CaseNoTable();
                    }
                }
                else
                {
                    return new CaseNoTable();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public int Create(CaseNoTable model,IDbTransaction trans = null)
        {
            string strSql = @" insert into CaseNoTable  (CaseType,CaseDate,CaseNo) 
                                        values (
                                        @CaseType,@CaseDate,@CaseNo);";

            base.Parameter.Clear();

            // 添加參數
            base.Parameter.Add(new CommandParameter("@CaseType", model.CaseType));
            base.Parameter.Add(new CommandParameter("@CaseDate", model.CaseDate));
            base.Parameter.Add(new CommandParameter("@CaseNo", model.CaseNo));

            try
            {
                var n = trans == null ? base.ExecuteNonQuery(strSql) : base.ExecuteNonQuery(strSql,trans);
                return n>0 ? 1 : 0;
            }
            catch (Exception ex)
            {
                // 拋出異常
                throw;
            }
        }

        public int UpdateNoAdd1(string caseType, string caseDate, IDbTransaction trans = null)
        {
            string sql = @"  UPDATE [CaseNoTable] SET [CaseNo] = [CaseNo] + 1 WHERE CaseType = @CaseType AND CaseDate = @CaseDate;
                            SELECT [CaseType],[CaseDate],[CaseNo] FROM [CaseNoTable] WITH (NOLOCK) WHERE CaseType = @CaseType AND CaseDate = @CaseDate";

            base.Parameter.Clear();
            base.Parameter.Add(new CommandParameter("@CaseType", caseType));
            base.Parameter.Add(new CommandParameter("@CaseDate", caseDate));

            IList<CaseNoTable> list = trans == null
                                                    ? base.SearchList<CaseNoTable>(sql)
                                                    : base.SearchList<CaseNoTable>(sql, trans);
            if (list != null && list.Any()) return list[0].CaseNo;
                
            return 0;
            
        }
    }
}
