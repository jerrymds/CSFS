using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.IO;
using NPOI.SS.UserModel;
using NPOI.HSSF.Util;
using NPOI.SS.Util;
using NPOI.HSSF.UserModel;
using CTBC.FrameWork.Platform;
using CTBC.FrameWork.Util;
using CTBC.CSFS.Pattern;
using CTBC.CSFS.Models;

namespace CTBC.CSFS.BussinessLogic
{
    public class WarningFraudBIZ : CommonBIZ
    {
        public WarningFraudBIZ(AppController appController) : base(appController)
        { }

        public WarningFraudBIZ()
        { }

        /// <summary>
        /// 查詢聯防案件
        /// </summary>
        /// <param name="model"></param>
        /// <param name="isPaged"></param>
        /// <returns></returns>
        public IList<WarningFraud> GetQueryList(WarningFraud model, bool isPaged = true)
        {
            try
            {
                string sqlStr = "";
                string sqlStrWhere = "";
                base.Parameter.Clear();

                if (!string.IsNullOrEmpty(model.COL_C1003CASE))
                {
                    sqlStrWhere += @" AND w.COL_C1003CASE = @COL_C1003CASE";
                    base.Parameter.Add(new CommandParameter("@COL_C1003CASE", model.COL_C1003CASE.Trim()));
                }
                if (!string.IsNullOrEmpty(model.COL_ACCOUNT2))
                {
                    sqlStrWhere += @" AND w.COL_ACCOUNT2 = @COL_ACCOUNT2";
                    base.Parameter.Add(new CommandParameter("@COL_ACCOUNT2", model.COL_ACCOUNT2.Trim()));
                }
                if (!string.IsNullOrEmpty(model.COL_165CASE))
                {
                    sqlStrWhere += @" AND w.COL_165CASE = @COL_165CASE";
                    base.Parameter.Add(new CommandParameter("@COL_165CASE", model.COL_165CASE.Trim()));
                }
                if (!string.IsNullOrEmpty(model.Unit))
                {
                    sqlStrWhere += @" AND w.Unit = @Unit";
                    base.Parameter.Add(new CommandParameter("@Unit", model.Unit.Trim()));
                }
                if (!string.IsNullOrEmpty(model.CreateDateS))
                {
                    sqlStrWhere += @" AND w.CreatedDate >= @CreateDateS";
                    base.Parameter.Add(new CommandParameter("@CreateDateS", model.CreateDateS));
                }
                if (!string.IsNullOrEmpty(model.CreateDateE))
                {
                    string CreateDateE = UtlString.FormatDateString(Convert.ToDateTime(model.CreateDateE.Replace('/', ' ').ToString()).AddDays(1).ToString("yyyyMMdd"));
                    sqlStrWhere += @" AND w.CreatedDate <= @CreateDateE ";
                    base.Parameter.Add(new CommandParameter("@CreateDateE", CreateDateE));
                }
                if(!string.IsNullOrWhiteSpace(model.COL_OTHERBANKID))
                {
                    sqlStrWhere += " AND w.COL_OTHERBANKID LIKE @COL_OTHERBANKID ";
                    base.Parameter.Add(new CommandParameter("@COL_OTHERBANKID", "%" + model.COL_OTHERBANKID + "%"));
                }

                sqlStr = @"SELECT w.[NO], w.COL_ID, w.CASE_NO, w.COL_C1003CASE, w.CaseCreator
                          , w.COL_POLICE, w.COL_VICTIM, w.COL_OTHERBANKID, w.COL_165CASE, w.Unit
                          , w.COL_ACCOUNT2, w.EXT, w.Memo, a.AttachmentId
                          , CONVERT(varchar, w.CreatedDate, 111) AS CreatedDate
                        FROM WarningFraud AS w
						LEFT JOIN WarningFraudAttach AS a ON w.NO = a.WarningFraudNo
						WHERE C1003_BUSTYPE <> '警示通報' " + sqlStrWhere + "";

                IList<WarningFraud> _ilsit;
                if(isPaged)
                {
                    _ilsit = base.SearchPagedList<WarningFraud>(sqlStr);
                }
                else
                {
                    _ilsit = base.SearchList<WarningFraud>(sqlStr);
                }
                return _ilsit;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 新增聯防案件
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool CreateWarningFraud(WarningFraud model)
        {
            IDbConnection dbConnection;
            IDbTransaction dbTransaction;
            bool flag = false;

            try
            {
                using (dbConnection = OpenConnection())
                {
                    dbTransaction = dbConnection.BeginTransaction();

                    #region 聯防案件
                    string sql = @"Insert Into WarningFraud (CASE_NO, COL_165CASE, COL_C1003CASE, C1003_BUSTYPE, COL_POLICE, COL_ACCOUNT2, Unit, CaseCreator, EXT, COL_VICTIM, COL_OTHERBANKID, Memo, CreatedDate, CreatedUser, ModifiedDate, ModifiedUser, Status)
                                   Values (@CASE_NO, @COL_165CASE, @COL_C1003CASE, '聯防', @COL_POLICE, @COL_ACCOUNT2, @Unit, @CaseCreator, @EXT, @COL_VICTIM, @COL_OTHERBANKID, @Memo, @CreatedDate, @CreatedUser, GetDate(), @ModifiedUser, '0');";

                    sql += "Select SCOPE_IDENTITY() AS No;";

                    Parameter.Clear();
                    Parameter.Add(new CommandParameter("@CASE_NO", model.CASE_NO));
                    Parameter.Add(new CommandParameter("@COL_165CASE", model.COL_165CASE));
                    Parameter.Add(new CommandParameter("@COL_C1003CASE", model.COL_C1003CASE));
                    Parameter.Add(new CommandParameter("@COL_POLICE", model.COL_POLICE));
                    Parameter.Add(new CommandParameter("@COL_ACCOUNT2", model.COL_ACCOUNT2));
                    Parameter.Add(new CommandParameter("@Unit", model.Unit));
                    Parameter.Add(new CommandParameter("@CaseCreator", model.CaseCreator));
                    Parameter.Add(new CommandParameter("@EXT", model.EXT));
                    Parameter.Add(new CommandParameter("@COL_VICTIM", model.COL_VICTIM));
                    Parameter.Add(new CommandParameter("@COL_OTHERBANKID", model.COL_OTHERBANKID));
                    Parameter.Add(new CommandParameter("@Memo", model.Memo));
                    Parameter.Add(new CommandParameter("@CreatedDate", model.CreatedDate));
                    Parameter.Add(new CommandParameter("@CreatedUser", model.CreatedUser));
                    Parameter.Add(new CommandParameter("@ModifiedUser", model.ModifiedUser));

                    object insertResult = ExecuteScalar(sql, dbTransaction);
                    #endregion

                    if (insertResult == null)
                    {
                        dbTransaction.Rollback();
                    }
                    else
                    {
                        #region 附件
                        if (model.WarningFraudAttach != null)
                        {
                            model.WarningFraudAttach.WarningFraudNo = int.Parse(insertResult.ToString());
                            CreateAttatchment(model.WarningFraudAttach, dbTransaction);
                        }
                        #endregion

                        dbTransaction.Commit();
                        flag = true;
                    }

                }

                return flag;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 修改聯防案件
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool EditWarningFraud(WarningFraud model)
        {
            IDbConnection dbConnection;
            IDbTransaction dbTransaction;
            bool flag = false;

            try
            {
                using (dbConnection = OpenConnection())
                {
                    dbTransaction = dbConnection.BeginTransaction();

                    #region 聯防案件
                    string sql = @"Update WarningFraud Set CASE_NO = @CASE_NO, COL_165CASE = @COL_165CASE, COL_C1003CASE = @COL_C1003CASE
                                   , COL_POLICE = @COL_POLICE, COL_ACCOUNT2 = @COL_ACCOUNT2, Unit = @Unit, CaseCreator = @CaseCreator, EXT = @EXT
                                   , COL_VICTIM = @COL_VICTIM, COL_OTHERBANKID = @COL_OTHERBANKID, Memo = @Memo
                                   , CreatedDate = @CreatedDate, ModifiedDate = GETDATE()
                                   Where NO = @NO";

                    Parameter.Clear();
                    Parameter.Add(new CommandParameter("@CASE_NO", model.CASE_NO));
                    Parameter.Add(new CommandParameter("@COL_165CASE", model.COL_165CASE));
                    Parameter.Add(new CommandParameter("@COL_C1003CASE", model.COL_C1003CASE));
                    Parameter.Add(new CommandParameter("@COL_POLICE", model.COL_POLICE));
                    Parameter.Add(new CommandParameter("@COL_ACCOUNT2", model.COL_ACCOUNT2));
                    Parameter.Add(new CommandParameter("@Unit", model.Unit));
                    Parameter.Add(new CommandParameter("@CaseCreator", model.CaseCreator));
                    Parameter.Add(new CommandParameter("@EXT", model.EXT));
                    Parameter.Add(new CommandParameter("@COL_VICTIM", model.COL_VICTIM));
                    Parameter.Add(new CommandParameter("@COL_OTHERBANKID", model.COL_OTHERBANKID));
                    Parameter.Add(new CommandParameter("@Memo", model.Memo));
                    Parameter.Add(new CommandParameter("@CreatedDate", model.CreatedDate));
                    Parameter.Add(new CommandParameter("@ModifiedUser", model.ModifiedUser));
                    Parameter.Add(new CommandParameter("@NO", model.No));

                    int effactCount = ExecuteNonQuery(sql, dbTransaction);

                    #endregion

                    #region 附件
                    if (model.WarningFraudAttach != null)
                    {
                        model.WarningFraudAttach.WarningFraudNo = model.No;
                        effactCount += CreateAttatchment(model.WarningFraudAttach, dbTransaction);
                    }
                    #endregion

                    if (effactCount == 0)
                    {
                        dbTransaction.Rollback();
                    }
                    else
                    {
                        dbTransaction.Commit();
                        flag = true;
                    }
                }

                return flag;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 新增附件
        /// </summary>
        /// <param name="model"></param>
        /// <param name="trans"></param>
        /// <returns></returns>
        public int CreateAttatchment(WarningFraudAttach model, IDbTransaction trans = null)
        {
            string strSql = @" Insert Into WarningFraudAttach (WarningFraudNo, AttachmentName, AttachmentServerPath, AttachmentServerName, CreatedUser, CreatedDate) 
                               Values (@WarningFraudNo, @AttachmentName, @AttachmentServerPath, @AttachmentServerName, @CreatedUser,GETDATE());";

            base.Parameter.Clear();

            // 添加參數
            base.Parameter.Add(new CommandParameter("@WarningFraudNo", model.WarningFraudNo));
            base.Parameter.Add(new CommandParameter("@AttachmentName", model.AttachmentName));
            base.Parameter.Add(new CommandParameter("@AttachmentServerPath", model.AttachmentServerPath));
            base.Parameter.Add(new CommandParameter("@AttachmentServerName", model.AttachmentServerName));
            base.Parameter.Add(new CommandParameter("@CreatedUser", model.CreatedUser));
            return trans == null ? base.ExecuteNonQuery(strSql) : base.ExecuteNonQuery(strSql, trans);
        }

        /// <summary>
        /// 檢查 COL_165CASE 是否存在
        /// </summary>
        /// <param name="caseNo"></param>
        /// <returns>true:存在, false:不存在</returns>
        public bool Check_165CaseNo(string caseNo)
        {
            try
            {
                string sql = "Select Count(1) From WarningFraud Where COL_165CASE = @COL_165CASE";
                Parameter.Clear();
                Parameter.Add(new CommandParameter("@COL_165CASE", caseNo));
                var result = ExecuteScalar(sql);
                return (int)result > 0;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 刪除一筆聯防資料
        /// </summary>
        /// <param name="no"></param>
        /// <returns></returns>
        public bool DeleteWarningFraud(long no)
        {
            try
            {
                string sql = "Delete From WarningFraud Where NO = @NO";
                Parameter.Clear();
                Parameter.Add(new CommandParameter("@NO", no));
                var result = ExecuteNonQuery(sql);
                return (int)result > 0;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 取附件資料
        /// </summary>
        /// <param name="attachId"></param>
        /// <returns></returns>
        public WarningFraudAttach GetAttachInfo(int attachId)
        {
            try
            {
                string sql = @"Select AttachmentId, WarningFraudNo, AttachmentName, AttachmentServerPath, AttachmentServerName
                            From WarningFraudAttach Where AttachmentId = @AttachmentId";

                Parameter.Clear();
                Parameter.Add(new CommandParameter("@AttachmentId", attachId));
                IList<WarningFraudAttach> list = SearchList<WarningFraudAttach>(sql);
                return list.FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 刪除附件
        /// </summary>
        /// <param name="attachId"></param>
        /// <returns></returns>
        public bool DelAttach(int attachId)
        {
            try
            {
                string sql = "Delete From WarningFraudAttach Where AttachmentId = @AttachmentId";
                Parameter.Clear();
                Parameter.Add(new CommandParameter("@AttachmentId", attachId));
                int effactCount = ExecuteNonQuery(sql);
                return effactCount > 0;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 匯出Excel
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public MemoryStream Export_Excel(WarningFraud model)
        {
            var ms = new MemoryStream();
            string[] headerColumns = new[]
            {
                "序號", "鍵檔日期", "165案號", "被聯防帳號", "通報單位", "通報人員", "分機", "警局", "被害人", "工單編號", "銀行別", "e化案號", "備註"
            };

            var list = GetQueryList(model, false).ToList();

            if(list != null)
            {
                var row = 1;
                list.ForEach(m => 
                {
                    m.RowNum = row;
                    row++;
                });
                ms = ExcelExport(list, headerColumns, delegate(HSSFRow dataRow, WarningFraud dataItem)
                {
                    dataRow.CreateCell(0).SetCellValue(dataItem.RowNum);
                    dataRow.CreateCell(1).SetCellValue(UtlString.FormatDateTw(dataItem.CreatedDate));
                    dataRow.CreateCell(2).SetCellValue(dataItem.COL_165CASE);
                    dataRow.CreateCell(3).SetCellValue(dataItem.COL_ACCOUNT2);
                    dataRow.CreateCell(4).SetCellValue(dataItem.Unit);
                    dataRow.CreateCell(5).SetCellValue(dataItem.CaseCreator);
                    dataRow.CreateCell(6).SetCellValue(dataItem.EXT);
                    dataRow.CreateCell(7).SetCellValue(dataItem.COL_POLICE);
                    dataRow.CreateCell(8).SetCellValue(dataItem.COL_EVENTP);
                    dataRow.CreateCell(9).SetCellValue(dataItem.COL_C1003CASE);
                    dataRow.CreateCell(10).SetCellValue(dataItem.COL_OTHERBANKID);
                    dataRow.CreateCell(11).SetCellValue(dataItem.COL_C1003CASE);
                    dataRow.CreateCell(12).SetCellValue(dataItem.Memo);
                });
            }
            return ms;
        }

        /// <summary>
        /// 取得聯防案件資料
        /// </summary>
        /// <param name="no"></param>
        /// <returns></returns>
        public WarningFraud GetWarningFraud(long no)
        {
            try
            {
                string sql = @"SELECT w.[NO], w.COL_ID, w.CASE_NO, w.COL_C1003CASE, w.CaseCreator
                                  , w.COL_POLICE, w.COL_VICTIM, w.COL_OTHERBANKID, w.COL_165CASE, w.Unit
                                  , w.COL_ACCOUNT2, w.EXT, w.Memo, a.AttachmentId
                                  , CONVERT(varchar, w.CreatedDate, 111) AS CreatedDate
                                FROM WarningFraud AS w
						        LEFT JOIN WarningFraudAttach AS a ON w.NO = a.WarningFraudNo
						        WHERE [NO] = @NO";

                Parameter.Clear();
                Parameter.Add(new CommandParameter("@NO", no));
                IList<WarningFraud> list = SearchList<WarningFraud>(sql);
                return list.FirstOrDefault();
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }
    }
}
