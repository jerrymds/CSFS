using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTBC.WinExe.CSFS.FTPdown
{
    public class batchmetadata
    {
        public string CompanyId { get; set; }
        public string BatchNo { get; set; }
        public DateTime BatchDate { get; set; }
        public int TransferType { get; set; }
        public int TotalNumber { get; set; }
        public string[] CnoNoInfo { get; set; }
    }
    public class metadata
    {
        public string CnoNo { get; set; }
        public File[] Files { get; set; }
        public string CaseKind { get; set; }
    }

    public class File
    {
        public string FileType { get; set; }
        public string FileName { get; set; }
    }
}
