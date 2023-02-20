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
    public class CaseTrsBIZ : CommonBIZ
    {
        string TodayYYYMMDD = "";
        CaseTrsCommonBIZ pCaseTrsCommonBIZ = new CaseTrsCommonBIZ();

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
        //    CaseTrsBIZ Biz = new CaseTrsBIZ();
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

        ///
        /// 上傳EXCEL
        /// <summary>
        /// 
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
                //統一編號	帳號	基資 幣別	交易明細	區間起日(YYYY/MM/DD)	區間迄日(YYYY/MM/DD)
                for (int colIdx = 0; colIdx <= 7; colIdx++)
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
                        for (int colIdx = exlRow.FirstCellNum; colIdx <= 7; colIdx++)   //exlRow.LastCellNum 每一個欄位做迴圈
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
                                err = err + colIdx.ToString() + "-" + exlRow.GetCell(colIdx).ToString() + ";";
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
            if (no == 1)
            {
                if ((fd.Length != 8) && (fd.Length != 10) && (fd.Length != 11))
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
            string pMaxTrnNum = "";
            // 查詢C外來文案件狀態爲未處理資料
            DataTable dt = GetCaseTrsQueryData();

            TodayYYYMMDD = DateTime.Now.ToString("yyyyMMdd");

            // 查詢本月最大的流水號
            string pCSFSMaxTrnNum = GetMaxTrnNum();
            if (pCSFSMaxTrnNum != "")
            {
                pMaxTrnNum = pCSFSMaxTrnNum.Substring(pCSFSMaxTrnNum.Length - 5, 5);
            }

            // 流水號變量
            int pTrnNum = 0;

            // 截取流水號
            if (pMaxTrnNum != "")
            {
                pTrnNum = Convert.ToInt32(pMaxTrnNum);
            }

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
                        // 記錄CaseTrsQuery主鍵值/sql語句變量
                        string strLastNewID = "";
                        string updateSql = "";

                        // 記錄不符合查詢迄日的案件編號
                        //StringBuilder notWorkNoList = new StringBuilder("");

                        //int diffDay = GetParmCodeEndDateDiff();

                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            string dtQDateS = "";
                            string dtQDateE = "";

                            if (dt.Rows[i]["QDateS"] != null && dt.Rows[i]["QDateS"].ToString().Trim() != "")
                            {
                                dtQDateS = dt.Rows[i]["QDateS"].ToString().Substring(0, 4) + "/" + dt.Rows[i]["QDateS"].ToString().Substring(4, 2) + "/" + dt.Rows[i]["QDateS"].ToString().Substring(6, 2);
                            }

                            if (dt.Rows[i]["QDateE"] != null && dt.Rows[i]["QDateE"].ToString().Trim() != "")
                            {
                                dtQDateE = dt.Rows[i]["QDateE"].ToString().Substring(0, 4) + "/" + dt.Rows[i]["QDateE"].ToString().Substring(4, 2) + "/" + dt.Rows[i]["QDateE"].ToString().Substring(6, 2);
                            }

                            Boolean processFlag = false;

                            //if (string.IsNullOrEmpty(dtQDateS) || string.IsNullOrEmpty(dtQDateE))
                            //{
                            //   // 不查交易明細，繼續作業

                            if (dt.Rows.Count == 0)
                            {
                                processFlag = false;
                            }
                            else
                            {
                                processFlag = true;
                            }
  
                            if (processFlag)
                            {
                                // 重查才有可能用到
                                //updateSql += @" update BOPS000401Send 
                                //                set sendstatus = '02'
	                               //                 ,QueryErrMsg = ''
                                //                    ,ModifiedUser = @CreatedUser
                                //                    ,ModifiedDate = getdate() 
                                //            WHERE VersionNewID = @VersionNewID401" + i.ToString() + @" 
                                //            and sendstatus = '03';
                                //            update BOPS060490Send 
                                //                set sendstatus = '02'
                                //                    ,QueryErrMsg = ''
                                //                    ,ModifiedUser = @CreatedUser
                                //                    ,ModifiedDate = getdate() 
                                //            WHERE VersionNewID = @VersionNewID401" + i.ToString() + @" 
                                //            and sendstatus = '03'; ";

                                //base.Parameter.Add(new CommandParameter("@VersionNewID401" + i.ToString(), caseid));

                                #region insert 或 update  BOPS060628Send/BOPS067050Send
                                // 當OpenFlag=“Y”時，更新BOPS060628Send
                                // 基本資料=OpenFlag =Y Adam 2020-02-10 只要有CUST ID都先儲存一筆６７０５０
                                // if (dt.Rows[i]["OpenFlag"] != null && dt.Rows[i]["OpenFlag"].ToString() == "Y" && dt.Rows[i]["CustId"].ToString().Length > 7)
                                if (dt.Rows[i]["CustId"].ToString().Length > 7  )
                                {
                                    #region BOPS060628Send
                                    // 判斷資料是否存在與BOPS060628Send，存在就update，否則就insert
                                    // SendStatus = 2 
                                    //CodeTypeDesc CodeNo  CodeDesc
                                    //案件狀態    00  新建檔
                                    //案件狀態    01  未處理
                                    //案件狀態    02  拋查中
                                    //案件狀態    03  成功
                                    //案件狀態    04  失敗
                                    //案件狀態    06  重查拋查中
                                    //案件狀態    07  重查成功
                                    //案件狀態    08  重查失敗
                                    //案件狀態    66  已處理 / 強制結案
                                    //案件狀態    99  txt檔案格式錯誤
                                    //案件狀態    77  未獲取回文字號
                                    //案件狀態    98  di檔案格式錯誤
                                    // SendNewID 是第一次發查自動給號
                                    // 重查才有可能用到
                                    //if (dt.Rows[i]["SendNewID"] != null && dt.Rows[i]["SendNewID"].ToString() != "")
                                    //{
                                    //    updateSql += @"
                                    //            update BOPS060628Send
                                    //            set SendStatus = '02'
                                    //            	,QueryErrMsg = ''
                                    //                ,ModifiedUser = @CreatedUser
                                    //                ,ModifiedDate = getdate()
                                    //            where NewID = @60628NewID" + i.ToString() +
                                    //            " and SendStatus = '03'; ";

                                    //    base.Parameter.Add(new CommandParameter("@60628NewID" + i.ToString(), dt.Rows[i]["SendNewID"]));
                                    //}
                                    //else
//                                    {
//                                        updateSql += @" insert BOPS060628Send
//                                             (
//                                             NewID
//                                             ,VersionNewID
//                                             , CustIdNo
//                                             , CreatedUser
//                                             , CreatedDate
//                                             , ModifiedUser
//                                             , ModifiedDate
//                                             , SendStatus
//                                             )
//                                             values(
//                                             newid()
//                                             ,@60628VersionNewID" + i.ToString() + @"
//                                             , @CustIdNo" + i.ToString() + @"
//                                             , @CreatedUser
//                                             , getdate()
//                                             , @CreatedUser
//                                             , getdate()
//                                             , '02'
//                                             ); ";
//                                        base.Parameter.Add(new CommandParameter("@60628VersionNewID" + i.ToString(), dt.Rows[i]["NewID"]));//改成 NewID 不是 CaseId
//                                        base.Parameter.Add(new CommandParameter("@CustIdNo" + i.ToString(), dt.Rows[i]["CustId"])); //IDNO
//                                    }

                                    #endregion

                                    #region BOPS067050Send
                                    if (dt.Rows[i]["CustID"].ToString().Length > 7  &&  dt.Rows[i]["CustAccount"].ToString().Length < 11)
                                    {
                                        updateSql += @" insert BOPS067050Send
                                             (
                                             NewID
                                             ,VersionNewID
                                             , CustIdNo
                                             , Optn
                                             , CreatedUser
                                             , CreatedDate
                                             , ModifiedUser
                                             , ModifiedDate
                                             ,SendStatus
                                             )
                                             values(
                                             @BOPS067050SendNewID" + i.ToString() + @"
                                             , @67050VersionNewID" + i.ToString() + @"
                                             , @67050CustIdNo" + i.ToString() + @"
                                             , 'D'
                                             , @CreatedUser
                                             , getdate()
                                             , @CreatedUser
                                             , getdate()
                                             , '02'
                                             ); ";
                                        base.Parameter.Add(new CommandParameter("@BOPS067050SendNewID" + i.ToString(), Guid.NewGuid()));
                                        base.Parameter.Add(new CommandParameter("@67050VersionNewID" + i.ToString(), dt.Rows[i]["NewID"]));//改 NewID caseid
                                        base.Parameter.Add(new CommandParameter("@67050CustIdNo" + i.ToString(), dt.Rows[i]["CustID"]));//IDNO
                                    } 
                                    #endregion
                                }
                                #region /// 只要有 帳號 都是只打000401
                                if ( dt.Rows[i]["CustAccount"].ToString().Length > 11) 
                                {
                                         updateSql += @"
                                         insert BOPS000401Send
                                        (
                                        NewID
                                        ,VersionNewID
                                        ,AcctNo
                                        ,Currency
                                        ,CreatedUser
                                        ,CreatedDate
                                        ,ModifiedUser
                                        ,ModifiedDate
                                        ,SendStatus
                                        )
                                          values(
                                              @BOP000401SendNewID" + i.ToString() + @"
                                             , @000401VersionNewID" + i.ToString() + @"
                                             , @000401AcctNo" + i.ToString() + @"
                                             , @000401Currency" + i.ToString() + @"
                                             , @CreatedUser
                                             , getdate()
                                             , @CreatedUser
                                             , getdate()
                                             , '02'
                                             ); ";// 先寫死TWD
                                         base.Parameter.Add(new CommandParameter("@BOP000401SendNewID" + i.ToString(), Guid.NewGuid()));
                                         base.Parameter.Add(new CommandParameter("@000401VersionNewID" + i.ToString(), dt.Rows[i]["NewID"]));//改 NewID caseid
                                         base.Parameter.Add(new CommandParameter("@000401AcctNo" + i.ToString(), dt.Rows[i]["CustAccount"]));//AcctNO
                                         if (dt.Rows[i]["Currency"].ToString().Length != 3)
                                         {
                                             base.Parameter.Add(new CommandParameter("@000401Currency" + i.ToString(), "TWD"));//Currency
                                         }
                                         else
                                         {
                                             base.Parameter.Add(new CommandParameter("@000401Currency" + i.ToString(), dt.Rows[i]["Currency"]));//Currency
                                         }
                                }
                                #endregion
                                #endregion
                                System.Text.RegularExpressions.Regex re = new System.Text.RegularExpressions.Regex("[A-Z|a-z]$");
                                //if (re.IsMatch(outputcode))
                                string eng = "";
                                if (dt.Rows[i]["CustID"] != null && dt.Rows[i]["CustID"].ToString().Length > 7)
                                {
                                     eng = dt.Rows[i]["CustID"].ToString().Substring(dt.Rows[i]["CustID"].ToString().Length - 3, 3);
                                }
                                #region BOPS067050V4Send
                                if (dt.Rows[i]["CustID"] != null &&  dt.Rows[i]["CustID"].ToString().Length > 7  && re.IsMatch(eng))
                                {
                                    updateSql += @" insert BOPS067050V4Send
                                             (
                                             NewID
                                             ,VersionNewID
                                             , CustIdNo
                                             , Optn
                                             , CreatedUser
                                             , CreatedDate
                                             , ModifiedUser
                                             , ModifiedDate
                                             ,SendStatus
                                             )
                                             values(
                                             @BOPS067050V4SendNewID" + i.ToString() + @"
                                             , @67050V4VersionNewID" + i.ToString() + @"
                                             , @67050V4CustIdNo" + i.ToString() + @"
                                             , '4'
                                             , @CreatedUser
                                             , getdate()
                                             , @CreatedUser
                                             , getdate()
                                             , '02'
                                             ); ";
                                    base.Parameter.Add(new CommandParameter("@BOPS067050V4SendNewID" + i.ToString(), Guid.NewGuid()));
                                    base.Parameter.Add(new CommandParameter("@67050V4VersionNewID" + i.ToString(), dt.Rows[i]["NewID"]));//改 NewID caseid
                                    base.Parameter.Add(new CommandParameter("@67050V4CustIdNo" + i.ToString(), dt.Rows[i]["CustID"]));//IDNO
                                }
                                #endregion

                                #region BOPS067050V6Send
                                if (dt.Rows[i]["CustID"] != null && dt.Rows[i]["CustID"].ToString().Length == 8)
                                {
                                    updateSql += @" insert BOPS067050V6Send
                                             (
                                             NewID
                                             ,VersionNewID
                                             , CustIdNo
                                             , Optn
                                             , CreatedUser
                                             , CreatedDate
                                             , ModifiedUser
                                             , ModifiedDate
                                             ,SendStatus
                                             )
                                             values(
                                             @BOPS067050V6SendNewID" + i.ToString() + @"
                                             , @67050V6VersionNewID" + i.ToString() + @"
                                             , @67050V6CustIdNo" + i.ToString() + @"
                                             , '6'
                                             , @CreatedUser
                                             , getdate()
                                             , @CreatedUser
                                             , getdate()
                                             , '02'
                                             ); ";
                                    base.Parameter.Add(new CommandParameter("@BOPS067050V6SendNewID" + i.ToString(), Guid.NewGuid()));
                                    base.Parameter.Add(new CommandParameter("@67050V6VersionNewID" + i.ToString(), dt.Rows[i]["NewID"]));//改 NewID caseid
                                    base.Parameter.Add(new CommandParameter("@67050V6CustIdNo" + i.ToString(), dt.Rows[i]["CustID"]));//IDNO
                                }
                                #endregion
                                #region insert 或 update CaseTrsRFDMSend
                                if (dt.Rows[i]["TransactionFlag"] != null && dt.Rows[i]["TransactionFlag"].ToString() == "Y")
                                {
                                    DataTable dtRFDMSend = QueryRFDMSend(dt.Rows[i]["NewID"].ToString());

                                    if (dtRFDMSend == null || dtRFDMSend.Rows.Count == 0)
                                    {
                                        updateSql += @" insert CaseTrsRFDMSend
                                             (
                                             TrnNum, VersionNewID, ID_No,Acct_No,Type, RFDMSendStatus, CreatedUser, CreatedDate, ModifiedUser, ModifiedDate,acctDesc,curr,channel,Start_Jnrst_Date,End_Jnrst_Date
                                             )
                                             values(
                                             @TrnNumS" + i.ToString() + @"
                                             , @RFDMVersionNewIDS" + i.ToString() + @"
                                             , @RFDMCustIdNoS" + i.ToString() + @"
                                             , @RFDMAccTNoS" + i.ToString() + @"
                                             , '0'
                                             , '02'
                                             , @CreatedUser
                                             , getdate()
                                             , @CreatedUser
                                             , getdate()
                                             ,'S' 
                                             , @CurrencyS" + i.ToString() + @"
                                             ,'CSFS' ";

                                        if (dtQDateS != "")
                                        {
                                            updateSql += ",'" + dtQDateS + "'";
                                        }
                                        else
                                        {
                                            updateSql += ",NULL";
                                        }
                                        if (dtQDateE != "")
                                        {
                                            updateSql += ",'" + dtQDateE + "'";
                                        }
                                        else
                                        {
                                            updateSql += ",NULL";
                                        }

                                        updateSql += " ); ";

                                        updateSql += @" insert CaseTrsRFDMSend
                                             (
                                             TrnNum, VersionNewID, ID_No,Acct_No, Type, RFDMSendStatus, CreatedUser, CreatedDate, ModifiedUser, ModifiedDate,acctDesc,curr,channel,Start_Jnrst_Date,End_Jnrst_Date
                                             )
                                             values(
                                             @TrnNumQ" + i.ToString() + @"
                                             , @RFDMVersionNewIDQ" + i.ToString() + @"
                                             , @RFDMCustIdNoQ" + i.ToString() + @"
                                             , @RFDMAccTNoQ" + i.ToString() + @"
                                             , '0'
                                             , '02'
                                             , @CreatedUser
                                             , getdate()
                                             , @CreatedUser
                                             , getdate()
                                             ,'Q' 
                                             , @CurrencyQ" + i.ToString() + @"
                                             ,'CSFS' ";

                                        if (dtQDateS != "")
                                        {
                                            updateSql += ",'" + dtQDateS + "'";
                                        }
                                        else
                                        {
                                            updateSql += ",NULL";
                                        }
                                        if (dtQDateE != "")
                                        {
                                            updateSql += ",'" + dtQDateE + "'";
                                        }
                                        else
                                        {
                                            updateSql += ",NULL";
                                        }

                                        updateSql += " ); ";

                                        updateSql += @" insert CaseTrsRFDMSend
                                             (
                                             TrnNum, VersionNewID, ID_No,Acct_No, Type, RFDMSendStatus, CreatedUser, CreatedDate, ModifiedUser, ModifiedDate,acctDesc,curr,channel,Start_Jnrst_Date,End_Jnrst_Date
                                             )
                                             values(
                                             @TrnNumT" + i.ToString() + @"
                                             , @RFDMVersionNewIDT" + i.ToString() + @"
                                             , @RFDMCustIdNoT" + i.ToString() + @"
                                             , @RFDMAccTNoT" + i.ToString() + @"
                                             , '0'
                                             , '02'
                                             , @CreatedUser
                                             , getdate()
                                             , @CreatedUser
                                             , getdate()
                                             ,'T' 
                                             , @CurrencyT" + i.ToString() + @"
                                             ,'CSFS' ";

                                        if (dtQDateS != "")
                                        {
                                            updateSql += ",'" + dtQDateS + "'";
                                        }
                                        else
                                        {
                                            updateSql += ",NULL";
                                        }
                                        if (dtQDateE != "")
                                        {
                                            updateSql += ",'" + dtQDateE + "'";
                                        }
                                        else
                                        {
                                            updateSql += ",NULL";
                                        }

                                        updateSql += " ); ";

                                        base.Parameter.Add(new CommandParameter("@TrnNumS" + i.ToString(),  "CSFS"+CalculateTrnNum(pTrnNum)));
                                        base.Parameter.Add(new CommandParameter("@RFDMCustIdNoS" + i.ToString(), dt.Rows[i]["CustId"]));
                                        base.Parameter.Add(new CommandParameter("@RFDMVersionNewIDS" + i.ToString(), dt.Rows[i]["NewID"]));
                                        base.Parameter.Add(new CommandParameter("@CurrencyS" + i.ToString(), dt.Rows[i]["Currency"]));
                                        if (dt.Rows[i]["CustAccount"].ToString().Length > 11)
                                        {
                                            base.Parameter.Add(new CommandParameter("@RFDMAccTNoS" + i.ToString(), dt.Rows[i]["CustAccount"].ToString().PadLeft(17, '0')));
                                        }
                                        else
                                        {
                                            base.Parameter.Add(new CommandParameter("@RFDMAccTNoS" + i.ToString(), ""));
                                        }
                                        pTrnNum++;

                                        base.Parameter.Add(new CommandParameter("@TrnNumQ" + i.ToString(),  "CSFS"+CalculateTrnNum(pTrnNum)));
                                        base.Parameter.Add(new CommandParameter("@RFDMCustIdNoQ" + i.ToString(), dt.Rows[i]["CustId"]));
                                        base.Parameter.Add(new CommandParameter("@RFDMVersionNewIDQ" + i.ToString(), dt.Rows[i]["NewID"]));
                                        base.Parameter.Add(new CommandParameter("@CurrencyQ" + i.ToString(), dt.Rows[i]["Currency"]));
                                        if (dt.Rows[i]["CustAccount"].ToString().Length > 11)
                                        {
                                            base.Parameter.Add(new CommandParameter("@RFDMAccTNoQ" + i.ToString(), dt.Rows[i]["CustAccount"].ToString().PadLeft(17, '0')));
                                        }
                                        else
                                        {
                                            base.Parameter.Add(new CommandParameter("@RFDMAccTNoQ" + i.ToString(), ""));
                                        }
                                        pTrnNum++;

                                        base.Parameter.Add(new CommandParameter("@TrnNumT" + i.ToString(),  "CSFS"+CalculateTrnNum(pTrnNum)));
                                        base.Parameter.Add(new CommandParameter("@RFDMCustIdNoT" + i.ToString(), dt.Rows[i]["CustId"]));
                                        base.Parameter.Add(new CommandParameter("@RFDMVersionNewIDT" + i.ToString(), dt.Rows[i]["NewID"]));
                                        base.Parameter.Add(new CommandParameter("@CurrencyT" + i.ToString(), dt.Rows[i]["Currency"]));
                                        if (dt.Rows[i]["CustAccount"].ToString().Length > 11)
                                        {
                                            base.Parameter.Add(new CommandParameter("@RFDMAccTNoT" + i.ToString(), dt.Rows[i]["CustAccount"].ToString().PadLeft(17, '0')));
                                        }
                                        else
                                        {
                                            base.Parameter.Add(new CommandParameter("@RFDMAccTNoT" + i.ToString(), ""));
                                        }
                                        pTrnNum++;
                                    }
                                    else
                                    {
                                        DataRow[] drArr = dtRFDMSend.Select("acctDesc='S'");

                                        if (drArr.Length > 0)
                                        {
                                            updateSql += @"
                                                update CaseTrsRFDMSend
                                                set RFDMSendStatus = '02'
                                                	,RspMsg = ''
                                                    ,ModifiedUser = @CreatedUser
                                                    ,ModifiedDate = getdate()
                                                where TrnNum = @TrnNumS" + i.ToString() +
                                                 " and RFDMSendStatus = '03'; ";

                                            base.Parameter.Add(new CommandParameter("@TrnNumS" + i.ToString(), drArr[0]["TrnNum"].ToString()));
                                        }
                                        else
                                        {
                                            updateSql += @" insert CaseTrsRFDMSend
                                             (
                                             TrnNum, VersionNewID, ID_No,Acct_No, Type, RFDMSendStatus, CreatedUser, CreatedDate, ModifiedUser, ModifiedDate,acctDesc,curr,channel,Start_Jnrst_Date,End_Jnrst_Date
                                             )
                                             values(
                                             @TrnNumS" + i.ToString() + @"
                                             , @RFDMVersionNewIDS" + i.ToString() + @"
                                             , @RFDMCustIdNoS" + i.ToString() + @"
                                             , @RFDMAccTNoS" + i.ToString() + @"
                                             , '0'
                                             , '02'
                                             , @CreatedUser
                                             , getdate()
                                             , @CreatedUser
                                             , getdate()
                                             ,'S' ,'TWD','CSFS' ";

                                            if (dtQDateS != "")
                                            {
                                                updateSql += ",'" + dtQDateS + "'";
                                            }
                                            else
                                            {
                                                updateSql += ",NULL";
                                            }
                                            if (dtQDateE != "")
                                            {
                                                updateSql += ",'" + dtQDateE + "'";
                                            }
                                            else
                                            {
                                                updateSql += ",NULL";
                                            }

                                            updateSql += " ); ";

                                            base.Parameter.Add(new CommandParameter("@TrnNumS" + i.ToString(),  "CSFS"+CalculateTrnNum(pTrnNum)));
                                            base.Parameter.Add(new CommandParameter("@RFDMCustIdNoS" + i.ToString(), dt.Rows[i]["CustId"]));
                                            base.Parameter.Add(new CommandParameter("@RFDMVersionNewIDS" + i.ToString(), dt.Rows[i]["NewID"]));
                                            if (dt.Rows[i]["CustAccount"].ToString().Length > 7)
                                            {
                                                base.Parameter.Add(new CommandParameter("@RFDMAccTNoS" + i.ToString(), dt.Rows[i]["CustAccount"].ToString().PadLeft(17, '0')));
                                            }
                                            else
                                            {
                                                base.Parameter.Add(new CommandParameter("@RFDMAccTNoS" + i.ToString(), ""));
                                            }
                                            pTrnNum++;
                                        }
                                    }
//                                        DataRow[] drArrQ = dtRFDMSend.Select("acctDesc='Q'");

//                                        if (drArrQ.Length > 0)
//                                        {
//                                            updateSql += @"
//                                                update CaseTrsRFDMSend
//                                                set RFDMSendStatus = '02'
//                                                	,RspMsg = ''
//                                                    ,ModifiedUser = @CreatedUser
//                                                    ,ModifiedDate = getdate()
//                                                where TrnNum = @TrnNumQ" + i.ToString() +
//                                                 " and RFDMSendStatus = '03'; ";

//                                            base.Parameter.Add(new CommandParameter("@TrnNumQ" + i.ToString(), drArrQ[0]["TrnNum"].ToString()));
//                                        }
//                                        else
//                                        {
//                                            updateSql += @" insert CaseTrsRFDMSend
//                                             (
//                                             TrnNum, VersionNewID, ID_No, Acct_No,Type, RFDMSendStatus, CreatedUser, CreatedDate, ModifiedUser, ModifiedDate,acctDesc,curr,channel,Start_Jnrst_Date,End_Jnrst_Date
//                                             )
//                                             values(
//                                             @TrnNumQ" + i.ToString() + @"
//                                             , @RFDMVersionNewIDQ" + i.ToString() + @"
//                                             , @RFDMCustIdNoQ" + i.ToString() + @"
//                                             , @RFDMAcctNoQ"+ i.ToString()+@"
//                                             , '0'
//                                             , '02'
//                                             , @CreatedUser
//                                             , getdate()
//                                             , @CreatedUser
//                                             , getdate()
//                                             ,'Q','TWD','CSFS' ";

//                                            if (dtQDateS != "")
//                                            {
//                                                updateSql += ",'" + dtQDateS + "'";
//                                            }
//                                            else
//                                            {
//                                                updateSql += ",NULL";
//                                            }
//                                            if (dtQDateE != "")
//                                            {
//                                                updateSql += ",'" + dtQDateE + "'";
//                                            }
//                                            else
//                                            {
//                                                updateSql += ",NULL";
//                                            }

//                                            updateSql += " ); ";

//                                            base.Parameter.Add(new CommandParameter("@TrnNumQ" + i.ToString(),  "CSFS"+CalculateTrnNum(pTrnNum)));
//                                            base.Parameter.Add(new CommandParameter("@RFDMCustIdNoQ" + i.ToString(), dt.Rows[i]["CustId"]));
//                                            base.Parameter.Add(new CommandParameter("@RFDMVersionNewIDQ" + i.ToString(), dt.Rows[i]["NewID"]));
//                                            if (dt.Rows[i]["CustAccount"].ToString().Length > 7)
//                                            {
//                                                base.Parameter.Add(new CommandParameter("@RFDMAccTNoQ" + i.ToString(), dt.Rows[i]["CustAccount"].ToString().PadLeft(17, '0')));
//                                            }
//                                            else
//                                            {
//                                                base.Parameter.Add(new CommandParameter("@RFDMAccTNoQ" + i.ToString(), ""));
//                                            }
//                                            pTrnNum++;
//                                        }

//                                        DataRow[] drArrT = dtRFDMSend.Select("acctDesc='T'");

//                                        if (drArrT.Length > 0)
//                                        {
//                                            updateSql += @"
//                                                update CaseTrsRFDMSend
//                                                set RFDMSendStatus = '02'
//                                                	,RspMsg = ''
//                                                    ,ModifiedUser = @CreatedUser
//                                                    ,ModifiedDate = getdate()
//                                                where TrnNum = @TrnNumT" + i.ToString() +
//                                                 " and RFDMSendStatus = '03'; ";

//                                            base.Parameter.Add(new CommandParameter("@TrnNumT" + i.ToString(), drArrT[0]["TrnNum"].ToString()));
//                                        }
//                                        else
//                                        {

//                                            updateSql += @" insert CaseTrsRFDMSend
//                                             (
//                                             TrnNum, VersionNewID, ID_No, Type, RFDMSendStatus, CreatedUser, CreatedDate, ModifiedUser, ModifiedDate,acctDesc,curr,channel,Start_Jnrst_Date,End_Jnrst_Date
//                                             )
//                                             values(
//                                             @TrnNumT" + i.ToString() + @"
//                                             , @RFDMVersionNewIDT" + i.ToString() + @"
//                                             , @RFDMCustIdNoT" + i.ToString() + @"
//                                             , @RFDMAcctNoT" + i.ToString() + @"
//                                             , '0'
//                                             , '02'
//                                             , @CreatedUser
//                                             , getdate()
//                                             , @CreatedUser
//                                             , getdate()
//                                             ,'T','TWD','CSFS' ";

//                                            if (dtQDateS != "")
//                                            {
//                                                updateSql += ",'" + dtQDateS + "'";
//                                            }
//                                            else
//                                            {
//                                                updateSql += ",NULL";
//                                            }
//                                            if (dtQDateE != "")
//                                            {
//                                                updateSql += ",'" + dtQDateE + "'";
//                                            }
//                                            else
//                                            {
//                                                updateSql += ",NULL";
//                                            }

//                                            updateSql += " ); ";

//                                            base.Parameter.Add(new CommandParameter("@TrnNumT" + i.ToString(),  "CSFS"+CalculateTrnNum(pTrnNum)));
//                                            base.Parameter.Add(new CommandParameter("@RFDMCustIdNoT" + i.ToString(), dt.Rows[i]["CustId"]));
//                                            base.Parameter.Add(new CommandParameter("@RFDMVersionNewIDT" + i.ToString(), dt.Rows[i]["NewID"]));
//                                            if (dt.Rows[i]["CustAccount"].ToString().Length > 7)
//                                            {
//                                                base.Parameter.Add(new CommandParameter("@RFDMAccTNoT" + i.ToString(), dt.Rows[i]["CustAccount"].ToString().PadLeft(17, '0')));
//                                            }
//                                            else
//                                            {
//                                                base.Parameter.Add(new CommandParameter("@RFDMAccTNoT" + i.ToString(), ""));
//                                            }
//                                            pTrnNum++;
//                                        }
                                    //}

                                    #region CaseTrsRFDMSend

                                    #endregion
                                }

                                #endregion

                                #region  update  CaseTrsQueryVersion
                                // adam 取消先拋查中,改為未處理
                                updateSql += @"
                                    update  CaseTrsQueryVersion
                                    set Status = '01' ";

                                // 更新HTGSendStatus欄位，'OpenFlag為Y AND (HTGSendStatus 為 0 或6) ,則存2，否則存空
                                if (dt.Rows[i]["OpenFlag"] != null && dt.Rows[i]["OpenFlag"].ToString() == "Y"
                                    && (dt.Rows[i]["HTGSendStatus"] != null &&
                                    (dt.Rows[i]["HTGSendStatus"].ToString() == "0" || dt.Rows[i]["HTGSendStatus"].ToString() == "6")))
                                {
                                    updateSql += @",HTGSendStatus = '2'
                                               ,HTGQryMessage = ''
                                               ,HTGModifiedDate = getdate() ";
                                }

                                // 更新RFDMSendStatus欄位，'TransactionFlag為Y AND (RFDMSendStatus 為 0 或6) ,則存2，否則存空
                                if (dt.Rows[i]["TransactionFlag"] != null && dt.Rows[i]["TransactionFlag"].ToString() == "Y"
                                    && (dt.Rows[i]["RFDMSendStatus"] != null
                                    && (dt.Rows[i]["RFDMSendStatus"].ToString() == "0" || dt.Rows[i]["RFDMSendStatus"].ToString() == "6")))
                                {
                                    updateSql += @",RFDMSendStatus = '2' 
                                               ,RFDMQryMessage = ''
                                               ,RFDModifiedDate = getdate() ";
                                }


                                updateSql += @"
                                            ,ModifiedDate = getdate()
                                            ,ModifiedUser = @CreatedUser 
                                    where NewID = @NewID" + i.ToString() + "; ";

                                base.Parameter.Add(new CommandParameter("@NewID" + i.ToString(), dt.Rows[i]["NewID"]));

                                #endregion

                                //#region update  CaseTrsQuery
                                //// 判斷該筆資料與上筆資料是否屬於同一個案件，如果不屬於，就更新該資料所屬案件的案件狀態欄位
                                //if (strLastNewID != dt.Rows[i]["CaseTrsNewID"].ToString())
                                //{
                                //    // CaseTrsQuery狀態改爲拋查中
                                //    updateSql += @"
                                //    update  CaseTrsQuery
                                //    set Status = '02'
                                //        ,QueryUserID = @CreatedUser
                                //        ,ModifiedDate = getdate()
                                //        ,ModifiedUser = @CreatedUser
                                //    where NewID = @CaseTrsNewID" + i.ToString() + "; ";

                                //    base.Parameter.Add(new CommandParameter("@CaseTrsNewID" + i.ToString(), dt.Rows[i]["CaseTrsNewID"]));
                                //}
                                //#endregion

                                strLastNewID = dt.Rows[i]["CaseTrsNewID"].ToString();

                            }
                        }

                        // 若有可發查案件，才更新表格
                        if (!string.IsNullOrEmpty(updateSql))
                        {
                            base.Parameter.Add(new CommandParameter("@CreatedUser", pUserAccount));

                            base.ExecuteNonQuery(updateSql, dbTrans);

                            // insert或update ApprMsgKey表
                            SaveMsg(dt, logonUser, dbTrans);

                        }

                        dbTrans.Commit();

                        // 回傳全部都有發查
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
                                ,CaseTrsQueryVersion.NewID
                            	,CaseTrsQueryVersion.CaseTrsNewID
                                ,CaseTrsQueryVersion.CustId
                                ,CaseTrsQueryVersion.CustAccount
                                ,CaseTrsQueryVersion.Currency
                            	,isnull(CaseTrsQueryVersion.OpenFlag,'') as OpenFlag
                            	,isnull(CaseTrsQueryVersion.TransactionFlag,'') as TransactionFlag
                            	,isnull(CaseTrsQueryVersion.HTGSendStatus,'') as HTGSendStatus
                            	,isnull(CaseTrsQueryVersion.RFDMSendStatus,'') as RFDMSendStatus
                            	--,BOPS060628Send.NewID as SendNewID
                            	--,BOPS060628Send.SendStatus
                            	--,BOPS067050Send.NewID as BOPS067050SendNewID
                                ,CaseTrsQueryVersion.QDateS
                                ,CaseTrsQueryVersion.QDateE
                            from CaseTrsQueryVersion
                            inner join CaseMaster
                            on CaseMaster.CaseId = CaseTrsQueryVersion.CaseTrsNewId
                            -- left join BOPS060628Send
                            --on BOPS060628Send.VersionNewID = CaseTrsQueryVersion.NewID
                            --left join BOPS067050Send
                           -- on BOPS067050Send.VersionNewID = CaseTrsQueryVersion.NewID
                            where CaseTrsQueryVersion.Status IN ('00') -- 新增資料
                            order by CaseMaster.CaseNo,CaseTrsQueryVersion.CaseTrsNewID,CaseTrsQueryVersion.QDateE desc ";

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

        public int GetDailyData(string CustNo,string AcctNo)
        {
            try
            {
                int DataCount = 0;
                if (CustNo.Length < 8) CustNo = "ZZZZZZZZ";
                if (AcctNo.Length < 12) AcctNo = "ZZZZZZZZZZZZ";

                string sql = @"
                            select NewId
                             from CaseTrsQueryVersion                          
                            WHERE  ( CustId = '" + CustNo + "' or  substring(CustAccount,6,12) = '" + AcctNo + "') and CONVERT(VARCHAR(10),CreatedDate,111) = CONVERT(VARCHAR(10),getdate(),111) ";                               

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
    
        public bool InsertCaseTrsQueryVersion(DataTable dt, string CaseNo, string caseid, User logonUser, string Option1, string filename, IDbTransaction trans)
        {
            // 取得連接并開放連接
            IDbConnection dbConnection = base.OpenConnection();

            // 定義事務
            IDbTransaction dbTransaction = null;
            using (dbConnection)
            {
                // 開啟事務
                // adam 20210413 改每筆
                //dbTransaction = dbConnection.BeginTransaction();

                string strTrnNum = "";
                string strsql = "";
                try
                {
                    for (int j = 0; j < dt.Rows.Count; j++)
                    {
                        if ((string.IsNullOrEmpty(dt.Rows[j][0].ToString())))
                        {
                            break;
                        }
                        dbTransaction = dbConnection.BeginTransaction();
                        strTrnNum = "CSFS" + DateTime.Now.ToString("yyyyMMddHHmmssf");
                        strsql = @"INSERT INTO CaseTRsQueryVersion    ( NewID ,CaseTrsNewID ,CustId ,CustAccount ,Currency, OpenFlag , TransactionFlag ,QDateS ,QDateE , Status ,HTGSendStatus , RFDMSendStatus , CreatedDate , CreatedUser ,ModifiedDate , ModifiedUser,Seq,DocNo)
     VALUES
	 (@NewID,@CaseTrsNewID,@CustId,@CustAccount,@Currency,@OpenFlag,@TransactionFlag,@ForCDateS,@ForCDateE,@Status,@HTGSendStatus,@RFDMSendStatus,getdate(),@CreatedUser,getdate(),@CreatedUser,@Seq,@DocNo)";
                        Parameter.Clear();
                        //編號 統一編號	帳號	基資	交易明細	區間起日(YYYY/MM/DD)	區間迄日(YYYY/MM/DD)
                        Parameter.Add(new CommandParameter("@NewID", Guid.NewGuid()));
                        Parameter.Add(new CommandParameter("@CaseTrsNewID", caseid));
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
                        ExecuteNonQuery(strsql, dbTransaction);
                        dbTransaction.Commit();                        
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
               // dbTransaction.Commit();
                return true;
            }
        }

        public int insertCaseTrsQueryHistory(CaseTrsCondition model, IDbTransaction trans, string filename)
        {
            try
            {
                string sql = @"INSERT INTO  [CaseTrsQueryHistory] ([NewID],[DocNo],[RecvDate],[QFileName]) VALUES  (@NewID,@DocNo,getdate(),@QFileName)";
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
