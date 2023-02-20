using System;
using System.Configuration;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.IO;
using System.Web;
using System.Web.Mvc;
using CTBC.CSFS.BussinessLogic;
using CTBC.CSFS.Filter;
using CTBC.CSFS.ViewModels;
using CTBC.CSFS.Models;
using CTBC.CSFS.Pattern;
using CTBC.CSFS.Resource;
using CTBC.FrameWork.Util;

namespace CTBC.CSFS.Areas.KeyInput.Controllers
{
    /// <summary>
    /// 建檔作業-扣押案件
    /// </summary>
    public class CaseGeneralController : AppController
    {

        [RootPageFilter]
        public ActionResult Index()
        {
            PARMCodeBIZ para = new PARMCodeBIZ();
            PARMWorkingDayBIZ workDate = new PARMWorkingDayBIZ();
            InitDropdownListOptions();
            LdapEmployeeBiz ldapEmploee = new LdapEmployeeBiz();
            List<LDAPEmployee> list = ldapEmploee.GetAllEmployeeInEmployeeView();
            ViewBag.Emploee = false;//判斷是否是部門的人
            foreach (var item in list)
            {
                if (item.EmpId == LogonUser.Account)
                {
                    ViewBag.Emploee = true;
                }
            }
            //*限辦日期
            string limitedate = string.Empty;
            DataTable dt = workDate.GetWorkDate();
            int chkdays = 0;
            if (dt != null && dt.Rows.Count > 0)
            {
                chkdays = Convert.ToInt32(para.GetCodeNoByCodeDesc("LIMITDATE2"));
                if (chkdays < dt.Rows.Count)
                {
                    limitedate = dt.Rows[chkdays]["workdate"].ToString();
                }
            }
            CaseSeizureViewModel model = new CaseSeizureViewModel()
            {
                CaseMaster = new CaseMaster
                {
                    LimitDate = limitedate,
                    Person = LogonUser.Account + " " + LogonUser.Name,
                    Unit = new LdapEmployeeBiz().GetBranchId(),
                    ReceiveDate = DateTime.Now.ToString("yyyy/MM/dd")
                },
                CaseObligorlistO = new List<CaseObligor>()
            };
            for (int i = 0; i < 10; i++)
            {
                model.CaseObligorlistO.Add(new CaseObligor());
            }
            return View(model);
        }

        /// <summary>
        /// 綁定來文機關的下拉菜單
        /// </summary>
        public void InitDropdownListOptions()
        {
            PARMCodeBIZ parm = new PARMCodeBIZ();
            ViewBag.CASE_END_TIME = parm.GetCASE_END_TIME("CASE_END_TIME");                                     //* 每天最晚時間
            ViewBag.GOV_KINDList = new SelectList(parm.GetCodeData("GOV_KIND"), "CodeDesc", "CodeDesc");        //* 來文機關類型
            ViewBag.SpeedList = new SelectList(parm.GetCodeData("INCOME_SPEED"), "CodeDesc", "CodeDesc");       //* 速別
            ViewBag.ReceiveKindList = new SelectList(parm.GetCodeData("INCOME_TYPE"), "CodeDesc", "CodeDesc");  //* 來文方式
            ViewBag.CaseKindList = new SelectList(parm.GetCodeData("CASE_KIND"), "CodeDesc", "CodeDesc", Lang.csfs_receive_case);       //* 類別-大類
            ViewBag.CaseKind2List = new SelectList(parm.GetCodeData("CASE_EXTERNAL"), "CodeDesc", "CodeDesc");   //* 類別-小類-扣押
        }

        ///// <summary>
        ///// 綁定來文機關
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
        /// 判斷來文字號是否重複
        /// </summary>
        /// <param name="txtGovNo"></param>
        /// <returns></returns>
        public ActionResult IsGovNoExist(string txtGovNo)
        {
            CaseMasterBIZ casemaster = new CaseMasterBIZ();
            //* 1-有重複 0-不重複
            return Json(casemaster.IsGovNoExist(txtGovNo) == "1" ? new JsonReturn { ReturnCode = "1", ReturnMsg = "" }
                                                                : new JsonReturn { ReturnCode = "0", ReturnMsg = "" });
        }

        /// <summary>
        /// 實際進行Create動作
        /// </summary>
        /// <param name="model"></param>
        /// <param name="fileAttNames"></param>
        /// <returns></returns>
        public ActionResult Create(CaseSeizureViewModel model, IEnumerable<HttpPostedFileBase> fileAttNames)
        {
            CaseMasterBIZ casemaster = new CaseMasterBIZ();
            model.CaseMaster.CaseKind = CaseKind.CASE_EXTERNAL;
            //model.CaseMaster.LimitDate = DateTime.Now.AddDays(Convert.ToInt32(new PARMCodeBIZ().GetCodeNoByCodeDesc("LIMITDATE2"))).ToString("yyyy/MM/dd");
            model.CaseMaster.Unit = new LdapEmployeeBiz().GetBranchId();
            model.CaseMaster.Status = CaseStatus.CaseInput;
            model.CaseMaster.CreatedUser = LogonUser.Account;
            model.CaseMaster.GovDate = UtlString.FormatDateTwStringToAd(model.CaseMaster.GovDate);
            model.CaseMaster.ReceiveDate = model.CaseMaster.ReceiveDate;
            model.CaseAttachmentlistO = new List<CaseAttachment>();

            #region  保存附件
            try
            {
                if (fileAttNames != null)
                {
                    foreach (var aModel in fileAttNames.Select(UploadFile).Where(aModel => aModel != null))
                    {
                        model.CaseAttachmentlistO.Add(aModel);
                    }
                }
            }
            catch (Exception ex)
            {
                return Content(@"<script>parent.showMessage('0','" + ex.Message + "');</script>");
            }
            #endregion

            #region 是否自動派件
            var scriptStr = "";
            bool result = false;
            AgentSettingBIZ ASBIZ = new AgentSettingBIZ();
            model.CaseMaster.IsAutoDispatchFS = ASBIZ.GetEnable().IsAutoDispatchFS;//AutoDispatchFS是否啟用
            if (model.CaseMaster.IsAutoDispatchFS)
            {
                result = casemaster.AutoModeCase(ref model);//自動派件
                scriptStr = result ? "parent.showMessage('1','" + string.Format(Lang.csfs_case_create_success2_0, model.CaseMaster.DocNo) + "');"
                                   : "parent.showMessage('0','" + Lang.csfs_add_fail + "');";
            }
            else
            {
                //非自動派件(原新增方法)
                scriptStr = casemaster.CreateCase(ref model) ? "parent.showMessage('1','" + string.Format(Lang.csfs_case_create_success2_0, model.CaseMaster.DocNo) + "');"
                                                             : "parent.showMessage('0','" + Lang.csfs_add_fail + "');";
            }
            //var scriptStr = casemaster.CreateCase(ref model) ? "parent.showMessage('1','" + string.Format(Lang.csfs_case_create_success2_0, model.CaseMaster.DocNo) + "');"
            //                                                 : "parent.showMessage('0','" + Lang.csfs_add_fail + "');";
            #endregion
            return Content(@"<script>" + scriptStr + "</script>");
        }

        /// <summary>
        /// 儲存上傳文件
        /// </summary>
        /// <param name="upFile"></param>
        /// <returns></returns>
        public CaseAttachment UploadFile(HttpPostedFileBase upFile)
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

            CaseAttachment aModel = new CaseAttachment
            {
                AttachmentName = Path.GetFileName(upFile.FileName),
                AttachmentServerName = newFileName,
                AttachmentServerPath = serverPath,
                isDelete = 0,
                CreatedUser = LogonUser.Account
            };
            return aModel;
        }
    }
}