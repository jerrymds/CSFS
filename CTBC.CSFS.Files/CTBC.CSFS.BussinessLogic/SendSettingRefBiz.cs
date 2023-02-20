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
    public class SendSettingRefBiz : CommonBIZ
    {
        public SendSettingRef GetSendSettingRef(string caseKind, string casekind2, IDbTransaction trans = null)
        {
            string sql = @"SELECT [CaseKind],[CaseKind2],[Subject],[Description] 
                            FROM [SendSettingRef] WHERE [CaseKind] = @CaseKind AND [CaseKind2] = @CaseKind2";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("CaseKind", caseKind));
            Parameter.Add(new CommandParameter("CaseKind2", casekind2));
            IList<SendSettingRef> list = trans == null ? SearchList<SendSettingRef>(sql) : SearchList<SendSettingRef>(sql, trans);
            return list.FirstOrDefault();
        }

        public SendSettingRef GetSubjectAndDescription(Guid caseId, string template, string sendKind, IDbTransaction trans = null, CasePayeeSetting payeeModel = null)
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
            SendSettingRefBiz refBiz = new SendSettingRefBiz();
            SendSettingRef refObj = refBiz.GetSendSettingRef(caseKind1, caseKind2, trans);
            if (refObj == null)
                return new SendSettingRef();

            string strSubject = refObj.Subject;
            string strDesc = refObj.Description;
            //* 公文
            strSubject = strSubject.Replace("{GovNo}", master.GovNo);
            int iyy =Convert.ToInt16(master.GovDate.Substring(0,4))-1911;
            string strmm=master.GovDate.Substring(5,2);
            string strdd=master.GovDate.Substring(8,2);
            strSubject = strSubject.Replace("{GovDate}",iyy.ToString()+strmm+strdd);
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
                    foreach (var item in obligorList)
                    {
                        string strObligorNo= item.ObligorNo;
                        //* 回圈每個義務人統編
                        if (!listForCaseSeizure2.Any() || listForCaseSeizure2.All(m => m.name.CustId != strObligorNo))
                        {
                            strOligorList = strOligorList + "義(債)務人戶名：" + item.ObligorName + " \r\n" +
                                                            "義(債)務人統編：" + item.ObligorNo + " \r\n" +
                                                            "說明：於本行無存款往來\r\n\r\n";
                        }
                        else
                        {
                            strOligorList = strOligorList + "義(債)務人戶名：" + item.ObligorName + "\r\n" +
                                                            "義(債)務人統編：" + item.ObligorNo + "\r\n";
                            int iMaxBranchLenght = listForCaseSeizure2.Where(m => m.name.CustId == strObligorNo).Select(itemList => itemList.name.BranchName.Length).Concat(new[] {0}).Max() + 1;
                            if (iMaxBranchLenght < 5)
                                iMaxBranchLenght = 5;
                            foreach (var itemList in listForCaseSeizure2.Where(m => m.name.CustId == strObligorNo))
                            {
                                strOligorList = strOligorList + "存款分行：" + itemList.name.BranchName.PadRight(iMaxBranchLenght,'　').Substring(0,iMaxBranchLenght);
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
                                    strDesc = strDesc.Replace("{Memo}", "其扣押款為外幣存款，收取時將依當時匯率兌換新台幣。\r\n {Memo}");
                                    strmemo = "1";
                                }
                            }
                            strOligorList = strOligorList + "\r\n";
                        }
                    }
                    //}
                    strDesc = strDesc.Replace("{ObligorList}", strOligorList);
                    CaseMemo memo = new CaseMemoBiz().Memo(caseId, CaseMemoType.CaseSeizureMemo, trans);
                    if (memo != null && !string.IsNullOrEmpty(memo.Memo))
                    {
                        strDesc = strDesc.Replace("{Memo}", memo.Memo);
                    }
                    else
                    {
                        if (strDesc.Contains("其扣押款為外幣存款，收取時將依當時匯率兌換新台幣。"))
                        {
                            strDesc = strDesc.Replace("{Memo}", "");
                        }
                        else
                        {
                            strDesc = strDesc.Replace("{Memo}", "無。");
                        }
                        
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

            return new SendSettingRef { Subject = strSubject, Description = strDesc ,SendDate = sendDate};
        }

        public int EditSSR(SendSettingRef model)
        {
            int rtn = 0;
            IDbConnection dbConnection = base.OpenConnection();
            IDbTransaction dbTransaction = null;
            try
            {
                using (dbConnection)
                {
                    dbTransaction = dbConnection.BeginTransaction();
                    string strSql = @"update SendSettingRef set
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

        public List<SendSettingRef> select(string CaseKind2)
        {
            try
            {
                string sqlStr = @"select Subject,Description from SendSettingRef where CaseKind2=@CaseKind2";

                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@CaseKind2", CaseKind2));
                IList<SendSettingRef> list = base.SearchList<SendSettingRef>(sqlStr);
                List<SendSettingRef> listItem = new List<SendSettingRef>();
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