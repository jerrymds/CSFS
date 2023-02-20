namespace CTBC.CSFS.Models
{
    /// <summary>
    /// 案件狀態
    /// </summary>
    public struct CaseStatus
    {
        /// <summary>
        /// 建檔作業, 來文鍵檔,待集作簽收
        /// </summary>
        public static string CaseInput = "B01";

        /// <summary>
        /// 建檔作業, 確認結案
        /// </summary>
        public static string InputCancelClose = "Z03";

        /// <summary>
        /// 集收將案件退回,
        /// </summary>
        public static string CollectionReturn = "A02";
        

        /// <summary>
        /// 集作修改,修改了還是待分文.
        /// </summary>
        public static string CollectionEdit = "B02";

        /// <summary>
        /// 集作分派,待經辦簽收
        /// </summary>
        public static string CollectionSubmit = "C01";

        /// <summary>
        /// 集作完成代辦
        /// </summary>
        public static string CollectionToAgent = "C05";
        
        /// <summary>
        /// 經辦辦理,編輯,待呈核
        /// </summary>
        public static string AgentEdit = "C02";

        /// <summary>
        /// 經辦改派
        /// </summary>
        public static string AgentReassign = "C03";

        /// <summary>
        /// 經辦退案
        /// </summary>
        public static string AgentReturnClose = "C06";

        /// <summary>
        /// 經辦委託集作代辦
        /// </summary>
        public static string AgentToCollection = "B03";

        /// <summary>
        /// 經辦呈核
        /// </summary>
        public static string AgentSubmit = "D01";

        /// <summary>
        /// 主管再呈核
        /// </summary>
        public static string DirectorSubmit = "D02";

        /// <summary>
        /// 主管通過
        /// </summary>
        public static string DirectorApprove = "Z01";

        /// <summary>
        /// 主管通過(扣押並支付的中间状态)
        /// </summary>
        public static string DirectorApproveSeizureAndPay = "D03";

        /// <summary>
        /// 主管退件
        /// </summary>
        public static string DirectorReturn = "C04";

        /// <summary>
        /// 主管改派
        /// </summary>
        public static string DirectorReassign = "C07";
    }

    /// <summary>
    /// 案件類型
    /// </summary>
    public struct CaseKind
    {
        /// <summary>
        /// 扣押案件
        /// </summary>
        public static string CASE_SEIZURE = "扣押案件";
        /// <summary>
        /// 外來文案件
        /// </summary>
        public static string CASE_EXTERNAL = "外來文案件";

    }

    /// <summary>
    /// 案件類型2
    /// </summary>
    public struct CaseKind2
    {
        /// <summary>
        /// 扣押
        /// </summary>
        public static string CaseSeizure = "扣押";
        /// <summary>
        /// 支付
        /// </summary>
        public static string CasePay= "支付";
        /// <summary>
        /// 扣押並支付
        /// </summary>
        public static string CaseSeizureAndPay = "扣押並支付";
        /// <summary>
        /// 撤銷
        /// </summary>
        public static string CaseSeizureCancel = "撤銷";
		/// <summary>
		/// 扣押電子回文
		/// </summary>
		public static string CaseSeizureEdoc = "扣押電子回文";


    }

    public struct CheckNoUseKind
    {
        /// <summary>
        /// 支付
        /// </summary>
        public static string CasePay = "支付";
    }
    /// <summary>
    /// 註記類型(CaseMemo表用)
    /// </summary>
    public struct CaseMemoType
    {
        /// <summary>
        /// 案件內外部註記
        /// </summary>
        public static string CaseMemo = "CaseMemo";
        /// <summary>
        /// 扣押設定中的Memo
        /// </summary>
        public static string CaseSeizureMemo = "CaseSeizure";
        /// <summary>
        /// 一般案件中的Memo
        /// </summary>
        public static string CaseExternalMemo = "CaseExternal";
    }

    /// <summary>
    /// 上傳類型
    /// </summary>
    public struct Uploadkind
    {
        /// <summary>
        /// 公文信息 上傳
        /// </summary>
        public static string CaseAttach = "CaseAttachment";
        /// <summary>
        /// 借出 上傳
        /// </summary>
        public static string LendAttach = "LendAttachment";
        /// <summary>
        /// 會辦結果 上傳
        /// </summary>
        public static string MeetingResultAttachment = "MeetingResultAttachment";

        /// <summary>
        /// 警示 上傳
        /// </summary>
        public static string WarnAttach = "WarnAttach";
    }

    /// <summary>
    /// 扣押狀態
    /// </summary>
    public struct SeizureStatus
    {
        /// <summary>
        /// 已扣押設定,但沒有設定支付
        /// </summary>
        public static string AfterSeizureSetting = "0";

        /// <summary>
        /// 已完成支付設定,可以當作結案
        /// </summary>
        public static string AfterPaySetting = "1";
        /// <summary>
        /// 取消扣押,也可以當作結案
        /// </summary>
        public static string AfterCancel = "2";
        /// <summary>
        /// 取消扣押,也可以當作結案
        /// </summary>
        public static string AfterPayCancel = "3";
        /// 電子支付未支付或沖正
    }

    /// <summary>
    /// 正本狀態
    /// </summary>
    public struct LendStatus
    {
        /// <summary>
        /// 正本調閱
        /// </summary>
        public static string LendStatusSetting = "0";

        /// <summary>
        /// 正本歸還
        /// </summary>
        public static string LendStatusLendSetting = "1";
    }

    public struct CaseSettingDetailType
    {
        public static int Receive = 1;
        public static int Cc = 2;
    }
}