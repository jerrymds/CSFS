using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CTBC.CSFS.BussinessLogic;
using CTBC.CSFS.Models;
using CTBC.CSFS.Pattern;
using CTBC.CSFS.Resource;
using CTBC.CSFS.ViewModels;
using CTBC.CSFS.Service;
using System.Configuration;
using System.Net;
using CTBC.FrameWork.Util;
using System.IO;
using System.Collections;
using System.Xml;
using System.Text.RegularExpressions;
using System.Data;

namespace CTBC.WinExe.ImportEDocALL
{
    class Program
    {
        private static int systime;
        private static FileLog m_fileLog;
        private static int timeSection1;
        private static int timeSection2;
        private static int timeSection3;
        private static ImportEDocBiz _ImportEDocBiz;
        private static string ftpserver;
        private static string port;
        private static string username;
        private static string password;
        private static string ftpdir;
        private static string loaclFilePath;
        private static FtpClient ftpClient;
        private static string[] fileTypes;
        private static bool isFtp;
        static Program()
        {
            systime = Convert.ToInt32(DateTime.Now.Hour.ToString().PadLeft(2, '0') + DateTime.Now.Minute.ToString().PadLeft(2, '0'));
            m_fileLog = new FileLog(ConfigurationManager.AppSettings["filelog"], AppDomain.CurrentDomain.FriendlyName.Replace(".vshost", "").Replace(".exe", ""));
            _ImportEDocBiz = new ImportEDocBiz();
            ftpserver = ConfigurationManager.AppSettings["ftpserver"];
            port = ConfigurationManager.AppSettings["port"];
            username = ConfigurationManager.AppSettings["username"];
            password = ConfigurationManager.AppSettings["password"];
            //20160729不加密--simon
            //由於framework中會先做解密
            //password = UtlString.EncodeBase64(password);

            ftpdir = ConfigurationManager.AppSettings["ftpdir"];
            loaclFilePath = ConfigurationManager.AppSettings["loaclFilePath"];
            ftpClient = new FtpClient(ftpserver, username, password, port);
            fileTypes = ConfigurationManager.AppSettings["fileTypes"].Split(',');
            isFtp = bool.Parse(ConfigurationManager.AppSettings["isFtp"].ToString());
        }
        static void Main(string[] args)
        {
            try
            {
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息-------- 電子收文作業開始----------------");
                IList<SendTimeSection> sendTimeSectionList = _ImportEDocBiz.GetSendTimeSectionList();
                string time = DateTime.Now.ToString("yyyy/MM/dd");
                foreach (var item in sendTimeSectionList)
                {
                    if (systime >= Convert.ToInt32(item.TimeSection.Replace(":", "")))
                    {
                        //if (_ImportEDocBiz.GetImportTime(time, item.TimeSection))
                        //{
                        //    m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------" + item.TimeSection + "時段的電子收文作業已執行，電子收文作業結束----------------");
                        //    return;
                        //}
                        //else
                        {
                            ImportEDoc(item.TimeSection);
                            m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------電子收文作業結束----------------");
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------電子收文作業失敗，失敗原因：" + ex.Message + "----------------");
            }
        }


        private static List<AutoPayResult> importAutoPay(string localPath, string timesection)
        {
            List<AutoPayResult> resultAutoPay = new List<AutoPayResult>();
            DirectoryInfo directoryInfo = new DirectoryInfo(localPath);
            foreach (var di in directoryInfo.GetFiles("*.di"))
            {
                AutoPayResult apr = new AutoPayResult();
                apr.caseId = Guid.NewGuid();
                apr.fileName = Path.GetFileNameWithoutExtension(di.Name);

                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, string.Format("開始處理 CaseId : {0} 檔名{1}", apr.caseId, apr.fileName));

                try
                {
                    string Message = string.Empty;

                    // 20200421, 若是X或Y類, 才要處理... 若是A類或K類, 則流到原程式去處理
                    ParseXML(loaclFilePath + di.Name);
                    if (fDocNode.SendNo2.ToUpper() == "X" || fDocNode.SendNo2.ToUpper() == "Y")
                    {
                        var result = ImportEDoc_AutoPay(loaclFilePath, di.Name);
                        // 20200413, 過濾不是822開頭的....
                        result = filter822XXXXInfo(result);
                        if (result.Count(x => x.fields2 != null) != 1) // 822開頭, 只會有一行, 不可能沒有或有二筆以上
                        {
                            m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.Error, string.Format("------發生錯誤, 收取媒體檔，只能有一筆822開頭的... CaseId : {0} 檔名{1}", apr.caseId, apr.fileName));

                            DocNode dn = fDocNode;
                            var sendGovDate = UtlString.FormatDateTwStringToAd(dn.SendDate.Replace("中華民國", "").Replace("年", "/").Replace("月", "/").Replace("日", "/").Trim());
                            var sendGovNo = dn.SendWord + "字第" + dn.SendYear + dn.SendNo + dn.SendNo2 + "號";

                            apr.Message = string.Format("該案件異常無法進件，請確認案件編號. 文號: {0}, 日期: {1} ", sendGovNo, sendGovDate);
                            apr.ImportStatus = false;
                            resultAutoPay.Add(apr);
                        }
                        else
                        {
                            apr.ImportStatus = SaveAutoPay(apr.caseId, result, loaclFilePath, di.Name, timesection, ref Message);
                            apr.Message = Message;
                            resultAutoPay.Add(apr);
                        }
                    }
                }
                catch (Exception ex)
                {
                    m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.Error, string.Format("------發生錯誤... CaseId : {0} 檔名{1}", apr.caseId, apr.fileName));
                    m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.Error, string.Format("{0}", ex.Message.ToString()));
                    apr.Message = ex.Message.ToString();
                }
                finally
                {
                    fDocNode = new DocNode();
                    GC.Collect();
                    GC.WaitForPendingFinalizers();

                }
            }
            return resultAutoPay;
        }


        private static List<AutoPayTxtFields> ImportEDoc_AutoPay(string loaclFilePath, string fileName)
        {
            // 20200408, 不讀取FTP, 直接讀loaclFilePath中的檔案

            // 收取媒體檔(txt檔), 的切割位置
            List<int> Attach2_Pos = new List<int>() {
                        24,10,7,40,100,100,14,14,7,40,7,11,11,11,1
                    };
            // 收取分配媒體檔(txt檔), 的切割位置
            List<int> Attach3_Pos = new List<int>() {
                        24,10,7,40,100,100,14,14,7,40,7,100,11,5,72,20,10,20,100
                    };

            try
            {
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, string.Format("\t開始Parsring {0}", fileName));
                ParseXML(loaclFilePath + fileName);
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "\t開始Parsring成功!!");
            }
            catch (Exception ex)
            {
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.Error, string.Format("\t Parsring{0}失敗", fileName));
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.Error, string.Format("{0}", ex.Message.ToString()));
            }
            string fn = Path.GetFileNameWithoutExtension(fileName);

            //var aaa = fDocNode;

            List<AutoPayTxtFields> txtFiles = new List<AutoPayTxtFields>();



            if (File.Exists(loaclFilePath + fn + "_ATTCH2.txt") && File.Exists(loaclFilePath + fn + "_ATTCH3.txt"))
            {
                try
                {


                    string txt = string.Empty;
                    string txt3 = string.Empty;

                    using (StreamReader streamReader = new StreamReader(loaclFilePath + fn + "_ATTCH2.txt", Encoding.UTF8))
                    {
                        txt = streamReader.ReadToEnd();
                        streamReader.Close();
                        streamReader.Dispose();
                    }


                    using (StreamReader streamReader = new StreamReader(loaclFilePath + fn + "_ATTCH3.txt", Encoding.UTF8))
                    {
                        txt3 = streamReader.ReadToEnd();
                        streamReader.Close();
                        streamReader.Dispose();
                    }


                    string[] lines = Regex.Split(txt, "\r\n");

                    for (int i = 0; i < lines.Count(); i++)
                    {
                        if (lines[i].Length < 50)
                            continue;
                        AutoPayTxtFields atf = new AutoPayTxtFields();

                        atf.parserDi = fDocNode;

                        string[] result = splitByPos(lines[i], Attach2_Pos);
                        atf.fileName = fn;
                        atf.fields2 = result;
                        txtFiles.Add(atf);
                    }

                    string[] lines3 = Regex.Split(txt3, "\r\n");
                    for (int i = 0; i < lines3.Count(); i++)
                    {
                        if (lines3[i].Length < 50)
                            continue;
                        AutoPayTxtFields atf = new AutoPayTxtFields();

                        atf.parserDi = fDocNode;

                        if (lines3[i].Length < 50)
                            continue;
                        string[] result3 = splitByPos(lines3[i], Attach3_Pos);
                        atf.fileName = fn;
                        atf.fields3 = result3;
                        txtFiles.Add(atf);
                    }
                }
                catch (Exception ex)
                {
                    m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.Error, string.Format("\t\t 讀取{0}的錯誤!!", fn));
                }
            }
            else
            {
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.Error, string.Format("\t Parsring{0} 的 ATTCH2.txt 或 ATTCH3.txt 不存在", fn));
            }
            return txtFiles;
        }


        private static List<AutoPayTxtFields> filter822XXXXInfo(List<AutoPayTxtFields> result)
        {
            //         public string fileName { get; set; }
            //          public string[] fields { get; set; } , fields[10]是銀行帳號...
            // 要過濾, 媒體檔與分配檔是822開頭的
            var res3 = result.Where(x => x.fields3 != null && x.fields3[10].StartsWith("822"));
            var res2 = result.Where(x => x.fields2 != null && x.fields2[10].StartsWith("822"));
            return res2.Union(res3).ToList();
        }

        /// <summary>
        /// Save 自動支付的TXT檔及實體檔....
        /// </summary>
        /// <param name="result"></param>
        /// <param name="loaclFilePath"></param>
        /// <param name="CaseNo"></param>
        /// <returns></returns>
        private static bool SaveAutoPay(Guid caseId, List<AutoPayTxtFields> result, string loaclFilePath, string CaseNo, string timesection, ref string Message)
        {
            bool retValue = false;
            bool isObligorNoValid = true; // 是否同時沒有身份證字號及統編.. 若都沒有, 則回失敗....
            try
            {
                DocNode dn = result.Where(x => x.fields2 != null).First().parserDi;

                CaseNoTableBIZ noBiz = new CaseNoTableBIZ();
                CaseMaster caseMaster = new CaseMaster();
                caseMaster.CaseId = caseId;

                caseMaster.GovUnit = dn.GovTitle;
                caseMaster.GovDate = UtlString.FormatDateTwStringToAd(dn.SendDate.Replace("中華民國", "").Replace("年", "/").Replace("月", "/").Replace("日", "/").Trim());
                caseMaster.Speed = dn.Speed;   //速別
                caseMaster.ReceiveKind = "電子公文";
                caseMaster.GovNo = dn.SendWord + "字第" + dn.SendYear + dn.SendNo + dn.SendNo2 + "號";

                //重複建檔檢核功能，檢核來文機關、來文字、來文號，公文案件中已有重覆案件時不匯入
                bool reCase = _ImportEDocBiz.ValiReCase(caseMaster.GovUnit, caseMaster.GovNo);
                if (!reCase)
                {
                    #region
                    caseMaster.LimitDate = DateTime.Now.AddDays(Convert.ToInt32(new PARMCodeBIZ().GetCodeNoByCodeDesc("LIMITDATE1"))).ToString("yyyy/MM/dd");
                    caseMaster.ReceiveDate = DateTime.Now.ToString("yyyy/MM/dd");
                    caseMaster.CaseKind = CaseKind.CASE_SEIZURE;
                    caseMaster.CaseKind2 = CaseKind2.CasePay;
                    caseMaster.Unit = "8888";
                    caseMaster.Person = "9999";   //* 建檔人
                    caseMaster.DocNo = noBiz.GetDocNo();   //* 系統編號
                    //* 案件編號 一般文的.在指派經辦時才有caseno
                    if (caseMaster.CaseKind == CaseKind.CASE_SEIZURE)
                        caseMaster.CaseNo = noBiz.GetCaseNo("A");
                    else
                        caseMaster.CaseNo = "";
                    caseMaster.Status = CaseStatus.CaseInput;   //* 狀態     
                    caseMaster.isDelete = 0;
                    caseMaster.CreatedDate = DateTime.Now.ToString("yyyy/MM/dd");
                    //adam 20180815
                    caseMaster.Receiver = "8888";
                    caseMaster.NotSeizureAmount = 450;
                    caseMaster.IsEnable = "0";

                    var o = result.Where(x => x.fields2 != null).First();


                    CaseObligor caseObligor1 = new CaseObligor();
                    caseObligor1.ObligorName = o.fields2[4].Replace("義務人：", "").Trim();
                    //2016/08/18 simon
                    caseObligor1.ObligorNo = o.fields2[6].Replace("營利事業編號：", "").Trim().Length > 0 ? o.fields2[6].Replace("營利事業編號：", "").Trim() : o.fields2[7].Replace("身分證統一編號：", "").Trim();

                    caseObligor1.CaseId = caseId;
                    caseObligor1.CreatedUser = "電子收文";
                    CaseObligor caseObligor2 = new CaseObligor();
                    caseObligor2.ObligorName = o.fields2[5].Replace("法定代理人：", "").Trim();
                    //2016/08/18 simon
                    caseObligor2.ObligorNo = o.fields2[7].Replace("身分證統一編號：", "").Trim().Length > 0 ? o.fields2[7].Replace("身分證統一編號：", "").Trim() : o.fields2[6].Replace("營利事業編號：", "").Trim();



                    caseObligor2.CaseId = caseId;
                    caseObligor2.CreatedUser = "電子收文";


                    if (string.IsNullOrEmpty(o.fields2[7].Trim()) && string.IsNullOrEmpty(o.fields2[6].Trim()))
                        isObligorNoValid = false;
                    else
                        isObligorNoValid = true;

                    List<CaseObligor> caseObligorList = new List<CaseObligor>();
                    if (caseObligor1.ObligorName != "" && caseObligor1.ObligorNo != "")
                        caseObligorList.Add(caseObligor1);
                    if (caseObligor2.ObligorName != "" && caseObligor2.ObligorNo != "")
                        caseObligorList.Add(caseObligor2);

                    //前案扣押總金額
                    caseMaster.PreSubAmount = int.Parse(o.fields2[11].Trim());
                    //收取金額
                    caseMaster.PreReceiveAmount = int.Parse(o.fields2[12].Trim());
                    //超過收取金額部份是否撤銷
                    caseMaster.OverCancel = o.fields2[14].Trim();
                    //前案來文字號
                    caseMaster.PreGovNo = o.fields2[9].Trim();
                    //前案扣押發文日期
                    caseMaster.PreSubDate = o.fields2[8].Trim();
                    // 收取金額_執行必要費用, 20200717, 芯瑜說, 預設250元, 若自動支付後, 才改成,250*ID數
                    caseMaster.AddCharge = 250;

                    #region 產出eDocTxt3
                    EDocTXT3 et3 = new EDocTXT3();
                    //CaseId, GovUnit, GovUnitCode, GovDate, ReceiverNo, ObligorName, Agent, RegistrationNo, ObligorNo, SeizureIssueDate, SeizureIssueNo, ReceiveBankId

                    et3.CaseId = caseId;
                    et3.GovUnit = o.fields2[0].Trim();
                    et3.GovUnitCode = o.fields2[1].Trim();
                    et3.GovDate = o.fields2[2].Trim();
                    et3.ReceiverNo = o.fields2[3].Trim();
                    et3.ObligorName = o.fields2[4].Trim();
                    et3.Agent = o.fields2[5].Trim();
                    et3.RegistrationNo = o.fields2[6].Trim();
                    et3.ObligorNo = o.fields2[7].Trim();
                    et3.SeizureIssueDate = o.fields2[8].Trim();
                    et3.SeizureIssueNo = o.fields2[9].Trim();
                    //et3.ReceiveBankId = o.fields2[10].Trim(); // 搬到detail 去, 因為可能會有1個以上的BankID (而且是822的)
                    _ImportEDocBiz.InsertEDocTXT3(et3);
                    #endregion


                    #region 產生eDocTxt3_Detail


                    List<EDocTXT3_Detail> list_et3_detail = new List<EDocTXT3_Detail>();
                    foreach (var o1 in result.Where(x => x.fields3 != null))
                    {
                        EDocTXT3_Detail et3_detail = new EDocTXT3_Detail();
                        //CaseId, SeizureAmount, ReceiveAmount, ReceiveFee, OverPayCancel, ReceiveUnit, ReceiveAmount_Case, ReceiveFee_Case, CheckAddress, Unit, PassbookAbsNo, WriteOffNo, ReceiveName
                        et3_detail.CaseId = caseId;
                        et3_detail.ReceiveBankId = o.fields2[10].Trim();
                        et3_detail.SeizureAmount = o.fields2[11].Trim();
                        et3_detail.ReceiveAmount = o.fields2[12].Trim();
                        et3_detail.ReceiveFee = o.fields2[13].Trim();
                        et3_detail.OverPayCancel = o.fields2[14].Trim();
                        et3_detail.ReceiveUnit = o1.fields3[11].Trim();
                        et3_detail.ReceiveAmount_Case = o1.fields3[12].Trim();
                        et3_detail.ReceiveFee_Case = o1.fields3[13].Trim();
                        et3_detail.CheckAddress = o1.fields3[14].Trim();
                        et3_detail.Unit = o1.fields3[15].Trim();
                        et3_detail.PassbookAbsNo = o1.fields3[16].Trim();
                        et3_detail.WriteOffNo = o1.fields3[17].Trim();
                        et3_detail.ReceiveName = o1.fields3[18].Trim();
                        list_et3_detail.Add(et3_detail);
                    }
                    _ImportEDocBiz.InsertEDocTXT3_Detail(list_et3_detail);

                    #endregion

                    if (!string.IsNullOrEmpty(list_et3_detail.First().SeizureAmount))
                    {
                        caseMaster.ReceiveAmount = int.Parse(o.fields2[12].Trim());
                    }
                    else
                    {
                        caseMaster.ReceiveAmount = 0;
                    }



                    _ImportEDocBiz.InsertCaseMast(caseMaster);

                    if (caseObligor1.ObligorName != "" && caseObligor1.ObligorNo != "")
                        _ImportEDocBiz.InsertCaseObligor(caseObligor1);
                    if (caseObligor2.ObligorName != "" && caseObligor2.ObligorNo != "")
                        _ImportEDocBiz.InsertCaseObligor(caseObligor2);
                    _ImportEDocBiz.CreateLog(caseObligorList, caseMaster, null);




                    string[] ext = new string[] { ".di", "_ATTCH1.pdf", "_ATTCH2.txt", "_ATTCH3.txt", "_di.pdf" };


                    string baseFilename = o.fileName;



                    #region 上傳到該案件的附件目錄(txt、di檔與pdf檔)

                    foreach (string ex in ext)
                    {

                        FileStream fs = File.OpenRead(loaclFilePath + baseFilename + ex);
                        StreamReader sr = new StreamReader(loaclFilePath + baseFilename + ex, Encoding.UTF8);
                        //StreamReader(fs["txt"], Encoding.Default)


                        CaseEdocFile caseEdocFile = new CaseEdocFile();
                        caseEdocFile.CaseId = caseId;
                        caseEdocFile.Type = "收文";

                        string[] extSplit = ex.Split('.');

                        caseEdocFile.FileType = extSplit[1];
                        caseEdocFile.FileName = baseFilename + ex;

                        //20200720, 若要修改_Attach2.txt的換行, 則在此處修改...

                        byte[] bytes = new byte[fs.Length];
                        fs.Position = 0;
                        fs.Read(bytes, 0, bytes.Length);


                        caseEdocFile.FileObject = bytes;
                        caseEdocFile.SendNo = baseFilename + ex;
                        _ImportEDocBiz.InsertCaseEdocFile(caseEdocFile);
                    }

                    #endregion





                    #endregion
                }
                else
                {
                    m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------案件重覆!!" + caseMaster.GovNo + "匯入資料失敗, 直接跳過----------------");
                }

                #region 所有匯入的資訊寫入ImportEdocData[電子公文匯入]的資料表
                ImportEdocData importEdocData = new ImportEdocData();
                importEdocData.Timesection = timesection;
                importEdocData.DocNo = caseMaster.DocNo;
                importEdocData.CaseNo = caseMaster.CaseNo;
                importEdocData.GovUnit = caseMaster.GovUnit;
                importEdocData.Added = reCase ? "0" : "1";
                importEdocData.GovDate = Convert.ToDateTime(caseMaster.GovDate);
                importEdocData.GovNo = caseMaster.GovNo;
                
                //20210503, 若有錯誤, 則回傳錯誤訊息
                string exMessage = string.Empty;
                _ImportEDocBiz.ImportEdocData(importEdocData, ref exMessage);



                #endregion

                if (!string.IsNullOrEmpty(exMessage))
                {
                    retValue = false;
                    m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "錯誤--------儲存ImportEDocData錯誤----------------" + exMessage.ToString());
                    Message = "儲存ImportEDocData錯誤";
                }
                else
                    retValue = true;
            }
            catch (Exception ex)
            {
                Message = ex.Message.ToString();

            }
            if (!isObligorNoValid)
            {
                retValue = false;
                Message = "營利事業編號與身分證統一編號皆無值";
            }

            return retValue;
        }


        private static int getLength(string txt)
        {
            int strLength = 0;
            int realPos = 0;
            char[] Temp = txt.ToCharArray();
            for (int i = 0; i != Temp.Length; i++)
            {
                if (((int)Temp[i]) < 255) //大於255的都是漢字或者特殊字符
                {
                    strLength++;
                    realPos++;
                }
                else
                {
                    strLength = strLength + 2;
                    realPos++;
                }
            }
            return strLength;
        }

        private static string[] splitByPos(string OrginTxt, List<int> Pos)
        {
            List<string> result = new List<string>();
            int TotalLen = getLength(OrginTxt);
            int strLength = 0;

            if (TotalLen <= 900)
            {
                OrginTxt = OrginTxt.PadRight(900, ' ');
            }

            char[] Temp = OrginTxt.ToCharArray();

            string txt = OrginTxt;
            int prePos = 0;
            int realPos = 0;

            foreach (var p in Pos)
            {
                for (int i = 0; i != Temp.Length; i++)
                {
                    if (((int)Temp[i]) < 255) //大於255的都是漢字或者特殊字符
                    {
                        strLength++;
                        realPos++;
                    }
                    else
                    {
                        strLength = strLength + 2;
                        realPos++;
                    }
                    if (strLength == p) // 斷開
                    {
                        result.Add(txt.Substring(0, realPos));
                        prePos = p;

                        txt = txt.Substring(realPos);
                        realPos = 0;
                        strLength = 0;
                        break;
                    }
                }
                Temp = txt.ToCharArray();
            }
            return result.ToArray();
        }

        private static bool IsFileLocked(string fileStr)
        {
            try
            {
                FileInfo file = new FileInfo(fileStr);
                using (FileStream stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    stream.Close();
                }
            }
            catch (IOException)
            {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                return true;
            }

            //file is not locked
            return false;
        }


        private static void noticeMail_Success(string[] mailFromTo, int TotalCount, string FileNames)
        {
            if (TotalCount > 0)
            {
                string mailFrom = ConfigurationManager.AppSettings["mailFrom"];
                string subject = string.Format("自動支付匯入成功!, 共計{0}件", TotalCount);
                string body = string.Format("匯入成功檔名： {0} \r\n本信件由系統直接發送!請勿直接回覆，謝謝!\r\n", FileNames);
                string host = ConfigurationManager.AppSettings["mailHost"];
                UtlMail.SendEmail(mailFrom, mailFromTo, subject, body, host);
            }
        }

        private static void noticeMail_All(string[] mailFromTo, int TotalSuccessCount, string SuccessFileNames, int TotalFailCount, string FailFileNames)
        {
            //if (TotalCount > 0)
            {
                string mailFrom = ConfigurationManager.AppSettings["mailFrom"];
                string subject = string.Format("自動支付匯入成功{0}件, 失敗{1}件", TotalSuccessCount, TotalFailCount);
                string body = string.Format("匯入成功檔名： {0} \r\n 匯入失敗檔名： {1} \r\n本信件由系統直接發送!請勿直接回覆，謝謝!\r\n", SuccessFileNames, FailFileNames);
                string host = ConfigurationManager.AppSettings["mailHost"];
                UtlMail.SendEmail(mailFrom, mailFromTo, subject, body, host);
            }
        }

        private static void noticeMail_Fail(string[] mailFromTo, int TotalCount, string FileNames)
        {
            if (TotalCount > 0)
            {
                string mailFrom = ConfigurationManager.AppSettings["mailFrom"];
                string subject = string.Format("自動支付匯入失敗!, 共計{0}件", TotalCount);
                string body = string.Format("匯入失敗檔名： {0} \r\n本信件由系統直接發送!請勿直接回覆，謝謝!\r\n", FileNames);
                string host = ConfigurationManager.AppSettings["mailHost"];
                UtlMail.SendEmail(mailFrom, mailFromTo, subject, body, host);
            }
        }



        /// <summary>
        /// 執行電子收文作業
        /// </summary>
        private static void ImportEDoc(string timesection)
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



            PARMCodeBIZ pbiz = new PARMCodeBIZ();
            var gssDocMail = pbiz.GetParmCodeByCodeType("GssDocNoticeMail").FirstOrDefault();
            string[] mailTo = gssDocMail.CodeMemo.Split(',');


            
            //循環建檔
            m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------開始匯入資料----------------");


            List<string> fileNames = new List<string>();

            if (isFtp)
            {

                //獲取FTP文件清單
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------正在獲取FTP文件清單----------------");
                ArrayList fileList = ftpClient.GetFileList(ftpdir);
                //下載FTP指定目錄下的所有文件
                foreach (var file in fileList)
                {
                    string remoteFile = ftpClient.SetRemotePath(ftpdir) + "//" + file;
                    string localFile = loaclFilePath.TrimEnd('\\') + "\\" + file;
                    ftpClient.GetFiles(remoteFile, localFile);
                }
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------獲取FTP文件清單結束----------------");

                fileNames = (Array.FindAll((string[])fileList.ToArray(typeof(string)), delegate(String item)
                {
                    return item.Contains("." + fileTypes[0]) ? true : false;
                })).ToList();
            }
            else
            {
                var fileNamesTemp = Directory.GetFiles(loaclFilePath, "*.txt").ToList();
                // 20200610, 要把路徑拿掉//
                foreach (var f in fileNamesTemp)
                {
                    fileNames.Add(f.Replace(loaclFilePath, ""));
                }
            }



            //***** ImportAutoPay **** *********************Start********************************************// 


            m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------開始匯入支付相關資料----------------");

            var result = importAutoPay(loaclFilePath, timesection);

            m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------發送支付匯入Mail----------------");
            // 20200416, 發送mail .... 未來, 要拿掉, 才會發mail ...
            var successImports = result.Where(x => x.ImportStatus).ToList();
            var failImports = result.Where(x => !x.ImportStatus).ToList();

            try
            {
                //// 20200610, 因為在測試時, isFTP會是false, 所以也不要寄成功或失敗的mail....
                //if( successImports.Count()>0)
                //    noticeMail_Success(mailTo, successImports.Count(), string.Join(",", successImports.Select(x => x.fileName)));
                //// 20200610, 因為在測試時, isFTP會是false, 所以也不要寄成功或失敗的mail....
                //if( failImports.Count()>0)
                //    noticeMail_Fail(mailTo, failImports.Count(), string.Join(",", failImports.Select(x => x.fileName)));

                // 20200827, 要把成功跟失敗mail合成一封信
                if (successImports.Count() > 0)
                    noticeMail_All(mailTo, successImports.Count(), string.Join(",", successImports.Select(x => x.fileName)), failImports.Count(), string.Join(",", failImports.Select(x => x.fileName)));



            }
            catch (Exception ex)
            {
                Console.WriteLine("Mail Error" + ex.Message.ToString());
            }
            m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------匯入資料結束,總筆數(支付)" + result.Count().ToString() + "筆，成功筆數" + successImports.Count().ToString() + "筆，失敗筆數" + failImports.Count().ToString() + "筆----------------");

            m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------完成匯入支付相關資料----------------");
            //***** ImportAutoPay **** *********************END********************************************// 

            int i = 0;
            m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------開始匯入扣押/撒銷相關資料----------------");
            int autoPayCount = 0; // 目的在計算自動支付的....數量, 要減掉...
            foreach (var fileName in fileNames)
            {

                string fname = Path.GetFileNameWithoutExtension(fileName);
                // 20200420 排除 自動支付的檔案... 因為前面已處理....
                bool isAutoPay = false;
                foreach (var ap in result.Select(x => x.fileName))
                {
                    if (fname.StartsWith(ap))
                        isAutoPay = true;
                }
                if (isAutoPay)
                {
                    autoPayCount++;
                    continue;
                }

                Dictionary<string, FileStream> fs = new Dictionary<string, FileStream>();
                StreamReader sr1 = null;
                StreamReader sr2 = null;

                EDocTXT1 eDocTXT1 = null;
                List<EDocTXT1_Detail> list1 = null;

                EDocTXT2 eDocTXT2 = null;
                List<EDocTXT2_Detail> list2 = null;



                string text1 = string.Empty;
                string text2 = string.Empty;

                Guid caseId = Guid.NewGuid();
                try
                {
                    string mDI_File = "";
                    foreach (var type in fileTypes)
                    {
                        string filePath = loaclFilePath.TrimEnd('\\') + "\\" + fileName.Replace("." + fileTypes[0], "." + type);
                        if (File.Exists(filePath))
                        {
                            fs.Add(type, File.OpenRead(filePath));
                        }
                        if (type.ToUpper() == "di".ToUpper())
                            mDI_File = filePath;
                    }
                    //sr1 = fs.ContainsKey("txt") ? new StreamReader(fs["txt"], Encoding.UTF8) : null;
                    //sr2 = fs.ContainsKey("di") ? new StreamReader(fs["di"], Encoding.UTF8) : null;
                    sr1 = fs.ContainsKey("txt") ? new StreamReader(fs["txt"], Encoding.UTF8) : null;
                    sr2 = fs.ContainsKey("di") ? new StreamReader(fs["di"], Encoding.UTF8) : null;
                    text1 = sr1 == null ? string.Empty : sr1.ReadToEnd();
                    text2 = sr2 == null ? string.Empty : sr2.ReadToEnd();
                    if (!string.IsNullOrEmpty(text1))
                    {
                        //解析XML  Simon 2016/08/08
                        ParseXML(mDI_File);
                        text1 = text1.Replace("附表：\r\n", "");

                        string[] fields = text1.Split(new string[] { "\r\n" }, StringSplitOptions.None);
                        string caseKind2 = string.Empty;
                        //扣押案件
                        if (fDocNode.SendNo2.ToUpper() == "A")
                        {
                            #region 扣押案件

                            eDocTXT1 = new EDocTXT1();
                            eDocTXT1.CaseId = caseId;
                            eDocTXT1.GovUnit = fields[0].Replace("發文機關：", "").Trim();
                            eDocTXT1.GovUnitCode = fields[1].Replace("發文機關代碼：", "").Trim();
                            eDocTXT1.GovDate = UtlString.FormatDateTwStringToAd(fields[2].Replace("發文日期：", "").Replace("年", "/").Replace("月", "/").Replace("日", "/").Trim());
                            eDocTXT1.ReceiverNo = fields[3].Replace("發文字號：", "").Trim();
                            eDocTXT1.ObligorName = fields[4].Replace("義務人：", "").Trim();
                            eDocTXT1.Agent = fields[5].Replace("法定代理人：", "").Trim();
                            eDocTXT1.RegistrationNo = fields[6].Replace("營利事業編號：", "").Trim();
                            eDocTXT1.ObligorNo = fields[7].Replace("身分證統一編號：", "").Trim();
                            eDocTXT1.Amount = fields[8].Replace("應執行金額總計：", "").Trim();
                            eDocTXT1.Fee = fields[9].Replace("執行必要費用：", "").Trim();
                            eDocTXT1.Total = fields[10].Replace("合計：", "").Trim();
                            eDocTXT1.Memo = fields[11].Replace("備註：", "").Trim();
                            eDocTXT1.Unit = fields[12].Replace("單位：", "").Trim();
                            eDocTXT1.Contact = fields[13].Replace("聯絡人：", "").Trim();
                            eDocTXT1.Telephone = fields[14].Replace("電話：", "").Trim();
                            list1 = new List<EDocTXT1_Detail>();
                            for (int j = 18; j < fields.Length; j++)
                            {
                                if (string.IsNullOrEmpty(fields[j]))
                                {
                                    continue;
                                }
                                EDocTXT1_Detail eDocTXT1_Detail = new EDocTXT1_Detail();
                                eDocTXT1_Detail.CaseId = caseId;
                                eDocTXT1_Detail.ExecCaseNo = fields[j].Substring(0, 15).Trim();
                                eDocTXT1_Detail.TransferUnitID = fields[j].Substring(19, 6).Trim();
                                eDocTXT1_Detail.TransferCaseNo = fields[j].Substring(26, 27).Trim();

                                //20160811 simon 修正測試案例的位元數不正確
                                //eDocTXT1_Detail.ManageID = fields[j].Substring(56, 24).Trim();
                                if (fields[j].Length >= 80)
                                    eDocTXT1_Detail.ManageID = fields[j].Substring(56, 24).Trim();
                                else
                                    eDocTXT1_Detail.ManageID = fields[j].Substring(56).Trim();

                                list1.Add(eDocTXT1_Detail);
                            }
                            caseKind2 = CaseKind2.CaseSeizure;
                            #endregion
                        }
                        //撤銷案件
                        else if (fDocNode.SendNo2.ToUpper() == "K")
                        {
                            #region    撤銷案件
                            eDocTXT2 = new EDocTXT2();
                            eDocTXT2.CaseId = caseId;
                            eDocTXT2.GovUnit = fields[0].Replace("發文機關：", "").Trim();
                            eDocTXT2.GovUnitCode = fields[1].Replace("發文機關代碼：", "").Trim();
                            eDocTXT2.GovDate = UtlString.FormatDateTwStringToAd(fields[2].Replace("發文日期：", "").Replace("年", "/").Replace("月", "/").Replace("日", "/").Trim());
                            eDocTXT2.ReceiverNo = fields[3].Replace("發文字號：", "").Trim();
                            eDocTXT2.ObligorName = fields[4].Replace("義務人：", "").Trim();
                            eDocTXT2.Agent = fields[5].Replace("法定代理人：", "").Trim();
                            eDocTXT2.RegistrationNo = fields[6].Replace("營利事業編號：", "").Trim();
                            eDocTXT2.ObligorNo = fields[7].Replace("身分證統一編號：", "").Trim();
                            eDocTXT2.GovDate2 = UtlString.FormatDateTwStringToAd(fields[8].Replace("扣押命令發文日期：", "").Replace("年", "/").Replace("月", "/").Replace("日", "/").Trim());
                            eDocTXT2.ReceiverNo2 = fields[9].Replace("扣押命令發文字號：", "").Trim();
                            eDocTXT2.Amount = fields[10].Replace("保留扣押金額：", "").Trim();
                            eDocTXT2.Memo = fields[11].Replace("備註：", "").Trim();
                            eDocTXT2.Unit = fields[12].Replace("單位：", "").Trim();
                            eDocTXT2.Contact = fields[13].Replace("聯絡人：", "").Trim();
                            eDocTXT2.Telephone = fields[14].Replace("電話：", "").Trim();
                            list2 = new List<EDocTXT2_Detail>();
                            for (int j = 18; j < fields.Length; j++)
                            {
                                if (string.IsNullOrEmpty(fields[j]))
                                {
                                    continue;
                                }
                                EDocTXT2_Detail eDocTXT2_Detail = new EDocTXT2_Detail();
                                eDocTXT2_Detail.CaseId = caseId;
                                eDocTXT2_Detail.ExecCaseNo = fields[j].Substring(0, 15).Trim();
                                eDocTXT2_Detail.TransferUnitID = fields[j].Substring(19, 6).Trim();
                                eDocTXT2_Detail.TransferCaseNo = fields[j].Substring(26, 27).Trim();

                                //20160811 simon 修正測試案例的位元數不正確
                                //eDocTXT2_Detail.ManageID = fields[j].Substring(56, 24).Trim();
                                if (fields[j].Length >= 80)
                                    eDocTXT2_Detail.ManageID = fields[j].Substring(56, 24).Trim();
                                else
                                    eDocTXT2_Detail.ManageID = fields[j].Substring(56).Trim();

                                list2.Add(eDocTXT2_Detail);
                            }
                            caseKind2 = CaseKind2.CaseSeizureCancel;
                            #endregion
                        }




                        CaseNoTableBIZ noBiz = new CaseNoTableBIZ();
                        CaseMaster caseMaster = new CaseMaster();
                        caseMaster.CaseId = caseId;

                        //caseMaster.GovUnit = fields[0].Replace("發文機關：", "").Trim();
                        //caseMaster.GovDate = UtlString.FormatDateTwStringToAd(fields[2].Replace("發文日期：", "").Replace("年", "/").Replace("月", "/").Replace("日", "/").Trim());
                        //caseMaster.Speed = (new Regex("<速別\\s*代碼=\"\\w*\" />")).Match(text2).Value.Replace("<速別", "").Replace("代碼=", "").Replace("\"", "").Replace("/>", "").Trim();   //速別
                        //caseMaster.ReceiveKind = "電子公文";
                        //caseMaster.GovNo = (new Regex("<字>\\w*</字>")).Match(text2).Value.Replace("<字>", "").Replace("</字>", "").Trim() + "字第" + (new Regex("<年度>\\w*</年度>")).Match(text2).Value.Replace("<年度>", "").Replace("</年度>", "").Trim() + (new Regex("<流水號>\\w*</流水號>")).Match(text2).Value.Replace("<流水號>", "").Replace("</流水號>", "").Trim() + (new Regex("<支號>\\w*</支號>")).Match(text2).Value.Replace("<支號>", "").Replace("</支號>", "").Trim() + "號";   //來文字號
                        caseMaster.GovUnit = fDocNode.GovTitle;
                        caseMaster.GovDate = UtlString.FormatDateTwStringToAd(fDocNode.SendDate.Replace("中華民國", "").Replace("年", "/").Replace("月", "/").Replace("日", "/").Trim());
                        caseMaster.Speed = fDocNode.Speed;   //速別
                        caseMaster.ReceiveKind = "電子公文";
                        caseMaster.GovNo = fDocNode.SendWord + "字第" + fDocNode.SendYear + fDocNode.SendNo + fDocNode.SendNo2 + "號";

                        //重複建檔檢核功能，檢核來文機關、來文字、來文號，公文案件中已有重覆案件時不匯入
                        bool reCase = _ImportEDocBiz.ValiReCase(caseMaster.GovUnit, caseMaster.GovNo);
                        if (!reCase)
                        {
                            #region
                            caseMaster.LimitDate = DateTime.Now.AddDays(Convert.ToInt32(new PARMCodeBIZ().GetCodeNoByCodeDesc("LIMITDATE1"))).ToString("yyyy/MM/dd");
                            caseMaster.ReceiveDate = DateTime.Now.ToString("yyyy/MM/dd");
                            caseMaster.CaseKind = CaseKind.CASE_SEIZURE;
                            caseMaster.CaseKind2 = caseKind2;
                            caseMaster.Unit = "8888";
                            caseMaster.Person = "9999";   //* 建檔人
                            caseMaster.DocNo = noBiz.GetDocNo();   //* 系統編號
                            //* 案件編號 一般文的.在指派經辦時才有caseno
                            if (caseMaster.CaseKind == CaseKind.CASE_SEIZURE)
                                caseMaster.CaseNo = noBiz.GetCaseNo("A");
                            else
                                caseMaster.CaseNo = "";
                            caseMaster.Status = CaseStatus.CaseInput;   //* 狀態     
                            caseMaster.isDelete = 0;
                            caseMaster.CreatedDate = DateTime.Now.ToString("yyyy/MM/dd");
                            //adam 20180815
                            caseMaster.Receiver = "8888";
                            caseMaster.NotSeizureAmount = 450;

                            CaseObligor caseObligor1 = new CaseObligor();
                            caseObligor1.ObligorName = fields[4].Replace("義務人：", "").Trim();
                            //2016/08/18 simon
                            caseObligor1.ObligorNo = fields[6].Replace("營利事業編號：", "").Trim().Length > 0 ? fields[6].Replace("營利事業編號：", "").Trim() : fields[7].Replace("身分證統一編號：", "").Trim();

                            caseObligor1.CaseId = caseId;
                            caseObligor1.CreatedUser = "電子收文";
                            CaseObligor caseObligor2 = new CaseObligor();
                            caseObligor2.ObligorName = fields[5].Replace("法定代理人：", "").Trim();
                            //2016/08/18 simon
                            caseObligor2.ObligorNo = fields[7].Replace("身分證統一編號：", "").Trim().Length > 0 ? fields[7].Replace("身分證統一編號：", "").Trim() : fields[6].Replace("營利事業編號：", "").Trim();

                            caseObligor2.CaseId = caseId;
                            caseObligor2.CreatedUser = "電子收文";

                            List<CaseObligor> caseObligorList = new List<CaseObligor>();
                            if (caseObligor1.ObligorName != "" && caseObligor1.ObligorNo != "")
                                caseObligorList.Add(caseObligor1);
                            if (caseObligor2.ObligorName != "" && caseObligor2.ObligorNo != "")
                                caseObligorList.Add(caseObligor2);

                            if (eDocTXT1 != null)
                            {
                                _ImportEDocBiz.InsertEDocTXT1(eDocTXT1);
                                _ImportEDocBiz.InsertEDocTXT1_Detail(list1);
                                if (!string.IsNullOrEmpty(eDocTXT1.Total))
                                {
                                    caseMaster.ReceiveAmount = int.Parse(eDocTXT1.Total);
                                }
                                else
                                {
                                    caseMaster.ReceiveAmount = 0;
                                }
                            }
                            else if (eDocTXT2 != null)
                            {
                                _ImportEDocBiz.InsertEDocTXT2(eDocTXT2);
                                _ImportEDocBiz.InsertEDocTXT2_Detail(list2);
                            }

                            _ImportEDocBiz.InsertCaseMast(caseMaster);

                            if (caseObligor1.ObligorName != "" && caseObligor1.ObligorNo != "")
                                _ImportEDocBiz.InsertCaseObligor(caseObligor1);
                            if (caseObligor2.ObligorName != "" && caseObligor2.ObligorNo != "")
                                _ImportEDocBiz.InsertCaseObligor(caseObligor2);
                            _ImportEDocBiz.CreateLog(caseObligorList, caseMaster, null);

                            #region 上傳到該案件的附件目錄(txt、di檔與pdf檔)
                            foreach (var item in fs)
                            {
                                byte[] bytes = new byte[item.Value.Length];
                                CaseEdocFile caseEdocFile = new CaseEdocFile();
                                caseEdocFile.CaseId = caseId;
                                caseEdocFile.Type = "收文";
                                caseEdocFile.FileType = item.Key;
                                caseEdocFile.FileName = fileName.Replace("." + fileTypes[0], "." + item.Key);
                                item.Value.Position = 0;
                                item.Value.Read(bytes, 0, bytes.Length);

                                //20200710 RC 扣押撤銷電子進件TXT改UTF8 宏祥 update start
                                //if (item.Key.ToUpper() == "TXT")
                                //{                                
                                //    bytes = Encoding.Convert(Encoding.Default, Encoding.UTF8, bytes);
                                //}
                                //20200710 RC 扣押撤銷電子進件TXT改UTF8 宏祥 update end

                                caseEdocFile.FileObject = bytes;
                                caseEdocFile.SendNo = "";
                                _ImportEDocBiz.InsertCaseEdocFile(caseEdocFile);
                            }
                            #endregion

                            #endregion
                        }
                        else
                        {
                            m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------案件重覆!!" + caseMaster.GovNo + "匯入資料失敗, 直接跳過----------------");
                        }


                        //所有匯入的資訊寫入ImportEdocData[電子公文匯入]的資料表
                        ImportEdocData importEdocData = new ImportEdocData();
                        importEdocData.Timesection = timesection;
                        importEdocData.DocNo = caseMaster.DocNo;
                        importEdocData.CaseNo = caseMaster.CaseNo;
                        importEdocData.GovUnit = caseMaster.GovUnit;
                        importEdocData.Added = reCase ? "0" : "1";
                        importEdocData.GovDate = Convert.ToDateTime(caseMaster.GovDate);
                        importEdocData.GovNo = caseMaster.GovNo;
                        _ImportEDocBiz.ImportEdocData(importEdocData);
                        i++;
                    }
                }
                catch (Exception ex)
                {
                    //throw ex; 
                    //紀錄Log
                    m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------" + fileName + "匯入資料失敗----------------,錯誤原因：" + ex.ToString());
                }
                finally
                {
                    foreach (var item in fs)
                    {
                        item.Value.Close();
                    }
                    if (sr1 != null)
                    {
                        sr1.Close();
                    }
                    if (sr2 != null)
                    {
                        sr2.Close();
                    }



                    try
                    {
                        List<string> delFiles = new List<string>();

                        var allFtpFiles = ftpClient.GetFileList(ftpdir);

                        if( allFtpFiles.Contains(fileName))
                            delFiles.Add(fileName);
                        if (allFtpFiles.Contains(fileName.Replace(".txt", ".pdf")))
                            delFiles.Add(fileName.Replace(".txt", ".pdf"));
                        if (allFtpFiles.Contains(fileName.Replace(".txt", ".di")))
                            delFiles.Add(fileName.Replace(".txt", ".di"));



                        if (isFtp)
                        {
                            ftpClient.DeleteFiles(ftpdir, delFiles.ToArray());
                            
                            //foreach (var f in delFiles)                            
                            //{
                            //        ftpClient.DeleteFile(f);
                            //}

                            m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "刪除:" + fileName + ",pdf,di  ----------------");
                        }
                    }
                    catch (Exception ex)
                    {
                        
                        m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "刪除失敗" + ex.Message.ToString());
                    }


                }
            }


            m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------匯入資料結束,總筆數(扣押及撒銷)" + (fileNames.Count() - autoPayCount).ToString() + "筆，成功筆數" + i + "筆，失敗筆數" + (fileNames.Count() - i - autoPayCount).ToString() + "筆----------------");

            //刪除FTP目錄的所有檔案
            //CTBC.CSFS.BussinessLogic.ImportEDocBiz edoc = new CTBC.CSFS.BussinessLogic.ImportEDocBiz();
            //string[] deletefiles = fileNames;
            //List<string> delFiles = new List<string>();
            //for (int j = 0; j < fileNames.Count(); j++)
            //{
            //    if (edoc.GetImportEdocDataByFileName(fileNames[j].ToString()) == true)
            //    {
            //        //deletefiles = deletefiles.Where(w => w != fileNames[j]).ToArray();
            //        delFiles.Add(fileNames[j]);
            //        delFiles.Add(fileNames[j].Replace(".txt", ".pdf"));
            //        delFiles.Add(fileNames[j].Replace(".txt", ".di"));
            //    }
            //}

            //ftpClient.DeleteFiles(ftpdir, delFiles.ToArray());
            //m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------已刪除" + fileNames.Count() + "筆----------------");

            //刪除本地所有檔案(扣押, 撒銷的...)
            foreach (var filename in fileNames)
            {
                foreach (var filetype in fileTypes)
                {
                    string filepath = loaclFilePath.TrimEnd('\\') + "\\" + filename.Replace("." + fileTypes[0], "." + filetype);
                    if (File.Exists(filepath))
                    {
                        if (!IsFileLocked(filepath)) // 檢查.. 先打開, 再關閉
                            File.Delete(filepath);
                    }
                }
            }

            string[] fileExt = new string[7] { ".di", "_ATTCH1.pdf", "_ATTCH2.txt", "_ATTCH3.txt", "_ATTCH4.txt", "_ATTCH5.pdf", "_di.pdf" };
            foreach (var r in result.Select(x => x.fileName))
            {
                foreach (var ext in fileExt)
                {
                    try
                    {
                        string filepath = loaclFilePath.TrimEnd('\\') + "\\" + r + ext;
                        if (File.Exists(filepath))
                        {
                            if(! IsFileLocked(filepath))
                                File.Delete(filepath);
                        }
                            
                    }
                    catch (Exception ex)
                    {
                        m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "刪除:" + r + "." + ext + " ----------------失敗!!!" + ex.Message.ToString());
                    }

                }
            }

            List<string> delFiles2 = new List<string>();
            var allFtpFiles2 = ftpClient.GetFileList(ftpdir);

            foreach (var r in result.Select(x => x.fileName))
            {
                if(allFtpFiles2.Contains(r+".di"))
                    delFiles2.Add(r + ".di");
                if (allFtpFiles2.Contains(r + "_ATTCH1.pdf"))
                    delFiles2.Add(r + "_ATTCH1.pdf");
                if (allFtpFiles2.Contains(r + "_ATTCH2.txt"))
                    delFiles2.Add(r + "_ATTCH2.txt");
                if (allFtpFiles2.Contains(r + "_ATTCH3.txt"))
                    delFiles2.Add(r + "_ATTCH3.txt");
                if (allFtpFiles2.Contains(r + "_ATTCH4.txt"))
                    delFiles2.Add(r + "_ATTCH4.txt");
                if (allFtpFiles2.Contains(r + "_ATTCH5.pdf"))
                    delFiles2.Add(r + "_ATTCH5.pdf");
                if (allFtpFiles2.Contains(r + "_di.pdf"))
                    delFiles2.Add(r + "_di.pdf");
            }
            if (isFtp)
            {
                try
                {
                    ftpClient.DeleteFiles(ftpdir, delFiles2.ToArray());
                    //20201222, 先檢查FTP有沒有那個檔. 若有才砍...
                    //foreach(var f in delFiles2)
                    //{
                    //    ftpClient.SetRemotePath(ftpdir);
                    //    if (ftpClient.ValidateFileExist(ftpserver, f, username, password))
                    //        ftpClient.DeleteFile(f);
                    //}                    
                    m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "刪除:" + string.Join(",", result.Select(x => x.fileName)) + "在FTP上的檔案 ----------------");
                }
                catch (Exception ex)
                {
                    m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "刪除FTP: ----------------失敗!!!" + ex.Message.ToString());
                }                
                
            }
            //執行後註記該日的該時段已經執行的紀錄
            _ImportEDocBiz.InsertRecord_ImportEDoc(timesection);
        }



        //公文欄位定義
        //public struct DocNode
        //{
        //    /// <summary>
        //    /// 發文機關全銜
        //    /// </summary>
        //    public string GovTitle;
        //    /// <summary>
        //    /// 機關代碼
        //    /// </summary>
        //    public string GovID;
        //    /// <summary>
        //    /// 函類別
        //    /// </summary>
        //    public string DocKind;
        //    /// <summary>
        //    /// 地址
        //    /// </summary>
        //    public string GovAddress;
        //    /// <summary>
        //    /// 聯絡方式
        //    /// </summary>
        //    public string ContactKind;
        //    /// <summary>
        //    /// 發文日期
        //    /// </summary>
        //    public string SendDate;
        //    /// <summary>
        //    /// 字
        //    /// </summary>
        //    public string SendWord;
        //    /// <summary>
        //    /// 年度
        //    /// </summary>
        //    public string SendYear;
        //    /// <summary>
        //    /// 流水號
        //    /// </summary>
        //    public string SendNo;
        //    /// <summary>
        //    /// 支號
        //    /// </summary>
        //    public string SendNo2;
        //    /// <summary>
        //    /// 速別
        //    /// </summary>
        //    public string Speed;
        //    /// <summary>
        //    /// 主旨文字
        //    /// </summary>
        //    public string Subject;
        //    /// <summary>
        //    /// 說明文字
        //    /// </summary>
        //    public string Description;
        //    /// <summary>
        //    /// 正本全銜
        //    /// </summary>
        //    public string ReceiveTitle;
        //    /// <summary>
        //    /// 副本全銜
        //    /// </summary>
        //    public string ReceiveCCTitle;

        //}






        /// <summary>
        /// di 資料結構
        /// </summary>
        private static DocNode fDocNode = new DocNode();
        /// <summary>
        /// Parser di xml data
        /// </summary>
        /// <param name="pfile"></param>
        /// <returns></returns>
        private static void ParseXML(string pfile)
        {
            DocNode mDocNode = new DocNode();
            try
            {
                fDocNode = new DocNode();

                XmlDocument xmlDom = new XmlDocument();
                xmlDom.XmlResolver = null; //不解析外部的 DTD、實體及結構描述
                xmlDom.Load(pfile);


                XmlNode node1; //node1取得 Root(根元素的所有 link節點);node2取得 link節點的所有子節點
                node1 = xmlDom.DocumentElement;


                GetField(node1);                
                xmlDom = null;
                node1 = null;
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
            catch (Exception ex)
            {
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "xml Parser Error:" + ex.Message);

            }
            finally
            {

            }
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
                else if (pDocNode.ParentNode.Name == "全銜")
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

    public class AutoPayTxtFields
    {
        public string fileName { get; set; }
        public DocNode parserDi { get; set; }
        public string[] fields2 { get; set; }
        public string[] fields3 { get; set; }
    }

    public class AutoPayResult
    {
        public Guid caseId { get; set; }
        public string fileName { get; set; }
        public bool ImportStatus { get; set; }
        public string Message { get; set; }
    }

    public struct DocNode
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
}
