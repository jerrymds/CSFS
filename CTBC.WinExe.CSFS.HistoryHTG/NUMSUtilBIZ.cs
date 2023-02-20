using System;
using System.Data;
using System.Data.SqlClient;
using CTCB.NUMS.Models;
//using CTCB.NUMS.Pattern;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using Microsoft.VisualBasic;
//using CTCB.NUMS.Util;
//using CTCB.NUMS.Library.HttpWebProcess;
using System.Text;
using CTBC.CSFS.BussinessLogic;
using CTBC.CSFS.Pattern;

namespace CTCB.NUMS.Models
{
    public class NUMSUtilBIZ :  BaseBusinessRule
    {
        #region 全域變數

        private string _user = "admin";

        private string _dataSource = "";
        private string _initCatalog = "";
        private string _userID = "";
        private string _passWord = "";

        #endregion

        #region 屬性設置(Get,Set)

        #endregion

        #region Private Method

        /// <summary>
        /// 
        /// </summary>
        /// <param name="strTableName"></param>
        /// <param name="dr"></param>
        /// <param name="strInsert"></param>
        /// <param name="cmdparm"></param>
        private void CreateInsertSql(string strTableName, DataRow dr, ref string strInsert, ref CommandParameterCollection cmdparm)
        {
            //產生 Insert SQL
            string strInsertCol = "";
            string strInsertParm = "";

            foreach (DataColumn clmData in dr.Table.Columns)
            {

                string strColumnName = clmData.ColumnName.ToUpper();
                switch (strColumnName)
                {
                    case "NEWID":
                    case "CREATEDDATE":
                        //case "MODIFIEDUSER":
                        break;
                    default:
                        strInsertCol += "," + strColumnName;
                        strInsertParm += ",@" + strColumnName;
                        cmdparm.Add(new CommandParameter("@" + strColumnName, dr[strColumnName]));
                        break;

                }


            }

            strInsertCol = strInsertCol.Substring(1);
            strInsertParm = strInsertParm.Substring(1);

            string[] strInsertCols = strInsertCol.Split(new string[] { "," }, StringSplitOptions.None);

            //產生命令
            strInsert = "INSERT INTO " + strTableName + " (" + strInsertCol + ") VALUES (" + strInsertParm + ")";

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
            strInsert = "Update " + strTableName + " Set " + strUpdateCol + " Where NewId=@NewId";

        }
        #region add by shenqixing 20161110
        //ISGFR0199 shenqixing added 20161110 start

        /// <summary>
        /// Update DataRow
        /// </summary>
        /// <param name="tablename"></param>
        /// <param name="dr"></param>
        public void UpdateDataPcsmRow(string tablename, DataRow dr, IDbTransaction dbTransaction = null)
        {

            if (dbTransaction != null)
            {
                UpdateDataRowWithTransaction(dr, tablename, dbTransaction);
                return;
            }

            // 連接數據庫
            IDbConnection dbConnection = base.OpenConnection();
            bool innertransaction = false;

            if (dbTransaction == null)
                innertransaction = true;

            string sql = "";

            if (tablename == "")
                throw new Exception("no TableName!!");

            // DB連接
            using (dbConnection)
            {
                try
                {
                    if (innertransaction == true)
                        // 開始事務
                        dbTransaction = dbConnection.BeginTransaction();

                    CommandParameterCollection cmdparm = new CommandParameterCollection();
                    CreateUpdatePcsmSql(tablename, dr, ref sql, ref  cmdparm);
                    base.Parameter.Clear();
                    foreach (CommandParameter cmp in cmdparm)
                    {
                        base.Parameter.Add(cmp);
                    }

                    base.ExecuteNonQuery(sql, dbTransaction, false);

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
        /// <summary>
        /// create insert sql command
        /// </summary>
        /// <param name="strTableName"></param>
        /// <param name="dr"></param>
        /// <param name="strInsert"></param>
        /// <param name="cmdparm"></param>
        private void CreateUpdatePcsmSql(string strTableName, DataRow dr, ref string strInsert, ref CommandParameterCollection cmdparm)
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
            strInsert = "Update " + strTableName + " Set " + strUpdateCol + " WHERE ApplNo =@ApplNo and ApplNoB=@ApplNoB and StepID=@StepID and UserID= @UserID";

        }

        //add by shenqixing 20161206 start 
        public void UpdateDataRowWithTransaction(DataRow dr, string tablename, IDbTransaction dbTransaction = null)
        {

            // 連接數據庫
            IDbConnection dbConnection = base.OpenConnection();
            bool innertransaction = false;

            if (dbTransaction == null)
                innertransaction = true;

            string sql = "";

            if (tablename == "")
                throw new Exception("no TableName!!");


            // DB連接

            try
            {
                // 開始事務
                if (innertransaction == true)
                    dbTransaction = dbConnection.BeginTransaction();

                if (innertransaction == true)
                    // 開始事務
                    dbTransaction = dbConnection.BeginTransaction();

                CommandParameterCollection cmdparm = new CommandParameterCollection();
                CreateUpdatePcsmSql(tablename, dr, ref sql, ref  cmdparm);
                base.Parameter.Clear();
                foreach (CommandParameter cmp in cmdparm)
                {
                    base.Parameter.Add(cmp);
                }

                base.ExecuteNonQuery(sql, dbTransaction, false);

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

        public void UpdateDataTable(DataTable rtable, IDbTransaction dbTransaction = null)
        {

            if (dbTransaction != null)
            {
                UpdateDataTableWithTransaction(rtable, dbTransaction);
                return;
            }

            // 連接數據庫
            IDbConnection dbConnection = base.OpenConnection();
            bool innertransaction = false;

            if (dbTransaction == null)
                innertransaction = true;

            string sql = "";

            if (rtable.TableName.Trim() == "" || rtable.TableName.ToUpper() == "TABLE")
                throw new Exception("DataTable has no TableName!!");

            // DB連接
            using (dbConnection)
            {
                try
                {
                    // 開始事務
                    if (innertransaction == true)
                        dbTransaction = dbConnection.BeginTransaction();

                    foreach (DataRow dr in rtable.Rows)
                    {
                        CommandParameterCollection cmdparm = new CommandParameterCollection();
                        CreateUpdateSql(rtable.TableName, dr, ref sql, ref  cmdparm);
                        base.Parameter.Clear();
                        foreach (CommandParameter cmp in cmdparm)
                        {
                            base.Parameter.Add(cmp);
                        }

                        base.ExecuteNonQuery(sql, dbTransaction, false);
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

        public void UpdateDataTableWithTransaction(DataTable rtable, IDbTransaction dbTransaction = null)
        {

            // 連接數據庫
            IDbConnection dbConnection = base.OpenConnection();
            bool innertransaction = false;

            if (dbTransaction == null)
                innertransaction = true;

            string sql = "";

            if (rtable.TableName.Trim() == "" || rtable.TableName.ToUpper() == "TABLE")
                throw new Exception("DataTable has no TableName!!");

            // DB連接

            try
            {
                // 開始事務
                if (innertransaction == true)
                    dbTransaction = dbConnection.BeginTransaction();

                foreach (DataRow dr in rtable.Rows)
                {
                    CommandParameterCollection cmdparm = new CommandParameterCollection();
                    CreateUpdateSql(rtable.TableName, dr, ref sql, ref  cmdparm);
                    base.Parameter.Clear();
                    foreach (CommandParameter cmp in cmdparm)
                    {
                        base.Parameter.Add(cmp);
                    }

                    base.ExecuteNonQuery(sql, dbTransaction, false);
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

        //add by shenqixing 20161206 start 


        //ISGFR0199 shenqixing added 20161110 end
        #endregion

        private void CreateUpdateSql(string strTableName, Dictionary<string, string> keyfield, DataRow dr, ref string strInsert, ref CommandParameterCollection cmdparm)
        {
            //產生 Insert SQL
            string strUpdateCol = "";
            string fieldsql = "";

            foreach (DataColumn clmData in dr.Table.Columns)
            {

                string strColumnName = clmData.ColumnName.ToUpper();
                //int keyidx = keyfield.ContainsKey(strColumnName);
                if (keyfield.ContainsKey(strColumnName))
                {
                    cmdparm.Add(new CommandParameter("@" + strColumnName, dr[strColumnName]));
                }
                else
                {
                    strUpdateCol += "," + strColumnName + "=@" + strColumnName;
                    cmdparm.Add(new CommandParameter("@" + strColumnName, dr[strColumnName]));
                }
            }

            strUpdateCol = strUpdateCol.Substring(1);

            fieldsql = " where 1=1 ";

            foreach (string key in keyfield.Keys)
            {
                fieldsql += " And " + key + "=@" + key;
            }
            //for (int i = 0; i < keyfield.Count; i++)
            //{
            //    fieldsql += " And " + keyfield[i].ToString() + "=@" + keyfield[i].ToString();
            //}

            //產生命令
            strInsert = "Update " + strTableName + " Set " + strUpdateCol + fieldsql;

        }

        //add by mel 20130828
        public void CreateSelectSql(string strTableName, Dictionary<string, string> keyfield, DataRow dr, ref string selectsql, ref CommandParameterCollection cmdparm)
        {

            //Dictionary<string, string> keyfieldtype;
            //keyfieldtype = new Dictionary<string, string>();
            //CreateSelectSql(strTableName, keyfield, keyfieldtype, dr, ref selectsql, ref cmdparm);

            //產生 select  SQL

            string fieldsql = "";
            string conditionsql = "";

            cmdparm = new CommandParameterCollection();
            selectsql = "";

            foreach (DataColumn clmData in dr.Table.Columns)
            {

                string strColumnName = clmData.ColumnName;
                if (keyfield.ContainsKey(strColumnName))
                {
                    if (dr[strColumnName] == System.DBNull.Value)
                        conditionsql += " and " + strColumnName + " is null ";
                    else
                        conditionsql += " and " + strColumnName + "=@" + strColumnName;


                    //cmdparm.Add(new CommandParameter("@" + strColumnName, dr[strColumnName] == System.DBNull.Value ? null : dr[strColumnName]));
                    cmdparm.Add(new CommandParameter("@" + strColumnName, dr[strColumnName]));
                }


            }



            //fieldsql = " where 1=1 ";

            foreach (string key in keyfield.Keys)
            {
                fieldsql += "," + key;
            }
            fieldsql = fieldsql.Substring(1);
            //產生命令
            selectsql = "select  " + fieldsql + " from " + strTableName + " where 1=1 " + conditionsql;

        }

        public void CreateSelectSql(string strTableName, Dictionary<string, string> keyfield, Dictionary<string, string> keyfieldttype, DataRow dr, ref string selectsql, ref CommandParameterCollection cmdparm)
        {
            //產生 select  SQL

            string fieldsql = "";
            string conditionsql = "";

            cmdparm = new CommandParameterCollection();
            selectsql = "";

            foreach (DataColumn clmData in dr.Table.Columns)
            {

                string strColumnName = clmData.ColumnName;
                if (keyfield.ContainsKey(strColumnName))
                {
                    if (keyfieldttype.ContainsKey(strColumnName))
                    {
                        //有設定type的處理
                        switch (keyfieldttype[strColumnName])
                        {
                            case "X":
                                conditionsql += " and isnull(" + strColumnName + ",'') =@" + strColumnName;
                                break;
                            case "N":
                                conditionsql += " and isnull(" + strColumnName + ",0) =@" + strColumnName;
                                break;
                            default:
                                //沒有設定type的處理
                                conditionsql += " and " + strColumnName + "=@" + strColumnName;
                                break;
                        }
                    }
                    else
                    {
                        //沒有設定type的處理
                        conditionsql += " and " + strColumnName + "=@" + strColumnName;
                    }

                    cmdparm.Add(new CommandParameter("@" + strColumnName, dr[strColumnName]));
                }


            }



            //fieldsql = " where 1=1 ";

            foreach (string key in keyfield.Keys)
            {
                fieldsql += "," + key;
            }
            fieldsql = fieldsql.Substring(1);
            //產生命令
            selectsql = "select  " + fieldsql + " from " + strTableName + " where 1=1 " + conditionsql;

        }



        /// <summary>
        /// 產生sql 資料
        /// </summary>
        /// <param name="dtblTable"></param>
        /// <returns></returns>
        private void MakeInsertSQL(string targettable, string sourcetable, DataRow[] fieldrow, ref string insertssql, ref string selectsql, ref string deletesql)
        {
            insertssql = "";
            string strvalue = "";
            selectsql = "";
            deletesql = "";

            deletesql = "delete  from " + targettable + " where ApplNo=@ApplNo and ApplNoB=@ApplNoB";
            selectsql = "select ";
            insertssql = "INSERT INTO " + targettable + " (";
            for (int i = 0; i < fieldrow.Length; i++)
            {
                if (i != 0)
                {
                    insertssql += ",";
                    selectsql += ",";
                }
                insertssql += fieldrow[i]["TargetField"].ToString();
                selectsql += fieldrow[i]["SourceField"].ToString();
                if (i != 0)
                {
                    strvalue += ",";
                }
                strvalue += "@" + fieldrow[i]["TargetField"].ToString();
            }

            insertssql += ") VALUES (" + strvalue + ")";
            selectsql += " from  " + sourcetable;


        }



        #endregion

        #region Public Method

        /// <summary>
        /// 構造函數
        /// </summary>
        /// <param name="dataSource">數據源</param>
        /// <param name="initCatalog">數據庫</param>
        /// <param name="userID">用戶</param>
        /// <param name="passWord">密碼</param>
        public NUMSUtilBIZ(string dataSource, string initCatalog, string userID, string passWord)
            : base("Data Source=" + dataSource + ";Initial Catalog=" + initCatalog + ";Persist Security Info=True;User ID=" + userID + ";PASSWORD=" + passWord + ";")
        {
            _dataSource = dataSource;
            _initCatalog = initCatalog;
            _userID = userID;
            _passWord = passWord;
        }

        /// <summary>
        /// 構造函數
        /// </summary>
        /// <param name="connectstring">連接字串</param>
        public NUMSUtilBIZ(string connectstring)
            : base(connectstring)
        {

            string[] parmht = connectstring.Split(new string[] { ";" }, StringSplitOptions.None);
            foreach (string item in parmht)
            {

                string[] key = item.Split(new string[] { "=" }, StringSplitOptions.None);
                switch (key[0].ToUpper())
                {
                    case "DATA SOURCE":
                        _dataSource = key[1];
                        break;
                    case "INITIAL CATALOG":
                        _initCatalog = key[1];
                        break;
                    case "USER ID":
                        _userID = key[1];
                        break;
                    case "PASSWORD":
                        _passWord = key[1];
                        break;
                }

            }
        }

        /// <summary>
        /// 無帶入connection
        /// </summary>
        public NUMSUtilBIZ() { }
        /// <summary>
        /// 得到Email信息
        /// </summary>
        /// <returns>Email信息</returns>
        public DataTable SetEmail()
        {
            string sql = @"SELECT 
                               * 
                           FROM 
                               PARMCode 
                           WHERE 
                               CODETYPE = 'MAIL_IMAGE_ERR'";

            return base.Search(sql);
        }

        /// <summary>
        /// 取出新序號值
        /// </summary>
        /// <param name="strSeqKey"></param>
        /// <returns></returns>
        public DataTable GetNUMSSeqTable(string strSeqKey)
        {

            try
            {
                DataTable returnValue = new DataTable();
                string sql = "select t.* from NUMSDataTransferSetting t where t.SeqType = @strSeqType and t.SeqKey = @strSeqKey";
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@strSeqKey", strSeqKey));
                returnValue = base.Search(sql);
                return returnValue;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 取出資料庫中各種轉檔的設定值
        /// </summary>
        /// <returns></returns>
        public DataTable GetDataTransferSetting()
        {
            try
            {
                DataTable returnValue = new DataTable();
                string sql = "select t.* from PARMDataTransferSetting t order by TransferType,SourceTable,sortOrder";
                base.Parameter.Clear();
                returnValue = base.Search(sql);
                return returnValue;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <remarks>add by mel 20130822 IR-62319</remarks>
        public DataTable GetDataTransferSettingForBatch()
        {
            try
            {
                DataTable returnValue = new DataTable();
                string sql = @"select t.* from PARMDataTransferSetting t 
                                where TransferType in 
                                (select TransferType from PARMBatchProcess where EnableFlag='Y' )
                                order by TransferType,SourceTable,sortOrder";
                base.Parameter.Clear();
                returnValue = base.Search(sql);
                return returnValue;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 取出資料庫中要轉檔的table
        /// </summary>
        /// <returns></returns>
        public DataTable GetDataTransferTable()
        {
            try
            {
                DataTable returnValue = new DataTable();
                string sql = @"select distinct TransferType,SourceTable,TargetTable , MAX(tableorder ) tableorder 
                            from PARMDataTransferSetting 
                            group by  TransferType,SourceTable,TargetTable 
                            order by MAX(tableorder) ,TransferType,SourceTable,TargetTable";
                base.Parameter.Clear();
                returnValue = base.Search(sql);
                return returnValue;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <remarks>add by mel 20130822 IR-62319</remarks>
        public DataTable GetDataTransferTableForBatch()
        {
            try
            {
                DataTable returnValue = new DataTable();
                string sql = @"select distinct TransferType,SourceTable,TargetTable from PARMDataTransferSetting
                                where TransferType in 
                                (select TransferType from PARMBatchProcess where EnableFlag='Y' )";
                base.Parameter.Clear();
                returnValue = base.Search(sql);
                return returnValue;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 取出客戶資訊
        /// </summary>
        /// <param name="strApplno"></param>
        /// <param name="strApplnoB"></param>
        /// <returns></returns>
        public DataTable GetNUMSCustomerInfo(string strApplno, string strApplnoB)
        {
            try
            {
                DataTable returnValue = new DataTable();
                string sql = "select distinct CusId from NUMSCustomerInfo where  applno =@strApplno and applnob =@strApplnob and isnull(Status,'Y')='Y' ";
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@strApplno", strApplno));
                base.Parameter.Add(new CommandParameter("@strApplnob", strApplnoB));
                returnValue = base.Search(sql);
                return returnValue;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }



        public string GetNUMSCustomerInfoMastID(string strApplno, string strApplnoB)
        {
            string result = "";
            try
            {
                DataTable returnValue = new DataTable();
                string sql = "select distinct CusId from NUMSCustomerInfo where  applno =@strApplno and applnob =@strApplnob and isnull(Status,'Y')='Y' and  LoanRelation ='1'";
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@strApplno", strApplno));
                base.Parameter.Add(new CommandParameter("@strApplnob", strApplnoB));
                returnValue = base.Search(sql);
                if (returnValue.Rows.Count > 0)
                    result = returnValue.Rows[0]["CusId"].ToString();


            }
            catch (Exception ex)
            {
                throw ex;
            }

            return result;
        }
        public DataTable GetRuleStepidList(string bustype, string stepid)
        {
            try
            {
                DataTable returnValue = new DataTable();
                string sql = "select distinct  stepid from RULEPolicy where BusType=@BusType and StepId like @StepId order by stepid";
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@StepId", stepid + "-%"));
                base.Parameter.Add(new CommandParameter("@BusType", bustype));
                returnValue = base.Search(sql);
                return returnValue;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Get ID to Query
        /// </summary>
        /// <param name="strApplno"></param>
        /// <param name="strApplnoB"></param>
        /// <returns></returns>
        public DataTable GetQueryID(string strApplno, string strApplnoB)
        {
            try
            {
                DataTable returnValue = new DataTable();
                string sql = @"select distinct a.CusId ,a.GuaranteeSeqNo ,a.LoanRelation ,b.ApplTypeCode from NUMSCustomerInfo a 
                                left outer join NUMSMaster b on a.ApplNo=b.ApplNo and a.ApplNoB =b.ApplNoB
                                where a.applno =@strApplno and a.applnob =@strApplnob   
                                and ISNULL(a.Status, 'Y') ='Y' order by a.GuaranteeSeqNo";
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@strApplno", strApplno));
                base.Parameter.Add(new CommandParameter("@strApplnob", strApplnoB));
                returnValue = base.Search(sql);
                return returnValue;
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
        /// 依傳入的參數讀取table
        /// </summary>
        /// <param name="strSQL"></param>
        /// <param name="parm"></param>
        /// <returns></returns>
        public DataTable OpenDataTable(string strSQL, Hashtable parm)
        {
            DataTable returnValue = new DataTable();

            try
            {
                base.Parameter.Clear();
                foreach (string key in parm.Keys)
                {
                    base.Parameter.Add(new CommandParameter(key, parm[key].ToString()));
                }
                returnValue = base.Search(strSQL);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return returnValue;
        }

        /// <summary>
        /// 依傳入的參數讀取table
        /// </summary>
        /// <param name="tablename"></param>
        /// <param name="applno"></param>
        /// <param name="applnob"></param>
        /// <param name="filter"></param>
        /// <param name="sortorder"></param>
        /// <returns></returns>
        public DataTable OpenDataTable(string tablename, string applno, string applnob, string filter, string sortorder)
        {
            DataTable returnValue = new DataTable();

            string strSQL = "Select * from " + tablename + " where ApplNo=@ApplNo and ApplNoB=@ApplNoB ";
            if (filter.Length > 0)
            { strSQL += " and " + filter; }

            if (sortorder.Length > 0)
            { strSQL += " order by  " + sortorder; }

            try
            {
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@Applno", applno));
                base.Parameter.Add(new CommandParameter("@ApplnoB", applnob));
                returnValue = base.Search(strSQL);
                returnValue.TableName = tablename;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return returnValue;
        }

        /// <summary>
        /// 依傳入的參數讀取table
        /// </summary>
        /// <param name="tablename"></param>
        /// <param name="applno"></param>
        /// <param name="applnob"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public DataTable OpenDataTable(string tablename, string applno, string applnob, string filter)
        {
            DataTable returnValue = new DataTable();

            string strSQL = "Select * from " + tablename + " where ApplNo=@ApplNo and ApplNoB=@ApplNoB ";

            if (filter.Length > 0)
            { strSQL += " and " + filter; }

            try
            {
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@Applno", applno));
                base.Parameter.Add(new CommandParameter("@ApplnoB", applnob));
                returnValue = base.Search(strSQL);
                returnValue.TableName = tablename;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return returnValue;
        }

        /// <summary>
        /// 依傳入的參數讀取table
        /// </summary>
        /// <param name="tablename"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public DataTable OpenDataTable(string tablename, string filter)
        {
            DataTable returnValue = new DataTable();

            string strSQL = "Select * from " + tablename + " where 1=1 ";
            if (filter.Length > 0)
            { strSQL += " and " + filter; }

            try
            {
                base.Parameter.Clear();
                returnValue = base.Search(strSQL);
                returnValue.TableName = tablename;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return returnValue;
        }


        /// <summary>
        /// 依傳入的參數讀取table
        /// </summary>
        /// <param name="tablename"></param>
        /// <param name="filter"></param>
        /// <param name="sortorder"></param>
        /// <returns></returns>
        public DataTable OpenDataTable(string tablename, string filter, string sortorder)
        {
            DataTable returnValue = new DataTable();

            string strSQL = "Select * from " + tablename + " where 1=1  ";
            if (filter.Length > 0)
            { strSQL += " and " + filter; }

            if (sortorder.Length > 0)
            { strSQL += " order by  " + sortorder; }

            try
            {
                base.Parameter.Clear();
                returnValue = base.Search(strSQL);
                returnValue.TableName = tablename;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return returnValue;
        }

        /// <summary>
        /// insert to database 
        /// </summary>
        /// <param name="rtable">要寫入的資料集</param>
        public void InsertIntoTable(DataTable rtable)
        {
            InsertIntoTable(rtable, null);
            /*
            // 連接數據庫
            IDbConnection dbConnection = base.OpenConnection();
            IDbTransaction dbTransaction = null;
            string sql = "";

            if (rtable.TableName.Trim() == "" || rtable.TableName.ToUpper() == "TABLE")
                throw new Exception("DataTable has no TableName!!");

            // DB連接
            using (dbConnection)
            {
                try
                {
                    // 開始事務
                    dbTransaction = dbConnection.BeginTransaction();

                    foreach (DataRow dr in rtable.Rows)
                    {
                        CommandParameterCollection cmdparm = new CommandParameterCollection();
                        CreateInsertSql(rtable.TableName, dr, ref sql, ref  cmdparm);
                        base.Parameter.Clear();
                        foreach (CommandParameter cmp in cmdparm)
                        {
                            base.Parameter.Add(cmp);
                        }

                        base.ExecuteNonQuery(sql, dbTransaction, false);
                    }


                    dbTransaction.Commit();


                }
                catch (Exception ex)
                {
                    dbTransaction.Rollback();

                    throw ex;
                }
            }
             **/
        }

        /// <summary>
        /// edit by mel 20130819  	IR-62275 add IDbTransaction
        /// </summary>
        /// <param name="rtable"></param>
        /// <param name="dbTransaction"></param>
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



            //IDbTransaction dbTransaction = null;
            //string sql = "";

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



            //IDbTransaction dbTransaction = null;
            //string sql = "";

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
        /// add by mel 20130827
        /// </summary>
        /// <param name="rtable"></param>
        /// <param name="dbTransaction"></param>
        public void InsertIntoTable(String tablename, DataRow row, IDbTransaction dbTransaction = null)
        {
            DataTable rtable = new DataTable();
            rtable = row.Table.Clone();
            rtable.TableName = tablename;
            rtable.ImportRow(row);
            InsertIntoTable(rtable, dbTransaction);

        }

        /// <summary>
        /// insert to database 
        /// </summary>
        /// <param name="rDataTables"></param>
        public void InsertIntoTable(System.Collections.Generic.SortedList<string, DataTable> rDataTables)
        {

            // 連接數據庫
            IDbConnection dbConnection = base.OpenConnection();
            IDbTransaction dbTransaction = null;
            string sql = "";

            using (dbConnection)
            {
                // 開始事務
                dbTransaction = dbConnection.BeginTransaction();
                try
                {
                    foreach (string key in rDataTables.Keys)
                    {
                        DataTable rtable = rDataTables[key];
                        if (rtable.TableName.Trim() == "" || rtable.TableName.ToUpper() == "TABLE")
                            throw new Exception("DataTable has no TableName!!");



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

                        //foreach (DataRow dr in rtable.Rows)
                        //{
                        //    sql = "";
                        //    CommandParameterCollection cmdparm = new CommandParameterCollection();
                        //    CreateInsertSql(rtable.TableName, dr, ref sql, ref  cmdparm);
                        //    base.Parameter.Clear();
                        //    foreach (CommandParameter cmp in cmdparm)
                        //    {
                        //        base.Parameter.Add(cmp);
                        //    }
                        //    base.ExecuteNonQuery(sql, dbTransaction, false);
                        //}
                    }
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
        /// 20150930
        /// </summary>
        /// <param name="table"></param>
        /// <param name="Applno"></param>
        /// <param name="ApplnoB"></param>
        /// <param name="CustID"></param>
        /// <param name="Module"></param>
        public void DeleteTableForJCICRaw(DataRow[] Rows, string Applno, string ApplnoB, string CustID, string Module)
        {
            System.Text.StringBuilder sql = new System.Text.StringBuilder();

            if (Rows.Length > 0)
            {
                for (int i = 0; i < Rows.Length; i++)
                {
                    sql.Append(@" DELETE FROM " + Rows[i]["AtomTable"] + " " +
                                  "Where ApplNo = @Applno and ApplnoB = @ApplnoB and CustID = @CustID and Module = @Module;");
                }
            }

            // 連接數據庫
            IDbConnection dbConnection = base.OpenConnection();
            IDbTransaction dbTransaction = null;

            using (dbConnection)
            {
                // 開始事務
                dbTransaction = dbConnection.BeginTransaction();
                try
                {
                    base.Parameter.Clear();
                    base.Parameter.Add(new CommandParameter("@Applno", Applno));
                    base.Parameter.Add(new CommandParameter("@ApplnoB", ApplnoB));
                    base.Parameter.Add(new CommandParameter("@CustID", CustID));
                    base.Parameter.Add(new CommandParameter("@Module", Module));
                    base.ExecuteNonQuery(sql.ToString(), dbTransaction, false);

                    dbTransaction.Commit();
                }
                catch (Exception ex)
                {
                    try
                    {
                        dbTransaction.Rollback();
                    }
                    catch (Exception ex2)
                    {
                        // This catch block will handle any errors that may have occurred
                        // on the server that would cause the rollback to fail, such as
                        // a closed connection.
                    }
                    throw ex;
                }
            }
        }
        /// <summary>
        /// insert to database 
        /// </summary>
        /// <param name="rDataTables"></param>
        public void InsertIntoTableForJCIC(System.Collections.Generic.SortedList<string, DataTable> rDataTables)
        {

            // 連接數據庫
            IDbConnection dbConnection = base.OpenConnection();
            IDbTransaction dbTransaction = null;
            //string sql = "";

            using (dbConnection)
            {
                // 開始事務
                dbTransaction = dbConnection.BeginTransaction();
                try
                {
                    foreach (string key in rDataTables.Keys)
                    {
                        DataTable rtable = rDataTables[key];
                        if (rtable.TableName.Trim() == "" || rtable.TableName.ToUpper() == "TABLE")
                            throw new Exception("DataTable has no TableName!!");
                        if (rtable.Rows.Count == 0)
                        { continue; }

                        //先刪除舊資料
                        //sql = "DELETE FROM " + rtable.TableName + "  ";
                        //sql += " WHERE Applno = @Applno and ApplnoB=@ApplnoB ";
                        //sql += " and CustID = @CustID and Module=@Module ";

                        //base.Parameter.Clear();

                        //base.Parameter.Add(new CommandParameter("@Applno", rtable.Rows[0]["Applno"].ToString()));
                        //base.Parameter.Add(new CommandParameter("@ApplnoB", rtable.Rows[0]["ApplnoB"].ToString()));
                        //base.Parameter.Add(new CommandParameter("@CustID", rtable.Rows[0]["CustID"].ToString()));
                        //base.Parameter.Add(new CommandParameter("@Module", rtable.Rows[0]["Module"].ToString()));
                        //base.ExecuteNonQuery(sql, dbTransaction, false);


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

                        //foreach (DataRow dr in rtable.Rows)
                        //{
                        //    sql = "";
                        //    CommandParameterCollection cmdparm = new CommandParameterCollection();
                        //    CreateInsertSql(rtable.TableName, dr, ref sql, ref  cmdparm);
                        //    base.Parameter.Clear();
                        //    foreach (CommandParameter cmp in cmdparm)
                        //    {
                        //        base.Parameter.Add(cmp);
                        //    }
                        //    base.ExecuteNonQuery(sql, dbTransaction, false);
                        //}
                    }
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
        /// insert to database 
        /// </summary>
        /// <param name="rDataTables"></param>
        public void InsertIntoTableForJCICByModule(System.Collections.Generic.SortedList<string, DataTable> rDataTables)
        {

            // 連接數據庫
            IDbConnection dbConnection = base.OpenConnection();
            IDbTransaction dbTransaction = null;
            string sql = "";

            using (dbConnection)
            {
                // 開始事務
                dbTransaction = dbConnection.BeginTransaction();
                try
                {
                    foreach (string key in rDataTables.Keys)
                    {
                        DataTable rtable = rDataTables[key];
                        if (rtable.TableName.Trim() == "" || rtable.TableName.ToUpper() == "TABLE")
                            throw new Exception("DataTable has no TableName!!");
                        if (rtable.Rows.Count == 0)
                        { continue; }

                        //先刪除舊資料
                        sql = "DELETE FROM " + rtable.TableName + "  ";
                        sql += " WHERE Applno = @Applno and ApplnoB=@ApplnoB ";
                        sql += " and CustID=@CustID";
                        sql += " and Module=@Module ";

                        base.Parameter.Clear();

                        base.Parameter.Add(new CommandParameter("@Applno", rtable.Rows[0]["Applno"].ToString()));
                        base.Parameter.Add(new CommandParameter("@ApplnoB", rtable.Rows[0]["ApplnoB"].ToString()));
                        base.Parameter.Add(new CommandParameter("@CustID", rtable.Rows[0]["CustID"].ToString()));
                        base.Parameter.Add(new CommandParameter("@Module", rtable.Rows[0]["Module"].ToString()));
                        base.ExecuteNonQuery(sql, dbTransaction, false);

                        foreach (DataRow dr in rtable.Rows)
                        {
                            sql = "";
                            CommandParameterCollection cmdparm = new CommandParameterCollection();
                            CreateInsertSql(rtable.TableName, dr, ref sql, ref  cmdparm);
                            base.Parameter.Clear();
                            foreach (CommandParameter cmp in cmdparm)
                            {
                                base.Parameter.Add(cmp);
                            }
                            base.ExecuteNonQuery(sql, dbTransaction, false);
                        }
                    }
                    dbTransaction.Commit();
                }
                catch (Exception ex)
                {
                    dbTransaction.Rollback();
                    throw ex;
                }
            }

        }

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="rtable"></param>
        ///// <param name="transactionflag">是否使用transaction</param>
        //public void InsertIntoTable(DataTable rtable ,bool transactionflag)
        //{
        //    if (transactionflag)
        //    {
        //        InsertIntoTable(rtable);
        //        return;
        //    }
        //    // 連接數據庫
        //    IDbConnection dbConnection = base.OpenConnection();
        //    IDbTransaction dbTransaction = null;
        //    string sql = "";

        //    if (rtable.TableName.Trim() == "" || rtable.TableName.Trim() == "Table")
        //        throw new Exception("DataTable has no TableName!!");

        //    // DB連接
        //    using (dbConnection)
        //    {
        //        try
        //        {
        //            // 開始事務
        //            dbTransaction = dbConnection.BeginTransaction();

        //            foreach (DataRow dr in rtable.Rows)
        //            {
        //                CommandParameterCollection cmdparm = new CommandParameterCollection();
        //                CreateInsertSql(rtable.TableName, dr, ref sql, ref  cmdparm);
        //                base.Parameter.Clear();
        //                foreach (CommandParameter cmp in cmdparm)
        //                {
        //                    base.Parameter.Add(cmp);
        //                }

        //                base.ExecuteNonQuery(sql, dbTransaction, false);
        //            }


        //            dbTransaction.Commit();


        //        }
        //        catch (Exception ex)
        //        {
        //            dbTransaction.Rollback();

        //            throw ex;
        //        }
        //    }
        //}


        public void CSCInsertToDisp(System.Collections.Generic.SortedList<string, DataTable> rDataTables)
        {

            // 連接數據庫
            IDbConnection dbConnection = base.OpenConnection();
            IDbTransaction dbTransaction = null;
            string sql = "";
            string applno = rDataTables["DISPMaster"].Rows[0]["Applno"].ToString();
            string applnob = rDataTables["DISPMaster"].Rows[0]["ApplnoB"].ToString();

            using (dbConnection)
            {
                // 開始事務
                dbTransaction = dbConnection.BeginTransaction();
                try
                {
                    foreach (string key in rDataTables.Keys)
                    {
                        DataTable rtable = rDataTables[key];
                        if (rtable.TableName.Trim() == "" || rtable.TableName.ToUpper() == "TABLE")
                            throw new Exception("DataTable has no TableName!!");
                        if (rtable.Rows.Count == 0)
                        { continue; }

                        //先刪除舊資料
                        sql = "DELETE FROM " + rtable.TableName + "  ";
                        sql += " WHERE Applno = @Applno and ApplnoB=@ApplnoB ";

                        base.Parameter.Clear();
                        base.Parameter.Add(new CommandParameter("@Applno", applno));
                        base.Parameter.Add(new CommandParameter("@ApplnoB", applnob));
                        base.ExecuteNonQuery(sql, dbTransaction, false);

                        InsertIntoTable(rtable, dbTransaction);



                    }

                    //update DISPAddCaseByCSC.ImportFlag to "Y"

                    sql = @"Update DISPAddCaseByCSC set ImportFlag='Y'   WHERE Applno = @Applno and ApplnoB=@ApplnoB ";

                    base.Parameter.Clear();
                    base.Parameter.Add(new CommandParameter("@Applno", applno));
                    base.Parameter.Add(new CommandParameter("@ApplnoB", applnob));
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
        // PCSM-CU6 shenqixing added 20161117 start
        /// <summary>
        /// 重算之后更新UDWRMaster
        /// </summary>
        /// <param name="strMessage"></param>
        /// <param name="strApplno"></param>
        /// <param name="strApplnoB"></param>
        public void UpdateUDWRMaster(ref string strMessage, string strApplno, string strApplnoB, bool isSave, ref DataTable queryDt, IDbTransaction dbTransaction = null)
        {
            // 連接數據庫
            // IDbConnection dbConnection = base.OpenConnection();
            //IDbTransaction dbTransaction = null;

            //using (dbConnection)
            //{
            // 開始事務
            //dbTransaction = dbConnection.BeginTransaction();
            try
            {
                string strsql = string.Empty;
                string TableName = "PCSM_JCU6_LOG";
                if (!isSave)
                {
                    TableName = TableName + "_TEMP";
                    strsql = @"declare @SECFINALCO varchar(10)
                                      declare @SECADDAPPROVALFLAG varchar(10)
                                      declare @SECADDAPPROVALCO varchar(10)
 
                        set @SECFINALCO=case when (SELECT TOP 1 CodeNo FROM PARMCode  WHERE CodeDesc=ISNULL((SELECT TOP 1 SECFINALCO  FROM  " + TableName + @" WHERE SRCApplno = @ApplNo and SRCApplnoB=@ApplNoB order by [QUERY_DATE] desc),'') AND CodeType='PCSM_SecFinalCo')='' then '0' else isnull((SELECT TOP 1 CodeNo FROM PARMCode  WHERE CodeDesc=ISNULL((SELECT TOP 1 SECFINALCO  FROM  " + TableName + @"   WHERE SRCApplno = @ApplNo and SRCApplnoB=@ApplNoB  order by [QUERY_DATE] desc ),'') AND CodeType='PCSM_SecFinalCo'),0) end
                        set @SECADDAPPROVALFLAG=(select top 1 SECADDAPPROVALFLAG from  " + TableName + @" where SRCAPPLNO=@ApplNo and SRCAPPLNOB=@ApplNoB order by [QUERY_DATE] desc)
                        set @SECADDAPPROVALCO=case when (SELECT TOP 1 CodeNo FROM PARMCode  WHERE CodeDesc=ISNULL((SELECT TOP 1 SECADDAPPROVALCO  FROM  " + TableName + @"   WHERE SRCApplno = @ApplNo and SRCApplnoB=@ApplNoB order by [QUERY_DATE] desc),'') AND CodeType='PCSM_SecFinalCo')='' then '0' else isnull((SELECT TOP 1 CodeNo FROM PARMCode  WHERE CodeDesc=ISNULL((SELECT TOP 1 SECADDAPPROVALCO  FROM  " + TableName + @"   WHERE SRCApplno = @ApplNo and SRCApplnoB=@ApplNoB  order by [QUERY_DATE] desc),'') AND CodeType='PCSM_SecFinalCo'),0) end


 SELECT 
                                        (case when @SECFINALCO >= @SECADDAPPROVALCO then @SECFINALCO else @SECADDAPPROVALCO end) AS CalTopCreditLevel, --檢核SecFinalCo需大於或等於SecAdditionalApprovalCo
                                        (CASE WHEN ISNULL(@SECADDAPPROVALFLAG, '')='Y' THEN 1
					                                      WHEN ISNULL(@SECADDAPPROVALFLAG, '')='B' THEN 1
					                                      WHEN ISNULL(@SECADDAPPROVALFLAG, '')='N' THEN 0
					                                      ELSE 0  END ) AS IsMultiCO,
                                        (CASE WHEN ISNULL(@SECADDAPPROVALFLAG, '')='Y' THEN 0
						                                       WHEN ISNULL(@SECADDAPPROVALFLAG, '')='B' THEN 1
						                                       WHEN ISNULL(@SECADDAPPROVALFLAG, '')='N' THEN 0
						                                       ELSE 0 END ) AS IsNeedCreditCO
                                    FROM UDWRMaster T
                                    inner join " + TableName + @" S ON T.ApplNO=S.SRCAPPLNO and T.ApplNoB=S.SRCAPPLNOB
                                    WHERE T.Applno = @ApplNo and T.ApplnoB=@ApplNoB";
                }
                else
                {
                    strsql = @"declare @SECFINALCO varchar(10)
                                      declare @SECADDAPPROVALFLAG varchar(10)
                                      declare @SECADDAPPROVALCO varchar(10)
 
set @SECFINALCO=case when (SELECT TOP 1 CodeNo FROM PARMCode  WHERE CodeDesc=ISNULL((SELECT TOP 1 SECFINALCO  FROM  " + TableName + @" WHERE SRCApplno = @ApplNo and SRCApplnoB=@ApplNoB order by [QUERY_DATE] desc),'') AND CodeType='PCSM_SecFinalCo')='' then '0' else isnull((SELECT TOP 1 CodeNo FROM PARMCode  WHERE CodeDesc=ISNULL((SELECT TOP 1 SECFINALCO  FROM  " + TableName + @"   WHERE SRCApplno = @ApplNo and SRCApplnoB=@ApplNoB  order by [QUERY_DATE] desc ),'') AND CodeType='PCSM_SecFinalCo'),0) end
set @SECADDAPPROVALFLAG=(select top 1 SECADDAPPROVALFLAG from  " + TableName + @" where SRCAPPLNO=@ApplNo and SRCAPPLNOB=@ApplNoB order by [QUERY_DATE] desc)
set @SECADDAPPROVALCO=case when (SELECT TOP 1 CodeNo FROM PARMCode  WHERE CodeDesc=ISNULL((SELECT TOP 1 SECADDAPPROVALCO  FROM  " + TableName + @"   WHERE SRCApplno = @ApplNo and SRCApplnoB=@ApplNoB order by [QUERY_DATE] desc),'') AND CodeType='PCSM_SecFinalCo')='' then '0' else isnull((SELECT TOP 1 CodeNo FROM PARMCode  WHERE CodeDesc=ISNULL((SELECT TOP 1 SECADDAPPROVALCO  FROM  " + TableName + @"   WHERE SRCApplno = @ApplNo and SRCApplnoB=@ApplNoB  order by [QUERY_DATE] desc),'') AND CodeType='PCSM_SecFinalCo'),0) end


UPDATE  T
                                    SET 
                                        T.CalTopCreditLevel= (case when @SECFINALCO >= @SECADDAPPROVALCO then @SECFINALCO else @SECADDAPPROVALCO end), --檢核SecFinalCo需大於或等於SecAdditionalApprovalCo
                                        T.IsMultiCO=(CASE WHEN ISNULL(@SECADDAPPROVALFLAG, '')='Y' THEN 1
					                                      WHEN ISNULL(@SECADDAPPROVALFLAG, '')='B' THEN 1
					                                      WHEN ISNULL(@SECADDAPPROVALFLAG, '')='N' THEN 0
					                                      ELSE 0  END ),
                                        T.IsNeedCreditCO=(CASE WHEN ISNULL(@SECADDAPPROVALFLAG, '')='Y' THEN 0
						                                       WHEN ISNULL(@SECADDAPPROVALFLAG, '')='B' THEN 1
						                                       WHEN ISNULL(@SECADDAPPROVALFLAG, '')='N' THEN 0
						                                       ELSE 0 END )
                                    FROM UDWRMaster T
                                    inner join " + TableName + @" S ON T.ApplNO=S.SRCAPPLNO and T.ApplNoB=S.SRCAPPLNOB
                                    WHERE T.Applno = @ApplNo and T.ApplnoB=@ApplNoB";
                }

                //回傳最高CO
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@Applno", strApplno));
                base.Parameter.Add(new CommandParameter("@ApplnoB", strApplnoB));
                if (dbTransaction != null)
                {
                    if (isSave)
                    {
                        base.ExecuteNonQuery(strsql, dbTransaction, false);
                    }
                    else
                    {
                        queryDt = new DataTable();
                        queryDt = base.Search(strsql, dbTransaction);
                    }
                }
                else
                {
                    if (isSave)
                    {
                        base.ExecuteNonQuery(strsql, false);
                    }
                    else
                    {
                        queryDt = new DataTable();
                        queryDt = base.Search(strsql);
                    }
                }

                //dbTransaction.Commit();
            }
            catch (Exception ex)
            {
                strMessage += ex.Message;
                //dbTransaction.Rollback();
                throw ex;
            }
            //}

        }

        // PCSM-CU6 shenqixing added 20161117 end

        // PCSM-ML5 shenqixing added 20161207 start
        /// <summary>
        /// 重算之后更新UDWRDistinguish 
        /// </summary>
        /// <param name="strMessage"></param>
        /// <param name="strApplno"></param>
        /// <param name="strApplnoB"></param>
        public void UpdateUDWRDistinguish(ref string strMessage, string strApplno, string strApplnoB, string strGuaraNo, bool isSave, ref string strNormalRate, IDbTransaction dbTransaction = null)
        {
            // 連接數據庫
            // IDbConnection dbConnection = base.OpenConnection();
            //IDbTransaction dbTransaction = null;

            //using (dbConnection)
            //{
            // 開始事務
            //dbTransaction = dbConnection.BeginTransaction();
            try
            {
                DataTable queryDt;
                string strsql = string.Empty;
                string TableName = "PCSM_JML5_LOG";
                if (!isSave)
                {
                    TableName = TableName + "_TEMP";
                    strsql = @"Select FinalLtvCap from " + TableName + @"
                                      WHERE srcApplno = @ApplNo and srcApplnoB=@ApplNoB and COLLATERALNO=@GuaraNo order by QUERY_DATE desc";
                }
                else
                {
                    // 入DB[NormalRate]前整數轉為小數格式 (例:79轉為0.790) 
                    strsql = @"update UDWRDistinguish set NormalRate = (select top 1 isnull(FinalLtvCap,0)/100 from " + TableName + @" WHERE srcApplno = @ApplNo and srcApplnoB=@ApplNoB and COLLATERALNO=@GuaraNo order by QUERY_DATE desc)
                                     WHERE Applno = @ApplNo and ApplnoB=@ApplNoB and GuaraNo=@GuaraNo";
                }

                //回傳最高CO
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@Applno", strApplno));
                base.Parameter.Add(new CommandParameter("@ApplnoB", strApplnoB));
                // add by shenqixing 20170215 start
                base.Parameter.Add(new CommandParameter("@GuaraNo", strGuaraNo));
                // add by shenqixing 20170215 start
                if (dbTransaction != null)
                {
                    if (isSave)
                    {
                        base.ExecuteNonQuery(strsql, dbTransaction, false);
                    }
                    else
                    {
                        queryDt = new DataTable();
                        queryDt = base.Search(strsql, dbTransaction);
                    }
                    string sql = @"Select FinalLtvCap from " + TableName + @"
                                       WHERE srcApplno = @ApplNo and srcApplnoB=@ApplNoB and COLLATERALNO=@GuaraNo order by QUERY_DATE desc";
                    queryDt = new DataTable();
                    queryDt = base.Search(sql, dbTransaction);
                    strNormalRate = "";
                    if (queryDt.Rows.Count > 0)
                    {
                        strNormalRate = queryDt.Rows[0]["FinalLtvCap"].ToString();
                    }
                }
                else
                {
                    if (isSave)
                    {
                        base.ExecuteNonQuery(strsql, false);

                    }
                    else
                    {
                        queryDt = new DataTable();
                        queryDt = base.Search(strsql);

                    }
                    string sql = @"Select FinalLtvCap from " + TableName + @"
                                    WHERE srcApplno = @ApplNo and srcApplnoB=@ApplNoB and COLLATERALNO=@GuaraNo order by QUERY_DATE desc";
                    // modify by shenqixing 20170215 start
                    queryDt = new DataTable();
                    queryDt = base.Search(sql);
                    strNormalRate = "";
                    if (queryDt.Rows.Count > 0)
                    {
                        strNormalRate = queryDt.Rows[0]["FinalLtvCap"].ToString();
                    }
                }

                //dbTransaction.Commit();
            }
            catch (Exception ex)
            {
                strMessage += ex.Message;
                //dbTransaction.Rollback();
                throw ex;
            }
            //}

        }

        // PCSM-ML5 shenqixing added 20161117 end
        /// <summary>
        /// 刪除NUMSCreditCifData
        /// </summary>
        /// <param name="strApplno"></param>
        /// <param name="strApplnoB"></param>
        /// <param name="cusID"></param>
        /// <param name="engstep"></param>
        public void DeleteNUMSCreditCifData(string strApplno, string strApplnoB, string cusID, string engstep)
        {
            // 連接數據庫
            IDbConnection dbConnection = base.OpenConnection();
            IDbTransaction dbTransaction = null;
            // DB連接
            using (dbConnection)
            {
                try
                {
                    dbTransaction = dbConnection.BeginTransaction();
                    string sql = @"DELETE
                               FROM 
	                               NUMSCreditCifData 
                               WHERE
	                               Applno = @Applno and ApplnoB=@ApplnoB and CusId=@CusId and StepId=@StepId  ";

                    base.Parameter.Clear();

                    base.Parameter.Add(new CommandParameter("@Applno", strApplno));
                    base.Parameter.Add(new CommandParameter("@ApplnoB", strApplnoB));
                    base.Parameter.Add(new CommandParameter("@cusID", cusID));
                    base.Parameter.Add(new CommandParameter("@StepId", engstep));

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

        public void DeleteNUMSCreditCifData(string strApplno, string strApplnoB, string cusID, string engstep, string module)
        {
            // 連接數據庫
            IDbConnection dbConnection = base.OpenConnection();
            IDbTransaction dbTransaction = null;
            // DB連接
            using (dbConnection)
            {
                try
                {
                    dbTransaction = dbConnection.BeginTransaction();
                    string sql = @"DELETE
                               FROM 
	                               NUMSCreditCifData 
                               WHERE
	                               Applno = @Applno and ApplnoB=@ApplnoB and CusId=@CusId and StepId=@StepId and Module=@Module  ";

                    base.Parameter.Clear();

                    base.Parameter.Add(new CommandParameter("@Applno", strApplno));
                    base.Parameter.Add(new CommandParameter("@ApplnoB", strApplnoB));
                    base.Parameter.Add(new CommandParameter("@cusID", cusID));
                    base.Parameter.Add(new CommandParameter("@StepId", engstep));
                    base.Parameter.Add(new CommandParameter("@Module", module));

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
        /// delete jcicmaster
        /// </summary>
        /// <param name="strApplno"></param>
        /// <param name="strApplnoB"></param>
        /// <param name="strcustid"></param>
        /// <param name="strmodule"></param>
        public void DeleteJCICMaster(string strApplno, string strApplnoB, string strcustid, string strmodule)
        {
            // 連接數據庫
            IDbConnection dbConnection = base.OpenConnection();
            IDbTransaction dbTransaction = null;
            // DB連接
            using (dbConnection)
            {
                try
                {
                    dbTransaction = dbConnection.BeginTransaction();
                    string sql = @"DELETE
                               FROM 
	                               JCICMaster
                               WHERE
	                               Applno = @Applno and ApplnoB=@ApplnoB and CustID=@CustID and Module=@Module   ";

                    base.Parameter.Clear();

                    base.Parameter.Add(new CommandParameter("@Applno", strApplno));
                    base.Parameter.Add(new CommandParameter("@ApplnoB", strApplnoB));
                    base.Parameter.Add(new CommandParameter("@CustID", strcustid));
                    base.Parameter.Add(new CommandParameter("@Module", strmodule));
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
        /// delete JCICDeriveSend
        /// </summary>
        /// <param name="strApplno"></param>
        /// <param name="strApplnoB"></param>
        /// <param name="strmodule"></param>
        public void DeleteJCICDeriveSend(string strApplno, string strApplnoB, string strmodule)
        {
            // 連接數據庫
            IDbConnection dbConnection = base.OpenConnection();
            IDbTransaction dbTransaction = null;
            // DB連接
            using (dbConnection)
            {
                try
                {
                    dbTransaction = dbConnection.BeginTransaction();
                    string sql = @"DELETE
                               FROM 
	                               JCICDeriveSend
                               WHERE
	                               Applno = @Applno and ApplnoB=@ApplnoB and Module=@Module   ";

                    base.Parameter.Clear();

                    base.Parameter.Add(new CommandParameter("@Applno", strApplno));
                    base.Parameter.Add(new CommandParameter("@ApplnoB", strApplnoB));
                    base.Parameter.Add(new CommandParameter("@Module", strmodule));
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
        /// delte JcicSend
        /// </summary>
        /// <param name="strApplno"></param>
        /// <param name="strApplnoB"></param>
        /// <param name="strmodule"></param>
        public void DeleteJcicSend(string strApplno, string strApplnoB, string strmodule)
        {
            // 連接數據庫
            IDbConnection dbConnection = base.OpenConnection();
            IDbTransaction dbTransaction = null;
            // DB連接
            using (dbConnection)
            {
                try
                {
                    dbTransaction = dbConnection.BeginTransaction();
                    string sql = @"DELETE
                               FROM 
	                               JCICSend
                               WHERE
	                               Applno = @Applno and ApplnoB=@ApplnoB and Module=@Module   ";

                    base.Parameter.Clear();

                    base.Parameter.Add(new CommandParameter("@Applno", strApplno));
                    base.Parameter.Add(new CommandParameter("@ApplnoB", strApplnoB));
                    base.Parameter.Add(new CommandParameter("@Module", strmodule));
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
        /// Delete JCICProcess
        /// </summary>
        /// <param name="strApplno"></param>
        /// <param name="strApplnoB"></param>
        /// <param name="strmodule"></param>
        public void DeleteJCICProcess(string strApplno, string strApplnoB, string strmodule)
        {
            // 連接數據庫
            IDbConnection dbConnection = base.OpenConnection();
            IDbTransaction dbTransaction = null;
            // DB連接
            using (dbConnection)
            {
                try
                {
                    dbTransaction = dbConnection.BeginTransaction();
                    string sql = @"DELETE
                               FROM 
	                               JCICProcess
                               WHERE
	                               Applno = @Applno and ApplnoB=@ApplnoB and Module=@Module   ";

                    base.Parameter.Clear();

                    base.Parameter.Add(new CommandParameter("@Applno", strApplno));
                    base.Parameter.Add(new CommandParameter("@ApplnoB", strApplnoB));
                    base.Parameter.Add(new CommandParameter("@Module", strmodule));
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
        /// 
        /// </summary>
        /// <param name="strApplno"></param>
        /// <param name="strApplnoB"></param>
        public void DeleteUDWRApplTagResult(string strApplno, string strApplnoB)
        {
            // 連接數據庫
            IDbConnection dbConnection = base.OpenConnection();
            IDbTransaction dbTransaction = null;
            // DB連接
            using (dbConnection)
            {
                try
                {
                    dbTransaction = dbConnection.BeginTransaction();
                    string sql = @"DELETE
                               FROM 
	                               UDWRApplTagResult
                               WHERE
	                               Applno = @Applno and ApplnoB=@ApplnoB";

                    base.Parameter.Clear();

                    base.Parameter.Add(new CommandParameter("@Applno", strApplno));
                    base.Parameter.Add(new CommandParameter("@ApplnoB", strApplnoB));

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
        /// delete data in datatable
        /// </summary>
        /// <param name="tablename"></param>
        /// <param name="strApplno"></param>
        /// <param name="strApplnoB"></param>
        public void DeleteDataTable(string tablename, string strApplno, string strApplnoB)
        {
            // 連接數據庫
            IDbConnection dbConnection = base.OpenConnection();
            IDbTransaction dbTransaction = null;
            // DB連接
            using (dbConnection)
            {
                try
                {
                    dbTransaction = dbConnection.BeginTransaction();
                    string sql = @"DELETE FROM " + tablename + " WHERE Applno = @Applno and ApplnoB=@ApplnoB";

                    base.Parameter.Clear();
                    base.Parameter.Add(new CommandParameter("@Applno", strApplno));
                    base.Parameter.Add(new CommandParameter("@ApplnoB", strApplnoB));

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
        /// delete data in datatable
        /// </summary>
        /// <param name="tablename"></param>
        /// <param name="strApplno"></param>
        /// <param name="strApplnoB"></param>
        /// <param name="strFilter"></param>
        public void DeleteDataTable(string tablename, string strApplno, string strApplnoB, string strFilter)
        {
            // 連接數據庫
            IDbConnection dbConnection = base.OpenConnection();
            IDbTransaction dbTransaction = null;
            // DB連接
            using (dbConnection)
            {
                try
                {
                    dbTransaction = dbConnection.BeginTransaction();
                    string sql = @"DELETE FROM " + tablename + " WHERE Applno = @Applno and ApplnoB=@ApplnoB ";

                    if (strFilter.Trim().Length > 0)
                        sql += " and " + strFilter;

                    base.Parameter.Clear();
                    base.Parameter.Add(new CommandParameter("@Applno", strApplno));
                    base.Parameter.Add(new CommandParameter("@ApplnoB", strApplnoB));

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
        /// delete data in datatable
        /// </summary>
        /// <param name="tablename"></param>
        /// <param name="strFilter"></param>
        public void DeleteDataTable(string tablename, string strFilter)
        {
            // 連接數據庫
            IDbConnection dbConnection = base.OpenConnection();
            IDbTransaction dbTransaction = null;
            // DB連接
            using (dbConnection)
            {
                try
                {
                    dbTransaction = dbConnection.BeginTransaction();
                    string sql = @"DELETE FROM " + tablename + " WHERE 1=1 ";

                    if (strFilter.Trim().Length > 0)
                        sql += " and " + strFilter;

                    base.Parameter.Clear();
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
        /// 執行初審轉檔
        /// </summary>
        /// <param name="transfertype">0001:初審件轉檔,0002申覆件轉檔</param>
        /// <param name="strApplno">新收編</param>
        /// <param name="strApplnoB"></param>
        /// <param name="busType">業務別</param>
        /// <param name="_datatransfersetting"></param>
        /// <param name="_transtable"></param>
        /// <param name="sourceApplno"></param>
        /// <param name="sourceApplnoB"></param>
        public void TransferData(DataTable _datatransfersetting, DataTable _transtable, string transfertype, string busType, string strApplno, string strApplnoB, string sourceApplno, string sourceApplnoB)
        {
            //string connection = "Data Source=" + dataSource + ";Initial Catalog=" + initCatalog + ";Persist Security Info=True;User ID=" + uid + ";PASSWORD=" + password + ";";

            SqlConnection _sqlcon = new SqlConnection();
            _sqlcon = base.OpenConnection();

            SqlCommand _sqlcmd;
            DataSet selectds;
            string insertsql = "";
            string selectsql = "";
            string deletesql = "";


            try
            {
                DataRow[] rowtable = _transtable.Select("TransferType='" + transfertype + "'");
                foreach (DataRow dr in rowtable)
                {
                    DataRow[] insertrow = _datatransfersetting.Select("TransferType='" + transfertype + "' and TargetTable ='" + dr["TargetTable"].ToString() + "'");
                    insertsql = "";
                    selectsql = "";
                    deletesql = "";
                    MakeInsertSQL(dr["TargetTable"].ToString(), dr["SourceTable"].ToString(), insertrow, ref insertsql, ref selectsql, ref deletesql);
                    selectsql += " where ApplNo=@ApplNo and ApplNoB=@ApplNoB ";

                    //取出資料
                    _sqlcmd = new SqlCommand(selectsql, _sqlcon);
                    _sqlcmd.Parameters.Add(new SqlParameter("@ApplNo", sourceApplno));
                    _sqlcmd.Parameters.Add(new SqlParameter("@ApplNoB", sourceApplnoB));

                    IDataAdapter Adapter = new SqlDataAdapter(_sqlcmd);
                    selectds = new DataSet();
                    Adapter.Fill(selectds);

                    if (!selectds.Tables[0].Columns.Contains("CreatedUser"))
                        selectds.Tables[0].Columns.Add(new DataColumn("CreatedUser"));

                    if (!selectds.Tables[0].Columns.Contains("ModifiedUser"))
                        selectds.Tables[0].Columns.Add(new DataColumn("ModifiedUser"));

                    //逐筆清空資料
                    foreach (DataRow datadr in selectds.Tables[0].Rows)
                    {
                        //新增之前要清空資料
                        _sqlcmd = new SqlCommand(deletesql, _sqlcon);
                        _sqlcmd.Parameters.Add(new SqlParameter("@ApplNo", strApplno));
                        _sqlcmd.Parameters.Add(new SqlParameter("@ApplNoB", strApplnoB));
                        _sqlcmd.ExecuteNonQuery();
                    }

                    //逐筆寫入資料
                    foreach (DataRow datadr in selectds.Tables[0].Rows)
                    {
                        //將新收編寫入
                        datadr["ApplNo"] = strApplno;
                        datadr["ApplNoB"] = strApplnoB;
                        datadr["CreatedUser"] = "UDWRTarnsferPSRNData";
                        datadr["ModifiedUser"] = "UDWRTarnsferPSRNData";
                        selectds.AcceptChanges();
                        //以setting 表的設定作為對應
                        _sqlcmd = new SqlCommand(insertsql, _sqlcon);
                        foreach (DataRow fieldrow in insertrow)
                        {
                            _sqlcmd.Parameters.Add(new SqlParameter(fieldrow["TargetField"].ToString(), datadr[fieldrow["SourceField"].ToString().ToString()]));
                        }
                        _sqlcmd.ExecuteNonQuery();

                    }

                }

            }
            catch (Exception exe)
            {
                throw exe;
            }
            finally
            {
                try
                {
                    _sqlcon.Close();
                    _sqlcon.Dispose();
                }
                catch
                { }
            }


        }


        //edit by mel ,add transaction
        public void UpdateTranferData(DataTable _datatransfersetting, DataTable _transtable
            , string transfertype
            , Dictionary<string, string> keyfield
            , string busType
            , string strApplno
            , string strApplnoB
            , string sourceApplno
            , string sourceApplnoB)
        {


            UpdateTranferData(
                _datatransfersetting
                , _transtable
                , transfertype
                , keyfield
                , busType
                , strApplno
                , strApplnoB
                , sourceApplno
                , sourceApplnoB
                , null
                , "UpdateTranferData" + transfertype
                );
            /*
            DataTable sourcedt = new DataTable();
            DataTable destidt = new DataTable();
            string targettablename="";
            string sourcetablename="";


            try
            {
                DataRow[] rowtable = _transtable.Select("TransferType = '" + transfertype + "'");
                foreach (DataRow dr in rowtable)
                {

                    targettablename = dr["TargetTable"].ToString();
                    sourcetablename = dr["SourceTable"].ToString();

                    DataRow[] updaterow = _datatransfersetting.Select("TransferType = '" + transfertype + "' and SourceTable ='" + sourcetablename + "'  and TargetTable ='" + targettablename + "' ");
                    #region 
                    
                    if (keyfield.Count == 2 && keyfield.ContainsKey("APPLNO") && keyfield.ContainsKey("APPLNOB"))
                    {
                        //key 值為ApplNo及ApplNoB
                        //取出來源資料
                        sourcedt = OpenDataTable(sourcetablename, sourceApplno, sourceApplnoB, "", "");
                        //取出目的資料
                        destidt = OpenDataTable(targettablename, strApplno, strApplnoB, "" , "");
                        //destidatafilter = "ApplNo='" + strApplno + "' and ApplNoB='" + strApplnoB + "'";
                        destidt.TableName = targettablename;
                    }
                    else if (keyfield.Count == 3 && keyfield.ContainsKey("APPLNO") && keyfield.ContainsKey("APPLNOB") && keyfield.ContainsKey("CUSID"))
                    {
                        //key 值為ApplNo及ApplNoB及CusID
                        //取出來源資料
                        sourcedt = OpenDataTable(sourcetablename, sourceApplno, sourceApplnoB, "CUSID='" + keyfield["CUSID"].ToString() + "'", "");
                        //取出目的資料
                        destidt = OpenDataTable(targettablename, strApplno, strApplnoB, "CUSID='" + keyfield["CUSID"].ToString() + "'", "");
                        //destidatafilter = "ApplNo='" + strApplno + "' and ApplNoB='" + strApplnoB + "'";
                        destidt.TableName = targettablename;
                    }
                    else 
                    {
                        throw new Exception("Key field does not defined!!");
                    }
                    #endregion

                    //無資料則離開
                    if (destidt.Rows.Count == 0)
                        return;


                    //更新資料
                    foreach (DataRow _dr in updaterow)
                    {
                        destidt.Rows[0][_dr["TargetField"].ToString()] = sourcedt.Rows[0][_dr["SourceField"].ToString()];
                    }

                    destidt.Rows[0]["CreatedUser"] = "UpdateTranferData";
                    destidt.Rows[0]["CreatedDate"] = System.DateTime.Now;
                    destidt.Rows[0]["ModifiedUser"] = "UpdateTranferData";
                    destidt.Rows[0]["ModifiedDate"] = System.DateTime.Now;
                    destidt.AcceptChanges();      

                    //for (int i = 0; i < sourcedt.Rows.Count; i++)
                    //{
                    //    //判斷目的表中有該筆資料
                    //    DataRow[] destirow = destidt.Select(destidatafilter);

                    //    if (destirow.Length > 0)
                    //    {
                    //        foreach (DataRow _dr in updaterow)
                    //        {
                    //            destidt.Rows[0][_dr["TargetField"].ToString()] = sourcedt.Rows[0][_dr["SourceField"].ToString()];
                    //        }

                    //        destidt.Rows[0]["CreatedUser"] = "UpdateTranferData";
                    //        destidt.Rows[0]["CreatedDate"] = System.DateTime.Now;
                    //        destidt.Rows[0]["ModifiedUser"] = "UpdateTranferData";
                    //        destidt.Rows[0]["ModifiedDate"] = System.DateTime.Now;
                    //        destidt.AcceptChanges();                      
                    //    }

                    //}

                    

                    UpdateDataTable(destidt, keyfield);



                }

            }
            catch (Exception exe)
            {
                throw exe;
            }
             */
        }


        public void UpdateTranferData(DataTable _datatransfersetting, DataTable _transtable
            , string transfertype
            , Dictionary<string, string> keyfield
            , string busType
            , string strApplno
            , string strApplnoB
            , string sourceApplno
            , string sourceApplnoB
            , IDbTransaction dbTransaction = null
            , string op = "")
        {


            DataTable sourcedt = new DataTable();
            DataTable destidt = new DataTable();
            string targettablename = "";
            string sourcetablename = "";


            try
            {
                //DataTable tmpdt = new DataTable();
                //_transtable.DefaultView.Sort="tableorder ,TransferType,SourceTable "
                //tmpdt=_transtable.DefaultView.ToTable();
                DataRow[] rowtable = _transtable.Select("TransferType = '" + transfertype + "'");
                foreach (DataRow dr in rowtable)
                {

                    targettablename = dr["TargetTable"].ToString();
                    sourcetablename = dr["SourceTable"].ToString();

                    DataRow[] updaterow = _datatransfersetting.Select("TransferType = '" + transfertype + "' and SourceTable ='" + sourcetablename + "'  and TargetTable ='" + targettablename + "' ");
                    #region

                    if (keyfield.Count == 2 && keyfield.ContainsKey("APPLNO") && keyfield.ContainsKey("APPLNOB"))
                    {
                        //key 值為ApplNo及ApplNoB
                        //取出來源資料
                        sourcedt = OpenDataTable(sourcetablename, sourceApplno, sourceApplnoB, "", "");
                        //取出目的資料
                        destidt = OpenDataTable(targettablename, strApplno, strApplnoB, "", "");
                        //destidatafilter = "ApplNo='" + strApplno + "' and ApplNoB='" + strApplnoB + "'";
                        destidt.TableName = targettablename;
                    }
                    else if (keyfield.Count == 3 && keyfield.ContainsKey("APPLNO") && keyfield.ContainsKey("APPLNOB") && keyfield.ContainsKey("CUSID"))
                    {
                        //key 值為ApplNo及ApplNoB及CusID
                        //取出來源資料
                        sourcedt = OpenDataTable(sourcetablename, sourceApplno, sourceApplnoB, "CUSID='" + keyfield["CUSID"].ToString() + "'", "");
                        //取出目的資料
                        destidt = OpenDataTable(targettablename, strApplno, strApplnoB, "CUSID='" + keyfield["CUSID"].ToString() + "'", "");

                        destidt.TableName = targettablename;
                    }
                    else
                    {
                        throw new Exception("Key field does not defined!!");
                    }
                    #endregion

                    //無資料則離開
                    if (destidt.Rows.Count == 0)
                        continue;

                    if (sourcedt.Rows.Count == 0)
                        continue;

                    //更新資料
                    foreach (DataRow _dr in updaterow)
                    {
                        destidt.Rows[0][_dr["TargetField"].ToString()] = sourcedt.Rows[0][_dr["SourceField"].ToString()];
                    }

                    destidt.Rows[0]["CreatedUser"] = op;
                    destidt.Rows[0]["CreatedDate"] = System.DateTime.Now;
                    destidt.Rows[0]["ModifiedUser"] = op;
                    destidt.Rows[0]["ModifiedDate"] = System.DateTime.Now;
                    destidt.AcceptChanges();

                    UpdateDataTable(destidt, keyfield, dbTransaction);



                }

            }
            catch (Exception exe)
            {
                throw exe;
            }
        }

        public void TransferDataAprlData(string strApplno, string strApplnoB, string sourceApplno, string sourceApplnoB)
        {
            /*1,APRLDistinguishAgency
             * 2,APRLDistinguishPicture
             * 3,APRLDistinguishReason
             * 4,APRLAddDoc
             * 5,APRLGuaranteeObligee
             * 6,APRLGuaranteeBuildingDetail
             * 7,APRLGuaranteeStallDetail
             * 8,APRLGuaranteeLandDetail
             * 9,APRLOwnerList
             * 
             */
            string sql = @" Delete APRLDistinguishAgency where dmid in (select dmid from APRLDistinguishMain where ApplNo=@ApplNo and ApplNoB=@ApplNoB);
                            Delete APRLDistinguishPicture where dmid in (select dmid from APRLDistinguishMain where ApplNo=@ApplNo and ApplNoB=@ApplNoB);
                            Delete APRLDistinguishReason where dmid in (select dmid from APRLDistinguishMain where ApplNo=@ApplNo and ApplNoB=@ApplNoB);
                            Delete APRLAddDoc where dmid in (select dmid from APRLDistinguishMain where ApplNo=@ApplNo and ApplNoB=@ApplNoB);
                            Delete APRLGuaranteeObligee where GmId in (select GmId from APRLGuaranteeMain where ApplNo=@ApplNo and ApplNoB=@ApplNoB);
                            Delete APRLGuaranteeBuildingDetail where GbId in (select GbId from APRLGuaranteeBuilding where ApplNo=@ApplNo and ApplNoB=@ApplNoB);
                            Delete APRLGuaranteeStallDetail where GbId in (select GbId from APRLGuaranteeBuilding where ApplNo=@ApplNo and ApplNoB=@ApplNoB);
                            Delete APRLGuaranteeLandDetail where GlId in (select GlId from APRLGuaranteeLand where ApplNo=@ApplNo and ApplNoB=@ApplNoB);
                            Delete APRLOwnerList where GId in (select GlId from APRLGuaranteeLand where ApplNo=@ApplNo and ApplNoB=@ApplNoB);
                            Delete APRLOwnerList where GId in (select GbId from APRLGuaranteeBuilding where ApplNo=@ApplNo and ApplNoB=@ApplNoB);
                            
                            insert into  APRLDistinguishAgency(DaId,DmId,BranchName,CurrentPrice,CurrentType,CurrentUnit,StallPrice,StallType, 
                            AgencySource,Sort,CreatedUser)
                            select newid() DaId, m.newdmid DmId,c.BranchName, c.CurrentPrice, c.CurrentType, c.CurrentUnit, c.StallPrice, c.StallType, 
                            c.AgencySource, c.Sort,@CreateUser  CreatedUser
                            from 
                            (
	                            select a.applno newapplno ,a.applnob newapplnob,a.DmId newdmid
	                            ,b.applno oldapplno ,b.applnob oldapplnob,b.dmid olddmid
	                            from 
	                            (select applno ,applnob,dmid,guarano from APRLDistinguishMain where ApplNo=@ApplNo and ApplNoB=@ApplNoB) a
	                            left outer join 
	                            (select applno ,applnob,dmid,guarano from APRLDistinguishMain where ApplNo=@sourceApplNo and ApplNoB=@sourceApplNoB) b
	                            on a.guarano =b.guarano 
                            ) m
                            left outer join APRLDistinguishAgency c  on m.olddmid=c.dmid 
                            where c.BranchName is not null ;

                            insert into  APRLDistinguishPicture(DpId,DmId,PicNo,PicIndex,[File],FilePath,FileName,CreatedUser)
                            select newid() DpId, m.newdmid DmId,c.PicNo, c.PicIndex, c.[File], c.FilePath, c.FileName,@CreateUser  CreatedUser
                            from 
                            (
	                            select a.applno newapplno ,a.applnob newapplnob,a.DmId newdmid
	                            ,b.applno oldapplno ,b.applnob oldapplnob,b.dmid olddmid
	                            from 
	                            (select applno ,applnob,dmid,guarano from APRLDistinguishMain where ApplNo=@ApplNo and ApplNoB=@ApplNoB) a
	                            left outer join 
	                            (select applno ,applnob,dmid,guarano from APRLDistinguishMain where ApplNo=@sourceApplNo and ApplNoB=@sourceApplNoB) b
	                            on a.guarano =b.guarano 
                            ) m
                            left outer join APRLDistinguishPicture c  on m.olddmid=c.dmid
                            where c.PicNo is not null ;

                            insert into  APRLDistinguishReason( DrId ,DmId, drType, ReasonCode , CreatedUser)
                            select newid() DrId, m.newdmid DmId,c.drType, c.ReasonCode,@CreateUser  CreatedUser
                            from 
                            (
	                            select a.applno newapplno ,a.applnob newapplnob,a.DmId newdmid
	                            ,b.applno oldapplno ,b.applnob oldapplnob,b.dmid olddmid
	                            from 
	                            (select applno ,applnob,dmid,guarano from APRLDistinguishMain where ApplNo=@ApplNo and ApplNoB=@ApplNoB) a
	                            left outer join 
	                            (select applno ,applnob,dmid,guarano from APRLDistinguishMain where ApplNo=@sourceApplNo and ApplNoB=@sourceApplNoB) b
	                            on a.guarano =b.guarano 
                            ) m
                            left outer join APRLDistinguishReason c  on m.olddmid=c.dmid
                            where c.drType is not null ;

                            insert into  APRLAddDoc( AdId ,DmId,ApplNo, ApplNoB, GuaraNo, SerialNo,AddCome, AddDoc, AddStatus, AddDate, ReceiveDate, CancelDate, CreatedUser)
                            select newid() AdId, m.newdmid DmId,c.ApplNo, c.ApplNoB, c.GuaraNo, c.SerialNo, c.AddCome, c.AddDoc, c.AddStatus, c.AddDate, c.ReceiveDate, c.CancelDate,@CreateUser  CreatedUser
                            from 
                            (
	                            select a.applno newapplno ,a.applnob newapplnob,a.DmId newdmid
	                            ,b.applno oldapplno ,b.applnob oldapplnob,b.dmid olddmid
	                            from 
	                            (select applno ,applnob,dmid,guarano from APRLDistinguishMain where ApplNo=@ApplNo and ApplNoB=@ApplNoB) a
	                            left outer join 
	                            (select applno ,applnob,dmid,guarano from APRLDistinguishMain where ApplNo=@sourceApplNo and ApplNoB=@sourceApplNoB) b
	                            on a.guarano =b.guarano 
                            ) m
                            left outer join APRLAddDoc c  on m.olddmid=c.dmid
                            where c.SerialNo  is not null;

                            insert into  APRLGuaranteeObligee(GoId, GmId, Sequence, SetDate, Obligee, SetMoney, CreatedUser)
                            select newid() GoId, m.newgmid GmId, Sequence, SetDate, Obligee, SetMoney,@CreateUser  CreatedUser
                            from 
                            (
	                            select a.applno newapplno ,a.applnob newapplnob,a.GmId newgmid
	                            ,b.applno oldapplno ,b.applnob oldapplnob,b.GmId oldgmid
	                            from 
	                            (select applno ,applnob,GmId,guarano from APRLGuaranteeMain where ApplNo=@ApplNo and ApplNoB=@ApplNoB) a
	                            left outer join 
	                            (select applno ,applnob,GmId,guarano from APRLGuaranteeMain where ApplNo=@sourceApplNo and ApplNoB=@sourceApplNoB) b
	                            on a.guarano =b.guarano 
                            ) m 
                            left outer join APRLGuaranteeObligee c  on m.oldgmid=c.GmId
                            where c.Sequence is not null;

                            insert into  APRLGuaranteeBuildingDetail( GdId, GbId,DetailNo, Purpose, TotalMeasure, DetailRangeM, DetailRangeC, DetailMeasure1, DetailMeasure2,CreatedUser)
                            select newid() GdId, m.newgbid GbId,DetailNo, Purpose, TotalMeasure, DetailRangeM, DetailRangeC, DetailMeasure1, DetailMeasure2,@CreateUser  CreatedUser
                            from 
                            (
	                            select a.applno newapplno ,a.applnob newapplnob,a.GbId newgbid
	                            ,b.applno oldapplno ,b.applnob oldapplnob,b.GbId oldgbid
	                            from 
	                            (select applno ,applnob,GbId,guarano,bulno from APRLGuaranteeBuilding where ApplNo=@ApplNo and ApplNoB=@ApplNoB) a
	                            left outer join 
	                            (select applno ,applnob,GbId,guarano,bulno from APRLGuaranteeBuilding where ApplNo=@sourceApplNo and ApplNoB=@sourceApplNoB) b
	                            on a.guarano =b.guarano and a.bulno =b.bulno
                            ) m 
                            left outer join APRLGuaranteeBuildingDetail c  on m.oldgbid=c.GbId
                            where c.DetailNo is not null;

                            insert into  APRLGuaranteeStallDetail( GsdId, GbId,StallType, StallNo, StallMeasure,CreatedUser)
                            select newid() GsdId, m.newgbid GbId,StallType, StallNo, StallMeasure,@CreateUser  CreatedUser
                            from 
                            (
	                            select a.applno newapplno ,a.applnob newapplnob,a.GbId newgbid
	                            ,b.applno oldapplno ,b.applnob oldapplnob,b.GbId oldgbid
	                            from 
	                            (select applno ,applnob,GbId,guarano,bulno from APRLGuaranteeBuilding where ApplNo=@ApplNo and ApplNoB=@ApplNoB) a
	                            left outer join 
	                            (select applno ,applnob,GbId,guarano,bulno from APRLGuaranteeBuilding where ApplNo=@sourceApplNo and ApplNoB=@sourceApplNoB) b
	                            on a.guarano =b.guarano  and a.BulNo=b.BulNo
                            ) m 
                            left outer join APRLGuaranteeStallDetail c  on m.oldgbid=c.GbId
                            where c.StallNo is not null;

                            insert into  APRLGuaranteeLandDetail( GldId, GlId,ShiftDate, ShiftPrice, ShiftRangeM, ShiftRangeC, PriceIndex,CreatedUser)
                            select newid() GldId, m.newglid GlId,ShiftDate, ShiftPrice, ShiftRangeM, ShiftRangeC, PriceIndex,@CreateUser  CreatedUser
                            from 
                            (
	                            select a.applno newapplno ,a.applnob newapplnob,a.GlId newglid
	                            ,b.applno oldapplno ,b.applnob oldapplnob,b.GlId oldglid
	                            from 
	                            (select applno ,applnob,GlId,guarano,sectorno from APRLGuaranteeLand where ApplNo=@ApplNo and ApplNoB=@ApplNoB) a
	                            left outer join 
	                            (select applno ,applnob,GlId,guarano,sectorno from APRLGuaranteeLand where ApplNo=@sourceApplNo and ApplNoB=@sourceApplNoB) b
	                            on a.guarano =b.guarano  and a.SectorNo=b.SectorNo
                            ) m 
                            left outer join APRLGuaranteeLandDetail c  on m.oldglid=c.GlId
                            where c.ShiftDate is not null;

                            insert into APRLOwnerList(OlId,  GId, OwnerType, Owner, OwnerID, RightRangeM, RightRangeC,CreatedUser) 
                            select newid() OlId, m.newglid GId, OwnerType, Owner, OwnerID, RightRangeM, RightRangeC,@CreateUser  CreatedUser
                            from 
                            (
	                            select a.applno newapplno ,a.applnob newapplnob,a.GlId newglid
	                            ,b.applno oldapplno ,b.applnob oldapplnob,b.GlId oldglid
	                            from 
	                            (select applno ,applnob,GlId,guarano,areano ,sectorno  from APRLGuaranteeLand where ApplNo=@ApplNo and ApplNoB=@ApplNoB) a
	                            left outer join 
	                            (select applno ,applnob,GlId,guarano,areano ,sectorno  from APRLGuaranteeLand where ApplNo=@sourceApplNo and ApplNoB=@sourceApplNoB) b
	                            on a.guarano =b.guarano  and a.areano =b.areano and a.sectorno=b.sectorno
                            ) m 
                            left outer join APRLOwnerList c  on m.oldglid=c.GId
                            where c.OwnerType=2  
                            union 
                            select newid() OlId, m.newgbid GId, OwnerType, Owner, OwnerID, RightRangeM, RightRangeC,@CreateUser  CreatedUser
                            from 
                            (
	                            select a.applno newapplno ,a.applnob newapplnob,a.GbId newgbid
	                            ,b.applno oldapplno ,b.applnob oldapplnob,b.GbId oldgbid
	                            from 
	                            (select applno ,applnob,GbId,guarano,areano,bulno  from APRLGuaranteeBuilding where ApplNo=@ApplNo and ApplNoB=@ApplNoB) a
	                            left outer join 
	                            (select applno ,applnob,GbId,guarano,areano,bulno  from APRLGuaranteeBuilding where ApplNo=@sourceApplNo and ApplNoB=@sourceApplNoB) b
	                            on a.guarano =b.guarano   and a.areano=b.areano and a.bulno=b.bulno
                            ) m 
                            left outer join APRLOwnerList c  on m.oldgbid=c.GId
                            where c.OwnerType=1  ; ";
            IDbConnection dbConnection = base.OpenConnection();
            IDbTransaction dbTransaction = null;
            using (dbConnection)
            {
                try
                {
                    dbTransaction = dbConnection.BeginTransaction();
                    base.Parameter.Clear();
                    base.Parameter.Add(new CommandParameter("@ApplNo", strApplno));
                    base.Parameter.Add(new CommandParameter("@ApplNoB", strApplnoB));
                    base.Parameter.Add(new CommandParameter("@sourceApplNo", sourceApplno));
                    base.Parameter.Add(new CommandParameter("@sourceApplNoB", sourceApplnoB));
                    base.Parameter.Add(new CommandParameter("@CreateUser", "UDWRTarnsferPSRNData"));
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
        /// 
        /// </summary>
        /// <param name="strApplno"></param>
        /// <param name="strApplnoB"></param>
        /// <remarks>edit by mel 20131031 IR-63176 覆轉檔:  當案件為申覆件且鑑價結果為核准時, 才轉徵信鑑價意見! 其餘不轉</remarks>
        public void CheckAndDeleteAprlData(string strApplno, string strApplnoB)
        {
            IDbConnection dbConnection = base.OpenConnection();
            DataTable returnValue = new DataTable();
            string sql = "";
            try
            {
                sql = @"select a.ApplNo,a.ApplNoB,b.ApplTypeCode ,c.FinalResult
                            from NUMSApplyVerify a 
                            left outer join NUMSMaster b on a.ApplNo=b.ApplNo and a.ApplNoB=b.ApplNoB
                            left outer join APRLMaster c on a.OldApplNo=c.ApplNo and a.OldApplNoB=c.ApplNoB
                            where a.ApplNo=@ApplNo and a.ApplNoB=@ApplNoB
                            and b.ApplTypeCode='L' and c.FinalResult <>'1'  ";

                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@ApplNo", strApplno));
                base.Parameter.Add(new CommandParameter("@ApplNoB", strApplnoB));
                returnValue = base.Search(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            //若無符合資料則無須處理
            if (returnValue.Rows.Count == 0)
                return;

            IDbTransaction dbTransaction = null;
            using (dbConnection)
            {
                try
                {
                    sql = @"delete UDWRDistinguish where ApplNo=@ApplNo and ApplNoB=@ApplNoB ";
                    dbTransaction = dbConnection.BeginTransaction();
                    base.Parameter.Clear();
                    base.Parameter.Add(new CommandParameter("@ApplNo", strApplno));
                    base.Parameter.Add(new CommandParameter("@ApplNoB", strApplnoB));
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
        /// 
        /// </summary>
        /// <param name="strApplno"></param>
        /// <param name="strApplnoB"></param>
        /// <remarks>取出申覆案前案的資訊</remarks>
        public DataTable CheckOriAprlData(string strApplno, string strApplnoB)
        {
            IDbConnection dbConnection = base.OpenConnection();
            DataTable returnValue = new DataTable();
            string sql = "";
            try
            {
                sql = @"select a.OldApplNo,a.OldApplNoB
                            ,c.FinalResult
                            ,isnull(c.Owner,'') Owner
                            from NUMSApplyVerify a 
                            left outer join NUMSMaster b on a.ApplNo=b.ApplNo and a.ApplNoB=b.ApplNoB
                            left outer join APRLMaster c on a.OldApplNo=c.ApplNo and a.OldApplNoB=c.ApplNoB
                            where a.ApplNo=@ApplNo and a.ApplNoB=@ApplNoB
                            --and c.FinalResult='1' 
                            and b.ApplTypeCode='L' ";

                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@ApplNo", strApplno));
                base.Parameter.Add(new CommandParameter("@ApplNoB", strApplnoB));
                returnValue = base.Search(sql);
                return returnValue;
            }
            catch (Exception ex)
            {
                throw ex;
            }



        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="strApplno"></param>
        /// <param name="strApplnoB"></param>
        /// <param name="sourceApplno"></param>
        /// <param name="sourceApplnoB"></param>
        /// <remarks>add by mel 20131007  IR-62816</remarks>
        public void UpdateAprlData(string strApplno, string strApplnoB, string sourceApplno, string sourceApplnoB)
        {
            /*
             * UDWRException 取手動新增部分 RelationKind = '99'
             * 新增資料的CreditSeqNo均為0
             */
            string sql = @" Delete UDWRException where ApplNo=@ApplNo and ApplNoB=@ApplNoB ;
                            insert into  UDWRException
                                (ApplNo,ApplNoB,CreditSeqNo,CodeType,CodeNo, CodeRelation, RelationKind, 
                            Sequence, CreatedDate,CreatedUser ,ModifiedDate,ModifiedUser)
                            SELECT     @ApplNo ApplNo, @ApplNoB ApplNoB, 
                            0 CreditSeqNo, CodeType, CodeNo, CodeRelation, RelationKind, Sequence, 
                            getdate() CreatedDate, 
                            'UDWRTarnsferPSRNData_Appear' CreatedUser, 
                            getdate() ModifiedDate, 
                            'UDWRTarnsferPSRNData_Appear' ModifiedUser
                            FROM         UDWRException
                            WHERE     (RelationKind = '99') and ApplNo =@sourceApplNo and ApplNoB=@sourceApplNoB ; 
                            update UDWRMaster set CreditSeqNo =0 ,ModifiedUser ='UDWRTarnsferPSRNData_Appear' , ModifiedDate =getdate()  where  ApplNo =@ApplNo and ApplNoB=@ApplNoB ;
                            update UDWRDetailByCusId set CreditSeqNo =0 ,ModifiedUser ='UDWRTarnsferPSRNData_Appear' , ModifiedDate =getdate()  where  ApplNo =@ApplNo and ApplNoB=@ApplNoB ;
                            update UDWRProduct set CreditSeqNo =0 ,ModifiedUser ='UDWRTarnsferPSRNData_Appear' , ModifiedDate =getdate()  where  ApplNo =@ApplNo and ApplNoB=@ApplNoB ;";

            IDbConnection dbConnection = base.OpenConnection();
            IDbTransaction dbTransaction = null;
            using (dbConnection)
            {
                try
                {
                    dbTransaction = dbConnection.BeginTransaction();
                    base.Parameter.Clear();
                    base.Parameter.Add(new CommandParameter("@ApplNo", strApplno));
                    base.Parameter.Add(new CommandParameter("@ApplNoB", strApplnoB));
                    base.Parameter.Add(new CommandParameter("@sourceApplNo", sourceApplno));
                    base.Parameter.Add(new CommandParameter("@sourceApplNoB", sourceApplnoB));
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
        /// 
        /// </summary>
        /// <param name="strApplno"></param>
        /// <param name="strApplnoB"></param>
        /// <param name="finalresult"></param>
        /// <param name="owner"></param>
        /// <remarks>edit by mel 20131112</remarks>
        public void UpdateAprlDataForAprl(string strApplno, string strApplnoB
            , string finalresult
            , string owner
            )
        {
            string sql = @" update APRLMaster set FinalResult =@FinalResult, Owner=@Owner 
                        ,ProcDateTime =getdate()  
                        ,ModifiedUser ='UpdateAprlDataForAprl' , ModifiedDate =getdate()  
                        where  ApplNo =@ApplNo and ApplNoB=@ApplNoB ;";

            IDbConnection dbConnection = base.OpenConnection();
            IDbTransaction dbTransaction = null;
            using (dbConnection)
            {
                try
                {
                    dbTransaction = dbConnection.BeginTransaction();
                    base.Parameter.Clear();
                    base.Parameter.Add(new CommandParameter("@ApplNo", strApplno));
                    base.Parameter.Add(new CommandParameter("@ApplNoB", strApplnoB));
                    base.Parameter.Add(new CommandParameter("@FinalResult", finalresult));
                    base.Parameter.Add(new CommandParameter("@Owner", owner));

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
        /// get  data from UDWRTagDbMatchData
        /// </summary>
        /// <param name="strBusType_New"></param>
        /// <param name="strQuery"></param>
        /// <returns></returns>
        public DataTable GetUDWRTagDbMatchData(string strBusType_New, string strQuery)
        {

            try
            {
                string sql = "select * from UDWRTagDb  where BusType in (" + strBusType_New + ") and  MatchContent in (" + strQuery + ")  and GETDATE()>= EffDate  and GETDATE() <= ineffdate  ";
                return OpenDataTable(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 取會辦件清單
        /// </summary>
        /// <returns></returns>
        public DataTable GetManageBillData()
        {
            string strSQL = "select ApplNo ,ApplNoB ,MBNo ,BillType, SendDate,PreFinishDate,FinishMan from NUMSManageBill where ISNULL(FinishMan,'') =''";

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
        /// Get employee no list 
        /// </summary>
        /// <returns></returns>
        public DataTable GetAPRNEmpIDList()
        {
            string strSQL = "select distinct AreaCode,EmpNo  ,Cast('0' as int) as DispatchCnt,'N' as IsDispatch from PARMDistinguishRule " +
                            "order by EmpNo ";

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

        public DataTable GetAPRNEmpIDList(string areacode)
        {
            DataTable returnValue = new DataTable();
            string strSQL = @"select a.AreaCode,a.EmpNo ,ISNULL(a.DispatchCnt,0) DispatchCnt
                                ,ISNULL(a.IsDispatch,'N') IsDispatch ,isnull(b.RoleID,'') RoleID
                                from PARMDistinguishRule a left outer join NUMSEmployeeToRole b
                                on a.EmpNo=b.EmpID 
                            where a.AreaCode=@AreaCode
                            and b.RoleID in (select CodeNo from PARMCode where CodeType='APRLDispatchRole') ";

            try
            {
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@AreaCode", areacode));
                returnValue = base.Search(strSQL);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return returnValue;
        }

        public DataTable GetUDWREmpIDList(string bumasterid, string bunumber)
        {
            DataTable returnValue = new DataTable();
            //            string strSQL = @"select a.*,isnull(b.RoleID,'') RoleID from NUMSBUToEmployee a left outer join 
            //                            NUMSEmployeeToRole b on a.EmpID=b.EmpID and b.RoleID='NUMSD0002'
            //                            Where a.BUMasterID =@BUMasterID  
            //                            order by EmpID ";
            string strSQL = @"select a.*,isnull(b.RoleID,'') RoleID,c.BUNumber from NUMSBUToEmployee a left outer join 
                            NUMSEmployeeToRole b on a.EmpID=b.EmpID 
                            left outer join NUMSBU c on a.BUID=c.BUID 
                            Where a.BUMasterID =@BUMasterID and c.BUNumber=@BUNumber 
                            and b.RoleID in (select CodeNo from PARMCode where CodeType='UDWRDispatchRole')
                            order by EmpID ";

            try
            {
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@BUMasterID", bumasterid));
                base.Parameter.Add(new CommandParameter("@BUNumber", bunumber));
                returnValue = base.Search(strSQL);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return returnValue;
        }

        /// <summary>
        /// get area from APRLGuaranteeBuilding
        /// </summary>
        /// <param name="strApplno"></param>
        /// <param name="strApplnoB"></param>
        /// <returns></returns>
        public string GetAPRLGuaranteeBuildingArea(string strApplno, string strApplnoB)
        {
            string areacode = "";
            //            string strSQL = @"SELECT top 1
            //                                a.ApplNo ,a.ApplNoB ,b.AreaNo
            //                                FROM APRLGuaranteeMain a
            //                                left outer join APRLGuaranteeBuilding b
            //                                on a.ApplNo=b.ApplNo and a.ApplNoB =b.ApplNoB  
            //                                where 
            //                                a.ApplNo =@ApplNo and  a.ApplNoB=@ApplNoB 
            //                                and not b.AreaNo is null";
            string strSQL = @" SELECT top 1 
                                a.ApplNo ,a.ApplNoB ,SUBSTRING(a.GuaraNo,1,3) as AreaNo
                                FROM APRLGuaranteeMain a 
                                where a.ApplNo =@ApplNo and  a.ApplNoB=@ApplNoB  and isnull(a.MbNo,'')=''";

            DataTable returnValue = new DataTable();
            try
            {
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@ApplNo", strApplno));
                base.Parameter.Add(new CommandParameter("@ApplNoB", strApplnoB));
                returnValue = base.Search(strSQL);
                if (returnValue.Rows.Count > 0)
                    areacode = returnValue.Rows[0]["AreaNo"].ToString();
            }

            catch (Exception ex)
            {
                throw ex;
            }
            return areacode;
        }

        /// <summary>
        /// get area from APRLGuaranteeBuilding for ManageBill
        /// </summary>
        /// <param name="strApplno"></param>
        /// <param name="strApplnoB"></param>
        /// <returns></returns>
        public string GetAPRLGuaranteeBuildingAreaForManageBill(string strApplno, string strApplnoB)
        {
            string areacode = "";

            string strSQL = @" SELECT top 1 
                                a.ApplNo ,a.ApplNoB ,SUBSTRING(a.GuaraNo,1,3) as AreaNo
                                FROM APRLGuaranteeMain a 
                                where a.ApplNo =@ApplNo and  a.ApplNoB=@ApplNoB  and isnull(a.MbNo,'')<>''";

            DataTable returnValue = new DataTable();
            try
            {
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@ApplNo", strApplno));
                base.Parameter.Add(new CommandParameter("@ApplNoB", strApplnoB));
                returnValue = base.Search(strSQL);
                if (returnValue.Rows.Count > 0)
                    areacode = returnValue.Rows[0]["AreaNo"].ToString();
            }

            catch (Exception ex)
            {
                throw ex;
            }
            return areacode;
        }

        public void UpdateDispatchTime(string strApplno, string strApplnoB)
        {

            string sql = @"UPDATE APRLMaster   SET 
                            SendDate=GETDATE(),
                            PreFinishDate =
                            (
                            select top 1 b.workdate from 
                            ( SELECT ROW_NUMBER() OVER( ORDER BY a.date) as rowno, a.date as workdate  from  PARMWorkingDay a where a.date >getdate() and a.Flag='1' ) b 
                            where  b.rowno = 
                            (select top 1 case isnumeric(a.CodeNo)  when 1  then CAST(a.CodeNo as int) else 0 end  as basedate from PARMCode a where CodeType in ('APRLDispatchDay'))
                            ) ,
                            ModifiedUser='UpdateDispatchTime' ,
                            ModifiedDate=getdate()  
                            where ApplNo =@ApplNo and  ApplNoB=@ApplNoB  ";
            IDbConnection dbConnection = base.OpenConnection();
            IDbTransaction dbTransaction = null;
            using (dbConnection)
            {
                try
                {
                    dbTransaction = dbConnection.BeginTransaction();
                    base.Parameter.Clear();
                    base.Parameter.Add(new CommandParameter("@ApplNo", strApplno));
                    base.Parameter.Add(new CommandParameter("@ApplNoB", strApplnoB));
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

        public void UpdateDispatchTime(string strApplno, string strApplnoB, DateTime senddate, DateTime prefinishdate)
        {

            string sql = @"UPDATE APRLMaster   SET 
                            SendDate=@SendDate,
                            PreFinishDate =@PreFinishDate ,
                            ModifiedUser='UpdateDispatchTime' ,
                            ModifiedDate=getdate()  
                            where ApplNo =@ApplNo and  ApplNoB=@ApplNoB  ";
            IDbConnection dbConnection = base.OpenConnection();
            IDbTransaction dbTransaction = null;
            using (dbConnection)
            {
                try
                {
                    dbTransaction = dbConnection.BeginTransaction();
                    base.Parameter.Clear();
                    base.Parameter.Add(new CommandParameter("@ApplNo", strApplno));
                    base.Parameter.Add(new CommandParameter("@ApplNoB", strApplnoB));
                    base.Parameter.Add(new CommandParameter("@SendDate", senddate));
                    base.Parameter.Add(new CommandParameter("@PreFinishDate", prefinishdate));
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
        public void UpdateManageBillDispatchTime(string strApplno, string strApplnoB, string empid)
        {

            string sql = @"UPDATE NUMSManageBill   SET 
                            SendDate=GETDATE(),
                            FinishMan = @FinishMan,
                            PreFinishDate =
                            (
                            select top 1 b.workdate from 
                            ( SELECT ROW_NUMBER() OVER( ORDER BY a.date) as rowno, a.date as workdate  from  PARMWorkingDay a where a.date >getdate() and a.Flag='1' ) b 
                            where  b.rowno = 
                            (select top 1 case isnumeric(a.CodeNo)  when 1  then CAST(a.CodeNo as int) else 0 end  as basedate from PARMCode a where CodeType in ('APRLDispatchDay'))
                            ) ,
                            ModifiedUser='UpdateManageBillDispatchTime' ,
                            ModifiedDate=getdate()  
                            where ApplNo =@ApplNo and  ApplNoB=@ApplNoB  ";
            IDbConnection dbConnection = base.OpenConnection();
            IDbTransaction dbTransaction = null;
            using (dbConnection)
            {
                try
                {
                    dbTransaction = dbConnection.BeginTransaction();
                    base.Parameter.Clear();
                    base.Parameter.Add(new CommandParameter("@ApplNo", strApplno));
                    base.Parameter.Add(new CommandParameter("@ApplNoB", strApplnoB));
                    base.Parameter.Add(new CommandParameter("@FinishMan", empid));
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

        public void UpdateParmSector()
        {

            string sql = @"declare
                        @AreaCode	varchar(3),
                        @CityName 	nvarchar(50),
                        @AreaName	nvarchar(50),
                        @parentCode	varchar(3)
                        select @AreaCode='',@CityName='',@AreaName='',@parentCode=''
                        DECLARE Rank3 CURSOR
                        FOR 
                        SELECT distinct b.areacode,A.CityName,A.AreaName FROM parmsector A inner join   parmareacode B on a.Cityname=B.areaname  where b.parentcode=''
                        OPEN Rank3
                                   while (1=1)
                                   BEGIN
                                             fetch next from Rank3 into @parentCode,@CityName,@AreaName
                                             if @@fetch_status!=0 break
					                        SET @AreaCode=''
					                        select @AreaCode=AreaCode from parmareacode where parentCode=@parentCode and AreaName=@AreaName
					                        update parmsector set AreaCode=@AreaCode where CityName=@CityName and AreaName=@AreaName
		                           END
                        CLOSE Rank3
                        DEALLOCATE Rank3
                        update parmsector set areacode='907' where cityname='屏東縣' and areaname='鹽埔鄉'
                        update parmsector set areacode='946' where cityname='屏東縣' and areaname='恒春鎮' ";
            IDbConnection dbConnection = base.OpenConnection();
            IDbTransaction dbTransaction = null;
            using (dbConnection)
            {
                try
                {
                    dbTransaction = dbConnection.BeginTransaction();
                    base.Parameter.Clear();
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
        /// 取出空的資料集
        /// </summary>
        /// <param name="tablename">Table name</param>
        /// <param name="removefield">要排除的欄位</param>
        /// <returns></returns>
        public DataTable GetEmptyDataTable(string tablename, string removefield)
        {
            try
            {
                if (removefield.Length == 0) return GetEmptyDataTable(tablename);

                DataTable dt = new DataTable();
                string sql = "select * from " + tablename + "  Where   1=2";
                dt = OpenDataTable(sql);
                dt.TableName = tablename;
                string[] _refield = removefield.Split(new string[] { "," }, StringSplitOptions.None); ;
                foreach (string _field in _refield)
                {
                    dt.Columns.Remove(_field);
                }

                return dt;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 依傳入的欄位取出該table的空的資料集(僅包含傳入的欄位)
        /// </summary>
        /// <param name="tablename"></param>
        /// <param name="columnname">以,區隔</param>
        /// <returns></returns>
        public DataTable GetEmptyDataTableByColumn(string tablename, string columnname)
        {
            try
            {
                if (columnname.Length == 0) return GetEmptyDataTable(tablename);
                string fieldstr = "";
                string[] _refield = columnname.Split(new string[] { "," }, StringSplitOptions.None);

                for (int i = 0; i < _refield.Length; i++)
                {
                    if (i != 0)
                    {
                        fieldstr += ",";

                    }
                    fieldstr += _refield[i];

                }

                DataTable dt = new DataTable();
                string sql = "select " + fieldstr + " from " + tablename + "  Where   1=2";
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
        /// Get Empty ApplTagResult datatable
        /// </summary>
        /// <returns></returns>
        public DataTable GetEmptyApplTagResult()
        {
            try
            {
                string sql = "select * from UDWRApplTagResult Where ApplNo='AZXXXXXXXXXX' and 1=2";
                return OpenDataTable(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// get Parmcode 
        /// </summary>
        /// <param name="codetype"></param>
        /// <param name="codeno"></param>
        /// <param name="codedesc"></param>
        /// <returns></returns>
        public DataTable GetPARMCodeTable(string codetype, string codeno, string codedesc)
        {

            try
            {
                DataTable returnValue = new DataTable();
                if (codetype.Trim().Length == 0) throw new Exception("No CodeType!!");
                string sql = "select * from PARMCode  Where CodeType=@CodeType ";
                if (codeno.Trim().Length > 0) sql += "and CodeNo='" + codeno + "' ";
                if (codedesc.Trim().Length > 0) sql += "and CodeDesc='" + codedesc + "' ";
                sql += " order by CodeType,CodeNo,SortOrder ";
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@CodeType", codetype));
                returnValue = base.Search(sql);
                return returnValue;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// get id from querydetail
        /// </summary>
        /// <param name="applno"></param>
        /// <param name="applnob"></param>
        /// <returns></returns>
        public DataTable GetQueryDetailId(string applno, string applnob)
        {
            //資料讀取原則為以NUMSCustomerInfo為主,若UDWRQueryDetail有資料,則視InnerQuery欄位看
            //要不要查,若UDWRQueryDetail沒有資料則預設要查
            try
            {
                string sql = "select a.applno,a.applnob,a.GuaranteeSeqNo,a.CusId,a.CTCBCusID , a.ForeignID ,";
                sql += " ISNULL(b.InnerQuery, 'Y') as InnerQuery , ";
                sql += " ISNULL(b.OuterQuery, 'Y') as OuterQuery  ";
                sql += " from NUMSCustomerInfo a ";
                sql += " left outer join UDWRQueryDetail b on a.Applno=b.Applno and a.applnob=b.applnob and a.cusid=b.cusid  ";
                sql += " where a.ApplNo=@ApplNo and a.ApplNoB=@ApplNoB and ISNULL(b.InnerQuery, 'Y') ='Y' and ISNULL(a.Status,'Y')='Y' ";
                Hashtable ht = new Hashtable();
                ht.Add("@ApplNo", applno);
                ht.Add("@ApplNoB", applnob);
                return OpenDataTable(sql, ht);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public DataTable GetQueryOuterDetailId(string applno, string applnob)
        {
            //資料讀取原則為以NUMSCustomerInfo為主,若UDWRQueryDetail有資料,則視OuterQuery欄位看
            //要不要查,若UDWRQueryDetail沒有資料則預設要查
            try
            {
                string sql = "select a.applno,a.applnob,a.GuaranteeSeqNo,Substring(a.cusid,1,10) CusId,a.CTCBCusID , a.ForeignID ,";
                sql += " ISNULL(b.InnerQuery, 'Y') as InnerQuery , ";
                sql += " ISNULL(b.OuterQuery, 'Y') as OuterQuery  ";
                sql += " from NUMSCustomerInfo a ";
                sql += " left outer join UDWRQueryDetail b on a.Applno=b.Applno and a.applnob=b.applnob and a.cusid=b.cusid  ";
                sql += " where a.ApplNo=@ApplNo and a.ApplNoB=@ApplNoB and ISNULL(b.OuterQuery, 'Y') ='Y' and ISNULL(a.Status,'Y')='Y' ";
                Hashtable ht = new Hashtable();
                ht.Add("@ApplNo", applno);
                ht.Add("@ApplNoB", applnob);
                return OpenDataTable(sql, ht);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="applno"></param>
        /// <param name="applnob"></param>
        /// <returns></returns>
        public DataTable _GetOuterQueryDetailId(string applno, string applnob)
        {
            //資料讀取原則為以NUMSCustomerInfo為主,若UDWRQueryDetail有資料,則視InnerQuery欄位看
            //要不要查,若UDWRQueryDetail沒有資料則預設要查
            try
            {
                string sql = "select a.applno,a.applnob,a.GuaranteeSeqNo,a.CusId, ";
                sql += " ISNULL(b.InnerQuery, 'Y') as InnerQuery , ";
                sql += " ISNULL(b.OuterQuery, 'Y') as OuterQuery  ";
                sql += " from NUMSCustomerInfo a ";
                sql += " left outer join UDWRQueryDetail b on a.Applno=b.Applno and a.applnob=b.applnob and a.cusid=b.cusid  ";
                sql += " where a.ApplNo=@ApplNo and a.ApplNoB=@ApplNoB and ISNULL(b.OuterQuery, 'Y') ='Y' and a.Status='Y' ";
                Hashtable ht = new Hashtable();
                ht.Add("@ApplNo", applno);
                ht.Add("@ApplNoB", applnob);
                return OpenDataTable(sql, ht);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Get JcicProceData
        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>
        public DataTable GetJcicProceData(string status)
        {
            try
            {

                DataTable returnValue = new DataTable();
                string sql = "select * from   JCICProcess where Status=@Status order by CreatedDate";
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@Status", status));
                returnValue = base.Search(sql);
                return returnValue;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// GetCustomerInfo By CusID
        /// </summary>
        /// <param name="strApplno"></param>
        /// <param name="strApplnoB"></param>
        /// <param name="sid"></param>
        /// <returns></returns>
        public DataTable GetCustomerInfoByID(string strApplno, string strApplnoB, string sid)
        {
            try
            {

                DataTable returnValue = new DataTable();

                string sql = "select a.* ,isnull(b.ApplTypeCode,'01') as FlowType    from    NUMSCustomerInfo a ";
                sql += "left outer join NUMSMaster b on a.applno=b.applno and a.applnob =b.applnob ";
                sql += "where a.ApplNo=@ApplNo and a.ApplNoB=@ApplNoB and substring(a.CusId,1,10)=substring(@CusId,1,10) and a.Status='Y'  ";

                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@ApplNo", strApplno));
                base.Parameter.Add(new CommandParameter("@ApplNoB", strApplnoB));
                base.Parameter.Add(new CommandParameter("@CusId", sid));
                returnValue = base.Search(sql);
                return returnValue;
            }
            catch (Exception ex)
            {
                throw ex;
            }



        }

        /// <summary>
        /// Get JCICMQSettingData
        /// </summary>
        /// <param name="jcictype"></param>
        /// <returns></returns>
        public DataTable GetJCICMQSettingData(string jcictype)
        {
            try
            {

                DataTable returnValue = new DataTable();

                string sql = @"select * from  JCICSendConfig where JCICType=@JCICType order by ColSeqNo";
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@JCICType", jcictype));
                returnValue = base.Search(sql);
                return returnValue;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        /// <summary>
        /// add by mel 20130306 
        /// 增加JCIC錯誤訊息的判斷
        /// </summary>
        /// <returns></returns>
        public DataTable GetMQErrorMSG()
        {
            try
            {

                DataTable returnValue = new DataTable();

                string sql = @"select CodeNo,CodeDesc,SortOrder ,CodeTag from parmcode where codetype='JCICMSG' order by sortorder";
                base.Parameter.Clear();
                returnValue = base.Search(sql);
                return returnValue;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Get JCICSeq
        /// </summary>
        /// <param name="hMsgId1"></param>
        /// <param name="jcictype"></param>
        /// <param name="mm"></param>
        /// <returns></returns>
        public int GetJCICSeqNext(string hMsgId1, string jcictype, string mm)
        {
            int seq = 0;
            string selectsql = "SELECT HMSGID1,MM,SEQ FROM JCICSeq  WHERE HMSGID1 = @HMSGID1 And JcicType=@JcicType AND  MM = @MM";
            string updatesql = "UPDATE JCICSeq  SET SEQ = (SEQ + 1),ModifiedUser='GetJCICSeqNext' ,ModifiedDate=getdate() where  HMSGID1 = @HMSGID1 And JcicType=@JcicType AND MM = @MM";
            string insertsql = "INSERT INTO JCICSeq (JcicType, HMSGID1, MM, SEQ,CreatedUser) VALUES (@JcicType, @HMSGID1, @MM, @SEQ,'GetJCICSeqNext')";
            DataTable dtblSeq = new DataTable();
            base.Parameter.Clear();
            base.Parameter.Add(new CommandParameter("@JcicType", jcictype));
            base.Parameter.Add(new CommandParameter("@HMSGID1", hMsgId1));
            base.Parameter.Add(new CommandParameter("@MM", mm));
            dtblSeq = base.Search(selectsql);

            IDbConnection dbConnection = base.OpenConnection();
            IDbTransaction dbTransaction = null;
            using (dbConnection)
            {
                try
                {
                    dbTransaction = dbConnection.BeginTransaction();

                    if (dtblSeq.Rows.Count == 0)
                    {
                        seq = 1;
                        base.Parameter.Clear();
                        base.Parameter.Add(new CommandParameter("@JcicType", jcictype));
                        base.Parameter.Add(new CommandParameter("@HMSGID1", hMsgId1));
                        base.Parameter.Add(new CommandParameter("@MM", mm));
                        base.Parameter.Add(new CommandParameter("@SEQ", seq));
                        base.ExecuteNonQuery(insertsql, dbTransaction, false);
                    }
                    else
                    {
                        seq = int.Parse(dtblSeq.Rows[0]["SEQ"].ToString()) + 1;
                        base.Parameter.Clear();
                        base.Parameter.Add(new CommandParameter("@JcicType", jcictype));
                        base.Parameter.Add(new CommandParameter("@HMSGID1", hMsgId1));
                        base.Parameter.Add(new CommandParameter("@MM", mm));
                        base.ExecuteNonQuery(updatesql, dbTransaction, false);
                    }

                    dbTransaction.Commit();

                }
                catch (Exception ex)
                {
                    dbTransaction.Rollback();
                    throw ex;
                }
            }

            return seq;
        }

        /// <summary>
        /// Get JCICAtomTxnConfigSetting
        /// </summary>
        /// <returns></returns>
        public DataTable GetJCICAtomTxnConfigSetting()
        {
            string strSQL = @" select 'NUMS' as SysID,TxID,Atomid,AtomTable  
                                from JCICAtomTxnConfig
                                where SUBSTRING(TxID,2,1)='Z' and SUBSTRING(TxID,1,1)='Q'
                                and CONVERT(nvarchar(8), getdate(),112) between jcicDateFrom and JCICDateTo
                                union 
                                select distinct 'NUMS' as SysID , 'NUMS' as TxID, atomid,atomTable 
                                from JCICAtomTxnConfig
                                where SUBSTRING(TxID,2,1)<>'Z' and SUBSTRING(TxID,1,1)='Q' 
                                and CONVERT(nvarchar(8), getdate(),112) between jcicDateFrom and JCICDateTo";
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
        /// Get 需刪除Table的List 20150930
        /// </summary>
        /// <returns></returns>
        public DataTable GetJCICAtomTxnConfigSettingForDelete()
        {
            string strSQL = @" select 'NUMS' as SysID,TxID,Atomid,AtomTable  
                                from JCICAtomTxnConfig
                                where SUBSTRING(TxID,2,1)='Z' and SUBSTRING(TxID,1,1)='Q'
                                and CONVERT(nvarchar(8), getdate(),112) between jcicDateFrom and JCICDateTo
                                union 
                                select distinct 'NUMS' as SysID , TxID, atomid,atomTable 
                                from JCICAtomTxnConfig
                                where SUBSTRING(TxID,2,1)<>'Z' and SUBSTRING(TxID,1,1)='Q' 
                                and CONVERT(nvarchar(8), getdate(),112) between jcicDateFrom and JCICDateTo";
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
        /// Get JCICAtomColConfigSetting
        /// </summary>
        /// <returns></returns>
        public DataTable GetJCICAtomColConfigSetting()
        {
            string strSQL = @" select 'NUMS' as SysId, AtomId,ColFieldID,ColDataType,ColDataLen,ColSeqNo from JCICAtomColConfig
                            where  ColSeqNo IS NOT NULL ORDER BY AtomId, ColSeqNo";
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
        /// Get JcicInfomation
        /// </summary>
        /// <param name="strApplno"></param>
        /// <param name="strApplnoB"></param>
        /// <returns></returns>
        public DataTable GetJcicInfo(string strApplno, string strApplnoB)
        {
            try
            {

                DataTable returnValue = new DataTable();

                string sql = @"select a.BusType ,b.MsgId1,b.Bdept,b.BBranch,b.Borg,b.Uid0 ,c.EmpName from  
                                NUMSMaster a 
                                left outer join NUMSBusMaster b on a.BusType=b.BusType 
                                left outer join NUMSEmployee c on b.Uid0=c.EmpID
                                where ApplNo =@ApplNo and ApplNoB=@ApplNoB ";

                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@ApplNo", strApplno));
                base.Parameter.Add(new CommandParameter("@ApplNoB", strApplnoB));
                returnValue = base.Search(sql);
                return returnValue;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// get JCICMappingForeign
        /// </summary>
        /// <param name="sid"></param>
        /// <returns></returns>
        public DataTable ForeignId(string sid)
        {
            try
            {

                DataTable returnValue = new DataTable();

                string sql = @"select * from  JCICMappingForeign where TaxNo=@Sid ";
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@Sid", sid));
                returnValue = base.Search(sql);
                return returnValue;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string ForeignId(string applno, string applnob, string sid)
        {
            try
            {
                string result = "";
                DataTable returnValue = new DataTable();

                //string sql = @"select * from  JCICMappingForeign where TaxNo=@Sid ";
                //edit by mel 20140123 修正用10碼比對
                //string sql = @"select Isnull(ForeignID,'') ForeignID from  NUMSCustomerInfo where ApplNo =@ApplNo and ApplNoB=@ApplNoB  and CusId=@Sid ";
                string sql = @"select Isnull(ForeignID,'') ForeignID from  NUMSCustomerInfo where ApplNo =@ApplNo and ApplNoB=@ApplNoB  and  substring(CusId,1,10)=substring(@Sid,1,10) ";
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@ApplNo", applno));
                base.Parameter.Add(new CommandParameter("@ApplNoB", applnob));
                base.Parameter.Add(new CommandParameter("@Sid", sid));
                returnValue = base.Search(sql);
                if (returnValue.Rows.Count > 0)
                    result = returnValue.Rows[0]["ForeignID"].ToString();

                return result;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Get Deriver data Check Item Data
        /// </summary>
        /// <param name="strApplno"></param>
        /// <param name="strApplnoB"></param>
        /// <param name="strmodule"></param>
        /// <returns></returns>
        public DataTable GetDeriverCheckQItemData(string strApplno, string strApplnoB, string strmodule)
        {
            string strSQL = @"select a.htxid
                            from 
                            (select distinct htxid from JCICSend where ApplNo =@ApplNo and ApplNoB=@ApplNoB and Module=@Module) a
                            INNER  join 
                            (select distinct txid from JCICAtomTxnConfig where IsDerived='Y') b
                            on a.htxid=b.txid";
            DataTable returnValue = new DataTable();

            try
            {
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@ApplNo", strApplno));
                base.Parameter.Add(new CommandParameter("@ApplNoB", strApplnoB));
                base.Parameter.Add(new CommandParameter("@Module", strmodule));
                returnValue = base.Search(strSQL);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return returnValue;
        }

        public DataTable GetDeriverCheckQItemData(string strApplno, string strApplnoB, string sid, string strmodule)
        {
            //edit by mel 20130523 
            //加入檢查當日是否有收回raw data ,若沒有則不作加工運算

            string strSQL = @"select a.htxid
                            from 
                            (
                            select distinct htxid 
                              from JCICSend a Where Applno=@ApplNo And ApplNoB=@ApplNoB And Module=@Module And Bid=@Bid
                             And a.receivedate= CONVERT(varchar(10), GETDATE(), 112) And  IsSend='Y' And IsReceive='Y' 
                            ) a
                            INNER  join 
                            (select distinct txid from JCICAtomTxnConfig where IsDerived='Y') b
                            on a.htxid=b.txid";
            DataTable returnValue = new DataTable();

            try
            {
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@ApplNo", strApplno));
                base.Parameter.Add(new CommandParameter("@ApplNoB", strApplnoB));
                base.Parameter.Add(new CommandParameter("@Module", strmodule));
                base.Parameter.Add(new CommandParameter("@Bid", sid));
                returnValue = base.Search(strSQL);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return returnValue;
        }

        /// <summary>
        /// Get Jcic SendQuery Date
        /// </summary>
        /// <param name="strApplno"></param>
        /// <param name="strApplnoB"></param>
        /// <param name="strmodule"></param>
        /// <param name="qitems"></param>
        /// <returns></returns>
        public DataTable GetJcicSendQueryDate(string strApplno, string strApplnoB, string strmodule, string qitems)
        {
            string filter = "";
            string strSQL = @"select top 1 Htxid,cast((cast(SUBSTRING(senddate,1,4)as int)-1911) as nvarchar(3)) +'/' 
                                +SUBSTRING(SendDate,5,2) + '/' +SUBSTRING(senddate,7,2) as SendDate
                                from JCICSend where ApplNo =@ApplNo and ApplNoB=@ApplNoB and Module=@Module";

            string[] _qitem = qitems.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < _qitem.Length; i++)
            {
                filter += ",'Q" + _qitem[i] + "'";
            }
            strSQL += " and Htxid in (" + filter.Substring(1) + ")";
            DataTable returnValue = new DataTable();

            try
            {
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@ApplNo", strApplno));
                base.Parameter.Add(new CommandParameter("@ApplNoB", strApplnoB));
                base.Parameter.Add(new CommandParameter("@Module", strmodule));
                returnValue = base.Search(strSQL);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return returnValue;
        }

        /// <summary>
        /// get JCICDeriveDatasSetting
        /// </summary>
        /// <returns></returns>
        public DataTable JCICDeriveDatasSetting()
        {
            string strSQL = @"select * from JCICDeriveDataSetting order by dataorder";
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
        /// Check PSRN Jcic FreshDate
        /// </summary>
        /// <param name="strApplno"></param>
        /// <param name="strApplnoB"></param>
        /// <returns></returns>
        public DataTable GetCheckPSRNJcicFreshDate(string strApplno, string strApplnoB)
        {

            string strSQL = @"select distinct b.Htxid
                                from JCICProcess a
                                left outer join JCICSend b
                                on a.ApplNo=b.ApplNo and a.ApplNoB =b.ApplNoB and a.Module =b.Module
                                where 
                                a.ApplNo=@ApplNo and a.ApplNoB=@ApplNoB and a.Module='PSRN' and a.ModifiedDate +
                                cast((select top 1 ISNULL(codetag,0) from PARMCode where CodeType='JCICFreshDay') as int)  
                                > GETDATE()
                                and Status='Z'  ";


            DataTable returnValue = new DataTable();
            try
            {
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@ApplNo", strApplno));
                base.Parameter.Add(new CommandParameter("@ApplNoB", strApplnoB));
                returnValue = base.Search(strSQL);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return returnValue;
        }

        //20130816 edit by mel  IR-62274請確認JCIC拋查邏輯
        public DataTable GetCheckPSRNJcicFreshDate(string strApplno, string strApplnoB, string custid, string hxid)
        {

            string strSQL = @"select a.Bid,a.Htxid ,ISNULL(b.IsDerived,'N') IsDerived
                                from JCICSend  a 
                                left outer join 
                                (select distinct txid ,IsDerived from JCICAtomTxnConfig ) b on a.Htxid=b.TxID 
                                where a.Module='PSRN' and a.IsSend='Y' and IsReceive='Y' 
                                and a.ModifiedDate +cast((select top 1 ISNULL(codetag,0) from PARMCode where CodeType='JCICFreshDay') as int)  > GETDATE() 
                                and a.ApplNo=@ApplNo and a.ApplNoB=@ApplNoB and a.Bid=@Bid and a.Htxid=@Htxid 
                                order by a.ApplNo ,a.Bid ,a.Htxid 
                                 ";


            DataTable returnValue = new DataTable();
            try
            {
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@ApplNo", strApplno));
                base.Parameter.Add(new CommandParameter("@ApplNoB", strApplnoB));
                base.Parameter.Add(new CommandParameter("@Bid", custid));
                base.Parameter.Add(new CommandParameter("@Htxid", hxid));
                returnValue = base.Search(strSQL);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return returnValue;
        }

        //20130816 edit by mel  IR-62274請確認JCIC拋查邏輯
        public DataTable CheckJcicFreshDate(string strApplno, string strApplnoB, string custid, string hxid, string module)
        {

            string strSQL = @"select a.Bid,a.Htxid ,ISNULL(b.IsDerived,'N') IsDerived
                                from JCICSend  a 
                                left outer join 
                                (select distinct txid ,IsDerived from JCICAtomTxnConfig ) b on a.Htxid=b.TxID 
                                where a.Module=@Module and a.IsSend='Y' and IsReceive='Y' 
                                --and a.ModifiedDate +cast((select top 1 ISNULL(codetag,0) from PARMCode where CodeType='JCICFreshDay') as int)  > GETDATE() 
                                and  (
                                a.SendDate+a.SendTime > replace(replace(replace(
						                                CONVERT(nvarchar(20) 
						                                ,(getdate() - cast((select top 1 ISNULL(codetag,0) from PARMCode where CodeType='JCICFreshDay') as int)) 
						                                ,20)
                                ,'-',''),':',''),' ','')
                                )
                                and a.ApplNo=@ApplNo and a.ApplNoB=@ApplNoB and a.Bid=@Bid and a.Htxid=@Htxid 
                                order by a.ApplNo ,a.Bid ,a.Htxid 
                                 ";


            DataTable returnValue = new DataTable();
            try
            {
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@ApplNo", strApplno));
                base.Parameter.Add(new CommandParameter("@ApplNoB", strApplnoB));
                base.Parameter.Add(new CommandParameter("@Bid", custid));
                base.Parameter.Add(new CommandParameter("@Htxid", hxid));
                base.Parameter.Add(new CommandParameter("@Module", module));
                returnValue = base.Search(strSQL);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return returnValue;
        }

        //20130816 edit by mel  IR-62274請確認JCIC拋查邏輯
        public DataTable GetJcicCheckData(string strApplno, string strApplnoB)
        {
            string strSQL = @"select a.applno ,a.ApplTypeCode  ,b.oldApplNo,b.oldApplNoB
                            ,(select COUNT(applno) tot from JCICProcess 
                                where ApplNo=@ApplNo and ApplNoB=@ApplNoB and Module='UDWR') UDWRJcicSendtime
                            from NUMSMaster a
                            left outer join NUMSApplyVerify b 
                            on a.ApplNo=b.ApplNo and a.ApplNoB=b.ApplNoB 
                                where a.ApplNo=@ApplNo and a.ApplNoB=@ApplNoB ";



            DataTable returnValue = new DataTable();

            try
            {
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@ApplNo", strApplno));
                base.Parameter.Add(new CommandParameter("@ApplNoB", strApplnoB));
                returnValue = base.Search(strSQL);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return returnValue;
        }

        //edit by mel 
        public DataTable GetJcicCheckData(string strApplno, string strApplnoB, string cusid, string txid)
        {
            string strSQL = @"select a.applno ,a.ApplTypeCode  ,b.oldApplNo,b.oldApplNoB
                            ,(select COUNT(applno) tot from JCICSend 
                                where ApplNo=@ApplNo and ApplNoB=@ApplNoB and Bid=@Bid and  Module='UDWR' and Htxid=@Htxid 
                             ) UDWRJcicSendtime
                            from NUMSMaster a
                            left outer join NUMSApplyVerify b 
                            on a.ApplNo=b.ApplNo and a.ApplNoB=b.ApplNoB 
                                where a.ApplNo=@ApplNo and a.ApplNoB=@ApplNoB ";



            DataTable returnValue = new DataTable();

            try
            {
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@ApplNo", strApplno));
                base.Parameter.Add(new CommandParameter("@ApplNoB", strApplnoB));
                base.Parameter.Add(new CommandParameter("@Bid", cusid));
                base.Parameter.Add(new CommandParameter("@Htxid", txid));
                returnValue = base.Search(strSQL);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return returnValue;
        }

        /// <summary>
        /// Get DataFor BulkCase
        /// </summary>
        /// <param name="strApplno"></param>
        /// <param name="strApplnoB"></param>
        /// <returns></returns>
        /// <remarks>edit by mel 20131028 只比對整案</remarks>
        /// 20140815 smallzhi 將投資客改成抓徵信的! 
        /// 20140918 smallzhi CusGrade改成用原值
        public DataTable GetDataForBulkCase(string strApplno, string strApplnoB)
        {
            //edit by mel IR-62884 比對邏輯修正如下：請將比對邏輯1~4的比對條件  加上 縣市別及鄉鎮：
            //            string strSQL = @" select a.ApplNo,a.ApplNoB,a.GuaraNo,b.GaranName ,c.AreaNo
            //                            , (select top 1 ContractDate from NUMSMaster where ApplNo=@ApplNo and  ApplNoB=@ApplNoB order by CreatedDate )  as ContractDate  
            //                            ,(select top 1 isnull(b.CodeDesc,'') as CucGrade from UDWRMaster a  left outer join PARMCode b on b.CodeType='CusGrade' and b.CodeNo=a.CusGrade
            //                                where ApplNo=@ApplNo and  ApplNoB=@ApplNoB  )  as  CusGrade
            //                            ,(select top 1 isnull(InvestorsFlag,'N') as InvestorsFlag from NUMSMaster  where ApplNo=@ApplNo and  ApplNoB=@ApplNoB ) as InvestorsFlag 
            //                            , (select top 1 ApplyDate from NUMSMaster where ApplNo=@ApplNo and  ApplNoB=@ApplNoB order by CreatedDate )  as ApplyDate  
            //                            --,Case when b.HouseType='R2' then '大樓' when b.HouseType='R5' then '透天' else b.HouseType end as HouseType
            //                            ,b.HouseType
            //                                ,c.FloorBel
            //                                --,Case when d.StallType='04' then '坡道平面' when d.StallType='03' then '子母' else d.StallType end as StallType
            //                            ,d.StallType                            from APRLGuaranteeMain a 
            //                            left outer join APRLDistinguishMain b on a.ApplNo=b.Applno and  a.ApplNoB=b.ApplnoB and a.GuaraNo=b.GuaraNo
            //                            left outer join APRLGuaranteeBuilding c on a.ApplNo=c.Applno and  a.ApplNoB=c.ApplnoB and a.GuaraNo=c.GuaraNo
            //                            left outer join APRLGuaranteeStallDetail d on c.GbId=d.GbId                         
            //                            where a.ApplNo=@ApplNo and  a.ApplNoB=@ApplNoB  and isnull(a.MbNo,'')='' ";
            //edit by mel 20131028 只比對整案
            //edit by mel 20140107 改InvestorsFlag為Investor
            string strSQL = @"select distinct  a.ApplNo,a.ApplNoB,a.GuaraNo,b.GaranName ,c.AreaNo
                            , (select ContractDate from NUMSMaster where ApplNo = @ApplNo and  ApplNoB = @ApplNoB)  as ContractDate  
                            ,(select a.CusGrade from UDWRMaster a where a.ApplNo = @ApplNo and a.ApplNoB = @ApplNoB)  as  CusGrade
                            ,(select isnull(Investor,'N') as InvestorsFlag from UDWRMaster  where ApplNo = @ApplNo and  ApplNoB = @ApplNoB) as InvestorsFlag  --smallzhi 20140815
                            , (select ApplyDate from NUMSMaster where ApplNo = @ApplNo and  ApplNoB = @ApplNoB)  as ApplyDate  
                            from APRLGuaranteeMain a 
                            left outer join APRLDistinguishMain b on a.ApplNo=b.Applno and  a.ApplNoB=b.ApplnoB and a.GuaraNo=b.GuaraNo
                            left outer join APRLGuaranteeBuilding c on a.ApplNo=c.Applno and  a.ApplNoB=c.ApplnoB and a.GuaraNo=c.GuaraNo
                            where a.ApplNo=@ApplNo and  a.ApplNoB=@ApplNoB  and isnull(a.MbNo,'')='' ";

            DataTable returnValue = new DataTable();
            try
            {
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@ApplNo", strApplno));
                base.Parameter.Add(new CommandParameter("@ApplNoB", strApplnoB));
                returnValue = base.Search(strSQL);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return returnValue;
        }
        /// <summary>
        /// Get DataFor BulkCase
        /// </summary>
        /// <param name="strApplno"></param>
        /// <param name="strApplnoB"></param>
        /// <returns></returns>
        /// <remarks>edit by mel 20131028 只比對主建</remarks>
        public DataTable GetDataForBulkCaseBuilding(string strApplno, string strApplnoB)
        {
            string strSQL = @" select distinct a.ApplNo,a.ApplNoB,a.GuaraNo,b.GaranName ,c.AreaNo
                            , (select top 1 ContractDate from NUMSMaster where ApplNo=@ApplNo and  ApplNoB=@ApplNoB order by CreatedDate )  as ContractDate  
                            , (select top 1 ApplyDate from NUMSMaster where ApplNo=@ApplNo and  ApplNoB=@ApplNoB order by CreatedDate )  as ApplyDate  
                            ,b.HouseType
                            ,c.FloorBel
                            from APRLGuaranteeMain a 
                            left outer join APRLDistinguishMain b on a.ApplNo=b.Applno and  a.ApplNoB=b.ApplnoB and a.GuaraNo=b.GuaraNo
                            left outer join APRLGuaranteeBuilding c on a.ApplNo=c.Applno and  a.ApplNoB=c.ApplnoB and a.GuaraNo=c.GuaraNo
                            where a.ApplNo=@ApplNo and  a.ApplNoB=@ApplNoB  and isnull(a.MbNo,'')=''  ";

            DataTable returnValue = new DataTable();
            try
            {
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@ApplNo", strApplno));
                base.Parameter.Add(new CommandParameter("@ApplNoB", strApplnoB));
                returnValue = base.Search(strSQL);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return returnValue;
        }
        /// <summary>
        /// Get DataFor BulkCase
        /// </summary>
        /// <param name="strApplno"></param>
        /// <param name="strApplnoB"></param>
        /// <returns></returns>
        /// <remarks>edit by mel 20131028 只比對車位</remarks>
        public DataTable GetDataForBulkCaseStall(string strApplno, string strApplnoB)
        {
            string strSQL = @" select a.ApplNo,a.ApplNoB,a.GuaraNo,b.GaranName ,c.AreaNo
                            , (select top 1 ContractDate from NUMSMaster where ApplNo=@ApplNo and  ApplNoB=@ApplNoB order by CreatedDate )  as ContractDate  
                            , (select top 1 ApplyDate from NUMSMaster where ApplNo=@ApplNo and  ApplNoB=@ApplNoB order by CreatedDate )  as ApplyDate  
                            ,d.StallType                            
                            from APRLGuaranteeMain a 
                            left outer join APRLDistinguishMain b on a.ApplNo=b.Applno and  a.ApplNoB=b.ApplnoB and a.GuaraNo=b.GuaraNo
                            left outer join APRLGuaranteeBuilding c on a.ApplNo=c.Applno and  a.ApplNoB=c.ApplnoB and a.GuaraNo=c.GuaraNo
                            left outer join APRLGuaranteeStallDetail d on c.GbId=d.GbId                         
                            where a.ApplNo=@ApplNo and  a.ApplNoB=@ApplNoB  and isnull(a.MbNo,'')=''     ";

            DataTable returnValue = new DataTable();
            try
            {
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@ApplNo", strApplno));
                base.Parameter.Add(new CommandParameter("@ApplNoB", strApplnoB));
                returnValue = base.Search(strSQL);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return returnValue;
        }

        /// <summary>
        /// Get Mapping Whole UDWRBulkCase
        /// </summary>
        /// <param name="bulkcasename"></param>
        /// <param name="casedate"></param>
        /// <param name="investorflag"></param>
        /// <returns></returns>
        public DataTable GetMappingWholeUDWRBulkCase(string bulkcasename, DateTime casedate, string investorflag)
        {
            string strSQL = @"select b.*,a.MaxCreditLimit,a.AproveDate,a.DueDate ,b.BulkCaseType 
                            from UDWRBulkCaseDetail b  
                            left outer join UDWRBulkCaseMaster a on b.BulkCaseNo=a.BulkCaseNo 
                            where  
                            a.BulkCaseName=@BulkCaseName 
                            and (@CaseDate between a.AproveDate and a.DueDate) 
                            and b.InvestorsFlag=@InvestorsFlag and b.BulkCaseType='00'  ";

            DataTable returnValue = new DataTable();
            try
            {
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@BulkCaseName", bulkcasename));
                base.Parameter.Add(new CommandParameter("@CaseDate", casedate));
                base.Parameter.Add(new CommandParameter("@InvestorsFlag", investorflag));
                returnValue = base.Search(strSQL);
            }

            catch (Exception ex)
            {
                throw ex;
            }
            return returnValue;
        }

        /// 20140918 smallzhi add contractdate
        public DataTable GetMappingUDWRBulkCase(string bulkcasename, DateTime casedate, DateTime contractdate, string areano)
        {
            string strSQL = @"select b.*,a.MaxCreditLimit,a.AproveDate,a.DueDate ,b.BulkCaseType 
                            from UDWRBulkCaseDetail b  
                            left outer join UDWRBulkCaseMaster a on b.BulkCaseNo = a.BulkCaseNo 
                            where  
                            a.BulkCaseName = @BulkCaseName 
                            and ((@CaseDate between a.AproveDate and a.DueDate) or (@ContractDate between a.AproveDate and a.DueDate)) --申請書填寫日跟申請日任一符合, 即符合 20140918 smallzhi
                            and a.BulkCaseZip = @BulkCaseZip   ";

            DataTable returnValue = new DataTable();
            try
            {
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@BulkCaseName", bulkcasename));
                base.Parameter.Add(new CommandParameter("@CaseDate", casedate));
                base.Parameter.Add(new CommandParameter("@ContractDate", contractdate)); //20140918 smallzhi
                base.Parameter.Add(new CommandParameter("@BulkCaseZip", areano));
                returnValue = base.Search(strSQL);
            }

            catch (Exception ex)
            {
                throw ex;
            }
            return returnValue;
        }


        //edit by mel IR-62884 比對邏輯修正如下：請將比對邏輯1~4的比對條件  加上 縣市別及鄉鎮：
        public DataTable GetMappingWholeUDWRBulkCase(string bulkcasename, DateTime casedate, string investorflag, string areano, string cusgrade)
        {
            string strSQL = @"select b.*,a.MaxCreditLimit,a.AproveDate,a.DueDate ,b.BulkCaseType 
                            from UDWRBulkCaseDetail b  
                            left outer join UDWRBulkCaseMaster a on b.BulkCaseNo=a.BulkCaseNo 
                            where  
                            a.BulkCaseName=@BulkCaseName 
                            and (@CaseDate between a.AproveDate and a.DueDate) 
                            and b.InvestorsFlag=@InvestorsFlag and b.BulkCaseType='00' 
                            and a.BulkCaseZip=@BulkCaseZip  
                            and b.CustGrade =@CustGrade ";

            DataTable returnValue = new DataTable();
            try
            {
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@BulkCaseName", bulkcasename));
                base.Parameter.Add(new CommandParameter("@CaseDate", casedate));
                base.Parameter.Add(new CommandParameter("@InvestorsFlag", investorflag));
                base.Parameter.Add(new CommandParameter("@BulkCaseZip", areano));
                base.Parameter.Add(new CommandParameter("@CustGrade", cusgrade));
                returnValue = base.Search(strSQL);
            }

            catch (Exception ex)
            {
                throw ex;
            }
            return returnValue;
        }

        /// <summary>
        /// Get Mappin gBuilding to UDWRBulkCase
        /// </summary>
        /// <param name="strBulkCaseName"></param>
        /// <param name="CaseDate"></param>
        /// <param name="strFloorBel"></param>
        /// <param name="strHouseType"></param>
        /// <returns></returns>
        public DataTable GetMappingBuildingUDWRBulkCase(string strBulkCaseName, DateTime CaseDate, string strFloorBel, string strHouseType)
        {
            string strSQL = @"select b.*,a.MaxCreditLimit,a.AproveDate,a.DueDate ,b.BulkCaseType 
                            from UDWRBulkCaseDetail b   
                            left outer join UDWRBulkCaseMaster a on b.BulkCaseNo=a.BulkCaseNo 
                            where   
                            a.BulkCaseName=@BulkCaseName  
                            and (@CaseDate between a.AproveDate and a.DueDate)  
                            and b.Floor=@Floor  
                            and b.HouseType=@HouseType and  b.BulkCaseType='01' ";

            DataTable returnValue = new DataTable();
            try
            {
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@BulkCaseName", strBulkCaseName));
                base.Parameter.Add(new CommandParameter("@CaseDate", CaseDate));
                base.Parameter.Add(new CommandParameter("@Floor", strFloorBel));
                base.Parameter.Add(new CommandParameter("@HouseType", strHouseType));
                returnValue = base.Search(strSQL);
            }

            catch (Exception ex)
            {
                throw ex;
            }
            return returnValue;
        }

        //edit by mel 20130930 IR-62883 
        /*因層次有可能會出現中文  導致沒法比對區間
            所以，經確認後  第3項的比對邏輯 只比對  比對案名+房屋種類  ，取出單價。
         */
        //edit by mel IR-62884 比對邏輯修正如下：請將比對邏輯1~4的比對條件  加上 縣市別及鄉鎮：
        public DataTable GetMappingBuildingUDWRBulkCase(string strBulkCaseName, string areano, DateTime CaseDate, string strHouseType)
        {
            string strSQL = @"select b.*,a.MaxCreditLimit,a.AproveDate,a.DueDate ,b.BulkCaseType 
                            from UDWRBulkCaseDetail b   
                            left outer join UDWRBulkCaseMaster a on b.BulkCaseNo=a.BulkCaseNo 
                            where   
                            a.BulkCaseName=@BulkCaseName  
                            and (@CaseDate between a.AproveDate and a.DueDate)  
                            and b.HouseType=@HouseType and  b.BulkCaseType='01'  
                            and a.BulkCaseZip=@BulkCaseZip  ";
            DataTable returnValue = new DataTable();
            try
            {
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@BulkCaseName", strBulkCaseName));
                base.Parameter.Add(new CommandParameter("@CaseDate", CaseDate));
                base.Parameter.Add(new CommandParameter("@HouseType", strHouseType));
                base.Parameter.Add(new CommandParameter("@BulkCaseZip", areano));
                returnValue = base.Search(strSQL);
            }

            catch (Exception ex)
            {
                throw ex;
            }
            return returnValue;
        }

        /// <summary>
        /// Get Mapping Stall to UDWRBulkCase
        /// </summary>
        /// <param name="strBulkCaseName"></param>
        /// <param name="CaseDate"></param>
        /// <param name="strStallType"></param>
        /// <returns></returns>
        public DataTable GetMappingStallUDWRBulkCase(string strBulkCaseName, DateTime CaseDate, string strStallType)
        {
            string strSQL = @"select b.*,a.MaxCreditLimit,a.AproveDate,a.DueDate ,b.BulkCaseType 
                            from UDWRBulkCaseDetail b   
                            left outer join UDWRBulkCaseMaster a on b.BulkCaseNo=a.BulkCaseNo 
                            where   
                            a.BulkCaseName=@BulkCaseName  
                            and (@CaseDate between a.AproveDate and a.DueDate)  
                            and b.StallType=@StallType  
                            and b.BulkCaseType='02' ";


            DataTable returnValue = new DataTable();
            try
            {
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@BulkCaseName", strBulkCaseName));
                base.Parameter.Add(new CommandParameter("@CaseDate", CaseDate));
                base.Parameter.Add(new CommandParameter("@StallType", strStallType));

                returnValue = base.Search(strSQL);
            }

            catch (Exception ex)
            {
                throw ex;
            }
            return returnValue;
        }

        //edit by mel IR-62884 比對邏輯修正如下：請將比對邏輯1~4的比對條件  加上 縣市別及鄉鎮：
        public DataTable GetMappingStallUDWRBulkCase(string strBulkCaseName, string areano, DateTime CaseDate, string strStallType)
        {
            string strSQL = @"select b.*,a.MaxCreditLimit,a.AproveDate,a.DueDate ,b.BulkCaseType 
                            from UDWRBulkCaseDetail b   
                            left outer join UDWRBulkCaseMaster a on b.BulkCaseNo=a.BulkCaseNo 
                            where   
                            a.BulkCaseName=@BulkCaseName  
                            and (@CaseDate between a.AproveDate and a.DueDate)  
                            and b.StallType=@StallType  
                            and b.BulkCaseType='02'   
                            and a.BulkCaseZip=@BulkCaseZip  ";

            DataTable returnValue = new DataTable();
            try
            {
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@BulkCaseName", strBulkCaseName));
                base.Parameter.Add(new CommandParameter("@CaseDate", CaseDate));
                base.Parameter.Add(new CommandParameter("@StallType", strStallType));
                base.Parameter.Add(new CommandParameter("@BulkCaseZip", areano));

                returnValue = base.Search(strSQL);
            }

            catch (Exception ex)
            {
                throw ex;
            }
            return returnValue;
        }

        /// <summary>
        /// Get Creditor Emp List
        /// edit by mel 20130304 CR 增加分件rule
        /// </summary>
        /// <param name="creditorrole"></param>
        /// <returns></returns>
        public DataTable GetCreditorEmpIDList(string creditorrole)
        {
            string[] rolelist = creditorrole.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            string filter = "";

            if (rolelist.Length > 0)
            {
                foreach (string role in rolelist)
                {
                    filter += "or BUMasterID like '" + role + "%'";
                }
                filter = filter.Substring(2);
            }
            else
            {
                throw new Exception("no BUMasterID !!!");
            }
            string strSQL = "select empid ,Cast('0' as int) as DispatchCnt,'N' as IsDispatch ,BUMasterID from NUMSBUToEmployee " +
                            "where " + filter;

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
        /// edit by mel ,直接讀取資料
        /// </summary>
        /// <returns></returns>
        public DataTable GetCreditorEmpIDList()
        {

            //string strSQL = "select empid ,Cast('0' as int) as DispatchCnt,'N' as IsDispatch ,BUMasterID from NUMSBUToEmployee order by BUMasterID,empid ";
            string strSQL = "select empid ,DispatchCnt, IsDispatch ,BUMasterID from NUMSBUToEmployee order by BUMasterID, DispatchCnt desc ";

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
        /// Get ProxyIncome Parm
        /// </summary>
        /// <param name="strApplno"></param>
        /// <param name="strApplnoB"></param>
        /// <returns></returns>
        public DataTable GetProxyIncomeParm(string strApplno, string strApplnoB)
        {
            string strSQL = @"SELECT     
                            cus.ApplNo APPLNO, 
                            cus.CusId CUST_ID, 
                            isnull(cus.CusAge,0) CUST_AGE, 
                            cus.CusSex CUST_SEX, 
                            cus.EducStatus EDUCATION, 
                            isnull(cus.JobCC,'00') CC, 
                            isnull(cus.JobOC,'00') OC,
                            cus.MaritalStatus MARRY, 
                            isnull(cus.JobTime,'0000')  SENIORITY,
                            isnull(cus.JobZip,'000') POST_CODE_COMP_3 , 
                            isnull(cus.ComZip,'000') POST_CODE_CONTACT_3 ,
                            isnull(ccoc.StableFlag,'') D_STABLE_FLAG, 
                            isnull(ccoc.CardGroup,'') SCORE_GROUP , 
                            isnull(jcmh.SysMinCo1 ,0) JCMH_SYS_MINCO1, 
                            isnull(jcmh.GumhIncomeDate,'00000000') JCMH_INCOME_DATE, 
                            '' AB_037,
                            '' AB_046,
                            '' AB_049,
                            '' GR_016,
                            '' GR_019,
                            '' GR_022,
                            '' GR_028,
                            '' GR_029,
                            '' GR_030,
                            '' GR_034,
                            '' GR_035,
                            '' GR_036,
                            '' INCOME_GP,
                            '' WORK_YEAR,
                            '' k22_2yr_stdr_avg,
                            '' k22_3yr_stdr_avg,
                            isnull(jcicm.Column220,0)-isnull(jcicm.Column221,0) Spend_CA_Y,
                            isnull(jcicm.Column220,0)-isnull(jcicm.Column221,0)-isnull(jcicm.Column222,0) Spend_Y,
                            isnull(jcicm.Column51,0) EJCIC_COL_51 , 
                            isnull(jcicm.Column52,0) EJCIC_COL_52    ,
                            isnull(jcicm.Column53,0) EJCIC_COL_53    ,
                            isnull(jcicm.Column55,0) EJCIC_COL_55    ,
                            isnull(jcicm.Column56,0) EJCIC_COL_56    ,
                            isnull(jcicm.Column66,0) EJCIC_COL_66    ,
                            isnull(jcicm.Column74,0) EJCIC_COL_74    ,
                            isnull(jcicm.Column77,0) EJCIC_COL_77    ,
                            isnull(jcicm.Column78,0) EJCIC_COL_78    ,
                            isnull(jcicm.Column81,0) EJCIC_COL_81    ,
                            isnull(jcicm.Column84,0) EJCIC_COL_84    ,
                            isnull(jcicm.Column87,0) EJCIC_COL_87    ,
                            isnull(jcicm.Column93,0) EJCIC_COL_93    ,
                            isnull(jcicm.Column94,0) EJCIC_COL_94    ,
                            isnull(jcicm.Column95,0) EJCIC_COL_95    ,
                            isnull(jcicm.Column98,0) EJCIC_COL_98    ,
                            isnull(jcicm.Column101,0) EJCIC_COL_101    ,
                            isnull(jcicm.Column102,0) EJCIC_COL_102    ,
                            isnull(jcicm.Column104,0) EJCIC_COL_104    ,
                            isnull(jcicm.Column105,0) EJCIC_COL_105    ,
                            isnull(jcicm.Column107,0) EJCIC_COL_107    ,
                            isnull(jcicm.Column108,0) EJCIC_COL_108    ,
                            isnull(jcicm.Column115,0) EJCIC_COL_115    ,
                            isnull(jcicm.Column122,0) EJCIC_COL_122    ,
                            isnull(jcicm.Column146,0) EJCIC_COL_146    ,
                            isnull(jcicm.Column147,0) EJCIC_COL_147    ,
                            isnull(jcicm.Column148,0) EJCIC_COL_148    ,
                            isnull(jcicm.Column149,0) EJCIC_COL_149    ,
                            isnull(jcicm.Column153,0) EJCIC_COL_153    ,
                            isnull(jcicm.Column154,0) EJCIC_COL_154    ,
                            isnull(jcicm.Column156,0) EJCIC_COL_156    ,
                            isnull(jcicm.Column179,0) EJCIC_COL_179    ,
                            isnull(jcicm.Column180,0) EJCIC_COL_180    ,
                            isnull(jcicm.Column181,0) EJCIC_COL_181    ,
                            isnull(jcicm.Column182,0) EJCIC_COL_182    ,
                            isnull(jcicm.Column183,0) EJCIC_COL_183    ,
                            isnull(jcicm.Column184,0) EJCIC_COL_184    ,
                            isnull(jcicm.Column185,0) EJCIC_COL_185    ,
                            isnull(jcicm.Column188,0) EJCIC_COL_188    ,
                            isnull(jcicm.Column189,0) EJCIC_COL_189    ,
                            isnull(jcicm.Column190,0) EJCIC_COL_190    ,
                            isnull(jcicm.Column199,0) EJCIC_COL_199    ,
                            isnull(jcicm.Column200,0) EJCIC_COL_200    ,
                            isnull(jcicm.Column209,0) EJCIC_COL_209    ,
                            isnull(jcicm.Column220,0) EJCIC_COL_220    ,
                            isnull(jcicm.Column221,0) EJCIC_COL_221    ,
                            isnull(jcicm.Column222,0) EJCIC_COL_222    ,
                            isnull(jcicm.Column223,0) EJCIC_COL_223    ,
                            isnull(jcicm.Column225,0) EJCIC_COL_225    ,
                            isnull(jcicm.Column226,0) EJCIC_COL_226    ,
                            isnull(jcicm.Column229,0) EJCIC_COL_229    ,
                            isnull(jcicm.Column231,0) EJCIC_COL_231    ,
                            isnull(jcicm.Column232,0) EJCIC_COL_232    ,
                            isnull(jcicm.Column233,0) EJCIC_COL_233    ,
                            isnull(jcicm.Column243,0) EJCIC_COL_243    ,
                            isnull(jcicm.Column244,0) EJCIC_COL_244    ,
                            isnull(jcicm.Column263,0) EJCIC_COL_263    ,
                            isnull(jcicm.Column264,0) EJCIC_COL_264    ,
                            isnull(jcicm.Column265,0) EJCIC_COL_265    ,
                            isnull(jcicm.Column266,0) EJCIC_COL_266    ,
                            isnull(jcicm.Column267,0) EJCIC_COL_267    ,
                            isnull(jcicm.Column268,0) EJCIC_COL_268    ,
                            isnull(jcicm.Column269,0) EJCIC_COL_269    ,
                            isnull(jcicm.Column270,0) EJCIC_COL_270    ,
                            isnull(jcicm.Column271,0) EJCIC_COL_271    ,
                            isnull(jcicm.Column272,0) EJCIC_COL_272    ,
                            isnull(jcicm.Column273,0) EJCIC_COL_273    ,
                            isnull(jcicm.Column274,0) EJCIC_COL_274    ,
                            isnull(jcicm.Column275,0) EJCIC_COL_275    ,
                            isnull(jcicm.Column276,0) EJCIC_COL_276    ,
                            isnull(jcicm.Column277,0) EJCIC_COL_277    ,
                            isnull(jcicm.Column278,0) EJCIC_COL_278    ,
                            isnull(jcicm.Column279,0) EJCIC_COL_279    ,
                            isnull(jcicm.Column280,0) EJCIC_COL_280    ,
                            isnull(jcicm.Column281,0) EJCIC_COL_281    ,
                            isnull(jcicm.Column282,0) EJCIC_COL_282    ,
                            isnull(jcicm.Column283,0) EJCIC_COL_283    ,
                            isnull(jcicm.Column284,0) EJCIC_COL_284    ,
                            isnull(jcicm.Column285,0) EJCIC_COL_285    ,
                            isnull(jcicm.Column286,0) EJCIC_COL_286    ,
                            isnull(jcicm.Column287,0) EJCIC_COL_287    ,
                            isnull(jcicm.Column288,0) EJCIC_COL_288    ,
                            isnull(jcicm.Column289,0) EJCIC_COL_289    ,
                            isnull(jcicm.Column290,0) EJCIC_COL_290    ,
                            isnull(jcicm.Column291,0) EJCIC_COL_291    ,
                            isnull(jcicm.Column292,0) EJCIC_COL_292    ,
                            isnull(jcicm.Column297,0) EJCIC_COL_297    ,
                            isnull(jcicm.Column298,0) EJCIC_COL_298    ,
                            isnull(jcicm.Column301,0) EJCIC_COL_301    ,
                            isnull(jcicm.Column302,0) EJCIC_COL_302    ,
                            isnull(jcicm.Column303,0) EJCIC_COL_303    ,
                            isnull(jcicm.Column304,0) EJCIC_COL_304    ,
                            isnull(jcicm.Column305,0) EJCIC_COL_305    ,
                            isnull(jcicm.Column306,0) EJCIC_COL_306    ,
                            isnull(jcicm.Column307,0) EJCIC_COL_307    ,
                            isnull(jcicm.Column308,0) EJCIC_COL_308    ,
                            isnull(jcicm.Column309,0) EJCIC_COL_309    ,
                            isnull(jcicm.Column310,0) EJCIC_COL_310    ,
                            isnull(jcicm.Column311,0) EJCIC_COL_311    ,
                            isnull(jcicm.Column312,0) EJCIC_COL_312    ,
                            isnull(jcicm.Column313,0) EJCIC_COL_313    ,
                            isnull(jcicm.Column314,0) EJCIC_COL_314    ,
                            isnull(jcicm.Column315,0) EJCIC_COL_315    ,
                            isnull(jcicm.Column316,0) EJCIC_COL_316    ,
                            udc.UnsecRiskGrade TARG 
                            FROM         NUMSCustomerInfo AS cus 
                            LEFT OUTER JOIN JCICMaster AS jcicm ON cus.ApplNo = jcicm.ApplNo AND cus.ApplNoB = jcicm.ApplNoB AND cus.CusId = jcicm.CustID AND jcicm.Module = 'UDWR' 
                            LEFT OUTER JOIN PARMCCOCStable AS ccoc ON cus.JobCC = ccoc.CC AND cus.JobOC = ccoc.OC 
                            LEFT OUTER JOIN NUMSCardJCMHMaster AS jcmh ON cus.ApplNo = jcmh.ApplNo AND cus.ApplNoB = jcmh.ApplNoB AND cus.CusId = jcmh.ID and jcmh.Module='UDWR' 
                            LEFT OUTER JOIN UDWRDetailByCusId as udc on cus.ApplNo=udc.ApplNo and cus.ApplNoB=udc.ApplNoB and cus.CusId=udc.CusId 
                            WHERE cus.ApplNo=@ApplNo and  cus.ApplNoB=@ApplNoB and isnull(cus.Status,'Y')='Y' ";

            DataTable returnValue = new DataTable();
            try
            {
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@ApplNo", strApplno));
                base.Parameter.Add(new CommandParameter("@ApplNoB", strApplnoB));
                returnValue = base.Search(strSQL);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return returnValue;
        }

        /// <summary>
        /// Get TARG ParmData
        /// 20150708 增加Column 166
        /// </summary>
        /// <param name="strApplno"></param>
        /// <param name="strApplnoB"></param>
        /// <returns></returns>
        public DataTable GetTARGParmData(string strApplno, string strApplnoB)
        {
            //edit by mel 20130108 修改學歷對照
            string strSQL = @"select b.CusId,
                                isnull( b.JobCC,'') as 'AA02-CC',
                                isnull(b.JobOC,'') as 'AA03-OC',
                                isnull(b.MaritalStatus,'')  as 'A03-MARRY',
                                isnull(b.CusSex,'') as 'A04-CUSSEX',
                                Case isnull(b.EducStatus,'0') 
                                when '7' then '1'
                                when '8' then '1'
                                else isnull(b.EducStatus,'0')  end as 'A05-EDUCATION1' ,
                                isnull(Cast(b.CusAge as nvarchar(50)),'') as 'A07-CUSAGE',
                                case when b.HouseTime is null then isnull(Cast(b.HouseTime as nvarchar(50)),'') 
                                else Cast(Cast(substring(b.HouseTime,1,2) as int) as nvarchar(50))  end as 'A15-HIUSETIME',
                                case when b.JobTime is null then isnull(Cast(b.JobTime as nvarchar(50)),'') 
                                else Cast(Cast(substring(b.JobTime,1,2) as int) as nvarchar(50))  end as 'A23-JOBTIME',
                                isnull(Cast(c.Column20 as nvarchar(50)),'') as   'A17-COLUMN20',
                                isnull(Cast(c.Column21 as nvarchar(50)),'') as   'A36-COLUMN21',
                                isnull(Cast(c.Column22 as nvarchar(50)),'') as   'A08-COLUMN22',
                                isnull(Cast(c.Column24 as nvarchar(50)),'') as   'A18-COLUMN24',
                                isnull(Cast(c.Column25 as nvarchar(50)),'') as   'A44-COLUMN25',
                                isnull(Cast(c.Column52 as nvarchar(50)),'') as   'A30-COLUMN52',
                                isnull(Cast(c.Column54 as nvarchar(50)),'') as   'B01-COLUMN54',
                                isnull(Cast(c.Column55 as nvarchar(50)),'') as   'A20-COLUMN55',
                                isnull(Cast(c.Column64 as nvarchar(50)),'') as   'A24-COLUMN64',
                                isnull(Cast(c.Column70 as nvarchar(50)),'') as   'AA06-COLUMN70',
                                isnull(Cast(c.Column20 as nvarchar(50)),'') as   'A17-COLUMN20',
                                isnull(Cast(c.Column74 as nvarchar(50)),'') as   'B02-COLUMN74',
                                isnull(Cast(c.Column77 as nvarchar(50)),'') as   'A25-COLUMN77',
                                isnull(Cast(c.Column83 as nvarchar(50)),'') as   'A43-COLUMN83',
                                isnull(Cast(c.Column84 as nvarchar(50)),'') as   'B03-COLUMN84',
                                isnull(Cast(c.Column85 as nvarchar(50)),'') as   'A41-COLUMN85',
                                isnull(Cast(c.Column88 as nvarchar(50)),'') as   'A38-COLUMN88',
                                isnull(Cast(c.Column93 as nvarchar(50)),'') as   'AA04-COLUMN93',
                                isnull(Cast(c.Column98 as nvarchar(50)),'') as   'A09-COLUMN98',
                                isnull(Cast(c.Column148 as nvarchar(50)),'') as   'B04-COLUMN148',
                                isnull(Cast(c.Column154 as nvarchar(50)),'') as   'B05-COLUMN154',
                                isnull(Cast(c.Column157 as nvarchar(50)),'') as   'B06-COLUMN157',
                                Ltrim(Rtrim(isnull(Cast(c.Column162 as nvarchar(50)),''))) as   'AA01-COLUMN162',
                                Ltrim(Rtrim(isnull(Cast(c.Column163 as nvarchar(50)),''))) as   'B07-COLUMN163',
                                isnull(Cast(c.Column166 as nvarchar(50)),'0') as   'A53-COLUMN166',
                                isnull(Cast(c.Column182 as nvarchar(50)),'') as   'A31-COLUMN182',
                                isnull(Cast(c.Column187 as nvarchar(50)),'') as   'A46-COLUMN187',
                                isnull(Cast(c.Column188 as nvarchar(50)),'') as   'B08-COLUMN188',
                                isnull(Cast(c.Column191 as nvarchar(50)),'') as   'A34-COLUMN191',
                                isnull(Cast(c.Column196 as nvarchar(50)),'') as   'A19-COLUMN196',
                                isnull(Cast(c.Column197 as nvarchar(50)),'') as   'A35-COLUMN197',
                                isnull(Cast(c.Column201 as nvarchar(50)),'') as   'B09-COLUMN201',
                                isnull(Cast(c.Column206 as nvarchar(50)),'') as   'A32-COLUMN206',
                                isnull(Cast(c.Column226 as nvarchar(50)),'') as   'A10-COLUMN226',       
                                isnull(Cast(c.Column231 as nvarchar(50)),'') as   'AA05-COLUMN231',
                                isnull(Cast(c.Column232 as nvarchar(50)),'') as   'A42-COLUMN232', 
                                isnull(Cast(c.Column243 as nvarchar(50)),'') as   'A45-COLUMN243', 
                                isnull(Cast(c.Column278 as nvarchar(50)),'') as   'A11-COLUMN278', 
                                isnull(Cast(c.Column279 as nvarchar(50)),'') as   'A12-COLUMN279', 
                                isnull(Cast(c.Column283 as nvarchar(50)),'') as   'A33-COLUMN283', 
                                isnull(Cast(c.Column287 as nvarchar(50)),'') as   'A13-COLUMN287', 
                                isnull(Cast(c.Column320 as nvarchar(50)),'') as   'A26-COLUMN320', 
                                isnull(Cast(c.Column321 as nvarchar(50)),'') as   'A21-COLUMN321', 
                                isnull(Cast(c.Column322 as nvarchar(50)),'') as   'A47-COLUMN322', 
                                isnull(Cast(c.Column323 as nvarchar(50)),'') as   'B10-COLUMN323', 
                                isnull(Cast(c.Column324 as nvarchar(50)),'') as   'B11-COLUMN324',
                                isnull(Cast(c.Column325 as nvarchar(50)),'') as   'B12-COLUMN325',
                                isnull(Cast(c.Column326 as nvarchar(50)),'') as   'A39-COLUMN326',
                                isnull(Cast(c.Column327 as nvarchar(50)),'') as   'A29-COLUMN327',
                                isnull(Cast(c.Column328 as nvarchar(50)),'') as   'A14-COLUMN328',
                                isnull(Cast(c.Column329 as nvarchar(50)),'') as   'A40-COLUMN329',
                                isnull(Cast(c.Column330 as nvarchar(50)),'') as   'A22-COLUMN330'
                                from NUMSCustomerInfo b 
                                left outer join jcicMaster c on b.applno=c.applno and b.applnob=c.applnob and b.CusId=c.CustID and c.Module='UDWR'
                                where 
                                b.ApplNo=@ApplNo and b.ApplNoB=@ApplNoB and 
                                isnull(b.Status,'Y')='Y'  ";

            DataTable returnValue = new DataTable();
            try
            {
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@ApplNo", strApplno));
                base.Parameter.Add(new CommandParameter("@ApplNoB", strApplnoB));
                returnValue = base.Search(strSQL);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return returnValue;
        }

        /// <summary>
        /// Get NewLoan Parm Data
        /// </summary>
        /// <param name="strApplno"></param>
        /// <param name="strApplnoB"></param>
        /// <returns></returns>
        public DataTable GetNewLoanParmData(string strApplno, string strApplnoB)
        {
            //            string strSQL = @"select distinct cus.ApplNo,cus.ApplNoB,cus.CusId 
            //                            ,edata.ID,edata.Column1 as COLUMN_1,edata.Column260 as COLUMN_260
            //                            ,edata.Column261 as COLUMN_261
            //                            ,edata.BAS_YYYYMM,edata.B28_DATA
            //                            from NUMSCustomerInfo cus left outer join 
            //                            (
            //                            SELECT e.ApplNo ,e.ApplNoB,e.CustID As ID,e.Column1,e.Column260,e.Column261  ,
            //                            b29.PayLoanDate + ',' + b29.AccountCode + ',' + b29.Accountcode2 + ',' + CAST(b29.NewContractAmt AS nvarchar(10)) AS B28_DATA 
            //                            ,isnull(CAST(CAST(bam95.DataYYY AS int) + 1911 AS nvarchar(4)) + bam95.DataMM ,'') as  BAS_YYYYMM
            //                            FROM   JCICAtomBAM029  b29  
            //                            inner join JCICMaster e 
            //                            on b29.ApplNo=e.ApplNo and b29.ApplNoB =e.ApplNoB  and b29.CustID=e.CustID
            //                            and CONVERT(varchar(12) ,b29.QryDate, 112 )  =e.Column1 
            //                            and b29.Module=e.Module
            //                            left outer join JCICAtomBAM095 bam95
            //                            on b29.ApplNo=bam95.ApplNo and b29.ApplNoB =bam95.ApplNoB  and bam95.CustID=e.CustID
            //                            where 
            //                            b29.Bankcode like '822%' 
            //                            and B29.NewLoanAmt > 0  
            //                            and B29.module='UDWR' 
            //                            ) edata on cus.ApplNo=edata.ApplNo and cus.ApplNoB=edata.ApplNoB and cus.CusId=edata.ID  
            //                            where  cus.ApplNo=@ApplNo and cus.ApplNoB=@ApplNoB and ISNULL(cus.status,'Y')='Y' ";

            string strSQL = @"select distinct cus.ApplNo,cus.ApplNoB,cus.CusId 
                    ,edata.ID,edata.Column1 as COLUMN_1,edata.Column260 as COLUMN_260
                    ,edata.Column261 as COLUMN_261
                    ,edata.BAS_YYYYMM,edata.B28_DATA
                    from NUMSCustomerInfo cus left outer join 
                    (
	                    SELECT e.ApplNo ,e.ApplNoB,e.CustID As ID,e.Column1,e.Column260,e.Column261  ,
	                    b29.PayLoanDate + ',' + b29.AccountCode + ',' + b29.Accountcode2 + ',' + CAST(b29.NewContractAmt AS nvarchar(10)) AS B28_DATA 
	                    ,isnull(CAST(CAST(bam95.DataYYY AS int) + 1911 AS nvarchar(4)) + bam95.DataMM ,'') as  BAS_YYYYMM
	                    FROM JCICMaster e 
	                    left outer join JCICAtomBAM029 b29
	                    on e.ApplNo=b29.ApplNo and e.ApplNoB=b29.ApplNoB  and e.CustID =b29.CustID and e.module =b29.module  and b29.Bankcode like '822%' and B29.NewLoanAmt > 0  
	                    left outer join JCICAtomBAM095 bam95
	                    on e.ApplNo=bam95.ApplNo and e.ApplNoB =bam95.ApplNoB  and e.CustID=bam95.CustID and e.module =bam95.module 
	                    where 
	                    e.module='UDWR' 
                    ) edata on cus.ApplNo=edata.ApplNo and cus.ApplNoB=edata.ApplNoB and cus.CusId=edata.ID  
                    where  cus.ApplNo=@ApplNo and cus.ApplNoB=@ApplNoB and ISNULL(cus.status,'Y')='Y'  ";

            DataTable returnValue = new DataTable();

            try
            {
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@ApplNo", strApplno));
                base.Parameter.Add(new CommandParameter("@ApplNoB", strApplnoB));
                returnValue = base.Search(strSQL);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return returnValue;
        }

        /// <summary>
        /// Get  GetPARMEgActMap
        /// </summary>
        /// <returns></returns>
        public DataTable GetPARMEgActMap(string AccountCode)
        {
            string strSQL = @" SELECT *   FROM PARMEgActMap   where AccountCode =@AccountCode ";

            DataTable returnValue = new DataTable();
            try
            {
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@AccountCode", AccountCode));

                returnValue = base.Search(strSQL);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return returnValue;
        }

        /// <summary>
        /// 取得科目的權重
        /// </summary>
        /// <param name="AccountCode"></param>
        /// <returns></returns>
        public double GetPARMEgActMapWeight(string AccountCode)
        {

            double weight = 0;
            string strSQL = @" SELECT  isnull(Weight,0) Weight   FROM PARMEgActMap   where AccountCode =@AccountCode ";

            DataTable returnValue = new DataTable();
            try
            {
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@AccountCode", AccountCode));

                returnValue = base.Search(strSQL);
                if (returnValue.Rows.Count > 0)
                {
                    weight = Convert.ToDouble(returnValue.Rows[0]["Weight"].ToString());
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }

            return weight;
        }


        /// <summary>
        /// Get Htg DataMapping
        /// </summary>
        /// <returns></returns>
        public DataTable GetHtgDataMapping()
        {
            string strSQL = @"select CodeNo as Txid, CodeDesc as WiiTable,CodeTag as Numstable from PARMCode where CodeType = 'HTGMapping' order by CodeNo ";

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

        public DataTable GetHistoryKeepMM()
        {
            string strSQL = @"select * from parmcode  where CodeType ='HistoryKeepMM' order by sortorder ";

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
        /// Get Custiomer Data ForFraud
        /// </summary>
        /// <param name="strApplno"></param>
        /// <param name="strApplnoB"></param>
        /// <returns></returns>
        public DataTable GetCusDataForFraud(string strApplno, string strApplnoB)
        {
            string strSQL = @"SELECT     CusId
                                ,CusId as FRAUD_ID
                                ,CusName+'|'+isnull(substring(JobUnit,1,4),'') as FRAUD_NAME
                                ,isnull(ComTelNo,'')+'|'+isnull(RegTelNo,'')+'|'+isnull(NowTelNo,'')+'|'+isnull(JobTelNo,'')+'|'+isnull(FormerJobTelNo,'')+'|'+isnull(MobileTel,'') as FRAUD_TEL
                                ,isnull(ComAddress,'')+'|'+isnull(RegAddress,'')+'|'+isnull(NowAddress,'')+'|'+isnull(JobAddress,'') as FRAUD_ADDRESS
                                ,CusName+SUBSTRING(CusId,1,3) + '****' + SUBSTRING(CusId,8,3) as FRAUD_IDNAME
                                FROM         NUMSCustomerInfo AS a
                            where a.ApplNo=@ApplNo and a.ApplNoB=@ApplNoB and ISNULL(a.status,'Y')='Y' ";

            DataTable returnValue = new DataTable();

            try
            {
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@ApplNo", strApplno));
                base.Parameter.Add(new CommandParameter("@ApplNoB", strApplnoB));
                returnValue = base.Search(strSQL);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return returnValue;
        }

        /// <summary>
        /// Get History Data
        /// edit by mel 20130625 
        /// IR-61400
        /// UDWRNUMSHistoryData 要多轉入CloseDate、ApplyDate的資料進去。
        /// Source:UDWRMaster.CloseDate(1對1) --> 用來判斷是否結案
        /// Source:NUMSMaster.ApplyDate(1對1) --> 判斷申請日
        /// IR-61175 
        /// 徵信主畫面(不論是一般/變簽),重新檢核區塊,有擔產品(NUMS、AU)轉入歷史TABLE:UDWRNUMSHistoryData、UDWRAUHistoryData時，
        /// 請新增一欄位ApplTypeCode 存放案件類型(I/J/L)
        /// </summary>
        /// <param name="strApplno"></param>
        /// <param name="strApplnoB"></param>
        /// <param name="guarSeqNo"></param>
        /// <param name="strCustID"></param>
        /// <param name="strNUMSMM"></param>
        /// <returns></returns>
        public DataTable GetHistoryData(string strApplno, string strApplnoB, string guarSeqNo, string strCustID, string strNUMSMM)
        {
            /*
                            ,case when udwrm.AuditResult='A' then '核准' 
                            when  udwrm.AuditResult='D' then '婉拒' 
                            when  udwrm.AuditResult='P' then '補件' 
                            when  udwrm.AuditResult='S' then '中止' 
                            when  udwrm.AuditResult='C' then '撤件' 
                            when  udwrm.AuditResult='B' then '退回' 
                            else udwrm.AuditResult end  as AuditResult         
           
          */
            string strSQL = @"select 
                            a1.* 
                            from 
                            (select
                            @ApplNo as ApplNo 
                            ,@ApplNoB as ApplNoB 
                            ,@CustId as  CustId
                            ,@GuarSeqNo as GuarSeqNo
                            ,numsb.BusName
                            ,numsm.ApplNo as ApplNoHistory
                            ,numsm.ApplNoB as ApplNoBHistory
                            ,numsm.ContractDate 
                            ,udwrm.CurrentApproveDateTime as ApproveDateTime
                            ,flow.StepName as CaseStatus
                            ,udwrm.AuditResult
                            ,udwrm.ApproveAmt
                            ,udwrm.CusGrade
                            ,udwrm.CusGradeMark
                            ,numsc.MobileTel
                            ,numsc.JobUnit
                            ,numsc.JobTelArea+'-'+numsc.JobTelNo+'#'+numsc.JobTelExt as CompanyTel
                            ,numsc.JobZip
                            ,numsc.JobCity
                            ,numsc.JobAddress
                            ,numsc.JobOC
                            ,numsc.ComTelArea+''+numsc.ComTelNo as ComTel
                            ,numsc.ComZip
                            ,numsc.ComCity
                            ,numsc.ComAddress
                            ,numsc.NowTelArea+'-'+ numsc.NowTelNo as NowTel
                            ,numsc.NowZip
                            ,numsc.NowCity
                            ,numsc.NowAddress
                            ,numsc.RegTelArea+''+numsc.RegTelNo as RegTel
                            ,numsc.RegZip
                            ,numsc.RegCity
                            ,numsc.RegAddress
                            ,udwrm.ReasonCode1
                            ,udwrm.ReasonCode2
                            ,udwrm.ReasonCode3
                            ,'GetHistoryData' as CreatedUser
                            ,isnull(udwrm.ReasonCode1,'')+'-'+isnull(udwrm.ReasonCode2,'')+'-'+isnull(udwrm.ReasonCode3,'') as TotReasonCode
                            ,case when (select COUNT(*) from RULEAdrCode where FraudFlag='Y'  and ReasonCode =udwrm.ReasonCode1 ) >0 then 'Y' 
                            when (select COUNT(*) from RULEAdrCode where FraudFlag='Y'  and ReasonCode =udwrm.ReasonCode2 ) >0 then 'Y'
                            when (select COUNT(*) from RULEAdrCode where FraudFlag='Y'  and ReasonCode =udwrm.ReasonCode3 ) >0 then 'Y'
                            else 'N' end  as FraudFileFlag
                            ,numsm.ApplyDate
                            ,udwrm.CloseDate
                            ,numsm.ApplTypeCode 
                            ,numsc.JobCC      
                            ,numsc.LoanRelation                    
                            from NUMSMaster  numsm
                            left outer join NUMSCustomerInfo numsc on numsm.ApplNo=numsc.ApplNo and numsm.ApplNoB=numsc.ApplNoB and numsc.GuaranteeSeqNo=0
                            left outer join NUMSBusMaster numsb on numsm.BusType=numsb.BusType
                            left outer join UDWRMaster udwrm on numsm.ApplNo=udwrm.ApplNo and numsm.ApplNoB=udwrm.ApplNoB
                            left outer join FlowControl flow on numsm.FlowStep=flow.StepId
                            where numsc.Cusid=@Cusid_1 and  numsm.ContractDate>dateadd(MONTH,-1*@NUMSMM,getdate()) 
                            and numsm.ApplNo+numsm.ApplNoB<>@ApplNo_1 
                            
                            ) a1 ";
            //20130621 user要求全部都load  
            //and udwrm.CloseDate is not null 

            DataTable returnValue = new DataTable();

            try
            {
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@ApplNo", strApplno));
                base.Parameter.Add(new CommandParameter("@ApplNoB", strApplnoB));
                base.Parameter.Add(new CommandParameter("@CustId", strCustID));
                base.Parameter.Add(new CommandParameter("@GuarSeqNo", guarSeqNo));
                base.Parameter.Add(new CommandParameter("@Cusid_1", strCustID));
                base.Parameter.Add(new CommandParameter("@ApplNo_1", strApplno + strApplnoB));
                base.Parameter.Add(new CommandParameter("@NUMSMM", strNUMSMM));
                returnValue = base.Search(strSQL);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return returnValue;
        }


        /// <summary>
        /// NUMS DB 變簽件 AU_LHC000T
        /// </summary>
        /// <param name="strApplno"></param>
        /// <param name="strApplnoB"></param>
        /// <param name="guarSeqNo"></param>
        /// <param name="strCustID"></param>
        /// <param name="strAUDISPMM"></param>
        /// <returns></returns>
        /// <remarks>add by 小朱 20150814  </remarks>
        public DataTable GetAUDISPData(string strApplno, string strApplnoB, string guarSeqNo, string strCustID, string strAUDISPMM)
        {
            string strSQL = @"SELECT * FROM (SELECT 
                                @ApplNo [ApplNo]
                                ,@ApplNoB [ApplNoB]
                                ,Left(A.App_No,1) AppType
                                ,A.App_No AppNo
                                ,B.Cus_Id CusID
                                ,B.Cus_Name CusName
                                ,A.App_Date AppDate
                                ,A.App_Time [AppTime]
                                ,'' [AppStatus]
                                ,'' [PromotionUnit]
                                ,Keyin_User  KeyinUser
                                ,'' [ConsignUser]
                                ,'' [SurveyorUser]
                                ,'' [Surveyor2Ok]
                                ,A.Credit_User  CreditUser
                                ,(SELECT top 1 Left(Examine,1) FROM AU_LHC008T  C1 WHERE C1.App_No = A.App_No AND C1.Examine_User = A.Examine_User) Examine1Ok --徵信
                                , A.Approve_User BossUser
                                ,(SELECT top 1 Left(Examine,1) FROM AU_LHC009T  C1 WHERE C1.App_No = A.App_No AND C1.Examine_User = A.Approve_User) Examine2Ok  --主管
                                , A.Close_User  CloseUser
                                ,A.Close_Date [CloseDate]
                                ,null [LoanAmtOff]
                                ,null [LoanRate]
                                ,null [LoanPeriod]
                                ,null [ExtendPeriod]
                                ,'' [PayDebtType]
                                ,(SELECT top 1 C1.Examine_Note FROM AU_LHC008T C1  WHERE C1.App_No = A.App_No AND C1.Examine_User = A.Examine_User) OtherNote --徵信
                                ,'' [APRLOverrideReason]
                                ,'' [OverrideReason]
                                ,'' [OverrideReason2]
                                ,'' [APRLRejectReason1]
                                ,'' [APRLRejectReason2]
                                ,'' [APRLRejectReason3]
                                ,'' [UDWRRejectReason1]
                                ,'' [UDWRRejectReason2]
                                ,'' [UDWRRejectReason3]
                                ,'' [RejectReason1]
                                ,'' [RejectReason2]
                                ,'' [RejectReason3]
                                ,'' [CusClass]
                                ,'' [CusGrade]
                                ,'' [CusGradeMark]
                                ,'' [UseType]
                                ,'' [LoanBusClass]
                                ,'' [UsedClass]
                                ,(Select sum(Income_Tot_amt) Total_Income from AU_LHC012T where App_No = A.App_No and Cus_Id = @CustId and Income_Status = 'Y') YearIncome
                                ,'' [JobUnit]
                                ,'' [JobTel]
                                ,B.TEL_H ComTel
                                ,'' [ComAddress]
                                ,'' [RegAddress]
                                ,'' [FraudFileFlag]
                                ,'J' [ApplTypeCode]
                                ,'' [CreatedUser]
                                ,null [CreatedDate]
                                ,'' [ModifiedUser]
                                ,null [ModifiedDate]
                                ,a.Keyin_User PromotionUser
                               ,(SELECT top 1 C1.Examine_Note FROM AU_LHC009T C1  WHERE C1.App_No = A.App_No AND C1.Examine_User = A.Approve_User) Examine_Note --主管
                                FROM AU_LHC000T A
                                inner JOIN AU_LHC001T B ON A.App_No = B.App_No and B.Cus_Id = @CustId
                                where                                    
                                cast(cast(LEFT(A.App_Date,3) as int)+1911 as varchar) + right(A.App_Date,5) > CONVERT(varchar ,dateadd(MONTH,-1*@AUDISPMM,getdate()) ,112)
                                UNION SELECT 
                                @ApplNo [ApplNo]
                                ,@ApplNoB [ApplNoB]
                                ,Left(A.App_No,1) AppType
                                ,A.App_No AppNo
                                ,B.Cus_Id CusID
                                ,B.Cus_Name CusName
                                ,A.App_Date AppDate
                                ,A.App_Time [AppTime]
                                ,'' [AppStatus]
                                ,'' [PromotionUnit]
                                ,Keyin_User  KeyinUser
                                ,'' [ConsignUser]
                                ,'' [SurveyorUser]
                                ,'' [Surveyor2Ok]
                                ,A.Credit_User  CreditUser
                                ,(SELECT top 1 Left(Examine,1) FROM AU_LHC008T  C1 WHERE C1.App_No = A.App_No AND C1.Examine_User = A.Examine_User) Examine1Ok --徵信
                                , A.Approve_User BossUser
                                ,(SELECT top 1 Left(Examine,1) FROM AU_LHC009T  C1 WHERE C1.App_No = A.App_No AND C1.Examine_User = A.Approve_User) Examine2Ok  --主管                          
                                , A.Close_User  CloseUser
                                ,A.Close_Date [CloseDate]
                                ,null [LoanAmtOff]
                                ,null [LoanRate]
                                ,null [LoanPeriod]
                                ,null [ExtendPeriod]
                                ,'' [PayDebtType]
                                ,(SELECT top 1 C1.Examine_Note FROM AU_LHC008T C1  WHERE C1.App_No = A.App_No AND C1.Examine_User = A.Examine_User) OtherNote --徵信
                                ,'' [APRLOverrideReason]
                                ,'' [OverrideReason]
                                ,'' [OverrideReason2]
                                ,'' [APRLRejectReason1]
                                ,'' [APRLRejectReason2]
                                ,'' [APRLRejectReason3]
                                ,'' [UDWRRejectReason1]
                                ,'' [UDWRRejectReason2]
                                ,'' [UDWRRejectReason3]
                                ,'' [RejectReason1]
                                ,'' [RejectReason2]
                                ,'' [RejectReason3]
                                ,'' [CusClass]
                                ,'' [CusGrade]
                                ,'' [CusGradeMark]
                                ,'' [UseType]
                                ,'' [LoanBusClass]
                                ,'' [UsedClass]
                                ,(Select sum(Income_Tot_amt) Total_Income from AU_LHC012T where App_No = A.App_No and Cus_Id = @CustId and Income_Status = 'Y') YearIncome
                                ,'' [JobUnit]
                                ,'' [JobTel]
                                ,B.TEL_H ComTel
                                ,'' [ComAddress]
                                ,'' [RegAddress]
                                ,'' [FraudFileFlag]
                                ,'J' [ApplTypeCode]
                                ,'' [CreatedUser]
                                ,null [CreatedDate]
                                ,'' [ModifiedUser]
                                ,null [ModifiedDate]
                                ,a.Keyin_User PromotionUser
                                ,(SELECT top 1 C1.Examine_Note FROM AU_LHC009T C1  WHERE C1.App_No = A.App_No AND C1.Examine_User = A.Approve_User) Examine_Note --主管
                                FROM AU_LHC000T A
                                inner JOIN AU_LHC002T B ON A.App_No = B.App_No and B.Cus_Id = @CustId
                                WHERE
                                cast(cast(LEFT(A.App_Date,3) as int)+1911 as varchar) + right(A.App_Date,5) > CONVERT(varchar ,dateadd(MONTH,-1*@AUDISPMM,getdate()) ,112)
                                ) A ORDER BY AppNo
                                ";

            DataTable returnValue = new DataTable();

            try
            {
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@ApplNo", strApplno));
                base.Parameter.Add(new CommandParameter("@ApplNoB", strApplnoB));
                base.Parameter.Add(new CommandParameter("@CustId", strCustID));
                base.Parameter.Add(new CommandParameter("@AUDISPMM", strAUDISPMM));
                returnValue = base.Search(strSQL);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return returnValue;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="strApplno"></param>
        /// <param name="strApplnoB"></param>
        /// <param name="strCustID"></param>
        /// <param name="strDISPMM"></param>
        /// <returns></returns>
        /// <remarks>add by mel 20131002 IR-61614 </remarks>
        public DataTable GetHistoryDataDISPMaster(string strApplno, string strApplnoB, string strCustID, string strDISPMM)
        {

            string strSQL = @"select 
                                @ApplNo ApplNo
                                ,@ApplNoB ApplNoB
                                ,t2.ApplNo OriApplNo
                                ,t2.ApplNoB	OriApplNoB
                                ,t2.CusId
                                ,t1.CusName
                                ,t2.ApplDT ApplyDate
                                ,t2.BusClassType
                                ,t2.DISPType 
                                ,Case when t2.DISPType='U' then t4.TopCreditLevel when t2.DISPType='P' then t2.FinalApprLevel else '' end FinalApprLevel
                                ,t2.ApplBUID
                                ,t2.ApplUserId
                                ,Case when t2.DISPType='U' then t3.LatestApproveApplEmpNo else '' end ReviewUserId
                                ,Case when t2.DISPType='U' then t3.LatestApproveApplDateTime else '' end ReviewDatetime
                                ,Case when t2.DISPType='U' then t3.LatestApproveSupervisorEmpNo  when t2.DISPType='P' then t2.FinalApprUserId else '' end FinalApprUserId
                                ,Case when t2.DISPType='U' then t3.LatestApproveSupervisorDateTime  when t2.DISPType='P' then t2.FinalApprDT else '' end FinalApprDT
                                ,Case when t2.DISPType='U' then t3.FinalApproveAuditResult  when t2.DISPType='P' then t2.FinalApprResult else '' end FinalApprResult
                                ,'GetHistoryDataDISPMaster' as CreatedUser
                                ,'GetHistoryDataDISPMaster' as ModifiedUser
                                from 
                                (
                                select *
                                from DISPMaster  where ApplNo+ApplNoB <> @ApplNo_1 and CusId=@CusId
                                and  ApplDT >dateadd(MONTH,-1*@DISPMM,getdate()) 
                                ) t2
                                left outer join NUMSCustomerInfo t1 on t2.ApplNo=t1.ApplNo and t2.ApplNoB=t1.ApplNoB and t2.CusId=t1.CusId
                                left outer join 
                                (
	                                select a.ApplNo ,a.ApplNoB,a.LatestApproveApplAuditResult
	                                ,a.LatestApproveApplEmpNo
	                                ,a.LatestApproveApplDateTime
	                                ,a.LatestApproveSupervisorEmpNo
	                                ,a.LatestApproveSupervisorDateTime
	                                ,case when a.isClose is null then a.LatestApproveApplAuditResult else '' end  FinalApproveAuditResult
	                                from  UDWRDerivedData a
                                ) t3 on t2.ApplNo=t3.ApplNo and t2.ApplNoB=t3.ApplNoB 
                                left outer join UDWRMaster t4 on t2.ApplNo=t4.ApplNo and t2.ApplNoB=t4.ApplNoB ";

            DataTable returnValue = new DataTable();

            try
            {
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@ApplNo", strApplno));
                base.Parameter.Add(new CommandParameter("@ApplNoB", strApplnoB));
                base.Parameter.Add(new CommandParameter("@CusId", strCustID));
                base.Parameter.Add(new CommandParameter("@ApplNo_1", strApplno + strApplnoB));
                base.Parameter.Add(new CommandParameter("@DISPMM", strDISPMM));
                returnValue = base.Search(strSQL);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return returnValue;
        }

        public DataTable GetHistoryDataDISPDetail(string strApplno, string strApplnoB, string strCustID, string strDISPMM)
        {

            string strSQL = @"select 
                                @ApplNo ApplNo
                                ,@ApplNoB ApplNoB
                                ,t1.ApplNo OriApplNo
                                ,t1.ApplNoB	OriApplNoB
                                ,t1.CusId
                                ,t1.DISPType
                                ,Case t1.DISPType when 'U'  then t3.ContentSeq else null end ContentSeq
                                ,Case t1.DISPType when 'U'  then t3.ApplUDWRType when 'P' then ''   else null end ApplType
                                ,Case t1.DISPType when 'U'  then t3.ApplUDWRType when 'P' then t2.ApplSubType  else null end ApplSubType
                                ,Case t1.DISPType when 'U'  then '' when 'P' then t2.ProdCodeN  else null end ProdCodeN
                                ,'GetHistoryDataDISPDetail' as CreatedUser
                                ,'GetHistoryDataDISPDetail' as ModifiedUser
                                from 
                                (
                                select *
                                from DISPMaster  where ApplNo+ApplNoB <> @ApplNo_1 and CusId=@CusId 
                                and  ApplDT >dateadd(MONTH,-1*@DISPMM,getdate()) 
                                ) t1
                                left outer join 
                                (
	                                select ApplNo,ApplNoB 
	                                ,ProdCodeN,ApplSubType
	                                from DISPApplProdContent
	                                unpivot
	                                (
	                                ApplSubType for ProdCodeN in ([DISPCode1],[DISPCode2],[DISPCode3],[DISPCode4],[DISPCode5],[DISPCode6])
	                                )
	                                as tempt      
                                )  t2 on t1.ApplNo=t2.ApplNo and t1.ApplNoB=t2.ApplNoB 
                                left outer join DISPApplUDWRContent t3 on t1.ApplNo=t3.ApplNo and t1.ApplNoB=t3.ApplNoB ";

            DataTable returnValue = new DataTable();

            try
            {
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@ApplNo", strApplno));
                base.Parameter.Add(new CommandParameter("@ApplNoB", strApplnoB));
                base.Parameter.Add(new CommandParameter("@CusId", strCustID));
                base.Parameter.Add(new CommandParameter("@ApplNo_1", strApplno + strApplnoB));
                base.Parameter.Add(new CommandParameter("@DISPMM", strDISPMM));
                returnValue = base.Search(strSQL);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return returnValue;
        }

        /// <summary>
        /// edit by mel 20130625
        /// IR-61175 
        /// 徵信主畫面(不論是一般/變簽),重新檢核區塊,有擔產品(NUMS、AU)轉入歷史TABLE:UDWRNUMSHistoryData、UDWRAUHistoryData時，
        /// 請新增一欄位ApplTypeCode 存放案件類型(I/J/L)
        /// </summary>
        /// <param name="strApplno"></param>
        /// <param name="strApplnoB"></param>
        /// <param name="guarSeqNo"></param>
        /// <param name="strCustID"></param>
        /// <param name="strNUMSMM"></param>
        /// <returns></returns>
        /// <remarks>edit by mel 20131108 IR-63246 增加AU_LCE001T.Promotion_User</remarks>
        public DataTable GetHistoryDataFromAU(string strApplno, string strApplnoB, string guarSeqNo, string strCustID, string strNUMSMM)
        {
            /*
                代碼	原因分類	原因說明	強制註記	轉婉拒檔	使用狀態
                39	資料作假	偽造申辦基本資料	否	是	不使用
                41	資料作假	偽造繳息記錄	否	是	不使用
                42	資料作假	詐貸、人頭戶	否	是	不使用
                70	鑑價	MA-偽造買賣契約書	是	是	使用中
                DO	風險	DO-意圖或曾經詐欺	是	是	使用中
                FH	風險	FH-人頭戶	是	是	使用中
                FI	風險	FI-偽造證明文件(收入證明…等)	是	是	使用中
                MA	風險	MA-偽造買賣契約書	是	是	使用中
           */
            string strSQL = @" select 
							@ApplNo as ApplNo 
							,@ApplNoB as ApplNoB 
							, a1.* ,
							case when 
							CHARINDEX('39', a1.TotReasonCode)>0 or 
							CHARINDEX('41', a1.TotReasonCode)>0 or   
							CHARINDEX('42', a1.TotReasonCode)>0 or 
							CHARINDEX('70', a1.TotReasonCode)>0 or 
							CHARINDEX('DO', a1.TotReasonCode)>0 or 
							CHARINDEX('FH', a1.TotReasonCode)>0 or 
							CHARINDEX('FI', a1.TotReasonCode)>0 or 
							CHARINDEX('MA', a1.TotReasonCode)>0 
							then 'Y' else 'N' end as FraudFileFlag ,
                            'GetHistoryDataFromAU' CreatedUser
							 from 
							 (
							 SELECT 
									AU_LCE000T.APP_TYPE AppType,
									AU_LCE000T.APP_NO AppNo,
									AU_LCE002T.CUS_ID CusID,
									AU_LCE002T.CUS_NAME CusName,
									AU_LCE000T.APP_DATE AppDate,
									AU_LCE000T.APP_TIME AppTime,
									AU_LCE000T.APP_STATUS AppStatus,
									AU_LCE001T.PROMOTION_UNIT PromotionUnit,
                                    AU_LCE001T.PROMOTION_USER PromotionUser,
									AU_LCE000T.KEYIN_USER KeyinUser,
									AU_LCE000T.CONSIGN_USER ConsignUser,
									AU_LCE000T.SURVEYOR_USER SurveyorUser,
									AU_LCE000T.SURVEYOR2_OK Surveyor2Ok,
									AU_LCE000T.CREDIT_USER CreditUser,
									AU_LCE000T.EXAMINE1_OK Examine1Ok,
									AU_LCE000T.BOSS_USER BossUser,
									AU_LCE000T.EXAMINE2_OK Examine2Ok,
									AU_LCE000T.CLOSE_USER CloseUser,
									AU_LCE000T.CLOSE_DATE CloseDate,
									AU_LCE006T.LOAN_AMTOFF LoanAmtOff,
									AU_LCE006T.LOAN_RATE LoanRate,
									AU_LCE006T.LOAN_PERIOD LoanPeriod,
									AU_LCE006T.EXTEND_PERIOD ExtendPeriod,
									AU_LCE006T.PAYDEBT_TYPE PayDebtType,
									AU_LCE005T.OTHER_NOTE OtherNote,
									AU_LCE007T.OVERRIDE_REASON  APRLOverrideReason,
									AU_LCE005T.OVERRIDE_REASON OverrideReason,
									AU_LCE005T.OVERRIDE_REASON2 OverrideReason2,
									AU_LCE007T.REJECT_REASON1 APRLRejectReason1,
									AU_LCE007T.REJECT_REASON2 APRLRejectReason2,
									AU_LCE007T.REJECT_REASON3 APRLRejectReason3,
									AU_LCE005T.REJECT_REASON1 UDWRRejectReason1,
									AU_LCE005T.REJECT_REASON2 UDWRRejectReason2,
									AU_LCE005T.REJECT_REASON3 UDWRRejectReason3,
									AU_LCE006T.REJECT_REASON1 RejectReason1,
									AU_LCE006T.REJECT_REASON2 RejectReason2,
									AU_LCE006T.REJECT_REASON3 RejectReason3,
									AU_LCE004T.CUS_CLASS CusClass,
									AU_LCE004T.CUS_GRADE CusGrade,
									AU_LCE004T.CUS_GRADE_MARK CusGradeMark,
									AU_LCE001T.USE_TYPE UseType ,
									AU_LCE004T.LOAN_BUS_CLASS LoanBusClass,
									AU_LCE004T.USED_CLASS UsedClass,
									AU_LCE002T.YEAR_INCOME YearIncome,
									AU_LCE003T.JOB_UNIT JobUnit,
									AU_LCE003T.JOB_TELAREA+'-'+AU_LCE003T.JOB_TELNO  JobTel,
									AU_LCE002T.COM_TELAREA+'-'+AU_LCE002T.COM_TELNO  ComTel,
									AU_LCE002T.COM_ADDRESS ComAddress,
									AU_LCE002T.REG_ADDRESS RegAddress,
									isnull(AU_LCE007T.REJECT_REASON1,'')+'-'+isnull(AU_LCE007T.REJECT_REASON2,'')+'-'+isnull(AU_LCE007T.REJECT_REASON3,'')+'-'+
									isnull(AU_LCE005T.REJECT_REASON1,'')+'-'+isnull(AU_LCE005T.REJECT_REASON2,'')+'-'+isnull(AU_LCE005T.REJECT_REASON3,'')+'-'+  
									isnull(AU_LCE006T.REJECT_REASON1,'')+'-'+isnull(AU_LCE006T.REJECT_REASON2,'')+'-'+isnull(AU_LCE006T.REJECT_REASON3,'') As TotReasonCode 
                                    ,SUBSTRING(AU_LCE002T.App_No ,2,1)  ApplTypeCode 
                                FROM 
                                    AU_LCE002T
                                LEFT JOIN 
                                    AU_LCE000T
                                ON 
                                    AU_LCE002T.App_No = AU_LCE000T.App_No
                                LEFT JOIN 
                                    AU_LCE001T
                                ON 
                                    AU_LCE002T.App_No = AU_LCE001T.App_No
                                LEFT JOIN 
                                    AU_LCE003T
                                ON 
                                    AU_LCE002T.App_No = AU_LCE003T.App_No
                                    AND AU_LCE003T.Job_User='C'
                                LEFT JOIN 
                                    AU_LCE004T
                                ON 
                                    AU_LCE002T.App_No = AU_LCE004T.App_No
                                LEFT JOIN 
                                    AU_LCE005T
                                ON 
                                    AU_LCE002T.App_No = AU_LCE005T.App_No
                                LEFT JOIN 
                                    (select * from AU_LCE006T where App_No+Boss_Auth in (select app_no+MAX(Boss_Auth)  Boss_Auth  from AU_LCE006T group by app_no))  AU_LCE006T
                                ON 
                                    AU_LCE002T.APP_NO = AU_LCE006T.APP_NO
                                LEFT JOIN 
                                    (select *  from AU_LCE007T  where app_no+Reject_Reason1 in (select app_no +MAX(Reject_Reason1) from AU_LCE007T group by app_no )) AU_LCE007T
                                ON 
                                    AU_LCE002T.APP_NO = AU_LCE007T.APP_NO
                                WHERE
                                    AU_LCE002T.Cus_Id =@CustId
                                    --and CONVERT(datetime ,AU_LCE000T.APP_DATE ,111 ) >dateadd(MONTH,-1*@NUMSMM,getdate())
									and cast(cast(LEFT(AU_LCE000T.App_Date,3) as int)+1911 as varchar) + right(AU_LCE000T.App_Date,5) 
									>CONVERT(varchar ,dateadd(MONTH,-1*@NUMSMM,getdate()) ,112)

                        ) a1  ";

            DataTable returnValue = new DataTable();

            try
            {
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@ApplNo", strApplno));
                base.Parameter.Add(new CommandParameter("@ApplNoB", strApplnoB));
                base.Parameter.Add(new CommandParameter("@CustId", strCustID));
                base.Parameter.Add(new CommandParameter("@GuarSeqNo", guarSeqNo));
                base.Parameter.Add(new CommandParameter("@NUMSMM", strNUMSMM));
                returnValue = base.Search(strSQL);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return returnValue;
        }


        /// <summary>
        /// edit by mel 20130801  IR-61973 
        ///1. 不動產資料建物標示部-------
        ///A. 建物區域+座落地址(路/街) 比對 Deviation資料庫中的行政區+地址(路/街)
        ///B. 建物區域+段小段+建號 比對 Deviation資料庫中的行政區+段小段+建號
        ///C. 建物區域+ 鑑價作業的擔保品案名 比對Deviation資料庫中的行政區+社區名稱
        ///三者任一筆有比對到時，就顯示Deviation資料在畫面中
        /// </summary>
        /// <param name="strApplno"></param>
        /// <param name="strApplnoB"></param>
        /// <returns></returns>
        public DataTable GetAprlDeviationMatch(string strApplno, string strApplnoB)
        {
            string strSQL = @"SELECT b.AreaNo,b.GuaraNo ,a.*   FROM APRLDeviation a 
                              left outer join 
                              (select b.AreaNo ,b.GuaraNo,b.BulNo,b.Address1,b.Address2,b.Address3,c.GaranName 
	                            from APRLGuaranteeMain a 
	                            left outer join APRLGuaranteeBuilding b on a.ApplNo=b.ApplNo and a.ApplNoB=b.ApplNoB and a.GuaraNo=b.GuaraNo 
	                            left outer join APRLDistinguishMain c on a.ApplNo=c.ApplNo and a.ApplNoB=c.ApplNoB and a.GuaraNo=c.GuaraNo 
	                            where a.ApplNo =@ApplNo and a.ApplNoB=@ApplNoB  and isnull(a.MbNo,'')='') b   on 1=1 
                              where 
                               a.Approve = '2' 
                               and 
                               (
                                  a.Building in( b.BulNo)
                                  or (a.Community in (b.GaranName))
                                  or (a.Address1+a.Address2+a.Address3 in (b.Address1+b.Address2+b.Address3) )
                               ) ";

            strSQL = @"SELECT a1.AreaNo,a1.GuaraNo,b1.*
                        FROM APRLDeviation b1
                        inner join 
                        (
                        select 
                        a.ApplNo,a.ApplNoB,a.GuaraNo,
                        b.Sector ,b.AreaNo ,b.BulNo,b.Address1,b.Address2,b.Address3,c.GaranName 
	                        from APRLGuaranteeMain a 
	                        left outer join APRLGuaranteeBuilding b on a.ApplNo=b.ApplNo and a.ApplNoB=b.ApplNoB and a.GuaraNo=b.GuaraNo 
	                        left outer join APRLDistinguishMain c on a.ApplNo=c.ApplNo and a.ApplNoB=c.ApplNoB and a.GuaraNo=c.GuaraNo 
	                        where 
                            a.ApplNo =@ApplNo and a.ApplNoB=@ApplNoB  
	                        and isnull(a.MbNo,'')=''
	                        and isnull(b.AreaNo,'')<>''
                        ) a1   on b1.Zip=a1.AreaNo
                        where 
                        b1.Approve = '2' 
                        and isnull(a1.ApplNo,'')<>'' 
                        and 
                        (
	                        a1.Address1+a1.Address2+a1.Address3 = b1.Address1+b1.Address2+b1.Address3
	                        or 
	                        (a1.Sector = b1.Sector and a1.BulNo=b1.Building  and ISNULL(a1.Sector,'')<>'' and ISNULL(a1.BulNo,'')<>'' )
	                        or 
	                        (a1.GaranName =b1.Community and ISNULL(a1.GaranName,'')<>'' )
                        ) ";

            DataTable returnValue = new DataTable();

            try
            {
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@ApplNo", strApplno));
                base.Parameter.Add(new CommandParameter("@ApplNoB", strApplnoB));

                returnValue = base.Search(strSQL);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return returnValue;
        }

        /// <summary>
        /// 取得擔保品主檔資料
        /// </summary>
        /// <param name="strApplno"></param>
        /// <param name="strApplnoB"></param>
        /// <returns></returns>
        public DataTable GetGuaranteeList(string strApplno, string strApplnoB)
        {
            string sql = @"SELECT *  FROM   APRLGuaranteeMain                            
                                WHERE   ApplNo  = @ApplNo  AND ApplNoB  = @ApplNoB  and isnull(MbNo,'')=''";

            DataTable returnValue = new DataTable();

            try
            {
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@ApplNo", strApplno));
                base.Parameter.Add(new CommandParameter("@ApplNoB", strApplnoB));

                returnValue = base.Search(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return returnValue;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="strApplno"></param>
        /// <param name="strApplnoB"></param>
        /// <returns></returns>
        public string GetNUMSMasterUDWRAssignEmpID(string strApplno, string strApplnoB)
        {
            string empid = "";
            string sql = @"select isnull(a.UDWRAssignEmpID,'') UDWRAssignEmpID 
                            from NUMSMaster a                    
                                WHERE   a.ApplNo  = @ApplNo  AND a.ApplNoB  = @ApplNoB ";

            DataTable returnValue = new DataTable();

            try
            {
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@ApplNo", strApplno));
                base.Parameter.Add(new CommandParameter("@ApplNoB", strApplnoB));

                returnValue = base.Search(sql);
                if (returnValue.Rows.Count > 0)
                    empid = returnValue.Rows[0]["UDWRAssignEmpID"].ToString();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return empid;

        }

        public DataTable GetNUMSMasterUDWRAssignEmpData(string strApplno, string strApplnoB)
        {

            string sql = @"select isnull(a.UDWRAssignEmpID,'') UDWRAssignEmpID 
                            ,TempUDWRDispatchOperatorEmpNo,TempUDWRDispatchOperatorDateTime,TempUDWRExpectedFinishedDateTime 
                            ,TempAPRLEmpNo,TemAPRLSendDate,TemAPRLPreFinishDate   
                            from NUMSMaster a                    
                                WHERE   a.ApplNo  = @ApplNo  AND a.ApplNoB  = @ApplNoB ";

            DataTable returnValue = new DataTable();

            try
            {
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@ApplNo", strApplno));
                base.Parameter.Add(new CommandParameter("@ApplNoB", strApplnoB));

                returnValue = base.Search(sql);
                //if (returnValue.Rows.Count > 0)
                //    empid = returnValue.Rows[0]["UDWRAssignEmpID"].ToString();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return returnValue;

        }


        public DataTable GetNUMSMasterUDWRAssignEmpDataByL(string strApplno, string strApplnoB)
        {

            string sql = @"select isnull(a.UDWRAssignEmpID,'') UDWRAssignEmpID 
                            ,FlowOwner 
                            from NUMSMaster a                    
                                WHERE   a.ApplNo  = @ApplNo  AND a.ApplNoB  = @ApplNoB ";

            DataTable returnValue = new DataTable();

            try
            {
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@ApplNo", strApplno));
                base.Parameter.Add(new CommandParameter("@ApplNoB", strApplnoB));

                returnValue = base.Search(sql);
                //if (returnValue.Rows.Count > 0)
                //    empid = returnValue.Rows[0]["UDWRAssignEmpID"].ToString();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return returnValue;

        }

        public DataTable GetNUMSVerifyAndDISP(string strApplno, string strApplnoB)
        {
            //edit by mel 20131014 IR-63049
            /*
            string sql = @"select  a. applno ,a.applnob ,a.ApplTypeCode 
                                ,b.OldApplNo,b.OldApplNoB,c.OriApplNo,c.OriApplNoB ,d.FinalApplEmpNo,e.EmpID
                                from NUMSMaster a 
                                left outer join NUMSApplyVerify b on a.ApplNo=b.ApplNo and a.ApplNoB=b.ApplNoB 
                                left outer join DISPMaster c on a.ApplNo=c.ApplNo and a.ApplNoB=c.ApplNoB 
                                left outer join UDWRMaster d on a.ApplNo=d.ApplNo and a.ApplNoB=d.ApplNoB 
                                left outer join 
                                (select EmpID  from NUMSEmployeeToRole where RoleID in ('NUMSD0002')) e on d.FinalApplEmpNo=e.EmpID
                                where isnull(e.EmpID ,'')<>''                           
                                and   a.ApplNo  = @ApplNo  AND a.ApplNoB  = @ApplNoB ";
            */
            string sql = @"select  a. applno ,a.applnob ,a.ApplTypeCode 
                        ,b.OldApplNo ,b.OldApplNoB 
                        ,d.LatestApproveApplEmpNo,e.EmpID
                        from NUMSMaster a 
                        left outer join NUMSApplyVerify b on a.ApplNo=b.ApplNo and a.ApplNoB=b.ApplNoB 
                        left outer join UDWRDerivedData d on b.OldApplNo=d.ApplNo and b.OldApplNoB=d.ApplNoB 
                        left outer join 
                        (select EmpID  from NUMSEmployeeToRole where RoleID in (select CodeNo from PARMCode where CodeType='UDWRDispatchRole')) e on d.LatestApproveApplEmpNo=e.EmpID
                        where isnull(e.EmpID ,'')<>''                           
                        and   a.ApplNo  = @ApplNo  AND a.ApplNoB  = @ApplNoB ";

            DataTable returnValue = new DataTable();

            try
            {
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@ApplNo", strApplno));
                base.Parameter.Add(new CommandParameter("@ApplNoB", strApplnoB));

                returnValue = base.Search(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return returnValue;

        }

        //edit by mel 20131014 IR-63049
        public DataTable GetVerifyForAprl(string strApplno, string strApplnoB)
        {

            //            string sql = @"select  a. applno ,a.applnob ,a.ApplTypeCode 
            //                        ,b.OldApplNo ,b.OldApplNoB 
            //                        ,d.Owner,e.EmpID
            //                        from NUMSMaster a 
            //                        left outer join NUMSApplyVerify b on a.ApplNo=b.ApplNo and a.ApplNoB=b.ApplNoB 
            //                        left outer join APRLMaster d on b.OldApplNo=d.ApplNo and b.OldApplNoB=d.ApplNoB 
            //                        left outer join 
            //                        (select EmpID  from NUMSEmployeeToRole where RoleID in ('NUMSD0003')) e on d.Owner=e.EmpID
            //                        where isnull(e.EmpID ,'')<>''                            
            //                        and   a.ApplNo  = @ApplNo  AND a.ApplNoB  = @ApplNoB ";
            string sql = @"select  a. applno ,a.applnob ,a.ApplTypeCode 
                        ,b.OldApplNo ,b.OldApplNoB 
                        ,d.LatestApproveApplEmpNo,e.EmpID
                        ,a.FlowOwner
                        from NUMSMaster a 
                        left outer join NUMSApplyVerify b on a.ApplNo=b.ApplNo and a.ApplNoB=b.ApplNoB 
                        left outer join UDWRDerivedData d on b.OldApplNo=d.ApplNo and b.OldApplNoB=d.ApplNoB 
                        left outer join APRLMaster f on b.OldApplNo=f.ApplNo and b.OldApplNoB=f.ApplNoB 
                        left outer join 
                        (select EmpID  from NUMSEmployeeToRole where RoleID in (select CodeNo from PARMCode where CodeType='APRLDispatchRole')) e on f.Owner=e.EmpID
                        where isnull(e.EmpID ,'')<>''                             
                        and   a.ApplNo  = @ApplNo  AND a.ApplNoB  = @ApplNoB ";

            DataTable returnValue = new DataTable();

            try
            {
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@ApplNo", strApplno));
                base.Parameter.Add(new CommandParameter("@ApplNoB", strApplnoB));

                returnValue = base.Search(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return returnValue;

        }


        /// <summary>
        /// Update JcicProces Status
        /// </summary>
        /// <param name="newid"></param>
        /// <param name="status"></param>
        public void UpdateJcicProcesStatus(string newid, string status)
        {


            string updatesql = "UPDATE JCICProcess   SET Status =@Status ,ModifiedUser=@ModifiedUser ,ModifiedDate=@ModifiedDate  where  NewId = @NewId";
            IDbConnection dbConnection = base.OpenConnection();
            IDbTransaction dbTransaction = null;
            using (dbConnection)
            {
                try
                {
                    dbTransaction = dbConnection.BeginTransaction();
                    base.Parameter.Clear();
                    base.Parameter.Add(new CommandParameter("@Status", status));
                    base.Parameter.Add(new CommandParameter("@NewId", newid));
                    base.Parameter.Add(new CommandParameter("@ModifiedUser", "UpdateJcicProcesStatus"));
                    base.Parameter.Add(new CommandParameter("@ModifiedDate", System.DateTime.Now));
                    base.ExecuteNonQuery(updatesql, dbTransaction, false);

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
        /// Update SendQ RetryTimes
        /// </summary>
        /// <param name="strNewId"></param>
        /// <param name="intSendCount"></param>
        public void UpdateSendQRetryTimes(string strNewId, int intSendCount)
        {

            string updatesql = "UPDATE JCICSend   SET SendTimes =@SendTimes  where  NewId = @NewId";
            IDbConnection dbConnection = base.OpenConnection();
            IDbTransaction dbTransaction = null;
            using (dbConnection)
            {
                try
                {
                    dbTransaction = dbConnection.BeginTransaction();
                    base.Parameter.Clear();
                    base.Parameter.Add(new CommandParameter("@SendTimes", intSendCount));
                    base.Parameter.Add(new CommandParameter("@NewId", strNewId));
                    base.ExecuteNonQuery(updatesql, dbTransaction, false);

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
        /// Update JcicSend IsSend
        /// </summary>
        /// <param name="dtJcicCases"></param>
        public void UpdateJcicSendIsSend(DataTable dtJcicCases)
        {

            IDbConnection dbConnection = base.OpenConnection();
            IDbTransaction dbTransaction = null;
            using (dbConnection)
            {
                try
                {
                    dbTransaction = dbConnection.BeginTransaction();
                    foreach (DataRow dr in dtJcicCases.Rows)
                    {
                        string updatesql = @"UPDATE JCICSend   SET IsSend =@IsSend,SendTimes=@SendTimes ,SendDate=@SendDate ,SendTime=@SendTime 
                                            ,ModifiedUser=@ModifiedUser ,ModifiedDate=@ModifiedDate 
                                            where  NewId = @NewId";

                        base.Parameter.Clear();
                        base.Parameter.Add(new CommandParameter("@IsSend", dr["IsSend"].ToString()));
                        base.Parameter.Add(new CommandParameter("@SendTimes", dr["SendTimes"].ToString()));
                        base.Parameter.Add(new CommandParameter("@SendDate", dr["SendDate"].ToString()));
                        base.Parameter.Add(new CommandParameter("@SendTime", dr["SendTime"].ToString()));
                        base.Parameter.Add(new CommandParameter("@ModifiedUser", "UpdateJcicSendIsSend"));
                        base.Parameter.Add(new CommandParameter("@ModifiedDate", System.DateTime.Now));
                        base.Parameter.Add(new CommandParameter("@NewId", dr["NewId"].ToString()));
                        base.ExecuteNonQuery(updatesql, dbTransaction, false);
                    }
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
        /// Update JcicProces
        /// </summary>
        /// <param name="_procdr"></param>
        public void UpdateJcicProces(DataRow _procdr)
        {

            string updatesql = @"UPDATE JCICProcess   
            SET 
                Module=@Module,
                ApplNo=@ApplNo,
                ApplNoB=@ApplNoB,
                CusId=@CusId,
                QryDate=@QryDate,
                QryTime=@QryTime ,
                StepId=@StepId,
                Status =@Status,
                FlowType=@FlowType,
                CreatedUser=@CreatedUser,
                CreatedDate=@CreatedDate,
                ModifiedUser=@ModifiedUser,
                ModifiedDate=@ModifiedDate  where  NewId = @NewId";
            IDbConnection dbConnection = base.OpenConnection();
            IDbTransaction dbTransaction = null;
            using (dbConnection)
            {
                try
                {
                    dbTransaction = dbConnection.BeginTransaction();
                    base.Parameter.Clear();
                    base.Parameter.Add(new CommandParameter("@NewId", _procdr["NewId"]));
                    base.Parameter.Add(new CommandParameter("@Module", _procdr["Module"]));
                    base.Parameter.Add(new CommandParameter("@ApplNo", _procdr["ApplNo"]));
                    base.Parameter.Add(new CommandParameter("@ApplNoB", _procdr["ApplNoB"]));
                    base.Parameter.Add(new CommandParameter("@CusId", _procdr["CusId"]));
                    base.Parameter.Add(new CommandParameter("@QryDate", _procdr["QryDate"]));
                    base.Parameter.Add(new CommandParameter("@QryTime", _procdr["QryTime"]));
                    base.Parameter.Add(new CommandParameter("@StepId", _procdr["StepId"]));
                    base.Parameter.Add(new CommandParameter("@Status", _procdr["Status"]));
                    base.Parameter.Add(new CommandParameter("@FlowType", _procdr["FlowType"]));
                    base.Parameter.Add(new CommandParameter("@CreatedUser", _procdr["CreatedUser"]));
                    base.Parameter.Add(new CommandParameter("@CreatedDate", _procdr["CreatedDate"]));
                    base.Parameter.Add(new CommandParameter("@ModifiedUser", _procdr["ModifiedUser"]));
                    base.Parameter.Add(new CommandParameter("@ModifiedDate", _procdr["ModifiedDate"]));
                    base.ExecuteNonQuery(updatesql, dbTransaction, false);

                    dbTransaction.Commit();

                }
                catch (Exception ex)
                {
                    dbTransaction.Rollback();
                    throw ex;
                }
            }
        }

        //edit by mel 20131213 IR-63339 
        public void UpdateUDWRQueryDetailInquiryErrors(string applno, string applnob, string cusid, string msg, string reason)
        {
            //若UDWRQueryDetail 無資料則新增,若有資料則update
            DataTable dt = new DataTable();
            string sql = "";

            msg = msg.Trim();
            //if (msg.Length > 200)
            //{
            //    msg = msg.Substring(0, 200);
            //}

            msg = String.Format("{0:s}", System.DateTime.Now) + ":" + msg;

            base.Parameter.Clear();
            dt = OpenDataTable("UDWRQueryDetail", applno, applnob, " CusId='" + cusid + "' ");

            base.Parameter.Clear();
            base.Parameter.Add(new CommandParameter("@ApplNo", applno));
            base.Parameter.Add(new CommandParameter("@ApplNoB", applnob));
            base.Parameter.Add(new CommandParameter("@CusId", cusid));
            base.Parameter.Add(new CommandParameter("@InquiryErrors", msg));
            base.Parameter.Add(new CommandParameter("@InquiryReason", reason));

            if (dt.Rows.Count == 0)
            {
                sql = @" INSERT INTO UDWRQueryDetail (ApplNo,ApplNoB,CusId,InquiryReason,InquiryErrors,CreatedDate,CreatedUser,ModifiedDate) 
                        VALUES(@ApplNo,@ApplNoB,@CusId,@InquiryReason,SUBSTRING(@InquiryErrors,1,295),GETDATE(),'UpdateUDWRQueryDetail',GETDATE())";
            }
            else
            {
                sql = @" update UDWRQueryDetail set InquiryErrors=SUBSTRING(Isnull(InquiryErrors,'') + @InquiryErrors,1,295) ,ModifiedDate =GETDATE(),ModifiedUser='UpdateUDWRQueryDetail'
                        where 
                        ApplNo=@ApplNo and ApplNoB=@ApplNoB  and CusId=@CusId ";
            }


            IDbConnection dbConnection = base.OpenConnection();
            IDbTransaction dbTransaction = null;
            using (dbConnection)
            {
                try
                {
                    dbTransaction = dbConnection.BeginTransaction();


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

        //edit by mel 20131213 IR-63339 
        public void UpdateUDWRQueryDetailInquiryErrorsToNull(string applno, string applnob, string cusid)
        {
            //若UDWRQueryDetail 無資料則新增,若有資料則update
            DataTable dt = new DataTable();
            string sql = "";

            base.Parameter.Clear();
            dt = OpenDataTable("UDWRQueryDetail", applno, applnob, " CusId='" + cusid + "' ");

            base.Parameter.Clear();
            base.Parameter.Add(new CommandParameter("@ApplNo", applno));
            base.Parameter.Add(new CommandParameter("@ApplNoB", applnob));
            base.Parameter.Add(new CommandParameter("@CusId", cusid));


            sql = @" update UDWRQueryDetail set InquiryErrors ='' ,ModifiedDate =GETDATE(),ModifiedUser='InquiryErrorsToNull'
                    where 
                    ApplNo=@ApplNo and ApplNoB=@ApplNoB  and CusId=@CusId ";


            IDbConnection dbConnection = base.OpenConnection();
            IDbTransaction dbTransaction = null;
            using (dbConnection)
            {
                try
                {
                    dbTransaction = dbConnection.BeginTransaction();


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

        //edit by mel 20131213 IR-63339 
        public bool ProcessJcicValue(string applno, string applnob)
        {
            bool result = true;
            DataTable dt = new DataTable();
            string sql = "";

            //若該案件項下的querydetail有"[聯徵中心]原業務查詢但無報送資料"
            dt = OpenDataTable("UDWRQueryDetail", applno, applnob, "InquiryErrors like '%原業務查詢但無報送資料%' ");
            if (dt.Rows.Count > 0)
            {
                result = false;
                sql = @" update UDWRQueryDetail set InquiryErrors= SUBSTRING( InquiryErrors + ',無JCIC加工資料',1,295)   ,ModifiedDate =GETDATE(),ModifiedUser='ProcessJcicValue'
                        where 
                        ApplNo=@ApplNo and ApplNoB=@ApplNoB";
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@ApplNo", applno));
                base.Parameter.Add(new CommandParameter("@ApplNoB", applnob));
                IDbConnection dbConnection = base.OpenConnection();
                IDbTransaction dbTransaction = null;
                using (dbConnection)
                {
                    try
                    {
                        dbTransaction = dbConnection.BeginTransaction();

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
            else
            {
                result = true;
            }



            return result;


            //
        }


        /// <summary>
        /// Update DataTable
        /// </summary>
        /// <param name="rtable"></param>
        public void UpdateDataTable(DataTable rtable)
        {

            // 連接數據庫
            IDbConnection dbConnection = base.OpenConnection();
            IDbTransaction dbTransaction = null;
            string sql = "";

            if (rtable.TableName.Trim() == "" || rtable.TableName.ToUpper() == "TABLE")
                throw new Exception("DataTable has no TableName!!");

            // DB連接
            using (dbConnection)
            {
                try
                {
                    // 開始事務
                    dbTransaction = dbConnection.BeginTransaction();

                    foreach (DataRow dr in rtable.Rows)
                    {
                        CommandParameterCollection cmdparm = new CommandParameterCollection();
                        CreateUpdateSql(rtable.TableName, dr, ref sql, ref  cmdparm);
                        base.Parameter.Clear();
                        foreach (CommandParameter cmp in cmdparm)
                        {
                            base.Parameter.Add(cmp);
                        }

                        base.ExecuteNonQuery(sql, dbTransaction, false);
                    }

                    //System.Threading.Thread.Sleep(600000);

                    dbTransaction.Commit();


                }
                catch (Exception ex)
                {
                    dbTransaction.Rollback();

                    throw ex;
                }
            }
        }

        //edit by mel 20130819 add transaction
        public void UpdateDataTable(DataTable rtable, Dictionary<string, string> keyfield)
        {
            UpdateDataTable(rtable, keyfield, null);

            /*
            // 連接數據庫
            IDbConnection dbConnection = base.OpenConnection();
            IDbTransaction dbTransaction = null;
            string sql = "";

            if (rtable.TableName.Trim() == "" || rtable.TableName.ToUpper() == "TABLE")
                throw new Exception("DataTable has no TableName!!");

            // DB連接
            using (dbConnection)
            {
                try
                {
                    // 開始事務
                    dbTransaction = dbConnection.BeginTransaction();

                    foreach (DataRow dr in rtable.Rows)
                    {
                        CommandParameterCollection cmdparm = new CommandParameterCollection();
                        CreateUpdateSql(rtable.TableName,keyfield, dr, ref sql, ref  cmdparm);
                        base.Parameter.Clear();
                        foreach (CommandParameter cmp in cmdparm)
                        {
                            base.Parameter.Add(cmp);
                        }

                        base.ExecuteNonQuery(sql, dbTransaction, false);
                    }


                    dbTransaction.Commit();


                }
                catch (Exception ex)
                {
                    dbTransaction.Rollback();

                    throw ex;
                }
            }
             * 
             * */
        }
        //edit by mel 20130819  	IR-62275 add IDbTransaction
        public void UpdateDataTable(DataTable rtable, Dictionary<string, string> keyfield, IDbTransaction dbTransaction = null)
        {

            if (dbTransaction != null)
            {
                UpdateDataTableWithTransaction(rtable, keyfield, dbTransaction);
                return;
            }

            // 連接數據庫
            IDbConnection dbConnection = base.OpenConnection();
            bool innertransaction = false;

            if (dbTransaction == null)
                innertransaction = true;

            string sql = "";

            if (rtable.TableName.Trim() == "" || rtable.TableName.ToUpper() == "TABLE")
                throw new Exception("DataTable has no TableName!!");

            // DB連接
            using (dbConnection)
            {
                try
                {
                    // 開始事務
                    if (innertransaction == true)
                        dbTransaction = dbConnection.BeginTransaction();

                    foreach (DataRow dr in rtable.Rows)
                    {
                        CommandParameterCollection cmdparm = new CommandParameterCollection();
                        CreateUpdateSql(rtable.TableName, keyfield, dr, ref sql, ref  cmdparm);
                        base.Parameter.Clear();
                        foreach (CommandParameter cmp in cmdparm)
                        {
                            base.Parameter.Add(cmp);
                        }

                        base.ExecuteNonQuery(sql, dbTransaction, false);
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

        public void UpdateDataTableWithTransaction(DataTable rtable, Dictionary<string, string> keyfield, IDbTransaction dbTransaction = null)
        {

            // 連接數據庫
            IDbConnection dbConnection = base.OpenConnection();
            bool innertransaction = false;

            if (dbTransaction == null)
                innertransaction = true;

            string sql = "";

            if (rtable.TableName.Trim() == "" || rtable.TableName.ToUpper() == "TABLE")
                throw new Exception("DataTable has no TableName!!");

            // DB連接

            try
            {
                // 開始事務
                if (innertransaction == true)
                    dbTransaction = dbConnection.BeginTransaction();

                foreach (DataRow dr in rtable.Rows)
                {
                    CommandParameterCollection cmdparm = new CommandParameterCollection();
                    CreateUpdateSql(rtable.TableName, keyfield, dr, ref sql, ref  cmdparm);
                    base.Parameter.Clear();
                    foreach (CommandParameter cmp in cmdparm)
                    {
                        base.Parameter.Add(cmp);
                    }

                    base.ExecuteNonQuery(sql, dbTransaction, false);
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
        /// add by mel 20130827
        /// </summary>
        /// <param name="row"></param>
        /// <param name="keyfield"></param>
        /// <param name="dbTransaction"></param>
        public void UpdateDataRow(String tablename, DataRow row, Dictionary<string, string> keyfield, IDbTransaction dbTransaction = null)
        {

            DataTable rtable = new DataTable();
            rtable = row.Table.Clone();
            rtable.TableName = tablename;
            rtable.ImportRow(row);
            UpdateDataTable(rtable, keyfield, dbTransaction);

        }

        /// <summary>
        /// add by mel 20130827
        /// </summary>
        /// <param name="tablename"></param>
        /// <param name="row"></param>
        /// <param name="keyfield"></param>
        /// <returns></returns>
        public DataTable GetDataSpecify(String tablename, DataRow row, Dictionary<string, string> keyfield)
        {
            DataTable returnvalue = new DataTable();
            IDbConnection dbConnection = base.OpenConnection();
            string sql = "";

            if (tablename.Trim() == "" || tablename.ToUpper() == "TABLE")
                throw new Exception("DataTable has no TableName!!");

            // DB連接
            using (dbConnection)
            {
                try
                {
                    CommandParameterCollection cmdparm = new CommandParameterCollection();
                    CreateSelectSql(tablename, keyfield, row, ref sql, ref  cmdparm);
                    base.Parameter.Clear();
                    foreach (CommandParameter cmp in cmdparm)
                    {
                        base.Parameter.Add(cmp);
                    }

                    returnvalue = base.Search(sql);
                }
                catch (Exception ex)
                {

                    throw ex;
                }
            }

            return returnvalue;
        }

        /// <summary>
        /// Update DataTable
        /// </summary>
        /// <param name="rtable"></param>
        public void UpdateDataTable(DataTable rtable, string excludefield)
        {

            // 連接數據庫
            IDbConnection dbConnection = base.OpenConnection();
            IDbTransaction dbTransaction = null;
            string sql = "";

            if (rtable.TableName.Trim() == "" || rtable.TableName.ToUpper() == "TABLE")
                throw new Exception("DataTable has no TableName!!");

            //移除不要更新的欄位
            string[] _refield = excludefield.Split(new string[] { "," }, StringSplitOptions.None); ;
            foreach (string _field in _refield)
            {
                rtable.Columns.Remove(_field);
            }

            // DB連接
            using (dbConnection)
            {
                try
                {
                    // 開始事務
                    dbTransaction = dbConnection.BeginTransaction();

                    foreach (DataRow dr in rtable.Rows)
                    {
                        CommandParameterCollection cmdparm = new CommandParameterCollection();
                        CreateUpdateSql(rtable.TableName, dr, ref sql, ref  cmdparm);
                        base.Parameter.Clear();
                        foreach (CommandParameter cmp in cmdparm)
                        {
                            base.Parameter.Add(cmp);
                        }

                        base.ExecuteNonQuery(sql, dbTransaction, false);
                    }


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
                    CreateUpdateSql(tablename, dr, ref sql, ref  cmdparm);
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
        /// 將主表更新成
        /// </summary>
        /// <param name="Applno"></param>
        /// <param name="ApplnoB"></param>
        /// <param name="BOPSStatus"></param>
        /// <returns></returns>
        public bool UpdateBOPSMaster(string Applno, string ApplnoB, string BOPSStatus)
        {
            base.Parameter.Clear();

            string sql = @"
                            UPDATE  dbo.BOPSMaster
                            SET     
                                BOPSStatus = @BOPSStatus,
                                ModifiedDate = GETDATE()
                            WHERE   
                            ApplNo = @ApplNo
                            AND ApplNoB = @ApplNoB ";

            base.Parameter.Add(new CommandParameter("@BOPSStatus", BOPSStatus));
            base.Parameter.Add(new CommandParameter("@ApplNo", Applno));
            base.Parameter.Add(new CommandParameter("@ApplNoB", ApplnoB));

            return base.ExecuteNonQuery(sql) >= 0 ? true : false;
        }

        /// <summary>
        /// Update UDWRDetailByCusId For ProxyIncome
        /// </summary>
        /// <param name="strApplno"></param>
        /// <param name="strApplnoB"></param>
        /// <param name="strCustID"></param>
        /// <param name="income"></param>
        public void UpdateUDWRDetailByCusIdForProxyIncome(string strApplno, string strApplnoB, string strCustID, long income)
        {
            string updatesql = @"UPDATE UDWRDetailByCusId Set ProxyIncome=@ProxyIncome ,ModifiedUser=@ModifiedUser , ModifiedDate=getdate()    
                                  where  ApplNo=@ApplNo and ApplNoB=@ApplNoB and CusId=@CusId";
            IDbConnection dbConnection = base.OpenConnection();
            IDbTransaction dbTransaction = null;
            using (dbConnection)
            {
                try
                {
                    dbTransaction = dbConnection.BeginTransaction();
                    base.Parameter.Clear();
                    base.Parameter.Add(new CommandParameter("@ProxyIncome", income));
                    base.Parameter.Add(new CommandParameter("@ApplNo", strApplno));
                    base.Parameter.Add(new CommandParameter("@ApplNoB", strApplnoB));
                    base.Parameter.Add(new CommandParameter("@CusId", strCustID));
                    base.Parameter.Add(new CommandParameter("@ModifiedUser", "UpdateUDWRDetailByCusIdForProxyIncome"));
                    base.ExecuteNonQuery(updatesql, dbTransaction, false);

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
        /// 派件之後更新UDWRMaster.GetApplEmpNo
        /// </summary>
        /// <param name="strApplno"></param>
        /// <param name="strApplnoB"></param>
        /// <param name="dispempid"></param>
        public void UpdateUDWRMasterGetApplEmpNo(string strApplno, string strApplnoB, string dispempid)
        {
            //edit by mel 20130322 
            //1.IR-59653 update 這個欄位ExpectedFinishedDateTime(UDWRMaster) 
            //2.同時要判斷DispatchOperatorEmpNo無值才更新
            //edit by mel 20130724 IR-61838
            //2310派件完後, 請清掉
            //[TempUDWRDispatchOperatorEmpNo]
            //[TempUDWRDispatchOperatorDateTime]
            //[TempUDWRExpectedFinishedDateTime]
            //這三個欄位
            string updatesql = @"UPDATE UDWRMaster Set 
                                    DispatchOperatorEmpNo=@DispatchOperatorEmpNo,
                                    DispatchOperatorDateTime=getdate() ,
                                    ModifiedUser=@ModifiedUser , 
                                    ModifiedDate=getdate(),
                                    CurrentCreditLevel=(select Top 1 ToAuthLevel from NUMSEmployeeToAuthLevel where EmpID=@DispatchOperatorEmpNo),
                                    ExpectedFinishedDateTime =
                                      (
                                          select top 1 b.workdate from 
                                          ( SELECT ROW_NUMBER() OVER( ORDER BY a.date) as rowno, a.date as workdate  from  PARMWorkingDay a where a.date >getdate() and a.Flag='1' ) b 
                                          where  b.rowno = 
                                              ( select top 1 case isnumeric(a.CodeNo)  when 1  then CAST(a.CodeNo as int) else 0 end  as basedate from PARMCode a 
                                                where CodeType = (Case when Substring(UDWRMaster.ApplNo,10,1) ='J' then 'DISPDispatchDay' else  'UDWRDispatchDay' end)
                                              )
                                     )
                                  where  ApplNo=@ApplNo and ApplNoB=@ApplNoB and  ISNULL(DispatchOperatorEmpNo,'')='' ;";
            updatesql += @" UPDATE NUMSMaster Set 
                            TempUDWRDispatchOperatorEmpNo =null,TempUDWRDispatchOperatorDateTime=null,TempUDWRExpectedFinishedDateTime = null
                            where  ApplNo=@ApplNo and ApplNoB=@ApplNoB  ;";
            IDbConnection dbConnection = base.OpenConnection();
            IDbTransaction dbTransaction = null;
            using (dbConnection)
            {
                try
                {
                    dbTransaction = dbConnection.BeginTransaction();
                    base.Parameter.Clear();
                    base.Parameter.Add(new CommandParameter("@DispatchOperatorEmpNo", dispempid));
                    base.Parameter.Add(new CommandParameter("@ApplNo", strApplno));
                    base.Parameter.Add(new CommandParameter("@ApplNoB", strApplnoB));
                    base.Parameter.Add(new CommandParameter("@ModifiedUser", "UpdateUDWRMasterGetApplEmpNo"));
                    base.ExecuteNonQuery(updatesql, dbTransaction, false);

                    dbTransaction.Commit();

                }
                catch (Exception ex)
                {
                    dbTransaction.Rollback();
                    throw ex;
                }
            }
        }

        public void UpdateUDWRMasterGetApplEmpNo(string strApplno, string strApplnoB, string dispempid, DateTime dipatchtime, DateTime exceptedtime)
        {
            //edit by mel 20130322 
            //1.IR-59653 update 這個欄位ExpectedFinishedDateTime(UDWRMaster) 
            //2.同時要判斷DispatchOperatorEmpNo無值才更新

            string updatesql = @"UPDATE UDWRMaster Set 
                                    DispatchOperatorEmpNo=@DispatchOperatorEmpNo,
                                    DispatchOperatorDateTime=@DispatchOperatorDateTime ,
                                    ModifiedUser=@ModifiedUser , 
                                    ModifiedDate=getdate(),
                                    CurrentCreditLevel=(select Top 1 ToAuthLevel from NUMSEmployeeToAuthLevel where EmpID=@DispatchOperatorEmpNo),
                                    ExpectedFinishedDateTime =@ExpectedFinishedDateTime
                                  where  ApplNo=@ApplNo and ApplNoB=@ApplNoB and  ISNULL(DispatchOperatorEmpNo,'')='' ";

            updatesql += @" UPDATE NUMSMaster Set 
                            TempUDWRDispatchOperatorEmpNo =null,TempUDWRDispatchOperatorDateTime=null,TempUDWRExpectedFinishedDateTime = null
                            where  ApplNo=@ApplNo and ApplNoB=@ApplNoB  ;";
            IDbConnection dbConnection = base.OpenConnection();
            IDbTransaction dbTransaction = null;
            using (dbConnection)
            {
                try
                {
                    dbTransaction = dbConnection.BeginTransaction();
                    base.Parameter.Clear();
                    base.Parameter.Add(new CommandParameter("@DispatchOperatorEmpNo", dispempid));
                    base.Parameter.Add(new CommandParameter("@DispatchOperatorDateTime", dipatchtime));
                    base.Parameter.Add(new CommandParameter("@ExpectedFinishedDateTime", exceptedtime));
                    base.Parameter.Add(new CommandParameter("@ApplNo", strApplno));
                    base.Parameter.Add(new CommandParameter("@ApplNoB", strApplnoB));
                    base.Parameter.Add(new CommandParameter("@ModifiedUser", "UpdateUDWRMasterGetApplEmpNo"));
                    base.ExecuteNonQuery(updatesql, dbTransaction, false);

                    dbTransaction.Commit();

                }
                catch (Exception ex)
                {
                    dbTransaction.Rollback();
                    throw ex;
                }
            }
        }

        public void UpdateUDWRMasterGetApplEmpNoDISP(string strApplno, string strApplnoB)
        {
            //edit by mel 20131104 

            string updatesql = @"UPDATE UDWRMaster Set 
                                ModifiedUser=@ModifiedUser , 
                                ModifiedDate=getdate(),
                                CurrentCreditLevel=(select Top 1 ToAuthLevel from NUMSEmployeeToAuthLevel where EmpID=UDWRMaster.DispatchOperatorEmpNo),
                                ExpectedFinishedDateTime =
                                  (
                                      select top 1 b.workdate from 
                                      ( SELECT ROW_NUMBER() OVER( ORDER BY a.date) as rowno, a.date as workdate  from  PARMWorkingDay a where a.date >UDWRMaster.DispatchOperatorDateTime and a.Flag='1' ) b 
                                      where  b.rowno = 
                                          ( select top 1 case isnumeric(a.CodeNo)  when 1  then CAST(a.CodeNo as int) else 0 end  as basedate from PARMCode a 
                                            where CodeType = (Case when Substring(UDWRMaster.ApplNo,10,1) ='J' then 'DISPDispatchDay' else  'UDWRDispatchDay' end)
                                          )
                                 )
                              where  ApplNo=@ApplNo and ApplNoB=@ApplNoB and  ExpectedFinishedDateTime is null ;";

            IDbConnection dbConnection = base.OpenConnection();
            IDbTransaction dbTransaction = null;
            using (dbConnection)
            {
                try
                {
                    dbTransaction = dbConnection.BeginTransaction();
                    base.Parameter.Clear();
                    base.Parameter.Add(new CommandParameter("@ApplNo", strApplno));
                    base.Parameter.Add(new CommandParameter("@ApplNoB", strApplnoB));
                    base.Parameter.Add(new CommandParameter("@ModifiedUser", "UpdateUDWRMasterGetApplEmpNoDISP"));
                    base.ExecuteNonQuery(updatesql, dbTransaction, false);

                    dbTransaction.Commit();

                }
                catch (Exception ex)
                {
                    dbTransaction.Rollback();
                    throw ex;
                }
            }
        }

        public void UpdateNUMSMasterUDWRTempInfo(string strApplno, string strApplnoB, string dispempid)
        {
            //edit by mel 20130322 
            //1.IR-59653 update 這個欄位ExpectedFinishedDateTime(UDWRMaster) 
            //2.同時要判斷DispatchOperatorEmpNo無值才更新


            string updatesql = "";

            updatesql = @"UPDATE NUMSMaster Set 
                                    TempUDWRDispatchOperatorEmpNo=@DispatchOperatorEmpNo,
                                    TempUDWRDispatchOperatorDateTime=getdate() ,
                                    ModifiedUser=@ModifiedUser , 
                                    ModifiedDate=getdate(),
                                    TempUDWRExpectedFinishedDateTime =
                                    (
	                                    select top 1 b.workdate from 
	                                    ( SELECT ROW_NUMBER() OVER( ORDER BY a.date) as rowno, a.date as workdate  from  PARMWorkingDay a where a.date >getdate() and a.Flag='1' ) b 
	                                    where  b.rowno = 
	                                    (select top 1 case isnumeric(a.CodeNo)  when 1  then CAST(a.CodeNo as int) else 0 end  as basedate from PARMCode a where CodeType in ('UDWRDispatchDay'))
                                    )
                                  where  ApplNo=@ApplNo and ApplNoB=@ApplNoB  ";

            IDbConnection dbConnection = base.OpenConnection();
            IDbTransaction dbTransaction = null;
            using (dbConnection)
            {
                try
                {
                    dbTransaction = dbConnection.BeginTransaction();
                    base.Parameter.Clear();
                    base.Parameter.Add(new CommandParameter("@DispatchOperatorEmpNo", dispempid));
                    base.Parameter.Add(new CommandParameter("@ApplNo", strApplno));
                    base.Parameter.Add(new CommandParameter("@ApplNoB", strApplnoB));
                    base.Parameter.Add(new CommandParameter("@ModifiedUser", "UpdateNUMSMasterUDWRTempInfo"));
                    base.ExecuteNonQuery(updatesql, dbTransaction, false);

                    dbTransaction.Commit();

                }
                catch (Exception ex)
                {
                    dbTransaction.Rollback();
                    throw ex;
                }
            }
        }

        public void UpdateNUMSMasterAPRLTempInfo(string strApplno, string strApplnoB, string dispempid)
        {
            //edit by mel 20130322 
            //1.IR-59653 update 這個欄位ExpectedFinishedDateTime(UDWRMaster) 
            //2.同時要判斷DispatchOperatorEmpNo無值才更新

            string updatesql = @"UPDATE NUMSMaster Set 
                                    TempAPRLEmpNo=@TempAPRLEmpNo,
                                    TemAPRLSendDate=getdate() ,
                                    ModifiedUser=@ModifiedUser , 
                                    ModifiedDate=getdate(),
                                    TemAPRLPreFinishDate =
                                    (
	                                    select top 1 b.workdate from 
	                                    ( SELECT ROW_NUMBER() OVER( ORDER BY a.date) as rowno, a.date as workdate  from  PARMWorkingDay a where a.date >getdate() and a.Flag='1' ) b 
	                                    where  b.rowno = 
	                                    (select top 1 case isnumeric(a.CodeNo)  when 1  then CAST(a.CodeNo as int) else 0 end  as basedate from PARMCode a where CodeType in ('UDWRDispatchDay'))
                                    )
                                  where  ApplNo=@ApplNo and ApplNoB=@ApplNoB  ";
            IDbConnection dbConnection = base.OpenConnection();
            IDbTransaction dbTransaction = null;
            using (dbConnection)
            {
                try
                {
                    dbTransaction = dbConnection.BeginTransaction();
                    base.Parameter.Clear();
                    base.Parameter.Add(new CommandParameter("@TempAPRLEmpNo", dispempid));
                    base.Parameter.Add(new CommandParameter("@ApplNo", strApplno));
                    base.Parameter.Add(new CommandParameter("@ApplNoB", strApplnoB));
                    base.Parameter.Add(new CommandParameter("@ModifiedUser", "UpdateNUMSMasterAPRLTempInfo"));
                    base.ExecuteNonQuery(updatesql, dbTransaction, false);

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
        /// 更新BUMasterID.IsDispatch為'N'
        /// </summary>
        /// <param name="BUMasterID"></param>
        /// <param name="IsDispatch"></param>
        public void UpdateNUMSBUToEmployeeToAll(string BUMasterID, string buid, string IsDispatch)
        {

            string updatesql = @"UPDATE NUMSBUToEmployee Set IsDispatch=@IsDispatch
                                  where  BUMasterID=@BUMasterID and  BUID=@BUID ";
            IDbConnection dbConnection = base.OpenConnection();
            IDbTransaction dbTransaction = null;
            using (dbConnection)
            {
                try
                {
                    dbTransaction = dbConnection.BeginTransaction();
                    base.Parameter.Clear();
                    base.Parameter.Add(new CommandParameter("@BUMasterID", BUMasterID));
                    base.Parameter.Add(new CommandParameter("@IsDispatch", IsDispatch));
                    base.Parameter.Add(new CommandParameter("@BUID", buid));
                    base.ExecuteNonQuery(updatesql, dbTransaction, false);

                    dbTransaction.Commit();

                }
                catch (Exception ex)
                {
                    dbTransaction.Rollback();
                    throw ex;
                }
            }
        }

        public void UpdateAllDistinguishRule(string area, string IsDispatch)
        {

            string updatesql = @"UPDATE PARMDistinguishRule Set IsDispatch=@IsDispatch
                                  where  AreaCode=@AreaCode ";
            IDbConnection dbConnection = base.OpenConnection();
            IDbTransaction dbTransaction = null;
            using (dbConnection)
            {
                try
                {
                    dbTransaction = dbConnection.BeginTransaction();
                    base.Parameter.Clear();
                    base.Parameter.Add(new CommandParameter("@IsDispatch", IsDispatch));
                    base.Parameter.Add(new CommandParameter("@AreaCode", area));
                    base.ExecuteNonQuery(updatesql, dbTransaction, false);

                    dbTransaction.Commit();

                }
                catch (Exception ex)
                {
                    dbTransaction.Rollback();
                    throw ex;
                }
            }
        }

        public void UpdateAPRLDistinguishRule(string area, string empid, string isdispatch, double dispatchcnt)
        {
            //edit by mel 20130322 
            //1.IR-59653 update 這個欄位ExpectedFinishedDateTime(UDWRMaster) 
            //2.同時要判斷DispatchOperatorEmpNo無值才更新

            string updatesql = @"UPDATE PARMDistinguishRule Set  IsDispatch=@IsDispatch ,DispatchCnt=@DispatchCnt
                                  where  AreaCode=@AreaCode and EmpNo=@EmpNo ";
            IDbConnection dbConnection = base.OpenConnection();
            IDbTransaction dbTransaction = null;
            using (dbConnection)
            {
                try
                {
                    dbTransaction = dbConnection.BeginTransaction();
                    base.Parameter.Clear();
                    base.Parameter.Add(new CommandParameter("@IsDispatch", isdispatch));
                    base.Parameter.Add(new CommandParameter("@DispatchCnt", dispatchcnt));
                    base.Parameter.Add(new CommandParameter("@AreaCode", area));
                    base.Parameter.Add(new CommandParameter("@EmpNo", empid));
                    base.ExecuteNonQuery(updatesql, dbTransaction, false);

                    dbTransaction.Commit();

                }
                catch (Exception ex)
                {
                    dbTransaction.Rollback();
                    throw ex;
                }
            }
        }

        public void UpdateNUMSBUToEmployeeCnt(string uid, string isdispatch, double dispatchcnt)
        {
            //edit by mel 20130322 
            //1.IR-59653 update 這個欄位ExpectedFinishedDateTime(UDWRMaster) 
            //2.同時要判斷DispatchOperatorEmpNo無值才更新

            string updatesql = @"UPDATE NUMSBUToEmployee Set IsDispatch=@IsDispatch ,DispatchCnt=@DispatchCnt
                                  where  UID=@UID ";
            IDbConnection dbConnection = base.OpenConnection();
            IDbTransaction dbTransaction = null;
            using (dbConnection)
            {
                try
                {
                    dbTransaction = dbConnection.BeginTransaction();
                    base.Parameter.Clear();
                    base.Parameter.Add(new CommandParameter("@UID", uid));
                    base.Parameter.Add(new CommandParameter("@IsDispatch", isdispatch));
                    base.Parameter.Add(new CommandParameter("@DispatchCnt", dispatchcnt));
                    base.ExecuteNonQuery(updatesql, dbTransaction, false);

                    dbTransaction.Commit();

                }
                catch (Exception ex)
                {
                    dbTransaction.Rollback();
                    throw ex;
                }
            }
        }

        #endregion


        public bool CheckMortgage(string strApplno, string strApplnoB)
        {
            string strSQL = @"select COUNT(applno) tot   from NUMSMaster where ApplNo =@ApplNo and ApplNoB=@ApplNoB  and BusClassType='E'";
            bool result = false;
            DataTable returnValue = new DataTable();
            try
            {
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@ApplNo", strApplno));
                base.Parameter.Add(new CommandParameter("@ApplNoB", strApplnoB));
                returnValue = base.Search(strSQL);
                if (Convert.ToInt16(returnValue.Rows[0]["tot"]) > 0)
                {
                    result = true;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return result;
        }

        public DataTable GetPSRNProductlist(string strApplno, string strApplnoB)
        {
            string strSQL = @"Select distinct a.NewID, a.ApplNo ,a.ApplNoB
                                ,a.SeqNo ,a.LoanExtend ,FLOOR(a.LoanAmt) Amt  
                                ,isnull(b.OverdrawnFlag,'0') OverdrawnFlag
                                ,isnull(a.FirstRate,0) FirstRate ,isnull(a.SecondRate,0) SecondRate ,isnull(a.ThirdRate,0) ThirdRate
                                ,isnull(a.AmortizarionPeriod,0) AmortizarionPeriod
                                ,a.LoanProdCode ,a.LoanPeriod
                                From PSRNProduct a left outer join PARMLoanProd b on a.LoanProdCode=b.LoanProdCode 
                                where a.ApplNo=@ApplNo and a.ApplNoB=@ApplNoB
                                order by SeqNo ";

            DataTable returnValue = new DataTable();
            try
            {
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@ApplNo", strApplno));
                base.Parameter.Add(new CommandParameter("@ApplNoB", strApplnoB));
                returnValue = base.Search(strSQL);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return returnValue;

        }

        /// <summary>
        /// 20140611 將舊案的額度, 餘額, 部份收回額度的無條件進位到萬元取消, 直接用原值
        /// </summary>
        /// <param name="strApplno"></param>
        /// <param name="strApplnoB"></param>
        /// <returns></returns>
        public DataTable GetUDWRProductlist(string strApplno, string strApplnoB)
        {
            //edit by mel 20130816 判斷NUMSCustomerInfo status
            //            string strSQL = @"select * from 
            //                                (
            //                                Select 'NEW' as Div, a.ApplNo ,a.ApplNoB
            //                                ,Cast(a.SeqNo as nvarchar(5)) CNo , isnull(a.Together,'') Together ,FLOOR(a.ApproveLoanAmt) Amt  
            //                                ,isnull((select top 1 OverdrawnFlag from  PARMLoanProd b1 where b1.LoanProdCode =a.LoanProdCode and b1.BusClassType=SUBSTRING(a.ApplNo,9,1)),'0') OverdrawnFlag
            //                                ,isnull(a.FirstRate,0) FirstRate ,isnull(a.SecondRate,0) SecondRate ,isnull(a.ThirdRate,0) ThirdRate
            //                                ,isnull(a.ApproveFirstRate,0) ApproveFirstRate ,isnull(a.ApproveSecondRate,0) ApproveSecondRate ,isnull(a.ApproveThirdRate,0) ApproveThirdRate
            //                                ,isnull(a.ApproveLoanExtend,0) ApproveLoanExtend 
            //                                ,isnull(a.FinalCalApproveAmt,0) FinalCalApproveAmt ,isnull(a.FinalCalRate,0) FinalCalRate,isnull(a.FinalCalPeriods,0) FinalCalPeriods
            //                                ,isnull(a.FinalCalAllowancePeriods,0) FinalCalAllowancePeriods ,isnull(a.FinalCalSubjectRate,0) FinalCalSubjectRate 
            //                                ,isnull(a.FinalCalSpendingByYear,0) FinalCalSpendingByYear ,isnull(a.AmortizarionPeriod,0) AmortizarionPeriod
            //                                ,isnull(a.ApproveAmortizarionPeriod,0) ApproveAmortizarionPeriod
            //                                ,isnull(c.TogetherAmt1,0) TogetherAmt1 ,isnull(c.TogetherAmt2,0) TogetherAmt2 ,isnull(c.ManageAmt1,0) ManageAmt1 
            //                                From UDWRProduct a 
            //                                left outer join UDWRMaster c on a.ApplNo=c.ApplNo and a.ApplNoB =c.ApplNoB
            //                                where a.ApplNo=@ApplNo and a.ApplNoB=@ApplNoB and a.AuditResult='A'
            //                                union 
            //                                select 'OLD' Div ,a.ApplNo,a.ApplNoB , a.NFOSeq CNo,
            //                                case when a.Together1='Y' then '1' when a.Together2='Y' then '2' else '' end Together 
            //                                 ,ceiling(cast(a.CurrBal as numeric)/cast(10000 as numeric))*10000  Amt ,'0' OverdrawnFlag  
            //                                 ,0 FirstRate,0 SecondRate ,0 ThirdRate
            //                                 ,0 ApproveFirstRate,0 ApproveSecondRate ,0 ApproveThirdRate
            //                                 ,0  FinalCalApproveAmt
            //                                 ,0  FinalCalRate,0  FinalCalPeriods ,0 ApproveLoanExtend , 0  FinalCalAllowancePeriods
            //                                 ,0  FinalCalSubjectRate,0  FinalCalSpendingByYear
            //                                 ,0 AmortizarionPeriod
            //                                 ,0 ApproveAmortizarionPeriod
            //                                 ,isnull(c.TogetherAmt1,0) TogetherAmt1 ,isnull(c.TogetherAmt2,0) TogetherAmt2 ,isnull(c.ManageAmt1,0) ManageAmt1 
            //                                from 
            //                                (                                  
            //                                  select * from UDWRNewForOld where ApplNo =@ApplNo  and ApplNoB=@ApplNoB 
            //                                and CusID in (select cusid from NUMSCustomerInfo where ApplNo =@ApplNo and ApplNoB=@ApplNoB and ISNULL(Status,'Y') ='Y'  )
            //                                )  a 
            //                                left outer join UDWRMaster c on a.ApplNo=c.ApplNo and a.ApplNoB =c.ApplNoB
            //                                where a.ApplNo=@ApplNo and a.ApplNoB=@ApplNoB  and (a.Together1 ='Y' or a.Together2='Y')
            //                                ) l
            //                                order by l.CNo ";

            string strSQL = @"select * from 
                                (
                                Select 'NEW' as Div, a.ApplNo ,a.ApplNoB
                                ,Cast(a.SeqNo as nvarchar(5)) CNo , isnull(a.Together,'') Together ,FLOOR(a.ApproveLoanAmt) Amt  
                                ,isnull((select top 1 OverdrawnFlag from  PARMLoanProd b1 where b1.LoanProdCode =a.ApproveLoanProdCode and b1.BusClassType=SUBSTRING(a.ApplNo,9,1)),'0') OverdrawnFlag
                                ,isnull(a.FirstRate,0) FirstRate ,isnull(a.SecondRate,0) SecondRate ,isnull(a.ThirdRate,0) ThirdRate
                                ,isnull(a.ApproveFirstRate,0) ApproveFirstRate ,isnull(a.ApproveSecondRate,0) ApproveSecondRate ,isnull(a.ApproveThirdRate,0) ApproveThirdRate
                                ,isnull(a.ApproveLoanExtend,0) ApproveLoanExtend 
                                ,isnull(a.FinalCalApproveAmt,0) FinalCalApproveAmt ,isnull(a.FinalCalRate,0) FinalCalRate,isnull(a.FinalCalPeriods,0) FinalCalPeriods
                                ,isnull(a.FinalCalAllowancePeriods,0) FinalCalAllowancePeriods ,isnull(a.FinalCalSubjectRate,0) FinalCalSubjectRate 
                                ,isnull(a.FinalCalSpendingByYear,0) FinalCalSpendingByYear ,isnull(a.AmortizarionPeriod,0) AmortizarionPeriod
                                ,isnull(a.ApproveAmortizarionPeriod,0) ApproveAmortizarionPeriod
                                ,isnull(c.TogetherAmt1,0) TogetherAmt1 ,isnull(c.TogetherAmt2,0) TogetherAmt2 ,isnull(c.ManageAmt1,0) ManageAmt1 
                                From UDWRProduct a 
                                left outer join UDWRMaster c on a.ApplNo=c.ApplNo and a.ApplNoB =c.ApplNoB
                                where a.ApplNo=@ApplNo and a.ApplNoB=@ApplNoB and a.AuditResult='A'
                                union 
                                select 'OLD' Div ,a.ApplNo,a.ApplNoB , a.NFOSeq CNo,
                                case when a.Together1='Y' then '1' when a.Together2='Y' then '2' else '' end Together 
                 ,case when a.RecPartial='Y' 
                 --then ISNULL(ceiling(cast(a.RecParAmt as numeric)/cast(10000 as numeric))*10000 ,0) 
                 then isnull(a.RecParAmt,0)
                 when b1.FinancialOverdraft='Y' 
                 --then  ISNULL(ceiling(cast(a.AppyAmt as numeric)/cast(10000 as numeric))*10000 ,0) 
                 then isnull(a.AppyAmt,0)
                 when b1.FinancialOverdraft='N' 
                 --then ISNULL(ceiling(cast(a.CurrBal as numeric)/cast(10000 as numeric))*10000 ,0) 
                 then isnull(a.CurrBal,0)
                 else 0 end Amt
                 ,case when b1.FinancialOverdraft ='Y' then '1' else '0' end  OverdrawnFlag
                                 ,0 FirstRate,0 SecondRate ,0 ThirdRate
                                 ,0 ApproveFirstRate,0 ApproveSecondRate ,0 ApproveThirdRate
                                 ,0  FinalCalApproveAmt
                                 ,0  FinalCalRate,0  FinalCalPeriods ,0 ApproveLoanExtend , 0  FinalCalAllowancePeriods
                                 ,0  FinalCalSubjectRate,0  FinalCalSpendingByYear
                                 ,0 AmortizarionPeriod
                                 ,0 ApproveAmortizarionPeriod
                                 ,isnull(c.TogetherAmt1,0) TogetherAmt1 ,isnull(c.TogetherAmt2,0) TogetherAmt2 ,isnull(c.ManageAmt1,0) ManageAmt1 
                                from 
                                (                                  
                                  select * from UDWRNewForOld where ApplNo =@ApplNo  and ApplNoB=@ApplNoB 
                                and CusID in (select cusid from NUMSCustomerInfo where ApplNo =@ApplNo and ApplNoB=@ApplNoB and ISNULL(Status,'Y') ='Y'  )
                                )  a 
                                left outer join UDWRMaster c on a.ApplNo=c.ApplNo and a.ApplNoB =c.ApplNoB
left outer join PARMBankAcctTypeCode b1 on  b1.AcctTypeCode =a.WXAcctType and b1.IntCategoryCode =a.WXIntCat 
                                where a.ApplNo=@ApplNo and a.ApplNoB=@ApplNoB  and (a.Together1 ='Y' or a.Together2='Y')
                                ) l
                                order by l.CNo ";

            DataTable returnValue = new DataTable();
            try
            {
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@ApplNo", strApplno));
                base.Parameter.Add(new CommandParameter("@ApplNoB", strApplnoB));
                returnValue = base.Search(strSQL);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return returnValue;

        }

        //20140327 把AUM其它的項目加進去
        //20150207 smallzhi 修改支出的計算方式
        public DataTable GetCaculatIncomAum(string strApplno, string strApplnoB, string CusId)
        {
            //edit by mel 20130806 IR-61527 新增TotIncomeForBS 存計算資產負債比的收入總額
            string strSQL = @"select nc.CusId 
                                ,isnull(d1.TotIncome ,0) TotIncome
                                ,isnull(d1_1.TotIncome ,0) TotIncomeForBS
                                ,isnull(d2.TotAum,0) TotAum
                                ,isnull(d21.TotAum,0) TotAumForCusGrade 
                                ,isnull(d3.TotNewLoanAmtForDBR,0) TotNewLoanAmtForDBR
                                ,isnull(d4.TotPayOffAmtForDBR,0) TotPayOffAmtForDBR
                                ,isnull(d5.TotNewLoanAmt,0) TotNewLoanAmt
                                ,isnull(d6.TotPayOffAmt,0) TotPayOffAmt                                
                                from NUMSCustomerInfo nc  
                                left outer join 
                                ( select a.CusId ,sum(a.incometotamt) as TotIncome from UDWRIncome a  where a.ApplNo =@ApplNo and a.ApplNoB=@ApplNoB and a.IncomeCode not in ('13','14','15') group by a.cusid) d1
                                on nc.CusId=d1.CusId
                                left outer join 
                                ( select a.CusId ,sum(a.incometotamt) as TotIncome from UDWRIncome a  where a.ApplNo =@ApplNo and a.ApplNoB=@ApplNoB  group by a.cusid) d1_1
                                on nc.CusId=d1_1.CusId
                                left outer join 
                                (
									select b.CustomerID ,sum(b.FinalAmt) TotAum
									from 
											(
											select b1.CustomerID,b1.AUMItem  ,
											case  when b1.AUMItem='01' or b1.AUMItem='02' or b1.AUMItem='04'  or b1.AUMItem='05' or b1.AUMItem='07' or b1.AUMItem='08' or b1.AUMItem='09' then FinalAmt
											when b1.AUMItem='03' and isnull(b1.ISThreeHold,'')='Y' then FinalAmt
											when b1.AUMItem='06' and (isnull(b1.IsTwoHold,'')='Y' or isnull(b1.ISThreeHold,'')='Y') then FinalAmt
											else 0 end FinalAmt
											from UDWRAUMIncome b1
											where b1.ApplNo =@ApplNo and b1.ApplNoB=@ApplNoB and isnull(AUMItem,'') <>'' and isnull(MbNo,'') =''
											) b
											group by b.CustomerID
                                ) d2
                                on nc.CusId=d2.CustomerID

                                left outer join 
                                (
									select b.CustomerID ,sum(b.FinalAmt) TotAum
									from 
											(
		                                    select b1.CustomerID,b1.AUMItem  ,
		                                    case  when b1.AUMItem='01' or b1.AUMItem='02' or b1.AUMItem='03'  then IncomeAmt
		                                    when b1.AUMItem='04' and isnull(b1.BankContent,'')='1' then IncomeAmt
		                                    when b1.AUMItem='06'  then IncomeAmt
		                                    else 0 end FinalAmt
											from UDWRAUMIncome b1
											where b1.ApplNo =@ApplNo and b1.ApplNoB=@ApplNoB and isnull(AUMItem,'') <>''  and isnull(MbNo,'') =''
											) b
											group by b.CustomerID
                                ) d21
                                on nc.CusId=d21.CustomerID                                
                                
                                left outer join 
                                (   select c.id, sum(isnull(AmountForDBR,0)) as TotNewLoanAmtForDBR
                                 from  UDWRApplCreditNewLoan c
                                  where c.ApplNo = @ApplNo and c.ApplNoB = @ApplNoB and isnull(c.Dbr,'N') = 'Y' and c.NewLoanKind = '1'
                                  group by c.id ) d3
                                on nc.CusId=d3.id
                                left outer join 
                                (   select c.id, sum(isnull(AmountForDBR,0)) as TotPayOffAmtForDBR
                                 from  UDWRApplCreditNewLoan c
                                  where c.ApplNo = @ApplNo and c.ApplNoB = @ApplNoB and isnull(c.Dbr,'N') = 'Y'  and c.NewLoanKind = '2'
                                  group by c.id ) d4 
                                on nc.CusId=d4.id

                                left outer join 
                                (   select c.id, sum(isnull(c.NewLoanAmt ,0)) as TotNewLoanAmt
                                 from  UDWRApplCreditNewLoan c
                                  where c.ApplNo =@ApplNo and c.ApplNoB=@ApplNoB  and c.NewLoanKind='1'
                                  group by c.id ) d5
                                on nc.CusId=d5.id
                                left outer join 
                                (   select c.id, sum(isnull(c.NewLoanAmt ,0)) as TotPayOffAmt
                                 from  UDWRApplCreditNewLoan c
                                  where c.ApplNo =@ApplNo and c.ApplNoB=@ApplNoB
                                      and c.NewLoanKind='2'
                                  group by c.id ) d6 
                                on nc.CusId=d6.id
                                                              
                                where nc.ApplNo =@ApplNo and nc.ApplNoB=@ApplNoB
                                and nc.CusId=@CusId
                                and nc.Status='Y' ";

            DataTable returnValue = new DataTable();
            try
            {
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@ApplNo", strApplno));
                base.Parameter.Add(new CommandParameter("@ApplNoB", strApplnoB));
                base.Parameter.Add(new CommandParameter("@CusId", CusId));
                returnValue = base.Search(strSQL);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return returnValue;

        }

        //20140327 加AUM其它進去
        public DataTable GetCaculatPSRNIncomAum(string strApplno, string strApplnoB, string CusId)
        {

            string strSQL = @"select nc.CusId 
                                ,isnull(d1.TotIncome ,0) TotIncome
                                ,isnull(d2.TotAum,0) TotAum
                                ,isnull(d21.TotAum,0) TotAumForCusGrade 
                                from NUMSCustomerInfo nc  
                                left outer join 
                                ( select a.CusId ,sum(a.incometotamt) as TotIncome from UDWRIncome a  where a.ApplNo =@ApplNo and a.ApplNoB=@ApplNoB  group by a.cusid) d1
                                on nc.CusId=d1.CusId
                                left outer join 
                                (
									select b.CustomerID ,sum(b.FinalAmt) TotAum
									from 
											(
											select b1.CustomerID,b1.AUMItem  ,
											case  when b1.AUMItem='01' or b1.AUMItem='02' or b1.AUMItem='04'  or b1.AUMItem='05' or b1.AUMItem='07' or b1.AUMItem='08' or b1.AUMItem='09' then FinalAmt
											when b1.AUMItem='03' and isnull(b1.ISThreeHold,'')='Y' then FinalAmt
											when b1.AUMItem='06' and (isnull(b1.IsTwoHold,'')='Y' or isnull(b1.ISThreeHold,'')='Y') then FinalAmt
											else 0 end FinalAmt
											from PSRNAUMIncome b1
											where b1.ApplNo =@ApplNo and b1.ApplNoB=@ApplNoB and isnull(AUMItem,'') <>''
											) b
											group by b.CustomerID
                                ) d2
                                on nc.CusId=d2.CustomerID

                                left outer join 
                                (
									select b.CustomerID ,sum(b.FinalAmt) TotAum
									from 
											(
											select b1.CustomerID,b1.AUMItem  ,
											case  when b1.AUMItem='01' or b1.AUMItem='02' or b1.AUMItem='03'  then FinalAmt
											when b1.AUMItem='04' and isnull(b1.BankContent,'')='1' then FinalAmt
											when b1.AUMItem='06' and (isnull(b1.IsTwoHold,'')='Y' or isnull(b1.ISThreeHold,'')='Y') then FinalAmt
											else 0 end FinalAmt
											from PSRNAUMIncome b1
											where b1.ApplNo =@ApplNo and b1.ApplNoB=@ApplNoB and isnull(AUMItem,'') <>''
											) b
											group by b.CustomerID
                                ) d21
                                on nc.CusId=d21.CustomerID                                
                                where nc.ApplNo =@ApplNo and nc.ApplNoB=@ApplNoB
                                and nc.CusId=@CusId  
                                and nc.Status='Y' ";

            DataTable returnValue = new DataTable();
            try
            {
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@ApplNo", strApplno));
                base.Parameter.Add(new CommandParameter("@ApplNoB", strApplnoB));
                base.Parameter.Add(new CommandParameter("@CusId", CusId));
                returnValue = base.Search(strSQL);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return returnValue;

        }

        public DataTable GetUDWRDetailByCusId(string strApplno, string strApplnoB)
        {
            string strSQL = @"select * from UDWRDetailByCusId where ApplNo =@ApplNo and ApplNoB =@ApplNoB  and CusId in 
                                (select CusId  from NUMSCustomerInfo where ApplNo =@ApplNo and ApplNoB =@ApplNoB 
                                and ISNULL(Status, 'Y') ='Y' )";

            DataTable returnValue = new DataTable();
            try
            {
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@ApplNo", strApplno));
                base.Parameter.Add(new CommandParameter("@ApplNoB", strApplnoB));
                returnValue = base.Search(strSQL);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return returnValue;

        }

        public DataTable GetTotalGuaraprice(string strApplno, string strApplnoB)
        {
            //TotalGuaraprice1 = 0;
            //TotalGuaraprice2 = 0;
            //            string strSQL = @"select a.ApplNo,a.ApplNoB , isnull(sum(isnull(TotalAppraisal,0)),0) TotalAppraisal_1 
            //                            , isnull(sum(FLOOR(isnull(TotalAppraisal,0)*0.9)),0) TotalAppraisal_2
            //                            ,isnull(sum(ISNULL(GaranPrice,0)),0) SysAddNetLoan
            //                            ,isnull(sum(ISNULL(BeforeAmt,0)),0) DeSetAmt
            //                            ,case when (select COUNT(b.ApplNo) as btot from UDWRBulkCaseMapping  b where b.applno=a.applno and b.ApplNoB =a.applnob     )  >0 then 'Y' else 'N' end  as BulkCaseFlag
            //                            from APRLDistinguishMain  a
            //                            where ApplNo =@ApplNo and ApplNoB =@ApplNoB and isnull(MbNo ,'')='' 
            //                             group by a.ApplNo,a.ApplNoB      ";
            string strSQL = @"select a.ApplNo,a.ApplNoB , isnull(sum(isnull(TotalAppraisal,0)),0) TotalAppraisal_1 
                            , isnull(sum(FLOOR(isnull(TotalAppraisal,0)*0.9)),0) TotalAppraisal_2
                            ,isnull(sum(ISNULL(GaranPrice,0)),0) SysAddNetLoan
                            ,isnull(sum(ISNULL(BeforeAmt,0)),0) DeSetAmt
                            from APRLDistinguishMain  a
                            where ApplNo =@ApplNo and ApplNoB =@ApplNoB and isnull(MbNo ,'')='' 
                             group by a.ApplNo,a.ApplNoB ";

            DataTable returnValue = new DataTable();
            try
            {
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@ApplNo", strApplno));
                base.Parameter.Add(new CommandParameter("@ApplNoB", strApplnoB));
                returnValue = base.Search(strSQL);
                //if (returnValue.Rows.Count > 0)
                //{
                //    TotalGuaraprice1 = Convert.ToDouble(returnValue.Rows[0]["TotalAppraisal_1"].ToString());
                //    TotalGuaraprice2 = Convert.ToDouble(returnValue.Rows[0]["TotalAppraisal_2"].ToString());
                //}


            }
            catch (Exception ex)
            {
                throw ex;
            }

            return returnValue;



        }

        /// <summary>
        /// 20141108 修改從我新加的Table來抓取
        /// </summary>
        /// <param name="strApplno"></param>
        /// <param name="strApplnoB"></param>
        /// <returns></returns>
        public DataTable GetBulkCaseFlag(string strApplno, string strApplnoB)
        {

            string strSQL = @"  SELECT ApplNo,ApplNoB,isnull(BulkCaseFlag,'') BulkCaseFlag,
                                  case when (select count(1) from UDWRBulkCaseInfo where UDWRBulkCaseInfo.ApplNo = UDWRMaster.ApplNo and UDWRBulkCaseInfo.ApplNoB = UDWRMaster.ApplNoB) > 0 then 'Y' else 'N' end BulkCaseCnt
                                  FROM UDWRMaster
                                  where ApplNo = @ApplNo and ApplNoB = @ApplNoB";

            DataTable returnValue = new DataTable();
            try
            {
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@ApplNo", strApplno));
                base.Parameter.Add(new CommandParameter("@ApplNoB", strApplnoB));
                returnValue = base.Search(strSQL);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return returnValue;
        }

        public DataTable GetPARMCCOCStable(string strApplno, string strApplnoB)
        {

            string strSQL = @"select a.CusId ,isnull(b.StableFlag,'') StableFlag
                            from UDWRDetailByCusId  a left outer join 
                            PARMCCOCStable b on a.JobCC=b.CC and a.JobOC=b.OC 
                            where a.ApplNo =@ApplNo and a.ApplNoB =@ApplNoB and a.CusId in 
                            (select CusId  from NUMSCustomerInfo where ApplNo =@ApplNo and ApplNoB =@ApplNoB and ISNULL(Status, 'Y') ='Y' ) ";

            DataTable returnValue = new DataTable();
            try
            {
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@ApplNo", strApplno));
                base.Parameter.Add(new CommandParameter("@ApplNoB", strApplnoB));
                returnValue = base.Search(strSQL);

            }
            catch (Exception ex)
            {
                throw ex;
            }

            return returnValue;

        }

        public DataTable GetDataForTotalAmt1(string strApplno, string strApplnoB)
        {
            //edit by mel 20130816 IR-62284 
            /*
             * 收回舊案[UDWRNewForOld]的欄位名稱有變更
                1. 請檢查BackGround
                a.ProdDesc ==> WXAcctTypeName
                a.AccTypeCode ==>WXAcctType
             //(edit by mel 20130916 ,經與建中確認過,有勾選全案收回的案件無須納入計算)
             * a.RecAll='N'
             */
            //edit by mel 20131101 IR-63213 
            string strSQL = @"select a.ApplNo ,a.ApplNoB,a.CusId,a.NFOSeq ,a.WXAcctTypeName ProdDesc
                            , case a.WXAcctTypeName  when '代繳卡款' then a.AppyAmt
                            else Cast(isnull(a.AppyAmt,0) as numeric(15,0))  end AppyAmt
                            ,Cast(isnull(a.CurrBal,0) as numeric(15,0)) CurrBal
                            ,Cast(isnull(a.RecParAmt,0)  as numeric(15,0)) RecParAmt
                            ,isnull(a.AcctStatus,'') AcctStatus
                            ,isnull(a.RecPartial,'N') RecPartial
                            ,isnull(a.CalAmt ,'N') CalAmt
                            ,isnull(a.WXAcctType,'') AccTypeCode
                            ,isnull(b.FinancialOverdraft,'N') FinancialOverdraft
                            ,c.LoanRelation
                            ,d.codedesc DbBtyp
                            from UDWRNewForOld  a 
                            left outer join 
                            (select distinct AcctTypeCode,IntCategoryCode,FinancialOverdraft from  PARMBankAcctTypeCode ) b 
                            on a.WXAcctType =b.AcctTypeCode and a.WXIntCat=b.IntCategoryCode
                            left outer join NUMSCustomerInfo c on a.ApplNo =c.ApplNo and a.ApplNoB =c.ApplNoB and a.CusId=c.CusId
                            left outer join 
                            (select * from PARMCode where CodeType ='DBBTYP' ) d on a.DbBtyp=d.codeno
                            where a.ApplNo =@ApplNo and a.ApplNoB=@ApplNoB
                            and isnull(a.AcctStatus,'') not in ('結清','婉拒')
                            and c.Status='Y' and a.AccountNo <>'000000000000' and a.RecAll='N'
                            order by NFOSeq";

            DataTable returnValue = new DataTable();
            try
            {
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@ApplNo", strApplno));
                base.Parameter.Add(new CommandParameter("@ApplNoB", strApplnoB));
                returnValue = base.Search(strSQL);

            }
            catch (Exception ex)
            {
                throw ex;
            }

            return returnValue;

        }

        /// <summary>
        ///edit by mel 20130816 IR-62266 卡的固定額度 邏輯如下
        ///1. 抓JCME的資料!
        ///2. 當流通卡數[IDRLCnt] >=1時, 就抓JCME的信用額度[CrlimitPerm]! 如果否, 就不抓了, 也就是0!
        /// </summary>
        /// <param name="strApplno"></param>
        /// <param name="strApplnoB"></param>
        /// <returns></returns>
        public DataTable GetCardDataCaculate(string strApplno, string strApplnoB)
        {
            //edit by mel 20131101 IR-63213 
            string strSQL = @"select  isnull(sum(cast(CrlimitPerm as numeric(16))),0) CrlimitPerm  from NUMSCardJCMEMaster  
                            where ApplNo =@ApplNo   and ApplNoB=@ApplNoB and Module='UDWR'
                                and ID in (select CusId from NUMSCustomerInfo where ApplNo =@ApplNo  and ApplNoB=@ApplNoB and  LoanRelation in ('1','2') and Status='Y' )
                                and IDRLCnt>=1";
            DataTable returnValue = new DataTable();
            try
            {
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@ApplNo", strApplno));
                base.Parameter.Add(new CommandParameter("@ApplNoB", strApplnoB));
                returnValue = base.Search(strSQL);

            }
            catch (Exception ex)
            {
                throw ex;
            }

            return returnValue;

        }

        public DataTable GetCaculateCreditData1(string strApplno, string strApplnoB)
        {
            //edit by mel 20130816 IR-62284  GH&GI的計算Base, 改抓收回舊案的資料! 初貸日欄位是FstAdvDate 
            #region

            /*
            string strSQL = @"select e.* , DATEDIFF(month,e.FirstAdv ,e.DispatchOperatorDateTime) mondif
                            ,case when DATEDIFF(month,e.FirstAdv ,e.DispatchOperatorDateTime) <=6 then 'GH'  
                            else 'GI' end LoanG
                            from (
                            select a.applno ,a.applnob,a.id,a.limno 
                            ,b.WXAcctType,b.WXFstAdvDte
                            ,c.UnsecureLoan ,d.AuditResult
                            ,d.DispatchOperatorDateTime
                            ,cast(SUBSTRING(b.WXFstAdvDte,5,4) + '/'+ SUBSTRING(b.WXFstAdvDte,3,2) +'/' + SUBSTRING(b.WXFstAdvDte,1,2) as datetime ) FirstAdv
                             from 
                             ( 
                           
									select applno, ApplNoB,ID,Module,LimNo ,Status from NUMSBank67072Detail  
									where LimNo not like '000%'   
									and ApplNo =@ApplNo and ApplNoB=@ApplNoB and  Module ='UDWR'                           
									union                            
									select a1.applno, a1.ApplNoB,a1.ID,a1.Module ,a2.accountno ,a2.ACCTSTUS Status
									--,a1.LimNo,a2.FACILITYNO
									from NUMSBank67072Detail a1 
									left outer join NUMSBank67073Detail a2 
									on a1.ApplNo=a2.ApplNo and a1.ApplNoB=a2.ApplNoB and a1.ID=a2.ID and a1.Module =a2.Module and '0000'+a1.LimNo =a2.FACILITYNO
									where a1.ApplNo =@ApplNo  and a1.ApplNoB=@ApplNoB and  a1.Module ='UDWR'    
									and a1.LimNo like '000%' and not a2.ACCOUNTNO is null

                             )
                             a
                             left outer join NUMSBank32107Master b on a.applno =b.applno and a.applnob=b.applnob and a.module=b.module
                             and a.id =b.id and a.limno=b.acctno
                             left outer join (select distinct AcctTypeCode,UnsecureLoan from  PARMBankAcctTypeCode ) c on b.WXAcctType=c.AcctTypeCode 
                             left outer join UDWRMaster d on a.ApplNo=d.ApplNo and a.ApplNoB =d.ApplNoB
                             where 
                              a.ApplNo =@ApplNo and a.ApplNoB=@ApplNoB  and
                             a.status not in  ('結清','婉拒')  
                             and d.DispatchOperatorDateTime is not null
                             and a.Module='UDWR'
                             and isnull(b.WXFstAdvDte ,'') not in ('','99999999') 
                             and c.UnsecureLoan='Y'
                             ) e ";
              */
            #endregion

            //edit by mel 20131122 
            //因應user要求,排除保人資料
            string strSQL = @"select a.*
                            ,b.UnsecureLoan,c.DispatchOperatorDateTime
                            ,case when DATEADD(month ,6,a.FstAdvDate) >  getdate() then 'GH'  
                            else 'GI' end LoanG
                            from 
                            (
	                            select ApplNo,ApplNoB,CusID ID 
	                            ,WXAcctType ,FstAdvDate  ,WXIntCat
	                              from 
                                    (                                  
                                      select * from UDWRNewForOld where ApplNo =@ApplNo  and ApplNoB=@ApplNoB  
                                    and CusID in (select cusid from NUMSCustomerInfo where ApplNo =@ApplNo and ApplNoB=@ApplNoB and LoanRelation in ('1','2') and Status ='Y'  )
                                    and DbBtyp in ( select CodeNo from PARMCode where CodeType='DBBTYP' and CodeTag in ('1','2') )
                                    ) a1
                                    where a1.ApplNo =@ApplNo and a1.ApplNoB =@ApplNoB 
                            ) a
                            left outer join (select distinct AcctTypeCode,IntCategoryCode,UnsecureLoan from  PARMBankAcctTypeCode ) b on a.WXAcctType=b.AcctTypeCode  and a.WXIntCat=b.IntCategoryCode
                            left outer join UDWRMaster c on a.ApplNo=c.ApplNo and a.ApplNoB =c.ApplNoB
                            where 
                            c.DispatchOperatorDateTime is not null
                            and a.FstAdvDate is not null
                            and b.UnsecureLoan='Y' ";

            DataTable returnValue = new DataTable();
            try
            {
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@ApplNo", strApplno));
                base.Parameter.Add(new CommandParameter("@ApplNoB", strApplnoB));
                returnValue = base.Search(strSQL);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return returnValue;

        }

        public DataTable GetDISPAddCaseByCSCData()
        {
            //edit by mel 20140109 修正申請金額以元顯示
            //string strSQL = @"select * from DISPAddCaseByCSC where ISNULL(importflag,'N')='N' order by ApplNo";
            string strSQL = @"SELECT     ApplNo, ApplNoB, CSC_No, Cus_Id, Cus_Name, Case_Prod_Type, Case_Type, Appr_Unit, Old_App_No, Account_No, Loan_Line * 10000 as Loan_Line, TEL_O, TEL_H, TEL_M, Reply_Item1, 
                            Lending_Rate_Old, Lending_Rate_New, Fine_Deadline_Yn, Fine_Deadline_New, Consent_Letter, Get_Item_Method, Reply_Item2, Loan_Period_Old, 
                            Loan_Period_New, Reply_Item3, Extended_Limit, Extended_Deadline_Old, Extended_Deadline_New, Reply_Item4, Loan_Terms_Old, Loan_Terms_New, Reply_Item5,
                            Guarantee_Old, Guarantee_New, Guarantee_Id_New, Guarantee_Tel_New, Reply_Item6, Reply_Item7, Reply_Item8, Apply_Comment, JCIC_Date, Print_Date, 
                            Guarantee_Tel, ImportFlag, CreatedUser, CreatedDate, ModifiedUser, ModifiedDate, CloseByNUMSFlag, FinishDate, DeliverDate, ProcessEmpNo, ProcessDesc
                            FROM         DISPAddCaseByCSC
                            WHERE     (ISNULL(ImportFlag, 'N') = 'N')
                            ORDER BY ApplNo";

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


        public void CaculateLiabSpendingAndAUMIncome(string applno, string applnob, out Dictionary<string, double> spending, out Dictionary<string, double> aumincome)
        {
            #region 說明
            /* 初審DBR
             * 負債支出計算= 聯徵的EJCIC COL195 ? + 本次(申請案件:依下列條件) + (LFA2上方主借人的JCIC區塊內的其它負債(科目百分比)*放款金額 )
                a.理透= 額度(申請金額) * (PARM) JCIC科目的支出百分比
                b.非理透=額度(申請金額) * (基本利率(最高final利率) + 1.07 (參數) )* (攤還期數-寬限期)無條件進位到萬元
                P.S 計算上方的非理透時,攤還期數最高只能用240 (系統參數放,因為可能會調整)
             * AUMINcome
             資產的認定規則如下：
                (1)	台幣活存:本行用3個月,他行月2個月->
                可認客層 + 可放入償債能力(資產負債比 中當資產)
                (2)	外幣活存 : 同台幣活存
                可認客層 + 可放入償債能力(資產負債比 中當資產)
                (3)	定存:一定要填是與否,預設:請選擇。
                拉是：可認客層 + 可放入償債能力(資產負債比 中當資產)
                拉否：可認客層 +不可放入償債能力(資產負債比 中當資產)
                (4)	保險:內容:他行也可認,須有相關文件,增加拉霸,預設 中信保經,但可填其他保險公司.權重不分。
                可認客層 + 可放入償債能力(資產負債比 中當資產)
                2013/4/24修正：只有中信保經要算客層，但中信保經和其他保險公司都要放入償債能力。
                (5)	股票：權重為固定，
                不可認客層 + 可放入償債能力(資產負債比 中當資產)。
                (6)	基金:權重本行 :100%、權重他行 :94%。
                本與他行都需key1,2月。
                可認客層、要持有2個月以上才可可放入償債能力(資產負債比 中當資產)
                (7)	房地不動產淨值：不可認客層 + 可放入償債能力(資產負債比 中當資產)
                (8)	土地淨值:：不可認客層 + 可放入償債能力(資產負債比 中當資產) 
             * * 
             */
            #endregion



            spending = new Dictionary<string, double>();
            aumincome = new Dictionary<string, double>();

            #region 變數定義
            //各產品申請金額(萬元)
            double loanamt = 0;
            //各產品最後計算後核淮額度
            //double finalprodamt = 0;
            //各產品各階段最高利率
            //double maxprodrate = 0;
            //該產品policy所定義加碼利率
            double policyrate = 0;
            //年負債支出Year liabilities expenses
            double yearliabilyexp = 0;
            //科目(ES)百分比
            double essubjectrate = 0;
            ////攤還期數
            //int amortizarionperiod = 0; // AmortizarionPeriod
            //policy所認定最高攤還期數
            int maxamortizarionperiod = 0;
            ////寬限期
            //int approveloanextend = 0;
            //預設為非理透產品
            string OverdrawnFlag = "0";

            string stepid = "";
            string strMessage = "";
            DataTable _prodlist = new DataTable();
            DataTable tmpdt = new DataTable();
            string newid = "";
            string custid = "";


            //建立model object 
            //_numsbiz = new NUMSUtilBIZ(dataSource, initCatalog, uid, password);

            //建立rule engine object 
            CTCB.NUMS.RuleEngine.RuleEngineBIZ _reBIZ = new CTCB.NUMS.RuleEngine.RuleEngineBIZ(_dataSource, _initCatalog, _userID, _passWord);

            CTCB.NUMS.RuleEngine.RuleEngine _ruleEngine = new CTCB.NUMS.RuleEngine.RuleEngine();
            _ruleEngine.Sqldb = _reBIZ;

            #endregion


            try
            {
                //計算policy所定的加碼利率及其他相關規定值

                #region
                Hashtable inputparm = new Hashtable();
                Hashtable returnparm = new Hashtable();
                DataTable parm2step = new DataTable();
                DataTable policy2step = new DataTable();
                stepid = "PolicyDefineForCal";
                //計算policy所定的加碼利率
                bool blnResult = _ruleEngine.PolicyProcessRet2("00", stepid, applno, applnob, "0", inputparm, ref parm2step, ref policy2step, ref strMessage);
                if (blnResult)
                {
                    //policy所定義加碼利率
                    DataRow[] ruledr = policy2step.Select("POLICYID='RatePlus'");
                    if (ruledr.Length > 0)
                        policyrate = GetDouble(ruledr[0]["_RETURNVALUE"].ToString());

                    //policy所認定最高攤還期數
                    ruledr = policy2step.Select("POLICYID='AmortizarionPeriod'");
                    if (ruledr.Length > 0)
                        maxamortizarionperiod = GetInt16(ruledr[0]["_RETURNVALUE"].ToString());
                }
                else
                {
                    throw new Exception("Error in CaculateLiabSpendingAndAUMIncome:" + strMessage);
                }
                #endregion


                //取出科目百分比
                essubjectrate = GetPARMEgActMapWeight("ES");

                _prodlist = GetPSRNProductlist(applno, applnob);

                #region 計算該產品的負債支出
                foreach (DataRow _dr in _prodlist.Rows)
                {
                    yearliabilyexp = 0;

                    loanamt = GetDouble(_dr["Amt"].ToString());
                    newid = _dr["NEWID"].ToString();

                    OverdrawnFlag = _dr["OverdrawnFlag"].ToString();
                    if (OverdrawnFlag == "1")
                    {
                        //取得
                        yearliabilyexp = RoundUp(loanamt * essubjectrate / 10000, 0) * 10000;
                    }
                    else
                    {
                        CalculateNoneOverdrawnProduct(_dr, maxamortizarionperiod, policyrate, loanamt
                            , ref yearliabilyexp);
                        //edit by mel ,取整數 20131225
                        //yearliabilyexp = RoundUp(yearliabilyexp / 10000, 0) * 10000;
                        yearliabilyexp = RoundUp(yearliabilyexp, 0);
                    }

                    spending.Add(newid, yearliabilyexp);

                }
                #endregion

                #region 計算AUMIncome

                DataTable iddt = GetQueryID(applno, applnob);

                foreach (DataRow _dr in iddt.Rows)
                {
                    custid = _dr["CusId"].ToString();
                    tmpdt = GetCaculatPSRNIncomAum(applno, applnob, custid);
                    if (tmpdt.Rows.Count > 0)
                        aumincome.Add(custid, GetDouble(tmpdt.Rows[0]["TotAum"].ToString()));
                    else
                        aumincome.Add(newid, 0);
                }

                #endregion


            }
            catch (Exception exe)
            {
                throw exe;
            }



        }

        /// <summary>
        /// add by mel 20130807 IR-61788 計算特審案件
        /// </summary>
        /// <param name="applNo"></param>
        /// <param name="applNoB"></param>
        /// <param name="dbTransaction"></param>
        public void CaculateSpecialAuditCase(string applNo, string applNoB, string _op = "", IDbTransaction dbTransaction = null)
        {
            string flag = "";
            CaculateSpecialAuditCase(applNo, applNoB, ref  flag, _op, dbTransaction);

        }

        /// <summary>
        /// add by mel 20130830 IR-62531
        /// </summary>
        /// <param name="applNo"></param>
        /// <param name="applNoB"></param>
        /// <param name="specflag"></param>
        /// <param name="_op"></param>
        /// <param name="dbTransaction"></param>
        public void CaculateSpecialAuditCase(string applNo, string applNoB, ref string specaseflg, string _op = "", IDbTransaction dbTransaction = null)
        {
            string sql = "";
            IDbConnection dbConnection = base.OpenConnection();

            // DB連接
            using (dbConnection)
            {
                try
                {
                    #region 0506RC Edit by 小朱
                    sql = @" 
                    declare @Result table(ApplNo nvarchar(20),ApplNoB nvarchar(1),UDWRMasterSpecialAuditCaseFlag char(1),UDWRMasterAsignSpecialAuditCaseFlag char(1),APRLDistinguishMainSpecialAuditCaseFlag char(1),GuaraNo nvarchar(5))

                    --取擔保品資訊並轉換金額單位
                    ;with  T0 as(
                    select
                     AM.ApplNo
                    ,AM.ApplNoB
                    ,isnull(AM.SellPrice,0)*10000 as SellPrice--(單位:元) 買賣價
                    ,M.LoanAmt--(單位:元)
                    ,AM.TotalPrice--(單位:元) 總時價
                    ,AM.HouseType
                    ,U.GuaraLoanRateOld --CLTV
                    ,GuaraNo
                    from APRLDistinguishMain AM 
                    left join UDWRMaster U on AM.ApplNo = U.ApplNo and AM.ApplNoB = U.ApplNoB
                    left join NUMSMaster M on AM.ApplNo = M.ApplNo and AM.ApplNoB = M.ApplNoB
                    where AM.ApplNo = @ApplNo and AM.ApplNoB = @ApplNoB
                    ) 

                    --處理CLTV(成數)
                    ,T1 as (
                    select
                     ApplNo
                    ,ApplNoB
                    ,SellPrice
                    ,TotalPrice
                    ,HouseType
                    ,ceiling(isnull(GuaraLoanRateOld,0)*100) CLTV1
                    ,(Case when LoanAmt is not null and LoanAmt <> '0' and SellPrice is not null and SellPrice <> '0'then ceiling((LoanAmt/SellPrice)*100) else '0' end) CLTV2
                    ,GuaraNo
                    from T0
                    )

                    --取擔保品區域資訊
                    ,T2 as(
                    select
                     ApplNo
                    ,ApplNoB
                    ,SellPrice
                    ,TotalPrice
                    ,(case when HouseType in ('R4','R5') then '1' when HouseType is null or HouseType = '' then (case when AB.FloorBel = 'ff3' then '1' else '2' end) else '2' end)  HouseType
                    ,CLTV1
                    ,CLTV2
                    ,AB.AreaNo 
                    ,GuaraNo
                    from T1 
                    cross apply (select top 1 AreaNo,FloorBel from APRLGuaranteeBuilding AB where T1.ApplNo = AB.ApplNo and T1.GuaraNo = AB.GuaraNo order by AB.BulNo asc) AB )

                    --取比對條件
                    ,T3 as (
                    select
                    ApplNo
                    ,ApplNoB
                    ,SellPrice
                    ,T2.TotalPrice
                    ,HouseType
                    ,CLTV1
                    ,CLTV2
                    ,AreaNo 
                    ,p.TotalPrice * 10000 PTotalPrice 
                    ,p.CLTV PCLTV
                    ,GuaraNo
                    from T2 
                    left join PARMAreaHighPrice P on T2.AreaNo = P.ZipCode and T2.HouseType = P.HouseTypeFlag)

                    --業務邏輯判斷
                    ,T4 as(
                    select 
                    ApplNo
                    ,ApplNoB
                    ,(case when T3.PTotalPrice is NULL OR T3.PCLTV is NULL then 'N' when T3.TotalPrice >= T3.PTotalPrice and T3.CLTV1 >= T3.PCLTV then 'Y' else 'N' end) UDWRMasterSpecialAuditCaseFlag
                    ,(case when T3.PTotalPrice is NULL OR T3.PCLTV is NULL then 'N' when T3.SellPrice >= T3.PTotalPrice and T3.CLTV2 >= T3.PCLTV then 'Y' else 'N' end) UDWRMasterAsignSpecialAuditCaseFlag
                    ,(case when T3.PTotalPrice is NULL then 'N' when T3.TotalPrice >= T3.PTotalPrice then 'Y' else 'N' end) APRLDistinguishMainSpecialAuditCaseFlag 
                    ,GuaraNo
                    from T3
                    )

                    insert into @Result select ApplNo,ApplNoB,UDWRMasterSpecialAuditCaseFlag,UDWRMasterAsignSpecialAuditCaseFlag,APRLDistinguishMainSpecialAuditCaseFlag,GuaraNo from T4

                    update UDWRMaster 
                    set 
                    ModifiedUser = @LogOnUser,
                    ModifiedDate = getdate(),
                    SpecialAuditCaseFlag = (case when (select COUNT(1) from @Result R where R.UDWRMasterSpecialAuditCaseFlag = 'Y') > 0 then 'Y' else 'N' end)
                    ,AsignSpecialAuditCaseFlag = (case when (select COUNT(1) from @Result R where R.UDWRMasterAsignSpecialAuditCaseFlag = 'Y') > 0 then 'Y' else 'N' end)
                    where ApplNo = @ApplNo and ApplNoB = @ApplNoB

                    update  AM 
                    set 
                    ModifiedUser = @LogOnUser,
                    ModifiedDate = getdate(),
                    SpecialAuditCaseFlag = isnull(R.APRLDistinguishMainSpecialAuditCaseFlag,'N') 
                    from APRLDistinguishMain AM left join @Result R on AM.ApplNo = R.ApplNo and AM.ApplNoB = R.ApplNoB and AM.GuaraNo = R.GuaraNo
                    where AM.ApplNo = @ApplNo and AM.ApplNoB = @ApplNoB
                    ";
                    // 清空容器
                    base.Parameter.Clear();
                    base.Parameter.Add(new CommandParameter("@ApplNo", applNo));
                    base.Parameter.Add(new CommandParameter("@ApplNoB", applNoB));
                    base.Parameter.Add(new CommandParameter("@LogOnUser", _op));

                    #endregion

                    if (dbTransaction != null)
                    {
                        base.ExecuteNonQuery(sql, dbTransaction);
                    }
                    else
                    {
                        base.ExecuteNonQuery(sql);
                    }

                }
                catch (Exception ex)
                {
                    // 拋出異常
                    throw ex;
                }
            }
        }

        /// <summary>
        /// 20141205 最後計算期數, 加一個判斷, 來判斷要不要被最大期數限制
        /// </summary>
        /// <param name="_dr"></param>
        /// <param name="maxamortizarionperiod"></param>
        /// <param name="policyrate"></param>
        /// <param name="loanamt"></param>
        /// <param name="yearliabilyexp"></param>
        private void CalculateNoneOverdrawnProduct(DataRow _dr, int maxamortizarionperiod, double policyrate, double loanamt
                    , ref double yearliabilyexp)
        {
            int amortizarionperiod = 0;
            double maxprodrate = 0;
            int approveloanextend = 0;

            //計算最後計算期數
            amortizarionperiod = GetInt16(_dr["AmortizarionPeriod"].ToString());
            NUMSProductBIZ _npz = new NUMSProductBIZ(); //20141205
            bool _isUseMax = false; //20140112
            string _PeriodMax = _npz.GetPeriodMax(_dr["ApplNo"].ToString(), _dr["ApplNoB"].ToString(), "PSRN"); //20141205 20140112
            if (_PeriodMax == "-1") { _isUseMax = true; }//20140112 表示要用原來的值
            if (_PeriodMax != "-1" && _PeriodMax != "0") { maxamortizarionperiod = Convert.ToInt32(_PeriodMax); _isUseMax = true; }//20140112 表示要用設定表的值
            if (_isUseMax) //要被最高期數限制 20141205
            {
                if (amortizarionperiod > maxamortizarionperiod)
                    amortizarionperiod = maxamortizarionperiod;
            }
            //計算最高利率
            //edit by mel 20131225
            maxprodrate = Max(GetDouble(_dr["FirstRate"].ToString()), GetDouble(_dr["SecondRate"].ToString()), GetDouble(_dr["ThirdRate"].ToString())) / 100 + policyrate;

            //取出寬限期
            approveloanextend = GetInt16(_dr["LoanExtend"].ToString());

            //攤還期數為0則無須計算
            if (amortizarionperiod == 0 || (amortizarionperiod - approveloanextend) < 1)
            {
                yearliabilyexp = 0;
                return;
            }

            //edit by mel 20131225
            //修正計算公式
            //計算負債年支出
            //yearliabilyexp = RoundUp(
            //                                    Financial.Pmt(
            //                                                    maxprodrate / 12
            //                                                    , (amortizarionperiod - approveloanextend)
            //                                                    , loanamt
            //                                                    , 0
            //                                                    , 0
            //                                                 )
            //                                    , 0
            //                                    ) * 12;

            ////轉換為正值
            //if (yearliabilyexp < 0) yearliabilyexp = yearliabilyexp * -1;

            yearliabilyexp = RoundUp(Financial.Pmt(maxprodrate / 12, (amortizarionperiod - approveloanextend), loanamt * -1, 0, 0) * 12, 0);
            if (yearliabilyexp < 0) yearliabilyexp = yearliabilyexp * -1;
        }

        public double GetDouble(string value)
        {
            double result = 0;
            try
            {
                result = Convert.ToDouble(value);
            }
            catch
            {
                result = 0;
            }

            return result;
        }

        public double RoundUp(double number, int place)
        {
            if (double.IsInfinity(number) || double.IsNaN(number) || double.IsNegativeInfinity(number) || double.IsPositiveInfinity(number))
                return 0;
            return (Math.Ceiling(number * Math.Pow(10, place)) / Math.Pow(10, place));
        }

        /// <summary>
        /// 無條件拾去
        /// </summary>
        /// <param name="number"></param>
        /// <param name="place"></param>
        /// <returns></returns>
        public double Round(double number, int place)
        {
            if (double.IsInfinity(number) || double.IsNaN(number) || double.IsNegativeInfinity(number) || double.IsPositiveInfinity(number))
                return 0;
            return ((int)(number * Math.Pow(10, place)) / Math.Pow(10, place));
        }

        public decimal GetDecimal(string value)
        {
            decimal result = 0;
            try
            {
                result = Convert.ToDecimal(value);
            }
            catch
            {
                result = 0;
            }

            return result;
        }

        public Int16 GetInt16(string value)
        {
            Int16 result = 0;
            try
            {
                result = Convert.ToInt16(value);
            }
            catch
            {
                result = 0;
            }

            return result;
        }

        /// <summary>
        /// 三數取最大數
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        public double Max(double a, double b, double c)
        {
            double result = 0;

            ArrayList arl = new ArrayList();
            arl.Add(a);
            arl.Add(b);
            arl.Add(c);
            arl.Sort();
            result = Convert.ToDouble(arl[2].ToString());
            return result;

        }

        public void GenerateDataForUDWR(DataTable _datatransfersetting, DataTable _transtable, string type, string strApplno, string strApplnoB, string _op = "")
        {

            GenerateDataForUDWR(_datatransfersetting, _transtable, type, strApplno, strApplnoB, "", _op);

        }

        public void GenerateDataForUDWR(DataTable _datatransfersetting, DataTable _transtable, string type, string strApplno, string strApplnoB, string filter, string _op, IDbTransaction dbTransaction = null)
        {
            DataTable _sourcedt, _destdt;
            DataTable _detdt = new DataTable();

            DataRow _destdr;
            string soucetablename, desttablename, transtype;
            DataRow[] _drtable;
            DataRow[] _settingdr;


            _drtable = _transtable.Select("TransferType like '" + type + "' ");

            foreach (DataRow dr in _drtable)
            {
                transtype = dr["TransferType"].ToString();
                soucetablename = dr["SourceTable"].ToString();
                desttablename = dr["TargetTable"].ToString();

                //取出資料來源
                _sourcedt = new DataTable();
                _sourcedt = OpenDataTable(soucetablename, strApplno, strApplnoB, filter);

                //刪除目的table的資料
                DeleteDataTable(desttablename, strApplno, strApplnoB, filter);


                //準備要新增的Table 及欄位
                string _colname = "";
                _settingdr = _datatransfersetting.Select("TransferType='" + transtype + "'");

                for (int i = 0; i < _settingdr.Length; i++)
                {
                    if (i != 0)
                    {
                        _colname += ",";

                    }
                    _colname += _settingdr[i]["TargetField"].ToString();

                }

                _colname += ",CreatedUser,CreatedDate";
                _destdt = new DataTable();
                //_detdt = _sUtilBiz.GetEmptyDataTable(desttablename, "ModifiedUser,ModifiedDate");
                _detdt = GetEmptyDataTableByColumn(desttablename, _colname);



                foreach (DataRow _sourcedr in _sourcedt.Rows)
                {
                    _destdr = _detdt.NewRow();


                    foreach (DataRow drA in _settingdr)
                    {
                        _destdr[drA["TargetField"].ToString()] = _sourcedr[drA["SourceField"].ToString()];
                    }
                    _destdr["CreatedUser"] = _op;
                    _destdr["CreatedDate"] = System.DateTime.Now;
                    _detdt.Rows.Add(_destdr);
                }

                InsertIntoTable(_detdt, dbTransaction);

            }


        }

        #region For 徵信
        /// <summary>
        /// Added by smallzhi for 自動將原因碼放入負面表列或異常事項中[關卡:2140,重查 重算 第一次]
        /// </summary>
        /// <param name="_applNo"></param>
        /// <param name="_applNoB"></param>
        /// 20140124 增加重算跟重查的判斷
        public void UDWRExceptionRefresh(string _applNo, string _applNoB, string _op)
        {
            string strSQL = @"delete from UDWRException where ApplNo = @ApplNo and ApplNoB = @ApplNoB and CodeType in (1,2) and RelationKind not in ('98','99'); --先刪除策略的原因碼[只刪負面跟異常的, 99是徵信手動, 98是鑑價要排除]

                                declare @UseRecastReason char(1);
                                select @UseRecastReason = UseRecastReason from UDWRMaster where ApplNo = @ApplNo and ApplNoB = @ApplNoB;
                                
                                set @UseRecastReason = isnull(@UseRecastReason,'0'); --如果是null就放0

                                --重新取得新的原因碼
                                ;With T1 as  --先抓原因碼跟重算原因碼
                                (
                                select 1 as Rtype, ReasonCode, CusID, DivKind from UDWRReasonCode where ApplNo = @ApplNo and ApplNoB = @ApplNoB
                                union all
                                select 2 as Rtype, ReasonCode, CusID, DivKind from UDWRRecastReasonCode where ApplNo = @ApplNo and ApplNoB = @ApplNoB
                                )
                                , T2 as --如果有重算原因碼就抓重算原因碼
                                (
                                select ReasonCode, CusID, DivKind from T1
                                where Rtype = case when (select count(0) from T1 where Rtype = 2) > 0 or @UseRecastReason = '1' then 2 else 1 end
                                ), T3 as --串原因碼的設定檔[抓啟用的]
                                (
                                select 
                                (select top 1 CodeType from RULEAdrCodeExtra where RuleCode = T2.ReasonCode and CodeCategory = '2') as CodeType,
                                T2.ReasonCode, 
                                CusID, 
                                DivKind,
                                OverrideFlag
                                from T2
                                inner join RULEAdrCode on T2.ReasonCode = RULEAdrCode.ReasonCode and EnableFlag = 'Y'
                                )
                                --寫入原因碼
                                insert into UDWRException([ApplNo],[ApplNoB],[CreditSeqNo],[CodeType],[CodeNo],[CodeRelation],[RelationKind]
                                ,[Sequence],[CreatedDate],[CreatedUser],[ModifiedDate],[ModifiedUser])
                                select 
                                @ApplNo,
                                @ApplNoB,
                                null,
                                case when CodeType in ('GE_N','GE_H','H_MT') then '1' else '2' end,
                                ReasonCode,
                                CusID, 
                                DivKind,
                                2,
                                getdate(),
                                @ModifiedUser,
                                getdate(),
                                @ModifiedUser
                                from T3
                                where codetype in ('GE_N','H_NE','GE_H','G_NE','H_MT');";

            try
            {
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@ApplNo", _applNo));
                base.Parameter.Add(new CommandParameter("@ApplNoB", _applNoB));
                base.Parameter.Add(new CommandParameter("@ModifiedUser", _op));
                base.ExecuteNonQuery(strSQL);

                UDWRBIZ _UDWRBIZ = new UDWRBIZ(GetConnectionString());

                IList<UDWRCommentVO> OverrideList = _UDWRBIZ.GetOverrideByFlag(_applNo, _applNoB); //依據OverrideFlag, 取得Override
                UDWRAuthInfoVO _UDWRAuthInfoVO = _UDWRBIZ.GetAuthInfoForB(_applNo, _applNoB);
                IList<UDWRCommentVO> NewList = _UDWRBIZ.GetOverrideByCase(OverrideList, _UDWRAuthInfoVO.NormalRate, _UDWRAuthInfoVO.OverrideRate, _UDWRAuthInfoVO.DebtRate, _UDWRAuthInfoVO.DebtPayingRule, _UDWRAuthInfoVO.GuaraLoanRateOld, _UDWRAuthInfoVO.DebtPayingMode); //依據案件的條件, 排掉Override

                #region 寫入DB OverrideFlag = Y
                string sql = @"delete from UDWRException where ApplNo = @ApplNo and ApplNoB = @ApplNoB and CodeType = 3;"; //先刪掉Override

                IList<UDWRCommentVO> _list = NewList.Where(p => p.OverrideFlag == "Y").ToList(); //只取OverrideFlag 最後還是Y的
                if (_list != null && _list.Count > 0)
                {
                    sql = sql + @"insert into UDWRException([ApplNo],[ApplNoB],[CreditSeqNo],[CodeType],[CodeNo],[CodeRelation],[RelationKind]
                                ,[Sequence],[CreatedDate],[CreatedUser],[ModifiedDate],[ModifiedUser]) values";
                    int i = 0;
                    foreach (UDWRCommentVO item in _list)
                    {
                        if (i > 0) { sql = sql + @","; }
                        sql = sql + @"(@ApplNo,@ApplNoB,null,3,@CodeNo" + i + @",@CodeRelation" + i + @",@RelationKind" + i + ",@Sequence" + i + ",getdate(),@ModifiedUser,getdate(),@ModifiedUser)";
                        base.Parameter.Add(new CommandParameter("@CodeNo" + i, item.CodeNo));
                        base.Parameter.Add(new CommandParameter("@CodeRelation" + i, item.CodeRelation));
                        base.Parameter.Add(new CommandParameter("@RelationKind" + i, item.RelationKind));
                        base.Parameter.Add(new CommandParameter("@Sequence" + i, item.Sequence));
                        i++;
                    }
                }
                base.ExecuteNonQuery(sql);
                #endregion
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Added by smallzhi for 將67072,670732的帳號[串32107資訊], 寫入UDWRNewForOld的Table[關卡:2140,只有重查 第一次]
        /// </summary>
        /// <param name="_applNo"></param>
        /// <param name="_applNoB"></param>
        public void UDWRNewForOldRefresh(string _applNo, string _applNoB, string _op)
        {
            string strSQL = @"  delete from dbo.UDWRNewForOld where ApplNo = @ApplNo and ApplNoB = @ApplNoB; --刪除UDWRNewForOld
                                delete from dbo.UDWRNewForOldExtra where ApplNo = @ApplNo and ApplNoB = @ApplNoB; --刪除UDWRNewForOldExtra

                                declare @T1 table([ApplNo] nvarchar(15),[ApplNoB] nvarchar(1),LoanRelation nvarchar(4),[ID] nvarchar(12)
                                ,[LimNo] nvarchar(17),[ProdU] nvarchar(10),[AppDat] nvarchar(8),[ExpDat] nvarchar(8)
                                ,[Amt] nvarchar(14),[Curr] nvarchar(3),[BalAmt] numeric(14,0),[DbBtyp] nvarchar(4),[Status] nvarchar(6),CaseType char(1)); --宣告T1

                                insert into @T1
                                SELECT NUMSCustomerInfo.[ApplNo]
                                      ,NUMSCustomerInfo.[ApplNoB]
                                      ,LoanRelation
                                      ,[CusID]
                                      ,[LimNo]
                                      ,[ProdU]
                                      ,[AppDat]
                                      ,[ExpDat]
                                      ,[Amt]
                                      ,[Curr]
                                      ,[BalAmt]
                                      ,[DbBtyp]
                                      ,[NUMSBank67072Detail].[Status]
                                      ,Case when substring(LimNo,1,3) = '000' then 'F' else 'A' end CaseType
                                  FROM NUMSCustomerInfo 
                                  inner join [NUMSBank67072Detail] on NUMSCustomerInfo.ApplNo = [NUMSBank67072Detail].applno and NUMSCustomerInfo.ApplNob = [NUMSBank67072Detail].applnob 
                                  and [NUMSBank67072Detail].ID = CusID and Module = 'UDWR' 
                                  and ([NUMSBank67072Detail].[Status] is null or ([NUMSBank67072Detail].[Status] <> '結清' and [NUMSBank67072Detail].[Status] <> '婉拒' and [NUMSBank67072Detail].[Status] <> '清檔' and [NUMSBank67072Detail].[Status] <> '鍵錯' and [NUMSBank67072Detail].[Status] <> '移出' and [NUMSBank67072Detail].[Status] <> '作廢'))
                                  where NUMSCustomerInfo.ApplNo = @ApplNo and NUMSCustomerInfo.ApplNoB = @ApplNoB and NUMSCustomerInfo.[Status] = 'Y'; --只抓啟用的客戶

                                  ;with T2 as
                                  (
                                  select
                                  'B' CaseSource,
                                  T1.Applno,
                                  T1.Applnob,
                                  LoanRelation,
                                  T1.ID,
                                  T1.LimNo AccountNo,
                                  T1.Curr,
                                  WXAcctType,
                                  WXIntCat,
                                  WXAcctTypeName,
                                  WXFstAdvDte,
                                  WXMaturityDate,
                                  WXApprAmt,
                                  WXLoanBal,
                                  isnull(WXStatus,'') WXStatus,
                                  T1.[DbBtyp],
                                  '' as FacilityNo,
                                  '00000000' as FacilityAmt,
                                  '' as FacilityProd,
                                  case when NUMSBank32107Master.[NewID] is null then 'N' else 'Y' end In32107,
                                  AppDat,
                                  EXPDAT
                                  from @T1 T1
                                  left join dbo.NUMSBank32107Master on NUMSBank32107Master.Applno = T1.Applno 
                                  and NUMSBank32107Master.Applnob = T1.Applnob and NUMSBank32107Master.ID = T1.ID
                                  and NUMSBank32107Master.AcctNo = T1.LimNo and Module = 'UDWR'
                                  where CaseType = 'A'
                                  union all
                                  select
                                  'B' CaseSource,
                                  T1.Applno,
                                  T1.Applnob,
                                  LoanRelation,
                                  T1.ID,
                                  NUMSBank67073Detail.AccountNo AccountNo,
                                  T1.Curr,
                                  WXAcctType,
                                  WXIntCat,
                                  WXAcctTypeName,
                                  WXFstAdvDte,
                                  WXMaturityDate,
                                  WXApprAmt,
                                  WXLoanBal,
                                  isnull(WXStatus,'') WXStatus,
                                  T1.[DbBtyp],
                                  T1.LimNo as FacilityNo,
                                  T1.Amt as FacilityAmt,
                                  T1.ProdU as FacilityProd,
                                  case when NUMSBank32107Master.[NewID] is null then 'N' else 'Y' end In32107,
                                  AppDat,
                                  EXPDAT
                                  from @T1 T1
                                  inner join [dbo].[NUMSBank67073Detail] on NUMSBank67073Detail.Applno = T1.Applno 
                                  and NUMSBank67073Detail.Applnob = T1.Applnob and NUMSBank67073Detail.ID = T1.ID
                                  and NUMSBank67073Detail.FacilityNo = '0000' + T1.LimNo and NUMSBank67073Detail.Module = 'UDWR'
                                  left join dbo.NUMSBank32107Master on NUMSBank32107Master.Applno = T1.Applno 
                                  and NUMSBank32107Master.Applnob = T1.Applnob and NUMSBank32107Master.ID = T1.ID
                                  and NUMSBank32107Master.AcctNo = NUMSBank67073Detail.AccountNo and NUMSBank32107Master.Module = 'UDWR'
                                  where CaseType = 'F'
                                  ),T3 as
                                  (
                                  select
                                  CaseSource,
                                  Applno,
                                  Applnob,
                                  LoanRelation,
                                  ID,
                                  AccountNo,
                                  Curr,
                                  WXAcctType,
                                  WXIntCat,
                                  WXAcctTypeName,
                                  WXFstAdvDte,
                                  WXMaturityDate,
                                  WXApprAmt,
                                  WXLoanBal,
                                  WXStatus,
                                  [DbBtyp],
                                  FacilityNo,
                                  FacilityAmt,
                                  FacilityProd,
                                  In32107,
                                  AppDat,
                                  EXPDAT,
                                  [dbo].[GetFstAdvDate](WXFstAdvDte,AppDat) FstAdvDate,
                                  [dbo].[GetFstAdvDate](WXMaturityDate,EXPDAT) MaturityDate
                                  from T2
                                  where [WXStatus] <> '結清' and [WXStatus] <> '婉拒'
                                  )

                                  insert into dbo.UDWRNewForOld(NFOSeq,NFOSeqNo,Source,ApplNo,ApplNoB,CusId,AccountNo,Curr,WXAcctType,WXIntCat,
                                  WXAcctTypeName,WXFstAdvDte,WXMaturityDate,WXApprAmt,WXLoanBal,AcctStatus,DbBtyp,FacilityNo,FacilityRealAmt,FacilityAmt,FacilityProd,AppyAmt,CurrBal,In32107,AppDat,
                                  FstAdvDate,EXPDAT,MaturityDate,CreatedUser,ModifiedUser)
                                  select 
                                  dbo.GenerateNFOSeq(row_number() over(order by LoanRelation,ID,AccountNo)) as NFOSeqCode,
                                  row_number() over(order by LoanRelation,ID,AccountNo) NFOSeqNo,
                                  CaseSource,
                                  Applno,
                                  Applnob,
                                  ID,
                                  AccountNo,
                                  Curr,
                                  WXAcctType,
                                  WXIntCat,
                                  WXAcctTypeName,
                                  WXFstAdvDte,
                                  WXMaturityDate,
                                  rtrim(ltrim(WXApprAmt)) WXApprAmt,
                                  rtrim(ltrim(WXLoanBal)) WXLoanBal,
                                  WXStatus,
                                  [DbBtyp],
                                  FacilityNo,
                                  rtrim(ltrim(FacilityAmt)),
                                  cast(rtrim(ltrim(FacilityAmt)) as numeric(14,0)) FacilityAmt,
                                  FacilityProd,
                                  cast(rtrim(ltrim(WXApprAmt)) as numeric(15,0)) AppyAmt,
                                  cast(rtrim(ltrim(WXLoanBal)) as numeric(19,4)) CurrBal,
                                  In32107,
                                  AppDat,
                                  FstAdvDate,
                                  EXPDAT,
                                  MaturityDate,
                                  @ModifiedUser,
                                  @ModifiedUser
                                  from T3;
  
                                  --檢查67072任一ID是否有14筆
                                  ;with T1 as
                                  (
                                  SELECT 
                                  [NUMSBank67072Detail].ID,count(1) as num
                                  FROM NUMSCustomerInfo 
                                  inner join [NUMSBank67072Detail] on NUMSCustomerInfo.ApplNo = [NUMSBank67072Detail].applno and NUMSCustomerInfo.ApplNob = [NUMSBank67072Detail].applnob 
                                  and [NUMSBank67072Detail].ID = CusID and Module = 'UDWR'
                                  where NUMSCustomerInfo.ApplNo = @ApplNo and NUMSCustomerInfo.ApplNoB = @ApplNoB and NUMSCustomerInfo.[Status] = 'Y'
                                  group by [NUMSBank67072Detail].ID
                                  )
  
                                  update UDWRDetailByCusID set Over14in67072 = case when num >= 14 then 'Y' else 'N' end , ModifiedUser = @ModifiedUser, ModifiedDate = getdate()
                                  from UDWRDetailByCusID
                                  inner join T1 on T1.ID = UDWRDetailByCusID.CusId;
                                  --檢查67072任一ID是否有14筆
  
                                  --檢查67072的FacilityNo在67073是否能找到! 找不到就寫入NewForOldExtra
                                  insert into dbo.UDWRNewForOldExtra
                                  select
                                  T1.Applno,
                                  T1.Applnob,
                                  T1.[ID],
                                  T1.LimNo as FacilityNo,
                                  @ModifiedUser,
                                  getdate(),
                                  @ModifiedUser,
                                  getdate()
                                  from @T1 T1
                                  left join [dbo].[NUMSBank67073Detail] on NUMSBank67073Detail.Applno = T1.Applno 
                                  and NUMSBank67073Detail.Applnob = T1.Applnob and NUMSBank67073Detail.ID = T1.ID
                                  and NUMSBank67073Detail.FacilityNo = '0000' + T1.LimNo and NUMSBank67073Detail.Module = 'UDWR'
                                  where CaseType = 'F' and [NUMSBank67073Detail].[NewID] is null
                                  --檢查67072的FacilityNo在67073是否能找到! 找不到就寫入NewForOldExtra";

            try
            {
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@ApplNo", _applNo));
                base.Parameter.Add(new CommandParameter("@ApplNoB", _applNoB));
                base.Parameter.Add(new CommandParameter("@ModifiedUser", _op));
                base.ExecuteNonQuery(strSQL);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 更新個人會員與實際等級不一致的產品Rate[關卡:2140,只有重查 第一次]
        /// </summary>
        /// <param name="_applNo"></param>
        /// <param name="_applNoB"></param>
        /// <param name="_op"></param>
        public void UDWRCleanProdRate(string _applNo, string _applNoB, string _op)
        {
            try
            {
                string sql = "";
                UDWRBIZ _UDWRBIZ = new UDWRBIZ(GetConnectionString());
                // UDWRDetailInformationVO _info = _UDWRBIZ.GetUDWRDetailInformation(_applNo, _applNoB); //取得案件資訊

                NUMSProductBIZ _np = new NUMSProductBIZ(GetConnectionString());
                NUMSBank60491Master _vo = _np.GetVipDegreeFromBank(_applNo, _applNoB, "UDWR");

                IList<NUMSProduct> _prodList = _UDWRBIZ.CheckVipDegreeCDI(_applNo, _applNoB, _vo.WXVipDegree, _vo.VipCDI); //取得不一致的產品
                if (_prodList.Count > 0)
                {
                    base.Parameter.Clear();
                    int i = 0;
                    foreach (NUMSProduct item in _prodList)
                    {
                        sql += "update UDWRProduct set " + UDWRBIZ.ProdRateCondition() + @" , ModifiedUser = @ModifiedUser, ModifiedDate = getdate() where ApplNo = @ApplNo and ApplNoB = @ApplNoB and SeqNo = @SeqNo" + i + @";";
                        base.Parameter.Add(new CommandParameter("@SeqNo" + i, item.SeqNo));
                        i++;
                    }

                    base.Parameter.Add(new CommandParameter("@ApplNo", _applNo));
                    base.Parameter.Add(new CommandParameter("@ApplNoB", _applNoB));
                    base.Parameter.Add(new CommandParameter("@ModifiedUser", _op));
                    if (!string.IsNullOrEmpty(sql)) { base.ExecuteNonQuery(sql); }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 計算最高核決層級[關卡:2150 重查 重算 第一次]
        /// </summary>
        /// <param name="applNo"></param>
        /// <param name="applNob"></param>
        /// <param name="EmpID"></param>
        public void CalTopCreditLevelRefresh(string applNo, string applNob, string EmpID)
        {
            try
            {
                #region 設定變數
                UDWRAuthInfoVO AuthInfo = null; //審核階段需用到的資訊
                UDWRBIZ _UDWRBIZ = new UDWRBIZ(GetConnectionString());
                base.Parameter.Clear(); //清空容器
                #endregion

                #region 取得必要的資料
                //取得必要的資料
                AuthInfo = _UDWRBIZ.GetAuthInfoForB(applNo, applNob); //取得目前案件審核階段會用到的資訊
                //取得必要的資料
                #endregion

                //回傳最高CO
                NUMSAuthLevel TopCO = _UDWRBIZ.ReturnTopCreditLevel(AuthInfo.ApproveAmt, AuthInfo.AcctApproveAmt, AuthInfo.AuditResult, EmpID);

                if (TopCO == null)
                {
                    return; //離開function 
                }
                string TopCreditLevel = TopCO.AuthLevelCode;
                string sql = @"update UDWRMaster set CalTopCreditLevel = @TopCreditLevel, CalTopCreditLevelSeq = @TopCreditLevelSeq
                                        ,ModifiedUser=@EmpID ,ModifiedDate =getdate()
                                      where ApplNo = @ApplNo and ApplNoB = @ApplNoB;";
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@TopCreditLevel", TopCreditLevel));
                base.Parameter.Add(new CommandParameter("@TopCreditLevelSeq", TopCO.Seq));
                base.Parameter.Add(new CommandParameter("@ApplNo", applNo));
                base.Parameter.Add(new CommandParameter("@ApplNoB", applNob));
                base.Parameter.Add(new CommandParameter("@EmpID", EmpID));

                //回傳最高CO

                base.ExecuteNonQuery(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 計算特審案件(第二邏輯), 基本上會用到最高核准層級某些邏輯
        /// IR-63343 smallzhi added 20131214
        /// 此Method, 必須放再CalTopCreditLevelRefresh前面
        /// </summary>
        public void CalSpecialAuditCaseRefresh(string applNo, string applNob, string EmpID)
        {
            try
            {
                #region 設定變數
                UDWRAuthInfoVO AuthInfo = null; //審核階段需用到的資訊
                UDWRBIZ _UDWRBIZ = new UDWRBIZ(GetConnectionString());
                base.Parameter.Clear(); //清空容器
                #endregion

                #region 取得必要的資料
                //取得必要的資料
                AuthInfo = _UDWRBIZ.GetAuthInfoForB(applNo, applNob); //取得目前案件審核階段會用到的資訊
                //取得必要的資料
                #endregion

                #region 判別特審欄位的值
                if (AuthInfo.SpecialAuditCaseFlag == "Y")
                {
                    return; //如果是Y的話, 就不需要做了!
                }
                #endregion

                //回傳特審案件的值
                string SpecialAuditCaseFlag = _UDWRBIZ.CaculateSpecialAuditCase(applNo, applNob);

                string sql = @"update UDWRMaster set SpecialAuditCaseFlag = @SpecialAuditCaseFlag,ModifiedUser=@EmpID ,ModifiedDate =getdate()
                                      where ApplNo = @ApplNo and ApplNoB = @ApplNoB;";

                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@SpecialAuditCaseFlag", SpecialAuditCaseFlag));
                base.Parameter.Add(new CommandParameter("@ApplNo", applNo));
                base.Parameter.Add(new CommandParameter("@ApplNoB", applNob));
                base.Parameter.Add(new CommandParameter("@EmpID", EmpID));

                //回傳最高CO

                base.ExecuteNonQuery(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 取出需寫入黑名單的案件
        /// </summary>
        /// <returns></returns>
        public DataTable GetNeedToWriteInFraud()
        {
            try
            {

                DataTable returnValue = new DataTable();
                string sql = @"declare @T1 table(ApplNo nvarchar(15),ApplNoB nvarchar(1), ReasonCode nvarchar(10),IWReasonCode varchar(3), IWReasonCodeDesc nvarchar(15),OriNum int,FarundNum int);
                            ;with t1 as
                            (
                            SELECT ApplNo,ApplNoB ,ReasonCode
                            FROM 
                               (select ApplNo,ApplNoB ,ReasonCode1 ,ReasonCode2 ,ReasonCode3
                                from UDWRMaster where AuditResult = 'D' and CloseDate is not null and (WriteInFraud is null or WriteInFraud <> 'Y')
                               ) p
                            UNPIVOT
                               (ReasonCode FOR ReasonCodeList IN 
                                  (ReasonCode1,ReasonCode2,ReasonCode3)
                            )AS unpvt
                            ),t2 as
                            (
                            select ApplNo,ApplNoB, T1.ReasonCode,IWReasonCode,IWReasonCodeDesc
                            from T1
                            inner join RULEADRCode on RULEAdrCode.ReasonCode = T1.ReasonCode and FraudFlag = 'Y' and IWReasonCode is not null
                            ),T3 as
                            (
                            select 
                            T2.ApplNo,T2.ApplNoB
                            from T2
                            inner join NUMSFraudFile on NUMSFraudFile.ApplNo = T2.ApplNo and NUMSFraudFile.ApplNoB = T2.ApplNoB and NUMSFraudFile.ReasonCodeFromUDWR = T2.ReasonCode
                            ),T4 as
                            (
                            select 
                            ApplNo,ApplNoB, ReasonCode,IWReasonCode,IWReasonCodeDesc,
                            (select count(1) from t2 where t2.applno = t.applno and t2.applnob = t.applnob) as OriNum,
                            (select count(1) from t3 where t3.applno = t.applno and t3.applnob = t.applnob) as FarundNum
                            from T2 t
                            )

                            insert into @T1(ApplNo,ApplNoB, ReasonCode,IWReasonCode,IWReasonCodeDesc,OriNum,FarundNum)
                            select ApplNo,ApplNoB, ReasonCode,IWReasonCode,IWReasonCodeDesc,OriNum,FarundNum from T4;

                            --將都處理好的案件 更新狀態
                            update UDWRMaster set WriteInFraud = 'Y'
                            from UDWRMaster
                            inner join @T1 t on t.ApplNo = UDWRMaster.ApplNo and t.ApplNoB = UDWRMaster.ApplNoB
                            where OriNum = FarundNum;

                            --將未處理好的案件 丟出繼續處理
                            select ApplNo,ApplNoB, ReasonCode,IWReasonCode,IWReasonCodeDesc from @T1
                            where OriNum <> FarundNum;";
                base.Parameter.Clear();
                returnValue = base.Search(sql);
                return returnValue;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 寫入黑名單
        /// </summary>
        /// <returns></returns>
        public bool WriteInFraud(string ApplNo, string ApplNoB, string IWReasonCode, string IWReasonCodeDesc, string ReasonCode, string ModifiedUser, out string sno, out string msg)
        {
            // 連接數據庫
            IDbConnection dbConnection = base.OpenConnection();
            IDbTransaction dbTransaction = null;
            msg = "";
            sno = "";
            // DB連接
            using (dbConnection)
            {
                try
                {
                    // 開始事務
                    dbTransaction = dbConnection.BeginTransaction();

                    string sql = @"declare @count int;
                                declare @YM varchar(6);
                                declare @sno int;
                                declare @realsno varchar(10);
                                declare @ApproveDate datetime;
                                declare @ApplEmpID nvarchar(50);
                                declare @ApplApproveDate datetime;

                                select @count = count(1) from NUMSFraudFile where ApplNo = @ApplNo and ApplNoB = @ApplNoB and ReasonCodeFromUDWR = @ReasonCode;
                                select @ApproveDate = CurrentApproveDateTime from UDWRMaster where ApplNo = @ApplNo and ApplNoB = @ApplNoB;
                                select @ApplEmpID = LatestApproveApplEmpNo, @ApplApproveDate = LatestApproveApplDateTime from UDWRDerivedData where ApplNo = @ApplNo and ApplNoB = @ApplNoB;

                                if(@count = 0)
                                begin
	                                set @YM = cast(year(getdate()) as varchar) + Replicate('0',2 - len(cast(month(getdate()) as varchar))) + cast(month(getdate()) as varchar);
	                                select top 1  @sno = right(FraudNo,3) from NUMSFraudFile where left(FraudNo,6) = @YM  order by FraudNo desc;
                                    set @sno = case when (@sno is null or @sno = 0) then 0 else @sno end;
                                    set @sno = @sno + 1;
                                    set @realsno = @YM + '5' + Replicate('0',3 - len(@sno)) + cast(@sno as varchar);

                                    --黑名單Master--
                                    insert into NUMSFraudFile(FraudNo,ApplNo,ApplNoB,ReasonCode,FraudDate,OrgaCode,FraudExpDate,FraudDesc,ApproveStatus,ApproveUser,ApproveDate,CreatedUser,CreatedDate,ModifiedUser,ModifiedDate,[Status],ReasonCodeFromUDWR)
                                    values(@realsno,@ApplNo,@ApplNoB,@IWReasonCode,@ApproveDate,'19',null,@IWReasonCodeDesc,NULL,NULL,NULL,@ApplEmpID,@ApplApproveDate,@ModifiedUser,getdate(),NULL,@ReasonCode);
                                    --黑名單Master--
   
   
                                    --黑名單Detail--
	                                ;with T1 as
	                                (
	                                SELECT ApplNo,ApplNoB,
	                                CusId,CusName,
	                                NowTelArea,NowTelNo,
	                                isnull((select rtrim(AreaName) from dbo.PARMAreaCode where PARMAreaCode.AreaCode = a.ComCity),'') + isnull((select AreaName from  dbo.PARMAreaCode where ParentCode = a.ComCity  and AreaCode = a.ComZip),'') + ComAddress ComAddress,
	                                isnull((select rtrim(AreaName) from dbo.PARMAreaCode where PARMAreaCode.AreaCode = a.RegCity),'') + isnull((select AreaName from  dbo.PARMAreaCode where ParentCode = a.RegCity  and AreaCode = a.RegZip),'') + RegAddress RegAddress, 
	                                isnull((select rtrim(AreaName) from dbo.PARMAreaCode where PARMAreaCode.AreaCode = a.NowCity),'') + isnull((select AreaName from  dbo.PARMAreaCode where ParentCode = a.NowCity  and AreaCode = a.NowZip),'') + NowAddress NowAddress,
	                                isnull((select rtrim(AreaName) from dbo.PARMAreaCode where PARMAreaCode.AreaCode = a.JobCity),'') + isnull((select AreaName from  dbo.PARMAreaCode where ParentCode = a.NowCity  and AreaCode = a.JobZip),'') + JobAddress JobAddress,
	                                JobTelArea,JobTelNo,JobTelExt,
	                                JobUnit,JobNo,MobileTel,isnull(CusName,'') + left(CusId,3) + '****' + rtrim(right(CusId,3)) LimitInfo
	                                FROM NUMSCustomerInfo a
	                                where applno = @ApplNo and applnob = @ApplNoB and LoanRelation = '1' 
	                                ),T2 as
	                                (
	                                select
	                                @realsno as realsno,
	                                codeno,
	                                case codeno
	                                when '01' then (select CusName from T1)
	                                when '02' then (select CusName from T1)
	                                when '03' then (select CusID from T1)
	                                when '04' then (select CusID from T1)
	                                when '05' then (select NowTelArea + '-' + NowTelNo from T1)
	                                when '06' then (select JobUnit from T1)
	                                when '07' then (select JobNo from T1)
	                                when '08' then (select JobTelArea + '-' + JobTelNo + '#' + JobTelExt from T1)
	                                when '09' then ''
	                                when '10' then ''
	                                when '11' then ''
	                                when '12' then (select ComAddress from T1)
	                                when '13' then (select RegAddress from T1)
	                                when '14' then (select NowAddress from T1)
	                                when '15' then (select JobAddress from T1)
	                                when '16' then (select LimitInfo from T1)
	                                when '17' then (select LimitInfo from T1)
	                                when '18' then (select LimitInfo from T1)
	                                when '19' then (select LimitInfo from T1)
	                                when '20' then (select MobileTel from T1)
	                                else '' end codevalue,
	                                @ApplEmpID as CreatedUser,
	                                @ApplApproveDate as CreatedDate,
	                                @ModifiedUser as ModifiedUser,
	                                getdate() as ModifiedDate
	                                from PARMCODE
	                                where CodeType = 'FDITEM'
	                                )

	                                insert into NUMSFraudFileDetl(FraudNo,FraudItemNo,FraudContent,CreatedUser,CreatedDate,ModifiedUser,ModifiedDate)
	                                select realsno,codeno,codevalue,CreatedUser,CreatedDate,ModifiedUser,ModifiedDate from T2
	                                where Codevalue is not null and Codevalue <> '';
                                    --黑名單Detail--
   
                                    select @realsno; --有寫入資料
                                end
                                    select '0'; --沒寫入資料";
                    base.Parameter.Clear();
                    base.Parameter.Add(new CommandParameter("@ApplNo", ApplNo));
                    base.Parameter.Add(new CommandParameter("@ApplNoB", ApplNoB));
                    base.Parameter.Add(new CommandParameter("@IWReasonCode", IWReasonCode));
                    base.Parameter.Add(new CommandParameter("@IWReasonCodeDesc", IWReasonCodeDesc));
                    base.Parameter.Add(new CommandParameter("@ReasonCode", ReasonCode));
                    base.Parameter.Add(new CommandParameter("@ModifiedUser", ModifiedUser));

                    object _r = base.ExecuteScalar(sql, dbTransaction);

                    dbTransaction.Commit();

                    if (_r == null || _r == "0") { return false; } else { sno = _r.ToString(); return true; }
                }
                catch (Exception ex)
                {
                    dbTransaction.Rollback();
                    msg = ex.ToString();
                    return false;
                }
            }
        }

        /// <summary>
        /// 取得黑名單資料
        /// </summary>
        /// <param name="ApplNo"></param>
        /// <param name="ApplNoB"></param>
        /// <param name="sno"></param>
        /// <returns></returns>
        public DataSet GetFraudData(string sno, out string _msg)
        {
            try
            {
                _msg = "";
                DataSet ds = new DataSet();
                string sql = @"select  FraudNo FRAUD_NO,ApplNo APPLNO,ApplNoB APPLNOB,convert(varchar(8),FraudDate,112) FILE_DATE,
                                       ReasonCode REASON,OrgaCode ORGANIZATION,convert(varchar(8),FraudExpDate,112) ENDDATE,FraudDesc REASONDESC,
                                       convert(varchar(8),CreatedDate,112) TAKEDOWN_DATE,'' DELETE_FLAG,'' PRNSTAT,'' VERIFYDATE, 
                                       right(rtrim(CreatedUser),5) TAKEDOWN_EMPNO,'5' FRAUDTYPE,'NUMS' SYS_FLAG
                                from dbo.NUMSFraudFile
                                where FraudNo = @FraudNo;

                                select FraudNo FRAUD_NO,ROW_NUMBER() OVER(PARTITION BY FraudNo ORDER BY FraudItemNo) AS FRAUD_NOB,
                                            FraudItemNo ITEM,left(rtrim(FraudContent),38) CONTENT,'' LASTMATCHDATE,'' LASTMATCHAPPLNO,'' LASTMATCHCOUNT
                                    from NUMSFraudFileDetl 
                                    where FraudNo = @FraudNo;";
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@FraudNo", sno));
                ds = base.SearchToDataSet(sql);
                return ds;
            }
            catch (Exception ex)
            {
                _msg = ex.ToString();
                return null;
            }
        }

        /// <summary>
        /// 寫回更新資料[IW回傳成功後]
        /// </summary>
        /// <returns></returns>
        public void UpdateFraud(string ApplNo, string ApplNoB, string sno, out string msg)
        {
            msg = "";
            try
            {
                string sql = @"declare @ApproveUser nvarchar(20)
                            declare @ApproveDate datetime

                            select @ApproveUser = CurrentApproveEmpNo,@ApproveDate = CurrentApproveDateTime from UDWRMaster where applno = @applno and applnob = @applnob;

                            update dbo.NUMSFraudFile
                            set ApproveStatus = 'Y', ApproveUser = @ApproveUser, ApproveDate = @ApproveDate
                            where FraudNo = @FraudNo";

                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@ApplNo", ApplNo));
                base.Parameter.Add(new CommandParameter("@ApplNoB", ApplNoB));
                base.Parameter.Add(new CommandParameter("@FraudNo", sno));

                base.ExecuteNonQuery(sql);
            }
            catch (Exception ex)
            {
                msg = ex.ToString();
            }
        }

        /// <summary>
        /// 更新UDWRDerivedData For BackGround
        /// </summary>
        /// <param name="ApplNo"></param>
        /// <param name="ApplNoB"></param>
        /// <param name="EmpID"></param>
        public void UDWRDerivedRefreshForBG(string ApplNo, string ApplNoB, string EmpID)
        {
            try
            {
                #region 設定變數
                UDWRBIZ _UDWRBIZ = new UDWRBIZ(GetConnectionString());
                base.Parameter.Clear(); //清空容器
                #endregion

                #region 執行更新的動作
                _UDWRBIZ.RefreshUDWRDerivedData(ApplNo, ApplNoB, EmpID);
                #endregion
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        /// <summary>
        /// 20160106 RefreshForOtherChanel 更新資訊
        /// </summary>
        /// <param name="ApplNo"></param>
        /// <param name="ApplNoB"></param>
        /// <param name="EmpID"></param>
        public void RefreshForOtherChanel(string ApplNo, string ApplNoB, string EmpID)
        {
            string strSQL = @"if (select dbo.[ReturnCaseSource](RouteNo,SalesNo) caseSource from NUMSMaster where applNo = @applNo and applNoB = @applNoB) = '網銀' --如果是網銀 就從暫存區Copy資料到NUMSMaster跟NUMSCustomerInfo
                                begin
	                                UPDATE NUMSMaster
	                                SET 
	                                NUMSMaster.UseTypeContent = NUMSWebBankApplyData.UseTypeContent 
	                                ,NUMSMaster.PayDebtType = NUMSWebBankApplyData.PayDebtType 
	                                ,NUMSMaster.LoanPurpose = NUMSWebBankApplyData.LoanPurpose
	                                ,NUMSMaster.LoanPeriod = NUMSWebBankApplyData.LoanPeriod
	                                ,NUMSMaster.ModifiedUser = @ModifiedUser
	                                ,NUMSMaster.ModifiedDate = getdate()
	                                from NUMSWebBankApplyData
	                                inner join NUMSMaster on NUMSWebBankApplyData.ApplNo = NUMSMaster.ApplNo AND NUMSWebBankApplyData.ApplNoB = NUMSMaster.ApplNoB
	                                WHERE 
	                                NUMSWebBankApplyData.ApplNo = @ApplNo AND NUMSWebBankApplyData.ApplNoB = @ApplNoB
	
	                                UPDATE NUMSCustomerInfo
                                    SET 
                                    NUMSCustomerInfo.CusName = NUMSWebBankApplyData.CusName    
                                   ,NUMSCustomerInfo.NationalityCode = (SELECT TOP 1 CodeNo FROM PARMCODE WHERE CodeType = 'NATN_CODE' AND CodeDesc like '%' + NUMSWebBankApplyData.NationalityCode + '%') 
                                   ,NUMSCustomerInfo.CusEnName = NUMSWebBankApplyData.CusEnName
                                   ,NUMSCustomerInfo.CusSex = NUMSWebBankApplyData.CusSex
                                   ,NUMSCustomerInfo.CTCBCusID = NUMSWebBankApplyData.CTBCCusID
                                   ,NUMSCustomerInfo.MaritalStatus = NUMSWebBankApplyData.MaritalStatus
                                   ,NUMSCustomerInfo.ChildrenPels = NUMSWebBankApplyData.ChildrenPels
                                   ,NUMSCustomerInfo.EducStatus = NUMSWebBankApplyData.EducStatus
                                   ,NUMSCustomerInfo.EmailAddress = NUMSWebBankApplyData.Email
                                   ,NUMSCustomerInfo.NowTelArea = NUMSWebBankApplyData.NowTelArea
                                   ,NUMSCustomerInfo.NowTelNo = NUMSWebBankApplyData.NowTelNo
                                   ,NUMSCustomerInfo.MobileTel = NUMSWebBankApplyData.MobileTel
                                   ,NUMSCustomerInfo.HouseType = NUMSWebBankApplyData.HouseType
                                   ,NUMSCustomerInfo.HouseTime = NUMSWebBankApplyData.HouseTime
                                   ,NUMSCustomerInfo.RegTelArea = NUMSWebBankApplyData.RegTelArea
                                   ,NUMSCustomerInfo.RegTelNo = NUMSWebBankApplyData.RegTelNo
                                   ,NUMSCustomerInfo.RegZip = NUMSWebBankApplyData.RegZip
                                   ,NUMSCustomerInfo.RegCity = (SELECT top 1 [ParentCode] FROM [dbo].[PARMAreaCode] where areacode = NUMSWebBankApplyData.RegZip)
                                   ,NUMSCustomerInfo.RegAddress = NUMSWebBankApplyData.RegAddress
                                   ,NUMSCustomerInfo.ComZip = NUMSWebBankApplyData.ComZip
                                   ,NUMSCustomerInfo.ComCity = (SELECT top 1 [ParentCode] FROM [dbo].[PARMAreaCode] where areacode = NUMSWebBankApplyData.ComZip)
                                   ,NUMSCustomerInfo.ComAddress = NUMSWebBankApplyData.ComAddress
                                   ,NUMSCustomerInfo.LoanNotifyType = NUMSWebBankApplyData.LoanNotifyType
                                   ,NUMSCustomerInfo.JobUnit = NUMSWebBankApplyData.JobUnit
                                   ,NUMSCustomerInfo.JobTelArea = NUMSWebBankApplyData.JobTelArea
                                   ,NUMSCustomerInfo.JobTelNo = NUMSWebBankApplyData.JobTelNo
                                   ,NUMSCustomerInfo.JobZip = NUMSWebBankApplyData.JobZip
                                   ,NUMSCustomerInfo.JobCity = (SELECT top 1 [ParentCode] FROM [dbo].[PARMAreaCode] where areacode = NUMSWebBankApplyData.JobZip)
                                   ,NUMSCustomerInfo.JobAddress = NUMSWebBankApplyData.JobAddress
                                   ,NUMSCustomerInfo.JobTime = NUMSWebBankApplyData.JobTime
                                   ,NUMSCustomerInfo.YearIncome = NUMSWebBankApplyData.YearIncome
                                   ,NUMSCustomerInfo.YearincomeProperty = NUMSWebBankApplyData.YearincomeProperty
                                   ,NUMSCustomerInfo.ModifiedUser = @ModifiedUser
                                   ,NUMSCustomerInfo.ModifiedDate = getdate() 
                                   from NUMSWebBankApplyData
                                   inner join NUMSCustomerInfo on NUMSWebBankApplyData.ApplNo = NUMSCustomerInfo.ApplNo AND NUMSWebBankApplyData.ApplNoB = NUMSCustomerInfo.ApplNoB AND NUMSWebBankApplyData.CusID = NUMSCustomerInfo.CusID
                                   WHERE 
                                   NUMSWebBankApplyData.ApplNo = @ApplNo AND NUMSWebBankApplyData.ApplNoB = @ApplNoB
                                end";
            try
            {
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@ApplNo", ApplNo));
                base.Parameter.Add(new CommandParameter("@ApplNoB", ApplNoB));
                base.Parameter.Add(new CommandParameter("@ModifiedUser", EmpID));
                base.ExecuteNonQuery(strSQL);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public string GetConnectionString()
        {
            string constr = "";
            constr = "Data Source=" + _dataSource + ";Initial Catalog=" + _initCatalog + ";Persist Security Info=True;User ID=" + _userID + ";PASSWORD=" + _passWord + ";";

            return constr;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="strMessage"></param>
        /// <param name="applno"></param>
        /// <param name="applnob"></param>
        /// <remarks>
        /// mark by mel IR-62764 
        /// 請於鑑價BACKGROUND加上Deviation的比對邏輯請呼叫意超的的FUNCTION 
        /// </remarks>
        public void GetAprlDeviation(out string strMessage, string applno, string applnob)
        {
            strMessage = "";
            System.Text.StringBuilder sblog = new System.Text.StringBuilder();
            DataTable tempdt = new DataTable();

            try
            {
                APRLDistinguishMainBIZ _APRLDistinguishMainBIZ = new APRLDistinguishMainBIZ(this.GetConnectionString());

                tempdt = OpenDataTable("APRLGuaranteeMain", applno, applnob, "", "");

                foreach (DataRow _dr in tempdt.Rows)
                {
                    sblog.AppendLine(DateTime.Now.ToString() + "|| ApplNo:" + applno + ",ApplNoB:" + applnob + " : Guarano: " + _dr["GuaraNo"].ToString());
                    _APRLDistinguishMainBIZ.ReAPRLDeviation(_dr["GuaraNo"].ToString(), applno, applnob);
                }

                strMessage = sblog.ToString();

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        /// <summary>
        /// IR-63340 -- 產出一般件的審核紀錄檔
        /// 20140310 新增欄位NunHousing,ProjectItem,IsMultiCO,IsNeedCreditCO,ApprovedApplEmpNo,ApprovedSupervisorEmpNo

        /// </summary>
        /// <returns></returns>
        public DataTable GetNormalCreditData(int year)
        {
            string strSQL = @"  declare @today nvarchar(8);
                                declare @startDate nvarchar(8);

                                set @today = CONVERT(nvarchar, GETDATE(), 112);
                                set @startDate = CONVERT(nvarchar, DateAdd(year,@year,GETDATE()), 112);

                                SELECT CONVERT(varchar, GETDATE(), 111) DataDate
                                      ,a.[SLANo]
                                      ,a.[ApplNo]
                                      ,a.[TopCreditLevel]
                                      ,a.[APRLEmpID]
                                      ,a.[APRLDateTime]
                                      ,a.[AuditResult]
                                      ,a.[DispatchOperatorEmpNo]
                                      ,a.[StartDateTime]
                                      ,a.[EndDateTime]
                                      ,a.[ApproveAmt]
                                      ,a.[ReasonCode1]
                                      ,a.[ReasonCode2]
                                      ,a.[ReasonCode3]
                                      ,a.[ProductDetail1]
                                      ,a.[ProductDetail2]
                                      ,a.[ProductDetail3]
                                      ,a.[ProductDetail4]
                                      ,a.[ProductDetail5]
                                      ,b.NunHousing
                                      ,b.ProjectItem
                                      ,case when b.IsMultiCO = 1 then 'Y' when b.IsMultiCO = 0 then 'N' else '' end IsMultiCO
                                      ,case when b.IsNeedCreditCO = 1 then 'Y' when b.IsNeedCreditCO = 0 then 'N' else '' end IsNeedCreditCO
                                      ,(select top 1 EmpID from dbo.UDWRSLAApprovedDetail where UDWRSLAApprovedDetail.ApplNo = a.ApplNo and UDWRSLAApprovedDetail.ApplNoB = a.ApplNoB and UDWRSLAApprovedDetail.SLANo = a.SLANo and UDWRSLAApprovedDetail.FlowStep = '2320') ApprovedApplEmpNo
                                      ,(select top 1 EmpID from dbo.UDWRSLAApprovedDetail where UDWRSLAApprovedDetail.ApplNo = a.ApplNo and UDWRSLAApprovedDetail.ApplNoB = a.ApplNoB and UDWRSLAApprovedDetail.SLANo = a.SLANo and UDWRSLAApprovedDetail.FlowStep <> '2320' order by CreditSeqNo asc) ApprovedSupervisorEmpNo
                                      ,(select top 1 ApproveDateTime from dbo.UDWRSLAApprovedDetail where UDWRSLAApprovedDetail.ApplNo = a.ApplNo and UDWRSLAApprovedDetail.ApplNoB = a.ApplNoB and UDWRSLAApprovedDetail.SLANo = a.SLANo and UDWRSLAApprovedDetail.FlowStep = '2320') ApprovedApplApproveDateTime
                                      ,(select top 1 ApproveDateTime from dbo.UDWRSLAApprovedDetail where UDWRSLAApprovedDetail.ApplNo = a.ApplNo and UDWRSLAApprovedDetail.ApplNoB = a.ApplNoB and UDWRSLAApprovedDetail.SLANo = a.SLANo and UDWRSLAApprovedDetail.FlowStep <> '2320' order by CreditSeqNo asc) ApprovedSupervisorApproveDateTime
                                  FROM [dbo].[UDWRSLAInfo] a
                                  inner join UDWRMaster b on a.ApplNo = b.ApplNo and a.ApplNoB = b.ApplNoB
                                  where a.ApplNo between @startDate and @today and a.CaseType = @CaseType";

            DataTable returnValue = new DataTable();
            try
            {
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@year", year));
                base.Parameter.Add(new CommandParameter("@CaseType", "N"));
                returnValue = base.Search(strSQL);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return returnValue;
        }

        /// <summary>
        /// IR-63340 -- 產出變簽件的審核紀錄檔
        /// </summary>
        /// <returns></returns>
        public DataTable GetDISPCreditData(int year)
        {
            string strSQL = @"  declare @today nvarchar(8);
                                declare @startDate nvarchar(8);

                                set @today = CONVERT(nvarchar, GETDATE(), 112);
                                set @startDate = CONVERT(nvarchar, DateAdd(year,@year,GETDATE()), 112);

                                SELECT CONVERT(varchar, GETDATE(), 111) DataDate
                                  ,[SLANo]
                                  ,UDWRSLAInfo.[ApplNo]
                                  ,d.OriApplNo
                                  ,[TopCreditLevel]
                                  ,d.DISPSourceType
                                  ,[DispatchOperatorEmpNo]
                                  ,[StartDateTime]
                                  ,[EndDateTime]
                                  ,[AuditResult]
                                  ,[ReasonCode1]
                                  ,[ReasonCode2]
                                  ,[ReasonCode3]
                              FROM [dbo].[UDWRSLAInfo]
                              left join DISPMaster d on d.applno = UDWRSLAInfo.applno and d.applnob = UDWRSLAInfo.applnob
                              where UDWRSLAInfo.ApplNo between @startDate and @today and CaseType = @CaseType";

            DataTable returnValue = new DataTable();
            try
            {
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@year", year));
                base.Parameter.Add(new CommandParameter("@CaseType", "D"));
                returnValue = base.Search(strSQL);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return returnValue;
        }

        /// <summary>
        /// IR-63340 -- 產出退件記錄
        /// </summary>
        /// <returns></returns>
        public DataTable GetReturnData(int year)
        {
            string strSQL = @"  declare @today nvarchar(8);
                                declare @startDate nvarchar(8);

                                set @today = CONVERT(nvarchar, GETDATE(), 112);
                                set @startDate = CONVERT(nvarchar, DateAdd(year,@year,GETDATE()), 112);

                                select 
                                CONVERT(varchar, GETDATE(), 111) DataDate,
                                a.SLANo,
                                a.ApplNo,
                                a.ApproveEmpNo as EmpID,
                                a.ApproveDateTime as ApproveDate,
                                (select top 1 ReasonItem from UDWRRescanTerminateReason where UDWRRescanTerminateReason.applno = a.applno and UDWRRescanTerminateReason.applnob = a.applnob and ReasonKind = '2' and UDWRRescanTerminateReason.CreditSeqNo = a.CreditSeqNo) ReasonItem,
                                a.CreditComment
                                from UDWRMasterHistory a 
                                where a.ApplNo between @startDate and @today and a.AuditResult = @AuditResult";

            DataTable returnValue = new DataTable();
            try
            {
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@year", year));
                base.Parameter.Add(new CommandParameter("@AuditResult", "B"));
                returnValue = base.Search(strSQL);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return returnValue;
        }

        /// <summary>
        /// IR-63340 -- 產出申覆原因記錄
        /// </summary>
        /// <returns></returns>
        public DataTable GetAppealData(int year)
        {
            string strSQL = @"  declare @today nvarchar(8);
                                declare @startDate nvarchar(8);

                                set @today = CONVERT(nvarchar, GETDATE(), 112);
                                set @startDate = CONVERT(nvarchar, DateAdd(year,@year,GETDATE()), 112);

                                select 
                                CONVERT(varchar, GETDATE(), 111) DataDate,
                                a.ApplNo,
                                a.ApplyTime,
                                a.AVEmpNo,
                                a.OldApplNo,
                                (select LatestApproveAuditResult from UDWRDerivedData b where a.OldApplNo = b.applno and a.OldApplNob = b.applnob) AuditResult,
                                a.FirstApplNo,
                                a.ApplyCode,
                                a.ApplyCode2,
                                a.ApplyCode3,
                                a.ApplyReason
                                from NUMSApplyVerify a 
                                where a.ApplNo between @startDate and @today";

            DataTable returnValue = new DataTable();
            try
            {
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@year", year));
                returnValue = base.Search(strSQL);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return returnValue;
        }

        /// <summary>
        /// IR-63340 -- 產出鑑價中止記錄
        /// </summary>
        /// <returns></returns>
        public DataTable GetAPRLTerminatedData(int year)
        {
            string strSQL = @"  declare @today nvarchar(8);
                                declare @startDate nvarchar(8);

                                set @today = CONVERT(nvarchar, GETDATE(), 112);
                                set @startDate = CONVERT(nvarchar, DateAdd(year,@year,GETDATE()), 112);

                                SELECT 
                                CONVERT(varchar, GETDATE(), 111) DataDate,
                                SLANo,
                                ApplNo,
                                GuaraNo,
                                EmpID,
                                AuditResult,
                                ApprovedDateTime,
                                ReasonCode,
                                Comment,
                                SurveyorReason
                                FROM [dbo].UDWRSLAAPRL
                                where ApplNo between @startDate and @today";

            DataTable returnValue = new DataTable();
            try
            {
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@year", year));
                returnValue = base.Search(strSQL);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return returnValue;
        }

        /// <summary>
        /// IR-63340 -- 產出審核明細記錄檔
        /// </summary>
        /// <returns></returns>
        public DataTable GetApprovedData(int year)
        {
            string strSQL = @"  declare @today nvarchar(8);
                                declare @startDate nvarchar(8);

                                set @today = CONVERT(nvarchar, GETDATE(), 112);
                                set @startDate = CONVERT(nvarchar, DateAdd(year,@year,GETDATE()), 112);

                                SELECT 
                                CONVERT(varchar, GETDATE(), 111) DataDate,
                                SLANo,
                                Row_Number() over (partition by applno,applnob order by CreditSeqNo asc) SeqNo,
                                FlowStep,
                                ApplNo,
                                CreditLevel,
                                EmpID,
                                AuditResult,
                                case when FlowStep in ('2620','3620') then DateAdd(s,-1,DateAdd(d,1,ApproveDateTime)) else ApproveDateTime end ApproveDateTime
                                FROM UDWRSLAApprovedDetail
                                where ApplNo between @startDate and @today";

            DataTable returnValue = new DataTable();
            try
            {
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@year", year));
                returnValue = base.Search(strSQL);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return returnValue;
        }

        /// <summary>
        /// IR-63340 -- 產出人員登入登出記錄檔
        /// 20140310 -- 登入只抓登入成功的Record
        /// </summary>
        /// <returns></returns>
        public DataTable GetActivityData()
        {
            string strSQL = @"  declare @today varchar(10);
                                declare @startDate varchar(10);

                                set @today = CONVERT(varchar, GETDATE(), 111);
                                set @startDate = CONVERT(varchar, DateAdd(d,-1,GETDATE()), 111);

                                ;with T1 as
                                (
                                select userid,min(LogID) minLogID from NUMSLog
                                where 
                                [TimeStamp] between @startDate and @today and Title = 'Login' and Result = 'S'
                                group by userid 
                                ),T2 as
                                (
                                select a.userid,max(LogID) maxLogID
                                from NUMSLog a
                                inner join T1 b on a.UserID = b.UserID
                                where a.[TimeStamp] between @startDate and @today
                                group by a.userid 
                                ),T3 as
                                (
                                select 
                                T2.userid,
                                (select EmpName from NUMSEmployee where EmpID = T2.userid) EmpName,
                                (select [TimeStamp] from NUMSLog where LogID = T1.minLogID) LoginTime,
                                (select case when Title = 'Logout' then [TimeStamp] else DateAdd(mi,30,[TimeStamp]) end [TimeStamp] from NUMSLog where LogID = MaxLogID) LastActiveTime
                                from T2
                                inner join T1 on T1.userid = T2.userid
                                )

                                select CONVERT(varchar, GETDATE(), 111) DataDate,userid,EmpName,LoginTime,LastActiveTime from T3;";

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
        /// 20140310 產出中止_補件重啟案件(RestartData)記錄檔
        /// </summary>
        /// <returns></returns>
        public DataTable GetRestartData(int year)
        {
            string strSQL = @"  declare @today nvarchar(8);
                                declare @startDate nvarchar(8);

                                set @today = CONVERT(nvarchar, GETDATE(), 112);
                                set @startDate = CONVERT(nvarchar, DateAdd(year,@year,GETDATE()), 112);

                                ;With T1 as
                                (
                                select UDWRMasterHistory.ApplNo,ApproveDateTime
                                ,(select ReasonItem from (select ReasonItem,Row_Number() over (ORDER BY ReasonItem asc) rownum from dbo.UDWRRescanTerminateReason where UDWRRescanTerminateReason.CreditSeqNo = UDWRMasterHistory.CreditSeqNo and UDWRMasterHistory.ApplNo = UDWRRescanTerminateReason.ApplNo) a where a.rownum = 1) ReasonItem1
                                ,(select ReasonItem from (select ReasonItem,Row_Number() over (ORDER BY ReasonItem asc) rownum from dbo.UDWRRescanTerminateReason where UDWRRescanTerminateReason.CreditSeqNo = UDWRMasterHistory.CreditSeqNo and UDWRMasterHistory.ApplNo = UDWRRescanTerminateReason.ApplNo) a where a.rownum = 2) ReasonItem2
                                ,(select ReasonItem from (select ReasonItem,Row_Number() over (ORDER BY ReasonItem asc) rownum from dbo.UDWRRescanTerminateReason where UDWRRescanTerminateReason.CreditSeqNo = UDWRMasterHistory.CreditSeqNo and UDWRMasterHistory.ApplNo = UDWRRescanTerminateReason.ApplNo) a where a.rownum = 3) ReasonItem3
                                ,(select a.PromotionUnit from NUMSMaster a where a.ApplNo = UDWRMasterHistory.ApplNo and a.ApplNoB = UDWRMasterHistory.ApplNoB) PromotionUnit
                                ,SLANo
                                from dbo.UDWRMasterHistory
                                where ApplNo between @startDate and @today and AuditResult = @AuditResult
                                ), T2 as
                                (
                                select 
                                ApplNo,ApproveDateTime,ReasonItem1,ReasonItem2,ReasonItem3,PromotionUnit,SLANo
                                ,(select top 1 b.BUName from dbo.NUMSBU b where b.PromotionUnit = T1.PromotionUnit) PromotionName
                                from T1
                                )

                                select CONVERT(varchar, GETDATE(), 111) DataDate,T2.* from T2";

            DataTable returnValue = new DataTable();
            try
            {
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@year", year));
                base.Parameter.Add(new CommandParameter("@AuditResult", "R"));
                returnValue = base.Search(strSQL);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return returnValue;
        }

        /// <summary>
        /// 20140310 產出鑑價資訊(APRLInfoData)記錄檔
        /// </summary>
        /// <returns></returns>
        public DataTable GetAPRLInfoData(int year)
        {
            string strSQL = @"  declare @today nvarchar(8);
                                declare @startDate nvarchar(8);

                                set @today = CONVERT(nvarchar, GETDATE(), 112);
                                set @startDate = CONVERT(nvarchar, DateAdd(year,@year,GETDATE()), 112);

                                select 
                                CONVERT(varchar, GETDATE(), 111) DataDate,
                                a.ApplNo,
                                a.GuaraNo, 
                                a.PresumeTotal,
                                a.PriceDiff,
                                a.PhotoUse
                                from APRLDistinguishMain a
                                inner join NUMSMaster b on b.ApplNo between @startDate and @today and a.ApplNo = b.ApplNo and a.ApplNoB = b.ApplNoB";

            DataTable returnValue = new DataTable();
            try
            {
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@year", year));
                returnValue = base.Search(strSQL);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return returnValue;
        }

        /// <summary>
        /// 20140310 產出原因碼資料檔(RULEAdrCodeData)記錄檔
        /// </summary>
        /// <returns></returns>
        public DataTable GetRULEAdrCodeData()
        {
            string strSQL = @"  select 
                                CONVERT(varchar, GETDATE(), 111) DataDate,
                                *
                                from RULEAdrCode";

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
        /// 20140310 產出參數資料檔(PARMCodeData)記錄檔
        /// </summary>
        /// <returns></returns>
        public DataTable GetPARMCodeData()
        {
            string strSQL = @"  select 
                                CONVERT(varchar, GETDATE(), 111) DataDate
                                ,[CodeUid]
                                ,[CodeType]
                                ,[CodeTypeDesc]
                                ,[CodeNo]
                                ,[CodeDesc]
                                ,[SortOrder]
                                ,[CodeTag]
                                ,[CodeMemo]
                                ,Case when [Enable] = 1 then 'Y' when [Enable] = 0 then 'N' else '' end [Enable]
                                ,[CreatedUser]
                                ,[CreatedDate]
                                ,[ModifiedUser]
                                ,[ModifiedDate]
                                ,[BANCSCode]
                                from PARMCode";

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
        /// 20140507 產出工時(CPCTTime)記錄檔
        /// </summary>
        /// <returns></returns>
        /// 20140603 add [ApproveEmpID] [ApproveDate]
        public DataTable GetCPCTTimeData(int year)
        {
            string strSQL = @"  declare @today datetime;
                                declare @startDate datetime;
  
                                set @today = CONVERT(varchar, GETDATE(), 111);
                                set @startDate = CONVERT(varchar, DateAdd(year,@year,GETDATE()), 111);

                                  select 
                                  CONVERT(varchar, GETDATE(), 111) DataDate,
                                  case when WorkingDate is not null then CONVERT(varchar, WorkingDate, 111) else '' end WorkingDate,
                                  EmpID,
                                  PlusReduceCode,
                                  WorkingTime,
                                  ReasonCode,
                                  StatusCode,
                                  ApproveEmpID,
                                  ApproveDate
                                  from [dbo].[CPCTTime]
                                  where WorkingDate between @startDate and @today;";

            DataTable returnValue = new DataTable();
            try
            {
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@year", year));
                returnValue = base.Search(strSQL);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return returnValue;
        }

        /// <summary>
        /// 20140507 產出點數(CPCTPoint)記錄檔
        /// </summary>
        /// <returns></returns>
        /// 20140603 add [ApproveEmpID] [ApproveDate]
        public DataTable GetCPCTPointData(int year)
        {
            string strSQL = @"    declare @today datetime;
                                  declare @startDate datetime;
  
                                  set @today = CONVERT(varchar, GETDATE(), 111);
                                  set @startDate = CONVERT(varchar, DateAdd(year,@year,GETDATE()), 111);

                                SELECT   
	                                  CONVERT(varchar, GETDATE(), 111) DataDate,
	                                  case when WorkingDate is not null then CONVERT(varchar, WorkingDate, 111) else '' end WorkingDate
                                      ,[EmpID]
                                      ,[CodeTypeNo]
                                      ,[ReasonCode]
                                      ,[WorkingCases]
                                      ,[WorkingPoints]
                                      ,[StatusCode]
                                      ,ApproveEmpID
                                      ,ApproveDate
                                  FROM [dbo].[CPCTPoint]
                                  where WorkingDate between @startDate and @today;";

            DataTable returnValue = new DataTable();
            try
            {
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@year", year));
                returnValue = base.Search(strSQL);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return returnValue;
        }

        /// <summary>
        /// 20140507 產出假日檔(PARMWorkingDay)記錄檔
        /// </summary>
        /// <returns></returns>
        public DataTable GetPARMWorkingDayData()
        {
            string strSQL = @"SELECT
	                          CONVERT(varchar, GETDATE(), 111) DataDate 
                              ,case when [Date] is not null then CONVERT(varchar, [Date], 111) else '' end [Date]
                              ,case when [Flag] = 1 then 'Y' else 'N' end Flag
                          FROM [dbo].[PARMWorkingDay];";

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
        /// 取出需回寫2代的徵信案件
        /// 20140127
        /// </summary>
        /// <returns></returns>
        public IList<DISPToCSCVO> GetNeedToUpdateCSC()
        {
            try
            {
                string sql = @";with T1 as
                            (
                            select a.applno,a.applnob,CSCNo,
                            (select top 1 CreditSeqNo from UDWRApproveHistory c where c.applno = a.applno and c.applnob = a.applnob order by CreditSeqNo desc) SupervisorCreditSeqNo,
                            (select top 1 CreditSeqNo from UDWRApproveHistory d where d.applno = a.applno and d.applnob = a.applnob and d.FlowStep = '3310') ApplCreditSeqNo
                            from dbo.DISPMaster a
                            inner join UDWRDerivedData b on a.applno = b.applno and a.applnob = b.applnob and isClose = 'Y'
                            where DISPSourceType = '3' and UDWRToCSC = '0'
                            ),T2 as
                            (
                            select 
                            T1.applno,
                            T1.applnob,
                            T1.CSCNo CSC_No,
                            substring(isnull((select CodeDesc from PARMCode where CodeType = 'UDWR_AuditResult' and CodeNo = b.AuditResult),''),1,5) ApprvResult,
                            substring(isnull(b.CreditComment,''),1,240) ApprvNote,
                            b.ApproveDateTime ApprvDate,
                            substring(isnull(c.CreditComment,''),1,240) InvestigateNote,
                            substring(isnull((select EmpName from NUMSEmployee where EmpID = c.ApproveEmpNo),''),1,10) InvestigateName,
                            (select DispatchOperatorDateTime from UDWRMaster e where e.applno = T1.applno and e.applnob = T1.applnob) InvestigateDate
                            from T1
                            left join UDWRMasterHistory b on T1.applno = b.applno and T1.applnob = b.applnob and T1.SupervisorCreditSeqNo = b.CreditSeqNo
                            left join UDWRMasterHistory c on T1.applno = c.applno and T1.applnob = c.applnob and T1.ApplCreditSeqNo = c.CreditSeqNo
                            )

                            select * from T2;";
                base.Parameter.Clear();
                return base.SearchList<DISPToCSCVO>(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 成功寫回CSC的資料  要更新Flag
        /// 20140127
        /// </summary>
        /// <returns></returns>
        /// 20140218 smallzhi 多增加更新ModifiedDate跟ModifiedUser
        public bool UpdateFlagToDISP(string ApplNo, string ApplNoB, string code, string User, out string msg)
        {
            try
            {
                string sql = @"update DISPMaster set UDWRToCSC = @Code,ModifiedDate = Getdate(), ModifiedUser = @User where ApplNo = @ApplNo and ApplNoB = @ApplNoB;";
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@ApplNo", ApplNo));
                base.Parameter.Add(new CommandParameter("@ApplNoB", ApplNoB));
                base.Parameter.Add(new CommandParameter("@Code", code));
                base.Parameter.Add(new CommandParameter("@User", User));

                base.ExecuteNonQuery(sql);
                msg = "";
                return true;
            }
            catch (Exception ex)
            {
                msg = ex.ToString();
                return false;
            }
        }
        /// <summary>
        /// 20161024 產生PCSM_JCU6記錄檔
        /// </summary>
        /// <returns></returns>
        public DataTable GetJCU6InfoData()
        {

            string strSQL = @"SELECT *
                              FROM PCSM_JCU6_LOG  WHERE QUERY_DATE >= DATEADD(YEAR,-2,GETDATE());";
            //modify by shenqixing 20161129 end            
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
        // PCSM-ML5 add by shenqixing 20161208 start
        /// <summary>
        ///  產生PCSM_JML5記錄檔
        /// </summary>
        /// <returns></returns>
        public DataTable GetJML5InfoData()
        {

            string strSQL = @"SELECT [NewId]
                                    ,[ALIAS]
                                    ,[SIGNATURE]
                                    ,[APPLNO]
                                    ,[ApplNoB]
                                    ,[COLLATERALNO]
                                    ,[CREFLAG]
                                    ,[MLPRODTYPE]
                                    ,[LASTBANKLOANPURPOSE]
                                    ,[NATIONALITY]
                                    ,[RESIDENCESTATUS]
                                    ,[SELFUSEFLAG]
                                    ,[INVESTORFLAG]
                                    ,[PAYMENTTYPE]
                                    ,[COLLATERALLOCATION]
                                    ,[AUMGROUP]
                                    ,[LTVTYPE]
                                    ,[CBCONTROLFLAG]
                                    ,[CBHIGHAPPRASIAL]
                                    ,[DBRCAP]
                                    ,[CURDBR]
                                    ,[COLLATERALZIP]
                                    ,[PUBLICSERVANTFLAG]
                                    ,[SPECIALCOLLATERALTYPE]
                                    ,[MISTRACKING]
                                    ,[COLLATERALPRICE]
                                    ,[COLLATERALAPPRASIAL]
                                    ,[DURATIONMONTH]
                                    ,[PROJECTCODE]
                                    ,[HOUSETYPE]
                                    ,[RESIDUALDURATIONMONTH]
                                    ,[RESIDUALDBR]
                                    ,[DURATIONRATIO]
                                    ,[M502POLICYCONTROL_LEAFNODEID]
                                    ,[M502POLICYCONTROL_OUTCOMENAME]
                                    ,[M502POLICYCONTROL_TESTGROUPSETNAME]
                                    ,[M502POLICYCONTROL_TESTGROUPNAME]
                                    ,[M502POLICYCONTROL_VSNODEID]
                                    ,[M502POLICYCONTROL_VSOUTCOMENAME]
                                    ,[M502POLICYCONTROL_LTVCAP]
                                    ,[M503CBCONTROL_LEAFNODEID]
                                    ,[M503CBCONTROL_OUTCOMENAME]
                                    ,[M503CBCONTROL_LTVCAP]
                                    ,[M504INVESTORCONTROL_LEAFNODEID]
                                    ,[M504INVESTORCONTROL_OUTCOMENAME]
                                    ,[M504INVESTORCONTROL_LTVCAP]
                                    ,[M505HIGHHOUSINGVALUECONTROL_LEAFNODEID]
                                    ,[M505HIGHHOUSINGVALUECONTROL_OUTCOMENAME]
                                    ,[M505HIGHHOUSINGVALUECONTROL_LTVCAP]
                                    ,[M506DATADERIVATION_RESULTINT1]
                                    ,[M506DATADERIVATION_RESULTINT2]
                                    ,[M506DATADERIVATION_RESULTINT3]
                                    ,[M506DATADERIVATION_RESULTDECIMAL1]
                                    ,[M506DATADERIVATION_RESULTDECIMAL2]
                                    ,[M506DATADERIVATION_RESULTDECIMAL3]
                                    ,[M506DATADERIVATION_RESULTSTRING1]
                                    ,[M506DATADERIVATION_RESULTSTRING2]
                                    ,[M506DATADERIVATION_RESULTSTRING3]
                                    ,[FINALLTVCAP]
                                    ,[NEW]
                                    ,[QUERY_DATE]
                                    ,[SRCAPPLNO]
                                    ,[SRCAPPLNOB]
                                    ,[StepID]
                                    ,[UserID]
                                    ,[INCOMEFORCUS]
                                    ,[PROJECTCHANNEL]
                                    ,[AGEFORREVERSE]
                              FROM PCSM_JML5_LOG  WHERE QUERY_DATE >= DATEADD(YEAR,-2,GETDATE());";

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
        //PCSM-ML5 add by shenqixing 20161208 end

        /// <summary>
        /// 20140701 產出段小段資料檔(PARMSector)記錄檔
        /// </summary>
        /// <returns></returns>
        public DataTable GetPARMSectorData()
        {
            string strSQL = @"  select 
                                *
                                from PARMSector";

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
        /// 20141010 smallzhi 產出HPI Log記錄檔
        /// </summary>
        /// <returns></returns>
        public DataTable GetHPILogData(int year)
        {
            string strSQL = @"  declare @today nvarchar(8);
                                declare @startDate nvarchar(8);

                                set @today = CONVERT(nvarchar, GETDATE(), 112);
                                set @startDate = CONVERT(nvarchar, DateAdd(year,@year,GETDATE()), 112);  
                              
                              SELECT 
                                   CONVERT(varchar, GETDATE(), 111) DataDate
                                  ,[ApplNo]
                                  ,[ApplNoB]
                                  ,[GuaraNo]
                                  ,[Pre_App_Unit_Value]
                              FROM [dbo].[HPI_Log]
                              where ApplNo between @startDate and @today";

            DataTable returnValue = new DataTable();
            try
            {
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@year", year));
                returnValue = base.Search(strSQL);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return returnValue;
        }

        /// <summary>
        /// 20141016 smallzhi 產出NUMSNormalData記錄檔
        /// 20141208 add NunHousing
        /// </summary>
        /// <returns></returns>
        public DataTable GetNUMSNormalData(int year)
        {
            string strSQL = @"  declare @today nvarchar(8);
                                declare @startDate nvarchar(8);

                                set @today = CONVERT(nvarchar, GETDATE(), 112);
                                set @startDate = CONVERT(nvarchar, DateAdd(year,@year,GETDATE()), 112);

                                select
                                CONVERT(varchar, GETDATE(), 111) DataDate
                                ,NUMSMaster.[ApplNo]
                                ,NUMSMaster.[ApplNoB]
                                ,HouseTime
                                ,MaritalStatus
                                ,NunHousing
                                from dbo.NUMSMaster
                                inner join dbo.NUMSCustomerInfo on NUMSMaster.ApplNo = NUMSCustomerInfo.ApplNo and NUMSMaster.ApplNoB = NUMSCustomerInfo.ApplNoB and NUMSCustomerInfo.LoanRelation = N'1'
                                where NUMSMaster.ApplNo between @startDate and @today and ApplTypeCode <> 'J'";

            DataTable returnValue = new DataTable();
            try
            {
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@year", year));
                returnValue = base.Search(strSQL);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return returnValue;
        }

        /// <summary>
        /// 20141016 smallzhi 產出HPI的資料
        /// </summary>
        /// <returns></returns>
        public DataTable GetHPIData()
        {
            string strSQL = @"     	;with T1 as --找出第一筆擔保品
                                    (
                                    select 
                                    UDWRMaster.ApplNo,
                                    UDWRMaster.ApplNoB,
                                    CloseDate,
                                    LatestApproveAuditResult,
                                    dmid
                                    from UDWRMaster
                                    inner join dbo.UDWRDerivedData on UDWRMaster.ApplNo = UDWRDerivedData.ApplNo and UDWRMaster.ApplNoB = UDWRDerivedData.ApplNoB and isClose = 'Y'
                                    inner join APRLDistinguishMain on APRLDistinguishMain.ApplNo = UDWRMaster.ApplNo and APRLDistinguishMain.ApplNoB = UDWRMaster.ApplNoB
                                    left join HPI_Log on UDWRMaster.applno = HPI_Log.applno and UDWRMaster.applnoB = HPI_Log.applnoB and APRLDistinguishMain.GuaraNo = HPI_Log.GuaraNo
                                    where HPI_Log.Applno is null
                                    ),T2 as --串出擔保品的資訊(一)
                                    (
                                    select
                                    T1.ApplNo, --Application_Nbr	案件編號
                                    T1.ApplNoB,
                                    GaranName,--Collateral_Name	擔保品案名
                                    GuaraNo,--Collateral_Nbr	擔保品編號
                                    (select count(1) from APRLDistinguishReason where APRLDistinguishReason.DMID = T1.DmId and drType = 'RISK_ITEM') RISKNUM, --V_High_Abnormal 個數
                                    (select count(1) from APRLDistinguishReason where APRLDistinguishReason.DMID = T1.DmId and drType = 'NEG_ITEM') NEGNUM, --V_Negative_Code 個數
                                    HouseType, --House_Type_Code	房屋種類
                                    IsPackage, --Loan_Package_Flag	是否為整批房貸
                                    PresumeUnit * 10000 as PresumeUnit, --Presume_Unit_Price	認定單價(萬元/坪)*10000???
                                    RefPrice * 10000 as RefPrice, --Ref_Unit_Price	買賣每坪參考單價(萬元/坪)*10000??
                                    TotalStallPrice * 10000 as TotalStallPrice, --Stall_Appraise_Price	車位總價(萬元)*10000??
                                    Surroundings, --Surrounding_Code	周圍情形
                                    TotalPrice, --Total_Current_Price	總時價(元) 
                                    CloseDate, --Final_Verify_Result_Date	案件終結日
                                    LatestApproveAuditResult,--Final_Verify_Result_Code	RSLT
                                    isnull((select sum(TotalStallMeasure) from APRLGuaranteeBuilding where APRLGuaranteeBuilding.ApplNo = T1.ApplNo and APRLGuaranteeBuilding.ApplNoB = T1.ApplNoB and APRLGuaranteeBuilding.GuaraNo = APRLDistinguishMain.GuaraNo and StallInclude = 'Y'),0) StallMeasure, --Stall_In_Area	面積內含車位_建物檔???
                                    (select count(1) from APRLGuaranteeBuildingDetail where Gbid in (select GbId from APRLGuaranteeBuilding where APRLGuaranteeBuilding.ApplNo = T1.ApplNo and APRLGuaranteeBuilding.ApplNoB = T1.ApplNoB and APRLGuaranteeBuilding.GuaraNo = APRLDistinguishMain.GuaraNo)) Detail_Cnt, --Detail_Cnt	明細檔資料筆數
                                    (select count(1) from APRLGuaranteeBuildingDetail where Gbid in (select GbId from APRLGuaranteeBuilding where APRLGuaranteeBuilding.ApplNo = T1.ApplNo and APRLGuaranteeBuilding.ApplNoB = T1.ApplNoB and APRLGuaranteeBuilding.GuaraNo = APRLDistinguishMain.GuaraNo) and purpose = '01') Main_Build_cnt, --Main_Build_cnt	主建物個數_明細檔
                                    isnull((select sum(DetailMeasure2) from APRLGuaranteeBuildingDetail where Gbid in (select GbId from APRLGuaranteeBuilding where APRLGuaranteeBuilding.ApplNo = T1.ApplNo and APRLGuaranteeBuilding.ApplNoB = T1.ApplNoB and APRLGuaranteeBuilding.GuaraNo = APRLDistinguishMain.GuaraNo) and purpose = '01'),0) Main_Build_Own_Area, --Main_Build_Own_Area	主建物持份坪數_明細檔
                                    isnull((select sum(DetailMeasure2) from APRLGuaranteeBuildingDetail where Gbid in (select GbId from APRLGuaranteeBuilding where APRLGuaranteeBuilding.ApplNo = T1.ApplNo and APRLGuaranteeBuilding.ApplNoB = T1.ApplNoB and APRLGuaranteeBuilding.GuaraNo = APRLDistinguishMain.GuaraNo) and purpose = '05'),0) Terrace_Own_Area, --Terrace_Own_Area	露台持份坪數_明細檔
                                    isnull((select sum(DetailMeasure2) from APRLGuaranteeBuildingDetail where Gbid in (select GbId from APRLGuaranteeBuilding where APRLGuaranteeBuilding.ApplNo = T1.ApplNo and APRLGuaranteeBuilding.ApplNoB = T1.ApplNoB and APRLGuaranteeBuilding.GuaraNo = APRLDistinguishMain.GuaraNo) and purpose = '07'),0) Basement_Own_Area, --Basement_Own_Area	地下室持份坪數_明細檔
                                    (select count(1) from APRLGuaranteeBuildingDetail where Gbid in (select GbId from APRLGuaranteeBuilding where APRLGuaranteeBuilding.ApplNo = T1.ApplNo and APRLGuaranteeBuilding.ApplNoB = T1.ApplNoB and APRLGuaranteeBuilding.GuaraNo = APRLDistinguishMain.GuaraNo) and purpose = '08') Public_Own_Cnt, --Public_Own_Cnt 	公設個數_明細檔
                                    isnull((select sum(DetailMeasure2) from APRLGuaranteeBuildingDetail where Gbid in (select GbId from APRLGuaranteeBuilding where APRLGuaranteeBuilding.ApplNo = T1.ApplNo and APRLGuaranteeBuilding.ApplNoB = T1.ApplNoB and APRLGuaranteeBuilding.GuaraNo = APRLDistinguishMain.GuaraNo) and purpose = '08'),0) Public_Own_Area, --Public_Own_Area	公設持份坪數_明細檔
                                    isnull((select sum(DetailMeasure2) from APRLGuaranteeBuildingDetail where Gbid in (select GbId from APRLGuaranteeBuilding where APRLGuaranteeBuilding.ApplNo = T1.ApplNo and APRLGuaranteeBuilding.ApplNoB = T1.ApplNoB and APRLGuaranteeBuilding.GuaraNo = APRLDistinguishMain.GuaraNo)),0) Build_Total_Area, --Build_Total_Area	建物總坪數_明細檔
                                    '' BulAge, --Building_Age	Building_Age
                                    (select BulFinishDate from APRLGuaranteeMain where APRLGuaranteeMain.ApplNo = T1.ApplNo and APRLGuaranteeMain.ApplNoB = T1.ApplNoB and APRLGuaranteeMain.GuaraNo = APRLDistinguishMain.GuaraNo) BulFinishDate, --Building_Finished_Date	Building_Finished_Date
                                    (select RealtyArea from APRLGuaranteeMain where APRLGuaranteeMain.ApplNo = T1.ApplNo and APRLGuaranteeMain.ApplNoB = T1.ApplNoB and APRLGuaranteeMain.GuaraNo = APRLDistinguishMain.GuaraNo) Property_Location_Code,--Property_Location_Code	Property_Location_Code
                                    isnull((select sum(RightMeasure2) from APRLGuaranteeLand where APRLGuaranteeLand.ApplNo = T1.ApplNo and APRLGuaranteeLand.ApplNoB = T1.ApplNoB and APRLGuaranteeLand.GuaraNo = APRLDistinguishMain.GuaraNo),0) RightMeasure2, --Right_Measure2	Right_Measure2 
                                    isnull((select top 1 a.AreaNo from APRLGuaranteeLand a left join dbo.APRLGuaranteeBuilding b on a.ApplNo = b.ApplNo and a.ApplNoB = b.ApplNoB and a.GuaraNo = b.GuaraNo and TotalMeasure1 > 0 where a.ApplNo = T1.ApplNo and a.ApplNoB = T1.ApplNoB and a.GuaraNo = APRLDistinguishMain.GuaraNo order by a.Sector asc),(select top 1 a.AreaNo from APRLGuaranteeLand a where a.ApplNo = T1.ApplNo and a.ApplNoB = T1.ApplNoB and a.GuaraNo = APRLDistinguishMain.GuaraNo order by a.Sector asc)) AreaNo, --Area_Nbr	Sector1_code 
                                    isnull((select top 1 a.Sector from APRLGuaranteeLand a left join dbo.APRLGuaranteeBuilding b on a.ApplNo = b.ApplNo and a.ApplNoB = b.ApplNoB and a.GuaraNo = b.GuaraNo and TotalMeasure1 > 0 where a.ApplNo = T1.ApplNo and a.ApplNoB = T1.ApplNoB and a.GuaraNo = APRLDistinguishMain.GuaraNo order by a.Sector asc),(select top 1 a.Sector from APRLGuaranteeLand a where a.ApplNo = T1.ApplNo and a.ApplNoB = T1.ApplNoB and a.GuaraNo = APRLDistinguishMain.GuaraNo order by a.Sector asc)) Sector, --Sector_Nbr	Sector2
                                    getdate() as_of_date --as_of_date	as_of_date
                                    from T1
                                    inner join APRLDistinguishMain on APRLDistinguishMain.Dmid = T1.Dmid
                                    )
                                    ,T3 as --串出擔保品的資訊
                                    (
                                    select
                                    T2.ApplNo, --Application_Nbr	案件編號
                                    T2.ApplNoB,
                                    GaranName,--Collateral_Name	擔保品案名
                                    GuaraNo,--Collateral_Nbr	擔保品編號
                                    RISKNUM, --V_High_Abnormal 個數
                                    NEGNUM, --V_Negative_Code 個數
                                    HouseType, --House_Type_Code	房屋種類
                                    IsPackage, --Loan_Package_Flag	是否為整批房貸
                                    PresumeUnit, --Presume_Unit_Price	認定單價(萬元/坪)*10000???
                                    RefPrice, --Ref_Unit_Price	買賣每坪參考單價(萬元/坪)*10000??
                                    TotalStallPrice, --Stall_Appraise_Price	車位總價(萬元)*10000??
                                    Surroundings, --Surrounding_Code	周圍情形
                                    TotalPrice, --Total_Current_Price	總時價(元) 
                                    CloseDate, --Final_Verify_Result_Date	案件終結日
                                    LatestApproveAuditResult,--Final_Verify_Result_Code	RSLT
                                    StallMeasure, --Stall_In_Area	面積內含車位_建物檔???
                                    Detail_Cnt, --Detail_Cnt	明細檔資料筆數
                                    Main_Build_cnt, --Main_Build_cnt	主建物個數_明細檔
                                    Main_Build_Own_Area, --Main_Build_Own_Area	主建物持份坪數_明細檔
                                    Terrace_Own_Area, --Terrace_Own_Area	露台持份坪數_明細檔
                                    Basement_Own_Area, --Basement_Own_Area	地下室持份坪數_明細檔
                                    Public_Own_Cnt, --Public_Own_Cnt 	公設個數_明細檔
                                    Public_Own_Area, --Public_Own_Area	公設持份坪數_明細檔
                                    Build_Total_Area, --Build_Total_Area	建物總坪數_明細檔
                                    '' BulAge, --Building_Age	Building_Age
                                    BulFinishDate, --Building_Finished_Date	Building_Finished_Date
                                    Property_Location_Code,--Property_Location_Code	Property_Location_Code
                                    RightMeasure2, --Right_Measure2	Right_Measure2 
                                    AreaNo, --Area_Nbr	Sector1_code 
                                    Sector, --Sector_Nbr	Sector2
                                    as_of_date --as_of_date	as_of_date
                                    from T2
                                    )

                                    select * from T3";

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
        /// 20141016 新增HPI_Log
        /// </summary>
        /// <param name="ApplNo"></param>
        /// <param name="GuaraNo"></param>
        /// <param name="Pre_App_Unit_Value"></param>
        public void InsertHPI_Log(string ApplNo, string GuaraNo, string Pre_App_Unit_Value)
        {
            try
            {
                base.Parameter.Clear();
                #region SQL
                Pre_App_Unit_Value = Pre_App_Unit_Value.Trim(); //先去空白

                decimal _o = 0;
                bool _numcheck = false;
                if (!string.IsNullOrEmpty(Pre_App_Unit_Value) && Decimal.TryParse(Pre_App_Unit_Value, out _o)) //先看是否有空白 並且看是不是數字
                {
                    _o = DataTypeHelper.Floor(_o, 4); //無條件捨去到第四位
                    _numcheck = true;
                }

                decimal? _Pre_App_Unit_Value = null;
                if (_numcheck) { _Pre_App_Unit_Value = _o; }

                ApplNo = ApplNo.Trim();
                GuaraNo = GuaraNo.Trim();
                string sql = @" INSERT INTO HPI_Log
                                (ApplNo
                                ,ApplNoB
                                ,GuaraNo
                                ,Pre_App_Unit_Value
                                ,CreatedUser
                                ,CreatedDate
                                ,ModifiedUser
                                ,ModifiedDate)
                            VALUES
                                (@ApplNo
                                ,0
                                ,@GuaraNo
                                ,@Pre_App_Unit_Value
                                ,'SYS'
                                ,getdate()
                                ,'SYS'
                                ,getdate())";

                base.Parameter.Add(new CommandParameter("@ApplNo", ApplNo));
                base.Parameter.Add(new CommandParameter("@GuaraNo", GuaraNo));
                base.Parameter.Add(new CommandParameter("@Pre_App_Unit_Value", _Pre_App_Unit_Value));

                base.ExecuteNonQuery(sql.ToString());

                #endregion
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        /// <summary>
        /// 20141207 透過NUMSUtil 來取得NUMSProduct的資料
        /// 20140112
        /// </summary>
        /// <param name="ApplNo"></param>
        /// <param name="ApplNoB"></param>
        /// <param name="Source"></param>
        /// <returns></returns>
        public string GetPeriodMax(string ApplNo, string ApplNoB, string Source)
        {
            try
            {
                NUMSProductBIZ _npz = new NUMSProductBIZ(GetConnectionString()); //20141205
                return _npz.GetPeriodMax(ApplNo, ApplNoB, Source); //20141205
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 20150604 產出進件檢核表資訊
        /// </summary>
        /// <returns></returns>
        public DataTable GetNUMSCaseReceivedContent(int year)
        {
            string strSQL = @"    declare @today nvarchar(8);
                                  declare @startDate nvarchar(8);
  
                                  set @today = CONVERT(varchar, GETDATE(), 112);
                                  set @startDate = CONVERT(varchar, DateAdd(year,@year,GETDATE()), 112);

                                    ;with T1 as --取得兩年內結案的案件, 結案包含1.核准與婉拒結案的案件 2.落入中止補件的案件(主管已Approved)
                                    (
                                    select ApplNo,ApplNoB
                                    from dbo.UDWRDerivedData
                                    where ApplNo between @startDate and @today and isClose = 'Y'
                                    union all
                                    select UDWRDerivedData.ApplNo,UDWRDerivedData.ApplNoB
                                    from dbo.UDWRDerivedData
                                    inner join NUMSMaster on UDWRDerivedData.ApplNo = NUMSMaster.ApplNo and UDWRDerivedData.ApplNoB = NUMSMaster.ApplNoB and FlowStep = N'2330'
                                    where UDWRDerivedData.ApplNo between @startDate and @today and isClose = 'N'
                                    ),T2 as --串資料
                                    (
                                    select 
                                    SLANo,
                                    T1.ApplNo,
                                    T1.ApplNoB,
                                    (select GroupName FROM [dbo].[PARMCaseReceivedCheckGroup] where GroupID = Group1ID) Group1Name,
                                    (select GroupName FROM [dbo].[PARMCaseReceivedCheckGroup] where GroupID = Group2ID) Group2Name,
                                    ItemNo,
                                    case when ItemType = '1' then  '一般缺漏項目' when ItemType = '2' then  '檢核項目' else '' end ItemType,
                                    case when ItemCateGory = '1' then '必要' when ItemCateGory = '2' then '次要' else '' end ItemCateGory,
                                    ItemName,
                                    ItemMemo,
                                    ItemRequired,
                                    Case when ContentType = 1 then 'LFA' when ContentType = 2 then 'LFA主管' when ContentType = 3 then '徵信/鑑價' else '' end ContentType,
                                    NUMSCaseReceivedCheckContentFirstRecord.ModifiedUser,
                                    Case when ItemResult = 'Y' then '有' when ItemResult = 'N' then '無' else '' end ItemResult,
                                    NUMSCaseReceivedCheckContentFirstRecord.ModifiedDate
                                    from T1
                                    inner join dbo.NUMSCaseReceivedCheckContentFirstRecord on T1.ApplNo = NUMSCaseReceivedCheckContentFirstRecord.ApplNo and T1.ApplNoB = NUMSCaseReceivedCheckContentFirstRecord.ApplNoB
                                    inner join UDWRMaster on T1.ApplNo = UDWRMaster.ApplNo and T1.ApplNoB = UDWRMaster.ApplNoB
                                    inner join PARMCaseReceivedCheckItem on PARMCaseReceivedCheckItem.SerialNo = ItemSerialNo
                                    )

                                    select CONVERT(varchar, GETDATE(), 111) DataDate,* from T2;";

            DataTable returnValue = new DataTable();
            try
            {
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@year", year));
                returnValue = base.Search(strSQL);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return returnValue;
        }

        /// <summary>
        /// 20151128 產出LFA組織資訊
        /// </summary>
        /// <returns></returns>
        public DataTable GetLFAINFO()
        {
            string strSQL = @";with T1 as --先找出需要給網銀的科或組  
                                (
                                select BUID,BUName, BUBOSS SupervisorEmpNo
                                from NUMSBU
                                where BUID in (select distinct BUID from NUMSBUtoAreaCode)
                                ),T2 as --找出符合此組織的經辦(可作0100跟初審鍵檔的LFA) 
                                (
                                select distinct NUMSBUToEmployee.BUID,BUName,NUMSBUToEmployee.EmpID LFAEmpNo,SupervisorEmpNo
                                from dbo.NUMSBUToEmployee
                                inner join T1 on NUMSBUToEmployee.BUID = T1.BUID
                                inner join dbo.NUMSEmployeeToRole on NUMSEmployeeToRole.EmpID = NUMSBUToEmployee.EmpID and RoleID in (select CodeNo from PARMCODE where CodeType = 'WebBankRole')
                                ),T3 as --串出每個員工的資料
                                (
                                select
                                BUID
                                ,BUName
                                ,LFA.EmpName LFAName
                                ,LFAEmpNo
                                ,LFA.EmpEMail LFAEmail
                                ,Supervisor.EmpName SupervisorName
                                ,SupervisorEmpNo
                                ,Supervisor.EmpEMail SupervisorEmail
                                from T2
                                left JOIN dbo.NUMSEmployee LFA ON T2.LFAEmpNo = LFA.EmpID
                                left JOIN dbo.NUMSEmployee Supervisor ON T2.SupervisorEmpNo = Supervisor.EmpID
                                )

                                select * from T3 order by BUID";

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
        /// 20151128 產出擔保品組織資訊
        /// </summary>
        /// <returns></returns>
        public DataTable GetGuaraBUMapping()
        {
            string strSQL = @"SELECT [AreaCode]
                                     ,(SELECT TOP 1 AreaName FROM dbo.PARMAreaCode WHERE PARMAreaCode.AreaCode = NUMSBUToAreaCode.AreaCode) AreaName
                                     ,[BUID]
                                     ,(SELECT TOP 1 BUName FROM NUMSBU WHERE NUMSBU.BUID = NUMSBUToAreaCode.BUID) BUName
                                     FROM [dbo].[NUMSBUToAreaCode]";

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
        #endregion

        #region For整批案件 20141105
        /// <summary>
        /// 處理整批案件 For 額度控管 20141104
        /// 20141116 DueDate加 + '23:59:59.000'  以避免ApproveDate跟ContractDate有時分秒 還是可以判斷的到
        /// CaseType = "ApplNo" 用案件來比對, CaseType = "BulkCase" 直接用整批編號來比對
        /// </summary>
        public void UDWRBulkCaseForLimitProcess(string ApplNo, string ApplNoB, string CaseType = "ApplNo", string BulkCaseNo = "")
        {
            try
            {
                base.Parameter.Clear();
                System.Text.StringBuilder _sql = new System.Text.StringBuilder();
                #region SQL
                if (CaseType == "ApplNo")
                {
                    _sql.AppendLine(" delete from UDWRBulkCaseInfo where applNo = @ApplNo and ApplNoB = @ApplNoB --先刪除 ");
                    _sql.AppendLine(" delete from UDWRBulkCaseForLimit where applNo = @ApplNo and ApplNoB = @ApplNoB --先刪除 ");
                }

                if (CaseType == "BulkCase")
                {
                    _sql.AppendLine(" delete from UDWRBulkCaseForLimit where applNo = @ApplNo and ApplNoB = @ApplNoB and BulkCaseNo = @BulkCaseNo --先刪除 ");
                }

                if (CaseType == "ApplNo")
                {
                    _sql.Append(@"  update NUMSMaster set UDWRBulkProcessDate = CONVERT(char(10), getdate(),111) where applNo = @ApplNo and ApplNoB = @ApplNoB -- 先Update 整批比對的處理日

                                    ;with T1 as --先找出此案的資訊(案名 鄉鎮行政區)
                                    (
                                    select 
                                    distinct
                                    a.applNo,
                                    a.ApplNoB,
                                    a.GaranName, --案名
                                    a.GuaraNo, --擔保品編號
                                    b.AreaNo --鄉鎮行政區
                                    from dbo.APRLDistinguishMain a
                                    left join dbo.APRLGuaranteeBuilding b on b.ApplNo = a.applno and b.ApplNob = a.applnob and b.GuaraNo = a.GuaraNo --會有多筆 反正就都取出來
                                    where a.applNo = @ApplNo and a.ApplNoB = @ApplNoB
                                    ), T2 as --取出案件的申請日跟契約日
                                    (
                                    select
                                    T1.ApplNo,
                                    T1.ApplNoB,
                                    T1.GaranName,
                                    T1.GuaraNo,
                                    T1.AreaNo,
                                    ApplyDate, --申請日
                                    ContractDate, --契約日
                                    UDWRBulkProcessDate --整批比對處理日
                                    from T1
                                    inner join NUMSMaster on T1.ApplNo = NUMSMaster.ApplNo and T1.ApplNoB = NUMSMaster.ApplNoB
                                    ), T3 as --比對整批編號(日期區間 = 1. 申請日或契約日任一符合整批區間, 2.處理日符合整批區間)
                                    (
                                    select
                                    BulkCaseNo,
                                    T2.GuaraNo,
                                    T2.GaranName,
                                    T2.AreaNo,
                                    a.MaxCreditLimit,
                                    a.AproveDate,
                                    a.DueDate
                                    from UDWRBulkCaseMaster a
                                    inner join T2 on a.BulkCaseName = T2.GaranName --案名
                                    and a.BulkCaseZip = T2.AreaNo --行政區
                                    and ((T2.ApplyDate between a.AproveDate and a.DueDate + '23:59:59.000') or (T2.ContractDate between a.AproveDate and a.DueDate + '23:59:59.000') or (UDWRBulkProcessDate between a.AproveDate and a.DueDate + '23:59:59.000')) --申請書填寫日跟申請日任一符合, 即符合 20140918 smallzhi
                                    )

                                    insert into UDWRBulkCaseInfo
                                    select @ApplNo,@ApplNoB,GuaraNo,AreaNo,GaranName,BulkCaseNo,MaxCreditLimit,AproveDate,DueDate,getdate(),'SYS',getdate(),'SYS' from T3 ");
                }

                if (CaseType == "ApplNo")
                {
                    _sql.Append(@";with T1 as --找出此案的整批資訊
                                (
                                select
                                UDWRBulkCaseMaster.BulkCaseName,BulkCaseZip,UDWRBulkCaseMaster.BulkCaseNo
                                from UDWRBulkCaseInfo
                                inner join UDWRBulkCaseMaster on UDWRBulkCaseInfo.BulkCaseNo = UDWRBulkCaseMaster.BulkCaseNo
                                where applNo = @ApplNo and ApplNoB = @ApplNoB
                                )");
                }
                else
                {
                    _sql.Append(@";with T1 as --找出整批編號的資訊
                                (
                                select
                                UDWRBulkCaseMaster.BulkCaseName,BulkCaseZip,UDWRBulkCaseMaster.BulkCaseNo
                                from UDWRBulkCaseMaster where BulkCaseNo = @BulkCaseNo
                                )");
                }

                _sql.Append(@",T2 as --取出跟整批一樣的案名跟行政區的案件,直接用比對出來的整批來用
                                (
                                select 
                                a.ApplNo,a.ApplNoB,a.GaranName,b.AreaNo
                                ,ApplyDate --申請日
                                ,ContractDate --契約日
                                ,LatestApproveSupervisorDateTime --最後核決日
                                ,isClose --結案Flag
                                ,UDWRBulkProcessDate --整批比對處理日
                                from dbo.APRLDistinguishMain a
                                inner join T1 on a.GaranName = BulkCaseName
                                inner join APRLGuaranteeBuilding b on b.ApplNo = a.applno and b.ApplNob = a.applnob and b.GuaraNo = a.GuaraNo and b.AreaNo = T1.BulkCaseZip
                                inner join NUMSMaster on a.ApplNo = NUMSMaster.ApplNo and a.ApplNoB = NUMSMaster.ApplNoB
                                left join UDWRDerivedData WITH (NOLOCK) on a.ApplNo = UDWRDerivedData.ApplNo and a.ApplNoB = UDWRDerivedData.ApplNoB
                                ), T3 as --找出跟本案一樣的整批編號的案件, 比對整批編號(日期區間 = 1. 申請日或契約日任一符合整批區間, 2.處理日符合整批區間)
                                (
                                select
                                distinct
                                ApplNo,
                                ApplNoB,
                                a.BulkCaseNo,
                                '1' caseType --利用整批編號比對到的案件
                                from UDWRBulkCaseMaster a
                                inner join T2 on a.BulkCaseName = T2.GaranName --案名
                                and a.BulkCaseZip = T2.AreaNo --行政區
                                and ((T2.ApplyDate between a.AproveDate and a.DueDate + '23:59:59.000') or (T2.ContractDate between a.AproveDate and a.DueDate + '23:59:59.000') or (T2.UDWRBulkProcessDate between a.AproveDate and a.DueDate + '23:59:59.000')) --申請書填寫日跟申請日任一符合, 即符合 20140918 smallzhi
                                inner join T1 on T1.BulkCaseNo = a.BulkCaseNo
                                ),T4 as --找出其他案的最後核准日(結案), 此時間是否有在整批的核准日往前三個月內  (核准件算入額度，婉拒件計入報表資訊)
                                (
                                select
                                distinct
                                ApplNo,
                                ApplNoB,
                                a.BulkCaseNo,
                                '2' caseType --利用整批編號比對到的案件
                                from UDWRBulkCaseMaster a
                                inner join T2 on a.BulkCaseName = T2.GaranName --案名
                                and a.BulkCaseZip = T2.AreaNo --行政區
                                and T2.isClose = 'Y'
                                and T2.LatestApproveSupervisorDateTime between DateAdd(month,-3,a.AproveDate) + 1 and a.AproveDate + '23:59:59.000'  --日期區間
                                inner join T1 on T1.BulkCaseNo = a.BulkCaseNo
                                ), T5 as --合併兩者的資料
                                (
                                select * from T3
                                union all
                                select * from T4
                                union all
                                select ApplNo,ApplNoB,BulkCaseNo,'3' from UDWRBulkCaseInfo where ApplNo = @ApplNo and ApplNoB = @ApplNoB --也要將自己放進來
                                ), T6 as --整理資料
                                (
                                select
                                row_number() over (partition by ApplNo,ApplNoB,BulkCaseNo order by casetype asc) rownum--須驗證
                                ,*
                                from T5
                                )

                                insert into UDWRBulkCaseForLimit --再新增
                                select @ApplNo,@ApplNoB,BulkCaseNo,CaseType,ApplNo,ApplNoB,getdate(),'SYS',getdate(),'SYS' from T6 where rownum = 1");

                //                string sql = @" delete from UDWRBulkCaseInfo where applNo = @ApplNo and ApplNoB = @ApplNoB --先刪除
                //
                //                                delete from UDWRBulkCaseForLimit where applNo = @ApplNo and ApplNoB = @ApplNoB --先刪除
                //
                //                                update NUMSMaster set UDWRBulkProcessDate = CONVERT(char(10), getdate(),111) where applNo = @ApplNo and ApplNoB = @ApplNoB -- 先Update 整批比對的處理日
                //
                //                                ;with T1 as --先找出此案的資訊(案名 鄉鎮行政區)
                //                                (
                //                                select 
                //                                distinct
                //                                a.applNo,
                //                                a.ApplNoB,
                //                                a.GaranName, --案名
                //                                a.GuaraNo, --擔保品編號
                //                                b.AreaNo --鄉鎮行政區
                //                                from dbo.APRLDistinguishMain a
                //                                left join dbo.APRLGuaranteeBuilding b on b.ApplNo = a.applno and b.ApplNob = a.applnob and b.GuaraNo = a.GuaraNo --會有多筆 反正就都取出來
                //                                where a.applNo = @ApplNo and a.ApplNoB = @ApplNoB
                //                                ), T2 as --取出案件的申請日跟契約日
                //                                (
                //                                select
                //                                T1.ApplNo,
                //                                T1.ApplNoB,
                //                                T1.GaranName,
                //                                T1.GuaraNo,
                //                                T1.AreaNo,
                //                                ApplyDate, --申請日
                //                                ContractDate, --契約日
                //                                UDWRBulkProcessDate --整批比對處理日
                //                                from T1
                //                                inner join NUMSMaster on T1.ApplNo = NUMSMaster.ApplNo and T1.ApplNoB = NUMSMaster.ApplNoB
                //                                ), T3 as --比對整批編號(日期區間 = 1. 申請日或契約日任一符合整批區間, 2.處理日符合整批區間)
                //                                (
                //                                select
                //                                BulkCaseNo,
                //                                T2.GuaraNo,
                //                                T2.GaranName,
                //                                T2.AreaNo,
                //                                a.MaxCreditLimit,
                //                                a.AproveDate,
                //                                a.DueDate
                //                                from UDWRBulkCaseMaster a
                //                                inner join T2 on a.BulkCaseName = T2.GaranName --案名
                //                                and a.BulkCaseZip = T2.AreaNo --行政區
                //                                and ((T2.ApplyDate between a.AproveDate and a.DueDate + '23:59:59.000') or (T2.ContractDate between a.AproveDate and a.DueDate + '23:59:59.000') or (UDWRBulkProcessDate between a.AproveDate and a.DueDate + '23:59:59.000')) --申請書填寫日跟申請日任一符合, 即符合 20140918 smallzhi
                //                                )
                //
                //                                insert into UDWRBulkCaseInfo
                //                                select @ApplNo,@ApplNoB,GuaraNo,AreaNo,GaranName,BulkCaseNo,MaxCreditLimit,AproveDate,DueDate,getdate(),'SYS',getdate(),'SYS' from T3
                //
                //
                //                                ;with T1 as --找出此案的整批資訊
                //                                (
                //                                select
                //                                UDWRBulkCaseMaster.BulkCaseName,BulkCaseZip,UDWRBulkCaseMaster.BulkCaseNo
                //                                from UDWRBulkCaseInfo
                //                                inner join UDWRBulkCaseMaster on UDWRBulkCaseInfo.BulkCaseNo = UDWRBulkCaseMaster.BulkCaseNo
                //                                where applNo = @ApplNo and ApplNoB = @ApplNoB
                //                                ),T2 as --取出跟本案一樣的案名跟行政區的案件,直接用比對出來的整批來用
                //                                (
                //                                select 
                //                                a.ApplNo,a.ApplNoB,a.GaranName,b.AreaNo
                //                                ,ApplyDate --申請日
                //                                ,ContractDate --契約日
                //                                ,LatestApproveSupervisorDateTime --最後核決日
                //                                ,isClose --結案Flag
                //                                ,UDWRBulkProcessDate --整批比對處理日
                //                                from dbo.APRLDistinguishMain a
                //                                inner join T1 on a.GaranName = BulkCaseName
                //                                inner join APRLGuaranteeBuilding b on b.ApplNo = a.applno and b.ApplNob = a.applnob and b.GuaraNo = a.GuaraNo and b.AreaNo = T1.BulkCaseZip
                //                                inner join NUMSMaster on a.ApplNo = NUMSMaster.ApplNo and a.ApplNoB = NUMSMaster.ApplNoB
                //                                left join UDWRDerivedData WITH (NOLOCK) on a.ApplNo = UDWRDerivedData.ApplNo and a.ApplNoB = UDWRDerivedData.ApplNoB
                //                                ), T3 as --找出跟本案一樣的整批編號的案件, 比對整批編號(日期區間 = 1. 申請日或契約日任一符合整批區間, 2.處理日符合整批區間)
                //                                (
                //                                select
                //                                distinct
                //                                ApplNo,
                //                                ApplNoB,
                //                                a.BulkCaseNo,
                //                                '1' caseType --利用整批編號比對到的案件
                //                                from UDWRBulkCaseMaster a
                //                                inner join T2 on a.BulkCaseName = T2.GaranName --案名
                //                                and a.BulkCaseZip = T2.AreaNo --行政區
                //                                and ((T2.ApplyDate between a.AproveDate and a.DueDate + '23:59:59.000') or (T2.ContractDate between a.AproveDate and a.DueDate + '23:59:59.000') or (T2.UDWRBulkProcessDate between a.AproveDate and a.DueDate + '23:59:59.000')) --申請書填寫日跟申請日任一符合, 即符合 20140918 smallzhi
                //                                inner join T1 on T1.BulkCaseNo = a.BulkCaseNo
                //                                ),T4 as --找出其他案的最後核准日(結案), 此時間是否有在整批的核准日往前三個月內  (核准件算入額度，婉拒件計入報表資訊)
                //                                (
                //                                select
                //                                distinct
                //                                ApplNo,
                //                                ApplNoB,
                //                                a.BulkCaseNo,
                //                                '2' caseType --利用整批編號比對到的案件
                //                                from UDWRBulkCaseMaster a
                //                                inner join T2 on a.BulkCaseName = T2.GaranName --案名
                //                                and a.BulkCaseZip = T2.AreaNo --行政區
                //                                and T2.isClose = 'Y'
                //                                and T2.LatestApproveSupervisorDateTime between DateAdd(month,-3,a.AproveDate) + 1 and a.AproveDate + '23:59:59.000'  --日期區間
                //                                inner join T1 on T1.BulkCaseNo = a.BulkCaseNo
                //                                ), T5 as --合併兩者的資料
                //                                (
                //                                select * from T3
                //                                union all
                //                                select * from T4
                //                                union all
                //                                select ApplNo,ApplNoB,BulkCaseNo,'3' from UDWRBulkCaseInfo where ApplNo = @ApplNo and ApplNoB = @ApplNoB --也要將自己放進來
                //                                ), T6 as --整理資料
                //                                (
                //                                select
                //                                row_number() over (partition by ApplNo,ApplNoB,BulkCaseNo order by casetype asc) rownum--須驗證
                //                                ,*
                //                                from T5
                //                                )
                //
                //                                insert into UDWRBulkCaseForLimit --再新增
                //                                select @ApplNo,@ApplNoB,BulkCaseNo,CaseType,ApplNo,ApplNoB,getdate(),'SYS',getdate(),'SYS' from T6 where rownum = 1";

                base.Parameter.Add(new CommandParameter("@ApplNo", (CaseType == "BulkCase") ? "BulkCaseTrigger" : ApplNo));
                base.Parameter.Add(new CommandParameter("@ApplNoB", (CaseType == "BulkCase") ? "0" : ApplNoB));
                if (CaseType == "BulkCase")
                {
                    base.Parameter.Add(new CommandParameter("@BulkCaseNo", BulkCaseNo));
                }

                base.ExecuteNonQuery(_sql.ToString());
                #endregion
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        /// <summary>
        /// 取得比對後的整批編號資訊 20141104
        /// </summary>
        /// <param name="ApplNo"></param>
        /// <param name="ApplNoB"></param>
        /// <returns></returns>
        public DataTable GetMappingBulkCase(string ApplNo, string ApplNoB)
        {
            string strSQL = @";with T1 --先找出此案有哪些整批編號
                                as (
                                SELECT distinct BulkCaseNo,GuaraNo
                                  FROM [UDWRBulkCaseInfo]
                                  where ApplNo = @ApplNo and ApplNoB = @ApplNoB
                                  ), T2 --串出整批編號的資訊
                                  as(
                                  select 
                                  UDWRBulkCaseDetail.*,UDWRBulkCaseMaster.MaxCreditLimit,UDWRBulkCaseMaster.BulkCaseZip,UDWRBulkCaseMaster.BulkCaseName,GuaraNo
                                  from T1 
                                  inner join dbo.UDWRBulkCaseMaster on T1.BulkCaseNo = UDWRBulkCaseMaster.BulkCaseNo
                                  inner join dbo.UDWRBulkCaseDetail on T1.BulkCaseNo = UDWRBulkCaseDetail.BulkCaseNo
                                  )


                                  select * from T2";

            DataTable returnValue = new DataTable();
            try
            {
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@ApplNo", ApplNo));
                base.Parameter.Add(new CommandParameter("@ApplNoB", ApplNoB));
                returnValue = base.Search(strSQL);
            }

            catch (Exception ex)
            {
                throw ex;
            }
            return returnValue;
        }

        /// <summary>
        /// 取得UDWRMaster的資料 20141104
        /// </summary>
        /// <param name="ApplNo"></param>
        /// <param name="ApplNoB"></param>
        /// <returns></returns>
        public DataTable GetUDWRMasterData(string ApplNo, string ApplNoB)
        {
            string strSQL = @"select distinct 
                                a.GuaraNo
                                ,CusGrade,isnull(Investor,'N') as InvestorsFlag
                                from APRLGuaranteeMain a 
                                left join UDWRMaster on UDWRMaster.ApplNo = a.ApplNo and UDWRMaster.ApplNoB = a.ApplNoB
                                where a.ApplNo = @ApplNo and  a.ApplNoB = @ApplNoB";

            DataTable returnValue = new DataTable();
            try
            {
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@ApplNo", ApplNo));
                base.Parameter.Add(new CommandParameter("@ApplNoB", ApplNoB));
                returnValue = base.Search(strSQL);
            }

            catch (Exception ex)
            {
                throw ex;
            }
            return returnValue;
        }

        /// <summary>
        /// 取得主建物的資料 20141104
        /// </summary>
        /// <param name="ApplNo"></param>
        /// <param name="ApplNoB"></param>
        /// <returns></returns>
        public DataTable GetAPRLHouseData(string ApplNo, string ApplNoB)
        {
            string strSQL = @"select distinct 
                                a.GuaraNo
                                ,b.HouseType
                                from APRLGuaranteeMain a 
                                left join APRLDistinguishMain b on a.ApplNo = b.Applno and  a.ApplNoB = b.ApplnoB and a.GuaraNo = b.GuaraNo
                                where a.ApplNo = @ApplNo and  a.ApplNoB = @ApplNoB";

            DataTable returnValue = new DataTable();
            try
            {
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@ApplNo", ApplNo));
                base.Parameter.Add(new CommandParameter("@ApplNoB", ApplNoB));
                returnValue = base.Search(strSQL);
            }

            catch (Exception ex)
            {
                throw ex;
            }
            return returnValue;
        }

        /// <summary>
        /// 取得車位的資料 20141104
        /// </summary>
        /// <param name="ApplNo"></param>
        /// <param name="ApplNoB"></param>
        /// <returns></returns>
        public DataTable GetAPRLStallData(string ApplNo, string ApplNoB)
        {
            string strSQL = @"select 
                                a.GuaraNo
                                ,d.StallType                            
                                from APRLGuaranteeMain a 
                                left join APRLDistinguishMain b on a.ApplNo = b.Applno and  a.ApplNoB = b.ApplnoB and a.GuaraNo = b.GuaraNo
                                left join APRLGuaranteeBuilding c on a.ApplNo = c.Applno and  a.ApplNoB = c.ApplnoB and a.GuaraNo = c.GuaraNo
                                left join APRLGuaranteeStallDetail d on c.GbId = d.GbId                         
                                where a.ApplNo = @ApplNo and  a.ApplNoB = @ApplNoB";

            DataTable returnValue = new DataTable();
            try
            {
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@ApplNo", ApplNo));
                base.Parameter.Add(new CommandParameter("@ApplNoB", ApplNoB));
                returnValue = base.Search(strSQL);
            }

            catch (Exception ex)
            {
                throw ex;
            }
            return returnValue;
        }
        #endregion

        #region NameChecking 更新客戶咨詢 20160201
        /// <summary>
        /// 查詢check的案件列表
        /// </summary>
        /// <returns></returns>
        /// 20151208 XiulianLiu 查詢check的案件列表
        public DataTable GetNameCheckingList(string ApplNo, string ApplNoB)
        {
            string sql = @"select NUMSMaster.newid ,
			NUMSMaster.ApplNo, 
			NUMSMaster.ApplNoB,
			LoanRelation,
			PromotionUser LFAName,
			CusID,
			(case when ForeignFlag ='N' then '' else  CusEnName end)CusEnName,--20160519 edit by 小朱 判斷是否為外國人  
			CusName,
			CusBirth,
			NationalityCode,
			(select [CurrentProcessingApplEmpNo] from UDWRDerivedData where ApplNo = NUMSMaster.applno and ApplNoB = NUMSMaster.ApplNoB) ApplEmpNo
			from NUMSMaster
			inner join NUMSCustomerInfo on NUMSMaster.ApplNo = NUMSCustomerInfo.ApplNo and NUMSMaster.ApplNoB = NUMSCustomerInfo.ApplNoB and status = N'Y'
			where NUMSMaster.ApplNo = @ApplNo and NUMSMaster.ApplNoB = @ApplNoB";
            // 清除參數
            base.Parameter.Clear();
            base.Parameter.Add(new CommandParameter("@ApplNo", ApplNo));
            base.Parameter.Add(new CommandParameter("@ApplNoB", ApplNoB));
            return base.Search(sql);
        }

        /// <summary>
        /// 20160201
        /// </summary>
        /// <param name="applNo"></param>
        /// <param name="applNoB"></param>
        /// <param name="amlMatchedResult"></param>
        /// <param name="amlRCScore"></param>
        /// <param name="amlReferenceNumber"></param>
        /// <param name="dbTransaction"></param>
        /// <returns></returns>
        public void UpdateCusAML(string applNo, string applNoB, string CusID, string amlMatchedResult, string amlRCScore, string amlReferenceNumber, string AMLESBReturnCode)
        {
            string sql = "Update UDWRDetailByCusId  set AMLMatchedResult = @AMLMatchedResult, AMLRCScore = @AMLRCScore, AMLReferenceNumber = @AMLReferenceNumber,AMLESBReturnCode = @AMLESBReturnCode, Punish = @AMLMatchedResult, PEP = @AMLMatchedResult, NN = @AMLMatchedResult,ModifiedDate = getdate(), ModifiedUser ='UpdateCusAML' where ApplNo = @ApplNo and ApplNoB = @ApplNoB and CusID = @CusID";
            base.Parameter.Clear();
            base.Parameter.Add(new CommandParameter("@AMLMatchedResult", amlMatchedResult));
            base.Parameter.Add(new CommandParameter("@AMLRCScore", amlRCScore));
            base.Parameter.Add(new CommandParameter("@AMLReferenceNumber", amlReferenceNumber));
            base.Parameter.Add(new CommandParameter("@ApplNo", applNo));
            base.Parameter.Add(new CommandParameter("@ApplNoB", applNoB));
            base.Parameter.Add(new CommandParameter("@CusID", CusID));
            base.Parameter.Add(new CommandParameter("@AMLESBReturnCode", AMLESBReturnCode));
            base.ExecuteNonQuery(sql);
        }

        #region RASCNameChecking 更新客戶咨詢 20160204

        /// <summary>
        /// 查詢check的案件列表
        /// </summary>
        /// <returns></returns>
        public DataTable RASCGetNameCheckingList(string MaxQryTime)
        {

            string strYear = Convert.ToInt16(DateTime.Now.Year - 1911).ToString("000");
            string strMonth = Convert.ToInt16(DateTime.Now.Month).ToString("00");
            string strDay = Convert.ToInt16(DateTime.Now.Day).ToString("00");
            string strHour = System.DateTime.Now.ToString("HHmmssfff");
            string strSno = "NUMS" + strYear.Substring(1, 2) + strMonth + strDay + strHour;

            string sql = @";
             with  T1 as (
            Select ApplNo,ApplNoB from RASCNameCheckSend 
            where QueryStatus=5  and SendTimes <= @MaxQryTime)
            ,T2 as(
            select 
            @strSno newid,
            T1.ApplNo, 
            T1.ApplNoB,
            C.LoanRelation,
            '' LFAName,
            C.CusId CusID,
            C.CusEnName CusEnName,
            C.CusName CusName,
            C.CusBirth CusBirth,
            C.NationalityCode NationalityCode,
            C.CreatedUser ApplEmpNo
            from 
            RASCCustomer C  
            left join 
            T1 on C.ApplNo=T1.ApplNo and C.ApplNoB= T1.ApplNoB
            where C.ApplNo=T1.ApplNo and C.ApplNoB= T1.ApplNoB
            UNION ALL  
            select 
            @strSno newid,
            T1.ApplNo, 
            T1.ApplNoB,
            '1' LoanRelation,
            '' LFAName,
            C.BinId CusID,
            '' CusEnName, -- 20160519 edit by 小朱
            C.BinName CusName,
            C.OpenDate CusBirth,
            'TW' NationalityCode, -- 20160519 edit by 小朱
            C.CreatedUser ApplEmpNo
            from 
            RASCCompany  C  
            left join 
            T1 on C.ApplNo=T1.ApplNo and C.ApplNoB= T1.ApplNoB
            where C.ApplNo=T1.ApplNo and C.ApplNoB= T1.ApplNoB
            )
            select * from T2 order by ApplNo,ApplNoB";
            // 清除參數
            base.Parameter.Clear();
            base.Parameter.Add(new CommandParameter("@MaxQryTime", MaxQryTime));
            base.Parameter.Add(new CommandParameter("@strSno", strSno));
            return base.Search(sql);
        }

        /// <summary>
        /// AU付買AML
        /// </summary>
        /// <param name="applNo"></param>
        /// <param name="applNoB"></param>
        /// <param name="amlMatchedResult"></param>
        /// <param name="amlRCScore"></param>
        /// <param name="amlReferenceNumber"></param>
        /// <param name="dbTransaction"></param>
        /// <returns></returns>
        public void RASCUpdateCusAML(string applNo, string applNoB, string CusID, string amlMatchedResult, string sRspCode, string MaxQryTime)
        {
            string sql = null;
            //查詢是否已存在RASCNameCheckResult
            string RASCNameCheckResult = SelectRASCNameCheckResult(applNo, applNoB);

            if (applNo == RASCNameCheckResult)
            {
                if (amlMatchedResult != "")
                {
                    sql = @"  update RASCCustomer
                                  set NameCheckResult=@AMLMatchedResult,
                                      ModifiedDate = getdate(), 
                                      ModifiedUser ='UpdateCusAML'
                                      where ApplNo=@ApplNo and ApplNoB=@ApplNoB and CusId=@CusID
                              
                                    update RASCCompany 
                                    set NameCheckResult=@AMLMatchedResult,
                                        ModifiedDate = getdate(), 
                                        ModifiedUser ='UpdateCusAML'
                                        where ApplNo=@ApplNo and ApplNoB=@ApplNoB and BinId=@CusID

                                    update RASCNameCheckResult
                                    set NameCheckResult=@AMLMatchedResult,
                                        ModifiedDate = getdate(),
                                        ModifiedUser ='UpdateCusAML'
                                        where ApplNo=@ApplNo and ApplNoB=@ApplNoB 

                                    update RASCNameCheckSend
                                    set  QueryStatus='0',
                                            SendTimes='0',
                                            ReturnCode=@sRspCode,
                                            ModifiedDate = getdate(),
                                            ModifiedUser ='UpdateCusAML' 
                                            where ApplNo = @ApplNo and ApplNoB = @ApplNoB";
                }
                else
                {
                    sql = "update RASCNameCheckSend  set QueryStatus='3',SendTimes='0',ModifiedDate = getdate(), ModifiedUser ='UpdateCusAML' where ApplNo = @ApplNo and ApplNoB = @ApplNoB";
                }

                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@AMLMatchedResult", amlMatchedResult));
                base.Parameter.Add(new CommandParameter("@MaxQryTime", MaxQryTime));
                base.Parameter.Add(new CommandParameter("@ApplNo", applNo));
                base.Parameter.Add(new CommandParameter("@ApplNoB", applNoB));
                base.Parameter.Add(new CommandParameter("@CusID", CusID));
                base.Parameter.Add(new CommandParameter("@sRspCode", sRspCode));
                base.ExecuteNonQuery(sql);


            }
            else
            {
                if (amlMatchedResult != "")
                {

                    sql = @"  update RASCCustomer
                                  set NameCheckResult=@AMLMatchedResult,
                                      ModifiedDate = getdate(), 
                                      ModifiedUser ='UpdateCusAML'
                                      where ApplNo=@ApplNo and ApplNoB=@ApplNoB and CusId=@CusID
                              
                                  update RASCCompany 
                                  set NameCheckResult=@AMLMatchedResult,
                                      ModifiedDate = getdate(), 
                                      ModifiedUser ='UpdateCusAML'
                                      where ApplNo=@ApplNo and ApplNoB=@ApplNoB and BinId=@CusID

                                    INSERT INTO [RASCNameCheckResult]
                                               ([ApplNo]
                                               ,[ApplNoB]
                                               ,[NameCheckResult]
                                               ,[CreatedUser]
                                               ,[CreatedDate]
                                               ,[ModifiedUser]
                                               ,[ModifiedDate])
                                         VALUES
                                               (@ApplNo
                                               ,@ApplNoB
                                               ,@AMLMatchedResult
                                               ,'sys'
                                               ,GETDATE()
                                               ,'sys'
                                               ,GETDATE())

                                    update RASCNameCheckSend
                                    set  QueryStatus='0',
                                            SendTimes='0',
                                            ReturnCode=@sRspCode,
                                            ModifiedDate = getdate(),
                                            ModifiedUser ='UpdateCusAML' 
                                            where ApplNo = @ApplNo and ApplNoB = @ApplNoB";
                }
                else
                {
                    sql = "update RASCNameCheckSend  set QueryStatus='3',SendTimes='0',ModifiedDate = getdate(), ModifiedUser ='UpdateCusAML' where ApplNo = @ApplNo and ApplNoB = @ApplNoB";
                }

                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@AMLMatchedResult", amlMatchedResult));
                base.Parameter.Add(new CommandParameter("@MaxQryTime", MaxQryTime));
                base.Parameter.Add(new CommandParameter("@ApplNo", applNo));
                base.Parameter.Add(new CommandParameter("@ApplNoB", applNoB));
                base.Parameter.Add(new CommandParameter("@CusID", CusID));
                base.Parameter.Add(new CommandParameter("@sRspCode", sRspCode));
                base.ExecuteNonQuery(sql);
            }
        }

        /// <summary>
        /// 查詢案件是否已存在RASCNameCheckResult
        /// </summary>
        /// <param name="applNo"></param>
        /// <param name="applNoB"></param>
        /// <returns></returns>
        public string SelectRASCNameCheckResult(string applNo, string applNoB)
        {
            DataTable _dt = new DataTable();
            string strSql = @"  select top 1 ApplNo from RASCNameCheckResult where ApplNo=@ApplNo and ApplNoB=@ApplNoB";
            base.Parameter.Clear();
            base.Parameter.Add(new CommandParameter("@ApplNo", applNo));
            base.Parameter.Add(new CommandParameter("@ApplNoB", applNoB));
            object StepID = null;

            StepID = ExecuteScalar(strSql);

            return StepID == null ? "" : StepID.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        public void UpdateRASCNameCheckSend(string applNo, string applNoB)
        {
            string sql = null;
            sql = @"Update RASCNameCheckSend set QueryStatus=1 ,QueryTimes=0 where ApplNo=@ApplNo and ApplNoB=@ApplNoB";
            base.Parameter.Clear();
            base.Parameter.Add(new CommandParameter("@ApplNo", applNo));
            base.Parameter.Add(new CommandParameter("@ApplNoB", applNoB));
            base.ExecuteNonQuery(sql);
        }

        /// <summary>
        /// 
        /// </summary>
        public void UpdateRASCNameCheckSendTimes(string applNo, string applNoB, string MaxQryTime)
        {
            string sql = null;
            sql = @"update RASCNameCheckSend  set SendTimes= case when RASCNameCheckSend.SendTimes < @MaxQryTime then SendTimes+1 else SendTimes end , QueryStatus= case when RASCNameCheckSend.SendTimes >= @MaxQryTime then '1' else QueryStatus end ,ModifiedDate = getdate(), ModifiedUser ='UpdateCusAML' where RASCNameCheckSend.ApplNo=@ApplNo and RASCNameCheckSend.ApplNoB=@ApplNoB";

            base.Parameter.Clear();
            base.Parameter.Add(new CommandParameter("@MaxQryTime", MaxQryTime));
            base.Parameter.Add(new CommandParameter("@ApplNo", applNo));
            base.Parameter.Add(new CommandParameter("@ApplNoB", applNoB));
            base.ExecuteNonQuery(sql);
        }
        #endregion
        #endregion

        #region AML PCSM 20160215
        /// <summary>
        /// 取得PCSM的資料 20160214
        /// </summary>
        /// <param name="ApplNo"></param>
        /// <param name="ApplNoB"></param>
        /// <returns></returns>
        public DataTable GetPCSMList(string ApplNo, string ApplNoB)
        {
            int _total = 10;//需十筆
            DataTable _t;
            string sql = @"select top 10
                            NUMSCustomerInfo.CusID CusID,
                            NUMSMaster.ApplNo, 
                            NUMSMaster.ApplNoB,
                            isnull((select top 1 CodeDesc from PARMCODE where CodeType = 'PCSMAffiliatesCode' and CodeTag = LoanRelation),'') AffiliatesCode,
                            isnull((select top 1 CodeDesc from PARMCODE where CodeType = 'PCSMAffiliatesType' and CodeTag = LoanRelation),'') AffiliatesType,
                            'N' CorporateFlag,
                            AMLMatchedResult NameCheck,
                            SCDDCC IndustryCode, --CC每個人都要自己抓
                            SCDDOC OccupationCode, --OC每個人都要自己抓
                            NationalityCode Nation,
                            case when RegNationalityCode is null or RegNationalityCode = '' then 'TW' else RegNationalityCode end PermAddrNation,
                            case when ComNationalityCode is null or ComNationalityCode = '' then 'TW' else ComNationalityCode end CommAddrNation,
                            case when UDWRDetailByCusId.Punish is null or UDWRDetailByCusId.Punish = '' then AMLMatchedResult else UDWRDetailByCusId.Punish end Sanction,
                            case when UDWRDetailByCusId.PEP is null or UDWRDetailByCusId.PEP = '' then AMLMatchedResult else UDWRDetailByCusId.PEP end Politicallyexposedperson,
                            case when UDWRDetailByCusId.NN is null or UDWRDetailByCusId.NN = '' then AMLMatchedResult else UDWRDetailByCusId.NN end NegtiveNews,
                            '' CorporateIndustry1,
                            '' CorporateIndustry2,
                            '' CorporateIndustry3,
                            '' I1,
                            '' I2,
                            '' I3,
                            '' I4,
                            '' I5,
                            '' I6,
                            '' I7,
                            '' I8,
                            '' I9,
                            '' I10,
                            case when UDWRSCDD.IsRejectGiveInfo is null or UDWRSCDD.IsRejectGiveInfo = '' then 'N' else UDWRSCDD.IsRejectGiveInfo end DCFlag,
                            '' BearerCompany
                            from NUMSMaster
                            inner join NUMSCustomerInfo on NUMSMaster.ApplNo = NUMSCustomerInfo.ApplNo and NUMSMaster.ApplNoB = NUMSCustomerInfo.ApplNoB and status = N'Y'
                            inner join UDWRDetailByCusId on NUMSMaster.ApplNo = UDWRDetailByCusId.ApplNo and NUMSMaster.ApplNoB = UDWRDetailByCusId.ApplNoB and UDWRDetailByCusId.CusID = NUMSCustomerInfo.CusID
                            left join UDWRSCDD on NUMSMaster.ApplNo = UDWRSCDD.ApplNo and NUMSMaster.ApplNoB = UDWRSCDD.ApplNoB
                            where NUMSMaster.ApplNo = @ApplNo and NUMSMaster.ApplNoB = @ApplNoB
                            order by LoanRelation,NUMSCustomerInfo.CusID";
            try
            {
                // 清除參數
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@ApplNo", ApplNo));
                base.Parameter.Add(new CommandParameter("@ApplNoB", ApplNoB));
                _t = base.Search(sql);
                int _rowscount = _t.Rows.Count;
                if (_rowscount < _total)
                {
                    for (int i = 1; i <= (_total - _rowscount); i++)
                    {
                        DataRow row = _t.NewRow();
                        _t.Rows.Add(row);
                    }
                }

                return _t;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// PCSM的處理,回寫資訊到資料庫中 20160214
        /// </summary>
        /// <param name="XmlData"></param>
        /// <param name="url"></param>
        /// <param name="method"></param>
        /// <param name="encoding"></param>
        /// <param name="contenttype"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public void UpdatePCSMData(string ApplNo, string ApplNoB, UDWRPCSMResultVO _vo, IList<UDWRPCSMResultVO> _detail, string _processname)
        {
            try
            {
                base.Parameter.Clear();
                string sql = @"
                                delete from UDWRPCSMResult where ApplNo = @ApplNo and ApplNoB = @ApplNoB; --先刪除
                                delete from UDWRPCSMDetailResult where ApplNo = @ApplNo and ApplNoB = @ApplNoB; --先刪除";

                sql = sql + @"
                                  INSERT INTO [dbo].[UDWRPCSMResult]
                                           ([ApplNo]
                                           ,[ApplNoB]
                                           ,[AMLRisk]
                                           ,[QH1]
                                           ,[QH2]
                                           ,[QH3]
                                           ,[QH4]
                                           ,[QH5]
                                           ,[QH6]
                                           ,[QH7]
                                           ,[QH8]
                                           ,[QM1]
                                           ,[QM2]
                                           ,[QM3]
                                           ,[UndoEddFlag]
                                           ,[CreatedUser]
                                           ,[CreatedDate]
                                           ,[ModifiedUser]
                                           ,[ModifiedDate])
                                     VALUES
                                           (@ApplNo
                                           ,@ApplNoB
                                           ,@AMLRisk
                                           ,@QH1
                                           ,@QH2
                                           ,@QH3
                                           ,@QH4
                                           ,@QH5
                                           ,@QH6
                                           ,@QH7
                                           ,@QH8
                                           ,@QM1
                                           ,@QM2
                                           ,@QM3
                                           ,@UndoEddFlag
                                           ,@CreatedUser
                                           ,getdate()
                                           ,@CreatedUser
                                           ,getdate());";

                int i = 0;
                foreach (UDWRPCSMResultVO item in _detail)
                {
                    if (!string.IsNullOrEmpty(item.CusID))
                    {
                        sql = sql + @"
                                   INSERT INTO [dbo].[UDWRPCSMDetailResult]
                                          ([ApplNo]
                                           ,[ApplNoB]
                                           ,[CusID]
                                           ,[QH1]
                                           ,[QH2]
                                           ,[QH3]
                                           ,[QH4]
                                           ,[QH5]
                                           ,[QH6]
                                           ,[QH7]
                                           ,[QH8]
                                           ,[QM1]
                                           ,[QM2]
                                           ,[QM3]
                                           ,[CreatedUser]
                                           ,[CreatedDate]
                                           ,[ModifiedUser]
                                           ,[ModifiedDate])
                         VALUES
                                          (@ApplNo
                                           ,@ApplNoB" +
                                               @",@CusID" + i +
                                               ",@QH1" + i +
                                               ",@QH2" + i +
                                               ",@QH3" + i +
                                               ",@QH4" + i +
                                               ",@QH5" + i +
                                               ",@QH6" + i +
                                               ",@QH7" + i +
                                               ",@QH8" + i +
                                               ",@QM1" + i +
                                               ",@QM2" + i +
                                               ",@QM3" + i +
                                               @",@CreatedUser
                                           ,getdate()
                                           ,@CreatedUser
                                           ,getdate());";

                        base.Parameter.Add(new CommandParameter("@CusID" + i, item.CusID));
                        base.Parameter.Add(new CommandParameter("@QH1" + i, item.Qh1));
                        base.Parameter.Add(new CommandParameter("@QH2" + i, item.Qh2));
                        base.Parameter.Add(new CommandParameter("@QH3" + i, item.Qh3));
                        base.Parameter.Add(new CommandParameter("@QH4" + i, item.Qh4));
                        base.Parameter.Add(new CommandParameter("@QH5" + i, item.Qh5));
                        base.Parameter.Add(new CommandParameter("@QH6" + i, item.Qh6));
                        base.Parameter.Add(new CommandParameter("@QH7" + i, item.Qh7));
                        base.Parameter.Add(new CommandParameter("@QH8" + i, item.Qh8));
                        base.Parameter.Add(new CommandParameter("@QM1" + i, item.Qm1));
                        base.Parameter.Add(new CommandParameter("@QM2" + i, item.Qm2));
                        base.Parameter.Add(new CommandParameter("@QM3" + i, item.Qm3));

                        i++;
                    }
                }

                base.Parameter.Add(new CommandParameter("@ApplNo", ApplNo));
                base.Parameter.Add(new CommandParameter("@ApplNoB", ApplNoB));
                base.Parameter.Add(new CommandParameter("@AMLRisk", _vo.AMLRisk));
                base.Parameter.Add(new CommandParameter("@QH1", _vo.Qh1));
                base.Parameter.Add(new CommandParameter("@QH2", _vo.Qh2));
                base.Parameter.Add(new CommandParameter("@QH3", _vo.Qh3));
                base.Parameter.Add(new CommandParameter("@QH4", _vo.Qh4));
                base.Parameter.Add(new CommandParameter("@QH5", _vo.Qh5));
                base.Parameter.Add(new CommandParameter("@QH6", _vo.Qh6));
                base.Parameter.Add(new CommandParameter("@QH7", _vo.Qh7));
                base.Parameter.Add(new CommandParameter("@QH8", _vo.Qh8));
                base.Parameter.Add(new CommandParameter("@QM1", _vo.Qm1));
                base.Parameter.Add(new CommandParameter("@QM2", _vo.Qm2));
                base.Parameter.Add(new CommandParameter("@QM3", _vo.Qm3));
                base.Parameter.Add(new CommandParameter("@UndoEddFlag", _vo.UndoEddFlag));
                base.Parameter.Add(new CommandParameter("@CreatedUser", _processname));
                base.ExecuteNonQuery(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// PCSM的處理, 20160214
        /// </summary>
        /// <param name="XmlData"></param>
        /// <param name="url"></param>
        /// <param name="method"></param>
        /// <param name="encoding"></param>
        /// <param name="contenttype"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public void DeleteSCDDData(string ApplNo, string ApplNoB)
        {
            try
            {
                string sql = @"
                                delete from UDWRSCDD where ApplNo = @ApplNo and ApplNoB = @ApplNoB; --先刪除
                                delete from UDWRSCDDEDDItem where ApplNo = @ApplNo and ApplNoB = @ApplNoB; --先刪除
                                ";
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@ApplNo", ApplNo));
                base.Parameter.Add(new CommandParameter("@ApplNoB", ApplNoB));
                base.ExecuteNonQuery(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// PCSM的處理,產生XML文件 20160214
        /// </summary>
        /// <param name="XmlData"></param>
        /// <param name="url"></param>
        /// <param name="method"></param>
        /// <param name="encoding"></param>
        /// <param name="contenttype"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public string GenerateRequestXml(string ApplNo, string ApplNoB, DataTable _t)
        {
            try
            {
                int i = 1;//客戶編號
                int total = 10;//總客戶數
                const string _ALIAS = "GL2AML";
                const string _SIGNATURE = "CTBCPCSM";
                StringBuilder _s = new StringBuilder();
                _s.Append("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                _s.AppendLine("<soap:envelope xmlns:soap=\"http://www.w3.org/2003/05/soap-envelope\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">");
                _s.AppendLine("<soap:body>");
                _s.AppendLine("<DAXMLDocument xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">");
                _s.AppendLine("<OCONTROL><ALIAS>" + _ALIAS + "</ALIAS>");
                _s.AppendLine("<SIGNATURE>" + _SIGNATURE + "</SIGNATURE>");
                _s.AppendLine("</OCONTROL>");
                _s.AppendLine("<JGL2LOCL><New></New>");
                _s.AppendLine("</JGL2LOCL>");
                _s.AppendLine("<JGL2RSLT><AmlRisk></AmlRisk>");
                _s.AppendLine("<Qh1></Qh1>");
                _s.AppendLine("<Qh2></Qh2>");
                _s.AppendLine("<Qh3></Qh3>");
                _s.AppendLine("<Qh4></Qh4>");
                _s.AppendLine("<Qh5></Qh5>");
                _s.AppendLine("<Qh6></Qh6>");
                _s.AppendLine("<Qh7></Qh7>");
                _s.AppendLine("<Qh8></Qh8>");
                _s.AppendLine("<Qm1></Qm1>");
                _s.AppendLine("<Qm2></Qm2>");
                _s.AppendLine("<Qm3></Qm3>");
                _s.AppendLine("<UndoEddFlag></UndoEddFlag>");
                for (int j = 1; j <= total; j++)
                {
                    _s.AppendLine("<Details" + j + "><Qh1></Qh1>");
                    _s.AppendLine("<Qh2></Qh2>");
                    _s.AppendLine("<Qh3></Qh3>");
                    _s.AppendLine("<Qh4></Qh4>");
                    _s.AppendLine("<Qh5></Qh5>");
                    _s.AppendLine("<Qh6></Qh6>");
                    _s.AppendLine("<Qh7></Qh7>");
                    _s.AppendLine("<Qh8></Qh8>");
                    _s.AppendLine("<Qm1></Qm1>");
                    _s.AppendLine("<Qm2></Qm2>");
                    _s.AppendLine("<Qm3></Qm3>");
                    _s.AppendLine("</Details" + j + ">");
                }

                _s.AppendLine("<G203DataDerivation><ResultDecimal1>0</ResultDecimal1>");
                _s.AppendLine("<ResultDecimal2>0</ResultDecimal2>");
                _s.AppendLine("<ResultDecimal3>0</ResultDecimal3>");
                _s.AppendLine("<ResultInt1>0</ResultInt1>");
                _s.AppendLine("<ResultInt2>0</ResultInt2>");
                _s.AppendLine("<ResultInt3>0</ResultInt3>");
                _s.AppendLine("<ResultString1></ResultString1>");
                _s.AppendLine("<ResultString2></ResultString2>");
                _s.AppendLine("<ResultString3></ResultString3>");
                _s.AppendLine("</G203DataDerivation></JGL2RSLT>");

                _s.AppendLine("<JGL2INPT><ApplNo>" + ApplNo + "</ApplNo>");

                #region 客戶資訊
                foreach (DataRow _row in _t.Rows)
                {
                    _s.AppendLine("<ClientInfo" + i + "><AffilatesType>" + _row["AffiliatesType"] + "</AffilatesType>");
                    _s.AppendLine("<AffiliatesCode>" + _row["AffiliatesCode"] + "</AffiliatesCode>");
                    _s.AppendLine("<BearerCompany>" + _row["BearerCompany"] + "</BearerCompany>");
                    _s.AppendLine("<CommAddrNation>" + _row["CommAddrNation"] + "</CommAddrNation>");
                    _s.AppendLine("<CorporateFlag>" + _row["CorporateFlag"] + "</CorporateFlag>");
                    _s.AppendLine("<CorporateIndustry1>" + _row["CorporateIndustry1"] + "</CorporateIndustry1>");
                    _s.AppendLine("<CorporateIndustry2>" + _row["CorporateIndustry2"] + "</CorporateIndustry2>");
                    _s.AppendLine("<CorporateIndustry3>" + _row["CorporateIndustry3"] + "</CorporateIndustry3>");
                    _s.AppendLine("<CorporateType><I1>" + _row["I1"] + "</I1>");
                    _s.AppendLine("<I2>" + _row["I2"] + "</I2>");
                    _s.AppendLine("<I3>" + _row["I3"] + "</I3>");
                    _s.AppendLine("<I4>" + _row["I4"] + "</I4>");
                    _s.AppendLine("<I5>" + _row["I5"] + "</I5>");
                    _s.AppendLine("<I6>" + _row["I6"] + "</I6>");
                    _s.AppendLine("<I7>" + _row["I7"] + "</I7>");
                    _s.AppendLine("<I8>" + _row["I8"] + "</I8>");
                    _s.AppendLine("<I9>" + _row["I9"] + "</I9>");
                    _s.AppendLine("<I10>" + _row["I10"] + "</I10>");
                    _s.AppendLine("</CorporateType><DcFlag>" + _row["DcFlag"] + "</DcFlag>");
                    _s.AppendLine("<IndustryCode>" + _row["IndustryCode"] + "</IndustryCode>");
                    _s.AppendLine("<NameCheck>" + _row["NameCheck"] + "</NameCheck>");
                    _s.AppendLine("<Nation>" + _row["Nation"] + "</Nation>");
                    _s.AppendLine("<NegtiveNews>" + _row["NegtiveNews"] + "</NegtiveNews>");
                    _s.AppendLine("<OccupationCode>" + _row["OccupationCode"] + "</OccupationCode>");
                    _s.AppendLine("<PermAddrNation>" + _row["PermAddrNation"] + "</PermAddrNation>");
                    _s.AppendLine("<PoliticallyExposedPerson>" + _row["PoliticallyExposedPerson"] + "</PoliticallyExposedPerson>");
                    _s.AppendLine("<Sanction>" + _row["Sanction"] + "</Sanction>");
                    _s.AppendLine("</ClientInfo" + i + ">");
                    i++;
                }
                #endregion

                _s.AppendLine("</JGL2INPT>");
                _s.AppendLine("</DAXMLDocument>");
                _s.AppendLine("</soap:body>");
                _s.AppendLine("</soap:envelope>");

                return _s.ToString();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #region RASC PCSM 20160224

        /// <summary>
        /// RCSM待查詢 案件列表 20160318RC
        /// </summary>
        /// <returns></returns>
        public DataTable RASCGetPCSMList(string MaxQryTime)
        {
            string sql = @"Select applno,applnob from RASCPCSMSend
                           where QueryStatus=5 and SendTimes <= @MaxQryTime";
            // 清除參數
            base.Parameter.Clear();
            base.Parameter.Add(new CommandParameter("@MaxQryTime", MaxQryTime));
            return base.Search(sql);
        }

        /// <summary>
        /// PCSM BY 案 撈發查資料 20160318RC
        /// </summary>
        /// <param name="ApplNo"></param>
        /// <param name="ApplNoB"></param>
        /// <returns></returns>
        public DataTable RASCGetPCSMList(string ApplNo, string ApplNoB)
        {
            int _total = 10;//需十筆
            DataTable _t;
            string sql = @";with T2 as 
                            (
                            select 
                            M.ApplNo,
                            M.ApplNoB,
                            M.CaseType
                            from 
                            RASCMaster M 
                            where M.ApplNo=@ApplNo and M.ApplNoB=@ApplNoB
                            )       
                            ,T3 as 
                            (
                            select 
                            C.ApplNo,
                            C.ApplNoB,
                            isnull((select top 1 CodeDesc from PARMCODE where CodeType = 'PCSMAffiliatesCode' and CodeTag = C.LoanRelation),'') AffiliatesCode,
                            isnull((select top 1 CodeDesc from PARMCODE where CodeType = 'PCSMAffiliatesType' and CodeTag = C.LoanRelation),'') AffiliatesType,
                            'N' CorporateFlag,
                            C.NameCheckResult NameCheck,
                            C.Punish Sanction,
                            C.PEP Politicallyexposedperson,
                            C.NN NegtiveNews,
                            C.NationalityCode Nation,
                            C.DCFlag DCFlag,
                            C.NationalityCodeCom CommAddrNation,
                            C.NationalityCodeReg PermAddrNation,
                            C.AMLCC IndustryCode,
                            C.AMLOC OccupationCode,
		                    '' CorporateIndustry1,
		                    '' CorporateIndustry2,
		                    '' CorporateIndustry3,
		                    '' I1,
		                    '' I2,
		                    '' I3,
		                    '' I4,
		                    '' I5,
		                    '' I6,
		                    '' I7,
		                    '' I8,
		                    '' I9,
		                    '' I10,
		                    '' BearerCompany
                            from 
                            RASCCustomer C
                            left join 
                            T2 T
                            on 
                            C.ApplNo=T.ApplNo
                            and 
                            C.ApplNoB=T.ApplNoB
                            where 
                            C.ApplNo=T.ApplNo
                            and 
                            C.ApplNoB=T.ApplNoB
                            and 
                            T.CaseType='1'
                            and
                            C.LoanRelation='1'
                            UNION ALL 
                            select 
                            C.ApplNo,
                            C.ApplNoB,
                            'APP' AffiliatesCode,
                            'APP' AffiliatesType,
		                    'Y' CorporateFlag,
                            C.NameCheckResult NameCheck,
                            C.Punish Sanction,
                            C.PEP Politicallyexposedperson,
                            C.NN NegtiveNews,
                            C.NationalityCode Nation,
                            C.DCFlag DCFlag,
                            C.NationalityCodeCom CommAddrNation,
                            C.NationalityCodeReg PermAddrNation,
                            C.AMLCC IndustryCode,
                            C.AMLOC OccupationCode,
		                    SCDD.CC1 CorporateIndustry1,
		                    SCDD.CC2 CorporateIndustry2,
		                    SCDD.CC3 CorporateIndustry3,
		                    (case when CHARINDEX('I1',C.CorporateType)='1' then 'Y' else 'N' end) I1,
		                    (case when CHARINDEX('I2',C.CorporateType)='1' then 'Y' else 'N' end) I2,
		                    (case when CHARINDEX('I3',C.CorporateType)='1' then 'Y' else 'N' end) I3,
		                    (case when CHARINDEX('I4',C.CorporateType)='1' then 'Y' else 'N' end) I4,
		                    (case when CHARINDEX('I5',C.CorporateType)='1' then 'Y' else 'N' end) I5,
		                    (case when CHARINDEX('I6',C.CorporateType)='1' then 'Y' else 'N' end) I6,
		                    (case when CHARINDEX('I7',C.CorporateType)='1' then 'Y' else 'N' end) I7,
		                    (case when CHARINDEX('I8',C.CorporateType)='1' then 'Y' else 'N' end) I8,
		                    (case when CHARINDEX('I9',C.CorporateType)='1' then 'Y' else 'N' end) I9,
		                    (case when CHARINDEX('I10',C.CorporateType)='1' then 'Y' else 'N' end) I10,
		                    SCDD.IsBearerCompany BearerCompany
                            from 
                            RASCCompany C
                            left join 
                            T2 T
                            on 
                            C.ApplNo=T.ApplNo
                            and 
                            C.ApplNoB=T.ApplNoB
                            left join
                            RASCCompanySCDD SCDD
                            on
                            SCDD.ApplNo=T.ApplNo
                            and 
                            SCDD.ApplNoB=T.ApplNoB
                            where 
                            C.ApplNo=T.ApplNo
                            and 
                            C.ApplNoB=T.ApplNoB
                            and
                            SCDD.ApplNo=T.ApplNo
                            and 
                            SCDD.ApplNoB=T.ApplNoB
                            and 
                            T.CaseType='2'
                            )
                            select
                            ApplNo,
                            ApplNoB,
                            AffiliatesCode,
                            AffiliatesType,
                            CorporateFlag,
                            NameCheck,
                            IndustryCode,
                            OccupationCode,
                            Nation,
                            PermAddrNation,
                            CommAddrNation,
                            Sanction,
                            Politicallyexposedperson,
                            NegtiveNews,
                            CorporateIndustry1,
                            CorporateIndustry2,
                            CorporateIndustry3,
                            I1,
                            I2,
                            I3,
                            I4,
                            I5,
                            I6,
                            I7,
                            I8,
                            I9,
                            I10,
                            DCFlag,
                            BearerCompany 
                            from T3 order by ApplNo,ApplNoB";
            try
            {
                // 清除參數
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@ApplNo", ApplNo));
                base.Parameter.Add(new CommandParameter("@ApplNoB", ApplNoB));
                _t = base.Search(sql);
                int _rowscount = _t.Rows.Count;
                if (_rowscount < _total)
                {
                    for (int i = 1; i <= (_total - _rowscount); i++)
                    {
                        DataRow row = _t.NewRow();
                        _t.Rows.Add(row);
                    }
                }
                return _t;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// PCSM 發查成功，儲存資料 20160318RC
        /// </summary>
        /// <param name="ApplNo"></param>
        /// <param name="ApplNoB"></param>
        /// <param name="_vo"></param>
        public void RASCUpdatePCSMData(string ApplNo, string ApplNoB, UDWRPCSMResultVO _vo)
        {
            //檢查是否已存在RASCPCSMResult table
            string CheckRASCPCSMResult = SelectRASCPCSMResult(ApplNo, ApplNoB);
            //更新成功發查狀態
            UpdateRASCPCSMSendQueryStatus(ApplNo, ApplNoB);
            if (ApplNo == CheckRASCPCSMResult)
            {

                string strSql = @" update RASCPCSMResult
                                        set
                                           [AMLRisk]=@AMLRisk
                                           ,[QH1]=@QH1
                                           ,[QH2]=@QH2
                                           ,[QH3]=@QH3
                                           ,[QH4]=@QH4
                                           ,[QH5]=@QH5
                                           ,[QH6]=@QH6
                                           ,[QH7]=@QH7
                                           ,[QH8]=@QH8
                                           ,[QM1]=@QM1
                                           ,[QM2]=@QM2
                                           ,[QM3]=@QM3
                                           ,[ModifiedUser]='RASCPCSMInquiry'
                                           ,[ModifiedDate]=getdate()
                                            where 
                                            ApplNo=@ApplNo
                                            and
                                            ApplNoB=@ApplNoB";


                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@ApplNo", ApplNo));
                base.Parameter.Add(new CommandParameter("@ApplNoB", ApplNoB));
                base.Parameter.Add(new CommandParameter("@AMLRisk", _vo.AMLRisk));
                base.Parameter.Add(new CommandParameter("@QH1", _vo.Qh1));
                base.Parameter.Add(new CommandParameter("@QH2", _vo.Qh2));
                base.Parameter.Add(new CommandParameter("@QH3", _vo.Qh3));
                base.Parameter.Add(new CommandParameter("@QH4", _vo.Qh4));
                base.Parameter.Add(new CommandParameter("@QH5", _vo.Qh5));
                base.Parameter.Add(new CommandParameter("@QH6", _vo.Qh6));
                base.Parameter.Add(new CommandParameter("@QH7", _vo.Qh7));
                base.Parameter.Add(new CommandParameter("@QH8", _vo.Qh8));
                base.Parameter.Add(new CommandParameter("@QM1", _vo.Qm1));
                base.Parameter.Add(new CommandParameter("@QM2", _vo.Qm2));
                base.Parameter.Add(new CommandParameter("@QM3", _vo.Qm3));
                base.ExecuteNonQuery(strSql);

            }
            else
            {
                string strSql = @"
                                INSERT INTO RASCPCSMResult
                                           ([ApplNo]
                                           ,[ApplNoB]
                                           ,[AMLRisk]
                                           ,[QH1]
                                           ,[QH2]
                                           ,[QH3]
                                           ,[QH4]
                                           ,[QH5]
                                           ,[QH6]
                                           ,[QH7]
                                           ,[QH8]
                                           ,[QM1]
                                           ,[QM2]
                                           ,[QM3]
                                           ,[CreatedUser]
                                           ,[CreatedDate]
                                           ,[ModifiedUser]
                                           ,[ModifiedDate])
                                     VALUES
                                           (@ApplNo
                                           ,@ApplNoB
                                           ,@AMLRisk
                                           ,@QH1
                                           ,@QH2
                                           ,@QH3
                                           ,@QH4
                                           ,@QH5
                                           ,@QH6
                                           ,@QH7
                                           ,@QH8
                                           ,@QM1
                                           ,@QM2
                                           ,@QM3
                                           ,'RASCPCSMInquiry'
                                           ,getdate()
                                           ,'RASCPCSMInquiry'
                                           ,getdate())
                                                ";

                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@ApplNo", ApplNo));
                base.Parameter.Add(new CommandParameter("@ApplNoB", ApplNoB));
                base.Parameter.Add(new CommandParameter("@AMLRisk", _vo.AMLRisk));
                base.Parameter.Add(new CommandParameter("@QH1", _vo.Qh1));
                base.Parameter.Add(new CommandParameter("@QH2", _vo.Qh2));
                base.Parameter.Add(new CommandParameter("@QH3", _vo.Qh3));
                base.Parameter.Add(new CommandParameter("@QH4", _vo.Qh4));
                base.Parameter.Add(new CommandParameter("@QH5", _vo.Qh5));
                base.Parameter.Add(new CommandParameter("@QH6", _vo.Qh6));
                base.Parameter.Add(new CommandParameter("@QH7", _vo.Qh7));
                base.Parameter.Add(new CommandParameter("@QH8", _vo.Qh8));
                base.Parameter.Add(new CommandParameter("@QM1", _vo.Qm1));
                base.Parameter.Add(new CommandParameter("@QM2", _vo.Qm2));
                base.Parameter.Add(new CommandParameter("@QM3", _vo.Qm3));
                base.ExecuteNonQuery(strSql);
            }

        }


        /// <summary>
        /// 查詢案件是否已存在PCSMResult 20160318RC
        /// </summary>
        /// <param name="applNo"></param>
        /// <param name="applNoB"></param>
        /// <returns></returns>
        public string SelectRASCPCSMResult(string applNo, string applNoB)
        {
            DataTable _dt = new DataTable();
            string strSql = @"  select top 1 ApplNo from RASCPCSMResult where ApplNo=@ApplNo and ApplNoB=@ApplNoB";
            base.Parameter.Clear();
            base.Parameter.Add(new CommandParameter("@ApplNo", applNo));
            base.Parameter.Add(new CommandParameter("@ApplNoB", applNoB));
            object StepID = null;

            StepID = ExecuteScalar(strSql);

            return StepID == null ? "" : StepID.ToString();
        }

        /// <summary>
        /// 更新發查狀態 20160318RC
        /// </summary>
        public void UpdateRASCPCSMSend(string applNo, string applNoB)
        {
            string sql = null;
            sql = @"Update RASCPCSMSend set QueryStatus=1 ,SendTimes=0 where ApplNo=@ApplNo and ApplNoB=@ApplNoB";
            base.Parameter.Clear();
            base.Parameter.Add(new CommandParameter("@ApplNo", applNo));
            base.Parameter.Add(new CommandParameter("@ApplNoB", applNoB));
            base.ExecuteNonQuery(sql);
        }

        /// <summary>
        /// 發查次數+1 20160318RC
        /// </summary>
        public void UpdateRASCPCSMSendTimes(string applNo, string applNoB, string MaxQryTime)
        {
            string sql = null;
            sql = @"update RASCPCSMSend  set SendTimes= case when RASCPCSMSend.SendTimes < @MaxQryTime then SendTimes+1 else SendTimes end , QueryStatus= case when RASCPCSMSend.SendTimes >= @MaxQryTime then '1' else QueryStatus end ,ModifiedDate = getdate(), ModifiedUser ='RASCPCSMInquiry' where RASCPCSMSend.ApplNo=@ApplNo and RASCPCSMSend.ApplNoB=@ApplNoB";

            base.Parameter.Clear();
            base.Parameter.Add(new CommandParameter("@MaxQryTime", MaxQryTime));
            base.Parameter.Add(new CommandParameter("@ApplNo", applNo));
            base.Parameter.Add(new CommandParameter("@ApplNoB", applNoB));
            base.ExecuteNonQuery(sql);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="applNo"></param>
        /// <param name="applNoB"></param>
        public void UpdateRASCPCSMSendQueryStatus(string applNo, string applNoB)
        {
            base.Parameter.Clear();
            base.Parameter.Add(new CommandParameter("@ApplNo", applNo));
            base.Parameter.Add(new CommandParameter("@ApplNoB", applNoB));
            string sql = null;
            sql = @"update RASCPCSMSend  set QueryStatus='0',SendTimes='0',ModifiedDate = getdate(), ModifiedUser ='RASCPCSMInquiry' where ApplNo = @ApplNo and ApplNoB = @ApplNoB";
            base.ExecuteNonQuery(sql);
        }

        #endregion
        #endregion

        #region AML 黃頁查詢 20160301
        /// <summary>
        /// 取得AML 黃頁的資料 20160214
        /// </summary>
        /// <param name="ApplNo"></param>
        /// <param name="ApplNoB"></param>
        /// <returns></returns>
        public DataTable GetYPList(string ApplNo, string ApplNoB)
        {
            DataTable _t;
            string sql = @"select 
                            NUMSMaster.ApplNo, 
                            NUMSMaster.ApplNoB,
                            '' CaseID,
                            NUMSCustomerInfo.CusID,
							(select  top 1  YP_OC from PARMYPOCMapping where oc=JobOC) OC,--20160805RC edit by 小朱
                            '' ApplyJobTitle,
                            isnull(ltrim(rtrim(JobTelArea)) + ltrim(rtrim(JobTelNo)),'00000000') TEL_COMP,
                            isnull((select top 1 CurrentProcessingApplEmpNo from UDWRDerivedData where UDWRDerivedData.ApplNo = NUMSMaster.ApplNo and UDWRDerivedData.ApplNoB = NUMSMaster.ApplNoB),'NoEmpID') CreatedUser
                            from NUMSMaster
                            inner join NUMSCustomerInfo on NUMSMaster.ApplNo = NUMSCustomerInfo.ApplNo and NUMSMaster.ApplNoB = NUMSCustomerInfo.ApplNoB and [Status] = N'Y'
                            where NUMSMaster.ApplNo = @ApplNo and NUMSMaster.ApplNoB = @ApplNoB";
            try
            {
                // 清除參數
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@ApplNo", ApplNo));
                base.Parameter.Add(new CommandParameter("@ApplNoB", ApplNoB));
                _t = base.Search(sql);
                return _t;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 取得AML 黃頁的Send資料 20160214
        /// 20160805RC edit by 小朱 需求內容:發查前轉換JobOC to YP_OC
        /// </summary>
        /// <param name="ApplNo"></param>
        /// <param name="ApplNoB"></param>
        /// <returns></returns>
        public DataTable GetYPGetList(string ApplNo, string ApplNoB)
        {
            DataTable _t;
            string sql = @"select 
                            (select top 1 CodeDesc from PARMCODE where CodeType = 'YellowPGetSecs') YellowPGetSecs,
                            NUMSMaster.ApplNo, 
                            NUMSMaster.ApplNoB,
                            UDWRYellowPageSend.CaseID,
                            NUMSCustomerInfo.CusID,
							(select  top 1  YP_OC from PARMYPOCMapping where oc=JobOC) OC,--20160805RC edit by 小朱
                            'NoJobTitle' ApplyJobTitle,
                            isnull(ltrim(rtrim(JobTelArea)) + ltrim(rtrim(JobTelNo)),'00000000') TEL_COMP,
                            SendTimes,
                            NUMSCustomerInfo.CreatedUser CreatedUser
                            from NUMSMaster
                            inner join NUMSCustomerInfo on NUMSMaster.ApplNo = NUMSCustomerInfo.ApplNo and NUMSMaster.ApplNoB = NUMSCustomerInfo.ApplNoB and [Status] = N'Y'
                            inner join UDWRYellowPageSend on NUMSMaster.ApplNo = UDWRYellowPageSend.ApplNo and NUMSMaster.ApplNoB = UDWRYellowPageSend.ApplNoB and UDWRYellowPageSend.CusID = NUMSCustomerInfo.CusID
                            where NUMSMaster.ApplNo = @ApplNo and NUMSMaster.ApplNoB = @ApplNoB order by SendTimes desc";
            try
            {
                // 清除參數
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@ApplNo", ApplNo));
                base.Parameter.Add(new CommandParameter("@ApplNoB", ApplNoB));
                _t = base.Search(sql);
                return _t;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion



        public void DeleteAndUpdate090012(string applNo, string applNoB, string cusId, string module, DataTable dt)
        {
            try
            {
                System.Text.StringBuilder sbsql = new System.Text.StringBuilder();

                //先組刪除的sql
                sbsql.AppendLine(@"Delete from NUMSBank90012Master where ApplNo=@ApplNo and ApplNoB=@ApplNoB and ID=@CusID and Module=@Module ;");
                base.Parameter.Add(new CommandParameter("@ApplNo", applNo));
                base.Parameter.Add(new CommandParameter("@ApplNoB", applNoB));
                base.Parameter.Add(new CommandParameter("@CusID", cusId));
                base.Parameter.Add(new CommandParameter("@Module", module));
                base.Parameter.Add(new CommandParameter("@CreatedUser", Account));


                int i = 1;
                foreach (DataRow dr in dt.Rows) //先將Insert UDWRApplMessage組好
                {
                    sbsql.AppendLine(@"insert into NUMSBank90012Master(ApplNo,ApplNoB,ID,Module,Class,ClassName,OrigInvestAmt,RefCurrAmt,RefInvestNet,RefReturnRate,CreatedUser,CreatedDate) ");
                    sbsql.Append(@" Values(@ApplNo,@ApplNoB,@CusID,@Module,");
                    sbsql.Append(@"@Class" + i.ToString() + ",");
                    sbsql.Append(@"@ClassName" + i.ToString() + ",");
                    sbsql.Append(@"@OrigInvestAmt" + i.ToString() + ",");
                    sbsql.Append(@"@RefCurrAmt" + i.ToString() + ",");
                    sbsql.Append(@"@RefInvestNet" + i.ToString() + ",");
                    sbsql.Append(@"@RefReturnRate" + i.ToString() + ",");
                    sbsql.Append(@"@CreatedUser,getdate());");
                    sbsql.AppendLine("");


                    base.Parameter.Add(new CommandParameter("@Class" + i.ToString(), dr["Class"].ToString()));
                    base.Parameter.Add(new CommandParameter("@ClassName" + i.ToString(), dr["ClassName"].ToString()));
                    base.Parameter.Add(new CommandParameter("@OrigInvestAmt" + i.ToString(), dr["OrigInvestAmt"].ToString()));
                    base.Parameter.Add(new CommandParameter("@RefCurrAmt" + i.ToString(), dr["RefCurrAmt"].ToString()));
                    base.Parameter.Add(new CommandParameter("@RefInvestNet" + i.ToString(), dr["RefInvestNet"].ToString()));
                    base.Parameter.Add(new CommandParameter("@RefReturnRate" + i.ToString(), dr["RefReturnRate"].ToString()));
                    i++;
                }
                base.ExecuteNonQuery(sbsql.ToString());
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //檢查案件是否是第一次進徵信的原因碼
        public bool CheckUDWRUDWRReasonCode(string strApplno, string strApplnoB)
        {
            string strSQL = @"select COUNT(stepid) as tot from FlowDetail where WorkItemID in 
                            (select WorkItemID from FlowCaseInformation where  ApplNo =@ApplNo and ApplNoB=@ApplNoB  )
                            and StepID in ('2320','3310') ";
            bool result = false;
            DataTable returnValue = new DataTable();
            try
            {
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@ApplNo", strApplno));
                base.Parameter.Add(new CommandParameter("@ApplNoB", strApplnoB));
                returnValue = base.Search(strSQL);
                if (Convert.ToInt16(returnValue.Rows[0]["tot"]) > 0)
                {
                    result = true;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return result;
        }

        /// <summary>
        /// OpeningCheck1 產生Service啟動狀況Log
        /// </summary>
        /// <returns></returns>
        /// 20140630 horace
        public DataTable ServiceStatusData(int dataPeriod)
        {
            string strSQL = @"select Timestamp,Message 
                                from NUMSLog with (nolock) 
                                where Title='Schedule' and Timestamp between DATEAdd(HH,@dataPeriod,getdate()) and getdate()
                                and Message not like '%schedules prepare to execute%'
                                order by Timestamp desc;";

            DataTable returnValue = new DataTable();
            try
            {
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@dataPeriod", dataPeriod));
                returnValue = base.Search(strSQL);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return returnValue;
        }

        /// <summary>
        /// OpeningCheck2 產生定時案件數量監控狀況Log
        /// </summary>
        /// <returns>舉例為:PSRN=15,NUMS=19,APRL=24,UDWR=24,UDWRApprove=1,DISPUDWR=15,DISPProduct=15</returns>
        /// 20140630 horace
        public string CaseCheckData()
        {
            string strSQL = "";
            string returnValue = "";

            //------------------------------------
            //--Step 1.宣告變數與Temp Table
            //------------------------------------
            strSQL = @"declare @SDt nvarchar(20);
                        declare @EDt nvarchar(20);
                        declare @PSRN int=0,@NUMS int=0,@APRL int=0,@UDWR int=0,@UDWRApprove int=0,@DISPUDWR int=0,@DISPProduct int=0;
                        declare @returnString nvarchar(200);
                        set @SDt = CONVERT(nvarchar(10),GETDATE(),120) + ' 00:00:00';
                        set @EDt = CONVERT(nvarchar(10),GETDATE(),120) + ' 23:59:29';
                        --For本日起案一般件
                        create table #TmpNUMSMaster
                        (
                        ApplNo nvarchar(15),
                        ApplNoB nvarchar(1)
                        );

                        --For變簽件
                        create table #TmpNUMSMasterJ
                        (
                        ApplNo nvarchar(15),
                        ApplNoB nvarchar(1)
                        );";

            //------------------------------------
            //--Step 2.本日起案一般件
            //--請參考程式CTCB.NUMS.Models > QRYNumsCaseBIZ.cs
            //------------------------------------
            strSQL = strSQL + @"--本日起案一般件
                        insert into #TmpNUMSMaster(ApplNo,ApplNoB) 
                        select ApplNo,ApplNoB from NUMSMaster with (nolock)
                        where ApplyDate between @SDt and @EDt and ApplTypeCode <> 'J';

                        --初審處理中之筆數
                        set @PSRN = (
                        select COUNT(0) from #TmpNUMSMaster a
                        left join NUMSMaster n with (nolock) on n.ApplNo = a.ApplNo and n.ApplNoB = a.ApplNoB
                        where n.FlowStep in ('0002','0010','0050','0100','0270','0280','0285','0300')
                        );

                        --鍵檔作業處理中之筆數
                        set @NUMS = (
                        select COUNT(0) from #TmpNUMSMaster a
                        left join NUMSMaster n with (nolock) on n.ApplNo = a.ApplNo and n.ApplNoB = a.ApplNoB
                        where n.FlowStep in ('0550','0680','0700')
                        );

                        --鑑價作業處理中之筆數
                        set @APRL = (
                        select COUNT(0) from #TmpNUMSMaster a
                        left join FlowCaseInformation p on p.ApplNo = a.ApplNo and p.ApplNoB = a.ApplNoB
                        where p.StepID2 in ('A060','A100','A140','A150','A310','A400') and FormNum = '1001'
                        );

                        --徵信作業
                        --徵信中止補件處理中之筆數
                        set @UDWR = (
                        select COUNT(0) from #TmpNUMSMaster a
                        left join NUMSMaster n with (nolock) on n.ApplNo = a.ApplNo and n.ApplNoB = a.ApplNoB
                        left join UDWRDerivedData m  with (nolock)  on m.ApplNo = a.ApplNo and m.ApplNoB = a.ApplNoB
                        where (m.CaseType='UnderUDWR_Processing' and m.LatestApproveSupervisorEmpNo is null) or
                        (n.FlowStep = '2610' and m.LatestApproveAuditResult in ('P','S') )
                        );

                        --徵/授信主管審核作業處理中之筆數
                        set @UDWRApprove = (
                        select COUNT(0) from #TmpNUMSMaster a
                        left join NUMSMaster n with (nolock) on n.ApplNo = a.ApplNo and n.ApplNoB = a.ApplNoB
                        where n.FlowStep in ('2610','2620','2630')
                        );";
            //------------------------------------
            //--Step 3.變簽
            //--暫不含"變簽鍵檔作業中"
            //--請參考程式CTCB.NUMS.Models > QRYNumsJCaseBIZ.cs
            //------------------------------------
            strSQL = strSQL + @"--本日起案變簽件
                        insert into #TmpNUMSMasterJ(ApplNo,ApplNoB) 
                        select ApplNo,ApplNoB from NUMSMaster with (nolock)
                        where ApplyDate between @SDt and @EDt and ApplTypeCode = 'J';

                        --變簽徵信助理派件('3300')
                        --徵信('3610','3620'),授信主管審核中('3630')
                        --徵信作業處理(CaseType='UnderUDWR_Processing' and LatestApproveSupervisorEmpNo is null)
                        --變簽徵信中止_補件作業處理('3610') and LatestApproveAuditResult in ('P','S')
                        set @DISPUDWR = (
                        select COUNT(0) from #TmpNUMSMasterJ a
                        left join NUMSMaster n with (nolock) on n.ApplNo = a.ApplNo and n.ApplNoB = a.ApplNoB
                        left join UDWRDerivedData m  with (nolock)  on m.ApplNo = a.ApplNo and m.ApplNoB = a.ApplNoB
                        where n.FlowStep in ('3300','3610','3620','3630') or
                        (m.CaseType='UnderUDWR_Processing' and m.LatestApproveSupervisorEmpNo is null) or
                        (n.FlowStep = '3610' and m.LatestApproveAuditResult in ('P','S'))
                        );

                        --產品變簽處理中
                        set @DISPProduct = (
                        select COUNT(0) from #TmpNUMSMasterJ a
                        left join NUMSMaster n with (nolock) on n.ApplNo = a.ApplNo and n.ApplNoB = a.ApplNoB
                        where n.FlowStep in ('3210')
                        );";
            //------------------------------------
            //--Step 4.組合並查詢最後字串,ex:PSRN=15,NUMS=19,APRL=24,UDWR=24,UDWRApprove=1,DISPUDWR=15,DISPProduct=15
            //------------------------------------
            strSQL = strSQL + @"set @returnString = 'PSRN=' + convert(varchar(5),isnull(@PSRN,0)) + ',' + 
                        'NUMS=' + convert(varchar(5),isnull(@NUMS,0)) + ',' +
                        'APRL=' + convert(varchar(5),isnull(@APRL,0)) + ',' +
                        'UDWR=' + convert(varchar(5),isnull(@UDWR,0)) + ',' +
                        'UDWRApprove=' + convert(varchar(5),isnull(@UDWRApprove,0)) + ',' +
                        'DISPUDWR=' + convert(varchar(5),isnull(@DISPUDWR,0)) + ',' +
                        'DISPProduct=' + convert(varchar(5),isnull(@DISPProduct,0))
                        select @returnString as CaseCheckString;";
            //------------------------------------
            //--Step 5.釋放Temp Table
            //------------------------------------
            strSQL = strSQL + @"drop table #TmpNUMSMasterJ;
                        drop table #TmpNUMSMaster";
            //------------------------------------
            //--End
            //------------------------------------
            try
            {
                base.Parameter.Clear();
                IDataReader dr = base.ExecuteReader(strSQL);
                while (dr.Read())
                {
                    returnValue = dr.GetString(0);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return returnValue;
        }

        //add by 線上展延與寬限期 shenqixing  20161221 start 
        /// <summary>
        /// 產出網銀變簽件進度
        /// </summary>
        /// <returns></returns>
        public DataTable WEBBankDISPInfoData(string retType)
        {
            // modify by shenqixing 20170118 start
            /*
            string strSQL = @";with T1 as --取得符合客戶資訊的案件
                                        (
                                        select distinct ApplNo,ApplNoB,CusId,CusName,MobileTel
                                        ,cast((isnull((select top 1 CodeDesc from  PARMCode where  CodeType='DISP_WEBBANK' and CodeNo='01' and Enable=1),'0')) as int) MonthCount
                                        from dbo.NUMSCustomerInfo
                                        ),T2 as --取得案件資訊
                                        (
                                        SELECT 
                                        NUMSMaster.[ApplNo]
                                        ,T1.CusId
                                        ,T1.CusName
                                        ,T1.MobileTel
                                        ,[FlowStep] --可判斷是否在完整鍵檔,或中止補件
                                        ,(select top 1 isClose from UDWRDerivedData where UDWRDerivedData.ApplNo = NUMSMaster.ApplNo and UDWRDerivedData.ApplNoB = NUMSMaster.ApplNoB) isClose --如果有值 就是在徵信,再用Y或N 代表是否結案
                                        ,(select top 1 ExamineResult from PSRNExamine where PSRNExamine.ApplNo = NUMSMaster.ApplNo and PSRNExamine.ApplNoB = NUMSMaster.ApplNoB order by ModifiedDate desc) ExamineResult --初審的結果
                                        ,(select top 1 AuditResult from UDWRMaster where UDWRMaster.ApplNo = NUMSMaster.ApplNo and UDWRMaster.ApplNoB = NUMSMaster.ApplNoB) AuditResult --徵審的結果
                                        ,isnull((select top 1 ApplUDWRSubType from DISPMaster DISP  where  DISP.ApplNo=NUMSMaster.ApplNo and DISP.ApplNoB=NUMSMaster.ApplNoB),'') DISPSubTypeNo--取得DISPSubTypeNo modify by  shenqixing 20170104
                                        ,(case when dateadd(month,T1.MonthCount,ApplyDate)<getdate()then 'Y' else 'N' end) ISexpired
                                        ,(case when isnull((select top 1 ApproveDate from DISPMaster where DISPMaster.ApplNo=NUMSMaster.ApplNo and DISPMaster.ApplNoB=NUMSMaster.ApplNoB),null) is null then 'N' else 'Y' end) IsMasterSET
                                        ,(case when isnull((select top 1 ApplNo from DISPMaster where DISPMaster.ApplNo=NUMSMaster.ApplNo and DISPMaster.ApplNoB=NUMSMaster.ApplNoB and  DISPMaster.ApplNo like '%EJ%'),null) is null then 'N' else 'Y' end) IsVariablesign  --是否为變簽件 modify by  shenqixing 20170105
                                        ,(select top 1 IsUploadFlag from DISPMaster where DISPMaster.ApplNo=NUMSMaster.ApplNo and DISPMaster.ApplNoB=NUMSMaster.ApplNoB) IsUploadFlag-- add by shenqixing 20170118 判斷是否已經上傳簡訊
                                        FROM [dbo].[NUMSMaster]
                                        inner join T1 on T1.ApplNo = NUMSMaster.ApplNo and T1.ApplNoB = NUMSMaster.ApplNoB
                                        ),T3 as --處理進度跟結果
                                        (
                                        select 
                                        [ApplNo]
                                        ,CusId
                                        ,CusName
                                        ,MobileTel
                                        ,[FlowStep]
                                        ,case when isClose is not null and isClose <> '' then 'C' when FlowStep = '0550' then 'B' when  IsVariablesign = 'Y' then 'D' else 'A' end ReturnCode --先判斷徵信 再判斷完整建檔,再判斷變簽 最後是初審
                                        ,isClose
                                        ,DISPSubTypeNo
                                        ,ExamineResult
                                        ,AuditResult
                                        ,ISexpired
                                        ,IsMasterSET
                                        ,IsUploadFlag
                                        from T2
                                        ),T4 as  --處理進度跟結果
                                        (
                                        select 
                                        [ApplNo]
                                        ,CusId
                                        ,CusName
                                        ,MobileTel
                                        ,[FlowStep]
                                        ,ReturnCode 
                                        -- 增加AuditResult為F E的判斷 modify by shenqixing 20161219
                                        ,case when isClose = 'Y' then AuditResult when FlowStep = '2330' then AuditResult when ISexpired = 'Y'  then 'F' when  IsMasterSET = 'Y' then 'E' when ReturnCode = 'A'  and FlowStep = 'E' and ExamineResult = '2' then 'D' else '0' end AuditResult
                                        ,DISPSubTypeNo
                                        ,IsUploadFlag
                                        from T3
                                        )
                                        select [ApplNo]
                                        ,CusId
                                        ,CusName
                                        ,MobileTel
                                        ,ReturnCode --先判斷徵信 再判斷完整建檔 最後是初審 
                                        ,AuditResult
                                        ,DISPSubTypeNo
                                        ,IsUploadFlag
                                        from T4 where AuditResult=@AuditResult  and isnull(IsUploadFlag,'0')='0' ";
            */
            string strSQL = @" select distinct M.ApplNo,M.ApplNoB,U.CusId,U.CusName,U.MobileTel,d.ApplUDWRSubType AS DISPSubTypeNo, IsUploadFlag
                               from UDWRMaster M
                                     inner join NUMSCustomerInfo U ON M.APPLNO = U.APPLNO AND M.APPLNOB = U.APPLNOB
                                     inner join dispmaster d ON d.APPLNO = U.APPLNO AND d.APPLNOB = U.APPLNOB
                               where M.AuditResult = @AuditResult 
                               and isnull(IsUploadFlag,'0')='0'  
                               and M.APPLNO like '%EJ%' 
                               and d.ApplUDWRSubType in('10','12')
                               AND U.MobileTel IS NOT NULL";
            // modify by shenqixing 20170118 end
            DataTable returnValue = new DataTable();
            try
            {
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@AuditResult", retType));
                returnValue = base.Search(strSQL);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return returnValue;
        }
        //add by 線上展延與寬限期 shenqixing  20161221 end 

        // add by shenqixing 20170118 start
        /// <summary>
        /// 線上展延與寬限期-簡訊名單上傳完成註記
        /// </summary>
        /// <returns></returns>
        public void SetIsUploadFlag(string ApplNo, string ApplNoB)
        {
            //string strSQL = @" update  DISPMaster set IsUploadFlag='1' where  ApplNo=@ApplNo and ApplNoB=@ApplNoB";
            // modify by shenqixing 20170124
            string strSQL = @" update  DISPMaster set IsUploadFlag=Convert(varchar(8),getdate(),112) ,ModifiedDate=getdate() where  ApplNo=@ApplNo and ApplNoB=@ApplNoB";
            try
            {
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@ApplNo", ApplNo));
                base.Parameter.Add(new CommandParameter("@ApplNoB", ApplNoB));
                base.ExecuteNonQuery(strSQL, false);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        // add by shenqixing 20170118 end

        /// <summary>
        /// OpeningCheck2 產生定時錯誤件數量監控狀況Log-part1
        /// </summary>
        /// <returns></returns>
        /// 20140630 horace
        public IList<NUMSHandleErrorCaseListVO> GetErrCase()
        {
            try
            {
                NUMSHandleErrorCaseBIZ _hErrBIZ = new NUMSHandleErrorCaseBIZ(_applController);
                IList<NUMSHandleErrorCaseListVO> rtnList = new List<NUMSHandleErrorCaseListVO>();
                rtnList = _hErrBIZ.GetQueryList();
                return rtnList;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// OpeningCheck2 產生定時錯誤件數量監控狀況Log-part2
        /// </summary>
        /// <returns></returns>
        /// 20140630 horace
        public IList<NUMSHandleErrorCaseListVO> GetPersonHandleCase()
        {
            try
            {
                NUMSHandleErrorCaseBIZ _hErrBIZ = new NUMSHandleErrorCaseBIZ(_applController);
                IList<NUMSHandleErrorCaseListVO> rtnList = new List<NUMSHandleErrorCaseListVO>();
                rtnList = _hErrBIZ.PersonHandleList();
                return rtnList;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        /// <summary>
        /// OpeningCheck2 產生定時案件數量監控狀況Log
        /// </summary>
        /// <returns>舉例為:PSRN=15,NUMS=19,APRL=24,UDWR=24,UDWRApprove=1,DISPUDWR=15,DISPProduct=15</returns>
        /// 20140630 horace
        public string CheckFlowStepId(string strApplno, string strApplnoB)
        {
            string strSQL = "";
            string returnValue = "";

            //------------------------------------
            //--Step 1.宣告變數與Temp Table
            //------------------------------------
            strSQL = @"select top 1 FlowStep from NUMSMaster where ApplNo=@strApplno and ApplNoB=@strApplnoB ";
            try
            {
                base.Parameter.Clear();

                base.Parameter.Add(new CommandParameter("@strApplno", strApplno));
                base.Parameter.Add(new CommandParameter("@strApplnoB", strApplnoB));
                IDataReader dr = base.ExecuteReader(strSQL);
                while (dr.Read())
                {
                    returnValue = dr.GetString(0);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return returnValue;
        }

        //        public DataTable GetDataForTotalAmt2(string strApplno, string strApplnoB)
        //        {

        //            string strSQL = @"select a.ApplNo ,a.ApplNoB,a.CusId,a.NFOSeq
        //                                ,Cast(isnull(a.AppyAmt,0) as numeric(15,0))*1000 AppyAmt
        //                                ,Cast(isnull(a.CurrBal,0) as numeric(15,0)) CurrBal
        //                                ,Cast(isnull(a.RecParAmt,0)  as numeric(15,0))*10000 RecParAmt
        //                                ,a.AcctStatus
        //                                ,isnull(a.RecPartial,'N') RecPartial
        //                                ,isnull(a.CalAmt ,'N') CalAmt
        //                                ,a.AccTypeCode
        //                                ,isnull(b.FinancialOverdraft,'N') FinancialOverdraft
        //                                 from UDWRNewForOld  a 
        //                                 left outer join 
        //                                 (select distinct AcctTypeCode,FinancialOverdraft from  PARMBankAcctTypeCode ) b 
        //                                 on a.AccTypeCode =b.AcctTypeCode
        //                                 left outer join NUMSCustomerInfo c on a.ApplNo =c.ApplNo and a.ApplNoB =c.ApplNoB and a.CusId=c.CusId
        //                                 where a.ApplNo =@ApplNo and a.ApplNoB=@ApplNoB
        //                                 and isnull(a.CalAmt ,'Y')='N' and ProdDesc not in ('代繳卡款') and AcctStatus not in ('結清','婉拒')
        //                                 and ISNULL( c.Status,'Y') ='Y'
        //                                 order by CusId,NFOSeq";

        //            DataTable returnValue = new DataTable();
        //            try
        //            {
        //                base.Parameter.Clear();
        //                base.Parameter.Add(new CommandParameter("@ApplNo", strApplno));
        //                base.Parameter.Add(new CommandParameter("@ApplNoB", strApplnoB));
        //                returnValue = base.Search(strSQL);

        //            }
        //            catch (Exception ex)
        //            {
        //                throw ex;
        //            }

        //            return returnValue;

        //        }

    }
}
