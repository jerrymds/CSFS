using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using CTBC.CSFS.Models;
using CTBC.CSFS.BussinessLogic;
using CTBC.FrameWork.Util;
using CTBC.CSFS.Pattern;
using System.Web.Mvc;
using CTBC.CSFS.Filter;
using CTBC.CSFS.ViewModels;
using System.Configuration;
using System.Collections;

namespace CTBC.CSFS.Areas.Director.Controllers
{
    public class CaseSendUpLoadController : AppController
    {
        DirectorToApproveBIZ _directorBiz;
        SendEDocBiz _SendEDocBiz;
        EmailGroupBiz _EmailGroupBiz;
		PARMCodeBIZ parm = new PARMCodeBIZ();
        public CaseSendUpLoadController()
        {
            _directorBiz = new DirectorToApproveBIZ(this);
            _SendEDocBiz = new SendEDocBiz();
            _EmailGroupBiz = new EmailGroupBiz(this);
        }
        [RootPageFilter]
        public ActionResult Index(string isBack)
        {
            DirectorToApprove model = new DirectorToApprove();
            if (isBack == "1")
            {
                HttpCookie cookies = Request.Cookies.Get("QueryCookie");
                if (cookies != null)
                {
                    if (cookies.Values["CaseKind"] != null) model.CaseKind = cookies.Values["CaseKind"];
                    if (cookies.Values["CaseKind2"] != null) ViewBag.CaseKind2Query = cookies.Values["CaseKind2"];
                    if (cookies.Values["GovUnit"] != null) model.GovUnit = cookies.Values["GovUnit"];
                    if (cookies.Values["CaseNo"] != null) model.CaseNo = cookies.Values["CaseNo"];
                    if (cookies.Values["GovDateS"] != null) model.GovDateS = cookies.Values["GovDateS"];
                    if (cookies.Values["GovDateE"] != null) model.GovDateE = cookies.Values["GovDateE"];
                    if (cookies.Values["Speed"] != null) model.Speed = cookies.Values["Speed"];
                    if (cookies.Values["ReceiveKind"] != null) model.ReceiveKind = cookies.Values["ReceiveKind"];
                    if (cookies.Values["GovNo"] != null) model.GovNo = cookies.Values["GovNo"];
                    if (cookies.Values["CreatedDateS"] != null) model.CreatedDateS = cookies.Values["CreatedDateS"];
                    if (cookies.Values["CreatedDateE"] != null) model.CreatedDateE = cookies.Values["CreatedDateE"];
                    if (cookies.Values["Unit"] != null) model.Unit = cookies.Values["Unit"];
                    if (cookies.Values["CreateUser"] != null) model.CreateUser = cookies.Values["CreateUser"];
                    if (cookies.Values["SendDateS"] != null) model.SendDateS = cookies.Values["SendDateS"];
                    if (cookies.Values["SendDateE"] != null) model.SendDateE = cookies.Values["SendDateE"];
                    //20170811 RC RQ-2015-019666-020 派件至跨單位 宏祥 update start
                    //if (cookies.Values["AgentUser"] != null) model.AgentUser = cookies.Values["AgentUser"];
                    if (cookies.Values["AgentDepartment"] != null) model.AgentDepartment = cookies.Values["AgentDepartment"];
                    if (cookies.Values["AgentDepartment2"] != null) model.AgentDepartment2 = cookies.Values["AgentDepartment2"];
                    if (cookies.Values["AgentDepartmentUser"] != null) model.AgentDepartmentUser = cookies.Values["AgentDepartmentUser"];
                    //20170811 RC RQ-2015-019666-020 派件至跨單位 宏祥 update end
                    ViewBag.CurrentPage = cookies.Values["CurrentPage"];
                    ViewBag.isQuery = "1";
                }
            }

			var uploadCount = parm.GetCodeData("UploadCount").FirstOrDefault().CodeDesc;
            var BatchCount = parm.GetCodeData("BatchCount").FirstOrDefault().CodeDesc;
            ViewBag.BatchQueryCount = 0;
            ViewBag.UploadCount = uploadCount;
            ViewBag.BatchCount = BatchCount;

            InitDropdownListOptions();
            return View(model);
        }
        [HttpPost]
        public ActionResult _QueryResult(DirectorToApprove model, int pageNum = 1, string strSortExpression = "CaseNo", string strSortDirection = "asc")
        {
            #region Cookie
            HttpCookie modelCookie = new HttpCookie("QueryCookie");
            modelCookie.Values.Add("CaseKind", model.CaseKind);
            modelCookie.Values.Add("CaseKind2", model.CaseKind2);
            modelCookie.Values.Add("GovUnit", model.GovUnit);
            modelCookie.Values.Add("CaseNo", model.CaseNo);
            modelCookie.Values.Add("GovDateS", model.GovDateS);
            modelCookie.Values.Add("GovDateE", model.GovDateE);
            modelCookie.Values.Add("Speed", model.Speed);
            modelCookie.Values.Add("ReceiveKind", model.ReceiveKind);
            modelCookie.Values.Add("GovNo", model.GovNo);
            modelCookie.Values.Add("CreatedDateS", model.CreatedDateS);
            modelCookie.Values.Add("CreatedDateE", model.CreatedDateE);
            //20170811 RC RQ-2015-019666-020 派件至跨單位 宏祥 update start
            //modelCookie.Values.Add("AgentUser", model.AgentUser);
            modelCookie.Values.Add("AgentDepartment", model.AgentDepartment);
            modelCookie.Values.Add("AgentDepartment2", model.AgentDepartment2);
            modelCookie.Values.Add("AgentDepartmentUser", model.AgentDepartmentUser);
            //20170811 RC RQ-2015-019666-020 派件至跨單位 宏祥 update end
            modelCookie.Values.Add("CreateUser", model.CreateUser);
            modelCookie.Values.Add("Unit", model.Unit);
            modelCookie.Values.Add("SendDateS", model.SendDateS);
            modelCookie.Values.Add("SendDateE", model.SendDateE);
            modelCookie.Values.Add("CurrentPage", pageNum.ToString());
            Response.Cookies.Add(modelCookie);
            #endregion

            string UserId = LogonUser.Account;
            if (!string.IsNullOrEmpty(model.GovDateE))
            {
                model.GovDateE = UtlString.FormatDateTwStringToAd(model.GovDateE);
            }
            if (!string.IsNullOrEmpty(model.GovDateS))
            {
                model.GovDateS = UtlString.FormatDateTwStringToAd(model.GovDateS);
            }
            if (!string.IsNullOrEmpty(model.CreatedDateE))
            {
                model.CreatedDateE = UtlString.FormatDateTwStringToAd(model.CreatedDateE);
            }
            if (!string.IsNullOrEmpty(model.CreatedDateS))
            {
                model.CreatedDateS = UtlString.FormatDateTwStringToAd(model.CreatedDateS);
            }

            if (!string.IsNullOrEmpty(model.SendDateS))
            {
                model.SendDateS = UtlString.FormatDateTwStringToAd(model.SendDateS);
            }
            if (!string.IsNullOrEmpty(model.SendDateE))
            {
                model.SendDateE = UtlString.FormatDateTwStringToAd(model.SendDateE);
            }
            IList<DirectorToApprove> list = _directorBiz.GetCaseSendUpLoad(model, pageNum, strSortExpression, strSortDirection, UserId);
            var dtvm = new DirectorToApproveViewModel()
            {
                DirectorToApprove = model,
                DirectorToApprovelist = list,
            };
            //分頁相關設定
            dtvm.DirectorToApprove.PageSize = _directorBiz.PageSize;
            dtvm.DirectorToApprove.CurrentPage = _directorBiz.PageIndex;
            dtvm.DirectorToApprove.TotalItemCount = _directorBiz.DataRecords;
            ViewBag.BatchQueryCount = _directorBiz.DataRecords;
            dtvm.DirectorToApprove.SortExpression = strSortExpression;
            dtvm.DirectorToApprove.SortDirection = strSortDirection;

            dtvm.DirectorToApprove.CaseNo = model.CaseNo;
            dtvm.DirectorToApprove.GovUnit = model.GovUnit;
            dtvm.DirectorToApprove.GovDate = model.GovDate;
            dtvm.DirectorToApprove.GovNo = model.GovNo;
            dtvm.DirectorToApprove.Person = model.Person;
            dtvm.DirectorToApprove.CaseKind = model.CaseKind;
            dtvm.DirectorToApprove.CaseKind2 = model.CaseKind2;
            dtvm.DirectorToApprove.Speed = model.Speed;
            dtvm.DirectorToApprove.LimitDate = model.LimitDate;
            
            return PartialView("_QueryResult", dtvm);
        }
        public void InitDropdownListOptions()
        {
            PARMCodeBIZ parm = new PARMCodeBIZ();
            ViewBag.CASE_END_TIME = parm.GetCASE_END_TIME("CASE_END_TIME");                                     //* 每天最晚時間
            ViewBag.GOV_KINDList = new SelectList(parm.GetCodeData("GOV_KIND"), "CodeDesc", "CodeDesc");        //* 來文機關類型
            ViewBag.SpeedList = new SelectList(parm.GetCodeData("INCOME_SPEED"), "CodeDesc", "CodeDesc");       //* 速別
            ViewBag.ReceiveKindList = new SelectList(parm.GetCodeData("INCOME_TYPE"), "CodeDesc", "CodeDesc");  //* 來文方式
            ViewBag.CaseKindList = new SelectList(parm.GetCodeData("CASE_KIND"), "CodeDesc", "CodeDesc");
            ViewBag.CaseKind2List = new SelectList(parm.GetCodeData("CASE_SEIZURE"), "CodeDesc", "CodeDesc");
            //LdapEmployeeBiz emp = new LdapEmployeeBiz();
            //List<string> leaderList = emp.GetTopEmployeeDirectorIdList();
            //ViewBag.isTopDirector = leaderList.Contains(LogonUser.Account);
            //20170811 RC RQ-2015-019666-020 派件至跨單位 宏祥 update start
            //ViewBag.AgentUserList = new SelectList(new LdapEmployeeBiz().GetAllEmployeeNoMangerView(), "EmpID", "EmpIdAndName");
            ViewBag.AgentDepartmentList = new SelectList(parm.GetCodeData("CollectionToAgent_AgentDDLDepartment"), "CodeNo", "CodeDesc");
            ViewBag.AgentDepartment2List = new SelectList(new AgentSettingBIZ().GetAgentDepartment2View(), "SectionName", "SectionName");
            ViewBag.AgentDepartmentUserList = new SelectList(new AgentSettingBIZ().GetAgentDepartmentUserView(), "EmpID", "EmpIdAndName");
            //20170811 RC RQ-2015-019666-020 派件至跨單位 宏祥 update end
        }
        public ActionResult Batch(string caseIdarr)
        {
            string flag = "0";
            try
            {
                string userId = LogonUser.Account;
                string[] caseid = caseIdarr.Split(',');
                caseIdarr = string.Empty;
                foreach (var item in caseid)
                {
                    caseIdarr += item.PadLeft(item.Length + 1, '\'').PadRight(item.Length + 2, '\'') + ",";
                }
                caseIdarr = caseIdarr.TrimEnd(',');
                List<Guid> caseIdList = new List<Guid>();
                IList<CaseEdocFile> list = _directorBiz.GetBatchControlNotUp(caseIdarr);
                if (list != null && list.Count > 0)
                {
                    //上傳檔案
                    string ftpserver = ConfigurationManager.AppSettings["ftpserver"];
                    string username = ConfigurationManager.AppSettings["username"];
                    string password = ConfigurationManager.AppSettings["password"];
                    //由於framework中會先做解密
                    //password = UtlString.EncodeBase64(password);


                    string port = ConfigurationManager.AppSettings["port"];
                    string ftpdir = ConfigurationManager.AppSettings["ftpdir"];
                    string[] ftpdirs = ftpdir.Split('/').Where(m => !string.IsNullOrEmpty(m)).ToArray();
                    FtpClient ftpClient = new FtpClient(ftpserver, username, password, port);
                    ArrayList rootDirList = ftpClient.GetDirList();
                    if (rootDirList.Contains(ftpdirs[0]))
                    {
                        int i = 0;
                        string dir = string.Empty;
                        for (; i < ftpdirs.Length; i++)
                        {
                            if (i == ftpdirs.Length - 1)
                            {
                                break;
                            }
                            dir += "/" + ftpdirs[i];
                            ArrayList arr = ftpClient.GetDirList(dir);
                            if (!arr.Contains(ftpdirs[i + 1]))
                            {
                                break;
                            }
                        }
                        if (i != ftpdirs.Length - 1)
                        {
                            for (; i < ftpdirs.Length - 1; i++)
                            {
                                dir += "/" + ftpdirs[i + 1];
                                ftpClient.CreateDir(dir);
                            }
                        }
                    }
                    else
                    {
                        string dir = string.Empty;
                        for (int i = 0; i < ftpdirs.Length; i++)
                        {
                            dir += "/" + ftpdirs[i];
                            ftpClient.CreateDir(dir);
                        }
                    }
                    foreach (var item in list)
                    {
                        ftpClient.SendFile(ftpdir, item.FileName, item.FileObject);
                    }
                    foreach (var item in list)
                    {
                        if (!caseIdList.Contains(item.CaseId))
                        {
                            caseIdList.Add(item.CaseId);
                        }
                    }
                    if (_directorBiz.UpdateBatchControlUp(caseIdList, userId))
                    {
                        flag = "1";
                    }
                    if (flag == "1")
                    {
                        IList<Email_Notice> Email_NoticeList = _EmailGroupBiz.GetQueryList();
                        if (Email_NoticeList != null && Email_NoticeList.Count > 0)
                        {
                            string mailFrom = ConfigurationManager.AppSettings["mailFrom"];
                            string[] mailFromTo = new string[Email_NoticeList.Count];
                            for (int i = 0; i < Email_NoticeList.Count; i++)
                            {
                                mailFromTo[i] = Email_NoticeList[i].Email;
                            }
                            string[] date = UtlString.FormatDateTw(DateTime.Now.ToString("yyyy/MM/dd")).Split('/');
                            string subject = String.Format("{0}年{1}月{2}日(個金集作)已發送電子發文共計{3}件，請協助發文", date[0], date[1], date[2], caseid.Length);
                            string body = "電子發文郵件";
                            string host = ConfigurationManager.AppSettings["mailHost"];
                            UtlMail.SendEmail(mailFrom, mailFromTo, subject, body, host);
                            flag = "2";
                        }
                    }
                }
                else
                {
                    flag = "3";
                }
                return Json(new JsonReturn() { ReturnCode = flag });
            }
            catch (Exception ex)
            {
                return Json(new JsonReturn() { ReturnCode = flag });
            }

            //return Json(_directorBiz.UpdateBatchControlUp(caseIdList, userId));
        }
        private void Log()
        {
            CSFSLogBIZ _csfsLogBIZ = new CSFSLogBIZ();
            this.ApLog.Categories.Add("LogonLogout");
            this.ApLog.Title = "CaseSendUpload";
            this.ApLog.Priority = 1;
            this.ApLog.EventId = 101;
            this.ApLog.TimeStamp = DateTime.Now;
            this.ApLog.Severity = System.Diagnostics.TraceEventType.Start;
            this.ApLog.dic["ActionCode"] = ActionCode.Login;
            this.ApLog.dic["TranFlag"] = TranFlag.After;
            this.ApLog.dic["FunctionId"] = "Action." + this.ControllerName + "." + this.ActionName;
            this.ApLog.dic["SessionId"] = HttpUtility.HtmlEncode(HttpContext.Session.SessionID);//20150108 horace 弱掃
            this.ApLog.dic["URL"] = HttpUtility.HtmlEncode(HttpContext.Request.RawUrl);//20150108 horace 弱掃
            this.ApLog.dic["IP"] = HttpUtility.HtmlEncode(HttpContext.Request.UserHostAddress);//20150108 horace 弱掃
            this.ApLog.dic["MachineName"] = HttpUtility.HtmlEncode(HttpContext.Request.UserHostName);//20150108 horace 弱掃
            this.ApLog.ExtendedProperties = this.ApLog.dic;
            _csfsLogBIZ.LogonLogoutLog(this.ApLog);//20150209弱掃
            this.ApLog.Categories.Remove("LogonLogout");
        }
        public ActionResult Upload(string caseIdarr)
        {
            string flag = "0";
            try
            {
                string userId = LogonUser.Account;
                string[] caseid = caseIdarr.Split(',');
                caseIdarr = string.Empty;
                foreach (var item in caseid)
                {
                    caseIdarr += item.PadLeft(item.Length + 1, '\'').PadRight(item.Length + 2, '\'') + ",";
                }
                caseIdarr = caseIdarr.TrimEnd(',');
                List<Guid> caseIdList = new List<Guid>();
                IList<CaseEdocFile> list = _directorBiz.GetBatchControlNotUp(caseIdarr);
                if (list != null && list.Count > 0)
                {
                    //上傳檔案
                    string ftpserver = ConfigurationManager.AppSettings["ftpserver"];
                    string username = ConfigurationManager.AppSettings["username"];
                    string password = ConfigurationManager.AppSettings["password"];
                    //由於framework中會先做解密
                    //password = UtlString.EncodeBase64(password);


                    string port = ConfigurationManager.AppSettings["port"];
                    string ftpdir = ConfigurationManager.AppSettings["ftpdir"];
                    string[] ftpdirs = ftpdir.Split('/').Where(m => !string.IsNullOrEmpty(m)).ToArray();
                    FtpClient ftpClient = new FtpClient(ftpserver, username, password, port);
                    ArrayList rootDirList = ftpClient.GetDirList();
                    if (rootDirList.Contains(ftpdirs[0]))
                    {
                        int i = 0;
                        string dir = string.Empty;
                        for (; i < ftpdirs.Length; i++)
                        {
                            if (i == ftpdirs.Length - 1)
                            {
                                break;
                            }
                            dir += "/" + ftpdirs[i];
                            ArrayList arr = ftpClient.GetDirList(dir);
                            if (!arr.Contains(ftpdirs[i + 1]))
                            {
                                break;
                            }
                        }
                        if (i != ftpdirs.Length - 1)
                        {
                            for (; i < ftpdirs.Length - 1; i++)
                            {
                                dir += "/" + ftpdirs[i + 1];
                                ftpClient.CreateDir(dir);
                            }
                        }
                    }
                    else
                    {
                        string dir = string.Empty;
                        for (int i = 0; i < ftpdirs.Length; i++)
                        {
                            dir += "/" + ftpdirs[i];
                            ftpClient.CreateDir(dir);
                        }
                    }
                    foreach (var item in list)
                    {
                        ftpClient.SendFile(ftpdir, item.FileName, item.FileObject);
                    }
                    foreach (var item in list)
                    {
                        if (!caseIdList.Contains(item.CaseId))
                        {
                            caseIdList.Add(item.CaseId);
                        }
                    }
                    if (_directorBiz.UpdateBatchControlUp(caseIdList, userId))
                    {
                        flag = "1";
                    }
                    if (flag == "1")
                    {
                        this.LogWriter = this.AppLog.Writer;
                        this.ApLog.Message = "電子上傳成功!!";
                        this.ApLog.dic["UserId"] = "System";
                        this.ApLog.dic["FunctionId"] = this.ControllerName;
                        Log();
                        IList<Email_Notice> Email_NoticeList = _EmailGroupBiz.GetQueryList();
                        if (Email_NoticeList != null && Email_NoticeList.Count > 0)
                        {
                            string mailFrom = ConfigurationManager.AppSettings["mailFrom"];
                            string[] mailFromTo = new string[Email_NoticeList.Count];
                            for (int i = 0; i < Email_NoticeList.Count; i++)
                            {
                                mailFromTo[i] = Email_NoticeList[i].Email;
                            }
                            string[] date = UtlString.FormatDateTw(DateTime.Now.ToString("yyyy/MM/dd")).Split('/');
                            string subject = String.Format("{0}年{1}月{2}日(個金集作)已發送電子發文共計{3}件，請協助發文", date[0], date[1], date[2], caseid.Length);
                            string body = "電子發文郵件";
                            string host = ConfigurationManager.AppSettings["mailHost"];
                            UtlMail.SendEmail(mailFrom, mailFromTo, subject, body, host);
                            flag = "2";
                        }
                    }
                }
                else
                {
                    flag = "3";
                }
                return Json(new JsonReturn() { ReturnCode = flag });   
            }
            catch (Exception ex)
            {
                this.LogWriter = this.AppLog.Writer;
                this.ApLog.Message = "上傳失敗:"+ex.ToString();
                this.ApLog.dic["UserId"] = "System";
                this.ApLog.dic["FunctionId"] = this.ControllerName;
                Log();
                //CSFSLogBIZ _csfsLogBIZ = new CSFSLogBIZ();               
                //_csfsLogBIZ.LogonLogoutLog(this.ApLog);
                return Json(new JsonReturn() { ReturnCode = flag });  
            }
            
            //return Json(_directorBiz.UpdateBatchControlUp(caseIdList, userId));
        } 
	}
}