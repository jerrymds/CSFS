using CTBC.CSFS.Pattern;
using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Web;
using System.Web.Mvc;
using CTBC.CSFS.ViewModels;
using CTBC.CSFS.Models;
using CTBC.CSFS.BussinessLogic;
using CTBC.CSFS.Filter;
using CTBC.FrameWork.Util;
using CTBC.CSFS.Resource;

namespace CTBC.CSFS.Areas.QueryAndExport.Controllers
{
    public class MeetingResultUploadController : AppController
    {
        MeetingResultBIZ mResult = new MeetingResultBIZ();
        MeetingResultDetailBIZ dResult = new MeetingResultDetailBIZ();

        // GET: QueryAndExport/MeetingResultUpload
        [RootPageFilter]
        public ActionResult Index(string date)
        {
            if (string.IsNullOrEmpty(date))
            {
                date = DateTime.Now.ToString("yyyy/MM/dd");//初始化時顯示為當天日期
            }
            else
            {
                date = UtlString.FormatDateTwStringToAd(date);//改變日期時為選中日期
            }
            MeetingResult rmodel = mResult.GetMeetingResultInfo(date);//根據日期獲取相應附件資料
            if (rmodel.ResultDate != null)//DB中有當天相應資料
            {
                rmodel.ResultDate = UtlString.FormatDateTw(rmodel.ResultDate);                
            }
            else
            {
                rmodel.ResultDate = UtlString.FormatDateTw(date);//DB中無當天相應資料，則顯示為當天日期或者為更改的日期
            }
            if (rmodel.ResultCompleteDate!=null)
            {
                rmodel.ResultCompleteDate = UtlString.FormatDateTw(rmodel.ResultCompleteDate);
            }
            ViewBag.date = rmodel.ResultDate;
            List<MeetingResultDetail> list = dResult.GetMeetingResultDetailInfo(rmodel.ResultId);
            MeetingResultViewModel model = new MeetingResultViewModel()
            {
                MeetingResult = rmodel,
                MeetingResultDetailList = list
            };
            return View(model);
        }


        public ActionResult Create(MeetingResultViewModel model, IEnumerable<HttpPostedFileBase> fileAttNames)
        {
            if (model.MeetingResult.ResultDate !=null&&model.MeetingResult.ResultDate != "")
            {
                model.MeetingResult.ResultDate = UtlString.FormatDateTwStringToAd(model.MeetingResult.ResultDate);
            }

            if (model.MeetingResult.ResultCompleteDate!=null&&model.MeetingResult.ResultCompleteDate.Trim() != "")
            {
                model.MeetingResult.ResultCompleteDate = UtlString.FormatDateTwStringToAd(model.MeetingResult.ResultCompleteDate);
            }
           
            model.MeetingResult.CreatedUser = LogonUser.Account;
            model.MeetingResult.CreatedDate = DateTime.Now.ToString("yyyy/MM/dd");

            model.MeetingResultDetailList = new List<MeetingResultDetail>();
            #region  保存附件
            try
            {
                if (fileAttNames != null)
                {
                    foreach (var aModel in fileAttNames.Select(UploadFile).Where(aModel => aModel != null))
                    {
                        model.MeetingResultDetailList.Add(aModel);
                    }
                }
            }
            catch (Exception ex)
            {
                return Content(@"<script>parent.showMessage('0','" + ex.Message + "');</script>");
            }
            #endregion
            var scriptStr = mResult.Create(model) ? "parent.showMessage('1','"+Lang.csfs_add_ok+"');"
                                                           : "parent.showMessage('0','"+Lang.csfs_add_fail+"');";
            return Content(@"<script>" + scriptStr + "</script>");
        }

        /// <summary>
        /// 儲存上傳文件
        /// </summary>
        /// <param name="upFile"></param>
        /// <returns></returns>
        public MeetingResultDetail UploadFile(HttpPostedFileBase upFile)
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

            MeetingResultDetail aModel = new MeetingResultDetail
            {
                AttatchDetailName = Path.GetFileName(upFile.FileName),
                AttatchDetailServerName = newFileName,
                AttatchDetailServerPath = serverPath,
                isDelete = 0,
                CreatedUser = LogonUser.Account
            };
            return aModel;
        }

        /// <summary>
        /// 刪除一筆附件資料
        /// </summary>
        /// <param name="attachId"></param>
        /// <returns></returns>
        public ActionResult DeleteAttatch(string attachId)
        {
            return Json(dResult.DeleteAttatch(attachId) > 0 ? new JsonReturn { ReturnCode = "1", ReturnMsg = Lang.csfs_del_ok }
                                                    : new JsonReturn { ReturnCode = "0", ReturnMsg = Lang.csfs_del_fail });
        }
    }
}