using System;
using System.Collections.Generic;
using System.Data;
using CTBC.CSFS.Models;
using CTBC.CSFS.Pattern;
using CTBC.CSFS.ViewModels;
using NPOI.SS.UserModel;
using NPOI.HSSF.Util;
using NPOI.SS.Util;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Web;
using CTBC.CSFS.Resource;
using CTBC.FrameWork.Platform;
using NPOI.HSSF.UserModel;
using CTBC.FrameWork.Util;
using Microsoft.Practices.Unity.InterceptionExtension;

namespace CTBC.CSFS.BussinessLogic
{
    public class CaseMasterBIZ : CommonBIZ
    {
        #region 建檔一個案件
        /// <summary>
        /// 建檔一個案件
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool CreateCase(ref CaseSeizureViewModel model)
        {
            Guid caseId = Guid.NewGuid();
            CaseNoTableBIZ noBiz = new CaseNoTableBIZ();
            CaseHistoryBIZ historyBiz = new CaseHistoryBIZ();
            IDbConnection dbConnection = OpenConnection();
            IDbTransaction dbTransaction = null;

            try
            {
                using (dbConnection)
                {
                    dbTransaction = dbConnection.BeginTransaction();
                    #region 主表
                    CaseMasterBIZ casemaster = new CaseMasterBIZ();
                    model.CaseMaster.CaseId = caseId;                               //* caseid
                    model.CaseMaster.DocNo = noBiz.GetDocNo(dbTransaction);         //* 系統編號
                    //* 案件編號 一般文的.在指派經辦時才有caseno
                    if (model.CaseMaster.CaseKind == CaseKind.CASE_SEIZURE)
                        model.CaseMaster.CaseNo = noBiz.GetCaseNo("A", dbTransaction);
                    else
                        model.CaseMaster.CaseNo = "";
                    model.CaseMaster.Status = CaseStatus.CaseInput;                 //* 建檔
                    model.CaseMaster.Person = model.CaseMaster.CreatedUser;         //* 建檔人
                    model.CaseMaster.CreatedDate = DateTime.Now.ToString("yyyy/MM/dd");
                    casemaster.Create(model.CaseMaster, dbTransaction);
                    #endregion

                    #region 責任人
                    CaseObligorBIZ caseobligor = new CaseObligorBIZ();
                    foreach (CaseObligor obligor in model.CaseObligorlistO)
                    {
                        obligor.CaseId = caseId;
                        if (obligor.ObligorNo != null)
                        {
                            string strUp = obligor.ObligorNo;
                            obligor.ObligorNo = strUp.ToUpper();
                        }
                        obligor.CreatedUser = model.CaseMaster.CreatedUser;
                    }
                    caseobligor.Create(model.CaseObligorlistO, dbTransaction);
                    caseobligor.CreateLog(model.CaseObligorlistO, model.CaseMaster, dbTransaction);
                    #endregion

                    #region 附件
                    CaseAttachmentBIZ attachment = new CaseAttachmentBIZ();
                    foreach (CaseAttachment attach in model.CaseAttachmentlistO)
                    {
                        attach.CaseId = caseId;
                        attach.CreatedUser = model.CaseMaster.CreatedUser;
                        attachment.Create(attach, dbTransaction);
                    }
                    #endregion

                    #region History
                    historyBiz.insertCaseHistory(caseId, CaseStatus.CaseInput, model.CaseMaster.CreatedUser, dbTransaction);
                    #endregion
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
                    // ignored

                }
                throw ex;
                return false;
            }
        }

        /// <summary>
        /// 建檔一個案件并自動派件.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        public bool AutoModeCase(ref CaseSeizureViewModel model)
        {
            Guid caseId = Guid.NewGuid();
            CaseNoTableBIZ noBiz = new CaseNoTableBIZ();
            CaseHistoryBIZ historyBiz = new CaseHistoryBIZ();
            IDbConnection dbConnection = OpenConnection();
            IDbTransaction dbTransaction = null;
            DateTime dtNow = GetNowDateTime();
            try
            {
                using (dbConnection)
                {
                    dbTransaction = dbConnection.BeginTransaction();
                    #region 主表
                    CaseMasterBIZ casemaster = new CaseMasterBIZ();
                    LdapEmployeeBiz agentBiz = new LdapEmployeeBiz();
                    model.CaseMaster.AgentUser = GetAutoEmployee(model.CaseMaster.CaseKind, model.CaseMaster.IsAutoDispatch, model.CaseMaster.IsAutoDispatchFS);
                    LDAPEmployee agent = agentBiz.GetAllEmployeeInEmployeeViewByEmpId(model.CaseMaster.AgentUser);
                    model.CaseMaster.CaseId = caseId;                               //* caseid
                    model.CaseMaster.DocNo = noBiz.GetDocNo(dbTransaction);         //* 系統編號
                    //* 案件編號 一般文的.在指派經辦時才有caseno
                    if (model.CaseMaster.CaseKind == CaseKind.CASE_SEIZURE)
                    {
                        model.CaseMaster.CaseNo = noBiz.GetCaseNo("A", dbTransaction);
                    }
                    else if (model.CaseMaster.CaseKind == CaseKind.CASE_EXTERNAL)
                    {
                        //*	外來文案件: C+民國年7碼+KXX (K:0~7 一科，K:9 二科，K:8 三科) (於集作收件派案後，系統自動編列)
                        string type = agent == null ? "C"
                                                    : agent.SectionName.Contains("一") ? "C1"
                                                    : agent.SectionName.Contains("二") ? "C2"
                                                    : agent.SectionName.Contains("三") ? "C3"
                                                    : "C";
                        model.CaseMaster.CaseNo = new CaseNoTableBIZ().GetCaseNo(type, dbTransaction);
                    }
                    else
                    {
                        model.CaseMaster.CaseNo = "";
                    }
                    model.CaseMaster.Status = CaseStatus.CollectionSubmit;                 //* 建檔并派件
                    model.CaseMaster.Person = model.CaseMaster.CreatedUser;         //* 建檔人
                    model.CaseMaster.CreatedDate = DateTime.Now.ToString("yyyy/MM/dd");
                    casemaster.Create(model.CaseMaster, dbTransaction);
                    #endregion

                    #region CaseAssignTable
                    //* 插入新
                    string strSql = @"INSERT INTO [CaseAssignTable] ([CaseId],[EmpId],[AlreadyAssign],[CreatdUser],[CreatedDate])VALUES (@CaseId,@EmpId,0,@UserId,@CreateDate);";
                    Parameter.Clear();
                    Parameter.Add(new CommandParameter("@CaseId", caseId));
                    Parameter.Add(new CommandParameter("@EmpId", model.CaseMaster.AgentUser));
                    Parameter.Add(new CommandParameter("@UserId", model.CaseMaster.CreatedUser));
                    Parameter.Add(new CommandParameter("@CreateDate", dtNow));
                    ExecuteNonQuery(strSql, dbTransaction);
                    #endregion

                    #region 責任人
                    CaseObligorBIZ caseobligor = new CaseObligorBIZ();
                    foreach (CaseObligor obligor in model.CaseObligorlistO)
                    {
                        obligor.CaseId = caseId;
                        if (obligor.ObligorNo != null)
                        {
                            string strUp = obligor.ObligorNo;
                            obligor.ObligorNo = strUp.ToUpper();
                        }
                        obligor.CreatedUser = model.CaseMaster.CreatedUser;
                    }
                    caseobligor.Create(model.CaseObligorlistO, dbTransaction);
                    caseobligor.CreateLog(model.CaseObligorlistO, model.CaseMaster, dbTransaction);
                    #endregion

                    #region 附件
                    CaseAttachmentBIZ attachment = new CaseAttachmentBIZ();
                    foreach (CaseAttachment attach in model.CaseAttachmentlistO)
                    {
                        attach.CaseId = caseId;
                        attach.CreatedUser = model.CaseMaster.CreatedUser;
                        attachment.Create(attach, dbTransaction);
                    }
                    #endregion

                    #region History
                    historyBiz.insertCaseHistory(caseId, CaseStatus.CaseInput, model.CaseMaster.CreatedUser, dbTransaction);//來文建檔
                    historyBiz.insertCaseHistory(caseId, CaseStatus.CollectionSubmit, model.CaseMaster.CreatedUser, dbTransaction);//收發分派
                    #endregion
                    dbTransaction.Commit();
                }
                return true;
            }
            catch (Exception ex)
            {
                try
                {
                    if (dbTransaction != null) dbTransaction.Rollback();
                    return false;
                }
                catch (Exception)
                {
                    // ignored

                }
                throw ex;
            }
        }
        /// <summary>
        /// 取得當下自動經辦的人.
        /// </summary>
        /// <returns></returns>
        public string GetAutoEmployee(string Ckd, bool IsAutoDispatch, bool IsAutoDispatchFS)
        {
            string strSql = "";
            string sql = "";
            int count = 0;
            string next = "";
            if (IsAutoDispatch && Ckd == CaseKind.CASE_SEIZURE)//扣押案件
            {
                strSql = @"select count(*) from CaseMaster where CaseNo like 'A%' and CreatedDate between convert(nvarchar(10),DATEADD(DAY,0, GETDATE()),23) and getdate();";
                sql = @"select distinct * from AgentSetting where IsSeizure = 1 order by SectionName,EmpId asc";
                count = (int)base.ExecuteScalar(strSql);
                IList<AgentSetting> agent = base.SearchList<AgentSetting>(sql);
                if(agent.Count > 0)
                {
                    int iAgentNo = count % agent.Count;
                    next = agent[iAgentNo].EmpId;
                }
            }
            if (IsAutoDispatchFS && Ckd == CaseKind.CASE_EXTERNAL)//外來文案件
            {
                strSql = @"select count(*) from CaseMaster where CaseNo like 'C%' and CreatedDate between convert(nvarchar(10),DATEADD(DAY,0, GETDATE()),23) and getdate();";
                sql = @"select distinct * from AgentSetting where IsCase = 1 order by SectionName,EmpId asc";
                count = (int)base.ExecuteScalar(strSql);
                IList<AgentSetting> agent = base.SearchList<AgentSetting>(sql);
                if(agent.Count > 0)
                {
                    int iAgentNo = count % agent.Count;
                    next = agent[iAgentNo].EmpId;
                }
            }
            return next;
        }
        #endregion

		/// <summary>
		/// 描    述：弹出模型对象列表比较器(根据ID比较)
		/// 作    者：kogu
		/// 创建日期：2010-02-23
		/// </summary>
		public class PopupComparer : IEqualityComparer<CaseObligor>
		{
			public static PopupComparer Default = new PopupComparer();
			#region IEqualityComparer<CaseObligor> 成员
			public bool Equals(CaseObligor x, CaseObligor y)
			{
				return x.ObligorId.Equals(y.ObligorId) 
					&& x.ObligorNo.Equals(y.ObligorNo == null ? "" : y.ObligorNo) 
					&& x.ObligorName.Equals(y.ObligorName == null ? "" : y.ObligorName) 
					&& x.ObligorAccount.Equals(y.ObligorAccount == null ? "" : y.ObligorAccount);
			}
			public int GetHashCode(CaseObligor obj)
			{
				return obj.GetHashCode();
			}
			#endregion
		}

		private string userId;
		public string UserId
		{
			set { userId = value; } 
			get { return userId; } 
		}

		private string caseId;
		public string CaseId
		{
			set { caseId = value; }
			get { return caseId; } 
		}

		private string caseNo;
		public string CaseNo
		{
			set { caseNo = value; }
			get { return caseNo; }
		}

		private string pageFrom;
		public string PageFrom
		{
			set { pageFrom = value; }
			get { return pageFrom; }
		}

		public int InsertCaseDataLog(CaseDataLog log, IDbTransaction trans = null)
		{
			CaseObligorBIZ caseobligor = new CaseObligorBIZ();
			LdapEmployeeBiz empBiz = new LdapEmployeeBiz();
			LDAPEmployee empNow = empBiz.GetLdapEmployeeByEmpId(UserId == null ? log.TXUser : UserId);

			log.TXUser = (empNow != null && !string.IsNullOrEmpty(empNow.EmpId) ? empNow.EmpId : " ") == "" ? log.TXUser : (empNow != null && !string.IsNullOrEmpty(empNow.EmpId) ? empNow.EmpId : " ");
			log.TXUserName = (empNow != null && !string.IsNullOrEmpty(empNow.EmpName) ? empNow.EmpName : " ") == "" ? log.TXUserName : (empNow != null && !string.IsNullOrEmpty(empNow.EmpName) ? empNow.EmpName : " ");
			if (PageFrom == "1")
			{
				log.md_FuncID = "Menu.CollectionToAgent";
				log.TITLE = "收發作業-收發代辦";
			}
			else if (PageFrom == "2")
			{
				log.md_FuncID = "Menu.CollectionToAgent";
				log.TITLE = "經辦作業-待辦理";
			}

			log.CaseId = CaseId == null ? log.CaseId : CaseId;
			log.CaseNo = CaseNo == null ? log.CaseNo : caseNo;

			return caseobligor.InsertCaseDataLog(log);
		}

        #region 修改一個案件
        /// <summary>
        /// 修改一個案件
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool EditCase(ref CaseSeizureViewModel model)
        {
            CaseNoTableBIZ noBiz = new CaseNoTableBIZ();
            CaseHistoryBIZ historyBiz = new CaseHistoryBIZ();
            IDbConnection dbConnection = OpenConnection();
            IDbTransaction dbTransaction = null;
            try
            {
                using (dbConnection)
                {
                    dbTransaction = dbConnection.BeginTransaction();
                    #region 主表
                    CaseMasterBIZ casemaster = new CaseMasterBIZ();
                    CaseMaster oldMaster = MasterModel(model.CaseMaster.CaseId, dbTransaction);
                    casemaster.Edit(model.CaseMaster, dbTransaction);
                    UpdateCaseMasterPayDate(model.CaseMaster.CaseId, oldMaster.CreatedDate, model.CaseMaster.CaseKind2, dbTransaction);

                    if (oldMaster.CaseKind != model.CaseMaster.CaseKind)//*修改了案件類型
                    {
                        CaseMemoBiz casememo = new CaseMemoBiz();
                        CaseMemo memoModel = new CaseMemo();
                        memoModel.CaseId = model.CaseMaster.CaseId;
                        memoModel.MemoType = "CaseMemo";
                        memoModel.Memo = "案件類型變更為: " + model.CaseMaster.CaseKind + ",案件編號從: " + oldMaster.CaseNo + " 變為:" + model.CaseMaster.CaseNo;
                        memoModel.MemoDate = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                        memoModel.MemoUser = model.CaseMaster.ModifiedUser;
                        casememo.Create(memoModel, null);
                        CreateCaseNoChangHistory(model.CaseMaster.CaseId, oldMaster.CaseNo, model.CaseMaster.CaseNo, model.CaseMaster.ModifiedUser, dbTransaction);
                    }
                    #endregion

					#region 附件
					CaseAttachmentBIZ attachment = new CaseAttachmentBIZ();
					foreach (CaseAttachment attach in model.CaseAttachmentlistO)
					{
						attach.CaseId = model.CaseMaster.CaseId;
						attach.CreatedUser = model.CaseMaster.ModifiedUser;
					}
					attachment.Edit(model.CaseAttachmentlistO, dbTransaction);
					#endregion

					#region History
					historyBiz.insertCaseHistory(model.CaseMaster.CaseId, CaseStatus.AgentEdit, model.CaseMaster.ModifiedUser, dbTransaction);
					#endregion
					dbTransaction.Commit();

					#region 責任人

					CaseObligorBIZ caseobligor = new CaseObligorBIZ();
					List<CaseObligor> oldObligorList = caseobligor.ObligorModel(model.CaseMaster.CaseId, null);

					for (int i = 0; i < oldObligorList.Count; i++)
					{
						if (model.CaseObligorlistO[i].ObligorId != 0 && !PopupComparer.Default.Equals(oldObligorList[i], model.CaseObligorlistO[i]))
						{
							model.CaseObligorlistO[i].isUpdate = true;
						}
						if (model.CaseObligorlistO[i].ObligorId == 0 && (string.IsNullOrEmpty(model.CaseObligorlistO[i].ObligorNo) == true ? "" : model.CaseObligorlistO[i].ObligorNo) == "" && (string.IsNullOrEmpty(model.CaseObligorlistO[i].ObligorName) == true ? "" : model.CaseObligorlistO[i].ObligorName) == "" && (string.IsNullOrEmpty(model.CaseObligorlistO[i].ObligorAccount) == true ? "" : model.CaseObligorlistO[i].ObligorAccount) == "")
						{
							model.CaseObligorlistO[i].isDelete = true;
						}
					}

					foreach (CaseObligor obligor in model.CaseObligorlistO)
					{
						obligor.CaseId = model.CaseMaster.CaseId;
						if (obligor.ObligorNo != null)
						{
							string strUp = obligor.ObligorNo;
							obligor.ObligorNo = strUp.ToUpper();
						}
						obligor.CreatedUser = model.CaseMaster.ModifiedUser;
					}

					Guid TXSNO = Guid.NewGuid();
					DateTime TXDateTime = DateTime.Parse(System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

					caseobligor.PageFrom = PageFrom;
					caseobligor.TXSNO = TXSNO;
					caseobligor.TXDateTime = TXDateTime;
					caseobligor.UserId = UserId;

					caseobligor.Edit(oldObligorList, model.CaseObligorlistO, null);
                    #endregion

					#region 操作記錄表
					CaseDataLog log = new CaseDataLog();

					#region CaseMaster表
					if (oldMaster.GovUnit != model.CaseMaster.GovUnit)
					{
						log.TXSNO = TXSNO;
						log.TXDateTime = TXDateTime;
						log.TXType = "修改";
						log.ColumnID = "GovUnit";
						log.ColumnName = "來文機關";
						log.ColumnValueBefore = oldMaster.GovUnit;
						log.ColumnValueAfter = model.CaseMaster.GovUnit;

						log.TabID = "Tab1-2";
						log.TabName = "公文資訊-案件資訊";
						log.TableName = "CaseMaster";
						log.DispSrNo = 1;
						log.TableDispActive = "1";
						log.LinkDataKey = model.CaseMaster.CaseId.ToString();
						InsertCaseDataLog(log);
					}
					if (oldMaster.GovDate != model.CaseMaster.GovDate)
					{
						log.TXSNO = TXSNO;
						log.TXDateTime = TXDateTime;
						log.TXType = "修改";
						log.ColumnID = "GovDate";
						log.ColumnName = "來文日期";
						log.ColumnValueBefore = oldMaster.GovDate;
						log.ColumnValueAfter = model.CaseMaster.GovDate;
						log.TabID = "Tab1-2";
						log.TabName = "公文資訊-案件資訊";
						log.TableName = "CaseMaster";
						log.DispSrNo = 2;
						log.TableDispActive = "1";
                        //List<CaseObligor> list = caseobligor.ObligorModel(model.CaseMaster.CaseId);
						log.LinkDataKey = model.CaseMaster.CaseId.ToString();
						InsertCaseDataLog(log);
					}
					if (oldMaster.GovNo != model.CaseMaster.GovNo)
					{
						log.TXSNO = TXSNO;
						log.TXDateTime = TXDateTime;
						log.TXType = "修改";
						log.ColumnID = "GovNo";
						log.ColumnName = "來文字號";
						log.ColumnValueBefore = oldMaster.GovNo;
						log.ColumnValueAfter = model.CaseMaster.GovNo;
						log.TabID = "Tab1-2";
						log.TabName = "公文資訊-案件資訊";
						log.TableName = "CaseMaster";
						log.DispSrNo = 3;
						log.TableDispActive = "1";
                        //List<CaseObligor> list = caseobligor.ObligorModel(model.CaseMaster.CaseId);
						log.LinkDataKey = model.CaseMaster.CaseId.ToString();
						InsertCaseDataLog(log);
					}
					if (oldMaster.Speed != model.CaseMaster.Speed)
					{
						log.TXSNO = TXSNO;
						log.TXDateTime = TXDateTime;
						log.TXType = "修改";
						log.ColumnID = "Speed";
						log.ColumnName = "速別";
						log.ColumnValueBefore = oldMaster.Speed;
						log.ColumnValueAfter = model.CaseMaster.Speed;
						log.TabID = "Tab1-2";
						log.TabName = "公文資訊-案件資訊";
						log.TableName = "CaseMaster";
						log.DispSrNo = 4;
						log.TableDispActive = "1";
                        //List<CaseObligor> list = caseobligor.ObligorModel(model.CaseMaster.CaseId);
						log.LinkDataKey = model.CaseMaster.CaseId.ToString();
						InsertCaseDataLog(log);
					}
					if (oldMaster.ReceiveKind != model.CaseMaster.ReceiveKind)
					{
						log.TXSNO = TXSNO;
						log.TXDateTime = TXDateTime;
						log.TXType = "修改";
						log.ColumnID = "ReceiveKind";
						log.ColumnName = "來文方式";
						log.ColumnValueBefore = oldMaster.ReceiveKind;
						log.ColumnValueAfter = model.CaseMaster.ReceiveKind;
						log.TabID = "Tab1-2";
						log.TabName = "公文資訊-案件資訊";
						log.TableName = "CaseMaster";
						log.DispSrNo = 5;
						log.TableDispActive = "1";
                        //List<CaseObligor> list = caseobligor.ObligorModel(model.CaseMaster.CaseId);
						log.LinkDataKey = model.CaseMaster.CaseId.ToString();
						InsertCaseDataLog(log);
					}
					if (oldMaster.CaseKind2 != model.CaseMaster.CaseKind2)
					{
						log.TXSNO = TXSNO;
						log.TXDateTime = TXDateTime;
						log.TXType = "修改";
						log.ColumnID = "CaseKind2";
						log.ColumnName = "類別";
						log.ColumnValueBefore = oldMaster.CaseKind2;
						log.ColumnValueAfter = model.CaseMaster.CaseKind2;
						log.TabID = "Tab1-2";
						log.TabName = "公文資訊-案件資訊";
						log.TableName = "CaseMaster";
						log.DispSrNo = 6;
						log.TableDispActive = "1";
                        //List<CaseObligor> list = caseobligor.ObligorModel(model.CaseMaster.CaseId);
						log.LinkDataKey = model.CaseMaster.CaseId.ToString();
						InsertCaseDataLog(log);
					}
                    if (oldMaster.Receiver != model.CaseMaster.Receiver)//受文者，有修改，新增CaseDataLog
                    {
                        log.TXSNO = TXSNO;
                        log.TXDateTime = TXDateTime;
                        log.TXType = "修改";
                        log.ColumnID = "Receiver";
                        log.ColumnName = "受文者";
                        log.ColumnValueBefore = oldMaster.Receiver;
                        log.ColumnValueAfter = model.CaseMaster.Receiver;
                        log.TabID = "Tab1-2";
                        log.TabName = "公文資訊-案件資訊";
                        log.TableName = "CaseMaster";
                        log.DispSrNo = 7;
                        log.TableDispActive = "1";
                        log.LinkDataKey = model.CaseMaster.CaseId.ToString();
                        InsertCaseDataLog(log);
                    }
                    if (oldMaster.ReceiveAmount != model.CaseMaster.ReceiveAmount)//來函扣押總金額，有修改，新增CaseDataLog
                    {
                        log.TXSNO = TXSNO;
                        log.TXDateTime = TXDateTime;
                        log.TXType = "修改";
                        log.ColumnID = "ReceiveAmount";
                        log.ColumnName = "來函扣押總金額";
                        log.ColumnValueBefore = oldMaster.ReceiveAmount.ToString();
                        log.ColumnValueAfter = model.CaseMaster.ReceiveAmount.ToString();
                        log.TabID = "Tab1-2";
                        log.TabName = "公文資訊-案件資訊";
                        log.TableName = "CaseMaster";
                        log.DispSrNo = 8;
                        log.TableDispActive = "1";
                        log.LinkDataKey = model.CaseMaster.CaseId.ToString();
                        InsertCaseDataLog(log);
                    }
                    if (oldMaster.NotSeizureAmount != model.CaseMaster.NotSeizureAmount)//金額未達毋需扣押，有修改，新增CaseDataLog
                    {
                        log.TXSNO = TXSNO;
                        log.TXDateTime = TXDateTime;
                        log.TXType = "修改";
                        log.ColumnID = "NotSeizureAmount";
                        log.ColumnName = "金額未達毋需扣押";
                        log.ColumnValueBefore = oldMaster.NotSeizureAmount.ToString();
                        log.ColumnValueAfter = model.CaseMaster.NotSeizureAmount.ToString();
                        log.TabID = "Tab1-2";
                        log.TabName = "公文資訊-案件資訊";
                        log.TableName = "CaseMaster";
                        log.DispSrNo = 9;
                        log.TableDispActive = "1";
                        log.LinkDataKey = model.CaseMaster.CaseId.ToString();
                        InsertCaseDataLog(log);
                    }
					#endregion 

					#region 附件
					#region 記錄操作日誌 
					foreach (CaseAttachment attach in model.CaseAttachmentlistO)
					{
						log.TXSNO = TXSNO;
						log.TXType = "上傳";
						log.TXDateTime = TXDateTime;
						log.ColumnID = "AttachmentName";
						log.ColumnName = "附檔名稱";
						log.ColumnValueBefore = "";
						log.ColumnValueAfter = attach.AttachmentName;
						log.TabID = "Tab1-3";
						log.TabName = "公文資訊-附件資訊";
						log.TableName = "CaseAttachment";
						log.TableDispActive = "0";
						log.DispSrNo = 1;
						log.TableDispActive = "0";
						log.CaseId = model.CaseMaster.CaseId.ToString();
						log.CaseNo = "";
						log.TXUser = UserId;
						//log.TXUserName = empNow.EmpName;
						log.LinkDataKey = model.CaseMaster.CaseId.ToString();
						casemaster.InsertCaseDataLog(log);
					}
					
					#endregion 
					#endregion 

					#endregion
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
		#endregion 

        public bool UpdateCaseMasterPayDate(Guid caseId, string crateDate, string caseKind2, IDbTransaction dbTrans = null, string agent = null)
        {
            string payDate = "";
            if(!string.IsNullOrEmpty(agent))
            {
                payDate = crateDate;//從CaseAccountBiz傳過來的支付時間
            }
            else
            {
                DateTime dtCreateDate;
                if (!DateTime.TryParse(crateDate, out dtCreateDate))
                    dtCreateDate = DateTime.Today;

                //* 支付時間
                payDate = GetPayDate(caseKind2, crateDate).ToString("yyyy/MM/dd"); ;
                //Add by zhangwei 20180315 start
                //判斷支票列印日期是否工作日，如果為非工作日自動帶入下一個營業日期
                CheckQueryAndPrintBIZ CKP = new CheckQueryAndPrintBIZ();
                payDate = CKP.GetWorkingDays(payDate);
                //Add by zhangwei 20180315 end
            }
            
            string sql = @"UPDATE [CaseMaster] SET [PayDate] = @PayDate WHERE [CaseId] = @CaseId";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("PayDate", payDate));
            Parameter.Add(new CommandParameter("CaseId", caseId));
            return dbTrans == null ? ExecuteNonQuery(sql) > 0 : ExecuteNonQuery(sql, dbTrans) > 0;
        }


        #region  實際新增 CaseNoChangHistory
        /// <summary>
        /// 實際新增 CaseNoChangHistory
        /// </summary>
        /// <param name="model"></param>
        /// <param name="trans"></param>
        /// <returns></returns>
        public int CreateCaseNoChangHistory(Guid CaseId, string oldCaseNo, string newCaseNo, string userId, IDbTransaction trans = null)
        {
            string sql = @" insert into CaseNoChangeHistory  (CaseId,OldCaseNo,NewCaseNo,CreatedUser,CreatedDate) 
                                        values (
                                        @CaseId,@OldCaseNo,@NewCaseNo,@CreatedUser,GETDATE());";

            Parameter.Clear();

            // 添加參數
            Parameter.Add(new CommandParameter("@CaseId", CaseId));
            Parameter.Add(new CommandParameter("@OldCaseNo", oldCaseNo));
            Parameter.Add(new CommandParameter("@NewCaseNo", newCaseNo));
            Parameter.Add(new CommandParameter("@CreatedUser", userId));
            return trans == null ? ExecuteNonQuery(sql) : ExecuteNonQuery(sql, trans);
        }
        #endregion

        #region 實際編輯操作
        public int Edit(CaseMaster model, IDbTransaction trans = null)
        {
            CaseNoTableBIZ noBiz = new CaseNoTableBIZ();
            int rtn = 0;
            string sql = @" update  CaseMaster  set 
                                    DocNo=@DocNo,Status=@Status,isDelete=@isDelete,CaseNo=@CaseNo,Receiver=@Receiver,ReceiveAmount=@ReceiveAmount,
                                    GovKind=@GovKind,GovUnit=@GovUnit,ReceiverNo=@ReceiverNo,GovDate=@GovDate,Speed=@Speed,NotSeizureAmount=@NotSeizureAmount,
                                    ReceiveKind=@ReceiveKind,GovNo=@GovNo,LimitDate=@LimitDate,CaseKind=@CaseKind,CaseKind2=@CaseKind2,
                                    Person=@Person,AssignPerson=@AssignPerson,ModifiedUser=@ModifiedUser,ModifiedDate=@ModifiedDate,PropertyDeclaration=@PropertyDeclaration
                                    where CaseId= @CaseId";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("@DocNo", model.DocNo));
            Parameter.Add(new CommandParameter("@Status", model.Status));
            Parameter.Add(new CommandParameter("@isDelete", model.isDelete));
            if (model.CaseNo == null)
            {
                Parameter.Add(new CommandParameter("@CaseNo", ""));
            }
            else
            {
                Parameter.Add(new CommandParameter("@CaseNo", model.CaseNo));
            }
            Parameter.Add(new CommandParameter("@GovKind", model.GovKind));
            Parameter.Add(new CommandParameter("@GovUnit", model.GovUnit));
            Parameter.Add(new CommandParameter("@ReceiverNo", model.ReceiverNo));
            Parameter.Add(new CommandParameter("@GovDate", model.GovDate));
            Parameter.Add(new CommandParameter("@Speed", model.Speed));
            Parameter.Add(new CommandParameter("@ReceiveKind", model.ReceiveKind));
            Parameter.Add(new CommandParameter("@GovNo", model.GovNo));
            Parameter.Add(new CommandParameter("@LimitDate", model.LimitDate));
            Parameter.Add(new CommandParameter("@CaseKind", model.CaseKind));
            Parameter.Add(new CommandParameter("@CaseKind2", model.CaseKind2));
            Parameter.Add(new CommandParameter("@PropertyDeclaration", model.PropertyDeclaration));
            Parameter.Add(new CommandParameter("@Person", model.Person));
            Parameter.Add(new CommandParameter("@AssignPerson", model.AssignPerson));
            Parameter.Add(new CommandParameter("@ModifiedUser", model.ModifiedUser));
            Parameter.Add(new CommandParameter("@ModifiedDate", DateTime.Now.ToString("yyyy/MM/dd")));
            Parameter.Add(new CommandParameter("@Receiver", model.Receiver));
            Parameter.Add(new CommandParameter("@ReceiveAmount", model.ReceiveAmount));
            Parameter.Add(new CommandParameter("@NotSeizureAmount", model.NotSeizureAmount));
            Parameter.Add(new CommandParameter("@CaseId", model.CaseId));

            rtn = trans == null ? ExecuteNonQuery(sql) : ExecuteNonQuery(sql, trans);
            //*20150514 CR 允許修改大類,重新取號
            if (model.CaseKind != model.OldCaseKind)
            {
                if (model.CaseKind == CaseKind.CASE_SEIZURE)
                {
                    model.CaseNo = noBiz.GetCaseNo("A", trans);
                }
                if (model.CaseKind == CaseKind.CASE_EXTERNAL)
                {
                    //LdapEmployeeBiz agentBiz = new LdapEmployeeBiz();
                    //LDAPEmployee agent = agentBiz.GetAllEmployeeInEmployeeViewByEmpId(Account);
                    //string type = agent == null ? "C"
                    //                                    : agent.SectionName.Contains("一") ? "C1"
                    //                                    : agent.SectionName.Contains("二") ? "C2"
                    //                                    : agent.SectionName.Contains("三") ? "C3"
                    //                                    : "C";
                    //model.CaseNo = new CaseNoTableBIZ().GetCaseNo(type, trans);

                    //* 集作才能修改大類.外來文集作時沒有號碼
                    model.CaseNo = "";
                }

                sql = "UPDATE [CaseMaster] SET [CaseNo] = @CaseNo WHERE [CaseId] = @CaseId";
                Parameter.Clear();
                Parameter.Add(new CommandParameter("CaseId", model.CaseId));
                Parameter.Add(new CommandParameter("CaseNo", model.CaseNo));
                ExecuteNonQuery(sql, trans);
            }


            return rtn;
        }
        #endregion

        #region  實際新增 CaseMaster

        public DateTime GetPayDate(string caseKind2, string createDate)
        {
            //案件類型扣押並支付 建檔日期 + 20日 -> D日 
            //案件類型 支付 建檔日期為 D日 
            //若D日 為上周二到本周一   (支付日期為當周三)
            //為本周二到下周一   (支付日期為下周三) 
            //* 建檔時間
            DateTime dtCreateDate;
            if (!DateTime.TryParse(createDate, out dtCreateDate))
                dtCreateDate = DateTime.Today;
            //* 支付時間
            return GetPayDate(caseKind2, dtCreateDate);
        }

        public DateTime GetPayDate(string caseKind2, DateTime createDate)
        {
            //案件類型扣押並支付 建檔日期 + 20日 -> D日 
            //案件類型 支付 建檔日期為 D日 
            //若D日 為上周二到本周一   (支付日期為當周三)
            //為本周二到下周一   (支付日期為下周三) 
            //* 支付時間
            DateTime payDate = caseKind2 == CaseKind2.CaseSeizureAndPay
                ? UtlString.GetCheckDateForSeizureAndPay(createDate.AddDays(20))
                : UtlString.GetCheckDate(createDate);
            return payDate;
        }


        /// <summary>
        /// 實際新增 CaseMaster
        /// </summary>
        /// <param name="model"></param>
        /// <param name="trans"></param>
        /// <returns></returns>
        public int Create(CaseMaster model, IDbTransaction trans = null)
        {
            //* 查詢支付時間CR
            //案件類型扣押並支付 建檔日期 + 20日 -> D日 
            //案件類型 支付 建檔日期為 D日 
            //若D日 為上周二到本周一   (支付日期為當周三)
            //為本周二到下周一   (支付日期為下周三) 
            //* 建檔時間

            //* 支付時間
            string payDate = GetPayDate(model.CaseKind2, model.CreatedDate).ToString("yyyy/MM/dd");
            //Add by zhangwei 20180315 start
            //判斷支票列印日期是否工作日，如果為非工作日自動帶入下一個營業日期
            CheckQueryAndPrintBIZ CKP = new CheckQueryAndPrintBIZ();
            payDate =CKP.GetWorkingDays(payDate);
            payDate = Convert.ToDateTime(payDate).ToString("yyyy/MM/dd");
            //Add by zhangwei 20180315 end
            string sql = @" insert into CaseMaster  (CaseId,DocNo,Status,isDelete,CaseNo,GovKind,GovUnit,ReceiverNo,GovDate,Speed,ReceiveKind,GovNo,LimitDate,
                                        CaseKind,CaseKind2,ReceiveDate,Unit,Person,AssignPerson,CreatedUser,CreatedDate,PropertyDeclaration,AgentUser,payDate,AgentBranchId,AgentDeptId,AgentSection,Receiver,ReceiveAmount,NotSeizureAmount) 
                                        values (
                                        @CaseId,@DocNo,@Status,@isDelete,@CaseNo,@GovKind,@GovUnit,@ReceiverNo,@GovDate,@Speed,@ReceiveKind,@GovNo,@LimitDate,
                                        @CaseKind,@CaseKind2,@ReceiveDate,@Unit,@Person,@AssignPerson,@CreatedUser,@CreatedDate,@PropertyDeclaration,@AgentUser,@payDate,@AgentBranchId,@AgentDeptId,@AgentSection,@Receiver,@ReceiveAmount,@NotSeizureAmount);";

            Parameter.Clear();

            // 添加參數
            Parameter.Add(new CommandParameter("@CaseId", model.CaseId));
            Parameter.Add(new CommandParameter("@DocNo", model.DocNo));
            Parameter.Add(new CommandParameter("@Status", model.Status));
            Parameter.Add(new CommandParameter("@isDelete", model.isDelete));
            Parameter.Add(new CommandParameter("@CaseNo", model.CaseNo));
            Parameter.Add(new CommandParameter("@GovKind", model.GovKind));
            Parameter.Add(new CommandParameter("@GovUnit", model.GovUnit));
            Parameter.Add(new CommandParameter("@ReceiverNo", model.ReceiverNo));
            Parameter.Add(new CommandParameter("@GovDate", model.GovDate));
            Parameter.Add(new CommandParameter("@Speed", model.Speed));
            Parameter.Add(new CommandParameter("@ReceiveKind", model.ReceiveKind));
            Parameter.Add(new CommandParameter("@GovNo", model.GovNo));
            Parameter.Add(new CommandParameter("@LimitDate", model.LimitDate));
            Parameter.Add(new CommandParameter("@CaseKind", model.CaseKind));
            Parameter.Add(new CommandParameter("@CaseKind2", model.CaseKind2));
            Parameter.Add(new CommandParameter("@PropertyDeclaration", model.PropertyDeclaration));
            Parameter.Add(new CommandParameter("@ReceiveDate", model.ReceiveDate));
            Parameter.Add(new CommandParameter("@Unit", model.Unit));
            Parameter.Add(new CommandParameter("@Person", model.Person));
            Parameter.Add(new CommandParameter("@AssignPerson", model.AssignPerson));
            Parameter.Add(new CommandParameter("@CreatedUser", model.CreatedUser));
            Parameter.Add(new CommandParameter("@CreatedDate", model.CreatedDate));
            Parameter.Add(new CommandParameter("@Receiver", model.Receiver));
            Parameter.Add(new CommandParameter("@ReceiveAmount", model.ReceiveAmount));
            Parameter.Add(new CommandParameter("@NotSeizureAmount", model.NotSeizureAmount));
            //*再次發文新增
            LdapEmployeeBiz agentBiz = new LdapEmployeeBiz();
            LDAPEmployee agent = agentBiz.GetAllEmployeeInEmployeeViewByEmpId(model.AgentUser);
            Parameter.Add(new CommandParameter("@AgentUser", model.AgentUser));
            if (model.AgentUser != null && agent.SectionName != null && agent.DepId != null)
            {
                Parameter.Add(new CommandParameter("@AgentBranchId", agent.BranchId));
                Parameter.Add(new CommandParameter("@AgentDeptId", agent.DepId));
                Parameter.Add(new CommandParameter("@AgentSection", agent.SectionName));
            }
            else
            {
                sql = @" insert into CaseMaster  (CaseId,DocNo,Status,isDelete,CaseNo,GovKind,GovUnit,ReceiverNo,GovDate,Speed,ReceiveKind,GovNo,LimitDate,
                                        CaseKind,CaseKind2,ReceiveDate,Unit,Person,AssignPerson,CreatedUser,CreatedDate,PropertyDeclaration,AgentUser,payDate,Receiver,ReceiveAmount,NotSeizureAmount) 
                                        values (
                                        @CaseId,@DocNo,@Status,@isDelete,@CaseNo,@GovKind,@GovUnit,@ReceiverNo,@GovDate,@Speed,@ReceiveKind,@GovNo,@LimitDate,
                                        @CaseKind,@CaseKind2,@ReceiveDate,@Unit,@Person,@AssignPerson,@CreatedUser,@CreatedDate,@PropertyDeclaration,@AgentUser,@payDate,@Receiver,@ReceiveAmount,@NotSeizureAmount);";
            }
            //* 查詢新增
            Parameter.Add(new CommandParameter("@payDate", payDate));

            if (trans != null)
            {
                // 執行新增返回是否成功
                return ExecuteNonQuery(sql, trans);
            }
            IDbConnection dbConnection = OpenConnection();
            IDbTransaction tans = dbConnection.BeginTransaction();
            return ExecuteNonQuery(sql, tans);
        }
        #endregion

        #region 刪除casemaster
        public bool DeleteCaseMaster(Guid caseId, IDbTransaction dbtrans = null)
        {
            string strSql = "DELETE CaseMaster WHERE " +
                            "CaseId = @CaseId ";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("CaseId", caseId));
            return dbtrans == null ? ExecuteNonQuery(strSql) > 0 : ExecuteNonQuery(strSql, dbtrans) > 0;
        }
        #endregion

        #region 檢查此GovNo是否存在
        /// <summary>
        /// 檢查此GovNo是否存在
        /// </summary>
        /// <param name="txtGovNo"></param>
        /// <returns></returns>
        public string IsGovNoExist(string txtGovNo)
        {
            string strSql = "select count(0) from CaseMaster where  GovNo=@GovNo";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("@GovNo", txtGovNo));
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
        #endregion

        #region 檢查此CaseNo是否存在
        /// <summary>
        /// 檢查此CaseNo是否存在
        /// </summary>
        /// <param name="txtGovNo"></param>
        /// <returns></returns>
        public string IsCaseNoExist(string CaseNo, IDbTransaction trans = null)
        {
            string strSql = "select count(0) from CaseMaster where  CaseNo=@CaseNo";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("@CaseNo", CaseNo));

            try
            {
                int a = (int)ExecuteScalar(strSql, trans);
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
        #endregion

		public CaseMaster MasterModelNew(Guid caseId, IDbTransaction trans = null)
		{
            string strSql = @"select [CaseId],[DocNo],[Status],[isDelete],[CaseNo],[GovKind],[Receiver],[ReceiveAmount],[NotSeizureAmount]
                                    ,[GovUnit],[ReceiverNo],CONVERT(varchar(100), [GovDate], 111) as [GovDate] 
                                    ,[Speed],[ReceiveKind],[GovNo],CONVERT(varchar(100), [LimitDate], 111) as [LimitDate] 
                                    ,[CaseKind],[CaseKind2],[ReceiveDate],[Unit],[Person],[AssignPerson],[CreatedUser],CONVERT(varchar(100), [PayDate], 111) as  [PayDate],
                                    CONVERT(varchar(100), [CreatedDate], 111) as  [CreatedDate],[ModifiedUser],[ModifiedDate],AfterSeizureApproved,AgentUser2,AgentUser
                                     from CaseMaster where CaseId=@CaseId";
			Parameter.Clear();
			Parameter.Add(new CommandParameter("@CaseId", caseId));
			IList<CaseMaster> list = trans == null ? SearchList<CaseMaster>(strSql) : SearchList<CaseMaster>(strSql, trans);
			if (list != null)
			{
				return list.Count > 0 ? list[0] : new CaseMaster();
			}
			return new CaseMaster();
		}

        #region 通過一個CaseId得到對應的CaseMaster
        /// <summary>
        /// 通過一個CaseId得到對應的CaseMaster
        /// </summary>
        /// <param name="caseId"></param>
        /// <param name="trans"></param>
        /// <returns></returns>
        public CaseMaster MasterModel(Guid caseId, IDbTransaction trans = null)
        {
            string strSql = @";with T1 as(select distinct [CaseId],[DocNo],[Status],[isDelete],[CaseNo],[GovKind],[Receiver],[ReceiveAmount],[NotSeizureAmount]
                                    ,[GovUnit],[ReceiverNo],CONVERT(varchar(100), [GovDate], 111) as [GovDate] 
                                    ,[Speed],[ReceiveKind],[GovNo],CONVERT(varchar(100), [LimitDate], 111) as [LimitDate] 
                                    ,[CaseKind],[CaseKind2],[ReceiveDate],[Unit],[Person],[AssignPerson],[CreatedUser],CONVERT(varchar(100), [PayDate], 111) as  [PayDate],
                                    CONVERT(varchar(100), [CreatedDate], 111) as  [CreatedDate],[ModifiedUser],[ModifiedDate],AfterSeizureApproved,AgentUser2,AgentUser
                                    from CaseMaster 
									where CaseId=@CaseId) ,
									T2 as (select top 1 ColumnValueBefore,ColumnValueAfter,ColumnID,LinkDataKey from CaseDataLog c1 
                                    where CaseId=@CaseId and TabName='公文資訊-案件資訊' and TableName='Casemaster' and LinkDataKey=cast(CaseId as nvarchar(100)) and ColumnID='GovUnit'
                                    and TXDateTime=(select max(TXDateTime) from CaseDataLog c2 where c1.CaseId=c2.CaseId and c1.TabName=c2.TabName and c1.TableName=c2.TableName and c1.LinkDataKey=c2.LinkDataKey and ColumnID='GovUnit')
                                    union all 
                                    select top 1 ColumnValueBefore,ColumnValueAfter,ColumnID,LinkDataKey from CaseDataLog c1 
                                    where CaseId=@CaseId and TabName='公文資訊-案件資訊' and TableName='Casemaster' and LinkDataKey=cast(CaseId as nvarchar(100)) and ColumnID='GovNo'
                                    and TXDateTime=(select max(TXDateTime) from CaseDataLog c2 where c1.CaseId=c2.CaseId and c1.TabName=c2.TabName and c1.TableName=c2.TableName and c1.LinkDataKey=c2.LinkDataKey and ColumnID='GovNo') 
                                    union all
                                    select top 1 ColumnValueBefore,ColumnValueAfter,ColumnID,LinkDataKey from CaseDataLog c1 
                                    where CaseId=@CaseId and TabName='公文資訊-案件資訊' and TableName='Casemaster' and LinkDataKey=cast(CaseId as nvarchar(100)) and ColumnID='GovDate'
                                    and TXDateTime=(select max(TXDateTime) from CaseDataLog c2 where c1.CaseId=c2.CaseId and c1.TabName=c2.TabName and c1.TableName=c2.TableName and c1.LinkDataKey=c2.LinkDataKey and ColumnID='GovDate')
									union all 
                                    select top 1 ColumnValueBefore,ColumnValueAfter,ColumnID,LinkDataKey from CaseDataLog c1 
                                    where CaseId=@CaseId and TabName='公文資訊-案件資訊' and TableName='Casemaster' and LinkDataKey=cast(CaseId as nvarchar(100)) and ColumnID='Receiver'
                                    and TXDateTime=(select max(TXDateTime) from CaseDataLog c2 where c1.CaseId=c2.CaseId and c1.TabName=c2.TabName and c1.TableName=c2.TableName and c1.LinkDataKey=c2.LinkDataKey and ColumnID='Receiver') 
									union all
                                    select top 1 ColumnValueBefore,ColumnValueAfter,ColumnID,LinkDataKey from CaseDataLog c1 
                                    where CaseId=@CaseId and TabName='公文資訊-案件資訊' and TableName='Casemaster' and LinkDataKey=cast(CaseId as nvarchar(100)) and ColumnID='ReceiveAmount'
                                    and TXDateTime=(select max(TXDateTime) from CaseDataLog c2 where c1.CaseId=c2.CaseId and c1.TabName=c2.TabName and c1.TableName=c2.TableName and c1.LinkDataKey=c2.LinkDataKey and ColumnID='ReceiveAmount')
									union all
                                    select top 1 ColumnValueBefore,ColumnValueAfter,ColumnID,LinkDataKey from CaseDataLog c1 
                                    where CaseId=@CaseId and TabName='公文資訊-案件資訊' and TableName='Casemaster' and LinkDataKey=cast(CaseId as nvarchar(100)) and ColumnID='NotSeizureAmount'
                                    and TXDateTime=(select max(TXDateTime) from CaseDataLog c2 where c1.CaseId=c2.CaseId and c1.TabName=c2.TabName and c1.TableName=c2.TableName and c1.LinkDataKey=c2.LinkDataKey and ColumnID='NotSeizureAmount')),
									T3 as (select T1.*,
								    (select case  when ColumnValueBefore<>[GovKind] then 'true' else 'false' end from T2 where ColumnID='GovKind' and LinkDataKey=cast(CaseId as nvarchar(100))) as GovKindflag,
								    (select case  when ColumnValueBefore<>[GovUnit] then 'true' else 'false' end from T2 where ColumnID='GovUnit' and LinkDataKey=cast(CaseId as nvarchar(100))) as GovUnitflag,
								    (select case  when ColumnValueBefore<>[GovNo] then 'true' else 'false' end from T2 where ColumnID='GovNo' and LinkDataKey=cast(CaseId as nvarchar(100))) as GovNoflag,
								    (select case  when ColumnValueBefore<>[GovDate] then 'true' else 'false' end from T2 where ColumnID='GovDate' and LinkDataKey=cast(CaseId as nvarchar(100))) as GovDateflag,
									(select case  when ColumnValueBefore<>[Receiver] then 'true' else 'false' end from T2 where ColumnID='Receiver' and LinkDataKey=cast(CaseId as nvarchar(100))) as Receiverflag,
									(select case  when ColumnValueBefore<>cast([ReceiveAmount] as nvarchar(500)) then 'true' else 'false' end from T2 where ColumnID='ReceiveAmount' and LinkDataKey=cast(CaseId as nvarchar(100))) as ReceiveAmountflag,
									(select case  when ColumnValueBefore<>cast([NotSeizureAmount] as nvarchar(500)) then 'true' else 'false' end from T2 where ColumnID='NotSeizureAmount' and LinkDataKey=cast(CaseId as nvarchar(100))) as NotSeizureAmountflag
								    from T1)
								    select * from T3";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("@CaseId", caseId));
            IList<CaseMaster> list = trans == null ? SearchList<CaseMaster>(strSql) : SearchList<CaseMaster>(strSql, trans);
            if (list != null)
            {
                return list.Count > 0 ? list[0] : new CaseMaster();
            }
            return new CaseMaster();
        }

        /// <summary>
        /// 判斷來文機關的資料是否有在DB的資料中
        /// </summary>
        /// <param name="govName">來文機關名稱</param>
        /// <returns></returns>
        public string IsGovNameExist(string govName)
        {
            string strSql = "select count(0) from GovAddress where GovName=@GovName";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("@GovName", govName));
            try
            {
                int a = (int)ExecuteScalar(strSql);
                if (a > 0)
                {
                    return "1";//有
                }
                return "0";//無
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public CaseMaster NewMasterModel(Guid caseId, IDbTransaction trans = null)
        {
            string strSql = @"select [CaseId],[DocNo],[Status],[isDelete],[CaseNo],[GovKind]
                                    ,[GovUnit],[ReceiverNo],CONVERT(varchar(100), [GovDate], 111) as [GovDate] 
                                    ,[Speed],[ReceiveKind],[GovNo],CONVERT(varchar(100), [LimitDate], 111) as [LimitDate] 
                                    ,[CaseKind],[CaseKind2],[ReceiveDate],[Unit],[Person],[AssignPerson],[CreatedUser],CONVERT(varchar(100), [PayDate], 111) as  [PayDate],
                                    CONVERT(varchar(100), [CreatedDate], 111) as  [CreatedDate],[ModifiedUser],[ModifiedDate],AfterSeizureApproved,AgentUser2,AgentUser,AgentDeptId,AgentSection
                                     from CaseMaster where CaseId=@CaseId";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("@CaseId", caseId));
            IList<CaseMaster> list = trans == null ? SearchList<CaseMaster>(strSql) : SearchList<CaseMaster>(strSql, trans);
            if (list != null)
            {
                return list.Count > 0 ? list[0] : new CaseMaster();
            }
            return new CaseMaster();
        }
        #endregion

        #region 修改CaseMaster的狀態,並且記錄Log
        /// <summary>
        /// 修改CaseMaster的狀態,並且記錄Log
        /// </summary>
        /// <param name="caseid"></param>
        /// <param name="newStatus"></param>
        /// <param name="userId"></param>
        /// <param name="trans"></param>
        /// <returns></returns>
        public int UpdateCaseStatus(Guid caseid, string newStatus, string userId, IDbTransaction trans = null)
        {
            DateTime dtNow = GetNowDateTime();
            CaseHistoryBIZ history = new CaseHistoryBIZ();
            string strSql = @"UPDATE [CaseMaster] SET [Status] = @Status,
                [ModifiedUser] =@UpdateUserId, [ModifiedDate]=@UpdateDate WHERE [CaseId] = @CaseId";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("@CaseId", caseid));
            Parameter.Add(new CommandParameter("@Status", newStatus));
            Parameter.Add(new CommandParameter("@UpdateUserId", userId));
            Parameter.Add(new CommandParameter("@UpdateDate", dtNow));
            if (trans != null)
            {
                // 執行新增返回是否成功
                if (ExecuteNonQuery(strSql, trans) > 0)
                {
                    history.insertCaseHistory(caseid, newStatus, userId, trans);
                    return 1;
                }
            }
            IDbConnection dbConnection = OpenConnection();
            IDbTransaction tans = dbConnection.BeginTransaction();
            if (ExecuteNonQuery(strSql, tans) > 0)
            {
                history.insertCaseHistory(caseid, newStatus, userId, tans);
                tans.Commit();
                return 1;
            }
            tans.Rollback();
            return 0;
        }
        public int UpdateCaseStatusAndAfterSeizureApproved(Guid caseid, string newStatus, string userId, IDbTransaction trans = null)
        {
            DateTime dtNow = GetNowDateTime();
            CaseHistoryBIZ history = new CaseHistoryBIZ();
            string strSql = @"UPDATE [CaseMaster] SET [Status] = @Status,[AfterSeizureApproved]=@AfterSeizureApproved,
                [ModifiedUser] =@UpdateUserId, [ModifiedDate]=@UpdateDate WHERE [CaseId] = @CaseId";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("@CaseId", caseid));
            Parameter.Add(new CommandParameter("@Status", newStatus));
            Parameter.Add(new CommandParameter("AfterSeizureApproved", 1));
            Parameter.Add(new CommandParameter("@UpdateUserId", userId));
            Parameter.Add(new CommandParameter("@UpdateDate", dtNow));
            if (trans != null)
            {
                // 執行新增返回是否成功
                if (ExecuteNonQuery(strSql, trans) > 0)
                {
                    history.insertCaseHistory(caseid, newStatus, userId, trans);
                    return 1;
                }
            }
            IDbConnection dbConnection = OpenConnection();
            IDbTransaction tans = dbConnection.BeginTransaction();
            if (ExecuteNonQuery(strSql, tans) > 0)
            {
                history.insertCaseHistory(caseid, newStatus, userId, tans);
                tans.Commit();
                return 1;
            }
            tans.Rollback();
            return 0;
        }
        #endregion

        #region 判斷caseID是否存在
        public string IsCaseIdExist(string tableName, Guid caseId)
        {
            string strSql = "select count(0) from " + tableName + " where  CaseId=@CaseId";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("@CaseId", caseId));
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
        #endregion

        public DataTable GetCaseMasterByCaseIdList(List<string> caseIdList)
        {
            string strsql = @"SELECT *,(SELECT ISNULL(SUM(Amount),0) FROM [CaseAccountExternal] AS CAE WHERE CAE.CaseId = M.CaseId) AS ExtTotal FROM [CaseMaster] AS M WHERE 1 = 2 ";
            Parameter.Clear();
            for (int i = 0; i < caseIdList.Count; i++)
            {
                strsql = strsql + " OR [CaseId] = @CaseId" + i + " ";
                Parameter.Add(new CommandParameter("@CaseId" + i, caseIdList[i]));
            }
            strsql = strsql + "order by CaseNo";
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

        #region 收件處理統計表
        #region 科別派件統計表導出
        //科別派件統計表數據源
        public DataTable CaseMasterForDepartSearchList(CaseMaster model, string depart)
        {
            string sqlWhere = "";
            string sqlStr = "";
            base.Parameter.Clear();
            base.Parameter.Add(new CommandParameter("@depart", depart));

            if (!string.IsNullOrEmpty(model.CaseKind) && string.IsNullOrEmpty(model.CaseKind2))
            {
                sqlWhere += @" and M.CaseKind = @CaseKind ";
                base.Parameter.Add(new CommandParameter("@CaseKind", model.CaseKind));
            }
            else if (!string.IsNullOrEmpty(model.CaseKind) && !string.IsNullOrEmpty(model.CaseKind2))
            {
                sqlWhere += @" and (M.CaseKind+'-'+M.CaseKind2) = @CaseKind ";
                base.Parameter.Add(new CommandParameter("@CaseKind", model.CaseKind + "-" + model.CaseKind2));
            }
            if (!string.IsNullOrEmpty(model.CreatedDateStart))
            {
                sqlWhere += @" AND M.CreatedDate >= @CreatedDateStart";
                Parameter.Add(new CommandParameter("@CreatedDateStart", model.CreatedDateStart));
            }
            if (!string.IsNullOrEmpty(model.CreatedDateEnd))
            {
                string createdDateEnd = Convert.ToDateTime(UtlString.FormatDateString(model.CreatedDateEnd)).AddDays(1).ToString("yyyyMMdd");
                sqlWhere += @" AND M.CreatedDate < @CreatedDateEnd ";
                Parameter.Add(new CommandParameter("@CreatedDateEnd", createdDateEnd));
            }
            if (!string.IsNullOrEmpty(model.CloseDateStart))
            {
                sqlWhere += @" AND M.CloseDate >= @CloseDateStart";
                Parameter.Add(new CommandParameter("@CloseDateStart", model.CloseDateStart));
            }
            if (!string.IsNullOrEmpty(model.CloseDateEnd))
            {
                string closeDateEnd = Convert.ToDateTime(UtlString.FormatDateString(model.CloseDateEnd)).AddDays(1).ToString("yyyyMMdd");
                sqlWhere += @" AND M.CloseDate < @CloseDateEnd ";
                Parameter.Add(new CommandParameter("@CloseDateEnd", closeDateEnd));
            }
            if (!string.IsNullOrEmpty(model.Unit))
            {
                sqlWhere += @" and M.Unit like @Unit ";
                base.Parameter.Add(new CommandParameter("@Unit", "%" + model.Unit + "%"));
            }

            sqlStr = @"SELECT
                    (ISNULL(CaseKind,'')+'-'+ISNULL(CaseKind2,'')) AS New_CaseKind,
                    SectionName,
                    COUNT(1) AS caseCount
                    FROM CaseMaster AS M
                    INNER JOIN V_AgentAndDept AS U ON M.AgentUser = U.EmpID
                    WHERE U.SectionName = @depart " + sqlWhere + @"
                    GROUP BY (ISNULL(CaseKind,'')+'-'+ISNULL(CaseKind2,'')),U.SectionName
                    order by New_CaseKind desc ";
            DataTable dt = base.Search(sqlStr);
            if (dt != null && dt.Rows.Count > 0) return dt;
            else return new DataTable();
        }

        //各科收件統計表導出
        //public MemoryStream CaseMasterListForDepartReportExcel_NPOI(CaseMaster model)
        //{
        //    IWorkbook workbook = new HSSFWorkbook();
        //    ISheet sheet = null;
        //    ISheet sheet2 = null;
        //    ISheet sheet3 = null;
        //    DataTable dt = new DataTable();
        //    DataTable dt2 = new DataTable();
        //    DataTable dt3 = new DataTable();

        //    #region def style
        //    ICellStyle styleHead12 = workbook.CreateCellStyle();
        //    IFont font12 = workbook.CreateFont();
        //    font12.FontHeightInPoints = 12;
        //    font12.FontName = "新細明體";
        //    styleHead12.FillPattern = FillPattern.SolidForeground;
        //    styleHead12.FillForegroundColor = HSSFColor.White.Index;
        //    styleHead12.BorderTop = BorderStyle.None;
        //    styleHead12.BorderLeft = BorderStyle.None;
        //    styleHead12.BorderRight = BorderStyle.None;
        //    styleHead12.BorderBottom = BorderStyle.None;
        //    styleHead12.WrapText = true;
        //    styleHead12.Alignment = HorizontalAlignment.Center;
        //    styleHead12.VerticalAlignment = VerticalAlignment.Center;
        //    styleHead12.SetFont(font12);

        //    ICellStyle styleHead10 = workbook.CreateCellStyle();
        //    IFont font10 = workbook.CreateFont();
        //    font10.FontHeightInPoints = 10;
        //    font10.FontName = "新細明體";
        //    styleHead10.FillPattern = FillPattern.SolidForeground;
        //    styleHead10.FillForegroundColor = HSSFColor.White.Index;
        //    styleHead10.BorderTop = BorderStyle.Thin;
        //    styleHead10.BorderLeft = BorderStyle.Thin;
        //    styleHead10.BorderRight = BorderStyle.Thin;
        //    styleHead10.BorderBottom = BorderStyle.Thin;
        //    styleHead10.WrapText = true;
        //    styleHead10.Alignment = HorizontalAlignment.Left;
        //    styleHead10.VerticalAlignment = VerticalAlignment.Center;
        //    styleHead10.SetFont(font10);

        //    ICellStyle styleBodyBackGray = workbook.CreateCellStyle();
        //    IFont fontBodyBackGray = workbook.CreateFont();
        //    fontBodyBackGray.FontHeightInPoints = 10;
        //    fontBodyBackGray.FontName = "新細明體";
        //    styleBodyBackGray.FillPattern = FillPattern.SolidForeground;
        //    styleBodyBackGray.WrapText = true;
        //    styleBodyBackGray.Alignment = HorizontalAlignment.Center;
        //    styleBodyBackGray.VerticalAlignment = VerticalAlignment.Center;
        //    styleBodyBackGray.FillForegroundColor = HSSFColor.Grey25Percent.Index;
        //    styleBodyBackGray.BorderTop = BorderStyle.Thin;
        //    styleBodyBackGray.BorderLeft = BorderStyle.Thin;
        //    styleBodyBackGray.BorderRight = BorderStyle.Thin;
        //    styleBodyBackGray.BorderBottom = BorderStyle.Thin;
        //    styleBodyBackGray.SetFont(fontBodyBackGray);

        //    #endregion

        //    #region 獲取數據源
        //    //獲取人員
        //    if (model.Depart == "1" || model.Depart == "0")//*集作一科
        //    {
        //        sheet = workbook.CreateSheet("集作一科");
        //        dt = CaseMasterForDepartSearchList(model, "集作一科");//獲取查詢集作一科的案件
        //    }
        //    if (model.Depart == "2")//*集作二科
        //    {
        //        sheet = workbook.CreateSheet("集作二科");
        //        dt = CaseMasterForDepartSearchList(model, "集作二科");//獲取查詢集作二科的案件
        //    }
        //    if (model.Depart == "3")//*集作三科
        //    {
        //        sheet = workbook.CreateSheet("集作三科");
        //        dt = CaseMasterForDepartSearchList(model, "集作三科");//獲取查詢集作三科的案件
        //    }
        //    if (model.Depart == "0")//*全部
        //    {
        //        sheet2 = workbook.CreateSheet("集作二科");
        //        sheet3 = workbook.CreateSheet("集作三科");
        //        dt2 = CaseMasterForDepartSearchList(model, "集作二科");//獲取查詢集作二科的案件
        //        dt3 = CaseMasterForDepartSearchList(model, "集作三科");//獲取查詢集作三科的案件
        //    }
        //    #endregion

        //    #region title
        //    //*大標題 line0
        //    SetExcelCell(sheet, 0, 0, styleHead12, "各科收件統計表");
        //    sheet.AddMergedRegion(new CellRangeAddress(0, 1, 0, 2));

        //    //*查詢條件 line1
        //    SetExcelCell(sheet, 2, 0, styleHead12, "收件日期：");
        //    sheet.AddMergedRegion(new CellRangeAddress(2, 2, 0, 0));
        //    SetExcelCell(sheet, 2, 1, styleHead12, model.CreatedDateStart + "~" + model.CreatedDateEnd);
        //    sheet.AddMergedRegion(new CellRangeAddress(2, 2, 1, 1));
        //    SetExcelCell(sheet, 2, 2, styleHead12, "結案日期：");
        //    sheet.AddMergedRegion(new CellRangeAddress(2, 2, 2, 2));
        //    SetExcelCell(sheet, 2, 3, styleHead12, model.CloseDateStart + "~" + model.CloseDateEnd);
        //    sheet.AddMergedRegion(new CellRangeAddress(2, 2, 3, 3));

        //    //*結果集表頭 line2
        //    SetExcelCell(sheet, 3, 0, styleBodyBackGray, "科別");
        //    SetExcelCell(sheet, 3, 1, styleBodyBackGray, "案件類型");
        //    SetExcelCell(sheet, 3, 2, styleBodyBackGray, "收件件數");
        //    #endregion

        //    int count = 3;//行數
        //    #region body
        //    for (int iRow = 0; iRow < dt.Rows.Count; iRow++)
        //    {
        //        if (dt.Rows[iRow]["caseCount"].ToString() != "0")
        //        {
        //            count = count + 1;
        //            if (model.Depart == "1" || model.Depart == "0")
        //            {
        //                SetExcelCell(sheet, count, 0, styleHead10, "集作一科");
        //            }
        //            if (model.Depart == "2")
        //            {
        //                SetExcelCell(sheet, count, 0, styleHead10, "集作二科");
        //            }
        //            if (model.Depart == "3")
        //            {
        //                SetExcelCell(sheet, count, 0, styleHead10, "集作三科");
        //            }
        //            SetExcelCell(sheet, count, 1, styleHead10, Convert.ToString(dt.Rows[iRow]["New_CaseKind"]));
        //            sheet.SetColumnWidth(1, 100 * 70);
        //            SetExcelCell(sheet, count, 2, styleHead10, Convert.ToString(dt.Rows[iRow]["caseCount"]));
        //        }
        //    }
        //    #endregion

        //    if (model.Depart == "0")
        //    {
        //        #region title2
        //        //*大標題 line0
        //        SetExcelCell(sheet2, 0, 0, styleHead12, "各科收件統計表");
        //        sheet2.AddMergedRegion(new CellRangeAddress(0, 1, 0, 2));

        //        //*查詢條件 line1
        //        SetExcelCell(sheet2, 2, 0, styleHead12, "收件日期：");
        //        sheet2.AddMergedRegion(new CellRangeAddress(2, 2, 0, 0));
        //        SetExcelCell(sheet2, 2, 1, styleHead12, model.CreatedDateStart + "~" + model.CreatedDateEnd);
        //        sheet2.AddMergedRegion(new CellRangeAddress(2, 2, 1, 1));
        //        SetExcelCell(sheet2, 2, 2, styleHead12, "結案日期：");
        //        sheet2.AddMergedRegion(new CellRangeAddress(2, 2, 2, 2));
        //        SetExcelCell(sheet2, 2, 3, styleHead12, model.CloseDateStart + "~" + model.CloseDateEnd);
        //        sheet2.AddMergedRegion(new CellRangeAddress(2, 2, 3, 3));

        //        //*結果集表頭 line2
        //        SetExcelCell(sheet2, 3, 0, styleBodyBackGray, "科別");
        //        sheet2.AddMergedRegion(new CellRangeAddress(3, 3, 0, 0));
        //        SetExcelCell(sheet2, 3, 1, styleBodyBackGray, "案件類型");
        //        sheet2.AddMergedRegion(new CellRangeAddress(3, 3, 1, 1));
        //        SetExcelCell(sheet2, 3, 2, styleBodyBackGray, "收件件數");
        //        sheet2.AddMergedRegion(new CellRangeAddress(3, 3, 2, 2));
        //        #endregion
        //        #region body2
        //        count = 3;
        //        for (int iRow = 0; iRow < dt2.Rows.Count; iRow++)
        //        {
        //            if (dt2.Rows[iRow]["caseCount"].ToString() != "0")
        //            {
        //                count = count + 1;
        //                SetExcelCell(sheet2, count, 0, styleHead10, "集作二科");
        //                SetExcelCell(sheet2, count, 1, styleHead10, Convert.ToString(dt2.Rows[iRow]["New_CaseKind"]));
        //                sheet2.SetColumnWidth(1, 100 * 70);
        //                SetExcelCell(sheet2, count, 2, styleHead10, Convert.ToString(dt2.Rows[iRow]["caseCount"]));
        //            }
        //        }
        //        #endregion

        //        #region title3
        //        //*大標題 line0
        //        SetExcelCell(sheet3, 0, 0, styleHead12, "各科收件統計表");
        //        sheet3.AddMergedRegion(new CellRangeAddress(0, 1, 0, 2));

        //        //*查詢條件 line1
        //        SetExcelCell(sheet3, 2, 0, styleHead12, "收件日期：");
        //        sheet3.AddMergedRegion(new CellRangeAddress(2, 2, 0, 0));
        //        SetExcelCell(sheet3, 2, 1, styleHead12, model.CreatedDateStart + "~" + model.CreatedDateEnd);
        //        sheet3.AddMergedRegion(new CellRangeAddress(2, 2, 1, 1));
        //        SetExcelCell(sheet3, 2, 2, styleHead12, "結案日期：");
        //        sheet3.AddMergedRegion(new CellRangeAddress(2, 2, 2, 2));
        //        SetExcelCell(sheet3, 2, 3, styleHead12, model.CloseDateStart + "~" + model.CloseDateEnd);
        //        sheet3.AddMergedRegion(new CellRangeAddress(2, 2, 3, 3));


        //        //*結果集表頭 line2
        //        SetExcelCell(sheet3, 3, 0, styleBodyBackGray, "科別");
        //        sheet3.AddMergedRegion(new CellRangeAddress(3, 3, 0, 0));
        //        SetExcelCell(sheet3, 3, 1, styleBodyBackGray, "案件類型");
        //        sheet3.AddMergedRegion(new CellRangeAddress(3, 3, 1, 1));
        //        SetExcelCell(sheet3, 3, 2, styleBodyBackGray, "收件件數");
        //        sheet3.AddMergedRegion(new CellRangeAddress(3, 3, 2, 2));
        //        #endregion
        //        #region body3
        //        count = 3;
        //        for (int iRow = 0; iRow < dt3.Rows.Count; iRow++)
        //        {
        //            if (dt3.Rows[iRow]["caseCount"].ToString() != "0")
        //            {
        //                count = count + 1;
        //                SetExcelCell(sheet3, count, 0, styleHead10, "集作三科");
        //                SetExcelCell(sheet3, count, 1, styleHead10, Convert.ToString(dt3.Rows[iRow]["New_CaseKind"]));
        //                sheet3.SetColumnWidth(1, 100 * 70);
        //                SetExcelCell(sheet3, count, 2, styleHead10, Convert.ToString(dt3.Rows[iRow]["caseCount"]));
        //            }
        //        }
        //        #endregion
        //    }

        //    MemoryStream ms = new MemoryStream();
        //    workbook.Write(ms);
        //    ms.Flush();
        //    ms.Position = 0;
        //    workbook = null;
        //    return ms;
        //}

        //20170811 RC RQ-2015-019666-020 派件至跨單位(原產生邏輯程式寫死，組織異動必出問題，改為參數) 宏祥 add start
        //各科收件統計表導出
        public MemoryStream CaseMasterListForDepartReportExcel_NPOI(CaseMaster model, IList<PARMCode> listCode)
        {
           IWorkbook workbook = new HSSFWorkbook();
           int Departmentcount = listCode.Count();
           int count = 3;//行數

           #region def style
           ICellStyle styleHead12 = workbook.CreateCellStyle();
           IFont font12 = workbook.CreateFont();
           font12.FontHeightInPoints = 12;
           font12.FontName = "新細明體";
           styleHead12.FillPattern = FillPattern.SolidForeground;
           styleHead12.FillForegroundColor = HSSFColor.White.Index;
           styleHead12.BorderTop = BorderStyle.None;
           styleHead12.BorderLeft = BorderStyle.None;
           styleHead12.BorderRight = BorderStyle.None;
           styleHead12.BorderBottom = BorderStyle.None;
           styleHead12.WrapText = true;
           styleHead12.Alignment = HorizontalAlignment.Center;
           styleHead12.VerticalAlignment = VerticalAlignment.Center;
           styleHead12.SetFont(font12);

           ICellStyle styleHead10 = workbook.CreateCellStyle();
           IFont font10 = workbook.CreateFont();
           font10.FontHeightInPoints = 10;
           font10.FontName = "新細明體";
           styleHead10.FillPattern = FillPattern.SolidForeground;
           styleHead10.FillForegroundColor = HSSFColor.White.Index;
           styleHead10.BorderTop = BorderStyle.Thin;
           styleHead10.BorderLeft = BorderStyle.Thin;
           styleHead10.BorderRight = BorderStyle.Thin;
           styleHead10.BorderBottom = BorderStyle.Thin;
           styleHead10.WrapText = true;
           styleHead10.Alignment = HorizontalAlignment.Left;
           styleHead10.VerticalAlignment = VerticalAlignment.Center;
           styleHead10.SetFont(font10);

           ICellStyle styleBodyBackGray = workbook.CreateCellStyle();
           IFont fontBodyBackGray = workbook.CreateFont();
           fontBodyBackGray.FontHeightInPoints = 10;
           fontBodyBackGray.FontName = "新細明體";
           styleBodyBackGray.FillPattern = FillPattern.SolidForeground;
           styleBodyBackGray.WrapText = true;
           styleBodyBackGray.Alignment = HorizontalAlignment.Center;
           styleBodyBackGray.VerticalAlignment = VerticalAlignment.Center;
           styleBodyBackGray.FillForegroundColor = HSSFColor.Grey25Percent.Index;
           styleBodyBackGray.BorderTop = BorderStyle.Thin;
           styleBodyBackGray.BorderLeft = BorderStyle.Thin;
           styleBodyBackGray.BorderRight = BorderStyle.Thin;
           styleBodyBackGray.BorderBottom = BorderStyle.Thin;
           styleBodyBackGray.SetFont(fontBodyBackGray);

           #endregion

           //判斷科別搜尋條件
           if (model.Depart != "0")
           {
              for (int k = 0; k < Departmentcount; k++)
              {
                 if (model.Depart == (k + 1).ToString())
                 {
                    ISheet sheet = null;
                    DataTable dt = new DataTable();

                    sheet = workbook.CreateSheet(listCode[k].CodeDesc);
                    dt = CaseMasterForDepartSearchList(model, listCode[k].CodeDesc);//獲取查詢科別的案件

                    #region title
                    //*大標題 line0
                    SetExcelCell(sheet, 0, 0, styleHead12, "各科收件統計表");
                    sheet.AddMergedRegion(new CellRangeAddress(0, 1, 0, 2));

                    //*查詢條件 line1
                    SetExcelCell(sheet, 2, 0, styleHead12, "收件日期：");
                    sheet.AddMergedRegion(new CellRangeAddress(2, 2, 0, 0));
                    SetExcelCell(sheet, 2, 1, styleHead12, model.CreatedDateStart + "~" + model.CreatedDateEnd);
                    sheet.AddMergedRegion(new CellRangeAddress(2, 2, 1, 1));
                    SetExcelCell(sheet, 2, 2, styleHead12, "結案日期：");
                    sheet.AddMergedRegion(new CellRangeAddress(2, 2, 2, 2));
                    SetExcelCell(sheet, 2, 3, styleHead12, model.CloseDateStart + "~" + model.CloseDateEnd);
                    sheet.AddMergedRegion(new CellRangeAddress(2, 2, 3, 3));

                    //*結果集表頭 line2
                    SetExcelCell(sheet, 3, 0, styleBodyBackGray, "科別");
                    SetExcelCell(sheet, 3, 1, styleBodyBackGray, "案件類型");
                    SetExcelCell(sheet, 3, 2, styleBodyBackGray, "收件件數");
                    #endregion

                    #region body
                    for (int iRow = 0; iRow < dt.Rows.Count; iRow++)
                    {
                       if (dt.Rows[iRow]["caseCount"].ToString() != "0")
                       {
                          count = count + 1;
                          SetExcelCell(sheet, count, 0, styleHead10, listCode[k].CodeDesc);
                          SetExcelCell(sheet, count, 1, styleHead10, Convert.ToString(dt.Rows[iRow]["New_CaseKind"]));
                          sheet.SetColumnWidth(1, 100 * 70);
                          SetExcelCell(sheet, count, 2, styleHead10, Convert.ToString(dt.Rows[iRow]["caseCount"]));
                       }
                    }
                    #endregion
                 }
              }
           }
           else
           {
              for (int k = 0; k < Departmentcount; k++)
              {
                 ISheet sheet = null;
                 DataTable dt = new DataTable();

                 sheet = workbook.CreateSheet(listCode[k].CodeDesc);
                 dt = CaseMasterForDepartSearchList(model, listCode[k].CodeDesc);//獲取查詢科別的案件

                 #region title
                 //*大標題 line0
                 SetExcelCell(sheet, 0, 0, styleHead12, "各科收件統計表");
                 sheet.AddMergedRegion(new CellRangeAddress(0, 1, 0, 2));

                 //*查詢條件 line1
                 SetExcelCell(sheet, 2, 0, styleHead12, "收件日期：");
                 sheet.AddMergedRegion(new CellRangeAddress(2, 2, 0, 0));
                 SetExcelCell(sheet, 2, 1, styleHead12, model.CreatedDateStart + "~" + model.CreatedDateEnd);
                 sheet.AddMergedRegion(new CellRangeAddress(2, 2, 1, 1));
                 SetExcelCell(sheet, 2, 2, styleHead12, "結案日期：");
                 sheet.AddMergedRegion(new CellRangeAddress(2, 2, 2, 2));
                 SetExcelCell(sheet, 2, 3, styleHead12, model.CloseDateStart + "~" + model.CloseDateEnd);
                 sheet.AddMergedRegion(new CellRangeAddress(2, 2, 3, 3));

                 //*結果集表頭 line2
                 SetExcelCell(sheet, 3, 0, styleBodyBackGray, "科別");
                 sheet.AddMergedRegion(new CellRangeAddress(3, 3, 0, 0));
                 SetExcelCell(sheet, 3, 1, styleBodyBackGray, "案件類型");
                 sheet.AddMergedRegion(new CellRangeAddress(3, 3, 1, 1));
                 SetExcelCell(sheet, 3, 2, styleBodyBackGray, "收件件數");
                 sheet.AddMergedRegion(new CellRangeAddress(3, 3, 2, 2));
                 #endregion

                 #region body
                 count = 3;
                 for (int iRow = 0; iRow < dt.Rows.Count; iRow++)
                 {
                    if (dt.Rows[iRow]["caseCount"].ToString() != "0")
                    {
                       count = count + 1;
                       SetExcelCell(sheet, count, 0, styleHead10, listCode[k].CodeDesc);
                       SetExcelCell(sheet, count, 1, styleHead10, Convert.ToString(dt.Rows[iRow]["New_CaseKind"]));
                       sheet.SetColumnWidth(1, 100 * 70);
                       SetExcelCell(sheet, count, 2, styleHead10, Convert.ToString(dt.Rows[iRow]["caseCount"]));
                    }
                 }
                 #endregion
              }
           }

           MemoryStream ms = new MemoryStream();
           workbook.Write(ms);
           ms.Flush();
           ms.Position = 0;
           workbook = null;
           return ms;
        }
        #endregion
        //20170811 RC RQ-2015-019666-020 派件至跨單位(原產生邏輯程式寫死，組織異動必出問題，改為參數) 宏祥 add end

        #region 經辦收件統計表導出
        //經辦收件獲取數據源
        public DataTable GetCaseMasterList(CaseMaster model, string depart)
        {
            string sqlWhere = "";
            string sqlStr = "";
            base.Parameter.Clear();
            base.Parameter.Add(new CommandParameter("@depart", depart));

            if (!string.IsNullOrEmpty(model.CaseKind) && string.IsNullOrEmpty(model.CaseKind2))
            {
                sqlWhere += @" and CaseKind = @CaseKind ";
                base.Parameter.Add(new CommandParameter("@CaseKind", model.CaseKind));
            }
            else if (!string.IsNullOrEmpty(model.CaseKind) && !string.IsNullOrEmpty(model.CaseKind2))
            {
                sqlWhere += @" and (CaseKind+'-'+CaseKind2) = @CaseKind ";
                base.Parameter.Add(new CommandParameter("@CaseKind", model.CaseKind + "-" + model.CaseKind2));
            }
            if (!string.IsNullOrEmpty(model.CreatedDateStart))
            {
                sqlWhere += @" AND CreatedDate >= @CreatedDateStart";
                Parameter.Add(new CommandParameter("@CreatedDateStart", model.CreatedDateStart));
            }
            if (!string.IsNullOrEmpty(model.CreatedDateEnd))
            {
                string CreatedDateEnd = Convert.ToDateTime(UtlString.FormatDateString(model.CreatedDateEnd)).AddDays(1).ToString("yyyyMMdd");
                sqlWhere += @" AND CreatedDate < @CreatedDateEnd ";
                Parameter.Add(new CommandParameter("@CreatedDateEnd", CreatedDateEnd));
            }
            if (!string.IsNullOrEmpty(model.CloseDateStart))
            {
                sqlWhere += @" AND CloseDate >= @CloseDateStart";
                Parameter.Add(new CommandParameter("@CloseDateStart", model.CloseDateStart));
            }
            if (!string.IsNullOrEmpty(model.CloseDateEnd))
            {
                string closeDateEnd = Convert.ToDateTime(UtlString.FormatDateString(model.CloseDateEnd)).AddDays(1).ToString("yyyyMMdd");
                sqlWhere += @" AND CloseDate < @CloseDateEnd ";
                Parameter.Add(new CommandParameter("@CloseDateEnd", closeDateEnd));
            }
            if (!string.IsNullOrEmpty(model.Unit))
            {
                sqlWhere += @" and Unit like @Unit ";
                base.Parameter.Add(new CommandParameter("@Unit", "%" + model.Unit + "%"));
            }
            sqlStr = @" select A.*, employee.EmpName,(select count(1) from CaseMaster where AgentUser=a.AgentUser " + sqlWhere + @") as UserCount
                                from (select  (CaseKind+'-'+CaseKind2) as New_CaseKind,AgentUser, count(1) as case_num from CaseMaster  
                                 where 1=1 " + sqlWhere + @"
                                group by (CaseKind+'-'+CaseKind2), AgentUser) as A 
                                inner join (SELECT P.* FROM  [LDAPDepartment] d
                                inner join ldapEmployee p on P.DepDN LIKE '%'+d.depid + '%'
                                where  d.DepName in (@depart)) as employee on a.AgentUser = employee.EmpID
                                order by New_CaseKind, EmpID ";
            DataTable dt = base.Search(sqlStr);
            return dt;
        }


        ////經辦收件統計表導出
        //public MemoryStream CaseMasterListReportExcel_NPOI(CaseMaster model)
        //{
        //    IWorkbook workbook = new HSSFWorkbook();
        //    ISheet sheet = null;
        //    ISheet sheet2 = null;
        //    ISheet sheet3 = null;
        //    Dictionary<string, string> dicldapList = new Dictionary<string, string>();
        //    Dictionary<string, string> dicldapList2 = new Dictionary<string, string>();
        //    Dictionary<string, string> dicldapList3 = new Dictionary<string, string>();
        //    DataTable dtCase = new DataTable();//導出資料
        //    DataTable dtCase2 = new DataTable();
        //    DataTable dtCase3 = new DataTable();

        //    int rowscountExcelresult = 0;//合計參數
        //    string caseExcel = "";//案件類型
        //    int rowsExcel = 2;//行數
        //    int rowscountExcel = 0;//最後一列合計
        //    int rowstatolExcel = 0;//總合計      
        //    int sort = 1;//記錄每個名字在哪一格

        //    #region def style
        //    ICellStyle styleHead12 = workbook.CreateCellStyle();
        //    IFont font12 = workbook.CreateFont();
        //    font12.FontHeightInPoints = 12;
        //    font12.FontName = "新細明體";
        //    styleHead12.FillPattern = FillPattern.SolidForeground;
        //    styleHead12.FillForegroundColor = HSSFColor.White.Index;
        //    styleHead12.BorderTop = BorderStyle.None;
        //    styleHead12.BorderLeft = BorderStyle.None;
        //    styleHead12.BorderRight = BorderStyle.None;
        //    styleHead12.BorderBottom = BorderStyle.None;
        //    styleHead12.WrapText = true;
        //    styleHead12.Alignment = HorizontalAlignment.Center;
        //    styleHead12.VerticalAlignment = VerticalAlignment.Center;
        //    styleHead12.SetFont(font12);

        //    ICellStyle styleHead10 = workbook.CreateCellStyle();
        //    IFont font10 = workbook.CreateFont();
        //    font10.FontHeightInPoints = 10;
        //    font10.FontName = "新細明體";
        //    styleHead10.FillPattern = FillPattern.SolidForeground;
        //    styleHead10.FillForegroundColor = HSSFColor.White.Index;
        //    styleHead10.BorderTop = BorderStyle.Thin;
        //    styleHead10.BorderLeft = BorderStyle.Thin;
        //    styleHead10.BorderRight = BorderStyle.Thin;
        //    styleHead10.BorderBottom = BorderStyle.Thin;
        //    styleHead10.WrapText = true;
        //    styleHead10.Alignment = HorizontalAlignment.Left;
        //    styleHead10.VerticalAlignment = VerticalAlignment.Center;
        //    styleHead10.SetFont(font10);

        //    ICellStyle styleBodyBackGray = workbook.CreateCellStyle();
        //    IFont fontBodyBackGray = workbook.CreateFont();
        //    fontBodyBackGray.FontHeightInPoints = 10;
        //    fontBodyBackGray.FontName = "新細明體";
        //    styleBodyBackGray.FillPattern = FillPattern.SolidForeground;
        //    styleBodyBackGray.WrapText = true;
        //    styleBodyBackGray.Alignment = HorizontalAlignment.Left;
        //    styleBodyBackGray.VerticalAlignment = VerticalAlignment.Center;
        //    styleBodyBackGray.FillForegroundColor = HSSFColor.Grey25Percent.Index;
        //    styleBodyBackGray.BorderTop = BorderStyle.Thin;
        //    styleBodyBackGray.BorderLeft = BorderStyle.Thin;
        //    styleBodyBackGray.BorderRight = BorderStyle.Thin;
        //    styleBodyBackGray.BorderBottom = BorderStyle.Thin;
        //    styleBodyBackGray.SetFont(fontBodyBackGray);
        //    #endregion

        //    #region 單獨科別的數據源(科別及案件資料)
        //    //獲取人員
        //    if (model.Depart == "1" || model.Depart == "0")//* 集作一科
        //    {
        //        sheet = workbook.CreateSheet("集作一科");
        //        dtCase = GetCaseMasterList(model, "集作一科");
        //        foreach (DataRow dr in dtCase.Rows)//去重複的人員名稱
        //        {
        //            if (!dicldapList.Keys.Contains(dr["AgentUser"].ToString()))
        //            {
        //                dicldapList.Add(dr["AgentUser"].ToString(), dr["EmpName"].ToString() + "|" + sort);
        //                sort++;
        //            }
        //        }
        //        if (dicldapList.Count > 3)
        //        {
        //            SetExcelCell(sheet, 1, dicldapList.Count + 1, styleHead12, "集作一科");
        //            sheet.AddMergedRegion(new CellRangeAddress(1, 1, dicldapList.Count + 1, dicldapList.Count + 1));
        //        }
        //        else
        //        {
        //            SetExcelCell(sheet, 1, 5, styleHead12, "集作一科");
        //            sheet.AddMergedRegion(new CellRangeAddress(1, 1, 5, 5));
        //            sheet.SetColumnWidth(5, 100 * 30);
        //        }
        //    }
        //    if (model.Depart == "2")//* 集作二科
        //    {
        //        sheet = workbook.CreateSheet("集作二科");
        //        dtCase = GetCaseMasterList(model, "集作二科");
        //        foreach (DataRow dr in dtCase.Rows)//去重複的人員名稱
        //        {
        //            if (!dicldapList.Keys.Contains(dr["AgentUser"].ToString()))
        //            {
        //                dicldapList.Add(dr["AgentUser"].ToString(), dr["EmpName"].ToString() + "|" + sort);
        //                sort++;
        //            }
        //        }
        //        if (dicldapList.Count > 3)
        //        {
        //            SetExcelCell(sheet, 1, dicldapList.Count + 1, styleHead12, "集作二科");
        //            sheet.AddMergedRegion(new CellRangeAddress(1, 1, dicldapList.Count + 1, dicldapList.Count + 1));
        //        }
        //        else
        //        {
        //            SetExcelCell(sheet, 1, 5, styleHead12, "集作二科");
        //            sheet.AddMergedRegion(new CellRangeAddress(1, 1, 5, 5));
        //            sheet.SetColumnWidth(5, 100 * 30);
        //        }

        //    }
        //    if (model.Depart == "3")//* 集作三科
        //    {
        //        sheet = workbook.CreateSheet("集作三科");
        //        dtCase = GetCaseMasterList(model, "集作三科");
        //        foreach (DataRow dr in dtCase.Rows)//去重複的人員名稱
        //        {
        //            if (!dicldapList.Keys.Contains(dr["AgentUser"].ToString()))
        //            {
        //                dicldapList.Add(dr["AgentUser"].ToString(), dr["EmpName"].ToString() + "|" + sort);
        //                sort++;
        //            }
        //        }
        //        if (dicldapList.Count > 3)
        //        {
        //            SetExcelCell(sheet, 1, dicldapList.Count + 1, styleHead12, "集作三科");
        //            sheet.AddMergedRegion(new CellRangeAddress(1, 1, dicldapList.Count + 1, dicldapList.Count + 1));
        //        }
        //        else
        //        {
        //            SetExcelCell(sheet, 1, 5, styleHead12, "集作三科");
        //            sheet.AddMergedRegion(new CellRangeAddress(1, 1, 5, 5));
        //            sheet.SetColumnWidth(5, 100 * 30);
        //        }

        //    }
        //    if (model.Depart == "0")//* 全部
        //    {
        //        sheet2 = workbook.CreateSheet("集作二科");
        //        sheet3 = workbook.CreateSheet("集作三科");
        //        dtCase2 = GetCaseMasterList(model, "集作二科");//獲取查詢集作二科的案件
        //        dtCase3 = GetCaseMasterList(model, "集作三科");//獲取查詢集作三科的案件
        //        sort = 1;
        //        foreach (DataRow dr in dtCase2.Rows)//去重複的人員名稱
        //        {
        //            if (!dicldapList2.Keys.Contains(dr["AgentUser"].ToString()))
        //            {
        //                dicldapList2.Add(dr["AgentUser"].ToString(), dr["EmpName"].ToString() + "|" + sort);
        //                sort++;
        //            }
        //        }
        //        sort = 1;
        //        foreach (DataRow dr in dtCase3.Rows)//去重複的人員名稱
        //        {
        //            if (!dicldapList3.Keys.Contains(dr["AgentUser"].ToString()))
        //            {
        //                dicldapList3.Add(dr["AgentUser"].ToString(), dr["EmpName"].ToString() + "|" + sort);
        //                sort++;
        //            }
        //        }
        //    }
        //    #endregion

        //    string caseKind = "";//*去重複
        //    int rows = 2;//定義行數

        //    #region title
        //    //*大標題 line0
        //    SetExcelCell(sheet, 0, 0, styleHead12, "經辦收件統計表");
        //    sheet.AddMergedRegion(new CellRangeAddress(0, 0, 0, dicldapList.Count + 1));

        //    //*查詢條件 line1
        //    SetExcelCell(sheet, 1, 0, styleHead12, "收件日期：");
        //    sheet.AddMergedRegion(new CellRangeAddress(1, 1, 0, 0));
        //    SetExcelCell(sheet, 1, 1, styleHead12, model.CreatedDateStart + "~" + model.CreatedDateEnd);
        //    sheet.AddMergedRegion(new CellRangeAddress(1, 1, 1, 1));
        //    SetExcelCell(sheet, 1, 2, styleHead12, "結案日期：");
        //    sheet.AddMergedRegion(new CellRangeAddress(1, 1, 2, 2));
        //    SetExcelCell(sheet, 1, 3, styleHead12, model.CloseDateStart + "~" + model.CloseDateEnd);
        //    sheet.AddMergedRegion(new CellRangeAddress(1, 1, 3, 3));

        //    if (dicldapList.Count > 3)
        //    {
        //        SetExcelCell(sheet, 1, dicldapList.Count - 1, styleHead12, "部門別/科別");
        //        sheet.AddMergedRegion(new CellRangeAddress(1, 1, dicldapList.Count - 1, dicldapList.Count));
        //    }
        //    else
        //    {
        //        SetExcelCell(sheet, 1, 4, styleHead12, "部門別/科別");
        //        sheet.AddMergedRegion(new CellRangeAddress(1, 1, 4, 4));
        //        sheet.SetColumnWidth(4, 100 * 30);
        //    }


        //    //*結果集表頭 line2
        //    SetExcelCell(sheet, 2, 0, styleBodyBackGray, "處理人員");
        //    sheet.AddMergedRegion(new CellRangeAddress(2, 2, 0, 0));
        //    sheet.SetColumnWidth(0, 100 * 50);

        //    int agentrows = 1;
        //    foreach (var item in dicldapList)
        //    {
        //        SetExcelCell(sheet, 2, agentrows, styleHead10, item.Value.Split('|')[0].ToString());
        //        sheet.AddMergedRegion(new CellRangeAddress(2, 2, agentrows, agentrows));
        //        agentrows++;
        //    }
        //    SetExcelCell(sheet, 2, dicldapList.Count + 1, styleHead10, "合計");
        //    sheet.AddMergedRegion(new CellRangeAddress(2, 2, dicldapList.Count + 1, dicldapList.Count + 1));

        //    //*扣押案件類型 line3-lineN 
        //    for (int i = 0; i < dtCase.Rows.Count; i++)
        //    {   //去重複的案件類型
        //        if (caseKind != dtCase.Rows[i]["New_CaseKind"].ToString())
        //        {
        //            rows = rows + 1;
        //            SetExcelCell(sheet, rows, 0, styleBodyBackGray, dtCase.Rows[i]["New_CaseKind"].ToString());
        //            sheet.AddMergedRegion(new CellRangeAddress(rows, rows, 0, 0));
        //            SetExcelCell(sheet, rows, dicldapList.Count + 1, styleHead10, "0");
        //            sheet.AddMergedRegion(new CellRangeAddress(rows, rows, dicldapList.Count + 1, dicldapList.Count + 1));
        //            caseKind = dtCase.Rows[i]["New_CaseKind"].ToString();
        //            for (int j = 0; j < dicldapList.Count; j++)//*初始表格賦初值 
        //            {
        //                SetExcelCell(sheet, rows, j + 1, styleHead10, "0");
        //                sheet.AddMergedRegion(new CellRangeAddress(rows, rows, j + 1, j + 1));
        //            }
        //        }
        //    }

        //    //*合計 lineLast
        //    SetExcelCell(sheet, rows + 1, 0, styleBodyBackGray, "合計");
        //    sheet.AddMergedRegion(new CellRangeAddress(rows + 1, rows + 1, 0, 0));
        //    for (int j = 0; j < dicldapList.Count; j++)//*給最後一行合計初始表格賦初值 
        //    {
        //        SetExcelCell(sheet, rows + 1, j + 1, styleHead10, "0");
        //        sheet.AddMergedRegion(new CellRangeAddress(rows + 1, rows + 1, j + 1, j + 1));
        //    }
        //    #endregion

        //    #region  body
        //    for (int iRow = 0; iRow < dtCase.Rows.Count; iRow++)//根據案件類型進行循環
        //    {
        //        foreach (var item in dicldapList)//循環excel中的列
        //        {
        //            int irows = Convert.ToInt32(item.Value.Split('|')[1]);
        //            if (caseExcel == dtCase.Rows[iRow]["New_CaseKind"].ToString())//重複同一案件類型的數據
        //            {
        //                if (item.Key == dtCase.Rows[iRow]["AgentUser"].ToString())
        //                {
        //                    SetExcelCell(sheet, rowsExcel, irows, styleHead10, dtCase.Rows[iRow]["case_num"].ToString());
        //                    rowscountExcelresult = Convert.ToInt32(dtCase.Rows[iRow]["case_num"].ToString());//每格資料
        //                    SetExcelCell(sheet, rows + 1, irows, styleHead10, dtCase.Rows[iRow]["UserCount"].ToString());//最後一行合計
        //                    rowscountExcel += rowscountExcelresult;
        //                    rowstatolExcel += rowscountExcelresult;
        //                }
        //                SetExcelCell(sheet, rowsExcel, dicldapList.Count + 1, styleHead10, rowscountExcel.ToString());//最後一列合計
        //            }
        //            else//不重複的案件類型
        //            {
        //                rowscountExcel = 0;
        //                rowsExcel = rowsExcel + 1;
        //                if (dtCase.Rows[iRow]["AgentUser"].ToString() == item.Key)
        //                {
        //                    SetExcelCell(sheet, rowsExcel, irows, styleHead10, dtCase.Rows[iRow]["case_num"].ToString());
        //                    rowscountExcelresult = Convert.ToInt32(dtCase.Rows[iRow]["case_num"].ToString());//第一條不重複的數據儲存下值
        //                    SetExcelCell(sheet, rows + 1, irows, styleHead10, dtCase.Rows[iRow]["UserCount"].ToString());//最後一行合計
        //                    rowscountExcel += rowscountExcelresult;
        //                    rowstatolExcel += rowscountExcelresult;
        //                }
        //                caseExcel = dtCase.Rows[iRow]["New_CaseKind"].ToString();
        //                SetExcelCell(sheet, rowsExcel, dicldapList.Count + 1, styleHead10, rowscountExcel.ToString());//最後一列合計
        //            }
        //        }
        //    }
        //    SetExcelCell(sheet, rows + 1, dicldapList.Count + 1, styleHead10, rowstatolExcel.ToString());//總合計
        //    #endregion

        //    if (model.Depart == "0")//* 全部
        //    {
        //        #region title2
        //        //*大標題 line0
        //        SetExcelCell(sheet2, 0, 0, styleHead12, "經辦收件統計表");
        //        sheet2.AddMergedRegion(new CellRangeAddress(0, 0, 0, dicldapList2.Count + 1));

        //        //*查詢條件 line1
        //        SetExcelCell(sheet2, 1, 0, styleHead12, "收件日期：");
        //        sheet2.AddMergedRegion(new CellRangeAddress(1, 1, 0, 0));
        //        SetExcelCell(sheet2, 1, 1, styleHead12, model.CreatedDateStart + "~" + model.CreatedDateEnd);
        //        sheet2.AddMergedRegion(new CellRangeAddress(1, 1, 1, 1));
        //        SetExcelCell(sheet2, 1, 2, styleHead12, "結案日期：");
        //        sheet2.AddMergedRegion(new CellRangeAddress(1, 1, 2, 2));
        //        SetExcelCell(sheet2, 1, 3, styleHead12, model.CloseDateStart + "~" + model.CloseDateEnd);
        //        sheet2.AddMergedRegion(new CellRangeAddress(1, 1, 3, 3));

        //        if (dicldapList2.Count > 3)
        //        {
        //            SetExcelCell(sheet2, 1, dicldapList2.Count - 1, styleHead12, "部門別/科別");
        //            sheet2.AddMergedRegion(new CellRangeAddress(1, 1, dicldapList2.Count - 1, dicldapList2.Count));
        //            SetExcelCell(sheet2, 1, dicldapList2.Count + 1, styleHead12, "集作二科");
        //            sheet2.AddMergedRegion(new CellRangeAddress(1, 1, dicldapList2.Count + 1, dicldapList2.Count + 1));
        //        }
        //        else
        //        {
        //            SetExcelCell(sheet2, 1, 4, styleHead12, "部門別/科別");
        //            sheet2.AddMergedRegion(new CellRangeAddress(1, 1, 4, 4));
        //            sheet2.SetColumnWidth(4, 100 * 30);
        //            SetExcelCell(sheet2, 1, 5, styleHead12, "集作二科");
        //            sheet2.AddMergedRegion(new CellRangeAddress(1, 1, 5, 5));
        //        }

        //        //*結果集表頭 line2
        //        SetExcelCell(sheet2, 2, 0, styleBodyBackGray, "處理人員");
        //        sheet2.AddMergedRegion(new CellRangeAddress(2, 2, 0, 0));
        //        sheet2.SetColumnWidth(0, 100 * 50);

        //        agentrows = 1;
        //        foreach (var item in dicldapList2)
        //        {
        //            SetExcelCell(sheet2, 2, agentrows, styleHead10, item.Value.Split('|')[0].ToString());
        //            sheet2.AddMergedRegion(new CellRangeAddress(2, 2, agentrows, agentrows));
        //            agentrows++;
        //        }
        //        SetExcelCell(sheet2, 2, dicldapList2.Count + 1, styleHead10, "合計");
        //        sheet2.AddMergedRegion(new CellRangeAddress(2, 2, dicldapList2.Count + 1, dicldapList2.Count + 1));

        //        //*扣押案件類型 line3-lineN 
        //        caseKind = "";//*去重複
        //        rows = 2;//定義行數
        //        for (int i = 0; i < dtCase2.Rows.Count; i++)
        //        {
        //            if (caseKind != dtCase2.Rows[i]["New_CaseKind"].ToString())
        //            {
        //                rows = rows + 1;
        //                SetExcelCell(sheet2, rows, 0, styleBodyBackGray, dtCase2.Rows[i]["New_CaseKind"].ToString());
        //                sheet2.AddMergedRegion(new CellRangeAddress(rows, rows, 0, 0));

        //                SetExcelCell(sheet2, rows, dicldapList2.Count + 1, styleHead10, "0");
        //                sheet2.AddMergedRegion(new CellRangeAddress(rows, rows, dicldapList2.Count + 1, dicldapList2.Count + 1));

        //                caseKind = dtCase2.Rows[i]["New_CaseKind"].ToString();
        //                for (int j = 0; j < dicldapList2.Count; j++)//*初始表格賦初值 
        //                {
        //                    SetExcelCell(sheet2, rows, j + 1, styleHead10, "0");
        //                    sheet2.AddMergedRegion(new CellRangeAddress(rows, rows, j + 1, j + 1));
        //                }
        //            }
        //        }

        //        //*合計 lineLast
        //        SetExcelCell(sheet2, rows + 1, 0, styleBodyBackGray, "合計");
        //        sheet2.AddMergedRegion(new CellRangeAddress(rows + 1, rows + 1, 0, 0));
        //        for (int j = 0; j < dicldapList2.Count; j++)//*初始表格賦初值 
        //        {
        //            SetExcelCell(sheet2, rows + 1, j + 1, styleHead10, "0");
        //            sheet2.AddMergedRegion(new CellRangeAddress(rows + 1, rows + 1, j + 1, j + 1));
        //        }
        //        #endregion
        //        #region body2
        //        caseExcel = "";//案件類型
        //        rowsExcel = 2;//行數
        //        rowscountExcel = 0;//最後一列合計
        //        rowstatolExcel = 0;//總合計      
        //        for (int iRow = 0; iRow < dtCase2.Rows.Count; iRow++)//根據案件類型進行循環
        //        {
        //            foreach (var item in dicldapList2)//循環excel中的列
        //            {
        //                int irows = Convert.ToInt32(item.Value.Split('|')[1]);
        //                if (dtCase2.Rows[iRow]["New_CaseKind"].ToString() == caseExcel)//重複同一案件類型的數據
        //                {
        //                    if (dtCase2.Rows[iRow]["AgentUser"].ToString() == item.Key.ToString())
        //                    {
        //                        SetExcelCell(sheet2, rowsExcel, irows, styleHead10, dtCase2.Rows[iRow]["case_num"].ToString());
        //                        SetExcelCell(sheet2, rows + 1, irows, styleHead10, dtCase2.Rows[iRow]["UserCount"].ToString());//最後一行合計
        //                        rowscountExcelresult = Convert.ToInt32(dtCase2.Rows[iRow]["case_num"].ToString());//每格資料
        //                        rowscountExcel += rowscountExcelresult;
        //                        rowstatolExcel += rowscountExcelresult;
        //                    }
        //                    SetExcelCell(sheet2, rowsExcel, dicldapList2.Count + 1, styleHead10, rowscountExcel.ToString());//最後一列合計
        //                }
        //                else//不重複的案件類型
        //                {
        //                    rowscountExcel = 0;
        //                    rowsExcel = rowsExcel + 1;
        //                    if (dtCase2.Rows[iRow]["AgentUser"].ToString() == item.Key.ToString())
        //                    {
        //                        SetExcelCell(sheet2, rowsExcel, irows, styleHead10, dtCase2.Rows[iRow]["case_num"].ToString());
        //                        SetExcelCell(sheet2, rows + 1, irows, styleHead10, dtCase2.Rows[iRow]["UserCount"].ToString());//最後一行合計
        //                        rowscountExcelresult = Convert.ToInt32(dtCase2.Rows[iRow]["case_num"].ToString());//第一條不重複的數據儲存下值
        //                        rowscountExcel += rowscountExcelresult;
        //                        rowstatolExcel += rowscountExcelresult;
        //                    }
        //                    caseExcel = dtCase2.Rows[iRow]["New_CaseKind"].ToString();
        //                    SetExcelCell(sheet2, rowsExcel, dicldapList2.Count + 1, styleHead10, rowscountExcel.ToString());//最後一列合計      
        //                }
        //            }
        //        }
        //        SetExcelCell(sheet2, rows + 1, dicldapList2.Count + 1, styleHead10, rowstatolExcel.ToString());//總合計
        //        #endregion

        //        #region title3
        //        //*大標題 line0
        //        SetExcelCell(sheet3, 0, 0, styleHead12, "經辦收件統計表");
        //        sheet3.AddMergedRegion(new CellRangeAddress(0, 0, 0, dicldapList3.Count + 1));

        //        //*查詢條件 line1
        //        SetExcelCell(sheet3, 1, 0, styleHead12, "收件日期：");
        //        sheet3.AddMergedRegion(new CellRangeAddress(1, 1, 0, 0));
        //        SetExcelCell(sheet3, 1, 1, styleHead12, model.CreatedDateStart + "~" + model.CreatedDateEnd);
        //        sheet3.AddMergedRegion(new CellRangeAddress(1, 1, 1, 1));
        //        SetExcelCell(sheet3, 1, 2, styleHead12, "結案日期：");
        //        sheet3.AddMergedRegion(new CellRangeAddress(1, 1, 2, 2));
        //        SetExcelCell(sheet3, 1, 3, styleHead12, model.CloseDateStart + "~" + model.CloseDateEnd);
        //        sheet3.AddMergedRegion(new CellRangeAddress(1, 1, 3, 3));

        //        if (dicldapList3.Count > 3)
        //        {
        //            SetExcelCell(sheet3, 1, dicldapList3.Count - 1, styleHead12, "部門別/科別");
        //            sheet3.AddMergedRegion(new CellRangeAddress(1, 1, dicldapList3.Count - 1, dicldapList3.Count));
        //            SetExcelCell(sheet3, 1, dicldapList3.Count + 1, styleHead12, "集作三科");
        //            sheet3.AddMergedRegion(new CellRangeAddress(1, 1, dicldapList3.Count + 1, dicldapList3.Count + 1));
        //        }
        //        else
        //        {
        //            SetExcelCell(sheet3, 1, 4, styleHead12, "部門別/科別");
        //            sheet3.AddMergedRegion(new CellRangeAddress(1, 1, 4, 4));
        //            sheet3.SetColumnWidth(4, 100 * 30);
        //            SetExcelCell(sheet3, 1, 5, styleHead12, "集作三科");
        //            sheet3.AddMergedRegion(new CellRangeAddress(1, 1, 5, 5));
        //        }


        //        //*結果集表頭 line2
        //        SetExcelCell(sheet3, 2, 0, styleBodyBackGray, "處理人員");
        //        sheet3.AddMergedRegion(new CellRangeAddress(2, 2, 0, 0));
        //        sheet3.SetColumnWidth(0, 100 * 50);
        //        agentrows = 1;
        //        foreach (var item in dicldapList3)
        //        {
        //            SetExcelCell(sheet3, 2, agentrows, styleHead10, item.Value.Split('|')[0].ToString());
        //            sheet3.AddMergedRegion(new CellRangeAddress(2, 2, agentrows, agentrows));
        //            agentrows++;
        //        }
        //        SetExcelCell(sheet3, 2, dicldapList3.Count + 1, styleHead10, "合計");
        //        sheet3.AddMergedRegion(new CellRangeAddress(2, 2, dicldapList3.Count + 1, dicldapList3.Count + 1));

        //        //*扣押案件類型 line3-lineN 
        //        caseKind = "";//*去重複
        //        rows = 2;//定義行數
        //        for (int i = 0; i < dtCase3.Rows.Count; i++)
        //        {
        //            if (caseKind != dtCase3.Rows[i]["New_CaseKind"].ToString())
        //            {
        //                rows = rows + 1;
        //                SetExcelCell(sheet3, rows, 0, styleBodyBackGray, dtCase3.Rows[i]["New_CaseKind"].ToString());
        //                sheet3.AddMergedRegion(new CellRangeAddress(rows, rows, 0, 0));
        //                SetExcelCell(sheet3, rows, dicldapList3.Count + 1, styleHead10, "0");
        //                sheet3.AddMergedRegion(new CellRangeAddress(rows, rows, dicldapList3.Count + 1, dicldapList3.Count + 1));
        //                caseKind = dtCase3.Rows[i]["New_CaseKind"].ToString();
        //                for (int j = 0; j < dicldapList3.Count; j++)//*初始表格賦初值 
        //                {
        //                    SetExcelCell(sheet3, rows, j + 1, styleHead10, "0");
        //                    sheet3.AddMergedRegion(new CellRangeAddress(rows, rows, j + 1, j + 1));
        //                }
        //            }
        //        }

        //        //*合計 lineLast
        //        SetExcelCell(sheet3, rows + 1, 0, styleBodyBackGray, "合計");
        //        sheet3.AddMergedRegion(new CellRangeAddress(rows + 1, rows + 1, 0, 0));
        //        for (int j = 0; j < dicldapList3.Count; j++)//*初始表格賦初值 
        //        {
        //            SetExcelCell(sheet3, rows + 1, j + 1, styleHead10, "0");
        //            sheet3.AddMergedRegion(new CellRangeAddress(rows + 1, rows + 1, j + 1, j + 1));
        //        }
        //        #endregion
        //        #region body3
        //        caseExcel = "";//案件類型
        //        rowsExcel = 2;//行數
        //        rowscountExcel = 0;//最後一列合計
        //        rowstatolExcel = 0;//總合計  
        //        for (int iRow = 0; iRow < dtCase3.Rows.Count; iRow++)//根據案件類型進行循環
        //        {
        //            foreach (var item in dicldapList3)//循環excel中的列
        //            {
        //                int irows = Convert.ToInt32(item.Value.Split('|')[1]);
        //                if (dtCase3.Rows[iRow]["New_CaseKind"].ToString() == caseExcel)//重複同一案件類型的數據
        //                {
        //                    if (dtCase3.Rows[iRow]["AgentUser"].ToString() == item.Key)
        //                    {
        //                        SetExcelCell(sheet3, rowsExcel, irows, styleHead10, dtCase3.Rows[iRow]["case_num"].ToString());
        //                        SetExcelCell(sheet3, rows + 1, irows, styleHead10, dtCase3.Rows[iRow]["UserCount"].ToString());//最後一行合計
        //                        rowscountExcelresult = Convert.ToInt32(dtCase3.Rows[iRow]["case_num"].ToString());//每格資料
        //                        rowscountExcel += rowscountExcelresult;
        //                        rowstatolExcel += rowscountExcelresult;
        //                    }
        //                    SetExcelCell(sheet3, rowsExcel, dicldapList3.Count + 1, styleHead10, rowscountExcel.ToString());//最後一列合計
        //                }
        //                else//不重複的案件類型
        //                {
        //                    rowscountExcel = 0;
        //                    rowsExcel = rowsExcel + 1;
        //                    if (dtCase3.Rows[iRow]["AgentUser"].ToString() == item.Key)
        //                    {
        //                        SetExcelCell(sheet3, rowsExcel, irows, styleHead10, dtCase3.Rows[iRow]["case_num"].ToString());
        //                        SetExcelCell(sheet3, rows + 1, irows, styleHead10, dtCase3.Rows[iRow]["UserCount"].ToString());//最後一行合計
        //                        rowscountExcelresult = Convert.ToInt32(dtCase3.Rows[iRow]["case_num"].ToString());//第一條不重複的數據儲存下值
        //                        rowscountExcel += rowscountExcelresult;
        //                        rowstatolExcel += rowscountExcelresult;
        //                    }
        //                    caseExcel = dtCase3.Rows[iRow]["New_CaseKind"].ToString();
        //                    SetExcelCell(sheet3, rowsExcel, dicldapList3.Count + 1, styleHead10, rowscountExcel.ToString());//最後一列合計      
        //                }
        //            }
        //        }
        //        SetExcelCell(sheet3, rows + 1, dicldapList3.Count + 1, styleHead10, rowstatolExcel.ToString());//總合計
        //        #endregion
        //    }

        //    MemoryStream ms = new MemoryStream();
        //    workbook.Write(ms);
        //    ms.Flush();
        //    ms.Position = 0;
        //    workbook = null;
        //    return ms;
        //}

        //20170811 RC RQ-2015-019666-020 派件至跨單位(原產生邏輯程式寫死，組織異動必出問題，改為參數) 宏祥 add start
        //經辦收件統計表導出
        public MemoryStream CaseMasterListReportExcel_NPOI(CaseMaster model, IList<PARMCode> listCode)
        {
           IWorkbook workbook = new HSSFWorkbook();
           int Departmentcount = listCode.Count();

           int rowscountExcelresult = 0;//合計參數
           string caseExcel = "";//案件類型
           int rowsExcel = 2;//行數
           int rowscountExcel = 0;//最後一列合計
           int rowstatolExcel = 0;//總合計      
           int sort = 1;//記錄每個名字在哪一格

           #region def style
           ICellStyle styleHead12 = workbook.CreateCellStyle();
           IFont font12 = workbook.CreateFont();
           font12.FontHeightInPoints = 12;
           font12.FontName = "新細明體";
           styleHead12.FillPattern = FillPattern.SolidForeground;
           styleHead12.FillForegroundColor = HSSFColor.White.Index;
           styleHead12.BorderTop = BorderStyle.None;
           styleHead12.BorderLeft = BorderStyle.None;
           styleHead12.BorderRight = BorderStyle.None;
           styleHead12.BorderBottom = BorderStyle.None;
           styleHead12.WrapText = true;
           styleHead12.Alignment = HorizontalAlignment.Center;
           styleHead12.VerticalAlignment = VerticalAlignment.Center;
           styleHead12.SetFont(font12);

           ICellStyle styleHead10 = workbook.CreateCellStyle();
           IFont font10 = workbook.CreateFont();
           font10.FontHeightInPoints = 10;
           font10.FontName = "新細明體";
           styleHead10.FillPattern = FillPattern.SolidForeground;
           styleHead10.FillForegroundColor = HSSFColor.White.Index;
           styleHead10.BorderTop = BorderStyle.Thin;
           styleHead10.BorderLeft = BorderStyle.Thin;
           styleHead10.BorderRight = BorderStyle.Thin;
           styleHead10.BorderBottom = BorderStyle.Thin;
           styleHead10.WrapText = true;
           styleHead10.Alignment = HorizontalAlignment.Left;
           styleHead10.VerticalAlignment = VerticalAlignment.Center;
           styleHead10.SetFont(font10);

           ICellStyle styleBodyBackGray = workbook.CreateCellStyle();
           IFont fontBodyBackGray = workbook.CreateFont();
           fontBodyBackGray.FontHeightInPoints = 10;
           fontBodyBackGray.FontName = "新細明體";
           styleBodyBackGray.FillPattern = FillPattern.SolidForeground;
           styleBodyBackGray.WrapText = true;
           styleBodyBackGray.Alignment = HorizontalAlignment.Left;
           styleBodyBackGray.VerticalAlignment = VerticalAlignment.Center;
           styleBodyBackGray.FillForegroundColor = HSSFColor.Grey25Percent.Index;
           styleBodyBackGray.BorderTop = BorderStyle.Thin;
           styleBodyBackGray.BorderLeft = BorderStyle.Thin;
           styleBodyBackGray.BorderRight = BorderStyle.Thin;
           styleBodyBackGray.BorderBottom = BorderStyle.Thin;
           styleBodyBackGray.SetFont(fontBodyBackGray);
           #endregion

           //判斷科別搜尋條件
           if (model.Depart != "0")
           {
              for (int k = 0; k < Departmentcount; k++)
              {
                 if (model.Depart == (k + 1).ToString())
                 {
                    ISheet sheet = null;
                    Dictionary<string, string> dicldapList = new Dictionary<string, string>();
                    DataTable dtCase = new DataTable();

                    sheet = workbook.CreateSheet(listCode[k].CodeDesc);
                    dtCase = GetCaseMasterList(model, listCode[k].CodeDesc);

                    foreach (DataRow dr in dtCase.Rows)//去重複的人員名稱
                    {
                       if (!dicldapList.Keys.Contains(dr["AgentUser"].ToString()))
                       {
                          dicldapList.Add(dr["AgentUser"].ToString(), dr["EmpName"].ToString() + "|" + sort);
                          sort++;
                       }
                    }
                    if (dicldapList.Count > 3)
                    {
                       SetExcelCell(sheet, 1, dicldapList.Count + 1, styleHead12, listCode[k].CodeDesc);
                       sheet.AddMergedRegion(new CellRangeAddress(1, 1, dicldapList.Count + 1, dicldapList.Count + 1));
                    }
                    else
                    {
                       SetExcelCell(sheet, 1, 5, styleHead12, listCode[k].CodeDesc);
                       sheet.AddMergedRegion(new CellRangeAddress(1, 1, 5, 5));
                       sheet.SetColumnWidth(5, 100 * 30);
                    }

                    string caseKind = "";//*去重複
                    int rows = 2;//定義行數

                    #region title
                    //*大標題 line0
                    SetExcelCell(sheet, 0, 0, styleHead12, "經辦收件統計表");
                    sheet.AddMergedRegion(new CellRangeAddress(0, 0, 0, dicldapList.Count + 1));

                    //*查詢條件 line1
                    SetExcelCell(sheet, 1, 0, styleHead12, "收件日期：");
                    sheet.AddMergedRegion(new CellRangeAddress(1, 1, 0, 0));
                    SetExcelCell(sheet, 1, 1, styleHead12, model.CreatedDateStart + "~" + model.CreatedDateEnd);
                    sheet.AddMergedRegion(new CellRangeAddress(1, 1, 1, 1));
                    SetExcelCell(sheet, 1, 2, styleHead12, "結案日期：");
                    sheet.AddMergedRegion(new CellRangeAddress(1, 1, 2, 2));
                    SetExcelCell(sheet, 1, 3, styleHead12, model.CloseDateStart + "~" + model.CloseDateEnd);
                    sheet.AddMergedRegion(new CellRangeAddress(1, 1, 3, 3));

                    if (dicldapList.Count > 3)
                    {
                       SetExcelCell(sheet, 1, dicldapList.Count - 1, styleHead12, "部門別/科別");
                       sheet.AddMergedRegion(new CellRangeAddress(1, 1, dicldapList.Count - 1, dicldapList.Count));
                    }
                    else
                    {
                       SetExcelCell(sheet, 1, 4, styleHead12, "部門別/科別");
                       sheet.AddMergedRegion(new CellRangeAddress(1, 1, 4, 4));
                       sheet.SetColumnWidth(4, 100 * 30);
                    }


                    //*結果集表頭 line2
                    SetExcelCell(sheet, 2, 0, styleBodyBackGray, "處理人員");
                    sheet.AddMergedRegion(new CellRangeAddress(2, 2, 0, 0));
                    sheet.SetColumnWidth(0, 100 * 50);

                    int agentrows = 1;
                    foreach (var item in dicldapList)
                    {
                       SetExcelCell(sheet, 2, agentrows, styleHead10, item.Value.Split('|')[0].ToString());
                       sheet.AddMergedRegion(new CellRangeAddress(2, 2, agentrows, agentrows));
                       agentrows++;
                    }
                    SetExcelCell(sheet, 2, dicldapList.Count + 1, styleHead10, "合計");
                    sheet.AddMergedRegion(new CellRangeAddress(2, 2, dicldapList.Count + 1, dicldapList.Count + 1));

                    //*扣押案件類型 line3-lineN 
                    for (int i = 0; i < dtCase.Rows.Count; i++)
                    {   //去重複的案件類型
                       if (caseKind != dtCase.Rows[i]["New_CaseKind"].ToString())
                       {
                          rows = rows + 1;
                          SetExcelCell(sheet, rows, 0, styleBodyBackGray, dtCase.Rows[i]["New_CaseKind"].ToString());
                          sheet.AddMergedRegion(new CellRangeAddress(rows, rows, 0, 0));
                          SetExcelCell(sheet, rows, dicldapList.Count + 1, styleHead10, "0");
                          sheet.AddMergedRegion(new CellRangeAddress(rows, rows, dicldapList.Count + 1, dicldapList.Count + 1));
                          caseKind = dtCase.Rows[i]["New_CaseKind"].ToString();
                          for (int j = 0; j < dicldapList.Count; j++)//*初始表格賦初值 
                          {
                             SetExcelCell(sheet, rows, j + 1, styleHead10, "0");
                             sheet.AddMergedRegion(new CellRangeAddress(rows, rows, j + 1, j + 1));
                          }
                       }
                    }

                    //*合計 lineLast
                    SetExcelCell(sheet, rows + 1, 0, styleBodyBackGray, "合計");
                    sheet.AddMergedRegion(new CellRangeAddress(rows + 1, rows + 1, 0, 0));
                    for (int j = 0; j < dicldapList.Count; j++)//*給最後一行合計初始表格賦初值 
                    {
                       SetExcelCell(sheet, rows + 1, j + 1, styleHead10, "0");
                       sheet.AddMergedRegion(new CellRangeAddress(rows + 1, rows + 1, j + 1, j + 1));
                    }
                    #endregion

                    #region  body
                    for (int iRow = 0; iRow < dtCase.Rows.Count; iRow++)//根據案件類型進行循環
                    {
                       foreach (var item in dicldapList)//循環excel中的列
                       {
                          int irows = Convert.ToInt32(item.Value.Split('|')[1]);
                          if (caseExcel == dtCase.Rows[iRow]["New_CaseKind"].ToString())//重複同一案件類型的數據
                          {
                             if (item.Key == dtCase.Rows[iRow]["AgentUser"].ToString())
                             {
                                SetExcelCell(sheet, rowsExcel, irows, styleHead10, dtCase.Rows[iRow]["case_num"].ToString());
                                rowscountExcelresult = Convert.ToInt32(dtCase.Rows[iRow]["case_num"].ToString());//每格資料
                                SetExcelCell(sheet, rows + 1, irows, styleHead10, dtCase.Rows[iRow]["UserCount"].ToString());//最後一行合計
                                rowscountExcel += rowscountExcelresult;
                                rowstatolExcel += rowscountExcelresult;
                             }
                             SetExcelCell(sheet, rowsExcel, dicldapList.Count + 1, styleHead10, rowscountExcel.ToString());//最後一列合計
                          }
                          else//不重複的案件類型
                          {
                             rowscountExcel = 0;
                             rowsExcel = rowsExcel + 1;
                             if (dtCase.Rows[iRow]["AgentUser"].ToString() == item.Key)
                             {
                                SetExcelCell(sheet, rowsExcel, irows, styleHead10, dtCase.Rows[iRow]["case_num"].ToString());
                                rowscountExcelresult = Convert.ToInt32(dtCase.Rows[iRow]["case_num"].ToString());//第一條不重複的數據儲存下值
                                SetExcelCell(sheet, rows + 1, irows, styleHead10, dtCase.Rows[iRow]["UserCount"].ToString());//最後一行合計
                                rowscountExcel += rowscountExcelresult;
                                rowstatolExcel += rowscountExcelresult;
                             }
                             caseExcel = dtCase.Rows[iRow]["New_CaseKind"].ToString();
                             SetExcelCell(sheet, rowsExcel, dicldapList.Count + 1, styleHead10, rowscountExcel.ToString());//最後一列合計
                          }
                       }
                    }
                    SetExcelCell(sheet, rows + 1, dicldapList.Count + 1, styleHead10, rowstatolExcel.ToString());//總合計
                    #endregion
                 }
              }
           }
           else
           {
              for (int k = 0; k < Departmentcount; k++)
              {
                 ISheet sheet = null;
                 Dictionary<string, string> dicldapList = new Dictionary<string, string>();
                 DataTable dtCase = new DataTable();

                 sheet = workbook.CreateSheet(listCode[k].CodeDesc);
                 dtCase = GetCaseMasterList(model, listCode[k].CodeDesc);

                 sort = 1;

                 foreach (DataRow dr in dtCase.Rows)//去重複的人員名稱
                 {
                    if (!dicldapList.Keys.Contains(dr["AgentUser"].ToString()))
                    {
                       dicldapList.Add(dr["AgentUser"].ToString(), dr["EmpName"].ToString() + "|" + sort);
                       sort++;
                    }
                 }

                 string caseKind = "";//*去重複
                 int rows = 2;//定義行數

                 #region title
                 //*大標題 line0
                 SetExcelCell(sheet, 0, 0, styleHead12, "經辦收件統計表");
                 sheet.AddMergedRegion(new CellRangeAddress(0, 0, 0, dicldapList.Count + 1));

                 //*查詢條件 line1
                 SetExcelCell(sheet, 1, 0, styleHead12, "收件日期：");
                 sheet.AddMergedRegion(new CellRangeAddress(1, 1, 0, 0));
                 SetExcelCell(sheet, 1, 1, styleHead12, model.CreatedDateStart + "~" + model.CreatedDateEnd);
                 sheet.AddMergedRegion(new CellRangeAddress(1, 1, 1, 1));
                 SetExcelCell(sheet, 1, 2, styleHead12, "結案日期：");
                 sheet.AddMergedRegion(new CellRangeAddress(1, 1, 2, 2));
                 SetExcelCell(sheet, 1, 3, styleHead12, model.CloseDateStart + "~" + model.CloseDateEnd);
                 sheet.AddMergedRegion(new CellRangeAddress(1, 1, 3, 3));

                 if (dicldapList.Count > 3)
                 {
                    SetExcelCell(sheet, 1, dicldapList.Count - 1, styleHead12, "部門別/科別");
                    sheet.AddMergedRegion(new CellRangeAddress(1, 1, dicldapList.Count - 1, dicldapList.Count));
                    SetExcelCell(sheet, 1, dicldapList.Count + 1, styleHead12, listCode[k].CodeDesc);
                    sheet.AddMergedRegion(new CellRangeAddress(1, 1, dicldapList.Count + 1, dicldapList.Count + 1));
                 }
                 else
                 {
                    SetExcelCell(sheet, 1, 4, styleHead12, "部門別/科別");
                    sheet.AddMergedRegion(new CellRangeAddress(1, 1, 4, 4));
                    sheet.SetColumnWidth(4, 100 * 30);
                    SetExcelCell(sheet, 1, 5, styleHead12, listCode[k].CodeDesc);
                    sheet.AddMergedRegion(new CellRangeAddress(1, 1, 5, 5));
                 }


                 //*結果集表頭 line2
                 SetExcelCell(sheet, 2, 0, styleBodyBackGray, "處理人員");
                 sheet.AddMergedRegion(new CellRangeAddress(2, 2, 0, 0));
                 sheet.SetColumnWidth(0, 100 * 50);
                 int agentrows = 1;
                 foreach (var item in dicldapList)
                 {
                    SetExcelCell(sheet, 2, agentrows, styleHead10, item.Value.Split('|')[0].ToString());
                    sheet.AddMergedRegion(new CellRangeAddress(2, 2, agentrows, agentrows));
                    agentrows++;
                 }
                 SetExcelCell(sheet, 2, dicldapList.Count + 1, styleHead10, "合計");
                 sheet.AddMergedRegion(new CellRangeAddress(2, 2, dicldapList.Count + 1, dicldapList.Count + 1));

                 //*扣押案件類型 line3-lineN 
                 caseKind = "";//*去重複
                 rows = 2;//定義行數
                 for (int i = 0; i < dtCase.Rows.Count; i++)
                 {
                    if (caseKind != dtCase.Rows[i]["New_CaseKind"].ToString())
                    {
                       rows = rows + 1;
                       SetExcelCell(sheet, rows, 0, styleBodyBackGray, dtCase.Rows[i]["New_CaseKind"].ToString());
                       sheet.AddMergedRegion(new CellRangeAddress(rows, rows, 0, 0));
                       SetExcelCell(sheet, rows, dicldapList.Count + 1, styleHead10, "0");
                       sheet.AddMergedRegion(new CellRangeAddress(rows, rows, dicldapList.Count + 1, dicldapList.Count + 1));
                       caseKind = dtCase.Rows[i]["New_CaseKind"].ToString();
                       for (int j = 0; j < dicldapList.Count; j++)//*初始表格賦初值 
                       {
                          SetExcelCell(sheet, rows, j + 1, styleHead10, "0");
                          sheet.AddMergedRegion(new CellRangeAddress(rows, rows, j + 1, j + 1));
                       }
                    }
                 }

                 //*合計 lineLast
                 SetExcelCell(sheet, rows + 1, 0, styleBodyBackGray, "合計");
                 sheet.AddMergedRegion(new CellRangeAddress(rows + 1, rows + 1, 0, 0));
                 for (int j = 0; j < dicldapList.Count; j++)//*初始表格賦初值 
                 {
                    SetExcelCell(sheet, rows + 1, j + 1, styleHead10, "0");
                    sheet.AddMergedRegion(new CellRangeAddress(rows + 1, rows + 1, j + 1, j + 1));
                 }
                 #endregion

                 #region body
                 caseExcel = "";//案件類型
                 rowsExcel = 2;//行數
                 rowscountExcel = 0;//最後一列合計
                 rowstatolExcel = 0;//總合計  
                 for (int iRow = 0; iRow < dtCase.Rows.Count; iRow++)//根據案件類型進行循環
                 {
                    foreach (var item in dicldapList)//循環excel中的列
                    {
                       int irows = Convert.ToInt32(item.Value.Split('|')[1]);
                       if (dtCase.Rows[iRow]["New_CaseKind"].ToString() == caseExcel)//重複同一案件類型的數據
                       {
                          if (dtCase.Rows[iRow]["AgentUser"].ToString() == item.Key)
                          {
                             SetExcelCell(sheet, rowsExcel, irows, styleHead10, dtCase.Rows[iRow]["case_num"].ToString());
                             SetExcelCell(sheet, rows + 1, irows, styleHead10, dtCase.Rows[iRow]["UserCount"].ToString());//最後一行合計
                             rowscountExcelresult = Convert.ToInt32(dtCase.Rows[iRow]["case_num"].ToString());//每格資料
                             rowscountExcel += rowscountExcelresult;
                             rowstatolExcel += rowscountExcelresult;
                          }
                          SetExcelCell(sheet, rowsExcel, dicldapList.Count + 1, styleHead10, rowscountExcel.ToString());//最後一列合計
                       }
                       else//不重複的案件類型
                       {
                          rowscountExcel = 0;
                          rowsExcel = rowsExcel + 1;
                          if (dtCase.Rows[iRow]["AgentUser"].ToString() == item.Key)
                          {
                             SetExcelCell(sheet, rowsExcel, irows, styleHead10, dtCase.Rows[iRow]["case_num"].ToString());
                             SetExcelCell(sheet, rows + 1, irows, styleHead10, dtCase.Rows[iRow]["UserCount"].ToString());//最後一行合計
                             rowscountExcelresult = Convert.ToInt32(dtCase.Rows[iRow]["case_num"].ToString());//第一條不重複的數據儲存下值
                             rowscountExcel += rowscountExcelresult;
                             rowstatolExcel += rowscountExcelresult;
                          }
                          caseExcel = dtCase.Rows[iRow]["New_CaseKind"].ToString();
                          SetExcelCell(sheet, rowsExcel, dicldapList.Count + 1, styleHead10, rowscountExcel.ToString());//最後一列合計      
                       }
                    }
                 }
                 SetExcelCell(sheet, rows + 1, dicldapList.Count + 1, styleHead10, rowstatolExcel.ToString());//總合計
                 #endregion
              }
           }

           MemoryStream ms = new MemoryStream();
           workbook.Write(ms);
           ms.Flush();
           ms.Position = 0;
           workbook = null;
           return ms;
        }
        //20170811 RC RQ-2015-019666-020 派件至跨單位(原產生邏輯程式寫死，組織異動必出問題，改為參數) 宏祥 add end
        #endregion


        #region 收發退件明細表
        //收發退件明細表數據源
        public DataTable CaseMasterForDetailSearchList(CaseMaster model, string depart)
        {
            string sqlWhere = "";
            string sqlStr = "";
            base.Parameter.Clear();
            base.Parameter.Add(new CommandParameter("@depart", depart));

            if (!string.IsNullOrEmpty(model.CaseKind) && string.IsNullOrEmpty(model.CaseKind2))
            {
                sqlWhere += @" and CaseKind = @CaseKind ";
                base.Parameter.Add(new CommandParameter("@CaseKind", model.CaseKind));
            }
            else if (!string.IsNullOrEmpty(model.CaseKind) && !string.IsNullOrEmpty(model.CaseKind2))
            {
                sqlWhere += @" and (CaseKind+'-'+CaseKind2) = @CaseKind ";
                base.Parameter.Add(new CommandParameter("@CaseKind", model.CaseKind + "-" + model.CaseKind2));
            }
            //if (!string.IsNullOrEmpty(model.CreatedDateStart) && !string.IsNullOrEmpty(model.CreatedDateEnd))
            //{
            //    sqlWhere += @" and CreatedDate between @CreatedDateStart  and @CreatedDateEnd ";
            //    base.Parameter.Add(new CommandParameter("@CreatedDateStart", model.CreatedDateStart));
            //    base.Parameter.Add(new CommandParameter("@CreatedDateEnd", model.CreatedDateEnd));
            //}
            if (!string.IsNullOrEmpty(model.CreatedDateStart))
            {
                //adam sqlWhere += @" AND CreatedDate >= @CreatedDateStart";
                sqlWhere += @" AND M.CreatedDate >= @CreatedDateStart";
                Parameter.Add(new CommandParameter("@CreatedDateStart", model.CreatedDateStart));
            }
            if (!string.IsNullOrEmpty(model.CreatedDateEnd))
            {
                string CreatedDateEnd = Convert.ToDateTime(UtlString.FormatDateString(model.CreatedDateEnd)).AddDays(1).ToString("yyyyMMdd");
                //adam sqlWhere += @" AND CreatedDate < @CreatedDateEnd ";
                sqlWhere += @" AND M.CreatedDate < @CreatedDateEnd ";
                Parameter.Add(new CommandParameter("@CreatedDateEnd", CreatedDateEnd));
            }
            if (!string.IsNullOrEmpty(model.CloseDateStart))
            {
                sqlWhere += @" AND CloseDate >= @CloseDateStart";
                Parameter.Add(new CommandParameter("@CloseDateStart", model.CloseDateStart));
            }
            if (!string.IsNullOrEmpty(model.CloseDateEnd))
            {
                string closeDateEnd = Convert.ToDateTime(UtlString.FormatDateString(model.CloseDateEnd)).AddDays(1).ToString("yyyyMMdd");
                sqlWhere += @" AND CloseDate < @CloseDateEnd ";
                Parameter.Add(new CommandParameter("@CloseDateEnd", closeDateEnd));
            }
            if (!string.IsNullOrEmpty(model.Unit))
            {
                sqlWhere += @" and Unit like @Unit ";
                base.Parameter.Add(new CommandParameter("@Unit", "%" + model.Unit + "%"));
            }



            base.Parameter.Add(new CommandParameter("@Status1", CaseStatus.InputCancelClose));
            base.Parameter.Add(new CommandParameter("@Status2", CaseStatus.CollectionReturn));
            sqlStr = @"SELECT
	                        M.Unit,
	                        N.EmpName,
	                        (M.CaseKind+'-'+M.CaseKind2) AS casekind,
	                        M.ReceiveKind,
	                        M.CaseNo,
	                        M.CloseReason,
	                        M.ReturnAnswer
                        FROM [CaseMaster] AS M
                        LEFT OUTER JOIN [V_AgentAndDept] AS U ON M.AgentUser = U.EmpID
                        LEFT OUTER JOIN LDAPEmployee AS N ON M.Person = N.EmpID
                        WHERE
                        M.[Status] in (@Status1,@Status2) 
                        AND ISNULL(U.SectionName,'集作一科') = @depart
                        " + sqlWhere + @"
                        ORDER BY (M.CaseKind+'-'+M.CaseKind2) desc";
            DataTable dt = base.Search(sqlStr);
            if (dt != null && dt.Rows.Count > 0) return dt;
            else return new DataTable();
        }

        ////收發退件明細表導出
        //public MemoryStream CaseMasterListDetailExcel_NPOI(CaseMaster model)
        //{
        //    IWorkbook workbook = new HSSFWorkbook();
        //    ISheet sheet = null;
        //    ISheet sheet2 = null;
        //    ISheet sheet3 = null;

        //    DataTable dt = new DataTable();
        //    DataTable dt2 = new DataTable();
        //    DataTable dt3 = new DataTable();
        //    #region def style
        //    ICellStyle styleHead12 = workbook.CreateCellStyle();
        //    IFont font12 = workbook.CreateFont();
        //    font12.FontHeightInPoints = 12;
        //    font12.FontName = "新細明體";
        //    styleHead12.FillPattern = FillPattern.SolidForeground;
        //    styleHead12.FillForegroundColor = HSSFColor.White.Index;
        //    styleHead12.BorderTop = BorderStyle.None;
        //    styleHead12.BorderLeft = BorderStyle.None;
        //    styleHead12.BorderRight = BorderStyle.None;
        //    styleHead12.BorderBottom = BorderStyle.None;
        //    styleHead12.WrapText = true;
        //    styleHead12.Alignment = HorizontalAlignment.Center;
        //    styleHead12.VerticalAlignment = VerticalAlignment.Center;
        //    styleHead12.SetFont(font12);

        //    ICellStyle styleHead10 = workbook.CreateCellStyle();
        //    IFont font10 = workbook.CreateFont();
        //    font10.FontHeightInPoints = 10;
        //    font10.FontName = "新細明體";
        //    styleHead10.FillPattern = FillPattern.SolidForeground;
        //    styleHead10.FillForegroundColor = HSSFColor.White.Index;
        //    styleHead10.BorderTop = BorderStyle.Thin;
        //    styleHead10.BorderLeft = BorderStyle.Thin;
        //    styleHead10.BorderRight = BorderStyle.Thin;
        //    styleHead10.BorderBottom = BorderStyle.Thin;
        //    styleHead10.WrapText = true;
        //    styleHead10.Alignment = HorizontalAlignment.Left;
        //    styleHead10.VerticalAlignment = VerticalAlignment.Center;
        //    styleHead10.SetFont(font10);

        //    ICellStyle styleBodyBackGray = workbook.CreateCellStyle();
        //    IFont fontBodyBackGray = workbook.CreateFont();
        //    fontBodyBackGray.FontHeightInPoints = 10;
        //    fontBodyBackGray.FontName = "新細明體";
        //    styleBodyBackGray.FillPattern = FillPattern.SolidForeground;
        //    styleBodyBackGray.WrapText = true;
        //    styleBodyBackGray.Alignment = HorizontalAlignment.Center;
        //    styleBodyBackGray.VerticalAlignment = VerticalAlignment.Center;
        //    styleBodyBackGray.FillForegroundColor = HSSFColor.Grey25Percent.Index;
        //    styleBodyBackGray.BorderTop = BorderStyle.Thin;
        //    styleBodyBackGray.BorderLeft = BorderStyle.Thin;
        //    styleBodyBackGray.BorderRight = BorderStyle.Thin;
        //    styleBodyBackGray.BorderBottom = BorderStyle.Thin;
        //    styleBodyBackGray.SetFont(fontBodyBackGray);
        //    #endregion

        //    #region 獲取數據源(集作一科及案件資料)
        //    //獲取人員
        //    if (model.Depart == "1")//* 集作一科
        //    {
        //        sheet = workbook.CreateSheet("集作一科");
        //        dt = CaseMasterForDetailSearchList(model, "集作一科");//獲取查詢集作一科的案件
        //        SetExcelCell(sheet, 1, 6, styleHead12, "集作一科");
        //        sheet.AddMergedRegion(new CellRangeAddress(1, 1, 6, 6));
        //    }
        //    if (model.Depart == "2")//* 集作二科
        //    {
        //        sheet = workbook.CreateSheet("集作二科");
        //        dt = CaseMasterForDetailSearchList(model, "集作二科");//獲取查詢集作二科的案件
        //        SetExcelCell(sheet, 1, 6, styleHead12, "集作二科");
        //        sheet.AddMergedRegion(new CellRangeAddress(1, 1, 6, 6));
        //    }
        //    if (model.Depart == "3")//*集作三科
        //    {
        //        sheet = workbook.CreateSheet("集作三科");
        //        dt = CaseMasterForDetailSearchList(model, "集作三科");//獲取查詢集作三科的案件
        //        SetExcelCell(sheet, 1, 6, styleHead12, "集作三科");
        //        sheet.AddMergedRegion(new CellRangeAddress(1, 1, 6, 6));
        //    }
        //    if (model.Depart == "0")//*全部
        //    {
        //        sheet = workbook.CreateSheet("集作一科");
        //        sheet2 = workbook.CreateSheet("集作二科");
        //        sheet3 = workbook.CreateSheet("集作三科");
        //        dt = CaseMasterForDetailSearchList(model, "集作一科");//獲取查詢集作一科的案件
        //        dt2 = CaseMasterForDetailSearchList(model, "集作二科");//獲取查詢集作二科的案件
        //        dt3 = CaseMasterForDetailSearchList(model, "集作三科");//獲取查詢集作三科的案件
        //        SetExcelCell(sheet, 1, 6, styleHead12, "集作一科");
        //        sheet.AddMergedRegion(new CellRangeAddress(1, 1, 6, 6));
        //        SetExcelCell(sheet2, 1, 6, styleHead12, "集作二科");
        //        sheet2.AddMergedRegion(new CellRangeAddress(1, 1, 6, 6));
        //        SetExcelCell(sheet3, 1, 6, styleHead12, "集作三科");
        //        sheet3.AddMergedRegion(new CellRangeAddress(1, 1, 6, 6));
        //    }
        //    #endregion

        //    #region title
        //    //*大標題 line0
        //    SetExcelCell(sheet, 0, 0, styleHead12, "收發退件明細表");
        //    sheet.AddMergedRegion(new CellRangeAddress(0, 0, 0, 6));

        //    //*查詢條件 line1
        //    SetExcelCell(sheet, 1, 0, styleHead12, "收件日期：");
        //    sheet.AddMergedRegion(new CellRangeAddress(1, 1, 0, 0));
        //    SetExcelCell(sheet, 1, 1, styleHead12, model.CreatedDateStart + "~" + model.CreatedDateEnd);
        //    sheet.AddMergedRegion(new CellRangeAddress(1, 1, 1, 1));
        //    SetExcelCell(sheet, 1, 2, styleHead12, "結案日期：");
        //    sheet.AddMergedRegion(new CellRangeAddress(1, 1, 2, 2));
        //    SetExcelCell(sheet, 1, 3, styleHead12, model.CloseDateStart + "~" + model.CloseDateEnd);
        //    sheet.AddMergedRegion(new CellRangeAddress(1, 1, 3, 3));
        //    SetExcelCell(sheet, 1, 5, styleHead12, "部門別/科別");
        //    sheet.AddMergedRegion(new CellRangeAddress(1, 1, 5, 5));


        //    //*結果集表頭 line2
        //    SetExcelCell(sheet, 2, 0, styleBodyBackGray, "分行別");
        //    sheet.AddMergedRegion(new CellRangeAddress(2, 2, 0, 0));
        //    sheet.SetColumnWidth(0, 100 * 30);
        //    SetExcelCell(sheet, 2, 1, styleBodyBackGray, "建檔人員");
        //    sheet.AddMergedRegion(new CellRangeAddress(2, 2, 1, 1));
        //    SetExcelCell(sheet, 2, 2, styleBodyBackGray, "案件類型");
        //    sheet.AddMergedRegion(new CellRangeAddress(2, 2, 2, 2));
        //    sheet.SetColumnWidth(2, 100 * 50);
        //    SetExcelCell(sheet, 2, 3, styleBodyBackGray, "來文方式");
        //    sheet.AddMergedRegion(new CellRangeAddress(2, 2, 3, 3));
        //    SetExcelCell(sheet, 2, 4, styleBodyBackGray, "案件編號");
        //    sheet.AddMergedRegion(new CellRangeAddress(2, 2, 4, 4));
        //    sheet.SetColumnWidth(4, 100 * 50);
        //    SetExcelCell(sheet, 2, 5, styleBodyBackGray, "退件原因");
        //    sheet.AddMergedRegion(new CellRangeAddress(2, 2, 5, 5));
        //    sheet.SetColumnWidth(5, 100 * 200);
        //    SetExcelCell(sheet, 2, 6, styleBodyBackGray, "分行回覆原因");
        //    sheet.AddMergedRegion(new CellRangeAddress(2, 2, 6, 6));
        //    sheet.SetColumnWidth(6, 100 * 100);
        //    #endregion

        //    #region body
        //    for (int iRow = 0; iRow < dt.Rows.Count; iRow++)
        //    {
        //        for (int iCol = 0; iCol < dt.Columns.Count; iCol++)
        //        {
        //            SetExcelCell(sheet, iRow + 3, iCol, styleHead10, Convert.ToString(dt.Rows[iRow][iCol]));
        //        }
        //    }
        //    #endregion

        //    if (model.Depart == "0")//* 全部
        //    {
        //        #region title2
        //        //*大標題 line0
        //        // adam
        //        //SetExcelCell(sheet2, 0, 0, styleHead12, "收發明細統計表");
        //        SetExcelCell(sheet2, 0, 0, styleHead12, "收發退件明細表");
        //        //
        //        sheet2.AddMergedRegion(new CellRangeAddress(0, 0, 0, 6));

        //        //*查詢條件 line1
        //        SetExcelCell(sheet2, 1, 0, styleHead12, "收件日期：");
        //        sheet2.AddMergedRegion(new CellRangeAddress(1, 1, 0, 0));
        //        SetExcelCell(sheet2, 1, 1, styleHead12, model.CreatedDateStart + "~" + model.CreatedDateEnd);
        //        sheet2.AddMergedRegion(new CellRangeAddress(1, 1, 1, 1));
        //        SetExcelCell(sheet2, 1, 2, styleHead12, "結案日期：");
        //        sheet2.AddMergedRegion(new CellRangeAddress(1, 1, 2, 2));
        //        SetExcelCell(sheet2, 1, 3, styleHead12, model.CloseDateStart + "~" + model.CloseDateEnd);
        //        sheet2.AddMergedRegion(new CellRangeAddress(1, 1, 3, 3));
        //        SetExcelCell(sheet2, 1, 5, styleHead12, "部門別/科別");
        //        sheet2.AddMergedRegion(new CellRangeAddress(1, 1, 5, 5));


        //        //*結果集表頭 line2
        //        SetExcelCell(sheet2, 2, 0, styleBodyBackGray, "分行別");
        //        sheet2.AddMergedRegion(new CellRangeAddress(2, 2, 0, 0));
        //        sheet2.SetColumnWidth(0, 100 * 30);
        //        SetExcelCell(sheet2, 2, 1, styleBodyBackGray, "建檔人員");
        //        sheet2.AddMergedRegion(new CellRangeAddress(2, 2, 1, 1));
        //        SetExcelCell(sheet2, 2, 2, styleBodyBackGray, "案件類型");
        //        sheet2.AddMergedRegion(new CellRangeAddress(2, 2, 2, 2));
        //        sheet2.SetColumnWidth(2, 100 * 50);
        //        SetExcelCell(sheet2, 2, 3, styleBodyBackGray, "來文方式");
        //        sheet2.AddMergedRegion(new CellRangeAddress(2, 2, 3, 3));
        //        SetExcelCell(sheet2, 2, 4, styleBodyBackGray, "案件編號");
        //        sheet2.AddMergedRegion(new CellRangeAddress(2, 2, 4, 4));
        //        sheet2.SetColumnWidth(4, 100 * 50);
        //        SetExcelCell(sheet2, 2, 5, styleBodyBackGray, "退件原因");
        //        sheet2.AddMergedRegion(new CellRangeAddress(2, 2, 5, 5));
        //        sheet2.SetColumnWidth(5, 100 * 200);
        //        SetExcelCell(sheet2, 2, 6, styleBodyBackGray, "分行回復原因");
        //        sheet2.AddMergedRegion(new CellRangeAddress(2, 2, 6, 6));
        //        sheet2.SetColumnWidth(6, 100 * 100);
        //        #endregion
        //        #region body2
        //        for (int iRow = 0; iRow < dt2.Rows.Count; iRow++)
        //        {
        //            for (int iCol = 0; iCol < dt2.Columns.Count; iCol++)
        //            {
        //                SetExcelCell(sheet2, iRow + 3, iCol, styleHead10, Convert.ToString(dt2.Rows[iRow][iCol]));
        //            }
        //        }
        //        #endregion

        //        #region title3
        //        //*大標題 line0
        //        // adam
        //        //SetExcelCell(sheet3, 0, 0, styleHead12, "收發明細統計表");
        //        SetExcelCell(sheet3, 0, 0, styleHead12, "收發退件明細表");
        //        sheet3.AddMergedRegion(new CellRangeAddress(0, 0, 0, 6));

        //        //*查詢條件 line1
        //        SetExcelCell(sheet3, 1, 0, styleHead12, "收件日期：");
        //        sheet3.AddMergedRegion(new CellRangeAddress(1, 1, 0, 0));
        //        SetExcelCell(sheet3, 1, 1, styleHead12, model.CreatedDateStart + "~" + model.CreatedDateEnd);
        //        sheet3.AddMergedRegion(new CellRangeAddress(1, 1, 1, 1));
        //        SetExcelCell(sheet3, 1, 2, styleHead12, "結案日期：");
        //        sheet3.AddMergedRegion(new CellRangeAddress(1, 1, 2, 2));
        //        SetExcelCell(sheet3, 1, 3, styleHead12, model.CloseDateStart + "~" + model.CloseDateEnd);
        //        sheet3.AddMergedRegion(new CellRangeAddress(1, 1, 3, 3));
        //        SetExcelCell(sheet3, 1, 5, styleHead12, "部門別/科別");
        //        sheet3.AddMergedRegion(new CellRangeAddress(1, 1, 5, 5));


        //        //*結果集表頭 line2
        //        SetExcelCell(sheet3, 2, 0, styleBodyBackGray, "分行別");
        //        sheet3.AddMergedRegion(new CellRangeAddress(2, 2, 0, 0));
        //        sheet3.SetColumnWidth(0, 100 * 30);
        //        SetExcelCell(sheet3, 2, 1, styleBodyBackGray, "建檔人員");
        //        sheet3.AddMergedRegion(new CellRangeAddress(2, 2, 1, 1));
        //        SetExcelCell(sheet3, 2, 2, styleBodyBackGray, "案件類型");
        //        sheet3.AddMergedRegion(new CellRangeAddress(2, 2, 2, 2));
        //        sheet3.SetColumnWidth(2, 100 * 50);
        //        SetExcelCell(sheet3, 2, 3, styleBodyBackGray, "來文方式");
        //        sheet3.AddMergedRegion(new CellRangeAddress(2, 2, 3, 3));
        //        SetExcelCell(sheet3, 2, 4, styleBodyBackGray, "案件編號");
        //        sheet3.AddMergedRegion(new CellRangeAddress(2, 2, 4, 4));
        //        sheet3.SetColumnWidth(4, 100 * 50);
        //        SetExcelCell(sheet3, 2, 5, styleBodyBackGray, "退件原因");
        //        sheet3.AddMergedRegion(new CellRangeAddress(2, 2, 5, 5));
        //        sheet3.SetColumnWidth(5, 100 * 200);
        //        SetExcelCell(sheet3, 2, 6, styleBodyBackGray, "分行回復原因");
        //        sheet3.AddMergedRegion(new CellRangeAddress(2, 2, 6, 6));
        //        sheet3.SetColumnWidth(6, 100 * 100);
        //        #endregion
        //        #region body3
        //        for (int iRow = 0; iRow < dt3.Rows.Count; iRow++)
        //        {
        //            for (int iCol = 0; iCol < dt3.Columns.Count; iCol++)
        //            {
        //                SetExcelCell(sheet3, iRow + 3, iCol, styleHead10, Convert.ToString(dt3.Rows[iRow][iCol]));
        //            }
        //        }
        //        #endregion
        //    }

        //    MemoryStream ms = new MemoryStream();
        //    workbook.Write(ms);
        //    ms.Flush();
        //    ms.Position = 0;
        //    workbook = null;
        //    return ms;
        //}

        //20170811 RC RQ-2015-019666-020 派件至跨單位(原產生邏輯程式寫死，組織異動必出問題，改為參數) 宏祥 add start
        //收發退件明細表導出
        public MemoryStream CaseMasterListDetailExcel_NPOI(CaseMaster model, IList<PARMCode> listCode)
        {
           IWorkbook workbook = new HSSFWorkbook();
           int Departmentcount = listCode.Count();

           #region def style
           ICellStyle styleHead12 = workbook.CreateCellStyle();
           IFont font12 = workbook.CreateFont();
           font12.FontHeightInPoints = 12;
           font12.FontName = "新細明體";
           styleHead12.FillPattern = FillPattern.SolidForeground;
           styleHead12.FillForegroundColor = HSSFColor.White.Index;
           styleHead12.BorderTop = BorderStyle.None;
           styleHead12.BorderLeft = BorderStyle.None;
           styleHead12.BorderRight = BorderStyle.None;
           styleHead12.BorderBottom = BorderStyle.None;
           styleHead12.WrapText = true;
           styleHead12.Alignment = HorizontalAlignment.Center;
           styleHead12.VerticalAlignment = VerticalAlignment.Center;
           styleHead12.SetFont(font12);

           ICellStyle styleHead10 = workbook.CreateCellStyle();
           IFont font10 = workbook.CreateFont();
           font10.FontHeightInPoints = 10;
           font10.FontName = "新細明體";
           styleHead10.FillPattern = FillPattern.SolidForeground;
           styleHead10.FillForegroundColor = HSSFColor.White.Index;
           styleHead10.BorderTop = BorderStyle.Thin;
           styleHead10.BorderLeft = BorderStyle.Thin;
           styleHead10.BorderRight = BorderStyle.Thin;
           styleHead10.BorderBottom = BorderStyle.Thin;
           styleHead10.WrapText = true;
           styleHead10.Alignment = HorizontalAlignment.Left;
           styleHead10.VerticalAlignment = VerticalAlignment.Center;
           styleHead10.SetFont(font10);

           ICellStyle styleBodyBackGray = workbook.CreateCellStyle();
           IFont fontBodyBackGray = workbook.CreateFont();
           fontBodyBackGray.FontHeightInPoints = 10;
           fontBodyBackGray.FontName = "新細明體";
           styleBodyBackGray.FillPattern = FillPattern.SolidForeground;
           styleBodyBackGray.WrapText = true;
           styleBodyBackGray.Alignment = HorizontalAlignment.Center;
           styleBodyBackGray.VerticalAlignment = VerticalAlignment.Center;
           styleBodyBackGray.FillForegroundColor = HSSFColor.Grey25Percent.Index;
           styleBodyBackGray.BorderTop = BorderStyle.Thin;
           styleBodyBackGray.BorderLeft = BorderStyle.Thin;
           styleBodyBackGray.BorderRight = BorderStyle.Thin;
           styleBodyBackGray.BorderBottom = BorderStyle.Thin;
           styleBodyBackGray.SetFont(fontBodyBackGray);
           #endregion

           //判斷科別搜尋條件
           if (model.Depart != "0")
           {
              for (int k = 0; k < Departmentcount; k++)
              {
                 if (model.Depart == (k + 1).ToString())
                 {
                    ISheet sheet = null;
                    DataTable dt = new DataTable();

                    sheet = workbook.CreateSheet(listCode[k].CodeDesc);
                    dt = CaseMasterForDetailSearchList(model, listCode[k].CodeDesc);//獲取查詢科別的案件
                    SetExcelCell(sheet, 1, 6, styleHead12, listCode[k].CodeDesc);
                    sheet.AddMergedRegion(new CellRangeAddress(1, 1, 6, 6));

                    #region title
                    //*大標題 line0
                    SetExcelCell(sheet, 0, 0, styleHead12, "收發退件明細表");
                    sheet.AddMergedRegion(new CellRangeAddress(0, 0, 0, 6));

                    //*查詢條件 line1
                    SetExcelCell(sheet, 1, 0, styleHead12, "收件日期：");
                    sheet.AddMergedRegion(new CellRangeAddress(1, 1, 0, 0));
                    SetExcelCell(sheet, 1, 1, styleHead12, model.CreatedDateStart + "~" + model.CreatedDateEnd);
                    sheet.AddMergedRegion(new CellRangeAddress(1, 1, 1, 1));
                    SetExcelCell(sheet, 1, 2, styleHead12, "結案日期：");
                    sheet.AddMergedRegion(new CellRangeAddress(1, 1, 2, 2));
                    SetExcelCell(sheet, 1, 3, styleHead12, model.CloseDateStart + "~" + model.CloseDateEnd);
                    sheet.AddMergedRegion(new CellRangeAddress(1, 1, 3, 3));
                    SetExcelCell(sheet, 1, 5, styleHead12, "部門別/科別");
                    sheet.AddMergedRegion(new CellRangeAddress(1, 1, 5, 5));


                    //*結果集表頭 line2
                    SetExcelCell(sheet, 2, 0, styleBodyBackGray, "分行別");
                    sheet.AddMergedRegion(new CellRangeAddress(2, 2, 0, 0));
                    sheet.SetColumnWidth(0, 100 * 30);
                    SetExcelCell(sheet, 2, 1, styleBodyBackGray, "建檔人員");
                    sheet.AddMergedRegion(new CellRangeAddress(2, 2, 1, 1));
                    SetExcelCell(sheet, 2, 2, styleBodyBackGray, "案件類型");
                    sheet.AddMergedRegion(new CellRangeAddress(2, 2, 2, 2));
                    sheet.SetColumnWidth(2, 100 * 50);
                    SetExcelCell(sheet, 2, 3, styleBodyBackGray, "來文方式");
                    sheet.AddMergedRegion(new CellRangeAddress(2, 2, 3, 3));
                    SetExcelCell(sheet, 2, 4, styleBodyBackGray, "案件編號");
                    sheet.AddMergedRegion(new CellRangeAddress(2, 2, 4, 4));
                    sheet.SetColumnWidth(4, 100 * 50);
                    SetExcelCell(sheet, 2, 5, styleBodyBackGray, "退件原因");
                    sheet.AddMergedRegion(new CellRangeAddress(2, 2, 5, 5));
                    sheet.SetColumnWidth(5, 100 * 200);
                    SetExcelCell(sheet, 2, 6, styleBodyBackGray, "分行回覆原因");
                    sheet.AddMergedRegion(new CellRangeAddress(2, 2, 6, 6));
                    sheet.SetColumnWidth(6, 100 * 100);
                    #endregion

                    #region body
                    for (int iRow = 0; iRow < dt.Rows.Count; iRow++)
                    {
                       for (int iCol = 0; iCol < dt.Columns.Count; iCol++)
                       {
                          SetExcelCell(sheet, iRow + 3, iCol, styleHead10, Convert.ToString(dt.Rows[iRow][iCol]));
                       }
                    }
                    #endregion
                 }
              }
           }
           else
           {
              for (int k = 0; k < Departmentcount; k++)
              {
                 ISheet sheet = null;
                 DataTable dt = new DataTable();

                 sheet = workbook.CreateSheet(listCode[k].CodeDesc);
                 dt = CaseMasterForDetailSearchList(model, listCode[k].CodeDesc);//獲取查詢科別的案件
                 SetExcelCell(sheet, 1, 6, styleHead12, listCode[k].CodeDesc);
                 sheet.AddMergedRegion(new CellRangeAddress(1, 1, 6, 6));

                 #region title
                 //*大標題 line0
                 SetExcelCell(sheet, 0, 0, styleHead12, "收發退件明細表");
                 sheet.AddMergedRegion(new CellRangeAddress(0, 0, 0, 6));

                 //*查詢條件 line1
                 SetExcelCell(sheet, 1, 0, styleHead12, "收件日期：");
                 sheet.AddMergedRegion(new CellRangeAddress(1, 1, 0, 0));
                 SetExcelCell(sheet, 1, 1, styleHead12, model.CreatedDateStart + "~" + model.CreatedDateEnd);
                 sheet.AddMergedRegion(new CellRangeAddress(1, 1, 1, 1));
                 SetExcelCell(sheet, 1, 2, styleHead12, "結案日期：");
                 sheet.AddMergedRegion(new CellRangeAddress(1, 1, 2, 2));
                 SetExcelCell(sheet, 1, 3, styleHead12, model.CloseDateStart + "~" + model.CloseDateEnd);
                 sheet.AddMergedRegion(new CellRangeAddress(1, 1, 3, 3));
                 SetExcelCell(sheet, 1, 5, styleHead12, "部門別/科別");
                 sheet.AddMergedRegion(new CellRangeAddress(1, 1, 5, 5));


                 //*結果集表頭 line2
                 SetExcelCell(sheet, 2, 0, styleBodyBackGray, "分行別");
                 sheet.AddMergedRegion(new CellRangeAddress(2, 2, 0, 0));
                 sheet.SetColumnWidth(0, 100 * 30);
                 SetExcelCell(sheet, 2, 1, styleBodyBackGray, "建檔人員");
                 sheet.AddMergedRegion(new CellRangeAddress(2, 2, 1, 1));
                 SetExcelCell(sheet, 2, 2, styleBodyBackGray, "案件類型");
                 sheet.AddMergedRegion(new CellRangeAddress(2, 2, 2, 2));
                 sheet.SetColumnWidth(2, 100 * 50);
                 SetExcelCell(sheet, 2, 3, styleBodyBackGray, "來文方式");
                 sheet.AddMergedRegion(new CellRangeAddress(2, 2, 3, 3));
                 SetExcelCell(sheet, 2, 4, styleBodyBackGray, "案件編號");
                 sheet.AddMergedRegion(new CellRangeAddress(2, 2, 4, 4));
                 sheet.SetColumnWidth(4, 100 * 50);
                 SetExcelCell(sheet, 2, 5, styleBodyBackGray, "退件原因");
                 sheet.AddMergedRegion(new CellRangeAddress(2, 2, 5, 5));
                 sheet.SetColumnWidth(5, 100 * 200);
                 SetExcelCell(sheet, 2, 6, styleBodyBackGray, "分行回復原因");
                 sheet.AddMergedRegion(new CellRangeAddress(2, 2, 6, 6));
                 sheet.SetColumnWidth(6, 100 * 100);
                 #endregion

                 #region body
                 for (int iRow = 0; iRow < dt.Rows.Count; iRow++)
                 {
                    for (int iCol = 0; iCol < dt.Columns.Count; iCol++)
                    {
                       SetExcelCell(sheet, iRow + 3, iCol, styleHead10, Convert.ToString(dt.Rows[iRow][iCol]));
                    }
                 }
                 #endregion
              }
           }

           MemoryStream ms = new MemoryStream();
           workbook.Write(ms);
           ms.Flush();
           ms.Position = 0;
           workbook = null;
           return ms;
        }
        //20170811 RC RQ-2015-019666-020 派件至跨單位(原產生邏輯程式寫死，組織異動必出問題，改為參數) 宏祥 add end

        #endregion
        #endregion

        #region 理債人員查詢
        #region 理債人員查詢結果集(含分頁)
        //理債人員查詢結果集
        public List<CaseMaster> CaseMasterSearchList(CaseMaster model, int pageIndex, string strSortExpression, string strSortDirection, ref string strWhere)
        {
            try
            {
                base.PageIndex = pageIndex;
                string sqlStr = "";
                string sqlWhere = "";
                strWhere = string.Empty;
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@pageS", (base.PageSize * (base.PageIndex - 1)) + 1));
                base.Parameter.Add(new CommandParameter("@pageE", base.PageSize * base.PageIndex));
                base.Parameter.Add(new CommandParameter("@Status1", CaseStatus.CollectionReturn));
                base.Parameter.Add(new CommandParameter("@Status2", CaseStatus.CollectionSubmit));//*已排除
                base.Parameter.Add(new CommandParameter("@Status3", CaseStatus.AgentReturnClose));
                base.Parameter.Add(new CommandParameter("@Status4", CaseStatus.InputCancelClose));

                if (!string.IsNullOrEmpty(model.ObligorNo))
                {
                    sqlWhere += @" and ObligorNo like @ObligorNo ";
                    strWhere += @" and ObligorNo like '%" + model.ObligorNo + "%'";
                    base.Parameter.Add(new CommandParameter("@ObligorNo", "%" + model.ObligorNo + "%"));
                }
                if (!string.IsNullOrEmpty(model.CreatedDateStart))
                {
                    sqlWhere += @" and c.CreatedDate >= @CreatedDateStart";
                    strWhere += @" and c.CreatedDate >= '" + model.CreatedDateStart + "'";
                    base.Parameter.Add(new CommandParameter("@CreatedDateStart", model.CreatedDateStart));
                }
                if (!string.IsNullOrEmpty(model.CreatedDateEnd))
                {
                    string QDateE = Convert.ToDateTime(model.CreatedDateEnd).AddDays(1).ToString("yyyyMMdd");
                    sqlWhere += @" and c.CreatedDate < @CreateDateEnd ";
                    strWhere += @" and c.CreatedDate < '" + QDateE + "'";
                    base.Parameter.Add(new CommandParameter("@CreateDateEnd", QDateE));
                }
                if (!string.IsNullOrEmpty(model.CaseKind))
                {
                    sqlWhere += @" and CaseKind = @CaseKind ";
                    strWhere += @" and CaseKind = '" + model.CaseKind.Trim() + "'";
                    base.Parameter.Add(new CommandParameter("@CaseKind", model.CaseKind.Trim()));
                }
                if (!string.IsNullOrEmpty(model.CaseKind2))
                {
                    sqlWhere += @" and CaseKind2 = @CaseKind2 ";
                    strWhere += @" and CaseKind2 = '" + model.CaseKind2.Trim() + "'";
                    base.Parameter.Add(new CommandParameter("@CaseKind2", model.CaseKind2.Trim()));
                }
                if (!string.IsNullOrEmpty(model.GovNo))
                {
                    sqlWhere += @" and GovNo like @GovNo ";
                    strWhere += @" and GovNo like '%" + model.GovNo.Trim() + "%'";
                    base.Parameter.Add(new CommandParameter("@GovNo", "%" + model.GovNo.Trim() + "%"));
                }
                sqlStr += @";with T1 
	                        as
	                        (
		                        select ROW_NUMBER() OVER(ORDER BY c.CaseNo asc) as Sno, 
                                c.CaseNo,c.GovNo, o.ObligorNo,c.CaseKind,c.CaseKind2,
                                CONVERT(nvarchar(10), c.CreatedDate,111) as CreatedDate,c.CaseId
                                from CaseMaster c left join CaseObligor o on c.CaseId=o.CaseId where 1=1 
                                and (CaseKind2 = '扣押' or CaseKind2 ='扣押並支付' or CaseKind2 = '支付' or CaseKind2 ='撤銷')
                                and Status<>@Status1 and Status<>@Status3 and Status<>@Status4 " + sqlWhere + @"   
	                        ),T2 as
	                        (
		                        select *, row_number() over (order by CaseNo  asc) RowNum
		                        from T1
	                        ),T3 as 
	                        (
		                        select *,(select max(RowNum) from T2) maxnum from T2 
		                        where rownum between @pageS and @pageE
	                        )
	                        select a.* from T3 a order by a.RowNum";

                IList<CaseMaster> _ilsit = base.SearchList<CaseMaster>(sqlStr);
                List<CaseMaster> listitem = new List<CaseMaster>();
                if (_ilsit != null)
                {
                    if (_ilsit.Count > 0)
                    {
                        base.DataRecords = _ilsit[0].maxnum;
                    }
                    else
                    {
                        base.DataRecords = 0;
                        _ilsit = new List<CaseMaster>();
                    }
                    foreach (var item in _ilsit)
                    {
                        listitem.Add(item);
                    }
                    return listitem;
                }
                else
                {
                    return new List<CaseMaster>();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion
        #region 理債人員導出數據源
        //理債人員導出數據源
        public DataTable CaseMasterSearchList(string strWhere)
        {
            base.Parameter.Add(new CommandParameter("@Status1", CaseStatus.CollectionReturn));
            base.Parameter.Add(new CommandParameter("@Status2", CaseStatus.CollectionSubmit));
            base.Parameter.Add(new CommandParameter("@Status3", CaseStatus.AgentReturnClose));
            base.Parameter.Add(new CommandParameter("@Status4", CaseStatus.InputCancelClose));

            string strSql = @" select ROW_NUMBER() OVER(ORDER BY c.CaseNo asc) as Sno, 
                                c.CaseNo,o.ObligorNo,c.CaseKind,c.CaseKind2,c.GovNo, 
                                CONVERT(nvarchar(10), c.CreatedDate,111) as CreatedDate,c.CaseId
                                from CaseMaster c left join CaseObligor o on c.CaseId=o.CaseId where 1=1 
                                and (CaseKind2 = '扣押' or CaseKind2 ='扣押並支付' or CaseKind2 = '支付' or CaseKind2 ='撤銷')
                                and Status<>@Status1 and Status<>@Status3 and Status<>@Status4 " + strWhere;

            DataTable dt = base.Search(strSql);
            if (dt != null && dt.Rows.Count > 0) return dt;
            else return new DataTable();
        }
        #endregion
        #region 理債人員導出方法
        //理債人員導出方法
        public MemoryStream CaseObligorListReportExcel_NPOI(string strWhere)
        {
            IWorkbook workbook = new HSSFWorkbook();
            ISheet sheet = workbook.CreateSheet("理債人員查詢");

            #region 獲取數據源
            DataTable dt = CaseMasterSearchList(strWhere);
            #endregion

            #region def style
            ICellStyle styleHead12 = workbook.CreateCellStyle();
            IFont font12 = workbook.CreateFont();
            font12.FontHeightInPoints = 12;
            font12.FontName = "新細明體";
            styleHead12.FillPattern = FillPattern.SolidForeground;
            styleHead12.FillForegroundColor = HSSFColor.White.Index;
            styleHead12.BorderTop = BorderStyle.None;
            styleHead12.BorderLeft = BorderStyle.None;
            styleHead12.BorderRight = BorderStyle.None;
            styleHead12.BorderBottom = BorderStyle.None;
            styleHead12.WrapText = true;
            styleHead12.Alignment = HorizontalAlignment.Center;
            styleHead12.VerticalAlignment = VerticalAlignment.Center;
            styleHead12.SetFont(font12);

            ICellStyle styleHead10 = workbook.CreateCellStyle();
            IFont font10 = workbook.CreateFont();
            font10.FontHeightInPoints = 10;
            font10.FontName = "新細明體";
            styleHead10.FillPattern = FillPattern.SolidForeground;
            styleHead10.FillForegroundColor = HSSFColor.White.Index;
            styleHead10.BorderTop = BorderStyle.Thin;
            styleHead10.BorderLeft = BorderStyle.Thin;
            styleHead10.BorderRight = BorderStyle.Thin;
            styleHead10.BorderBottom = BorderStyle.Thin;
            styleHead10.WrapText = true;
            styleHead10.Alignment = HorizontalAlignment.Left;
            styleHead10.VerticalAlignment = VerticalAlignment.Center;
            styleHead10.SetFont(font10);
            #endregion

            #region title
            //*大標題 line0
            SetExcelCell(sheet, 0, 0, styleHead12, "理債人員查詢");
            sheet.AddMergedRegion(new CellRangeAddress(0, 0, 0, 6));

            //*查詢條件 line1
            SetExcelCell(sheet, 1, 0, styleHead10, "順序");
            sheet.AddMergedRegion(new CellRangeAddress(1, 1, 0, 0));
            SetExcelCell(sheet, 1, 1, styleHead10, "案件編號");
            sheet.AddMergedRegion(new CellRangeAddress(1, 1, 1, 1));
            sheet.SetColumnWidth(1, 100 * 50);
            SetExcelCell(sheet, 1, 2, styleHead10, "客戶ID");
            sheet.AddMergedRegion(new CellRangeAddress(1, 1, 2, 2));
            sheet.SetColumnWidth(2, 100 * 50);
            SetExcelCell(sheet, 1, 3, styleHead10, "類別");
            sheet.SetColumnWidth(3, 100 * 50);
            sheet.AddMergedRegion(new CellRangeAddress(1, 1, 3, 3));
            SetExcelCell(sheet, 1, 4, styleHead10, "細分類");
            sheet.AddMergedRegion(new CellRangeAddress(1, 1, 4, 4));
            sheet.SetColumnWidth(4, 100 * 50);
            SetExcelCell(sheet, 1, 5, styleHead10, "來文字號");
            sheet.AddMergedRegion(new CellRangeAddress(1, 1, 5, 5));
            sheet.SetColumnWidth(5, 100 * 50);
            SetExcelCell(sheet, 1, 6, styleHead10, "建檔日期");
            sheet.AddMergedRegion(new CellRangeAddress(1, 1, 6, 6));
            #endregion

            #region body
            for (int iRow = 0; iRow < dt.Rows.Count; iRow++)
            {
                for (int iCol = 0; iCol < dt.Columns.Count - 1; iCol++)
                {
                    SetExcelCell(sheet, iRow + 2, iCol, styleHead10, Convert.ToString(dt.Rows[iRow][iCol]));
                }
            }
            #endregion

            MemoryStream ms = new MemoryStream();
            workbook.Write(ms);
            ms.Flush();
            ms.Position = 0;
            workbook = null;
            return ms;
        }
        #endregion
        #endregion

        #region 寫入單元格方法
        public ICell SetExcelCell(ISheet sheet, int rowNum, int colNum, ICellStyle style, string value)
        {

            IRow row = sheet.GetRow(rowNum) ?? sheet.CreateRow(rowNum);
            ICell cell = row.GetCell(colNum) ?? row.CreateCell(colNum);
            cell.CellStyle = style;
            cell.SetCellValue(value);
            return cell;
        }
        #endregion

        #region 發文郵寄
        public IList<CaseMaster> GetQueryList(CaseMaster model, int pageIndex, string strSortExpression, string strSortDirection, ref string strsqlWhere)
        {
            try
            {
                base.PageIndex = pageIndex;
                string sqlStr = "";
                string sqlWhere = "";
                strsqlWhere = string.Empty;
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@pageS", (base.PageSize * (base.PageIndex - 1)) + 1));
                base.Parameter.Add(new CommandParameter("@pageE", base.PageSize * base.PageIndex));

                if (!string.IsNullOrEmpty(model.GovName))
                {
                    sqlWhere += @" and GovName like @GovName ";
                    strsqlWhere += @" and GovName like '%" + model.GovName + "%'";
                    base.Parameter.Add(new CommandParameter("@GovName", "%" + model.GovName + "%"));
                }
                if (!string.IsNullOrEmpty(model.CaseNo))
                {
                    sqlWhere += @" and m.CaseNo like @CaseNo ";
                    strsqlWhere += @" and m.CaseNo like '%" + model.CaseNo + "%' ";
                    base.Parameter.Add(new CommandParameter("@CaseNo", "%" + model.CaseNo + "%"));
                }
                if (!string.IsNullOrEmpty(model.SendDateStart))
                {
                    sqlWhere += @" and SendDate >= @SendDateStart";
                    strsqlWhere += @" and SendDate >= '" + model.SendDateStart + "' ";
                    base.Parameter.Add(new CommandParameter("@SendDateStart", model.SendDateStart));
                }
                if (!string.IsNullOrEmpty(model.SendDateEnd))
                {
                    string strSendDateEnd = Convert.ToDateTime(UtlString.FormatDateString(model.SendDateEnd)).AddDays(1).ToString("yyyyMMdd");
                    sqlWhere += @" and SendDate < @SendDateEnd ";
                    strsqlWhere += @" and SendDate < '" + strSendDateEnd + "' ";
                    base.Parameter.Add(new CommandParameter("@SendDateEnd", strSendDateEnd));
                }
                if (!string.IsNullOrEmpty(model.SendWord))
                {
                    sqlWhere += @" and (css.SendWord like @SendWord or css.SendNo like @SendWord)";
                    strsqlWhere += @" and (css.SendWord like '%" + model.SendWord + "%' or css.SendNo like '%" + model.SendWord + "%')";
                    base.Parameter.Add(new CommandParameter("@SendWord", "%" + model.SendWord + "%"));
                }
                if (!string.IsNullOrEmpty(model.CloseDateStart))
                {
                    sqlWhere += @" and CloseDate >= @CloseDateStart";
                    strsqlWhere += @" and CloseDate >= '" + model.CloseDateStart + "' ";
                    base.Parameter.Add(new CommandParameter("@CloseDateStart", model.CloseDateStart));
                }
                if (!string.IsNullOrEmpty(model.CloseDateEnd))
                {
                    string CloseDateEnd = Convert.ToDateTime(UtlString.FormatDateString(model.CloseDateEnd)).AddDays(1).ToString("yyyyMMdd");
                    sqlWhere += @" and CloseDate < @CloseDateEnd ";
                    strsqlWhere += @" and CloseDate < '" + CloseDateEnd + "' ";
                    base.Parameter.Add(new CommandParameter("@CloseDateEnd", CloseDateEnd));
                }
                //20170811 RC RQ-2015-019666-020 派件至跨單位 宏祥 add start
                //if (!string.IsNullOrEmpty(model.AgentUser))
                //{
                //    sqlWhere += @" and AgentUser like @AgentUser ";
                //    strsqlWhere += @" and AgentUser like '%" + model.AgentUser + "%' ";
                //    base.Parameter.Add(new CommandParameter("@AgentUser", "%" + model.AgentUser + "%"));
                //}string[] aryAgentDepartmentUser;
                string[] aryAgentDepartmentUser;
                string strAgentDepartmentUser = "";
                if (!string.IsNullOrEmpty(model.AgentDepartmentUser))
                {
                   aryAgentDepartmentUser = model.AgentDepartmentUser.Split('-');
                   strAgentDepartmentUser = aryAgentDepartmentUser.GetValue(0).ToString().Trim();
                   sqlWhere += @" AND AgentUser like @AgentUser ";
                   base.Parameter.Add(new CommandParameter("@AgentUser", "%" + strAgentDepartmentUser + "%"));
                }
                else if (!string.IsNullOrEmpty(model.AgentDepartment2))
                {
                   AgentSettingBIZ agentsettingBiz = new AgentSettingBIZ();
                   aryAgentDepartmentUser = agentsettingBiz.GetAgentSetting(model.AgentDepartment2).Split(',');
                   if (aryAgentDepartmentUser.Length > 0)
                   {
                      for (int i = 0; i < aryAgentDepartmentUser.Length; i++)
                      {
                         strAgentDepartmentUser += "'" + aryAgentDepartmentUser.GetValue(i).ToString().Trim() + "',";
                      }
                      strAgentDepartmentUser = strAgentDepartmentUser.Trim(',');

                      sqlWhere += @" AND AgentUser in (" + strAgentDepartmentUser + ")";
                   }
                }
                else if (!string.IsNullOrEmpty(model.AgentDepartment))
                {
                   AgentSettingBIZ agentsettingBiz = new AgentSettingBIZ();
                   IList<AgentSetting> list = agentsettingBiz.GetAgentDepartmentUserView(model.AgentDepartment);
                   foreach (AgentSetting item in list)
                   {
                      strAgentDepartmentUser += "'" + item.EmpId.ToString().Trim() + "',";
                   }
                   strAgentDepartmentUser = strAgentDepartmentUser.Trim(',');

                   sqlWhere += @" AND AgentUser in (" + strAgentDepartmentUser + ")";
                }
                //20170811 RC RQ-2015-019666-020 派件至跨單位 宏祥 add end
                if (!string.IsNullOrEmpty(model.MailNo1))
                {
                    sqlWhere += @" and MailNo >= @MailNo1 ";
                    strsqlWhere += @" and MailNo >= '" + model.MailNo1 + "' ";
                    base.Parameter.Add(new CommandParameter("@MailNo1", model.MailNo1));
                }
                if (!string.IsNullOrEmpty(model.MailNo2))
                {
                    sqlWhere += @" and MailNo <= @MailNo2 ";
                    strsqlWhere += @" and MailNo <= '" + model.MailNo2 + "' ";
                    base.Parameter.Add(new CommandParameter("@MailNo2", model.MailNo2));
                }

                if (!string.IsNullOrEmpty(model.CreatedUser))
                {
                    sqlWhere += @" and b.CreatedUser like @CreatedUser ";
                    strsqlWhere += @" and b.CreatedUser like '%" + model.CreatedUser + "%' ";
                    base.Parameter.Add(new CommandParameter("@CreatedUser", "%" + model.CreatedUser + "%"));
                }
                if (!string.IsNullOrEmpty(model.MailDateStart))
                {
                    sqlWhere += @" and MailDate >= @MailDateStart";
                    strsqlWhere += @" and MailDate >= '" + model.MailDateStart + "' ";
                    base.Parameter.Add(new CommandParameter("@MailDateStart", model.MailDateStart));
                }
                if (!string.IsNullOrEmpty(model.MailDateEnd))
                {
                    string strMailDateEnd = Convert.ToDateTime(UtlString.FormatDateString(model.MailDateEnd)).AddDays(1).ToString("yyyyMMdd");
                    sqlWhere += @" and MailDate < @MailDateEnd ";
                    strsqlWhere += @" and MailDate < ' " + strMailDateEnd + "' "; ;
                    base.Parameter.Add(new CommandParameter("@MailDateEnd", strMailDateEnd));
                }
                if (model.MailStatus != "0")
                {
                    if (model.MailStatus == "1")
                    {
                        sqlWhere += @" and MailNo is null ";
                        strsqlWhere += @" and MailNo is null ";
                    }
                    if (model.MailStatus == "2")
                    {
                        sqlWhere += @" and MailNo  is not null ";
                        strsqlWhere += @" and MailNo  is not null ";
                    }
                }
                base.Parameter.Add(new CommandParameter("@Status1", CaseStatus.DirectorApprove));
                base.Parameter.Add(new CommandParameter("@Status2", CaseStatus.DirectorApproveSeizureAndPay));
                string strSql = "";
                if (model.PostType == "-1")//全部
                {
                    //sqlStr += @" select  ROW_NUMBER() OVER (ORDER BY GovName) AS Sno,a.CaseId,a.DetailsId,
                    //            a.SendType ,A.GovName,A.GovAddr,
                    //            B.MailNo,CONVERT(varchar(100),b.MailDate,111) as MailDate,b.CreatedUser, 
                    //            css.SendWord,css.SendNo,css.SendDate,
                    //            m.DocNo,m.CaseNo,m.CaseKind,m.CaseKind2,m.AgentUser
                    //            from CaseSendSettingDetails as A
                    //            LEFT OUTER JOIN MAILINFO AS B ON A.CaseId=B.CaseId AND A.DetailsId=B.SendDetailId
                    //            LEFT OUTER JOIN CaseSendSetting as css on A.CaseId=css.CaseId and a.SerialID=css.SerialID
                    //            LEFT OUTER JOIN CaseMaster as M ON A.CaseId=m.CaseId
                    //            where  (css.Template='扣押'  or css.Template='支付' or (css.Template='165調閱' or css.Template='非165調閱及財產申報' or css.Template='財產申報') )  
                    //            and  (m.Status=@Status1 or m.Status=@Status2) and m.ReceiveKind = '紙本' " + sqlWhere;
                    sqlStr += @" select  ROW_NUMBER() OVER (ORDER BY GovName) AS Sno,a.CaseId,a.DetailsId,
                                 a.SendType ,A.GovName,A.GovAddr,
                                B.MailNo,CONVERT(varchar(100),b.MailDate,111) as MailDate,b.CreatedUser, 
                                css.SendWord,css.SendNo,css.SendDate,
                                m.DocNo,m.CaseNo,m.CaseKind,m.CaseKind2,m.AgentUser
                                from CaseSendSettingDetails as A
                                LEFT OUTER JOIN MAILINFO AS B ON A.CaseId=B.CaseId AND A.DetailsId=B.SendDetailId
                                LEFT OUTER JOIN CaseSendSetting as css on A.CaseId=css.CaseId and a.SerialID=css.SerialID
                                LEFT OUTER JOIN CaseMaster as M ON A.CaseId=m.CaseId
                                where (m.Status=@Status1 and css.SendKind='紙本發文' or A.CaseId IN (SELECT CaseId FROM PARMCODE AS A,
                                 CaseHistory AS B WHERE A.CODETYPE = 'EVENT_NAME' AND A.CODENO = @Status2 AND A.CodeDesc = B.Event and css.Template='扣押')) "
                                + sqlWhere;// +" order by GovName,GovAddr,SendType asc";

                    strSql = @";with T1 
	                        as
	                        (
                               " + sqlStr + "),";
                    strSql += @"
	                        T2 as
	                        (
		                         select *, row_number() over (order by Sno ) RowNum
		                        from T1
	                        ),T3 as 
	                        (
		                        select *,(select max(RowNum) from T2) maxnum from T2 
		                        where rownum between @pageS and @pageE
	                        )
	                        select a.* from T3 a order by a.RowNum";
                }
                if (model.PostType == "0")  //扣押類
                {
                    sqlStr += @" select  ROW_NUMBER() OVER (ORDER BY GovName) AS Sno,a.CaseId,a.DetailsId,
                                 a.SendType ,A.GovName,A.GovAddr,
                                B.MailNo,CONVERT(varchar(100),b.MailDate,111) as MailDate,b.CreatedUser, 
                                css.SendWord,css.SendNo,css.SendDate,
                                m.DocNo,m.CaseNo,m.CaseKind,m.CaseKind2,m.AgentUser
                                from CaseSendSettingDetails as A
                                LEFT OUTER JOIN MAILINFO AS B ON A.CaseId=B.CaseId AND A.DetailsId=B.SendDetailId
                                LEFT OUTER JOIN CaseSendSetting as css on A.CaseId=css.CaseId and a.SerialID=css.SerialID
                                LEFT OUTER JOIN CaseMaster as M ON A.CaseId=m.CaseId
                                where (css.Template='扣押')  and  (m.Status=@Status1 and css.SendKind='紙本發文' or A.CaseId IN (SELECT CaseId FROM PARMCODE AS A,
                                 CaseHistory AS B WHERE A.CODETYPE = 'EVENT_NAME' AND A.CODENO = @Status2 AND A.CodeDesc = B.Event)) "
                                + sqlWhere;// +" order by GovName,GovAddr,SendType asc";
                    strSql = @";with T1 
	                        as
	                        (
                               " + sqlStr + "),";
                    strSql += @"
	                        T2 as
	                        (
		                         select *, row_number() over (order by  GovName,GovAddr,SendType asc ) RowNum
		                        from T1
	                        ),T3 as 
	                        (
		                        select *,(select max(RowNum) from T2) maxnum from T2 
		                        where rownum between @pageS and @pageE
	                        )
	                        select a.* from T3 a order by a.RowNum";
                }
                if (model.PostType == "1") //支付類
                {
                    //郵寄處理類別為支付類時，查詢結果區未依下列方式排序
                    //(1) 正副本(正本)、支票號碼
                    //(2) 正副本(副本)、受文者、地址(發文資料)
                    //* 20150723 支付沒有中間狀態.算未完成
                    sqlStr += @" select  a.CaseId,a.DetailsId,
                                a.SendType,A.GovName,A.GovAddr,B.MailNo,CONVERT(varchar(100),b.MailDate,111) as MailDate,
                                b.CreatedUser,css.SendWord,css.SendNo,css.SendDate,
                                m.DocNo,m.CaseNo,m.CaseKind,m.CaseKind2,m.AgentUser,C.CheckNo
                                from CaseSendSettingDetails as A
                                LEFT  JOIN MAILINFO AS B ON A.CaseId=B.CaseId AND A.DetailsId=B.SendDetailId
                                LEFT  JOIN CaseSendSetting as css on A.CaseId=css.CaseId and a.SerialID=css.SerialID
                                LEFT  JOIN CaseMaster as M ON A.CaseId=m.CaseId 
                                LEFT JOIN  CasePayeeSetting C ON A.CaseId=C.CaseId and A.SerialID = C.SendId
                                where (css.Template='支付')  and  (m.Status=@Status1)  " + sqlWhere;// +@"
                    //ORDER BY  SendType asc,
                    //        (case SendType when '1' then CheckNo when '2' then 0 END) ASC,
                    //        GovName,
                    //        GovAddr ";
                    strSql = @";with T1 
	                        as
	                        (
                               " + sqlStr + "),";
                    strSql += @"
	                        T2 as
	                        (
		                         select *, row_number() over (ORDER BY  SendType ,(case SendType when '1' then CheckNo when '2' then 0 END) ,
                                        GovName,
                                        GovAddr asc ) RowNum
		                        from T1
	                        ),T3 as 
	                        (
		                        select *,(select max(RowNum) from T2) maxnum from T2 
		                        where rownum between @pageS and @pageE
	                        )
	                        select a.* from T3 a order by a.RowNum";
                }
                if (model.PostType == "2")  //外來文類
                {
                    sqlStr += @" select  ROW_NUMBER() OVER (ORDER BY GovName) AS Sno,a.CaseId,a.DetailsId,
                                 a.SendType,A.GovName,
                                B.MailNo,CONVERT(varchar(100),b.MailDate,111) as MailDate,b.CreatedUser, 
                                css.SendWord,css.SendNo,css.SendDate,
                                m.DocNo,m.CaseNo,m.CaseKind,m.CaseKind2,m.AgentUser
                                from CaseSendSettingDetails as A
                                LEFT  JOIN MAILINFO AS B ON A.CaseId=B.CaseId AND A.DetailsId=B.SendDetailId
                                LEFT  JOIN CaseSendSetting as css on A.CaseId=css.CaseId and a.SerialID=css.SerialID
                                LEFT  JOIN CaseMaster as M ON A.CaseId=m.CaseId 
                                where (css.Template='165調閱' or css.Template='非165調閱及財產申報'  or css.Template='財產申報' )  and  (m.Status=@Status1 or m.Status=@Status2) " + sqlWhere;// +" order by AgentUser,CaseNo,SendType asc ";
                    strSql = @";with T1 
	                        as
	                        (
                               " + sqlStr + "),";
                    strSql += @"
	                        T2 as
	                        (
		                         select *, row_number() over (order by AgentUser,CaseNo,SendType asc ) RowNum
		                        from T1
	                        ),T3 as 
	                        (
		                        select *,(select max(RowNum) from T2) maxnum from T2 
		                        where rownum between @pageS and @pageE
	                        )
	                        select a.* from T3 a order by a.RowNum";
                }
                /// adam
                /// 
                ///
                //                string strSql = @";with T1 
                //	                        as
                //	                        (
                //                               " + sqlStr + "),";
                //                       strSql +=@"
                //	                        T2 as
                //	                        (
                //		                         select *, row_number() over (order by " + strSortExpression + " " + strSortDirection + @" ) RowNum
                //		                        from T1
                //	                        ),T3 as 
                //	                        (
                //		                        select *,(select max(RowNum) from T2) maxnum from T2 
                //		                        where rownum between @pageS and @pageE
                //	                        )
                //	                        select a.* from T3 a order by a.RowNum";

                IList<CaseMaster> _ilsit = base.SearchList<CaseMaster>(strSql);

                //IList<CaseMaster> _ilsit = base.SearchList<CaseMaster>(sqlStr);

                if (_ilsit != null)
                {
                    if (_ilsit.Count > 0)
                    {
                        base.DataRecords = _ilsit[0].maxnum;
                    }
                    else
                    {
                        base.DataRecords = 0;
                        _ilsit = new List<CaseMaster>();
                    }
                    return _ilsit;
                }
                else
                {
                    return new List<CaseMaster>();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region 設定掛號號碼
        public int CreateMailNo(CaseMaster model, string[] aryId, string userId)
        {
            /// adam
            int intType = 1;
            string sno = "0";
            string strSendType = "";
            string strCaseKind = "";
            string strCaseKind2 = "";
            string strAfterSeizureApproved = "";
            DataTable DT = new DataTable();
            DataTable dtMap = new DataTable();
            dtMap.Columns.Add("DetailsId");
            dtMap.Columns.Add("No");
            DataTable dtGovAddress = new DataTable();
            dtGovAddress.Columns.Add("GovName");
            dtGovAddress.Columns.Add("GovAddr");
            dtGovAddress.Columns.Add("MailNo");
            /// adam
            CaseSendSettingBIZ caseSend = new CaseSendSettingBIZ();
            IDbConnection dbConnection = OpenConnection();
            IDbTransaction dbTransaction = null;
            try
            {
                using (dbConnection)
                {
                    dbTransaction = dbConnection.BeginTransaction();

                    string strMailNo = model.MailNo;//掛號號碼
                    string strMailNo2 = model.MailNo;//掛號號碼
                    string strMailNo1 = model.MailNo;//地址有重複的時候的掛號號碼
                    string strGovName = "";
                    string strTemplate = "";
                    string strGovAddr = "";
                    string strDetailsId = "";
                    List<string> DetailIdList = aryId.ToList();//獲取detailsId

                    foreach (var item in DetailIdList)//判斷是否是當天設置的
                    {
                        string mailCreateDate = MailNoFroCreateDate(item.Split('|')[0]);//item
                        if (item.Split('|')[0].ToString().Trim().Length > 0)
                        {
                            strDetailsId = strDetailsId + "'" + item.Split('|')[0].ToString() + "',";
                            DataRow drMap = dtMap.NewRow();
                            drMap["DetailsId"] = item.Split('|')[0].ToString().Trim();
                            drMap["No"] = item.Split('|')[1].ToString().Trim();
                            dtMap.Rows.Add(drMap);
                        }
                        if (mailCreateDate != "" && mailCreateDate != null)
                        {
                            if (mailCreateDate != DateTime.Now.ToString("yyyy/MM/dd"))
                            {
                                dbTransaction.Rollback();
                                return 2;
                            }
                        }
                    }
                    if (strDetailsId.Length > 0)
                    {
                        strDetailsId = strDetailsId.Substring(0, strDetailsId.Length - 1);
                    }
                    DT = QueryMailList(strDetailsId);
                    IList<CaseSendSettingDetails> list = caseSend.GetSendSettingDetailsByOrder(DetailIdList);
                    //IList<CaseSendSettingDetails> list = caseSend.GetSendSettingDetails(DetailIdList);
                    dtGovAddress.Clear();
                    foreach (var item in list)
                    {
                        ///adam
                        DataRow[] rowrelation = DT.Select("DetailsId='" + item.DetailsId + "'");
                        if (rowrelation.Count() > 0)
                        {
                            sno = rowrelation[0]["Sno"].ToString();
                            strSendType = rowrelation[0]["SendType"].ToString();// 1 正本 2 副本
                            strCaseKind = rowrelation[0]["CaseKind"].ToString();
                            strCaseKind2 = rowrelation[0]["CaseKind2"].ToString();
                            strTemplate = rowrelation[0]["Template"].ToString();
                        }

                        ///adam
                        ///分成3種案件處理
                        ///int 1 扣押 2 支付 3 外來文 
                        if (strCaseKind == "扣押案件" && strCaseKind2 == "扣押")
                        {
                            intType = 1;
                        }
                        if (strCaseKind == "扣押案件" && strCaseKind2 == "支付")
                        {
                            intType = 2;
                        }
                        if (strCaseKind == "扣押案件" && strCaseKind2 == "扣押並支付")
                        {
                            if (strTemplate == "扣押")
                            {
                                intType = 1;
                            }
                            else
                            {
                                intType = 2;
                            }
                        }

                        if (strCaseKind == "外來文案件")
                        {
                            intType = 3;
                        }
                        switch (intType)
                        {
                            case 1:
                                DataRow[] rowrelation1 = dtGovAddress.Select("GovName='" + item.GovName + "' and GovAddr ='" + item.GovAddr + "'");
                                if (rowrelation1.Count() == 0)
                                //if (item.GovName != strGovName || item.GovAddr != strGovAddr)//第一次如果沒有出現重複的時候
                                {
                                    Create(item.CaseId, item.DetailsId.ToString(), userId, strMailNo, model.MailDate, dbTransaction);
                                    strMailNo2 = strMailNo;
                                    strGovName = item.GovName;
                                    strGovAddr = item.GovAddr;
                                    strMailNo = Convert.ToString(Convert.ToInt32(strMailNo) + 1);//地址不同時,將mailNo加1                                   
                                }
                                else
                                {
                                    //strMailNo1 = Convert.ToString(Convert.ToInt32(strMailNo) - 1);//當遇到相同地址時,再將mailno減1
                                    strMailNo1 = Convert.ToString(Convert.ToInt32(rowrelation1[0]["MailNo"].ToString()));//取出原來編號
                                    Create(item.CaseId, item.DetailsId.ToString(), userId, strMailNo1, model.MailDate, dbTransaction);
                                    strMailNo2 = strMailNo1;
                                }
                                DataRow drGovAddress1 = dtGovAddress.NewRow();
                                drGovAddress1["GovName"] = item.GovName;
                                drGovAddress1["GovAddr"] = item.GovAddr;
                                drGovAddress1["MailNo"] = strMailNo2;
                                dtGovAddress.Rows.Add(drGovAddress1);
                                break;
                            case 2:
                                DataRow[] rowrelation2 = dtGovAddress.Select("GovName='" + item.GovName + "' and GovAddr ='" + item.GovAddr + "'");
                                if (rowrelation2.Count() == 0)
                                //if (item.GovName != strGovName || item.GovAddr != strGovAddr)//第一次如果沒有出現重複的時候
                                {
                                    Create(item.CaseId, item.DetailsId.ToString(), userId, strMailNo, model.MailDate, dbTransaction);
                                    strMailNo2 = strMailNo;
                                    strGovName = item.GovName;
                                    strGovAddr = item.GovAddr;
                                    strMailNo = Convert.ToString(Convert.ToInt32(strMailNo) + 1);//地址不同時,將mailNo加1
                                }
                                else
                                {
                                    if (strSendType == "2")
                                    {
                                        //strMailNo1 = Convert.ToString(Convert.ToInt32(strMailNo) - 1);//當遇到相同地址時,再將mailno減1
                                        strMailNo1 = Convert.ToString(Convert.ToInt32(rowrelation2[0]["MailNo"].ToString()));//取出原來編號
                                        Create(item.CaseId, item.DetailsId.ToString(), userId, strMailNo1, model.MailDate, dbTransaction);
                                        strMailNo2 = strMailNo1;
                                    }
                                    else
                                    {
                                        Create(item.CaseId, item.DetailsId.ToString(), userId, strMailNo, model.MailDate, dbTransaction);
                                        strMailNo2 = strMailNo;
                                        strGovName = item.GovName;
                                        strGovAddr = item.GovAddr;
                                        strMailNo = Convert.ToString(Convert.ToInt32(strMailNo) + 1);
                                    }
                                }
                                DataRow drGovAddress2 = dtGovAddress.NewRow();
                                drGovAddress2["GovName"] = item.GovName;
                                drGovAddress2["GovAddr"] = item.GovAddr;
                                drGovAddress2["MailNo"] = strMailNo2;
                                dtGovAddress.Rows.Add(drGovAddress2);
                                break;
                            case 3:
                                Create(item.CaseId, item.DetailsId.ToString(), userId, strMailNo, model.MailDate, dbTransaction);
                                strMailNo2 = strMailNo;
                                strGovName = item.GovName;
                                strGovAddr = item.GovAddr;
                                strMailNo = Convert.ToString(Convert.ToInt32(strMailNo) + 1);//地址不同時,將mailNo加1
                                DataRow drGovAddress3 = dtGovAddress.NewRow();
                                drGovAddress3["GovName"] = item.GovName;
                                drGovAddress3["GovAddr"] = item.GovAddr;
                                drGovAddress3["MailNo"] = strMailNo2;
                                dtGovAddress.Rows.Add(drGovAddress3);
                                break;
                        }
                    }
                    dbTransaction.Commit();
                }
                return 1;
            }
            catch (Exception ex)
            {
                try
                {
                    dbTransaction.Rollback();
                }
                catch (Exception ex2)
                {
                }
                throw ex;
            }
        }
        //新增郵件清單包含正副本案件類型
        public DataTable QueryMailList(string strDetailsId)
        {
            string sqlStr = @" select  ROW_NUMBER() OVER (ORDER BY GovName) AS Sno,a.CaseId,a.DetailsId,
                                 a.SendType,A.GovName,
                                B.MailNo,CONVERT(varchar(100),b.MailDate,111) as MailDate,b.CreatedUser, 
                                css.SendWord,css.SendNo,css.SendDate,
                                m.DocNo,m.CaseNo,m.CaseKind,m.CaseKind2,m.AgentUser,m.AfterSeizureApproved,css.Template
                                from CaseSendSettingDetails as A
                                LEFT  JOIN MAILINFO AS B ON A.CaseId=B.CaseId AND A.DetailsId=B.SendDetailId
                                LEFT  JOIN CaseSendSetting as css on A.CaseId=css.CaseId and a.SerialID=css.SerialID
                                --left  join CaseSendSettingDetails CSD as CSD  on a.DetailsId = CSD.DetailsId
                                LEFT  JOIN CaseMaster as M ON A.CaseId=m.CaseId where a.DetailsId in (" + strDetailsId + ")";
            //base.Parameter.Clear();
            //base.Parameter.Add(new CommandParameter("@DetailsId", strDetailsId));
            return (DataTable)base.Search(sqlStr);
        }

        //新增mailNo表
        public int Create(Guid CaseId, string id, string userId, string mailNo, string mailDate, IDbTransaction trans = null)
        {
            string sqlStr = @"delete from MailInfo where SendDetailId=@SendDetailId; insert into MailInfo (CaseId,SendDetailId,MailNo,MailDate,CreatedUser,CreatedDate) 
                                          values(@CaseId,@SendDetailId,@MailNo,@MailDate,@CreatedUser,GetDate());";

            base.Parameter.Clear();
            base.Parameter.Add(new CommandParameter("@CaseId", CaseId));
            base.Parameter.Add(new CommandParameter("@SendDetailId", id));
            base.Parameter.Add(new CommandParameter("@MailNo", mailNo));
            base.Parameter.Add(new CommandParameter("@MailDate", mailDate));
            base.Parameter.Add(new CommandParameter("@CreatedUser", userId));
            return base.ExecuteNonQuery(sqlStr, trans);
        }

        //根據DetailId查詢mailNo表新增時間
        public string MailNoFroCreateDate(string detailsId)
        {
            string strSql = @"SELECT  Convert(nvarchar(10),CreatedDate,111) as CreatedDate FROM MailInfo
                            WHERE SendDetailId = @DetailsId ";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("@DetailsId", detailsId));
            return (string)base.ExecuteScalar(strSql);
            //return SearchList<CaseSendSettingDetails>(strSql);
        }
        #endregion

        #region 取消掛號號碼
        public int DeleteMailNo(List<string> idList)
        {
            IDbConnection dbConnection = OpenConnection();
            IDbTransaction dbTransaction = null;
            try
            {
                using (dbConnection)
                {
                    dbTransaction = dbConnection.BeginTransaction();
                    foreach (var item in idList)//判斷是否是當天設置的
                    {
                        string mailCreateDate = MailNoFroCreateDate(item.Split('|')[0]);
                        if (mailCreateDate != "" && mailCreateDate != null)
                        {
                            if (mailCreateDate != DateTime.Now.ToString("yyyy/MM/dd"))
                            {
                                dbTransaction.Rollback();
                                return 2;
                            }
                        }
                    }
                    DeleteMailNos(idList, dbTransaction);
                    dbTransaction.Commit();
                }
                return 1;
            }
            catch (Exception ex)
            {
                try
                {
                    dbTransaction.Rollback();
                }
                catch (Exception ex2)
                {
                }
                throw ex;
            }
        }

        public int IsExistsMailNo(List<string> idList)
        {
            string sqlStr = @"select count(*) from MailInfo where 1=2  ";

            base.Parameter.Clear();
            for (int i = 0; i < idList.Count; i++)
            {
                sqlStr = sqlStr + " OR SendDetailId = @SendDetailId" + i + " ";
                Parameter.Add(new CommandParameter("@SendDetailId" + i, idList[i].Split('|')[0]));
            }
            return (int)base.ExecuteScalar(sqlStr);
        }

        public int DeleteMailNos(List<string> idList, IDbTransaction trans = null)
        {
            string sqlStr = @"delete from MailInfo where 1=2 ";

            base.Parameter.Clear();
            for (int i = 0; i < idList.Count; i++)
            {
                sqlStr = sqlStr + " OR SendDetailId = @SendDetailId" + i + " ";
                Parameter.Add(new CommandParameter("@SendDetailId" + i, idList[i].Split('|')[0]));
            }

            return base.ExecuteNonQuery(sqlStr, trans);
        }
        #endregion

        #region  匯出大宗掛號單
        //數據源
        public List<CaseMaster> MailNoSearchList(string postType, string strSqlWhere, string strMailNo1, string strMailNo2)
        {
            string strSql = "";
            string sqlWhere = "";

            if (!string.IsNullOrEmpty(strMailNo1))
            {
                sqlWhere += @" and MailNo >= @MailNo1 ";
                base.Parameter.Add(new CommandParameter("@MailNo1", strMailNo1));
            }
            if (!string.IsNullOrEmpty(strMailNo2))
            {
                sqlWhere += @" and MailNo <= @MailNo2 ";
                base.Parameter.Add(new CommandParameter("@MailNo2", strMailNo2));
            }
            if (postType == "0")
            {
                sqlWhere += @"  and  (m.Status=@Status1 or A.CaseId IN (SELECT CaseId FROM PARMCODE AS A,
                                 CaseHistory AS B WHERE A.CODETYPE = 'EVENT_NAME' AND A.CODENO = @Status2 AND A.CodeDesc = B.Event)) and css.Template='扣押'  ";
            }
            if (postType == "1")
            {
                sqlWhere += " and (m.Status=@Status1 or m.Status=@Status2)  and css.Template='支付'  ";
            }
            if (postType == "2")
            {
                sqlWhere += " and (m.Status=@Status1 or m.Status=@Status2) and (css.Template='165調閱' or css.Template='非165調閱及財產申報'  or css.Template='財產申報')  ";
            }
            base.Parameter.Add(new CommandParameter("@Status1", CaseStatus.DirectorApprove));
            base.Parameter.Add(new CommandParameter("@Status2", CaseStatus.DirectorApproveSeizureAndPay));
            strSql = @"  select   B.MailNo,A.GovName,A.GovAddr 
                                from CaseSendSettingDetails as A
                                LEFT OUTER JOIN MAILINFO AS B ON A.CaseId=B.CaseId AND A.DetailsId=B.SendDetailId
                                LEFT OUTER JOIN CaseSendSetting as css on A.CaseId=css.CaseId and a.SerialID=css.SerialID
                                LEFT OUTER JOIN CaseMaster as M ON A.CaseId=m.CaseId
                                where  1=1 AND MailNo IS NOT NULL " + sqlWhere + strSqlWhere + "    order by   B.MailNo asc";

            List<CaseMaster> list = base.SearchList<CaseMaster>(strSql).ToList();
            if (list != null && list.Count > 0) return list;
            else return new List<CaseMaster>();
        }

        private void MyInsertRow(HSSFSheet sheet, int insertRow, int insertRowCount, HSSFRow sourceRow)
        {
            sheet.ShiftRows(insertRow, sheet.LastRowNum, insertRowCount, true, false, true);

            #region 对批量移动后空出的空行插，创建相应的行，并以插入行的上一行为格式源(即：插入行-1的那一行)
            for (int i = insertRow; i < insertRow + insertRowCount - 1; i++)
            {
                HSSFRow targetRow = null;
                HSSFCell sourceCell = null;
                HSSFCell targetCell = null;

                targetRow = (HSSFRow)sheet.CreateRow(i + 1);
                targetRow.Height = sourceRow.Height;
                for (int m = sourceRow.FirstCellNum; m < sourceRow.LastCellNum; m++)
                {
                    sourceCell = ((HSSFRow)sourceRow).GetCell(m) as HSSFCell;
                    if (sourceCell == null)
                        continue;
                    targetCell = targetRow.CreateCell(m) as HSSFCell;

                    targetCell.CellStyle = sourceCell.CellStyle;
                    targetCell.SetCellType(sourceCell.CellType);
                }
            }

            HSSFRow firstTargetRow = (HSSFRow)sheet.GetRow(insertRow);
            firstTargetRow.Height = sourceRow.Height;
            HSSFCell firstSourceCell = null;
            HSSFCell firstTargetCell = null;
            for (int m = sourceRow.FirstCellNum; m < sourceRow.LastCellNum; m++)
            {
                firstSourceCell = (HSSFCell)sourceRow.GetCell(m);
                if (firstSourceCell == null)
                    continue;
                firstTargetCell = (HSSFCell)firstTargetRow.CreateCell(m);
                firstTargetCell.CellStyle = firstSourceCell.CellStyle;
                firstTargetCell.SetCellType(firstSourceCell.CellType);
            }
            #endregion
        }


        public MemoryStream MailNoExcel_NPOI(string TemplatePath, string postType, string strSqlWhere, string strMailNo1, string strMailNo2, string userId)
        {
            #region 獲取數據源
            List<CaseMaster> list = MailNoSearchList(postType, strSqlWhere, strMailNo1, strMailNo2);

            var queryList = (from lists in list
                             group lists by new { lists.MailNo, lists.GovName, lists.GovAddr } into list2
                             select new
                             {
                                 mailno = list2.Key,
                                 mailCount =
                                     list2.Count(m => m.MailNo == list2.Key.MailNo)
                             }).ToList();
            #endregion

            #region body
            HSSFWorkbook hssfworkbookDown;

            string modelExlPath = TemplatePath;
            //读入刚复制的要导出的excel文件
            using (FileStream file = new FileStream(modelExlPath, FileMode.Open, FileAccess.Read))
            {
                hssfworkbookDown = new HSSFWorkbook(file);
                file.Close();
            }
            IWorkbook workbook = hssfworkbookDown;
            HSSFSheet sheet = (HSSFSheet)workbook.GetSheetAt(1);

            HSSFRow sourceRow = (HSSFRow)sheet.GetRow(20);
            HSSFRow sourceRowAll = (HSSFRow)sheet.GetRow(27);

            SetExcelCell(sheet, 2, 3, sourceRowAll.Cells[6].CellStyle, userId);

            if (queryList != null && queryList.Count > 0)
            {
                if (queryList.Count <= 20)
                {
                    int r = 7;
                    foreach (var item in queryList)
                    {
                        SetExcelCell(sheet, r, 2, sourceRow.Cells[2].CellStyle, item.mailno.MailNo);
                        SetExcelCell(sheet, r, 3, sourceRow.Cells[3].CellStyle, item.mailno.GovName);
                        SetExcelCell(sheet, r, 4, sourceRow.Cells[4].CellStyle, item.mailno.GovAddr);
                        SetExcelCell(sheet, r, 11, sourceRow.Cells[11].CellStyle, item.mailCount.ToString());
                        r++;
                    }
                    SetExcelCell(sheet, 31, 9, sourceRowAll.Cells[6].CellStyle, queryList.Count.ToString());
                }

                if (queryList.Count > 20)
                {
                    int r = 7;
                    foreach (var item in queryList)
                    {
                        if (r > 26)
                        {
                            break;
                        }
                        SetExcelCell(sheet, r, 2, sourceRow.Cells[2].CellStyle, item.mailno.MailNo);
                        SetExcelCell(sheet, r, 3, sourceRow.Cells[3].CellStyle, item.mailno.GovName);
                        SetExcelCell(sheet, r, 4, sourceRow.Cells[4].CellStyle, item.mailno.GovAddr);
                        SetExcelCell(sheet, r, 11, sourceRow.Cells[11].CellStyle, item.mailCount.ToString());
                        r++;
                    }

                    SetExcelCell(sheet, 31, 9, sourceRowAll.Cells[6].CellStyle, queryList.Count.ToString());//前20行數據原模板，剩下數據增加行
                    MyInsertRow(sheet, 27, queryList.Count - 20, sourceRow);//新增行 
                    r = 27;
                    int sort = 20;
                    foreach (var item in queryList.Skip(20))
                    {
                        SetExcelCell(sheet, r, 1, sourceRow.Cells[1].CellStyle, (sort + 1).ToString());//*序號
                        sheet.AddMergedRegion(new CellRangeAddress(r, r, 4, 5));

                        SetExcelCell(sheet, r, 2, sourceRow.Cells[2].CellStyle, item.mailno.MailNo);
                        SetExcelCell(sheet, r, 3, sourceRow.Cells[3].CellStyle, item.mailno.GovName);
                        SetExcelCell(sheet, r, 4, sourceRow.Cells[4].CellStyle, item.mailno.GovAddr);
                        SetExcelCell(sheet, r, 11, sourceRow.Cells[11].CellStyle, item.mailCount.ToString());
                        r++;
                        sort++;
                    }

                    HSSFRow sourceRowAllC = (HSSFRow)sheet.GetRow(queryList.Count + 7);
                    SetExcelCell(sheet, queryList.Count + 11, 9, sourceRowAllC.Cells[6].CellStyle, queryList.Count.ToString());
                }
            }

            //if (dt.Rows.Count > 20)
            //{
            //    for (int iRow = 0; iRow < 20; iRow++)
            //    {
            //        for (int iCol = 0; iCol < dt.Columns.Count; iCol++)
            //        {
            //            SetExcelCell(sheet, iRow + 7, iCol + 2, sourceRow.Cells[iCol + 2].CellStyle, dt.Rows[iRow][iCol].ToString());
            //        }
            //        SetExcelCell(sheet, iRow + 7, 11, sourceRow.Cells[11].CellStyle, dt.Rows[iRow]["MailCount"].ToString());
            //    }

            //    MyInsertRow(sheet, 27, dt.Rows.Count - 20, sourceRow);
            //    for (int iRow = 20; iRow < dt.Rows.Count; iRow++)
            //    {
            //        SetExcelCell(sheet, iRow + 7, 1, sourceRow.Cells[1].CellStyle, (iRow + 1).ToString());
            //        sheet.AddMergedRegion(new CellRangeAddress(iRow + 7, iRow + 7, 4, 5));
            //        for (int iCol = 0; iCol < dt.Columns.Count; iCol++)
            //        {
            //            SetExcelCell(sheet, iRow + 7, iCol + 2, sourceRow.Cells[iCol + 2].CellStyle, dt.Rows[iRow][iCol].ToString());
            //        }
            //        SetExcelCell(sheet, iRow + 7, 11, sourceRow.Cells[11].CellStyle, dt.Rows[iRow]["MailCount"].ToString());
            //    }
            //    HSSFRow sourceRowAllC = (HSSFRow)sheet.GetRow(dt.Rows.Count + 7);
            //    SetExcelCell(sheet, dt.Rows.Count + 11, 9, sourceRowAllC.Cells[6].CellStyle, dt.Rows.Count.ToString());
            //}

            //if (dt.Rows.Count <= 20)//20行以內用模板的數據
            //{
            //    for (int iRow = 0; iRow < dt.Rows.Count; iRow++)
            //    {
            //        for (int iCol = 0; iCol < dt.Columns.Count; iCol++)
            //        {
            //            SetExcelCell(sheet, iRow + 7, iCol + 2, sourceRow.Cells[iCol + 2].CellStyle, dt.Rows[iRow][iCol].ToString());
            //        }
            //        SetExcelCell(sheet, iRow + 7, 11, sourceRow.Cells[11].CellStyle, dt.Rows[iRow]["MailCount"].ToString());
            //    }
            //    SetExcelCell(sheet, 31, 9, sourceRowAll.Cells[6].CellStyle, dt.Rows.Count.ToString());
            //}
            #endregion

            MemoryStream ms = new MemoryStream();
            workbook.Write(ms);
            ms.Flush();
            ms.Position = 0;
            workbook = null;
            return ms;
        }
        #endregion

        #region 發文郵寄 列印函文
        public DataTable MailNoReportList(List<string> detailIdList)
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
									,(select EmpName from [LDAPEmployee] emp where emp.EmpID=M.CreatedUser) as CreatedUser
                                    ,D.DetailsId
                                   ,(select TelNo + ' 分機 '+TelExt from [LDAPEmployee] emp where emp.EmpID=M.CreatedUser) as TelNo                                    
                                FROM [CaseSendSetting] AS M
                                LEFT OUTER JOIN [CaseSendSettingDetails] AS D ON M.CaseId = D.CaseId AND M.SerialID =D.SerialID
                                WHERE 1=2 ";
            Parameter.Clear();
            for (int i = 0; i < detailIdList.Count; i++)
            {
                strsql = strsql + " OR D.DetailsId = @DetailsId" + i + " ";
                Parameter.Add(new CommandParameter("@DetailsId" + i, detailIdList[i].Split('|')[0]));
            }
            strsql = strsql + " order by GovName,GovAddr,SendType asc ";
            DataTable Dt = Search(strsql);
            Dt.Columns.Add("Sort",typeof(Int16));
            Dt.Columns.Add("Receive", typeof(String));
            Dt.Columns.Add("Cc", typeof(String));
            if (Dt != null && Dt.Rows.Count > 0)
            {
                string strSerialId = string.Empty;
                foreach (DataRow dr in Dt.Rows)
                {
                    foreach (string item in detailIdList.Where(m => m.Split('|')[0] == dr["DetailsId"].ToString()))
                    {
                        dr["Sort"] = Convert.ToInt16(item.Split('|')[1]);
                    }
                    strSerialId += "'" + dr["SerialID"].ToString() + "',";
                }
                strSerialId = strSerialId.TrimEnd(',');
                string sqlRecive = "SELECT GovName,SerialID FROM CaseSendSettingDetails WHERE SendType=1 and SerialID In (" + strSerialId + ")";
                string sqlCc = "SELECT GovName,SerialID FROM CaseSendSettingDetails WHERE SendType=2 and SerialID In (" + strSerialId + ")";
                List<CaseSendSettingDetails> listRecive = base.SearchList<CaseSendSettingDetails>(sqlRecive).ToList();
                List<CaseSendSettingDetails> listCc = base.SearchList<CaseSendSettingDetails>(sqlCc).ToList();
                if (listRecive != null || listCc != null) //&& listCc.Any())
                {
                    string strRecive = string.Empty;
                    string strCc = string.Empty;
                    foreach (DataRow dr in Dt.Rows)
                    {
                        foreach (CaseSendSettingDetails item in listRecive.Where(m => m.SerialID == Convert.ToInt32(dr["SerialID"])))
                        {
                            strRecive += item.GovName + "、";
                        }
                        strRecive = strRecive.TrimEnd('、');
                        dr["Receive"] = strRecive;
                        strRecive = "";
                        foreach (CaseSendSettingDetails item in listCc.Where(m => m.SerialID == Convert.ToInt32(dr["SerialID"])))
                        {
                            strCc += item.GovName + "、";
                        }
                        strCc = strCc.TrimEnd('、');
                        dr["Cc"] = strCc;
                        strCc = "";
                    }
                }
                return Dt;
            }
            else
            {
                return Dt = new DataTable();
            }
        }
        #endregion

        #region 手續費查詢
        public List<CaseMaster> CaseMasterForPaySearchList(CaseMaster model, int pageIndex, string strSortExpression, string strSortDirection)
        {
            try
            {
                base.PageIndex = pageIndex;
                string sqlStr = "";
                string sqlWhere = "";
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@pageS", (base.PageSize * (base.PageIndex - 1)) + 1));
                base.Parameter.Add(new CommandParameter("@pageE", base.PageSize * base.PageIndex));

                if (!string.IsNullOrEmpty(model.GovKind))
                {
                    sqlWhere += @" and GovKind like @GovKind ";
                    base.Parameter.Add(new CommandParameter("@GovKind", "%" + model.GovKind + "%"));
                }
                if (!string.IsNullOrEmpty(model.GovUnit))
                {
                    sqlWhere += @" and GovUnit like @GovUnit ";
                    base.Parameter.Add(new CommandParameter("@GovUnit", "%" + model.GovUnit + "%"));
                }
                if (!string.IsNullOrEmpty(model.CaseNo))
                {
                    sqlWhere += @" and CaseNo like @CaseNo ";
                    base.Parameter.Add(new CommandParameter("@CaseNo", "%" + model.CaseNo + "%"));
                }
                if (!string.IsNullOrEmpty(model.GovDateStart))
                {
                    sqlWhere += @" and GovDate >= @GovDateStart";
                    base.Parameter.Add(new CommandParameter("@GovDateStart", model.GovDateStart));
                }
                if (!string.IsNullOrEmpty(model.GovDateEnd))
                {
                    string QDateE = Convert.ToDateTime(model.GovDateEnd).AddDays(1).ToString("yyyyMMdd");
                    sqlWhere += @" and GovDate < @GovDateEND ";
                    base.Parameter.Add(new CommandParameter("@GovDateEND", QDateE));
                }
                if (!string.IsNullOrEmpty(model.Speed))
                {
                    sqlWhere += @" and Speed like @Speed ";
                    base.Parameter.Add(new CommandParameter("@Speed", "%" + model.Speed + "%"));
                }
                if (!string.IsNullOrEmpty(model.ReceiveKind))
                {
                    sqlWhere += @" and ReceiveKind like @ReceiveKind ";
                    base.Parameter.Add(new CommandParameter("@ReceiveKind", "%" + model.ReceiveKind + "%"));
                }
                if (!string.IsNullOrEmpty(model.GovNo))
                {
                    sqlWhere += @" and GovNo like @GovNo ";
                    base.Parameter.Add(new CommandParameter("@GovNo", "%" + model.GovNo + "%"));
                }
                if (!string.IsNullOrEmpty(model.AgentUser))
                {
                    sqlWhere += @" and AgentUser like @AgentUser ";
                    base.Parameter.Add(new CommandParameter("@AgentUser", "%" + model.AgentUser + "%"));
                }
                string sqlWhere1 = string.Empty;
                if (!string.IsNullOrEmpty(model.SendDateS) || !string.IsNullOrEmpty(model.SendDateE))
                {
                    if (!string.IsNullOrEmpty(model.SendDateS))
                    {
                        sqlWhere1 += @" AND SendDate >= @SendDateStart";
                        Parameter.Add(new CommandParameter("@SendDateStart", model.SendDateS));
                    }
                    if (!string.IsNullOrEmpty(model.SendDateE))
                    {
                        string sendDateEnd = Convert.ToDateTime(UtlString.FormatDateString(model.SendDateE)).AddDays(1).ToString("yyyyMMdd");
                        sqlWhere1 += @" AND SendDate < @SendDateEnd ";
                        Parameter.Add(new CommandParameter("@SendDateEnd", sendDateEnd));
                    }
                    sqlWhere += " AND A.CaseId IN (SELECT CaseId  FROM CaseSendSetting AS CSS where 1=1 " + sqlWhere1 + ") ";
                }
                if (model.PayStatus == "1")//*未銷帳
                {
                    sqlWhere += @" and D.ModifiedUser IS NULL ";
                }
                if (model.PayStatus == "2")//*已銷帳
                {
                    sqlWhere += @" and D.ModifiedUser IS NOT NULL ";
                }
                if (!string.IsNullOrEmpty(model.HangingDateStart))
                {
                    sqlWhere += @" and HangingDate >= @HangingDateStart";
                    base.Parameter.Add(new CommandParameter("@HangingDateStart", model.HangingDateStart));
                }
                if (!string.IsNullOrEmpty(model.HangingDateEnd))
                {
                    string QDateE = Convert.ToDateTime(model.HangingDateEnd).AddDays(1).ToString("yyyyMMdd");
                    sqlWhere += @" and HangingDate < @HangingDateEnd ";
                    base.Parameter.Add(new CommandParameter("@HangingDateEnd", QDateE));
                }
                if (!string.IsNullOrEmpty(model.ChargeOffsDateStart))
                {
                    sqlWhere += @" and ChargeOffsDate >= @ChargeOffsDateStart";
                    base.Parameter.Add(new CommandParameter("@ChargeOffsDateStart", model.ChargeOffsDateStart));
                }
                if (!string.IsNullOrEmpty(model.ChargeOffsDateEnd))
                {
                    string QDateE = Convert.ToDateTime(model.ChargeOffsDateEnd).AddDays(1).ToString("yyyyMMdd");
                    sqlWhere += @" and ChargeOffsDate < @ChargeOffsDateEnd ";
                    base.Parameter.Add(new CommandParameter("@ChargeOffsDateEnd", QDateE));
                }


                sqlStr += @";with T1 
	                        as
	                        (
		                       SELECT 
                                        A.CaseId,
                                        CASE WHEN A.CaseKind = '外來文案件' THEN B.PayeeId ELSE C.PayeeId END AS PayeeId,
                                        A.CaseNo,
                                        A.GovUnit,
                                        CASE WHEN A.CaseKind = '外來文案件' THEN B.ReceivePerson ELSE C.ReceivePerson END AS ReceivePerson,
                                        CASE WHEN A.CaseKind = '外來文案件' THEN B.Fee ELSE C.Fee END AS Fee,
                                        CASE WHEN A.CaseKind = '外來文案件' THEN B.SendDate ELSE C.SendDate END AS SendDate,
                                        D.HangingAmount,
                                        D.HangingDate,
                                        D.ChargeOffsAmount,
                                        D.ChargeOffsDate,
                                        D.Memo
                                FROM 
                                [CaseMaster] AS A
                                LEFT OUTER JOIN
                                ( 
	                                        SELECT B1.CaseId,B1.Fee,B2.SendDate,B2.GovName AS ReceivePerson, 1 AS PayeeId, '' AS Memo
	                                        FROM
	                                        (
	                                                SELECT CaseId,SUM([Amount]) AS Fee FROM [CaseAccountExternal] GROUP BY CaseId)  AS B1
	                                                LEFT OUTER JOIN 
	                                                (
		                                                    SELECT CaseId,SendDate,GovName
		                                                    FROM
		                                                    (
		                                                            SELECT B1.CaseId,B1.SendDate,B2.GovName,Row_Number() over(Partition By B1.CaseId Order by B1.SendDate Asc) RowID
		                                                            FROM [CaseSendSetting] AS B1 INNER JOIN [CaseSendSettingDetails] B2 
		                                                            ON B1.CaseId = B2.CaseId AND B1.SerialID = B2.SerialID AND B2.SendType = 1
		                                                    ) AS B WHERE RowId = 1 
	                                                ) AS B2 on B1.CaseId = B2.CaseId
                                 ) AS B ON A.CaseId = B.CaseId AND A.CaseKind = '外來文案件'
                                        LEFT OUTER JOIN
                                        (
	                                        SELECT 
	                                        C1.CaseId, C1.Fee,C2.SendDate,C1.ReceivePerson,C1.PayeeId,C1.Memo
	                                        FROM [CasePayeeSetting] AS C1
	                                        LEFT OUTER JOIN [CaseSendSetting] AS C2 ON C1.[SendId] = C2.SerialID

                                        ) AS C ON A.CaseId = C.CaseId AND A.CaseKind = '扣押案件'

                                        LEFT OUTER JOIN [FeeChargeOffs] AS D ON A.CaseId = D.CaseId AND 
                                        CASE WHEN A.CaseKind = '外來文案件' THEN B.PayeeId ELSE C.PayeeId END = D.PayeeId
                                        WHERE (B.Fee > 0 OR C.Fee > 0) 
                                        AND 
                                        (
	                                        (A.CaseKind2 = '扣押並支付' AND A.ApproveDate2 IS NOT NULL)
	                                        OR
	                                        (A.CaseKind2 <> '扣押並支付' AND A.ApproveDate IS NOT NULL)
                                        ) " + sqlWhere + @"   
	                        ),T2 as
	                        (
		                        select *, row_number() over (order by " + strSortExpression + " " + strSortDirection + @" ) RowNum
		                        from T1
	                        ),T3 as 
	                        (
		                        select *,(select max(RowNum) from T2) maxnum from T2 
		                        where rownum between @pageS and @pageE
	                        )
	                        select a.* from T3 a order by a.RowNum";

                IList<CaseMaster> _ilsit = base.SearchList<CaseMaster>(sqlStr);
                List<CaseMaster> listitem = new List<CaseMaster>();
                if (_ilsit != null)
                {
                    if (_ilsit.Count > 0)
                    {
                        base.DataRecords = _ilsit[0].maxnum;
                    }
                    else
                    {
                        base.DataRecords = 0;
                        _ilsit = new List<CaseMaster>();
                    }
                    foreach (var item in _ilsit)
                    {
                        listitem.Add(item);
                    }
                    return listitem;
                }
                else
                {
                    return new List<CaseMaster>();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region 設定掛號日期
        /// <summary>
        /// 
        /// </summary>
        /// <param name="aryId"></param>
        /// <param name="hangingDate"></param>
        /// <returns></returns>
        public JsonReturn SetHangingDate(string[] aryId, string hangingDate)
        {
            IDbConnection dbConnection = OpenConnection();
            IDbTransaction dbTransaction = null;
            try
            {
                using (dbConnection)
                {
                    dbTransaction = dbConnection.BeginTransaction();
                    foreach (var item in aryId)
                    {
                        string caseId = item.Split('|')[0];
                        string payeeId = item.Split('|')[1];
                        FeeChargeOffs oldObj = GetFeeChargeOffs(caseId, payeeId, dbTransaction);

                        if (string.IsNullOrEmpty(hangingDate))
                        {
                            //* 1.如果該筆已經設置了銷帳則不能刪除
                            if (oldObj != null && (oldObj.ChargeOffsDate.HasValue || oldObj.ChargeOffsAmount > 0))
                            {
                                dbTransaction.Rollback();
                                return new JsonReturn { ReturnCode = "0", ReturnMsg = Lang.csfs_alreadychargeoff };
                            }
                            //* 否則清空資料
                            DeleteFeeChargeOffs(caseId, payeeId, dbTransaction);
                        }
                        else
                        {
                            CreateFees(caseId, payeeId, Account, hangingDate, dbTransaction);
                        }
                    }
                    dbTransaction.Commit();
                }
                return new JsonReturn { ReturnCode = "1", ReturnMsg = Lang.csfs_save_ok };
            }
            catch (Exception ex)
            {
                try
                {
                    if (dbTransaction != null) dbTransaction.Rollback();
                }
                catch (Exception ex2)
                {
                }
                throw ex;
            }
        }

        /// <summary>
        /// 根據CaseId和受款人ID取得手續費信息
        /// </summary>
        /// <param name="caseId"></param>
        /// <param name="payeeId"></param>
        /// <param name="trans"></param>
        /// <returns></returns>
        public FeeChargeOffs GetFeeChargeOffs(string caseId, string payeeId, IDbTransaction trans = null)
        {
            string strSql = @"SELECT 
	                            [FeeId]
	                            ,[CaseId]
	                            ,[PayeeId]
	                            ,[HangingAmount]
	                            ,[HangingDate]
	                            ,[ChargeOffsAmount]
	                            ,[ChargeOffsDate]
	                            ,[CreatedUser]
	                            ,[CreatedDate]
	                            ,[ModifiedUser]
	                            ,[ModifiedDate]
	                            ,[Memo]
                            FROM [FeeChargeOffs]
                            WHERE [CaseId] = @CaseId AND [PayeeId] = @PayeeId";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("CaseId", caseId));
            Parameter.Add(new CommandParameter("PayeeId", payeeId));
            IList<FeeChargeOffs> list = trans == null ? SearchList<FeeChargeOffs>(strSql) : SearchList<FeeChargeOffs>(strSql, trans);
            if (list == null || !list.Any())
                return null;
            return list.FirstOrDefault();
        }

        public bool DeleteFeeChargeOffs(string caseId, string payeeId, IDbTransaction trans = null)
        {
            string strSql = @"DELETE [FeeChargeOffs] WHERE [CaseId] = @CaseId AND [PayeeId] = @PayeeId";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("CaseId", caseId));
            Parameter.Add(new CommandParameter("PayeeId", payeeId));
            return trans == null ? ExecuteNonQuery(strSql) > 0 : ExecuteNonQuery(strSql, trans) > 0;
        }
        //新增FeesChargeOff表
        public int CreateFees(string CaseId, string PayeeId, string userId, string hangingDate, IDbTransaction trans = null)
        {
            string sqlStr = @"delete from FeeChargeOffs where CaseId=@CaseId and PayeeId=@PayeeId; 
                                insert into FeeChargeOffs (CaseId,PayeeId,HangingDate,CreatedUser,CreatedDate) 
                                          values(@CaseId,@PayeeId,@HangingDate,@CreatedUser,GetDate());";

            base.Parameter.Clear();
            base.Parameter.Add(new CommandParameter("@CaseId", CaseId));
            base.Parameter.Add(new CommandParameter("@PayeeId", PayeeId));
            base.Parameter.Add(new CommandParameter("@HangingDate", hangingDate));
            base.Parameter.Add(new CommandParameter("@CreatedUser", userId));
            return base.ExecuteNonQuery(sqlStr, trans);
        }
        #endregion

        #region 銷帳
        public int SetChargeDate(Guid CaseId, string PayeeId, string ChargeDate, string ChargeAmount, string Memo, string userId)
        {
            IDbConnection dbConnection = OpenConnection();
            IDbTransaction dbTransaction = null;
            try
            {
                using (dbConnection)
                {
                    dbTransaction = dbConnection.BeginTransaction();
                    EditFees(CaseId, PayeeId, ChargeDate, ChargeAmount, Memo, userId, dbTransaction);
                    dbTransaction.Commit();
                }
                return 1;
            }
            catch (Exception ex)
            {
                try
                {
                    dbTransaction.Rollback();
                }
                catch (Exception ex2)
                {
                }
                throw ex;
            }
        }

        //新增FeesChargeOff表
        public int EditFees(Guid CaseId, string PayeeId, string ChargeDate, string ChargeAmount, string Memo, string userId, IDbTransaction trans = null)
        {
            string sqlStr = @" Update FeeChargeOffs set ChargeOffsDate=@ChargeDate, ChargeOffsAmount=@ChargeAmount,Memo=@Memo,
                                        ModifiedUser=@ModifiedUser,ModifiedDate=GetDate()
                                        where  CaseId=@CaseId and PayeeId=@PayeeId";

            base.Parameter.Clear();
            base.Parameter.Add(new CommandParameter("@CaseId", CaseId));
            base.Parameter.Add(new CommandParameter("@PayeeId", PayeeId));
            base.Parameter.Add(new CommandParameter("@ChargeDate", ChargeDate));
            base.Parameter.Add(new CommandParameter("@ChargeAmount", ChargeAmount));
            base.Parameter.Add(new CommandParameter("@Memo", Memo));
            base.Parameter.Add(new CommandParameter("@ModifiedUser", userId));
            return base.ExecuteNonQuery(sqlStr, trans);
        }
        #endregion

        #region 手續費統計表匯出
        // 手續費統計表導出數據源
        public DataTable FeeChargeSearchList(CaseMaster model)
        {
            string sqlStr = "";
            string sqlWhere = "";
            base.Parameter.Clear();

            if (!string.IsNullOrEmpty(model.GovKind))
            {
                sqlWhere += @" and GovKind like @GovKind ";
                base.Parameter.Add(new CommandParameter("@GovKind", "%" + model.GovKind + "%"));
            }
            if (!string.IsNullOrEmpty(model.GovUnit))
            {
                sqlWhere += @" and GovUnit like @GovUnit ";
                base.Parameter.Add(new CommandParameter("@GovUnit", "%" + model.GovUnit + "%"));
            }
            if (!string.IsNullOrEmpty(model.CaseNo))
            {
                sqlWhere += @" and CaseNo like @CaseNo ";
                base.Parameter.Add(new CommandParameter("@CaseNo", "%" + model.CaseNo + "%"));
            }
            if (!string.IsNullOrEmpty(model.GovDateStart))
            {
                sqlWhere += @" and GovDate >= @GovDateStart";
                base.Parameter.Add(new CommandParameter("@GovDateStart", model.GovDateStart));
            }
            if (!string.IsNullOrEmpty(model.GovDateEnd))
            {
                string QDateE = Convert.ToDateTime(model.GovDateEnd).AddDays(1).ToString("yyyyMMdd");
                sqlWhere += @" and GovDate < @GovDateEND ";
                base.Parameter.Add(new CommandParameter("@GovDateEND", QDateE));
            }
            if (!string.IsNullOrEmpty(model.Speed))
            {
                sqlWhere += @" and Speed like @Speed ";
                base.Parameter.Add(new CommandParameter("@Speed", "%" + model.Speed + "%"));
            }
            if (!string.IsNullOrEmpty(model.ReceiveKind))
            {
                sqlWhere += @" and ReceiveKind like @ReceiveKind ";
                base.Parameter.Add(new CommandParameter("@ReceiveKind", "%" + model.ReceiveKind + "%"));
            }
            if (!string.IsNullOrEmpty(model.GovNo))
            {
                sqlWhere += @" and GovNo like @GovNo ";
                base.Parameter.Add(new CommandParameter("@GovNo", "%" + model.GovNo + "%"));
            }
            if (!string.IsNullOrEmpty(model.AgentUser))
            {
                sqlWhere += @" and AgentUser like @AgentUser ";
                base.Parameter.Add(new CommandParameter("@AgentUser", "%" + model.AgentUser + "%"));
            }
            string sqlWhere1 = string.Empty;
            if (!string.IsNullOrEmpty(model.SendDateS) || !string.IsNullOrEmpty(model.SendDateE))
            {
                if (!string.IsNullOrEmpty(model.SendDateS))
                {
                    sqlWhere1 += @" AND SendDate >= @SendDateStart";
                    Parameter.Add(new CommandParameter("@SendDateStart", model.SendDateS));
                }
                if (!string.IsNullOrEmpty(model.SendDateE))
                {
                    string sendDateEnd = Convert.ToDateTime(UtlString.FormatDateString(model.SendDateE)).AddDays(1).ToString("yyyyMMdd");
                    sqlWhere1 += @" AND SendDate < @SendDateEnd ";
                    Parameter.Add(new CommandParameter("@SendDateEnd", sendDateEnd));
                }
                sqlWhere += " AND A.CaseId IN (SELECT CaseId  FROM CaseSendSetting AS CSS where 1=1 " + sqlWhere1 + ") ";
            }
            if (model.PayStatus == "1")//*未銷帳
            {
                sqlWhere += @" and D.ModifiedUser IS NULL ";
            }
            if (model.PayStatus == "2")//*已銷帳
            {
                sqlWhere += @" and D.ModifiedUser IS NOT NULL ";
            }
            if (!string.IsNullOrEmpty(model.HangingDateStart))
            {
                sqlWhere += @" and HangingDate >= @HangingDateStart";
                base.Parameter.Add(new CommandParameter("@HangingDateStart", model.HangingDateStart));
            }
            if (!string.IsNullOrEmpty(model.HangingDateEnd))
            {
                string QDateE = Convert.ToDateTime(model.HangingDateEnd).AddDays(1).ToString("yyyyMMdd");
                sqlWhere += @" and HangingDate < @HangingDateEnd ";
                base.Parameter.Add(new CommandParameter("@HangingDateEnd", QDateE));
            }
            if (!string.IsNullOrEmpty(model.ChargeOffsDateStart))
            {
                sqlWhere += @" and ChargeOffsDate >= @ChargeOffsDateStart";
                base.Parameter.Add(new CommandParameter("@ChargeOffsDateStart", model.ChargeOffsDateStart));
            }
            if (!string.IsNullOrEmpty(model.ChargeOffsDateEnd))
            {
                string QDateE = Convert.ToDateTime(model.ChargeOffsDateEnd).AddDays(1).ToString("yyyyMMdd");
                sqlWhere += @" and ChargeOffsDate < @ChargeOffsDateEnd ";
                base.Parameter.Add(new CommandParameter("@ChargeOffsDateEnd", QDateE));
            }


            sqlStr += @" SELECT 
                                        A.CaseNo,
                                        A.GovUnit,
                                        CASE WHEN A.CaseKind = '外來文案件' THEN B.ReceivePerson ELSE C.ReceivePerson END AS ReceivePerson,
                                        CASE WHEN A.CaseKind = '外來文案件' THEN CONVERT(nvarchar(10), B.SendDate,111)  ELSE CONVERT(nvarchar(10), C.SendDate,111) END AS SendDate,
                                        CONVERT(nvarchar(10), D.HangingDate,111) as HangingDate,
                                        CASE WHEN A.CaseKind = '外來文案件' THEN B.Fee ELSE C.Fee END AS Fee,
                                        CONVERT(nvarchar(10), D.ChargeOffsDate,111) as ChargeOffsDate,
                                        D.ChargeOffsAmount,
                                        D.Memo
                                FROM 
                                [CaseMaster] AS A
                                LEFT OUTER JOIN
                                ( 
	                                        SELECT B1.CaseId,B1.Fee,B2.SendDate,B2.GovName AS ReceivePerson, 1 AS PayeeId, '' AS Memo
	                                        FROM
	                                        (
	                                                SELECT CaseId,SUM([Amount]) AS Fee FROM [CaseAccountExternal] GROUP BY CaseId)  AS B1
	                                                LEFT OUTER JOIN 
	                                                (
		                                                    SELECT CaseId,SendDate,GovName
		                                                    FROM
		                                                    (
		                                                            SELECT B1.CaseId,B1.SendDate,B2.GovName,Row_Number() over(Partition By B1.CaseId Order by B1.SendDate Asc) RowID
		                                                            FROM [CaseSendSetting] AS B1 INNER JOIN [CaseSendSettingDetails] B2 
		                                                            ON B1.CaseId = B2.CaseId AND B1.SerialID = B2.SerialID AND B2.SendType = 1
		                                                    ) AS B WHERE RowId = 1 
	                                                ) AS B2 on B1.CaseId = B2.CaseId
                                 ) AS B ON A.CaseId = B.CaseId AND A.CaseKind = '外來文案件'
                                        LEFT OUTER JOIN
                                        (
	                                        SELECT 
	                                        C1.CaseId, C1.Fee,C2.SendDate,C1.ReceivePerson,C1.PayeeId,C1.Memo
	                                        FROM [CasePayeeSetting] AS C1
	                                        LEFT OUTER JOIN [CaseSendSetting] AS C2 ON C1.[SendId] = C2.SerialID

                                        ) AS C ON A.CaseId = C.CaseId AND A.CaseKind = '扣押案件'

                                        LEFT OUTER JOIN [FeeChargeOffs] AS D ON A.CaseId = D.CaseId AND 
                                        CASE WHEN A.CaseKind = '外來文案件' THEN B.PayeeId ELSE C.PayeeId END = D.PayeeId
                                        WHERE (B.Fee > 0 OR C.Fee > 0) 
                                        AND 
                                        (
	                                        (A.CaseKind2 = '扣押並支付' AND A.ApproveDate2 IS NOT NULL)
	                                        OR
	                                        (A.CaseKind2 <> '扣押並支付' AND A.ApproveDate IS NOT NULL)
                                        ) " + sqlWhere;

            DataTable dt = base.Search(sqlStr);
            if (dt != null && dt.Rows.Count > 0) return dt;
            else return new DataTable();
        }

        public MemoryStream FeeChargeOffsExcel_NPOI(CaseMaster model)
        {
            IWorkbook workbook = new HSSFWorkbook();
            ISheet sheet = workbook.CreateSheet("手續費統計表");

            #region 獲取數據源
            DataTable dt = FeeChargeSearchList(model);
            #endregion

            #region def style
            ICellStyle styleHead12 = workbook.CreateCellStyle();
            IFont font12 = workbook.CreateFont();
            font12.FontHeightInPoints = 12;
            font12.FontName = "新細明體";
            styleHead12.FillPattern = FillPattern.SolidForeground;
            styleHead12.FillForegroundColor = HSSFColor.White.Index;
            styleHead12.BorderTop = BorderStyle.None;
            styleHead12.BorderLeft = BorderStyle.None;
            styleHead12.BorderRight = BorderStyle.None;
            styleHead12.BorderBottom = BorderStyle.None;
            styleHead12.WrapText = true;
            styleHead12.Alignment = HorizontalAlignment.Center;
            styleHead12.VerticalAlignment = VerticalAlignment.Center;
            styleHead12.SetFont(font12);

            ICellStyle styleHead10 = workbook.CreateCellStyle();
            IFont font10 = workbook.CreateFont();
            font10.FontHeightInPoints = 10;
            font10.FontName = "新細明體";
            styleHead10.FillPattern = FillPattern.SolidForeground;
            styleHead10.FillForegroundColor = HSSFColor.White.Index;
            styleHead10.BorderTop = BorderStyle.Thin;
            styleHead10.BorderLeft = BorderStyle.Thin;
            styleHead10.BorderRight = BorderStyle.Thin;
            styleHead10.BorderBottom = BorderStyle.Thin;
            styleHead10.WrapText = true;
            styleHead10.Alignment = HorizontalAlignment.Left;
            styleHead10.VerticalAlignment = VerticalAlignment.Center;
            styleHead10.SetFont(font10);

            ICellStyle styleHead11 = workbook.CreateCellStyle();
            IFont font11 = workbook.CreateFont();
            font10.FontHeightInPoints = 10;
            font10.FontName = "新細明體";
            styleHead11.FillPattern = FillPattern.SolidForeground;
            styleHead11.FillForegroundColor = HSSFColor.White.Index;
            styleHead11.BorderTop = BorderStyle.Thin;
            styleHead11.BorderLeft = BorderStyle.Thin;
            styleHead11.BorderRight = BorderStyle.Thin;
            styleHead11.BorderBottom = BorderStyle.Thin;
            styleHead11.WrapText = true;
            styleHead11.Alignment = HorizontalAlignment.Right;
            styleHead11.VerticalAlignment = VerticalAlignment.Center;
            styleHead11.SetFont(font10);
            #endregion

            #region title
            //*大標題 line0
            SetExcelCell(sheet, 0, 0, styleHead12, "手續費統計表");
            sheet.AddMergedRegion(new CellRangeAddress(0, 0, 0, 5));

            //*查詢條件 line1
            SetExcelCell(sheet, 1, 0, styleHead10, "案件編號");
            sheet.AddMergedRegion(new CellRangeAddress(1, 1, 0, 0));
            sheet.SetColumnWidth(0, 100 * 50);
            SetExcelCell(sheet, 1, 1, styleHead10, "來文機關");
            sheet.AddMergedRegion(new CellRangeAddress(1, 1, 1, 1));
            sheet.SetColumnWidth(1, 100 * 50);
            SetExcelCell(sheet, 1, 2, styleHead10, "付款人");
            sheet.AddMergedRegion(new CellRangeAddress(1, 1, 2, 2));
            sheet.SetColumnWidth(2, 100 * 50);
            SetExcelCell(sheet, 1, 3, styleHead10, "發文日");
            sheet.SetColumnWidth(3, 100 * 50);
            sheet.AddMergedRegion(new CellRangeAddress(1, 1, 3, 3));
            SetExcelCell(sheet, 1, 4, styleHead10, "掛帳日");
            sheet.AddMergedRegion(new CellRangeAddress(1, 1, 4, 4));
            sheet.SetColumnWidth(4, 100 * 50);
            SetExcelCell(sheet, 1, 5, styleHead10, "掛帳金額");
            sheet.AddMergedRegion(new CellRangeAddress(1, 1, 5, 5));
            SetExcelCell(sheet, 1, 6, styleHead10, "銷帳日");
            sheet.AddMergedRegion(new CellRangeAddress(1, 1, 6, 6));
            SetExcelCell(sheet, 1, 7, styleHead10, "銷帳金額");
            sheet.AddMergedRegion(new CellRangeAddress(1, 1, 7, 7));
            SetExcelCell(sheet, 1, 8, styleHead10, "餘額");
            sheet.AddMergedRegion(new CellRangeAddress(1, 1, 8, 8));
            SetExcelCell(sheet, 1, 9, styleHead10, "備註");
            sheet.AddMergedRegion(new CellRangeAddress(1, 1, 9, 9));
            #endregion

            #region body
            int balance = 0;
            string strFee = string.Empty;
            string strCharge = string.Empty;
            for (int iRow = 0; iRow < dt.Rows.Count; iRow++)
            {
                for (int iCol = 0; iCol < dt.Columns.Count - 1; iCol++)
                {
                    SetExcelCell(sheet, iRow + 2, iCol, styleHead10, Convert.ToString(dt.Rows[iRow][iCol]));
                }
                strFee = dt.Rows[iRow]["Fee"].ToString();
                strCharge = dt.Rows[iRow]["ChargeOffsAmount"].ToString();
                if (!string.IsNullOrEmpty(strFee))
                {
                    if (!string.IsNullOrEmpty(strCharge))
                    {
                        balance = Convert.ToInt32(strFee) - Convert.ToInt32(strCharge);
                    }
                    else
                    {
                        balance = Convert.ToInt32(strFee);
                    }

                }
                SetExcelCell(sheet, iRow + 2, 5, styleHead11, UtlString.FormatCurrency(strFee.ToString(), 0));
                SetExcelCell(sheet, iRow + 2, 7, styleHead11, UtlString.FormatCurrency(strCharge.ToString(), 0));
                SetExcelCell(sheet, iRow + 2, 8, styleHead11, UtlString.FormatCurrency(balance.ToString(), 0));
                SetExcelCell(sheet, iRow + 2, 9, styleHead10, Convert.ToString(dt.Rows[iRow]["Memo"]));
            }
            #endregion

            MemoryStream ms = new MemoryStream();
            workbook.Write(ms);
            ms.Flush();
            ms.Position = 0;
            workbook = null;
            return ms;
        }
        #endregion

        #region 列印地址條
        public DataTable AddrSearchList(string postType, string strMailNo1, string strMailNo2)
        {
            string strSql = "";
            string sqlWhere = "";
            if (!string.IsNullOrEmpty(strMailNo1))
            {
                sqlWhere += @" and MailNo >= @MailNo1 ";
                base.Parameter.Add(new CommandParameter("@MailNo1", strMailNo1));
            }
            if (!string.IsNullOrEmpty(strMailNo2))
            {
                sqlWhere += @" and MailNo <= @MailNo2 ";
                base.Parameter.Add(new CommandParameter("@MailNo2", strMailNo2));
            }
            if (postType == "0")
            {
                sqlWhere += " and css.Template='扣押'  ";
            }
            if (postType == "1")
            {
                sqlWhere += " and css.Template='支付'  ";
            }
            if (postType == "2")
            {
                sqlWhere += " and (css.Template='165調閱' or css.Template='非165調閱及財產申報'  or css.Template='財產申報')  ";
            }
            strSql = @" select  (d.GovName + CHAR(10) + CHAR(13) +d.GovAddr) as govs from MailInfo m 
                                    left join CaseSendSettingDetails d on m.SendDetailId=d.DetailsId and m.CaseId=d.CaseId
                                    LEFT OUTER JOIN CaseSendSetting as css on d.CaseId=css.CaseId and d.SerialID=css.SerialID where 1=1" + sqlWhere + "    group by MailNo,d.GovName,d.GovAddr";
            DataTable dt = base.Search(strSql);
            if (dt != null && dt.Rows.Count > 0) return dt;
            else return new DataTable();

        }
        #endregion
        /// <summary>
        /// 批次得到外來文法院扣押數據
        /// </summary>
        /// <returns></returns>
        public List<CaseMaster> CaseMasterSearchListForBatch()
        {
            try
            {
                string strsql = @"select ROW_NUMBER() OVER(ORDER BY c.CaseNo asc) as Sno, 
c.CaseNo,c.GovNo, o.ObligorNo,c.CaseKind,c.CaseKind2,
CONVERT(nvarchar(10), c.CreatedDate,112) as CreatedDate,c.CaseId
from CaseMaster c left join CaseObligor o on c.CaseId=o.CaseId 
where  CaseKind2 <>'撤銷' and GovNo like '%健%'
and Status<>'A02' and Status<>'C06' and Status<>'Z03'  
and c.CreatedDate >=CONVERT(varchar(10), DateAdd(d,-7,getdate()), 23) 
and c.CreatedDate<=CONVERT(varchar(10), DateAdd(d,-1,getdate()), 23)  ";
                IList<CaseMaster> _ilsit = base.SearchList<CaseMaster>(strsql);
                return _ilsit.ToList();
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }
    }
}
