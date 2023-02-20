﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTBC.CSFS.Models
{
   public class DirectorCooperative
    {
        /// <summary>
        /// 案件ID
        /// </summary>
        public Guid CaseId { get; set; }
        /// <summary>
        /// 序號
        /// </summary>
        public int AssignId { get; set; }
        /// <summary>
        /// 案件編號
        /// </summary>
        public string CaseNo { get; set; }
        /// <summary>
        /// 來文機關
        /// </summary>
        public string GovUnit { get; set; }
        /// <summary>
        /// 來文日期
        /// </summary>
        public string GovDate { get; set; }
        /// <summary>
        /// 來文編號
        /// </summary>
        public string GovNo { get; set; }
        /// <summary>
        /// 經辦人員
        /// </summary>
        public string Person { get; set; }
        public string AgentUser { get; set; }
        /// <summary>
        /// 類別
        /// </summary>
        public string CaseKind { get; set; }
        /// <summary>
        /// 細分類
        /// </summary>
        public string CaseKind2 { get; set; }
        /// <summary>
        /// 速別
        /// </summary>
        public string Speed { get; set; }
        /// <summary>
        /// 限辦日期
        /// </summary>
        public string LimitDate { get; set; }
        public string SortExpression { get; set; }
        public string SortDirection { get; set; }
        public int TotalItemCount { get; set; }
        public int PageSize { get; set; }
        public int CurrentPage { get; set; }
        /// <summary>
        /// 發送日期
        /// </summary>
        public string SendDate { get; set; }
        /// <summary>
        /// 來文開始日期
        /// </summary>
        public string GovDateS { get; set; }
        /// <summary>
        /// 來文結束日期
        /// </summary>
        public string GovDateE { get; set; }
        /// <summary>
        /// 來問方式
        /// </summary>
        public string ReceiveKind { get; set; }
        public string CreatedDateS { get; set; }
        public string CreatedDateE { get; set; }
        public string SendDateS { get; set; }
        public string SendDateE { get; set; }
        public string Unit { get; set; }
        public string CreateUser { get; set; }
        public int maxnum { get; set; }
        public string GovKind { get; set; }
        public string CloseReason { get; set; }
        public string SendKind { get; set; }
        /// <summary>
        /// 案件狀態
        /// </summary>
        public string Status { get; set; }
        //20170714 固定 RQ-2015-019666-019 派件至跨單位 宏祥 add start
        public string AgentDepartment { get; set; }
        public string AgentDepartment2 { get; set; }
        public string AgentDepartmentUser { get; set; }
        public string IsBranchDirector { get; set; }
        //20170714 固定 RQ-2015-019666-019 派件至跨單位 宏祥 add end
    }
}
