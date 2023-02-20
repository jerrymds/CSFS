using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Web;
using System.Web.Mvc;
using CTBC.CSFS.BussinessLogic;
using CTBC.CSFS.Models;
using CTBC.CSFS.Pattern;
using CTBC.CSFS.ViewModels;
using CTBC.CSFS.Resource;

namespace CTBC.CSFS.Areas.Agent.Controllers
{
    public class AgentHandleDetailController : AppController
    {
        PARMCodeBIZ parm = new PARMCodeBIZ();
        CaseSeizureViewModel caseview = new CaseSeizureViewModel();
        CaseMasterBIZ casemaster = new CaseMasterBIZ();
        CaseAttachmentBIZ attachment = new CaseAttachmentBIZ();
        CaseObligorBIZ obligor = new CaseObligorBIZ();
        CaseCalculatorDetailsBIZ calDetail = new CaseCalculatorDetailsBIZ();
        AgentHandleDetailBIZ ahdBIZ = new AgentHandleDetailBIZ();


        // GET: Agent/AgentHandleDetail
        public ActionResult Index(Guid caseId)
        {
            ViewBag.CaseId = caseId;
            var master = casemaster.MasterModel(caseId);
            ViewBag.CaseKind = master.CaseKind;
            ViewBag.CaseKind2 = master.CaseKind2;
            return View();
        }

        /// <summary>
        /// 綁定來文機關
        /// </summary>
        /// <param name="govKind"></param>
        /// <returns></returns>
        public JsonResult ChangGovUnit(string govKind)
        {
            PARMCodeBIZ parm = new PARMCodeBIZ();
            List<KeyValuePair<string, string>> items = new List<KeyValuePair<string, string>>();
            if (string.IsNullOrEmpty(govKind)) return Json(items);
            //* 取得大類的CodeNo,以知道小類的CodeType
            var itemKind = parm.GetCodeData("GOV_KIND").FirstOrDefault(a => a.CodeDesc == govKind);
            if (itemKind == null) return Json(items);

            var list = parm.GetCodeData(itemKind.CodeNo);
            if (list.Any())
            {
                items.AddRange(list.Select(govUnit => new KeyValuePair<string, string>(govUnit.CodeNo.ToString(), govUnit.CodeDesc)));
            }
            return Json(items);
        }

        /// <summary>
        /// 判斷來文字號是否重複
        /// </summary>
        /// <param name="txtGovNo"></param>
        /// <returns></returns>
        public ActionResult IsGovNoExist(string txtGovNo)
        {
            CaseMasterBIZ casemaster = new CaseMasterBIZ();
            //* 1-有重複 0-不重複
            return Json(casemaster.IsGovNoExist(txtGovNo) == "1" ? new JsonReturn { ReturnCode = "1", ReturnMsg = "" }
                                                                : new JsonReturn { ReturnCode = "0", ReturnMsg = "" });
        }

        /// <summary>
        /// 綁定下拉菜單
        /// </summary>
        public void InitDropdownListOptions()
        {
            ViewBag.CASE_END_TIME = parm.GetCASE_END_TIME("CASE_END_TIME");                                     //* 每天最晚時間
            ViewBag.GOV_KINDList = new SelectList(parm.GetCodeData("GOV_KIND"), "CodeDesc", "CodeDesc");        //* 來文機關類型
            ViewBag.SpeedList = new SelectList(parm.GetCodeData("INCOME_SPEED"), "CodeDesc", "CodeDesc");       //* 速別
            ViewBag.ReceiveKindList = new SelectList(parm.GetCodeData("INCOME_TYPE"), "CodeDesc", "CodeDesc");  //* 來文方式
            ViewBag.CaseKindList = new SelectList(parm.GetCodeData("CASE_KIND"), "CodeDesc", "CodeDesc", "外來文案件");       //* 類別-大類
            ViewBag.CaseKind2List = new SelectList(parm.GetCodeData("CASE_EXTERNAL"), "CodeDesc", "CodeDesc");   //* 類別-小類-扣押
        }

        public ActionResult _Main(Guid CaseId)
        {
            InitDropdownListOptions();              //* 綁定頁面下拉列表

            caseview.CaseMaster = casemaster.MasterModel(CaseId);  //* 得到CaseMaster的model
            //* 以下是頁面初始化值
            ViewBag.GovUnitList = new SelectList(parm.SelectGovUnitByGOV_KIND(caseview.CaseMaster.GovKind), "CodeDesc", "CodeDesc");//* 綁定來文機關下拉列表
            caseview.CaseObligorlistO = new List<CaseObligor>(10);//* 初始化義務人員行數

            caseview.CaseAttachmentlistO = attachment.AttachmentList(CaseId);//* 得到CaseAttachment的model
            List<CaseObligor> list = obligor.ObligorModel(CaseId);    //* 得到CaseObligor的model
            for (int i = 0; i < list.Count; i++)
            {
                caseview.CaseObligorlistO.Add(list[i]);
            }
            for (int i = list.Count; i < 10; i++)
            {
                caseview.CaseObligorlistO.Add(new CaseObligor());
            }
            return View(caseview);
        }

        [HttpPost]
        public ActionResult EditMaster(CaseSeizureViewModel model, IEnumerable<HttpPostedFileBase> fileAttNames)
        {
            model.CaseMaster.ModifiedUser = LogonUser.Account;   //*修改人
            model.CaseAttachmentlistO = new List<CaseAttachment>();

            #region  保存附件
            try
            {
                foreach (var aModel in fileAttNames.Select(UploadFile).Where(aModel => aModel != null))
                {
                    model.CaseAttachmentlistO.Add(aModel);
                }
            }
            catch (Exception ex)
            {
                return Content(@"<script>parent.showMessage('0','" + ex.Message + "');</script>");
            }
            #endregion

            var scriptStr = casemaster.EditCase(ref model) ? "parent.showMessage('1','" + string.Format(Lang.csfs_case_delete_success0, model.CaseMaster.CaseNo) + "');"
                                                             : "parent.showMessage('0','" + Lang.csfs_edit_fail + "');";
            return Content(@"<script>" + scriptStr + "</script>");
        }

        /// <summary>
        /// 儲存上傳文件
        /// </summary>
        /// <param name="upFile"></param>
        /// <returns></returns>
        public CaseAttachment UploadFile(HttpPostedFileBase upFile)
        {
            if (upFile == null || upFile.ContentLength <= 0) return null;
            //获取用户上传文件的后缀名,重命名為當前登入者ID+年月日時分秒毫秒
            string newFileName = LogonUser.Account + "_" + DateTime.Now.ToString("yyyyMMddhhmmssfff") + Path.GetExtension(upFile.FileName);
            //利用file.SaveAs保存图片
            string name = Path.Combine(Server.MapPath("~/Uploads/"), newFileName);
            upFile.SaveAs(name);

            CaseAttachment aModel = new CaseAttachment
            {
                AttachmentName = Path.GetFileName(upFile.FileName),
                AttachmentServerName = newFileName,
                AttachmentServerPath = Server.MapPath("~/Uploads/"),
                isDelete = 0,
                CreatedUser = LogonUser.Account
            };
            return aModel;
        }

        /// <summary>
        /// 刪除一筆附件資料
        /// </summary>
        /// <param name="AttachId"></param>
        /// <returns></returns>
        public ActionResult DeleteAttatch(string AttachId)
        {
            return Content(attachment.DeleteAttatch(AttachId) > 0 ? "1" : "0");
        }

        public ActionResult ProcessLog(AgentHandleDetail AHD)
        {
            return PartialView("ProcessLog", SearchList(AHD));
        }

        public AgentHandleDetailViewModel SearchList(AgentHandleDetail AHD)
        {
            AgentHandleDetailViewModel viewModel;
            AHD.LanguageType = Session["CultureName"].ToString();
            IList<AgentHandleDetail> result = ahdBIZ.GetQueryList(AHD);

            viewModel = new AgentHandleDetailViewModel()
            {
                AgentHandleDetail = AHD,
                AgentHandleDetailList = result,
            };

            return viewModel;
        }

        /// <summary>
        /// 綁定利率類型下拉列表
        /// </summary>
        public void BindInterestRateList()
        {
            Dictionary<char, string> drop = new Dictionary<char, string>();
            drop.Add('Y', "年利率");
            drop.Add('D', "日利率");
            List<SelectListItem> item = new List<SelectListItem>();
            foreach (KeyValuePair<char, string> items in drop)
            {
                item.Add(new SelectListItem { Text = items.Value.ToString(), Value = items.Key.ToString() });
            }
            ViewBag.InterestRateList = item;
        }

        public ActionResult CaseCalculator(Guid CaseId)
        {
            BindInterestRateList();     //* 綁定利率類型下拉列表
            CaseCalculatorViewModel model = new CaseCalculatorViewModel()
            {
                CaseCalculatorMain = new CaseCalculatorMain()
                {
                    CaseId = CaseId,

                },
                CaseCalculatorDetails = new CaseCalculatorDetails()
                {
                    CaseId = CaseId,
                },
                CaseCalculatorDetailsList = calDetail.DetailModel(CaseId)
            };
            ViewBag.CaseId = CaseId;
            ViewBag.Sum = calDetail.SumInterest(CaseId);       //* 計息合計
            return View(model);
        }

        //新增一筆計算資料
        public ActionResult CreateCalculatorDetail(Decimal amount, Decimal InterestRate, DateTime StartDate, DateTime EndDate, Guid CaseId, string CaseType)
        {
            int n = 0;
            int interestSum = 0;
            if (CaseType == "D")        //*日利率
            {
                int days = (EndDate - StartDate).Days + 1; //* 計算天數  
                decimal interests = Math.Round(amount * InterestRate * days, 0);//* 計算利率
                n = CreateModel(amount, InterestRate, StartDate.ToString("yyyy/MM/dd"), EndDate.ToString("yyyy/MM/dd"), CaseId, days, interests);//* 新增一筆計算記過
                interestSum = calDetail.SumInterest(CaseId);       //* 計息合計
            }
            if (CaseType == "Y")        //*年利率
            {
                if (EndDate.Year - StartDate.Year == 0)     //* 相同年份時，插入一筆數據
                {
                    int days = (EndDate - StartDate).Days + 1;//* 計算天數  
                    decimal interests = Math.Round(amount * InterestRate * days / 365, 0);//* 計算利率
                    n = CreateModel(amount, InterestRate, StartDate.ToString("yyyy/MM/dd"), EndDate.ToString("yyyy/MM/dd"), CaseId, days, interests);//* 新增一筆計算結果
                    interestSum = calDetail.SumInterest(CaseId);       //* 計息合計
                }
                if (EndDate.Year - StartDate.Year > 0)     //* 年份不同時，插入多筆數據
                {
                    //* 計算第一階段利率
                    DateTime StartDate1 = Convert.ToDateTime(StartDate.Year + "/12/31");
                    int days1 = (StartDate1 - StartDate).Days + 1;//* 計算第一階段天數  
                    decimal interests1 = Math.Round(amount * InterestRate * days1 / 365, 0);//* 計算第一階段利率
                    int n1 = CreateModel(amount, InterestRate, StartDate.ToString("yyyy/MM/dd"), StartDate1.ToString("yyyy/MM/dd"), CaseId, days1, interests1);//* 計算第一階段利息

                    int daysamount = 0;              //*天數
                    string start = string.Empty;    //* 起始日
                    string end = string.Empty;      //*結束日
                    for (int i = 0; i < (EndDate.Year - StartDate.Year - 1); i++)
                    {
                        start = (StartDate.Year + 1 + i) + "/1/1";
                        end = (StartDate.Year + 1 + i) + "/12/31";
                        daysamount = (Convert.ToDateTime(end) - Convert.ToDateTime(start)).Days;
                        decimal interestsamount = Math.Round(amount * InterestRate * daysamount / 365, 0);//* 計算多個階段的利率
                        CreateModel(amount, InterestRate, start, end, CaseId, daysamount, interestsamount);
                    }

                    //* 計算第二階段利率
                    DateTime EndDate1 = Convert.ToDateTime(EndDate.Year + "/1/1");
                    int days2 = (EndDate - EndDate1).Days + 1;//* 計算最後階段天數  
                    decimal interests2 = Math.Round(amount * InterestRate * days2 / 365, 0);//* 計算最後階段利率
                    int n2 = CreateModel(amount, InterestRate, EndDate1.ToString("yyyy/MM/dd"), EndDate.ToString("yyyy/MM/dd"), CaseId, days2, interests2);
                    interestSum = calDetail.SumInterest(CaseId);       //* 計息合計
                    if (n1 > 0 && n2 > 0)
                    {
                        n = 1;
                    }
                }
            }
            if (n > 0) return Content("true|" + interestSum);
            else return Content("false");
        }

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

        public ActionResult DeleteCalCase(int calId, Guid CaseId)
        {
            int n = calDetail.Delete(calId);
            int sum = calDetail.SumInterest(CaseId);       //* 計息合計
            return Content(n > 0 ? "true|" + sum : "false");
        }




        #region 帳務資訊
        /// <summary>
        /// 帳務資訊-外來文
        /// </summary>
        /// <returns></returns>
        public ActionResult _AccountForExternal(Guid caseId)
        {

            return View();
        }
        /// <summary>
        /// 帳務資訊-扣押
        /// </summary>
        /// <returns></returns>
        public ActionResult _AccountForSeizure(Guid caseId)
        {
            var master = casemaster.MasterModel(caseId);
            ViewBag.CaseKind = master.CaseKind;
            ViewBag.CaseKind2 = master.CaseKind2;
            if(master.CaseKind != CaseKind.CASE_SEIZURE)
                return Content("不是扣押案件");

            //* 呼叫Biz 取得扣押資訊

            //* 如果扣押資訊取不到.說明沒有儲存過.呼叫Biz從電文撈最新的客戶資料(SQL類似如下.通過CaseId找到所有主表最新的第一筆.再找從表)
                    //* 案件編號.直接拿Master的欄位. 分行名稱通過參數表取
                    //SELECT 
                    //M.[CustomerId],
                    //M.[CustomerName],
                    //D.[Branch],
                    //D.[Account],
                    //D.[StsDesc],
                    //D.[Ccy],
                    //D.[Bal]
                    //FROM 
                    //(
                    //SELECT 
                    //ROW_NUMBER() OVER (PARTITION BY [CustomerId] ORDER BY [SNO] DESC) AS RowID, [SNO],[CustomerId],[CustomerName]
                    //FROM [TX_60491_Grp]
                    //WHERE [CustomerId] IN (SELECT [ObligorNo] FROM [CaseObligor] WHERE [CaseId] = @CaseId)
                    //) AS M
                    //LEFT OUTER JOIN [TX_60491_Detl] AS D ON M.[SNO] = D.[FKSNO]
                    //WHERE M.RowID = 1;

            return View();
        }
        #endregion
    }
}