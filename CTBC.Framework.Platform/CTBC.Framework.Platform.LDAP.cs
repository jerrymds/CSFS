using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration.ContainerModel;
using Novell.Directory.Ldap;

namespace CTBC.FrameWork.Platform
{
    public class LDAP
    {
        //---------------------------------------
        //			Properties
        //---------------------------------------
        private string _usrName = "";
        private string _usrDN = "";
        private string _usrMail = "";
        private string[] _usrGroup;

        //---------------------------------------
        //			Constructors
        //---------------------------------------
        public LDAP()
        {
            //* UseLdapCheck 不為1 則不用連Ldap.固定返回成功
            if (LDAPInfo.UseLdapCheck != "1")
            {
                _usrName = "林大仁";
                _usrDN = "";
                _usrMail = "John.Chang@ctbcbank.com";
                _usrGroup = new string[1] { "cn=CSFS001,ou=CSFS,ou=APPs,o=CTCB" };
            }
        }

        public string UserName { get { return _usrName; } }

        public string UserDN { get { return _usrDN; } }

        public string UserMail { get { return _usrMail; } }

        public string[] UserGroup { get { return _usrGroup; } }

        //---------------------------------------
        //			Public Method
        //---------------------------------------
		// authenticate user password
        /// <summary>
        /// 認證使用者LDAP帳號/密碼
        /// </summary>
        /// <param name="as_UserID">使用者登入帳號</param>
        /// <param name="as_UserPWD">使用者登入密碼</param>
        /// <returns></returns>
        public bool AuthenticateUser(string as_UserID, string as_UserPWD)
		{
            //* UseLdapCheck 不為1 則不用連Ldap.固定返回成功
            if (LDAPInfo.UseLdapCheck != "1")
                return true;

            #region 實際Ldap檢核
            try
            {
                bool _valid = false;
                string[] userDNInfo = new string[2] { "", "(cn=" + as_UserID + ")" };//{userDN,searchFilter)
                Dictionary<string, string> attrs = new Dictionary<string, string>();
                LdapConnection conn = new LdapConnection();
                try
                {
                    //Step1:get LDAP connection
                    conn.Connect(LDAPInfo.IP, LDAPInfo.Port);

                    //Step2:verify AP ID and AP password
                    conn.Bind(LDAPInfo.ServiceDN, LDAPInfo.ServicePWD);//(appDN, APPWD)

                    //Step3:search user DN
                    LdapSearchResults lsc = conn.Search(LDAPInfo.RootBaseDN, LdapConnection.SCOPE_SUB, userDNInfo[1], null, false);
                    LdapEntry userLdapEntry = null;
                    while (lsc.hasMore())
                    {
                        //LdapEntry nextEntry = null;
                        try
                        {
                            userLdapEntry = lsc.next();
                        }
                        catch (LdapException ex)
                        {
                            throw new Exception(ex.Message);
                        }
                        userDNInfo[0] = userLdapEntry.DN.Trim();
                    }
                    //Step4:verify user password
                    //LdapAttribute attr = new LdapAttribute("userPassword", user.UserPwd);
                    if (!string.IsNullOrEmpty(userDNInfo[0]))
                    {
                        LdapAttribute attr = new LdapAttribute("userPassword", as_UserPWD);
                        _valid = conn.Compare(userDNInfo[0], attr);
                        //CUF_UserDN = userDNInfo[0];
                    }

                    if (_valid)
                        GetLDAPGroups(userLdapEntry);
                    return _valid;
                }
                catch (Exception ex)
                {
                    string eee = ex.Message;
                    throw new Exception("LDAP connect failure!! ");
                }
                finally
                {
                    //Step6:close LDAP connection 
                    conn.Disconnect();
                }
            }
            catch { return false; }
            #endregion
        }

        /// <summary>
        /// 取得使用者在CSFS角色清單
        /// </summary>
        /// <param name="entry"></param>
        private void GetLDAPGroups(LdapEntry entry)
        {
            //* UseLdapCheck 不為1 則不用連Ldap.固定返回成功
            if (LDAPInfo.UseLdapCheck != "1")
                return;

            #region
            LdapAttributeSet attributeSet = entry.getAttributeSet();
            int c = 0;
            string[] tmpUsrGroup;
            _usrGroup = null;
            List<string> tGpList = new List<string>();
            foreach (LdapAttribute attribute in attributeSet)
            {
                switch (attribute.Name)
                {
                    case "fullName":
                        _usrName = attribute.StringValueArray[0];
                        c++;
                        break;
                    case "mail":
                        _usrMail = attribute.StringValueArray[0];
                        c++;
                        break;
                    case "groupMembership":
                        tmpUsrGroup = attribute.StringValueArray;//ex:cn=CSAPP01,ou=APPTL,ou=APPs,o=CTCB 
                        foreach (string roleDN in tmpUsrGroup)
                        {
                            //只取CSFS系統的角色清單
                            if (roleDN.Trim().Contains(LDAPInfo.ServiceDN))
                            {
                                tGpList.Add(roleDN);
                            }
                        }
                        if (tGpList.Any())
                            _usrGroup = tGpList.ToArray();
                        else
                            _usrGroup = new string[1] { "" };
                        c++;
                        break;
                    default:
                        break;
                }
                if (c == 3) break;//已取得fullName與groupMembership,直接跳離此while loop
            }
        }
        #endregion
    }

    /// <summary>
    /// 取得LDAP連線資訊
    /// </summary>
    public static class LDAPInfo
    {
        public static string IP = (System.Web.Configuration.WebConfigurationManager.AppSettings["LDAPServerIP"] == null ?
                    "" : System.Web.Configuration.WebConfigurationManager.AppSettings["LDAPServerIP"]);
        public static int Port = (short)(System.Web.Configuration.WebConfigurationManager.AppSettings["LDAPServerPort"] == null ?
                    0 : Convert.ToInt32(System.Web.Configuration.WebConfigurationManager.AppSettings["LDAPServerPort"]));
        public static string RootBaseDN = (System.Web.Configuration.WebConfigurationManager.AppSettings["LDAPRootBaseDN"] == null ?
                    "" : System.Web.Configuration.WebConfigurationManager.AppSettings["LDAPRootBaseDN"]);
        public static string ServiceDN = (System.Web.Configuration.WebConfigurationManager.AppSettings["LDAPServiceDN"] == null ?
                    "" : System.Web.Configuration.WebConfigurationManager.AppSettings["LDAPServiceDN"]);
        public static string ServicePWD = (System.Web.Configuration.WebConfigurationManager.AppSettings["LDAPServicePWD"] == null ?
                    "" : System.Web.Configuration.WebConfigurationManager.AppSettings["LDAPServicePWD"]);
        public static string RoleCodeFile = (System.Web.Configuration.WebConfigurationManager.AppSettings["LDAPRoleCodeFile"] == null ?
                    "" : System.Web.Configuration.WebConfigurationManager.AppSettings["LDAPRoleCodeFile"]);

        //* 20150428 Add by Ge.Song Start
        public static string UseLdapCheck = (System.Web.Configuration.WebConfigurationManager.AppSettings["UseLdapCheck"] == null ?
                    "" : System.Web.Configuration.WebConfigurationManager.AppSettings["UseLdapCheck"]);
        //* 20150428 Add by Ge.Song End

    }
}
