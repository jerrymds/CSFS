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
    public class HistoryCaseSendSettingBIZ : CommonBIZ
    {
        public HistoryCaseSendSettingBIZ(AppController appController)
            : base(appController)
        { }

        public HistoryCaseSendSettingBIZ()
        { }

        /// <summary>
        /// 通過CaseId查詢該CaseId下所有的發文設定信息
        /// </summary>
        /// <param name="caseId"></param>
        /// <returns></returns>
        public IList<HistoryCaseSendSettingQueryResultViewModel> GetSendSettingList(Guid caseId)
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
                                FROM [History_CaseSendSetting] AS S
                                LEFT OUTER JOIN [History_CaseMaster] AS M ON S.CaseId = M.CaseId
                                WHERE S.[CaseId] = @CaseId";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("caseId", caseId));
            return SearchList<HistoryCaseSendSettingQueryResultViewModel>(strSql);
        }
        public HistoryCaseSendSetting GetSendSetting(int serialId)
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
                                FROM [History_CaseSendSetting] AS S
                                WHERE S.[SerialID] = @SerialID";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("SerialID", serialId));
            var rtnList = SearchList<HistoryCaseSendSetting>(strSql);
            return rtnList.FirstOrDefault();
        }

        /// <summary>
        /// 通過CaseId取得該CaseId下所有發文明細
        /// </summary>
        /// <param name="caseId"></param>
        /// <returns></returns>
        public IList<HistoryCaseSendSettingDetails> GetSendSettingDetails(Guid caseId)
        {
            string strSql = @"SELECT  [DetailsId]
                                ,[CaseId]
                                ,[SerialID]
                                ,[SendType]
                                ,[GovName]
                                ,[GovAddr]
                            FROM [History_CaseSendSettingDetails]
                            WHERE [CaseId] = @CaseId";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("caseId", caseId));
            return SearchList<HistoryCaseSendSettingDetails>(strSql);
        }



        /// <summary>
        /// 同上功能, 提供給AutoPay用 
        /// </summary>
        /// <param name="model"></param>
        /// <param name="trans"></param>
        /// <returns></returns>
        public bool SaveCreate2(HistoryCaseSendSettingCreateViewModel model, IDbTransaction trans = null)
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
                rtn = rtn & InsertCaseSendSetting2(ref model, trans);
                if (model.ReceiveList != null && model.ReceiveList.Any())
                {
                    foreach (HistoryCaseSendSettingDetails item in model.ReceiveList.Where(item => !string.IsNullOrEmpty(item.GovAddr) && !string.IsNullOrEmpty(item.GovAddr)))
                    {
                        item.CaseId = model.CaseId;
                        item.SerialID = model.SerialId;
                        item.SendType = CaseSettingDetailType.Receive;  //*正本
                        rtn = rtn && InsertCaseSendSettingDetials(item, trans);
                    }
                }

                if (model.CcList != null && model.CcList.Any())
                {
                    foreach (HistoryCaseSendSettingDetails item in model.CcList.Where(item => !string.IsNullOrEmpty(item.GovAddr) && !string.IsNullOrEmpty(item.GovAddr)))
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

        public IList<HistoryCaseSendSettingDetails> GetSendSettingDetails(int serialId)
        {
            string strSql = @"SELECT  [DetailsId]
                                ,[CaseId]
                                ,[SerialID]
                                ,[SendType]
                                ,[GovName]
                                ,[GovAddr]
                            FROM [History_CaseSendSettingDetails]
                            WHERE [SerialID] = @SerialID";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("SerialID", serialId));
            return SearchList<HistoryCaseSendSettingDetails>(strSql);
        }
        public IList<HistoryCaseSendSettingDetails> GetSendSettingDetails(List<string> DetailIdList)
        {
            string strSql = @"SELECT  [DetailsId]
                                ,[CaseId]
                                ,[SerialID]
                                ,[SendType]
                                ,[GovName]
                                ,[GovAddr]
                            FROM [History_CaseSendSettingDetails]
                            WHERE 1=2 ";
            Parameter.Clear();
            for (int i = 0; i < DetailIdList.Count; i++)
            {
                strSql = strSql + " OR DetailsId = @DetailsId" + i + " ";
                Parameter.Add(new CommandParameter("@DetailsId" + i, DetailIdList[i].Split('|')[0]));
            }
            strSql = strSql + "order by GovName,GovAddr ";
            return SearchList<HistoryCaseSendSettingDetails>(strSql);
        }

        public IList<HistoryCaseSendSettingDetails> GetSendSettingDetailsByOrder(List<string> DetailIdList)
        {
            string strSql = @"select 0 as DetailsId,1 as No into #Map from History_CaseSendSettingDetails ";
            for (int i = 0; i < DetailIdList.Count; i++)
            {
                strSql = strSql + "insert #Map ( DetailsId,No) select DetailsId," + i + " from History_CaseSendSettingDetails where  DetailsId =" + DetailIdList[i].Split('|')[0] + " ";
            }
            strSql = strSql + @"SELECT  m.no,c.DetailsId
                                ,[CaseId]
                                ,[SerialID]
                                ,[SendType]
                                ,[GovName]
                                ,[GovAddr]
                            FROM [History_CaseSendSettingDetails] c
							join #Map m on c.DetailsId = m.DetailsId
                            WHERE 1=2 ";
            Parameter.Clear();
            for (int i = 0; i < DetailIdList.Count; i++)
            {
                strSql = strSql + " OR c.DetailsId = @DetailsId" + i + " ";
                Parameter.Add(new CommandParameter("@DetailsId" + i, DetailIdList[i].Split('|')[0]));
            }
            strSql = strSql + "order by m.No drop table #Map ";
            return SearchList<HistoryCaseSendSettingDetails>(strSql);
        }

        /// <summary>
        /// 儲存CaseSetting
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool SaveCreate(HistoryCaseSendSettingCreateViewModel model, IDbTransaction trans = null)
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
                    foreach (HistoryCaseSendSettingDetails item in model.ReceiveList.Where(item => !string.IsNullOrEmpty(item.GovAddr) && !string.IsNullOrEmpty(item.GovAddr)))
                    {
                        item.CaseId = model.CaseId;
                        item.SerialID = model.SerialId;
                        item.SendType = CaseSettingDetailType.Receive;  //*正本
                        rtn = rtn && InsertCaseSendSettingDetials(item, trans);
                    }
                }

                if (model.CcList != null && model.CcList.Any())
                {
                    foreach (HistoryCaseSendSettingDetails item in model.CcList.Where(item => !string.IsNullOrEmpty(item.GovAddr) && !string.IsNullOrEmpty(item.GovAddr)))
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
        public bool InsertCaseSendSetting(ref HistoryCaseSendSettingCreateViewModel model, IDbTransaction trans)
        {
            bool result = true;
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    HistoryCaseSendSettingBIZ cssBIZ = new HistoryCaseSendSettingBIZ();
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
                    SELECT TOP 1 @SendNoId=[SendNoId],@flag=[TimesFlag] FROM [History_SendNoTable] WHERE [SendNoYear] = @SendNoYear AND [SendNoNow] < [SendNoEnd] ORDER BY SendNoId;
                    UPDATE [History_SendNoTable] SET [SendNoNow] = [SendNoNow]+1 WHERE [SendNoId] = @SendNoId and [TimesFlag]=@flag;
                    INSERT INTO History_CaseSendSetting(CaseId,[Template],SendDate,SendWord,SendNo,Speed,Security,Subject,Description, Attachment,CreatedUser,CreatedDate,ModifiedUser,SendKind) 
                           VALUES (@CaseId,@Template,@SendDate,@SendWord,@SendNo1,@Speed,@Security,@Subject,@Description,@Attachment,@CreatedUser,GETDATE(),@ModifiedUser,@SendKind);
                    SELECT @@identity";
                    Parameter.Clear();
                    base.Parameter.Add(new CommandParameter("@CaseId", model.CaseId));
                    base.Parameter.Add(new CommandParameter("@Template", model.Template));
                    base.Parameter.Add(new CommandParameter("@SendDate", model.SendDate));
                    base.Parameter.Add(new CommandParameter("@SendWord", model.SendWord));
                    //adam 又改回取號
                    //if (model.flag == "AgentAccountInfo")//帳務資訊儲存時不產生發文字號，呈核時才產生
                    //{ 
                    //    model.SendNo = ""; 
                    //}
                    //else//發文資訊儲存時需要產生發文字號
                    {
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

        /// <summary>
        /// 同上一個, 差別在由AutoPay.exe來呼叫
        /// </summary>
        /// <param name="model"></param>
        /// <param name="trans"></param>
        /// <returns></returns>
        public bool InsertCaseSendSetting2(ref HistoryCaseSendSettingCreateViewModel model, IDbTransaction trans)
        {
            bool result = true;
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    HistoryCaseSendSettingBIZ cssBIZ = new HistoryCaseSendSettingBIZ();
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
                    SELECT TOP 1 @SendNoId=[SendNoId],@flag=[TimesFlag] FROM [History_SendNoTable] WHERE [SendNoYear] = @SendNoYear AND [SendNoNow] < [SendNoEnd] ORDER BY SendNoId;
                    UPDATE [History_SendNoTable] SET [SendNoNow] = [SendNoNow]+1 WHERE [SendNoId] = @SendNoId and [TimesFlag]=@flag;
                    INSERT INTO History_CaseSendSetting(CaseId,[Template],SendDate,SendWord,SendNo,Speed,Security,Subject,Description, Attachment,CreatedUser,CreatedDate,ModifiedUser,SendKind) 
                           VALUES (@CaseId,@Template,@SendDate,@SendWord,@SendNo1,@Speed,@Security,@Subject,@Description,@Attachment,@CreatedUser,GETDATE(),@ModifiedUser,@SendKind);
                    SELECT @@identity";
                    Parameter.Clear();
                    base.Parameter.Add(new CommandParameter("@CaseId", model.CaseId));
                    base.Parameter.Add(new CommandParameter("@Template", model.Template));
                    base.Parameter.Add(new CommandParameter("@SendDate", model.SendDate));
                    base.Parameter.Add(new CommandParameter("@SendWord", model.SendWord));
                    //adam 又改回取號
                    //if (model.flag == "AgentAccountInfo")//帳務資訊儲存時不產生發文字號，呈核時才產生
                    //{ 
                    //    model.SendNo = ""; 
                    //}
                    //else//發文資訊儲存時需要產生發文字號
                    {
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
                    }
                    base.Parameter.Add(new CommandParameter("@SendNo1", model.SendNo));
                    base.Parameter.Add(new CommandParameter("@Speed", model.Speed));
                    base.Parameter.Add(new CommandParameter("@Security", model.Security));
                    base.Parameter.Add(new CommandParameter("@Subject", model.Subject));
                    base.Parameter.Add(new CommandParameter("@Description", model.Description));
                    base.Parameter.Add(new CommandParameter("@Attachment", model.Attachment));
                    base.Parameter.Add(new CommandParameter("@CreatedUser", model.CreatedUser));
                    base.Parameter.Add(new CommandParameter("@ModifiedUser", model.ModifiedUser));
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

        public bool InsertCaseSendSettingDetials(HistoryCaseSendSettingDetails model, IDbTransaction trans)
        {
            string strSql = @"insert into History_CaseSendSettingDetails([CaseId],[SerialID],[SendType],[GovName],[GovAddr])
                                    values(@CaseId, @SerialID, @SendType, @GovName,@GovAddr)";
            Parameter.Clear(); ;
            base.Parameter.Add(new CommandParameter("@CaseId", model.CaseId));
            base.Parameter.Add(new CommandParameter("@SerialID", model.SerialID));
            base.Parameter.Add(new CommandParameter("@SendType", model.SendType));
            base.Parameter.Add(new CommandParameter("@GovName", model.GovName));
            base.Parameter.Add(new CommandParameter("@GovAddr", model.GovAddr));
            return trans == null ? ExecuteNonQuery(strSql) > 0 : ExecuteNonQuery(strSql, trans) > 0;
        }
        public bool SaveEdit(HistoryCaseSendSettingCreateViewModel model)
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
                        foreach (HistoryCaseSendSettingDetails item in model.ReceiveList.Where(item => !string.IsNullOrEmpty(item.GovAddr) && !string.IsNullOrEmpty(item.GovAddr)))
                        {
                            item.CaseId = model.CaseId;
                            item.SerialID = model.SerialId;
                            item.SendType = CaseSettingDetailType.Receive;  //*正本
                            rtn = rtn && InsertCaseSendSettingDetials(item, dbTransaction);
                        }
                    }

                    if (model.CcList != null && model.CcList.Any())
                    {
                        foreach (HistoryCaseSendSettingDetails item in model.CcList.Where(item => !string.IsNullOrEmpty(item.GovAddr) && !string.IsNullOrEmpty(item.GovAddr)))
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
        public bool UpdateCaseSendSetting(HistoryCaseSendSettingCreateViewModel model, IDbTransaction trans = null)
        {
            string sql = @"UPDATE [History_CaseSendSetting]
                           SET [Template] = @Template
                              ,[SendDate] = @SendDate
                              ,[Speed] = @Speed
                              ,[Security] = @Security
                              ,[Subject] = @Subject
                              ,[Description] = @Description
                              ,[Attachment] = @Attachment
                              ,[ModifiedUser] = @ModifiedUser
                              ,[ModifiedDate] = GETDATE()
                              ,[SendNo] = @SendNo
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
            string sql = @"DELETE FROM [History_CaseSendSettingDetails] WHERE [SerialID] = @SerialID";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("SerialID", serialId));
            return trans == null ? ExecuteNonQuery(sql) > 0 : ExecuteNonQuery(sql, trans) > 0;
        }

        public bool DeleteCaseSendSetting(int serialId, IDbTransaction trans = null)
        {
            string sql = @"DELETE FROM History_CaseSendSetting WHERE [SerialID] = @SerialID";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("SerialID", serialId));
            return trans == null ? ExecuteNonQuery(sql) > 0 : ExecuteNonQuery(sql, trans) > 0;
        }

        public bool DeleteCaseSendSetting(Guid caseId, IDbTransaction dbtrans = null)
        {
            string strSql = "DELETE History_CaseSendSetting WHERE  CaseId = @CaseId ";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("CaseId", caseId));
            return dbtrans == null ? ExecuteNonQuery(strSql) > 0 : ExecuteNonQuery(strSql, dbtrans) > 0;
        }

        public bool DeleteCaseSendSettingDetails(Guid caseId, IDbTransaction dbtrans = null)
        {
            string strSql = "DELETE History_CaseSendSettingDetails WHERE  CaseId = @CaseId ";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("CaseId", caseId));
            return dbtrans == null ? ExecuteNonQuery(strSql) > 0 : ExecuteNonQuery(strSql, dbtrans) > 0;
        }

        public int Delete(int serialId)
        {

            string strSql = @"UPDATE [History_CasePayeeSetting] SET [SendId] = NULL WHERE [SendId] = @SerialID;                            
                            DELETE FROM History_CaseSendSetting where SerialID=@SerialID";

            Parameter.Clear();
            Parameter.Add(new CommandParameter("@SerialID", serialId));

            return ExecuteNonQuery(strSql) > 0 ? 1 : 0;
        }
        public HistoryCaseSendSettingCreateViewModel GetCaseSettingAndDetails(int serialId)
        {
            HistoryCaseSendSetting main = GetSendSetting(serialId);
            HistoryCaseSendSettingCreateViewModel rtn = new HistoryCaseSendSettingCreateViewModel
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
                ReceiveList = new List<HistoryCaseSendSettingDetails>(),
                CcList = new List<HistoryCaseSendSettingDetails>()
            };

            IList<HistoryCaseSendSettingDetails> list = GetSendSettingDetails(serialId);
            if (list != null && list.Any())
            {
                foreach (HistoryCaseSendSettingDetails details in list)
                {
                    if (details.SendType == CaseSettingDetailType.Receive)
                        rtn.ReceiveList.Add(details);
                    if (details.SendType == CaseSettingDetailType.Cc)
                        rtn.CcList.Add(details);
                }
            }
            return rtn;
        }
        public DataTable GetSendSettingByCaseIdListWithType(string type,List<string> caseIdList)
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
                                FROM [History_CaseSendSetting] AS M with(Nolock)
                                LEFT OUTER JOIN [History_CaseSendSettingDetails] AS D ON M.CaseId = D.CaseId AND M.SerialID =D.SerialID
                                WHERE 1=2 ";
            Parameter.Clear();
            for (int i = 0; i < caseIdList.Count; i++)
            {
                strsql = strsql + " OR ( M.[CaseId] = @CaseId" + i + " and sendtype = "+type+ ") ";
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
                string sqlRecive = "SELECT GovName,SerialID FROM History_CaseSendSettingDetails WHERE SendType=1 and SerialID In (" + strSerialId + ")";
                string sqlCc = "SELECT GovName,SerialID FROM History_CaseSendSettingDetails WHERE SendType=2 and SerialID In (" + strSerialId + ")";
                List<HistoryCaseSendSettingDetails> listRecive = base.SearchList<HistoryCaseSendSettingDetails>(sqlRecive).ToList();
                List<HistoryCaseSendSettingDetails> listCc = base.SearchList<HistoryCaseSendSettingDetails>(sqlCc).ToList();
                if (listRecive != null && listRecive.Any())
                {
                    foreach (DataRow dr in Dt.Rows)
                    {
                        string strRecive = string.Empty;
                        foreach (HistoryCaseSendSettingDetails item in listRecive.Where(m => m.SerialID == Convert.ToInt32(dr["SerialID"])))
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
                        foreach (HistoryCaseSendSettingDetails item in listCc.Where(m => m.SerialID == Convert.ToInt32(dr["SerialID"])))
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
                                FROM [History_CaseSendSetting] AS M with(Nolock)
                                LEFT OUTER JOIN [History_CaseSendSettingDetails] AS D ON M.CaseId = D.CaseId AND M.SerialID =D.SerialID
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
                                FROM [History_CaseSendSetting] AS M
                                LEFT OUTER JOIN [History_CaseSendSettingDetails] AS D ON M.CaseId = D.CaseId AND M.SerialID =D.SerialID
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

        public string UpdateSendNo(IDbTransaction trans = null)
        {
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
                string sqlStr = @"select Top 1 (SendNoNow+1) as SendNoNow from SendNoTable where SendNoYear=@SendNoYear and SendNoNow<SendNoEnd";
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@SendNoYear", DateTime.Now.ToString("yyyy")));
                string result = Convert.ToString(base.ExecuteNonQuery(sqlStr));
                if (needSubmit)
                {
                    if (rtn)
                        trans.Commit();
                    else
                        trans.Rollback();
                }

                //return rtn;
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
        public HistoryCaseSendSettingCreateViewModel GetDefaultSendSetting(HistoryCasePayeeSetting model, out string errMsg, IDbTransaction trans = null, string _Account = null)
        {
            if (model == null || string.IsNullOrEmpty(model.CheckNo))
            {
                //* 理論上不會call到這裡.因為在外面就檢查了.這裡寫就是防呆
                errMsg = Lang.csfs_text_notnull;
                return null;
            }



            HistoryCaseSendSettingBIZ cssBiz = new HistoryCaseSendSettingBIZ();
            string sendNo = cssBiz.SendNo();
            if (string.IsNullOrEmpty(sendNo))
            {
                errMsg = Lang.csfs_sendno;
                return null;
            }
            HistoryCaseMasterBIZ masterBiz = new HistoryCaseMasterBIZ();
            HistorySendSettingRefBiz refbiz = new HistorySendSettingRefBiz();
            HistorySendSettingRef basic = refbiz.GetSubjectAndDescription(model.CaseId, CaseKind2.CasePay, CaseKind2.CaseSeizureEdoc, trans, model);
            HistoryCaseMaster master = masterBiz.MasterModel(model.CaseId, trans);

            if (string.IsNullOrEmpty(_Account))
            {
                HistoryCaseSendSettingCreateViewModel send = new HistoryCaseSendSettingCreateViewModel
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
                    ReceiveList = new List<HistoryCaseSendSettingDetails>(),
                    CcList = new List<HistoryCaseSendSettingDetails>()
                };
                //send.ReceiveList.Add(new CaseSendSettingDetails { CaseId = model.CaseId, GovAddr = model.Address, GovName = model.Receiver, SendType = 1 });
                //send.CcList.Add(new CaseSendSettingDetails { CaseId = model.CaseId, GovAddr = model.CCReceiver, GovName = model.Currency, SendType = 2 });
                //errMsg = "";
                //return send;
                // 20200804 Partrik
                send.ReceiveList.Add(new HistoryCaseSendSettingDetails { CaseId = model.CaseId, GovAddr = model.Address, GovName = model.Receiver, SendType = 1 });
                if (model.Receiver != model.Currency) // 20200803, 若正本=副本機關, 則不秀副本, CASE 5, CASE 27
                    send.CcList.Add(new HistoryCaseSendSettingDetails { CaseId = model.CaseId, GovAddr = model.CCReceiver, GovName = model.Currency, SendType = 2 });
                errMsg = "";
                return send;
            }
            else // 20200624, 原因是用Console程式呼叫, 無法由CommBiz, 取得HttpContext.Current.Session["LogonUser"], 是誰來產生, 所以由外部傳進來
            {
                HistoryCaseSendSettingCreateViewModel send = new HistoryCaseSendSettingCreateViewModel
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
                    CreatedUser = _Account,
                    CreatedDate = DateTime.Now,
                    ModifiedUser = _Account,
                    ModifiedDate = DateTime.Now,
                    ReceiveList = new List<HistoryCaseSendSettingDetails>(),
                    CcList = new List<HistoryCaseSendSettingDetails>()
                };
                //send.ReceiveList.Add(new CaseSendSettingDetails { CaseId = model.CaseId, GovAddr = model.Address, GovName = model.Receiver, SendType = 1 });
                //send.CcList.Add(new CaseSendSettingDetails { CaseId = model.CaseId, GovAddr = model.CCReceiver, GovName = model.Currency, SendType = 2 });
                //errMsg = "";
                // 20200804 partrik
                send.ReceiveList.Add(new HistoryCaseSendSettingDetails { CaseId = model.CaseId, GovAddr = model.Address, GovName = model.Receiver, SendType = 1 });
                if (model.Receiver != model.Currency) // 20200803, 若正本=副本機關, 則不秀副本 , CASE 5, CASE 27
                    send.CcList.Add(new HistoryCaseSendSettingDetails { CaseId = model.CaseId, GovAddr = model.CCReceiver, GovName = model.Currency, SendType = 2 });
                errMsg = "";
                return send;
            }


        }

    }
}
