using System;
using System.Collections.Generic;
using System.Threading;
using System.Web;

namespace CTBC.FrameWork.Pattern
{
    /// <summary>
    /// 設定多國語系
    /// 20140808 horace
    /// </summary>
    public class Culture
    {
        //Culture Name
        public static string CULTURE_NAME = (System.Web.Configuration.WebConfigurationManager.AppSettings["CUF_CultureName"] == null ?
                "en-tw" : System.Web.Configuration.WebConfigurationManager.AppSettings["CUF_CultureName"]);

        //---------------------------------------------
        //設定多國語系
        //---------------------------------------------
        /// <summary>
        /// 設定多國語系
        /// </summary>
        /// <param name="cultureName"></param>
        public void SetCulture(string cultureName)
        {
            if (cultureName != "")
                Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(cultureName);
            else
                Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(CULTURE_NAME);
            Thread.CurrentThread.CurrentUICulture = Thread.CurrentThread.CurrentCulture;
        }
        //---------------------------------------------
    }
}
