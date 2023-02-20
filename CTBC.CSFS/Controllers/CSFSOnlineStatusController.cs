/// <summary>
/// 程式說明:CSFSOnlineStatus Controller - 線上使用者管理
/// 維護部門:資訊管理處
/// CSFS Version:v3.0
/// 中國信託銀行 版權所有  ©  All Rights Reserved. 
/// </summary>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CTBC.FrameWork.Platform;
using CTBC.CSFS.Pattern;
using CTBC.CSFS.Models;
using CTBC.CSFS.Resource;

namespace CTBC.CSFS.Controllers
{
    public class CSFSOnlineStatusController : AppController
    {
        //
        // GET: /CSFSOnlineStatus/

        public ActionResult Index()
        {
            Dictionary<string, User> userLst = new Dictionary<string, User>();
            if (CTBC.FrameWork.Platform.AppCache.InCache("ONLINE_USER_LIST"))
                userLst = (Dictionary<string, User>)CTBC.FrameWork.Platform.AppCache.Get("ONLINE_USER_LIST");

            IEnumerable<User> list = userLst.Values.ToList();
            if (Session["UserAccount"] != null)
                ViewBag.Myself = (string)Session["UserAccount"];
            else ViewBag.Myself = "";
            return View(list);
        }

        //20150212 弱掃 不需要
        //public ActionResult Delete(string id)
        //{
        //    return View("Error");
        //}

        [HttpPost, ActionName("Delete")]
        public string DeleteConfirmed(string id)
        {
            Dictionary<string, User> userLst = new Dictionary<string, User>();
            if (CTBC.FrameWork.Platform.AppCache.InCache("ONLINE_USER_LIST"))
            {
                userLst = (Dictionary<string, User>)CTBC.FrameWork.Platform.AppCache.Get("ONLINE_USER_LIST");
                if (userLst.ContainsKey(id))
                {
                    userLst.Remove(id);
                    CTBC.FrameWork.Platform.AppCache.Update(userLst, id);
                    return Lang.csfs_del_ok;
                }
            }

            return Lang.csfs_del_ok;
        }

        public ActionResult GetLocalIP()
        {
            //string ip2 = HttpContext.Request.ServerVariables["REMOTE_ADDR"].ToString();
            string[] ipAry = new string[51];
            string[] keyn = HttpContext.Request.ServerVariables.AllKeys;
            for (int i = 0; i < 51; i++)
            {
                ipAry[i] = keyn[i] + " = " +  HttpContext.Request.ServerVariables[i].ToString();
            }
            ViewBag.ServerVars = ipAry;
            //ViewBag.REMOTE_ADDR = HttpContext.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];// HttpContext.Request.ServerVariables["REMOTE_ADDR"].ToString();
            return View();

        }
    }
}
