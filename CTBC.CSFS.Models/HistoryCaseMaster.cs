using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTBC.CSFS.Models
{
    public class HistoryCaseMaster : Entity
    {
        public Guid CaseId { get; set; }
        public string DocNo { get; set; }
        public string Status { get; set; }
        public int isDelete { get; set; }
        public string CaseNo { get; set; }
        public string GovKind { get; set; }
        public string GovKindflag { get; set; }
        public string GovUnit { get; set; }
        public string GovUnitflag { get; set; }
        public string ReceiverNo { get; set; }
        public string GovDate { get; set; }
        public string GovDateflag { get; set; }
        public string Speed { get; set; }
        public string ReceiveKind { get; set; }
        public string GovNo { get; set; }
        public string GovNoflag { get; set; }
        public string LimitDate { get; set; }
        public string CaseKind { get; set; }
        public string CaseKind2 { get; set; }
        public string ReceiveDate { get; set; }
        public string Unit { get; set; }
        public string Person { get; set; }
        public string AssignPerson { get; set; }
        public string CreatedUser { get; set; }
        public string CreatedDate { get; set; }
        public string ModifiedUser { get; set; }
        public string ModifiedDate { get; set; }
        public string PropertyDeclaration { get; set; }

        public string CloseUser { get; set; }
        public DateTime CloseDate { get; set; }
        public string CloseReason { get; set; }
        public string AgentUser { get; set; }
        public string AgentBranchId { get; set; }
        public string AgentDeptId { get; set; }
        public string AgentSection { get; set; }
        public string ApproveUser { get; set; }
        public DateTime ApproveDate { get; set; }

        public string AccountKind { get; set; }//報表種類
        public string Depart { get; set; }//科別
        public string CreatedDateStart { get; set; }
        public string CreatedDateEnd { get; set; }
        public string CloseDateStart { get; set; }
        public string CloseDateEnd { get; set; }

        public string ObligorNo { get; set; }
        public int maxnum { get; set; }
        public int Sno { get; set; }//序號

        public string PostType { get; set; }//類別
        public string MailStatus { get; set; }
        public string SendType { get; set; }
        public string SendWord { get; set; }
        public string SendNo { get; set; }
        public string GovName { get; set; }
        public string GovAddr { get; set; }
        public string SendDateStart { get; set; }
        public string SendDateEnd { get; set; }

        public int DetailsId { get; set; }
        public string MailNo { get; set; }
        public string MailNo1 { get; set; }
        public string MailNo2 { get; set; }
        public string MailNo3 { get; set; }
        public string MailNo4 { get; set; }
        public string MailDate { get; set; }
        public string MailDateStart { get; set; }
        public string MailDateEnd { get; set; }

        public string CheckNo { get; set; }

        //* 20150514 CR,要求案件類型也能改
        public string OldCaseKind { get; set; }

        public int PayeeId { get; set; }
        public string ReceivePerson { get; set; }
        public string SendDate { get; set; }
        public string HangingDate { get; set; }
        public string HangingDateStart { get; set; }
        public string HangingDateEnd { get; set; }
        public string Fee { get; set; }
        public string HangingAmount { get; set; }
        public string ChargeOffsDate { get; set; }
        public string ChargeOffsDateStart { get; set; }
        public string ChargeOffsDateEnd { get; set; }
        public string ChargeOffsAmount { get; set; }
        public string Balance { get; set; }
        public string Memo { get; set; }

        public string GovDateStart { get; set; }
        public string GovDateEnd { get; set; }
        public string SendDateS { get; set; }
        public string SendDateE { get; set; }
        public string PayStatus { get; set; }
        public string buttontype { get; set; }//*匯出

        public int AfterSeizureApproved { get; set; }

        public string OldCaseNo { get; set; }
        public string NewCaseNo { get; set; }

        public string PayDate { get; set; }
        public string ReturnReason { get; set; }

        public string AgentUser2 { get; set; }
		  public string PageFrom { get; set; }

        //20170811 RC RQ-2015-019666-020 派件至跨單位 宏祥 add start
        public string AgentDepartment { get; set; }
        public string AgentDepartment2 { get; set; }
        public string AgentDepartmentUser { get; set; }
        //20170811 RC RQ-2015-019666-020 派件至跨單位 宏祥 add end
        public string Receiver { get; set; }//受文者
        public string Receiverflag { get; set; }
        public int ReceiveAmount { get; set; }//來函扣押總金額
        public string ReceiveAmountflag { get; set; }
        public int NotSeizureAmount { get; set; }//金額未達毋需扣押
        public string NotSeizureAmountflag { get; set; }

        ///手續費
        /// <summary>
        /// 扣押是否啟用自動派件
        /// </summary>
        public bool IsAutoDispatch { get; set; }
        /// <summary>
        /// 外來文是否啟用自動派件
        /// </summary>
        public bool IsAutoDispatchFS { get; set; }
        /// <summary>
        /// 解扣日
        /// </summary>
        public string BreakDay { get; set; }
        public int PreSubAmount { get; set; }//前案扣押總金額
        public int PreReceiveAmount { get; set; }//收取支付總金額
        public string OverCancel { get; set; }//超過收取金額部份是否撤銷
        public int AddCharge { get; set; }//手續費
        public string IsEnable { get; set; } //是否拋查
        public string PreGovNo { get; set; } //前案來文字號
        public string PreSubDate { get; set; }//扣押來文日期
        public string RowNum { get; set; }
    }
}
