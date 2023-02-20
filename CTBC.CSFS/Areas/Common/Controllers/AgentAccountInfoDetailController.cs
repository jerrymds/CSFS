using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using CTBC.CSFS.BussinessLogic;
using CTBC.CSFS.Models;
using CTBC.CSFS.Pattern;
using CTBC.CSFS.Resource;
using CTBC.CSFS.ViewModels;

namespace CTBC.CSFS.Areas.Common.Controllers
{
    public class AgentAccountInfoDetailController : AppController
    {
        CasePayeeSettingBIZ cpsBIZ = new CasePayeeSettingBIZ();
        CaseMasterBIZ caseMaster = new CaseMasterBIZ();
        CaseAccountBiz caseAccount = new CaseAccountBiz();
        CustomerInfoBIZ CIBZ = new CustomerInfoBIZ();
        PARMCodeBIZ parm = new PARMCodeBIZ();
        // GET: Agent/AgentAccountInfoDetail
        public ActionResult Index(Guid caseId, string FromControl)
        {
            ViewBag.CaseId = caseId;
            ViewBag.FromControl = FromControl;
            var master = caseMaster.MasterModel(caseId);
            ViewBag.CaseKind = master.CaseKind;
            ViewBag.CaseKind2 = master.CaseKind2;
            ViewBag.GovNo = master.GovNo;
			ViewBag.AfterSeizureApproved = master.AfterSeizureApproved;
            //Add by zhangwei 20180315 start
            ViewBag.CaseNo = master.CaseNo;
            ViewBag.HidApproveStatus = master.Status;
            ViewBag.ReturnReasonList = new SelectList(parm.GetCodeData("DIRECTOR_RETURNREASON"), "CodeDesc", "CodeDesc");
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
            //Add by zhangwei 20180315 end
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
                else if (master.Status == "D01")
                {
                    ViewBag.Status = "1";
                }
                else
                {
                    ViewBag.Status = "0";
                }
            }
            CaseEdocFile caseEdocFile = caseAccount.OpenTxtDoc(caseId);
			ViewBag.HasFile = caseEdocFile == null ? "0" : "1";
            return View();
        }

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
        public ActionResult OpenPdfDoc(string caseId)
      {
            //CaseEdocFile caseEdocFile = caseAccount.OpenPdfDoc(caseId);
            //string text = string.Empty;
            //if (caseEdocFile != null)
            //{
            //   byte[] file = caseEdocFile.FileObject;
            //   return File(file, "application/pdf", caseEdocFile.FileName);
            //}
            //else
            //{
            //   return Content("<script>alert('" + Lang.csfs_no_data + "');</script>");
            //}
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
        //電文記錄
        public ActionResult OpenEmsg(Guid caseId)
        {
            List<BatchQueue> CaseQueryList = caseAccount.StatusList(caseId);
            if (CaseQueryList == null || !CaseQueryList.Any())
                CaseQueryList = new List<BatchQueue>();
            return PartialView("Emessage", CaseQueryList);
        }

        #region 扣押設定
        /// <summary>
        /// 扣押設定 顯示
        /// </summary>
        /// <param name="caseId"></param>
        /// <returns></returns>
        public ActionResult _SeizureSetting(Guid caseId)
      {
            CaseSeizureViewModel caseview = new CaseSeizureViewModel();
            caseview.CaseMaster = caseMaster.MasterModel(caseId);
            List<CaseObligor> list = new CaseObligorBIZ().ObligorModel(caseId);
            if (list != null && list.Any())
            {
                foreach (var item in list)
                {
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

            //adam 0063 資料重覆懷疑是重號
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

                        // 20190110, Patrick 把判斷有沒有扣押金額, 若有任何一個, 即是"Y"
                        string strOtherSeizure = "N";

                        //若TX450-30有出現代碼04則顯示Y，無則顯示N;若TX450-31有出現凍結碼66則顯示Y，無則顯示N
                        CTBC.CSFS.Models.TX_00450 CMOther = CIBZ.GetLatestTx00450(item.Account, caseId, "31", " 9093 ", " 66");
                        if (CMOther != null && CMOther.DATA1.Contains(" 9093 ") && CMOther.DATA2.Trim().Contains(" 66"))
                        {
                            strOtherSeizure = "Y";
                        }

                        //(查TX_33401的HoldAmt > 0 則為Y) IR-1006                        
                        CTBC.CSFS.Models.TX_33401 tx33401 = CIBZ.GetLatestTx33401(item.Account, caseId, item.Currency);
                        if (tx33401 != null && !string.IsNullOrEmpty(tx33401.HoldAmt) && Convert.ToDecimal(tx33401.HoldAmt) > 0)
                        {
                            strOtherSeizure = "Y";
                        }

                        CTBC.CSFS.Models.TX_00450 CM = CIBZ.GetLatestTx00450(item.Account, caseId, "30", " 9091 ", " 04");
                        if (CM != null && CM.DATA1.Contains(" 9091 ") && CM.DATA2.Trim().Contains(" 04"))
                        {
                            strOtherSeizure = "Y";
                        }



                        item.OtherSeizure = strOtherSeizure;

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
                PARMCodeBIZ pbiz = new PARMCodeBIZ();
                var seizureSequenceAll = pbiz.GetParmCodeByCodeType("SeizureSeqence");
                Dictionary<string, List<string>> SeizureOrder = getSeizureOrder(seizureSequenceAll);
                var pids = (from p in listSeizure group p by p.CustId into g select g.Key).ToList();
                int logcount = 0;
                foreach (string id in pids)
                {
                    foreach (var s in SeizureOrder)
                    {
                        //if (s.Key == "定存")
                        //    continue; adam
                        if (s.Key == "綜定" || s.Key == "定存")
                        {
                            //adam
                            //var accTemp = listSeizure.Where(x => x.CustId.Length > 8 && x.CustId == id && s.Value.Contains(x.TxtProdCode)).ToList();
                            var accTemp = listSeizure.Where(x =>  x.CustId == id && s.Value.Contains(x.TxtProdCode)).ToList();
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
                foreach (var item in listSeizure)
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
            #region Old
            //CaseMaster master = caseMaster.MasterModel(caseId);
            //if (master == null)
            //    return Content(Lang.csfs_not_caseseizure);
            //if (master.CaseKind != CaseKind.CASE_SEIZURE)
            //    return Content(Lang.csfs_not_caseseizure);

            //ViewBag.CaseId = caseId;
            //ViewBag.CaseNo = master.CaseNo;

            //IList<CaseSeizure> listSeizure = caseAccount.GetCaseSeizure(caseId);
            //if (listSeizure == null || !listSeizure.Any())
            //{
            //    //* 查主機
            //    listSeizure = caseAccount.GetCaseSeizureFromTx(caseId);
            //}
            //if (listSeizure == null || !listSeizure.Any())
            //    listSeizure = new List<CaseSeizure>();

            //CaseMemo memo = new CaseMemoBiz().Memo(caseId, CaseMemoType.CaseSeizureMemo);
            //if (memo != null)
            //    ViewBag.Memo = memo.Memo;
            //ViewBag.CheckCaseSeizure = new SelectList(caseAccount.CheckCaseSeizure(caseId), "ObligorNo", "ObligorName");
            //return View(listSeizure);
            #endregion
        }
        // adam 20180728
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

        // adam end

        ///// <summary>
        ///// 扣押設定 儲存
        ///// </summary>
        ///// <param name="listForm"></param>
        ///// <param name="memo"></param>
        ///// <returns></returns>
        //public ActionResult DoSaveSeizureSetting(List<CaseSeizure> listForm, string memo)
        //{
        //    string userId = LogonUser.Account;
        //    if (listForm != null && listForm.Any())
        //    {
        //        foreach (CaseSeizure item in listForm)
        //        {
        //            item.CreatedUser = userId;
        //            item.ModifiedUser = userId;
        //        }
        //    }

        //    return Json(caseAccount.SaveSeizureSetting(listForm,memo) ? new JsonReturn { ReturnCode = "1", ReturnMsg = "" }
        //                                                : new JsonReturn { ReturnCode = "0", ReturnMsg = Lang.csfs_save_fail });
        //}

        public ActionResult CustomerInfo(string custId,Guid caseId)
        {
            return View(CIBZ.GetData(custId,caseId));
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
            PaySettingViewModel model = new PaySettingViewModel();
            CaseMaster master = caseMaster.MasterModel(caseId);
            if (master == null)
                return Content(Lang.csfs_not_caseseizure);
            if (master.CaseKind != CaseKind.CASE_SEIZURE)
                return Content(Lang.csfs_not_caseseizure);
            model.CaseKind = master.CaseKind;
            model.CaseKind2 = master.CaseKind2;
            model.CaseId = caseId;

            IList<CaseSeizure> listPay = caseAccount.GetCaseSeizureWithEPaySetting(caseId);
            if (listPay != null && listPay.Any())
            {
                //* 已儲存的
                model.AlreadySaved = true;
                model.ListPay = listPay;
                model.ListSeizure = new List<CaseSeizure>();
            }
            else if (master.CaseKind2 == CaseKind2.CaseSeizureAndPay)
            {
                //* 未儲存,但只能看到本案件未設定
                model.AlreadySaved = false;
                model.ListPay = caseAccount.GetCaseSeizureWithoutPaySetting(caseId);
                model.ListSeizure = new List<CaseSeizure>();
            }
            else
            {
                //* 未储存,可以看到所有未設定客戶Id
                model.AlreadySaved = false;
                model.ListPay = new List<CaseSeizure>();
                model.ListSeizure = caseAccount.GetCaseSeizureWithoutPaySettingByCustomerList(caseId);
            }
            return View(model);
        }

        //public ActionResult DoSavePaySetting(PaySettingViewModel model)
        //{
        //    string userId = LogonUser.Account;
        //    List<CaseSeizure> listSave = new List<CaseSeizure>();
        //    if (model != null && model.ListPay != null && model.ListPay.Any())
        //    {
        //        listSave = model.ListPay.ToList();
        //        foreach (CaseSeizure item in listSave)
        //        {
        //            //* 補上差的欄位
        //            item.PayCaseId = model.CaseId;
        //            item.ModifiedUser = userId;
        //        }
        //    }

        //    return Json(caseAccount.SavePaySetting(listSave) ? new JsonReturn { ReturnCode = "1", ReturnMsg = "" }
        //                                                : new JsonReturn { ReturnCode = "0", ReturnMsg = Lang.csfs_save_fail });
        //}

        #endregion

        #region 受款人設定
        /// <summary>
        /// 扣押設定 顯示
        /// </summary>
        /// <param name="caseId"></param>
        /// <returns></returns>
        public ActionResult _PayeeSetting(CasePayeeSetting cps)
        {
            Bind();
            ViewBag.CaseId = cps.CaseId;
            ViewBag.PayAmount = string.Format("{0:N0}", cpsBIZ.PayAmountSum(cps.CaseId));
            ViewBag.MoneySum = string.Format("{0:N0}", cpsBIZ.MoneySum(cps.CaseId));
            return View(SearchList(cps, cps.CaseId));
        }

        public void Bind()
        {
            ViewBag.CurrencyList = new SelectList(caseMaster.GetCodeData("GOV_KIND"), "CodeDesc", "CodeDesc");
            ViewBag.AddressList = new SelectList(caseMaster.GetCodeData("GOV_COURT"), "CodeDesc", "CodeDesc");
            ViewBag.CCReceiverList = new SelectList(caseMaster.GetCodeData("GOV_COURT"), "CodeDesc", "CodeDesc");
        }
        public CasePayeeSettingViewModel SearchList(CasePayeeSetting cps, Guid caseId)
        {
            CasePayeeSettingViewModel viewModel;
            cps.LanguageType = Session["CultureName"].ToString();
            IList<CasePayeeSetting> result = cpsBIZ.GetQueryList(cps);

            viewModel = new CasePayeeSettingViewModel()
            {
                CasePayeeSetting = cps,
                CasePayeeSettingList = result,
                CaseKind = cpsBIZ.CaseKind(caseId),
                BankID=cpsBIZ.BankID(caseId),
            };
   

            return viewModel;
        }

        //修改
        public ActionResult _PayeeEdit(int PayeeId)
        {
            Bind();
            CasePayeeSetting model = cpsBIZ.Select(PayeeId);
            ViewBag.EditMode = "_PayeeEdit";
            return View("_PayeeEdit", model);
        }

        [HttpPost]
        public ActionResult _PayeeEdit(CasePayeeSetting model)
        {
            string userId = LogonUser.Account;
            return Json(cpsBIZ.Edit(model,userId));
        }
        
        //新增
        public ActionResult CreateCPS(Guid CaseId)
        {
            return View(new CasePayeeSetting());
        }

        //[HttpPost]
        //public ActionResult CreateCPS(CasePayeeSetting model)
        //{
        //    model.PayDate = DateTime.Now.AddDays(Convert.ToInt32(1 - Convert.ToInt32(DateTime.Now.DayOfWeek)) + 9);
        //    return Json(cpsBIZ.Create(model) > 0 ? new JsonReturn() { ReturnCode = "1", ReturnMsg = "" }
        //                                                    : new JsonReturn() { ReturnCode = "0", ReturnMsg = Lang.csfs_add_fail });
        //}
        //刪除
        //public ActionResult _PayeeDelete(int PayeeId)
        //{
        //    return Content(cpsBIZ.Delete(PayeeId) > 0 ? "true" : "false");
        //}
        public ActionResult _PayeeSelectAddress(string GovName)
        {
            return Content(cpsBIZ.Address(GovName));
        }

        #region 取票號
        //public ActionResult GetTicket(CasePayeeSetting model)
        //{
        //    model.CheckNo = cpsBIZ.StartNo();
        //    if (string.IsNullOrEmpty(model.CheckNo)) { return Json(new JsonReturn() { ReturnCode = "0", ReturnMsg = Lang.csfa_NoMax }); }
        //    return Json(cpsBIZ.GetTicket(model) > 0 ? new JsonReturn() { ReturnCode = "1", ReturnMsg = model.CheckNo }
        //                                                : new JsonReturn() { ReturnCode = "0", ReturnMsg = Lang.csfs_fail });
            
        //}
        #endregion
        #region 取消票號
        //public ActionResult CancelTicket(string checkno)
        //{
        //    if (string.IsNullOrEmpty(checkno)) { return Json(new JsonReturn() { ReturnCode = "0", ReturnMsg = Lang.csfs_plz_getticket }); }
        //    return Json(cpsBIZ.CancelTicket(checkno) > 0 ? new JsonReturn() { ReturnCode = "1", ReturnMsg = "" }
        //                                                : new JsonReturn() { ReturnCode = "0", ReturnMsg = Lang.csfs_fail });
        //}
        #endregion
        #endregion

        #region 撤銷設定
        /// <summary>
        /// 扣押設定 顯示
        /// </summary>
        /// <param name="caseId"></param>
        /// <returns></returns>
        public ActionResult _SeizureCancel(Guid caseId)
        {
            string CustId = "";
            CancelSeizureViewModel model = new CancelSeizureViewModel
            {
                CaseId = caseId,
                ListSaved = caseAccount.GetCaseSeizureWithCancel(caseId)                
            };
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
                    CustId = list[0].CustId.Trim();
                    if (CustId.Length > 0)
                    {
                        _business.ExecuteAPLOGSave(_user, _controller, _action, _ip, "CASEID=" + caseId.ToString(), CustId);
                    }
                //}
            }
            // 新增結束
            return View(model);
        }

        public ActionResult _SeizureCancelTable(CancelSeizureViewModel model)
        {
            return View(caseAccount.GetCaseSeizureByQuery(model));
        }

        //public ActionResult DoSaveSeizureCancel(Guid caseId,  string seizureList)
        //{
        //    if (string.IsNullOrEmpty(seizureList))
        //        return Json(new JsonReturn {ReturnCode = "0", ReturnMsg = Lang.csfs_edit_fail});

        //    List<string> ary = seizureList.Split(',').ToList();

        //    return Json(caseAccount.SaveCaseSeizureCancel(caseId, ary, LogonUser.Account) 
        //                                                ? new JsonReturn { ReturnCode = "1", ReturnMsg = Lang.csfs_save_ok }
        //                                                : new JsonReturn { ReturnCode = "0", ReturnMsg = Lang.csfs_save_fail });
        //}
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
        public ActionResult DoSaveAccountForExternal(List<CaseAccountExternal> listResult,string memo)
        {
            return Json(caseAccount.CreateCase(listResult,memo) > 0 ? new JsonReturn() { ReturnCode = "1", ReturnMsg = "" }
                                                                    : new JsonReturn() { ReturnCode = "0", ReturnMsg = Lang.csfs_add_fail });
        }
        #endregion
    }
}