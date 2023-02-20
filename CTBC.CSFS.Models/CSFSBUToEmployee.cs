namespace CTBC.CSFS.Models
{
    public class CSFSBUToEmployee : Entity
    {
        //BU ID
        public int BUID { get; set; }

        ///add by mel 20130911
        public int UID { get; set; }

        //員編
        public string EmpID { get; set; }

        //BUMaster ID
        public string BUMasterID { get; set; }

        //核准層級(變簽)
        public string ApprLevel { get; set; }

        //員編
        public string EmpName { get; set; }

        //全部組織名稱
        public string BuNameList { get; set; }

        public bool Enable { get; set; }

        public string BUNumber { get; set; }
        public string BUName { get; set; }
        public int BUParent { get; set; }
        public bool Node { get; set; }
        public int Sort { get; set; }
        public int CteLevel { get; set; }
        public string CteSort { get; set; }
        public string CteSortName { get; set; }

        //各層級BUID
        public string Level1BUID { get; set; }
        public string Level2BUID { get; set; }
        public string Level3BUID { get; set; }
        public string Level4BUID { get; set; }

        //EmployeeToRole
        public string RoleID { get; set; }
        public string RoleName { get; set; }
        public string BUBoss { get; set; }
        public string BUBossName { get; set; }
        public int RowNum { get; set; }

        //新增日期：2014/2/7
        //新增人員：莊筱婷
        public string BUDivType { get; set; }

    }
}