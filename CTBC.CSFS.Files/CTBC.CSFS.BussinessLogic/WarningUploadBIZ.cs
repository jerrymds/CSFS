using CTBC.CSFS.Models;
using CTBC.CSFS.Pattern;
using CTBC.CSFS.ViewModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTBC.CSFS.BussinessLogic
{
    public class WarningUploadBIZ : CommonBIZ
    {
        public bool SaveAttatchment(ref CaseWarningViewModel model)
        {
            IDbConnection dbConnection = OpenConnection();
            IDbTransaction dbTransaction = null;
            try
            {
                using (dbConnection)
                {
                    dbTransaction = dbConnection.BeginTransaction();
                    foreach (WarningAttachment attach in model.WarningAttachmentList)
                    {
                        attach.AttachmentName = "B" + attach.AttachmentName;
                        string DocNo = SelectDocNo(attach, dbTransaction);
                        if (DocNo != "")
                        {
                            int rtn = SelectAttatchment(attach, DocNo, dbTransaction);
                            if (rtn == 0)
                            {
                                CreateAttatchment(attach, DocNo, dbTransaction);
                            }
                            else
                            {
                                EditAttatchment(attach, DocNo, dbTransaction);
                            }
                        }
                        else {
                            return false;
                        }
                    }
                    dbTransaction.Commit();
                }
                return true;
            }
            catch (Exception ex)
            {
                try
                {
                    if (dbTransaction != null) dbTransaction.Rollback();
                }
                catch (Exception)
                {
                }
                throw ex;
                return false;
            }
        }

        #region 新增一筆附件
        public int CreateAttatchment(WarningAttachment model,string DocNo,IDbTransaction trans = null)
        {
            string strSql = @" insert into WarningAttachment (DocNo,AttachmentName,AttachmentServerPath,AttachmentServerName,CreatedUser,CreatedDate) 
                                        values (
                                        @DocNo,@AttachmentName,@AttachmentServerPath,@AttachmentServerName,@CreatedUser,GETDATE());";

            base.Parameter.Clear();

            // 添加參數
            base.Parameter.Add(new CommandParameter("@DocNo", DocNo));
            base.Parameter.Add(new CommandParameter("@AttachmentName", model.AttachmentName));
            base.Parameter.Add(new CommandParameter("@AttachmentServerPath", model.AttachmentServerPath));
            base.Parameter.Add(new CommandParameter("@AttachmentServerName", model.AttachmentServerName));
            base.Parameter.Add(new CommandParameter("@CreatedUser", model.CreatedUser));
            return trans == null ? base.ExecuteNonQuery(strSql) : base.ExecuteNonQuery(strSql, trans);
        }
        #endregion

        #region 修改一筆附件
        public int EditAttatchment(WarningAttachment model,string DocNo, IDbTransaction trans = null)
        {
            string strSql = @" update WarningAttachment set AttachmentName=@AttachmentName,AttachmentServerPath=@AttachmentServerPath,
                               AttachmentServerName=@AttachmentServerName,CreatedUser=@CreatedUser,CreatedDate=GETDATE() where DocNo = @DocNo;";

            base.Parameter.Clear();

            // 添加參數
            base.Parameter.Add(new CommandParameter("@DocNo", DocNo));
            base.Parameter.Add(new CommandParameter("@AttachmentName", model.AttachmentName));
            base.Parameter.Add(new CommandParameter("@AttachmentServerPath", model.AttachmentServerPath));
            base.Parameter.Add(new CommandParameter("@AttachmentServerName", model.AttachmentServerName));
            base.Parameter.Add(new CommandParameter("@CreatedUser", model.CreatedUser));
            return trans == null ? base.ExecuteNonQuery(strSql) : base.ExecuteNonQuery(strSql, trans);
        }
        #endregion

        #region 附件是否存在
        public string SelectDocNo(WarningAttachment model, IDbTransaction trans = null)
        {
            try
            {
                var s = model.AttachmentName;
                var index = s.IndexOf(".");//查找.所在的位置
                var AttachmentName = s.Substring(0, index);
                string sqlStr = @"select DocNo from WarningMaster where DocNo =@DocNo";

                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@DocNo", AttachmentName));
                return trans == null ? Convert.ToString(base.ExecuteScalar(sqlStr)) : Convert.ToString(base.ExecuteScalar(sqlStr, trans));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public int SelectAttatchment(WarningAttachment model,string DocNo, IDbTransaction trans = null)
        {
            try
            {
                string sqlStr = @"select count(*) from WarningAttachment where DocNo = @DocNo and AttachmentName=@AttachmentName";

            base.Parameter.Clear();
            base.Parameter.Add(new CommandParameter("@DocNo", DocNo));
            base.Parameter.Add(new CommandParameter("@AttachmentName", model.AttachmentName));
            return trans == null ? Convert.ToInt32(base.ExecuteScalar(sqlStr)) : Convert.ToInt32(base.ExecuteScalar(sqlStr,trans));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion
    }
}
