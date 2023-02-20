using CTBC.CSFS.Models;
using CTBC.CSFS.Pattern;
using CTBC.FrameWork.Platform;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Security.Cryptography;
using System.Text;
namespace CTBC.CSFS.BussinessLogic
{
    public class ePayBIZ : CommonBIZ
    {
        string TodayYYYMMDD = "";
        ePayCommonBIZ pePayCommonBIZ = new ePayCommonBIZ();

        /// <summary>
        /// 查詢電子支付啟動查詢清單資料
        /// </summary>
        /// <param name="model"></param>
        /// <param name="pageNum"></param>
        /// <returns></returns>
        public IList<CaseMaster> GetQueryList(CaseMaster model, int pageNum, string strSortExpression, string strSortDirection)
        {
            try
            {
                base.PageIndex = pageNum;

                #region sql
                StringBuilder sql = new StringBuilder();

                sql.Append(@"select  * from (select             		ROW_NUMBER() OVER ( ORDER BY DocNo ,ReceiveDate ) AS RowNum
                                    ,CaseMaster.CaseID
                            		,CaseMaster.CaseNo　--案件編碼 
                                   ,CaseMaster.GovNo    --來文字號
                                   ,CaseMaster.CaseKind    --類別
                                	,CaseMaster.CaseKind2    --細分類
                                    ,CaseMaster.GovUnit   --來文機關
     	                                ,CaseMaster.ReceiveDate　--來文日期	  
                                        ,CaseMaster.Status        
                                        ,CaseMaster.CreatedDate
                                        ,CaseMaster.docno   
									from CaseMaster           
                            WHERE   ReceiveKind = '電子公文'  and Status='B01' and CaseKind2 = '支付' and IsEnable <> '1' ) as TempTable  ");

                // 判斷是否分頁
                sql.Append(@" WHERE  RowNum > " + PageSize * (pageNum - 1)
                                   + " AND RowNum < " + ((PageSize * pageNum) + 1));
                #endregion

                // 資料總筆數
                string sqlCount = @"
                                select
                                    count(0)
                                from CaseMaster
                                WHERE  ReceiveKind = '電子公文' and CaseKind2 = '支付' and Status = 'B01' and IsEnable <> '1' ";
                base.DataRecords = int.Parse(base.ExecuteScalar(sqlCount).ToString());

                // 查詢清單資料
                IList<CaseMaster> _ilsit = base.SearchList<CaseMaster>(sql.ToString());

                if (_ilsit != null)
                {
                    // 處理日期
                    for (int i = 0; i < _ilsit.Count; i++)
                    {
                        _ilsit[i].LimitDate = pePayCommonBIZ.DateAdd(_ilsit[i].ReceiveDate, 7);
                    }
                    return _ilsit;
                }
                else
                {
                    return new List<CaseMaster>();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 啟動發查
        /// </summary>
        /// <returns></returns>
        public string StartSearch(User logonUser)
        {
            string pUserAccount = logonUser.Account;

            // 查詢案件狀態爲未處理資料
            int checknumberMax = GetCaseMasterCheckMax();
            int checknumber = GetCheckStock();
            DataTable dt = GetCaseMasterQueryData();

            // 20200515 檢查支票本
            if ( checknumber < checknumberMax)
            {
                return "CheckNoData";
            }

            //
            IDbConnection dbConnection = OpenConnection();
            IDbTransaction dbTrans = null;

            try
            {
                using (dbConnection)
                {
                    dbTrans = dbConnection.BeginTransaction();

                    base.Parameter.Clear();

                    if (dt != null && dt.Rows.Count > 0)
                    {
                        //// 記錄CaseCustQuery主鍵值/sql語句變量
                        string updateSql = "";


                        for (int i = 0; i < dt.Rows.Count; i++)
                        {

                            Boolean processFlag = true;

                            if (processFlag)
                            {    

                                #region update  CaseMaster
                                // 判斷該筆資料與上筆資料是否屬於同一個案件，如果不屬於，就更新該資料所屬案件的案件狀態欄位
                                    // CaseCustQuery狀態改爲拋查中
                                    updateSql += @"
                                    update  CaseMaster
                                    set IsEnable = '1'
                                        ,ModifiedDate = getdate()
                                        ,ModifiedUser = @CreatedUser
                                    where CaseId = @CaseId" + i.ToString() + "; ";

                                    base.Parameter.Add(new CommandParameter("@CaseId" + i.ToString(), dt.Rows[i]["CaseId"]));
                                #endregion

                            }
                        }

                        // 若有可發查案件，才更新表格
                        if (!string.IsNullOrEmpty(updateSql))
                        {
                            base.Parameter.Add(new CommandParameter("@CreatedUser", pUserAccount));

                            base.ExecuteNonQuery(updateSql, dbTrans);

                            // insert或update ApprMsgKey表
                            SaveMsg(dt, logonUser, dbTrans);

                        }

                        dbTrans.Commit();
                       return "OK";
                    }
                    else
                    {
                        return "NoData";
                    }
                }
            }
            catch (Exception ex)
            {
                dbTrans.Rollback();
                return "Error";
            }
        }

        /// <summary>
        /// 查詢需要啟動發查資料
        /// </summary>
        /// <returns></returns>
        public DataTable GetCaseMasterQueryData()
        {
            try
            {

                string sql = @"
                               select  *   
									from CaseMaster           
                            WHERE   ReceiveKind = '電子公文'  and Status='B01' and CaseKind2 = '支付'  and IsEnable <> '1' 
                            order by CaseMaster.CaseNo,CaseMaster.ReceiveDate,CaseMaster.CaseId desc ";

                return base.Search(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public int GetCheckStock()
        {
            try
            {

                string sql = @"
                               SELECT 
	                            D.[CheckIntervalID],
	                            D.[CheckNo],
	                            D.[Kind],
	                            D.[IsUsed],
	                            D.[IsPreserve]
                            FROM [CheckInterval] AS M
                            LEFT OUTER JOIN [CheckUse] AS D ON M.CheckIntervalID = D.CheckIntervalID
                            WHERE 
	                           M.UseStatus = '已使用' 
	                           AND D.IsUsed = 0 
	                           AND D.IsPreserve = 0
                            ORDER BY M.CheckIntervalID ASC , D.CheckNo ASC  ";

                DataTable dt = base.Search(sql);

                if (dt != null && dt.Rows.Count > 0)
                {
                    return dt.Rows.Count;
                }
                else
                {
                    return 0;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public int GetCaseMasterCheckMax()
        {
            try
            {

                string sql = @"
                               SELECT CaseNo,d.ReceiveName  FROM [dbo].[CaseMaster] c 
left join dbo.EDocTXT3_Detail d on c.CaseId = d.CaseId
   where CaseKind2='支付' and ReceiveKind='電子公文' and c.Status='B01'  and IsEnable <> '1'  ";

                DataTable dt = base.Search(sql);

                if (dt != null && dt.Rows.Count > 0)
                {
                    return dt.Rows.Count;
                }
                else
                {
                    return 0;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public DataTable QueryRFDMSend(string strVersionNewID)
        {
            try
            {

                string sql = @"
                            SELECT 
                                CaseCustRFDMSend.TrnNum as TrnNum
                                ,ISNULL(acctDesc,'') as acctDesc
                            FROM CaseCustRFDMSend
                            WHERE VersionNewID = '" + strVersionNewID + "'";

                return base.Search(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 案件總筆數
        /// </summary>
        /// <returns></returns>
        public int GetDataCount()
        {
            try
            {
                int DataCount = 0;

                string sql = @"
                            select CaseId
                            from CaseMaster
                            WHERE  ReceiveKind = '電子公文' and Status='B01' and CaseKind2 = '支付' and  IsEnable <>  '1'
                             ";

                DataTable dt = base.Search(sql);

                if (dt != null && dt.Rows.Count > 0)
                {
                    DataCount = dt.Rows.Count;
                }

                return DataCount;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #region insert ApprMsgKey

        /// <summary>
        /// insert或update ApprMsgKey表
        /// </summary>
        /// <param name="dt">資料集</param>
        /// <param name="logonUser"><登錄人員信息/param>
        /// <param name="dbTrans">事務</param>
        public void SaveMsg(DataTable dt, User logonUser, IDbTransaction dbTrans)
        {
            for (int j = 0; j < dt.Rows.Count; j++)
            {
                    SaveMsgKey(new Guid(dt.Rows[j]["CaseId"].ToString()), logonUser, dbTrans);
            }
        }

        /// <summary>
        /// 保存ApprMsgKey資料
        /// </summary>
        /// <param name="strVersionNewID">VersionNewID</param>
        /// <param name="logonUser">登錄人員資料</param>
        /// <param name="dbTrans">事務</param>
        /// <returns></returns>
        public bool SaveMsgKey(Guid strCaseId, User logonUser, IDbTransaction dbTrans)
        {
            try
            {
                bool flag = false;

                // 獲取登錄人員資料
                ApprMsgKeyVO model = new ApprMsgKeyVO();
                model.MsgUID = logonUser.Account;
                model.MsgKeyLP = logonUser.LDAPPwd;
                model.MsgKeyLU = logonUser.Account;
                model.MsgKeyRU = logonUser.RCAFAccount;
                model.MsgKeyRP = logonUser.RCAFPs;
                model.MsgKeyRB = logonUser.RCAFBranch;

                // VersionNewID
                model.VersionNewID = strCaseId;

                // 判斷資料是否存在ApprMsgKey,如果不存在就可向ApprMsgKey增加資料
                if (!isExistInMsgKey(strCaseId, logonUser.Account, dbTrans))
                {
                    flag = InsertApprMsgKey(model, dbTrans);
                }
                return flag;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 判斷是否存在ApprMsgKey
        /// </summary>
        /// <param name="strVersionNewID"></param>
        /// <param name="logonUser">登錄人員ID</param>
        /// <param name="dbTrans">事務</param>
        /// <returns></returns>
        public bool isExistInMsgKey(Guid strVersionNewID, string logonUser, IDbTransaction dbTrans)
        {
            try
            {
                string strSql = @"SELECT  COUNT(*)
                                  FROM    dbo.ApprMsgKey
                                  WHERE   VersionNewID = @VersionNewID
                                          AND MsgUID = @MsgUID ";
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@VersionNewID", strVersionNewID));
                base.Parameter.Add(new CommandParameter("@MsgUID", Encode(logonUser)));
                int n = (int)base.ExecuteScalar(strSql, dbTrans);
                if (n > 0) return true;
                else return false;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// insert ApprMsgKey
        /// </summary>
        /// <param name="model">實體資料</param>
        /// <param name="dbTrans">事務</param>
        /// <returns></returns>
        public bool InsertApprMsgKey(ApprMsgKeyVO model, IDbTransaction dbTrans)
        {
            try
            {
                string sql = @"INSERT INTO dbo.ApprMsgKey
                                                    ( MsgKeyLU ,
                                                      MsgKeyLP ,
                                                      MsgKeyRU ,
                                                      MsgKeyRP ,
                                                      MsgKeyRB ,
                                                      MsgUID ,
                                                      VersionNewID
                                                    )
                                            VALUES  ( @MsgKeyLU, 
                                                     @MsgKeyLP ,
                                                     @MsgKeyRU ,
                                                     @MsgKeyRP ,
                                                     @MsgKeyRB ,
                                                     @MsgUID ,
                                                     @VersionNewID )";
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@MsgKeyLU", Encode(model.MsgKeyLU)));
                base.Parameter.Add(new CommandParameter("@MsgKeyLP", Encode(model.MsgKeyLP)));
                base.Parameter.Add(new CommandParameter("@MsgKeyRU", Encode(model.MsgKeyRU)));
                base.Parameter.Add(new CommandParameter("@MsgKeyRP", Encode(model.MsgKeyRP)));
                base.Parameter.Add(new CommandParameter("@MsgKeyRB", Encode(model.MsgKeyRB)));
                base.Parameter.Add(new CommandParameter("@MsgUID", Encode(model.MsgUID)));
                base.Parameter.Add(new CommandParameter("@VersionNewID", model.VersionNewID));
                return base.ExecuteNonQuery(sql, dbTrans) > 0 ? true : false;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 加密
        /// </summary>
        /// <param name="data">加密字串</param>
        /// <returns></returns>
        public string Encode(string data)
        {
            string KEY_64 = "VavicApp";
            string IV_64 = "VavicApp";

            byte[] byKey = System.Text.ASCIIEncoding.ASCII.GetBytes(KEY_64);
            byte[] byIV = System.Text.ASCIIEncoding.ASCII.GetBytes(IV_64);

            DESCryptoServiceProvider cryptoProvider = new DESCryptoServiceProvider();
            int i = cryptoProvider.KeySize;
            MemoryStream ms = new MemoryStream();
            CryptoStream cst = new CryptoStream(ms, cryptoProvider.CreateEncryptor(byKey, byIV), CryptoStreamMode.Write);
            StreamWriter sw = new StreamWriter(cst);
            sw.Write(data);
            sw.Flush();
            cst.FlushFinalBlock();
            sw.Flush();

            return Convert.ToBase64String(ms.GetBuffer(), 0, (int)ms.Length);
        }

        /// <summary>
        /// 解密
        /// </summary>
        /// <param name="data">解密字串</param>
        /// <returns></returns>
        public string Decode(string data)
        {
            string KEY_64 = "VavicApp";
            string IV_64 = "VavicApp";

            byte[] byKey = System.Text.ASCIIEncoding.ASCII.GetBytes(KEY_64);
            byte[] byIV = System.Text.ASCIIEncoding.ASCII.GetBytes(IV_64);
            byte[] byEnc;

            try
            {
                byEnc = Convert.FromBase64String(data);
            }
            catch
            {
                return null;
            }

            DESCryptoServiceProvider cryptoProvider = new DESCryptoServiceProvider();
            MemoryStream ms = new MemoryStream(byEnc);
            CryptoStream cst = new CryptoStream(ms, cryptoProvider.CreateDecryptor(byKey, byIV), CryptoStreamMode.Read);
            StreamReader sr = new StreamReader(cst);

            return sr.ReadToEnd();
        }

        #endregion

        /// <summary>
        /// 查詢CaseCustRFDMSend中本月最大的流水號
        /// </summary>
        /// <returns></returns>
        public string GetMaxTrnNum()
        {
            try
            {

                string sql = @"
                            select 
                            	isnull(MAX(TrnNum),'') as TrnNumMax 
                            from CaseCustRFDMSend
                            where TrnNum like '" + DateTime.Now.ToString("yyyyMM") + "%' ";

                DataTable dt = base.Search(sql);

                if (dt != null && dt.Rows.Count > 0)
                {
                    return dt.Rows[0]["TrnNumMax"].ToString();
                }
                else
                {
                    return "";
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 計算流水號
        /// YYYMMDD+4位數流水號
        /// </summary>
        /// <param name="strMaxNo">根據最大流水號+1(純數字流水號)</param>
        /// <returns></returns>
        private string CalculateTrnNum(int strMaxNo)
        {
            return TodayYYYMMDD + String.Format("{0:D5}", strMaxNo + 1);
        }

        /// <summary>
        /// 取得查詢迄日要往前 n日
        /// </summary>
        /// <returns>往前 n日</returns>
        public int GetParmCodeEndDateDiff()
        {
           string sqlSelect = @"Select CodeDesc from PARMCode where CodeType='CSFSCode' and CodeNo='PreDay' ";

            // 清空容器
            base.Parameter.Clear();

            string day = base.Search(sqlSelect).Rows[0][0].ToString();

            return Convert.ToInt16(string.IsNullOrEmpty(day) ? "3" : day);
        }


    }
}
