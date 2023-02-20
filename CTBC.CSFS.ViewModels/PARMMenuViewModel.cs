using System;
using System.Collections.Generic;
using CTBC.CSFS.Models;

namespace CTBC.CSFS.ViewModels
{
    public class PARMMenuViewModel
    {
        public IList<PARMMenuXMLNode> PARMMenuXMLNodeList { get; set; }
        public PARMMenuXMLNode PARMMenuXMLNode { get; set; }
        public IList<CSFSRole> CSFSRoleList { get; set; }

        //PARMMenu管理維護
        public IList<PARMMenuVO> PARMMenuVOList { get; set; }
        public PARMMenuVO PARMMenuVO { get; set; }

        public PARMCode PARMCode { get; set; }

        public IList<PARMCode> PARMCodeList { get; set; }
    }
}