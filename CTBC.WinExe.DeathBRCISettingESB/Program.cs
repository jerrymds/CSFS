using CTBC.CSFS.BussinessLogic;
using CTBC.CSFS.Models;
using log4net;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTBC.WinExe.DeathBRCISettingESB
{
    class Program
    {
        
        private static string IsnotTest = ConfigurationManager.AppSettings["IsnotTest"].ToString();

        ILog log = LogManager.GetLogger("DebugLog");

        static void Main(string[] args)
        {
            Program mainProgram = new Program();
            mainProgram.Process(args);          
        }

        private void Process(string[] args)
        {
            log4net.Config.XmlConfigurator.Configure(new System.IO.FileInfo(AppDomain.CurrentDomain.BaseDirectory + @"\Log.config"));

            string thedate = null;
            if (args.Length==0)
                thedate = null;
            else
                thedate = args[0].ToString();
            

            BRCIBiz biz = new BRCIBiz();


            List<string> dCaseDetails = biz.getDeadVersion(thedate);


            foreach(var caseNo in dCaseDetails)
            {
                biz.WriteLog(string.Format( "\t==========  死亡註記 BRCI 發查開始案號{0} ==========", caseNo));
                #region 以案號來發查
                // 取得要發查的身份證字號
                List<CaseDeadDetail> dLists = biz.getDeathLists(caseNo);

                if (dLists == null || dLists.Count() == 0)
                {
                    biz.WriteLog("目前無資料可打BRCI");
                    // return 
                    // 20210409, 發現若當天有2筆, 而剛好第1筆沒有可打的筆數, 則會造成當天的第2筆沒有打
                    //      暫時解決方法: 將CaseDeadVersion.ModifiedDate='當天'
                    //      手動執行
                    //      執行完成後, 叫承辨人, 今日不要進新案件, 就不會重覆了
                    // 在20210526上版後, 就正常了
                    continue;
                }


                var groupNo = dLists.Where(x => !(x.CDBC_ID == "CIF無資料" || x.TX67050_Message == "戶名及生日不符")).GroupBy(x => x.CDBC_ID).Select(x => x.Key).OrderBy(x => x).ToList();
                //var groupNo = dLists.Where(x => (x.CDBC_ID != "CIF無資料" )).GroupBy(x => x.CDBC_ID).Select(x => x.Key).OrderBy(x => x).ToList();


                string AgentId = "Z0013771";
                string AgentBranch = "0495";
                // log 記錄
                biz.WriteLog("==========  死亡註記 BRCI 發查開始 ==========");

                try
                {

                    foreach (var grp in groupNo)
                    {
                        #region 每一個身份證號, 進行BRCI電文
                        var dListsItem = dLists.Where(x => x.CDBC_ID == grp).ToList();
                        string sRspCode = null;
                        var name = dListsItem.First();
                        // 讀取發查人... 用name.CaseDeadNewID 去AppMgrKey去找AgentId, AgentBranch..
                        string[] ldapInfo = new string[5];
                        ldapInfo = new ApprMsgKeyBiz().getLdapInfo(Guid.Parse(name.CaseDeadNewID));   // 找出承辦人的Racf, Ldap

                        if (!string.IsNullOrEmpty(ldapInfo[0]))
                        {
                            AgentId = ldapInfo[0];
                            AgentBranch = ldapInfo[4];
                        }

                        string TrnNum = string.Empty;
                        try
                        {
                            #region 逐筆設定



                            biz.WriteLog(string.Format("\t姓名 : {0}, Id : {1}", name.CDBC_NAME, name.CDBC_ID));

                            // 取得上行XML

                            string requestXml = biz.GenerateRequestXml(name, AgentId, AgentBranch, ref TrnNum);

                            // 上行XML若為空，則不再執行發查
                            if (requestXml == null)
                            {
                                biz.WriteLog("\t\t上行XML若為空, 繼續下一筆!!");
                                continue;
                            }

                            // 取得下行XML
                            string Sendresult = biz.SendESBData(name, TrnNum, requestXml, "ESB");

                            if (Sendresult != "")
                            {
                                // 解析下行XML  ... 
                                sRspCode = biz.ParseResponXMl(name, TrnNum, Sendresult);

                                // 如果有錯, 直接回錯
                                if (string.IsNullOrEmpty(sRspCode) || sRspCode == "|" || sRspCode.StartsWith("4444") )
                                {
                                    biz.WriteLog(string.Format("\t\tParsing XML錯誤\r\n ") + sRspCode);
                                }
                            }
                            else
                            {
                                // log 記錄
                                biz.WriteLog(" ==========  連接ESB服務器失敗 ");
                            }

                            #endregion
                        }
                        catch (Exception ex)
                        {
                            biz.WriteLog(string.Format("\t發生錯誤!! 姓名 : {0}, Id : {1} , 錯誤原因 : {2} ", name.CDBC_NAME, name.CDBC_ID, ex.Message.ToString()));
                        }
                        finally
                        {
                            #region 寫回CaseDeadDetail . BRCI_Status, BRCI_Message, TrnNum ...
                            //  發查成功, 20210203, 若是0000, 表示成功, 其他代碼, 都是失敗....
                            if (!string.IsNullOrEmpty(sRspCode))
                            {
                                // log 記錄
                                biz.WriteLog("\t\t發查結果， TrnNum=" + TrnNum + "\tRspCode = " + sRspCode + "\r\n");

                                // 查無資料時，將狀態更新完08代表案件處理完畢，因為更新為01之後，還要要獲取excel的明細會在更新成08
                                if (sRspCode.StartsWith("0000"))
                                {
                                    // 更新CaseDeadDetail...，為成功, BRCI_Status='Y'
                                    string sRspMsg = sRspCode.Split('|')[1];
                                    biz.updateCaseDeadDetailBRCI(name, TrnNum, "Y", sRspMsg);
                                }
                                else
                                {
                                    //若是9999, 表示失敗
                                    string sRspMsg = sRspCode.Split('|')[1];
                                    // 更新CaseDeadDetail...，為失敗, BRCI_Status='N'
                                    biz.updateCaseDeadDetailBRCI(name, TrnNum, "N", sRspMsg);
                                }
                            }
                            else
                            {
                                biz.WriteLog("==========    讀不到Respose Code 或非 0000 / 9999 " + TrnNum + "RspCode = " + sRspCode);
                                //string sRspMsg = sRspCode.Split('|')[1];
                                // 更新CaseDeadDetail...，為失敗, BRCI_Status='N'
                                biz.updateCaseDeadDetailBRCI(name, TrnNum, "N", "ESB 回傳XML錯誤");
                            }
                            #endregion
                        }
                        #endregion

                    } // end foreach(var grp in groupNo)

                    //再把戶名不符的.. 全部設為N
                    var nameNotMatch = dLists.Where(x => (x.CDBC_ID == "CIF無資料" || x.TX67050_Message == "戶名及生日不符")).GroupBy(x => x.CDBC_ID).Select(x => x.Key).OrderBy(x => x).ToList();
                    foreach (var grp in nameNotMatch)
                    {
                        var dListsItem = dLists.Where(x => x.CDBC_ID == grp).ToList();
                        var name = dListsItem.First();
                        biz.updateCaseDeadDetailBRCI(name, "", "N", "戶名及生日不符");
                    }
                }
                catch (Exception exAll)
                {
                    biz.WriteLog(string.Format("\t發生錯誤 {0}", exAll.Message.ToString()));
                } 

                #endregion
                biz.WriteLog(string.Format("\t==========  死亡註記 BRCI 發查結束案號{0} ==========", caseNo));
            }

            biz.WriteLog("==========  死亡註記 BRCI 發查結束 ==========");
        }



    }
}
