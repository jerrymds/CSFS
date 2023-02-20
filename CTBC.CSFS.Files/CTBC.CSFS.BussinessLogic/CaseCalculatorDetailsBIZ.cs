using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CTBC.CSFS.Models;
using CTBC.CSFS.Pattern;
using CTBC.FrameWork.Util;
using System.Data;

namespace CTBC.CSFS.BussinessLogic
{
    public class CaseCalculatorDetailsBIZ : CommonBIZ
    {
        public int Create(List<CaseCalculatorDetails> model)
        {
            string strSql = string.Empty;
            foreach (CaseCalculatorDetails detail in model)
            {
                strSql += @" insert into CaseCalculatorDetails  (CaseId,Amount,InterestRate,StartDate,EndDate,InterestDays,Interest,InterestReal) 
                                        values (
                                        @CaseId,@Amount,@InterestRate,@StartDate,@EndDate,@InterestDays,@Interest,@InterestReal);";

                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@CaseId", detail.CaseId));
                base.Parameter.Add(new CommandParameter("@Amount", detail.Amount));
                base.Parameter.Add(new CommandParameter("@InterestRate", detail.InterestRate));
                base.Parameter.Add(new CommandParameter("@StartDate", detail.StartDate));
                base.Parameter.Add(new CommandParameter("@EndDate", detail.EndDate));
                base.Parameter.Add(new CommandParameter("@InterestDays", detail.InterestDays));
                base.Parameter.Add(new CommandParameter("@Interest", detail.Interest));
                base.Parameter.Add(new CommandParameter("@InterestReal", detail.InterestReal));
            }

            try
            {
                // 執行新增返回是否成功
                return base.ExecuteNonQuery(strSql);
            }
            catch (Exception ex)
            {
                // 拋出異常
                throw ex;
            }
        }

        public int Create(CaseCalculatorDetails model)
        {
            string strSql = string.Empty;

            strSql = @" insert into CaseCalculatorDetails  (CaseId,Amount,InterestRate,StartDate,EndDate,InterestDays,Interest,InterestReal) 
                                        values (
                                        @CaseId,@Amount,@InterestRate,@StartDate,@EndDate,@InterestDays,@Interest,@InterestReal);";

            base.Parameter.Clear();
            base.Parameter.Add(new CommandParameter("@CaseId", model.CaseId));
            base.Parameter.Add(new CommandParameter("@Amount", model.Amount));
            base.Parameter.Add(new CommandParameter("@InterestRate", model.InterestRate));
            base.Parameter.Add(new CommandParameter("@StartDate", model.StartDate));
            base.Parameter.Add(new CommandParameter("@EndDate", model.EndDate));
            base.Parameter.Add(new CommandParameter("@InterestDays", model.InterestDays));
            base.Parameter.Add(new CommandParameter("@Interest", model.Interest));
            base.Parameter.Add(new CommandParameter("@InterestReal", model.InterestReal));


            try
            {
                // 執行新增返回是否成功
                return base.ExecuteNonQuery(strSql);
            }
            catch (Exception ex)
            {
                // 拋出異常
                throw ex;
            }
        }

        public List<CaseCalculatorDetails> DetailModel(Guid CaseId)
        {
            string strSql = @"select [CalcDId],[CaseId],CAST([Amount] as decimal(18,2)) as [Amount],[InterestRate],CONVERT(varchar(100), [StartDate], 111) as [StartDate] ,
                                        CONVERT(varchar(100), [EndDate], 111) as [EndDate],[InterestDays],CAST([Interest] as decimal(18,2)) as [Interest],[InterestReal] from 
                                        CaseCalculatorDetails  where CaseId=@CaseId";
            base.Parameter.Clear();
            base.Parameter.Add(new CommandParameter("@CaseId", CaseId));
            try
            {
                List<CaseCalculatorDetails> itemlist = new List<CaseCalculatorDetails>();
                IList<CaseCalculatorDetails> list = base.SearchList<CaseCalculatorDetails>(strSql);
                foreach (var item in list)
                {
                    item.StartDate = UtlString.FormatDateTw(item.StartDate);
                    item.EndDate = UtlString.FormatDateTw(item.EndDate);
                    itemlist.Add(item);
                }
                return itemlist;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public int SumInterest(Guid caseId)
        {
            //* Ge.Song
            string strSql = @"select ISNULL(SUM(InterestReal),0) as Sum from [dbo].[CaseCalculatorDetails] where CaseId=@CaseId";
            base.Parameter.Clear();
            base.Parameter.Add(new CommandParameter("@CaseId", caseId));
            try
            {
                int n =(int) base.ExecuteScalar(strSql);
                return Convert.ToInt32(n);

                //if ( n != null)
                //{
                    
                //}
                //else
                //{
                //    return 0;
                //}
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public int Delete(int calId)
        {
            string strSql = "delete from CaseCalculatorDetails where CalcDId=@CalcDId";
            base.Parameter.Clear();
            base.Parameter.Add(new CommandParameter("@CalcDId", calId));
            try
            {
                return base.ExecuteNonQuery(strSql);
            }
            catch (Exception ex)
            {
                // 拋出異常
                throw ex;
            }
        }

        public bool DeleteCaseCalculatorMain(Guid caseId, IDbTransaction dbtrans = null)
        {
            string strSql = "DELETE CaseCalculatorMain WHERE  CaseId = @CaseId ";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("CaseId", caseId));
            return dbtrans == null ? ExecuteNonQuery(strSql) > 0 : ExecuteNonQuery(strSql, dbtrans) > 0;
        }

        public bool DeleteCaseCalculatorDetails(Guid caseId, IDbTransaction dbtrans = null)
        {
            string strSql = "DELETE CaseCalculatorDetails WHERE  CaseId = @CaseId ";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("CaseId", caseId));
            return dbtrans == null ? ExecuteNonQuery(strSql) > 0 : ExecuteNonQuery(strSql, dbtrans) > 0;
        }
    }
}
