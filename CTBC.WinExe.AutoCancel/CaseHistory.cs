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
    
    public partial class CaseHistory
    {
        public int HistoryId { get; set; }
        public System.Guid CaseId { get; set; }
        public string FromRole { get; set; }
        public string FromUser { get; set; }
        public string FromFolder { get; set; }
        public string Event { get; set; }
        public Nullable<System.DateTime> EventTime { get; set; }
        public string ToRole { get; set; }
        public string ToUser { get; set; }
        public string ToFolder { get; set; }
        public string CreatedUser { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
    }
}
