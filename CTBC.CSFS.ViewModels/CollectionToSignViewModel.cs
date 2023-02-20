using CTBC.CSFS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTBC.CSFS.ViewModels
{
    public class CollectionToSignViewModel
    {
        // 介面顯示
        public CollectionToSign CollectionToSign { get; set; }

        // 清單顯示
        public IList<CollectionToSign> CollectionToSignList { get; set; }
    }
}
