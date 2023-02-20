using System;
using System.Collections.Generic;
using System.Web.Mvc;
using CTBC.CSFS.BussinessLogic;
using CTBC.CSFS.Models;
using CTBC.CSFS.Pattern;
using CTBC.CSFS.ViewModels;
using System.Transactions;
using CTBC.CSFS.Resource;
using CTBC.FrameWork.Util;

namespace CTBC.CSFS.Areas.Common.Controllers
{
    public class HistoryAgentCaseCalculatorDetailController : AppController
    {

        CaseCalculatorDetailsBIZ calDetail = new CaseCalculatorDetailsBIZ();
        CaseCalculatorMainBIZ calMain = new CaseCalculatorMainBIZ();

        /// <summary>
        /// 進入利率畫面
        /// </summary>
        /// <param name="caseId"></param>
        /// <returns></returns>
        public ActionResult Index(Guid caseId, string FromControl)
        {
            ViewBag.CaseId = caseId;
            ViewBag.FromControl = FromControl;
            BindInterestRateList();     //* 綁定利率類型下拉列表
            CaseCalculatorViewModel model = new CaseCalculatorViewModel()
            {
                CaseCalculatorMain = calMain.GetCaseMainInfo(caseId),
            };
            ViewBag.Sum = calDetail.SumInterest(caseId);       //* 計息合計
            return View(model);
        }

        /// <summary>
        /// 計算利息
        /// </summary>
        /// <param name="amount"></param>
        /// <param name="interestRate"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="caseId"></param>
        /// <param name="caseType"></param>
        /// <returns></returns>
        public ActionResult CreateCalculatorDetail(Decimal amount, Decimal interestRate, DateTime startDate, DateTime endDate, Guid caseId, string caseType)
        {
            int n = 0;
            int interestSum = 0;
            startDate = UtlString.FormatDateTwStringToAd(startDate);
            endDate = UtlString.FormatDateTwStringToAd(endDate);
            if (caseType == "D")        //*日利率
            {
                int days = (endDate - startDate).Days + 1; //* 計算天數  
                decimal interests = Math.Round(amount * interestRate * days, 0);//* 計算利率
                n = CreateModel(amount, interestRate, startDate.ToString("yyyy/MM/dd"), endDate.ToString("yyyy/MM/dd"), caseId, days, interests);//* 新增一筆計算記過
                interestSum = calDetail.SumInterest(caseId);       //* 計息合計
            }
            if (caseType == "Y")        //*年利率
            {
                if (endDate.Year - startDate.Year == 0)     //* 相同年份時，插入一筆數據
                {
                    int days = (endDate - startDate).Days + 1;//* 計算天數  
                    decimal interests = Math.Round(amount * interestRate * days / 365, 0);//* 計算利率
                    n = CreateModel(amount, interestRate, startDate.ToString("yyyy/MM/dd"), endDate.ToString("yyyy/MM/dd"), caseId, days, interests);//* 新增一筆計算結果
                    interestSum = calDetail.SumInterest(caseId);       //* 計息合計
                }
                if (endDate.Year - startDate.Year > 0)     //* 年份不同時，插入多筆數據
                {
                    using (TransactionScope ts = new TransactionScope())
                    {
                        //* 計算第一階段利率
                        DateTime startDate1 = Convert.ToDateTime(startDate.Year + "/12/31");
                        int days1 = (startDate1 - startDate).Days + 1;//* 計算第一階段天數  
                        decimal interests1 = Math.Round(amount * interestRate * days1 / 365, 0);//* 計算第一階段利率
                        CreateModel(amount, interestRate, startDate.ToString("yyyy/MM/dd"), startDate1.ToString("yyyy/MM/dd"), caseId, days1, interests1);//* 計算第一階段利息

                        int daysamount = 0;              //*天數
                        string start = string.Empty;    //* 起始日
                        string end = string.Empty;      //*結束日
                        for (int i = 0; i < (endDate.Year - startDate.Year - 1); i++)
                        {
                            start = (startDate.Year + 1 + i) + "/1/1";
                            end = (startDate.Year + 1 + i) + "/12/31";
                            //daysamount = (Convert.ToDateTime(end) - Convert.ToDateTime(start)).Days;
                            daysamount = 365;
                            decimal interestsamount = Math.Round(amount * interestRate * daysamount / 365, 0);//* 計算多個階段的利率
                            CreateModel(amount, interestRate, start, end, caseId, daysamount, interestsamount);
                        }

                        //* 計算第二階段利率
                        DateTime endDate1 = Convert.ToDateTime(endDate.Year + "/1/1");
                        int days2 = (endDate - endDate1).Days + 1;//* 計算最後階段天數  
                        decimal interests2 = Math.Round(amount * interestRate * days2 / 365, 0);//* 計算最後階段利率
                        CreateModel(amount, interestRate, endDate1.ToString("yyyy/MM/dd"), endDate.ToString("yyyy/MM/dd"), caseId, days2, interests2);
                        interestSum = calDetail.SumInterest(caseId);       //* 計息合計
                        n = 1;
                        ts.Complete();  //调用Complete进行事务提交，如果没有调用Complete则事务自动回滚
                    }
                }
            }
            if (n > 0) return Content("true|" + interestSum);
            else return Content("false");
        }

        public ActionResult EditMain(Guid caseId, int amount1, int amount2, int amount3, int amount4, int amount5)
        {
            int sum = calDetail.SumInterest(caseId);       //* 計息合計
            if (calMain.Count(caseId) > 0)                      //*已存在本caseid的數據，不存在則新增
                return Content(calMain.Edit(caseId, amount1, amount2, amount3, amount4, amount5) > 0 ? "true|" + sum : "false");
            else                                                     
                return Content(calMain.Create(caseId, amount1, amount2, amount3, amount4, amount5) > 0 ? "true|" + sum : "false");
        }

        /// <summary>
        /// 新增成功后的結果
        /// </summary>
        /// <param name="caseId"></param>
        /// <returns></returns>
        public ActionResult QueryResult(Guid caseId)
        {
            CaseCalculatorViewModel model = new CaseCalculatorViewModel()
            {
                CaseCalculatorDetailsList = calDetail.DetailModel(caseId)
            };
            ViewBag.Sum = calDetail.SumInterest(caseId);       //* 計息合計
            return PartialView("CalResult", model);
        }

        /// <summary>
        /// 刪除利息結果
        /// </summary>
        /// <param name="calId"></param>
        /// <param name="caseId"></param>
        /// <returns></returns>
        public ActionResult DeleteCalCase(int calId, Guid caseId)
        {
            int n = calDetail.Delete(calId);
            int sum = calDetail.SumInterest(caseId);       //* 計息合計
            return Content(n > 0 ? "true|" + sum : "false");
        }

        /// <summary>
        /// 計算利息結果
        /// </summary>
        /// <param name="amount"></param>
        /// <param name="interestrate"></param>
        /// <param name="startdate"></param>
        /// <param name="enddate"></param>
        /// <param name="caseid"></param>
        /// <param name="days"></param>
        /// <param name="interestsdata"></param>
        /// <returns></returns>
        public int CreateModel(Decimal amount, Decimal interestrate, string startdate, string enddate, Guid caseid, int days, decimal interestsdata)
        {
            CaseCalculatorViewModel model = new CaseCalculatorViewModel()
            {
                CaseCalculatorDetails = new CaseCalculatorDetails(),
            };
            model.CaseCalculatorDetails.CaseId = caseid;
            model.CaseCalculatorDetails.Amount = amount;
            model.CaseCalculatorDetails.InterestRate = interestrate;
            model.CaseCalculatorDetails.StartDate = startdate;
            model.CaseCalculatorDetails.EndDate = enddate;
            model.CaseCalculatorDetails.InterestDays = days;
            model.CaseCalculatorDetails.Interest = interestsdata;
            model.CaseCalculatorDetails.InterestReal = Convert.ToInt32(interestsdata);
            int n = calDetail.Create(model.CaseCalculatorDetails);
            if (n > 0) return 1;
            else return 0;
        }

        /// <summary>
        /// 綁定利率類型下拉列表
        /// </summary>
        public void BindInterestRateList()
        {
            List<SelectListItem> item2 = new List<SelectListItem>
            {
                new SelectListItem() {Text = Lang.csfs_cal_yinterest, Value = "Y"},
                new SelectListItem() {Text = Lang.csfs_cal_dinterest, Value = "D"}
            };
            ViewBag.InterestRateList = item2;
        }
    }
}