using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.IO;
using Newtonsoft.Json;
using System.Collections;
using System.Text.RegularExpressions;

namespace CTBC.WinExe.InsertCSV
{
    class Program
    {
        static void Main(string[] args)
        {
            log4net.Config.XmlConfigurator.Configure(new System.IO.FileInfo(AppDomain.CurrentDomain.BaseDirectory + @"\Log.config"));


            string dir = string.Empty;
            if (args[0].ToString().EndsWith(@"\"))
                dir = args[0];
            else
                dir = args[0] + "\\";

            Console.WriteLine(string.Format("開始匯入 {0} 裏面的CSV 到 資料庫", dir));
            Console.WriteLine("若確定開始, 請按ENTER, 否則請直接關閉 ....");
            Console.ReadLine();


            CSVBiz csv = new CSVBiz();

            //開始匯入595815筆的MailInfo
            csv.WriteLog(string.Format("\t===============================================\r\n"));
            csv.WriteLog(string.Format("\t開始匯入595815筆的MailInfo ...\r\n"));
            ImportMailInfoPROD(dir + "PROD.sql");
            csv.WriteLog(string.Format("\t結束匯入MailInfo ...\r\n"));


            //string dirname = args[0].ToString();
            string a = "<val>";
            string b = ConfigurationManager.AppSettings["NtPattern"].ToString();
            var ntpattern = b.Replace("{0}", a);
            

            List<mappings1> maps1 = new List<mappings1>();
            List<mappings2> maps2 = new List<mappings2>();

            //DirectoryInfo di = new DirectoryInfo(dir);




            csv.WriteLog(string.Format("\r\n\r\n\t===============================================\r\n"));
            {
                csv.WriteLog(string.Format("\t開始處理3個CSV檔案"));
                {
                    int Count = csv.moveCSV1(dir, "CaseSendSetting.csv", ntpattern, maps1);
                    Console.WriteLine(string.Format("共計匯入{0} 至 dbo.[CaseSendSetting] 中", Count.ToString()));
                    {
                        int Count2 = csv.moveCSV2(dir , "CaseSendSettingDetails.csv", maps1, maps2);
                        Console.WriteLine(string.Format("共計匯入{0} 至 dbo.[CaseSendSettingDetail] 中", Count2.ToString()));
                        {
                            int Count3 = csv.moveCSV3(dir, "MailInfo.csv", maps2);
                            Console.WriteLine(string.Format("共計匯入{0} 至 dbo.[MailInfo] 中", Count3.ToString()));
                        }

                    }
                }

            }


            csv.WriteLog(string.Format("\r\n\r\n\t===============================================\r\n"));
            csv.WriteLog(string.Format("\t開始計算3個CSV檔案回朔"));


            csv.OutputCaseSendConsistence(dir);




            Console.WriteLine("程式結束!!");
        }


        /// <summary>
        /// 匯入PROD的MailInfo ....
        /// </summary>
        /// <param name="p"></param>
        private static void ImportMailInfoPROD(string filename)
        {
            Console.WriteLine(string.Format("\t開始匯入595815筆的MailInfo ...\r\n"));

            Regex re = new Regex(@"(?<val>\(\d{6,7},)");
            CSVBiz csv = new CSVBiz();
            ArrayList array = new ArrayList();
            using (StreamReader sr = new StreamReader(filename, Encoding.UTF8))
            {
                int BatchSize = 1000;
                int i = 0;
                while( sr.Peek()>0)
                {
                    string line = sr.ReadLine();
                    if (string.IsNullOrEmpty(line) || line.StartsWith("GO") )
                        continue;
                    { // 開始把Idenety 的欄位砍掉...

                        line = line.Replace("([MailInfoId],", "(");
                        MatchCollection mc = re.Matches(line);
                        if( mc.Count==1)
                        {
                            foreach (Match m in mc)
                            {
                                var oldvalue = m.Groups["val"].Value;
                                line = line.Replace(oldvalue, "(");
                            }
                        }
                        else
                        {
                            csv.WriteLog("=====過濾Regex有錯...====== \r\n");
                        }
                    }


                    array.Add(line);
                    i++;
                    if (i % BatchSize == 0) // 每1000筆, 存一次DB
                    {
                        try
                        {
                            Console.Write(". ");
                            var bReslt = csv.SaveMailInfo(array);
                            if (bReslt == false)
                            {
                                //寫LOG....
                                csv.WriteLog("=====有錯====== \r\n");
                                foreach (var a in array)
                                    csv.WriteLog(a.ToString());
                                csv.WriteLog("=====有錯結束====== \r\n");
                            }
                            i = 0;
                            array.Clear();
                        }
                        catch (Exception ex)
                        {
                            csv.WriteLog("=====有錯====== \r\n");
                            foreach (var a in array)
                                csv.WriteLog(a.ToString());
                            csv.WriteLog("=====有錯結束====== \r\n");
                        }
                    }
                    
                }                
            }


            //把最後不到1000筆的, 也寫入DB...
            try
            {
                var bReslt1 = csv.SaveMailInfo(array);
                if (bReslt1 == false)
                {
                    //寫LOG....
                    csv.WriteLog("=====有錯====== \r\n");
                    foreach (var a in array)
                        csv.WriteLog(a.ToString());
                    csv.WriteLog("=====有錯結束====== \r\n");
                }


            }
            catch (Exception ex2)
            {
                
                    csv.WriteLog("=====有錯====== \r\n");
                    foreach (var a in array)
                        csv.WriteLog(a.ToString());
                    csv.WriteLog("=====有錯結束====== \r\n");
            }

        }


        

    }

    public class Docinfo
    {
        public string CnoNo { get; set; }
        public string DeptId { get; set; }
        public string DeptName { get; set; }
        public string HdlUser { get; set; }
        public string HdlUserName { get; set; }
        public string DeciDeptId { get; set; }
        public string DeciDeptName { get; set; }
        public string DeciUser { get; set; }
        public string DeciUserName { get; set; }
        public string DeciDate { get; set; }
        public string EndDeptId { get; set; }
        public string EndDeptName { get; set; }
        public string EndUser { get; set; }
        public string EndUserName { get; set; }
        public string EndDate { get; set; }
    }

    public class mappings1
    {
        public int SerialId { get; set; }
        public Guid CaseId { get; set; }
        public string SendNo { get; set; }
    }

    public class mappings2
    {
        public int DetailsId { get; set; }
        public int SerialId {get;set;}
        public Guid CaseId { get; set; }
    }
}
