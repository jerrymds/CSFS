using System.Collections.Generic;
using CTBC.CSFS.Models;

namespace CTBC.CSFS.ViewModels
{
    public class CaseSeizureViewModel
    {
        public CaseMaster CaseMaster { get; set; }
        public List<CaseMaster> CaseMasterlistO { get; set; }
        public CaseObligor CaseObligor { get; set; }
        public List<CaseObligor> CaseObligorlistO { get; set; }
        public CaseAttachment CaseAttachment { get; set; }
        public List<CaseAttachment> CaseAttachmentlistO { get; set; }

        public CaseEdocFile CaseEdocFile { get; set; }
        public List<CaseEdocFile> CaseEdocFilelist { get; set; }

        public CaseSendSettingDetails CaseSendSettingDetails { get; set; } 
        public IList<CaseMaster> CaseMasterlist { get; set; }
        public IList<CaseSendSettingDetails> CaseSendSettingDetailsList { get; set; }

        public IList<CaseSeizure> CaseSeizure { get; set; }
        public IList<CaseSeizure> CaseSeizureList { get; set; }
    }
}
