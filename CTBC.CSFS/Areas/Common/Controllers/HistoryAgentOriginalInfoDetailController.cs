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
    public class HistoryAgentOriginalInfoDetailController : AppController
    {
        HistoryLendAttachmentBIZ lendAttatch = new HistoryLendAttachmentBIZ();
        HistoryCaseObligorBIZ obligor = new HistoryCaseObligorBIZ();
        HistoryLendDataBIZ lendData;

        public HistoryAgentOriginalInfoDetailController()
        {
            lendData = new HistoryLendDataBIZ(this);
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
            HistoryLendData lendmodel = new HistoryLendData();
            HistoryCaseObligor obligorModel = obligor.ObligorModelInfo(CaseID);
            lendmodel.CaseId = CaseID;
            lendmodel.ClientID = obligorModel.ObligorNo;
            lendmodel.Name = obligorModel.ObligorName;
            HistoryAgentOriginalInfoViewModel model = new HistoryAgentOriginalInfoViewModel()
            {
                LendDataInfo = lendmodel,
            };
            return PartialView("Create", model);
        }

        [HttpPost]
        public ActionResult Create(HistoryAgentOriginalInfoViewModel model, IEnumerable<HttpPostedFileBase> fileAttNames)
        {
            model.LendAttachmentInfoList = new List<HistoryLendAttachment>();
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
        public HistoryLendAttachment UploadFile(HttpPostedFileBase upFile)
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

            HistoryLendAttachment aModel = new HistoryLendAttachment
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
        public ActionResult _QueryResult(HistoryAgentOriginalInfoViewModel model, int pageNum = 1, string strSortExpression = "LendID", string strSortDirection = "asc")
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
        public HistoryAgentOriginalInfoViewModel SearchList(HistoryAgentOriginalInfoViewModel model, int pageNum = 1, string strSortExpression = "LendID", string strSortDirection = "asc")
        {
            string LendId="";
            IList<HistoryLendData> result =  lendData.GetQueryList(model, pageNum, strSortExpression, strSortDirection);
            if (result.Count > 0)
            {
              LendId = result.First().LendID.ToString();
              var viewModel = new HistoryAgentOriginalInfoViewModel()
                {
                    LendDataInfo = model.LendDataInfo,
                    LendDataInfoList = result,
                    LendAttachmentInfoList = lendAttatch.getAttachList(LendId),
                };
              return viewModel;
            }
            else
             {
                 var viewModel = new HistoryAgentOriginalInfoViewModel()
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
            HistoryAgentOriginalInfoViewModel model = new HistoryAgentOriginalInfoViewModel()
            {
                LendDataInfo = lendData.getModel(LendId),
                LendAttachmentInfoList = lendAttatch.getAttachList(LendId)
            };
            return PartialView("Edit", model);
        }

        [HttpPost]
        public ActionResult Edit(HistoryAgentOriginalInfoViewModel model, IEnumerable<HttpPostedFileBase> fileAttNames)
        {
            model.LendAttachmentInfoList = new List<HistoryLendAttachment>();
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
        public ActionResult _LendCaseQueryResult(HistoryAgentOriginalInfoViewModel model, int pageNum = 1, string strSortExpression = "LendID", string strSortDirection = "asc")
        {
            //APLog Redis ader 2022-07-07 - START
            //return PartialView("_LendQueryResult", LendCaseSearchList(model, pageNum, strSortExpression, strSortDirection));
            var vm = LendCaseSearchList(model, pageNum, strSortExpression, strSortDirection);
            if (vm.LendDataInfoList != null && vm.LendDataInfoList.Count > 0)
            {
                lendData.SaveAPLog(vm.LendDataInfoList.Select(x => x.ClientID).ToArray());
            }
            return PartialView("_LendQueryResult", vm);
            //APLog Redis ader 2022-07-07 - END
        }

        /// <summary>
        /// 實際查詢本案件正本歸還動作
        /// </summary>
        /// <param name="AToH"></param>
        /// <param name="pageNum"></param>
        /// <param name="strSortExpression"></param>
        /// <param name="strSortDirection"></param>
        /// <returns></returns>
        public HistoryAgentOriginalInfoViewModel LendCaseSearchList(HistoryAgentOriginalInfoViewModel model, int pageNum = 1, string strSortExpression = "LendID", string strSortDirection = "asc")
        {
            IList<HistoryLendData> result = lendData.GetLendCaseQueryList(model, pageNum, strSortExpression, strSortDirection);

            var viewModel = new HistoryAgentOriginalInfoViewModel()
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
        public ActionResult _LendQueryResult(HistoryAgentOriginalInfoViewModel model, Guid nowCaseId, int pageNum = 1, string strSortExpression = "LendID", string strSortDirection = "asc")
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
        public HistoryAgentOriginalInfoViewModel LendSearchList(HistoryAgentOriginalInfoViewModel model, int pageNum = 1, string strSortExpression = "LendID", string strSortDirection = "asc")
        {
            IList<HistoryLendData> result = lendData.GetLendQueryList(model, pageNum, strSortExpression, strSortDirection);
            var viewModel = new HistoryAgentOriginalInfoViewModel()
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
            HistoryLendData model = new HistoryLendData();
            model = lendData.getLendModel(LendId);
            model.ReturnCaseId = nowCaseId;
            model.ReturnBankDate =UtlString.FormatDateTw(DateTime.Now.ToString("yyyy/MM/dd"));
            model.ReturnDate = UtlString.FormatDateTw(DateTime.Now.ToString("yyyy/MM/dd"));
            return PartialView("LendEdit", model);
        }

        [HttpPost]
        public ActionResult LendEdit(HistoryLendData model)
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