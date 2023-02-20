using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTBC.CSFS.Models
{
    public  class CaseTrsQueryHistory : Entity
    {
        public string NewID { get; set; }
        public string DocNo { get; set; }
        public string CustAccount { get; set; }

        public string DataBase { get; set; }

        public string Details { get; set; }
        public string ForCDateS { get; set; }
        public string ForCDateE { get; set; }
        public string SendDate { get; set; }
        public string RecvDate { get; set; }
        public string QFileName { get; set; }
        public string QFileName2 { get; set; }
        public string CreatedUser { get; set; }
        public string CreatedDate { get; set; }
        public string ModifiedUser { get; set; }
        public string ModifiedDate { get; set; }

        public string CustId { get; set; }
        public string Status { get; set; }
        public string TrnNum { get; set; }
        public string ESBStatus { get; set; }
        public string FileName { get; set; }
        public string AcctDesc { get; set; }
        public string Curr { get; set; }
        public string Channel { get; set; }

        public Boolean boolean { get; set; }




    }


    //public class CaseTrsQuery : Entity
    //{
    //    public string NewID { get; set; }
    //    public string DocNo { get; set; }
    //    public string CustAccount { get; set; }
    //    public string ForCDateS { get; set; }
    //    public string ForCDateE { get; set; }
    //    public string SendDate { get; set; }
    //    public string RecvDate { get; set; }
    //    public string QFileName { get; set; }
    //    public string QFileName2 { get; set; }
    //    public string CreatedUser { get; set; }
    //    public string CreatedDate { get; set; }
    //    public string ModifiedUser { get; set; }
    //    public string ModifiedDate { get; set; }

    //    public string CustId { get; set; }
    //    public string Status { get; set; }
    //    public string TrnNum { get; set; }
    //    public string ESBStatus { get; set; }
    //    public string FileName { get; set; }
    //    public string AcctDesc { get; set; }
    //    public string Curr { get; set; }
    //    public string Channel { get; set; }

    //    public string Option1 { get; set; }

    //    public string Option2 { get; set; }

    //    public string Option3 { get; set; }


    //}
}
