using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTBC.CSFS.BackupHistory
{
    public class BackupSetting
    {
        public int Id { get; set; }
        public string TableName { get; set; }
        public string DateField { get; set; }
        public string IgnoreField1 { get; set; }
        public string IgnoreField2 { get; set; }
        public string IgnoreField3 { get; set; }
        public int SortOrder { get; set; }
        public bool Enable { get; set; }
        public bool isMove { get; set; }
        public bool isDelete { get; set; }
        public int GroupNo { get; set; }
        public string TimeFreq { get; set; }
        public int TimeFreqValue { get; set; }
        public DateTime LastBackupDate { get; set; }
    }
}
