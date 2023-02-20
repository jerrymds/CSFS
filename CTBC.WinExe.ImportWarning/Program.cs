using CTBC.CSFS.BussinessLogic;
using CTBC.FrameWork.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTBC.WinExe.ImportWarning
{
    
    class Program
    {
        private static FileLog m_fileLog;
        private static string ftpserver;
        private static string port;
        private static string username;
        private static string password;
        private static string ftpdir;
        private static string loaclFilePath;
        private static FtpClient ftpClient;

        static void Main(string[] args)
        {
            string ftp = ConfigurationManager.AppSettings["ftp"].ToString();
            port = ConfigurationManager.AppSettings["port"];
            ftpserver = ConfigurationManager.AppSettings["ftpserver"];
            username = ConfigurationManager.AppSettings["username"];
            password = ConfigurationManager.AppSettings["password"];
            ftpdir = ConfigurationManager.AppSettings["ftpdir"];
            loaclFilePath = ConfigurationManager.AppSettings["loaclFilePath"];
            m_fileLog = new FileLog(ConfigurationManager.AppSettings["filelog"], AppDomain.CurrentDomain.FriendlyName.Replace(".vshost", "").Replace(".exe", ""));
            //fileTypes = ConfigurationManager.AppSettings["fileTypes"].Split(',');
            bool importG01 = bool.Parse( ConfigurationManager.AppSettings["importG01"].ToString());
            bool importG20 = bool.Parse(ConfigurationManager.AppSettings["importG20"].ToString());
            bool importG36 = bool.Parse(ConfigurationManager.AppSettings["importG36"].ToString());
            bool importSUSMX = bool.Parse(ConfigurationManager.AppSettings["importSUSMX"].ToString());
            bool merge = bool.Parse(ConfigurationManager.AppSettings["merge"].ToString());


            bool isFTP = false;
            if( !string.IsNullOrEmpty(ftp) )
            {
                isFTP = bool.Parse(ftp);
            }



            string[] fileNames;
            if (isFTP)
            {
                ftpClient = new FtpClient(ftpserver, username, password, port);
                 fileNames = getFileList(); // 取得FTP上的檔案
            }
            else
            {
                DirectoryInfo di = new DirectoryInfo(loaclFilePath);
                fileNames = di.GetFiles("*.TXT").Select(x=>x.FullName).ToArray();
            }

            if(fileNames.Count()==0)
            {

                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, loaclFilePath + "目錄中, 沒有TXT檔案!");
                //Console.WriteLine("{0}目錄中, 沒有TXT檔案!", loaclFilePath);
                //Console.WriteLine("按ENTER 離開");
                //Console.ReadLine();
                return;
            }
            
            //if ((args.Length<1))
            //{
            //    Console.WriteLine("請輸入匯入的檔案格式: ( GL01 / GL20 / GL36 / TX20480 / MERGE");
            //    Console.WriteLine("使用方法: ImportWarning.exe <GL01|GL20|GL36|TX20480|MERGE> ");
            //    Console.WriteLine("按ENTER鍵結束");
            //    Console.ReadLine();
            //    return;
            //}

            //var lstfileNames = ;

            var lstFiles = fileNames.ToList().Where(x => x.EndsWith("GL01X.TXT")).ToList(); 
            if (lstFiles.Count()>0 && importG01)
            {
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息-------- 開始匯入GL01----------------");
                try
                {
                    //從fileNames中, 取出GL01的檔案
                    foreach(var g in lstFiles)
                        GL01(g);
                }
                catch (Exception ex)
                {
                    m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "發生錯誤" + ex.Message.ToString());
                }
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息-------- 匯入GL01結束----------------");
            }
            else
            {
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "找不到GL01X.TXT檔案");
            }

            lstFiles = fileNames.ToList().Where(x => x.EndsWith("GL20X.TXT")).ToList();
            if (lstFiles.Count() > 0 && importG20)
            {
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息-------- 開始匯入GL20----------------");
                try
                {
                    //從fileNames中, 取出GL20的檔案
                    foreach (var g in lstFiles)
                        GL20(g);
                }
                catch (Exception ex)
                {
                    m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "發生錯誤" + ex.Message.ToString());
                }
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息-------- 匯入GL20結束----------------");
            }
            else
            {
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "找不到GL20X.TXT檔案");
            }

            lstFiles = fileNames.ToList().Where(x => x.EndsWith("GL36X.TXT")).ToList();
            if (lstFiles.Count() > 0 && importG36)
            {  
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息-------- 開始匯入GL36----------------");            
                try
                {
                    //從fileNames中, 取出GL36的檔案
                    foreach (var g in lstFiles)
                        GL36(g);
                }
                catch (Exception ex)
                {
                    m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "發生錯誤" + ex.Message.ToString());
                }
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息-------- 匯入GL36結束----------------");
            }
            else
            {
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "找不到GL36X.TXT檔案");
            }

            lstFiles = fileNames.ToList().Where(x => x.EndsWith("SUSMX.TXT")).ToList();
            if (lstFiles.Count() > 0 && importSUSMX)
            {
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息-------- 開始匯入歷史TX20480----------------");
                try
                {
                    //從fileNames中, 取出GL20的檔案
                    foreach (var g in lstFiles)
                        TX20480(g);
                }
                catch (Exception ex)
                {
                    m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "發生錯誤" + ex.Message.ToString());
                }
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息-------- 匯入歷史TX20480結束----------------");
            }
            else
            {
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "找不到SUSMX.TXT檔案");
            }


            if (merge)
            {
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息-------- 開始MERGE----------------");
                try
                {

                    MERGE();
                }
                catch (Exception ex)
                {
                    m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "發生錯誤" + ex.Message.ToString());
                }
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息-------- MERGE結束----------------");
            }
        }
        /// <summary>
        /// 合併GL01, GL20, GL36 到GL
        /// </summary>
        private static void MERGE()
        {
            //var srcdata = CreateGL();
            //var dupdata = CreateGL();




            var myconnection = ConfigurationManager.ConnectionStrings["CSFS_ADO"].ToString();
            using (SqlConnection conn = new SqlConnection(myconnection))
            {
                conn.Open();
                KDSql kd = new KDSql();
//                string strSQL = @"
//SELECT g20.CHQ_PAYEE,g01.*,g36.ACCOUNT_NO,g36.[SYSTEM], g36.DESCR FROM [CSFS_UAT].[dbo].[WarningGL20] g20 inner join [CSFS_UAT].[dbo].[WarningGL01] g01 on g01.JRNL_NO=g20.JRNL_NO 
//inner join [CSFS_UAT].[dbo].[WarningGL36] g36 on g36.JRNL_NO=g20.JRNL_NO  ";
                string strSQL = @"
with g20 as (
select JRNL_NO,CHQ_PAYEE  from [dbo].[WarningGL20] group by JRNL_NO,CHQ_PAYEE
)
SELECT g01.INST_NO,g01.HOME_BRCH,g01.ACCT_NO,g01.ACT_DATE_TIME,g01.ACT_DATE,g01.ACT_CCYY,g01.ACT_MM,g01.ACT_DD,g01.ACT_TIME,g01.TRAN_TYPE,g01.TRAN_STATUS,g01.TRAN_DATE,g01.BRANCH,g01.BRANCH_TERM,g01.TELLER,g01.TRAN_CODE,g01.POST_DATE,g01.JRNL_NO,g01.AMOUNT,g01.BTCH_NO_U,g01.CORRECTION,g01.DEFER_DAYS,g01.BALANCE,g01.FOREIGN_FLAG,g01.FILLER,g01.CreatedId,g01.CreatedTime,g20.CHQ_PAYEE,g36.ACCOUNT_NO,g36.[SYSTEM], g36.DESCR FROM [dbo].[WarningGL01] g01 
inner join [dbo].[WarningGL36] g36 on g36.JRNL_NO=g01.JRNL_NO and g01.AMOUNT=g36.AMOUNT
inner  join  g20 on g01.JRNL_NO=g20.JRNL_NO  ";
                var dt = kd.getDataTable(strSQL);

                string strSQL12 = @"INSERT INTO [dbo].[WarningGenAcct]
([INST_NO],[HOME_BRCH],[ACCT_NO],[ACT_DATE_TIME],[ACT_DATE],[ACT_CCYY],[ACT_MM],[ACT_DD],[ACT_TIME],[TRAN_TYPE],[TRAN_STATUS],[TRAN_DATE],[BRANCH],[BRANCH_TERM],[TELLER],[TRAN_CODE],[POST_DATE],[JRNL_NO],[AMOUNT],[BTCH_NO_U],[CORRECTION],[DEFER_DAYS],[BALANCE],[FOREIGN_FLAG],[FILLER],[CreatedId],[CreatedTime],[CHQ_PAYEE],[ACCOUNT_NO],[SYSTEM],[DESCR]) VALUES
('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}','{12}','{13}','{14}','{15}','{16}','{17}','{18}','{19}','{20}','{21}','{22}','{23}','{24}','{25}','{26}','{27}','{28}','{29}','{30}')";

                List<string> lstSQL = new List<string>();
                //20190419, 要加入歷史的TX20480的Balance 那個欄位值... 否則會是GL01的Balance的值, 是整統的值不是各銷帳碼的值
                foreach (DataRow dr in dt.Rows)
                {
                    string ref_code = dr["CHQ_PAYEE"].ToString().Trim();
                    string acc_no = dr["ACCT_NO"].ToString().Trim();
                    string tx_date = dr["TRAN_DATE"].ToString().Trim();


                    string sql1 = string.Format(@"  select TOP 1 * from [dbo].[WarningTX20480] where ACCT_NO='{0}' and REF_CODE like '%{1}%' and OPEN_DATE='{2}'",acc_no,ref_code, tx_date);

                    DataTable dt20480 = kd.getDataTable(sql1);

                    string newBalance = "";
                    if (dt20480.Rows.Count > 0)
                    {
                        //dr["BALANCE"] = dt20480.Rows[0]["OUTSTANDING_AMT"];
                        newBalance = dt20480.Rows[0]["OUTSTANDING_AMT"].ToString().Trim();
                    }
                    var stramt = dr["AMOUNT"];
                    string strSQL1 = string.Format(strSQL12, dr["INST_NO"], dr["HOME_BRCH"], dr["ACCT_NO"], dr["ACT_DATE_TIME"], dr["ACT_DATE"], dr["ACT_CCYY"], dr["ACT_MM"], dr["ACT_DD"], dr["ACT_TIME"], dr["TRAN_TYPE"], dr["TRAN_STATUS"], dr["TRAN_DATE"], dr["BRANCH"], dr["BRANCH_TERM"], dr["TELLER"], dr["TRAN_CODE"], dr["POST_DATE"], dr["JRNL_NO"], dr["AMOUNT"], dr["BTCH_NO_U"], dr["CORRECTION"], dr["DEFER_DAYS"], newBalance, dr["FOREIGN_FLAG"], dr["FILLER"], dr["CreatedId"], DateTime.Now.ToString("yyyyMMdd hh:mm:ss"), dr["CHQ_PAYEE"].ToString().Trim(), dr["ACCOUNT_NO"], dr["SYSTEM"], dr["DESCR"]);
                    lstSQL.Add(strSQL1);
                    //else
                    //{
                    //    // 如果舊TX20480找不到, 則顯示空值
                    //    dr["BALANCE"] = "";
                    //}
                }




                //List<string> lstSQL = new List<string>();
                //foreach (DataRow dr in dt.Rows)
                //{
                //   strSQL1 = string.Format(strSQL1, dr["INST_NO"], dr["HOME_BRCH"], dr["ACCT_NO"], dr["ACT_DATE_TIME"], dr["ACT_DATE"], dr["ACT_CCYY"], dr["ACT_MM"], dr["ACT_DD"], dr["ACT_TIME"], dr["TRAN_TYPE"], dr["TRAN_STATUS"], dr["TRAN_DATE"], dr["BRANCH"], dr["BRANCH_TERM"], dr["TELLER"], dr["TRAN_CODE"], dr["POST_DATE"], dr["JRNL_NO"], dr["AMOUNT"], dr["BTCH_NO_U"], dr["CORRECTION"], dr["DEFER_DAYS"], dr["BALANCE"], dr["FOREIGN_FLAG"], dr["FILLER"], dr["CreatedId"], DateTime.Now.ToString("yyyyMMdd hh:mm:ss"),dr["CHQ_PAYEE"],dr["ACCOUNT_NO"],dr["SYSTEM"],dr["DESCR"]);
                //   lstSQL.Add(strSQL1);
                //}

                using (NewsEntities ctx = new NewsEntities())
                {
                   foreach (var s in lstSQL)
                   {
                      int noOfRowInserted = ctx.Database.ExecuteSqlCommand(s);
                   }
                }

                //trans.Commit();
            }
        }

        private static void TX20480(string fn)
        {
            List<int> posG20 = new List<int>()
            {
                1,4,20,40,42,50,58,77
            };
            List<string> lstSQL = new List<string>();
            List<WarningTX20480> lstG01 = new List<WarningTX20480>();
            using (StreamReader sr = new StreamReader(fn, Encoding.Default))
            {
                while (sr.Peek() > 0)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append(@"INSERT INTO [dbo].[WarningTX20480]
           ([INST_NO]           
           ,[ACCT_NO]
           ,[REF_CODE]
           ,[STATUS]
           ,[OPEN_DATE]
           ,[LAST_UPD_DATE]
           ,[OUTSTANDING_AMT]
           ,[CreatedId]
           ,[CreatedTime])
     VALUES
           (");
                    WarningTX20480 g20 = new WarningTX20480();
                    List<string> spStr = new List<string>();
                    string line = sr.ReadLine();
                    if (string.IsNullOrEmpty(line))
                        continue;
                    if (line.Length < 10)
                        continue;
                    int totalLen = line.Length;
                    int i = 0;
                    int prevPos = 0;
                    foreach (var p in posG20)
                    {
                        if (i == 0)
                        {
                            prevPos = p;
                            i++;
                            continue;
                        }
                        if (p > totalLen + 1)
                        {
                            sb.Append(string.Format("'{0}',", line.Substring(prevPos - 1)));
                            break;
                        }
                        //spStr.Add(line.Substring(prevPos - 1, p - prevPos));
                        sb.Append(string.Format("'{0}',", line.Substring(prevPos - 1, p - prevPos)));
                        prevPos = p;
                        i++;
                    }
                    string sqlstr = sb.ToString();
                    sqlstr = sqlstr.Substring(0, sqlstr.Length - 1) + ",'Sys', GETDATE());";
                    lstSQL.Add(sqlstr);
                    //Console.WriteLine(sqlstr);
                    //lstG01.Add(g01);
                }
            }

            {
                //Console.WriteLine(s);

            }
            using (NewsEntities ctx = new NewsEntities())
            {
                foreach (var s in lstSQL)
                {
                    int noOfRowInserted = ctx.Database.ExecuteSqlCommand(s);
                }
                //int noOfRowInserted1 = ctx.Database.ExecuteSqlCommand("update [CSFS_UAT].[dbo].[WarningGL20] set ACT_DATE_TIME = ACT_CCYY + ACT_MM + ACT_DD + ACT_TIME");

                //int noOfRowInserted2 = ctx.Database.ExecuteSqlCommand("update [CSFS_UAT].[dbo].[WarningGL20] set ACT_DATE = ACT_CCYY + ACT_MM + ACT_DD");

            }
        }


        private static void GL20(string fn)
        {
            List<int> posG20 = new List<int>()
            {
                1,4,8,24,28,30,32,44,46,48,56,63,113,166
            };
            List<string> lstSQL = new List<string>();
            List<WarningGL01> lstG01 = new List<WarningGL01>();
            using (StreamReader sr = new StreamReader(fn, Encoding.Default))
            {
                while (sr.Peek() > 0)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append(@"INSERT INTO [dbo].[WarningGL20]
           ([INST_NO]
           ,[HOME_BRCH]
           ,[ACCT_NO]
           ,[ACT_CCYY]
           ,[ACT_MM]
           ,[ACT_DD]
           ,[ACT_TIME]
           ,[CODE]
           ,[STATUS]
           ,[POST_DATE]
           ,[JRNL_NO]
           ,[CHQ_PAYEE]
           ,[FILLER]
           ,[CreatedId]
           ,[CreatedTime])
     VALUES
           (");
                    WarningGL20 g20 = new WarningGL20();
                    List<string> spStr = new List<string>();
                    string line = sr.ReadLine();
                    if (string.IsNullOrEmpty(line))
                        continue;
                    if (line.Length < 10)
                        continue;
                    int totalLen = line.Length;
                    int i = 0;
                    int prevPos = 0;
                    foreach (var p in posG20)
                    {
                        if (i == 0)
                        {
                            prevPos = p;
                            i++;
                            continue;
                        }
                        if (p > totalLen + 1)
                        {
                            sb.Append(string.Format("'{0}',", line.Substring(prevPos - 1)));
                            break;
                        }
                           
                        //spStr.Add(line.Substring(prevPos - 1, p - prevPos));
                        sb.Append(string.Format("'{0}',", line.Substring(prevPos - 1, p - prevPos)));
                        prevPos = p;
                        i++;
                    }
                    string sqlstr = sb.ToString();
                    sqlstr = sqlstr.Substring(0, sqlstr.Length - 1) + ",'Sys', GETDATE());";
                    lstSQL.Add(sqlstr);
                    //Console.WriteLine(sqlstr);
                    //lstG01.Add(g01);
                }
            }

            {
                //Console.WriteLine(s);

            }
            using (NewsEntities ctx = new NewsEntities())
            {
                foreach (var s in lstSQL)
                {
                    int noOfRowInserted = ctx.Database.ExecuteSqlCommand(s);
                }
                int noOfRowInserted1 = ctx.Database.ExecuteSqlCommand("update [dbo].[WarningGL20] set ACT_DATE_TIME = ACT_CCYY + ACT_MM + ACT_DD + ACT_TIME");

                int noOfRowInserted2 = ctx.Database.ExecuteSqlCommand("update [dbo].[WarningGL20] set ACT_DATE = ACT_CCYY + ACT_MM + ACT_DD");

            }
        }

        private static void GL01(string fn)
        {
            List<int> posG01 = new List<int>()
            {
                //1,4,8,24,28,30,32,44,46,48,56,60,63,68,75,83,90,109,110,113,130,131,163
                1,4,8,24,28,30,32,44,46,48,56,60,63,68,75,83,90, 109,111,112,115,134,135,167
            };
            List<string> lstSQL = new List<string>();
            List<WarningGL01> lstG01 = new List<WarningGL01>();
            using (StreamReader sr = new StreamReader(fn, Encoding.Default))
            {
                while (sr.Peek() > 0)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append(@"INSERT INTO [dbo].[WarningGL01]
           ([INST_NO]
           ,[HOME_BRCH]
           ,[ACCT_NO]
           ,[ACT_CCYY]
           ,[ACT_MM]
           ,[ACT_DD]
           ,[ACT_TIME]
           ,[TRAN_TYPE]
           ,[TRAN_STATUS]
           ,[TRAN_DATE]
           ,[BRANCH]
           ,[BRANCH_TERM]
           ,[TELLER]
           ,[TRAN_CODE]
           ,[POST_DATE]
           ,[JRNL_NO]
           ,[AMOUNT]
           ,[BTCH_NO_U]
           ,[CORRECTION]
           ,[DEFER_DAYS]
           ,[BALANCE]
           ,[FOREIGN_FLAG]
           ,[FILLER]
           ,[CreatedId]
           ,[CreatedTime])
     VALUES
           (");
                    WarningGL01 g01 = new WarningGL01();
                    List<string> spStr = new List<string>();
                    string line = sr.ReadLine();
                    if (string.IsNullOrEmpty(line) )
                        continue;
                    if (line.Length < 10)
                        continue;
                    int totalLen = line.Length;
                    int i = 0;
                    int prevPos = 0;
                    foreach (var p in posG01)
                    {
                        if (i == 0)
                        {
                            prevPos = p;
                            i++;
                            continue;
                        }
                        if (p > totalLen + 1)
                        {
                            sb.Append(string.Format("'{0}',", line.Substring(prevPos - 1)));
                            break;
                        }
                        //spStr.Add(line.Substring(prevPos - 1, p - prevPos));
                        sb.Append(string.Format("'{0}',", line.Substring(prevPos - 1, p - prevPos)));
                        prevPos = p;
                        i++;
                    }
                    string sqlstr = sb.ToString();
                    sqlstr = sqlstr.Substring(0, sqlstr.Length - 1) + ",'Sys', GETDATE());";
                    lstSQL.Add(sqlstr);
                    //Console.WriteLine(sqlstr);
                    //lstG01.Add(g01);
                }
            }

            {
                //Console.WriteLine(s);

            }
            using (NewsEntities ctx = new NewsEntities())
            {
                foreach (var s in lstSQL)
                {
                    int noOfRowInserted = ctx.Database.ExecuteSqlCommand(s);
                }
                int noOfRowInserted1 = ctx.Database.ExecuteSqlCommand("update  [dbo].[WarningGL01] set ACT_DATE_TIME = ACT_CCYY + ACT_MM + ACT_DD + ACT_TIME");

                int noOfRowInserted2 = ctx.Database.ExecuteSqlCommand("update [dbo].[WarningGL01] set ACT_DATE = ACT_CCYY + ACT_MM + ACT_DD");

            }
        }

        private static void GL36(string fn)
        {
            List<int> posG01 = new List<int>()
            {
                //1,4,8,24,28,30,32,44,46,48,56,65,72,75,76,83,99,118,133,136,190
                1,4,8,24,28,30,32,44,46,48,56,65,72,75,76,83,99,118,133
            };
            List<string> lstSQL = new List<string>();
            List<WarningGL01> lstG01 = new List<WarningGL01>();
            using (StreamReader sr = new StreamReader(fn, Encoding.Default))
            {
                while (sr.Peek() > 0)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append(@"INSERT INTO [dbo].[WarningGL36]
           ([INST_NO]
           ,[HOME_BRCH]
           ,[ACCT_NO]
           ,[ACT_CCYY]
           ,[ACT_MM]
           ,[ACT_DD]
           ,[ACT_TIME]
           ,[CODE]
           ,[STATUS]
           ,[POST_DATE]
           ,[TELL_AND_BR]
           ,[TRN_CODE]
           ,[SYSTEM]
           ,[DESCR]
           ,[JRNL_NO]
           ,[ACCOUNT_NO]
           ,[AMOUNT]
           ,[FILLER1]
           ,[CreatedId]
           ,[CreatedTime])
     VALUES
           (");
                    //WarningGL01 g01 = new WarningGL01();
                    List<string> spStr = new List<string>();
                    string line = sr.ReadLine();
                    if (string.IsNullOrEmpty(line))
                        continue;
                    if (line.Length < 118)
                        continue;
                    int totalLen = line.Length;
                    int i = 0;
                    int prevPos = 0;
                    foreach (var p in posG01)
                    {
                        if (i == 0)
                        {
                            prevPos = p;
                            i++;
                            continue;
                        }
                        if (p > totalLen + 1)
                        {
                            sb.Append(string.Format("'{0}',", line.Substring(prevPos - 1)));
                            break;
                        }
                        spStr.Add(line.Substring(prevPos - 1, p - prevPos));
                        sb.Append(string.Format("'{0}',", line.Substring(prevPos - 1, p - prevPos)));
                        prevPos = p;
                        i++;
                    }
                    string sqlstr = sb.ToString();
                    sqlstr = sqlstr.Substring(0, sqlstr.Length - 1) + ",'Sys', GETDATE());";
                    lstSQL.Add(sqlstr);
                    //Console.WriteLine(sqlstr);
                    //lstG01.Add(g01);
                }
            }

            {
                //Console.WriteLine(s);

            }
            using (NewsEntities ctx = new NewsEntities())
            {
                foreach (var s in lstSQL)
                {
                    int noOfRowInserted = ctx.Database.ExecuteSqlCommand(s);
                }
                int noOfRowInserted1 = ctx.Database.ExecuteSqlCommand("update  [dbo].[WarningGL36] set ACT_DATE_TIME = ACT_CCYY + ACT_MM + ACT_DD + ACT_TIME");

                int noOfRowInserted2 = ctx.Database.ExecuteSqlCommand("update  [dbo].[WarningGL36] set ACT_DATE = ACT_CCYY + ACT_MM + ACT_DD");


                
            }
        }

        /// <summary>
        /// 取得FTP上的檔案, 並轉換成Local的檔名
        /// </summary>
        /// <returns></returns>
        private static string[] getFileList()
        {
            if (loaclFilePath.Trim() != "")
            {
                if (!Directory.Exists(loaclFilePath))
                {
                    Directory.CreateDirectory(loaclFilePath);
                }
            }
            else
            {
                loaclFilePath = AppDomain.CurrentDomain.BaseDirectory;
            }
            //獲取FTP文件清單
            m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------正在獲取FTP文件清單----------------");
            ArrayList fileList = ftpClient.GetFileList(ftpdir);
            List<string> result = new List<string>();
            //下載FTP指定目錄下的所有文件
            foreach (var file in fileList)
            {
                string remoteFile = ftpClient.SetRemotePath(ftpdir) + "//" + file;
                string localFile = loaclFilePath.TrimEnd('\\') + "\\" + file;
                ftpClient.GetFiles(remoteFile, localFile);
                result.Add(localFile);
            }
            m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------獲取FTP文件清單結束----------------");
            //string[] fileNames = Array.FindAll((string[])fileList.ToArray(typeof(string)), delegate (String item)
            //{
            //    return item.Contains("." + fileTypes[0]) ? true : false;
            //});

            return result.ToArray();
        }


        static DataTable CreateGL()
        {
            DataTable result = new DataTable();
            result.Columns.Add(new DataColumn("CHQ_PAYEE", typeof(String)));
            result.Columns.Add(new DataColumn("id", typeof(int)));
            result.Columns.Add(new DataColumn("INST_NO", typeof(String)));
            result.Columns.Add(new DataColumn("HOME_BRCH", typeof(String)));
            result.Columns.Add(new DataColumn("ACCT_NO", typeof(String)));
            result.Columns.Add(new DataColumn("ACT_DATE_TIME", typeof(String)));
            result.Columns.Add(new DataColumn("ACT_DATE", typeof(String)));
            result.Columns.Add(new DataColumn("ACT_CCYY", typeof(String)));
            result.Columns.Add(new DataColumn("ACT_MM", typeof(String)));
            result.Columns.Add(new DataColumn("ACT_DD", typeof(String)));
            result.Columns.Add(new DataColumn("ACT_TIME", typeof(String)));
            result.Columns.Add(new DataColumn("TRAN_TYPE", typeof(String)));
            result.Columns.Add(new DataColumn("TRAN_STATUS", typeof(String)));
            result.Columns.Add(new DataColumn("TRAN_DATE", typeof(String)));
            result.Columns.Add(new DataColumn("BRANCH",typeof(String)));
            result.Columns.Add(new DataColumn("BRANCH_TERM", typeof(String)));
            result.Columns.Add(new DataColumn("TELLER", typeof(String)));
            result.Columns.Add(new DataColumn("TRAN_CODE", typeof(String)));
            result.Columns.Add(new DataColumn("POST_DATE", typeof(String)));
            result.Columns.Add(new DataColumn("JRNL_NO", typeof(String)));
            result.Columns.Add(new DataColumn("AMOUNT", typeof(String)));
            result.Columns.Add(new DataColumn("BTCH_NO_U", typeof(String)));
            result.Columns.Add(new DataColumn("CORRECTION", typeof(String)));
            result.Columns.Add(new DataColumn("DEFER_DAYS", typeof(String)));
            result.Columns.Add(new DataColumn("BALANCE", typeof(String)));
            result.Columns.Add(new DataColumn("FOREIGN_FLAG", typeof(String)));
            result.Columns.Add(new DataColumn("FILLER", typeof(String)));
            result.Columns.Add(new DataColumn("CreatedId", typeof(String)));
            result.Columns.Add(new DataColumn("CreatedTime", typeof(DateTime)));
            result.Columns.Add(new DataColumn("ACCOUNT_NO", typeof(String)));
            result.Columns.Add(new DataColumn("SYSTEM", typeof(String)));
            result.Columns.Add(new DataColumn("DESCR", typeof(String)));

            return result;

        }
    }
    class KDSql
    {
        #region 宣告
        public System.Data.SqlClient.SqlConnection SQLCon;
        public System.Data.SqlClient.SqlConnection CountCon;
        public System.Data.SqlClient.SqlDataAdapter AD;
        public System.Data.DataSet DS;
        public int Count = 0;
        public double dobCount = 0;
        public string strSQL = "";
        public string strErr = "";
        public string strReturn = "";
        public string strSQLCon = "";
        //		public string strSQLCECon = @"Data Source=.\Program Files\SMS\TPEMS.sdf";
        //		public string FileName = @".\Program Files\SMS\Host.ini";
        #endregion

        #region TODO: 在此加入建構函式的程式碼
        public KDSql()
        {


            strSQLCon = ConfigurationManager.ConnectionStrings["CSFS_ADO"].ToString();
            //
            // TODO: 在此加入建構函式的程式碼
            //
        }
        #endregion


        #region SQLServExec(string _strSQL)，執行一 SQL 命令，若成功傳回1
        //執行一 SQL 命令，若成功傳回1
        public int SQLServExec(string _strSQL)
        {
            try
            {
                //Cursor.Current = Cursors.WaitCursor;
                SQLCon = new SqlConnection(strSQLCon);
                SQLCon.Open();
                System.Data.SqlClient.SqlCommand cmd = new SqlCommand(_strSQL, SQLCon);
                Count = cmd.ExecuteNonQuery();
                SQLCon.Close();
            }
            catch (System.Data.SqlClient.SqlException Err)
            {
                strErr = "執行 SQL 命令[" + _strSQL + "] 失敗" + "\n" + Err.Message.ToString();
                //MessageBox.Show( strErr ,  "執行SQL Command 錯誤" );
            }
            finally
            {
                //Cursor.Current = Cursors.Default;
            }
            return Count;
        }

        #endregion


        #region SQLServGetInt(string _strSQL)，執行 SQL 命令並傳回一 Int
        //執行 SQL 命令並傳回一 int
        public int SQLServGetInt(string _strSQL)
        {
            try
            {
                //Cursor.Current = Cursors.WaitCursor;
                SQLCon = new SqlConnection(strSQLCon);
                SQLCon.Open();
                System.Data.SqlClient.SqlCommand cecmd = new SqlCommand(_strSQL, SQLCon);
                Count = Convert.ToUInt16(cecmd.ExecuteScalar());
                SQLCon.Close();
            }
            catch (System.Data.SqlClient.SqlException Err)
            {
                strErr = "執行 SQL 命令[" + _strSQL + "] 失敗" + "\n" + Err.Message.ToString();
                //MessageBox.Show( strErr ,  "執行SQL Command 錯誤" );
            }
            finally
            {
                //Cursor.Current = Cursors.Default;
            }
            return Count;
        }
        #endregion

        #region SQLServGetInt2(string _strSQL,_strConn)，執行 SQL 命令並傳回一 Int
        //執行 SQL 命令並傳回一 int
        public int SQLServGetInt2(string _strSQL, string _strConn)
        {
            try
            {
                //Cursor.Current = Cursors.WaitCursor;
                SQLCon = new SqlConnection(_strConn);
                SQLCon.Open();
                System.Data.SqlClient.SqlCommand cecmd = new SqlCommand(_strSQL, SQLCon);
                Count = Convert.ToInt32(cecmd.ExecuteScalar());
                SQLCon.Close();
            }
            catch (System.Data.SqlClient.SqlException Err)
            {
                strErr = "執行 SQL 命令[" + _strSQL + "] 失敗" + "\n" + Err.Message.ToString();
                //MessageBox.Show( strErr ,  "執行SQL Command 錯誤" );
            }
            finally
            {
                //Cursor.Current = Cursors.Default;
            }
            return Count;
        }
        #endregion


        #region SQLServGetDouble(string _strSQL)，執行 SQL 命令並傳回一 Double
        //執行 SQL 命令並傳回一 Double
        public double SQLServGetDouble(string _strSQL)
        {
            //object myObject;
            try
            {
                //Cursor.Current = Cursors.WaitCursor;
                SQLCon = new SqlConnection(strSQLCon);
                SQLCon.Open();
                System.Data.SqlClient.SqlCommand cmd = new SqlCommand(_strSQL, SQLCon);
                object myObject = cmd.ExecuteScalar();
                if (myObject != null)
                {
                    if (myObject.GetType().ToString() != "System.DBNull")
                        dobCount = (double)myObject;
                    else
                        dobCount = 0;
                }
                else
                    dobCount = 0;

                //string strTemp =myObject.GetType().ToString();
                //dobCount = Convert.ToDouble(cecmd.ExecuteScalar());
                //dobCount = (double)cecmd.ExecuteScalar();
                SQLCon.Close();
            }
            catch (System.Data.SqlClient.SqlException Err)
            {
                strErr = "執行 SQL 命令[" + _strSQL + "] 失敗" + "\n" + Err.Message.ToString();
                //MessageBox.Show( strErr ,  "執行SQL Command 錯誤" );
            }
            finally
            {
                //Cursor.Current = Cursors.Default;
            }
            return dobCount;
        }
        #endregion

        #region SQLServGetStr(string _strSQL)，執行 SQL 命令並傳回一 String
        //執行 SQL 命令並傳回一 string
        public string SQLServGetStr(string _strSQL)
        {
            try
            {
                //Cursor.Current = Cursors.WaitCursor;
                SQLCon = new SqlConnection(strSQLCon);
                SQLCon.Open();
                System.Data.SqlClient.SqlCommand cmd = new SqlCommand(_strSQL, SQLCon);
                //strReturn = (string)(cmd.ExecuteScalar());
                object myObject = cmd.ExecuteScalar();
                if (myObject != null)
                {
                    if (myObject.GetType().ToString() != "System.DBNull")
                        strReturn = (string)myObject;
                    else
                        strReturn = "";
                }
                else
                    strReturn = "";

                SQLCon.Close();
            }
            catch (System.Data.SqlClient.SqlException Err)
            {
                strErr = "執行 SQL 命令[" + _strSQL + "] 失敗" + "\n" + Err.Message.ToString();
                //MessageBox.Show( strErr ,  "執行SQL Command 錯誤" );
            }
            finally
            {
                //Cursor.Current = Cursors.Default;
            }
            return strReturn;
        }
        #endregion
        #region SQLServGetStr2(string _strSQL,_strConn)，執行 SQL 命令並傳回一 String
        //執行 SQL 命令並傳回一 string
        public string SQLServGetStr2(string _strSQL, string _strConn)
        {
            try
            {
                //Cursor.Current = Cursors.WaitCursor;
                SQLCon = new SqlConnection(_strConn);
                SQLCon.Open();
                System.Data.SqlClient.SqlCommand cmd = new SqlCommand(_strSQL, SQLCon);
                //strReturn = (string)(cmd.ExecuteScalar());
                object myObject = cmd.ExecuteScalar();
                if (myObject != null)
                {
                    if (myObject.GetType().ToString() != "System.DBNull")
                        strReturn = (string)myObject;
                    else
                        strReturn = "";
                }
                else
                    strReturn = "";

                SQLCon.Close();
            }
            catch (System.Data.SqlClient.SqlException Err)
            {
                strErr = "執行 SQL 命令[" + _strSQL + "] 失敗" + "\n" + Err.Message.ToString();
                //MessageBox.Show( strErr ,  "執行SQL Command 錯誤" );
            }
            finally
            {
                //Cursor.Current = Cursors.Default;
            }
            return strReturn;
        }
        #endregion
        #region SQLServGetDataSet(string _strSQL)，執行 SQL 命令並傳回一 DataSet
        //執行一 SQL Serv 命令並傳回一 DataSet
        public System.Data.DataSet SQLServGetDataSet(string _strSQL)
        {
            try
            {
                //Cursor.Current = Cursors.WaitCursor;
                SQLCon = new SqlConnection(strSQLCon);
                SQLCon.Open();
                AD = new SqlDataAdapter(_strSQL, SQLCon);
                DS = new DataSet();
                AD.Fill(DS, "Result");
                AD.Dispose();
                SQLCon.Close();
            }
            catch (System.Data.SqlClient.SqlException Err)
            {
                strErr = "執行 SQL 命令[" + _strSQL + "] 失敗" + "\n" + Err.Message.ToString();
                //MessageBox.Show( strErr ,  "執行SQL Command 錯誤" );
            }
            finally
            {
                //Cursor.Current = Cursors.Default;
            }
            return DS;
        }


        public System.Data.DataTable getDataTable(string _strSQL)
        {
            DataTable Result = new DataTable();
            try
            {

                //Cursor.Current = Cursors.WaitCursor;
                SQLCon = new SqlConnection(strSQLCon);
                SQLCon.Open();
                AD = new SqlDataAdapter(_strSQL, SQLCon);
                DS = new DataSet();
                AD.Fill(DS, "Result");
                if (DS.Tables.Count > 0)
                {
                    Result = DS.Tables[0];
                    if (Result == null)
                        Result = null;
                }
                else
                    Result = null;
                AD.Dispose();
                SQLCon.Close();
            }
            catch (System.Data.SqlClient.SqlException Err)
            {
                strErr = "執行 SQL 命令[" + _strSQL + "] 失敗" + "\n" + Err.Message.ToString();
                //MessageBox.Show( strErr ,  "執行SQL Command 錯誤" );
            }
            finally
            {
                //Cursor.Current = Cursors.Default;
            }
            return Result;
        }

        #endregion

        #region SQLServ2Trans
        public void SQLServ2Trans(string strSQL1, string strSQL2)
        {
            SQLCon = new SqlConnection(strSQLCon);
            SQLCon.Open();

            System.Data.SqlClient.SqlCommand cmd = SQLCon.CreateCommand();
            System.Data.SqlClient.SqlTransaction SqlTrans;

            // Start a local transaction
            SqlTrans = SQLCon.BeginTransaction("KDTransaction");
            // Must assign both transaction object and connection
            // to Command object for a pending local transaction
            cmd.Connection = SQLCon;
            cmd.Transaction = SqlTrans;

            try
            {
                cmd.CommandText = strSQL1;
                cmd.ExecuteNonQuery();
                cmd.CommandText = strSQL2;
                cmd.ExecuteNonQuery();
                SqlTrans.Commit();
                SQLCon.Close();
            }
            catch (Exception Err)
            {
                try
                {
                    SqlTrans.Rollback("KDTransaction");
                }
                catch (SqlException Err2)
                {
                    if (SqlTrans.Connection != null)
                    {
                        strErr = Err2.GetType().ToString();
                    }
                }
                //strErr = Err.GetType().ToString();
                strErr = "執行 SQL 命令[" + strSQL1 + "] 失敗" + "\n" + Err.Message.ToString();
                //MessageBox.Show( strErr ,  "執行SQL Command 錯誤" );
            }
        }
        #endregion

        #region FillError
        //攔截Fill發生錯誤時的事件，強迫忽略錯誤並繼續執行
        protected static void FillError(object sender, FillErrorEventArgs args)
        {
            if (args.Errors.GetType() == typeof(System.OverflowException))
            {
                //Code to handle Precision Loss
                args.Continue = true;
            }
        }
        #endregion


        #region GetSQLNumber
        //轉換資料成 SQL COMMAND 格式之字串
        public string GetSQLString(string strValue)
        {
            string strReturn;

            if (strValue == "")
                strReturn = "NULL";
            else
                strReturn = "'" + strValue + "'";
            return strReturn;

        }

        public string GetSQLString(object objData)
        {
            string strReturn;

            if ((objData == null) || (objData is DBNull))
                strReturn = "NULL";
            else
                strReturn = "'" + objData.ToString() + "'";
            return strReturn;

        }


        public string GetSQLNumber(string strValue)
        {
            string strReturn;

            if (strValue == "")
                strReturn = "NULL";
            else
                strReturn = strValue;
            return strReturn;

        }

        public string GetSQLNumber(object objData)
        {
            string strReturn;

            if ((objData == null) || (objData is DBNull))
                strReturn = "NULL";
            else
                strReturn = objData.ToString();
            return strReturn;

        }

        #endregion

    }

    public class groupItem
    {
        public string JRNL { get; set; }
        public string PostDate { get; set; }
        public int Count { get; set; }
    }
}
