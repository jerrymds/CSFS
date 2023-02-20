/// <summary>
/// 程式說明：PARMMenu View Object
/// 維護部門:資訊管理處
/// 中國信託銀行 版權所有  ©  All Rights Reserved.
/// </summary>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CTBC.CSFS.Models
{
    public class PARMMenuVO : Entity
    {
        /// <summary>
        /// 序號
        /// </summary>
        public int RowNum { get; set; }

        /// <summary>
        /// Menu的ID
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// Menu程式中所辨別的ID
        /// </summary>
        public string md_FuncID { get; set; }

        /// <summary>
        /// Menu描述
        /// </summary>
        public string TITLE { get; set; }

        /// <summary>
        /// Menu的AuthZ
        /// </summary>
        public string md_AuthZ { get; set; }

        /// <summary>
        /// Menu的md_URL
        /// </summary>
        public string md_URL { get; set; }

        /// <summary>
        /// Menu的md_Ctrl
        /// </summary>
        public string md_Ctrl { get; set; }


        /// <summary>
        /// Menu的md_LogEntry
        /// </summary>
        public string md_LogEntry { get; set; }

        /// <summary>
        /// Menu的md_EntryLogLevel
        /// </summary>
        public string md_EntryLogLevel { get; set; }

        /// <summary>
        /// Menu的md_LogExit
        /// </summary>
        public string md_LogExit { get; set; }

        /// <summary>
        /// Menu的md_ExitLogLevel
        /// </summary>
        public string md_ExitLogLevel { get; set; }

        /// <summary>
        /// Menu的md_LogLocation
        /// </summary>
        public string md_LogLocation { get; set; }

        /// <summary>
        /// Menu的md_ExcPolicy
        /// </summary>
        public string md_ExcPolicy { get; set; }

        /// <summary>
        /// Menu的md_ExcLocation
        /// </summary>
        public string md_ExcLocation { get; set; }

        /// <summary>
        /// 第幾層的Menu的父節點
        /// </summary>
        public int? MenuLevel { get; set; }

        /// <summary>
        /// menu的父節點
        /// </summary>
        public int? Parent { get; set; }

        /// <summary>
        /// Menu種類(M=Menu/C=Controller/A=Action/P=Page)
        /// </summary>
        public string MenuType { get; set; }

        /// <summary>
        /// 可進行此Menu維護權限的Role
        /// </summary>
        public string ModifyRole { get; set; }

        /// <summary>
        /// Menu顯示的前後順序
        /// </summary>
        public int? MenuSort { get; set; }

        /// <summary>
        /// 快速查詢條件
        /// </summary>
        public string QuickSearchCon { get; set; }
        /// <summary>
        /// 總筆數
        /// </summary>
        public int maxnum { get; set; }
    }
}
