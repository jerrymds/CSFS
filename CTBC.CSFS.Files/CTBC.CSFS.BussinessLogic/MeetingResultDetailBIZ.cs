using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CTBC.CSFS.Models;
using CTBC.CSFS.Pattern;

namespace CTBC.CSFS.BussinessLogic
{
    public class MeetingResultDetailBIZ : CommonBIZ
    {
        /// <summary>
        /// 新增一筆附件
        /// </summary>
        /// <param name="model">model</param>
        /// <param name="trans">事務</param>
        /// <returns></returns>
        public int Create(MeetingResultDetail model, IDbTransaction trans = null)
        {
            string sql = @" insert into MeetingResultDetail  
                                    (ResultId,AttatchDetailName,AttatchDetailServerPath,AttatchDetailServerName,isDelete,CreatedUser,CreatedDate) 
                                        values 
                                    (@ResultId,@AttatchDetailName,@AttatchDetailServerPath,@AttatchDetailServerName,@isDelete,@CreatedUser,@CreatedDate);";

            Parameter.Clear();

            // 添加參數
            Parameter.Add(new CommandParameter("@ResultId", model.ResultId));
            Parameter.Add(new CommandParameter("@AttatchDetailName", model.AttatchDetailName));
            Parameter.Add(new CommandParameter("@AttatchDetailServerPath", model.AttatchDetailServerPath));
            Parameter.Add(new CommandParameter("@AttatchDetailServerName", model.AttatchDetailServerName));
            Parameter.Add(new CommandParameter("@isDelete", model.isDelete));
            Parameter.Add(new CommandParameter("@CreatedUser", model.CreatedUser));
            base.Parameter.Add(new CommandParameter("@CreatedDate", model.CreatedDate));

            if (trans != null)
            {
                // 執行新增返回是否成功
                return ExecuteNonQuery(sql, trans);
            }
            IDbConnection dbConnection = OpenConnection();
            IDbTransaction tans = dbConnection.BeginTransaction();
            return ExecuteNonQuery(sql, tans);
        }

        /// <summary>
        /// 得到附件list
        /// </summary>
        /// <param name="resultId">主表的id</param>
        /// <returns></returns>
        public List<MeetingResultDetail> GetMeetingResultDetailInfo(int resultId)
        {
            string sql = @"select d.AttatchDetailId,d.ResultId,d.AttatchDetailName,
                                d.AttatchDetailServerName,d.AttatchDetailServerPath,d.CreatedUser,Convert(nvarchar(10), d.CreatedDate,111) as CreatedDate from [dbo].[MeetingResultDetail] d
                                where d.ResultId=@ResultId";
            base.Parameter.Clear();
            base.Parameter.Add(new CommandParameter("@ResultId", resultId));
            IList<MeetingResultDetail> _list = base.SearchList<MeetingResultDetail>(sql);
            List<MeetingResultDetail> list = new List<MeetingResultDetail>();
            foreach (var item in _list)
            {
                list.Add(item);
            }
            return list;         
        }

        /// <summary>
        /// 刪除一筆附件
        /// </summary>
        /// <param name="attachid">附件id</param>
        /// <returns></returns>
        public int DeleteAttatch(string attachid)
        {
            string strSql = " delete from MeetingResultDetail where AttatchDetailId=@AttatchDetailId ";
            base.Parameter.Clear();

            // 添加參數
            base.Parameter.Add(new CommandParameter("@AttatchDetailId", attachid));

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

        public MeetingResultDetail GetAttachDetailInfo(int attachId)
        {
            string strSql = @"SELECT [ResultId],[AttatchDetailName],[AttatchDetailServerPath],[AttatchDetailServerName],[isDelete]
                       ,[CreatedUser],[CreatedDate] FROM [MeetingResultDetail] WHERE [AttatchDetailId]=@AttatchDetailId";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("@AttatchDetailId", attachId));
            IList<MeetingResultDetail> list = SearchList<MeetingResultDetail>(strSql);
            return list.FirstOrDefault();
        }
    }
}
