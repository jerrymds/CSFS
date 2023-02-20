using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CTBC.CSFS.Models;
using CTBC.CSFS.Pattern;
using CTBC.CSFS.ViewModels;
using Microsoft.Practices.EnterpriseLibrary.Logging.Configuration.Manageability.TraceListeners;
using System.Data;

namespace CTBC.CSFS.BussinessLogic
{
    public class LdapEmployeeBiz : CommonBIZ
    {
        public LdapEmployeeBiz(AppController appController)
            : base(appController)
        {

        }

        public LdapEmployeeBiz() { }
        /// <summary>
        /// 取出LdapEmployee中指定的人員不限一二三科(不含科)
        /// </summary>
        /// <param name="empid"></param>
        /// <returns></returns>
        public LDAPEmployee GetLdapEmployeeByEmpId(string empid)
        {
            try
            {
                string sql = @"SELECT [EmpID]
                                  ,[EmpName]
                                  ,[EmpTitle]
                                  ,[EmpBusinessCategory]
                                  ,[IsManager]
                                  ,[EMail]
                                  ,E.[DepDN]
                                  ,E.[DepID]
                                  ,[ManagerID]
                                  ,E.[CreatedUser]
                                  ,E.[CreatedDate]
                                  ,E.[ModifiedUser]
                                  ,E.[ModifiedDate]
	                              ,D.[DepName]
                                  ,E.[BranchID]
                                  ,E.[BranchName]
                                  ,E.[TelNo]
                                  ,E.[TelExt]
                              FROM [LDAPEmployee] AS E with (nolock)
                              LEFT OUTER JOIN [LDAPDepartment] AS D with (nolock) ON E.DepID = D.DepID
                              WHERE E.EmpID=@EmpID";
                // 清空容器
                Parameter.Clear();
                Parameter.Add(new CommandParameter("@EmpID", empid));
                IList<LDAPEmployee> list = SearchList<LDAPEmployee>(sql);
                return list != null  && list.Count > 0 ? list[0] : null;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        /// <summary>
        /// 取出LdapEmployee中指定的人員不限一二三科(不含科)
        /// </summary>
        /// <param name="depart"></param>
        /// <returns></returns>
        public List<LDAPEmployee> GetLdapEmployeeListByDepart(string depart)
        {
            try
            {
                string sql = @"SELECT P.* FROM  [LDAPDepartment] d
                                       inner join ldapEmployee p on P.DepDN LIKE '%'+d.depid + '%'
                                       where  d.DepName in (@depart) order by empid;";
                // 清空容器
                Parameter.Clear();
                Parameter.Add(new CommandParameter("@depart", depart));
                IList<LDAPEmployee> list = SearchList<LDAPEmployee>(sql);
                List<LDAPEmployee> listitem = new List<LDAPEmployee>();
                if (list != null && list.Count > 0)
                {
                    foreach (var item in list)
                    {
                        listitem.Add(item);
                    }
                    return listitem;
                }
                else
                {
                    return new List<LDAPEmployee>();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 取出LdapEmployee中主管
        /// </summary>
        /// <param name="depart"></param>
        /// <returns></returns>
        public List<LDAPEmployee> GetLdapManagerListByDepart(string depart)
        {
            try
            {
                string sql = @"SELECT P.*,n.DepName
                               FROM  [LDAPDepartment] d
                               inner join ldapEmployee p on P.DepDN LIKE '%'+d.depid + '%'
                               inner join LDAPDepartment n on p.DepID = n.DepID
                               where  d.DepName in (@depart) and len(p.isManager) > 5";
                // 清空容器
                Parameter.Clear();
                Parameter.Add(new CommandParameter("@depart", depart));
                IList<LDAPEmployee> list = SearchList<LDAPEmployee>(sql);
                List<LDAPEmployee> listitem = new List<LDAPEmployee>();
                if (list != null && list.Count > 0)
                {
                    foreach (var item in list)
                    {
                        listitem.Add(item);
                    }
                    return listitem;
                }
                else
                {
                    return new List<LDAPEmployee>();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        /// <summary>
        /// 取得所有集作 一科二科三科的經辦(包含科名稱)
        /// </summary>
        /// <returns></returns>
        public List<LDAPEmployee> GetAllEmployeeNoMangerView()
        {
            string strSql = @"SELECT  [EmpID]
                                  ,[EmpName]
                                  ,[SectionName]
                                  ,[DepName]
                                  ,[UpperDepID]
                                  ,[UpDepName]
                                  ,[EmpTitle]
                                  ,[EmpBusinessCategory]
                                  ,[IsManager]
                                  ,[EMail]
                                  ,[DepDN]
                                  ,[DepID]
                                  ,[ManagerID]
                                  ,[BranchID]
                                  ,[BranchName]
                                  ,[CreatedUser]
                                  ,[CreatedDate]
                                  ,[ModifiedUser]
                                  ,[ModifiedDate]
                                  ,[EmpID] + ' - ' + [EmpName] AS [EmpIdAndName]
                              FROM [V_AgentAndDept] where len(IsManager) = 0
                              ORDER BY [SectionName],DepName,EmpID";
            Parameter.Clear();
            IList<LDAPEmployee> rtn = SearchList<LDAPEmployee>(strSql);
            return rtn == null ? new List<LDAPEmployee>() : rtn.ToList();
        }

		/// <summary>
		/// 取得所有放行主管
        /// </summary>
        /// <returns></returns>
		public List<LDAPEmployee> GetAllMangers()
        {
			string strSql = @"select distinct [LDAPEmployee].EmpID,[LDAPEmployee].EmpID + ' - ' + [LDAPEmployee].EmpName as [EmpIdAndName] from [dbo].[CaseMaster]
inner join [dbo].[LDAPEmployee]
on [CaseMaster].ApproveUser = [dbo].[LDAPEmployee].EmpID
and [CaseMaster].ApproveUser is not null
and [dbo].[LDAPEmployee].EmpID is not null";
            Parameter.Clear();
            IList<LDAPEmployee> rtn = SearchList<LDAPEmployee>(strSql);
            return rtn == null ? new List<LDAPEmployee>() : rtn.ToList();
        }

        /// <summary>
        /// 取得所有集作 一科二科三科的經辦(包含科名稱)
        /// </summary>
        /// <returns></returns>
        public List<LDAPEmployee> GetAllEmployeeInEmployeeView()
        {
            string strSql = @"SELECT  [EmpID]
                                  ,[EmpName]
                                  ,[SectionName]
                                  ,[DepName]
                                  ,[UpperDepID]
                                  ,[UpDepName]
                                  ,[EmpTitle]
                                  ,[EmpBusinessCategory]
                                  ,[IsManager]
                                  ,[EMail]
                                  ,[DepDN]
                                  ,[DepID]
                                  ,[ManagerID]
                                  ,[BranchID]
                                  ,[BranchName]
                                  ,[CreatedUser]
                                  ,[CreatedDate]
                                  ,[ModifiedUser]
                                  ,[ModifiedDate]
                                  ,[EmpID] + ' - ' + [EmpName] AS [EmpIdAndName]
                              FROM [V_AgentAndDept]
                              ORDER BY [SectionName]";
            Parameter.Clear();
            IList<LDAPEmployee> rtn = SearchList<LDAPEmployee>(strSql);
            return rtn == null ? new List<LDAPEmployee>() : rtn.ToList();
        }
        /// <summary>
        /// 取出一科二科三科中指定的經辦.(包含科名稱)
        /// </summary>
        /// <param name="empId"></param>
        /// <returns></returns>
        public LDAPEmployee GetAllEmployeeInEmployeeViewByEmpId(string empId)
        {
            string strSql = @"SELECT  [EmpID]
                                  ,[EmpName]
                                  ,[SectionName]
                                  ,[DepName]
                                  ,[UpperDepID]
                                  ,[UpDepName]
                                  ,[EmpTitle]
                                  ,[EmpBusinessCategory]
                                  ,[IsManager]
                                  ,[EMail]
                                  ,[DepDN]
                                  ,[DepID]
                                  ,[ManagerID]
                                  ,[BranchID]
                                  ,[BranchName]
                                  ,[CreatedUser]
                                  ,[CreatedDate]
                                  ,[ModifiedUser]
                                  ,[ModifiedDate]
                              FROM [V_AgentAndDept] 
                              WHERE [EmpID] = @EmpId ";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("EmpId", empId));
            IList<LDAPEmployee> rtn = SearchList<LDAPEmployee>(strSql);
            return rtn == null ? null : rtn.FirstOrDefault();
        }


        /// <summary>
        /// 取得最高主管ID列表
        /// </summary>
        /// <returns></returns>
        public List<string> GetTopEmployeeDirectorIdList()
        {
            string strSql = @"SELECT EmpId FROM LDAPEmployee WHERE EmpBusinessCategory LIKE '%部長%' 
                    AND DepDN = 'ou=M00024839,ou=M00022677,ou=M00022311,ou=U00021934,ou=U00021933,ou=U00021932,ou=U00021931,ou=U00021800,ou=HRIS,o=CTCB';";
            Parameter.Clear();
            IList<LDAPEmployee> result = SearchList<LDAPEmployee>(strSql);
            List<string> rtn = new List<string>();
            if (result != null && result.Any())
            {
                rtn.AddRange(result.Select(employee => employee.EmpId));
            }
            return rtn;

        }

        public IList<AgentAndBuInfo> GetAgentAndBuInfosByBuId(string deptId)
        {
           //20170421 固定 RQ-2015-019666-017 派件至跨單位 宏祥 update start
//            string strSql = @"SELECT
//                                A.EmpId,E.EmpName, E.SectionName AS DeptId ,E.SectionName AS DepName
//                                FROM [AgentSetting] AS A
//                                LEFT OUTER JOIN [V_AgentAndDept] AS E ON A.EmpId = E.EmpId
//                                WHERE ISNULL(@DeptId,'') = '' OR E.SectionName = @DeptId
//                                ORDER BY E.SectionName,A.EmpId";
           string strSql = @"SELECT
                                A.EmpId,E.EmpName, 
								        CASE WHEN E.BranchName = E.UpDepName THEN E.UpDepName + ' - ' + E.SectionName
								             WHEN E.BranchName = E.DepName THEN E.DepName
								        ELSE E.SectionName END AS DeptId,E.SectionName AS DepName
                                FROM [AgentSetting] AS A
                                LEFT OUTER JOIN [V_AgentAndDept] AS E ON A.EmpId = E.EmpId
                                WHERE ISNULL(@DeptId,'') = '' OR E.SectionName = @DeptId
                                ORDER BY E.SectionName,A.EmpId";
           //20170421 固定 RQ-2015-019666-017 派件至跨單位 宏祥 update end
            Parameter.Clear();
            Parameter.Add(new CommandParameter("@DeptId", deptId));
            return SearchList<AgentAndBuInfo>(strSql);
        }


        public IList<AgentAndBuInfo> GetAgentAndBu(string deptId)
        {
           //20170421 固定 RQ-2015-019666-017 派件至跨單位 宏祥 update start
//            string strSql = @"SELECT
//                                A.EmpId,E.EmpName, E.SectionName AS DeptId ,E.SectionName AS DepName
//                                FROM V_AgentAndDept AS A
//                                LEFT OUTER JOIN [V_AgentAndDept] AS E ON A.EmpId = E.EmpId
//                                WHERE ISNULL(@DeptId,'') = '' OR E.SectionName = @DeptId
//                                ORDER BY E.SectionName,A.EmpId";
           string strSql = @"SELECT EmpId,EmpName, 
								            CASE WHEN BranchName = UpDepName THEN UpDepName + ' - ' + SectionName
									         WHEN BranchName = DepName THEN DepName
								            ELSE SectionName END AS DeptId 
								            ,SectionName AS DepName
                             FROM V_AgentAndDept
                             WHERE ISNULL(@DeptId,'') = '' OR SectionName = @DeptId
                             ORDER BY DeptId";
           //20170421 固定 RQ-2015-019666-017 派件至跨單位 宏祥 update end
            Parameter.Clear();
            Parameter.Add(new CommandParameter("@DeptId", deptId));
            return SearchList<AgentAndBuInfo>(strSql);
        }

        /// <summary>
        /// 取得所有可以不用設置Role的EmpId列表
        /// </summary>
        /// <returns></returns>
        public IList<LDAPEmployee> GetNoRoleEmployeeIdList()
        {
            string[] ary = {};
            //* 得到參數設定中無需設置role的EmpBusinessCategory列表,用";"分割
            IList<PARMCode> listNoRoleBusinessCategorys = GetCodeData("NoRoleBusinessCategory");
            if (listNoRoleBusinessCategorys != null && listNoRoleBusinessCategorys.Any())
            {
                var obj = listNoRoleBusinessCategorys.FirstOrDefault();
                if (obj != null && !string.IsNullOrEmpty(obj.CodeMemo))
                {
                    ary = obj.CodeMemo.Split(';');
                }
            }
            //* 沒有就返回
            if(ary.Length<=0)
                return null;
            
            //*開始取資料
            //20170630 緊急 RQ-2015-019666-018 派件至跨單位 宏祥 update start
            string sql =
                @"SELECT [EmpID],[EmpName],[EmpTitle],[EmpBusinessCategory],[IsManager],[EMail],[DepDN],[DepID],[ManagerID],[BranchID],[BranchName],[CreatedUser],[CreatedDate],[ModifiedUser],[ModifiedDate],[TelNo],[TelExt] 
                  FROM [LDAPEmployee] WHERE ISNULL([BranchID],'') <> '' AND ISNULL([BranchName],'') <> '' AND ( 1=2 ";
              //@"SELECT [EmpID],[EmpName],[EmpTitle],[EmpBusinessCategory],[IsManager],[EMail],[DepDN],[DepID],[ManagerID],[BranchID],[BranchName],[CreatedUser],[CreatedDate],[ModifiedUser],[ModifiedDate],[TelNo],[TelExt] 
                //FROM [LDAPEmployee] WHERE 1=2";            
            Parameter.Clear();
            for (int i = 0; i < ary.Length; i++)
            {
                string parmName = "Category" + i;
                sql = sql + " OR [EmpBusinessCategory] = @" + parmName + " ";
                Parameter.Add(new CommandParameter(parmName, ary[i]));
            }
            sql = sql + ")";

            string strCodeMemo = "";
            //* 得到參數設定中無需設置role的EmpBusinessCategoryException列表
            IList<PARMCode> listNoRoleBusinessCategorysException = GetCodeData("NoRoleBusinessCategoryException");
            if (listNoRoleBusinessCategorysException != null && listNoRoleBusinessCategorysException.Any())
            {
               var obj = listNoRoleBusinessCategorysException.FirstOrDefault();
               if (obj != null && !string.IsNullOrEmpty(obj.CodeMemo))
               {
                  strCodeMemo = obj.CodeMemo;
               }
            }
            if (strCodeMemo != "")
            {
               sql = sql + " OR [EmpID] in (" + strCodeMemo + ")";
            }
            return SearchList<LDAPEmployee>(sql);
           //20170630 緊急 RQ-2015-019666-018 派件至跨單位 宏祥 update end
        }

        //* 20150818 新增分行主管
        public IList<LDAPEmployee> GetNoRoleEmployeeIdList2()
        {
            string[] ary = { };
            //* 得到參數設定中無需設置role的EmpBusinessCategory列表,用";"分割
            IList<PARMCode> listNoRoleBusinessCategorys = GetCodeData("NoRoleBusinessCategory2");
            if (listNoRoleBusinessCategorys != null && listNoRoleBusinessCategorys.Any())
            {
                var obj = listNoRoleBusinessCategorys.FirstOrDefault();
                if (obj != null && !string.IsNullOrEmpty(obj.CodeMemo))
                {
                    ary = obj.CodeMemo.Split(';');
                }
            }
            //* 沒有就返回
            if (ary.Length <= 0)
                return null;

            //*開始取資料
            //20170630 緊急 RQ-2015-019666-018 派件至跨單位 宏祥 update start
            string sql =
                @"SELECT [EmpID],[EmpName],[EmpTitle],[EmpBusinessCategory],[IsManager],[EMail],[DepDN],[DepID],[ManagerID],[BranchID],[BranchName],[CreatedUser],[CreatedDate],[ModifiedUser],[ModifiedDate],[TelNo],[TelExt] 
                  FROM [LDAPEmployee] WHERE ISNULL([BranchID],'') <> '' AND ISNULL([BranchName],'') <> '' AND ( 1=2 ";
//                  @"SELECT [EmpID],[EmpName],[EmpTitle],[EmpBusinessCategory],[IsManager],[EMail],[DepDN],[DepID],[ManagerID],[BranchID],[BranchName],[CreatedUser],[CreatedDate],[ModifiedUser],[ModifiedDate],[TelNo],[TelExt] 
//                  FROM [LDAPEmployee] WHERE 1=2 ";
            Parameter.Clear();
            for (int i = 0; i < ary.Length; i++)
            {
                string parmName = "Category" + i;
                sql = sql + " OR [EmpBusinessCategory] = @" + parmName + " ";
                Parameter.Add(new CommandParameter(parmName, ary[i]));
            }
            sql = sql + ")";

            string strCodeMemo = "";
            //* 得到參數設定中無需設置role的EmpBusinessCategory2Exception列表
            IList<PARMCode> listNoRoleBusinessCategorysException = GetCodeData("NoRoleBusinessCategory2Exception");
            if (listNoRoleBusinessCategorysException != null && listNoRoleBusinessCategorysException.Any())
            {
               var obj = listNoRoleBusinessCategorysException.FirstOrDefault();
               if (obj != null && !string.IsNullOrEmpty(obj.CodeMemo))
               {
                  strCodeMemo = obj.CodeMemo;
               }
            }
            if (strCodeMemo != "")
            {
               sql = sql + " OR [EmpID] in (" + strCodeMemo + ")";
            }
            return SearchList<LDAPEmployee>(sql);
           //20170630 緊急 RQ-2015-019666-018 派件至跨單位 宏祥 update end
        }


        /// <summary>
        /// 取得所有總務的部門ID列表
        /// </summary>
        /// <returns></returns>
        public List<string> GetAffairsDeptIdList()
        {
            IList<PARMCode> list = GetCodeData("AffairsDeptId", true);
            if(list == null || !list.Any())
                return new List<string>();

            var obj = list.FirstOrDefault();

            if (obj != null && !string.IsNullOrEmpty(obj.CodeMemo))
            {
                return obj.CodeMemo.Split(';').ToList();
            }
            return new List<string>();
        }

        public string GetBranchId()
        {
            LDAPEmployee user = GetLdapEmployeeByEmpId(Account);
            List<string> affairs = GetAffairsDeptIdList();

            string depdn = user.DepDn;
            if (affairs != null && affairs.Any())
            {
                if (affairs.Any(affair => depdn.Contains(affair)))
                {
                    return "8888";
                }
            }
            return user.BranchId;
        }
        public string GetBranchId(LDAPEmployee user)
        {
            List<string> affairs = GetAffairsDeptIdList();
            if (user != null)
            {
                string depdn = user.DepDn;
                if (affairs != null && affairs.Any(affair => depdn.Contains(affair)))
                {
                    return "8888";
                }
                return user.BranchId;
            }
            return "";
        }

        /// <summary>
        /// 20170714 固定 RQ-2015-019666-019 派件至跨單位 宏祥 add
        /// 取得所有科別參數集作的經辦(包含科名稱)
        /// </summary>
        /// <returns></returns>
        public List<LDAPEmployee> GetAllDepartmentEmployeeInEmployeeView()
        {
           string strSql = @"SELECT  [EmpID]
                                  ,[EmpName]
                                  ,[SectionName]
                                  ,[DepName]
                                  ,[UpperDepID]
                                  ,[UpDepName]
                                  ,[EmpTitle]
                                  ,[EmpBusinessCategory]
                                  ,[IsManager]
                                  ,[EMail]
                                  ,[DepDN]
                                  ,[DepID]
                                  ,[ManagerID]
                                  ,[BranchID]
                                  ,[BranchName]
                                  ,A.[CreatedUser]
                                  ,A.[CreatedDate]
                                  ,A.[ModifiedUser]
                                  ,A.[ModifiedDate]
                                  ,[EmpID] + ' - ' + [EmpName] AS [EmpIdAndName]
                              FROM [V_AgentAndDept] AS A
                              INNER JOIN [PARMCode] AS B ON A.SectionName = B.CodeDesc
                              WHERE B.CodeType = 'Department'
                              ORDER BY [SectionName]";
           Parameter.Clear();
           IList<LDAPEmployee> rtn = SearchList<LDAPEmployee>(strSql);
           return rtn == null ? new List<LDAPEmployee>() : rtn.ToList();
        }


        /// <summary>
        /// 取出RM的email ..
        /// 
        /// </summary>
        /// <returns></returns>
        public DataTable GetRMInfo(string EmpName, string BranchID)
        {
            string strSql = @"SELECT  *
                              FROM LDAPEmployee
                              WHERE EmpName='{0}' AND BranchID = '{1}' ";

            strSql = string.Format(strSql, EmpName, BranchID.Trim());
            Parameter.Clear();
            DataSet ds = SearchToDataSet(strSql);
            return ds.Tables[0]; 
            
        }
        /// <summary>
        /// 取得讓分行的分行經理與服務經理
        /// </summary>
        /// <param name="BranchID"></param>
        /// <returns></returns>
        public DataTable GetManagerInfo(string BranchID)
        {
            //000662#母行經理                                                     //000547#母行服務經理
            //000662#分行經理                                                     //000548#服務經理
            string strSql = @"SELECT  *
                              FROM LDAPEmployee
                              WHERE ((IsManager like '%000662%' or IsManager like '%000663%' or IsManager like '%000547%' or IsManager like '%000548%' ) AND BranchID = '{0}' )";



            strSql = string.Format(strSql, BranchID.Trim());
            Parameter.Clear();
            DataSet ds = SearchToDataSet(strSql);
            return ds.Tables[0]; 
        }

        public DataTable GetLdapLDAPDepartmentByDepId(string Depid)
        {
            try
            {
                string sql = @"SELECT TOP 1 *   FROM [dbo].[LDAPDepartment] where DepID=@DepID";
                // 清空容器
                Parameter.Clear();
                Parameter.Add(new CommandParameter("@DepID", Depid));
                DataSet ds = SearchToDataSet(sql);
                return ds.Tables[0];
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
