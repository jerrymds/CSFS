using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CTBC.CSFS.BussinessLogic;
using CTBC.CSFS.Pattern;
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
using CTBC.WinExe.WarningReturnFile.Model;
using System.Globalization;

namespace CTBC.WinExe.WarningReturnFile
{
    public class ReturnFileBiz : BaseBusinessRule
    {

        // 半形空白變量
        public string strNull = " ";


        internal bool updateFixSend(int SerialID, string Status)
        {
            string sql = @"UPDATE WarningDetails Set FixSend=@Status WHERE SerialID=@SerialID";
            base.Parameter.Clear();
            base.Parameter.Add(new CommandParameter("@SerialID", SerialID));
            base.Parameter.Add(new CommandParameter("@Status", Status));
            return base.ExecuteNonQuery(sql) > 0;
        }

        internal bool holdDetails()
        {
            string sql = @"UPDATE WarningDetails set FixSend='9' WHERE STATUS='Z01' AND FixSend is null ";

            return base.ExecuteNonQuery(sql) > 0;
        }

        internal List<wd> getDetails()
        {
            string sql = @"SELECT * FROM WarningDetails WHERE STATUS='Z01' AND FixSend is null ";
            
            return base.SearchList<wd>(sql).ToList();
        }

        internal DataTable getParm()
        {
            string sql = @"SELECT * FROM PARMCode WHERE  codetype='WarningParm' Order by SortOrder ";

            return base.Search(sql);
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

        public int ExcuteSQL(string sql)
        {
            IDbConnection dbConnection = base.OpenConnection();
            IDbTransaction dbTransaction = null;
            using (dbConnection)
            {
                try
                {
                    // 開始事務
                    dbTransaction = dbConnection.BeginTransaction();

                    base.ExecuteNonQuery(sql, dbTransaction, false);

                    dbTransaction.Commit();
                }
                catch (Exception ex)
                {
                    dbTransaction.Rollback();

                    return 0;
                }
            }

            return 1;
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

        /// <summary>
        /// 字串轉換,長度不夠右側補空白
        /// </summary>
        /// <param name="strValue">指定字串</param>
        /// <param name="strValueLen">指定長度</param>
        /// <returns></returns>
        public string ChangeValue(string strValue, int strValueLen)
        {
            if (!string.IsNullOrEmpty(strValue))
            {
                // 差值 = 字串指定長度-字串實際長度
                int strNullNumber = strValueLen - strValue.Length;

                // strNullNumber>0,就在字串後拼接strNullNumber個半形空白,否則就截取指定長度
                strValue = strNullNumber > 0 ? strValue + AddSpace(strNullNumber, strNull) : strValue.Substring(0, strValueLen);
            }
            else
            {
                // 拼接strValueLen個半形空白
                strValue += AddSpace(strValueLen, strNull);
            }
            return strValue;
        }

        /// <summary>
        /// 補充指定長度的空格
        /// </summary>
        /// <param name="strSpaceLen">指定長度</param>
        /// <returns></returns>
        public string AddSpace(int strSpaceLen, string flag)
        {
            string result = "";

            // 拼接strNullNumber個半形空白
            for (int m = 0; m < strSpaceLen; m++)
            {
                result += flag;
            }

            return result;
        }

        internal string getEmployee(string eQueryStaff)
        {

            LdapEmployeeBiz agentBiz = new LdapEmployeeBiz();
            LDAPEmployee agent = agentBiz.GetAllEmployeeInEmployeeViewByEmpId(eQueryStaff);
            return agent.EmpName;
        }

        internal string ouputTxt(wd d,  DataTable dtParm)
        {

            DateTime thenow = DateTime.Now;
            CultureInfo culture = new CultureInfo("zh-TW");
            culture.DateTimeFormat.Calendar = new TaiwanCalendar();
            string emp = getEmployee(d.CreatedUser).Substring(0, 1) + "小姐";
            string Tel = dtParm.Rows[3]["CodeDesc"].ToString();
            wm m = getWarningMaster(d.DocNo);

            StringBuilder sb = new StringBuilder();
            sb.Append(ChangeValue(d.DocNo, 10)); // 流水號
            sb.Append(thenow.ToString("yyyMMdd", culture)); // 發佈日期
            sb.Append(thenow.ToString("HHmm"));  // 發佈時間
            sb.Append(ChangeValue(dtParm.Rows[0]["CodeDesc"].ToString(), 7)); // 機構代號
            sb.Append(ChangeValue(dtParm.Rows[1]["CodeDesc"].ToString(), 40)); // 機構名
            sb.Append(ChangeValue(dtParm.Rows[2]["CodeDesc"].ToString(), 2)); // 案件類別
            sb.Append(ChangeValue(dtParm.Rows[2]["CodeDesc"].ToString(), 32)); // 類別名稱
            sb.Append(ChangeValue(emp, 12)); // 承辦人
            sb.Append(ChangeValue(Tel, 20)); // 承辦人電話
            sb.Append(d.HappenDateTime.ToString("yyyMMdd", culture)); // 案發日期
            sb.Append(d.HappenDateTime.ToString("HHmm"));  // 案發時間
            sb.Append(ChangeValue(d.DocAddress, 80)); // 案發地點
            sb.Append(ChangeValue(d.PoliceStation, 40)); // 警局

            sb.Append(ChangeValue(m.CustName, 10)); // 開戶人
            sb.Append(ChangeValue(m.CustId, 60)); // 中文戶名
            sb.Append(ChangeValue(m.CustAccount, 50)); // 帳號
            sb.Append(ChangeValue(" ", 80)); // 單據種類與號碼
            sb.Append(ChangeValue("", 160)); // 特徵
            sb.Append(ChangeValue("", 160)); // 事項
            string caseDesc = string.Empty;
            if (d.Retry!="1") { //  一般案件
                caseDesc = "本行接獲 " + d.PoliceStation + " 傳真受理詐騙帳戶通報警示簡便格式表、報案三聯單/案件證明單，要求對該帳戶設立警示";
            }
            else
            {
                caseDesc = "本行接獲 " + d.PoliceStation + " 傳真受理詐騙帳戶通報警示金融機構聯訪機制通知單，要求對該帳戶設立警示";
            }

            sb.Append(ChangeValue(caseDesc, 600)); // 案情描述


            string caseStatus = "A";
            if (d.Extend == "1") // 延長
                caseStatus = "P";

            sb.Append(ChangeValue(caseStatus, 1)); // 狀態


            string updDateTime = thenow.ToString("yyyMMddhhmmss", culture);
            sb.Append(ChangeValue(updDateTime, 13));

            string ref_coe = " ";
            // 若是解除, 要去WarningStatus 中, 去找那筆的的狀態.....
            sb.Append(ChangeValue(ref_coe, 1)); // 警示帳戶解除代碼

            sb.Append(ChangeValue(d.No_165, 12)); // 165案號


            // 用 select * from BOPS067050Recv where VersionNewID=dr["NewID"] 找到67050 的其他身分...

            string s67040 = @"select TOP 1 * from BOPS067050Recv where VersionNewID='" + d.NewId + "'";
            var v67050 = base.Search(s67040);
            if (v67050.Rows.Count > 0)
            {
                // 25 找BOPS067050Recv.DATE_OF_BIRTH ==> 生日
                string birth = v67050.Rows[0]["DATE_OF_BIRTH"].ToString(); // 日月年格式.. 要轉民國年 01011976
                string strYear = (int.Parse(birth.Substring(4, 4))-1911).ToString().PadLeft(3,'0');
                string strMon = birth.Substring(2, 2);
                string strday = birth.Substring(0, 2);
                sb.Append(ChangeValue(strYear + strMon + strday, 7)); // 生日
                // 26 找BOPS067050Recv.NIGTEL_NO  ==> 晚上電話
                sb.Append(ChangeValue(v67050.Rows[0]["NIGTEL_NO"].ToString(), 16)); // 晚上電話
                // 27   BOPS067050Recv.MOBIL_NO ==> 行動
                sb.Append(ChangeValue(v67050.Rows[0]["MOBIL_NO"].ToString(), 12));
                // 28 聯絡地址 BOPS067050Recv.COMM_ADDR1  COMM_ADDR2 COMM_ADDR3 COMM_ADDR4
                string addr = (v67050.Rows[0]["COMM_ADDR1"].ToString() + v67050.Rows[0]["COMM_ADDR2"].ToString() + v67050.Rows[0]["COMM_ADDR3"].ToString() + v67050.Rows[0]["COMM_ADDR4"].ToString()).Trim();

                if( addr.Length > 99)
                    addr = addr.Substring(0, 99);
                sb.Append(ChangeValue(addr, 100));
            }

            // 29, 更改ID
            // 30 訂正案號
            if (!string.IsNullOrEmpty(m.CustId_Old)) // ID變更
            {
                sb.Append(ChangeValue("Y", 1));
                sb.Append(ChangeValue(d.UniteNo_Old.Substring(0,10), 10));
            }
            else
            {
                sb.Append(ChangeValue(" ", 1));
                sb.Append(ChangeValue(" ", 10));
            }

            return sb.ToString();
        }

        internal wm getWarningMaster(string p)
        {
            string sql = "SELECT * from WarningMaster where [DocNo]='" + p + "'";
            var ddd = base.SearchList<wm>(sql);
            return base.SearchList<wm>(sql).First();
        }

        internal string getFileName(string outputDir)
        {
            // 先開今日的目錄...
            if (!outputDir.EndsWith("\\"))
                outputDir += "\\";

            string todayDir = outputDir +  DateTime.Now.ToString("yyyyMMdd");
            string filename = todayDir + "\\";
            if (!Directory.Exists(todayDir))
            {
                Directory.CreateDirectory(todayDir);
                filename += "822" + DateTime.Now.ToString("MMdd") + "A" + ".bf1";
            }
            else {
                // 找出最後一個檔名...
                DirectoryInfo di = new DirectoryInfo(todayDir);
                FileInfo fi = di.GetFiles("*.bf1").OrderByDescending(x => x.FullName).FirstOrDefault();
                if (fi == null)
                {
                    filename += "822" + DateTime.Now.ToString("MMdd") + "A" + ".bf1";
                }
                else
                {
                    char[] arrayChar = fi.Name.ToCharArray();
                    char lastchar = arrayChar[fi.Name.Length - 5];

                    arrayChar[fi.Name.Length - 5] = (char)((int)lastchar + 1);
                    
                    filename += new string(arrayChar);
                }
            }

            return filename;
            
        }


    }
}
