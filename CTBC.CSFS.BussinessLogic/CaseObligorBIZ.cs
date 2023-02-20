using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CTBC.CSFS.Models;
using CTBC.CSFS.Pattern;
using System.Data;
using NPOI.OpenXmlFormats.Dml;

namespace CTBC.CSFS.BussinessLogic
{
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

    public class CaseObligorBIZ : CommonBIZ
    {
		private string pageFrom;
		public string PageFrom
		{
			set { pageFrom = value; }
			get { return pageFrom; }
		}

		private string userId;
		public string UserId
		{
			set { userId = value; }
			get { return userId; }
		}


		private Guid txsno;
		public Guid TXSNO
		{
			set { txsno = value; }
			get { return txsno; }
		}

		private DateTime txdatetime;
		public DateTime TXDateTime
		{
			set { txdatetime = value; }
			get { return txdatetime; }
		}

        public int Create(List<CaseObligor> model, IDbTransaction trans = null)
        {
  
            int rtn = 0;
            foreach (CaseObligor obligor in model.Where(obligor => !string.IsNullOrEmpty(obligor.ObligorName) || !string.IsNullOrEmpty(obligor.ObligorNo) ||
                                                                   !string.IsNullOrEmpty(obligor.ObligorAccount)))
            {
                        string strCaseKind = "";
                        string sSql = @"SELECT * from CaseMaster where CaseID=@CaseID";
                        // 清空參數容器
                        base.Parameter.Clear();
                        // 添加參數
                        base.Parameter.Add(new CommandParameter("@CaseID", obligor.CaseId));
                        IList<CaseMaster> list = trans == null ? SearchList<CaseMaster>(sSql) : SearchList<CaseMaster>(sSql, trans);
                        if (list.Count > 0)
                        {
                            foreach (var item in list)
                            {
                                strCaseKind = item.CaseKind.ToString();
                            }
                        }
                         
                var strSql = @" insert into CaseObligor  (CaseId,ObligorName,ObligorNo,ObligorAccount,CreatedUser,CreatedDate) 
                                        values (
                                        @CaseId,@ObligorName,@ObligorNo,@ObligorAccount,@CreatedUser,GETDATE());";
                Parameter.Clear();
                Parameter.Add(new CommandParameter("@CaseId", obligor.CaseId));
                Parameter.Add(new CommandParameter("@ObligorName", obligor.ObligorName));
                ////20160825 宏祥 update start
                //if (obligor.ObligorNo.Length > 0 && obligor.ObligorNo.Length < 8 && strCaseKind == "扣押案件")
                //{
                //    obligor.ObligorNo = "111111";
                //}
                ////20160825 宏祥 update end
                Parameter.Add(new CommandParameter("@ObligorNo", obligor.ObligorNo));
                Parameter.Add(new CommandParameter("@ObligorAccount", obligor.ObligorAccount));
                Parameter.Add(new CommandParameter("@CreatedUser", obligor.CreatedUser));
                rtn = rtn + (trans == null ? ExecuteNonQuery(strSql) : ExecuteNonQuery(strSql, trans));
            }
            return rtn;
        }
        public int CreateLog(List<CaseObligor> model, CaseMaster cm, IDbTransaction trans = null)
        {

            int rtn = 0;
            if (!model.Any())
                return 0;

            //*更新舊狀態為已重查,不然永遠都是失敗.0->3,1->4,2->5
            string strSql = "UPDATE [BatchQueue] SET [Status] = [Status] + 3 WHERE [CaseId] = @CaseId AND [Status] < 3";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("@CaseId", cm.CaseId));
            if (trans == null)
                ExecuteNonQuery(strSql);
            else
                ExecuteNonQuery(strSql, trans);

            foreach (CaseObligor obligor in model.Where(obligor => !string.IsNullOrEmpty(obligor.ObligorName) || !string.IsNullOrEmpty(obligor.ObligorNo) ||
                                                                   !string.IsNullOrEmpty(obligor.ObligorAccount)))
            {
                //* 0-待查詢,1-成功,2-失敗,3-已重查
                //20160122 RC --> 20150115 宏祥 update 新增67100電文 start
//                strSql = @" insert into BatchQueue(CaseId,SendUser,DocNo,ObligorNo,ServiceName,Status,CreateDatetime)
//                            values(@CaseId,@CreatedUser,@DocNo,@ObligorNo,'60491','0',GetDate());
//                            insert into BatchQueue(CaseId,SendUser,DocNo,ObligorNo,ServiceName,Status,CreateDatetime)
//                            values(@CaseId,@CreatedUser,@DocNo,@ObligorNo,'67072','0',GetDate());";
                strSql = @" insert into BatchQueue(CaseId,SendUser,DocNo,ObligorNo,ServiceName,Status,CreateDatetime)
                            values(@CaseId,@CreatedUser,@DocNo,@ObligorNo,'60491','0',GetDate());
                            insert into BatchQueue(CaseId,SendUser,DocNo,ObligorNo,ServiceName,Status,CreateDatetime)
                            values(@CaseId,@CreatedUser,@DocNo,@ObligorNo,'67072','0',GetDate());
                            insert into BatchQueue(CaseId,SendUser,DocNo,ObligorNo,ServiceName,Status,CreateDatetime)
                            values(@CaseId,@CreatedUser,@DocNo,@ObligorNo,'67100','0',GetDate());";
                //20160122 RC --> 20150115 宏祥 update 新增67100電文 end
                Parameter.Clear();
                Parameter.Add(new CommandParameter("@DocNo", cm.DocNo));
                Parameter.Add(new CommandParameter("@CaseId", obligor.CaseId));
                //20160825 宏祥 update  start
                if (obligor.ObligorNo.Length > 0 && obligor.ObligorNo.Length < 8)
                {
                    obligor.ObligorNo = "11111111";
                }
                //20160825 宏祥 update end
                Parameter.Add(new CommandParameter("@ObligorNo", obligor.ObligorNo));
                Parameter.Add(new CommandParameter("@CreatedUser", obligor.CreatedUser));
                rtn = rtn + (trans == null ? ExecuteNonQuery(strSql) : ExecuteNonQuery(strSql, trans));
            }
            return rtn;
        }


        public IList<CaseObligor> GetAllServiceObligorNo()
        {
            string strSql = @"SELECT TOP 1 [ID] AS ObligorId,
                                    [ObligorNo] 
                            FROM [BatchQueue]
                            WHERE [ServiceName] in ('60491','62072','67100')
                                AND [Status] = 0                                 
                                AND [ObligorNo] IS NOT NULL";


            Parameter.Clear();
            //Parameter.Add(new CommandParameter("ServiceName", serviceName));
            //Parameter.Add(new CommandParameter("DocNo", docNo));
            //DataTable dt = Search(strSql);
            IList<CaseObligor> list = SearchList<CaseObligor>(strSql);
            return list;
        }
		public string GetTXType(int ObligorId)
		{
			return "";
		}

		public int InsertCaseDataLog(CaseDataLog log, IDbTransaction trans = null)
		{
			int rtn = 0;
			string strSql = @" INSERT INTO dbo.CaseDataLog
						   (TXSNO
						   ,TXDateTime
						   ,TXUser
						   ,TXUserName
						   ,TXType
						   ,md_FuncID
						   ,TITLE
						   ,TabID
						   ,TabName
						   ,TableName
						   ,TableDispActive
						   ,ColumnID
						   ,ColumnName
						   ,ColumnValueBefore
						   ,ColumnValueAfter
						   ,CaseId
						   ,CaseNo
						   ,DispSrNo
						   ,LinkDataKey)
					 VALUES
						   (
						   @TXSNO,
						   @TXDateTime,
						   @TXUser,
						   @TXUserName,
						   @TXType,
						   @md_FuncID,
						   @TITLE,
						   @TabID,
						   @TabName,
						   @TableName,
						   @TableDispActive,
						   @ColumnID,
						   @ColumnName,
						   @ColumnValueBefore,
						   @ColumnValueAfter,
						   @CaseId,
						   @CaseNo,
						   @DispSrNo,
						   @LinkDataKey)";

				// 清空參數容器
				base.Parameter.Clear();
				// 添加參數
				//畫面每按一次儲存系統編號
				Parameter.Add(new CommandParameter("@TXSNO", log.TXSNO));
				//交易日期時間
				Parameter.Add(new CommandParameter("@TXDateTime", log.TXDateTime));
				//操作人員ID
				Parameter.Add(new CommandParameter("@TXUser", log.TXUser));
				//操作人員名稱
				Parameter.Add(new CommandParameter("@TXUserName", log.TXUserName));
				//事件：新增，修改，刪除
				Parameter.Add(new CommandParameter("@TXType", log.TXType));
				//功能ID
				Parameter.Add(new CommandParameter("@md_FuncID", log.md_FuncID));
				//功能Menu中文名稱
				Parameter.Add(new CommandParameter("@TITLE", log.TITLE));
				//頁籤ID
				Parameter.Add(new CommandParameter("@TabID", log.TabID));
				//頁籤名稱
				Parameter.Add(new CommandParameter("@TabName", log.TabName));
				//tablename
				Parameter.Add(new CommandParameter("@TableName", log.TableName));
				//"1"-畫面要顯示
				Parameter.Add(new CommandParameter("@TableDispActive", log.TableDispActive));
				//ColumnID
				Parameter.Add(new CommandParameter("@ColumnID", log.ColumnID));
				//畫面顯示的欄位名稱
				Parameter.Add(new CommandParameter("@ColumnName", log.ColumnName));
				//修改前(Varchar型態的寫入)
				Parameter.Add(new CommandParameter("@ColumnValueBefore", log.ColumnValueBefore));
				//修改後(Varchar型態的寫入)
				Parameter.Add(new CommandParameter("@ColumnValueAfter", log.ColumnValueAfter));
				//案件ID
				Parameter.Add(new CommandParameter("@CaseId", log.CaseId));
				//案件編號
				Parameter.Add(new CommandParameter("@CaseNo", log.CaseNo));
				//畫面顯示與否'1'畫面要顯示
				Parameter.Add(new CommandParameter("@DispSrNo", log.DispSrNo));
				Parameter.Add(new CommandParameter("@LinkDataKey", log.LinkDataKey));
				rtn = rtn + (trans == null ? ExecuteNonQuery(strSql) : ExecuteNonQuery(strSql, trans));
				
				return rtn;
		}

		public int InsertLog(CaseDataLog log, IDbTransaction trans = null)
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
			return InsertCaseDataLog(log);
		}

		public int Edit(List<CaseObligor> oldList, List<CaseObligor> newList, IDbTransaction trans = null)
        {
            int rtn = 0;
			for (int i = 0; i < newList.Count; i++)
			{
				string strCaseKind = "";
				string sSql = @"SELECT * from CaseMaster where CaseID=@CaseID";
				// 清空參數容器
				base.Parameter.Clear();
				// 添加參數
				base.Parameter.Add(new CommandParameter("@CaseID", newList[i].CaseId));
				IList<CaseMaster> list = trans == null ? SearchList<CaseMaster>(sSql) : SearchList<CaseMaster>(sSql, trans);
				if (list.Count > 0)
				{
					foreach (var item in list)
					{
						strCaseKind = item.CaseKind.ToString();
					}
				}
				if (i <= oldList.Count -1)
				{
					if (newList[i].ObligorId != 0 && !PopupComparer.Default.Equals(oldList[i], newList[i]))
					{
						newList[i].isUpdate = true;
						int updateRtn = 0;
						string strUpdateSql = string.Empty;
						strUpdateSql = @" update CaseObligor set ObligorName=@ObligorName, ObligorNo=@ObligorNo,ObligorAccount=@ObligorAccount where CaseId=@CaseId and ObligorId=@ObligorId;";
						Parameter.Clear();
						Parameter.Add(new CommandParameter("@CaseId", newList[i].CaseId));
						Parameter.Add(new CommandParameter("@ObligorId", newList[i].ObligorId));
						Parameter.Add(new CommandParameter("@ObligorName", newList[i].ObligorName));
						Parameter.Add(new CommandParameter("@ObligorNo", newList[i].ObligorNo));
						Parameter.Add(new CommandParameter("@ObligorAccount", newList[i].ObligorAccount));
						updateRtn = trans == null ? ExecuteNonQuery(strUpdateSql) : ExecuteNonQuery(strUpdateSql, trans);

						#region 記錄修改Log
						CaseDataLog log = new CaseDataLog();
						log.TXType = "修改";
						CaseMasterBIZ masterBiz = new CaseMasterBIZ();
						CaseMaster master = masterBiz.MasterModelNew(newList[i].CaseId);
						if (oldList[i].ObligorName != newList[i].ObligorName)
						{
							log.TXSNO = TXSNO;
							log.TXDateTime = TXDateTime;
							log.ColumnID = "ObligorName";
							log.ColumnName = "義(債)務人戶名";
							log.ColumnValueBefore = oldList[i].ObligorName;
							log.ColumnValueAfter = newList[i].ObligorName;
							log.TabID = "Tab1-1";
							log.TabName = "公文資訊-義(債)務人資訊";
							log.TableName = "CaseObligor";
							log.DispSrNo = 1;
							log.TableDispActive = "1";
							log.CaseId = newList[i].CaseId.ToString();
							log.CaseNo = master.CaseNo.ToString();
							//List<CaseObligor> list = caseobligor.ObligorModel(model.CaseMaster.CaseId);
							log.LinkDataKey = newList[i].ObligorId.ToString();
							InsertLog(log);
						}
						if (oldList[i].ObligorNo != (string.IsNullOrEmpty(newList[i].ObligorNo) ? "" : newList[i].ObligorNo))
						{
							log.TXSNO = TXSNO;
							log.TXDateTime = TXDateTime;
							log.ColumnID = "ObligorNo";
							log.ColumnName = "義(債)務人統編";
							log.ColumnValueBefore = oldList[i].ObligorNo;
							log.ColumnValueAfter = newList[i].ObligorNo;
							log.TabID = "Tab1-1";
							log.TabName = "公文資訊-義(債)務人資訊";
							log.TableName = "CaseObligor";
							log.DispSrNo = 2;
							log.TableDispActive = "1";
							log.CaseId = newList[i].CaseId.ToString();
							log.CaseNo = master.CaseNo.ToString();
							//List<CaseObligor> list = caseobligor.ObligorModel(model.CaseMaster.CaseId);
							log.LinkDataKey = newList[i].ObligorId.ToString();
							InsertLog(log);
						}
						if (oldList[i].ObligorAccount != (string.IsNullOrEmpty(newList[i].ObligorAccount) ? "" : newList[i].ObligorAccount))
						{
							log.TXSNO = TXSNO;
							log.TXDateTime = TXDateTime;
							log.ColumnID = "ObligorAccount";
							log.ColumnName = "義務人帳號";
							log.ColumnValueBefore = oldList[i].ObligorAccount;
							log.ColumnValueAfter = newList[i].ObligorAccount;
							log.TabID = "Tab1-1";
							log.TabName = "公文資訊-義(債)務人資訊";
							log.TableName = "CaseObligor";
							log.DispSrNo = 3;
							log.TableDispActive = "1";
							log.CaseId = newList[i].CaseId.ToString();
							log.CaseNo = master.CaseNo.ToString();
							//List<CaseObligor> list = caseobligor.ObligorModel(model.CaseMaster.CaseId);
							log.LinkDataKey = newList[i].ObligorId.ToString();
							InsertLog(log);
						}
						#endregion 
					}
					if (newList[i].ObligorId == 0 && (string.IsNullOrEmpty(newList[i].ObligorNo) == true ? "" : newList[i].ObligorNo) == "" && (string.IsNullOrEmpty(newList[i].ObligorName) == true ? "" : newList[i].ObligorName) == "" && (string.IsNullOrEmpty(newList[i].ObligorAccount) == true ? "" : newList[i].ObligorAccount) == "")
					{
						newList[i].isDelete = true;
						int deleteRtn = 0;
						string strDeleteSql = string.Empty;
						strDeleteSql = @" delete from CaseObligor where CaseId=@CaseId and ObligorId=@ObligorId;";
						Parameter.Clear();
						Parameter.Add(new CommandParameter("@CaseId", newList[i].CaseId));
						Parameter.Add(new CommandParameter("@ObligorId", oldList[i].ObligorId));
						deleteRtn = trans == null ? ExecuteNonQuery(strDeleteSql) : ExecuteNonQuery(strDeleteSql, trans);

						#region 記錄刪除Log
						CaseDataLog log = new CaseDataLog();
						log.TXType = "刪除";
						CaseMasterBIZ masterBiz = new CaseMasterBIZ();
						CaseMaster master = masterBiz.MasterModelNew(newList[i].CaseId);
						if (oldList[i].ObligorName != newList[i].ObligorName)
						{
							log.TXSNO = TXSNO;
							log.TXDateTime = TXDateTime;
							log.ColumnID = "ObligorName";
							log.ColumnName = "義(債)務人戶名";
							log.ColumnValueBefore = oldList[i].ObligorName;
							log.ColumnValueAfter = "";
							log.TabID = "Tab1-1";
							log.TabName = "公文資訊-義(債)務人資訊";
							log.TableName = "CaseObligor";
							log.DispSrNo = 1;
							log.TableDispActive = "1";
							log.CaseId = newList[i].CaseId.ToString();
							log.CaseNo = master.CaseNo.ToString();
							//List<CaseObligor> list = caseobligor.ObligorModel(model.CaseMaster.CaseId);
							log.LinkDataKey = oldList[i].ObligorId.ToString();
							InsertLog(log);
						}
						if (oldList[i].ObligorNo != newList[i].ObligorNo)
						{
							log.TXSNO = TXSNO;
							log.TXDateTime = TXDateTime;
							log.ColumnID = "ObligorNo";
							log.ColumnName = "義(債)務人統編";
							log.ColumnValueBefore = oldList[i].ObligorNo;
							log.ColumnValueAfter = "";
							log.TabID = "Tab1-1";
							log.TabName = "公文資訊-義(債)務人資訊";
							log.TableName = "CaseObligor";
							log.DispSrNo = 2;
							log.TableDispActive = "1";
							log.CaseId = newList[i].CaseId.ToString();
							log.CaseNo = master.CaseNo.ToString();
							//List<CaseObligor> list = caseobligor.ObligorModel(model.CaseMaster.CaseId);
							log.LinkDataKey = oldList[i].ObligorId.ToString();
							InsertLog(log);
						}
						if (oldList[i].ObligorAccount != newList[i].ObligorAccount)
						{
							log.TXSNO = TXSNO;
							log.TXDateTime = TXDateTime;
							log.ColumnID = "ObligorAccount";
							log.ColumnName = "義務人帳號";
							log.ColumnValueBefore = oldList[i].ObligorAccount;
							log.ColumnValueAfter = "";
							log.TabID = "Tab1-1";
							log.TabName = "公文資訊-義(債)務人資訊";
							log.TableName = "CaseObligor";
							log.DispSrNo = 3;
							log.TableDispActive = "1";
							log.CaseId = newList[i].CaseId.ToString();
							log.CaseNo = master.CaseNo.ToString();
							//List<CaseObligor> list = caseobligor.ObligorModel(model.CaseMaster.CaseId);
							log.LinkDataKey = oldList[i].ObligorId.ToString();
							InsertLog(log);
						}
						#endregion 
					}
				}
				if (newList[i].ObligorId == 0 && newList[i].ObligorNo == null && newList[i].ObligorName == null && newList[i].ObligorAccount == null)
				{
					continue;
				}
				if (newList[i].ObligorId == 0 && (newList[i].ObligorNo != "" || newList[i].ObligorName != "" || newList[i].ObligorAccount != ""))
				{
					CaseMasterBIZ masterBiz = new CaseMasterBIZ();
					CaseMaster master = masterBiz.MasterModelNew(newList[i].CaseId);
					newList[i].isAdd = true;
					int insertRtn = 0;
					string strInsertSql = string.Empty;
					strInsertSql = @" insert into CaseObligor  (CaseId,ObligorName,ObligorNo,ObligorAccount,CreatedUser,CreatedDate) 
                                        values (
                                        @CaseId,@ObligorName,@ObligorNo,@ObligorAccount,@CreatedUser,GETDATE());select @@IDENTITY;";

					Parameter.Clear();
					Parameter.Add(new CommandParameter("@CaseId", newList[i].CaseId));
					Parameter.Add(new CommandParameter("@ObligorName", newList[i].ObligorName));
					if (newList[i].ObligorNo.Length > 0 && newList[i].ObligorNo.Length < 8 && strCaseKind == "扣押案件")
					{
						newList[i].ObligorNo = "111111";
					}
					Parameter.Add(new CommandParameter("@ObligorNo", newList[i].ObligorNo));
					Parameter.Add(new CommandParameter("@ObligorAccount", newList[i].ObligorAccount));
					Parameter.Add(new CommandParameter("@CreatedUser", newList[i].CreatedUser));
					insertRtn = Convert.ToInt32(ExecuteScalar(strInsertSql));

					#region 記錄新增Log
					CaseDataLog log = new CaseDataLog();
					CaseObligorBIZ caseobligor = new CaseObligorBIZ();
					log.TXType = "新增";
					#region 義(債)務人戶名
					log.TXSNO = TXSNO;
					log.TXDateTime = TXDateTime;
					log.ColumnID = "ObligorName";
					log.ColumnName = "義(債)務人戶名";
					log.ColumnValueBefore = "";
					log.ColumnValueAfter = newList[i].ObligorName;
					log.TabID = "Tab1-1";
					log.TabName = "公文資訊-義(債)務人資訊";
					log.TableName = "CaseObligor";
					log.DispSrNo = 1;
					log.TableDispActive = "1";
					log.CaseId = newList[i].CaseId.ToString();
					log.CaseNo = master.CaseNo.ToString();
					List<CaseObligor> list1 = caseobligor.ObligorModel(newList[i].CaseId);
					log.LinkDataKey = insertRtn.ToString();
					InsertLog(log);
					#endregion

					#region 義(債)務人統編
					log.TXSNO = TXSNO;
					log.TXDateTime = TXDateTime;
					log.ColumnID = "ObligorNo";
					log.ColumnName = "義(債)務人統編";
					log.ColumnValueBefore = "";
					log.ColumnValueAfter = newList[i].ObligorNo;
					log.TabID = "Tab1-1";
					log.TabName = "公文資訊-義(債)務人資訊";
					log.TableName = "CaseObligor";
					log.DispSrNo = 2;
					log.TableDispActive = "1";
					log.CaseId = newList[i].CaseId.ToString();
					log.CaseNo = master.CaseNo.ToString();
					log.LinkDataKey = insertRtn.ToString();
					InsertLog(log);
					#endregion

					#region 帳號
					log.TXSNO = TXSNO;
					log.TXDateTime = TXDateTime;
					log.ColumnID = "ObligorAccount";
					log.ColumnName = "義務人帳號";
					log.ColumnValueBefore = "";
					log.ColumnValueAfter = newList[i].ObligorAccount;
					log.TabID = "Tab1-1";
					log.TabName = "公文資訊-義(債)務人資訊";
					log.TableName = "CaseObligor";
					log.DispSrNo = 3;
					log.TableDispActive = "1";
					log.CaseId = newList[i].CaseId.ToString();
					log.CaseNo = master.CaseNo.ToString();
					log.LinkDataKey = insertRtn.ToString();
					InsertLog(log);
					#endregion
					#endregion
				}
			}

            return rtn;
        }


        public List<CaseObligor> ObligorModel(Guid caseId, IDbTransaction trans = null)
        {
			string strSql = @";with  T1 as(select *, ObligorNo + '-' + ObligorName as ObligorNoAndName 
									from CaseObligor where CaseId=@CaseId) ,
									T2 as (select ColumnValueBefore,ColumnValueAfter,ColumnID,LinkDataKey from CaseDataLog c1,CaseObligor where c1.CaseId=@CaseId and TabName='公文資訊-義(債)務人資訊'
                                    and TableName='CaseObligor' and LinkDataKey=cast(ObligorId as nvarchar(100) )
									and TXDateTime=(select max(TXDateTime) from CaseDataLog c2 where c1.CaseId=c2.CaseId and c1.TabName=c2.TabName and c1.TableName=c2.TableName and c1.LinkDataKey=c2.LinkDataKey)	),
									T3 as (select T1.*,
								   (select case  when ColumnValueBefore<>ObligorName then 'true' else 'false' end from T2 where ColumnID='ObligorName' and LinkDataKey=cast(ObligorId as nvarchar(100))  ) as Nameflag,
								   (select case  when ColumnValueBefore<>ObligorNo then 'true' else 'false' end from T2 where ColumnID='ObligorNo' and LinkDataKey=cast(ObligorId as nvarchar(100))  ) as Noflag,
								   (select case  when ColumnValueBefore<>ObligorAccount then 'true' else 'false' end from T2 where ColumnID='ObligorAccount' and LinkDataKey=cast(ObligorId as nvarchar(100))  ) as Accountflag
								   from T1)
								   select * from T3";
			//string strSql = "select *, ObligorNo + '-' + ObligorName as ObligorNoAndName from CaseObligor where CaseId=@CaseId order by ObligorId desc";
            base.Parameter.Clear();
            base.Parameter.Add(new CommandParameter("@CaseId", caseId));
            try
            {
                List<CaseObligor> itemlist = new List<CaseObligor>();
                IList<CaseObligor> list = trans == null ? SearchList<CaseObligor>(strSql) : SearchList<CaseObligor>(strSql, trans);
                foreach (var item in list)
                {
                    itemlist.Add(item);
                }
                return itemlist;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public CaseObligor ObligorModelInfo(Guid CaseId)
        {
            string strSql = "select top 1 ObligorNo,ObligorName from [dbo].[CaseObligor] where Caseid=@CaseId";
            base.Parameter.Clear();
            base.Parameter.Add(new CommandParameter("@CaseId", CaseId));
            try
            {
                IList<CaseObligor> list = base.SearchList<CaseObligor>(strSql);
                if (list != null) return list[0];
                else return new CaseObligor();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public bool EditAllServiceCaseObligor(string status, string tailNum)
        {
            string sql1 = @"UPDATE [BatchQueue] 
                            SET [SendDate] = GETDATE(), 
                                [Status] = @Status
                            WHERE [Status] = '0' AND [DocNo]  LIKE '%{0}' ";
            Parameter.Clear();
            string sql = string.Format(sql1, tailNum);
            //Parameter.Add(new CommandParameter("Id", id));
            Parameter.Add(new CommandParameter("Status", status));
            //Parameter.Add(new CommandParameter("ErrorMsg", errorMsg));
            return ExecuteNonQuery(sql) > 0;
        }


        public IList<CaseObligor> GetObligorNo99(string serviceName, string tailNum)
        {
            string strSql1 = @"SELECT  [ID] AS ObligorId,
                                    [ObligorNo] 
                            FROM [BatchQueue]
                            WHERE [ServiceName] = @ServiceName 
                                AND [Status] = 99 
                                AND [DocNo] LIKE '%{0}'
                                AND [ObligorNo] IS NOT NULL";


            Parameter.Clear();
            string strSql = string.Format(strSql1, tailNum);
            Parameter.Add(new CommandParameter("ServiceName", serviceName));
            //Parameter.Add(new CommandParameter("DocNo", docNo));
            //DataTable dt = Search(strSql);
            IList<CaseObligor> list = SearchList<CaseObligor>(strSql);
            return list;
        }

        public IList<CaseObligor> GetObligor(string ObligorNo)
        {
            try
            {
                string strSql = @"select * from BatchQueue where [ServiceName] = '60491' and [Status] = 0 and ObligorNo=@ObligorNo and ObligorNo is not null";
                // 清空參數容器
                base.Parameter.Clear();
                // 添加參數
                base.Parameter.Add(new CommandParameter("@ObligorNo", ObligorNo));
                IList<CaseObligor> list = base.SearchList<CaseObligor>(strSql);
                return list;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IList<CaseObligor> GetObligorsList(Guid CaseId)
        {
            try
            {
                string strSql = @"select * from CaseObligor where CaseID = @CaseID";
                // 清空參數容器
                base.Parameter.Clear();
                // 添加參數
                base.Parameter.Add(new CommandParameter("@CaseID", CaseId));
                IList<CaseObligor> list = base.SearchList<CaseObligor>(strSql);
                return list;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IList<CaseObligor> GetObligorNo(string serviceName)
        {
            string strSql = @"SELECT [ID] AS ObligorId,
                                    [ObligorNo] 
                            FROM [BatchQueue]
                            WHERE [ServiceName] = @ServiceName 
                                AND [Status] = 0 
                                AND [ObligorNo] IS NOT NULL";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("ServiceName", serviceName));
            IList<CaseObligor> list = SearchList<CaseObligor>(strSql);
            return list;
        }

        public bool EditCaseObligor(string id, string status, string errorMsg)
        {
            string sql = @"UPDATE [BatchQueue] 
                            SET [SendDate] = GETDATE(), 
                                [Status] = @Status,
                                [ErrorMsg] = @ErrorMsg 
                            WHERE Id = @Id";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("Id", id));
            Parameter.Add(new CommandParameter("Status", status));
            Parameter.Add(new CommandParameter("ErrorMsg", errorMsg));
            return ExecuteNonQuery(sql) > 0;
        }

        public bool UpdateCaseid(string id, string status, string errorMsg)
        {
            string sql = @"UPDATE [BatchQueue] 
                            SET [SendDate] = GETDATE(), 
                                [Status] = @Status,
                                [ErrorMsg] = @ErrorMsg 
                            WHERE Id = @Id";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("Id", id));
            Parameter.Add(new CommandParameter("Status", status));
            Parameter.Add(new CommandParameter("ErrorMsg", errorMsg));
            return ExecuteNonQuery(sql) > 0;
        }
        public string GetBatchQueueID(string id)
        {
            try
            {
                string sqlStr = @"SELECT [caseid] 
                                FROM [BatchQueue]
                                WHERE Id = @Id";

                base.Parameter.Clear();
                Parameter.Add(new CommandParameter("Id", id));
                string result = Convert.ToString(base.ExecuteScalar(sqlStr));
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string GetBatchQueueByID(string ObligorNo)
        {
            try
            {
                string sqlStr = @"SELECT [ID] 
                                FROM [BatchQueue]
                                WHERE [ServiceName] = '60491' 
                                    AND [Status] = 0 
                                    AND [ObligorNo] = @ObligorNo 
                                    AND [ObligorNo] is not null";

                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@ObligorNo", ObligorNo));

                string result = Convert.ToString(base.ExecuteScalar(sqlStr));
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
