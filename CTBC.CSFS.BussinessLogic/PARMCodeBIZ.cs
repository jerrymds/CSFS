/// <summary>
/// 程式說明：維護PARMCode參數檔
/// 維護部門:資訊管理處
/// 中國信託銀行 版權所有  ©  All Rights Reserved.
/// </summary>
/// 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using CTBC.CSFS.Models;
using CTBC.FrameWork.Platform;
using CTBC.CSFS.Pattern;
using NPOI.SS.UserModel;
using NPOI.HSSF.Util;
using NPOI.SS.Util;
using System.IO;
using NPOI.HSSF.UserModel;
using CTBC.FrameWork.HTG;

namespace CTBC.CSFS.BussinessLogic
{
    public class PARMCodeBIZ : CommonBIZ
    {
        public PARMCodeBIZ(AppController appController)
            : base(appController)
        { }

        public PARMCodeBIZ()
        { }

        /// <summary>
        /// 獲取所有代碼檔資料
        /// </summary>
        /// <returns></returns>
        public IEnumerable<PARMCode> SelectAllPARMCode()
        {
            try
            {
                string sql = @"SELECT *, (CodeNo + '-' + CodeDesc) as DescNo, (CodeNo + ',' + CodeTag) as CodeMix FROM PARMCode where Enable='1'";
                base.Parameter.Clear();
                return base.SearchList<PARMCode>(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 2018/04/24 Written by Patrick Yang
        /// </summary>
        /// <param name="codeType"></param>
        /// <returns></returns>
        public IList<PARMCode> GetParmCodeByCodeType(string codeType)
        {
            try
            {
                string sql = @"select * from PARMCode where CodeType=@CodeType order by codeno";
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@CodeType", codeType));
                return base.SearchList<PARMCode>(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 獲取參數類型(CodeType)列表
        /// </summary>
        /// <returns></returns>
        /// <remarks>2012/07/09 Kyle</remarks>
        public IList<PARMCode> GetCodeTypeListAll()
        {
            try
            {
                List<PARMCode> fList = new List<PARMCode>();
                IList<PARMCode> list = (IList<PARMCode>)AppCache.Get("PARMCode");
                var plist = (
                    from p in list
                    orderby p.CodeTypeDesc
                    select new { CodeType = p.CodeType, CodeTypeDesc = p.CodeTypeDesc }).Distinct();
                foreach (var k in plist)
                {
                    fList.Add(new PARMCode { CodeType = k.CodeType, CodeTypeDesc = k.CodeTypeDesc });
                }
                return fList;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 根據CodeUid的值獲取 參數信息
        /// </summary>
        /// <param name="codeUid">查詢條件</param>
        /// <returns></returns>
        /// <remarks>2012/07/09 Kyle</remarks>
        public PARMCode GetParmCodeModelByCodeUid(string codeUid)
        {
            try
            {
                PARMCode model;// 返回參數
                // 查詢語句
                IList<PARMCode> list = (IList<PARMCode>)AppCache.Get("PARMCode");
                var plist = from p in list
                            where p.CodeUid == codeUid
                            select p;
                IList<PARMCode> fList = plist.ToList();
                if (fList.Count > 0)
                {
                    model = fList[0];
                    //if (model.Enable) model.Enable = false;//不啟用
                    //else if (!model.Enable) model.Enable = true;//啟用
                }
                else
                {
                    model = new PARMCode();
                }

                return model;
            }
            catch (Exception ex)
            {
                // 拋出異常
                throw ex;
            }

            // 選取要返回的結果集
        }

        /// <summary>
        /// 根據參數類型，獲取該參數類型下數據量
        /// </summary>
        /// <param name="codeType">參數類型編號</param>
        /// <returns></returns>
        /// <remarks>2012/07/10 Kyle</remarks>
        public string GetSortOrderByCodeType(string codeType)
        {
            try
            {
                int count = 0;
                IList<PARMCode> list = (IList<PARMCode>)AppCache.Get("PARMCode");
                count = (from p in list
                         where p.CodeType == codeType
                         select p).Count();
                // 查詢返回數量+1 返回
                return (count + 1).ToString();
            }
            catch (Exception ex)
            {
                // 拋出異常
                throw ex;
            }
        }

        /// <summary>
        /// 根據參數類型，獲取參數類型細項(CodeNo)，用於下拉列表聯動
        /// </summary>
        /// <param name="codeType">參數類型Code</param>
        /// <returns></returns>
        /// <remarks>Add By Kyle 2012/07/23</remarks>
        public IList<PARMCode> GetCodeByCodeType(string codeType)
        {
            try
            {
                List<PARMCode> fList = new List<PARMCode>();
                IList<PARMCode> list = (IList<PARMCode>)AppCache.Get("PARMCode");
                fList = (from p in list
                         where p.CodeType == codeType
                         select new PARMCode { CodeNo = p.CodeNo, CodeDesc = p.CodeDesc }).ToList();
                return fList;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 根據參數類型，獲取參數類型細項(CodeNo),Sort by CodeNo，用於下拉列表聯動
        /// </summary>
        /// <param name="codeType">參數類型Code</param>
        /// <returns></returns>
        /// <remarks>2013/2/2</remarks>
        public List<PARMCode> GetCodeNoSorted(string codeType)
        {
            try
            {
                List<PARMCode> fList = new List<PARMCode>();
                fList = (from p in ((IList<PARMCode>)AppCache.Get("PARMCode"))
                         where p.CodeType == codeType
                         orderby p.CodeNo
                         select new PARMCode { CodeNo = p.CodeNo, CodeDesc = p.CodeNo + " " + p.CodeDesc }).ToList();
                return fList;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 根據參數類型編號和參數細項編號
        /// 獲取信息數量，用於判在新增欠斷
        /// 資料是否重複
        /// </summary>
        /// <param name="codeNo">參數類型編號</param>
        /// <param name="codeType">參數細項編號</param>
        /// <returns></returns>
        /// <remarks>2012/07/10 Kyle</remarks>
        public int GetParmCodeCount(string codeNo, string codeType)
        {
            try
            {
                int count = 0;
                IList<PARMCode> list = (IList<PARMCode>)AppCache.Get("PARMCode");
                count = (from p in list
                         where p.CodeNo == codeNo && p.CodeType == codeType
                         select p).Count();
                return count;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        ///  取得Code Desc, For 代碼的轉換
        /// </summary>
        /// <param name="codeType">代碼類別</param>
        /// <param name="codeNo">代碼</param>
        /// <returns></returns>
        /// <remarks>Added By Smallzhi 2013/01/05</remarks>
        public static string GetCodeDesc(string codeType, string codeNo)
        {
            #region 參數

            //可從Cache中將參數資料,透過ParmCode這個Key值直接取出
            var parmCodeList = (IEnumerable<PARMCode>)AppCache.Get("PARMCode");

            string query = (from item in parmCodeList
                            where item.CodeType == codeType && item.CodeNo == codeNo
                            select item.CodeDesc).FirstOrDefault();
            #endregion

            return (string.IsNullOrEmpty(query)) ? "" : query;
        }

        /// <summary>
        ///  取得Code Desc, For 代碼的轉換
        /// </summary>
        /// <param name="codeType">代碼類別</param>
        /// <param name="codeNo">代碼</param>
        /// <param name="codeTag"></param>
        /// <returns></returns>
        /// <remarks>Added By Smallzhi 2013/01/05</remarks>
        public static string GetCodeDescByCodeTag(string codeType, string codeNo, string codeTag)
        {
            #region 參數

            //可從Cache中將參數資料,透過ParmCode這個Key值直接取出
            var parmCodeList = (IEnumerable<PARMCode>)AppCache.Get("PARMCode");

            string query = (from item in parmCodeList
                            where item.CodeType == codeType && item.CodeNo == codeNo && item.CodeTag == codeTag
                            select item.CodeDesc).FirstOrDefault();
            #endregion

            return (string.IsNullOrEmpty(query)) ? "" : query;
        }

        //-------------------------------------------------------------------------------------
        //以下為維護PARMCode專屬Method,直接取自資料庫中PARMCode(啟用+非啟用)資料,非來自Cache
        //-------------------------------------------------------------------------------------
        /// <summary>
        /// Select全部PARMCode資料
        /// </summary>
        /// <returns></returns>
        public IEnumerable<PARMCode> Select()
        {
            try
            {
                string sql = @"SELECT * FROM PARMCode";
                base.Parameter.Clear();
                return base.SearchList<PARMCode>(sql);
            }
            catch (Exception ex)
            {
                // 拋出異常
                throw ex;
            }
        }

        public DataTable SelectToDT()
        {
            try
            {
                string sql = @"SELECT * FROM PARMCode";
                base.Parameter.Clear();
                return base.Search(sql);
            }
            catch (Exception ex)
            {
                // 拋出異常
                throw ex;
            }
        }

        /// <summary>
        /// 新增PARMCode資料
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool Create(PARMCode model)
        {
            // 插入SQL語句
            string sqlStr = @"insert into 
                                    PARMCode
                                    (
                                        CodeType,
                                        CodeTypeDesc,
                                        CodeNo,
                                        CodeDesc,
                                        SortOrder,
                                        CodeTag,
                                        CodeMemo,
                                        Enable,
                                        CreatedUser,
                                        CreatedDate,
                                        ModifiedUser,
                                        ModifiedDate
                                    ) 
                                    values
                                    (
                                        @CodeType,
                                        @CodeTypeDesc,
                                        @CodeNo,
                                        @CodeDesc,
                                        @SortOrder,
                                        @CodeTag,
                                        @CodeMemo,
                                        @Enable,
                                        @CreatedUser,
                                        GETDATE(),
                                        @ModifiedUser,
                                        GETDATE()
                                    )";

            // 清空參數容器
            base.Parameter.Clear();

            // 添加參數
            base.Parameter.Add(new CommandParameter("@CodeType", model.CodeType1));
            base.Parameter.Add(new CommandParameter("@CodeTypeDesc", model.CodeTypeDesc));
            base.Parameter.Add(new CommandParameter("@CodeNo", model.CodeNo));
            base.Parameter.Add(new CommandParameter("@CodeDesc", model.CodeDesc));
            base.Parameter.Add(new CommandParameter("@SortOrder", model.SortOrder));
            base.Parameter.Add(new CommandParameter("@CodeTag", model.CodeTag));
            base.Parameter.Add(new CommandParameter("@CodeMemo", model.CodeMemo));
            base.Parameter.Add(new CommandParameter("@Enable", (model.Enable.Value) ? "1" : "0"));//false=0=enable;true=1=disable
            base.Parameter.Add(new CommandParameter("@CreatedUser", Account));
            base.Parameter.Add(new CommandParameter("@ModifiedUser", Account));

            try
            {
                // 執行修改返回是否成功
                return base.ExecuteNonQuery(sqlStr) > 0;
            }
            catch (Exception ex)
            {
                // 拋出異常
                throw ex;
            }
        }

        /// <summary>
        /// 更新PARMCode資料
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        /// <remarks>Added By NianhuaXiao 2018/03/07</remarks>
        public bool UpdateCodeMemo(PARMCode model)
        {
            // 更新SQL語句
            string sqlStr = @"UPDATE 
                                    ParmCode
                                SET 
                                    CodeMemo = @CodeMemo,
                                    ModifiedUser = @ModifiedUser,
                                    ModifiedDate = GETDATE() 
                                WHERE 
                                    CodeUid = @CodeUid";

            // 清空參數容器
            base.Parameter.Clear();

            // 添加參數
            base.Parameter.Add(new CommandParameter("@CodeUid", model.CodeUid));
            base.Parameter.Add(new CommandParameter("@CodeMemo", model.CodeMemo));
            base.Parameter.Add(new CommandParameter("@ModifiedUser", Account));

            try
            {
                // 執行修改返回是否成功
                return base.ExecuteNonQuery(sqlStr) > 0;
            }
            catch (Exception ex)
            {
                // 拋出異常
                throw ex;
            }
        }

        /// <summary>
        /// 更新PARMCode資料
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        /// ------------------------------------
        /// 修改日期：2014/3/19
        /// 修改人員：莊筱婷
        /// 修改內容：讓USER可修改"順序"欄位
        public bool Update(PARMCode model)
        {
            // 修改SQL語句
            string sqlStr = @"UPDATE 
                                    ParmCode
                                SET 
                                    CodeNo = @CodeNo,
                                    CodeDesc = @CodeDesc,
                                    CodeMemo = @CodeMemo,
                                    CodeTag = @CodeTag,
                                    Enable = @Enable,
                                    SortOrder = @SortOrder,
                                    ModifiedUser = @ModifiedUser,
                                    ModifiedDate = GETDATE() 
                                WHERE 
                                    CodeUid = @CodeUid";

            // 編輯更新
            CSFSModificationNoticeBIZ _numBIZEdit = new CSFSModificationNoticeBIZ(); 
            CommandParameterCollection Parameters = new CommandParameterCollection();
            Parameters.Add(new CommandParameter("@CodeUid", model.CodeUid));
            _numBIZEdit.SaveBeforeData("ParmCode", "where CodeUid = @CodeUid", Parameters); 

            // 清空參數容器
            base.Parameter.Clear();

            // 添加參數
            base.Parameter.Add(new CommandParameter("@CodeNo", model.CodeNo));
            base.Parameter.Add(new CommandParameter("@CodeDesc", model.CodeDesc));
            base.Parameter.Add(new CommandParameter("@CodeMemo", model.CodeMemo));
            base.Parameter.Add(new CommandParameter("@CodeTag", model.CodeTag));
            base.Parameter.Add(new CommandParameter("@Enable", model.Enable));
            base.Parameter.Add(new CommandParameter("@ModifiedUser", Account));
            base.Parameter.Add(new CommandParameter("@CodeUid", model.CodeUid));

            //add by katie 2014/3/19
            base.Parameter.Add(new CommandParameter("@SortOrder", model.SortOrder));

            try
            {
                // 執行修改返回是否成功
                int _r = base.ExecuteNonQuery(sqlStr);

                Parameters = new CommandParameterCollection();
                Parameters.Add(new CommandParameter("@CodeUid", model.CodeUid));
                _numBIZEdit.SaveAfterData("ParmCode", "where CodeUid = @CodeUid", Parameters); 
                _numBIZEdit.SendMail("PARMCode維護", "修改");

                if (_r > 0) { return true; } else { return false; }
            }
            catch (Exception ex)
            {
                // 拋出異常
                throw ex;
            }
        }

        /// <summary>
        /// 刪除PARMCode資料
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool Delete(PARMCode model)
        {
            // 修改SQL語句
            string sqlStr = @"DELETE 
                                    ParmCode
                                WHERE 
                                    CodeUid = @CodeUid";

            // 清空參數容器
            base.Parameter.Clear();

            // 添加參數
            base.Parameter.Add(new CommandParameter("@CodeUid", model.CodeUid));

            try
            {
                // 執行修改返回是否成功
                return base.ExecuteNonQuery(sqlStr) > 0;
            }
            catch (Exception ex)
            {
                // 拋出異常
                throw ex;
            }
        }

        /// <summary>
        /// 根據參數類型編號和參數細項編號
        /// 獲取信息數量，用於判在新增欠斷
        /// 資料是否重複
        /// </summary>
        /// <param name="codeNo">參數類型編號</param>
        /// <param name="codeType">參數細項編號</param>
        /// <returns></returns>
        /// <remarks>2012/07/10 Kyle</remarks>
        public int Count(string codeNo, string codeType)
        {
            try
            {
                int count = 0;
                string sql = @"select count(0) from PARMCode where CodeType=@CodeType and CodeNo=@CodeNo";
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@CodeNo", codeNo));
                base.Parameter.Add(new CommandParameter("@CodeType", codeType));
                count = (int)base.ExecuteScalar(sql);
                return count;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 組合CodeTypeList下拉鍵
        /// </summary>
        /// <returns></returns>
        public IList<PARMCode> GetAllCodeTypeList()
        {
            try
            {
                string sql = @"select distinct CodeType,CodeTypeDesc from PARMCode order by CodeTypeDesc";
                base.Parameter.Clear();
                return base.SearchList<PARMCode>(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IList<PARMCode> CodeTypeList(string codeType)
        {
            try
            {
                if (!string.IsNullOrEmpty(codeType))
                {
                    IList<PARMCode> list = (IList<PARMCode>)AppCache.Get("PARMCode");
                    var pamCode = (from p in list
                                   where p.CodeType == codeType
                                   select new PARMCode { CodeType = p.CodeType, CodeNo = p.CodeNo, CodeTypeDesc = p.CodeTypeDesc }).First();
                    list = new List<PARMCode>();
                    if (pamCode == null)
                        return GetAllCodeTypeList();
                    else
                    {
                        list.Add(pamCode);
                        return list;
                    }
                }
                else
                    return GetAllCodeTypeList();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 顯示CodeType+CodeTypeDesc的下拉清單
        /// </summary>
        /// <param name="codeType"></param>
        /// <returns></returns>
        public IList<PARMCode> GetCodeTypeDescByCodeType(string codeType)
        {
            try
            {
                string sql = @"select CodeType,CodeTypeDesc from PARMCode where CodeType=@CodeType";
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@CodeType", codeType));
                return base.SearchList<PARMCode>(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        // <summary>
        // 顯示CodeNo+CodeDesc的下拉清單
        // </summary>
        // <param name="codeType">參數類型Code</param>
        // <returns></returns>
        public IList<PARMCode> GetCodeDescByCodeType(string codeType)
        {
            try
            {
                string sql = @"select CodeNo,CodeDesc from PARMCode where CodeType=@CodeType";
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@CodeType", codeType));
                return base.SearchList<PARMCode>(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 根據參數類型，獲取該參數類型下數據量
        /// </summary>
        /// <param name="codeType">參數類型編號</param>
        /// <returns></returns>
        /// <remarks>2012/07/10 Kyle</remarks>
        public string SortOrderByCodeType(string codeType)
        {
            try
            {
                int count = 0;
                string sql = @"select count(0) from PARMCode where CodeType=@CodeType";
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@CodeType", codeType));
                count = (int)base.ExecuteScalar(sql);
                return (count + 1).ToString();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 根據CodeUid的值獲取 參數信息
        /// </summary>
        /// <param name="codeUid">查詢條件</param>
        /// <returns></returns>
        /// <remarks</remarks>
        public PARMCode ModelByCodeUid(string codeUid)
        {
            try
            {
                string sql = @"select * from PARMCode where CodeUid=@CodeUid";
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@CodeUid", codeUid));
                IList<PARMCode> list = base.SearchList<PARMCode>(sql);
                if (list != null)
                {
                    if (list.Count > 0)
                    {
                        return list[0];
                    }
                    else
                    {
                        return new PARMCode();
                    }
                }
                else
                {
                    return new PARMCode();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IList<PARMCode> GetQueryList(PARMCode parmCode, int pageIndex, string strSortExpression, string strSortDirection)
        {
            try
            {
                base.PageIndex = pageIndex;
                string sqlStr = "";
                string sqlStrWhere = "";
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@pageS", (base.PageSize * (base.PageIndex - 1)) + 1));
                base.Parameter.Add(new CommandParameter("@pageE", base.PageSize * base.PageIndex));

                if (!string.IsNullOrEmpty(parmCode.CodeType))
                {
                    sqlStrWhere += @" AND CodeType = @CodeType";
                    base.Parameter.Add(new CommandParameter("@CodeType", parmCode.CodeType));
                }
                if (!string.IsNullOrEmpty(parmCode.CodeNo))
                {
                    sqlStrWhere += @" AND CodeNo = @CodeNo";
                    base.Parameter.Add(new CommandParameter("@CodeNo", parmCode.CodeNo));
                }
                if (parmCode.Enable.HasValue)
                {
                    sqlStrWhere += @" AND Enable = CONVERT(bit,@Enable)";
                    base.Parameter.Add(new CommandParameter("@Enable", parmCode.Enable.Value.ToString()));
                }
                sqlStr += @";with T1 
	                        as
	                        (
		                     select CodeUid,CodeType,CodeTypeDesc,CodeNo,CodeDesc,SortOrder,Enable from ParmCode with(nolock)	                            
	                            where 1=1 " + sqlStrWhere + @"   
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

                IList<PARMCode> _ilsit = base.SearchList<PARMCode>(sqlStr);

                if (_ilsit.Count > 0)
                {
                    base.DataRecords = _ilsit[0].maxnum;
                }
                else
                {
                    base.DataRecords = 0;
                    _ilsit = new List<PARMCode>();
                }
                return _ilsit;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        /// <summary>
        /// 查詢清單
        /// </summary>
        /// <param name="parmCode">查詢條件</param>
        /// <returns></returns>
        /// <remarks>2012/07/09 Kyle</remarks>
        public IList<PARMCode> QueryByPage(PARMCode parmCode, int pageIndex, bool state)
        {
            base.PageIndex = pageIndex;
            string sqlStr = "";
            if (state)
            {
                sqlStr += @"SELECT CodeUid,CodeType,CodeTypeDesc,CodeNo,CodeDesc,SortOrder,Enable FROM 
                    (
                        SELECT 
                            ROW_NUMBER() 
                            OVER(ORDER BY CodeType) 
                            AS ROWNUMBER,
                            * 
                        FROM 
                            ParmCode 
                        WHERE 
                            1 = 1";
            }
            else { sqlStr += @"select CodeUid from (select CodeUid from PARMCode where 1=1"; }

            try
            {
                // 清空參數容器
                base.Parameter.Clear();

                // 判斷有無選擇參數細項
                if (parmCode.QueryCodeNo != null && parmCode.QueryCodeNo != "")//存在參數細項則根據參數細項進行查詢
                {
                    sqlStr += @" AND CodeNo = @CodeNo";
                    base.Parameter.Add(new CommandParameter("@CodeNo", parmCode.QueryCodeNo));
                }

                // 判斷有無參數類型編號
                if (parmCode.QueryCodeType != null && parmCode.QueryCodeType != "")//不存在參數細項則根據參數類型進行查詢
                {
                    sqlStr += @" AND CodeType = @CodeType";
                    base.Parameter.Add(new CommandParameter("@CodeType", parmCode.QueryCodeType));
                }

                // 判斷參數狀態限制
                if (parmCode.QueryEnable != null && parmCode.QueryEnable != "")
                {
                    sqlStr += @" AND Enable = @Enable";
                    base.Parameter.Add(new CommandParameter("@Enable", parmCode.QueryEnable));
                }

                sqlStr += @") PAGE";

                // 判斷是否分頁
                if (state)
                {
                    sqlStr += @" WHERE ROWNUMBER > " + base.PageSize * (base.PageIndex - 1)
                                       + " AND ROWNUMBER < " + ((base.PageSize * base.PageIndex) + 1);
                }

                // 查詢筆數
                if (state)
                {
                    base.DataRecords = QueryByPage(parmCode, pageIndex, false).Count;
                }

                // 執行返回
                return base.SearchList<PARMCode>(sqlStr);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 根據GOV_KIND獲得GovUnit
        /// </summary>
        /// <param name="countryCode"></param>
        /// <param name="language"></param>
        /// <returns></returns>
        public IEnumerable<PARMCode> SelectGovUnitByGOV_KIND(string GOV_KIND)
        {
            try
            {
                string sql = @"select CodeNo,CodeDesc from [dbo].[PARMCode] where CodeTypeDesc=@CodeTypeDesc";
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@CodeTypeDesc", GOV_KIND));
                return base.SearchList<PARMCode>(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        /// <summary>
        /// 獲得預設時間
        /// </summary>
        /// <param name="GOV_KIND"></param>
        /// <returns></returns>
        public string GetCASE_END_TIME(string CASE_END_TIME)
        {
            try
            {
                string sql = @"select CodeNo from [dbo].[PARMCode]  where CodeType=@CodeType";
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@CodeType", CASE_END_TIME));
                return base.ExecuteScalar(sql).ToString();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 根據CodeDesc獲得CodeNo
        /// </summary>
        /// <param name="countryCode"></param>
        /// <param name="language"></param>
        /// <returns></returns>
        public string GetCodeNoByCodeDesc(string CodeDesc)
        {
            try
            {
                string sql = @"select CodeNo from [dbo].[PARMCode] where CodeDesc=@CodeDesc";
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@CodeDesc", CodeDesc));
                return base.ExecuteScalar(sql).ToString();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public string GetCaseIdByMemo(Guid CaseId)
        {
            try
            {
            	  // adam 取消財產目錄
                //string sql = @"select PropertyDeclaration from CaseMaster where CaseId=@CaseId and CaseKind2='財產申報'";
                string sql = @"select PropertyDeclaration from CaseMaster where CaseId=@CaseId and CaseKind='外來文案件'";
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@CaseId", CaseId));
                return base.ExecuteScalar(sql).ToString();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string GetCodeDescByCodeNo(string CodeNo)
        {
            try
            {
                string sql = @"select CodeDesc from [dbo].[PARMCode] where CodeNo=@CodeNo";
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@CodeNo", CodeNo));
                return base.ExecuteScalar(sql).ToString();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public DataTable ParmCodeSearchList(string CodeType, string Code, string Enable)
        {
            string sqlStrWhere = "";
            if (!string.IsNullOrEmpty(CodeType))
            {
                sqlStrWhere += @" AND CodeType = @CodeType";
                base.Parameter.Add(new CommandParameter("@CodeType", CodeType));
            }
            if (!string.IsNullOrEmpty(Code))
            {
                sqlStrWhere += @" AND CodeNo = @CodeNo";
                base.Parameter.Add(new CommandParameter("@CodeNo", Code));
            }
            if (!string.IsNullOrEmpty(Enable))
            {
                sqlStrWhere += @" AND Enable = @Enable";
                base.Parameter.Add(new CommandParameter("@Enable", Enable));
            }
            string strSql = @"select (CodeType+'/'+CodeTypeDesc) as CodeType,CodeNo,CodeDesc,SortOrder,
                                       case [enable] when 1 then '啟用' when 0 then '停用' end as enable from ParmCode  where 1=1  " + sqlStrWhere;
            DataTable dt = base.Search(strSql);
            if (dt != null && dt.Rows.Count > 0) return dt;
            else return new DataTable();
        }

        public MemoryStream ParmCodeExcel_NPOI(string CodeType, string Code, string Enable)
        {
            IWorkbook workbook = new HSSFWorkbook();
            ISheet sheet = workbook.CreateSheet("參數設定");

            #region 獲取數據源
            DataTable dt = ParmCodeSearchList(CodeType, Code, Enable);
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
            SetExcelCell(sheet, 0, 0, styleHead12, "參數設定");
            sheet.AddMergedRegion(new CellRangeAddress(0, 0, 0, 4));

            //*查詢條件 line1
            SetExcelCell(sheet, 1, 0, styleHead10, "參數類別(CodeType)/參數類別名稱(CodeTypeDesc)");
            sheet.AddMergedRegion(new CellRangeAddress(1, 1, 0, 0));
            sheet.SetColumnWidth(0, 100 * 120);
            SetExcelCell(sheet, 1, 1, styleHead10, "參數細項代碼(CodeNo)");
            sheet.AddMergedRegion(new CellRangeAddress(1, 1, 1, 1));
            sheet.SetColumnWidth(1, 100 * 50);
            SetExcelCell(sheet, 1, 2, styleHead10, "參數細項名稱(CodeDesc)");
            sheet.AddMergedRegion(new CellRangeAddress(1, 1, 2, 2));
            sheet.SetColumnWidth(2, 100 * 120);
            SetExcelCell(sheet, 1, 3, styleHead10, "參數細項順序(SortOrder)");
            sheet.SetColumnWidth(3, 100 * 50);
            sheet.AddMergedRegion(new CellRangeAddress(1, 1, 3, 3));
            SetExcelCell(sheet, 1, 4, styleHead10, "啟用狀態");
            sheet.AddMergedRegion(new CellRangeAddress(1, 1, 4, 4));
            sheet.SetColumnWidth(4, 100 * 50);
            #endregion

            #region body
            for (int iRow = 0; iRow < dt.Rows.Count; iRow++)
            {
                for (int iCol = 0; iCol < dt.Columns.Count ; iCol++)
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

        public PARMCode CheckLDAP(string userL, string passwordL, string userR, string passwordR)
        {
            string branchNo = "";
            IList<PARMCode> codeList = GetCodeDescByCodeType("eTabsQueryStaffBranchNo");
            if(codeList != null && codeList.Any())
            {
                branchNo = codeList.FirstOrDefault().CodeDesc;
            }
            //string branchNo = GetCodeDescByCodeType("eTabsQueryStaffBranchNo").FirstOrDefault().CodeDesc;
            var result = new PARMCode();
            ExecuteHTG objHTG = new ExecuteHTG(userL, passwordL, userR, passwordR, branchNo);
            var rtn = objHTG.HTGInitialize(userL, passwordL, userR, passwordR, branchNo);
            if(rtn)
            {
                result.status = "true";
                result.msg = "檢核成功!";
            }
            else
            {
                result.status = "false";
                result.msg = "檢核失敗!";
            }
            return result;
        }
    }
}
