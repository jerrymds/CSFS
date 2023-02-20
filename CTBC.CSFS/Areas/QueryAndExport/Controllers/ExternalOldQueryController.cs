using CTBC.CSFS.Pattern;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CTBC.CSFS.BussinessLogic;
using CTBC.CSFS.Models;
using System.IO;
using CTBC.CSFS.Filter;
using CTBC.FrameWork.Util;
using CTBC.CSFS.ViewModels;
using CTBC.CSFS.Resource;

namespace CTBC.CSFS.Areas.QueryAndExport.Controllers
{
	public class ExternalOldQueryController : AppController
	{
		ExternalOldQueryBIZ EQBIZ = new ExternalOldQueryBIZ();
		PARMCodeBIZ parm = new PARMCodeBIZ();
		static string where = string.Empty;
		//
		// GET: /QueryAndExport/ExternalOldQuery/
		[RootPageFilter]
		public ActionResult Index(string isBack)
		{
			ExternalOldQuery model = new ExternalOldQuery();
			if (isBack == "1")
			{
				HttpCookie cookies = Request.Cookies.Get("QueryCookie");
				if (cookies != null)
				{
					//收文開始序號
					if (cookies.Values["ReceiverNbrS"] != null) model.ReceiverNbrS = cookies.Values["ReceiverNbrS"];
					//收文結束序號
					if (cookies.Values["ReceiverNbrE"] != null) model.ReceiverNbrE = cookies.Values["ReceiverNbrE"];
					//分行別起
					if (cookies.Values["BranchCodeS"] != null) model.BranchCodeS = cookies.Values["BranchCodeS"];
					//分行別迄
					if (cookies.Values["BranchCodeE"] != null) model.BranchCodeE = cookies.Values["BranchCodeE"];
					//來文機關發文字號
					if (cookies.Values["OrgSendCaseNbr"] != null) model.OrgSendCaseNbr = cookies.Values["OrgSendCaseNbr"];
					//收件開始日期
					if (cookies.Values["ReceiveDateS"] != null) model.ReceiveDateS = cookies.Values["ReceiveDateS"];
					//收件結束日期
					if (cookies.Values["ReceiveDateE"] != null) model.ReceiveDateE = cookies.Values["ReceiveDateE"];
					//結案開始日期
					if (cookies.Values["CloseDateS"] != null) model.CloseDateS = cookies.Values["CloseDateS"];
					//結案結束日期
					if (cookies.Values["CloseDateE"] != null) model.CloseDateE = cookies.Values["CloseDateE"];
					//發文字號
					if (cookies.Values["ResponseCaseNbr"] != null) model.ResponseCaseNbr = cookies.Values["ResponseCaseNbr"];
					ViewBag.CurrentPage = cookies.Values["CurrentPage"];
					ViewBag.isQuery = "1";
				}
			}
			else
			{
				//model.BranchCodeS = model.BranchCodeE = LogonUser.UnitForKeyIn;
				//*20150811新需求, 分行角色只能看自己分行資料且不能修改
				LdapEmployeeBiz empBiz = new LdapEmployeeBiz();
				IList<LDAPEmployee> list = empBiz.GetNoRoleEmployeeIdList();
				IList<LDAPEmployee> list2 = empBiz.GetNoRoleEmployeeIdList2();
				if ((list != null && list.Any(m => m.EmpId.Trim().ToUpper() == LogonUser.Account.Trim().ToUpper()))
					|| (list2 != null && list2.Any(m => m.EmpId.Trim().ToUpper() == LogonUser.Account.Trim().ToUpper())))
				{
					//* 屬於分行角色
					model.BranchCodeS = model.BranchCodeE = new LdapEmployeeBiz().GetBranchId();
					ViewBag.UnitRead = "1";
				}
			}
			return View(model);
		}

		[HttpPost]
		public ActionResult _QueryResult(ExternalOldQuery model, int pageNum = 1, string strSortExpression = "ReceiveNbr", string strSortDirection = "asc")
		{
			#region Cookies
			HttpCookie modelCookie = new HttpCookie("QueryCookie");
			//收文開始序號
			modelCookie.Values.Add("ReceiverNbrS", model.ReceiverNbrS);
			//收文結束序號
			modelCookie.Values.Add("ReceiverNbrE", model.ReceiverNbrE);
			//收件開始日期
			modelCookie.Values.Add("ReceiveDateS", model.ReceiveDateS);
			//收件結束日期
			modelCookie.Values.Add("ReceiveDateE", model.ReceiveDateE);
			//來文機關發文字號
			modelCookie.Values.Add("OrgSendCaseNbr", model.OrgSendCaseNbr);
			//發文字號
			modelCookie.Values.Add("ResponseCaseNbr", model.ResponseCaseNbr);
			//分行別起
			modelCookie.Values.Add("BranchCodeS", model.BranchCodeS);
			//分行別訖
			modelCookie.Values.Add("BranchCodeE", model.BranchCodeE);
			//結案開始日期
			modelCookie.Values.Add("CloseDateS", model.CloseDateS);
			//結案結束日期
			modelCookie.Values.Add("CloseDateE", model.CloseDateE);
			modelCookie.Values.Add("CurrentPage", pageNum.ToString());
			Response.Cookies.Add(modelCookie);
			#endregion

			string UserId = LogonUser.Account;
			if (!string.IsNullOrEmpty(model.ReceiveDateS))
			{
				model.ReceiveDateS = UtlString.FormatDateTwStringToAd(model.ReceiveDateS);
			}
			if (!string.IsNullOrEmpty(model.ReceiveDateE))
			{
				model.ReceiveDateE = UtlString.FormatDateTwStringToAd(model.ReceiveDateE);
			}
			if (!string.IsNullOrEmpty(model.CloseDateS))
			{
				model.CloseDateS = UtlString.FormatDateTwStringToAd(model.CloseDateS);
			}
			if (!string.IsNullOrEmpty(model.CloseDateE))
			{
				model.CloseDateE = UtlString.FormatDateTwStringToAd(model.CloseDateE);
			}
			//model.ResponseEmp = LogonUser.UnitForKeyIn;
			//model.ResponseEmp = new LdapEmployeeBiz().GetBranchId();
			
			IList<ExternalOldQuery> list = EQBIZ.GetData(model, pageNum, strSortExpression, strSortDirection, UserId, ref where);
			var dtvm = new ExternalOldQueryViewModel()
			{
				ExternalOldQuery = model,
				ExternalOldQueryList = list
			};

            EQBIZ.SaveAPLog(list.Select(x => x.AccountID).ToArray());//APLog Redis ader 2022-01-01 - ADD

            //分頁相關設定
            dtvm.ExternalOldQuery.PageSize = EQBIZ.PageSize;
			dtvm.ExternalOldQuery.CurrentPage = EQBIZ.PageIndex;
			dtvm.ExternalOldQuery.TotalItemCount = EQBIZ.DataRecords;
			dtvm.ExternalOldQuery.SortExpression = strSortExpression;
			dtvm.ExternalOldQuery.SortDirection = strSortDirection;

			return PartialView("_QueryResult", dtvm);
		}

		public ActionResult Details(string ReceiveNbr)
		{
			ViewBag.ReceiveNbr = ReceiveNbr;
			ExternalOldQueryBIZ EQBIZ = new ExternalOldQueryBIZ();
			ExternalOldQueryViewModel viewmodel = new ExternalOldQueryViewModel()
			{
				ExternalOldDetailsList = EQBIZ.GetExternalOldDetailList(ReceiveNbr)
			};
            // 新增個資LOG
            string _user = (HttpContext.Session["UserAccount"] != null) ? HttpContext.Session["UserAccount"].ToString() : "";
            System.Collections.Specialized.NameValueCollection _parameters = new System.Collections.Specialized.NameValueCollection();
            //System.Collections.Specialized.NameValueCollection _parameters = ("POST".Equals(HttpContext.Request.HttpMethod)) ? HttpContext.Request.Form : HttpContext.Request.QueryString;
            string _controller = (this.ControllerContext.RouteData.Values["Controller"] != null) ? this.ControllerContext.RouteData.Values["Controller"].ToString() : "";
            string _action = (this.ControllerContext.RouteData.Values["Action"] != null) ? this.ControllerContext.RouteData.Values["Action"].ToString() : "";
            string _ip = (HttpContext.Request != null && !string.IsNullOrEmpty(HttpContext.Request.ServerVariables["REMOTE_ADDR"])) ? HttpContext.Request.ServerVariables["REMOTE_ADDR"] : "";
            BaseBusinessRule _business = new BaseBusinessRule();
            if (viewmodel.ExternalOldDetailsList.Count > 0)
            {
                for (int i = 0; i < viewmodel.ExternalOldDetailsList.Count; i++)
                {
                    if (!String.IsNullOrEmpty(viewmodel.ExternalOldDetailsList[i].AccountID))
                {
                    _business.ExecuteAPLOGSave(_user, _controller, _action, _ip, "CASEID=" + ReceiveNbr.ToString(), viewmodel.ExternalOldDetailsList[i].AccountID.ToString());
                }
                }
            }
            // 新增結束
            return PartialView("Details", viewmodel);
		}
	}
}