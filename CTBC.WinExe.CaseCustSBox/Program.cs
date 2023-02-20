using CTBC.CSFS.BussinessLogic;
using CTBC.CSFS.Models;
using CTBC.CSFS.Pattern;
using CTBC.FrameWork.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

namespace CTBC.WinExe.CaseCustSBox
{
    class Program : BaseBusinessRule
    {
        private static FileLog m_fileLog;

        private static string reciveftpserver;
        private static string reciveport;
        private static string reciveusername;
        private static string recivepassword;
        private static string reciveftpdir;
        private static string ftpdir;
        private static string reciveloaclFilePath;        
        private static string backupFilePath;
        private static FtpClient reciveftpClient;

        private static string specifyDay;



        public Program()
        {
            m_fileLog = new FileLog(ConfigurationManager.AppSettings["filelog"], AppDomain.CurrentDomain.FriendlyName.Replace(".vshost", "").Replace(".exe", ""));

            #region 獲取FTP檔案參數配置

            reciveftpserver = ConfigurationManager.AppSettings["reciveftpserver"];
            reciveport = ConfigurationManager.AppSettings["reciveport"];
            reciveusername = ConfigurationManager.AppSettings["reciveusername"];
            recivepassword = ConfigurationManager.AppSettings["recivepassword"];
            ftpdir = ConfigurationManager.AppSettings["ftpdir"];
            reciveftpdir = ConfigurationManager.AppSettings["reciveftpdir"];

            reciveloaclFilePath = AppDomain.CurrentDomain.BaseDirectory + ConfigurationManager.AppSettings["reciveloaclFilePath"];
            backupFilePath = AppDomain.CurrentDomain.BaseDirectory + ConfigurationManager.AppSettings["backupFilePath"];

            reciveftpClient = new FtpClient(reciveftpserver, reciveusername, recivepassword, reciveport);

            #endregion
        }

        static void Main(string[] args)
        {
            Program mainProgram = new Program();
            
            mainProgram.UploadSBox();
            mainProgram.DownloadSBox();
        }

        /// <summary>
        /// 上傳Sbox
        /// </summary>
        private bool UploadSBox()
        {
            // 由Web 界面, 打勾後, 若是保管箱.. 請Adam把CaseCustDetails.Status='02'  
            var sql = @"SELECT * FROM CaseCustDetails WHERE Status = '02'  AND QueryType='5' and SBoxSendStatus='00' ";
            var sboxList = base.SearchList<CaseCustDetails>(sql);
            StringBuilder sb = new StringBuilder();
            foreach(var sbox in sboxList)
            {
                sb.Append(sbox.CustIdNo + "\r\n");
            }

            // 判斷路徑是否存在
            if (!Directory.Exists(reciveloaclFilePath))
            {
                Directory.CreateDirectory(reciveloaclFilePath);
            }

            string filename = "Sbox_Upload_" + DateTime.Now.ToString("yyyyMMdd") + ".txt";
            //string filename = "SU" + DateTime.Now.ToString("yyyyMMdd") + ".txt";
            string fullename = Path.Combine(reciveloaclFilePath, filename);


            var totalsbox = sb.ToString().TrimEnd('\r','\n');
            //Encoding utf8WithoutBom = new UTF8Encoding(false);
            // 主機, 是吃ANSI 碼.. 所以要用Encoding.Default ...
            using (StreamWriter sw = new StreamWriter(fullename, false, Encoding.Default))
            {
                sw.Write(totalsbox);
            }

            bool ret = false;
            bool isFtp = bool.Parse(ConfigurationManager.AppSettings["isFtp"].ToString());
            try
            {
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息---------從保管箱資訊, 上傳到MFTP檔案作業開始----------------");

                if (isFtp) // 由Ftp 中, 取得檔案
                {
                    //  ftp 文件
                    //string remoteFile = reciveftpClient.SetRemotePath(reciveftpdir) + "//" + filename;
                    reciveftpClient.SendFile(reciveftpdir, fullename);
                    m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------從MFTP上傳檔案" + fullename + "----------------");

                    // 更新SBoxSendStatus, 改01, 表示, 已上傳....
                    var sqlUpdate = @"Update CaseCustDetails Set SBoxSendStatus='01' WHERE SBoxSendStatus = '00' AND QueryType='5' ";
                    base.ExecuteNonQuery(sqlUpdate);
                }
                else // 直接取File目錄裏的檔案
                {                    
                    ret = false;
                }
            }
            catch (Exception ex)
            {
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------從MFTP下載檔案作業失敗，失敗原因：" + ex.Message + "----------------");

                ret = false;
            }
            finally
            {
                
                //m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------無資料下載----------------");
                
            }

            return true;



        }


        /// <summary>
        /// 下載
        /// </summary>
        private void DownloadSBox()
        {

            string theday =  DateTime.Now.AddDays(-1).ToString("yyyyMMdd");
            // 
            // 20220914, 增加一個參數檔... 若是空的.. 則跑預設的前一個工作日...
            // CodeType='SBoxDownloadDate'
            // CodeNo='' 或 CodeNo='20220914'
            // CodeMemo='指定Sbox 到FTP抓的檔案日期.. 格式: yyyyMMdd , 則會主動去抓 SBox_Download_yyyyMMdd.txt '
            // 若有設定(Enable), 則跑指定的日期...            
            var pSQL = string.Format("select TOP 1 CodeNo from ParmCode where CodeType='SBoxDownloadDate' AND Enable='1'; ");
            var retResult = base.Search(pSQL);
            if (retResult.Rows.Count > 0)
            {
                theday = retResult.Rows[0][0].ToString();
            }
            else // 若沒有啟用任何日期, 則去讀取前一個工作日的日期.. 以yyyyMMdd 為主.
            {
                // 20220914, 只能抓前一個工作日的檔案...
                var predate = DateTime.Now.ToString("yyyy-MM-dd");
                var pSQL1 = string.Format("SELECT top 1 replace(convert(varchar, date, 111), '/','-') as prevDay   FROM PARMWorkingDay where date < '{0}' and Flag = 1 order by date desc", predate);
                var retResult1 = base.Search(pSQL1);
                if (retResult1.Rows.Count > 0)
                {
                    var prevWorkingDate = Convert.ToDateTime(retResult1.Rows[0][0].ToString());
                    theday = prevWorkingDate.ToString("yyyyMMdd");
                }
            }

            try
            {
                // 讀取FTP
                var files = ReciveFile(theday);

                // 存入CaseCustOutputF3
                if( files.Count>0)
                {
                    var rows = InsertDB(files);
                }
                else
                {
                    m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.Information, "目前FTP上, 沒有Sbox_Download_" + theday + ".txt檔案: ");
                }

            }
            catch (Exception ex)
            {
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.Information, "處理錯誤: " + ex.Message);
                throw ex;
            }
            finally
            {
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.Information, "-------處理結束-------");
            }

        }

        private bool InsertDB(List<string> files)
        {
            try
            {
                string prevWorkingDate = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd"); // 預設前一天... 
                // 找前一個工作日
                var predate = DateTime.Now.ToString("yyyy-MM-dd");
                var pSQL = string.Format("SELECT top 1 replace(convert(varchar, date, 111), '/','-') as prevDay   FROM PARMWorkingDay where date < '{0}' and Flag = 1 order by date desc", predate);
                var retResult = base.Search(pSQL);
                if (retResult.Rows.Count > 0)
                {
                    prevWorkingDate = Convert.ToDateTime(retResult.Rows[0][0].ToString()).ToString("yyyy-MM-dd");
                }



                // 找出已設定發查的保管箱....
                var sql = @"SELECT * FROM CaseCustDetails WHERE SBoxSendStatus = '01' AND QueryType='5' ";
                var sboxList = base.SearchList<CaseCustDetails>(sql);

                //var theday = DateTime.Now.ToString("yyyyMMdd");
                List<CaseCustOutputF3> ret = new List<CaseCustOutputF3>();
                List<string> successFiles = new List<string>();
                foreach (var file in files)
                {
                    if (file.IndexOf("Sbox_Download_" + prevWorkingDate.Replace("-","")) < 0)
                        continue;

                    #region 讀取File Content
                    m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息---------讀取檔案----------------" + file);
                    using (StreamReader sr = new StreamReader(file, Encoding.Default))
                    {
                        while (sr.Peek() > 0)
                        {
                            string line = sr.ReadLine();
                            if (line.StartsWith("身分證"))
                                continue;
                            string[] t = line.Split(',');
                            var hitDetail = sboxList.Where(x => x.CustIdNo == t[0].Trim()).FirstOrDefault();
                            if (hitDetail != null)
                            {
                                // Id, DocNo, MasterId, DetailsId, CUST_ID_NO, RENT_BRANCH, RENT_KIND, CUSTOMER_NAME, NIGTEL_NO, MOBIL_NO, COMM_ADDR, CUST_ADD, BOX_NO, MODIFIED_DATE, RENT_START, RENT_END, MEMO
                                CaseCustOutputF3 f3 = new CaseCustOutputF3()
                                {
                                    DocNo = hitDetail.DocNo,
                                    MasterId = hitDetail.CaseCustMasterId,
                                    DetailsId = hitDetail.DetailsId,
                                    CUST_ID_NO = t[0].Trim(),
                                    RENT_BRANCH = t[1].Trim(),
                                    RENT_KIND = t[2].Trim(),
                                    CUSTOMER_NAME = t[3].Trim(),
                                    NIGTEL_NO = t[4].Trim(),
                                    MOBIL_NO = t[5].Trim(),
                                    COMM_ADDR = t[6].Trim(),
                                    CUST_ADD = t[7].Trim(),
                                    BOX_NO = t[8].Trim(),
                                    MODIFIED_DATE = t[9].Trim(),
                                    RENT_START = t[10].Trim(),
                                    RENT_END = t[11].Trim(),
                                    MEMO = t[12].Trim()
                                };
                                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, string.Format("\t信息---------{0} / 保管箱 {1} ", f3.CUST_ID_NO, f3.BOX_NO));
                                ret.Add(f3);
                            }
                        }
                    }
                    successFiles.Add(file);

                    #endregion
                }

                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, string.Format("信息---------共比對到{0} 筆保管箱資訊----------------" , ret.Count().ToString()));

                // SAVE to DB .. ( ret -> CaseCustF3 )
                StringBuilder sb = new StringBuilder();
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, string.Format("信息---------開始儲存至CaseCustOutputF3----------------" ));
                foreach (var r in ret)
                {
                    // 找到是那筆CaseCustDetails 的 detailsID , 才能組出
                    var sql2 = string.Format("INSERT INTO CaseCustOutputF3 (DocNo,MasterId,DetailsId,CUST_ID_NO,RENT_BRANCH,RENT_KIND,CUSTOMER_NAME,NIGTEL_NO,MOBIL_NO,COMM_ADDR,CUST_ADD,BOX_NO,MODIFIED_DATE,RENT_START,RENT_END,MEMO) VALUES ('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}','{12}','{13}','{14}','{15}');", r.DocNo, r.MasterId, r.DetailsId, r.CUST_ID_NO, r.RENT_BRANCH, r.RENT_KIND, r.CUSTOMER_NAME, r.NIGTEL_NO, r.MOBIL_NO, r.COMM_ADDR, r.CUST_ADD, r.BOX_NO, r.MODIFIED_DATE, r.RENT_START, r.RENT_END, r.MEMO);
                    sb.Append(sql2);

                    var sql3 = string.Format("UPDATE CaseCustDetails SET SBoxSendStatus='02',Status='03' WHERE DetailsId='{0}' ", r.DetailsId);
                    sb.Append(sql3);
                }
                // var sql4 = sb.ToString();
                if (sb.Length!=0)
                {
                    
                    base.ExecuteNonQuery(sb.ToString());
                }
                else
                {
                    m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, string.Format("信息---------無保管箱資料----------------"));
                }

				try
				{
                    // 20211229, 因為今日收的保管箱可能只有部分.. (例如昨天要求5筆, 但今日只給2筆.. 那麼其餘三筆, 要寫成功, 且訊息要寫"無保管箱資料"..
                    // 先找前一個工作日..
                    //var predate = DateTime.Now.ToString("yyyy-MM-dd");
                    //var pSQL = string.Format("SELECT top 1 replace(convert(varchar, date, 111), '/','-') as prevDay   FROM PARMWorkingDay where date < '{0}' and Flag = 1 order by date desc", predate);
                    //var retResult = base.Search(pSQL);
                    if( retResult.Rows.Count>0)
					{
                        //var prevWorkingDate = retResult.Rows[0][0].ToString();
                        // 找出前一個上傳的保管箱.. 但未比對到的DetailsId
                        var sql4 = string.Format("SELECT * FROM CaseCustDetails WHERE SBoxSendStatus='01' AND QueryType='5' AND CONVERT(char(10), CreatedDate,126)='{0}'; ", prevWorkingDate);
                        var uItems = base.SearchList<CaseCustDetails>(sql4);
                        foreach (var r in uItems)
						{
                            var sql5 = string.Format("UPDATE CaseCustDetails SET SBoxSendStatus='02',Status='03',SboxQryMessage='無保管箱資料' WHERE DetailsId='{0}' ", r.DetailsId);
                            base.ExecuteNonQuery(sql5);
                        }
                    }

				}
				catch (Exception ex)
				{
                    m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, string.Format("信息---------讀取前一工作日錯誤----------------" + ex.Message.ToString()));
                }




                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, string.Format("信息---------開始儲存結束----------------"));


                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, string.Format("信息---------開始刪除下載檔案----------------"));
                //foreach (var f in successFiles)
                //    File.Delete(f);

                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, string.Format("信息---------結束下載檔案----------------"));

            }
            catch (Exception ex)
            {

                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "錯誤--------" + ex.Message.ToString() + "----------------");
            }

            return true;
        }

        /// <summary>
        /// 將FTP檔案拷貝到本機
        /// </summary>
        /// <returns></returns>
        public List<string> ReciveFile(string theday)
        {
            List<string> ret = new List<string>();
            bool isFtp = bool.Parse(ConfigurationManager.AppSettings["isFtp"].ToString());
            try
            {
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息---------從MFTP下載檔案作業開始----------------");

                // 判斷路徑是否存在
                if (!Directory.Exists(reciveloaclFilePath))
                {
                    Directory.CreateDirectory(reciveloaclFilePath);
                }


                if (isFtp) // 由Ftp 中, 取得檔案
                {




                    // 取得FTP指定目錄下的所有文件名稱
                    ArrayList fileList = reciveftpClient.GetFileList(reciveftpdir);

                    // 下載FTP指定目錄下的所有文件
                    foreach (var file in fileList)
                    {
                        if( ! file.ToString().StartsWith("Sbox_Download_" + theday + ".txt") )
                        {
                            continue;
                        }

                        m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------準備從MFTP下載檔案" + file + "----------------");

                        //  ftp 文件
                        string remoteFile = reciveftpClient.SetRemotePath(reciveftpdir) + "//" + file;

                        // 本地文件
                        string localFile = reciveloaclFilePath.TrimEnd('\\') + "\\" + file;

                        // 若已經存在，則先刪除掉舊的文件
                        if (File.Exists(localFile))
                        {
                            File.Delete(localFile);
                        }

                        reciveftpClient.GetFiles(remoteFile, localFile);
                        ret.Add(localFile);

                        reciveftpClient.DeleteFile(remoteFile);

                        m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------從MFTP下載檔案完成" + file + "----------------");

                        
                        
                    }
                }
                else // 直接取File目錄裏的檔案
                {
                    DirectoryInfo di = new DirectoryInfo(reciveloaclFilePath);
                    ret = di.GetFiles("*.*").Select(x=>x.FullName).ToList();
                }
            }
            catch (Exception ex)
            {
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------從MFTP下載檔案作業失敗，失敗原因：" + ex.Message + "----------------");

                ret = new List<string>();
            }
            finally
            {
                if (ret.Count()==0)
                {
                    m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------無資料下載----------------");
                }
            }

            return ret;
        }

    }
}
