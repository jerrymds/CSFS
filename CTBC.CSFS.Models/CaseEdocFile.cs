using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTBC.CSFS.Models
{
    public class CaseEdocFile
    {
        /// <summary>
        /// 案件編號
        /// </summary>
        public Guid CaseId { get; set; }
        /// <summary>
        /// 收發類別
        /// </summary>
        public string Type { get; set; }
        /// <summary>
        /// 檔案類型
        /// </summary>
        public string FileType { get; set; }
        /// <summary>
        /// 檔案名稱
        /// </summary>
        public string FileName { get; set; }
        /// <summary>
        /// 實體檔案
        /// </summary>
        public byte[] FileObject { get; set; }
        /// <summary>
        /// 發文號
        /// </summary>
        public string SendNo { get; set; }
    }
}
