using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CTBC.FrameWork.Util;
using System.Configuration;
using log4net;
using System.IO;
using CTBC.CSFS.BussinessLogic;
using CTBC.CSFS.Models;
using CTBC.CSFS.Pattern;
using System.Data;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Collections;

namespace CTBC.WinExe.SendRMEmail
{
    class Program
    {
        static ILog log = LogManager.GetLogger("loginfo");
        static void Main(string[] args)
        {


            log4net.Config.XmlConfigurator.Configure(new System.IO.FileInfo(AppDomain.CurrentDomain.BaseDirectory + @"\Log.config"));
            WriteLog("開始發RM 相關eMail含附件");
            LdapEmployeeBiz leb = new LdapEmployeeBiz();
            //DateTime theday =  DateTime.Now;
            DateTime todayit = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd"));
            DateTime yestodayit = todayit.AddDays(-1);
            //DateTime theday = new DateTime(2018, 4, 28);
            using(CSFS1Entities ctx = new CSFS1Entities())
            {

                // 20180905, 改成日曆日就送, 不管營業日了...

                // 20180803 , 若今天不是WorkingDay , 則不執行...
                //var workday1 = ctx.PARMWorkingDay.Where(x => x.Date == todayit && x.Flag).FirstOrDefault();
                //if (workday1 == null)
                //{
                //    WriteLog("今日非工作日, 所以不發Email");
                //    return;
                //}

                // 20180906, 先刪除昨天以前的PDF檔
                var sDir1 = AppDomain.CurrentDomain.BaseDirectory;
                if (!sDir1.EndsWith("\\"))
                    sDir1 += "\\";

                sDir1 += @"PDF\";
                if (Directory.Exists(sDir1))
                {
                    DirectoryInfo di = new DirectoryInfo(sDir1);
                    foreach(var f in di.GetFiles("*.pdf").Where(x=>x.CreationTime < yestodayit))
                    {
                        File.Delete(f.FullName);
                        System.Threading.Thread.Sleep(100);
                    }

                }



                //  20180905, 改成日曆日就送, 不管營業日了...
                //找出CaseMaster中, 昨天, 主管放行的案例
                // 取得上一個工作日 
                //var workday = ctx.PARMWorkingDay.Where(x => x.Date < todayit && x.Flag).OrderByDescending(x => x.Date).FirstOrDefault();
                //if( workday!=null)
                {

                    var allCaseClose = ctx.CaseMaster.Where(x => x.AgentSubmitDate >= yestodayit && x.AgentSubmitDate < todayit ).ToList();

                    //allCaseClose = ctx.CaseMaster.Where(x => x.CaseNo == "A107110200001").ToList();

                    WriteLog(string.Format("今日準備要寄的案件號碼{0}  ", string.Join(",", allCaseClose.Select(x=>x.CaseNo) )));
                    foreach(var cc in allCaseClose)
                    {

                        WriteLog(string.Format("案件號碼{0} / {1} ",cc.CaseNo, cc.CaseId));
                        // 找出理專及其email
                        var allrmInfoNOTDist = ctx.TX_60491_Grp.Where(x => x.CaseId == cc.CaseId && ! string.IsNullOrEmpty(x.VipCdI) && !string.IsNullOrEmpty(x.FbTeller)).ToList();

                        var allrmInfo = (from p in allrmInfoNOTDist group p by new { p.CaseId, p.VipCdI } into g select g.First()).ToList();
                        ArrayList eMailList = new ArrayList();
                        ArrayList eMailList2 = new ArrayList();
                        if( allrmInfo.Count()==0)
                        {
                            WriteLog(string.Format("\t\t本案件無等級資訊(VipCdI) 或 無理專資訊 ", cc.CaseNo, cc.CaseId));
                            continue;
                        }


                        foreach (var rmInfo in allrmInfo)
                        {
                            try
                            {
                                #region Step 2
                                if (rmInfo != null)
                                {
                                    string branchid = rmInfo.FbAoBranch;

                                    if (branchid.Length > 4)
                                        branchid = branchid.Substring(branchid.Length - 4);
                                    DataTable dt2 = leb.GetRMInfo(rmInfo.FbTeller, branchid);
                                    if (dt2.Rows.Count > 0)
                                    {
                                        if (!string.IsNullOrEmpty(dt2.Rows[0]["EMail"].ToString()))
                                        {
                                            eMailList.Add(dt2.Rows[0]["EMail"].ToString() + "/" + dt2.Rows[0]["EmpName"].ToString());
                                            eMailList2.Add(dt2.Rows[0]["EMail"].ToString());
                                            WriteLog(string.Format("\t\t找到理專 {0} / {1} ", branchid, dt2.Rows[0]["EmpName"].ToString(), dt2.Rows[0]["EMail"].ToString()));
                                            #region 找經理
                                            DataTable dt3 = leb.GetManagerInfo(branchid);
                                            if (dt3.Rows.Count > 0)
                                            {
                                                foreach (DataRow dr in dt3.Rows)
                                                {
                                                    // 20180928, 還有以下四個身份, 都屬於分行經理/服務經理
                                                    //000662#母行經理                                                     //000547#母行服務經理
                                                    //000663#分行經理                                                     //000548#服務經理
                                                    if (dr["IsManager"].ToString().Contains("000663") || dr["IsManager"].ToString().Contains("000662"))
                                                    {
                                                        if (!string.IsNullOrEmpty(dr["EMail"].ToString()))
                                                        {
                                                            eMailList.Add(dr["EMail"].ToString() + "/" + dr["EmpName"].ToString());
                                                            eMailList2.Add(dr["EMail"].ToString());
                                                            WriteLog(string.Format("\t\t分行經理{0} /  {1}  ", dr["EmpName"].ToString(), dr["EMail"].ToString()));
                                                        }
                                                        else
                                                            WriteLog(string.Format("\t\t沒有找到  分行經理{0} /  {1}   ........... 的資訊", dr["EmpName"].ToString(), dr["EMail"].ToString()));
                                                    }
                                                    if (dr["IsManager"].ToString().Contains("000548") || dr["IsManager"].ToString().Contains("000547"))
                                                    {
                                                        if (!string.IsNullOrEmpty(dr["EMail"].ToString()))
                                                        {
                                                            eMailList.Add(dr["EMail"].ToString() + "/" + dr["EmpName"].ToString());
                                                            eMailList2.Add(dr["EMail"].ToString());
                                                            WriteLog(string.Format("\t\t服務經理{0} / {1}  ", dr["EmpName"].ToString(), dr["EMail"].ToString()));
                                                        }
                                                        else
                                                            WriteLog(string.Format("\t\t沒有  服務經理{0} / {1}   ........... 的資訊", dr["EmpName"].ToString(), dr["EMail"].ToString()));
                                                    }
                                                }
                                            }
                                            #endregion
                                        }
                                        else
                                            WriteLog(string.Format("\t\t沒有找到   理專 {0} / {1} ........... 的資訊", branchid, dt2.Rows[0]["EmpName"].ToString(), dt2.Rows[0]["EMail"].ToString()));
                                    }

                                }
                                #endregion
                                // 找出客戶等級
                                int CustLevel = string.IsNullOrEmpty(rmInfo.VipCdI) ? 0 : int.Parse(rmInfo.VipCdI);

                                WriteLog(string.Format("\t\t客戶等級{0} ", CustLevel.ToString()));
                                // 找出扣押金額
                                // 若無CaseSeizure 的筆數, 則不需要RUN下去了...
                                var allCaseSeizure = ctx.CaseSeizure.Where(x => x.CaseId == cc.CaseId).ToList();
                                if( allCaseSeizure.Count()==0)
                                {
                                    WriteLog("\t\t無存款往來\r\n\r\n ");
                                    continue;
                                }
                                decimal TotalSeizureAmt = (decimal)ctx.CaseSeizure.Where(x => x.CaseId == cc.CaseId).Sum(x => x.SeizureAmountNtd);
                                WriteLog(string.Format("\t\t扣押金額(台幣){0} ", TotalSeizureAmt.ToString()));
                                //(1)有扣押到金額(ETABS扣押金額欄位有值)
                                //(2)有理專 
                                //(3)客戶等級01-08或>=300  
                                #region Step 3
                                if (TotalSeizureAmt > 0 && eMailList.Count > 0 && ((CustLevel >= 1 && CustLevel <= 8) || CustLevel >= 300))
                                {
                                    string text = string.Empty;
                                    // 找到來文的PDF檔
                                    var ePDF = ctx.CaseEdocFile.Where(x => x.CaseId == cc.CaseId && x.Type == "收文" && x.FileType == "pdf").FirstOrDefault();
                                    if (ePDF != null)
                                    {
                                        byte[] pdfSource = ePDF.FileObject;
                                        var sDir = AppDomain.CurrentDomain.BaseDirectory;
                                        if (!sDir.EndsWith("\\"))
                                            sDir += "\\";

                                        sDir += @"PDF\";
                                        if (!Directory.Exists(sDir ))                                    {
                                        
                                            Directory.CreateDirectory(sDir);
                                        }

                                        string filename = sDir + cc.CaseNo + ".pdf";

                                        File.WriteAllBytes(sDir  + "PdfSource.pdf", pdfSource);
                                        // 822822+案件編號 822822+cc.
                                        var pass = "822822" + cc.CaseNo.Replace("A","");
                                        EncryptPDF(sDir  + "PdfSource.pdf", filename, false, pass, pass, PdfWriter.ALLOW_SCREENREADERS);


                                        // 刪除原文
                                        File.Delete(sDir  + "PdfSource.pdf");

                                        // 發二封信, 一封是主文+PDF, 另一封為密碼... 
                                        if (eMailList2.Count > 0)
                                        {
                                            noticeMail((string[])eMailList.ToArray(typeof(string)), cc.GovDate, cc.GovNo, cc.CaseNo, filename);
                                            noticePass((string[])eMailList2.ToArray(typeof(string)), filename, pass, cc.CaseNo);
                                        }
                                        else
                                        {
                                            WriteLog(string.Format("\r\n本案件, 無Email 可傳送\r\n "));
                                        }

                                        WriteLog(string.Format("\r\n\r\n "));
                                    }
                                    else // 只要發Body就好, 不用Attach PDF檔, 也不用寄Password
                                    {
                                        WriteLog(string.Format("\r\n本案件, 無PDF可傳送，只送信件主檔\r\n "));
                                        noticeMail((string[])eMailList.ToArray(typeof(string)), cc.GovDate, cc.GovNo, cc.CaseNo);
                                    }

                                }

                                else
                                {
                                    WriteLog(string.Format("不符合發送eMail條件\r\n\r\n "));
                                }
                                #endregion
                            }
                            catch(Exception ex)
                            {
                                WriteLog("發生錯誤：" + ex.Message.ToString());
                            }


                        }
                    }
                }

            }


        }





        /// <summary>
        /// pdf加密
        /// </summary>
        /// <param name="SrcPath">來源</param>
        /// <param name="OutPath">輸出</param>
        /// <param name="strength">強度(高:安全,但耗時)</param>
        /// <param name="UserPw">user密碼</param>
        /// <param name="OwrPw">owner密碼</param>
        /// <param name="pmss">權限(ex. PdfWriter.ALLOW_SCREENREADERS)</param>
        public static void EncryptPDF(string SrcPath, string OutPath, bool strength, string UserPw, string OwrPw, int pmss)
        {

            PdfReader reader = new PdfReader(SrcPath);
            using (var os = new FileStream(OutPath, FileMode.Create))
            {
                PdfEncryptor.Encrypt(reader, os, strength, UserPw, OwrPw, pmss);
            }
            reader.Close();
            reader = null;
            
        }


        private static void noticeMail(string[] mailFromTo, DateTime GovDate,string GovUnit, string CaseNo,string attach=null)
        {
            string mailFrom = ConfigurationManager.AppSettings["mailFrom"];
            //string[] mailFromTo = ConfigurationManager.AppSettings["mailTo"].ToString().Split(',');

            string subject = "通知-行政機關來函扣押VIP客戶存款(" +CaseNo.Substring(1) + ")";
            string body = "長官 您好：<br/>" +
"本科於{0} 收到{1}來函扣押存款債權執行命令乙份，函文內容及扣押情形(詳如附件及ETABS系統)。</br>" +
"案件編號:{2} <br/> "+
"如您已知悉此訊息，無須理會此MAIL。 <br/>" +
"承辦人: {3} 集作一科-一組  <br/> " +
"Tel:(02)8178-7777 <br/> " + 
"Fax:(02)2760-9536 <br/> " + 
"Information Classification :  C <br/> " + 
"Declassified Date：9999/99/99 <br/> " + 
"(P-Public : I- Internal Use : C- Confidential : R-Restricted)  <br/> " ;
            string DepName = ConfigurationManager.AppSettings["DepName"];
            string host = ConfigurationManager.AppSettings["mailHost"];
            body = string.Format(body, GovDate.ToString("yyy.MM.dd"), GovUnit, CaseNo, DepName);

            //        public UtlMail(
            //string m_FromEmail
            //, string m_FromName
            //, string[] m_ToEmailAndName
            //, string m_Subject
            //, string m_EmailBody
            //, bool m_IsBodyHtml
            //, string[] m_Attachments
            //, string m_EmailServer)

            var em1 =new UtlMail();
            if( attach==null)
            {
                em1 = new UtlMail(mailFrom, "CSFS", mailFromTo, subject, body, true, null, host);
            }
            else
            {
                em1 = new UtlMail(mailFrom, "CSFS", mailFromTo, subject, body, true, new string[] { attach }, host);
            }
            

            var aaa = em1.SendMail();
            if (aaa)
                WriteLog("\t\t發送成功");
            else
                WriteLog("\t\t發送失敗");
            //var eMail = new UtlMail();
            //eMail._FromEmail = mailFrom;
            //eMail._EmailServer = host;
            //eMail._FromName = "CSFS";
            //eMail._Subject = subject;
            //eMail._EmailBody = body;
            //eMail._IsBodyHtml = true;

            //eMail._ToEmailAndName = mailFromTo;

            //if( attach!=null)
            //    eMail._Attachments = new string[] { attach };
            
            //UtlMail.SendEmail(mailFrom, mailFromTo, subject, body, host);
            //eMail.SendMail();
            //UtlMail.SendMail(mailFromTo, body, new string[] { attach });
            ////eMail.SendMail(mailFromTo, body, null);
        }


        private static void noticePass(string[] mailFromTo, string filename, string pass, string CaseNo)
        {
            string mailFrom = ConfigurationManager.AppSettings["mailFrom"];
            //string[] mailFromTo = ConfigurationManager.AppSettings["mailTo"].ToString().Split(',');

            string subject = "開啟PDF密碼通知(" + CaseNo.Substring(1) + ")" ;
            string body = "長官 您好：\r\n" +
"附件:{0}, 開啟密碼為 {1}\r\n" +
"Tel:(02)8178-7777 \r\n " +
"Fax:(02)2760-9536 \r\n " +
"Information Classification :  C \r\n " +
"Declassified Date：9999/99/99 \r\n " +
"(P-Public : I- Internal Use : C- Confidential : R-Restricted)  \r\n";
            string DepName = ConfigurationManager.AppSettings["DepName"];
            string host = ConfigurationManager.AppSettings["mailHost"];
            body = string.Format(body, filename, pass);

            UtlMail.SendEmail(mailFrom, mailFromTo, subject, body, host);
            
        }

        public static void WriteLog(string msg)
        {
            if (Directory.Exists(@"\Log") == false)
            {
                Directory.CreateDirectory(@"\Log");
            }
            //LogManager.Exists("DebugLog").Debug(msg);
            log.Info(msg);
            LogManager.Exists("DebugLog").Debug(msg);
        }
    }
}
