using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using CTBC.CSFS.Pattern;
using CTBC.CSFS.Models;

namespace CTBC.CSFS.BussinessLogic
{
    public class SendEDocBiz : CommonBIZ
    {
        public IList<PARMCode> GetPARMCodeByCodeType(string CodeType)
        {
            
            try
            {
                string sql = "select * from PARMCode where CodeType = @CodeType";
                Parameter.Clear();
                Parameter.Add(new CommandParameter("@CodeType", CodeType));
                IList<PARMCode> list = SearchList<PARMCode>(sql);
                return list;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public IList<BatchControl> GetBatchControlF()
        {
            try
            {
                string sql = @"select * from BatchControl where STATUS_Create=0";
                IList<BatchControl> list = SearchList<BatchControl>(sql);
                return list;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public IList<BatchControl> GetBatchControlG(Guid caseId)
        {
            try
            {
                string sql = @"select * from BatchControl where STATUS_Create=1 and caseid='" + caseId + "' ";
                IList<BatchControl> list = SearchList<BatchControl>(sql);
                return list;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public bool UpdateBatchControlT(Guid caseId)
        {
            try
            {
                string sql = @"update BatchControl set STATUS_Create=1 where caseid='" + caseId + "' ";
                return (int)base.ExecuteNonQuery(sql) > 0;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public bool DeleteBatchControl(Guid caseId)
        {
            try
            {
                string sql = @"Delete BatchControl where caseid='" + caseId + "' ";
                return (int)base.ExecuteNonQuery(sql) > 0;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public DataTable GetCaseMasterByCaseId(Guid caseId)
        {
            string strsql = @"SELECT *,(SELECT ISNULL(SUM(Amount),0) FROM [CaseAccountExternal] AS CAE WHERE CAE.CaseId = M.CaseId) AS ExtTotal FROM [CaseMaster] AS M WHERE CaseId = @CaseId";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("@CaseId", caseId));
            strsql = strsql + " order by CaseNo";
            DataTable Dt = Search(strsql);
            if (Dt.Rows.Count > 0)
            {
                return Dt;
            }
            else
            {
                return Dt = new DataTable();
            }
        }

        /// <summary>
        /// 扣押資訊
        /// </summary>
        /// <param name="caseId"></param>
        /// <returns></returns>
        public DataTable GetCaseSeizureByCaseId(Guid caseId)
        {
            string strsql = "";
            //CSFS-66 modify by nianhuaxiao 20170728 start
            //strsql = "select CustId,CustName,sum(SeizureAmountNtd) as TotalAmount from CaseSeizure where caseid='" + caseId + "' ";
            //strsql += "group by CustId,CustName ";
            strsql = "select CustId,CustName,sum(SeizureAmountNtd) as TotalAmount ,ObligorName,ObligorNo from CaseSeizure cs left join CaseObligor co on co.CaseId = cs.CaseId and co.ObligorNo = cs.CustId where cs.CaseId ='" + caseId + "' ";
            strsql += "group by CustId,CustName,ObligorName,ObligorNo ";
            //CSFS-66 modify by nianhuaxiao 20170728 end

            //Parameter.Clear();
            //Parameter.Add(new CommandParameter("@CaseId", caseId));
            //strsql = strsql + " order by CaseNo";
            DataTable Dt = Search(strsql);
            if (Dt.Rows.Count > 0)
            {
                return Dt;
            }
            else
            {
                return Dt = new DataTable();
            }
        }
        /// <summary>
        /// 義務人資訊
        /// </summary>
        /// <param name="caseId"></param>
        /// <returns></returns>
        public DataTable GetCaseObligorByCaseId(Guid caseId)
        {
            string strsql = "";
            strsql = "select * from CaseObligor where caseid='" + caseId + "' ";
           

            //Parameter.Clear();
            //Parameter.Add(new CommandParameter("@CaseId", caseId));
            //strsql = strsql + " order by CaseNo";
            DataTable Dt = Search(strsql);
            if (Dt.Rows.Count > 0)
            {
                return Dt;
            }
            else
            {
                return Dt = new DataTable();
            }
        }

        public IList<CaseSendSettingDetails> GetSendSettingDetails(int serialId)
        {
            string strSql = @"SELECT distinct a.DetailsId
                                ,a.CaseId
                                ,a.SerialID
                                ,a.SendType
                                ,a.GovName
                                ,a.GovAddr
                                ,b.GovCode
                            FROM CaseSendSettingDetails a inner join GovAddress b on a.GovName=b.GovName
                            WHERE a.SerialID = @SerialID";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("SerialID", serialId));
            return SearchList<CaseSendSettingDetails>(strSql);
        }

        public DataTable GetSendSettingBySerialID(int SerialID)
        {
            string strsql = @"SELECT M.[SerialID]
                                    , M.[CaseId]
                                    ,[Template]
                                    ,[SendWord]
                                    ,[SendNo]
                                    ,[SendDate]
                                    ,[Speed]
                                    ,[Security]
                                    ,[Subject]
                                    ,[Description]
                                    ,[isFinish]
                                    ,[FinishDate]
									,[Attachment]
                                    ,[GovName]
                                    ,[GovAddr]
                                    ,[SendType]
									,(select EmpName from [LDAPEmployee] emp where emp.EmpID=M.CreatedUser) as CreatedUser									
									,(select TelNo + ' 分機 '+TelExt from [LDAPEmployee] emp where emp.EmpID=M.CreatedUser) as TelNo 
                                FROM [CaseSendSetting] AS M with(Nolock)
                                LEFT OUTER JOIN [CaseSendSettingDetails] AS D ON M.CaseId = D.CaseId AND M.SerialID =D.SerialID
                                WHERE D.SerialID=@SerialID";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("@SerialID", SerialID));

            DataTable Dt = Search(strsql);
            Dt.Columns.Add("Receive");
            Dt.Columns.Add("Cc");
            if (Dt != null && Dt.Rows.Count > 0)
            {
                string strSerialId = string.Empty;
                foreach (DataRow dr in Dt.Rows)
                {
                    strSerialId += "'" + dr["SerialID"].ToString() + "',";
                }
                strSerialId = strSerialId.TrimEnd(',');
                string sqlRecive = "SELECT GovName,SerialID FROM CaseSendSettingDetails WHERE SendType=1 and SerialID In (" + strSerialId + ")";
                string sqlCc = "SELECT GovName,SerialID FROM CaseSendSettingDetails WHERE SendType=2 and SerialID In (" + strSerialId + ")";
                List<CaseSendSettingDetails> listRecive = base.SearchList<CaseSendSettingDetails>(sqlRecive).ToList();
                List<CaseSendSettingDetails> listCc = base.SearchList<CaseSendSettingDetails>(sqlCc).ToList();
                if (listRecive != null && listRecive.Any())
                {
                    foreach (DataRow dr in Dt.Rows)
                    {
                        string strRecive = string.Empty;
                        foreach (CaseSendSettingDetails item in listRecive.Where(m => m.SerialID == Convert.ToInt32(dr["SerialID"])))
                        {
                            strRecive += item.GovName + "、";
                        }
                        strRecive = strRecive.TrimEnd('、');
                        dr["Receive"] = strRecive;
                    }
                }

                if (listCc != null && listCc.Any())
                {
                    foreach (DataRow dr in Dt.Rows)
                    {
                        string strCc = string.Empty;
                        foreach (CaseSendSettingDetails item in listCc.Where(m => m.SerialID == Convert.ToInt32(dr["SerialID"])))
                        {
                            strCc += item.GovName + "、";
                        }
                        strCc = strCc.TrimEnd('、');
                        dr["Cc"] = strCc;
                    }
                }
                return Dt;
            }
            else
            {
                return Dt = new DataTable();
            }
        }

        public IList<SendTimeSection> GetSendTimeSectionList()
        {
            try
            {
                string strSql = @"SELECT * FROM [SendTimeSection] order by TimeSection";
                Parameter.Clear();
                return SearchList<SendTimeSection>(strSql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public int Create(SendTimeSection model)
        {
            try
            {
                string sqlStr = @"insert into SendTimeSection
                                        (
                                            TimeSection
                                        ) 
                                        VALUES
                                        (
                                            @TimeSection
                                        )";

                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@TimeSection", model.TimeSection));
                return base.ExecuteNonQuery(sqlStr);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public int ValiReTime(SendTimeSection model)
        {
            try
            {
                string sqlStr = @"select count(*) from SendTimeSection where TimeSection=@TimeSection";
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@TimeSection", model.TimeSection));
                return (int)base.ExecuteScalar(sqlStr);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public int Delete(string timeSection)
        {
            try
            {
                string sqlStr = @"delete SendTimeSection where TimeSection = @TimeSection";

                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@TimeSection", timeSection));

                return base.ExecuteNonQuery(sqlStr);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public SendTimeSection Select(string timesection)
        {
            try
            {
                string sqlStr = @"select * from SendTimeSection where timesection=@timesection";

                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@timesection", timesection));

                IList<SendTimeSection> list = base.SearchList<SendTimeSection>(sqlStr);
                if (list != null)
                {
                    if (list.Count > 0)
                    {
                        return list[0];
                    }
                    else
                    {
                        return new SendTimeSection();
                    }
                }
                else
                {
                    return new SendTimeSection();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public bool Edit(SendTimeSection model, string timesectionEdit)
        {
            string sqlStr = @"update SendTimeSection
                                    set timesection=@timesection
                                    where timesection = @timesectionEdit";

            base.Parameter.Clear();
            base.Parameter.Add(new CommandParameter("@timesection", model.TimeSection));
            base.Parameter.Add(new CommandParameter("@timesectionEdit", timesectionEdit));
            try
            {
                return base.ExecuteNonQuery(sqlStr) > 0;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
