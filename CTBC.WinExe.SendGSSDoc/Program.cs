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

namespace CTBC.WinExe.SendGSSDoc
{
    class Program
    {
        private static FileLog m_fileLog;
        private static string ftpserver;
        private static string gss2csfs;
        private static string port;
        private static string username;
        private static string password;
        private static string ftpdir;
        private static string loaclFilePath;
        private static FtpClient ftpClient;
        private static CaseHistoryBIZ _CaseHistoryBiz;

        static void Main(string[] args)
        {
            string ftp = ConfigurationManager.AppSettings["ftp"].ToString();
            port = ConfigurationManager.AppSettings["port"];
            ftpserver = ConfigurationManager.AppSettings["ftpserver"];
            username = ConfigurationManager.AppSettings["username"];
            password = ConfigurationManager.AppSettings["password"];
            ftpdir = ConfigurationManager.AppSettings["ftpdir"];
            //gss2csfs = ConfigurationManager.AppSettings["GSS2CSFS"];
            loaclFilePath = ConfigurationManager.AppSettings["localFilePath"];
            m_fileLog = new FileLog(ConfigurationManager.AppSettings["filelog"], AppDomain.CurrentDomain.FriendlyName.Replace(".vshost", "").Replace(".exe", ""));

            PARMCodeBIZ pbiz = new PARMCodeBIZ();


            var deciManagerInfo = pbiz.GetParmCodeByCodeType("deciManager").FirstOrDefault();
            LdapEmployeeBiz _ldap = new LdapEmployeeBiz();


            string msg = string.Format("開始處理======================");
            m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, msg);

            if (deciManagerInfo == null)
            {
                string msg1 = string.Format("\t\t Error 讀取不到決行主管的資訊, 請去參數檔設定CodeType=deciManager");
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, msg1);
                return;
            }


            bool isFTP = false;
            if (!string.IsNullOrEmpty(ftp))
            {
                isFTP = bool.Parse(ftp);
            }

            //1. 取出目前在GssDoc_Details中, 尚未回傳的案件
            List<GssDoc_Detail> gdocList = new List<GssDoc_Detail>();
            List<CaseMaster> allCaseMaster0 = new List<CaseMaster>();
            List<CaseMaster> allCaseMaster3 = new List<CaseMaster>();

            // 2. 找出gdocDetails中, 每個CaseID, 在CaseMaster.Status = 'Z01' (表示結案)
            //  sendstatus 0: 未回NULL , 1: 已回 2, 回文失敗 3, 重回 ,4 重回成功, 5重回失敗,

           var allUnSend = getUnsentDoc("1"); // 參數1表示, 取轉外來文的案件

            foreach (var gDoc in allUnSend) // 取出目前在GssDoc_Details中, 尚未回傳的案件, SendStatus=null, 0, 3
            {
                CaseMaster master = getCaseMaster((Guid)gDoc.CaseId);
                if (master != null)
                {
                    if (master.Status == "Z01" && (gDoc.SendStatus==null || gDoc.SendStatus == 0))
                    {
                        gdocList.Add(gDoc);
                        allCaseMaster0.Add(master);
                    }
                    if(master.Status == "Z01" && (gDoc.SendStatus == 3))
                    {
                        gdocList.Add(gDoc);
                        allCaseMaster3.Add(master);
                    }
                }
            }

            var allCaseMaster = allCaseMaster0.Union(allCaseMaster3);

            // 20191202, 若沒有案件, 則不應執行 124-423行的程式, 以免產出JSON及發Mail....
            bool isContinue = true;

            if( allCaseMaster.Count()>0)
            {
                msg = string.Format("\t目前共有{0}案件待回傳，目前已結案的案件為{1}", allUnSend.Count().ToString(),allCaseMaster.Count().ToString());
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, msg);
            }
            else
            {
                msg = string.Format("\t目前共有{0}案件待回傳，目前沒有結案的案件", allUnSend.Count().ToString());
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, msg);
                msg = string.Format("結束處理======================\r\n\r\n\r\n");
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, msg);
                //return; // 20190909, 不應馬上停止, 因為後面還有線上投單的要傳
                // 20191202, 若沒有案件, 則不應執行 124-423行的程式, 以免產出JSON及發Mail....
                isContinue = false;
            }

            
            var gssDocMail = pbiz.GetParmCodeByCodeType("GssDocNoticeMail").FirstOrDefault();
            string[] mailTo = gssDocMail.CodeMemo.Split(',');

            // 讀取若錯誤, 只寄給IT....
            string[] ErrorMailTo =  ConfigurationManager.AppSettings["ErrorMailTo"].ToString().Split(',');

            if (isContinue)
            {               

                // 找出未傳送文號, 在原來是幾個批次

                //var BatchNoGrouper = (from p in gdocList group p by p.BatchNo into g select g.Key).ToList();



                msg = string.Format("\t目前案件共{0}筆回傳", allCaseMaster.Count().ToString());
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, msg);

                Dictionary<string, string> outputJson = new Dictionary<string, string>();

                Dictionary<Guid, string> sendDocNos = new Dictionary<Guid, string>();

                //foreach (var batchNo in BatchNoGrouper) // 20191111, 不用分批
                {
                    DateTime thenow = DateTime.Now;
                    System.Threading.Thread.Sleep(2300);
                    string sendBatchNo = thenow.ToString("MMddHHmmss"); // 產出回傳的批號
                                                                        // 20190923, 要用民國年, 不可用西元年
                    int iYear1 = thenow.Year - 1911;
                    sendBatchNo = iYear1.ToString() + sendBatchNo;

                    //var gMaster = getGssDoc(batchNo); // 由detail取得原來gssDoc , 以備查
                    //var allDetails = gdocList.Where(x => x.BatchNo == batchno).ToList(); // 在同一個批號下, 目前未回傳的文號集合
                    // 20191111, 不需要分批.. 一次傳送
                    var allDetails = gdocList;

                    msg = string.Format("\t\t批號{0} ，共有{1}個文號:  {2}", sendBatchNo, allDetails.Count().ToString(), string.Join(",", allDetails.Select(x => x.DocNo)));
                    m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, msg);

                    #region 用SendDocBase, 開始產出Json檔
                    // 20191111, 讀取參數檔
                    var gssCompany2 = pbiz.GetParmCodeByCodeType("GssCompanyId").FirstOrDefault();
                    SendDocBase sdb = new SendDocBase();
                    sdb.CompanyId = gssCompany2.CodeNo; ;
                    sdb.TransferType = "1"; // 一般外來文, 是"1"
                    sdb.BatchNo = sendBatchNo;
                    sdb.BatchDate = thenow.ToString("yyyy-MM-dd HH:mm:ss.fff");
                    sdb.TotalNumber = allDetails.Count().ToString();

                    #region 開始填寫每個DocInfo

                    List<Docinfo> docinfoList = new List<Docinfo>();
                    foreach (var gdoc in allDetails)
                    {
                        var _caseMaster = allCaseMaster.Where(x => x.CaseId == gdoc.CaseId).FirstOrDefault();
                        if (_caseMaster != null)
                        {
                            try
                            {
                                #region 開始處理


                                msg = string.Format("\t\t開始處理案件號 {0} ", _caseMaster.CaseNo);
                                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, msg);
                                // 20190828, Patrick , 只有DeciDeptId等, 用approveUser, 其他人, 都用經辦人
                                // 用[LDAPEmployee].[DepDN] = ou=U00024989,ou=D00022499,ou=R00023452,ou=M00023321,ou=M00022311,ou=U00021934,ou=U00021933,ou=U00021932,ou=U00021931,ou=U00021800,ou=HRIS,o=CTCB
                                // 找出第一個M0000XXX, 即為該人的部門ID
                                // 再去[LDAPDepartment] 找出部門名稱
                                sendDocNos.Add(_caseMaster.CaseId, _caseMaster.CaseNo);


                                // 經辦人部門資訊, 20200520, 若發生原承辨人
                                var empInfo = _ldap.GetLdapEmployeeByEmpId(_caseMaster.AgentUser);
                                if (empInfo == null)
                                {
                                    noticeError_toIT(ErrorMailTo, _caseMaster.AgentUser);
                                    continue;
                                }
                                string agentDN = empInfo.DepDn;
                                string agentDepID = getDepID(agentDN);
                                var agentInfo = _ldap.GetLdapLDAPDepartmentByDepId(agentDepID);
                                string agentDepName = null;
                                if (agentInfo != null)
                                {
                                    agentDepName = agentInfo.Rows[0]["DepName"].ToString();
                                }

                                // 核準人部門資訊
                                var ApproveInfo = _ldap.GetLdapEmployeeByEmpId(_caseMaster.ApproveUser);
                                if (ApproveInfo == null)
                                {
                                    noticeError_toIT(ErrorMailTo, _caseMaster.ApproveUser);
                                    continue;
                                }
                                string deciDN = ApproveInfo.DepDn;
                                string deciDepID = getDepID(deciDN);
                                var deciInfo = _ldap.GetLdapLDAPDepartmentByDepId(deciDepID);
                                string deciDepName = null;
                                if (deciInfo != null)
                                {
                                    deciDepName = deciInfo.Rows[0]["DepName"].ToString();
                                }

                                Docinfo di = new Docinfo() { CnoNo = gdoc.DocNo, };
                                di.DeptId = agentDepID;  //20190828, 應是經辦人部門
                                di.DeptName = agentDepName;  //20190828, 應是經辦人部門
                                di.HdlUser = _caseMaster.AgentUser;
                                di.HdlUserName = _ldap.GetLdapEmployeeByEmpId(_caseMaster.AgentUser).EmpName;

                                di.DeciDeptId = deciDepID;  //20190828, 應是核準人部門(韻如)
                                di.DeciDeptName = deciDepName;   //20190828, 應是核準人部門(韻如)
                                // 20190924, 改成讀取參數檔
                                string[] dManagerInfo = deciManagerInfo.CodeNo.Split(',');
                                di.DeciUser = dManagerInfo[0];
                                di.DeciUserName = dManagerInfo[1];
                                di.DeciDate = ((DateTime)_caseMaster.ApproveDate).ToString("yyyy-MM-dd");

                                di.EndDeptId = agentDepID; //20190828, 應是經辦人部門
                                di.EndDeptName = agentDepName;  //20190828, 應是經辦人部門
                                di.EndUser = agentDepID; //20190828, 應是經辦人部門
                                di.EndUserName = _ldap.GetLdapEmployeeByEmpId(_caseMaster.AgentUser).EmpName;
                                di.EndDate = ((DateTime)_caseMaster.ApproveDate).ToString("yyyy-MM-dd");

                                docinfoList.Add(di);
                                gdoc.SendStatus = 1;
                                gdoc.SendMessage = "成功";
                                gdoc.Sendmetadata = JsonConvert.SerializeObject(di);
                                #endregion
                            }
                            catch( Exception ex)
                            {
                                msg = string.Format("\t\t\t開始處理案件號 {0}  異常, 發生異常: {1}", _caseMaster.CaseNo, ex.Message.ToString());
                                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, msg);
                            }
                        }
                        else
                        {
                            gdoc.SendStatus = 2;
                            gdoc.SendMessage = "異常, 找不到原CaseMaster";
                            msg = string.Format("\t\t開始處理案件號 {0}  異常, 找不到原CaseMaster", _caseMaster.CaseNo);
                            m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, msg);
                        }
                        gdoc.SendBatchNo = sendBatchNo;
                        gdoc.SendDate = thenow;
                    }
                    #endregion

                    sdb.DocInfo = docinfoList.ToArray();

                    #endregion

                    #region 產出Json檔, 先寫到LocalPath
                    string filename = string.Empty;
                    // 20190903, 怕重傳時, 多了A,B 會找不到....

                    filename += "A" + sendBatchNo + ".json";


                    //filename += sendBatchNo + ".json";

                    try
                    {
                        string sb = JsonConvert.SerializeObject(sdb);
                        if (!Directory.Exists(loaclFilePath))
                        {
                            Directory.CreateDirectory(loaclFilePath);
                        }

                        using (StreamWriter sw = new StreamWriter(loaclFilePath + "\\" + filename, false, Encoding.UTF8))
                        {
                            sw.WriteLine(sb);
                        }
                        outputJson.Add(filename, "成功");
                        msg = string.Format("\t\t批號{0} ，產出Json檔{1}", sendBatchNo, loaclFilePath + "\\" + filename);
                        m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, msg);
                    }
                    catch (Exception ex)
                    {
                        msg = string.Format("\t寫入Json檔失敗");
                        m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, msg);
                        Console.WriteLine("寫入Json檔失敗");
                        outputJson.Add(filename, "寫入Json檔失敗");
                    }


                    #endregion
                }






                #region 將OutputJson 寫入ftp 中
                if (isFTP)
                {
                    try
                    {
                        #region 連線FTP


                        // 上傳到FTP
                        msg = string.Format("\t開啟Ftp連線");
                        m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, msg);
                        ftpClient = new FtpClient(ftpserver, username, password, port);
                        // 取得 轉新外來文 的根目錄, 通常都是 yyyyMMddhhmmss

                        msg = string.Format("\t共計有{0}個Json檔", outputJson.Count.ToString());
                        m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, msg);
                        foreach (KeyValuePair<string, string> json in outputJson)
                        {
                            if (json.Value == "成功")
                            {
                                ftpClient.SendFile(ftpdir, loaclFilePath + "\\" + json.Key);
                            }
                        }
                        msg = string.Format("\t\t上傳json檔 : {0} ", string.Join(",", outputJson));
                        m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, msg);
                        #endregion
                    }
                    catch(Exception ex )
                    {
                        msg = string.Format("連線FTP中途發生錯誤!!");
                        m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, msg);
                    }
                    //fileNames = getFileList(); // 取得FTP上的檔案
                }
                #endregion

                //var failBatchNo = outputJson.Where(x => x.Value != "成功").Select(x=>x.Key.Substring(1).Replace(".json","")).ToList();


                try
                {

                    #region 並把gssDoc_Details 中的sendbatchno, sendstatus, sendmessage, senddata填回
                    using (GssEntities ctx = new GssEntities())
                    {
                        foreach (var gdoc in gdocList)
                        {
                            var updateRow = ctx.GssDoc_Detail.Where(x => x.id == gdoc.id).SingleOrDefault();
                            if (allCaseMaster3.Where(x => x.CaseId == (Guid)gdoc.CaseId).FirstOrDefault() != null)
                            {
                                if (updateRow != null)
                                {
                                    updateRow.SendBatchNo = gdoc.SendBatchNo;
                                    updateRow.SendDate = gdoc.SendDate;
                                    updateRow.SendStatus = 1;
                                    updateRow.SendMessage = "結案資訊重傳";
                                    updateRow.Sendmetadata = gdoc.Sendmetadata;
                                    //insertCaseHistory((Guid)updateRow.CaseId, "結案資訊重傳", "結案-重傳"); 

                                }

                            }
                            else
                            {


                                if (updateRow != null)
                                {
                                    updateRow.SendBatchNo = gdoc.SendBatchNo;
                                    updateRow.SendDate = gdoc.SendDate;
                                    updateRow.SendStatus = gdoc.SendStatus;
                                    updateRow.SendMessage = gdoc.SendMessage;
                                    updateRow.Sendmetadata = gdoc.Sendmetadata;
                                    insertCaseHistory((Guid)updateRow.CaseId, "結案資訊上傳", "結案-上傳");
                                }
                            }
                        }
                        ctx.SaveChanges();
                    }
                    #endregion
                }
                catch( Exception ex)
                {
                    msg = string.Format("填回 sendbatchno, sendstatus, sendmessage, senddata 狀態發生錯誤!!");
                    m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, msg);
                }




                #region 發出email通知


                msg = string.Format("\t準備發送外來文信件");
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, msg);



                //var allSendBatch = outputJson.Select(x => x.Key.Substring(1).Replace(".json", "")).ToList();

                if (gssDocMail != null)
                {
                    using (GssEntities ctx = new GssEntities())
                    {
                        foreach (var output in outputJson)
                        {
                            string batchNo = output.Key.Substring(1).Replace(".json", "");

                            var gssDocs = ctx.GssDoc_Detail.Where(x => x.SendBatchNo == batchNo).ToList();
                            var errDocs = gssDocs.Where(x => x.SendStatus == 2).ToList(); // 如果是2, 表示, 有這筆有錯

                            string sendBatchNo = gssDocs.First().SendBatchNo;
                            DateTime sendBatchDate = (DateTime)gssDocs.First().SendDate;
                            var bno = gssDocs.First().BatchNo;
                            var gssBatchNo = ctx.GssDoc.Where(x => x.BatchNo == bno).FirstOrDefault();
                            //if (gssBatchNo.TransferType == "1")
                            //    sendBatchNo = "A" + sendBatchNo;
                            //if (gssBatchNo.TransferType == "2")
                            //    sendBatchNo = "B" + sendBatchNo;

                            // 20190925, 新增原外來文的案件編號
                            List<string> sb = new List<string>();
                            foreach (var g in gssDocs)
                            {
                                var caseno = sendDocNos[(Guid)g.CaseId];
                                if (caseno != null)
                                    sb.Add(caseno);
                            }

                            try
                            {
                                if (errDocs.Count() > 0) // 表示其中有錯...
                                {
                                    msg = string.Format("\t\t準備發送外來文信件共有{0}件, 其中失敗{1}件", gssDocs.Count().ToString(), errDocs.Count().ToString());
                                    m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, msg);
                                    msg = string.Format("\t\t\t成功文件編號{0}", string.Join(",", gssDocs.Select(x => x.DocNo)));
                                    m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, msg);
                                    msg = string.Format("\t\t\t失敗文件編號{0}", string.Join(",", errDocs.Select(x => x.DocNo)));
                                    m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, msg);
                                    noticeMail_Fail(mailTo, gssBatchNo.CompanyID, sendBatchNo, sendBatchDate, gssDocs.Count().ToString(), string.Join(",", gssDocs.Select(x => x.DocNo)), string.Join(",", errDocs.Select(x => x.DocNo)), string.Join(",", sb));
                                }
                                else // 全對
                                {
                                    msg = string.Format("\t\t準備發送外來文信件共有{0}件, 全部成功", gssDocs.Count().ToString());
                                    m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, msg);
                                    msg = string.Format("\t\t\t成功文件編號{0}", string.Join(",", gssDocs.Select(x => x.DocNo)));
                                    m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, msg);
                                    noticeMail_Success(mailTo, gssBatchNo.CompanyID, sendBatchNo, sendBatchDate, gssDocs.Count().ToString(), string.Join(",", gssDocs.Select(x => x.DocNo)), string.Join(",", sb));
                                }
                            }
                            catch (Exception ex)
                            {
                                msg = string.Format("\t\t 發生錯誤{0}", ex.Message.ToString());
                                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, msg);
                            }
                        }
                    }
                }

                msg = string.Format("\t結束發送外來文信件=========================\r\n\r\n");
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, msg);

                #endregion

            }

            // ========================================================================================
            // 20190908, 第二部分...........線上投單... 
            // 去找[CaseCustQuery]中的FileStatus = 'Y' and CloseDate= 今日的..

            Dictionary<string, string> outputJsonOnline = new Dictionary<string, string>();

            msg = string.Format("線上投單開始處理======================\r\n\r\n\r\n");
            m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, msg);
            //DateTime thedate = DateTime.Today;


            //  sendstatus 0: 未回NULL , 1: 已回 2, 回文失敗 3, 重回 ,4 重回成功, 5重回失敗,
            // 20191002, 也要分為第一次回傳跟重回...
            var allUnSend2 = getUnsentDoc("2"); // 參數2表示, 取 轉線上投單的案件            

            //DateTime tomorrow = thedate.AddDays(1);
            List<CaseCustMaster> result = new List<CaseCustMaster>(); // 未回的
            List<CaseCustMaster> result3 = new List<CaseCustMaster>(); // 重回的



            try
            {
                #region 讀取目前未傳送/重回的的CaseCustQuery, 讀到Result



                using (GssEntities ctx = new GssEntities())
                {
                    foreach (var unSend in allUnSend2)
                    {
                        var r = ctx.CaseCustMaster.Where(x => x.FileStatus == "Y" && x.NewID == unSend.CaseId).FirstOrDefault();
                        if (r != null)
                        {
                            if (unSend.SendStatus == 3)
                            {
                                result3.Add(r);
                            }
                            if (unSend.SendStatus == 0 || unSend.SendStatus == null)
                            {
                                result.Add(r);
                            }
                        }
                        else
                        {
                            msg = string.Format("線上投單CaseCustQuery 表, 找不到{0}案件編號", unSend.DocNo.ToString());
                            m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, msg);
                        }

                    }
                    //result = ctx.CaseCustQuery.Where(x => x.FileStatus == "Y" && x.CloseDate >= thedate && x.CloseDate < tomorrow).ToList();
                }


                result.AddRange(result3); // 將重回的案件與未回的合併, 都要一起發送

                #endregion
            }
            catch (Exception ex)
            {
                msg = string.Format("讀取目前未傳送/重回的的CaseCustQuery, 讀到Result發生錯誤!!{0}" , ex.Message.ToString());
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, msg);
            }

            msg = string.Format("\t讀取共{0}個案件", result.Count().ToString());
            m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, msg);

            
            var gssCompanyID = pbiz.GetParmCodeByCodeType("GssCompanyId").FirstOrDefault();


            #region 用SendDocBase, 開始產出Json檔
            DateTime thenow1 = DateTime.Now;
            System.Threading.Thread.Sleep(2300);
            string sendBatchNo1 = thenow1.ToString("MMddHHmmss"); // 產出回傳的批號

            // 20190923, 要用民國年, 不可用西元年
            int iYear = thenow1.Year - 1911;
            sendBatchNo1 = iYear.ToString() + sendBatchNo1;

            SendDocBase sdb1 = new SendDocBase();
            sdb1.CompanyId = gssCompanyID.CodeNo;
            sdb1.TransferType = "2";
            sdb1.BatchNo = sendBatchNo1;
            sdb1.BatchDate = thenow1.ToString("yyyy-MM-dd HH:mm:ss.fff");
            

            List<Docinfo> docinfoList1 = new List<Docinfo>();
            Dictionary<Guid, string> ftplst = new Dictionary<Guid, string>();
            Dictionary<Guid, string> ftplstError = new Dictionary<Guid, string>();
            using (GssEntities ctx = new GssEntities())
            {
                foreach (var gdoc in result)
                {
                    msg = string.Format("\t\t開始進行 {0} 案件產出", gdoc.DocNo);
                    m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, msg);
                    //  20190911, 要過濾是否在GssDoc_Detail中, 已寄出
                    ////  sendstatus 0: 未回NULL , 1: 已回 2, 回文失敗 3, 重回 ,4 重回成功, 5重回失敗,
                    var gDetail = ctx.GssDoc_Detail.Where(x => x.CaseId == gdoc.NewID).FirstOrDefault();
                    if( gDetail!=null )
                    {
                        if (gDetail.SendStatus == 1)  // 表示已傳送, 不需要再送一次了...
                            continue;
                    }
                    else
                    {
                        msg = string.Format("\t\t案號{0} ，在GssDoc_Detail中找不到", gdoc.NewID);
                        m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, msg);
                        continue;
                    }

                    try
                    {
                        #region 開始填寫每個DocInfo

                        // 20190828, Patrick , 只有DeciDeptId等, 用approveUser, 其他人, 都用經辦人
                        // 用[LDAPEmployee].[DepDN] = ou=U00024989,ou=D00022499,ou=R00023452,ou=M00023321,ou=M00022311,ou=U00021934,ou=U00021933,ou=U00021932,ou=U00021931,ou=U00021800,ou=HRIS,o=CTCB
                        // 找出第一個M0000XXX, 即為該人的部門ID
                        // 再去[LDAPDepartment] 找出部門名稱

                        // 經辦人部門資訊
                        string agentDN = _ldap.GetLdapEmployeeByEmpId(gdoc.QueryUserID).DepDn;
                        string agentDepID = getDepID(agentDN);
                        var agentInfo = _ldap.GetLdapLDAPDepartmentByDepId(agentDepID);
                        string agentDepName = null;
                        if (agentInfo != null)
                        {
                            agentDepName = agentInfo.Rows[0]["DepName"].ToString();
                        }


                        // 核準人部門資訊
                        // 20191016, 因為gdoc.CearanceUserID 可能是空白, 所以要算錯誤件
                        string deciDN = _ldap.GetLdapEmployeeByEmpId(gdoc.CearanceUserID).DepDn;
                        string deciDepID = getDepID(deciDN);
                        var deciInfo = _ldap.GetLdapLDAPDepartmentByDepId(deciDepID);
                        string deciDepName = null;
                        if (deciInfo != null)
                        {
                            deciDepName = deciInfo.Rows[0]["DepName"].ToString();
                        }
                        // 20191007, 應該讀取GssDoc_Detail.DocNo 那個欄位才對....
                        // Docinfo di = new Docinfo() { CnoNo = gdoc.DocNo, }; 錯的... 
                        Docinfo di = new Docinfo() { CnoNo = gDetail.DocNo };
                        di.DeptId = agentDepID;  //20190828, 應是經辦人部門
                        di.DeptName = agentDepName;  //20190828, 應是經辦人部門
                        di.HdlUser = gdoc.QueryUserID;
                        di.HdlUserName = _ldap.GetLdapEmployeeByEmpId(gdoc.QueryUserID).EmpName;

                        di.DeciDeptId = deciDepID;  //20190828, 應是核準人部門(韻如)
                        di.DeciDeptName = deciDepName;   //20190828, 應是核準人部門(韻如)
                                                         // 20190924, 改成讀取參數檔
                        string[] dManagerInfo = deciManagerInfo.CodeNo.Split(',');
                        di.DeciUser = dManagerInfo[0];
                        di.DeciUserName = dManagerInfo[1];
                        di.DeciDate = ((DateTime)gdoc.CloseDate).ToString("yyyy-MM-dd");

                        di.EndDeptId = agentDepID; //20190828, 應是經辦人部門
                        di.EndDeptName = agentDepName;  //20190828, 應是經辦人部門
                        di.EndUser = agentDepID; //20190828, 應是經辦人部門
                        di.EndUserName = _ldap.GetLdapEmployeeByEmpId(gdoc.QueryUserID).EmpName;
                        di.EndDate = ((DateTime)gdoc.CloseDate).ToString("yyyy-MM-dd");

                        docinfoList1.Add(di);

                        // 20190911並且還要寄出去後, 要修改SendStatus... 等欄位, 以免下次執行, 會再去寄一次...
                        //要用result.NewsID 跟 allUnsend2.CaseID 對映.. 找出那些案件, 已經寄過了, 不能再寄一次... 

                        if (gDetail != null)
                        {
                            var aaa = result3.Where(x => x.NewID == gdoc.NewID).FirstOrDefault();
                            if (aaa == null)
                            {
                                gDetail.SendStatus = 1;
                                gDetail.SendMessage = "結案資訊上傳";
                                gDetail.Sendmetadata = JsonConvert.SerializeObject(di);
                                gDetail.SendBatchNo = sendBatchNo1;
                                gDetail.SendDate = thenow1;
                            }
                            else // 表示是重回的線上投單
                            {
                                gDetail.SendStatus = 1;
                                gDetail.SendMessage = "結案資訊重傳";
                                gDetail.Sendmetadata = JsonConvert.SerializeObject(di);
                                gDetail.SendBatchNo = sendBatchNo1;
                                gDetail.SendDate = thenow1;
                            }
                        }
                        #endregion

                        ftplst.Add(gdoc.NewID, gDetail.DocNo); // 寫到陣列, 以備後面FTP用...
                    }
                    catch (Exception ex)
                    {
                        #region 
                        gDetail.SendStatus = 2;
                        gDetail.SendMessage = "失敗";
                        gDetail.Sendmetadata = ex.Message.ToString();
                        gDetail.SendBatchNo = sendBatchNo1;
                        gDetail.SendDate = thenow1;
                        ctx.SaveChanges();
                        ftplstError.Add(gdoc.NewID, gDetail.DocNo);// 寫到陣列, 以備後面FTP用...
                        msg = string.Format("\t\t產出案號{0} ，發生不明的錯誤 {1}", gdoc.DocNo, ex.Message.ToString());
                        m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, msg);
                        #endregion
                    }
                }
                ctx.SaveChanges();
            }

            sdb1.TotalNumber = ftplst.Count().ToString();
            sdb1.DocInfo = docinfoList1.ToArray();

            #endregion

            #region 產出Json檔, 先寫到LocalPath, 寫入outputJsonOnline

            // 20190911, 如果docinfoList1.count()==0, 代表沒有新的要寄送, 則不要產出Json檔,
            if (docinfoList1.Count() > 0)
            {

                string filename1 = string.Empty;

                filename1 = "B" + sendBatchNo1 + ".json";

                try
                {
                    string sb = JsonConvert.SerializeObject(sdb1);
                    if (!Directory.Exists(loaclFilePath))
                    {
                        Directory.CreateDirectory(loaclFilePath);
                    }

                    using (StreamWriter sw = new StreamWriter(loaclFilePath + "\\" + filename1, false, Encoding.UTF8))
                    {
                        sw.WriteLine(sb);
                    }
                    outputJsonOnline.Add(filename1, "成功");
                    msg = string.Format("\t\t批號{0} ，產出Json檔{1}", sendBatchNo1, loaclFilePath + "\\" + filename1);
                    m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, msg);
                }
                catch (Exception ex)
                {
                    msg = string.Format("\t寫入Json檔失敗");
                    m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, msg);
                    Console.WriteLine("寫入Json檔失敗");
                    outputJsonOnline.Add(filename1, "寫入Json檔失敗");
                }

                msg = string.Format("\t共產生{0}個Json檔", outputJsonOnline.Count().ToString());
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, msg);
                msg = string.Format("\t\t批號{0}", string.Join(",", outputJsonOnline.Select(x => x.Key)));
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, msg);
            }
            else
            {
                msg = string.Format("\t過濾已寄送案號，沒有需要寄送案號!!");
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, msg);
            }

            #endregion


            // 線上投單, 只會產出一個批號, 不是成功, 就是失敗

            #region outputJsonOnline 寫入ftp 中
            if (isFTP)
            {

                try
                {
                    // 上傳到FTP
                    msg = string.Format("\t開啟Ftp連線");
                    m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, msg);
                    ftpClient = new FtpClient(ftpserver, username, password, port);
                    // 取得 轉新外來文 的根目錄, 通常都是 yyyyMMddhhmmss

                    msg = string.Format("\t共計有{0}個Json檔", outputJsonOnline.Count.ToString());
                    m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, msg);


                    if (outputJsonOnline.Count() > 0)
                    {
                        try
                        {
                            KeyValuePair<string, string> json = outputJsonOnline.First();

                            if (json.Value == "成功")
                            {
                                ftpClient.SendFile(ftpdir, loaclFilePath + "\\" + json.Key);
                                msg = string.Format("\t\t上傳json檔 : {0} ", string.Join(",", outputJsonOnline));
                                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, msg);
                            }
                        }
                        catch (Exception ex)
                        {
                            msg = string.Format("\t\t上傳json檔失敗 : {0} ", string.Join(",", outputJsonOnline));
                            m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, msg);
                        }
                    }
                }
                catch (Exception ex)
                {
                    msg = string.Format("寫入ftp  發生錯誤!!{0}", ex.Message.ToString());
                    m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, msg);
                }




                //fileNames = getFileList(); // 取得FTP上的檔案
            }
            #endregion


            #region 發出email通知

            msg = string.Format("\t準備發送線上投單信件");
            m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, msg);


            if (gssDocMail != null && outputJsonOnline.Count()>0)
            {
                msg = string.Format("\t\t進入 if 條件 準備發送線上投單信件");
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, msg);
                var output = outputJsonOnline.First();

                // 找出原來來文的公文文號, 放進SB陣列中
                string sb = string.Join(",", ftplst.Select(x => x.Value));
                string sbError = string.Join(",", ftplstError.Select(x => x.Value));
                try 
				{
					if (output.Value=="成功")
					{
                        // 20191016, 要分全部成功, 或部分成功, 全部失敗
                        if (ftplstError.Count() == 0 && ftplst.Count() > 0) // 表示全部成功
                        {
                            msg = string.Format("\t\t確認產出Json檔成功!!, 並且全部成功");
                            m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, msg);
                            //string batchNo = output.Key.Substring(1).Replace(".json", "");
                            noticeMail_Success1(mailTo, sdb1.CompanyId, sendBatchNo1, thenow1, result.Count().ToString(), sb, string.Join(",", result.Select(x => x.DocNo)));
                            msg = string.Format("\t\t已寄出 ", string.Join(",", outputJsonOnline.Select(x => x.Key)));
                            m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, msg);
                        }
                        if(ftplstError.Count() > 0 && ftplst.Count() > 0) // 表示部分成功, 部分失敗
                        {
                            msg = string.Format("\t\t確認產出Json檔成功!!, 有部分成功, 部分失敗");
                            m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, msg);
                            //string batchNo = output.Key.Substring(1).Replace(".json", "");
                             noticeMail_Fail1(mailTo, sdb1.CompanyId, sendBatchNo1, thenow1, result.Count().ToString(), ftplst, ftplstError,  result);
                            msg = string.Format("\t\t已寄出 ", string.Join(",", outputJsonOnline.Select(x => x.Key)));
                            m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, msg);
                        }
                        if (ftplstError.Count() > 0 && ftplst.Count() == 0) // 表示全部失敗
                        {
                            msg = string.Format("\t\t確認產出Json檔成功!!, 並且全部失敗");
                            m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, msg);
                            noticeMail_Fail_All1(mailTo, sdb1.CompanyId, sendBatchNo1, thenow1, result.Count().ToString(), sbError, string.Join(",", result.Select(x => x.DocNo)));
                            msg = string.Format("\t\t產出Json或上傳失敗 ", string.Join(",", outputJsonOnline.Select(x => x.Key)));
                            m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, msg);
                        }

                    }
					else
					{
						msg = string.Format("\t\t確認產出Json檔失敗!!");
						m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, msg);
						noticeMail_Fail_All1(mailTo, sdb1.CompanyId, sendBatchNo1, thenow1, result.Count().ToString(), sbError, string.Join(",", result.Select(x => x.DocNo)));
						msg = string.Format("\t\t產出Json或上傳失敗 ", string.Join(",", outputJsonOnline.Select(x => x.Key)));
						m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, msg);

					}
				}
				catch( Exception ex)
				{
					msg = string.Format("\t\t 發生錯誤{0}", ex.Message.ToString());
					m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, msg);
				}
            }
            msg = string.Format("\t結束發送線上投單信件=========================\r\n\r\n");
            m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, msg);

            #endregion


            msg = string.Format("結束處理======================\r\n\r\n\r\n");
            m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, msg);

        }

        private static void noticeError_toIT(string [] mailFromTo, string empID)
        {
            string mailFrom = ConfigurationManager.AppSettings["mailFrom"];
            
            string subject = string.Format(" ****Error 找不到承辨人的資訊-{0}", empID);

            string body = "找不到承辨人的資訊 {0}\r\n";
            body += "本信件由系統直接發送!請勿直接回覆，謝謝!\r\n";
            body = string.Format(body, empID);
            string host = ConfigurationManager.AppSettings["mailHost"];
            UtlMail.SendEmail(mailFrom, mailFromTo, subject, body, host);
        }

        private static void noticeMail_Success1(string[] mailFromTo, string companyid, string batchno, DateTime batchdate, string totNum, string docNos, string CSFSDocNos)
        {
            string mailFrom = ConfigurationManager.AppSettings["mailFrom"];
            //string[] mailFromTo = ConfigurationManager.AppSettings["mailTo"].ToString().Split(',');
            //   批號+結案成功XX筆-外來文C
            string subject = string.Format("{0} 結案成功 {1} 筆- 外來文C", batchno, totNum);   

            string body = "公司代碼： {0}\r\n";
            body += "批次批號： {1}\r\n";
            body += "批次執行日期時間： {2}\r\n";
            body += "轉換類型： 線上投單\r\n";
            body += "介接文號總數： {3}\r\n";
            body += "公文文號清單： {4}\r\n";
            body += "線上投單案件編號： {5}\r\n";
            body += "批次執行成功!\r\n\r\n\r\n";
            body += "本信件由系統直接發送!請勿直接回覆，謝謝!\r\n";
            body = string.Format(body, companyid, batchno, batchdate.ToLongDateString(), totNum, docNos, CSFSDocNos);
            string host = ConfigurationManager.AppSettings["mailHost"];
            UtlMail.SendEmail(mailFrom, mailFromTo, subject, body, host);
        }

        private static void noticeMail_Fail1(string[] mailFromTo, string companyid, string batchno, DateTime batchdate, string totNum, Dictionary<Guid,string> success, Dictionary<Guid, string> fail, List<CaseCustMaster> Result)
        {
            string mailFrom = ConfigurationManager.AppSettings["mailFrom"];
            //string[] mailFromTo = ConfigurationManager.AppSettings["mailTo"].ToString().Split(',');
            //   批號+結案成功XX筆-外來文C
            //string subject = string.Format("{0} 產檔失敗 {1} 筆- 外來文C", batchno, totNum);
            string subject = string.Format("{0} 結案成功 {1} 筆/失敗 {2} 筆-外來文C", batchno, success.Count().ToString(), fail.Count().ToString());

            string sb = string.Join(",", success.Select(x => x.Value));
            string sbError = string.Join(",", fail.Select(x => x.Value));
            List<string> lstSuccess = new List<string>();
            foreach( var s in success)
            {
                var a = Result.Where(x => x.NewID == s.Key).FirstOrDefault();
                if (a != null)
                    lstSuccess.Add(a.DocNo);
            }
            List<string> lstFail = new List<string>();
            foreach (var s in fail)
            {
                var a = Result.Where(x => x.NewID == s.Key).FirstOrDefault();
                if (a != null)
                    lstFail.Add(a.DocNo);
            }
            string CSFSDocSuccess = string.Join(",", lstSuccess);
            string CSFSDocFail = string.Join(",", lstFail);

            string body = "公司代碼： {0}\r\n";
            body += "批次批號： {1}\r\n";
            body += "批次執行日期時間： {2}\r\n";
            body += "轉換類型： 線上投單\r\n";
            body += "介接文號總數： {3}\r\n";
            body += "成功的公文文號清單： {4}\r\n";
            body += "成功的線上投單案件編號： {5}\r\n";
            body += "失敗的公文文號清單： {6}\r\n";
            body += "失敗的線上投單案件編號： {7}\r\n";
            body += "批次執行部分成功/ 部分失敗!\r\n\r\n\r\n";
            body += "本信件由系統直接發送!請勿直接回覆，謝謝!\r\n";
            body = string.Format(body, companyid, batchno, batchdate.ToLongDateString(), totNum, sb, CSFSDocSuccess, sbError, CSFSDocFail);
            string host = ConfigurationManager.AppSettings["mailHost"];
            UtlMail.SendEmail(mailFrom, mailFromTo, subject, body, host);
        }


        private static void noticeMail_Fail_All1(string[] mailFromTo, string companyid, string batchno, DateTime batchdate, string totNum, string docNos, string CSFSDocNos)
        {
            string mailFrom = ConfigurationManager.AppSettings["mailFrom"];
            //string[] mailFromTo = ConfigurationManager.AppSettings["mailTo"].ToString().Split(',');
            //   批號+結案成功XX筆-外來文C
            string subject = string.Format("{0} 產檔失敗 {1} 筆- 外來文C", batchno, totNum);

            string body = "公司代碼： {0}\r\n";
            body += "批次批號： {1}\r\n";
            body += "批次執行日期時間： {2}\r\n";
            body += "轉換類型： 線上投單\r\n";
            body += "介接文號總數： {3}\r\n";
            body += "失敗的公文文號清單： {4}\r\n";            
            body += "失敗的線上投單案件編號： {5} \r\n";
            body += "批次執行失敗!\r\n\r\n\r\n";
            body += "本信件由系統直接發送!請勿直接回覆，謝謝!\r\n";
            body = string.Format(body, companyid, batchno, batchdate.ToLongDateString(), totNum, docNos, CSFSDocNos);
            string host = ConfigurationManager.AppSettings["mailHost"];
            UtlMail.SendEmail(mailFrom, mailFromTo, subject, body, host);
        }

        private static void insertCaseHistory(Guid caseId, string strEvent, string strToFolder)
        {
            _CaseHistoryBiz = new CaseHistoryBIZ();
            #region 插入一筆CaseHistory
            CaseHistory ch = new CaseHistory()
            {
                CaseId = caseId,
                FromRole = "自動化處理",
                FromFolder = "主管-放行結案",
                FromUser = "Sys",
                Event = strEvent, 
                EventTime = DateTime.Now,
                ToRole = "自動化處理",
                ToUser = "Sys",
                ToFolder = strToFolder,
                CreatedUser = "Sys",
                CreatedDate = DateTime.Now
            };
            _CaseHistoryBiz.insertCaseHistory(ch);
            
            #endregion
        }

        private static string getDepID(string agentDN)
        {
            // ou=U00024989,ou=D00022499,ou=R00023452,ou=M00023321,ou=M00022311,ou=U00021934,ou=U00021933,ou=U00021932,ou=U00021931,ou=U00021800,ou=HRIS,o=CTCB
            // 找出第一個ou=M000XXX開頭的.. 即可DepID
            string DepID = null;
            string[] depInfo = agentDN.Split(',');
            foreach(var s in depInfo)
            {
                if( s.Trim().StartsWith("ou=M"))
                {
                    DepID = s.Trim().Replace("ou=", "");
                    break;
                }
            }
            return DepID;
        }

        private static CaseMaster getCaseMaster(Guid caseId)
        {
            CaseMaster result = new CaseMaster();
            using (GssEntities ctx = new GssEntities())
            { 
                    result = ctx.CaseMaster.Where(x => x.CaseId == caseId).FirstOrDefault();
            }
            return result;
        }

        private static GssDoc getGssDoc(string batchNo)
        {

            GssDoc result = new GssDoc();
            using (GssEntities ctx = new GssEntities())
            {
                result = ctx.GssDoc.Where(x => x.BatchNo == batchNo).FirstOrDefault();
            }
            return result;
        }

        

        private static List<GssDoc_Detail> getUnsentDoc(string TransferType)
        {

            //  sendstatus 0: 未回NULL , 1: 已回 2, 回文失敗 3, 重回 ,4 重回成功, 5重回失敗,
            List<GssDoc_Detail> result = new List<GssDoc_Detail>();
            List<GssDoc> gdoc = new List<GssDoc>();
            using (GssEntities ctx = new GssEntities())
            {
                result = ctx.GssDoc_Detail.Where(x => x.SendStatus == null || x.SendStatus == 0 || x.SendStatus == 3).ToList();
                var batchnoList = result.Select(x => x.BatchNo).Distinct().ToList();
                gdoc = ctx.GssDoc.Where(x => batchnoList.Contains(x.BatchNo)).ToList();
            }

            List<GssDoc_Detail> result1 = new List<GssDoc_Detail>();
            // 20190911, 要過濾TransferType
            foreach(var r in result)
            {
                var gd = gdoc.Where(x => x.BatchNo == r.BatchNo).FirstOrDefault();
                if( gd!=null)
                {
                    if (gd.TransferType.Trim() == TransferType)
                    {
                        result1.Add(r);
                    }
                }
            }
            return result1;
        }

        private static void noticeMail_Success(string[] mailFromTo, string companyid, string batchno, DateTime batchdate, string totNum, string docNos, string CSFSDocNos)
        {
            string mailFrom = ConfigurationManager.AppSettings["mailFrom"];
            //string[] mailFromTo = ConfigurationManager.AppSettings["mailTo"].ToString().Split(',');
            //   批號+結案成功XX筆-外來文C
            string subject = string.Format("{0} 結案成功 {1} 筆-外來文C", batchno, totNum);   //"e化公文管理系統-集作拋轉公文排程執行完畢";
            //string body = "外來文系統 RACF 登入錯誤，錯誤原因：" + InitMessage;
//            string body = @"公司代碼: {0}\r\n
//批次批號: {1}\r\n
//批次執行日期時間: {2}\r\n
//轉換類型: 轉新外來文\r\n
//介接文號總數: {3}\r\n
//公文文號清單: {4}\r\n
//批次執行成功!


//本信件由系統直接發送!請勿直接回覆，謝謝!\r\n";

            string body = "公司代碼： {0}\r\n";
            body += "批次批號： {1}\r\n";
            body += "批次執行日期時間： {2}\r\n";
            body += "轉換類型： 轉新外來文\r\n";
            body += "介接文號總數： {3}\r\n";
            body += "公文文號清單： {4}\r\n";
            body += "外來文案件編號： {5}\r\n";
            body += "批次執行成功!\r\n\r\n\r\n";
            body += "本信件由系統直接發送!請勿直接回覆，謝謝!\r\n";
            body = string.Format(body, companyid, batchno, batchdate.ToLongDateString(), totNum, docNos, CSFSDocNos);
            string host = ConfigurationManager.AppSettings["mailHost"];
            UtlMail.SendEmail(mailFrom, mailFromTo, subject, body, host);
        }

        private static void noticeMail_Fail(string[] mailFromTo, string companyid, string batchno, DateTime batchdate, string totNum, string docNos, string faildocs, string CSFSDocNos)
        {
            string mailFrom = ConfigurationManager.AppSettings["mailFrom"];
            //string[] mailFromTo = ConfigurationManager.AppSettings["mailTo"].ToString().Split(',');
            // 批號+結案成功XX筆/失敗xx筆-外來文C
            string[] fdocs = faildocs.Split(',');
            int TotalSuccess = int.Parse(totNum) - fdocs.Length;
            string subject = string.Format("{0} 結案成功 {1} 筆/失敗 {2} 筆-外來文C", batchno, TotalSuccess.ToString(), fdocs.Length.ToString());
            //string body = "外來文系統 RACF 登入錯誤，錯誤原因：" + InitMessage;
//            string body = @"公司代碼: {0}\r\n
//批次批號: {1}\r\n
//批次執行日期時間: {2}\r\n
//轉換類型: 轉新外來文\r\n
//介接文號總數: {3}\r\n
//公文文號清單: {4}\r\n
//批次執行失敗!

//失敗公文文號清單及訊息: \r\n
//{5}
//本信件由系統直接發送!請勿直接回覆，謝謝!\r\n";
            string body = "公司代碼： {0}\r\n";
            body += "批次批號： {1}\r\n";
            body += "批次執行日期時間： {2}\r\n";
            body += "轉換類型： 轉新外來文\r\n";
            body += "介接文號總數： {3}\r\n";
            body += "公文文號清單： {4}\r\n";
            body += "外來文案件編號： {6}\r\n";
            body += "批次執行失敗!\r\n\r\n";
            body += "失敗公文文號清單及訊息：\r\n";
            body += "{5}\r\n\r\n\r\n";
            body += "本信件由系統直接發送!請勿直接回覆，謝謝!\r\n";

            body = string.Format(body, companyid, batchno, batchdate.ToLongDateString(), totNum, docNos, faildocs, CSFSDocNos);
            string host = ConfigurationManager.AppSettings["mailHost"];
            UtlMail.SendEmail(mailFrom, mailFromTo, subject, body, host);
        }

        private static void noticeMail_Fail_All(string[] mailFromTo, string companyid, string batchno, DateTime batchdate, string totNum, string docNos, string faildocs)
        {
            string mailFrom = ConfigurationManager.AppSettings["mailFrom"];
            //string[] mailFromTo = ConfigurationManager.AppSettings["mailTo"].ToString().Split(',');
            // 批號+結案失敗-外來C
            string[] fdocs = faildocs.Split(',');
            string subject = string.Format("{0} 結案失敗-外來文C", batchno, totNum, fdocs.Length.ToString());
            //string body = "外來文系統 RACF 登入錯誤，錯誤原因：" + InitMessage;
            //            string body = @"公司代碼: {0}\r\n
            //批次批號: {1}\r\n
            //批次執行日期時間: {2}\r\n
            //轉換類型: 轉新外來文\r\n
            //介接文號總數: {3}\r\n
            //公文文號清單: {4}\r\n
            //批次執行失敗!

            //失敗公文文號清單及訊息: \r\n
            //{5}
            //本信件由系統直接發送!請勿直接回覆，謝謝!\r\n";

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
            body = string.Format(body, companyid, batchno, batchdate.ToLongDateString(), totNum, docNos, faildocs);
            string host = ConfigurationManager.AppSettings["mailHost"];
            UtlMail.SendEmail(mailFrom, mailFromTo, subject, body, host);
        }

    }



}
