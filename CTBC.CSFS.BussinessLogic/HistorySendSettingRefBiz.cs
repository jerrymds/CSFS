using System;
using System.Collections.Generic;
using System.Linq;
using CTBC.CSFS.Models;
using CTBC.CSFS.Pattern;
using System.Data;
using CTBC.CSFS.Resource;
using CTBC.FrameWork.Util;

namespace CTBC.CSFS.BussinessLogic
{
    public class HistorySendSettingRefBiz : CommonBIZ
    {
        public HistorySendSettingRef GetSendSettingRef(string caseKind, string casekind2, IDbTransaction trans = null)
        {
            string sql = @"SELECT [CaseKind],[CaseKind2],[Subject],[Description] 
                            FROM [History_SendSettingRef] WHERE [CaseKind] = @CaseKind AND [CaseKind2] = @CaseKind2";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("CaseKind", caseKind));
            Parameter.Add(new CommandParameter("CaseKind2", casekind2));
            IList<HistorySendSettingRef> list = trans == null ? SearchList<HistorySendSettingRef>(sql) : SearchList<HistorySendSettingRef>(sql, trans);
            return list.FirstOrDefault();
        }


        public HistorySendSettingRef GetSubjectAndDescription(Guid caseId, string template, string sendKind, IDbTransaction trans = null, HistoryCasePayeeSetting payeeModel = null)
        {
            string caseKind1 = "";
            string caseKind2 = "";
            HistoryCaseMaster master = new HistoryCaseMasterBIZ().MasterModel(caseId, trans);
            string sendDate = master.PayDate;
            if (master.CaseKind == "外來文案件")
            {
                caseKind1 = @"外來文案件";
                caseKind2 = master.CaseKind2 == "165調閱" ? @"165調閱" : master.CaseKind2 == Lang.csfs_property_declaration1 ? Lang.csfs_property_declaration1 : @"非165調閱及財產申報";
            }
            if (master.CaseKind == "扣押案件")
            {
                caseKind1 = @"法扣案件";
                switch (master.CaseKind2)
                {
                    case "扣押":
                        if (sendKind == @"電子發文")
                        {
                            caseKind2 = @"扣押電子回文";
                        }
                        else
                        {
                            caseKind2 = @"扣押";
                        }
                        break;
                    case "支付":
                        caseKind2 = @"支付";
                        break;
                    case "扣押並支付":
                        caseKind2 = template;
                        break;
                }
            }
            HistorySendSettingRefBiz refBiz = new HistorySendSettingRefBiz();
            HistorySendSettingRef refObj = refBiz.GetSendSettingRef(caseKind1, caseKind2, trans);
            if (refObj == null)
                return new HistorySendSettingRef();

            string strSubject = refObj.Subject;
            string strDesc = refObj.Description;
            //* 公文
            strSubject = strSubject.Replace("{GovNo}", master.GovNo);
            int iyy = Convert.ToInt16(master.GovDate.Substring(0, 4)) - 1911;
            string strmm = master.GovDate.Substring(5, 2);
            string strdd = master.GovDate.Substring(8, 2);
            strSubject = strSubject.Replace("{GovDate}", iyy.ToString() + strmm + strdd);
            strDesc = strDesc.Replace("{GovNo}", master.GovNo);
            strSubject = strSubject.Replace("{CaseNo}", master.CaseNo);
            strDesc = strDesc.Replace("{CaseNo}", master.CaseNo);
            //strDesc = strDesc.Replace("{GovDate}", master.GovDate);
            DateTime dt = Convert.ToDateTime(master.GovDate);
            strDesc = strDesc.Replace("{GovDate}", dt.AddYears(-1911).Year + "年" + dt.Month + "月" + dt.Day + "日");

            //* 類別
            strDesc = strDesc.Replace("{CaseKind2}", master.CaseKind2);
            //* 承辦人
            strDesc = strDesc.Replace("{AgentUser}", master.AgentUser);
            //* 收文編號
            strDesc = strDesc.Replace("{DocNo}", master.DocNo);

            CaseAccountBiz accountBiz = new CaseAccountBiz();

            if (caseKind2 == "扣押" || caseKind2 == "扣押電子回文")
            {
                string strOligorList = string.Empty;
                List<CaseSeizure> list = accountBiz.GetCaseSeizure(caseId, trans).ToList();
                //*來函扣押總金額
                decimal TotalSeiAmt = master.ReceiveAmount;
                //*金額未達毋需扣押
                decimal NotSei = master.NotSeizureAmount;
                //*SeizureAmountNtd 欄位的加總
                var allSei = list.Where(x => x.SeizureAmountNtd > 0).ToList();
                decimal allseiamt = allSei.Sum(x => x.SeizureAmountNtd); // 已扣押的金額
                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i].CustId.ToString().Trim().Length > 10)
                    {
                        list[i].CustId = list[i].CustId.ToString().Trim().Substring(0, 10).Trim();
                    }
                }
                var listForCaseSeizure2 = (from lists in list
                                           group lists by new { lists.CustId, lists.BranchNo, lists.BranchName, lists.Currency } into list2
                                           select new { name = list2.Key, SeizureAmount = list2.Sum(m => m.SeizureAmount) }).ToList();

                CaseObligorBIZ co = new CaseObligorBIZ();
                List<CaseObligor> obligorList = co.ObligorModel(caseId, trans);
                //*扣押義務人,統編,存款分行,扣押金額備註
                //if (listForCaseSeizure2 != null && listForCaseSeizure2.Any())
                //{
                if (obligorList != null && obligorList.Any())
                {
                    string strmemo = string.Empty;//*備註
                    string newMemo = string.Empty; // 在發文中, 若有加入新的備註, 也要寫回CaseMemo
                    string ZueMemo = string.Empty;//足額備註
                    string NoZueMemo = string.Empty;//不足額備註
                    Guid memoCaseid = caseId;
                    foreach (var item in obligorList)
                    {
                        string strObligorNo = item.ObligorNo;
                        if (item.ObligorNo.Length > 10)
                        {
                            strObligorNo = item.ObligorNo.Trim().Substring(0, 10).Trim();
                        }

                        //* 回圈每個義務人統編
                        if (!(listForCaseSeizure2.Select(g => g.name.CustId).Contains(strObligorNo)))//!listForCaseSeizure2.Any() IR-0129
                        {
                            strOligorList = strOligorList + "義(債)務人戶名：" + item.ObligorName + " \r\n" +
                                                            "義(債)務人統編：" + item.ObligorNo + " \r\n" +
                                                            "說明：於本行無存款往來\r\n\r\n";
                        }
                        else
                        {
                            strOligorList = strOligorList + "義(債)務人戶名：" + item.ObligorName + "\r\n" +
                                                            "義(債)務人統編：" + item.ObligorNo + "\r\n";
                            //adam
                            int iMaxBranchLenght = listForCaseSeizure2.Where(m => m.name.CustId == strObligorNo).Select(itemList => itemList.name.BranchName.Length).Concat(new[] { 0 }).Max() + 1;
                            if (iMaxBranchLenght < 5)
                                iMaxBranchLenght = 5;

                            Dictionary<string, string> CurrTrans = new Dictionary<string, string>() { { "AUD", "澳幣" }, { "CAD", "加拿大幣" }, { "CHF", "瑞士法郎" }, { "CNY", "人民幣" }, { "EUR", "歐元" }, { "GBP", "英鎊" }, { "HKD", "港幣" }, { "IDR", "印尼盾" }, { "INR", "印度盧比" }, { "JPY", "日圓" }, { "KRW", "韓圜" }, { "MYR", "馬來西亞幣" }, { "NZD", "紐西蘭幣" }, { "PHP", "菲律賓披索" }, { "SEK", "瑞典幣" }, { "SGD", "新加坡幣" }, { "THB", "泰銖" }, { "USD", "美元" }, { "VND", "越南盾" }, { "ZAR", "南非幣" } };
                            CaseMemo memo1 = new CaseMemo();
                            List<CaseMemo> allCM2 = new CaseMemoBiz().MemoList2(caseId, CaseMemoType.CaseSeizureMemo, trans);
                            if (allCM2 != null && allCM2.Count() > 0)
                            {
                                memo1 = allCM2.First();
                                string allmemo333 = string.Join("\r\n", allCM2.Select(x => x.Memo).ToList());
                                memo1.Memo = allmemo333;
                            }
                            else
                            {
                                memo1.Memo = "";
                            }
                            //if (allseiamt < NotSei && list.Count() > 0)
                            //{
                            //    if (!memo1.Memo.Contains("戶名不符"))
                            //    {
                            //        string _branchNo = list.First().BranchName;
                            //        strOligorList = strOligorList + "存款分行：" + _branchNo.PadRight(iMaxBranchLenght, '　').Substring(0, iMaxBranchLenght);
                            //        strOligorList = strOligorList + "扣押金額：" + "TWD" + " " + "0" + "\r\n";
                            //    }
                            //    if (memo1.Memo.Contains("帳戶開立之戶名與來函戶名不符"))
                            //    {
                            //        string _branchNo = list.First().BranchName;
                            //        strOligorList = strOligorList + "存款分行：" + _branchNo.PadRight(iMaxBranchLenght, '　').Substring(0, iMaxBranchLenght);
                            //        strOligorList = strOligorList + "扣押金額：" + "TWD" + " " + "0" + "\r\n";
                            //    }
                            //}
                            //else
                            {
                                if (TotalSeiAmt > allseiamt) // 不足額
                                {
                                    foreach (var itemList in listForCaseSeizure2.Where(m => m.name.CustId == strObligorNo))
                                    {
                                        strOligorList = strOligorList + "存款分行：" + itemList.name.BranchName.PadRight(iMaxBranchLenght, '　').Substring(0, iMaxBranchLenght);
                                        if (itemList.name.Currency == "TWD")
                                        {
                                            if (itemList.SeizureAmount != 0)
                                            {
                                                strOligorList = strOligorList + "扣押金額：" + itemList.name.Currency + " " + itemList.SeizureAmount.ToString("###,###") + "\r\n";
                                            }
                                            else
                                            {
                                                strOligorList = strOligorList + "扣押金額：" + itemList.name.Currency + " 0 \r\n";
                                            }
                                        }
                                        else
                                        {
                                            strOligorList = strOligorList + "扣押金額：" + itemList.name.Currency + " " + string.Format("{0:N}", itemList.SeizureAmount) + "\r\n";
                                        }
                                        //* 外幣備註要多一筆
                                        if (itemList.name.Currency != "TWD" && itemList.SeizureAmount > 0 && strmemo != "1")//*寫入備註的話只執行一次
                                        {
                                            //strDesc = strDesc.Replace("{Memo}", "其扣押款為外幣存款，收取時將依當時匯率兌換新台幣。\r\n {Memo}");
                                            NoZueMemo = "其扣押款為外幣存款，收取時將依當時匯率兌換新台幣。\r\n";
                                            strmemo = "1";
                                            newMemo = "其扣押款為外幣存款，收取時將依當時匯率兌換新台幣。\r\n";
                                        }
                                    }
                                }
                                else //足額
                                {
                                    //先以CustId group by 并找出台幣金額大於0的資料
                                    var CustIdSei = list.Where(x => x.CustId == strObligorNo).ToList();
                                    //再以分行別group, 加總每個分行要扣多少NTD
                                    var branchGroup = (from p in CustIdSei
                                                       group p by p.BranchName into g
                                                       select new { BranchName = g.Key, BranchTotal = g.Sum(x => x.SeizureAmountNtd) }).ToList();

                                    foreach (var b in branchGroup)
                                    {
                                        string _branchNo = b.BranchName;
                                        strOligorList = strOligorList + "存款分行：" + _branchNo.PadRight(iMaxBranchLenght, '　').Substring(0, iMaxBranchLenght);
                                        if (b.BranchTotal > 0)
                                        {
                                            strOligorList = strOligorList + "扣押金額：" + "TWD" + " " + b.BranchTotal.ToString("###,###") + "\r\n";
                                        }
                                        else
                                        {
                                            strOligorList = strOligorList + "扣押金額：" + "TWD" + " 0" + "\r\n";
                                        }

                                        // 足額, 用 ... 其中永吉分行部分扣押款新臺幣150元為外幣存款(約當5美元)，收取時將依當時匯率兌換新臺幣。

                                        string _ntd = b.BranchTotal.ToString("###,###,###,###");
                                        string currStr = "";
                                        var groupCurrency = from p in CustIdSei.Where(x => x.BranchName == b.BranchName)
                                                            group p by p.Currency into g
                                                            select new { Ccy = g.Key, Total = g.Sum(x => x.SeizureAmount), CcyTotal = g.Sum(x => x.SeizureAmountNtd) };

                                        decimal _ntdTotal = 0.0m;
                                        foreach (var s in groupCurrency)
                                        {
                                            string chineseCcy = "";
                                            string strMoney = "";
                                            if (CurrTrans.ContainsKey(s.Ccy))
                                            {
                                                chineseCcy = CurrTrans[s.Ccy].ToString();
                                                strMoney = ((decimal)s.Total).ToString("###,###,##0.##");
                                                _ntdTotal += s.CcyTotal;
                                                currStr += strMoney + chineseCcy + "、";
                                            }
                                        }
                                        if (currStr.EndsWith("、"))
                                            currStr = currStr.Substring(0, currStr.Length - 1);

                                        if (_ntdTotal > 0)
                                        {
                                            string message = "其中" + _branchNo + "部分扣押款新臺幣" + _ntdTotal.ToString("###,###,###") + "元為外幣存款(約當" + currStr + ")，收取時將依當時匯率兌換新臺幣。\r\n{Memo}";
                                            //strDesc = strDesc.Replace("{Memo}", message);
                                            ZueMemo = ZueMemo + message.Replace("{Memo}", "");
                                            newMemo = newMemo + message;
                                        }
                                    }
                                }
                                strOligorList = strOligorList + "\r\n";
                            }
                        }
                    }
                    //}
                    strDesc = strDesc.Replace("{ObligorList}", strOligorList);
                    //CaseMemo memo = new CaseMemoBiz().Memo(caseId, CaseMemoType.CaseSeizureMemo, trans);
                    List<CaseMemo> allCM = new CaseMemoBiz().MemoList2(caseId, CaseMemoType.CaseSeizureMemo, trans);
                    string memo = null;
                    string flag = "";//是否需要寫CaseMemo，以免重複

                    if (allCM != null && allCM.Count() > 0)
                    {
                        flag = string.Join("\r\n", allCM.Select(x => x.Memo));
                        memo = string.Join("\r\n", allCM.Select(x => x.Memo.TrimEnd('\n').TrimEnd('\r'))) + "\r\n";
                        //if (allCM.Where(x => !x.Memo.Contains("收取時將依當時匯率兌換新臺幣。") && !x.Memo.Contains("收取時將依當時匯率兌換新台幣。")).Count() > 0)
                        //{
                        //    memo = string.Join("\r\n", allCM.Where(x => !x.Memo.Contains("收取時將依當時匯率兌換新臺幣。") && !x.Memo.Contains("收取時將依當時匯率兌換新台幣。")).Select(x => x.Memo));
                        //}
                    }
                    if (!string.IsNullOrEmpty(memo))
                    {
                        string nullstr = memo.Replace("\r\n", "");
                        if (nullstr == "")
                            memo = "";
                        if (!memo.Contains("其扣押款為外幣存款，收取時將依當時匯率兌換新台幣。"))
                        {
                            memo = memo + NoZueMemo;
                        }
                        if (!memo.Contains("元為外幣存款(約當") && !memo.Contains(")，收取時將依當時匯率兌換新臺幣。"))
                        {
                            memo = memo + ZueMemo;
                        }
                    }
                    //string backupstr = memo.Memo.Replace("\r\n","");
                    //if (backupstr == "")
                    //    memo.Memo = "";

                    //if (memo != null && !string.IsNullOrEmpty(memo.Memo))
                    //{
                    //    strDesc = strDesc.Replace("{Memo}", memo.Memo);
                    //}
                    if (memo != null && !string.IsNullOrEmpty(memo))
                    {
                        strDesc = strDesc.Replace("{Memo}", memo);
                    }
                    else
                    {
                        //if (strDesc.Contains("其扣押款為外幣存款，收取時將依當時匯率兌換新台幣。"))
                        if (strDesc.Contains("收取時將依當時匯率兌換新臺幣。") || strDesc.Contains("收取時將依當時匯率兌換新台幣。"))
                        {
                            strDesc = strDesc.Replace("{Memo}", "");
                        }
                        else
                        {
                            strDesc = strDesc.Replace("{Memo}", "無。");
                        }
                    }
                    if (!string.IsNullOrEmpty(newMemo) && ((newMemo.Contains("收取時將依當時匯率兌換新臺幣。") && (string.IsNullOrEmpty(flag) || !flag.Contains("收取時將依當時匯率兌換新臺幣。"))) || (newMemo.Contains("收取時將依當時匯率兌換新台幣。") && (string.IsNullOrEmpty(flag) || !flag.Contains("收取時將依當時匯率兌換新台幣。")))))
                    {
                        newMemo = newMemo.Replace("{Memo}", "");
                        CaseMemo cm = new CaseMemo() { CaseId = memoCaseid, Memo = newMemo, MemoType = CaseMemoType.CaseSeizureMemo, MemoUser = "Z00004771" };
                        CaseMemoBiz cmb = new CaseMemoBiz();
                        cmb.Create2(cm);
                    }
                }
            }
            if (caseKind2 == "支付")
            {
                string strObligorList = "";
                List<CaseObligor> listObligor = new CaseObligorBIZ().ObligorModel(caseId, trans);
                if (listObligor != null && listObligor.Any())
                {
                    foreach (CaseObligor obligor in listObligor)
                    {
                        strObligorList = strObligorList + "義(債)務人戶名：" + obligor.ObligorName + "\r\n";
                        strObligorList = strObligorList + "義(債)務人統編：" + obligor.ObligorNo + "\r\n\r\n";
                    }
                }
                strObligorList = strObligorList.Length > 0 ? strObligorList.Substring(0, strObligorList.Length - 2) : "";
                strDesc = strDesc.Replace("{ObligorList}", strObligorList);

                if (payeeModel == null)
                {
                    payeeModel = new HistoryCasePayeeSettingBIZ().GetQueryList(new HistoryCasePayeeSetting { CaseId = caseId }, trans).FirstOrDefault();
                }
                if (payeeModel != null)
                {
                    string checkno = UtlString.Right("0000000" + payeeModel.CheckNo, 7);
                    strDesc = strDesc.Replace("{BranchName}", payeeModel.Bank);
                    strDesc = strDesc.Replace("{Money}", "TWD " + UtlString.FormatCurrency(payeeModel.Money, 0));
                    strDesc = strDesc.Replace("{Fee}", "TWD " + UtlString.FormatCurrency(payeeModel.Fee, 0));
                    strDesc = strDesc.Replace("{CheckNo}", checkno);
                    strDesc = strDesc.Replace("{ReceivePerson}", payeeModel.ReceivePerson);
                    string strMemo = payeeModel.Memo;
                    if (string.IsNullOrEmpty(strMemo))
                    {
                        strMemo = "無";
                    }
                    strDesc = strDesc.Replace("{Memo}", strMemo);
                }
            }
            if (caseKind1 == "外來文案件")
            {
                CaseObligorBIZ co = new CaseObligorBIZ();
                List<CaseObligor> list = co.ObligorModel(caseId, trans);
                if (list != null && list.Any())
                {
                    strDesc = strDesc.Replace("{ObligorName}", list[0].ObligorName);
                    strDesc = strDesc.Replace("{ObligorNo}", list[0].ObligorNo);

                    string account = list[0].ObligorAccount;
                    if (!string.IsNullOrEmpty(account))
                    {
                        account = "、" + account;
                    }
                    strDesc = strDesc.Replace("{ObligorAccount}", account);
                }
            }

            return new HistorySendSettingRef { Subject = strSubject, Description = strDesc, SendDate = sendDate };
        }

        public HistorySendSettingRef AutoGetSubjectAndDescription(Guid caseId, string template, string sendKind, decimal TotalSeiAmt, IDbTransaction trans = null, CasePayeeSetting payeeModel = null)
        {
            string caseKind1 = "";
            string caseKind2 = "";
            CaseMaster master = new CaseMasterBIZ().MasterModel(caseId, trans);
            string sendDate = master.PayDate;
            if (master.CaseKind == "外來文案件")
            {
                caseKind1 = @"外來文案件";
                caseKind2 = master.CaseKind2 == "165調閱" ? @"165調閱" : master.CaseKind2 == Lang.csfs_property_declaration1 ? Lang.csfs_property_declaration1 : @"非165調閱及財產申報";
            }
            if (master.CaseKind == "扣押案件")
            {
                caseKind1 = @"法扣案件";
                switch (master.CaseKind2)
                {
                    case "扣押":
                        if (sendKind == @"電子發文")
                        {
                            caseKind2 = @"扣押電子回文";
                        }
                        else
                        {
                            caseKind2 = @"扣押";
                        }
                        break;
                    case "支付":
                        caseKind2 = @"支付";
                        break;
                    case "扣押並支付":
                        caseKind2 = template;
                        break;
                }
            }
            HistorySendSettingRefBiz refBiz = new HistorySendSettingRefBiz();
            HistorySendSettingRef refObj = refBiz.GetSendSettingRef(caseKind1, caseKind2, trans);
            if (refObj == null)
                return new HistorySendSettingRef();

            string strSubject = refObj.Subject;
            string strDesc = refObj.Description;
            //* 公文
            strSubject = strSubject.Replace("{GovNo}", master.GovNo);
            int iyy = Convert.ToInt16(master.GovDate.Substring(0, 4)) - 1911;
            string strmm = master.GovDate.Substring(5, 2);
            string strdd = master.GovDate.Substring(8, 2);
            strSubject = strSubject.Replace("{GovDate}", iyy.ToString() + strmm + strdd);
            strDesc = strDesc.Replace("{GovNo}", master.GovNo);
            strSubject = strSubject.Replace("{CaseNo}", master.CaseNo);
            strDesc = strDesc.Replace("{CaseNo}", master.CaseNo);
            //strDesc = strDesc.Replace("{GovDate}", master.GovDate);
            DateTime dt = Convert.ToDateTime(master.GovDate);
            strDesc = strDesc.Replace("{GovDate}", dt.AddYears(-1911).Year + "年" + dt.Month + "月" + dt.Day + "日");

            //* 類別
            strDesc = strDesc.Replace("{CaseKind2}", master.CaseKind2);
            //* 承辦人
            strDesc = strDesc.Replace("{AgentUser}", master.AgentUser);
            //* 收文編號
            strDesc = strDesc.Replace("{DocNo}", master.DocNo);

            CaseAccountBiz accountBiz = new CaseAccountBiz();

            if (caseKind2 == "扣押" || caseKind2 == "扣押電子回文")
            {
                string strOligorList = string.Empty;
                List<CaseSeizure> list = accountBiz.GetCaseSeizure(caseId, trans).ToList();
                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i].CustId.ToString().Trim().Length == 11)
                    {
                        list[i].CustId = list[i].CustId.ToString().Substring(0, 10).Trim();
                    }
                }
                var listForCaseSeizure2 = (from lists in list
                                           group lists by new { lists.CustId, lists.BranchNo, lists.BranchName, lists.Currency } into list2
                                           select new { name = list2.Key, SeizureAmount = list2.Sum(m => m.SeizureAmount) }).ToList();

                decimal allSet = list.Sum(x=>x.SeizureAmountNtd);

                
                bool isEnough = false;
                if( allSet>=TotalSeiAmt)
                    isEnough=true;
                else
                    isEnough = false;

                CaseObligorBIZ co = new CaseObligorBIZ();
                List<CaseObligor> obligorList = co.ObligorModel(caseId, trans);
                //*扣押義務人,統編,存款分行,扣押金額備註
                //if (listForCaseSeizure2 != null && listForCaseSeizure2.Any())
                //{
                if (obligorList != null && obligorList.Any())
                {
                    string strmemo = string.Empty;//*備註
                    string newMemo = string.Empty; // 在發文中, 若有加入新的備註, 也要寫回CaseMemo
                    Guid memoCaseid = new Guid();
                    // 找出行號型態中, 若備註,有註明"負責人不符的, 要寫在說明中, 不可寫在備註中
                    List<CaseMemo> allCMTypeD = new CaseMemoBiz().MemoList2(caseId, CaseMemoType.CaseSeizureMemo, trans);
                    bool isTypeDNRespName = allCMTypeD.Any(x => x.Memo.Contains("!@!經查於本行負責人"));
                    bool isAccNameNOTSame = allCMTypeD.Any(x => x.Memo.Contains("!@!戶名不符"));
                    bool isEnough450All = allCMTypeD.Any(x => x.Memo.Contains("!@!" + "扣除手續費存款餘額未達200元"));

                    foreach (var item in obligorList)
                    {
                        bool isAccNameNOTSameByNo = false;
                        string strObligorNo = item.ObligorNo;
                        if( isAccNameNOTSame)
                        {
                            string a = strObligorNo + "!@!戶名不符";
                            isAccNameNOTSameByNo = allCMTypeD.Any(x => x.Memo.Contains(strObligorNo + "!@!戶名不符"));
                        }

                        bool isEnough450 = false;
                        if( isEnough450All)
                        {                            
                            isEnough450 = allCMTypeD.Any(x => x.Memo.Contains(strObligorNo + "!@!扣除手續費存款餘額未達200元，不予扣押。"));
                            var a = new CaseMemoBiz().Update2(new CaseMemo() { CaseId = caseId, MemoType = CaseMemoType.CaseSeizureMemo }, trans);

                        }
                        

                        //* 回圈每個義務人統編
                        if (!listForCaseSeizure2.Any() || listForCaseSeizure2.All(m => m.name.CustId != strObligorNo))
                        {

                            strOligorList = strOligorList + "義(債)務人戶名：" + item.ObligorName + " \r\n" +
                                "義(債)務人統編：" + item.ObligorNo + " \r\n" +
                                "說明：於本行無存款往來\r\n\r\n";
                            //20181207, 即來, 無存款往來, 就什麼都不講
                            var b = new CaseMemoBiz().Delete3(caseId, CaseMemoType.CaseSeizureMemo, strObligorNo, trans);
                            //再把原來有此行的訊息, 從CASEMEMO中刪除
                            var a = new CaseMemoBiz().Update2(new CaseMemo() { CaseId = caseId, MemoType = CaseMemoType.CaseSeizureMemo }, trans);
                        
						}
                        else
                        {

                            if (isTypeDNRespName && item.ObligorNo.Length < 10) // 行號中, 負責人不符的訊息 
                            {

                                #region 行號CASE, 負責人不符, 跟負責人不同. 是不一樣的


                                List<string> delString = new List<string>();

                                var aaa = allCMTypeD.Where(x => x.Memo.Contains(item.ObligorNo + "!@!" + "經查於本行負責人戶名不符")).FirstOrDefault();
                                

                                if (aaa != null)
                                {

                                    strOligorList = strOligorList + "義(債)務人戶名：" + item.ObligorName + " \r\n" +
                                                                    "義(債)務人統編：" + item.ObligorNo + " \r\n" +
                                                                    "說明：經查於本行負責人戶名不符，故無執行扣押。\r\n\r\n";
                                    var samIds = allCMTypeD.Where(x=>x.Memo.Contains(item.ObligorNo+"!@!") && x.Memo!=aaa.Memo).ToList();

                                    delString.AddRange(samIds.Select(x=>x.Memo));
                                }
                                var bbb = allCMTypeD.Where(x => x.Memo.Contains(item.ObligorNo + "!@!" + "經查於本行負責人不同")).FirstOrDefault();
                                if (bbb != null)
                                {

                                    strOligorList = strOligorList + "義(債)務人戶名：" + item.ObligorName + " \r\n" +
                                                                    "義(債)務人統編：" + item.ObligorNo + " \r\n" +
                                                                    "說明：經查於本行負責人不同，故無執行扣押。\r\n\r\n";
                                    var samIds = allCMTypeD.Where(x => x.Memo.Contains(item.ObligorNo + "!@!") && x.Memo != bbb.Memo).ToList();

                                    delString.AddRange(samIds.Select(x => x.Memo));
                                }


                                //20181221, 若有任何負責人不同/不符, 其他資訊都不說...
                                foreach(var del in delString)
                                {
                                    int iPos = del.IndexOf("!@!");
                                    var ids = del.Substring(0,iPos);
                                    string aaa1 = "其中統編：" + del.Replace("!@!", "，");
                                    var b = new CaseMemoBiz().Delete3(caseId, CaseMemoType.CaseSeizureMemo, aaa1, trans);
                                    
                                }

                                //再把原來有此行的訊息, 從CASEMEMO中刪除
                                var a = new CaseMemoBiz().Update2(new CaseMemo() { CaseId = caseId, MemoType = CaseMemoType.CaseSeizureMemo }, trans);
                                #endregion 


                            }
                            else if (isAccNameNOTSameByNo) // 如果戶名不符的話
                            {
                                #region 行號CASE, 戶名不符
                                // 先抓出是那個義務人戶名不符
                                var temp = allCMTypeD.Where(x => x.Memo.Contains("!@!戶名不符")).FirstOrDefault();
                                if (temp != null)
                                {
                                    int iPos = temp.Memo.IndexOf("!@!戶名不符");
                                    if (iPos > 0)
                                    {
                                        string ids = temp.Memo.Substring(0, iPos);
                                        if (ids == strObligorNo)
                                        {
                                            strOligorList = strOligorList + "義(債)務人戶名：" + item.ObligorName + " \r\n" +
                                                                    "義(債)務人統編：" + item.ObligorNo + " \r\n" +
                                                                    "說明：戶名不符。\r\n\r\n";
                                            var cmb = new CaseMemoBiz();
                                            //var a = cmb.Delete2(new CaseMemo() { CaseId = caseId, MemoType = CaseMemoType.CaseSeizureMemo }, trans);
                                            //var a = cmb.Delete3(caseId, CaseMemoType.CaseSeizureMemo,  item.ObligorNo + "!@!" );
                                            //var b = cmb.Delete3(caseId, CaseMemoType.CaseSeizureMemo, "戶名不符。");
                                            //var c = cmb.Delete3(caseId, CaseMemoType.CaseSeizureMemo, "戶名不符且存款");

                                        }
                                    }
                                }
                                #endregion
                            }
                            else
                            {

                                #region 相關檔頭資訊

                                strOligorList = strOligorList + "義(債)務人戶名：" + item.ObligorName + "\r\n" +
                                                                "義(債)務人統編：" + item.ObligorNo + "\r\n";
                                int iMaxBranchLenght = listForCaseSeizure2.Select(itemList => itemList.name.BranchName.Length).Concat(new[] { 0 }).Max() + 1;
                                if (iMaxBranchLenght < 5)
                                    iMaxBranchLenght = 5;

                                Dictionary<string, string> CurrTrans = new Dictionary<string, string>() { { "AUD", "澳幣" }, { "CAD", "加拿大幣" }, { "CHF", "瑞士法郎" }, { "CNY", "人民幣" }, { "EUR", "歐元" }, { "GBP", "英鎊" }, { "HKD", "港幣" }, { "IDR", "印尼盾" }, { "INR", "印度盧比" }, { "JPY", "日圓" }, { "KRW", "韓圜" }, { "MYR", "馬來西亞幣" }, { "NZD", "紐西蘭幣" }, { "PHP", "菲律賓披索" }, { "SEK", "瑞典幣" }, { "SGD", "新加坡幣" }, { "THB", "泰銖" }, { "USD", "美元" }, { "VND", "越南盾" }, { "ZAR", "南非幣" } };


                                //@@@20180716, Patrick
                                // 找出來文扣押的金額
                                List<CaseSeizure> list2 = accountBiz.GetCaseSeizure(caseId, trans).Where(x => x.CustId.StartsWith(strObligorNo)).ToList();
                                var allSei = list2.ToList();
                                decimal allseiamt = allSei.Sum(x => x.SeizureAmountNtd); // 已扣押的金額

                                #endregion

                                #region 找出備註
                                CaseMemo memo1 = new CaseMemo();
                                List<CaseMemo> allCM2 = new CaseMemoBiz().MemoList2(caseId, CaseMemoType.CaseSeizureMemo, trans);
                                if (allCM2.Count() > 0)
                                {
                                    memo1 = allCM2.First();
                                    string allmemo333 = string.Join("\r\n", allCM2.Where(x=>!x.Memo.Contains("!@!")).Select(x => x.Memo).ToList());
                                    memo1.Memo = allmemo333;
                                    memoCaseid = caseId;
                                }
                                else
                                {
                                    memo1.CaseId = caseId;
                                    memoCaseid = caseId;
                                    memo1.Memo = "";
                                    memo1.MemoType = CaseMemoType.CaseSeizureMemo;
                                }

                                #endregion

                                if (allseiamt < 450 && list2.Count() > 0)
                                {
                                    #region 總扣押金額<450
                                    if (!memo1.Memo.Contains("戶名不符"))
                                    {
                                        // 要依分行, 再依匯率來顯示
                                        var list3 = list2.Where(x => x.CustId.StartsWith(strObligorNo)).ToList();
                                        var brGroup = (from p in list3
                                                       group p by new { p.BranchName, p.Currency } into g
                                                       select new { BranchName = g.Key.BranchName, Curr = g.Key.Currency, SeiAmt1 = g.Sum(x=>x.SeizureAmountNtd) }).ToList();

                                        foreach (var itemList in brGroup)
                                        {
                                            string _branchNo = itemList.BranchName.PadRight(iMaxBranchLenght, '　').Substring(0, iMaxBranchLenght);
                                            strOligorList = strOligorList + "存款分行：" + _branchNo.PadRight(iMaxBranchLenght, '　').Substring(0, iMaxBranchLenght);
                                            if( itemList.SeiAmt1==0)
                                                strOligorList = strOligorList + "扣押金額：" + itemList.Curr  + " " + "0" + "\r\n";
                                            else
                                                strOligorList = strOligorList + "扣押金額：" + itemList.Curr + " " + itemList.SeiAmt1.ToString("###,##0.##") + "\r\n";
                                        }
                                        if (isEnough450)
                                        {
                                            strOligorList = strOligorList +  "扣除手續費存款餘額未達200元，不予扣押。";
                                        }
                                    }
                                    if (memo1.Memo.Contains("帳戶開立之戶名與來函戶名不符"))
                                    {
                                        // 要依分行, 再依匯率來顯示
                                        var list3 = list2.Where(x => x.CustId.StartsWith(strObligorNo)).ToList();
                                        var brGroup = (from p in list3
                                                       group p by new { p.BranchName, p.Currency } into g
                                                       select new { BranchName = g.Key.BranchName, Curr = g.Key.Currency }).ToList();
                                        foreach (var itemList in brGroup)
                                        {
                                            string _branchNo = itemList.BranchName.PadRight(iMaxBranchLenght, '　').Substring(0, iMaxBranchLenght);
                                            strOligorList = strOligorList + "存款分行：" + _branchNo.PadRight(iMaxBranchLenght, '　').Substring(0, iMaxBranchLenght);
                                            strOligorList = strOligorList + "扣押金額：" + itemList.Curr + " " + "0" + "\r\n";
                                        }
                                        //string _branchNo = list2.First().BranchName;
                                        //strOligorList = strOligorList + "存款分行：" + _branchNo.PadRight(iMaxBranchLenght, '　').Substring(0, iMaxBranchLenght);
                                        //strOligorList = strOligorList + "扣押金額：" + "TWD" + " " + "0" + "\r\n";
                                    }
                                    #endregion
                                    strOligorList = strOligorList + "\r\n\r\n";
                                }
                                else
                                {
                                    #region 總扣押金額>=450
                                    if (! isEnough) // 不足額
                                    {
                                        
                                        #region 不足額的情況
                                        if (!memo1.Memo.Contains("戶名不符"))
                                        {
                                            #region 戶名相符的情況
                                            // 7/25 因為CASE 15 , 有多種幣別.. 調整.Where(x=>x.SeizureAmount>0)
                                            foreach (var itemList in listForCaseSeizure2.Where(x =>  x.name.CustId.StartsWith(strObligorNo)))
                                            {
                                                strOligorList = strOligorList + "存款分行：" + itemList.name.BranchName.PadRight(iMaxBranchLenght, '　').Substring(0, iMaxBranchLenght);
                                                if (itemList.name.Currency == "TWD")
                                                {
                                                    if (itemList.SeizureAmount != 0)
                                                    {
                                                        strOligorList = strOligorList + "扣押金額：" + itemList.name.Currency + " " + itemList.SeizureAmount.ToString("###,###") + "\r\n";
                                                    }
                                                    else
                                                    {
                                                        strOligorList = strOligorList + "扣押金額：" + itemList.name.Currency + " 0 \r\n";
                                                    }
                                                }
                                                else
                                                {
                                                    strOligorList = strOligorList + "扣押金額：" + itemList.name.Currency + " " + string.Format("{0:N}", itemList.SeizureAmount) + "\r\n";
                                                }

                                                if( isEnough450 )
                                                {
                                                    strOligorList = "\r\n" + "扣除手續費存款餘額未達200元，不予扣押。";
                                                }



                                                //* 外幣備註要多一筆
                                                if (itemList.name.Currency != "TWD" && itemList.SeizureAmount > 0 && strmemo != "1")//*寫入備註的話只執行一次
                                                {
                                                    strDesc = strDesc.Replace("{Memo}", "其扣押款為外幣存款，收取時將依當時匯率兌換新台幣。\r\n{Memo}");
                                                    // 將此文字, 寫回CaseMemo
                                                    //{
                                                    //    CaseMemo cm = new CaseMemo() { CaseId = memo1.CaseId, MemoType = CaseMemoType.CaseSeizureMemo, MemoUser = "Z00004771" };
                                                    //    cm.Memo = "其扣押款為外幣存款，收取時將依當時匯率兌換新台幣。\r\n";
                                                    //    CaseMemoBiz cmb = new CaseMemoBiz();
                                                    //    cmb.Create2(cm);
                                                    //}
                                                    newMemo = "其扣押款為外幣存款，收取時將依當時匯率兌換新台幣。\r\n";
                                                    strmemo = "1";
                                                }
                                            }
                                            #endregion
                                        }// if 戶名不符
                                        else // 戶名不符的
                                        {
                                            #region 戶名不符的情況
                                            if (memo1.Memo.Contains("另一帳戶開立之戶名與來函戶名不符"))
                                            {
                                                foreach (var itemList in listForCaseSeizure2.Where(x => x.SeizureAmount >=0  && x.name.CustId.StartsWith(strObligorNo)))
                                                {
                                                    strOligorList = strOligorList + "存款分行：" + itemList.name.BranchName.PadRight(iMaxBranchLenght, '　').Substring(0, iMaxBranchLenght);
                                                    if (itemList.name.Currency == "TWD")
                                                    {
                                                        if (itemList.SeizureAmount != 0)
                                                        {
                                                            strOligorList = strOligorList + "扣押金額：" + itemList.name.Currency + " " + itemList.SeizureAmount.ToString("###,###") + "\r\n";
                                                        }
                                                        else
                                                        {
                                                            strOligorList = strOligorList + "扣押金額：" + itemList.name.Currency + " 0 \r\n";
                                                        }
                                                    }
                                                    else
                                                    {
                                                        strOligorList = strOligorList + "扣押金額：" + itemList.name.Currency + " " + string.Format("{0:N}", itemList.SeizureAmount) + "\r\n";
                                                    }
                                                    //* 外幣備註要多一筆
                                                    if (itemList.name.Currency != "TWD" && itemList.SeizureAmount > 0 && strmemo != "1")//*寫入備註的話只執行一次
                                                    {
                                                        strDesc = strDesc.Replace("{Memo}", "其扣押款為外幣存款，收取時將依當時匯率兌換新台幣。\r\n{Memo}");
                                                        newMemo = "其扣押款為外幣存款，收取時將依當時匯率兌換新台幣。\r\n";
                                                        strmemo = "1";
                                                    }
                                                }
                                            }
                                            #endregion
                                        } // 戶名不符的
                                        #endregion
                                    }
                                    else //足額
                                    {
                                        #region 足額的情況
                                        // 先以分行別group, 加總每個分行要扣多少NTD
                                        var branchGroup = (from p in allSei
                                                           group p by p.BranchName into g
                                                           select new { BranchName = g.Key, BranchTotal = g.Sum(x => x.SeizureAmountNtd) }).ToList();

                                        //var branchGroup = listForCaseSeizure2.Where(x => x.name.CustId == strObligorNo).ToList();
                                        foreach (var b in branchGroup)
                                        {

                                            string _branchNo = b.BranchName;
                                            strOligorList = strOligorList + "存款分行：" + _branchNo.PadRight(iMaxBranchLenght, '　').Substring(0, iMaxBranchLenght);
                                            //strOligorList = strOligorList + "扣押金額：" + b.name.Currency + " " + b.SeizureAmount.ToString("###,##0.##") + "\r\n";
                                            strOligorList = strOligorList + "扣押金額：" + "TWD" + " " + b.BranchTotal.ToString("###,##0.##") + "\r\n";



                                        } // end foreach (var b in branchGroup)

                                        // 足額, 用 ... 其中永吉分行部分扣押款新臺幣150元為外幣存款(約當5美元)，收取時將依當時匯率兌換新臺幣。

                                        var gBranch = (from p in branchGroup group p by p.BranchName into g select g.Key).ToList();

                                        foreach(var g1 in gBranch)
                                        {
                                            string _ntd = allseiamt.ToString("###,###,###,###");
                                            string currStr = "";
                                            var groupCurrency = from p in allSei.Where(x => x.BranchName == g1 && x.CustId.StartsWith(strObligorNo))
                                                                group p by p.Currency into g
                                                                select new { Ccy = g.Key, Total = g.Sum(x => x.SeizureAmount), CcyTotal = g.Sum(x => x.SeizureAmountNtd) };

                                            decimal _ntdTotal = 0.0m;
                                            foreach (var s in groupCurrency)
                                            {
                                                string chineseCcy = "";
                                                string strMoney = "";
                                                if (CurrTrans.ContainsKey(s.Ccy))
                                                {
                                                    chineseCcy = CurrTrans[s.Ccy].ToString();
                                                    strMoney = ((decimal)s.Total).ToString("###,###,##0.##");
                                                    _ntdTotal += s.CcyTotal;
                                                    currStr += strMoney + chineseCcy + "、";
                                                }
                                            }
                                            if (currStr.EndsWith("、"))
                                                currStr = currStr.Substring(0, currStr.Length - 1);

                                            if (_ntdTotal > 0)
                                            {
                                                string message = "";
                                                if( obligorList.Count()>1 )
                                                    message = "其中統編：" + strObligorNo + " "+ g1 + "部分扣押款新臺幣" + _ntdTotal.ToString("###,###,###") + "元為外幣存款(約當" + currStr + ")，收取時將依當時匯率兌換新臺幣。\r\n{Memo}";
                                                else
                                                    message = "其中"+ g1 + "部分扣押款新臺幣" + _ntdTotal.ToString("###,###,###") + "元為外幣存款(約當" + currStr + ")，收取時將依當時匯率兌換新臺幣。\r\n{Memo}";
                                                strDesc = strDesc.Replace("{Memo}", message);
                                                newMemo = newMemo + message;
                                            }
                                        }

                                        #endregion
                                    }

                                    strOligorList = strOligorList + "\r\n\r\n";
                                    #endregion
                                }

                            }

                        }
                    }
                    //}
                    strDesc = strDesc.Replace("{ObligorList}", strOligorList);
                    //CaseMemo memo = new CaseMemoBiz().Memo(caseId, CaseMemoType.CaseSeizureMemo, trans);
                    List<CaseMemo> allCM = new CaseMemoBiz().MemoList2(caseId, CaseMemoType.CaseSeizureMemo, trans);
                    string memo = null;

                    if (allCM.Count() > 0)
                    {
                        memo = string.Join("\r\n", allCM.Select(x => x.Memo));
                    }

                    if (!string.IsNullOrEmpty(memo))
                    {
                        string nullstr = memo.Replace("\r\n", "");
                        if (nullstr == "")
                            memo = "";
                    }

                    if (memo != null && !string.IsNullOrEmpty(memo))
                    {
                        int iPos = memo.IndexOf("!@!");
                        if( iPos>0)
                        {
                            //memo = memo.Substring(iPos+3);
                            memo="";
                            var cmb = new CaseMemoBiz();
                            //先砍掉
                            var a = cmb.Update2(new CaseMemo() { CaseId = caseId, MemoType = CaseMemoType.CaseSeizureMemo }, trans);
                            foreach (var m in allCM)
                            {
                                if (m.Memo.Contains("!@!"))
                                {
                                    int iPos1 = m.Memo.IndexOf("!@!");
                                    string newMemo1 = m.Memo.Substring(iPos1 + 3);
                                    CaseMemo cm222 = new CaseMemo() { CaseId = memoCaseid, Memo = newMemo1, MemoType = CaseMemoType.CaseSeizureMemo, MemoUser = "Z00004771" };
                                    memo += newMemo1 + "\r\n";
                                    cmb.Create2(cm222);
                                }
                                else
                                    memo += m.Memo + "\r\n";
                            }
                        }
                        strDesc = strDesc.Replace("{Memo}", memo);
                    }
                    else
                    {
                        if (strDesc.Contains("收取時將依當時匯率兌換新臺幣。") || strDesc.Contains("收取時將依當時匯率兌換新台幣。"))
                        {
                            strDesc = strDesc.Replace("{Memo}", "");
                        }
                        else
                        {
                            strDesc = strDesc.Replace("{Memo}", "無。");
                        }

                    }

                    if (!string.IsNullOrEmpty(newMemo))
                    {
                        newMemo = newMemo.Replace("{Memo}", "");
                        CaseMemo cm = new CaseMemo() { CaseId = memoCaseid, Memo = newMemo, MemoType = CaseMemoType.CaseSeizureMemo, MemoUser = "Z00004771" };
                        CaseMemoBiz cmb = new CaseMemoBiz();
                        cmb.Create2(cm);
                    }

                }
            }
            if (caseKind2 == "支付")
            {
                string strObligorList = "";
                List<CaseObligor> listObligor = new CaseObligorBIZ().ObligorModel(caseId, trans);
                if (listObligor != null && listObligor.Any())
                {
                    foreach (CaseObligor obligor in listObligor)
                    {
                        strObligorList = strObligorList + "義(債)務人戶名：" + obligor.ObligorName + "\r\n";
                        strObligorList = strObligorList + "義(債)務人統編：" + obligor.ObligorNo + "\r\n\r\n";
                    }
                }
                strObligorList = strObligorList.Length > 0 ? strObligorList.Substring(0, strObligorList.Length - 2) : "";
                strDesc = strDesc.Replace("{ObligorList}", strObligorList);

                if (payeeModel == null)
                {
                    payeeModel = new CasePayeeSettingBIZ().GetQueryList(new CasePayeeSetting { CaseId = caseId }, trans).FirstOrDefault();
                }
                if (payeeModel != null)
                {
                    string checkno = UtlString.Right("0000000" + payeeModel.CheckNo, 7);
                    strDesc = strDesc.Replace("{BranchName}", payeeModel.Bank);
                    strDesc = strDesc.Replace("{Money}", "TWD " + UtlString.FormatCurrency(payeeModel.Money, 0));
                    strDesc = strDesc.Replace("{Fee}", "TWD " + UtlString.FormatCurrency(payeeModel.Fee, 0));
                    strDesc = strDesc.Replace("{CheckNo}", checkno);
                    strDesc = strDesc.Replace("{ReceivePerson}", payeeModel.ReceivePerson);
                    string strMemo = payeeModel.Memo;
                    if (string.IsNullOrEmpty(strMemo))
                    {
                        strMemo = "無";
                    }
                    strDesc = strDesc.Replace("{Memo}", strMemo);
                }
            }
            if (caseKind1 == "外來文案件")
            {
                CaseObligorBIZ co = new CaseObligorBIZ();
                List<CaseObligor> list = co.ObligorModel(caseId, trans);
                if (list != null && list.Any())
                {
                    strDesc = strDesc.Replace("{ObligorName}", list[0].ObligorName);
                    strDesc = strDesc.Replace("{ObligorNo}", list[0].ObligorNo);

                    string account = list[0].ObligorAccount;
                    if (!string.IsNullOrEmpty(account))
                    {
                        account = "、" + account;
                    }
                    strDesc = strDesc.Replace("{ObligorAccount}", account);
                }
            }

            return new HistorySendSettingRef { Subject = strSubject, Description = strDesc, SendDate = sendDate };
        }


        public int EditSSR(HistorySendSettingRef model)
        {
            int rtn = 0;
            IDbConnection dbConnection = base.OpenConnection();
            IDbTransaction dbTransaction = null;
            try
            {
                using (dbConnection)
                {
                    dbTransaction = dbConnection.BeginTransaction();
                    string strSql = @"update History_SendSettingRef set
                                            Subject=@Subject,
                                            Description=@Description
                                    where CaseKind=@CaseKind and CaseKind2=@CaseKind2";
                    base.Parameter.Clear();

                    // 添加參數
                    base.Parameter.Add(new CommandParameter("@Subject", model.Subject));
                    base.Parameter.Add(new CommandParameter("@Description", model.Description));
                    base.Parameter.Add(new CommandParameter("@CaseKind", model.CaseKind));
                    base.Parameter.Add(new CommandParameter("@CaseKind2", model.CaseKind2));
                    rtn = base.ExecuteNonQuery(strSql, dbTransaction);
                    dbTransaction.Commit();
                }
                return rtn;
            }
            catch (Exception ex)
            {
                try
                {
                    dbTransaction.Rollback();
                }
                catch (Exception ex2)
                {
                }
                throw ex;
            }
        }

        public List<HistorySendSettingRef> select(string CaseKind2)
        {
            try
            {
                string sqlStr = @"select Subject,Description from History_SendSettingRef where CaseKind2=@CaseKind2";

                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@CaseKind2", CaseKind2));
                IList<HistorySendSettingRef> list = base.SearchList<HistorySendSettingRef>(sqlStr);
                List<HistorySendSettingRef> listItem = new List<HistorySendSettingRef>();
                if (list != null & list.Count > 0)
                {
                    foreach (var item in list)
                    {
                        listItem.Add(item);
                    }
                }
                return listItem;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}