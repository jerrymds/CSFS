using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CTBC.CSFS.Resource;
using CTBC.CSFS.Models;
using CTBC.CSFS.BussinessLogic;
using CTBC.CSFS.ViewModels;
using System.IO;
using CTBC.CSFS.Pattern;
using Newtonsoft.Json;
using CTBC.FrameWork.Util;

namespace CTBC.CSFS.Areas.SystemManagement.Controllers
{
    public class ParmCodeController : AppController
    {
        PARMCodeBIZ _parmCodeBiz = new PARMCodeBIZ();
        AgentSettingBIZ ASBIZ = new AgentSettingBIZ();
        //
        // GET: /SystemManagement/ParmCode/
        public ActionResult Index()
        {
            BindDdl();//CodeType下拉列表
            BindCodeEnable();
            BindDdlByCodeNo();//參數細項下拉菜單
            return View();
        }

        public ActionResult Create()
        {
            List<SelectListItem> list = new List<SelectListItem>(){};
            list.AddRange(new SelectList(_parmCodeBiz.GetAllCodeTypeList(), "CodeType", "CodeTypeDesc"));
            List<SelectListItem> listItem = new List<SelectListItem>()
            {
                new SelectListItem(){Text=Lang.csfs_others,Value="other"}
            };
            list.AddRange(listItem);
            ViewBag.codeTypeList = list;
            return View();
        }

        [HttpPost]
        public ActionResult Create(PARMCode model)
        {
            int count = _parmCodeBiz.Count(model.CodeNo, model.CodeType);//判斷代碼是否重複
            if (count > 0) { return Json(new JsonReturn() { ReturnCode = "2", ReturnMsg = "" }); }
            else
            {
                return Json(_parmCodeBiz.Create(model) ? new JsonReturn() { ReturnCode = "1", ReturnMsg = "" }
                                                 : new JsonReturn() { ReturnCode = "0", ReturnMsg = Lang.csfs_add_fail });
            }
        }

        /// <summary>
        /// 結果列表
        /// </summary>
        public ActionResult _QueryResult(PARMCode model, int pageNum = 1, string strSortExpression = "CodeType", string strSortDirection = "asc")
        {
            return PartialView("_QueryResult", SearchList(model, pageNum, strSortExpression, strSortDirection));
        }

        /// <summary>
        /// 實際查詢動作
        /// </summary>
        public PARMMenuViewModel SearchList(PARMCode model, int pageNum = 1, string strSortExpression = "CodeType", string strSortDirection = "asc")
        {
            IList<PARMCode> result = _parmCodeBiz.GetQueryList(model, pageNum, strSortExpression, strSortDirection);

            var viewModel = new PARMMenuViewModel()
            {
                PARMCode = model,
                PARMCodeList = result,
            };

            viewModel.PARMCode.PageSize = _parmCodeBiz.PageSize;
            viewModel.PARMCode.CurrentPage = _parmCodeBiz.PageIndex;
            viewModel.PARMCode.TotalItemCount = _parmCodeBiz.DataRecords;
            viewModel.PARMCode.SortExpression = strSortExpression;
            viewModel.PARMCode.SortDirection = strSortDirection;

            return viewModel;
        }

        /// 取得eTabsQueryStaff編輯PARMCode
        public ActionResult CheckUser(string id)
        {
            PARMCode model = new PARMCode { CodeUid = id };
            return PartialView("CheckUser", model);
        }

        [HttpPost]
        public ActionResult CheckUser(PARMCode model)
        {
            string id = model.CodeUid;
            string codeMemo = UtlString.EncodeBase64(model.CodeMemo);
            PARMCode Newmodel = new PARMCode { CodeUid = id, CodeMemo = codeMemo };
            bool updateResult = _parmCodeBiz.UpdateCodeMemo(Newmodel);
            return Json(updateResult ? new JsonReturn() { ReturnCode = "1", ReturnMsg = Lang.csfs_save_ok }
                :new JsonReturn() { ReturnCode = "0", ReturnMsg = Lang.csfs_save_fail });
        }

        /// 取得編輯PARMCode
        public ActionResult Edit(string id)
        {
            PARMCode model = _parmCodeBiz.ModelByCodeUid(id);
            return View(model);
        }

        [HttpPost]
        public ActionResult Edit(PARMCode model)
        {
            if (model.CodeType == "AutoDispatch")
            {
                if (model.Enable.Equals(true))//AutoDispatch是否啟用
                {
                    int AutoDispatch = ASBIZ.GetAutoDispatchNum();//查扣押經辦人數
                    return Json(_parmCodeBiz.Update(model) ? new JsonReturn() { ReturnCode = "1", ReturnMsg = string.Format(Lang.csfs_pm_AutoDispatch_Seizure, AutoDispatch) }
                                             : new JsonReturn() { ReturnCode = "0", ReturnMsg = Lang.csfs_save_fail });
                }
                else
                {
                    return Json(_parmCodeBiz.Update(model) ? new JsonReturn() { ReturnCode = "1", ReturnMsg = Lang.csfs_save_ok }
                                             : new JsonReturn() { ReturnCode = "0", ReturnMsg = Lang.csfs_save_fail });
                }
            }
            else if (model.CodeType == "AutoDispatchFS")
            {
                if (model.Enable.Equals(true))//AutoDispatchFS是否啟用
                {
                    int AutoDispatchFS = ASBIZ.GetAutoDispatchFSNum();//查外來文經辦人數
                    return Json(_parmCodeBiz.Update(model) ? new JsonReturn() { ReturnCode = "1", ReturnMsg = string.Format(Lang.csfs_pm_AutoDispatch_FS, AutoDispatchFS) }
                                             : new JsonReturn() { ReturnCode = "0", ReturnMsg = Lang.csfs_save_fail });
                }
                else
                {
                    return Json(_parmCodeBiz.Update(model) ? new JsonReturn() { ReturnCode = "1", ReturnMsg = Lang.csfs_save_ok }
                                             : new JsonReturn() { ReturnCode = "0", ReturnMsg = Lang.csfs_save_fail });
                }
            }
            else
            {
                return Json(_parmCodeBiz.Update(model) ? new JsonReturn() { ReturnCode = "1", ReturnMsg = Lang.csfs_save_ok }
                                             : new JsonReturn() { ReturnCode = "0", ReturnMsg = Lang.csfs_save_fail });
            }
        }

        /// 獲取順序
        public ActionResult GetSortOrder(string codeType)
        {
            return Content(_parmCodeBiz.SortOrderByCodeType(codeType));
        }

        //導出
        public ActionResult ExcelParmCode(string CodeType, string Code, string Enable)
        {
            MemoryStream ms = new MemoryStream();
            string fileName = string.Empty;
            ms = _parmCodeBiz.ParmCodeExcel_NPOI(CodeType, Code, Enable);
            fileName = Lang.csfs_menu_tit_parmcode+"_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xls";

            if (ms != null && ms.Length > 0)
            {
                Response.ClearContent();
                Response.ClearHeaders();
            }
            else
            {
                ms = new MemoryStream();
            }
            return File(ms.ToArray(), "application/vnd.ms-excel", fileName);
        }

        /// 綁定參數狀態下拉列表
        public void BindCodeEnable()
        {
            IList<SelectListItem> codeStatusList = new List<SelectListItem>()
            {
                new SelectListItem { Value = "True", Text = Lang.csfs_enable },
                new SelectListItem { Value = "False", Text = Lang.csfs_disable }
            };
            ViewBag.codeStatusList = codeStatusList;
        }

        /// 顯示參數類別下拉下單
        public void BindDdl()
        {
            IList<PARMCode> codeTypeList = _parmCodeBiz.GetAllCodeTypeList();
            ViewBag.codeTypeList = new SelectList(codeTypeList, "CodeType", "CodeTypeDesc");
        }

        ///參數細項下拉菜單
        public void BindDdlByCodeNo()
        {
            IList<PARMCode> codeList = _parmCodeBiz.GetCodeDescByCodeType("");
            ViewBag.codeList = new SelectList(codeList, "CodeNo", "CodeDesc");
        }

        /// 顯示CodeType+CodeTypeDesc的下拉清單
        public void BindDdlByCodeType(string codeType)
        {
            IList<PARMCode> codeTypeList = new List<PARMCode>();
            if (string.IsNullOrEmpty(codeType))
                codeTypeList = _parmCodeBiz.GetCodeTypeDescByCodeType(codeType);
            else
                codeTypeList = new List<PARMCode>();
            ViewBag.codeTypeList = new SelectList(codeTypeList, "CodeType", "CodeTypeDesc", codeType);
        }

     

        /// 綁定參數細項下拉列表
        public JsonResult BindCode(string codeType)
        {
            List<PARMCode> codeList = (List<PARMCode>)_parmCodeBiz.GetCodeDescByCodeType(codeType);
            return Json(codeList);
        }

        public ActionResult GetLDAPAuthorized(string userL, string passwordL, string userR, string passwordR)
        {
            //連線至LDAP主機進行檢核
            var result = new PARMCode();
            result = _parmCodeBiz.CheckLDAP(userL, passwordL, userR, passwordR);
            //result.status = "true";
            //result.msg = "123";
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        //public ActionResult GetRACFAuthorized(string user, string password)
        //{
        //    //連線至LDAP主機進行檢核
        //    var result = new PARMCode();
        //    result.status = "true";
        //    result.msg = "123";
        //    return Json(result, JsonRequestBehavior.AllowGet);
        //}
    }
    
}