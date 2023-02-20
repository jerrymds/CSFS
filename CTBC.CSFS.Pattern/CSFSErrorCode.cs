/// <summary>
/// 程式說明:CSFS ErrorCode物件
/// 維護部門:資訊管理處
/// CSFS Version:v3.0
/// 中國信託銀行 版權所有  ©  All Rights Reserved. 
/// </summary>
/// 
using System;
using System.Collections.Generic;
using CTBC.FrameWork.Platform;

namespace CTBC.CSFS.Pattern
{
    public class CSFSErrorCode
    {
        /// <summary>
        /// 錯誤訊息碼
        /// </summary>
        public string ErrId { get; set; }

        /// <summary>
        /// 錯誤訊息說明
        /// </summary>
        public string ErrDesc { get; set; }

        /// <summary>
        /// 錯誤訊息說明
        /// </summary>
        /// <param name="errId"></param>
        /// <returns></returns>
        public static string GetErrDesc(string errId)
        {
            try {
                string rtn = (string)((Dictionary<string, string>)AppCache.Get("CSFSErrorCode"))[errId];
                if (string.IsNullOrEmpty(rtn))
                    return "No Setting This ErrorDesc. CSFSErrorCode";
                else
                    return rtn;                
            }
            catch(Exception ex) { throw ex; }
        }
    }
}

