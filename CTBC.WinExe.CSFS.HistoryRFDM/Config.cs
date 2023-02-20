using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.IO;
using CTBC.CSFS.Resource;

namespace CTBC.WinExe.CSFS.HistoryRFDM
{
    public class Config
    {
        ///<summary>
        /// 連線參數 
        ///</summary>
        static public string ConnectionString
        {
            get
            {
                object connectionString = ConfigurationManager.ConnectionStrings["CSFS_ADO"];

                return connectionString == null ? "" : connectionString.ToString();
            }
        }

        ///<summary>
        /// 預設連線參數 
        ///</summary>
        static public string DefaultConnect
        {
            get
            {
                return "server=127.0.0.1;uid=csfsuser;pwd=csfsuser;database=CSFS";
            }
        }

        /// <summary>
        /// 取得WebConfig中設定資料--清單每頁顯示筆數
        /// </summary>
        /// <returns></returns>
        public static int GetPerPageRows()
        {
            return Convert.ToInt32(ConfigurationManager.AppSettings["RowsPerPage"]);
        }

        /// <summary>
        /// 取得系統設定資料--獲取WebConfig信息
        /// </summary>
        /// <returns></returns>
        public static string GetValue(string name)
        {
            #region--20130201暫無使用
            //if (name.ToUpper().Contains("PASSWORD"))
            //{
            //    return AESDecrypt(ConfigurationManager.AppSettings[name], ConfigurationManager.AppSettings["key"]);
            //}
            #endregion
            return ConfigurationManager.AppSettings[name];
        }

        /// <summary>
        /// AES 解密
        /// </summary>
        /// <param name="DecryptString">待解密密文</param>
        /// <param name="DecryptKey">解密密钥</param>
        /// <returns></returns>
        private static string AESDecrypt(string DecryptString, string DecryptKey)
        {
            if (string.IsNullOrEmpty(DecryptString)) { return ""; }

            if (string.IsNullOrEmpty(DecryptKey)) { throw (new Exception(Lang.csfs_err_empty_pwd)); }

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
        public static string ReportServerURL
        {
            get
            {
                return ConfigurationManager.AppSettings["ReportServerURL"];
            }
        }

        public static string ReportServerDomain
        {
            get
            {
                return ConfigurationManager.AppSettings["ReportServerDomain"];
            }
        }

        public static string ReportServerUser
        {
            get
            {
                return ConfigurationManager.AppSettings["ReportServerUser"];
            }
        }

        public static string ReportServerPassword
        {
            get
            {
                return ConfigurationManager.AppSettings["ReportServerPassword"];
            }
        }

        public static string CUF_UserLogoutURL
        {
            get
            {
                return ConfigurationManager.AppSettings["CUF_UserLogoutURL"];
            }
        }

        public static string WAS_URL
        {
            get
            {
                return ConfigurationManager.AppSettings["WAS_URL"];
            }
        }

        public static string Mail_FromEmail
        {
            get
            {
                return ConfigurationManager.AppSettings["Mail_FromEmail"];
            }
        }

        public static string Mail_FromName
        {
            get
            {
                return ConfigurationManager.AppSettings["Mail_FromName"];
            }
        }

        public static string Mail_EmailServer
        {
            get
            {
                return ConfigurationManager.AppSettings["Mail_EmailServer"];
            }
        }

        public static int Mail_EmailPort
        {
            get
            {
                return Convert.ToInt32(ConfigurationManager.AppSettings["Mail_EmailPort"]);
            }
        }

        /// <summary>
        /// Added by smallzhi
        /// </summary>
        public static string ConfigServices_URL
        {
            get
            {
                return ConfigurationManager.AppSettings["ConfigServices_URL"];
            }
        }

        /// <summary>
        /// 20130314 Tom
        /// </summary>
        public static string ConfigServicesAPRL_URL
        {
            get
            {
                return ConfigurationManager.AppSettings["ConfigServicesAPRL_URL"];
            }
        }

        /// <summary>
        /// Added by smallzhi
        /// </summary>
        public static int UDWRApproveLayer
        {
            get
            {
                return Convert.ToInt32(ConfigurationManager.AppSettings["UDWRApproveLayer"]);
            }
        }

        /// <summary>
        /// Added by smallzhi
        /// </summary>
        public static int LayerMinNum
        {
            get
            {
                return Convert.ToInt32(ConfigurationManager.AppSettings["LayerMinNum"]);
            }
        }

        /// <summary>
        /// Added by smallzhi
        /// </summary>
        public static int NeedMultiCOAmt
        {
            get
            {
                return Convert.ToInt32(ConfigurationManager.AppSettings["NeedMultiCOAmt"]);
            }
        }

        /// <summary>
        /// Added by smallzhi
        /// </summary>
        public static int NeedCreditCOAmt
        {
            get
            {
                return Convert.ToInt32(ConfigurationManager.AppSettings["NeedCreditCOAmt"]);
            }
        }

        /// <summary>
        /// Added by smallzhi
        /// </summary>
        public static int LimitMonth
        {
            get
            {
                int result = 0;
                if (int.TryParse(ConfigurationManager.AppSettings["LimitMonth"], out result))
                {
                    result = Convert.ToInt32(ConfigurationManager.AppSettings["LimitMonth"]);
                }
                else
                {
                    result = 0;
                }

                result = -result;
                return result;
            }
        }

        /// <summary>
        /// Added by smallzhi
        /// </summary>
        public static System.Collections.Generic.List<int> LayerIsNeedCreditCO
        {
            get
            {
                System.Collections.Generic.List<int> returnvalue = new System.Collections.Generic.List<int>();

                string[] temp = ConfigurationManager.AppSettings["LayerIsNeedCreditCO"].ToString().Split(',');

                for (int i = 0; i < temp.Length; i++)
                {
                    returnvalue.Add(Convert.ToInt32(temp[i]));
                }
                return returnvalue;
            }
        }

        /// <summary>
        /// Added by smallzhi,For test
        /// </summary>
        public static string CurrentCO
        {
            get
            {
                return ConfigurationManager.AppSettings["CurrentCO"];
            }
        }

        /// <summary>
        /// Added by smallzhi,For test
        /// </summary>
        public static string Flow
        {
            get
            {
                return ConfigurationManager.AppSettings["Flow"];
            }
        }

        /// <summary>
        /// Added by smallzhi,For test
        /// </summary>
        public static string AuditResult
        {
            get
            {
                return ConfigurationManager.AppSettings["AuditResult"];
            }
        }

        /// <summary>
        /// Added by horace 20130201
        /// </summary>
        public static string FileNETLibraryName
        {
            get
            {
                return ConfigurationManager.AppSettings["FileNETLibraryName"];
            }
        }

        /// <summary>
        /// Added by horace 20130201
        /// </summary>
        public static string FileNETLogonUserID
        {
            get
            {
                return ConfigurationManager.AppSettings["FileNETLogonUserID"];
            }
        }

        /// <summary>
        /// Added by horace 20130201
        /// </summary>
        public static string FileNETLogonPwd
        {
            get
            {
                return ConfigurationManager.AppSettings["FileNETLogonPwd"];
            }
        }
    }
}
