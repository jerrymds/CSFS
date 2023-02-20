using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CTBC.CSFS.Models;
using CTBC.CSFS.Pattern;

namespace CTBC.CSFS.BussinessLogic
{
    public class CaseMeetBiz : CommonBIZ
    {

        /// <summary>
        /// 通過CaseId取得該案件下的會辦資訊主表及從表
        /// </summary>
        /// <param name="caseId"></param>
        /// <returns></returns>
        public CaseMeetMaster GetCaseMeetMaster(Guid caseId)
        {
            //* 首先查詢下面的sql得到meet主表
            string strSql = "SELECT [MeetId],[CaseId],[StandardDateS],[StandardDateE],[BranchPaySave],[BranchVip],[BranchViptext], " +
                            "[RmNotice],[RmNoticeAndConfirm],[MeetMemo],[CreatedUser],[CreatedDate],[ModifiedUser],[ModifiedDate] " +
                            "FROM [CaseMeetMaster] WHERE [CaseId] = @CaseId";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("@CaseId", caseId));
            IList<CaseMeetMaster> listMaster = SearchList<CaseMeetMaster>(strSql);
            if (listMaster != null && listMaster.Any())
            {
                //* 說明有主表.是儲存過的,自動帶出從表
                CaseMeetMaster master = listMaster[0];
                master.ListDetails = GetCaseMeetDetails(caseId);
                return master;
            }
            return null;
        }
        /// <summary>
        /// 通過CaseId取得該案件下會辦從表資料
        /// </summary>
        /// <param name="caseId"></param>
        /// <returns></returns>
        public List<CaseMeetDetails> GetCaseMeetDetails(Guid caseId)
        {
            string strSql = "SELECT [MeetDetailId],[CaseId],[MeetKind],[MeetUnit],[SortOrder],[IsSelected],[Result] " +
                            "FROM [CaseMeetDetails] WHERE [CaseId] = @CaseId ORDER BY [SortOrder] ASC";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("@CaseId", caseId));
            IList<CaseMeetDetails> listDetails = SearchList<CaseMeetDetails>(strSql);
            if (listDetails != null && listDetails.Any())
            {
                return listDetails.ToList();
            }
            return null;
        }

        /// <summary>
        /// 儲存會辦資料
        /// </summary>
        /// <param name="master"></param>
        /// <returns></returns>
        public bool SaveCaseMeetInfo(CaseMeetMaster master)
        {
            //* 參數錯誤
            if (master == null || master.CaseId == null || master.CaseId == Guid.Empty)
                return false;

            bool rtn = true;
            //* todo:這裡只是思路沒有實際實現
            IDbConnection dbConnection = OpenConnection();
            IDbTransaction dbTransaction = null;
            try
            {
                using (dbConnection)
                {
                    dbTransaction = dbConnection.BeginTransaction();
                    if (IsCaseMeetMasterExist(master.CaseId, dbTransaction))
                    {
                        //* 存在.Update
                        rtn = rtn && UpdateCaseMeetMaster(master);
                    }
                    else
                    {
                        //* 不存在新增
                        rtn = rtn && InsertCaseMeetMaster(master);

                    }
                    rtn = rtn && SaveCaseMeetDetails(master.ListDetails, dbTransaction);
                    //* 全部都是true就提交事務
                    if (rtn)
                        dbTransaction.Commit();
                    else
                        dbTransaction.Rollback();
                    return rtn;
                }
            }
            catch (Exception ex)
            {
                try
                {
                    if (dbTransaction != null)
                        dbTransaction.Rollback();
                }
                catch (Exception ex2)
                {
                    //* 捕捉關閉事務異常
                }
                throw ex;
            }
        }

        #region master
        /// <summary>
        /// 檢查這個CaseId下的Master是否存在
        /// </summary>
        /// <param name="caseId"></param>
        /// <param name="trans"></param>
        /// <returns></returns>
        public bool IsCaseMeetMasterExist(Guid caseId, IDbTransaction trans = null)
        {
            string strSql = @"select count(*) from CaseMeetMaster where CaseId=@CaseId";
            base.Parameter.Clear();
            base.Parameter.Add(new CommandParameter("@CaseId", caseId));
            int obj = Convert.ToInt32(base.ExecuteScalar(strSql));
            if (obj > 0)
            {
                return true;
            }
            else
            {
                return false;
            }

        }
        /// <summary>
        /// 新增
        /// </summary>
        /// <param name="master"></param>
        /// <param name="trans"></param>
        /// <returns></returns>
        public bool InsertCaseMeetMaster(CaseMeetMaster master, IDbTransaction trans = null)
        {
            string strSql = @"INSERT INTO CaseMeetMaster(CaseId,StandardDateS,StandardDateE,BranchPaySave,BranchVip,BranchViptext,RmNotice,RmNoticeAndConfirm,MeetMemo,CreatedUser,CreatedDate,ModifiedUser,ModifiedDate) 
                VALUES(@CaseId,@StandardDateS,@StandardDateE,@BranchPaySave,@BranchVip,@BranchViptext,@RmNotice,@RmNoticeAndConfirm,@MeetMemo,@CreatedUser,GETDATE(),@CreatedUser,GETDATE())";
            base.Parameter.Clear();
            base.Parameter.Add(new CommandParameter("@CaseId", master.CaseId));
            base.Parameter.Add(new CommandParameter("@StandardDateS", master.StandardDateS));
            base.Parameter.Add(new CommandParameter("@StandardDateE", master.StandardDateE));
            base.Parameter.Add(new CommandParameter("@BranchPaySave", master.BranchPaySave));
            base.Parameter.Add(new CommandParameter("@BranchVip", master.BranchVip));
            base.Parameter.Add(new CommandParameter("@BranchViptext", master.BranchViptext));
            base.Parameter.Add(new CommandParameter("@RmNotice", master.RmNotice));
            base.Parameter.Add(new CommandParameter("@RmNoticeAndConfirm", master.RmNoticeAndConfirm));
            base.Parameter.Add(new CommandParameter("@MeetMemo", master.MeetMemo));
            base.Parameter.Add(new CommandParameter("@CreatedUser", master.CreatedUser));
            return trans == null ? ExecuteNonQuery(strSql) > 0 : ExecuteNonQuery(strSql, trans) > 0;
        }
        /// <summary>
        /// 修改
        /// </summary>
        /// <param name="master"></param>
        /// <param name="trans"></param>
        /// <returns></returns>
        public bool UpdateCaseMeetMaster(CaseMeetMaster master, IDbTransaction trans = null)
        {
            string strSql = @"UPDATE CaseMeetMaster SET StandardDateS=@StandardDateS,StandardDateE=@StandardDateE,BranchPaySave=@BranchPaySave,BranchVip=@BranchVip,BranchViptext=@BranchViptext,RmNotice=@RmNotice,RmNoticeAndConfirm=@RmNoticeAndConfirm,MeetMemo=@MeetMemo,ModifiedUser=@ModifiedUser,ModifiedDate=GETDATE() WHERE CaseId=@CaseId";
            base.Parameter.Clear();
            base.Parameter.Add(new CommandParameter("@CaseId", master.CaseId));
            base.Parameter.Add(new CommandParameter("@StandardDateS", master.StandardDateS));
            base.Parameter.Add(new CommandParameter("@StandardDateE", master.StandardDateE));
            base.Parameter.Add(new CommandParameter("@BranchPaySave", master.BranchPaySave));
            base.Parameter.Add(new CommandParameter("@BranchVip", master.BranchVip));
            base.Parameter.Add(new CommandParameter("@BranchViptext", master.BranchViptext));
            base.Parameter.Add(new CommandParameter("@RmNotice", master.RmNotice));
            base.Parameter.Add(new CommandParameter("@RmNoticeAndConfirm", master.RmNoticeAndConfirm));
            base.Parameter.Add(new CommandParameter("@MeetMemo", master.MeetMemo));
            base.Parameter.Add(new CommandParameter("@ModifiedUser", master.ModifiedUser));
            return trans == null ? ExecuteNonQuery(strSql) > 0 : ExecuteNonQuery(strSql, trans) > 0;
        }
        #endregion

        #region details
        /// <summary>
        /// 儲存明細
        /// </summary>
        /// <param name="details"></param>
        /// <param name="trans"></param>
        /// <returns></returns>
        public bool SaveCaseMeetDetails(List<CaseMeetDetails> details, IDbTransaction trans = null)
        {
            if (details == null || !details.Any() || details[0].CaseId == Guid.Empty)
                return false;
            bool rtn = true;
            //* 先Delete caseid下所有的,再Insert
            if (IsCaseCaseMeetDetailsExist(details[0].CaseId))
            {
                rtn = rtn && DeleteCaseMeetDetails(details[0].CaseId, trans);
                foreach (CaseMeetDetails item in details)
                {

                    rtn = rtn && InsertCaseMeetDetails(item, trans);
                }
            }
            else
            {
                foreach (CaseMeetDetails item in details)
                {

                    rtn = rtn && InsertCaseMeetDetails(item, trans);
                }
            }
            
            return rtn;
        }
        public bool IsCaseCaseMeetDetailsExist(Guid caseId)
        {
            string strSql = @"select count(*) from CaseMeetDetails where CaseId=@CaseId";
            base.Parameter.Clear();
            base.Parameter.Add(new CommandParameter("@CaseId", caseId));
            int obj = Convert.ToInt32(base.ExecuteScalar(strSql));
            if (obj > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// 刪除
        /// </summary>
        /// <param name="caseId"></param>
        /// <param name="trans"></param>
        /// <returns></returns>
        public bool DeleteCaseMeetDetails(Guid caseId, IDbTransaction trans = null)
        {
            string strSql = @"DELETE CaseMeetDetails WHERE CaseId=@CaseId";
            base.Parameter.Clear();
            base.Parameter.Add(new CommandParameter("@CaseId", caseId));
            return trans == null ? ExecuteNonQuery(strSql) > 0 : ExecuteNonQuery(strSql, trans) > 0;
        }

        /// <summary>
        /// 刪除
        /// </summary>
        /// <param name="caseId"></param>
        /// <param name="trans"></param>
        /// <returns></returns>
        public bool DeleteCaseMeetMaster(Guid caseId, IDbTransaction trans = null)
        {
            string strSql = @"DELETE CaseMeetMaster WHERE CaseId=@CaseId";
            base.Parameter.Clear();
            base.Parameter.Add(new CommandParameter("@CaseId", caseId));
            return trans == null ? ExecuteNonQuery(strSql) > 0 : ExecuteNonQuery(strSql, trans) > 0;
        }
        /// <summary>
        /// 新增
        /// </summary>
        /// <param name="details"></param>
        /// <param name="trans"></param>
        /// <returns></returns>
        public bool InsertCaseMeetDetails(CaseMeetDetails details, IDbTransaction trans = null)
        {
            string strSql = @"INSERT INTO CaseMeetDetails(CaseId,MeetKind,MeetUnit,SortOrder,IsSelected,Result) 
                              VALUES(@CaseId,@MeetKind,@MeetUnit,@SortOrder,@IsSelected,@Result)";
            base.Parameter.Clear();
            base.Parameter.Add(new CommandParameter("@CaseId", details.CaseId));
            base.Parameter.Add(new CommandParameter("@MeetKind", details.MeetKind));
            base.Parameter.Add(new CommandParameter("@MeetUnit", details.MeetUnit));
            base.Parameter.Add(new CommandParameter("@SortOrder", details.SortOrder));
            base.Parameter.Add(new CommandParameter("@IsSelected", details.IsSelected));
            base.Parameter.Add(new CommandParameter("@Result", details.Result));
            return trans == null ? ExecuteNonQuery(strSql) > 0 : ExecuteNonQuery(strSql, trans) > 0;
        }
        #endregion
    }
}
