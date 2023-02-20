using CTBC.CSFS.Models;
using CTBC.CSFS.ViewModels;
using CTBC.CSFS.Pattern;
using ICSharpCode.SharpZipLib.Checksums;
using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;

namespace CTBC.CSFS.BussinessLogic
{
    public class DeadRecordBIZ : CommonBIZ
    {
        CaseDeadCommonBIZ pCaseDeadCommonBIZ = new CaseDeadCommonBIZ();



        public bool Save(string strKey, string CustID, string CustAccount,string DateS ,string DateE,string LogonUser)
        {
            string Sql = "";
            IDbConnection dbConnection = OpenConnection();
            IDbTransaction dbTrans = null;
            try
            {
                using (dbConnection)
                {
                    dbTrans = dbConnection.BeginTransaction();

                    base.Parameter.Clear();

                    Sql += @" 
                            update CaseDeadVersion
                            set CustId = @CustId
                             ,CustAccount = @CustAccount
                             ,QDateS = @QDateS
                             ,QDateE = @QDateE
                             ,ModifiedDate = getdate()
                             ,ModifiedUser = @ModifiedUser 
                            where NewID = @NewID ; ";
                    base.Parameter.Add(new CommandParameter("@NewID", strKey));
                    base.Parameter.Add(new CommandParameter("@CustID", CustID));
                    base.Parameter.Add(new CommandParameter("@CustAccount", CustAccount));
                    base.Parameter.Add(new CommandParameter("@QDateS", DateS));
                    base.Parameter.Add(new CommandParameter("@QDateE", DateE));
                    base.Parameter.Add(new CommandParameter("@ModifiedUser", LogonUser));
                    int result = base.ExecuteNonQuery(Sql);

                    // 如果執行結果=資料筆數，就返回true，否則返回false
                    dbTrans.Commit();
                    return true;
                }
            }
            catch (Exception ex)
            {
                dbTrans.Rollback();

                return false;
            }
        }


        /// <summary>
        /// 上傳
        /// </summary>
        /// <param name="strKey">主檔主鍵</param>
        /// <param name="strAuditStatus">主檔審核狀態</param>
        /// <param name="LogonUser">登錄人員ID</param>
        /// <returns></returns>
        public bool Upload(string strKey, string strAuditStatus, string LogonUser)
        {
            IDbConnection dbConnection = OpenConnection();
            IDbTransaction dbTrans = null;
            try
            {
                using (dbConnection)
                {
                    dbTrans = dbConnection.BeginTransaction();

                    string[] arrayKey = strKey.Split(',');
                    string[] arrayAuditStatus = strAuditStatus.Split(',');

                    string Sql = "";
                    base.Parameter.Clear();

                    // 遍歷主鍵數組,組sql
                    for (int i = 0; i < arrayKey.Length; i++)
                    {
                        Sql += @" 
                            update CaseCustQuery
                            set 
                                UploadStatus = 'Y'
,UploadUserID = @ModifiedUser 
,CloseDate = getdate()
                                ,ModifiedDate = getdate()
                                ,ModifiedUser = @ModifiedUser 
                            where NewID = @NewID" + i.ToString() + "; ";
                        base.Parameter.Add(new CommandParameter("@NewID" + i.ToString(), arrayKey[i]));
                    }
                    base.Parameter.Add(new CommandParameter("@ModifiedUser", LogonUser));

                    int result = base.ExecuteNonQuery(Sql);

                    // 如果執行結果=資料筆數，就返回true，否則返回false
                    dbTrans.Commit();
                    return true;
                }
            }
            catch (Exception ex)
            {
                dbTrans.Rollback();

                return false;
            }
        }


        #region 回文檢視

        /// <summary>
        /// 根據主鍵取得回文檔案名稱
        /// </summary>
        /// <param name="strPk">主鍵</param>
        /// <returns></returns>
        public CaseCustQuery GetReturnFilesByPk(string strPk)
        {

            string sqlSlect = @" SELECT  ROpenFileName ,
                                          RFileTransactionFileName,
                                          Version
                                  FROM    CaseCustQuery
                                  WHERE   NewID=@NewID";

            base.Parameter.Clear();

            base.Parameter.Add(new CommandParameter("@NewID", strPk));

            return base.SearchList<CaseCustQuery>(sqlSlect)[0];
        }

        /// <summary>
        /// 根據主鍵取得回文檔案名稱
        /// </summary>
        /// <param name="strPk">主鍵</param>
        /// <returns></returns>


        public CaseMaster GetCaseMaster(string caseId, IDbTransaction trans = null)
        {
            string strSql = @" SELECT  *                                       
                                  FROM    CaseMaster
                                  WHERE   caseid=@CaseId";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("@CaseId", caseId));
            IList<CaseMaster> list = trans == null ? SearchList<CaseMaster>(strSql) : SearchList<CaseMaster>(strSql, trans);
            if (list != null)
            {
                return list.Count > 0 ? list[0] : new CaseMaster();
            }
            return new CaseMaster();
        }

        public CaseHisCondition GetCaseHisCondition(string NewID, IDbTransaction trans = null)
        {
            string strSql = @" SELECT  *                                       
                                  FROM    CaseDeadVersion
                                  WHERE   NewID=@CaseId";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("@CaseId", NewID));
            IList<CaseHisCondition> list = trans == null ? SearchList<CaseHisCondition>(strSql) : SearchList<CaseHisCondition>(strSql, trans);
            if (list != null)
            {
                return list.Count > 0 ? list[0] : new CaseHisCondition();
            }
            return new CaseHisCondition();
        }

        public CaseDeadVersion GetCaseDeadVersion(string NewID, IDbTransaction trans = null)
        {
            string strSql = @" SELECT  *                                       
                                  FROM    CaseDeadVersion
                                  WHERE   NewID=@CaseId";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("@CaseId", NewID));
            IList<CaseDeadVersion> list = trans == null ? SearchList<CaseDeadVersion>(strSql) : SearchList<CaseDeadVersion>(strSql, trans);
            if (list != null)
            {
                return list.Count > 0 ? list[0] : new CaseDeadVersion();
            }
            return new CaseDeadVersion();
        }
        /// <summary>
        /// 查詢勾選案件的回文
        /// </summary>
        /// <param name="strDocNo">案件編號</param>
        /// <returns></returns>
        public IList<CaseCustQuery> GedReturnFile(string strNewID)
        {

            string sqlSelect = @"  SELECT  DocNo + '_'+ CONVERT(NVARCHAR, Version)  + '.pdf'  AS PDFFileName
                                    FROM    CaseCustQuery
                                    WHERE   @NewID LIKE '%' + CONVERT(NVARCHAR(50), [NewID]) + '%'
                                    ORDER BY DocNo";

            base.Parameter.Clear();

            base.Parameter.Add(new CommandParameter("@NewID", strNewID));

            return base.SearchList<CaseCustQuery>(sqlSelect);
        }

        /// <summary>
        /// 压缩多個文件
        /// </summary>
        /// <param name="sourceFilePath"></param>
        /// <param name="destinationZipFilePath"></param>
        public void CreateZip(string sourceFilePath, string destinationZipFilePath, List<string> files, string strPassWord)
        {
            if (sourceFilePath[sourceFilePath.Length - 1] != System.IO.Path.DirectorySeparatorChar)
                sourceFilePath += System.IO.Path.DirectorySeparatorChar;

            ZipOutputStream zipStream = new ZipOutputStream(File.Create(destinationZipFilePath));

            zipStream.SetLevel(6);  // 压缩级别 0-9
            zipStream.Password = strPassWord;
            foreach (string file in files)
            {
                FileStream fileStream = File.OpenRead(file);

                byte[] buffer = new byte[fileStream.Length];


                fileStream.Read(buffer, 0, buffer.Length);
                string tempFile = file.Substring(sourceFilePath.LastIndexOf("\\") + 1);
                ZipEntry entry = new ZipEntry(Path.GetFileName(file));

                entry.DateTime = DateTime.Now;
                entry.Size = fileStream.Length;
                fileStream.Close();

                zipStream.PutNextEntry(entry);

                zipStream.Write(buffer, 0, buffer.Length);

            }

            zipStream.Finish();
            zipStream.Close();

            GC.Collect();
            GC.Collect(1);
        }

        #endregion
    }
}
