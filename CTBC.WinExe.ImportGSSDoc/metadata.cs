using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTBC.WinExe.ImportGSSDoc
{

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
