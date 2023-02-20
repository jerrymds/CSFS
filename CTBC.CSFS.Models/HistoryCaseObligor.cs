using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTBC.CSFS.Models
{
    public  class HistoryCaseObligor:Entity
    {
        public int  ObligorId{get;set;}
        public Guid  CaseId{get;set;}
        public string  ObligorName{get;set;}
        public string Nameflag { get; set; }
        public string  ObligorNo{get;set;}
        public string Noflag { get; set; }
        public string  ObligorAccount{get;set;}
        public string Accountflag { get; set; }
        public string CreatedUser {get;set;}
        public DateTime CreatedDate {get;set;}
		//Modified by Jerry for 外來文系統公文資訊/帳務資訊，人工異動需反色提醒 on 2016.12.30 begin
		public bool isUpdate { get; set; }
		public bool isDelete { get; set; }
		public bool isAdd { get; set; }
		//Modified by Jerry for 外來文系統公文資訊/帳務資訊，人工異動需反色提醒 on 2016.12.30 end

        public string ObligorNoAndName { get; set; }
        /// <summary>
        /// ID重號
        /// </summary>
        public string DuplicateID { get; set; }
        /// <summary>
        /// 客戶等級
        /// </summary>
        public string CustomerLevel { get; set; }
        /// <summary>
        /// RM/理專
        /// </summary>
        public string RMcommissioner { get; set; }
    }
}
