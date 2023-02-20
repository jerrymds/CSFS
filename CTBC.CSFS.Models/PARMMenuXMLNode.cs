/// <summary>
/// 程式說明：PARMMenuXMLNode 物件
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
    public class PARMMenuXMLNode
    {
        /// <summary>
        /// Menu的ID
        /// </summary>
        [XmlAttribute("ID")]
        public int ID { get; set; }

        /// <summary>
        /// Menu程式中所辨別的ID
        /// </summary>
        [XmlAttribute("md_FuncID")]
        public string md_FuncID { get; set; }

        /// <summary>
        /// Menu描述
        /// </summary>
        [XmlAttribute("TITLE")]
        public string TITLE { get; set; }

        /// <summary>
        /// Menu的AuthZ
        /// </summary>
        [XmlAttribute("md_AuthZ")]
        public string md_AuthZ { get; set; }

        /// <summary>
        /// Menu的md_URL
        /// </summary>
        [XmlAttribute("md_URL")]
        public string md_URL { get; set; }

        /// <summary>
        /// Menu的md_Ctrl
        /// </summary>
        [XmlAttribute("md_Ctrl")]
        public string md_Ctrl { get; set; }


        /// <summary>
        /// Menu的md_LogEntry
        /// </summary>
        [XmlAttribute("md_LogEntry")]
        public string md_LogEntry { get; set; }

        /// <summary>
        /// Menu的md_EntryLogLevel
        /// </summary>
        [XmlAttribute("md_EntryLogLevel")]
        public string md_EntryLogLevel { get; set; }

        /// <summary>
        /// Menu的md_LogExit
        /// </summary>
        [XmlAttribute("md_LogExit")]
        public string md_LogExit { get; set; }

        /// <summary>
        /// Menu的md_ExitLogLevel
        /// </summary>
        [XmlAttribute("md_ExitLogLevel")]
        public string md_ExitLogLevel { get; set; }

        /// <summary>
        /// Menu的md_LogLocation
        /// </summary>
        [XmlAttribute("md_LogLocation")]
        public string md_LogLocation { get; set; }

        /// <summary>
        /// Menu的md_ExcPolicy
        /// </summary>
        [XmlAttribute("md_ExcPolicy")]
        public string md_ExcPolicy { get; set; }

        /// <summary>
        /// Menu的md_ExcLocation
        /// </summary>
        [XmlAttribute("md_ExcLocation")]
        public string md_ExcLocation { get; set; }

        /// <summary>
        /// 第幾層的Menu的父節點
        /// </summary>
        [XmlIgnore]
        public int MenuLevel { get; set; }

        /// <summary>
        /// menu的父節點
        /// </summary>
        [XmlIgnore]
        public int Parent { get; set; }

        /// <summary>
        /// Menu種類(M=Menu/C=Controller/A=Action/P=Page)
        /// </summary>
        [XmlIgnore]
        public string MenuType { get; set; }

        /// <summary>
        /// 可進行此Menu維護權限的Role
        /// </summary>
        [XmlIgnore]
        public string ModifyRole { get; set; }

        /// <summary>
        /// Menu顯示的前後順序
        /// </summary>
        [XmlIgnore]
        public int MenuSort { get; set; }

        /// <summary>
        /// Menu維護頁面的所選擇的role list
        /// </summary>
        [XmlIgnore]
        public string md_AuthZ_Seleted { get; set; }

        /// <summary>
        /// PageToAction中使否該Action已授權給某個page
        /// </summary>
        [XmlIgnore]
        public string ActionChecked { get; set; }


        /// <summary>
        /// 新增日期
        /// </summary>
        [XmlIgnore]
        public DateTime? CreatedDate { get; set; }

        /// <summary>
        /// 新增人員
        /// </summary>
        [XmlIgnore]
        public string CreatedUser { get; set; }

        /// <summary>
        /// 更新日期
        /// </summary>
        [XmlIgnore]
        public DateTime? ModifiedDate { get; set; }

        /// <summary>
        /// 更新人員
        /// </summary>
        [XmlIgnore]
        public string ModifiedUser { get; set; }

        [XmlElement("Node")]
        public List<PARMMenuXMLNode> SubMenu { get; set; }
    }
}
