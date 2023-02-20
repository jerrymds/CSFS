/// <summary>
/// 程式說明：維護CSFS Role
/// 維護部門:資訊管理處
/// 中國信託銀行 版權所有  ©  All Rights Reserved.
/// </summary>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using CTBC.CSFS.Pattern;
using CTBC.FrameWork.Platform;
using CTBC.CSFS.Models;

namespace CTBC.CSFS.BussinessLogic
{
    public class CSFSRoleBIZ : CommonBIZ
    {
        /// <summary>
        /// 取得角色清單
        /// </summary>
        /// <param name="busClassType"></param>
        /// <returns></returns>
        public IEnumerable<CSFSRole> SelectCSFSRole()
        {
            try
            {
                string sql = @"select * from CSFSRole";
                return base.SearchList<CSFSRole>(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        // get application roles
        public List<CTBC.FrameWork.Platform.User.RoleInfo> GetAppRoles()
        {
            List<CTBC.FrameWork.Platform.User.RoleInfo> _AppRoles = new List<CTBC.FrameWork.Platform.User.RoleInfo>();
            try
            {
                string AppRoles = "";
                if (CTBC.FrameWork.Platform.AppCache.InCache("AppRoles"))
                    AppRoles = (string)CTBC.FrameWork.Platform.AppCache.Get("AppRoles");
                else
                    AppRoles = "";
                XmlDocument AppRoleDom = null;
                AppRoleDom = XML.Create_LoadXML(AppRoles);
                // get application roles from UserRoles table
                foreach (XmlNode lo_Node in AppRoleDom.DocumentElement.ChildNodes)
                {
                    CTBC.FrameWork.Platform.User.RoleInfo lo_NewRole = new CTBC.FrameWork.Platform.User.RoleInfo();
                    lo_NewRole.RoleId = XML.GetAttribute(lo_Node, "Code");
                    lo_NewRole.RoleName = lo_Node.InnerText;
                    _AppRoles.Add(lo_NewRole);
                }
                return _AppRoles;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}