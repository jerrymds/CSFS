using System.Collections.Generic;
using CTBC.CSFS.BussinessLogic;
using CTBC.CSFS.Models;
using CTBC.CSFS.ViewModels;
using System.Web.Mvc;
using System;
using System.Text;
using CTBC.CSFS.Resource;
using System.IO;
using System.Configuration;
using CTBC.CSFS.Pattern;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Web;
using CTBC.CSFS.WebService.OpenStream;
using Newtonsoft.Json;



namespace CTBC.CSFS.Areas.TrsDetail.Contrllers
{
    public class eTrsHisRecordDetailController : AppController
    {
        #region 全局變量
        eTrsHisRecordBIZ _eTrsHisRecordBIZ = new eTrsHisRecordBIZ();
        PARMCodeBIZ _PARMCodeBIZ = new PARMCodeBIZ();
        #endregion

        #region 明細頁面


        /// </summary>
        /// <param name="DocNo">案件編號</param>
        /// <param name="pageFrom">頁碼</param>
        /// <param name="Flag">註記 Flag=1：主管檢視放行；Flag=2：歷史記錄查詢與重送 </param>
        /// <returns></returns>
        public ActionResult Index(Guid strKey,String strNewID, String pageFrom, string Flag)
        {
            //CaseTrsDetailViewModel Model = new CaseTrsDetailViewModel();
            string strFilePath = ConfigurationManager.AppSettings["txtFilePath"] + @"\";

            //CaseMaster model1 = _eTrsHisRecordBIZ.GetCaseMaster(strKey.ToString());
            //  model.PageSize 
            CaseHisCondition model = _eTrsHisRecordBIZ.GetCaseHisCondition(strNewID);
            model.CaseId = strKey.ToString();
            model.NewID = strNewID;

            // 前頁面進入前停留的頁數
            model.PageSource = Flag;

            // 前頁面進入前停留的頁數
            model.PageFrom = pageFrom;
            ViewData["txtReciveFilePath"] = ConfigurationManager.AppSettings["txtReciveFilePath"];
            //ModelState.Clear();
            return View(model);
        }

        /// </summary>
        /// <param name="model">實體類</param>
        /// <param name="pageNum">當前頁面</param>
        /// <param name="strSortExpression">排序欄位</param>
        /// <param name="strSortDirection">排序方式</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult _QueryResult(CaseHisCondition model, int pageNum = 1, string strSortExpression = "CaseHisCondition.DocNo", string strSortDirection = "asc")
        {
            HttpCookie modelCookie = new HttpCookie("CaseTrsQueryDetail");
            modelCookie.Values.Add("NewID", model.NewID);
            modelCookie.Values.Add("CaseId", model.CaseId);
            Response.Cookies.Add(modelCookie);
            return PartialView("_QueryResult", DetailSearchList(model, strSortExpression, strSortDirection, pageNum));
        }

        #endregion

        #region 自定義方法
        /// <summary>
        /// 主管檢視放行明細頁面-清單查詢方法
        /// </summary>
        /// <param name="model"></param>
        /// <param name="strSortExpression"></param>
        /// <param name="strSortDirection"></param>
        /// <param name="pageNum"></param>
        /// <returns></returns>
        public CaseTrsDetailViewModel DetailSearchList(CaseHisCondition model, string strSortExpression, string strSortDirection, int pageNum = 1)
        {
            // 查詢清單資料
            IList<CaseTrsQueryVersion> result = _eTrsHisRecordBIZ.GetDetailQueryList(model, pageNum, strSortExpression, strSortDirection);

            CaseMaster model1 = _eTrsHisRecordBIZ.GetCaseMaster(model.CaseId);
            //  model.PageSize 
            CaseHisCondition model2 = _eTrsHisRecordBIZ.GetCaseHisCondition(model.NewID);
            CaseTrsQueryVersion model3 = _eTrsHisRecordBIZ.GetCaseTrsQueryVersion(model.NewID);
            //var Model = new CaseTrsDetailViewModel()
            //{
            var Model = new CaseTrsDetailViewModel()
            {
                CaseMaster = model1,
                CaseHisCondition= model2,
                CaseTrsQueryVersion = model3,
                CaseTrsQueryVersionList = result,
            };

            // 資料清單之每頁資料數、當前頁頁碼、資料總筆數賦值
            Model.CaseHisCondition.PageSize = _eTrsHisRecordBIZ.PageSize;
            Model.CaseHisCondition.CurrentPage = _eTrsHisRecordBIZ.PageIndex;
            Model.CaseHisCondition.TotalItemCount = _eTrsHisRecordBIZ.DataRecords;
            Model.CaseHisCondition.SortExpression = strSortExpression;
            Model.CaseHisCondition.SortDirection = strSortDirection;
            Model.CaseHisCondition.CaseId = model.CaseId;
            return Model;
        }

        //  public ActionResult Save(CaseTrsDetailViewModel model, Guid CaseId, string NewID)
        //public ActionResult Save(List<CaseTrsQueryDetails> model, String CaseId, string NewID)
        [HttpPost]
        public ActionResult Save(CaseTrsDetailViewModel model)
        {
            HttpCookie cookies = Request.Cookies.Get("CaseTrsQueryDetail");
            bool rtn = false;
            string userId = LogonUser.Account;
            string Message = "";
            string QDateS = "";
            string QDateE = "";
            #region 儲存

            if (model != null && model.CaseTrsQueryVersionList.Count > 0)
            {
                for (int i = 0; i < model.CaseTrsQueryVersionList.Count; i++) // 使用 Count
                {
                    //Message = @"New" + i.ToString() + "-" + 

                    ///string RowNum = model.CaseTrsQueryVersionList[i].RowNum.ToString();
                    if (model.CaseTrsQueryVersionList[i].QDateS.ToString() != "")
                    {
                          QDateS = Convert.ToDateTime(model.CaseTrsQueryVersionList[i].QDateS).ToString("yyyyMMdd");
                    }
                    if (model.CaseTrsQueryVersionList[i].QDateE.ToString() != "")
                    {
                         QDateE = Convert.ToDateTime(model.CaseTrsQueryVersionList[i].QDateE).ToString("yyyyMMdd");
                    }
                    //model[i].CustAccount.ToString() + "||" +
                    //model[i].CaseStatusName.ToString() + "||" +
                    //model[i].QDateS.ToString() + "||" +
                    //model[i].QDateE.ToString() + "||" +
                    //model[i].RFDMQryMessage.ToString() + "||" +
                    //model[i].OpenDate.ToString() + "||" +
                    //model[i].LastDate.ToString();
                    //string NewID = cookies.Values["NewID"].ToString();
                    if (model.CaseTrsQueryVersionList[i].CaseStatusName == "未處理" || model.CaseTrsQueryVersionList[i].CaseStatusName == "拋查中" ) // 正式要改 ==  "未處理"
                    {
                        rtn = _eTrsHisRecordBIZ.Save(model.CaseTrsQueryVersionList[i].NewID.ToString(), model.CaseTrsQueryVersionList[i].CustID, model.CaseTrsQueryVersionList[i].CustAccount, QDateS, QDateE, userId);
                        if (rtn)
                        {
                            Message = Message + model.CaseTrsQueryVersionList[i].NewID.ToString() + "修改成功. ";
                        }
                        else
                        {
                            Message = Message + model.CaseTrsQueryVersionList[i].NewID.ToString() + "修改失敗";
                        }
                    }
                }


            }
            if (Message.IndexOf("成功") < 0)
            {
                return Json(new JsonReturn { ReturnCode = "0", ReturnMsg = Message });
            }
            else
            {
                return Json(new JsonReturn { ReturnCode = "1", ReturnMsg = Message });
            }
            #endregion


        }
  

        /// <summary>
        /// 來文txt 
        /// 因window.open 只能打開工程下的文件，不能指定目錄，故調整為下載
        /// </summary>
        /// <param name="FileName">文件名</param>
        /// <returns></returns>

        #endregion




    }
}
