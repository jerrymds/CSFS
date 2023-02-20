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
    public class EmailGroupController : AppController
    {
        EmailGroupBiz _EmailGroupBiz;
        public EmailGroupController()
        {
            _EmailGroupBiz = new EmailGroupBiz(this);
        }

        public ActionResult Query()
        {
            Email_Notice model = new Email_Notice();
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
            return View("_QueryResult", GetEmailGroupList());
        }

        /// <summary>
        /// 取得Email群組
        /// </summary>
        /// <param name="parmSchVO"></param>
        /// <param name="pageNum"></param>
        /// <returns></returns>
        public Email_NoticeViewModel GetEmailGroupList()
        {
            Email_NoticeViewModel viewModel;
            IList<Email_Notice> result = _EmailGroupBiz.GetQueryList();
            viewModel = new Email_NoticeViewModel()
            {
                Email_NoticeList = result
            };
            return viewModel;        
        }

        /// <summary>
        /// 新增Email群組
        /// </summary>
        /// <returns></returns>
        public ActionResult Create()
        {
            Email_Notice model = new Email_Notice();
            return View(model); 
        }

        /// <summary>
        /// 保存新增
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Create(Email_Notice model)
        {
            string scriptStr = "";
            if (_EmailGroupBiz.ValiReEmail(model) > 0)
            {
                scriptStr = "alert('" + Lang.csfs_emailgroup_msg1 + "');";
            }
            else
            {
                int rtn = _EmailGroupBiz.Create(model);
                if (rtn > 0)
                {
                    scriptStr = "alert('" + Lang.csfs_add_ok + "');location.href='" + Url.Action("Query", "EmailGroup") + "';";
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
        public ActionResult Edit(string Email)
        {
            Email_Notice model = _EmailGroupBiz.Select(Email);
            return View(model); 
        }

        /// <summary>
        /// 保存修改
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Edit(Email_Notice model, string EmailEdit)
        {
            string scriptStr = "";
            if (_EmailGroupBiz.ValiReEmail(model) > 0)
            {
                scriptStr = "alert('" + Lang.csfs_emailgroup_msg1 + "');";
            }
            else
            {
                if (_EmailGroupBiz.Edit(model, EmailEdit))
                {
                    scriptStr = "alert('" + Lang.csfs_edit_ok + "');location.href='" + Url.Action("Query", "EmailGroup") + "';";
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
        public ActionResult Delete(string Email)
        {
            string rtnStr = "0";
            int rtn = _EmailGroupBiz.Delete(Email);
            if (rtn > 0)
                rtnStr = "1";                
            return Content(rtnStr);
        }
	}
}