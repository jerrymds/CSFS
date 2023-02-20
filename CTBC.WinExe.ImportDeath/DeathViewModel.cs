using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTBC.WinExe.ImportDeath
{
    public class DeathViewModel
    {
        public int Sno { get; set; }
        public string IDNO { get; set; }
        public string Name { get; set; }
        public string BaseDate { get; set; }
        public string SBox_Flag { get; set; }
        public DepositViewModel[] Deposits { get; set; }
        public InvestViewModel[] Invests { get; set; }
        public LoanViewModel[] Loans { get; set; }
    }

    public class DepositViewModel
    {
        public string BANK_NAME {get;set;}
        public string DEP_KIND {get;set;}
        public string ACCT_NO { get; set; }
        public string CURRENCY_TWD { get; set; }
        public string BAL { get; set; }
    }

    public class InvestViewModel
    {
        public string  BANK_NAME { get; set; }
        public string DESCRIPTION { get; set; }
        public string UNIT { get; set; }
        public string CURRENCY { get; set; }
        public string BAL { get; set; }
    }
    public class LoanViewModel
    {
        public string PROD_TYPE { get; set; }
        public string ACC_NO { get; set; }
        public string BAL { get; set; }
    }

}
