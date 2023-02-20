using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CTBC.CSFS.BussinessLogic;
using CTBC.CSFS.Models;
using CTBC.FrameWork.Util;
using System.Data;
using System.Collections;
using System.Data.SqlClient;
using System.Configuration;
using System.IO;
using log4net;
using System.Security.Cryptography;
using System.Xml;

namespace CTBC.CSFS.BackupHistory
{
    //20211009..  因為底層 Command.TimeOut ...預設為900 (15分鍾).. 但目前此程式要搬移的資料量很大, 在PROD的環境中, 一個命令會超過15分鍾..
    // 為此程式, 仿製底層.. BaseBusinessRule .. 只修改TimeOut 的時間....
    public class HistoryDBBIZ : BBRule  // : BaseBusinessRule  .. 
    {

        public DataTable getHugeDate()
        {
            DataTable dt = new DataTable();
            using (IDbConnection dbConnection = OpenConnection())
            {
                WriteLog("測試資料, 開始時間: " + DateTime.Now.ToLongTimeString());
                WriteLog("Command TimeOut 設定: " + base.commandTimeOut.ToString() );

                string sqlStr = @"select * from MailInfo m left outer join casemaster c on m.caseid  = c.caseid ";                 
                dt = base.Search(sqlStr);

                WriteLog("測試資料, 結束時間: " + DateTime.Now.ToLongTimeString());      
            };

            return dt;
        }

        /// <summary>
        /// 找出可以被搬的案件..
        /// 1. 已支付或撒銷的案件
        /// 2. 扣押0元的 SeizureAmount=0 , 或是CaseSeizure.sum(SeizureAmount)==0, 或是無CaseSeizure
        /// 3. 扣押並支付的案子, 不會再有支付, 要當成扣押已完成 ... (若有撒銷, 也會在20天內撒銷完成.. 所以撒銷情況不考慮) ..
        /// 4. 外來文案件
        /// 5. 撒銷或支付, 找不到前案(沒有補建的.. 才會適用)
        /// </summary>
        /// <returns></returns>
        public List<CaseMoveLog> getMoveableCases(List<BackupSetting> tables, DateTime sDate, DateTime eDate)
        {
            //throw new NotImplementedException();
            List<CaseMoveLog> Result = new List<CaseMoveLog>();


            #region 案件類型1


            #endregion




            return Result;
        }


        /// <summary>
        /// 複製正式區的完整案件
        /// </summary>
        /// <param name="Caseid"></param>
        /// <param name="tables"></param>
        /// <returns></returns>
        public int copyCaseid(Guid Caseid, List<BackupSetting> tables)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 刪除正式區的完整案件
        /// </summary>
        /// <param name="Caseid"></param>
        /// <returns></returns>
        private int deleteCaseid(Guid Caseid, List<BackupSetting> tables)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// 刪除歷史區中, 已經搬過的案件
        /// </summary>
        /// <param name="tobeDel"></param>
        /// <returns></returns>
        public int deletHistoryCaseid(List<Guid> tobeDel)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 找出已支付或撒銷的案件
        /// </summary>
        /// <param name="sDate"></param>
        /// <param name="eDate"></param>
        /// <returns></returns>
        public List<CaseMoveLog> getCaseType1byCreateDate(DateTime sDate, DateTime eDate)
        {
            List<CaseMoveLog> caseIds = new List<CaseMoveLog>();
            using (SqlConnection con = OpenConnection())
            {
                string sqlStr = string.Empty;
                sqlStr = string.Format(@"SELECT DISTINCT m.CaseNo, s.CaseId, s.PayCaseId, s.CancelCaseId,'1' as MoveType, m.CreatedDate as CaseCreateDate FROM CaseSeizure s inner join  CaseMaster m on s.CancelCaseId=m.CaseId WHERE m.CreatedDate>='{0}' AND m.CreatedDate<'{1}' AND m.CaseKind='扣押案件' AND m.CaseKind2='撤銷' AND (m.Status='Z01' OR m.Status='Z03')",
                    sDate.ToString("yyyy-MM-dd"), eDate.AddDays(1).ToString("yyyy-MM-dd"));
                caseIds.AddRange(base.SearchList<CaseMoveLog>(sqlStr).ToList()); // 加入撒銷案

                sqlStr = string.Format(@"SELECT DISTINCT m.CaseNo, s.CaseId, s.PayCaseId, s.CancelCaseId,'1' as MoveType, m.CreatedDate as CaseCreateDate FROM CaseSeizure s inner join  CaseMaster m on s.PayCaseId=m.CaseId WHERE m.CreatedDate>='{0}' AND m.CreatedDate<'{1}' AND m.CaseKind='扣押案件' AND m.CaseKind2='支付' AND (m.Status='Z01' OR m.Status='Z03')",
                    sDate.ToString("yyyy-MM-dd"), eDate.AddDays(1).ToString("yyyy-MM-dd"));
                caseIds.AddRange(base.SearchList<CaseMoveLog>(sqlStr).ToList()); // 加入支付案
            }
            return caseIds;
        }

        /// <summary>
        /// 找出扣押0元的 SeizureAmount=0 , 或是CaseSeizure.sum(SeizureAmount)==0, 或是無CaseSeizure
        /// </summary>
        /// <param name="sDate"></param>
        /// <param name="eDate"></param>
        /// <returns></returns>
        public List<CaseMoveLog> getCaseType2byCreateDate(DateTime sDate, DateTime eDate)
        {
            List<CaseMoveLog> Result = new List<CaseMoveLog>();
            using (SqlConnection con = OpenConnection())
            {
                string sqlStr = string.Empty;
                // 扣押案件, 0元扣押
                sqlStr = string.Format(@"select  DISTINCT m.CaseNo, s.CaseId, s.PayCaseId, s.CancelCaseId,'2' as MoveType, m.CreatedDate as CaseCreateDate from CaseSeizure s inner join CaseMaster m on s.CaseId=m.CaseId where m.CreatedDate>='{0}' AND m.CreatedDate<'{1}'  AND m.CaseKind='扣押案件' AND m.CaseKind2='扣押' AND (m.Status='Z01' OR m.Status='Z03') group by s.CaseId  having sum(s.SeizureAmountNtd)=0 ",
                    sDate.ToString("yyyy-MM-dd"), eDate.AddDays(1).ToString("yyyy-MM-dd"));
                Result.AddRange(base.SearchList<CaseMoveLog>(sqlStr).ToList());

                // 無存款往來的
                sqlStr = string.Format(@"select  DISTINCT m.CaseNo, s.CaseId, s.PayCaseId, s.CancelCaseId,'2' as MoveType, m.CreatedDate as CaseCreateDate from CaseMaster m left join CaseSeizure s on m.CaseId=s.CaseId where m.CreatedDate>='{0}' AND m.CreatedDate<'{1}'  AND m.CaseKind='扣押案件' AND m.CaseKind2='扣押' AND (m.Status='Z01' OR m.Status='Z03') AND s.SeizureId is null",
                    sDate.ToString("yyyy-MM-dd"), eDate.AddDays(1).ToString("yyyy-MM-dd"));
                Result.AddRange(base.SearchList<CaseMoveLog>(sqlStr).ToList()); // 加入支付案
            }
            return Result;
        }

        /// <summary>
        /// 找出扣押並支付的案子, 不會再有支付, 要當成扣押已完成
        /// </summary>
        /// <param name="sDate"></param>
        /// <param name="eDate"></param>
        /// <returns></returns>
        public List<CaseMoveLog> getCaseType3byCreateDate(DateTime sDate, DateTime eDate)
        {
            List<CaseMoveLog> Result = new List<CaseMoveLog>();
            using (SqlConnection con = OpenConnection())
            {
                string sqlStr = string.Empty;
                // 找出扣押並支付案
                sqlStr = string.Format(@"SELECT DISTINCT m.CaseNo, s.CaseId, s.PayCaseId, s.CancelCaseId,'3' as MoveType, m.CreatedDate as CaseCreateDate   FROM CaseSeizure s inner join  CaseMaster m on s.CaseId=m.CaseId WHERE m.CreatedDate>='{0}' AND m.CreatedDate<'{1}' AND m.CaseKind='扣押案件' AND m.CaseKind2='扣押並支付' AND (m.Status='Z01' OR m.Status='Z03') ",
                    sDate.ToString("yyyy-MM-dd"), eDate.AddDays(1).ToString("yyyy-MM-dd"));
                Result.AddRange(base.SearchList<CaseMoveLog>(sqlStr).ToList());
            }
            return Result;
        }


        /// <summary>
        /// 外來文案件
        /// </summary>
        /// <param name="sDate"></param>
        /// <param name="eDate"></param>
        /// <returns></returns>
        public List<CaseMoveLog> getCaseType4byCreateDate(DateTime sDate, DateTime eDate)
        {
            List<CaseMoveLog> Result = new List<CaseMoveLog>();
            using (SqlConnection con = OpenConnection())
            {
                string sqlStr = string.Empty;
                // 找出扣押並支付案
                sqlStr = string.Format(@"SELECT DISTINCT m.CaseNo, m.CaseId, null as PayCaseId, null CancelCaseId,'4' as MoveType, m.CreatedDate as CaseCreateDate   FROM  CaseMaster m WHERE m.CreatedDate>='{0}' AND m.CreatedDate<'{1}' AND m.CaseKind='外來文案件'  AND (m.Status='Z01' OR m.Status='Z03')",
                    sDate.ToString("yyyy-MM-dd"), eDate.AddDays(1).ToString("yyyy-MM-dd"));
                Result.AddRange(base.SearchList<CaseMoveLog>(sqlStr).ToList());
            }
            return Result;
        }














        // 全取, 並要檢查是否每一個GroupNo 的設定是一致的..
        // 只回傳相同的GroupNo 的設定回去...
        public List<BackupSetting> getHistory_BackupSetting()
        {
            List<BackupSetting> result = new List<BackupSetting>();
            List<BackupSetting> resultTemp = new List<BackupSetting>();
            using (IDbConnection dbConnection = OpenConnection())
            {
                string sqlStr = @"SELECT * FROM [dbo].[History_BackupSetting]";

                resultTemp = base.SearchList<BackupSetting>(sqlStr).ToList();

            };

            var lstGroupNo = resultTemp.GroupBy(x => x.GroupNo).Select(x => x.Key);
            foreach (var gno in lstGroupNo)
            {
                if (gno == 0) // 如果是0是, 表示所有表格的動作是單獨行動, 只要考慮Enable,即可
                {
                    var lst = resultTemp.Where(x => x.GroupNo == gno && x.Enable).ToList(); // 檢查是否為啟動, 若有啟動, 才會加入回傳
                    lst.ForEach(x => { result.Add(x); });
                }
                else
                {
                    var GroupCount = resultTemp.Where(x => x.GroupNo == gno).GroupBy(x => new { x.Enable, x.isDelete, x.TimeFreq, x.TimeFreqValue, x.LastBackupDate }).Count();
                    if (GroupCount == 1) // 表示同一個GroupNo 只有一致的參數
                    {
                        WriteLog(string.Format("\t參數設定檔, 相同的GroupNo={0}, 的參數設定一致!!", gno.ToString()));
                        var lst = resultTemp.Where(x => x.GroupNo == gno && x.Enable).ToList(); // 檢查是否為啟動, 若有啟動, 才會加入回傳
                        lst.ForEach(x => { result.Add(x); });
                    }
                    else
                    {
                        WriteLog(string.Format("\t參數設定檔, 相同的GroupNo={0}, 的參數設定不一致", gno.ToString()));
                    }
                }
            }
            return result.OrderBy(x => lstGroupNo).ThenBy(y => y.SortOrder).ToList();
        }

        /// <summary>
        /// 檢查區間是否已有資料....
        /// </summary>
        /// <param name="sDate"></param>
        /// <param name="eDate"></param>
        /// <param name="dest"></param>
        /// <param name="dateField"></param>
        /// <returns></returns>
        public int CheckDataExit(DateTime sDate, DateTime eDate, string dest, string dateField, List<Guid> CaseIds)
        {
            int Count = 0;
            using (IDbConnection dbConnection = OpenConnection())
            {
                string sqlStr = "";
                string whereCaseIds = "";
                if (CaseIds.Count() > 0)
                {
                    whereCaseIds = "  CaseId in ('" + string.Join("','", CaseIds) + "')";
                    sqlStr = string.Format("Select Count(*) from {0} where {1} ", dest, whereCaseIds);
                }
                else
                {
                    sqlStr = string.Format("Select Count(*) from {0} where {1}>='{2}' AND {1}<'{3}'", dest, dateField, sDate.ToString("yyyy-MM-dd"), eDate.AddDays(1).ToString("yyyy-MM-dd"));
                }
                var dt = base.Search(sqlStr);
                Count = int.Parse(dt.Rows[0][0].ToString());
            };
            return Count;
        }

        public int CheckDataExit_CloseDate(DateTime sDate, DateTime eDate, string dest, string dateField, List<Guid> CaseIds)
        {


            int Count = 0;
            using (IDbConnection dbConnection = OpenConnection())
            {
                string sqlStr = string.Empty;
                string whereCaseIds = "";
                if (dest.ToUpper().Contains("CASEMASTER"))
                {
                    if (CaseIds.Count() > 0)
                        whereCaseIds = " CaseId in ('" + string.Join("','", CaseIds) + "')";
                    else
                        whereCaseIds = " {1}>='{2}' AND {1}<'{3}'";
                    sqlStr = string.Format("Select Count(*) from {0} where " + whereCaseIds, dest, dateField, sDate.ToString("yyyy-MM-dd"), eDate.AddDays(1).ToString("yyyy-MM-dd"));
                }
                else
                {
                    if (CaseIds.Count() > 0)
                        whereCaseIds = "  h.CaseId in ('" + string.Join("','", CaseIds) + "')";
                    else
                        whereCaseIds = " {1}>='{2}' AND {1}<'{3}'";
                    sqlStr = string.Format("Select Count(*) from {0} h inner join CaseMaster cm on h.caseid =cm.CaseId  where " + whereCaseIds, dest, dateField, sDate.ToString("yyyy-MM-dd"), eDate.AddDays(1).ToString("yyyy-MM-dd"));
                }
                var dt = base.Search(sqlStr);
                Count = int.Parse(dt.Rows[0][0].ToString());
            };
            return Count;
        }



        public int DeleteData(DateTime sDate, DateTime eDate, string dest, string dateField, List<Guid> CaseIds)
        {
            IDbConnection dbConnection = OpenConnection();
            //WriteLog(string.Format("  嘗試砍掉表格 {0} 重覆的資料", dest));
            string sqlStr = "";
            string whereCaseIds = "";
            if (CaseIds.Count() > 0)
            {
                whereCaseIds = "  CaseId in ('" + string.Join("','", CaseIds) + "')";
                sqlStr = string.Format("delete from {0} where " + whereCaseIds, dest);
            }
            else
                sqlStr = string.Format("delete from {0} where {1}>='{2}' AND {1}<'{3}'" + whereCaseIds, dest, dateField, sDate.ToString("yyyy-MM-dd"), eDate.AddDays(1).ToString("yyyy-MM-dd"));

            var trans = dbConnection.BeginTransaction();
            try
            {
                //sqlStr = string.Format("delete from {0} where {1}>='{2}' AND {1}<'{3}'" + whereCaseIds, dest, dateField, sDate.ToString("yyyy-MM-dd"), eDate.AddDays(1).ToString("yyyy-MM-dd"));
                base.ExecuteNonQuery(sqlStr, trans);
                trans.Commit();
                //WriteLog(string.Format("  表格 {0} 重覆的資料已刪除", dest));
            }
            catch (Exception ex)
            {
                trans.Rollback();
                WriteLog(string.Format("  刪除重覆資料有錯，已恢復. 錯誤原因: {0}", ex.Message));
                //Console.WriteLine("Commit Exception Type: {0}", ex.GetType());
                //Console.WriteLine("  Message: {0}", ex.Message);   
                return 0;
            }

            return 1;
        }

        public int DeleteData_CloseDate(DateTime sDate, DateTime eDate, string dest, string dateField, List<Guid> CaseIds)
        {
            IDbConnection dbConnection = OpenConnection();
            //WriteLog(string.Format("  嘗試砍掉表格 {0} 重覆的資料", dest));
            var trans = dbConnection.BeginTransaction();
            try
            {
                //string sqlStr = string.Format("delete from {0} h inner join CaseMaster cm on h.caseid =cm.CaseId  where {1}>='{2}' AND {1}<'{3}'", dest, dateField, sDate.ToString("yyyy-MM-dd"), eDate.AddDays(1).ToString("yyyy-MM-dd"));
                string sqlStr = string.Empty;
                string whereCaseIds = "";
                if (dest.ToUpper().Contains("CASEMASTER"))
                {
                    if (CaseIds.Count() > 0)
                        whereCaseIds = "  CaseId in ('" + string.Join("','", CaseIds) + "')";
                    else
                        whereCaseIds = " {1}>='{2}' AND {1}<'{3}'";
                    sqlStr = string.Format("delete from {0} where " + whereCaseIds, dest, dateField, sDate.ToString("yyyy-MM-dd"), eDate.AddDays(1).ToString("yyyy-MM-dd"));
                }
                else
                {
                    if (CaseIds.Count() > 0)
                    {
                        whereCaseIds = "  CaseId in ('" + string.Join("','", CaseIds) + "')";
                        sqlStr = string.Format("delete from {0} where " + whereCaseIds, dest);
                    }
                    else
                    {
                        sqlStr = string.Format("delete from {0} where CaseId in (select CaseId from CaseMaster where {1}>='{2}' AND {1}<'{3}')", dest, dateField, sDate.ToString("yyyy-MM-dd"), eDate.AddDays(1).ToString("yyyy-MM-dd"));
                    }
                }




                base.ExecuteNonQuery(sqlStr, trans);
                trans.Commit();
                //WriteLog(string.Format("  表格 {0} 重覆的資料已刪除", dest));
            }
            catch (Exception ex)
            {
                trans.Rollback();
                WriteLog(string.Format("  刪除重覆資料有錯，已恢復. 錯誤原因: {0}", ex.Message));
                //Console.WriteLine("Commit Exception Type: {0}", ex.GetType());
                //Console.WriteLine("  Message: {0}", ex.Message);   
                return 0;
            }

            return 1;
        }


        public int DeleteData_TX60491(DateTime sDate, DateTime eDate, string dest, string dateField, List<Guid> CaseIds)
        {
            IDbConnection dbConnection = OpenConnection();
            //WriteLog(string.Format("  嘗試砍掉表格 {0} 重覆的資料", dest));
            var trans = dbConnection.BeginTransaction();
            try
            {
                string sqlStr = string.Empty;
                string whereCaseIds = "";
                if (CaseIds.Count() > 0)
                    whereCaseIds = "  CaseId in ('" + string.Join("','", CaseIds) + "')";
                else
                    whereCaseIds = string.Format("cCretDT>='{0}' and cCretDT<'{1}' ", sDate.ToString("yyyy-MM-dd"), eDate.AddDays(1).ToString("yyyy-MM-dd"));

                if (dest.StartsWith("History"))
                {
                    //sqlStr = string.Format("delete  from History_TX_60491_Detl where FKSNO in (select SNO from History_TX_60491_Grp where cCretDT>='{0}' and cCretDT<'{1}');;delete from History_TX_60491_Grp where cCretDT>='{0}' and cCretDT<'{1}';", sDate.ToString("yyyy-MM-dd"), eDate.AddDays(1).ToString("yyyy-MM-dd"));
                    var SQL1 = string.Format("delete  from History_TX_60491_Detl where FKSNO in (select SNO from History_TX_60491_Grp where {0})", whereCaseIds);
                    var SQL2 = string.Format(";delete from History_TX_60491_Grp where {0};", whereCaseIds);
                    sqlStr = SQL1 + SQL2;
                }
                else
                {
                    //sqlStr = string.Format("delete  from TX_60491_Detl where FKSNO in (select SNO from TX_60491_Grp where cCretDT>='{0}' and cCretDT<'{1}');;delete from TX_60491_Grp where cCretDT>='{0}' and cCretDT<'{1}';", sDate.ToString("yyyy-MM-dd"), eDate.AddDays(1).ToString("yyyy-MM-dd"));
                    var SQL1 = string.Format("delete  from TX_60491_Detl where FKSNO in (select SNO from TX_60491_Grp where {0})", whereCaseIds);
                    var SQL2 = string.Format(";delete from TX_60491_Grp where {0}", whereCaseIds);
                    sqlStr = SQL1 + SQL2;
                }

                //string sqlStr = string.Format("delete from {0} where {1}>='{2}' AND {1}<'{3}'", dest, dateField, sDate.ToString("yyyy-MM-dd"), eDate.AddDays(1).ToString("yyyy-MM-dd"));
                base.ExecuteNonQuery(sqlStr, trans);
                trans.Commit();
                //WriteLog(string.Format("  表格 {0} 重覆的資料已刪除", dest));
            }
            catch (Exception ex)
            {
                trans.Rollback();
                WriteLog(string.Format("  刪除重覆資料有錯，已恢復. 錯誤原因: {0}", ex.Message));
                //Console.WriteLine("Commit Exception Type: {0}", ex.GetType());
                //Console.WriteLine("  Message: {0}", ex.Message);   
                return 0;
            }

            return 1;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sDate">開始時間</param>
        /// <param name="eDate">結束時間</param>
        /// <param name="source">來源TableName</param>
        /// <param name="dest">目標TableName</param>
        /// <returns></returns>
        public int BulkInsertion(DateTime sDate, DateTime eDate, string source, string dest, string dateField, List<string> ignoreFields, List<Guid> CaseIds)
        {
            //IDbTransaction dbTrans = null;
            string whereCaseIds = "";
            if (CaseIds.Count() > 0)
                whereCaseIds = "  CaseId in ('" + string.Join("','", CaseIds) + "')";
            else
                whereCaseIds = " {1}>='{2}' AND {1}<'{3}'";
            using (SqlConnection con = OpenConnection())
            {
                //con.Open();
                string sqlStr = string.Format("Select * from {0} where " + whereCaseIds, source, dateField, sDate.ToString("yyyy-MM-dd"), eDate.AddDays(1).ToString("yyyy-MM-dd"));
                var dt = base.Search(sqlStr);


                string colStr = string.Format(@"SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = N'{0}'", source);
                var dtColAll = base.Search(colStr);


                foreach (var reField in ignoreFields)
                {
                    if (!string.IsNullOrEmpty(reField))
                        dt.Columns.Remove(reField);
                }

                //dbTrans = con.BeginTransaction();
                try
                {

                    using (SqlBulkCopy bulkCopy = new SqlBulkCopy(con))
                    {
                        bulkCopy.DestinationTableName = dest;
                        bulkCopy.BulkCopyTimeout = 0;
                        bulkCopy.BatchSize = 1000;
                        for (int i = 0; i < dtColAll.Rows.Count; i++)
                        {
                            var colName = dtColAll.Rows[i]["COLUMN_NAME"].ToString();
                            if (!string.IsNullOrEmpty(colName) && !ignoreFields.Contains(colName))
                            {
                                bulkCopy.ColumnMappings.Add(dtColAll.Rows[i]["COLUMN_NAME"].ToString(), dtColAll.Rows[i]["COLUMN_NAME"].ToString());
                            }
                        }
                        //bulkCopy.ColumnMappings(
                        bulkCopy.WriteToServer(dt);
                    }
                }
                catch (Exception ex)
                {
                    //dbTrans.Rollback();
                    WriteLog(string.Format("\t\t\t\tBulkinsertion 發生錯誤!! 錯誤原因 {0}", ex.Message.ToString()));
                    return 0;
                }
                //dbTrans.Commit();
                WriteLog(string.Format("\t\t\t\t共計備份出{0}筆資料", dt.Rows.Count.ToString()));
            }

            return 1;
        }



        public int BulkInsertion_CloseDate(DateTime sDate, DateTime eDate, string source, string dest, string dateField, List<string> ignoreFields, List<Guid> CaseIds)
        {
            //IDbTransaction dbTrans = null;
            using (SqlConnection con = OpenConnection())
            {
                //con.Open();
                string sqlStr = string.Empty;
                string whereCaseIds = "";
                if (dest.ToUpper().Contains("CASEMASTER"))
                {
                    if (CaseIds.Count() > 0)
                        whereCaseIds = " CaseId in ('" + string.Join("','", CaseIds) + "')";
                    else
                        whereCaseIds = " {1}>='{2}' AND {1}<'{3}' AND (STATUS='Z01' OR STATUS='Z03')";
                    sqlStr = string.Format("Select * from {0}  where  " + whereCaseIds, source, dateField, sDate.ToString("yyyy-MM-dd"), eDate.AddDays(1).ToString("yyyy-MM-dd"));
                }
                else
                {
                    if (CaseIds.Count() > 0)
                        whereCaseIds = "  h.CaseId in ('" + string.Join("','", CaseIds) + "')";
                    else
                        whereCaseIds = " {1}>='{2}' AND {1}<'{3}'";
                    sqlStr = string.Format("Select h.* from {0} h inner join CaseMaster cm on h.caseid =cm.CaseId where " + whereCaseIds, source, dateField, sDate.ToString("yyyy-MM-dd"), eDate.AddDays(1).ToString("yyyy-MM-dd"));
                }

                var dt = base.Search(sqlStr);

                //               string pkStr = string.Format( @"
                //               SELECT Col.Column_Name from INFORMATION_SCHEMA.TABLE_CONSTRAINTS Tab,     INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE Col 
                //                   WHERE Col.Constraint_Name = Tab.Constraint_Name AND Col.Table_Name = Tab.Table_Name AND Constraint_Type = 'PRIMARY KEY' AND Col.Table_Name = '{0}'", source );
                //               var dtCol = base.Search(pkStr);
                //               string pkCols = dtCol.Rows[0][0].ToString();

                string colStr = string.Format(@"SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = N'{0}'", source);
                var dtColAll = base.Search(colStr);


                foreach (var reField in ignoreFields)
                {
                    if (!string.IsNullOrEmpty(reField))
                        dt.Columns.Remove(reField);
                }

                //dbTrans = con.BeginTransaction();
                try
                {
                    using (SqlBulkCopy bulkCopy = new SqlBulkCopy(con))
                    {
                        bulkCopy.DestinationTableName = dest;
                        bulkCopy.BulkCopyTimeout = 0;
                        bulkCopy.BatchSize = 1000;
                        for (int i = 0; i < dtColAll.Rows.Count; i++)
                        {
                            var colName = dtColAll.Rows[i]["COLUMN_NAME"].ToString();
                            if (!string.IsNullOrEmpty(colName) && !ignoreFields.Contains(colName))
                            {
                                bulkCopy.ColumnMappings.Add(dtColAll.Rows[i]["COLUMN_NAME"].ToString(), dtColAll.Rows[i]["COLUMN_NAME"].ToString());
                            }
                        }
                        bulkCopy.WriteToServer(dt);
                    }
                }
                catch (Exception ex)
                {
                    //dbTrans.Rollback();
                    WriteLog(string.Format("\t\t\t\tBulkinsertion 發生錯誤!! 錯誤原因 {0}", ex.Message.ToString()));
                    return 0;
                }
                //dbTrans.Commit();
                WriteLog(string.Format("\t\t\t\t共計備份出{0}筆資料", dt.Rows.Count.ToString()));
            }

            return 1;
        }



        public int BulkInsertion_TX60491(DateTime sDate, DateTime eDate, string source, string dest, string dateField, List<string> ignoreFields)
        {
            using (SqlConnection con = OpenConnection())
            {
                //con.Open();
                string sqlStr = string.Format("Select * from {0} where {1}>='{2}' AND {1}<'{3}'", source, dateField, sDate.ToString("yyyy-MM-dd"), eDate.AddDays(1).ToString("yyyy-MM-dd"));
                var dt = base.Search(sqlStr);

                //               string pkStr = string.Format( @"
                //               SELECT Col.Column_Name from INFORMATION_SCHEMA.TABLE_CONSTRAINTS Tab,     INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE Col 
                //                   WHERE Col.Constraint_Name = Tab.Constraint_Name AND Col.Table_Name = Tab.Table_Name AND Constraint_Type = 'PRIMARY KEY' AND Col.Table_Name = '{0}'", source );
                //               var dtCol = base.Search(pkStr);
                //               string pkCols = dtCol.Rows[0][0].ToString();

                string colStr = string.Format(@"SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = N'{0}'", source);
                var dtColAll = base.Search(colStr);


                foreach (var reField in ignoreFields)
                {
                    if (!string.IsNullOrEmpty(reField))
                        dt.Columns.Remove(reField);
                }

                try
                {
                    using (SqlBulkCopy bulkCopy = new SqlBulkCopy(con))
                    {
                        bulkCopy.DestinationTableName = dest;
                        for (int i = 0; i < dtColAll.Rows.Count; i++)
                        {
                            var colName = dtColAll.Rows[i]["COLUMN_NAME"].ToString();
                            if (!string.IsNullOrEmpty(colName) && !ignoreFields.Contains(colName))
                            {
                                bulkCopy.ColumnMappings.Add(dtColAll.Rows[i]["COLUMN_NAME"].ToString(), dtColAll.Rows[i]["COLUMN_NAME"].ToString());
                            }
                        }
                        //bulkCopy.ColumnMappings(
                        bulkCopy.WriteToServer(dt);
                    }
                }
                catch (Exception ex)
                {

                    WriteLog(string.Format("\t\t\t\tBulkinsertion 發生錯誤!! 錯誤原因 {0}", ex.Message.ToString()));
                    return 0;
                }
                WriteLog(string.Format("\t\t\t\t共計TX60491_Grp 備份出{0}筆資料", dt.Rows.Count.ToString()));


                List<string> sNos = new List<string>();
                foreach (DataRow dr in dt.Rows)
                {
                    sNos.Add(dr["SNO"].ToString());
                }

                string whereStr = "'" + string.Join("','", sNos) + "'";

                string sql2Str = string.Format("select * from TX_60491_Detl where FKSNO in ({0})", whereStr);
                var dt2 = base.Search(sql2Str);

                string col2Str = string.Format(@"SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = N'{0}'", "TX_60491_Detl");
                var dt2ColAll = base.Search(col2Str);


                try
                {
                    using (SqlBulkCopy bulkCopy = new SqlBulkCopy(con))
                    {
                        bulkCopy.DestinationTableName = "History_TX_60491_Detl";
                        for (int i = 0; i < dt2ColAll.Rows.Count; i++)
                        {
                            var colName = dt2ColAll.Rows[i]["COLUMN_NAME"].ToString();
                            //if (!string.IsNullOrEmpty(colName) && !ignoreFields.Contains(colName))
                            {
                                bulkCopy.ColumnMappings.Add(dt2ColAll.Rows[i]["COLUMN_NAME"].ToString(), dt2ColAll.Rows[i]["COLUMN_NAME"].ToString());
                            }
                        }
                        //bulkCopy.ColumnMappings(
                        bulkCopy.WriteToServer(dt2);
                    }
                }
                catch (Exception ex)
                {

                    WriteLog(string.Format("\t\t\t\tBulkinsertion 發生錯誤!! 錯誤原因 {0}", ex.Message.ToString()));
                    return 0;
                }
                WriteLog(string.Format("\t\t\t\t共計TX60491_Detl 備份出{0}筆資料", dt2.Rows.Count.ToString()));


            }

            return 1;
        }

        /// <summary>
        /// 20201028, 發現用Bulkinsert 會出錯, 但找不到那一筆的問題, 改成一筆一筆Insert ...
        /// </summary>
        /// <param name="sDate"></param>
        /// <param name="eDate"></param>
        /// <param name="source"></param>
        /// <param name="dest"></param>
        /// <param name="dateField"></param>
        /// <param name="ignoreFields"></param>
        /// <returns></returns>
        public int BulkInsertion_TX60491_New(DateTime sDate, DateTime eDate, string source, string dest, string dateField, List<string> ignoreFields, List<Guid> CaseIds)
        {
            IDbTransaction dbTrans = null;
            string whereCaseIds = "";
            if (CaseIds.Count() > 0)
                whereCaseIds = "  CaseId in ('" + string.Join("','", CaseIds) + "')";
            else
                whereCaseIds = " {1}>='{2}' AND {1}<'{3}'";

            using (SqlConnection con = OpenConnection())
            {
                //con.Open();
                string sqlStr = string.Format("Select DISTINCT SNO from {0} where " + whereCaseIds, source, dateField, sDate.ToString("yyyy-MM-dd"), eDate.AddDays(1).ToString("yyyy-MM-dd"));
                var dt = base.Search(sqlStr);

                dbTrans = con.BeginTransaction();
                List<string> successSNO = new List<string>();
                List<string> errorSNO = new List<string>();
                try
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        try
                        {
                            string sqlStr1 = string.Format("INSERT INTO History_TX_60491_Grp SELECT * FROM TX_60491_Grp WHERE SNO='{0}'", dr["SNO"].ToString());
                            base.ExecuteNonQuery(sqlStr1);
                            successSNO.Add(dr["SNO"].ToString());
                        }
                        catch (Exception ex2)
                        {
                            errorSNO.Add(dr["SNO"].ToString());
                            WriteLog(string.Format("\t\t\t\tBulkinsertion 發生錯誤!! SNO={0} 錯誤原因 {1}", dr["SNO"].ToString(), ex2.Message.ToString()));
                        }
                    }

                }
                catch (Exception ex)
                {
                    dbTrans.Rollback();
                    WriteLog(string.Format("\t\t\t\tInsertion TX_60491_Grp 發生錯誤!! 錯誤原因 {0}", ex.Message.ToString()));
                    return 0;
                }
                dbTrans.Commit();

                if (successSNO.Count() > 0)
                    WriteLog(string.Format("\t\t\t\t共計TX60491_Grp 成功備份出{0}筆資料", successSNO.Count().ToString()));
                if (errorSNO.Count() > 0)
                    WriteLog(string.Format("\t\t\t\t共計TX60491_Grp 錯誤的筆數, 共{0}筆", errorSNO.Count().ToString()));
                WriteLog("");


                List<string> successDelt = new List<string>();
                List<string> failDetl = new List<string>();

                dbTrans = con.BeginTransaction();
                try
                {

                    foreach (var sno in successSNO)
                    {
                        try
                        {
                            string sqlStr1 = string.Format("INSERT INTO History_TX_60491_Detl SELECT * FROM TX_60491_Detl WHERE FKSNO='{0}'", sno);
                            base.ExecuteNonQuery(sqlStr1);
                            successDelt.Add(sno);
                        }
                        catch (Exception ex2)
                        {
                            failDetl.Add(sno);
                            WriteLog(string.Format("\t\t\t\tInsertion TX_60491_Detl 發生錯誤!! FKSNO={0} 錯誤原因 {1}", sno, ex2.Message.ToString()));
                        }
                    }

                }
                catch (Exception ex)
                {
                    dbTrans.Rollback();
                    WriteLog(string.Format("\t\t\t\tInsertion TX60491_Detl 發生錯誤!! 錯誤原因 {0}", ex.Message.ToString()));
                    return 0;
                }
                dbTrans.Commit();

                // 輸出成功筆數
                if (successDelt.Count() > 0)
                {
                    StringBuilder sb = new StringBuilder();
                    successDelt.ForEach(x => sb.Append(x + ","));
                    string sqlCount = string.Format(@"SELECT Count(*) FROM History_TX_60491_Detl where FKSNO in ({0})", sb.ToString().TrimEnd(','));
                    var dtCount = base.Search(sqlCount);
                    if (dtCount.Rows.Count > 0)
                    {
                        WriteLog(string.Format("\t\t\t\t共計TX60491_Detl 成功備份出{0}筆資料", dtCount.Rows[0][0].ToString()));
                    }
                }

                //輸出失敗筆數
                if (failDetl.Count() > 0)
                {
                    StringBuilder sb1 = new StringBuilder();
                    failDetl.ForEach(x => sb1.Append(x + ","));
                    string sqlCount1 = string.Format(@"SELECT Count(*) FROM TX_60491_Detl where FKSNO in ({0})", sb1.ToString().TrimEnd(','));
                    var dtCount1 = base.Search(sqlCount1);
                    if (dtCount1.Rows.Count > 0)
                    {
                        WriteLog(string.Format("\t\t\t\t共計TX60491_Detl 錯誤備份出{0}筆資料", dtCount1.Rows[0][0].ToString()));
                    }
                }
            }

            return 1;
        }


        /// <summary>
        /// 更新歷史轉檔記錄
        /// </summary>
        /// <param name="TableName"></param>
        /// <param name="SendStatus"></param>
        /// <param name="Applno"></param>
        /// <param name="ApplnoB"></param>
        /// <returns></returns>


        public string InsertToTable(IDbTransaction trans, string _sourceTable, string _targetTable, string _year)
        {
            IDbConnection dbConnection = OpenConnection();
            bool rtn = true;
            try
            {
                string sqlStr = "";
                trans = dbConnection.BeginTransaction();
                switch (_sourceTable)
                {
                    case "CaseNoChangeHistory":
                        sqlStr = @" SET IDENTITY_INSERT History_CaseNoChangeHistory OFF ";
                        sqlStr = sqlStr + @"delete " + _targetTable + " where Year(CreatedDate) = " + _year + " insert into " + _targetTable + " ( CaseId, OldCaseNo, NewCaseNo, CreatedUser, CreatedDate) Select  CaseId, OldCaseNo, NewCaseNo, CreatedUser, CreatedDate from " + _sourceTable + "  where   Year(CreatedDate) = " + _year;
                        break;
                    case "CaseObligor":
                        sqlStr = @" SET IDENTITY_INSERT History_CaseObligor OFF ";
                        sqlStr = sqlStr + @"delete " + _targetTable + " where Year(CreatedDate) = " + _year + " insert into " + _targetTable + " (  CaseId, ObligorName, ObligorNo, ObligorAccount, CreatedUser, CreatedDate) Select   CaseId, ObligorName, ObligorNo, ObligorAccount, CreatedUser, CreatedDate from " + _sourceTable + "  where   Year(CreatedDate) = " + _year;
                        break;
                    case "CaseSeizure":
                        sqlStr = @" SET IDENTITY_INSERT History_CaseSeizure OFF ";
                        sqlStr = sqlStr + @"delete " + _targetTable + " where Year(CreatedDate) = " + _year + " insert into " + _targetTable + " ( CaseId, PayCaseId, CaseNo, CustId, CustName, BranchNo, BranchName, Account, AccountStatus, Currency, Balance, SeizureAmount, ExchangeRate, SeizureAmountNtd, PayAmount, SeizureStatus, CreatedUser, CreatedDate, ModifiedUser, ModifiedDate, CancelCaseId, ProdCode, Link, SegmentCode, CancelAmount, TripAmount, OtherSeizure, Seq, SeizureAmountSUB, TxtStatus, TxtProdCode) Select  CaseId, PayCaseId, CaseNo, CustId, CustName, BranchNo, BranchName, Account, AccountStatus, Currency, Balance, SeizureAmount, ExchangeRate, SeizureAmountNtd, PayAmount, SeizureStatus, CreatedUser, CreatedDate, ModifiedUser, ModifiedDate, CancelCaseId, ProdCode, Link, SegmentCode, CancelAmount, TripAmount, OtherSeizure, Seq, SeizureAmountSUB, TxtStatus, TxtProdCode from " + _sourceTable + "  where   Year(CreatedDate) = " + _year;
                        break;
                    case "CaseHistory":
                        sqlStr = @" SET IDENTITY_INSERT History_CaseHistory OFF ";
                        sqlStr = sqlStr + @"delete " + _targetTable + " where Year(CreatedDate) = " + _year + " insert into " + _targetTable + " ( CaseId, FromRole, FromUser, FromFolder, Event, EventTime, ToRole, ToUser, ToFolder, CreatedUser, CreatedDate) Select  CaseId, FromRole, FromUser, FromFolder, Event, EventTime, ToRole, ToUser, ToFolder, CreatedUser, CreatedDate from " + _sourceTable + "  where   Year(CreatedDate) = " + _year;
                        break;
                    case "CaseDataLog":
                        //sqlStr = @" SET IDENTITY_INSERT History_CaseDataLog OFF ";
                        sqlStr = @"delete " + _targetTable + " where Year(TXDateTime) = " + _year + " insert into " + _targetTable + "  Select * from " + _sourceTable + "  where   Year(TXDateTime) = " + _year;
                        break;
                    case "CaseNoTable":
                        sqlStr = @"delete " + _targetTable + " where Year(CaseDate) = " + _year + " insert into " + _targetTable + " Select * from " + _sourceTable + "  where   Year(CaseDate) = " + _year;
                        break;
                    case "CaseEdocFile":
                        sqlStr = @"delete  History_CaseEdocFile where CaseId in
                                   (select t.CaseID  from History_CaseEdocFile t
                                  inner join  History_CaseMaster m   on t.CaseId = m.CaseId
                                  where  Year(m.CloseDate) = " + _year + ")";
                        sqlStr = sqlStr + @" insert into  History_CaseEdocFile Select * from CaseEdocFile where CaseId in
                                   (select t.CaseID  from CaseEdocFile t
                                  inner join  CaseMaster m   on t.CaseId = m.CaseId
                                  where   Year(m.CloseDate) = " + _year + ")";
                        break;
                    case "CaseSendSetting":
                        sqlStr = @" SET IDENTITY_INSERT History_CaseSendSetting OFF 
                                   delete  History_CaseSendSetting where CaseId in
                                   (select t.CaseID from History_CaseSendSetting t
                                  inner join  History_CaseMaster m   on t.CaseId = m.CaseId
                                  where m.status='Z01' and Year(m.CloseDate) = " + _year + ")";
                        sqlStr = sqlStr + @" insert into  History_CaseSendSetting (CaseId, Template, SendWord, SendNo, SendDate, Speed, Security, Subject, Description, isFinish, FinishDate, Attachment, CreatedUser, CreatedDate, ModifiedUser, ModifiedDate, SendKind, SendUpDate) Select  CaseId, Template, SendWord, SendNo, SendDate, Speed, Security, Subject, Description, isFinish, FinishDate, Attachment, CreatedUser, CreatedDate, ModifiedUser, ModifiedDate, SendKind, SendUpDate from CaseSendSetting where CaseId in
                                   (select t.CaseID  from CaseSendSetting t
                                  inner join  CaseMaster m   on t.CaseId = m.CaseId
                                  where m.status='Z01' and Year(m.CloseDate) = " + _year + ")";
                        break;
                    case "CaseSendSettingDetails":
                        sqlStr = @" SET IDENTITY_INSERT History_CaseSendSettingDetails OFF  delete  History_CaseSendSettingDetails where CaseId in
                                   (select t.CaseID  from  History_CaseSendSettingDetails t
                                  inner join  History_CaseMaster m   on t.CaseId = m.CaseId
                                  where  Year(m.CloseDate) = " + _year + ")";
                        sqlStr = sqlStr + @" insert into  History_CaseSendSettingDetails (CaseId, SerialID, SendType, GovName, GovAddr) Select  CaseId, SerialID, SendType, GovName, GovAddr from CaseSendSettingDetails where CaseId in
                                   (select t.CaseID  from  CaseSendSettingDetails t
                                  inner join  CaseMaster m   on t.CaseId = m.CaseId
                                  where  Year(m.CloseDate) = " + _year + ")";
                        break;
                    case "TX_60491_Detl":
                        sqlStr = @"delete  History_TX_60491_Detl where CaseId in
                                   (select t.CaseID  from History_TX_60491_Detl t
                                  inner join  History_CaseMaster m   on t.CaseId = m.CaseId
                                  where  Year(m.CloseDate) = " + _year + ")";
                        sqlStr = sqlStr + @" insert into  History_TX_60491_Detl Select * from TX_60491_Detl where CaseId in
                                   (select t.CaseID  from TX_60491_Detl t
                                  inner join  CaseMaster m   on t.CaseId = m.CaseId
                                  where  Year(m.CloseDate) = " + _year + ")";
                        break;
                    case "TX_60491_Grp":
                        sqlStr = @"delete  History_TX_60491_Grp where CaseId in
                                   (select t.CaseID  from History_TX_60491_Grp t
                                  inner join  History_CaseMaster m   on t.CaseId = m.CaseId
                                  where Year(m.CloseDate) = " + _year + ")";
                        sqlStr = sqlStr + @" insert into  History_TX_60491_Grp Select * from TX_60491_Grp where CaseId in
                                   (select t.CaseID  from TX_60491_Grp t
                                  inner join  CaseMaster m   on t.CaseId = m.CaseId
                                  where  Year(m.CloseDate) = " + _year + ")";
                        break;
                    case "TX_67002":
                        sqlStr = @" SET IDENTITY_INSERT History_TX_67002 OFF   delete  History_TX_67002 where CaseId in
                                   (select t.CaseID  from History_TX_67002 t
                                  inner join  History_CaseMaster m   on t.CaseId = m.CaseId
                                  where  Year(m.CloseDate) = " + _year + ")";
                        sqlStr = sqlStr + @" insert into  History_TX_67002 ( cCretDT, CaseId, ANU_RPMT_AMT, ANU_RPMT_CHK, ANU_TOVR_AMT, ANU_TOVR_CHK, AUDT_NAME, BASEL_LIST, CIF_NO, CLN_CAPT_AMT, CLN_CAPT_CHK, CR_RATG, CREDIT_SCORE, CUST_NAME, EXTN_DPOS_AMT, FG_CONTRACT, FRWD_CMIT_AMT, ID_TYPE, LONBUS_DATE, N_CREDIT_GRADE, ORR_EDATE, OTH_BANK_DESC, OTH_BANK_OTSD_BAL, PAL_AMT_YY_01, PAL_AMT_YY_01_CHK, PAL_AMT_YY_02, PAL_AMT_YY_02_CHK, PAL_AMT_YY_03, PAL_AMT_YY_03_CHK, RM_NAME, RM_NO, SBG_NXT_RV_DT, SBG_PRV_RV_DT, SHDR_EQTY_AMT, SHDR_EQTY_CHK, SMART_FLAG, SORD_DEBT_AMT, SORD_DEBT_CHK, VIP_CODE, VIP_DEGREE, VIP_DEGREE_FLG, TrnNum, RepMessage) Select  cCretDT, CaseId, ANU_RPMT_AMT, ANU_RPMT_CHK, ANU_TOVR_AMT, ANU_TOVR_CHK, AUDT_NAME, BASEL_LIST, CIF_NO, CLN_CAPT_AMT, CLN_CAPT_CHK, CR_RATG, CREDIT_SCORE, CUST_NAME, EXTN_DPOS_AMT, FG_CONTRACT, FRWD_CMIT_AMT, ID_TYPE, LONBUS_DATE, N_CREDIT_GRADE, ORR_EDATE, OTH_BANK_DESC, OTH_BANK_OTSD_BAL, PAL_AMT_YY_01, PAL_AMT_YY_01_CHK, PAL_AMT_YY_02, PAL_AMT_YY_02_CHK, PAL_AMT_YY_03, PAL_AMT_YY_03_CHK, RM_NAME, RM_NO, SBG_NXT_RV_DT, SBG_PRV_RV_DT, SHDR_EQTY_AMT, SHDR_EQTY_CHK, SMART_FLAG, SORD_DEBT_AMT, SORD_DEBT_CHK, VIP_CODE, VIP_DEGREE, VIP_DEGREE_FLG, TrnNum, RepMessage from TX_67002 where CaseId in
                                   (select t.CaseID  from TX_67002 t
                                  inner join  CaseMaster m   on t.CaseId = m.CaseId
                                  where  Year(m.CloseDate) = " + _year + ")";
                        break;

                    default:
                        sqlStr = @"delete " + _targetTable + " where Year(CreatedDate) = " + _year + " insert into " + _targetTable + " Select * from " + _sourceTable + "  where   Year(CreatedDate) = " + _year;
                        break;
                }

                //sqlStr = @"delete "+ _targetTable+ " where Year(CreatedDate) = "+ _year+ " insert into " + _targetTable+" Select * from "+_sourceTable+ "  where   Year(CreatedDate) = "+_year;
                base.Parameter.Clear();
                //base.Parameter.Add(new CommandParameter("@sourceTable", _sourceTable));
                //base.Parameter.Add(new CommandParameter("@targetTable", _targetTable));
                //base.Parameter.Add(new CommandParameter("@Year", _year));
                string result = Convert.ToString(base.ExecuteNonQuery(sqlStr));
                if (rtn)
                    trans.Commit();
                else
                    trans.Rollback();

                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="strSQL"></param>
        /// <returns></returns>
        public DataTable OpenDataTable(string strSQL)
        {
            DataTable returnValue = new DataTable();

            try
            {
                base.Parameter.Clear();
                returnValue = base.Search(strSQL);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return returnValue;
        }

        /// <summary>
        /// Update DataRow
        /// </summary>
        /// <param name="tablename"></param>
        /// <param name="dr"></param>
        public void UpdateDataRow(string tablename, DataRow dr)
        {
            // 連接數據庫
            IDbConnection dbConnection = base.OpenConnection();
            IDbTransaction dbTransaction = null;
            string sql = "";

            if (tablename == "")
                throw new Exception("no TableName!!");

            // DB連接
            using (dbConnection)
            {
                try
                {
                    // 開始事務
                    dbTransaction = dbConnection.BeginTransaction();

                    CommandParameterCollection cmdparm = new CommandParameterCollection();  
                    CreateUpdateSql(tablename, dr, ref sql, ref cmdparm);
                    base.Parameter.Clear();
                    foreach (CommandParameter cmp in cmdparm)
                    {
                        base.Parameter.Add(cmp);
                    }

                    base.ExecuteNonQuery(sql, dbTransaction, false);

                    dbTransaction.Commit();
                }
                catch (Exception ex)
                {
                    dbTransaction.Rollback();

                    throw ex;
                }
            }
        }

        /// <summary>
        /// create insert sql command
        /// </summary>
        /// <param name="strTableName"></param>
        /// <param name="dr"></param>
        /// <param name="strInsert"></param>
        /// <param name="cmdparm"></param>
        private void CreateUpdateSql(string strTableName, DataRow dr, ref string strInsert, ref CommandParameterCollection cmdparm)
        {
            //產生 Insert SQL
            string strUpdateCol = "";

            foreach (DataColumn clmData in dr.Table.Columns)
            {
                string strColumnName = clmData.ColumnName.ToUpper();

                switch (strColumnName)
                {
                    case "NEWID":
                        cmdparm.Add(new CommandParameter("@" + strColumnName, dr[strColumnName]));
                        break;
                    default:
                        strUpdateCol += "," + strColumnName + "=@" + strColumnName;
                        cmdparm.Add(new CommandParameter("@" + strColumnName, dr[strColumnName]));
                        break;
                }
            }

            strUpdateCol = strUpdateCol.Substring(1);

            //產生命令
            strInsert = "Update " + strTableName + " Set " + strUpdateCol + " Where NewId = @NewId";
        }

        /// <summary>
        /// 解密
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public string Decode(string data)
        {
            string KEY_64 = "VavicApp";
            string IV_64 = "VavicApp";

            byte[] byKey = System.Text.ASCIIEncoding.ASCII.GetBytes(KEY_64);
            byte[] byIV = System.Text.ASCIIEncoding.ASCII.GetBytes(IV_64);
            byte[] byEnc;

            try
            {
                byEnc = Convert.FromBase64String(data);
            }
            catch
            {
                return null;
            }

            DESCryptoServiceProvider cryptoProvider = new DESCryptoServiceProvider();
            MemoryStream ms = new MemoryStream(byEnc);
            CryptoStream cst = new CryptoStream(ms, cryptoProvider.CreateDecryptor(byKey, byIV), CryptoStreamMode.Read);
            StreamReader sr = new StreamReader(cst);

            return sr.ReadToEnd();
        }

        /// <summary>
        /// 發送電文
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="txid"></param>
        /// <param name="drApprMsgDefine"></param>
        /// <param name="applNo"></param>
        /// <param name="applNoB"></param>
        /// <returns></returns>


        /// <summary>
        /// 發送60491電文
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="txid"></param>
        /// <param name="drApprMsgDefine"></param>
        /// <param name="VersionNewID"></param>
        /// <returns>ErrorSetting:設定錯誤；Error:發查失敗；Next：需要發查下一筆 </returns>


        public void InsertIntoTable(DataTable rtable, IDbTransaction dbTransaction = null)
        {
            if (dbTransaction != null)
            {
                InsertIntoTableWithTransaction(rtable, dbTransaction);
                return;
            }

            bool innertransaction = false;
            IDbConnection dbConnection;
            if (dbTransaction == null)
            {
                innertransaction = true;
                dbConnection = base.OpenConnection();
            }
            else
            {
                dbConnection = dbTransaction.Connection;
            }

            if (rtable.TableName.Trim() == "" || rtable.TableName.ToUpper() == "TABLE")
                throw new Exception("DataTable has no TableName!!");

            if (rtable.Columns.Contains("NewId"))
            {
                rtable.Columns.Remove("NewId");
            }

            using (dbConnection)
            {
                try
                {
                    // 開始事務
                    if (innertransaction == true)
                        dbTransaction = dbConnection.BeginTransaction();

                    using (SqlBulkCopy bulkinsert = new SqlBulkCopy((SqlConnection)dbConnection, SqlBulkCopyOptions.FireTriggers, (SqlTransaction)dbTransaction))
                    {
                        bulkinsert.BatchSize = 1000;
                        bulkinsert.BulkCopyTimeout = 60;
                        bulkinsert.DestinationTableName = rtable.TableName;

                        var arrayNames = (from DataColumn x in rtable.Columns
                                          select x.ColumnName).ToArray();

                        //取出該talbe 空的資料表
                        DataTable desdt = new DataTable();
                        desdt = GetEmptyDataTable(rtable.TableName);
                        string destColName = "";

                        bulkinsert.ColumnMappings.Clear();
                        for (int i = 0; i < arrayNames.Length; i++)
                        {
                            if (desdt.Columns[arrayNames[i]].ColumnName.ToUpper() == arrayNames[i].ToUpper())
                            {
                                destColName = desdt.Columns[arrayNames[i]].ColumnName;
                            }
                            else
                            {
                                //比對不到欄位則由bulkinsert來回error
                                destColName = arrayNames[i];
                            }
                            bulkinsert.ColumnMappings.Add(arrayNames[i], destColName);
                        }

                        //寫入
                        bulkinsert.WriteToServer(rtable);
                        bulkinsert.Close();
                    }

                    if (innertransaction == true)
                        dbTransaction.Commit();


                }
                catch (Exception ex)
                {
                    if (innertransaction == true)
                        dbTransaction.Rollback();

                    throw ex;
                }
            }
        }

        public void InsertIntoTableWithTransaction(DataTable rtable, IDbTransaction dbTransaction)
        {
            bool innertransaction = false;
            IDbConnection dbConnection;
            if (dbTransaction == null)
            {
                innertransaction = true;
                dbConnection = base.OpenConnection();
            }
            else
            {
                dbConnection = dbTransaction.Connection;
            }

            if (rtable.TableName.Trim() == "" || rtable.TableName.ToUpper() == "TABLE")
                throw new Exception("DataTable has no TableName!!");

            if (rtable.Columns.Contains("NewId"))
            {
                rtable.Columns.Remove("NewId");
            }

            try
            {
                // 開始事務
                if (innertransaction == true)
                    dbTransaction = dbConnection.BeginTransaction();

                using (SqlBulkCopy bulkinsert = new SqlBulkCopy((SqlConnection)dbConnection, SqlBulkCopyOptions.FireTriggers, (SqlTransaction)dbTransaction))
                {
                    bulkinsert.BatchSize = 1000;
                    bulkinsert.BulkCopyTimeout = 60;
                    bulkinsert.DestinationTableName = rtable.TableName;

                    var arrayNames = (from DataColumn x in rtable.Columns
                                      select x.ColumnName).ToArray();

                    //取出該talbe 空的資料表
                    DataTable desdt = new DataTable();
                    desdt = GetEmptyDataTable(rtable.TableName);
                    string destColName = "";

                    bulkinsert.ColumnMappings.Clear();
                    for (int i = 0; i < arrayNames.Length; i++)
                    {
                        if (desdt.Columns[arrayNames[i]].ColumnName.ToUpper() == arrayNames[i].ToUpper())
                        {
                            destColName = desdt.Columns[arrayNames[i]].ColumnName;
                        }
                        else
                        {
                            //比對不到欄位則由bulkinsert來回error
                            destColName = arrayNames[i];
                        }
                        bulkinsert.ColumnMappings.Add(arrayNames[i], destColName);
                    }

                    //寫入
                    bulkinsert.WriteToServer(rtable);
                    bulkinsert.Close();
                }

                if (innertransaction == true)
                    dbTransaction.Commit();
            }
            catch (Exception ex)
            {
                if (innertransaction == true)
                    dbTransaction.Rollback();

                throw ex;
            }
        }

        /// <summary>
        /// 取出空的資料集
        /// </summary>
        /// <param name="tablename"></param>
        /// <returns></returns>
        public DataTable GetEmptyDataTable(string tablename)
        {
            try
            {
                DataTable dt = new DataTable();
                string sql = "select * from " + tablename + "  Where  1=2";
                dt = OpenDataTable(sql);
                dt.TableName = tablename;
                return dt;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }





        /// <summary>
        /// log 記錄
        /// </summary>
        /// <param name="msg"></param>
        public void WriteLog(string msg)
        {
            if (Directory.Exists(@".\Log") == false)
            {
                Directory.CreateDirectory(@".\Log");
            }
            LogManager.Exists("DebugLog").Debug(msg);
        }









        internal void updateLastBackupDate(BackupSetting backupSetting, DateTime eDate)
        {
            IDbConnection dbConnection = OpenConnection();
            //WriteLog(string.Format("  嘗試砍掉表格 {0} 重覆的資料", dest));
            var trans = dbConnection.BeginTransaction();
            try
            {
                //string sqlStr = string.Format("delete from {0} h inner join CaseMaster cm on h.caseid =cm.CaseId  where {1}>='{2}' AND {1}<'{3}'", dest, dateField, sDate.ToString("yyyy-MM-dd"), eDate.AddDays(1).ToString("yyyy-MM-dd"));
                string sqlStr = string.Empty;

                sqlStr = string.Format("Update History_BackupSetting SET LastBackupDate='{0}' WHERE id ='{1}' ", eDate.ToString("yyyy-MM-dd"), backupSetting.Id);

                base.ExecuteNonQuery(sqlStr, trans);
                trans.Commit();

            }
            catch (Exception ex)
            {
                trans.Rollback();
                WriteLog(string.Format("  更新最後日期有誤. 錯誤原因: {0}", ex.Message));
            }
        }
        /// <summary>
        /// 取出目前被啟動的GroupNo
        /// </summary>
        /// <returns></returns>
        internal DataTable getEnabledGroupNo()
        {
            using (IDbConnection dbConnection = OpenConnection())
            {
                string sqlStr = @"SELECT GroupNo FROM [dbo].[History_BackupSetting] where Enable='1' Group by GroupNo";

                return base.Search(sqlStr);

            };
        }

        internal List<BackupSetting> getTablesByGroupNo(string GroupNo)
        {
            List<BackupSetting> result = new List<BackupSetting>();
            using (IDbConnection dbConnection = OpenConnection())
            {
                string sqlStr = string.Format(@"SELECT * FROM [dbo].[History_BackupSetting] where GroupNo='{0}' AND Enable='1' ", GroupNo);
                result = base.SearchList<BackupSetting>(sqlStr).OrderBy(x => x.SortOrder).ToList();
            }
            return result;
        }


        /// 1. 若扣押金額為0時, 直接搬走     
        /// 2. 若扣押金額>0 時,且已有支付或撒銷案, 已結案, 則搬走..
        internal List<Guid> getCaseIdCouldMove(BackupSetting CaseMasterSetting, DateTime sDate, DateTime eDate)
        {

            List<Guid> Result = new List<Guid>();

            #region Case 1. 若扣押金額為0時, 直接搬走
            var Case1CaseList = new List<CaseSeizure>();
            using (IDbConnection dbConnection = OpenConnection())
            {
                string sqlStr = string.Format(@"WITH CTE1 AS 
(select cs.CaseId, cs.SeizureAmount from CaseSeizure cs inner join CaseMaster cm on cs.CaseId=cm.CaseId 
	where cm.CloseDate>='{0}' AND cm.CloseDate<'{1}' )
select CaseId, sum(SeizureAmount) from CTE1 group by CaseId having sum(SeizureAmount)=0 ", sDate.ToString("yyyy-MM-dd"), eDate.ToString("yyyy-MM-dd"));
                Case1CaseList = base.SearchList<CaseSeizure>(sqlStr).ToList();
            }
            #endregion

            Result.AddRange(Case1CaseList.Select(x => x.CaseId));


            #region Case 2. 若扣押金額>0 時,且已有支付或撒銷案, 已結案, 則搬走..
            // 會帶出三個CaseId, PayCaseId, 跟CancelCaseId 三個.. 要展開, 都搬
            // 要記得, 防止若PayCase 或 CancelCase 沒有其他的23個關聯Table , 不會出錯....
            var Case2CaseList = new List<CaseSeizure>();
            using (IDbConnection dbConnection = OpenConnection())
            {
                string sqlStr = string.Format(@"select DISTINCT cs.CaseId, PayCaseId, CancelCaseId from CaseSeizure cs inner join CaseMaster cm on cs.CaseId=cm.CaseId 
where (cm.CloseDate>='{0}' AND cm.CloseDate<'{1}') AND (PayCaseId is not null or CancelCaseid is not null)
order by caseid ", sDate.ToString("yyyy-MM-dd"), eDate.ToString("yyyy-MM-dd"));
                Case2CaseList = base.SearchList<CaseSeizure>(sqlStr).ToList();
            }




            #endregion

            // 展開所有CaseId,包括CaseId, PayCaseId, 跟CancelCaseId 三個欄位
            Case2CaseList.ForEach(x =>
            {
                
                if (x.PayCaseId != null)
                {
                    // 20210114, 再來, 要過濾是否為Z01, Z03 , 若是, 才能進去搬動
                    bool isFinish = isCaseFinish(x.PayCaseId); 
                    if (isFinish) 
                    {
                        Result.Add((Guid)x.PayCaseId); // 加入支付案件
                        Result.Add(x.CaseId); // 若有支付, 才能加入扣押案件
                    }
                }
                if (x.CancelCaseId != null)
                {
                    // 20210114, 再來, 要過濾是否為Z01, Z03 , 若是, 才能進去搬動
                    bool isFinish = isCaseFinish(x.CancelCaseId);
                    if (isFinish)
                    {
                        Result.Add((Guid)x.CancelCaseId); // 加入撒銷案件
                        Result.Add(x.CaseId); // 若有撒銷, 才能加入扣押案件
                    }
                }
            });



            #region Case 3. 一般案件沒有搬
            //  CaseKind='外來文案件' and (STATUS='Z01' OR STATUS='Z03') and CloseDate>='{0}' and CloseDate<'{1}'
            using (IDbConnection dbConnection = OpenConnection())
            {
                string sqlStr = string.Format(@"SELECT *
  FROM CaseMaster where CaseKind='外來文案件' and (STATUS='Z01' OR STATUS='Z03') 
and CloseDate>='{0}' and CloseDate<'{1}' ", sDate.ToString("yyyy-MM-dd"), eDate.ToString("yyyy-MM-dd"));
                var CaseGeneral = base.SearchList<CaseMaster>(sqlStr).ToList();

                CaseGeneral.ForEach(x => Result.Add(x.CaseId));
            }
            #endregion


            #region Case 4. 支付或撒銷, 第二次來文... 此時, 要判斷... 案件的Z01 或Z03 .. 且沒有發文(.. 表示沒有CaseSendSetting
            // 20210201 SIT的測試方法, 是匯入一個新的支付案件, 用人工方法, 呈核到Z01... 目前是用 caseno='A110020100001'案例來測試...
            using (IDbConnection dbConnection = OpenConnection())
            {
                string sqlStr = string.Format(@"select m.* from casemaster m 
where (m.casekind2='支付' or m.casekind2='撒銷') and (m.status='Z01' or m.status='Z03') and m.caseid not in (select distinct caseid from CaseSendSetting) 
and CloseDate>='{0}' and CloseDate<'{1}' ", sDate.ToString("yyyy-MM-dd"), eDate.ToString("yyyy-MM-dd"));
                var Case2nd = base.SearchList<CaseMaster>(sqlStr).ToList();

                Case2nd.ForEach(x => Result.Add(x.CaseId));
            }


            
            #endregion

            return Result.Distinct().ToList();
        }

        private bool isCaseFinish(Guid? caseid)
        {
            bool result = false;
            using (IDbConnection dbConnection = OpenConnection())
            {
                try
                {
                    string sqlStr = string.Format(@"select * from CaseMaster where CaseId='{0}' and (Status='Z01' OR Status='Z03') ", caseid);
                    var ret = base.SearchList<CaseMaster>(sqlStr).ToList();
                    if (ret.Count() > 0)
                        result = true;
                    else
                        result = false;
                }
                catch (Exception ex)
                {
                    WriteLog("\t\t\t*****************錯誤, 查詢Castmaster 無法取得狀態");
                    result = false;
                }

            }

            return result;
        }

        internal int DeleteData_All(string dest)
        {
            IDbConnection dbConnection = OpenConnection();
            //WriteLog(string.Format("  嘗試砍掉表格 {0} 重覆的資料", dest));
            string sqlStr = string.Format("delete from {0} ", dest);

            var trans = dbConnection.BeginTransaction();
            try
            {
                //sqlStr = string.Format("delete from {0} where {1}>='{2}' AND {1}<'{3}'" + whereCaseIds, dest, dateField, sDate.ToString("yyyy-MM-dd"), eDate.AddDays(1).ToString("yyyy-MM-dd"));
                base.ExecuteNonQuery(sqlStr, trans);
                trans.Commit();
                //WriteLog(string.Format("  表格 {0} 重覆的資料已刪除", dest));
            }
            catch (Exception ex)
            {
                trans.Rollback();
                WriteLog(string.Format("  刪除重覆資料有錯，已恢復. 錯誤原因: {0}", ex.Message));
                //Console.WriteLine("Commit Exception Type: {0}", ex.GetType());
                //Console.WriteLine("  Message: {0}", ex.Message);   
                return 0;
            }

            return 1;
        }

        internal int BulkInsertionAll(string source, string dest)
        {
            IDbTransaction dbTrans = null;
            string whereCaseIds = "";

            using (SqlConnection con = OpenConnection())
            {


                dbTrans = con.BeginTransaction();
                try
                {
                    string sqlStr1 = string.Format("INSERT INTO {0} SELECT * FROM {1} ", dest, source);
                    base.ExecuteNonQuery(sqlStr1);
                    dbTrans.Commit();
                }
                catch (Exception ex2)
                {
                    WriteLog(string.Format("\t\t\t\tBulkinsertion 發生錯誤!! {0}  / {1} ", source, ex2.Message.ToString()));
                    return 0;
                }

                string sqlStr2 = string.Format("Select count(*) from {0} ", dest);
                DataTable ret = base.Search(sqlStr2);
                WriteLog(string.Format("\t\t\t\t共計 {0} 成功備份出{0} {1}筆資料", source, ret.Rows[0][0].ToString()));


            }


            return 1;
        }
    }
}
