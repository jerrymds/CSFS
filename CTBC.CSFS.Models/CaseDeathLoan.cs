using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTBC.CSFS.Models
{
    public class CaseDeathLoan
    {
        public int Sno{get;set;} 
        public string IDHIS_IDNO_ACNO{get;set;} 
        public string UTIDNO{get;set;} 
        public string ACC_NO{get;set;} 
        public string BANK{get;set;} 
        public string BASE_DATE{get;set;} 
        public string PROD_TYPE{get;set;} 
        public string CONTRACT_S_DT{get;set;} 
        public string CONTRACT_E_DT{get;set;} 
        public string USE_TYPE{get;set;} 
        public string BAL{get;set;} 
        public Guid CaseDeadVersionNewID{get;set;} 
        public string CaseNo{get;set;} 
        public int Seq{get;set;} 
        public DateTime CreatedDate  {get;set;} 
    }
}
