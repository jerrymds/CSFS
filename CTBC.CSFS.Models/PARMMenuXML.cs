/// <summary>
/// 程式說明：PARMMenuXML 物件
/// 維護部門:資訊管理處
/// 中國信託銀行 版權所有  ©  All Rights Reserved.
/// </summary>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace CTBC.CSFS.Models
{
    [XmlTypeAttribute(AnonymousType = true)]
    public class PARMMenuXML
    {
        [XmlAttribute("ID")]
        public string ID { get; set; }

        [XmlAttribute("NextID")]
        public string NextID { get; set; }

        [XmlAttribute("TITLE")]
        public string TITLE { get; set; }

        [XmlAttribute("HIDE")]
        public string HIDE { get; set; }

        [XmlAttribute("Flow")]
        public string Flow { get; set; }

        [XmlAttribute("Web")]
        public string Web { get; set; }

        [XmlAttribute("Profile")]
        public string Profile { get; set; }

        [XmlAttribute("Create_ID")]
        public string Create_ID { get; set; }

        [XmlAttribute("Create_Name")]
        public string Create_Name { get; set; }

        [XmlAttribute("Time_Create")]
        public string Time_Create { get; set; }

        [XmlAttribute("Update_ID")]
        public string Update_ID { get; set; }

        [XmlAttribute("Update_Name")]
        public string Update_Name { get; set; }

        [XmlAttribute("Time_Update")]
        public string Time_Update { get; set; }

        [XmlAttribute("NodeProfile")]
        public string NodeProfile { get; set; }

        [XmlAttribute("md_AppName")]
        public string md_AppName { get; set; }

        [XmlAttribute("md_RoleSource")]
        public string md_RoleSource { get; set; }

        [XmlAttribute("md_RoleCodeFile")]
        public string md_RoleCodeFile { get; set; }

        [XmlAttribute("md_ActionCodeFile")]
        public string md_ActionCodeFile { get; set; }

        [XmlAttribute("md_Ctrl")]
        public string md_Ctrl { get; set; }

        [XmlAttribute("md_DeployMethod")]
        public string md_DeployMethod { get; set; }

        [XmlAttribute("md_FolderPath")]
        public string md_FolderPath { get; set; }

        [XmlAttribute("md_Database")]
        public string md_Database { get; set; }

        [XmlAttribute("md_ServerIP")]
        public string md_ServerIP { get; set; }

        [XmlAttribute("md_ServerPort")]
        public string md_ServerPort { get; set; }

        [XmlAttribute("md_RootBaseDN")]
        public string md_RootBaseDN { get; set; }

        [XmlAttribute("md_ServiceDN")]
        public string md_ServiceDN { get; set; }

        [XmlAttribute("md_ServicePWD")]
        public string md_ServicePWD { get; set; }

        [XmlAttribute("GUID")]
        public string GUID { get; set; }

        [XmlAttribute("md_AAAadmin")]
        public string md_AAAadmin { get; set; }

        [XmlAttribute("md_AAAauthorizer")]
        public string md_AAAauthorizer { get; set; }

        [XmlAttribute("md_AAAauditor")]
        public string md_AAAauditor { get; set; }

        [XmlElement("Node")]
        public List<PARMMenuXMLNode> Menu { get; set; }

        public PARMMenuXML()
        {
            Menu = new List<PARMMenuXMLNode>();
        }
    }
}
