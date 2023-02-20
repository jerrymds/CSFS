/// <summary>
/// 程式說明:CSFS Exception物件
/// 維護部門:資訊管理處
/// CSFS Version:v3.0
/// 中國信託銀行 版權所有  ©  All Rights Reserved. 
/// </summary>

using System.Runtime.Serialization;
using System;
using System.Web.Mvc;
using System.Diagnostics;

namespace CTBC.CSFS.Pattern
{
    public class CSFSException : Exception, ISerializable
    {
        public string message = "";
        public string errId = "";
        public string errDesc = "";
        public string errDetail = "";
        public string sverity ="Error";
        public string controller = "";
        public string action = "";

        public CSFSException() { }

        public CSFSException(string message, Exception inner)
            : base(message, inner) { }

        protected CSFSException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        public CSFSException(string errId) {
                this.errId = errId;
                this.message = errId;
        }

        public void GetErrInfo()
        {
            if (string.IsNullOrEmpty(this.Message))
            {
                this.errId = "CSFS-SYS-9999";
                this.errDesc = this.Message;
                this.errDetail = this.Source + " " + this.StackTrace;
            }
            if (this.errId != "CSFS-SYS-9999")
            {
                // 至cache取錯誤代碼的對應訊息
                try
                {
                    this.errDesc = CTBC.CSFS.Pattern.CSFSErrorCode.GetErrDesc(this.errId);
                    this.errDetail = this.Message + " " + this.Source + " " + this.StackTrace;
                }
                catch (Exception exc)
                {
                    if (!string.IsNullOrEmpty(this.errId))
                        this.errDesc = this.errId;
                    else                  
                        this.errDesc = this.Message;
                    this.errId = "CSFS-SYS-9999"; 
                    this.errDetail = this.Source + " " + this.StackTrace;
                }
            }        
        }
    }
}
