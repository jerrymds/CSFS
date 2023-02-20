using CTBC.CSFS.Models;
using CTBC.CSFS.Pattern;
using CTBC.FrameWork.Platform;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using NPOI.HSSF.UserModel;
using NPOI.XSSF.UserModel;
using System.Security.Cryptography;
using System.Text;
namespace CTBC.CSFS.BussinessLogic
{
    public class CaseDeadBIZ : CommonBIZ
    {
        string TodayYYYMMDD = "";
        CaseDeadCommonBIZ pCaseDeadCommonBIZ = new CaseDeadCommonBIZ();

        /// <summary>
        /// 查詢外文啟動查詢清單資料
        /// </summary>
        /// <param name="model"></param>
        /// <param name="pageNum"></param>
        /// <returns></returns>
  
        public int ProduceGenDetail(string DocNo, string CustId, string CustAccount, string ForCDateS, string ForCDateE, string UserID, ref string strTrnNum, string AcctDesc,string Option1,string Filename, IDbTransaction trans = null)
        {
            try
            {
                strTrnNum = "CSFS" + DateTime.Now.ToString("yyyyMMddHHmmssf" + AcctDesc);
                string strsql = @"insert into CaseTrsQueryHistory([NewID],[DocNo],[CustId],[CustAccount],[ForCDateS],[ForCDateE],[CreatedUser],[CreatedDate],[Status],TrnNum,AcctDesc,Option1,Option2,Option3)
	values(@NewID,@DocNo,@CustId,@CustAccount,@ForCDateS,@ForCDateE,@CreatedUser,GETDATE(),@Status,@TrnNum,@AcctDesc,@Option1,@Option2,@Option3)";
                Parameter.Clear();
                Parameter.Add(new CommandParameter("@NewID", Guid.NewGuid()));
                Parameter.Add(new CommandParameter("@DocNo", DocNo));
                Parameter.Add(new CommandParameter("@CustId", CustId));
                Parameter.Add(new CommandParameter("@CustAccount", CustAccount));
                Parameter.Add(new CommandParameter("@ForCDateS", ForCDateS));
                Parameter.Add(new CommandParameter("@ForCDateE", ForCDateE));
                Parameter.Add(new CommandParameter("@CreatedUser", UserID));
                Parameter.Add(new CommandParameter("@Status", "0"));
                Parameter.Add(new CommandParameter("@AcctDesc", AcctDesc));
                Parameter.Add(new CommandParameter("@TrnNum", strTrnNum));
                Parameter.Add(new CommandParameter("@Option1", Option1));
                Parameter.Add(new CommandParameter("@FileName", Filename));
                return trans == null ? ExecuteNonQuery(strsql) : ExecuteNonQuery(strsql, trans);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public int ProduceGenBatch(string DocNo, string CustId, string CustAccount,string DataBase,string Details,string ForCDateS, string ForCDateE, string UserID, ref string strTrnNum, string AcctDesc, string Option1, string Filename, IDbTransaction trans = null)
        {
            try
            {
                strTrnNum = "CSFS" + DateTime.Now.ToString("yyyyMMddHHmmssf" + AcctDesc);
                string strsql = @"insert into CaseTrsQueryHistory([NewID],[DocNo],[CustId],[CustAccount],[DataBase],[Details],[ForCDateS],[ForCDateE],[CreatedUser],[CreatedDate],[Status],TrnNum,AcctDesc,Option1)
	values(@NewID,@DocNo,@CustId,@CustAccount,@DataBase,@Details,@ForCDateS,@ForCDateE,@CreatedUser,GETDATE(),@Status,@TrnNum,@AcctDesc,@Option1)";
                Parameter.Clear();
                Parameter.Add(new CommandParameter("@NewID", Guid.NewGuid()));
                Parameter.Add(new CommandParameter("@DocNo", DocNo));
                Parameter.Add(new CommandParameter("@CustId", CustId));
                Parameter.Add(new CommandParameter("@CustAccount", CustAccount));
                Parameter.Add(new CommandParameter("@DataBase", DataBase));
                Parameter.Add(new CommandParameter("@Details", Details));
                Parameter.Add(new CommandParameter("@ForCDateS", ForCDateS));
                Parameter.Add(new CommandParameter("@ForCDateE", ForCDateE));
                Parameter.Add(new CommandParameter("@CreatedUser", UserID));
                Parameter.Add(new CommandParameter("@Status", "0"));
                Parameter.Add(new CommandParameter("@AcctDesc", AcctDesc));
                Parameter.Add(new CommandParameter("@TrnNum", strTrnNum));
                Parameter.Add(new CommandParameter("@Option1", Option1));
                Parameter.Add(new CommandParameter("@FileName", Filename));
                return trans == null ? ExecuteNonQuery(strsql) : ExecuteNonQuery(strsql, trans);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string GetParmCodeCurrency(string code)
        {
            string sqlSelect = @" select count(CodeDesc) from PARMCode where  CodeType='CaseCust_CURRENCY' and CodeDesc = '" + code + "'";

            // 清空容器
            base.Parameter.Clear();
           string  ct = base.Search(sqlSelect).Rows[0][0].ToString();
            return ct;
        }
        /// <summary>
        /// 取得查詢迄日要往前 n日
        /// </summary>
        /// <returns>往前 n日</returns>
        public int GetParmCodeEndDateDiff(string LastDay)
        {
            string sqlSelect = @"Select CodeDesc from PARMCode where CodeType='CSFSCode' and CodeNo='PreDay' ";

            // 清空容器
            base.Parameter.Clear();

            string day = base.Search(sqlSelect).Rows[0][0].ToString();

            int days = Convert.ToInt16(string.IsNullOrEmpty(day) ? "3" : day);

            DateTime eDate = Convert.ToDateTime(LastDay);
            DateTime eDateDiff = Convert.ToDateTime(DateTime.Today.AddDays(-days).ToShortDateString());

            if (eDate > eDateDiff)
            {
                return 1;
            }
            else
            {
                return 0;
            }

        }

        public DataTable GetCaseTrsQueryVersionToDT()
        {
            try
            {

                string sql = @"
                            SELECT [SEQ]
      ,[CustId]
      ,[CustAccount]
      ,[Currency]
      ,[OpenFlag]
      ,[TransactionFlag]
      ,[QDateS]
      ,[QDateE]
  
  from CaseTrsQueryVersion where DocNo = '123456'";

                return base.Search(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        /// <summary>
        /// 刪除清單資料
        /// </summary>
        /// <param name="Content"></param>
        /// <returns></returns>
        //public bool DeleteCaseTrsQuery(string Content)
        //{
        //    IDbConnection dbConnection = OpenConnection();
        //    IDbTransaction dbTrans = null;
        //    try
        //    {
        //        using (dbConnection)
        //        {
        //            dbTrans = dbConnection.BeginTransaction();

        //            // 將字串拆分成數組
        //            string[] arrayNewID = Content.Split(',');

        //            string sql = "";
        //            base.Parameter.Clear();

        //            // 遍歷，組刪除sql
        //            for (int i = 0; i < arrayNewID.Length; i++)
        //            {
        //               sql += " delete from CaseTrsQueryVersion where NewID = @NewID" + i.ToString() + "; ";

        //                base.Parameter.Add(new CommandParameter("@NewID" + i.ToString(), arrayNewID[i]));
        //            }

        //            base.ExecuteNonQuery(sql);

        //           // 若案件下所有待查ID都被刪除了，同步刪除主案件
        //            sql = @"delete from CaseTrsQuery
        //                    where (Status = '01')
        //                    AND NewID in 
        //                     (select CaseTrsQuery.NewID as CaseTrsNewID
        //                      from CaseTrsQuery
        //                      left join CaseTrsQueryVersion on CaseTrsQuery.NewID = CaseTrsQueryVersion.CaseTrsNewID
        //                      group by CaseTrsQuery.NewID
        //                      having count(CaseTrsQueryVersion.CaseTrsNewID) = 0
        //                     )";

        //            base.Parameter.Clear();
        //            base.ExecuteNonQuery(sql);

        //            dbTrans.Commit();

        //            return true;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        dbTrans.Rollback();
        //        return false;
        //    }
        //}

        //public DataTable ImportCSVFile(string PathSource)
        //{
        //    CaseDeadBIZ Biz = new CaseDeadBIZ();
        //    int i_state = 0;
        //    DataTable dt = new DataTable();
        //    try
        //    {
        //        //開啟Users.csv檔...
        //        using (FileStream fs = new FileStream(PathSource, FileMode.Open, FileAccess.Read))
        //        {
        //            using (StreamReader reader = new StreamReader(fs, System.Text.Encoding.UTF8))
        //            {
        //                string linestr;
        //                string[] Ary = new string[6];
        //                linestr = reader.ReadLine();
        //                //統一編號	帳號	基資	交易明細	區間起日(YYYY/MM/DD)	區間迄日(YYYY/MM/DD)
        //                while (linestr != null)
        //                {
        //                    Ary = linestr.Split(',');
        //                    //如果切出來是6個欄位才執行
        //                    if (Ary.Length == 6)
        //                    {
        //                        dt.Columns.Add(new DataColumn("Ｎｏ"));
        //                        dt.Columns.Add(new DataColumn("CustId"));
        //                        dt.Columns.Add(new DataColumn("CustAccount"));
        //                        dt.Columns.Add(new DataColumn("DataBase"));
        //                        dt.Columns.Add(new DataColumn("TrsDetails"));
        //                        dt.Columns.Add(new DataColumn("ForCDateS"));
        //                        dt.Columns.Add(new DataColumn("ForCDateE"));
        //                    }

        //                    DataRow newDataRow = dt.NewRow();
        //                    newDataRow[0] = (Ary[0] == null) ? "" : Ary[0];
        //                    newDataRow[1] = (Ary[1] == null) ? "" : Ary[1];
        //                    newDataRow[2] = (Ary[2] == null) ? "" : Ary[2];
        //                    newDataRow[3] = (Ary[3] == null) ? "" : Ary[3];
        //                    newDataRow[4] = (Ary[4] == null) ? "" : Ary[4];
        //                    newDataRow[5] = (Ary[5] == null) ? "" : Ary[5];
        //                    dt.Rows.Add(newDataRow);
        //                    //統一編號	帳號	基資	交易明細	區間起日(YYYY/MM/DD)	區間迄日(YYYY/MM/DD)
        //                    //i_state = Biz.ProduceGenDetail(DocNo, CustId, CustAccount, ForCDateS, ForCDateE, LogonUser.Account, ref strTrnNum, "S", false, false, true, pathSource);
        //                    //i_state = Biz.ProduceGenDetail(DocNo, CustId, CustAccount, ForCDateS, ForCDateE, LogonUser.Account, ref strTrnNum, "Q", false, false, true, pathSource);
        //                    //i_state = Biz.ProduceGenDetail(DocNo, CustId, CustAccount, ForCDateS, ForCDateE, LogonUser.Account, ref strTrnNum, "T", false, false, true, pathSource);
        //                }
        //            }
        //        }
        //        //CaseTrsCondition md = new CaseTrsCondition();
        //        //List<CaseTrsCondition> lst = new List<CaseTrsCondition>();
        //        //foreach (DataRow dr in dt.Rows)
        //        //{

        //        //    md.CustId = dr.Field<string>(0);
        //        //    md.CustAccount = dr.Field<string>(1);
        //        //    md.DataBase = dr.Field<string>(2);
        //        //    md.TrsDetails = dr.Field<string>(3);
        //        //    md.ForCDateS = dr.Field<string>(4);
        //        //    md.ForCDateE = dr.Field<string>(5);
        //        //    lst.Add(md);
        //        //}
        //        //md.CaseTrsConditionList = lst;
        //        ////return dt;
        //        //return md;
        //    }
        //    catch (Exception err)
        //    {
        //        throw err;
        //    }
        //    return dt;
        //}
        public CaseTrsCondition ImportToCaseTrsHis(string pathSource)
        {
            DataTable dt = new DataTable();

            FileStream fs = new FileStream(pathSource, FileMode.Open, FileAccess.ReadWrite);
            HSSFWorkbook templateWorkbook = new HSSFWorkbook(fs, true);
            HSSFSheet sheet = (HSSFSheet)templateWorkbook.GetSheet("工作表1");


            //dataRow.GetCell(1).SetCellValue("foo");

            MemoryStream ms = new MemoryStream();
            //templateWorkbook.Write(ms);

            //HSSFWorkbook workbook = null;
            //HSSFSheet sheet = null;

            try
            {
                #region 讀Excel檔，逐行寫入DataTable

                //sheet = (HSSFSheet)workbook.GetSheetAt(0);   //0表示：第一個 worksheet工作表


                HSSFRow headerRow = (HSSFRow)sheet.GetRow(0);   //Excel 表頭列
                //統一編號	帳號	基資	交易明細	區間起日(YYYY/MM/DD)	區間迄日(YYYY/MM/DD)
                for (int colIdx = 0; colIdx <= 6; colIdx++)   
                {
                    if (headerRow.GetCell(colIdx) != null)
                        dt.Columns.Add(new DataColumn(headerRow.GetCell(colIdx).StringCellValue));
                    //欄位名有折行時，只取第一行的名稱做法是headerRow.GetCell(colIdx).StringCellValue.Replace("\n", ",").Split(',')[0]
                }

                //For迴圈的「啟始值」為1，表示不包含 Excel表頭列
                for (int rowIdx = 1; rowIdx <= sheet.LastRowNum; rowIdx++)   //每一列做迴圈
                {
                    try
                    {
                        HSSFRow exlRow = (HSSFRow)sheet.GetRow(rowIdx); //不包含 Excel表頭列的 "其他資料列"
                        DataRow newDataRow = dt.NewRow();

                        for (int colIdx = exlRow.FirstCellNum; colIdx <= 5; colIdx++)   //exlRow.LastCellNum 每一個欄位做迴圈
                        {
                            if (exlRow.GetCell(colIdx) != null)
                            {
                                newDataRow[colIdx] = exlRow.GetCell(colIdx).ToString();
                            }
                            else
                            {
                                newDataRow[colIdx] = null;
                            }

                            //每一個欄位，都加入同一列 DataRow
                        }
                        //統一編號	帳號	基資	交易明細	區間起日(YYYY/MM/DD)	區間迄日(YYYY/MM/DD)
                        //i_state = Biz.ProduceGenDetail(DocNo, CustId, CustAccount, ForCDateS, ForCDateE, LogonUser.Account, ref strTrnNum, "S", false, false, true, pathSource);
                        //i_state = Biz.ProduceGenDetail(DocNo, CustId, CustAccount, ForCDateS, ForCDateE, LogonUser.Account, ref strTrnNum, "Q", false, false, true, pathSource);
                        //i_state = Biz.ProduceGenDetail(DocNo, CustId, CustAccount, ForCDateS, ForCDateE, LogonUser.Account, ref strTrnNum, "T", false, false, true, pathSource);
                        dt.Rows.Add(newDataRow);
                    }
                    catch (Exception err)
                    {
                        throw err;
                    }

                }
          
                using (var file = new FileStream(pathSource, FileMode.Open, FileAccess.ReadWrite))
                {
                    templateWorkbook.Write(file);
                }
                templateWorkbook.Write(ms);

                #endregion 讀Excel檔，逐行寫入DataTable
            }
            catch (Exception err)
            {
                throw err;    
                          
            }
            finally
            {
                //釋放 NPOI的資源
                templateWorkbook = null;
                sheet = null;
            }
            CaseTrsCondition md = new CaseTrsCondition();
            List<CaseTrsCondition> lst = new List<CaseTrsCondition>();
            foreach (DataRow dr in dt.Rows)
            {
                
                md.CustId = dr.Field<string>(0);
                md.CustAccount = dr.Field<string>(1);
                md.DataBase = dr.Field<string>(2);
                md.TrsDetails =dr.Field<string>(3);
                md.ForCDateS = dr.Field<string>(4);
                md.ForCDateE = dr.Field<string>(5);
                lst.Add(md);                
            }
            md.CaseTrsConditionList = lst;
            //return dt;
            return md;

        }

        public string GetCaseMasterbyCaseNo(string CaseNo)
        {
            string sqlSelect = @" select CaseId from CaseMaster where CaseNo = '" + CaseNo + "'";

            // 清空容器
            base.Parameter.Clear();
            string ct = base.Search(sqlSelect).Rows[0][0].ToString();
            return ct;
        }

        public bool DeleteCaseDeathLoan(string strCaseTrsNewID)
        {
            // 取得連接并開放連接
            IDbConnection dbConnection = base.OpenConnection();

            // 定義事務
            IDbTransaction dbTransaction = null;
            using (dbConnection)
            {
                // 開啟事務
                dbTransaction = dbConnection.BeginTransaction();
                DateTime thenow = DateTime.Now;
                string sql = @"Delete CaseDeathLoan where CaseDeadVersionNewID = @CaseDeadVersionNewID; Delete CaseDeathDeposit where CaseDeadVersionNewID = @CaseDeadVersionNewID;  Delete CaseDeathInvestment where CaseDeadVersionNewID = @CaseDeadVersionNewID;     ";
                    try
                    {
                        //ID	查詢基準	產品碼	案件編號	陳報金額	收件編號
                        base.Parameter.Clear();
                         base.Parameter.Add(new CommandParameter("@CaseDeadVersionNewID", strCaseTrsNewID));
                         ExecuteNonQuery(sql, dbTransaction);
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }

                dbTransaction.Commit();
                return true;
            }
        }
        public bool InsertCaseDeathLoan(DataTable  DT,string docno,string strCaseTrsNewID)
        {
            // 取得連接并開放連接
            IDbConnection dbConnection = base.OpenConnection();

            // 定義事務
            IDbTransaction dbTransaction = null;
            using (dbConnection)
            {
                // 開啟事務
                dbTransaction = dbConnection.BeginTransaction();
                DateTime thenow = DateTime.Now;
                int seq = 0;
                string sql = @"Delete CaseDeathLoan where IDHIS_IDNO_ACNO = @IDHIS_IDNO_ACNO and  RIGHT(RTRIM(ACC_NO),12) =  RIGHT(RTRIM(@ACC_NO),12)  ; INSERT INTO [dbo].[CaseDeathLoan] ([IDHIS_IDNO_ACNO], [ACC_NO],[BASE_DATE], [PROD_TYPE],[BAL],CaseDeadVersionNewID, [CaseNo], [Seq], [CreatedDate]) Values (@IDHIS_IDNO_ACNO,@ACC_NO, @BASE_DATE, @PROD_TYPE, @BAL,@CaseDeadVersionNewID,@CaseNo, @Seq, @CreatedDate)";
                for (int i = 0; i < DT.Rows.Count; i++)
                {
                    if (DT.Rows[i][0].ToString().Length > 0)
                    {
                        try
                        {
                            //ID	查詢基準	產品碼	案件編號	陳報金額	收件編號
                            base.Parameter.Clear();
                            base.Parameter.Add(new CommandParameter("@IDHIS_IDNO_ACNO", DT.Rows[i][0].ToString().Trim()));
                            //base.Parameter.Add(new CommandParameter("@UTIDNO", r[1]));
                            base.Parameter.Add(new CommandParameter("@ACC_NO", DT.Rows[i][3].ToString().Trim()));
                            //base.Parameter.Add(new CommandParameter("@BANK", r[3]));
                            base.Parameter.Add(new CommandParameter("@BASE_DATE", DT.Rows[i][1].ToString().Trim()));
                            base.Parameter.Add(new CommandParameter("@PROD_TYPE", DT.Rows[i][2].ToString().Trim()));
                            string Bal = DT.Rows[i][4].ToString() + "00";
                            if (Bal.Length <= 15)
                            {
                                Bal = '+' + Bal.PadLeft(15, '0');
                            }
                            else
                            {
                                Bal = '+' + Bal.Substring(Bal.Length - 15, 15);
                            }
                            base.Parameter.Add(new CommandParameter("@BAL", Bal)) ;// DT.Rows[i][4].ToString()));
                            base.Parameter.Add(new CommandParameter("@CaseDeadVersionNewID", strCaseTrsNewID));
                            base.Parameter.Add(new CommandParameter("@CaseNo", docno));
                            base.Parameter.Add(new CommandParameter("@Seq", seq++));
                            base.Parameter.Add(new CommandParameter("@CreatedDate", System.DateTime.Now));
                            ExecuteNonQuery(sql, dbTransaction);
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                    }
                }
                dbTransaction.Commit();
                return true;
            }
        }
        ///
        /// 上傳EXCEL
        /// <summary>
        /// 
        public DataTable ImportXLSX_MONEY(string pathSource,string strCaseTrsNewID,string filename,string DocNo)
        {
            //存入理債CaseEdocFile
            string UploadFile = pathSource;
            using (FileStream FileStream = System.IO.File.OpenRead(UploadFile))
            {
                byte[] Bytes = new byte[FileStream.Length];
                FileStream.Position = 0;
                FileStream.Read(Bytes, 0, Bytes.Length);
                CaseAccountBiz caseAccount = new CaseAccountBiz();
                Guid CaseId = Guid.Parse(strCaseTrsNewID);
                CaseEdocFile EdocFile = caseAccount.OpenDeadXlsx3(CaseId);
                //
                if (EdocFile != null)
                {
                    DeleteXlsxCaseEdocFile(CaseId);
                }
                CaseEdocFile caseEdocFile = new CaseEdocFile();
                caseEdocFile.CaseId = CaseId;
                caseEdocFile.Type = "死亡";
                caseEdocFile.FileType = "xlsx3";
                caseEdocFile.FileName = Path.GetFileName(pathSource);
                caseEdocFile.FileObject = Bytes;
                caseEdocFile.SendNo = DocNo;
                InsertCaseEdocFile(caseEdocFile);

            };


            DataTable dt = new DataTable();
            var fs = new FileStream(pathSource, FileMode.Open, FileAccess.ReadWrite);
            //FileStream fs = new FileStream(pathSource, FileMode.Open, FileAccess.ReadWrite);

            XSSFWorkbook templateWorkbook = new XSSFWorkbook(fs);
            string wh = templateWorkbook.GetSheetName(0);
            XSSFSheet sheet = (XSSFSheet)templateWorkbook.GetSheet(wh);


            //dataRow.GetCell(1).SetCellValue("foo");

            MemoryStream ms = new MemoryStream();
            //templateWorkbook.Write(ms);

            //HSSFWorkbook workbook = null;
            //HSSFSheet sheet = null;

            try
            {
                #region 讀Excel檔，逐行寫入DataTable

                //sheet = (HSSFSheet)workbook.GetSheetAt(0);   //0表示：第一個 worksheet工作表


                XSSFRow headerRow = (XSSFRow)sheet.GetRow(0);   //Excel 表頭列

                //ID	查詢基準	產品碼	案件編號	陳報金額	收件編號
                for (int colIdx = 0; colIdx <= 5; colIdx++)
                {
                    if (headerRow.GetCell(colIdx) != null)
                        dt.Columns.Add(new DataColumn("Columns" + colIdx.ToString()));
                    //欄位名有折行時，只取第一行的名稱做法是headerRow.GetCell(colIdx).StringCellValue.Replace("\n", ",").Split(',')[0]
                }

                //For迴圈的「啟始值」為1，表示不包含 Excel表頭列
                for (int rowIdx = 1; rowIdx <= sheet.LastRowNum; rowIdx++)   //每一列做迴圈
                {
                    try
                    {
                        XSSFRow exlRow = (XSSFRow)sheet.GetRow(rowIdx); //不包含 Excel表頭列的 "其他資料列"
                        DataRow newDataRow = dt.NewRow();
                        for (int colIdx = exlRow.FirstCellNum; colIdx <= 5; colIdx++)   //exlRow.LastCellNum 每一個欄位做迴圈
                        {
                            if ((colIdx == exlRow.FirstCellNum) && (exlRow.GetCell(colIdx).ToString().Trim().Length == 0))
                            {
                                break;
                            }
                            if (exlRow.GetCell(colIdx) != null)
                            {
                                checkdata(colIdx, exlRow.GetCell(colIdx).ToString().Trim());
                                newDataRow[colIdx] = exlRow.GetCell(colIdx).ToString().Trim();
                            }
                            else
                            {
                                newDataRow[colIdx] = "";
                                //err = err + colIdx.ToString() + "-" + exlRow.GetCell(colIdx).ToString() + ";";
                                //newDataRow[colIdx] = null;
                            }

                            //每一個欄位，都加入同一列 DataRow
                        }
                        //ID	查詢基準	產品碼	案件編號	陳報金額	收件編號
                        dt.Rows.Add(newDataRow);
                    }
                    catch (Exception e)
                    {
                        throw e;
                    }

                }

                using (var file = new FileStream(pathSource, FileMode.Open, FileAccess.ReadWrite))
                {
                    templateWorkbook.Write(file);
                }
                templateWorkbook.Write(ms);

                #endregion 讀Excel檔，逐行寫入DataTable
            }
            catch (Exception e)
            {
                throw e;

            }
            finally
            {
                //釋放 NPOI的資源
                templateWorkbook = null;
                sheet = null;
                fs.Close();
                fs.Dispose();
                File.Delete(pathSource);
            }
            return dt;
        }

        public int InsertCaseEdocFile(CaseEdocFile caseEdocFile)
        {
            try
            {
                int rtn = 0;
                string sql = @"insert into CaseEdocFile values(@CaseId,@Type,@FileType,@FileName,@FileObject,@SendNo)";
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@CaseId", caseEdocFile.CaseId));
                base.Parameter.Add(new CommandParameter("@Type", caseEdocFile.Type));
                base.Parameter.Add(new CommandParameter("@FileType", caseEdocFile.FileType));
                base.Parameter.Add(new CommandParameter("@FileName", caseEdocFile.FileName));
                base.Parameter.Add(new CommandParameter("@SendNo", caseEdocFile.SendNo));
                base.Parameter.Add(new CommandParameter("@FileObject", caseEdocFile.FileObject, SqlDbType.VarBinary, 0));
                rtn = base.ExecuteNonQuery(sql);
                return rtn;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public int DeleteCaseEdocFile(Guid caseid)
        {
            try
            {
                int rtn = 0;
                string sql = @"delete CaseEdocFile where CaseId=@CaseId and FileType='pdf' and Type='歷史'";
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@CaseId", caseid));
                rtn = base.ExecuteNonQuery(sql);
                return rtn;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public int DeleteXlsxCaseEdocFile(Guid caseid)
        {
            try
            {
                int rtn = 0;
                string sql = @"delete CaseEdocFile where CaseId=@CaseId and FileType='xlsx3' and Type='死亡'";
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@CaseId", caseid));
                rtn = base.ExecuteNonQuery(sql);
                return rtn;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public DataTable  ImportXLSX_NPOI(string pathSource)
        {
            string err = "";
            DataTable dt = new DataTable();
            var fs = new FileStream(pathSource, FileMode.Open, FileAccess.ReadWrite);
            //FileStream fs = new FileStream(pathSource, FileMode.Open, FileAccess.ReadWrite);

            XSSFWorkbook templateWorkbook = new XSSFWorkbook(fs);
            string wh = templateWorkbook.GetSheetName(0);
            XSSFSheet sheet = (XSSFSheet)templateWorkbook.GetSheet(wh);


            //dataRow.GetCell(1).SetCellValue("foo");

            MemoryStream ms = new MemoryStream();
            //templateWorkbook.Write(ms);

            //HSSFWorkbook workbook = null;
            //HSSFSheet sheet = null;

            try
            {
                #region 讀Excel檔，逐行寫入DataTable

                //sheet = (HSSFSheet)workbook.GetSheetAt(0);   //0表示：第一個 worksheet工作表


                XSSFRow headerRow = (XSSFRow)sheet.GetRow(0);   //Excel 表頭列

                //金融遺產查詢單位
                //縣市代號
                //總分局處所代號
                //申請日期
                //流水號
                //被繼承人身分證字號
                //承人姓名被繼
                //被繼承人出生日期
                //被繼承人死亡日期
                //申請人身分證字號
                //申請人姓名
                //申請人電話號碼
                //申請人與被繼承人關係
                //代理人身分證字號
                //代理人姓名
                //代理人電話號碼
                //送達地址縣市名稱
                //送達地址鄉鎮市區名稱
                //送達地址村里名稱
                //送達地址鄰
                //送達地址街道門牌
                for (int colIdx = 0; colIdx <= 20; colIdx++)
                {
                    if (headerRow.GetCell(colIdx) != null)
                        dt.Columns.Add(new DataColumn("Columns" + colIdx.ToString()));
                    //欄位名有折行時，只取第一行的名稱做法是headerRow.GetCell(colIdx).StringCellValue.Replace("\n", ",").Split(',')[0]
                }

                //For迴圈的「啟始值」為1，表示不包含 Excel表頭列
                for (int rowIdx = 1; rowIdx <= sheet.LastRowNum; rowIdx++)   //每一列做迴圈
                {
                    try
                    {
                        XSSFRow exlRow = (XSSFRow)sheet.GetRow(rowIdx); //不包含 Excel表頭列的 "其他資料列"
                        DataRow newDataRow = dt.NewRow();
                        for (int colIdx = exlRow.FirstCellNum; colIdx <= 20; colIdx++)   //exlRow.LastCellNum 每一個欄位做迴圈
                        {
                            if ( (colIdx == exlRow.FirstCellNum) && (exlRow.GetCell(colIdx).ToString().Trim().Length == 0)  )
                            {
                                break;
                            }
                            if (exlRow.GetCell(colIdx) != null)
                            {
                                checkdata(colIdx, exlRow.GetCell(colIdx).ToString());
                                newDataRow[colIdx] = exlRow.GetCell(colIdx).ToString();
                            }
                            else
                            {                                
                                newDataRow[colIdx] = "";
                                //err = err + colIdx.ToString() + "-" + exlRow.GetCell(colIdx).ToString() + ";";
                                //newDataRow[colIdx] = null;
                            }

                            //每一個欄位，都加入同一列 DataRow
                        }
                        //統一編號	帳號	基資	交易明細	區間起日(YYYY/MM/DD)	區間迄日(YYYY/MM/DD)
                        //i_state = Biz.ProduceGenDetail(DocNo, CustId, CustAccount, ForCDateS, ForCDateE, LogonUser.Account, ref strTrnNum, "S", false, false, true, pathSource);
                        //i_state = Biz.ProduceGenDetail(DocNo, CustId, CustAccount, ForCDateS, ForCDateE, LogonUser.Account, ref strTrnNum, "Q", false, false, true, pathSource);
                        //i_state = Biz.ProduceGenDetail(DocNo, CustId, CustAccount, ForCDateS, ForCDateE, LogonUser.Account, ref strTrnNum, "T", false, false, true, pathSource);
                        dt.Rows.Add(newDataRow);
                    }
                    catch (Exception e)
                    {
                        throw e;
                    }

                }

                using (var file = new FileStream(pathSource, FileMode.Open, FileAccess.ReadWrite))
                {
                    templateWorkbook.Write(file);
                }
                templateWorkbook.Write(ms);

                #endregion 讀Excel檔，逐行寫入DataTable
            }
            catch (Exception e)
            {
                throw e;

            }
            finally
            {
                //釋放 NPOI的資源
                templateWorkbook = null;
                sheet = null;
                File.Delete(pathSource);
            }
            return dt;
        }
        public string checkdata(int no,string fd)
        {
            string er = "";
            if (no >4 && no < 11)
            {
                if ((fd.Length == 0) )
                {
                    er = er + "no=" + no.ToString() + "-長度錯誤!" + ";";
                    return er;
                }
            }
            if (no == 16)
            {
                if ((fd.Length == 0))
                {
                    er = er + "no=" + no.ToString() + "-長度錯誤!" + ";";
                    return er;
                }
            }

            return er;
        }
        /// 啟動發查
        /// </summary>
        /// <returns></returns>
        public string StartSearch(User logonUser,string docno,string caseid)
        {
            string pUserAccount = logonUser.Account;
            //string pMaxTrnNum = "";
            // 查詢C外來文案件狀態爲未處理資料
            DataTable dt = GetCaseTrsQueryData();

            //TodayYYYMMDD = DateTime.Now.ToString("yyyyMMdd");

            //// 查詢本月最大的流水號
            //string pCSFSMaxTrnNum = GetMaxTrnNum();
            //if (pCSFSMaxTrnNum != "")
            //{
            //    pMaxTrnNum = pCSFSMaxTrnNum.Substring(pCSFSMaxTrnNum.Length - 5, 5);
            //}

            //// 流水號變量
            //int pTrnNum = 0;

            //// 截取流水號
            //if (pMaxTrnNum != "")
            //{
            //    pTrnNum = Convert.ToInt32(pMaxTrnNum);
            //}

            IDbConnection dbConnection = OpenConnection();
            IDbTransaction dbTrans = null;

            try
            {
                using (dbConnection)
                {
                    dbTrans = dbConnection.BeginTransaction();

                    base.Parameter.Clear();

                    if (dt != null && dt.Rows.Count > 0)
                    {
                        SaveMsg(dt, logonUser, dbTrans);
                        dbTrans.Commit();
                        return "設定成功-待發查";
                    }
                    else
                    {
                        return "NoData";
                    }
                }
            }
            catch (Exception ex)
            {
                dbTrans.Rollback();
                return "Error";
            }
        }

        /// <summary>
        /// 查詢需要啟動發查資料
        /// </summary>
        /// <returns></returns>
        public DataTable GetCaseTrsQueryData()
        {
            try
            {

                string sql = @"
                            select distinct
								         CaseMaster.CaseNo
                                ,CaseDeadVersion.*
                            from CaseDeadVersion
                            inner join CaseMaster
                            on CaseMaster.CaseId = CaseDeadVersion.CaseTrsNewId
                            where CaseDeadVersion.Status IN ('00') -- 新增資料
                            order by CaseMaster.CaseNo,CaseDeadVersion.CaseTrsNewID,CaseDeadVersion.CreatedDate desc ";

                return base.Search(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public DataTable QueryRFDMSend(string strVersionNewID)
        {
            try
            {

                string sql = @"
                            SELECT 
                                CaseTrsRFDMSend.TrnNum as TrnNum
                                ,ISNULL(acctDesc,'') as acctDesc
                            FROM CaseTrsRFDMSend
                            WHERE VersionNewID = '" + strVersionNewID + "'";

                return base.Search(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string GetDailyCase()
        {
            // Update CaseDeadVersion set SendStatus = '4' WHERE  SendStatus = '0' and CONVERT(VARCHAR(10),CreatedDate,111) = CONVERT(VARCHAR(10),getdate(),111) ;
            try
            {
                string DocNo = "";
                string sql = @"
                            select top 1 DocNo
                             from CaseDeadVersion                          
                            WHERE  CONVERT(VARCHAR(10),ModifiedDate,111) = CONVERT(VARCHAR(10),getdate(),111) and SendStatus = '0' ";

                DataTable dt = base.Search(sql);

                if (dt != null && dt.Rows.Count > 0)
                {
                    DocNo = dt.Rows[0][0].ToString();
                }

                return DocNo;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public int GetDailyData(string DocNo,string HeirId)
        {
            try
            {
                int DataCount = 0;
                string sql = @"
                            select NewId
                             from CaseDeadVersion                          
                            WHERE  ( DocNo = '" + DocNo + "' ) and CONVERT(VARCHAR(10),CreatedDate,111) = CONVERT(VARCHAR(10),getdate(),111) ";                               

                DataTable dt = base.Search(sql);

                if (dt != null && dt.Rows.Count > 0)
                {
                    DataCount = dt.Rows.Count;
                }

                return DataCount;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        /// <summary>
        /// 案件總筆數
        /// </summary>
        /// <returns></returns>
        public int GetDataCount()
        {
            try
            {
                int DataCount = 0;

                string sql = @"
                            select 
                            	CaseTrsNewID
                            from CaseTrsQuery
                            inner join CaseTrsQueryVersion
                            on CaseTrsQueryVersion.CaseTrsNewID = CaseTrsQuery.NewID
                            WHERE  isnull( CaseTrsQueryVersion.Status,'') = '01'
                                or isnull( CaseTrsQueryVersion.Status,'') = '02'
                            	or isnull( CaseTrsQueryVersion.Status,'') = '04'
                            group by CaseTrsNewID ";

                DataTable dt = base.Search(sql);

                if (dt != null && dt.Rows.Count > 0)
                {
                    DataCount = dt.Rows.Count;
                }

                return DataCount;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #region insert ApprMsgKey

        /// <summary>
        /// insert或update ApprMsgKey表
        /// </summary>
        /// <param name="dt">資料集</param>
        /// <param name="logonUser"><登錄人員信息/param>
        /// <param name="dbTrans">事務</param>
        public void SaveMsg(DataTable dt, User logonUser, IDbTransaction dbTrans)
        {
            for (int j = 0; j < dt.Rows.Count; j++)
            {
                // 當OpenFlag=“Y”時，更新BOPS060628Send
                //if (dt.Rows[j]["OpenFlag"].ToString() == "Y")
                //{
                    SaveMsgKey(new Guid(dt.Rows[j]["NewID"].ToString()), logonUser, dbTrans);
                //}
            }
        }

        /// <summary>
        /// 保存ApprMsgKey資料
        /// </summary>
        /// <param name="strVersionNewID">VersionNewID</param>
        /// <param name="logonUser">登錄人員資料</param>
        /// <param name="dbTrans">事務</param>
        /// <returns></returns>
        public bool SaveMsgKey(Guid strVersionNewID, User logonUser, IDbTransaction dbTrans)
        {
            try
            {
                bool flag = false;

                // 獲取登錄人員資料
                ApprMsgKeyVO model = new ApprMsgKeyVO();
                model.MsgUID = logonUser.Account;
                model.MsgKeyLP = logonUser.LDAPPwd;
                model.MsgKeyLU = logonUser.Account;
                model.MsgKeyRU = logonUser.RCAFAccount;
                model.MsgKeyRP = logonUser.RCAFPs;
                model.MsgKeyRB = logonUser.RCAFBranch;

                // VersionNewID
                model.VersionNewID = strVersionNewID;

                // 判斷資料是否存在ApprMsgKey,如果不存在就可向ApprMsgKey增加資料
                if (!isExistInMsgKey(strVersionNewID, logonUser.Account, dbTrans))
                {
                    flag = InsertApprMsgKey(model, dbTrans);
                }
                return flag;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 判斷是否存在ApprMsgKey
        /// </summary>
        /// <param name="strVersionNewID"></param>
        /// <param name="logonUser">登錄人員ID</param>
        /// <param name="dbTrans">事務</param>
        /// <returns></returns>
        public bool isExistInMsgKey(Guid strVersionNewID, string logonUser, IDbTransaction dbTrans)
        {
            try
            {
                string strSql = @"SELECT  COUNT(*)
                                  FROM    dbo.ApprMsgKey
                                  WHERE   VersionNewID = @VersionNewID
                                          AND MsgUID = @MsgUID ";
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@VersionNewID", strVersionNewID));
                base.Parameter.Add(new CommandParameter("@MsgUID", Encode(logonUser)));
                int n = (int)base.ExecuteScalar(strSql, dbTrans);
                if (n > 0) return true;
                else return false;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        ////
        //// DT (頁面或上傳檔案) 寫入 CaseTrsQuery
        ////
    
        public string InsertCaseDeadVersion(DataTable dt, string CaseNo, string caseid, User logonUser, string Option1, string filename, IDbTransaction trans)
        {
            // 取得連接并開放連接
            IDbConnection dbConnection = base.OpenConnection();

            // 定義事務
            IDbTransaction dbTransaction = null;
            using (dbConnection)
            {
                // 開啟事務
                dbTransaction = dbConnection.BeginTransaction();

                string strTrnNum = "";
                string strsql = "";
                //string _CaseTrsNewID = "";
                //string _DocNo = "";
                //string _QueryUnit = "";
                //string _CityNo = "";
                //string _BrigeNo = "";
                //string _AppDate = "";
                //string _SNo = "";
                //string _HeirId = "";
                //string _HeirName = "";
                //string _HeirBirthDay = "";
                //string _HeirDeadDate = "";
                //string _AppId = "";
                //string _AppName = "";
                //string _AppTel = "";
                //string _Relation = "";
                //string _AgentId = "";
                //string _AgentName = "";
                //string _AgentTel = "";
                //string _SendCity = "";
                //string _SendTown = "";
                //string _SendLe = "";
                //string _SendLin = "";
                //string _SendStreet = "";
                //string _CreatedUser = "";
                //string _Seq = "";
                //string _Status = "00";
                string kk = "";
                try
                {
                    for (int j = 0; j < dt.Rows.Count; j++)
                    {
                        if ((string.IsNullOrEmpty(dt.Rows[j][0].ToString())))
                        {
                            break;
                        }
                        strTrnNum = "CSFS" + DateTime.Now.ToString("yyyyMMddHHmmssf");
                        // MergeQU, deposit, Loan, LoanMgr, Box, CashCard, CreditCard, InvestMgr, 
                        strsql = @"INSERT INTO CaseDeadVersion (NewID, CaseTrsNewID, DocNo, QueryUnit, CityNo, BrigeNo, AppDate, SNo, HeirId, HeirName, HeirBirthDay, HeirDeadDate, AppId, AppName, AppTel, Relation, AgentId, AgentName, AgentTel, SendCity, SendTown,SendLe, SendLin, SendStreet,CreatedDate, CreatedUser, ModifiedDate, ModifiedUser,Seq, Status,SendStatus)  VALUES (@NewID, @CaseTrsNewID, @DocNo, @QueryUnit, @CityNo, @BrigeNo, @AppDate, @SNo, @HeirId, @HeirName, @HeirBirthDay, @HeirDeadDate, @AppId, @AppName, @AppTel, @Relation, @AgentId, @AgentName, @AgentTel, @SendCity, @SendTown, @SendLe,@SendLin, @SendStreet,  getdate(), @CreatedUser, getdate(), @CreatedUser,@Seq,  @Status,@SendStatus)";
                        Parameter.Clear();
                        Parameter.Add(new CommandParameter("@NewID", Guid.NewGuid()));
                        Parameter.Add(new CommandParameter("@CaseTrsNewID", caseid));
                        Parameter.Add(new CommandParameter("@DocNo", CaseNo));
                        Parameter.Add(new CommandParameter("@QueryUnit", dt.Rows[j][0].ToString().Trim()));
                        Parameter.Add(new CommandParameter("@CityNo", dt.Rows[j][1].ToString().Trim()));
                        Parameter.Add(new CommandParameter("@BrigeNo", dt.Rows[j][2].ToString().Trim()));
                        Parameter.Add(new CommandParameter("@AppDate", dt.Rows[j][3].ToString().Trim()));
                        Parameter.Add(new CommandParameter("@SNo", dt.Rows[j][4].ToString().Trim()));
                        Parameter.Add(new CommandParameter("@HeirId", dt.Rows[j][5].ToString().Trim()));
                        Parameter.Add(new CommandParameter("@HeirName", dt.Rows[j][6].ToString().Trim()));
                        Parameter.Add(new CommandParameter("@HeirBirthDay", dt.Rows[j][7].ToString().Trim()));
                        Parameter.Add(new CommandParameter("@HeirDeadDate", dt.Rows[j][8].ToString().Trim()));
                        Parameter.Add(new CommandParameter("@AppId", dt.Rows[j][9].ToString().Trim()));
                        Parameter.Add(new CommandParameter("@AppName", dt.Rows[j][10].ToString().Trim()));
                        Parameter.Add(new CommandParameter("@AppTel", dt.Rows[j][11].ToString().Trim()));
                        Parameter.Add(new CommandParameter("@Relation", dt.Rows[j][12].ToString().Trim()));
                        Parameter.Add(new CommandParameter("@AgentId", dt.Rows[j][13].ToString().Trim()));
                        Parameter.Add(new CommandParameter("@AgentName", dt.Rows[j][14].ToString().Trim()));
                        Parameter.Add(new CommandParameter("@AgentTel", dt.Rows[j][15].ToString().Trim()));
                        Parameter.Add(new CommandParameter("@SendCity", dt.Rows[j][16].ToString().Trim()));
                        Parameter.Add(new CommandParameter("@SendTown", dt.Rows[j][17].ToString().Trim()));
                        Parameter.Add(new CommandParameter("@SendLe", dt.Rows[j][18].ToString().Trim()));
                        Parameter.Add(new CommandParameter("@SendLin", dt.Rows[j][19].ToString().Trim()));
                        Parameter.Add(new CommandParameter("@SendStreet", dt.Rows[j][20].ToString().Trim()));
                        Parameter.Add(new CommandParameter("@CreatedUser", logonUser.Account));
                        Parameter.Add(new CommandParameter("@Seq", j+1));
                        Parameter.Add(new CommandParameter("@Status", "00"));
                        Parameter.Add(new CommandParameter("@SendStatus", "4"));
                        //Guid CaseNewID = Guid.NewGuid();
                        ////Parameter.Add(new CommandParameter("@NewID", CaseNewID));
                        //strsql = @"INSERT INTO CaseDeadVersion (NewID, CaseTrsNewID, DocNo, QueryUnit, CityNo, BrigeNo, AppDate, SNo, HeirId, HeirName, HeirBirthDay, HeirDeadDate, AppId, AppName, AppTel, Relation, AgentId, AgentName, AgentTel, SendCity, SendTown,SendLe, SendLin, SendStreet,CreatedDate, CreatedUser, ModifiedDate, ModifiedUser,Seq, Status)  VALUES ('"+ CaseNewID.ToString()+"','";

                        //_CaseTrsNewID = caseid;
                        //_DocNo = CaseNo;
                        //_QueryUnit = dt.Rows[j][0].ToString();
                        //_CityNo = dt.Rows[j][1].ToString();
                        //_BrigeNo = dt.Rows[j][2].ToString();
                        //_AppDate = dt.Rows[j][3].ToString();
                        //_SNo = dt.Rows[j][4].ToString();
                        //_HeirId = dt.Rows[j][5].ToString();
                        //_HeirName = dt.Rows[j][6].ToString();
                        //_HeirBirthDay = dt.Rows[j][7].ToString();
                        //_HeirDeadDate = dt.Rows[j][8].ToString();
                        //_AppId = dt.Rows[j][9].ToString();
                        //_AppName = dt.Rows[j][10].ToString();
                        //_AppTel = dt.Rows[j][11].ToString();
                        //_Relation = dt.Rows[j][12].ToString();
                        //_AgentId = dt.Rows[j][13].ToString();
                        //_AgentName = dt.Rows[j][14].ToString();
                        //_AgentTel = dt.Rows[j][15].ToString();
                        //_SendCity = dt.Rows[j][16].ToString();
                        //_SendTown = dt.Rows[j][17].ToString();
                        //_SendLe = dt.Rows[j][18].ToString();
                        //_SendLin = dt.Rows[j][19].ToString();
                        //_SendStreet = dt.Rows[j][20].ToString();
                        //_CreatedUser = logonUser.Account;
                        //_Seq = j + 1.ToString();
                        //_Status = "00";
                        //strsql = strsql + _CaseTrsNewID + "','";
                        //strsql = strsql + _DocNo + "','";
                        //strsql = strsql + _QueryUnit + "','";
                        //strsql = strsql + _CityNo + "','";
                        //strsql = strsql + _BrigeNo + "','";
                        //strsql = strsql + _AppDate + "','";
                        //strsql = strsql + _SNo + "','";
                        //strsql = strsql + _HeirId +"','";
                        //strsql = strsql + _HeirName + "','";
                        //strsql = strsql + _HeirBirthDay + "','";
                        //strsql = strsql + _HeirDeadDate + "','";
                        //strsql = strsql + _AppId + "','";
                        //strsql = strsql + _AppName + "','";
                        //strsql = strsql + _AppTel + "','";
                        //strsql = strsql + _Relation + "','";
                        //strsql = strsql + _AgentId + "','";
                        //strsql = strsql + _AgentName + "','";
                        //strsql = strsql + _AgentTel + "','";
                        //strsql = strsql + _SendCity + "','";
                        //strsql = strsql + _SendTown + "','";
                        //strsql = strsql + _SendLe + "','";
                        //strsql = strsql + _SendLin + "','";
                        //strsql = strsql + _SendStreet + "','";
                        //strsql = strsql + _CreatedUser + "',getdate(),'"+ _CreatedUser +"',getdate(),";
                        //strsql = strsql + Convert.ToUInt16(_Seq) + ",'";
                        //strsql = strsql + _Status + "')";

                        //Parameter.Add(new CommandParameter("@CaseTrsNewID", caseid));
                        //Parameter.Add(new CommandParameter("@DocNo", CaseNo));
                        //Parameter.Add(new CommandParameter("@QueryUnit", dt.Rows[j][0].ToString()));
                        //Parameter.Add(new CommandParameter("@CityNo", dt.Rows[j][1].ToString()));
                        //Parameter.Add(new CommandParameter("@BrigeNo", dt.Rows[j][2].ToString()));
                        //Parameter.Add(new CommandParameter("@AppDate", dt.Rows[j][3].ToString()));
                        //Parameter.Add(new CommandParameter("@SNo", dt.Rows[j][4].ToString()));
                        //Parameter.Add(new CommandParameter("@HeirId", dt.Rows[j][5].ToString()));
                        //Parameter.Add(new CommandParameter("@HeirName", dt.Rows[j][6].ToString()));
                        //Parameter.Add(new CommandParameter("@HeirBirthDay", dt.Rows[j][7].ToString()));
                        //Parameter.Add(new CommandParameter("@HeirDeadDate", dt.Rows[j][8].ToString()));
                        //Parameter.Add(new CommandParameter("@AppId", dt.Rows[j][9].ToString()));
                        //Parameter.Add(new CommandParameter("@AppName", dt.Rows[j][10].ToString()));
                        //Parameter.Add(new CommandParameter("@AppTel", dt.Rows[j][11].ToString()));
                        //Parameter.Add(new CommandParameter("@Relation", dt.Rows[j][12].ToString()));
                        //Parameter.Add(new CommandParameter("@AgentId", dt.Rows[j][13].ToString()));
                        //Parameter.Add(new CommandParameter("@AgentName", dt.Rows[j][14].ToString()));
                        //Parameter.Add(new CommandParameter("@AgentTel", dt.Rows[j][15].ToString()));
                        //Parameter.Add(new CommandParameter("@SendCity", dt.Rows[j][16].ToString()));
                        //Parameter.Add(new CommandParameter("@SendTown", dt.Rows[j][17].ToString()));
                        //Parameter.Add(new CommandParameter("@SendLe", dt.Rows[j][18].ToString()));
                        //Parameter.Add(new CommandParameter("@SendLin", dt.Rows[j][19].ToString()));
                        //Parameter.Add(new CommandParameter("@SendStreet", dt.Rows[j][20].ToString()));
                        //Parameter.Add(new CommandParameter("@CreatedUser", logonUser.Account));
                        ////Parameter.Add(new CommandParameter("@IdNo        ", dt.Rows[j][32].ToString()));
                        //Parameter.Add(new CommandParameter("@Seq", j+1.ToString()));
                        //Parameter.Add(new CommandParameter("@Status", "00"));
                        /*
                        Parameter.Add(new CommandParameter("@CustId", dt.Rows[j][1].ToString()));
                        if (dt.Rows[j][2].ToString().Length > 7)
                        {
                            Parameter.Add(new CommandParameter("@CustAccount", dt.Rows[j][2].ToString().PadLeft(17, '0')));
                        }
                        else
                        {
                            Parameter.Add(new CommandParameter("@CustAccount", ""));
                        }
                        Parameter.Add(new CommandParameter("@Currency", dt.Rows[j][3].ToString()));
                        Parameter.Add(new CommandParameter("@OpenFlag", dt.Rows[j][4].ToString()));
                        Parameter.Add(new CommandParameter("@TransactionFlag", dt.Rows[j][5].ToString()));
                        DateTime dateS = DateTime.Parse("1911/01/01");
                        if (dt.Rows[j][6].ToString().Length > 7)
                        {
                            dateS = DateTime.Parse(dt.Rows[j][6].ToString());
                        }
                        string dateST = dateS.ToString("yyyyMMdd");
                        DateTime dateE = System.DateTime.Now;                           
                        if (dt.Rows[j][7].ToString().Length > 7)
                        {
                            dateE = DateTime.Parse(dt.Rows[j][7].ToString());
                        }
                        string dateEN = dateE.ToString("yyyyMMdd");
                        Parameter.Add(new CommandParameter("@ForCDateS", dateST));
                        Parameter.Add(new CommandParameter("@ForCDateE", dateEN));
                        Parameter.Add(new CommandParameter("@CreatedUser", logonUser.Account));
                        Parameter.Add(new CommandParameter("@Status", "00"));//Adam 20200205 00新增
                        // HTG查詢狀態
                        string strHTGSendStatus = dt.Rows[j][4].ToString() == "Y" ? "0" : "0";
                        // RFDM查詢狀態
                        string strRFDMSendStatus = dt.Rows[j][5].ToString() == "Y" ? "0" : "0";
                        Parameter.Add(new CommandParameter("@HTGSendStatus", strHTGSendStatus));
                        Parameter.Add(new CommandParameter("@RFDMSendStatus", strRFDMSendStatus));
                        Parameter.Add(new CommandParameter("@Seq", dt.Rows[j][0].ToString()));
                        Parameter.Add(new CommandParameter("@DocNo", CaseNo));
                        */
                        kk = "第"+(j+1).ToString()+"筆資料有誤:";
                        ExecuteNonQuery(strsql, dbTransaction);
                        //ExecuteNonQuery(strsql);
                    }
                }
                catch (Exception ex)
                {
                    dbTransaction.Rollback();
                    return "新增死亡名單錯誤:"+kk+ex.Message.ToString();
                    //hrow ex;
                }
                dbTransaction.Commit();
                return "";
            }
        }

        public int insertCaseDeadQueryHistory(CaseDeadCondition model, IDbTransaction trans, string filename)
        {
            try
            {
                string sql = @"INSERT INTO  [CaseDeadQueryHistory] ([NewID],[DocNo],[RecvDate],[QFileName]) VALUES  (@NewID,@DocNo,getdate(),@QFileName)";
                Parameter.Clear();
                Parameter.Add(new CommandParameter("@NewID", Guid.NewGuid()));
                Parameter.Add(new CommandParameter("@DocNo", model.DocNo));
                Parameter.Add(new CommandParameter("@QFileName", filename));
                return trans == null ? ExecuteNonQuery(sql) : ExecuteNonQuery(sql, trans);
            }
            catch (Exception ex)
            {
                throw ex;               
            }
        }


        /// <summary>
        /// insert ApprMsgKey
        /// </summary>
        /// <param name="model">實體資料</param>
        /// <param name="dbTrans">事務</param>
        /// <returns></returns>
        public bool InsertApprMsgKey(ApprMsgKeyVO model, IDbTransaction dbTrans)
        {
            try
            {
                string sql = @"INSERT INTO dbo.ApprMsgKey
                                                    ( MsgKeyLU ,
                                                      MsgKeyLP ,
                                                      MsgKeyRU ,
                                                      MsgKeyRP ,
                                                      MsgKeyRB ,
                                                      MsgUID ,
                                                      VersionNewID
                                                    )
                                            VALUES  ( @MsgKeyLU, 
                                                     @MsgKeyLP ,
                                                     @MsgKeyRU ,
                                                     @MsgKeyRP ,
                                                     @MsgKeyRB ,
                                                     @MsgUID ,
                                                     @VersionNewID )";
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@MsgKeyLU", Encode(model.MsgKeyLU)));
                base.Parameter.Add(new CommandParameter("@MsgKeyLP", Encode(model.MsgKeyLP)));
                base.Parameter.Add(new CommandParameter("@MsgKeyRU", Encode(model.MsgKeyRU)));
                base.Parameter.Add(new CommandParameter("@MsgKeyRP", Encode(model.MsgKeyRP)));
                base.Parameter.Add(new CommandParameter("@MsgKeyRB", Encode(model.MsgKeyRB)));
                base.Parameter.Add(new CommandParameter("@MsgUID", Encode(model.MsgUID)));
                base.Parameter.Add(new CommandParameter("@VersionNewID", model.VersionNewID));
                return base.ExecuteNonQuery(sql, dbTrans) > 0 ? true : false;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 加密
        /// </summary>
        /// <param name="data">加密字串</param>
        /// <returns></returns>
        public string Encode(string data)
        {
            string KEY_64 = "VavicApp";
            string IV_64 = "VavicApp";

            byte[] byKey = System.Text.ASCIIEncoding.ASCII.GetBytes(KEY_64);
            byte[] byIV = System.Text.ASCIIEncoding.ASCII.GetBytes(IV_64);

            DESCryptoServiceProvider cryptoProvider = new DESCryptoServiceProvider();
            int i = cryptoProvider.KeySize;
            MemoryStream ms = new MemoryStream();
            CryptoStream cst = new CryptoStream(ms, cryptoProvider.CreateEncryptor(byKey, byIV), CryptoStreamMode.Write);
            StreamWriter sw = new StreamWriter(cst);
            sw.Write(data);
            sw.Flush();
            cst.FlushFinalBlock();
            sw.Flush();

            return Convert.ToBase64String(ms.GetBuffer(), 0, (int)ms.Length);
        }

        /// <summary>
        /// 解密
        /// </summary>
        /// <param name="data">解密字串</param>
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

        #endregion

        /// <summary>
        /// 查詢CaseTrsRFDMSend中本月最大的流水號
        /// </summary>
        /// <returns></returns>
        public string GetMaxTrnNum()
        {
            try
            {

                string sql = @"
                            select 
                            	isnull(MAX(TrnNum),'') as TrnNumMax 
                            from CaseTrsRFDMSend
                            where TrnNum like '%" + DateTime.Now.ToString("yyyyMM") + "%' ";

                DataTable dt = base.Search(sql);

                if (dt != null && dt.Rows.Count > 0)
                {
                    return dt.Rows[0]["TrnNumMax"].ToString();
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

        /// <summary>
        /// 計算流水號
        /// YYYMMDD+4位數流水號
        /// </summary>
        /// <param name="strMaxNo">根據最大流水號+1(純數字流水號)</param>
        /// <returns></returns>
        private string CalculateTrnNum(int strMaxNo)
        {
            return  TodayYYYMMDD + String.Format("{0:D5}", strMaxNo + 1);
        }

    
   


    }
}
