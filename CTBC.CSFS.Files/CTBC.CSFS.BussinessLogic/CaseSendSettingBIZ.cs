using CTBC.CSFS.Models;
using CTBC.CSFS.Pattern;
using CTBC.FrameWork.Util;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CTBC.CSFS.Resource;
using CTBC.CSFS.ViewModels;

namespace CTBC.CSFS.BussinessLogic
{
    public class CaseSendSettingBIZ : CommonBIZ
    {
        public CaseSendSettingBIZ(AppController appController)
            : base(appController)
        { }

        public CaseSendSettingBIZ()
        { }

        /// <summary>
        /// 通過CaseId查詢該CaseId下所有的發文設定信息
        /// </summary>
        /// <param name="caseId"></param>
        /// <returns></returns>
        public IList<CaseSendSettingQueryResultViewModel> GetSendSettingList(Guid caseId)
        {
            string strSql = @"SELECT [SerialID]
                                    ,S.[CaseId]
                                    ,[Template]
                                    ,[SendWord]
                                    ,[SendNo]
                                    ,[SendDate]
                                    ,S.[Speed]
                                    ,[Security]
                                    ,[Subject]
                                    ,[Description]
                                    ,[isFinish]
                                    ,[FinishDate]
                                    ,[Attachment]
                                    ,S.[CreatedUser]
                                    ,S.[CreatedDate]
                                    ,S.[ModifiedUser]
                                    ,S.[ModifiedDate]
                                    ,S.[SendKind]
	                                ,M.[CaseNo]
	                                ,M.[ApproveDate]
                                    ,M.[Status]
                                FROM [CaseSendSetting] AS S
                                LEFT OUTER JOIN [CaseMaster] AS M ON S.CaseId = M.CaseId
                                WHERE S.[CaseId] = @CaseId";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("caseId", caseId));
            return SearchList<CaseSendSettingQueryResultViewModel>(strSql);
        }
        public CaseSendSetting GetSendSetting(int serialId)
        {
            string strSql = @"SELECT [SerialID]
                                    ,S.[CaseId]
                                    ,[Template]
                                    ,[SendWord]
                                    ,[SendNo]
                                    ,[SendDate]
                                    ,S.[Speed]
                                    ,[Security]
                                    ,[Subject]
                                    ,[Description]
                                    ,[isFinish]
                                    ,[FinishDate]
                                    ,[Attachment]
                                    ,S.[CreatedUser]
                                    ,S.[CreatedDate]
                                    ,S.[ModifiedUser]
                                    ,S.[ModifiedDate]
                                    ,S.[SendKind]
                                FROM [CaseSendSetting] AS S
                                WHERE S.[SerialID] = @SerialID";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("SerialID", serialId));
            var rtnList = SearchList<CaseSendSetting>(strSql);
            return rtnList.FirstOrDefault();
        }

        /// <summary>
        /// 通過CaseId取得該CaseId下所有發文明細
        /// </summary>
        /// <param name="caseId"></param>
        /// <returns></returns>
        public IList<CaseSendSettingDetails> GetSendSettingDetails(Guid caseId)
        {
            string strSql = @"SELECT  [DetailsId]
                                ,[CaseId]
                                ,[SerialID]
                                ,[SendType]
                                ,[GovName]
                                ,[GovAddr]
                            FROM [CaseSendSettingDetails]
                            WHERE [CaseId] = @CaseId";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("caseId", caseId));
            return SearchList<CaseSendSettingDetails>(strSql);
        }
        public IList<CaseSendSettingDetails> GetSendSettingDetails(int serialId)
        {
            string strSql = @"SELECT  [DetailsId]
                                ,[CaseId]
                                ,[SerialID]
                                ,[SendType]
                                ,[GovName]
                                ,[GovAddr]
                            FROM [CaseSendSettingDetails]
                            WHERE [SerialID] = @SerialID";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("SerialID", serialId));
            return SearchList<CaseSendSettingDetails>(strSql);
        }
        public IList<CaseSendSettingDetails> GetSendSettingDetails(List<string> DetailIdList)
        {
            string strSql = @"SELECT  [DetailsId]
                                ,[CaseId]
                                ,[SerialID]
                                ,[SendType]
                                ,[GovName]
                                ,[GovAddr]
                            FROM [CaseSendSettingDetails]
                            WHERE 1=2 ";
            Parameter.Clear();
            for (int i = 0; i < DetailIdList.Count; i++)
            {
                strSql = strSql + " OR DetailsId = @DetailsId" + i + " ";
                Parameter.Add(new CommandParameter("@DetailsId" + i, DetailIdList[i].Split('|')[0]));
            }
            strSql = strSql + "order by GovName,GovAddr ";
            return SearchList<CaseSendSettingDetails>(strSql);
        }

        public IList<CaseSendSettingDetails> GetSendSettingDetailsByOrder(List<string> DetailIdList)
        {
            string strSql = @"select 0 as DetailsId,1 as No into #Map from CaseSendSettingDetails ";
            for (int i = 0; i < DetailIdList.Count; i++)
            {
                strSql = strSql + "insert #Map ( DetailsId,No) select DetailsId," + i + " from CaseSendSettingDetails where  DetailsId =" + DetailIdList[i].Split('|')[0] + " ";
            }
            strSql = strSql + @"SELECT  m.no,c.DetailsId
                                ,[CaseId]
                                ,[SerialID]
                                ,[SendType]
                                ,[GovName]
                                ,[GovAddr]
                            FROM [CaseSendSettingDetails] c
							join #Map m on c.DetailsId = m.DetailsId
                            WHERE 1=2 ";
            Parameter.Clear();
            for (int i = 0; i < DetailIdList.Count; i++)
            {
                strSql = strSql + " OR c.DetailsId = @DetailsId" + i + " ";
                Parameter.Add(new CommandParameter("@DetailsId" + i, DetailIdList[i].Split('|')[0]));
            }
            strSql = strSql + "order by m.No drop table #Map ";
            return SearchList<CaseSendSettingDetails>(strSql);
        }

        /// <summary>
        /// 儲存CaseSetting
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool SaveCreate(CaseSendSettingCreateViewModel model, IDbTransaction trans = null)
        {
            //simon 2016/09/29
            if (model.SendKind != "電子發文")
                model.SendKind = "紙本發文";

            IDbConnection dbConnection = OpenConnection();
            bool rtn = true;
            bool needSubmit = false;
            try
            {
                if (trans == null)
                {
                    needSubmit = true;
                    trans = dbConnection.BeginTransaction();
                }
                rtn = rtn & InsertCaseSendSetting(ref model, trans);
                if (model.ReceiveList != null && model.ReceiveList.Any())
                {
                    foreach (CaseSendSettingDetails item in model.ReceiveList.Where(item => !string.IsNullOrEmpty(item.GovAddr) && !string.IsNullOrEmpty(item.GovAddr)))
                    {
                        item.CaseId = model.CaseId;
                        item.SerialID = model.SerialId;
                        item.SendType = CaseSettingDetailType.Receive;  //*正本
                        rtn = rtn && InsertCaseSendSettingDetials(item, trans);
                    }
                }

                if (model.CcList != null && model.CcList.Any())
                {
                    foreach (CaseSendSettingDetails item in model.CcList.Where(item => !string.IsNullOrEmpty(item.GovAddr) && !string.IsNullOrEmpty(item.GovAddr)))
                    {
                        item.CaseId = model.CaseId;
                        item.SerialID = model.SerialId;
                        item.SendType = CaseSettingDetailType.Cc; //*副本
                        rtn = rtn && InsertCaseSendSettingDetials(item, trans);
                    }
                }

                if (needSubmit)
                {
                    if (rtn)
                        trans.Commit();
                    else
                        trans.Rollback();
                }

                return rtn;
            }
            catch (Exception)
            {
                try
                {
                    if (trans != null) trans.Rollback();
                }
                catch (Exception)
                {
                    // ignored
                }
                return false;
            }
        }
        public bool InsertCaseSendSetting(ref CaseSendSettingCreateViewModel model, IDbTransaction trans)
        {
            bool result = true;
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    CaseSendSettingBIZ cssBIZ = new CaseSendSettingBIZ();
                    string sql = "";
                    //sql = @"DECLARE @SendNo bigint;
                    //        DECLARE @SendNoId bigint;
                    //        SELECT TOP 1 @SendNo = [SendNoNow] + 1,@SendNoId=[SendNoId] FROM [SendNoTable] WHERE [SendNoYear] = @SendNoYear AND [SendNoNow] < [SendNoEnd] ORDER BY SendNoId;
                    //        UPDATE [SendNoTable] SET [SendNoNow] = @SendNo WHERE [SendNoId] = @SendNoId;
                    //        INSERT INTO CaseSendSetting(CaseId,[Template],SendDate,SendWord,SendNo,Speed,Security,Subject,Description, Attachment,CreatedUser,CreatedDate,ModifiedUser,SendKind) 
                    //               VALUES (@CaseId,@Template,@SendDate,@SendWord,@SendNo1,@Speed,@Security,@Subject,@Description,@Attachment,@CreatedUser,GETDATE(),@ModifiedUser,@SendKind);
                    //        SELECT @@identity";
                    sql = @"DECLARE @SendNoId bigint;
                    DECLARE @flag as timestamp;
                    SELECT TOP 1 @SendNoId=[SendNoId],@flag=[TimesFlag] FROM [SendNoTable] WHERE [SendNoYear] = @SendNoYear AND [SendNoNow] < [SendNoEnd] ORDER BY SendNoId;
                    UPDATE [SendNoTable] SET [SendNoNow] = [SendNoNow]+1 WHERE [SendNoId] = @SendNoId and [TimesFlag]=@flag;
                    INSERT INTO CaseSendSetting(CaseId,[Template],SendDate,SendWord,SendNo,Speed,Security,Subject,Description, Attachment,CreatedUser,CreatedDate,ModifiedUser,SendKind) 
                           VALUES (@CaseId,@Template,@SendDate,@SendWord,@SendNo1,@Speed,@Security,@Subject,@Description,@Attachment,@CreatedUser,GETDATE(),@ModifiedUser,@SendKind);
                    SELECT @@identity";
                    Parameter.Clear();
                    base.Parameter.Add(new CommandParameter("@CaseId", model.CaseId));
                    base.Parameter.Add(new CommandParameter("@Template", model.Template));
                    base.Parameter.Add(new CommandParameter("@SendDate", model.SendDate));
                    base.Parameter.Add(new CommandParameter("@SendWord", model.SendWord));
                    if (model.SendKind == "電子發文")
                    {
                        //第四碼固定為2 --simon 2016/08/05
                        //model.SendNo = model.SendDate.Year + "00" + model.SendNo.Substring(9);
                        model.SendNo = cssBIZ.SendNo();
                        //20170630 緊急 RQ-2015-019666-0?? 發文字號擴碼 宏祥 update start
                        //model.SendNo = model.SendDate.Year + "20" + model.SendNo.Substring(9);
                        model.SendNo = (DateTime.Now.Year - 1911) + "2" + model.SendNo.Substring(9);
                        //20170630 緊急 RQ-2015-019666-0?? 發文字號擴碼 宏祥 update end
                    }
                    else
                    {
                        model.SendNo = cssBIZ.SendNo();
                    }
                    base.Parameter.Add(new CommandParameter("@SendNo1", model.SendNo));
                    base.Parameter.Add(new CommandParameter("@Speed", model.Speed));
                    base.Parameter.Add(new CommandParameter("@Security", model.Security));
                    base.Parameter.Add(new CommandParameter("@Subject", model.Subject));
                    base.Parameter.Add(new CommandParameter("@Description", model.Description));
                    base.Parameter.Add(new CommandParameter("@Attachment", model.Attachment));
                    base.Parameter.Add(new CommandParameter("@CreatedUser", Account));
                    base.Parameter.Add(new CommandParameter("@ModifiedUser", Account));
                    base.Parameter.Add(new CommandParameter("@SendNoYear", DateTime.Now.ToString("yyyy")));
                    base.Parameter.Add(new CommandParameter("@GovName", model.GovName));
                    base.Parameter.Add(new CommandParameter("@GovAddr", model.GovAddr));
                    base.Parameter.Add(new CommandParameter("@GovNameCc", model.GovNameCc));
                    base.Parameter.Add(new CommandParameter("@GovAddrCc", model.GovAddrCc));
                    base.Parameter.Add(new CommandParameter("@SendKind", model.SendKind));
                    
                    model.SerialId = trans == null ? Convert.ToInt32(ExecuteScalar(sql)) : Convert.ToInt32(ExecuteScalar(sql, trans));
                    result = true;
                    break;
                }
                catch (Exception)
                {
                    i++;
                    result = false;
                    trans.Rollback();
                }
            }
            return result;
        }
        public bool InsertCaseSendSettingDetials(CaseSendSettingDetails model, IDbTransaction trans)
        {
            string strSql = @"insert into CaseSendSettingDetails([CaseId],[SerialID],[SendType],[GovName],[GovAddr])
                                    values(@CaseId, @SerialID, @SendType, @GovName,@GovAddr)";
            Parameter.Clear(); ;
            base.Parameter.Add(new CommandParameter("@CaseId", model.CaseId));
            base.Parameter.Add(new CommandParameter("@SerialID", model.SerialID));
            base.Parameter.Add(new CommandParameter("@SendType", model.SendType));
            base.Parameter.Add(new CommandParameter("@GovName", model.GovName));
            base.Parameter.Add(new CommandParameter("@GovAddr", model.GovAddr));
            return trans == null ? ExecuteNonQuery(strSql) > 0 : ExecuteNonQuery(strSql, trans) > 0;
        }
        public bool SaveEdit(CaseSendSettingCreateViewModel model)
        {
            IDbConnection dbConnection = OpenConnection();
            IDbTransaction dbTransaction = null;
            bool rtn = true;
            try
            {
                using (dbConnection)
                {
                    dbTransaction = dbConnection.BeginTransaction();

                    rtn = rtn & UpdateCaseSendSetting(model, dbTransaction);
                    DeleteCaseSendSettingDetails(model.SerialId, dbTransaction);
                    if (model.ReceiveList != null && model.ReceiveList.Any())
                    {
                        foreach (CaseSendSettingDetails item in model.ReceiveList.Where(item => !string.IsNullOrEmpty(item.GovAddr) && !string.IsNullOrEmpty(item.GovAddr)))
                        {
                            item.CaseId = model.CaseId;
                            item.SerialID = model.SerialId;
                            item.SendType = CaseSettingDetailType.Receive;  //*正本
                            rtn = rtn && InsertCaseSendSettingDetials(item, dbTransaction);
                        }
                    }

                    if (model.CcList != null && model.CcList.Any())
                    {
                        foreach (CaseSendSettingDetails item in model.CcList.Where(item => !string.IsNullOrEmpty(item.GovAddr) && !string.IsNullOrEmpty(item.GovAddr)))
                        {
                            item.CaseId = model.CaseId;
                            item.SerialID = model.SerialId;
                            item.SendType = CaseSettingDetailType.Cc; //*副本
                            rtn = rtn && InsertCaseSendSettingDetials(item, dbTransaction);
                        }
                    }
                    if (rtn)
                        dbTransaction.Commit();
                    else
                        dbTransaction.Rollback();
                    return rtn;
                }
            }
            catch (Exception)
            {
                try
                {
                    if (dbTransaction != null) dbTransaction.Rollback();
                }
                catch (Exception)
                {
                    // ignored
                }
                return false;
            }
        }
        public bool UpdateCaseSendSetting(CaseSendSettingCreateViewModel model, IDbTransaction trans = null)
        {
            string sql = @"UPDATE [CaseSendSetting]
                           SET [Template] = @Template
                              ,[SendDate] = @SendDate
                              ,[Speed] = @Speed
                              ,[Security] = @Security
                              ,[Subject] = @Subject
                              ,[Description] = @Description
                              ,[Attachment] = @Attachment
                              ,[ModifiedUser] = @ModifiedUser
                              ,[ModifiedDate] = GETDATE()
                        WHERE [SerialID] = @SerialID
	                        AND [CaseId] = @CaseId";
            Parameter.Clear();
            base.Parameter.Add(new CommandParameter("SerialID", model.SerialId));
            base.Parameter.Add(new CommandParameter("CaseId", model.CaseId));
            base.Parameter.Add(new CommandParameter("Template", model.Template));
            base.Parameter.Add(new CommandParameter("@SendWord", model.SendWord));
            base.Parameter.Add(new CommandParameter("@SendNo", model.SendNo));
            base.Parameter.Add(new CommandParameter("@SendDate", model.SendDate));
            base.Parameter.Add(new CommandParameter("@Speed", model.Speed));
            base.Parameter.Add(new CommandParameter("@Security", model.Security));
            base.Parameter.Add(new CommandParameter("@Subject", model.Subject));
            base.Parameter.Add(new CommandParameter("@Description", model.Description));
            base.Parameter.Add(new CommandParameter("@Attachment", model.Attachment));
            base.Parameter.Add(new CommandParameter("@ModifiedUser", Account));
            return trans == null ? base.ExecuteNonQuery(sql) > 0 : base.ExecuteNonQuery(sql, trans) > 0;
        }
        public bool DeleteCaseSendSettingDetails(int serialId, IDbTransaction trans = null)
        {
            string sql = @"DELETE FROM [CaseSendSettingDetails] WHERE [SerialID] = @SerialID";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("SerialID", serialId));
            return trans == null ? ExecuteNonQuery(sql) > 0 : ExecuteNonQuery(sql, trans) > 0;
        }

        public bool DeleteCaseSendSetting(int serialId, IDbTransaction trans = null)
        {
            string sql = @"DELETE FROM CaseSendSetting WHERE [SerialID] = @SerialID";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("SerialID", serialId));
            return trans == null ? ExecuteNonQuery(sql) > 0 : ExecuteNonQuery(sql, trans) > 0;
        }

        public bool DeleteCaseSendSetting(Guid caseId, IDbTransaction dbtrans = null)
        {
            string strSql = "DELETE CaseSendSetting WHERE  CaseId = @CaseId ";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("CaseId", caseId));
            return dbtrans == null ? ExecuteNonQuery(strSql) > 0 : ExecuteNonQuery(strSql, dbtrans) > 0;
        }

        public bool DeleteCaseSendSettingDetails(Guid caseId, IDbTransaction dbtrans = null)
        {
            string strSql = "DELETE CaseSendSettingDetails WHERE  CaseId = @CaseId ";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("CaseId", caseId));
            return dbtrans == null ? ExecuteNonQuery(strSql) > 0 : ExecuteNonQuery(strSql, dbtrans) > 0;
        }

        public int Delete(int serialId)
        {

            string strSql = @"UPDATE [CasePayeeSetting] SET [SendId] = NULL WHERE [SendId] = @SerialID;                            
                            DELETE FROM CaseSendSetting where SerialID=@SerialID";

            Parameter.Clear();
            Parameter.Add(new CommandParameter("@SerialID", serialId));

            return ExecuteNonQuery(strSql) > 0 ? 1 : 0;
        }
        public CaseSendSettingCreateViewModel GetCaseSettingAndDetails(int serialId)
        {
            CaseSendSetting main = GetSendSetting(serialId);
            CaseSendSettingCreateViewModel rtn = new CaseSendSettingCreateViewModel
            {
                SerialId = main.SerialId,
                CaseId = main.CaseId,
                Template = main.Template,
                SendWord = main.SendWord,
                SendNo = main.SendNo,
                Speed = main.Speed,
                SendDate = main.SendDate,
                Security = main.Security,
                Subject = main.Subject,
                Description = main.Description,
                Attachment = main.Attachment,
                CreatedUser = main.CreatedUser,
                CreatedDate = main.CreatedDate,
                ModifiedUser = main.ModifiedUser,
                ModifiedDate = main.ModifiedDate,
                SendKind = main.SendKind,
                ReceiveList = new List<CaseSendSettingDetails>(),
                CcList = new List<CaseSendSettingDetails>()
            };

            IList<CaseSendSettingDetails> list = GetSendSettingDetails(serialId);
            if (list != null && list.Any())
            {
                foreach (CaseSendSettingDetails details in list)
                {
                    if (details.SendType == CaseSettingDetailType.Receive)
                        rtn.ReceiveList.Add(details);
                    if (details.SendType == CaseSettingDetailType.Cc)
                        rtn.CcList.Add(details);
                }
            }
            return rtn;
        }
        public DataTable GetSendSettingByCaseIdList(List<string> caseIdList)
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
                                WHERE 1=2 ";
            Parameter.Clear();
            for (int i = 0; i < caseIdList.Count; i++)
            {
                strsql = strsql + " OR M.[CaseId] = @CaseId" + i + " ";
                Parameter.Add(new CommandParameter("@CaseId" + i, caseIdList[i]));
            }
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
                        strCc=strCc.TrimEnd('、');
                        dr["Cc"]=strCc;
                    }
                }
                return Dt;
            }
            else
            {
                return Dt = new DataTable();
            }
        }

        public DataTable GetSeizurePayByCaseIdList(List<string> caseIdList)
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
                                FROM [CaseSendSetting] AS M
                                LEFT OUTER JOIN [CaseSendSettingDetails] AS D ON M.CaseId = D.CaseId AND M.SerialID =D.SerialID
                                WHERE (1=2 ";
            Parameter.Clear();
            for (int i = 0; i < caseIdList.Count; i++)
            {
                strsql = strsql + " OR M.[CaseId] = @CaseId" + i + " ";
                Parameter.Add(new CommandParameter("@CaseId" + i, caseIdList[i]));
            }
            strsql += @") and Template='支付'";
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
                string sqlRecive = "SELECT GovName,SerialID,SendType FROM CaseSendSettingDetails WHERE SendType=1 and SerialID In (" + strSerialId + ")";
                string sqlCc = "SELECT GovName,SerialID,SendType FROM CaseSendSettingDetails WHERE SendType=2 and SerialID In (" + strSerialId + ")";
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
        public string SendNo()
        {
            try
            {
                string sqlStr = @"select Top 1 (SendNoNow+1) as SendNoNow from SendNoTable where SendNoYear=@SendNoYear and SendNoNow<SendNoEnd";
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@SendNoYear", DateTime.Now.ToString("yyyy")));
                string result = Convert.ToString(base.ExecuteScalar(sqlStr));
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        /// <summary>
        /// 根據受款人資訊.和發文模版,生成一個發文的Model
        /// </summary>
        /// <param name="model">發文資訊</param>
        /// <param name="errMsg">錯誤訊息</param>
        /// <param name="trans"></param>
        /// <returns></returns>
        public CaseSendSettingCreateViewModel GetDefaultSendSetting(CasePayeeSetting model, out string errMsg, IDbTransaction trans = null)
        {
            if (model == null || string.IsNullOrEmpty(model.CheckNo))
            {
                //* 理論上不會call到這裡.因為在外面就檢查了.這裡寫就是防呆
                errMsg = Lang.csfs_text_notnull;
                return null;
            }

            CaseSendSettingBIZ cssBiz = new CaseSendSettingBIZ();
            string sendNo = cssBiz.SendNo();
            if (string.IsNullOrEmpty(sendNo))
            {
                errMsg = Lang.csfs_sendno;
                return null;
            }
            CaseMasterBIZ masterBiz = new CaseMasterBIZ();
            SendSettingRefBiz refbiz = new SendSettingRefBiz();
			SendSettingRef basic = refbiz.GetSubjectAndDescription(model.CaseId, CaseKind2.CasePay, CaseKind2.CaseSeizureEdoc, trans, model);
            CaseMaster master = masterBiz.MasterModel(model.CaseId, trans);
            CaseSendSettingCreateViewModel send = new CaseSendSettingCreateViewModel
            {
                CaseId = model.CaseId,
                Template = CaseKind2.CasePay,
                SendWord = Lang.csfs_ctci_bank,
                SendNo = sendNo,
                SendDate = String.IsNullOrEmpty(master.PayDate) ? masterBiz.GetPayDate(master.CaseKind2, master.CreatedDate) : Convert.ToDateTime(master.PayDate),
                Speed = Lang.csfs_speed1,
                Security = Lang.csfs_security1,
                Attachment = "",
                Subject = basic == null ? "" : basic.Subject,
                Description = basic == null ? "" : basic.Description,
                CreatedUser = Account,
                CreatedDate = DateTime.Now,
                ModifiedUser = Account,
                ModifiedDate = DateTime.Now,
                ReceiveList = new List<CaseSendSettingDetails>(),
                CcList = new List<CaseSendSettingDetails>()
            };
            send.ReceiveList.Add(new CaseSendSettingDetails { CaseId = model.CaseId, GovAddr = model.Address, GovName = model.Receiver, SendType = 1 });
            send.CcList.Add(new CaseSendSettingDetails { CaseId = model.CaseId, GovAddr = model.CCReceiver, GovName = model.Currency, SendType = 2 });
            errMsg = "";
            return send;
        }

    }
}
