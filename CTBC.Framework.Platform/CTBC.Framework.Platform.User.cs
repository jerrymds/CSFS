using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;
using System.Web;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Security;//20150108 horace 弱掃
using System.Runtime.InteropServices;//20150108 horace 弱掃

namespace CTBC.FrameWork.Platform
{
    [Serializable]
    public class User
    {
        //---------------------------------------
        //			Enumeration
        //---------------------------------------
         [Serializable]
        public enum UserRoleMode { Single, Combined }

        //---------------------------------------
        //			Structure
        //---------------------------------------
        [Serializable]
        public struct RoleInfo
        {
            //LDAP DN(ex:cn=CSFSM0001,ou=CSFS,ou=APPs,o=CTCB)
            public string RoleId;

            //LDAP角色代號(ex:CSFSM0001)
            public string RoleLDAPId;

            //LDAP角色名稱(ex:系統管理者)
            public string RoleName;
        }
        //
        //---------------------------------------
        //			Properties
        //---------------------------------------
        public string Account = string.Empty;
        public string Name = string.Empty;
        public string Country = string.Empty;
        public string BU = string.Empty;
        public string Mail = "";
        public bool Authenticated = true;
        public string RCAFAccount = string.Empty;
        public string RCAFPs = string.Empty;
        public string RCAFBranch = string.Empty;
        public string LDAPPwd = string.Empty;
        public string COLevel = string.Empty;//2013/2/20 horace :user所屬CO層級
        public int COlevelSeqNo = -1;//20130627 horace =>CSFSAuthLevel.Seq
        
        //* Add By Ge.Song start
        public string DepId = "";
        public string DepName = "";
        public string BranchId = "";
        public string BranchName = "";

        public bool IsInEmployeeView= false;
        public string UnitForKeyIn = "";
        //* Add By Ge.Song end

        //使用者目前所使用的角色名稱
        public string ActiveRole = string.Empty;

        //使用者是採還併所有角色權限/或是單一角色權限登入系統
        public UserRoleMode RoleMode = UserRoleMode.Combined;

        //使用者被授權的LDAP角色清單,以字串格式儲存
        public string RoleCodes = "";

        //使用者被授權的LDAP角色清單,以List<RoleInfo>格式儲存
        public List<RoleInfo> Roles = new List<RoleInfo>();

        //使用者認證LDAP物件
        [NonSerialized]
        public LDAP myLDAP = null;

        public string SessionID = string.Empty;
        public string SessionIP = string.Empty;
        public DateTime SessionLoginTime;
        public DateTime LastActiveTime;//20130313 horace user最後執行Action時間

        //---------------------------------------
        //			Constructors
        //---------------------------------------          
        //public User(string psAccount, bool pbAuthenticate = false, string psPin = "", string rcafAccount = "", string rcafPs = "", string rcafbrh = "")
        public User(string psAccount, bool pbAuthenticate = false, SecureString secpsPin=null, string rcafAccount = "", SecureString secrcafPs=null, string rcafbrh = "")
        {
            //---------------------------
            //解開SecureString to String
            //---------------------------
            //20150108 horace 弱掃
            string psPin = "";
            string rcafPs = "";
            if (secpsPin != null)
                psPin = SecureStringToString(secpsPin);
            if (secrcafPs != null)
                rcafPs = SecureStringToString(secrcafPs);
            //---------------------------
            //解開SecureString to String
            //---------------------------

            // determine role mode
            if (UserBaseInfo.CUF_UserRoleMode != null) 
                RoleMode = (UserBaseInfo.CUF_UserRoleMode == "C" ? UserRoleMode.Combined : UserRoleMode.Single);

            // 2. get user profile

            // initialize LDAP connection
            myLDAP = new LDAP();//20140103 horace

            // get user DN
            Authenticated = myLDAP.AuthenticateUser(psAccount, psPin);//20140103 horace

            // load user profile and roles if authenticated
            if (Authenticated)
            {

                // load user properties to user profile
                Account = psAccount;
                Name = myLDAP.UserName;//20140103 horace
                Mail = myLDAP.UserMail;//20140103 horace
                Country = "";
                BU = UserBaseInfo.CUF_CompanyNum;
                RCAFAccount = rcafAccount;
                RCAFPs = rcafPs;
                RCAFBranch = rcafbrh;
                LDAPPwd = psPin;

                //取得CSFS所有的角色清單XML格式
                string AppRoles = "";
                if (CTBC.FrameWork.Platform.AppCache.InCache("AppRoles"))
                    AppRoles = (string)CTBC.FrameWork.Platform.AppCache.Get("AppRoles");
                else
                    AppRoles = "";
                XmlDocument AppRole = new XmlDocument();
                AppRole = XML.Create_LoadXML(AppRoles);

                //取得使用者目前被授權的LDAP角色,以物件格式Roles存放
                //組合使用者被授權的LDAP角色,以字串格式RoleCodes存
                string[] la_Role = myLDAP.UserGroup;//20140103 horace//myLDAP.GetUserRoles(UserDN);
                if (la_Role != null)
                {
                    foreach (string ls_Role in la_Role)
                    {
                        RoleCodes += ";" + ls_Role;
                        RoleInfo lo_NewRole = new RoleInfo();
                        lo_NewRole.RoleId = ls_Role;
                        lo_NewRole.RoleLDAPId = GetRoleLDAPID(ls_Role);
                        XmlNode lo_Node = AppRole.DocumentElement.SelectSingleNode("*[@Code='" + ls_Role + "']");
                        lo_NewRole.RoleName = (lo_Node == null ? ls_Role : lo_Node.InnerText);
                        Roles.Add(lo_NewRole);
                    }
                    if (RoleCodes.Length > 0) RoleCodes = RoleCodes.Substring(1);
                    // determine active role
                    if (ActiveRole == "" && Roles.Count > 0)
                        ActiveRole = Roles[0].RoleName;
                }
                else {
                    //無CSFS角色,則不可登入系統
                    RoleCodes = "";//20140103 horace
                    Roles = null;
                    ActiveRole = "";
                }
            }
        }

        //---------------------------------------
        //			Public Method
        //---------------------------------------

        //取得LDAP Role ID 
        public string GetRoleLDAPID(string roleCode)
        {
            string ldapRoleId = "";
            if (roleCode != "")
            {
                string[] lpRole = roleCode.Split(',');
                if (lpRole[0].Length > 0)
                {
                    string[] ldapRole = lpRole[0].Split('=');
                    if (ldapRole.Length > 0)
                    {
                        ldapRoleId = ldapRole[1];
                    }
                }
            }
            return ldapRoleId;
        }

        // check function actions against codes
        public bool IsAuthorized(string functionID, string itemID, string codes)
        {
            // get action codes
            string ls_Actions = GetAuthZActions(functionID, itemID);
            foreach (char c in codes)
                if (ls_Actions.IndexOf(c) >= 0) return true;
            return false;
        }

        // get item actions authorized to this user
        public string GetAuthZActions(string functionID, string itemID)
        {
            // Single role mode
            if (this.RoleMode == UserRoleMode.Single)
                return this.GetAuthZActionsByRole(functionID, itemID, this.ActiveRole);

            // Combined role mode
            string ls_Actions = "";
            foreach (RoleInfo R in this.Roles)
                ls_Actions += this.GetAuthZActionsByRole(functionID, itemID, R.RoleId);
            return ls_Actions;
        }

        // get specified role action codes
        public string GetAuthZActionsByRole(string functionID, string itemID, string roleID)
        {
            //取得CSFS所有AuthZ授權與Menu資料,以XML格式
            string AppAuthZ = "";
            if (CTBC.FrameWork.Platform.AppCache.InCache("AppAuthZ"))
                AppAuthZ = (string)CTBC.FrameWork.Platform.AppCache.Get("AppAuthZ");
            else
                AppAuthZ = "";
            XmlDocument AuthZDom = null;
            AuthZDom = XML.Create_LoadXML(AppAuthZ);
            
            XmlNode lo_Node = AuthZDom.DocumentElement.SelectSingleNode("//*[@md_FuncID='" + functionID + "']");
            if (lo_Node == null) return "";
            string ls_XML = XML.GetAttribute(lo_Node, "md_AuthZ");
            XmlDocument lo_DOM = XML.Create();
            try { lo_DOM.LoadXml(HttpUtility.UrlDecode(ls_XML)); }
            catch (Exception) { return ""; }

            lo_Node = lo_DOM.DocumentElement.SelectSingleNode("*[Role='" + roleID + "']");
            if (lo_Node == null) return "";
            return XML.GetNodeValue(lo_Node, "Actions");
        }

        /// <summary>
        /// 登入者是否擁有某個roleID的權限
        /// </summary>
        /// <param name="roleID"></param>
        /// <returns>true=擁有某個roleID的權限/false</returns>
        /// 20130815 horace
        public bool IsExistAppRole(string roleID)
        {
            bool rtn = false;
            List<RoleInfo> list = Roles;
            if (list != null)
            {
                if (list.Count != 0)
                {
                    roleID = roleID.Trim();
                    foreach (RoleInfo item in list)
                    {
                        if (item.RoleId.Contains(roleID))
                        {
                            rtn = true;
                            break;
                        }
                    }
                }
            }
            return rtn;
        }

        //---------------------------------------
        //			Private Method
        //---------------------------------------

        private string AppendFolderChar(string psPath)
        {
            int ln_Length = psPath.Length;
            string ls_NewPath = psPath;
            if (ln_Length <= 0) return ls_NewPath;
            string ls_LastChar = psPath.Substring(ln_Length - 1);
            if (ls_LastChar != "\\" && ls_LastChar != "/") ls_NewPath += "\\";
            return ls_NewPath;
        }

        //將SecurityString 轉成 String
        //20150108 horace 弱掃
        public String SecureStringToString(SecureString value)
        {
            IntPtr bstr = Marshal.SecureStringToBSTR(value);

            try
            {
                return Marshal.PtrToStringBSTR(bstr);
            }
            finally
            {
                Marshal.FreeBSTR(bstr);
            }
        }
    }

    public static class UserBaseInfo
    { 
        public static string CUF_UserRoleMode = (System.Web.Configuration.WebConfigurationManager.AppSettings["CUF_UserRoleMode"] == null ?
                "C" : System.Web.Configuration.WebConfigurationManager.AppSettings["CUF_UserRoleMode"]);
        public static string CUF_AppName = (System.Web.Configuration.WebConfigurationManager.AppSettings["CUF_AppName"] == null ?
                "CSFS" : System.Web.Configuration.WebConfigurationManager.AppSettings["CUF_AppName"]);
        public static string CUF_AuthZDatabase = (System.Web.Configuration.WebConfigurationManager.AppSettings["CUF_AuthZDatabase"] == null ?
                "" : System.Web.Configuration.WebConfigurationManager.AppSettings["CUF_AuthZDatabase"]);
        public static string CUF_CompanyNum = (System.Web.Configuration.WebConfigurationManager.AppSettings["CUF_CompanyNum"] == null ?
                "CTCB" : System.Web.Configuration.WebConfigurationManager.AppSettings["CUF_CompanyNum"]);
    }
}
