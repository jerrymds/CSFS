using System;
using System.Collections.Generic;
using System.Web.Mvc;
using CTBC.CSFS.Models;
using CTBC.CSFS.Pattern;
using CTBC.CSFS.Resource;
using CTBC.CSFS.ViewModels;
using CTBC.CSFS.BussinessLogic;

namespace CTBC.CSFS.Areas.SystemManagement.Controllers
{
    public class SendTimeSectionController : AppController
    {
        SendEDocBiz _SendEDocBiz;
        public SendTimeSectionController()
        {
            _SendEDocBiz = new SendEDocBiz();
        }
        public ActionResult Query()
        {
            SendTimeSection model = new SendTimeSection();
            return View(model);
        }
        /// <summary>
        /// 查詢
        /// </summary>
        /// <param name="parmSchVO"></param>
        /// <param name="pageNum"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult _QueryResult()
        {
            return View("_QueryResult", GetSendTimeSectionList());
        }
        /// <summary>
        /// 取得電子發文時段
        /// </summary>
        /// <param name="parmSchVO"></param>
        /// <param name="pageNum"></param>
        /// <returns></returns>
        public SendTimeSectionViewModel GetSendTimeSectionList()
        {
            SendTimeSectionViewModel viewModel;
            IList<SendTimeSection> result = _SendEDocBiz.GetSendTimeSectionList();
            viewModel = new SendTimeSectionViewModel()
            {
                SendTimeSectionList = result
            };
            return viewModel;
        }
        /// <summary>
        /// 新增電子發文時段
        /// </summary>
        /// <returns></returns>
        public ActionResult Create()
        {
            SendTimeSection model = new SendTimeSection();
            return View(model);
        }
        /// <summary>
        /// 保存新增
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Create(SendTimeSection model, string time_h, string time_m)
        {
            string scriptStr = "";
            model.TimeSection = time_h + ":" + time_m;
            if (_SendEDocBiz.ValiReTime(model) > 0)
            {
                scriptStr = "alert('" + Lang.csfs_timesection_msg2 + "');";
            }
            else
            {
                int rtn = _SendEDocBiz.Create(model);
                if (rtn > 0)
                {
                    scriptStr = "alert('" + Lang.csfs_add_ok + "');location.href='" + Url.Action("Query", "SendTimeSection") + "';";
                }
                else
                {
                    scriptStr = "alert('" + Lang.csfs_add_fail + "');";
                }
            }
            return JavaScript(scriptStr);
        }

        /// <summary>
        /// 修改
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult Edit(string timesection)
        {
            SendTimeSection model = _SendEDocBiz.Select(timesection);
            return View(model);
        }

        /// <summary>
        /// 保存修改
        /// </summary>
        /// <param name="model">排程</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Edit(SendTimeSection model, string timesectionEdit, string time_h, string time_m)
        {
            string scriptStr = "";
            model.TimeSection = time_h + ":" + time_m;
            if (_SendEDocBiz.ValiReTime(model) > 0)
            {
                scriptStr = "alert('" + Lang.csfs_timesection_msg2 + "');";
            }
            else
            {
                if (_SendEDocBiz.Edit(model, timesectionEdit))
                {
                    scriptStr = "alert('" + Lang.csfs_edit_ok + "');location.href='" + Url.Action("Query", "SendTimeSection") + "';";
                }
                else
                {
                    scriptStr = "alert('" + Lang.csfs_edit_fail + "');";
                }
            }
            return JavaScript(scriptStr);
        }
        /// <summary>
        /// 刪除
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Delete(string timesection)
        {
            string rtnStr = "0";
            int rtn = _SendEDocBiz.Delete(timesection);
            if (rtn > 0)
                rtnStr = "1";
            return Content(rtnStr);
        }
	}
}