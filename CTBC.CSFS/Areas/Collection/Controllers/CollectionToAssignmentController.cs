using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using CTBC.CSFS.BussinessLogic;
using CTBC.CSFS.Models;
using CTBC.CSFS.Pattern;
using CTBC.CSFS.Resource;
using CTBC.CSFS.ViewModels;
using CTBC.FrameWork.Util;
using System.Web;
using CTBC.CSFS.Filter;

namespace CTBC.CSFS.Areas.Collection.Controllers
{
    public class CollectionToAssignmentController : AppController
    {
        CollectionToSignBIZ _c2aBiz;

        public CollectionToAssignmentController()
        {
            _c2aBiz = new CollectionToSignBIZ(this);
        }
        /// <summary>
        /// 查詢
        /// </summary>
        /// <returns></returns>
        /// 
        [RootPageFilter]
        public ActionResult Index(string isBack)
        {
            CollectionToSign model = new CollectionToSign();
            if (isBack == "1")
            {
                //*執行cookie里的內容
                HttpCookie getQueryCookie = Request.Cookies.Get("QueryCookie");
                if (getQueryCookie != null)
                {
                    if (getQueryCookie.Values["GovUnit"] != null) model.GovUnit = getQueryCookie.Values["GovUnit"];
                    if (getQueryCookie.Values["GovNo"] != null) model.GovNo = getQueryCookie.Values["GovNo"];
                    if (getQueryCookie.Values["Person"] != null) model.Person = getQueryCookie.Values["Person"];
                    if (getQueryCookie.Values["Speed"] != null) model.Speed = getQueryCookie.Values["Speed"];
                    if (getQueryCookie.Values["ReceiveKind"] != null) model.ReceiveKind = getQueryCookie.Values["ReceiveKind"];
                    if (getQueryCookie.Values["CaseKind"] != null) ViewBag.CaseKindQuery = getQueryCookie.Values["CaseKind"];
                    if (getQueryCookie.Values["CaseKind2"] != null) ViewBag.CaseKind2Query = getQueryCookie.Values["CaseKind2"];
                    if (getQueryCookie.Values["GovDateS"] != null) model.GovDateS = getQueryCookie.Values["GovDateS"];
                    if (getQueryCookie.Values["GovDateE"] != null) model.GovDateE = getQueryCookie.Values["GovDateE"];
                    if (getQueryCookie.Values["Unit"] != null) model.Unit = getQueryCookie.Values["Unit"];
                    if (getQueryCookie.Values["CreatedDateS"] != null) model.CreatedDateS = getQueryCookie.Values["CreatedDateS"];
                    if (getQueryCookie.Values["CreatedDateE"] != null) model.CreatedDateE = getQueryCookie.Values["CreatedDateE"];
                    if (getQueryCookie.Values["CaseNo"] != null) model.CaseNo = getQueryCookie.Values["CaseNo"];
                    ViewBag.CurrentPage = getQueryCookie.Values["CurrentPage"];
                    ViewBag.isQuery = "1";
                    ViewBag.isAutoDispatch = getQueryCookie.Values["isAutoDispatch"] != null ? getQueryCookie.Values["isAutoDispatch"] : "true";
                    ViewBag.isAutoDispatchFS = getQueryCookie.Values["isAutoDispatchFS"] != null ? getQueryCookie.Values["isAutoDispatchFS"] : "true";
                }
            }
            Bind();
            return View(model);
        }
        /// <summary>
        /// 初始化查詢
        /// </summary>
        public void Bind()
        {
            LdapEmployeeBiz empBiz = new LdapEmployeeBiz();
            AgentSettingBIZ ASBIZ = new AgentSettingBIZ();

            string strAgentList = JsonHelper.ObjectToJson(empBiz.GetAgentAndBuInfosByBuId(""));
            ViewBag.AgentList = strAgentList;

            //20170421 固定 RQ-2015-019666-017 派件至跨單位 宏祥 update start
            //ViewBag.Department = new SelectList(ASBIZ.GetKeBie(), "Department", "Department");            
            ViewBag.Department = new SelectList(ASBIZ.GetAgentSettingDepartment(), "Department", "Department");
            //20170421 固定 RQ-2015-019666-017 派件至跨單位 宏祥 update end

            ViewBag.SpeedList = new SelectList(empBiz.GetCodeData("INCOME_SPEED"), "CodeDesc", "CodeDesc");
            ViewBag.ReceiveKindList = new SelectList(empBiz.GetCodeData("INCOME_TYPE"), "CodeDesc", "CodeDesc");
            ViewBag.CaseKindList = new SelectList(empBiz.GetCodeData("CASE_KIND"), "CodeDesc", "CodeDesc");
            ViewBag.CaseKind2List = new SelectList(empBiz.GetCodeData("CASE_SEIZURE"), "CodeDesc", "CodeDesc");
            ViewBag.GOV_KINDList = new SelectList(empBiz.GetCodeData("GOV_KIND"), "CodeDesc", "CodeDesc");
            ViewBag.isAutoDispatch = ASBIZ.GetEnable().IsAutoDispatch ? "true" : "false";
            ViewBag.isAutoDispatchFS = ASBIZ.GetEnable().IsAutoDispatchFS ? "true" : "false";
        }

        /// <summary>
        /// 結果列表
        /// </summary>
        /// <param name="ctoS"></param>
        /// <param name="pageNum"></param>
        /// <param name="strSortExpression"></param>
        /// <param name="strSortDirection"></param>
        /// <returns></returns>
        public ActionResult _QueryResult(CollectionToSign ctoS, int pageNum = 1, string strSortExpression = "CaseNo,DocNo ", string strSortDirection = "asc")
        {
            HttpCookie AToHCookie = new HttpCookie("QueryCookie");
            AToHCookie.Values.Add("GovUnit", ctoS.GovUnit);
            AToHCookie.Values.Add("GovNo", ctoS.GovNo);
            AToHCookie.Values.Add("Person", ctoS.Person);
            AToHCookie.Values.Add("Speed", ctoS.Speed);
            AToHCookie.Values.Add("ReceiveKind", ctoS.ReceiveKind);
            AToHCookie.Values.Add("CaseKind", ctoS.CaseKind);
            AToHCookie.Values.Add("CaseKind2", ctoS.CaseKind2);
            AToHCookie.Values.Add("GovDateS", ctoS.GovDateS);
            AToHCookie.Values.Add("GovDateE", ctoS.GovDateE);
            AToHCookie.Values.Add("Unit", ctoS.Unit);
            AToHCookie.Values.Add("CreatedDateS", ctoS.CreatedDateS);
            AToHCookie.Values.Add("CreatedDateE", ctoS.CreatedDateE);
            AToHCookie.Values.Add("CaseNo", ctoS.CaseNo);
            AToHCookie.Values.Add("CurrentPage", pageNum.ToString());
            Response.Cookies.Add(AToHCookie);
            return PartialView("_QueryResult", SearchList(ctoS, pageNum, strSortExpression, strSortDirection));
        }
        /// <summary>
        /// 實際查詢動作
        /// </summary>
        /// <param name="ctoS"></param>
        /// <param name="pageNum"></param>
        /// <param name="strSortExpression"></param>
        /// <param name="strSortDirection"></param>
        /// <returns></returns>
        public CollectionToSignViewModel SearchList(CollectionToSign ctoS, int pageNum = 1, string strSortExpression = "CaseNo", string strSortDirection = "asc")
        {
            ctoS.LanguageType = Session["CultureName"].ToString();
            ctoS.CreatedDateS = UtlString.FormatDateTwStringToAd(ctoS.CreatedDateS);
            ctoS.CreatedDateE = UtlString.FormatDateTwStringToAd(ctoS.CreatedDateE);
            ctoS.GovDateS = UtlString.FormatDateTwStringToAd(ctoS.GovDateS);
            ctoS.GovDateE = UtlString.FormatDateTwStringToAd(ctoS.GovDateE);
            // 新增個資LOG
            string CustId = "";
            string _user = (HttpContext.Session["UserAccount"] != null) ? HttpContext.Session["UserAccount"].ToString() : "";
            System.Collections.Specialized.NameValueCollection _parameters = new System.Collections.Specialized.NameValueCollection();
            //System.Collections.Specialized.NameValueCollection _parameters = ("POST".Equals(HttpContext.Request.HttpMethod)) ? HttpContext.Request.Form : HttpContext.Request.QueryString;
            string _controller = (this.ControllerContext.RouteData.Values["Controller"] != null) ? this.ControllerContext.RouteData.Values["Controller"].ToString() : "";
            string _action = (this.ControllerContext.RouteData.Values["Action"] != null) ? this.ControllerContext.RouteData.Values["Action"].ToString() : "";
            string _ip = (HttpContext.Request != null && !string.IsNullOrEmpty(HttpContext.Request.ServerVariables["REMOTE_ADDR"])) ? HttpContext.Request.ServerVariables["REMOTE_ADDR"] : "";
            BaseBusinessRule _business = new BaseBusinessRule();
            IList<CollectionToSign> result = _c2aBiz.GetQueryList(ctoS, pageNum, strSortExpression, strSortDirection);
            if (result.Count > 0)
            {
                for (int i = 0; i < result.Count; i++)
                {
                    if (!String.IsNullOrEmpty(result[i].ObligorNo))
                    {
                        CustId = result[i].ObligorNo.Trim();
                        if (CustId.Trim().Length > 11)
                        {
                            CustId = CustId.Substring(0, 11);
                        }
                        _business.ExecuteAPLOGSave(_user, _controller, _action, _ip, "CASEID=" + result[i].CaseId.ToString(), CustId);
                    }
                }
            }
                var viewModel = new CollectionToSignViewModel()
            {
                CollectionToSign = ctoS,
                CollectionToSignList = result,
            };

            //分頁相關設定
            viewModel.CollectionToSign.PageSize = _c2aBiz.PageSize;
            viewModel.CollectionToSign.CurrentPage = _c2aBiz.PageIndex;
            viewModel.CollectionToSign.TotalItemCount = _c2aBiz.DataRecords;
            viewModel.CollectionToSign.SortExpression = strSortExpression;
            viewModel.CollectionToSign.SortDirection = strSortDirection;

            viewModel.CollectionToSign.GovUnit = ctoS.GovUnit;
            viewModel.CollectionToSign.GovNo = ctoS.GovNo;
            viewModel.CollectionToSign.Person = ctoS.Person;
            viewModel.CollectionToSign.Speed = ctoS.Speed;
            viewModel.CollectionToSign.ReceiveKind = ctoS.ReceiveKind;
            viewModel.CollectionToSign.CaseKind = ctoS.CaseKind;
            viewModel.CollectionToSign.CaseKind2 = ctoS.CaseKind2;
            viewModel.CollectionToSign.GovDateS = ctoS.GovDateS;
            viewModel.CollectionToSign.GovDateE = ctoS.GovDateE;
            return viewModel;
        }

        //public ActionResult Sign(string strIds)
        //{
        //    string[] aryId = strIds.Split(',');
        //    List<Guid> guidList = (from id in aryId where !string.IsNullOrEmpty(id) select new Guid(id)).ToList();
        //    string userId = Convert.ToString(Session["UserAccount"]);
        //    return Json(_c2aBiz.Sign(guidList, userId) > 0 ? new JsonReturn() { ReturnCode = "1", ReturnMsg = "" }
        //                                                : new JsonReturn() { ReturnCode = "0", ReturnMsg = Lang.csfs_del_fail });
        //}

        /// <summary>
        /// 案件退回
        /// </summary>
        /// <param name="strIds"></param>
        /// <returns></returns>
        public ActionResult Return(string strIds, string returnReason)
        {
            string[] aryId = strIds.Split(',');
            List<Guid> guidList = (from id in aryId where !string.IsNullOrEmpty(id) select new Guid(id)).ToList();
            string userId = LogonUser.Account;
            return Json(_c2aBiz.Return(guidList, userId, returnReason) > 0 ? new JsonReturn() { ReturnCode = "1", ReturnMsg = "" }
                                                        : new JsonReturn() { ReturnCode = "0", ReturnMsg = Lang.csfs_del_fail });
        }

        /// <summary>
        /// 實際分派動作
        /// </summary>
        /// <param name="caseIdList"></param>
        /// <param name="agentIdList"></param>
        /// <returns></returns>
        public ActionResult AssignAgent(string caseIdList, string agentIdList)
        {
            string[] aryId = caseIdList.Split(',');
            List<Guid> aryCaseId = (from id in aryId where !string.IsNullOrEmpty(id) select new Guid(id)).ToList();
            aryId = agentIdList.Split(',');
            List<string> aryAgentId = (from id in aryId where !string.IsNullOrEmpty(id) select id).ToList();
            string userId = LogonUser.Account;
            return Json(_c2aBiz.CollectionAssign(aryCaseId, aryAgentId, userId));
        }

        ///// <summary>
        ///// 更改來文機關類型.取得來文機關
        ///// </summary>
        ///// <param name="govKind"></param>
        ///// <returns></returns>
        //public JsonResult ChangGovUnit(string govKind)
        //{
        //    PARMCodeBIZ parm = new PARMCodeBIZ();
        //    List<KeyValuePair<string, string>> items = new List<KeyValuePair<string, string>>();
        //    if (string.IsNullOrEmpty(govKind)) return Json(items);
        //    //* 取得大類的CodeNo,以知道小類的CodeType
        //    var itemKind = parm.GetCodeData("GOV_KIND").FirstOrDefault(a => a.CodeDesc == govKind);
        //    if (itemKind == null) return Json(items);

        //    var list = parm.GetCodeData(itemKind.CodeNo);
        //    if (list.Any())
        //    {
        //        items.AddRange(list.Select(govUnit => new KeyValuePair<string, string>(govUnit.CodeNo.ToString(), govUnit.CodeDesc)));
        //    }
        //    return Json(items);
        //}
        /// <summary>
        /// 根據案件類型大類(扣押/外來文)來取小類
        /// </summary>
        /// <param name="caseKind"></param>
        /// <returns></returns>
        public JsonResult ChangCaseKind1(string caseKind)
        {
            PARMCodeBIZ parm = new PARMCodeBIZ();
            List<KeyValuePair<string, string>> items = new List<KeyValuePair<string, string>>();
            if (string.IsNullOrEmpty(caseKind)) return Json(items);
            //* 取得大類的CodeNo,以知道小類的CodeType
            var itemKind = parm.GetCodeData("CASE_KIND").FirstOrDefault(a => a.CodeDesc == caseKind);
            if (itemKind == null) return Json(items);

            var list = parm.GetCodeData(itemKind.CodeNo);
            if (list.Any())
            {
                items.AddRange(list.Select(govUnit => new KeyValuePair<string, string>(govUnit.CodeNo.ToString(), govUnit.CodeDesc)));
            }
            return Json(items);
        }

    }
}