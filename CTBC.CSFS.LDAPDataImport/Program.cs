using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Mail;
using System.Data;
using System.Data.SqlClient;
using Novell.Directory.Ldap;
using System.Configuration;
using CTCB.NUMS.Batch.Logger;

namespace CTBC.CSFS.LDAPDataImport
{
    public class Program
    {
		static string MSG0 = "CtcbBackupDataVerify";		//不可更動的執行訊息與log所用的method字串名稱常數
		static string MSG1 = "CtcbUserVerify";
		static string MSG2 = "CtcbManagerVerify";
		static string MSG3 = "CtcbOrganizationVerify";
		static string MSG4 = "CtcbMyBossVerify";
		static string MSG5 = "CtcbUpdEmailVerify";
        static string MSG6 = "CtcbUpdBranchVerify";

        static string authMode = null;
        static string ldapHost = null;                  //LDAP認證server的IP
        static string hrisRoot = null;                  //rootBaseDN是指會使用此AP的部門單位的DN，如:中國信託商業銀行或僅是個金或法金	
        static int ldapPort = 0;                        //LDAP認證server的port
        static string apdn = null;                      //serviceID(就是AP註冊在LDAP的物件)的DN與密碼
        static string apid = null;                      //serviceID(就是AP註冊在LDAP的物件)的DN與密碼
        static string apPwd = null;                     //serviceID(就是AP註冊在LDAP的物件)的DN與密碼
        static string createdUser = null;               //建立者
        static string searchUserFilter = null;          //查詢LDAP過濾符號
        static string searchMgrFilter = null;           //查詢LDAP過濾符號
        static string searchOrgFilter = null;           //查詢LDAP過濾符號
        static string searchBranch = null;              //查詢部門=分行 過濾符號
        static string logPath = null;                   //Log 檔案路徑
        static string sLogFile = null;                  //Log 檔案路徑
        static string mailFromDisplayName = null;       //mail 寄件人顯示字串
        static string mailSubject = null;               //mail 主旨顯示字串
        static string mailFrom = null;                  //mail 寄件人
        static string mailToWho = null;                 //mail 收件人
        static string smtpServer = null;                //SMTP Server IP
        static string connectionString = null;          //資料庫連線字串

        static SqlConnection connection = null;         //資料庫共用的連線參數
        static SqlTransaction transaction = null;       //資料庫共用Transaction參數

        static Log logger = null;                       //Log 檔案路徑       
                        
        static LdapConnection ldapConn = new LdapConnection();                  //LDAP連線參數
        static LdapSearchConstraints ldapCons = new LdapSearchConstraints();    //LDAP連線參數
        static List<LDAPEmployee> listEmp = new List<LDAPEmployee>();           //LDAPEmployee資料集合
        static List<LDAPManager> listMgr = new List<LDAPManager>();             //LDAPManager資料集合
        static List<LDAPDepartment> listDep = new List<LDAPDepartment>();       //LDAPDepartment資料集合
        static bool isOpenDBOK = false;                  //判斷true= Open DB 執行OK
        static bool isOpenLDAPOK = false;                //判斷true= Open LDAP 執行OK

        static bool isExeOK0 = false;                    //判斷true=CtcbBackupDataVerify執行OK
        static bool isExeOK1 = false;                    //判斷true=CtcbUserVerify執行OK
        static bool isExeOK2 = false;                    //判斷true=CtcbManagerVerify執行OK
        static bool isExeOK3 = false;                    //判斷true=CtcbOrganizationVerify執行OK
        static bool isExeOK4 = false;                    //判斷true=CtcbMyBossVerify執行OK
        static bool isExeOK5 = false;                    //判斷true=CtcbUpdEmailVerify執行OK
        static bool isExeOK6 = false;                    //判斷true=CtcbUpdBranchVerify執行OK
        /// <summary>
        /// Step 0. 讀取各項Config資訊                                                    => GetProperties()
        /// Step 1. 備份資料庫table                                                       => CtcbBackupDataVerify()
        /// Step 2. 連線DB與LDAP                                                          => OpenDBConn() 與 OpenLDAPConn()
        /// Step 3. 更新LDAPEmployee表格中的同仁資料                                      => CtcbUserVerify()
        /// Step 4. 更新LDAPManager表格中的資料(更新主管資料)                             => CtcbManagerVerify()
        /// Step 5. 更新LDAPDepartment表格中的部門資料(更新組織資料)                      => CtcbOrganizationVerify()
        /// Step 6. 更新LDAPEmployee表格中的ManagerID這個欄位                             => CtcbMyBossVerify()
        /// Step 7. 將LDAPEmployee中email欄位空白者依照相同member_id的email來更新         => CtcbUpdEmailVerify()
        /// Step 8. 將LDAPEmployee中分行同仁的BranchID與BranchName欄位來更新              => CtcbUpdBranchVerify() 
        /// Step 9. 將整個交易異動進行Commit                                              => Transaction Commit
        /// Step 10. 關閉DB與LDAP                                                         => ColseLDAPConn() 與 CloseDBConn()
        /// Step 11. 寄送執行完畢mail                                                     => SendMonitorEmail()
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args)
        {
			//Program wmspb=new Program();
			try{
                    GetProperties();
                    logger.WriteLogs("LDAP Data Import Start");
                    Console.WriteLine("LDAP Data Import Start");
                    OpenDBConn();
                    OpenLDAPConn();
					//如果任何一個method執行錯誤則rollback資料異動,若所有method皆成功,則commit資料異動
                    if (isOpenDBOK && isOpenLDAPOK)
                    {
                        isExeOK0 = CtcbBackupDataVerify();
                    }
                    if(isExeOK0){
                            isExeOK1=CtcbUserVerify();
                            if(isExeOK1){
                                    isExeOK2=CtcbManagerVerify();
                                    if(isExeOK2){
                                            isExeOK3=CtcbOrganizationVerify();
                                            if(isExeOK3){
                                                    isExeOK4=CtcbMyBossVerify();
                                                    if(isExeOK4){													
															isExeOK5=CtcbUpdEmailVerify();
															if(isExeOK5){
                                                                isExeOK6 = CtcbUpdBranchVerify();
                                                                if (isExeOK6)
                                                                {
                                                                    transaction.Commit();
                                                                }
                                                                else logger.WriteLogs("Problem in " + MSG6 + "!");
                                                            }
                                                            else logger.WriteLogs("Problem in " + MSG5 + "!");
                                                    }
                                                    else logger.WriteLogs("Problem in " + MSG4 + "!");
                                            }
                                            else logger.WriteLogs("Problem in " + MSG3 + "!");
                                    }
                                    else logger.WriteLogs("Problem in " + MSG2 + "!");
                            }
                            else logger.WriteLogs("Problem in " + MSG1 + "!>");
                    }
                    else logger.WriteLogs("Problem in " + MSG0 + "!");

			}catch(Exception e){
                logger.WriteLogs(e.Message + e.Source + e.StackTrace);
			}finally{
                    ColseLDAPConn();
                    CloseDBConn();
                    logger.WriteLogs("LDAP Data Import End");
                    //寄送通知mail
                    SendMonitorEmail();
                    logger.WriteLogs("--------------------------------------------------------------");
                    Console.WriteLine("LDAP Data Import End");
			}
        }

        //取得參數
        public static void GetProperties()
        {
            try {
                    authMode = ConfigurationManager.AppSettings["AuthenticationMode"];
                    ldapHost = ConfigurationManager.AppSettings["LDAPServer"];
                    hrisRoot = ConfigurationManager.AppSettings["LDAPRoot"];
                    ldapPort = Convert.ToInt32(ConfigurationManager.AppSettings["LDAPPort"]);
                    apdn = ConfigurationManager.AppSettings["APDN"];
                    apid = ConfigurationManager.AppSettings["APID"];
                    apPwd = ConfigurationManager.AppSettings["APPwd"];
                    createdUser = ConfigurationManager.AppSettings["CreatedUser"];
                    searchUserFilter = ConfigurationManager.AppSettings["SearchUserFilter"];
                    searchMgrFilter = ConfigurationManager.AppSettings["SearchMgrFilter"];
                    searchOrgFilter = ConfigurationManager.AppSettings["SearchOrgFilter"];
                    searchBranch = ConfigurationManager.AppSettings["SearchBranch"];
                    logPath = ConfigurationManager.AppSettings["LogPath"];
                    mailFromDisplayName = ConfigurationManager.AppSettings["MailFromDisplayName"];
                    mailSubject = ConfigurationManager.AppSettings["MailSubject"];
                    mailFrom = ConfigurationManager.AppSettings["MailFrom"];
                    mailToWho = ConfigurationManager.AppSettings["MailToWho"];
                    smtpServer = ConfigurationManager.AppSettings["SMTPServer"];
                    connectionString = ConfigurationManager.ConnectionStrings["NUMS_ADO"].ConnectionString;      
     
                    sLogFile =logPath + "LDAPDataImport-" + DateTime.Now.ToString("yyyyMMdd") + ".log";    //Log 檔案路徑
                    logger = new Log(sLogFile);//Log 檔案路徑    
            }
            catch (Exception e) {
                logger.WriteLogs(e.Message + e.Source + e.StackTrace);
            }     
        }
        //取得DB ConnectionString
        public static void OpenDBConn()
        {
            try
            {
                connection = new SqlConnection(connectionString);
                connection.Open();
                transaction = connection.BeginTransaction("SyncLDAPDataImport");
                logger.WriteLogs("Open DB Connection");
                isOpenDBOK = true;
            }
            catch (Exception e)
            {
                isOpenDBOK = false;
                logger.WriteLogs(e.Message + e.Source + e.StackTrace);
            }        
        }
        //關閉DB ConnectionString
        public static void CloseDBConn()
        {
            try
            {
                connection.Close();
                connection = null;
                logger.WriteLogs("Close DB Connection");
            }
            catch (Exception e)
            {
                logger.WriteLogs(e.Message + e.Source + e.StackTrace);
            }            
        }
		//設定LDAP 連線
        public static void OpenLDAPConn()
        {
            try
            {
                ldapConn = new LdapConnection();
                ldapCons = new LdapSearchConstraints();
                //Step1:get LDAP connection
                ldapConn.Connect(ldapHost, ldapPort);

                //Step2:verify AP ID and AP password
                ldapConn.Bind(apdn,apPwd);//(appDN, APPWD)
                logger.WriteLogs("Open LDAP Connection Successed");
                isOpenLDAPOK = true;
            }
            catch (Exception e)
            {
                isOpenLDAPOK = false;
                logger.WriteLogs(e.Message + e.Source + e.StackTrace);
            }            
        }
		//結束LDAP Service ID連線
        public static void ColseLDAPConn()
        {
            try
            {
                //Step6:close LDAP connection 
                // "關閉LDAP連線。" ;
                ldapConn.Disconnect();
                ldapConn = null;
                ldapCons = null;
                logger.WriteLogs("Close LDAP Connection Successed");
            }
            catch (Exception e)
            {
                logger.WriteLogs(e.Message + e.Source + e.StackTrace);
            }            
        }

        //更新LDAPEmployee表格中的同仁資料
        public static bool CtcbUserVerify()
        {
            bool rtn = false;
            int tt = 0;
            string[] tmpdn = null;
            string tmpdnStr = "";
            try
            {
                Console.WriteLine("Delete Table");
                rtn = DelDBTable("LDAPEmployee");
                Console.WriteLine("Insert LDAPEmployee");
                if (rtn)
                {
                    string[] searchFilter = new string[2] { "", "(&(" + searchUserFilter + "))" };//ex: "(&(cn=*))"
                    string[] searchAttr = new string[8];
                    searchAttr[0] = "cn"; 
                    searchAttr[1] = "fullName";
                    searchAttr[2] = "mail";
                    searchAttr[3] = "ctcbEmployeeRole";
                    searchAttr[4] = "title";
                    searchAttr[5] = "businessCategory";
                    searchAttr[6] = "facsimileTelephoneNumber";
                    searchAttr[7] = "physicalDeliveryOfficeName";
                    //Step3:search user DN
                    ldapCons.MaxResults = 0;
                    LdapSearchResults ldapResults = ldapConn.Search(hrisRoot, LdapConnection.SCOPE_SUB, searchFilter[1], searchAttr, false, ldapCons);
                    LdapEntry userLdapEntry = null;
                    while (ldapResults.hasMore())
                    {
                        tt++;
                        try
                        {
                            userLdapEntry = ldapResults.next();
                        }
                        catch (LdapException ex)
                        {
                            logger.WriteLogs(ex.Message + " " + ex.StackTrace);
                        }


                       //取出該user在LDAP中的各項屬性(Name,Email...)
                        LDAPEmployee employee = new LDAPEmployee() { 
                            EmpID = "",
                            EmpName = "",
                            EmpTitle = "",
                            EmpBusinessCategory = "",
                            IsManager = "",
                            EMail = "",
                            DepDN = "",
                            DepID = "",
                            TelNo = "",
                            TelExt = "",
                            ManagerID = "",
                            CreatedUser = "",
                            ModifiedUser = ""                              
                        };

                        LdapAttributeSet attributeSet = userLdapEntry.getAttributeSet();

                        //員工員編(CN)
                        LdapAttribute eid = attributeSet.getAttribute("cn");
                        if (eid != null)
                            employee.EmpID = eid.StringValue;

                        //員工姓名(fullname)
                        LdapAttribute ename = attributeSet.getAttribute("fullName");
                        if (ename != null)
                            employee.EmpName = ename.StringValue;

                        //員工email(mail)
                        LdapAttribute email = attributeSet.getAttribute("mail");
                        if (email != null)
                            employee.EMail = email.StringValue;

                        //員工DN=>cn=Z00033831,ou=U00022381,ou=D00022379,ou=R00023452,ou=M00023321,ou=M00022311,ou=U00021934,ou=U00021933,ou=U00021932,ou=U00021931,ou=U00021800,ou=HRIS,o=CTCB
                        //移除DN中的第一個cn=Z00033831,
                        tmpdnStr = userLdapEntry.DN.Trim();
                        tmpdn = tmpdnStr.Split(',');//將DN拆解
                        employee.DepDN = tmpdnStr.Substring(tmpdn[0].Length + 1);//移除DN中的第一個cn=Zxxxxxxx,重新得到剩下整串DN
                        tmpdnStr = "";
                        tmpdn = null;

                        //身份別(ctcbEmployeeRole)有值表示該員工為主管(如000505#科長,000664#業務主管...)
                        LdapAttribute empRole = attributeSet.getAttribute("ctcbEmployeeRole");
                        if (empRole != null)
                            employee.IsManager = empRole.StringValue;

                        //員工所屬部門ID
                        employee.DepID = GetDepID(userLdapEntry.DN.Trim());

                        //員工職稱(如TW0251#Sales Representative,TW0258#Manager...)
                        LdapAttribute emptitle = attributeSet.getAttribute("title");
                        if (emptitle != null)
                            employee.EmpTitle = emptitle.StringValue;

                        //業務別(BusinessCategory)(如000571#客服人員,000554#CSR1,001006#內勤業務助理...)
                        LdapAttribute busCategory = attributeSet.getAttribute("businessCategory");
                        if (busCategory != null)
                            employee.EmpBusinessCategory = busCategory.StringValue;

                        //員工公司電話(facsimileTelephoneNumber)(如02-87878818)
                        LdapAttribute telNo = attributeSet.getAttribute("facsimileTelephoneNumber");
                        if (telNo != null)
                            employee.TelNo = telNo.StringValue;

                        //員工公司分機(physicalDeliveryOfficeName)(如**021#3838)
                        LdapAttribute telExt = attributeSet.getAttribute("physicalDeliveryOfficeName");
                        if (telExt != null)
                            employee.TelExt = telExt.StringValue;

                        employee.CreatedUser = createdUser;
                        employee.ModifiedUser = createdUser;

                        //若EmpID確認非null+非空白,則儲存到待儲存本系統使用者清單中
                        if (!string.IsNullOrEmpty(employee.EmpID))
                        {
                            listEmp.Add(employee);
                        }

                        //新增到LDAPEmployee
                        rtn = InsertToLDAPEmployee(employee);
                        if (!rtn) break;
                    }
                }
                logger.WriteLogs("Insert " + tt + " Employees to LDAPEmployee");
                return rtn; 
            }
            catch (Exception e)
            {
                logger.WriteLogs(e.Message + e.Source + e.StackTrace);
                return rtn;
            }                
        }

        //更新LDAPManager表格中的資料(更新主管資料)
        public static bool CtcbManagerVerify()
        {
            bool rtn = false;
            bool isMgr = false;
            int tt = 0;
            try
            {
                Console.WriteLine("Delete Table");
                rtn = DelDBTable("LDAPManager");
                Console.WriteLine("Insert LDAPManager");
                if (rtn)
                {
                    string[] searchFilter = new string[2] { "", "(&(" + searchMgrFilter + "))" };//ex: "(&(cn=*))"
                    string[] searchAttr = new string[2];
                    searchAttr[0] = "cn";
                    searchAttr[1] = "ctcbEmployeeRole";
                    //Step3:search user DN
                    ldapCons.MaxResults = 0;
                    LdapSearchResults ldapResults = ldapConn.Search(hrisRoot, LdapConnection.SCOPE_SUB, searchFilter[1], searchAttr, false, ldapCons);
                    LdapEntry userLdapEntry = null;
                    while (ldapResults.hasMore())
                    {
                        try
                        {
                            userLdapEntry = ldapResults.next();
                        }
                        catch (LdapException ex)
                        {
                            logger.WriteLogs(ex.Message + " " + ex.StackTrace);
                        }

                        isMgr = false;

                        //取出該user在LDAP中的各項屬性(Name,Email...)
                        LDAPManager manager = new LDAPManager() { 
                            EmpID = "",
                            DepID = "",
                            CreatedUser = "",
                            ModifiedUser = ""        
                        };

                        LdapAttributeSet attributeSet = userLdapEntry.getAttributeSet();

                        //身份別(ctcbEmployeeRole)有值表示該員工為主管(如000505#科長,000664#業務主管...)
                        LdapAttribute empRole = attributeSet.getAttribute("ctcbEmployeeRole");
                        if (empRole != null)
                        {
                            if (!string.IsNullOrEmpty(empRole.StringValue))
                            {
                                tt++;
                                //員工員編(CN)
                                LdapAttribute eid = attributeSet.getAttribute("cn");
                                if (eid != null)
                                    manager.EmpID = eid.StringValue;

                                //員工所屬部門ID
                                manager.DepID = GetDepID(userLdapEntry.DN.Trim());
                                manager.CreatedUser = createdUser;
                                manager.ModifiedUser = createdUser;
                                isMgr = true;
                            }
                        }

                        //若EmpID確認非null+非空白,則儲存到待儲存本系統使用者清單中
                        if (!string.IsNullOrEmpty(manager.EmpID) && isMgr)
                            listMgr.Add(manager);

                        //新增到LDAPManager
                        if (isMgr)
                            rtn = InsertToLDAPManager(manager);

                        if (!rtn) break;
                    }
                }
                logger.WriteLogs("Insert " + tt + " Managers to LDAPManager");
                return rtn; 
            }
            catch (Exception e)
            {
                logger.WriteLogs(e.Message + e.Source + e.StackTrace);
                return true;
            }
        }

        //更新LDAPDepartment表格中的部門資料(更新組織資料)
        public static bool CtcbOrganizationVerify()
        {
            bool rtn = false;
            int tt = 0;
            try
            {
                Console.WriteLine("Delete Table");
                rtn = DelDBTable("LDAPDepartment");
                Console.WriteLine("Insert LDAPDepartment");
                if (rtn)
                {
                    string[] searchFilter = new string[2] { "", "(!(" + searchOrgFilter + "))" };//撈取LDAP中非人的資訊,即撈取組織資訊ex: "(!(cn=*))"
                    string[] searchAttr = new string[6];
                    string[] tmpBusCat = new string[1];
                    searchAttr[0] = "ou";                   //D00022460
                    searchAttr[1] = "fullName";             //永吉分行
                    searchAttr[2] = "businessCategory";     //2#分行
                    searchAttr[3] = "title";                //TW_TEAM_LVL#科
                    searchAttr[4] = "ctcbHrisOrgLevel";     //17
                    searchAttr[5] = "ctcbBankingID";        //0495

                    //Step3:search user DN
                    ldapCons.MaxResults = 0;
                    LdapSearchResults ldapResults = ldapConn.Search(hrisRoot, LdapConnection.SCOPE_SUB, searchFilter[1], searchAttr, false, ldapCons);
                    LdapEntry userLdapEntry = null;
                    while (ldapResults.hasMore())
                    {
                        tt++;
                        try
                        {
                            userLdapEntry = ldapResults.next();
                        }
                        catch (LdapException ex)
                        {
                            logger.WriteLogs(ex.Message + " " + ex.StackTrace);
                        }

                        //取出該user在LDAP中的各項屬性(Name,Email...)
                        LDAPDepartment department = new LDAPDepartment() { 
                            DepID = "",
                            DepName = "",
                            DepDN = "",
                            BusinessCategory="",
                            DepTitle = "",
                            CtcbHrisOrgLevel = "",
                            CtcbBankingID = "",
                            CreatedUser = "",
                            ModifiedUser = ""        
                        };

                        LdapAttributeSet attributeSet = userLdapEntry.getAttributeSet();

                        //組織代號(ou)
                        LdapAttribute depID = attributeSet.getAttribute("ou");
                        if (depID != null)
                            department.DepID = depID.StringValue;

                        //組織名稱(fullname)
                        LdapAttribute depName = attributeSet.getAttribute("fullName");
                        if (depName != null)
                            department.DepName = depName.StringValue;

                        //組織DN
                        department.DepDN = userLdapEntry.DN.Trim();

                        //部門類別
                        LdapAttribute depBusCat = attributeSet.getAttribute("businessCategory");
                        if (depBusCat != null)
                        {
                            tmpBusCat[0] = depBusCat.StringValue;
                            department.BusinessCategory = tmpBusCat[0];

                            //如果部門類別=分行,則抓取分行代號
                            if (tmpBusCat[0] == searchBranch)
                            {
                                //分行代碼
                                LdapAttribute depBranchID = attributeSet.getAttribute("ctcbBankingID");
                                if (depBranchID != null)
                                    department.CtcbBankingID = depBranchID.StringValue;
                            }
                        }
                        //部門層級名稱(組/科/部/處)
                        LdapAttribute depTitle = attributeSet.getAttribute("title");
                        if (depTitle != null)
                            department.DepTitle = depTitle.StringValue;

                        //部門層級(數字)
                        LdapAttribute depHrisOrgLev = attributeSet.getAttribute("ctcbHrisOrgLevel");
                        if (depHrisOrgLev != null)
                            department.CtcbHrisOrgLevel = depHrisOrgLev.StringValue;



                        department.CreatedUser = createdUser;
                        department.ModifiedUser = createdUser;

                        //若EmpID確認非null+非空白,則儲存到待儲存本系統使用者清單中
                        if (!string.IsNullOrEmpty(department.DepID))
                        {
                            listDep.Add(department);
                        }

                        //新增到LDAPDepartment
                        rtn = InsertToLDAPDepartment(department);
                        if (!rtn) break;
                    }
                }
                logger.WriteLogs("Insert " + tt + " Departments to LDAPDepartment");
                return rtn;
            }
            catch (Exception e)
            {
                logger.WriteLogs(e.Message + e.Source + e.StackTrace);
                return true;
            }
        }
		//更新LDAPEmployee表格中的MANAGER_ID這個欄位
        public static bool CtcbMyBossVerify()
        {
            bool rtn = false;
            int tt = 0;
            try
            {
                List<LDAPEmployee> listEmpBoss = new List<LDAPEmployee>();
                listEmpBoss = GetEmployeePrepareForBoss();
                //如果同仁為主管,則抓取主管所在部門的上一層來判對主管的"老闆"的員編,else若為非主管同仁,則以DepID抓取其部門主管
                foreach (LDAPEmployee emp in listEmpBoss) {
                    if (!string.IsNullOrEmpty(emp.IsManager))
                    {
                        rtn = UpdateBossManagerID(emp);
                    }
                    else {
                        rtn = UpdateStaffManagerID(emp);
                    }
                    tt++;
                    Console.WriteLine(emp.EmpID);
                }
                logger.WriteLogs("Update CtcbMyBossVerify() " + tt + " ManagerID successed!");
                return rtn;
            }
            catch (Exception e)
            {
                logger.WriteLogs("exe[CtcbMyBossVerify()] error, transaction is rollbacked!");
                logger.WriteLogs(e.Message + e.Source + e.StackTrace);
                return rtn;
            }
        }
        //將LDAPEmployee中(Day-1)的資料備份到LDAPEmployeeHistory
        public static bool CtcbBackupDataVerify()
        {
            try
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.Transaction = transaction;
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandText = "sp_LDAPBackupData";
                    command.CommandTimeout = 900;
                    //SqlParameter retValParam1 = command.Parameters.Add("@msg1", SqlDbType.NVarChar, 100);
                    //SqlParameter retValParam2 = command.Parameters.Add("@msg2", SqlDbType.NVarChar, 100);
                    //SqlParameter retValParam3 = command.Parameters.Add("@msg3", SqlDbType.NVarChar, 100);
                    //SqlParameter retValParam4 = command.Parameters.Add("@msg4", SqlDbType.NVarChar, 100);
                    //retValParam1.Direction = ParameterDirection.Output;
                    //retValParam2.Direction = ParameterDirection.Output;
                    //retValParam3.Direction = ParameterDirection.Output;
                    //retValParam4.Direction = ParameterDirection.Output;
                    command.ExecuteNonQuery();
                    //logger.WriteLogs(retValParam1.Value.ToString());
                    //logger.WriteLogs(retValParam2.Value.ToString());
                    //logger.WriteLogs(retValParam3.Value.ToString());
                    //logger.WriteLogs(retValParam4.Value.ToString());
                    logger.WriteLogs("Backup LDAPEmployee OK!");
                }
                return true;
            }
            catch (Exception e)
            {
                transaction.Rollback();
                logger.WriteLogs("exeStoredProcedure[sp_LDAPBackupData] error, transaction is rollbacked!");
                logger.WriteLogs(e.Message + e.Source + e.StackTrace);
                return false;
            }
        }
		//將LDAPEmployee中email欄位空白者依照相同EmpID(取後8碼)的email來更新
        public static bool CtcbUpdEmailVerify()
        {
            try
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.Transaction = transaction;
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandText = "sp_LDAPUpdateEmail";
                    command.CommandTimeout = 900;
                    SqlParameter retValParam1 = command.Parameters.Add("@updNum", SqlDbType.Int);
                    retValParam1.Direction = ParameterDirection.Output;
                    command.ExecuteNonQuery();
                    logger.WriteLogs("exeStoredProcedure[sp_LDAPUpdateEmail] " + retValParam1.Value.ToString() + " successed!");
                }
                return true;
            }
            catch (Exception e)
            {
                transaction.Rollback();
                logger.WriteLogs("exeStoredProcedure[sp_LDAPUpdateEmail] error, transaction is rollbacked!");
                logger.WriteLogs(e.Message + e.Source + e.StackTrace);
                return false;
            }
        }

        //將LDAPEmployee中分行同仁的BranchID與BranchName欄位來更新
        public static bool CtcbUpdBranchVerify()
        {
            try
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.Transaction = transaction;
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandText = "sp_LDAPUpdateBranch";
                    command.CommandTimeout = 900;
                    command.Parameters.Add("@businessCategoryInput", SqlDbType.NVarChar, 100);
                    command.Parameters["@businessCategoryInput"].Value = searchBranch.Trim();
                    SqlParameter retValParam1 = command.Parameters.Add("@updNum", SqlDbType.Int);
                    retValParam1.Direction = ParameterDirection.Output;
                    command.ExecuteNonQuery();
                    logger.WriteLogs("exeStoredProcedure[sp_LDAPUpdateBranch] " + retValParam1.Value.ToString() + " successed!");
                }
                return true;
            }
            catch (Exception e)
            {
                transaction.Rollback();
                logger.WriteLogs("exeStoredProcedure[sp_LDAPUpdateBranch] error, transaction is rollbacked!");
                logger.WriteLogs(e.Message + e.Source + e.StackTrace);
                return false;
            }        
        }

        //刪除Table
        public static bool DelDBTable(string tbName)
        {
            try
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.Transaction = transaction;
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandText = "sp_TruncateTable";
                    command.CommandTimeout = 900;
                    command.Parameters.Clear();
                    command.Parameters.Add("@tabname", SqlDbType.VarChar).Value = tbName;
                    command.ExecuteNonQuery();
                    logger.WriteLogs("Delete Table : " + tbName + " Successed");
                }
                return true;
            }
            catch (Exception e)
            {
                transaction.Rollback();
                logger.WriteLogs("delete error,  transaction is rollbacked!");
                logger.WriteLogs(e.Message + e.Source + e.StackTrace);
                return false;
            }
        }

        //取得員工部門ID
        public static string GetDepID(string dn)
        {
            try
            {
                string rtn = "";
                string[] tdn = null;
                if (dn.Contains(','))
                {
                    tdn = dn.Split(',');
                    if (tdn.Length > 0)
                    {
                        if (tdn[1].ToString().Length > 3)
                        {
                            rtn = tdn[1].ToString().Substring(3, 9);//將DN去除第一個cn後,取第一個ou=U00012345的U0001234
                        }
                    }
                }
                return rtn;
            }
            catch (Exception ex)
            {
                logger.WriteLogs(dn);
                return "";
            }
        }

        //新增LDAPEmployee資料
        public static bool InsertToLDAPEmployee(LDAPEmployee emp)
        {
            string sqlstr = "insert into LDAPEmployee (EmpID,ManagerID,EmpName,EMail,DepDN,IsManager,DepID,EmpTitle,EmpBusinessCategory,TelNo,TelExt,CreatedUser,ModifiedUser) values(@EmpID,@ManagerID" +
                ",@EmpName,@EMail,@DepDN,@IsManager,@DepID,@EmpTitle,@EmpBusinessCategory,@TelNo,@TelExt,@CreatedUser,@ModifiedUser)";
            try
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.Transaction = transaction;
                    command.CommandType = CommandType.Text;
                    command.CommandText = sqlstr;
                    command.Parameters.Clear();
                    command.Parameters.Add("@EmpID", SqlDbType.NVarChar, 15).Value = emp.EmpID;
                    command.Parameters.Add("@ManagerID", SqlDbType.NVarChar, 500).Value = emp.ManagerID;
                    command.Parameters.Add("@EmpName", SqlDbType.NVarChar, 50).Value = emp.EmpName;
                    command.Parameters.Add("@EMail", SqlDbType.NVarChar, 100).Value = emp.EMail;
                    command.Parameters.Add("@DepDN", SqlDbType.NVarChar, 500).Value = emp.DepDN;
                    command.Parameters.Add("@IsManager", SqlDbType.NVarChar, 100).Value = emp.IsManager;
                    command.Parameters.Add("@DepID", SqlDbType.NVarChar, 50).Value = emp.DepID;
                    command.Parameters.Add("@EmpTitle", SqlDbType.NVarChar, 100).Value = emp.EmpTitle;
                    command.Parameters.Add("@EmpBusinessCategory", SqlDbType.NVarChar, 100).Value = emp.EmpBusinessCategory;
                    command.Parameters.Add("@TelNo", SqlDbType.NVarChar, 20).Value = emp.TelNo;
                    command.Parameters.Add("@TelExt", SqlDbType.NVarChar, 20).Value = emp.TelExt;
                    command.Parameters.Add("@CreatedUser", SqlDbType.NVarChar, 50).Value = emp.CreatedUser;
                    command.Parameters.Add("@ModifiedUser", SqlDbType.NVarChar, 50).Value = emp.ModifiedUser;
                    command.ExecuteNonQuery();
                }
                Console.WriteLine(emp.EmpID);
                return true;
            }
            catch (Exception e)
            {
                transaction.Rollback();
                logger.WriteLogs("exe[InsertToLDAPEmployee] error, transaction is rollbacked!");
                logger.WriteLogs(e.Message + e.Source + e.StackTrace);
                return false;
            }        
        }
        
        //新增LDAPManager資料
        public static bool InsertToLDAPManager(LDAPManager mgr)
        {
            string sqlstr = "insert into LDAPManager (EmpID,DepID,CreatedUser,ModifiedUser) values(@EmpID,@DepID,@CreatedUser,@ModifiedUser)";
            try
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.Transaction = transaction;
                    command.CommandType = CommandType.Text;
                    command.CommandText = sqlstr;
                    command.Parameters.Clear();
                    command.Parameters.Add("@EmpID", SqlDbType.NVarChar, 15).Value = mgr.EmpID;
                    command.Parameters.Add("@DepID", SqlDbType.NVarChar, 50).Value = mgr.DepID;
                    command.Parameters.Add("@CreatedUser", SqlDbType.NVarChar, 50).Value = mgr.CreatedUser;
                    command.Parameters.Add("@ModifiedUser", SqlDbType.NVarChar, 50).Value = mgr.ModifiedUser;
                    command.ExecuteNonQuery();
                }
                Console.WriteLine(mgr.EmpID);
                return true;
            }
            catch (Exception e)
            {
                transaction.Rollback();
                logger.WriteLogs("exe[InsertToLDAPManager] error, transaction is rollbacked!");
                logger.WriteLogs(e.Message + e.Source + e.StackTrace);
                return false;
            }        
        }

        //新增LDAPDepartment資料
        public static bool InsertToLDAPDepartment(LDAPDepartment dep)
        {
            string sqlstr = "insert into LDAPDepartment (DepID,DepName,DepDN,BusinessCategory,DepTitle,CtcbHrisOrgLevel,CtcbBankingID,CreatedUser,ModifiedUser) values(@DepID,@DepName,@DepDN,@BusinessCategory,@DepTitle,@CtcbHrisOrgLevel,@CtcbBankingID,@CreatedUser,@ModifiedUser)";
            try
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.Transaction = transaction;
                    command.CommandType = CommandType.Text;
                    command.CommandText = sqlstr;
                    command.Parameters.Clear();
                    command.Parameters.Add("@DepID", SqlDbType.NVarChar, 50).Value = dep.DepID;
                    command.Parameters.Add("@DepName", SqlDbType.NVarChar, 50).Value = dep.DepName;
                    command.Parameters.Add("@DepDN", SqlDbType.NVarChar, 500).Value = dep.DepDN;
                    command.Parameters.Add("@BusinessCategory", SqlDbType.NVarChar, 100).Value = dep.BusinessCategory;
                    command.Parameters.Add("@DepTitle", SqlDbType.NVarChar, 100).Value = dep.DepTitle;
                    command.Parameters.Add("@CtcbHrisOrgLevel", SqlDbType.NVarChar, 10).Value = dep.CtcbHrisOrgLevel;
                    command.Parameters.Add("@CtcbBankingID", SqlDbType.NVarChar, 10).Value = dep.CtcbBankingID;
                    command.Parameters.Add("@CreatedUser", SqlDbType.NVarChar, 50).Value = dep.CreatedUser;
                    command.Parameters.Add("@ModifiedUser", SqlDbType.NVarChar, 50).Value = dep.ModifiedUser;
                    command.ExecuteNonQuery();
                }
                Console.WriteLine(dep.DepID);
                return true;
            }
            catch (Exception e)
            {
                transaction.Rollback();
                logger.WriteLogs("exe[InsertToLDAPDepartment] error, transaction is rollbacked!");
                logger.WriteLogs(e.Message + e.Source + e.StackTrace);
                return false;
            }        
        }

        //查詢LDAPEmployee 資料for 進一步過濾出my boss
        public static List<LDAPEmployee> GetEmployeePrepareForBoss()
        {
            string sqlstr = "select EmpID,substring(DepDN,17,9) as DepDN,IsManager,DepID from LDAPEmployee with (nolock) ";
            List<LDAPEmployee> listEmpPreBoss = new List<LDAPEmployee>();
            try
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.Transaction = transaction;
                    command.CommandType = CommandType.Text;
                    command.CommandText = sqlstr;
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            listEmpPreBoss.Add(new LDAPEmployee
                            {
                                EmpID = reader[0].ToString(),
                                DepDN = reader[1].ToString(),
                                IsManager = reader[2].ToString(),
                                DepID = reader[3].ToString()
                            });
                        }
                    }
                }
                //Console.WriteLine(dep.DepID);
                return listEmpPreBoss;
            }
            catch (Exception e)
            {
                transaction.Rollback();
                logger.WriteLogs("exe[InsertToLDAPDepartment] error, transaction is rollbacked!");
                logger.WriteLogs(e.Message + e.Source + e.StackTrace);
                return listEmpPreBoss;
            }        
        }

        //將此同仁的ManagerID更新回LDAPEmployee.ManagerID
        public static bool UpdateLDAPEmployeeManagerID(LDAPEmployee emp)
        {
            string sqlstr = "update LDAPEmployee set ManagerID=@ManagerID where EmpID = @EmpID";
            try
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.Transaction = transaction;
                    command.CommandType = CommandType.Text;
                    command.CommandText = sqlstr;
                    command.Parameters.Clear();
                    command.Parameters.Add("@EmpID", SqlDbType.NVarChar, 15).Value = emp.EmpID;
                    command.Parameters.Add("@ManagerID", SqlDbType.NVarChar, 500).Value = emp.ManagerID;
                    command.ExecuteNonQuery();
                }
                Console.WriteLine(emp.DepID + " " + emp.ManagerID);
                return true;
            }
            catch (Exception e)
            {
                transaction.Rollback();
                logger.WriteLogs("exe[UpdateLDAPEmployeeManagerID()] error, transaction is rollbacked!");
                logger.WriteLogs(e.Message + e.Source + e.StackTrace);
                return false;
            }          
        }

        //同仁為=>主管,則抓取主管所在部門的上一層來判對主管的"老闆"的員編
        public static bool UpdateBossManagerID(LDAPEmployee emp)
        {
            bool rtn = false;
            string sqlstr = "select EmpID,DepID from LDAPManager with (nolock) where DepID=@DepID";
            string mgrList = "";
            List<LDAPManager> listEmpBoss = new List<LDAPManager>();
            try
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.Transaction = transaction;
                    command.CommandType = CommandType.Text;
                    command.CommandText = sqlstr;
                    command.Parameters.Clear();
                    command.Parameters.Add("@DepID", SqlDbType.NVarChar, 50).Value = emp.DepDN;//此時此欄為一個組織代號,非整串DN
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            listEmpBoss.Add(new LDAPManager
                            {
                                EmpID = reader[0].ToString(),
                                DepID = reader[1].ToString()
                            });
                        }
                    }
                }
                if (listEmpBoss != null) 
                {
                    foreach (LDAPManager mgr in listEmpBoss)
                    {
                        mgrList += mgr.EmpID + ",";
                    }
                    mgrList = mgrList.TrimEnd(',');
                    emp.ManagerID = mgrList;
                }
                //將此同仁的ManagerID更新回LDAPEmployee.ManagerID
                rtn = UpdateLDAPEmployeeManagerID(emp);
                return rtn;
            }
            catch (Exception e)
            {
                transaction.Rollback();
                logger.WriteLogs("exe[UpdateBossManagerID()] error, transaction is rollbacked!");
                logger.WriteLogs(e.Message + e.Source + e.StackTrace);
                return rtn;
            }        
        }

        //同仁為=>非主管,則以DepID抓取其部門主管
        public static bool UpdateStaffManagerID(LDAPEmployee emp)
        {
            bool rtn = false;
            string sqlstr = "select EmpID,DepID from LDAPManager with (nolock) where DepID=@DepID";
            string mgrList = "";
            List<LDAPManager> listEmpBoss = new List<LDAPManager>();
            try
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.Transaction = transaction;
                    command.CommandType = CommandType.Text;
                    command.CommandText = sqlstr;
                    command.Parameters.Clear();
                    command.Parameters.Add("@DepID", SqlDbType.NVarChar, 50).Value = emp.DepID;
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            listEmpBoss.Add(new LDAPManager
                            {
                                EmpID = reader[0].ToString(),
                                DepID = reader[1].ToString()
                            });
                        }
                    }
                }
                if (listEmpBoss != null)
                {
                    foreach (LDAPManager mgr in listEmpBoss)
                    {
                        mgrList += mgr.EmpID + ",";
                    }
                    mgrList = mgrList.TrimEnd(',');
                    emp.ManagerID = mgrList;                
                }

                //將此同仁的ManagerID更新回LDAPEmployee.ManagerID
                rtn = UpdateLDAPEmployeeManagerID(emp);
                return rtn;
            }
            catch (Exception e)
            {
                transaction.Rollback();
                logger.WriteLogs("exe[UpdateStaffManagerID()] error, transaction is rollbacked!");
                logger.WriteLogs(e.Message + e.Source + e.StackTrace);
                return rtn;
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
                string[] whoList = mailToWho.Split(delimiterChars);  //誰可收到批次執行狀況回報mail
                if (whoList.Length >= 1)
                {
                    MailAddress mailFromAdd = new MailAddress(mailFrom, mailFromDisplayName);
                    MailAddress mailToAdd;
                    MailMessage message;
                    SmtpClient client = new SmtpClient(smtpServer);
                    for (int i = 0; i < whoList.Length; i++)
                    {
                        mailToAdd = new MailAddress(whoList[i]);
                        message = new MailMessage(mailFromAdd, mailToAdd);
                        message.Subject = mailSubject;
                        message.IsBodyHtml = true;
                        message.Body = @"<html><head><title>CSFS LDAP Data Import</title></head><body><div style='font-size:13px;font-family:Verdana, Helvetica, Sans-Serif;'>" + logger.AllMessageForMail + @"</div></body></html>";
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
            catch (Exception ex)
            {
                logger.WriteLogs(ex.Message + " " + ex.StackTrace);
            }
        }
    }
}
