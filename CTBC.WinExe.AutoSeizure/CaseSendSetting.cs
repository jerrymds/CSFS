//------------------------------------------------------------------------------
// <auto-generated>
//     這個程式碼是由範本產生。
//
//     對這個檔案進行手動變更可能導致您的應用程式產生未預期的行為。
//     如果重新產生程式碼，將會覆寫對這個檔案的手動變更。
// </auto-generated>
//------------------------------------------------------------------------------

namespace CTBC.WinExe.AutoSeizure
{
    using System;
    using System.Collections.Generic;
    
    public partial class CaseSendSetting
    {
        public int SerialID { get; set; }
        public Nullable<System.Guid> CaseId { get; set; }
        public string Template { get; set; }
        public string SendWord { get; set; }
        public string SendNo { get; set; }
        public Nullable<System.DateTime> SendDate { get; set; }
        public string Speed { get; set; }
        public string Security { get; set; }
        public string Subject { get; set; }
        public string Description { get; set; }
        public Nullable<int> isFinish { get; set; }
        public Nullable<System.DateTime> FinishDate { get; set; }
        public string Attachment { get; set; }
        public string CreatedUser { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public string ModifiedUser { get; set; }
        public Nullable<System.DateTime> ModifiedDate { get; set; }
        public string SendKind { get; set; }
        public Nullable<System.DateTime> SendUpDate { get; set; }
    }
}
