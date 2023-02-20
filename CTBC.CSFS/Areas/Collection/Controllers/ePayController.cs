using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CTBC.CSFS.Models;
using CTBC.CSFS.Resource;
using CTBC.CSFS.BussinessLogic;
using CTBC.CSFS.ViewModels;
using CTBC.CSFS.Pattern;
using System.Data;

namespace CTBC.CSFS.Areas.Collection.Contrllers
{
    public class ePayController : AppController
    {
        #region 全局變量
        ePayBIZ _ePayBIZ = new ePayBIZ();
        #endregion

        #region 頁面
        /// <summary>
        /// 外文啟動查詢
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            CaseMaster model = new CaseMaster();

            if (LogonUser != null && !string.IsNullOrEmpty(LogonUser.RCAFAccount) && !string.IsNullOrEmpty(LogonUser.RCAFBranch))
            {
                model.IsEnable = "Y";
            }
            else
            {
                model.IsEnable = "N";
            }

            return View(model);
        }

        /// <summary>
        /// 查詢結果清單
        /// </summary>
        /// <param name="model">實體類</param>
        /// <param name="pageNum">當前頁面</param>
        /// <param name="strSortExpression">排序欄位:案件編號/來文字號/義(債)務人統編</param>
        /// <param name="strSortDirection">排序方式：asc</param>
        /// <returns></returns>
        public ActionResult _QueryResult(CaseMaster model, int pageNum = 1, string strSortExpression = "CaseMaster.CaseNo,CaseMaster.ReceiveDate", string strSortDirection = "ASC")
        {
            return PartialView("_QueryResult", SearchList(model, strSortExpression, strSortDirection, pageNum));
        }

        #endregion

        #region 自定義方法
        /// <summary>
        /// 實際查詢動作
        /// </summary>
        /// <param name="model">實體類</param>
        /// <param name="pageNum">當前頁面</param>
        /// <returns></returns>
        public CaseMasterViewModel SearchList(CaseMaster model, string strSortExpression, string strSortDirection, int pageNum = 1)
        {
            // 查詢清單資料
            IList<CaseMaster> result = _ePayBIZ.GetQueryList(model, pageNum, strSortExpression, strSortDirection);

            var viewModel = new CaseMasterViewModel()
            {
                CaseMaster = model,
                CaseMasterList = result,

                // 查詢案件總筆數
                DataCount = _ePayBIZ.GetDataCount()
            };

            // 資料清單之每頁資料數、當前頁頁碼、資料總筆數賦值
            viewModel.CaseMaster.PageSize = _ePayBIZ.PageSize;
            viewModel.CaseMaster.CurrentPage = _ePayBIZ.PageIndex;
            viewModel.CaseMaster.TotalItemCount = _ePayBIZ.DataRecords;
            viewModel.CaseMaster.SortExpression = strSortExpression;
            viewModel.CaseMaster.SortDirection = strSortDirection;

            return viewModel;
        }

    
        /// <summary>
        /// 啟動發查
        /// </summary>
        /// <returns></returns>
        public ActionResult StartSearch()
        {
            DataTable dt = new DataTable();
            string flag = _ePayBIZ.StartSearch(this.LogonUser);

            return Json(flag, JsonRequestBehavior.AllowGet);
        }

        #endregion
    }
}