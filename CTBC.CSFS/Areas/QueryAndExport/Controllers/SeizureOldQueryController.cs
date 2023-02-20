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
	public class SeizureOldQueryController : AppController
	{
		SeizureOldQueryBIZ SQBIZ = new SeizureOldQueryBIZ();
		PARMCodeBIZ parm = new PARMCodeBIZ();
		static string where = string.Empty;
		//
		// GET: /QueryAndExport/SeizureOldQuery/
		[RootPageFilter]
		public ActionResult Index(string isBack)
		{
			SeizureOldQuery model = new SeizureOldQuery();
			if (isBack == "1")
			{
				HttpCookie cookies = Request.Cookies.Get("QueryCookie");
				if (cookies != null)
				{
					//收文開始序號
					if (cookies.Values["ReceiptSeqS"] != null) model.ReceiptSeqS = cookies.Values["ReceiptSeqS"];
					//收文結束序號
					if (cookies.Values["ReceiptSeqE"] != null) model.ReceiptSeqE = cookies.Values["ReceiptSeqE"];
					//收文開始日期
					if (cookies.Values["ReceivedDateS"] != null) model.ReceivedDateS = cookies.Values["ReceivedDateS"];
					//收文結束日期
					if (cookies.Values["ReceivedDateE"] != null) model.ReceivedDateE = cookies.Values["ReceivedDateE"];
					//發文字號
					if (cookies.Values["SendSeq"] != null) model.SendSeq = cookies.Values["SendSeq"];
					//統編
					if (cookies.Values["ObligorCompanyId"] != null) model.ObligorCompanyId = cookies.Values["ObligorCompanyId"];
					//分行別起
					if (cookies.Values["BranchIdS"] != null)
					{
						model.BranchIdS = cookies.Values["BranchIdS"];
					}
					//分行別迄
					if (cookies.Values["BranchIdE"] != null)
					{
						model.BranchIdE = cookies.Values["BranchIdE"];
					}
					ViewBag.CurrentPage = cookies.Values["CurrentPage"];
					ViewBag.isQuery = "1";
				}
			}

			//model.BranchIdS = model.BranchIdE = LogonUser.UnitForKeyIn;
			//*20150811新需求, 分行角色只能看自己分行資料且不能修改
			LdapEmployeeBiz empBiz = new LdapEmployeeBiz();
			IList<LDAPEmployee> list = empBiz.GetNoRoleEmployeeIdList();
			IList<LDAPEmployee> list2 = empBiz.GetNoRoleEmployeeIdList2();
			if ((list != null && list.Any(m => m.EmpId.Trim().ToUpper() == LogonUser.Account.Trim().ToUpper()))
				|| (list2 != null && list2.Any(m => m.EmpId.Trim().ToUpper() == LogonUser.Account.Trim().ToUpper())))
			{
				//* 屬於分行角色
				model.BranchIdS = model.BranchIdE = new LdapEmployeeBiz().GetBranchId();
				ViewBag.UnitRead = "1";
			}

			return View(model);
		}

		[HttpPost]
		public ActionResult _QueryResult(SeizureOldQuery model, int pageNum = 1, string strSortExpression = "ReceiptSeq", string strSortDirection = "asc")
		{
			#region Cookies
			HttpCookie modelCookie = new HttpCookie("QueryCookie");
			//收文開始序號
			modelCookie.Values.Add("ReceiptSeqS", model.ReceiptSeqS);
			//收文結束序號
			modelCookie.Values.Add("ReceiptSeqE", model.ReceiptSeqE);
			//收文開始日期
			modelCookie.Values.Add("ReceivedDateS", model.ReceivedDateS);
			//收文結束日期
			modelCookie.Values.Add("ReceivedDateE", model.ReceivedDateE);
			//發文字號
			modelCookie.Values.Add("SendSeq", model.SendSeq);
			//統編
			modelCookie.Values.Add("ObligorCompanyId", model.ObligorCompanyId);
			//分行別起
			modelCookie.Values.Add("BranchIdS", model.BranchIdS);
			//分行別訖
			modelCookie.Values.Add("BranchIdE", model.BranchIdE);
			modelCookie.Values.Add("CurrentPage", pageNum.ToString());
			Response.Cookies.Add(modelCookie);
			#endregion

			string UserId = LogonUser.Account;
			if (!string.IsNullOrEmpty(model.ReceivedDateS))
			{
				model.ReceivedDateS = UtlString.FormatDateTwStringToAd(model.ReceivedDateS);
			}
			if (!string.IsNullOrEmpty(model.ReceivedDateE))
			{
				model.ReceivedDateE = UtlString.FormatDateTwStringToAd(model.ReceivedDateE);
			}
			IList<SeizureOldQuery> list = SQBIZ.GetData(model, pageNum, strSortExpression, strSortDirection, UserId, ref where);
			var dtvm = new SeizureOldQueryViewModel()
			{
				SeizureOldQuery = model,
				SeizureOldQueryList = list,
			};

			SQBIZ.SaveAPLog(list.Select(x => x.ObligorCompanyId).ToArray());//APLog Redis ader 2022-01-01 - ADD

			//分頁相關設定
			dtvm.SeizureOldQuery.PageSize = SQBIZ.PageSize;
			dtvm.SeizureOldQuery.CurrentPage = SQBIZ.PageIndex;
			dtvm.SeizureOldQuery.TotalItemCount = SQBIZ.DataRecords;
			dtvm.SeizureOldQuery.SortExpression = strSortExpression;
			dtvm.SeizureOldQuery.SortDirection = strSortDirection;

			//收文序號
			dtvm.SeizureOldQuery.ReceiptSeq = model.ReceiptSeq;
			//收件日期
			dtvm.SeizureOldQuery.ReceivedDate = model.ReceivedDate;
			//分行別
			dtvm.SeizureOldQuery.BranchId = model.BranchId;
			//義(債)務人戶名
			dtvm.SeizureOldQuery.ObligorAccountName = model.ObligorAccountName;
			//義(債)務人統編
			dtvm.SeizureOldQuery.ObligorCompanyId = model.ObligorCompanyId;
			//案件處理狀態
			dtvm.SeizureOldQuery.CaseProcessStatus = model.CaseProcessStatus;
			//發文日
			dtvm.SeizureOldQuery.SendDate = model.SendDate;
			//發文字號
			dtvm.SeizureOldQuery.SendSeq = model.SendSeq;
			//結案備註
			dtvm.SeizureOldQuery.EndCaseRemark = model.EndCaseRemark;
			return PartialView("_QueryResult", dtvm);
		}

		public ActionResult Details(string ReceiptId)
		{
			ViewBag.ReceiptId = ReceiptId;
			SeizureOldQueryBIZ SQBIZ = new SeizureOldQueryBIZ();

			SeizureOldQueryViewModel viewmodel = new SeizureOldQueryViewModel()
			{
				SeizureOldDetails1List = SQBIZ.GetSeizureOldDetail1List(ReceiptId),
				SeizureOldDetails1_1List = SQBIZ.GetSeizureOldDetail1_1List(ReceiptId),
				SeizureOldDetails2_1List = SQBIZ.GetSeizureOldDetail2_1List(ReceiptId),
				SeizureOldDetails2_2List = SQBIZ.GetSeizureOldDetail2_2List(ReceiptId),
				SeizureOldDetails3List = SQBIZ.GetSeizureOldDetail3List(ReceiptId)
			};
			return PartialView("Details", viewmodel);
		}
	}
}