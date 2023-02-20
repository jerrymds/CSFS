/// <summary>
/// 程式說明:PARMScheduleSetting Controller - 排程
/// 維護部門:資訊管理處
/// CSFS Version:v3.0
/// 中國信託銀行 版權所有  ©  All Rights Reserved. 
/// </summary>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CTBC.CSFS.ViewModels;
using CTBC.CSFS.Pattern;
using CTBC.CSFS.Models;
using CTBC.CSFS.Resource;

namespace CTBC.CSFS.Controllers
{
    public class PARMScheduleSettingController : AppController
    {
        PARMScheduleSettingBIZ _PARMSchSetBIZ;

        public PARMScheduleSettingController()
        {
            _PARMSchSetBIZ = new PARMScheduleSettingBIZ(this);
        }

        /// <summary>
        /// 查詢排程設定資料條件頁
        /// </summary>
        /// <returns></returns>
        public ActionResult Query()
        {
            PARMScheduleSettingVO model = new PARMScheduleSettingVO();
            return View(model);
        }

        /// <summary>
        /// 查詢排程設定資料
        /// </summary>
        /// <param name="parmSchVO"></param>
        /// <param name="pageNum"></param>
        /// <returns></returns>
        /// 20150212弱掃加上[HttpPost]
        [HttpPost]
        public ActionResult _QueryResult(PARMScheduleSettingVO parmSchVO, int pageNum = 1)
        {
            //var model = _PARMSchSetBIZ.GetScheduleList();
            return View("_QueryResult", GetScheduleList(parmSchVO, pageNum));
        }

        /// <summary>
        /// 取得排程設定資料(分頁)
        /// </summary>
        /// <param name="parmSchVO"></param>
        /// <param name="pageNum"></param>
        /// <returns></returns>
        public PARMScheduleSettingViewModel GetScheduleList(PARMScheduleSettingVO parmSchVO, int pageNum = 1)
        {
            PARMScheduleSettingViewModel viewModel;

            //取出符合條件的排程設定資訊
            IList<PARMScheduleSettingVO> result = _PARMSchSetBIZ.GetQueryList(parmSchVO, pageNum);

            viewModel = new PARMScheduleSettingViewModel()
            {
                PARMScheduleSettingVO = parmSchVO,
                PARMScheduleSettingList = result,
            };

            //分頁相關設定
            viewModel.PARMScheduleSettingVO.PageSize = _PARMSchSetBIZ.PageSize;
            viewModel.PARMScheduleSettingVO.CurrentPage = _PARMSchSetBIZ.PageIndex;
            viewModel.PARMScheduleSettingVO.TotalItemCount = _PARMSchSetBIZ.DataRecords;

            return viewModel;        
        }

        /// <summary>
        /// 新增排程設定資料
        /// </summary>
        /// <returns></returns>
        public ActionResult Create()
        {
            PARMScheduleSettingVO model = new PARMScheduleSettingVO();
            return View(model); 
        }

        /// <summary>
        /// 新增排程設定資料
        /// </summary>
        /// <param name="model">排程</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Create(PARMScheduleSettingVO model)
        {
            string scriptStr = "";
            int rtn = _PARMSchSetBIZ.Create(model);
            if (rtn > 0)
            {
                scriptStr = "alert('" + Lang.csfs_add_ok + "');location.href='" + Url.Action("Query", "PARMScheduleSetting") + "';";            
            }
            else {
                scriptStr = "alert('" + Lang.csfs_add_fail + "');";
            }
            return JavaScript(scriptStr);
        }

        /// <summary>
        /// 修改排程設定資料
        /// </summary>
        /// <param name="id">排程ID</param>
        /// <returns></returns>
        public ActionResult Edit(Guid id)
        {
            PARMScheduleSettingVO model = _PARMSchSetBIZ.Select(id);
            return View(model); 
        }

        /// <summary>
        /// 修改排程設定資料
        /// </summary>
        /// <param name="model">排程</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Edit(PARMScheduleSettingVO model)
        {
            string scriptStr = "";
            if (_PARMSchSetBIZ.Edit(model))
            {
                scriptStr = "alert('" + Lang.csfs_edit_ok + "');location.href='" + Url.Action("Query", "PARMScheduleSetting") + "';";
            }
            else
            {
                scriptStr = "alert('" + Lang.csfs_edit_fail + "');";
            }
            return JavaScript(scriptStr);
        }

        /// <summary>
        /// 刪除排程設定資料
        /// </summary>
        /// <param name="id">排程ID</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Delete(Guid id)
        {
            string rtnStr = "0";
            int rtn = _PARMSchSetBIZ.Delete(id);
            if (rtn > 0)
                rtnStr = "1";                
            return Content(rtnStr);
        }
    }
}
