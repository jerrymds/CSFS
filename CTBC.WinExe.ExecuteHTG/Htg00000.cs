using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExcuteHTG
{
    public partial class Htg00000 : HtgXmlPara
    {
        #region 全域變數



        #endregion

        #region 私域變數

        #endregion

        public override string GetPostTransactionXml()
        {
            string strxml = @"<?xml version=""1.0""?>
                                <hostgateway>
                                  <header>
                                    <action type=""transaction""/>
                                    <data id=""application"" value=""CSFS""/>
                                    <data id=""luType"" value=""LU62""/>
                                    <data id=""transactionId"" value=""000417""/>
                                    <data id=""sessionId"" value=""#sessionId#""/>
                                    <data id=""lowValue"" value=""""/>
                                  </header>
                                  <body>
                                    <data value=""CSFS"" id=""applicationId""/>
                                    <data value="""" id=""ACCT_NO""/>
                                    <data value=""0"" id=""STS_SEL""/>
                                  </body>
                                </hostgateway>";



            return strxml;
        }
        /// <summary>
        /// 是否sing on lu0,062072要sign lu0
        /// </summary>
        /// <returns></returns>
        public override bool CheckIsSignOnlu0()
        {
            return true;
        }
    }
}
