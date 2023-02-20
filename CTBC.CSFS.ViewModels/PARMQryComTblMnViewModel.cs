using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;

namespace CTBC.CSFS.ViewModels
{
    public class PARMQryComTblMnViewModel
    {
        // 輸入的QryString語句
        public string QryCondition { get; set; }

        // 查詢結果Table
        public DataTable ResultList { get; set; }

        // 更新刪除及insert的執行結果
        public string QryResult { get; set; }
    }
}