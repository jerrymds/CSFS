using System.Collections.Generic;
using CTBC.CSFS.Models;
namespace CTBC.CSFS.ViewModels
{
    public class TxCustomerInfo
    {
        public TX_60491_Grp Tx60491Grp { get; set; }

        public IList<TX_60491_Detl> Tx60491Detls { get; set; }

        public TX_67072_Grp Tx67072Grp { get; set; }

        public IList<TX_67072_Detl> Tx67072Detls { get; set; }

        //20160122 RC --> 20150115 宏祥 add 新增67100電文
        public TX_67100 Tx67100 { get; set; }
        public TX_67002 Tx67002 { get; set; }
        public string ErrMsg { get; set; }
        //ID重號的資料
        public IList<TX_60491_Detl> Tx60491DetlIdDupDatas { get; set; }
    }
}