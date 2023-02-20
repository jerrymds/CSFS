/// <summary>
/// 程式說明:  批次接收高檢署法務部資料
/// 2021/09/30
/// </summary>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CTBC.CSFS.BussinessLogic;
using CTBC.CSFS.Pattern;
using CTBC.FrameWork.Util;
using CTBC.CSFS.Models;
using System.Collections;
using System.Configuration;
using System.Data;
using System.IO;
using System.IO.Compression;
using System.Text.RegularExpressions;
using System.Xml;
using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json;

namespace CTBC.WinExe.CaseCust.Import
{
	class Program : BaseBusinessRule
	{
        private static FileLog m_fileLog;
        private static string sendftpserver;
        private static string sendport;
        private static string sendusername;
        private static string sendpassword;
        private static string sendftpdir;
        private static string sendloaclFilePath;
        private static FtpClient sendftpClient;


        private static string reciveftpserver;
        private static string reciveport;
        private static string reciveusername;
        private static string recivepassword;
        private static string reciveftpdir;
        private static string reciveloaclFilePath;
        private static FtpClient reciveftpClient;

        // MAIL 設定
        private static string mailFrom;
        private string[] mailFromTo;
        private static string mailHost;

        public static int intIndex = 0;

        // 案件編號
        public static string pDocNoPrefix;

        public static bool isContinue = true;

        public static string dtYYYMMDD;


        /// <summary>
        /// 所有檔案
        /// </summary>
        public static ArrayList fileList;

        public static List<FileGroups> lstGroup = new List<FileGroups>();

        public static int delZipDay = 0;
        public static int delDirDay = 0;
        public static string batchfilename = "";
        public static string metafilename = "";

        public static CaseCustImportBiz importBiz;

        static Program()
        {
            m_fileLog = new FileLog(ConfigurationManager.AppSettings["filelog"], AppDomain.CurrentDomain.FriendlyName.Replace(".vshost", "").Replace(".exe", ""));

            string dt = DateTime.Now.ToString("yyyyMMdd").Substring(4, 4);

            #region 收文設定

            reciveftpserver = ConfigurationManager.AppSettings["reciveftpserver"];
            reciveport = ConfigurationManager.AppSettings["reciveport"];
            reciveusername = ConfigurationManager.AppSettings["reciveusername"];
            recivepassword = ConfigurationManager.AppSettings["recivepassword"];


            reciveftpdir = ConfigurationManager.AppSettings["reciveftpdir"];
            reciveloaclFilePath = ConfigurationManager.AppSettings["reciveloaclFilePath"];
            reciveftpClient = new FtpClient(reciveftpserver, reciveusername, recivepassword, reciveport);
            delZipDay = int.Parse(ConfigurationManager.AppSettings["deleteZipDay"].ToString()); // 解ZIP後, 是否要把ZIP檔刪除
            delDirDay = int.Parse(ConfigurationManager.AppSettings["deleteDirDay"].ToString()); // 解ZIP後, 是否要把目錄刪除

            batchfilename = ConfigurationManager.AppSettings["batchfilename"].ToString();
            metafilename = ConfigurationManager.AppSettings["metafilename"].ToString();

            #endregion

            #region 發文設定

            sendftpserver = ConfigurationManager.AppSettings["sendftpserver"];
            sendport = ConfigurationManager.AppSettings["sendport"];
            sendusername = ConfigurationManager.AppSettings["sendusername"];
            sendpassword = ConfigurationManager.AppSettings["sendpassword"];


            sendftpdir = ConfigurationManager.AppSettings["sendftpdir"];
            sendloaclFilePath = ConfigurationManager.AppSettings["sendloaclFilePath"];
            sendftpClient = new FtpClient(sendftpserver, sendusername, sendpassword, sendport);

            #endregion

            #region mail 設定

            mailFrom = ConfigurationManager.AppSettings["mailFrom"];
            mailHost = ConfigurationManager.AppSettings["mailHost"];

            #endregion

            // 當下日期YYYMMDD
            pDocNoPrefix = ConvertYYYMMDD();

            // 當下日期YYY/MM/DD
            dtYYYMMDD = ConvertToYYYMMDD();

            importBiz = new CaseCustImportBiz();
        }


        static void Main(string[] args)
		{
            Program mainProgram = new Program();
            mainProgram.Process();
        }


        private void Process()
        {
            // 從ftp 收文

            bool isExsit = ReciveFile1();
            // 是否成功下載到本地
            if (isExsit)
            {
                // 匯入DB
                InsertDB(reciveloaclFilePath);
            }


            // 20211008
            // 在網頁上, 主管審核後, 將 UploadStatus='Y', 就讀取檔案, 上傳回總務
            // 待ReturnFile程式完成後, 再回頭改此段上傳回文....

            // 上傳檔案到ftp
            SendFile();
        }
        /// <summary>
        /// 20190731,Written by  WistronITS Patrick 
        /// 修改收檔規則, 改由ftp指定路徑存入ZIP檔, 解開後,把所有的檔案 存入lstGroup
        /// </summary>
        /// <returns></returns>
        private bool ReciveFile1()
        {
            string msg = string.Empty;
            try
            {
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息---------從MFTP下載檔案作業開始----------------");

                // 判斷路徑是否存在
                if (!Directory.Exists(reciveloaclFilePath))
                {
                    Directory.CreateDirectory(reciveloaclFilePath);
                }

                string ftp = ConfigurationManager.AppSettings["ftp"].ToString();
                bool bFtp = bool.Parse(ftp);

                if (bFtp)
                {
                    msg = string.Format("\t開啟Ftp連線");
                    m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, msg);

                    ArrayList allZips = reciveftpClient.GetFileList(reciveftpdir);

                    msg = string.Format("\t共計有{0}個zip檔", allZips.Count.ToString());
                    m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, msg);
                    foreach (string zipfile in allZips)
                    {
                        reciveftpClient.GetFiles(reciveftpdir + "/" + zipfile, reciveloaclFilePath + "\\" + zipfile);
                    }
                    msg = string.Format("\t\t下載zip檔 : {0} ", string.Join(",", allZips.ToArray()));
                    m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, msg);

                    #region 刪除Zip 檔

                    foreach (var zipfile in allZips)
                    {
                        reciveftpClient.DeleteFile(reciveftpdir + "/" + zipfile);
                    }
                    msg = string.Format("\t\t刪除 mftp上的zip檔 : {0} ", string.Join(",", allZips.ToArray()));
                    m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, msg);
                    #endregion
                }



                DirectoryInfo allDi = new DirectoryInfo(reciveloaclFilePath + "\\");

                // 過濾已處理過的BatchNo

                FileInfo[] allzips = BatchHasRead(allDi.GetFiles("*.zip"));

                if (allzips.Count() == 0)
                {
                    Console.WriteLine("無新的批號，勿須要處理");
                    msg = string.Format("\t無新的批號，勿須要處理");
                    m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, msg);
                    msg = string.Format("結束處理======================\r\n\r\n\r\n");
                    m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, msg);
                    return false;
                }

                msg = string.Format("\t\t過濾已Parse批次 :{0} ", string.Join(",", allzips.Select(x => x.Name)));
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, msg);

                foreach (FileInfo fi in allzips)
                {
                    string batchNo = fi.Name.Replace(".zip", "");
                    try
                    {
                        //清空, 若原有目錄有資料
                        if (Directory.Exists(reciveloaclFilePath + "\\" + batchNo))
                            System.IO.Directory.Delete(reciveloaclFilePath + "\\" + batchNo, true);
                        System.IO.Directory.CreateDirectory(reciveloaclFilePath + "\\" + batchNo);
                        System.IO.Compression.ZipFile.ExtractToDirectory(fi.FullName, reciveloaclFilePath + "\\");
                        //allBatchDirs.Add(fi.FullName.Replace(".zip", "") + "\\");

                        msg = string.Format("\t\t解開{0}批次, 到 {1}  ", batchNo, reciveloaclFilePath + "\\" + fi.Name.Replace(".zip", "") + "\\");
                        m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, msg);

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("解壓縮失敗!!, 檔名" + fi.FullName);
                        msg = string.Format("\t\t解壓縮失敗!!, 檔名 ", batchNo);
                        m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, msg);
                    }

                    if (delZipDay == 0) // 當場刪除
                    {
                        fi.Delete();
                        msg = string.Format("\t\t刪除{0} ", fi.Name);
                        m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, msg);
                    }

                    if (delDirDay == 0) //當場刪除, 2021-11-16, 決議留在Enforce_Recv中, 以讓畫面可以帶出原TXT及PDF
                    {
                        //if (Directory.Exists(reciveloaclFilePath + "\\" + batchNo))
                        //{
                        //    System.IO.Directory.Delete(reciveloaclFilePath + "\\" + batchNo, true);
                        //    //Directory.Delete(reciveloaclFilePath + "\\" + batchNo);
                        //    msg = string.Format("\t\t刪除目錄{0} ", batchNo);
                        //    m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, msg);
                        //}
                    }

                }


                #region 開始存入GssDoc

                List<GssDoc> gssList = new List<GssDoc>();
                List<GssDoc_Detail> gssDetails = new List<GssDoc_Detail>();


                #region 把目錄中每一個目錄, 全部讀進gssList, gssDetails

                msg = string.Format("\t\t開始Parse json檔 ");
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, msg);

                foreach (var di in allDi.GetDirectories())
                {
                    try
                    {
                        // 20190911, 若在allzips 中的批號, 才需要繼續Parsing 檔.. 因此, 就不需要在搬到備份的目錄了...
                        if (!allzips.Select(x => x.Name.Replace(".zip", "")).Contains(di.Name))
                            continue;

                        msg = string.Format("\t\t\t開始 Parse 批號{0} ..... ", di.Name);
                        m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, msg);

                        GssDoc gss = new GssDoc() { DocType = 1, BatchNo = di.Name, CreatedDate = DateTime.Now, TransferType = "2" }; // DocType = 1, 表示轉新外來文,     2 = 轉線上投單
                        batchmetadata bmdata = new batchmetadata();

                        string strMetaFileName = di.FullName + "\\" + batchfilename;
                        FileInfo sFi = new FileInfo(strMetaFileName);
                        if (sFi.Exists)
                        {
                            using (StreamReader sr = new StreamReader(di.FullName + "\\" + batchfilename, Encoding.UTF8))
                            {
                                string strBatchmetadata = sr.ReadToEnd();
                                bmdata = JsonConvert.DeserializeObject<batchmetadata>(strBatchmetadata);
                                gss.Batchmetadata = strBatchmetadata;
                                gss.CompanyID = bmdata.CompanyId;
                                gss.BatchDate = DateTime.Parse(bmdata.BatchDate.ToString());
                            }

                            #region 抓每個文號內資料, 寫入GssDoc_Detail
                            string metaDir = di.FullName + "\\";
                            int countDir = 0;
                            foreach (var _CnoNoInfo in bmdata.CnoNoInfo)  // 讀出BatchMetaData中, 共有幾個檔
                            {
                                try
                                {
                                    #region 各文號, Parse
                                    Guid caseId = Guid.NewGuid();

                                    GssDoc_Detail gdd = new GssDoc_Detail() { BatchNo = di.Name, DocNo = _CnoNoInfo, CreatedDate = DateTime.Now, CaseId = caseId };
                                    string cnoDir = metaDir + _CnoNoInfo + "\\";
                                    metadata mdata = new metadata();
                                    using (StreamReader sr = new StreamReader(cnoDir + metafilename, Encoding.UTF8))
                                    {
                                        string strmetadata = sr.ReadToEnd();
                                        mdata = JsonConvert.DeserializeObject<metadata>(strmetadata);
                                        gdd.Metadata = strmetadata;
                                    }
                                    gdd.CaseKind = mdata.CaseKind;

                                    bool isSuccess = true;
                                    FileGroups fg = new FileGroups() { IsExist = false, FileName25 = _CnoNoInfo, CaseID = caseId };
                                    int csvCount = 0;
                                    int pdfCount = 0;
                                    int diCount = 0;


                                    foreach (var f in mdata.Files)
                                    {
                                        var aaa1 = cnoDir + f.FileName;
                                        FileInfo fi = new FileInfo(aaa1);
                                        if (!fi.Exists)
                                        {
                                            isSuccess = false;
                                            gdd.ParserStatus = 2;
                                            gdd.ParserMessage = aaa1 + "檔案找不到!";
                                            m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "\t\t\t" + gdd.ParserMessage);
                                        }
                                        Files _f = new Files() { Extension = fi.Extension, Name = fi.Name };



                                        if (fi.Extension.ToUpper() == ".CSV") 
                                        {
                                            fg.TXT = fi.FullName.Replace(reciveloaclFilePath + "\\", "");
                                            csvCount++;
                                        }


                                        // 20220831.. 若發現有ZIP檔, 表示CSV檔被包在ZIP檔中, 要再解開...
                                        if (string.IsNullOrEmpty(fg.TXT) && fi.Extension.ToUpper() == ".ZIP" )
                                        {
                                            
                                            #region 解開ZIP...取得裏面的CSV 
                                            try
                                            {
                                                //清空, 若原有目錄有資料
                                                //if (Directory.Exists(reciveloaclFilePath + "\\" + batchNo))
                                                //    System.IO.Directory.Delete(reciveloaclFilePath + "\\" + fi.Name, true);
                                                //System.IO.Directory.CreateDirectory(reciveloaclFilePath + "\\" + fi.Name);
                                                System.IO.Compression.ZipFile.ExtractToDirectory(fi.FullName, reciveloaclFilePath + "\\" + _CnoNoInfo + "\\" + _CnoNoInfo);
                                                //allBatchDirs.Add(fi.FullName.Replace(".zip", "") + "\\");
                                                // 20220916, 找這個目錄底下第一個CSV檔... 只找第一個... 
                                                DirectoryInfo di2 = new DirectoryInfo(reciveloaclFilePath + "\\" + _CnoNoInfo + "\\" + _CnoNoInfo);
                                                var fi2 = di2.GetFiles("*.csv", SearchOption.AllDirectories);
                                                if (fi2.Count() ==1 ) // zip檔中, 只能有一個CSV
                                                {
                                                    fg.TXT = fi2.First().FullName.Replace(reciveloaclFilePath + "\\", "");
                                                    msg = string.Format("\t\t解開{0}批次, 找到 {1}  ", fi.Name, fg.TXT);
                                                    m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, msg);
                                                    csvCount++;
                                                }
                                                else
                                                {
                                                    fg.TXT = "";// 表示找不到CSV檔...
                                                    msg = string.Format("\t\t找不到ZIP檔({0})中的CSV檔 / 或超過1個以上的CSV檔 ", fi.Name);
                                                    m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, msg);
                                                }


                                            }
                                            catch (Exception ex)
                                            {
                                                Console.WriteLine("解壓縮失敗!!, 檔名" + fi.FullName);
                                                msg = string.Format("\t\t解壓縮失敗!!, 檔名 ", fi.Name);
                                                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, msg);
                                            } 
                                            #endregion


                                        }

                                        if (fi.Extension.ToUpper() == ".DI")
                                        {
                                            fg.DI = fi.FullName.Replace(reciveloaclFilePath + "\\", "");
                                            diCount++;
                                        }
                                        if (fi.Extension.ToUpper() == ".PDF")
                                        {
                                            fg.PDF = fi.FullName.Replace(reciveloaclFilePath + "\\", "");
                                            pdfCount++;
                                        }

                                        //ftpClient.GetFiles("轉新外來文/" + docBatch + "/" + _CnoNoInfo + "/" + f.FileName, metaDir + "\\" + f.FileName);
                                    }


                                    if (! (csvCount == 1 && pdfCount == 1 && diCount == 1) )
                                    {
                                        // 表示有二個以上的CSV檔....
                                        fg.TXT = "";// 表示找不到CSV檔...
                                        fg.DI = "";
                                        fg.PDF = "";
                                        msg = string.Format("\t\t 案件{0} 中, 有DI,CSV, PDF檔數量不是各有一個, 所以附件檔錯誤 ", _CnoNoInfo);
                                        m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, msg);
                                    }



                                    // 20190828, Patrick 新增檢核條件, TXT跟DI檔是必要, PDF是非必要
                                    // 20220922, 順便可以Insert 一筆CaseCustMaster 提示使用者....
                                    if (string.IsNullOrEmpty(fg.TXT) || string.IsNullOrEmpty(fg.DI))
                                    {
                                        isSuccess = false;
                                        // 20190828, Patrick , 看下面的程式, 若沒有TXT跟DI檔,只是發出Mail
                                        fg.IsExist = true;
                                        gdd.FileNames = string.Join(",", mdata.Files.Select(x => x.FileName));
                                        gdd.ParserStatus = 2;
                                        gdd.ParserMessage = "失敗, 原因: 缺少CSV或DI檔";
                                        msg = string.Format("\t\t\t\tParse 文號{0} 失敗, 原因: 缺少CSV或DI檔", _CnoNoInfo);
                                        m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, msg);



                                        // ----------------------------------
                                        // 20220921..... 沒有DI或CSV檔, 也要寫成立一個案號.....
                                        // 20220921, 若有格式錯誤, 則打包原來案件為一毎ZIP檔, 放在  Enforce\Enforce_send .. 檔名 案號_Error.zip

                                        #region 沒有DI或CSV檔, 也要寫成立一個案號
                                        var newFileGroup = new FileGroups();
                                        newFileGroup.CaseID = Guid.NewGuid();
                                        newFileGroup.FileName25 = di.Name + "\\" + _CnoNoInfo;
                                        string CaseNo = CreateErrorCase(newFileGroup, "附件檔有誤");
                                        #endregion

                                        #region 若有格式錯誤, 則打包原來案件為一毎ZIP檔
                                        var QfileName3 = formatErrorZip(reciveloaclFilePath, newFileGroup, "附件檔有誤");
                                        var ddd = QfileName3;
                                        // 寫入Qfilename3 ...
                                        string sqlUpdate = @"Update CaseCustMaster Set QFileName3='" + QfileName3 + "' WHERE NewID='" + newFileGroup.CaseID + "';";
                                        base.ExecuteNonQuery(sqlUpdate);

                                        #endregion

                                    }

                                    if (isSuccess)
                                    {
                                        fg.IsExist = false;
                                        gdd.FileNames = string.Join(",", mdata.Files.Select(x => x.FileName));
                                        gdd.ParserStatus = 1;
                                        gdd.ParserMessage = "成功";
                                        msg = string.Format("\t\t\t\tParse 文號{0} 成功, CaseID={1} ", _CnoNoInfo, caseId);
                                        m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, msg);
                                    }
                                    gssDetails.Add(gdd);
                                    #endregion
                                    countDir++;
                                    lstGroup.Add(fg);
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine("Error " + _CnoNoInfo + "Parsing Error");
                                    msg = string.Format("\t\tParse 文檔錯誤{0} ", _CnoNoInfo);
                                    m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, msg);
                                }

                            } // end      foreach (var _CnoNoInfo in bmdata.CnoNoInfo)
                            #endregion

                            #region 檢查此批號記錄的TotalNumber, 是否有實際路徑, 若無, 則寫整批異常
                            if (bmdata.TotalNumber == countDir)
                            {
                                gss.ParserStatus = 1;
                                gss.ParserMessage = "成功";
                                gssList.Add(gss);
                                msg = string.Format("\t\t\tParse 批號{0} 成功 ", di.Name);
                                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, msg);
                            }
                            else
                            {
                                gss.ParserStatus = 2;
                                gss.ParserMessage = "批次數量TotalNumber 與 實際路徑數量不符";
                                gssList.Add(gss);
                                Console.WriteLine("Error " + di.Name + "Parsing Error");
                                msg = string.Format("\t\tParse 批號錯誤{0} ,批次數量TotalNumber 與 實際路徑數量不符", di.Name);
                                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, msg);
                            }
                            #endregion
                        }
                        else
                        {
                            msg = string.Format("\t\t\t開始 批號{0} 底下的BatchMetaData讀不到 ", di.Name);
                            m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, msg);
                            gss.ParserStatus = 2;
                            gss.ParserMessage = msg;
                            gssList.Add(gss);
                        }
                    }
                    catch (Exception ex)
                    {

                        Console.WriteLine("Error " + di.Name + "Parsing Error");
                        msg = string.Format("\t\tParse 批號錯誤{0} ", di.Name);
                        m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, msg);
                    }
                } // foreach(var di in allDi.GetDirectories())

                #endregion

                // 儲存進DB
                if (gssList.Count() > 0)
                {
                    int retStatus = SaveGssDoc(gssList, gssDetails);
                    if (retStatus < 0)
                    {
                        Console.WriteLine("Error in SaveGssDoc");
                        msg = string.Format("\t\t儲存GssDoc錯誤 ");
                        m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, msg);
                    }
                    else
                    {
                        msg = string.Format("\t\t儲存GssDoc成功 ");
                        m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, msg);
                    }
                }
                else
                {
                    msg = string.Format("\t\t目前沒有批號 ");
                    m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, msg);
                }

                #endregion

                #region 發出email通知
                PARMCodeBIZ pbiz = new PARMCodeBIZ();

                var gssDocMail = pbiz.GetParmCodeByCodeType("GssDocNoticeMail").FirstOrDefault();
                string[] mailTo = gssDocMail.CodeMemo.Split(',');

                if (gssDocMail != null)
                {
                    foreach (var gssBatchNo in gssList.Where(x => x.ParserStatus == 1))
                    {
                        var gssDocs = gssDetails.Where(x => x.BatchNo == gssBatchNo.BatchNo).ToList();
                        var errDocs = gssDocs.Where(x => x.ParserStatus == 2).ToList(); // 如果是2, 表示, 有這筆有錯

                        try
                        {
                            // 20190912, 討論後, 若這批共有3個文號, 而且3個都錯,就定義為全錯
                            if (gssDocs.Count() == errDocs.Count() && errDocs.Count() > 0)
                            {
                                noticeMail_Fail_All(mailTo, "", gssBatchNo.BatchNo, DateTime.Now, gssDocs.Count().ToString(), string.Join(",", gssDocs.Select(x => x.DocNo)), "整批錯誤!!");
                            }
                            else
                            {
                                if (errDocs.Count() > 0) // 表示其中有錯...
                                {
                                    noticeMail_Fail(mailTo, gssBatchNo.CompanyID, gssBatchNo.BatchNo, (DateTime)gssBatchNo.BatchDate, gssDocs.Count().ToString(), string.Join(",", gssDocs.Select(x => x.DocNo)), string.Join(",", errDocs.Select(x => x.DocNo)));
                                }
                                else // 全對
                                {
                                    noticeMail_Success(mailTo, gssBatchNo.CompanyID, gssBatchNo.BatchNo, (DateTime)gssBatchNo.BatchDate, gssDocs.Count().ToString(), string.Join(",", gssDocs.Select(x => x.DocNo)));
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            msg = string.Format("\t\t 發生錯誤{0}", ex.Message.ToString());
                            m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, msg);
                        }
                    }
                    // 整批異常
                    foreach (var gssBatchNo in gssList.Where(x => x.ParserStatus == 2))
                    {
                        var gssDocs = gssDetails.Where(x => x.BatchNo == gssBatchNo.BatchNo).ToList();

                        try
                        {
                            if (gssDocs.Count() == 0) // 表示Batch層次, 就錯了.. 整批錯誤, 所以沒有gssDetails.......
                            {
                                noticeMail_Fail_All(mailTo, "", gssBatchNo.BatchNo, DateTime.Now, gssDocs.Count().ToString(), string.Join(",", gssDocs.Select(x => x.DocNo)), "整批錯誤!!");
                            }
                        }
                        catch (Exception ex)
                        {
                            msg = string.Format("\t\t 發生錯誤{0}", ex.Message.ToString());
                            m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, msg);
                        }
                    }


                }

                #endregion

            }
            catch (Exception ex)
            {

            }

            return true;
        }



        private static int SaveGssDoc(List<GssDoc> gssList, List<GssDoc_Detail> gssDetails)
        {
            //throw new NotImplementedException();
            int status = 0;
            try
            {
                //using (GssEntities ctx = new GssEntities())
                //{
                //    ctx.GssDoc.AddRange(gssList);
                //    ctx.GssDoc_Detail.AddRange(gssDetails);
                //    ctx.SaveChanges();
                //}

                var ret = importBiz.insertGSS(gssList, gssDetails);
                status = ret ? 0 : -1;

            }
            catch (Exception ex)
            {
                status = -1;
                Console.WriteLine("Error in SaveChanges");
            }
            return status;
        }


        private static void noticeMail_Success(string[] mailFromTo, string companyid, string batchno, DateTime batchdate, string totNum, string docNos)
        {
            string mailFrom = ConfigurationManager.AppSettings["mailFrom"];
            //string[] mailFromTo = ConfigurationManager.AppSettings["mailTo"].ToString().Split(',');
            //   批號+建檔成功XX筆-外來文A
            string subject = string.Format("{0} 建檔成功 {1} 筆-外來文A", batchno, totNum);   //"e化公文管理系統-集作拋轉公文排程執行完畢";
                                                                                      //20190822 宏祥 update start
                                                                                      //string body = "外來文系統 RACF 登入錯誤，錯誤原因：" + InitMessage;
                                                                                      //            string body = @"公司代碼: {0}\r\n
                                                                                      //批次批號: {1}\r\n
                                                                                      //批次執行日期時間: {2}\r\n
                                                                                      //轉換類型: 轉新外來文\r\n
                                                                                      //介接文號總數: {3}\r\n
                                                                                      //公文文號清單: {4}\r\n
                                                                                      //批次執行成功!
                                                                                      //
                                                                                      //
                                                                                      //本信件由系統直接發送!請勿直接回覆，謝謝!\r\n";
            string body = "公司代碼： {0}\r\n";
            body += "批次批號： {1}\r\n";
            body += "批次執行日期時間： {2}\r\n";
            body += "轉換類型： 轉線上投單\r\n";
            body += "介接文號總數： {3}\r\n";
            body += "公文文號清單： {4}\r\n";
            body += "批次執行成功!\r\n\r\n\r\n";
            body += "本信件由系統直接發送!請勿直接回覆，謝謝!\r\n";
            //20190822 宏祥 update end
            body = string.Format(body, companyid, batchno, batchdate.ToLongDateString(), totNum, docNos);
            string host = ConfigurationManager.AppSettings["mailHost"];
            UtlMail.SendEmail(mailFrom, mailFromTo, subject, body, host);
        }

        private static void noticeMail_Fail(string[] mailFromTo, string companyid, string batchno, DateTime batchdate, string totNum, string docNos, string faildocs)
        {
            string mailFrom = ConfigurationManager.AppSettings["mailFrom"];
            //string[] mailFromTo = ConfigurationManager.AppSettings["mailTo"].ToString().Split(',');
            // 批號+建檔成功XX筆/失敗xx筆-外來文A
            string[] fdocs = faildocs.Split(',');
            int totalSuccess = int.Parse(totNum) - fdocs.Length;
            string subject = string.Format("{0} 建檔成功 {1} 筆/失敗 {2} 筆-外來文A", batchno, totalSuccess.ToString(), fdocs.Length.ToString());
            //string body = "外來文系統 RACF 登入錯誤，錯誤原因：" + InitMessage;
            //20190822 宏祥 update start
            //            string body = @"公司代碼： {0}\r\n
            //批次批號： {1}\r\n
            //批次執行日期時間： {2}\r\n
            //轉換類型： 轉新外來文\r\n
            //介接文號總數： {3}\r\n
            //公文文號清單： {4}\r\n
            //批次執行失敗!
            //
            //失敗公文文號清單及訊息： \r\n
            //{5}
            //本信件由系統直接發送!請勿直接回覆，謝謝!\r\n";
            string body = "公司代碼： {0}\r\n";
            body += "批次批號： {1}\r\n";
            body += "批次執行日期時間： {2}\r\n";
            body += "轉換類型： 轉線上投單\r\n";
            body += "介接文號總數： {3}\r\n";
            body += "公文文號清單： {4}\r\n";
            body += "批次執行失敗!\r\n\r\n";
            body += "失敗公文文號清單及訊息：\r\n";
            body += "{5}\r\n\r\n\r\n";
            body += "本信件由系統直接發送!請勿直接回覆，謝謝!\r\n";
            //20190822 宏祥 update end
            body = string.Format(body, companyid, batchno, batchdate.ToLongDateString(), totNum, docNos, faildocs);
            string host = ConfigurationManager.AppSettings["mailHost"];
            UtlMail.SendEmail(mailFrom, mailFromTo, subject, body, host);
        }

        private static void noticeMail_Fail_All(string[] mailFromTo, string companyid, string batchno, DateTime batchdate, string totNum, string docNos, string faildocs)
        {
            string mailFrom = ConfigurationManager.AppSettings["mailFrom"];
            //string[] mailFromTo = ConfigurationManager.AppSettings["mailTo"].ToString().Split(',');
            // 批號+建檔失敗-外來文A
            string[] fdocs = faildocs.Split(',');
            string subject = string.Format("{0} 建檔失敗-外來文A", batchno, totNum, fdocs.Length.ToString());
            //string body = "外來文系統 RACF 登入錯誤，錯誤原因：" + InitMessage;
            //20190822 宏祥 update start
            //            string body = @"公司代碼: {0}\r\n
            //批次批號: {1}\r\n
            //批次執行日期時間: {2}\r\n
            //轉換類型: 轉新外來文\r\n
            //介接文號總數: {3}\r\n
            //公文文號清單: {4}\r\n
            //批次執行失敗!
            //
            //失敗公文文號清單及訊息: \r\n
            //{5}
            //本信件由系統直接發送!請勿直接回覆，謝謝!\r\n";
            string body = "公司代碼： {0}\r\n";
            body += "批次批號： {1}\r\n";
            body += "批次執行日期時間： {2}\r\n";
            body += "轉換類型： 轉線上投單\r\n";
            body += "介接文號總： {3}\r\n";
            body += "公文文號清單： {4}\r\n";
            body += "批次執行失敗!\r\n\r\n";
            body += "失敗公文文號清單及訊息： \r\n";
            body += "{5}\r\n\r\n\r\n";
            body += "本信件由系統直接發送!請勿直接回覆，謝謝!\r\n";
            //20190822 宏祥 update end
            body = string.Format(body, companyid, batchno, batchdate.ToLongDateString(), totNum, docNos, faildocs);
            string host = ConfigurationManager.AppSettings["mailHost"];
            UtlMail.SendEmail(mailFrom, mailFromTo, subject, body, host);
        }



        /// <summary>
        /// 檢查, 此一批號, 是否已經讀取過了
        /// </summary>
        /// <param name="docBatch"></param>
        /// <returns></returns>
        private static FileInfo[] BatchHasRead(FileInfo[] docBatchNo)
        {
            List<FileInfo> result = new List<FileInfo>();



			foreach (var d in docBatchNo)
			{
                if( ! importBiz.checkExist( d.Name.Replace(".zip","")))
				{
                    result.Add(d);
				}
				else
				{
					string msg = string.Format("\t\t批號{0}已重覆，不再匯入", d.Name.Replace(".zip", ""));
					m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, msg);
				}
			}


			return result.ToArray();
        }

        /// <summary>
        /// 上傳檔案到ftp ... 
        /// </summary>
        public void SendFile()
        {
            try
            {
                // 取得要上傳的檔案
                IList<CaseCustMaster> list = GetFiles();

                //  逐筆處理
                foreach (CaseCustMaster item in list)
                {
                    m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息-------- 上傳檔案到MFTP作業開始----------------");

                    # region 上傳本地文件到指定目錄
                    /*
                      * 20180326,PeterHiseh : 依據新的CR規格修改如下
                      * 1.要上傳的檔案有三種格式 : .sw, .di, .zip
                      * 2..zip檔案包含兩個 .txt (基本資料與交易明細)，且需要加密，密碼為'822822'+案件編號(去除第一碼)
                      */
                    string localROpenFileName = sendloaclFilePath + @"\" + item.ROpenFileName;
                    string localRFileTransactionFileName = sendloaclFilePath + @"\" + item.RFileTransactionFileName;
                    string localRFileSW = sendloaclFilePath + @"\" + item.DocNo + "_" + item.Version + ".sw";
                    string localRFileDI = sendloaclFilePath + @"\" + item.DocNo + "_" + item.Version + ".di";


                    // 20211217, 已將文字檔壓縮功能, 搬到CaseCustReturnFile中, 產生TXT後, 直接壓縮
                    // 將文字檔壓縮
                    //m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, string.Format("信息--------壓縮檔案 ({0}, {1})----------------", item.ROpenFileName, item.RFileTransactionFileName));

                    //List<string> FilenameList = new List<string>();

                    //if (System.IO.File.Exists(localROpenFileName))
                    //{
                    //    FilenameList.Add(localROpenFileName);
                    //}

                    //if (System.IO.File.Exists(localRFileTransactionFileName))
                    //{
                    //    FilenameList.Add(localRFileTransactionFileName);
                    //}

                    try
                    {
                        // 20211217, 已將文字檔壓縮功能, 搬到CaseCustReturnFile中, 產生TXT後, 直接壓縮
                        //m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, string.Format("信息--------案件{0}的文字檔案進行壓縮處理----------------", item.DocNo));

                        //// 臨時文件
                        //string zipFilename = sendloaclFilePath + @"\" + item.DocNo + "_" + item.Version + ".zip";

                        //if (!Directory.Exists(sendloaclFilePath))
                        //{
                        //    Directory.CreateDirectory(sendloaclFilePath);
                        //}

                        //// 壓縮密碼
                        //string password = "822822" + item.DocNo.Substring(1, item.DocNo.Length - 1);

                        //if (System.IO.File.Exists(zipFilename))
                        //{
                        //    m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, string.Format("信息--------案件{0}的進行舊壓縮檔刪除處理----------------", item.DocNo));

                        //    // 刪除舊檔案
                        //    System.IO.File.Delete(zipFilename);

                        //}

                        //CreateZip(sendloaclFilePath, zipFilename, FilenameList, password);

                        m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, string.Format("信息--------開始上傳案件{0}的相關檔案到MFTP上----------------", item.DocNo));

                        // 上傳 .sw檔案到FTP
                        if (System.IO.File.Exists(localRFileSW))
                        {
                            sendftpClient.SendFile(sendftpdir, localRFileSW);

                            m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------已上傳檔案" + localRFileSW + "到MFTP上----------------");
                        }

                        // 上傳 .di檔案到FTP
                        if (System.IO.File.Exists(localRFileDI))
                        {
                            sendftpClient.SendFile(sendftpdir, localRFileDI);

                            m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------已上傳檔案" + localRFileDI + "到MFTP上----------------");
                        }

                        // 上傳 .zip檔案到FTP
                        string zipFilename = sendloaclFilePath + @"\" + item.DocNo + "_" + item.Version + ".zip";
                        if (System.IO.File.Exists(zipFilename))
                        {
                            sendftpClient.SendFile(sendftpdir, zipFilename);

                            m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------已上傳檔案" + zipFilename + "到MFTP上----------------");
                        }

                        m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, string.Format("信息--------已上傳案件{0}的相關檔案到MFTP上----------------", item.DocNo));

                        // 根據主鍵更新
                        UpdateCaseCustQuery(item.NewID);

                    }
                    catch (Exception ex)
                    {
                        m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "程式異常，錯誤信息: " + ex.Source);
                        m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "程式異常，錯誤信息: " + ex.ToString());
                        m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "程式異常，錯誤信息: " + ex.StackTrace);

                    }

                    #endregion
                }

            }
            catch (Exception ex)
            {
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------上傳檔案到MFTP作業失敗，失敗原因：" + ex.Message + "----------------");
            }
        }

        public void InsertDB(string filePath)
        {
            try
            {

                string QFileName3 = string.Empty; // 20220921, 若有格式錯誤, 則打包原來案件為一毎ZIP檔, 放在  Enforce\Enforce_send .. 檔名 案號_Error.zip
                

                // 錯誤信息
                string sumerroMessage = "";

                // 格式錯誤筆數
                int erroCount = 0;

                //  正確筆數
                int sucessCount = 0;

                string isExistMessage = "";

                // 逐個檔案讀取,尚未匯入的
                foreach (FileGroups fileGroups in lstGroup.Where(t => (!t.IsExist)))
                {

                    try
                    {
                        #region 是否可以匯入DB
                        bool IsCan = true;

                        string strTxtName = fileGroups.TXT;
                        string strTxtNamePDF = fileGroups.PDF;

                        if (string.IsNullOrEmpty(strTxtName) || !System.IO.File.Exists(filePath + @"\" + fileGroups.TXT))
                        {
                            isExistMessage += fileGroups.FileName25 + "無對應的來文csv檔案\r\n";

                            IsCan = false;
                        }

                        if (string.IsNullOrEmpty(fileGroups.DI) || !System.IO.File.Exists(filePath + @"\" + fileGroups.TXT))
                        {
                            isExistMessage += fileGroups.FileName25 + "無對應的來文di檔案\r\n";

                            IsCan = false;
                        }

                        // 可以匯入DB
                        if (IsCan)
                        {
                            string strImportMessage = "";

                            // 是否存在PDF 文件
                            if (string.IsNullOrEmpty(fileGroups.PDF))
                            {
                                strImportMessage = "無對應的來文PDF檔案";
                            }

                            #region 讀取di 內容
                            Boolean diFormatFail = false;

                            // di全部文件內容
                            string strFileDI = System.IO.File.ReadAllText(filePath + @"\" + fileGroups.DI);

                            // 轉XML
                            XmlDocument xmlDoc = new XmlDocument();
                            xmlDoc.XmlResolver = null;
                            xmlDoc.LoadXml(strFileDI);

                            #region 來文日期

                            //string xmlLetterDate = xmlDoc.SelectSingleNode("//函/發文日期/年月日").InnerText;
                            string xmlLetterDate = GetXMLNode(xmlDoc, "//函/發文日期/年月日");
                            string strLetterDate = string.Empty;

                            if (string.IsNullOrEmpty(xmlLetterDate))
                            {
                                diFormatFail = true;

                            }
                            else
                            {
                                //Modify by Brian 20190524
                                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------原始來文日期：" + xmlLetterDate.ToString() + "----------------");

                                //來文年
                                strLetterDate = xmlLetterDate.Substring(4, 3);

                                //來文月
                                if (xmlLetterDate.IndexOf("月") - xmlLetterDate.IndexOf("年") == 3)
                                {
                                    strLetterDate += xmlLetterDate.Substring(xmlLetterDate.IndexOf("年") + 1, 2);
                                }
                                else
                                {
                                    strLetterDate += "0" + xmlLetterDate.Substring(xmlLetterDate.IndexOf("年") + 1, 1);
                                }

                                //來文日
                                if (xmlLetterDate.IndexOf("日") - xmlLetterDate.IndexOf("月") == 3)
                                {
                                    strLetterDate += xmlLetterDate.Substring(xmlLetterDate.IndexOf("月") + 1, 2);
                                }
                                else
                                {
                                    strLetterDate += "0" + xmlLetterDate.Substring(xmlLetterDate.IndexOf("月") + 1, 1);
                                }

                                // 來文日期日期為西元年YYYYMMDD
                                //strLetterDate = xmlLetterDate.Replace("中華民國", "").Replace("年", "").Replace("月", "").Replace("日", "");

                                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------加工來文日期：" + strLetterDate + "----------------");

                                //如果來文日期不足七碼，落錯誤件 
                                if (strLetterDate.Length != 7)
                                {
                                    diFormatFail = true;
                                    strImportMessage = "來文日期加工錯誤：" + strLetterDate;
                                }
                                else
                                {
                                    strLetterDate = (Convert.ToInt32(strLetterDate.Substring(0, 3)) + 1911).ToString() + "/" + strLetterDate.Substring(3, 2) + "/" + strLetterDate.Substring(5, 2);
                                }
                                //Modify End by Brian 20190524

                            }

                            #endregion

                            #region 來文機關

                            // 來文機關
                            // string xmlLetterDeptName = xmlDoc.SelectSingleNode("//函/發文機關/全銜").InnerText;
                            string xmlLetterDeptName = GetXMLNode(xmlDoc, "//函/發文機關/全銜");
                            string xmlLetterDeptNo = GetXMLNode(xmlDoc, "//函/發文機關/機關代碼");

                            if (string.IsNullOrEmpty(xmlLetterDeptName)) // 若是沒有全銜, 試著找.. 單位名...
                            {
                                try
                                {
                                    xmlLetterDeptName = GetXMLNode(xmlDoc, "//函/發文機關/單位名");
                                }
                                catch (Exception ex)
                                {

                                    diFormatFail = true;
                                    strImportMessage = "找不到  全銜 / 單位名：";
                                }

                            }



                            diFormatFail = diFormatFail || string.IsNullOrEmpty(xmlLetterDeptName);
                            diFormatFail = diFormatFail || string.IsNullOrEmpty(xmlLetterDeptNo);

                            #endregion

                            #region 來文字號

                            // 來文字號
                            string xmlLetterNo = string.Empty;
                            string xmlLetterNo_1 = GetXMLNode(xmlDoc, "//函/發文字號/字");
                            string xmlLetterNo_2 = GetXMLNode(xmlDoc, "//函/發文字號/文號/年度");
                            string xmlLetterNo_3 = GetXMLNode(xmlDoc, "//函/發文字號/文號/流水號");
                            string xmlLetterNo_4 = GetXMLNode(xmlDoc, "//函/發文字號/文號/支號");

                            // 20220921, 集作決定, 不檢查..文號的所有項目.. 只要有"字", 即可.

                            if (string.IsNullOrEmpty(xmlLetterNo_1))
                            {
                                diFormatFail = true;
                                xmlLetterNo = "";
                            }
                            else
                            {
                                xmlLetterNo = string.Format("{0}字第{1}{2}{3}號", xmlLetterNo_1, xmlLetterNo_2, xmlLetterNo_3, xmlLetterNo_4);
                            }

                            #endregion

                            #region 受文者

                            // 來文字號
                            //string xmlRecipient = xmlDoc.SelectSingleNode("//函/受文者/全銜").InnerText;
                            string xmlRecipient = GetXMLNode(xmlDoc, "//函/受文者/全銜");

                            diFormatFail = diFormatFail || string.IsNullOrEmpty(xmlRecipient);

                            #endregion

                            #region 承辦人員

                            // 承辦人員
                            string xmlInCharge = GetXMLNodeInCharge(xmlDoc, "//函/聯絡方式");

                            diFormatFail = diFormatFail || string.IsNullOrEmpty(xmlInCharge);

                            #endregion

                            #endregion

                            // 取得連接并開放連接
                            IDbConnection dbConnection = base.OpenConnection();

                            // 定義事務
                            IDbTransaction dbTransaction = null;

                            using (dbConnection)
                            {
                                // 開啟事務
                                dbTransaction = dbConnection.BeginTransaction();

                                if (diFormatFail)
                                {
                                    #region di檔案格式錯誤處理

                                    // 主表主鍵
                                    string strMainPK = Guid.NewGuid().ToString();
                                    // 20190911, 因為要記錄CaseID, 才能回到GssDoc_Detail中
                                    strMainPK = fileGroups.CaseID.ToString();

                                    #region 案件編號

                                    // 取得最大收編
                                    string strMaxDocNo = GetMaxDocNo(dbTransaction);

                                    // 取得流水號
                                    int intMaxDocNo = 0;

                                    if (strMaxDocNo == "")
                                    {
                                        intMaxDocNo = 0;
                                    }
                                    else
                                    {
                                        intMaxDocNo = Convert.ToInt32(strMaxDocNo.Substring(8, 5));
                                    }

                                    // 案件編號 規則：E+民國106+月份10+日16+00001(五碼流水編號)
                                    string strDocNo = GetDocNo(intMaxDocNo);

                                    #endregion

                                    // di檔案格式錯誤
                                    string sqlCaseCustQuery = @"    INSERT CaseCustMaster  ( NewID , DocNo , Version , RecvDate , QFileName , QFileName2, ROpenFileName, RFileTransactionFileName, Status , ImportFormFlag, ImportMessage, FileDI, LetterDate, LetterDeptName, LetterDeptNo, LetterNo, Recipient, CreatedDate , CreatedUser , ModifiedDate , ModifiedUser) VALUES  ( '" + strMainPK + @"' ,  '" + strDocNo + @"' ,   0 ,   GETDATE() ,   '" + strTxtName + @"' ,   '" + strTxtNamePDF + @"' ,   '' ,  '' ,   '98' ,   'Y' ,   '" + strImportMessage + @"' ,   '" + strFileDI + @"' ,   '" + strLetterDate + @"' ,   '" + xmlLetterDeptName + @"' ,   '" + xmlLetterDeptNo + @"' ,   '" + xmlLetterNo + @"' ,   '" + xmlRecipient + @"' ,   GETDATE() ,   'SYSTEM' ,  GETDATE() ,   'SYSTEM' )";

                                    base.ExecuteNonQuery(sqlCaseCustQuery, dbTransaction);

                                    dbTransaction.Commit();

                                    #endregion
                                }
                                else
                                {
                                    // di檔案格式正確
                                    #region 讀取TXT 檔案

                                    try
                                    {
                                        m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------讀取檔案" + strTxtName + "到DB作業開始----------------");

                                        #region 讀取資料

                                        // 行數
                                        int txtCount = 0;

                                        //  日期正則表達式
                                        Regex reg = new Regex(@"(([0-9]{3}[1-9]|[0-9]{2}[1-9][0-9]{1}|[0-9]{1}[1-9][0-9]{2}|[1-9][0-9]{3})(((0[13578]|1[02])(0[1-9]|[12][0-9]|3[01]))|((0[469]|11)(0[1-9]|[12][0-9]|30))|(02(0[1-9]|[1][0-9]|2[0-8]))))|((([0-9]{2})(0[48]|[2468][048]|[13579][26])|((0[48]|[2468][048]|[3579][26])00))0229)");

                                        string strLine = "";

                                        string erroMessage = "";

                                        // 主表主鍵
                                        string strMainPK = Guid.NewGuid().ToString();
                                        // 20190911, 因為要記錄CaseID, 才能回到GssDoc_Detail中
                                        strMainPK = fileGroups.CaseID.ToString();

                                        #region 案件編號

                                        // 取得最大收編
                                        string strMaxDocNo = GetMaxDocNo(dbTransaction);

                                        // 取得流水號
                                        int intMaxDocNo = 0;

                                        if (strMaxDocNo == "")
                                        {
                                            intMaxDocNo = 0;
                                        }
                                        else
                                        {
                                            intMaxDocNo = Convert.ToInt32(strMaxDocNo.Substring(8, 5));
                                        }

                                        // 案件編號 規則：E+民國106+月份10+日16+00001(五碼流水編號)
                                        string strDocNo = GetDocNo(intMaxDocNo);

                                        #endregion
                                        Regex f1 = new Regex(@"^\d{1,5}$");
                                        Regex f2 = new Regex(@"^(\w{10}|\d{8})$");
                                        //Regex f3 = new Regex(@"^(\d{11,12}|\d{16})$"); // 11,12, 16碼才能進...
                                        Regex f4 = new Regex(@"^[1-2]\d{7}$");
                                        Regex f5 = new Regex(@"^[1-2]\d{7}$");
                                        #region  逐筆取得檔案資料, 至imports

                                        List<CaseCustImportModel> imports = new List<CaseCustImportModel>();
                                        using (StreamReader sr = new StreamReader(filePath + @"\" + strTxtName, Encoding.UTF8))
                                        {
                                            while (sr.Peek() > 0)
                                            {
                                                string Line = sr.ReadLine();
                                                if (string.IsNullOrEmpty(Line))
                                                    continue;
                                                if (Line.Contains("查詢種類"))
                                                    continue;

                                                CaseCustImportModel f = new CaseCustImportModel() { isValid = true, ErrorMessage = "" };
                                                // 20221020, 先檢查, 是不是五個欄位...
                                                string[] nc = Line.Split(',');
                                                if (nc.Length != 5)
                                                {
                                                    f.isValid = false;
                                                    f.ErrorMessage += "不是5個欄位;";
                                                }
                                                // 檢查是不是有雙引號包著.
                                                for(int i=0;i<nc.Length;i++)
                                                {
                                                    if(! (nc[i].StartsWith("\"") && nc[i].EndsWith("\"")) ) 
                                                    {
                                                        f.isValid = false;
                                                        f.ErrorMessage +="第" + (i+1) + "個欄位, 沒有包雙引號;";
                                                    }
                                                }

                                                nc[0] = nc[0].Replace("\"", "").Trim();
                                                // 第一個欄位.. 必須是, 數字1-5, 其餘皆錯
                                                if (!f1.Match(nc[0]).Success)
                                                {
                                                    f.isValid = false;
                                                    f.ErrorMessage += "第一欄, 不是1-5的數字;";
                                                }
                                                else
                                                {
                                                    f.type = nc[0];
                                                }

                                                nc[1] = nc[1].Replace("\"", "").Trim();
                                                // 第二個欄位.. 身分證號, 行號
                                                if( !string.IsNullOrEmpty( nc[1]) )
                                                    if (!f2.Match(nc[1]).Success)
                                                    {
                                                        f.isValid = false;
                                                        f.ErrorMessage += "第二欄, 不是身分證號/統編;";
                                                    }
                                                    else
                                                    {
                                                        f.id = nc[1];
                                                    }

                                                // 第三個欄位.. 集作說, 不檢查......
                                                nc[2] = nc[2].Replace("\"", "").Trim();
                                                {
                                                    f.accno = nc[2];
                                                    if (f.accno.Length > 16) // 因為DB長度只開17碼, 所以最多只存16碼....
                                                        f.accno = f.accno.Substring(0, 16);
                                                }


                                                // 第四個欄位.. 日期一定8碼數字
                                                nc[3] = nc[3].Replace("\"", "").Trim();
                                                if (!string.IsNullOrEmpty(nc[3]))
                                                    if (!f4.Match(nc[3]).Success)
                                                    {
                                                        f.isValid = false;
                                                        f.ErrorMessage += "第四欄, 長度不是8碼西元年;";
                                                        f.sdate = nc[3];
                                                    }
                                                    else
                                                    {
                                                        f.sdate = nc[3];
                                                    }
                                                // 第五個欄位.. 日期一定8碼數字
                                                nc[4] = nc[4].Replace("\"", "").Trim();
                                                if (!string.IsNullOrEmpty(nc[4]))
                                                    if (!f5.Match(nc[4]).Success)
                                                    {
                                                        f.isValid = false;
                                                        f.ErrorMessage += "第五欄, 長度不是8碼西元年;";
                                                        f.edate = nc[4];
                                                    }
                                                    else
                                                    {
                                                        f.edate = nc[4];
                                                    }

                                                
                                                //if (f.isValid)
                                                {
                                                    f.caseid = fileGroups.CaseID;
                                                    imports.Add(f);  
                                                }
                                            }
                                        }
                                        #endregion


                                        // 20221020, 集作決定,若是部分錯誤, 也不匯入, 只提供附件檔及文號(要Insert Master)讓畫面可以下載 
                                        // 20221101, 若有格式錯誤,要換成CSV檔錯誤, 而不是附件檔有誤... 所以這段要全部取消...

                                        //if( imports.Count(x=>! x.isValid ) > 0)
                                        //{


                                        //    #region 沒有DI或CSV檔, 也要寫成立一個案號
                                        //    string CaseNo = CreateErrorCase2(fileGroups, "附件檔有誤", xmlLetterNo, xmlLetterDeptName);
                                        //    #endregion

                                        //    #region 若有格式錯誤, 則打包原來案件為一毎ZIP檔
                                        //    formatErrorZip2(filePath, fileGroups, "附件檔有誤");
                                        //    #endregion
                                        
                                        //}
                                        //else
                                        {
                                            #region imports 存入CaseCustImport


                                            var bRet = importBiz.insertCaseCustImport(imports);

                                            if (bRet)
                                                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------新增" + strTxtName + "到CaseCustImport 完成----------------");
                                            else
                                            {
                                                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------新增" + strTxtName + "到CaseCustImport 失敗----------------");
                                                return;
                                            }

                                            #endregion

                                            #region  從Imports 寫入CaseCustDetails

                                            // 20220822 ....若發現Details的全部都是  txt檔案格式錯誤 , 則必須把Master.Status 押成99... 
                                            // 若有其中一筆正常的.. 就必須要打電文.. 要產生...

                                            string strMasterStatus = "01"; // 是成功的狀態, 

                                            bRet = importBiz.insertCaseCustDetails(strDocNo, imports, strMainPK, m_fileLog, ref strMasterStatus);
                                            if (bRet)
                                                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------新增" + strTxtName + "到CaseCustDetails 完成----------------");
                                            else
                                            {
                                                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------新增" + strTxtName + "到CaseCustDetails 失敗----------------");
                                                return;
                                            }
                                            #endregion

                                            #region  Insert  CaseCustMaster 主表

                                            // Y:匯入的文檔有資料格式錯誤 N:格式正確
                                            string strImportFormFlag = "N";

                                            string sqlCaseCustQuery = @"    INSERT CaseCustMaster
                                                                   ( NewID ,DocNo ,Version ,RecvDate ,QFileName ,QFileName2,ROpenFileName,RFileTransactionFileName,
Status ,ImportFormFlag,ImportMessage,FileDI,LetterDate,LetterDeptName,
LetterDeptNo,LetterNo,Recipient,InCharge,CreatedDate ,CreatedUser ,ModifiedDate ,ModifiedUser)
                                                           VALUES  ( '" + strMainPK + @"' ,'" + strDocNo + @"' , 0 , 
GETDATE() , '" + strTxtName + @"' , '" + strTxtNamePDF + @"' , '" + strDocNo + @"_0_Base.txt' ,'" + strDocNo + @"_0_Detail.txt' ,                      
'" + strMasterStatus + "' , '" + strImportFormFlag + @"' , '" + strImportMessage + @"' , '" + strFileDI + @"' , '" + strLetterDate + @"' , '" + xmlLetterDeptName + @"' , 
'" + xmlLetterDeptNo + @"' , '" + xmlLetterNo + @"' , '" + xmlRecipient + @"' , '" + xmlInCharge + @"' , GETDATE() , 'SYSTEM' ,GETDATE() , 'SYSTEM' )";



                                            base.ExecuteNonQuery(sqlCaseCustQuery, dbTransaction);

                                            dbTransaction.Commit();

                                            #endregion

                                        }








                                        // 資料格式不正確
                                        if (erroMessage != "")
                                        {

                                            #region 輸出錯誤
                                            string pHtml = "<table border='1' cellpadding='1' cellspacing='0' style='width: 60%; border-color: #CCCCCC; border-collapse: collapse'>";

                                            pHtml += "<tr>";

                                            pHtml += "<td style='color: #039; white-space: nowrap; background-color: #DFF0D8;width: 30%'>匯入文檔名稱：</td>";

                                            pHtml += "<td width: 70%'>" + strTxtName + "</td>";

                                            pHtml += "</tr>";

                                            pHtml += "</table>";

                                            pHtml += "<table border='1' cellpadding='1' cellspacing='0' style='width: 60%; border-color: #CCCCCC; border-collapse: collapse'>";
                                            pHtml += "<tr>";
                                            pHtml += "<td style='color: #039; white-space: nowrap; background-color: #DFF0D8;width: 100%'>匯入失敗資料如下：</td>";

                                            // erroMessage = "匯入文檔名稱：" + file.Name + "\r\n" + "匯入失敗資料如下：\r\n" + erroMessage + "\r\n";

                                            erroMessage = pHtml + erroMessage + "</table><br />";

                                            // log 記錄
                                            m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, erroMessage);

                                            sumerroMessage = sumerroMessage + erroMessage; 
                                            #endregion
                                        }

                                        #endregion
                                    }
                                    catch (Exception ex)
                                    {
                                        dbTransaction.Rollback();

                                        isContinue = false;

                                        m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, " 事務回滾, 錯誤訊息: " + ex.ToString());


                                    }

                                    #endregion
                                }
                            }
                        }
                        else
                        {
                            // ----------------------------------
                            // 20220921..... 沒有DI或CSV檔, 也要寫成立一個案號.....
                            // 20220921, 若有格式錯誤, 則打包原來案件為一毎ZIP檔, 放在  Enforce\Enforce_send .. 檔名 案號_Error.zip

                            #region 沒有DI或CSV檔, 也要寫成立一個案號
                            string CaseNo = CreateErrorCase(fileGroups, "附件檔有誤");
                            #endregion

                            #region 若有格式錯誤, 則打包原來案件為一毎ZIP檔
                            formatErrorZip(filePath, fileGroups, "附件檔有誤");
                            #endregion


                        }
                        #endregion
                    }
                    catch (Exception ex)
                    {
                        m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, " 處理案件 " + fileGroups.CaseID + ", 發生未知錯誤訊息: " + ex.ToString());
                        #region 沒有DI或CSV檔, 也要寫成立一個案號
                        string CaseNo = CreateErrorCase(fileGroups, "發生未知錯誤訊息");
                        #endregion

                        #region 若有格式錯誤, 則打包原來案件為一毎ZIP檔
                        formatErrorZip(filePath, fileGroups, "發生未知錯誤訊息");
                        #endregion
                    }
                }

                //  檔案格式錯誤
                if (sumerroMessage != "")
                {
                    #region 發送mail 通知

                    //  主旨
                    string strSubject = "主旨: 【通知】" + dtYYYMMDD + " 線上投單電子文第一批，失敗" + erroCount + "(筆數)、成功" + sucessCount + "(筆數)，總計" + (erroCount + sucessCount) + "(筆數)";

                    // 收件人
                    mailFromTo = getMailFromTo("SendMail");

                    UtlMail.SendEmailIsBodyHtml(mailFrom, mailFromTo, strSubject, sumerroMessage, mailHost);

                    # endregion
                }

                //  檔案缺少發送mail
                if (isExistMessage != "")
                {
                    #region 發送mail 通知

                    //  主旨
                    string strSubject = "主旨: 【通知】" + dtYYYMMDD + " 線上投單電子文第一批，缺少文件";

                    // 收件人
                    mailFromTo = getMailFromTo("SendMail");

                    //20190828, Patrick ,討論後, 由收檔子程序, 進行發mail, 以免收到二次信件
                    //UtlMail.SendEmail(mailFrom, mailFromTo, strSubject, isExistMessage, mailHost);

                    # endregion
                }

            }
            catch (Exception ex)
            {

                isContinue = false;

                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, " 事務回滾, 錯誤訊息: " + ex.ToString());

            }
        }

        /// <summary>
        /// 沒有DI或CSV檔, 也要寫成立一個案號
        /// </summary>
        /// <param name="fileGroups"></param>
        /// <returns>CaseNo</returns>
        private string CreateErrorCase(FileGroups fileGroups,string errorMessage)
        {
            string strMainPK = Guid.NewGuid().ToString();
            // 20190911, 因為要記錄CaseID, 才能回到GssDoc_Detail中
            strMainPK = fileGroups.CaseID.ToString();

            string strMaxDocNo = GetMaxDocNo1();

            // 取得流水號
            int intMaxDocNo = 0;

            if (strMaxDocNo == "")
            {
                intMaxDocNo = 0;
            }
            else
            {
                intMaxDocNo = Convert.ToInt32(strMaxDocNo.Substring(8, 5));
            }

            // 案件編號 規則：E+民國106+月份10+日16+00001(五碼流水編號)
            string strDocNo = GetDocNo(intMaxDocNo);


            var bRet = importBiz.insertCaseCustErrorCaseNo(strDocNo, strMainPK, fileGroups, errorMessage, m_fileLog);

            if (bRet)
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------新增" + strDocNo + "到 CaseCustMaster / CaseCustDetails 錯誤訊息 ----------------");
            else
            {
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------新增" + strDocNo + "到 CaseCustMaster / CaseCustDetails 失敗----------------");                
            }

            return strDocNo;
        }


        /// <summary>
        /// 沒有DI或CSV檔, 也要寫成立一個案號
        /// </summary>
        /// <param name="fileGroups"></param>
        /// <returns>CaseNo</returns>
        private string CreateErrorCase2(FileGroups fileGroups, string errorMessage, string LetterNo, string LetterDeptName)
        {
            string strMainPK = Guid.NewGuid().ToString();
            // 20190911, 因為要記錄CaseID, 才能回到GssDoc_Detail中
            strMainPK = fileGroups.CaseID.ToString();

            string strMaxDocNo = GetMaxDocNo1();

            // 取得流水號
            int intMaxDocNo = 0;

            if (strMaxDocNo == "")
            {
                intMaxDocNo = 0;
            }
            else
            {
                intMaxDocNo = Convert.ToInt32(strMaxDocNo.Substring(8, 5));
            }

            // 案件編號 規則：E+民國106+月份10+日16+00001(五碼流水編號)
            string strDocNo = GetDocNo(intMaxDocNo);


            var bRet = importBiz.insertCaseCustErrorCaseNo2(strDocNo, strMainPK, fileGroups, errorMessage, m_fileLog, LetterNo, LetterDeptName);

            if (bRet)
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------新增" + strDocNo + "到 CaseCustMaster / CaseCustDetails 錯誤訊息 ----------------");
            else
            {
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------新增" + strDocNo + "到 CaseCustMaster / CaseCustDetails 失敗----------------");
            }

            return strDocNo;
        }

        /// <summary>
        /// 產生 案號_Error.zip, CaseCustMaster.ImportMessage 加上訊息
        /// </summary>
        /// <param name="fg"></param>
        /// <param name="message"></param>
        public static string formatErrorZip(string filePath, FileGroups fg, string message)
        {
            // filePath + @"\" + fileGroups.TXT

            // System.IO.Compression.ZipFile.ExtractToDirectory(fi.FullName, reciveloaclFilePath + "\\");

            string zipFileName = reciveloaclFilePath + "\\" + fg.FileName25 + "_Error.zip";
            string zipFolder = reciveloaclFilePath + "\\" + fg.FileName25;

            try
            {
                System.IO.Compression.ZipFile.CreateFromDirectory(zipFolder, zipFileName);
                
            }
            catch (Exception ex)
            {                
                  m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, " 壓縮錯誤檔ZIP發生問題,  錯誤訊息: " + ex.ToString());
            }
            return zipFileName.Replace(reciveloaclFilePath + "\\", "");
        }


        public static string formatErrorZip2(string filePath, FileGroups fg, string message)
        {
            // filePath + @"\" + fileGroups.TXT

            // System.IO.Compression.ZipFile.ExtractToDirectory(fi.FullName, reciveloaclFilePath + "\\");

            string[] bP = fg.DI.Split('\\');
            fg.FileName25 = bP[0] + "\\" + bP[1];


            string zipFileName = reciveloaclFilePath + "\\" + fg.FileName25 + "_Error.zip";
            string zipFolder = reciveloaclFilePath + "\\" + fg.FileName25;

            try
            {
                System.IO.Compression.ZipFile.CreateFromDirectory(zipFolder, zipFileName);

            }
            catch (Exception ex)
            {
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, " 壓縮錯誤檔ZIP發生問題,  錯誤訊息: " + ex.ToString());
            }
            return zipFileName.Replace(reciveloaclFilePath + "\\", "");
        }

        /// <summary>
        /// 取得XML的Tag值
        /// </summary>
        /// <param name="xmlDoc"></param>
        /// <param name="tag"></param>
        /// <returns></returns>
        public static string GetXMLNode(XmlDocument xmlDoc, string tag)
        {
            string value = string.Empty;

            try
            {
                XmlNode node = xmlDoc.SelectSingleNode(tag);

                if (node != null)
                {
                    value = node.InnerText;
                }

            }
            catch (Exception ex)
            {
                value = string.Empty;
            }

            return value;
        }

        /// <summary>
        /// 取得承辦人Tag值 (由於有可能會有多個同樣的Tag，所以獨立處理)
        /// 處理原則：
        /// 1.如有值內容包含"承辦人："，就取此關鍵字後面所有內容
        /// 2.如無#1的值，就取下一個順序中的所有值，不論內容為何
        /// 3.如無任何符合的Tag存在，則回傳空白
        /// </summary>
        /// <param name="xmlDoc"></param>
        /// <param name="tag"></param>
        /// <returns></returns>
        public static string GetXMLNodeInCharge(XmlDocument xmlDoc, string tag)
        {
            XmlNodeList nodeList = null;
            string value = string.Empty;
            string firstValue = string.Empty;

            try
            {
                // 記錄下第一個符合Tag的值
                firstValue = GetXMLNode(xmlDoc, tag);

                if (!string.IsNullOrEmpty(firstValue))
                {
                    nodeList = xmlDoc.SelectNodes(tag);

                    foreach (XmlNode note in nodeList)
                    {
                        string nodeValue = note.InnerText;
                        int pos = nodeValue.IndexOf("承辦人：");

                        if (pos > -1)
                        {
                            value = nodeValue.Substring(pos + "承辦人：".Length);

                            break;
                        }
                    }

                    // 若沒有符合"承辦人："的Tag，就取第一個Tag值
                    if (string.IsNullOrEmpty(value))
                    {
                        value = firstValue.Replace(":", "").Replace("：", "");
                    }
                }
            }
            catch (Exception ex)
            {
                value = string.Empty;
            }

            return value;
        }


        /// <summary>  
        /// 删除指定目录下所有内容 
        /// </summary>  
        /// <param name="dirPath"></param>  
        public static void DeleteFolder(string dirPath)
        {
            if (Directory.Exists(dirPath))
            {
                foreach (string content in Directory.GetFileSystemEntries(dirPath))
                {
                    if (Directory.Exists(content))
                    {
                        Directory.Delete(content, true);
                    }
                    else if (System.IO.File.Exists(content))
                    {
                        System.IO.File.Delete(content);
                    }
                }
            }
        }

        /// <summary>
        /// 用Byte截長度
        /// </summary>
        /// <param name="a_SrcStr"></param>
        /// <param name="a_Cnt"></param>
        /// <returns></returns>
        public static string Big5SubStr(string a_SrcStr, int a_Cnt)
        {
            Encoding l_Encoding = Encoding.GetEncoding("big5");
            byte[] l_byte = l_Encoding.GetBytes(a_SrcStr);
            if (a_Cnt <= 0)
                return "";
            //例若長度10 
            //若a_StartIndex傳入9 -> ok, 10 ->不行 
            if (intIndex + 1 > l_byte.Length)
                return "";
            else
            {
                //若a_StartIndex傳入9 , a_Cnt 傳入2 -> 不行 -> 改成 9,1 
                if (intIndex + a_Cnt > l_byte.Length)
                    a_Cnt = l_byte.Length - intIndex;
            }

            string strResult = l_Encoding.GetString(l_byte, intIndex, a_Cnt);
            intIndex = intIndex + a_Cnt;

            return strResult;
        }

        /// <summary>
        /// 取得最大案件編號
        /// </summary>
        /// <returns></returns>
        public string GetMaxDocNo(IDbTransaction dbTransaction)
        {

            string sqlSelect = @"SELECT	
                                   ISNULL(MAX(DocNo),'')
                                FROM 
                                    CaseCustMaster
                                WHERE
                                    Substring(DocNo,2,7) = @DocNo";

            // 清空容器
            base.Parameter.Clear();

            // 添加參數
            base.Parameter.Add(new CommandParameter("@DocNo", pDocNoPrefix));

            return base.ExecuteScalar(sqlSelect, dbTransaction).ToString();

        }

        /// <summary>
        /// 取得最大案件編號
        /// </summary>
        /// <returns></returns>
        public string GetMaxDocNo1()
        {

            string sqlSelect = @"SELECT	
                                   ISNULL(MAX(DocNo),'')
                                FROM 
                                    CaseCustMaster
                                WHERE
                                    Substring(DocNo,2,7) = @DocNo";

            // 清空容器
            base.Parameter.Clear();

            // 添加參數
            base.Parameter.Add(new CommandParameter("@DocNo", pDocNoPrefix));

            return base.ExecuteScalar(sqlSelect).ToString();

        }

        /// <summary>
        /// 獲取案件編號
        /// 規則：E+民國106+月份10+日16+00001(五碼流水編號)
        /// </summary>
        /// <param name="strMaxDocNo"></param>
        /// <returns></returns>
        private string GetDocNo(int strMaxDocNo)
        {
            string strReturn = "";

            strReturn = "E" + pDocNoPrefix + String.Format("{0:D5}", strMaxDocNo + 1);

            return strReturn;
        }

        /// <summary>
        /// 轉換日期格式(西元年轉換成民國年) YYYMMDD
        /// </summary>
        /// <param name="dateDb">日期</param>
        /// <returns>轉換後的日期</returns>
        /// <remarks>判斷年份是否為1911年以前日期,若為1911以前,頁面預示為"0"年份</remarks>
        public static string ConvertYYYMMDD()
        {
            string dateDb = DateTime.Today.ToString();

            // 轉換後的日期
            string dateValue = "";

            if (dateDb != "" && dateDb.Length > 8)
            {
                DateTime dt = Convert.ToDateTime(dateDb);

                // 判斷年份是否為1911年以前日期,若為1911以前,頁面預示為"0"年份 
                if (Convert.ToInt32(dt.Year) <= 1911)
                {
                    dateValue = string.Format("{0}{1}{2}", "0", dt.Month.ToString("00"), dt.Day.ToString("00"));
                }
                else
                {
                    dateValue = string.Format("{0}{1}{2}", dt.AddYears(-1911).Year.ToString("000"), dt.Month.ToString("00"), dt.Day.ToString("00"));
                }
            }

            return dateValue;
        }

        /// <summary>
        /// 當前日期轉換日期格式(西元年轉換成民國年) YYY/MM/DD
        /// </summary>
        /// <returns></returns>
        public static string ConvertToYYYMMDD()
        {
            string dateDb = DateTime.Today.ToString();

            // 轉換後的日期
            string dateValue = "";

            if (dateDb != "" && dateDb.Length > 8)
            {
                DateTime dt = Convert.ToDateTime(dateDb);

                // 判斷年份是否為1911年以前日期,若為1911以前,頁面預示為"0"年份 
                if (Convert.ToInt32(dt.Year) <= 1911)
                {
                    dateValue = string.Format("{0}/{1}/{2}", "0", dt.Month.ToString("00"), dt.Day.ToString("00"));
                }
                else
                {
                    dateValue = string.Format("{0}/{1}/{2}", dt.AddYears(-1911).Year.ToString("000"), dt.Month.ToString("00"), dt.Day.ToString("00"));
                }
            }

            return dateValue;
        }

        /// <summary>
        ///  20190822 修改取得收件人(CodeMemo) 宏祥
        ///  取得收mail者 
        /// </summary>
        /// <returns></returns>
        public string[] getMailFromTo(string strCodeNo)
        {
            //string sqlSelect = @" SELECT CodeDesc FROM PARMCode WHERE CodeType = 'CSFSCode' AND CodeNo = @CodeNo";
            string sqlSelect = @" SELECT CodeMemo FROM PARMCode WHERE CodeType = 'CSFSCode' AND CodeNo = @CodeNo";

            base.Parameter.Clear();

            base.Parameter.Add(new CommandParameter("@CodeNo", strCodeNo));

            return base.ExecuteScalar(sqlSelect).ToString().Split(',');

        }

        /// <summary>
        /// 取得要上傳的檔案
        /// </summary>
        /// <returns></returns>
        public IList<CaseCustMaster> GetFiles()
        {
            string sql = @"SELECT ROpenFileName,RFileTransactionFileName ,NewID 
,DocNo ,Version
FROM CaseCustMaster where UploadStatus = 'Y' AND FileStatus = 'N'";

            base.Parameter.Clear();

            return SearchList<CaseCustMaster>(sql);
        }

        /// <summary>
        /// 根據主鍵更新
        /// </summary>
        /// <param name="pkNewID"></param>
        public void UpdateCaseCustQuery(Guid pkNewID)
        {
            string sqlUpdate = @"UPDATE CaseCustMaster SET FileStatus = 'Y' WHERE NewID=@NewID";

            base.Parameter.Clear();

            base.Parameter.Add(new CommandParameter("@NewID", pkNewID));

            base.ExecuteNonQuery(sqlUpdate);
        }

        /// <summary>
        ///  根據文件的前25碼判斷是否已經將該檔案匯入DB 返回為true 則已經存在，否則不存在
        /// </summary>
        /// <param name="strFilleName25"></param>
        /// <returns></returns>
        public string IsExistFile(string strFilleName25)
        {
            string strSql = @"SELECT TOP(1) DocNo FROM CaseCustQuery  WHERE SUBSTRING(QFileName,1,25)=@QFileName25";

            base.Parameter.Clear();

            base.Parameter.Add(new CommandParameter("@QFileName25", strFilleName25));

            object obj = base.ExecuteScalar(strSql);

            if (obj != null)
            {
                return obj.ToString();

            }
            else
            {
                return "";
            }
        }

        /// <summary>
        /// 壓縮多個文件
        /// </summary>
        /// <param name="sourceFilePath"></param>
        /// <param name="destinationZipFilePath"></param>
        public void CreateZip(string sourceFilePath, string destinationZipFilePath, List<string> files, string password)
        {
            if (sourceFilePath[sourceFilePath.Length - 1] != System.IO.Path.DirectorySeparatorChar)
                sourceFilePath += System.IO.Path.DirectorySeparatorChar;

            ZipOutputStream zipStream = new ZipOutputStream(System.IO.File.Create(destinationZipFilePath));

            zipStream.SetLevel(6);  // 压缩级别 0-9
            zipStream.Password = password;

            foreach (string file in files)
            {
                FileStream stream = System.IO.File.OpenRead(file);

                byte[] buffer = new byte[stream.Length];

                stream.Read(buffer, 0, buffer.Length);

                string tempFile = file.Substring(sourceFilePath.LastIndexOf("\\") + 1);

                ZipEntry entry = new ZipEntry(Path.GetFileName(file));

                entry.DateTime = DateTime.Now;
                entry.Size = stream.Length;
                stream.Close();

                zipStream.PutNextEntry(entry);

                zipStream.Write(buffer, 0, buffer.Length);

            }

            zipStream.Finish();
            zipStream.Close();

            GC.Collect();
            GC.Collect(1);
        }

        public class CaseCustQuery
        {
            public string ROpenFileName { get; set; }

            public string RFileTransactionFileName { get; set; }

            public string DocNo { get; set; }

            public string Version { get; set; }

            public Guid NewID { get; set; }


        }

        /// <summary>
        /// 存放從FTP上取得的檔案
        /// </summary>
        public class FileGroups
        {
            public string TXT { get; set; }

            public string PDF { get; set; }

            public string DI { get; set; }

            public string FileName25 { get; set; }

            public bool IsExist { get; set; }

            public Guid CaseID { get; set; }
        }

        /// <summary>
        /// 文件結構
        /// </summary>
        public class Files
        {
            public string Name { get; set; }

            public string Extension { get; set; }

        }
    }
}
