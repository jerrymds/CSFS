using CTBC.CSFS.BussinessLogic;
using CTBC.CSFS.Pattern;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Text;
using CTBC.CSFS.Models;
using System.Text.RegularExpressions;
using System.Linq;

namespace CTBC.WinExe.CSFS.ReturnFile
{
    class Program : BaseBusinessRule
    {
        #region 全局變量

        // HTG/RFDM文件路徑
        private static string txtFilePath = ConfigurationManager.AppSettings["txtFilePath"];

        // 獲取log路徑
        private static FileLog m_fileLog = new FileLog(ConfigurationManager.AppSettings["fileLog"], AppDomain.CurrentDomain.FriendlyName.Replace(".vshost", "").Replace(".exe", ""));

        // 半形空白變量
        public string strNull = " ";

        public DataTable CurrencyList = null;
        #endregion

        /// <summary>
        /// 程序入口
        /// </summary>
        static void Main()
        {
            Program mainProgram = new Program();

            // 產生回文TXT
            mainProgram.Process();

            // 產生回文PDF
            mainProgram.OutPutPDF();
        }

        /// <summary>
        /// 主方法
        /// </summary>
        private void Process()
        {
            // 獲取幣別代碼檔資料
            CurrencyList = GetParmCodeCurrency();

            // 程序開始記入log
            m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------回文開始------");

            // 判斷文件夾是否存在，若不存在，則創建
            if (!Directory.Exists(txtFilePath)) Directory.CreateDirectory(txtFilePath);

            // HTG回文
            ExportHtgTxt();

            // RFDM回文
            ExportRFDMTxt();

            // HTG發查回文產生成功 並且 RFDF回文產生成功，將Version狀態更新成成功或者重查成功
            ModifyVersionStatus1();

            // 更新案件主檔的狀態
            ModifyCaseStatus2();

            // 程序結束記入log
            m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------回文結束------");
        }

        #region HTG回文

        /// <summary>
        /// 產生HTG回文Txt
        /// </summary>
        public void ExportHtgTxt()
        {
            m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------匯出回文檔（存款帳戶開戶資料）開始------");

            // 查詢產生回文檔案的資料
            DataTable dtHTGResult = GetHTGData();

            // 判斷是否有HTG回文資料
            if (dtHTGResult != null && dtHTGResult.Rows.Count > 0)
            {
               DataTable dt401Recv = new DataTable();

               string DocNo = string.Empty;

                // 遍歷HTGResult,將資料寫進對應的txt文件中
                for (int p = 0; p < dtHTGResult.Rows.Count; p++)
                {
                   // 獲取存款帳戶開戶資料
                    dt401Recv = Get401RecvData(dtHTGResult.Rows[p]["CaseCustNewID"].ToString());

                    if (DocNo != dtHTGResult.Rows[p]["DocNo"].ToString())
                    {
                       string Filename = txtFilePath + "\\" + dtHTGResult.Rows[p]["ROpenFileName"].ToString();

                       if (File.Exists(Filename))
                       {
                          File.Delete(Filename);
                       }

                       DocNo = dtHTGResult.Rows[p]["DocNo"].ToString();
                    }

                    // HTG&RFDM內容及對應資料筆數變量
                    string fileContent1 = "";
                    int DataCount1 = 0;

                    if (dt401Recv != null && dt401Recv.Rows.Count > 0)
                    {
                        // 拼接存款帳戶開戶資料
                        for (int q = 0; q < dt401Recv.Rows.Count; q++)
                        {
                            #region 內容拼接
                            // 身分證統一編號
                            fileContent1 += ChangeValue(dt401Recv.Rows[q]["CUST_ID_NO"].ToString(), 10);

                            // 開戶行總、分支機構代碼
                            fileContent1 += ChangeValue(dt401Recv.Rows[q]["BRANCH_NO"].ToString(), 7);

                            // 存款種類	X(02)
                            string strPD_TYPE_DESC = ChangeValue(dt401Recv.Rows[q]["PD_TYPE_DESC"].ToString(), 2);
                            strPD_TYPE_DESC = (string.IsNullOrEmpty(strPD_TYPE_DESC.Trim()) ? "99" : strPD_TYPE_DESC);
                            fileContent1 += strPD_TYPE_DESC;

                            // 幣別 X(03)
                            fileContent1 += ChangeValue(dt401Recv.Rows[q]["CURRENCY"].ToString(), 3);

                            // 戶名  X(60)
                            fileContent1 += ChangeChiness(dt401Recv.Rows[q]["CUSTOMER_NAME"].ToString(), 60);

                            // 住家電話    X(20)
                            fileContent1 += ChangeValue(dt401Recv.Rows[q]["NIGTEL_NO"].ToString(), 20);

                            // 行動電話    X(20)
                            fileContent1 += ChangeValue(dt401Recv.Rows[q]["MOBIL_NO"].ToString(), 20);

                            // 戶籍地址    X(200)
                            fileContent1 += ChangeChiness(dt401Recv.Rows[q]["CUST_ADD"].ToString(), 200);

                            // 通訊地址    X(200)
                            fileContent1 += ChangeChiness(dt401Recv.Rows[q]["COMM_ADDR"].ToString(), 200);

                            // 帳號  X(20)
                            //20180622 RC 線上投單CR修正 宏祥 update start
                            //fileContent1 += ChangeAccount(dt401Recv.Rows[q]["ACCT_NO"].ToString(), 20, strPD_TYPE_DESC);
                            fileContent1 += ChangeAccount(dt401Recv.Rows[q]["ACCT_NO"].ToString(), 20);
                            //20180622 RC 線上投單CR修正 宏祥 update end

                            // 資料提供日期  X(08)
                            fileContent1 += ChangeValue(dtHTGResult.Rows[p]["HTGModifiedDate"].ToString(), 8);

                            // 開戶日 X(08)
                            fileContent1 += ChangeValue(dt401Recv.Rows[q]["OPEN_DATE"].ToString(), 8);

                            // 結清日 X(08)
                            fileContent1 += ChangeValue(dt401Recv.Rows[q]["CLOSE_DATE"].ToString(), 8);

                            // 資料提供日帳戶結餘   X(16)
                            string strCUR_BAL = dt401Recv.Rows[q]["CUR_BAL"].ToString();
                            strCUR_BAL = strCUR_BAL.Contains("-") ? strCUR_BAL : strCUR_BAL + "+";

                            fileContent1 += ChangeNumber(strCUR_BAL, 16, 2, true);

                            // 備註  X(300)
                            fileContent1 += ChangeValue("", 300);

                            #endregion

                            // 換行
                            fileContent1 += "\r\n";

                            DataCount1++;
                        }

                        // 將內容拼接到對應的txt文件中
                        AppendContent(dtHTGResult.Rows[p]["ROpenFileName"].ToString(), fileContent1, DataCount1);

                        // 記錄LOG
                        m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------向" + dtHTGResult.Rows[p]["ROpenFileName"].ToString() + "文件中增加" + DataCount1 + "筆資料!");
                    }
                    else
                    {
                      // 查無資料也要寫檔
                      // 身分證統一編號 + '與本行無存款往來'
                      fileContent1 += ChangeValue(dtHTGResult.Rows[p]["CustIdNo"].ToString() + "與本行無存款往來", 395) + "\r\n";

                      // 將內容拼接到對應的txt文件中
                      AppendContent(dtHTGResult.Rows[p]["ROpenFileName"].ToString(), fileContent1, 1);

                      // 記錄LOG
                      m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------向" + dtHTGResult.Rows[p]["ROpenFileName"].ToString() + "文件中增加1筆資料!");
                    }

                    // 更新HTGSendStatus狀態
                    int HTGCount = UpdateHTGSendStatus(dtHTGResult.Rows[p]["CaseCustNewID"].ToString());

                    // 記錄LOG
                    if (HTGCount > 0)
                    {
                        m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------更新CaseCustQueryVersion表CaseCustNewID=" + dtHTGResult.Rows[p]["CaseCustNewID"].ToString() + "的HTGSendStatus='99'");
                    }
                }
            }
            else
            {
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------沒有查到匯出回文檔（存款帳戶開戶資料）");
            }

            m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------匯出回文檔（存款帳戶開戶資料）結束------");
        }

        /// <summary>
        /// 查詢產生回文檔案的資料
        /// </summary>
        /// <returns></returns>
        public DataTable GetHTGData()
        {
            string sqlSelect = @"
                                select
                                  CaseCustQueryVersion.CustIdNo
                                  ,CaseCustQueryVersion.NewID as CaseCustNewID
                                  ,CONVERT(varchar(100),CaseCustQueryVersion.HTGModifiedDate, 112) as HTGModifiedDate
                                  ,CaseCustQuery.ROpenFileName
                                  ,CaseCustQuery.DocNo
                                from
                                  CaseCustQuery
                                left join
                                  CaseCustQueryVersion
                                on CaseCustQuery.NewID = CaseCustQueryVersion.CaseCustNewID
                                where
                                  OpenFlag = 'Y'
                                  and CaseCustQuery.Status in ('02','06')
                                  and CaseCustQueryVersion.CaseCustNewID not in (
                                    select 
                                      CaseCustQuery.NewID
                                    from
                                      CaseCustQuery
                                    left join
                                      CaseCustQueryVersion
                                    on CaseCustQuery.NewID = CaseCustQueryVersion.CaseCustNewID
                                    where
                                      CaseCustQuery.Status IN ('02', '06')
                                      and OpenFlag = 'Y'
                                      and CaseCustQueryVersion.HTGSendStatus not in ('4','99')
                                    group by
                                      CaseCustQuery.NewID
                                  )
                                order by
                                  CaseCustQuery.DocNo
                                  ,CaseCustQueryVersion.IdNo;

                              ";

            // 清空容器
            base.Parameter.Clear();

            return base.Search(sqlSelect);
        }

        /// <summary>
        /// 查詢HTG回文txt資料
        /// </summary>
        /// <param name="VersionNewID"></param>
        /// <returns></returns>
        public DataTable Get401RecvData(string VersionNewID)
        {
            string sqlSelect = @"
                                SELECT 
                                	BOPS000401Recv.CUST_ID_NO -- 統一編號
                                	,CASE 
                                        WHEN ISNULL(BOPS000401Recv.BRANCH_NO,'') <> '' THEN '822' + BOPS000401Recv.BRANCH_NO 
                                        ELSE ''
                                    END BRANCH_NO --分行別
                                	,isnull((select top 1 PARMCode.CodeNo
				                        from PARMCode
				                        where PARMCode.CodeType = 'PD_TYPE_DESC'
				                        and PARMCode.CodeMemo = BOPS000401Recv.PD_TYPE_DESC
										      order by PARMCode.CodeNo
			                        ),'99') as  PD_TYPE_DESC--產品別
                                	,BOPS000401Recv.CURRENCY --幣別
                                	,BOPS067050Recv.CUSTOMER_NAME --戶名
                                	,BOPS067050Recv.NIGTEL_NO --晚上電話
                                	,BOPS067050Recv.MOBIL_NO --手機號碼
                                	,isnull(BOPS067050Recv.POST_CODE,'') + ' ' 
                                	+isnull(BOPS067050Recv.COMM_ADDR1,'')
                                	+isnull(BOPS067050Recv.COMM_ADDR2,'')
                                	+isnull(BOPS067050Recv.COMM_ADDR3,'') as COMM_ADDR--通訊地址
                                	,isnull(BOPS067050Recv.ZIP_CODE,'') + ' ' + 
                                	isnull(BOPS067050Recv.CUST_ADD1,'') +
                                	isnull(BOPS067050Recv.CUST_ADD2,'') +
                                	isnull(BOPS067050Recv.CUST_ADD3,'') as CUST_ADD--戶籍/證照地址
                                	,BOPS000401Recv.ACCT_NO --帳號
                                	,CASE
                                      WHEN LEN(BOPS000401Recv.OPEN_DATE) > 0 
                                           and BOPS000401Recv.OPEN_DATE <>'00/00/0000'
                                      THEN substring(BOPS000401Recv.OPEN_DATE,7,4)
                                      + substring(BOPS000401Recv.OPEN_DATE,4, 2)
                                      +substring(BOPS000401Recv.OPEN_DATE,1, 2) ELSE '' END as OPEN_DATE --開戶日
                                	,CASE
                                		 WHEN LEN(BOPS000401Recv.CLOSE_DATE) > 0 
                                            and BOPS000401Recv.CLOSE_DATE <> '00/00/0000'
                                		 THEN substring(BOPS000401Recv.CLOSE_DATE,7,4)
                                		 + substring(BOPS000401Recv.CLOSE_DATE,4, 2)
                                		 +substring(BOPS000401Recv.CLOSE_DATE,1, 2) ELSE '' END as CLOSE_DATE --結清日
                                	,BOPS000401Recv.CUR_BAL --目前餘額
                                	,BOPS000401Recv.ACCT_NO --帳號
                                FROM BOPS000401Recv
                                LEFT JOIN BOPS067050Recv
                                    ON BOPS000401Recv.VersionNewID = BOPS067050Recv.VersionNewID
                                    --AND BOPS000401Recv.ACCT_NO = BOPS067050Recv.CIF_NO  --待客戶環境測試
                                LEFT JOIN CaseCustQueryVersion
                                 ON CaseCustQueryVersion.NewId = BOPS000401Recv.VersionNewID
                                WHERE BOPS000401Recv.VersionNewID = @VersionNewID
                                order by CaseCustQueryVersion.IdNo,BOPS000401Recv.CUST_ID_NO,OPEN_DATE ";

            // 清空容器
            base.Parameter.Clear();
            base.Parameter.Add(new CommandParameter("@VersionNewID", VersionNewID));

            return base.Search(sqlSelect);
        }

        /// <summary>
        /// 更新CaseCustQueryVersion表HTGSendStatus
        /// </summary>
        /// <param name="VersionNewID"></param>
        public int UpdateHTGSendStatus(string VersionNewID)
        {
            string sql = @"
                         Update CaseCustQueryVersion 
                         set HTGSendStatus = '99' 
                         where NewID = @NewID ";

            // 清空容器
            base.Parameter.Clear();
            base.Parameter.Add(new CommandParameter("@NewID", VersionNewID));

            return base.ExecuteNonQuery(sql);
        }
        #endregion

        #region RFDM回文
        public void ExportRFDMTxt()
        {
            m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------回文檔名稱（存款往來明細資料）開始------");
            // 查詢產生回文檔案的資料
            DataTable dtResult = GetRFDMData();

            string DocNo = string.Empty;

            if (dtResult != null && dtResult.Rows.Count > 0)
            {
               // 遍歷去除重複的資料
                for (int i = 0; i < dtResult.Rows.Count; i++)
                {
                   if (DocNo != dtResult.Rows[i]["DocNo"].ToString())
                   {
                      string Filename = txtFilePath + "\\" + dtResult.Rows[i]["RFileTransactionFileName"].ToString();

                      if (File.Exists(Filename))
                      {
                         File.Delete(Filename);
                      }

                      DocNo = dtResult.Rows[i]["DocNo"].ToString();
                   }

                   // 根據主檔主鍵獲取該案件下身分證統一編號資料
                    DataTable drRecvData = GetRFDMRecvData(dtResult.Rows[i]["CaseCustNewID"].ToString());

                    // RFDM內容及對應資料筆數變量
                    string fileContent2 = "";

                    // 遍歷並追加txt文件
                    if (drRecvData != null && drRecvData.Rows.Count > 0)
                    {
                        // 遍歷並追加txt文件
                        for (int j = 0; j < drRecvData.Rows.Count; j++)
                        {
                            #region 拼接內容
                            // 身分證統一編號 X(10)
                            fileContent2 += ChangeValue(dtResult.Rows[i]["CustIdNo"].ToString(), 10);

                            // 帳號  X(20)
                            //20180622 RC 線上投單CR修正 宏祥 update start
                            //fileContent2 += ChangeAccount(drRecvData.Rows[j]["ACCT_NO"].ToString(), 20, drRecvData.Rows[j]["PD_TYPE_DESC"].ToString());
                            fileContent2 += ChangeAccount(drRecvData.Rows[j]["ACCT_NO"].ToString(), 20);
                            //20180622 RC 線上投單CR修正 宏祥 update start

                            // 交易序號    9(08)
                            fileContent2 += ChangeValue(drRecvData.Rows[j]["JRNL_NO"].ToString(), 8);

                            // 交易日期    X(08)
                            fileContent2 += ChangeValue(drRecvData.Rows[j]["TRAN_DATE"].ToString(), 8);

                            // 交易時間    X(06)???
                            fileContent2 += ChangeValue(drRecvData.Rows[j]["JNRST_TIME"].ToString(), 6);

                            // 交易行(或所屬分行代號)    X(07)
                            fileContent2 += ChangeValue(drRecvData.Rows[j]["TRAN_BRANCH"].ToString(), 7);

                            // 交易摘要    X(40)
                            fileContent2 += ChangeChiness(drRecvData.Rows[j]["TXN_DESC"].ToString(), 40);

                            // 支出金額    X(16)
                            // 20180119,PeterHsieh : CTBC修改規格為，當金額為負數就放在支出金額，為正就放在存入金額，另一金額欄位就放'+0'
                            string strTRAN_AMT = drRecvData.Rows[j]["TRAN_AMT"].ToString();
                            strTRAN_AMT = strTRAN_AMT.Contains("-") ? strTRAN_AMT : "+0.00";

                            //20180622 RC 線上投單CR修正 宏祥 add start
                            string strTRAN_AMT2 = ChangeNumber(strTRAN_AMT, 16, 2, false);
                            fileContent2 += strTRAN_AMT2.Contains("-") ? strTRAN_AMT2.Replace("-", "+") : strTRAN_AMT2;
                            //20180622 RC 線上投單CR修正 宏祥 add start                            

                            // 存入金額    X(16)
                            // 20180119,PeterHsieh : CTBC修改規格為，當金額為負數就放在支出金額，為正就放在存入金額，另一金額欄位就放'+0'
                            string SaveAMT = drRecvData.Rows[j]["TRAN_AMT"].ToString();
                            SaveAMT = SaveAMT.Contains("-") ? "+0.00" : ("+" + SaveAMT);

                            fileContent2 += ChangeNumber(SaveAMT, 16, 2, false);

                            // 餘額  X(16)
                            string strBALANCE = drRecvData.Rows[j]["BALANCE"].ToString();
                            strBALANCE = strBALANCE.Contains("-") ? strBALANCE : "+" + strBALANCE;

                            fileContent2 += ChangeNumber(strBALANCE, 16, 2, false);

                            // 幣別  X(03)
                            string Currency = GetCurrency(drRecvData.Rows[j]["ACCT_NO"].ToString());

                            fileContent2 += Currency;

                            // ATM或端末機代號   X(20)
                            fileContent2 += ChangeValue(drRecvData.Rows[j]["ATM_NO"].ToString(), 20);

                            // 櫃員代號    X(20)
                            fileContent2 += ChangeValue(drRecvData.Rows[j]["TELLER"].ToString(), 20);

                            // 轉出入行庫代碼及帳號 (RFDM) TRF_BANK + TRF_ACCT  X(20)
                            fileContent2 += ChangeValue(drRecvData.Rows[j]["TRF_BANK"].ToString(), 20);

                            // 備註(RFDM) NARRATIVE X(300)
                            fileContent2 += ChangeChiness(drRecvData.Rows[j]["NARRATIVE"].ToString(), 300);
                            #endregion

                            // 換行
                            fileContent2 += "\r\n";
                        }

                        // 調用內容拼接方法 
                        AppendContent(dtResult.Rows[i]["RFileTransactionFileName"].ToString(), fileContent2, drRecvData.Rows.Count);

                        // 記錄LOG
                        m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------向" + dtResult.Rows[i]["RFileTransactionFileName"].ToString() + "文件中增加" + drRecvData.Rows.Count + "筆資料!");
                    }
                    else
                    {
                       // 查無資料也要寫檔
                       // 身分證統一編號 + '與本行無存款往來'
                       fileContent2 += ChangeValue(string.Format("{0}此區間無交易往來明細", dtResult.Rows[i]["CustIdNo"].ToString()), 400) + "\r\n";

                       // 將內容拼接到對應的txt文件中
                       AppendContent(dtResult.Rows[i]["RFileTransactionFileName"].ToString(), fileContent2, 1);

                       // 記錄LOG
                       m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------向" + dtResult.Rows[i]["RFileTransactionFileName"].ToString() + "文件中增加1筆資料!");
                    }

                    // 更新RFDMSendStatus狀態
                    int RFDMInt = UpdateRFDMSendStatus(dtResult.Rows[i]["CaseCustNewID"].ToString());

                    if (RFDMInt > 0)
                    {
                        // 記錄LOG
                        m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------更新CaseCustQueryVersion表RFDMSendStatus=" + dtResult.Rows[i]["CaseCustNewID"].ToString() + "的RFDMSendStatus='99'");
                    }
                }
            }
            else
            {
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------沒有查詢回文檔名稱（存款往來明細資料）");
            }

            m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------回文檔名稱（存款往來明細資料）結束------");
        }

        /// <summary>
        /// 查詢產生回文檔案的資料
        /// </summary>
        /// <returns></returns>
        public DataTable GetRFDMData()
        {
            string sqlSelect = @"
                                select
                                  CaseCustQueryVersion.CustIdNo
                                  ,CaseCustQueryVersion.NewID as CaseCustNewID
                                  ,CaseCustQuery.RFileTransactionFileName
                                  ,CaseCustQueryVersion.TransactionFlag
                                  ,CaseCustQueryVersion.RFDMSendStatus
                                  ,CaseCustQuery.DocNo
                                from
                                  CaseCustQuery
                                left join
                                  CaseCustQueryVersion
                                on CaseCustQuery.NewID = CaseCustQueryVersion.CaseCustNewID
                                where
                                  TransactionFlag = 'Y'
                                  and CaseCustQuery.Status in ('02','06')
                                  and CaseCustQueryVersion.CaseCustNewID not in (
                                    select 
                                      CaseCustQuery.NewID
                                    from
                                      CaseCustQuery
                                    left join
                                      CaseCustQueryVersion
                                    on CaseCustQuery.NewID = CaseCustQueryVersion.CaseCustNewID
                                    where
                                      CaseCustQuery.Status IN ('02', '06')
                                      and TransactionFlag = 'Y'
                                      and CaseCustQueryVersion.RFDMSendStatus not in ('8','99')
                                    group by
                                      CaseCustQuery.NewID
                                  )
                                order by
                                  CaseCustQuery.DocNo
                                  ,CaseCustQueryVersion.IdNo
                              ";

            // 清空容器
            base.Parameter.Clear();

            return base.Search(sqlSelect);
        }

        /// <summary>
        /// 判斷是'未開戶'或'無查詢區間資料'
        /// </summary>
        /// <returns></returns>
        public string CheckCustomer(string VersionNewID, string CustIdNo)
        {
           string desc = string.Empty;

           string sqlSelect = @"
                                SELECT BOPS067050Recv.CUSTOMER_NAME 
                                from BOPS067050Recv
                                where BOPS067050Recv.VersionNewID = @VersionNewID
                                and BOPS067050Recv.CUST_ID_NO = @CustIdNo
                              ";

           // 清空容器
           base.Parameter.Clear();
           base.Parameter.Add(new CommandParameter("@VersionNewID", VersionNewID));
           base.Parameter.Add(new CommandParameter("@CustIdNo", CustIdNo));

            DataTable dtResult =base.Search(sqlSelect);

            if (dtResult != null && dtResult.Rows.Count > 0)
            {
               desc = "此區間無交易往來明細";
            }
            else
            {
               desc = "與本行無存款往來";
            }

            dtResult = null;

            return desc;
        }

        /// <summary>
        /// 查詢HTG回文txt資料
        /// </summary>
        /// <param name="VersionNewID"></param>
        /// <returns></returns>
        public DataTable GetRFDMRecvData(string VersionNewID)
        {
            /*
             * 20180718,PeterHsieh : 配合 CR處理，修改 ATM或端末機代號欄位取得來源
             *      (ATM.交易日期 = RFDM.交易日期 and ATM.交易序號 = RFDM.交易序號 and ATM.交易時間 = RFDM.交易時間(前 6bytes)
             * 
             * 20180725,PeterHsieh : 調整如下
             *      01.交易序號 : 改取 CaseCustRFDMRecv.FISC_BANK(金資序號)，但若此欄位為空或'00000000'時，則取 CaseCustRFDMRecv.JRNL_NO(8 bytes)
             *      02.ATM或端末機代號 : 修改 Join條件
             *          (ATM.交易日期 = RFDM.交易日期 and ('0'+ATM.交易序號) = RFDM.金資序號)
             */

            string sqlSelect = @"
   with 
cr as
 (
                                                           SELECT 
                                    (Cast(IdNo as varchar)
	                                     + CustIdNo
	                                     + CaseCustRFDMRecv.ACCT_NO
	                                     + ISNULL(QDateS, '')
	                                     + ISNULL(QDateE, '')
                                    ) AS GroupId
                                    ,CaseCustRFDMRecv.ACCT_NO --帳號	X(20)
                                	,CASE WHEN ISNULL(CaseCustRFDMRecv.FISC_SEQNO, '') = '' OR (CaseCustRFDMRecv.FISC_SEQNO = '00000000')
                                        THEN RIGHT('00000000'+JRNL_NO,8)
                                        ELSE CaseCustRFDMRecv.FISC_SEQNO
                                    END AS JRNL_NO--交易序號	9(08)
                                	,CONVERT(nvarchar(8),TRAN_DATE,112 ) as TRAN_DATE--交易日期	X(08)
                                	,( select top 1 CASE WHEN ISNULL(CaseCustATMRecv.[YBTXLOG_TXN_HHMMSS],'') = ''  THEN '' ELSE SUBSTRING(CaseCustATMRecv.[YBTXLOG_TXN_HHMMSS],1,2)+SUBSTRING(CaseCustATMRecv.[YBTXLOG_TXN_HHMMSS],3,2)+SUBSTRING(CaseCustATMRecv.[YBTXLOG_TXN_HHMMSS],5,2) END
                                       from CaseCustATMRecv
                                       where CaseCustATMRecv.[YBTXLOG_YYYYMMDD] = CONVERT(Varchar, CaseCustRFDMRecv.TRAN_DATE, 112)
                                       and ('0'+CaseCustATMRecv.[YBTXLOG_STAND_NO]) = CaseCustRFDMRecv.FISC_SEQNO
                                       order by CaseCustATMRecv.CreatedDate DESC
                                    ) as ATM_TIME
                                    ,CASE WHEN ISNULL(JNRST_TIME, '') = ''
                                        THEN ''
                                        ELSE 
                                        CASE WHEN (JNRST_TIME = JRNL_NO OR JNRST_TIME = CONVERT(varchar, JNRST_DATE, 112))
                                            THEN ''
                                            ELSE
                                            --CASE WHEN TRY_CONVERT(datetime, SUBSTRING(JNRST_TIME,1,2)+':'+SUBSTRING(JNRST_TIME,3,2)+':'+SUBSTRING(JNRST_TIME,5,2)) IS NULL
                                                --THEN ''
                                                SUBSTRING(JNRST_TIME,1,6)
                                            --END
                                        END
                                    END AS JNRST_TIME --交易時間	X(06)
                                    ,CASE WHEN isnull(TRAN_BRANCH,'') <> '' 
                                        THEN '822' + isnull(TRAN_BRANCH,'') 
                                        ELSE ''
                                    END TRAN_BRANCH --交易行(或所屬分行代號)	X(07)
                                	,isnull(TXN_DESC,'') as TXN_DESC--交易摘要	X(40)
                                	,TRAN_AMT--支出金額	X(16)
                                	,TRAN_AMT*(-1) as SaveAMT--存入金額	X(16)
                                	,BALANCE --餘額	X(16)
                                	,(
                                       select top 1 CaseCustATMRecv.[YBTXLOG_SAFE_TMNL_ID]
                                       from CaseCustATMRecv
                                       where CaseCustATMRecv.[YBTXLOG_YYYYMMDD] = CONVERT(Varchar, CaseCustRFDMRecv.TRAN_DATE, 112)
                                       and ('0'+CaseCustATMRecv.[YBTXLOG_STAND_NO]) = CaseCustRFDMRecv.FISC_SEQNO
                                       order by CaseCustATMRecv.CreatedDate DESC
                                    ) as ATM_NO --ATM或端末機代號	X(20)
                                	,TELLER as TELLER --櫃員代號	X(20)
                                	,CASE WHEN CAST(isnull(TRF_ACCT,'0') AS NUMERIC) = 0
                                        THEN ''
                                        ELSE replace(replace(isnull(TRF_BANK,''),'448','822'),'000','822') + isnull(TRF_ACCT,'')
                                    END as TRF_BANK --轉出入行庫代碼及帳號 (RFDM)TRF_BANK+TRF_ACCT
                                	,isnull(NARRATIVE,'') as NARRATIVE  --備註 (RFDM) NARRATIVE
                                    ,CASE WHEN ISNULL(JRNL_NO,'') = ''
                                        THEN ''
                                        ELSE isnull(
										    (select top 1 PARMCode.CodeNo
											    from PARMCode
											    where PARMCode.CodeType = 'PD_TYPE_DESC'
											    and PARMCode.CodeMemo = BOPS000401Recv.PD_TYPE_DESC
										    order by PARMCode.CodeNo
										    ),'99') 
		                            END as PD_TYPE_DESC --產品別
                                FROM 
                                    CaseCustRFDMRecv 
                                LEFT JOIN CaseCustQueryVersion ON CaseCustQueryVersion.NewID = CaseCustRFDMRecv.VersionNewID
                                LEFT JOIN CaseCustQuery ON CaseCustQuery.NewID = CaseCustQueryVersion.CaseCustNewID
                                LEFT JOIN BOPS000401Recv ON BOPS000401Recv.VersionNewID = CaseCustRFDMRecv.VersionNewID
	                                    and BOPS000401Recv.ACCT_NO = CaseCustRFDMRecv.ACCT_NO
                                WHERE 
                                    CaseCustRFDMRecv.VersionNewID = @VersionNewID
)

select GroupId,ACCT_NO,JRNL_NO,TRAN_DATE,TRAN_BRANCH ,TXN_DESC,TRAN_AMT,SaveAMT,BALANCE,ATM_NO,TELLER,TRF_BANK,NARRATIVE,PD_TYPE_DESC,
(CASE WHEN ISNULL(ATM_TIME,'')<>'' THEN ATM_TIME
              ELSE JNRST_TIME END) as JNRST_TIME from cr ORDER BY GroupId,TRAN_DATE,JNRST_TIME,JRNL_NO
                        ";

            // 清空容器
            base.Parameter.Clear();
            base.Parameter.Add(new CommandParameter("@VersionNewID", VersionNewID));

            return base.Search(sqlSelect);
        }

        /// <summary>
        /// 更新CaseCustQueryVersion表RFDMSendStatus
        /// </summary>
        /// <param name="VersionNewID"></param>
        public int UpdateRFDMSendStatus(string strNewID)
        {
            string sql = @"
                         Update CaseCustQueryVersion 
                         set RFDMSendStatus = '99' 
                         where NewID = @NewID ";

            // 清空容器
            base.Parameter.Clear();
            base.Parameter.Add(new CommandParameter("@NewID", strNewID));

            return base.ExecuteNonQuery(sql);
        }
        #endregion

        #region 更新案件主檔的狀態和Version狀態更新成成功或者重查成功

        /// <summary>
        /// HTG發查回文產生成功 並且 RFDF回文產生成功，將Version狀態更新成成功或者重查成功
        /// </summary>
        public void ModifyVersionStatus1()
        {
            try
            {
                // 將發查中的案件狀態更新為【成功】
                string sql = @"
UPDATE CaseCustQueryVersion SET Status = '03' WHERE  EXISTS
(
SELECT NEWID
 FROM
 (
	SELECT NEWID from CaseCustQueryVersion WHERE OpenFlag = 'Y' AND HTGSendStatus = '99' AND (TransactionFlag IS NULL OR TransactionFlag <> 'Y') AND Status = '02'
	UNION
	SELECT NEWID from CaseCustQueryVersion WHERE TransactionFlag = 'Y' AND RFDMSendStatus = '99' AND (OpenFlag IS NULL OR OpenFlag <> 'Y') AND Status = '02'
	UNION
	SELECT NEWID from CaseCustQueryVersion WHERE OpenFlag = 'Y' AND HTGSendStatus = '99' AND TransactionFlag = 'Y' AND RFDMSendStatus = '99' AND Status = '02'
)RESULT
WHERE CaseCustQueryVersion.NewID = RESULT.NewID
); ";
                // 將重查拋查中的案件狀態更新為【重查成功】
                sql += @"
UPDATE CaseCustQueryVersion SET Status = '07' WHERE  EXISTS
(
SELECT NEWID
 FROM
 (
	SELECT NEWID from CaseCustQueryVersion WHERE OpenFlag = 'Y' AND HTGSendStatus = '99' AND (TransactionFlag IS NULL OR TransactionFlag <> 'Y') AND Status = '06'
	UNION
	SELECT NEWID from CaseCustQueryVersion WHERE TransactionFlag = 'Y' AND RFDMSendStatus = '99' AND (OpenFlag IS NULL OR OpenFlag <> 'Y') AND Status = '06'
	UNION
	SELECT NEWID from CaseCustQueryVersion WHERE OpenFlag = 'Y' AND HTGSendStatus = '99' AND TransactionFlag = 'Y' AND RFDMSendStatus = '99' AND Status = '06'
)RESULT
WHERE CaseCustQueryVersion.NewID = RESULT.NewID
)
";
                base.ExecuteNonQuery(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 更新案件主檔的狀態
        /// </summary>
        public void ModifyCaseStatus2()
        {
            // 查詢發查中的案件主檔
            DataTable dtSuccessSend = GetSuccessSend();

            if (dtSuccessSend != null && dtSuccessSend.Rows.Count > 0)
            {
                for (int i = 0; i < dtSuccessSend.Rows.Count; i++)
                {
                    // 案件編號狀態為發查中，並且version檔的資料全部發查成功，將主檔狀態更新為03
                    if (dtSuccessSend.Rows[i]["VERSIONCOUNT"].ToString() == dtSuccessSend.Rows[i]["CASECOUNT"].ToString()
                        && dtSuccessSend.Rows[i]["Status"].ToString() == "02")
                    {
                        UpdateCaseQueryStatus("03", dtSuccessSend.Rows[i]["CaseCustNewID"].ToString());
                    }
                    // 案件編號狀態為重查拋查中，並且version檔的資料全部發查成功，將主檔狀態更新為07[重查成功]
                    else if (dtSuccessSend.Rows[i]["VERSIONCOUNT"].ToString() == dtSuccessSend.Rows[i]["CASECOUNT"].ToString()
                        && dtSuccessSend.Rows[i]["Status"].ToString() == "06")
                    {
                        UpdateCaseQueryStatus("07", dtSuccessSend.Rows[i]["CaseCustNewID"].ToString());
                    }
                }
            }
        }

        /// <summary>
        /// 查詢Version檔成功的案件筆數，和案件下應該發查的資料筆數
        /// </summary>
        /// <returns></returns>
        public DataTable GetSuccessSend()
        {
            try
            {
                string strDataSql = @"
SELECT 
	RESULT.VERSIONCOUNT, RESULT.Status, CaseCustNewID,
	(SELECT COUNT(CaseCustNewID) FROM  CaseCustQueryVersion WHERE CaseCustQueryVersion.CaseCustNewID = RESULT.CaseCustNewID  ) AS CASECOUNT
FROM
(
	SELECT CaseCustNewID , CaseCustQuery.Status, COUNT(CaseCustNewID) AS VERSIONCOUNT
	from CaseCustQueryVersion
	INNER JOIN CaseCustQuery	
		ON  CaseCustQueryVersion.CaseCustNewID = CaseCustQuery.NewID
		AND CaseCustQuery.Status IN ('02', '06') -- 主檔案件狀態為發查中
	WHERE CaseCustQueryVersion.Status = '03' OR CaseCustQueryVersion.Status = '07' --Version狀態為發查成功或重查成功
	GROUP BY CaseCustQueryVersion.CaseCustNewID, CaseCustQuery.Status
)RESULT
                    ";

                return base.Search(strDataSql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 更新案件主檔的狀態
        /// </summary>
        /// <param name="Status"></param>
        /// <param name="NewID"></param>
        /// <returns></returns>
        public int UpdateCaseQueryStatus(string Status, string NewID)
        {
            try
            {
                string sql = @"UPDATE CaseCustQuery SET Status = @Status  WHERE  NewID = @NewID ";

                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@Status", Status));
                base.Parameter.Add(new CommandParameter("@NewID", NewID));

                return base.ExecuteNonQuery(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        #region 共用自定義方法

        /// <summary>
        /// 將符合條件的內容拼接到指定txt文件中
        /// </summary>
        /// <param name="FileName">指定txt文件</param>
        /// <param name="Content">拼接的內容</param>
        /// <param name="DataCount">拼接資料筆數</param>
        public void AppendContent(string FileName, string Content, int DataCount)
        {
            // 資料筆數>0時,向對應的txt文件追加內容
            if (DataCount > 0)
            {
                // 文件路徑
                string filePath = txtFilePath + "\\" + FileName;

                #region 向指定文件追加TXT內容

                // 判斷文件是否存在，若不存在，則創建
                if (!File.Exists(filePath))
                {
                    FileStream cFile = File.Create(filePath);
                    cFile.Dispose();
                    cFile.Close();
                }

                // 記錄相關信息
                FileStream p_FS = new FileStream(filePath, FileMode.Append);

                StreamWriter p_SW = new StreamWriter(p_FS, System.Text.Encoding.GetEncoding("UTF-8"));

                DateTime time = DateTime.Now;

                // 追加拼接內容
                p_SW.Write(Content);

                // 關閉資料流
                p_SW.Close();
                p_FS.Dispose();
                p_FS.Close();
                #endregion
            }
        }

        /// <summary>
        /// 字串轉換,長度不夠右側補空白
        /// </summary>
        /// <param name="strValue">指定字串</param>
        /// <param name="strValueLen">指定長度</param>
        /// <returns></returns>
        public string ChangeValue(string strValue, int strValueLen)
        {
            if (!string.IsNullOrEmpty(strValue))
            {
                // 差值 = 字串指定長度-字串實際長度
                int strNullNumber = strValueLen - strValue.Length;

                // strNullNumber>0,就在字串後拼接strNullNumber個半形空白,否則就截取指定長度
                strValue = strNullNumber > 0 ? strValue + AddSpace(strNullNumber, strNull) : strValue.Substring(0, strValueLen);
            }
            else
            {
                // 拼接strValueLen個半形空白
                strValue += AddSpace(strValueLen, strNull);
            }
            return strValue;
        }

        /// <summary>
        /// 補充指定長度的空格
        /// </summary>
        /// <param name="strSpaceLen">指定長度</param>
        /// <returns></returns>
        public string AddSpace(int strSpaceLen, string flag)
        {
            string result = "";

            // 拼接strNullNumber個半形空白
            for (int m = 0; m < strSpaceLen; m++)
            {
                result += flag;
            }

            return result;
        }

        /// <summary>
        /// 根據帳號中的標誌位獲取幣別
        /// </summary>
        /// <param name="strValue"></param>
        /// <returns></returns>
        public string GetCurrency(string strValue)
        {
            string strCurrency = "";

            if (!string.IsNullOrEmpty(strValue))
            {
                // 截取標誌位
                string strFlag = strValue.Length > 4 ? strValue.Substring(0, 4) : "";

                // strFlag != "0000",標誌位爲帳號的後3位
                strFlag = strFlag == "0000" ? strFlag : strValue.Substring(strValue.Length - 3, 3);

                // 獲取幣別
                DataRow[] dr = CurrencyList.Select("CodeNo ='" + strFlag + "'");
                strCurrency = dr != null && dr.Length > 0 ? dr[0]["CodeDesc"].ToString() : "";

                // 如果幣別爲空,就用空白代替
                if (string.IsNullOrEmpty(strCurrency) || strCurrency.Length < 3)
                {
                    strCurrency = strCurrency + AddSpace(3 - strCurrency.Length, strNull);
                }
                else
                {
                    strCurrency = strCurrency.Substring(0, 3);
                }
            }
            else
            {
                strCurrency = AddSpace(3, strNull);
            }

            return strCurrency;
        }

        //20180622 RC 線上投單CR修正 宏祥 update start
        /// <summary>
        /// 帳號轉換,長度不夠右側補空白
        /// </summary>
        /// <param name="strValue">帳號</param>
        /// <param name="strValueLen">長度</param>
        /// <param name="strPD_TYPE_DESC">產品別</param>
        /// <returns></returns>
        public string ChangeAccount(string strValue, int strValueLen)
        {
           // 帳號
           string strAccount = "";

           if (!string.IsNullOrEmpty(strValue))
           {
              // 截取標誌位
              string strFlag = strValue.Length > 4 ? strValue.Substring(0, 4) : "";

              // strFlag == "0000",帳號爲strValue後12位,否則爲前幾位
              if (strFlag == "0000")
              {
                 // 截取帳號
                 strAccount = strValue.Substring(4, strValue.Length - 4);
              }
              else
              {
                 // 截取帳號
                 strAccount = strValue.Substring(1, strValue.Length - 4);

                 // 截取標誌位
                 //strFlag = strValue.Substring(strValue.Length - 3, 3);
              }

              // 獲取幣別
              //DataRow[] dr = CurrencyList.Select("CodeNo ='" + strFlag + "'");
              //string strCurrency = dr != null && dr.Length > 0 ? dr[0]["CodeDesc"].ToString() : "";
              // 如果幣別爲空,就用空白代替
              //if (string.IsNullOrEmpty(strCurrency) || strCurrency.Length < 3)
              //{
              //   strCurrency = strCurrency + AddSpace(3 - strCurrency.Length, strNull);
              //}
              //else
              //{
              //   strCurrency = strCurrency.Substring(0, 3);
              //}

              // 拼接帳號和幣別
              //strAccount = strAccount + strCurrency;
           }

           // 拼接產品別
           //strAccount += "-" + strPD_TYPE_DESC;

           // 指定長度-字串長度
           int strNullNumber = strValueLen - strAccount.Length;

           // strNullNumber>0,就在字串後拼接strNullNumber個半形空白,否則就截取指定長度
           strAccount = strNullNumber > 0 ? strAccount + AddSpace(strNullNumber, strNull) : strAccount.Substring(0, strValueLen);

           return strAccount;
        }

        //public string ChangeAccount(string strValue, int strValueLen, string strPD_TYPE_DESC)
        //{
        //    // 帳號
        //    string strAccount = "";

        //    if (!string.IsNullOrEmpty(strValue))
        //    {
        //        // 截取標誌位
        //        string strFlag = strValue.Length > 4 ? strValue.Substring(0, 4) : "";

        //        // strFlag == "0000",帳號爲strValue後12位,否則爲前幾位
        //        if (strFlag == "0000")
        //        {
        //            // 截取帳號
        //            strAccount = strValue.Substring(4, strValue.Length - 4);
        //        }
        //        else
        //        {
        //            // 截取帳號
        //            strAccount = strValue.Substring(0, strValue.Length - 3);

        //            // 截取標誌位
        //            strFlag = strValue.Substring(strValue.Length - 3, 3);
        //        }

        //        // 獲取幣別
        //        DataRow[] dr = CurrencyList.Select("CodeNo ='" + strFlag + "'");
        //        string strCurrency = dr != null && dr.Length > 0 ? dr[0]["CodeDesc"].ToString() : "";
        //        // 如果幣別爲空,就用空白代替
        //        if (string.IsNullOrEmpty(strCurrency) || strCurrency.Length < 3)
        //        {
        //            strCurrency = strCurrency + AddSpace(3 - strCurrency.Length, strNull);
        //        }
        //        else
        //        {
        //            strCurrency = strCurrency.Substring(0, 3);
        //        }

        //        // 拼接帳號和幣別
        //        strAccount = strAccount + strCurrency;
        //    }

        //    // 拼接產品別
        //    strAccount += "-" + strPD_TYPE_DESC;

        //    // 指定長度-字串長度
        //    int strNullNumber = strValueLen - strAccount.Length;

        //    // strNullNumber>0,就在字串後拼接strNullNumber個半形空白,否則就截取指定長度
        //    strAccount = strNullNumber > 0 ? strAccount + AddSpace(strNullNumber, strNull) : strAccount.Substring(0, strValueLen);

        //    return strAccount;
        //}
        //20180622 RC 線上投單CR修正 宏祥 update end

        /// <summary>
        /// 獲取 幣別代碼檔
        /// </summary>
        /// <returns></returns>
        public DataTable GetParmCodeCurrency()
        {
            string sqlSelect = @" select CodeNo,CodeDesc from PARMCode where  CodeType='CaseCust_CURRENCY' ";

            // 清空容器
            base.Parameter.Clear();
            return base.Search(sqlSelect);
        }

        /// <summary>
        /// 轉換含有正負號字串
        /// </summary>
        /// <param name="strValue">指定字串</param>
        /// <param name="strValueLen">指定長度</param>
        /// <param name="floatLen">小數位數</param>
        /// <param name="flag">true:正負號在最後一位,false:正負號在第一位</param>
        /// <returns></returns>
        public string ChangeNumber(string strValue, int strValueLen, int floatLen, bool flag)
        {
            string strResult = "";

            // 判斷是否有值,沒有值固定顯示"+000000000000000"
            if (!string.IsNullOrEmpty(strValue))
            {
                // 正負號變量
                string strLastFlag = "";

                // 正負號在最後一位
                if (flag)
                {
                    // 截取正負號
                    strLastFlag = strValue.Substring(strValue.Length - 1, 1);

                    // 去掉正負號
                    strValue = strValue.Substring(0, strValue.Length - 1);
                }
                else
                {
                    strLastFlag = strValue.Substring(0, 1);

                    // 去掉正負號
                    strValue = strValue.Substring(1, strValue.Length - 1);
                }

                // 判斷是否有值,如果沒有值,就在正負號後追加半形空白
                if (strValue != "")
                {
                    #region 獲取小數位
                    // 獲取小數點位置
                    int dotIndex = strValue.IndexOf('.');

                    // 截取小數位
                    string strFloat = strValue.Substring(dotIndex + 1);

                    // 判斷小數位長度,如果大於指定長度,就截取指定長度,否則在右邊補充0
                    int floatNum = floatLen - strFloat.Length;
                    strFloat = floatNum > 0 ? strFloat + AddSpace(floatNum, "0") : strFloat.Substring(0, floatLen);

                    #endregion

                    #region 獲取整數位
                    // 截取整數位
                    string strInt = strValue.Substring(0, dotIndex);

                    // 計算正整數最大長度
                    int intMaxLength = strValueLen - floatLen - 1;

                    // 判斷整數位長度,如果大於指定長度,就截取指定長度,否則在左邊補充0
                    int inttNum = intMaxLength - strInt.Length;
                    strInt = inttNum > 0 ? AddSpace(inttNum, "0") + strInt : strInt.Substring(0, intMaxLength);

                    #endregion

                    strResult = strLastFlag + strInt + strFloat;
                }
                else
                {
                    strResult = strLastFlag + AddSpace(strValueLen - 1, "0");
                }
            }
            else
            {
                strResult = "+" + AddSpace(strValueLen - 1, "0");
            }
            return strResult;
        }

        /// <summary>
        /// 用Byte截長度
        /// </summary>
        /// <param name="a_SrcStr"></param>
        /// <param name="a_Cnt"></param>
        /// <returns></returns>
        public string ChangeChiness(string strValue, int strLength)
        {
            string strResult = "";

            if (!string.IsNullOrEmpty(strValue))
            {
                Encoding l_Encoding = Encoding.GetEncoding("big5");
                byte[] l_byte = l_Encoding.GetBytes(strValue);

                strResult = l_byte.Length > strLength ? l_Encoding.GetString(l_byte, 0, strLength) : l_Encoding.GetString(l_byte, 0, l_byte.Length) + AddSpace(strLength - l_byte.Length, strNull);

                strResult = strResult.Replace("?", strNull);
            }
            else
            {
                strResult = AddSpace(strLength, strNull);
            }

            return strResult;
        }
        #endregion

        #region 產生回文文檔

        /// <summary>
        /// 列印案件的PDF檔案
        /// </summary>
        public void OutPutPDF()
        {
            // 查詢要列印PDF的案件編號(一個案件下有多個人，只有有一個人HTG、RFDM發查成功就產出PDF,歷史記錄功能用)
            DataTable dtPDFList = GetQueryPDFList();

            if (dtPDFList != null && dtPDFList.Rows.Count > 0)
            {
                ExportReportPDF _ExportReportPDF = new ExportReportPDF(m_fileLog);

                #region 獲取回文用到的代碼

                DataTable dtPARMCode = GetPARMCode();
                DataTable dtSendSettingRef = GetSendSettingRef();

                string Address = dtPARMCode.Rows.Count > 0 ? dtPARMCode.Select("CodeNo = 'Address'")[0].ItemArray[0].ToString() : "";
                string ButtomLine = dtPARMCode.Rows.Count > 0 ? dtPARMCode.Select("CodeNo = 'ButtomLine'")[0].ItemArray[0].ToString() : "";
                string ButtomLine2 = dtPARMCode.Rows.Count > 0 ? dtPARMCode.Select("CodeNo = 'ButtomLine2'")[0].ItemArray[0].ToString() : "";
                string Speed = dtPARMCode.Rows.Count > 0 ? dtPARMCode.Select("CodeNo = 'Speed'")[0].ItemArray[0].ToString() : "";
                string Security = dtPARMCode.Rows.Count > 0 ? dtPARMCode.Select("CodeNo = 'Security'")[0].ItemArray[0].ToString() : "";

                #endregion

                for (int i = 0; i < dtPDFList.Rows.Count; i++)
                {
                    string strPDFReturn = "";

                    dtPDFList.Rows[i]["Address"] = Address;
                    dtPDFList.Rows[i]["ButtomLine"] = ButtomLine;
                    dtPDFList.Rows[i]["ButtomLine2"] = ButtomLine2;
                    dtPDFList.Rows[i]["Speed"] = Speed;
                    dtPDFList.Rows[i]["Security"] = Security;
                    dtPDFList.Rows[i]["Subject"] = dtSendSettingRef.Rows[0]["Subject"].ToString();
                    dtPDFList.Rows[i]["Description"] = dtSendSettingRef.Rows[0]["Description"].ToString();

                    // 案件狀態為成功，PDF需要產生第一頁
                    if (dtPDFList.Rows[i]["Status"].ToString() == "03" || dtPDFList.Rows[i]["Status"].ToString() == "07")
                    {
                        // 查詢發文字號
                       string strSendNoNow = string.Empty;

                       if (string.IsNullOrEmpty(dtPDFList.Rows[i]["MessageNo"].ToString()))
                       {
                          // 首次取號
                          strSendNoNow = QuerySendNoNow(dtPDFList.Rows[i]["NewID"].ToString());
                       }
                       else
                       {
                          // 原案件編號重查，使用原回文文號，不重新取號
                          strSendNoNow = dtPDFList.Rows[i]["MessageNo"].ToString();
                       }

                        if (strSendNoNow != "")
                        {
                            strPDFReturn = _ExportReportPDF.SavePDF(dtPDFList.Rows[i], "Y", strSendNoNow);
                        }
                        else
                        {
                            // 將狀態更新為沒有發文字號
                            UpdatePDFStatus(dtPDFList.Rows[i]["NewID"].ToString(), "W", "77");
                        }
                    }
                    else
                    {
                        // 案件狀態不為成功，PDF不產生第一頁
                        strPDFReturn = _ExportReportPDF.SavePDF(dtPDFList.Rows[i], "N", "");
                    }

                    // 主檔的狀態為成功代表整個案件下所有人都發查成功，則更新PDF匯出狀態，下次不再產出PDF
                    if ((dtPDFList.Rows[i]["Status"].ToString() == "03" || dtPDFList.Rows[i]["Status"].ToString() == "07"
                        || dtPDFList.Rows[i]["Status"].ToString() == "66") && strPDFReturn == "Y")
                    {
                        // 將案件PDF產出狀態更新成Y
                        UpdatePDFStatus(dtPDFList.Rows[i]["NewID"].ToString(), "Y", "");
                    }
                }
            }
        }

        /// <summary>
        /// 查詢要列印PDF的案件編號
        /// </summary>
        /// <returns></returns>
        public DataTable GetQueryPDFList()
        {
            string sqlSelect = @"          
                              SELECT
                               CaseCust.NewID
                              , CaseCust.Status
                              , CaseCustQuery.Recipient AS ComeFile -- '受文者'
                              , CaseCustQuery.DocNo --   系統案件編號
                              , CaseCustQuery.ROpenFileName --附件1
                              , CaseCustQuery.RFileTransactionFileName --附件2
                              , LDAPEmployee.EmpID --承辦人ID
                              , LDAPEmployee.EmpName  --承辦人名字
                              , ISNULL(LDAPEmployee.TelNo, '') AS TelNo --電話
                              , ISNULL(LDAPEmployee.TelExt, '') AS TelExt --分機
                              , CaseCustQuery.LetterDeptName --   來文機關
                              , CaseCustQuery.LetterDeptNo --   來文機關代碼
                              , CaseCustQuery.LetterDate --   來文日期
                              , CaseCustQuery.LetterNo --來文字號
                              , '' AS Address -- 地址
                              , '' AS ButtomLine 
                              , '' AS ButtomLine2
                              , '' AS Speed -- 速別
                              , '' AS Security -- 密等
                              , '' AS Subject -- 主旨，內容
                              , '' AS Description -- 主旨，內容
                              , CaseCustQueryVersion.CustIdNo + 
                               case 
	                              when  ISNULL(CaseCustQueryVersion.countID, 0) > 0 then '等'
                              else  '' 
                              end CustIdNo  --S12XXXXX49等
                              , '' AS Remark -- 1061106 高院偵字第10637396號
                              , CaseCustQuery.Version --版本號
                              , CaseCustQuery.InCharge AS InCharge -- '承辦人員'
                              , CaseCustQuery.MessageNo AS MessageNo -- '原回文文號'
                              FROM 
                              (
	                              SELECT 
		                              CaseCustQuery.NewID, CaseCustQuery.Status 
	                              FROM CaseCustQueryVersion 
	                              INNER JOIN CaseCustQuery
		                              ON CaseCustQueryVersion.CaseCustNewID = CaseCustQuery.NewID
	                              WHERE (CaseCustQuery.PDFStatus IS NULL OR CaseCustQuery.PDFStatus = 'N') 
		                              AND CaseCustQueryVersion.Status <> '01' 
		                              AND CaseCustQueryVersion.Status <> '02' 
		                              AND CaseCustQueryVersion.Status <> '06'
	                              GROUP BY CaseCustQuery.NewID, CaseCustQuery.Status
                              ) CaseCust
                              INNER JOIN CaseCustQuery
	                              ON CaseCust.NewID = CaseCustQuery.NewID
                              LEFT JOIN LDAPEmployee
	                              ON CaseCustQuery.QueryUserID = LDAPEmployee.EmpID
                              LEFT JOIN 
                              (
	                              SELECT CaseCustNewID,  count(CaseCustNewID) AS countID, MAX(CustIdNo) AS CustIdNo FROM CaseCustQueryVersion 
	                              GROUP BY CaseCustNewID
                              ) CaseCustQueryVersion
                              ON CaseCustQuery.NewID = CaseCustQueryVersion.CaseCustNewID
                              ORDER BY CaseCustQuery.DocNo,CaseCustQuery.Version
                            ";

            // 清空容器
            base.Parameter.Clear();
            return base.Search(sqlSelect);
        }

        public DataTable GetPARMCode()
        {
            string sqlSelect = @" 
                                SELECT CodeDesc,CodeNo FROM PARMCode WHERE CodeType = 'REPORT_SETTING' 
                            ";

            // 清空容器
            base.Parameter.Clear();
            return base.Search(sqlSelect);
        }

        public DataTable GetSendSettingRef()
        {
            string sqlSelect = @" 
                                SELECT Subject, Description FROM SendSettingRef WHERE CaseKind = '外來文案件' AND CaseKind2 = '外來文回文'
                            ";

            // 清空容器
            base.Parameter.Clear();
            return base.Search(sqlSelect);
        }

        /// <summary>
        /// 將PDF列印狀態更新到系統中
        /// </summary>
        /// <param name="VersionNewID"></param>
        /// <param name="strPDFStatus">W: 沒有回文編號</param>
        /// <param name="strStatus">77：未獲取回文字號</param>
        /// <returns></returns>
        public int UpdatePDFStatus(string VersionNewID, string strPDFStatus, string strStatus)
        {
            string sql = @" Update CaseCustQuery 
                         set PDFStatus = @PDFStatus";

            if (strStatus != "")
            {
                sql += @" ,Status = '77' ";
            }

            sql += @" where NewID = @NewID ";

            // 清空容器
            base.Parameter.Clear();
            base.Parameter.Add(new CommandParameter("@NewID", VersionNewID));
            base.Parameter.Add(new CommandParameter("@PDFStatus", strPDFStatus));

            return base.ExecuteNonQuery(sql);
        }

        public string QuerySendNoNow(string strNewID)
        {
            string sqlSelect = @" 
                                DECLARE @SendNoId BIGINT;
                                DECLARE @SendNoNow BIGINT
                                SELECT @SendNoId = SendNoId, @SendNoNow = SendNoNow + 1 FROM SendNoTable WHERE SendNoYear = left(convert(varchar,getdate(),21),4)
                                AND SendNoNow < SendNoEnd
                                ORDER BY SendNoId ASC
                                UPDATE SendNoTable SET SendNoNow = @SendNoNow WHERE SendNoId = @SendNoId
                                SELECT @SendNoNow    as SendNoNow
                              ";

            DataTable dt = base.Search(sqlSelect);

            if (dt != null && dt.Rows.Count > 0)
            {
                // 20180326,PeteHsieh : 規格修改成，將取得的文號中[單位代碼]移除(4~9碼)
                Regex regex = new Regex(Regex.Escape("24839"));
                string NewSendNo = regex.Replace(dt.Rows[0]["SendNoNow"].ToString(), string.Empty, 1);

                string sql = @"UPDATE CaseCustQuery SET MessageNo = @SendNoNow, MessageDate = GETDATE() WHERE NewID = @NewID";

                // 清空容器
                base.Parameter.Clear();

                base.Parameter.Add(new CommandParameter("@SendNoNow", NewSendNo));
                base.Parameter.Add(new CommandParameter("@NewID", strNewID));

                base.ExecuteNonQuery(sql);

                return NewSendNo;
            }
            else
            {
                return "";
            }

        }

        #endregion
    }
}