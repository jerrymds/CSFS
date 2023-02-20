using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Caching;
//using Microsoft.Practices.EnterpriseLibrary.Caching;
//using Microsoft.Practices.EnterpriseLibrary.Caching.Expirations;
//using Microsoft.Practices.EnterpriseLibrary.Logging;

namespace CTBC.FrameWork.Platform
{
	public class AppCache
    {
        #region 使用 System.Runtime.Caching-------
        //移除 參照 Microsoft.Practices.EnterpriseLibrary.Caching
        //增加 參照 System.Runtime.Caching
        //20140616 horace

        // AppCache enum
        public enum CacheError { None, NotFound, KeyInUse };

        // AppCache public properties
        private static ObjectCache Cache { get { return MemoryCache.Default; } }
        public delegate object NewObjectFunc(string key);

        //---------------------------------------
        //			Public Method
        //---------------------------------------

        // check if the object in cache?
        public static bool InCache(string key)
        {
            return (Cache[key] != null);
        }

        // get data from cache
        public static object Get(string key)
        {
            return Cache[key];
        }

        // get data from cache, add a new object if data is not in the cahce
        public static object Get(string key, NewObjectFunc Func)
        {
            object NewObject = Cache[key];
            if (NewObject == null)
            {
                NewObject = Func(key);
                Add(NewObject, key);
            }
            return NewObject;
        }

        // put data into cache
        public static void Add(object data, string key)
        {
            // return CacheError.KeyInUse if key is duplicated, otherwise return CacheError.None
            try {
                CacheItemPolicy policy = new CacheItemPolicy();
                policy.AbsoluteExpiration = ObjectCache.InfiniteAbsoluteExpiration;
                Cache.Add(new CacheItem(key, data), policy);
                return; 
            }
            catch { throw; }
        }

        //put data into cache with cacheTime(milliseconds)
        //20140616 horace
        public static void Add(object data, string key,int cacheTime)
        {
            // return CacheError.KeyInUse if key is duplicated, otherwise return CacheError.None
            try
            {
                CacheItemPolicy policy = new CacheItemPolicy();
                policy.AbsoluteExpiration = DateTime.Now.AddMilliseconds(cacheTime);
                Cache.Add(new CacheItem(key, data), policy);
                return;
            }
            catch { throw; }
        }

        // update cache
        public static CacheError Update(object data, string key)
        {
            // return CacheError.NotFound if key is not found, otherwise return CacheError.None
            try { Add(data,key); return CacheError.None; }
            catch { throw; }
        }

        // erase cache
        public static void Erase(string key)
        {
            // erase cache by key
            try{Cache.Remove(key);return;}
            catch { throw; };
        }
        #endregion

        #region 使用EL 5.0 Caching------
        //////// AppCache enum
        //////public enum CacheError { None, NotFound, KeyInUse };

        //////// AppCache public properties
        //////private static string CUF_CacheManagerName =
        //////    (System.Web.Configuration.WebConfigurationManager.AppSettings["CUF_CacheManagerName"] == null ?
        //////        "CUF_CacheManager" : System.Web.Configuration.WebConfigurationManager.AppSettings["CUF_CacheManagerName"]);
        //////public static CacheManager CacheMgr = (CacheManager) CacheFactory.GetCacheManager(CUF_CacheManagerName);
        //////public delegate object NewObjectFunc(string key);

        ////////---------------------------------------
        ////////			Public Method
        ////////---------------------------------------
		
        //////// check if the object in cache?
        //////public static bool InCache(string key)
        //////{
        //////    return CacheMgr.GetData(key) != null;
        //////}

        //////// get data from cache
        //////public static object Get(string key)
        //////{
        //////    return CacheMgr.GetData(key);
        //////}

        //////// get data from cache, add a new object if data is not in the cahce
        //////public static object Get(string key, NewObjectFunc Func)
        //////{
        //////    object NewObject = CacheMgr.GetData(key);
        //////    if (NewObject == null) {
        //////        NewObject = Func(key);
        //////        Add(NewObject, key);
        //////    }
        //////    return NewObject;
        //////}

        //////// put data into cache
        //////public static void Add(object data, string key)
        //////{
        //////    // return CacheError.KeyInUse if key is duplicated, otherwise return CacheError.None
        //////    try { CacheMgr.Add(key, data); return; }
        //////    catch { throw; }
        //////}

        //////// update cache
        //////public static CacheError Update(object data, string key)
        //////{
        //////    // return CacheError.NotFound if key is not found, otherwise return CacheError.None
        //////    try { CacheMgr.Add(key, data); return CacheError.None; }
        //////    catch { throw; }
        //////}

        //////// erase cache
        //////public static void Erase(string key)
        //////{
        //////    // erase cache by key
        //////    try { CacheMgr.Remove(key); return; }
        //////    catch { throw; };
        //////}
        #endregion
    }
}
