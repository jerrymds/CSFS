/// <summary>
/// 程式說明:CSFS Log物件
/// 維護部門:資訊管理處
/// CSFS Version:v3.0
/// 中國信託銀行 版權所有  ©  All Rights Reserved. 
/// </summary>

using System;
using System.Data;
using System.Web;
using System.Web.Mvc;
using System.Collections.Generic;
using Microsoft.Practices.EnterpriseLibrary.Logging;

namespace CTBC.CSFS.Pattern
{
    public struct ActionCode
    {
        public static string Update = "U";
        public static string Delete = "D";
        public static string Create = "C";
        public static string Approve = "A";
        public static string Rejcect = "R";
        public static string View = "V";
        public static string Login = "I";
        public static string Logout = "O";
        public static string Error = "E";
        public static string Eexc = "EXE";
    }

    public struct TranFlag
    {
        public static string After = "AF";
        public static string Parameter = "PARAM";
        public static string Before = "BF";
    }

    public struct Result
    {
        public static string Success = "S";
        public static string Failure = "F";
    }

    public class CSFSLog : LogEntry
    {
        public IDictionary<string, object> dic = new Dictionary<string, object>();
        public ActionExecutedContext context { get; set; }
        public CSFSLog()
        {
            this.dic.Add("UserId", "");
            this.dic.Add("Result", "");
            this.dic.Add("ActionCode", "");
            this.dic.Add("TranFlag", "");
            this.dic.Add("FunctionId", "");
            this.dic.Add("SessionId", "");
            this.dic.Add("URL", "");
            this.dic.Add("IP", "");
            this.dic.Add("MachineName", "");
        }

        /// <summary>
        /// 處理APLOG issue
        /// 20150729
        /// </summary>
        public void PersonalProcessLog(string LoginUser, string ControllerName, string ActionName, string IP, System.Collections.Specialized.NameValueCollection Parameters)
        {
            try
            {
                string _user = LoginUser;
                string _controller = (string.IsNullOrEmpty(ControllerName)) ? "" : ControllerName;
                string _action = (string.IsNullOrEmpty(ActionName)) ? "" : ActionName;
                string _ip = IP;
                System.Collections.Specialized.NameValueCollection _parameters = Parameters;
                //List<string> PositiveCAlist = Config.APLOGCONTROLLERLIST; //要處理的CONTEOLLER + ACTION (一般來說是針對Main Page, 搜尋頁面應該是針對Result Page)
                //List<string> Paramlist = Config.APLOGPARMLIST; //排掉不要記錄的Parameter(有些值太長的參數 就不要記了)
               
                List<string> PositiveCAlist = GetConfigInfo("APLOGCONTROLLER");//要處理的CONTEOLLER + ACTION (一般來說是針對Main Page, 搜尋頁面應該是針對Result Page)
                List<string> Paramlist = GetConfigInfo("APLOGPARM");//排掉不要記錄的Parameter(有些值太長的參數 就不要記了)

                string CA = _controller + "." + _action;

                if (PositiveCAlist.Contains(CA.ToUpper())) //如果有値 就代表要記錄
                {
                    System.Text.StringBuilder _sb = new System.Text.StringBuilder(); //組Paramters字串

                    //List<string> CUSIDlist = Config.APLOGCUSIDLIST; //要另外存的CUSID
                    //List<string> APPLNOlist = Config.APLOGAPPLNOLIST; //要另外存的APPLNO

                    List<string> CUSIDlist = GetConfigInfo("APLOGCUSID"); //要另外存的CUSID
                    //List<string> CASENOlist = GetConfigInfo("APLOGCASENO");//要另外存的CASENO

                    string CUSID = "";
                    //string CASENO = "";
                    foreach (string key in _parameters.AllKeys)
                    {
                        if (!Paramlist.Contains(key.ToUpper())) //沒有含在要排掉的參數表裏  就是要記錄
                        {
                            string value = _parameters[key];
                            _sb.Append(key.ToUpper() + "=" + value + "|");
                        }

                        _sb = _sb.Replace(',', '.'); //Replace逗號

                        if (CUSIDlist.Contains(key.ToUpper()) && string.IsNullOrEmpty(CUSID)) //有含在CUSID裏 且CUSID等於空値
                        {
                            CUSID = _parameters[key];
                        }

                        //if (CASENOlist.Contains(key.ToUpper()) && string.IsNullOrEmpty(CASENO)) //有含在CASENO裏 且CASENO等於空値
                        //{
                        //    CASENO = _parameters[key];
                        //}

                    }
                    BaseBusinessRule _business = new BaseBusinessRule();
                    LoginUser = LoginUser.Substring(0, (LoginUser.Length > 20) ? 20 : LoginUser.Length);
                    ControllerName = ControllerName.Substring(0, (ControllerName.Length > 50) ? 50 : ControllerName.Length);
                    ActionName = ActionName.Substring(0, (ActionName.Length > 50) ? 50 : ActionName.Length);
                    CUSID = CUSID.Substring(0, (CUSID.Length > 11) ? 11 : CUSID.Length);
                    //CASENO = CASENO.Substring(0, (CASENO.Length > 15) ? 15 : CASENO.Length);
                    if (CUSID.Length > 0)
                    {
                        _business.ExecuteAPLOGSave(LoginUser, ControllerName, ActionName, _ip, _sb.ToString(), CUSID);
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }

        public List<string> GetConfigInfo(string strparm)
        {
            string sql = " SELECT CodeDesc FROM PARMCode WHERE CodeType='" + strparm + "' ";

            DataTable dt   = new BaseBusinessRule().Search(sql);

            List<string> list = new List<string>();
            if (dt != null && dt.Rows.Count > 0)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    list.Add(dr["CodeDesc"].ToString().ToUpper());
                }
                return list;
            }
            else
            {
                return new List<string>();
            }
        }
    }
}
