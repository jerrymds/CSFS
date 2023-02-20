using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTBC.WinExe.WarningReturnFile.Model
{
    public class wd
    {
        public int SerialID { get; set; }
        public string  DocNo { get; set; }
        public DateTime  HappenDateTime { get; set; }
        public string  No_165 { get; set; }
        public string  No_e { get; set; }
        public string  NotificationContent { get; set; }
        public string  NotificationSource { get; set; }
        public DateTime  ForCDate { get; set; }
        public DateTime  EtabsDatetime { get; set; }
        public string  NotificationUnit { get; set; }
        public string  NotificationName { get; set; }
        public string  ExtPhone { get; set; }
        public string  PoliceStation { get; set; }
        public string  VictimName { get; set; }
        public string  CreatedUser { get; set; }
        public DateTime  CreatedDate { get; set; }
        public string  ModifiedUser { get; set; }
        public DateTime  ModifiedDate { get; set; }
        public string  DocAddress { get; set; }
        public string  Original { get; set; }
        public string  StateType { get; set; }
        public string  StateCode { get; set; }
        public string  Status { get; set; }
        public string  UniteNo { get; set; }
        public DateTime  UniteDate { get; set; }
        public string  Retry { get; set; }
        public string  ExtendNo { get; set; }
        public string  Extend { get; set; }
        public DateTime  ExtendDate { get; set; }
        public string  FIX { get; set; }
        public string  FIXSEND { get; set; }
        public string  Set { get; set; }
        public DateTime  SetDate { get; set; }
        public string  Kind { get; set; }
        public string  UniteNo_Old { get; set; }
        public string  UniteDate_Old { get; set; }
        public string  Flag_909113 { get; set; }
        public string  Release { get; set; }
        public DateTime  ReleaseDate { get; set; }
        public Guid  NewId { get; set; }
        public Guid? CaseId { get; set; }
    }
}
