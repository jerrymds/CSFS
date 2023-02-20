using System;
using System.Collections.Generic;
using CTBC.CSFS.Models;

namespace CTBC.CSFS.ViewModels
{
    public class CSFSLogViewModel
    {
        public IList<CSFSLogVO> CSFSLogList { get; set; }

        public CSFSLogVO CSFSLogVO { get; set; }
    }
}