using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CTBC.CSFS.BussinessLogic;
using CTBC.FrameWork.Util;
using System.Configuration;
using System.Collections;
using System.IO;
using System.IO.Compression;
using Newtonsoft.Json;
using System.Xml;
using CTBC.CSFS.Models;
using System.Globalization;
using System.Data;

namespace CTBC.WinExe.ImportGSSDoc
{
    class Program
    {
        private static ImportEDocBiz _ImportEDocBiz;
        private static CaseHistoryBIZ _CaseHistoryBiz;
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

            loaclFilePath = ConfigurationManager.AppSettings["localFilePath"];
            m_fileLog = new FileLog(ConfigurationManager.AppSettings["filelog"], AppDomain.CurrentDomain.FriendlyName.Replace(".vshost", "").Replace(".exe", ""));
            string batchfilename = ConfigurationManager.AppSettings["batchfilename"].ToString();
            string metafilename = ConfigurationManager.AppSettings["metafilename"].ToString();
            int delZipDay = int.Parse(ConfigurationManager.AppSettings["deleteZipDay"].ToString()); // 解ZIP後, 是否要把ZIP檔刪除
            int delDirDay = int.Parse(ConfigurationManager.AppSettings["deleteDirDay"].ToString()); // 解ZIP後, 是否要把目錄刪除

            string msg = string.Format("開始處理======================");
            m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, msg);

            bool isFTP = false;
            if (!string.IsNullOrEmpty(ftp))
            {
                isFTP = bool.Parse(ftp);
            }

            if( ! Directory.Exists(loaclFilePath))
            {
                Directory.CreateDirectory(loaclFilePath);
            }

            if (delZipDay > 0) // 幾天前的要刪除
            {
                DirectoryInfo di = new DirectoryInfo(loaclFilePath);
                foreach(var f in di.GetFiles("*.zip"))
                {
                    if (f.CreationTime < DateTime.Now.AddDays(delZipDay * -1))
                        f.Delete();
                }
                msg = string.Format("\t刪除前{0}天的zip檔", delZipDay);
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, msg);
            }

            if (delDirDay > 0) //幾天前的要刪除
            {
                DirectoryInfo di = new DirectoryInfo(loaclFilePath);
                foreach(var d in di.GetDirectories())
                {
                    if( d.CreationTime < DateTime.Now.AddDays(delDirDay * -1))
                    {
                        if (Directory.Exists(d.FullName))
                        {
                            System.IO.Directory.Delete(d.FullName, true);
                            //Directory.Delete(d.FullName);
                        }
                    }
                }
                msg = string.Format("\t刪除前{0}天的目錄檔", delZipDay);
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, msg);
            }


            //string[] fileNames;
            if (isFTP)
            {
                msg = string.Format("\t開啟Ftp連線");
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, msg);
                ftpClient = new FtpClient(ftpserver, username, password, port);
                // 取得 轉新外來文 的根目錄, 通常都是 yyyyMMddhhmmss
                //ArrayList allDocBatchDir = ftpClient.GetDirList(ftpdir);

                ArrayList allZips = ftpClient.GetFileList(ftpdir);


                msg = string.Format("\t共計有{0}個zip檔", allZips.Count.ToString());
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, msg);

                if (allZips.Count == 0)
                {
                    Console.WriteLine("無Zip檔");
                    msg = string.Format("\t無Zip檔");
                    m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, msg);
                    msg = string.Format("結束處理======================\r\n\r\n\r\n");
                    m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, msg);
                    return;
                }

                foreach (string zipfile in allZips)
                {
                   ftpClient.GetFiles(ftpdir + "/" + zipfile, loaclFilePath + "\\" + zipfile);
                }
                msg = string.Format("\t\t下載zip檔 : {0} ", string.Join(",", allZips.ToArray()));
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, msg);

                #region 刪除Zip 檔

                foreach(var zipfile in allZips)
                {
                    ftpClient.DeleteFile(ftpdir + "/" + zipfile);
                }
                msg = string.Format("\t\t刪除 mftp上的zip檔 : {0} ", string.Join(",", allZips.ToArray()));
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, msg);

                #endregion


                #region 後來改成用ZIP 包, 所以不用管檔案結構
                //foreach (string docBatch in allDocBatchDir)
                //{
                //    // 1. 要先把之前已經下載的公文, 跳過
                //    //if( docHasRead(docBatch))
                //    //{
                //    //    continue;
                //    //}

                //    // 2. 讀取底下的json檔


                //    string batchDir = loaclFilePath + "\\" + docBatch + "\\";
                //    if( ! Directory.Exists(batchDir))
                //    {
                //        Directory.CreateDirectory(batchDir);
                //    }

                //    ftpClient.GetFiles("GSS2CSFS/" + docBatch + "/"  + batchfilename, batchDir + batchfilename);

                //    string strBatchmetadata = null;
                //    using (StreamReader sr = new StreamReader(batchDir + batchfilename, Encoding.UTF8))
                //    {
                //        strBatchmetadata = sr.ReadToEnd();
                //    }

                //    batchmetadata bmdata = JsonConvert.DeserializeObject<batchmetadata>(strBatchmetadata);

                //    foreach(var _CnoNoInfo in bmdata.CnoNoInfo)  // 讀出BatchMetaData中, 共有幾個檔
                //    {
                //        string metaDir = batchDir + _CnoNoInfo + "\\";
                //        if (!Directory.Exists(metaDir))
                //        {
                //            Directory.CreateDirectory(metaDir);
                //        }

                //        ftpClient.GetFiles("GSS2CSFS/" + docBatch + "/" + _CnoNoInfo + "/" + metafilename, metaDir + metafilename);
                //        string strmetadata = null;
                //        using (StreamReader sr = new StreamReader(metaDir + metafilename, Encoding.UTF8))
                //        {
                //            strmetadata = sr.ReadToEnd();
                //        }
                //        metadata mdata = JsonConvert.DeserializeObject<metadata>(strmetadata);
                //        foreach(var f in mdata.Files)
                //        {
                //            ftpClient.GetFiles("GSS2CSFS/" + docBatch + "/" + _CnoNoInfo + "/" + f.FileName, metaDir + "\\" + f.FileName);
                //        }


                //    }



                //}
                #endregion
                //fileNames = getFileList(); // 取得FTP上的檔案
            }


            DirectoryInfo allDi = new DirectoryInfo(loaclFilePath + "\\");

            // 過濾已處理過的BatchNo

            FileInfo[] allzips = BatchHasRead(allDi.GetFiles("*.zip"));

            if( allzips.Count()==0)
            {
                Console.WriteLine("無新的批號，勿須要處理");
                msg = string.Format("\t無新的批號，勿須要處理");
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, msg);
                msg = string.Format("結束處理======================\r\n\r\n\r\n");
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, msg);
                return;
            }

            msg = string.Format("\t\t過濾已Parse批次 :{0} ", string.Join(",", allzips.Select(x=>x.Name)));
            m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, msg);

            foreach (FileInfo fi in allzips)
            {
                string batchNo = fi.Name.Replace(".zip", "");
                try
                {       
                    //清空, 若原有目錄有資料
                    if (Directory.Exists(loaclFilePath + "\\" + batchNo))
                        System.IO.Directory.Delete(loaclFilePath + "\\" + batchNo, true);
                    System.IO.Directory.CreateDirectory(loaclFilePath + "\\" + batchNo);

                    ZipFile.ExtractToDirectory(fi.FullName, loaclFilePath + "\\");
                    //allBatchDirs.Add(fi.FullName.Replace(".zip", "") + "\\");

                    msg = string.Format("\t\t解開{0}批次, 到 {1}  ", batchNo, loaclFilePath + "\\" +  fi.Name.Replace(".zip", "") + "\\");
                    m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, msg);

                }
                catch( Exception ex)
                {
                    Console.WriteLine("解壓縮失敗!!, 檔名" + fi.FullName);
                    msg = string.Format("\t\t解壓縮失敗!!, 檔名 ", batchNo);
                    m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, msg);
                }

                if(delZipDay==0) // 當場刪除
                {
                    fi.Delete();
                    msg = string.Format("\t\t刪除{0} ", fi.Name);
                    m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, msg);
                }

                if( delDirDay==0) //當場刪除
                {
                    if (Directory.Exists(loaclFilePath + "\\" + batchNo))
                    {
                        System.IO.Directory.Delete(loaclFilePath + "\\" + batchNo, true);
                        //Directory.Delete(loaclFilePath + "\\" + batchNo);
                        msg = string.Format("\t\t刪除目錄{0} ", batchNo);
                        m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, msg);
                    }
                }

            }


            {

                List<GssDoc> gssList = new List<GssDoc>();
                List<GssDoc_Detail> gssDetails = new List<GssDoc_Detail>();


                #region 把目錄中每一個目錄, 全部讀進gssList, gssDetails

                msg = string.Format("\t\t開始Parse json檔 ");
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, msg);
                foreach (var di in allDi.GetDirectories())
                {
                    try
                    {
                        // 20190911, 若在allzips 中的批號, 才需要繼續Parsing 檔.. . 因此, 就不需要在搬到備份的目錄了...
                        if (!allzips.Select(x => x.Name.Replace(".zip", "")).Contains(di.Name))
                            continue;

                        msg = string.Format("\t\t\t開始 Parse 批號{0} ..... ", di.Name);
                        m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, msg);

                        GssDoc gss = new GssDoc() { DocType = 1, BatchNo = di.Name, CreatedDate = DateTime.Now, TransferType = "1" }; // DocType = 1, 表示轉新外來文,     2 = 轉線上投單
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
                                        if (fi.Extension.ToUpper() == ".DI")
                                        {
                                            ParseXML(aaa1); // 會把di檔, Parsing 到fDocNode 這個變數
                                            List<string> ErrorMessage = CheckRequireField(fDocNode);
                                            if (ErrorMessage.Count() > 0)
                                            {
                                                isSuccess = false;
                                                gdd.ParserStatus = 2;
                                                gdd.ParserMessage = fi.Name + " 發生錯誤: " + string.Join(",", ErrorMessage);
                                                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "\t\t\t\t" + gdd.ParserMessage);
                                            }
                                        }

                                        //ftpClient.GetFiles("轉新外來文/" + docBatch + "/" + _CnoNoInfo + "/" + f.FileName, metaDir + "\\" + f.FileName);
                                    }
                                    if (isSuccess)
                                    {
                                        gdd.FileNames = string.Join(",", mdata.Files.Select(x => x.FileName));
                                        gdd.ParserStatus = 1;
                                        gdd.ParserMessage = "成功";
                                        msg = string.Format("\t\t\t\tParse 文號{0} 成功, CaseID={1} ", _CnoNoInfo, caseId);
                                        m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, msg);
                                    }
                                    gssDetails.Add(gdd);
                                    #endregion
                                    countDir++;
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
                        GssDoc gss = new GssDoc() { DocType = 1, BatchNo = di.Name, CreatedDate = DateTime.Now, TransferType = "1" }; // DocType = 1, 表示轉新外來文,     2 = 轉線上投單
                        gss.ParserStatus = 2;
                        gss.ParserMessage = msg;
                        gssList.Add(gss);
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


                if (gssList.Where(x=>x.ParserStatus==1).Count() > 0)
                {
                    // 開始儲存進CaseMaster, 發給CaseId 併儲進gssDetail.CaseId
                    int retStatus = SaveCaseMaster(gssList, gssDetails);
                    if (retStatus < 0)
                    {
                        Console.WriteLine("Error in SaveCaseMaster");
                        msg = string.Format("\t\t儲存CaseMaster錯誤 ");
                        m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, msg);
                    }
                    else
                    {
                        msg = string.Format("\t\t儲存CaseMaster,CaseHistory, CaseEdocFile成功 ");
                        m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, msg);
                    }
                }
                else
                {
                    msg = string.Format("\t\t目前沒有批號, 未儲存CaseMaster!! ");
                    m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, msg);
                }






                #region 發出email通知
                PARMCodeBIZ pbiz = new PARMCodeBIZ();

                var gssDocMail = pbiz.GetParmCodeByCodeType("GssDocNoticeMail").FirstOrDefault();
                string[] mailTo = gssDocMail.CodeMemo.Split(',');
                
                if (gssDocMail != null)
                {
                    foreach(var gssBatchNo in gssList.Where(x=>x.ParserStatus==1))
                    {
                        var gssDocs = gssDetails.Where(x => x.BatchNo == gssBatchNo.BatchNo).ToList();
                        var errDocs = gssDocs.Where(x => x.ParserStatus == 2).ToList(); // 如果是2, 表示, 有這筆有錯

                        try
                        {
                            // 20190912, 討論後, 若這批共有3個文號, 而且3個都錯,就定義為全錯
                            if (gssDocs.Count() == errDocs.Count() && errDocs.Count() > 0)
                            {
                                noticeMail_Fail_All(mailTo, "", gssBatchNo.BatchNo, DateTime.Now, gssDocs.Count().ToString(), string.Join(",", gssDocs.Select(x => x.DocNo)), errDocs);
                            }
                            else
                            {
                                if (errDocs.Count() > 0) // 表示其中有錯...
                                {
                                    noticeMail_Fail(mailTo, gssBatchNo.CompanyID, gssBatchNo.BatchNo, (DateTime)gssBatchNo.BatchDate, gssDocs.Count().ToString(), string.Join(",", gssDocs.Select(x => x.DocNo)), errDocs);
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
                    foreach (var gssBatchNo in gssList.Where(x => x.ParserStatus==2))
                    {
                        var gssDocs = gssDetails.Where(x => x.BatchNo == gssBatchNo.BatchNo).ToList();                        
                        try
                        {
                            if (gssDocs.Count() == 0) // 表示Batch層次, 就錯了.. 整批錯誤, 所以沒有gssDetails......
                            {
                                noticeMail_Fail_All2(mailTo, "", gssBatchNo.BatchNo, DateTime.Now, gssDocs.Count().ToString(), string.Join(",", gssDocs.Select(x => x.DocNo)), "整批錯誤!!");
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

                msg = string.Format("結束處理======================\r\n\r\n\r\n");
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, msg);

            }

        }

        private static void noticeMail_Success(string[] mailFromTo, string companyid,string batchno, DateTime batchdate, string totNum, string docNos)
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
                   body += "轉換類型： 轉新外來文\r\n";
                   body += "介接文號總數： {3}\r\n";
                   body += "公文文號清單： {4}\r\n";
                   body += "批次執行成功!\r\n\r\n\r\n";
                   body += "本信件由系統直接發送!請勿直接回覆，謝謝!\r\n";
            //20190822 宏祥 update end
            body = string.Format(body, companyid, batchno, batchdate.ToLongDateString(), totNum, docNos);
            string host = ConfigurationManager.AppSettings["mailHost"];
            UtlMail.SendEmail(mailFrom, mailFromTo, subject, body, host);
        }

        private static void noticeMail_Fail(string[] mailFromTo, string companyid, string batchno, DateTime batchdate, string totNum, string docNos, List<GssDoc_Detail> faildocs)
        {
            string mailFrom = ConfigurationManager.AppSettings["mailFrom"];
            //string[] mailFromTo = ConfigurationManager.AppSettings["mailTo"].ToString().Split(',');
            // 批號+件檔成功XX筆/失敗xx筆-外來文A
            int iCount = faildocs.Count();
            //string[] fdocs = faildocs.Split(',');
            int totalSuccess = int.Parse(totNum) - iCount;
            string subject = string.Format("{0} 建檔成功 {1} 筆/失敗 {2} 筆-外來文A", batchno, totalSuccess.ToString(), iCount.ToString());

            // 20191108, 要把Parse錯誤的原因, 寫在mail中
            //errDocs.Select(x => x.DocNo)

            StringBuilder sb = new StringBuilder();
            foreach (var e in faildocs)
            {
                sb.Append(string.Format("文號{0} / 錯誤原因{1}\r\n", e.DocNo, e.ParserMessage));
            }


            string body = "公司代碼： {0}\r\n";
                   body += "批次批號： {1}\r\n";
                   body += "批次執行日期時間： {2}\r\n";
                   body += "轉換類型： 轉新外來文\r\n";
                   body += "介接文號總數： {3}\r\n";
                   body += "公文文號清單： {4}\r\n";
                   body += "批次執行失敗!\r\n\r\n";
                   body += "失敗公文文號清單及訊息：\r\n";
                   body += "{5}\r\n\r\n\r\n";
                   body += "本信件由系統直接發送!請勿直接回覆，謝謝!\r\n";
            //20190822 宏祥 update end
            body = string.Format(body, companyid, batchno, batchdate.ToLongDateString(), totNum, docNos, sb.ToString());
            string host = ConfigurationManager.AppSettings["mailHost"];
            UtlMail.SendEmail(mailFrom, mailFromTo, subject, body, host);
        }

        private static void noticeMail_Fail_All(string[] mailFromTo, string companyid, string batchno, DateTime batchdate, string totNum, string docNos, List<GssDoc_Detail> faildocs)
        {
            string mailFrom = ConfigurationManager.AppSettings["mailFrom"];
            //string[] mailFromTo = ConfigurationManager.AppSettings["mailTo"].ToString().Split(',');
            // 批號+建檔失敗-外來文A
            //string[] fdocs = faildocs.Split(',');
            string fdocsLen = faildocs.Count().ToString();
            string subject = string.Format("{0} 建檔失敗-外來文A", batchno, totNum, fdocsLen);

            // 20191108, 要把Parse錯誤的原因, 寫在mail中
            //errDocs.Select(x => x.DocNo)

            StringBuilder sb = new StringBuilder();
            foreach(var e in faildocs)
            {
                sb.Append(string.Format("文號{0} / 錯誤原因{1}\r\n", e.DocNo, e.ParserMessage));
            }

            string body = "公司代碼： {0}\r\n";
                   body += "批次批號： {1}\r\n";
                   body += "批次執行日期時間： {2}\r\n";
                   body += "轉換類型： 轉新外來文\r\n";
                   body += "介接文號總： {3}\r\n";
                   body += "公文文號清單： {4}\r\n";
                   body += "批次執行失敗!\r\n\r\n";
                   body += "失敗公文文號清單及訊息： \r\n";
                   body += "{5}\r\n\r\n\r\n";
                   body += "本信件由系統直接發送!請勿直接回覆，謝謝!\r\n";
            //20190822 宏祥 update end
            body = string.Format(body, companyid, batchno, batchdate.ToLongDateString(), totNum, docNos, sb.ToString());
            string host = ConfigurationManager.AppSettings["mailHost"];
            UtlMail.SendEmail(mailFrom, mailFromTo, subject, body, host);
        }


        private static void noticeMail_Fail_All2(string[] mailFromTo, string companyid, string batchno, DateTime batchdate, string totNum, string docNos, string faildocs)
        {
            string mailFrom = ConfigurationManager.AppSettings["mailFrom"];
            //string[] mailFromTo = ConfigurationManager.AppSettings["mailTo"].ToString().Split(',');
            // 批號+建檔失敗-外來文A
            //string[] fdocs = faildocs.Split(',');
            //string fdocsLen = faildocs.Count().ToString();
            string subject = string.Format("{0} 建檔失敗-外來文A", batchno, totNum);

            // 20191108, 要把Parse錯誤的原因, 寫在mail中
            //errDocs.Select(x => x.DocNo)

            //StringBuilder sb = new StringBuilder();
            //foreach (var e in faildocs)
            //{
            //    sb.Append(string.Format("文號{0} / 錯誤原因{1}\r\n", e.DocNo, e.ParserMessage));
            //}

            string body = "公司代碼： {0}\r\n";
            body += "批次批號： {1}\r\n";
            body += "批次執行日期時間： {2}\r\n";
            body += "轉換類型： 轉新外來文\r\n";
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


        private static int SaveCaseMaster(List<GssDoc> gssList, List<GssDoc_Detail> gssDetails)
        {
            int retStatus = 0;
            _ImportEDocBiz = new ImportEDocBiz();
            _CaseHistoryBiz = new CaseHistoryBIZ();
            PARMWorkingDayBIZ workDate = new PARMWorkingDayBIZ();

            foreach (var gssBatchNo in gssList.Where(x=>x.ParserStatus==1))
            {
                var gssDocs = gssDetails.Where(x => x.BatchNo == gssBatchNo.BatchNo).ToList();
               
                try
                {
                    
                    foreach (var gssDoc in gssDocs.Where(x=>x.ParserStatus==1))
                    {
                        try
                        {
                            Dictionary<string, FileStream> fs = new Dictionary<string, FileStream>();
                            Guid caseId = (Guid)gssDoc.CaseId;
                            #region 讀取檔案, 至fs


                            string[] fileArray = gssDoc.FileNames.Split(',');
                            string mDI_File = "";
                            foreach (string filename in fileArray)
                            {
                                string filePath = loaclFilePath.TrimEnd('\\') + "\\" +  gssBatchNo.BatchNo + "\\" + gssDoc.DocNo + "\\" + filename;

                                if (System.IO.File.Exists(filePath))
                                {
                                    FileInfo fi = new FileInfo(filePath);
                                    fs.Add(filename, System.IO.File.OpenRead(filePath));
                                }
                                if (filename.ToUpper().EndsWith("DI"))
                                    mDI_File = filePath;
                            }
                            #endregion

                            ParseXML(mDI_File); // 會把di檔, Parsing 到fDocNode 這個變數

                            CaseNoTableBIZ noBiz = new CaseNoTableBIZ();
                            CaseMaster caseMaster = new CaseMaster();
                            caseMaster.CaseId = caseId;

                            caseMaster.GovUnit = fDocNode.GovTitle;
                            caseMaster.GovDate = UtlString.FormatDateTwStringToAd(fDocNode.SendDate.Replace("中華民國", "").Replace("年", "/").Replace("月", "/").Replace("日", "/").Trim());
                            caseMaster.Speed = fDocNode.Speed;   //速別
                            caseMaster.ReceiveKind = "紙本";
                            caseMaster.GovNo = fDocNode.SendWord + "字第" + fDocNode.SendYear + fDocNode.SendNo + fDocNode.SendNo2 + "號";
                            //重複建檔檢核功能，檢核來文機關、來文字、來文號，公文案件中已有重覆案件時不匯入
                            // 20190830, 後來決議, 不需要檢查是否有沒有匯入公文文號
                            //bool reCase = _ImportEDocBiz.ValiReCase(caseMaster.GovUnit, caseMaster.GovNo);
                            bool reCase = false;
                            if (!reCase)
                            {

                                #region 寫入CaseMaster 

                                string caseKind2 = "一般件"; // 若是E: 一般件, C:複雜件, O:其他
                                if (gssDoc.CaseKind.ToUpper().Trim() == "C")
                                    caseKind2 = "複雜件";
                                if (gssDoc.CaseKind.ToUpper().Trim() == "O")
                                    caseKind2 = "其他";
                                if (gssDoc.CaseKind.ToUpper().Trim() == "E")
                                    caseKind2 = "一般件";


                                // 20191121, //*限辦日期, 要比照網頁版的邏輯
                                //caseMaster.LimitDate = DateTime.Now.AddDays(Convert.ToInt32(new PARMCodeBIZ().GetCodeNoByCodeDesc("LIMITDATE1"))).ToString("yyyy/MM/dd");
                                string limitedate = string.Empty;
                                DataTable dt = workDate.GetWorkDate();
                                int chkdays = 0;
                                if (dt != null && dt.Rows.Count > 0)
                                {
                                    chkdays = Convert.ToInt32(new PARMCodeBIZ().GetCodeNoByCodeDesc("LIMITDATE2"));
                                    if (chkdays < dt.Rows.Count)
                                    {
                                        limitedate = dt.Rows[chkdays]["workdate"].ToString();
                                    }
                                }
                                caseMaster.LimitDate = limitedate;
                                // 20191121, //*限辦日期, ------>END

                                caseMaster.ReceiveDate = DateTime.Now.ToString("yyyy/MM/dd");
                                caseMaster.CaseKind = CaseKind.CASE_EXTERNAL; // 改成  外來文案件
                                caseMaster.CaseKind2 = caseKind2; // 若是E: 一般件, C:複雜件, O:其他
                                caseMaster.Unit = "8888";
                                caseMaster.Person = "9999";   //* 建檔人
                                caseMaster.DocNo = noBiz.GetDocNo();   //* 系統編號

                                //                                       //* 案件編號 一般文的.在指派經辦時才有caseno
                                //if (caseMaster.CaseKind == CaseKind.CASE_SEIZURE)                              //    caseMaster.CaseNo = noBiz.GetCaseNo("A");
                                //else
                                //    caseMaster.CaseNo = "";
                                var thenow = DateTime.Now.ToString("yyyy/MM/dd");
                                //案件編號 一般文的.在指派經辦時才有caseno
                                caseMaster.CaseNo = "";
                                caseMaster.Status = CaseStatus.CaseInput;   //* 狀態      
                                caseMaster.isDelete = 0;
                                caseMaster.CreatedDate = thenow;
                                caseMaster.ModifiedDate = thenow;
                                caseMaster.CreatedUser = "Sys";
                                caseMaster.ModifiedUser = "Sys";
                                //adam 20180815
                                caseMaster.Receiver = "8888";
                                caseMaster.NotSeizureAmount = 450;


                                #region 目前已無用


                                //CaseObligor caseObligor1 = new CaseObligor();
                                //caseObligor1.ObligorName = fields[4].Replace("義務人：", "").Trim();
                                ////2016/08/18 simon
                                //caseObligor1.ObligorNo = fields[6].Replace("營利事業編號：", "").Trim().Length > 0 ? fields[6].Replace("營利事業編號：", "").Trim() : fields[7].Replace("身分證統一編號：", "").Trim();

                                //caseObligor1.CaseId = caseId;
                                //caseObligor1.CreatedUser = "電子收文";
                                //CaseObligor caseObligor2 = new CaseObligor();
                                //caseObligor2.ObligorName = fields[5].Replace("法定代理人：", "").Trim();
                                ////2016/08/18 simon
                                //caseObligor2.ObligorNo = fields[7].Replace("身分證統一編號：", "").Trim().Length > 0 ? fields[7].Replace("身分證統一編號：", "").Trim() : fields[6].Replace("營利事業編號：", "").Trim();

                                //caseObligor2.CaseId = caseId;
                                //caseObligor2.CreatedUser = "電子收文";

                                //List<CaseObligor> caseObligorList = new List<CaseObligor>();
                                //if (caseObligor1.ObligorName != "" && caseObligor1.ObligorNo != "")
                                //    caseObligorList.Add(caseObligor1);
                                //if (caseObligor2.ObligorName != "" && caseObligor2.ObligorNo != "")
                                //    caseObligorList.Add(caseObligor2);

                                //if (eDocTXT1 != null)
                                //{
                                //    _ImportEDocBiz.InsertEDocTXT1(eDocTXT1);
                                //    _ImportEDocBiz.InsertEDocTXT1_Detail(list1);
                                //    if (!string.IsNullOrEmpty(eDocTXT1.Total))
                                //    {
                                //        caseMaster.ReceiveAmount = int.Parse(eDocTXT1.Total);
                                //    }
                                //    else
                                //    {
                                //        caseMaster.ReceiveAmount = 0;
                                //    }
                                //}
                                //else if (eDocTXT2 != null)
                                //{
                                //    _ImportEDocBiz.InsertEDocTXT2(eDocTXT2);
                                //    _ImportEDocBiz.InsertEDocTXT2_Detail(list2);
                                //}



                                //if (caseObligor1.ObligorName != "" && caseObligor1.ObligorNo != "")
                                //    _ImportEDocBiz.InsertCaseObligor(caseObligor1);
                                //if (caseObligor2.ObligorName != "" && caseObligor2.ObligorNo != "")
                                //    _ImportEDocBiz.InsertCaseObligor(caseObligor2);
                                //_ImportEDocBiz.CreateLog(caseObligorList, caseMaster, null);
                                #endregion

                                _ImportEDocBiz.InsertCaseMast(caseMaster);

                                #endregion

                                #region 插入一筆CaseHistory
                                CaseHistory ch = new CaseHistory()
                                {
                                    CaseId = caseId,
                                    FromRole = "",
                                    FromFolder = "",
                                    FromUser = "",
                                    Event = "來文建檔",
                                    EventTime = DateTime.Now,
                                    ToRole = "建檔人員",
                                    ToUser = "Sys",
                                    ToFolder = "收發-待分文",
                                    CreatedUser = "Sys",
                                    CreatedDate = DateTime.Now
                                };
                                _CaseHistoryBiz.insertCaseHistory(ch);

                                #endregion

                                #region 上傳到該案件的附件目錄CaseEdocFile
                                foreach (var item in fs)
                                {
                                    byte[] bytes = new byte[item.Value.Length];
                                    CaseEdocFile caseEdocFile = new CaseEdocFile();
                                    caseEdocFile.CaseId = caseId;
                                    caseEdocFile.Type = "收文";

                                    FileInfo fi = new FileInfo(item.Key);
                                    caseEdocFile.FileType = fi.Extension;
                                    caseEdocFile.FileName = item.Key;
                                    item.Value.Position = 0;
                                    item.Value.Read(bytes, 0, bytes.Length);

                                    if (item.Key.ToUpper() == "TXT")
                                    {
                                        bytes = Encoding.Convert(Encoding.Default, Encoding.UTF8, bytes);
                                    }

                                    caseEdocFile.FileObject = bytes;
                                    caseEdocFile.SendNo = "";
                                    _ImportEDocBiz.InsertCaseEdocFile2(caseEdocFile);
                                }
                                #endregion

                                #region 所有匯入的資訊寫入ImportEdocData[電子公文匯入]的資料表
                                //所有匯入的資訊寫入ImportEdocData[電子公文匯入]的資料表//Adam建議, 放到另一個Table 
                                ImportEdocData importEdocData = new ImportEdocData();
                                importEdocData.Timesection = DateTime.Now.ToString("mm:ss");
                                importEdocData.DocNo = caseMaster.DocNo;
                                importEdocData.CaseNo = caseMaster.CaseNo;
                                importEdocData.GovUnit = caseMaster.GovUnit;
                                importEdocData.Added = reCase ? "0" : "1";
                                importEdocData.GovDate = Convert.ToDateTime(caseMaster.GovDate);
                                importEdocData.GovNo = caseMaster.GovNo;
                                importEdocData.CreatedUser = "Sys";

                                //20210503, 若有錯誤, 則回傳錯誤訊息
                                string exMessage = string.Empty;
                                _ImportEDocBiz.ImportEdocData(importEdocData, ref exMessage);

                                #endregion

                                if (! string.IsNullOrEmpty(exMessage))
                                {
                                    m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "錯誤--------儲存ImportEDocData錯誤----------------" + exMessage.ToString());
                                }

                            }
                            else {
                                string msg = string.Format("\t\t\t案件{0} , 已曾經匯入過, 跳過此文號", caseMaster.GovUnit+caseMaster.GovNo);
                                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, msg);
                            }


                        }
                        catch (Exception ex)
                        {
                            //throw ex; 
                            //紀錄Log
                            retStatus = -1;
                            m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------" + gssDoc.DocNo + "匯入資料失敗----------------,錯誤原因：" + ex.ToString());
                        }
                        finally
                        {
                            //foreach (var item in fs)
                            //{
                            //    item.Value.Close();
                            //}
                            //m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "刪除:" + gssDoc.DocNo + ",pdf,di  ----------------");
                        }



                    }
                }
                catch (Exception ex)
                {
                    //throw ex; 
                    //紀錄Log
                    retStatus = -1;
                    m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------" + gssBatchNo.BatchNo + "匯入批次失敗----------------,錯誤原因：" + ex.ToString());
                }
                finally
                {
                    //foreach (var item in fs)
                    //{
                    //    item.Value.Close();
                    //}
                    //m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "刪除:" + gssBatchNo.BatchNo + ",pdf,di  ----------------");
                }



            }
            return retStatus;
           
        }

        private static List<string> CheckRequireField(DocNode fDocNode)
        {
            bool result = true;
            List<string> Message = new List<string>();
            if( string.IsNullOrEmpty(fDocNode.GovTitle))
            {
                result = false;
                Message.Add("發文機關錯誤");
            }
            if (string.IsNullOrEmpty(fDocNode.SendDate))
            {
                result = false;
                Message.Add("發文日期錯誤");
            }
            else
            {
                TaiwanCalendar c = new TaiwanCalendar();
                CultureInfo ci = new CultureInfo("zh-TW", true);
                ci.DateTimeFormat.Calendar = c;
                DateTime dt2;
                var b = DateTime.TryParseExact(fDocNode.SendDate, "中華民國yy年M月d日", ci, DateTimeStyles.AllowWhiteSpaces, out dt2);
                //Console.WriteLine(dt2.ToLongDateString());
                if (!b)
                {
                    result = false;
                    Message.Add("發文日期錯誤");
                }
            }

            // fDocNode.SendWord + "字第" + fDocNode.SendYear + fDocNode.SendNo + fDocNode.SendNo2 + "號";
            if( string.IsNullOrEmpty(fDocNode.SendWord) || string.IsNullOrEmpty(fDocNode.SendYear) || string.IsNullOrEmpty(fDocNode.SendNo))
            {
                result = false;
                Message.Add("發文字號錯誤");
            }

            if(string.IsNullOrEmpty(fDocNode.Speed))
            {
                result = false;
                Message.Add("速別錯誤");
            }
            return Message;
        }

        private static int SaveGssDoc(List<GssDoc> gssList, List<GssDoc_Detail> gssDetails)
        {
            int status = 0;
            try
            {
                using (GssEntities ctx = new GssEntities())
                {
                    ctx.GssDoc.AddRange(gssList);
                    ctx.GssDoc_Detail.AddRange(gssDetails);
                    ctx.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                status = -1;
                Console.WriteLine("Error in SaveChanges");
            }
            return status;
        }

        /// <summary>
        /// 檢查, 此一批號, 是否已經讀取過了
        /// </summary>
        /// <param name="docBatch"></param>
        /// <returns></returns>
        private static FileInfo[] BatchHasRead(FileInfo[] docBatchNo)
        {
            List<FileInfo> result = new List<FileInfo>();
            using (GssEntities ctx = new GssEntities())
            {
                foreach(var d in docBatchNo)
                {
                    if( ! ctx.GssDoc.Any(x=>x.BatchNo==d.Name.Replace(".zip","")) )
                    {
                        result.Add(d);
                    }
                    else
                    {
                        string msg = string.Format("\t\t批號{0}已重覆，不再匯入", d.Name.Replace(".zip", ""));
                        m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, msg);
                    }
                }
            }
            
            return result.ToArray();
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

        private struct DocNode
        {
            /// <summary>
            /// 發文機關全銜
            /// </summary>
            public string GovTitle;
            /// <summary>
            /// 機關代碼
            /// </summary>
            public string GovID;
            /// <summary>
            /// 函類別
            /// </summary>
            public string DocKind;
            /// <summary>
            /// 地址
            /// </summary>
            public string GovAddress;
            /// <summary>
            /// 聯絡方式
            /// </summary>
            public string ContactKind;
            /// <summary>
            /// 發文日期
            /// </summary>
            public string SendDate;
            /// <summary>
            /// 字
            /// </summary>
            public string SendWord;
            /// <summary>
            /// 年度
            /// </summary>
            public string SendYear;
            /// <summary>
            /// 流水號
            /// </summary>
            public string SendNo;
            /// <summary>
            /// 支號
            /// </summary>
            public string SendNo2;
            /// <summary>
            /// 速別
            /// </summary>
            public string Speed;
            /// <summary>
            /// 主旨文字
            /// </summary>
            public string Subject;
            /// <summary>
            /// 說明文字
            /// </summary>
            public string Description;
            /// <summary>
            /// 正本全銜
            /// </summary>
            public string ReceiveTitle;
            /// <summary>
            /// 副本全銜
            /// </summary>
            public string ReceiveCCTitle;

        }

        private static DocNode fDocNode = new DocNode();
        /// <summary>
        /// Parser di xml data
        /// </summary>
        /// <param name="pfile"></param>
        /// <returns></returns>
        private static void ParseXML(string pfile)
        {
            //DocNode mDocNode = new DocNode();
            try
            {
                fDocNode = new DocNode();
                XmlDocument xmlDom = new XmlDocument();
                xmlDom.XmlResolver = null; //不解析外部的 DTD、實體及結構描述
                xmlDom.Load(pfile);

                XmlNode node1; //node1取得 Root(根元素的所有 link節點);node2取得 link節點的所有子節點
                node1 = xmlDom.DocumentElement;

                GetField(node1);
            }
            catch (Exception ex)
            {
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "xml Parser Error:" + ex.Message);

            }
            //return mDocNode;
        }

        private static void GetField(XmlNode pDocNode)
        {
            if (pDocNode.NodeType != XmlNodeType.Text)
            {
                if (pDocNode.Name == "速別" && pDocNode.Attributes.Count > 0)
                {
                    SetField(pDocNode.Name, pDocNode.Attributes[0].Value);
                }
                else
                {
                    foreach (XmlNode node2 in pDocNode.ChildNodes)
                    {
                        GetField(node2);
                    }
                }
            }
            else
            {
                if (pDocNode.ParentNode.Name == "文字")
                {
                    if (pDocNode.ParentNode.ParentNode.Name == "條列" && pDocNode.ParentNode.ParentNode.Attributes.Count > 0)
                        SetField(pDocNode.ParentNode.ParentNode.Name + "文字", pDocNode.ParentNode.ParentNode.Attributes[0].Value + pDocNode.ParentNode.InnerText);
                    else
                        SetField(pDocNode.ParentNode.ParentNode.Name + "文字", pDocNode.ParentNode.InnerText);

                }
                else if (pDocNode.ParentNode.Name == "全銜" || pDocNode.ParentNode.Name == "單位名")
                {
                    SetField(pDocNode.ParentNode.ParentNode.Name + "全銜", pDocNode.ParentNode.InnerText);
                }
                else
                    SetField(pDocNode.ParentNode.Name, pDocNode.ParentNode.InnerText);
            }
            //Response.Write("節點名稱：" + node1.Name + "<br>子節點的標記：" + node1.InnerXml + "<br>這個節點和其所有子節點的標記：" + node1.OuterXml + "<br>")
            //If (node1.HasChildNodes) Then '指出這個節點是否有子節點
            //    For Each node2 In node1.ChildNodes
            //        Response.Write("節點名稱2：" + node2.Name + "，值：" + node2.InnerText + "<br>")
            //    Next
            //End If
            //Response.Write("<br>")
            //Next
        }

        private static void SetField(string pName, string pInnerText)
        {

            switch (pName)
            {
                case "發文機關全銜":
                    fDocNode.GovTitle = pInnerText;
                    break;
                case "機關代碼":
                    fDocNode.GovID = pInnerText;
                    break;
                case "函類別":
                    fDocNode.DocKind = pInnerText;
                    break;
                case "地址":
                    fDocNode.GovAddress = pInnerText;
                    break;
                case "聯絡方式":
                    fDocNode.ContactKind += pInnerText + "\r\n";
                    break;
                case "年月日":
                    fDocNode.SendDate = pInnerText;
                    break;
                case "字":
                    fDocNode.SendWord = pInnerText;
                    break;
                case "年度":
                    fDocNode.SendYear = pInnerText;
                    break;
                case "流水號":
                    fDocNode.SendNo = pInnerText;
                    break;
                case "支號":
                    fDocNode.SendNo2 = pInnerText;
                    break;
                case "速別":
                    fDocNode.Speed = pInnerText;
                    break;
                case "主旨文字":
                    fDocNode.Subject = pInnerText;
                    break;
                case "條列文字":
                    fDocNode.Description += pInnerText + "\r\n";
                    break;
                case "正本全銜":
                    fDocNode.ReceiveTitle = pInnerText;
                    break;
                case "副本全銜":
                    fDocNode.ReceiveCCTitle = pInnerText;
                    break;

            }
        }
    }
}
