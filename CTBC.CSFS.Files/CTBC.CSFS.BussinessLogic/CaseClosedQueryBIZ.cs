using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using CTBC.CSFS.Models;
using CTBC.CSFS.Pattern;
using CTBC.FrameWork.Util;
using NPOI.HSSF.UserModel;
using NPOI.HSSF.Util;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using System.Linq;
using System.Reflection;

namespace CTBC.CSFS.BussinessLogic
{
    public class CaseClosedQueryBIZ : CommonBIZ
    {
        //獲取收件查詢的結果
        //用印薄
        public DataTable GetList(CaseClosedQuery model, string depart)
        {
            string sqlWhere = "";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("@depart", depart));
            Parameter.Add(new CommandParameter("Status", CaseStatus.DirectorApproveSeizureAndPay));
            //類型
            if (!string.IsNullOrEmpty(model.CaseKind))
            {
                sqlWhere += @" and CaseKind like @CaseKind ";
                Parameter.Add(new CommandParameter("@CaseKind", "" + model.CaseKind.Trim() + ""));
            }
            if (!string.IsNullOrEmpty(model.CaseKind2))
            {
                sqlWhere += @" and CaseKind2 like @CaseKind2 ";
                Parameter.Add(new CommandParameter("@CaseKind2", "" + model.CaseKind2.Trim() + ""));
            }
            //收件日期
            if (!string.IsNullOrEmpty(model.ReceiveDateStart))
            {
                sqlWhere += @" AND ReceiveDate >= @ReceiveDateStart";
                Parameter.Add(new CommandParameter("@ReceiveDateStart", model.ReceiveDateStart));
            }
            if (!string.IsNullOrEmpty(model.ReceiveDateEnd))
            {
                string receiveDateEnd = Convert.ToDateTime(UtlString.FormatDateString(model.ReceiveDateEnd)).AddDays(1).ToString("yyyyMMdd");
                sqlWhere += @" AND ReceiveDate < @ReceiveDateEnd ";
                Parameter.Add(new CommandParameter("@ReceiveDateEnd", receiveDateEnd));
            }
            //發文日期
            if (!string.IsNullOrEmpty(model.SendDateStart))
            {
                sqlWhere += @" AND SendDate >= @SendDateStart";
                Parameter.Add(new CommandParameter("@SendDateStart", model.SendDateStart));
            }
            if (!string.IsNullOrEmpty(model.SendDateEnd))
            {
                string sendDateEnd = Convert.ToDateTime(UtlString.FormatDateString(model.SendDateEnd)).AddDays(1).ToString("yyyyMMdd");
                sqlWhere += @" AND SendDate < @SendDateEnd ";
                Parameter.Add(new CommandParameter("@SendDateEnd", sendDateEnd));
            }
            //結案日期
            if (!string.IsNullOrEmpty(model.CloseDateStart))
            {
                sqlWhere += @" AND CloseDate >= @CloseDateStart";
                Parameter.Add(new CommandParameter("@CloseDateStart", model.CloseDateStart));
            }
            if (!string.IsNullOrEmpty(model.CloseDateEnd))
            {
                string closeDateEnd = Convert.ToDateTime(UtlString.FormatDateString(model.CloseDateEnd)).AddDays(1).ToString("yyyyMMdd");
                sqlWhere += @" AND CloseDate < @CloseDateEnd ";
                Parameter.Add(new CommandParameter("@CloseDateEnd", closeDateEnd));
            }
            var sqlStr = @"SELECT (css.SendWord+ '字' + css.SendNo +'號') as newSend,m.CaseNo,
                        A.EmpName,m.CaseKind,m.CaseKind2,D.EmpName,M.[OverDueMemo] as overnote,SerialID
                        FROM [CaseMaster] AS M
                        LEFT OUTER JOIN [CaseSendSetting] AS CSS ON M.CaseId = CSS.CaseId
                        LEFT OUTER JOIN [V_AgentAndDept] AS A ON A.EmpID = M.AgentUser
                        LEFT OUTER JOIN [V_AgentAndDept] AS D ON D.EmpID = M.ApproveUser
                        where ([Status] LIKE 'Z%' or [Status]=@Status) and M.[AgentSection] =@depart and  CaseKind2<>'撤銷' and
                        SendWord  is not null and SendNo is not null  " + sqlWhere + "";
            DataTable Dt = Search(sqlStr);
            Dt.Columns.Add("Receive").SetOrdinal(2);
            Dt.Columns.Add("Cc").SetOrdinal(3);
            if (Dt != null && Dt.Rows.Count > 0)
            {
                string strSerialId = string.Empty;
                foreach (DataRow dr in Dt.Rows)
                {
                    strSerialId += "'" + dr["SerialID"].ToString() + "',";
                }
                strSerialId = strSerialId.TrimEnd(',');
                string sqlRecive = "SELECT GovName,SerialID,CaseId FROM CaseSendSettingDetails WHERE SendType=1 and SerialID In (" + strSerialId + ")";
                string sqlCc = "SELECT GovName,SerialID,CaseId FROM CaseSendSettingDetails WHERE SendType=2 and SerialID In (" + strSerialId + ")";
                List<CaseSendSettingDetails> listRecive = base.SearchList<CaseSendSettingDetails>(sqlRecive).ToList();
                List<CaseSendSettingDetails> listCc = base.SearchList<CaseSendSettingDetails>(sqlCc).ToList();
                if (listCc != null && listCc.Any())
                {
                    foreach (DataRow dr in Dt.Rows)
                    {
                        string strRecive = string.Empty;
                        string strCc = string.Empty;
                        foreach (CaseSendSettingDetails item in listRecive.Where(m => m.SerialID == Convert.ToInt32(dr["SerialID"])))
                        {
                            strRecive += item.GovName + "、";
                        }
                        strRecive = strRecive.TrimEnd('、');
                        dr["Receive"] = strRecive;

                        foreach (CaseSendSettingDetails item in listCc.Where(m => m.SerialID == Convert.ToInt32(dr["SerialID"])))
                        {
                            strCc += item.GovName + "、";
                        }
                        strCc = strCc.TrimEnd('、');
                        dr["Cc"] = strCc;
                    }
                }
            }
            return Dt;
        }
        public DataTable GetList1(CaseClosedQuery model, string depart)
        {
            string sqlWhere = "";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("@depart", depart));
            //類型
            if (!string.IsNullOrEmpty(model.CaseKind))
            {
                sqlWhere += @" and CaseKind like @CaseKind ";
                Parameter.Add(new CommandParameter("@CaseKind", "" + model.CaseKind.Trim() + ""));
            }
            if (!string.IsNullOrEmpty(model.CaseKind2))
            {
                sqlWhere += @" and CaseKind2 like @CaseKind2 ";
                Parameter.Add(new CommandParameter("@CaseKind2", "" + model.CaseKind2.Trim() + ""));
            }
            //發文方式
            if (!string.IsNullOrEmpty(model.SendKind))
            {
                sqlWhere += @" and B.SendKind like @SendKind ";
                Parameter.Add(new CommandParameter("@SendKind", "" + model.SendKind.Trim() + ""));
			}
			//電子發文上傳日
            if (!string.IsNullOrEmpty(model.SendUpDateStart))
            {
                sqlWhere += @" AND SendUpDate >= @SendUpDateStart";
                Parameter.Add(new CommandParameter("@SendUpDateStart", model.SendUpDateStart));
            }
            if (!string.IsNullOrEmpty(model.SendUpDateEnd))
            {
                string SendUpDateEnd = Convert.ToDateTime(UtlString.FormatDateString(model.SendUpDateEnd)).AddDays(1).ToString("yyyy/MM/dd");
                sqlWhere += @" AND SendUpDate < @SendUpDateEnd ";
                Parameter.Add(new CommandParameter("@SendUpDateEnd", SendUpDateEnd));
            }
            //收件日期
            if (!string.IsNullOrEmpty(model.ReceiveDateStart))
            {
                sqlWhere += @" AND ReceiveDate >= @ReceiveDateStart";
                Parameter.Add(new CommandParameter("@ReceiveDateStart", model.ReceiveDateStart));
            }
            if (!string.IsNullOrEmpty(model.ReceiveDateEnd))
            {
                string receiveDateEnd = Convert.ToDateTime(UtlString.FormatDateString(model.ReceiveDateEnd)).AddDays(1).ToString("yyyyMMdd");
                sqlWhere += @" AND ReceiveDate < @ReceiveDateEnd ";
                Parameter.Add(new CommandParameter("@ReceiveDateEnd", receiveDateEnd));
            }
            //發文日期
            if (!string.IsNullOrEmpty(model.SendDateStart))
            {
                sqlWhere += @" AND SendDate >= @SendDateStart";
                Parameter.Add(new CommandParameter("@SendDateStart", model.SendDateStart));
            }
            if (!string.IsNullOrEmpty(model.SendDateEnd))
            {
                string sendDateEnd = Convert.ToDateTime(UtlString.FormatDateString(model.SendDateEnd)).AddDays(1).ToString("yyyyMMdd");
                sqlWhere += @" AND SendDate < @SendDateEnd ";
                Parameter.Add(new CommandParameter("@SendDateEnd", sendDateEnd));
            }
            //主管放行日
            if (!string.IsNullOrEmpty(model.ApproveDateStart))
            {
                sqlWhere += @" AND ApproveDate2 >= @ApproveDateStart";
                Parameter.Add(new CommandParameter("@ApproveDateStart", model.ApproveDateStart));
            }
            if (!string.IsNullOrEmpty(model.ApproveDateEnd))
            {
                string approveDateEnd = Convert.ToDateTime(UtlString.FormatDateString(model.ApproveDateEnd)).AddDays(1).ToString("yyyyMMdd");
                sqlWhere += @" AND ApproveDate2 < @ApproveDateEnd ";
                Parameter.Add(new CommandParameter("@ApproveDateEnd", approveDateEnd));
            }
            var sqlStr = @" SELECT 
                                B.SerialID,
                                B.SendWord + '字' + B.SendNo +'號' AS SendNo,
                                A.CaseNo,
                                C.EmpName AS CreatedUserName,
                                A.CaseKind,
                                A.CaseKind2,
                                CASE WHEN (A.CaseKind2 = '扣押並支付' AND B.Template = '支付') THEN E.EmpName ELSE D.EmpName END AS ApproveUserName,
                                --B.SendKind,
                                --B.SendUpDate,
                                CONVERT(nvarchar(10),A.LimitDate,111) as LimiteDate,
	                            CONVERT(nvarchar(10),A.ApproveDate,111) as ApproveDate
                            FROM CaseMaster AS A
                            INNER JOIN CaseSendSetting AS B ON A.CaseId = B.CaseId
                            LEFT OUTER JOIN [V_AgentAndDept] AS C ON C.EmpID = A.AgentUser
                            LEFT OUTER JOIN [V_AgentAndDept] AS D ON D.EmpID = A.ApproveUser
                            LEFT OUTER JOIN [V_AgentAndDept] AS E ON E.EmpID = A.ApproveUser2
                            WHERE  A.[AgentSection] =@depart
	                            AND
                            (    
                                (
                                    (A.CaseKind2 <> '扣押並支付' OR (A.CaseKind2 = '扣押並支付' AND B.Template = '扣押'))
                                    AND A.ApproveDate IS NOT NULL 
                                    " + sqlWhere.Replace("ApproveDate2", "ApproveDate") + @"
                                )
                                OR
                                (
                                    A.CaseKind2 = '扣押並支付' 
                                    AND B.Template = '支付'
                                    AND A.ApproveDate IS NOT NULL 
                                    AND A.ApproveDate2 IS NOT NULL 
                                    " + sqlWhere + @"
                                )  
                            ) 
                            ";

            DataTable dt = Search(sqlStr);
            dt.Columns.Add("Receiver").SetOrdinal(3);
            dt.Columns.Add("CC").SetOrdinal(4);
            dt.Columns.Add("overnote").SetOrdinal(9);
            if (dt != null && dt.Rows.Count > 0)
            {
                string strSerialId = string.Empty;
                foreach (DataRow dr in dt.Rows)
                {
                    if (Convert.ToDateTime(dr["ApproveDate"]) > Convert.ToDateTime(dr["LimiteDate"]))
                    {
                        dr["overnote"] = "V";
                    }
                    strSerialId += "'" + dr["SerialID"] + "',";
                }
                strSerialId = strSerialId.TrimEnd(',');
                sqlStr = "  SELECT   SerialID,GovName FROM CaseSendSettingDetails WHERE SendType=1 AND  SerialID in (" + strSerialId + ")";
                IList<CaseSendSettingDetails> RecieveList = base.SearchList<CaseSendSettingDetails>(sqlStr);
                if (RecieveList != null && RecieveList.Any())
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        string strReceiver = string.Empty;
                        foreach (CaseSendSettingDetails item in RecieveList.Where(m => m.SerialID == Convert.ToInt32(dr["SerialID"])))
                        {
                            strReceiver += item.GovName + "、";
                        }
                        strReceiver = strReceiver.TrimEnd('、');
                        dr["Receiver"] = strReceiver;
                    }
                }
                sqlStr = "  SELECT  SerialID,GovName FROM CaseSendSettingDetails WHERE SendType=2 AND  SerialID in (" + strSerialId + ")";
                IList<CaseSendSettingDetails> CCList = base.SearchList<CaseSendSettingDetails>(sqlStr);
                if (CCList != null && CCList.Any())
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        string strCC = string.Empty;
                        foreach (CaseSendSettingDetails item in CCList.Where(m => m.SerialID == Convert.ToInt32(dr["SerialID"])))
                        {
                            strCC += item.GovName + "、";
                        }
                        strCC = strCC.TrimEnd('、');
                        dr["CC"] = strCC;
                    }
                }
            }
            dt.DefaultView.Sort = "CreatedUserName,CaseNo";
            dt = dt.DefaultView.ToTable();
            return dt;
        }

        /// <summary>
        /// 獲取電子發文明細
        /// </summary>
        /// <param name="model"></param>
        /// <param name="depart"></param>
        /// <returns></returns>
        public DataTable GetCaseSend1(CaseClosedQuery model, string depart)
        {
            string sqlWhere = "";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("@depart", depart));
            //類型
            if (!string.IsNullOrEmpty(model.CaseKind))
            {
                sqlWhere += @" and CaseKind like @CaseKind ";
                Parameter.Add(new CommandParameter("@CaseKind", "" + model.CaseKind.Trim() + ""));
            }
            if (!string.IsNullOrEmpty(model.CaseKind2))
            {
                sqlWhere += @" and CaseKind2 like @CaseKind2 ";
                Parameter.Add(new CommandParameter("@CaseKind2", "" + model.CaseKind2.Trim() + ""));
            }
            //發文方式
            sqlWhere += @" and B.SendKind like @SendKind ";
            Parameter.Add(new CommandParameter("@SendKind", "" + model.SendKind.Trim() + ""));
            
            if (!string.IsNullOrEmpty(model.SendUpDateStart))
            {
                sqlWhere += @" AND SendUpDate >= @SendUpDateStart";
                Parameter.Add(new CommandParameter("@SendUpDateStart", model.SendUpDateStart));
            }
            if (!string.IsNullOrEmpty(model.SendUpDateEnd))
            {
                string SendUpDateEnd = Convert.ToDateTime(UtlString.FormatDateString(model.SendUpDateEnd)).AddDays(1).ToString("yyyy/MM/dd");
                sqlWhere += @" AND SendUpDate < @SendUpDateEnd ";
                Parameter.Add(new CommandParameter("@SendUpDateEnd", SendUpDateEnd));
            }
            //收件日期
            if (!string.IsNullOrEmpty(model.ReceiveDateStart))
            {
                sqlWhere += @" AND ReceiveDate >= @ReceiveDateStart";
                Parameter.Add(new CommandParameter("@ReceiveDateStart", model.ReceiveDateStart));
            }
            if (!string.IsNullOrEmpty(model.ReceiveDateEnd))
            {
                string receiveDateEnd = Convert.ToDateTime(UtlString.FormatDateString(model.ReceiveDateEnd)).AddDays(1).ToString("yyyyMMdd");
                sqlWhere += @" AND ReceiveDate < @ReceiveDateEnd ";
                Parameter.Add(new CommandParameter("@ReceiveDateEnd", receiveDateEnd));
            }
            //發文日期
            if (!string.IsNullOrEmpty(model.SendDateStart))
            {
                sqlWhere += @" AND SendDate >= @SendDateStart";
                Parameter.Add(new CommandParameter("@SendDateStart", model.SendDateStart));
            }
            if (!string.IsNullOrEmpty(model.SendDateEnd))
            {
                string sendDateEnd = Convert.ToDateTime(UtlString.FormatDateString(model.SendDateEnd)).AddDays(1).ToString("yyyyMMdd");
                sqlWhere += @" AND SendDate < @SendDateEnd ";
                Parameter.Add(new CommandParameter("@SendDateEnd", sendDateEnd));
            }
            //主管放行日
            if (!string.IsNullOrEmpty(model.ApproveDateStart))
            {
                sqlWhere += @" AND ApproveDate2 >= @ApproveDateStart";
                Parameter.Add(new CommandParameter("@ApproveDateStart", model.ApproveDateStart));
            }
            if (!string.IsNullOrEmpty(model.ApproveDateEnd))
            {
                string approveDateEnd = Convert.ToDateTime(UtlString.FormatDateString(model.ApproveDateEnd)).AddDays(1).ToString("yyyyMMdd");
                sqlWhere += @" AND ApproveDate2 < @ApproveDateEnd ";
                Parameter.Add(new CommandParameter("@ApproveDateEnd", approveDateEnd));
            }
            var sqlStr = @" SELECT 
                                B.SerialID,
                                B.SendWord + '字' + B.SendNo +'號' AS SendNo,
                                A.CaseNo,
                                C.EmpName AS CreatedUserName,
                                A.CaseKind,
                                A.CaseKind2,
                                CASE WHEN (A.CaseKind2 = '扣押並支付' AND B.Template = '支付') THEN E.EmpName ELSE D.EmpName END AS ApproveUserName,
                                --B.SendKind,
                                CONVERT(varchar(100), B.SendUpDate, 120) as SendUpDate,
                                CONVERT(nvarchar(10),A.LimitDate,111) as LimiteDate, 
	                            CONVERT(nvarchar(10),A.ApproveDate,111) as ApproveDate
                            FROM CaseMaster AS A
                            INNER JOIN CaseSendSetting AS B ON A.CaseId = B.CaseId
                            INNER JOIN BatchControl AS BC ON B.CaseId = BC.CaseId
                            LEFT OUTER JOIN [V_AgentAndDept] AS C ON C.EmpID = A.AgentUser
                            LEFT OUTER JOIN [V_AgentAndDept] AS D ON D.EmpID = A.ApproveUser
                            LEFT OUTER JOIN [V_AgentAndDept] AS E ON E.EmpID = A.ApproveUser2
                            WHERE  A.[AgentSection] =@depart and BC.STATUS_Transfer=1
	                            AND
                            (    
                                (
                                    (A.CaseKind2 <> '扣押並支付' OR (A.CaseKind2 = '扣押並支付' AND B.Template = '扣押'))
                                    AND A.ApproveDate IS NOT NULL 
                                    " + sqlWhere.Replace("ApproveDate2", "ApproveDate") + @"
                                )
                                OR
                                (
                                    A.CaseKind2 = '扣押並支付' 
                                    AND B.Template = '支付'
                                    AND A.ApproveDate IS NOT NULL 
                                    AND A.ApproveDate2 IS NOT NULL 
                                    " + sqlWhere + @"
                                )  
                            ) 
                            ";

            DataTable dt = Search(sqlStr);
            dt.Columns.Add("Receiver").SetOrdinal(3);
            dt.Columns.Add("CC").SetOrdinal(4);
            dt.Columns.Add("overnote").SetOrdinal(9);
            if (dt != null && dt.Rows.Count > 0)
            {
                string strSerialId = string.Empty;
                foreach (DataRow dr in dt.Rows)
                {
                    if (Convert.ToDateTime(dr["ApproveDate"]) > Convert.ToDateTime(dr["LimiteDate"]))
                    {
                        dr["overnote"] = "V";
                    }
                    strSerialId += "'" + dr["SerialID"] + "',";
                }
                strSerialId = strSerialId.TrimEnd(',');
                sqlStr = "  SELECT   SerialID,GovName FROM CaseSendSettingDetails WHERE SendType=1 AND  SerialID in (" + strSerialId + ")";
                IList<CaseSendSettingDetails> RecieveList = base.SearchList<CaseSendSettingDetails>(sqlStr);
                if (RecieveList != null && RecieveList.Any())
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        string strReceiver = string.Empty;
                        foreach (CaseSendSettingDetails item in RecieveList.Where(m => m.SerialID == Convert.ToInt32(dr["SerialID"])))
                        {
                            strReceiver += item.GovName + "、";
                        }
                        strReceiver = strReceiver.TrimEnd('、');
                        dr["Receiver"] = strReceiver;
                    }
                }
                sqlStr = "  SELECT  SerialID,GovName FROM CaseSendSettingDetails WHERE SendType=2 AND  SerialID in (" + strSerialId + ")";
                IList<CaseSendSettingDetails> CCList = base.SearchList<CaseSendSettingDetails>(sqlStr);
                if (CCList != null && CCList.Any())
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        string strCC = string.Empty;
                        foreach (CaseSendSettingDetails item in CCList.Where(m => m.SerialID == Convert.ToInt32(dr["SerialID"])))
                        {
                            strCC += item.GovName + "、";
                        }
                        strCC = strCC.TrimEnd('、');
                        dr["CC"] = strCC;
                    }
                }
            }
            dt.DefaultView.Sort = "CreatedUserName,CaseNo";
            dt = dt.DefaultView.ToTable();
            return dt;
        }
        public DataTable GetCountList(CaseClosedQuery model, string depart)
        {
            string sqlWhere = "";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("@depart", depart));
            Parameter.Add(new CommandParameter("Status", CaseStatus.DirectorApproveSeizureAndPay));

            //類型
            if (!string.IsNullOrEmpty(model.CaseKind))
            {
                sqlWhere += @" and CaseKind like @CaseKind ";
                Parameter.Add(new CommandParameter("@CaseKind", "" + model.CaseKind.Trim() + ""));
            }
            if (!string.IsNullOrEmpty(model.CaseKind2))
            {
                sqlWhere += @" and CaseKind2 like @CaseKind2 ";
                Parameter.Add(new CommandParameter("@CaseKind2", "" + model.CaseKind2.Trim() + ""));
            }
            //收件日期
            if (!string.IsNullOrEmpty(model.ReceiveDateStart))
            {
                sqlWhere += @" AND ReceiveDate >= @ReceiveDateStart";
                Parameter.Add(new CommandParameter("@ReceiveDateStart", model.ReceiveDateStart));
            }
            if (!string.IsNullOrEmpty(model.ReceiveDateEnd))
            {
                string receiveDateEnd = Convert.ToDateTime(UtlString.FormatDateString(model.ReceiveDateEnd)).AddDays(1).ToString("yyyyMMdd");
                sqlWhere += @" AND ReceiveDate < @ReceiveDateEnd ";
                Parameter.Add(new CommandParameter("@ReceiveDateEnd", receiveDateEnd));
            }
            //發文日期
            if (!string.IsNullOrEmpty(model.SendDateStart))
            {
                sqlWhere += @" AND SendDate >= @SendDateStart";
                Parameter.Add(new CommandParameter("@SendDateStart", model.SendDateStart));
            }
            if (!string.IsNullOrEmpty(model.SendDateEnd))
            {
                string sendDateEnd = Convert.ToDateTime(UtlString.FormatDateString(model.SendDateEnd)).AddDays(1).ToString("yyyyMMdd");
                sqlWhere += @" AND SendDate < @SendDateEnd ";
                Parameter.Add(new CommandParameter("@SendDateEnd", sendDateEnd));
            }
            //結案日期
            if (!string.IsNullOrEmpty(model.CloseDateStart))
            {
                sqlWhere += @" AND CloseDate >= @CloseDateStart";
                Parameter.Add(new CommandParameter("@CloseDateStart", model.CloseDateStart));
            }
            if (!string.IsNullOrEmpty(model.CloseDateEnd))
            {
                string closeDateEnd = Convert.ToDateTime(UtlString.FormatDateString(model.CloseDateEnd)).AddDays(1).ToString("yyyyMMdd");
                sqlWhere += @" AND CloseDate < @CloseDateEnd ";
                Parameter.Add(new CommandParameter("@CloseDateEnd", closeDateEnd));
            }
            var sqlStr = @";WITH T 
                        AS
                        (SELECT (css.SendWord+ '字' + css.SendNo +'號') as newSend,m.CaseNo,
                        A.EmpName as AgentUserEmpName,m.CaseKind,m.CaseKind2,D.EmpName as ApproveUserEmpName,M.[OverDueMemo] as overnote
                        FROM [CaseMaster] AS M
                        LEFT OUTER JOIN [CaseSendSetting] AS CSS ON M.CaseId = CSS.CaseId
                        LEFT OUTER JOIN [V_AgentAndDept] AS A ON A.EmpID = M.AgentUser
                        LEFT OUTER JOIN [V_AgentAndDept] AS D ON D.EmpID = M.ApproveUser
                        where ([Status] LIKE 'Z%' or [Status]=@Status) and M.[AgentSection] =@depart and  CaseKind2<>'撤銷'
                        and  SendWord  is not null and SendNo is not null  " + sqlWhere + @")
                        select COUNT(*) AS C,CaseKind2 from t GROUP BY CaseKind2 HAVING CaseKind2 IN ('扣押','支付','扣押並支付')
                        UNION ALL
                        SELECT COUNT(*) AS C,CaseKind FROM T GROUP BY CaseKind HAVING CASEKIND='外來文案件'";
            DataTable Dt = base.Search(sqlStr);
            return Dt;
        }

        //經辦結案統計表
        public DataTable GetCaseMasterList(CaseClosedQuery model, string depart)
        {
            string sqlWhere = "";
            string sqlWhereTmp = "";
            string sqlWhere1 = "";
            string sqlWhere2 = "";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("depart", depart));
            Parameter.Add(new CommandParameter("Status", CaseStatus.AgentReturnClose));

            //類型
            if (!string.IsNullOrEmpty(model.CaseKind))
            {
                sqlWhere += @" AND C.CaseKind LIKE @CaseKind ";
                Parameter.Add(new CommandParameter("@CaseKind", "" + model.CaseKind.Trim() + ""));
            }
            if (!string.IsNullOrEmpty(model.CaseKind2))
            {
                sqlWhere += @" AND C.CaseKind2 LIKE @CaseKind2 ";
                Parameter.Add(new CommandParameter("@CaseKind2", "" + model.CaseKind2.Trim() + ""));
            }
            //收件日期
            if (!string.IsNullOrEmpty(model.ReceiveDateStart))
            {
                sqlWhere += @" AND ReceiveDate >= @ReceiveDateStart";
                Parameter.Add(new CommandParameter("@ReceiveDateStart", model.ReceiveDateStart));
            }
            if (!string.IsNullOrEmpty(model.ReceiveDateEnd))
            {
                string receiveDateEnd = Convert.ToDateTime(UtlString.FormatDateString(model.ReceiveDateEnd)).AddDays(1).ToString("yyyyMMdd");
                sqlWhere += @" AND ReceiveDate < @ReceiveDateEnd ";
                Parameter.Add(new CommandParameter("@ReceiveDateEnd", receiveDateEnd));
            }
            //發文日期
            if (!string.IsNullOrEmpty(model.SendDateStart) || !string.IsNullOrEmpty(model.SendDateEnd))
            {
                if (!string.IsNullOrEmpty(model.SendDateStart))
                {
                    sqlWhereTmp += @" AND SendDate >= @SendDateStart";
                    Parameter.Add(new CommandParameter("@SendDateStart", model.SendDateStart));
                }
                if (!string.IsNullOrEmpty(model.SendDateEnd))
                {
                    string sendDateEnd = Convert.ToDateTime(UtlString.FormatDateString(model.SendDateEnd)).AddDays(1).ToString("yyyy/MM/dd");
                    sqlWhereTmp += @" AND SendDate < @SendDateEnd ";
                    Parameter.Add(new CommandParameter("@SendDateEnd", sendDateEnd));
                }
                sqlWhere1 += @" AND CaseId IN (SELECT CaseId  FROM CaseSendSetting AS CSS   where [Template] <> '支付' " + sqlWhereTmp + ") ";
                sqlWhere2 += @" AND CaseId IN (SELECT CaseId  FROM CaseSendSetting AS CSS   where [Template] = '支付' " + sqlWhereTmp + ") ";
            }
            //主管放行日
            if (!string.IsNullOrEmpty(model.CloseDateStart))
            {
                sqlWhere += @" AND CloseDate >= @CloseDateStart";
                Parameter.Add(new CommandParameter("@CloseDateStart", model.CloseDateStart));
            }
            if (!string.IsNullOrEmpty(model.CloseDateEnd))
            {
                string CloseDateEnd = Convert.ToDateTime(UtlString.FormatDateString(model.CloseDateEnd)).AddDays(1).ToString("yyyyMMdd");
                sqlWhere += @" AND CloseDate < @CloseDateEnd ";
                Parameter.Add(new CommandParameter("@CloseDateEnd", CloseDateEnd));
            }

            var sqlStr = @"WITH  
                            BastTable AS(
	                            SELECT  (CaseKind+'-'+CaseKind2) AS New_CaseKind,AgentUser FROM CaseMaster C WHERE ApproveDate IS NOT NULL " + sqlWhere.Replace("ApproveDate2", "ApproveDate") + sqlWhere1 + @" 
                                UNION ALL
	                            SELECT  (CaseKind+'-'+CaseKind2) AS New_CaseKind,AgentUser2 AS AgentUser  FROM CaseMaster C WHERE  ApproveDate IS NOT NULL AND ApproveDate2 IS NOT NULL " + sqlWhere + sqlWhere2 + @" 
                            ),
                            UserByKind AS(
	                            SELECT  New_CaseKind,AgentUser,COUNT(1) AS case_num 
	                            FROM BastTable
                                GROUP BY New_CaseKind, AgentUser
                            ),
                            UserCounts AS 
                            (
	                            SELECT  AgentUser,COUNT(1) AS User_Count 
	                            FROM BastTable 
	                            GROUP BY AgentUser
                            )
                            SELECT 
	                            A.New_CaseKind,
	                            A.AgentUser,
	                            A.case_num,
	                            employee.EmpName,
	                            userCounts.User_Count
                            FROM  UserByKind AS A 
                            INNER JOIN [V_AgentAndDept] AS employee ON a.AgentUser = employee.EmpID
                            LEFT JOIN UserCounts ON a.AgentUser=userCounts.AgentUser
                            WHERE employee.SectionName=@depart
                            ORDER BY New_CaseKind, EmpID";
            DataTable dt = Search(sqlStr);
            return dt;
            /*
            string sqlWhere = "";
            string sqlWhere1 = "";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("depart", depart));
            Parameter.Add(new CommandParameter("Status", CaseStatus.AgentReturnClose));

            //類型
            if (!string.IsNullOrEmpty(model.CaseKind))
            {
                sqlWhere += @" and CaseKind like @CaseKind ";
                Parameter.Add(new CommandParameter("@CaseKind", "" + model.CaseKind.Trim() + ""));
            }
            if (!string.IsNullOrEmpty(model.CaseKind2))
            {
                sqlWhere += @" and CaseKind2 like @CaseKind2 ";
                Parameter.Add(new CommandParameter("@CaseKind2", "" + model.CaseKind2.Trim() + ""));
            }
            //收件日期
            if (!string.IsNullOrEmpty(model.ReceiveDateStart))
            {
                sqlWhere += @" AND ReceiveDate >= @ReceiveDateStart";
                Parameter.Add(new CommandParameter("@ReceiveDateStart", model.ReceiveDateStart));
            }
            if (!string.IsNullOrEmpty(model.ReceiveDateEnd))
            {
                string receiveDateEnd = Convert.ToDateTime(UtlString.FormatDateString(model.ReceiveDateEnd)).AddDays(1).ToString("yyyyMMdd");
                sqlWhere += @" AND ReceiveDate < @ReceiveDateEnd ";
                Parameter.Add(new CommandParameter("@ReceiveDateEnd", receiveDateEnd));
            }
            //發文日期
            if (!string.IsNullOrEmpty(model.SendDateStart) || !string.IsNullOrEmpty(model.SendDateEnd))
            {
                if (!string.IsNullOrEmpty(model.SendDateStart))
                {
                    sqlWhere1 += @" AND SendDate >= @SendDateStart";
                    Parameter.Add(new CommandParameter("@SendDateStart", model.SendDateStart));
                }
                if (!string.IsNullOrEmpty(model.SendDateEnd))
                {
                    string sendDateEnd = Convert.ToDateTime(UtlString.FormatDateString(model.SendDateEnd)).AddDays(1).ToString("yyyyMMdd");
                    sqlWhere1 += @" AND SendDate < @SendDateEnd ";
                    Parameter.Add(new CommandParameter("@SendDateEnd", sendDateEnd));
                }
                sqlWhere += " AND CaseId IN (SELECT CaseId  FROM CaseSendSetting AS CSS where 1=1 " + sqlWhere1 + @") ";
            }
            //結案日期
            if (!string.IsNullOrEmpty(model.CloseDateStart))
            {
                sqlWhere += @" AND CloseDate >= @CloseDateStart";
                Parameter.Add(new CommandParameter("@CloseDateStart", model.CloseDateStart));
            }
            if (!string.IsNullOrEmpty(model.CloseDateEnd))
            {
                string closeDateEnd = Convert.ToDateTime(UtlString.FormatDateString(model.CloseDateEnd)).AddDays(1).ToString("yyyyMMdd");
                sqlWhere += @" AND CloseDate < @CloseDateEnd ";
                Parameter.Add(new CommandParameter("@CloseDateEnd", closeDateEnd));
            }

            var sqlStr = @" select A.*, employee.EmpName,(select count(1) from CaseMaster where AgentUser=a.AgentUser and [Status] LIKE 'Z%' 
                                " + sqlWhere + @") as UserCount
                                from (select  (CaseKind+'-'+CaseKind2) as New_CaseKind,AgentUser, count(1) as case_num from CaseMaster
                                where [Status] LIKE 'Z%' " + sqlWhere + @"
                                group by (CaseKind+'-'+CaseKind2), AgentUser) as A 
                                INNER JOIN [V_AgentAndDept] AS employee ON A.AgentUser = employee.EmpID
                                WHERE employee.SectionName = @depart
                                order by New_CaseKind, EmpID ";
            DataTable dt = Search(sqlStr);
            return dt;
             * */
        }

        public DataTable GetCaseMasterList1(CaseClosedQuery model, string depart)
        {
            string sqlWhere = "";
            string sqlWhereTmp = "";
            string sqlWhere1 = "";
            string sqlWhere2 = "";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("depart", depart));
            Parameter.Add(new CommandParameter("Status", CaseStatus.AgentReturnClose));

            //類型
            if (!string.IsNullOrEmpty(model.CaseKind))
            {
                sqlWhere += @" AND C.CaseKind LIKE @CaseKind ";
                Parameter.Add(new CommandParameter("@CaseKind", "" + model.CaseKind.Trim() + ""));
            }
            if (!string.IsNullOrEmpty(model.CaseKind2))
            {
                sqlWhere += @" AND C.CaseKind2 LIKE @CaseKind2 ";
                Parameter.Add(new CommandParameter("@CaseKind2", "" + model.CaseKind2.Trim() + ""));
            }
            //收件日期
            if (!string.IsNullOrEmpty(model.ReceiveDateStart))
            {
                sqlWhere += @" AND ReceiveDate >= @ReceiveDateStart";
                Parameter.Add(new CommandParameter("@ReceiveDateStart", model.ReceiveDateStart));
            }
            if (!string.IsNullOrEmpty(model.ReceiveDateEnd))
            {
                string receiveDateEnd = Convert.ToDateTime(UtlString.FormatDateString(model.ReceiveDateEnd)).AddDays(1).ToString("yyyyMMdd");
                sqlWhere += @" AND ReceiveDate < @ReceiveDateEnd ";
                Parameter.Add(new CommandParameter("@ReceiveDateEnd", receiveDateEnd));
            }
            //發文日期
            if (!string.IsNullOrEmpty(model.SendDateStart))
            {
                sqlWhereTmp += @" AND SendDate >= @SendDateStart";
                Parameter.Add(new CommandParameter("@SendDateStart", model.SendDateStart));
            }
            if (!string.IsNullOrEmpty(model.SendDateEnd))
            {
                string sendDateEnd = Convert.ToDateTime(UtlString.FormatDateString(model.SendDateEnd)).AddDays(1).ToString("yyyy/MM/dd");
                sqlWhereTmp += @" AND SendDate < @SendDateEnd ";
                Parameter.Add(new CommandParameter("@SendDateEnd", sendDateEnd));
            }
			//發文方式
            if (!string.IsNullOrEmpty(model.SendKind))
            {
                sqlWhereTmp += @" AND SendKind = @SendKind ";
                Parameter.Add(new CommandParameter("@SendKind", model.SendKind));
            }
			//電子發文上傳日
            if (!string.IsNullOrEmpty(model.SendUpDateStart))
            {
                sqlWhereTmp += @" AND SendUpDate >= @SendUpDateStart";
                Parameter.Add(new CommandParameter("@SendUpDateStart", model.SendUpDateStart));
            }
            if (!string.IsNullOrEmpty(model.SendUpDateEnd))
            {
                string SendUpDateEnd = Convert.ToDateTime(UtlString.FormatDateString(model.SendUpDateEnd)).AddDays(1).ToString("yyyy/MM/dd");
                sqlWhereTmp += @" AND SendUpDate < @SendUpDateEnd ";
                Parameter.Add(new CommandParameter("@SendUpDateEnd", SendUpDateEnd));
            }
			if (!string.IsNullOrEmpty(model.SendDateStart) || !string.IsNullOrEmpty(model.SendDateEnd) || !string.IsNullOrEmpty(model.SendKind) || !string.IsNullOrEmpty(model.SendUpDateStart) || !string.IsNullOrEmpty(model.SendUpDateEnd))
			{
				sqlWhere1 += @" AND CaseId IN (SELECT CaseId  FROM CaseSendSetting AS CSS   where [Template] <> '支付' " + sqlWhereTmp + ") ";
				sqlWhere2 += @" AND CaseId IN (SELECT CaseId  FROM CaseSendSetting AS CSS   where [Template] = '支付' " + sqlWhereTmp + ") ";	
			}
            //主管放行日
            if (!string.IsNullOrEmpty(model.ApproveDateStart))
            {
                sqlWhere += @" AND ApproveDate2 >= @ApproveDateStart";
                Parameter.Add(new CommandParameter("@ApproveDateStart", model.ApproveDateStart));
            }
            if (!string.IsNullOrEmpty(model.ApproveDateEnd))
            {
                string approveDateEnd = Convert.ToDateTime(UtlString.FormatDateString(model.ApproveDateEnd)).AddDays(1).ToString("yyyyMMdd");
                sqlWhere += @" AND ApproveDate2 < @ApproveDateEnd ";
                Parameter.Add(new CommandParameter("@ApproveDateEnd", approveDateEnd));
            }

            var sqlStr = @"WITH  
                            BastTable AS(
	                            SELECT  (CaseKind+'-'+CaseKind2) AS New_CaseKind,AgentUser FROM CaseMaster C WHERE ApproveDate IS NOT NULL " + sqlWhere.Replace("ApproveDate2", "ApproveDate") + sqlWhere1 + @" 
                                UNION ALL
	                            SELECT  (CaseKind+'-'+CaseKind2) AS New_CaseKind,AgentUser2 AS AgentUser  FROM CaseMaster C WHERE  ApproveDate IS NOT NULL AND ApproveDate2 IS NOT NULL " + sqlWhere + sqlWhere2 + @" 
                            ),
                            UserByKind AS(
	                            SELECT  New_CaseKind,AgentUser,COUNT(1) AS case_num 
	                            FROM BastTable
                                GROUP BY New_CaseKind, AgentUser
                            ),
                            UserCounts AS 
                            (
	                            SELECT  AgentUser,COUNT(1) AS User_Count 
	                            FROM BastTable 
	                            GROUP BY AgentUser
                            )
                            SELECT 
	                            A.New_CaseKind,
	                            A.AgentUser,
	                            A.case_num,
	                            employee.EmpName,
	                            userCounts.User_Count
                            FROM  UserByKind AS A 
                            INNER JOIN [V_AgentAndDept] AS employee ON a.AgentUser = employee.EmpID
                            LEFT JOIN UserCounts ON a.AgentUser=userCounts.AgentUser
                            WHERE employee.SectionName=@depart
                            ORDER BY New_CaseKind, EmpID";

            DataTable dt = Search(sqlStr);
            return dt;

        }
        //品質時效統計表
        public DataTable GetTradeList(CaseClosedQuery model, string depart)
        {
            string sqlWhere = "  ";
            string sqlWhere1 = " ";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("@depart", depart));

            //類型
            if (!string.IsNullOrEmpty(model.CaseKind))
            {
                sqlWhere += @" and CaseKind like @CaseKind ";
                Parameter.Add(new CommandParameter("@CaseKind", "" + model.CaseKind.Trim() + ""));
            }
            if (!string.IsNullOrEmpty(model.CaseKind2))
            {
                sqlWhere += @" and CaseKind2 like @CaseKind2 ";
                Parameter.Add(new CommandParameter("@CaseKind2", "" + model.CaseKind2.Trim() + ""));
            }
            //收件日期
            if (!string.IsNullOrEmpty(model.ReceiveDateStart))
            {
                sqlWhere += @" AND ReceiveDate >= @ReceiveDateStart";
                Parameter.Add(new CommandParameter("@ReceiveDateStart", model.ReceiveDateStart));
            }
            if (!string.IsNullOrEmpty(model.ReceiveDateEnd))
            {
                string receiveDateEnd = Convert.ToDateTime(UtlString.FormatDateString(model.ReceiveDateEnd)).AddDays(1).ToString("yyyyMMdd");
                sqlWhere += @" AND ReceiveDate < @ReceiveDateEnd ";
                Parameter.Add(new CommandParameter("@ReceiveDateEnd", receiveDateEnd));
            }
            //發文日期
            if (!string.IsNullOrEmpty(model.SendDateStart) || !string.IsNullOrEmpty(model.SendDateEnd))
            {
                if (!string.IsNullOrEmpty(model.SendDateStart))
                {
                    sqlWhere1 += @" AND SendDate >= @SendDateStart";
                    Parameter.Add(new CommandParameter("@SendDateStart", model.SendDateStart));
                }
                if (!string.IsNullOrEmpty(model.SendDateEnd))
                {
                    string sendDateEnd = Convert.ToDateTime(UtlString.FormatDateString(model.SendDateEnd)).AddDays(1).ToString("yyyy/MM/dd");
                    sqlWhere1 += @" AND SendDate < @SendDateEnd ";
                    Parameter.Add(new CommandParameter("@SendDateEnd", sendDateEnd));
                }
                sqlWhere += " AND CaseId IN (SELECT CaseId  FROM CaseSendSetting AS CSS   where 1=1 " + sqlWhere1 + @") ";
            }
            //結案日期
            if (!string.IsNullOrEmpty(model.CloseDateStart))
            {
                sqlWhere += @" AND CloseDate >= @CloseDateStart";
                Parameter.Add(new CommandParameter("@CloseDateStart", model.CloseDateStart));
            }
            if (!string.IsNullOrEmpty(model.CloseDateEnd))
            {
                string closeDateEnd = Convert.ToDateTime(UtlString.FormatDateString(model.CloseDateEnd)).AddDays(1).ToString("yyyyMMdd");
                sqlWhere += @" AND CloseDate < @CloseDateEnd ";
                Parameter.Add(new CommandParameter("@CloseDateEnd", closeDateEnd));
            }

            var sqlStr = @"select isnull(A.CaseKind,0) as CaseKind,isnull(employee.EmpName,0) as EmpName,A.[Case],isnull(ReturnCase,0) as ReturnCase,
					   isnull((select convert(varchar(10),cast((ReturnCase)*1.0/[Case] as decimal(10,2))*100)+'%'),'0.00%') as ReturnCaseRate,
					   isnull(OutCase,0) as OutCase,isnull((select convert(varchar(10),cast((OutCase)*1.0/[Case] as decimal(10,2))*100)+'%'),'0.00%') as OutCaseRate
					   from (select CaseKind,AgentUser, count(1) as [Case] from CaseMaster WHERE 1=1  " + sqlWhere + @" group by CaseKind, AgentUser) as A
					   inner join (SELECT P.* FROM  [LDAPDepartment] d
                       inner join ldapEmployee p on P.DepDN LIKE '%'+d.depid + '%'
                       where  d.DepName in (@depart)) as employee on a.AgentUser = employee.EmpID
					   left join (select AgentUser,CaseKind,count(*) as ReturnCase from CaseMaster c where [Status] LIKE 'Z%' " + sqlWhere + @" group by AgentUser,CaseKind) t 
					   on t.AgentUser = a.AgentUser and t.CaseKind=A.CaseKind
					   left join (select AgentUser,CaseKind,count(*) as OutCase from CaseMaster c where LimitDate < CloseDate " + sqlWhere + @" group by AgentUser,CaseKind) s 
					   on s.AgentUser = a.AgentUser and s.CaseKind=A.CaseKind";
            DataTable dt = Search(sqlStr);
            return dt;
        }

        public DataTable GetTradeList1(CaseClosedQuery model, string depart)
        {
            string sqlWhere = "  ";
            string sqlWhereTmp = " ";
            string sqlWhere1 = " ";
            string sqlWhere2 = " ";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("@depart", depart));

            //類型
            if (!string.IsNullOrEmpty(model.CaseKind))
            {
                sqlWhere += @" AND B.CaseKind LIKE @CaseKind ";
                Parameter.Add(new CommandParameter("@CaseKind", "" + model.CaseKind.Trim() + ""));
            }
            if (!string.IsNullOrEmpty(model.CaseKind2))
            {
                sqlWhere += @" AND B.CaseKind2 LIKE @CaseKind2 ";
                Parameter.Add(new CommandParameter("@CaseKind2", "" + model.CaseKind2.Trim() + ""));
            }
            //收件日期
            if (!string.IsNullOrEmpty(model.ReceiveDateStart))
            {
                sqlWhere += @" AND ReceiveDate >= @ReceiveDateStart";
                Parameter.Add(new CommandParameter("@ReceiveDateStart", model.ReceiveDateStart));
            }
            if (!string.IsNullOrEmpty(model.ReceiveDateEnd))
            {
                string receiveDateEnd = Convert.ToDateTime(UtlString.FormatDateString(model.ReceiveDateEnd)).AddDays(1).ToString("yyyy/MM/dd");
                sqlWhere += @" AND ReceiveDate < @ReceiveDateEnd ";
                Parameter.Add(new CommandParameter("@ReceiveDateEnd", receiveDateEnd));
            }
            //發文日期
            if (!string.IsNullOrEmpty(model.SendDateStart))
            {
                sqlWhereTmp += @" AND SendDate >= @SendDateStart";
                Parameter.Add(new CommandParameter("@SendDateStart", model.SendDateStart));
            }
            if (!string.IsNullOrEmpty(model.SendDateEnd))
            {
                string sendDateEnd = Convert.ToDateTime(UtlString.FormatDateString(model.SendDateEnd)).AddDays(1).ToString("yyyy/MM/dd");
                sqlWhereTmp += @" AND SendDate < @SendDateEnd ";
                Parameter.Add(new CommandParameter("@SendDateEnd", sendDateEnd));
            }
			//發文方式
            if (!string.IsNullOrEmpty(model.SendKind))
            {
                sqlWhereTmp += @" AND SendKind = @SendKind ";
                Parameter.Add(new CommandParameter("@SendKind", model.SendKind));
            }
			//電子發文上傳日
            if (!string.IsNullOrEmpty(model.SendUpDateStart))
            {
                sqlWhereTmp += @" AND SendUpDate >= @SendUpDateStart";
                Parameter.Add(new CommandParameter("@SendUpDateStart", model.SendUpDateStart));
            }
            if (!string.IsNullOrEmpty(model.SendUpDateEnd))
            {
                string SendUpDateEnd = Convert.ToDateTime(UtlString.FormatDateString(model.SendUpDateEnd)).AddDays(1).ToString("yyyy/MM/dd");
                sqlWhereTmp += @" AND SendUpDate < @SendUpDateEnd ";
                Parameter.Add(new CommandParameter("@SendUpDateEnd", SendUpDateEnd));
            }
			if (!string.IsNullOrEmpty(model.SendDateStart) || !string.IsNullOrEmpty(model.SendDateEnd) || !string.IsNullOrEmpty(model.SendKind) || !string.IsNullOrEmpty(model.SendUpDateStart) || !string.IsNullOrEmpty(model.SendUpDateEnd))
			{
				sqlWhere1 += @" AND B.CaseId IN (SELECT CaseId  FROM CaseSendSetting AS CSS   where [Template] <> '支付' " + sqlWhereTmp + ") ";
				sqlWhere2 += @" AND B.CaseId IN (SELECT CaseId  FROM CaseSendSetting AS CSS   where [Template] = '支付' " + sqlWhereTmp + ") ";	
			}
            //主管放行日
            if (!string.IsNullOrEmpty(model.ApproveDateStart))
            {
                sqlWhere += @" AND ApproveDate2 >= @ApproveDateStart";
                Parameter.Add(new CommandParameter("@ApproveDateStart", model.ApproveDateStart));
            }
            //  adam 修改 yyyymmdd -> yyyy/MM/dd
            if (!string.IsNullOrEmpty(model.ApproveDateEnd))
            {
                string approveDateEnd = Convert.ToDateTime(UtlString.FormatDateString(model.ApproveDateEnd)).AddDays(1).ToString("yyyy/MM/dd");
                sqlWhere += @" AND ApproveDate2 < @ApproveDateEnd ";
                Parameter.Add(new CommandParameter("@ApproveDateEnd", approveDateEnd));
            }

            var sqlStr = @"WITH 
                            BaseMaster AS(
	                            --符合條件的基本資料Table
	                            SELECT  CaseKind,LimitDate,ApproveDate,AgentSubmitDate,AgentUser FROM  CaseMaster B WHERE ApproveDate IS NOT NULL " + sqlWhere.Replace("ApproveDate2", "ApproveDate") + sqlWhere1 + @"
	                            UNION  ALL
	                            SELECT  CaseKind,LimitDate,ApproveDate,AgentSubmitDate,AgentUser2 AS AgentUser FROM  CaseMaster B WHERE ApproveDate IS NOT NULL AND ApproveDate2 IS NOT NULL " + sqlWhere + sqlWhere2 + @"
                                 UNION ALL
	                                    SELECT B.CaseKind,B.LimitDate,B.ApproveDate,B.AgentSubmitDate,C.AgentUser AS AgentUser
	                                    FROM CaseMaster AS B
	                                    INNER JOIN DirectorReturnHistory AS C ON B.CaseId = C.CaseId
	                                    WHERE C.AfterSeizureApproved = 1 AND ApproveDate IS NOT NULL AND ApproveDate2 IS NOT NULL  " + sqlWhere + sqlWhere2 + @"
                            ),
                            TotalTable AS(
	                            --按類別和人員統計筆數
	                            SELECT  CaseKind,AgentUser,COUNT(1) AS [Case] 
	                            FROM  BaseMaster 
	                            GROUP BY CaseKind, AgentUser
                            ),
                            ReturnTable AS(
	                            --按類別和人員統計退件筆數
                                SELECT CaseKind,AgentUser,COUNT(1) AS ReturnCase
	                            FROM
	                            (
	                                SELECT DISTINCT B.CaseId, B.CaseKind,C.AgentUser 
	                                FROM CaseMaster AS B
	                                INNER JOIN DirectorReturnHistory AS C ON B.CaseId = C.CaseId
	                                WHERE C.AfterSeizureApproved = 0  AND ApproveDate IS NOT NULL" + sqlWhere.Replace("ApproveDate2", "ApproveDate") + sqlWhere1 + @"
	                                UNION  ALL
	                                SELECT DISTINCT B.CaseId, B.CaseKind,C.AgentUser AS AgentUser
	                                FROM CaseMaster AS B
	                                INNER JOIN DirectorReturnHistory AS C ON B.CaseId = C.CaseId
	                                WHERE C.AfterSeizureApproved = 1 AND ApproveDate IS NOT NULL AND ApproveDate2 IS NOT NULL  " + sqlWhere + sqlWhere2 + @"
                               ) Tmp
                               GROUP BY CaseKind,AgentUser
                            ),
                            OutTable AS(
	                            --按類別和人員統計逾期筆數
	                            SELECT AgentUser,CaseKind,COUNT(*) AS OutCase
	                            FROM  BaseMaster B 
	                            WHERE CONVERT(nvarchar(10), LimitDate,111) < CONVERT(nvarchar(10),AgentSubmitDate,111) 
	                            GROUP BY AgentUser,CaseKind
                            )
                            SELECT  
	                            isnull(A.CaseKind,0) AS CaseKind,
	                            isnull(E.EmpName,0) AS EmpName,
	                            A.[Case],
	                            isnull(ReturnCase,0) AS ReturnCase,
                                CASE A.[Case] WHEN 0 THEN '0.00%' ELSE
	                            isnull((CONVERT(VARCHAR(10),CAST((ReturnCase)*1.0/[Case] AS decimal(10,2))*100)+'%'),'0.00%') END AS ReturnCaseRate,
	                            isnull(OutCase,0) as OutCase,
                                CASE A.[Case] WHEN 0 THEN '0.00%' ELSE
	                            isnull((convert(varchar(10),cast((OutCase)*1.0/[Case] AS decimal(10,2))*100)+'%'),'0.00%') END AS OutCaseRate
                            FROM  TotalTable A 
                            LEFT OUTER JOIN [V_AgentAndDept] AS E ON A.AgentUser = E.EmpID
                            LEFT JOIN  ReturnTable t ON t.AgentUser = a.AgentUser AND t.CaseKind=A.CaseKind  
                            LEFT JOIN  OutTable s ON s.AgentUser = a.AgentUser AND s.CaseKind=A.CaseKind 
                            WHERE  E.SectionName=@depart
                            ORDER BY CaseKind,EmpID
                            ";
            DataTable dt = Search(sqlStr);
            if (dt != null && dt.Rows.Count > 0)
            {
                return dt;
            }
            else
            {
                return new DataTable();
            }
        }
        //主管放行統計表
        public DataTable GetApproveList(CaseClosedQuery model, string depart)
        {
            string sqlWhere = "";
            string sqlWhere1 = "";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("@depart", depart));

            //類型
            if (!string.IsNullOrEmpty(model.CaseKind))
            {
                sqlWhere += @" and CaseKind like @CaseKind ";
                Parameter.Add(new CommandParameter("@CaseKind", "" + model.CaseKind.Trim() + ""));
            }
            if (!string.IsNullOrEmpty(model.CaseKind2))
            {
                sqlWhere += @" and CaseKind2 like @CaseKind2 ";
                Parameter.Add(new CommandParameter("@CaseKind2", "" + model.CaseKind2.Trim() + ""));
            }
            //收件日期
            if (!string.IsNullOrEmpty(model.ReceiveDateStart))
            {
                sqlWhere += @" AND ReceiveDate >= @ReceiveDateStart";
                Parameter.Add(new CommandParameter("@ReceiveDateStart", model.ReceiveDateStart));
            }
            if (!string.IsNullOrEmpty(model.ReceiveDateEnd))
            {
                string receiveDateEnd = Convert.ToDateTime(UtlString.FormatDateString(model.ReceiveDateEnd)).AddDays(1).ToString("yyyyMMdd");
                sqlWhere += @" AND ReceiveDate < @ReceiveDateEnd ";
                Parameter.Add(new CommandParameter("@ReceiveDateEnd", receiveDateEnd));
            }
            //發文日期
            if (!string.IsNullOrEmpty(model.SendDateStart) || !string.IsNullOrEmpty(model.SendDateEnd))
            {
                if (!string.IsNullOrEmpty(model.SendDateStart))
                {
                    sqlWhere1 += @" AND SendDate >= @SendDateStart";
                    Parameter.Add(new CommandParameter("@SendDateStart", model.SendDateStart));
                }
                if (!string.IsNullOrEmpty(model.SendDateEnd))
                {
                    string sendDateEnd = Convert.ToDateTime(UtlString.FormatDateString(model.SendDateEnd)).AddDays(1).ToString("yyyyMMdd");
                    sqlWhere1 += @" AND SendDate < @SendDateEnd ";
                    Parameter.Add(new CommandParameter("@SendDateEnd", sendDateEnd));
                }
                sqlWhere += " AND CaseId IN (SELECT CaseId  FROM CaseSendSetting AS CSS where 1=1 " + sqlWhere1 + @") ";
            }
            //結案日期
            if (!string.IsNullOrEmpty(model.CloseDateStart))
            {
                sqlWhere += @" AND CloseDate >= @CloseDateStart";
                Parameter.Add(new CommandParameter("@CloseDateStart", model.CloseDateStart));
            }
            if (!string.IsNullOrEmpty(model.CloseDateEnd))
            {
                string closeDateEnd = Convert.ToDateTime(UtlString.FormatDateString(model.CloseDateEnd)).AddDays(1).ToString("yyyyMMdd");
                sqlWhere += @" AND CloseDate < @CloseDateEnd ";
                Parameter.Add(new CommandParameter("@CloseDateEnd", closeDateEnd));
            }

            var sqlStr = @"select A.*, employee.EmpName,(select count(1) from CaseMaster where ApproveUser=a.ApproveUser " + sqlWhere + @") as UserCount
                                from (select  (CaseKind+'-'+CaseKind2) as New_CaseKind,ApproveUser, count(1) as case_num from CaseMaster  
                                where 1=1 " + sqlWhere + @"
                                group by (CaseKind+'-'+CaseKind2), ApproveUser) as A 
                                inner join (SELECT P.*,n.DepName FROM  [LDAPDepartment] d
                                inner join ldapEmployee p on P.DepDN LIKE '%'+d.depid + '%'
                                inner join LDAPDepartment n on p.DepID = n.DepID
                                where  d.DepName in (@depart) and len(p.isManager) > 5) as employee on a.ApproveUser = employee.EmpID
                                order by New_CaseKind, EmpID";
            DataTable dt = Search(sqlStr);
            return dt;
        }

        public DataTable GetApproveList1(CaseClosedQuery model, string depart)
        {
            string sqlWhere = "";
            string sqlWhereTmp = "";
            string sqlWhere1 = "";
            string sqlWhere2 = "";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("@depart", depart));

            //類型
            if (!string.IsNullOrEmpty(model.CaseKind))
            {
                sqlWhere += @" AND CaseKind LIKE @CaseKind ";
                Parameter.Add(new CommandParameter("@CaseKind", "" + model.CaseKind.Trim() + ""));
            }
            if (!string.IsNullOrEmpty(model.CaseKind2))
            {
                sqlWhere += @" AND CaseKind2 LIKE @CaseKind2 ";
                Parameter.Add(new CommandParameter("@CaseKind2", "" + model.CaseKind2.Trim() + ""));
            }
            //收件日期
            if (!string.IsNullOrEmpty(model.ReceiveDateStart))
            {
                sqlWhere += @" AND ReceiveDate >= @ReceiveDateStart";
                Parameter.Add(new CommandParameter("@ReceiveDateStart", model.ReceiveDateStart));
            }
            if (!string.IsNullOrEmpty(model.ReceiveDateEnd))
            {
                string receiveDateEnd = Convert.ToDateTime(UtlString.FormatDateString(model.ReceiveDateEnd)).AddDays(1).ToString("yyyyMMdd");
                sqlWhere += @" AND ReceiveDate < @ReceiveDateEnd ";
                Parameter.Add(new CommandParameter("@ReceiveDateEnd", receiveDateEnd));
            }
            //發文日期
            if (!string.IsNullOrEmpty(model.SendDateStart))
            {
                sqlWhereTmp += @" AND SendDate >= @SendDateStart";
                Parameter.Add(new CommandParameter("@SendDateStart", model.SendDateStart));
            }
            if (!string.IsNullOrEmpty(model.SendDateEnd))
            {
                string sendDateEnd = Convert.ToDateTime(UtlString.FormatDateString(model.SendDateEnd)).AddDays(1).ToString("yyyy/MM/dd");
                sqlWhereTmp += @" AND SendDate < @SendDateEnd ";
                Parameter.Add(new CommandParameter("@SendDateEnd", sendDateEnd));
            }
			//發文方式
            if (!string.IsNullOrEmpty(model.SendKind))
            {
                sqlWhereTmp += @" AND SendKind = @SendKind ";
                Parameter.Add(new CommandParameter("@SendKind", model.SendKind));
            }
			//電子發文上傳日
            if (!string.IsNullOrEmpty(model.SendUpDateStart))
            {
                sqlWhereTmp += @" AND SendUpDate >= @SendUpDateStart";
                Parameter.Add(new CommandParameter("@SendUpDateStart", model.SendUpDateStart));
            }
            if (!string.IsNullOrEmpty(model.SendUpDateEnd))
            {
                string SendUpDateEnd = Convert.ToDateTime(UtlString.FormatDateString(model.SendUpDateEnd)).AddDays(1).ToString("yyyy/MM/dd");
                sqlWhereTmp += @" AND SendUpDate < @SendUpDateEnd ";
                Parameter.Add(new CommandParameter("@SendUpDateEnd", SendUpDateEnd));
            }
            if (!string.IsNullOrEmpty(model.SendDateStart) || !string.IsNullOrEmpty(model.SendDateEnd) || !string.IsNullOrEmpty(model.SendKind) || !string.IsNullOrEmpty(model.SendUpDateStart) || !string.IsNullOrEmpty(model.SendUpDateEnd))
			{
				sqlWhere1 += @" AND CaseId IN (SELECT CaseId  FROM CaseSendSetting AS CSS   where [Template] <> '支付' " + sqlWhereTmp + ") ";
				sqlWhere2 += @" AND CaseId IN (SELECT CaseId  FROM CaseSendSetting AS CSS   where [Template] = '支付' " + sqlWhereTmp + ") ";
			}
            //主管放行日
            if (!string.IsNullOrEmpty(model.ApproveDateStart))
            {
                sqlWhere += @" AND ApproveDate2 >= @ApproveDateStart";
                Parameter.Add(new CommandParameter("@ApproveDateStart", model.ApproveDateStart));
            }
            if (!string.IsNullOrEmpty(model.ApproveDateEnd))
            {
                string approveDateEnd = Convert.ToDateTime(UtlString.FormatDateString(model.ApproveDateEnd)).AddDays(1).ToString("yyyyMMdd");
                sqlWhere += @" AND ApproveDate2 < @ApproveDateEnd ";
                Parameter.Add(new CommandParameter("@ApproveDateEnd", approveDateEnd));
            }

            var sqlStr = @"WITH 
                            BaseTable AS(
	                            SELECT (CaseKind+'-'+CaseKind2) AS New_CaseKind,ApproveUser AS ApproveUser  FROM  CaseMaster WHERE  ApproveUser IS NOT NULL  " + sqlWhere.Replace("ApproveDate2", "ApproveDate") + sqlWhere1 + @"
	                            UNION ALL 
	                            SELECT (CaseKind+'-'+CaseKind2) AS New_CaseKind,ApproveUser2 AS ApproveUser FROM  CaseMaster WHERE  ApproveUser IS NOT NULL AND ApproveUser2 IS NOT NULL  " + sqlWhere + sqlWhere2 + @"
                            ),
                            UserByKind AS(
	                            SELECT New_CaseKind,ApproveUser,COUNT(1) AS case_num
	                            FROM BaseTable
	                            GROUP BY New_CaseKind, ApproveUser
                            ),
                            UserCount AS(
	                            SELECT ApproveUser,COUNT(1) AS UserCount 
	                            FROM BaseTable 
	                            GROUP BY ApproveUser
                            )
                            SELECT  
	                            A.*, 
	                            C.EmpName,
	                            userCount.UserCount
                            FROM  UserByKind AS A 
                            LEFT JOIN  UserCount ON A.ApproveUser=userCount.ApproveUser
                            LEFT OUTER JOIN [V_AgentAndDept] AS C ON C.EmpID = A.ApproveUser
                            WHERE C.SectionName = @depart
                            ORDER BY New_CaseKind, EmpID";
            DataTable dt = Search(sqlStr);
            return dt;
        }
        //經辦退件明細表
        public DataTable GetReturnDetailList(CaseClosedQuery model, string depart)
        {
            string sqlWhere = "";
            string sqlWhere1 = "";
            string sqlStr;
            Parameter.Clear();

            //類型
            if (!string.IsNullOrEmpty(model.CaseKind))
            {
                sqlWhere += @" and C.CaseKind like @CaseKind ";
                Parameter.Add(new CommandParameter("@CaseKind", "" + model.CaseKind.Trim() + ""));
            }
            if (!string.IsNullOrEmpty(model.CaseKind2))
            {
                sqlWhere += @" and C.CaseKind2 like @CaseKind2 ";
                Parameter.Add(new CommandParameter("@CaseKind2", "" + model.CaseKind2.Trim() + ""));
            }
            //收件日期
            if (!string.IsNullOrEmpty(model.ReceiveDateStart))
            {
                sqlWhere += @" AND C.ReceiveDate >= @ReceiveDateStart";
                Parameter.Add(new CommandParameter("@ReceiveDateStart", model.ReceiveDateStart));
            }
            if (!string.IsNullOrEmpty(model.ReceiveDateEnd))
            {
                string receiveDateEnd = Convert.ToDateTime(UtlString.FormatDateString(model.ReceiveDateEnd)).AddDays(1).ToString("yyyyMMdd");
                sqlWhere += @" AND C.ReceiveDate < @ReceiveDateEnd ";
                Parameter.Add(new CommandParameter("@ReceiveDateEnd", receiveDateEnd));
            }
            //發文日期
            if (!string.IsNullOrEmpty(model.SendDateStart) || !string.IsNullOrEmpty(model.SendDateEnd))
            {
                if (!string.IsNullOrEmpty(model.SendDateStart))
                {
                    sqlWhere1 += @" AND SendDate >= @SendDateStart";
                    Parameter.Add(new CommandParameter("@SendDateStart", model.SendDateStart));
                }
                if (!string.IsNullOrEmpty(model.SendDateEnd))
                {
                    string sendDateEnd = Convert.ToDateTime(UtlString.FormatDateString(model.SendDateEnd)).AddDays(1).ToString("yyyyMMdd");
                    sqlWhere1 += @" AND SendDate < @SendDateEnd ";
                    Parameter.Add(new CommandParameter("@SendDateEnd", sendDateEnd));
                }
                sqlWhere += " AND C.CaseId IN (SELECT CaseId  FROM CaseSendSetting AS CSS where 1=1 " + sqlWhere1 + @") ";
            }
            //結案日期
            if (!string.IsNullOrEmpty(model.CloseDateStart))
            {
                sqlWhere += @" AND C.CloseDate >= @CloseDateStart";
                Parameter.Add(new CommandParameter("@CloseDateStart", model.CloseDateStart));
            }
            if (!string.IsNullOrEmpty(model.CloseDateEnd))
            {
                string closeDateEnd = Convert.ToDateTime(UtlString.FormatDateString(model.CloseDateEnd)).AddDays(1).ToString("yyyyMMdd");
                sqlWhere += @" AND C.CloseDate < @CloseDateEnd ";
                Parameter.Add(new CommandParameter("@CloseDateEnd", closeDateEnd));
            }
            //* 20150708 經辦退件統計表 不是統計經辦退給集作的(C06). 而是主管推退給經辦的(C04)
            //* 另外只要有過的都算.如果單純判斷status有可能會被蓋掉
            //            sqlStr = @"select CaseKind,CaseNo,employee.EmpName,ReturnReason from CaseMaster c							  							 
            //							  inner join (SELECT P.* FROM  [LDAPDepartment] d
            //                              inner join ldapEmployee p on P.DepDN LIKE '%'+d.depid + '%'
            //                              where  d.DepName = @depart) as employee on c.AgentUser = employee.EmpID
            //							  where [Status] =@Status " + sqlWhere;

            sqlStr = @"SELECT 
                        DISTINCT C.CaseKind,C.CaseNo,E.EmpName,C.CaseId
                        FROM [CaseMaster] AS C
                        INNER JOIN [V_AgentAndDept] AS E ON C.AgentUser = E.EmpID
                        INNER JOIN [CaseHistory] AS H ON C.CaseId = H.CaseId AND H.[Event] = '主管退件'
                        where E.SectionName = @depart " + sqlWhere;

            Parameter.Add(new CommandParameter("@depart", depart));
            Parameter.Add(new CommandParameter("Status", CaseStatus.AgentReturnClose));
            DataTable dt = base.Search(sqlStr);
            dt.Columns.Add("ReturnReason").SetOrdinal(3);
            string strCaseID = string.Empty;
            if (dt != null && dt.Rows.Count > 0)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    strCaseID += "'" + dr["CaseId"].ToString() + "',";
                }
                strCaseID = strCaseID.TrimEnd(',');
                string sql = " SELECT ReturnReason,CASEID FROM DirectorReturnHistory WHERE CASEID  IN ( " + strCaseID + ")";
                List<CaseReturn> list = base.SearchList<CaseReturn>(sql).ToList();
                if (list != null && list.Any())
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        string strReason = string.Empty;
                        foreach (CaseReturn item in list.Where(m => m.CaseId.ToString() == dr["CASEID"].ToString()))
                        {
                            strReason += item.ReturnReason + "、";
                        }
                        strReason = strReason.TrimEnd('、');
                        dr["ReturnReason"] = strReason;
                    }
                }
            }
            return dt;
        }
        public DataTable GetReturnDetailList1(CaseClosedQuery model, string depart)
        {
            string sqlWhere = "";
            string sqlWhereTmp = "";
            string sqlWhere1 = "";
            string sqlWhere2 = "";
            string sqlStr;
            Parameter.Clear();
            Parameter.Add(new CommandParameter("@depart", depart));

            //類型
            if (!string.IsNullOrEmpty(model.CaseKind))
            {
                sqlWhere += @" AND B.CaseKind LIKE @CaseKind ";
                Parameter.Add(new CommandParameter("@CaseKind", "" + model.CaseKind.Trim() + ""));
            }
            if (!string.IsNullOrEmpty(model.CaseKind2))
            {
                sqlWhere += @" AND B.CaseKind2 LIKE @CaseKind2 ";
                Parameter.Add(new CommandParameter("@CaseKind2", "" + model.CaseKind2.Trim() + ""));
            }
            //收件日期
            if (!string.IsNullOrEmpty(model.ReceiveDateStart))
            {
                sqlWhere += @" AND B.ReceiveDate >= @ReceiveDateStart";
                Parameter.Add(new CommandParameter("@ReceiveDateStart", model.ReceiveDateStart));
            }
            if (!string.IsNullOrEmpty(model.ReceiveDateEnd))
            {
                string receiveDateEnd = Convert.ToDateTime(UtlString.FormatDateString(model.ReceiveDateEnd)).AddDays(1).ToString("yyyyMMdd");
                sqlWhere += @" AND B.ReceiveDate < @ReceiveDateEnd ";
                Parameter.Add(new CommandParameter("@ReceiveDateEnd", receiveDateEnd));
            }
            //發文日期
            if (!string.IsNullOrEmpty(model.SendDateStart))
            {
                sqlWhereTmp += @" AND SendDate >= @SendDateStart";
                Parameter.Add(new CommandParameter("@SendDateStart", model.SendDateStart));
            }
            if (!string.IsNullOrEmpty(model.SendDateEnd))
            {
                string sendDateEnd = Convert.ToDateTime(UtlString.FormatDateString(model.SendDateEnd)).AddDays(1).ToString("yyyy/MM/dd");
                sqlWhereTmp += @" AND SendDate < @SendDateEnd ";
                Parameter.Add(new CommandParameter("@SendDateEnd", sendDateEnd));
            }
			//發文方式
            if (!string.IsNullOrEmpty(model.SendKind))
            {
                sqlWhereTmp += @" AND SendKind = @SendKind ";
                Parameter.Add(new CommandParameter("@SendKind", model.SendKind));
            }
			//電子發文上傳日
            if (!string.IsNullOrEmpty(model.SendUpDateStart))
            {
                sqlWhereTmp += @" AND SendUpDate >= @SendUpDateStart";
                Parameter.Add(new CommandParameter("@SendUpDateStart", model.SendUpDateStart));
            }
            if (!string.IsNullOrEmpty(model.SendUpDateEnd))
            {
                string SendUpDateEnd = Convert.ToDateTime(UtlString.FormatDateString(model.SendUpDateEnd)).AddDays(1).ToString("yyyy/MM/dd");
                sqlWhereTmp += @" AND SendUpDate < @SendUpDateEnd ";
                Parameter.Add(new CommandParameter("@SendUpDateEnd", SendUpDateEnd));
            }
			if (!string.IsNullOrEmpty(model.SendDateStart) || !string.IsNullOrEmpty(model.SendDateEnd) || !string.IsNullOrEmpty(model.SendKind) || !string.IsNullOrEmpty(model.SendUpDateStart) || !string.IsNullOrEmpty(model.SendUpDateEnd))
			{
				sqlWhere1 += @" AND A.CaseId IN (SELECT CaseId  FROM CaseSendSetting AS CSS   where [Template] <> '支付' " + sqlWhereTmp + ") ";
				sqlWhere2 += @" AND A.CaseId IN (SELECT CaseId  FROM CaseSendSetting AS CSS   where [Template] = '支付' " + sqlWhereTmp + ") ";
			}
            //主管放行日
            if (!string.IsNullOrEmpty(model.ApproveDateStart))
            {
                sqlWhere += @" AND ApproveDate2 >= @ApproveDateStart";
                Parameter.Add(new CommandParameter("@ApproveDateStart", model.ApproveDateStart));
            }
            if (!string.IsNullOrEmpty(model.ApproveDateEnd))
            {
                string approveDateEnd = Convert.ToDateTime(UtlString.FormatDateString(model.ApproveDateEnd)).AddDays(1).ToString("yyyyMMdd");
                sqlWhere += @" AND ApproveDate2 < @ApproveDateEnd ";
                Parameter.Add(new CommandParameter("@ApproveDateEnd", approveDateEnd));
            }

            sqlStr = @"SELECT
                            C.CaseId,
                            C.CaseKind,
                            C.CaseNo,
                            E.EmpName,
                            C.AgentUser
                        FROM 
                        (
	                        SELECT DISTINCT
		                        A.CaseId,
		                        B.CaseKind,
		                        B.CaseNo,
		                        A.AgentUser AS AgentUser
	                        FROM 
	                        DirectorReturnHistory AS A
	                        INNER JOIN CaseMaster AS B ON A.CaseId = B.CaseId 
	                        WHERE A.AfterSeizureApproved = 0  AND ApproveDate IS NOT NULL  " + sqlWhere.Replace("ApproveDate2", "ApproveDate") + sqlWhere1 + @"
	                        UNION
	                        SELECT DISTINCT
		                        A.CaseId,
		                        B.CaseKind,
		                        B.CaseNo,
		                        A.AgentUser AS AgentUser
	                        FROM 
	                        DirectorReturnHistory AS A
	                        INNER JOIN CaseMaster AS B ON A.CaseId = B.CaseId 
	                        WHERE A.AfterSeizureApproved = 1  AND ApproveDate IS NOT NULL AND ApproveDate2 IS NOT NULL " + sqlWhere + sqlWhere2 + @"
                        ) AS C 
                        LEFT OUTER JOIN [V_AgentAndDept] AS E ON C.AgentUser = E.EmpID
                        WHERE E.SectionName = @depart 
                        ORDER BY  CaseNo ASC;";

            DataTable dt = base.Search(sqlStr);
            dt.Columns.Add("ReturnReason").SetOrdinal(4);
            if (dt != null && dt.Rows.Count > 0)
            {
                string strCaseId = string.Empty;
                foreach (DataRow dr in dt.Rows)
                {
                    strCaseId += "'" + Convert.ToString(dr["CaseId"]) + "',";
                }
                strCaseId = strCaseId.TrimEnd(',');
                sqlStr = @" SELECT 
	                            A.CaseId,	
	                            A.AgentUser,
	                            A.ReturnReason
                            FROM 
                            DirectorReturnHistory AS A
                            WHERE A.CaseId IN ( " + strCaseId + ")";
                List<CaseMaster> dtReason = base.SearchList<CaseMaster>(sqlStr).ToList();
                if (dtReason != null && dtReason.Any())
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        string strReason = string.Empty;
                        foreach (CaseMaster item in dtReason.Where(item => item.CaseId.ToString() == Convert.ToString(dr["CaseId"]) && item.AgentUser == dr["AgentUser"].ToString()))
                        {
                            strReason += item.ReturnReason + "、";
                        }
                        strReason = strReason.TrimEnd('、');
                        dr["ReturnReason"] = strReason;
                    }
                }
            }
            return dt;
        }

        //逾期案件明細表
        public DataTable GetOverDateList(CaseClosedQuery model, string depart)
        {
            string sqlWhere = "";
            string sqlWhere1 = "";
            string sqlStr;
            Parameter.Clear();
            Parameter.Add(new CommandParameter("@depart", depart));

            //類型
            if (!string.IsNullOrEmpty(model.CaseKind))
            {
                sqlWhere += @" and CaseKind like @CaseKind ";
                Parameter.Add(new CommandParameter("@CaseKind", "" + model.CaseKind.Trim() + ""));
            }
            if (!string.IsNullOrEmpty(model.CaseKind2))
            {
                sqlWhere += @" and CaseKind2 like @CaseKind2 ";
                Parameter.Add(new CommandParameter("@CaseKind2", "" + model.CaseKind2.Trim() + ""));
            }
            //收件日期
            if (!string.IsNullOrEmpty(model.ReceiveDateStart))
            {
                sqlWhere += @" AND ReceiveDate >= @ReceiveDateStart";
                Parameter.Add(new CommandParameter("@ReceiveDateStart", model.ReceiveDateStart));
            }
            if (!string.IsNullOrEmpty(model.ReceiveDateEnd))
            {
                string receiveDateEnd = Convert.ToDateTime(UtlString.FormatDateString(model.ReceiveDateEnd)).AddDays(1).ToString("yyyyMMdd");
                sqlWhere += @" AND ReceiveDate < @ReceiveDateEnd ";
                Parameter.Add(new CommandParameter("@ReceiveDateEnd", receiveDateEnd));
            }
            //發文日期
            if (!string.IsNullOrEmpty(model.SendDateStart) || !string.IsNullOrEmpty(model.SendDateEnd))
            {
                if (!string.IsNullOrEmpty(model.SendDateStart))
                {
                    sqlWhere1 += @" AND SendDate >= @SendDateStart";
                    Parameter.Add(new CommandParameter("@SendDateStart", model.SendDateStart));
                }
                if (!string.IsNullOrEmpty(model.SendDateEnd))
                {
                    string sendDateEnd = Convert.ToDateTime(UtlString.FormatDateString(model.SendDateEnd)).AddDays(1).ToString("yyyyMMdd");
                    sqlWhere1 += @" AND SendDate < @SendDateEnd ";
                    Parameter.Add(new CommandParameter("@SendDateEnd", sendDateEnd));
                }
                sqlWhere += " AND CaseId IN (SELECT CaseId  FROM CaseSendSetting AS CSS where 1=1 " + sqlWhere1 + @") ";
            }
            //結案日期
            if (!string.IsNullOrEmpty(model.CloseDateStart))
            {
                sqlWhere += @" AND CloseDate >= @CloseDateStart";
                Parameter.Add(new CommandParameter("@CloseDateStart", model.CloseDateStart));
            }
            if (!string.IsNullOrEmpty(model.CloseDateEnd))
            {
                string closeDateEnd = Convert.ToDateTime(UtlString.FormatDateString(model.CloseDateEnd)).AddDays(1).ToString("yyyyMMdd");
                sqlWhere += @" AND CloseDate < @CloseDateEnd ";
                Parameter.Add(new CommandParameter("@CloseDateEnd", closeDateEnd));
            }

            sqlStr = @"select CaseKind,CaseNo,employee.EmpName,
                                OverDueMemo,
                               CloseDate,c.CreatedDate,CaseKind from CaseMaster c
							  inner join (SELECT P.* FROM  [LDAPDepartment] d
                              inner join ldapEmployee p on P.DepDN LIKE '%'+d.depid + '%'
                              where  d.DepName in (@depart)) as employee on c.AgentUser = employee.EmpID
							  where CloseDate > LimitDate" + sqlWhere + "";
            DataTable dt = base.Search(sqlStr);
            dt.Columns.Add("OverDay").SetOrdinal(3);
            if (dt != null && dt.Rows.Count > 0)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    if (dr["CaseKind"].ToString() == "外來文案件")
                    {
                        string sqls = "SELECT COUNT(1) AS OverDate FROM [PARMWorkingDay] WHERE FLAG = 1 AND [DATE] > '" + DateTime.Parse(dr["CreatedDate"].ToString()).ToString("yyyy/MM/dd") + "' AND [DATE] <= '" + DateTime.Parse(dr["CloseDate"].ToString()).ToString("yyyy/MM/dd")+"'";
                        dr["OverDay"] = (int)base.ExecuteScalar(sqls);
                    }
                    else
                    {
                        dr["OverDay"] = new TimeSpan(Convert.ToDateTime(dr["CloseDate"]).Ticks - Convert.ToDateTime(dr["CreatedDate"]).Ticks).Days;
                    }
                }
            }
            return dt;
        }
        public DataTable GetOverDateList1(CaseClosedQuery model, string depart)
        {
            string sqlWhere = "";
            string sqlWhere1 = "";
            string sqlStr;
            Parameter.Clear();
            Parameter.Add(new CommandParameter("@depart", depart));

            //類型
            if (!string.IsNullOrEmpty(model.CaseKind))
            {
                sqlWhere += @" AND CaseKind LIKE @CaseKind ";
                Parameter.Add(new CommandParameter("@CaseKind", "" + model.CaseKind.Trim() + ""));
            }
            if (!string.IsNullOrEmpty(model.CaseKind2))
            {
                sqlWhere += @" AND CaseKind2 LIKE @CaseKind2 ";
                Parameter.Add(new CommandParameter("@CaseKind2", "" + model.CaseKind2.Trim() + ""));
            }
            //收件日期
            if (!string.IsNullOrEmpty(model.ReceiveDateStart))
            {
                sqlWhere += @" AND ReceiveDate >= @ReceiveDateStart";
                Parameter.Add(new CommandParameter("@ReceiveDateStart", model.ReceiveDateStart));
            }
            if (!string.IsNullOrEmpty(model.ReceiveDateEnd))
            {
                string receiveDateEnd = Convert.ToDateTime(UtlString.FormatDateString(model.ReceiveDateEnd)).AddDays(1).ToString("yyyyMMdd");
                sqlWhere += @" AND ReceiveDate < @ReceiveDateEnd ";
                Parameter.Add(new CommandParameter("@ReceiveDateEnd", receiveDateEnd));
            }
            //發文日期
            if (!string.IsNullOrEmpty(model.SendDateStart))
            {
                sqlWhere1 += @" AND SendDate >= @SendDateStart";
                Parameter.Add(new CommandParameter("@SendDateStart", model.SendDateStart));
            }
            if (!string.IsNullOrEmpty(model.SendDateEnd))
            {
                string sendDateEnd = Convert.ToDateTime(UtlString.FormatDateString(model.SendDateEnd)).AddDays(1).ToString("yyyy/MM/dd");
                sqlWhere1 += @" AND SendDate < @SendDateEnd ";
                Parameter.Add(new CommandParameter("@SendDateEnd", sendDateEnd));
            }
			//發文方式
            if (!string.IsNullOrEmpty(model.SendKind))
            {
                sqlWhere1 += @" AND SendKind = @SendKind ";
                Parameter.Add(new CommandParameter("@SendKind", model.SendKind));
            }
			//電子發文上傳日
            if (!string.IsNullOrEmpty(model.SendUpDateStart))
            {
                sqlWhere1 += @" AND SendUpDate >= @SendUpDateStart";
                Parameter.Add(new CommandParameter("@SendUpDateStart", model.SendUpDateStart));
            }
            if (!string.IsNullOrEmpty(model.SendUpDateEnd))
            {
                string SendUpDateEnd = Convert.ToDateTime(UtlString.FormatDateString(model.SendUpDateEnd)).AddDays(1).ToString("yyyy/MM/dd");
                sqlWhere1 += @" AND SendUpDate < @SendUpDateEnd ";
                Parameter.Add(new CommandParameter("@SendUpDateEnd", SendUpDateEnd));
            }
			if (!string.IsNullOrEmpty(model.SendDateStart) || !string.IsNullOrEmpty(model.SendDateEnd) || !string.IsNullOrEmpty(model.SendKind) || !string.IsNullOrEmpty(model.SendUpDateStart) || !string.IsNullOrEmpty(model.SendUpDateEnd))
			{
				sqlWhere += @" AND CaseId IN (SELECT CaseId  FROM CaseSendSetting AS CSS   where 1=1 " + sqlWhere1 + ") ";
			}
            //主管放行日
            if (!string.IsNullOrEmpty(model.ApproveDateStart))
            {
                sqlWhere += @" AND ApproveDate >= @ApproveDateStart";
                Parameter.Add(new CommandParameter("@ApproveDateStart", model.ApproveDateStart));
            }
            if (!string.IsNullOrEmpty(model.ApproveDateEnd))
            {
                string approveDateEnd = Convert.ToDateTime(UtlString.FormatDateString(model.ApproveDateEnd)).AddDays(1).ToString("yyyyMMdd");
                sqlWhere += @" AND ApproveDate < @ApproveDateEnd ";
                Parameter.Add(new CommandParameter("@ApproveDateEnd", approveDateEnd));
            }

            sqlStr = @"SELECT  
                            CaseKind,
                            CaseNo,
                            v.EmpName,
                            OverDueMemo,
                            AgentSubmitDate,c.CreatedDate
                        FROM CaseMaster c
                        INNER JOIN  [V_AgentAndDept] as v ON c.AgentUser = v.EmpID
                        WHERE 
                        V.SectionName = @depart
                        AND CONVERT(nvarchar(10), LimitDate,111) < CONVERT(nvarchar(10),AgentSubmitDate,111) 
                        AND ApproveDate IS NOT NULL " + sqlWhere + @"
                        ORDER BY CaseNo ASC";
            DataTable dt = base.Search(sqlStr);
            dt.Columns.Add("OverDay").SetOrdinal(3);
            if (dt != null && dt.Rows.Count > 0)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    if (dr["CaseKind"].ToString() == "外來文案件")
                    {
                        string sqls = "SELECT COUNT(1) AS OverDate FROM [PARMWorkingDay] WHERE FLAG = 1 AND [DATE] > '" + DateTime.Parse(dr["CreatedDate"].ToString()).ToString("yyyy/MM/dd") + "' AND [DATE] <= '" + DateTime.Parse(dr["AgentSubmitDate"].ToString()).ToString("yyyy/MM/dd") + "'";
                        dr["OverDay"] = (int)base.ExecuteScalar(sqls);
                    }
                    else
                    {
                        DateTime e = Convert.ToDateTime(dr["AgentSubmitDate"].ToString());
                        DateTime s=Convert.ToDateTime(dr["CreatedDate"].ToString());
                        dr["OverDay"] = e.Subtract(s).Days;
                    }
                }
            }
            return dt;
        }

        #region 用印簿
        public MemoryStream ListReportExcel_NPOI(CaseClosedQuery model)
        {
            IWorkbook workbook = new HSSFWorkbook();
            ISheet sheet = null;
            ISheet sheet2 = null;
            ISheet sheet3 = null;

            DataTable dt = new DataTable();
            DataTable dt2 = new DataTable();
            DataTable dt3 = new DataTable();
            DataTable dtcount = new DataTable();
            DataTable dtcount2 = new DataTable();
            DataTable dtcount3 = new DataTable();

            #region def style
            ICellStyle styleHead12 = workbook.CreateCellStyle();
            IFont font12 = workbook.CreateFont();
            font12.FontHeightInPoints = 12;
            font12.FontName = "新細明體";
            styleHead12.FillPattern = FillPattern.SolidForeground;
            styleHead12.FillForegroundColor = HSSFColor.White.Index;
            styleHead12.BorderTop = BorderStyle.None;
            styleHead12.BorderLeft = BorderStyle.None;
            styleHead12.BorderRight = BorderStyle.None;
            styleHead12.BorderBottom = BorderStyle.None;
            styleHead12.WrapText = true;
            styleHead12.Alignment = HorizontalAlignment.Center;
            styleHead12.VerticalAlignment = VerticalAlignment.Center;
            styleHead12.SetFont(font12);

            ICellStyle styleHead10 = workbook.CreateCellStyle();
            IFont font10 = workbook.CreateFont();
            font10.FontHeightInPoints = 10;
            font10.FontName = "新細明體";
            styleHead10.FillPattern = FillPattern.SolidForeground;
            styleHead10.FillForegroundColor = HSSFColor.White.Index;
            styleHead10.BorderTop = BorderStyle.Thin;
            styleHead10.BorderLeft = BorderStyle.Thin;
            styleHead10.BorderRight = BorderStyle.Thin;
            styleHead10.BorderBottom = BorderStyle.Thin;
            styleHead10.WrapText = true;
            styleHead10.Alignment = HorizontalAlignment.Left;
            styleHead10.VerticalAlignment = VerticalAlignment.Center;
            styleHead10.SetFont(font10);
            #endregion

            #region 獲取數據源(集作一科及案件資料)
            //獲取人員
            if (model.Depart == "1")//* 集作一科
            {
                sheet = workbook.CreateSheet("集作一科");
                dt = GetList(model, "集作一科");//獲取查詢集作一科的案件
                SetExcelCell(sheet, 1, 8, styleHead12, "集作一科");
                sheet.AddMergedRegion(new CellRangeAddress(1, 1, 8, 8));
                dtcount = GetCountList(model, "集作一科");
            }
            if (model.Depart == "2")//* 集作二科
            {
                sheet = workbook.CreateSheet("集作二科");
                dt = GetList(model, "集作二科");//獲取查詢集作二科的案件
                SetExcelCell(sheet, 1, 8, styleHead12, "集作二科");
                sheet.AddMergedRegion(new CellRangeAddress(1, 1, 8, 8));
                dtcount = GetCountList(model, "集作二科");
            }
            if (model.Depart == "3")//*集作三科
            {
                sheet = workbook.CreateSheet("集作三科");
                dt = GetList(model, "集作三科");//獲取查詢集作三科的案件
                SetExcelCell(sheet, 1, 8, styleHead12, "集作三科");
                sheet.AddMergedRegion(new CellRangeAddress(1, 1, 8, 8));
                dtcount = GetCountList(model, "集作三科");
            }
            if (model.Depart == "0")//*全部
            {
                sheet = workbook.CreateSheet("集作一科");
                sheet2 = workbook.CreateSheet("集作二科");
                sheet3 = workbook.CreateSheet("集作三科");
                dt = GetList(model, "集作一科");//獲取查詢集作一科的案件
                dt2 = GetList(model, "集作二科");//獲取查詢集作二科的案件
                dt3 = GetList(model, "集作三科");//獲取查詢集作三科的案件
                SetExcelCell(sheet, 1, 8, styleHead12, "集作一科");
                sheet.AddMergedRegion(new CellRangeAddress(1, 1, 8, 8));
                SetExcelCell(sheet2, 1, 8, styleHead12, "集作二科");
                sheet2.AddMergedRegion(new CellRangeAddress(1, 1, 8, 8));
                SetExcelCell(sheet3, 1, 8, styleHead12, "集作三科");
                sheet3.AddMergedRegion(new CellRangeAddress(1, 1, 8, 8));
                dtcount = GetCountList(model, "集作一科");
                dtcount2 = GetCountList(model, "集作二科");
                dtcount3 = GetCountList(model, "集作三科");
            }
            #endregion

            #region title
            SetExcelCell(sheet, 0, 0, styleHead12, "用印簿");
            sheet.AddMergedRegion(new CellRangeAddress(0, 0, 0, 8));

            //* line1
            SetExcelCell(sheet, 1, 0, styleHead12, "收件日期：");
            sheet.AddMergedRegion(new CellRangeAddress(1, 1, 0, 0));
            SetExcelCell(sheet, 1, 1, styleHead12, model.ReceiveDateStart + '~' + model.ReceiveDateEnd);
            sheet.AddMergedRegion(new CellRangeAddress(1, 1, 1, 1));
            SetExcelCell(sheet, 1, 2, styleHead12, "發文日期：");
            sheet.AddMergedRegion(new CellRangeAddress(1, 1, 2, 2));
            SetExcelCell(sheet, 1, 3, styleHead12, model.SendDateStart + '~' + model.SendDateEnd);
            sheet.AddMergedRegion(new CellRangeAddress(1, 1, 3, 3));
            SetExcelCell(sheet, 1, 4, styleHead12, "結案日期：");
            sheet.AddMergedRegion(new CellRangeAddress(1, 1, 4, 4));
            SetExcelCell(sheet, 1, 5, styleHead12, model.CloseDateStart + '~' + model.CloseDateEnd);
            sheet.AddMergedRegion(new CellRangeAddress(1, 1, 5, 6));
            SetExcelCell(sheet, 1, 7, styleHead12, "部門別/科別：");
            sheet.AddMergedRegion(new CellRangeAddress(1, 1, 7, 7));

            //* line2
            SetExcelCell(sheet, 2, 0, styleHead10, "發文字號");
            sheet.AddMergedRegion(new CellRangeAddress(2, 2, 0, 0));
            SetExcelCell(sheet, 2, 1, styleHead10, "案件編號");
            sheet.AddMergedRegion(new CellRangeAddress(2, 2, 1, 1));
            SetExcelCell(sheet, 2, 2, styleHead10, "受文者");
            sheet.AddMergedRegion(new CellRangeAddress(2, 2, 2, 2));
            SetExcelCell(sheet, 2, 3, styleHead10, "副本");
            sheet.AddMergedRegion(new CellRangeAddress(2, 2, 3, 3));
            SetExcelCell(sheet, 2, 4, styleHead10, "經辦");
            sheet.AddMergedRegion(new CellRangeAddress(2, 2, 4, 4));
            SetExcelCell(sheet, 2, 5, styleHead10, "案件大類");
            sheet.AddMergedRegion(new CellRangeAddress(2, 2, 5, 5));
            SetExcelCell(sheet, 2, 6, styleHead10, "案件細類");
            sheet.AddMergedRegion(new CellRangeAddress(2, 2, 6, 6));
            SetExcelCell(sheet, 2, 7, styleHead10, "放行主管");
            sheet.AddMergedRegion(new CellRangeAddress(2, 2, 7, 7));
            SetExcelCell(sheet, 2, 8, styleHead10, "逾期註記");
            sheet.AddMergedRegion(new CellRangeAddress(2, 2, 8, 8));

            SetExcelCell(sheet, dt.Rows.Count + 4, 0, styleHead10, "案件類別");
            sheet.AddMergedRegion(new CellRangeAddress(dt.Rows.Count + 4, dt.Rows.Count + 4, 0, 0));
            SetExcelCell(sheet, dt.Rows.Count + 4, 1, styleHead10, "扣押");
            sheet.AddMergedRegion(new CellRangeAddress(dt.Rows.Count + 4, dt.Rows.Count + 4, 1, 1));
            SetExcelCell(sheet, dt.Rows.Count + 4, 2, styleHead10, "支付");
            sheet.AddMergedRegion(new CellRangeAddress(dt.Rows.Count + 4, dt.Rows.Count + 4, 2, 2));
            SetExcelCell(sheet, dt.Rows.Count + 4, 3, styleHead10, "扣押並支付");
            sheet.AddMergedRegion(new CellRangeAddress(dt.Rows.Count + 4, dt.Rows.Count + 4, 3, 3));
            SetExcelCell(sheet, dt.Rows.Count + 4, 4, styleHead10, "外來文案件");
            sheet.AddMergedRegion(new CellRangeAddress(dt.Rows.Count + 4, dt.Rows.Count + 4, 4, 4));
            SetExcelCell(sheet, dt.Rows.Count + 4, 5, styleHead10, "合計");
            sheet.AddMergedRegion(new CellRangeAddress(dt.Rows.Count + 4, dt.Rows.Count + 4, 5, 5));

            SetExcelCell(sheet, dt.Rows.Count + 5, 0, styleHead10, "件數");
            sheet.AddMergedRegion(new CellRangeAddress(dt.Rows.Count + 5, dt.Rows.Count + 5, 0, 0));

            SetExcelCell(sheet, dt.Rows.Count + 7, 4, styleHead12, "覆核主管");
            sheet.AddMergedRegion(new CellRangeAddress(dt.Rows.Count + 7, dt.Rows.Count + 7, 4, 4));
            SetExcelCell(sheet, dt.Rows.Count + 7, 6, styleHead12, "覆核人員");
            sheet.AddMergedRegion(new CellRangeAddress(dt.Rows.Count + 7, dt.Rows.Count + 7, 6, 6));

            for (int i = dt.Rows.Count + 4; i < dt.Rows.Count + 5; i++)//*初始表格賦初值 
            {
                for (int j = 0; j < 5; j++)
                {
                    SetExcelCell(sheet, i + 1, j + 1, styleHead10, "0");
                    sheet.AddMergedRegion(new CellRangeAddress(i + 1, i + 1, j + 1, j + 1));
                }
            }
            #endregion
            #region Width
            sheet.SetColumnWidth(0, 100 * 100);
            sheet.SetColumnWidth(1, 100 * 40);
            sheet.SetColumnWidth(2, 100 * 100);
            sheet.SetColumnWidth(3, 100 * 100);
            sheet.SetColumnWidth(4, 100 * 30);
            sheet.SetColumnWidth(5, 100 * 30);
            sheet.SetColumnWidth(6, 100 * 30);
            sheet.SetColumnWidth(7, 100 * 40);
            sheet.SetColumnWidth(8, 100 * 30);
            #endregion
            #region body

            for (int iRow = 0; iRow < dt.Rows.Count; iRow++)
            {
                for (int iCol = 0; iCol < dt.Columns.Count - 1; iCol++)
                {
                    SetExcelCell(sheet, iRow + 3, iCol, styleHead10, dt.Rows[iRow][iCol].ToString());
                }
            }

            int rows = dt.Rows.Count + 5;
            int Count = 0;
            int CountNum = 0;
            for (int i = 0; i < dtcount.Rows.Count; i++)//*初始表格賦初值 
            {
                if (Convert.ToString(dtcount.Rows[i]["CaseKind2"]) == "扣押")
                {
                    SetExcelCell(sheet, rows, 1, styleHead10, Convert.ToString(dtcount.Rows[i]["C"]));
                }
                if (Convert.ToString(dtcount.Rows[i]["CaseKind2"]) == "支付")
                {
                    SetExcelCell(sheet, rows, 2, styleHead10, Convert.ToString(dtcount.Rows[i]["C"]));
                }
                if (Convert.ToString(dtcount.Rows[i]["CaseKind2"]) == "扣押並支付")
                {
                    SetExcelCell(sheet, rows, 3, styleHead10, Convert.ToString(dtcount.Rows[i]["C"]));
                }
                if (Convert.ToString(dtcount.Rows[i]["CaseKind2"]) == "外來文案件")
                {
                    SetExcelCell(sheet, rows, 4, styleHead10, Convert.ToString(dtcount.Rows[i]["C"]));
                }
                Count = Convert.ToInt32(dtcount.Rows[i]["C"].ToString());
                CountNum += Count;
            }
            SetExcelCell(sheet, rows, 5, styleHead10, CountNum.ToString());
            #endregion

            if (model.Depart == "0")//* 全部
            {
                #region title2
                SetExcelCell(sheet2, 0, 0, styleHead12, "用印簿");
                sheet2.AddMergedRegion(new CellRangeAddress(0, 0, 0, 8));

                //* line1
                SetExcelCell(sheet2, 1, 0, styleHead12, "收件日期：");
                sheet2.AddMergedRegion(new CellRangeAddress(1, 1, 0, 0));
                SetExcelCell(sheet2, 1, 1, styleHead12, model.ReceiveDateStart + '~' + model.ReceiveDateEnd);
                sheet2.AddMergedRegion(new CellRangeAddress(1, 1, 1, 1));
                SetExcelCell(sheet2, 1, 2, styleHead12, "發文日期：");
                sheet2.AddMergedRegion(new CellRangeAddress(1, 1, 2, 2));
                SetExcelCell(sheet2, 1, 3, styleHead12, model.SendDateStart + '~' + model.SendDateEnd);
                sheet2.AddMergedRegion(new CellRangeAddress(1, 1, 3, 3));
                SetExcelCell(sheet2, 1, 4, styleHead12, "結案日期：");
                sheet2.AddMergedRegion(new CellRangeAddress(1, 1, 4, 4));
                SetExcelCell(sheet2, 1, 5, styleHead12, model.CloseDateStart + '~' + model.CloseDateEnd);
                sheet2.AddMergedRegion(new CellRangeAddress(1, 1, 5, 5));
                SetExcelCell(sheet2, 1, 7, styleHead12, "部門別/科別：");
                sheet2.AddMergedRegion(new CellRangeAddress(1, 1, 7, 7));


                //* line2
                SetExcelCell(sheet2, 2, 0, styleHead10, "發文字號");
                sheet2.AddMergedRegion(new CellRangeAddress(2, 2, 0, 0));
                SetExcelCell(sheet2, 2, 1, styleHead10, "案件編號");
                sheet2.AddMergedRegion(new CellRangeAddress(2, 2, 1, 1));
                SetExcelCell(sheet2, 2, 2, styleHead10, "受文者");
                sheet2.AddMergedRegion(new CellRangeAddress(2, 2, 2, 2));
                SetExcelCell(sheet2, 2, 3, styleHead10, "副本");
                sheet2.AddMergedRegion(new CellRangeAddress(2, 2, 3, 3));
                SetExcelCell(sheet2, 2, 4, styleHead10, "經辦");
                sheet2.AddMergedRegion(new CellRangeAddress(2, 2, 4, 4));
                SetExcelCell(sheet2, 2, 5, styleHead10, "案件大類");
                sheet2.AddMergedRegion(new CellRangeAddress(2, 2, 5, 5));
                SetExcelCell(sheet2, 2, 6, styleHead10, "案件細類");
                sheet2.AddMergedRegion(new CellRangeAddress(2, 2, 6, 6));
                SetExcelCell(sheet2, 2, 7, styleHead10, "放行主管");
                sheet2.AddMergedRegion(new CellRangeAddress(2, 2, 7, 7));
                SetExcelCell(sheet2, 2, 8, styleHead10, "逾期註記");
                sheet2.AddMergedRegion(new CellRangeAddress(2, 2, 8, 8));

                SetExcelCell(sheet2, dt2.Rows.Count + 4, 0, styleHead10, "案件類別");
                sheet2.AddMergedRegion(new CellRangeAddress(dt2.Rows.Count + 4, dt2.Rows.Count + 4, 0, 0));
                SetExcelCell(sheet2, dt2.Rows.Count + 4, 1, styleHead10, "扣押");
                sheet2.AddMergedRegion(new CellRangeAddress(dt2.Rows.Count + 4, dt2.Rows.Count + 4, 1, 1));
                SetExcelCell(sheet2, dt2.Rows.Count + 4, 2, styleHead10, "支付");
                sheet2.AddMergedRegion(new CellRangeAddress(dt2.Rows.Count + 4, dt2.Rows.Count + 4, 2, 2));
                SetExcelCell(sheet2, dt2.Rows.Count + 4, 3, styleHead10, "扣押並支付");
                sheet2.AddMergedRegion(new CellRangeAddress(dt2.Rows.Count + 4, dt2.Rows.Count + 4, 3, 3));
                SetExcelCell(sheet2, dt2.Rows.Count + 4, 4, styleHead10, "外來文案件");
                sheet2.AddMergedRegion(new CellRangeAddress(dt2.Rows.Count + 4, dt2.Rows.Count + 4, 4, 4));
                SetExcelCell(sheet2, dt2.Rows.Count + 4, 5, styleHead10, "合計");
                sheet2.AddMergedRegion(new CellRangeAddress(dt2.Rows.Count + 4, dt2.Rows.Count + 4, 5, 5));

                SetExcelCell(sheet2, dt2.Rows.Count + 5, 0, styleHead10, "件數");
                sheet2.AddMergedRegion(new CellRangeAddress(dt2.Rows.Count + 5, dt2.Rows.Count + 5, 0, 0));

                SetExcelCell(sheet2, dt2.Rows.Count + 7, 4, styleHead12, "覆核主管");
                sheet2.AddMergedRegion(new CellRangeAddress(dt2.Rows.Count + 7, dt2.Rows.Count + 7, 4, 4));
                SetExcelCell(sheet2, dt2.Rows.Count + 7, 6, styleHead12, "覆核人員");
                sheet2.AddMergedRegion(new CellRangeAddress(dt2.Rows.Count + 7, dt2.Rows.Count + 7, 6, 6));

                for (int i = dt2.Rows.Count + 4; i < dt2.Rows.Count + 5; i++)//*初始表格賦初值 
                {
                    for (int j = 0; j < 5; j++)
                    {
                        SetExcelCell(sheet2, i + 1, j + 1, styleHead10, "0");
                        sheet2.AddMergedRegion(new CellRangeAddress(i + 1, i + 1, j + 1, j + 1));
                    }
                }
                #endregion
                #region title3
                SetExcelCell(sheet3, 0, 0, styleHead12, "用印簿");
                sheet3.AddMergedRegion(new CellRangeAddress(0, 0, 0, 8));

                //* line1
                SetExcelCell(sheet3, 1, 0, styleHead12, "收件日期：");
                sheet3.AddMergedRegion(new CellRangeAddress(1, 1, 0, 0));
                SetExcelCell(sheet3, 1, 1, styleHead12, model.ReceiveDateStart + '~' + model.ReceiveDateEnd);
                sheet3.AddMergedRegion(new CellRangeAddress(1, 1, 1, 1));
                SetExcelCell(sheet3, 1, 2, styleHead12, "發文日期：");
                sheet3.AddMergedRegion(new CellRangeAddress(1, 1, 2, 2));
                SetExcelCell(sheet3, 1, 3, styleHead12, model.SendDateStart + '~' + model.SendDateEnd);
                sheet3.AddMergedRegion(new CellRangeAddress(1, 1, 3, 3));
                SetExcelCell(sheet3, 1, 4, styleHead12, "結案日期：");
                sheet3.AddMergedRegion(new CellRangeAddress(1, 1, 4, 4));
                SetExcelCell(sheet3, 1, 5, styleHead12, model.CloseDateStart + '~' + model.CloseDateEnd);
                sheet3.AddMergedRegion(new CellRangeAddress(1, 1, 5, 5));
                SetExcelCell(sheet3, 1, 7, styleHead12, "部門別/科別：");
                sheet3.AddMergedRegion(new CellRangeAddress(1, 1, 7, 7));


                //* line2
                SetExcelCell(sheet3, 2, 0, styleHead10, "發文字號");
                sheet3.AddMergedRegion(new CellRangeAddress(2, 2, 0, 0));
                SetExcelCell(sheet3, 2, 1, styleHead10, "案件編號");
                sheet3.AddMergedRegion(new CellRangeAddress(2, 2, 1, 1));
                SetExcelCell(sheet3, 2, 2, styleHead10, "受文者");
                sheet3.AddMergedRegion(new CellRangeAddress(2, 2, 2, 2));
                SetExcelCell(sheet3, 2, 3, styleHead10, "副本");
                sheet3.AddMergedRegion(new CellRangeAddress(2, 2, 3, 3));
                SetExcelCell(sheet3, 2, 4, styleHead10, "經辦");
                sheet3.AddMergedRegion(new CellRangeAddress(2, 2, 4, 4));
                SetExcelCell(sheet3, 2, 5, styleHead10, "案件大類");
                sheet3.AddMergedRegion(new CellRangeAddress(2, 2, 5, 5));
                SetExcelCell(sheet3, 2, 6, styleHead10, "案件細類");
                sheet3.AddMergedRegion(new CellRangeAddress(2, 2, 6, 6));
                SetExcelCell(sheet3, 2, 7, styleHead10, "放行主管");
                sheet3.AddMergedRegion(new CellRangeAddress(2, 2, 7, 7));
                SetExcelCell(sheet3, 2, 8, styleHead10, "逾期註記");
                sheet3.AddMergedRegion(new CellRangeAddress(2, 2, 8, 8));

                SetExcelCell(sheet3, dt3.Rows.Count + 4, 0, styleHead10, "案件類別");
                sheet3.AddMergedRegion(new CellRangeAddress(dt3.Rows.Count + 4, dt3.Rows.Count + 4, 0, 0));
                SetExcelCell(sheet3, dt3.Rows.Count + 4, 1, styleHead10, "扣押");
                sheet3.AddMergedRegion(new CellRangeAddress(dt3.Rows.Count + 4, dt3.Rows.Count + 4, 1, 1));
                SetExcelCell(sheet3, dt3.Rows.Count + 4, 2, styleHead10, "支付");
                sheet3.AddMergedRegion(new CellRangeAddress(dt3.Rows.Count + 4, dt3.Rows.Count + 4, 2, 2));
                SetExcelCell(sheet3, dt3.Rows.Count + 4, 3, styleHead10, "扣押並支付");
                sheet3.AddMergedRegion(new CellRangeAddress(dt3.Rows.Count + 4, dt3.Rows.Count + 4, 3, 3));
                SetExcelCell(sheet3, dt3.Rows.Count + 4, 4, styleHead10, "外來文案件");
                sheet3.AddMergedRegion(new CellRangeAddress(dt3.Rows.Count + 4, dt3.Rows.Count + 4, 4, 4));
                SetExcelCell(sheet3, dt3.Rows.Count + 4, 5, styleHead10, "合計");
                sheet3.AddMergedRegion(new CellRangeAddress(dt3.Rows.Count + 4, dt3.Rows.Count + 4, 5, 5));

                SetExcelCell(sheet3, dt3.Rows.Count + 5, 0, styleHead10, "件數");
                sheet3.AddMergedRegion(new CellRangeAddress(dt3.Rows.Count + 5, dt3.Rows.Count + 5, 0, 0));

                SetExcelCell(sheet3, dt3.Rows.Count + 7, 4, styleHead12, "覆核主管");
                sheet3.AddMergedRegion(new CellRangeAddress(dt3.Rows.Count + 7, dt3.Rows.Count + 7, 4, 4));
                SetExcelCell(sheet3, dt3.Rows.Count + 7, 6, styleHead12, "覆核人員");
                sheet3.AddMergedRegion(new CellRangeAddress(dt3.Rows.Count + 7, dt3.Rows.Count + 7, 6, 6));

                for (int i = dt3.Rows.Count + 4; i < dt3.Rows.Count + 5; i++)//*初始表格賦初值 
                {
                    for (int j = 0; j < 5; j++)
                    {
                        SetExcelCell(sheet3, i + 1, j + 1, styleHead10, "0");
                        sheet3.AddMergedRegion(new CellRangeAddress(i + 1, i + 1, j + 1, j + 1));
                    }
                }
                #endregion
                #region Width2
                sheet2.SetColumnWidth(0, 100 * 30);
                sheet2.SetColumnWidth(1, 100 * 40);
                sheet2.SetColumnWidth(2, 100 * 40);
                sheet2.SetColumnWidth(3, 100 * 100);
                sheet2.SetColumnWidth(4, 100 * 30);
                sheet2.SetColumnWidth(5, 100 * 30);
                sheet2.SetColumnWidth(6, 100 * 30);
                sheet2.SetColumnWidth(7, 100 * 40);
                sheet2.SetColumnWidth(8, 100 * 30);
                #endregion
                #region Width3
                sheet3.SetColumnWidth(0, 100 * 30);
                sheet3.SetColumnWidth(1, 100 * 40);
                sheet3.SetColumnWidth(2, 100 * 40);
                sheet3.SetColumnWidth(3, 100 * 100);
                sheet3.SetColumnWidth(4, 100 * 30);
                sheet3.SetColumnWidth(5, 100 * 30);
                sheet3.SetColumnWidth(6, 100 * 30);
                sheet3.SetColumnWidth(7, 100 * 40);
                sheet3.SetColumnWidth(8, 100 * 30);
                #endregion
                #region body2
                for (int iRow = 0; iRow < dt2.Rows.Count; iRow++)
                {
                    for (int iCol = 0; iCol < dt2.Columns.Count - 1; iCol++)
                    {
                        SetExcelCell(sheet2, iRow + 3, iCol, styleHead10, dt2.Rows[iRow][iCol].ToString());
                    }
                }
                int rows2 = dt2.Rows.Count + 5;
                int Count2 = 0;
                int CountNum2 = 0;
                for (int i = 0; i < dtcount2.Rows.Count; i++)//*初始表格賦初值 
                {
                    if (Convert.ToString(dtcount2.Rows[i]["CaseKind2"]) == "扣押")
                    {
                        SetExcelCell(sheet2, rows2, 1, styleHead10, Convert.ToString(dtcount2.Rows[i]["C"]));
                    }
                    if (Convert.ToString(dtcount2.Rows[i]["CaseKind2"]) == "支付")
                    {
                        SetExcelCell(sheet2, rows2, 2, styleHead10, Convert.ToString(dtcount2.Rows[i]["C"]));
                    }
                    if (Convert.ToString(dtcount2.Rows[i]["CaseKind2"]) == "扣押並支付")
                    {
                        SetExcelCell(sheet2, rows2, 3, styleHead10, Convert.ToString(dtcount2.Rows[i]["C"]));
                    }
                    if (Convert.ToString(dtcount2.Rows[i]["CaseKind2"]) == "外來文案件")
                    {
                        SetExcelCell(sheet2, rows2, 4, styleHead10, Convert.ToString(dtcount2.Rows[i]["C"]));
                    }
                    Count2 = Convert.ToInt32(dtcount2.Rows[i]["C"].ToString());
                    CountNum2 += Count2;
                }
                SetExcelCell(sheet2, rows2, 5, styleHead10, CountNum2.ToString());
                #endregion
                #region body3
                for (int iRow = 0; iRow < dt3.Rows.Count; iRow++)
                {
                    for (int iCol = 0; iCol < dt3.Columns.Count - 1; iCol++)
                    {
                        SetExcelCell(sheet3, iRow + 3, iCol, styleHead10, dt3.Rows[iRow][iCol].ToString());
                    }
                }
                int rows3 = dt3.Rows.Count + 5;
                int Count3 = 0;
                int CountNum3 = 0;
                for (int i = 0; i < dtcount2.Rows.Count; i++)//*初始表格賦初值 
                {
                    if (Convert.ToString(dtcount2.Rows[i]["CaseKind2"]) == "扣押")
                    {
                        SetExcelCell(sheet3, rows3, 1, styleHead10, Convert.ToString(dtcount2.Rows[i]["C"]));
                    }
                    if (Convert.ToString(dtcount2.Rows[i]["CaseKind2"]) == "支付")
                    {
                        SetExcelCell(sheet3, rows3, 2, styleHead10, Convert.ToString(dtcount2.Rows[i]["C"]));
                    }
                    if (Convert.ToString(dtcount2.Rows[i]["CaseKind2"]) == "扣押並支付")
                    {
                        SetExcelCell(sheet3, rows3, 3, styleHead10, Convert.ToString(dtcount2.Rows[i]["C"]));
                    }
                    if (Convert.ToString(dtcount2.Rows[i]["CaseKind2"]) == "外來文案件")
                    {
                        SetExcelCell(sheet3, rows3, 4, styleHead10, Convert.ToString(dtcount2.Rows[i]["C"]));
                    }
                    Count3 = Convert.ToInt32(dtcount2.Rows[i]["C"].ToString());
                    CountNum3 += Count3;
                }
                SetExcelCell(sheet3, rows3, 5, styleHead10, CountNum3.ToString());
                #endregion
            }
            MemoryStream ms = new MemoryStream();
            workbook.Write(ms);
            ms.Flush();
            ms.Position = 0;
            workbook = null;
            return ms;
        }
        #endregion
        #region 經辦結案統計表

        public MemoryStream CaseMasterListReportExcel_NPOI(CaseClosedQuery model)
        {
            IWorkbook workbook = new HSSFWorkbook();
            ISheet sheet = null;
            ISheet sheet2 = null;
            ISheet sheet3 = null;
            Dictionary<string, string> dicldapList = new Dictionary<string, string>();
            Dictionary<string, string> dicldapList2 = new Dictionary<string, string>();
            Dictionary<string, string> dicldapList3 = new Dictionary<string, string>();
            DataTable dtCase = new DataTable();//導出資料
            DataTable dtCase2 = new DataTable();
            DataTable dtCase3 = new DataTable();

            int rowscountExcelresult = 0;//合計參數
            string caseExcel = "";//案件類型
            int rowsExcel = 4;//行數
            int rowscountExcel = 0;//最後一列合計
            int rowstatolExcel = 0;//總合計      
            int sort = 1;//記錄每個名字在哪一格

            #region def style
            ICellStyle styleHead12 = workbook.CreateCellStyle();
            IFont font12 = workbook.CreateFont();
            font12.FontHeightInPoints = 12;
            font12.FontName = "新細明體";
            styleHead12.FillPattern = FillPattern.SolidForeground;
            styleHead12.FillForegroundColor = HSSFColor.White.Index;
            styleHead12.BorderTop = BorderStyle.None;
            styleHead12.BorderLeft = BorderStyle.None;
            styleHead12.BorderRight = BorderStyle.None;
            styleHead12.BorderBottom = BorderStyle.None;
            styleHead12.WrapText = true;
            styleHead12.Alignment = HorizontalAlignment.Center;
            styleHead12.VerticalAlignment = VerticalAlignment.Center;
            styleHead12.SetFont(font12);

            ICellStyle styleHead10 = workbook.CreateCellStyle();
            IFont font10 = workbook.CreateFont();
            font10.FontHeightInPoints = 10;
            font10.FontName = "新細明體";
            styleHead10.FillPattern = FillPattern.SolidForeground;
            styleHead10.FillForegroundColor = HSSFColor.White.Index;
            styleHead10.BorderTop = BorderStyle.Thin;
            styleHead10.BorderLeft = BorderStyle.Thin;
            styleHead10.BorderRight = BorderStyle.Thin;
            styleHead10.BorderBottom = BorderStyle.Thin;
            styleHead10.WrapText = true;
            styleHead10.Alignment = HorizontalAlignment.Left;
            styleHead10.VerticalAlignment = VerticalAlignment.Center;
            styleHead10.SetFont(font10);
            #endregion

            #region 單獨科別的數據源(科別及案件資料)
            //獲取人員
            if (model.Depart == "1" || model.Depart == "0")//* 集作一科
            {
                sheet = workbook.CreateSheet("集作一科");
                dtCase = GetCaseMasterList(model, "集作一科");
                //判斷人員
                foreach (DataRow dr in dtCase.Rows)
                {
                    if (!dicldapList.Keys.Contains(dr["AgentUser"].ToString()))
                    {
                        dicldapList.Add(dr["AgentUser"].ToString(), dr["EmpName"].ToString() + "|" + sort);
                        sort++;
                    }
                }
                if (dicldapList.Count > 2)
                {
                    SetExcelCell(sheet, 1, dicldapList.Count + 1, styleHead12, "集作一科");
                    sheet.AddMergedRegion(new CellRangeAddress(1, 1, dicldapList.Count + 1, dicldapList.Count + 1));
                }
                else
                {
                    SetExcelCell(sheet, 1, 4, styleHead12, "集作一科");
                    sheet.AddMergedRegion(new CellRangeAddress(1, 1, 4, 4));
                    sheet.SetColumnWidth(4, 100 * 30);
                }
            }
            if (model.Depart == "2")//* 集作二科
            {
                sheet = workbook.CreateSheet("集作二科");
                dtCase = GetCaseMasterList(model, "集作二科");
                //判斷人員
                foreach (DataRow dr in dtCase.Rows)
                {
                    if (!dicldapList.Keys.Contains(dr["AgentUser"].ToString()))
                    {
                        dicldapList.Add(dr["AgentUser"].ToString(), dr["EmpName"].ToString() + "|" + sort);
                        sort++;
                    }
                }
                if (dicldapList.Count > 2)
                {
                    SetExcelCell(sheet, 1, dicldapList.Count + 1, styleHead12, "集作二科");
                    sheet.AddMergedRegion(new CellRangeAddress(1, 1, dicldapList.Count + 1, dicldapList.Count + 1));
                }
                else
                {
                    SetExcelCell(sheet, 1, 4, styleHead12, "集作二科");
                    sheet.AddMergedRegion(new CellRangeAddress(1, 1, 4, 4));
                    sheet.SetColumnWidth(4, 100 * 30);
                }
            }
            if (model.Depart == "3")//* 集作三科
            {
                sheet = workbook.CreateSheet("集作三科");
                dtCase = GetCaseMasterList(model, "集作三科");
                //判斷人員
                foreach (DataRow dr in dtCase.Rows)
                {
                    if (!dicldapList.Keys.Contains(dr["AgentUser"].ToString()))
                    {
                        dicldapList.Add(dr["AgentUser"].ToString(), dr["EmpName"].ToString() + "|" + sort);
                        sort++;
                    }
                }
                if (dicldapList.Count > 2)
                {
                    SetExcelCell(sheet, 1, dicldapList.Count + 1, styleHead12, "集作三科");
                    sheet.AddMergedRegion(new CellRangeAddress(1, 1, dicldapList.Count + 1, dicldapList.Count + 1));
                }
                else
                {
                    SetExcelCell(sheet, 1, 4, styleHead12, "集作三科");
                    sheet.AddMergedRegion(new CellRangeAddress(1, 1, 4, 4));
                    sheet.SetColumnWidth(4, 100 * 30);
                }
            }
            if (model.Depart == "0")//* 全部
            {
                sheet2 = workbook.CreateSheet("集作二科");
                sheet3 = workbook.CreateSheet("集作三科");
                dtCase2 = GetCaseMasterList(model, "集作二科");//獲取查詢集作二科的案件
                dtCase3 = GetCaseMasterList(model, "集作三科");//獲取查詢集作三科的案件
                sort = 1;
                //判斷集作二科人員
                foreach (DataRow dr in dtCase2.Rows)
                {
                    if (!dicldapList2.Keys.Contains(dr["AgentUser"].ToString()))
                    {
                        dicldapList2.Add(dr["AgentUser"].ToString(), dr["EmpName"].ToString() + "|" + sort);
                        sort++;
                    }
                }

                sort = 1;
                //判斷集作三科人員
                foreach (DataRow dr in dtCase3.Rows)
                {
                    if (!dicldapList3.Keys.Contains(dr["AgentUser"].ToString()))
                    {
                        dicldapList3.Add(dr["AgentUser"].ToString(), dr["EmpName"].ToString() + "|" + sort);
                        sort++;
                    }
                }
            }
            #endregion

            string caseKind = "";//*去重複
            int rows = 4;//title中定義行數
            #region title
            //*大標題 line0
            SetExcelCell(sheet, 0, 0, styleHead12, "經辦結案統計表");
            sheet.AddMergedRegion(new CellRangeAddress(0, 0, 0, dicldapList.Count + 1));

            //*查詢條件 line1
            SetExcelCell(sheet, 1, 0, styleHead12, "收件日期：");
            sheet.AddMergedRegion(new CellRangeAddress(1, 1, 0, 0));
            SetExcelCell(sheet, 1, 1, styleHead12, model.ReceiveDateStart + '~' + model.ReceiveDateEnd);
            sheet.AddMergedRegion(new CellRangeAddress(1, 1, 1, 2));

            if (dicldapList.Count > 2)
            {
                SetExcelCell(sheet, 1, dicldapList.Count, styleHead12, "部門別/科別");
                sheet.AddMergedRegion(new CellRangeAddress(1, 1, dicldapList.Count, dicldapList.Count));
            }
            else
            {
                SetExcelCell(sheet, 1, 3, styleHead12, "部門別/科別");
                sheet.AddMergedRegion(new CellRangeAddress(1, 1, 3, 3));
                sheet.SetColumnWidth(3, 100 * 30);
            }

            //*結果集表頭 line2
            SetExcelCell(sheet, 2, 0, styleHead12, "發文日期：");
            sheet.AddMergedRegion(new CellRangeAddress(2, 2, 0, 0));
            SetExcelCell(sheet, 2, 1, styleHead12, model.SendDateStart + '~' + model.SendDateEnd);
            sheet.AddMergedRegion(new CellRangeAddress(2, 2, 1, 2));
            //*結果集表頭 line3
            SetExcelCell(sheet, 3, 0, styleHead12, "結案日期：");
            sheet.AddMergedRegion(new CellRangeAddress(3, 3, 0, 0));
            SetExcelCell(sheet, 3, 1, styleHead12, model.CloseDateStart + '~' + model.CloseDateEnd);
            sheet.AddMergedRegion(new CellRangeAddress(3, 3, 1, 2));


            //*結果集表頭 line4
            SetExcelCell(sheet, 4, 0, styleHead10, "處理人員");
            sheet.AddMergedRegion(new CellRangeAddress(4, 4, 0, 0));
            sheet.SetColumnWidth(0, 100 * 50);
            //依次排列人員名稱
            int a = 1;
            foreach (var item in dicldapList)
            {
                SetExcelCell(sheet, 4, a, styleHead10, item.Value.Split('|')[0]);
                sheet.AddMergedRegion(new CellRangeAddress(4, 4, a, a));
                a++;
            }
            SetExcelCell(sheet, 4, dicldapList.Count + 1, styleHead10, "合計");
            sheet.AddMergedRegion(new CellRangeAddress(4, 4, dicldapList.Count + 1, dicldapList.Count + 1));

            //*扣押案件類型 line5-lineN 
            for (int i = 0; i < dtCase.Rows.Count; i++)
            {
                if (caseKind != dtCase.Rows[i]["New_CaseKind"].ToString())
                {
                    rows = rows + 1;
                    SetExcelCell(sheet, rows, 0, styleHead10, dtCase.Rows[i]["New_CaseKind"].ToString());
                    sheet.AddMergedRegion(new CellRangeAddress(rows, rows, 0, 0));
                    SetExcelCell(sheet, rows, dicldapList.Count + 1, styleHead10, "0");//最後一列合計賦初值
                    sheet.AddMergedRegion(new CellRangeAddress(rows, rows, dicldapList.Count + 1, dicldapList.Count + 1));
                    caseKind = dtCase.Rows[i]["New_CaseKind"].ToString();
                    for (int j = 0; j < dicldapList.Count; j++)//*初始表格賦初值 
                    {
                        SetExcelCell(sheet, rows, j + 1, styleHead10, "0");
                        sheet.AddMergedRegion(new CellRangeAddress(rows, rows, j + 1, j + 1));
                    }
                }
            }

            //*合計 lineLast (案件下面的合計以及整行賦初值)
            SetExcelCell(sheet, rows + 1, 0, styleHead10, "合計");
            sheet.AddMergedRegion(new CellRangeAddress(rows + 1, rows + 1, 0, 0));
            if (dicldapList.Count > 0)
            {
                for (int j = 0; j < dicldapList.Count; j++)//*初始表格賦初值 
                {
                    SetExcelCell(sheet, rows + 1, j + 1, styleHead10, "0");
                    sheet.AddMergedRegion(new CellRangeAddress(rows + 1, rows + 1, j + 1, j + 1));
                }
            }
            else
            {
                SetExcelCell(sheet, rows + 1, 1, styleHead10, "0");
                sheet.AddMergedRegion(new CellRangeAddress(rows + 1, rows + 1, 1, 1));
            }

            #endregion
            #region  body
            for (int iRow = 0; iRow < dtCase.Rows.Count; iRow++)//根據案件類型進行循環
            {
                foreach (var item in dicldapList)
                {
                    int irows = Convert.ToInt32(item.Value.Split('|')[1]);
                    if (caseExcel == dtCase.Rows[iRow]["New_CaseKind"].ToString())//重複同一案件類型的數據
                    {
                        if (item.Key == dtCase.Rows[iRow]["AgentUser"].ToString())
                        {
                            SetExcelCell(sheet, rowsExcel, irows, styleHead10, dtCase.Rows[iRow]["case_num"].ToString());
                            rowscountExcelresult = Convert.ToInt32(dtCase.Rows[iRow]["case_num"].ToString());//每格資料
                            SetExcelCell(sheet, rows + 1, irows, styleHead10, dtCase.Rows[iRow]["User_Count"].ToString());//最後一行合計
                            rowscountExcel += rowscountExcelresult;
                            rowstatolExcel += rowscountExcelresult;
                        }
                        SetExcelCell(sheet, rowsExcel, dicldapList.Count + 1, styleHead10, rowscountExcel.ToString());//最後一列合計
                    }
                    else//不重複的案件類型
                    {
                        rowscountExcel = 0;
                        rowsExcel = rowsExcel + 1;
                        if (item.Key == dtCase.Rows[iRow]["AgentUser"].ToString())
                        {
                            SetExcelCell(sheet, rowsExcel, irows, styleHead10, dtCase.Rows[iRow]["case_num"].ToString());
                            rowscountExcelresult = Convert.ToInt32(dtCase.Rows[iRow]["case_num"].ToString());//第一條不重複的數據儲存下值
                            SetExcelCell(sheet, rows + 1, irows, styleHead10, dtCase.Rows[iRow]["User_Count"].ToString());//最後一行合計
                            rowscountExcel += rowscountExcelresult;
                            rowstatolExcel += rowscountExcelresult;
                        }
                        caseExcel = dtCase.Rows[iRow]["New_CaseKind"].ToString();
                        SetExcelCell(sheet, rowsExcel, dicldapList.Count + 1, styleHead10, rowscountExcel.ToString());//最後一列合計
                    }
                }
            }
            SetExcelCell(sheet, rows + 1, dicldapList.Count + 1, styleHead10, rowstatolExcel.ToString());//總合計
            #endregion

            if (model.Depart == "0")//* 全部
            {
                #region title2
                //*大標題 line0
                SetExcelCell(sheet2, 0, 0, styleHead12, "經辦結案統計表");
                sheet2.AddMergedRegion(new CellRangeAddress(0, 0, 0, dicldapList2.Count + 1));

                //*查詢條件 line1
                SetExcelCell(sheet2, 1, 0, styleHead12, "收件日期：");
                sheet2.AddMergedRegion(new CellRangeAddress(1, 1, 0, 0));
                SetExcelCell(sheet2, 1, 1, styleHead12, model.ReceiveDateStart + '~' + model.ReceiveDateEnd);
                sheet2.AddMergedRegion(new CellRangeAddress(1, 1, 1, 2));

                if (dicldapList2.Count > 2)
                {
                    SetExcelCell(sheet2, 1, dicldapList2.Count, styleHead12, "部門別/科別");
                    sheet2.AddMergedRegion(new CellRangeAddress(1, 1, dicldapList2.Count, dicldapList2.Count));
                    SetExcelCell(sheet2, 1, dicldapList2.Count + 1, styleHead12, "集作二科");
                    sheet.AddMergedRegion(new CellRangeAddress(1, 1, dicldapList2.Count + 1, dicldapList2.Count + 1));
                }
                else
                {
                    SetExcelCell(sheet2, 1, 3, styleHead12, "部門別/科別");
                    sheet.AddMergedRegion(new CellRangeAddress(1, 1, 3, 3));
                    sheet2.SetColumnWidth(3, 100 * 50);
                    SetExcelCell(sheet2, 1, 4, styleHead12, "集作二科");
                    sheet.AddMergedRegion(new CellRangeAddress(1, 1, 4, 4));
                    sheet2.SetColumnWidth(4, 100 * 30);
                }

                //*查詢條件 line2
                SetExcelCell(sheet2, 2, 0, styleHead12, "發文日期：");
                sheet2.AddMergedRegion(new CellRangeAddress(2, 2, 0, 0));
                SetExcelCell(sheet2, 2, 1, styleHead12, model.SendDateStart + '~' + model.SendDateEnd);
                sheet2.AddMergedRegion(new CellRangeAddress(2, 2, 1, 2));

                //*查詢條件 line3
                SetExcelCell(sheet2, 3, 0, styleHead12, "結案日期：");
                sheet2.AddMergedRegion(new CellRangeAddress(3, 3, 0, 0));
                SetExcelCell(sheet2, 3, 1, styleHead12, model.CloseDateStart + '~' + model.CloseDateEnd);
                sheet2.AddMergedRegion(new CellRangeAddress(3, 3, 1, 2));

                //*結果集表頭 line4
                SetExcelCell(sheet2, 4, 0, styleHead10, "處理人員");
                sheet2.AddMergedRegion(new CellRangeAddress(4, 4, 0, 0));
                sheet2.SetColumnWidth(0, 100 * 50);
                int icols = 1;
                foreach (var item in dicldapList2)
                {
                    SetExcelCell(sheet2, 4, icols, styleHead10, item.Value.Split('|')[0]);
                    sheet2.AddMergedRegion(new CellRangeAddress(4, 4, icols, icols));
                    icols++;
                }
                SetExcelCell(sheet2, 4, dicldapList2.Count + 1, styleHead10, "合計");
                sheet2.AddMergedRegion(new CellRangeAddress(2, 2, dicldapList2.Count + 1, dicldapList2.Count + 1));

                //*扣押案件類型 line5-lineN 
                caseKind = "";//*去重複
                rows = 4;//定義行數
                for (int i = 0; i < dtCase2.Rows.Count; i++)
                {
                    if (caseKind != dtCase2.Rows[i]["New_CaseKind"].ToString())
                    {
                        rows = rows + 1;
                        SetExcelCell(sheet2, rows, 0, styleHead10, dtCase2.Rows[i]["New_CaseKind"].ToString());
                        sheet2.AddMergedRegion(new CellRangeAddress(rows, rows, 0, 0));

                        SetExcelCell(sheet2, rows, dicldapList2.Count + 1, styleHead10, "0");//給最後一列合計賦初值
                        sheet2.AddMergedRegion(new CellRangeAddress(rows, rows, dicldapList2.Count + 1, dicldapList2.Count + 1));

                        caseKind = dtCase2.Rows[i]["New_CaseKind"].ToString();
                        for (int j = 0; j < dicldapList2.Count; j++)//*初始表格賦初值 
                        {
                            SetExcelCell(sheet2, rows, j + 1, styleHead10, "0");
                            sheet2.AddMergedRegion(new CellRangeAddress(rows, rows, j + 1, j + 1));
                        }
                    }
                }

                //*合計 lineLast (最後一行合計)
                SetExcelCell(sheet2, rows + 1, 0, styleHead10, "合計");
                sheet2.AddMergedRegion(new CellRangeAddress(dicldapList2.Count + 3, dicldapList2.Count + 3, 0, 0));
                if (dicldapList2.Count > 0)
                {
                    for (int j = 0; j < dicldapList2.Count; j++)//*初始表格賦初值 
                    {
                        SetExcelCell(sheet2, rows + 1, j + 1, styleHead10, "0");
                        sheet2.AddMergedRegion(new CellRangeAddress(rows + 1, rows + 1, j + 1, j + 1));
                    }
                }
                else
                {
                    SetExcelCell(sheet2, rows + 1, 1, styleHead10, "0");
                    sheet2.AddMergedRegion(new CellRangeAddress(rows + 1, rows + 1, 1, 1));
                }

                #endregion
                #region body2
                caseExcel = "";//案件類型
                rowsExcel = 4;//行數
                rowscountExcel = 0;//最後一列合計
                rowstatolExcel = 0;//總合計      
                for (int iRow = 0; iRow < dtCase2.Rows.Count; iRow++)//根據案件類型進行循環
                {
                    foreach (var item in dicldapList2)
                    {
                        int icol = Convert.ToInt32(item.Value.Split('|')[1]);
                        if (dtCase2.Rows[iRow]["New_CaseKind"].ToString() == caseExcel)//重複同一案件類型的數據
                        {
                            if (item.Key == dtCase2.Rows[iRow]["AgentUser"].ToString())
                            {
                                SetExcelCell(sheet2, rowsExcel, icol, styleHead10, dtCase2.Rows[iRow]["case_num"].ToString());
                                SetExcelCell(sheet2, rows + 1, icol, styleHead10, dtCase2.Rows[iRow]["User_Count"].ToString());//最後一行合計
                                rowscountExcelresult = Convert.ToInt32(dtCase2.Rows[iRow]["case_num"].ToString());//每格資料
                                rowscountExcel += rowscountExcelresult;
                                rowstatolExcel += rowscountExcelresult;
                            }
                            SetExcelCell(sheet2, rowsExcel, dicldapList2.Count + 1, styleHead10, rowscountExcel.ToString());//最後一列合計
                        }
                        else//不重複的案件類型
                        {
                            rowscountExcel = 0;
                            rowsExcel = rowsExcel + 1;
                            if (item.Key == dtCase2.Rows[iRow]["AgentUser"].ToString())
                            {
                                SetExcelCell(sheet2, rowsExcel, icol, styleHead10, dtCase2.Rows[iRow]["case_num"].ToString());
                                SetExcelCell(sheet2, rows + 1, icol, styleHead10, dtCase2.Rows[iRow]["User_Count"].ToString());//最後一行合計
                                rowscountExcelresult = Convert.ToInt32(dtCase2.Rows[iRow]["case_num"].ToString());//第一條不重複的數據儲存下值
                                rowscountExcel += rowscountExcelresult;
                                rowstatolExcel += rowscountExcelresult;
                            }
                            caseExcel = dtCase2.Rows[iRow]["New_CaseKind"].ToString();
                            SetExcelCell(sheet2, rowsExcel, dicldapList2.Count + 1, styleHead10, rowscountExcel.ToString());//最後一列合計      
                        }
                    }
                    SetExcelCell(sheet2, rows + 1, dicldapList2.Count + 1, styleHead10, rowstatolExcel.ToString());//總合計
                }
                #endregion

                #region title3
                //*大標題 line0
                SetExcelCell(sheet3, 0, 0, styleHead12, "經辦結案統計表");
                sheet3.AddMergedRegion(new CellRangeAddress(0, 0, 0, dicldapList3.Count + 1));

                //*查詢條件 line1
                SetExcelCell(sheet3, 1, 0, styleHead12, "收件日期：");
                sheet3.AddMergedRegion(new CellRangeAddress(1, 1, 0, 0));
                SetExcelCell(sheet3, 1, 1, styleHead12, model.ReceiveDateStart + '~' + model.ReceiveDateEnd);
                sheet3.AddMergedRegion(new CellRangeAddress(1, 1, 1, 2));
                if (dicldapList3.Count > 2)
                {
                    SetExcelCell(sheet3, 1, dicldapList3.Count, styleHead12, "部門別/科別");
                    sheet3.AddMergedRegion(new CellRangeAddress(1, 1, dicldapList3.Count, dicldapList3.Count));
                    SetExcelCell(sheet3, 1, dicldapList3.Count + 1, styleHead12, "集作三科");
                    sheet.AddMergedRegion(new CellRangeAddress(1, 1, dicldapList3.Count + 1, dicldapList3.Count + 1));
                }
                else
                {
                    SetExcelCell(sheet3, 1, 3, styleHead12, "部門別/科別");
                    sheet3.AddMergedRegion(new CellRangeAddress(1, 1, 3, 3));
                    sheet3.SetColumnWidth(3, 100 * 50);
                    SetExcelCell(sheet3, 1, 4, styleHead12, "集作三科");
                    sheet.AddMergedRegion(new CellRangeAddress(1, 1, 4, 4));
                    sheet3.SetColumnWidth(4, 100 * 30);
                }


                //*查詢條件 line2
                SetExcelCell(sheet3, 2, 0, styleHead12, "發文日期：");
                sheet3.AddMergedRegion(new CellRangeAddress(2, 2, 0, 0));
                SetExcelCell(sheet3, 2, 1, styleHead12, model.SendDateStart + '~' + model.SendDateEnd);
                sheet3.AddMergedRegion(new CellRangeAddress(2, 2, 1, 2));

                //*查詢條件 line3
                SetExcelCell(sheet3, 3, 0, styleHead12, "結案日期：");
                sheet3.AddMergedRegion(new CellRangeAddress(3, 3, 0, 0));
                SetExcelCell(sheet3, 3, 1, styleHead12, model.CloseDateStart + '~' + model.CloseDateEnd);
                sheet3.AddMergedRegion(new CellRangeAddress(3, 3, 1, 2));

                //*結果集表頭 line4
                SetExcelCell(sheet3, 4, 0, styleHead10, "處理人員");
                sheet3.AddMergedRegion(new CellRangeAddress(4, 4, 0, 0));
                sheet3.SetColumnWidth(0, 100 * 50);

                int irows3 = 1;
                foreach (var item in dicldapList3)
                {
                    SetExcelCell(sheet3, 4, irows3, styleHead10, item.Value.Split('|')[0]);
                    sheet3.AddMergedRegion(new CellRangeAddress(4, 4, irows3, irows3));
                    irows3++;
                }

                SetExcelCell(sheet3, 4, dicldapList3.Count + 1, styleHead10, "合計");
                sheet3.AddMergedRegion(new CellRangeAddress(4, 4, dicldapList3.Count + 1, dicldapList3.Count + 1));

                //*扣押案件類型 line5-lineN 
                caseKind = "";//*去重複
                rows = 4;//定義行數
                for (int i = 0; i < dtCase3.Rows.Count; i++)
                {
                    if (caseKind != dtCase3.Rows[i]["New_CaseKind"].ToString())
                    {
                        rows = rows + 1;
                        SetExcelCell(sheet3, rows, 0, styleHead10, dtCase3.Rows[i]["New_CaseKind"].ToString());
                        sheet3.AddMergedRegion(new CellRangeAddress(rows, rows, 0, 0));
                        SetExcelCell(sheet3, rows, dicldapList3.Count + 1, styleHead10, "0");
                        sheet3.AddMergedRegion(new CellRangeAddress(rows, rows, dicldapList3.Count + 1, dicldapList3.Count + 1));
                        caseKind = dtCase3.Rows[i]["New_CaseKind"].ToString();
                        for (int j = 0; j < dicldapList3.Count; j++)//*初始表格賦初值 
                        {
                            SetExcelCell(sheet3, rows, j + 1, styleHead10, "0");
                            sheet3.AddMergedRegion(new CellRangeAddress(rows, rows, j + 1, j + 1));
                        }
                    }
                }

                //*合計 lineLast
                SetExcelCell(sheet3, rows + 1, 0, styleHead10, "合計");
                sheet3.AddMergedRegion(new CellRangeAddress(dicldapList3.Count + 3, dicldapList3.Count + 3, 0, 0));
                if (dicldapList3.Count > 0)
                {
                    for (int j = 0; j < dicldapList3.Count; j++)//*初始表格賦初值 
                    {
                        SetExcelCell(sheet3, rows + 1, j + 1, styleHead10, "0");
                        sheet3.AddMergedRegion(new CellRangeAddress(rows + 1, rows + 1, j + 1, j + 1));
                    }
                }
                else
                {
                    SetExcelCell(sheet3, rows + 1, 1, styleHead10, "0");
                    sheet3.AddMergedRegion(new CellRangeAddress(rows + 1, rows + 1, 1, 1));
                }

                #endregion
                #region body3
                caseExcel = "";//案件類型
                rowsExcel = 4;//行數
                rowscountExcel = 0;//最後一列合計
                rowstatolExcel = 0;//總合計  
                for (int iRow = 0; iRow < dtCase3.Rows.Count; iRow++)//根據案件類型進行循環
                {
                    foreach (var item in dicldapList3)
                    {
                        int icols3 = Convert.ToInt32(item.Value.Split('|')[1]);
                        if (dtCase3.Rows[iRow]["New_CaseKind"].ToString() == caseExcel)//重複同一案件類型的數據
                        {
                            if (dtCase3.Rows[iRow]["AgentUser"].ToString() == item.Key)
                            {
                                SetExcelCell(sheet3, rowsExcel, icols3, styleHead10, dtCase3.Rows[iRow]["case_num"].ToString());
                                SetExcelCell(sheet3, rows + 1, icols3, styleHead10, dtCase3.Rows[iRow]["User_Count"].ToString());//最後一行合計
                                rowscountExcelresult = Convert.ToInt32(dtCase3.Rows[iRow]["case_num"].ToString());//每格資料
                                rowscountExcel += rowscountExcelresult;
                                rowstatolExcel += rowscountExcelresult;
                            }
                            SetExcelCell(sheet3, rowsExcel, dicldapList3.Count + 1, styleHead10, rowscountExcel.ToString());//最後一列合計
                        }
                        else//不重複的案件類型
                        {
                            rowscountExcel = 0;
                            rowsExcel = rowsExcel + 1;
                            if (dtCase3.Rows[iRow]["AgentUser"].ToString() == item.Key)
                            {
                                SetExcelCell(sheet3, rowsExcel, icols3, styleHead10, dtCase3.Rows[iRow]["case_num"].ToString());
                                SetExcelCell(sheet3, rows + 1, icols3, styleHead10, dtCase3.Rows[iRow]["User_Count"].ToString());//最後一行合計
                                rowscountExcelresult = Convert.ToInt32(dtCase3.Rows[iRow]["case_num"].ToString());//第一條不重複的數據儲存下值
                                rowscountExcel += rowscountExcelresult;
                                rowstatolExcel += rowscountExcelresult;
                            }
                            caseExcel = dtCase3.Rows[iRow]["New_CaseKind"].ToString();
                            SetExcelCell(sheet3, rowsExcel, dicldapList3.Count + 1, styleHead10, rowscountExcel.ToString());//最後一列合計      
                        }
                    }
                    SetExcelCell(sheet3, rows + 1, dicldapList3.Count + 1, styleHead10, rowstatolExcel.ToString());//總合計
                }
                #endregion
            }
            MemoryStream ms = new MemoryStream();
            workbook.Write(ms);
            ms.Flush();
            ms.Position = 0;
            workbook = null;
            return ms;
            /*
            IWorkbook workbook = new HSSFWorkbook();
            ISheet sheet = null;
            ISheet sheet2 = null;
            ISheet sheet3 = null;
            LdapEmployeeBiz ldap = new LdapEmployeeBiz();
            List<LDAPEmployee> ldapList = new List<LDAPEmployee>();//人員
            List<LDAPEmployee> ldapList2 = new List<LDAPEmployee>();//集作二科
            List<LDAPEmployee> ldapList3 = new List<LDAPEmployee>();//集作三科
            DataTable dtCase = new DataTable();//導出資料
            DataTable dtCase2 = new DataTable();
            DataTable dtCase3 = new DataTable();

            int rowscountExcelresult = 0;//合計參數
            string caseExcel = "";//案件類型
            int rowsExcel = 2;//行數
            int rowscountExcel = 0;//最後一列合計
            int rowstatolExcel = 0;//總合計      

            #region def style
            ICellStyle styleHead12 = workbook.CreateCellStyle();
            IFont font12 = workbook.CreateFont();
            font12.FontHeightInPoints = 12;
            font12.FontName = "新細明體";
            styleHead12.FillPattern = FillPattern.SolidForeground;
            styleHead12.FillForegroundColor = HSSFColor.White.Index;
            styleHead12.BorderTop = BorderStyle.None;
            styleHead12.BorderLeft = BorderStyle.None;
            styleHead12.BorderRight = BorderStyle.None;
            styleHead12.BorderBottom = BorderStyle.None;
            styleHead12.WrapText = true;
            styleHead12.Alignment = HorizontalAlignment.Center;
            styleHead12.VerticalAlignment = VerticalAlignment.Center;
            styleHead12.SetFont(font12);

            ICellStyle styleHead10 = workbook.CreateCellStyle();
            IFont font10 = workbook.CreateFont();
            font10.FontHeightInPoints = 10;
            font10.FontName = "新細明體";
            styleHead10.FillPattern = FillPattern.SolidForeground;
            styleHead10.FillForegroundColor = HSSFColor.White.Index;
            styleHead10.BorderTop = BorderStyle.Thin;
            styleHead10.BorderLeft = BorderStyle.Thin;
            styleHead10.BorderRight = BorderStyle.Thin;
            styleHead10.BorderBottom = BorderStyle.Thin;
            styleHead10.WrapText = true;
            styleHead10.Alignment = HorizontalAlignment.Left;
            styleHead10.VerticalAlignment = VerticalAlignment.Center;
            styleHead10.SetFont(font10);
            #endregion

            #region 單獨科別的數據源(科別及案件資料)
            //獲取人員
            if (model.Depart == "1" || model.Depart == "0")//* 集作一科
            {
                sheet = workbook.CreateSheet("集作一科");
                //ldapList = ldap.GetLdapEmployeeListByDepart("集作一科");//獲取集作一科人員
                dtCase = GetCaseMasterList(model, "集作一科");
                SetExcelCell(sheet, 1, ldapList.Count + 1, styleHead12, "集作一科");
                sheet.AddMergedRegion(new CellRangeAddress(1, 1, ldapList.Count + 1, ldapList.Count + 1));
            }
            if (model.Depart == "2")//* 集作二科
            {
                sheet = workbook.CreateSheet("集作二科");
                //ldapList = ldap.GetLdapEmployeeListByDepart("集作二科");//獲取集作二科人員
                dtCase = GetCaseMasterList(model, "集作二科");
                SetExcelCell(sheet, 1, ldapList.Count + 1, styleHead12, "集作二科");
                sheet.AddMergedRegion(new CellRangeAddress(1, 1, ldapList.Count + 1, ldapList.Count + 1));
            }
            if (model.Depart == "3")//* 集作三科
            {
                sheet = workbook.CreateSheet("集作三科");
                //ldapList = ldap.GetLdapEmployeeListByDepart("集作三科");//獲取集作三科人員
                dtCase = GetCaseMasterList(model, "集作三科");
                SetExcelCell(sheet, 1, ldapList.Count + 1, styleHead12, "集作三科");
                sheet.AddMergedRegion(new CellRangeAddress(1, 1, ldapList.Count + 1, ldapList.Count + 1));
            }
            if (model.Depart == "0")//* 全部
            {
                sheet2 = workbook.CreateSheet("集作二科");
                sheet3 = workbook.CreateSheet("集作三科");
                //ldapList2 = ldap.GetLdapEmployeeListByDepart("集作二科");//獲取集作二科人員
                //ldapList3 = ldap.GetLdapEmployeeListByDepart("集作三科");//獲取集作三科人員
                dtCase2 = GetCaseMasterList(model, "集作二科");//獲取查詢集作二科的案件
                dtCase3 = GetCaseMasterList(model, "集作三科");//獲取查詢集作三科的案件
            }
            #endregion

            string caseKind = "";//*去重複
            int rows = 2;//定義行數

            #region title
            //*大標題 line0
            SetExcelCell(sheet, 0, 0, styleHead12, "經辦結案統計表");
            sheet.AddMergedRegion(new CellRangeAddress(0, 0, 0, ldapList.Count + 1));

            //*查詢條件 line1
            SetExcelCell(sheet, 1, 0, styleHead12, "收件日期：");
            sheet.AddMergedRegion(new CellRangeAddress(1, 1, 0, 0));
            SetExcelCell(sheet, 1, 1, styleHead12, model.ReceiveDateStart + '~' + model.ReceiveDateEnd);
            sheet.AddMergedRegion(new CellRangeAddress(1, 1, 1, 1));
            SetExcelCell(sheet, 1, 2, styleHead12, "發文日期：");
            sheet.AddMergedRegion(new CellRangeAddress(1, 1, 2, 2));
            SetExcelCell(sheet, 1, 3, styleHead12, model.SendDateStart + '~' + model.SendDateEnd);
            sheet.AddMergedRegion(new CellRangeAddress(1, 1, 3, 3));
            SetExcelCell(sheet, 1, 4, styleHead12, "結案日期：");
            sheet.AddMergedRegion(new CellRangeAddress(1, 1, 4, 4));
            SetExcelCell(sheet, 1, 5, styleHead12, model.CloseDateStart + '~' + model.CloseDateEnd);
            sheet.AddMergedRegion(new CellRangeAddress(1, 1, 5, 5));
            SetExcelCell(sheet, 1, ldapList.Count - 1, styleHead12, "部門別/科別");
            sheet.AddMergedRegion(new CellRangeAddress(1, 1, ldapList.Count - 1, ldapList.Count));

            //*結果集表頭 line2
            SetExcelCell(sheet, 2, 0, styleHead10, "處理人員");
            sheet.AddMergedRegion(new CellRangeAddress(2, 2, 0, 0));
            sheet.SetColumnWidth(0, 100 * 50);
            for (int i = 0; i < ldapList.Count; i++)//集作一科人員詳細
            {
                SetExcelCell(sheet, 2, i + 1, styleHead10, ldapList[i].EmpName);
                sheet.AddMergedRegion(new CellRangeAddress(2, 2, i + 1, i + 1));
            }
            SetExcelCell(sheet, 2, ldapList.Count + 1, styleHead10, "合計");
            sheet.AddMergedRegion(new CellRangeAddress(2, 2, ldapList.Count + 1, ldapList.Count + 1));

            //*扣押案件類型 line3-lineN 
            for (int i = 0; i < dtCase.Rows.Count; i++)
            {
                if (caseKind != dtCase.Rows[i]["New_CaseKind"].ToString())
                {
                    rows = rows + 1;
                    SetExcelCell(sheet, rows, 0, styleHead10, dtCase.Rows[i]["New_CaseKind"].ToString());
                    sheet.AddMergedRegion(new CellRangeAddress(rows, rows, 0, 0));
                    SetExcelCell(sheet, rows, ldapList.Count + 1, styleHead10, "0");
                    sheet.AddMergedRegion(new CellRangeAddress(rows, rows, ldapList.Count + 1, ldapList.Count + 1));
                    caseKind = dtCase.Rows[i]["New_CaseKind"].ToString();
                    for (int j = 0; j < ldapList.Count; j++)//*初始表格賦初值 
                    {
                        SetExcelCell(sheet, rows, j + 1, styleHead10, "0");
                        sheet.AddMergedRegion(new CellRangeAddress(rows, rows, j + 1, j + 1));
                    }
                }
            }

            //*合計 lineLast
            SetExcelCell(sheet, rows + 1, 0, styleHead10, "合計");
            sheet.AddMergedRegion(new CellRangeAddress(ldapList.Count + 3, ldapList.Count + 3, 0, 0));
            for (int j = 0; j < ldapList.Count; j++)//*初始表格賦初值 
            {
                SetExcelCell(sheet, rows + 1, j + 1, styleHead10, "0");
                sheet.AddMergedRegion(new CellRangeAddress(rows + 1, rows + 1, j + 1, j + 1));
            }
            #endregion

            #region  body
            for (int iRow = 0; iRow < dtCase.Rows.Count; iRow++)//根據案件類型進行循環
            {
                for (int i = 0; i < ldapList.Count; i++)//循環excel中的列
                {
                    if (caseExcel == dtCase.Rows[iRow]["New_CaseKind"].ToString())//重複同一案件類型的數據
                    {
                        if (ldapList[i].EmpName == dtCase.Rows[iRow]["EmpName"].ToString())
                        {
                            SetExcelCell(sheet, rowsExcel, i + 1, styleHead10, dtCase.Rows[iRow]["case_num"].ToString());
                            rowscountExcelresult = Convert.ToInt32(dtCase.Rows[iRow]["case_num"].ToString());//每格資料
                            SetExcelCell(sheet, rows + 1, i + 1, styleHead10, dtCase.Rows[iRow]["UserCount"].ToString());//最後一行合計
                            rowscountExcel += rowscountExcelresult;
                            rowstatolExcel += rowscountExcelresult;
                        }
                        SetExcelCell(sheet, rowsExcel, ldapList.Count + 1, styleHead10, rowscountExcel.ToString());//最後一列合計
                    }
                    else//不重複的案件類型
                    {
                        rowscountExcel = 0;
                        rowsExcel = rowsExcel + 1;
                        if (dtCase.Rows[iRow]["EmpName"].ToString() == ldapList[i].EmpName)
                        {
                            SetExcelCell(sheet, rowsExcel, i + 1, styleHead10, dtCase.Rows[iRow]["case_num"].ToString());
                            rowscountExcelresult = Convert.ToInt32(dtCase.Rows[iRow]["case_num"].ToString());//第一條不重複的數據儲存下值
                            SetExcelCell(sheet, rows + 1, i + 1, styleHead10, dtCase.Rows[iRow]["UserCount"].ToString());//最後一行合計
                            rowscountExcel += rowscountExcelresult;
                            rowstatolExcel += rowscountExcelresult;
                        }
                        caseExcel = dtCase.Rows[iRow]["New_CaseKind"].ToString();
                        SetExcelCell(sheet, rowsExcel, ldapList.Count + 1, styleHead10, rowscountExcel.ToString());//最後一列合計
                    }
                }
            }
            SetExcelCell(sheet, rows + 1, ldapList.Count + 1, styleHead10, rowstatolExcel.ToString());//總合計
            #endregion

            if (model.Depart == "0")//* 全部
            {
                #region title2
                //*大標題 line0
                SetExcelCell(sheet2, 0, 0, styleHead12, "經辦結案統計表");
                sheet2.AddMergedRegion(new CellRangeAddress(0, 0, 0, ldapList2.Count + 1));

                //*查詢條件 line1
                SetExcelCell(sheet2, 1, 0, styleHead12, "收件日期：");
                sheet2.AddMergedRegion(new CellRangeAddress(1, 1, 0, 0));
                SetExcelCell(sheet2, 1, 1, styleHead12, model.ReceiveDateStart + '~' + model.ReceiveDateEnd);
                sheet2.AddMergedRegion(new CellRangeAddress(1, 1, 1, 1));
                SetExcelCell(sheet2, 1, 2, styleHead12, "發文日期：");
                sheet2.AddMergedRegion(new CellRangeAddress(1, 1, 2, 2));
                SetExcelCell(sheet2, 1, 3, styleHead12, model.SendDateStart + '~' + model.SendDateEnd);
                sheet2.AddMergedRegion(new CellRangeAddress(1, 1, 3, 3));
                SetExcelCell(sheet2, 1, 4, styleHead12, "結案日期：");
                sheet2.AddMergedRegion(new CellRangeAddress(1, 1, 4, 4));
                SetExcelCell(sheet2, 1, 5, styleHead12, model.CloseDateStart + '~' + model.CloseDateEnd);
                sheet2.AddMergedRegion(new CellRangeAddress(1, 1, 5, 5));
                SetExcelCell(sheet2, 1, ldapList2.Count - 1, styleHead12, "部門別/科別");
                sheet2.AddMergedRegion(new CellRangeAddress(1, 1, ldapList2.Count - 1, ldapList2.Count));
                SetExcelCell(sheet2, 1, ldapList2.Count + 1, styleHead12, "集作二科");
                sheet2.AddMergedRegion(new CellRangeAddress(1, 1, ldapList2.Count + 1, ldapList2.Count + 1));

                //*結果集表頭 line2
                SetExcelCell(sheet2, 2, 0, styleHead10, "處理人員");
                sheet2.AddMergedRegion(new CellRangeAddress(2, 2, 0, 0));
                sheet2.SetColumnWidth(0, 100 * 50);
                for (int i = 0; i < ldapList2.Count; i++)//集作一科人員詳細
                {
                    SetExcelCell(sheet2, 2, i + 1, styleHead10, ldapList2[i].EmpName);
                    sheet2.AddMergedRegion(new CellRangeAddress(2, 2, i + 1, i + 1));
                }
                SetExcelCell(sheet2, 2, ldapList2.Count + 1, styleHead10, "合計");
                sheet2.AddMergedRegion(new CellRangeAddress(2, 2, ldapList2.Count + 1, ldapList2.Count + 1));

                //*扣押案件類型 line3-lineN 
                caseKind = "";//*去重複
                rows = 2;//定義行數
                for (int i = 0; i < dtCase2.Rows.Count; i++)
                {
                    if (caseKind != dtCase2.Rows[i]["New_CaseKind"].ToString())
                    {
                        rows = rows + 1;
                        SetExcelCell(sheet2, rows, 0, styleHead10, dtCase2.Rows[i]["New_CaseKind"].ToString());
                        sheet2.AddMergedRegion(new CellRangeAddress(rows, rows, 0, 0));

                        SetExcelCell(sheet2, rows, ldapList2.Count + 1, styleHead10, "0");
                        sheet2.AddMergedRegion(new CellRangeAddress(rows, rows, ldapList2.Count + 1, ldapList2.Count + 1));

                        caseKind = dtCase2.Rows[i]["New_CaseKind"].ToString();
                        for (int j = 0; j < ldapList2.Count; j++)//*初始表格賦初值 
                        {
                            SetExcelCell(sheet2, rows, j + 1, styleHead10, "0");
                            sheet2.AddMergedRegion(new CellRangeAddress(rows, rows, j + 1, j + 1));
                        }
                    }
                }

                //*合計 lineLast
                SetExcelCell(sheet2, rows + 1, 0, styleHead10, "合計");
                sheet2.AddMergedRegion(new CellRangeAddress(ldapList2.Count + 3, ldapList2.Count + 3, 0, 0));
                for (int j = 0; j < ldapList2.Count; j++)//*初始表格賦初值 
                {
                    SetExcelCell(sheet2, rows + 1, j + 1, styleHead10, "0");
                    sheet2.AddMergedRegion(new CellRangeAddress(rows + 1, rows + 1, j + 1, j + 1));
                }
                #endregion
                #region body2
                caseExcel = "";//案件類型
                rowsExcel = 2;//行數
                rowscountExcel = 0;//最後一列合計
                rowstatolExcel = 0;//總合計      
                for (int iRow = 0; iRow < dtCase2.Rows.Count; iRow++)//根據案件類型進行循環
                {
                    for (int i = 0; i < ldapList2.Count; i++)//循環excel中的列
                    {
                        if (dtCase2.Rows[iRow]["New_CaseKind"].ToString() == caseExcel)//重複同一案件類型的數據
                        {
                            if (dtCase2.Rows[iRow]["EmpName"].ToString() == ldapList2[i].EmpName)
                            {
                                SetExcelCell(sheet2, rowsExcel, i + 1, styleHead10, dtCase2.Rows[iRow]["case_num"].ToString());
                                SetExcelCell(sheet2, rows + 1, i + 1, styleHead10, dtCase2.Rows[iRow]["UserCount"].ToString());//最後一行合計
                                rowscountExcelresult = Convert.ToInt32(dtCase2.Rows[iRow]["case_num"].ToString());//每格資料
                                rowscountExcel += rowscountExcelresult;
                                rowstatolExcel += rowscountExcelresult;
                            }
                            SetExcelCell(sheet2, rowsExcel, ldapList2.Count + 1, styleHead10, rowscountExcel.ToString());//最後一列合計
                        }
                        else//不重複的案件類型
                        {
                            rowscountExcel = 0;
                            rowsExcel = rowsExcel + 1;
                            if (dtCase2.Rows[iRow]["EmpName"].ToString() == ldapList2[i].EmpName)
                            {
                                SetExcelCell(sheet2, rowsExcel, i + 1, styleHead10, dtCase2.Rows[iRow]["case_num"].ToString());
                                SetExcelCell(sheet2, rows + 1, i + 1, styleHead10, dtCase2.Rows[iRow]["UserCount"].ToString());//最後一行合計
                                rowscountExcelresult = Convert.ToInt32(dtCase2.Rows[iRow]["case_num"].ToString());//第一條不重複的數據儲存下值
                                rowscountExcel += rowscountExcelresult;
                                rowstatolExcel += rowscountExcelresult;
                            }
                            caseExcel = dtCase2.Rows[iRow]["New_CaseKind"].ToString();
                            SetExcelCell(sheet2, rowsExcel, ldapList2.Count + 1, styleHead10, rowscountExcel.ToString());//最後一列合計      
                        }
                    }
                }
                SetExcelCell(sheet2, rows + 1, ldapList2.Count + 1, styleHead10, rowstatolExcel.ToString());//總合計
                #endregion

                #region title3
                //*大標題 line0
                SetExcelCell(sheet3, 0, 0, styleHead12, "經辦結案統計表");
                sheet3.AddMergedRegion(new CellRangeAddress(0, 0, 0, ldapList3.Count + 1));

                //*查詢條件 line1
                SetExcelCell(sheet3, 1, 0, styleHead12, "收件日期：");
                sheet3.AddMergedRegion(new CellRangeAddress(1, 1, 0, 0));
                SetExcelCell(sheet3, 1, 1, styleHead12, model.ReceiveDateStart + '~' + model.ReceiveDateEnd);
                sheet3.AddMergedRegion(new CellRangeAddress(1, 1, 1, 1));
                SetExcelCell(sheet3, 1, 2, styleHead12, "發文日期：");
                sheet3.AddMergedRegion(new CellRangeAddress(1, 1, 2, 2));
                SetExcelCell(sheet3, 1, 3, styleHead12, model.SendDateStart + '~' + model.SendDateEnd);
                sheet3.AddMergedRegion(new CellRangeAddress(1, 1, 3, 3));
                SetExcelCell(sheet3, 1, 4, styleHead12, "結案日期：");
                sheet3.AddMergedRegion(new CellRangeAddress(1, 1, 4, 4));
                SetExcelCell(sheet3, 1, 5, styleHead12, model.CloseDateStart + '~' + model.CloseDateEnd);
                sheet3.AddMergedRegion(new CellRangeAddress(1, 1, 5, 5));
                SetExcelCell(sheet3, 1, ldapList3.Count - 1, styleHead12, "部門別/科別");
                sheet3.AddMergedRegion(new CellRangeAddress(1, 1, ldapList3.Count - 1, ldapList3.Count));
                SetExcelCell(sheet3, 1, ldapList3.Count + 1, styleHead12, "集作三科");
                sheet3.AddMergedRegion(new CellRangeAddress(1, 1, ldapList3.Count + 1, ldapList3.Count + 1));

                //*結果集表頭 line2
                SetExcelCell(sheet3, 2, 0, styleHead10, "處理人員");
                sheet3.AddMergedRegion(new CellRangeAddress(2, 2, 0, 0));
                sheet3.SetColumnWidth(0, 100 * 50);
                for (int i = 0; i < ldapList3.Count; i++)//集作一科人員詳細
                {
                    SetExcelCell(sheet3, 2, i + 1, styleHead10, ldapList3[i].EmpName);
                    sheet3.AddMergedRegion(new CellRangeAddress(2, 2, i + 1, i + 1));
                }
                SetExcelCell(sheet3, 2, ldapList3.Count + 1, styleHead10, "合計");
                sheet3.AddMergedRegion(new CellRangeAddress(2, 2, ldapList3.Count + 1, ldapList3.Count + 1));

                //*扣押案件類型 line3-lineN 
                caseKind = "";//*去重複
                rows = 2;//定義行數
                for (int i = 0; i < dtCase3.Rows.Count; i++)
                {
                    if (caseKind != dtCase3.Rows[i]["New_CaseKind"].ToString())
                    {
                        rows = rows + 1;
                        SetExcelCell(sheet3, rows, 0, styleHead10, dtCase3.Rows[i]["New_CaseKind"].ToString());
                        sheet3.AddMergedRegion(new CellRangeAddress(rows, rows, 0, 0));
                        SetExcelCell(sheet3, rows, ldapList3.Count + 1, styleHead10, "0");
                        sheet3.AddMergedRegion(new CellRangeAddress(rows, rows, ldapList3.Count + 1, ldapList3.Count + 1));
                        caseKind = dtCase3.Rows[i]["New_CaseKind"].ToString();
                        for (int j = 0; j < ldapList3.Count; j++)//*初始表格賦初值 
                        {
                            SetExcelCell(sheet3, rows, j + 1, styleHead10, "0");
                            sheet3.AddMergedRegion(new CellRangeAddress(rows, rows, j + 1, j + 1));
                        }
                    }
                }

                //*合計 lineLast
                SetExcelCell(sheet3, rows + 1, 0, styleHead10, "合計");
                sheet3.AddMergedRegion(new CellRangeAddress(ldapList3.Count + 3, ldapList3.Count + 3, 0, 0));
                for (int j = 0; j < ldapList3.Count; j++)//*初始表格賦初值 
                {
                    SetExcelCell(sheet3, rows + 1, j + 1, styleHead10, "0");
                    sheet3.AddMergedRegion(new CellRangeAddress(rows + 1, rows + 1, j + 1, j + 1));
                }
                #endregion
                #region body3
                caseExcel = "";//案件類型
                rowsExcel = 2;//行數
                rowscountExcel = 0;//最後一列合計
                rowstatolExcel = 0;//總合計  
                for (int iRow = 0; iRow < dtCase3.Rows.Count; iRow++)//根據案件類型進行循環
                {
                    for (int i = 0; i < ldapList3.Count; i++)//循環excel中的列
                    {
                        if (dtCase3.Rows[iRow]["New_CaseKind"].ToString() == caseExcel)//重複同一案件類型的數據
                        {
                            if (dtCase3.Rows[iRow]["EmpName"].ToString() == ldapList3[i].EmpName)
                            {
                                SetExcelCell(sheet3, rowsExcel, i + 1, styleHead10, dtCase3.Rows[iRow]["case_num"].ToString());
                                SetExcelCell(sheet3, rows + 1, i + 1, styleHead10, dtCase3.Rows[iRow]["UserCount"].ToString());//最後一行合計
                                rowscountExcelresult = Convert.ToInt32(dtCase3.Rows[iRow]["case_num"].ToString());//每格資料
                                rowscountExcel += rowscountExcelresult;
                                rowstatolExcel += rowscountExcelresult;
                            }
                            SetExcelCell(sheet3, rowsExcel, ldapList3.Count + 1, styleHead10, rowscountExcel.ToString());//最後一列合計
                        }
                        else//不重複的案件類型
                        {
                            rowscountExcel = 0;
                            rowsExcel = rowsExcel + 1;
                            if (dtCase3.Rows[iRow]["EmpName"].ToString() == ldapList3[i].EmpName)
                            {
                                SetExcelCell(sheet3, rowsExcel, i + 1, styleHead10, dtCase3.Rows[iRow]["case_num"].ToString());
                                SetExcelCell(sheet3, rows + 1, i + 1, styleHead10, dtCase3.Rows[iRow]["UserCount"].ToString());//最後一行合計
                                rowscountExcelresult = Convert.ToInt32(dtCase3.Rows[iRow]["case_num"].ToString());//第一條不重複的數據儲存下值
                                rowscountExcel += rowscountExcelresult;
                                rowstatolExcel += rowscountExcelresult;
                            }
                            caseExcel = dtCase3.Rows[iRow]["New_CaseKind"].ToString();
                            SetExcelCell(sheet3, rowsExcel, ldapList3.Count + 1, styleHead10, rowscountExcel.ToString());//最後一列合計      
                        }
                    }
                }
                SetExcelCell(sheet3, rows + 1, ldapList3.Count + 1, styleHead10, rowstatolExcel.ToString());//總合計
                #endregion
            }

            MemoryStream ms = new MemoryStream();
            workbook.Write(ms);
            ms.Flush();
            ms.Position = 0;
            workbook = null;
            return ms;
             * */
        }
        #endregion
        #region 品質時效統計表
        public MemoryStream TradeListReportExcel_NPOI(CaseClosedQuery model)
        {
            IWorkbook workbook = new HSSFWorkbook();
            ISheet sheet = null;
            ISheet sheet2 = null;
            ISheet sheet3 = null;

            DataTable dt = new DataTable();
            DataTable dt2 = new DataTable();
            DataTable dt3 = new DataTable();

            #region def style
            ICellStyle styleHead12 = workbook.CreateCellStyle();
            IFont font12 = workbook.CreateFont();
            font12.FontHeightInPoints = 12;
            font12.FontName = "新細明體";
            styleHead12.FillPattern = FillPattern.SolidForeground;
            styleHead12.FillForegroundColor = HSSFColor.White.Index;
            styleHead12.BorderTop = BorderStyle.None;
            styleHead12.BorderLeft = BorderStyle.None;
            styleHead12.BorderRight = BorderStyle.None;
            styleHead12.BorderBottom = BorderStyle.None;
            styleHead12.WrapText = true;
            styleHead12.Alignment = HorizontalAlignment.Center;
            styleHead12.VerticalAlignment = VerticalAlignment.Center;
            styleHead12.SetFont(font12);

            ICellStyle styleHead10 = workbook.CreateCellStyle();
            IFont font10 = workbook.CreateFont();
            font10.FontHeightInPoints = 10;
            font10.FontName = "新細明體";
            styleHead10.FillPattern = FillPattern.SolidForeground;
            styleHead10.FillForegroundColor = HSSFColor.White.Index;
            styleHead10.BorderTop = BorderStyle.Thin;
            styleHead10.BorderLeft = BorderStyle.Thin;
            styleHead10.BorderRight = BorderStyle.Thin;
            styleHead10.BorderBottom = BorderStyle.Thin;
            styleHead10.WrapText = true;
            styleHead10.Alignment = HorizontalAlignment.Left;
            styleHead10.VerticalAlignment = VerticalAlignment.Center;
            styleHead10.SetFont(font10);
            #endregion

            #region 獲取數據源(集作一科及案件資料)
            //獲取人員
            if (model.Depart == "1")//* 集作一科
            {
                sheet = workbook.CreateSheet("集作一科");
                dt = GetTradeList(model, "集作一科");//獲取查詢集作一科的案件
                SetExcelCell(sheet, 1, 7, styleHead12, "集作一科");
                sheet.AddMergedRegion(new CellRangeAddress(1, 1, 7, 7));
            }
            if (model.Depart == "2")//* 集作二科
            {
                sheet = workbook.CreateSheet("集作二科");
                dt = GetTradeList(model, "集作二科");//獲取查詢集作二科的案件
                SetExcelCell(sheet, 1, 7, styleHead12, "集作二科");
                sheet.AddMergedRegion(new CellRangeAddress(1, 1, 7, 7));
            }
            if (model.Depart == "3")//*集作三科
            {
                sheet = workbook.CreateSheet("集作三科");
                dt = GetTradeList(model, "集作三科");//獲取查詢集作三科的案件
                SetExcelCell(sheet, 1, 7, styleHead12, "集作三科");
                sheet.AddMergedRegion(new CellRangeAddress(1, 1, 7, 7));
            }
            if (model.Depart == "0")//*全部
            {
                sheet = workbook.CreateSheet("集作一科");
                sheet2 = workbook.CreateSheet("集作二科");
                sheet3 = workbook.CreateSheet("集作三科");
                dt = GetTradeList(model, "集作一科");//獲取查詢集作一科的案件
                dt2 = GetTradeList(model, "集作二科");//獲取查詢集作二科的案件
                dt3 = GetTradeList(model, "集作三科");//獲取查詢集作三科的案件
                SetExcelCell(sheet, 1, 7, styleHead12, "集作一科");
                sheet.AddMergedRegion(new CellRangeAddress(1, 1, 7, 7));
                SetExcelCell(sheet2, 1, 7, styleHead12, "集作二科");
                sheet2.AddMergedRegion(new CellRangeAddress(1, 1, 7, 7));
                SetExcelCell(sheet3, 1, 7, styleHead12, "集作三科");
                sheet3.AddMergedRegion(new CellRangeAddress(1, 1, 7, 7));
            }
            #endregion

            #region title
            SetExcelCell(sheet, 0, 0, styleHead12, "品質時效統計表");
            sheet.AddMergedRegion(new CellRangeAddress(0, 0, 0, 6));

            //* line1
            SetExcelCell(sheet, 1, 0, styleHead12, "收件日期：");
            sheet.AddMergedRegion(new CellRangeAddress(1, 1, 0, 0));
            SetExcelCell(sheet, 1, 1, styleHead12, model.ReceiveDateStart + '~' + model.ReceiveDateEnd);
            sheet.AddMergedRegion(new CellRangeAddress(1, 1, 1, 1));
            SetExcelCell(sheet, 1, 2, styleHead12, "發文日期：");
            sheet.AddMergedRegion(new CellRangeAddress(1, 1, 2, 2));
            SetExcelCell(sheet, 1, 3, styleHead12, model.SendDateStart + '~' + model.SendDateEnd);
            sheet.AddMergedRegion(new CellRangeAddress(1, 1, 3, 3));
            SetExcelCell(sheet, 1, 4, styleHead12, "結案日期：");
            sheet.AddMergedRegion(new CellRangeAddress(1, 1, 4, 4));
            SetExcelCell(sheet, 1, 5, styleHead12, model.CloseDateStart + '~' + model.CloseDateEnd);
            sheet.AddMergedRegion(new CellRangeAddress(1, 1, 5, 5));
            SetExcelCell(sheet, 1, 6, styleHead12, "部門別/科別:");
            sheet.AddMergedRegion(new CellRangeAddress(1, 1, 6, 6));

            //* line2
            SetExcelCell(sheet, 2, 0, styleHead10, "案件類別-大類");
            sheet.AddMergedRegion(new CellRangeAddress(2, 2, 0, 0));
            SetExcelCell(sheet, 2, 1, styleHead10, "經辦人員");
            sheet.AddMergedRegion(new CellRangeAddress(2, 2, 1, 1));
            SetExcelCell(sheet, 2, 2, styleHead10, "件數");
            sheet.AddMergedRegion(new CellRangeAddress(2, 2, 2, 2));
            SetExcelCell(sheet, 2, 3, styleHead10, "退件件數");
            sheet.AddMergedRegion(new CellRangeAddress(2, 2, 3, 3));
            SetExcelCell(sheet, 2, 4, styleHead10, "退件率");
            sheet.AddMergedRegion(new CellRangeAddress(2, 2, 4, 4));
            SetExcelCell(sheet, 2, 5, styleHead10, "逾期件數");
            sheet.AddMergedRegion(new CellRangeAddress(2, 2, 5, 5));
            SetExcelCell(sheet, 2, 6, styleHead10, "逾期率");
            sheet.AddMergedRegion(new CellRangeAddress(2, 2, 6, 6));

            SetExcelCell(sheet, dt.Rows.Count + 3, 1, styleHead12, "合計");
            sheet.AddMergedRegion(new CellRangeAddress(dt.Rows.Count + 3, dt.Rows.Count + 3, 1, 1));

            for (int i = 0; i < dt.Rows.Count + 1; i++)//*初始表格賦初值 
            {
                for (int j = 1; j < 6; j++)
                {
                    SetExcelCell(sheet, i + 3, j + 1, styleHead12, "0");
                    sheet.AddMergedRegion(new CellRangeAddress(i + 3, i + 3, j + 1, j + 1));
                }
            }
            #endregion
            #region Width
            sheet.SetColumnWidth(0, 100 * 40);
            sheet.SetColumnWidth(1, 100 * 40);
            sheet.SetColumnWidth(2, 100 * 40);
            sheet.SetColumnWidth(3, 100 * 40);
            sheet.SetColumnWidth(4, 100 * 40);
            sheet.SetColumnWidth(5, 100 * 40);
            sheet.SetColumnWidth(6, 100 * 40);
            sheet.SetColumnWidth(7, 100 * 40);
            #endregion
            #region body
            int totalNum = 0;
            int totalNumCount = 0;
            int totalReturnNum = 0;
            int totalReturnNumCount = 0;
            int totalOverDue = 0;
            int totalOverDueCount = 0;
            for (int iRow = 0; iRow < dt.Rows.Count; iRow++)
            {
                for (int iCol = 0; iCol < dt.Columns.Count; iCol++)
                {
                    SetExcelCell(sheet, iRow + 3, iCol, styleHead10, dt.Rows[iRow][iCol].ToString());
                }
                totalNum = Convert.ToInt32(dt.Rows[iRow]["Case"].ToString());
                totalNumCount += totalNum;
                totalReturnNum = Convert.ToInt32(dt.Rows[iRow]["ReturnCase"].ToString());
                totalReturnNumCount += totalReturnNum;
                totalOverDue = Convert.ToInt32(dt.Rows[iRow]["OutCase"].ToString());
                totalOverDueCount += totalOverDue;
            }
            SetExcelCell(sheet, dt.Rows.Count + 3, 2, styleHead12, totalNumCount.ToString());
            SetExcelCell(sheet, dt.Rows.Count + 3, 3, styleHead12, totalReturnNumCount.ToString());
            if (totalReturnNumCount == 0)
            {
                SetExcelCell(sheet, dt.Rows.Count + 3, 4, styleHead12, "0.00%");
            }
            else
            {
                string result = (((totalReturnNumCount * 1.0) / totalNumCount) * 100).ToString();
                if (result.ToString().IndexOf(".") > 0)
                {
                    SetExcelCell(sheet, dt.Rows.Count + 3, 4, styleHead12, (result.Substring(0, result.IndexOf(".") + 3)) + "%");
                }
                else
                {
                    SetExcelCell(sheet, dt.Rows.Count + 3, 4, styleHead12, result + ".00%");
                }
            }
            SetExcelCell(sheet, dt.Rows.Count + 3, 5, styleHead12, totalOverDueCount.ToString());
            if (totalOverDueCount == 0)
            {
                SetExcelCell(sheet, dt.Rows.Count + 3, 6, styleHead12, "0.00%");
            }
            else
            {
                string results = (((totalOverDueCount * 1.0) / totalNumCount) * 100).ToString();
                if (results.ToString().IndexOf(".") > 0)
                {
                    SetExcelCell(sheet, dt.Rows.Count + 3, 6, styleHead12, (results.Substring(0, results.IndexOf(".") + 3)) + "%");
                }
                else
                {
                    SetExcelCell(sheet, dt.Rows.Count + 3, 6, styleHead12, results + ".00%");
                }
            }
            #endregion

            if (model.Depart == "0")//* 全部
            {
                #region title2
                SetExcelCell(sheet2, 0, 0, styleHead12, "品質時效統計表");
                sheet2.AddMergedRegion(new CellRangeAddress(0, 0, 0, 6));

                //* line1
                SetExcelCell(sheet2, 1, 0, styleHead12, "收件日期：");
                sheet2.AddMergedRegion(new CellRangeAddress(1, 1, 0, 0));
                SetExcelCell(sheet2, 1, 1, styleHead12, model.ReceiveDateStart + '~' + model.ReceiveDateEnd);
                sheet2.AddMergedRegion(new CellRangeAddress(1, 1, 1, 1));
                SetExcelCell(sheet2, 1, 2, styleHead12, "發文日期：");
                sheet2.AddMergedRegion(new CellRangeAddress(1, 1, 2, 2));
                SetExcelCell(sheet2, 1, 3, styleHead12, model.SendDateStart + '~' + model.SendDateEnd);
                sheet2.AddMergedRegion(new CellRangeAddress(1, 1, 3, 3));
                SetExcelCell(sheet2, 1, 4, styleHead12, "結案日期：");
                sheet2.AddMergedRegion(new CellRangeAddress(1, 1, 4, 4));
                SetExcelCell(sheet2, 1, 5, styleHead12, model.CloseDateStart + '~' + model.CloseDateEnd);
                sheet2.AddMergedRegion(new CellRangeAddress(1, 1, 5, 5));
                SetExcelCell(sheet2, 1, 6, styleHead12, "部門別/科別:");
                sheet2.AddMergedRegion(new CellRangeAddress(1, 1, 6, 6));

                //* line2
                SetExcelCell(sheet2, 2, 0, styleHead10, "案件類別-大類");
                sheet2.AddMergedRegion(new CellRangeAddress(2, 2, 0, 0));
                SetExcelCell(sheet2, 2, 1, styleHead10, "經辦人員");
                sheet2.AddMergedRegion(new CellRangeAddress(2, 2, 1, 1));
                SetExcelCell(sheet2, 2, 2, styleHead10, "件數");
                sheet2.AddMergedRegion(new CellRangeAddress(2, 2, 2, 2));
                SetExcelCell(sheet2, 2, 3, styleHead10, "退件件數");
                sheet2.AddMergedRegion(new CellRangeAddress(2, 2, 3, 3));
                SetExcelCell(sheet2, 2, 4, styleHead10, "退件率");
                sheet2.AddMergedRegion(new CellRangeAddress(2, 2, 4, 4));
                SetExcelCell(sheet2, 2, 5, styleHead10, "逾期件數");
                sheet2.AddMergedRegion(new CellRangeAddress(2, 2, 5, 5));
                SetExcelCell(sheet2, 2, 6, styleHead10, "逾期率");
                sheet2.AddMergedRegion(new CellRangeAddress(2, 2, 6, 6));

                SetExcelCell(sheet2, dt2.Rows.Count + 3, 1, styleHead12, "合計");
                sheet2.AddMergedRegion(new CellRangeAddress(dt2.Rows.Count + 3, dt2.Rows.Count + 3, 1, 1));

                for (int i = 0; i < dt2.Rows.Count + 1; i++)//*初始表格賦初值 
                {
                    for (int j = 1; j < 6; j++)
                    {
                        SetExcelCell(sheet2, i + 3, j + 1, styleHead12, "0");
                        sheet2.AddMergedRegion(new CellRangeAddress(i + 3, i + 3, j + 1, j + 1));
                    }
                }
                #endregion

                #region title3
                SetExcelCell(sheet3, 0, 0, styleHead12, "品質時效統計表");
                sheet3.AddMergedRegion(new CellRangeAddress(0, 0, 0, 6));

                //* line1
                SetExcelCell(sheet3, 1, 0, styleHead12, "收件日期：");
                sheet3.AddMergedRegion(new CellRangeAddress(1, 1, 0, 0));
                SetExcelCell(sheet3, 1, 1, styleHead12, model.ReceiveDateStart + '~' + model.ReceiveDateEnd);
                sheet3.AddMergedRegion(new CellRangeAddress(1, 1, 1, 1));
                SetExcelCell(sheet3, 1, 2, styleHead12, "發文日期：");
                sheet3.AddMergedRegion(new CellRangeAddress(1, 1, 2, 2));
                SetExcelCell(sheet3, 1, 3, styleHead12, model.SendDateStart + '~' + model.SendDateEnd);
                sheet3.AddMergedRegion(new CellRangeAddress(1, 1, 3, 3));
                SetExcelCell(sheet3, 1, 4, styleHead12, "結案日期：");
                sheet3.AddMergedRegion(new CellRangeAddress(1, 1, 4, 4));
                SetExcelCell(sheet3, 1, 5, styleHead12, model.CloseDateStart + '~' + model.CloseDateEnd);
                sheet3.AddMergedRegion(new CellRangeAddress(1, 1, 5, 5));
                SetExcelCell(sheet3, 1, 6, styleHead12, "部門別/科別:");
                sheet3.AddMergedRegion(new CellRangeAddress(1, 1, 6, 6));

                //* line2
                SetExcelCell(sheet3, 2, 0, styleHead10, "案件類別-大類");
                sheet3.AddMergedRegion(new CellRangeAddress(2, 2, 0, 0));
                SetExcelCell(sheet3, 2, 1, styleHead10, "經辦人員");
                sheet3.AddMergedRegion(new CellRangeAddress(2, 2, 1, 1));
                SetExcelCell(sheet3, 2, 2, styleHead10, "件數");
                sheet3.AddMergedRegion(new CellRangeAddress(2, 2, 2, 2));
                SetExcelCell(sheet3, 2, 3, styleHead10, "退件件數");
                sheet3.AddMergedRegion(new CellRangeAddress(2, 2, 3, 3));
                SetExcelCell(sheet3, 2, 4, styleHead10, "退件率");
                sheet3.AddMergedRegion(new CellRangeAddress(2, 2, 4, 4));
                SetExcelCell(sheet3, 2, 5, styleHead10, "逾期件數");
                sheet3.AddMergedRegion(new CellRangeAddress(2, 2, 5, 5));
                SetExcelCell(sheet3, 2, 6, styleHead10, "逾期率");
                sheet3.AddMergedRegion(new CellRangeAddress(2, 2, 6, 6));

                SetExcelCell(sheet3, dt3.Rows.Count + 3, 1, styleHead12, "合計");
                sheet3.AddMergedRegion(new CellRangeAddress(dt3.Rows.Count + 3, dt3.Rows.Count + 3, 1, 1));

                for (int i = 0; i < dt3.Rows.Count + 1; i++)//*初始表格賦初值 
                {
                    for (int j = 1; j < 6; j++)
                    {
                        SetExcelCell(sheet3, i + 3, j + 1, styleHead12, "0");
                        sheet3.AddMergedRegion(new CellRangeAddress(i + 3, i + 3, j + 1, j + 1));
                    }
                }
                #endregion
                #region Width2
                sheet2.SetColumnWidth(0, 100 * 40);
                sheet2.SetColumnWidth(1, 100 * 40);
                sheet2.SetColumnWidth(2, 100 * 40);
                sheet2.SetColumnWidth(3, 100 * 40);
                sheet2.SetColumnWidth(4, 100 * 40);
                sheet2.SetColumnWidth(5, 100 * 40);
                sheet2.SetColumnWidth(6, 100 * 40);
                sheet2.SetColumnWidth(7, 100 * 40);
                #endregion
                #region Width3
                sheet3.SetColumnWidth(0, 100 * 40);
                sheet3.SetColumnWidth(1, 100 * 40);
                sheet3.SetColumnWidth(2, 100 * 40);
                sheet3.SetColumnWidth(3, 100 * 40);
                sheet3.SetColumnWidth(4, 100 * 40);
                sheet3.SetColumnWidth(5, 100 * 40);
                sheet3.SetColumnWidth(6, 100 * 40);
                sheet3.SetColumnWidth(7, 100 * 40);
                #endregion
                #region body2
                int totalNums = 0;
                int totalNumCounts = 0;
                int totalReturnNums = 0;
                int totalReturnNumCounts = 0;
                int totalOverDues = 0;
                int totalOverDueCounts = 0;
                for (int iRow2 = 0; iRow2 < dt2.Rows.Count; iRow2++)
                {
                    for (int iCol = 0; iCol < dt2.Columns.Count; iCol++)
                    {
                        SetExcelCell(sheet2, iRow2 + 3, iCol, styleHead10, dt2.Rows[iRow2][iCol].ToString());
                    }
                    totalNums = Convert.ToInt32(dt2.Rows[iRow2]["Case"].ToString());
                    totalNumCounts += totalNums;
                    totalReturnNums = Convert.ToInt32(dt2.Rows[iRow2]["ReturnCase"].ToString());
                    totalReturnNumCounts += totalReturnNums;
                    totalOverDues = Convert.ToInt32(dt2.Rows[iRow2]["OutCase"].ToString());
                    totalOverDueCounts += totalOverDues;
                }
                SetExcelCell(sheet2, dt2.Rows.Count + 3, 2, styleHead12, totalNumCounts.ToString());
                SetExcelCell(sheet2, dt2.Rows.Count + 3, 3, styleHead12, totalReturnNumCounts.ToString());
                if (totalReturnNumCounts == 0)
                {
                    SetExcelCell(sheet2, dt2.Rows.Count + 3, 4, styleHead12, "0.00%");
                }
                else
                {
                    string tuijian = (((totalReturnNumCounts * 1.0) / totalNumCounts) * 100).ToString();
                    if (tuijian.ToString().IndexOf(".") > 0)
                    {
                        SetExcelCell(sheet2, dt2.Rows.Count + 3, 4, styleHead12, (tuijian.Substring(0, tuijian.IndexOf(".") + 3)) + "%");
                    }
                    else
                    {
                        SetExcelCell(sheet2, dt2.Rows.Count + 3, 4, styleHead12, tuijian + ".00%");
                    }
                }
                SetExcelCell(sheet2, dt2.Rows.Count + 3, 5, styleHead12, totalOverDueCounts.ToString());
                if (totalOverDueCounts == 0)
                {
                    SetExcelCell(sheet2, dt2.Rows.Count + 3, 6, styleHead12, "0.00%");
                }
                else
                {
                    string results = (((totalOverDueCounts * 1.0) / totalNumCounts) * 100).ToString();
                    if (results.ToString().IndexOf(".") > 0)
                    {
                        SetExcelCell(sheet2, dt2.Rows.Count + 3, 6, styleHead12, (results.Substring(0, results.IndexOf(".") + 3)) + "%");
                    }
                    else
                    {
                        SetExcelCell(sheet2, dt2.Rows.Count + 3, 6, styleHead12, results + ".00%");
                    }
                }
                #endregion

                #region body3
                int totalNum3 = 0;
                int totalNumCount3 = 0;
                int totalReturnNum3 = 0;
                int totalReturnNumCount3 = 0;
                int totalOverDue3 = 0;
                int totalOverDueCount3 = 0;
                for (int iRow3 = 0; iRow3 < dt3.Rows.Count; iRow3++)
                {
                    for (int iCol = 0; iCol < dt3.Columns.Count; iCol++)
                    {
                        SetExcelCell(sheet3, iRow3 + 3, iCol, styleHead10, dt3.Rows[iRow3][iCol].ToString());
                    }
                    totalNum3 = Convert.ToInt32(dt3.Rows[iRow3]["Case"].ToString());
                    totalNumCount3 += totalNum3;
                    totalReturnNum3 = Convert.ToInt32(dt3.Rows[iRow3]["ReturnCase"].ToString());
                    totalReturnNumCount3 += totalReturnNum3;
                    totalOverDue3 = Convert.ToInt32(dt3.Rows[iRow3]["OutCase"].ToString());
                    totalOverDueCount3 += totalOverDue3;
                }
                SetExcelCell(sheet3, dt3.Rows.Count + 3, 2, styleHead12, totalNumCount3.ToString());
                SetExcelCell(sheet3, dt3.Rows.Count + 3, 3, styleHead12, totalReturnNumCount3.ToString());
                if (totalReturnNumCount3 == 0)
                {
                    SetExcelCell(sheet3, dt3.Rows.Count + 3, 4, styleHead12, "0.00%");
                }
                else
                {
                    string result3 = (((totalReturnNumCount3 * 1.0) / totalNumCount3) * 100).ToString();
                    if (result3.ToString().IndexOf(".") > 0)
                    {
                        SetExcelCell(sheet3, dt3.Rows.Count + 3, 4, styleHead12, (result3.Substring(0, result3.IndexOf(".") + 3)) + "%");
                    }
                    else
                    {
                        SetExcelCell(sheet3, dt3.Rows.Count + 3, 4, styleHead12, result3 + ".00%");
                    }
                }
                SetExcelCell(sheet3, dt3.Rows.Count + 3, 5, styleHead12, totalOverDueCount3.ToString());
                if (totalOverDueCount3 == 0)
                {
                    SetExcelCell(sheet3, dt3.Rows.Count + 3, 6, styleHead12, "0.00%");
                }
                else
                {
                    string results3 = (((totalOverDueCount3 * 1.0) / totalNumCount3) * 100).ToString();
                    if (results3.ToString().IndexOf(".") > 0)
                    {
                        SetExcelCell(sheet3, dt3.Rows.Count + 3, 6, styleHead12, (results3.Substring(0, results3.IndexOf(".") + 3)) + "%");
                    }
                    else
                    {
                        SetExcelCell(sheet3, dt3.Rows.Count + 3, 6, styleHead12, results3 + ".00%");
                    }
                }
                #endregion
            }

            MemoryStream ms = new MemoryStream();
            workbook.Write(ms);
            ms.Flush();
            ms.Position = 0;
            workbook = null;
            return ms;
        }

        #endregion
        #region 主管放行統計表
        public MemoryStream ApproveReportExcel_NPOI(CaseClosedQuery model)
        {
            IWorkbook workbook = new HSSFWorkbook();
            ISheet sheet = null;
            ISheet sheet2 = null;
            ISheet sheet3 = null;
            LdapEmployeeBiz ldap = new LdapEmployeeBiz();
            List<LDAPEmployee> ldapList = new List<LDAPEmployee>();//人員
            List<LDAPEmployee> ldapList2 = new List<LDAPEmployee>();//集作二科
            List<LDAPEmployee> ldapList3 = new List<LDAPEmployee>();//集作三科
            DataTable dtCase = new DataTable();//導出資料
            DataTable dtCase2 = new DataTable();
            DataTable dtCase3 = new DataTable();

            int rowscountExcelresult = 0;//合計參數
            string caseExcel = "";//案件類型
            int rowsExcel = 2;//行數
            int rowscountExcel = 0;//最後一列合計
            int rowstatolExcel = 0;//總合計      

            #region def style
            ICellStyle styleHead12 = workbook.CreateCellStyle();
            IFont font12 = workbook.CreateFont();
            font12.FontHeightInPoints = 12;
            font12.FontName = "新細明體";
            styleHead12.FillPattern = FillPattern.SolidForeground;
            styleHead12.FillForegroundColor = HSSFColor.White.Index;
            styleHead12.BorderTop = BorderStyle.None;
            styleHead12.BorderLeft = BorderStyle.None;
            styleHead12.BorderRight = BorderStyle.None;
            styleHead12.BorderBottom = BorderStyle.None;
            styleHead12.WrapText = true;
            styleHead12.Alignment = HorizontalAlignment.Center;
            styleHead12.VerticalAlignment = VerticalAlignment.Center;
            styleHead12.SetFont(font12);

            ICellStyle styleHead10 = workbook.CreateCellStyle();
            IFont font10 = workbook.CreateFont();
            font10.FontHeightInPoints = 10;
            font10.FontName = "新細明體";
            styleHead10.FillPattern = FillPattern.SolidForeground;
            styleHead10.FillForegroundColor = HSSFColor.White.Index;
            styleHead10.BorderTop = BorderStyle.Thin;
            styleHead10.BorderLeft = BorderStyle.Thin;
            styleHead10.BorderRight = BorderStyle.Thin;
            styleHead10.BorderBottom = BorderStyle.Thin;
            styleHead10.WrapText = true;
            styleHead10.Alignment = HorizontalAlignment.Left;
            styleHead10.VerticalAlignment = VerticalAlignment.Center;
            styleHead10.SetFont(font10);
            #endregion

            #region 單獨科別的數據源(科別及案件資料)
            //獲取人員
            if (model.Depart == "1" || model.Depart == "0")//* 集作一科
            {
                sheet = workbook.CreateSheet("集作一科");
                ldapList = ldap.GetLdapManagerListByDepart("集作一科");//獲取集作一科人員
                dtCase = GetApproveList(model, "集作一科");
                if (ldapList.Count < 7)
                {
                    SetExcelCell(sheet, 1, 7, styleHead12, "集作一科");
                    sheet.AddMergedRegion(new CellRangeAddress(1, 1, 7, 7));
                }
                else
                {
                    SetExcelCell(sheet, 1, ldapList.Count + 1, styleHead12, "集作一科");
                    sheet.AddMergedRegion(new CellRangeAddress(1, 1, ldapList.Count + 1, ldapList.Count + 1));
                }
            }
            if (model.Depart == "2")//* 集作二科
            {
                sheet = workbook.CreateSheet("集作二科");
                ldapList = ldap.GetLdapManagerListByDepart("集作二科");//獲取集作二科人員
                dtCase = GetApproveList(model, "集作二科");
                if (ldapList.Count < 7)
                {
                    SetExcelCell(sheet, 1, 7, styleHead12, "集作二科");
                    sheet.AddMergedRegion(new CellRangeAddress(1, 1, 7, 7));
                }
                else
                {
                    SetExcelCell(sheet, 1, ldapList.Count + 1, styleHead12, "集作二科");
                    sheet.AddMergedRegion(new CellRangeAddress(1, 1, ldapList.Count + 1, ldapList.Count + 1));
                }
            }
            if (model.Depart == "3")//* 集作三科
            {
                sheet = workbook.CreateSheet("集作三科");
                ldapList = ldap.GetLdapManagerListByDepart("集作三科");//獲取集作三科人員
                dtCase = GetApproveList(model, "集作三科");
                if (ldapList.Count < 7)
                {
                    SetExcelCell(sheet, 1, 7, styleHead12, "集作三科");
                    sheet.AddMergedRegion(new CellRangeAddress(1, 1, 7, 7));
                }
                else
                {
                    SetExcelCell(sheet, 1, ldapList.Count + 1, styleHead12, "集作三科");
                    sheet.AddMergedRegion(new CellRangeAddress(1, 1, ldapList.Count + 1, ldapList.Count + 1));
                }
            }
            if (model.Depart == "0")//* 全部
            {
                sheet2 = workbook.CreateSheet("集作二科");
                sheet3 = workbook.CreateSheet("集作三科");
                ldapList2 = ldap.GetLdapManagerListByDepart("集作二科");//獲取集作二科人員
                ldapList3 = ldap.GetLdapManagerListByDepart("集作三科");//獲取集作三科人員
                dtCase2 = GetApproveList(model, "集作二科");//獲取查詢集作二科的案件
                dtCase3 = GetApproveList(model, "集作三科");//獲取查詢集作三科的案件
            }
            #endregion

            string caseKind = "";//*去重複
            int rows = 2;//定義行數

            #region title
            //*大標題 line0
            SetExcelCell(sheet, 0, 0, styleHead12, "主管放行統計表");
            sheet.AddMergedRegion(new CellRangeAddress(0, 0, 0, ldapList.Count + 1));

            //*查詢條件 line1
            SetExcelCell(sheet, 1, 0, styleHead12, "收件日期：");
            sheet.AddMergedRegion(new CellRangeAddress(1, 1, 0, 0));
            SetExcelCell(sheet, 1, 1, styleHead12, model.ReceiveDateStart + '~' + model.ReceiveDateEnd);
            sheet.AddMergedRegion(new CellRangeAddress(1, 1, 1, 1));
            SetExcelCell(sheet, 1, 2, styleHead12, "發文日期：");
            sheet.AddMergedRegion(new CellRangeAddress(1, 1, 2, 2));
            SetExcelCell(sheet, 1, 3, styleHead12, model.SendDateStart + '~' + model.SendDateEnd);
            sheet.AddMergedRegion(new CellRangeAddress(1, 1, 3, 3));
            SetExcelCell(sheet, 1, 4, styleHead12, "結案日期：");
            sheet.AddMergedRegion(new CellRangeAddress(1, 1, 4, 4));
            SetExcelCell(sheet, 1, 5, styleHead12, model.CloseDateStart + '~' + model.CloseDateEnd);
            sheet.AddMergedRegion(new CellRangeAddress(1, 1, 5, 5));
            if (ldapList.Count < 7)
            {
                SetExcelCell(sheet, 1, 6, styleHead12, "部門別/科別");
                sheet.AddMergedRegion(new CellRangeAddress(1, 1, 6, 6));
            }
            else
            {
                SetExcelCell(sheet, 1, ldapList.Count - 1, styleHead12, "部門別/科別");
                sheet.AddMergedRegion(new CellRangeAddress(1, 1, ldapList.Count - 1, ldapList.Count));

            }

            //*結果集表頭 line2
            SetExcelCell(sheet, 2, 0, styleHead10, "處理人員");
            sheet.AddMergedRegion(new CellRangeAddress(2, 2, 0, 0));
            sheet.SetColumnWidth(0, 100 * 50);
            for (int i = 0; i < ldapList.Count; i++)//集作一科人員詳細
            {
                SetExcelCell(sheet, 2, i + 1, styleHead10, ldapList[i].EmpName);
                sheet.AddMergedRegion(new CellRangeAddress(2, 2, i + 1, i + 1));
            }
            SetExcelCell(sheet, 2, ldapList.Count + 1, styleHead10, "合計");
            sheet.AddMergedRegion(new CellRangeAddress(2, 2, ldapList.Count + 1, ldapList.Count + 1));

            //*扣押案件類型 line3-lineN 
            for (int i = 0; i < dtCase.Rows.Count; i++)
            {
                if (caseKind != dtCase.Rows[i]["New_CaseKind"].ToString())
                {
                    rows = rows + 1;
                    SetExcelCell(sheet, rows, 0, styleHead10, dtCase.Rows[i]["New_CaseKind"].ToString());
                    sheet.AddMergedRegion(new CellRangeAddress(rows, rows, 0, 0));
                    SetExcelCell(sheet, rows, ldapList.Count + 1, styleHead10, "0");
                    sheet.AddMergedRegion(new CellRangeAddress(rows, rows, ldapList.Count + 1, ldapList.Count + 1));
                    caseKind = dtCase.Rows[i]["New_CaseKind"].ToString();
                    for (int j = 0; j < ldapList.Count; j++)//*初始表格賦初值 
                    {
                        SetExcelCell(sheet, rows, j + 1, styleHead10, "0");
                        sheet.AddMergedRegion(new CellRangeAddress(rows, rows, j + 1, j + 1));
                    }
                }
            }

            //*合計 lineLast
            SetExcelCell(sheet, rows + 1, 0, styleHead10, "合計");
            sheet.AddMergedRegion(new CellRangeAddress(ldapList.Count + 3, ldapList.Count + 3, 0, 0));
            for (int j = 0; j < ldapList.Count; j++)//*初始表格賦初值 
            {
                SetExcelCell(sheet, rows + 1, j + 1, styleHead10, "0");
                sheet.AddMergedRegion(new CellRangeAddress(rows + 1, rows + 1, j + 1, j + 1));
            }
            #endregion

            #region  body
            for (int iRow = 0; iRow < dtCase.Rows.Count; iRow++)//根據案件類型進行循環
            {
                for (int i = 0; i < ldapList.Count; i++)//循環excel中的列
                {
                    if (caseExcel == dtCase.Rows[iRow]["New_CaseKind"].ToString())//重複同一案件類型的數據
                    {
                        if (ldapList[i].EmpName == dtCase.Rows[iRow]["EmpName"].ToString())
                        {
                            SetExcelCell(sheet, rowsExcel, i + 1, styleHead10, dtCase.Rows[iRow]["case_num"].ToString());
                            rowscountExcelresult = Convert.ToInt32(dtCase.Rows[iRow]["case_num"].ToString());//每格資料
                            SetExcelCell(sheet, rows + 1, i + 1, styleHead10, dtCase.Rows[iRow]["UserCount"].ToString());//最後一行合計
                            rowscountExcel += rowscountExcelresult;
                            rowstatolExcel += rowscountExcelresult;
                        }
                        SetExcelCell(sheet, rowsExcel, ldapList.Count + 1, styleHead10, rowscountExcel.ToString());//最後一列合計
                    }
                    else//不重複的案件類型
                    {
                        rowscountExcel = 0;
                        rowsExcel = rowsExcel + 1;
                        if (dtCase.Rows[iRow]["EmpName"].ToString() == ldapList[i].EmpName)
                        {
                            SetExcelCell(sheet, rowsExcel, i + 1, styleHead10, dtCase.Rows[iRow]["case_num"].ToString());
                            rowscountExcelresult = Convert.ToInt32(dtCase.Rows[iRow]["case_num"].ToString());//第一條不重複的數據儲存下值
                            SetExcelCell(sheet, rows + 1, i + 1, styleHead10, dtCase.Rows[iRow]["UserCount"].ToString());//最後一行合計
                            rowscountExcel += rowscountExcelresult;
                            rowstatolExcel += rowscountExcelresult;
                        }
                        caseExcel = dtCase.Rows[iRow]["New_CaseKind"].ToString();
                        SetExcelCell(sheet, rowsExcel, ldapList.Count + 1, styleHead10, rowscountExcel.ToString());//最後一列合計
                    }
                }
            }
            SetExcelCell(sheet, rows + 1, ldapList.Count + 1, styleHead10, rowstatolExcel.ToString());//總合計
            #endregion

            if (model.Depart == "0")//* 全部
            {
                #region title2
                //*大標題 line0
                SetExcelCell(sheet2, 0, 0, styleHead12, "主管放行統計表");
                sheet2.AddMergedRegion(new CellRangeAddress(0, 0, 0, ldapList2.Count + 1));

                //*查詢條件 line1
                SetExcelCell(sheet2, 1, 0, styleHead12, "收件日期：");
                sheet2.AddMergedRegion(new CellRangeAddress(1, 1, 0, 0));
                SetExcelCell(sheet2, 1, 1, styleHead12, model.ReceiveDateStart + '~' + model.ReceiveDateEnd);
                sheet2.AddMergedRegion(new CellRangeAddress(1, 1, 1, 1));
                SetExcelCell(sheet2, 1, 2, styleHead12, "發文日期：");
                sheet2.AddMergedRegion(new CellRangeAddress(1, 1, 2, 2));
                SetExcelCell(sheet2, 1, 3, styleHead12, model.SendDateStart + '~' + model.SendDateEnd);
                sheet2.AddMergedRegion(new CellRangeAddress(1, 1, 3, 3));
                SetExcelCell(sheet2, 1, 4, styleHead12, "結案日期：");
                sheet2.AddMergedRegion(new CellRangeAddress(1, 1, 4, 4));
                SetExcelCell(sheet2, 1, 5, styleHead12, model.CloseDateStart + '~' + model.CloseDateEnd);
                sheet2.AddMergedRegion(new CellRangeAddress(1, 1, 5, 5));
                if (ldapList2.Count < 7)
                {
                    SetExcelCell(sheet2, 1, 6, styleHead12, "部門別/科別");
                    sheet2.AddMergedRegion(new CellRangeAddress(1, 1, 6, 6));
                    SetExcelCell(sheet2, 1, 7, styleHead12, "集作二科");
                    sheet2.AddMergedRegion(new CellRangeAddress(1, 1, 7, 7));
                }
                else
                {
                    SetExcelCell(sheet2, 1, ldapList2.Count - 1, styleHead12, "部門別/科別");
                    sheet2.AddMergedRegion(new CellRangeAddress(1, 1, ldapList2.Count - 1, ldapList2.Count));
                    SetExcelCell(sheet2, 1, ldapList2.Count + 1, styleHead12, "集作二科");
                    sheet2.AddMergedRegion(new CellRangeAddress(1, 1, ldapList2.Count + 1, ldapList2.Count + 1));
                }

                //*結果集表頭 line2
                SetExcelCell(sheet2, 2, 0, styleHead10, "處理人員");
                sheet2.AddMergedRegion(new CellRangeAddress(2, 2, 0, 0));
                sheet2.SetColumnWidth(0, 100 * 50);
                for (int i = 0; i < ldapList2.Count; i++)//集作一科人員詳細
                {
                    SetExcelCell(sheet2, 2, i + 1, styleHead10, ldapList2[i].EmpName);
                    sheet2.AddMergedRegion(new CellRangeAddress(2, 2, i + 1, i + 1));
                }
                SetExcelCell(sheet2, 2, ldapList2.Count + 1, styleHead10, "合計");
                sheet2.AddMergedRegion(new CellRangeAddress(2, 2, ldapList2.Count + 1, ldapList2.Count + 1));

                //*扣押案件類型 line3-lineN 
                caseKind = "";//*去重複
                rows = 2;//定義行數
                for (int i = 0; i < dtCase2.Rows.Count; i++)
                {
                    if (caseKind != dtCase2.Rows[i]["New_CaseKind"].ToString())
                    {
                        rows = rows + 1;
                        SetExcelCell(sheet2, rows, 0, styleHead10, dtCase2.Rows[i]["New_CaseKind"].ToString());
                        sheet2.AddMergedRegion(new CellRangeAddress(rows, rows, 0, 0));

                        SetExcelCell(sheet2, rows, ldapList2.Count + 1, styleHead10, "0");
                        sheet2.AddMergedRegion(new CellRangeAddress(rows, rows, ldapList2.Count + 1, ldapList2.Count + 1));

                        caseKind = dtCase2.Rows[i]["New_CaseKind"].ToString();
                        for (int j = 0; j < ldapList2.Count; j++)//*初始表格賦初值 
                        {
                            SetExcelCell(sheet2, rows, j + 1, styleHead10, "0");
                            sheet2.AddMergedRegion(new CellRangeAddress(rows, rows, j + 1, j + 1));
                        }
                    }
                }

                //*合計 lineLast
                SetExcelCell(sheet2, rows + 1, 0, styleHead10, "合計");
                sheet2.AddMergedRegion(new CellRangeAddress(ldapList2.Count + 3, ldapList2.Count + 3, 0, 0));
                for (int j = 0; j < ldapList2.Count; j++)//*初始表格賦初值 
                {
                    SetExcelCell(sheet2, rows + 1, j + 1, styleHead10, "0");
                    sheet2.AddMergedRegion(new CellRangeAddress(rows + 1, rows + 1, j + 1, j + 1));
                }
                #endregion
                #region body2
                caseExcel = "";//案件類型
                rowsExcel = 2;//行數
                rowscountExcel = 0;//最後一列合計
                rowstatolExcel = 0;//總合計      
                for (int iRow = 0; iRow < dtCase2.Rows.Count; iRow++)//根據案件類型進行循環
                {
                    for (int i = 0; i < ldapList2.Count; i++)//循環excel中的列
                    {
                        if (dtCase2.Rows[iRow]["New_CaseKind"].ToString() == caseExcel)//重複同一案件類型的數據
                        {
                            if (dtCase2.Rows[iRow]["EmpName"].ToString() == ldapList2[i].EmpName)
                            {
                                SetExcelCell(sheet2, rowsExcel, i + 1, styleHead10, dtCase2.Rows[iRow]["case_num"].ToString());
                                SetExcelCell(sheet2, rows + 1, i + 1, styleHead10, dtCase2.Rows[iRow]["UserCount"].ToString());//最後一行合計
                                rowscountExcelresult = Convert.ToInt32(dtCase2.Rows[iRow]["case_num"].ToString());//每格資料
                                rowscountExcel += rowscountExcelresult;
                                rowstatolExcel += rowscountExcelresult;
                            }
                            SetExcelCell(sheet2, rowsExcel, ldapList2.Count + 1, styleHead10, rowscountExcel.ToString());//最後一列合計
                        }
                        else//不重複的案件類型
                        {
                            rowscountExcel = 0;
                            rowsExcel = rowsExcel + 1;
                            if (dtCase2.Rows[iRow]["EmpName"].ToString() == ldapList2[i].EmpName)
                            {
                                SetExcelCell(sheet2, rowsExcel, i + 1, styleHead10, dtCase2.Rows[iRow]["case_num"].ToString());
                                SetExcelCell(sheet2, rows + 1, i + 1, styleHead10, dtCase2.Rows[iRow]["UserCount"].ToString());//最後一行合計
                                rowscountExcelresult = Convert.ToInt32(dtCase2.Rows[iRow]["case_num"].ToString());//第一條不重複的數據儲存下值
                                rowscountExcel += rowscountExcelresult;
                                rowstatolExcel += rowscountExcelresult;
                            }
                            caseExcel = dtCase2.Rows[iRow]["New_CaseKind"].ToString();
                            SetExcelCell(sheet2, rowsExcel, ldapList2.Count + 1, styleHead10, rowscountExcel.ToString());//最後一列合計      
                        }
                    }
                }
                SetExcelCell(sheet2, rows + 1, ldapList2.Count + 1, styleHead10, rowstatolExcel.ToString());//總合計
                #endregion

                #region title3
                //*大標題 line0
                SetExcelCell(sheet3, 0, 0, styleHead12, "主管放行統計表");
                sheet3.AddMergedRegion(new CellRangeAddress(0, 0, 0, ldapList3.Count + 1));

                //*查詢條件 line1
                SetExcelCell(sheet3, 1, 0, styleHead12, "收件日期：");
                sheet3.AddMergedRegion(new CellRangeAddress(1, 1, 0, 0));
                SetExcelCell(sheet3, 1, 1, styleHead12, model.ReceiveDateStart + '~' + model.ReceiveDateEnd);
                sheet3.AddMergedRegion(new CellRangeAddress(1, 1, 1, 1));
                SetExcelCell(sheet3, 1, 2, styleHead12, "發文日期：");
                sheet3.AddMergedRegion(new CellRangeAddress(1, 1, 2, 2));
                SetExcelCell(sheet3, 1, 3, styleHead12, model.SendDateStart + '~' + model.SendDateEnd);
                sheet3.AddMergedRegion(new CellRangeAddress(1, 1, 3, 3));
                SetExcelCell(sheet3, 1, 4, styleHead12, "結案日期：");
                sheet3.AddMergedRegion(new CellRangeAddress(1, 1, 4, 4));
                SetExcelCell(sheet3, 1, 5, styleHead12, model.CloseDateStart + '~' + model.CloseDateEnd);
                sheet3.AddMergedRegion(new CellRangeAddress(1, 1, 5, 5));
                if (ldapList3.Count < 7)
                {
                    SetExcelCell(sheet3, 1, 6, styleHead12, "部門別/科別");
                    sheet3.AddMergedRegion(new CellRangeAddress(1, 1, 6, 6));
                    SetExcelCell(sheet3, 1, 7, styleHead12, "集作三科");
                    sheet3.AddMergedRegion(new CellRangeAddress(1, 1, 7, 7));
                }
                else
                {
                    SetExcelCell(sheet3, 1, ldapList3.Count - 1, styleHead12, "部門別/科別");
                    sheet3.AddMergedRegion(new CellRangeAddress(1, 1, ldapList3.Count - 1, ldapList3.Count));
                    SetExcelCell(sheet3, 1, ldapList3.Count + 1, styleHead12, "集作三科");
                    sheet3.AddMergedRegion(new CellRangeAddress(1, 1, ldapList3.Count + 1, ldapList3.Count + 1));
                }

                //*結果集表頭 line2
                SetExcelCell(sheet3, 2, 0, styleHead10, "處理人員");
                sheet3.AddMergedRegion(new CellRangeAddress(2, 2, 0, 0));
                sheet3.SetColumnWidth(0, 100 * 50);
                for (int i = 0; i < ldapList3.Count; i++)//集作一科人員詳細
                {
                    SetExcelCell(sheet3, 2, i + 1, styleHead10, ldapList3[i].EmpName);
                    sheet3.AddMergedRegion(new CellRangeAddress(2, 2, i + 1, i + 1));
                }
                SetExcelCell(sheet3, 2, ldapList3.Count + 1, styleHead10, "合計");
                sheet3.AddMergedRegion(new CellRangeAddress(2, 2, ldapList3.Count + 1, ldapList3.Count + 1));

                //*扣押案件類型 line3-lineN 
                caseKind = "";//*去重複
                rows = 2;//定義行數
                for (int i = 0; i < dtCase3.Rows.Count; i++)
                {
                    if (caseKind != dtCase3.Rows[i]["New_CaseKind"].ToString())
                    {
                        rows = rows + 1;
                        SetExcelCell(sheet3, rows, 0, styleHead10, dtCase3.Rows[i]["New_CaseKind"].ToString());
                        sheet3.AddMergedRegion(new CellRangeAddress(rows, rows, 0, 0));
                        SetExcelCell(sheet3, rows, ldapList3.Count + 1, styleHead10, "0");
                        sheet3.AddMergedRegion(new CellRangeAddress(rows, rows, ldapList3.Count + 1, ldapList3.Count + 1));
                        caseKind = dtCase3.Rows[i]["New_CaseKind"].ToString();
                        for (int j = 0; j < ldapList3.Count; j++)//*初始表格賦初值 
                        {
                            SetExcelCell(sheet3, rows, j + 1, styleHead10, "0");
                            sheet3.AddMergedRegion(new CellRangeAddress(rows, rows, j + 1, j + 1));
                        }
                    }
                }

                //*合計 lineLast
                SetExcelCell(sheet3, rows + 1, 0, styleHead10, "合計");
                sheet3.AddMergedRegion(new CellRangeAddress(ldapList3.Count + 3, ldapList3.Count + 3, 0, 0));
                for (int j = 0; j < ldapList3.Count; j++)//*初始表格賦初值 
                {
                    SetExcelCell(sheet3, rows + 1, j + 1, styleHead10, "0");
                    sheet3.AddMergedRegion(new CellRangeAddress(rows + 1, rows + 1, j + 1, j + 1));
                }
                #endregion
                #region body3
                caseExcel = "";//案件類型
                rowsExcel = 2;//行數
                rowscountExcel = 0;//最後一列合計
                rowstatolExcel = 0;//總合計  
                for (int iRow = 0; iRow < dtCase3.Rows.Count; iRow++)//根據案件類型進行循環
                {
                    for (int i = 0; i < ldapList3.Count; i++)//循環excel中的列
                    {
                        if (dtCase3.Rows[iRow]["New_CaseKind"].ToString() == caseExcel)//重複同一案件類型的數據
                        {
                            if (dtCase3.Rows[iRow]["EmpName"].ToString() == ldapList3[i].EmpName)
                            {
                                SetExcelCell(sheet3, rowsExcel, i + 1, styleHead10, dtCase3.Rows[iRow]["case_num"].ToString());
                                SetExcelCell(sheet3, rows + 1, i + 1, styleHead10, dtCase3.Rows[iRow]["UserCount"].ToString());//最後一行合計
                                rowscountExcelresult = Convert.ToInt32(dtCase3.Rows[iRow]["case_num"].ToString());//每格資料
                                rowscountExcel += rowscountExcelresult;
                                rowstatolExcel += rowscountExcelresult;
                            }
                            SetExcelCell(sheet3, rowsExcel, ldapList3.Count + 1, styleHead10, rowscountExcel.ToString());//最後一列合計
                        }
                        else//不重複的案件類型
                        {
                            rowscountExcel = 0;
                            rowsExcel = rowsExcel + 1;
                            if (dtCase3.Rows[iRow]["EmpName"].ToString() == ldapList3[i].EmpName)
                            {
                                SetExcelCell(sheet3, rowsExcel, i + 1, styleHead10, dtCase3.Rows[iRow]["case_num"].ToString());
                                SetExcelCell(sheet3, rows + 1, i + 1, styleHead10, dtCase3.Rows[iRow]["UserCount"].ToString());//最後一行合計
                                rowscountExcelresult = Convert.ToInt32(dtCase3.Rows[iRow]["case_num"].ToString());//第一條不重複的數據儲存下值
                                rowscountExcel += rowscountExcelresult;
                                rowstatolExcel += rowscountExcelresult;
                            }
                            caseExcel = dtCase3.Rows[iRow]["New_CaseKind"].ToString();
                            SetExcelCell(sheet3, rowsExcel, ldapList3.Count + 1, styleHead10, rowscountExcel.ToString());//最後一列合計      
                        }
                    }
                }
                SetExcelCell(sheet3, rows + 1, ldapList3.Count + 1, styleHead10, rowstatolExcel.ToString());//總合計
                #endregion
            }

            MemoryStream ms = new MemoryStream();
            workbook.Write(ms);
            ms.Flush();
            ms.Position = 0;
            workbook = null;
            return ms;
        }
        #endregion
        #region 經辦退件明細表
        public MemoryStream ReturnDetailReportExcel_NPOI(CaseClosedQuery model)
        {
            IWorkbook workbook = new HSSFWorkbook();
            ISheet sheet = null;
            ISheet sheet2 = null;
            ISheet sheet3 = null;

            DataTable dt = new DataTable();
            DataTable dt2 = new DataTable();
            DataTable dt3 = new DataTable();

            #region def style
            ICellStyle styleHead12 = workbook.CreateCellStyle();
            IFont font12 = workbook.CreateFont();
            font12.FontHeightInPoints = 12;
            font12.FontName = "新細明體";
            styleHead12.FillPattern = FillPattern.SolidForeground;
            styleHead12.FillForegroundColor = HSSFColor.White.Index;
            styleHead12.BorderTop = BorderStyle.None;
            styleHead12.BorderLeft = BorderStyle.None;
            styleHead12.BorderRight = BorderStyle.None;
            styleHead12.BorderBottom = BorderStyle.None;
            styleHead12.WrapText = true;
            styleHead12.Alignment = HorizontalAlignment.Center;
            styleHead12.VerticalAlignment = VerticalAlignment.Center;
            styleHead12.SetFont(font12);

            ICellStyle styleHead10 = workbook.CreateCellStyle();
            IFont font10 = workbook.CreateFont();
            font10.FontHeightInPoints = 10;
            font10.FontName = "新細明體";
            styleHead10.FillPattern = FillPattern.SolidForeground;
            styleHead10.FillForegroundColor = HSSFColor.White.Index;
            styleHead10.BorderTop = BorderStyle.Thin;
            styleHead10.BorderLeft = BorderStyle.Thin;
            styleHead10.BorderRight = BorderStyle.Thin;
            styleHead10.BorderBottom = BorderStyle.Thin;
            styleHead10.WrapText = true;
            styleHead10.Alignment = HorizontalAlignment.Left;
            styleHead10.VerticalAlignment = VerticalAlignment.Center;
            styleHead10.SetFont(font10);
            #endregion

            #region 獲取數據源(集作一科及案件資料)
            //獲取人員
            if (model.Depart == "1")//* 集作一科
            {
                sheet = workbook.CreateSheet("集作一科");
                dt = GetReturnDetailList(model, "集作一科");//獲取查詢集作一科的案件
                SetExcelCell(sheet, 1, 7, styleHead12, "集作一科");
                sheet.AddMergedRegion(new CellRangeAddress(1, 1, 7, 7));
            }
            if (model.Depart == "2")//* 集作二科
            {
                sheet = workbook.CreateSheet("集作二科");
                dt = GetReturnDetailList(model, "集作二科");//獲取查詢集作二科的案件
                SetExcelCell(sheet, 1, 7, styleHead12, "集作二科");
                sheet.AddMergedRegion(new CellRangeAddress(1, 1, 7, 7));
            }
            if (model.Depart == "3")//*集作三科
            {
                sheet = workbook.CreateSheet("集作三科");
                dt = GetReturnDetailList(model, "集作三科");//獲取查詢集作三科的案件
                SetExcelCell(sheet, 1, 7, styleHead12, "集作三科");
                sheet.AddMergedRegion(new CellRangeAddress(1, 1, 7, 7));
            }
            if (model.Depart == "0")//*全部
            {
                sheet = workbook.CreateSheet("集作一科");
                sheet2 = workbook.CreateSheet("集作二科");
                sheet3 = workbook.CreateSheet("集作三科");
                dt = GetReturnDetailList(model, "集作一科");//獲取查詢集作一科的案件
                dt2 = GetReturnDetailList(model, "集作二科");//獲取查詢集作二科的案件
                dt3 = GetReturnDetailList(model, "集作三科");//獲取查詢集作三科的案件
                SetExcelCell(sheet, 1, 7, styleHead12, "集作一科");
                sheet.AddMergedRegion(new CellRangeAddress(1, 1, 7, 7));
                SetExcelCell(sheet2, 1, 7, styleHead12, "集作二科");
                sheet2.AddMergedRegion(new CellRangeAddress(1, 1, 7, 7));
                SetExcelCell(sheet3, 1, 7, styleHead12, "集作三科");
                sheet3.AddMergedRegion(new CellRangeAddress(1, 1, 7, 7));
            }
            #endregion

            #region title
            SetExcelCell(sheet, 0, 0, styleHead12, "經辦退件明細表");
            sheet.AddMergedRegion(new CellRangeAddress(0, 0, 0, 3));

            //* line1
            SetExcelCell(sheet, 1, 0, styleHead12, "收件日期：");
            sheet.AddMergedRegion(new CellRangeAddress(1, 1, 0, 0));
            SetExcelCell(sheet, 1, 1, styleHead12, model.ReceiveDateStart + '~' + model.ReceiveDateEnd);
            sheet.AddMergedRegion(new CellRangeAddress(1, 1, 1, 1));
            SetExcelCell(sheet, 1, 2, styleHead12, "發文日期：");
            sheet.AddMergedRegion(new CellRangeAddress(1, 1, 2, 2));
            SetExcelCell(sheet, 1, 3, styleHead12, model.SendDateStart + '~' + model.SendDateEnd);
            sheet.AddMergedRegion(new CellRangeAddress(1, 1, 3, 3));
            SetExcelCell(sheet, 1, 4, styleHead12, "結案日期：");
            sheet.AddMergedRegion(new CellRangeAddress(1, 1, 4, 4));
            SetExcelCell(sheet, 1, 5, styleHead12, model.CloseDateStart + '~' + model.CloseDateEnd);
            sheet.AddMergedRegion(new CellRangeAddress(1, 1, 5, 5));
            SetExcelCell(sheet, 1, 6, styleHead12, "部門別/科別：");
            sheet.AddMergedRegion(new CellRangeAddress(1, 1, 6, 6));

            //* line2
            SetExcelCell(sheet, 2, 0, styleHead10, "案件類別-大類");
            sheet.AddMergedRegion(new CellRangeAddress(2, 2, 0, 0));
            SetExcelCell(sheet, 2, 1, styleHead10, "案件編號");
            sheet.AddMergedRegion(new CellRangeAddress(2, 2, 1, 1));
            SetExcelCell(sheet, 2, 2, styleHead10, "處理經辦");
            sheet.AddMergedRegion(new CellRangeAddress(2, 2, 2, 2));
            SetExcelCell(sheet, 2, 3, styleHead10, "退件原因");
            sheet.AddMergedRegion(new CellRangeAddress(2, 2, 3, 3));
            #endregion
            #region Width
            sheet.SetColumnWidth(0, 100 * 40);
            sheet.SetColumnWidth(1, 100 * 40);
            sheet.SetColumnWidth(2, 100 * 40);
            sheet.SetColumnWidth(3, 100 * 100);
            #endregion
            #region body
            for (int iRow = 0; iRow < dt.Rows.Count; iRow++)
            {
                for (int iCol = 0; iCol < dt.Columns.Count - 1; iCol++)
                {
                    SetExcelCell(sheet, iRow + 3, iCol, styleHead10, dt.Rows[iRow][iCol].ToString());
                }
            }
            #endregion

            if (model.Depart == "0")//* 全部
            {
                #region title2
                SetExcelCell(sheet2, 0, 0, styleHead12, "經辦退件明細表");
                sheet2.AddMergedRegion(new CellRangeAddress(0, 0, 0, 3));

                //* line1
                SetExcelCell(sheet2, 1, 0, styleHead12, "收件日期：");
                sheet2.AddMergedRegion(new CellRangeAddress(1, 1, 0, 0));
                SetExcelCell(sheet2, 1, 1, styleHead12, model.ReceiveDateStart + '~' + model.ReceiveDateEnd);
                sheet2.AddMergedRegion(new CellRangeAddress(1, 1, 1, 1));
                SetExcelCell(sheet2, 1, 2, styleHead12, "發文日期：");
                sheet2.AddMergedRegion(new CellRangeAddress(1, 1, 2, 2));
                SetExcelCell(sheet2, 1, 3, styleHead12, model.SendDateStart + '~' + model.SendDateEnd);
                sheet2.AddMergedRegion(new CellRangeAddress(1, 1, 3, 3));
                SetExcelCell(sheet2, 1, 4, styleHead12, "結案日期：");
                sheet2.AddMergedRegion(new CellRangeAddress(1, 1, 4, 4));
                SetExcelCell(sheet2, 1, 5, styleHead12, model.CloseDateStart + '~' + model.CloseDateEnd);
                sheet2.AddMergedRegion(new CellRangeAddress(1, 1, 5, 5));
                SetExcelCell(sheet2, 1, 6, styleHead12, "部門別/科別：");
                sheet2.AddMergedRegion(new CellRangeAddress(1, 1, 6, 6));

                //* line2
                SetExcelCell(sheet2, 2, 0, styleHead10, "案件類別-大類");
                sheet2.AddMergedRegion(new CellRangeAddress(2, 2, 0, 0));
                SetExcelCell(sheet2, 2, 1, styleHead10, "案件編號");
                sheet2.AddMergedRegion(new CellRangeAddress(2, 2, 1, 1));
                SetExcelCell(sheet2, 2, 2, styleHead10, "處理經辦");
                sheet2.AddMergedRegion(new CellRangeAddress(2, 2, 2, 2));
                SetExcelCell(sheet2, 2, 3, styleHead10, "退件原因");
                sheet2.AddMergedRegion(new CellRangeAddress(2, 2, 3, 3));
                #endregion

                #region title3
                SetExcelCell(sheet3, 0, 0, styleHead12, "經辦退件明細表");
                sheet3.AddMergedRegion(new CellRangeAddress(0, 0, 0, 3));

                //* line1
                SetExcelCell(sheet3, 1, 0, styleHead12, "收件日期：");
                sheet3.AddMergedRegion(new CellRangeAddress(1, 1, 0, 0));
                SetExcelCell(sheet3, 1, 1, styleHead12, model.ReceiveDateStart + '~' + model.ReceiveDateEnd);
                sheet3.AddMergedRegion(new CellRangeAddress(1, 1, 1, 1));
                SetExcelCell(sheet3, 1, 2, styleHead12, "發文日期：");
                sheet3.AddMergedRegion(new CellRangeAddress(1, 1, 2, 2));
                SetExcelCell(sheet3, 1, 3, styleHead12, model.SendDateStart + '~' + model.SendDateEnd);
                sheet3.AddMergedRegion(new CellRangeAddress(1, 1, 3, 3));
                SetExcelCell(sheet3, 1, 4, styleHead12, "結案日期：");
                sheet3.AddMergedRegion(new CellRangeAddress(1, 1, 4, 4));
                SetExcelCell(sheet3, 1, 5, styleHead12, model.CloseDateStart + '~' + model.CloseDateEnd);
                sheet3.AddMergedRegion(new CellRangeAddress(1, 1, 5, 5));
                SetExcelCell(sheet3, 1, 6, styleHead12, "部門別/科別：");
                sheet3.AddMergedRegion(new CellRangeAddress(1, 1, 6, 6));

                //* line2
                SetExcelCell(sheet3, 2, 0, styleHead10, "案件類別-大類");
                sheet3.AddMergedRegion(new CellRangeAddress(2, 2, 0, 0));
                SetExcelCell(sheet3, 2, 1, styleHead10, "案件編號");
                sheet3.AddMergedRegion(new CellRangeAddress(2, 2, 1, 1));
                SetExcelCell(sheet3, 2, 2, styleHead10, "處理經辦");
                sheet3.AddMergedRegion(new CellRangeAddress(2, 2, 2, 2));
                SetExcelCell(sheet3, 2, 3, styleHead10, "退件原因");
                sheet3.AddMergedRegion(new CellRangeAddress(2, 2, 3, 3));
                #endregion
                #region Width
                sheet2.SetColumnWidth(0, 100 * 40);
                sheet2.SetColumnWidth(1, 100 * 40);
                sheet2.SetColumnWidth(2, 100 * 40);
                sheet2.SetColumnWidth(3, 100 * 100);
                #endregion
                #region Width
                sheet3.SetColumnWidth(0, 100 * 40);
                sheet3.SetColumnWidth(1, 100 * 40);
                sheet3.SetColumnWidth(2, 100 * 40);
                sheet3.SetColumnWidth(3, 100 * 100);
                #endregion
                #region body2
                for (int iRow = 0; iRow < dt2.Rows.Count; iRow++)
                {
                    for (int iCol = 0; iCol < dt2.Columns.Count - 1; iCol++)
                    {
                        SetExcelCell(sheet2, iRow + 3, iCol, styleHead10, dt2.Rows[iRow][iCol].ToString());
                    }
                }
                #endregion

                #region body3
                for (int iRow = 0; iRow < dt3.Rows.Count; iRow++)
                {
                    for (int iCol = 0; iCol < dt3.Columns.Count - 1; iCol++)
                    {
                        SetExcelCell(sheet3, iRow + 3, iCol, styleHead10, dt3.Rows[iRow][iCol].ToString());
                    }
                }
                #endregion
            }

            MemoryStream ms = new MemoryStream();
            workbook.Write(ms);
            ms.Flush();
            ms.Position = 0;
            workbook = null;
            return ms;
        }

        #endregion
        #region 逾期案件明細表
        public MemoryStream OverDateReportExcel_NPOI(CaseClosedQuery model)
        {
            IWorkbook workbook = new HSSFWorkbook();
            ISheet sheet = null;
            ISheet sheet2 = null;
            ISheet sheet3 = null;

            DataTable dt = new DataTable();
            DataTable dt2 = new DataTable();
            DataTable dt3 = new DataTable();

            #region def style
            ICellStyle styleHead12 = workbook.CreateCellStyle();
            IFont font12 = workbook.CreateFont();
            font12.FontHeightInPoints = 12;
            font12.FontName = "新細明體";
            styleHead12.FillPattern = FillPattern.SolidForeground;
            styleHead12.FillForegroundColor = HSSFColor.White.Index;
            styleHead12.BorderTop = BorderStyle.None;
            styleHead12.BorderLeft = BorderStyle.None;
            styleHead12.BorderRight = BorderStyle.None;
            styleHead12.BorderBottom = BorderStyle.None;
            styleHead12.WrapText = true;
            styleHead12.Alignment = HorizontalAlignment.Center;
            styleHead12.VerticalAlignment = VerticalAlignment.Center;
            styleHead12.SetFont(font12);

            ICellStyle styleHead10 = workbook.CreateCellStyle();
            IFont font10 = workbook.CreateFont();
            font10.FontHeightInPoints = 10;
            font10.FontName = "新細明體";
            styleHead10.FillPattern = FillPattern.SolidForeground;
            styleHead10.FillForegroundColor = HSSFColor.White.Index;
            styleHead10.BorderTop = BorderStyle.Thin;
            styleHead10.BorderLeft = BorderStyle.Thin;
            styleHead10.BorderRight = BorderStyle.Thin;
            styleHead10.BorderBottom = BorderStyle.Thin;
            styleHead10.WrapText = true;
            styleHead10.Alignment = HorizontalAlignment.Left;
            styleHead10.VerticalAlignment = VerticalAlignment.Center;
            styleHead10.SetFont(font10);
            #endregion

            #region 獲取數據源(集作一科及案件資料)
            //獲取人員
            if (model.Depart == "1")//* 集作一科
            {
                sheet = workbook.CreateSheet("集作一科");
                dt = GetOverDateList(model, "集作一科");//獲取查詢集作一科的案件
                SetExcelCell(sheet, 1, 7, styleHead12, "集作一科");
                sheet.AddMergedRegion(new CellRangeAddress(1, 1, 7, 7));
            }
            if (model.Depart == "2")//* 集作二科
            {
                sheet = workbook.CreateSheet("集作二科");
                dt = GetOverDateList(model, "集作二科");//獲取查詢集作二科的案件
                SetExcelCell(sheet, 1, 7, styleHead12, "集作二科");
                sheet.AddMergedRegion(new CellRangeAddress(1, 1, 7, 7));
            }
            if (model.Depart == "3")//*集作三科
            {
                sheet = workbook.CreateSheet("集作三科");
                dt = GetOverDateList(model, "集作三科");//獲取查詢集作三科的案件
                SetExcelCell(sheet, 1, 7, styleHead12, "集作三科");
                sheet.AddMergedRegion(new CellRangeAddress(1, 1, 7, 7));
            }
            if (model.Depart == "0")//*全部
            {
                sheet = workbook.CreateSheet("集作一科");
                sheet2 = workbook.CreateSheet("集作二科");
                sheet3 = workbook.CreateSheet("集作三科");
                dt = GetOverDateList(model, "集作一科");//獲取查詢集作一科的案件
                dt2 = GetOverDateList(model, "集作二科");//獲取查詢集作二科的案件
                dt3 = GetOverDateList(model, "集作三科");//獲取查詢集作三科的案件
                SetExcelCell(sheet, 1, 7, styleHead12, "集作一科");
                sheet.AddMergedRegion(new CellRangeAddress(1, 1, 7, 7));
                SetExcelCell(sheet2, 1, 7, styleHead12, "集作二科");
                sheet2.AddMergedRegion(new CellRangeAddress(1, 1, 7, 7));
                SetExcelCell(sheet3, 1, 7, styleHead12, "集作三科");
                sheet3.AddMergedRegion(new CellRangeAddress(1, 1, 7, 7));
            }
            #endregion

            #region title
            SetExcelCell(sheet, 0, 0, styleHead12, "逾期案件明細表");
            sheet.AddMergedRegion(new CellRangeAddress(0, 0, 0, 4));

            //* line1
            SetExcelCell(sheet, 1, 0, styleHead12, "收件日期：");
            sheet.AddMergedRegion(new CellRangeAddress(1, 1, 0, 0));
            SetExcelCell(sheet, 1, 1, styleHead12, model.ReceiveDateStart + '~' + model.ReceiveDateEnd);
            sheet.AddMergedRegion(new CellRangeAddress(1, 1, 1, 1));
            SetExcelCell(sheet, 1, 2, styleHead12, "發文日期：");
            sheet.AddMergedRegion(new CellRangeAddress(1, 1, 2, 2));
            SetExcelCell(sheet, 1, 3, styleHead12, model.SendDateStart + '~' + model.SendDateEnd);
            sheet.AddMergedRegion(new CellRangeAddress(1, 1, 3, 3));
            SetExcelCell(sheet, 1, 4, styleHead12, "結案日期：");
            sheet.AddMergedRegion(new CellRangeAddress(1, 1, 4, 4));
            SetExcelCell(sheet, 1, 5, styleHead12, model.CloseDateStart + '~' + model.CloseDateEnd);
            sheet.AddMergedRegion(new CellRangeAddress(1, 1, 5, 5));
            SetExcelCell(sheet, 1, 6, styleHead12, "部門別/科別:");
            sheet.AddMergedRegion(new CellRangeAddress(1, 1, 6, 6));

            //* line2
            SetExcelCell(sheet, 2, 0, styleHead10, "案件類別-大類");
            sheet.AddMergedRegion(new CellRangeAddress(2, 2, 0, 0));
            SetExcelCell(sheet, 2, 1, styleHead10, "案件編號");
            sheet.AddMergedRegion(new CellRangeAddress(2, 2, 1, 1));
            SetExcelCell(sheet, 2, 2, styleHead10, "經辦人員");
            sheet.AddMergedRegion(new CellRangeAddress(2, 2, 2, 2));
            SetExcelCell(sheet, 2, 3, styleHead10, "處理天數");
            sheet.AddMergedRegion(new CellRangeAddress(2, 2, 3, 3));
            SetExcelCell(sheet, 2, 4, styleHead10, "逾期原因");
            sheet.AddMergedRegion(new CellRangeAddress(2, 2, 4, 4));
            #endregion
            #region Width
            sheet.SetColumnWidth(0, 100 * 40);
            sheet.SetColumnWidth(1, 100 * 40);
            sheet.SetColumnWidth(2, 100 * 40);
            sheet.SetColumnWidth(3, 100 * 40);
            sheet.SetColumnWidth(4, 100 * 100);
            #endregion
            #region body
            for (int iRow = 0; iRow < dt.Rows.Count; iRow++)
            {
                for (int iCol = 0; iCol < dt.Columns.Count - 3; iCol++)
                {
                    SetExcelCell(sheet, iRow + 3, iCol, styleHead10, dt.Rows[iRow][iCol].ToString());
                    //SetExcelCell(sheet, iRow + 3, 4, styleHead10, "");
                }
            }
            #endregion

            if (model.Depart == "0")//* 全部
            {
                #region title2
                SetExcelCell(sheet2, 0, 0, styleHead12, "逾期案件明細表");
                sheet2.AddMergedRegion(new CellRangeAddress(0, 0, 0, 4));

                //* line1
                SetExcelCell(sheet2, 1, 0, styleHead12, "收件日期：");
                sheet2.AddMergedRegion(new CellRangeAddress(1, 1, 0, 0));
                SetExcelCell(sheet2, 1, 1, styleHead12, model.ReceiveDateStart + '~' + model.ReceiveDateEnd);
                sheet2.AddMergedRegion(new CellRangeAddress(1, 1, 1, 1));
                SetExcelCell(sheet2, 1, 2, styleHead12, "發文日期：");
                sheet2.AddMergedRegion(new CellRangeAddress(1, 1, 2, 2));
                SetExcelCell(sheet2, 1, 3, styleHead12, model.SendDateStart + '~' + model.SendDateEnd);
                sheet2.AddMergedRegion(new CellRangeAddress(1, 1, 3, 3));
                SetExcelCell(sheet2, 1, 4, styleHead12, "結案日期：");
                sheet2.AddMergedRegion(new CellRangeAddress(1, 1, 4, 4));
                SetExcelCell(sheet2, 1, 5, styleHead12, model.CloseDateStart + '~' + model.CloseDateEnd);
                sheet2.AddMergedRegion(new CellRangeAddress(1, 1, 5, 5));
                SetExcelCell(sheet2, 1, 6, styleHead12, "部門別/科別:");
                sheet2.AddMergedRegion(new CellRangeAddress(1, 1, 6, 6));


                //* line2
                SetExcelCell(sheet2, 2, 0, styleHead10, "案件類別-大類");
                sheet2.AddMergedRegion(new CellRangeAddress(2, 2, 0, 0));
                SetExcelCell(sheet2, 2, 1, styleHead10, "案件編號");
                sheet2.AddMergedRegion(new CellRangeAddress(2, 2, 1, 1));
                SetExcelCell(sheet2, 2, 2, styleHead10, "經辦人員");
                sheet2.AddMergedRegion(new CellRangeAddress(2, 2, 2, 2));
                SetExcelCell(sheet2, 2, 3, styleHead10, "處理天數");
                sheet2.AddMergedRegion(new CellRangeAddress(2, 2, 3, 3));
                SetExcelCell(sheet2, 2, 4, styleHead10, "逾期原因");
                sheet2.AddMergedRegion(new CellRangeAddress(2, 2, 4, 4));
                #endregion

                #region title3
                SetExcelCell(sheet3, 0, 0, styleHead12, "逾期案件明細表");
                sheet3.AddMergedRegion(new CellRangeAddress(0, 0, 0, 4));

                //* line1
                SetExcelCell(sheet3, 1, 0, styleHead12, "收件日期：");
                sheet3.AddMergedRegion(new CellRangeAddress(1, 1, 0, 0));
                SetExcelCell(sheet3, 1, 1, styleHead12, model.ReceiveDateStart + '~' + model.ReceiveDateEnd);
                sheet3.AddMergedRegion(new CellRangeAddress(1, 1, 1, 1));
                SetExcelCell(sheet3, 1, 2, styleHead12, "發文日期：");
                sheet3.AddMergedRegion(new CellRangeAddress(1, 1, 2, 2));
                SetExcelCell(sheet3, 1, 3, styleHead12, model.SendDateStart + '~' + model.SendDateEnd);
                sheet3.AddMergedRegion(new CellRangeAddress(1, 1, 3, 3));
                SetExcelCell(sheet3, 1, 4, styleHead12, "結案日期：");
                sheet3.AddMergedRegion(new CellRangeAddress(1, 1, 4, 4));
                SetExcelCell(sheet3, 1, 5, styleHead12, model.CloseDateStart + '~' + model.CloseDateEnd);
                sheet3.AddMergedRegion(new CellRangeAddress(1, 1, 5, 5));
                SetExcelCell(sheet3, 1, 6, styleHead12, "部門別/科別:");
                sheet3.AddMergedRegion(new CellRangeAddress(1, 1, 6, 6));


                //* line2
                SetExcelCell(sheet3, 2, 0, styleHead10, "案件類別-大類");
                sheet3.AddMergedRegion(new CellRangeAddress(2, 2, 0, 0));
                SetExcelCell(sheet3, 2, 1, styleHead10, "案件編號");
                sheet3.AddMergedRegion(new CellRangeAddress(2, 2, 1, 1));
                SetExcelCell(sheet3, 2, 2, styleHead10, "經辦人員");
                sheet3.AddMergedRegion(new CellRangeAddress(2, 2, 2, 2));
                SetExcelCell(sheet3, 2, 3, styleHead10, "處理天數");
                sheet3.AddMergedRegion(new CellRangeAddress(2, 2, 3, 3));
                SetExcelCell(sheet3, 2, 4, styleHead10, "逾期原因");
                sheet3.AddMergedRegion(new CellRangeAddress(2, 2, 4, 4));
                #endregion
                #region Width
                sheet2.SetColumnWidth(0, 100 * 40);
                sheet2.SetColumnWidth(1, 100 * 40);
                sheet2.SetColumnWidth(2, 100 * 40);
                sheet2.SetColumnWidth(3, 100 * 40);
                sheet2.SetColumnWidth(4, 100 * 100);
                #endregion
                #region Width
                sheet3.SetColumnWidth(0, 100 * 40);
                sheet3.SetColumnWidth(1, 100 * 40);
                sheet3.SetColumnWidth(2, 100 * 40);
                sheet3.SetColumnWidth(3, 100 * 40);
                sheet3.SetColumnWidth(4, 100 * 100);
                #endregion
                #region body2
                for (int iRow = 0; iRow < dt2.Rows.Count; iRow++)
                {
                    for (int iCol = 0; iCol < dt2.Columns.Count - 3; iCol++)
                    {
                        SetExcelCell(sheet2, iRow + 3, iCol, styleHead10, dt2.Rows[iRow][iCol].ToString());
                        //SetExcelCell(sheet2, iRow + 3, 4, styleHead10, "");
                    }
                }
                #endregion

                #region body3
                for (int iRow = 0; iRow < dt3.Rows.Count; iRow++)
                {
                    for (int iCol = 0; iCol < dt3.Columns.Count - 3; iCol++)
                    {
                        SetExcelCell(sheet3, iRow + 3, iCol, styleHead10, dt3.Rows[iRow][iCol].ToString());
                        //SetExcelCell(sheet3, iRow + 3, 4, styleHead10, "");
                    }
                }
                #endregion
            }

            MemoryStream ms = new MemoryStream();
            workbook.Write(ms);
            ms.Flush();
            ms.Position = 0;
            workbook = null;
            return ms;
        }
        #endregion

        //#region 用印簿1
        //public MemoryStream ListReportExcel_NPOI1(CaseClosedQuery model)
        //{
        //    IWorkbook workbook = new HSSFWorkbook();
        //    ISheet sheet = null;
        //    ISheet sheet2 = null;
        //    ISheet sheet3 = null;
        //    //20170630 緊急 RQ-2015-019666-018 派件至跨單位 宏祥 add&update start
        //    ISheet sheet4 = null;

        //    DataTable dt = new DataTable();
        //    DataTable dt2 = new DataTable();
        //    DataTable dt3 = new DataTable();
        //    DataTable dt4 = new DataTable();
        //    //20170630 緊急 RQ-2015-019666-018 派件至跨單位 宏祥 add&update start

        //    #region def style
        //    ICellStyle styleHead12 = workbook.CreateCellStyle();
        //    IFont font12 = workbook.CreateFont();
        //    font12.FontHeightInPoints = 12;
        //    font12.FontName = "新細明體";
        //    styleHead12.FillPattern = FillPattern.SolidForeground;
        //    styleHead12.FillForegroundColor = HSSFColor.White.Index;
        //    styleHead12.BorderTop = BorderStyle.None;
        //    styleHead12.BorderLeft = BorderStyle.None;
        //    styleHead12.BorderRight = BorderStyle.None;
        //    styleHead12.BorderBottom = BorderStyle.None;
        //    styleHead12.WrapText = true;
        //    styleHead12.Alignment = HorizontalAlignment.Center;
        //    styleHead12.VerticalAlignment = VerticalAlignment.Center;
        //    styleHead12.SetFont(font12);

        //    ICellStyle styleHead10 = workbook.CreateCellStyle();
        //    IFont font10 = workbook.CreateFont();
        //    font10.FontHeightInPoints = 10;
        //    font10.FontName = "新細明體";
        //    styleHead10.FillPattern = FillPattern.SolidForeground;
        //    styleHead10.FillForegroundColor = HSSFColor.White.Index;
        //    styleHead10.BorderTop = BorderStyle.Thin;
        //    styleHead10.BorderLeft = BorderStyle.Thin;
        //    styleHead10.BorderRight = BorderStyle.Thin;
        //    styleHead10.BorderBottom = BorderStyle.Thin;
        //    styleHead10.WrapText = true;
        //    styleHead10.Alignment = HorizontalAlignment.Left;
        //    styleHead10.VerticalAlignment = VerticalAlignment.Center;
        //    styleHead10.SetFont(font10);
        //    #endregion

        //    #region 獲取數據源(集作一科及案件資料)
        //    //獲取人員
        //    if (model.Depart == "1" || model.Depart == "0")//* 集作一科
        //    {
        //        sheet = workbook.CreateSheet("集作一科");
        //        dt = GetList1(model, "集作一科");//獲取查詢集作一科的案件
        //        SetExcelCell(sheet, 1, 6, styleHead12, "集作一科");
        //        sheet.AddMergedRegion(new CellRangeAddress(1, 1, 6, 7));
        //    }
        //    if (model.Depart == "2")//* 集作二科
        //    {
        //        sheet = workbook.CreateSheet("集作二科");
        //        dt = GetList1(model, "集作二科");//獲取查詢集作二科的案件
        //        SetExcelCell(sheet, 1, 6, styleHead12, "集作二科");
        //        sheet.AddMergedRegion(new CellRangeAddress(1, 1, 6, 7));
        //    }
        //    if (model.Depart == "3")//*集作三科
        //    {
        //        sheet = workbook.CreateSheet("集作三科");
        //        dt = GetList1(model, "集作三科");//獲取查詢集作三科的案件
        //        SetExcelCell(sheet, 1, 6, styleHead12, "集作三科");
        //        sheet.AddMergedRegion(new CellRangeAddress(1, 1, 6, 7));
        //    }
        //    //20170630 緊急 RQ-2015-019666-018 派件至跨單位 宏祥 add&update start
        //    if (model.Depart == "4")//*集作四科
        //    {
        //       sheet = workbook.CreateSheet("集作四科");
        //       dt = GetList1(model, "集作四科");//獲取查詢集作三科的案件
        //       SetExcelCell(sheet, 1, 6, styleHead12, "集作四科");
        //       sheet.AddMergedRegion(new CellRangeAddress(1, 1, 6, 7));
        //    }            
        //    if (model.Depart == "0")//*全部
        //    {
        //        sheet2 = workbook.CreateSheet("集作二科");
        //        sheet3 = workbook.CreateSheet("集作三科");
        //        sheet4 = workbook.CreateSheet("集作四科");
        //        dt2 = GetList1(model, "集作二科");//獲取查詢集作二科的案件
        //        dt3 = GetList1(model, "集作三科");//獲取查詢集作三科的案件
        //        dt4 = GetList1(model, "集作四科");//獲取查詢集作四科的案件
        //        SetExcelCell(sheet2, 1, 6, styleHead12, "集作二科");
        //        sheet2.AddMergedRegion(new CellRangeAddress(1, 1, 6, 7));
        //        SetExcelCell(sheet3, 1, 6, styleHead12, "集作三科");
        //        sheet3.AddMergedRegion(new CellRangeAddress(1, 1, 6, 7));
        //        SetExcelCell(sheet4, 1, 6, styleHead12, "集作四科");
        //        sheet4.AddMergedRegion(new CellRangeAddress(1, 1, 6, 7));
        //    }
        //    //20170630 緊急 RQ-2015-019666-018 派件至跨單位 宏祥 add&update end
        //    #endregion

        //    #region title
        //    SetExcelCell(sheet, 0, 0, styleHead12, "用印簿");
        //    sheet.AddMergedRegion(new CellRangeAddress(0, 0, 0, 8));

        //    //* line1
        //    SetExcelCell(sheet, 1, 0, styleHead12, "收件日期：");
        //    sheet.AddMergedRegion(new CellRangeAddress(1, 1, 0, 0));
        //    SetExcelCell(sheet, 1, 1, styleHead12, model.ReceiveDateStart + '~' + model.ReceiveDateEnd);
        //    sheet.AddMergedRegion(new CellRangeAddress(1, 1, 1, 2));
        //    SetExcelCell(sheet, 1, 4, styleHead12, "部門別/科別：");
        //    sheet.AddMergedRegion(new CellRangeAddress(1, 1, 4, 5));

        //    SetExcelCell(sheet, 2, 0, styleHead12, "發文日期：");
        //    sheet.AddMergedRegion(new CellRangeAddress(2, 2, 0, 0));
        //    SetExcelCell(sheet, 2, 1, styleHead12, model.SendDateStart + '~' + model.SendDateEnd);
        //    sheet.AddMergedRegion(new CellRangeAddress(2, 2, 1, 2));
        //    SetExcelCell(sheet, 3, 0, styleHead12, "主管放行日：");
        //    sheet.AddMergedRegion(new CellRangeAddress(3, 3, 0, 0));
        //    SetExcelCell(sheet, 3, 1, styleHead12, model.ApproveDateStart + '~' + model.ApproveDateEnd);
        //    sheet.AddMergedRegion(new CellRangeAddress(3, 3, 1, 2));

        //    //SetExcelCell(sheet, 4, 0, styleHead12, "電子發文上傳日：");
        //    //sheet.AddMergedRegion(new CellRangeAddress(4, 4, 0, 0));
        //    //SetExcelCell(sheet, 4, 1, styleHead12, model.SendUpDateStart + '~' + model.SendUpDateEnd);
        //    //sheet.AddMergedRegion(new CellRangeAddress(4, 4, 1, 2));

        //    //SetExcelCell(sheet, 5, 0, styleHead12, "發文方式：");
        //    //sheet.AddMergedRegion(new CellRangeAddress(5, 5, 0, 0));
        //    //SetExcelCell(sheet, 5, 1, styleHead12, model.SendKind);
        //    //sheet.AddMergedRegion(new CellRangeAddress(5, 5, 1, 2));

        //    //* line2
        //    SetExcelCell(sheet, 4, 0, styleHead10, "發文字號");
        //    sheet.AddMergedRegion(new CellRangeAddress(4, 4, 0, 0));
        //    SetExcelCell(sheet, 4, 1, styleHead10, "案件編號");
        //    sheet.AddMergedRegion(new CellRangeAddress(4, 4, 1, 1));
        //    SetExcelCell(sheet, 4, 2, styleHead10, "受文者");
        //    sheet.AddMergedRegion(new CellRangeAddress(4, 4, 2, 2));
        //    SetExcelCell(sheet, 4, 3, styleHead10, "副本");
        //    sheet.AddMergedRegion(new CellRangeAddress(4, 4, 3, 3));
        //    SetExcelCell(sheet, 4, 4, styleHead10, "經辦");
        //    sheet.AddMergedRegion(new CellRangeAddress(4, 4, 4, 4));
        //    SetExcelCell(sheet, 4, 5, styleHead10, "案件大類");
        //    sheet.AddMergedRegion(new CellRangeAddress(4, 4, 5, 5));
        //    SetExcelCell(sheet, 4, 6, styleHead10, "案件細類");
        //    sheet.AddMergedRegion(new CellRangeAddress(4, 4, 6, 6));
        //    SetExcelCell(sheet, 4, 7, styleHead10, "放行主管");
        //    sheet.AddMergedRegion(new CellRangeAddress(4, 4, 7, 7));
        //    SetExcelCell(sheet, 4, 8, styleHead10, "逾期註記");
        //    sheet.AddMergedRegion(new CellRangeAddress(4, 4, 8, 8));
        //    //SetExcelCell(sheet, 4, 9, styleHead10, "發文方式");
        //    //sheet.AddMergedRegion(new CellRangeAddress(4, 4, 9, 9));
        //    //SetExcelCell(sheet, 4, 10, styleHead10, "電子發文上傳日");
        //    //sheet.AddMergedRegion(new CellRangeAddress(4, 4, 10, 10));

        //    SetExcelCell(sheet, dt.Rows.Count + 6, 0, styleHead10, "案件類別");
        //    sheet.AddMergedRegion(new CellRangeAddress(dt.Rows.Count + 6, dt.Rows.Count + 6, 0, 0));
        //    SetExcelCell(sheet, dt.Rows.Count + 6, 1, styleHead10, "扣押");
        //    sheet.AddMergedRegion(new CellRangeAddress(dt.Rows.Count + 6, dt.Rows.Count + 6, 1, 1));
        //    SetExcelCell(sheet, dt.Rows.Count + 6, 2, styleHead10, "支付");
        //    sheet.AddMergedRegion(new CellRangeAddress(dt.Rows.Count + 6, dt.Rows.Count + 6, 2, 2));
        //    SetExcelCell(sheet, dt.Rows.Count + 6, 3, styleHead10, "扣押並支付");
        //    sheet.AddMergedRegion(new CellRangeAddress(dt.Rows.Count + 6, dt.Rows.Count + 6, 3, 3));
        //    SetExcelCell(sheet, dt.Rows.Count + 6, 4, styleHead10, "外來文案件");
        //    sheet.AddMergedRegion(new CellRangeAddress(dt.Rows.Count + 6, dt.Rows.Count + 6, 4, 4));
        //    SetExcelCell(sheet, dt.Rows.Count + 6, 5, styleHead10, "合計");
        //    sheet.AddMergedRegion(new CellRangeAddress(dt.Rows.Count + 6, dt.Rows.Count + 6, 5, 5));

        //    SetExcelCell(sheet, dt.Rows.Count + 7, 0, styleHead10, "件數");
        //    sheet.AddMergedRegion(new CellRangeAddress(dt.Rows.Count + 7, dt.Rows.Count + 7, 0, 0));

        //    SetExcelCell(sheet, dt.Rows.Count + 9, 4, styleHead12, "覆核主管");
        //    sheet.AddMergedRegion(new CellRangeAddress(dt.Rows.Count + 9, dt.Rows.Count + 9, 4, 4));
        //    SetExcelCell(sheet, dt.Rows.Count + 9, 6, styleHead12, "覆核人員");
        //    sheet.AddMergedRegion(new CellRangeAddress(dt.Rows.Count + 9, dt.Rows.Count + 9, 6, 6));

        //    for (int i = dt.Rows.Count + 7; i < dt.Rows.Count + 8; i++)//*初始表格賦初值 
        //    {
        //        for (int j = 0; j < 5; j++)
        //        {
        //            SetExcelCell(sheet, i, j + 1, styleHead10, "0");
        //            sheet.AddMergedRegion(new CellRangeAddress(i, i, j + 1, j + 1));
        //        }
        //    }
        //    #endregion
        //    #region Width
        //    sheet.SetColumnWidth(0, 100 * 100);
        //    sheet.SetColumnWidth(1, 100 * 40);
        //    sheet.SetColumnWidth(2, 100 * 100);
        //    sheet.SetColumnWidth(3, 100 * 100);
        //    sheet.SetColumnWidth(4, 100 * 30);
        //    sheet.SetColumnWidth(5, 100 * 30);
        //    sheet.SetColumnWidth(6, 100 * 30);
        //    sheet.SetColumnWidth(7, 100 * 40);
        //    sheet.SetColumnWidth(8, 100 * 30);
        //    //sheet.SetColumnWidth(9, 100 * 40);
        //    //sheet.SetColumnWidth(10, 100 * 40);
        //    #endregion
        //    #region body
        //    for (int iRow = 0; iRow < dt.Rows.Count; iRow++)
        //    {
        //        for (int iCol = 1; iCol < dt.Columns.Count - 2; iCol++)
        //        {
        //            SetExcelCell(sheet, iRow + 5, iCol - 1, styleHead10, dt.Rows[iRow][iCol].ToString());
        //        }
        //    }
        //    //總計
        //    int rows = dt.Rows.Count + 7;
        //    DataRow[] drkouya = dt.Select(" CaseKind2 = '扣押'");
        //    SetExcelCell(sheet, rows, 1, styleHead10, drkouya.Length.ToString());
        //    DataRow[] drzhifu = dt.Select(" CaseKind2 = '支付'");
        //    SetExcelCell(sheet, rows, 2, styleHead10, drzhifu.Length.ToString());
        //    DataRow[] drkouyaandPay = dt.Select(" CaseKind2 = '扣押並支付'");
        //    SetExcelCell(sheet, rows, 3, styleHead10, drkouyaandPay.Length.ToString());
        //    DataRow[] drWai = dt.Select(" CaseKind = '外來文案件'");
        //    SetExcelCell(sheet, rows, 4, styleHead10, drWai.Length.ToString());
        //    SetExcelCell(sheet, rows, 5, styleHead10, dt.Rows.Count.ToString());
        //    #endregion

        //    if (model.Depart == "0")//* 全部
        //    {
        //        #region title2
        //        SetExcelCell(sheet2, 0, 0, styleHead12, "用印簿");
        //        sheet2.AddMergedRegion(new CellRangeAddress(0, 0, 0, 8));

        //        //* line1
        //        SetExcelCell(sheet2, 1, 0, styleHead12, "收件日期：");
        //        sheet2.AddMergedRegion(new CellRangeAddress(1, 1, 0, 0));
        //        SetExcelCell(sheet2, 1, 1, styleHead12, model.ReceiveDateStart + '~' + model.ReceiveDateEnd);
        //        sheet2.AddMergedRegion(new CellRangeAddress(1, 1, 1, 2));
        //        SetExcelCell(sheet2, 1, 4, styleHead12, "部門別/科別：");
        //        sheet2.AddMergedRegion(new CellRangeAddress(1, 1, 4, 5));


        //        SetExcelCell(sheet2, 2, 0, styleHead12, "發文日期：");
        //        sheet2.AddMergedRegion(new CellRangeAddress(2, 2, 0, 0));
        //        SetExcelCell(sheet2, 2, 1, styleHead12, model.SendDateStart + '~' + model.SendDateEnd);
        //        sheet2.AddMergedRegion(new CellRangeAddress(2, 2, 1, 2));
        //        SetExcelCell(sheet2, 3, 0, styleHead12, "主管放行日：");
        //        sheet2.AddMergedRegion(new CellRangeAddress(3, 3, 0, 0));
        //        SetExcelCell(sheet2, 3, 1, styleHead12, model.ApproveDateStart + '~' + model.ApproveDateEnd);
        //        sheet2.AddMergedRegion(new CellRangeAddress(3, 3, 1, 2));

        //        //SetExcelCell(sheet2, 4, 0, styleHead12, "電子發文上傳日：");
        //        //sheet2.AddMergedRegion(new CellRangeAddress(4, 4, 0, 0));
        //        //SetExcelCell(sheet2, 4, 1, styleHead12, model.SendUpDateStart + '~' + model.SendUpDateEnd);
        //        //sheet2.AddMergedRegion(new CellRangeAddress(4, 4, 1, 2));
        //        //SetExcelCell(sheet2, 5, 0, styleHead12, "發文方式：");
        //        //sheet2.AddMergedRegion(new CellRangeAddress(5, 5, 0, 0));
        //        //SetExcelCell(sheet2, 5, 1, styleHead12, model.SendKind);
        //        //sheet2.AddMergedRegion(new CellRangeAddress(5, 5, 1, 2));

        //        //* line2
        //        SetExcelCell(sheet2, 4, 0, styleHead10, "發文字號");
        //        sheet2.AddMergedRegion(new CellRangeAddress(4, 4, 0, 0));
        //        SetExcelCell(sheet2, 4, 1, styleHead10, "案件編號");
        //        sheet2.AddMergedRegion(new CellRangeAddress(4, 4, 1, 1));
        //        SetExcelCell(sheet2, 4, 2, styleHead10, "受文者");
        //        sheet2.AddMergedRegion(new CellRangeAddress(4, 4, 2, 2));
        //        SetExcelCell(sheet2, 4, 3, styleHead10, "副本");
        //        sheet2.AddMergedRegion(new CellRangeAddress(4, 4, 3, 3));
        //        SetExcelCell(sheet2, 4, 4, styleHead10, "經辦");
        //        sheet2.AddMergedRegion(new CellRangeAddress(4, 4, 4, 4));
        //        SetExcelCell(sheet2, 4, 5, styleHead10, "案件大類");
        //        sheet2.AddMergedRegion(new CellRangeAddress(4, 4, 5, 5));
        //        SetExcelCell(sheet2, 4, 6, styleHead10, "案件細類");
        //        sheet2.AddMergedRegion(new CellRangeAddress(4, 4, 6, 6));
        //        SetExcelCell(sheet2, 4, 7, styleHead10, "放行主管");
        //        sheet2.AddMergedRegion(new CellRangeAddress(4, 4, 7, 7));
        //        SetExcelCell(sheet2, 4, 8, styleHead10, "逾期註記");
        //        sheet2.AddMergedRegion(new CellRangeAddress(4, 4, 8, 8));
        //        //SetExcelCell(sheet2, 4, 9, styleHead10, "發文方式");
        //        //sheet2.AddMergedRegion(new CellRangeAddress(4, 4, 9, 9));
        //        //SetExcelCell(sheet2, 4, 10, styleHead10, "電子發文上傳日");
        //        //sheet2.AddMergedRegion(new CellRangeAddress(4, 4, 10, 10));

        //        int dtrows2 = dt2.Rows.Count + 6;
        //        SetExcelCell(sheet2, dtrows2, 0, styleHead10, "案件類別");
        //        sheet2.AddMergedRegion(new CellRangeAddress(dtrows2, dtrows2, 0, 0));
        //        SetExcelCell(sheet2, dtrows2, 1, styleHead10, "扣押");
        //        sheet2.AddMergedRegion(new CellRangeAddress(dtrows2, dtrows2, 1, 1));
        //        SetExcelCell(sheet2, dtrows2, 2, styleHead10, "支付");
        //        sheet2.AddMergedRegion(new CellRangeAddress(dtrows2, dtrows2, 2, 2));
        //        SetExcelCell(sheet2, dtrows2, 3, styleHead10, "扣押並支付");
        //        sheet2.AddMergedRegion(new CellRangeAddress(dtrows2, dtrows2, 3, 3));
        //        SetExcelCell(sheet2, dtrows2, 4, styleHead10, "外來文案件");
        //        sheet2.AddMergedRegion(new CellRangeAddress(dtrows2, dtrows2, 4, 4));
        //        SetExcelCell(sheet2, dtrows2, 5, styleHead10, "合計");
        //        sheet2.AddMergedRegion(new CellRangeAddress(dtrows2, dtrows2, 5, 5));

        //        SetExcelCell(sheet2, dt2.Rows.Count + 7, 0, styleHead10, "件數");
        //        sheet2.AddMergedRegion(new CellRangeAddress(dt2.Rows.Count + 7, dt2.Rows.Count + 7, 0, 0));

        //        SetExcelCell(sheet2, dt2.Rows.Count + 9, 4, styleHead12, "覆核主管");
        //        sheet2.AddMergedRegion(new CellRangeAddress(dt2.Rows.Count + 9, dt2.Rows.Count + 9, 4, 4));
        //        SetExcelCell(sheet2, dt2.Rows.Count + 9, 6, styleHead12, "覆核人員");
        //        sheet2.AddMergedRegion(new CellRangeAddress(dt2.Rows.Count + 9, dt2.Rows.Count + 9, 6, 6));

        //        for (int i = dt2.Rows.Count + 7; i < dt2.Rows.Count + 8; i++)//*初始表格賦初值 
        //        {
        //            for (int j = 0; j < 5; j++)
        //            {
        //                SetExcelCell(sheet2, i, j + 1, styleHead10, "0");
        //                sheet2.AddMergedRegion(new CellRangeAddress(i, i, j + 1, j + 1));
        //            }
        //        }
        //        #endregion
        //        #region title3
        //        SetExcelCell(sheet3, 0, 0, styleHead12, "用印簿");
        //        sheet3.AddMergedRegion(new CellRangeAddress(0, 0, 0, 8));

        //        //* line1
        //        SetExcelCell(sheet3, 1, 0, styleHead12, "收件日期：");
        //        sheet3.AddMergedRegion(new CellRangeAddress(1, 1, 0, 0));
        //        SetExcelCell(sheet3, 1, 1, styleHead12, model.ReceiveDateStart + '~' + model.ReceiveDateEnd);
        //        sheet3.AddMergedRegion(new CellRangeAddress(1, 1, 1, 2));
        //        SetExcelCell(sheet3, 1, 4, styleHead12, "部門別/科別：");
        //        sheet3.AddMergedRegion(new CellRangeAddress(1, 1, 4, 5));

        //        SetExcelCell(sheet3, 2, 0, styleHead12, "發文日期：");
        //        sheet3.AddMergedRegion(new CellRangeAddress(2, 2, 0, 0));
        //        SetExcelCell(sheet3, 2, 1, styleHead12, model.SendDateStart + '~' + model.SendDateEnd);
        //        sheet3.AddMergedRegion(new CellRangeAddress(2, 2, 1, 2));
        //        SetExcelCell(sheet3, 3, 0, styleHead12, "主管放行日：");
        //        sheet3.AddMergedRegion(new CellRangeAddress(3, 3, 0, 0));
        //        SetExcelCell(sheet3, 3, 1, styleHead12, model.ApproveDateStart + '~' + model.ApproveDateEnd);
        //        sheet3.AddMergedRegion(new CellRangeAddress(3, 3, 1, 2));

        //        //SetExcelCell(sheet3, 4, 0, styleHead12, "電子發文上傳日：");
        //        //sheet3.AddMergedRegion(new CellRangeAddress(4, 4, 0, 0));
        //        //SetExcelCell(sheet3, 4, 1, styleHead12, model.SendUpDateStart + '~' + model.SendUpDateEnd);
        //        //sheet3.AddMergedRegion(new CellRangeAddress(4, 4, 1, 2));
        //        //SetExcelCell(sheet3, 5, 0, styleHead12, "發文方式：");
        //        //sheet3.AddMergedRegion(new CellRangeAddress(5, 5, 0, 0));
        //        //SetExcelCell(sheet3, 5, 1, styleHead12, model.SendKind);
        //        //sheet3.AddMergedRegion(new CellRangeAddress(5, 5, 1, 2));

        //        //* line2
        //        SetExcelCell(sheet3, 4, 0, styleHead10, "發文字號");
        //        sheet3.AddMergedRegion(new CellRangeAddress(4, 4, 0, 0));
        //        SetExcelCell(sheet3, 4, 1, styleHead10, "案件編號");
        //        sheet3.AddMergedRegion(new CellRangeAddress(4, 4, 1, 1));
        //        SetExcelCell(sheet3, 4, 2, styleHead10, "受文者");
        //        sheet3.AddMergedRegion(new CellRangeAddress(4, 4, 2, 2));
        //        SetExcelCell(sheet3, 4, 3, styleHead10, "副本");
        //        sheet3.AddMergedRegion(new CellRangeAddress(4, 4, 3, 3));
        //        SetExcelCell(sheet3, 4, 4, styleHead10, "經辦");
        //        sheet3.AddMergedRegion(new CellRangeAddress(4, 4, 4, 4));
        //        SetExcelCell(sheet3, 4, 5, styleHead10, "案件大類");
        //        sheet3.AddMergedRegion(new CellRangeAddress(4, 4, 5, 5));
        //        SetExcelCell(sheet3, 4, 6, styleHead10, "案件細類");
        //        sheet3.AddMergedRegion(new CellRangeAddress(4, 4, 6, 6));
        //        SetExcelCell(sheet3, 4, 7, styleHead10, "放行主管");
        //        sheet3.AddMergedRegion(new CellRangeAddress(4, 4, 7, 7));
        //        SetExcelCell(sheet3, 4, 8, styleHead10, "逾期註記");
        //        sheet3.AddMergedRegion(new CellRangeAddress(4, 4, 8, 8));
        //        //SetExcelCell(sheet3, 4, 9, styleHead10, "發文方式");
        //        //sheet3.AddMergedRegion(new CellRangeAddress(4, 4, 9, 9));
        //        //SetExcelCell(sheet3, 4, 10, styleHead10, "電子發文上傳日");
        //        //sheet3.AddMergedRegion(new CellRangeAddress(4, 4, 10, 10));

        //        int countRows = dt3.Rows.Count + 6;
        //        SetExcelCell(sheet3, countRows, 0, styleHead10, "案件類別");
        //        sheet3.AddMergedRegion(new CellRangeAddress(countRows, countRows, 0, 0));
        //        SetExcelCell(sheet3, countRows, 1, styleHead10, "扣押");
        //        sheet3.AddMergedRegion(new CellRangeAddress(countRows, countRows, 1, 1));
        //        SetExcelCell(sheet3, countRows, 2, styleHead10, "支付");
        //        sheet3.AddMergedRegion(new CellRangeAddress(countRows, countRows, 2, 2));
        //        SetExcelCell(sheet3, countRows, 3, styleHead10, "扣押並支付");
        //        sheet3.AddMergedRegion(new CellRangeAddress(countRows, countRows, 3, 3));
        //        SetExcelCell(sheet3, countRows, 4, styleHead10, "外來文案件");
        //        sheet3.AddMergedRegion(new CellRangeAddress(countRows, countRows, 4, 4));
        //        SetExcelCell(sheet3, countRows, 5, styleHead10, "合計");
        //        sheet3.AddMergedRegion(new CellRangeAddress(countRows, countRows, 5, 5));

        //        int countRows1 = dt3.Rows.Count + 7;
        //        SetExcelCell(sheet3, countRows1, 0, styleHead10, "件數");
        //        sheet3.AddMergedRegion(new CellRangeAddress(countRows1, countRows1, 0, 0));

        //        int countRows2 = dt3.Rows.Count + 9;
        //        SetExcelCell(sheet3, countRows2, 4, styleHead12, "覆核主管");
        //        sheet3.AddMergedRegion(new CellRangeAddress(countRows2, countRows2, 4, 4));
        //        SetExcelCell(sheet3, countRows2, 6, styleHead12, "覆核人員");
        //        sheet3.AddMergedRegion(new CellRangeAddress(countRows2, countRows2, 6, 6));

        //        for (int i = dt3.Rows.Count + 7; i < dt3.Rows.Count + 8; i++)//*初始表格賦初值 
        //        {
        //            for (int j = 0; j < 5; j++)
        //            {
        //                SetExcelCell(sheet3, i, j + 1, styleHead10, "0");
        //                sheet3.AddMergedRegion(new CellRangeAddress(i, i, j + 1, j + 1));
        //            }
        //        }
        //        #endregion
        //        //20170630 緊急 RQ-2015-019666-018 派件至跨單位 宏祥 add start
        //        #region title4
        //        SetExcelCell(sheet4, 0, 0, styleHead12, "用印簿");
        //        sheet4.AddMergedRegion(new CellRangeAddress(0, 0, 0, 8));

        //        //* line1
        //        SetExcelCell(sheet4, 1, 0, styleHead12, "收件日期：");
        //        sheet4.AddMergedRegion(new CellRangeAddress(1, 1, 0, 0));
        //        SetExcelCell(sheet4, 1, 1, styleHead12, model.ReceiveDateStart + '~' + model.ReceiveDateEnd);
        //        sheet4.AddMergedRegion(new CellRangeAddress(1, 1, 1, 2));
        //        SetExcelCell(sheet4, 1, 4, styleHead12, "部門別/科別：");
        //        sheet4.AddMergedRegion(new CellRangeAddress(1, 1, 4, 5));

        //        SetExcelCell(sheet4, 2, 0, styleHead12, "發文日期：");
        //        sheet4.AddMergedRegion(new CellRangeAddress(2, 2, 0, 0));
        //        SetExcelCell(sheet4, 2, 1, styleHead12, model.SendDateStart + '~' + model.SendDateEnd);
        //        sheet4.AddMergedRegion(new CellRangeAddress(2, 2, 1, 2));
        //        SetExcelCell(sheet4, 3, 0, styleHead12, "主管放行日：");
        //        sheet4.AddMergedRegion(new CellRangeAddress(3, 3, 0, 0));
        //        SetExcelCell(sheet4, 3, 1, styleHead12, model.ApproveDateStart + '~' + model.ApproveDateEnd);
        //        sheet4.AddMergedRegion(new CellRangeAddress(3, 3, 1, 2));

        //        //SetExcelCell(sheet4, 4, 0, styleHead12, "電子發文上傳日：");
        //        //sheet4.AddMergedRegion(new CellRangeAddress(4, 4, 0, 0));
        //        //SetExcelCell(sheet4, 4, 1, styleHead12, model.SendUpDateStart + '~' + model.SendUpDateEnd);
        //        //sheet4.AddMergedRegion(new CellRangeAddress(4, 4, 1, 2));
        //        //SetExcelCell(sheet4, 5, 0, styleHead12, "發文方式：");
        //        //sheet4.AddMergedRegion(new CellRangeAddress(5, 5, 0, 0));
        //        //SetExcelCell(sheet4, 5, 1, styleHead12, model.SendKind);
        //        //sheet4.AddMergedRegion(new CellRangeAddress(5, 5, 1, 2));

        //        //* line2
        //        SetExcelCell(sheet4, 4, 0, styleHead10, "發文字號");
        //        sheet4.AddMergedRegion(new CellRangeAddress(4, 4, 0, 0));
        //        SetExcelCell(sheet4, 4, 1, styleHead10, "案件編號");
        //        sheet4.AddMergedRegion(new CellRangeAddress(4, 4, 1, 1));
        //        SetExcelCell(sheet4, 4, 2, styleHead10, "受文者");
        //        sheet4.AddMergedRegion(new CellRangeAddress(4, 4, 2, 2));
        //        SetExcelCell(sheet4, 4, 3, styleHead10, "副本");
        //        sheet4.AddMergedRegion(new CellRangeAddress(4, 4, 3, 3));
        //        SetExcelCell(sheet4, 4, 4, styleHead10, "經辦");
        //        sheet4.AddMergedRegion(new CellRangeAddress(4, 4, 4, 4));
        //        SetExcelCell(sheet4, 4, 5, styleHead10, "案件大類");
        //        sheet4.AddMergedRegion(new CellRangeAddress(4, 4, 5, 5));
        //        SetExcelCell(sheet4, 4, 6, styleHead10, "案件細類");
        //        sheet4.AddMergedRegion(new CellRangeAddress(4, 4, 6, 6));
        //        SetExcelCell(sheet4, 4, 7, styleHead10, "放行主管");
        //        sheet4.AddMergedRegion(new CellRangeAddress(4, 4, 7, 7));
        //        SetExcelCell(sheet4, 4, 8, styleHead10, "逾期註記");
        //        sheet4.AddMergedRegion(new CellRangeAddress(4, 4, 8, 8));
        //        //SetExcelCell(sheet3, 4, 9, styleHead10, "發文方式");
        //        //sheet3.AddMergedRegion(new CellRangeAddress(4, 4, 9, 9));
        //        //SetExcelCell(sheet3, 4, 10, styleHead10, "電子發文上傳日");
        //        //sheet3.AddMergedRegion(new CellRangeAddress(4, 4, 10, 10));

        //        int countRows4 = dt4.Rows.Count + 6;
        //        SetExcelCell(sheet4, countRows4, 0, styleHead10, "案件類別");
        //        sheet4.AddMergedRegion(new CellRangeAddress(countRows4, countRows4, 0, 0));
        //        SetExcelCell(sheet4, countRows4, 1, styleHead10, "扣押");
        //        sheet4.AddMergedRegion(new CellRangeAddress(countRows4, countRows4, 1, 1));
        //        SetExcelCell(sheet4, countRows4, 2, styleHead10, "支付");
        //        sheet4.AddMergedRegion(new CellRangeAddress(countRows4, countRows4, 2, 2));
        //        SetExcelCell(sheet4, countRows4, 3, styleHead10, "扣押並支付");
        //        sheet4.AddMergedRegion(new CellRangeAddress(countRows4, countRows4, 3, 3));
        //        SetExcelCell(sheet4, countRows4, 4, styleHead10, "外來文案件");
        //        sheet4.AddMergedRegion(new CellRangeAddress(countRows4, countRows4, 4, 4));
        //        SetExcelCell(sheet4, countRows4, 5, styleHead10, "合計");
        //        sheet4.AddMergedRegion(new CellRangeAddress(countRows4, countRows4, 5, 5));

        //        SetExcelCell(sheet4, dt4.Rows.Count + 7, 0, styleHead10, "件數");
        //        sheet4.AddMergedRegion(new CellRangeAddress(dt4.Rows.Count + 7, dt4.Rows.Count + 7, 0, 0));

        //        SetExcelCell(sheet4, dt4.Rows.Count + 9, 4, styleHead12, "覆核主管");
        //        sheet4.AddMergedRegion(new CellRangeAddress(dt4.Rows.Count + 9, dt4.Rows.Count + 9, 4, 4));
        //        SetExcelCell(sheet4, dt4.Rows.Count + 9, 6, styleHead12, "覆核人員");
        //        sheet4.AddMergedRegion(new CellRangeAddress(dt4.Rows.Count + 9, dt4.Rows.Count + 9, 6, 6));

        //        for (int i = dt4.Rows.Count + 7; i < dt4.Rows.Count + 8; i++)//*初始表格賦初值 
        //        {
        //           for (int j = 0; j < 5; j++)
        //           {
        //              SetExcelCell(sheet4, i, j + 1, styleHead10, "0");
        //              sheet4.AddMergedRegion(new CellRangeAddress(i, i, j + 1, j + 1));
        //           }
        //        }
        //        #endregion
        //        //20170630 緊急 RQ-2015-019666-018 派件至跨單位 宏祥 add end
        //        #region Width2
        //        sheet2.SetColumnWidth(0, 100 * 100);
        //        sheet2.SetColumnWidth(1, 100 * 40);
        //        sheet2.SetColumnWidth(2, 100 * 100);
        //        sheet2.SetColumnWidth(3, 100 * 100);
        //        sheet2.SetColumnWidth(4, 100 * 30);
        //        sheet2.SetColumnWidth(5, 100 * 30);
        //        sheet2.SetColumnWidth(6, 100 * 30);
        //        sheet2.SetColumnWidth(7, 100 * 40);
        //        sheet2.SetColumnWidth(8, 100 * 30);
        //        //sheet2.SetColumnWidth(9, 100 * 40);
        //        //sheet2.SetColumnWidth(10, 100 * 40);
        //        #endregion
        //        #region Width3
        //        sheet3.SetColumnWidth(0, 100 * 100);
        //        sheet3.SetColumnWidth(1, 100 * 40);
        //        sheet3.SetColumnWidth(2, 100 * 100);
        //        sheet3.SetColumnWidth(3, 100 * 100);
        //        sheet3.SetColumnWidth(4, 100 * 30);
        //        sheet3.SetColumnWidth(5, 100 * 30);
        //        sheet3.SetColumnWidth(6, 100 * 30);
        //        sheet3.SetColumnWidth(7, 100 * 40);
        //        sheet3.SetColumnWidth(8, 100 * 30);
        //        //sheet3.SetColumnWidth(9, 100 * 40);
        //        //sheet3.SetColumnWidth(10, 100 * 40);
        //        #endregion
        //        //20170630 緊急 RQ-2015-019666-018 派件至跨單位 宏祥 add start
        //        #region Width4
        //        sheet4.SetColumnWidth(0, 100 * 100);
        //        sheet4.SetColumnWidth(1, 100 * 40);
        //        sheet4.SetColumnWidth(2, 100 * 100);
        //        sheet4.SetColumnWidth(3, 100 * 100);
        //        sheet4.SetColumnWidth(4, 100 * 30);
        //        sheet4.SetColumnWidth(5, 100 * 30);
        //        sheet4.SetColumnWidth(6, 100 * 30);
        //        sheet4.SetColumnWidth(7, 100 * 40);
        //        sheet4.SetColumnWidth(8, 100 * 30);
        //        //sheet3.SetColumnWidth(9, 100 * 40);
        //        //sheet3.SetColumnWidth(10, 100 * 40);
        //        #endregion
        //        //20170630 緊急 RQ-2015-019666-018 派件至跨單位 宏祥 add end
        //        #region body2
        //        for (int iRow = 0; iRow < dt2.Rows.Count; iRow++)
        //        {
        //            for (int iCol = 1; iCol < dt2.Columns.Count - 2; iCol++)
        //            {
        //                SetExcelCell(sheet2, iRow + 5, iCol - 1, styleHead10, dt2.Rows[iRow][iCol].ToString());
        //            }
        //        }
        //        //總計
        //        int rows2 = dt2.Rows.Count + 7;
        //        DataRow[] drkouya2 = dt2.Select(" CaseKind2 = '扣押'");
        //        SetExcelCell(sheet2, rows2, 1, styleHead10, drkouya2.Length.ToString());
        //        DataRow[] drzhifu2 = dt2.Select(" CaseKind2 = '支付'");
        //        SetExcelCell(sheet2, rows2, 2, styleHead10, drzhifu2.Length.ToString());
        //        DataRow[] drkouyaandPay2 = dt2.Select(" CaseKind2 = '扣押並支付'");
        //        SetExcelCell(sheet2, rows2, 3, styleHead10, drkouyaandPay2.Length.ToString());
        //        DataRow[] drWai2 = dt2.Select(" CaseKind = '外來文案件'");
        //        SetExcelCell(sheet2, rows2, 4, styleHead10, drWai2.Length.ToString());
        //        SetExcelCell(sheet2, rows2, 5, styleHead10, dt2.Rows.Count.ToString());
        //        #endregion
        //        #region body3
        //        for (int iRow = 0; iRow < dt3.Rows.Count; iRow++)
        //        {
        //            //20170630 緊急 RQ-2015-019666-018 派件至跨單位 宏祥 update start
        //            for (int iCol = 1; iCol < dt3.Columns.Count - 2; iCol++)
        //            {
        //                SetExcelCell(sheet3, iRow + 5, iCol - 1, styleHead10, dt3.Rows[iRow][iCol].ToString());
        //            }
        //            //20170630 緊急 RQ-2015-019666-018 派件至跨單位 宏祥 update end
        //        }
        //        //總計
        //        int rows3 = dt3.Rows.Count + 7;
        //        DataRow[] drkouya3 = dt3.Select(" CaseKind2 = '扣押'");
        //        SetExcelCell(sheet3, rows3, 1, styleHead10, drkouya3.Length.ToString());
        //        DataRow[] drzhifu3 = dt3.Select(" CaseKind2 = '支付'");
        //        SetExcelCell(sheet3, rows3, 2, styleHead10, drzhifu3.Length.ToString());
        //        DataRow[] drkouyaandPay3 = dt3.Select(" CaseKind2 = '扣押並支付'");
        //        SetExcelCell(sheet3, rows3, 3, styleHead10, drkouyaandPay3.Length.ToString());
        //        DataRow[] drWai3 = dt3.Select(" CaseKind = '外來文案件'");
        //        SetExcelCell(sheet3, rows3, 4, styleHead10, drWai3.Length.ToString());
        //        SetExcelCell(sheet3, rows3, 5, styleHead10, dt3.Rows.Count.ToString());
        //        #endregion
        //        //20170630 緊急 RQ-2015-019666-018 派件至跨單位 宏祥 add start
        //        #region body4
        //        for (int iRow = 0; iRow < dt4.Rows.Count; iRow++)
        //        {
        //           for (int iCol = 1; iCol < dt4.Columns.Count - 2; iCol++)
        //           {
        //              SetExcelCell(sheet4, iRow + 5, iCol - 1, styleHead10, dt4.Rows[iRow][iCol].ToString());
        //           }
        //        }
        //        //總計
        //        int rows4 = dt4.Rows.Count + 7;
        //        DataRow[] drkouya4 = dt4.Select(" CaseKind2 = '扣押'");
        //        SetExcelCell(sheet4, rows4, 1, styleHead10, drkouya4.Length.ToString());
        //        DataRow[] drzhifu4 = dt4.Select(" CaseKind2 = '支付'");
        //        SetExcelCell(sheet4, rows4, 2, styleHead10, drzhifu4.Length.ToString());
        //        DataRow[] drkouyaandPay4 = dt4.Select(" CaseKind2 = '扣押並支付'");
        //        SetExcelCell(sheet4, rows4, 3, styleHead10, drkouyaandPay4.Length.ToString());
        //        DataRow[] drWai4 = dt4.Select(" CaseKind = '外來文案件'");
        //        SetExcelCell(sheet4, rows4, 4, styleHead10, drWai4.Length.ToString());
        //        SetExcelCell(sheet4, rows4, 5, styleHead10, dt4.Rows.Count.ToString());
        //        #endregion
        //        //20170630 緊急 RQ-2015-019666-018 派件至跨單位 宏祥 add end
        //    }
        //    MemoryStream ms = new MemoryStream();
        //    workbook.Write(ms);
        //    ms.Flush();
        //    ms.Position = 0;
        //    workbook = null;
        //    return ms;
        //}
        //#endregion

        //20170714 固定 RQ-2015-019666-019 派件至跨單位(原產生邏輯程式寫死，組織異動必出問題，改為參數) 宏祥 add start
        #region 用印簿1
        public MemoryStream ListReportExcel_NPOI1(CaseClosedQuery model, IList<PARMCode> listCode)
        {
           IWorkbook workbook = new HSSFWorkbook();
           int Departmentcount = listCode.Count();

           #region def style
           ICellStyle styleHead12 = workbook.CreateCellStyle();
           IFont font12 = workbook.CreateFont();
           font12.FontHeightInPoints = 12;
           font12.FontName = "新細明體";
           styleHead12.FillPattern = FillPattern.SolidForeground;
           styleHead12.FillForegroundColor = HSSFColor.White.Index;
           styleHead12.BorderTop = BorderStyle.None;
           styleHead12.BorderLeft = BorderStyle.None;
           styleHead12.BorderRight = BorderStyle.None;
           styleHead12.BorderBottom = BorderStyle.None;
           styleHead12.WrapText = true;
           styleHead12.Alignment = HorizontalAlignment.Center;
           styleHead12.VerticalAlignment = VerticalAlignment.Center;
           styleHead12.SetFont(font12);

           ICellStyle styleHead10 = workbook.CreateCellStyle();
           IFont font10 = workbook.CreateFont();
           font10.FontHeightInPoints = 10;
           font10.FontName = "新細明體";
           styleHead10.FillPattern = FillPattern.SolidForeground;
           styleHead10.FillForegroundColor = HSSFColor.White.Index;
           styleHead10.BorderTop = BorderStyle.Thin;
           styleHead10.BorderLeft = BorderStyle.Thin;
           styleHead10.BorderRight = BorderStyle.Thin;
           styleHead10.BorderBottom = BorderStyle.Thin;
           styleHead10.WrapText = true;
           styleHead10.Alignment = HorizontalAlignment.Left;
           styleHead10.VerticalAlignment = VerticalAlignment.Center;
           styleHead10.SetFont(font10);
           #endregion

           //判斷科別搜尋條件
           if (model.Depart != "0")
           {
              for (int k = 0; k < Departmentcount; k++)
              {
                 if (model.Depart == (k + 1).ToString())
                 {
                    ISheet sheet = null;
                    DataTable dt = new DataTable();

                    sheet = workbook.CreateSheet(listCode[k].CodeDesc);
                    dt = GetList1(model, listCode[k].CodeDesc);//獲取查詢科別的案件
                    SetExcelCell(sheet, 1, 6, styleHead12, listCode[k].CodeDesc);
                    sheet.AddMergedRegion(new CellRangeAddress(1, 1, 6, 7));

                    #region title
                    SetExcelCell(sheet, 0, 0, styleHead12, "用印簿");
                    sheet.AddMergedRegion(new CellRangeAddress(0, 0, 0, 8));

                    //* line1
                    SetExcelCell(sheet, 1, 0, styleHead12, "收件日期：");
                    sheet.AddMergedRegion(new CellRangeAddress(1, 1, 0, 0));
                    SetExcelCell(sheet, 1, 1, styleHead12, model.ReceiveDateStart + '~' + model.ReceiveDateEnd);
                    sheet.AddMergedRegion(new CellRangeAddress(1, 1, 1, 2));
                    SetExcelCell(sheet, 1, 4, styleHead12, "部門別/科別：");
                    sheet.AddMergedRegion(new CellRangeAddress(1, 1, 4, 5));

                    SetExcelCell(sheet, 2, 0, styleHead12, "發文日期：");
                    sheet.AddMergedRegion(new CellRangeAddress(2, 2, 0, 0));
                    SetExcelCell(sheet, 2, 1, styleHead12, model.SendDateStart + '~' + model.SendDateEnd);
                    sheet.AddMergedRegion(new CellRangeAddress(2, 2, 1, 2));
                    SetExcelCell(sheet, 3, 0, styleHead12, "主管放行日：");
                    sheet.AddMergedRegion(new CellRangeAddress(3, 3, 0, 0));
                    SetExcelCell(sheet, 3, 1, styleHead12, model.ApproveDateStart + '~' + model.ApproveDateEnd);
                    sheet.AddMergedRegion(new CellRangeAddress(3, 3, 1, 2));

                    //SetExcelCell(sheet, 4, 0, styleHead12, "電子發文上傳日：");
                    //sheet.AddMergedRegion(new CellRangeAddress(4, 4, 0, 0));
                    //SetExcelCell(sheet, 4, 1, styleHead12, model.SendUpDateStart + '~' + model.SendUpDateEnd);
                    //sheet.AddMergedRegion(new CellRangeAddress(4, 4, 1, 2));

                    //SetExcelCell(sheet, 5, 0, styleHead12, "發文方式：");
                    //sheet.AddMergedRegion(new CellRangeAddress(5, 5, 0, 0));
                    //SetExcelCell(sheet, 5, 1, styleHead12, model.SendKind);
                    //sheet.AddMergedRegion(new CellRangeAddress(5, 5, 1, 2));

                    //* line2
                    SetExcelCell(sheet, 4, 0, styleHead10, "發文字號");
                    sheet.AddMergedRegion(new CellRangeAddress(4, 4, 0, 0));
                    SetExcelCell(sheet, 4, 1, styleHead10, "案件編號");
                    sheet.AddMergedRegion(new CellRangeAddress(4, 4, 1, 1));
                    SetExcelCell(sheet, 4, 2, styleHead10, "受文者");
                    sheet.AddMergedRegion(new CellRangeAddress(4, 4, 2, 2));
                    SetExcelCell(sheet, 4, 3, styleHead10, "副本");
                    sheet.AddMergedRegion(new CellRangeAddress(4, 4, 3, 3));
                    SetExcelCell(sheet, 4, 4, styleHead10, "經辦");
                    sheet.AddMergedRegion(new CellRangeAddress(4, 4, 4, 4));
                    SetExcelCell(sheet, 4, 5, styleHead10, "案件大類");
                    sheet.AddMergedRegion(new CellRangeAddress(4, 4, 5, 5));
                    SetExcelCell(sheet, 4, 6, styleHead10, "案件細類");
                    sheet.AddMergedRegion(new CellRangeAddress(4, 4, 6, 6));
                    SetExcelCell(sheet, 4, 7, styleHead10, "放行主管");
                    sheet.AddMergedRegion(new CellRangeAddress(4, 4, 7, 7));
                    SetExcelCell(sheet, 4, 8, styleHead10, "逾期註記");
                    sheet.AddMergedRegion(new CellRangeAddress(4, 4, 8, 8));
                    //SetExcelCell(sheet, 4, 9, styleHead10, "發文方式");
                    //sheet.AddMergedRegion(new CellRangeAddress(4, 4, 9, 9));
                    //SetExcelCell(sheet, 4, 10, styleHead10, "電子發文上傳日");
                    //sheet.AddMergedRegion(new CellRangeAddress(4, 4, 10, 10));

                    SetExcelCell(sheet, dt.Rows.Count + 6, 0, styleHead10, "案件類別");
                    sheet.AddMergedRegion(new CellRangeAddress(dt.Rows.Count + 6, dt.Rows.Count + 6, 0, 0));
                    SetExcelCell(sheet, dt.Rows.Count + 6, 1, styleHead10, "扣押");
                    sheet.AddMergedRegion(new CellRangeAddress(dt.Rows.Count + 6, dt.Rows.Count + 6, 1, 1));
                    SetExcelCell(sheet, dt.Rows.Count + 6, 2, styleHead10, "支付");
                    sheet.AddMergedRegion(new CellRangeAddress(dt.Rows.Count + 6, dt.Rows.Count + 6, 2, 2));
                    SetExcelCell(sheet, dt.Rows.Count + 6, 3, styleHead10, "扣押並支付");
                    sheet.AddMergedRegion(new CellRangeAddress(dt.Rows.Count + 6, dt.Rows.Count + 6, 3, 3));
                    SetExcelCell(sheet, dt.Rows.Count + 6, 4, styleHead10, "外來文案件");
                    sheet.AddMergedRegion(new CellRangeAddress(dt.Rows.Count + 6, dt.Rows.Count + 6, 4, 4));
                    SetExcelCell(sheet, dt.Rows.Count + 6, 5, styleHead10, "合計");
                    sheet.AddMergedRegion(new CellRangeAddress(dt.Rows.Count + 6, dt.Rows.Count + 6, 5, 5));

                    SetExcelCell(sheet, dt.Rows.Count + 7, 0, styleHead10, "件數");
                    sheet.AddMergedRegion(new CellRangeAddress(dt.Rows.Count + 7, dt.Rows.Count + 7, 0, 0));

                    SetExcelCell(sheet, dt.Rows.Count + 9, 4, styleHead12, "覆核主管");
                    sheet.AddMergedRegion(new CellRangeAddress(dt.Rows.Count + 9, dt.Rows.Count + 9, 4, 4));
                    SetExcelCell(sheet, dt.Rows.Count + 9, 6, styleHead12, "覆核人員");
                    sheet.AddMergedRegion(new CellRangeAddress(dt.Rows.Count + 9, dt.Rows.Count + 9, 6, 6));

                    for (int i = dt.Rows.Count + 7; i < dt.Rows.Count + 8; i++)//*初始表格賦初值 
                    {
                       for (int j = 0; j < 5; j++)
                       {
                          SetExcelCell(sheet, i, j + 1, styleHead10, "0");
                          sheet.AddMergedRegion(new CellRangeAddress(i, i, j + 1, j + 1));
                       }
                    }
                    #endregion
                    #region Width
                    sheet.SetColumnWidth(0, 100 * 100);
                    sheet.SetColumnWidth(1, 100 * 40);
                    sheet.SetColumnWidth(2, 100 * 100);
                    sheet.SetColumnWidth(3, 100 * 100);
                    sheet.SetColumnWidth(4, 100 * 30);
                    sheet.SetColumnWidth(5, 100 * 30);
                    sheet.SetColumnWidth(6, 100 * 30);
                    sheet.SetColumnWidth(7, 100 * 40);
                    sheet.SetColumnWidth(8, 100 * 30);
                    //sheet.SetColumnWidth(9, 100 * 40);
                    //sheet.SetColumnWidth(10, 100 * 40);
                    #endregion
                    #region body
                    for (int iRow = 0; iRow < dt.Rows.Count; iRow++)
                    {
                       for (int iCol = 1; iCol < dt.Columns.Count - 2; iCol++)
                       {
                          SetExcelCell(sheet, iRow + 5, iCol - 1, styleHead10, dt.Rows[iRow][iCol].ToString());
                       }
                    }
                    //總計
                    int rows = dt.Rows.Count + 7;
                    DataRow[] drkouya = dt.Select(" CaseKind2 = '扣押'");
                    SetExcelCell(sheet, rows, 1, styleHead10, drkouya.Length.ToString());
                    DataRow[] drzhifu = dt.Select(" CaseKind2 = '支付'");
                    SetExcelCell(sheet, rows, 2, styleHead10, drzhifu.Length.ToString());
                    DataRow[] drkouyaandPay = dt.Select(" CaseKind2 = '扣押並支付'");
                    SetExcelCell(sheet, rows, 3, styleHead10, drkouyaandPay.Length.ToString());
                    DataRow[] drWai = dt.Select(" CaseKind = '外來文案件'");
                    SetExcelCell(sheet, rows, 4, styleHead10, drWai.Length.ToString());
                    SetExcelCell(sheet, rows, 5, styleHead10, dt.Rows.Count.ToString());
                    #endregion
                 }
              }
           }
           else
           {
              for (int k = 0; k < Departmentcount; k++)
              {
                 ISheet sheet = null;
                 DataTable dt = new DataTable();

                 sheet = workbook.CreateSheet(listCode[k].CodeDesc);
                 dt = GetList1(model, listCode[k].CodeDesc);//獲取查詢科別的案件
                 SetExcelCell(sheet, 1, 6, styleHead12, listCode[k].CodeDesc);
                 sheet.AddMergedRegion(new CellRangeAddress(1, 1, 6, 7));

                 #region title
                 SetExcelCell(sheet, 0, 0, styleHead12, "用印簿");
                 sheet.AddMergedRegion(new CellRangeAddress(0, 0, 0, 8));

                 //* line1
                 SetExcelCell(sheet, 1, 0, styleHead12, "收件日期：");
                 sheet.AddMergedRegion(new CellRangeAddress(1, 1, 0, 0));
                 SetExcelCell(sheet, 1, 1, styleHead12, model.ReceiveDateStart + '~' + model.ReceiveDateEnd);
                 sheet.AddMergedRegion(new CellRangeAddress(1, 1, 1, 2));
                 SetExcelCell(sheet, 1, 4, styleHead12, "部門別/科別：");
                 sheet.AddMergedRegion(new CellRangeAddress(1, 1, 4, 5));

                 SetExcelCell(sheet, 2, 0, styleHead12, "發文日期：");
                 sheet.AddMergedRegion(new CellRangeAddress(2, 2, 0, 0));
                 SetExcelCell(sheet, 2, 1, styleHead12, model.SendDateStart + '~' + model.SendDateEnd);
                 sheet.AddMergedRegion(new CellRangeAddress(2, 2, 1, 2));
                 SetExcelCell(sheet, 3, 0, styleHead12, "主管放行日：");
                 sheet.AddMergedRegion(new CellRangeAddress(3, 3, 0, 0));
                 SetExcelCell(sheet, 3, 1, styleHead12, model.ApproveDateStart + '~' + model.ApproveDateEnd);
                 sheet.AddMergedRegion(new CellRangeAddress(3, 3, 1, 2));

                 //SetExcelCell(sheet, 4, 0, styleHead12, "電子發文上傳日：");
                 //sheet.AddMergedRegion(new CellRangeAddress(4, 4, 0, 0));
                 //SetExcelCell(sheet, 4, 1, styleHead12, model.SendUpDateStart + '~' + model.SendUpDateEnd);
                 //sheet.AddMergedRegion(new CellRangeAddress(4, 4, 1, 2));

                 //SetExcelCell(sheet, 5, 0, styleHead12, "發文方式：");
                 //sheet.AddMergedRegion(new CellRangeAddress(5, 5, 0, 0));
                 //SetExcelCell(sheet, 5, 1, styleHead12, model.SendKind);
                 //sheet.AddMergedRegion(new CellRangeAddress(5, 5, 1, 2));

                 //* line2
                 SetExcelCell(sheet, 4, 0, styleHead10, "發文字號");
                 sheet.AddMergedRegion(new CellRangeAddress(4, 4, 0, 0));
                 SetExcelCell(sheet, 4, 1, styleHead10, "案件編號");
                 sheet.AddMergedRegion(new CellRangeAddress(4, 4, 1, 1));
                 SetExcelCell(sheet, 4, 2, styleHead10, "受文者");
                 sheet.AddMergedRegion(new CellRangeAddress(4, 4, 2, 2));
                 SetExcelCell(sheet, 4, 3, styleHead10, "副本");
                 sheet.AddMergedRegion(new CellRangeAddress(4, 4, 3, 3));
                 SetExcelCell(sheet, 4, 4, styleHead10, "經辦");
                 sheet.AddMergedRegion(new CellRangeAddress(4, 4, 4, 4));
                 SetExcelCell(sheet, 4, 5, styleHead10, "案件大類");
                 sheet.AddMergedRegion(new CellRangeAddress(4, 4, 5, 5));
                 SetExcelCell(sheet, 4, 6, styleHead10, "案件細類");
                 sheet.AddMergedRegion(new CellRangeAddress(4, 4, 6, 6));
                 SetExcelCell(sheet, 4, 7, styleHead10, "放行主管");
                 sheet.AddMergedRegion(new CellRangeAddress(4, 4, 7, 7));
                 SetExcelCell(sheet, 4, 8, styleHead10, "逾期註記");
                 sheet.AddMergedRegion(new CellRangeAddress(4, 4, 8, 8));
                 //SetExcelCell(sheet, 4, 9, styleHead10, "發文方式");
                 //sheet.AddMergedRegion(new CellRangeAddress(4, 4, 9, 9));
                 //SetExcelCell(sheet, 4, 10, styleHead10, "電子發文上傳日");
                 //sheet.AddMergedRegion(new CellRangeAddress(4, 4, 10, 10));

                 SetExcelCell(sheet, dt.Rows.Count + 6, 0, styleHead10, "案件類別");
                 sheet.AddMergedRegion(new CellRangeAddress(dt.Rows.Count + 6, dt.Rows.Count + 6, 0, 0));
                 SetExcelCell(sheet, dt.Rows.Count + 6, 1, styleHead10, "扣押");
                 sheet.AddMergedRegion(new CellRangeAddress(dt.Rows.Count + 6, dt.Rows.Count + 6, 1, 1));
                 SetExcelCell(sheet, dt.Rows.Count + 6, 2, styleHead10, "支付");
                 sheet.AddMergedRegion(new CellRangeAddress(dt.Rows.Count + 6, dt.Rows.Count + 6, 2, 2));
                 SetExcelCell(sheet, dt.Rows.Count + 6, 3, styleHead10, "扣押並支付");
                 sheet.AddMergedRegion(new CellRangeAddress(dt.Rows.Count + 6, dt.Rows.Count + 6, 3, 3));
                 SetExcelCell(sheet, dt.Rows.Count + 6, 4, styleHead10, "外來文案件");
                 sheet.AddMergedRegion(new CellRangeAddress(dt.Rows.Count + 6, dt.Rows.Count + 6, 4, 4));
                 SetExcelCell(sheet, dt.Rows.Count + 6, 5, styleHead10, "合計");
                 sheet.AddMergedRegion(new CellRangeAddress(dt.Rows.Count + 6, dt.Rows.Count + 6, 5, 5));

                 SetExcelCell(sheet, dt.Rows.Count + 7, 0, styleHead10, "件數");
                 sheet.AddMergedRegion(new CellRangeAddress(dt.Rows.Count + 7, dt.Rows.Count + 7, 0, 0));

                 SetExcelCell(sheet, dt.Rows.Count + 9, 4, styleHead12, "覆核主管");
                 sheet.AddMergedRegion(new CellRangeAddress(dt.Rows.Count + 9, dt.Rows.Count + 9, 4, 4));
                 SetExcelCell(sheet, dt.Rows.Count + 9, 6, styleHead12, "覆核人員");
                 sheet.AddMergedRegion(new CellRangeAddress(dt.Rows.Count + 9, dt.Rows.Count + 9, 6, 6));

                 for (int i = dt.Rows.Count + 7; i < dt.Rows.Count + 8; i++)//*初始表格賦初值 
                 {
                    for (int j = 0; j < 5; j++)
                    {
                       SetExcelCell(sheet, i, j + 1, styleHead10, "0");
                       sheet.AddMergedRegion(new CellRangeAddress(i, i, j + 1, j + 1));
                    }
                 }
                 #endregion
                 #region Width
                 sheet.SetColumnWidth(0, 100 * 100);
                 sheet.SetColumnWidth(1, 100 * 40);
                 sheet.SetColumnWidth(2, 100 * 100);
                 sheet.SetColumnWidth(3, 100 * 100);
                 sheet.SetColumnWidth(4, 100 * 30);
                 sheet.SetColumnWidth(5, 100 * 30);
                 sheet.SetColumnWidth(6, 100 * 30);
                 sheet.SetColumnWidth(7, 100 * 40);
                 sheet.SetColumnWidth(8, 100 * 30);
                 //sheet.SetColumnWidth(9, 100 * 40);
                 //sheet.SetColumnWidth(10, 100 * 40);
                 #endregion
                 #region body
                 for (int iRow = 0; iRow < dt.Rows.Count; iRow++)
                 {
                    for (int iCol = 1; iCol < dt.Columns.Count - 2; iCol++)
                    {
                       SetExcelCell(sheet, iRow + 5, iCol - 1, styleHead10, dt.Rows[iRow][iCol].ToString());
                    }
                 }
                 //總計
                 int rows = dt.Rows.Count + 7;
                 DataRow[] drkouya = dt.Select(" CaseKind2 = '扣押'");
                 SetExcelCell(sheet, rows, 1, styleHead10, drkouya.Length.ToString());
                 DataRow[] drzhifu = dt.Select(" CaseKind2 = '支付'");
                 SetExcelCell(sheet, rows, 2, styleHead10, drzhifu.Length.ToString());
                 DataRow[] drkouyaandPay = dt.Select(" CaseKind2 = '扣押並支付'");
                 SetExcelCell(sheet, rows, 3, styleHead10, drkouyaandPay.Length.ToString());
                 DataRow[] drWai = dt.Select(" CaseKind = '外來文案件'");
                 SetExcelCell(sheet, rows, 4, styleHead10, drWai.Length.ToString());
                 SetExcelCell(sheet, rows, 5, styleHead10, dt.Rows.Count.ToString());
                 #endregion
              }
           }

           MemoryStream ms = new MemoryStream();
           workbook.Write(ms);
           ms.Flush();
           ms.Position = 0;
           workbook = null;
           return ms;
        }
        #endregion
        //20170714 固定 RQ-2015-019666-019 派件至跨單位(原產生邏輯程式寫死，組織異動必出問題，改為參數) 宏祥 add end

        //#region 經辦結案統計表1
        //public MemoryStream CaseMasterListReportExcel_NPOI1(CaseClosedQuery model)
        //{
        //   IWorkbook workbook = new HSSFWorkbook();
        //   ISheet sheet = null;
        //   ISheet sheet2 = null;
        //   ISheet sheet3 = null;
        //   Dictionary<string, string> dicldapList = new Dictionary<string, string>();
        //   Dictionary<string, string> dicldapList2 = new Dictionary<string, string>();
        //   Dictionary<string, string> dicldapList3 = new Dictionary<string, string>();
        //   DataTable dtCase = new DataTable();//導出資料
        //   DataTable dtCase2 = new DataTable();
        //   DataTable dtCase3 = new DataTable();

        //   int rowscountExcelresult = 0;//合計參數
        //   string caseExcel = "";//案件類型
        //   int rowsExcel = 6;//行數
        //   int rowscountExcel = 0;//最後一列合計
        //   int rowstatolExcel = 0;//總合計      
        //   int sort = 1;//記錄每個名字在哪一格

        //   #region def style
        //   ICellStyle styleHead12 = workbook.CreateCellStyle();
        //   IFont font12 = workbook.CreateFont();
        //   font12.FontHeightInPoints = 12;
        //   font12.FontName = "新細明體";
        //   styleHead12.FillPattern = FillPattern.SolidForeground;
        //   styleHead12.FillForegroundColor = HSSFColor.White.Index;
        //   styleHead12.BorderTop = BorderStyle.None;
        //   styleHead12.BorderLeft = BorderStyle.None;
        //   styleHead12.BorderRight = BorderStyle.None;
        //   styleHead12.BorderBottom = BorderStyle.None;
        //   styleHead12.WrapText = true;
        //   styleHead12.Alignment = HorizontalAlignment.Center;
        //   styleHead12.VerticalAlignment = VerticalAlignment.Center;
        //   styleHead12.SetFont(font12);

        //   ICellStyle styleHead10 = workbook.CreateCellStyle();
        //   IFont font10 = workbook.CreateFont();
        //   font10.FontHeightInPoints = 10;
        //   font10.FontName = "新細明體";
        //   styleHead10.FillPattern = FillPattern.SolidForeground;
        //   styleHead10.FillForegroundColor = HSSFColor.White.Index;
        //   styleHead10.BorderTop = BorderStyle.Thin;
        //   styleHead10.BorderLeft = BorderStyle.Thin;
        //   styleHead10.BorderRight = BorderStyle.Thin;
        //   styleHead10.BorderBottom = BorderStyle.Thin;
        //   styleHead10.WrapText = true;
        //   styleHead10.Alignment = HorizontalAlignment.Left;
        //   styleHead10.VerticalAlignment = VerticalAlignment.Center;
        //   styleHead10.SetFont(font10);
        //   #endregion

        //   #region 單獨科別的數據源(科別及案件資料)
        //   //獲取人員
        //   if (model.Depart == "1" || model.Depart == "0")//* 集作一科
        //   {
        //      sheet = workbook.CreateSheet("集作一科");
        //      dtCase = GetCaseMasterList1(model, "集作一科");
        //      //判斷人員
        //      foreach (DataRow dr in dtCase.Rows)
        //      {
        //         if (!dicldapList.Keys.Contains(dr["AgentUser"].ToString()))
        //         {
        //            dicldapList.Add(dr["AgentUser"].ToString(), dr["EmpName"].ToString() + "|" + sort);
        //            sort++;
        //         }
        //      }
        //      if (dicldapList.Count > 2)
        //      {
        //         SetExcelCell(sheet, 1, dicldapList.Count + 1, styleHead12, "集作一科");
        //         sheet.AddMergedRegion(new CellRangeAddress(1, 1, dicldapList.Count + 1, dicldapList.Count + 1));
        //      }
        //      else
        //      {
        //         SetExcelCell(sheet, 1, 4, styleHead12, "集作一科");
        //         sheet.AddMergedRegion(new CellRangeAddress(1, 1, 4, 4));
        //         sheet.SetColumnWidth(4, 100 * 30);
        //      }
        //   }
        //   if (model.Depart == "2")//* 集作二科
        //   {
        //      sheet = workbook.CreateSheet("集作二科");
        //      dtCase = GetCaseMasterList1(model, "集作二科");
        //      //判斷人員
        //      foreach (DataRow dr in dtCase.Rows)
        //      {
        //         if (!dicldapList.Keys.Contains(dr["AgentUser"].ToString()))
        //         {
        //            dicldapList.Add(dr["AgentUser"].ToString(), dr["EmpName"].ToString() + "|" + sort);
        //            sort++;
        //         }
        //      }
        //      if (dicldapList.Count > 2)
        //      {
        //         SetExcelCell(sheet, 1, dicldapList.Count + 1, styleHead12, "集作二科");
        //         sheet.AddMergedRegion(new CellRangeAddress(1, 1, dicldapList.Count + 1, dicldapList.Count + 1));
        //      }
        //      else
        //      {
        //         SetExcelCell(sheet, 1, 4, styleHead12, "集作二科");
        //         sheet.AddMergedRegion(new CellRangeAddress(1, 1, 4, 4));
        //         sheet.SetColumnWidth(4, 100 * 30);
        //      }
        //   }
        //   if (model.Depart == "3")//* 集作三科
        //   {
        //      sheet = workbook.CreateSheet("集作三科");
        //      dtCase = GetCaseMasterList1(model, "集作三科");
        //      //判斷人員
        //      foreach (DataRow dr in dtCase.Rows)
        //      {
        //         if (!dicldapList.Keys.Contains(dr["AgentUser"].ToString()))
        //         {
        //            dicldapList.Add(dr["AgentUser"].ToString(), dr["EmpName"].ToString() + "|" + sort);
        //            sort++;
        //         }
        //      }
        //      if (dicldapList.Count > 2)
        //      {
        //         SetExcelCell(sheet, 1, dicldapList.Count + 1, styleHead12, "集作三科");
        //         sheet.AddMergedRegion(new CellRangeAddress(1, 1, dicldapList.Count + 1, dicldapList.Count + 1));
        //      }
        //      else
        //      {
        //         SetExcelCell(sheet, 1, 4, styleHead12, "集作三科");
        //         sheet.AddMergedRegion(new CellRangeAddress(1, 1, 4, 4));
        //         sheet.SetColumnWidth(4, 100 * 30);
        //      }
        //   }
        //   if (model.Depart == "0")//* 全部
        //   {
        //      sheet2 = workbook.CreateSheet("集作二科");
        //      sheet3 = workbook.CreateSheet("集作三科");
        //      dtCase2 = GetCaseMasterList1(model, "集作二科");//獲取查詢集作二科的案件
        //      dtCase3 = GetCaseMasterList1(model, "集作三科");//獲取查詢集作三科的案件
        //      sort = 1;
        //      //判斷集作二科人員
        //      foreach (DataRow dr in dtCase2.Rows)
        //      {
        //         if (!dicldapList2.Keys.Contains(dr["AgentUser"].ToString()))
        //         {
        //            dicldapList2.Add(dr["AgentUser"].ToString(), dr["EmpName"].ToString() + "|" + sort);
        //            sort++;
        //         }
        //      }

        //      sort = 1;
        //      //判斷集作三科人員
        //      foreach (DataRow dr in dtCase3.Rows)
        //      {
        //         if (!dicldapList3.Keys.Contains(dr["AgentUser"].ToString()))
        //         {
        //            dicldapList3.Add(dr["AgentUser"].ToString(), dr["EmpName"].ToString() + "|" + sort);
        //            sort++;
        //         }
        //      }
        //   }
        //   #endregion

        //   string caseKind = "";//*去重複
        //   int rows = 6;//title中定義行數
        //   #region title
        //   //*大標題 line0
        //   SetExcelCell(sheet, 0, 0, styleHead12, "經辦結案統計表");
        //   sheet.AddMergedRegion(new CellRangeAddress(0, 0, 0, dicldapList.Count + 1));

        //   //*查詢條件 line1
        //   SetExcelCell(sheet, 1, 0, styleHead12, "收件日期：");
        //   sheet.AddMergedRegion(new CellRangeAddress(1, 1, 0, 0));
        //   SetExcelCell(sheet, 1, 1, styleHead12, model.ReceiveDateStart + '~' + model.ReceiveDateEnd);
        //   sheet.AddMergedRegion(new CellRangeAddress(1, 1, 1, 2));

        //   if (dicldapList.Count > 2)
        //   {
        //      SetExcelCell(sheet, 1, dicldapList.Count, styleHead12, "部門別/科別");
        //      sheet.AddMergedRegion(new CellRangeAddress(1, 1, dicldapList.Count, dicldapList.Count));
        //   }
        //   else
        //   {
        //      SetExcelCell(sheet, 1, 3, styleHead12, "部門別/科別");
        //      sheet.AddMergedRegion(new CellRangeAddress(1, 1, 3, 3));
        //      sheet.SetColumnWidth(3, 100 * 30);
        //   }

        //   //*結果集表頭 line2
        //   SetExcelCell(sheet, 2, 0, styleHead12, "發文日期：");
        //   sheet.AddMergedRegion(new CellRangeAddress(2, 2, 0, 0));
        //   SetExcelCell(sheet, 2, 1, styleHead12, model.SendDateStart + '~' + model.SendDateEnd);
        //   sheet.AddMergedRegion(new CellRangeAddress(2, 2, 1, 2));
        //   //*結果集表頭 line3
        //   SetExcelCell(sheet, 3, 0, styleHead12, "主管放行日：");
        //   sheet.AddMergedRegion(new CellRangeAddress(3, 3, 0, 0));
        //   SetExcelCell(sheet, 3, 1, styleHead12, model.ApproveDateStart + '~' + model.ApproveDateEnd);
        //   sheet.AddMergedRegion(new CellRangeAddress(3, 3, 1, 2));

        //   SetExcelCell(sheet, 4, 0, styleHead12, "電子發文上傳日：");
        //   sheet.AddMergedRegion(new CellRangeAddress(4, 4, 0, 0));
        //   SetExcelCell(sheet, 4, 1, styleHead12, model.SendUpDateStart + '~' + model.SendUpDateEnd);
        //   sheet.AddMergedRegion(new CellRangeAddress(4, 4, 1, 2));
        //   SetExcelCell(sheet, 5, 0, styleHead12, "發文方式：");
        //   sheet.AddMergedRegion(new CellRangeAddress(5, 5, 0, 0));
        //   SetExcelCell(sheet, 5, 1, styleHead12, model.SendKind);
        //   sheet.AddMergedRegion(new CellRangeAddress(5, 5, 1, 2));

        //   //*結果集表頭 line4
        //   SetExcelCell(sheet, 6, 0, styleHead10, "處理人員");
        //   sheet.AddMergedRegion(new CellRangeAddress(6, 6, 0, 0));
        //   sheet.SetColumnWidth(0, 100 * 50);
        //   //依次排列人員名稱
        //   int a = 1;
        //   foreach (var item in dicldapList)
        //   {
        //      SetExcelCell(sheet, 6, a, styleHead10, item.Value.Split('|')[0]);
        //      sheet.AddMergedRegion(new CellRangeAddress(6, 6, a, a));
        //      a++;
        //   }
        //   SetExcelCell(sheet, 6, dicldapList.Count + 1, styleHead10, "合計");
        //   sheet.AddMergedRegion(new CellRangeAddress(6, 6, dicldapList.Count + 1, dicldapList.Count + 1));

        //   //*扣押案件類型 line5-lineN 
        //   for (int i = 0; i < dtCase.Rows.Count; i++)
        //   {
        //      if (caseKind != dtCase.Rows[i]["New_CaseKind"].ToString())
        //      {
        //         rows = rows + 1;
        //         SetExcelCell(sheet, rows, 0, styleHead10, dtCase.Rows[i]["New_CaseKind"].ToString());
        //         sheet.AddMergedRegion(new CellRangeAddress(rows, rows, 0, 0));
        //         SetExcelCell(sheet, rows, dicldapList.Count + 1, styleHead10, "0");//最後一列合計賦初值
        //         sheet.AddMergedRegion(new CellRangeAddress(rows, rows, dicldapList.Count + 1, dicldapList.Count + 1));
        //         caseKind = dtCase.Rows[i]["New_CaseKind"].ToString();
        //         for (int j = 0; j < dicldapList.Count; j++)//*初始表格賦初值 
        //         {
        //            SetExcelCell(sheet, rows, j + 1, styleHead10, "0");
        //            sheet.AddMergedRegion(new CellRangeAddress(rows, rows, j + 1, j + 1));
        //         }
        //      }
        //   }

        //   //*合計 lineLast (案件下面的合計以及整行賦初值)
        //   SetExcelCell(sheet, rows + 1, 0, styleHead10, "合計");
        //   sheet.AddMergedRegion(new CellRangeAddress(rows + 1, rows + 1, 0, 0));
        //   if (dicldapList.Count > 0)
        //   {
        //      for (int j = 0; j < dicldapList.Count; j++)//*初始表格賦初值 
        //      {
        //         SetExcelCell(sheet, rows + 1, j + 1, styleHead10, "0");
        //         sheet.AddMergedRegion(new CellRangeAddress(rows + 1, rows + 1, j + 1, j + 1));
        //      }
        //   }
        //   else
        //   {
        //      SetExcelCell(sheet, rows + 1, 1, styleHead10, "0");
        //      sheet.AddMergedRegion(new CellRangeAddress(rows + 1, rows + 1, 1, 1));
        //   }

        //   #endregion
        //   #region  body
        //   for (int iRow = 0; iRow < dtCase.Rows.Count; iRow++)//根據案件類型進行循環
        //   {
        //      foreach (var item in dicldapList)
        //      {
        //         int irows = Convert.ToInt32(item.Value.Split('|')[1]);
        //         if (caseExcel == dtCase.Rows[iRow]["New_CaseKind"].ToString())//重複同一案件類型的數據
        //         {
        //            if (item.Key == dtCase.Rows[iRow]["AgentUser"].ToString())
        //            {
        //               SetExcelCell(sheet, rowsExcel, irows, styleHead10, dtCase.Rows[iRow]["case_num"].ToString());
        //               rowscountExcelresult = Convert.ToInt32(dtCase.Rows[iRow]["case_num"].ToString());//每格資料
        //               SetExcelCell(sheet, rows + 1, irows, styleHead10, dtCase.Rows[iRow]["User_Count"].ToString());//最後一行合計
        //               rowscountExcel += rowscountExcelresult;
        //               rowstatolExcel += rowscountExcelresult;
        //            }
        //            SetExcelCell(sheet, rowsExcel, dicldapList.Count + 1, styleHead10, rowscountExcel.ToString());//最後一列合計
        //         }
        //         else//不重複的案件類型
        //         {
        //            rowscountExcel = 0;
        //            rowsExcel = rowsExcel + 1;
        //            if (item.Key == dtCase.Rows[iRow]["AgentUser"].ToString())
        //            {
        //               SetExcelCell(sheet, rowsExcel, irows, styleHead10, dtCase.Rows[iRow]["case_num"].ToString());
        //               rowscountExcelresult = Convert.ToInt32(dtCase.Rows[iRow]["case_num"].ToString());//第一條不重複的數據儲存下值
        //               SetExcelCell(sheet, rows + 1, irows, styleHead10, dtCase.Rows[iRow]["User_Count"].ToString());//最後一行合計
        //               rowscountExcel += rowscountExcelresult;
        //               rowstatolExcel += rowscountExcelresult;
        //            }
        //            caseExcel = dtCase.Rows[iRow]["New_CaseKind"].ToString();
        //            SetExcelCell(sheet, rowsExcel, dicldapList.Count + 1, styleHead10, rowscountExcel.ToString());//最後一列合計
        //         }
        //      }
        //   }
        //   SetExcelCell(sheet, rows + 1, dicldapList.Count + 1, styleHead10, rowstatolExcel.ToString());//總合計
        //   #endregion

        //   if (model.Depart == "0")//* 全部
        //   {
        //      #region title2
        //      //*大標題 line0
        //      SetExcelCell(sheet2, 0, 0, styleHead12, "經辦結案統計表");
        //      sheet2.AddMergedRegion(new CellRangeAddress(0, 0, 0, dicldapList2.Count + 1));

        //      //*查詢條件 line1
        //      SetExcelCell(sheet2, 1, 0, styleHead12, "收件日期：");
        //      sheet2.AddMergedRegion(new CellRangeAddress(1, 1, 0, 0));
        //      SetExcelCell(sheet2, 1, 1, styleHead12, model.ReceiveDateStart + '~' + model.ReceiveDateEnd);
        //      sheet2.AddMergedRegion(new CellRangeAddress(1, 1, 1, 2));

        //      if (dicldapList2.Count > 2)
        //      {
        //         SetExcelCell(sheet2, 1, dicldapList2.Count, styleHead12, "部門別/科別");
        //         sheet2.AddMergedRegion(new CellRangeAddress(1, 1, dicldapList2.Count, dicldapList2.Count));
        //         SetExcelCell(sheet2, 1, dicldapList2.Count + 1, styleHead12, "集作二科");
        //         sheet.AddMergedRegion(new CellRangeAddress(1, 1, dicldapList2.Count + 1, dicldapList2.Count + 1));
        //      }
        //      else
        //      {
        //         SetExcelCell(sheet2, 1, 3, styleHead12, "部門別/科別");
        //         sheet.AddMergedRegion(new CellRangeAddress(1, 1, 3, 3));
        //         sheet2.SetColumnWidth(3, 100 * 50);
        //         SetExcelCell(sheet2, 1, 4, styleHead12, "集作二科");
        //         sheet.AddMergedRegion(new CellRangeAddress(1, 1, 4, 4));
        //         sheet2.SetColumnWidth(4, 100 * 30);
        //      }

        //      //*查詢條件 line2
        //      SetExcelCell(sheet2, 2, 0, styleHead12, "發文日期：");
        //      sheet2.AddMergedRegion(new CellRangeAddress(2, 2, 0, 0));
        //      SetExcelCell(sheet2, 2, 1, styleHead12, model.SendDateStart + '~' + model.SendDateEnd);
        //      sheet2.AddMergedRegion(new CellRangeAddress(2, 2, 1, 2));

        //      //*查詢條件 line3
        //      SetExcelCell(sheet2, 3, 0, styleHead12, "主管放行日：");
        //      sheet2.AddMergedRegion(new CellRangeAddress(3, 3, 0, 0));
        //      SetExcelCell(sheet2, 3, 1, styleHead12, model.ApproveDateStart + '~' + model.ApproveDateEnd);
        //      sheet2.AddMergedRegion(new CellRangeAddress(3, 3, 1, 2));

        //      SetExcelCell(sheet2, 4, 0, styleHead12, "電子發文上傳日：");
        //      sheet2.AddMergedRegion(new CellRangeAddress(4, 4, 0, 0));
        //      SetExcelCell(sheet2, 4, 1, styleHead12, model.SendUpDateStart + '~' + model.SendUpDateEnd);
        //      sheet2.AddMergedRegion(new CellRangeAddress(4, 4, 1, 2));
        //      SetExcelCell(sheet2, 5, 0, styleHead12, "發文方式：");
        //      sheet2.AddMergedRegion(new CellRangeAddress(5, 5, 0, 0));
        //      SetExcelCell(sheet2, 5, 1, styleHead12, model.SendKind);
        //      sheet2.AddMergedRegion(new CellRangeAddress(5, 5, 1, 2));

        //      //*結果集表頭 line4
        //      SetExcelCell(sheet2, 6, 0, styleHead10, "處理人員");
        //      sheet2.AddMergedRegion(new CellRangeAddress(6, 6, 0, 0));
        //      sheet2.SetColumnWidth(0, 100 * 50);
        //      int icols = 1;
        //      foreach (var item in dicldapList2)
        //      {
        //         SetExcelCell(sheet2, 6, icols, styleHead10, item.Value.Split('|')[0]);
        //         sheet2.AddMergedRegion(new CellRangeAddress(6, 6, icols, icols));
        //         icols++;
        //      }
        //      SetExcelCell(sheet2, 6, dicldapList2.Count + 1, styleHead10, "合計");
        //      sheet2.AddMergedRegion(new CellRangeAddress(6, 6, dicldapList2.Count + 1, dicldapList2.Count + 1));

        //      //*扣押案件類型 line5-lineN 
        //      caseKind = "";//*去重複
        //      rows = 6;//定義行數
        //      for (int i = 0; i < dtCase2.Rows.Count; i++)
        //      {
        //         if (caseKind != dtCase2.Rows[i]["New_CaseKind"].ToString())
        //         {
        //            rows = rows + 1;
        //            SetExcelCell(sheet2, rows, 0, styleHead10, dtCase2.Rows[i]["New_CaseKind"].ToString());
        //            sheet2.AddMergedRegion(new CellRangeAddress(rows, rows, 0, 0));

        //            SetExcelCell(sheet2, rows, dicldapList2.Count + 1, styleHead10, "0");//給最後一列合計賦初值
        //            sheet2.AddMergedRegion(new CellRangeAddress(rows, rows, dicldapList2.Count + 1, dicldapList2.Count + 1));

        //            caseKind = dtCase2.Rows[i]["New_CaseKind"].ToString();
        //            for (int j = 0; j < dicldapList2.Count; j++)//*初始表格賦初值 
        //            {
        //               SetExcelCell(sheet2, rows, j + 1, styleHead10, "0");
        //               sheet2.AddMergedRegion(new CellRangeAddress(rows, rows, j + 1, j + 1));
        //            }
        //         }
        //      }

        //      //*合計 lineLast (最後一行合計)
        //      SetExcelCell(sheet2, rows + 1, 0, styleHead10, "合計");
        //      sheet2.AddMergedRegion(new CellRangeAddress(dicldapList2.Count + 3, dicldapList2.Count + 3, 0, 0));
        //      if (dicldapList2.Count > 0)
        //      {
        //         for (int j = 0; j < dicldapList2.Count; j++)//*初始表格賦初值 
        //         {
        //            SetExcelCell(sheet2, rows + 1, j + 1, styleHead10, "0");
        //            sheet2.AddMergedRegion(new CellRangeAddress(rows + 1, rows + 1, j + 1, j + 1));
        //         }
        //      }
        //      else
        //      {
        //         SetExcelCell(sheet2, rows + 1, 1, styleHead10, "0");
        //         sheet2.AddMergedRegion(new CellRangeAddress(rows + 1, rows + 1, 1, 1));
        //      }

        //      #endregion
        //      #region body2
        //      caseExcel = "";//案件類型
        //      //20170630 緊急 RQ-2015-019666-018 派件至跨單位 宏祥 update start
        //      //rowsExcel = 4;//行數
        //      rowsExcel = 6;//行數
        //      //20170630 緊急 RQ-2015-019666-018 派件至跨單位 宏祥 update end
        //      rowscountExcel = 0;//最後一列合計
        //      rowstatolExcel = 0;//總合計      
        //      for (int iRow = 0; iRow < dtCase2.Rows.Count; iRow++)//根據案件類型進行循環
        //      {
        //         foreach (var item in dicldapList2)
        //         {
        //            int icol = Convert.ToInt32(item.Value.Split('|')[1]);
        //            if (dtCase2.Rows[iRow]["New_CaseKind"].ToString() == caseExcel)//重複同一案件類型的數據
        //            {
        //               if (item.Key == dtCase2.Rows[iRow]["AgentUser"].ToString())
        //               {
        //                  SetExcelCell(sheet2, rowsExcel, icol, styleHead10, dtCase2.Rows[iRow]["case_num"].ToString());
        //                  SetExcelCell(sheet2, rows + 1, icol, styleHead10, dtCase2.Rows[iRow]["User_Count"].ToString());//最後一行合計
        //                  rowscountExcelresult = Convert.ToInt32(dtCase2.Rows[iRow]["case_num"].ToString());//每格資料
        //                  rowscountExcel += rowscountExcelresult;
        //                  rowstatolExcel += rowscountExcelresult;
        //               }
        //               SetExcelCell(sheet2, rowsExcel, dicldapList2.Count + 1, styleHead10, rowscountExcel.ToString());//最後一列合計
        //            }
        //            else//不重複的案件類型
        //            {
        //               rowscountExcel = 0;
        //               rowsExcel = rowsExcel + 1;
        //               if (item.Key == dtCase2.Rows[iRow]["AgentUser"].ToString())
        //               {
        //                  SetExcelCell(sheet2, rowsExcel, icol, styleHead10, dtCase2.Rows[iRow]["case_num"].ToString());
        //                  SetExcelCell(sheet2, rows + 1, icol, styleHead10, dtCase2.Rows[iRow]["User_Count"].ToString());//最後一行合計
        //                  rowscountExcelresult = Convert.ToInt32(dtCase2.Rows[iRow]["case_num"].ToString());//第一條不重複的數據儲存下值
        //                  rowscountExcel += rowscountExcelresult;
        //                  rowstatolExcel += rowscountExcelresult;
        //               }
        //               caseExcel = dtCase2.Rows[iRow]["New_CaseKind"].ToString();
        //               SetExcelCell(sheet2, rowsExcel, dicldapList2.Count + 1, styleHead10, rowscountExcel.ToString());//最後一列合計      
        //            }
        //         }
        //         SetExcelCell(sheet2, rows + 1, dicldapList2.Count + 1, styleHead10, rowstatolExcel.ToString());//總合計
        //      }
        //      #endregion

        //      #region title3
        //      //*大標題 line0
        //      SetExcelCell(sheet3, 0, 0, styleHead12, "經辦結案統計表");
        //      sheet3.AddMergedRegion(new CellRangeAddress(0, 0, 0, dicldapList3.Count + 1));

        //      //*查詢條件 line1
        //      SetExcelCell(sheet3, 1, 0, styleHead12, "收件日期：");
        //      sheet3.AddMergedRegion(new CellRangeAddress(1, 1, 0, 0));
        //      SetExcelCell(sheet3, 1, 1, styleHead12, model.ReceiveDateStart + '~' + model.ReceiveDateEnd);
        //      sheet3.AddMergedRegion(new CellRangeAddress(1, 1, 1, 2));
        //      if (dicldapList3.Count > 2)
        //      {
        //         SetExcelCell(sheet3, 1, dicldapList3.Count, styleHead12, "部門別/科別");
        //         sheet3.AddMergedRegion(new CellRangeAddress(1, 1, dicldapList3.Count, dicldapList3.Count));
        //         SetExcelCell(sheet3, 1, dicldapList3.Count + 1, styleHead12, "集作三科");
        //         sheet.AddMergedRegion(new CellRangeAddress(1, 1, dicldapList3.Count + 1, dicldapList3.Count + 1));
        //      }
        //      else
        //      {
        //         SetExcelCell(sheet3, 1, 3, styleHead12, "部門別/科別");
        //         sheet3.AddMergedRegion(new CellRangeAddress(1, 1, 3, 3));
        //         sheet3.SetColumnWidth(3, 100 * 50);
        //         SetExcelCell(sheet3, 1, 4, styleHead12, "集作三科");
        //         sheet.AddMergedRegion(new CellRangeAddress(1, 1, 4, 4));
        //         sheet3.SetColumnWidth(4, 100 * 30);
        //      }


        //      //*查詢條件 line2
        //      SetExcelCell(sheet3, 2, 0, styleHead12, "發文日期：");
        //      sheet3.AddMergedRegion(new CellRangeAddress(2, 2, 0, 0));
        //      SetExcelCell(sheet3, 2, 1, styleHead12, model.SendDateStart + '~' + model.SendDateEnd);
        //      sheet3.AddMergedRegion(new CellRangeAddress(2, 2, 1, 2));

        //      //*查詢條件 line3
        //      SetExcelCell(sheet3, 3, 0, styleHead12, "主管放行日：");
        //      sheet3.AddMergedRegion(new CellRangeAddress(3, 3, 0, 0));
        //      SetExcelCell(sheet3, 3, 1, styleHead12, model.ApproveDateStart + '~' + model.ApproveDateEnd);
        //      sheet3.AddMergedRegion(new CellRangeAddress(3, 3, 1, 2));

        //      SetExcelCell(sheet3, 4, 0, styleHead12, "電子發文上傳日：");
        //      sheet3.AddMergedRegion(new CellRangeAddress(4, 4, 0, 0));
        //      SetExcelCell(sheet3, 4, 1, styleHead12, model.SendUpDateStart + '~' + model.SendUpDateEnd);
        //      sheet3.AddMergedRegion(new CellRangeAddress(4, 4, 1, 2));
        //      SetExcelCell(sheet3, 5, 0, styleHead12, "發文方式：");
        //      sheet3.AddMergedRegion(new CellRangeAddress(5, 5, 0, 0));
        //      SetExcelCell(sheet3, 5, 1, styleHead12, model.SendKind);
        //      sheet3.AddMergedRegion(new CellRangeAddress(5, 5, 1, 2));

        //      //*結果集表頭 line4
        //      SetExcelCell(sheet3, 6, 0, styleHead10, "處理人員");
        //      sheet3.AddMergedRegion(new CellRangeAddress(6, 6, 0, 0));
        //      sheet3.SetColumnWidth(0, 100 * 50);

        //      int irows3 = 1;
        //      foreach (var item in dicldapList3)
        //      {
        //         SetExcelCell(sheet3, 6, irows3, styleHead10, item.Value.Split('|')[0]);
        //         sheet3.AddMergedRegion(new CellRangeAddress(6, 6, irows3, irows3));
        //         irows3++;
        //      }

        //      SetExcelCell(sheet3, 6, dicldapList3.Count + 1, styleHead10, "合計");
        //      sheet3.AddMergedRegion(new CellRangeAddress(6, 6, dicldapList3.Count + 1, dicldapList3.Count + 1));

        //      //*扣押案件類型 line5-lineN 
        //      caseKind = "";//*去重複
        //      rows = 6;//定義行數
        //      for (int i = 0; i < dtCase3.Rows.Count; i++)
        //      {
        //         if (caseKind != dtCase3.Rows[i]["New_CaseKind"].ToString())
        //         {
        //            rows = rows + 1;
        //            SetExcelCell(sheet3, rows, 0, styleHead10, dtCase3.Rows[i]["New_CaseKind"].ToString());
        //            sheet3.AddMergedRegion(new CellRangeAddress(rows, rows, 0, 0));
        //            SetExcelCell(sheet3, rows, dicldapList3.Count + 1, styleHead10, "0");
        //            sheet3.AddMergedRegion(new CellRangeAddress(rows, rows, dicldapList3.Count + 1, dicldapList3.Count + 1));
        //            caseKind = dtCase3.Rows[i]["New_CaseKind"].ToString();
        //            for (int j = 0; j < dicldapList3.Count; j++)//*初始表格賦初值 
        //            {
        //               SetExcelCell(sheet3, rows, j + 1, styleHead10, "0");
        //               sheet3.AddMergedRegion(new CellRangeAddress(rows, rows, j + 1, j + 1));
        //            }
        //         }
        //      }

        //      //*合計 lineLast
        //      SetExcelCell(sheet3, rows + 1, 0, styleHead10, "合計");
        //      sheet3.AddMergedRegion(new CellRangeAddress(dicldapList3.Count + 3, dicldapList3.Count + 3, 0, 0));
        //      if (dicldapList3.Count > 0)
        //      {
        //         for (int j = 0; j < dicldapList3.Count; j++)//*初始表格賦初值 
        //         {
        //            SetExcelCell(sheet3, rows + 1, j + 1, styleHead10, "0");
        //            sheet3.AddMergedRegion(new CellRangeAddress(rows + 1, rows + 1, j + 1, j + 1));
        //         }
        //      }
        //      else
        //      {
        //         SetExcelCell(sheet3, rows + 1, 1, styleHead10, "0");
        //         sheet3.AddMergedRegion(new CellRangeAddress(rows + 1, rows + 1, 1, 1));
        //      }

        //      #endregion
        //      #region body3
        //      caseExcel = "";//案件類型
        //      //20170630 緊急 RQ-2015-019666-018 派件至跨單位 宏祥 update start
        //      //rowsExcel = 4;//行數
        //      rowsExcel = 6;//行數
        //      //20170630 緊急 RQ-2015-019666-018 派件至跨單位 宏祥 update end
        //      rowscountExcel = 0;//最後一列合計
        //      rowstatolExcel = 0;//總合計  
        //      for (int iRow = 0; iRow < dtCase3.Rows.Count; iRow++)//根據案件類型進行循環
        //      {
        //         foreach (var item in dicldapList3)
        //         {
        //            int icols3 = Convert.ToInt32(item.Value.Split('|')[1]);
        //            if (dtCase3.Rows[iRow]["New_CaseKind"].ToString() == caseExcel)//重複同一案件類型的數據
        //            {
        //               if (dtCase3.Rows[iRow]["AgentUser"].ToString() == item.Key)
        //               {
        //                  SetExcelCell(sheet3, rowsExcel, icols3, styleHead10, dtCase3.Rows[iRow]["case_num"].ToString());
        //                  SetExcelCell(sheet3, rows + 1, icols3, styleHead10, dtCase3.Rows[iRow]["User_Count"].ToString());//最後一行合計
        //                  rowscountExcelresult = Convert.ToInt32(dtCase3.Rows[iRow]["case_num"].ToString());//每格資料
        //                  rowscountExcel += rowscountExcelresult;
        //                  rowstatolExcel += rowscountExcelresult;
        //               }
        //               SetExcelCell(sheet3, rowsExcel, dicldapList3.Count + 1, styleHead10, rowscountExcel.ToString());//最後一列合計
        //            }
        //            else//不重複的案件類型
        //            {
        //               rowscountExcel = 0;
        //               rowsExcel = rowsExcel + 1;
        //               if (dtCase3.Rows[iRow]["AgentUser"].ToString() == item.Key)
        //               {
        //                  SetExcelCell(sheet3, rowsExcel, icols3, styleHead10, dtCase3.Rows[iRow]["case_num"].ToString());
        //                  SetExcelCell(sheet3, rows + 1, icols3, styleHead10, dtCase3.Rows[iRow]["User_Count"].ToString());//最後一行合計
        //                  rowscountExcelresult = Convert.ToInt32(dtCase3.Rows[iRow]["case_num"].ToString());//第一條不重複的數據儲存下值
        //                  rowscountExcel += rowscountExcelresult;
        //                  rowstatolExcel += rowscountExcelresult;
        //               }
        //               caseExcel = dtCase3.Rows[iRow]["New_CaseKind"].ToString();
        //               SetExcelCell(sheet3, rowsExcel, dicldapList3.Count + 1, styleHead10, rowscountExcel.ToString());//最後一列合計      
        //            }
        //         }
        //         SetExcelCell(sheet3, rows + 1, dicldapList3.Count + 1, styleHead10, rowstatolExcel.ToString());//總合計
        //      }
        //      #endregion
        //   }
        //   MemoryStream ms = new MemoryStream();
        //   workbook.Write(ms);
        //   ms.Flush();
        //   ms.Position = 0;
        //   workbook = null;
        //   return ms;
        //}
        //#endregion

        //20170714 固定 RQ-2015-019666-019 派件至跨單位(原產生邏輯程式寫死，組織異動必出問題，改為參數) 宏祥 add start
        #region 經辦結案統計表1
        public MemoryStream CaseMasterListReportExcel_NPOI1(CaseClosedQuery model, IList<PARMCode> listCode)
        {
           IWorkbook workbook = new HSSFWorkbook();
           int Departmentcount = listCode.Count();

           int rowscountExcelresult = 0;//合計參數
           string caseExcel = "";//案件類型
           int rowsExcel = 6;//行數
           int rowscountExcel = 0;//最後一列合計
           int rowstatolExcel = 0;//總合計      
           int sort = 1;//記錄每個名字在哪一格

           #region def style
           ICellStyle styleHead12 = workbook.CreateCellStyle();
           IFont font12 = workbook.CreateFont();
           font12.FontHeightInPoints = 12;
           font12.FontName = "新細明體";
           styleHead12.FillPattern = FillPattern.SolidForeground;
           styleHead12.FillForegroundColor = HSSFColor.White.Index;
           styleHead12.BorderTop = BorderStyle.None;
           styleHead12.BorderLeft = BorderStyle.None;
           styleHead12.BorderRight = BorderStyle.None;
           styleHead12.BorderBottom = BorderStyle.None;
           styleHead12.WrapText = true;
           styleHead12.Alignment = HorizontalAlignment.Center;
           styleHead12.VerticalAlignment = VerticalAlignment.Center;
           styleHead12.SetFont(font12);

           ICellStyle styleHead10 = workbook.CreateCellStyle();
           IFont font10 = workbook.CreateFont();
           font10.FontHeightInPoints = 10;
           font10.FontName = "新細明體";
           styleHead10.FillPattern = FillPattern.SolidForeground;
           styleHead10.FillForegroundColor = HSSFColor.White.Index;
           styleHead10.BorderTop = BorderStyle.Thin;
           styleHead10.BorderLeft = BorderStyle.Thin;
           styleHead10.BorderRight = BorderStyle.Thin;
           styleHead10.BorderBottom = BorderStyle.Thin;
           styleHead10.WrapText = true;
           styleHead10.Alignment = HorizontalAlignment.Left;
           styleHead10.VerticalAlignment = VerticalAlignment.Center;
           styleHead10.SetFont(font10);
           #endregion

           //判斷科別搜尋條件
           if (model.Depart != "0")
           {
              for (int k = 0; k < Departmentcount; k++)
              {
                 if (model.Depart == (k + 1).ToString())
                 {
                    ISheet sheet = null;
                    Dictionary<string, string> dicldapList = new Dictionary<string, string>();
                    DataTable dtCase = new DataTable();

                    sheet = workbook.CreateSheet(listCode[k].CodeDesc);
                    dtCase = GetCaseMasterList1(model, listCode[k].CodeDesc);

                    //判斷人員
                    foreach (DataRow dr in dtCase.Rows)
                    {
                       if (!dicldapList.Keys.Contains(dr["AgentUser"].ToString()))
                       {
                          dicldapList.Add(dr["AgentUser"].ToString(), dr["EmpName"].ToString() + "|" + sort);
                          sort++;
                       }
                    }
                    if (dicldapList.Count > 2)
                    {
                       SetExcelCell(sheet, 1, dicldapList.Count + 1, styleHead12, listCode[k].CodeDesc);
                       sheet.AddMergedRegion(new CellRangeAddress(1, 1, dicldapList.Count + 1, dicldapList.Count + 1));
                       sheet.SetColumnWidth(dicldapList.Count + 1, 100 * 40);
                    }
                    else
                    {
                       SetExcelCell(sheet, 1, 4, styleHead12, listCode[k].CodeDesc);
                       sheet.AddMergedRegion(new CellRangeAddress(1, 1, 4, 4));
                       sheet.SetColumnWidth(4, 100 * 40);
                    }

                    string caseKind = "";//*去重複
                    int rows = 6;//title中定義行數

                    #region title
                    //*大標題 line0
                    SetExcelCell(sheet, 0, 0, styleHead12, "經辦結案統計表");
                    sheet.AddMergedRegion(new CellRangeAddress(0, 0, 0, dicldapList.Count + 1));

                    //*查詢條件 line1
                    SetExcelCell(sheet, 1, 0, styleHead12, "收件日期：");
                    sheet.AddMergedRegion(new CellRangeAddress(1, 1, 0, 0));
                    SetExcelCell(sheet, 1, 1, styleHead12, model.ReceiveDateStart + '~' + model.ReceiveDateEnd);
                    sheet.AddMergedRegion(new CellRangeAddress(1, 1, 1, 2));

                    if (dicldapList.Count > 2)
                    {
                       SetExcelCell(sheet, 1, dicldapList.Count, styleHead12, "部門別/科別");
                       sheet.AddMergedRegion(new CellRangeAddress(1, 1, dicldapList.Count, dicldapList.Count));
                    }
                    else
                    {
                       SetExcelCell(sheet, 1, 3, styleHead12, "部門別/科別");
                       sheet.AddMergedRegion(new CellRangeAddress(1, 1, 3, 3));
                       sheet.SetColumnWidth(3, 100 * 40);
                    }

                    //*結果集表頭 line2
                    SetExcelCell(sheet, 2, 0, styleHead12, "發文日期：");
                    sheet.AddMergedRegion(new CellRangeAddress(2, 2, 0, 0));
                    SetExcelCell(sheet, 2, 1, styleHead12, model.SendDateStart + '~' + model.SendDateEnd);
                    sheet.AddMergedRegion(new CellRangeAddress(2, 2, 1, 2));
                    //*結果集表頭 line3
                    SetExcelCell(sheet, 3, 0, styleHead12, "主管放行日：");
                    sheet.AddMergedRegion(new CellRangeAddress(3, 3, 0, 0));
                    SetExcelCell(sheet, 3, 1, styleHead12, model.ApproveDateStart + '~' + model.ApproveDateEnd);
                    sheet.AddMergedRegion(new CellRangeAddress(3, 3, 1, 2));

                    SetExcelCell(sheet, 4, 0, styleHead12, "電子發文上傳日：");
                    sheet.AddMergedRegion(new CellRangeAddress(4, 4, 0, 0));
                    SetExcelCell(sheet, 4, 1, styleHead12, model.SendUpDateStart + '~' + model.SendUpDateEnd);
                    sheet.AddMergedRegion(new CellRangeAddress(4, 4, 1, 2));
                    SetExcelCell(sheet, 5, 0, styleHead12, "發文方式：");
                    sheet.AddMergedRegion(new CellRangeAddress(5, 5, 0, 0));
                    SetExcelCell(sheet, 5, 1, styleHead12, model.SendKind);
                    sheet.AddMergedRegion(new CellRangeAddress(5, 5, 1, 2));

                    //*結果集表頭 line4
                    SetExcelCell(sheet, 6, 0, styleHead10, "處理人員");
                    sheet.AddMergedRegion(new CellRangeAddress(6, 6, 0, 0));
                    sheet.SetColumnWidth(0, 100 * 50);
                    //依次排列人員名稱
                    int a = 1;
                    foreach (var item in dicldapList)
                    {
                       SetExcelCell(sheet, 6, a, styleHead10, item.Value.Split('|')[0]);
                       sheet.AddMergedRegion(new CellRangeAddress(6, 6, a, a));
                       a++;
                    }
                    SetExcelCell(sheet, 6, dicldapList.Count + 1, styleHead10, "合計");
                    sheet.AddMergedRegion(new CellRangeAddress(6, 6, dicldapList.Count + 1, dicldapList.Count + 1));

                    //*扣押案件類型 line5-lineN 
                    for (int i = 0; i < dtCase.Rows.Count; i++)
                    {
                       if (caseKind != dtCase.Rows[i]["New_CaseKind"].ToString())
                       {
                          rows = rows + 1;
                          SetExcelCell(sheet, rows, 0, styleHead10, dtCase.Rows[i]["New_CaseKind"].ToString());
                          sheet.AddMergedRegion(new CellRangeAddress(rows, rows, 0, 0));
                          SetExcelCell(sheet, rows, dicldapList.Count + 1, styleHead10, "0");//最後一列合計賦初值
                          sheet.AddMergedRegion(new CellRangeAddress(rows, rows, dicldapList.Count + 1, dicldapList.Count + 1));
                          caseKind = dtCase.Rows[i]["New_CaseKind"].ToString();
                          for (int j = 0; j < dicldapList.Count; j++)//*初始表格賦初值 
                          {
                             SetExcelCell(sheet, rows, j + 1, styleHead10, "0");
                             sheet.AddMergedRegion(new CellRangeAddress(rows, rows, j + 1, j + 1));
                          }
                       }
                    }

                    //*合計 lineLast (案件下面的合計以及整行賦初值)
                    SetExcelCell(sheet, rows + 1, 0, styleHead10, "合計");
                    sheet.AddMergedRegion(new CellRangeAddress(rows + 1, rows + 1, 0, 0));
                    if (dicldapList.Count > 0)
                    {
                       for (int j = 0; j < dicldapList.Count; j++)//*初始表格賦初值 
                       {
                          SetExcelCell(sheet, rows + 1, j + 1, styleHead10, "0");
                          sheet.AddMergedRegion(new CellRangeAddress(rows + 1, rows + 1, j + 1, j + 1));
                       }
                    }
                    else
                    {
                       SetExcelCell(sheet, rows + 1, 1, styleHead10, "0");
                       sheet.AddMergedRegion(new CellRangeAddress(rows + 1, rows + 1, 1, 1));
                    }
                    #endregion
                    #region  body
                    for (int iRow = 0; iRow < dtCase.Rows.Count; iRow++)//根據案件類型進行循環
                    {
                       foreach (var item in dicldapList)
                       {
                          int irows = Convert.ToInt32(item.Value.Split('|')[1]);
                          if (caseExcel == dtCase.Rows[iRow]["New_CaseKind"].ToString())//重複同一案件類型的數據
                          {
                             if (item.Key == dtCase.Rows[iRow]["AgentUser"].ToString())
                             {
                                SetExcelCell(sheet, rowsExcel, irows, styleHead10, dtCase.Rows[iRow]["case_num"].ToString());
                                rowscountExcelresult = Convert.ToInt32(dtCase.Rows[iRow]["case_num"].ToString());//每格資料
                                SetExcelCell(sheet, rows + 1, irows, styleHead10, dtCase.Rows[iRow]["User_Count"].ToString());//最後一行合計
                                rowscountExcel += rowscountExcelresult;
                                rowstatolExcel += rowscountExcelresult;
                             }
                             SetExcelCell(sheet, rowsExcel, dicldapList.Count + 1, styleHead10, rowscountExcel.ToString());//最後一列合計
                          }
                          else//不重複的案件類型
                          {
                             rowscountExcel = 0;
                             rowsExcel = rowsExcel + 1;
                             if (item.Key == dtCase.Rows[iRow]["AgentUser"].ToString())
                             {
                                SetExcelCell(sheet, rowsExcel, irows, styleHead10, dtCase.Rows[iRow]["case_num"].ToString());
                                rowscountExcelresult = Convert.ToInt32(dtCase.Rows[iRow]["case_num"].ToString());//第一條不重複的數據儲存下值
                                SetExcelCell(sheet, rows + 1, irows, styleHead10, dtCase.Rows[iRow]["User_Count"].ToString());//最後一行合計
                                rowscountExcel += rowscountExcelresult;
                                rowstatolExcel += rowscountExcelresult;
                             }
                             caseExcel = dtCase.Rows[iRow]["New_CaseKind"].ToString();
                             SetExcelCell(sheet, rowsExcel, dicldapList.Count + 1, styleHead10, rowscountExcel.ToString());//最後一列合計
                          }
                       }
                    }
                    SetExcelCell(sheet, rows + 1, dicldapList.Count + 1, styleHead10, rowstatolExcel.ToString());//總合計
                    #endregion
                 }
              }
           }
           else
           {
              for (int k = 0; k < Departmentcount; k++)
              {
                 ISheet sheet = null;
                 Dictionary<string, string> dicldapList = new Dictionary<string, string>();
                 DataTable dtCase = new DataTable();

                 sheet = workbook.CreateSheet(listCode[k].CodeDesc);
                 dtCase = GetCaseMasterList1(model, listCode[k].CodeDesc);

                 sort = 1;

                 //判斷人員
                 foreach (DataRow dr in dtCase.Rows)
                 {
                    if (!dicldapList.Keys.Contains(dr["AgentUser"].ToString()))
                    {
                       dicldapList.Add(dr["AgentUser"].ToString(), dr["EmpName"].ToString() + "|" + sort);
                       sort++;
                    }
                 }
                 if (dicldapList.Count > 2)
                 {
                    SetExcelCell(sheet, 1, dicldapList.Count + 1, styleHead12, listCode[k].CodeDesc);
                    sheet.AddMergedRegion(new CellRangeAddress(1, 1, dicldapList.Count + 1, dicldapList.Count + 1));
                    sheet.SetColumnWidth(dicldapList.Count + 1, 100 * 40);
                 }
                 else
                 {
                    SetExcelCell(sheet, 1, 4, styleHead12, listCode[k].CodeDesc);
                    sheet.AddMergedRegion(new CellRangeAddress(1, 1, 4, 4));
                    sheet.SetColumnWidth(4, 100 * 40);
                 }

                 string caseKind = "";//*去重複
                 int rows = 6;//title中定義行數

                 #region title
                 //*大標題 line0
                 SetExcelCell(sheet, 0, 0, styleHead12, "經辦結案統計表");
                 sheet.AddMergedRegion(new CellRangeAddress(0, 0, 0, dicldapList.Count + 1));

                 //*查詢條件 line1
                 SetExcelCell(sheet, 1, 0, styleHead12, "收件日期：");
                 sheet.AddMergedRegion(new CellRangeAddress(1, 1, 0, 0));
                 SetExcelCell(sheet, 1, 1, styleHead12, model.ReceiveDateStart + '~' + model.ReceiveDateEnd);
                 sheet.AddMergedRegion(new CellRangeAddress(1, 1, 1, 2));

                 if (dicldapList.Count > 2)
                 {
                    SetExcelCell(sheet, 1, dicldapList.Count, styleHead12, "部門別/科別");
                    sheet.AddMergedRegion(new CellRangeAddress(1, 1, dicldapList.Count, dicldapList.Count));
                 }
                 else
                 {
                    SetExcelCell(sheet, 1, 3, styleHead12, "部門別/科別");
                    sheet.AddMergedRegion(new CellRangeAddress(1, 1, 3, 3));
                    sheet.SetColumnWidth(3, 100 * 40);
                 }

                 //*結果集表頭 line2
                 SetExcelCell(sheet, 2, 0, styleHead12, "發文日期：");
                 sheet.AddMergedRegion(new CellRangeAddress(2, 2, 0, 0));
                 SetExcelCell(sheet, 2, 1, styleHead12, model.SendDateStart + '~' + model.SendDateEnd);
                 sheet.AddMergedRegion(new CellRangeAddress(2, 2, 1, 2));
                 //*結果集表頭 line3
                 SetExcelCell(sheet, 3, 0, styleHead12, "主管放行日：");
                 sheet.AddMergedRegion(new CellRangeAddress(3, 3, 0, 0));
                 SetExcelCell(sheet, 3, 1, styleHead12, model.ApproveDateStart + '~' + model.ApproveDateEnd);
                 sheet.AddMergedRegion(new CellRangeAddress(3, 3, 1, 2));

                 SetExcelCell(sheet, 4, 0, styleHead12, "電子發文上傳日：");
                 sheet.AddMergedRegion(new CellRangeAddress(4, 4, 0, 0));
                 SetExcelCell(sheet, 4, 1, styleHead12, model.SendUpDateStart + '~' + model.SendUpDateEnd);
                 sheet.AddMergedRegion(new CellRangeAddress(4, 4, 1, 2));
                 SetExcelCell(sheet, 5, 0, styleHead12, "發文方式：");
                 sheet.AddMergedRegion(new CellRangeAddress(5, 5, 0, 0));
                 SetExcelCell(sheet, 5, 1, styleHead12, model.SendKind);
                 sheet.AddMergedRegion(new CellRangeAddress(5, 5, 1, 2));

                 //*結果集表頭 line4
                 SetExcelCell(sheet, 6, 0, styleHead10, "處理人員");
                 sheet.AddMergedRegion(new CellRangeAddress(6, 6, 0, 0));
                 sheet.SetColumnWidth(0, 100 * 50);
                 //依次排列人員名稱
                 int a = 1;
                 foreach (var item in dicldapList)
                 {
                    SetExcelCell(sheet, 6, a, styleHead10, item.Value.Split('|')[0]);
                    sheet.AddMergedRegion(new CellRangeAddress(6, 6, a, a));
                    a++;
                 }
                 SetExcelCell(sheet, 6, dicldapList.Count + 1, styleHead10, "合計");
                 sheet.AddMergedRegion(new CellRangeAddress(6, 6, dicldapList.Count + 1, dicldapList.Count + 1));

                 //*扣押案件類型 line5-lineN 
                 for (int i = 0; i < dtCase.Rows.Count; i++)
                 {
                    if (caseKind != dtCase.Rows[i]["New_CaseKind"].ToString())
                    {
                       rows = rows + 1;
                       SetExcelCell(sheet, rows, 0, styleHead10, dtCase.Rows[i]["New_CaseKind"].ToString());
                       sheet.AddMergedRegion(new CellRangeAddress(rows, rows, 0, 0));
                       SetExcelCell(sheet, rows, dicldapList.Count + 1, styleHead10, "0");//最後一列合計賦初值
                       sheet.AddMergedRegion(new CellRangeAddress(rows, rows, dicldapList.Count + 1, dicldapList.Count + 1));
                       caseKind = dtCase.Rows[i]["New_CaseKind"].ToString();
                       for (int j = 0; j < dicldapList.Count; j++)//*初始表格賦初值 
                       {
                          SetExcelCell(sheet, rows, j + 1, styleHead10, "0");
                          sheet.AddMergedRegion(new CellRangeAddress(rows, rows, j + 1, j + 1));
                       }
                    }
                 }

                 //*合計 lineLast (案件下面的合計以及整行賦初值)
                 SetExcelCell(sheet, rows + 1, 0, styleHead10, "合計");
                 sheet.AddMergedRegion(new CellRangeAddress(rows + 1, rows + 1, 0, 0));
                 if (dicldapList.Count > 0)
                 {
                    for (int j = 0; j < dicldapList.Count; j++)//*初始表格賦初值 
                    {
                       SetExcelCell(sheet, rows + 1, j + 1, styleHead10, "0");
                       sheet.AddMergedRegion(new CellRangeAddress(rows + 1, rows + 1, j + 1, j + 1));
                    }
                 }
                 else
                 {
                    SetExcelCell(sheet, rows + 1, 1, styleHead10, "0");
                    sheet.AddMergedRegion(new CellRangeAddress(rows + 1, rows + 1, 1, 1));
                 }
                 #endregion
                 #region  body

                 caseExcel = "";//案件類型
                 rowsExcel = 6;//行數
                 rowscountExcel = 0;//最後一列合計
                 rowstatolExcel = 0;//總合計
 
                 for (int iRow = 0; iRow < dtCase.Rows.Count; iRow++)//根據案件類型進行循環
                 {
                    foreach (var item in dicldapList)
                    {
                       int irows = Convert.ToInt32(item.Value.Split('|')[1]);
                       if (caseExcel == dtCase.Rows[iRow]["New_CaseKind"].ToString())//重複同一案件類型的數據
                       {
                          if (item.Key == dtCase.Rows[iRow]["AgentUser"].ToString())
                          {
                             SetExcelCell(sheet, rowsExcel, irows, styleHead10, dtCase.Rows[iRow]["case_num"].ToString());
                             rowscountExcelresult = Convert.ToInt32(dtCase.Rows[iRow]["case_num"].ToString());//每格資料
                             SetExcelCell(sheet, rows + 1, irows, styleHead10, dtCase.Rows[iRow]["User_Count"].ToString());//最後一行合計
                             rowscountExcel += rowscountExcelresult;
                             rowstatolExcel += rowscountExcelresult;
                          }
                          SetExcelCell(sheet, rowsExcel, dicldapList.Count + 1, styleHead10, rowscountExcel.ToString());//最後一列合計
                       }
                       else//不重複的案件類型
                       {
                          rowscountExcel = 0;
                          rowsExcel = rowsExcel + 1;
                          if (item.Key == dtCase.Rows[iRow]["AgentUser"].ToString())
                          {
                             SetExcelCell(sheet, rowsExcel, irows, styleHead10, dtCase.Rows[iRow]["case_num"].ToString());
                             rowscountExcelresult = Convert.ToInt32(dtCase.Rows[iRow]["case_num"].ToString());//第一條不重複的數據儲存下值
                             SetExcelCell(sheet, rows + 1, irows, styleHead10, dtCase.Rows[iRow]["User_Count"].ToString());//最後一行合計
                             rowscountExcel += rowscountExcelresult;
                             rowstatolExcel += rowscountExcelresult;
                          }
                          caseExcel = dtCase.Rows[iRow]["New_CaseKind"].ToString();
                          SetExcelCell(sheet, rowsExcel, dicldapList.Count + 1, styleHead10, rowscountExcel.ToString());//最後一列合計
                       }
                    }
                 }
                 SetExcelCell(sheet, rows + 1, dicldapList.Count + 1, styleHead10, rowstatolExcel.ToString());//總合計
                 #endregion
              }
           }
           MemoryStream ms = new MemoryStream();
           workbook.Write(ms);
           ms.Flush();
           ms.Position = 0;
           workbook = null;
           return ms;
        }
        #endregion
        //20170714 固定 RQ-2015-019666-019 派件至跨單位(原產生邏輯程式寫死，組織異動必出問題，改為參數) 宏祥 add end

        //#region 品質時效統計表1
        //public MemoryStream TradeListReportExcel_NPOI1(CaseClosedQuery model)
        //{
        //    IWorkbook workbook = new HSSFWorkbook();
        //    ISheet sheet = null;
        //    ISheet sheet2 = null;
        //    ISheet sheet3 = null;

        //    DataTable dt = new DataTable();
        //    DataTable dt2 = new DataTable();
        //    DataTable dt3 = new DataTable();

        //    #region def style
        //    ICellStyle styleHead12 = workbook.CreateCellStyle();
        //    IFont font12 = workbook.CreateFont();
        //    font12.FontHeightInPoints = 12;
        //    font12.FontName = "新細明體";
        //    styleHead12.FillPattern = FillPattern.SolidForeground;
        //    styleHead12.FillForegroundColor = HSSFColor.White.Index;
        //    styleHead12.BorderTop = BorderStyle.None;
        //    styleHead12.BorderLeft = BorderStyle.None;
        //    styleHead12.BorderRight = BorderStyle.None;
        //    styleHead12.BorderBottom = BorderStyle.None;
        //    styleHead12.WrapText = true;
        //    styleHead12.Alignment = HorizontalAlignment.Center;
        //    styleHead12.VerticalAlignment = VerticalAlignment.Center;
        //    styleHead12.SetFont(font12);

        //    ICellStyle styleHead10 = workbook.CreateCellStyle();
        //    IFont font10 = workbook.CreateFont();
        //    font10.FontHeightInPoints = 10;
        //    font10.FontName = "新細明體";
        //    styleHead10.FillPattern = FillPattern.SolidForeground;
        //    styleHead10.FillForegroundColor = HSSFColor.White.Index;
        //    styleHead10.BorderTop = BorderStyle.Thin;
        //    styleHead10.BorderLeft = BorderStyle.Thin;
        //    styleHead10.BorderRight = BorderStyle.Thin;
        //    styleHead10.BorderBottom = BorderStyle.Thin;
        //    styleHead10.WrapText = true;
        //    styleHead10.Alignment = HorizontalAlignment.Left;
        //    styleHead10.VerticalAlignment = VerticalAlignment.Center;
        //    styleHead10.SetFont(font10);
        //    #endregion

        //    #region 獲取數據源(集作一科及案件資料)
        //    //獲取人員
        //    if (model.Depart == "1")//* 集作一科
        //    {
        //        sheet = workbook.CreateSheet("集作一科");
        //        dt = GetTradeList1(model, "集作一科");//獲取查詢集作一科的案件
        //        SetExcelCell(sheet, 1, 6, styleHead12, "集作一科");
        //        sheet.AddMergedRegion(new CellRangeAddress(1, 1, 6, 6));
        //    }
        //    if (model.Depart == "2")//* 集作二科
        //    {
        //        sheet = workbook.CreateSheet("集作二科");
        //        dt = GetTradeList1(model, "集作二科");//獲取查詢集作二科的案件
        //        SetExcelCell(sheet, 1, 6, styleHead12, "集作二科");
        //        sheet.AddMergedRegion(new CellRangeAddress(1, 1, 6, 6));
        //    }
        //    if (model.Depart == "3")//*集作三科
        //    {
        //        sheet = workbook.CreateSheet("集作三科");
        //        dt = GetTradeList1(model, "集作三科");//獲取查詢集作三科的案件
        //        SetExcelCell(sheet, 1, 6, styleHead12, "集作三科");
        //        sheet.AddMergedRegion(new CellRangeAddress(1, 1, 6, 6));
        //    }
        //    if (model.Depart == "0")//*全部
        //    {
        //        sheet = workbook.CreateSheet("集作一科");
        //        sheet2 = workbook.CreateSheet("集作二科");
        //        sheet3 = workbook.CreateSheet("集作三科");
        //        dt = GetTradeList1(model, "集作一科");//獲取查詢集作一科的案件
        //        dt2 = GetTradeList1(model, "集作二科");//獲取查詢集作二科的案件
        //        dt3 = GetTradeList1(model, "集作三科");//獲取查詢集作三科的案件
        //        SetExcelCell(sheet, 1, 6, styleHead12, "集作一科");
        //        sheet.AddMergedRegion(new CellRangeAddress(1, 1, 6, 6));
        //        SetExcelCell(sheet2, 1, 6, styleHead12, "集作二科");
        //        sheet2.AddMergedRegion(new CellRangeAddress(1, 1, 6, 6));
        //        SetExcelCell(sheet3, 1, 6, styleHead12, "集作三科");
        //        sheet3.AddMergedRegion(new CellRangeAddress(1, 1, 6, 6));
        //    }
        //    #endregion

        //    #region title
        //    SetExcelCell(sheet, 0, 0, styleHead12, "品質時效統計表");
        //    sheet.AddMergedRegion(new CellRangeAddress(0, 0, 0, 6));

        //    //* line1
        //    SetExcelCell(sheet, 1, 0, styleHead12, "收件日期：");
        //    sheet.AddMergedRegion(new CellRangeAddress(1, 1, 0, 0));
        //    SetExcelCell(sheet, 1, 1, styleHead12, model.ReceiveDateStart + '~' + model.ReceiveDateEnd);
        //    sheet.AddMergedRegion(new CellRangeAddress(1, 1, 1, 2));

        //    SetExcelCell(sheet, 2, 0, styleHead12, "發文日期：");
        //    sheet.AddMergedRegion(new CellRangeAddress(2, 2, 0, 0));
        //    SetExcelCell(sheet, 2, 1, styleHead12, model.SendDateStart + '~' + model.SendDateEnd);
        //    sheet.AddMergedRegion(new CellRangeAddress(2, 2, 1, 2));

        //    SetExcelCell(sheet, 3, 0, styleHead12, "主管放行日：");
        //    sheet.AddMergedRegion(new CellRangeAddress(3, 3, 0, 0));
        //    SetExcelCell(sheet, 3, 1, styleHead12, model.ApproveDateStart + '~' + model.ApproveDateEnd);
        //    sheet.AddMergedRegion(new CellRangeAddress(3, 3, 1, 2));
        //    SetExcelCell(sheet, 1, 5, styleHead12, "部門別/科別:");
        //    sheet.AddMergedRegion(new CellRangeAddress(1, 1, 5, 5));

        //    SetExcelCell(sheet, 4, 0, styleHead12, "電子發文上傳日：");
        //    sheet.AddMergedRegion(new CellRangeAddress(4, 4, 0, 0));
        //    SetExcelCell(sheet, 4, 1, styleHead12, model.SendUpDateStart + '~' + model.SendUpDateEnd);
        //    sheet.AddMergedRegion(new CellRangeAddress(4, 4, 1, 2));

        //    SetExcelCell(sheet, 5, 0, styleHead12, "發文方式：");
        //    sheet.AddMergedRegion(new CellRangeAddress(5, 5, 0, 0));
        //    SetExcelCell(sheet, 5, 1, styleHead12, model.SendKind);
        //    sheet.AddMergedRegion(new CellRangeAddress(5, 5, 1, 2));

        //    //* line2
        //    SetExcelCell(sheet, 6, 0, styleHead10, "案件類別-大類");
        //    sheet.AddMergedRegion(new CellRangeAddress(4, 4, 0, 0));
        //    SetExcelCell(sheet, 6, 1, styleHead10, "經辦人員");
        //    sheet.AddMergedRegion(new CellRangeAddress(4, 4, 1, 1));
        //    SetExcelCell(sheet, 6, 2, styleHead10, "件數");
        //    sheet.AddMergedRegion(new CellRangeAddress(4, 4, 2, 2));
        //    SetExcelCell(sheet, 6, 3, styleHead10, "退件件數");
        //    sheet.AddMergedRegion(new CellRangeAddress(4, 4, 3, 3));
        //    SetExcelCell(sheet, 6, 4, styleHead10, "退件率");
        //    sheet.AddMergedRegion(new CellRangeAddress(4, 4, 4, 4));
        //    SetExcelCell(sheet, 6, 5, styleHead10, "逾期件數");
        //    sheet.AddMergedRegion(new CellRangeAddress(4, 4, 5, 5));
        //    SetExcelCell(sheet, 6, 6, styleHead10, "逾期率");
        //    sheet.AddMergedRegion(new CellRangeAddress(4, 4, 6, 6));

        //    SetExcelCell(sheet, dt.Rows.Count + 7, 1, styleHead12, "合計");
        //    sheet.AddMergedRegion(new CellRangeAddress(dt.Rows.Count + 7, dt.Rows.Count + 7, 1, 1));

        //    for (int i = 0; i < dt.Rows.Count + 1; i++)//*初始表格賦初值 
        //    {
        //        for (int j = 1; j < 6; j++)
        //        {
        //            SetExcelCell(sheet, i + 7, j + 1, styleHead12, "0");
        //            sheet.AddMergedRegion(new CellRangeAddress(i + 7, i + 7, j + 1, j + 1));
        //        }
        //    }
        //    #endregion
        //    #region Width
        //    sheet.SetColumnWidth(0, 100 * 40);
        //    sheet.SetColumnWidth(1, 100 * 40);
        //    sheet.SetColumnWidth(2, 100 * 40);
        //    sheet.SetColumnWidth(3, 100 * 40);
        //    sheet.SetColumnWidth(4, 100 * 40);
        //    sheet.SetColumnWidth(5, 100 * 40);
        //    sheet.SetColumnWidth(6, 100 * 40);
        //    #endregion
        //    #region body
        //    int totalNum = 0;
        //    int totalNumCount = 0;
        //    int totalReturnNum = 0;
        //    int totalReturnNumCount = 0;
        //    int totalOverDue = 0;
        //    int totalOverDueCount = 0;
        //    for (int iRow = 0; iRow < dt.Rows.Count; iRow++)
        //    {
        //        for (int iCol = 0; iCol < dt.Columns.Count; iCol++)
        //        {
        //            SetExcelCell(sheet, iRow + 7, iCol, styleHead10, dt.Rows[iRow][iCol].ToString());
        //        }
        //        totalNum = Convert.ToInt32(dt.Rows[iRow]["Case"].ToString());
        //        totalNumCount += totalNum;
        //        totalReturnNum = Convert.ToInt32(dt.Rows[iRow]["ReturnCase"].ToString());
        //        totalReturnNumCount += totalReturnNum;
        //        totalOverDue = Convert.ToInt32(dt.Rows[iRow]["OutCase"].ToString());
        //        totalOverDueCount += totalOverDue;
        //    }
        //    SetExcelCell(sheet, dt.Rows.Count + 7, 2, styleHead12, totalNumCount.ToString());
        //    SetExcelCell(sheet, dt.Rows.Count + 7, 3, styleHead12, totalReturnNumCount.ToString());
        //    if (totalReturnNumCount == 0)
        //    {
        //        SetExcelCell(sheet, dt.Rows.Count + 7, 4, styleHead12, "0.00%");
        //    }
        //    else
        //    {
        //        string result = (((totalReturnNumCount * 1.0) / totalNumCount) * 100).ToString();
        //        SetExcelCell(sheet, dt.Rows.Count + 7, 4, styleHead12, StringHelper.getFarmatNums(result) + "%");
        //        //if (result.ToString().IndexOf(".") > 0)
        //        //{
        //        //    SetExcelCell(sheet, dt.Rows.Count + 5, 4, styleHead12, (  result.Substring(0, result.IndexOf(".") + 3)) + "%");
        //        //}
        //        //else
        //        //{
        //        //    SetExcelCell(sheet, dt.Rows.Count + 5, 4, styleHead12, result + ".00%");
        //        //}
        //    }
        //    SetExcelCell(sheet, dt.Rows.Count + 7, 5, styleHead12, totalOverDueCount.ToString());
        //    if (totalOverDueCount == 0)
        //    {
        //        SetExcelCell(sheet, dt.Rows.Count + 7, 6, styleHead12, "0.00%");
        //    }
        //    else
        //    {
        //        string results = (((totalOverDueCount * 1.0) / totalNumCount) * 100).ToString();
        //        SetExcelCell(sheet, dt.Rows.Count + 7, 6, styleHead12, StringHelper.getFarmatNums(results) + "%");

        //        //if (results.ToString().IndexOf(".") > 0)
        //        //{
        //        //    SetExcelCell(sheet, dt.Rows.Count + 5, 6, styleHead12, (results.Substring(0, results.IndexOf(".") + 3)) + "%");
        //        //}
        //        //else
        //        //{
        //        //    SetExcelCell(sheet, dt.Rows.Count + 5, 6, styleHead12, results + ".00%");
        //        //}
        //    }
        //    #endregion

        //    if (model.Depart == "0")//* 全部
        //    {
        //        #region title2
        //        SetExcelCell(sheet2, 0, 0, styleHead12, "品質時效統計表");
        //        sheet2.AddMergedRegion(new CellRangeAddress(0, 0, 0, 6));

        //        //* line1
        //        SetExcelCell(sheet2, 1, 0, styleHead12, "收件日期：");
        //        sheet2.AddMergedRegion(new CellRangeAddress(1, 1, 0, 0));
        //        SetExcelCell(sheet2, 1, 1, styleHead12, model.ReceiveDateStart + '~' + model.ReceiveDateEnd);
        //        sheet2.AddMergedRegion(new CellRangeAddress(1, 1, 1, 2));

        //        SetExcelCell(sheet2, 2, 0, styleHead12, "發文日期：");
        //        sheet2.AddMergedRegion(new CellRangeAddress(2, 2, 0, 0));
        //        SetExcelCell(sheet2, 2, 1, styleHead12, model.SendDateStart + '~' + model.SendDateEnd);
        //        sheet2.AddMergedRegion(new CellRangeAddress(2, 2, 1, 2));

        //        SetExcelCell(sheet2, 3, 0, styleHead12, "主管放行日：");
        //        sheet2.AddMergedRegion(new CellRangeAddress(3, 3, 0, 0));
        //        SetExcelCell(sheet2, 3, 1, styleHead12, model.ApproveDateStart + '~' + model.ApproveDateEnd);
        //        sheet2.AddMergedRegion(new CellRangeAddress(3, 3, 1, 2));

        //        SetExcelCell(sheet2, 1, 5, styleHead12, "部門別/科別:");
        //        sheet2.AddMergedRegion(new CellRangeAddress(1, 1, 5, 5));

        //        SetExcelCell(sheet2, 4, 0, styleHead12, "電子發文上傳日：");
        //        sheet2.AddMergedRegion(new CellRangeAddress(4, 4, 0, 0));
        //        SetExcelCell(sheet2, 4, 1, styleHead12, model.SendUpDateStart + '~' + model.SendUpDateEnd);
        //        sheet2.AddMergedRegion(new CellRangeAddress(4, 4, 1, 2));

        //        SetExcelCell(sheet2, 5, 0, styleHead12, "發文方式：");
        //        sheet2.AddMergedRegion(new CellRangeAddress(5, 5, 0, 0));
        //        SetExcelCell(sheet2, 5, 1, styleHead12, model.SendKind);
        //        sheet2.AddMergedRegion(new CellRangeAddress(5, 5, 1, 2));

        //        //* line2
        //        SetExcelCell(sheet2, 6, 0, styleHead10, "案件類別-大類");
        //        sheet2.AddMergedRegion(new CellRangeAddress(4, 4, 0, 0));
        //        SetExcelCell(sheet2, 6, 1, styleHead10, "經辦人員");
        //        sheet2.AddMergedRegion(new CellRangeAddress(4, 4, 1, 1));
        //        SetExcelCell(sheet2, 6, 2, styleHead10, "件數");
        //        sheet2.AddMergedRegion(new CellRangeAddress(4, 4, 2, 2));
        //        SetExcelCell(sheet2, 6, 3, styleHead10, "退件件數");
        //        sheet2.AddMergedRegion(new CellRangeAddress(4, 4, 3, 3));
        //        SetExcelCell(sheet2, 6, 4, styleHead10, "退件率");
        //        sheet2.AddMergedRegion(new CellRangeAddress(4, 4, 4, 4));
        //        SetExcelCell(sheet2, 6, 5, styleHead10, "逾期件數");
        //        sheet2.AddMergedRegion(new CellRangeAddress(4, 4, 5, 5));
        //        SetExcelCell(sheet2, 6, 6, styleHead10, "逾期率");
        //        sheet2.AddMergedRegion(new CellRangeAddress(4, 4, 6, 6));

        //        SetExcelCell(sheet2, dt2.Rows.Count + 7, 1, styleHead12, "合計");
        //        sheet2.AddMergedRegion(new CellRangeAddress(dt2.Rows.Count + 7, dt2.Rows.Count + 7, 1, 1));

        //        for (int i = 0; i < dt2.Rows.Count + 1; i++)//*初始表格賦初值 
        //        {
        //            for (int j = 1; j < 6; j++)
        //            {
        //                SetExcelCell(sheet2, i + 7, j + 1, styleHead12, "0");
        //                sheet2.AddMergedRegion(new CellRangeAddress(i + 7, i + 7, j + 1, j + 1));
        //            }
        //        }
        //        #endregion

        //        #region title3
        //        SetExcelCell(sheet3, 0, 0, styleHead12, "品質時效統計表");
        //        sheet3.AddMergedRegion(new CellRangeAddress(0, 0, 0, 6));

        //        //* line1
        //        SetExcelCell(sheet3, 1, 0, styleHead12, "收件日期：");
        //        sheet3.AddMergedRegion(new CellRangeAddress(1, 1, 0, 0));
        //        SetExcelCell(sheet3, 1, 1, styleHead12, model.ReceiveDateStart + '~' + model.ReceiveDateEnd);
        //        sheet3.AddMergedRegion(new CellRangeAddress(1, 1, 1, 2));

        //        SetExcelCell(sheet3, 2, 0, styleHead12, "發文日期：");
        //        sheet3.AddMergedRegion(new CellRangeAddress(2, 2, 0, 0));
        //        SetExcelCell(sheet3, 2, 1, styleHead12, model.SendDateStart + '~' + model.SendDateEnd);
        //        sheet3.AddMergedRegion(new CellRangeAddress(2, 2, 1, 2));

        //        SetExcelCell(sheet3, 3, 0, styleHead12, "主管放行日：");
        //        sheet3.AddMergedRegion(new CellRangeAddress(3, 3, 0, 0));
        //        SetExcelCell(sheet3, 3, 1, styleHead12, model.ApproveDateStart + '~' + model.ApproveDateEnd);
        //        sheet3.AddMergedRegion(new CellRangeAddress(3, 3, 1, 2));

        //        SetExcelCell(sheet3, 1, 5, styleHead12, "部門別/科別:");
        //        sheet3.AddMergedRegion(new CellRangeAddress(1, 1, 5, 5));

        //        SetExcelCell(sheet3, 4, 0, styleHead12, "電子發文上傳日：");
        //        sheet3.AddMergedRegion(new CellRangeAddress(4, 4, 0, 0));
        //        SetExcelCell(sheet3, 4, 1, styleHead12, model.SendUpDateStart + '~' + model.SendUpDateEnd);
        //        sheet3.AddMergedRegion(new CellRangeAddress(4, 4, 1, 2));

        //        SetExcelCell(sheet3, 5, 0, styleHead12, "發文方式：");
        //        sheet3.AddMergedRegion(new CellRangeAddress(5, 5, 0, 0));
        //        SetExcelCell(sheet3, 5, 1, styleHead12, model.SendKind);
        //        sheet3.AddMergedRegion(new CellRangeAddress(5, 5, 1, 2));

        //        //* line2
        //        SetExcelCell(sheet3, 6, 0, styleHead10, "案件類別-大類");
        //        sheet3.AddMergedRegion(new CellRangeAddress(4, 4, 0, 0));
        //        SetExcelCell(sheet3, 6, 1, styleHead10, "經辦人員");
        //        sheet3.AddMergedRegion(new CellRangeAddress(4, 4, 1, 1));
        //        SetExcelCell(sheet3, 6, 2, styleHead10, "件數");
        //        sheet3.AddMergedRegion(new CellRangeAddress(4, 4, 2, 2));
        //        SetExcelCell(sheet3, 6, 3, styleHead10, "退件件數");
        //        sheet3.AddMergedRegion(new CellRangeAddress(4, 4, 3, 3));
        //        SetExcelCell(sheet3, 6, 4, styleHead10, "退件率");
        //        sheet3.AddMergedRegion(new CellRangeAddress(4, 4, 4, 4));
        //        SetExcelCell(sheet3, 6, 5, styleHead10, "逾期件數");
        //        sheet3.AddMergedRegion(new CellRangeAddress(4, 4, 5, 5));
        //        SetExcelCell(sheet3, 6, 6, styleHead10, "逾期率");
        //        sheet3.AddMergedRegion(new CellRangeAddress(4, 4, 6, 6));

        //        SetExcelCell(sheet3, dt3.Rows.Count + 7, 1, styleHead12, "合計");
        //        sheet3.AddMergedRegion(new CellRangeAddress(dt3.Rows.Count + 7, dt3.Rows.Count + 7, 1, 1));

        //        for (int i = 0; i < dt3.Rows.Count + 1; i++)//*初始表格賦初值 
        //        {
        //            for (int j = 1; j < 6; j++)
        //            {
        //                SetExcelCell(sheet3, i + 7, j + 1, styleHead12, "0");
        //                sheet3.AddMergedRegion(new CellRangeAddress(i + 7, i + 7, j + 1, j + 1));
        //            }
        //        }
        //        #endregion
        //        #region Width2
        //        sheet2.SetColumnWidth(0, 100 * 40);
        //        sheet2.SetColumnWidth(1, 100 * 40);
        //        sheet2.SetColumnWidth(2, 100 * 40);
        //        sheet2.SetColumnWidth(3, 100 * 40);
        //        sheet2.SetColumnWidth(4, 100 * 40);
        //        sheet2.SetColumnWidth(5, 100 * 40);
        //        sheet2.SetColumnWidth(6, 100 * 40);
        //        #endregion
        //        #region Width3
        //        sheet3.SetColumnWidth(0, 100 * 40);
        //        sheet3.SetColumnWidth(1, 100 * 40);
        //        sheet3.SetColumnWidth(2, 100 * 40);
        //        sheet3.SetColumnWidth(3, 100 * 40);
        //        sheet3.SetColumnWidth(4, 100 * 40);
        //        sheet3.SetColumnWidth(5, 100 * 40);
        //        sheet3.SetColumnWidth(6, 100 * 40);
        //        #endregion
        //        #region body2
        //        int totalNums = 0;
        //        int totalNumCounts = 0;
        //        int totalReturnNums = 0;
        //        int totalReturnNumCounts = 0;
        //        int totalOverDues = 0;
        //        int totalOverDueCounts = 0;
        //        for (int iRow2 = 0; iRow2 < dt2.Rows.Count; iRow2++)
        //        {
        //            for (int iCol = 0; iCol < dt2.Columns.Count; iCol++)
        //            {
        //                SetExcelCell(sheet2, iRow2 + 7, iCol, styleHead10, dt2.Rows[iRow2][iCol].ToString());
        //            }
        //            totalNums = Convert.ToInt32(dt2.Rows[iRow2]["Case"].ToString());
        //            totalNumCounts += totalNums;
        //            totalReturnNums = Convert.ToInt32(dt2.Rows[iRow2]["ReturnCase"].ToString());
        //            totalReturnNumCounts += totalReturnNums;
        //            totalOverDues = Convert.ToInt32(dt2.Rows[iRow2]["OutCase"].ToString());
        //            totalOverDueCounts += totalOverDues;
        //        }
        //        SetExcelCell(sheet2, dt2.Rows.Count + 7, 2, styleHead12, totalNumCounts.ToString());
        //        SetExcelCell(sheet2, dt2.Rows.Count + 7, 3, styleHead12, totalReturnNumCounts.ToString());
        //        if (totalReturnNumCounts == 0)
        //        {
        //            SetExcelCell(sheet2, dt2.Rows.Count + 7, 4, styleHead12, "0.00%");
        //        }
        //        else
        //        {
        //            string tuijian = (((totalReturnNumCounts * 1.0) / totalNumCounts) * 100).ToString();
        //            SetExcelCell(sheet2, dt2.Rows.Count + 7, 4, styleHead12, StringHelper.getFarmatNums(tuijian) + "%");
        //            //if (tuijian.ToString().IndexOf(".") > 0)
        //            //{
        //            //    SetExcelCell(sheet2, dt2.Rows.Count + 5, 4, styleHead12, (tuijian.Substring(0, tuijian.IndexOf(".") + 3)) + "%");
        //            //}
        //            //else
        //            //{
        //            //    SetExcelCell(sheet2, dt2.Rows.Count + 5, 4, styleHead12, tuijian + ".00%");
        //            //}
        //        }
        //        SetExcelCell(sheet2, dt2.Rows.Count + 7, 5, styleHead12, totalOverDueCounts.ToString());
        //        if (totalOverDueCounts == 0)
        //        {
        //            SetExcelCell(sheet2, dt2.Rows.Count + 7, 6, styleHead12, "0.00%");
        //        }
        //        else
        //        {
        //            string results = (((totalOverDueCounts * 1.0) / totalNumCounts) * 100).ToString();
        //            SetExcelCell(sheet2, dt2.Rows.Count + 7, 6, styleHead12, StringHelper.getFarmatNums(results) + "%");
        //            //if (results.ToString().IndexOf(".") > 0)
        //            //{
        //            //    SetExcelCell(sheet2, dt2.Rows.Count + 5, 6, styleHead12, (results.Substring(0, results.IndexOf(".") + 3)) + "%");
        //            //}
        //            //else
        //            //{
        //            //    SetExcelCell(sheet2, dt2.Rows.Count + 5, 6, styleHead12, results + ".00%");
        //            //}
        //        }
        //        #endregion

        //        #region body3
        //        int totalNum3 = 0;
        //        int totalNumCount3 = 0;
        //        int totalReturnNum3 = 0;
        //        int totalReturnNumCount3 = 0;
        //        int totalOverDue3 = 0;
        //        int totalOverDueCount3 = 0;
        //        for (int iRow3 = 0; iRow3 < dt3.Rows.Count; iRow3++)
        //        {
        //            for (int iCol = 0; iCol < dt3.Columns.Count; iCol++)
        //            {
        //                SetExcelCell(sheet3, iRow3 + 7, iCol, styleHead10, dt3.Rows[iRow3][iCol].ToString());
        //            }
        //            totalNum3 = Convert.ToInt32(dt3.Rows[iRow3]["Case"].ToString());
        //            totalNumCount3 += totalNum3;
        //            totalReturnNum3 = Convert.ToInt32(dt3.Rows[iRow3]["ReturnCase"].ToString());
        //            totalReturnNumCount3 += totalReturnNum3;
        //            totalOverDue3 = Convert.ToInt32(dt3.Rows[iRow3]["OutCase"].ToString());
        //            totalOverDueCount3 += totalOverDue3;
        //        }
        //        SetExcelCell(sheet3, dt3.Rows.Count + 7, 2, styleHead12, totalNumCount3.ToString());
        //        SetExcelCell(sheet3, dt3.Rows.Count + 7, 3, styleHead12, totalReturnNumCount3.ToString());
        //        if (totalReturnNumCount3 == 0)
        //        {
        //            SetExcelCell(sheet3, dt3.Rows.Count + 7, 4, styleHead12, "0.00%");
        //        }
        //        else
        //        {
        //            string result3 = (((totalReturnNumCount3 * 1.0) / totalNumCount3) * 100).ToString();
        //            SetExcelCell(sheet3, dt3.Rows.Count + 7, 4, styleHead12, StringHelper.getFarmatNums(result3) + "%");
        //            //if (result3.ToString().IndexOf(".") > 0)
        //            //{
        //            //    SetExcelCell(sheet3, dt3.Rows.Count + 5, 4, styleHead12, (result3.Substring(0, result3.IndexOf(".") + 3)) + "%");
        //            //}
        //            //else
        //            //{
        //            //    SetExcelCell(sheet3, dt3.Rows.Count + 5, 4, styleHead12, result3 + ".00%");
        //            //}
        //        }
        //        SetExcelCell(sheet3, dt3.Rows.Count + 7, 5, styleHead12, totalOverDueCount3.ToString());
        //        if (totalOverDueCount3 == 0)
        //        {
        //            SetExcelCell(sheet3, dt3.Rows.Count + 7, 6, styleHead12, "0.00%");
        //        }
        //        else
        //        {
        //            string results3 = (((totalOverDueCount3 * 1.0) / totalNumCount3) * 100).ToString();
        //            SetExcelCell(sheet3, dt3.Rows.Count + 7, 6, styleHead12, StringHelper.getFarmatNums(results3) + "%");
        //            //if (results3.ToString().IndexOf(".") > 0)
        //            //{
        //            //    SetExcelCell(sheet3, dt3.Rows.Count + 5, 6, styleHead12, (results3.Substring(0, results3.IndexOf(".") + 3)) + "%");
        //            //}
        //            //else
        //            //{
        //            //    SetExcelCell(sheet3, dt3.Rows.Count + 5, 6, styleHead12, results3 + ".00%");
        //            //}
        //        }
        //        #endregion
        //    }

        //    MemoryStream ms = new MemoryStream();
        //    workbook.Write(ms);
        //    ms.Flush();
        //    ms.Position = 0;
        //    workbook = null;
        //    return ms;
        //}

        //#endregion

        //20170811 RC RQ-2015-019666-020 派件至跨單位(原產生邏輯程式寫死，組織異動必出問題，改為參數) 宏祥 add start
        #region 品質時效統計表1
        public MemoryStream TradeListReportExcel_NPOI1(CaseClosedQuery model, IList<PARMCode> listCode)
        {
           IWorkbook workbook = new HSSFWorkbook();
           int Departmentcount = listCode.Count();

           #region def style
           ICellStyle styleHead12 = workbook.CreateCellStyle();
           IFont font12 = workbook.CreateFont();
           font12.FontHeightInPoints = 12;
           font12.FontName = "新細明體";
           styleHead12.FillPattern = FillPattern.SolidForeground;
           styleHead12.FillForegroundColor = HSSFColor.White.Index;
           styleHead12.BorderTop = BorderStyle.None;
           styleHead12.BorderLeft = BorderStyle.None;
           styleHead12.BorderRight = BorderStyle.None;
           styleHead12.BorderBottom = BorderStyle.None;
           styleHead12.WrapText = true;
           styleHead12.Alignment = HorizontalAlignment.Center;
           styleHead12.VerticalAlignment = VerticalAlignment.Center;
           styleHead12.SetFont(font12);

           ICellStyle styleHead10 = workbook.CreateCellStyle();
           IFont font10 = workbook.CreateFont();
           font10.FontHeightInPoints = 10;
           font10.FontName = "新細明體";
           styleHead10.FillPattern = FillPattern.SolidForeground;
           styleHead10.FillForegroundColor = HSSFColor.White.Index;
           styleHead10.BorderTop = BorderStyle.Thin;
           styleHead10.BorderLeft = BorderStyle.Thin;
           styleHead10.BorderRight = BorderStyle.Thin;
           styleHead10.BorderBottom = BorderStyle.Thin;
           styleHead10.WrapText = true;
           styleHead10.Alignment = HorizontalAlignment.Left;
           styleHead10.VerticalAlignment = VerticalAlignment.Center;
           styleHead10.SetFont(font10);
           #endregion

           //判斷科別搜尋條件
           if (model.Depart != "0")
           {
              for (int k = 0; k < Departmentcount; k++)
              {
                 if (model.Depart == (k + 1).ToString())
                 {
                    ISheet sheet = null;
                    DataTable dt = new DataTable();

                    sheet = workbook.CreateSheet(listCode[k].CodeDesc);
                    dt = GetTradeList1(model, listCode[k].CodeDesc);//獲取查詢科別的案件
                    SetExcelCell(sheet, 1, 6, styleHead12, listCode[k].CodeDesc);
                    sheet.AddMergedRegion(new CellRangeAddress(1, 1, 6, 6));

                    #region title
                    SetExcelCell(sheet, 0, 0, styleHead12, "品質時效統計表");
                    sheet.AddMergedRegion(new CellRangeAddress(0, 0, 0, 6));

                    //* line1
                    SetExcelCell(sheet, 1, 0, styleHead12, "收件日期：");
                    sheet.AddMergedRegion(new CellRangeAddress(1, 1, 0, 0));
                    SetExcelCell(sheet, 1, 1, styleHead12, model.ReceiveDateStart + '~' + model.ReceiveDateEnd);
                    sheet.AddMergedRegion(new CellRangeAddress(1, 1, 1, 2));

                    SetExcelCell(sheet, 2, 0, styleHead12, "發文日期：");
                    sheet.AddMergedRegion(new CellRangeAddress(2, 2, 0, 0));
                    SetExcelCell(sheet, 2, 1, styleHead12, model.SendDateStart + '~' + model.SendDateEnd);
                    sheet.AddMergedRegion(new CellRangeAddress(2, 2, 1, 2));

                    SetExcelCell(sheet, 3, 0, styleHead12, "主管放行日：");
                    sheet.AddMergedRegion(new CellRangeAddress(3, 3, 0, 0));
                    SetExcelCell(sheet, 3, 1, styleHead12, model.ApproveDateStart + '~' + model.ApproveDateEnd);
                    sheet.AddMergedRegion(new CellRangeAddress(3, 3, 1, 2));
                    SetExcelCell(sheet, 1, 5, styleHead12, "部門別/科別:");
                    sheet.AddMergedRegion(new CellRangeAddress(1, 1, 5, 5));

                    SetExcelCell(sheet, 4, 0, styleHead12, "電子發文上傳日：");
                    sheet.AddMergedRegion(new CellRangeAddress(4, 4, 0, 0));
                    SetExcelCell(sheet, 4, 1, styleHead12, model.SendUpDateStart + '~' + model.SendUpDateEnd);
                    sheet.AddMergedRegion(new CellRangeAddress(4, 4, 1, 2));

                    SetExcelCell(sheet, 5, 0, styleHead12, "發文方式：");
                    sheet.AddMergedRegion(new CellRangeAddress(5, 5, 0, 0));
                    SetExcelCell(sheet, 5, 1, styleHead12, model.SendKind);
                    sheet.AddMergedRegion(new CellRangeAddress(5, 5, 1, 2));

                    //* line2
                    SetExcelCell(sheet, 6, 0, styleHead10, "案件類別-大類");
                    sheet.AddMergedRegion(new CellRangeAddress(4, 4, 0, 0));
                    SetExcelCell(sheet, 6, 1, styleHead10, "經辦人員");
                    sheet.AddMergedRegion(new CellRangeAddress(4, 4, 1, 1));
                    SetExcelCell(sheet, 6, 2, styleHead10, "件數");
                    sheet.AddMergedRegion(new CellRangeAddress(4, 4, 2, 2));
                    SetExcelCell(sheet, 6, 3, styleHead10, "退件件數");
                    sheet.AddMergedRegion(new CellRangeAddress(4, 4, 3, 3));
                    SetExcelCell(sheet, 6, 4, styleHead10, "退件率");
                    sheet.AddMergedRegion(new CellRangeAddress(4, 4, 4, 4));
                    SetExcelCell(sheet, 6, 5, styleHead10, "逾期件數");
                    sheet.AddMergedRegion(new CellRangeAddress(4, 4, 5, 5));
                    SetExcelCell(sheet, 6, 6, styleHead10, "逾期率");
                    sheet.AddMergedRegion(new CellRangeAddress(4, 4, 6, 6));

                    SetExcelCell(sheet, dt.Rows.Count + 7, 1, styleHead12, "合計");
                    sheet.AddMergedRegion(new CellRangeAddress(dt.Rows.Count + 7, dt.Rows.Count + 7, 1, 1));

                    for (int i = 0; i < dt.Rows.Count + 1; i++)//*初始表格賦初值 
                    {
                       for (int j = 1; j < 6; j++)
                       {
                          SetExcelCell(sheet, i + 7, j + 1, styleHead12, "0");
                          sheet.AddMergedRegion(new CellRangeAddress(i + 7, i + 7, j + 1, j + 1));
                       }
                    }
                    #endregion
                    #region Width
                    sheet.SetColumnWidth(0, 100 * 40);
                    sheet.SetColumnWidth(1, 100 * 40);
                    sheet.SetColumnWidth(2, 100 * 40);
                    sheet.SetColumnWidth(3, 100 * 40);
                    sheet.SetColumnWidth(4, 100 * 40);
                    sheet.SetColumnWidth(5, 100 * 40);
                    sheet.SetColumnWidth(6, 100 * 40);
                    #endregion
                    #region body
                    int totalNum = 0;
                    int totalNumCount = 0;
                    int totalReturnNum = 0;
                    int totalReturnNumCount = 0;
                    int totalOverDue = 0;
                    int totalOverDueCount = 0;
                    for (int iRow = 0; iRow < dt.Rows.Count; iRow++)
                    {
                       for (int iCol = 0; iCol < dt.Columns.Count; iCol++)
                       {
                          SetExcelCell(sheet, iRow + 7, iCol, styleHead10, dt.Rows[iRow][iCol].ToString());
                       }
                       totalNum = Convert.ToInt32(dt.Rows[iRow]["Case"].ToString());
                       totalNumCount += totalNum;
                       totalReturnNum = Convert.ToInt32(dt.Rows[iRow]["ReturnCase"].ToString());
                       totalReturnNumCount += totalReturnNum;
                       totalOverDue = Convert.ToInt32(dt.Rows[iRow]["OutCase"].ToString());
                       totalOverDueCount += totalOverDue;
                    }
                    SetExcelCell(sheet, dt.Rows.Count + 7, 2, styleHead12, totalNumCount.ToString());
                    SetExcelCell(sheet, dt.Rows.Count + 7, 3, styleHead12, totalReturnNumCount.ToString());
                    if (totalReturnNumCount == 0)
                    {
                       SetExcelCell(sheet, dt.Rows.Count + 7, 4, styleHead12, "0.00%");
                    }
                    else
                    {
                       string result = (((totalReturnNumCount * 1.0) / totalNumCount) * 100).ToString();
                       SetExcelCell(sheet, dt.Rows.Count + 7, 4, styleHead12, StringHelper.getFarmatNums(result) + "%");
                    }
                    SetExcelCell(sheet, dt.Rows.Count + 7, 5, styleHead12, totalOverDueCount.ToString());
                    if (totalOverDueCount == 0)
                    {
                       SetExcelCell(sheet, dt.Rows.Count + 7, 6, styleHead12, "0.00%");
                    }
                    else
                    {
                       string results = (((totalOverDueCount * 1.0) / totalNumCount) * 100).ToString();
                       SetExcelCell(sheet, dt.Rows.Count + 7, 6, styleHead12, StringHelper.getFarmatNums(results) + "%");
                    }
                    #endregion
                 }
              }
           }
           else
           {
              for (int k = 0; k < Departmentcount; k++)
              {
                 ISheet sheet = null;
                 DataTable dt = new DataTable();

                 sheet = workbook.CreateSheet(listCode[k].CodeDesc);
                 dt = GetTradeList1(model, listCode[k].CodeDesc);//獲取查詢科別的案件
                 SetExcelCell(sheet, 1, 6, styleHead12, listCode[k].CodeDesc);
                 sheet.AddMergedRegion(new CellRangeAddress(1, 1, 6, 6));

                 #region title
                 SetExcelCell(sheet, 0, 0, styleHead12, "品質時效統計表");
                 sheet.AddMergedRegion(new CellRangeAddress(0, 0, 0, 6));

                 //* line1
                 SetExcelCell(sheet, 1, 0, styleHead12, "收件日期：");
                 sheet.AddMergedRegion(new CellRangeAddress(1, 1, 0, 0));
                 SetExcelCell(sheet, 1, 1, styleHead12, model.ReceiveDateStart + '~' + model.ReceiveDateEnd);
                 sheet.AddMergedRegion(new CellRangeAddress(1, 1, 1, 2));

                 SetExcelCell(sheet, 2, 0, styleHead12, "發文日期：");
                 sheet.AddMergedRegion(new CellRangeAddress(2, 2, 0, 0));
                 SetExcelCell(sheet, 2, 1, styleHead12, model.SendDateStart + '~' + model.SendDateEnd);
                 sheet.AddMergedRegion(new CellRangeAddress(2, 2, 1, 2));

                 SetExcelCell(sheet, 3, 0, styleHead12, "主管放行日：");
                 sheet.AddMergedRegion(new CellRangeAddress(3, 3, 0, 0));
                 SetExcelCell(sheet, 3, 1, styleHead12, model.ApproveDateStart + '~' + model.ApproveDateEnd);
                 sheet.AddMergedRegion(new CellRangeAddress(3, 3, 1, 2));
                 SetExcelCell(sheet, 1, 5, styleHead12, "部門別/科別:");
                 sheet.AddMergedRegion(new CellRangeAddress(1, 1, 5, 5));

                 SetExcelCell(sheet, 4, 0, styleHead12, "電子發文上傳日：");
                 sheet.AddMergedRegion(new CellRangeAddress(4, 4, 0, 0));
                 SetExcelCell(sheet, 4, 1, styleHead12, model.SendUpDateStart + '~' + model.SendUpDateEnd);
                 sheet.AddMergedRegion(new CellRangeAddress(4, 4, 1, 2));

                 SetExcelCell(sheet, 5, 0, styleHead12, "發文方式：");
                 sheet.AddMergedRegion(new CellRangeAddress(5, 5, 0, 0));
                 SetExcelCell(sheet, 5, 1, styleHead12, model.SendKind);
                 sheet.AddMergedRegion(new CellRangeAddress(5, 5, 1, 2));

                 //* line2
                 SetExcelCell(sheet, 6, 0, styleHead10, "案件類別-大類");
                 sheet.AddMergedRegion(new CellRangeAddress(4, 4, 0, 0));
                 SetExcelCell(sheet, 6, 1, styleHead10, "經辦人員");
                 sheet.AddMergedRegion(new CellRangeAddress(4, 4, 1, 1));
                 SetExcelCell(sheet, 6, 2, styleHead10, "件數");
                 sheet.AddMergedRegion(new CellRangeAddress(4, 4, 2, 2));
                 SetExcelCell(sheet, 6, 3, styleHead10, "退件件數");
                 sheet.AddMergedRegion(new CellRangeAddress(4, 4, 3, 3));
                 SetExcelCell(sheet, 6, 4, styleHead10, "退件率");
                 sheet.AddMergedRegion(new CellRangeAddress(4, 4, 4, 4));
                 SetExcelCell(sheet, 6, 5, styleHead10, "逾期件數");
                 sheet.AddMergedRegion(new CellRangeAddress(4, 4, 5, 5));
                 SetExcelCell(sheet, 6, 6, styleHead10, "逾期率");
                 sheet.AddMergedRegion(new CellRangeAddress(4, 4, 6, 6));

                 SetExcelCell(sheet, dt.Rows.Count + 7, 1, styleHead12, "合計");
                 sheet.AddMergedRegion(new CellRangeAddress(dt.Rows.Count + 7, dt.Rows.Count + 7, 1, 1));

                 for (int i = 0; i < dt.Rows.Count + 1; i++)//*初始表格賦初值 
                 {
                    for (int j = 1; j < 6; j++)
                    {
                       SetExcelCell(sheet, i + 7, j + 1, styleHead12, "0");
                       sheet.AddMergedRegion(new CellRangeAddress(i + 7, i + 7, j + 1, j + 1));
                    }
                 }
                 #endregion
                 #region Width
                 sheet.SetColumnWidth(0, 100 * 40);
                 sheet.SetColumnWidth(1, 100 * 40);
                 sheet.SetColumnWidth(2, 100 * 40);
                 sheet.SetColumnWidth(3, 100 * 40);
                 sheet.SetColumnWidth(4, 100 * 40);
                 sheet.SetColumnWidth(5, 100 * 40);
                 sheet.SetColumnWidth(6, 100 * 40);
                 #endregion
                 #region body
                 int totalNum = 0;
                 int totalNumCount = 0;
                 int totalReturnNum = 0;
                 int totalReturnNumCount = 0;
                 int totalOverDue = 0;
                 int totalOverDueCount = 0;
                 for (int iRow = 0; iRow < dt.Rows.Count; iRow++)
                 {
                    for (int iCol = 0; iCol < dt.Columns.Count; iCol++)
                    {
                       SetExcelCell(sheet, iRow + 7, iCol, styleHead10, dt.Rows[iRow][iCol].ToString());
                    }
                    totalNum = Convert.ToInt32(dt.Rows[iRow]["Case"].ToString());
                    totalNumCount += totalNum;
                    totalReturnNum = Convert.ToInt32(dt.Rows[iRow]["ReturnCase"].ToString());
                    totalReturnNumCount += totalReturnNum;
                    totalOverDue = Convert.ToInt32(dt.Rows[iRow]["OutCase"].ToString());
                    totalOverDueCount += totalOverDue;
                 }
                 SetExcelCell(sheet, dt.Rows.Count + 7, 2, styleHead12, totalNumCount.ToString());
                 SetExcelCell(sheet, dt.Rows.Count + 7, 3, styleHead12, totalReturnNumCount.ToString());
                 if (totalReturnNumCount == 0)
                 {
                    SetExcelCell(sheet, dt.Rows.Count + 7, 4, styleHead12, "0.00%");
                 }
                 else
                 {
                    string result = (((totalReturnNumCount * 1.0) / totalNumCount) * 100).ToString();
                    SetExcelCell(sheet, dt.Rows.Count + 7, 4, styleHead12, StringHelper.getFarmatNums(result) + "%");
                 }
                 SetExcelCell(sheet, dt.Rows.Count + 7, 5, styleHead12, totalOverDueCount.ToString());
                 if (totalOverDueCount == 0)
                 {
                    SetExcelCell(sheet, dt.Rows.Count + 7, 6, styleHead12, "0.00%");
                 }
                 else
                 {
                    string results = (((totalOverDueCount * 1.0) / totalNumCount) * 100).ToString();
                    SetExcelCell(sheet, dt.Rows.Count + 7, 6, styleHead12, StringHelper.getFarmatNums(results) + "%");
                 }
                 #endregion
              }
           }

           MemoryStream ms = new MemoryStream();
           workbook.Write(ms);
           ms.Flush();
           ms.Position = 0;
           workbook = null;
           return ms;
        }

        #endregion
        //20170811 RC RQ-2015-019666-020 派件至跨單位(原產生邏輯程式寫死，組織異動必出問題，改為參數) 宏祥 add end

        //#region 主管放行統計表1
        //public MemoryStream ApproveReportExcel_NPOI1(CaseClosedQuery model)
        //{
        //    IWorkbook workbook = new HSSFWorkbook();
        //    ISheet sheet = null;
        //    ISheet sheet2 = null;
        //    ISheet sheet3 = null;
        //    Dictionary<string, string> dicldapList = new Dictionary<string, string>();
        //    Dictionary<string, string> dicldapList2 = new Dictionary<string, string>();
        //    Dictionary<string, string> dicldapList3 = new Dictionary<string, string>();
        //    DataTable dtCase = new DataTable();//導出資料
        //    DataTable dtCase2 = new DataTable();
        //    DataTable dtCase3 = new DataTable();

        //    int rowscountExcelresult = 0;//合計參數
        //    string caseExcel = "";//案件類型
        //    int rowsExcel = 6;//行數
        //    int rowscountExcel = 0;//最後一列合計
        //    int rowstatolExcel = 0;//總合計 
        //    int sort = 1;

        //    #region def style
        //    ICellStyle styleHead12 = workbook.CreateCellStyle();
        //    IFont font12 = workbook.CreateFont();
        //    font12.FontHeightInPoints = 12;
        //    font12.FontName = "新細明體";
        //    styleHead12.FillPattern = FillPattern.SolidForeground;
        //    styleHead12.FillForegroundColor = HSSFColor.White.Index;
        //    styleHead12.BorderTop = BorderStyle.None;
        //    styleHead12.BorderLeft = BorderStyle.None;
        //    styleHead12.BorderRight = BorderStyle.None;
        //    styleHead12.BorderBottom = BorderStyle.None;
        //    styleHead12.WrapText = true;
        //    styleHead12.Alignment = HorizontalAlignment.Center;
        //    styleHead12.VerticalAlignment = VerticalAlignment.Center;
        //    styleHead12.SetFont(font12);

        //    ICellStyle styleHead10 = workbook.CreateCellStyle();
        //    IFont font10 = workbook.CreateFont();
        //    font10.FontHeightInPoints = 10;
        //    font10.FontName = "新細明體";
        //    styleHead10.FillPattern = FillPattern.SolidForeground;
        //    styleHead10.FillForegroundColor = HSSFColor.White.Index;
        //    styleHead10.BorderTop = BorderStyle.Thin;
        //    styleHead10.BorderLeft = BorderStyle.Thin;
        //    styleHead10.BorderRight = BorderStyle.Thin;
        //    styleHead10.BorderBottom = BorderStyle.Thin;
        //    styleHead10.WrapText = true;
        //    styleHead10.Alignment = HorizontalAlignment.Left;
        //    styleHead10.VerticalAlignment = VerticalAlignment.Center;
        //    styleHead10.SetFont(font10);
        //    #endregion

        //    #region 單獨科別的數據源(科別及案件資料)
        //    //獲取人員
        //    if (model.Depart == "1" || model.Depart == "0")//* 集作一科
        //    {
        //        sheet = workbook.CreateSheet("集作一科");
        //        dtCase = GetApproveList1(model, "集作一科");
        //        //判斷人員
        //        foreach (DataRow dr in dtCase.Rows)
        //        {
        //            if (!dicldapList.Keys.Contains(dr["ApproveUser"].ToString()))
        //            {
        //                dicldapList.Add(dr["ApproveUser"].ToString(), dr["EmpName"].ToString() + "|" + sort);
        //                sort++;
        //            }
        //        }

        //        if (dicldapList.Count > 2)
        //        {
        //            SetExcelCell(sheet, 1, dicldapList.Count + 1, styleHead12, "集作一科");
        //            sheet.AddMergedRegion(new CellRangeAddress(1, 1, dicldapList.Count + 1, dicldapList.Count + 1));
        //        }
        //        else
        //        {
        //            SetExcelCell(sheet, 1, 4, styleHead12, "集作一科");
        //            sheet.AddMergedRegion(new CellRangeAddress(1, 1, 4, 4));
        //        }
        //    }
        //    if (model.Depart == "2")//* 集作二科
        //    {
        //        sheet = workbook.CreateSheet("集作二科");
        //        dtCase = GetApproveList1(model, "集作二科");
        //        sort = 1;
        //        //判斷人員
        //        foreach (DataRow dr in dtCase.Rows)
        //        {
        //            if (!dicldapList.Keys.Contains(dr["ApproveUser"].ToString()))
        //            {
        //                dicldapList.Add(dr["ApproveUser"].ToString(), dr["EmpName"].ToString() + "|" + sort);
        //                sort++;
        //            }
        //        }

        //        if (dicldapList.Count > 2)
        //        {
        //            SetExcelCell(sheet, 1, dicldapList.Count + 1, styleHead12, "集作二科");
        //            sheet.AddMergedRegion(new CellRangeAddress(1, 1, dicldapList.Count + 1, dicldapList.Count + 1));
        //        }
        //        else
        //        {
        //            SetExcelCell(sheet, 1, 4, styleHead12, "集作二科");
        //            sheet.AddMergedRegion(new CellRangeAddress(1, 1, 4, 4));
        //        }
        //    }
        //    if (model.Depart == "3")//* 集作三科
        //    {
        //        sheet = workbook.CreateSheet("集作三科");
        //        dtCase = GetApproveList1(model, "集作三科");
        //        sort = 1;
        //        //判斷人員
        //        foreach (DataRow dr in dtCase.Rows)
        //        {
        //            if (!dicldapList.Keys.Contains(dr["ApproveUser"].ToString()))
        //            {
        //                dicldapList.Add(dr["ApproveUser"].ToString(), dr["EmpName"].ToString() + "|" + sort);
        //                sort++;
        //            }
        //        }

        //        if (dicldapList.Count > 2)
        //        {
        //            SetExcelCell(sheet, 1, dicldapList.Count + 1, styleHead12, "集作三科");
        //            sheet.AddMergedRegion(new CellRangeAddress(1, 1, dicldapList.Count + 1, dicldapList.Count + 1));
        //        }
        //        else
        //        {
        //            SetExcelCell(sheet, 1, 4, styleHead12, "集作三科");
        //            sheet.AddMergedRegion(new CellRangeAddress(1, 1, 4, 4));
        //        }
        //    }
        //    if (model.Depart == "0")//* 全部
        //    {
        //        sheet2 = workbook.CreateSheet("集作二科");
        //        sheet3 = workbook.CreateSheet("集作三科");
        //        dtCase2 = GetApproveList1(model, "集作二科");//獲取查詢集作二科的案件
        //        dtCase3 = GetApproveList1(model, "集作三科");//獲取查詢集作三科的案件
        //        sort = 1;
        //        //判斷集作二科人員
        //        foreach (DataRow dr in dtCase2.Rows)
        //        {
        //            if (!dicldapList2.Keys.Contains(dr["ApproveUser"].ToString()))
        //            {
        //                dicldapList2.Add(dr["ApproveUser"].ToString(), dr["EmpName"].ToString() + "|" + sort);
        //                sort++;
        //            }
        //        }
        //        sort = 1;
        //        //判斷集作三科人員
        //        foreach (DataRow dr in dtCase3.Rows)
        //        {
        //            if (!dicldapList3.Keys.Contains(dr["ApproveUser"].ToString()))
        //            {
        //                dicldapList3.Add(dr["ApproveUser"].ToString(), dr["EmpName"].ToString() + "|" + sort);
        //                sort++;
        //            }
        //        }
        //    }
        //    #endregion

        //    string caseKind = "";//*去重複
        //    int rows = 6;//定義行數

        //    #region title
        //    //*大標題 line0
        //    SetExcelCell(sheet, 0, 0, styleHead12, "主管放行統計表");
        //    sheet.AddMergedRegion(new CellRangeAddress(0, 0, 0, dicldapList.Count + 1));

        //    //*查詢條件 line1
        //    SetExcelCell(sheet, 1, 0, styleHead12, "收件日期：");
        //    sheet.AddMergedRegion(new CellRangeAddress(1, 1, 0, 0));
        //    SetExcelCell(sheet, 1, 1, styleHead12, model.ReceiveDateStart + '~' + model.ReceiveDateEnd);
        //    sheet.AddMergedRegion(new CellRangeAddress(1, 1, 1, 2));


        //    SetExcelCell(sheet, 2, 0, styleHead12, "發文日期：");
        //    sheet.AddMergedRegion(new CellRangeAddress(2, 2, 0, 0));
        //    SetExcelCell(sheet, 2, 1, styleHead12, model.SendDateStart + '~' + model.SendDateEnd);
        //    sheet.AddMergedRegion(new CellRangeAddress(2, 2, 1, 2));

        //    SetExcelCell(sheet, 3, 0, styleHead12, "主管放行日：");
        //    sheet.AddMergedRegion(new CellRangeAddress(3, 3, 0, 0));
        //    SetExcelCell(sheet, 3, 1, styleHead12, model.ApproveDateStart + '~' + model.ApproveDateEnd);
        //    sheet.AddMergedRegion(new CellRangeAddress(3, 3, 1, 2));

        //    SetExcelCell(sheet, 4, 0, styleHead12, "電子發文上傳日：");
        //    sheet.AddMergedRegion(new CellRangeAddress(4, 4, 0, 0));
        //    SetExcelCell(sheet, 4, 1, styleHead12, model.SendUpDateStart + '~' + model.SendUpDateEnd);
        //    sheet.AddMergedRegion(new CellRangeAddress(4, 4, 1, 2));

        //    SetExcelCell(sheet, 5, 0, styleHead12, "發文方式：");
        //    sheet.AddMergedRegion(new CellRangeAddress(5, 5, 0, 0));
        //    SetExcelCell(sheet, 5, 1, styleHead12, model.SendKind);
        //    sheet.AddMergedRegion(new CellRangeAddress(5, 5, 1, 2));

        //    if (dicldapList.Count > 2)
        //    {
        //        SetExcelCell(sheet, 1, dicldapList.Count, styleHead12, "部門別/科別");
        //        sheet.AddMergedRegion(new CellRangeAddress(1, 1, dicldapList.Count, dicldapList.Count));
        //    }
        //    else
        //    {
        //        SetExcelCell(sheet, 1, 3, styleHead12, "部門別/科別");
        //        sheet.AddMergedRegion(new CellRangeAddress(1, 1, 3, 3));
        //        sheet.SetColumnWidth(3, 100 * 30);
        //    }

        //    //*結果集表頭 line2
        //    SetExcelCell(sheet, 6, 0, styleHead10, "處理人員");
        //    sheet.AddMergedRegion(new CellRangeAddress(6, 6, 0, 0));
        //    sheet.SetColumnWidth(0, 100 * 50);

        //    //依次排列人員名稱
        //    int a = 1;
        //    foreach (var item in dicldapList)
        //    {
        //        SetExcelCell(sheet, 6, a, styleHead10, item.Value.Split('|')[0]);
        //        sheet.AddMergedRegion(new CellRangeAddress(6, 6, a, a));
        //        a++;
        //    }

        //    SetExcelCell(sheet, 6, dicldapList.Count + 1, styleHead10, "合計");
        //    sheet.AddMergedRegion(new CellRangeAddress(6, 6, dicldapList.Count + 1, dicldapList.Count + 1));

        //    //*扣押案件類型 line5-lineN 
        //    for (int i = 0; i < dtCase.Rows.Count; i++)
        //    {
        //        if (caseKind != dtCase.Rows[i]["New_CaseKind"].ToString())
        //        {
        //            rows = rows + 1;
        //            SetExcelCell(sheet, rows, 0, styleHead10, dtCase.Rows[i]["New_CaseKind"].ToString());
        //            sheet.AddMergedRegion(new CellRangeAddress(rows, rows, 0, 0));
        //            SetExcelCell(sheet, rows, dicldapList.Count + 1, styleHead10, "0");
        //            sheet.AddMergedRegion(new CellRangeAddress(rows, rows, dicldapList.Count + 1, dicldapList.Count + 1));
        //            caseKind = dtCase.Rows[i]["New_CaseKind"].ToString();
        //            for (int j = 0; j < dicldapList.Count; j++)//*初始表格賦初值 
        //            {
        //                SetExcelCell(sheet, rows, j + 1, styleHead10, "0");
        //                sheet.AddMergedRegion(new CellRangeAddress(rows, rows, j + 1, j + 1));
        //            }
        //        }
        //    }

        //    //*合計 lineLast
        //    SetExcelCell(sheet, rows + 1, 0, styleHead10, "合計");
        //    sheet.AddMergedRegion(new CellRangeAddress(rows + 1, rows + 1, 0, 0));

        //    for (int j = 0; j < dicldapList.Count; j++)//*初始表格賦初值 
        //    {
        //        SetExcelCell(sheet, rows + 1, j + 1, styleHead10, "0");
        //        sheet.AddMergedRegion(new CellRangeAddress(rows + 1, rows + 1, j + 1, j + 1));
        //    }
        //    #endregion

        //    #region  body
        //    for (int iRow = 0; iRow < dtCase.Rows.Count; iRow++)//根據案件類型進行循環
        //    {
        //        foreach (var item in dicldapList)
        //        {
        //            int irows = Convert.ToInt32(item.Value.Split('|')[1]);
        //            if (caseExcel == dtCase.Rows[iRow]["New_CaseKind"].ToString())//重複同一案件類型的數據
        //            {
        //                if (item.Key == dtCase.Rows[iRow]["ApproveUser"].ToString())
        //                {
        //                    SetExcelCell(sheet, rowsExcel, irows, styleHead10, dtCase.Rows[iRow]["case_num"].ToString());
        //                    rowscountExcelresult = Convert.ToInt32(dtCase.Rows[iRow]["case_num"].ToString());//每格資料
        //                    SetExcelCell(sheet, rows + 1, irows, styleHead10, dtCase.Rows[iRow]["UserCount"].ToString());//最後一行合計
        //                    rowscountExcel += rowscountExcelresult;
        //                    rowstatolExcel += rowscountExcelresult;
        //                }
        //                SetExcelCell(sheet, rowsExcel, dicldapList.Count + 1, styleHead10, rowscountExcel.ToString());//最後一列合計
        //            }
        //            else//不重複的案件類型
        //            {
        //                rowscountExcel = 0;
        //                rowsExcel = rowsExcel + 1;
        //                if (dtCase.Rows[iRow]["ApproveUser"].ToString() == item.Key)
        //                {
        //                    SetExcelCell(sheet, rowsExcel, irows, styleHead10, dtCase.Rows[iRow]["case_num"].ToString());
        //                    rowscountExcelresult = Convert.ToInt32(dtCase.Rows[iRow]["case_num"].ToString());//第一條不重複的數據儲存下值
        //                    SetExcelCell(sheet, rows + 1, irows, styleHead10, dtCase.Rows[iRow]["UserCount"].ToString());//最後一行合計
        //                    rowscountExcel += rowscountExcelresult;
        //                    rowstatolExcel += rowscountExcelresult;
        //                }
        //                caseExcel = dtCase.Rows[iRow]["New_CaseKind"].ToString();
        //                SetExcelCell(sheet, rowsExcel, dicldapList.Count + 1, styleHead10, rowscountExcel.ToString());//最後一列合計
        //            }
        //        }
        //    }
        //    SetExcelCell(sheet, rows + 1, dicldapList.Count + 1, styleHead10, rowstatolExcel.ToString());//總合計
        //    #endregion

        //    if (model.Depart == "0")//* 全部
        //    {
        //        #region title2
        //        //*大標題 line0
        //        SetExcelCell(sheet2, 0, 0, styleHead12, "主管放行統計表");
        //        sheet2.AddMergedRegion(new CellRangeAddress(0, 0, 0, dicldapList2.Count + 1));

        //        //*查詢條件 line1
        //        SetExcelCell(sheet2, 1, 0, styleHead12, "收件日期：");
        //        sheet2.AddMergedRegion(new CellRangeAddress(1, 1, 0, 0));
        //        SetExcelCell(sheet2, 1, 1, styleHead12, model.ReceiveDateStart + '~' + model.ReceiveDateEnd);
        //        sheet2.AddMergedRegion(new CellRangeAddress(1, 1, 1, 2));

        //        SetExcelCell(sheet2, 2, 0, styleHead12, "發文日期：");
        //        sheet2.AddMergedRegion(new CellRangeAddress(2, 2, 0, 0));
        //        SetExcelCell(sheet2, 2, 1, styleHead12, model.SendDateStart + '~' + model.SendDateEnd);
        //        sheet2.AddMergedRegion(new CellRangeAddress(2, 2, 1, 2));

        //        SetExcelCell(sheet2, 3, 0, styleHead12, "主管放行日：");
        //        sheet2.AddMergedRegion(new CellRangeAddress(3, 3, 0, 0));
        //        SetExcelCell(sheet2, 3, 1, styleHead12, model.ApproveDateStart + '~' + model.ApproveDateEnd);
        //        sheet2.AddMergedRegion(new CellRangeAddress(3, 3, 1, 2));

        //        SetExcelCell(sheet2, 4, 0, styleHead12, "電子發文上傳日：");
        //        sheet2.AddMergedRegion(new CellRangeAddress(4, 4, 0, 0));
        //        SetExcelCell(sheet2, 4, 1, styleHead12, model.SendUpDateStart + '~' + model.SendUpDateEnd);
        //        sheet2.AddMergedRegion(new CellRangeAddress(4, 4, 1, 2));

        //        SetExcelCell(sheet2, 5, 0, styleHead12, "發文方式：");
        //        sheet2.AddMergedRegion(new CellRangeAddress(5, 5, 0, 0));
        //        SetExcelCell(sheet2, 5, 1, styleHead12, model.SendKind);
        //        sheet2.AddMergedRegion(new CellRangeAddress(5, 5, 1, 2));

        //        if (dicldapList2.Count > 2)
        //        {
        //            SetExcelCell(sheet2, 1, dicldapList2.Count - 1, styleHead12, "部門別/科別");
        //            sheet2.AddMergedRegion(new CellRangeAddress(1, 1, dicldapList2.Count - 1, dicldapList2.Count));
        //            SetExcelCell(sheet2, 1, dicldapList2.Count + 1, styleHead12, "集作二科");
        //            sheet2.AddMergedRegion(new CellRangeAddress(1, 1, dicldapList2.Count + 1, dicldapList2.Count + 1));
        //        }
        //        else
        //        {
        //            SetExcelCell(sheet2, 1, 3, styleHead12, "部門別/科別");
        //            sheet2.AddMergedRegion(new CellRangeAddress(1, 1, 3, 3));
        //            SetExcelCell(sheet2, 1, 4, styleHead12, "集作二科");
        //            sheet2.AddMergedRegion(new CellRangeAddress(1, 1, 4, 4));
        //        }

        //        //*結果集表頭 line4
        //        SetExcelCell(sheet2, 6, 0, styleHead10, "處理人員");
        //        sheet2.AddMergedRegion(new CellRangeAddress(6, 6, 0, 0));
        //        sheet2.SetColumnWidth(0, 100 * 50);

        //        //依次排列人員名稱
        //        a = 1;
        //        foreach (var item in dicldapList2)
        //        {
        //            SetExcelCell(sheet2, 6, a, styleHead10, item.Value.Split('|')[0]);
        //            sheet2.AddMergedRegion(new CellRangeAddress(6, 6, a, a));
        //            a++;
        //        }

        //        SetExcelCell(sheet2, 6, dicldapList2.Count + 1, styleHead10, "合計");
        //        sheet2.AddMergedRegion(new CellRangeAddress(6, 6, dicldapList2.Count + 1, dicldapList2.Count + 1));

        //        //*扣押案件類型 line3-lineN 
        //        caseKind = "";//*去重複
        //        rows = 6;//定義行數
        //        for (int i = 0; i < dtCase2.Rows.Count; i++)
        //        {
        //            if (caseKind != dtCase2.Rows[i]["New_CaseKind"].ToString())
        //            {
        //                rows = rows + 1;
        //                SetExcelCell(sheet2, rows, 0, styleHead10, dtCase2.Rows[i]["New_CaseKind"].ToString());
        //                sheet2.AddMergedRegion(new CellRangeAddress(rows, rows, 0, 0));

        //                SetExcelCell(sheet2, rows, dicldapList2.Count + 1, styleHead10, "0");
        //                sheet2.AddMergedRegion(new CellRangeAddress(rows, rows, dicldapList2.Count + 1, dicldapList2.Count + 1));

        //                caseKind = dtCase2.Rows[i]["New_CaseKind"].ToString();
        //                for (int j = 0; j < dicldapList2.Count; j++)//*初始表格賦初值 
        //                {
        //                    SetExcelCell(sheet2, rows, j + 1, styleHead10, "0");
        //                    sheet2.AddMergedRegion(new CellRangeAddress(rows, rows, j + 1, j + 1));
        //                }
        //            }
        //        }

        //        //*合計 lineLast
        //        SetExcelCell(sheet2, rows + 1, 0, styleHead10, "合計");
        //        sheet2.AddMergedRegion(new CellRangeAddress(rows + 1, rows + 1, 0, 0));
        //        for (int j = 0; j < dicldapList2.Count; j++)//*初始表格賦初值 
        //        {
        //            SetExcelCell(sheet2, rows + 1, j + 1, styleHead10, "0");
        //            sheet2.AddMergedRegion(new CellRangeAddress(rows + 1, rows + 1, j + 1, j + 1));
        //        }
        //        #endregion
        //        #region body2
        //        caseExcel = "";//案件類型
        //        rowsExcel = 6;//行數
        //        rowscountExcel = 0;//最後一列合計
        //        rowstatolExcel = 0;//總合計      
        //        for (int iRow = 0; iRow < dtCase2.Rows.Count; iRow++)//根據案件類型進行循環
        //        {
        //            foreach (var item in dicldapList2)
        //            {
        //                int irows2 = Convert.ToInt32(item.Value.Split('|')[1]);
        //                if (dtCase2.Rows[iRow]["New_CaseKind"].ToString() == caseExcel)//重複同一案件類型的數據
        //                {
        //                    if (dtCase2.Rows[iRow]["ApproveUser"].ToString() == item.Key)
        //                    {
        //                        SetExcelCell(sheet2, rowsExcel, irows2, styleHead10, dtCase2.Rows[iRow]["case_num"].ToString());
        //                        SetExcelCell(sheet2, rows + 1, irows2, styleHead10, dtCase2.Rows[iRow]["UserCount"].ToString());//最後一行合計
        //                        rowscountExcelresult = Convert.ToInt32(dtCase2.Rows[iRow]["case_num"].ToString());//每格資料
        //                        rowscountExcel += rowscountExcelresult;
        //                        rowstatolExcel += rowscountExcelresult;
        //                    }
        //                    SetExcelCell(sheet2, rowsExcel, dicldapList2.Count + 1, styleHead10, rowscountExcel.ToString());//最後一列合計
        //                }
        //                else//不重複的案件類型
        //                {
        //                    rowscountExcel = 0;
        //                    rowsExcel = rowsExcel + 1;
        //                    if (dtCase2.Rows[iRow]["ApproveUser"].ToString() == item.Key)
        //                    {
        //                        SetExcelCell(sheet2, rowsExcel, irows2, styleHead10, dtCase2.Rows[iRow]["case_num"].ToString());
        //                        SetExcelCell(sheet2, rows + 1, irows2, styleHead10, dtCase2.Rows[iRow]["UserCount"].ToString());//最後一行合計
        //                        rowscountExcelresult = Convert.ToInt32(dtCase2.Rows[iRow]["case_num"].ToString());//第一條不重複的數據儲存下值
        //                        rowscountExcel += rowscountExcelresult;
        //                        rowstatolExcel += rowscountExcelresult;
        //                    }
        //                    caseExcel = dtCase2.Rows[iRow]["New_CaseKind"].ToString();
        //                    SetExcelCell(sheet2, rowsExcel, dicldapList2.Count + 1, styleHead10, rowscountExcel.ToString());//最後一列合計      
        //                }
        //            }
        //        }
        //        SetExcelCell(sheet2, rows + 1, dicldapList2.Count + 1, styleHead10, rowstatolExcel.ToString());//總合計
        //        #endregion

        //        #region title3
        //        //*大標題 line0
        //        SetExcelCell(sheet3, 0, 0, styleHead12, "主管放行統計表");
        //        sheet3.AddMergedRegion(new CellRangeAddress(0, 0, 0, dicldapList3.Count + 1));

        //        //*查詢條件 line1
        //        SetExcelCell(sheet3, 1, 0, styleHead12, "收件日期：");
        //        sheet3.AddMergedRegion(new CellRangeAddress(1, 1, 0, 0));
        //        SetExcelCell(sheet3, 1, 1, styleHead12, model.ReceiveDateStart + '~' + model.ReceiveDateEnd);
        //        sheet3.AddMergedRegion(new CellRangeAddress(1, 1, 1, 2));

        //        SetExcelCell(sheet3, 2, 0, styleHead12, "發文日期：");
        //        sheet3.AddMergedRegion(new CellRangeAddress(2, 2, 0, 0));
        //        SetExcelCell(sheet3, 2, 1, styleHead12, model.SendDateStart + '~' + model.SendDateEnd);
        //        sheet3.AddMergedRegion(new CellRangeAddress(2, 2, 1, 2));

        //        SetExcelCell(sheet3, 3, 0, styleHead12, "主管放行日：");
        //        sheet3.AddMergedRegion(new CellRangeAddress(3, 3, 0, 0));
        //        SetExcelCell(sheet3, 3, 1, styleHead12, model.ApproveDateStart + '~' + model.ApproveDateEnd);
        //        sheet3.AddMergedRegion(new CellRangeAddress(3, 3, 1, 2));

        //        SetExcelCell(sheet3, 4, 0, styleHead12, "電子發文上傳日：");
        //        sheet3.AddMergedRegion(new CellRangeAddress(4, 4, 0, 0));
        //        SetExcelCell(sheet3, 4, 1, styleHead12, model.SendUpDateStart + '~' + model.SendUpDateEnd);
        //        sheet3.AddMergedRegion(new CellRangeAddress(4, 4, 1, 2));

        //        SetExcelCell(sheet3, 5, 0, styleHead12, "發文方式：");
        //        sheet3.AddMergedRegion(new CellRangeAddress(5, 5, 0, 0));
        //        SetExcelCell(sheet3, 5, 1, styleHead12, model.SendKind);
        //        sheet3.AddMergedRegion(new CellRangeAddress(5, 5, 1, 2));

        //        if (dicldapList3.Count > 2)
        //        {
        //            SetExcelCell(sheet3, 1, dicldapList3.Count - 1, styleHead12, "部門別/科別");
        //            sheet3.AddMergedRegion(new CellRangeAddress(1, 1, dicldapList3.Count - 1, dicldapList3.Count));
        //            SetExcelCell(sheet3, 1, dicldapList3.Count + 1, styleHead12, "集作三科");
        //            sheet3.AddMergedRegion(new CellRangeAddress(1, 1, dicldapList3.Count + 1, dicldapList3.Count + 1));
        //        }
        //        else
        //        {
        //            SetExcelCell(sheet3, 1, 3, styleHead12, "部門別/科別");
        //            sheet3.AddMergedRegion(new CellRangeAddress(1, 1, 3, 3));
        //            SetExcelCell(sheet3, 1, 4, styleHead12, "集作三科");
        //            sheet3.AddMergedRegion(new CellRangeAddress(1, 1, 4, 4));
        //        }

        //        //*結果集表頭 line2
        //        SetExcelCell(sheet3, 6, 0, styleHead10, "處理人員");
        //        sheet3.AddMergedRegion(new CellRangeAddress(6, 6, 0, 0));
        //        sheet3.SetColumnWidth(0, 100 * 50);
        //        //依次排列人員名稱
        //        a = 1;
        //        foreach (var item in dicldapList3)
        //        {
        //            SetExcelCell(sheet3, 6, a, styleHead10, item.Value.Split('|')[0]);
        //            sheet3.AddMergedRegion(new CellRangeAddress(6, 6, a, a));
        //            a++;
        //        }
        //        SetExcelCell(sheet3, 6, dicldapList3.Count + 1, styleHead10, "合計");
        //        sheet3.AddMergedRegion(new CellRangeAddress(6, 6, dicldapList3.Count + 1, dicldapList3.Count + 1));

        //        //*扣押案件類型 line3-lineN 
        //        caseKind = "";//*去重複
        //        rows = 6;//定義行數
        //        for (int i = 0; i < dtCase3.Rows.Count; i++)
        //        {
        //            if (caseKind != dtCase3.Rows[i]["New_CaseKind"].ToString())
        //            {
        //                rows = rows + 1;
        //                SetExcelCell(sheet3, rows, 0, styleHead10, dtCase3.Rows[i]["New_CaseKind"].ToString());
        //                sheet3.AddMergedRegion(new CellRangeAddress(rows, rows, 0, 0));
        //                SetExcelCell(sheet3, rows, dicldapList3.Count + 1, styleHead10, "0");
        //                sheet3.AddMergedRegion(new CellRangeAddress(rows, rows, dicldapList3.Count + 1, dicldapList3.Count + 1));
        //                caseKind = dtCase3.Rows[i]["New_CaseKind"].ToString();
        //                for (int j = 0; j < dicldapList3.Count; j++)//*初始表格賦初值 
        //                {
        //                    SetExcelCell(sheet3, rows, j + 1, styleHead10, "0");
        //                    sheet3.AddMergedRegion(new CellRangeAddress(rows, rows, j + 1, j + 1));
        //                }
        //            }
        //        }

        //        //*合計 lineLast
        //        SetExcelCell(sheet3, rows + 1, 0, styleHead10, "合計");
        //        sheet3.AddMergedRegion(new CellRangeAddress(rows + 1, rows + 1, 0, 0));
        //        for (int j = 0; j < dicldapList3.Count; j++)//*初始表格賦初值 
        //        {
        //            SetExcelCell(sheet3, rows + 1, j + 1, styleHead10, "0");
        //            sheet3.AddMergedRegion(new CellRangeAddress(rows + 1, rows + 1, j + 1, j + 1));
        //        }
        //        #endregion
        //        #region body3
        //        caseExcel = "";//案件類型
        //        rowsExcel = 6;//行數
        //        rowscountExcel = 0;//最後一列合計
        //        rowstatolExcel = 0;//總合計  
        //        for (int iRow = 0; iRow < dtCase3.Rows.Count; iRow++)//根據案件類型進行循環
        //        {
        //            foreach (var item in dicldapList3)
        //            {
        //                int irows3 = Convert.ToInt32(item.Value.Split('|')[1]);
        //                if (dtCase3.Rows[iRow]["New_CaseKind"].ToString() == caseExcel)//重複同一案件類型的數據
        //                {
        //                    if (dtCase3.Rows[iRow]["ApproveUser"].ToString() == item.Key)
        //                    {
        //                        SetExcelCell(sheet3, rowsExcel, irows3, styleHead10, dtCase3.Rows[iRow]["case_num"].ToString());
        //                        SetExcelCell(sheet3, rows + 1, irows3, styleHead10, dtCase3.Rows[iRow]["UserCount"].ToString());//最後一行合計
        //                        rowscountExcelresult = Convert.ToInt32(dtCase3.Rows[iRow]["case_num"].ToString());//每格資料
        //                        rowscountExcel += rowscountExcelresult;
        //                        rowstatolExcel += rowscountExcelresult;
        //                    }
        //                    SetExcelCell(sheet3, rowsExcel, dicldapList3.Count + 1, styleHead10, rowscountExcel.ToString());//最後一列合計
        //                }
        //                else//不重複的案件類型
        //                {
        //                    rowscountExcel = 0;
        //                    rowsExcel = rowsExcel + 1;
        //                    if (dtCase3.Rows[iRow]["ApproveUser"].ToString() == item.Key)
        //                    {
        //                        SetExcelCell(sheet3, rowsExcel, irows3, styleHead10, dtCase3.Rows[iRow]["case_num"].ToString());
        //                        SetExcelCell(sheet3, rows + 1, irows3, styleHead10, dtCase3.Rows[iRow]["UserCount"].ToString());//最後一行合計
        //                        rowscountExcelresult = Convert.ToInt32(dtCase3.Rows[iRow]["case_num"].ToString());//第一條不重複的數據儲存下值
        //                        rowscountExcel += rowscountExcelresult;
        //                        rowstatolExcel += rowscountExcelresult;
        //                    }
        //                    caseExcel = dtCase3.Rows[iRow]["New_CaseKind"].ToString();
        //                    SetExcelCell(sheet3, rowsExcel, dicldapList3.Count + 1, styleHead10, rowscountExcel.ToString());//最後一列合計      
        //                }
        //            }
        //        }
        //        SetExcelCell(sheet3, rows + 1, dicldapList3.Count + 1, styleHead10, rowstatolExcel.ToString());//總合計
        //        #endregion
        //    }

        //    MemoryStream ms = new MemoryStream();
        //    workbook.Write(ms);
        //    ms.Flush();
        //    ms.Position = 0;
        //    workbook = null;
        //    return ms;
        //}
        //#endregion

        //20170811 RC RQ-2015-019666-020 派件至跨單位(原產生邏輯程式寫死，組織異動必出問題，改為參數) 宏祥 add start
        #region 主管放行統計表1
        public MemoryStream ApproveReportExcel_NPOI1(CaseClosedQuery model, IList<PARMCode> listCode)
        {
           IWorkbook workbook = new HSSFWorkbook();
           int Departmentcount = listCode.Count();

           //ISheet sheet = null;
           //ISheet sheet2 = null;
           //ISheet sheet3 = null;
           //Dictionary<string, string> dicldapList = new Dictionary<string, string>();
           //Dictionary<string, string> dicldapList2 = new Dictionary<string, string>();
           //Dictionary<string, string> dicldapList3 = new Dictionary<string, string>();
           //DataTable dtCase = new DataTable();//導出資料
           //DataTable dtCase2 = new DataTable();
           //DataTable dtCase3 = new DataTable();

           int rowscountExcelresult = 0;//合計參數
           string caseExcel = "";//案件類型
           int rowsExcel = 6;//行數
           int rowscountExcel = 0;//最後一列合計
           int rowstatolExcel = 0;//總合計 
           int sort = 1;

           #region def style
           ICellStyle styleHead12 = workbook.CreateCellStyle();
           IFont font12 = workbook.CreateFont();
           font12.FontHeightInPoints = 12;
           font12.FontName = "新細明體";
           styleHead12.FillPattern = FillPattern.SolidForeground;
           styleHead12.FillForegroundColor = HSSFColor.White.Index;
           styleHead12.BorderTop = BorderStyle.None;
           styleHead12.BorderLeft = BorderStyle.None;
           styleHead12.BorderRight = BorderStyle.None;
           styleHead12.BorderBottom = BorderStyle.None;
           styleHead12.WrapText = true;
           styleHead12.Alignment = HorizontalAlignment.Center;
           styleHead12.VerticalAlignment = VerticalAlignment.Center;
           styleHead12.SetFont(font12);

           ICellStyle styleHead10 = workbook.CreateCellStyle();
           IFont font10 = workbook.CreateFont();
           font10.FontHeightInPoints = 10;
           font10.FontName = "新細明體";
           styleHead10.FillPattern = FillPattern.SolidForeground;
           styleHead10.FillForegroundColor = HSSFColor.White.Index;
           styleHead10.BorderTop = BorderStyle.Thin;
           styleHead10.BorderLeft = BorderStyle.Thin;
           styleHead10.BorderRight = BorderStyle.Thin;
           styleHead10.BorderBottom = BorderStyle.Thin;
           styleHead10.WrapText = true;
           styleHead10.Alignment = HorizontalAlignment.Left;
           styleHead10.VerticalAlignment = VerticalAlignment.Center;
           styleHead10.SetFont(font10);
           #endregion

           //判斷科別搜尋條件
           if (model.Depart != "0")
           {
              for (int k = 0; k < Departmentcount; k++)
              {
                 if (model.Depart == (k + 1).ToString())
                 {
                    ISheet sheet = null;
                    Dictionary<string, string> dicldapList = new Dictionary<string, string>();
                    DataTable dtCase = new DataTable();

                    sheet = workbook.CreateSheet(listCode[k].CodeDesc);
                    dtCase = GetApproveList1(model, listCode[k].CodeDesc);

                    foreach (DataRow dr in dtCase.Rows)
                    {
                       if (!dicldapList.Keys.Contains(dr["ApproveUser"].ToString()))
                       {
                          dicldapList.Add(dr["ApproveUser"].ToString(), dr["EmpName"].ToString() + "|" + sort);
                          sort++;
                       }
                    }

                    if (dicldapList.Count > 2)
                    {
                       SetExcelCell(sheet, 1, dicldapList.Count + 1, styleHead12, listCode[k].CodeDesc);
                       sheet.AddMergedRegion(new CellRangeAddress(1, 1, dicldapList.Count + 1, dicldapList.Count + 1));
                    }
                    else
                    {
                       SetExcelCell(sheet, 1, 4, styleHead12, listCode[k].CodeDesc);
                       sheet.AddMergedRegion(new CellRangeAddress(1, 1, 4, 4));
                    }

                    string caseKind = "";//*去重複
                    int rows = 6;//定義行數

                    #region title
                    //*大標題 line0
                    SetExcelCell(sheet, 0, 0, styleHead12, "主管放行統計表");
                    sheet.AddMergedRegion(new CellRangeAddress(0, 0, 0, dicldapList.Count + 1));

                    //*查詢條件 line1
                    SetExcelCell(sheet, 1, 0, styleHead12, "收件日期：");
                    sheet.AddMergedRegion(new CellRangeAddress(1, 1, 0, 0));
                    SetExcelCell(sheet, 1, 1, styleHead12, model.ReceiveDateStart + '~' + model.ReceiveDateEnd);
                    sheet.AddMergedRegion(new CellRangeAddress(1, 1, 1, 2));


                    SetExcelCell(sheet, 2, 0, styleHead12, "發文日期：");
                    sheet.AddMergedRegion(new CellRangeAddress(2, 2, 0, 0));
                    SetExcelCell(sheet, 2, 1, styleHead12, model.SendDateStart + '~' + model.SendDateEnd);
                    sheet.AddMergedRegion(new CellRangeAddress(2, 2, 1, 2));

                    SetExcelCell(sheet, 3, 0, styleHead12, "主管放行日：");
                    sheet.AddMergedRegion(new CellRangeAddress(3, 3, 0, 0));
                    SetExcelCell(sheet, 3, 1, styleHead12, model.ApproveDateStart + '~' + model.ApproveDateEnd);
                    sheet.AddMergedRegion(new CellRangeAddress(3, 3, 1, 2));

                    SetExcelCell(sheet, 4, 0, styleHead12, "電子發文上傳日：");
                    sheet.AddMergedRegion(new CellRangeAddress(4, 4, 0, 0));
                    SetExcelCell(sheet, 4, 1, styleHead12, model.SendUpDateStart + '~' + model.SendUpDateEnd);
                    sheet.AddMergedRegion(new CellRangeAddress(4, 4, 1, 2));

                    SetExcelCell(sheet, 5, 0, styleHead12, "發文方式：");
                    sheet.AddMergedRegion(new CellRangeAddress(5, 5, 0, 0));
                    SetExcelCell(sheet, 5, 1, styleHead12, model.SendKind);
                    sheet.AddMergedRegion(new CellRangeAddress(5, 5, 1, 2));

                    if (dicldapList.Count > 2)
                    {
                       SetExcelCell(sheet, 1, dicldapList.Count, styleHead12, "部門別/科別");
                       sheet.AddMergedRegion(new CellRangeAddress(1, 1, dicldapList.Count, dicldapList.Count));
                    }
                    else
                    {
                       SetExcelCell(sheet, 1, 3, styleHead12, "部門別/科別");
                       sheet.AddMergedRegion(new CellRangeAddress(1, 1, 3, 3));
                       sheet.SetColumnWidth(3, 100 * 40);
                    }

                    //*結果集表頭 line2
                    SetExcelCell(sheet, 6, 0, styleHead10, "處理人員");
                    sheet.AddMergedRegion(new CellRangeAddress(6, 6, 0, 0));
                    sheet.SetColumnWidth(0, 100 * 50);

                    //依次排列人員名稱
                    int a = 1;
                    foreach (var item in dicldapList)
                    {
                       SetExcelCell(sheet, 6, a, styleHead10, item.Value.Split('|')[0]);
                       sheet.AddMergedRegion(new CellRangeAddress(6, 6, a, a));
                       a++;
                    }

                    SetExcelCell(sheet, 6, dicldapList.Count + 1, styleHead10, "合計");
                    sheet.AddMergedRegion(new CellRangeAddress(6, 6, dicldapList.Count + 1, dicldapList.Count + 1));

                    //*扣押案件類型 line5-lineN 
                    for (int i = 0; i < dtCase.Rows.Count; i++)
                    {
                       if (caseKind != dtCase.Rows[i]["New_CaseKind"].ToString())
                       {
                          rows = rows + 1;
                          SetExcelCell(sheet, rows, 0, styleHead10, dtCase.Rows[i]["New_CaseKind"].ToString());
                          sheet.AddMergedRegion(new CellRangeAddress(rows, rows, 0, 0));
                          SetExcelCell(sheet, rows, dicldapList.Count + 1, styleHead10, "0");
                          sheet.AddMergedRegion(new CellRangeAddress(rows, rows, dicldapList.Count + 1, dicldapList.Count + 1));
                          caseKind = dtCase.Rows[i]["New_CaseKind"].ToString();
                          for (int j = 0; j < dicldapList.Count; j++)//*初始表格賦初值 
                          {
                             SetExcelCell(sheet, rows, j + 1, styleHead10, "0");
                             sheet.AddMergedRegion(new CellRangeAddress(rows, rows, j + 1, j + 1));
                          }
                       }
                    }

                    //*合計 lineLast
                    SetExcelCell(sheet, rows + 1, 0, styleHead10, "合計");
                    sheet.AddMergedRegion(new CellRangeAddress(rows + 1, rows + 1, 0, 0));

                    for (int j = 0; j < dicldapList.Count; j++)//*初始表格賦初值 
                    {
                       SetExcelCell(sheet, rows + 1, j + 1, styleHead10, "0");
                       sheet.AddMergedRegion(new CellRangeAddress(rows + 1, rows + 1, j + 1, j + 1));
                    }
                    #endregion
                    #region  body
                    for (int iRow = 0; iRow < dtCase.Rows.Count; iRow++)//根據案件類型進行循環
                    {
                       foreach (var item in dicldapList)
                       {
                          int irows = Convert.ToInt32(item.Value.Split('|')[1]);
                          if (caseExcel == dtCase.Rows[iRow]["New_CaseKind"].ToString())//重複同一案件類型的數據
                          {
                             if (item.Key == dtCase.Rows[iRow]["ApproveUser"].ToString())
                             {
                                SetExcelCell(sheet, rowsExcel, irows, styleHead10, dtCase.Rows[iRow]["case_num"].ToString());
                                rowscountExcelresult = Convert.ToInt32(dtCase.Rows[iRow]["case_num"].ToString());//每格資料
                                SetExcelCell(sheet, rows + 1, irows, styleHead10, dtCase.Rows[iRow]["UserCount"].ToString());//最後一行合計
                                rowscountExcel += rowscountExcelresult;
                                rowstatolExcel += rowscountExcelresult;
                             }
                             SetExcelCell(sheet, rowsExcel, dicldapList.Count + 1, styleHead10, rowscountExcel.ToString());//最後一列合計
                          }
                          else//不重複的案件類型
                          {
                             rowscountExcel = 0;
                             rowsExcel = rowsExcel + 1;
                             if (dtCase.Rows[iRow]["ApproveUser"].ToString() == item.Key)
                             {
                                SetExcelCell(sheet, rowsExcel, irows, styleHead10, dtCase.Rows[iRow]["case_num"].ToString());
                                rowscountExcelresult = Convert.ToInt32(dtCase.Rows[iRow]["case_num"].ToString());//第一條不重複的數據儲存下值
                                SetExcelCell(sheet, rows + 1, irows, styleHead10, dtCase.Rows[iRow]["UserCount"].ToString());//最後一行合計
                                rowscountExcel += rowscountExcelresult;
                                rowstatolExcel += rowscountExcelresult;
                             }
                             caseExcel = dtCase.Rows[iRow]["New_CaseKind"].ToString();
                             SetExcelCell(sheet, rowsExcel, dicldapList.Count + 1, styleHead10, rowscountExcel.ToString());//最後一列合計
                          }
                       }
                    }
                    SetExcelCell(sheet, rows + 1, dicldapList.Count + 1, styleHead10, rowstatolExcel.ToString());//總合計
                    #endregion
                 }
              }
           }
           else
           {
              for (int k = 0; k < Departmentcount; k++)
              {
                 ISheet sheet = null;
                 Dictionary<string, string> dicldapList = new Dictionary<string, string>();
                 DataTable dtCase = new DataTable();

                 sheet = workbook.CreateSheet(listCode[k].CodeDesc);
                 dtCase = GetApproveList1(model, listCode[k].CodeDesc);

                 sort = 1;

                 foreach (DataRow dr in dtCase.Rows)
                 {
                    if (!dicldapList.Keys.Contains(dr["ApproveUser"].ToString()))
                    {
                       dicldapList.Add(dr["ApproveUser"].ToString(), dr["EmpName"].ToString() + "|" + sort);
                       sort++;
                    }
                 }

                 if (dicldapList.Count > 2)
                 {
                    SetExcelCell(sheet, 1, dicldapList.Count + 1, styleHead12, listCode[k].CodeDesc);
                    sheet.AddMergedRegion(new CellRangeAddress(1, 1, dicldapList.Count + 1, dicldapList.Count + 1));
                 }
                 else
                 {
                    SetExcelCell(sheet, 1, 4, styleHead12, listCode[k].CodeDesc);
                    sheet.AddMergedRegion(new CellRangeAddress(1, 1, 4, 4));
                 }

                 string caseKind = "";//*去重複
                 int rows = 6;//定義行數

                 #region title
                 //*大標題 line0
                 SetExcelCell(sheet, 0, 0, styleHead12, "主管放行統計表");
                 sheet.AddMergedRegion(new CellRangeAddress(0, 0, 0, dicldapList.Count + 1));

                 //*查詢條件 line1
                 SetExcelCell(sheet, 1, 0, styleHead12, "收件日期：");
                 sheet.AddMergedRegion(new CellRangeAddress(1, 1, 0, 0));
                 SetExcelCell(sheet, 1, 1, styleHead12, model.ReceiveDateStart + '~' + model.ReceiveDateEnd);
                 sheet.AddMergedRegion(new CellRangeAddress(1, 1, 1, 2));

                 SetExcelCell(sheet, 2, 0, styleHead12, "發文日期：");
                 sheet.AddMergedRegion(new CellRangeAddress(2, 2, 0, 0));
                 SetExcelCell(sheet, 2, 1, styleHead12, model.SendDateStart + '~' + model.SendDateEnd);
                 sheet.AddMergedRegion(new CellRangeAddress(2, 2, 1, 2));

                 SetExcelCell(sheet, 3, 0, styleHead12, "主管放行日：");
                 sheet.AddMergedRegion(new CellRangeAddress(3, 3, 0, 0));
                 SetExcelCell(sheet, 3, 1, styleHead12, model.ApproveDateStart + '~' + model.ApproveDateEnd);
                 sheet.AddMergedRegion(new CellRangeAddress(3, 3, 1, 2));

                 SetExcelCell(sheet, 4, 0, styleHead12, "電子發文上傳日：");
                 sheet.AddMergedRegion(new CellRangeAddress(4, 4, 0, 0));
                 SetExcelCell(sheet, 4, 1, styleHead12, model.SendUpDateStart + '~' + model.SendUpDateEnd);
                 sheet.AddMergedRegion(new CellRangeAddress(4, 4, 1, 2));

                 SetExcelCell(sheet, 5, 0, styleHead12, "發文方式：");
                 sheet.AddMergedRegion(new CellRangeAddress(5, 5, 0, 0));
                 SetExcelCell(sheet, 5, 1, styleHead12, model.SendKind);
                 sheet.AddMergedRegion(new CellRangeAddress(5, 5, 1, 2));

                 if (dicldapList.Count > 2)
                 {
                    SetExcelCell(sheet, 1, dicldapList.Count - 1, styleHead12, "部門別/科別");
                    sheet.AddMergedRegion(new CellRangeAddress(1, 1, dicldapList.Count - 1, dicldapList.Count));
                    SetExcelCell(sheet, 1, dicldapList.Count + 1, styleHead12, listCode[k].CodeDesc);
                    sheet.AddMergedRegion(new CellRangeAddress(1, 1, dicldapList.Count + 1, dicldapList.Count + 1));
                 }
                 else
                 {
                    SetExcelCell(sheet, 1, 3, styleHead12, "部門別/科別");
                    sheet.AddMergedRegion(new CellRangeAddress(1, 1, 3, 3));
                    SetExcelCell(sheet, 1, 4, styleHead12, listCode[k].CodeDesc);
                    sheet.AddMergedRegion(new CellRangeAddress(1, 1, 4, 4));
                 }

                 //*結果集表頭 line4
                 SetExcelCell(sheet, 6, 0, styleHead10, "處理人員");
                 sheet.AddMergedRegion(new CellRangeAddress(6, 6, 0, 0));
                 sheet.SetColumnWidth(0, 100 * 50);

                 //依次排列人員名稱
                 int a = 1;
                 foreach (var item in dicldapList)
                 {
                    SetExcelCell(sheet, 6, a, styleHead10, item.Value.Split('|')[0]);
                    sheet.AddMergedRegion(new CellRangeAddress(6, 6, a, a));
                    a++;
                 }

                 SetExcelCell(sheet, 6, dicldapList.Count + 1, styleHead10, "合計");
                 sheet.AddMergedRegion(new CellRangeAddress(6, 6, dicldapList.Count + 1, dicldapList.Count + 1));

                 //*扣押案件類型 line3-lineN 
                 caseKind = "";//*去重複
                 rows = 6;//定義行數
                 for (int i = 0; i < dtCase.Rows.Count; i++)
                 {
                    if (caseKind != dtCase.Rows[i]["New_CaseKind"].ToString())
                    {
                       rows = rows + 1;
                       SetExcelCell(sheet, rows, 0, styleHead10, dtCase.Rows[i]["New_CaseKind"].ToString());
                       sheet.AddMergedRegion(new CellRangeAddress(rows, rows, 0, 0));

                       SetExcelCell(sheet, rows, dicldapList.Count + 1, styleHead10, "0");
                       sheet.AddMergedRegion(new CellRangeAddress(rows, rows, dicldapList.Count + 1, dicldapList.Count + 1));

                       caseKind = dtCase.Rows[i]["New_CaseKind"].ToString();
                       for (int j = 0; j < dicldapList.Count; j++)//*初始表格賦初值 
                       {
                          SetExcelCell(sheet, rows, j + 1, styleHead10, "0");
                          sheet.AddMergedRegion(new CellRangeAddress(rows, rows, j + 1, j + 1));
                       }
                    }
                 }

                 //*合計 lineLast
                 SetExcelCell(sheet, rows + 1, 0, styleHead10, "合計");
                 sheet.AddMergedRegion(new CellRangeAddress(rows + 1, rows + 1, 0, 0));
                 for (int j = 0; j < dicldapList.Count; j++)//*初始表格賦初值 
                 {
                    SetExcelCell(sheet, rows + 1, j + 1, styleHead10, "0");
                    sheet.AddMergedRegion(new CellRangeAddress(rows + 1, rows + 1, j + 1, j + 1));
                 }
                 #endregion
                 #region body

                 caseExcel = "";//案件類型
                 rowsExcel = 6;//行數
                 rowscountExcel = 0;//最後一列合計
                 rowstatolExcel = 0;//總合計
      
                 for (int iRow = 0; iRow < dtCase.Rows.Count; iRow++)//根據案件類型進行循環
                 {
                    foreach (var item in dicldapList)
                    {
                       int irows = Convert.ToInt32(item.Value.Split('|')[1]);
                       if (dtCase.Rows[iRow]["New_CaseKind"].ToString() == caseExcel)//重複同一案件類型的數據
                       {
                          if (dtCase.Rows[iRow]["ApproveUser"].ToString() == item.Key)
                          {
                             SetExcelCell(sheet, rowsExcel, irows, styleHead10, dtCase.Rows[iRow]["case_num"].ToString());
                             SetExcelCell(sheet, rows + 1, irows, styleHead10, dtCase.Rows[iRow]["UserCount"].ToString());//最後一行合計
                             rowscountExcelresult = Convert.ToInt32(dtCase.Rows[iRow]["case_num"].ToString());//每格資料
                             rowscountExcel += rowscountExcelresult;
                             rowstatolExcel += rowscountExcelresult;
                          }
                          SetExcelCell(sheet, rowsExcel, dicldapList.Count + 1, styleHead10, rowscountExcel.ToString());//最後一列合計
                       }
                       else//不重複的案件類型
                       {
                          rowscountExcel = 0;
                          rowsExcel = rowsExcel + 1;
                          if (dtCase.Rows[iRow]["ApproveUser"].ToString() == item.Key)
                          {
                             SetExcelCell(sheet, rowsExcel, irows, styleHead10, dtCase.Rows[iRow]["case_num"].ToString());
                             SetExcelCell(sheet, rows + 1, irows, styleHead10, dtCase.Rows[iRow]["UserCount"].ToString());//最後一行合計
                             rowscountExcelresult = Convert.ToInt32(dtCase.Rows[iRow]["case_num"].ToString());//第一條不重複的數據儲存下值
                             rowscountExcel += rowscountExcelresult;
                             rowstatolExcel += rowscountExcelresult;
                          }
                          caseExcel = dtCase.Rows[iRow]["New_CaseKind"].ToString();
                          SetExcelCell(sheet, rowsExcel, dicldapList.Count + 1, styleHead10, rowscountExcel.ToString());//最後一列合計      
                       }
                    }
                 }
                 SetExcelCell(sheet, rows + 1, dicldapList.Count + 1, styleHead10, rowstatolExcel.ToString());//總合計
                 #endregion
              }
           }

           MemoryStream ms = new MemoryStream();
           workbook.Write(ms);
           ms.Flush();
           ms.Position = 0;
           workbook = null;
           return ms;
        }
        #endregion
        //20170811 RC RQ-2015-019666-020 派件至跨單位(原產生邏輯程式寫死，組織異動必出問題，改為參數) 宏祥 add end

        //#region 經辦退件明細表1
        //public MemoryStream ReturnDetailReportExcel_NPOI1(CaseClosedQuery model)
        //{
        //    IWorkbook workbook = new HSSFWorkbook();
        //    ISheet sheet = null;
        //    ISheet sheet2 = null;
        //    ISheet sheet3 = null;

        //    DataTable dt = new DataTable();
        //    DataTable dt2 = new DataTable();
        //    DataTable dt3 = new DataTable();

        //    #region def style
        //    ICellStyle styleHead12 = workbook.CreateCellStyle();
        //    IFont font12 = workbook.CreateFont();
        //    font12.FontHeightInPoints = 12;
        //    font12.FontName = "新細明體";
        //    styleHead12.FillPattern = FillPattern.SolidForeground;
        //    styleHead12.FillForegroundColor = HSSFColor.White.Index;
        //    styleHead12.BorderTop = BorderStyle.None;
        //    styleHead12.BorderLeft = BorderStyle.None;
        //    styleHead12.BorderRight = BorderStyle.None;
        //    styleHead12.BorderBottom = BorderStyle.None;
        //    styleHead12.WrapText = true;
        //    styleHead12.Alignment = HorizontalAlignment.Center;
        //    styleHead12.VerticalAlignment = VerticalAlignment.Center;
        //    styleHead12.SetFont(font12);

        //    ICellStyle styleHead10 = workbook.CreateCellStyle();
        //    IFont font10 = workbook.CreateFont();
        //    font10.FontHeightInPoints = 10;
        //    font10.FontName = "新細明體";
        //    styleHead10.FillPattern = FillPattern.SolidForeground;
        //    styleHead10.FillForegroundColor = HSSFColor.White.Index;
        //    styleHead10.BorderTop = BorderStyle.Thin;
        //    styleHead10.BorderLeft = BorderStyle.Thin;
        //    styleHead10.BorderRight = BorderStyle.Thin;
        //    styleHead10.BorderBottom = BorderStyle.Thin;
        //    styleHead10.WrapText = true;
        //    styleHead10.Alignment = HorizontalAlignment.Left;
        //    styleHead10.VerticalAlignment = VerticalAlignment.Center;
        //    styleHead10.SetFont(font10);
        //    #endregion

        //    #region 獲取數據源(集作一科及案件資料)
        //    //獲取人員
        //    if (model.Depart == "1")//* 集作一科
        //    {
        //        sheet = workbook.CreateSheet("集作一科");
        //        dt = GetReturnDetailList1(model, "集作一科");//獲取查詢集作一科的案件
        //        SetExcelCell(sheet, 1, 4, styleHead12, "集作一科");
        //        sheet.AddMergedRegion(new CellRangeAddress(1, 1, 4, 4));
        //    }
        //    if (model.Depart == "2")//* 集作二科
        //    {
        //        sheet = workbook.CreateSheet("集作二科");
        //        dt = GetReturnDetailList1(model, "集作二科");//獲取查詢集作二科的案件
        //        SetExcelCell(sheet, 1, 4, styleHead12, "集作二科");
        //        sheet.AddMergedRegion(new CellRangeAddress(1, 1, 4, 4));
        //    }
        //    if (model.Depart == "3")//*集作三科
        //    {
        //        sheet = workbook.CreateSheet("集作三科");
        //        dt = GetReturnDetailList1(model, "集作三科");//獲取查詢集作三科的案件
        //        SetExcelCell(sheet, 1, 4, styleHead12, "集作三科");
        //        sheet.AddMergedRegion(new CellRangeAddress(1, 1, 4, 4));
        //    }
        //    if (model.Depart == "0")//*全部
        //    {
        //        sheet = workbook.CreateSheet("集作一科");
        //        sheet2 = workbook.CreateSheet("集作二科");
        //        sheet3 = workbook.CreateSheet("集作三科");
        //        dt = GetReturnDetailList1(model, "集作一科");//獲取查詢集作一科的案件
        //        dt2 = GetReturnDetailList1(model, "集作二科");//獲取查詢集作二科的案件
        //        dt3 = GetReturnDetailList1(model, "集作三科");//獲取查詢集作三科的案件
        //        SetExcelCell(sheet, 1, 4, styleHead12, "集作一科");
        //        sheet.AddMergedRegion(new CellRangeAddress(1, 1, 4, 4));
        //        SetExcelCell(sheet2, 1, 4, styleHead12, "集作二科");
        //        sheet2.AddMergedRegion(new CellRangeAddress(1, 1, 4, 4));
        //        SetExcelCell(sheet3, 1, 4, styleHead12, "集作三科");
        //        sheet3.AddMergedRegion(new CellRangeAddress(1, 1, 4, 4));
        //    }
        //    #endregion

        //    #region title
        //    SetExcelCell(sheet, 0, 0, styleHead12, "經辦退件明細表");
        //    sheet.AddMergedRegion(new CellRangeAddress(0, 0, 0, 3));

        //    //* line1
        //    SetExcelCell(sheet, 1, 0, styleHead12, "收件日期：");
        //    sheet.AddMergedRegion(new CellRangeAddress(1, 1, 0, 0));
        //    SetExcelCell(sheet, 1, 1, styleHead12, model.ReceiveDateStart + '~' + model.ReceiveDateEnd);
        //    sheet.AddMergedRegion(new CellRangeAddress(1, 1, 1, 2));

        //    SetExcelCell(sheet, 2, 0, styleHead12, "發文日期：");
        //    sheet.AddMergedRegion(new CellRangeAddress(2, 2, 0, 0));
        //    SetExcelCell(sheet, 2, 1, styleHead12, model.SendDateStart + '~' + model.SendDateEnd);
        //    sheet.AddMergedRegion(new CellRangeAddress(2, 2, 1, 2));

        //    SetExcelCell(sheet, 3, 0, styleHead12, "主管放行日：");
        //    sheet.AddMergedRegion(new CellRangeAddress(3, 3, 0, 0));
        //    SetExcelCell(sheet, 3, 1, styleHead12, model.ApproveDateStart + '~' + model.ApproveDateEnd);
        //    sheet.AddMergedRegion(new CellRangeAddress(3, 3, 1, 2));

        //    SetExcelCell(sheet, 1, 3, styleHead12, "部門別/科別：");
        //    sheet.AddMergedRegion(new CellRangeAddress(1, 1, 3, 3));

        //    SetExcelCell(sheet, 4, 0, styleHead12, "電子發文上傳日：");
        //    sheet.AddMergedRegion(new CellRangeAddress(4, 4, 0, 0));
        //    SetExcelCell(sheet, 4, 1, styleHead12, model.SendUpDateStart + '~' + model.SendUpDateEnd);
        //    sheet.AddMergedRegion(new CellRangeAddress(4, 4, 1, 2));

        //    SetExcelCell(sheet, 5, 0, styleHead12, "發文方式：");
        //    sheet.AddMergedRegion(new CellRangeAddress(5, 5, 0, 0));
        //    SetExcelCell(sheet, 5, 1, styleHead12, model.SendKind);
        //    sheet.AddMergedRegion(new CellRangeAddress(5, 5, 1, 2));

        //    //* line2
        //    SetExcelCell(sheet, 6, 0, styleHead10, "案件類別-大類");
        //    sheet.AddMergedRegion(new CellRangeAddress(6, 6, 0, 0));
        //    SetExcelCell(sheet, 6, 1, styleHead10, "案件編號");
        //    sheet.AddMergedRegion(new CellRangeAddress(6, 6, 1, 1));
        //    SetExcelCell(sheet, 6, 2, styleHead10, "處理經辦");
        //    sheet.AddMergedRegion(new CellRangeAddress(6, 6, 2, 2));
        //    SetExcelCell(sheet, 6, 3, styleHead10, "退件原因");
        //    sheet.AddMergedRegion(new CellRangeAddress(6, 6, 3, 3));
        //    #endregion
        //    #region Width
        //    sheet.SetColumnWidth(0, 100 * 40);
        //    sheet.SetColumnWidth(1, 100 * 40);
        //    sheet.SetColumnWidth(2, 100 * 40);
        //    sheet.SetColumnWidth(3, 100 * 100);
        //    sheet.SetColumnWidth(4, 100 * 50);
        //    #endregion
        //    #region body
        //    for (int iRow = 0; iRow < dt.Rows.Count; iRow++)
        //    {
        //        for (int iCol = 1; iCol < dt.Columns.Count - 1; iCol++)
        //        {
        //            SetExcelCell(sheet, iRow + 7, iCol - 1, styleHead10, dt.Rows[iRow][iCol].ToString());
        //        }
        //    }
        //    #endregion

        //    if (model.Depart == "0")//* 全部
        //    {
        //        #region title2
        //        SetExcelCell(sheet2, 0, 0, styleHead12, "經辦退件明細表");
        //        sheet2.AddMergedRegion(new CellRangeAddress(0, 0, 0, 3));

        //        //* line1
        //        SetExcelCell(sheet2, 1, 0, styleHead12, "收件日期：");
        //        sheet2.AddMergedRegion(new CellRangeAddress(1, 1, 0, 0));
        //        SetExcelCell(sheet2, 1, 1, styleHead12, model.ReceiveDateStart + '~' + model.ReceiveDateEnd);
        //        sheet2.AddMergedRegion(new CellRangeAddress(1, 1, 1, 2));

        //        SetExcelCell(sheet2, 2, 0, styleHead12, "發文日期：");
        //        sheet2.AddMergedRegion(new CellRangeAddress(2, 2, 0, 0));
        //        SetExcelCell(sheet2, 2, 1, styleHead12, model.SendDateStart + '~' + model.SendDateEnd);
        //        sheet2.AddMergedRegion(new CellRangeAddress(2, 2, 1, 2));

        //        SetExcelCell(sheet2, 3, 0, styleHead12, "主管放行日：");
        //        sheet2.AddMergedRegion(new CellRangeAddress(3, 3, 0, 0));
        //        SetExcelCell(sheet2, 3, 1, styleHead12, model.ApproveDateStart + '~' + model.ApproveDateEnd);
        //        sheet2.AddMergedRegion(new CellRangeAddress(3, 3, 1, 2));

        //        SetExcelCell(sheet2, 1, 3, styleHead12, "部門別/科別：");
        //        sheet2.AddMergedRegion(new CellRangeAddress(1, 1, 3, 3));

        //        SetExcelCell(sheet2, 4, 0, styleHead12, "電子發文上傳日：");
        //        sheet2.AddMergedRegion(new CellRangeAddress(4, 4, 0, 0));
        //        SetExcelCell(sheet2, 4, 1, styleHead12, model.SendUpDateStart + '~' + model.SendUpDateEnd);
        //        sheet2.AddMergedRegion(new CellRangeAddress(4, 4, 1, 2));

        //        SetExcelCell(sheet2, 5, 0, styleHead12, "發文方式：");
        //        sheet2.AddMergedRegion(new CellRangeAddress(5, 5, 0, 0));
        //        SetExcelCell(sheet2, 5, 1, styleHead12, model.SendKind);
        //        sheet2.AddMergedRegion(new CellRangeAddress(5, 5, 1, 2));

        //        //* line2
        //        SetExcelCell(sheet2, 6, 0, styleHead10, "案件類別-大類");
        //        sheet2.AddMergedRegion(new CellRangeAddress(6, 6, 0, 0));
        //        SetExcelCell(sheet2, 6, 1, styleHead10, "案件編號");
        //        sheet2.AddMergedRegion(new CellRangeAddress(6, 6, 1, 1));
        //        SetExcelCell(sheet2, 6, 2, styleHead10, "處理經辦");
        //        sheet2.AddMergedRegion(new CellRangeAddress(6, 6, 2, 2));
        //        SetExcelCell(sheet2, 6, 3, styleHead10, "退件原因");
        //        sheet2.AddMergedRegion(new CellRangeAddress(6, 6, 3, 3));
        //        #endregion

        //        #region title3
        //        SetExcelCell(sheet3, 0, 0, styleHead12, "經辦退件明細表");
        //        sheet3.AddMergedRegion(new CellRangeAddress(0, 0, 0, 3));

        //        //* line1
        //        SetExcelCell(sheet3, 1, 0, styleHead12, "收件日期：");
        //        sheet3.AddMergedRegion(new CellRangeAddress(1, 1, 0, 0));
        //        SetExcelCell(sheet3, 1, 1, styleHead12, model.ReceiveDateStart + '~' + model.ReceiveDateEnd);
        //        sheet3.AddMergedRegion(new CellRangeAddress(1, 1, 1, 2));

        //        SetExcelCell(sheet3, 2, 0, styleHead12, "發文日期：");
        //        sheet3.AddMergedRegion(new CellRangeAddress(2, 2, 0, 0));
        //        SetExcelCell(sheet3, 2, 1, styleHead12, model.SendDateStart + '~' + model.SendDateEnd);
        //        sheet3.AddMergedRegion(new CellRangeAddress(2, 2, 1, 2));

        //        SetExcelCell(sheet3, 3, 0, styleHead12, "主管放行日：");
        //        sheet3.AddMergedRegion(new CellRangeAddress(3, 3, 0, 0));
        //        SetExcelCell(sheet3, 3, 1, styleHead12, model.ApproveDateStart + '~' + model.ApproveDateEnd);
        //        sheet3.AddMergedRegion(new CellRangeAddress(3, 3, 1, 2));

        //        SetExcelCell(sheet3, 1, 3, styleHead12, "部門別/科別：");
        //        sheet3.AddMergedRegion(new CellRangeAddress(1, 1, 3, 3));

        //        SetExcelCell(sheet3, 4, 0, styleHead12, "電子發文上傳日：");
        //        sheet3.AddMergedRegion(new CellRangeAddress(4, 4, 0, 0));
        //        SetExcelCell(sheet3, 4, 1, styleHead12, model.SendUpDateStart + '~' + model.SendUpDateEnd);
        //        sheet3.AddMergedRegion(new CellRangeAddress(4, 4, 1, 2));

        //        SetExcelCell(sheet3, 5, 0, styleHead12, "發文方式：");
        //        sheet3.AddMergedRegion(new CellRangeAddress(5, 5, 0, 0));
        //        SetExcelCell(sheet3, 5, 1, styleHead12, model.SendKind);
        //        sheet3.AddMergedRegion(new CellRangeAddress(5, 5, 1, 2));

        //        //* line2
        //        SetExcelCell(sheet3, 6, 0, styleHead10, "案件類別-大類");
        //        sheet3.AddMergedRegion(new CellRangeAddress(6, 6, 0, 0));
        //        SetExcelCell(sheet3, 6, 1, styleHead10, "案件編號");
        //        sheet3.AddMergedRegion(new CellRangeAddress(6, 6, 1, 1));
        //        SetExcelCell(sheet3, 6, 2, styleHead10, "處理經辦");
        //        sheet3.AddMergedRegion(new CellRangeAddress(6, 6, 2, 2));
        //        SetExcelCell(sheet3, 6, 3, styleHead10, "退件原因");
        //        sheet3.AddMergedRegion(new CellRangeAddress(6, 6, 3, 3));
        //        #endregion
        //        #region Width
        //        sheet2.SetColumnWidth(0, 100 * 40);
        //        sheet2.SetColumnWidth(1, 100 * 40);
        //        sheet2.SetColumnWidth(2, 100 * 40);
        //        sheet2.SetColumnWidth(3, 100 * 100);
        //        sheet2.SetColumnWidth(4, 100 * 50);
        //        #endregion
        //        #region Width
        //        sheet3.SetColumnWidth(0, 100 * 40);
        //        sheet3.SetColumnWidth(1, 100 * 40);
        //        sheet3.SetColumnWidth(2, 100 * 40);
        //        sheet3.SetColumnWidth(3, 100 * 100);
        //        sheet3.SetColumnWidth(4, 100 * 50);
        //        #endregion
        //        #region body2
        //        for (int iRow = 0; iRow < dt2.Rows.Count; iRow++)
        //        {
        //            for (int iCol = 1; iCol < dt2.Columns.Count - 1; iCol++)
        //            {
        //                SetExcelCell(sheet2, iRow + 7, iCol - 1, styleHead10, dt2.Rows[iRow][iCol].ToString());
        //            }
        //        }
        //        #endregion

        //        #region body3
        //        for (int iRow = 0; iRow < dt3.Rows.Count; iRow++)
        //        {
        //            for (int iCol = 1; iCol < dt3.Columns.Count - 1; iCol++)
        //            {
        //                SetExcelCell(sheet3, iRow + 7, iCol - 1, styleHead10, dt3.Rows[iRow][iCol].ToString());
        //            }
        //        }
        //        #endregion
        //    }

        //    MemoryStream ms = new MemoryStream();
        //    workbook.Write(ms);
        //    ms.Flush();
        //    ms.Position = 0;
        //    workbook = null;
        //    return ms;
        //}

        //#endregion

        //20170811 RC RQ-2015-019666-020 派件至跨單位(原產生邏輯程式寫死，組織異動必出問題，改為參數) 宏祥 add start
        #region 經辦退件明細表1
        public MemoryStream ReturnDetailReportExcel_NPOI1(CaseClosedQuery model, IList<PARMCode> listCode)
        {
           IWorkbook workbook = new HSSFWorkbook();
           int Departmentcount = listCode.Count();

           #region def style
           ICellStyle styleHead12 = workbook.CreateCellStyle();
           IFont font12 = workbook.CreateFont();
           font12.FontHeightInPoints = 12;
           font12.FontName = "新細明體";
           styleHead12.FillPattern = FillPattern.SolidForeground;
           styleHead12.FillForegroundColor = HSSFColor.White.Index;
           styleHead12.BorderTop = BorderStyle.None;
           styleHead12.BorderLeft = BorderStyle.None;
           styleHead12.BorderRight = BorderStyle.None;
           styleHead12.BorderBottom = BorderStyle.None;
           styleHead12.WrapText = true;
           styleHead12.Alignment = HorizontalAlignment.Center;
           styleHead12.VerticalAlignment = VerticalAlignment.Center;
           styleHead12.SetFont(font12);

           ICellStyle styleHead10 = workbook.CreateCellStyle();
           IFont font10 = workbook.CreateFont();
           font10.FontHeightInPoints = 10;
           font10.FontName = "新細明體";
           styleHead10.FillPattern = FillPattern.SolidForeground;
           styleHead10.FillForegroundColor = HSSFColor.White.Index;
           styleHead10.BorderTop = BorderStyle.Thin;
           styleHead10.BorderLeft = BorderStyle.Thin;
           styleHead10.BorderRight = BorderStyle.Thin;
           styleHead10.BorderBottom = BorderStyle.Thin;
           styleHead10.WrapText = true;
           styleHead10.Alignment = HorizontalAlignment.Left;
           styleHead10.VerticalAlignment = VerticalAlignment.Center;
           styleHead10.SetFont(font10);
           #endregion

           //判斷科別搜尋條件
           if (model.Depart != "0")
           {
              for (int k = 0; k < Departmentcount; k++)
              {
                 if (model.Depart == (k + 1).ToString())
                 {
                    ISheet sheet = null;
                    DataTable dt = new DataTable();

                    sheet = workbook.CreateSheet(listCode[k].CodeDesc);
                    dt = GetReturnDetailList1(model, listCode[k].CodeDesc);//獲取查詢科別的案件
                    SetExcelCell(sheet, 1, 4, styleHead12, listCode[k].CodeDesc);
                    sheet.AddMergedRegion(new CellRangeAddress(1, 1, 4, 4));

                    #region title
                    SetExcelCell(sheet, 0, 0, styleHead12, "經辦退件明細表");
                    sheet.AddMergedRegion(new CellRangeAddress(0, 0, 0, 3));

                    //* line1
                    SetExcelCell(sheet, 1, 0, styleHead12, "收件日期：");
                    sheet.AddMergedRegion(new CellRangeAddress(1, 1, 0, 0));
                    SetExcelCell(sheet, 1, 1, styleHead12, model.ReceiveDateStart + '~' + model.ReceiveDateEnd);
                    sheet.AddMergedRegion(new CellRangeAddress(1, 1, 1, 2));

                    SetExcelCell(sheet, 2, 0, styleHead12, "發文日期：");
                    sheet.AddMergedRegion(new CellRangeAddress(2, 2, 0, 0));
                    SetExcelCell(sheet, 2, 1, styleHead12, model.SendDateStart + '~' + model.SendDateEnd);
                    sheet.AddMergedRegion(new CellRangeAddress(2, 2, 1, 2));

                    SetExcelCell(sheet, 3, 0, styleHead12, "主管放行日：");
                    sheet.AddMergedRegion(new CellRangeAddress(3, 3, 0, 0));
                    SetExcelCell(sheet, 3, 1, styleHead12, model.ApproveDateStart + '~' + model.ApproveDateEnd);
                    sheet.AddMergedRegion(new CellRangeAddress(3, 3, 1, 2));

                    SetExcelCell(sheet, 1, 3, styleHead12, "部門別/科別：");
                    sheet.AddMergedRegion(new CellRangeAddress(1, 1, 3, 3));

                    SetExcelCell(sheet, 4, 0, styleHead12, "電子發文上傳日：");
                    sheet.AddMergedRegion(new CellRangeAddress(4, 4, 0, 0));
                    SetExcelCell(sheet, 4, 1, styleHead12, model.SendUpDateStart + '~' + model.SendUpDateEnd);
                    sheet.AddMergedRegion(new CellRangeAddress(4, 4, 1, 2));

                    SetExcelCell(sheet, 5, 0, styleHead12, "發文方式：");
                    sheet.AddMergedRegion(new CellRangeAddress(5, 5, 0, 0));
                    SetExcelCell(sheet, 5, 1, styleHead12, model.SendKind);
                    sheet.AddMergedRegion(new CellRangeAddress(5, 5, 1, 2));

                    //* line2
                    SetExcelCell(sheet, 6, 0, styleHead10, "案件類別-大類");
                    sheet.AddMergedRegion(new CellRangeAddress(6, 6, 0, 0));
                    SetExcelCell(sheet, 6, 1, styleHead10, "案件編號");
                    sheet.AddMergedRegion(new CellRangeAddress(6, 6, 1, 1));
                    SetExcelCell(sheet, 6, 2, styleHead10, "處理經辦");
                    sheet.AddMergedRegion(new CellRangeAddress(6, 6, 2, 2));
                    SetExcelCell(sheet, 6, 3, styleHead10, "退件原因");
                    sheet.AddMergedRegion(new CellRangeAddress(6, 6, 3, 3));
                    #endregion
                    #region Width
                    sheet.SetColumnWidth(0, 100 * 40);
                    sheet.SetColumnWidth(1, 100 * 40);
                    sheet.SetColumnWidth(2, 100 * 40);
                    sheet.SetColumnWidth(3, 100 * 100);
                    sheet.SetColumnWidth(4, 100 * 50);
                    #endregion
                    #region body
                    for (int iRow = 0; iRow < dt.Rows.Count; iRow++)
                    {
                       for (int iCol = 1; iCol < dt.Columns.Count - 1; iCol++)
                       {
                          SetExcelCell(sheet, iRow + 7, iCol - 1, styleHead10, dt.Rows[iRow][iCol].ToString());
                       }
                    }
                    #endregion
                 }
              }
           }
           else
           {
              for (int k = 0; k < Departmentcount; k++)
              {
                 ISheet sheet = null;
                 DataTable dt = new DataTable();

                 sheet = workbook.CreateSheet(listCode[k].CodeDesc);
                 dt = GetReturnDetailList1(model, listCode[k].CodeDesc);//獲取查詢科別的案件
                 SetExcelCell(sheet, 1, 4, styleHead12, listCode[k].CodeDesc);
                 sheet.AddMergedRegion(new CellRangeAddress(1, 1, 4, 4));

                 #region title
                 SetExcelCell(sheet, 0, 0, styleHead12, "經辦退件明細表");
                 sheet.AddMergedRegion(new CellRangeAddress(0, 0, 0, 3));

                 //* line1
                 SetExcelCell(sheet, 1, 0, styleHead12, "收件日期：");
                 sheet.AddMergedRegion(new CellRangeAddress(1, 1, 0, 0));
                 SetExcelCell(sheet, 1, 1, styleHead12, model.ReceiveDateStart + '~' + model.ReceiveDateEnd);
                 sheet.AddMergedRegion(new CellRangeAddress(1, 1, 1, 2));

                 SetExcelCell(sheet, 2, 0, styleHead12, "發文日期：");
                 sheet.AddMergedRegion(new CellRangeAddress(2, 2, 0, 0));
                 SetExcelCell(sheet, 2, 1, styleHead12, model.SendDateStart + '~' + model.SendDateEnd);
                 sheet.AddMergedRegion(new CellRangeAddress(2, 2, 1, 2));

                 SetExcelCell(sheet, 3, 0, styleHead12, "主管放行日：");
                 sheet.AddMergedRegion(new CellRangeAddress(3, 3, 0, 0));
                 SetExcelCell(sheet, 3, 1, styleHead12, model.ApproveDateStart + '~' + model.ApproveDateEnd);
                 sheet.AddMergedRegion(new CellRangeAddress(3, 3, 1, 2));

                 SetExcelCell(sheet, 1, 3, styleHead12, "部門別/科別：");
                 sheet.AddMergedRegion(new CellRangeAddress(1, 1, 3, 3));

                 SetExcelCell(sheet, 4, 0, styleHead12, "電子發文上傳日：");
                 sheet.AddMergedRegion(new CellRangeAddress(4, 4, 0, 0));
                 SetExcelCell(sheet, 4, 1, styleHead12, model.SendUpDateStart + '~' + model.SendUpDateEnd);
                 sheet.AddMergedRegion(new CellRangeAddress(4, 4, 1, 2));

                 SetExcelCell(sheet, 5, 0, styleHead12, "發文方式：");
                 sheet.AddMergedRegion(new CellRangeAddress(5, 5, 0, 0));
                 SetExcelCell(sheet, 5, 1, styleHead12, model.SendKind);
                 sheet.AddMergedRegion(new CellRangeAddress(5, 5, 1, 2));

                 //* line2
                 SetExcelCell(sheet, 6, 0, styleHead10, "案件類別-大類");
                 sheet.AddMergedRegion(new CellRangeAddress(6, 6, 0, 0));
                 SetExcelCell(sheet, 6, 1, styleHead10, "案件編號");
                 sheet.AddMergedRegion(new CellRangeAddress(6, 6, 1, 1));
                 SetExcelCell(sheet, 6, 2, styleHead10, "處理經辦");
                 sheet.AddMergedRegion(new CellRangeAddress(6, 6, 2, 2));
                 SetExcelCell(sheet, 6, 3, styleHead10, "退件原因");
                 sheet.AddMergedRegion(new CellRangeAddress(6, 6, 3, 3));
                 #endregion
                 #region Width
                 sheet.SetColumnWidth(0, 100 * 40);
                 sheet.SetColumnWidth(1, 100 * 40);
                 sheet.SetColumnWidth(2, 100 * 40);
                 sheet.SetColumnWidth(3, 100 * 100);
                 sheet.SetColumnWidth(4, 100 * 50);
                 #endregion
                 #region body
                 for (int iRow = 0; iRow < dt.Rows.Count; iRow++)
                 {
                    for (int iCol = 1; iCol < dt.Columns.Count - 1; iCol++)
                    {
                       SetExcelCell(sheet, iRow + 7, iCol - 1, styleHead10, dt.Rows[iRow][iCol].ToString());
                    }
                 }
                 #endregion
              }
           }

           MemoryStream ms = new MemoryStream();
           workbook.Write(ms);
           ms.Flush();
           ms.Position = 0;
           workbook = null;
           return ms;
        }

        #endregion
        //20170811 RC RQ-2015-019666-020 派件至跨單位(原產生邏輯程式寫死，組織異動必出問題，改為參數) 宏祥 add end

        //#region 逾期案件明細表1
        //public MemoryStream OverDateReportExcel_NPOI1(CaseClosedQuery model)
        //{
        //    IWorkbook workbook = new HSSFWorkbook();
        //    ISheet sheet = null;
        //    ISheet sheet2 = null;
        //    ISheet sheet3 = null;

        //    DataTable dt = new DataTable();
        //    DataTable dt2 = new DataTable();
        //    DataTable dt3 = new DataTable();

        //    #region def style
        //    ICellStyle styleHead12 = workbook.CreateCellStyle();
        //    IFont font12 = workbook.CreateFont();
        //    font12.FontHeightInPoints = 12;
        //    font12.FontName = "新細明體";
        //    styleHead12.FillPattern = FillPattern.SolidForeground;
        //    styleHead12.FillForegroundColor = HSSFColor.White.Index;
        //    styleHead12.BorderTop = BorderStyle.None;
        //    styleHead12.BorderLeft = BorderStyle.None;
        //    styleHead12.BorderRight = BorderStyle.None;
        //    styleHead12.BorderBottom = BorderStyle.None;
        //    styleHead12.WrapText = true;
        //    styleHead12.Alignment = HorizontalAlignment.Center;
        //    styleHead12.VerticalAlignment = VerticalAlignment.Center;
        //    styleHead12.SetFont(font12);

        //    ICellStyle styleHead10 = workbook.CreateCellStyle();
        //    IFont font10 = workbook.CreateFont();
        //    font10.FontHeightInPoints = 10;
        //    font10.FontName = "新細明體";
        //    styleHead10.FillPattern = FillPattern.SolidForeground;
        //    styleHead10.FillForegroundColor = HSSFColor.White.Index;
        //    styleHead10.BorderTop = BorderStyle.Thin;
        //    styleHead10.BorderLeft = BorderStyle.Thin;
        //    styleHead10.BorderRight = BorderStyle.Thin;
        //    styleHead10.BorderBottom = BorderStyle.Thin;
        //    styleHead10.WrapText = true;
        //    styleHead10.Alignment = HorizontalAlignment.Left;
        //    styleHead10.VerticalAlignment = VerticalAlignment.Center;
        //    styleHead10.SetFont(font10);
        //    #endregion

        //    #region 獲取數據源(集作一科及案件資料)
        //    //獲取人員
        //    if (model.Depart == "1")//* 集作一科
        //    {
        //        sheet = workbook.CreateSheet("集作一科");
        //        dt = GetOverDateList1(model, "集作一科");//獲取查詢集作一科的案件
        //        SetExcelCell(sheet, 1, 4, styleHead12, "集作一科");
        //        sheet.AddMergedRegion(new CellRangeAddress(1, 1, 4, 4));
        //    }
        //    if (model.Depart == "2")//* 集作二科
        //    {
        //        sheet = workbook.CreateSheet("集作二科");
        //        dt = GetOverDateList1(model, "集作二科");//獲取查詢集作二科的案件
        //        SetExcelCell(sheet, 1, 4, styleHead12, "集作二科");
        //        sheet.AddMergedRegion(new CellRangeAddress(1, 1, 4, 4));
        //    }
        //    if (model.Depart == "3")//*集作三科
        //    {
        //        sheet = workbook.CreateSheet("集作三科");
        //        dt = GetOverDateList1(model, "集作三科");//獲取查詢集作三科的案件
        //        SetExcelCell(sheet, 1, 4, styleHead12, "集作三科");
        //        sheet.AddMergedRegion(new CellRangeAddress(1, 1, 4, 4));
        //    }
        //    if (model.Depart == "0")//*全部
        //    {
        //        sheet = workbook.CreateSheet("集作一科");
        //        sheet2 = workbook.CreateSheet("集作二科");
        //        sheet3 = workbook.CreateSheet("集作三科");
        //        dt = GetOverDateList1(model, "集作一科");//獲取查詢集作一科的案件
        //        dt2 = GetOverDateList1(model, "集作二科");//獲取查詢集作二科的案件
        //        dt3 = GetOverDateList1(model, "集作三科");//獲取查詢集作三科的案件
        //        SetExcelCell(sheet, 1, 4, styleHead12, "集作一科");
        //        sheet.AddMergedRegion(new CellRangeAddress(1, 1, 4, 4));
        //        SetExcelCell(sheet2, 1, 4, styleHead12, "集作二科");
        //        sheet2.AddMergedRegion(new CellRangeAddress(1, 1, 4, 4));
        //        SetExcelCell(sheet3, 1, 4, styleHead12, "集作三科");
        //        sheet3.AddMergedRegion(new CellRangeAddress(1, 1, 4, 4));
        //    }
        //    #endregion

        //    #region title
        //    SetExcelCell(sheet, 0, 0, styleHead12, "逾期案件明細表");
        //    sheet.AddMergedRegion(new CellRangeAddress(0, 0, 0, 4));

        //    //* line1
        //    SetExcelCell(sheet, 1, 0, styleHead12, "收件日期：");
        //    sheet.AddMergedRegion(new CellRangeAddress(1, 1, 0, 0));
        //    SetExcelCell(sheet, 1, 1, styleHead12, model.ReceiveDateStart + '~' + model.ReceiveDateEnd);
        //    sheet.AddMergedRegion(new CellRangeAddress(1, 1, 1, 2));

        //    SetExcelCell(sheet, 2, 0, styleHead12, "發文日期：");
        //    sheet.AddMergedRegion(new CellRangeAddress(2, 2, 0, 0));
        //    SetExcelCell(sheet, 2, 1, styleHead12, model.SendDateStart + '~' + model.SendDateEnd);
        //    sheet.AddMergedRegion(new CellRangeAddress(2, 2, 1, 2));

        //    SetExcelCell(sheet, 3, 0, styleHead12, "主管放行日：");
        //    sheet.AddMergedRegion(new CellRangeAddress(3, 3, 0, 0));
        //    SetExcelCell(sheet, 3, 1, styleHead12, model.ApproveDateStart + '~' + model.ApproveDateEnd);
        //    sheet.AddMergedRegion(new CellRangeAddress(3, 3, 1, 2));

        //    SetExcelCell(sheet, 1, 3, styleHead12, "部門別/科別:");
        //    sheet.AddMergedRegion(new CellRangeAddress(1, 1, 3, 3));

        //    SetExcelCell(sheet, 4, 0, styleHead12, "電子發文上傳日：");
        //    sheet.AddMergedRegion(new CellRangeAddress(4, 4, 0, 0));
        //    SetExcelCell(sheet, 4, 1, styleHead12, model.SendUpDateStart + '~' + model.SendUpDateEnd);
        //    sheet.AddMergedRegion(new CellRangeAddress(4, 4, 1, 2));

        //    SetExcelCell(sheet, 5, 0, styleHead12, "發文方式：");
        //    sheet.AddMergedRegion(new CellRangeAddress(5, 5, 0, 0));
        //    SetExcelCell(sheet, 5, 1, styleHead12, model.SendKind);
        //    sheet.AddMergedRegion(new CellRangeAddress(5, 5, 1, 2));

        //    //* line2
        //    SetExcelCell(sheet, 6, 0, styleHead10, "案件類別-大類");
        //    sheet.AddMergedRegion(new CellRangeAddress(6, 6, 0, 0));
        //    SetExcelCell(sheet, 6, 1, styleHead10, "案件編號");
        //    sheet.AddMergedRegion(new CellRangeAddress(6, 6, 1, 1));
        //    SetExcelCell(sheet, 6, 2, styleHead10, "經辦人員");
        //    sheet.AddMergedRegion(new CellRangeAddress(6, 6, 2, 2));
        //    SetExcelCell(sheet, 6, 3, styleHead10, "處理天數");
        //    sheet.AddMergedRegion(new CellRangeAddress(6, 6, 3, 3));
        //    SetExcelCell(sheet, 6, 4, styleHead10, "逾期原因");
        //    sheet.AddMergedRegion(new CellRangeAddress(6, 6, 4, 4));
        //    #endregion
        //    #region Width
        //    sheet.SetColumnWidth(0, 100 * 40);
        //    sheet.SetColumnWidth(1, 100 * 40);
        //    sheet.SetColumnWidth(2, 100 * 40);
        //    sheet.SetColumnWidth(3, 100 * 40);
        //    sheet.SetColumnWidth(4, 100 * 100);
        //    #endregion
        //    #region body
        //    for (int iRow = 0; iRow < dt.Rows.Count; iRow++)
        //    {
        //        for (int iCol = 0; iCol < dt.Columns.Count-2; iCol++)
        //        {
        //            SetExcelCell(sheet, iRow + 7, iCol, styleHead10, dt.Rows[iRow][iCol].ToString());
        //        }
        //    }
        //    #endregion

        //    if (model.Depart == "0")//* 全部
        //    {
        //        #region title2
        //        SetExcelCell(sheet2, 0, 0, styleHead12, "逾期案件明細表");
        //        sheet2.AddMergedRegion(new CellRangeAddress(0, 0, 0, 4));

        //        //* line1
        //        SetExcelCell(sheet2, 1, 0, styleHead12, "收件日期：");
        //        sheet2.AddMergedRegion(new CellRangeAddress(1, 1, 0, 0));
        //        SetExcelCell(sheet2, 1, 1, styleHead12, model.ReceiveDateStart + '~' + model.ReceiveDateEnd);
        //        sheet2.AddMergedRegion(new CellRangeAddress(1, 1, 1, 2));

        //        SetExcelCell(sheet2, 2, 0, styleHead12, "發文日期：");
        //        sheet2.AddMergedRegion(new CellRangeAddress(2, 2, 0, 0));
        //        SetExcelCell(sheet2, 2, 1, styleHead12, model.SendDateStart + '~' + model.SendDateEnd);
        //        sheet2.AddMergedRegion(new CellRangeAddress(2, 2, 1, 2));

        //        SetExcelCell(sheet2, 3, 0, styleHead12, "主管放行日：");
        //        sheet2.AddMergedRegion(new CellRangeAddress(3, 3, 0, 0));
        //        SetExcelCell(sheet2, 3, 1, styleHead12, model.ApproveDateStart + '~' + model.ApproveDateEnd);
        //        sheet2.AddMergedRegion(new CellRangeAddress(3, 3, 1, 2));

        //        SetExcelCell(sheet2, 1, 3, styleHead12, "部門別/科別:");
        //        sheet2.AddMergedRegion(new CellRangeAddress(1, 1, 3, 3));

        //        SetExcelCell(sheet2, 4, 0, styleHead12, "電子發文上傳日：");
        //        sheet2.AddMergedRegion(new CellRangeAddress(4, 4, 0, 0));
        //        SetExcelCell(sheet2, 4, 1, styleHead12, model.SendUpDateStart + '~' + model.SendUpDateEnd);
        //        sheet2.AddMergedRegion(new CellRangeAddress(4, 4, 1, 2));

        //        SetExcelCell(sheet2, 5, 0, styleHead12, "發文方式：");
        //        sheet2.AddMergedRegion(new CellRangeAddress(5, 5, 0, 0));
        //        SetExcelCell(sheet2, 5, 1, styleHead12, model.SendKind);
        //        sheet2.AddMergedRegion(new CellRangeAddress(5, 5, 1, 2));

        //        //* line2
        //        SetExcelCell(sheet2, 6, 0, styleHead10, "案件類別-大類");
        //        sheet2.AddMergedRegion(new CellRangeAddress(6, 6, 0, 0));
        //        SetExcelCell(sheet2, 6, 1, styleHead10, "案件編號");
        //        sheet2.AddMergedRegion(new CellRangeAddress(6, 6, 1, 1));
        //        SetExcelCell(sheet2, 6, 2, styleHead10, "經辦人員");
        //        sheet2.AddMergedRegion(new CellRangeAddress(6, 6, 2, 2));
        //        SetExcelCell(sheet2, 6, 3, styleHead10, "處理天數");
        //        sheet2.AddMergedRegion(new CellRangeAddress(6, 6, 3, 3));
        //        SetExcelCell(sheet2, 6, 4, styleHead10, "逾期原因");
        //        sheet2.AddMergedRegion(new CellRangeAddress(6, 6, 4, 4));
        //        #endregion

        //        #region title3
        //        SetExcelCell(sheet3, 0, 0, styleHead12, "逾期案件明細表");
        //        sheet3.AddMergedRegion(new CellRangeAddress(0, 0, 0, 4));

        //        //* line1
        //        SetExcelCell(sheet3, 1, 0, styleHead12, "收件日期：");
        //        sheet3.AddMergedRegion(new CellRangeAddress(1, 1, 0, 0));
        //        SetExcelCell(sheet3, 1, 1, styleHead12, model.ReceiveDateStart + '~' + model.ReceiveDateEnd);
        //        sheet3.AddMergedRegion(new CellRangeAddress(1, 1, 1, 2));

        //        SetExcelCell(sheet3, 2, 0, styleHead12, "發文日期：");
        //        sheet3.AddMergedRegion(new CellRangeAddress(1, 1, 0, 0));
        //        SetExcelCell(sheet3, 2, 1, styleHead12, model.SendDateStart + '~' + model.SendDateEnd);
        //        sheet3.AddMergedRegion(new CellRangeAddress(2, 2, 1, 2));

        //        SetExcelCell(sheet3, 3, 0, styleHead12, "主管放行日：");
        //        sheet3.AddMergedRegion(new CellRangeAddress(3, 3, 0, 0));
        //        SetExcelCell(sheet3, 3, 1, styleHead12, model.ApproveDateStart + '~' + model.ApproveDateEnd);
        //        sheet3.AddMergedRegion(new CellRangeAddress(3, 3, 1, 2));

        //        SetExcelCell(sheet3, 1, 3, styleHead12, "部門別/科別:");
        //        sheet3.AddMergedRegion(new CellRangeAddress(1, 1, 3, 3));

        //        SetExcelCell(sheet3, 4, 0, styleHead12, "電子發文上傳日：");
        //        sheet3.AddMergedRegion(new CellRangeAddress(4, 4, 0, 0));
        //        SetExcelCell(sheet3, 4, 1, styleHead12, model.SendUpDateStart + '~' + model.SendUpDateEnd);
        //        sheet3.AddMergedRegion(new CellRangeAddress(4, 4, 1, 2));

        //        SetExcelCell(sheet3, 5, 0, styleHead12, "發文方式：");
        //        sheet3.AddMergedRegion(new CellRangeAddress(5, 5, 0, 0));
        //        SetExcelCell(sheet3, 5, 1, styleHead12, model.SendKind);
        //        sheet3.AddMergedRegion(new CellRangeAddress(5, 5, 1, 2));

        //        //* line2
        //        SetExcelCell(sheet3, 6, 0, styleHead10, "案件類別-大類");
        //        sheet3.AddMergedRegion(new CellRangeAddress(6, 6, 0, 0));
        //        SetExcelCell(sheet3, 6, 1, styleHead10, "案件編號");
        //        sheet3.AddMergedRegion(new CellRangeAddress(6, 6, 1, 1));
        //        SetExcelCell(sheet3, 6, 2, styleHead10, "經辦人員");
        //        sheet3.AddMergedRegion(new CellRangeAddress(6, 6, 2, 2));
        //        SetExcelCell(sheet3, 6, 3, styleHead10, "處理天數");
        //        sheet3.AddMergedRegion(new CellRangeAddress(6, 6, 3, 3));
        //        SetExcelCell(sheet3, 6, 4, styleHead10, "逾期原因");
        //        sheet3.AddMergedRegion(new CellRangeAddress(6, 6, 4, 4));
        //        #endregion
        //        #region Width
        //        sheet2.SetColumnWidth(0, 100 * 40);
        //        sheet2.SetColumnWidth(1, 100 * 40);
        //        sheet2.SetColumnWidth(2, 100 * 40);
        //        sheet2.SetColumnWidth(3, 100 * 40);
        //        sheet2.SetColumnWidth(4, 100 * 100);
        //        #endregion
        //        #region Width
        //        sheet3.SetColumnWidth(0, 100 * 40);
        //        sheet3.SetColumnWidth(1, 100 * 40);
        //        sheet3.SetColumnWidth(2, 100 * 40);
        //        sheet3.SetColumnWidth(3, 100 * 40);
        //        sheet3.SetColumnWidth(4, 100 * 100);
        //        #endregion
        //        #region body2
        //        for (int iRow = 0; iRow < dt2.Rows.Count; iRow++)
        //        {
        //            for (int iCol = 0; iCol < dt2.Columns.Count-2; iCol++)
        //            {
        //                SetExcelCell(sheet2, iRow + 7, iCol, styleHead10, dt2.Rows[iRow][iCol].ToString());
        //            }
        //        }
        //        #endregion

        //        #region body3
        //        for (int iRow = 0; iRow < dt3.Rows.Count; iRow++)
        //        {
        //            for (int iCol = 0; iCol < dt3.Columns.Count-2; iCol++)
        //            {
        //                SetExcelCell(sheet3, iRow + 7, iCol, styleHead10, dt3.Rows[iRow][iCol].ToString());
        //            }
        //        }
        //        #endregion
        //    }

        //    MemoryStream ms = new MemoryStream();
        //    workbook.Write(ms);
        //    ms.Flush();
        //    ms.Position = 0;
        //    workbook = null;
        //    return ms;
        //}
        //#endregion

        //20170811 RC RQ-2015-019666-020 派件至跨單位(原產生邏輯程式寫死，組織異動必出問題，改為參數) 宏祥 add start
        #region 逾期案件明細表1
        public MemoryStream OverDateReportExcel_NPOI1(CaseClosedQuery model, IList<PARMCode> listCode)
        {
           IWorkbook workbook = new HSSFWorkbook();
           int Departmentcount = listCode.Count();

           #region def style
           ICellStyle styleHead12 = workbook.CreateCellStyle();
           IFont font12 = workbook.CreateFont();
           font12.FontHeightInPoints = 12;
           font12.FontName = "新細明體";
           styleHead12.FillPattern = FillPattern.SolidForeground;
           styleHead12.FillForegroundColor = HSSFColor.White.Index;
           styleHead12.BorderTop = BorderStyle.None;
           styleHead12.BorderLeft = BorderStyle.None;
           styleHead12.BorderRight = BorderStyle.None;
           styleHead12.BorderBottom = BorderStyle.None;
           styleHead12.WrapText = true;
           styleHead12.Alignment = HorizontalAlignment.Center;
           styleHead12.VerticalAlignment = VerticalAlignment.Center;
           styleHead12.SetFont(font12);

           ICellStyle styleHead10 = workbook.CreateCellStyle();
           IFont font10 = workbook.CreateFont();
           font10.FontHeightInPoints = 10;
           font10.FontName = "新細明體";
           styleHead10.FillPattern = FillPattern.SolidForeground;
           styleHead10.FillForegroundColor = HSSFColor.White.Index;
           styleHead10.BorderTop = BorderStyle.Thin;
           styleHead10.BorderLeft = BorderStyle.Thin;
           styleHead10.BorderRight = BorderStyle.Thin;
           styleHead10.BorderBottom = BorderStyle.Thin;
           styleHead10.WrapText = true;
           styleHead10.Alignment = HorizontalAlignment.Left;
           styleHead10.VerticalAlignment = VerticalAlignment.Center;
           styleHead10.SetFont(font10);
           #endregion

           //判斷科別搜尋條件
           if (model.Depart != "0")
           {
              for (int k = 0; k < Departmentcount; k++)
              {
                 if (model.Depart == (k + 1).ToString())
                 {
                    ISheet sheet = null;
                    DataTable dt = new DataTable();

                    sheet = workbook.CreateSheet(listCode[k].CodeDesc);
                    dt = GetOverDateList1(model, listCode[k].CodeDesc);//獲取查詢科別的案件
                    SetExcelCell(sheet, 1, 4, styleHead12, listCode[k].CodeDesc);
                    sheet.AddMergedRegion(new CellRangeAddress(1, 1, 4, 4));

                    #region title
                    SetExcelCell(sheet, 0, 0, styleHead12, "逾期案件明細表");
                    sheet.AddMergedRegion(new CellRangeAddress(0, 0, 0, 4));

                    //* line1
                    SetExcelCell(sheet, 1, 0, styleHead12, "收件日期：");
                    sheet.AddMergedRegion(new CellRangeAddress(1, 1, 0, 0));
                    SetExcelCell(sheet, 1, 1, styleHead12, model.ReceiveDateStart + '~' + model.ReceiveDateEnd);
                    sheet.AddMergedRegion(new CellRangeAddress(1, 1, 1, 2));

                    SetExcelCell(sheet, 2, 0, styleHead12, "發文日期：");
                    sheet.AddMergedRegion(new CellRangeAddress(2, 2, 0, 0));
                    SetExcelCell(sheet, 2, 1, styleHead12, model.SendDateStart + '~' + model.SendDateEnd);
                    sheet.AddMergedRegion(new CellRangeAddress(2, 2, 1, 2));

                    SetExcelCell(sheet, 3, 0, styleHead12, "主管放行日：");
                    sheet.AddMergedRegion(new CellRangeAddress(3, 3, 0, 0));
                    SetExcelCell(sheet, 3, 1, styleHead12, model.ApproveDateStart + '~' + model.ApproveDateEnd);
                    sheet.AddMergedRegion(new CellRangeAddress(3, 3, 1, 2));

                    SetExcelCell(sheet, 1, 3, styleHead12, "部門別/科別:");
                    sheet.AddMergedRegion(new CellRangeAddress(1, 1, 3, 3));

                    SetExcelCell(sheet, 4, 0, styleHead12, "電子發文上傳日：");
                    sheet.AddMergedRegion(new CellRangeAddress(4, 4, 0, 0));
                    SetExcelCell(sheet, 4, 1, styleHead12, model.SendUpDateStart + '~' + model.SendUpDateEnd);
                    sheet.AddMergedRegion(new CellRangeAddress(4, 4, 1, 2));

                    SetExcelCell(sheet, 5, 0, styleHead12, "發文方式：");
                    sheet.AddMergedRegion(new CellRangeAddress(5, 5, 0, 0));
                    SetExcelCell(sheet, 5, 1, styleHead12, model.SendKind);
                    sheet.AddMergedRegion(new CellRangeAddress(5, 5, 1, 2));

                    //* line2
                    SetExcelCell(sheet, 6, 0, styleHead10, "案件類別-大類");
                    sheet.AddMergedRegion(new CellRangeAddress(6, 6, 0, 0));
                    SetExcelCell(sheet, 6, 1, styleHead10, "案件編號");
                    sheet.AddMergedRegion(new CellRangeAddress(6, 6, 1, 1));
                    SetExcelCell(sheet, 6, 2, styleHead10, "經辦人員");
                    sheet.AddMergedRegion(new CellRangeAddress(6, 6, 2, 2));
                    SetExcelCell(sheet, 6, 3, styleHead10, "處理天數");
                    sheet.AddMergedRegion(new CellRangeAddress(6, 6, 3, 3));
                    SetExcelCell(sheet, 6, 4, styleHead10, "逾期原因");
                    sheet.AddMergedRegion(new CellRangeAddress(6, 6, 4, 4));
                    #endregion
                    #region Width
                    sheet.SetColumnWidth(0, 100 * 40);
                    sheet.SetColumnWidth(1, 100 * 40);
                    sheet.SetColumnWidth(2, 100 * 40);
                    sheet.SetColumnWidth(3, 100 * 40);
                    sheet.SetColumnWidth(4, 100 * 100);
                    #endregion
                    #region body
                    for (int iRow = 0; iRow < dt.Rows.Count; iRow++)
                    {
                       for (int iCol = 0; iCol < dt.Columns.Count - 2; iCol++)
                       {
                          SetExcelCell(sheet, iRow + 7, iCol, styleHead10, dt.Rows[iRow][iCol].ToString());
                       }
                    }
                    #endregion
                 }
              }
           }
           else
           {
              for (int k = 0; k < Departmentcount; k++)
              {
                 ISheet sheet = null;
                 DataTable dt = new DataTable();

                 sheet = workbook.CreateSheet(listCode[k].CodeDesc);
                 dt = GetOverDateList1(model, listCode[k].CodeDesc);//獲取查詢科別的案件
                 SetExcelCell(sheet, 1, 4, styleHead12, listCode[k].CodeDesc);
                 sheet.AddMergedRegion(new CellRangeAddress(1, 1, 4, 4));

                 #region title
                 SetExcelCell(sheet, 0, 0, styleHead12, "逾期案件明細表");
                 sheet.AddMergedRegion(new CellRangeAddress(0, 0, 0, 4));

                 //* line1
                 SetExcelCell(sheet, 1, 0, styleHead12, "收件日期：");
                 sheet.AddMergedRegion(new CellRangeAddress(1, 1, 0, 0));
                 SetExcelCell(sheet, 1, 1, styleHead12, model.ReceiveDateStart + '~' + model.ReceiveDateEnd);
                 sheet.AddMergedRegion(new CellRangeAddress(1, 1, 1, 2));

                 SetExcelCell(sheet, 2, 0, styleHead12, "發文日期：");
                 sheet.AddMergedRegion(new CellRangeAddress(2, 2, 0, 0));
                 SetExcelCell(sheet, 2, 1, styleHead12, model.SendDateStart + '~' + model.SendDateEnd);
                 sheet.AddMergedRegion(new CellRangeAddress(2, 2, 1, 2));

                 SetExcelCell(sheet, 3, 0, styleHead12, "主管放行日：");
                 sheet.AddMergedRegion(new CellRangeAddress(3, 3, 0, 0));
                 SetExcelCell(sheet, 3, 1, styleHead12, model.ApproveDateStart + '~' + model.ApproveDateEnd);
                 sheet.AddMergedRegion(new CellRangeAddress(3, 3, 1, 2));

                 SetExcelCell(sheet, 1, 3, styleHead12, "部門別/科別:");
                 sheet.AddMergedRegion(new CellRangeAddress(1, 1, 3, 3));

                 SetExcelCell(sheet, 4, 0, styleHead12, "電子發文上傳日：");
                 sheet.AddMergedRegion(new CellRangeAddress(4, 4, 0, 0));
                 SetExcelCell(sheet, 4, 1, styleHead12, model.SendUpDateStart + '~' + model.SendUpDateEnd);
                 sheet.AddMergedRegion(new CellRangeAddress(4, 4, 1, 2));

                 SetExcelCell(sheet, 5, 0, styleHead12, "發文方式：");
                 sheet.AddMergedRegion(new CellRangeAddress(5, 5, 0, 0));
                 SetExcelCell(sheet, 5, 1, styleHead12, model.SendKind);
                 sheet.AddMergedRegion(new CellRangeAddress(5, 5, 1, 2));

                 //* line2
                 SetExcelCell(sheet, 6, 0, styleHead10, "案件類別-大類");
                 sheet.AddMergedRegion(new CellRangeAddress(6, 6, 0, 0));
                 SetExcelCell(sheet, 6, 1, styleHead10, "案件編號");
                 sheet.AddMergedRegion(new CellRangeAddress(6, 6, 1, 1));
                 SetExcelCell(sheet, 6, 2, styleHead10, "經辦人員");
                 sheet.AddMergedRegion(new CellRangeAddress(6, 6, 2, 2));
                 SetExcelCell(sheet, 6, 3, styleHead10, "處理天數");
                 sheet.AddMergedRegion(new CellRangeAddress(6, 6, 3, 3));
                 SetExcelCell(sheet, 6, 4, styleHead10, "逾期原因");
                 sheet.AddMergedRegion(new CellRangeAddress(6, 6, 4, 4));
                 #endregion
                 #region Width
                 sheet.SetColumnWidth(0, 100 * 40);
                 sheet.SetColumnWidth(1, 100 * 40);
                 sheet.SetColumnWidth(2, 100 * 40);
                 sheet.SetColumnWidth(3, 100 * 40);
                 sheet.SetColumnWidth(4, 100 * 100);
                 #endregion
                 #region body
                 for (int iRow = 0; iRow < dt.Rows.Count; iRow++)
                 {
                    for (int iCol = 0; iCol < dt.Columns.Count - 2; iCol++)
                    {
                       SetExcelCell(sheet, iRow + 7, iCol, styleHead10, dt.Rows[iRow][iCol].ToString());
                    }
                 }
                 #endregion
              }
           }

           MemoryStream ms = new MemoryStream();
           workbook.Write(ms);
           ms.Flush();
           ms.Position = 0;
           workbook = null;
           return ms;
        }
        #endregion
        //20170811 RC RQ-2015-019666-020 派件至跨單位(原產生邏輯程式寫死，組織異動必出問題，改為參數) 宏祥 add end

        //#region 電子發文明細表
        //public MemoryStream ListReportExcel_Send1(CaseClosedQuery model)
        //{
        //    IWorkbook workbook = new HSSFWorkbook();
        //    ISheet sheet = null;
        //    ISheet sheet2 = null;
        //    ISheet sheet3 = null;

        //    DataTable dt = new DataTable();
        //    DataTable dt2 = new DataTable();
        //    DataTable dt3 = new DataTable();

        //    #region def style
        //    ICellStyle styleHead12 = workbook.CreateCellStyle();
        //    IFont font12 = workbook.CreateFont();
        //    font12.FontHeightInPoints = 12;
        //    font12.FontName = "新細明體";
        //    styleHead12.FillPattern = FillPattern.SolidForeground;
        //    styleHead12.FillForegroundColor = HSSFColor.White.Index;
        //    styleHead12.BorderTop = BorderStyle.None;
        //    styleHead12.BorderLeft = BorderStyle.None;
        //    styleHead12.BorderRight = BorderStyle.None;
        //    styleHead12.BorderBottom = BorderStyle.None;
        //    styleHead12.WrapText = true;
        //    styleHead12.Alignment = HorizontalAlignment.Center;
        //    styleHead12.VerticalAlignment = VerticalAlignment.Center;
        //    styleHead12.SetFont(font12);

        //    ICellStyle styleHead10 = workbook.CreateCellStyle();
        //    IFont font10 = workbook.CreateFont();
        //    font10.FontHeightInPoints = 10;
        //    font10.FontName = "新細明體";
        //    styleHead10.FillPattern = FillPattern.SolidForeground;
        //    styleHead10.FillForegroundColor = HSSFColor.White.Index;
        //    styleHead10.BorderTop = BorderStyle.Thin;
        //    styleHead10.BorderLeft = BorderStyle.Thin;
        //    styleHead10.BorderRight = BorderStyle.Thin;
        //    styleHead10.BorderBottom = BorderStyle.Thin;
        //    styleHead10.WrapText = true;
        //    styleHead10.Alignment = HorizontalAlignment.Left;
        //    styleHead10.VerticalAlignment = VerticalAlignment.Center;
        //    styleHead10.SetFont(font10);
        //    #endregion

        //    #region 獲取數據源(集作一科及案件資料)
        //    //獲取人員
        //    if (model.Depart == "1" || model.Depart == "0")//* 集作一科
        //    {
        //        sheet = workbook.CreateSheet("集作一科");
        //        dt = GetCaseSend1(model, "集作一科");//獲取查詢集作一科的案件
        //        SetExcelCell(sheet, 1, 6, styleHead12, "集作一科");
        //        sheet.AddMergedRegion(new CellRangeAddress(1, 1, 6, 7));
        //    }
        //    if (model.Depart == "2")//* 集作二科
        //    {
        //        sheet = workbook.CreateSheet("集作二科");
        //        dt = GetCaseSend1(model, "集作二科");//獲取查詢集作二科的案件
        //        SetExcelCell(sheet, 1, 6, styleHead12, "集作二科");
        //        sheet.AddMergedRegion(new CellRangeAddress(1, 1, 6, 7));
        //    }
        //    if (model.Depart == "3")//*集作三科
        //    {
        //        sheet = workbook.CreateSheet("集作三科");
        //        dt = GetCaseSend1(model, "集作三科");//獲取查詢集作三科的案件
        //        SetExcelCell(sheet, 1, 6, styleHead12, "集作三科");
        //        sheet.AddMergedRegion(new CellRangeAddress(1, 1, 6, 7));
        //    }
        //    if (model.Depart == "0")//*全部
        //    {
        //        sheet2 = workbook.CreateSheet("集作二科");
        //        sheet3 = workbook.CreateSheet("集作三科");
        //        dt2 = GetCaseSend1(model, "集作二科");//獲取查詢集作二科的案件
        //        dt3 = GetCaseSend1(model, "集作三科");//獲取查詢集作三科的案件
        //        SetExcelCell(sheet2, 1, 6, styleHead12, "集作二科");
        //        sheet2.AddMergedRegion(new CellRangeAddress(1, 1, 6, 7));
        //        SetExcelCell(sheet3, 1, 6, styleHead12, "集作三科");
        //        sheet3.AddMergedRegion(new CellRangeAddress(1, 1, 6, 7));
        //    }
        //    #endregion

        //    #region title
        //    SetExcelCell(sheet, 0, 0, styleHead12, "電子發文明細表");
        //    sheet.AddMergedRegion(new CellRangeAddress(0, 0, 0, 8));

        //    //* line1
        //    SetExcelCell(sheet, 1, 0, styleHead12, "");
        //    //sheet.AddMergedRegion(new CellRangeAddress(1, 1, 0, 0));
        //    SetExcelCell(sheet, 1, 1, styleHead12, "");
        //    //sheet.AddMergedRegion(new CellRangeAddress(1, 1, 1, 2));
        //    SetExcelCell(sheet, 1, 4, styleHead12, "部門別/科別：");
        //    sheet.AddMergedRegion(new CellRangeAddress(1, 1, 4, 5));

        //    SetExcelCell(sheet, 2, 0, styleHead12, "電子發文上傳日：");
        //    sheet.AddMergedRegion(new CellRangeAddress(2, 2, 0, 0));
        //    SetExcelCell(sheet, 2, 1, styleHead12, model.SendUpDateStart + '~' + model.SendUpDateEnd);
        //    sheet.AddMergedRegion(new CellRangeAddress(2, 2, 1, 2));
        //    SetExcelCell(sheet, 3, 0, styleHead12, "發文方式");
        //    sheet.AddMergedRegion(new CellRangeAddress(3, 3, 0, 0));
        //    SetExcelCell(sheet, 3, 1, styleHead12, model.SendKind);
        //    sheet.AddMergedRegion(new CellRangeAddress(3, 3, 1, 2));

        //    //* line2
        //    SetExcelCell(sheet, 4, 0, styleHead10, "發文字號");
        //    sheet.AddMergedRegion(new CellRangeAddress(4, 4, 0, 0));
        //    SetExcelCell(sheet, 4, 1, styleHead10, "案件編號");
        //    sheet.AddMergedRegion(new CellRangeAddress(4, 4, 1, 1));
        //    SetExcelCell(sheet, 4, 2, styleHead10, "受文者");
        //    sheet.AddMergedRegion(new CellRangeAddress(4, 4, 2, 2));
        //    SetExcelCell(sheet, 4, 3, styleHead10, "副本");
        //    sheet.AddMergedRegion(new CellRangeAddress(4, 4, 3, 3));
        //    SetExcelCell(sheet, 4, 4, styleHead10, "經辦");
        //    sheet.AddMergedRegion(new CellRangeAddress(4, 4, 4, 4));
        //    SetExcelCell(sheet, 4, 5, styleHead10, "案件大類");
        //    sheet.AddMergedRegion(new CellRangeAddress(4, 4, 5, 5));
        //    SetExcelCell(sheet, 4, 6, styleHead10, "案件細類");
        //    sheet.AddMergedRegion(new CellRangeAddress(4, 4, 6, 6));
        //    SetExcelCell(sheet, 4, 7, styleHead10, "放行主管");
        //    sheet.AddMergedRegion(new CellRangeAddress(4, 4, 7, 7));
        //    SetExcelCell(sheet, 4, 8, styleHead10, "逾期註記");
        //    sheet.AddMergedRegion(new CellRangeAddress(4, 4, 8, 8));
        //    SetExcelCell(sheet, 4, 9, styleHead10, "電子發文上傳日");
        //    sheet.AddMergedRegion(new CellRangeAddress(4, 4, 9, 9));
        //    //SetExcelCell(sheet, 4, 10, styleHead10, "發文方式");
        //    //sheet.AddMergedRegion(new CellRangeAddress(4, 4, 10, 10));

        //    SetExcelCell(sheet, dt.Rows.Count + 6, 0, styleHead10, "案件類別");
        //    sheet.AddMergedRegion(new CellRangeAddress(dt.Rows.Count + 6, dt.Rows.Count + 6, 0, 0));
        //    SetExcelCell(sheet, dt.Rows.Count + 6, 1, styleHead10, "扣押");
        //    sheet.AddMergedRegion(new CellRangeAddress(dt.Rows.Count + 6, dt.Rows.Count + 6, 1, 1));
        //    SetExcelCell(sheet, dt.Rows.Count + 6, 2, styleHead10, "支付");
        //    sheet.AddMergedRegion(new CellRangeAddress(dt.Rows.Count + 6, dt.Rows.Count + 6, 2, 2));
        //    SetExcelCell(sheet, dt.Rows.Count + 6, 3, styleHead10, "扣押並支付");
        //    sheet.AddMergedRegion(new CellRangeAddress(dt.Rows.Count + 6, dt.Rows.Count + 6, 3, 3));
        //    SetExcelCell(sheet, dt.Rows.Count + 6, 4, styleHead10, "外來文案件");
        //    sheet.AddMergedRegion(new CellRangeAddress(dt.Rows.Count + 6, dt.Rows.Count + 6, 4, 4));
        //    SetExcelCell(sheet, dt.Rows.Count + 6, 5, styleHead10, "合計");
        //    sheet.AddMergedRegion(new CellRangeAddress(dt.Rows.Count + 6, dt.Rows.Count + 6, 5, 5));

        //    SetExcelCell(sheet, dt.Rows.Count + 7, 0, styleHead10, "件數");
        //    sheet.AddMergedRegion(new CellRangeAddress(dt.Rows.Count + 7, dt.Rows.Count + 7, 0, 0));

        //    SetExcelCell(sheet, dt.Rows.Count + 9, 4, styleHead12, "覆核主管");
        //    sheet.AddMergedRegion(new CellRangeAddress(dt.Rows.Count + 9, dt.Rows.Count + 9, 4, 4));
        //    SetExcelCell(sheet, dt.Rows.Count + 9, 6, styleHead12, "覆核人員");
        //    sheet.AddMergedRegion(new CellRangeAddress(dt.Rows.Count + 9, dt.Rows.Count + 9, 6, 6));

        //    for (int i = dt.Rows.Count + 7; i < dt.Rows.Count + 8; i++)//*初始表格賦初值 
        //    {
        //        for (int j = 0; j < 5; j++)
        //        {
        //            SetExcelCell(sheet, i, j + 1, styleHead10, "0");
        //            sheet.AddMergedRegion(new CellRangeAddress(i, i, j + 1, j + 1));
        //        }
        //    }
        //    #endregion
        //    #region Width
        //    sheet.SetColumnWidth(0, 100 * 100);
        //    sheet.SetColumnWidth(1, 100 * 40);
        //    sheet.SetColumnWidth(2, 100 * 100);
        //    sheet.SetColumnWidth(3, 100 * 100);
        //    sheet.SetColumnWidth(4, 100 * 30);
        //    sheet.SetColumnWidth(5, 100 * 30);
        //    sheet.SetColumnWidth(6, 100 * 30);
        //    sheet.SetColumnWidth(7, 100 * 40);
        //    sheet.SetColumnWidth(8, 100 * 30);
        //    sheet.SetColumnWidth(9, 100 * 100);
        //    //sheet.SetColumnWidth(9, 100 * 40);
        //    //sheet.SetColumnWidth(10, 100 * 40);
        //    #endregion
        //    #region body
        //    for (int iRow = 0; iRow < dt.Rows.Count; iRow++)
        //    {
        //        for (int iCol = 1; iCol < dt.Columns.Count - 2; iCol++)
        //        {
        //            SetExcelCell(sheet, iRow + 5, iCol - 1, styleHead10, dt.Rows[iRow][iCol].ToString());
        //        }
        //    }
        //    //總計
        //    int rows = dt.Rows.Count + 7;
        //    DataRow[] drkouya = dt.Select(" CaseKind2 = '扣押'");
        //    SetExcelCell(sheet, rows, 1, styleHead10, drkouya.Length.ToString());
        //    DataRow[] drzhifu = dt.Select(" CaseKind2 = '支付'");
        //    SetExcelCell(sheet, rows, 2, styleHead10, drzhifu.Length.ToString());
        //    DataRow[] drkouyaandPay = dt.Select(" CaseKind2 = '扣押並支付'");
        //    SetExcelCell(sheet, rows, 3, styleHead10, drkouyaandPay.Length.ToString());
        //    DataRow[] drWai = dt.Select(" CaseKind = '外來文案件'");
        //    SetExcelCell(sheet, rows, 4, styleHead10, drWai.Length.ToString());
        //    SetExcelCell(sheet, rows, 5, styleHead10, dt.Rows.Count.ToString());
        //    #endregion

        //    if (model.Depart == "0")//* 全部
        //    {
        //        #region title2
        //        SetExcelCell(sheet2, 0, 0, styleHead12, "電子發文明細表");
        //        sheet2.AddMergedRegion(new CellRangeAddress(0, 0, 0, 8));

        //        //* line1
        //        SetExcelCell(sheet2, 1, 0, styleHead12, "");
        //        //sheet2.AddMergedRegion(new CellRangeAddress(1, 1, 0, 0));
        //        SetExcelCell(sheet2, 1, 1, styleHead12, "");
        //        //sheet2.AddMergedRegion(new CellRangeAddress(1, 1, 1, 2));
        //        SetExcelCell(sheet2, 1, 4, styleHead12, "部門別/科別：");
        //        sheet2.AddMergedRegion(new CellRangeAddress(1, 1, 4, 5));


        //        SetExcelCell(sheet2, 2, 0, styleHead12, "電子發文上傳日：");
        //        sheet2.AddMergedRegion(new CellRangeAddress(2, 2, 0, 0));
        //        SetExcelCell(sheet2, 2, 1, styleHead12, model.SendUpDateStart + '~' + model.SendUpDateEnd);
        //        sheet2.AddMergedRegion(new CellRangeAddress(2, 2, 1, 2));
        //        SetExcelCell(sheet2, 3, 0, styleHead12, "發文方式");
        //        sheet2.AddMergedRegion(new CellRangeAddress(3, 3, 0, 0));
        //        SetExcelCell(sheet2, 3, 1, styleHead12, model.SendKind);
        //        sheet2.AddMergedRegion(new CellRangeAddress(3, 3, 1, 2));

        //        //* line2
        //        SetExcelCell(sheet2, 4, 0, styleHead10, "發文字號");
        //        sheet2.AddMergedRegion(new CellRangeAddress(4, 4, 0, 0));
        //        SetExcelCell(sheet2, 4, 1, styleHead10, "案件編號");
        //        sheet2.AddMergedRegion(new CellRangeAddress(4, 4, 1, 1));
        //        SetExcelCell(sheet2, 4, 2, styleHead10, "受文者");
        //        sheet2.AddMergedRegion(new CellRangeAddress(4, 4, 2, 2));
        //        SetExcelCell(sheet2, 4, 3, styleHead10, "副本");
        //        sheet2.AddMergedRegion(new CellRangeAddress(4, 4, 3, 3));
        //        SetExcelCell(sheet2, 4, 4, styleHead10, "經辦");
        //        sheet2.AddMergedRegion(new CellRangeAddress(4, 4, 4, 4));
        //        SetExcelCell(sheet2, 4, 5, styleHead10, "案件大類");
        //        sheet2.AddMergedRegion(new CellRangeAddress(4, 4, 5, 5));
        //        SetExcelCell(sheet2, 4, 6, styleHead10, "案件細類");
        //        sheet2.AddMergedRegion(new CellRangeAddress(4, 4, 6, 6));
        //        SetExcelCell(sheet2, 4, 7, styleHead10, "放行主管");
        //        sheet2.AddMergedRegion(new CellRangeAddress(4, 4, 7, 7));
        //        SetExcelCell(sheet2, 4, 8, styleHead10, "逾期註記");
        //        sheet2.AddMergedRegion(new CellRangeAddress(4, 4, 8, 8));
        //        SetExcelCell(sheet2, 4, 9, styleHead10, "電子發文上傳日");
        //        sheet2.AddMergedRegion(new CellRangeAddress(4, 4, 9, 9));
        //        //SetExcelCell(sheet2, 4, 10, styleHead10, "電子發文上傳日");
        //        //sheet2.AddMergedRegion(new CellRangeAddress(4, 4, 10, 10));

        //        int dtrows2 = dt2.Rows.Count + 6;
        //        SetExcelCell(sheet2, dtrows2, 0, styleHead10, "案件類別");
        //        sheet2.AddMergedRegion(new CellRangeAddress(dtrows2, dtrows2, 0, 0));
        //        SetExcelCell(sheet2, dtrows2, 1, styleHead10, "扣押");
        //        sheet2.AddMergedRegion(new CellRangeAddress(dtrows2, dtrows2, 1, 1));
        //        SetExcelCell(sheet2, dtrows2, 2, styleHead10, "支付");
        //        sheet2.AddMergedRegion(new CellRangeAddress(dtrows2, dtrows2, 2, 2));
        //        SetExcelCell(sheet2, dtrows2, 3, styleHead10, "扣押並支付");
        //        sheet2.AddMergedRegion(new CellRangeAddress(dtrows2, dtrows2, 3, 3));
        //        SetExcelCell(sheet2, dtrows2, 4, styleHead10, "外來文案件");
        //        sheet2.AddMergedRegion(new CellRangeAddress(dtrows2, dtrows2, 4, 4));
        //        SetExcelCell(sheet2, dtrows2, 5, styleHead10, "合計");
        //        sheet2.AddMergedRegion(new CellRangeAddress(dtrows2, dtrows2, 5, 5));

        //        SetExcelCell(sheet2, dt2.Rows.Count + 7, 0, styleHead10, "件數");
        //        sheet2.AddMergedRegion(new CellRangeAddress(dt2.Rows.Count + 7, dt2.Rows.Count + 7, 0, 0));

        //        SetExcelCell(sheet2, dt2.Rows.Count + 9, 4, styleHead12, "覆核主管");
        //        sheet2.AddMergedRegion(new CellRangeAddress(dt2.Rows.Count + 9, dt2.Rows.Count + 9, 4, 4));
        //        SetExcelCell(sheet2, dt2.Rows.Count + 9, 6, styleHead12, "覆核人員");
        //        sheet2.AddMergedRegion(new CellRangeAddress(dt2.Rows.Count + 9, dt2.Rows.Count + 9, 6, 6));

        //        for (int i = dt2.Rows.Count + 7; i < dt2.Rows.Count + 8; i++)//*初始表格賦初值 
        //        {
        //            for (int j = 0; j < 5; j++)
        //            {
        //                SetExcelCell(sheet2, i, j + 1, styleHead10, "0");
        //                sheet2.AddMergedRegion(new CellRangeAddress(i, i, j + 1, j + 1));
        //            }
        //        }
        //        #endregion
        //        #region title3
        //        SetExcelCell(sheet3, 0, 0, styleHead12, "電子發文明細表");
        //        sheet3.AddMergedRegion(new CellRangeAddress(0, 0, 0, 8));

        //        //* line1
        //        SetExcelCell(sheet3, 1, 0, styleHead12, "");
        //        //sheet3.AddMergedRegion(new CellRangeAddress(1, 1, 0, 0));
        //        SetExcelCell(sheet3, 1, 1, styleHead12, "");
        //        //sheet3.AddMergedRegion(new CellRangeAddress(1, 1, 1, 2));
        //        SetExcelCell(sheet3, 1, 4, styleHead12, "部門別/科別：");
        //        sheet3.AddMergedRegion(new CellRangeAddress(1, 1, 4, 5));

        //        SetExcelCell(sheet3, 2, 0, styleHead12, "電子發文上傳日：");
        //        sheet3.AddMergedRegion(new CellRangeAddress(2, 2, 0, 0));
        //        SetExcelCell(sheet3, 2, 1, styleHead12, model.SendUpDateStart + '~' + model.SendUpDateEnd);
        //        sheet3.AddMergedRegion(new CellRangeAddress(2, 2, 1, 2));
        //        SetExcelCell(sheet3, 3, 0, styleHead12, "發文方式");
        //        sheet3.AddMergedRegion(new CellRangeAddress(3, 3, 0, 0));
        //        SetExcelCell(sheet3, 3, 1, styleHead12, model.SendKind);
        //        sheet3.AddMergedRegion(new CellRangeAddress(3, 3, 1, 2));

        //        //* line2
        //        SetExcelCell(sheet3, 4, 0, styleHead10, "發文字號");
        //        sheet3.AddMergedRegion(new CellRangeAddress(4, 4, 0, 0));
        //        SetExcelCell(sheet3, 4, 1, styleHead10, "案件編號");
        //        sheet3.AddMergedRegion(new CellRangeAddress(4, 4, 1, 1));
        //        SetExcelCell(sheet3, 4, 2, styleHead10, "受文者");
        //        sheet3.AddMergedRegion(new CellRangeAddress(4, 4, 2, 2));
        //        SetExcelCell(sheet3, 4, 3, styleHead10, "副本");
        //        sheet3.AddMergedRegion(new CellRangeAddress(4, 4, 3, 3));
        //        SetExcelCell(sheet3, 4, 4, styleHead10, "經辦");
        //        sheet3.AddMergedRegion(new CellRangeAddress(4, 4, 4, 4));
        //        SetExcelCell(sheet3, 4, 5, styleHead10, "案件大類");
        //        sheet3.AddMergedRegion(new CellRangeAddress(4, 4, 5, 5));
        //        SetExcelCell(sheet3, 4, 6, styleHead10, "案件細類");
        //        sheet3.AddMergedRegion(new CellRangeAddress(4, 4, 6, 6));
        //        SetExcelCell(sheet3, 4, 7, styleHead10, "放行主管");
        //        sheet3.AddMergedRegion(new CellRangeAddress(4, 4, 7, 7));
        //        SetExcelCell(sheet3, 4, 8, styleHead10, "逾期註記");
        //        sheet3.AddMergedRegion(new CellRangeAddress(4, 4, 8, 8));
        //        SetExcelCell(sheet3, 4, 9, styleHead10, "電子發文上傳日");
        //        sheet3.AddMergedRegion(new CellRangeAddress(4, 4, 9, 9));
        //        //SetExcelCell(sheet3, 4, 10, styleHead10, "電子發文上傳日");
        //        //sheet3.AddMergedRegion(new CellRangeAddress(4, 4, 10, 10));

        //        int countRows = dt3.Rows.Count + 6;
        //        SetExcelCell(sheet3, countRows, 0, styleHead10, "案件類別");
        //        sheet3.AddMergedRegion(new CellRangeAddress(countRows, countRows, 0, 0));
        //        SetExcelCell(sheet3, countRows, 1, styleHead10, "扣押");
        //        sheet3.AddMergedRegion(new CellRangeAddress(countRows, countRows, 1, 1));
        //        SetExcelCell(sheet3, countRows, 2, styleHead10, "支付");
        //        sheet3.AddMergedRegion(new CellRangeAddress(countRows, countRows, 2, 2));
        //        SetExcelCell(sheet3, countRows, 3, styleHead10, "扣押並支付");
        //        sheet3.AddMergedRegion(new CellRangeAddress(countRows, countRows, 3, 3));
        //        SetExcelCell(sheet3, countRows, 4, styleHead10, "外來文案件");
        //        sheet3.AddMergedRegion(new CellRangeAddress(countRows, countRows, 4, 4));
        //        SetExcelCell(sheet3, countRows, 5, styleHead10, "合計");
        //        sheet3.AddMergedRegion(new CellRangeAddress(countRows, countRows, 5, 5));

        //        int countRows1 = dt3.Rows.Count + 7;
        //        SetExcelCell(sheet3, countRows1, 0, styleHead10, "件數");
        //        sheet3.AddMergedRegion(new CellRangeAddress(countRows1, countRows1, 0, 0));

        //        int countRows2 = dt3.Rows.Count + 9;
        //        SetExcelCell(sheet3, countRows2, 4, styleHead12, "覆核主管");
        //        sheet3.AddMergedRegion(new CellRangeAddress(countRows2, countRows2, 4, 4));
        //        SetExcelCell(sheet3, countRows2, 6, styleHead12, "覆核人員");
        //        sheet3.AddMergedRegion(new CellRangeAddress(countRows2, countRows2, 6, 6));

        //        for (int i = dt3.Rows.Count + 7; i < dt3.Rows.Count + 8; i++)//*初始表格賦初值 
        //        {
        //            for (int j = 0; j < 5; j++)
        //            {
        //                SetExcelCell(sheet3, i, j + 1, styleHead10, "0");
        //                sheet3.AddMergedRegion(new CellRangeAddress(i, i, j + 1, j + 1));
        //            }
        //        }
        //        #endregion
        //        #region Width2
        //        sheet2.SetColumnWidth(0, 100 * 100);
        //        sheet2.SetColumnWidth(1, 100 * 40);
        //        sheet2.SetColumnWidth(2, 100 * 100);
        //        sheet2.SetColumnWidth(3, 100 * 100);
        //        sheet2.SetColumnWidth(4, 100 * 30);
        //        sheet2.SetColumnWidth(5, 100 * 30);
        //        sheet2.SetColumnWidth(6, 100 * 30);
        //        sheet2.SetColumnWidth(7, 100 * 40);
        //        sheet2.SetColumnWidth(8, 100 * 30);
        //        sheet2.SetColumnWidth(9, 100 * 100);
        //        //sheet2.SetColumnWidth(10, 100 * 40);
        //        #endregion
        //        #region Width3
        //        sheet3.SetColumnWidth(0, 100 * 100);
        //        sheet3.SetColumnWidth(1, 100 * 40);
        //        sheet3.SetColumnWidth(2, 100 * 100);
        //        sheet3.SetColumnWidth(3, 100 * 100);
        //        sheet3.SetColumnWidth(4, 100 * 30);
        //        sheet3.SetColumnWidth(5, 100 * 30);
        //        sheet3.SetColumnWidth(6, 100 * 30);
        //        sheet3.SetColumnWidth(7, 100 * 40);
        //        sheet3.SetColumnWidth(8, 100 * 30);
        //        sheet3.SetColumnWidth(9, 100 * 100);
        //        //sheet3.SetColumnWidth(10, 100 * 40);
        //        #endregion
        //        #region body2
        //        for (int iRow = 0; iRow < dt2.Rows.Count; iRow++)
        //        {
        //            for (int iCol = 1; iCol < dt2.Columns.Count - 2; iCol++)
        //            {
        //                SetExcelCell(sheet2, iRow + 5, iCol - 1, styleHead10, dt2.Rows[iRow][iCol].ToString());
        //            }
        //        }
        //        //總計
        //        int rows2 = dt2.Rows.Count + 7;
        //        DataRow[] drkouya2 = dt2.Select(" CaseKind2 = '扣押'");
        //        SetExcelCell(sheet2, rows2, 1, styleHead10, drkouya2.Length.ToString());
        //        DataRow[] drzhifu2 = dt2.Select(" CaseKind2 = '支付'");
        //        SetExcelCell(sheet2, rows2, 2, styleHead10, drzhifu2.Length.ToString());
        //        DataRow[] drkouyaandPay2 = dt2.Select(" CaseKind2 = '扣押並支付'");
        //        SetExcelCell(sheet2, rows2, 3, styleHead10, drkouyaandPay2.Length.ToString());
        //        DataRow[] drWai2 = dt2.Select(" CaseKind = '外來文案件'");
        //        SetExcelCell(sheet2, rows2, 4, styleHead10, drWai2.Length.ToString());
        //        SetExcelCell(sheet2, rows2, 5, styleHead10, dt2.Rows.Count.ToString());
        //        #endregion
        //        #region body3
        //        for (int iRow = 0; iRow < dt3.Rows.Count; iRow++)
        //        {
        //            for (int iCol = 0; iCol < dt3.Columns.Count; iCol++)
        //            {
        //                SetExcelCell(sheet3, iRow + 3, iCol, styleHead10, dt3.Rows[iRow][iCol].ToString());
        //            }
        //        }
        //        //總計
        //        int rows3 = dt3.Rows.Count + 7;
        //        DataRow[] drkouya3 = dt3.Select(" CaseKind2 = '扣押'");
        //        SetExcelCell(sheet3, rows3, 1, styleHead10, drkouya3.Length.ToString());
        //        DataRow[] drzhifu3 = dt3.Select(" CaseKind2 = '支付'");
        //        SetExcelCell(sheet3, rows3, 2, styleHead10, drzhifu3.Length.ToString());
        //        DataRow[] drkouyaandPay3 = dt3.Select(" CaseKind2 = '扣押並支付'");
        //        SetExcelCell(sheet3, rows3, 3, styleHead10, drkouyaandPay3.Length.ToString());
        //        DataRow[] drWai3 = dt3.Select(" CaseKind = '外來文案件'");
        //        SetExcelCell(sheet3, rows3, 4, styleHead10, drWai3.Length.ToString());
        //        SetExcelCell(sheet3, rows3, 5, styleHead10, dt3.Rows.Count.ToString());
        //        #endregion
        //    }
        //    MemoryStream ms = new MemoryStream();
        //    workbook.Write(ms);
        //    ms.Flush();
        //    ms.Position = 0;
        //    workbook = null;
        //    return ms;
        //}
        //#endregion

        //20170811 RC RQ-2015-019666-020 派件至跨單位(原產生邏輯程式寫死，組織異動必出問題，改為參數) 宏祥 add start
        #region 電子發文明細表
        public MemoryStream ListReportExcel_Send1(CaseClosedQuery model, IList<PARMCode> listCode)
        {
           IWorkbook workbook = new HSSFWorkbook();
           int Departmentcount = listCode.Count();

           #region def style
           ICellStyle styleHead12 = workbook.CreateCellStyle();
           IFont font12 = workbook.CreateFont();
           font12.FontHeightInPoints = 12;
           font12.FontName = "新細明體";
           styleHead12.FillPattern = FillPattern.SolidForeground;
           styleHead12.FillForegroundColor = HSSFColor.White.Index;
           styleHead12.BorderTop = BorderStyle.None;
           styleHead12.BorderLeft = BorderStyle.None;
           styleHead12.BorderRight = BorderStyle.None;
           styleHead12.BorderBottom = BorderStyle.None;
           styleHead12.WrapText = true;
           styleHead12.Alignment = HorizontalAlignment.Center;
           styleHead12.VerticalAlignment = VerticalAlignment.Center;
           styleHead12.SetFont(font12);

           ICellStyle styleHead10 = workbook.CreateCellStyle();
           IFont font10 = workbook.CreateFont();
           font10.FontHeightInPoints = 10;
           font10.FontName = "新細明體";
           styleHead10.FillPattern = FillPattern.SolidForeground;
           styleHead10.FillForegroundColor = HSSFColor.White.Index;
           styleHead10.BorderTop = BorderStyle.Thin;
           styleHead10.BorderLeft = BorderStyle.Thin;
           styleHead10.BorderRight = BorderStyle.Thin;
           styleHead10.BorderBottom = BorderStyle.Thin;
           styleHead10.WrapText = true;
           styleHead10.Alignment = HorizontalAlignment.Left;
           styleHead10.VerticalAlignment = VerticalAlignment.Center;
           styleHead10.SetFont(font10);
           #endregion

           //判斷科別搜尋條件
           if (model.Depart != "0")
           {
              for (int k = 0; k < Departmentcount; k++)
              {
                 if (model.Depart == (k + 1).ToString())
                 {
                    ISheet sheet = null;
                    DataTable dt = new DataTable();

                    sheet = workbook.CreateSheet(listCode[k].CodeDesc);
                    dt = GetCaseSend1(model, listCode[k].CodeDesc);//獲取查詢科別的案件
                    SetExcelCell(sheet, 1, 6, styleHead12, listCode[k].CodeDesc);
                    sheet.AddMergedRegion(new CellRangeAddress(1, 1, 6, 7));

                    #region title
                    SetExcelCell(sheet, 0, 0, styleHead12, "電子發文明細表");
                    sheet.AddMergedRegion(new CellRangeAddress(0, 0, 0, 8));

                    //* line1
                    SetExcelCell(sheet, 1, 0, styleHead12, "");
                    //sheet.AddMergedRegion(new CellRangeAddress(1, 1, 0, 0));
                    SetExcelCell(sheet, 1, 1, styleHead12, "");
                    //sheet.AddMergedRegion(new CellRangeAddress(1, 1, 1, 2));
                    SetExcelCell(sheet, 1, 4, styleHead12, "部門別/科別：");
                    sheet.AddMergedRegion(new CellRangeAddress(1, 1, 4, 5));

                    SetExcelCell(sheet, 2, 0, styleHead12, "電子發文上傳日：");
                    sheet.AddMergedRegion(new CellRangeAddress(2, 2, 0, 0));
                    SetExcelCell(sheet, 2, 1, styleHead12, model.SendUpDateStart + '~' + model.SendUpDateEnd);
                    sheet.AddMergedRegion(new CellRangeAddress(2, 2, 1, 2));
                    SetExcelCell(sheet, 3, 0, styleHead12, "發文方式");
                    sheet.AddMergedRegion(new CellRangeAddress(3, 3, 0, 0));
                    SetExcelCell(sheet, 3, 1, styleHead12, model.SendKind);
                    sheet.AddMergedRegion(new CellRangeAddress(3, 3, 1, 2));

                    //* line2
                    SetExcelCell(sheet, 4, 0, styleHead10, "發文字號");
                    sheet.AddMergedRegion(new CellRangeAddress(4, 4, 0, 0));
                    SetExcelCell(sheet, 4, 1, styleHead10, "案件編號");
                    sheet.AddMergedRegion(new CellRangeAddress(4, 4, 1, 1));
                    SetExcelCell(sheet, 4, 2, styleHead10, "受文者");
                    sheet.AddMergedRegion(new CellRangeAddress(4, 4, 2, 2));
                    SetExcelCell(sheet, 4, 3, styleHead10, "副本");
                    sheet.AddMergedRegion(new CellRangeAddress(4, 4, 3, 3));
                    SetExcelCell(sheet, 4, 4, styleHead10, "經辦");
                    sheet.AddMergedRegion(new CellRangeAddress(4, 4, 4, 4));
                    SetExcelCell(sheet, 4, 5, styleHead10, "案件大類");
                    sheet.AddMergedRegion(new CellRangeAddress(4, 4, 5, 5));
                    SetExcelCell(sheet, 4, 6, styleHead10, "案件細類");
                    sheet.AddMergedRegion(new CellRangeAddress(4, 4, 6, 6));
                    SetExcelCell(sheet, 4, 7, styleHead10, "放行主管");
                    sheet.AddMergedRegion(new CellRangeAddress(4, 4, 7, 7));
                    SetExcelCell(sheet, 4, 8, styleHead10, "逾期註記");
                    sheet.AddMergedRegion(new CellRangeAddress(4, 4, 8, 8));
                    SetExcelCell(sheet, 4, 9, styleHead10, "電子發文上傳日");
                    sheet.AddMergedRegion(new CellRangeAddress(4, 4, 9, 9));
                    //SetExcelCell(sheet, 4, 10, styleHead10, "發文方式");
                    //sheet.AddMergedRegion(new CellRangeAddress(4, 4, 10, 10));

                    SetExcelCell(sheet, dt.Rows.Count + 6, 0, styleHead10, "案件類別");
                    sheet.AddMergedRegion(new CellRangeAddress(dt.Rows.Count + 6, dt.Rows.Count + 6, 0, 0));
                    SetExcelCell(sheet, dt.Rows.Count + 6, 1, styleHead10, "扣押");
                    sheet.AddMergedRegion(new CellRangeAddress(dt.Rows.Count + 6, dt.Rows.Count + 6, 1, 1));
                    SetExcelCell(sheet, dt.Rows.Count + 6, 2, styleHead10, "支付");
                    sheet.AddMergedRegion(new CellRangeAddress(dt.Rows.Count + 6, dt.Rows.Count + 6, 2, 2));
                    SetExcelCell(sheet, dt.Rows.Count + 6, 3, styleHead10, "扣押並支付");
                    sheet.AddMergedRegion(new CellRangeAddress(dt.Rows.Count + 6, dt.Rows.Count + 6, 3, 3));
                    SetExcelCell(sheet, dt.Rows.Count + 6, 4, styleHead10, "外來文案件");
                    sheet.AddMergedRegion(new CellRangeAddress(dt.Rows.Count + 6, dt.Rows.Count + 6, 4, 4));
                    SetExcelCell(sheet, dt.Rows.Count + 6, 5, styleHead10, "合計");
                    sheet.AddMergedRegion(new CellRangeAddress(dt.Rows.Count + 6, dt.Rows.Count + 6, 5, 5));

                    SetExcelCell(sheet, dt.Rows.Count + 7, 0, styleHead10, "件數");
                    sheet.AddMergedRegion(new CellRangeAddress(dt.Rows.Count + 7, dt.Rows.Count + 7, 0, 0));

                    SetExcelCell(sheet, dt.Rows.Count + 9, 4, styleHead12, "覆核主管");
                    sheet.AddMergedRegion(new CellRangeAddress(dt.Rows.Count + 9, dt.Rows.Count + 9, 4, 4));
                    SetExcelCell(sheet, dt.Rows.Count + 9, 6, styleHead12, "覆核人員");
                    sheet.AddMergedRegion(new CellRangeAddress(dt.Rows.Count + 9, dt.Rows.Count + 9, 6, 6));

                    for (int i = dt.Rows.Count + 7; i < dt.Rows.Count + 8; i++)//*初始表格賦初值 
                    {
                       for (int j = 0; j < 5; j++)
                       {
                          SetExcelCell(sheet, i, j + 1, styleHead10, "0");
                          sheet.AddMergedRegion(new CellRangeAddress(i, i, j + 1, j + 1));
                       }
                    }
                    #endregion
                    #region Width
                    sheet.SetColumnWidth(0, 100 * 100);
                    sheet.SetColumnWidth(1, 100 * 40);
                    sheet.SetColumnWidth(2, 100 * 100);
                    sheet.SetColumnWidth(3, 100 * 100);
                    sheet.SetColumnWidth(4, 100 * 30);
                    sheet.SetColumnWidth(5, 100 * 30);
                    sheet.SetColumnWidth(6, 100 * 30);
                    sheet.SetColumnWidth(7, 100 * 40);
                    sheet.SetColumnWidth(8, 100 * 30);
                    sheet.SetColumnWidth(9, 100 * 100);
                    //sheet.SetColumnWidth(9, 100 * 40);
                    //sheet.SetColumnWidth(10, 100 * 40);
                    #endregion
                    #region body
                    for (int iRow = 0; iRow < dt.Rows.Count; iRow++)
                    {
                       for (int iCol = 1; iCol < dt.Columns.Count - 2; iCol++)
                       {
                          SetExcelCell(sheet, iRow + 5, iCol - 1, styleHead10, dt.Rows[iRow][iCol].ToString());
                       }
                    }
                    //總計
                    int rows = dt.Rows.Count + 7;
                    DataRow[] drkouya = dt.Select(" CaseKind2 = '扣押'");
                    SetExcelCell(sheet, rows, 1, styleHead10, drkouya.Length.ToString());
                    DataRow[] drzhifu = dt.Select(" CaseKind2 = '支付'");
                    SetExcelCell(sheet, rows, 2, styleHead10, drzhifu.Length.ToString());
                    DataRow[] drkouyaandPay = dt.Select(" CaseKind2 = '扣押並支付'");
                    SetExcelCell(sheet, rows, 3, styleHead10, drkouyaandPay.Length.ToString());
                    DataRow[] drWai = dt.Select(" CaseKind = '外來文案件'");
                    SetExcelCell(sheet, rows, 4, styleHead10, drWai.Length.ToString());
                    SetExcelCell(sheet, rows, 5, styleHead10, dt.Rows.Count.ToString());
                    #endregion
                 }
              }
           }
           else
           {
              for (int k = 0; k < Departmentcount; k++)
              {
                 ISheet sheet = null;
                 DataTable dt = new DataTable();

                 sheet = workbook.CreateSheet(listCode[k].CodeDesc);
                 dt = GetCaseSend1(model, listCode[k].CodeDesc);//獲取查詢科別的案件
                 SetExcelCell(sheet, 1, 6, styleHead12, listCode[k].CodeDesc);
                 sheet.AddMergedRegion(new CellRangeAddress(1, 1, 6, 7));

                 #region title
                 SetExcelCell(sheet, 0, 0, styleHead12, "電子發文明細表");
                 sheet.AddMergedRegion(new CellRangeAddress(0, 0, 0, 8));

                 //* line1
                 SetExcelCell(sheet, 1, 0, styleHead12, "");
                 //sheet.AddMergedRegion(new CellRangeAddress(1, 1, 0, 0));
                 SetExcelCell(sheet, 1, 1, styleHead12, "");
                 //sheet.AddMergedRegion(new CellRangeAddress(1, 1, 1, 2));
                 SetExcelCell(sheet, 1, 4, styleHead12, "部門別/科別：");
                 sheet.AddMergedRegion(new CellRangeAddress(1, 1, 4, 5));

                 SetExcelCell(sheet, 2, 0, styleHead12, "電子發文上傳日：");
                 sheet.AddMergedRegion(new CellRangeAddress(2, 2, 0, 0));
                 SetExcelCell(sheet, 2, 1, styleHead12, model.SendUpDateStart + '~' + model.SendUpDateEnd);
                 sheet.AddMergedRegion(new CellRangeAddress(2, 2, 1, 2));
                 SetExcelCell(sheet, 3, 0, styleHead12, "發文方式");
                 sheet.AddMergedRegion(new CellRangeAddress(3, 3, 0, 0));
                 SetExcelCell(sheet, 3, 1, styleHead12, model.SendKind);
                 sheet.AddMergedRegion(new CellRangeAddress(3, 3, 1, 2));

                 //* line2
                 SetExcelCell(sheet, 4, 0, styleHead10, "發文字號");
                 sheet.AddMergedRegion(new CellRangeAddress(4, 4, 0, 0));
                 SetExcelCell(sheet, 4, 1, styleHead10, "案件編號");
                 sheet.AddMergedRegion(new CellRangeAddress(4, 4, 1, 1));
                 SetExcelCell(sheet, 4, 2, styleHead10, "受文者");
                 sheet.AddMergedRegion(new CellRangeAddress(4, 4, 2, 2));
                 SetExcelCell(sheet, 4, 3, styleHead10, "副本");
                 sheet.AddMergedRegion(new CellRangeAddress(4, 4, 3, 3));
                 SetExcelCell(sheet, 4, 4, styleHead10, "經辦");
                 sheet.AddMergedRegion(new CellRangeAddress(4, 4, 4, 4));
                 SetExcelCell(sheet, 4, 5, styleHead10, "案件大類");
                 sheet.AddMergedRegion(new CellRangeAddress(4, 4, 5, 5));
                 SetExcelCell(sheet, 4, 6, styleHead10, "案件細類");
                 sheet.AddMergedRegion(new CellRangeAddress(4, 4, 6, 6));
                 SetExcelCell(sheet, 4, 7, styleHead10, "放行主管");
                 sheet.AddMergedRegion(new CellRangeAddress(4, 4, 7, 7));
                 SetExcelCell(sheet, 4, 8, styleHead10, "逾期註記");
                 sheet.AddMergedRegion(new CellRangeAddress(4, 4, 8, 8));
                 SetExcelCell(sheet, 4, 9, styleHead10, "電子發文上傳日");
                 sheet.AddMergedRegion(new CellRangeAddress(4, 4, 9, 9));
                 //SetExcelCell(sheet, 4, 10, styleHead10, "發文方式");
                 //sheet.AddMergedRegion(new CellRangeAddress(4, 4, 10, 10));

                 SetExcelCell(sheet, dt.Rows.Count + 6, 0, styleHead10, "案件類別");
                 sheet.AddMergedRegion(new CellRangeAddress(dt.Rows.Count + 6, dt.Rows.Count + 6, 0, 0));
                 SetExcelCell(sheet, dt.Rows.Count + 6, 1, styleHead10, "扣押");
                 sheet.AddMergedRegion(new CellRangeAddress(dt.Rows.Count + 6, dt.Rows.Count + 6, 1, 1));
                 SetExcelCell(sheet, dt.Rows.Count + 6, 2, styleHead10, "支付");
                 sheet.AddMergedRegion(new CellRangeAddress(dt.Rows.Count + 6, dt.Rows.Count + 6, 2, 2));
                 SetExcelCell(sheet, dt.Rows.Count + 6, 3, styleHead10, "扣押並支付");
                 sheet.AddMergedRegion(new CellRangeAddress(dt.Rows.Count + 6, dt.Rows.Count + 6, 3, 3));
                 SetExcelCell(sheet, dt.Rows.Count + 6, 4, styleHead10, "外來文案件");
                 sheet.AddMergedRegion(new CellRangeAddress(dt.Rows.Count + 6, dt.Rows.Count + 6, 4, 4));
                 SetExcelCell(sheet, dt.Rows.Count + 6, 5, styleHead10, "合計");
                 sheet.AddMergedRegion(new CellRangeAddress(dt.Rows.Count + 6, dt.Rows.Count + 6, 5, 5));

                 SetExcelCell(sheet, dt.Rows.Count + 7, 0, styleHead10, "件數");
                 sheet.AddMergedRegion(new CellRangeAddress(dt.Rows.Count + 7, dt.Rows.Count + 7, 0, 0));

                 SetExcelCell(sheet, dt.Rows.Count + 9, 4, styleHead12, "覆核主管");
                 sheet.AddMergedRegion(new CellRangeAddress(dt.Rows.Count + 9, dt.Rows.Count + 9, 4, 4));
                 SetExcelCell(sheet, dt.Rows.Count + 9, 6, styleHead12, "覆核人員");
                 sheet.AddMergedRegion(new CellRangeAddress(dt.Rows.Count + 9, dt.Rows.Count + 9, 6, 6));

                 for (int i = dt.Rows.Count + 7; i < dt.Rows.Count + 8; i++)//*初始表格賦初值 
                 {
                    for (int j = 0; j < 5; j++)
                    {
                       SetExcelCell(sheet, i, j + 1, styleHead10, "0");
                       sheet.AddMergedRegion(new CellRangeAddress(i, i, j + 1, j + 1));
                    }
                 }
                 #endregion
                 #region Width
                 sheet.SetColumnWidth(0, 100 * 100);
                 sheet.SetColumnWidth(1, 100 * 40);
                 sheet.SetColumnWidth(2, 100 * 100);
                 sheet.SetColumnWidth(3, 100 * 100);
                 sheet.SetColumnWidth(4, 100 * 30);
                 sheet.SetColumnWidth(5, 100 * 30);
                 sheet.SetColumnWidth(6, 100 * 30);
                 sheet.SetColumnWidth(7, 100 * 40);
                 sheet.SetColumnWidth(8, 100 * 30);
                 sheet.SetColumnWidth(9, 100 * 100);
                 //sheet.SetColumnWidth(9, 100 * 40);
                 //sheet.SetColumnWidth(10, 100 * 40);
                 #endregion
                 #region body
                 for (int iRow = 0; iRow < dt.Rows.Count; iRow++)
                 {
                    for (int iCol = 1; iCol < dt.Columns.Count - 2; iCol++)
                    {
                       SetExcelCell(sheet, iRow + 5, iCol - 1, styleHead10, dt.Rows[iRow][iCol].ToString());
                    }
                 }
                 //總計
                 int rows = dt.Rows.Count + 7;
                 DataRow[] drkouya = dt.Select(" CaseKind2 = '扣押'");
                 SetExcelCell(sheet, rows, 1, styleHead10, drkouya.Length.ToString());
                 DataRow[] drzhifu = dt.Select(" CaseKind2 = '支付'");
                 SetExcelCell(sheet, rows, 2, styleHead10, drzhifu.Length.ToString());
                 DataRow[] drkouyaandPay = dt.Select(" CaseKind2 = '扣押並支付'");
                 SetExcelCell(sheet, rows, 3, styleHead10, drkouyaandPay.Length.ToString());
                 DataRow[] drWai = dt.Select(" CaseKind = '外來文案件'");
                 SetExcelCell(sheet, rows, 4, styleHead10, drWai.Length.ToString());
                 SetExcelCell(sheet, rows, 5, styleHead10, dt.Rows.Count.ToString());
                 #endregion
              }
           }

           MemoryStream ms = new MemoryStream();
           workbook.Write(ms);
           ms.Flush();
           ms.Position = 0;
           workbook = null;
           return ms;
        }
        #endregion
        //20170811 RC RQ-2015-019666-020 派件至跨單位(原產生邏輯程式寫死，組織異動必出問題，改為參數) 宏祥 add end
               
        //#region 電子發文統計表
        //public MemoryStream ListReportExcel_Send2(CaseClosedQuery model)
        //{
        //    IWorkbook workbook = new HSSFWorkbook();
        //    ISheet sheet = null;
        //    ISheet sheet2 = null;
        //    ISheet sheet3 = null;
        //    Dictionary<string, string> dicldapList = new Dictionary<string, string>();
        //    Dictionary<string, string> dicldapList2 = new Dictionary<string, string>();
        //    Dictionary<string, string> dicldapList3 = new Dictionary<string, string>();
        //    DataTable dtCase = new DataTable();//導出資料
        //    DataTable dtCase2 = new DataTable();
        //    DataTable dtCase3 = new DataTable();

        //    int rowscountExcelresult = 0;//合計參數
        //    string caseExcel = "";//案件類型
        //    int rowsExcel = 4;//行數
        //    int rowscountExcel = 0;//最後一列合計
        //    int rowstatolExcel = 0;//總合計 
        //    int sort = 1;

        //    #region def style
        //    ICellStyle styleHead12 = workbook.CreateCellStyle();
        //    IFont font12 = workbook.CreateFont();
        //    font12.FontHeightInPoints = 12;
        //    font12.FontName = "新細明體";
        //    styleHead12.FillPattern = FillPattern.SolidForeground;
        //    styleHead12.FillForegroundColor = HSSFColor.White.Index;
        //    styleHead12.BorderTop = BorderStyle.None;
        //    styleHead12.BorderLeft = BorderStyle.None;
        //    styleHead12.BorderRight = BorderStyle.None;
        //    styleHead12.BorderBottom = BorderStyle.None;
        //    styleHead12.WrapText = true;
        //    styleHead12.Alignment = HorizontalAlignment.Center;
        //    styleHead12.VerticalAlignment = VerticalAlignment.Center;
        //    styleHead12.SetFont(font12);

        //    ICellStyle styleHead10 = workbook.CreateCellStyle();
        //    IFont font10 = workbook.CreateFont();
        //    font10.FontHeightInPoints = 10;
        //    font10.FontName = "新細明體";
        //    styleHead10.FillPattern = FillPattern.SolidForeground;
        //    styleHead10.FillForegroundColor = HSSFColor.White.Index;
        //    styleHead10.BorderTop = BorderStyle.Thin;
        //    styleHead10.BorderLeft = BorderStyle.Thin;
        //    styleHead10.BorderRight = BorderStyle.Thin;
        //    styleHead10.BorderBottom = BorderStyle.Thin;
        //    styleHead10.WrapText = true;
        //    styleHead10.Alignment = HorizontalAlignment.Left;
        //    styleHead10.VerticalAlignment = VerticalAlignment.Center;
        //    styleHead10.SetFont(font10);
        //    #endregion

        //    #region 單獨科別的數據源(科別及案件資料)
        //    //獲取人員
        //    if (model.Depart == "1" || model.Depart == "0")//* 集作一科
        //    {
        //        sheet = workbook.CreateSheet("集作一科");
        //        dtCase = GetCaseSend2(model, "集作一科");
        //        //判斷人員
        //        foreach (DataRow dr in dtCase.Rows)
        //        {
        //            if (!dicldapList.Keys.Contains(dr["ApproveUser"].ToString()))
        //            {
        //                dicldapList.Add(dr["ApproveUser"].ToString(), dr["EmpName"].ToString() + "|" + sort);
        //                sort++;
        //            }
        //        }

        //        if (dicldapList.Count > 2)
        //        {
        //            SetExcelCell(sheet, 1, dicldapList.Count + 1, styleHead12, "集作一科");
        //            sheet.AddMergedRegion(new CellRangeAddress(1, 1, dicldapList.Count + 1, dicldapList.Count + 1));
        //        }
        //        else
        //        {
        //            SetExcelCell(sheet, 1, 4, styleHead12, "集作一科");
        //            sheet.AddMergedRegion(new CellRangeAddress(1, 1, 4, 4));
        //        }
        //    }
        //    if (model.Depart == "2")//* 集作二科
        //    {
        //        sheet = workbook.CreateSheet("集作二科");
        //        dtCase = GetCaseSend2(model, "集作二科");
        //        sort = 1;
        //        //判斷人員
        //        foreach (DataRow dr in dtCase.Rows)
        //        {
        //            if (!dicldapList.Keys.Contains(dr["ApproveUser"].ToString()))
        //            {
        //                dicldapList.Add(dr["ApproveUser"].ToString(), dr["EmpName"].ToString() + "|" + sort);
        //                sort++;
        //            }
        //        }

        //        if (dicldapList.Count > 2)
        //        {
        //            SetExcelCell(sheet, 1, dicldapList.Count + 1, styleHead12, "集作二科");
        //            sheet.AddMergedRegion(new CellRangeAddress(1, 1, dicldapList.Count + 1, dicldapList.Count + 1));
        //        }
        //        else
        //        {
        //            SetExcelCell(sheet, 1, 4, styleHead12, "集作二科");
        //            sheet.AddMergedRegion(new CellRangeAddress(1, 1, 4, 4));
        //        }
        //    }
        //    if (model.Depart == "3")//* 集作三科
        //    {
        //        sheet = workbook.CreateSheet("集作三科");
        //        dtCase = GetCaseSend2(model, "集作三科");
        //        sort = 1;
        //        //判斷人員
        //        foreach (DataRow dr in dtCase.Rows)
        //        {
        //            if (!dicldapList.Keys.Contains(dr["ApproveUser"].ToString()))
        //            {
        //                dicldapList.Add(dr["ApproveUser"].ToString(), dr["EmpName"].ToString() + "|" + sort);
        //                sort++;
        //            }
        //        }

        //        if (dicldapList.Count > 2)
        //        {
        //            SetExcelCell(sheet, 1, dicldapList.Count + 1, styleHead12, "集作三科");
        //            sheet.AddMergedRegion(new CellRangeAddress(1, 1, dicldapList.Count + 1, dicldapList.Count + 1));
        //        }
        //        else
        //        {
        //            SetExcelCell(sheet, 1, 4, styleHead12, "集作三科");
        //            sheet.AddMergedRegion(new CellRangeAddress(1, 1, 4, 4));
        //        }
        //    }
        //    if (model.Depart == "0")//* 全部
        //    {
        //        sheet2 = workbook.CreateSheet("集作二科");
        //        sheet3 = workbook.CreateSheet("集作三科");
        //        dtCase2 = GetCaseSend2(model, "集作二科");//獲取查詢集作二科的案件
        //        dtCase3 = GetCaseSend2(model, "集作三科");//獲取查詢集作三科的案件
        //        sort = 1;
        //        //判斷集作二科人員
        //        foreach (DataRow dr in dtCase2.Rows)
        //        {
        //            if (!dicldapList2.Keys.Contains(dr["ApproveUser"].ToString()))
        //            {
        //                dicldapList2.Add(dr["ApproveUser"].ToString(), dr["EmpName"].ToString() + "|" + sort);
        //                sort++;
        //            }
        //        }
        //        sort = 1;
        //        //判斷集作三科人員
        //        foreach (DataRow dr in dtCase3.Rows)
        //        {
        //            if (!dicldapList3.Keys.Contains(dr["ApproveUser"].ToString()))
        //            {
        //                dicldapList3.Add(dr["ApproveUser"].ToString(), dr["EmpName"].ToString() + "|" + sort);
        //                sort++;
        //            }
        //        }
        //    }
        //    #endregion

        //    string caseKind = "";//*去重複
        //    int rows = 4;//定義行數

        //    #region title
        //    //*大標題 line0
        //    SetExcelCell(sheet, 0, 0, styleHead12, "電子發文統計表");
        //    sheet.AddMergedRegion(new CellRangeAddress(0, 0, 0, dicldapList.Count + 1));

        //    //*查詢條件 line1
        //    SetExcelCell(sheet, 1, 0, styleHead12, "");
        //    sheet.AddMergedRegion(new CellRangeAddress(1, 1, 0, 0));
        //    SetExcelCell(sheet, 1, 1, styleHead12, "");
        //    sheet.AddMergedRegion(new CellRangeAddress(1, 1, 1, 2));


        //    SetExcelCell(sheet, 2, 0, styleHead12, "電子發文上傳日：");
        //    sheet.AddMergedRegion(new CellRangeAddress(2, 2, 0, 0));
        //    SetExcelCell(sheet, 2, 1, styleHead12, model.SendUpDateStart + '~' + model.SendUpDateEnd);
        //    sheet.AddMergedRegion(new CellRangeAddress(2, 2, 1, 2));

        //    SetExcelCell(sheet, 3, 0, styleHead12, "發文方式：");
        //    sheet.AddMergedRegion(new CellRangeAddress(3, 3, 0, 0));
        //    SetExcelCell(sheet, 3, 1, styleHead12, model.SendKind);
        //    sheet.AddMergedRegion(new CellRangeAddress(3, 3, 1, 2));

        //    if (dicldapList.Count > 2)
        //    {
        //        SetExcelCell(sheet, 1, dicldapList.Count, styleHead12, "部門別/科別");
        //        sheet.AddMergedRegion(new CellRangeAddress(1, 1, dicldapList.Count, dicldapList.Count));
        //    }
        //    else
        //    {
        //        SetExcelCell(sheet, 1, 3, styleHead12, "部門別/科別");
        //        sheet.AddMergedRegion(new CellRangeAddress(1, 1, 3, 3));
        //        sheet.SetColumnWidth(3, 100 * 30);
        //    }

        //    //*結果集表頭 line2
        //    SetExcelCell(sheet, 4, 0, styleHead10, "處理人員");
        //    sheet.AddMergedRegion(new CellRangeAddress(4, 4, 0, 0));
        //    sheet.SetColumnWidth(0, 100 * 50);

        //    //依次排列人員名稱
        //    int a = 1;
        //    foreach (var item in dicldapList)
        //    {
        //        SetExcelCell(sheet, 4, a, styleHead10, item.Value.Split('|')[0]);
        //        sheet.AddMergedRegion(new CellRangeAddress(4, 4, a, a));
        //        a++;
        //    }

        //    SetExcelCell(sheet, 4, dicldapList.Count + 1, styleHead10, "合計");
        //    sheet.AddMergedRegion(new CellRangeAddress(4, 4, dicldapList.Count + 1, dicldapList.Count + 1));

        //    //*扣押案件類型 line5-lineN 
        //    for (int i = 0; i < dtCase.Rows.Count; i++)
        //    {
        //        if (caseKind != dtCase.Rows[i]["New_CaseKind"].ToString())
        //        {
        //            rows = rows + 1;
        //            SetExcelCell(sheet, rows, 0, styleHead10, dtCase.Rows[i]["New_CaseKind"].ToString());
        //            sheet.AddMergedRegion(new CellRangeAddress(rows, rows, 0, 0));
        //            SetExcelCell(sheet, rows, dicldapList.Count + 1, styleHead10, "0");
        //            sheet.AddMergedRegion(new CellRangeAddress(rows, rows, dicldapList.Count + 1, dicldapList.Count + 1));
        //            caseKind = dtCase.Rows[i]["New_CaseKind"].ToString();
        //            for (int j = 0; j < dicldapList.Count; j++)//*初始表格賦初值 
        //            {
        //                SetExcelCell(sheet, rows, j + 1, styleHead10, "0");
        //                sheet.AddMergedRegion(new CellRangeAddress(rows, rows, j + 1, j + 1));
        //            }
        //        }
        //    }

        //    //*合計 lineLast
        //    SetExcelCell(sheet, rows + 1, 0, styleHead10, "合計");
        //    sheet.AddMergedRegion(new CellRangeAddress(rows + 1, rows + 1, 0, 0));

        //    for (int j = 0; j < dicldapList.Count; j++)//*初始表格賦初值 
        //    {
        //        SetExcelCell(sheet, rows + 1, j + 1, styleHead10, "0");
        //        sheet.AddMergedRegion(new CellRangeAddress(rows + 1, rows + 1, j + 1, j + 1));
        //    }
        //    #endregion

        //    #region  body
        //    for (int iRow = 0; iRow < dtCase.Rows.Count; iRow++)//根據案件類型進行循環
        //    {
        //        foreach (var item in dicldapList)
        //        {
        //            int irows = Convert.ToInt32(item.Value.Split('|')[1]);
        //            if (caseExcel == dtCase.Rows[iRow]["New_CaseKind"].ToString())//重複同一案件類型的數據
        //            {
        //                if (item.Key == dtCase.Rows[iRow]["ApproveUser"].ToString())
        //                {
        //                    SetExcelCell(sheet, rowsExcel, irows, styleHead10, dtCase.Rows[iRow]["case_num"].ToString());
        //                    rowscountExcelresult = Convert.ToInt32(dtCase.Rows[iRow]["case_num"].ToString());//每格資料
        //                    SetExcelCell(sheet, rows + 1, irows, styleHead10, dtCase.Rows[iRow]["UserCount"].ToString());//最後一行合計
        //                    rowscountExcel += rowscountExcelresult;
        //                    rowstatolExcel += rowscountExcelresult;
        //                }
        //                SetExcelCell(sheet, rowsExcel, dicldapList.Count + 1, styleHead10, rowscountExcel.ToString());//最後一列合計
        //            }
        //            else//不重複的案件類型
        //            {
        //                rowscountExcel = 0;
        //                rowsExcel = rowsExcel + 1;
        //                if (dtCase.Rows[iRow]["ApproveUser"].ToString() == item.Key)
        //                {
        //                    SetExcelCell(sheet, rowsExcel, irows, styleHead10, dtCase.Rows[iRow]["case_num"].ToString());
        //                    rowscountExcelresult = Convert.ToInt32(dtCase.Rows[iRow]["case_num"].ToString());//第一條不重複的數據儲存下值
        //                    SetExcelCell(sheet, rows + 1, irows, styleHead10, dtCase.Rows[iRow]["UserCount"].ToString());//最後一行合計
        //                    rowscountExcel += rowscountExcelresult;
        //                    rowstatolExcel += rowscountExcelresult;
        //                }
        //                caseExcel = dtCase.Rows[iRow]["New_CaseKind"].ToString();
        //                SetExcelCell(sheet, rowsExcel, dicldapList.Count + 1, styleHead10, rowscountExcel.ToString());//最後一列合計
        //            }
        //        }
        //    }
        //    SetExcelCell(sheet, rows + 1, dicldapList.Count + 1, styleHead10, rowstatolExcel.ToString());//總合計
        //    #endregion

        //    if (model.Depart == "0")//* 全部
        //    {
        //        #region title2
        //        //*大標題 line0
        //        SetExcelCell(sheet2, 0, 0, styleHead12, "電子發文統計表");
        //        sheet2.AddMergedRegion(new CellRangeAddress(0, 0, 0, dicldapList2.Count + 1));

        //        //*查詢條件 line1
        //        SetExcelCell(sheet2, 1, 0, styleHead12, "");
        //        sheet2.AddMergedRegion(new CellRangeAddress(1, 1, 0, 0));
        //        SetExcelCell(sheet2, 1, 1, styleHead12, "");
        //        sheet2.AddMergedRegion(new CellRangeAddress(1, 1, 1, 2));

        //        SetExcelCell(sheet2, 2, 0, styleHead12, "電子發文上傳日：");
        //        sheet2.AddMergedRegion(new CellRangeAddress(2, 2, 0, 0));
        //        SetExcelCell(sheet2, 2, 1, styleHead12, model.SendUpDateStart + '~' + model.SendUpDateEnd);
        //        sheet2.AddMergedRegion(new CellRangeAddress(2, 2, 1, 2));

        //        SetExcelCell(sheet2, 3, 0, styleHead12, "發文方式：");
        //        sheet2.AddMergedRegion(new CellRangeAddress(3, 3, 0, 0));
        //        SetExcelCell(sheet2, 3, 1, styleHead12, model.SendKind);
        //        sheet2.AddMergedRegion(new CellRangeAddress(3, 3, 1, 2));

        //        if (dicldapList2.Count > 2)
        //        {
        //            SetExcelCell(sheet2, 1, dicldapList2.Count - 1, styleHead12, "部門別/科別");
        //            sheet2.AddMergedRegion(new CellRangeAddress(1, 1, dicldapList2.Count - 1, dicldapList2.Count));
        //            SetExcelCell(sheet2, 1, dicldapList2.Count + 1, styleHead12, "集作二科");
        //            sheet2.AddMergedRegion(new CellRangeAddress(1, 1, dicldapList2.Count + 1, dicldapList2.Count + 1));
        //        }
        //        else
        //        {
        //            SetExcelCell(sheet2, 1, 3, styleHead12, "部門別/科別");
        //            sheet2.AddMergedRegion(new CellRangeAddress(1, 1, 3, 3));
        //            SetExcelCell(sheet2, 1, 4, styleHead12, "集作二科");
        //            sheet2.AddMergedRegion(new CellRangeAddress(1, 1, 4, 4));
        //        }

        //        //*結果集表頭 line4
        //        SetExcelCell(sheet2, 4, 0, styleHead10, "處理人員");
        //        sheet2.AddMergedRegion(new CellRangeAddress(4, 4, 0, 0));
        //        sheet2.SetColumnWidth(0, 100 * 50);

        //        //依次排列人員名稱
        //        a = 1;
        //        foreach (var item in dicldapList2)
        //        {
        //            SetExcelCell(sheet2, 4, a, styleHead10, item.Value.Split('|')[0]);
        //            sheet2.AddMergedRegion(new CellRangeAddress(4, 4, a, a));
        //            a++;
        //        }

        //        SetExcelCell(sheet2, 4, dicldapList2.Count + 1, styleHead10, "合計");
        //        sheet2.AddMergedRegion(new CellRangeAddress(4, 4, dicldapList2.Count + 1, dicldapList2.Count + 1));

        //        //*扣押案件類型 line3-lineN 
        //        caseKind = "";//*去重複
        //        rows = 4;//定義行數
        //        for (int i = 0; i < dtCase2.Rows.Count; i++)
        //        {
        //            if (caseKind != dtCase2.Rows[i]["New_CaseKind"].ToString())
        //            {
        //                rows = rows + 1;
        //                SetExcelCell(sheet2, rows, 0, styleHead10, dtCase2.Rows[i]["New_CaseKind"].ToString());
        //                sheet2.AddMergedRegion(new CellRangeAddress(rows, rows, 0, 0));

        //                SetExcelCell(sheet2, rows, dicldapList2.Count + 1, styleHead10, "0");
        //                sheet2.AddMergedRegion(new CellRangeAddress(rows, rows, dicldapList2.Count + 1, dicldapList2.Count + 1));

        //                caseKind = dtCase2.Rows[i]["New_CaseKind"].ToString();
        //                for (int j = 0; j < dicldapList2.Count; j++)//*初始表格賦初值 
        //                {
        //                    SetExcelCell(sheet2, rows, j + 1, styleHead10, "0");
        //                    sheet2.AddMergedRegion(new CellRangeAddress(rows, rows, j + 1, j + 1));
        //                }
        //            }
        //        }

        //        //*合計 lineLast
        //        SetExcelCell(sheet2, rows + 1, 0, styleHead10, "合計");
        //        sheet2.AddMergedRegion(new CellRangeAddress(rows + 1, rows + 1, 0, 0));
        //        for (int j = 0; j < dicldapList2.Count; j++)//*初始表格賦初值 
        //        {
        //            SetExcelCell(sheet2, rows + 1, j + 1, styleHead10, "0");
        //            sheet2.AddMergedRegion(new CellRangeAddress(rows + 1, rows + 1, j + 1, j + 1));
        //        }
        //        #endregion
        //        #region body2
        //        caseExcel = "";//案件類型
        //        rowsExcel = 4;//行數
        //        rowscountExcel = 0;//最後一列合計
        //        rowstatolExcel = 0;//總合計      
        //        for (int iRow = 0; iRow < dtCase2.Rows.Count; iRow++)//根據案件類型進行循環
        //        {
        //            foreach (var item in dicldapList2)
        //            {
        //                int irows2 = Convert.ToInt32(item.Value.Split('|')[1]);
        //                if (dtCase2.Rows[iRow]["New_CaseKind"].ToString() == caseExcel)//重複同一案件類型的數據
        //                {
        //                    if (dtCase2.Rows[iRow]["ApproveUser"].ToString() == item.Key)
        //                    {
        //                        SetExcelCell(sheet2, rowsExcel, irows2, styleHead10, dtCase2.Rows[iRow]["case_num"].ToString());
        //                        SetExcelCell(sheet2, rows + 1, irows2, styleHead10, dtCase2.Rows[iRow]["UserCount"].ToString());//最後一行合計
        //                        rowscountExcelresult = Convert.ToInt32(dtCase2.Rows[iRow]["case_num"].ToString());//每格資料
        //                        rowscountExcel += rowscountExcelresult;
        //                        rowstatolExcel += rowscountExcelresult;
        //                    }
        //                    SetExcelCell(sheet2, rowsExcel, dicldapList2.Count + 1, styleHead10, rowscountExcel.ToString());//最後一列合計
        //                }
        //                else//不重複的案件類型
        //                {
        //                    rowscountExcel = 0;
        //                    rowsExcel = rowsExcel + 1;
        //                    if (dtCase2.Rows[iRow]["ApproveUser"].ToString() == item.Key)
        //                    {
        //                        SetExcelCell(sheet2, rowsExcel, irows2, styleHead10, dtCase2.Rows[iRow]["case_num"].ToString());
        //                        SetExcelCell(sheet2, rows + 1, irows2, styleHead10, dtCase2.Rows[iRow]["UserCount"].ToString());//最後一行合計
        //                        rowscountExcelresult = Convert.ToInt32(dtCase2.Rows[iRow]["case_num"].ToString());//第一條不重複的數據儲存下值
        //                        rowscountExcel += rowscountExcelresult;
        //                        rowstatolExcel += rowscountExcelresult;
        //                    }
        //                    caseExcel = dtCase2.Rows[iRow]["New_CaseKind"].ToString();
        //                    SetExcelCell(sheet2, rowsExcel, dicldapList2.Count + 1, styleHead10, rowscountExcel.ToString());//最後一列合計      
        //                }
        //            }
        //        }
        //        SetExcelCell(sheet2, rows + 1, dicldapList2.Count + 1, styleHead10, rowstatolExcel.ToString());//總合計
        //        #endregion

        //        #region title3
        //        //*大標題 line0
        //        SetExcelCell(sheet3, 0, 0, styleHead12, "電子發文統計表");
        //        sheet3.AddMergedRegion(new CellRangeAddress(0, 0, 0, dicldapList3.Count + 1));

        //        //*查詢條件 line1
        //        SetExcelCell(sheet3, 1, 0, styleHead12, "");
        //        sheet3.AddMergedRegion(new CellRangeAddress(1, 1, 0, 0));
        //        SetExcelCell(sheet3, 1, 1, styleHead12, "");
        //        sheet3.AddMergedRegion(new CellRangeAddress(1, 1, 1, 2));

        //        SetExcelCell(sheet3, 2, 0, styleHead12, "電子發文上傳日：");
        //        sheet3.AddMergedRegion(new CellRangeAddress(2, 2, 0, 0));
        //        SetExcelCell(sheet3, 2, 1, styleHead12, model.SendUpDateStart + '~' + model.SendUpDateEnd);
        //        sheet3.AddMergedRegion(new CellRangeAddress(2, 2, 1, 2));

        //        SetExcelCell(sheet3, 3, 0, styleHead12, "發文方式：");
        //        sheet3.AddMergedRegion(new CellRangeAddress(3, 3, 0, 0));
        //        SetExcelCell(sheet3, 3, 1, styleHead12, model.SendKind);
        //        sheet3.AddMergedRegion(new CellRangeAddress(3, 3, 1, 2));

        //        if (dicldapList3.Count > 2)
        //        {
        //            SetExcelCell(sheet3, 1, dicldapList3.Count - 1, styleHead12, "部門別/科別");
        //            sheet3.AddMergedRegion(new CellRangeAddress(1, 1, dicldapList3.Count - 1, dicldapList3.Count));
        //            SetExcelCell(sheet3, 1, dicldapList3.Count + 1, styleHead12, "集作三科");
        //            sheet3.AddMergedRegion(new CellRangeAddress(1, 1, dicldapList3.Count + 1, dicldapList3.Count + 1));
        //        }
        //        else
        //        {
        //            SetExcelCell(sheet3, 1, 3, styleHead12, "部門別/科別");
        //            sheet3.AddMergedRegion(new CellRangeAddress(1, 1, 3, 3));
        //            SetExcelCell(sheet3, 1, 4, styleHead12, "集作三科");
        //            sheet3.AddMergedRegion(new CellRangeAddress(1, 1, 4, 4));
        //        }

        //        //*結果集表頭 line2
        //        SetExcelCell(sheet3, 4, 0, styleHead10, "處理人員");
        //        sheet3.AddMergedRegion(new CellRangeAddress(4, 4, 0, 0));
        //        sheet3.SetColumnWidth(0, 100 * 50);
        //        //依次排列人員名稱
        //        a = 1;
        //        foreach (var item in dicldapList3)
        //        {
        //            SetExcelCell(sheet3, 4, a, styleHead10, item.Value.Split('|')[0]);
        //            sheet3.AddMergedRegion(new CellRangeAddress(4, 4, a, a));
        //            a++;
        //        }
        //        SetExcelCell(sheet3, 4, dicldapList3.Count + 1, styleHead10, "合計");
        //        sheet3.AddMergedRegion(new CellRangeAddress(4, 4, dicldapList3.Count + 1, dicldapList3.Count + 1));

        //        //*扣押案件類型 line3-lineN 
        //        caseKind = "";//*去重複
        //        rows = 4;//定義行數
        //        for (int i = 0; i < dtCase3.Rows.Count; i++)
        //        {
        //            if (caseKind != dtCase3.Rows[i]["New_CaseKind"].ToString())
        //            {
        //                rows = rows + 1;
        //                SetExcelCell(sheet3, rows, 0, styleHead10, dtCase3.Rows[i]["New_CaseKind"].ToString());
        //                sheet3.AddMergedRegion(new CellRangeAddress(rows, rows, 0, 0));
        //                SetExcelCell(sheet3, rows, dicldapList3.Count + 1, styleHead10, "0");
        //                sheet3.AddMergedRegion(new CellRangeAddress(rows, rows, dicldapList3.Count + 1, dicldapList3.Count + 1));
        //                caseKind = dtCase3.Rows[i]["New_CaseKind"].ToString();
        //                for (int j = 0; j < dicldapList3.Count; j++)//*初始表格賦初值 
        //                {
        //                    SetExcelCell(sheet3, rows, j + 1, styleHead10, "0");
        //                    sheet3.AddMergedRegion(new CellRangeAddress(rows, rows, j + 1, j + 1));
        //                }
        //            }
        //        }

        //        //*合計 lineLast
        //        SetExcelCell(sheet3, rows + 1, 0, styleHead10, "合計");
        //        sheet3.AddMergedRegion(new CellRangeAddress(rows + 1, rows + 1, 0, 0));
        //        for (int j = 0; j < dicldapList3.Count; j++)//*初始表格賦初值 
        //        {
        //            SetExcelCell(sheet3, rows + 1, j + 1, styleHead10, "0");
        //            sheet3.AddMergedRegion(new CellRangeAddress(rows + 1, rows + 1, j + 1, j + 1));
        //        }
        //        #endregion
        //        #region body3
        //        caseExcel = "";//案件類型
        //        rowsExcel = 4;//行數
        //        rowscountExcel = 0;//最後一列合計
        //        rowstatolExcel = 0;//總合計  
        //        for (int iRow = 0; iRow < dtCase3.Rows.Count; iRow++)//根據案件類型進行循環
        //        {
        //            foreach (var item in dicldapList3)
        //            {
        //                int irows3 = Convert.ToInt32(item.Value.Split('|')[1]);
        //                if (dtCase3.Rows[iRow]["New_CaseKind"].ToString() == caseExcel)//重複同一案件類型的數據
        //                {
        //                    if (dtCase3.Rows[iRow]["ApproveUser"].ToString() == item.Key)
        //                    {
        //                        SetExcelCell(sheet3, rowsExcel, irows3, styleHead10, dtCase3.Rows[iRow]["case_num"].ToString());
        //                        SetExcelCell(sheet3, rows + 1, irows3, styleHead10, dtCase3.Rows[iRow]["UserCount"].ToString());//最後一行合計
        //                        rowscountExcelresult = Convert.ToInt32(dtCase3.Rows[iRow]["case_num"].ToString());//每格資料
        //                        rowscountExcel += rowscountExcelresult;
        //                        rowstatolExcel += rowscountExcelresult;
        //                    }
        //                    SetExcelCell(sheet3, rowsExcel, dicldapList3.Count + 1, styleHead10, rowscountExcel.ToString());//最後一列合計
        //                }
        //                else//不重複的案件類型
        //                {
        //                    rowscountExcel = 0;
        //                    rowsExcel = rowsExcel + 1;
        //                    if (dtCase3.Rows[iRow]["ApproveUser"].ToString() == item.Key)
        //                    {
        //                        SetExcelCell(sheet3, rowsExcel, irows3, styleHead10, dtCase3.Rows[iRow]["case_num"].ToString());
        //                        SetExcelCell(sheet3, rows + 1, irows3, styleHead10, dtCase3.Rows[iRow]["UserCount"].ToString());//最後一行合計
        //                        rowscountExcelresult = Convert.ToInt32(dtCase3.Rows[iRow]["case_num"].ToString());//第一條不重複的數據儲存下值
        //                        rowscountExcel += rowscountExcelresult;
        //                        rowstatolExcel += rowscountExcelresult;
        //                    }
        //                    caseExcel = dtCase3.Rows[iRow]["New_CaseKind"].ToString();
        //                    SetExcelCell(sheet3, rowsExcel, dicldapList3.Count + 1, styleHead10, rowscountExcel.ToString());//最後一列合計      
        //                }
        //            }
        //        }
        //        SetExcelCell(sheet3, rows + 1, dicldapList3.Count + 1, styleHead10, rowstatolExcel.ToString());//總合計
        //        #endregion
        //    }

        //    MemoryStream ms = new MemoryStream();
        //    workbook.Write(ms);
        //    ms.Flush();
        //    ms.Position = 0;
        //    workbook = null;
        //    return ms;
        //}
        //#endregion

        //20170811 RC RQ-2015-019666-020 派件至跨單位(原產生邏輯程式寫死，組織異動必出問題，改為參數) 宏祥 add start
        #region 電子發文統計表
        public MemoryStream ListReportExcel_Send2(CaseClosedQuery model, IList<PARMCode> listCode)
        {
           IWorkbook workbook = new HSSFWorkbook();
           int Departmentcount = listCode.Count();

           //ISheet sheet = null;
           //ISheet sheet2 = null;
           //ISheet sheet3 = null;
           //Dictionary<string, string> dicldapList = new Dictionary<string, string>();
           //Dictionary<string, string> dicldapList2 = new Dictionary<string, string>();
           //Dictionary<string, string> dicldapList3 = new Dictionary<string, string>();
           //DataTable dtCase = new DataTable();//導出資料
           //DataTable dtCase2 = new DataTable();
           //DataTable dtCase3 = new DataTable();

           int rowscountExcelresult = 0;//合計參數
           string caseExcel = "";//案件類型
           int rowsExcel = 4;//行數
           int rowscountExcel = 0;//最後一列合計
           int rowstatolExcel = 0;//總合計 
           int sort = 1;

           #region def style
           ICellStyle styleHead12 = workbook.CreateCellStyle();
           IFont font12 = workbook.CreateFont();
           font12.FontHeightInPoints = 12;
           font12.FontName = "新細明體";
           styleHead12.FillPattern = FillPattern.SolidForeground;
           styleHead12.FillForegroundColor = HSSFColor.White.Index;
           styleHead12.BorderTop = BorderStyle.None;
           styleHead12.BorderLeft = BorderStyle.None;
           styleHead12.BorderRight = BorderStyle.None;
           styleHead12.BorderBottom = BorderStyle.None;
           styleHead12.WrapText = true;
           styleHead12.Alignment = HorizontalAlignment.Center;
           styleHead12.VerticalAlignment = VerticalAlignment.Center;
           styleHead12.SetFont(font12);

           ICellStyle styleHead10 = workbook.CreateCellStyle();
           IFont font10 = workbook.CreateFont();
           font10.FontHeightInPoints = 10;
           font10.FontName = "新細明體";
           styleHead10.FillPattern = FillPattern.SolidForeground;
           styleHead10.FillForegroundColor = HSSFColor.White.Index;
           styleHead10.BorderTop = BorderStyle.Thin;
           styleHead10.BorderLeft = BorderStyle.Thin;
           styleHead10.BorderRight = BorderStyle.Thin;
           styleHead10.BorderBottom = BorderStyle.Thin;
           styleHead10.WrapText = true;
           styleHead10.Alignment = HorizontalAlignment.Left;
           styleHead10.VerticalAlignment = VerticalAlignment.Center;
           styleHead10.SetFont(font10);
           #endregion

           //判斷科別搜尋條件
           if (model.Depart != "0")
           {
              for (int k = 0; k < Departmentcount; k++)
              {
                 if (model.Depart == (k + 1).ToString())
                 {
                    ISheet sheet = null;
                    Dictionary<string, string> dicldapList = new Dictionary<string, string>();
                    DataTable dtCase = new DataTable();

                    sheet = workbook.CreateSheet(listCode[k].CodeDesc);
                    dtCase = GetCaseSend2(model, listCode[k].CodeDesc);

                    //判斷人員
                    foreach (DataRow dr in dtCase.Rows)
                    {
                       if (!dicldapList.Keys.Contains(dr["ApproveUser"].ToString()))
                       {
                          dicldapList.Add(dr["ApproveUser"].ToString(), dr["EmpName"].ToString() + "|" + sort);
                          sort++;
                       }
                    }

                    if (dicldapList.Count > 2)
                    {
                       SetExcelCell(sheet, 1, dicldapList.Count + 1, styleHead12, listCode[k].CodeDesc);
                       sheet.AddMergedRegion(new CellRangeAddress(1, 1, dicldapList.Count + 1, dicldapList.Count + 1));
                    }
                    else
                    {
                       SetExcelCell(sheet, 1, 4, styleHead12, listCode[k].CodeDesc);
                       sheet.AddMergedRegion(new CellRangeAddress(1, 1, 4, 4));
                    }

                    string caseKind = "";//*去重複
                    int rows = 4;//定義行數

                    #region title
                    //*大標題 line0
                    SetExcelCell(sheet, 0, 0, styleHead12, "電子發文統計表");
                    sheet.AddMergedRegion(new CellRangeAddress(0, 0, 0, dicldapList.Count + 1));

                    //*查詢條件 line1
                    SetExcelCell(sheet, 1, 0, styleHead12, "");
                    sheet.AddMergedRegion(new CellRangeAddress(1, 1, 0, 0));
                    SetExcelCell(sheet, 1, 1, styleHead12, "");
                    sheet.AddMergedRegion(new CellRangeAddress(1, 1, 1, 2));


                    SetExcelCell(sheet, 2, 0, styleHead12, "電子發文上傳日：");
                    sheet.AddMergedRegion(new CellRangeAddress(2, 2, 0, 0));
                    SetExcelCell(sheet, 2, 1, styleHead12, model.SendUpDateStart + '~' + model.SendUpDateEnd);
                    sheet.AddMergedRegion(new CellRangeAddress(2, 2, 1, 2));

                    SetExcelCell(sheet, 3, 0, styleHead12, "發文方式：");
                    sheet.AddMergedRegion(new CellRangeAddress(3, 3, 0, 0));
                    SetExcelCell(sheet, 3, 1, styleHead12, model.SendKind);
                    sheet.AddMergedRegion(new CellRangeAddress(3, 3, 1, 2));

                    if (dicldapList.Count > 2)
                    {
                       SetExcelCell(sheet, 1, dicldapList.Count, styleHead12, "部門別/科別");
                       sheet.AddMergedRegion(new CellRangeAddress(1, 1, dicldapList.Count, dicldapList.Count));
                    }
                    else
                    {
                       SetExcelCell(sheet, 1, 3, styleHead12, "部門別/科別");
                       sheet.AddMergedRegion(new CellRangeAddress(1, 1, 3, 3));
                       sheet.SetColumnWidth(3, 100 * 30);
                    }

                    //*結果集表頭 line2
                    SetExcelCell(sheet, 4, 0, styleHead10, "處理人員");
                    sheet.AddMergedRegion(new CellRangeAddress(4, 4, 0, 0));
                    sheet.SetColumnWidth(0, 100 * 50);

                    //依次排列人員名稱
                    int a = 1;
                    foreach (var item in dicldapList)
                    {
                       SetExcelCell(sheet, 4, a, styleHead10, item.Value.Split('|')[0]);
                       sheet.AddMergedRegion(new CellRangeAddress(4, 4, a, a));
                       a++;
                    }

                    SetExcelCell(sheet, 4, dicldapList.Count + 1, styleHead10, "合計");
                    sheet.AddMergedRegion(new CellRangeAddress(4, 4, dicldapList.Count + 1, dicldapList.Count + 1));

                    //*扣押案件類型 line5-lineN 
                    for (int i = 0; i < dtCase.Rows.Count; i++)
                    {
                       if (caseKind != dtCase.Rows[i]["New_CaseKind"].ToString())
                       {
                          rows = rows + 1;
                          SetExcelCell(sheet, rows, 0, styleHead10, dtCase.Rows[i]["New_CaseKind"].ToString());
                          sheet.AddMergedRegion(new CellRangeAddress(rows, rows, 0, 0));
                          SetExcelCell(sheet, rows, dicldapList.Count + 1, styleHead10, "0");
                          sheet.AddMergedRegion(new CellRangeAddress(rows, rows, dicldapList.Count + 1, dicldapList.Count + 1));
                          caseKind = dtCase.Rows[i]["New_CaseKind"].ToString();
                          for (int j = 0; j < dicldapList.Count; j++)//*初始表格賦初值 
                          {
                             SetExcelCell(sheet, rows, j + 1, styleHead10, "0");
                             sheet.AddMergedRegion(new CellRangeAddress(rows, rows, j + 1, j + 1));
                          }
                       }
                    }

                    //*合計 lineLast
                    SetExcelCell(sheet, rows + 1, 0, styleHead10, "合計");
                    sheet.AddMergedRegion(new CellRangeAddress(rows + 1, rows + 1, 0, 0));

                    for (int j = 0; j < dicldapList.Count; j++)//*初始表格賦初值 
                    {
                       SetExcelCell(sheet, rows + 1, j + 1, styleHead10, "0");
                       sheet.AddMergedRegion(new CellRangeAddress(rows + 1, rows + 1, j + 1, j + 1));
                    }
                    #endregion

                    #region  body
                    for (int iRow = 0; iRow < dtCase.Rows.Count; iRow++)//根據案件類型進行循環
                    {
                       foreach (var item in dicldapList)
                       {
                          int irows = Convert.ToInt32(item.Value.Split('|')[1]);
                          if (caseExcel == dtCase.Rows[iRow]["New_CaseKind"].ToString())//重複同一案件類型的數據
                          {
                             if (item.Key == dtCase.Rows[iRow]["ApproveUser"].ToString())
                             {
                                SetExcelCell(sheet, rowsExcel, irows, styleHead10, dtCase.Rows[iRow]["case_num"].ToString());
                                rowscountExcelresult = Convert.ToInt32(dtCase.Rows[iRow]["case_num"].ToString());//每格資料
                                SetExcelCell(sheet, rows + 1, irows, styleHead10, dtCase.Rows[iRow]["UserCount"].ToString());//最後一行合計
                                rowscountExcel += rowscountExcelresult;
                                rowstatolExcel += rowscountExcelresult;
                             }
                             SetExcelCell(sheet, rowsExcel, dicldapList.Count + 1, styleHead10, rowscountExcel.ToString());//最後一列合計
                          }
                          else//不重複的案件類型
                          {
                             rowscountExcel = 0;
                             rowsExcel = rowsExcel + 1;
                             if (dtCase.Rows[iRow]["ApproveUser"].ToString() == item.Key)
                             {
                                SetExcelCell(sheet, rowsExcel, irows, styleHead10, dtCase.Rows[iRow]["case_num"].ToString());
                                rowscountExcelresult = Convert.ToInt32(dtCase.Rows[iRow]["case_num"].ToString());//第一條不重複的數據儲存下值
                                SetExcelCell(sheet, rows + 1, irows, styleHead10, dtCase.Rows[iRow]["UserCount"].ToString());//最後一行合計
                                rowscountExcel += rowscountExcelresult;
                                rowstatolExcel += rowscountExcelresult;
                             }
                             caseExcel = dtCase.Rows[iRow]["New_CaseKind"].ToString();
                             SetExcelCell(sheet, rowsExcel, dicldapList.Count + 1, styleHead10, rowscountExcel.ToString());//最後一列合計
                          }
                       }
                    }
                    SetExcelCell(sheet, rows + 1, dicldapList.Count + 1, styleHead10, rowstatolExcel.ToString());//總合計
                    #endregion
                 }
              }
           }
           else
           {
              for (int k = 0; k < Departmentcount; k++)
              {
                 ISheet sheet = null;
                 Dictionary<string, string> dicldapList = new Dictionary<string, string>();
                 DataTable dtCase = new DataTable();

                 sheet = workbook.CreateSheet(listCode[k].CodeDesc);
                 dtCase = GetCaseSend2(model, listCode[k].CodeDesc);

                 sort = 1;

                 //判斷人員
                 foreach (DataRow dr in dtCase.Rows)
                 {
                    if (!dicldapList.Keys.Contains(dr["ApproveUser"].ToString()))
                    {
                       dicldapList.Add(dr["ApproveUser"].ToString(), dr["EmpName"].ToString() + "|" + sort);
                       sort++;
                    }
                 }

                 if (dicldapList.Count > 2)
                 {
                    SetExcelCell(sheet, 1, dicldapList.Count + 1, styleHead12, listCode[k].CodeDesc);
                    sheet.AddMergedRegion(new CellRangeAddress(1, 1, dicldapList.Count + 1, dicldapList.Count + 1));
                 }
                 else
                 {
                    SetExcelCell(sheet, 1, 4, styleHead12, listCode[k].CodeDesc);
                    sheet.AddMergedRegion(new CellRangeAddress(1, 1, 4, 4));
                 }

                 string caseKind = "";//*去重複
                 int rows = 4;//title中定義行數

                 #region title
                 //*大標題 line0
                 SetExcelCell(sheet, 0, 0, styleHead12, "電子發文統計表");
                 sheet.AddMergedRegion(new CellRangeAddress(0, 0, 0, dicldapList.Count + 1));

                 //*查詢條件 line1
                 SetExcelCell(sheet, 1, 0, styleHead12, "");
                 sheet.AddMergedRegion(new CellRangeAddress(1, 1, 0, 0));
                 SetExcelCell(sheet, 1, 1, styleHead12, "");
                 sheet.AddMergedRegion(new CellRangeAddress(1, 1, 1, 2));

                 SetExcelCell(sheet, 2, 0, styleHead12, "電子發文上傳日：");
                 sheet.AddMergedRegion(new CellRangeAddress(2, 2, 0, 0));
                 SetExcelCell(sheet, 2, 1, styleHead12, model.SendUpDateStart + '~' + model.SendUpDateEnd);
                 sheet.AddMergedRegion(new CellRangeAddress(2, 2, 1, 2));

                 SetExcelCell(sheet, 3, 0, styleHead12, "發文方式：");
                 sheet.AddMergedRegion(new CellRangeAddress(3, 3, 0, 0));
                 SetExcelCell(sheet, 3, 1, styleHead12, model.SendKind);
                 sheet.AddMergedRegion(new CellRangeAddress(3, 3, 1, 2));

                 if (dicldapList.Count > 2)
                 {
                    SetExcelCell(sheet, 1, dicldapList.Count - 1, styleHead12, "部門別/科別");
                    sheet.AddMergedRegion(new CellRangeAddress(1, 1, dicldapList.Count - 1, dicldapList.Count));
                    SetExcelCell(sheet, 1, dicldapList.Count + 1, styleHead12, listCode[k].CodeDesc);
                    sheet.AddMergedRegion(new CellRangeAddress(1, 1, dicldapList.Count + 1, dicldapList.Count + 1));
                 }
                 else
                 {
                    SetExcelCell(sheet, 1, 3, styleHead12, "部門別/科別");
                    sheet.AddMergedRegion(new CellRangeAddress(1, 1, 3, 3));
                    SetExcelCell(sheet, 1, 4, styleHead12, listCode[k].CodeDesc);
                    sheet.AddMergedRegion(new CellRangeAddress(1, 1, 4, 4));
                 }

                 //*結果集表頭 line4
                 SetExcelCell(sheet, 4, 0, styleHead10, "處理人員");
                 sheet.AddMergedRegion(new CellRangeAddress(4, 4, 0, 0));
                 sheet.SetColumnWidth(0, 100 * 50);

                 //依次排列人員名稱
                 int a = 1;
                 foreach (var item in dicldapList)
                 {
                    SetExcelCell(sheet, 4, a, styleHead10, item.Value.Split('|')[0]);
                    sheet.AddMergedRegion(new CellRangeAddress(4, 4, a, a));
                    a++;
                 }

                 SetExcelCell(sheet, 4, dicldapList.Count + 1, styleHead10, "合計");
                 sheet.AddMergedRegion(new CellRangeAddress(4, 4, dicldapList.Count + 1, dicldapList.Count + 1));

                 //*扣押案件類型 line3-lineN 
                 //caseKind = "";//*去重複
                 //rows = 6;//定義行數
                 for (int i = 0; i < dtCase.Rows.Count; i++)
                 {
                    if (caseKind != dtCase.Rows[i]["New_CaseKind"].ToString())
                    {
                       rows = rows + 1;
                       SetExcelCell(sheet, rows, 0, styleHead10, dtCase.Rows[i]["New_CaseKind"].ToString());
                       sheet.AddMergedRegion(new CellRangeAddress(rows, rows, 0, 0));

                       SetExcelCell(sheet, rows, dicldapList.Count + 1, styleHead10, "0");
                       sheet.AddMergedRegion(new CellRangeAddress(rows, rows, dicldapList.Count + 1, dicldapList.Count + 1));

                       caseKind = dtCase.Rows[i]["New_CaseKind"].ToString();
                       for (int j = 0; j < dicldapList.Count; j++)//*初始表格賦初值 
                       {
                          SetExcelCell(sheet, rows, j + 1, styleHead10, "0");
                          sheet.AddMergedRegion(new CellRangeAddress(rows, rows, j + 1, j + 1));
                       }
                    }
                 }

                 //*合計 lineLast
                 SetExcelCell(sheet, rows + 1, 0, styleHead10, "合計");
                 sheet.AddMergedRegion(new CellRangeAddress(rows + 1, rows + 1, 0, 0));
                 for (int j = 0; j < dicldapList.Count; j++)//*初始表格賦初值 
                 {
                    SetExcelCell(sheet, rows + 1, j + 1, styleHead10, "0");
                    sheet.AddMergedRegion(new CellRangeAddress(rows + 1, rows + 1, j + 1, j + 1));
                 }
                 #endregion
                 #region body
                 caseExcel = "";//案件類型
                 rowsExcel = 4;//行數
                 rowscountExcel = 0;//最後一列合計
                 rowstatolExcel = 0;//總合計      
                 for (int iRow = 0; iRow < dtCase.Rows.Count; iRow++)//根據案件類型進行循環
                 {
                    foreach (var item in dicldapList)
                    {
                       int irows2 = Convert.ToInt32(item.Value.Split('|')[1]);
                       if (dtCase.Rows[iRow]["New_CaseKind"].ToString() == caseExcel)//重複同一案件類型的數據
                       {
                          if (dtCase.Rows[iRow]["ApproveUser"].ToString() == item.Key)
                          {
                             SetExcelCell(sheet, rowsExcel, irows2, styleHead10, dtCase.Rows[iRow]["case_num"].ToString());
                             SetExcelCell(sheet, rows + 1, irows2, styleHead10, dtCase.Rows[iRow]["UserCount"].ToString());//最後一行合計
                             rowscountExcelresult = Convert.ToInt32(dtCase.Rows[iRow]["case_num"].ToString());//每格資料
                             rowscountExcel += rowscountExcelresult;
                             rowstatolExcel += rowscountExcelresult;
                          }
                          SetExcelCell(sheet, rowsExcel, dicldapList.Count + 1, styleHead10, rowscountExcel.ToString());//最後一列合計
                       }
                       else//不重複的案件類型
                       {
                          rowscountExcel = 0;
                          rowsExcel = rowsExcel + 1;
                          if (dtCase.Rows[iRow]["ApproveUser"].ToString() == item.Key)
                          {
                             SetExcelCell(sheet, rowsExcel, irows2, styleHead10, dtCase.Rows[iRow]["case_num"].ToString());
                             SetExcelCell(sheet, rows + 1, irows2, styleHead10, dtCase.Rows[iRow]["UserCount"].ToString());//最後一行合計
                             rowscountExcelresult = Convert.ToInt32(dtCase.Rows[iRow]["case_num"].ToString());//第一條不重複的數據儲存下值
                             rowscountExcel += rowscountExcelresult;
                             rowstatolExcel += rowscountExcelresult;
                          }
                          caseExcel = dtCase.Rows[iRow]["New_CaseKind"].ToString();
                          SetExcelCell(sheet, rowsExcel, dicldapList.Count + 1, styleHead10, rowscountExcel.ToString());//最後一列合計      
                       }
                    }
                 }
                 SetExcelCell(sheet, rows + 1, dicldapList.Count + 1, styleHead10, rowstatolExcel.ToString());//總合計
                 #endregion
              }
           }

           MemoryStream ms = new MemoryStream();
           workbook.Write(ms);
           ms.Flush();
           ms.Position = 0;
           workbook = null;
           return ms;
        }
        #endregion
        //20170811 RC RQ-2015-019666-020 派件至跨單位(原產生邏輯程式寫死，組織異動必出問題，改為參數) 宏祥 add end

        public DataTable GetCaseSend2(CaseClosedQuery model, string depart)
        {
            string sqlWhere = "";
            string sqlWhereTmp = "";
            string sqlWhere1 = "";
            string sqlWhere2 = "";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("@depart", depart));

            //類型
            if (!string.IsNullOrEmpty(model.CaseKind))
            {
                sqlWhere += @" AND CaseKind LIKE @CaseKind ";
                Parameter.Add(new CommandParameter("@CaseKind", "" + model.CaseKind.Trim() + ""));
            }
            if (!string.IsNullOrEmpty(model.CaseKind2))
            {
                sqlWhere += @" AND CaseKind2 LIKE @CaseKind2 ";
                Parameter.Add(new CommandParameter("@CaseKind2", "" + model.CaseKind2.Trim() + ""));
            }

            //收件日期
            if (!string.IsNullOrEmpty(model.ReceiveDateStart))
            {
                sqlWhere += @" AND ReceiveDate >= @ReceiveDateStart";
                Parameter.Add(new CommandParameter("@ReceiveDateStart", model.ReceiveDateStart));
            }
            if (!string.IsNullOrEmpty(model.ReceiveDateEnd))
            {
                string receiveDateEnd = Convert.ToDateTime(UtlString.FormatDateString(model.ReceiveDateEnd)).AddDays(1).ToString("yyyyMMdd");
                sqlWhere += @" AND ReceiveDate < @ReceiveDateEnd ";
                Parameter.Add(new CommandParameter("@ReceiveDateEnd", receiveDateEnd));
            }
			//發文日期
            if (!string.IsNullOrEmpty(model.SendDateStart))
            {
                sqlWhereTmp += @" AND SendDate >= @SendDateStart";
                Parameter.Add(new CommandParameter("@SendDateStart", model.SendDateStart));
            }
            if (!string.IsNullOrEmpty(model.SendDateEnd))
            {
                string sendDateEnd = Convert.ToDateTime(UtlString.FormatDateString(model.SendDateEnd)).AddDays(1).ToString("yyyy/MM/dd");
                sqlWhereTmp += @" AND SendDate < @SendDateEnd ";
                Parameter.Add(new CommandParameter("@SendDateEnd", sendDateEnd));
            }
			//電子發文上傳日
            if (!string.IsNullOrEmpty(model.SendUpDateStart))
            {
                sqlWhereTmp += @" AND SendUpDate >= @SendUpDateStart";
                Parameter.Add(new CommandParameter("@SendUpDateStart", model.SendUpDateStart));
            }
            if (!string.IsNullOrEmpty(model.SendUpDateEnd))
            {
                string SendUpDateEnd = Convert.ToDateTime(UtlString.FormatDateString(model.SendUpDateEnd)).AddDays(1).ToString("yyyy/MM/dd");
                sqlWhereTmp += @" AND SendUpDate < @SendUpDateEnd ";
                Parameter.Add(new CommandParameter("@SendUpDateEnd", SendUpDateEnd));
            }
			//發文方式
            if (!string.IsNullOrEmpty(model.SendKind))
            {
                sqlWhereTmp += @" AND SendKind = @SendKind ";
                Parameter.Add(new CommandParameter("@SendKind", model.SendKind));
            }
			if (!string.IsNullOrEmpty(model.SendDateStart) || !string.IsNullOrEmpty(model.SendDateEnd) || !string.IsNullOrEmpty(model.SendKind) || !string.IsNullOrEmpty(model.SendUpDateStart) || !string.IsNullOrEmpty(model.SendUpDateEnd))
			{
				sqlWhere1 += @" AND CaseId IN (SELECT CaseId  FROM CaseSendSetting AS CSS   where [Template] <> '支付' " + sqlWhereTmp + ") ";
				sqlWhere2 += @" AND CaseId IN (SELECT CaseId  FROM CaseSendSetting AS CSS   where [Template] = '支付' " + sqlWhereTmp + ") ";
			}
            //主管放行日
            if (!string.IsNullOrEmpty(model.ApproveDateStart))
            {
                sqlWhere += @" AND ApproveDate2 >= @ApproveDateStart";
                Parameter.Add(new CommandParameter("@ApproveDateStart", model.ApproveDateStart));
            }
            if (!string.IsNullOrEmpty(model.ApproveDateEnd))
            {
                string approveDateEnd = Convert.ToDateTime(UtlString.FormatDateString(model.ApproveDateEnd)).AddDays(1).ToString("yyyyMMdd");
                sqlWhere += @" AND ApproveDate2 < @ApproveDateEnd ";
                Parameter.Add(new CommandParameter("@ApproveDateEnd", approveDateEnd));
            }

            var sqlStr = @"WITH 
                            BaseTable AS(
	                            SELECT (CaseKind+'-'+CaseKind2) AS New_CaseKind,ApproveUser AS ApproveUser  FROM  CaseMaster WHERE  ApproveUser IS NOT NULL  " + sqlWhere.Replace("ApproveDate2", "ApproveDate") + sqlWhere1 + @"
	                            UNION ALL 
	                            SELECT (CaseKind+'-'+CaseKind2) AS New_CaseKind,ApproveUser2 AS ApproveUser FROM  CaseMaster WHERE  ApproveUser IS NOT NULL AND ApproveUser2 IS NOT NULL  " + sqlWhere + sqlWhere2 + @"
                            ),
                            UserByKind AS(
	                            SELECT New_CaseKind,ApproveUser,COUNT(1) AS case_num
	                            FROM BaseTable
	                            GROUP BY New_CaseKind, ApproveUser
                            ),
                            UserCount AS(
	                            SELECT ApproveUser,COUNT(1) AS UserCount 
	                            FROM BaseTable 
	                            GROUP BY ApproveUser
                            )
                            SELECT  
	                            A.*, 
	                            C.EmpName,
	                            userCount.UserCount
                            FROM  UserByKind AS A 
                            LEFT JOIN  UserCount ON A.ApproveUser=userCount.ApproveUser
                            LEFT OUTER JOIN [V_AgentAndDept] AS C ON C.EmpID = A.ApproveUser
                            WHERE C.SectionName = @depart
                            ORDER BY New_CaseKind, EmpID";
            DataTable dt = Search(sqlStr);
            return dt;
        }

        public ICell SetExcelCell(ISheet sheet, int rowNum, int colNum, ICellStyle style, string value)
        {

            IRow row = sheet.GetRow(rowNum) ?? sheet.CreateRow(rowNum);
            ICell cell = row.GetCell(colNum) ?? row.CreateCell(colNum);
            cell.CellStyle = style;
            cell.SetCellValue(value);
            return cell;
        }
    }
}
