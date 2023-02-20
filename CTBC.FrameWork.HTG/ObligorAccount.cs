using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTBC.FrameWork.HTG
{
    public class ObligorAccount
    {
        public string Account { get; set; }
        public string Name { get; set; }
        public string Id { get; set; }
        public string CaseId { get; set; } 
        public bool isHuman { get; set; } // 落人工處理
        public string BranchNo { get; set; }
        public string BranchName { get; set; }
        public bool isDifficultWord { get; set; } // 有難字
        public bool is450OK { get; set; } // 是否事故
        public bool is45031OK { get; set; } // 是否他案扣押
        public string StsDesc { get; set; }
        
        public string AccountStatus { get; set; }
        public string SegmentCode { get; set; }
        public string message450Code { get; set; } // 若有事故的代號
        public string message450 { get; set; } // 若有事故, 的原因
        //public string message45031 { get; set; }
        public bool isOD { get; set; }  // 是否透支 
        public bool isTD { get; set; }  // 是否質借
        public bool isLon { get; set; } // 是否放款
        public int SeizureStatus { get; set; } // 扣押結果, 0: 不扣押, 1: 部分扣押, 2: 全部扣押 , 3: 超額扣押 (即被連結的活存帳號)
        public string AccountType { get; set; } // 活存, 定存, 綜存 .....
        public string Link { get; set; }
        public string LinkAccount { get; set; } // 若有連結帳戶
        public string ProdCode { get; set; } // 產品代碼
        public string ProdDesc { get; set; }
        public string Ccy { get; set; } // 幣別
        public decimal Rate { get; set; } // 匯率
        public decimal Twd { get; set; } // 折合台幣是多少錢
        public decimal Bal { get; set; } // 發401後, 找到的可用餘額
        public decimal planSeizure { get; set; } // 原計劃應該要扣押多少金額
        public decimal realSeizure { get; set; } // 實際扣押到的金額( 活儲 扣押的總金額) , 例如可用餘額10,000 , 但連結定存 共有50,000 ... 則實際要打9093 .. 60,000元
        public decimal showSeizure { get; set; } // 顯示(有連結定存類, 實際打9093(活儲 扣押的總金額) , 但在畫面上, 要連結類, 扣了多少) , 例如前例 , 要留下50,000元
        public int SeizureSeq { get; set; } // 扣押的順序
        public string Memo { get; set; } // 所有要加的備註, 
        public bool noSeizure { get; set; } // 此帳戶不得進行扣押, 原因, 可能是已被定存連結 的活儲, 被他案扣押了.. 就要落人工
        public string CoType { get; set; } // 若是個人: P, 公司: C, 行號: D
        public bool isRename { get; set; } // 本帳戶是否有更名
        public string newName { get; set; } // 若有更名, 新名字
        public bool isSame { get; set; } // 更名後的結果, 是否相同?
        public bool isDoubleAcc { get; set; } // 若是重號, 則為True;

    }
}
