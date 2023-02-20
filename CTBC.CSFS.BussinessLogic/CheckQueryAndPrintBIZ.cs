﻿using CTBC.CSFS.Pattern;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CTBC.CSFS.Models;
using System.Data;
using System.IO;
using NPOI.HSSF.UserModel;

namespace CTBC.CSFS.BussinessLogic
{
    public class CheckQueryAndPrintBIZ : CommonBIZ
    {
        public CheckQueryAndPrintBIZ(AppController AppController)
            : base(AppController)
        { }

        public CheckQueryAndPrintBIZ()
        { }
        //Add by zhangwei 20180315 start
        public IList<CheckQueryAndPrint> GetData(CheckQueryAndPrint model, int pageNum, string strSortExpression, string strSortDirection, string UserId)
        {
            try
            {
                string sqlWhere = "";
                base.PageIndex = pageNum;
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@pageS", (base.PageSize * (base.PageIndex - 1)) + 1));
                base.Parameter.Add(new CommandParameter("@pageE", base.PageSize * base.PageIndex));
                if (model.Type == "3"  )
                {
                    Parameter.Add(new CommandParameter("@sendtype", "1"));
                }
                if (  model.Type == "4")
                {
                    Parameter.Add(new CommandParameter("@sendtype", "2"));
                }
                string strSql = "";
                if (model.Type == "1" || model.Type == "3" || model.Type == "4" || model.Type == "5")
                {
                    if (!string.IsNullOrEmpty(model.CheckDate))
                    {
                        sqlWhere += " AND CONVERT(varchar(100), M.[PayDate], 111)=@CheckDate";
                        Parameter.Add(new CommandParameter("@CheckDate", model.CheckDate));
                    }
                    if (model.Type == "3" || model.Type == "1" )
                    {
                        if (!string.IsNullOrEmpty(model.ReceivePerson))
                        {
                            sqlWhere += " AND ReceivePerson =@ReceivePerson";
                            Parameter.Add(new CommandParameter("@ReceivePerson", model.ReceivePerson));
                        }
                    }
                    if (model.Type == "4")
                    {
                        if (!string.IsNullOrEmpty(model.ReceivePerson))
                        {
                            Parameter.Add(new CommandParameter("@GovName", model.ReceivePerson));
                        }
                    }
                    //if (model.Type == "3")
                    //{
                    //    sqlWhere += " AND D.SendType = 1 ";
                    //    //Parameter.Add(new CommandParameter("@SendType", 1));
                    //}
                    //if (model.Type == "4")
                    //{
                    //    sqlWhere += " AND D.SendType = 2 ";
                    //    //Parameter.Add(new CommandParameter("@SendType", 1));
                    //}
                    if (!string.IsNullOrEmpty(model.AmtConsistentType))
                    {
                        if (model.AmtConsistentType == "1")//支付金額與解扣金額相符
                        {
                            sqlWhere += " and P.[Money]=CS.TripAmount";//P.SeizureAmountSUB IR-8004
                        }
                        else if (model.AmtConsistentType == "2")
                        {
                            sqlWhere += " and P.[Money]<>CS.TripAmount";//P.SeizureAmountSUB IR-8004
                        }
                    }
                    if (!string.IsNullOrEmpty(model.CheckNoStart) && !string.IsNullOrEmpty(model.CheckNoEnd))
                    {
                        sqlWhere += " and P.CheckNo>=@CheckNoStart and P.CheckNo<=@CheckNoEnd";
                        Parameter.Add(new CommandParameter("@CheckNoStart", model.CheckNoStart));
                        Parameter.Add(new CommandParameter("@CheckNoEnd", model.CheckNoEnd));
                    }
                    if (model.Type == "1" || model.Type == "2")
                    {
                        strSql = @";with T1 
	                        as
	                        (
                                SELECT 
	                                ROW_NUMBER() OVER (ORDER BY P.CheckNo ASC ) num,
	                                P.PayeeId,
	                                P.CaseId,
	                                P.CheckNo,
	                                M.CaseNo,
	                                S.SendWord +'字'+ S.SendNo + '號' AS SendNo,
	                                CONVERT(varchar(100), M.PayDate, 111) as CheckDate,
	                                P.ReceivePerson,
	                                P.[Money] ,
	                                P.Fee,
	                                M.GovNo,
	                                M.CaseKind2,
	                                M.AgentUser ,
                                    M.Status--,
                                    --CS.TripAmount as SeizureAmountSUB
                                    --P.SeizureAmountSUB
	                                FROM CaseMaster AS M
                                INNER JOIN CasePayeeSetting AS P ON M.CaseId = P.CaseId
                                LEFT OUTER JOIN CaseSendSetting AS S ON P.CaseId = S.CaseId AND P.SendId = S.SerialID
                                --LEFT OUTER JOIN CaseSeizure AS CS ON M.CaseId = CS.PayCaseId 
                                WHERE 1=1 " + sqlWhere + @"
	                        ),T2 as
	                        (
		                        select *, row_number() over (order by CheckNo asc , CaseNo asc)  RowNum
		                        from T1 
								),T3 as(
								select T2.*,(select sum(TripAmount) from CaseSeizure CS where T2.CaseId = CS.PayCaseId) as SeizureAmountSUB from T2 
	                        ),T4 as 
	                        (
		                        select *,(select max(RowNum) from T3) maxnum,(select sum([Money]) from T3) TotalPayment,
		                        (select sum(SeizureAmountSUB) from T3) TotalSeizureAmount,(select sum(Fee) from T3) TotalFee
		                        from T3 
		                        where rownum between @pageS and @pageE
	                        )
	                        select a.* from T4 a order by a.RowNum";
                    }
                    if (model.Type == "3" || model.Type == "4" || model.Type == "5")
                    {
                        if (model.Type == "3")
                        {
                            strSql = @";with T1 
	                        as
	                        (
                                SELECT 
	                                ROW_NUMBER() OVER (ORDER BY P.CheckNo ASC ) num,
	                                P.PayeeId,
	                                P.CaseId,
	                                P.CheckNo,
	                                M.CaseNo,
	                                S.SendWord +'字'+ S.SendNo + '號' AS SendNo,
	                                CONVERT(varchar(100), M.PayDate, 111) as CheckDate,
	                                P.ReceivePerson,
	                                P.[Money] ,
	                                P.Fee,
	                                M.GovNo,
	                                M.CaseKind2,
	                                M.AgentUser ,
                                    M.Status--,
                                    --CS.TripAmount as SeizureAmountSUB
                                    --P.SeizureAmountSUB
	                                FROM CaseMaster AS M
                                INNER JOIN CasePayeeSetting AS P ON M.CaseId = P.CaseId
                                LEFT OUTER JOIN CaseSendSetting AS S ON P.CaseId = S.CaseId AND P.SendId = S.SerialID
                                --LEFT OUTER JOIN CaseSeizure AS CS ON M.CaseId = CS.PayCaseId 
                                WHERE 1=1 and ISNULL(P.CheckNo,'')  <> '' " + sqlWhere + @"
	                        ),T2 as
	                        (
		                        select *, row_number() over (order by CheckNo asc , CaseNo asc)  RowNum
		                        from T1 
								),T3 as(
								select T2.*,(select sum(TripAmount) from CaseSeizure CS where T2.CaseId = CS.PayCaseId) as SeizureAmountSUB from T2 
	                        ),T4 as 
	                        (
		                        select *,(select max(RowNum) from T3) maxnum,(select sum([Money]) from T3) TotalPayment,
		                        (select sum(SeizureAmountSUB) from T3) TotalSeizureAmount,(select sum(Fee) from T3) TotalFee
		                        from T3 
		                        where rownum between @pageS and @pageE
	                        )
	                        select distinct a.*,D.SendType from T4 a 
							inner JOIN CaseSendSettingDetails   D ON a.CaseId = D.CaseId 
							 where 1=1 and D.sendtype  = @sendtype
							order by a.RowNum";
                        }

                        if (model.Type == "5")
                        {
                            strSql = @";with T1 
	                        as
	                        (
                                SELECT 
	                                ROW_NUMBER() OVER (ORDER BY P.CheckNo ASC ) num,
	                                P.PayeeId,
	                                P.CaseId,
	                                P.CheckNo,
	                                M.CaseNo,
	                                S.SendWord +'字'+ S.SendNo + '號' AS SendNo,
	                                CONVERT(varchar(100), M.PayDate, 111) as CheckDate,
	                                P.ReceivePerson,
	                                P.[Money] ,
	                                P.Fee,
	                                M.GovNo,
	                                M.CaseKind2,
	                                M.AgentUser ,
                                    M.Status--,
                                    --CS.TripAmount as SeizureAmountSUB
                                    --P.SeizureAmountSUB
	                                FROM CaseMaster AS M
                                INNER JOIN CasePayeeSetting AS P ON M.CaseId = P.CaseId
                                LEFT OUTER JOIN CaseSendSetting AS S ON P.CaseId = S.CaseId AND P.SendId = S.SerialID
                                --LEFT OUTER JOIN CaseSeizure AS CS ON M.CaseId = CS.PayCaseId 
                                WHERE 1=1 and ISNULL(P.CheckNo,'')  <> '' " + sqlWhere + @"
	                        ),T2 as
	                        (
		                        select *, row_number() over (order by CheckNo asc , CaseNo asc)  RowNum
		                        from T1 
								),T3 as(
								select T2.*,(select sum(TripAmount) from CaseSeizure CS where T2.CaseId = CS.PayCaseId) as SeizureAmountSUB from T2 
	                        ),T4 as 
	                        (
		                        select *,(select max(RowNum) from T3) maxnum,(select sum([Money]) from T3) TotalPayment,
		                        (select sum(SeizureAmountSUB) from T3) TotalSeizureAmount,(select sum(Fee) from T3) TotalFee
		                        from T3 
		                        where rownum between @pageS and @pageE
	                        )
	                        select distinct a.* from T4 a 
							inner JOIN CaseSendSettingDetails   D ON a.CaseId = D.CaseId 
							 where 1=1 and Convert(int,ISNULL(Fee,0)) > 0
							order by a.RowNum";
                        }
                        if (model.Type == "4")
                        {
                            strSql = @";with T1 
	                        as
	                        (
                                SELECT 
	                                ROW_NUMBER() OVER (ORDER BY P.CheckNo ASC ) num,
	                                P.PayeeId,
	                                P.CaseId,
	                                P.CheckNo,
	                                M.CaseNo,
	                                S.SendWord +'字'+ S.SendNo + '號' AS SendNo,
	                                CONVERT(varchar(100), M.PayDate, 111) as CheckDate,
	                                P.ReceivePerson,
	                                P.[Money] ,
	                                P.Fee,
	                                M.GovNo,
	                                M.CaseKind2,
	                                M.AgentUser ,
                                    M.Status--,
                                    --CS.TripAmount as SeizureAmountSUB
                                    --P.SeizureAmountSUB
	                                FROM CaseMaster AS M
                                INNER JOIN CasePayeeSetting AS P ON M.CaseId = P.CaseId
                                LEFT OUTER JOIN CaseSendSetting AS S ON P.CaseId = S.CaseId AND P.SendId = S.SerialID
                                --LEFT OUTER JOIN CaseSeizure AS CS ON M.CaseId = CS.PayCaseId 
                                WHERE 1=1 and ISNULL(P.CheckNo,'')  <> '' " + sqlWhere + @"
	                        ),T2 as
	                        (
		                        select *, row_number() over (order by CheckNo asc , CaseNo asc)  RowNum
		                        from T1 
								),T3 as(
								select T2.*,(select sum(TripAmount) from CaseSeizure CS where T2.CaseId = CS.PayCaseId) as SeizureAmountSUB from T2 
	                        ),T4 as 
	                        (
		                        select *,(select max(RowNum) from T3) maxnum,(select sum([Money]) from T3) TotalPayment,
		                        (select sum(SeizureAmountSUB) from T3) TotalSeizureAmount,(select sum(Fee) from T3) TotalFee
		                        from T3 
		                        where rownum between @pageS and @pageE
	                        )
	                        select distinct a.*,D.SendType from T4 a 
							inner JOIN CaseSendSettingDetails   D ON a.CaseId = D.CaseId 
							 where 1=1 and D.sendtype  = @sendtype  and  D.GovName like '%"+ model.ReceivePerson + "%' order by a.RowNum"; 
							
                        }
                    }
                }
                else
                {
                    //* 支付明細
                    if (!string.IsNullOrEmpty(model.CheckDate))
                    {
                        sqlWhere += " AND CONVERT(varchar(100),M.[PayDate], 111)=@CheckDate ";
                        Parameter.Add(new CommandParameter("@CheckDate", model.CheckDate));
                    }
                    //strSql = @";with T1 
                    //     as
                    //     (
                    //         SELECT row_number() over (order by M.CaseId asc ) num,
                    //            M.CaseId,
                    //            M.CaseNo,
                    //            CONVERT(varchar(100), M.CreatedDate, 111) as CreatedDate,
                    //            S.Account,
                    //            S.CustId,
                    //            S.CustName,
                    //            s.PayAmount,
                    //            (select sum(PayAmount) from CaseSeizure where PayCaseId=S.PayCaseId AND CustName= s.CustName) as sum1,
                    //            M.GovNo,
                    //            m.CaseKind2,
                    //            M.AgentUser,
                    //            M.Status
                    //            FROM CaseMaster AS M
                    //            LEFT OUTER JOIN CaseSeizure AS S ON M.CaseId = S.PayCaseId
                    //            WHERE S.CaseId IS NOT NULL  " + sqlWhere + @" 
                    //     ),T2 as
                    //     (
                    //      select *, row_number() over (order by " + strSortExpression + " " + strSortDirection + @" ) RowNum
                    //      from T1
                    //     ),T3 as 
                    //     (
                    //      select *,(select max(RowNum) from T2) maxnum from T2 
                    //      where rownum between @pageS and @pageE
                    //     )
                    //     select a.* from T3 a order by a.RowNum";
                    strSql = @";With T0 as
                            (select CaseId ,sum(Fee) as Fee,sum(SeizureAmountSUB)as SeizureAmountSUB   from CasePayeeSetting group by CaseId),
                            T1 
	                        as
	                        (
                             SELECT row_number() over (order by M.CaseId asc ) num,
                                M.CaseId,
                                M.CaseNo,
                                CONVERT(varchar(100), M.CreatedDate, 111) as CreatedDate,
                                S.Account,
                                S.CustId,
                                S.CustName,
                                s.PayAmount,
                                (select sum(PayAmount) from CaseSeizure where PayCaseId=S.PayCaseId AND CustName= s.CustName) as sum1,
                                M.GovNo,
                                m.CaseKind2,
                                M.AgentUser,
                                M.Status,
                                S.ProdCode,
                                S.Currency,
                                T0.Fee,
                                T0.SeizureAmountSUB
                                FROM CaseMaster AS M
                                left JOIN T0  ON M.CaseId = T0.CaseId
                                LEFT OUTER JOIN CaseSeizure AS S ON M.CaseId = S.PayCaseId
                                WHERE S.CaseId IS NOT NULL   " + sqlWhere + @" 
	                        ),T2 as
	                        (
		                        select *, row_number() over (order by " + strSortExpression + " " + strSortDirection + @" ) RowNum
		                        from T1
	                        ),T3 as 
	                        (
		                        select *,(select max(RowNum) from T2) maxnum,(select count(distinct CustId) from T2)as TotalID,(select sum(PayAmount) from T2) TotalPayment,
		                        (select sum(SeizureAmountSUB) from T2) TotalSeizureAmount,(select sum(Fee) from T2) TotalFee  from T2 
		                        where rownum between @pageS and @pageE
	                        ) ";
                            //if (model.Type == "5")
                            //{
                            //    strSql = strSql + "  select a.* from T3 a  where Convert(int,ISNULL(Fee,0)) > 0  order by a.RowNum ";
                            //}
                            //else
                            //{
                                strSql = strSql + " select a.* from T3 a order by a.RowNum ";
                            //}
                }
                IList<CheckQueryAndPrint> ilst = base.SearchList<CheckQueryAndPrint>(strSql);
                
                if (ilst != null)
                {
                    if (ilst.Count > 0)
                    {
                        base.DataRecords = ilst[0].maxnum;
                    }
                    else
                    {
                        base.DataRecords = 0;
                        ilst = new List<CheckQueryAndPrint>();
                    }
                    return ilst;
                }
                else
                {
                    return new List<CheckQueryAndPrint>();
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        //Add by zhangwei 20180315 end
        public DataTable GetQueryAndPrintByCaseIdList(List<string> payeeIdList)
        {
            /* adam 改成 以 發文日期 當成支付日期
            string strsql = @"SELECT
                                P.CaseId,
                                P.ReceivePerson,
                                B.SendNo,
                                P.CheckNo,
                                P.[Money],
                                (SELECT CONVERT(varchar(100), M.PayDate, 111)) AS CheckDate ,
                                B.SendNo
                            FROM [CaseMaster] AS M
                            INNER JOIN [CasePayeeSetting] AS P ON M.CaseId = P.CaseId
                            INNER JOIN [CaseSendSetting] AS B ON P.CaseId = B.CaseId AND P.SendId = B.SerialID
                            WHERE P.CheckNo IS NOT NULL AND P.CheckNo <> '' AND (1=2 ";
             * */
            string strsql = @"SELECT
                                P.CaseId,
                                P.ReceivePerson,
                                B.SendNo,
                                P.CheckNo,
                                P.[Money],
                                (SELECT CONVERT(varchar(100), B.SendDate, 111)) AS CheckDate ,
                                B.SendNo
                            FROM [CaseMaster] AS M
                            INNER JOIN [CasePayeeSetting] AS P ON M.CaseId = P.CaseId
                            INNER JOIN [CaseSendSetting] AS B ON P.CaseId = B.CaseId AND P.SendId = B.SerialID
                            WHERE P.CheckNo IS NOT NULL AND P.CheckNo <> '' AND (1=2 ";
            Parameter.Clear();
            for (int i = 0; i < payeeIdList.Count; i++)
            {
                strsql = strsql + " OR P.PayeeId = @PayeeId" + i + " ";
                Parameter.Add(new CommandParameter("@PayeeId" + i, payeeIdList[i]));
            }
            strsql = strsql + ") ORDER BY CheckNo asc ";
            DataTable dt = Search(strsql);
            return dt != null && dt.Rows.Count > 0 ? dt : new DataTable();
        }
        public MemoryStream Excel(string[] headerColumns,CheckQueryAndPrint model)
        {
            var ms = new MemoryStream();

            IList<CheckQueryAndPrint> ilst = GetPrintData(model);
            
            if (ilst != null)
            {
                ms = ExcelExport(ilst, headerColumns,
                                                   delegate(HSSFRow dataRow, CheckQueryAndPrint dataItem)
                                                   {
                                                       //* 這裡可以針對每一個欄位做額外處理.比如日期
                                                       dataRow.CreateCell(0).SetCellValue(dataItem.num);
                                                       dataRow.CreateCell(1).SetCellValue(dataItem.CaseNo);
                                                       dataRow.CreateCell(2).SetCellValue(dataItem.CreatedDate);
                                                       if (dataItem.Account.Trim().Length > 15)
                                                       {
                                                           if (dataItem.Currency == "TWD")
                                                           {
                                                               dataRow.CreateCell(3).SetCellValue(dataItem.Account.Substring(dataItem.Account.Length-12,12));
                                                           }
                                                           else
                                                           {
                                                               string strAccount = dataItem.Account.Substring(0, dataItem.Account.Length - 3);
                                                               dataRow.CreateCell(3).SetCellValue(strAccount.Substring(strAccount.Length - 12, 12));
                                                           }
                                                       }
                                                       else
                                                       {
                                                           if (dataItem.Account.Trim().Length > 12 && dataItem.Account.Trim().Length != 14)
                                                           {
                                                               dataRow.CreateCell(3).SetCellValue(dataItem.Account.Substring(dataItem.Account.Length - 12, 12));
                                                           }
                                                           else
                                                           {
                                                               if (dataItem.Account.Trim().Length == 14)
                                                               {
                                                                   dataRow.CreateCell(3).SetCellValue(dataItem.Account.Substring(dataItem.Account.Length - 12, 12));
                                                               }
                                                               else
                                                               {
                                                                   dataRow.CreateCell(3).SetCellValue(dataItem.Account);
                                                               }
                                                           }
                                                       }
                                                       dataRow.CreateCell(4).SetCellValue(dataItem.CustId);
                                                       dataRow.CreateCell(5).SetCellValue(dataItem.CustName);
                                                       dataRow.CreateCell(6).SetCellValue(dataItem.PayAmount);
                                                       dataRow.CreateCell(7).SetCellValue(dataItem.sum1);
                                                       dataRow.CreateCell(8).SetCellValue(dataItem.GovNo);
                                                       dataRow.CreateCell(9).SetCellValue(dataItem.CaseKind2);
                                                       dataRow.CreateCell(10).SetCellValue(dataItem.AgentUser);
                                                       dataRow.CreateCell(11).SetCellValue(dataItem.CheckDate);
                                                   });
            }
            return ms;
        }
        public IList<CheckQueryAndPrint> GetPrintData(CheckQueryAndPrint model)
        {
            try
            {
                string strSql = @"SELECT row_number() over (order by M.CaseId asc ) num,
                            M.CaseNo,
                            CONVERT(varchar(100), M.CreatedDate, 111) as CreatedDate,
                            S.Account,
                            S.CustId,
                            S.CustName,
                            S.Currency,
                            s.PayAmount,
                            (select sum(PayAmount) from CaseSeizure where CaseId=S.CaseId) as sum1,
                            M.GovNo,
                            m.CaseKind2,
                            M.AgentUser,
                           ( select top 1 CASE WHEN ISNULL(TX_00450.[DATA1],'') = ''  THEN '' ELSE SUBSTRING(TX_00450.DATA1,3,10) END
                                       from TX_00450
                                       where M.CaseId = TX_00450.CaseId and SUBSTRING(TX_00450.DATA1,16,4) = '9095'
                                       order by TX_00450.TrnNum DESC
                                    ) as CheckDate
                            FROM CaseMaster AS M
                            LEFT OUTER JOIN CaseSeizure AS S ON M.CaseId = S.CaseId
                            
                                WHERE S.CaseId IS NOT NULL ";
                
                IList<CheckQueryAndPrint> ilst = base.SearchList<CheckQueryAndPrint>(strSql);
                if (ilst != null)
                {
                    if (ilst.Count > 0)
                    {
                        base.DataRecords = ilst[0].maxnum;
                    }
                    else
                    {
                        base.DataRecords = 0;
                        ilst = new List<CheckQueryAndPrint>();
                    }
                    return ilst;
                }
                else
                {
                    return new List<CheckQueryAndPrint>();
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public MemoryStream ExportExcelForType1(string CheckDate, string CheckNoStart, string CheckNoEnd, string AmtConsistentType)
        {
            string sqlWhere = "";
            string strSql = "";
            if (!string.IsNullOrEmpty(CheckDate))
            {
                sqlWhere += " AND CONVERT(varchar(100), M.[PayDate], 111)=@CheckDate";
                Parameter.Add(new CommandParameter("@CheckDate", CheckDate));
            }
            if (!string.IsNullOrEmpty(AmtConsistentType))
            {
                if (AmtConsistentType == "1")//支付金額與解扣金額相符
                {
                    sqlWhere += " and P.[Money]=CS.TripAmount";//P.SeizureAmountSUB IR-8004
                }
                else if (AmtConsistentType == "2")
                {
                    sqlWhere += " and P.[Money]<>CS.TripAmount";//P.SeizureAmountSUB IR-8004
                }
                //if (AmtConsistentType == "1")//支付金額與解扣金額相符
                //{
                //    sqlWhere += " and P.[Money]=P.SeizureAmountSUB";
                //}
                //else if (AmtConsistentType == "2")
                //{
                //    sqlWhere += " and P.[Money]<>P.SeizureAmountSUB";
                //}
            }
            if (!string.IsNullOrEmpty(CheckNoStart) && !string.IsNullOrEmpty(CheckNoEnd))
            {
                sqlWhere += " and P.CheckNo>=@CheckNoStart and P.CheckNo<=@CheckNoEnd";
                Parameter.Add(new CommandParameter("@CheckNoStart", CheckNoStart));
                Parameter.Add(new CommandParameter("@CheckNoEnd", CheckNoEnd));
            }
            strSql = @";with T1 
	                        as
	                        (
                                SELECT 
	                                ROW_NUMBER() OVER (ORDER BY P.CheckNo ASC ) num,
	                                P.PayeeId,
	                                P.CaseId,
	                                P.CheckNo,
	                                M.CaseNo,
	                                S.SendWord +'字'+ S.SendNo + '號' AS SendNo,
	                                CONVERT(varchar(100), M.PayDate, 111) as CheckDate,
	                                P.ReceivePerson,
	                                P.[Money] ,
	                                P.Fee,
	                                M.GovNo,
	                                M.CaseKind2,
	                                M.AgentUser ,
                                    M.Status--,
                                    --P.SeizureAmountSUB
	                                FROM CaseMaster AS M
                                INNER JOIN CasePayeeSetting AS P ON M.CaseId = P.CaseId
                                LEFT OUTER JOIN CaseSendSetting AS S ON P.CaseId = S.CaseId AND P.SendId = S.SerialID
                                WHERE 1=1 " + sqlWhere + @"
                            ),T2 as
	                        (
		                        select *, row_number() over (order by CheckNo asc , CaseNo asc)  RowNum
		                        from T1 
								),T3 as(
								select T2.*,(select sum(TripAmount) from CaseSeizure CS where T2.CaseId = CS.PayCaseId) as SeizureAmountSUB from T2 
	                        ),T4 as 
	                        (
		                        select *,(select max(RowNum) from T3) maxnum,(select sum([Money]) from T3) TotalPayment,
		                        (select sum(SeizureAmountSUB) from T3) TotalSeizureAmount,(select sum(Fee) from T3) TotalFee
		                        from T3 
	                        )
	                        select a.* from T4 a order by a.RowNum";
            IList<CheckQueryAndPrint> ilst = base.SearchList<CheckQueryAndPrint>(strSql);
            string[] headerColumns = new[]
                    {
                        "支票號碼",
                        "案件編號",
                        "發文字號",
                        "支付日期",
                        "受款人員",
                        "解扣金額",
                        "支付金額",
                        "手續費",
                        "來文字號",
                        "細分類",
                        "經辦人員",
                        "功能"
                    };
            var ms = new MemoryStream();

            if (ilst != null)
            {
                ms = ExcelExportEx("1",new HSSFWorkbook(),ilst, headerColumns,
                                               delegate (HSSFRow dataRow, CheckQueryAndPrint dataItem)
                                               {
                                                   //* 這裡可以針對每一個欄位做額外處理.比如日期
                                                   dataRow.CreateCell(0).SetCellValue(dataItem.CheckNo.ToString());
                                                   dataRow.CreateCell(1).SetCellValue(dataItem.CaseNo);
                                                   dataRow.CreateCell(2).SetCellValue(dataItem.SendNo);
                                                   dataRow.CreateCell(3).SetCellValue(dataItem.CheckDate.ToString());
                                                   dataRow.CreateCell(4).SetCellValue(dataItem.ReceivePerson);
                                                   dataRow.CreateCell(5).SetCellValue(dataItem.SeizureAmountSUB);
                                                   dataRow.CreateCell(6).SetCellValue(dataItem.Money);
                                                   dataRow.CreateCell(7).SetCellValue(dataItem.Fee);
                                                   dataRow.CreateCell(8).SetCellValue(dataItem.GovNo);
                                                   dataRow.CreateCell(9).SetCellValue(dataItem.CaseKind2);
                                                   dataRow.CreateCell(10).SetCellValue(dataItem.AgentUser);
                                                   dataRow.CreateCell(11).SetCellValue("");
                                               });
            }
            return ms;
        }
        public MemoryStream ExportExcelForType2(string CheckDate, string CheckNoStart, string CheckNoEnd, string AmtConsistentType)
        {
            string sqlWhere = "";
            string strSql = "";
            if (!string.IsNullOrEmpty(CheckDate))
            {
                sqlWhere += " AND CONVERT(varchar(100), M.[PayDate], 111)=@CheckDate";
                Parameter.Add(new CommandParameter("@CheckDate", CheckDate));
            }
            strSql = @";With T0 as
                            (select CaseId ,sum(Fee) as Fee,sum(SeizureAmountSUB)as SeizureAmountSUB   from CasePayeeSetting group by CaseId),
                            T1 
	                        as
	                        (
                             SELECT row_number() over (order by M.CaseId asc ) num,
                                M.CaseId,
                                M.CaseNo,
                                CONVERT(varchar(100), M.CreatedDate, 111) as CreatedDate,
                                S.Account,
                                S.CustId,
                                S.CustName,
                                s.PayAmount,
                                (select sum(PayAmount) from CaseSeizure where PayCaseId=S.PayCaseId AND CustName= s.CustName) as sum1,
                                M.GovNo,
                                m.CaseKind2,
                                M.AgentUser,
                                M.Status,
                                 CONVERT(varchar(100), M.PayDate, 111) as PayDate,
                                S.ProdCode,
                                S.Currency,
                                T0.Fee,
                                T0.SeizureAmountSUB
                                FROM CaseMaster AS M
                                left JOIN T0  ON M.CaseId = T0.CaseId
                                LEFT OUTER JOIN CaseSeizure AS S ON M.CaseId = S.PayCaseId
                                WHERE S.CaseId IS NOT NULL   " + sqlWhere + @" 
	                        ),T2 as
	                        (
		                        select *, row_number() over (order by CaseId asc ) RowNum
		                        from T1
	                        ),T3 as 
	                        (
		                        select *,(select max(RowNum) from T2) maxnum,(select count(distinct CustId) from T2)as TotalID,(select sum(PayAmount) from T2) TotalPayment,
		                        (select sum(SeizureAmountSUB) from T2) TotalSeizureAmount,(select sum(Fee) from T2) TotalFee  from T2 
	                        )
	                        select a.* from T3 a order by a.RowNum";
            IList<CheckQueryAndPrint> ilst = base.SearchList<CheckQueryAndPrint>(strSql);
            string[] headerColumns = new[]
                    {
                        "案件編號",
                        "建檔日期",
                        "帳號",
                        "產品別",
                        "幣別",
                        "義（債）務人統編",
                        "義（債）務人戶名",
                        "支付金額",
                        "手續費",
                        "來文字號",
                        "細分類",
                        "經辦人員",
                        "解扣日期"
                    };
            var ms = new MemoryStream();

            if (ilst != null)
            {
                ms = ExcelExportEx("2",new HSSFWorkbook(), ilst, headerColumns,
                                               delegate (HSSFRow dataRow, CheckQueryAndPrint dataItem)
                                               {
                                                   //* 這裡可以針對每一個欄位做額外處理.比如日期
                                                   dataRow.CreateCell(0).SetCellValue(dataItem.CaseNo);
                                                   dataRow.CreateCell(1).SetCellValue(dataItem.CreatedDate);
                                                   string strAccount = "";
                                                   int at = 0;
                                                   string strFlag = dataItem.Currency.ToUpper();
                                                   // strFlag == "0000",帳號爲strValue後12位,否則爲前幾位
                                                   if (dataItem.Account.Trim().Length > 15)
                                                   {
                                                       if (strFlag == "TWD")
                                                       {
                                                           // 截取帳號
                                                           if (dataItem.Account.Length >= 12)
                                                           {
                                                               strAccount = dataItem.Account.Substring(dataItem.Account.Length - 12, 12);
                                                           }
                                                           else
                                                           {
                                                               string strAccount2 = dataItem.Account.Substring(0, dataItem.Account.Length - 3);
                                                               strAccount=strAccount2.Substring(strAccount2.Length - 12, 12);
                                                           }
                                                       }
                                                       else
                                                       {
                                                           // 截取帳號
                                                          if (dataItem.ProdCode.IndexOf("外幣", 0, dataItem.ProdCode.Length) >= 0 && dataItem.ProdCode.IndexOf("外幣定期", 0, dataItem.ProdCode.Length)  <  0)
                                                           {
                                                               at = dataItem.Account.IndexOf("13", 0, dataItem.Account.Length);
                                                               if ((at > 2) && (at + 2 + 7 < dataItem.Account.Length))
                                                               {
                                                                   strAccount = dataItem.Account.Substring(at - 3, 3) + "13" + dataItem.Account.Substring(at + 2, 7);
                                                               } 
                                                               else
                                                               {
                                                                  if ( dataItem.Account.Substring(0,4)== "0000")
                                                                  {
                                                                     strAccount = dataItem.Account.Substring(dataItem.Account.Length - 12, 12);                                                                     
                                                                   }
                                                                  else
                                                                  {
                                                                     string strAccount3 = dataItem.Account.Substring(0, dataItem.Account.Length - 3);
                                                                     strAccount = strAccount3.Substring(strAccount3.Length - 12, 12);                                             
                                                                  }
                                                               }
                                                           }
                                                           else
                                                           {
                                                              if (dataItem.ProdCode.IndexOf("外幣定期", 0, dataItem.ProdCode.Length) >= 0)
                                                               {
                                                                   at = dataItem.Account.IndexOf("006", 0, dataItem.Account.Length);
                                                                   if (dataItem.Account.Length > 12)
                                                                   {
                                                                       strAccount =  "006" + dataItem.Account.Substring(at+3, 9);
                                                                   }
                                                                   else
                                                                   {
                                                                       strAccount = dataItem.Account.Substring(dataItem.Account.Length - 12, 12);
                                                                   }
                                                               }
                                                               else
                                                               {
                                                                   if (dataItem.ProdCode.IndexOf("MyWay外", 0, dataItem.ProdCode.Length) >= 0)
                                                                   {
                                                                       at = dataItem.Account.IndexOf("90120", 0, dataItem.Account.Length);
                                                                       if (at + 12 <= dataItem.Account.Length)
                                                                       {
                                                                           strAccount = "90120" + dataItem.Account.Substring(at + 5, 7);
                                                                       }
                                                                       else
                                                                       {
                                                                           strAccount = dataItem.Account.Substring(dataItem.Account.Length - 12, 12);
                                                                       }
                                                                   }
                                                                   else
                                                                   {
                                                                       strAccount = dataItem.Account.Substring(dataItem.Account.Length - 12, 12);
                                                                   }
                                                               }
                                                           }
                                                       }
                                                   }
                                                   else
                                                   {
                                                       if (dataItem.Account.Trim().Length > 12 && dataItem.Account.Trim().Length != 14)
                                                       {
                                                           strAccount = dataItem.Account.Substring(dataItem.Account.Length - 12, 12);
                                                       }
                                                       else
                                                       {
                                                           if (dataItem.Account.Trim().Length == 14)
                                                           {
                                                               strAccount=dataItem.Account.Substring(dataItem.Account.Length - 12, 12);
                                                           }
                                                           else
                                                           {
                                                               strAccount = dataItem.Account;
                                                           }
                                                       }

                                                       // 截取帳號
                                                       //if (dataItem.Account.Length > 3)
                                                       //{
                                                       //    strAccount = dataItem.Account.Substring(0, dataItem.Account.Length - 3);
                                                       //}
                                                       //if (strAccount.Length >= 12)
                                                       //{
                                                       //    strAccount = strAccount.Substring(strAccount.Length - 12, 12);
                                                       //}
                                                       //else
                                                       //{
                                                       //    strAccount = strAccount.PadLeft(12, '0');
                                                       //}
                                                   }
                                                   dataRow.CreateCell(2).SetCellValue(strAccount);
                                                   dataRow.CreateCell(3).SetCellValue(dataItem.ProdCode);
                                                   dataRow.CreateCell(4).SetCellValue(dataItem.Currency);
                                                   dataRow.CreateCell(5).SetCellValue(dataItem.CustId);
                                                   dataRow.CreateCell(6).SetCellValue(dataItem.CustName);
                                                   dataRow.CreateCell(7).SetCellValue(dataItem.PayAmount);
                                                   dataRow.CreateCell(8).SetCellValue(dataItem.Fee);
                                                   dataRow.CreateCell(9).SetCellValue(dataItem.GovNo);
                                                   dataRow.CreateCell(10).SetCellValue(dataItem.CaseKind2);
                                                   dataRow.CreateCell(11).SetCellValue(dataItem.AgentUser);
                                                   dataRow.CreateCell(12).SetCellValue(dataItem.PayDate);
                                               });
            }
            return ms;
        }
        public  MemoryStream ExcelExportEx<TType>(string Type,HSSFWorkbook workbook, IList<TType> list, string[] headerColumns, Action<HSSFRow, TType> setExcelRow)
        {
            var ms = new MemoryStream();

            #region set column style
            var headerStyle = workbook.CreateCellStyle() as HSSFCellStyle;
            var fonts = workbook.CreateFont() as HSSFFont;
            fonts.Boldweight = (short)NPOI.SS.UserModel.FontBoldWeight.Bold;
            headerStyle.SetFont(fonts);
            #endregion

            int iPage = 0;
            int iTotal = 0;
            int pageSize = 65534;

            if (!list.Any())
            {
                var sheet = workbook.CreateSheet() as HSSFSheet;
                var dataRowTotal = sheet.CreateRow(0) as HSSFRow;
                if(Type=="1")
                {
                    dataRowTotal.CreateCell(0).SetCellValue("解扣總金額：");
                    dataRowTotal.CreateCell(1).SetCellValue("");
                    dataRowTotal.CreateCell(2).SetCellValue("支付總金額：");
                    dataRowTotal.CreateCell(3).SetCellValue("");
                    dataRowTotal.CreateCell(4).SetCellValue("手續費總金額：");
                    dataRowTotal.CreateCell(5).SetCellValue("");
                }
                if (Type == "2")
                {
                    dataRowTotal.CreateCell(0).SetCellValue("ID總筆數：");
                    dataRowTotal.CreateCell(1).SetCellValue("");
                    dataRowTotal.CreateCell(2).SetCellValue("解扣總金額：");
                    dataRowTotal.CreateCell(3).SetCellValue("");
                    dataRowTotal.CreateCell(4).SetCellValue("支付總金額：");
                    dataRowTotal.CreateCell(5).SetCellValue("");
                    dataRowTotal.CreateCell(6).SetCellValue("手續費總金額：");
                    dataRowTotal.CreateCell(7).SetCellValue("");
                }
                var dataRow = sheet.CreateRow(1) as HSSFRow;
                //set header
                for (var j = 0; j < headerColumns.Length; j++)
                {
                    dataRow.CreateCell(j).SetCellValue(headerColumns[j]);
                    dataRow.GetCell(j).CellStyle = headerStyle;
                }
            }
            else
            {
                while (iTotal < list.Count)
                {
                    iTotal = iPage * pageSize;
                    int iCurrentCount = 0;
                    if (list.Count - iTotal > pageSize)
                    {
                        iCurrentCount = pageSize;
                    }
                    else
                    {
                        iCurrentCount = list.Count - iTotal;

                    }
                    CheckQueryAndPrint itemTotal =list[0] as CheckQueryAndPrint;
                    var sheet = workbook.CreateSheet() as HSSFSheet;
                    var dataRowTotal = sheet.CreateRow(0) as HSSFRow;
                    if (Type == "1")
                    {
                        dataRowTotal.CreateCell(0).SetCellValue("解扣總金額：");
                        dataRowTotal.CreateCell(1).SetCellValue(itemTotal.TotalSeizureAmount);
                        dataRowTotal.CreateCell(2).SetCellValue("支付總金額：");
                        dataRowTotal.CreateCell(3).SetCellValue(itemTotal.TotalPayment);
                        dataRowTotal.CreateCell(4).SetCellValue("手續費總金額：");
                        dataRowTotal.CreateCell(5).SetCellValue(itemTotal.TotalFee);
                    }
                    if (Type == "2")
                    {
                        dataRowTotal.CreateCell(0).SetCellValue("ID總筆數：");
                        dataRowTotal.CreateCell(1).SetCellValue(itemTotal.TotalID);
                        dataRowTotal.CreateCell(2).SetCellValue("解扣總金額：");
                        dataRowTotal.CreateCell(3).SetCellValue(itemTotal.TotalSeizureAmount);
                        dataRowTotal.CreateCell(4).SetCellValue("支付總金額：");
                        dataRowTotal.CreateCell(5).SetCellValue(itemTotal.TotalPayment);
                        dataRowTotal.CreateCell(6).SetCellValue("手續費總金額：");
                        dataRowTotal.CreateCell(7).SetCellValue(itemTotal.TotalFee);
                    }
                    var dataRow = sheet.CreateRow(1) as HSSFRow;
                    //set header
                    for (var j = 0; j < headerColumns.Length; j++)
                    {
                        dataRow.CreateCell(j).SetCellValue(headerColumns[j]);
                        dataRow.GetCell(j).CellStyle = headerStyle;
                    }

                    for (var i = iTotal; i < iTotal + iCurrentCount; i++)
                    {
                        //*set row data
                        dataRow = (HSSFRow)sheet.CreateRow(i - iTotal + 2);
                        setExcelRow(dataRow, list[i]);

                    }
                    sheet = null;
                    iTotal += iCurrentCount;
                    iPage++;
                }
            }

            workbook.Write(ms);
            ms.Flush();
            ms.Position = 0;
            workbook = null;
            return ms;
        }
        /// <summary>
        /// 根據提供的日期返回該日期之後的一個營業日（如果該日期為非營業日）
        /// 如果該日期就是營業日則返回自己
        /// </summary>
        /// <param name="strDate"></param>
        /// <returns></returns>
        public string GetWorkingDays(string strDate)
        {
            //string strsql = @"select * from PARMWorkingDay where Flag=1 and [Date]>=@Date order by [Date] asc";
            string strsql = @"select  CONVERT(varchar(10), [Date], 111) as [Date] from PARMWorkingDay where Flag=1 and [Date]>=@Date order by [Date] asc";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("@Date" , strDate));
            DataTable dt = Search(strsql);
            return dt != null && dt.Rows.Count > 0 ? dt.Rows[0]["Date"].ToString() : "";
        }

    }
}
