//------------------------------------------------------------------------------
// <auto-generated>
//     這個程式碼是由範本產生。
//
//     對這個檔案進行手動變更可能導致您的應用程式產生未預期的行為。
//     如果重新產生程式碼，將會覆寫對這個檔案的手動變更。
// </auto-generated>
//------------------------------------------------------------------------------

namespace CTBC.WinExe.AutoCancel
{
    using System;
    using System.Collections.Generic;
    
    public partial class BatchQueue
    {
        public long Id { get; set; }
        public Nullable<System.Guid> CaseId { get; set; }
        public string SendUser { get; set; }
        public string DocNo { get; set; }
        public string ObligorNo { get; set; }
        public string ServiceName { get; set; }
        public Nullable<System.DateTime> SendDate { get; set; }
        public Nullable<int> Status { get; set; }
        public string ErrorMsg { get; set; }
        public Nullable<System.DateTime> CreateDatetime { get; set; }
    }
}
