using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTBC.CSFS.BackupHistory
{
    public class CaseMoveLog
    {
        public int Id { get; set; }
        public string CaseNo { get; set; }
        public Guid Caseid { get; set; }
        public Guid? PayCaseid { get; set; }
        public Guid? CancelCaseid { get; set; }
        public int MoveType { get; set; }
        public string MoveMessage { get; set; }
        public DateTime CaseCreateDate { get; set; }
        public DateTime? MoveDate { get; set; }
    }
}
