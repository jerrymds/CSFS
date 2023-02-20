using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using CTBC.CSFS.Models;
using CTBC.CSFS.BussinessLogic;
using CTBC.FrameWork.Util;
using CTBC.CSFS.Pattern;
using System.Web.Mvc;
using CTBC.CSFS.Filter;
using CTBC.CSFS.ViewModels;
using System.Configuration;
using System.Collections;
using System.Text;


namespace CTBC.CSFS.Areas.Collection.Controllers
{
    public class eDocUploadController : AppController
    {
        DirectorToApproveBIZ _directorBiz;
        CaseHistoryBIZ historyBiz = new CaseHistoryBIZ();
        SendEDocBiz _SendEDocBiz;
        EmailGroupBiz _EmailGroupBiz;
	    	PARMCodeBIZ parm = new PARMCodeBIZ();
        public eDocUploadController()
        {
            _directorBiz = new DirectorToApproveBIZ(this);
            _SendEDocBiz = new SendEDocBiz();
            _EmailGroupBiz = new EmailGroupBiz(this);
        }
        [RootPageFilter]
        public ActionResult Index(string isBack)
        {
            DirectorToApprove model = new DirectorToApprove();
            //if (isBack == "1")
            //{
            //    HttpCookie cookies = Request.Cookies.Get("QueryCookie");
            //    if (cookies != null)
            //    {
            //        if (cookies.Values["CaseNo"] != null) model.CaseNo = cookies.Values["CaseNo"];
            //         //20170811 RC RQ-2015-019666-020 派件至跨單位 宏祥 update start
            //        //if (cookies.Values["AgentUser"] != null) model.AgentUser = cookies.Values["AgentUser"];
            //        if (cookies.Values["AgentDepartment"] != null) model.AgentDepartment = cookies.Values["AgentDepartment"];
            //        if (cookies.Values["AgentDepartment2"] != null) model.AgentDepartment2 = cookies.Values["AgentDepartment2"];
            //        if (cookies.Values["AgentDepartmentUser"] != null) model.AgentDepartmentUser = cookies.Values["AgentDepartmentUser"];
            //        //20170811 RC RQ-2015-019666-020 派件至跨單位 宏祥 update end
            //        ViewBag.CurrentPage = cookies.Values["CurrentPage"];
            //        ViewBag.isQuery = "1";
            //    }
            //}

			//var uploadCount = parm.GetCodeData("UploadCount").FirstOrDefault().CodeDesc;
			//ViewBag.UploadCount = uploadCount;

            //InitDropdownListOptions();
            return View(model);
        }
        //[HttpPost]
        //public ActionResult _QueryResult(DirectorToApprove model, int pageNum = 1, string strSortExpression = "CaseNo", string strSortDirection = "asc")
        //{
        //    #region Cookie
        //    HttpCookie modelCookie = new HttpCookie("QueryCookie");
        //    modelCookie.Values.Add("CaseKind", model.CaseKind);
        //    modelCookie.Values.Add("CaseKind2", model.CaseKind2);
        //    modelCookie.Values.Add("GovUnit", model.GovUnit);
        //    modelCookie.Values.Add("CaseNo", model.CaseNo);
        //    modelCookie.Values.Add("GovDateS", model.GovDateS);
        //    modelCookie.Values.Add("GovDateE", model.GovDateE);
        //    modelCookie.Values.Add("Speed", model.Speed);
        //    modelCookie.Values.Add("ReceiveKind", model.ReceiveKind);
        //    modelCookie.Values.Add("GovNo", model.GovNo);
        //    modelCookie.Values.Add("CreatedDateS", model.CreatedDateS);
        //    modelCookie.Values.Add("CreatedDateE", model.CreatedDateE);
        //    //20170811 RC RQ-2015-019666-020 派件至跨單位 宏祥 update start
        //    //modelCookie.Values.Add("AgentUser", model.AgentUser);
        //    modelCookie.Values.Add("AgentDepartment", model.AgentDepartment);
        //    modelCookie.Values.Add("AgentDepartment2", model.AgentDepartment2);
        //    modelCookie.Values.Add("AgentDepartmentUser", model.AgentDepartmentUser);
        //    //20170811 RC RQ-2015-019666-020 派件至跨單位 宏祥 update end
        //    modelCookie.Values.Add("CreateUser", model.CreateUser);
        //    modelCookie.Values.Add("Unit", model.Unit);
        //    modelCookie.Values.Add("SendDateS", model.SendDateS);
        //    modelCookie.Values.Add("SendDateE", model.SendDateE);
        //    modelCookie.Values.Add("CurrentPage", pageNum.ToString());
        //    Response.Cookies.Add(modelCookie);
        //    #endregion

        //    string UserId = LogonUser.Account;
        //    if (!string.IsNullOrEmpty(model.GovDateE))
        //    {
        //        model.GovDateE = UtlString.FormatDateTwStringToAd(model.GovDateE);
        //    }
        //    if (!string.IsNullOrEmpty(model.GovDateS))
        //    {
        //        model.GovDateS = UtlString.FormatDateTwStringToAd(model.GovDateS);
        //    }
        //    if (!string.IsNullOrEmpty(model.CreatedDateE))
        //    {
        //        model.CreatedDateE = UtlString.FormatDateTwStringToAd(model.CreatedDateE);
        //    }
        //    if (!string.IsNullOrEmpty(model.CreatedDateS))
        //    {
        //        model.CreatedDateS = UtlString.FormatDateTwStringToAd(model.CreatedDateS);
        //    }

        //    if (!string.IsNullOrEmpty(model.SendDateS))
        //    {
        //        model.SendDateS = UtlString.FormatDateTwStringToAd(model.SendDateS);
        //    }
        //    if (!string.IsNullOrEmpty(model.SendDateE))
        //    {
        //        model.SendDateE = UtlString.FormatDateTwStringToAd(model.SendDateE);
        //    }
        //    IList<DirectorToApprove> list = _directorBiz.GeteDocUpload(model, pageNum, strSortExpression, strSortDirection, UserId);
        //    var dtvm = new DirectorToApproveViewModel()
        //    {
        //        DirectorToApprove = model,
        //        DirectorToApprovelist = list,
        //    };
        //    //分頁相關設定
        //    dtvm.DirectorToApprove.PageSize = _directorBiz.PageSize;
        //    dtvm.DirectorToApprove.CurrentPage = _directorBiz.PageIndex;
        //    dtvm.DirectorToApprove.TotalItemCount = _directorBiz.DataRecords;
        //    dtvm.DirectorToApprove.SortExpression = strSortExpression;
        //    dtvm.DirectorToApprove.SortDirection = strSortDirection;

        //    dtvm.DirectorToApprove.CaseNo = model.CaseNo;
        //    dtvm.DirectorToApprove.GovUnit = model.GovUnit;
        //    dtvm.DirectorToApprove.GovDate = model.GovDate;
        //    dtvm.DirectorToApprove.GovNo = model.GovNo;
        //    dtvm.DirectorToApprove.Person = model.Person;
        //    dtvm.DirectorToApprove.CaseKind = model.CaseKind;
        //    dtvm.DirectorToApprove.CaseKind2 = model.CaseKind2;
        //    dtvm.DirectorToApprove.Speed = model.Speed;
        //    dtvm.DirectorToApprove.LimitDate = model.LimitDate;
            
        //    return PartialView("_QueryResult", dtvm);
        //}
        //public void InitDropdownListOptions()
        //{
        //    PARMCodeBIZ parm = new PARMCodeBIZ();
        //    ViewBag.CASE_END_TIME = parm.GetCASE_END_TIME("CASE_END_TIME");                                     //* 每天最晚時間
        //    ViewBag.GOV_KINDList = new SelectList(parm.GetCodeData("GOV_KIND"), "CodeDesc", "CodeDesc");        //* 來文機關類型
        //    ViewBag.SpeedList = new SelectList(parm.GetCodeData("INCOME_SPEED"), "CodeDesc", "CodeDesc");       //* 速別
        //    ViewBag.ReceiveKindList = new SelectList(parm.GetCodeData("INCOME_TYPE"), "CodeDesc", "CodeDesc");  //* 來文方式
        //    ViewBag.CaseKindList = new SelectList(parm.GetCodeData("CASE_KIND"), "CodeDesc", "CodeDesc");
        //    ViewBag.CaseKind2List = new SelectList(parm.GetCodeData("CASE_SEIZURE"), "CodeDesc", "CodeDesc");
        //    ViewBag.AgentDepartmentList = new SelectList(parm.GetCodeData("CollectionToAgent_AgentDDLDepartment"), "CodeNo", "CodeDesc");
        //    ViewBag.AgentDepartment2List = new SelectList(new AgentSettingBIZ().GetAgentDepartment2View(), "SectionName", "SectionName");
        //    ViewBag.AgentDepartmentUserList = new SelectList(new AgentSettingBIZ().GetAgentDepartmentUserView(), "EmpID", "EmpIdAndName");
        //    //20170811 RC RQ-2015-019666-020 派件至跨單位 宏祥 update end
        //}

        public ActionResult Upload(string caseIdarr)
        {
            string flag = "0";
            System.Data.DataTable  dt = new System.Data.DataTable();
             try
            {

                if (caseIdarr.Length > 5)
                {
                    bool ret = _SendEDocBiz.UpdateGssDoc(caseIdarr);
                    if (ret == true)
                    {
                        dt = _SendEDocBiz.GetGssDocCaseID(caseIdarr);

                        if (dt != null)
                        {
                            for (int i = 0; i < dt.Rows.Count; i++)
                            {
                                if (dt.Rows[i]["CaseId"] != null)
                                {
                                    historyBiz.insertCaseHistory2((Guid)dt.Rows[i]["CaseId"], "R01", LogonUser.Account, null);
                                }
                                flag = "2";
                            }
                        }
                        else
                        {
                            flag = "3";
                        }
                    }
                    else
                    {
                        flag = "3";
                    }
                }  

                return Json(new JsonReturn() { ReturnCode = flag });   
            }
            catch (Exception ex)
            {
                flag = "1";
                return Json(new JsonReturn() { ReturnCode = flag });  
            }
            
            //return Json(_directorBiz.UpdateBatchControlUp(caseIdList, userId));
        } 
	}
}