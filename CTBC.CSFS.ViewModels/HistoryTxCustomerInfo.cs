using System.Collections.Generic;
using CTBC.CSFS.Models;
namespace CTBC.CSFS.ViewModels
{
    public class HistoryTxCustomerInfo
    {
        public HistoryTX_60491_Grp HistoryTx60491Grp { get; set; }

        public IList<HistroyTX_60491_Detl> HistoryTx60491Detls { get; set; }

        public HistoryTX_67072_Grp HistoryTx67072_Grp { get; set; }

        public IList<HistoryTX_67072_Detl> HistoryTx67072Detls { get; set; }

        //20160122 RC --> 20150115 宏祥 add 新增67100電文
        public HistoryTX_67100 HistoryTx67100 { get; set; }
        public HistoryTX_67002 HistoryTx67002 { get; set; }
        public string ErrMsg { get; set; }
        //ID重號的資料
        public IList<HistroyTX_60491_Detl> HistoryTx60491DetlIdDupDatas { get; set; }
        public HistoryTX_60491_Grp HistoryTx60491_Grp { get; set; }
    }
}