using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CTBC.CSFS.Models;
using CTBC.CSFS.Pattern;
using System.Data;
using System.Transactions;
using CTBC.CSFS.ViewModels;

namespace CTBC.CSFS.BussinessLogic
{
    public class CaseAttachmentBIZ : CommonBIZ
    {
		private string userId;
		public string UserId
		{
			set { userId = value; }
			get { return userId; }
		}
        public int Create(CaseAttachment model, IDbTransaction trans = null)
        {
            string strSql = @" insert into CaseAttachment  (CaseId,AttachmentName,AttachmentServerPath,AttachmentServerName,isDelete,CreatedUser,CreatedDate) 
                                        values (
                                        @CaseId,@AttachmentName,@AttachmentServerPath,@AttachmentServerName,@isDelete,@CreatedUser,GETDATE());";

            base.Parameter.Clear();

            // 添加參數
            base.Parameter.Add(new CommandParameter("@CaseId", model.CaseId));
            base.Parameter.Add(new CommandParameter("@AttachmentName", model.AttachmentName));
            base.Parameter.Add(new CommandParameter("@AttachmentServerPath", model.AttachmentServerPath));
            base.Parameter.Add(new CommandParameter("@AttachmentServerName", model.AttachmentServerName));
            base.Parameter.Add(new CommandParameter("@isDelete", model.isDelete));
            base.Parameter.Add(new CommandParameter("@CreatedUser", model.CreatedUser));

            try
            {
                if (trans != null)
                {
                    return base.ExecuteNonQuery(strSql, trans);
                }
                else
                {
                    IDbConnection dbConnection = base.OpenConnection();
                    IDbTransaction tans = dbConnection.BeginTransaction();
                    return base.ExecuteNonQuery(strSql, trans);
                }
            }
            catch (Exception ex)
            {
                // 拋出異常
                throw ex;
            }
        }

        public int Edit(IList<CaseAttachment> model, IDbTransaction trans = null)
        {
            string strSql = string.Empty;
            foreach (CaseAttachment caseAttach in model.Where(caseAttach => caseAttach.AttachmentName != null && caseAttach.AttachmentServerName != null && caseAttach.AttachmentServerPath != null).Where(caseAttach => caseAttach.AttachmentName != "" && caseAttach.AttachmentServerName != "" && caseAttach.AttachmentServerPath != ""))
            {
                strSql += @" insert into CaseAttachment  (CaseId,AttachmentName,AttachmentServerPath,AttachmentServerName,isDelete,CreatedUser,CreatedDate) 
                                        values (
                                        '" + caseAttach.CaseId + "','" + caseAttach.AttachmentName + "','" + caseAttach.AttachmentServerPath + "','" + caseAttach.AttachmentServerName + "','" + caseAttach.isDelete + "','" + caseAttach.CreatedUser + "','" + DateTime.Now.ToString("yyyy/MM/dd") + "');";

                base.Parameter.Clear();

                // 添加參數
                //base.Parameter.Add(new CommandParameter("@CaseId", caseAttach.CaseId));
                //base.Parameter.Add(new CommandParameter("@AttachmentName", caseAttach.AttachmentName));
                //base.Parameter.Add(new CommandParameter("@AttachmentServerPath", caseAttach.AttachmentServerPath));
                //base.Parameter.Add(new CommandParameter("@AttachmentServerName", caseAttach.AttachmentServerName));
                //base.Parameter.Add(new CommandParameter("@isDelete", caseAttach.isDelete));
                //base.Parameter.Add(new CommandParameter("@CreatedUser", caseAttach.CreatedUser));
                //base.Parameter.Add(new CommandParameter("@CreatedDate", DateTime.Now));
            }

            try
            {
                if (strSql != "")//* 未上傳附件，則sql為空，返回值為1。
                {
                    if (trans != null)
                    {
                        return base.ExecuteNonQuery(strSql, trans);
                    }
                    IDbConnection dbConnection = base.OpenConnection();
                    IDbTransaction tans = dbConnection.BeginTransaction();
                    return base.ExecuteNonQuery(strSql, trans);
                }
                else
                {
                    return 1;
                }
            }
            catch (Exception ex)
            {

                // 拋出異常
                throw ex;
            }
        }

        public List<CaseAttachment> AttachmentList(Guid CaseId, IDbTransaction trans = null)
        {
            string strSql = "select * from CaseAttachment where CaseId=@CaseId";
            base.Parameter.Clear();
            base.Parameter.Add(new CommandParameter("@CaseId", CaseId));
            try
            {
                return trans == null ? base.SearchList<CaseAttachment>(strSql).ToList() : base.SearchList<CaseAttachment>(strSql, trans).ToList();

                //List<CaseAttachment> listItem = new List<CaseAttachment>();
                //IList<CaseAttachment> list = base.SearchList<CaseAttachment>(strSql);
                //for (int i = 0; i < list.Count; i++)
                //{
                //    listItem.Add(list[i]);
                //}
                //return listItem;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public CaseAttachment GetAttachmentInfo(int attachId)
        {
            string strSql = "SELECT [AttachmentId],[CaseId],[AttachmentName],[AttachmentServerPath],[AttachmentServerName],[isDelete],[CreatedUser],[CreatedDate] " +
                            "FROM [CaseAttachment] " +
                            "WHERE [AttachmentId]=@AttachmentId";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("@AttachmentId", attachId));
            IList<CaseAttachment> list = SearchList<CaseAttachment>(strSql);
            return list.FirstOrDefault();
        }
        public int DeleteAttatch(string AttatchId)
        {
            string strSql = " delete from CaseAttachment where AttachmentId=@AttachmentId ";
            base.Parameter.Clear();

            // 添加參數
            base.Parameter.Add(new CommandParameter("@AttachmentId", AttatchId));

            try
            {
				#region 記錄操作日誌
				CaseDataLog log = new CaseDataLog();
				CaseMasterBIZ casemaster = new CaseMasterBIZ();
				LdapEmployeeBiz empBiz = new LdapEmployeeBiz();
				LDAPEmployee empNow = empBiz.GetLdapEmployeeByEmpId(UserId);
				CaseAttachment attachment = GetAttachmentInfo(int.Parse(AttatchId));
				Guid TXSNO = Guid.NewGuid();
				DateTime TXDateTime = DateTime.Parse(System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
				log.TXSNO = TXSNO;
				log.TXType = "刪除";
				log.TXDateTime = TXDateTime;
				log.ColumnID = "AttachmentName";
				log.ColumnName = "附檔名稱";
				log.ColumnValueBefore = attachment.AttachmentName;
				log.ColumnValueAfter = "";
				log.TITLE = "收發作業-收發代辦";
				log.TabID = "Tab1-3";
				log.TabName = "公文資訊-附件資訊";
				log.TableName = "CaseAttachment";
				log.TableDispActive = "0";
				log.DispSrNo = 1;
				log.TableDispActive = "0";
				log.CaseId = attachment.CaseId.ToString();
				log.CaseNo = "";
				log.TXUser = UserId;
				log.TXUserName = empNow.EmpName;
				log.LinkDataKey = attachment.CaseId.ToString();
				casemaster.InsertCaseDataLog(log);
				#endregion 

				int result =  base.ExecuteNonQuery(strSql);
				return result;
			}
            catch (Exception ex)
            {
                // 拋出異常
                throw ex;
            }
        }

    }
}
