using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using CTCB.NUMS.ConfigEncrypt;

namespace EncryptAppConfig
{
    public class Program
    {
        static Log logger = new Log(ConfigurationManager.AppSettings["LogPath"].ToString());
        public static void Main(string[] args)
        {            
            logger.WriteLogs("--開始 加解密 App.Config--");
            try
            {
                string argment = "";
                string action = "";
                int argsLength = args.Length;
                if (argsLength == 0)
                {
                    //執行檔*.exe與*.exe.Config一定要在同目錄下
                    Console.WriteLine("請輸入欲加解密的應用程式名稱 (格式範例:CTCB.NUMS.Batch.LDAPSync.exe或CTCB.NUMS.Service.WebLog.dll)");
                    argment = Console.ReadLine();
                    Console.WriteLine("---請輸入處理 代碼---");
                    Console.WriteLine("輸入 1 可加解密 connectionStrings");
                    Console.WriteLine("輸入 2 可加解密 appSettings");
                    Console.WriteLine("輸入 3 可同時加解密 connectionStrings 與 appSettings");
                    Console.WriteLine("-----------------------");
                    action = Console.ReadLine();
                }
                else
                {
                    action = args[0].ToString();
                    argment = args[1].ToString();
                }
                logger.WriteLogs("你輸入的應用程式名稱:" + argment);
                logger.WriteLogs("你輸入的 處理 代碼:" + action);
                //ToggleConfigEncryption(argment);
                EncryptConfig eypt = new EncryptConfig();
                switch (action)
                {
                    case "1":
                        eypt.ConnectionStringsEncryption(argment);
                        break;
                    case "2":
                        eypt.AppSettingsEncryption(argment);
                        break;
                    case "3":
                        eypt.ConnectionStringsEncryption(argment);
                        eypt.AppSettingsEncryption(argment);
                        break;
                    default:
                        break;
                }
                if (action == "1" || action == "2" || action == "3" )
                {
                    WriteLog("完成加解密!!");
                }
                else
                    WriteLog("你輸入錯誤的處理 代碼!!");
            }
            catch (Exception ex)
            {
                WriteLog("你可能輸入錯誤的應用程式名稱格式 或 錯誤的處理 代碼!");
                WriteLog("");
                WriteLog("錯誤訊息如下:");
                WriteLog("");
                WriteLog(ex.Message);
                WriteLog(ex.StackTrace);
                WriteLog("");
            }
            finally
            {
                logger.WriteLogs("--結束 加解密 App.Config--");
                Console.WriteLine("按 Enter 離開此項作業...");
                Console.ReadLine();            
            }
        }

        public static void WriteLog(string message)
        {
            logger.WriteLogs(message);
            Console.WriteLine(message);
        }
    }
}
