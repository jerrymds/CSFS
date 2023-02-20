using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Caching;
using System.Security.Cryptography;
using System.Text;

namespace CTCB.APLog
{

    public static class APLogUtil
    {
        public static readonly string SYSID_NUMS = "NUMS";
        public static readonly string SYSID_WII = "CSIW_WII";
        public static readonly string SYSID_SBRS = "SBRS";
        public static readonly string SYSID_CSFS = "CSFS";

        private static StackExchange.Redis.ConnectionMultiplexer _connection = null;
        private static object _lockObj = new object();

        #region Save Method
        public static void SaveAPLog(APLogVO vo, string systemID)
        {
            if (_connectionString == null)
            {
                return;
            }

            StackExchange.Redis.IDatabase db = Connection.GetDatabase();

            DateTime today = DateTime.Today;
            string systemIndex = GetSystemIndex(systemID, today);

            string date = today.ToString("yyyyMMdd");
            string value = Newtonsoft.Json.JsonConvert.SerializeObject(vo);
            //string value = System.Text.Json.JsonSerializer.Serialize(vo);

            string srcipt = string.Format(@"
                local systemIndex = '{0}'
                local result = redis.call('incr', systemIndex)
                local key = string.format('APLOG-%s-%s-%09d', KEYS[1], KEYS[2], result)
                redis.call('set', key, KEYS[3])
                return result", systemIndex);
            var result = (long)db.ScriptEvaluate(srcipt, new StackExchange.Redis.RedisKey[] { systemID, date, value });

        }
        #endregion Save Method

        #region Output Methtd
        public static int GetAPLogCount(string systemID, DateTime date)
        {
            string systemIndex = GetSystemIndex(systemID, date);
            StackExchange.Redis.IDatabase db = Connection.GetDatabase();
            var obj = db.StringGet(systemIndex);
            if (obj.IsNullOrEmpty)
            {
                return 0;
            }

            return (int)obj;
        }
        
        public static int AppendAPLogDetail(string systemID, DateTime date, ref StringBuilder sbBuffer, out int contentLength, out string message)
        {
            int apLogCount = 0;
            contentLength = 0;
            message = "";
            int initLength = sbBuffer.Length;

            try
            {
                CleanMemoryCache(systemID);

                StackExchange.Redis.IDatabase db = Connection.GetDatabase();

                int redisCount = GetAPLogCount(systemID, date);
                for (int i = 1; i <= redisCount; i++)
                {
                    string key = GetSystemKey(systemID, date, i);
                    try
                    {
                        var obj = db.StringGet(key);

                        if (obj.IsNullOrEmpty)
                        {
                            continue;
                        }

                        APLogVO vo = Newtonsoft.Json.JsonConvert.DeserializeObject<APLogVO>(obj);
                        //APLogVO vo = System.Text.Json.JsonSerializer.Deserialize<APLogVO>(obj);

                        #region 宣告變數
                        string System_Code = string.Empty;
                        string Login_Account_Nbr = string.Empty;
                        string Query_Datetime = string.Empty;
                        string AP_Txn_Code = string.Empty;
                        string Server_Name = string.Empty;
                        string User_Terminal = string.Empty;
                        string AP_Account_Nbr = string.Empty;
                        string Txn_Type_Code = string.Empty;
                        string Statement_Text = string.Empty;
                        string Object_Name = string.Empty;
                        string Txn_Status_Code = string.Empty;
                        string Customer_Id = string.Empty;
                        string Account_Nbr = string.Empty;
                        string Branch_Nbr = string.Empty;
                        string Role_Id = string.Empty;
                        string Import_Source = string.Empty;
                        string As_Of_Date = string.Empty;
                        #endregion

                        #region Assign變數
                        System_Code = systemID;
                        Login_Account_Nbr = vo.LogonUser;
                        Query_Datetime = vo.DataTimestamp.ToString("yyyy/MM/dd HH:mm:ss.fff");
                        AP_Txn_Code = vo.TxnCode;
                        Server_Name = "";
                        User_Terminal = vo.IP;
                        AP_Account_Nbr = "";
                        Txn_Type_Code = "";
                        Statement_Text = vo.Parameters ?? "";
                        Statement_Text = Statement_Text.TrimEnd('|');
                        Object_Name = "";
                        Txn_Status_Code = "";
                        Account_Nbr = "";
                        Branch_Nbr = "";
                        Role_Id = "";
                        Import_Source = systemID + "_APLOG";
                        As_Of_Date = date.ToString("yyyyMMdd");
                        #endregion

                        #region Replace逗號 20150828
                        Login_Account_Nbr = Login_Account_Nbr.Replace(',', '.');
                        AP_Txn_Code = AP_Txn_Code.Replace(',', '.');
                        User_Terminal = User_Terminal.Replace(',', '.');
                        Statement_Text = Statement_Text.Replace(',', '.');
                        #endregion

                        #region 欄位長度判斷
                        Login_Account_Nbr = Big5SubStr(Login_Account_Nbr, 0, 10);
                        AP_Txn_Code = Big5SubStr(AP_Txn_Code, 0, 30);
                        User_Terminal = Big5SubStr(User_Terminal, 0, 20);
                        Statement_Text = Big5SubStr(Statement_Text, 0, 1000);
                        #endregion

                        string[] cusIDs = vo.CusIDs.Split('|');
                        foreach (var cusID in cusIDs.Distinct())
                        {
                            if (string.IsNullOrWhiteSpace(cusID) 
                                || IsDuplicate(systemID, vo.TxnCode, vo.IP,vo.LogonUser,cusID,vo.DataTimestamp))
                            {
                                continue;
                            }

                            Customer_Id = Big5SubStr(cusID.Trim().Replace(',', '.'), 0, 14);

                            sbBuffer.Append(systemID + "," + Login_Account_Nbr + "," + Query_Datetime + "," + AP_Txn_Code + "," + Server_Name + "," + User_Terminal + "," + AP_Account_Nbr + "," + Txn_Type_Code + "," + Statement_Text + "," + Object_Name + "," + Txn_Status_Code + "," + Customer_Id + "," + Account_Nbr + "," + Branch_Nbr + "," + Role_Id + "," + Import_Source + "," + As_Of_Date + "\r\n");
                            apLogCount++;
                        }
                    }
                    catch (Exception ex)
                    {
                        message += string.Format("REDIS APLOG 處理 KEY={0} 時發生錯誤，錯誤原因：{1}", key, ex.ToString(), Environment.NewLine);
                    }
                }
            }
            catch (Exception ex)
            {
                message += string.Format("REDIS APLOG 處理時發生錯誤，錯誤原因：{0}", ex.ToString());
            }

            contentLength = sbBuffer.Length - initLength;
            return apLogCount;
        }

        /// <summary>
        /// 避免同一 systemID, txnCode, ip, logonUser, cusID 重覆傳送
        /// </summary>
        private static ConcurrentDictionary<string, MemoryCache> _cacheDic = new ConcurrentDictionary<string, MemoryCache>();
        private static MemoryCache GetMemoryCache(string systemID)
        {
            if (!_cacheDic.ContainsKey(systemID))
            {
                MemoryCache cache = new MemoryCache("APLOG_"+ systemID);
                _cacheDic[systemID] = cache;
            }
            return _cacheDic[systemID];
        }
        private static void CleanMemoryCache(string systemID)
        {
            if (_cacheDic.ContainsKey(systemID))
            {
                MemoryCache cache;
                if (_cacheDic.TryRemove(systemID, out cache))
                {
                    cache.Dispose();
                }
            }
        }

        public static bool IsDuplicate(string systemID, string txnCode,string ip, string logonUser, string cusID, DateTime dataTimestamp)
        {
            bool isDuplicate = true;

            MemoryCache cache = GetMemoryCache(systemID);
            DateTime lastDataTimestamp = DateTime.MinValue;
            string cacheKey = string.Format("##APLOG_{0}_{1}_{2}_{3}_{4}##", systemID, txnCode, ip, logonUser, cusID); 
            if (cache.Contains(cacheKey))
            {
                lastDataTimestamp = (DateTime)cache[cacheKey];
            }

            if (dataTimestamp.Subtract(lastDataTimestamp).TotalMinutes > 3)
            {
                isDuplicate = false;
                CacheItemPolicy cacheItemPolicy = new CacheItemPolicy() { AbsoluteExpiration = DateTime.Now.AddMinutes(10) };
                cache.Set(cacheKey, dataTimestamp, cacheItemPolicy);
            }

            return isDuplicate;
        }

        public static void CleanData(string systemID, DateTime date, out string message)
        {
            message = "";
            int CLEAN_DATA_PERIOD_DAY = 30;
            
            try
            {
                StackExchange.Redis.IDatabase db = Connection.GetDatabase();
                date = date.AddDays(-1 * _config.CleanDataBefore);
                for (int i = 0; i < CLEAN_DATA_PERIOD_DAY; i++)
                {
                    int redisCount = GetAPLogCount(systemID, date);
                    for (int j = 1; j <= redisCount; j++)
                    {
                        string key = GetSystemKey(systemID, date, j);
                        try
                        {
                            if (db.KeyExists(key))
                            {
                                db.KeyDelete(key);
                            }
                        }
                        catch (Exception ex)
                        {
                            message += string.Format("REDIS APLOG 刪除 KEY={0} 時發生錯誤，錯誤原因：{1}", key, ex.ToString(), Environment.NewLine);
                        }
                    }

                    string systemIndex = GetSystemIndex(systemID, date);
                    try
                    {
                        if (db.KeyExists(systemIndex))
                        {
                            db.KeyDelete(systemIndex);
                        }
                    }
                    catch (Exception ex)
                    {
                        message += string.Format("REDIS APLOG 刪除 KEY={0} 時發生錯誤，錯誤原因：{1}", systemIndex, ex.ToString(), Environment.NewLine);
                    }

                    date = date.AddDays(-1);
                }

            }
            catch (Exception ex)
            {
                message += string.Format("REDIS APLOG 刪除時發生錯誤，錯誤原因：{0}", ex.ToString());
            }
        }

        #endregion Output Methtd

        #region Helper Method 
        private static string _connectionString = null;
        
        private static Config _config = new Config()
        {
            CleanDataBefore = 3,
        };

        static APLogUtil()
        {
            var uri = new Uri(Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase));
            var fileMap = new ExeConfigurationFileMap { ExeConfigFilename = Path.Combine(uri.LocalPath, Assembly.GetExecutingAssembly().FullName.Split(',')[0] + ".dll.config") };
            var assemblyConfig = ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);
            if (assemblyConfig.HasFile)
            {
                AppSettingsSection section = (assemblyConfig.GetSection("appSettings") as AppSettingsSection);
                string value = section.Settings["RedisConnectionString"].Value;
                if (!string.IsNullOrWhiteSpace(value))
                {
                    value = AESDecrypt(value, "defaultkey");
                    SetRedisConnectionString(value);
                }

                value = section.Settings["CleanDataBefore"].Value;
                if (!string.IsNullOrWhiteSpace(value))
                {
                    _config.CleanDataBefore = System.Convert.ToInt32(value);
                }

            }
        }

        public static void SetRedisConnectionString(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                connectionString = null;
            }

            if (_connectionString== connectionString)
            {
                return;
            }
            lock (_lockObj)
            {
                if (_connection != null)
                {
                    _connection.Dispose();
                    _connection = null;
                }
                _connectionString = connectionString;
            }
        }

        public static void CheckRedisStatus()
        {
            if (_connectionString == null)
            {
                throw new Exception("\"RedisConnectionString\" is missing.");
            }


            try
            {
                if (!Connection.IsConnected)
                {
                    throw new Exception("Can not connect to Redis server.");
                }

                StackExchange.Redis.IDatabase db = Connection.GetDatabase();

                string testValue = Guid.NewGuid().ToString();
                db.StringSet("##APLOG_CHECK_FLAG##", testValue);
                string returrValue = db.StringGet("##APLOG_CHECK_FLAG##");
                if (testValue != returrValue)
                {
                    throw new Exception("Something wrong ablut Redis server.");
                }
            }
            catch (Exception)
            {
                SetRedisConnectionString(null);
                throw;
            }
        }

        private static StackExchange.Redis.ConnectionMultiplexer Connection
        {
            get
            {
                if (_connection == null)
                {
                    lock (_lockObj)
                    {
                        if (_connection == null)
                        {
                            _connection = StackExchange.Redis.ConnectionMultiplexer.Connect(_connectionString);
                        }
                    }
                }
                return _connection;
            }
        }

        private static string GetSystemIndex(string systemID, DateTime date)
        {
            return string.Format("APLOG-{0}-{1:yyyyMMdd}-INDEX", systemID, date);
        }
        private static string GetSystemKey(string systemID, DateTime date, int index)
        {
            return string.Format("APLOG-{0}-{1:yyyyMMdd}-{2:D9}", systemID, date, index);
        }

        private static string Big5SubStr(string a_SrcStr, int a_StartIndex, int a_Cnt)
        {
            Encoding l_Encoding = Encoding.GetEncoding("big5");
            byte[] l_byte = l_Encoding.GetBytes(a_SrcStr);
            if (a_Cnt <= 0)
                return "";
            //例若長度10 
            //若a_StartIndex傳入9 -> ok, 10 ->不行 
            if (a_StartIndex + 1 > l_byte.Length)
                return "";
            else
            {
                //若a_StartIndex傳入9 , a_Cnt 傳入2 -> 不行 -> 改成 9,1 
                if (a_StartIndex + a_Cnt > l_byte.Length)
                    a_Cnt = l_byte.Length - a_StartIndex;
            }

            if (a_StartIndex == 0 && a_Cnt < l_byte.Length)
            {
                string result = "";
                int resultLength = 0;
                for (int i = 0; i < a_SrcStr.Length; i++)
                {
                    string s = a_SrcStr.Substring(i, 1);
                    var bytes = l_Encoding.GetBytes(s);
                    resultLength += bytes.Length;
                    if (resultLength <= a_Cnt)
                    {
                        result += s;
                    }
                    else
                    {
                        break;
                    }
                }
                return result;
            }

            return l_Encoding.GetString(l_byte, a_StartIndex, a_Cnt);
        }

        private static string AESDecrypt(string DecryptString, string DecryptKey)    //APLog Redis ader 2022-01-01 - ADD
        {
            if (string.IsNullOrEmpty(DecryptString)) { return ""; }

            if (string.IsNullOrEmpty(DecryptKey)) { throw (new ArgumentNullException("DecryptKey")); }

            string m_strDecrypt = "";

            byte[] m_btIV = Convert.FromBase64String("Rkb4jvUy/ye7Cd7k89QQgQ==");

            Rijndael m_AESProvider = Rijndael.Create();

            try
            {
                byte[] m_btDecryptString = Convert.FromBase64String(DecryptString);

                MemoryStream m_stream = new MemoryStream();

                CryptoStream m_csstream = new CryptoStream(m_stream, m_AESProvider.CreateDecryptor(Encoding.Default.GetBytes(DecryptKey), m_btIV), CryptoStreamMode.Write);

                m_csstream.Write(m_btDecryptString, 0, m_btDecryptString.Length); m_csstream.FlushFinalBlock();

                m_strDecrypt = Encoding.Default.GetString(m_stream.ToArray());

                m_stream.Close(); m_stream.Dispose();

                m_csstream.Close(); m_csstream.Dispose();
            }
            catch (IOException ex) { throw ex; }
            catch (CryptographicException ex) { throw ex; }
            catch (ArgumentException ex) { throw ex; }
            catch (Exception ex) { throw ex; }
            finally { m_AESProvider.Clear(); }

            return m_strDecrypt;
        }
        #endregion Helper Method 

        #region Helper Class
        private class Config
        {
            public int CleanDataBefore { get; set; }
        }
        #endregion Helper Class
    }
}
