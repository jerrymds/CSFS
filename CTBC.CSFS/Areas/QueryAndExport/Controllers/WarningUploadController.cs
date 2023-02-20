using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CTBC.CSFS.Pattern;
using CTBC.CSFS.ViewModels;
using CTBC.CSFS.Models;
using CTBC.CSFS.BussinessLogic;
using CTBC.FrameWork.Util;
using CTBC.CSFS.Resource;
using System.IO;
using System.Configuration;
using CTBC.CSFS.Filter;

namespace CTBC.CSFS.Areas.QueryAndExport.Controllers
{
    public class WarningUploadController : AppController
    {
        // GET: /QueryAndExport/WarningUpload/
        CaseWarningBIZ warn = new CaseWarningBIZ();
        WarningUploadBIZ wu = new WarningUploadBIZ();
        [RootPageFilter]
        public ActionResult Index()
        {
            return View();
        }

        public WarningAttachment UploadFile(HttpPostedFileBase upFile)
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

            WarningAttachment aModel = new WarningAttachment
            {
                AttachmentName = Path.GetFileName(upFile.FileName),
                AttachmentServerName = newFileName,
                AttachmentServerPath = serverPath,
                CreatedUser = LogonUser.Account
            };
            return aModel;
        }

        public ActionResult SaveAttatch()
        {
            WarningMaster model = new WarningMaster();
            CaseWarningViewModel viewmodel = new CaseWarningViewModel()
            {
                WarningMaster = model
            };
            return View(viewmodel);
        }

        [HttpPost]
        public ActionResult SaveAttatch(CaseWarningViewModel model, IEnumerable<HttpPostedFileBase> fileAttNames)
        {
            model.WarningAttachmentList = new List<WarningAttachment>();
            try
            {
                if (fileAttNames != null)
                {
                    foreach (var aModel in fileAttNames.Select(UploadFile).Where(aModel => aModel != null))
                    {
                        model.WarningAttachmentList.Add(aModel);
                    }
                }
            }
            catch (Exception ex)
            {

            }
            var scriptStr = wu.SaveAttatchment(ref model) ? "parent.showMessage('1','" + Lang.csfs_save_ok + "');"
                                                            : "parent.showMessage('0','" + Lang.csfs_no_mate_file + "');";
            return Content(@"<script>" + scriptStr + "</script>");
        }

        public ActionResult DeleteAttatch(string attachId) 
        {
            return Json(warn.DeleteAttatch(attachId) > 0 ? new JsonReturn { ReturnCode = "1", ReturnMsg = Lang.csfs_del_ok }
                                                  : new JsonReturn { ReturnCode = "0", ReturnMsg = Lang.csfs_del_fail });
        }
	}
}