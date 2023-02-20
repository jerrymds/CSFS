using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CTBC.CSFS.Pattern;
using CTBC.CSFS.Models;
using System.Data;


namespace CTBC.CSFS.BussinessLogic
{
    public class AgentSettingBIZ : CommonBIZ
    {
        public AgentSettingBIZ(AppController AppController)
            : base(AppController)
        { }
        public AgentSettingBIZ()
        { }

        public IList<AgentSetting> GetKeBie()
        {
            try
            {
               //20170421 固定 RQ-2015-019666-017 派件至跨單位 宏祥 update start
                //string strSql = @"select distinct SectionName as Department from V_AgentAndDept";
               string strSql = @"select distinct (CASE WHEN BranchName = UpDepName THEN UpDepName + ' - ' + SectionName
					                                       WHEN BranchName = DepName THEN DepName
					                                       ELSE SectionName END) AS Department,
					                                       p.CodeTag,v.BranchID 
                                 FROM V_AgentAndDept AS v INNER JOIN
                                 PARMCode AS p ON v.DepID = p.CodeNo or v.UpperDepID = p.CodeNo or v.BranchID = p.CodeNo
                                 ORDER BY p.CodeTag desc,v.BranchID";
               //20170421 固定 RQ-2015-019666-017 派件至跨單位 宏祥 update end
                return base.SearchList<AgentSetting>(strSql);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        /// <summary>
        /// 取得已設定經辦人員的單位下拉選單(收發作業-待辦理-派件)(主管作業-主管改派)
        /// 20170421 固定 RQ-2015-019666-017 派件至跨單位 宏祥 add
        /// </summary>
        /// <returns></returns>
        public IList<AgentSetting> GetAgentSettingDepartment()
        {
           try
           {
              string strSql = @"SELECT SectionName AS Department,p.CodeTag
                                FROM AgentSetting AS A LEFT OUTER JOIN
                                PARMCode AS P ON P.CodeDesc = A.SectionName AND P.CodeType='AgentSetting'
                                GROUP BY SectionName,p.CodeTag
                                ORDER BY p.CodeTag desc";
              return base.SearchList<AgentSetting>(strSql);
           }
           catch (Exception ex)
           {
              throw ex;
           }
        }

        /// <summary>
        /// 取得員編所屬下拉選單部門名稱
        /// 20170421 固定 RQ-2015-019666-017 派件至跨單位 宏祥 add
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public string GetSectionName(string userId)
        {
           try
           {
              string strSql = @"SELECT SectionName FROM V_AgentAndDept where EmpID=@EmpID";
              base.Parameter.Clear();
              base.Parameter.Add(new CommandParameter("@EmpID", userId));
              return base.ExecuteScalar(strSql) == null ? "" : base.ExecuteScalar(strSql).ToString();
           }
           catch (Exception ex)
           {
              throw ex;
           }
        }

        /// <summary>
        /// 改派部門下拉選單顯示判斷
        /// </summary>
        /// <param name="Department"></param>
        /// <returns></returns>
        public int GetAgentToHandleDepartmentDDL(string Department)
        {
           try
           {
              string strSql = @"SELECT count(0) FROM PARMCode WHERE CodeType='AgentToHandleDepartmentDDL' and CodeDesc=@CodeDesc";
              base.Parameter.Clear();
              base.Parameter.Add(new CommandParameter("@CodeDesc", Department));
              return (int)base.ExecuteScalar(strSql);
           }
           catch (Exception ex)
           {
              throw ex;
           }
        }

        /// <summary>
        /// 取得已設定經辦人員的單位下拉選單(經辦作業-待辦理-改派)
        /// 20170421 固定 RQ-2015-019666-017 派件至跨單位 宏祥 add
        /// </summary>
        /// <returns></returns>
        public IList<AgentSetting> GetAgentSettingDepartment(string Department, string BranchName, int Flag)
        {
           try
           {
              string SectionName = "";
              string strSql = @"SELECT SectionName AS Department,p.CodeTag
                                FROM AgentSetting AS A LEFT OUTER JOIN
                                PARMCode AS P ON P.CodeDesc = A.SectionName AND P.CodeType='AgentSetting'
                                WHERE 1=1";

              if (BranchName == "")
	           {
                 if (Flag == 0)
                 {
                    SectionName = Department + "%";
                    strSql = strSql + " and SectionName like @SectionName ";                    
                 }
	           }
              else
	           {
                 SectionName = BranchName + "%";
                 strSql = strSql + " and SectionName like @SectionName ";
	           }

              strSql = strSql + " GROUP BY SectionName,p.CodeTag ORDER BY p.CodeTag desc ";

              base.Parameter.Clear();
              base.Parameter.Add(new CommandParameter("@SectionName", SectionName));     
              return base.SearchList<AgentSetting>(strSql);
           }
           catch (Exception ex)
           {
              throw ex;
           }
        }

        public string GetAgentSetting(string Department = null)
        {
            try
            {
                string strSql = @"select EmpId as AsempId,SectionName as AsSname from AgentSetting where 1=1";
                if(!string.IsNullOrEmpty(Department))
                {
                    strSql += " and SectionName = @SectionName";
                    base.Parameter.Clear();
                    base.Parameter.Add(new CommandParameter("@SectionName", Department));
                }
                IList<AgentSetting> list = base.SearchList<AgentSetting>(strSql);
                if (list.Count > 0)
                {
                    string Empid = string.Empty;
                    foreach (AgentSetting item in list)
                    {
                        Empid += item.AsempId + ",";
                    }
                    Empid = Empid.Trim(',');
                    return Empid;
                }
                else
                {
                    return "";
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public string GetAgentSettingAll(string Department)
        {
            try
            {
                string strSql = @"select EmpId as AsempId,SectionName as AsSname, IsSeizure, IsCase from AgentSetting ";//where SectionName=@SectionName
                //base.Parameter.Clear();
                //base.Parameter.Add(new CommandParameter("@SectionName", Department));
                IList<AgentSetting> list = base.SearchList<AgentSetting>(strSql);
                if (list.Count > 0)
                {
                    string Empid = string.Empty;
                    string IsSeizure = string.Empty;
                    string IsCase = string.Empty;
                    string IsAutoDispatch = string.Empty;
                    string IsAutoDispatchFS = string.Empty;
                    foreach (AgentSetting item in list)
                    {
                        Empid += item.AsempId + ",";
                        if (item.IsSeizure)
                        { IsSeizure += item.AsempId + ","; }
                        if (item.IsCase)
                        { IsCase += item.AsempId + ","; }
                    }
                    Empid = Empid.Trim(',');
                    IsSeizure = IsSeizure.Trim(',');
                    IsCase = IsCase.Trim(',');
                    string result = Empid + "|" + IsSeizure + "|" + IsCase;
                    string sql = @"select CodeType,Enable from PARMCode where CodeType like 'AutoDispatch%'";
                    IList<PARMCode> PMlist = base.SearchList<PARMCode>(sql);
                    foreach(PARMCode item in PMlist)
                    {
                        if(item.CodeType == "AutoDispatch" && item.Enable == true)
                        {
                            IsAutoDispatch = "true";
                        }
                        else if (item.CodeType == "AutoDispatch" && item.Enable == false)
                        {
                            IsAutoDispatch = "false";
                        }
                        if (item.CodeType == "AutoDispatchFS" && item.Enable == true)
                        {
                            IsAutoDispatchFS = "true";
                        }
                        else if (item.CodeType == "AutoDispatchFS" && item.Enable == false)
                        {
                            IsAutoDispatchFS = "false";
                        }
                    }
                    result = result + "|" + IsAutoDispatch + "|" + IsAutoDispatchFS;
                    return result;
                }
                else
                {
                    return "||||";
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public string Dosave(string Empidarr, string Department, string IsAutoDispatch, string IsAutoDispatchFS)
        {
            int rtn = 0;
            IDbConnection dbConnection = OpenConnection();
            IDbTransaction dbTransaction = null;
            CaseHistoryBIZ history = new CaseHistoryBIZ();
            //string[] Empid = Empidarr.Split(',');
            string[] Emp = Empidarr.Split('|');
            string[] Empid = Emp[0].Split(',');//授權人員
            string[] EmpSeizure = Emp[1].Split(',');//扣押經辦
            string[] EmpCase = Emp[2].Split(',');//外來文經辦

            //20170421 固定 RQ-2015-019666-017 派件至跨單位 宏祥 add start
            string strEmpid = Empid.GetValue(0).ToString().Trim();
            string strSQL = @"select count(0) from LDAPEmployee where EmpID = @Empid and ISNULL(BranchID,'') <> ''";
            base.Parameter.Clear();
            base.Parameter.Add(new CommandParameter("@EmpId", strEmpid));
            int intIsBranch = (int)base.ExecuteScalar(strSQL);

            //string[] aryDepartment = Department.Split('-');
            //string strDepartment = aryDepartment.GetValue(0).ToString().Trim();
            //string strIsBranch = strDepartment.Substring(strDepartment.Length - 2, 2);
            string strDepartmentID = "";

            if (intIsBranch >= 1) { strDepartmentID = "M00023321"; } else { strDepartmentID = "M00022677"; }          
            //20170421 固定 RQ-2015-019666-017 派件至跨單位 宏祥 add end
            try
            {
                using (dbConnection)
                {
                    dbTransaction = dbConnection.BeginTransaction();
                    //string Dsql = @"delete  from AgentSetting where SectionName=@SectionName";
                    //base.Parameter.Clear();
                    //base.Parameter.Add(new CommandParameter("@SectionName", Department));
                    //base.ExecuteNonQuery(Dsql);
                    string[] SectionEmpid1 = GetAgentSetting().Split(',');//查找AgentSetting中所有人員
                    List<string> InsertEmpId = new List<string>();//要insert的資料
                    List<string> DeleteEmpId = new List<string>();//要刪除的資料
                    foreach (string item in SectionEmpid1)
                    {
                        if (!Empid.Contains(item))
                        {
                            DeleteEmpId.Add(item);
                        }
                    }
                    foreach (string id in DeleteEmpId)
                    {
                        string Dsql = @"delete  from AgentSetting where EmpId=@EmpId";
                        base.Parameter.Clear();
                        base.Parameter.Add(new CommandParameter("@EmpId", id));
                        base.ExecuteNonQuery(Dsql);
                    }
                    foreach (string item in Empid)
                    {
                        if (!SectionEmpid1.Contains(item))
                        {
                            InsertEmpId.Add(item);
                        }
                    }
                    foreach (string id in InsertEmpId)
                    {
                        string isSeizure = "0";
                        string isCase = "0";
                        if (EmpSeizure.Contains(id))
                        {
                            isSeizure = "1";
                        }
                        if (EmpCase.Contains(id))
                        {
                            isCase = "1";
                        }
                        
                        //20170421 固定 RQ-2015-019666-017 派件至跨單位 宏祥 update start
                        //string sqlStr = @"insert into AgentSetting values(@EmpId,@SectionName)";
                        //string sqlStr = @"insert into AgentSetting values(@EmpId,@SectionName,@DepartmentID)";
                        string sqlStr = @"insert into AgentSetting values(@EmpId,@SectionName,@DepartmentID,@IsSeizure,@IsCase)";
                        base.Parameter.Clear();
                        base.Parameter.Add(new CommandParameter("@EmpId", id));
                        base.Parameter.Add(new CommandParameter("@SectionName", Department));
                        base.Parameter.Add(new CommandParameter("@DepartmentID", strDepartmentID));
                        //20170421 固定 RQ-2015-019666-017 派件至跨單位 宏祥 update start
                        base.Parameter.Add(new CommandParameter("@IsSeizure", isSeizure));//0否,1是
                        base.Parameter.Add(new CommandParameter("@IsCase", isCase));//0否,1是
                        rtn = base.ExecuteNonQuery(sqlStr, dbTransaction);
                    }

                    dbTransaction.Commit();

                    //是否啟用自動派送，更新PARMCode資料
                    string AutoDispatch = string.Empty;
                    string AutoDispatchFS = string.Empty;
                    AgentSetting AScode = GetEnable();
                    AutoDispatch = AScode.IsAutoDispatch ? "true" : "false";
                    AutoDispatchFS = AScode.IsAutoDispatchFS ? "true" : "false";
                    if(AutoDispatch != IsAutoDispatch)
                    {
                        string Usql = @"UPDATE ParmCode SET 
                                    Enable = @Enable,
                                    ModifiedUser = @ModifiedUser,
                                    ModifiedDate = GETDATE() 
                                WHERE CodeType = @CodeType";// CodeUid = @CodeUid and 
                        base.Parameter.Clear();
                        //base.Parameter.Add(new CommandParameter("@CodeUid", "8052"));
                        base.Parameter.Add(new CommandParameter("@CodeType", "AutoDispatch"));
                        base.Parameter.Add(new CommandParameter("@Enable", IsAutoDispatch == "true" ? "1" : "0"));
                        base.Parameter.Add(new CommandParameter("@ModifiedUser", Account));
                        rtn = base.ExecuteNonQuery(Usql);
                    }
                    if(AutoDispatchFS != IsAutoDispatchFS)
                    {
                        string UsqlFS = @"UPDATE ParmCode SET 
                                    Enable = @Enable,
                                    ModifiedUser = @ModifiedUser,
                                    ModifiedDate = GETDATE() 
                                WHERE CodeType = @CodeType";// CodeUid = @CodeUid and 
                        base.Parameter.Clear();
                        //base.Parameter.Add(new CommandParameter("@CodeUid", "8053"));
                        base.Parameter.Add(new CommandParameter("@CodeType", "AutoDispatchFS"));
                        base.Parameter.Add(new CommandParameter("@Enable", IsAutoDispatchFS == "true" ? "1" : "0"));
                        base.Parameter.Add(new CommandParameter("@ModifiedUser", Account));
                        rtn = base.ExecuteNonQuery(UsqlFS);
                    }
                    
                }
                if (rtn > 0)
                {
                    return "1";
                }
                else
                {
                    return "0";
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }

        }     
        /// <summary>
        /// 查詢是否啟用自動派送
        /// </summary>
        /// <returns></returns>
        public AgentSetting GetEnable()
        {
            string AutoDispatch = string.Empty;
            string AutoDispatchFS = string.Empty;
            string sql = @"select CodeType,Enable from PARMCode where CodeType like 'AutoDispatch%'";
            IList<PARMCode> PMlist = base.SearchList<PARMCode>(sql);
            foreach (PARMCode item in PMlist)
            {
                if (item.CodeType == "AutoDispatch" && item.Enable == true)
                {
                    AutoDispatch = "true";
                }
                else if (item.CodeType == "AutoDispatch" && item.Enable == false)
                {
                    AutoDispatch = "false";
                }
                if (item.CodeType == "AutoDispatchFS" && item.Enable == true)
                {
                    AutoDispatchFS = "true";
                }
                else if (item.CodeType == "AutoDispatchFS" && item.Enable == false)
                {
                    AutoDispatchFS = "false";
                }
            }
            AgentSetting AScode = new AgentSetting();
            AScode.IsAutoDispatch = AutoDispatch == "true" ? true : false;
            AScode.IsAutoDispatchFS = AutoDispatchFS == "true" ? true : false;
            return AScode;
        }
        /// <summary>
        /// 查扣押經辦人數
        /// </summary>
        /// <returns></returns>
        public int GetAutoDispatchNum(string Department = null)
        {
            try
            {
                int count = 0;
                string sqlStr = @"select count(*) from AgentSetting where IsSeizure =1";
                if (!string.IsNullOrEmpty(Department))
                {
                    sqlStr += " and SectionName = @SectionName";
                    base.Parameter.Clear();
                    base.Parameter.Add(new CommandParameter("@SectionName", Department));
                }
                count = (int)base.ExecuteScalar(sqlStr);
                return count;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
     
        /// <summary>
        /// 查外來文經辦人數
        /// </summary>
        /// <returns></returns>
        public int GetAutoDispatchFSNum(string Department = null)
        {
            try
            {
                int count = 0;
                string sqlStr = @"select count(*) from AgentSetting where IsCase =1";
                if (!string.IsNullOrEmpty(Department))
                {
                    sqlStr += " and SectionName = @SectionName";
                    base.Parameter.Clear();
                    base.Parameter.Add(new CommandParameter("@SectionName", Department));
                }
                count = (int)base.ExecuteScalar(sqlStr);
                return count;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //20170421 固定 RQ-2015-019666-017 派件至跨單位 宏祥 add start
        /// <summary>
        /// 取得設定的所有經辦人員科組
        /// </summary>
        /// <returns></returns>
        public IList<AgentSetting> GetAgentDepartment2View()
        {
           string strSql = @"select distinct SectionName,DepartmentID from AgentSetting";
           base.Parameter.Clear();
           return base.SearchList<AgentSetting>(strSql);
        }

        //20170421 固定 RQ-2015-019666-017 派件至跨單位 宏祥 add start
        /// <summary>
        /// 取得設定的所有經辦人員
        /// </summary>
        /// <returns></returns>
        public IList<AgentSetting> GetAgentDepartmentUserView()
        {
           string strSql = @"select A.EmpID,A.EmpID + ' - ' + E.EmpName AS EmpIdAndName 
                             from AgentSetting AS A LEFT OUTER JOIN
                             LDAPEmployee AS E ON A.EmpId = E.EmpID";
           base.Parameter.Clear();
           return base.SearchList<AgentSetting>(strSql);
        }

        //20170421 固定 RQ-2015-019666-017 派件至跨單位 宏祥 add start
        /// <summary>
        /// 取得分行主管角色，主管改派頁面科組下拉選單
        /// </summary>
        /// <returns></returns>
        public IList<AgentSetting> GetDirectorReassignAgentDepartment2View(string EmpID)
        {
           string strSql = @"select distinct A.SectionName,A.DepartmentID from AgentSetting as A LEFT OUTER JOIN
                             LDAPEmployee as L on A.EmpID=L.EmpID
                             where L.managerid like @managerid";
           base.Parameter.Clear();
           base.Parameter.Add(new CommandParameter("@managerid", "%" + EmpID + "%"));
           return base.SearchList<AgentSetting>(strSql);
        }

        //20170421 固定 RQ-2015-019666-017 派件至跨單位 宏祥 add start
        /// <summary>
        /// 取得分行主管角色，主管改派頁面經辦人員下拉選單
        /// </summary>
        /// <returns></returns>
        public IList<AgentSetting> GetDirectorReassignAgentDepartmentUserView(string EmpID)
        {
           string strSql = @"select A.EmpID,L.EmpName,A.SectionName AS DeptId,A.EmpID + ' - ' + L.EmpName AS EmpIdAndName
                             from AgentSetting as A LEFT OUTER JOIN
                             LDAPEmployee as L on A.EmpID=L.EmpID
                             where L.managerid like @managerid";
           base.Parameter.Clear();
           base.Parameter.Add(new CommandParameter("@managerid", "%" + EmpID + "%"));
           return base.SearchList<AgentSetting>(strSql);
        }

        //20170421 固定 RQ-2015-019666-017 派件至跨單位 宏祥 add start
        /// <summary>
        /// 取得選取某處的所有經辦人員
        /// </summary>
        /// <returns></returns>
        public IList<AgentSetting> GetAgentDepartmentUserView(string DepartmentID)
        {
           try
           {
              string strSql = @"select EmpID from AgentSetting where DepartmentID=@DepartmentID";
              base.Parameter.Clear();
              base.Parameter.Add(new CommandParameter("@DepartmentID", DepartmentID));
              return base.SearchList<AgentSetting>(strSql);
              //IList<AgentSetting> list = base.SearchList<AgentSetting>(strSql);
              //if (list.Count > 0)
              //{
              //   string Empid = string.Empty;
              //   foreach (AgentSetting item in list)
              //   {
              //      Empid += item.EmpId + ",";
              //   }
              //   Empid = Empid.Trim(',');
              //   return Empid;
              //}
              //else
              //{
              //   return "";
              //}
           }
           catch (Exception ex)
           {

              throw ex;
           }
        }

        /// <summary>
        /// 取得已設定經辦人員的單位下拉選單(收發作業-待辦理-派件)(主管作業-主管改派)
        /// 20170421 固定 RQ-2015-019666-017 派件至跨單位 宏祥 add
        /// </summary>
        /// <returns></returns>
        public IList<AgentSetting> GetAgentDepartmentView(string EmpID)
        {
           try
           {
              string strSql = @"select DepartmentID from AgentSetting where EmpID=@EmpID";
              base.Parameter.Clear();
              base.Parameter.Add(new CommandParameter("@EmpID", EmpID));
              return base.SearchList<AgentSetting>(strSql);
           }
           catch (Exception ex)
           {
              throw ex;
           }
        }
    }
}
