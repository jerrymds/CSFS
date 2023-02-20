/// <summary>
/// 程式說明：維護PARMMenu -Menu管理
/// 維護部門:資訊管理處
/// 中國信託銀行 版權所有  ©  All Rights Reserved.
/// </summary>

using System;
using System.IO;
using System.Web;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.Data.SqlClient;
using System.Data;
using CTBC.CSFS.Models;
using CTBC.FrameWork.Platform;
using CTBC.CSFS.Pattern;

namespace CTBC.CSFS.BussinessLogic
{
    public class PARMMenuBIZ : CommonBIZ
    {
        public PARMMenuBIZ(AppController appController)
            : base(appController)
        { }

        public PARMMenuBIZ()
        { }

        private List<PARMMenuXMLNode> _menuListSorted;

        private List<PARMMenuXMLNode> _menuPageList;

        List<PARMMenuXMLNode> newList = new List<PARMMenuXMLNode>();

        IList<CSFSRole> _csfsRoleList = new List<CSFSRole>();

        public PARMMenuXML SyncAuthZXML()
        {
            //1.取出PARMMenu
            List<PARMMenuXMLNode> menuList = GetPARMMenuList().ToList();

            //2.轉換成XML 物件
            PARMMenuXML xmlObj = ConvertToXMLObject(menuList);

            //3.賦予根目錄初始值
            xmlObj = SetRootDefValue(xmlObj);

            return xmlObj;
        }

        //設定AAA XML Root節點預設值
        public PARMMenuXML SetRootDefValue(PARMMenuXML obj)
        {
            PARMMenuXML m = new PARMMenuXML();
            m = obj;
            m.ID = @"1";
            m.NextID = @"1380";
            m.TITLE = Config.GetValue("CUF_AppName") + "授權樹";
            m.HIDE = @"false";
            m.Flow = @"";
            m.Web = @"ALL";
            m.Profile = @"form_應用系統設定.xml";
            m.Create_ID = @"CTBCBank\AST4";
            m.Create_Name = @"CTBCBan\AST4";
            m.Time_Create = @"2014-08-08 18:18:18";
            m.Update_ID = Account;
            m.Update_Name = @"CTBC\IT";
            m.Time_Update = DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToShortTimeString();
            m.NodeProfile = "";//@"form_AuthZ_List.xml";
            m.md_AppName = Config.GetValue("CUF_AppName");//@"CSFS"; 
            m.md_RoleSource = @"true";
            m.md_RoleCodeFile = "";//@"CSFS_Role"; 
            m.md_ActionCodeFile = "";//@"CSFS_Action";
            m.md_Ctrl = @"true";
            m.md_DeployMethod = @"D";
            m.md_FolderPath = "";
            m.md_Database = "";
            m.md_ServerIP = "";
            m.md_ServerPort = "";
            m.md_RootBaseDN = "";
            m.md_ServiceDN = "";
            m.md_ServicePWD = "";
            m.GUID = "";// @"0B17416C-3C18-4493-8DD8-F06532AD8DE0";
            m.md_AAAadmin = "";// @"cn=CSFSM0001,ou=CSFS,ou=APPs,o=CTCB";
            m.md_AAAauthorizer = "";// @"cn=CSFSM0003,ou=CSFS,ou=APPs,o=CTCB";
            m.md_AAAauditor = "";// @"cn=CSFSM0005,ou=CSFS,ou=APPs,o=CTCB";
            return m;
        }

        public IList<PARMMenuXMLNode> GetPARMMenuList()
        {
            try
            {
                //* 20150707修正同級別菜單下順序問題
                string sql = @"select *,dbo.GetAuthZList(ID) as md_AuthZ 
                            from PARMMenu 
                            where MenuType <> 'P' order by MenuLevel,MenuSort ";
                return base.SearchList<PARMMenuXMLNode>(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public PARMMenuXML ConvertToXMLObject(List<PARMMenuXMLNode> allList)
        {
            _csfsRoleList = (IList<CSFSRole>)AppCache.Get("CSFSRole");
            PARMMenuXML newPARMMenu = new PARMMenuXML();
            List<PARMMenuXMLNode> selList = new List<PARMMenuXMLNode>();
            int lev = 1;
            //--先取出最上層Level=1的Menu-------------------------------------------------
            selList = (from m in allList
                       where m.MenuLevel == lev
                       select m).ToList();
            //---------------------------------------------------
            foreach (PARMMenuXMLNode n in selList)
            {
                n.md_AuthZ = FormatAuthZ(n.md_AuthZ);
                n.md_AuthZ = HttpUtility.UrlEncode(n.md_AuthZ, Encoding.UTF8);
                n.SubMenu = ConvertToXMLObjectNode(allList, lev + 1, n.ID);
                newPARMMenu.Menu.Add(n);
            }
            return newPARMMenu;
        }

        public List<PARMMenuXMLNode> ConvertToXMLObjectNode(List<PARMMenuXMLNode> allList, int lev, int parent)
        {
            List<PARMMenuXMLNode> _list = new List<PARMMenuXMLNode>();
            List<PARMMenuXMLNode> selList = new List<PARMMenuXMLNode>();
            //--同層Menu List先排序好-------------------------------------------------
            selList = (from m in allList
                       where m.MenuLevel == lev && m.Parent == parent
                       orderby m.MenuSort
                       select m).ToList();
            //---------------------------------------------------
            foreach (PARMMenuXMLNode n in selList)
            {
                n.md_AuthZ = FormatAuthZ(n.md_AuthZ);
                n.md_AuthZ = HttpUtility.UrlEncode(n.md_AuthZ, Encoding.UTF8);
                n.SubMenu = ConvertToXMLObjectNode(allList, lev + 1, n.ID);
                _list.Add(n);
            }
            return _list;
        }

        public void DeployAuthZ(string appAuthZ)
        {
            try
            {
                //CheckAuthZ.AppName=CSFS是否已存在
                string sqlStr = "";
                sqlStr = @"select count(0) from AuthZ where AppName=@AppName";
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@AppName", Config.GetValue("CUF_AppName").Trim()));
                int authZCnt = 0;
                authZCnt = int.Parse(base.ExecuteScalar(sqlStr).ToString());

                //如果AuthZ.AppName=CSFS已存在,則備份與新增一筆AuthZ
                if (authZCnt > 0)
                {
                    IDbConnection dbConnection = base.OpenConnection();
                    IDbTransaction dbTransaction = null;
                    try
                    {
                        using (dbConnection)
                        {
                            dbTransaction = dbConnection.BeginTransaction();
                            sqlStr = @"--先將目前版本AuthZ的AppName=AppName+Version
                                        declare @NowVersion varchar(10)
                                        declare @NewVersion int 
                                        set @NowVersion= (select Cast(isnull([Version],0) as varchar(10)) from AuthZ where  AppName=@AppName);
                                        set @NewVersion= (select max(isnull([Version],0))+1 from AuthZ where  AppName=@AppName);
                                        ----------------            
                                        update AuthZ set AppName = AppName + @NowVersion,ModifiedDate=GETDATE(),ModifiedUser=@ModifiedUser where AppName=@AppName ;
                                        --新增新的MenuTree XML到AuthZ.AppAuthZ欄位上 與 RoleXML 到AuthZ.AppRoles欄位上
                                        insert into AuthZ (AppName,[Version],AppAuthZ,AppRoles,CreatedUser,ModifiedUser,AppInformation) values (
                                        @AppName,@NewVersion,@AppAuthZ,@AppRoles,@ModifiedUser,@ModifiedUser,@AppInformation);
                                        --刪除最近5版後的menu版本--
                                        declare @mf int
                                        declare @cf int
                                        select @mf=max(isnull([Version],0)) from AuthZ
                                        if (@mf > 5)
                                        begin
                                            set @cf = @mf - 5
                                            delete AuthZ where [Version] <= @cf 
                                        end
                                        ";
                            base.Parameter.Clear();
                            base.Parameter.Add(new CommandParameter("@AppAuthZ", appAuthZ));
                            base.Parameter.Add(new CommandParameter("@AppRoles", GetAppRoles()));
                            base.Parameter.Add(new CommandParameter("@AppInformation", System.Net.Dns.GetHostName() + ".SyncService"));
                            base.Parameter.Add(new CommandParameter("@ModifiedUser", Account));
                            base.Parameter.Add(new CommandParameter("@AppName", Config.GetValue("CUF_AppName").Trim()));
                            base.ExecuteNonQuery(sqlStr, dbTransaction);
                            dbTransaction.Commit();
                        }
                    }
                    catch (Exception ex)
                    {
                        try
                        {
                            dbTransaction.Rollback();
                        }
                        catch (Exception ex2)
                        {
                        }
                        throw ex;
                    }
                }
                else
                {
                    //如果AuthZ.AppName=CSFS不存在,則新增一筆AuthZ
                    IDbConnection dbConnection = base.OpenConnection();
                    IDbTransaction dbTransaction = null;
                    try
                    {
                        using (dbConnection)
                        {
                            dbTransaction = dbConnection.BeginTransaction();
                            sqlStr = @"declare @NewVersion int
                                        set @NewVersion= (select max(isnull([Version],0))+1 from AuthZ where  AppName=@AppName);
                                        insert into AuthZ (AppName,[Version],AppAuthZ,AppRoles,CreatedUser,ModifiedUser,AppInformation) values (
                                        @AppName,@NewVersion,@AppAuthZ,@AppRoles,@ModifiedUser,@ModifiedUser,@AppInformation)";
                            base.Parameter.Clear();
                            base.Parameter.Add(new CommandParameter("@AppAuthZ", appAuthZ));
                            base.Parameter.Add(new CommandParameter("@AppRoles", GetAppRoles()));
                            base.Parameter.Add(new CommandParameter("@AppInformation", System.Net.Dns.GetHostName() + ".SyncService"));
                            base.Parameter.Add(new CommandParameter("@ModifiedUser", Account));
                            base.Parameter.Add(new CommandParameter("@AppName", Config.GetValue("CUF_AppName").Trim()));
                            base.ExecuteNonQuery(sqlStr, dbTransaction);
                            dbTransaction.Commit();
                        }
                    }
                    catch (Exception ex)
                    {
                        try
                        {
                            dbTransaction.Rollback();
                        }
                        catch (Exception ex2)
                        {
                        }
                        throw ex;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void SortMenuList(List<PARMMenuXMLNode> list, int lev, int parent)
        {
            List<PARMMenuXMLNode> _list = new List<PARMMenuXMLNode>();
            var selList = (from m in list
                           where m.MenuLevel == lev && m.Parent == parent
                           orderby m.MenuSort
                           select m).ToList();
            foreach (PARMMenuXMLNode n in selList)
            {
                _menuListSorted.Add(n);
                SortMenuList(list, lev + 1, n.ID);
            }
        }

        //取得Menu清單
        public List<PARMMenuXMLNode> GetMenuList(string menuType)
        {
            try
            {
                List<PARMMenuXMLNode> _list = new List<PARMMenuXMLNode>();
                List<PARMMenuXMLNode> _Newlist = new List<PARMMenuXMLNode>();
                string sql = "";
                if (menuType == "M")
                {
                    //M=Menu才需取得再授權資料
                    sql = @"select ID,MenuType,MenuSort,MenuLevel,md_funcID,dbo.GetAuthZModifyList(ID) as ModifyRole,Parent,TITLE,md_URL,dbo.GetAuthZList(ID) as md_AuthZ from PARMMenu where MenuType=@MenuType ";
                }
                else if (menuType == "P")
                {
                    sql = @"select ID,MenuType,MenuSort,MenuLevel,md_funcID,Parent,TITLE,md_URL,dbo.GetAuthZMenuToPageList(ID) as md_AuthZ from PARMMenu where MenuType='M' ";
                }
                else
                {
                    //C=Controller與A=Action不需取得再授權資料
                    sql = @"select ID,MenuType,MenuSort,MenuLevel,md_funcID,Parent,TITLE,md_URL,dbo.GetAuthZList(ID) as md_AuthZ from PARMMenu where MenuType in ('C','A') order by TITLE ";
                }
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@MenuType", menuType));
                _list = base.SearchList<PARMMenuXMLNode>(sql).ToList();

                //取得角色名稱
                string newmd_AuthZ;
                bool hasModifyR = false;
                CTBC.FrameWork.Platform.User usr = (CTBC.FrameWork.Platform.User)HttpContext.Current.Session["LogonUser"];

                //MenuToPage 要顯示Menu目前所對應的Page清單之用
                if (menuType == "P")
                {
                    _menuPageList = GetPageList();
                }

                foreach (PARMMenuXMLNode item in _list)
                {
                    //--再授權----------------------------------------------------------------
                    //M=Menu才需取得再授權資料
                    if (menuType == "M")
                    {
                        //判斷目前使用者角色是否可編輯此menu
                        //(如:再授權需在LDAP上擁有特定角色,並記錄在PARMMenu.ModifyRole這欄位清單上,方可編輯此menu)
                        //PARMMenu.ModifyRole這欄位值舉例=CSFSM0001,CSFSM0002,CSFSK0002等等
                        if (!string.IsNullOrEmpty(item.ModifyRole.Trim()))
                        {
                            string[] aryMdfRole = item.ModifyRole.TrimEnd(',').Split(',');

                            hasModifyR = false;
                            for (int i = 0; i < aryMdfRole.Length; i++)
                            {
                                hasModifyR = usr.IsExistAppRole(aryMdfRole[i].Trim());
                                if (hasModifyR)
                                    break;
                            }
                        }
                    }
                    else
                    {
                        //C=Controller與A=Action與P=Page,不需取得再授權資料
                        hasModifyR = true;
                    }
                    //--再授權----------------------------------------------------------------

                    //如果目前使用者角色可編輯此menu(取得再授權),才需組合出AuthZ List---------------------
                    if (hasModifyR)
                    {
                        //取得newmd_AuthZ=Role Name List
                        newmd_AuthZ = "";
                        if (!string.IsNullOrEmpty(item.md_AuthZ.Trim()))
                        {
                            string[] aryAuthZID = item.md_AuthZ.TrimEnd(',').Split(',');

                            foreach (string s in aryAuthZID)
                            {
                                if (menuType == "P")
                                {
                                    //如果MenuToPage,AuthZ List應為Page 名稱,如:Group1Name,Group2Name,...
                                    newmd_AuthZ += ConvertToPageName(s.Trim()) + ",";
                                }
                                else
                                {
                                    //如果MenuToRole,AuthZ List應為角色 名稱 ,如:徵信主管B群,徵審-徵信經辦,系統管理者...
                                    newmd_AuthZ += ConvertToRoleName(s.Trim()) + ",";
                                }
                            }
                            item.md_AuthZ = newmd_AuthZ.TrimEnd(',');
                        }
                        _Newlist.Add(item);
                    }
                    //---------------------------------------------------------------------------
                }
                _menuListSorted = new List<PARMMenuXMLNode>();
                SortMenuList(_Newlist, 1, 1);
                return _menuListSorted;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //取得一筆Menu
        public PARMMenuXMLNode GetOneMenu(string menuID, string menuType, string settingType)
        {
            try
            {
                int _menuID = 1;
                int i = 0;
                bool isInt = int.TryParse(menuID, out i);
                if (isInt)
                {
                    _menuID = (string.IsNullOrEmpty(menuID)) ? 1 : Convert.ToInt32(menuID);
                    IList<PARMMenuXMLNode> _list = new List<PARMMenuXMLNode>();
                    string sql = "";
                    switch (settingType)
                    {
                        case "1":
                            sql = @"select dbo.GetAuthZList(ID) as md_AuthZ,ID,TITLE,md_FuncID,md_URL from PARMMenu where MenuType=@MenuType and ID=@menuID ";
                            break;
                        case "2":
                            sql = @"select dbo.GetAuthZMenuToPageLog(ID) as md_AuthZ,ID,TITLE,md_FuncID,md_URL from PARMMenu where MenuType=@MenuType and ID=@menuID ";
                            break;
                        case "3":
                            sql = @"select dbo.GetAuthZPageModifyList(ID) as md_AuthZ ,ID,TITLE,md_FuncID,md_URL from PARMMenu where MenuType=@MenuType and ID=@menuID";
                            break;
                    }
                    base.Parameter.Clear();
                    base.Parameter.Add(new CommandParameter("@menuID", _menuID));
                    base.Parameter.Add(new CommandParameter("@MenuType", menuType));
                    _list = base.SearchList<PARMMenuXMLNode>(sql);
                    if (_list.Count > 0)
                    {
                        _list[0].md_AuthZ.TrimEnd(',');
                        return _list[0];
                    }
                    else
                    {
                        return new PARMMenuXMLNode();
                    }
                }
                else
                {
                    return new PARMMenuXMLNode();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //取得所有角色清單+是否被授權
        public IList<CSFSRole> GetCheckedRoleList(string authZ)
        {
            IList<CSFSRole> list = (IList<CSFSRole>)AppCache.Get("CSFSRole");
            if (!string.IsNullOrEmpty(authZ))
            {
                string[] aryRoleID = authZ.TrimEnd(',').Split(',');
                foreach (CSFSRole item in list)
                {
                    //如果也有授權
                    if (aryRoleID.Contains(item.RoleID.Trim()))
                    {
                        item.Checked = "Y";
                    }
                    else item.Checked = "N";
                }
            }
            else
            {
                foreach (CSFSRole item in list)
                {
                    item.Checked = "N";
                }
            }
            return list;
        }

        //取得所有Page清單+是否被授權
        public List<PARMMenuXMLNode> GetCheckedMenuToPageList(List<PARMMenuXMLNode> menuList)
        {
            List<PARMMenuXMLNode> list = GetPageList();
            if (list != null && menuList != null)
            {
                //有Action/Controller被授權給某個page
                if (menuList.Any())
                {
                    int[] aryList = new int[menuList.Count];
                    int i = 0;
                    foreach (PARMMenuXMLNode p in menuList)
                    {
                        aryList[i] = p.ID;
                        i++;
                    }


                    foreach (PARMMenuXMLNode item in list)
                    {
                        if (aryList.Contains(item.ID))
                        {
                            item.ActionChecked = "Y";
                        }
                        else
                        {
                            item.ActionChecked = "N";
                        }
                    }
                }
                else
                {
                    //如果沒有任何Action/Controller被授權給某個page
                    foreach (PARMMenuXMLNode item in list)
                    {
                        item.ActionChecked = "N";
                    }
                }
            }
            else
            {
                foreach (PARMMenuXMLNode item in list)
                {
                    item.ActionChecked = "N";
                }
            }
            return list;
        }

        /// <summary>
        /// Save Menu To Role
        /// </summary>
        /// <param name="menuID">帶儲存的PARMMenu.ID</param>
        /// <param name="authZ">新設定的角色或Controller/Action清單(ex:CSFSM0001,CSFSM0002...或是1130,1131...等皆以逗號隔開</param>
        /// <param name="menyType">PARMMenu.MenuType</param>
        /// <param name="settingType">PARMMenuSetting.Type</param>
        /// <returns></returns>
        public int SaveOneMenu(int menuID, string authZ, string menyType, string settingType)
        {
            string ActionLabel = "";
            PARMMenuXMLNode bfMenu = new PARMMenuXMLNode();

            //--------------------------------------------------
            //先暫存before log
            //--------------------------------------------------
            switch (settingType)
            {
                case "1":
                    //取出MenuToAction before log            
                    bfMenu = GetOneMenu(menuID.ToString(), "M", "1");
                    ActionLabel = "*";
                    break;
                case "2":
                    //取出MenuToPage before log            
                    bfMenu = GetOneMenu(menuID.ToString(), "M", "2");
                    break;
                case "3":
                    //取出PageToAction  after log
                    bfMenu = GetOneMenu(menuID.ToString(), "P", "3");
                    break;
                case "4":
                    break;
                default:
                    break;
            }
            //--------------------------------------------------

            int rtn = 0;
            IDbConnection dbConnection = base.OpenConnection();
            IDbTransaction dbTransaction = null;
            try
            {
                using (dbConnection)
                {
                    dbTransaction = dbConnection.BeginTransaction();
                    string sql = "";
                    //先刪除在PARMMenuSetting中,@Type + MasterID 對應的所有資料
                    sql = @"delete PARMMenuSetting where Type=@Type and MasterID=@menuID;";
                    if (!string.IsNullOrEmpty(authZ))
                    {
                        string[] aryRoleID = authZ.TrimEnd(',').Split(',');
                        //在PARMMenuSetting中,新增@Type + MasterID 新設定的所有對應資料
                        for (int i = 0; i < aryRoleID.Length; i++)
                        {
                            sql = sql + @"insert into PARMMenuSetting (Type,MasterID,DetailID,ActionLabel,CreatedUser,ModifiedUser) values(@Type,@menuID,@aryRoleID" + i + @",@ActionLabel,@CreatedUser,@CreatedUser); ";
                        }
                        base.Parameter.Clear();
                        for (int i = 0; i < aryRoleID.Length; i++)
                        {
                            base.Parameter.Add(new CommandParameter("@aryRoleID" + i, aryRoleID[i].Trim()));
                        }
                    }
                    else
                    {
                        base.Parameter.Clear();
                    }

                    base.Parameter.Add(new CommandParameter("@menuID", menuID));
                    base.Parameter.Add(new CommandParameter("@Type", settingType));
                    base.Parameter.Add(new CommandParameter("@ActionLabel", ActionLabel));
                    base.Parameter.Add(new CommandParameter("@CreatedUser", Account));
                    rtn = base.ExecuteNonQuery(sql, dbTransaction);
                    dbTransaction.Commit();
                }

                //--------------------------------------------------
                //如果無錯誤已commit,則記錄before,after log
                //--------------------------------------------------
                if (rtn >= 0)
                {
                    string logRecNo = Guid.NewGuid().ToString();
                    //before log
                    SaveLog(bfMenu, logRecNo, "Before");

                    PARMMenuXMLNode menu = new PARMMenuXMLNode();
                    switch (settingType)
                    {
                        case "1":
                            //取出MenuToAction after log
                            menu = GetOneMenu(menuID.ToString(), "M", "1");
                            break;
                        case "2":
                            //取出MenuToAction after log
                            menu = GetOneMenu(menuID.ToString(), "M", "2");
                            break;
                        case "3":
                            //取出PageToAction  after log
                            menu = GetOneMenu(menuID.ToString(), "P", "3");
                            break;
                        case "4":
                            break;
                        default:
                            break;
                    }

                    SaveLog(menu, logRecNo, "After");
                }
                //--------------------------------------------------

                return rtn;
            }
            catch (Exception ex)
            {
                try
                {
                    dbTransaction.Rollback();
                }
                catch (Exception ex2)
                {
                }
                throw ex;
            }
        }

        //取得PARMMenu 中Page的清單
        public List<PARMMenuXMLNode> GetPageList()
        {
            try
            {
                List<PARMMenuXMLNode> _list = new List<PARMMenuXMLNode>();
                string sql = @"select ID,TITLE,md_FuncID from PARMMenu where MenuType='P'";
                base.Parameter.Clear();
                _list = base.SearchList<PARMMenuXMLNode>(sql).ToList();
                return _list;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<PARMMenuXMLNode> GetOnePage(string pageID)
        {
            List<PARMMenuXMLNode> _list = new List<PARMMenuXMLNode>();
            string sql = @"with data
                            as
                            (
                            	select DetailID from PARMMenuSetting where [Type]=3 and MasterID=@pageID
                            )
                            select b.ID from data a
                            left join PARMMenu b on a.DetailID = b.ID  ";
            base.Parameter.Clear();
            base.Parameter.Add(new CommandParameter("@pageID", pageID));
            _list = base.SearchList<PARMMenuXMLNode>(sql).ToList();
            return _list;
        }

        public List<PARMMenuXMLNode> GetOneMenuToPage(string menuID)
        {
            List<PARMMenuXMLNode> _list = new List<PARMMenuXMLNode>();
            string sql = @"select DetailID as ID from PARMMenuSetting where MasterID=@menuID and [Type]='2'  ";
            base.Parameter.Clear();
            base.Parameter.Add(new CommandParameter("@menuID", menuID));
            _list = base.SearchList<PARMMenuXMLNode>(sql).ToList();
            return _list;
        }

        public List<PARMMenuXMLNode> GetAllActionControllerList()
        {
            string sql = @"select ID,TITLE,md_funcID,MenuType from PARMMenu where MenuType in ('C','A')";
            base.Parameter.Clear();
            return base.SearchList<PARMMenuXMLNode>(sql).ToList();
        }

        //取得所有Action/Controller清單+是否被授權給某個page
        public List<PARMMenuXMLNode> GetCheckedActionList(List<PARMMenuXMLNode> menuList)
        {
            List<PARMMenuXMLNode> list = GetMenuList("A");//GetAllActionControllerList();

            if (list != null && menuList != null)
            {
                //有Action/Controller被授權給某個page
                if (menuList.Any())
                {
                    int[] aryList = new int[menuList.Count];
                    int i = 0;
                    foreach (PARMMenuXMLNode p in menuList)
                    {
                        aryList[i] = p.ID;
                        i++;
                    }


                    foreach (PARMMenuXMLNode item in list)
                    {
                        if (aryList.Contains(item.ID))
                        {
                            item.ActionChecked = "Y";
                        }
                        else
                        {
                            item.ActionChecked = "N";
                        }
                    }
                }
                else
                {
                    //如果沒有任何Action/Controller被授權給某個page
                    foreach (PARMMenuXMLNode item in list)
                    {
                        item.ActionChecked = "N";
                    }
                }
            }
            else
            {
                foreach (PARMMenuXMLNode item in list)
                {
                    item.ActionChecked = "N";
                }
            }
            return list;
        }

        public void SaveLog(PARMMenuXMLNode menu, string guid, string afFlag)
        {
            CSFSLogBIZ.WriteLog(new CSFSLog()
            {
                Title = "PARMMenu",
                Message = "LogNo=" + guid + ";ID=" + menu.ID + ";TITLE=" + menu.TITLE + ";Func_ID=" + menu.md_FuncID + ";URL=" + menu.md_URL + ";" + afFlag + "AuthZ=" + menu.md_AuthZ.TrimEnd(',')
            });
        }

        public string TrimEnd(string s)
        {
            if (!string.IsNullOrEmpty(s))
                s = s.TrimEnd(',');
            return s;
        }

        /// <summary>
        /// 格式化XML屬性AuthZ的值        
        /// </summary>
        /// <param name="strAuthZ"></param>
        /// <returns></returns>
        // AuthZ的值舉例:<Data><Role Title=\"系統管理者\">cn=CSFSM0001,ou=CSFS,ou=APPs,o=CTCB</Role><Actions><![CDATA[V]]></Actions></Data>
        public string FormatAuthZ(string strAuthZ)
        {
            strAuthZ = TrimEnd(strAuthZ);
            string rtn = "";
            string strRoleName = "";
            if (!string.IsNullOrEmpty(strAuthZ))
            {
                //有角色被授權
                string[] aryRoleID = strAuthZ.Split(',');
                int i = 0;
                rtn = "<Data>";
                foreach (string item in aryRoleID)
                {
                    strRoleName = "";
                    strRoleName = (from m in _csfsRoleList
                                   where m.RoleID == aryRoleID[i]
                                   select m.RoleName).FirstOrDefault();
                    rtn += "<Data><Role Title=\"" + "" + "\">cn=" + aryRoleID[i] + "," + Config.GetValue("LDAPServiceDN") + "</Role><Actions><![CDATA[*]]></Actions></Data>";
                    i++;
                }
                rtn += "</Data>";
            }
            else
            {
                //若無任何角色被授權
                rtn = "<Data></Data>";
            }
            return rtn;
        }

        //轉換出Page Name
        public string ConvertToPageName(string pID)
        {
            string pageName = "";
            int pageID = (string.IsNullOrEmpty(pID)) ? 1 : Convert.ToInt32(pID);
            pageName = (from p in _menuPageList
                        where p.ID == pageID
                        select p.TITLE).FirstOrDefault();
            return pageName;
        }

        //轉換出Role Name
        public string ConvertToRoleName(string roleID)
        {
            string strRoleName = "";
            _csfsRoleList = (IList<CSFSRole>)AppCache.Get("CSFSRole");
            strRoleName = (from m in _csfsRoleList
                           where m.RoleID == roleID
                           select m.RoleName).FirstOrDefault();
            return strRoleName;
        }

        /// <summary>
        /// 取得AuthZ.AppRoles所需的Role XML
        /// </summary>
        /// <returns>Role XML 字串</returns>
        public string GetAppRoles()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(@"<?xml version=""1.0""?>");
            sb.Append(@"<Codes NodeName=""CSFS_Role"" Title=""CSFS Roles"" Create_ID=""CTBCBank\AST4""");
            sb.Append(@" Create_Name=""CTBCBank\AST4"" Time_Create=""2014-08-08 18:18:18"" Update_ID=""CTBC\" + Account + @"""");
            sb.Append(@" Update_Name=""CTBC\IT"" Time_Update=""" + DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToShortTimeString() + @""" InMetadata=""false""");
            sb.Append(@" withParent=""false"" ParentCodeFile="""" LatestSync=""2014-08-08 18:18:18"">");
            foreach (CSFSRole r in _csfsRoleList)
            {
                sb.Append(@"<CSFS_Role Status=""ON"" Code=""cn=" + r.RoleID + "," + Config.GetValue("LDAPServiceDN") + "\" Parent=\"\">" + r.RoleName + "</CSFS_Role>");
            }
            sb.Append("</Codes>");
            return sb.ToString();
        }

        //------------------------------------------------
        // PARMMenuNew管理維護
        //------------------------------------------------

        /// <summary>
        /// 取得PARMMenuNew查詢後資料
        /// </summary>
        /// <param name="qryCsfsVO"></param>
        /// <param name="pageIndex"></param>
        /// <returns></returns>
        public IList<PARMMenuVO> GetQueryList(PARMMenuVO qryCsfsVO, int pageIndex, string strSortExpression, string strSortDirection)
        {
            try
            {
                base.PageIndex = pageIndex;
                string sqlStr = "";
                string sqlStrWhere = "";
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@pageS", (base.PageSize * (base.PageIndex - 1)) + 1));
                base.Parameter.Add(new CommandParameter("@pageE", base.PageSize * base.PageIndex));

                if (!string.IsNullOrEmpty(qryCsfsVO.TITLE))
                {
                    sqlStrWhere += @" and TITLE like @TITLE ";
                    base.Parameter.Add(new CommandParameter("@TITLE", "%" + qryCsfsVO.TITLE + "%"));
                }

                if (!string.IsNullOrEmpty(qryCsfsVO.md_FuncID))
                {
                    sqlStrWhere += @" and md_FuncID like @md_FuncID ";
                    base.Parameter.Add(new CommandParameter("@md_FuncID", "%" + qryCsfsVO.md_FuncID + "%"));
                }

                sqlStr += @";with T1 
                            as
                            (
                                select ID,MenuType,MenuLevel,Parent,TITLE,md_FuncID,md_URL,MenuSort from PARMMenu with(nolock)
                                where 1=1 " + sqlStrWhere + @" 
                            ),T2 as
                            (
                                select *, row_number() over (order by " + strSortExpression + " " + strSortDirection + @") RowNum
                                from T1
                            ),T3 as 
                            (
                                select *,(select max(RowNum) from T2) maxnum from T2 
                                where rownum between @pageS and @pageE
                            )
                            select * from T3";

                IList<PARMMenuVO> _ilsit = base.SearchList<PARMMenuVO>(sqlStr);

                if (_ilsit.Count > 0)
                {
                    base.DataRecords = _ilsit[0].maxnum;
                }
                else
                {
                    base.DataRecords = 0;
                    _ilsit = new List<PARMMenuVO>();
                }
                return _ilsit;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 新增一筆PARMMenu設定
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public int Create(PARMMenuVO model)
        {
            //取web.config中設定預設menu管理者角色
            string mgrRole = Config.GetValue("AAAMgr");
            mgrRole = (string.IsNullOrEmpty(mgrRole)) ? "CSFS001" : mgrRole;

            int rtn = 0;
            IDbConnection dbConnection = base.OpenConnection();
            IDbTransaction dbTransaction = null;
            try
            {
                using (dbConnection)
                {
                    dbTransaction = dbConnection.BeginTransaction();
                    string sqlStr = @"declare @ID int 
                                  select @ID=max(ID)+1 from PARMMenu
                                  insert into PARMMenu(
                                            ID,
                                            MenuType,
                                            MenuLevel,
                                            Parent,
                                            TITLE,
                                            md_FuncID,
                                            md_URL,
                                            md_Ctrl,
                                            md_LogEntry,
                                            md_EntryLogLevel,
                                            md_LogExit,
                                            md_ExitLogLevel,
                                            md_ExcPolicy,
                                            MenuSort,
                                            CreatedUser,
                                            ModifiedUser
                                            )
                                    values(
                                            @ID,
                                            @MenuType,
                                            @MenuLevel,
                                            @Parent,
                                            @TITLE,
                                            @md_FuncID,
                                            @md_URL,
                                            'true',
                                            'true',
                                            '3',
                                            'true',
                                            '3',
                                            'S',
                                            @MenuSort,
                                            @User,
                                            @User
                                            );";

                    base.Parameter.Clear();
                    if (model.MenuType == "M")
                    {
                        sqlStr = sqlStr + @"insert into PARMMenuSetting 
                                                (Type,MasterID,DetailID,CreatedUser,ModifiedUser)
                                            values('4',@ID,@DetailID,@User,@User)";
                        base.Parameter.Add(new CommandParameter("@DetailID", mgrRole));
                    }
                    base.Parameter.Add(new CommandParameter("@MenuType", model.MenuType));
                    base.Parameter.Add(new CommandParameter("@MenuLevel", model.MenuLevel));
                    base.Parameter.Add(new CommandParameter("@Parent", model.Parent));
                    base.Parameter.Add(new CommandParameter("@TITLE", model.TITLE));
                    base.Parameter.Add(new CommandParameter("@md_FuncID", model.md_FuncID));
                    base.Parameter.Add(new CommandParameter("@md_URL", model.md_URL));
                    base.Parameter.Add(new CommandParameter("@MenuSort", model.MenuSort));
                    base.Parameter.Add(new CommandParameter("@User", Account));
                    rtn = base.ExecuteNonQuery(sqlStr, dbTransaction);
                    dbTransaction.Commit();
                }
                return rtn;
            }
            catch (Exception ex)
            {
                try
                {
                    dbTransaction.Rollback();
                }
                catch (Exception ex2)
                {
                }
                throw ex;
            }
        }

        /// <summary>
        /// 取得一筆PARMMenu設定
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public PARMMenuVO Select(int id)
        {
            try
            {
                string sqlStr = @"select * from PARMMenu where ID=@ID";

                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@ID", id));

                IList<PARMMenuVO> list = base.SearchList<PARMMenuVO>(sqlStr);
                if (list != null)
                {
                    if (list.Count > 0)
                    {
                        return list[0];
                    }
                    else
                    {
                        return new PARMMenuVO();
                    }
                }
                else
                {
                    return new PARMMenuVO();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 修改一筆PARMMenu設定
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public int Edit(PARMMenuVO model)
        {
            int rtn = 0;
            IDbConnection dbConnection = base.OpenConnection();
            IDbTransaction dbTransaction = null;
            try
            {
                using (dbConnection)
                {
                    dbTransaction = dbConnection.BeginTransaction();
                    string sqlStr = @"update PARMMenu set 
                                            MenuType=@MenuType,
                                            MenuLevel=@MenuLevel,
                                            Parent=@Parent,
                                            TITLE=@TITLE,
                                            md_FuncID=@md_FuncID,
                                            md_URL=@md_URL,
                                            MenuSort=@MenuSort,
                                            ModifiedUser=@User,
                                            ModifiedDate=GetDate()
                                    where ID=@ID";

                    base.Parameter.Clear();
                    base.Parameter.Add(new CommandParameter("@ID", Convert.ToString(model.ID)));
                    base.Parameter.Add(new CommandParameter("@MenuType", Convert.ToString(model.MenuType)));
                    base.Parameter.Add(new CommandParameter("@MenuLevel", Convert.ToString(model.MenuLevel)));
                    base.Parameter.Add(new CommandParameter("@Parent", Convert.ToString(model.Parent)));
                    base.Parameter.Add(new CommandParameter("@TITLE", Convert.ToString(model.TITLE)));
                    base.Parameter.Add(new CommandParameter("@md_FuncID", Convert.ToString(model.md_FuncID)));
                    base.Parameter.Add(new CommandParameter("@md_URL", Convert.ToString(model.md_URL)));
                    base.Parameter.Add(new CommandParameter("@MenuSort", Convert.ToString(model.MenuSort)));
                    base.Parameter.Add(new CommandParameter("@User", Convert.ToString(Account)));
                    rtn = base.ExecuteNonQuery(sqlStr, dbTransaction);
                    dbTransaction.Commit();
                }
                return rtn;
            }
            catch (Exception ex)
            {
                try
                {
                    dbTransaction.Rollback();
                }
                catch (Exception ex2)
                {
                }
                throw ex;
            }
        }

        /// <summary>
        /// 刪除一筆PARMMenu設定
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// PARMMenuSetting.Type=1 > Menu To Role 設定
        /// PARMMenuSetting.Type=2 > Menu To MainPage 設定
        /// PARMMenuSetting.Type=3 > MainPage To Action/Controller 設定 
        /// PARMMenuSetting.Type=4 > Menu To Role 設定(可維護Menu的權限)
        public int Delete(int id)
        {
            int rtn = 0;
            IDbConnection dbConnection = base.OpenConnection();
            IDbTransaction dbTransaction = null;
            try
            {
                using (dbConnection)
                {
                    dbTransaction = dbConnection.BeginTransaction();
                    string sqlStr = @"declare @MenuType char(1)
                                        select @MenuType=MenuType from PARMMenu where ID=@ID
                                        delete PARMMenu where ID=@ID and MenuType=@MenuType
                                        if(@MenuType='M')--此筆資料為Menu
                                        begin
                                            delete PARMMenuSetting where Type=1 and MasterID=@ID
                                            delete PARMMenuSetting where Type=2 and MasterID=@ID
                                            delete PARMMenuSetting where Type=4 and MasterID=@ID
                                        end
                                        if(@MenuType='P')--此筆資料為MainPage
                                        begin
                                            delete PARMMenuSetting where Type=2 and DetailID=@ID
                                            delete PARMMenuSetting where Type=3 and MasterID=@ID
                                        end
                                        if(@MenuType='C')--此筆資料為Controller
                                        begin
                                            delete PARMMenuSetting where Type=3 and DetailID=@ID
                                        end
                                        if(@MenuType='A')--此筆資料為Action
                                        begin
                                            delete PARMMenuSetting where Type=3 and DetailID=@ID
                                        end";

                    base.Parameter.Clear();
                    base.Parameter.Add(new CommandParameter("@ID", id));
                    rtn = base.ExecuteNonQuery(sqlStr, dbTransaction);
                    dbTransaction.Commit();

                    //動態記錄至CSFSLog Sample
                    CSFSLogBIZ.WriteLog(new CSFSLog()
                    {
                        Title = "PARMMenu",
                        Message = "PARMMenu ID=" + id + " deleted"
                    });
                }
                return rtn;
            }
            catch (Exception ex)
            {
                try
                {
                    dbTransaction.Rollback();
                }
                catch (Exception ex2)
                {
                }
                throw ex;
            }
        }
    }
}
