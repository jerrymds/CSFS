/// <summary>
/// 程式說明：維護員工主檔
/// 維護部門:資訊管理處
/// 中國信託銀行 版權所有  ©  All Rights Reserved.
/// </summary>

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using CTBC.CSFS.Models;
using CTBC.CSFS.Pattern;
using CTBC.CSFS.ViewModels;
using CTBC.FrameWork.Util;
using CTBC.FrameWork.Platform;


namespace CTBC.CSFS.BussinessLogic
{
    public class CSFSEmployeeBIZ : CommonBIZ
    {
        public CSFSEmployeeBIZ(AppController appController)
            : base(appController)
        {

        }

        public CSFSEmployeeBIZ() { }

        public CSFSEmployee GetCSFSEmployeeByEmpID(string empid)
        {
            try
            {
                string sql = @"select EmpID,EmpName,EmpEMail,EmpTitle,EmpLevel,OnBoard,EmpRole from CSFSEmployee where EmpID=@EmpID";
                // 清空容器
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@EmpID", empid));

                IList<CSFSEmployee> list = base.SearchList<CSFSEmployee>(sql);

                return list.Count > 0 ? list[0] : null;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IList<CSFSEmployee> GetCSFSEmployeeAll()
        {
            try
            {
                string sql = @"select * from CSFSEmployee";
                // 清空容器
                base.Parameter.Clear();

                return base.SearchList<CSFSEmployee>(sql);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        ///  取得EMPName
        /// </summary>
        /// <param name="EmpID">EmpID</param>
        /// <returns></returns>
        public static string GetEmployeeName(string EmpID)
        {
            #region 參數
            var CSFSEmployeeList = (IEnumerable<CSFSEmployee>)AppCache.Get("CSFSEmployee");

            string query = (from item in CSFSEmployeeList
                            where item.EmpID == EmpID
                            select item.EmpName).FirstOrDefault();
            #endregion

            return (string.IsNullOrEmpty(query)) ? "" : query;
        }

        public CSFSEmployee GetCSFSEmployeeByEmpName(string empname)
        {
            try
            {
                string sql = @"select EmpID,EmpName,EmpEMail,EmpTitle,EmpLevel,OnBoard,EmpRole from CSFSEmployee where EmpName=@EmpName";
                // 清空容器
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@EmpName", empname));

                IList<CSFSEmployee> list = base.SearchList<CSFSEmployee>(sql);

                return list.Count > 0 ? list[0] : null;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //20140128 Tom新增
        public IList<CSFSEmployee> GetCSFSEmployeeByEmpVO(CSFSEmployee EmpVO)
        {
            try
            {

                string sqlStr = "";
                string sqlStrWhere = "";
                base.Parameter.Clear();

                if (!string.IsNullOrEmpty(EmpVO.EmpID))
                {
                    sqlStrWhere += @" and A.EmpID=@EmpID ";
                    base.Parameter.Add(new CommandParameter("@EmpID", EmpVO.EmpID));
                }

                if (!string.IsNullOrEmpty(EmpVO.EmpDept))
                {
                    sqlStrWhere += @" and B.BUID= @BUID ";
                    base.Parameter.Add(new CommandParameter("@BUID", EmpVO.EmpDept));
                }

                if (!string.IsNullOrEmpty(EmpVO.LandDept))
                {
                    sqlStrWhere += @" and A.LandDept= @LandDept ";
                    base.Parameter.Add(new CommandParameter("@LandDept", EmpVO.LandDept));
                }
                if (!string.IsNullOrEmpty(EmpVO.LandGroup))
                {
                    sqlStrWhere += @" and A.LandGroup= @LandGroup ";
                    base.Parameter.Add(new CommandParameter("@LandGroup", EmpVO.LandGroup));
                }
                if (!string.IsNullOrEmpty(EmpVO.LandRole))
                {
                    sqlStrWhere += @" and A.LandRole= @LandRole ";
                    base.Parameter.Add(new CommandParameter("@LandRole", EmpVO.LandRole));
                }
                sqlStr += @";with T1 
                            as
                            (
	                            select distinct A.EmpID,EmpName,C1.CodeDesc as LandDept,C2.CodeDesc as LandGroup,C3.CodeDesc as LandRole from CSFSEmployee A  with(nolock)
                                Left Join CSFSBUToEmployee B  with(nolock) ON A.EmpID=B.EmpID
                                Left Join PARMCode C1 with(nolock) ON A.LandDept=C1.CodeNo AND C1.CodeType='LFADept'
                                Left Join PARMCode C2 with(nolock) ON A.LandGroup=C2.CodeNo AND C2.CodeType='LFAGroup'
                                Left Join PARMCode C3 with(nolock) ON A.LandRole=C3.CodeNo AND C3.CodeType='LFARole'
	                            where 1=1 " + sqlStrWhere + @" 
                            ),T2 as
                            (
	                            select *, row_number() over (order by EmpID) RowNum
	                            from T1
                            )
                            select * from T2";

                IList<CSFSEmployee> _ilsit = base.SearchList<CSFSEmployee>(sqlStr);

                if (_ilsit.Count > 0)
                {
                    base.DataRecords = _ilsit[0].maxnum;
                }
                else
                {
                    base.DataRecords = 0;
                    _ilsit = new List<CSFSEmployee>();
                }
                return _ilsit;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //20140205 Tom 
        public int Delete(string id)
        {
            try
            {
                string sqlStr = @"delete CSFSEmployee where EmpID = @EmpID";

                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@EmpID", id));

                return base.ExecuteNonQuery(sqlStr);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 部門下拉選單取值
        /// </summary>
        /// <param name="cityCode">郵遞區號(父)</param>
        /// <returns>縣市(鄉鎮)</returns>
        /// <remarks>20140205 Tom</remarks>
        public IList<PARMCode> GetDept()
        {
            try
            {
                string sql = @"SELECT  
	                                BUID as CodeNo
	                               ,BUName as CodeDesc
                               FROM 
	                               CSFSBU 
                               WHERE  
	                               Enable = '1'
                               ORDER BY 
	                               BUName";

                // 清空容器
                base.Parameter.Clear();


                // 執行查詢
                return base.SearchList<PARMCode>(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //20140205 Tom新增員工
        public int Create(CSFSEmployee model)
        {
            try
            {
                //檢核員編
                bool chkEmp = chkEmpID(model.EmpID);
                if (chkEmp == true)
                {
                    string sqlStr = @"insert into CSFSEmployee
                                        (
                                            EmpID,
                                            EmpName,
                                            LandDept,
                                            LandGroup,
                                            LandRole,
                                            CreatedDate,
                                            CreatedUser,
                                            ModifiedDate,
                                            ModifiedUser,
                                            OnBoard
                                        ) 
                                        VALUES
                                        (
                                            @EmpID,
                                            @EmpName,
                                            @LandDept,
                                            @LandGroup,
                                            @LandRole,
                                            Getdate(),
                                            @CreatedUser,
                                            Getdate(),
                                            @ModifiedUser,
                                            '1'
                                        )";

                    base.Parameter.Clear();
                    base.Parameter.Add(new CommandParameter("@EmpID", model.EmpID));
                    base.Parameter.Add(new CommandParameter("@EmpName", model.EmpName));
                    base.Parameter.Add(new CommandParameter("@LandDept", model.LandDept));
                    base.Parameter.Add(new CommandParameter("@LandGroup", model.LandGroup));
                    base.Parameter.Add(new CommandParameter("@LandRole", model.LandRole));
                    base.Parameter.Add(new CommandParameter("@CreatedUser", Account));
                    base.Parameter.Add(new CommandParameter("@ModifiedUser", Account));

                    return base.ExecuteNonQuery(sqlStr);
                }
                else
                {
                    return 0;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //20140206 Tom 檢核員編
        private bool chkEmpID(string EmpID)
        {
            string sql = @"SELECT 
	                                EmpID
                               FROM 
                                    CSFSEmployee
                               WHERE
                                    EmpID = @EmpID
                                ";
            // 清除參數
            base.Parameter.Clear();

            // 參數賦值
            base.Parameter.Add(new CommandParameter("@EmpID", EmpID));

            object count = base.ExecuteScalar(sql);

            return count == null ? true : false;
        }

        /// <summary>
        /// 電謄組別下拉選單取值
        /// </summary>
        /// <param name="LFADept">科別</param>
        /// <returns>LFA組別</returns>
        /// <remarks>20140206 Tom</remarks>
        public IList<PARMCode> GetGroup(string LFADept)
        {
            try
            {

                string sql = @"select CodeNo,CodeDesc from PARMCode where CodeType='LFAGroup' AND CodeTag=@CodeTag AND Enable='1' Order by SortOrder ";

                // 清空容器
                base.Parameter.Clear();

                // 添加參數
                base.Parameter.Add(new CommandParameter("@CodeTag", LFADept));

                // 執行查詢
                return base.SearchList<PARMCode>(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //20140206 Tom
        public CSFSEmployee Select(string EmpID)
        {
            try
            {
                string sqlStr = @"select * from CSFSEmployee where EmpID=@EmpID";

                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@EmpID", EmpID));

                IList<CSFSEmployee> list = base.SearchList<CSFSEmployee>(sqlStr);
                if (list != null)
                {
                    if (list.Count > 0)
                    {
                        return list[0];
                    }
                    else
                    {
                        return new CSFSEmployee();
                    }
                }
                else
                {
                    return new CSFSEmployee();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //20140206 Tom
        public int Edit(CSFSEmployee model)
        {
            try
            {
                string sqlStr = @"update CSFSEmployee
                                  set
                                    EmpName=@EmpName,
                                    LandDept=@LandDept,
                                    LandGroup=@LandGroup,
                                    LandRole=@LandRole,
                                    ModifiedUser=@ModifiedUser,
                                    ModifiedDate=getdate()
                                  where EmpID = @EmpID";

                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@EmpName", model.EmpName));
                base.Parameter.Add(new CommandParameter("@LandDept", model.LandDept));
                base.Parameter.Add(new CommandParameter("@LandGroup", model.LandGroup));
                base.Parameter.Add(new CommandParameter("@LandRole", model.LandRole));
                base.Parameter.Add(new CommandParameter("@EmpID", model.EmpID));
                base.Parameter.Add(new CommandParameter("@ModifiedUser", Account));

                return base.ExecuteNonQuery(sqlStr);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        
        public IList<AgentAndBuInfo> GetAgentAndBuInfosByBuId(string deptId)
        {
            string strSql = @"SELECT
                                A.EmpId,E.EmpName, E.SectionName AS DeptId ,E.SectionName AS DepName
                                FROM [AgentSetting] AS A
                                LEFT OUTER JOIN [V_AgentAndDept] AS E ON A.EmpId = E.EmpId
                                WHERE ISNULL(@DeptId,'') = '' OR E.SectionName = @DeptId
                                ORDER BY E.SectionName,A.EmpId";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("@DeptId", deptId));
            return SearchList<AgentAndBuInfo>(strSql);
        }


        public IList<AgentAndBuInfo> GetAgentAndBu(string deptId)
        {
            string strSql = @"SELECT
                                A.EmpId,E.EmpName, E.SectionName AS DeptId ,E.SectionName AS DepName
                                FROM V_AgentAndDept AS A
                                LEFT OUTER JOIN [V_AgentAndDept] AS E ON A.EmpId = E.EmpId
                                WHERE ISNULL(@DeptId,'') = '' OR E.SectionName = @DeptId
                                ORDER BY E.SectionName,A.EmpId";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("@DeptId", deptId));
            return SearchList<AgentAndBuInfo>(strSql);
        }

    }
}