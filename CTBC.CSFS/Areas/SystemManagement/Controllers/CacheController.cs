/// <summary>
/// 程式說明：Cache Controller - 快取
/// 維護部門:資訊管理處
/// 中國信託銀行 版權所有  ©  All Rights Reserved.
/// </summary>

using System.Web.Mvc;
using CTBC.CSFS.BussinessLogic;
using CTBC.CSFS.Pattern;
using CTBC.CSFS.Resource;

namespace CTBC.CSFS.Areas.SystemManagement.Controllers
{
    public class CacheController : AppController
    {
        //
        // GET: /Test/

        public ActionResult CacheQuery()
        {
            return View();
        }

        [HttpPost]
        public ActionResult SyncCache() {
            CacheManager cacheMgr = new CacheManager();
            cacheMgr.UploadToCache();
            return Content(Lang.csfs_sync_cache_ok);
        }
    }
}
