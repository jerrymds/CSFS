using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CTBC.CSFS.BussinessLogic;
using CTBC.CSFS.Models;
using CTBC.CSFS.Pattern;
using CTBC.CSFS.Resource;
using CTBC.CSFS.ViewModels;
using Microsoft.Reporting.WebForms;
using System.Data;
using CTBC.FrameWork.Util;
using System.Text;
using System.IO;
using CTBC.FrameWork.HTG;

namespace CTBC.CSFS.Areas.Agent.Controllers
{
    public class AgentAccountInfoController : AppController
    {
        CasePayeeSettingBIZ cpsBIZ = new CasePayeeSettingBIZ();
        CaseMasterBIZ caseMaster = new CaseMasterBIZ();
        CaseAccountBiz caseAccount = new CaseAccountBiz();
        CustomerInfoBIZ CIBZ = new CustomerInfoBIZ();
        List<ReportDataSource> subDataSource = new List<ReportDataSource>();

		public static string pageFromName = "";

        // GET: Agent/AgentAccountInfo
		public ActionResult Index(Guid caseId, String pageFrom = "")
        {
            ViewBag.CaseId = caseId;
			if (pageFrom == "1")
			{
				pageFromName = "1";
			}
			else if (pageFrom == "2")
			{
				pageFromName = "2";
			}
            var master = caseMaster.MasterModel(caseId);
            ViewBag.CaseKind = master.CaseKind;
            ViewBag.CaseKind2 = master.CaseKind2;
            ViewBag.GovNo = master.GovNo;
            //20200407
            ViewBag.ReceiveKind = master.ReceiveKind;
            ViewBag.IsEnable = master.IsEnable;
            ViewBag.OverCancel = master.OverCancel;
            ViewBag.PreSubAmount = master.PreSubAmount;
            ViewBag.PreReceiveAmount = master.PreReceiveAmount;
            ViewBag.AddCharge = master.AddCharge;
            ViewBag.PreGovNo = master.PreGovNo;
            if (master.PreSubDate != null && master.PreSubDate.Length > 7)
            {
                ViewBag.PreSubDate = Convert.ToDateTime(master.PreSubDate).ToShortDateString();
            }
            else
            {
                ViewBag.PreSubDate = "";
            }
            //
            ViewBag.AfterSeizureApproved = master.AfterSeizureApproved;
            ViewData["AfterSeizureApproved2"] = master.AfterSeizureApproved;
            ViewBag.CaseId = caseId;
            ViewBag.LimiteDate = master.LimitDate;
            PARMCodeBIZ para = new PARMCodeBIZ();
            ViewBag.AddDay = (Convert.ToInt32(para.GetCASE_END_TIME("OVERDUE_DAYS"))) * 1;
            ViewBag.ReceiveKind = master.ReceiveKind;//來文方式為紙本，則無[開啟TXT檔案]按鈕
            //* 讀取該案件以前發文儲存的資料,如果沒有資料則開啟發文內容按鈕進入新增畫面,否則進入編輯畫面
            CaseSendSettingBIZ cssBIZ = new CaseSendSettingBIZ();
            IList<CaseSendSettingQueryResultViewModel> listRtn = cssBIZ.GetSendSettingList(caseId);
            if (listRtn != null && listRtn.Any())
            {
                ViewBag.CaseSendSettingView = "Edit";//已經產生過發文檔就進入編輯畫面
                ViewBag.SerialId = listRtn[0].SerialId;
            }
            else
            {
                ViewBag.CaseSendSettingView = "Add";//未產生過發文檔就進入新增畫面
            }

            #region Old
            ////*查詢電文狀態
            //List<BatchQueue> CaseQueryList = caseAccount.StatusList(caseId);
            //List<string> StatusList = new List<string>();

            //foreach (BatchQueue item in CaseQueryList)
            //{
            //    StatusList.Add(item.Status);
            //}

            //if (StatusList != null && StatusList.Count > 0)
            //{
            //    foreach (string item in StatusList)
            //    {
            //        if (item.Contains("2"))//*只要包含有2則賦值'失敗' 跳出循環
            //        {
            //            ViewBag.Status = "2";
            //            break;
            //        }
            //        if (item.Contains("0")) //有跟第一筆不一樣的即有0 和1 的
            //        {
            //            ViewBag.Status = "0";
            //            break;
            //        }
            //        ViewBag.Status = "1";
            //    }
            //}
            #endregion
            decimal Reset = 0;
            Reset = caseAccount.ResetList(caseId);
            if (master.ReceiveKind == "紙本")
            {
                //*查詢電文狀態
                List<BatchQueue> CaseQueryList = caseAccount.StatusList(caseId);
                List<string> StatusList = new List<string>();

                foreach (BatchQueue item in CaseQueryList)
                {
                    StatusList.Add(item.Status);
                }
                if (StatusList != null && StatusList.Count > 0)
                {
                    foreach (string item in StatusList)
                    {
                        if (item.Contains("2"))//*只要包含有2則賦值'失敗' 跳出循環
                        {
                            ViewBag.Status = "2";
                            break;
                        }
                        if (item.Contains("0") || item.Contains("99")) //有跟第一筆不一樣的即有0 和1 的
                        {
                            ViewBag.Status = "0";
                            break;
                        }
                        ViewBag.Status = "1";
                    }
                }
            }
            else //電子來文的電文狀態根據CaseMaster的Status欄位來判斷
            {
                if (master.Status == "C01")
                {
                    ViewBag.Status = "2";
                }
                else if (master.Status == "D01" )
                {
                    ViewBag.Status = "1";
                }
                else
                {
                    ViewBag.Status = "0";
                }

                
            }
            if ( Reset <= 0)
            {
                ViewBag.Status = "9";
            }
            CaseEdocFile caseEdocFile = caseAccount.OpenTxtDoc(caseId);
            ViewBag.HasFile = caseEdocFile == null ? "0" : "1";
            return View();
        }

        #region 扣押設定
        /// <summary>
        /// 扣押設定 顯示
        /// </summary>
        /// <param name="caseId"></param>
        /// <returns></returns>
        public ActionResult _SeizureSetting(Guid caseId)
        {
            int logcount = 0;
            string CustId = "";
            CaseSeizureViewModel caseview = new CaseSeizureViewModel();
            caseview.CaseMaster = caseMaster.MasterModel(caseId);
            List<CaseObligor> list = new CaseObligorBIZ().ObligorModel(caseId);    //* 得到CaseObligor的model
            ViewBag.ObligorNoList = new SelectList(list, "ObligorNo", "ObligorNoAndName");
            if(list != null && list.Any())
            {
                foreach (var item in list)
                {
                    // 新增個資LOG
                    string _user = (HttpContext.Session["UserAccount"] != null) ? HttpContext.Session["UserAccount"].ToString() : "";
                    System.Collections.Specialized.NameValueCollection _parameters = new System.Collections.Specialized.NameValueCollection();
                    //System.Collections.Specialized.NameValueCollection _parameters = ("POST".Equals(HttpContext.Request.HttpMethod)) ? HttpContext.Request.Form : HttpContext.Request.QueryString;
                    string _controller = (this.ControllerContext.RouteData.Values["Controller"] != null) ? this.ControllerContext.RouteData.Values["Controller"].ToString() : "";
                    string _action = (this.ControllerContext.RouteData.Values["Action"] != null) ? this.ControllerContext.RouteData.Values["Action"].ToString() : "";
                    string _ip = (HttpContext.Request != null && !string.IsNullOrEmpty(HttpContext.Request.ServerVariables["REMOTE_ADDR"])) ? HttpContext.Request.ServerVariables["REMOTE_ADDR"] : "";
                    CustId = item.ObligorNo;
                    BaseBusinessRule _business = new BaseBusinessRule();
                    if (logcount == 0)
                    {
                        _business.ExecuteAPLOGSave(_user, _controller, _action, _ip, "CASEID=" + caseId, CustId);
                        logcount = logcount + 1;
                    }

                    // 新增結束
                    TxCustomerInfo txCustInfo = CIBZ.GetRMData(item.ObligorNo.ToString(), caseId);
                    if (txCustInfo != null && string.IsNullOrEmpty(txCustInfo.ErrMsg))
                    {
                        item.DuplicateID = txCustInfo.Tx67072Grp.IdDupFlag;
                        item.RMcommissioner = txCustInfo.Tx67002.RM_NO + " " + txCustInfo.Tx67002.RM_NAME + " / " + txCustInfo.Tx60491Grp.FbAoCode + " " + txCustInfo.Tx60491Grp.FbAoBranch + " - " + txCustInfo.Tx60491Grp.FbTeller;
                        string viplevel = "";
                        switch (txCustInfo.Tx60491Grp.VipCdI)
                        {
                            case "01": viplevel = "-1.5E"; break;
                            case "02": viplevel = "-6000"; break;
                            case "03": viplevel = "-3000"; break;
                            case "04": viplevel = "-1500"; break;
                            case "05": viplevel = "-1200"; break;
                            case "06": viplevel = "-900"; break;
                            case "07": viplevel = "-600"; break;
                            case "08": viplevel = "-300"; break;
                            case "09": viplevel = "-150"; break;
                            case "10": viplevel = "-100"; break;
                            case "11": viplevel = "-50"; break;
                            case "12": viplevel = "-10"; break;
                            default: viplevel = "<10"; break;
                        }
                        item.CustomerLevel = txCustInfo.Tx60491Grp.VipCdI + " " + viplevel;
                    }
                }
            }
            Bind();

            if (caseview.CaseMaster == null)
                return Content(Lang.csfs_not_caseseizure);
            if (caseview.CaseMaster.CaseKind != CaseKind.CASE_SEIZURE)
                return Content(Lang.csfs_not_caseseizure);

            ViewBag.CaseId = caseId;
            ViewBag.CaseNo = caseview.CaseMaster.CaseNo;

            ViewBag.ReceiveAmount = caseview.CaseMaster.ReceiveAmount;//來函扣押總金額
            ViewBag.NotSeizureAmount = caseview.CaseMaster.NotSeizureAmount;//金額未達毋需扣押
            #region //來函扣押總金額、金額未達毋需扣押顯示數值與公文資訊一致  IR-1026
            //if (caseview.CaseMaster.ReceiveKind == "電子公文")
            //{
            //    if (caseview.CaseMaster.CaseKind2 == CaseKind2.CaseSeizure || caseview.CaseMaster.CaseKind2 == CaseKind2.CaseSeizureAndPay)//細類為扣押或扣押並支付時，這三個欄位才有默認值
            //    {
            //        CaseEdocFile caseEdocFile = caseAccount.OpenTxtDoc(caseview.CaseMaster.CaseId);
            //        string text = string.Empty;
            //        if (caseEdocFile != null)
            //        {
            //            byte[] file = caseEdocFile.FileObject;
            //            text = Encoding.UTF8.GetString(file);
            //            int beginIndex = text.IndexOf("合計：");
            //            int endIndex = text.IndexOf("備註：");
            //            string amt = "";
            //            if (beginIndex > 0 && endIndex > 0 && beginIndex < endIndex)
            //            {
            //                amt = text.Substring(beginIndex + 3, endIndex - beginIndex - 3).Trim();
            //            }
            //            if (!string.IsNullOrEmpty(amt))
            //            {
            //                ViewBag.ReceiveAmount = int.Parse(amt);
            //            }
            //            else
            //            {
            //                ViewBag.ReceiveAmount = caseview.CaseMaster.ReceiveAmount;
            //            }
            //        }
            //        else
            //        {
            //            ViewBag.ReceiveAmount = caseview.CaseMaster.ReceiveAmount;
            //        }
            //        if (caseview.CaseMaster.NotSeizureAmount == 0)
            //        {
            //            ViewBag.NotSeizureAmount = 450;//1.電子來文 450(預設值)
            //        }
            //        else
            //        {
            //            ViewBag.NotSeizureAmount = caseview.CaseMaster.NotSeizureAmount;
            //        }
            //    }
            //}
            //else
            //{
            //    if (caseview.CaseMaster.CaseKind2 == CaseKind2.CaseSeizure || caseview.CaseMaster.CaseKind2 == CaseKind2.CaseSeizureAndPay)//細類為扣押或扣押並支付時，這三個欄位才有默認值
            //    {
            //        ViewBag.ReceiveAmount = caseview.CaseMaster.ReceiveAmount;//來函扣押總金額
            //        if (caseview.CaseMaster.NotSeizureAmount == 0)//金額未達毋需扣押
            //        {
            //            //紙本來文 預設值:法院1250 執行署450
            //            if (caseview.CaseMaster.GovKind == "法院")
            //            {
            //                ViewBag.NotSeizureAmount = 1250;
            //            }
            //            else
            //            {
            //                ViewBag.NotSeizureAmount = 450;
            //            }
            //        }
            //        else
            //        {
            //            ViewBag.NotSeizureAmount = caseview.CaseMaster.NotSeizureAmount;
            //        }
            //    }
            //}
            #endregion

            PARMCodeBIZ pbiz = new PARMCodeBIZ();
            ViewBag.Deposit = string.Join(",", pbiz.GetParmCodeByCodeType("SeizureSeqence").ToList().Where(x => x.CodeDesc == "定存").Select(g => g.CodeMemo));//定存的產品代碼
            ViewBag.ForeignCurrencyDeposit = string.Join(",", pbiz.GetParmCodeByCodeType("SeizureSeqence").ToList().Where(x => x.CodeDesc == "外幣定存").Select(g => g.CodeMemo));//外幣定存的產品代碼

            //案件類型:扣押並支付 時,解扣日預設日為目前系統20日支付方法的日期
            if(caseview.CaseMaster.CaseKind2 == CaseKind2.CaseSeizureAndPay)
            {
                CheckQueryAndPrintBIZ CKP = new CheckQueryAndPrintBIZ();
                string DTSRC_Date = caseMaster.GetPayDate(caseview.CaseMaster.CaseKind2, caseview.CaseMaster.CreatedDate).ToString("yyyy/MM/dd");//CreatedDate:建檔日期
                DTSRC_Date = CKP.GetWorkingDays(DTSRC_Date);//如遇非工作日,自動順延一個營業日
                DTSRC_Date = Convert.ToDateTime(DTSRC_Date).ToString("yyyy/MM/dd");
                caseview.CaseMaster.BreakDay = DTSRC_Date;
            }
            //adam 去除重覆科目
            IList<CaseSeizure> listSeizure = caseAccount.GetCaseSeizure(caseId);
            if (listSeizure != null)
            {
                listSeizure = listSeizure.GroupBy(x => new { x.Account, x.Currency }).Select(g => g.First()).ToList();//用帳號與幣別去重
            }
            if (listSeizure == null || !listSeizure.Any())
            {
                //* 查主機
                listSeizure = caseAccount.GetCaseSeizureFromTx(caseId);
                if (listSeizure != null && listSeizure.Any())
                {
                    #region 他案扣押顯示
                    foreach (CaseSeizure item in listSeizure)
                    {
                        //若TX450-30有出現代碼04則顯示Y，無則顯示N;若TX450-31有出現凍結碼66則顯示Y，無則顯示N
                        CTBC.CSFS.Models.TX_00450 CMOther = CIBZ.GetLatestTx00450(item.Account, caseId, "31", " 9093 ", " 66");
                        if (CMOther != null && CMOther.DATA1.Contains(" 9093 ") && CMOther.DATA2.Trim().Contains(" 66"))
                        {
                            item.OtherSeizure = "Y";
                        }
                        else
                        {
                            item.OtherSeizure = "N";
                        }
                        CTBC.CSFS.Models.TX_00450 CM = CIBZ.GetLatestTx00450(item.Account, caseId, "30", " 9091 ", " 04");
                        if (CM != null && CM.DATA1.Contains(" 9091 ") && CM.DATA2.Trim().Contains(" 04"))
                        {
                            item.OtherSeizure = "Y";
                        }
                        else
                        {
                            item.OtherSeizure = "N";
                        }
                        //(查TX_33401的HoldAmt > 0 則為Y) IR-1006
                        CTBC.CSFS.Models.TX_33401 tx33401 = CIBZ.GetLatestTx33401(item.Account, caseId, item.Currency);
                        if (tx33401 != null && !string.IsNullOrEmpty(tx33401.HoldAmt) && Convert.ToDecimal(tx33401.HoldAmt) > 0)
                        {
                            item.OtherSeizure = "Y";
                        }
                        else
                        {
                            item.OtherSeizure = "N";
                        }
                    }
                    #endregion
                }
            }
            if (listSeizure == null || !listSeizure.Any())
            {
                listSeizure = new List<CaseSeizure>();
            }
            #region 給存款帳號過濾并排序
            else
            {
                List<CaseSeizure> OrderedAccLists = new List<CaseSeizure>();
                List<CaseSeizure> NoTxtProdCodeLists = listSeizure.Where(x => (x.TxtProdCode == "ZZZZ" || x.TxtProdCode == "" || x.TxtProdCode == null) && !string.IsNullOrEmpty(x.Account)).ToList();//TxtProdCode為ZZZZ或為空的資料直接放在最後
                int _SeiSeq = 1;
                var seizureSequenceAll = pbiz.GetParmCodeByCodeType("SeizureSeqence");
                Dictionary<string, List<string>> SeizureOrder = getSeizureOrder(seizureSequenceAll);
                var pids = (from p in listSeizure group p by p.CustId into g select g.Key).ToList();
                logcount = 0;
                foreach (string id in pids)
                {
                    foreach (var s in SeizureOrder)
                    {
                        //if (s.Key == "定存")
                        //    continue; adam
                        if (s.Key == "綜定" || s.Key == "定存")
                        {
                            // adam 不限定 10碼
                            var accTemp = listSeizure.Where(x =>  x.CustId == id && s.Value.Contains(x.TxtProdCode)).ToList();
                            //var accTemp = listSeizure.Where(x => x.CustId.Length > 8 && x.CustId == id && s.Value.Contains(x.TxtProdCode)).ToList();
                            if (accTemp != null && accTemp.Count() > 0)
                            {
                                foreach (var acc in accTemp)
                                {
                                    acc.SeizureSeq = _SeiSeq++;
                                }
                                OrderedAccLists.AddRange(accTemp);
                            }
                        }
                        else
                        {
                            // adam
                            //var accTemp = listSeizure.Where(x => x.CustId.Length > 8 && x.CustId == id && s.Value.Contains(x.TxtProdCode)).ToList();
                            var accTemp = listSeizure.Where(x =>  x.CustId == id && s.Value.Contains(x.TxtProdCode)).ToList();
                            if (accTemp != null && accTemp.Count() > 0)
                            {
                                foreach (var at in accTemp)
                                {
                                    at.SeizureSeq = _SeiSeq++;
                                }
                                OrderedAccLists.AddRange(accTemp);
                            }
                            List<string> XXStartWith = s.Value.Where(x => x.StartsWith("XX")).ToList();
                            if (XXStartWith.Count() > 0) // 表示有XX頭, 則必須要用EndWith遂一過濾, 
                            {
                                foreach (var xx in XXStartWith)
                                {
                                    var tail = xx.Substring(2);
                                    // adam 
                                    //var lst = listSeizure.Where(x => x.CustId.Length > 8 && x.CustId == id && x.TxtProdCode.EndsWith(tail)).ToList();
                                    var lst = listSeizure.Where(x =>  x.CustId == id && x.TxtProdCode.EndsWith(tail)).ToList();
                                    if (lst != null && lst.Count() > 0)
                                    {
                                        foreach (var at in lst)
                                        {
                                            at.SeizureSeq = _SeiSeq++;
                                        }
                                        OrderedAccLists.AddRange(lst);
                                    }
                                }
                            }
                        }

                        // 新增個資LOG
                        string _user = (HttpContext.Session["UserAccount"] != null) ? HttpContext.Session["UserAccount"].ToString() : "";
                        System.Collections.Specialized.NameValueCollection _parameters = new System.Collections.Specialized.NameValueCollection();
                        //System.Collections.Specialized.NameValueCollection _parameters = ("POST".Equals(HttpContext.Request.HttpMethod)) ? HttpContext.Request.Form : HttpContext.Request.QueryString;
                        string _controller = (this.ControllerContext.RouteData.Values["Controller"] != null) ? this.ControllerContext.RouteData.Values["Controller"].ToString() : "";
                        string _action = (this.ControllerContext.RouteData.Values["Action"] != null) ? this.ControllerContext.RouteData.Values["Action"].ToString() : "";
                        string _ip = (HttpContext.Request != null && !string.IsNullOrEmpty(HttpContext.Request.ServerVariables["REMOTE_ADDR"])) ? HttpContext.Request.ServerVariables["REMOTE_ADDR"] : "";
                        BaseBusinessRule _business = new BaseBusinessRule();
                        if (logcount == 0)
                        {
                            _business.ExecuteAPLOGSave(_user, _controller, _action, _ip, "CASEID=" + caseId, id);
                            logcount = logcount + 1;
                        }

                        // 新增結束


                    }
                }
                if (OrderedAccLists.Count() > 0)
                {
                    #region  排除 "結清", "已貸", "啟用", "誤開", "新戶" 的帳戶
                    List<string> noSave = new List<string>() { "結清", "已貸", "啟用", "誤開", "新戶" };
                    List<CaseSeizure> newOrderedAccLists = new List<CaseSeizure>();
                    foreach (var acc in OrderedAccLists)
                    {
                        bool bfilter = true;
                        if (acc.Account.StartsWith("000000000000"))
                            bfilter = false;

                        #region 判斷是否是現金卡等等
                        // 若 prod_code = 0058, 或XX80 , 不用存
                        if (acc.TxtProdCode.ToString().Equals("0058") || acc.TxtProdCode.ToString().EndsWith("80"))
                            bfilter = false;

                        // 若  Link<>'JOIN' , 不用存
                        if (acc.Link.ToString().Equals("JOIN"))
                            bfilter = false;

                        // 若 StsDesc='結清' AND  StsDesc='已貸' AND  StsDesc='啟用' AND  StsDesc='誤開'  AND  StsDesc='新戶', 也不用存
                        string sdesc = acc.AccountStatus.ToString().Trim();
                        if (noSave.Contains(sdesc))
                            bfilter = false;

                        #endregion

                        if (bfilter)
                            newOrderedAccLists.Add(acc);
                    }
                    #endregion
                    if (newOrderedAccLists.Count() > 0)
                    {
                        listSeizure = newOrderedAccLists.OrderBy(o => o.SeizureSeq).ToList();//升序
                        #region 外幣活存要砍掉右邊3碼  IR-1019
                        string foreignCcy = pbiz.GetParmCodeByCodeType("SeizureSeqence").Where(m => m.CodeDesc == "外幣活存").FirstOrDefault().CodeMemo;//外幣活存的產品代碼
                        foreach (CaseSeizure item in listSeizure)
                        {
                            if (!string.IsNullOrEmpty(item.TxtProdCode.Trim()) && item.TxtProdCode.Length > 2)//TxtProdCode排空才能Substring
                            {
                                if (item.Currency.ToUpper() != "TWD" && item.Account.Length >= 15 && foreignCcy.Contains(item.TxtProdCode.Substring(2)))
                                {
                                    item.Account = item.Account.Substring(0, item.Account.Length - 3);
                                }
                            }
                        }
                        #endregion
                    }
                    else
                    {
                        listSeizure = new List<CaseSeizure>();
                    }
                }
                else
                {
                    listSeizure = new List<CaseSeizure>();
                }
                //TxtProdCode為ZZZZ或為空的資料直接放在最後
                if (NoTxtProdCodeLists.Count > 0)
                {
                    foreach (var item in NoTxtProdCodeLists)
                    {
                        listSeizure.Add(item);
                    }
                }
            }
            #endregion
            #region 台幣欄位直接去掉小數取整 IR-1052
            if (listSeizure != null && listSeizure.Any())
            {
                listSeizure = listSeizure.GroupBy(x => new { x.Account, x.Currency }).Select(g => g.First()).ToList();//用帳號與幣別去重
                foreach(var item in listSeizure)
                {
                    item.SeizureAmountNtd = Math.Floor(item.SeizureAmountNtd);
                }
            }
            #endregion

            #region Old Memo
            //CaseMemo memo = new CaseMemoBiz().Memo(caseId, CaseMemoType.CaseSeizureMemo);
            //if (memo != null)
            //    ViewBag.Memo = memo.Memo;
            #endregion
            List<CaseMemo> listMemo = new CaseMemoBiz().MemoList2(caseId, CaseMemoType.CaseSeizureMemo);
            string memo = "";
            if (listMemo != null && listMemo.Any())
            {
                foreach (var item in listMemo)
                {
                    if (!string.IsNullOrEmpty(item.Memo) && item.Memo != "\r\n")
                    {
                        memo = memo + item.Memo.TrimEnd('\n').TrimEnd('\r').TrimEnd('\n').TrimEnd('\r').TrimEnd('\n').TrimEnd('\r').TrimEnd('\n').TrimEnd('\r').TrimEnd('\n').TrimEnd('\r') + "\r\n";
                    }
                }
            }
            ViewBag.Memo = memo;
            ViewBag.CheckCaseSeizure = new SelectList(caseAccount.CheckCaseSeizure(caseId), "ObligorNo", "ObligorName");
            
            caseview.CaseObligorlistO = list;
            caseview.CaseSeizureList = listSeizure;
            return View(caseview);
        }
        #region 過濾帳號類別
        private static Dictionary<string, List<string>> getSeizureOrder(IList<CSFS.Models.PARMCode> seizureSequenceAll)
        {
            var seizureSequence = seizureSequenceAll.Where(x => x.CodeNo != "99").ToList();
            Dictionary<string, List<string>> SeizureOrder = new Dictionary<string, List<string>>();
            foreach (var s in seizureSequence)
            {
                if (!string.IsNullOrEmpty(s.CodeMemo))
                {
                    SeizureOrder.Add(s.CodeDesc, s.CodeMemo.Split(',').ToList());
                }
            }
            return SeizureOrder;
        }
        #endregion
        /// <summary>
        /// 扣押設定 儲存 /扣押 /沖正 /事故 /呈核
        /// </summary>
        /// <param name="listForm"></param>
        /// <param name="memo"></param>
        /// <param name="functionId"></param>
        /// <returns></returns>
        /// 
        private static string getNewMemo(string govUnit, string payGovNo)
        {
            //20200624, 新的方法
            // 1. 若是執行署, 則取(第一個中文字)+執+(執後第一個字) + 號(前面的六碼)
            // 2. 若是法院, 則取 (院前所有字)+(字前一個字)+號(前面六碼)

            if (govUnit.IndexOf("執行署") > 0)
            {

                var pos1 = payGovNo.IndexOf("執") + 1;
                var pos2 = payGovNo.LastIndexOf("號");
                string f1 = payGovNo.Substring(0, 1) + "執" + payGovNo.Substring(pos1, 1);
                string f2 = payGovNo.Substring(pos2 - 6, 6);
                return f1 + f2;
            }
            if (govUnit.IndexOf("地方法院") > 0)
            {
                var pos1 = payGovNo.IndexOf("院") + 1;
                var pos2 = payGovNo.LastIndexOf("字第");
                var pos3 = payGovNo.LastIndexOf("號");
                string f1 = payGovNo.Substring(0, pos1);
                string f2 = payGovNo.Substring(pos2 - 1, 1);
                string f3 = payGovNo.Substring(pos3 - 6, 6);
                return f1 + f2 + f3;

            }
            return "";
        }
        public ActionResult DoSaveSeizureSetting(CaseSeizureViewModel model, string memo, Guid CaseId, string functionSeizureId, string BreakDay)
        {
            List<CaseSeizure> listForm = new List<CaseSeizure>();
            if (model != null && model.CaseSeizureList != null && model.CaseSeizureList.Any())
            {
                //過濾空的CaseSeizure
                //listForm = model.CaseSeizureList.ToList();
                foreach (var item in model.CaseSeizureList)
                {
                    if(item != null && !item.CaseId.Equals("00000000-0000-0000-0000-000000000000") && !string.IsNullOrEmpty(item.CaseNo) && !string.IsNullOrEmpty(item.Account))
                    {
                        listForm.Add(item);
                    }
                }
            }
            string userId = LogonUser.Account;
            #region 儲存
            if (string.IsNullOrEmpty(functionSeizureId))
            {
                bool rtn = false;
                string Message = "";
                if (listForm != null && listForm.Any())
                {
                    //給帳號去左側0000加幣別來比較去重
                    List<CaseSeizure> listAcc = new List<CaseSeizure>();
                    foreach(var acc in listForm)
                    {
                        CaseSeizure cs = new CaseSeizure();
                        cs.Account = acc.Account;
                        cs.Currency = acc.Currency;
                        cs.Account = cs.Account.TrimStart('0');
                        listAcc.Add(cs);
                    }
                    var distinct = (from t in listAcc group t by new { t.Account, t.Currency } into g select new { count = g.Count() } into c where c.count > 1 select c);
                    if (distinct.Count() > 0)
                    {
                        Message = "帳號及幣別,不能重複,請重新輸入!!";
                    }
                    else
                    {
                        foreach (CaseSeizure item in listForm)
                        {
                            item.CreatedUser = userId;
                            item.ModifiedUser = userId;
                        }
                        caseAccount.UserId = userId;
                        caseAccount.PageFrom = pageFromName;
                        rtn = caseAccount.SaveSeizureSetting(listForm, memo, CaseId);
                    }
                }
                else
                {
                    caseAccount.UserId = userId;
                    caseAccount.PageFrom = pageFromName;
                    rtn = caseAccount.SaveSeizureSetting(listForm, memo, CaseId);
                }
                if (string.IsNullOrEmpty(Message))
                {
                    if (rtn)
                    {
                        Message = Lang.csfs_save_ok;
                    }
                    else
                    {
                        Message = Lang.csfs_save_fail;
                    }
                }
 
                return Json(rtn ? new JsonReturn { ReturnCode = "1", ReturnMsg = Message }
                                : new JsonReturn { ReturnCode = "0", ReturnMsg = Message });
            }
            #endregion
            #region 呈核
            else if (functionSeizureId == "Nuclear")
            {
                bool result = ChengHe(CaseId);
                return Json(result ? new JsonReturn { ReturnCode = "1", ReturnMsg = Lang.csfs_nuclear_success }
                                  : new JsonReturn { ReturnCode = "0", ReturnMsg = Lang.csfs_nuclear_fail });
            }
            #endregion
            else
            {
                bool rtn = false;
                string Message = "";
                string result = "";
                List<CaseSeizure> rtnList = new List<CaseSeizure>();
                var master = caseMaster.MasterModel(CaseId);   
                if (master.GovUnit.Length  == 0)
                {
                    if ( master.ReceiveKind == "紙本")
                    {
                        master.GovUnit = "地方法院";
                    }
                    else
                    {
                        master.GovUnit = "執行署";
                    }
                } 
                string strMemo = getNewMemo(master.GovUnit, master.GovNo);
                //string strMemo = model.CaseMaster.GovNo.Substring(0, 3) + model.CaseMaster.GovNo.Substring(model.CaseMaster.GovNo.Length - 7, 6);//查電文時，備註中要用到的來文字號是caseMaster表中的GovNo
                //當經辦登入外來文系統時未輸入RCAF的帳號、密碼及分行時，登入之權限應不可執行任何有關電文的交易按鈕。 IR-0048
                if (!string.IsNullOrEmpty(LogonUser.RCAFAccount.Trim()) && !string.IsNullOrEmpty(LogonUser.RCAFPs.Trim()) && !string.IsNullOrEmpty(LogonUser.RCAFBranch.Trim()))
                {
                    if (listForm != null && listForm.Any())
                    {
                        foreach (CaseSeizure item in listForm)
                        {
                            item.CreatedUser = userId;
                            item.ModifiedUser = userId;
                            if (item.IsCheck == "true")
                            {
                                //發查電文做扣押
                                if (functionSeizureId == "Seizure")
                                {
                                    result = caseAccount.SeizureTelegram(item, strMemo, CaseId, model.CaseMaster.CaseKind2, BreakDay, "Seizure", LogonUser);
                                    if (result == "true")
                                    {
                                        rtn = true;
                                        Message = Message + "存款帳號為:" + item.Account + " 的帳戶發查扣押電文成功;<br/>";
                                    }
                                    else
                                    {
                                        Message = Message + "存款帳號為:" + item.Account + " 的帳戶發查扣押電文失敗,原因是 " + result + ";<br/>";
                                    }
                                }
                                //發查電文做沖正
                                if (functionSeizureId == "Reset")
                                {
                                    result = caseAccount.ResetTelegram(item, strMemo, CaseId, "Seizure", LogonUser, BreakDay);
                                    if (result == "true")
                                    {
                                        rtn = true;
                                        Message = Message + "存款帳號為:" + item.Account + " 的帳戶發查沖正電文成功;<br/>";
                                    }
                                    else
                                    {
                                        Message = Message + "存款帳號為:" + item.Account + " 的帳戶發查沖正電文失敗,原因是 " + result + ";<br/>";
                                    }
                                }
                                //發查電文做事故
                                if (functionSeizureId == "Accident")
                                {
                                    rtn = caseAccount.AccidentTelegram(item, "Seizure", LogonUser, CaseId);
                                    if (rtn)
                                    {
                                        Message = Message + "存款帳號為:" + item.Account + " 的帳戶發查事故電文成功;<br/>";
                                    }
                                    else
                                    {
                                        Message = Message + "存款帳號為:" + item.Account + " 的帳戶發查事故電文失敗;<br/>";
                                    }
                                }
                            }
                        }
                        if (functionSeizureId == "Seizure" && rtn)
                        {
                            caseAccount.UserId = userId;
                            caseAccount.PageFrom = pageFromName;
                            rtn = caseAccount.SaveSeizureSetting(listForm, memo, CaseId);//如果網頁上 勾選 扣押 後,全部儲存到caseseisure IR-0063
                            if (!rtn)
                                Message = Message + "其他帳戶儲存失敗!<br/>";
                        }
                    }
                }
                else
                {
                    Message = "抱歉,您的RCAF帳號登入異常,無法使用此功能!";
                }
                return Json(rtn ? new JsonReturn { ReturnCode = "1", ReturnMsg = Message }
                                : new JsonReturn { ReturnCode = "0", ReturnMsg = Message });
            }
        }

        public ActionResult GetBranchName(string codeNo)
        {
            return Content(new PARMCodeBIZ().GetCodeDescByCodeNo(codeNo));
        }

        public ActionResult CustomerInfo(string custId,Guid caseId)
        {
            return View(CIBZ.GetData(custId,caseId));
        }
        public void Bind()
        {
            ViewBag.CurrencyList = new SelectList(caseMaster.GetCodeData("CURRENCY"), "CodeNo", "CodeMemo");//抓取中信外幣現鈔買匯匯率，CodeMemo
            ViewBag.AccountStatusList = new SelectList(caseMaster.GetCodeData("ACCOUNT_STATUS"), "CodeNo", "CodeNo");
            ViewBag.BranchNoList = new SelectList(caseMaster.GetCodeData("RCAF_BRANCH"), "CodeNo", "CodeDesc");
            ViewBag.BranchName = new SelectList(caseMaster.GetCodeData("RCAF_BRANCH"), "CodeNo", "CodeDesc").FirstOrDefault().Text;
            ViewBag.MemoList = new SelectList(caseMaster.GetCodeData("CaseSeizureMemo"), "CodeMemo", "CodeDesc");
        }

        public ActionResult QueryCaseSeizureMemo(Guid CaseID)
        {
            AgentDepartmentAccess ADAccess = new AgentDepartmentAccess();
            ADAccess.CaseId = CaseID;
            AgentDepartmentAccessViewModel model = new AgentDepartmentAccessViewModel()
            {
                AgentDeptAccess = ADAccess,
            };
            ViewBag.MemoList = new SelectList(caseMaster.GetCodeData("CaseSeizureMemo"), "CodeMemo", "CodeDesc");
            return PartialView("QueryCaseSeizureMemo", model);
        }

        public ActionResult ResetStatus(Guid CaseId)
        {
            return Json(caseAccount.ResetStatus(CaseId) > 0 ? new JsonReturn { ReturnCode = "1", ReturnMsg = "" }
                                               : new JsonReturn { ReturnCode = "0", ReturnMsg = Lang.csfs_del_fail });
        }
        #endregion

        public ActionResult OpenTxtDoc(Guid CaseId)
        {
            CaseEdocFile caseEdocFile = caseAccount.OpenTxtDoc(CaseId);
            string text = string.Empty;
            if (caseEdocFile != null)
            {
                byte[] file = caseEdocFile.FileObject;
                text = Encoding.UTF8.GetString(file);
            }
            string ReturnMsg = string.IsNullOrEmpty(text) ? Lang.csfs_txtdocnotfound : text;
            return PartialView("TxtOpen", new CaseMaster { Memo = ReturnMsg });
            //return Json(string.IsNullOrEmpty(text) ? new JsonReturn { ReturnCode = "0", ReturnMsg = Lang.csfs_txtdocnotfound } : new JsonReturn { ReturnCode = "1", ReturnMsg = text });
        }

		public ActionResult OpenPdfDoc(string caseId)
		{
            string strCaseId = "";
            string strType = "";
            int at = caseId.IndexOf("|");
            if (at > 0)
            {
                strCaseId = caseId.Substring(0, at);
                strType = caseId.Substring(at + 1, (caseId.Length - at - 1));
            }
            if (strType == "di")
            {
                CaseEdocFile caseEdocFile = caseAccount.OpenPayPdfDoc1(caseId);           
                string text = string.Empty;
                if (caseEdocFile != null)
                {
                    byte[] file = caseEdocFile.FileObject;
                    return File(file, "application/pdf", caseEdocFile.FileName);
                }
                else
                {
                    return Content("<script>alert('" + Lang.csfs_no_data + "');</script>");
                }
            }
            else if (strType == "Attach")
            {
                CaseEdocFile caseEdocFile = caseAccount.OpenPayPdfDoc2(caseId);
                string text = string.Empty;
                if (caseEdocFile != null)
                {
                    byte[] file = caseEdocFile.FileObject;
                    return File(file, "application/pdf", caseEdocFile.FileName);
                }
                else
                {
                    return Content("<script>alert('" + Lang.csfs_no_data + "');</script>");
                }
            }
            else
            {
                CaseEdocFile caseEdocFile = caseAccount.OpenPdfDoc(caseId);
                string text = string.Empty;
                if (caseEdocFile != null)
                {
                    byte[] file = caseEdocFile.FileObject;
                    return File(file, "application/pdf", caseEdocFile.FileName);
                }
                else
                {
                    return Content("<script>alert('" + Lang.csfs_no_data + "');</script>");
                }
            }


        }
        public ActionResult OpenPayPdfDoc1(string caseId)
        {
            CaseEdocFile caseEdocFile = caseAccount.OpenPayPdfDoc1(caseId);
            string text = string.Empty;
            if (caseEdocFile != null)
            {
                byte[] file = caseEdocFile.FileObject;
                return File(file, "application/pdf", caseEdocFile.FileName);
            }
            else
            {
                return Content("<script>alert('" + Lang.csfs_no_data + "');</script>");
            }
        }
        public ActionResult OpenPayPdfDoc2(string caseId)
        {
            CaseEdocFile caseEdocFile = caseAccount.OpenPayPdfDoc2(caseId);
            string text = string.Empty;
            if (caseEdocFile != null)
            {
                byte[] file = caseEdocFile.FileObject;
                return File(file, "application/pdf", caseEdocFile.FileName);
            }
            else
            {
                return Content("<script>alert('" + Lang.csfs_no_data + "');</script>");
            }
        }
        //電文記錄
        public ActionResult OpenEmsg(Guid caseId)
        {
            List<BatchQueue> CaseQueryList = caseAccount.StatusList(caseId);
            if (CaseQueryList == null || !CaseQueryList.Any())
                CaseQueryList = new List<BatchQueue>();
            return PartialView("Emessage", CaseQueryList);
        }
        //呈核
        private bool ChengHe(Guid CaseId)
        {
            string userId = LogonUser.Account;
            CaseSendSettingBIZ cssBIZ = new CaseSendSettingBIZ();
            AgentToHandleBIZ AToHBIZ = new AgentToHandleBIZ();
            SendSettingRefBiz refBiz = new SendSettingRefBiz();
            bool result = false;
            //* 案件主表信息
            CaseMaster master = caseMaster.MasterModel(CaseId);
            //* 讀取該案件以前發文儲存的資料
            IList<CaseSendSettingQueryResultViewModel> listRtn = cssBIZ.GetSendSettingList(CaseId);
            //* 用以返回的viewmodel
            CaseSendSettingCreateViewModel css = new CaseSendSettingCreateViewModel
            {
                CaseId = CaseId,
                ReceiveKind = master.ReceiveKind,
                ReceiveList = new List<CaseSendSettingDetails>(),
                CcList = new List<CaseSendSettingDetails>()
            };
            if(master.CaseKind == Lang.csfs_menu_tit_caseseizure && master.CaseKind2 == Lang.csfs_repeal)
            {
                result = true;//撤銷案件不會產生發文檔，直接呈核主管
            }
            else
            {
                if (listRtn != null && listRtn.Any())
                {
                    result = true;//已經產生過發文檔就不再新增
                }
                else
                {
                    #region 產生發文檔
                    //* 取得以前沒有更新發文資訊的
                    IList<CasePayeeSetting> cpsList = new CasePayeeSettingBIZ().GetPayeeSettingWhichNotSendSetting(CaseId);
                    //來文機關資料.取資料
                    string govAddr = new GovAddressBIZ().GetEnabledGovAddrByGovName(master.GovUnit);
                    #region SendDate
                    if (master.CaseKind2 == Lang.csfs_seizure)
                    {
                        //* 扣押 (當天)
                        css.SendDate = Convert.ToDateTime(UtlString.FormatDateTw(DateTime.Today.ToString("yyyy/MM/dd")));
                    }
                    else if (master.CaseKind2 == Lang.csfs_seizureandpay && master.AfterSeizureApproved != 1)
                    {
                        //* 扣押並支付 的扣押 (當天)
                        css.SendDate = Convert.ToDateTime(UtlString.FormatDateTw(DateTime.Today.ToString("yyyy/MM/dd")));
                    }
                    else if (ViewBag.CaseKind2 == Lang.csfs_Pay)
                    {
                        //* 支付類 (看Master)
                        css.SendDate = Convert.ToDateTime(UtlString.FormatDateTw(master.PayDate));
                    }
                    else if (master.CaseKind2 == Lang.csfs_seizureandpay && master.AfterSeizureApproved == 1)
                    {
                        //* 扣押並支付 的支付(看Master)
                        css.SendDate = Convert.ToDateTime(UtlString.FormatDateTw(master.PayDate));
                    }
                    else
                    {
                        //* 其他(當天)
                        css.SendDate = Convert.ToDateTime(UtlString.FormatDateTw(DateTime.Today.ToString("yyyy/MM/dd")));
                    }
                    #endregion
                    if (master.CaseKind == Lang.csfs_receive_case)
                    {
                        //3.外來文 發文正本預設為來文機關
                        css.ReceiveList.Add(new CaseSendSettingDetails { CaseId = CaseId, SendType = 1, GovName = master.GovUnit, GovAddr = govAddr });
                        if (master.CaseKind2 == Lang.csfs_165_reading)
                        {
                            css.Template = Lang.csfs_165_reading;
                        }
                        else if (master.CaseKind2 == Lang.csfs_property_declaration1)
                        {
                            css.Template = Lang.csfs_property_declaration1;
                        }
                        else
                        {
                            css.Template = Lang.csfs_not_165_reading;
                        }
                    }
                    if (master.CaseKind == Lang.csfs_menu_tit_caseseizure)
                    {

                        if (master.CaseKind2 == Lang.csfs_seizure)
                        {
                            //1.扣押 發文正本預設為來文機關
                            css.ReceiveList.Add(new CaseSendSettingDetails { CaseId = CaseId, SendType = 1, GovName = master.GovUnit, GovAddr = govAddr });
                            css.Template = Lang.csfs_seizure;
                        }
                        else if (master.CaseKind2 == Lang.csfs_Pay)
                        {
                            //2.支付 發文副本預設為來文機關
                            css.CcList.Add(new CaseSendSettingDetails { CaseId = CaseId, SendType = 2, GovName = master.GovUnit, GovAddr = govAddr });
                            css.Template = Lang.csfs_Pay;
                        }
                        else if (master.CaseKind2 == Lang.csfs_seizureandpay)
                        {
                            //1.扣押 發文正本預設為來文機關
                            css.ReceiveList.Add(new CaseSendSettingDetails { CaseId = CaseId, SendType = 1, GovName = master.GovUnit, GovAddr = govAddr });
                            css.Template = master.AfterSeizureApproved == 1 ? Lang.csfs_Pay : Lang.csfs_seizure;
                        }
                        else
                        {
                            //1.扣押 發文正本預設為來文機關
                            css.ReceiveList.Add(new CaseSendSettingDetails { CaseId = CaseId, SendType = 1, GovName = master.GovUnit, GovAddr = govAddr });
                        }
                    }
                    //* 沒有發文的受款人正本.副本
                    if (cpsList != null && cpsList.Any())
                    {
                        foreach (CasePayeeSetting item in cpsList)
                        {
                            css.ReceiveList.Add(new CaseSendSettingDetails() { CaseId = CaseId, SendType = 1, GovName = item.Receiver, GovAddr = item.Address });
                            css.CcList.Add(new CaseSendSettingDetails() { CaseId = CaseId, SendType = 2, GovName = item.CCReceiver, GovAddr = item.Currency });
                        }
                    }
                    css.SendKind = master.ReceiveKind == "紙本" ? "紙本發文" : "電子發文";
                    css.SendWord = master.ReceiveKind == "紙本" ? Lang.csfs_ctci_bank : cssBIZ.GetFirstCodeDataByDesc("SendGovName", "").CodeDesc;
                    css.SendNo = master.SendNo;
                    css.Speed = master.Speed;
                    css.Security = Lang.csfs_security1;
                    SendSettingRef SSref = refBiz.GetSubjectAndDescription(CaseId, css.Template, css.SendKind);
                    css.Subject = SSref.Subject;
                    css.Description = SSref.Description;
                    #endregion

                    #region 儲存發文檔
                    css.SendDate = UtlString.FormatDateTwStringToAd(css.SendDate);
                    result = cssBIZ.SaveCreate(css);
                    #endregion
                }
            }

            #region 呈核主管
            if (result)
            {
                //adam 又改回呈核主管不取號
                //呈核時才產生發文字號(IR-0015)
                //if (css.SendKind == "電子發文")
                //{
                //    css.SendNo = cssBIZ.SendNo();
                //    css.SendNo = (DateTime.Now.Year - 1911) + "2" + css.SendNo.Substring(9);
                //}
                //else
                //{
                //    css.SendNo = cssBIZ.SendNo();
                //}
                //result = cssBIZ.UpdateCaseSendSetting(css);
                string[] caseid = CaseId.ToString().Split(','); ;
                List<Guid> aryCaseId = (from id in caseid where !string.IsNullOrEmpty(id) select new Guid(id)).ToList();
                string agentIdList = AToHBIZ.GetManagerID(userId);
                caseid = agentIdList.Split(',');
                List<string> aryAgentId = (from id in caseid where !string.IsNullOrEmpty(id) select id).ToList();
                JsonReturn Return = AToHBIZ.AgentSubmit(aryCaseId, aryAgentId, userId);
                if (Return.ReturnCode == "1")
                {
                    result = true;
                }
                else
                {
                    result = false;
                }
            }
            #endregion
            return result;
        }
        //事故資訊
        public ActionResult AccidentInfo(string Account, Guid caseId,string ccy)
        {
            return View(CIBZ.GetTx00450List(Account, caseId, "30",ccy));
        }

        #region 扣押维护
        /// <summary>
        /// 扣押维护 顯示
        /// </summary>
        /// <param name="caseId"></param>
        /// <returns></returns>
        public ActionResult SeizureMaintain(Guid caseId)
        {
            List<CaseObligor> list = new CaseObligorBIZ().ObligorModel(caseId);    //* 得到CaseObligor的model
            ViewBag.ObligorNoList = new SelectList(list, "ObligorNo", "ObligorNoAndName");
            ViewBag.BranchNoList = new SelectList(caseMaster.GetCodeData("RCAF_BRANCH"), "CodeNo", "CodeDesc");
            ViewBag.BranchName = new SelectList(caseMaster.GetCodeData("RCAF_BRANCH"), "CodeNo", "CodeDesc").FirstOrDefault().Text;
            ViewBag.CurrencyList = new SelectList(caseMaster.GetCodeData("CURRENCY"), "CodeNo", "CodeNo", "TWD");

            return View();
        }

        /// <summary>
        /// 扣押维护 儲存
        /// </summary>
        /// <param name="listForm"></param>
        /// <param name="memo"></param>
        /// <returns></returns>
        public ActionResult DoSaveSeizureMaintain(List<CaseSeizure> listForm)
        {
            string userId = LogonUser.Account;
            var query = from lists in listForm
                        orderby lists.CaseNo ascending
                        select lists;

            if (query != null && query.Any())
            {
                foreach (CaseSeizure item in query)
                {
                    item.Balance = item.SeizureAmount.ToString();
                    item.CreatedUser = userId;
                    item.ModifiedUser = userId;
                }
            }
            return Json(caseAccount.SaveSeizureSetting(query.ToList()));
        }


        /// <summary>
        /// 扣押维护编辑查询
        /// </summary>
        /// <param name="caseId"></param>
        /// <returns></returns>
        public ActionResult SeizureMaintainQuery(Guid caseId)
        {
            ViewBag.CaseId = caseId;
            return View();
        }

        /// <summary>
        /// 查詢結果grid顯示
        /// </summary>
        /// <returns></returns>
        public ActionResult SeizureMaintainQueryResult(Guid CaseId)
        {
            ViewBag.caseId = CaseId;
            List<CaseSeizure> listRtn = new CaseAccountBiz().QuerySeizureMaintainList(CaseId);
            return View(listRtn);
        }

        public ActionResult SeizureMaintainEdit(string seizureId, Guid caseid)
        {
            ViewBag.caseId = caseid;

            List<CaseObligor> list = new CaseObligorBIZ().ObligorModel(caseid);    //* 得到CaseObligor的model
            ViewBag.ObligorNoList = new SelectList(list, "ObligorNo", "ObligorNoAndName");
            ViewBag.BranchNoList = new SelectList(caseMaster.GetCodeData("RCAF_BRANCH"), "CodeNo", "CodeNo");
            ViewBag.CurrencyList = new SelectList(caseMaster.GetCodeData("CURRENCY"), "CodeNo", "CodeNo", "TWD");

            CaseSeizure model = new CaseAccountBiz().GetCaseSeizure(seizureId);
            return View(model);
        }

        [HttpPost]
        public ActionResult SeizureMaintainEdit(CaseSeizure model)
        {
            return Json(new CaseAccountBiz().SeizureMaintainEdit(model) ? new JsonReturn { ReturnCode = "1", ReturnMsg = "" }
                                                : new JsonReturn { ReturnCode = "0", ReturnMsg = Lang.csfs_del_fail });
        }

        public ActionResult Delete(int seizureId, Guid caseId, string caseno)
        {
            return Json(new CaseAccountBiz().DeleteSeizureMaintain(caseId, seizureId, caseno) ? new JsonReturn { ReturnCode = "1", ReturnMsg = "" }
                                               : new JsonReturn { ReturnCode = "0", ReturnMsg = Lang.csfs_del_fail });

            //var scriptStr = new CaseAccountBiz().DeleteAttatch(attachIds) > 0 ? "parent.jAlertSuccess('" + Lang.csfs_del_ok + "', function () { parent.$.colorbox.close();parent.location.reload();})"
            //                                             : "parent.jAlertSuccess('" + Lang.csfs_edit_fail + "',function () { parent.$.colorbox.close();parent.location.reload();});";

            //return Content(@"<script>" + scriptStr + "</script>");
        }
        #endregion

        #region 支付設定
        /// <summary>
        /// 扣押設定 顯示
        /// </summary>
        /// <param name="caseId"></param>
        /// <returns></returns>
        public ActionResult _PaySetting(Guid caseId)
        {
            string CustId = "";
            ViewBag.CaseId = caseId;
            PaySettingViewModel model = new PaySettingViewModel();
            CaseMaster master = caseMaster.MasterModel(caseId);
            if (master == null)
                return Content(Lang.csfs_not_caseseizure);
            if (master.CaseKind != CaseKind.CASE_SEIZURE)
                return Content(Lang.csfs_not_caseseizure);
            model.GovNo = master.GovNo;//查電文時，備註中要用到的來文字號是caseMaster表中的GovNo
            model.CaseKind = master.CaseKind;
            model.CaseKind2 = master.CaseKind2;
            model.CaseId = caseId;

            CheckQueryAndPrintBIZ CKP = new CheckQueryAndPrintBIZ();
            string DTSRC_Date = caseMaster.GetPayDate(master.CaseKind2, master.CreatedDate).ToString("yyyy/MM/dd");//解扣日:上周四到本周一 顯示 本周三日期, 本周二,三建檔日為下周三
            DTSRC_Date = CKP.GetWorkingDays(DTSRC_Date);//如遇非工作日,自動順延一個營業日
            DTSRC_Date = Convert.ToDateTime(DTSRC_Date).ToString("yyyy/MM/dd");
            model.BreakDay = DTSRC_Date;

            ViewBag.CaseKind2 = master.CaseKind2;

            IList<CaseSeizure> listPay = caseAccount.GetCaseSeizureWithEPaySetting(caseId);
            // 新增個資LOG
            string _user = (HttpContext.Session["UserAccount"] != null) ? HttpContext.Session["UserAccount"].ToString() : "";
            System.Collections.Specialized.NameValueCollection _parameters = new System.Collections.Specialized.NameValueCollection();
            //System.Collections.Specialized.NameValueCollection _parameters = ("POST".Equals(HttpContext.Request.HttpMethod)) ? HttpContext.Request.Form : HttpContext.Request.QueryString;
            string _controller = (this.ControllerContext.RouteData.Values["Controller"] != null) ? this.ControllerContext.RouteData.Values["Controller"].ToString() : "";
            string _action = (this.ControllerContext.RouteData.Values["Action"] != null) ? this.ControllerContext.RouteData.Values["Action"].ToString() : "";
            string _ip = (HttpContext.Request != null && !string.IsNullOrEmpty(HttpContext.Request.ServerVariables["REMOTE_ADDR"])) ? HttpContext.Request.ServerVariables["REMOTE_ADDR"] : "";
            BaseBusinessRule _business = new BaseBusinessRule();
            //var list = caseAccount.GetCaseSeizureWithCancel(caseId).ToList();
            if (listPay.Count > 0)
            {
                //for (int i = 0; i < list.Count; i++)
                //{
                CustId = listPay[0].CustId.Trim();
                if (CustId.Length > 0)
                {
                    _business.ExecuteAPLOGSave(_user, _controller, _action, _ip, "CASEID=" + caseId.ToString(), CustId);
                }
                //}
            }
            // 新增結束
            if (listPay != null && listPay.Any())
            {
                //* 已儲存的
                model.AlreadySaved = true;
                model.ListPay = listPay;
                model.ListSeizure = new List<CaseSeizure>();
            }

            if (master.CaseKind2 == CaseKind2.CaseSeizureAndPay)
            {
                model.ListSeizure = caseAccount.GetCaseSeizureWithoutPaySetting(caseId);
            }
            else
            {
                model.ListSeizure = caseAccount.GetCaseSeizureWithoutPaySettingByCustomerList(caseId);
            }
            return View(model);
        }

        public ActionResult DoSavePaySetting(PaySettingViewModel model, Guid CaseId, string functionPayId, string BreakDay)
        {
            List<CaseSeizure> listSave = new List<CaseSeizure>();
            if (model != null && model.ListPay != null && model.ListPay.Any())
            {
                listSave = model.ListPay.Where(x => x.Account != null).ToList();
            }
            if (listSave != null && listSave.Any())
            {
                foreach (CaseSeizure it in listSave)
                {

                    string strSeizureId = it.SeizureId.ToString();
                    CaseSeizure cs = caseAccount.GetCaseSeizure(strSeizureId);
                    it.CaseId = cs.CaseId;
                }
            }
                    string userId = LogonUser.Account;
            #region 儲存
            if (string.IsNullOrEmpty(functionPayId))
            {
                if (listSave != null && listSave.Any())
                {
                    foreach (CaseSeizure item in listSave)
                    {
                        //* 補上差的欄位
                        item.PayCaseId = model.CaseId;
                        item.ModifiedUser = userId;
                        if (item.AccountStatus == null || item.AccountStatus == "")
                        {
                            item.AccountStatus = "";
                        }
                    }
                }

                return Json(caseAccount.SavePaySetting(listSave, CaseId) ? new JsonReturn { ReturnCode = "1", ReturnMsg = "" }
                                                            : new JsonReturn { ReturnCode = "0", ReturnMsg = Lang.csfs_save_fail });
            }
            #endregion
            else
            {
                bool rtn = false;
                string Message = "";
                string result = "";
                var master = caseMaster.MasterModel(CaseId);

                if (master.GovUnit.Length == 0)
                {
                    if (master.ReceiveKind == "紙本")
                    {
                        master.GovUnit = "地方法院";
                    }
                    else
                    {
                        master.GovUnit = "執行署";
                    }
                }
                string strMemo = getNewMemo(master.GovUnit, master.GovNo);
                //string strMemo = model.GovNo.Substring(0, 3) + model.GovNo.Substring(model.GovNo.Length - 7, 6);//查電文時，備註中要用到的來文字號是caseMaster表中的GovNo
                //當經辦登入外來文系統時未輸入RCAF的帳號、密碼及分行時，登入之權限應不可執行任何有關電文的交易按鈕。 IR-0048
                if (!string.IsNullOrEmpty(LogonUser.RCAFAccount.Trim()) && !string.IsNullOrEmpty(LogonUser.RCAFPs.Trim()) && !string.IsNullOrEmpty(LogonUser.RCAFBranch.Trim()))
                {
                    if (listSave != null && listSave.Any())
                    {
                        foreach (CaseSeizure item in listSave)
                        {
                            item.PayCaseId = model.CaseId;
                            if (item.IsCheck == "true")
                            {
                                //* 補上差的欄位
                                //item.PayCaseId = model.CaseId;
                                item.ModifiedUser = userId;
                                //支付
                                if (functionPayId == "Pay")
                                {
                                    result = caseAccount.PayTelegram(item, strMemo, CaseId, BreakDay, "Pay", LogonUser);
                                    if (result == "true")
                                    {
                                        rtn = true;
                                        Message = Message + "存款帳號為:" + item.Account + " 的帳戶發查支付電文成功;<br/>";
                                        
                                    }
                                    else
                                    {
                                        Message = Message + "存款帳號為:" + item.Account + " 的帳戶發查支付電文失敗,原因是 " + result + ";<br/>";
                                    }
                                }
                                //沖正
                                else if (functionPayId == "Reset")
                                {
                                    result = caseAccount.PayTelegram(item, strMemo, CaseId, "", "Reset", LogonUser);
                                    if (result == "true")
                                    {
                                        rtn = true;
                                        Message = Message + "存款帳號為:" + item.Account + " 的帳戶發查沖正電文成功;<br/>";
                                    }
                                    else
                                    {
                                        Message = Message + "存款帳號為:" + item.Account + " 的帳戶發查沖正電文失敗,原因是 " + result + ";<br/>";
                                    }
                                }
                                //事故
                                else if (functionPayId == "Accident")
                                {
                                    rtn = caseAccount.AccidentTelegram(item, "Pay", LogonUser, CaseId);
                                    if (rtn)
                                    {
                                        Message = Message + "存款帳號為:" + item.Account + " 的帳戶發查事故電文成功;<br/>";
                                    }
                                    else
                                    {
                                        Message = Message + "存款帳號為:" + item.Account + " 的帳戶發查事故電文失敗;<br/>";
                                    }
                                }
                            }
                        }
                        //支付案件留在本案件區(支付、沖正、事故、其他不發查電文的案件直接儲存) 20181101
                        rtn = caseAccount.SavePaySetting(listSave, CaseId);//如果網頁上 勾選 支付 後,全部儲存到caseseisure IR-2010
                        if (!rtn)
                            Message = Message + "其他帳戶儲存失敗!<br/>";
                        //if (functionPayId == "Pay" && rtn)
                        //{
                        //    caseAccount.UserId = userId;
                        //    caseAccount.PageFrom = pageFromName;
                        //    rtn = caseAccount.SavePaySetting(listSave, CaseId);//如果網頁上 勾選 支付 後,全部儲存到caseseisure IR-2010
                        //    if (!rtn)
                        //        Message = Message + "其他帳戶儲存失敗!<br/>";
                        //}
                    }
                }
                else
                {
                    Message = "抱歉,您的RCAF帳號登入異常,無法使用此功能!";
                }
                return Json(rtn ? new JsonReturn { ReturnCode = "1", ReturnMsg = Message }
                                : new JsonReturn { ReturnCode = "0", ReturnMsg = Message });
            }
        }

        #endregion

        #region 受款人設定
        /// <summary>
        /// 扣押設定 顯示
        /// </summary>
        /// <param name="caseId"></param>
        /// <returns></returns>
        public ActionResult _PayeeSetting(CasePayeeSetting cps)
        {
            //Bind();
            ViewBag.CaseId = cps.CaseId;
            ViewBag.PayAmount = string.Format("{0:N0}", cpsBIZ.PayAmountSum(cps.CaseId));
            ViewBag.MoneySum = string.Format("{0:N0}", cpsBIZ.MoneySum(cps.CaseId));

            //ViewBag.PayAmount = cpsBIZ.PayAmount(cps.CaseId);
            return View(SearchList(cps, cps.CaseId));
        }

        public CasePayeeSettingViewModel SearchList(CasePayeeSetting cps, Guid caseId)
        {
            string CustId = "";
            CasePayeeSettingViewModel viewModel;
            cps.LanguageType = Session["CultureName"].ToString();
            IList<CasePayeeSetting> result = cpsBIZ.GetQueryList(cps);
            // 新增個資LOG
            string _user = (HttpContext.Session["UserAccount"] != null) ? HttpContext.Session["UserAccount"].ToString() : "";
            System.Collections.Specialized.NameValueCollection _parameters = new System.Collections.Specialized.NameValueCollection();
            //System.Collections.Specialized.NameValueCollection _parameters = ("POST".Equals(HttpContext.Request.HttpMethod)) ? HttpContext.Request.Form : HttpContext.Request.QueryString;
            string _controller = (this.ControllerContext.RouteData.Values["Controller"] != null) ? this.ControllerContext.RouteData.Values["Controller"].ToString() : "";
            string _action = (this.ControllerContext.RouteData.Values["Action"] != null) ? this.ControllerContext.RouteData.Values["Action"].ToString() : "";
            string _ip = (HttpContext.Request != null && !string.IsNullOrEmpty(HttpContext.Request.ServerVariables["REMOTE_ADDR"])) ? HttpContext.Request.ServerVariables["REMOTE_ADDR"] : "";
            BaseBusinessRule _business = new BaseBusinessRule();
            //var list = caseAccount.GetCaseSeizureWithCancel(caseId).ToList();
            if (result.Count > 0)
            {
                //for (int i = 0; i < list.Count; i++)
                //{
                CustId = result[0].ReceivePerson.Trim();
                if (CustId.Length > 0)
                {
                    _business.ExecuteAPLOGSave(_user, _controller, _action, _ip, "CASEID=" + caseId.ToString(), CustId);
                }
                //}
            }
            // 新增結束
            CaseMaster master = new CaseMasterBIZ().MasterModel(caseId);

            viewModel = new CasePayeeSettingViewModel()
            {
                CasePayeeSetting = cps,
                CasePayeeSettingList = result,
                CaseKind = cpsBIZ.CaseKind(caseId),
                //BankID=cpsBIZ.BankID(caseId),
                Bank = cpsBIZ.BankForPay(caseId),
                PayAmountSum = cpsBIZ.PayAmountSum(caseId),
                MoneySum = cpsBIZ.MoneySum(caseId),
                Currency = master.GovUnit,
                CCReceiver = new GovAddressBIZ().GetEnabledGovAddrByGovName(master.GovUnit),
            };

            return viewModel;
        }

        //修改
        public ActionResult _PayeeEdit(int PayeeId)
        {
            //Bind();
            CasePayeeSetting model = cpsBIZ.Select(PayeeId);
            model.PayAmountSum = cpsBIZ.PayAmountSum(model.CaseId);
            model.MoneySum = cpsBIZ.MoneySum(model.CaseId, PayeeId);
            ViewBag.EditMode = "_PayeeEdit";
            return View("_PayeeEdit", model);
        }

        [HttpPost]
        public ActionResult _PayeeEdit(CasePayeeSetting model)
        {
            string userId = LogonUser.Account;
            return Json(cpsBIZ.Edit(model,userId));
        }

        /// <summary>
        /// 新增受款人
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult CreateCPS(CasePayeeSetting model)
        {
            string userId = LogonUser.Account;
            //model.CasePayeeSetting = new CasePayeeSetting();
            return Json(cpsBIZ.Create(model, userId));
        }
        //刪除
        public ActionResult _PayeeDelete(int payeeId)
        {
            return Json(cpsBIZ.Delete(payeeId));
        }
        public ActionResult _PayeeSelectAddress(string GovName)
        {
            return Content(cpsBIZ.Address(GovName));
        }

        
        #endregion

        #region 撤銷設定
        /// <summary>
        /// 撤銷設定 顯示
        /// </summary>
        /// <param name="caseId"></param>
        /// <returns></returns>
        public ActionResult _SeizureCancel(Guid caseId)
        {
            string CustId = "";
            ViewBag.CaseId = caseId;
            var master = caseMaster.MasterModel(caseId);
            ViewBag.CaseKind = master.CaseKind;
            ViewBag.CaseKind2 = master.CaseKind2;

            CancelSeizureViewModel model = new CancelSeizureViewModel
            {
                CaseId = caseId,
                ListSaved = caseAccount.GetCaseSeizureWithCancel(caseId),
                QueryResult = caseAccount.GetCaseSeizureWithoutPaySettingByCustomerList(caseId),
                GovNo = master.GovNo//查電文時，備註中要用到的來文字號是caseMaster表中的GovNo
            };
            //ViewBag.CheckCaseSeizure = new SelectList(caseAccount.CheckCaseSeizure(caseId), "ObligorNo", "ObligorName");
            // 新增個資LOG
            string _user = (HttpContext.Session["UserAccount"] != null) ? HttpContext.Session["UserAccount"].ToString() : "";
            System.Collections.Specialized.NameValueCollection _parameters = new System.Collections.Specialized.NameValueCollection();
            //System.Collections.Specialized.NameValueCollection _parameters = ("POST".Equals(HttpContext.Request.HttpMethod)) ? HttpContext.Request.Form : HttpContext.Request.QueryString;
            string _controller = (this.ControllerContext.RouteData.Values["Controller"] != null) ? this.ControllerContext.RouteData.Values["Controller"].ToString() : "";
            string _action = (this.ControllerContext.RouteData.Values["Action"] != null) ? this.ControllerContext.RouteData.Values["Action"].ToString() : "";
            string _ip = (HttpContext.Request != null && !string.IsNullOrEmpty(HttpContext.Request.ServerVariables["REMOTE_ADDR"])) ? HttpContext.Request.ServerVariables["REMOTE_ADDR"] : "";
            BaseBusinessRule _business = new BaseBusinessRule();
            var list = caseAccount.GetCaseSeizureWithCancel(caseId).ToList();
            if (list.Count > 0)
            {
                //for (int i = 0; i < list.Count; i++)
                //{
                if (!String.IsNullOrEmpty(list[0].CustId))
                {
                    CustId = list[0].CustId.Trim();
                    if (CustId.Length > 0)
                    {
                        _business.ExecuteAPLOGSave(_user, _controller, _action, _ip, "CASEID=" + caseId.ToString(), CustId);
                    }
                }
                //}
            }
            // 新增結束
            return View(model);
        }

        public ActionResult DoSaveSeizureCancel(CancelSeizureViewModel model, Guid caseId, string functionCancelId)
        {
            List<CaseSeizure> listSave = new List<CaseSeizure>();
            if (model != null && model.ListSaved != null && model.ListSaved.Any())
            {
                //listSave = model.ListSaved.ToList();
                foreach (var item in model.ListSaved.ToList())
                {
                    if (item != null && item.SeizureId > 0 &&  !string.IsNullOrEmpty(item.Account))
                    {
                        listSave.Add(item);
                    }
                }
            }


            string userId = LogonUser.Account;
            #region 儲存
            if (string.IsNullOrEmpty(functionCancelId))
            {
                if (listSave != null && listSave.Any())
                {
                    foreach (CaseSeizure item in listSave)
                    {
                        //* 補上差的欄位
                        item.PayCaseId = model.CaseId;
                        item.ModifiedUser = userId;
                    }
                }

                return Json(caseAccount.SaveCaseSeizureCancel(caseId, listSave) ? new JsonReturn { ReturnCode = "1", ReturnMsg = Lang.csfs_save_ok }
                                                            : new JsonReturn { ReturnCode = "0", ReturnMsg = Lang.csfs_save_fail });

                //if (string.IsNullOrEmpty(seizureList))
                //    return Json(new JsonReturn { ReturnCode = "0", ReturnMsg = Lang.csfs_edit_fail });

                //List<string> ary = seizureList.Split(',').ToList();

                //return Json(caseAccount.SaveCaseSeizureCancel(caseId, ary, LogonUser.Account)
                //                                            ? new JsonReturn { ReturnCode = "1", ReturnMsg = Lang.csfs_save_ok }
                //                                            : new JsonReturn { ReturnCode = "0", ReturnMsg = Lang.csfs_save_fail });
            }
            #endregion
            #region 呈核
            else if (functionCancelId == "Nuclear")
            {
                bool result = ChengHe(caseId);
                return Json(result ? new JsonReturn { ReturnCode = "1", ReturnMsg = Lang.csfs_nuclear_success }
                                  : new JsonReturn { ReturnCode = "0", ReturnMsg = Lang.csfs_nuclear_fail });
            }
            #endregion
            else
            {
                bool rtn = false;
                string Message = "";
                string result = "";
                var master = caseMaster.MasterModel(caseId);
                if (master.GovUnit.Length == 0)
                {
                    if (master.ReceiveKind == "紙本")
                    {
                        master.GovUnit = "地方法院";
                    }
                    else
                    {
                        master.GovUnit = "執行署";
                    }
                }
                string strMemo = getNewMemo(master.GovUnit, master.GovNo);
                //string strMemo = model.GovNo.Substring(0, 3) + model.GovNo.Substring(model.GovNo.Length - 7, 6);//查電文時，備註中要用到的來文字號是caseMaster表中的GovNo
                //當經辦登入外來文系統時未輸入RCAF的帳號、密碼及分行時，登入之權限應不可執行任何有關電文的交易按鈕。 IR-0048
                if (!string.IsNullOrEmpty(LogonUser.RCAFAccount.Trim()) && !string.IsNullOrEmpty(LogonUser.RCAFPs.Trim()) && !string.IsNullOrEmpty(LogonUser.RCAFBranch.Trim()))
                {
                    if (listSave != null && listSave.Any())
                    {
                        foreach (CaseSeizure item in listSave)
                        {
                            //* 補上差的欄位
                            item.PayCaseId = model.CaseId;
                            item.ModifiedUser = userId;
                            if (item.IsCheck == "true")
                            {
                                //撤銷(同扣押的沖正)
                                if (functionCancelId == "Cancel")
                                {
                                    result = caseAccount.ResetTelegram(item, strMemo, caseId, "Cancel", LogonUser,"");
                                    if (result == "true")
                                    {
                                        rtn = true;
                                        Message = Message + "存款帳號為:" + item.Account + " 的帳戶發查撤銷電文成功;<br/>";
                                    }
                                    else
                                    {
                                        Message = Message + "存款帳號為:" + item.Account + " 的帳戶發查撤銷電文失敗,原因是 " + result + ";<br/>";
                                    }
                                }
                                //沖正(同扣押的扣押)
                                if (functionCancelId == "Reset")
                                {
                                    // 20201112, IR_2025, 發現, 若是撒銷沖正, 應該要帶回原來的扣押備註
                                    strMemo = getNewMemo(item.GovUnit, item.GovNo);
                                    result = caseAccount.SeizureTelegram(item, strMemo, caseId, "", "", "Cancel", LogonUser);
                                    if (result == "true")
                                    {
                                        rtn = true;
                                        Message = Message + "存款帳號為:" + item.Account + " 的帳戶發查沖正電文成功;<br/>";
                                    }
                                    else
                                    {
                                        Message = Message + "存款帳號為:" + item.Account + " 的帳戶發查沖正電文失敗,原因是 " + result + ";<br/>";
                                    }
                                }
                            }
                        }
                        rtn = caseAccount.SaveCaseSeizureCancel(caseId, listSave);//其他不發查電文的案件直接儲存  IR-1021
                        if (!rtn)
                            Message = Message + "其他帳戶儲存失敗!<br/>";
                    }
                }
                else
                {
                    Message = "抱歉,您的RCAF帳號登入異常,無法使用此功能!";
                }
                return Json(rtn ? new JsonReturn { ReturnCode = "1", ReturnMsg = Message }
                                : new JsonReturn { ReturnCode = "0", ReturnMsg = Message });
            }
        }
        #endregion

        #region 外來文

        /// <summary>
        /// 初始化外來文-帳務資訊
        /// </summary>
        /// <param name="caseId"></param>
        /// <returns></returns>
        public ActionResult _AccountForExternal(Guid caseId)
        {
            IList<CaseAccountExternal> ilst = caseAccount.GetDataFromCAEnal(caseId);
            ViewBag.CaseId = caseId;
            CaseMemo memo = new CaseMemoBiz().Memo(caseId, CaseMemoType.CaseExternalMemo);
            if (memo != null)
                ViewBag.Memo = memo.Memo;
            if (ilst.Count > 0)
            {
                return View(ilst);
            }
            else
            {
                IList<CaseAccountExternal> list = caseAccount.GetDataFromCAESetting();
                foreach (var obj in list)
                {
                    obj.CaseId = caseId;
                }
                return View(list);
            }
        }
        /// <summary>
        /// 儲存外來文-帳務資訊
        /// </summary>
        /// <param name="listResult"></param>
        /// <returns></returns>
        public ActionResult DoSaveAccountForExternal(List<CaseAccountExternal> listResult, string memo)
        {
            return Json(caseAccount.CreateCase(listResult, memo) > 0 ? new JsonReturn() { ReturnCode = "1", ReturnMsg = "" }
                                                                    : new JsonReturn() { ReturnCode = "0", ReturnMsg = Lang.csfs_add_fail });
        }

        public ActionResult ReportExternal(string prCaseId)
        {
            CaseMasterBIZ masterBiz = new CaseMasterBIZ();
            List<string> aryCaseIdList = prCaseId.Split(',').ToList();
            List<ReportParameter> listParm = new List<ReportParameter>();

            listParm.Add(new ReportParameter("CaseId", prCaseId));
            CaseAccountBiz caz = new CaseAccountBiz();
            listParm.Add(new ReportParameter("CaseNo", caz.CaseNo(prCaseId)));
            //* master
            DataTable dtMaster = masterBiz.GetCaseMasterByCaseIdList(aryCaseIdList);

            DataTable dtExternal = new CaseAccountBiz().GetCaseAccountExternalByCaseIdList(aryCaseIdList);
            //* 發票
            DataTable dtReceipt = new CaseAccountBiz().GetCaseReceiptByCaseIdList(aryCaseIdList);


            LocalReport localReport = new LocalReport { ReportPath = Server.MapPath("~/Reports/Rdlc/AccountForExternal.rdlc") };
            localReport.SetParameters(listParm); //*添加參數
            localReport.DataSources.Add(new ReportDataSource("DataSet1", dtMaster));   //* 添加數據源,可以多個
            localReport.DataSources.Add(new ReportDataSource("CaseReceipt", dtReceipt));
            localReport.DataSources.Add(new ReportDataSource("CaseAccountExternal", dtExternal));
            localReport.SubreportProcessing += SubreportProcessingEventHandler;

            subDataSource.Add(new ReportDataSource("CaseAccountExternal", dtExternal));
            subDataSource.Add(new ReportDataSource("CaseReceipt", dtReceipt));

            Warning[] warnings;
            string[] streams;
            string mimeType;
            string encoding;
            string fileNameExtension;

            var renderedBytes = localReport.Render("PDF",
                null,
                out  mimeType,
                out encoding,
                out fileNameExtension,
                out streams,
                out warnings);
            localReport.Dispose();

            Response.ClearContent();
            Response.ClearHeaders();
            return File(renderedBytes, mimeType, "Report.pdf");
        }

        void SubreportProcessingEventHandler(object sender, SubreportProcessingEventArgs e)
        {
            foreach (var reportDataSource in subDataSource)
            {
                e.DataSources.Add(reportDataSource);
            }
        }
        #endregion
    }
}