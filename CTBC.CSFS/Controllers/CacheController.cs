/// <summary>
/// 程式說明：Cache Controller - 快取
/// 維護部門:資訊管理處
/// CSFS Version:v3.0
/// 中國信託銀行 版權所有  ©  All Rights Reserved.
/// </summary>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
//using Microsoft.Practices.EnterpriseLibrary.Caching;
using System.Collections;
using CTBC.FrameWork.Platform;
using CTBC.CSFS.Pattern;
using CTBC.CSFS.Models;
using CTBC.CSFS.Resource;

namespace CTBC.CSFS.Controllers
{
    public class CacheController : AppController
    {
        //
        // GET: /Test/

        public ActionResult CacheQuery()
        {
            //string CUF_CacheManagerName = System.Web.Configuration.WebConfigurationManager.AppSettings["CUF_CacheManagerName"];
            //Microsoft.Practices.EnterpriseLibrary.Caching.CacheManager CacheMgr = (Microsoft.Practices.EnterpriseLibrary.Caching.CacheManager)CacheFactory.GetCacheManager(CUF_CacheManagerName);

            //Cache myCache = (Cache)CacheMgr.GetType().GetField("realCache", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(CacheMgr);
            
            //string _count = "";
            //string _object = "物件:";
            //string _onlineuser = "線上員工編號:";

            //_count = "總數:" + " " + CacheMgr.Count.ToString();

            //foreach (DictionaryEntry Item in myCache.CurrentCacheState)
            //{
            //    Object Key = Item.Key;
            //    _object = _object + Key.ToString() + ",";
            //}
            //_object = _object.TrimEnd(',');

            //Dictionary<string, User> userLst = new Dictionary<string, User>();

            //if (CacheMgr.GetData("ONLINE_USER_LIST") != null)
            //{
            //    userLst = (Dictionary<string, User>)CacheMgr.GetData("ONLINE_USER_LIST");

            //    foreach (string item in userLst.Keys)
            //    {
            //        _onlineuser = _onlineuser + item + ",";
            //    }
            //}
            //else
            //{
            //    _onlineuser = "無法取得ONLINE_USER_LIS! 也許物件已被清空!";
            //}
            //_onlineuser = _onlineuser.TrimEnd(',');

            //ViewData["_count"] = _count;
            //ViewData["_object"] = _object;
            //ViewData["_onlineuser"] = _onlineuser;
            return View();
        }

        [HttpPost]
        public ActionResult SyncCache() {
            CTBC.CSFS.BussinessLogic.CacheManager cacheMgr = new CTBC.CSFS.BussinessLogic.CacheManager();
            cacheMgr.UploadToCache();
            return Content(Lang.csfs_sync_cache_ok);
        }
    }
}
