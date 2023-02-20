/// <summary>
/// 程式說明:PARMCode Controller - 共用參數
/// 維護部門:資訊管理處
/// CSFS Version:v3.0
/// 中國信託銀行 版權所有  ©  All Rights Reserved. 
/// </summary>

using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Data;
using CTBC.CSFS.BussinessLogic;
using CTBC.CSFS.Pattern;
using CTBC.CSFS.Models;
using CTBC.CSFS.Resource;
using CTBC.CSFS.ViewModels;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;

namespace CTBC.CSFS.Controllers
{
    public class PARMCodeController : AppController
    {
        #region 全域變數

        // ADO訪問類
        PARMCodeBIZ _parmCodeBiz;

        public PARMCodeController()
        {
            //創建數據庫操作類對象
            _parmCodeBiz = new PARMCodeBIZ(this);
        }

        #endregion

        #region 頁面載入

        //20150108 horace 弱掃
        public ActionResult Query()
        {
            PARMCode criteria = new PARMCode();
            BindDdl();//CodeType下拉列表

            //20130306 hroace 修正判斷QueryCodeType是否有值
            //BindCodeEnable();//CodeNo下拉列表
            if (string.IsNullOrEmpty(criteria.QueryCodeType))
                BindDdlByCodeNo("");
            else
                BindDdlByCodeNo(criteria.QueryCodeType);
            return View(criteria);
        }
        /// <summary>
        /// 加載參數清單頁面
        /// </summary>
        /// <returns></returns>
        /// <remarks>2012/07/09 Kyle</remarks>
        [HttpPost]//20150108 horace 弱掃
        public ActionResult Query(PARMCode criteria)
        {
            BindDdl();//CodeType下拉列表

            //20130306 hroace 修正判斷QueryCodeType是否有值
            //BindCodeEnable();//CodeNo下拉列表
            if (string.IsNullOrEmpty(criteria.QueryCodeType))
                BindDdlByCodeNo("");
            else
                BindDdlByCodeNo(criteria.QueryCodeType);
            return View(criteria);
        }
        /// <summary>
        /// 返回查詢清單
        /// </summary>
        /// <param name="criteria">查詢條件</param>
        /// <returns></returns>
        /// <remarks>2012/07/09 Kyle</remarks>
        [HttpPost]//20150108 horace 弱掃
        public ActionResult _QueryResult(PARMCode criteria, int pageNum = 1)
        {
            return PartialView("_QueryResult", SearchList(criteria, pageNum));
        }

        [HttpGet]//20150108 horace 弱掃暫時mark
        //public ActionResult Delete(string CodeUid)
        //{
        //    PARMCode model = new PARMCode();
        //    model.CodeUid = CodeUid;
        //    if (_parmCodeBiz.Delete(model))
        //        ViewBag.DelMsg = "success!!!!";
        //    CacheManager cm = new CacheManager();
        //    cm.ParmCodeToCache();
        //    return View();
        //}

        /// <summary>
        /// 取得編輯PARMCode
        /// </summary>
        /// <param name="id">CodeUid</param>
        /// <returns></returns>
        public ActionResult Edit(string id)
        {
            PARMCode model = _parmCodeBiz.ModelByCodeUid(id);
            //BindDdlByCodeType(model.QueryCodeType);
            return View(model);
        }

        [HttpPost]
        public ActionResult Edit(PARMCode model)
        {
            string scriptStr = string.Empty;
            if (_parmCodeBiz.Update(model))
            {
                //提示修改成功
                scriptStr = "alert('" + Lang.csfs_edit_ok + "');location.href = '" + Url.Action("Query", "PARMCode") + "?QueryCodeType=" + model.CodeType + "&QueryCodeNo=" + model.CodeNo + "&QueryEnable=" + model.Enable + "';";

                //重新加載緩存
                CacheManager cm = new CacheManager();
                cm.ParmCodeToCache();
            }
            else
            {
                //提示修改失敗
                scriptStr = "alert('" + Lang.csfs_edit_fail + "');";

            }
            return JavaScript(scriptStr);
        }

        public ActionResult Create()
        {
            PARMCode model = new PARMCode();

            //獲取傳入參數
            string action = "Add";
            string key = "";

            ViewData["hidAction"] = action;
            ViewData["hidCodeUid"] = key;

            //20150108 horace 弱掃
            model.QueryCodeNo = Request.Form["CodeNo"];
            model.QueryCodeType = Request.QueryString["CodeType"];
            model.QueryEnable = Request.Form["Enable"];

            //BindDdlByCodeType("");
            BindDdl();
            return View(new PARMCode());
        }

        [HttpPost]
        public ActionResult Create(PARMCode model)
        {
            //獲取傳入參數
            string hidAction = Request.Form["hidAction"];//20150108 horace 弱掃

            //初始化返回提示script代碼
            string scriptStr = string.Empty;

            CacheManager cm = new CacheManager();
            int count = _parmCodeBiz.Count(model.CodeNo, model.CodeType);

            if (count > 0)
            {
                //提示“該代碼已存在，請重新輸入！”
                scriptStr = "alert('" + Lang.csfs_pm_data_repeat + "');$(\"#txtCode\").val(\"\");$(\"#txtCode\").focus();";
            }
            else
            {
                if (_parmCodeBiz.Create(model))
                {
                    //提示新增成功
                    scriptStr = "alert('" + Lang.csfs_add_ok + "');location.href='" + Url.Action("Query") + "?QueryCodeType=" + model.CodeType + "'";

                    //重新加載緩存
                    cm.ParmCodeToCache();
                }
                else
                {
                    //提示新增失敗
                    scriptStr = "alert('" + Lang.csfs_add_fail + "');";
                }
            }
            return JavaScript(scriptStr);
        }

        /// <summary>
        /// 列印
        /// </summary>
        /// <param name="entity">列印條件</param>
        /// <param name="state">列印條件</param>
        /// <remarks></remarks>
        //public string Export(string typeValue, string fileName)
        //{
        //    CSFSExportBIZ _expBiz = new CSFSExportBIZ();

        //    //Step 1:取得原始資料
        //    DataTable result = _parmCodeBiz.SelectToDT();

        //    if (result.Rows.Count > 0)
        //    {

        //        //Step 2: 組合暫存路徑+檔名
        //        string vfilePath = Config.GetValue("Export_Path") + fileName + "." + typeValue.ToString().ToLower();
        //        string filePath = Server.MapPath(vfilePath);

        //        //Step 3: 產生檔案
        //        bool isOK = _expBiz.ExportData(result, typeValue, filePath);
        //        if (isOK)
        //        {
        //            //Step 4: 組合輸入Action URL
        //            //範例:/CSFS/Upload/PARMCode.xls
        //            filePath = Url.Action(vfilePath.Substring(2).Replace("///", "/"), "/");//
        //            //Step 5: 輸出檔案
        //            return "Success:" + filePath.Replace("%5C", "/");
        //        }
        //        else return "Failure:Error";
        //    }
        //    else
        //        return "Empty:NoData";
        //}

        #endregion

        #region 自定義方法

        /// <summary>
        /// 顯示CodeType的下拉清單
        /// </summary>
        /// <remarks></remarks>
        public void BindDdl()
        {
            IList<PARMCode> codeTypeList = _parmCodeBiz.GetAllCodeTypeList();
            ViewBag.codeTypeList = new SelectList(codeTypeList, "CodeType", "CodeTypeDesc");
        }

        /// <summary>
        /// 顯示CodeType+CodeTypeDesc的下拉清單
        /// </summary>
        /// <param name="codeType"></param>
        public void BindDdlByCodeType(string codeType)
        {
            IList<PARMCode> codeTypeList = new List<PARMCode>();
            if (string.IsNullOrEmpty(codeType))
                codeTypeList = _parmCodeBiz.GetCodeTypeDescByCodeType(codeType);
            else
                codeTypeList = new List<PARMCode>();
            ViewBag.codeTypeList = new SelectList(codeTypeList, "CodeType", "CodeTypeDesc", codeType);
        }

        /// <summary>
        /// 顯示CodeNo+CodeDesc的下拉清單
        /// </summary>
        /// <remarks></remarks>
        public void BindDdlByCodeNo(string codeType)
        {
            IList<PARMCode> codeList = _parmCodeBiz.GetCodeDescByCodeType(codeType);
            ViewBag.codeList = new SelectList(codeList, "CodeNo", "CodeDesc");
        }

        /// <summary>
        /// 綁定參數細項下拉列表
        /// </summary>
        /// <param name="codeType">參數類型編號</param>
        /// <returns></returns>
        /// <remarks>2012/07/18 Kyle</remarks>
        public JsonResult BindCode(string codeType)
        {
            List<PARMCode> codeList = (List<PARMCode>)_parmCodeBiz.GetCodeDescByCodeType(codeType);
            return Json(codeList);
        }

        /// <summary>
        /// 綁定參數狀態下拉列表
        /// </summary>
        /// <remarks>2012/07/09 Kyle</remarks>
        public void BindCodeEnable()
        {
            IList<SelectListItem> codeStatusList = new List<SelectListItem>();
            codeStatusList.Add(new SelectListItem { Value = "1", Text = Lang.csfs_enable });
            codeStatusList.Add(new SelectListItem { Value = "0", Text = Lang.csfs_disable });
            ViewBag.codeStatusList = new SelectList(codeStatusList, "Value", "Text");
        }

        /// <summary>
        /// 查詢清單
        /// </summary>
        /// <param name="parmCode">查詢條件</param>
        /// <returns>清單</returns>
        /// <remarks>2012/07/09 Kyle</remarks>
        public PARMCodeViewModel SearchList(PARMCode criteria, int pageNum)
        {
            PARMCodeViewModel parmCodeViewModel;
            IList<PARMCode> result = _parmCodeBiz.GetQueryList(criteria, pageNum);

            parmCodeViewModel = new PARMCodeViewModel()
            {
                Result = result,
                Criteria = criteria
            };

            // 當前每頁筆數
            parmCodeViewModel.Criteria.PageSize = _parmCodeBiz.PageSize;

            // 當前總筆數
            parmCodeViewModel.Criteria.CurrentPage = _parmCodeBiz.PageIndex;

            // 當前頁碼
            parmCodeViewModel.Criteria.TotalItemCount = _parmCodeBiz.DataRecords;

            return parmCodeViewModel;
        }

        /// <summary>
        /// 獲取順序
        /// </summary>
        /// <param name="codeType"></param>
        /// <returns></returns>
        /// <remarks>2012/07/10 Kyle</remarks>
        public string GetSortOrder(string codeType)
        {
            // 返回順序
            return _parmCodeBiz.SortOrderByCodeType(codeType);
        }

        #endregion
    }
}