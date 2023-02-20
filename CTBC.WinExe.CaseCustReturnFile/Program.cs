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
using ICSharpCode.SharpZipLib.Zip;

namespace CTBC.WinExe.CaseCustReturnFile
{
    class Program : BaseBusinessRule
    {
        #region 全局變量

        // HTG/RFDM文件路徑
        private static string txtFilePath = ConfigurationManager.AppSettings["txtFilePath"];

        private static string reciveloaclFilePath = ConfigurationManager.AppSettings["reciveloaclFilePath"];

        // 獲取log路徑
        private static FileLog m_fileLog = new FileLog(ConfigurationManager.AppSettings["fileLog"], AppDomain.CurrentDomain.FriendlyName.Replace(".vshost", "").Replace(".exe", ""));

        // 半形空白變量
        public string strNull = " ";

        public DataTable CurrencyList = null;
        #endregion

        static void Main(string[] args)
        {
            Program mainProgram = new Program();

            DateTime thenow = DateTime.Now;
            m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, DateTime.Now.ToString());
            try
            {
                //20201120, 要避開營業日
                bool? isWorkDay = mainProgram.getWorkDay(thenow); // 應該要讀取前一天的工作日才對
                if (isWorkDay == null)
                {
                    m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "工作日設定為空 !!");
                    return;
                }
                else
                {
                    if (!(bool)isWorkDay) //假日
                    {
                        m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "非工作日 !!");
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "工作日參數設定為空:" + ex.Message.ToString());
                //throw ex;
            }


            //20220928, 將拋查超過2個工作日的案件, 落失敗....(要讀取.. PARMCODE.CodeType='CaseCustFailDays', 決定天數, 預設2天)
            mainProgram.setRuningOver2Day();

            m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息-----------");
            //取得目前尚未產檔的MasterID, 並且檢查, 那些MasterID , 已經都回傳了(要考量QueryType, 有隔天回傳的.... 如81019, 保管箱)
            var allMaster = mainProgram.getMasterId2();
            m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "共計找到:" + allMaster.Count().ToString() + "需要產檔");

            foreach (var m in allMaster.OrderBy(x=>x.DocNo))
            {
                try
                {
                    m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "----------------------------------開始產檔: " + m.DocNo);
                    // 產生回文TXT
                    mainProgram.Process(m);

                    // 產生回文PDF
                    mainProgram.OutPutPDF(m);
                }
                catch (Exception)
                {
                    m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "----------------------------------發生錯誤: " + m.DocNo);
                }
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "----------------------------------結束產檔: " + m.DocNo);
            }

        }
        /// <summary>
        /// T+2日, 還在拋查中, 則押失敗....
        /// </summary>
        private void setRuningOver2Day()
        {
            // 先讀參數檔, 若讀不到, 預設2個工作天...
            int beforeDay = 2;
            var sqlParm = @"select CodeNo from PARMCode where  CodeType='CaseCustFailDays' AND Enable='1'; ";
            var rParm = base.Search(sqlParm);
            if( rParm!=null) {
                if( rParm.Rows.Count > 0 ) {
                    beforeDay = int.Parse(rParm.Rows[0][0].ToString());
                }
            }



            // 讀參數檔.. 決定N天前的案件, 要落失敗...
            DateTime beDay = DateTime.Now.AddDays(-5); // 預設前5天日曆天....
            string theday = DateTime.Now.ToString("yyyy-MM-dd");            
            string dSQL = string.Format("SELECT TOP 10 * FROM [dbo].[PARMWorkingDay] WHERE Date<='{0}' AND Flag=1 order by Date desc", theday);
            var dParm = base.Search(dSQL);
            if (dParm != null)
            {
                if (dParm.Rows.Count >= beforeDay)
                {
                    
                    beDay = DateTime.Parse(dParm.Rows[beforeDay]["Date"].ToString());
                }
            }


            // 將原始來文檔, 也給QfileName3....

            var aSql =string.Format( @"select * from CaseCustMaster where status='02' and pdfStatus='N' and Convert(nvarchar(20),CreatedDate,23) <='{0}'", beDay.ToString("yyyy-MM-dd"));
            var MasterCaseToFail = base.SearchList<CaseCustMaster>(aSql);

            if (MasterCaseToFail.Count() > 0)
            {
                foreach (var master in MasterCaseToFail)
                {
                    #region 若有格式錯誤, 則打包原來案件為一毎ZIP檔
                    formatErrorZip(master, "T+2日未回應");
                    #endregion
                }

            }





            // 找出beDay前的案件, 押成失敗....

            var sSql = string.Format(@"update CaseCustDetails Set Status='86',ErrorMessage='T+2日未回應'  where CaseCustMasterID in (
	select NewID from CaseCustMaster where status='02' and pdfStatus='N' and Convert(nvarchar(20),CreatedDate,23) <='{0}');
    update CaseCustMaster set Status='86' where status='02' and pdfStatus='N' and Convert(nvarchar(20),CreatedDate,23) <='{0}'"
                , beDay.ToString("yyyy-MM-dd"));

            base.ExecuteNonQuery(sSql);








            
        }

        public string formatErrorZip(CaseCustMaster master, string message)
        {
            // filePath + @"\" + fileGroups.TXT

            // System.IO.Compression.ZipFile.ExtractToDirectory(fi.FullName, reciveloaclFilePath + "\\");

            // 由Qfilename 中, 找出要壓縮的路徑....
            string[] filp = master.QFileName.Split('\\');

            string zipFileName = reciveloaclFilePath + "\\" + filp[0] + "\\" + filp[1]  + "_Error.zip";
            string zipFolder = reciveloaclFilePath + "\\" + filp[0] + "\\" + filp[1] ;

            try
            {
                System.IO.Compression.ZipFile.CreateFromDirectory(zipFolder, zipFileName);

            }
            catch (Exception ex)
            {
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, " 壓縮錯誤檔ZIP發生問題,  錯誤訊息: " + ex.ToString());
            }

            // 更新QFileName3 ...
            string sql = string.Format(@"Update CaseCustMaster Set QFileName3='{0}' WHERE NewID='{1}';", zipFileName.Replace(reciveloaclFilePath + "\\",""), master.NewID);
            base.ExecuteNonQuery(sql);

            return "";
        }






        internal bool? getWorkDay(DateTime BizDate)
        {
            string SQL = "SELECT TOP 1 * FROM [dbo].[PARMWorkingDay] where date ='{0}' ";
            SQL = string.Format(SQL, BizDate.ToString("yyyy-MM-dd"));
            var d = base.SearchList<PARMWorkingDay>(SQL).FirstOrDefault();
            if (d != null)
            {
                return d.Flag;
            }
            else
            {
                return null;
            }
        }

        private List<CaseCustMaster> getMasterId()
        {
            List<CaseCustMaster> output = new List<CaseCustMaster>();
            var sql = @"select * from CaseCustMaster where status='02' and pdfStatus='N' ";
            var ret = SearchList<CaseCustMaster>(sql).ToList();

            foreach(var r in ret)
            {
                var sql1 = @"select distinct QueryType from CaseCustDetails where CaseCustMasterId='" + r.NewID +"'";
                var qt = Search(sql1); // 找出有幾種QueryType, 是否都回傳了...
                bool allPass = true;
                foreach(DataRow dr in qt.Rows)
                {
                    var iqt = int.Parse(dr[0].ToString());
                    switch (iqt)
                    {
                        case 1:
                        case 3:
                            // Status=55 代表是未打HTG的外國人.... HTGSendStatus=4 , 49, 代表打成功/失敗.
                            string sql2 = @"select * from CaseCustDetails WHERE OpenFlag='Y' and (HTGSendStatus='4' or HTGSendStatus='49' or Status='55' ) and QueryType in ('1','3') and CaseCustMasterId='" + r.NewID + "'";
                            var ret2 = SearchList<CaseCustDetails>(sql2);
                            if( ret2.Count()==0)                            
                                allPass = false;
                            break;
                        case 2:
                        case 4:
                            // Status=55 代表是未打HTG的外國人....HTGSendStatus=4 , 49, 代表打成功/失敗.RFDMSendStatus=8, 4, 99  代表有打過RFDM ( 即.. CaseCustESB 中打ESB 回傳的...)
                            // 20220804, 要卡BOPS081019Send.ATMFlag='Y'         
                            // 20220905, 因為有可能打67050 及60628 時, 就"無相關資料", 造成沒有打BOPS081019Send , 所以會變成一直在拋查中....
                            // 20220905, 因為 用Details 去Join BOPS81019Send 以判斷是否沒有Insert BOPS81019, 代表無相關資料.... 今天以前有打過電文...
                            string thenow = DateTime.Now.ToString("yyyy-MM-dd");
                            string sql5 = @"select * from CaseCustDetails d inner join BOPS081019Send b on d.DetailsId=b.VersionNewID WHERE d.ModifiedDate < '" + thenow + "' and CaseCustMasterId='" + r.NewID + "'";
                            var ret5 = SearchList<CaseCustDetails>(sql5);
                            if (ret5.Count() > 0) // 代表有任何一個帳戶, 打到了.. 81019.. 所以, 就要等ATMFlag下來...
                            {
                                string sql3 = @"select * from CaseCustDetails d Inner join BOPS081019Send b on d.DetailsId=b.VersionNewID  WHERE OpenFlag='Y' AND TransactionFlag='Y' and (HTGSendStatus='4' or HTGSendStatus='49' or Status='55' ) and (RFDMSendStatus='8' or RFDMSendStatus='4'  or RFDMSendStatus='99') and QueryType in ('2','4')  and b.ATMFlag='Y' and CaseCustMasterId='" + r.NewID + "'";
                                var ret3 = SearchList<CaseCustDetails>(sql3);
                                if (ret3.Count() == 0)
                                    allPass = false;
                            }
                            else // 代表, 沒有帳戶資料, 不用等ATMFlag , 直接輸出....
                            {
                                
                            }



                            break;
                        case 5:
                            string sql4 = @"select * from CaseCustDetails WHERE SBoxSendStatus='02' and  QueryType in ('5') and CaseCustMasterId='" + r.NewID + "'";
                            var ret4 = SearchList<CaseCustDetails>(sql4);
                            if( ret4.Count()==0)                            
                                allPass = false;
                            break;
                    }
                }                
                if( allPass)
                {
                    output.Add(r);
                }
            }
            return output;
        }


        private List<CaseCustMaster> getMasterId2()
        {
            // 接下來, 代表有案件要產... 要小於今日的工作日...  convert(varchar, getdate(), 23)
            var theday = DateTime.Now.ToString("yyyy-MM-dd");

            List<CaseCustMaster> output = new List<CaseCustMaster>();
            var sql = @"select * from CaseCustMaster where status in('02','06') and pdfStatus='N' and ModifiedDate<'" + theday + "'";
            //var sql = @"select * from CaseCustMaster where status='02' and pdfStatus='N' and convert(varchar, ModifiedDate, 23)='" + theday + "'";
            var ret = SearchList<CaseCustMaster>(sql).OrderBy(x => x.DocNo).ToList();

            m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "目前有 " + ret.Count.ToString() + "案件未產檔");


            foreach (var r in ret)
            {
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "\t開始過濾是否可以產檔 " + r.DocNo);
                var sql1 = @"select distinct QueryType from CaseCustDetails where CaseCustMasterId='" + r.NewID + "'";
                var qt = Search(sql1); // 找出有幾種QueryType, 是否都回傳了...
                bool allPass = true;
                foreach (DataRow dr in qt.Rows)
                {
                    var iqt = int.Parse(dr[0].ToString());
                    switch (iqt)
                    {
                        case 1:
                        case 3:
                            // 判斷是否有CaseCustOutputF1 , 
                            string sql2 = @"select Count(*) from CaseCustOutputF1 where MasterId='" + r.NewID + "'";
                            var ret2 = Search(sql2);
                            if (Convert.ToInt32(ret2.Rows[0][0]) == 0)
                            {
                                var sql333 = @"select count(*) from CaseCustDetails where CaseCustMasterId='" + r.NewID + "' AND EXCEL_FILE='FTP'";
                                var ret333 = Search(sql333);
                                if (Convert.ToInt32(ret333.Rows[0][0]) == 0)
                                {
                                    allPass = false;
                                    m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "\t\t" + r.DocNo + "QueryType 1, 3 尚未完成");
                                }
                            }
                            break;
                        case 2:
                        case 4:
                            // 判斷是否有CaseCustOutputF2 , 
                            string sql3 = @"select Count(*) from CaseCustOutputF2 where MasterId='" + r.NewID + "' ";
                            var ret3 = Search(sql3);
                            if (Convert.ToInt32(ret3.Rows[0][0]) == 0)
                            {
                                var sql333 = @"select count(*) from CaseCustDetails where CaseCustMasterId='" + r.NewID + "' AND EXCEL_FILE='FTP'";
                                var ret333 = Search(sql333);
                                if (Convert.ToInt32(ret333.Rows[0][0]) == 0)
                                {
                                    allPass = false;
                                    m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "\t\t" + r.DocNo + "QueryType 2, 4 尚未完成");
                                }

                            }
                            break;
                        case 5:
                            string sql4 = @"select * from CaseCustDetails WHERE SBoxSendStatus='02' and  QueryType in ('5') and CaseCustMasterId='" + r.NewID + "'";
                            var ret4 = SearchList<CaseCustDetails>(sql4);
                            if (ret4.Count() == 0)
                            {
                                allPass = false;
                                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "\t\t" + r.DocNo + "QueryType 5 尚未完成");
                            }

                            break;
                    }
                }
                if (allPass)
                {
                    m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "\t\t案件" + r.DocNo + "可以產檔!!!!");
                    output.Add(r);
                }
            }
            return output;
        }
        /// <summary>
        /// 思路改成, 判斷是否有任何一筆81019回來, 若有.. 則表示昨天的電文,ESB, 都打完了....
        /// </summary>
        /// <returns></returns>
        private List<CaseCustMaster> getMasterId_New()
        {
            if (check81019() == 0) // 表示目前沒有一個81019有回來, 結束
            {
                return new List<CaseCustMaster>();
            }


            // 接下來, 代表有案件要產... 要小於今日的工作日...
            var theday = DateTime.Now.ToString("yyyy-MM-dd");

            List<CaseCustMaster> output = new List<CaseCustMaster>();
            var sql = @"select * from CaseCustMaster where status='02' and pdfStatus='N' and ModifiedDate<'" + theday + "'";
            output = SearchList<CaseCustMaster>(sql).ToList();

            //foreach (var r in ret)
            //{
            //    var sql1 = @"select distinct QueryType from CaseCustDetails where CaseCustMasterId='" + r.NewID + "'";
            //    var qt = Search(sql1); // 找出有幾種QueryType, 是否都回傳了...
            //    bool allPass = true;
            //    foreach (DataRow dr in qt.Rows)
            //    {
            //        var iqt = int.Parse(dr[0].ToString());
            //        switch (iqt)
            //        {
            //            case 1:
            //            case 3:
            //                // 判斷是否有CaseCustOutputF1 , 若無, 則Insert 1筆outputF1說無資料....
            //                string sql2 = @"select Count(*) from CaseCustOutputF1 where MasterId='" + r.NewID + "'";
            //                var ret2 = Search(sql2);
            //                if (Convert.ToInt32(ret2.Rows[0][0])==0)
            //                {
            //                    insertOutputF1_NoData(r.NewID);  
            //                }                                
            //                break;
            //            case 2:
            //            case 4:
            //                // 判斷是否有CaseCustOutputF2 , 若無, 則Insert 1筆outputF2說無資料....
            //                string sql3 = @"select Count(*) from CaseCustOutputF2 where MasterId='" + r.NewID + "'";
            //                var ret3 = Search(sql3);
            //                if (Convert.ToInt32(ret3.Rows[0][0]) == 0)
            //                {
            //                    insertOutputF2_NoData(r.NewID);
            //                }
            //                break;
            //            case 5:
            //                // 判斷是否有CaseCustOutputF3 , 若無, 則Insert 1筆outputF3說無資料....
            //                string sql4 = @"select Count(*) from CaseCustOutputF3 where MasterId='" + r.NewID + "'";
            //                var ret4 = Search(sql2);
            //                if (Convert.ToInt32(ret4.Rows[0][0]) == 0)
            //                {
            //                    insertOutputF3_NoData(r.NewID);
            //                }
            //                break;
            //        }
            //    }
            //    if (allPass)
            //    {
            //        output.Add(r);
            //    }
            //}
            return output;
        }


        public int check81019()
        {

            string sql = @"select count(*) from CaseCustMaster m inner join CaseCustDetails d on m.NewID = d.CaseCustMasterId
																   left join BOPS081019Send  on d.DetailsId = BOPS081019Send.VersionNewID
where m.status='02' and m.pdfStatus='N' AND d.RFDMSendStatus = '8' and d.Status = '02' AND BOPS081019Send.ATMFlag = 'Y'";
            base.Parameter.Clear();
            //int val = base.ExecuteNonQuery(sql);\
            var ret = base.Search(sql);
            return Convert.ToInt32(ret.Rows[0][0]);
        }

        /// <summary>
        /// 主方法
        /// </summary>
        private void Process(CaseCustMaster mas)
        {




            // 獲取幣別代碼檔資料
            CurrencyList = GetParmCodeCurrency();

            // 程序開始記入log
            m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------回文開始------");

            // 判斷文件夾是否存在，若不存在，則創建
            if (!Directory.Exists(txtFilePath)) Directory.CreateDirectory(txtFilePath);



            List<string> FilenameList = new List<string>();
            //取得要回文的案號

            List<CaseCustDetails> retFileBase = getReturnFileBase(mas);


            //20220906, 若發生原來的檔案存在, 則先刪除, 以免重覆RUN時, 會重覆Append
            string fn = txtFilePath + "\\" + mas.DocNo + "_" + mas.Version.ToString() + "_Base.csv";
            if (File.Exists(fn))
            {
                File.Delete(fn);
            }


            int rowId = 1;
            foreach (var d in retFileBase)
            {
                CaseCustMaster m = getCaseCustMaster(d.CaseCustMasterId);
                FilenameList.Add(writeBasicInfo(d, m, rowId));
                rowId++;
            }
            if (retFileBase.Count() == 0) // 表示本案件, 沒有產生基資的檔案, 要把  CaseCustMaster.ROpenFileName 押成空白
            {
                string sql = @"UPDATE CaseCustMaster SET ROpenFileName='' where NewID = @masid";
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@masid", mas.NewID));
                base.ExecuteNonQuery(sql);
            }


            List<CaseCustDetails> retFileTrans = getReturnFileTranscation(mas);
            //20220906, 若發生原來的檔案存在, 則先刪除, 以免重覆RUN時, 會重覆Append
            string fn1 = txtFilePath + "\\" + mas.DocNo + "_" + mas.Version.ToString() + "_Detail.csv";

            if (File.Exists(fn1))
            {
                File.Delete(fn1);
            }


            rowId = 1;
            foreach (var d in retFileTrans)
            {
                CaseCustMaster m = getCaseCustMaster(d.CaseCustMasterId);
                FilenameList.Add(writeTransaction(d, m,rowId));
                rowId++;
            }
            if (retFileTrans.Count() == 0) // 表示本案件, 沒有產生基資的檔案, 要把  CaseCustMaster.[RFileTransactionFileName] 押成空白
            {
                string sql = @"UPDATE CaseCustMaster SET RFileTransactionFileName='' where NewID = @masid";
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@masid", mas.NewID));
                base.ExecuteNonQuery(sql);
            }


            List<CaseCustDetails> retSbox = getReturnFileSbox(mas);            
            //20220906, 若發生原來的檔案存在, 則先刪除, 以免重覆RUN時, 會重覆Append
            string fn2 = txtFilePath + "\\" + mas.DocNo + "_" + mas.Version.ToString() + "_SBox.csv";
            if (File.Exists(fn2))
            {
                File.Delete(fn2);
            }

            rowId = 1;
            foreach (var d in retSbox)
            {
                CaseCustMaster m = getCaseCustMaster(d.CaseCustMasterId);
                FilenameList.Add(writeSBox(d, m, rowId));
                rowId++;
            }


            if( FilenameList.Count==0) // 沒有產出任何TXT檔, 表示失敗....
			{
                UpdateCaseCustMasterStatus("04", mas.NewID.ToString()); // 
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "產檔失敗: " + mas.DocNo);
            }
            else
			{
                // 壓縮Zip.. 檔名為   案號_0.Zip
                FilenameList = FilenameList.Distinct().ToList();
                // 找出目前的Master
                List<CaseCustDetails> allDetail = new List<CaseCustDetails>();
                allDetail.AddRange(retFileBase);
                allDetail.AddRange(retFileTrans);
                allDetail.AddRange(retSbox);

                var grp = allDetail.GroupBy(x => x.CaseCustMasterId).ToList();



                foreach (var g in grp)
                {
                    CaseCustMaster master = getCaseCustMaster(g.Key);
                    UpdateCaseCustMasterStatus("04", master.NewID.ToString()); // 先押失敗
                    createZip(FilenameList, master);
                }
                // HTG回文 --> 20211111, 尚未改寫.. Type 1, 3
                // ExportHtgTxt();\

                // RFDM回文 
                // ExportRFDMTxt();

                // HTG發查回文產生成功 並且 RFDF回文產生成功，將Version狀態更新成成功或者重查成功
                ModifyVersionStatus1();

                //ModifyCaseStatus2(); // 這部若產檔成功, 由createZip 那支來押'03'
            }

            // 程序結束記入log
            m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------回文結束------");
        }

        private void createZip(List<string> FilenameList, CaseCustMaster m)
        {
            List<string> zipFiles = new List<string>();

            foreach (var s in FilenameList.Distinct())
            {
                if (System.IO.File.Exists(txtFilePath + @"\" + s) && s.StartsWith(m.DocNo))
                    zipFiles.Add(txtFilePath + @"\" + s);
            }

            try
            {
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, string.Format("信息--------案件{0}的文字檔案進行壓縮處理----------------", m.DocNo));

                // 臨時文件
                string zipFilename = txtFilePath + @"\" + m.DocNo + "_0.zip";

                if (!Directory.Exists(txtFilePath))
                {
                    Directory.CreateDirectory(txtFilePath);
                }

                // 壓縮密碼
                string password = "822822" + m.DocNo.Substring(1, m.DocNo.Length - 1);

                if (System.IO.File.Exists(zipFilename))
                {
                    m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, string.Format("信息--------案件{0}的進行舊壓縮檔刪除處理----------------", m.DocNo));

                    // 刪除舊檔案
                    System.IO.File.Delete(zipFilename);

                }

                CreateZip(txtFilePath, zipFilename, zipFiles, password);

                UpdateCaseCustMasterStatus("03", m.NewID.ToString()); // 20220124,  產出ZIP, 則把Status 押 成功...
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "產檔成功: " + m.DocNo);
            }
            catch (Exception ex)
            {
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "程式異常，錯誤信息: " + ex.Source);
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "程式異常，錯誤信息: " + ex.ToString());
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "程式異常，錯誤信息: " + ex.StackTrace);
            }


        }

        /// <summary>
        /// 壓縮多個文件
        /// </summary>
        /// <param name="sourceFilePath"></param>
        /// <param name="destinationZipFilePath"></param>
        public void CreateZip(string sourceFilePath, string destinationZipFilePath, List<string> files, string password)
        {
            if (sourceFilePath[sourceFilePath.Length - 1] != System.IO.Path.DirectorySeparatorChar)
                sourceFilePath += System.IO.Path.DirectorySeparatorChar;

            ZipOutputStream zipStream = new ZipOutputStream(System.IO.File.Create(destinationZipFilePath));

            zipStream.SetLevel(6);  // 压缩级别 0-9
            zipStream.Password = password;

            foreach (string file in files)
            {
                FileStream stream = System.IO.File.OpenRead(file);

                byte[] buffer = new byte[stream.Length];

                stream.Read(buffer, 0, buffer.Length);

                string tempFile = file.Substring(sourceFilePath.LastIndexOf("\\") + 1);

                ZipEntry entry = new ZipEntry(Path.GetFileName(file));

                entry.DateTime = DateTime.Now;
                entry.Size = stream.Length;
                stream.Close();

                zipStream.PutNextEntry(entry);

                zipStream.Write(buffer, 0, buffer.Length);

            }

            zipStream.Finish();
            zipStream.Close();

            GC.Collect();
            GC.Collect(1);
        }

        private CaseCustMaster getCaseCustMaster(Guid guid)
        {
            string sql = @"select * from CaseCustMaster where NewID = @MasterId";

            base.Parameter.Clear();
            base.Parameter.Add(new CommandParameter("@MasterId", guid));
            return base.SearchList<CaseCustMaster>(sql).FirstOrDefault();
        }


        /// <summary>
        /// 讀取CaseCustOutputF2 , 以產生交易明細檔
        /// </summary>
        /// <param name="d"></param>
        private string writeTransaction(CaseCustDetails d, CaseCustMaster m, int rowId)
        {
            string retFile = string.Empty;


            try
            {
                string sql = @"select * from CaseCustOutputF2 where DetailsId = @DetailsId";
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@DetailsId", d.DetailsId));

                var drRecvData = base.SearchList<CaseCustOutputF2>(sql);


                //20220504 , 重新編寫JRNL_NO, group by Account_NO, TRAN_DATE , 由1開始...

                var grp = (from c in drRecvData
                           group c by new { c.ACCT_NO, c.TRAN_DATE } into g
                           select new { a = g.Key.ACCT_NO, d = g.Key.TRAN_DATE }).ToList();


                foreach (var g in grp)
                {
                    //var samegroup = drRecvData.Where(x => x.ACCT_NO == g.a && x.TRAN_DATE == g.d).OrderBy(y => y.JNRST_TIME).ToList();
                    int seq = 1;
                    foreach (var s in drRecvData.Where(x => x.ACCT_NO == g.a && x.TRAN_DATE == g.d).OrderBy(y => y.JNRST_TIME))
                    {
                        s.JRNL_NO = (seq++).ToString().PadLeft(8, '0');
                    }
                }

                //var debug = drRecvData.Where(x => x.ACCT_NO == "0000006111910070" && x.TRAN_DATE == "20180601").ToList();


                // RFDM內容及對應資料筆數變量
                string fileContent2 = "";
                if (rowId == 1)
                    fileContent2 = "\"身分證統一編號\",\"帳號\",\"交易序號\",\"交易日期\",\"交易時間\",\"交易行\",\"交易摘要\",\"幣別\",\"支出金額\",\"存入金額\",\"餘額\",\"ATM或端末機代號\",\"櫃員代號\",\"轉出入行庫代碼及帳號\",\"備註\"\r\n";

                // 遍歷並追加txt文件
                if (drRecvData.Count > 0)
                {
                    // 遍歷並追加txt文件
                    //for (int j = 0; j < drRecvData.Rows.Count; j++)
                    foreach (var dr in drRecvData.OrderBy(x => x.ACCT_NO).ThenBy(y => y.TRAN_DATE).ThenBy(z => z.JRNL_NO))
                    {
                        #region 拼接內容
                        // 身分證統一編號 X(10)
                        fileContent2 += "\"" + dr.CUST_ID_NO.ToString().Trim() + "\",";

                        // 帳號  X(20)
                        //20180622 RC 線上投單CR修正 宏祥 update start
                        //fileContent2 += drRecvData.Rows[j]["ACCT_NO"].ToString(), 20, drRecvData.Rows[j]["PD_TYPE_DESC"].ToString());
                        fileContent2 += "\"" + dr.ACCT_NO.ToString().Trim() + "\",";
                        //20180622 RC 線上投單CR修正 宏祥 update start

                        // 交易序號    9(08)
                        fileContent2 += "\"" + dr.JRNL_NO.ToString().Trim().PadLeft(8, '0') + "\",";

                        // 交易日期    X(08)
                        fileContent2 += "\"" + dr.TRAN_DATE.ToString().Trim() + "\",";

                        // 交易時間    X(06)???
                        fileContent2 += "\"" + dr.JNRST_TIME.ToString().Trim() + "\",";

                        // 交易行(或所屬分行代號)    X(07)
                        fileContent2 += "\"" + dr.TRAN_BRANCH.ToString().Trim() + "\",";

                        // 交易摘要    X(40)
                        fileContent2 += "\"" + dr.TXN_DESC.ToString().Trim() + "\",";

                        // 幣別  X(03)
                        string Currency = GetCurrency(dr.ACCT_NO.ToString());

                        fileContent2 += "\"" + Currency.Trim() + "\",";

                        // 支出金額    X(16)
                        // 20180119,PeterHsieh : CTBC修改規格為，當金額為負數就放在支出金額，為正就放在存入金額，另一金額欄位就放'+0'
                        //string strTRAN_AMT = dr.TRAN_AMT.ToString();
                        //strTRAN_AMT = strTRAN_AMT.Contains("-") ? strTRAN_AMT : "+0.00";

                        //20180622 RC 線上投單CR修正 宏祥 add start
                        //string strTRAN_AMT2 = strTRAN_AMT, 16, 2, false);
                        string tamt = "";
                        string samt = "";

                        if (dr.TRAN_AMT.StartsWith("-"))  // 表示是支出
                        {
                            tamt = dr.TRAN_AMT.Replace("-", "+");
                            samt = "";
                        }
                        else
                        {
                            tamt = "";
                            samt = "+" + dr.TRAN_AMT;

                        }
                        fileContent2 += "\"" + tamt.Trim() + "\"," + "\"" + samt.Trim() + "\",";

                        //20180622 RC 線上投單CR修正 宏祥 add start                            


                        // 餘額  X(16)
                        string strBALANCE = dr.BALANCE.ToString();
                        float iBal = float.Parse(strBALANCE);
                        if (iBal > 0)
                            strBALANCE = "+" + strBALANCE;
                        if (iBal < 0)
                            strBALANCE = "-" + strBALANCE;
                        if (iBal == 0)
                            strBALANCE = "+0.00";
                        fileContent2 += "\"" + strBALANCE.Trim() + "\",";



                        // ATM或端末機代號   X(20)
                        fileContent2 += "\"" + dr.ATM_NO.ToString().Trim() + "\",";

                        // 櫃員代號    X(20)
                        fileContent2 += "\"" + dr.TELLER.ToString().Trim() + "\",";

                        // 轉出入行庫代碼及帳號 (RFDM) TRF_BANK + TRF_ACCT  X(20)
                        fileContent2 += "\"" + dr.TRF_BANK.ToString().Trim() + "\",";

                        // 備註(RFDM) NARRATIVE X(300)
                        fileContent2 += "\"" + dr.NARRATIVE.ToString().Trim() + "\"";
                        #endregion

                        // 換行
                        fileContent2 += "\r\n";
                    }

                    // 調用內容拼接方法 
                    // 20220121, 改檔名為csv檔, 且確認是UTF8
                    string fn = m.DocNo + "_" + m.Version.ToString() + "_Detail.csv";
                    AppendContent(fn, fileContent2, drRecvData.Count);
                    retFile = fn;
                    // 記錄LOG
                    m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------向" + m.RFileTransactionFileName + "文件中增加" + drRecvData.Count + "筆資料!");
                }
                else
                {
                    // 查無資料也要寫檔
                    // 身分證統一編號 + '與本行無存款往來'

                    string templae = @"'ID','ACC','','','','','','','','','','','','','MEMO'";
                    templae = templae.Replace("'", "\"");


                    if (!string.IsNullOrEmpty(d.ErrorMessage) && (d.ErrorMessage.Contains("查核區間日期有誤") || d.ErrorMessage.Contains("存款帳號碼數不符")))
                    {
                        if (d.QueryType == "2" && d.ErrorMessage.Contains("查核區間日期有誤"))
                        {
                            templae = templae.Replace("ID", d.CustIdNo).Replace("ACC", "").Replace("MEMO", "查核區間日期有誤。");
                        }
                        if (d.QueryType == "4" && d.ErrorMessage.Contains("存款帳號碼數不符"))
                        {
                            templae = templae.Replace("ID", "").Replace("ACC", d.CustAccount).Replace("MEMO", "貴單位來函提供之帳號與本行存款帳號碼數不符(本行帳號碼數為12碼), 故無法提供相關資料。");
                        }
                        if (d.QueryType == "4" && d.ErrorMessage.Contains("查核區間日期有誤"))
                        {
                            templae = templae.Replace("ID", "").Replace("ACC", d.CustAccount).Replace("MEMO", "查核區間日期有誤。");
                        }

                    }
                    else
                    {
                        if (d.QueryType == "2")
                        {
                            templae = templae.Replace("ID", d.CustIdNo).Replace("ACC", "").Replace("MEMO", "此區間無交易往來明細");
                        }
                        if (d.QueryType == "4")
                        {
                            string wMessage = CheckCustomer(d.DetailsId.ToString());
                            templae = templae.Replace("ID", "").Replace("ACC", d.CustAccount).Replace("MEMO", wMessage);
                        }
                    }



                    fileContent2 += templae + "\r\n";


                    //fileContent2 += ChangeValue(string.Format("{0}此區間無交易往來明細", d.CustIdNo), 400) + "\r\n";

                    // 將內容拼接到對應的txt文件中
                    // 20220121, 改檔名為csv檔, 且確認是UTF8
                    string fn = m.DocNo + "_" + m.Version.ToString() + "_Detail.csv";
                    AppendContent(fn, fileContent2, 1);
                    retFile = fn;
                    // 記錄LOG
                    m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------向" + m.RFileTransactionFileName + "文件中增加1筆資料!");
                }

                // 更新RFDMSendStatus狀態, 將Details.RFDMSendStatus 改為99
                int RFDMInt = UpdateRFDMSendStatus(d.DetailsId.ToString());

                if (RFDMInt > 0)
                {
                    // 記錄LOG
                    m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------更新CaseCustQueryVersion表RFDMSendStatus=" + d.DetailsId.ToString() + "的RFDMSendStatus='99'");
                }


                // 更新Master.ROpenFileName             
                if (UpdateMasterRFileTransactionFileName(m.NewID, retFile) < 0)
                {
                    m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "錯誤--------更新CaseCustMaster.RFileTransactionFileName MasterID=" + d.CaseCustMasterId.ToString());
                }
            }
            catch (Exception ex)
            {                
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------讀取CaseCustOutputF2 , 以產生交易明細檔  發生錯誤------" + ex.Message.ToString());
            }

            return retFile;
        }



        private string writeSBox(CaseCustDetails d, CaseCustMaster m,int rowId)
        {

            string retFile = string.Empty;
            d.SboxFileName = m.DocNo + "_" + m.Version.ToString() + "_SBox.csv";

            retFile = d.SboxFileName;

            string sql = @"select * from CaseCustOutputF3 where DetailsId = @DetailsId";
            base.Parameter.Clear();
            base.Parameter.Add(new CommandParameter("@DetailsId", d.DetailsId));

            var drRecvData = base.SearchList<CaseCustOutputF3>(sql);

            // RFDM內容及對應資料筆數變量
            string fileContent2 = "";
            if( rowId==1)
                fileContent2 = "\"身分證統一編號\",\"出租行總分支機構代碼\",\"承租種類\",\"承租人\",\"市內電話\",\"行動電話\",\"戶籍地址\",\"通訊地址\",\"箱號或室號\",\"資料提供日\",\"承租日\",\"退租日\",\"備註\"\r\n";

            // 遍歷並追加txt文件
            if (drRecvData.Count > 0)
            {
                // 遍歷並追加txt文件
                //for (int j = 0; j < drRecvData.Rows.Count; j++)
                char C = Convert.ToChar(0);
                foreach (var dr in drRecvData)
                {
                    string strMemo = dr.MEMO.Replace(C, ' ');
                    fileContent2 += string.Format("'{0}',", dr.CUST_ID_NO==null ? "" : dr.CUST_ID_NO.Trim());
                    fileContent2 += string.Format("'{0}',", dr.RENT_BRANCH==null ? "" : dr.RENT_BRANCH.Trim() );
                    fileContent2 += string.Format("'{0}',", dr.RENT_KIND == null ? "" : dr.RENT_KIND.Trim());
                    fileContent2 += string.Format("'{0}',", dr.CUSTOMER_NAME == null ? "" : dr.CUSTOMER_NAME.Trim());
                    fileContent2 += string.Format("'{0}',", dr.NIGTEL_NO == null ? "" : dr.NIGTEL_NO.Trim());
                    fileContent2 += string.Format("'{0}',", dr.MOBIL_NO == null ? "" : dr.MOBIL_NO.Trim());
                    fileContent2 += string.Format("'{0}',", dr.COMM_ADDR == null ? "" : dr.COMM_ADDR.Trim());
                    fileContent2 += string.Format("'{0}',", dr.CUST_ADD == null ? "" : dr.CUST_ADD.Trim());
                    fileContent2 += string.Format("'{0}',", dr.BOX_NO == null ? "" : dr.BOX_NO.Trim());
                    fileContent2 += string.Format("'{0}',", dr.MODIFIED_DATE == null ? "" : dr.MODIFIED_DATE.Trim());
                    fileContent2 += string.Format("'{0}',", dr.RENT_START == null ? "" : dr.RENT_START.Trim());
                    fileContent2 += string.Format("'{0}',", dr.RENT_END == null ? "" : dr.RENT_END.Trim());
                    fileContent2 += string.Format("'{0}'", strMemo == null ? "" : strMemo.Trim());
                    
                    fileContent2 = fileContent2.Replace("'","\"")+ "\r\n"  ;                    
                }
                // 調用內容拼接方法 
                // 20220121, 改檔名為csv檔, 且確認是UTF8
                string fn = m.DocNo + "_" + m.Version.ToString() + "_SBox.csv";
                AppendContent(fn, fileContent2, drRecvData.Count);
                retFile = fn;

                // 記錄LOG
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------向" + m.RFileTransactionFileName + "文件中增加" + drRecvData.Count + "筆資料!");
            }
            else
            {

                // 查無資料也要寫檔
                CaseCustOutputF3 f3 = new CaseCustOutputF3();
                f3.CUST_ID_NO = d.CustIdNo;
                f3.MEMO = "查詢對象於本行無保管箱(室)承租資料";
                fileContent2 += string.Format("'{0}',", f3.CUST_ID_NO == null ? "" : f3.CUST_ID_NO.Trim());
                fileContent2 += string.Format("'{0}',", f3.RENT_BRANCH == null ? "" : f3.RENT_BRANCH.Trim());
                fileContent2 += string.Format("'{0}',", f3.RENT_KIND == null ? "" : f3.RENT_KIND.Trim());
                fileContent2 += string.Format("'{0}',", f3.CUSTOMER_NAME == null ? "" : f3.CUSTOMER_NAME.Trim());
                fileContent2 += string.Format("'{0}',", f3.NIGTEL_NO == null ? "" : f3.NIGTEL_NO.Trim());
                fileContent2 += string.Format("'{0}',", f3.MOBIL_NO == null ? "" : f3.MOBIL_NO.Trim());
                fileContent2 += string.Format("'{0}',", f3.COMM_ADDR == null ? "" : f3.COMM_ADDR.Trim());
                fileContent2 += string.Format("'{0}',", f3.CUST_ADD == null ? "" : f3.CUST_ADD.Trim());
                fileContent2 += string.Format("'{0}',", f3.BOX_NO == null ? "" : f3.BOX_NO.Trim());
                fileContent2 += string.Format("'{0}',", f3.MODIFIED_DATE == null ? "" : f3.MODIFIED_DATE.Trim());
                fileContent2 += string.Format("'{0}',", f3.RENT_START == null ? "" : f3.RENT_START.Trim());
                fileContent2 += string.Format("'{0}',", f3.RENT_END == null ? "" : f3.RENT_END.Trim());
                fileContent2 += string.Format("'{0}'", f3.MEMO == null ? "" : f3.MEMO.Trim());

                fileContent2 = fileContent2.Replace("'", "\"") + "\r\n";
                                




                // 將內容拼接到對應的txt文件中
                // 20220121, 改檔名為csv檔, 且確認是UTF8
                string fn = m.DocNo + "_" + m.Version.ToString() + "_SBox.csv";
                AppendContent(fn, fileContent2, 1);
                retFile = fn;
                
                // 記錄LOG
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------向" + m.RFileTransactionFileName + "文件中增加1筆資料!");
            }


            foreach (var dr in drRecvData)
            {
                // 更新RFDMSendStatus狀態, 將Details.RFDMSendStatus 改為99
                int RFDMInt = UpdateRFDMSendStatus(dr.DetailsId.ToString(), d.SboxFileName);

                if (RFDMInt > 0)
                {
                    // 記錄LOG
                    m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------更新CaseCustQueryVersion表RFDMSendStatus=" + d.DetailsId.ToString() + "的RFDMSendStatus='99'");
                }
            }



            return retFile;
        }



        /// <summary>
        /// 讀取CaseCustOutputF1 , 以產生基本資料檔
        /// </summary>
        /// <param name="d"></param>
        private string writeBasicInfo(CaseCustDetails d, CaseCustMaster m, int rowid)
        {

            string retFile = string.Empty;
            string sql = @"select * from CaseCustOutputF1 where DetailsId = @DetailsId";

            base.Parameter.Clear();
            base.Parameter.Add(new CommandParameter("@DetailsId", d.DetailsId));

            var drRecvData = base.SearchList<CaseCustOutputF1>(sql);

            // HTG&RFDM內容及對應資料筆數變量
            string fileContent1 = "";
            if( rowid==1)
                 fileContent1 = "\"身分證統一編號\",\"開戶行總分支機構代碼\",\"存款種類\",\"幣別\",\"戶名\",\"住家電話\",\"行動電話\",\"戶籍地址\",\"通訊地址\",\"帳號\",\"資料提供日\",\"開戶日\",\"結清日\",\"資料提供日帳戶結餘\",\"備註\"\r\n";
            int DataCount1 = 0;

            if (drRecvData.Count > 0)
            {
                // 拼接存款帳戶開戶資料
                //for (int q = 0; q < dt401Recv.Rows.Count; q++)
                foreach (var dr in drRecvData)
                {
                    #region 內容拼接
                    // 身分證統一編號
                    fileContent1 += "\"" + dr.CUST_ID_NO.ToString().Trim() + "\",";

                    // 開戶行總、分支機構代碼
                    fileContent1 += "\"" + dr.BRANCH_NO.ToString().Trim() + "\","; ;

                    // 存款種類	X(02)
                    string strPD_TYPE_DESC = dr.PD_TYPE_DESC.ToString();
                    strPD_TYPE_DESC = (string.IsNullOrEmpty(strPD_TYPE_DESC.Trim()) ? "99" : strPD_TYPE_DESC);
                    fileContent1 += "\"" + strPD_TYPE_DESC.Trim() + "\","; 

                    // 幣別 X(03)
                    fileContent1 += "\"" + dr.CURRENCY.ToString().Trim() + "\",";

                    // 戶名  X(60)
                    fileContent1 += "\"" + dr.CUSTOMER_NAME.ToString().Trim() + "\",";

                    // 住家電話    X(20)
                    fileContent1 += "\"" + dr.NIGTEL_NO.ToString().Trim() + "\",";

                    // 行動電話    X(20)
                    fileContent1 += "\"" + dr.MOBIL_NO.ToString().Trim() + "\",";

                    // 戶籍地址    X(200)
                    fileContent1 += "\"" + dr.CUST_ADD.ToString().Trim() + "\",";

                    // 通訊地址    X(200)
                    fileContent1 += "\"" + dr.COMM_ADDR.ToString().Trim() + "\",";

                    // 帳號  X(20)
                    //20180622 RC 線上投單CR修正 宏祥 update start
                    //fileContent1 += ChangeAccount(dt401Recv.Rows[q]["ACCT_NO"].ToString(), 20, strPD_TYPE_DESC);
                    fileContent1 += "\"" + dr.ACCT_NO.ToString().Trim() + "\",";
                    //20180622 RC 線上投單CR修正 宏祥 update end

                    // 資料提供日期  X(08) adam 2022/3/3
                    DateTime datHTGModifiedDate = new DateTime();
                    if (d.HTGModifiedDate != null)
                    {
                        datHTGModifiedDate = (DateTime)d.HTGModifiedDate;
                        fileContent1 += "\"" + datHTGModifiedDate.ToString("yyyyMMdd").Trim() + "\",";
                    }
                    else
                    {
                        fileContent1 += "\"" + datHTGModifiedDate.ToString("        ").Trim() + "\",";
                    }

                    // 開戶日 X(08)
                    fileContent1 += "\"" + dr.OPEN_DATE.ToString().Trim() + "\",";

                    // 結清日 X(08)
                    fileContent1 += "\"" + dr.CLOSE_DATE.ToString().Trim() + "\",";

                    // 資料提供日帳戶結餘   X(16)
                    string strCUR_BAL = dr.CUR_BAL.ToString();
                    //strCUR_BAL = strCUR_BAL.Contains("-") ? strCUR_BAL :  "+" + strCUR_BAL;

                    fileContent1 += "\"" + strCUR_BAL.Trim() + "\",";


                    // 備註  X(300)
                    fileContent1 += "\"" +  "" + "\"";

                    #endregion

                    // 換行
                    fileContent1 += "\r\n";

                    DataCount1++;
                }

                // 將內容拼接到對應的txt文件中
                // 20220121, 改檔名為csv檔, 且確認是UTF8
                string fn = m.DocNo + "_" + m.Version.ToString() + "_Base.csv";
                AppendContent(fn, fileContent1, DataCount1);
                retFile = fn;
                // 記錄LOG
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------向" + m.ROpenFileName.ToString() + "文件中增加" + DataCount1 + "筆資料!");
            }
            else
            {
                // 查無資料也要寫檔
                // 身分證統一編號 + '與本行無存款往來'
                string templae = @"'ID','','','','','','','','','ACC','','','','','MEMO'";
                templae = templae.Replace("'", "\"");
                if( d.QueryType=="1")
                {
                    templae = templae.Replace("ID", d.CustIdNo).Replace("ACC", "").Replace("MEMO", "查詢對象於本行無開戶資料");
                }
                if (d.QueryType == "3")
                {
                    templae = templae.Replace("ID","").Replace("ACC", d.CustAccount).Replace("MEMO", "所查詢之帳號非本行帳號");
                }

                fileContent1 += templae + "\r\n";
                //fileContent1 += ChangeValue(d.CustIdNo.ToString() + "與本行無存款往來", 395) + "\r\n";

                // 將內容拼接到對應的txt文件中
                // 20220121, 改檔名為csv檔, 且確認是UTF8
                string fn = m.DocNo + "_" + m.Version.ToString() + "_Base.csv";
                AppendContent(fn, fileContent1, 1);
                retFile = fn;
                // 記錄LOG
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------向" + m.ROpenFileName.ToString() + "文件中增加1筆資料!");
            }

            // 更新HTGSendStatus狀態, 改為99
            int HTGCount = UpdateHTGSendStatus(d.DetailsId.ToString());

            // 記錄LOG
            if (HTGCount > 0)
            {
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------更新CaseCustDetails表CaseCustNewID=" + d.DetailsId.ToString() + "的HTGSendStatus='99'");
            }

            // 更新Master.ROpenFileName             
            if(UpdateMasterROpenFileName(m.NewID, retFile)<0)
			{
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "錯誤--------更新CaseCustMaster.ROpenFileName欄位錯誤 MasterID=" + d.CaseCustMasterId.ToString() );
            }

            return retFile;
        }


        #region 

        /// <summary>
        /// 讀取基本交易的
        /// </summary>
        /// <returns></returns>
        private List<CaseCustDetails> getReturnFileBase(CaseCustMaster cm)
        {
//            string sqlSelect = @"select d.* from CaseCustMaster m inner join CaseCustDetails d on m.NewID =d.CaseCustMasterId
//where  (m.Status='02' or m.Status='06') and d.OpenFlag='Y' and d.HTGSendStatus='4'  and d.QueryType in ('1','3') ";


            string sqlSelect = @"select d.* from CaseCustMaster m inner join CaseCustDetails d on m.NewID =d.CaseCustMasterId 
                                where d.QueryType in ('1','3')  AND d.CaseCustMasterId='" + cm.NewID + "'";
            var ret = base.SearchList<CaseCustDetails>(sqlSelect);
            return ret.ToList();
        }


        /// <summary>
        /// 讀取交易明細的
        /// </summary>
        /// <returns></returns>
        private List<CaseCustDetails> getReturnFileTranscation(CaseCustMaster cm)
        {
//            string sqlSelect = @"select d.* from CaseCustMaster m inner join CaseCustDetails d on m.NewID =d.CaseCustMasterId
//where (m.Status='02' or m.Status='06') and d.OpenFlag='Y' AND d.TransactionFlag='Y' and d.HTGSendStatus='4' and d.RFDMSendStatus='8' and d.QueryType in ('2','4')";


            string sqlSelect = @"select d.* from CaseCustMaster m inner join CaseCustDetails d on m.NewID =d.CaseCustMasterId 
                                where d.QueryType in ('2','4')  AND d.CaseCustMasterId='" + cm.NewID + "'";

            var ret =  base.SearchList<CaseCustDetails>(sqlSelect);
            return ret.ToList();
        }
        #endregion




        private List<CaseCustDetails> getReturnFileSbox(CaseCustMaster cm)
        {
//            string sqlSelect = @"select d.* from CaseCustMaster m inner join CaseCustDetails d on m.NewID =d.CaseCustMasterId
//where (m.Status='02' or m.Status='06') and d.SBoxSendStatus='02' and  d.QueryType in ('5')";

            string sqlSelect = @"select d.* from CaseCustMaster m inner join CaseCustDetails d on m.NewID =d.CaseCustMasterId
                                where  d.QueryType in ('5') AND d.CaseCustMasterId='" + cm.NewID + "'";

            var ret = base.SearchList<CaseCustDetails>(sqlSelect);
            return ret.ToList();
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
                    // 獲取存款帳戶開戶資料,  20211105--> Patrick 由401中, 去組出Type1, Type3 的基本資料...
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
                         Update CaseCustDetails
                         set HTGSendStatus = '99' 
                         where DetailsId = @NewID ";

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
                    // 2021-11-05, Patrick ... 去取Type 2, Type 4的交易明細
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
                        //fileContent2 += ChangeValue(string.Format("{0}此區間無交易往來明細", dtResult.Rows[i]["CustIdNo"].ToString()), 400) + "\r\n";

                        // 20230103, 要判斷.. 未開戶跟區間無往來.. 所以呼叫method來判斷...
                        var msg = CheckCustomer(dtResult.Rows[i]["CaseCustNewID"].ToString());
                        fileContent2 += ChangeValue(string.Format("{0}{1}", dtResult.Rows[i]["CustIdNo"].ToString(),msg), 400) + "\r\n";

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
        public string CheckCustomer(string VersionNewID)
        {
            string desc = string.Empty;


            try
            {
                // 20230103, 判斷方法, 去讀401Send中的QueryErrMsg , 若有.. 檢查碼錯誤... 
                //            QueryErrMsg
                //提示代碼:0169 提示原因:檢查碼錯誤 

                string sqlSelect = @"
                                SELECT count(*)
                                from BOPS000401Send
                                where BOPS000401Send.VersionNewID = @VersionNewID
                                AND QueryErrMsg like '%提示代碼:0169%'
                              ";


                // 清空容器
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@VersionNewID", VersionNewID));


                DataTable dtResult = base.Search(sqlSelect);

                if (dtResult != null && int.Parse(dtResult.Rows[0][0].ToString()) > 0)
                {
                    desc = "與本行無存款往來";
                }
                else
                {
                    desc = "此區間無交易往來明細";

                }

                dtResult = null;
            }
            catch (Exception ex)
            {
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息--------'未開戶'或'無查詢區間資料發生錯誤------" + ex.Message.ToString());
            }

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
        public int UpdateRFDMSendStatus(string strNewID, string strfilename = null)
        {
            string sql = @"
                         Update CaseCustDetails 
                         set RFDMSendStatus = '99' , SboxFileName=@filename
                         where DetailsId = @NewID ";

            // 清空容器
            base.Parameter.Clear();
            base.Parameter.Add(new CommandParameter("@NewID", strNewID));
            base.Parameter.Add(new CommandParameter("@filename", strfilename));

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
                // 2021-11-25, Patrick , 修改條件為      
                // 將發查中的案件狀態更新為【成功】
                string sql = @"
UPDATE CaseCustDetails SET Status = '03' WHERE  EXISTS
(
SELECT DetailsId
 FROM
	 (
		SELECT DetailsId from CaseCustDetails WHERE OpenFlag = 'Y' AND HTGSendStatus = '99' AND (TransactionFlag IS NULL OR TransactionFlag <> 'Y') AND Status = '02'
		UNION
		SELECT DetailsId from CaseCustDetails WHERE TransactionFlag = 'Y' AND RFDMSendStatus = '99' AND (OpenFlag IS NULL OR OpenFlag = 'Y') AND Status = '02'
		UNION
		SELECT DetailsId from CaseCustDetails WHERE OpenFlag = 'Y' AND HTGSendStatus = '99' AND TransactionFlag = 'Y' AND RFDMSendStatus = '99' AND Status = '02'

	)RESULT
 WHERE CaseCustDetails.DetailsId = RESULT.DetailsId
);";
                // 將重查拋查中的案件狀態更新為【重查成功】
                sql += @"
UPDATE CaseCustDetails SET Status = '07' WHERE  EXISTS
(
SELECT DetailsId
 FROM
 (
	SELECT DetailsId from CaseCustDetails WHERE OpenFlag = 'Y' AND HTGSendStatus = '99' AND (TransactionFlag IS NULL OR TransactionFlag <> 'Y') AND Status = '06'
	UNION
	SELECT DetailsId from CaseCustDetails WHERE TransactionFlag = 'Y' AND RFDMSendStatus = '99' AND (OpenFlag IS NULL OR OpenFlag = 'Y') AND Status = '06'
	UNION
	SELECT DetailsId from CaseCustDetails WHERE OpenFlag = 'Y' AND HTGSendStatus = '99' AND TransactionFlag = 'Y' AND RFDMSendStatus = '99' AND Status = '06'
)RESULT
WHERE CaseCustDetails.DetailsId = RESULT.DetailsId
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
                        UpdateCaseCustMasterStatus("03", dtSuccessSend.Rows[i]["CaseCustMasterId"].ToString());
                    }
                    // 案件編號狀態為重查拋查中，並且version檔的資料全部發查成功，將主檔狀態更新為07[重查成功]
                    else if (dtSuccessSend.Rows[i]["VERSIONCOUNT"].ToString() == dtSuccessSend.Rows[i]["CASECOUNT"].ToString()
                        && dtSuccessSend.Rows[i]["Status"].ToString() == "06")
                    {
                        UpdateCaseCustMasterStatus("07", dtSuccessSend.Rows[i]["CaseCustMasterId"].ToString());
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
	RESULT.VERSIONCOUNT, RESULT.Status, CaseCustMasterId,
	(SELECT COUNT(CaseCustMasterId) FROM  CaseCustDetails WHERE CaseCustDetails.CaseCustMasterId = RESULT.CaseCustMasterId  ) AS CASECOUNT
FROM
(
	SELECT CaseCustMasterId , CaseCustMaster.Status, COUNT(CaseCustMasterId) AS VERSIONCOUNT
	from CaseCustDetails
	INNER JOIN CaseCustMaster
		ON  CaseCustDetails.CaseCustMasterId = CaseCustMaster.NewID
		AND CaseCustMaster.Status IN ('02', '06') -- 主檔案件狀態為發查中
	WHERE CaseCustDetails.Status = '03' OR CaseCustDetails.Status = '07' --Version狀態為發查成功或重查成功
	GROUP BY CaseCustDetails.CaseCustMasterId, CaseCustMaster.Status
)RESULT";

                return base.Search(strDataSql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public int UpdateMasterROpenFileName(Guid masterId, string filename)
		{
            try
            {
                string sql = @"UPDATE CaseCustMaster SET ROpenFileName = @filename  WHERE  NewID = @NewID ";

                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@filename", filename));
                base.Parameter.Add(new CommandParameter("@NewID", masterId));

                return base.ExecuteNonQuery(sql);
            }
            catch (Exception ex)
            {
                return -1;
            }
        }
        public int UpdateMasterRFileTransactionFileName(Guid masterId, string filename)
        {
            try
            {
                string sql = @"UPDATE CaseCustMaster SET RFileTransactionFileName = @filename  WHERE  NewID = @NewID ";

                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@filename", filename));
                base.Parameter.Add(new CommandParameter("@NewID", masterId));

                return base.ExecuteNonQuery(sql);
            }
            catch (Exception ex)
            {
                return -1;
            }
        }
        /// <summary>
        /// 更新案件主檔的狀態
        /// </summary>
        /// <param name="Status"></param>
        /// <param name="NewID"></param>
        /// <returns></returns>
        public int UpdateCaseCustMasterStatus(string Status, string NewID)
        {
            try
            {
                string sql = @"UPDATE CaseCustMaster SET Status = @Status  WHERE  NewID = @NewID ";

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
        public void OutPutPDF( CaseCustMaster mas)
        {
            m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息-----------開始產生PDF檔 " + mas.DocNo );

            // 查詢要列印PDF的案件編號(一個案件下有多個人，只有有一個人HTG、RFDM發查成功就產出PDF,歷史記錄功能用)
            DataTable dtPDFList = GetQueryPDFList(mas);
            // 20220926, 新增LOG ...
            m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息-----------共找到" + dtPDFList.Rows.Count + "個案件");

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
                    m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息-----------開始處理 " + dtPDFList.Rows[i]["DocNo"].ToString() + " 案件");

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
                        m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息-----------案件狀態成功, 案號為" + strSendNoNow );

                        if (strSendNoNow != "")
                        {
                            strPDFReturn = _ExportReportPDF.SavePDF(dtPDFList.Rows[i], "Y", strSendNoNow);
                            m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息-----------產生出PDF檔 " + strSendNoNow);
                        }
                        else
                        {
                            m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息-----------將狀態更新為沒有發文字號(PDFStatus='W', Status='77') " + strSendNoNow);
                            // 將狀態更新為沒有發文字號
                            UpdatePDFStatus(dtPDFList.Rows[i]["NewID"].ToString(), "W", "77");
                        }
                    }
                    else
                    {
                        m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息-----------案件狀態不成功, PDF不產生第一頁");
                        // 案件狀態不為成功，PDF不產生第一頁
                        strPDFReturn = _ExportReportPDF.SavePDF(dtPDFList.Rows[i], "N", "");
                    }


                    // 主檔的狀態為成功代表整個案件下所有人都發查成功，則更新PDF匯出狀態，下次不再產出PDF
                    if ((dtPDFList.Rows[i]["Status"].ToString() == "03" || dtPDFList.Rows[i]["Status"].ToString() == "07"
                        || dtPDFList.Rows[i]["Status"].ToString() == "66") && strPDFReturn == "Y")
                    {
                        // 將案件PDF產出狀態更新成Y
                        UpdatePDFStatus(dtPDFList.Rows[i]["NewID"].ToString(), "Y", "");
                        m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息-----------將PDFStatus='Y' " + dtPDFList.Rows[i]["DocNo"].ToString());
                    }

                }
            }

            m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "信息-----------結束產生PDF檔" );
        }

        /// <summary>
        /// 查詢要列印PDF的案件編號
        /// </summary>
        /// <returns></returns>
        public DataTable GetQueryPDFList(CaseCustMaster cm)
        {
            string sqlSelect = @"SELECT
                               CaseCust.NewID
                              , CaseCust.Status
                              , CaseCustMaster.Recipient AS ComeFile -- '受文者'
                              , CaseCustMaster.DocNo --   系統案件編號
                              , CaseCustMaster.ROpenFileName --附件1
                              , CaseCustMaster.RFileTransactionFileName --附件2
                              , LDAPEmployee.EmpID --承辦人ID
                              , LDAPEmployee.EmpName  --承辦人名字
                              , ISNULL(LDAPEmployee.TelNo, '') AS TelNo --電話
                              , ISNULL(LDAPEmployee.TelExt, '') AS TelExt --分機
                              , CaseCustMaster.LetterDeptName --   來文機關
                              , CaseCustMaster.LetterDeptNo --   來文機關代碼
                              , CaseCustMaster.LetterDate --   來文日期
                              , CaseCustMaster.LetterNo --來文字號
                              , '' AS Address -- 地址
                              , '' AS ButtomLine 
                              , '' AS ButtomLine2
                              , '' AS Speed -- 速別
                              , '' AS Security -- 密等
                              , '' AS Subject -- 主旨，內容
                              , '' AS Description -- 主旨，內容
                              , CaseCustDetails.CustIdNo + 
                               case 
	                              when  ISNULL(CaseCustDetails.countID, 0) > 0 then '等'
                              else  '' 
                              end CustIdNo  --S12XXXXX49等
                              , '' AS Remark -- 1061106 高院偵字第10637396號
                              , CaseCustMaster.Version --版本號
                              , CaseCustMaster.InCharge AS InCharge -- '承辦人員'
                              , CaseCustMaster.MessageNo AS MessageNo -- '原回文文號'
                              FROM 
                              (
	                              SELECT 
		                              CaseCustMaster.NewID, CaseCustMaster.Status 
	                              FROM CaseCustDetails
	                              INNER JOIN CaseCustMaster
		                              ON CaseCustDetails.CaseCustMasterId = CaseCustMaster.NewID
	                              WHERE (NewId=@masterId) 
									  AND (CaseCustMaster.PDFStatus IS NULL OR CaseCustMaster.PDFStatus = 'N') 
		                              AND CaseCustDetails.Status <> '01' 
		                              AND CaseCustDetails.Status <> '02' 
		                              AND CaseCustDetails.Status <> '06'
	                              GROUP BY CaseCustMaster.NewID, CaseCustMaster.Status
                              ) CaseCust
                              INNER JOIN CaseCustMaster
	                              ON CaseCust.NewID = CaseCustMaster.NewID
                              LEFT JOIN LDAPEmployee
	                              ON CaseCustMaster.QueryUserID = LDAPEmployee.EmpID
                              LEFT JOIN 
                              (
	                              SELECT CaseCustMasterId,  count(CaseCustMasterId) AS countID, MAX(CustIdNo) AS CustIdNo FROM CaseCustDetails 
	                              GROUP BY CaseCustMasterId
                              ) CaseCustDetails
                              ON CaseCustMaster.NewID = CaseCustDetails.CaseCustMasterId
                              ORDER BY CaseCustMaster.DocNo,CaseCustMaster.Version
                            ";

            // 清空容器
            base.Parameter.Clear();
            base.Parameter.Add(new CommandParameter("@masterId", cm.NewID));
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
            string sql = @" Update CaseCustMaster 
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

                string sql = @"UPDATE CaseCustMaster SET MessageNo = @SendNoNow, MessageDate = GETDATE() WHERE NewID = @NewID";

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
