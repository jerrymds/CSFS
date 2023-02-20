/// <summary>
/// 程式說明:Cache管理
/// 維護部門:資訊管理處
/// 中國信託銀行 版權所有  ©  All Rights Reserved. 
/// </summary>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CTBC.CSFS.Models;
using CTBC.FrameWork.Platform;
using CTBC.CSFS.Pattern;

namespace CTBC.CSFS.BussinessLogic
{
    public class CacheManager
    {
        /// <summary>
        /// 將全部要放入cache的資料一次Upload到Cache中
        /// </summary>
        public void UploadToCache()
        {

            ParmCodeToCache();

            CSFSRoleToCache();

            AuthZToCache();
        }

        /// <summary>
        /// 更新key這個值所代表的資料到cache中 
        /// </summary>
        /// <param name="key"></param>
        public void updatetocache(string key)
        {
            switch (key)
            {
                case "parmcode":
                    ParmCodeToCache();
                    break;
                case "CSFSRole":
                    CSFSRoleToCache();
                    break;
                case "AuthZ":
                    AuthZToCache();
                    break;
                default:
                    break;
            }
            //呼叫各自xxxADO(未來改成xxxBIZ),將資料Upload到Cache中
        }

        /// <summary>
        /// [放入Cache資料 1]
        /// </summary>
        public void ParmCodeToCache()
        {
            try
            {
                //[放入Cache資料1]:[共用的參數資料]
                //自PARMCodeADO.cs中去抓取(_data.Select())整個參數資料,並且新增到Cache中,
                //並且命名key值為ParmCode如下範例
                PARMCodeBIZ _data = new PARMCodeBIZ();//外來改成PARMCodeBIZ
                IEnumerable<PARMCode> list = _data.SelectAllPARMCode();
                //* 20150707 好像天生的AppCache.Update有問題.沒有更新到Cache.現已改為強制先刪除再加
                if (AppCache.InCache("PARMCode"))
                    AppCache.Erase("PARMCode");
                 
                AppCache.Add(list, "PARMCode");

                //同步更新儲存在PARMCode內的CSFSErrorCode
                CSFSErrorCodeToCache();
            }
            catch (Exception ex)
            { throw ex; }
        }

        /// <summary>
        /// [放入Cache資料]:[錯誤訊息]
        /// </summary>
        public void CSFSErrorCodeToCache()
        {
            try
            {
                IEnumerable<CSFSErrorCode> errcodeList = new List<CSFSErrorCode>();
                IList<PARMCode> list = (IList<PARMCode>)AppCache.Get("PARMCode");
                errcodeList = (from p in list
                               where p.CodeType == "CSFSErrorCode"
                               select new CSFSErrorCode { ErrId = p.CodeNo, ErrDesc = p.CodeDesc }).ToList();

                Dictionary<string, string> dic = new Dictionary<string, string>();
                foreach (CSFSErrorCode n in errcodeList) { dic.Add(n.ErrId, n.ErrDesc); }

                if (AppCache.InCache("CSFSErrorCode"))
                    AppCache.Update(dic, "CSFSErrorCode");
                else
                    AppCache.Add(dic, "CSFSErrorCode");
            }
            catch (Exception ex)
            { throw ex; }
        }

        /// <summary>
        /// 將CSFSRole放入Cache
        /// </summary>
        public void CSFSRoleToCache()
        {
            try
            {
                CSFSRoleBIZ _data = new CSFSRoleBIZ();
                IEnumerable<CSFSRole> list = _data.SelectCSFSRole();
                //* 20150707 好像天生的AppCache.Update有問題.沒有更新到Cache.現已改為強制先刪除再加
                //if (AppCache.InCache("CSFSRole"))
                //    AppCache.Update(list, "CSFSRole");
                //else
                if (AppCache.InCache("CSFSRole"))
                    AppCache.Erase("CSFSRole");
                //* 20150707 好像天生的AppCache.Update有問題.沒有更新到Cache.現已改為強制先刪除再加

                AppCache.Add(list, "CSFSRole");
            }
            catch (Exception ex)
            { throw ex; }
        }

        /// <summary>
        /// 將AuthZ放入Cache
        /// </summary>
        /// 20140117 horace
        public void AuthZToCache()
        {
            try
            {
                AuthZBIZ _data = new AuthZBIZ();
                IEnumerable<AuthZ> list = _data.selectAuthZ();
                if (list != null)
                {
                    if (list.Any())
                    {
                        //* 20150707 好像天生的AppCache.Update有問題.沒有更新到Cache.現已改為強制先刪除再加
                        //將IEnumerable<AuthZ> 物件 存入Cache中
                        if (AppCache.InCache("AuthZ"))
                            AppCache.Erase("AuthZ");
                        //    AppCache.Update(list, "AuthZ");
                        //else
                        AppCache.Add(list, "AuthZ");

                        //將AuthZ.AppAuthZ欄位 xml 存入Cache中
                        if (AppCache.InCache("AppAuthZ"))
                            AppCache.Erase("AppAuthZ");
                        //    AppCache.Update(list.ToList()[0].AppAuthZ, "AppAuthZ");
                        //else
                            AppCache.Add(list.ToList()[0].AppAuthZ, "AppAuthZ");

                        //將AuthZ.AppRoles欄位 xml 存入Cache中
                        if (AppCache.InCache("AppRoles"))
                            AppCache.Erase("AppRoles");
                        //    AppCache.Update(list.ToList()[0].AppRoles, "AppRoles");
                        //else
                            AppCache.Add(list.ToList()[0].AppRoles, "AppRoles");
                        //* 20150707 好像天生的AppCache.Update有問題.沒有更新到Cache.現已改為強制先刪除再加
                    }
                }
            }
            catch (Exception ex)
            { throw ex; }
        }

        /// <summary>
        /// 將CSFSBUMaster放入Cache
        /// </summary>
        public void CSFSBUMasterToCache()
        {
            try
            {
                CSFSBUBIZ _data = new CSFSBUBIZ();
                IEnumerable<CSFSBUMaster> list = _data.SelectAllCSFSBUMaster();

                if (AppCache.InCache("CSFSBUMaster"))
                    AppCache.Update(list, "CSFSBUMaster");
                else
                    AppCache.Add(list, "CSFSBUMaster");
            }
            catch (Exception ex)
            { throw ex; }
        }

        /// <summary>
        /// 將CSFSBU放入Cache
        /// </summary>
        public void CSFSBUToCache()
        {
            try
            {
                CSFSBUBIZ _data = new CSFSBUBIZ();
                IEnumerable<CSFSBU> list = _data.SelectAllCSFSBU();

                if (AppCache.InCache("CSFSBU"))
                    AppCache.Update(list, "CSFSBU");
                else
                    AppCache.Add(list, "CSFSBU");
            }
            catch (Exception ex)
            { throw ex; }
        }

        /// <summary>
        /// 將CSFSBUToEmployee放入Cache
        /// </summary>
        public void CSFSBUToEmployeeToCache()
        {
            try
            {
                CSFSBUBIZ _data = new CSFSBUBIZ();
                IEnumerable<CSFSBUToEmployee> list = _data.SelectAllCSFSBUToEmployee();

                if (AppCache.InCache("CSFSBUToEmployee"))
                    AppCache.Update(list, "CSFSBUToEmployee");
                else
                    AppCache.Add(list, "CSFSBUToEmployee");
            }
            catch (Exception ex)
            { throw ex; }
        }

        /// <summary>
        /// 將CSFSEmployee放入Cache
        /// </summary>
        public void CSFSEmployeeToCache()
        {
            try
            {
                CSFSEmployeeBIZ _data = new CSFSEmployeeBIZ();
                IEnumerable<CSFSEmployee> list = _data.GetCSFSEmployeeAll();

                if (AppCache.InCache("CSFSEmployee"))
                    AppCache.Update(list, "CSFSEmployee");
                else
                    AppCache.Add(list, "CSFSEmployee");
            }
            catch (Exception ex)
            { throw ex; }
        }
    }
}