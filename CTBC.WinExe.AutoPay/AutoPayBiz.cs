using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using CTBC.CSFS.BussinessLogic;
using CTBC.CSFS.Models;
using CTBC.CSFS.Pattern;

namespace CTBC.WinExe.AutoPay
{
    public class AutoPayBiz : BaseBusinessRule
    {

        /// <summary>
        /// 20220411, 取得案件執行的時間
        /// </summary>
        /// <param name="Firstcasedoc"></param>
        /// <returns></returns>
        internal DateTime getExectueTime(CaseMaster Firstcasedoc)
        {
            string retReason = string.Empty;
            DateTime ret = DateTime.Now.AddDays(-1);


            List<CaseMaster> Result = new List<CaseMaster>();

            string strsql = string.Format(@"SELECT TOP 1 * FROM CaseMaster c where CaseID='{0}'", Firstcasedoc.CaseId);

            Parameter.Clear();
            Result = SearchList<CaseMaster>(strsql).ToList();


            if (Result.Count() > 0)
            {
                retReason = Result.First().ReturnReason.Trim().Replace("執行時間:", "");
            }


            if (DateTime.TryParseExact(retReason, "yyyy-MM-dd HH:mm:ss", null, System.Globalization.DateTimeStyles.None, out ret))
            {
                //TryParseExact轉換成功
                return ret;
            }
            else
            {
                return ret;
            }



        }
        /// <summary>
        /// 20220411, 記錄該案件開始執行的時間.....
        /// </summary>
        /// <param name="casedoc"></param>
        internal void UpdateReturnReasonExecuteTime(CaseMaster casedoc)
        {

            string strsql = @"UPDATE CASEMASTER SET ReturnReason='" + "執行時間:" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "' WHERE CASEID='" + casedoc.CaseId.ToString() + "'";

            Parameter.Clear();
            ExecuteNonQuery(strsql);

        }



        internal List<CaseMaster> getCaseMasterStillRuning()
        {
            List<CaseMaster> Result = new List<CaseMaster>();

            string strsql = @"SELECT c.*  FROM [dbo].[CaseMaster] c inner join [dbo].EDocTXT3 e on c.CaseId=e.CaseId 
   where CaseKind2='支付' and ReceiveKind='電子公文' and c.Status='997' and IsEnable='1' and (NULLIF(ReturnReason, '') IS NULL ) ORDER BY CASENO";

            Parameter.Clear();
            Result =  SearchList<CaseMaster>(strsql).ToList();

            return Result;
        }

        internal List<CaseMaster> getCaseMasterStillRuningCase()
        {
            //List<CaseMaster> Result = new List<CaseMaster>();

            string strsql = @"SELECT c.*  FROM [dbo].[CaseMaster] c inner join [dbo].EDocTXT3 e on c.CaseId=e.CaseId 
   where CaseKind2='支付' and ReceiveKind='電子公文' and c.Status='997' ";

            Parameter.Clear();
            return SearchList<CaseMaster>(strsql).ToList();
        }


        internal int UpdateParmCode(string strCodeType, string strCaseNo)
        {
            int iCount = 1;


            try
            {
                string sql1 = "SELECT TOP 1 * from PARMCode WHERE CodeType='" + strCodeType + "'";
                var result = SearchList<PARMCode>(sql1).ToList();
                var cNo = result.Where(x => x.CodeType == strCodeType).FirstOrDefault();
                if (cNo != null)
                {
                    if (cNo.CodeNo == strCaseNo) // 如果這次, 跟上次一樣, 則把次數+1
                    {
                        iCount = int.Parse(cNo.CodeTag.ToString()) + 1;
                        cNo.CodeTag = iCount.ToString();
                        string sql2 = "UPDATE PARMCode SET CodeTag='" + iCount.ToString() + "' WHERE CodeType='" + strCodeType + "'";
                        Parameter.Clear();
                        ExecuteNonQuery(sql2);
                    }
                    else
                    {
                        cNo.CodeNo = strCaseNo;
                        cNo.CodeTag = "1";
                        iCount = 1;
                        string sql2 = "UPDATE PARMCode SET CodeTag='" + iCount.ToString() + "',CodeNo='" + strCaseNo + "' WHERE CodeType='" + strCodeType + "'";
                        Parameter.Clear();
                        ExecuteNonQuery(sql2);
                    }

                }
            }
            catch (Exception)
            {

            }

            return iCount;
        }

        internal void setCaseMasterC01B01(Guid first)
        {
            try
            {
                string sql = "UPDATE CaseMaster SET Status='B01' WHERE Status='997';UPDATE CaseMaster SET Status='C01',AgentUser='SYS' WHERE CaseId=@CaseId";
                Parameter.Clear();
                Parameter.Add(new CommandParameter("@CaseId", first));

                ExecuteNonQuery(sql);

            }
            catch (Exception)
            {
                
                
            }
            return;
        }

        internal bool UpdateProcCaseNo(string strCodeType, string strCaseNo)
        {
            bool result = false;

            try
            {
                string sql1 = string.Format("UPDATE PARMCode SET CodeNo='{0}', CodeTag='1', ModifiedDate=getdate() WHERE CodeType='{1}' ", strCaseNo, strCodeType);
                Parameter.Clear();
                ExecuteNonQuery(sql1);
                result = true;
            }
            catch (Exception)
            {
                result = false;
                throw;
            }


            return result;
        }

        internal PARMCode getParmCodeByCodeType(string strCodeType)
        {
            PARMCode results = new PARMCode();
            string sql1 = "SELECT TOP 1 * from PARMCode WHERE CodeType='" + strCodeType + "'";
            results = SearchList<PARMCode>(sql1).ToList().First();

            return results;
        }

        internal string insertCaseMemo(Guid guid, string p, List<string> DocMemo, string eQueryStaff)
        {

            string ret = "";
            try
            {
                var newDocMemo = DocMemo.Distinct();
                foreach (var s in newDocMemo)
                {
                    string sql = @"INSERT INTO CaseMemo(CaseId, MemoType, Memo, MemoDate, MemoUser) 
                                                   VALUES (@CaseId, @MemoType, @Memo, getDate() , @MemoUser )";
                    Parameter.Clear();
                    Parameter.Add(new CommandParameter("@CaseId", guid));
                    Parameter.Add(new CommandParameter("@MemoType", p));
                    Parameter.Add(new CommandParameter("@Memo", s));
                    Parameter.Add(new CommandParameter("@MemoUser", eQueryStaff));

                    ExecuteNonQuery(sql);
                }

            }
            catch (Exception ex)
            {

                ret = ex.Message.ToString();
            }
            return ret;


        }


        internal string setExecuteCaseNo(string strCodeType, string CaseNo)
        {
            string ret = "";
            try
            {
                string sql = "UPDATE PARMCode SET CodeNo=@CaseNo, ModifiedDate=getdate() WHERE CodeType=@CodeType ";
                Parameter.Clear();
                Parameter.Add(new CommandParameter("@CaseNo", CaseNo));
                Parameter.Add(new CommandParameter("@CodeType", strCodeType));
                ExecuteNonQuery(sql);
            }
            catch (Exception ex)
            {
                ret = ex.Message.ToString();
            }
            return ret;
        }
    }
}
