using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CTBC.CSFS.ViewModels;
using CTBC.CSFS.Models;
using CTBC.CSFS.Pattern;

namespace CTBC.CSFS.BussinessLogic
{
    public class MeetingResultBIZ : CommonBIZ
    {
        public bool Create(MeetingResultViewModel model)
        {

            IDbConnection dbConnection = OpenConnection();
            IDbTransaction dbTransaction = null;
            try
            {
                using (dbConnection)
                {
                    dbTransaction = dbConnection.BeginTransaction();
                    string resultId = "";
                    #region 主表
                    MeetingResultBIZ meetResult = new MeetingResultBIZ();
                    MeetingResultDetailBIZ meetDetail = new MeetingResultDetailBIZ();

                    if (meetResult.IsDateExist(model.MeetingResult.ResultDate) == "1")//已存在數據 進行編輯
                    {
                        meetResult.Edit(model.MeetingResult, dbTransaction);
                        #region 附件
                        foreach (MeetingResultDetail attach in model.MeetingResultDetailList)
                        {
                            attach.ResultId = model.MeetingResult.ResultId;
                            attach.CreatedUser = model.MeetingResult.CreatedUser;
                            attach.CreatedDate = DateTime.Now.ToString("yyyy/MM/dd");
                            meetDetail.Create(attach, dbTransaction);
                        }
                        #endregion
                    }
                    else
                    {
                        meetResult.Create(model.MeetingResult, ref resultId, dbTransaction);//不存在，進行新增
                        #region 附件 
                        foreach (MeetingResultDetail attach in model.MeetingResultDetailList)
                        {
                            attach.ResultId = Convert.ToInt32(resultId);
                            attach.CreatedUser = model.MeetingResult.CreatedUser;
                            attach.CreatedDate = DateTime.Now.ToString("yyyy/MM/dd");
                            meetDetail.Create(attach, dbTransaction);
                        }
                        #endregion
                    }
                    #endregion
                    dbTransaction.Commit();
                }
                return true;
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

        /// <summary>
        /// 實際新增 MeetingResult
        /// </summary>
        /// <param name="model"></param>
        /// <param name="trans"></param>
        /// <returns></returns>
        public int Create(MeetingResult model, ref string resultId, IDbTransaction trans = null)
        {
            string sql = @" insert into MeetingResult  (ResultDate,ResultStatus,ResultCompleteDate,CreatedUser,CreatedDate) 
                                        values (@ResultDate,@ResultStatus,@ResultCompleteDate,@CreatedUser,@CreatedDate);
                                        select @@identity";

            Parameter.Clear();

            // 添加參數
            Parameter.Add(new CommandParameter("@ResultDate", model.ResultDate));
            Parameter.Add(new CommandParameter("@ResultStatus", model.ResultStatus));
            Parameter.Add(new CommandParameter("@ResultCompleteDate", model.ResultCompleteDate));
            Parameter.Add(new CommandParameter("@CreatedUser", model.CreatedUser));
            base.Parameter.Add(new CommandParameter("@CreatedDate", model.CreatedDate));
            resultId = trans == null ? Convert.ToString(ExecuteScalar(sql)) : Convert.ToString(ExecuteScalar(sql, trans));
            return 1;
        }

        public int Edit(MeetingResult model, IDbTransaction trans = null)
        {
            string sql = @" update  MeetingResult  set ResultStatus=@ResultStatus,ResultCompleteDate=@ResultCompleteDate,
                                    ModifiedUser=@ModifiedUser,ModifiedDate=@ModifiedDate where ResultId=@ResultId";

            Parameter.Clear();

            // 添加參數
            Parameter.Add(new CommandParameter("@ResultStatus", model.ResultStatus));
            Parameter.Add(new CommandParameter("@ResultCompleteDate", model.ResultCompleteDate));
            Parameter.Add(new CommandParameter("@ModifiedUser", model.ModifiedUser));
            base.Parameter.Add(new CommandParameter("@ModifiedDate", model.ModifiedDate));
            Parameter.Add(new CommandParameter("@ResultId", model.ResultId));
            return trans == null ? ExecuteNonQuery(sql) : ExecuteNonQuery(sql, trans);
        }

        public MeetingResult GetMeetingResultInfo(string date)
        {
            string sql = @"select r.ResultId,r.ResultDate,r.ResultStatus,r.ResultCompleteDate from  [dbo].[MeetingResult] r 
                                   where ResultDate =@ResultDate";
            Parameter.Clear();

            // 添加參數
            Parameter.Add(new CommandParameter("@ResultDate", date));
            IList<MeetingResult> _list = base.SearchList<MeetingResult>(sql);
            if (_list != null && _list.Count > 0) return _list[0];
            else return new MeetingResult();
        }

        /// <summary>
        /// 根據時間判斷當天是否存在數據
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public string IsDateExist(string date)
        {
            string strSql = "select count(0) from MeetingResult where  ResultDate=@ResultDate";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("@ResultDate", date));
            try
            {
                int a = (int)ExecuteScalar(strSql);
                if (a > 0)
                {
                    return "1";//有重複
                }
                return "0";//無重複
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

        }
    }
}
