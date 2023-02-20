using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTBC.WinExe.AutoSeizure
{
    public partial class CaseObligor
    {
        public string newName { get; set; } // 若有新名字的話, 填入此
        public bool isChangeName { get; set; } // 有更過名嗎?
        public bool isSame { get; set; } // 更名後的結果, 是否相同?
        public bool isDoubleAcc { get; set; } //若是重號, 則為True;
        public string type { get; set; } // P:個人, C:公司: D:行號
        public string RespName { get; set; } // 若是行號, 則填入負責人姓名
        public string RespId { get; set; } // 若是行號, 則填入負責人ID
        public bool isRespInfoSame { get; set; } // 行號負責人與來文, 均相同
        public bool isIDSame { get; set; } // 行號, 來文ID與主機相同
        public bool isNameSame { get; set; } // 行號, 來文NAME與主機相同
        public string docName { get; set; } //行號 來文的姓名
        public string docId { get; set; } // 行號, 來文的ID
        public string docMemo { get; set; } // 各別義務人的訊息.. 用@分開
        public bool isDiffWord { get; set; } //是否有難字
    }
}
