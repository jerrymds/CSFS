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

namespace CTBC.CSFS.Areas.NewCaseCust.Contrllers
{
    public class NewCaseCustController : AppController
    {
        #region 全局變量
        NewCaseCustBIZ _CaseCustBIZ = new NewCaseCustBIZ();
        #endregion

        #region 頁面
        /// <summary>
        /// 外文啟動查詢
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            CaseCustMaster model = new CaseCustMaster();

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
        public ActionResult _QueryResult(CaseCustMaster model, int pageNum = 1, string strSortExpression = "CaseCustMaster.DocNo,CaseCustMaster.Version,CaseCustDetails.IdNo", string strSortDirection = "ASC")
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
        public NewCaseCustViewModel SearchList(CaseCustMaster model, string strSortExpression, string strSortDirection, int pageNum = 1)
        {
            // 查詢清單資料
            IList<CaseCustMaster> result = _CaseCustBIZ.GetQueryList(model, pageNum, strSortExpression, strSortDirection);

            var viewModel = new NewCaseCustViewModel()
            {
                CaseCustMaster = model,
                NewCaseCustQueryList = result,

                // 查詢案件總筆數
                DataCount = _CaseCustBIZ.GetDataCount()
            };

            // 資料清單之每頁資料數、當前頁頁碼、資料總筆數賦值
            viewModel.CaseCustMaster.PageSize = _CaseCustBIZ.PageSize;
            viewModel.CaseCustMaster.CurrentPage = _CaseCustBIZ.PageIndex;
            viewModel.CaseCustMaster.TotalItemCount = _CaseCustBIZ.DataRecords;
            viewModel.CaseCustMaster.SortExpression = strSortExpression;
            viewModel.CaseCustMaster.SortDirection = strSortDirection;

            return viewModel;
        }

        /// <summary>
        /// 刪除
        /// </summary>
        /// <param name="Content">勾選資料</param>
        /// <returns></returns>
        public ActionResult DeleteCaseCustQuery(string Content)
        {
            return Json(_CaseCustBIZ.DeleteCaseCustMaster(Content), JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 啟動發查
        /// </summary>
        /// <returns></returns>
        public ActionResult StartSearch(string Content)
        {
            DataTable dt = new DataTable();
            string flag = _CaseCustBIZ.StartSearch(this.LogonUser, Content);

            return Json(flag, JsonRequestBehavior.AllowGet);
        }

        #endregion
    }
}