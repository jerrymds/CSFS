using System.Collections.Generic;
using CTBC.CSFS.Models;

namespace CTBC.CSFS.ViewModels
{
    public class HistoryCaseSeizureViewModel
    {
        public HistoryCaseMaster HistoryCaseMaster { get; set; }
        public List<HistoryCaseMaster> HistoryCaseMasterlistO { get; set; }
        public HistoryCaseObligor HistoryCaseObligor { get; set; }
        public List<HistoryCaseObligor> HistoryCaseObligorlistO { get; set; }
        public HistoryCaseAttachment HistoryCaseAttachment { get; set; }
        public List<HistoryCaseAttachment> HistoryCaseAttachmentlistO { get; set; }

        public HistoryCaseEdocFile HistoryCaseEdocFile { get; set; }
        public List<HistoryCaseEdocFile> HistoryCaseEdocFilelist { get; set; }

        public HistoryCaseSendSettingDetails HistoryCaseSendSettingDetails { get; set; } 
        public IList<HistoryCaseMaster> HistoryCaseMasterlist { get; set; }
        public IList<HistoryCaseSendSettingDetails> HistoryCaseSendSettingDetailsList { get; set; }

        public IList<HistoryCaseSeizure> HistoryCaseSeizure { get; set; }
        public IList<HistoryCaseSeizure> HistoryCaseSeizureList { get; set; }
    }
}
