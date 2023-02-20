using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using CTBC.CSFS.BussinessLogic;
using CTBC.FrameWork.Util;
using CTBC.CSFS.Models;
using System.IO;
using System.Data;


namespace CTBC.CSFS.BackupHistory
{
    class Program
    {
        //public string _taskname = "CTBC.WinExe.CSFS.BackupHistory";

        HistoryDBBIZ _HistoryDBBIZ = new HistoryDBBIZ();

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            bool isDebug = bool.Parse(ConfigurationManager.AppSettings["Debug"].ToString());
            Program mainProgram = new Program();
            // log.config
            log4net.Config.XmlConfigurator.Configure(new System.IO.FileInfo(AppDomain.CurrentDomain.BaseDirectory + @"\Log.config"));

            if (!isDebug)
            {
                mainProgram.Process_New();
            }
            else
            {
                mainProgram.testTimeOut();
            }
        }

        private void testTimeOut()
        {
            _HistoryDBBIZ.WriteLog("目前進行測試.... : " );
            try {
                Console.WriteLine("測試資料, 開始時間: " + DateTime.Now);
                
                var aaa = _HistoryDBBIZ.getHugeDate();
                _HistoryDBBIZ.WriteLog("測試資料, 共計: " + aaa.Rows.Count.ToString());

                Console.WriteLine("測試資料, 共計: " + aaa.Rows.Count.ToString());
                aaa = null;
                Console.WriteLine("測試資料, 結束時間: " + DateTime.Now);
                _HistoryDBBIZ.WriteLog("\r\n\r\n");      
            }
            catch (Exception ex)
            {
                _HistoryDBBIZ.WriteLog(string.Format("測試資料, 發生錯誤, 錯誤訊息: {0}", ex.Message.ToString()));
                _HistoryDBBIZ.WriteLog("\r\n\r\n");      
            }
        }


        /// <summary>
        /// 新版20201228, 支援以CaseMaster的建檔日期為主, 搬每個TABLE, 要考濾以下條件
        /// 1. 若扣押金額為0時, 直接搬走
        /// 2. 若扣押金額為0時, 已經搬走了, 但今天又來一件撒銷, 則, 承辦人會補一件扣押案, 然後就扣押跟撒銷一起搬
        /// 3. 若扣押金額>0 時,且已有支付或撒銷案, 已結案, 則搬走..
        /// </summary>
        private void Process_New()
        {
            #region 資料轉檔開始




            DateTime sDate = new DateTime(2015, 1, 1);
            DateTime eDate = DateTime.Now;

            bool isDelete = false;

            _HistoryDBBIZ.WriteLog(string.Format("開始歷史轉檔作業 ============================", sDate.ToShortDateString(), eDate.ToShortDateString()));


            //var tables = _HistoryDBBIZ.getHistory_BackupSetting();

            var GrouperEnabled = _HistoryDBBIZ.getEnabledGroupNo();


            // 針對GroupNo=0 跟GroupNo=1分開處理
            foreach (DataRow g in GrouperEnabled.Rows)
            {
                if (g[0].ToString() == "0")
                {
                    #region 處理GroupNo=0的
                    try
                    {
                        var tables = _HistoryDBBIZ.getTablesByGroupNo(g[0].ToString());
                        for (int i = 0; i < tables.Count(); i++)
                        {
                            #region 計算搬動的區間
                            // 20201016, 計算 eDate ....
                            DateTime thenow = DateTime.Now;
                            eDate = thenow;
                            switch (tables[i].TimeFreq.ToUpper())
                            {
                                case "D":
                                    eDate = thenow.AddDays(-1 * tables[i].TimeFreqValue);
                                    break;
                                case "M":
                                    eDate = thenow.AddMonths(-1 * tables[i].TimeFreqValue);
                                    break;
                                case "Y":
                                    eDate = thenow.AddYears(-1 * tables[i].TimeFreqValue);
                                    break;
                            }

                            //20201118, 新增計算啟始區間.... 由上次執行的結果
                            //因為上次存在LastBackupDate是包含上次已經搬過的區間, 所以開始日期要加一天
                            //sDate = tables[i].LastBackupDate == new DateTime(0000,1,1) ? new DateTime(2015, 1, 1) : tables[i].LastBackupDate.AddDays(1);
                            if (tables[i].LastBackupDate < new DateTime(2015, 1, 1))
                                sDate = new DateTime(2015, 1, 1);
                            else
                                sDate = tables[i].LastBackupDate;


                            _HistoryDBBIZ.WriteLog(string.Format("搬動的區間為 {0} ~ {1}", sDate.ToShortDateString(), eDate.ToShortDateString()));

                            #endregion


                            string source = tables[i].TableName;
                            string dest = "History_" + source;
                            string dateField = tables[i].DateField.ToString();
                            List<string> ignoreFields = new List<string>() { tables[i].IgnoreField1, tables[i].IgnoreField2, tables[i].IgnoreField3 };


                            if (tables[i].isMove) // 搬Group0
                            {
                                MoveGroup0(sDate, eDate, source, dest, dateField, ignoreFields);
                            }

                            if (tables[i].isDelete) // 刪Group0
                            {
                                DeleteGroup0(sDate, eDate, source, dateField);
                            }

                            //20201118, 若執行完成, 要把本次的eDate, 寫回LastBackupDate 這個欄位
                            _HistoryDBBIZ.updateLastBackupDate(tables[i], eDate);
                        }
                    }
                    catch (Exception ex)
                    {
                        _HistoryDBBIZ.WriteLog("程式異常，錯誤信息：" + ex.Message);
                    }

                    #endregion
                }
                else if (g[0].ToString() == "1")
                {
                    #region 20201203, 處理原來GroupNo=1的
                    try
                    {
                        var tables = _HistoryDBBIZ.getTablesByGroupNo(g[0].ToString());

                        if (tables.Count == 0)
                        {
                            _HistoryDBBIZ.WriteLog("====> ERROR : 歷史轉檔清單參數未設定");
                            return;
                        }

                        _HistoryDBBIZ.WriteLog(string.Format("\t歷史轉檔清單, 共計{0} 個表格, 分別為: {1}", tables.Count().ToString(), string.Join(",", tables.Select(x => x.TableName))));





                        #region 計算那些CaseId 要被搬到History_..... 將搬到CaseIds這個變數

                        var CaseMasterSetting = tables.Where(x => x.TableName == "CaseMaster").FirstOrDefault();

                        if (CaseMasterSetting == null)
                        {
                            _HistoryDBBIZ.WriteLog(string.Format("\t歷史設定檔CaseMaster沒有設定"));
                            return;
                        }

                        #region 計算搬動的區間
                        // 20201016, 計算 eDate ....
                        DateTime thenow = DateTime.Now;
                        eDate = thenow;
                        switch (CaseMasterSetting.TimeFreq.ToUpper())
                        {
                            case "D":
                                eDate = thenow.AddDays(-1 * CaseMasterSetting.TimeFreqValue);
                                break;
                            case "M":
                                eDate = thenow.AddMonths(-1 * CaseMasterSetting.TimeFreqValue);
                                break;
                            case "Y":
                                eDate = thenow.AddYears(-1 * CaseMasterSetting.TimeFreqValue);
                                break;
                        }

                        //20201118, 新增計算啟始區間.... 由上次執行的結果
                        //因為上次存在LastBackupDate是包含上次已經搬過的區間, 所以開始日期要加一天
                        //sDate = tables[i].LastBackupDate == new DateTime(0000,1,1) ? new DateTime(2015, 1, 1) : tables[i].LastBackupDate.AddDays(1);
                        if (CaseMasterSetting.LastBackupDate < new DateTime(2015, 1, 1))
                            sDate = new DateTime(2015, 1, 1);
                        else
                            sDate = CaseMasterSetting.LastBackupDate;


                        _HistoryDBBIZ.WriteLog(string.Format("搬動的區間為 {0} ~ {1}", sDate.ToShortDateString(), eDate.ToShortDateString()));

                        #endregion

                        var CaseIds = _HistoryDBBIZ.getCaseIdCouldMove(CaseMasterSetting, sDate, eDate); // 找出那些CaseId 可以在這次中去搬....

                        #endregion


                        for (int i = 0; i < tables.Count(); i++)
                        {
                            string source = tables[i].TableName;
                            string dest = "History_" + source;
                            string dateField = tables[i].DateField.ToString();
                            List<string> ignoreFields = new List<string>() { tables[i].IgnoreField1, tables[i].IgnoreField2, tables[i].IgnoreField3 };

                            if (!tables[i].Enable) // 若是disable , 則不搬...
                                continue;

                            if (dateField.ToUpper().Equals("CLOSEDATE"))
                                ProcessTable_CloseDate(sDate, eDate, source, dest, dateField, ignoreFields, tables[i].isDelete, tables[i].isMove, CaseIds);
                            else
                                if (source.ToUpper().Equals("TX_60491_GRP"))
                                {
                                    ProcessTable_TX60491(sDate, eDate, source, dest, dateField, ignoreFields, tables[i].isDelete, tables[i].isMove, CaseIds);
                                }
                                else
                                    if (source.ToUpper().Equals("CASENOTABLE"))
                                    {
                                        ProcessTable_CaseNoTable(sDate, eDate, source, dest, dateField, ignoreFields, tables[i].isDelete, tables[i].isMove, null); // 全部搬...不刪除
                                    }
                                    else
                                    {
                                        ProcessTable(sDate, eDate, source, dest, dateField, ignoreFields, tables[i].isDelete, tables[i].isMove, CaseIds);
                                    }

                            //20201118, 若執行完成, 要把本次的eDate, 寫回LastBackupDate 這個欄位
                            _HistoryDBBIZ.updateLastBackupDate(tables[i], eDate);
                        }
                    }
                    catch (Exception ex)
                    {
                        _HistoryDBBIZ.WriteLog("程式異常，錯誤信息：" + ex.Message);
                    }
                    #endregion
                }
                else
                {
                    #region 處理其他, 非0或1的

                    var tables = _HistoryDBBIZ.getTablesByGroupNo(g[0].ToString());
                    if (tables.Count == 0)
                    {
                        _HistoryDBBIZ.WriteLog("====> ERROR : 歷史轉檔清單參數未設定");
                        return;
                    }

                    _HistoryDBBIZ.WriteLog(string.Format("\t歷史轉檔清單, 共計{0} 個表格, 分別為: {1}", tables.Count().ToString(), string.Join(",", tables.Select(x => x.TableName))));

                    for (int i = 0; i < tables.Count(); i++)
                    {
                        #region 計算搬動的區間
                        // 20201016, 計算 eDate ....
                        DateTime thenow = DateTime.Now;
                        eDate = thenow;
                        switch (tables[i].TimeFreq.ToUpper())
                        {
                            case "D":
                                eDate = thenow.AddDays(-1 * tables[i].TimeFreqValue);
                                break;
                            case "M":
                                eDate = thenow.AddMonths(-1 * tables[i].TimeFreqValue);
                                break;
                            case "Y":
                                eDate = thenow.AddYears(-1 * tables[i].TimeFreqValue);
                                break;
                        }

                        //20201118, 新增計算啟始區間.... 由上次執行的結果
                        //因為上次存在LastBackupDate是包含上次已經搬過的區間, 所以開始日期要加一天
                        //sDate = tables[i].LastBackupDate == new DateTime(0000,1,1) ? new DateTime(2015, 1, 1) : tables[i].LastBackupDate.AddDays(1);
                        if (tables[i].LastBackupDate < new DateTime(2015, 1, 1))
                            sDate = new DateTime(2015, 1, 1);
                        else
                            sDate = tables[i].LastBackupDate.AddDays(1);


                        _HistoryDBBIZ.WriteLog(string.Format("搬動的區間為 {0} ~ {1}", sDate.ToShortDateString(), eDate.ToShortDateString()));

                        #endregion

                        try
                        {
                            string source = tables[i].TableName;
                            string dest = "History_" + source;
                            string dateField = tables[i].DateField.ToString();
                            List<string> ignoreFields = new List<string>() { tables[i].IgnoreField1, tables[i].IgnoreField2, tables[i].IgnoreField3 };

                            if (dateField.ToUpper().Equals("CLOSEDATE"))
                                ProcessTable_CloseDate(sDate, eDate, source, dest, dateField, ignoreFields, tables[i].isDelete, tables[i].isMove);
                            else
                                if (source.ToUpper().Equals("TX_60491_GRP"))
                                    ProcessTable_TX60491(sDate, eDate, source, dest, dateField, ignoreFields, tables[i].isDelete, tables[i].isMove);
                                else
                                    ProcessTable(sDate, eDate, source, dest, dateField, ignoreFields, tables[i].isDelete, tables[i].isMove);

                            //20201118, 若執行完成, 要把本次的eDate, 寫回LastBackupDate 這個欄位
                            _HistoryDBBIZ.updateLastBackupDate(tables[i], eDate);
                        }
                        catch (Exception ex)
                        {
                            _HistoryDBBIZ.WriteLog("程式異常，錯誤信息：" + ex.Message);
                        }
                    }

                    #endregion
                }
            }

            #endregion

        }


        // CaseNoTable 全部備份, 也不刪除
        private string ProcessTable_CaseNoTable(DateTime sDate, DateTime eDate, string source, string dest, string dateField, List<string> ignoreFields, bool isDelete, bool isMove, List<Guid> CaseIds)
        {
            _HistoryDBBIZ.WriteLog(string.Format("\t\t開始處理表格 {0}..... ", source));

            try
            {

                var result = _HistoryDBBIZ.DeleteData_All(dest);
                if (result == 0)
                {
                    _HistoryDBBIZ.WriteLog(string.Format("\t\t\t刪除歷史表格 {0} ,發生錯誤, 直接中斷本表格搬移程序", dest));
                    return "0001|" + string.Format("\t\t\t刪除歷史表格 {0} ,發生錯誤, 直接中斷本表格搬移程序", dest);
                }
                _HistoryDBBIZ.WriteLog(string.Format("\t\t\t歷史表格 {0} , 已刪除完成", dest));
            }
            catch (Exception ex)
            {
                _HistoryDBBIZ.WriteLog(string.Format("\t\t\t刪除歷史表格 {0} ,發生錯誤, 直接中斷本表格搬移程序", dest));
                _HistoryDBBIZ.WriteLog(string.Format("\t\t\t刪除歷史表格 {0} ,錯誤 {1}", ex.Message.ToString()));
                return "0002|" + string.Format("\t\t\t刪除歷史表格 {0} ,發生錯誤, 直接中斷本表格搬移程序", dest);
            }

            try
            {
                _HistoryDBBIZ.WriteLog(string.Format("\t\t\t表格 {0} , 進行搬移..", source));

                int result2 = 1;

                _HistoryDBBIZ.WriteLog(string.Format("\t\t\t表格 {0} , 搬移開始", source));
                result2 = _HistoryDBBIZ.BulkInsertionAll(source, dest);

                if (result2 == 1)
                {
                    _HistoryDBBIZ.WriteLog(string.Format("\t\t表格 {0} 搬移完成 ", source));
                }
                else
                {
                    _HistoryDBBIZ.WriteLog(string.Format("\t\t\t表格 {0} , 進行搬移中, 發生錯誤, 直接中斷本表格搬移程序", source));
                    return "0004|" + string.Format("\t\t\t表格 {0} , 進行搬移中, 發生錯誤, 直接中斷本表格搬移程序", source);
                }

            }
            catch (Exception ex)
            {
                _HistoryDBBIZ.WriteLog(string.Format("\t\t\t表格 {0} , 進行搬移中, 發生錯誤", source));
                return "0005|" + string.Format("\t\t\t表格 {0} , 進行搬移中, 發生錯誤", source);
            }

            _HistoryDBBIZ.WriteLog("\t\t============================================================");

            return "0000|成功";
        }


        /// <summary>
        /// 舊版, 不支援以CaseMaster的建檔日期為主, 搬每個TABLE, 都是獨立事件
        /// </summary>
        private void Process()
        {
            #region 資料轉檔開始



            // log.config
            log4net.Config.XmlConfigurator.Configure(new System.IO.FileInfo(AppDomain.CurrentDomain.BaseDirectory + @"\Log.config"));
            DateTime sDate = new DateTime(2015, 1, 1);
            DateTime eDate = DateTime.Now;

            bool isDelete = false;

            _HistoryDBBIZ.WriteLog(string.Format("開始歷史轉檔作業 ============================", sDate.ToShortDateString(), eDate.ToShortDateString()));


            var tables = _HistoryDBBIZ.getHistory_BackupSetting();

            if (tables.Count == 0)
            {
                _HistoryDBBIZ.WriteLog("====> ERROR : 歷史轉檔清單參數未設定");
                return;
            }

            tables = tables.OrderBy(x => x.GroupNo).ThenBy(x => x.SortOrder).ToList(); // 排序執行順序

            _HistoryDBBIZ.WriteLog(string.Format("\t歷史轉檔清單, 共計{0} 個表格, 分別為: {1}", tables.Count().ToString(), string.Join(",", tables.Select(x => x.TableName))));

            for (int i = 0; i < tables.Count(); i++)
            {

                #region 計算搬動的區間
                // 20201016, 計算 eDate ....
                DateTime thenow = DateTime.Now;
                eDate = thenow;
                switch (tables[i].TimeFreq.ToUpper())
                {
                    case "D":
                        eDate = thenow.AddDays(-1 * tables[i].TimeFreqValue);
                        break;
                    case "M":
                        eDate = thenow.AddMonths(-1 * tables[i].TimeFreqValue);
                        break;
                    case "Y":
                        eDate = thenow.AddYears(-1 * tables[i].TimeFreqValue);
                        break;
                }

                //20201118, 新增計算啟始區間.... 由上次執行的結果
                //因為上次存在LastBackupDate是包含上次已經搬過的區間, 所以開始日期要加一天
                //sDate = tables[i].LastBackupDate == new DateTime(0000,1,1) ? new DateTime(2015, 1, 1) : tables[i].LastBackupDate.AddDays(1);
                if (tables[i].LastBackupDate < new DateTime(2015, 1, 1))
                    sDate = new DateTime(2015, 1, 1);
                else
                    sDate = tables[i].LastBackupDate.AddDays(1);


                _HistoryDBBIZ.WriteLog(string.Format("搬動的區間為 {0} ~ {1}", sDate.ToShortDateString(), eDate.ToShortDateString()));

                #endregion


                if (tables[i].GroupNo == 0) // 20201203, 加入Group=0邏輯
                {
                    #region 處理GroupNo=0的
                    try
                    {
                        string source = tables[i].TableName;
                        string dest = "History_" + source;
                        string dateField = tables[i].DateField.ToString();
                        List<string> ignoreFields = new List<string>() { tables[i].IgnoreField1, tables[i].IgnoreField2, tables[i].IgnoreField3 };


                        if (tables[i].isMove) // 搬Group0
                        {
                            MoveGroup0(sDate, eDate, source, dest, dateField, ignoreFields);
                        }

                        if (tables[i].isDelete) // 刪Group0
                        {
                            DeleteGroup0(sDate, eDate, source, dateField);
                        }

                        //20201118, 若執行完成, 要把本次的eDate, 寫回LastBackupDate 這個欄位
                        _HistoryDBBIZ.updateLastBackupDate(tables[i], eDate);
                    }
                    catch (Exception ex)
                    {
                        _HistoryDBBIZ.WriteLog("程式異常，錯誤信息：" + ex.Message);
                    }

                    #endregion
                }
                else
                {

                    #region 20201203, 處理原來GroupNo非等於0的, 原邏輯
                    try
                    {

                        string source = tables[i].TableName;
                        string dest = "History_" + source;
                        string dateField = tables[i].DateField.ToString();
                        List<string> ignoreFields = new List<string>() { tables[i].IgnoreField1, tables[i].IgnoreField2, tables[i].IgnoreField3 };

                        if (dateField.ToUpper().Equals("CLOSEDATE"))
                            ProcessTable_CloseDate(sDate, eDate, source, dest, dateField, ignoreFields, tables[i].isDelete, tables[i].isMove);
                        else
                            if (source.ToUpper().Equals("TX_60491_GRP"))
                                ProcessTable_TX60491(sDate, eDate, source, dest, dateField, ignoreFields, tables[i].isDelete, tables[i].isMove);
                            else
                                ProcessTable(sDate, eDate, source, dest, dateField, ignoreFields, tables[i].isDelete, tables[i].isMove);

                        //20201118, 若執行完成, 要把本次的eDate, 寫回LastBackupDate 這個欄位
                        _HistoryDBBIZ.updateLastBackupDate(tables[i], eDate);
                    }
                    catch (Exception ex)
                    {
                        _HistoryDBBIZ.WriteLog("程式異常，錯誤信息：" + ex.Message);
                    }
                    #endregion
                }
            }


            #endregion
        }


        /// <summary>
        /// 搬移Group0
        /// </summary>
        /// <param name="sDate"></param>
        /// <param name="eDate"></param>
        /// <param name="source"></param>
        /// <param name="dest"></param>
        /// <param name="dateField"></param>
        /// <param name="ignoreFields"></param>
        private string MoveGroup0(DateTime sDate, DateTime eDate, string source, string dest, string dateField, List<string> ignoreFields)
        {
            var count = _HistoryDBBIZ.CheckDataExit(sDate, eDate, dest, dateField, new List<Guid>());
            if (count > 0) // 若有筆數, 代表此區間, 之前有跑過.. 砍掉歷史區, 再重匯
            {
                try
                {
                    _HistoryDBBIZ.WriteLog(string.Format("\t\t\t歷史表格 {0} , 已有重覆{1}筆, 進行刪除..", dest, count));
                    var result = _HistoryDBBIZ.DeleteData(sDate, eDate, dest, dateField, new List<Guid>());
                    if (result == 0)
                    {
                        _HistoryDBBIZ.WriteLog(string.Format("\t\t\t刪除歷史表格 {0} ,發生錯誤, 直接中斷本表格搬移程序", dest));
                        return "0001|" + string.Format("\t\t\t刪除歷史表格 {0} ,發生錯誤, 直接中斷本表格搬移程序", dest);
                    }
                    _HistoryDBBIZ.WriteLog(string.Format("\t\t\t歷史表格 {0} , 已刪除完成", dest));
                }
                catch (Exception ex)
                {
                    _HistoryDBBIZ.WriteLog(string.Format("\t\t\t刪除歷史表格 {0} ,發生錯誤, 直接中斷本表格搬移程序", dest));
                    _HistoryDBBIZ.WriteLog(string.Format("\t\t\t刪除歷史表格 {0} ,錯誤 {1}", ex.Message.ToString()));
                    return "0002|" + string.Format("\t\t\t刪除歷史表格 {0} ,發生錯誤, 直接中斷本表格搬移程序", dest);
                }
            }
            try
            {
                _HistoryDBBIZ.WriteLog(string.Format("\t\t\t表格 {0} , 進行搬移..", source));

                int result2 = 1;
                result2 = _HistoryDBBIZ.BulkInsertion(sDate, eDate, source, dest, dateField, ignoreFields, null);

                if (result2 == 1)
                {
                    _HistoryDBBIZ.WriteLog(string.Format("\t\t表格 {0} 搬移完成 ", source));
                }
                else
                {
                    _HistoryDBBIZ.WriteLog(string.Format("\t\t\t表格 {0} , 進行搬移中, 發生錯誤, 直接中斷本表格搬移程序", source));
                    return "0004|" + string.Format("\t\t\t表格 {0} , 進行搬移中, 發生錯誤, 直接中斷本表格搬移程序", source);
                }

            }
            catch (Exception ex)
            {
                _HistoryDBBIZ.WriteLog(string.Format("\t\t\t表格 {0} , 進行搬移中, 發生錯誤", source));
                return "0005|" + string.Format("\t\t\t表格 {0} , 進行搬移中, 發生錯誤", source);
            }

            _HistoryDBBIZ.WriteLog("\t\t============================================================");

            return "0000|搬移成功";
        }


        /// <summary>
        /// 刪除Group0 
        /// </summary>
        /// <param name="sDate"></param>
        /// <param name="eDate"></param>
        /// <param name="source"></param>
        /// <param name="dateField"></param>
        private string DeleteGroup0(DateTime sDate, DateTime eDate, string source, string dateField)
        {

            var count = _HistoryDBBIZ.CheckDataExit(sDate, eDate, source, dateField, new List<Guid>());

            _HistoryDBBIZ.WriteLog(string.Format("\t\t\t表格 {0} {1} ~ {2} 共 {3} 筆, 進行刪除..", source, sDate.ToShortDateString(), eDate.ToShortDateString(), count.ToString()));
            var result = _HistoryDBBIZ.DeleteData(sDate, eDate, source, dateField, new List<Guid>());
            if (result == 0)
            {
                _HistoryDBBIZ.WriteLog(string.Format("\t\t\t刪除表格 {0} ,發生錯誤", source));
                return "0001|" + string.Format("\t\t\t刪除表格 {0} ,發生錯誤, 直接中斷本表格搬移程序", source);
            }
            _HistoryDBBIZ.WriteLog(string.Format("\t\t\t表格 {0} , 已刪除完成", source));
            return "0000|" + string.Format("\t\t\t刪除表格 {0} 成功", source);
        }

        private string ProcessTable(DateTime sDate, DateTime eDate, string source, string dest, string dateField, List<string> ignoreFields, bool isDelete, bool isMove, List<Guid> CaseIds = null)
        {

            _HistoryDBBIZ.WriteLog(string.Format("\t\t開始處理表格 {0}..... ", source));

            var count = _HistoryDBBIZ.CheckDataExit(sDate, eDate, dest, dateField, CaseIds);

            {

                if (count > 0) // 若有筆數, 代表此區間, 之前有跑過.. 砍掉再重匯
                {
                    try
                    {
                        _HistoryDBBIZ.WriteLog(string.Format("\t\t\t歷史表格 {0} , 已有重覆{1}筆, 進行刪除..", dest, count));
                        var result = _HistoryDBBIZ.DeleteData(sDate, eDate, dest, dateField, CaseIds);
                        if (result == 0)
                        {
                            _HistoryDBBIZ.WriteLog(string.Format("\t\t\t刪除歷史表格 {0} ,發生錯誤, 直接中斷本表格搬移程序", dest));
                            return "0001|" + string.Format("\t\t\t刪除歷史表格 {0} ,發生錯誤, 直接中斷本表格搬移程序", dest);
                        }
                        _HistoryDBBIZ.WriteLog(string.Format("\t\t\t歷史表格 {0} , 已刪除完成", dest));
                    }
                    catch (Exception ex)
                    {
                        _HistoryDBBIZ.WriteLog(string.Format("\t\t\t刪除歷史表格 {0} ,發生錯誤, 直接中斷本表格搬移程序", dest));
                        _HistoryDBBIZ.WriteLog(string.Format("\t\t\t刪除歷史表格 {0} ,錯誤 {1}", ex.Message.ToString()));
                        return "0002|" + string.Format("\t\t\t刪除歷史表格 {0} ,發生錯誤, 直接中斷本表格搬移程序", dest);
                    }
                }
            }

            try
            {
                _HistoryDBBIZ.WriteLog(string.Format("\t\t\t表格 {0} , 進行搬移..", source));

                int result2 = 1;

                if (isMove)
                {
                    _HistoryDBBIZ.WriteLog(string.Format("\t\t\t表格 {0} , 搬移開始", source));
                    result2 = _HistoryDBBIZ.BulkInsertion(sDate, eDate, source, dest, dateField, ignoreFields, CaseIds);
                    _HistoryDBBIZ.WriteLog(string.Format("\t\t\t表格 {0} , 搬移完成", source));
                }


                if (result2 == 1)
                {
                    if (isDelete)
                    {
                        // 開始刪除Source
                        _HistoryDBBIZ.WriteLog(string.Format("\t\t\t表格 {0} , 進行刪除...", source));
                        var result3 = _HistoryDBBIZ.DeleteData(sDate, eDate, source, dateField, CaseIds);
                        if (result3 == 0)
                            return "0003|" + string.Format("\t\t\t表格 {0} , 刪除錯誤", source);
                        _HistoryDBBIZ.WriteLog(string.Format("\t\t\t表格 {0} , 刪除完成", source));
                    }
                    else
                    {
                        _HistoryDBBIZ.WriteLog(string.Format("\t\t\t表格 {0} , 不進行刪除...", source));
                    }
                    _HistoryDBBIZ.WriteLog(string.Format("\t\t表格 {0} 搬移完成 ", source));
                }
                else
                {
                    _HistoryDBBIZ.WriteLog(string.Format("\t\t\t表格 {0} , 進行搬移中, 發生錯誤, 直接中斷本表格搬移程序", source));
                    return "0004|" + string.Format("\t\t\t表格 {0} , 進行搬移中, 發生錯誤, 直接中斷本表格搬移程序", source);
                }

            }
            catch (Exception ex)
            {
                _HistoryDBBIZ.WriteLog(string.Format("\t\t\t表格 {0} , 進行搬移中, 發生錯誤", source));
                return "0005|" + string.Format("\t\t\t表格 {0} , 進行搬移中, 發生錯誤", source);
            }

            _HistoryDBBIZ.WriteLog("\t\t============================================================");

            return "0000|成功";
        }


        /// <summary>
        /// 原TABLE 沒有CreateTime , 只好去Join CaseMaster來決定那些要搬家....
        /// </summary>
        /// <param name="sDate"></param>
        /// <param name="eDate"></param>
        /// <param name="source"></param>
        /// <param name="dest"></param>
        /// <param name="dateField"></param>
        /// <param name="ignoreFields"></param>
        /// <returns></returns>
        private string ProcessTable_CloseDate(DateTime sDate, DateTime eDate, string source, string dest, string dateField, List<string> ignoreFields, bool isDelete, bool isMove, List<Guid> CaseIds = null)
        {

            _HistoryDBBIZ.WriteLog(string.Format("\t\t開始處理表格 {0}..... ", source));



            {
                var count = _HistoryDBBIZ.CheckDataExit_CloseDate(sDate, eDate, dest, dateField, CaseIds);
                if (count > 0) // 若有筆數, 代表此區間, 之前有跑過.. 砍掉再重匯
                {
                    try
                    {
                        _HistoryDBBIZ.WriteLog(string.Format("\t\t\t歷史表格 {0} , 已有重覆{1}筆, 進行刪除..", dest, count));
                        var result = _HistoryDBBIZ.DeleteData_CloseDate(sDate, eDate, dest, dateField, CaseIds);
                        if (result == 0)
                        {
                            _HistoryDBBIZ.WriteLog(string.Format("\t\t\t刪除歷史表格 {0} ,發生錯誤, 直接中斷本表格搬移程序", dest));
                            return "0001|" + string.Format("\t\t\t刪除歷史表格 {0} ,發生錯誤, 直接中斷本表格搬移程序", dest);
                        }
                        _HistoryDBBIZ.WriteLog(string.Format("\t\t\t歷史表格 {0} , 已刪除完成", dest));
                    }
                    catch (Exception ex)
                    {
                        _HistoryDBBIZ.WriteLog(string.Format("\t\t\t刪除歷史表格 {0} ,發生錯誤, 直接中斷本表格搬移程序", dest));
                        _HistoryDBBIZ.WriteLog(string.Format("\t\t\t刪除歷史表格 {0} ,錯誤 {1}", ex.Message.ToString()));
                        return "0002|" + string.Format("\t\t\t刪除歷史表格 {0} ,發生錯誤, 直接中斷本表格搬移程序", dest);
                    }
                }
            }

            try
            {
                _HistoryDBBIZ.WriteLog(string.Format("\t\t\t表格 {0} , 進行搬移..", source));

                int result2 = 1;
                if (isMove)
                {
                    _HistoryDBBIZ.WriteLog(string.Format("\t\t\t表格 {0} , 搬移開始", source));
                    result2 = _HistoryDBBIZ.BulkInsertion_CloseDate(sDate, eDate, source, dest, dateField, ignoreFields, CaseIds);
                    _HistoryDBBIZ.WriteLog(string.Format("\t\t\t表格 {0} , 搬移完成", source));
                }

                if (result2 == 1)
                {
                    // 開始刪除Source
                    if (isDelete)
                    {
                        _HistoryDBBIZ.WriteLog(string.Format("\t\t\t表格 {0} , 進行刪除...", source));
                        var result3 = _HistoryDBBIZ.DeleteData_CloseDate(sDate, eDate, source, dateField, CaseIds);
                        if (result3 == 0)
                            return "0003|" + string.Format("\t\t\t表格 {0} , 刪除錯誤", source);
                        _HistoryDBBIZ.WriteLog(string.Format("\t\t\t表格 {0} , 刪除完成", source));
                    }
                    else
                    {
                        _HistoryDBBIZ.WriteLog(string.Format("\t\t\t表格 {0} , 不進行刪除...", source));
                    }
                    _HistoryDBBIZ.WriteLog(string.Format("\t\t表格 {0} 搬移完成 ", source));
                }
                else
                {
                    _HistoryDBBIZ.WriteLog(string.Format("\t\t\t表格 {0} , 進行搬移中, 發生錯誤, 直接中斷本表格搬移程序", source));
                    return "0004|" + string.Format("\t\t\t表格 {0} , 進行搬移中, 發生錯誤, 直接中斷本表格搬移程序", source);
                }

            }
            catch (Exception ex)
            {
                _HistoryDBBIZ.WriteLog(string.Format("\t\t\t表格 {0} , 進行搬移中, 發生錯誤", source));
                return "0005|" + string.Format("\t\t\t表格 {0} , 進行搬移中, 發生錯誤", source);
            }

            _HistoryDBBIZ.WriteLog("\t\t============================================================");

            return "0000|成功";
        }


        /// <summary>
        /// TX_60491的Master Detail結構, 但Detail沒有CreatedDate , 所以必須依靠Master.[cCretDT] 時間, 才知搬的範圍...
        /// 要一起搬才行...
        /// </summary>
        /// <param name="sDate"></param>
        /// <param name="eDate"></param>
        /// <param name="source"></param>
        /// <param name="dest"></param>
        /// <param name="dateField"></param>
        /// <param name="ignoreFields"></param>
        /// <returns></returns>
        private string ProcessTable_TX60491(DateTime sDate, DateTime eDate, string source, string dest, string dateField, List<string> ignoreFields, bool isDelete, bool isMove, List<Guid> CaseIds = null)
        {

            _HistoryDBBIZ.WriteLog(string.Format("\t\t開始處理表格 {0}..... ", source));

            var count = _HistoryDBBIZ.CheckDataExit(sDate, eDate, dest, dateField, CaseIds);
            if (count == 0)
            {
                _HistoryDBBIZ.WriteLog(string.Format("\t\t\t\t共計TX60491_Grp 無資料!!"));
                //return "0000|成功";
            }


            {
                if (count > 0) // 若有筆數, 代表此區間, 之前有跑過.. 砍掉再重匯
                {
                    try
                    {
                        _HistoryDBBIZ.WriteLog(string.Format("\t\t\t歷史表格 {0} , 已有重覆{1}筆, 進行刪除..", dest, count));
                        var result = _HistoryDBBIZ.DeleteData_TX60491(sDate, eDate, dest, dateField, CaseIds);
                        if (result == 0)
                        {
                            _HistoryDBBIZ.WriteLog(string.Format("\t\t\t刪除歷史表格 {0} ,發生錯誤, 直接中斷本表格搬移程序", dest));
                            return "0001|" + string.Format("\t\t\t刪除歷史表格 {0} ,發生錯誤, 直接中斷本表格搬移程序", dest);
                        }
                        _HistoryDBBIZ.WriteLog(string.Format("\t\t\t歷史表格 {0} , 已刪除完成", dest));
                    }
                    catch (Exception ex)
                    {
                        _HistoryDBBIZ.WriteLog(string.Format("\t\t\t刪除歷史表格 {0} ,發生錯誤, 直接中斷本表格搬移程序", dest));
                        _HistoryDBBIZ.WriteLog(string.Format("\t\t\t刪除歷史表格 {0} ,錯誤 {1}", ex.Message.ToString()));
                        return "0002|" + string.Format("\t\t\t刪除歷史表格 {0} ,發生錯誤, 直接中斷本表格搬移程序", dest);
                    }
                }
            }

            try
            {
                _HistoryDBBIZ.WriteLog(string.Format("\t\t\t表格 {0} , 進行搬移..", source));

                int result2 = 1;

                if (isMove)
                {
                    _HistoryDBBIZ.WriteLog(string.Format("\t\t\t表格 {0} , 搬移開始", source));
                    result2 = _HistoryDBBIZ.BulkInsertion_TX60491_New(sDate, eDate, source, dest, dateField, ignoreFields, CaseIds);
                    _HistoryDBBIZ.WriteLog(string.Format("\t\t\t表格 {0} , 搬移完成", source));
                }

                if (result2 == 1)
                {
                    if (isDelete)
                    {
                        // 開始刪除Source
                        _HistoryDBBIZ.WriteLog(string.Format("\t\t\t表格 {0} , 進行刪除...", source));
                        var result3 = _HistoryDBBIZ.DeleteData_TX60491(sDate, eDate, source, dateField, CaseIds);
                        if (result3 == 0)
                            return "0003|" + string.Format("\t\t\t表格 {0} , 刪除錯誤", source);
                        _HistoryDBBIZ.WriteLog(string.Format("\t\t\t表格 {0} , 刪除完成", source));
                    }
                    else
                    {
                        _HistoryDBBIZ.WriteLog(string.Format("\t\t\t表格 {0} , 不進行刪除...", source));
                    }
                    _HistoryDBBIZ.WriteLog(string.Format("\t\t表格 {0} 搬移完成 ", source));
                }
                else
                {
                    _HistoryDBBIZ.WriteLog(string.Format("\t\t\t表格 {0} , 進行搬移中, 發生錯誤, 直接中斷本表格搬移程序", source));
                    return "0004|" + string.Format("\t\t\t表格 {0} , 進行搬移中, 發生錯誤, 直接中斷本表格搬移程序", source);
                }

            }
            catch (Exception ex)
            {
                _HistoryDBBIZ.WriteLog(string.Format("\t\t\t表格 {0} , 進行搬移中, 發生錯誤", source));
                return "0005|" + string.Format("\t\t\t表格 {0} , 進行搬移中, 發生錯誤", source);
            }

            _HistoryDBBIZ.WriteLog("\t\t============================================================");

            return "0000|成功";
        }


    }

    public class TableParam
    {
        public string TableName { get; set; }
        public string DateField { get; set; }
        public List<string> IngoreField { get; set; }
    }
}
