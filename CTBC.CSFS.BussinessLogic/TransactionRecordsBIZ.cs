using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CTBC.CSFS.Models;
using CTBC.CSFS.Pattern;
using CTBC.CSFS.ViewModels;

namespace CTBC.CSFS.BussinessLogic 
{
    public class TransactionRecordsBIZ : CommonBIZ
    {
        public IList<CaseDataLog> GetQueryDetail(CaseDataLog TransRecords)
        {
            base.Parameter.Clear();
            base.Parameter.Add(new CommandParameter("@CaseId", TransRecords.CaseId));
            string sqlStr = @"select (cast(TXSNO as varchar(100))+TabID+TableName+LinkDataKey) as idkey, ColumnName,ColumnValueBefore,ColumnValueAfter
                              from CaseDataLog left join PARMCode on CodeNo = ColumnID and CodeType = TableName
                              where CaseId=@CaseId and (TXType='修改' or TXType='新增')  and Enable='1' ";
            IList<CaseDataLog> lst = new List<CaseDataLog>();
            lst = base.SearchList<CaseDataLog>(sqlStr);
            if (lst == null || lst.Count == 0)
            {
                return new List<CaseDataLog>();
            }
            else
            {
                return lst;
            }
        }
        public IList<CaseDataLog> GetQueryList(CaseDataLog TransRecords, int pageIndex)
        {
            try
            {
                base.PageIndex = pageIndex;
                string sqlStr = "";
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@pageS", (base.PageSize * (base.PageIndex - 1)) + 1));
                base.Parameter.Add(new CommandParameter("@pageE", base.PageSize * base.PageIndex));
                base.Parameter.Add(new CommandParameter("@CaseId", TransRecords.CaseId));
                sqlStr = @";with T1 as(select distinct * from
		                    (select (cast(TXSNO as varchar(100))+TabID+TableName+LinkDataKey) as idkey,CaseId,TabName,TXDateTime,TITLE,TXType,TxUser,TXSNO,TabID,LinkDataKey from CaseDataLog 
                            left join PARMCode on CodeNo = ColumnID and CodeType = TableName
                            where CaseId = @CaseId and TableDispActive='1' and TabName='公文資訊-案件資訊' and TableName='CaseMaster' 
                            and ColumnID in ('GovKind','GovDate','GovUnit','GovNo','Speed','ReceiveKind','CaseKind2','Receiver','ReceiveAmount','NotSeizureAmount','PreSubAmount','PreReceiveAmount','OverCancel') and Enable='1'
							union all
							select (cast(TXSNO as varchar(100))+TabID+TableName+LinkDataKey) as idkey,CaseId,TabName,TXDateTime,TITLE,TXType,TxUser,TXSNO,TabID,LinkDataKey from CaseDataLog 
                            left join PARMCode on CodeNo = ColumnID and CodeType = TableName
                            where CaseId = @CaseId and TableDispActive='1' and TabName='公文資訊-義(債)務人資訊' and TableName='CaseObligor' 
                            and ColumnID in ('ObligorName','ObligorNo','ObligorAccount') and Enable='1'
                            union all
							select (cast(TXSNO as varchar(100))+TabID+TableName+LinkDataKey) as idkey,CaseId,TabName,TXDateTime,TITLE,TXType,TxUser,TXSNO,TabID,LinkDataKey from CaseDataLog 
                            left join PARMCode on CodeNo = ColumnID and CodeType = TableName
                            where CaseId = @CaseId and TableDispActive='1' and TabName='撤銷設定' and TableName='CaseSeizure' 
                            and ColumnID in ('CancelAmount') and Enable='1'
                            union all
							select (cast(TXSNO as varchar(100))+TabID+TableName+LinkDataKey) as idkey,CaseId,TabName,TXDateTime,TITLE,TXType,TxUser,TXSNO,TabID,LinkDataKey from CaseDataLog 
                            left join PARMCode on CodeNo = ColumnID and CodeType = TableName
                            where CaseId = @CaseId and TableDispActive='1' and TabName='支付設定' and TableName='CaseSeizure' 
                            and ColumnID in ('PayAmount','TripAmount','AccountStatus') and Enable='1'
							union all
							select (cast(TXSNO as varchar(100))+TabID+TableName+LinkDataKey) as idkey,CaseId,TabName,TXDateTime,TITLE,TXType,TxUser,TXSNO,TabID,LinkDataKey from CaseDataLog 
                            left join PARMCode on CodeNo = ColumnID and CodeType = TableName
                            where CaseId = @CaseId and TableDispActive='1' and TabName='扣押設定' and TableName='CaseSeizure' and Enable='1'
							and ColumnID in ('CustId','CustName','Account','Currency','SeizureAmount','SeizureAmountNtd') ) a),T2 as
                            (select *, row_number() over (order by T1.TXDateTime asc) RowNum from T1),T3 as
                            (select *,(select max(RowNum) from T2) maxnum from T2
                            where RowNum between @pageS and @pageE)
                            select a.* from T3 a ";
                IList<CaseDataLog> lst = new List<CaseDataLog>();
                lst = base.SearchList<CaseDataLog>(sqlStr);
                if (lst == null || lst.Count == 0)
                {
                    base.DataRecords = 0;
                    return new List<CaseDataLog>();
                }
                else
                {
                    base.DataRecords = lst[0].maxnum;
                    return lst;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IList<CaseDataLog> GetDetailList(CaseDataLog TransRecords)
        {
            base.Parameter.Clear();
            base.Parameter.Add(new CommandParameter("@CaseId", TransRecords.CaseId));
            base.Parameter.Add(new CommandParameter("@idkey", TransRecords.idkey));
            string sqlStr = @"select (cast(TXSNO as varchar(100))+TabID+TableName+LinkDataKey) as idkey,CaseId,TabName,TXDateTime,TITLE,TXType,TxUser,ColumnName,ColumnValueBefore,ColumnValueAfter
                             from CaseDataLog left join PARMCode on CodeNo = ColumnID and CodeType = TableName
                             where CaseId=@CaseId and (cast(TXSNO as varchar(100))+TabID+TableName+LinkDataKey)=@idkey and Enable='1' order by DispSrNo";
            IList<CaseDataLog> lst = new List<CaseDataLog>();
            lst = base.SearchList<CaseDataLog>(sqlStr);
            if (lst == null || lst.Count == 0)
            {
                return new List<CaseDataLog>();
            }
            else
            {
                return lst;
            }
        }


    }
}
