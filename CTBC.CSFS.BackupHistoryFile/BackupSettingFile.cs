using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTBC.CSFS.BackupHistoryFile
{
    public class BackupSettingFile
    {

        public int Id { get; set; }
        public string DirPath { get; set; }
        public bool Enable { get; set; }
        public bool isDelete { get; set; }
        public string TimeFreq { get; set; }
        public int TimeFreqValue { get; set; }
    }
}
