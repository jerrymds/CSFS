using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Mail;
using Novell.Directory.Ldap;
using System.Data;
using System.Data.SqlClient;
using CTCB.NUMS.Batch.Logger;

namespace CTCB.NUMS.Batch.LDAPSync
{
    public class Program
    {
        static List<NUMSEmployee> listEmp=new List<NUMSEmployee>();
        static List<NUMSRole> listRole = new List<NUMSRole>();//本系統在LDAP上的使用者List
        static string sLogFile = @Config.LogPath + "LDAPSync-" + DateTime.Now.ToString("yyyyMMdd") + ".log";    //Log 檔案路徑
        static Log logger = new Log(sLogFile);    //Log 檔案路徑
        static StringBuilder mailContent=new StringBuilder();                                                                                //發送監控批次mail的內容

        public static void Main(string[] args)
        {
            try
            {
                logger.WriteLogs("-------- Start of LDAP Sync ----------------");
                int argsLength = args.Length;
                if (argsLength == 0)
                {
                    GetUser();

                    UpdateUserFile();

                    GetRole();

                    UpdateRoleFile();
                }

                if (argsLength != 0)
                {
                    string argment = args[0].ToString();
                    if (argment == "SyncNUMSFromCSVFile")
                    {                        
                        GetCSVFile();
                    }
                }

                SyncNUMSEmployee();

                SyncNUMSEmployeeToRole();

                logger.WriteLogs("LDAP Sync Finish");
                logger.WriteLogs("-------- End of LDAP Sync ----------------");
                
                SendMonitorEmail();
            }
            catch (Exception ex)
            {
                logger.WriteLogs(ex.Message + " " + ex.StackTrace);
            }
        }

        /// <summary>
        /// 自LDAP上取得本系統使用者資訊
        /// </summary>
        public static void GetUser()
        {
            ///連線LDAP
            //string[] userDNInfo = new string[2] { "", "(cn=" + userid + ")" };//{userDN,searchFilter)
            string[] searchFilter = new string[2] { "", "(&(" + Config.SearchFilter + "))" };//ex: "(&(cn=*))"
            string[] searchAttr = new string[6];
            searchAttr[0] = "cn";
            searchAttr[1] = "mail";
            searchAttr[2] = "fullName";
            searchAttr[3] = "ctcbEmployeeLevel";
            searchAttr[4] = "title";
            searchAttr[5] = "groupMembership";
            LdapConnection conn = new LdapConnection();
            LdapSearchConstraints cons = new LdapSearchConstraints();
            try
            {
                //Step1:get LDAP connection
                conn.Connect(Config.LDAPHost, Config.LDAPPort);

                //Step2:verify AP ID and AP password
                conn.Bind(Config.APDN, Config.APPwd);//(appDN, APPWD)

                Console.WriteLine("Conntection LDAP Server Success");
                logger.WriteLogs("Conntection LDAP Server Success");

                //Step3:search user DN
                cons.MaxResults = 0;
                LdapSearchResults lsc = conn.Search(Config.HrisRoot, LdapConnection.SCOPE_SUB, searchFilter[1], searchAttr, false, cons);
                LdapEntry userLdapEntry = null;

                int tt = 0;
                while (lsc.hasMore())
                {
                    tt++;
                    try
                    {
                        userLdapEntry = lsc.next();
                    }
                    catch (LdapException ex)
                    {
                        logger.WriteLogs(ex.Message + " " + ex.StackTrace);
                    }

                    //Step4:filter user role by ou=NUMS,ou=APPs,o=CTCB
                    string[] userLDAPGroups = GetLDAPGroups(userLdapEntry);
                    bool isUser = false;
                    if (userLDAPGroups != null)
                    {
                        foreach (string s in userLDAPGroups)
                        {
                            if (s.Contains(Config.APDN))
                            {
                                //20150520 horace CSFS在LDAP的角色長度=7 ,且要檢查=Y則加到listRole中,不檢查=N也要加到listRole中
                                if (Config.CheckRoleLength == "Y" && CheckRoleLength(s))
                                {
                                    isUser = true;
                                    break;
                                }
                                else if (Config.CheckRoleLength == "N")
                                {
                                    isUser = true;
                                    break;
                                }
                                else
                                    isUser = false;
                            }
                        }
                    }

                    //是本系統user,則取出該user在LDAP中的各項屬性(Name,Email...)
                    if (isUser)
                    {
                        NUMSEmployee employee = new NUMSEmployee
                        {
                            EmpID = "",
                            EmpName = "",
                            EmpEMail = "",
                            EmpLevel = "",
                            EmpTitle = "",
                            EmpGroups= "",
                            OnBoard = true
                        };
                        LdapAttributeSet attributeSet = userLdapEntry.getAttributeSet();

                        LdapAttribute eid = attributeSet.getAttribute("cn");
                        if (eid != null)
                            employee.EmpID = eid.StringValue;

                        LdapAttribute ename = attributeSet.getAttribute("fullName");
                        if (ename != null)
                            employee.EmpName = ename.StringValue;

                        LdapAttribute email = attributeSet.getAttribute("mail");
                        if (email != null)
                            employee.EmpEMail = email.StringValue;

                        LdapAttribute elevel = attributeSet.getAttribute("ctcbEmployeeLevel");
                        if (elevel != null)
                            employee.EmpLevel = elevel.StringValue;

                        LdapAttribute etitle = attributeSet.getAttribute("title");
                        if (etitle != null)
                            employee.EmpTitle = etitle.StringValue;
                        if (userLDAPGroups != null)
                        {
                            for (int i = 0; i < userLDAPGroups.Count(); i++)
                            {
                                if (userLDAPGroups[i].IndexOf("," + Config.APDN) > 0)
                                {
                                    //取出如NUMS001;NUMS002;NUMS003...如此格式的角色清單
                                    if (Config.CheckRoleLength == "Y" && CheckRoleLength(userLDAPGroups[i]))
                                    {
                                        employee.EmpGroups += userLDAPGroups[i].Replace("," + Config.APDN, "") + ";";
                                    }
                                    else if (Config.CheckRoleLength == "N")
                                    {
                                        employee.EmpGroups += userLDAPGroups[i].Replace("," + Config.APDN, "") + ";";                                    
                                    }
                                }
                            }
                            employee.EmpGroups = employee.EmpGroups.Replace("cn=", "");
                        }

                        //若EmpID確認非null+非空白,則儲存到待儲存本系統使用者清單中
                        if (!string.IsNullOrEmpty(employee.EmpID))
                        {
                            listEmp.Add(employee);
                        }
                    }
                    Console.WriteLine(tt);
                }
                logger.WriteLogs("Total Process " + tt + " Users in LDAP");
            }
            catch (Exception ex)
            {
                logger.WriteLogs(ex.Message + " " + ex.StackTrace);
            }
            finally
            {
                //Step6:close LDAP connection 
                // "關閉LDAP連線。" ;
                conn.Disconnect();
                conn = null;
                cons = null;
                logger.WriteLogs("Close LDAP Connection Success");
            }        
        }

        /// <summary>
        /// 取得使用者在LDAP上的角色清單
        /// </summary>
        /// <param name="entry"></param>
        /// <returns></returns>
        public static string[] GetLDAPGroups(LdapEntry entry)
        {
            string[] _userLDAPGroups = null;
            try
            {                
                LdapAttributeSet attributeSet = entry.getAttributeSet();
                //取得user所有角色
                if (attributeSet.getAttribute("groupMembership") != null)
                {
                    _userLDAPGroups = attributeSet.getAttribute("groupMembership").StringValueArray;
                }
                return _userLDAPGroups;
            }
            catch (Exception ex)
            {                
                logger.WriteLogs(ex.Message + " " + ex.StackTrace);
                return _userLDAPGroups;
            }
        }

        /// <summary>
        /// 更新User.csv檔案
        /// </summary>
        public static void UpdateUserFile()
        {
            try
            {
                if (listEmp.Count != 0)
                {
                    //備份舊的csv檔
                    logger.WriteLogs("Backup Old Users.csv");
                    if (File.Exists(@Config.CSVPath + "Users.csv"))
                    {
                        File.Copy(@Config.CSVPath + "Users.csv", @Config.CSVPath + "Users.bak", true);
                    }

                    // "開啟 Users.csv …… " ;
                    logger.WriteLogs("Open Users.csv");
                    using (FileStream fs = new FileStream(@Config.CSVPath + "Users.csv", FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        using (StreamWriter streamWriter = new StreamWriter(fs, System.Text.Encoding.UTF8))
                        {

                            streamWriter.BaseStream.Seek(0, SeekOrigin.End);
                            logger.WriteLogs("Update User.csv file");

                            //寫到User.csv檔案
                            foreach (NUMSEmployee emp in listEmp)
                            {
                                Console.WriteLine("ID=" + emp.EmpID + ", Name=" + emp.EmpName + ", EMail=" + emp.EmpEMail + ", Level=" + emp.EmpLevel + ", Title=" + emp.EmpTitle + ",Groups=" + emp.EmpGroups);
                                streamWriter.WriteLine(emp.EmpID + "," + emp.EmpName.Replace("'", "") + "," + emp.EmpEMail.Replace("'", "") + "," + emp.EmpLevel + "," + emp.EmpTitle.Replace("'", "") + "," + emp.EmpGroups);
                            }
                            logger.WriteLogs("Total Update " + listEmp.Count + " Users of CSFS in User.csv ");
                        }
                    }
                }
                else
                {
                    Console.WriteLine("No user in " + Config.APDN);
                    logger.WriteLogs("No user in" + Config.APDN);
                }
            }
            catch (Exception ex)
            {
                logger.WriteLogs(ex.Message + " " + ex.StackTrace);
            }
        }

        /// <summary>
        /// 取得LDAP 中本系統的所有角色
        /// </summary>
        public static void GetRole()
        {
            ///連線LDAP
            //string[] userDNInfo = new string[2] { "", "(cn=" + userid + ")" };//{userDN,searchFilter)
            string[] searchFilter = new string[2] { "", "(&(" + Config.SearchFilter + "))" };//ex: "(&(cn=*))"
            string[] searchAttr = new string[2];
            searchAttr[0] = "cn";
            searchAttr[1] = "fullName";
            LdapConnection conn = new LdapConnection();
            LdapSearchConstraints cons = new LdapSearchConstraints();
            try
            {
                //Step1:get LDAP connection
                conn.Connect(Config.LDAPHost, Config.LDAPPort);

                //Step2:verify AP ID and AP password
                conn.Bind(Config.APDN, Config.APPwd);//(appDN, APPWD)

                Console.WriteLine("Conntection LDAP Server Success");
                logger.WriteLogs("Conntection LDAP Server Success");

                //Step3:search user DN
                cons.MaxResults = 0;
                LdapSearchResults lsc = conn.Search(Config.APDN, LdapConnection.SCOPE_SUB, searchFilter[1], searchAttr, false, cons);
                LdapEntry userLdapEntry = null;

                int tt = 0;
                while (lsc.hasMore())
                {
                    tt++;
                    try
                    {
                        userLdapEntry = lsc.next();
                    }
                    catch (LdapException ex)
                    {
                        logger.WriteLogs(ex.Message + " " + ex.StackTrace);
                    }

                    NUMSRole role = new NUMSRole
                        {
                            RoleID = "",
                            RoleName = "",
                            RoleGroupID = "",
                            RoleDesc = ""
                        };
                        LdapAttributeSet attributeSet = userLdapEntry.getAttributeSet();

                        LdapAttribute laCN = attributeSet.getAttribute("cn");
                        if (laCN != null)
                        {
                            role.RoleID = laCN.StringValue;
                        }

                        LdapAttribute laFullName = attributeSet.getAttribute("fullName");
                        if (laFullName != null)
                        {
                            role.RoleName = laFullName.StringValue;
                        }

                        //若EmpID確認非null+非空白,則儲存到待儲存本系統使用者清單中
                        //20150520 horace CSFS在LDAP的角色長度=7 ,且要檢查=Y則加到listRole中,不檢查=N也要加到listRole中
                        if (!string.IsNullOrEmpty(role.RoleID))
                        {
                            if(Config.CheckRoleLength=="Y" && role.RoleID.Length==Convert.ToInt32(Config.RoleLength))
                                listRole.Add(role);
                            else if(Config.CheckRoleLength=="N")
                                listRole.Add(role);
                        }
                    
                    Console.WriteLine(tt);                    
                }
                logger.WriteLogs("Total Process " + tt + " Roles in LDAP");
            }
            catch (Exception ex)
            {
                logger.WriteLogs(ex.Message + " " + ex.StackTrace);
            }
            finally
            {
                //Step6:close LDAP connection 
                // "關閉LDAP連線。" ;
                conn.Disconnect();
                conn = null;
                cons = null;
                logger.WriteLogs("Close LDAP Connection Success");
            }
        }

        /// <summary>
        /// 更新Roles.csv檔案
        /// </summary>
        public static void UpdateRoleFile()
        {
            try
            {
                if (listRole.Count != 0)
                {
                    //備份舊的csv檔
                    logger.WriteLogs("Backup Old Roles.csv");
                    if (File.Exists(@Config.CSVPath + "Roles.csv"))
                    {
                        File.Copy(@Config.CSVPath + "Roles.csv", @Config.CSVPath + "Roles.bak", true);
                    }

                    // "開啟 Roles.csv …… " ;
                    logger.WriteLogs("Open Roles.csv");
                    using (FileStream fs = new FileStream(@Config.CSVPath + "Roles.csv", FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        using (StreamWriter streamWriter = new StreamWriter(fs, System.Text.Encoding.UTF8))
                        {

                            streamWriter.BaseStream.Seek(0, SeekOrigin.End);
                            logger.WriteLogs("Update Roles.csv file");

                            //寫到User.csv檔案
                            foreach (NUMSRole r in listRole)
                            {
                                Console.WriteLine("ID=" + r.RoleID + ", Name=" + r.RoleName);
                                streamWriter.WriteLine(r.RoleID + "," + r.RoleName);
                            }
                            logger.WriteLogs("Total Update " + listRole.Count + " Roles of CSFS in Roles.csv ");
                        }
                    }
                }
                else
                {
                    Console.WriteLine("No role in " + Config.APDN);
                    logger.WriteLogs("No role in" + Config.APDN);
                }
            }
            catch (Exception ex)
            {
                logger.WriteLogs(ex.Message + " " + ex.StackTrace);
            }
        }

        /// <summary>
        /// 手動執行批次,自檔案Users.csv與Role.csv的資料來桐步道NUMS資料庫中
        /// </summary>
        public static void GetCSVFile()
        {
            Console.WriteLine("Get Data From GetCSVFile()");
            logger.WriteLogs("Get Data From GetCSVFile()");
            try
            {
                List<NUMSEmployee> _listEmp = new List<NUMSEmployee>();
                List<NUMSRole> _listRole = new List<NUMSRole>();//本系統在LDAP上的使用者List

                //開啟Users.csv檔...
                using (FileStream fs = new FileStream(@Config.CSVPath + "Users.csv", FileMode.Open, FileAccess.Read))
                {
                    using (StreamReader reader = new StreamReader(fs, System.Text.Encoding.UTF8))
                    {
                        string linestr;
                        string[] empAry = new string[6];
                        linestr = reader.ReadLine();
                        // "讀取並寫入角色資料中…" ;
                        while (linestr != null)
                        {
                            empAry = linestr.Split(',');
                            //如果切出來是6個欄位才執行
                            if (empAry.Length == 6)
                            {
                                _listEmp.Add(new NUMSEmployee
                                {
                                    EmpID = empAry[0],
                                    EmpName = (empAry[1] == null) ? "" : empAry[1],
                                    EmpEMail = (empAry[2] == null) ? "" : empAry[2],
                                    EmpLevel = (empAry[3] == null) ? "" : empAry[3],
                                    EmpTitle = (empAry[4] == null) ? "" : empAry[4],
                                    EmpGroups = (empAry[5] == null) ? "" : empAry[5],
                                    OnBoard = true
                                });
                            }
                            linestr = reader.ReadLine();
                        }
                        if (_listEmp.Count > 0)
                            listEmp = _listEmp;//將Users.csv資料copy到Global List變數listEmp上
                        else
                            logger.WriteLogs("No user data in Users.csv => GetCSVFile()");
                    }
                }

                //開啟Roles.csv檔...
                using (FileStream fs = new FileStream(@Config.CSVPath + "Roles.csv", FileMode.Open, FileAccess.Read))
                {
                    using (StreamReader reader = new StreamReader(fs, System.Text.Encoding.UTF8))
                    {
                        string linestr;
                        string[] roleAry = new string[2];
                        linestr = reader.ReadLine();
                        // "讀取並寫入角色資料中…" ;
                        while (linestr != null)
                        {
                            roleAry = linestr.Split(',');
                            //如果切出來是2個欄位才執行
                            if (roleAry.Length == 2)
                            {
                                _listRole.Add(new NUMSRole
                                {
                                    RoleID = roleAry[0],
                                    RoleName = (roleAry[1] == null) ? "" : roleAry[1]
                                });
                            }
                            linestr = reader.ReadLine();
                        }
                        if (_listRole.Count > 0)
                            listRole = _listRole;//將Roles.csv資料copy到Global List變數listRole上
                        else
                            logger.WriteLogs("No user data in Roles.csv => GetCSVFile()");
                    }
                }
            }
            catch (Exception ex)
            {
                logger.WriteLogs(ex.Message + " " + ex.StackTrace);            
            }
        }

        /// <summary>
        /// 更新LDAP Employee資料到NUMS資料庫Table:NUMSEmployee      
        /// 1.先將所有 NUMSEmployee.OnBoard=false
        /// 2.將LDAP資料比對NUMSEmployee
        /// 3.比對Rule:
        ///     3.1 若比對不到->新增員工資料到NUMEmployee,NUMSEmployee.OnBoard=true
        ///     3.2 若比對到->改NUMSEmployee.OnBoard=true
        /// </summary>
        public static void SyncNUMSEmployee()
        {
            try
            {
                if (listEmp.Count == 0)
                {
                    logger.WriteLogs("No Data in Users");
                    return;
                }
                using (SqlConnection connection = new SqlConnection(Config.ConnectionString))
                {
                    connection.Open();

                        SqlTransaction transaction;

                        transaction = connection.BeginTransaction("SyncEmployeeTransaction");

                        try
                        {
                            string sqlStr = "";
                            int successCount = 0;
                            using (SqlCommand command = connection.CreateCommand())
                            {
                                command.Transaction = transaction;
                                command.CommandType = CommandType.Text;
                                command.CommandText = "update CSFSEmployee set OnBoard=0";
                                command.ExecuteNonQuery();
                            }

                            sqlStr = "   if (select count(*) from CSFSEmployee where EmpID=@EmpID)=0 " +
                                    "       begin " +
                                    "           insert into CSFSEmployee(EmpID,EmpName,EmpEMail,EmpTitle,EmpLevel,OnBoard,CreatedUser,ModifiedUser) " +
                                    "               values (@EmpID,@EmpName,@EmpEMail,@EmpTitle,@EmpLevel,1,'CTBC.CSFS.Batch.LDAPSync','CTBC.CSFS.Batch.LDAPSync')" +
                                    "       end " +
                                    "   else " +
                                    "       begin " +
                                    "           update CSFSEmployee set EmpName=@EmpName,EmpEMail=@EmpEMail,EmpTitle=@EmpTitle,EmpLevel=@EmpLevel,OnBoard=1,ModifiedUser='CTBC.CSFS.Batch.LDAPSync',ModifiedDate=getDate() where EmpID=@EmpID " +
                                    "       end ";

                            foreach (NUMSEmployee item in listEmp)
                            {
                                using (SqlCommand command = connection.CreateCommand())
                                {
                                    command.Transaction = transaction;
                                    command.CommandType = CommandType.Text;
                                    command.CommandText = sqlStr;
                                    command.Parameters.Add("@EmpID", SqlDbType.NVarChar, 20).Value = item.EmpID;
                                    command.Parameters.Add("@EmpName", SqlDbType.NVarChar, 100).Value = item.EmpName;
                                    command.Parameters.Add("@EmpEMail", SqlDbType.NVarChar, 100).Value = item.EmpEMail;
                                    command.Parameters.Add("@EmpTitle", SqlDbType.NVarChar, 100).Value = item.EmpTitle;
                                    command.Parameters.Add("@EmpLevel", SqlDbType.NVarChar, 10).Value = item.EmpLevel;
                                    command.ExecuteNonQuery();
                                }
                                successCount++;
                            }

                            // Attempt to commit the transaction.
                            transaction.Commit();
                            Console.WriteLine("UPDATE " + successCount + " Employees Successfully");
                            logger.WriteLogs("UPDATE " + successCount + " Employees Successfully");
                        }
                        catch (Exception ex)
                        {
                            logger.WriteLogs("Commit Exception Type: " + ex.GetType());
                            logger.WriteLogs(ex.Message + " " + ex.StackTrace);
                              Console.WriteLine(ex.Message + " " + ex.StackTrace);
                            // Attempt to roll back the transaction.
                            try
                            {
                                transaction.Rollback();
                                logger.WriteLogs("Sync CSFS Employee() is Rollback Now");
                            }
                            catch (Exception ex2)
                            {
                                logger.WriteLogs("Rollback Exception Type: " + ex2.GetType());
                                logger.WriteLogs(ex.Message + " " + ex.StackTrace);
                                Console.WriteLine(ex.Message + " " + ex.StackTrace);
                            }
                        }
                }//end of sqlConnection
            }
            catch (Exception ex)
            {
                logger.WriteLogs(ex.Message + " " + ex.StackTrace);
            }
        }

        /// <summary>
        /// 更新LDAP Role資料到NUMS資料庫Table:NUMSRole  
        /// 更新LDAP Employee to Role資料到NUMS資料庫Table:NUMSEmployeeToRole
        /// 1.先將所有 NUMSRole 清除
        /// 2.將所有 NUMSEmployeeToRole 清除
        /// 3.將LDAP中的Roles 新增到NUMSRole
        /// 4.將LDAP中的Employee與Role對應新增到NUMSEmployeeToRole 
        /// </summary>
        public static void SyncNUMSEmployeeToRole()
        {
            try
            {
                if (listEmp.Count == 0)
                {
                    logger.WriteLogs("No Data in Users");
                    return;
                }
                using (SqlConnection connection = new SqlConnection(Config.ConnectionString))
                {
                    connection.Open();

                    SqlTransaction transaction;

                    transaction = connection.BeginTransaction("SyncEmployeeTransaction");

                    try
                    {
                        string sqlStr = "";
                        int successRoleCount = 0;
                        int successEmployeeToRoleCount = 0;
                        //清除 NUMSRole 與 NUMSEmployeeToRole
                        using (SqlCommand command = connection.CreateCommand())
                        {
                            command.Transaction = transaction;
                            command.CommandType = CommandType.Text;
                            sqlStr = "delete CSFSEmployeeToRole " +
                                " delete CSFSRole ";
                            command.CommandText = sqlStr;
                            command.ExecuteNonQuery();
                        }

                        //更新NUMSRole
                        sqlStr = " insert into CSFSRole(RoleID,RoleName,CreatedUser,ModifiedUser) " +
                                " values (@RoleID,@RoleName,'CTBC.CSFS.Batch.LDAPSync','CTBC.CSFS.Batch.LDAPSync')";

                        foreach (NUMSRole item in listRole)
                        {
                            using (SqlCommand command = connection.CreateCommand())
                            {
                                command.Transaction = transaction;
                                command.CommandType = CommandType.Text;
                                command.CommandText = sqlStr;
                                command.Parameters.Add("@RoleID", SqlDbType.NVarChar, 20).Value = item.RoleID;
                                command.Parameters.Add("@RoleName", SqlDbType.NVarChar, 100).Value = item.RoleName;
                                command.ExecuteNonQuery();
                            }
                            successRoleCount++;
                        }

                        //更新NUMSEmployeeToRole
                        sqlStr = " insert into CSFSEmployeeToRole(EmpID,RoleID,CreatedUser,ModifiedUser) " +
                                " values (@EmpID,@RoleID,'CTBC.CSFS.Batch.LDAPSync','CTBC.CSFS.Batch.LDAPSync')";
                        string[] roles;
                        foreach (NUMSEmployee item in listEmp)
                        {
                            roles = item.EmpGroups.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                            if (roles.Length > 0)
                            {
                                foreach (string roleID in roles)
                                {
                                    using (SqlCommand command = connection.CreateCommand())
                                    {
                                        command.Transaction = transaction;
                                        command.CommandType = CommandType.Text;
                                        command.CommandText = sqlStr;
                                        command.Parameters.Add("@EmpID", SqlDbType.NVarChar, 20).Value = item.EmpID;
                                        command.Parameters.Add("@RoleID", SqlDbType.NVarChar, 20).Value = roleID;
                                        command.ExecuteNonQuery();
                                    }
                                    successEmployeeToRoleCount++;
                                }
                            }
                        }

                        // Attempt to commit the transaction.
                        transaction.Commit();

                        Console.WriteLine("UPDATE " + successRoleCount + " Roles Successfully");
                        Console.WriteLine("UPDATE " + successEmployeeToRoleCount + " Records of Employee to Roles Successfully");
                        logger.WriteLogs("UPDATE " + successRoleCount + " Roles Successfully");
                        logger.WriteLogs("UPDATE " + successEmployeeToRoleCount + " Records of Employee to Roles Successfully");
                    }
                    catch (Exception ex)
                    {
                        logger.WriteLogs("Commit Exception Type: " + ex.GetType());
                        logger.WriteLogs("May be conflict occurred between  LDAP's Roles and Employee's Roles be asigned in LDAP");
                        logger.WriteLogs(ex.Message + " " + ex.StackTrace);
                        Console.WriteLine(ex.Message + " " + ex.StackTrace);
                        // Attempt to roll back the transaction.
                        try
                        {
                            transaction.Rollback();
                            logger.WriteLogs("Sync CSFS EmployeeToRole() is Rollback Now");
                        }
                        catch (Exception ex2)
                        {
                            // This catch block will handle any errors that may have occurred
                            // on the server that would cause the rollback to fail, such as
                            // a closed connection.
                            logger.WriteLogs("Rollback Exception Type: " + ex2.GetType());
                            logger.WriteLogs(ex.Message + " " + ex.StackTrace);
                            Console.WriteLine(ex.Message + " " + ex.StackTrace);
                        }
                    }
                }//end of sqlConnection
            }
            catch (Exception ex)
            {
                logger.WriteLogs(ex.Message + " " + ex.StackTrace);
            }        
        }

        /// <summary>
        /// 發送批次執行狀況監控mail給管理者
        /// </summary>
        public static void SendMonitorEmail()
        {
            try
            {
                #region//.NET >= 2.0寫法
                char[] delimiterChars = { ',' };
                string[] whoList = Config.MailToWho.Split(delimiterChars);  //誰可收到批次執行狀況回報mail
                if (whoList.Length >= 1)
                {
                    MailAddress mailFrom = new MailAddress(Config.MailFrom, Config.MailFromDisplayName);
                    MailAddress mailTo ;
                    MailMessage message;
                    SmtpClient client = new SmtpClient(Config.SMTPServer);
                    for (int i = 0; i < whoList.Length; i++)
                    {
                        mailTo = new MailAddress(whoList[i]);
                        message = new MailMessage(mailFrom, mailTo);
                        message.Subject = Config.MailSubject;
                        message.IsBodyHtml = true;
                        message.Body = @"<html><head><title>CSFS LDAP Sync Batch</title></head><body><div style='font-size:13px;font-family:Verdana, Helvetica, Sans-Serif;'>" + logger.AllMessageForMail + @"</div></body></html>";
                        client.Send(message);
                    }                   
                    logger.WriteLogs("Sending Batch Status Mail Success");                    
                }
                else
                {
                    logger.WriteLogs("No List of Mail To !!");
                }
                #endregion
            }
            catch(Exception ex)
            {
                logger.WriteLogs(ex.Message + " " + ex.StackTrace);
            }
        }

        //判斷Role的長度(ex:CSFS001)是否>Config.RoleLength
        public static bool CheckRoleLength(string roleDN)
        {
            string tmpRole = "";
            if (string.IsNullOrEmpty(roleDN))
            {
                return false;
            }
            else {
                string[] tmp = null;
                tmp = roleDN.Split(',');
                if (tmp.Length > 1 && tmp[0].Substring(0,3).ToUpper()=="CN=")
                {
                    tmpRole = tmp[0].Replace("cn=", "");
                    if (tmpRole.Length == Convert.ToInt32(Config.RoleLength))
                        return true;
                    else
                        return false;
                }
                else {
                    return false;
                }
            }
        }
    }


}
