using System;

namespace CTBC.CSFS.Models
{
    public class LDAPEmployee
    {
        public string EmpId { get; set; }
        public string EmpName { get; set; }
        public string EmpTitle { get; set; }
        public string EmpBusinessCategory { get; set; }
        public string IsManager { get; set; }
        public string EMail { get; set; }
        public string DepDn { get; set; }
        public string DepId { get; set; }
        public string ManagerId { get; set; }
        public string CreatedUser { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedUser { get; set; }
        public DateTime ModifiedDate { get; set; }

        public string DepName { get; set; }

        public string BranchId { get; set; }
        public string BranchName { get; set; }

        //* 20150618新增電話號碼
        public string TelNo { get; set; }
        public string TelExt { get; set; }


        //*下面幾個欄位是View裏Join出來的
        public string SectionName { get; set; }
        public string UpperDepId { get; set; }
        public string UpDepName { get; set; }
        public string EmpIdAndName { get; set; }
    }
}