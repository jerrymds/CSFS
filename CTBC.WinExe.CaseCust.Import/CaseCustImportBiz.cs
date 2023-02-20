using CTBC.CSFS.BussinessLogic;
using CTBC.CSFS.Pattern;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CTBC.CSFS.Models;

namespace CTBC.WinExe.CaseCust.Import
{
	public class CaseCustImportBiz : CommonBIZ
	{
		internal bool checkExist(string batchNo)
		{
			string sql = string.Format("Select Count(*) from GSSDoc where BatchNo='{0}';", batchNo);
			var dtResult = base.Search(sql);
			var iCount = int.Parse(dtResult.Rows[0][0].ToString());
			return iCount > 0 ? true : false;
		}

		public bool insertGSS(List<GssDoc> gssList, List<GssDoc_Detail> gssDetails)
		{
			IDbConnection dbConnection = OpenConnection();
			IDbTransaction dbTrans = null;
			try
			{
				using (dbConnection)
				{
					dbTrans = dbConnection.BeginTransaction();

					// 將字串拆分成數組
					//string[] arrayNewID = Content.Split(',');

					StringBuilder sb = new StringBuilder();
					string sql = "";
					base.Parameter.Clear();

                    //if (gssDetails.Count > 0)
                    {


                        // 先儲存detail..
                        foreach (var gd in gssDetails)
                        {
                            //sql += " delete from CaseCustQueryVersion where NewID = @NewID" + i.ToString() + "; ";

                            //base.Parameter.Add(new CommandParameter("@NewID" + i.ToString(), arrayNewID[i]));

                            string creadteddate = "";
                            if (gd.CreatedDate != null)
                                creadteddate = ((DateTime)gd.CreatedDate).ToString("yyyy/MM/dd HH:mm:ss");
                            string SendDate = "";
                            if (gd.SendDate != null)
                                SendDate = ((DateTime)gd.SendDate).ToString("yyyy/MM/dd HH:mm:ss");


                            string sqlString = string.Format("INSERT INTO [dbo].[GssDoc_Detail] ([BatchNo],[DocNo],[Metadata],[FileNames],[CreatedDate],[ParserStatus],[ParserMessage],[CaseKind],[CaseId],[SendBatchNo],[SendDate],[SendStatus],[SendMessage],[Sendmetadata]) Values ('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}','{12}','{13}') ", gd.BatchNo, gd.DocNo, gd.Metadata, gd.FileNames, creadteddate, gd.ParserStatus, gd.ParserMessage, gd.CaseKind, gd.CaseId, gd.SendBatchNo, SendDate, gd.SendStatus, gd.SendMessage, gd.Sendmetadata);

                            sb.Append(sqlString);
                        }

                        base.ExecuteNonQuery(sb.ToString());
                    }

					sb.Clear();
					// 新增主檔
					// 若案件下所有待查ID都被刪除了，同步刪除主案件
					foreach (var gd in gssList)
					{
                        string batchdate = "";
                        if( gd.BatchDate!=null)
                            batchdate = ((DateTime) gd.BatchDate).ToString("yyyy/MM/dd HH:mm:ss");
                        string creadteddate = "";
                        if( gd.CreatedDate!=null)
                            creadteddate = ((DateTime) gd.CreatedDate).ToString("yyyy/MM/dd HH:mm:ss");

                        string sqlsql = string.Format("INSERT INTO [dbo].[GssDoc]([DocType],[BatchNo],[CompanyID],[Batchmetadata],[BatchDate],[TransferType],[CreatedDate],[ParserStatus],[ParserMessage]) VALUES ('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}');",gd.DocType,gd.BatchNo,gd.CompanyID,gd.Batchmetadata,batchdate ,gd.TransferType,creadteddate,gd.ParserStatus,gd.ParserMessage);
						sb.Append(sqlsql);
					}

					base.ExecuteNonQuery(sb.ToString());

					dbTrans.Commit();

					return true;
				}
			}
			catch (Exception ex)
			{
				dbTrans.Rollback();

				return false;
			}
		}

		internal bool insertCaseCustImport(List<CaseCustImportModel> imports)
		{
			bool ret = false;
			IDbConnection dbConnection = OpenConnection();
			IDbTransaction dbTrans = null;
			try
			{
				using (dbConnection)
				{
					dbTrans = dbConnection.BeginTransaction();


					StringBuilder sb = new StringBuilder();
					string sql = "";
					base.Parameter.Clear();

					// 先儲存detail..
					foreach (var gd in imports)
					{

                        string sqlDate = string.Format("INSERT INTO CaseCustImport (caseid,Type, Id, accno, sdate, edate, ImportDate) Values ('{0}', '{1}','{2}','{3}','{4}','{5}', GETDATE() );",gd.caseid, gd.type,gd.id,gd.accno,gd.sdate,gd.edate);
						sb.Append(sqlDate);
					}

					base.ExecuteNonQuery(sb.ToString());


					dbTrans.Commit();

					return true;
				}
			}
			catch (Exception ex)
			{
				dbTrans.Rollback();

				return false;
			}



		}

		internal bool insertCaseCustDetails(string DocNo,List<CaseCustImportModel> imports, string strMainPK, FileLog m_fileLog, ref string strMasterStatus)
		{
			int txtCount = 0;
			int intIndex = 0;
			int sucessCount = 0;
			int erroCount = 0;
			// 資料格式是否正確
			bool isErro = true;
			string erroMessage = "";

            // 取出前一個工作日....
            DateTime thenow = DateTime.Now;
            DateTime lastWorkDay = getLastWorkDay(thenow); // 應該要讀取前一天的工作日才對
            

			IDbConnection dbConnection = OpenConnection();
			IDbTransaction dbTransaction = dbConnection.BeginTransaction(); 

			StringBuilder sb = new StringBuilder();

			foreach (CaseCustImportModel strLine in imports)
			{
				try
				{
					txtCount++;
					// 索引值設為0
					intIndex = 0;

					bool lineErro = true;
					//QueryType
					string QueryType = strLine.type;
					// 身分證統一編號

                    string strCustIdNo = "";
                    if (string.IsNullOrEmpty(strLine.id))
                        strCustIdNo = "          ";
                    else
                        strCustIdNo = strLine.id.Trim();

					// 開戶個人資料標記(目前無用)
					string strOpenFlag = "Y"; //20211014, 看起來預設要'Y'


                    // 20211029... Type=2, or type=4 時, TransactionFlag, 要是'Y'
					// 特定期間之交易明細往來（限新臺幣）標記==>(目前無用)
					string strTransactionFlag = "";
                    if (strLine.type == "2" || strLine.type == "4")
                        strTransactionFlag = "Y";

					// 查詢期間起日
					string strQDateS = strLine.sdate;
					// 查詢期間訖日
					string strQDateE = strLine.edate;
					string strVersionStatus = "";

                    // HTG查詢狀態
                    string strHTGSendStatus = strOpenFlag == "Y" ? "0" : null;
                    // RFDM查詢狀態
                    string strRFDMSendStatus = strTransactionFlag == "Y" ? "0" : null;


                    //20220928, 新增.. 檢查.Query 2, 4 時, 才要檢查日期是否有效...
                    if (strLine.type == "2" || strLine.type == "4")
                    {
                        // 檢查日期正確性...
                        if (!string.IsNullOrEmpty(strQDateS))
                        {
                            DateTime result;
                            if (strQDateS.StartsWith("0")) // 表示來的是民國年, 01111004 ...
                            {
                                lineErro = false;
                                erroMessage += "查核區間日期有誤。";
                            }
                            if (!DateTime.TryParseExact(strQDateS, "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out result))
                            {                            
                                    lineErro = false;
                                    erroMessage += "查核區間日期有誤。";                             
                            }

                        }

                        if (!string.IsNullOrEmpty(strQDateE))
                        {
                            DateTime result;
                            if (strQDateE.StartsWith("0")) // 表示來的是民國年, 01111004 ...
                            {
                                lineErro = false;
                                erroMessage += "查核區間日期有誤。";
                            }
                            if (!DateTime.TryParseExact(strQDateE, "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out result))
                            {                                
                                    lineErro = false;
                                    erroMessage += "查核區間日期有誤。";                                
                            }
                            // 20221014, 不檔迄日....
                            // 20220921, 檢查.. 迄日, 必須小於前一個工作日... (lastWorkDay)
                            // 例如, 今日只能查.. 20220920以前的日期
                            //if (result >= lastWorkDay) // 
                            //{
                            //    lineErro = false;
                            //    erroMessage += "迄日不得大於等於" + lastWorkDay.ToString("yyyyMMdd");
                            //}
                        }
                    }

                    //20220928, 新增.. 檢查.Query 1, 2 時, ID是必填..
                    if (strLine.type == "1" || strLine.type == "2")
                    {
                        if (String.IsNullOrEmpty(strLine.id))
                        {
                            lineErro = false;
                            erroMessage += "身份證號必填";
                        }
                        if (! String.IsNullOrEmpty(strLine.id) && strLine.id.Length>10 )
                        {
                            lineErro = false;
                            erroMessage += "身份證號長度超過10碼";
                        }

                    }



                    if (strLine.type == "3" || strLine.type == "4") // 只有... type 3, 4 時, 才會給accountNo , 才要檢查....
                    {
                        if (strLine.accno.Length == 11) // 2022-05-30, 只接受11碼或12碼, 11碼的, 左邊補零....
                        {
                            strLine.accno = "0" + strLine.accno;
                        }
                        if (strLine.accno.Length != 12) // 只收12碼的.. 若不是. 則.. 則寫到PDF及TXT
                        {
                            lineErro = false;
                            erroMessage += "貴單位來函提供之帳號與本行存款帳號碼數不符(本行帳號碼數為12碼), 故無法提供相關資料。";
                        }
                    }

                    string accno = "";

                    Guid detailsGuid = Guid.NewGuid();

                    // 為保持輸出格式不會跑版, 若是空的Accno, 則塞12個空白....
                    if (string.IsNullOrEmpty(strLine.accno))
                        accno = "            ";
                    else
                        accno= strLine.accno;


                    if (!strLine.isValid)
                    {
                        lineErro = false;
                        erroMessage += strLine.ErrorMessage;
                    }

					// 資料格式正確
					if (lineErro)
					{
						strVersionStatus = "01";
						sucessCount++;
					}
					else
					{
						strVersionStatus = "99";
						isErro = false;
						//erroMessage += "<tr><td>" + strLine + "</td></tr>";
						erroCount++;
                        // 若發生錯誤, 則必須要InSERT 一筆.. CaseCustOutputF1 , 否則版面會跑掉...
                        string outF1 = "insert into  CaseCustOutputF1(DocNo, MasterId,DetailsId,CUST_ID_NO) Values('" + DocNo + @"','" + strMainPK + "','" + detailsGuid + @"','            ');";
                        sb.Append(outF1);

					}
                    // 20220928, 發現有可能把帳號打到身份證字號那一欄, 造成Exception... 所以要限制... CustID那個欄位長度...

                    // 20221004, 發現若strCustIdNo 是空的, 也要塞10個空白, 以免跑版
                    if (String.IsNullOrEmpty(strCustIdNo.Trim()))
                    {
                        strCustIdNo = "          ";
                    }
                    else
                    {
                        if (strCustIdNo.Length > 10)
                        {
                            strCustIdNo = strCustIdNo.Substring(0, 10);
                        }
                        else
                            strCustIdNo = strCustIdNo.Trim();
                    }                    

					// INSERT CaseCustQueryVersion
                    string sqlCaseCustQueryVersion = @" INSERT INTO CaseCustDetails (DetailsId ,CaseCustMasterId ,QueryType, CustIdNo , OpenFlag , TransactionFlag , QDateS , QDateE , Status , HTGSendStatus , RFDMSendStatus , CreatedDate , CreatedUser , ModifiedDate , ModifiedUser, DocNo, CustAccount,ErrorMessage ) VALUES  ( '" + detailsGuid + @"','" + strMainPK + @"' ,'" + QueryType + @"' ,'" + strCustIdNo + @"', '" + strOpenFlag + @"' ,'" + strTransactionFlag + @"' , '" + strQDateS + @"' , '" + strQDateE + @"' ,'" + strVersionStatus + @"' ,'" + strHTGSendStatus + @"' ,  '" + strRFDMSendStatus + @"' , GETDATE() ,'SYSTEM' , GETDATE() ,'SYSTEM', '" + DocNo + "','" + accno + "','" + erroMessage + "' );";

					sb.Append(sqlCaseCustQueryVersion);

					

					// 讀完一行，索引值設為0
					intIndex = 0;


				}
				catch (Exception ex)
				{
					m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "匯入資料錯誤，具體原因：" + ex.ToString());
				}
			}


            if( sucessCount > 0) {
                strMasterStatus = "01"; // 表示押Master.Status 01 , 要打電文, 要產檔
            }
            else if (imports.Count == erroCount)
            {
                // 表示全錯.. 要在Master.Status 押 99 .. ==>txt檔案格式錯誤
                strMasterStatus = "99";
            }


			try
			{
                string aaa = sb.ToString();
				base.ExecuteNonQuery(sb.ToString() , dbTransaction);
				dbTransaction.Commit();
				return true;
			}
			catch (Exception ex)
			{
				m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "匯入資料錯誤，具體原因：" + ex.ToString());
				dbTransaction.Rollback();
				return false;
			}

		}




        internal DateTime getLastWorkDay(DateTime BizDate)
        {
            string SQL = "SELECT TOP 1 * FROM [dbo].[PARMWorkingDay] where  Flag=1 and  date<'{0}' order by [date] desc";
            SQL = string.Format(SQL, BizDate.ToString("yyyy-MM-dd"));
            var d = base.SearchList<PARMWorkingDay>(SQL).FirstOrDefault();
            if (d != null)
            {
                return d.Date;
            }
            else
            {
                return BizDate.AddDays(-1);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileGroups">Gss_Details</param>
        /// <param name="strDocNo">案號</param>
        /// <param name="strMainPK">CaseCustMasterId</param>
        /// <param name="m_fileLog">Log</param>
        /// <returns></returns>
        //internal bool insertCaseEDocFile(Program.FileGroups fileGroups, string strDocNo, string strMainPK, FileLog m_fileLog)
        //{
        //    string sql1 = @"SELECT TOP 1* FROM CaseCustMaster WHERE NEWID=@CaseID;";
        //    var cMaster = base.SearchList<CaseCustMaster>(sql1).FirstOrDefault();
        //    if (cMaster == null)
        //    {
        //        m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "找不到CaseCustMaster 的資料" +fileGroups.CaseID.ToString());
        //    }


        //    throw new NotImplementedException();
        //}

        internal bool insertCaseCustErrorCaseNo(string strDocNo, string strMainPK, Program.FileGroups fileGroups, string erroMessage,FileLog m_fileLog)
        {


            try
            {
                //Guid detailsGuid = new Guid();
                //string sqlCaseCustQueryVersion = @" INSERT INTO CaseCustDetails (DetailsId ,CaseCustMasterId ,QueryType, CustIdNo , OpenFlag , TransactionFlag , QDateS , QDateE , Status , HTGSendStatus , RFDMSendStatus , CreatedDate , CreatedUser , ModifiedDate , ModifiedUser, DocNo, CustAccount,ErrorMessage ) VALUES  ( '" + detailsGuid + @"','" + strMainPK + @"' ,'" + "0" + @"' ,'" + "          " + @"', '" + "N" + @"' ,'" + "N" + @"' , '" + "00000000" + @"' , '" + "00000000" + @"' ,'" + "88" + @"' ,'" + "0" + @"' ,  '" + "0" + @"' , GETDATE() ,'SYSTEM' , GETDATE() ,'SYSTEM', '" + strDocNo + "','" + "          " + "','" + erroMessage + "' );";

                string sqlCaseCustMaster = @"    INSERT CaseCustMaster(NewID ,DocNo ,Version ,RecvDate ,QFileName3,Status,ImportMessage,CreatedDate ,CreatedUser ,ModifiedDate ,ModifiedUser)
                                                           VALUES  (@NewID,@DocNo,'0',GETDATE() ,@QFileName3,'88',@errorMessage, GETDATE() , 'SYSTEM' ,GETDATE() , 'SYSTEM' )";


                Parameter.Clear();
                Parameter.Add(new CommandParameter("@NewID", strMainPK));
                Parameter.Add(new CommandParameter("@DocNo", strDocNo));
                Parameter.Add(new CommandParameter("@errorMessage", erroMessage));
                Parameter.Add(new CommandParameter("@QFileName3", strDocNo + "_Error.zip"));


                base.ExecuteNonQuery(sqlCaseCustMaster);
                return true;
            }
            catch (Exception ex)
            {
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "寫入錯誤，具體原因：" + ex.ToString());
                return false;
            }


        }


        internal bool insertCaseCustErrorCaseNo2(string strDocNo, string strMainPK, Program.FileGroups fileGroups, string erroMessage, FileLog m_fileLog, string LetterNo, string LetterDeptName)
        {


            try
            {
                //Guid detailsGuid = new Guid();
                //string sqlCaseCustQueryVersion = @" INSERT INTO CaseCustDetails (DetailsId ,CaseCustMasterId ,QueryType, CustIdNo , OpenFlag , TransactionFlag , QDateS , QDateE , Status , HTGSendStatus , RFDMSendStatus , CreatedDate , CreatedUser , ModifiedDate , ModifiedUser, DocNo, CustAccount,ErrorMessage ) VALUES  ( '" + detailsGuid + @"','" + strMainPK + @"' ,'" + "0" + @"' ,'" + "          " + @"', '" + "N" + @"' ,'" + "N" + @"' , '" + "00000000" + @"' , '" + "00000000" + @"' ,'" + "88" + @"' ,'" + "0" + @"' ,  '" + "0" + @"' , GETDATE() ,'SYSTEM' , GETDATE() ,'SYSTEM', '" + strDocNo + "','" + "          " + "','" + erroMessage + "' );";

                string sqlCaseCustMaster = @"    INSERT CaseCustMaster(NewID ,DocNo ,Version ,RecvDate ,QFileName3,Status,ImportMessage,CreatedDate ,CreatedUser ,ModifiedDate ,ModifiedUser, LetterNo, LetterDeptName)
                                                           VALUES  (@NewID,@DocNo,'0',GETDATE() ,@QFileName3,'88',@errorMessage, GETDATE() , 'SYSTEM' ,GETDATE() , 'SYSTEM', @LetterNo, @LetterDeptName )";
                string[] fp = fileGroups.DI.Split('\\');
                string qfile3 = fp[0] + "\\" + fp[1];

                Parameter.Clear();
                Parameter.Add(new CommandParameter("@NewID", strMainPK));
                Parameter.Add(new CommandParameter("@DocNo", strDocNo));
                Parameter.Add(new CommandParameter("@errorMessage", erroMessage));
                Parameter.Add(new CommandParameter("@LetterNo", LetterNo));
                Parameter.Add(new CommandParameter("@LetterDeptName", LetterDeptName));
                Parameter.Add(new CommandParameter("@QFileName3", qfile3 + "_Error.zip"));



                base.ExecuteNonQuery(sqlCaseCustMaster);
                return true;
            }
            catch (Exception ex)
            {
                m_fileLog.Write(FileLog.WorkType.Work, FileLog.ErrorType.None, "寫入錯誤，具體原因：" + ex.ToString());
                return false;
            }


        }

    }
}
