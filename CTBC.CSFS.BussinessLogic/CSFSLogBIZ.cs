/// <summary>
/// 程式說明：維護CSFSLog
/// 維護部門:資訊管理處
/// 中國信託銀行 版權所有  ©  All Rights Reserved.
/// </summary>

using System;
using System.Web;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CTBC.CSFS.Pattern;
using Microsoft.Practices.EnterpriseLibrary.Logging;
using System.Data;
using CTBC.CSFS.Models;

namespace CTBC.CSFS.BussinessLogic
{
    public class CSFSLogBIZ : CommonBIZ
    {
        public CSFSLogBIZ(AppController appController)
            : base(appController)
        { }

        public CSFSLogBIZ()
        { }

        public IEnumerable<CSFSLogVO> GetLogList()
        {
            try
            {
                string sql = @"select * from CSFSLog";
                base.Parameter.Clear();
                return base.SearchList<CSFSLogVO>(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IList<CSFSLogVO> GetQueryList(CSFSLogVO csfsLog, int pageIndex)
        {
            try
            {
                base.PageIndex = pageIndex;
                string sqlStr = "";
                string sqlWhere = "";
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@pageS", (base.PageSize * (base.PageIndex - 1)) + 1));
                base.Parameter.Add(new CommandParameter("@pageE", base.PageSize * base.PageIndex));

                if (!string.IsNullOrEmpty(csfsLog.StartTime.ToString()) && !string.IsNullOrEmpty(csfsLog.EndTime.ToString()))
                {
                    sqlWhere += @" and convert(date,Timestamp,111)>= convert(date,@SDte,111) AND  convert(date,Timestamp,111)<= convert(date,@EDte,111) ";
                    base.Parameter.Add(new CommandParameter("@SDte", csfsLog.StartTime));
                    base.Parameter.Add(new CommandParameter("@EDte", csfsLog.EndTime));
                }
                else if (!string.IsNullOrEmpty(csfsLog.StartTime.ToString()) && string.IsNullOrEmpty(csfsLog.EndTime.ToString()))
                {
                    sqlWhere += @" and DATEDIFF(day,Timestamp,@SDte) = 0 ";
                    base.Parameter.Add(new CommandParameter("@SDte", csfsLog.StartTime));
                }

                if (!string.IsNullOrEmpty(csfsLog.Title))
                {
                    sqlWhere += @" and Title like @Title ";
                    base.Parameter.Add(new CommandParameter("@Title", "%" + csfsLog.Title.Trim() + "%"));
                }

                if (!string.IsNullOrEmpty(csfsLog.Message))
                {
                    sqlWhere += @" and Message like @Message ";
                    base.Parameter.Add(new CommandParameter("@Message", "%" + csfsLog.Message.Trim() + "%"));
                }

                if (!string.IsNullOrEmpty(csfsLog.UserId))
                {
                    sqlWhere += @" and UserId like @UserId ";
                    base.Parameter.Add(new CommandParameter("@UserId", "%" + csfsLog.UserId.Trim() + "%"));
                }

                if (!string.IsNullOrEmpty(csfsLog.FunctionId))
                {
                    sqlWhere += @" and FunctionId like @FunctionId ";
                    base.Parameter.Add(new CommandParameter("@FunctionId", "%" + csfsLog.FunctionId.Trim() + "%"));
                }

                if (!string.IsNullOrEmpty(csfsLog.URL))
                {
                    sqlWhere += @" and URL like @URL ";
                    base.Parameter.Add(new CommandParameter("@URL", "%" + csfsLog.URL.Trim() + "%"));
                }

                if (!string.IsNullOrEmpty(csfsLog.IP))
                {
                    sqlWhere += @" and IP like @IP ";
                    base.Parameter.Add(new CommandParameter("@IP", "%" + csfsLog.IP.Trim() + "%"));
                }

                if (!string.IsNullOrEmpty(csfsLog.MachineName))
                {
                    sqlWhere += @" and MachineName like @MachineName ";
                    base.Parameter.Add(new CommandParameter("@MachineName", "%" + csfsLog.MachineName.Trim() + "%"));
                }

                sqlStr += @";with T1 
	                        as
	                        (
		                        select LogID from CSFSLog with(nolock)
			                        where 1=1 " + sqlWhere + @" 
	                        ),T2 as
	                        (
		                        select *, row_number() over (order by LogID desc) RowNum
		                        from T1
	                        ),T3 as 
	                        (
		                        select *,(select max(RowNum) from T2) maxnum from T2 
		                        where rownum between @pageS and @pageE
	                        )
	                        select a.RowNum,a.LogID,b.Timestamp,b.Title,b.Message,b.UserId,b.FunctionId,b.SessionId,b.URL,b.IP,b.MachineName,a.maxnum 
                            from T3 a
	                        left join CSFSLog b with(nolock) on b.LogId = a.LogID order by a.RowNum";// +sqlOrderType;

                IList<CSFSLogVO> _ilsit = base.SearchList<CSFSLogVO>(sqlStr);

                if (_ilsit != null)
                {
                    if (_ilsit.Count > 0)
                    {
                        base.DataRecords = _ilsit[0].maxnum;
                    }
                    else
                    {
                        base.DataRecords = 0;
                        _ilsit = new List<CSFSLogVO>();
                    }
                    return _ilsit;
                }
                else
                {
                    return new List<CSFSLogVO>();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        /// <summary>
        /// 寫到DB
        /// </summary>
        /// <param name="log"></param>
        /// 20130918 hroace
        public static void WriteLog(CSFSLog log)
        {
            try
            {
                log.Categories.Add("CSFS");
                log.TimeStamp = DateTime.Now;
                log.Priority = 3;
                log.EventId = 105;
                log.Severity = System.Diagnostics.TraceEventType.Information;
                if (HttpContext.Current != null)
                {
                    log.dic["UserId"] = (HttpContext.Current.Session["UserAccount"] != null) ? HttpContext.Current.Session["UserAccount"].ToString() : "Anonymous"; ;
                    log.dic["SessionId"] = HttpContext.Current.Session.SessionID;
                    log.dic["URL"] = HttpContext.Current.Request.RawUrl;
                    log.dic["IP"] = HttpContext.Current.Request.UserHostAddress;
                    log.dic["MachineName"] = HttpContext.Current.Request.UserHostName;
                    log.dic["Result"] = Result.Success;
                }
                else
                {
                    string tm = "Lose Session";
                    log.dic["UserId"] = "Anonymous";
                    log.dic["SessionId"] = tm;
                    log.dic["URL"] = tm;
                    log.dic["IP"] = tm;
                    log.dic["MachineName"] = tm;
                    log.dic["Result"] = Result.Failure;
                }
                log.dic["ActionCode"] = ActionCode.Update;
                log.dic["TranFlag"] = TranFlag.After;
                log.dic["FunctionId"] = "";
                log.ExtendedProperties = log.dic;
                Logger.Write(log);
                log.Categories.Remove("CSFS");
            }
            catch (Exception ex) { throw ex; }
        }
        //20150209弱掃
        public void LogonLogoutLog(CSFSLog ApLog)
        {
            Logger.Write(ApLog);
        }

        /// <summary>
        /// 20161007固定變更 宏祥
        /// </summary>
        /// <returns></returns>
        public string GetCSFSLog_HistoryTimestamp()
        {
            string strTimestamp = "";

            try
            {
                string strSql = @"select TOP 1 Timestamp from CSFSLog_History order by Timestamp desc";
                DataTable dt = base.Search(strSql);
                if (dt.Rows.Count > 0)
                {
                    strTimestamp = string.Format("{0:yyyy-MM-dd HH:mm:ss.fff}", dt.Rows[0][0]);         
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return strTimestamp;
        }

        public int InsertCSFSLog(string sTitle, string sMessage, string sUserId)
        {
            try
            {
           string strSql = @"INSERT INTO CSFSLog 
           ([Timestamp]
           ,[Title]
           ,[Message]
           ,[Priority]
           ,[EventID] 
           ,[Severity]
           ,[UserId]
           ,[Result]
           ,[ActionCode]
           ,[TranFlag]
           ,[FunctionId]
           ,[SessionId]
           ,[URL]
           ,[IP]
           ,[MachineName])
     VALUES
           (getdate()
           ,@Title 
           ,@Message 
           ,0 
           ,0
           ,'Sys'
           ,@UserId 
           ,null 
           ,null 
           ,null 
           ,null 
           ,null
           ,null 
           ,null
           ,'Sys') ";
                base.Parameter.Clear();
                // 添加參數
                base.Parameter.Add(new CommandParameter("@Title", sTitle));
                base.Parameter.Add(new CommandParameter("@Message", sMessage));
                base.Parameter.Add(new CommandParameter("@UserId", sUserId));
                return base.ExecuteNonQuery(strSql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 20161007固定變更 宏祥 DBHouseKeeping(暫不使用)
        /// </summary>
        /// <param name="model"></param>
        /// <param name="DocNo"></param>
        /// <param name="trans"></param>
        /// <returns></returns>
        public int InsertCSFSLog_History(string stroldTimestamp)
        {
            try 
	        {
                string strSqlwhere = @" Timestamp > @Timestamp ";

                if (stroldTimestamp == "")
                {
                    stroldTimestamp = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss.fff");
                    strSqlwhere = @" Timestamp < @Timestamp ";
                }

                string strSql = @" insert into CSFSLog_History select * from CSFSLog where " + strSqlwhere;
                base.Parameter.Clear();

                // 添加參數
                base.Parameter.Add(new CommandParameter("@Timestamp", stroldTimestamp));
                return base.ExecuteNonQuery(strSql);
	        }
	        catch (Exception ex)
	        {		
		        throw ex;
	        } 
        }

        /// <summary>
        /// 20161007固定變更 宏祥 DBHouseKeeping
        /// </summary>
        /// <param name="model"></param>
        /// <param name="DocNo"></param>
        /// <param name="trans"></param>
        /// <returns></returns>
        public int InsertCSFSLog_History()
        {
            try
            {
                string strSql = @" insert into CSFSLog_History select * from CSFSLog ";
                base.Parameter.Clear();

                return base.ExecuteNonQuery(strSql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 20161007固定變更 宏祥 DBHouseKeeping
        /// </summary>
        /// <param name="model"></param>
        /// <param name="DocNo"></param>
        /// <param name="trans"></param>
        /// <returns></returns>
        public int DeleteCSFSLog()
        {
            try
            {
                string strSql = @" delete CSFSLog ";
                base.Parameter.Clear();

                return base.ExecuteNonQuery(strSql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 20161007固定變更 宏祥 DBHouseKeeping
        /// </summary>
        /// <param name="model"></param>
        /// <param name="DocNo"></param>
        /// <param name="trans"></param>
        /// <returns></returns>
        public int InsertCSFSLogToCategory_History()
        {
            try
            {
                string strSql = @" insert into CSFSLogToCategory_History select * from CSFSLogToCategory ";
                base.Parameter.Clear();

                return base.ExecuteNonQuery(strSql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 20161007固定變更 宏祥 DBHouseKeeping
        /// </summary>
        /// <param name="model"></param>
        /// <param name="DocNo"></param>
        /// <param name="trans"></param>
        /// <returns></returns>
        public int DeleteCSFSLogToCategory()
        {
            try
            {
                string strSql = @" delete CSFSLogToCategory ";
                base.Parameter.Clear();

                return base.ExecuteNonQuery(strSql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}