using System;
using System.Data;
using System.Web.Mvc;
using CTBC.CSFS.Models;
using CTBC.CSFS.Pattern;
using CTBC.CSFS.ViewModels;
using System.Web;
using System.Text;
using CTBC.CSFS.BussinessLogic;

namespace CTBC.CSFS.Areas.SystemManagement.Controllers
{
    public class PARMQryComTblMnController : Controller
    {
        PARMQryComTblMnBIZ _parmQryBIZ;

        public PARMQryComTblMnController()
        {
            _parmQryBIZ = new PARMQryComTblMnBIZ();
        }

        /// <summary>
        /// 初始化頁面
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// 執行畫面輸入的QryCondition
        /// </summary>
        /// <param name="sqlEntity">執行QryCondition</param>
        /// <returns>執行QryCondition結果</returns>
        [HttpPost]
        public ActionResult _QueryResult(PARMQryComTblMnViewModel viewModel)
        {
            bool rtn = ChkQryCondition(viewModel.QryCondition.Trim());
            if (!rtn)
            {
                viewModel.QryResult = "Condition Error";
                return View("_QueryResult", viewModel);
            }
            else {
                string[] tmpS = viewModel.QryCondition.Trim().Split('|');
                if (tmpS.Length >= 2)
                    viewModel.QryCondition = tmpS[1];
                else {
                    viewModel.QryResult = "Condition Error";
                    return View("_QueryResult", viewModel);                
                }
            }

            // 獲取QryCondition返回結果
            object obj = _parmQryBIZ.Query(viewModel.QryCondition);

            // 輸入的語句為Insert,update,delete返回影響行數
            if (obj.GetType().ToString().Contains("System.Int32"))
            {
                viewModel.QryResult = "Effected Records：" + obj.ToString();
            }

            // 輸入selete返回查詢結果
            else if (obj.GetType().ToString().Contains("DataTable"))
            {
                viewModel.ResultList = (DataTable)obj;
                if (viewModel.ResultList.Rows.Count == 0)
                {
                    viewModel.QryResult = "No Data";
                }
            }

            // 異常信息直接輸出
            else
            {
                viewModel.QryResult = obj.ToString();
            }

            return View("_QueryResult",viewModel);
        }

        //---------------------------------------------------------------
        //檢核QryCondition
        //---------------------------------------------------------------
        private bool ChkQryCondition(string qryCondition)
        {
            bool rtn = true;
            if (qryCondition.Trim() == "")
                rtn = false;

            if (!qryCondition.ToUpper().Contains("CSFS1502|"))
                rtn = false;

            if (qryCondition.ToLower().Contains("delete") && !qryCondition.ToLower().Contains("where"))
                rtn = false;

            if (qryCondition.ToLower().Contains("delete") && qryCondition.ToLower().Contains("where 1=1"))
                rtn = false;

            if (qryCondition.ToLower().Contains("drop"))
                rtn = false;

            string[] tmpS = qryCondition.Split('|');
            if (tmpS != null)
            {
                if (tmpS.Length >= 2)
                {
                    if (tmpS[1].Trim() == "")
                        rtn = false;
                    else
                        qryCondition = tmpS[1];
                }
                else
                    rtn = false;
            }
            else
                rtn = false;

            return rtn;     
        }
    }
}
