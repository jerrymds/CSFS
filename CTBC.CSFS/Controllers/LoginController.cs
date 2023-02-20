/// <summary>
/// 程式說明:Login Controller - 登入
/// 維護部門:資訊管理處
/// CSFS Version:v3.0
/// 中國信託銀行 版權所有  ©  All Rights Reserved. 
/// </summary>

using System;
using System.Collections;//20130520 horace
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using CTBC.CSFS.Pattern;
using CTBC.CSFS.Models;
//////using CTBC.CSFS.Library.HTG;//20130520 horace
using Microsoft.Practices.EnterpriseLibrary.Logging;
using CTBC.FrameWork.Platform;//20130306 horace
using CTBC.CSFS.Resource;
using CTBC.CSFS.ViewModels;
using System.Security;//20150108 horace 弱掃
using System.Runtime.InteropServices;
using System.Web.ApplicationServices;
using CTBC.CSFS.BussinessLogic;
using CTCB.NUMS.Library.HTG;

//20150108 horace 弱掃

namespace CTBC.CSFS.Controllers
{
    public class LoginController : AppController
    {
		// GET: /Login/

		public ActionResult Index()
        {
            #region 已登入 則導入首頁 20140115 多增加Authentication的判斷
            //if (Session["LogonUser"] != null && Request.IsAuthenticated)
            //{
            //    return RedirectToAction("Index", "Home");
            //}
            #endregion

            if (Session["CultureName"] == null)
                Session["CultureName"] = Config.GetValue("CUF_CultureName");
            SelectCultureName();
            ViewBag.ErrorMessage = "";
            SelectRCAFBranch();
            LoginViewModel model = new LoginViewModel();
            return View(model);
		}

		// POST: Authenticate
        [HttpPost]
        public ActionResult Index(string username, string password, string usrRCAF, string psRCAF, string hid_brhRCAF)
        {
            #region
            //if (username.Trim() != "" && password.Trim() != "" && username.Trim() == "Z00001234" && password.Trim() == "Z00001234")
            if (username.Trim() != "" && password.Trim() != "")
            {
                //username = username.Trim().ToUpper();//20130221 horace
                //password = password.Trim();//20130526 horace

                //以下資料PROD請移除----
                //psRCAF = "Z00001234";
                //hid_brhRCAF = "0495";

                //------------------------
                //弱掃
                //------------------------
                //20150108 horace 弱掃
                SecureString securityPassword = new SecureString();
                SecureString securityRCAFPassword = new SecureString();
                if (username != null)
                    username = username.Trim().ToUpper();//20130221 horace
                else
                    username = "";

                if (password != null)
                {
                    securityPassword = ToSecureString(password.Trim());
                    securityPassword.MakeReadOnly();
                }
                else
                    securityPassword = ToSecureString("");

                // Legend 2017/10/25 添加  psRCAF 不等於 空白驗證
                if (psRCAF != null  && psRCAF.Trim() != "")
                {
                    securityRCAFPassword = ToSecureString(psRCAF.Trim());
                    securityRCAFPassword.MakeReadOnly();
                }
                else
                    securityRCAFPassword = ToSecureString("");
                //------------------------
                //弱掃
                //------------------------

                //20130307 horace 加同帳號只可登入一次----------------------------------------------------------
                Dictionary<string, User> userLst = new Dictionary<string, User>();
                //20130307 horace if App.Cache("OnlineUserList")存在
                if (CTBC.FrameWork.Platform.AppCache.InCache("ONLINE_USER_LIST"))
                {
                    //CTBC.FrameWork.Platform.AppCache.Update(userLst, "ONLINE_USER_LIST");
                    //SelectRCAFBranch();
                    //return RedirectToAction("Index", "Home");
                    #region
                    //-------------------------------------------------------------------------------------------
                    //string err1 = username + "在" + ((User)userLst[username]).SessionIP + "已經登入系統了!";
                    //ViewBag.ErrorMessage = err1;
                    //SelectRCAFBranch();
                    //this.ApLog.Message = "";
                    //this.ApLog.dic["UserId"] = username;
                    //this.ApLog.dic["Result"] = Result.Failure;
                    //Log();                        
                    //return View("Index");
                    //-------------------------------------------------------------------------------------------
                    #endregion
                    //}
                    //else
                    //{
                    //20130307 horace 將user加入線上使用者list中                         
                    //CTBC.FrameWork.Platform.User lo_User = new CUF.Platform.User(username, true, password, usrRCAF, psRCAF, hid_brhRCAF);
                    CTBC.FrameWork.Platform.User lo_User = new CTBC.FrameWork.Platform.User(username, true, securityPassword, usrRCAF, securityRCAFPassword, hid_brhRCAF);//20150108 horace 弱掃
                    if (lo_User.Authenticated)
                    {
                       // 20180606,PeterHsieh,增加RCAF判斷
                       if (Session["RCAFStatus"] != null)
                       {
                          if (!((Boolean)Session["RCAFStatus"]))
                          {
                             // RCAF登入失敗，清空RACF資料，避免後續若USER依然強迫登入時，造成系統依然可發查的錯誤狀況
                             lo_User.RCAFAccount = "";
                             lo_User.RCAFBranch = "";
                             lo_User.RCAFPs = "";

//                             Session["LogonUser"] = lo_User;
                          }
                       }
                       
                       //* 20150707 允許通過EmpBusinessCategory判斷, 部分沒有設定Role的人員視作建檔人員 Start
                        LdapEmployeeBiz empBiz = new LdapEmployeeBiz();
                        IList<LDAPEmployee> list = empBiz.GetNoRoleEmployeeIdList();

                        if (list != null && list.Any() && list.FirstOrDefault(m => m.EmpId.Trim().ToUpper() == username.Trim().ToUpper()) != null)
                        {
                            //* 屬於無需設置Role組
                            if(lo_User.Roles == null)
                                lo_User.Roles = new List<User.RoleInfo>();
                            lo_User.Roles.Add(new User.RoleInfo { RoleId = "cn=CSFS011,ou=CSFS,ou=APPs,o=CTCB", RoleLDAPId = "CSFS011", RoleName = "分行經辦" });
                        }
                        list = empBiz.GetNoRoleEmployeeIdList2();
                        if (list != null && list.Any() && list.FirstOrDefault(m => m.EmpId.Trim().ToUpper() == username.Trim().ToUpper()) != null)
                        {
                            //* 屬於無需設置Role組
                            if (lo_User.Roles == null)
                                lo_User.Roles = new List<User.RoleInfo>();
                            lo_User.Roles.Add(new User.RoleInfo { RoleId = "cn=CSFS015,ou=CSFS,ou=APPs,o=CTCB", RoleLDAPId = "CSFS015", RoleName = "分行主管" });
                        }
                        //* 20150707 允許通過EmpBusinessCategory判斷, 部分沒有設定Role的人員視作建檔人員 End


                        //20130418 horace 使用者認證通過,但user沒有被asign本系統角色
                        if (lo_User.Roles == null)
                        {
                            AlertMsg(Lang.csfs_err_noright_sys, username);
                            SelectCultureName();
                            return View("Index");
                        }
                        if (lo_User.Roles != null && lo_User.Roles.Any() && lo_User.Roles.Any(item => !string.IsNullOrEmpty(item.RoleId)))
                        {
                            userLst = (Dictionary<string, User>)AppCache.Get("ONLINE_USER_LIST");
                            if (userLst.ContainsKey(username))
                            {
                                this.ApLog.Message = Lang.csfs_dpc_usr1 + ((User)userLst[username]).SessionIP + Lang.csfs_dpc_usr2 + username + Lang.csfs_dpc_usr3;
                                this.ApLog.dic["UserId"] = username;
                                this.ApLog.dic["Result"] = Result.Success;
                                Log();
                                userLst.Remove(username);
                            }

                            //edit by mel 20130829 IR-62411 左邊的MenuTree, 有些字無法正確顯示
                            //因ladp 回傳的name 即為亂碼,些處以自employee中取出正常的名字後再回置

                            LdapEmployeeBiz ldapEmploee = new LdapEmployeeBiz(this);
                            LDAPEmployee empov = ldapEmploee.GetLdapEmployeeByEmpId(username);
                            if (empov != null)
                            {
                                lo_User.Name = empov.EmpName;
                                lo_User.DepId = empov.DepId;
                                lo_User.DepName = empov.DepName;
                                lo_User.BranchId = empov.BranchId;
                                lo_User.BranchName = empov.BranchName;
                                lo_User.UnitForKeyIn = ldapEmploee.GetBranchId(empov);
                            }
                            //* 判斷是否屬於一二三課。如果是則扣押鍵檔時不會檢核時間
                            List<LDAPEmployee> viewList = ldapEmploee.GetAllEmployeeInEmployeeView();
                            if (viewList != null && viewList.Any(m => m.EmpId == username))
                                lo_User.IsInEmployeeView = true;
                            //* 鍵檔時科別
                            //認證通過
                            FormsAuthentication.SetAuthCookie(username, false);
                            Session["UserAccount"] = username;
                            Session["LogonUser"] = lo_User;
                            
                            //加上session info---------------------------------------------------
                            lo_User.SessionID = Session.SessionID;
                            lo_User.SessionIP = Request.ServerVariables["REMOTE_ADDR"];
                            lo_User.SessionLoginTime = DateTime.Now;

                            //20130308 horace 將登入者資訊更新回ONLINE_USER_LIST,Key=登入員編----- 
                            userLst.Add(username, lo_User);
                            AppCache.Update(userLst, "ONLINE_USER_LIST");

                            ApLog.dic["UserId"] = lo_User.Account;
                            ApLog.dic["Result"] = Result.Success;
                            Log();
                            return RedirectToAction("Index", "Home");
                        }
                    }
                }
                else
                {
                    //20130418 hroace 整個App.Cache("OnlineUserList")不存在
                    AlertMsg(Lang.csfs_online_usr_netexist, username);
                    SelectCultureName();
                    return View("Index");
                }
            }
            //20130418 hroace 認證失敗
            AlertMsg(Lang.csfs_logon_err_idpwd, username);
            SelectCultureName();
            return View("Index");
            #endregion 
        }



        [HttpPost]
        public ActionResult ChkRCAF(string usrn, string usrp, string urcafn, string urcafp, string rcafb)
        {
            #region 原有代碼注釋
            //string rtn="";
            //if (usrn == "Z00001234" && usrp == "Z00001234")
            //{
            //    rtn = Lang.csfs_err_rcaf1;

            //    this.ApLog.Message = Lang.csfs_err_rcaf1;
            //    this.ApLog.dic["UserId"] = usrn;
            //    this.ApLog.dic["Result"] = Result.Failure;
            //    Log();                
            //}
            //else
            //{
            //    rtn = "VALID_LDAP_FAILURE";
            //}
            //return Content(rtn);
            #endregion

            #region Legend 2017/10/25 將NUMS中驗證RCAF部分添加
            HTGObject obj;
            string rtn = "";

            //------------------------
            //弱掃
            //------------------------
            SecureString securityPassword = new SecureString();
            SecureString securityRCAFPassword = new SecureString();
            if (usrn != null)
                usrn = usrn.Trim().ToUpper();
            else
                usrn = "";

            if (usrp != null)
            {
                securityPassword = ToSecureString(usrp.Trim());
                securityPassword.MakeReadOnly();
            }
            else
                securityPassword = ToSecureString("");

            if (urcafp != null && urcafp.Trim() != "")
            {
                securityRCAFPassword = ToSecureString(urcafp.Trim());
                securityRCAFPassword.MakeReadOnly();
            }
            else
                securityRCAFPassword = ToSecureString("");
            //------------------------
            //弱掃
            //------------------------

            CTBC.FrameWork.Platform.User lo_User = new CTBC.FrameWork.Platform.User(usrn, true, securityPassword, urcafn, securityRCAFPassword, rcafb);
            if (lo_User.Authenticated)
            {
                string _applicationid = "CSFS";
                string _htgurl = "";
                bool result = false;
                Hashtable htparm = new Hashtable();
                Hashtable htreturn = new Hashtable();
                _htgurl = Config.GetValue("HTGUrl");

                obj = new CTCB.NUMS.Library.HTG.HTGObject(_htgurl, _applicationid, usrn, usrp, urcafn, urcafp, rcafb);
                result = obj.CheckHtgLogin();
                if (!result)
                {
                    //登入失敗
                    //return code
                    htreturn = obj.ReturnCode;
                    //HTG 訊息
                    string HtgMessage = htreturn["HtgMessage"].ToString();
                    //主機回應碼
                    string HGExceptionMessage = htreturn["HGExceptionMessage"].ToString();
                    //訊息代碼
                    string HtgReturnCode = htreturn["HtgReturnCode"].ToString();
                    rtn = "訊息：RCAF 帳號登入異常\r\nHTG回應碼=" + HtgReturnCode + "\r\nHTG錯誤訊息=" + HGExceptionMessage + "\r\n是否繼續登入CSFS系統?";

                    this.ApLog.Message = "訊息=RCAF帳號登入異常;HtgReturnCode=" + HtgReturnCode + ";HtgMessage=" + HtgMessage + ";HGExceptionMessage=" + HGExceptionMessage;
                    this.ApLog.dic["UserId"] = usrn;
                    this.ApLog.dic["Result"] = Result.Failure;
                    Log();
                    #region
                    /*
* racf id 錯誤
+                    ["HtgMessage"]    "799 SESSION 建立失敗 (SIGN ON 失敗)"  
+                    ["HGExceptionMessage"]         "01 GN00找不到該USERID"  
+                    ["HtgReturnCode"]         "799"                           
* racf pwd 錯誤
+                    ["HtgMessage"]    "704 SESSION 建立失敗 (RACF Sign on 失敗)"    
+                    ["HGExceptionMessage"]         "密碼錯誤" 
+                    ["HtgReturnCode"]         "704"                            
* 
*/
                    #endregion

                    // 20180606,PeterHsieh,增加記錄RACF登入狀態，避免後續若USER依然強迫登入時，造成系統依然可發查的錯誤狀況
                    Session["RCAFStatus"] = false;
                }
            }
            else
            {
                rtn = "VALID_LDAP_FAILURE";
            }
            return Content(rtn);
            #endregion Legend 2017/10/25 將NUMS中驗證RCAF部分添加
        }


        //20130306 horace
        private object CreateNewUser(string key)
        {
            return new User(key);
        }

        /// <summary>
        /// 綁定參數細項下拉列表
        /// </summary>
        /// <remarks>2012/07/09 Kyle</remarks>
        public void SelectRCAFBranch()
        {
            PARMCodeBIZ biz = new PARMCodeBIZ();
            ViewBag.codeList = new SelectList(biz.GetCodeNoSorted("RCAF_BRANCH"), "CodeNo", "CodeDesc");            
        }

        public ActionResult _Redirect()
        {
            return View();
        }

        //---------------------------------------------
        //設定多國語系
        //---------------------------------------------
        public ActionResult SetCulture(string cultureName)
        {
            if (string.IsNullOrEmpty(cultureName))
                cultureName = Config.GetValue("CUF_CultureName");
            Session["CultureName"] = cultureName;
            SelectCultureName();
            ViewBag.ErrorMessage = "";
            SelectRCAFBranch();
            return View("Index");        
        }

        //取得多國語系下拉清單
        private void SelectCultureName()
        {
            CommonBIZ _commonBIZ = new CommonBIZ();
            ViewBag.CultureName = new SelectList(_commonBIZ.GetCodeData("CULTURE_NAME"), "CodeNo", "CodeDesc", (string)Session["CultureName"]);        
        }

        //20130418 hroace 拋出無法登入之警訊
        private void AlertMsg(string msg, string username)
        {
            ViewBag.ErrorMessage = msg;
            SelectRCAFBranch();
            this.ApLog.Message = msg;
            this.ApLog.dic["UserId"] = username;
            this.ApLog.dic["Result"] = Result.Failure;
            Log();
        }

        private void Log()
        {
            CSFSLogBIZ _csfsLogBIZ = new CSFSLogBIZ();
            this.ApLog.Categories.Add("LogonLogout");
            this.ApLog.Title = "Login";
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

        //--------------------------------
        //處理SecureString vs String
        //--------------------------------
        //將string 轉成 SecurityString
        //20150108 horace 弱掃
        public SecureString ToSecureString(string Source)
        {
            if (string.IsNullOrWhiteSpace(Source))
                return null;
            else
            {
                SecureString Result = new SecureString();
                foreach (char c in Source.ToCharArray())
                    Result.AppendChar(c);
                return Result;
            }
        }

        //將SecurityString 轉成 String
        //20150209 horace 弱掃 暫時不使用,因弱掃不可將SecureString 儲存到不安全的buffer內
        //public String SecureStringToString(SecureString value)
        //{
        //    IntPtr bstr = Marshal.SecureStringToBSTR(value);

        //    try
        //    {
        //        return Marshal.PtrToStringBSTR(bstr);
        //    }
        //    finally
        //    {
        //        Marshal.FreeBSTR(bstr);
        //    }
        //}
    }
}
