using System;

namespace CTCB.NUMS.Batch.LDAPSync
{
    public class NUMSRole
    {
        /// <summary>
        /// 角色編號
        /// </summary>
        public string RoleID {get;set;}

        /// <summary>
        /// 角色名稱
        /// </summary>
        public string RoleName {get;set;}

        /// <summary>
        /// 角色群組編號
        /// </summary>
        public string RoleGroupID {get;set;}

        /// <summary>
        /// 角色說明
        /// </summary>
        public string RoleDesc {get;set;}
    }
}
