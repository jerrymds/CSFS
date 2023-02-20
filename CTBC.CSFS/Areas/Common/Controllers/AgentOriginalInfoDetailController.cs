using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.IO;
using System.Web;
using System.Web.Mvc;
using CTBC.CSFS.Models;
using CTBC.CSFS.ViewModels;
using CTBC.CSFS.BussinessLogic;
using CTBC.CSFS.Resource;
using CTBC.CSFS.Pattern;
using CTBC.FrameWork.Util;

namespace CTBC.CSFS.Areas.Common.Controllers
{
    public class AgentOriginalInfoDetailController : AppController
    {
        LendAttachmentBIZ lendAttatch = new LendAttachmentBIZ();
        CaseObligorBIZ obligor = new CaseObligorBIZ();
        LendDataBIZ lendData;

        public AgentOriginalInfoDetailController()
        {
            lendData = new LendDataBIZ(this);
        }

        // GET: Agent/AgentOriginalInfo
        public ActionResult Index(Guid caseId, string FromControl)
        {
            ViewBag.CaseId = caseId;
            ViewBag.FromControl = FromControl;
            return View();
        }

        #region 新增正本調閱
        public ActionResult Create(Guid CaseID)
        {
            LendData lendmodel = new LendData();
            CaseObligor obligorModel = obligor.ObligorModelInfo(CaseID);
            lendmodel.CaseId = CaseID;
            lendmodel.ClientID = obligorModel.ObligorNo;
            lendmodel.Name = obligorModel.ObligorName;
            AgentOriginalInfoViewModel model = new AgentOriginalInfoViewModel()
            {
                LendDataInfo = lendmodel,
            };
            return PartialView("Create", model);
        }

        [HttpPost]
        public ActionResult Create(AgentOriginalInfoViewModel model, IEnumerable<HttpPostedFileBase> fileAttNames)
        {
            model.LendAttachmentInfoList = new List<LendAttachment>();
            #region  保存附件
            try
            {
                if (fileAttNames != null)
                {
                    foreach (var aModel in fileAttNames.Select(UploadFile).Where(aModel => aModel != null))
                    {
                        model.LendAttachmentInfoList.Add(aModel);
                    }
                }
            }
            catch (Exception ex)
            {
                return Content(@"<script>parent.showMessage('0','" + ex.Message + "');</script>");
            }
            #endregion

            var scriptStr = lendData.CreateLend(model) ? "parent.jAlertSuccess('" + Lang.csfs_add_ok + "', function () {parent.$.colorbox.close();parent.$.AgentOriginalInfo.GetQueryResult();})"
                                                           : "parent.jAlertSuccess('" + Lang.csfs_add_fail + "',function () {parent.$.colorbox.close();parent.$.AgentOriginalInfo.GetQueryResult();});";
            return Content(@"<script>" + scriptStr + "</script>");
        }

        /// <summary>
        /// 儲存上傳文件
        /// </summary>
        /// <param name="upFile"></param>
        /// <returns></returns>
        public LendAttachment UploadFile(HttpPostedFileBase upFile)
        {
            if (upFile == null || upFile.ContentLength <= 0) return null;

            //获取用户上传文件的后缀名,重命名為當前登入者ID+年月日時分秒毫秒
            string newFileName = LogonUser.Account + "_" + DateTime.Now.ToString("yyyyMMddhhmmssfff") + Path.GetExtension(upFile.FileName);

            string serverPath = Path.Combine("~/", ConfigurationManager.AppSettings["UploadFolder"], DateTime.Today.ToString("yyyyMM"));
            string realPath = Server.MapPath(serverPath);
            //*月份文件夾不存在則新增
            if (!FrameWork.Util.UtlFileSystem.FolderIsExist(realPath))
                FrameWork.Util.UtlFileSystem.CreateFolder(realPath);

            //利用file.SaveAs保存图片
            string name = Path.Combine(realPath, newFileName);
            upFile.SaveAs(name);

            LendAttachment aModel = new LendAttachment
            {
                LendAttachName = Path.GetFileName(upFile.FileName),
                LendAttachServerPath = newFileName,
                LendAttachServerName = serverPath,
                isDelete = 0,
                CreatedUser = LogonUser.Account
            };
            return aModel;
        }
        #endregion

        #region 查詢正本調閱結果集
        /// <summary>
        /// 結果列表
        /// </summary>
        /// <param name="AToH"></param>
        /// <param name="pageNum"></param>
        /// <param name="strSortExpression"></param>
        /// <param name="strSortDirection"></param>
        /// <returns></returns>
        public ActionResult _QueryResult(AgentOriginalInfoViewModel model, int pageNum = 1, string strSortExpression = "LendID", string strSortDirection = "asc")
        {
            //APLog Redis ader 2022-07-07 - START
            //return PartialView("_QueryResult", SearchList(model, pageNum, strSortExpression, strSortDirection));
            var vm = SearchList(model, pageNum, strSortExpression, strSortDirection);
            if (vm.LendDataInfoList != null && vm.LendDataInfoList.Count > 0)
            {
                lendData.SaveAPLog(vm.LendDataInfoList.Select(x => x.ClientID).ToArray());
            }
            return PartialView("_QueryResult", vm);
            //APLog Redis ader 2022-07-07 - END
        }

        /// <summary>
        /// 實際查詢動作
        /// </summary>
        /// <param name="AToH"></param>
        /// <param name="pageNum"></param>
        /// <param name="strSortExpression"></param>
        /// <param name="strSortDirection"></param>
        /// <returns></returns>
        public AgentOriginalInfoViewModel SearchList(AgentOriginalInfoViewModel model, int pageNum = 1, string strSortExpression = "LendID", string strSortDirection = "asc")
        {
            string LendId="";
            // 新增個資LOG
            string CustId = "";
            string _user = (HttpContext.Session["UserAccount"] != null) ? HttpContext.Session["UserAccount"].ToString() : "";
            System.Collections.Specialized.NameValueCollection _parameters = new System.Collections.Specialized.NameValueCollection();
            //System.Collections.Specialized.NameValueCollection _parameters = ("POST".Equals(HttpContext.Request.HttpMethod)) ? HttpContext.Request.Form : HttpContext.Request.QueryString;
            string _controller = (this.ControllerContext.RouteData.Values["Controller"] != null) ? this.ControllerContext.RouteData.Values["Controller"].ToString() : "";
            string _action = (this.ControllerContext.RouteData.Values["Action"] != null) ? this.ControllerContext.RouteData.Values["Action"].ToString() : "";
            string _ip = (HttpContext.Request != null && !string.IsNullOrEmpty(HttpContext.Request.ServerVariables["REMOTE_ADDR"])) ? HttpContext.Request.ServerVariables["REMOTE_ADDR"] : "";
            BaseBusinessRule _business = new BaseBusinessRule();
            IList<LendData> result = lendData.GetQueryList(model, pageNum, strSortExpression, strSortDirection);
            if (result.Count > 0)
            {
                for (int i = 0; i < result.Count; i++)
                {
                    if (!String.IsNullOrEmpty(result[i].ClientID))
                    { 
                        CustId = result[i].ClientID.Trim();
                        _business.ExecuteAPLOGSave(_user, _controller, _action, _ip, "CASEID=" + result[i].CaseId.ToString(), CustId);
                    }
                }
                LendId = result.First().LendID.ToString();
                var viewModel = new AgentOriginalInfoViewModel()
                {
                    LendDataInfo = model.LendDataInfo,
                    LendDataInfoList = result,
                    LendAttachmentInfoList = lendAttatch.getAttachList(LendId),
                };
                return viewModel;
            }
            else
             {
                 var viewModel = new AgentOriginalInfoViewModel()
                 {
                     LendDataInfo = model.LendDataInfo,
                     LendDataInfoList = result,
                     LendAttachmentInfoList = model.LendAttachmentInfoList,
                 };                
                 return viewModel;
            };

            
        }
        #endregion

        #region 刪除正本調閱
        //* 刪除正本調閱
        public ActionResult DeleteLend(string LendId)
        {
            return Content(lendData.DeleteLendData(LendId) > 0 ? "true" : "false");
        }
        #endregion

        #region 修改正本調閱
        public ActionResult Edit(string LendId)
        {
            AgentOriginalInfoViewModel model = new AgentOriginalInfoViewModel()
            {
                LendDataInfo = lendData.getModel(LendId),
                LendAttachmentInfoList = lendAttatch.getAttachList(LendId)
            };
            return PartialView("Edit", model);
        }

        [HttpPost]
        public ActionResult Edit(AgentOriginalInfoViewModel model, IEnumerable<HttpPostedFileBase> fileAttNames)
        {
            model.LendAttachmentInfoList = new List<LendAttachment>();
            #region  保存附件
            try
            {
                if (fileAttNames != null)
                {
                    foreach (var aModel in fileAttNames.Select(UploadFile).Where(aModel => aModel != null))
                    {
                        model.LendAttachmentInfoList.Add(aModel);
                    }
                }
            }
            catch (Exception ex)
            {
                return Content(@"<script>parent.showMessage('0','" + ex.Message + "');</script>");
            }
            #endregion

            var scriptStr = lendData.UpdateLend(model)  ? "parent.jAlertSuccess('" + Lang.csfs_edit_ok + "', function () {parent.$.colorbox.close();parent.$.AgentOriginalInfo.GetQueryResult();})"
                                                           : "parent.jAlertSuccess('" + Lang.csfs_edit_fail + "',function () {parent.$.colorbox.close();parent.$.AgentOriginalInfo.GetQueryResult();});";

            return Content(@"<script>" + scriptStr + "</script>");
        }
        #endregion

        // GET: Agent/AgentOriginalInfo
        public ActionResult LendIndex(Guid caseId)
        {
            ViewBag.CaseId = caseId;
            ViewBag.Status = LendStatus.LendStatusLendSetting;
            return View();
        }

        #region 查詢正本歸還
        /// <summary>
        /// 查詢本案件正本歸還
        /// </summary>
        /// <param name="AToH"></param>
        /// <param name="pageNum"></param>
        /// <param name="strSortExpression"></param>
        /// <param name="strSortDirection"></param>
        /// <returns></returns>
        public ActionResult _LendCaseQueryResult(AgentOriginalInfoViewModel model, int pageNum = 1, string strSortExpression = "LendID", string strSortDirection = "asc")
        {
            return PartialView("_LendQueryResult", LendCaseSearchList(model, pageNum, strSortExpression, strSortDirection));
        }

        /// <summary>
        /// 實際查詢本案件正本歸還動作
        /// </summary>
        /// <param name="AToH"></param>
        /// <param name="pageNum"></param>
        /// <param name="strSortExpression"></param>
        /// <param name="strSortDirection"></param>
        /// <returns></returns>
        public AgentOriginalInfoViewModel LendCaseSearchList(AgentOriginalInfoViewModel model, int pageNum = 1, string strSortExpression = "LendID", string strSortDirection = "asc")
        {
            // 新增個資LOG
            string CustId = "";
            string _user = (HttpContext.Session["UserAccount"] != null) ? HttpContext.Session["UserAccount"].ToString() : "";
            System.Collections.Specialized.NameValueCollection _parameters = new System.Collections.Specialized.NameValueCollection();
            //System.Collections.Specialized.NameValueCollection _parameters = ("POST".Equals(HttpContext.Request.HttpMethod)) ? HttpContext.Request.Form : HttpContext.Request.QueryString;
            string _controller = (this.ControllerContext.RouteData.Values["Controller"] != null) ? this.ControllerContext.RouteData.Values["Controller"].ToString() : "";
            string _action = (this.ControllerContext.RouteData.Values["Action"] != null) ? this.ControllerContext.RouteData.Values["Action"].ToString() : "";
            string _ip = (HttpContext.Request != null && !string.IsNullOrEmpty(HttpContext.Request.ServerVariables["REMOTE_ADDR"])) ? HttpContext.Request.ServerVariables["REMOTE_ADDR"] : "";
            BaseBusinessRule _business = new BaseBusinessRule();
            IList<LendData> result = lendData.GetLendCaseQueryList(model, pageNum, strSortExpression, strSortDirection);

            if (result.Count > 0)
            {
                for (int i = 0; i < result.Count; i++)
                {
                    if (!String.IsNullOrEmpty(result[i].ClientID))
                    {
                        CustId = result[i].ClientID;
                        _business.ExecuteAPLOGSave(_user, _controller, _action, _ip, "CASEID=" + result[i].CaseId.ToString(), CustId);
                    }
                }
            }
                var viewModel = new AgentOriginalInfoViewModel()
            {
                LendDataInfo = model.LendDataInfo,
                LendDataInfoList = result,
            };

            return viewModel;
        }

        /// <summary>
        /// 查詢所有案件未歸還資料
        /// </summary>
        /// <param name="model"></param>
        /// <param name="nowCaseId"></param>
        /// <param name="pageNum"></param>
        /// <param name="strSortExpression"></param>
        /// <param name="strSortDirection"></param>
        /// <returns></returns>
        public ActionResult _LendQueryResult(AgentOriginalInfoViewModel model, Guid nowCaseId, int pageNum = 1, string strSortExpression = "LendID", string strSortDirection = "asc")
        {
            ViewBag.NowCaseId = nowCaseId;
            return PartialView("_LendQueryResult", LendSearchList(model, pageNum, strSortExpression, strSortDirection));
        }

        /// <summary>
        /// 實際查詢所有案件未歸還資料動作
        /// </summary>
        /// <param name="AToH"></param>
        /// <param name="pageNum"></param>
        /// <param name="strSortExpression"></param>
        /// <param name="strSortDirection"></param>
        /// <returns></returns>
        public AgentOriginalInfoViewModel LendSearchList(AgentOriginalInfoViewModel model, int pageNum = 1, string strSortExpression = "LendID", string strSortDirection = "asc")
        {
            IList<LendData> result = lendData.GetLendQueryList(model, pageNum, strSortExpression, strSortDirection);
            var viewModel = new AgentOriginalInfoViewModel()
            {
                LendDataInfo = model.LendDataInfo,
                LendDataInfoList = result,
            };

            return viewModel;
        }
       
        #endregion

        #region 修改正本歸還
        public ActionResult LendEdit(string LendId, Guid nowCaseId)
        {
            LendData model = new LendData();
            model = lendData.getLendModel(LendId);
            model.ReturnCaseId = nowCaseId;
            model.ReturnBankDate =UtlString.FormatDateTw(DateTime.Now.ToString("yyyy/MM/dd"));
            model.ReturnDate = UtlString.FormatDateTw(DateTime.Now.ToString("yyyy/MM/dd"));
            return PartialView("LendEdit", model);
        }

        [HttpPost]
        public ActionResult LendEdit(LendData model)
        {
            model.LendStatus = LendStatus.LendStatusLendSetting;    //* 修改狀態為正本歸還
            //model.ReturnCaseId = model.CaseId;                                 //* 修改歸還CaseId
            model.ReturnBankDate = UtlString.FormatDateTwStringToAd(model.ReturnBankDate);
            model.ReturnDate = UtlString.FormatDateTwStringToAd(model.ReturnDate);
            var scriptStr = lendData.UpdateLend(model) > 0 ? "parent.jAlertSuccess('" + Lang.csfs_edit_ok + "', function () {parent.$.colorbox.close();parent.$.AgentOriginalInfo.GetQueryResultLend();parent.$.AgentOriginalInfo.GetQueryResultLendCase();})"
                                                            : "parent.jAlertSuccess('" + Lang.csfs_edit_fail + "',function () {parent.$.colorbox.close();parent.$.AgentOriginalInfo.GetQueryResultLend();parent.$.AgentOriginalInfo.GetQueryResultLendCase();});";

            return Content(@"<script>" + scriptStr + "</script>");
        }
        #endregion

        #region 更改正本歸還狀態(1.狀態改為調閱中，2.returncaseid清空)
        public ActionResult Delete(string LendId)
        {
            return Content(lendData.UpdateLendStatus(LendId) > 0 ? "true" : "false");
        }
        #endregion
    }
}