using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CTBC.CSFS.BussinessLogic;
using CTBC.CSFS.Pattern;
using CTBC.CSFS.Models;
using System.Data.SqlClient;
using System.IO;
using System.Text.RegularExpressions;
using log4net;
using System.Collections;
using System.Data;

namespace CTBC.WinExe.InsertCSV
{
    public class CSVBiz : BaseBusinessRule
    {

        internal int moveCSV1(string dir, string fileName, string NTpattern, List<mappings1> maps1)
        {
            int result = 0;
            using (SqlConnection con = OpenConnection())
            {

                string AllContent = string.Empty;
                using (StreamReader sr = new StreamReader(dir + fileName, Encoding.UTF8))
                {
                    AllContent = sr.ReadToEnd();
                }




                //string pattern = @"[.\r\n]\d{1,5},";
                string pattern = @"(.\n\d{1,6},[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12})";

                string input = AllContent;
                string[] line = Regex.Split(input, pattern,
                                              RegexOptions.IgnoreCase,
                                              TimeSpan.FromMilliseconds(500));
                // 第一行不要
                int totalrow = (line.Count() - 1) / 2;

                //string NTpattern = @"(?<val>(NT\$|NTD\s*|TWD\s*|CNY\s*|USD\s*|收取金額：|新臺幣|新台幣)\d{1,3},\d{1,3}(,\d{1,3})?(,\d{1,3})?)";
                Regex qariRegex = new Regex(NTpattern);

                string aboutPattern = @"(?<val>\((約當|分別約當).*\))";
                Regex aboutRegex = new Regex(aboutPattern);

                List<string> combin = new List<string>();
                for (int i = 0; i < totalrow; i++)
                {

                    string firString = line[i * 2 + 1].Replace("\r\n", "");
                    string secString = line[i * 2 + 2].Replace("\"", "");

                    MatchCollection mc = qariRegex.Matches(secString);
                    foreach (Match m in mc)
                    {
                        var oldvalue = m.Groups["val"].Value;
                        string newvalue = oldvalue.Replace(",", "@");
                        secString = secString.Replace(oldvalue, newvalue);
                    }

                    MatchCollection mc1 = aboutRegex.Matches(secString);
                    foreach (Match m in mc1)
                    {
                        var oldvalue = m.Groups["val"].Value;
                        string newvalue = oldvalue.Replace(",", "@");
                        secString = secString.Replace(oldvalue, newvalue);
                    }



                    combin.Add(firString + secString);

                    //Console.Write("'{0}'", line[ctr]);
                    //if (ctr < line.Length - 1)
                    //    Console.Write(", ");
                }

                WriteLog(string.Format("\t\t共計有{0}筆記錄", combin.Count().ToString()));

                string lineWork = string.Empty;
                string sqlStr = string.Empty;
                List<string> ErrorCSV = new List<string>();
                List<string> SerialId = new List<string>();
                try
                {


                    foreach (var line2 in combin)
                    {
                        lineWork = line2;
                        List<string> item = line2.Split(',').ToList();
                        // 
                        if( item.Count()>19)
                        {
                            // 合併9+10+ 11 + ....
                            int merCount = item.Count() - 19;
                            string sb1 = string.Empty;
                            for (int i = 0; i <9;i++ )
                            {
                                sb1+=item[i] + ",";
                            }
                            sb1 += item[9];
                            for (int i = 10; i <10 + merCount; i++)
                            {
                                sb1+="@" + item[i];
                                //item[9] += "@" + item[i];
                            }
                            sb1 += ",";
                            for(int i=10+merCount;i<item.Count();i++)
                            {
                                sb1+=item[i] + ",";
                            }

                            item = sb1.TrimEnd(',').Split(',').ToList();
                        }

                        StringBuilder sb = new StringBuilder();
                        // 將欄位0,1,4 欄位, 寫到maps 物件, 以供Detail去對映...
                        maps1.Add(new mappings1() { SerialId = int.Parse(item[0]), CaseId = Guid.Parse(item[1]), SendNo = item[4] });

                        try
                        {
                            for (int i = 1; i < item.Count; i++) // 從1開始, 因為SerialId不能Set Identtity_insert
                            {
                                string newX = item[i].Trim();
                                if (item[i].Contains("@"))
                                {
                                    newX = item[i].Replace("@", ",");
                                }
                                if (i == 10) // 因為是數字型態, 不加''
                                {
                                    sb.Append(newX + ",");
                                }
                                else if (i == 5 || i == 11 || i == 14 || i == 16 || i == 18) // 日期型態
                                {
                                    if (newX.ToLower().Equals("null"))
                                        sb.Append("NULL,");
                                    else
                                        sb.Append(" CAST('" + newX + "' AS DATETIME) ,");
                                }
                                else
                                    sb.Append("'" + newX + "',");
                            }
                            //sqlStr = string.Format(@"SET IDENTITY_INSERT CaseSendSetting ON;INSERT INTO [dbo].[CaseSendSetting] ([SerialID], [CaseId], [Template], [SendWord], [SendNo], [SendDate], [Speed], [Security], [Subject], [Description], [isFinish], [FinishDate], [Attachment], [CreatedUser], [CreatedDate], [ModifiedUser], [ModifiedDate], [SendKind], [SendUpDate])  Values ({0})", sb.ToString().TrimEnd(','));
                            sqlStr = string.Format(@"INSERT INTO [dbo].[CaseSendSetting] ([CaseId], [Template], [SendWord], [SendNo], [SendDate], [Speed], [Security], [Subject], [Description], [isFinish], [FinishDate], [Attachment], [CreatedUser], [CreatedDate], [ModifiedUser], [ModifiedDate], [SendKind], [SendUpDate])  Values ({0})", sb.ToString().TrimEnd(','));
                            base.ExecuteNonQuery(sqlStr);
                            SerialId.Add(item[0]);
                            result++;
                        }
                        catch (Exception ex)
                        {
                            WriteLog(string.Format("\tCaseSendSetting Insert 發生錯誤 {0} ", ex.Message.ToString()));
                            WriteLog(string.Format("\t發生錯誤 SQL {0} ", sqlStr));
                            ErrorCSV.Add(line2);
                            //Console.WriteLine("Error " + ex.Message.ToString() + "\r\n" + lineWork);
                            //Console.WriteLine("Error " + sqlStr);
                        }
                    }
                }
                catch (Exception ex)
                {
                    WriteLog("Error " + ex.Message.ToString());
                }
                WriteLog(string.Format("\tCaseSendSetting 共計成功Insert 以下SerialId {0}", string.Join(",", SerialId)));
                // 寫出ErrorCSV..
                if (ErrorCSV.Count() > 0)
                {
                    using (StreamWriter sw = new StreamWriter(dir + "CaseSendSetting_Error.csv", false, Encoding.UTF8))
                    {
                        foreach (var e in ErrorCSV)
                            sw.WriteLine(e);
                    }
                }
            }


            return result;
        }

        public void WriteLog(string msg)
        {
            if (Directory.Exists(@".\Log") == false)
            {
                Directory.CreateDirectory(@".\Log");
            }
            LogManager.Exists("DebugLog").Debug(msg);
        }

        internal int moveCSV2(string dir, string fileName, List<mappings1> maps1, List<mappings2> maps2)
        {
            int result = 0;
            List<string> ErrorCSV = new List<string>();
            List<string> SerialId = new List<string>();
            using (SqlConnection con = OpenConnection())
            {
                string sqlStr = string.Empty;
                string lineWork = string.Empty;
                using (StreamReader sr = new StreamReader(dir + fileName, Encoding.UTF8))
                {
                    while (sr.Peek() > 0)
                    {
                        string line = sr.ReadLine();

                        if (line.StartsWith("DetailsId,"))
                            continue;

                        string[] item = line.Split(',');
                        maps2.Add(new mappings2() { DetailsId = int.Parse(item[0]), SerialId = int.Parse(item[2]), CaseId = Guid.Parse(item[1]) });
                        StringBuilder sb = new StringBuilder();
                        try
                        {
                            for (int i = 1; i < item.Count(); i++) // 從1開始, 因為[DetailsId] 不能Set Idenetity Insert
                            {


                                if (i == 2) // SerialId要跟著CaseSendSetting的Id 來跑.. 所以要重新Selection
                                {
                                    // 要去maps 對找當初是那一個SerialId ..用CaseId + SendNo...

                                    int detailSeriaId = int.Parse(item[2]);
                                    var css = maps1.FirstOrDefault(x => x.SerialId == detailSeriaId);
                                    if (css != null)
                                    {
                                        // 用css.CaseId && css.SendNo 再去找資料庫中, 實際存入DB的SerialId 
                                        string sqlStr2 = string.Format(@"  select top 1 * from CaseSendSetting where caseid='{0}' and Sendno='{1}'", css.CaseId, css.SendNo);
                                        var cs = base.SearchList<CaseSendSetting>(sqlStr2);
                                        if (cs != null) // 若有找到.... 則修改item[2]的值, 改為之前存在CaseSendSetting中的值....
                                            item[i] = cs.First().SerialId.ToString();
                                        else
                                            WriteLog(string.Format("\t找不到 dbo.CaseSendSetting 中 發生錯誤 CaseId = {0}, SendNo={1} ", css.CaseId, css.SendNo));
                                    }
                                    else
                                    {
                                        // 若找不到, 用只用CaseId找..
                                        var css1 = maps1.FirstOrDefault(x => x.CaseId == Guid.Parse( item[1]));
                                        if (css1 != null)
                                        {
                                            string sqlStr2 = string.Format(@"  select top 1 * from CaseSendSetting where caseid='{0}' and Sendno='{1}'", css1.CaseId, css1.SendNo);
                                            var cs = base.SearchList<CaseSendSetting>(sqlStr2);
                                            if (cs != null) // 若有找到.... 則修改item[2]的值, 改為之前存在CaseSendSetting中的值....
                                                item[i] = cs.First().SerialId.ToString();
                                            else
                                                WriteLog(string.Format("\t找不到 dbo.CaseSendSetting 中 發生錯誤 CaseId = {0}, SendNo={1} ", css.CaseId, css.SendNo));
                                        }
                                        else
                                        {
                                            WriteLog(string.Format("\t CaseSendSettingDetails找不到CaseSendSetting 的對映 mapping表 發生錯誤 CaseId = {0} ", item[1]));
                                        }
                                    }
                                        
                                }
                                sb.Append("'" + item[i] + "',");
                            }
                            sqlStr = string.Format(@"INSERT INTO [dbo].[CaseSendSettingDetails] ([CaseId], [SerialID], [SendType], [GovName], [GovAddr])  Values ({0})", sb.ToString().TrimEnd(','));
                            base.ExecuteNonQuery(sqlStr);
                            result++;
                            SerialId.Add(item[0]);
                        }
                        catch (Exception ex)
                        {
                            //Console.WriteLine("Error " + ex.Message.ToString() + "\r\n" + lineWork);
                            //Console.WriteLine("Error " + sqlStr);
                            WriteLog(string.Format("\tInsert 發生錯誤 {0} ", ex.Message.ToString()));
                            WriteLog(string.Format("\tCaseSendSettingDetails Insert 發生錯誤 {0} ", sqlStr));
                            ErrorCSV.Add(line);
                        }


                    }
                }
            }
            WriteLog(string.Format("\t\t共計有{0}筆記錄", SerialId.Count().ToString()));
            WriteLog(string.Format("\tCaseSendSettingDetails 共計成功Insert 以下[DetailsId] {0}", string.Join(",", SerialId)));
            // 寫出ErrorCSV..
            if (ErrorCSV.Count() > 0)
            {
                using (StreamWriter sw = new StreamWriter(dir + "CaseSendSettingDetails.csv", false, Encoding.UTF8))
                {
                    foreach (var e in ErrorCSV)
                        sw.WriteLine(e);
                }
            }
            return result;
        }

        internal int moveCSV3(string dir, string fileName, List<mappings2> maps2)
        {
            int result = 0;
            List<string> SerialId = new List<string>();
            List<string> ErrorCSV = new List<string>();
            using (SqlConnection con = OpenConnection())
            {
                string sqlStr = string.Empty;
                string lineWork = string.Empty;
                using (StreamReader sr = new StreamReader(dir + fileName, Encoding.UTF8))
                {
                    while (sr.Peek() > 0)
                    {
                        string line = sr.ReadLine();

                        if (line.StartsWith("MailInfoId,"))
                            continue;

                        string[] item = line.Split(',');
                        StringBuilder sb = new StringBuilder();
                        try
                        {
                            #region 一筆一筆處理

                            for (int i = 1; i < item.Count(); i++)
                            {

                                if (i == 2) // 要修正SendDetailId
                                {
                                    var css = maps2.FirstOrDefault(x => x.DetailsId == int.Parse(item[2]));
                                    if (css != null)
                                    {
                                        // 用css.CaseId && css.SendNo 再去找資料庫中, 實際存入DB的SerialId 
                                        string sqlStr2 = string.Format(@"  select top 1 * from CaseSendSettingDetails where caseid='{0}' ", css.CaseId);
                                        var cs = base.SearchList<CaseSendSettingDetails>(sqlStr2);
                                        if (cs != null) // 若有找到.... 則修改item[2]的值, 改為之前存在CaseSendSetting中的值....
                                            item[i] = cs.First().DetailsId.ToString();
                                        else
                                            WriteLog(string.Format("\t找不到 dbo.CaseSendSetting 中 發生錯誤 CaseId = {0} ", css.CaseId));
                                    }
                                    else
                                        WriteLog(string.Format("\t找不到mapping表 發生錯誤 [SendDetailId] = {0} ", item[2]));
                                }

                                if (i == 4 || i == 6) // 日期型態
                                {
                                    sb.Append(" CAST('" + item[i] + "' AS DATETIME) ,");
                                }
                                else
                                    sb.Append("'" + item[i] + "',");
                            }
                            #endregion
                            sqlStr = string.Format(@"INSERT INTO MailInfo ([CaseId], [SendDetailId], [MailNo], [MailDate], [CreatedUser], [CreatedDate])  Values ({0})", sb.ToString().TrimEnd(','));
                            base.ExecuteNonQuery(sqlStr);
                            SerialId.Add(item[0]);
                            result++;
                        }
                        catch (Exception ex)
                        {
                            //Console.WriteLine("Error " + ex.Message.ToString() + "\r\n" + lineWork);
                            //Console.WriteLine("Error " + sqlStr);
                            WriteLog(string.Format("\tInsert 發生錯誤 {0} ", ex.Message.ToString()));
                            WriteLog(string.Format("\tMailInfo Insert 發生錯誤 {0} ", sqlStr));
                            ErrorCSV.Add(line);
                        }


                    }
                }
            }
            WriteLog(string.Format("\t\t共計有{0}筆記錄", SerialId.Count().ToString()));
            WriteLog(string.Format("\tMailInfo 共計成功Insert 以下MailInfoId {0}", string.Join(",", SerialId)));
            // 寫出ErrorCSV..
            if (ErrorCSV.Count() > 0)
            {
                using (StreamWriter sw = new StreamWriter(dir + "CaseSendSettingDetails.csv", false, Encoding.UTF8))
                {
                    foreach (var e in ErrorCSV)
                        sw.WriteLine(e);
                }
            }
            return result;
        }

        internal bool SaveMailInfo(ArrayList array)
        {
            bool flag = true;
            try
            {
                if (array.Count > 0)
                {
                    IDbConnection dbConnection = base.OpenConnection();
                    IDbTransaction dbTransaction = null;
                    using (dbConnection)
                    {
                        dbTransaction = dbConnection.BeginTransaction();
                        string strSql = "";
                        for (int i = 0; i < array.Count; i++)
                        {
                            strSql += array[i];
                        }
                        int rtn = base.ExecuteNonQuery(strSql, dbTransaction);
                        dbTransaction.Commit();
                    }
                }
            }
            catch
            {
                flag = false;
            }
            return flag;
        }

        internal void OutputCaseSendConsistence(string dir)
        {

            WriteLog("==================> 開始 找 2018年以前... CaseSendSettingDetail 找不到CaseSendSetting 的列表");

            string sqlStr = @"select [DetailsId],[CaseId] FROM [CaseSendSettingDetails] where detailsid not in (
SELECT d.detailsid FROM [CaseSendSettingDetails] d inner join [CaseSendSetting] m on d.SerialId = m.SerialID
  and d.caseid = m.caseid and CreatedDate <'2018-01-01'
  )";

            using (SqlConnection con = OpenConnection())
            {                
                DataTable dt = base.Search(sqlStr);
                if( dt.Rows.Count >0 )
                {
                    using(StreamWriter sw = new StreamWriter(dir + "ErrMap1.csv",false,Encoding.UTF8))
                	{
                        WriteLog(string.Format("\t以下是 2018年以前... CaseSendSettingDetail 找不到CaseSendSetting 的列表......"));
                        foreach(DataRow dr in dt.Rows)
                        {
                            sw.WriteLine(dr[0] + "," + dr[1]);
                        }		 
	                }
                }
            }
            WriteLog("==================>結束");




            WriteLog("==================> 開始 找 2018年以前... MailInfo 找不到CaseSendSettingDetail 的列表");


            sqlStr = @"select mailInfoid,CaseId from mailinfo where mailInfoid not in (
  select mailInfoid from mailinfo m inner join [CaseSendSettingDetails] d on m.caseid = d.caseid 
	and m.SendDetailId = d.detailsid 
	and d.detailsid in (
		SELECT d.detailsid FROM [CaseSendSettingDetails] d inner join [CaseSendSetting] m on d.SerialId = m.SerialID
		and d.caseid = m.caseid and CreatedDate <'2018-01-01'
	)
)";




            using (SqlConnection con = OpenConnection())
            {
                DataTable dt = base.Search(sqlStr);
                if (dt.Rows.Count > 0)
                {
                    using (StreamWriter sw = new StreamWriter(dir + "ErrMap2.csv", false, Encoding.UTF8))
                    {
                        WriteLog(string.Format("\t以下是 2018年以前... CaseSendSettingDetail 找不到CaseSendSetting 的列表......"));
                        foreach (DataRow dr in dt.Rows)
                        {
                            sw.WriteLine(dr[0] + "," + dr[1]);
                        }
                    }
                }
            }

            WriteLog("==================>結束");
        }
    }
}
