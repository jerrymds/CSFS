using System;
using System.Configuration;

namespace CTCB.NUMS.Batch.LDAPSync
{
    public static class Config
    {
        public static string AuthMode = ConfigurationManager.AppSettings["AuthenticationMode"];
        public static string LDAPHost = ConfigurationManager.AppSettings["LDAPServer"];
        public static string HrisRoot = ConfigurationManager.AppSettings["LDAPRoot"];
        public static int LDAPPort = Convert.ToInt32(ConfigurationManager.AppSettings["LDAPPort"]);
        public static string APDN = ConfigurationManager.AppSettings["APDN"];
        public static string APID = ConfigurationManager.AppSettings["APID"];
        public static string APPwd = ConfigurationManager.AppSettings["APPwd"];
        public static string CheckRoleLength = ConfigurationManager.AppSettings["CheckRoleLength"];
        public static string RoleLength = ConfigurationManager.AppSettings["RoleLength"];
        public static string SearchFilter = ConfigurationManager.AppSettings["SearchFilter"];
        public static string CSVPath = ConfigurationManager.AppSettings["CSVPath"];
        public static string LogPath = ConfigurationManager.AppSettings["LogPath"];
        public static string MailFromDisplayName = ConfigurationManager.AppSettings["MailFromDisplayName"];
        public static string MailSubject = ConfigurationManager.AppSettings["MailSubject"];
        public static string MailFrom = ConfigurationManager.AppSettings["MailFrom"];
        public static string MailToWho = ConfigurationManager.AppSettings["MailToWho"];
        public static string SMTPServer = ConfigurationManager.AppSettings["SMTPServer"];
        public static string ConnectionString = ConfigurationManager.ConnectionStrings["CSFS_ADO"].ConnectionString;
    }
}