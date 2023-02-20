using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using CTBC.CSFS.Pattern;
using CTBC.CSFS.Models;
using CTBC.FrameWork.Util;

namespace CTBC.CSFS.BussinessLogic
{
    public class ImportEDocBiz : CommonBIZ
    {
		public int CreateLog(List<CaseObligor> model, CaseMaster cm, IDbTransaction trans = null)
		{

			int rtn = 0;
			if (!model.Any())
				return 0;

			//*更新舊狀態為已重查,不然永遠都是失敗.0->3,1->4,2->5
			string strSql = "UPDATE [BatchQueue] SET [Status] = [Status] + 3 WHERE [CaseId] = @CaseId AND [Status] < 3";
			Parameter.Clear();
			Parameter.Add(new CommandParameter("@CaseId", cm.CaseId));
			if (trans == null)
				ExecuteNonQuery(strSql);
			else
				ExecuteNonQuery(strSql, trans);

			foreach (CaseObligor obligor in model.Where(obligor => !string.IsNullOrEmpty(obligor.ObligorName) || !string.IsNullOrEmpty(obligor.ObligorNo) ||
																   !string.IsNullOrEmpty(obligor.ObligorAccount)))
			{
				//* 0-待查詢,1-成功,2-失敗,3-已重查
				strSql = @" insert into BatchQueue(CaseId,SendUser,DocNo,ObligorNo,ServiceName,Status,CreateDatetime)
                            values(@CaseId,@CreatedUser,@DocNo,@ObligorNo,'60491','0',GetDate());
                            insert into BatchQueue(CaseId,SendUser,DocNo,ObligorNo,ServiceName,Status,CreateDatetime)
                            values(@CaseId,@CreatedUser,@DocNo,@ObligorNo,'67072','0',GetDate());";
				Parameter.Clear();
				Parameter.Add(new CommandParameter("@DocNo", cm.DocNo));
				Parameter.Add(new CommandParameter("@CaseId", obligor.CaseId));
				Parameter.Add(new CommandParameter("@ObligorNo", obligor.ObligorNo));
				Parameter.Add(new CommandParameter("@CreatedUser", obligor.CreatedUser));
				rtn = rtn + (trans == null ? ExecuteNonQuery(strSql) : ExecuteNonQuery(strSql, trans));
			}
			return rtn;
		}
        public bool GetImportTime(string executedDate, string timeSection)
        {
            try
            {
                string sql = @"select count(*) from Record_ImportEDoc where ExecutedDate=@executedDate and TimeSection=@timeSection";
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@executedDate", executedDate));
                base.Parameter.Add(new CommandParameter("@timeSection", timeSection));
                return (int)base.ExecuteScalar(sql) > 0;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public bool ValiReCase(string govUnit, string govNo)
        {
            try
            {
                string sql = @"select count(*) from CaseMaster where GovUnit=@GovUnit and GovNo=@GovNo";
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@GovUnit", govUnit));
                base.Parameter.Add(new CommandParameter("@GovNo", govNo));
                return (int)base.ExecuteScalar(sql) > 0;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public int InsertEDocTXT1(EDocTXT1 eDocTXT1)
        {
            try
            {
                int rtn = 0;
                string sql = @"insert into EDocTXT1 values(@CaseId,@GovUnit,@GovUnitCode,@GovDate,@ReceiverNo,@ObligorName,@Agent,@RegistrationNo,@ObligorNo,@Amount,@Fee,@Total,@Memo,@Unit,@Contact,@Telephone)";
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@CaseId", eDocTXT1.CaseId));
                base.Parameter.Add(new CommandParameter("@GovUnit", eDocTXT1.GovUnit));
                base.Parameter.Add(new CommandParameter("@GovUnitCode", eDocTXT1.GovUnitCode));
                base.Parameter.Add(new CommandParameter("@GovDate", eDocTXT1.GovDate));
                base.Parameter.Add(new CommandParameter("@ReceiverNo", eDocTXT1.ReceiverNo));
                base.Parameter.Add(new CommandParameter("@ObligorName", eDocTXT1.ObligorName));
                base.Parameter.Add(new CommandParameter("@Agent", eDocTXT1.Agent));
                base.Parameter.Add(new CommandParameter("@RegistrationNo", eDocTXT1.RegistrationNo));
                base.Parameter.Add(new CommandParameter("@ObligorNo", eDocTXT1.ObligorNo));
                base.Parameter.Add(new CommandParameter("@Amount", eDocTXT1.Amount));
                base.Parameter.Add(new CommandParameter("@Fee", eDocTXT1.Fee));
                base.Parameter.Add(new CommandParameter("@Total", eDocTXT1.Total));
                base.Parameter.Add(new CommandParameter("@Memo", eDocTXT1.Memo));
                base.Parameter.Add(new CommandParameter("@Unit", eDocTXT1.Unit));
                base.Parameter.Add(new CommandParameter("@Contact", eDocTXT1.Contact));
                base.Parameter.Add(new CommandParameter("@Telephone", eDocTXT1.Telephone));
                rtn = base.ExecuteNonQuery(sql);
                return rtn;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public int InsertEDocTXT1_Detail(List<EDocTXT1_Detail> list)
        {
            try
            {
                int rtn = 0;
                string sql = @"insert into EDocTXT1_Detail values(@CaseId,@ExecCaseNo,@TransferUnitID,@TransferCaseNo,@ManageID)";
                foreach(var eDocTXT1_Detail in list)
                {
                    base.Parameter.Clear();
                    base.Parameter.Add(new CommandParameter("@CaseId", eDocTXT1_Detail.CaseId));
                    base.Parameter.Add(new CommandParameter("@ExecCaseNo", eDocTXT1_Detail.ExecCaseNo));
                    base.Parameter.Add(new CommandParameter("@TransferUnitID", eDocTXT1_Detail.TransferUnitID));
                    base.Parameter.Add(new CommandParameter("@TransferCaseNo", eDocTXT1_Detail.TransferCaseNo));
                    base.Parameter.Add(new CommandParameter("@ManageID", eDocTXT1_Detail.ManageID));
                    rtn += base.ExecuteNonQuery(sql);
                }
                return rtn;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public int InsertEDocTXT2(EDocTXT2 eDocTXT2)
        {
            try
            {
                int rtn = 0;
                string sql = @"insert into EDocTXT2 values(@CaseId,@GovUnit,@GovUnitCode,@GovDate,@ReceiverNo,@ObligorName,@Agent,@RegistrationNo,@ObligorNo,@Amount,@GovDate2,@ReceiverNo2,@Memo,@Unit,@Contact,@Telephone)";
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@CaseId", eDocTXT2.CaseId));
                base.Parameter.Add(new CommandParameter("@GovUnit", eDocTXT2.GovUnit));
                base.Parameter.Add(new CommandParameter("@GovUnitCode", eDocTXT2.GovUnitCode));
                base.Parameter.Add(new CommandParameter("@GovDate", eDocTXT2.GovDate));
                base.Parameter.Add(new CommandParameter("@ReceiverNo", eDocTXT2.ReceiverNo));
                base.Parameter.Add(new CommandParameter("@ObligorName", eDocTXT2.ObligorName));
                base.Parameter.Add(new CommandParameter("@Agent", eDocTXT2.Agent));
                base.Parameter.Add(new CommandParameter("@RegistrationNo", eDocTXT2.RegistrationNo));
                base.Parameter.Add(new CommandParameter("@ObligorNo", eDocTXT2.ObligorNo));
                base.Parameter.Add(new CommandParameter("@Amount", eDocTXT2.Amount));
                base.Parameter.Add(new CommandParameter("@GovDate2", eDocTXT2.GovDate2));
                base.Parameter.Add(new CommandParameter("@ReceiverNo2", eDocTXT2.ReceiverNo2));
                base.Parameter.Add(new CommandParameter("@Memo", eDocTXT2.Memo));
                base.Parameter.Add(new CommandParameter("@Unit", eDocTXT2.Unit));
                base.Parameter.Add(new CommandParameter("@Contact", eDocTXT2.Contact));
                base.Parameter.Add(new CommandParameter("@Telephone", eDocTXT2.Telephone));
                rtn = base.ExecuteNonQuery(sql);
                return rtn;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public int InsertEDocTXT2_Detail(List<EDocTXT2_Detail> list)
        {
            try
            {
                int rtn = 0;
                string sql = @"insert into EDocTXT2_Detail values(@CaseId,@ExecCaseNo,@TransferUnitID,@TransferCaseNo,@ManageID)";
                foreach (var eDocTXT2_Detail in list)
                {
                    base.Parameter.Clear();
                    base.Parameter.Add(new CommandParameter("@CaseId", eDocTXT2_Detail.CaseId));
                    base.Parameter.Add(new CommandParameter("@ExecCaseNo", eDocTXT2_Detail.ExecCaseNo));
                    base.Parameter.Add(new CommandParameter("@TransferUnitID", eDocTXT2_Detail.TransferUnitID));
                    base.Parameter.Add(new CommandParameter("@TransferCaseNo", eDocTXT2_Detail.TransferCaseNo));
                    base.Parameter.Add(new CommandParameter("@ManageID", eDocTXT2_Detail.ManageID));
                    rtn += base.ExecuteNonQuery(sql);
                }
                return rtn;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public int InsertCaseMast(CaseMaster caseMast)
        {
            try
            {
                int rtn = 0;
                string sql = @"insert into CaseMaster(CaseId,GovUnit,GovDate,Speed,ReceiveKind,GovNo,LimitDate,ReceiveDate,CaseKind,CaseKind2,Unit,Person,DocNo,Status,isDelete,CaseNo,CreatedDate) values(@CaseId,@GovUnit,@GovDate,@Speed,@ReceiveKind,@GovNo,@LimitDate,@ReceiveDate,@CaseKind,@CaseKind2,@Unit,@Person,@DocNo,@Status,@isDelete,@CaseNo,@CreatedDate)";
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@CaseId", caseMast.CaseId));
                base.Parameter.Add(new CommandParameter("@GovUnit", caseMast.GovUnit));
                base.Parameter.Add(new CommandParameter("@GovDate", caseMast.GovDate));
                base.Parameter.Add(new CommandParameter("@Speed", caseMast.Speed));
                base.Parameter.Add(new CommandParameter("@ReceiveKind", caseMast.ReceiveKind));
                base.Parameter.Add(new CommandParameter("@GovNo", caseMast.GovNo));
                base.Parameter.Add(new CommandParameter("@LimitDate", caseMast.LimitDate));
                base.Parameter.Add(new CommandParameter("@ReceiveDate", caseMast.ReceiveDate));
                base.Parameter.Add(new CommandParameter("@CaseKind", caseMast.CaseKind));
                base.Parameter.Add(new CommandParameter("@CaseKind2", caseMast.CaseKind2));
                base.Parameter.Add(new CommandParameter("@Unit", caseMast.Unit));
                base.Parameter.Add(new CommandParameter("@Person", caseMast.Person));
                base.Parameter.Add(new CommandParameter("@DocNo", caseMast.DocNo));
                base.Parameter.Add(new CommandParameter("@Status", caseMast.Status));
                base.Parameter.Add(new CommandParameter("@isDelete", caseMast.isDelete));
                base.Parameter.Add(new CommandParameter("@CaseNo", caseMast.CaseNo));
                base.Parameter.Add(new CommandParameter("@CreatedDate", caseMast.CreatedDate));
                rtn = base.ExecuteNonQuery(sql);
                return rtn;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public int InsertCaseObligor(CaseObligor caseObligor)
        {
            try
            {
                int rtn = 0;
                string sql = @"insert into CaseObligor(CaseId,ObligorName,ObligorNo) values(@CaseId,@ObligorName,@ObligorNo)";
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@CaseId", caseObligor.CaseId));
                base.Parameter.Add(new CommandParameter("@ObligorName", caseObligor.ObligorName));
                base.Parameter.Add(new CommandParameter("@ObligorNo", caseObligor.ObligorNo));
                rtn = base.ExecuteNonQuery(sql);
                return rtn;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public int ImportEdocData(ImportEdocData importEdocData)
        {
            try
            {
                int rtn = 0;
                string sql = @"insert into ImportEdocData(ExecutedDate,Timesection,DocNo,CaseNo,GovUnit,GovNo,CreatedDate,Added,GovDate) values(getdate(),@Timesection,@DocNo,@CaseNo,@GovUnit,@GovNo,getdate(),@Added,@GovDate)";
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@Timesection", importEdocData.Timesection));
                base.Parameter.Add(new CommandParameter("@DocNo", importEdocData.DocNo));
                base.Parameter.Add(new CommandParameter("@CaseNo", importEdocData.CaseNo));
                base.Parameter.Add(new CommandParameter("@GovUnit", importEdocData.GovUnit));
                base.Parameter.Add(new CommandParameter("@GovNo", importEdocData.GovNo));
                base.Parameter.Add(new CommandParameter("@Added", importEdocData.Added));
                base.Parameter.Add(new CommandParameter("@GovDate", importEdocData.GovDate));
                rtn = base.ExecuteNonQuery(sql);
                return rtn;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public int InsertCaseEdocFile(CaseEdocFile caseEdocFile)
        {
            try
            {
                int rtn = 0;
                string selectSql = @"select count(*) from CaseEdocFile where CaseId = @CaseId and Type = @Type and FileType = @FileType and SendNo = @SendNo";
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@CaseId", caseEdocFile.CaseId));
                base.Parameter.Add(new CommandParameter("@Type", caseEdocFile.Type));
                base.Parameter.Add(new CommandParameter("@FileType", caseEdocFile.FileType));
                base.Parameter.Add(new CommandParameter("@SendNo", caseEdocFile.SendNo));
                int result = (int)base.ExecuteScalar(selectSql);
                if(result <= 0)
                {
                    string sql = @"insert into CaseEdocFile values(@CaseId,@Type,@FileType,@FileName,@FileObject,@SendNo)";
                    base.Parameter.Clear();
                    base.Parameter.Add(new CommandParameter("@CaseId", caseEdocFile.CaseId));
                    base.Parameter.Add(new CommandParameter("@Type", caseEdocFile.Type));
                    base.Parameter.Add(new CommandParameter("@FileType", caseEdocFile.FileType));
                    base.Parameter.Add(new CommandParameter("@FileName", caseEdocFile.FileName));
                    base.Parameter.Add(new CommandParameter("@SendNo", caseEdocFile.SendNo));
                    base.Parameter.Add(new CommandParameter("@FileObject", caseEdocFile.FileObject, SqlDbType.VarBinary, 0));
                    rtn = base.ExecuteNonQuery(sql);
                }
                return rtn;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public int InsertRecord_ImportEDoc(string timesection)
        {
            try
            {
                int rtn = 0;
                string sql = @"insert into Record_ImportEDoc values(getdate(),@TimeSection)";
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@TimeSection", timesection));
                rtn = base.ExecuteNonQuery(sql);
                return rtn;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IList<ImportEdocData> GetImportEdocData(ImportEdocData model, int pageIndex, string strSortExpression, string strSortDirection, string EmpId)
        {
            try
            {
                string sqlWhere = "";
                PageIndex = pageIndex;
                Parameter.Clear();
                Parameter.Add(new CommandParameter("@pageS", (PageSize * (PageIndex - 1)) + 1));
                Parameter.Add(new CommandParameter("@pageE", PageSize * PageIndex));
                if (!string.IsNullOrEmpty(model.Added))
                {
                    sqlWhere += @" and Added like @Added ";
                    Parameter.Add(new CommandParameter("@Added", "%" + model.Added.Trim() + "%"));
                }
                if (!string.IsNullOrEmpty(model.GovUnit))
                {
                    sqlWhere += @" and GovUnit like @GovUnit ";
                    Parameter.Add(new CommandParameter("@GovUnit", "%" + model.GovUnit.Trim() + "%"));
                }
                if (!string.IsNullOrEmpty(model.GovNo))
                {
                    sqlWhere += @" and GovNo like @GovNo ";
                    Parameter.Add(new CommandParameter("@GovNo", "%" + model.GovNo.Trim() + "%"));
                }
                if (!string.IsNullOrEmpty(model.GovDateS))
                {
                    sqlWhere += @" AND GovDate >= @GovDateS";
                    Parameter.Add(new CommandParameter("@GovDateS", model.GovDateS));
                }
                if (!string.IsNullOrEmpty(model.GovDateE))
                {
                    string qDateE = Convert.ToDateTime(UtlString.FormatDateString(model.GovDateE)).AddDays(1).ToString("yyyyMMdd");
                    sqlWhere += @" AND GovDate < @GovDateE ";
                    Parameter.Add(new CommandParameter("@GovDateE", qDateE));
                }
                if (!string.IsNullOrEmpty(model.ExecutedDateS))
                {
                    sqlWhere += @" AND ExecutedDate >= @ExecutedDateS";
                    Parameter.Add(new CommandParameter("@ExecutedDateS", model.ExecutedDateS));
                }
                if (!string.IsNullOrEmpty(model.ExecutedDateE))
                {
                    string qDateE = Convert.ToDateTime(UtlString.FormatDateString(model.ExecutedDateE)).AddDays(1).ToString("yyyyMMdd");
                    sqlWhere += @" AND ExecutedDate < @ExecutedDateE ";
                    Parameter.Add(new CommandParameter("@ExecutedDateE", qDateE));
                }
                
                string strSql = @";with T1 
	                        as
	                        (
		                       select CONVERT(varchar(100),ExecutedDate, 111) as ExecutedDate,Timesection,DocNo,CaseNo,GovUnit,GovNo,Added,GovDate from ImportEdocData WHERE 1=1 " + sqlWhere + @" 
	                        ),T2 as
	                        (
		                        select *, row_number() over (order by ExecutedDate ASC , CaseNo ASC ) RowNum
		                        from T1
	                        ),T3 as 
	                        (
		                        select *,(select max(RowNum) from T2) maxnum from T2 
		                        where rownum between @pageS and @pageE
	                        )
	                        select a.* from T3 a order by a.RowNum";

                IList<ImportEdocData> ilst = SearchList<ImportEdocData>(strSql);
                if (ilst != null)
                {
                    if (ilst.Count > 0)
                    {
                        DataRecords = ilst[0].maxnum;
                    }
                    else
                    {
                        DataRecords = 0;
                        ilst = new List<ImportEdocData>();
                    }
                    return ilst;
                }
                return new List<ImportEdocData>();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public bool GetImportEdocDataByFileName(string filename)
        {
            try
            {
                
                string strSql = @"SELECT count(fileType)
  FROM [CaseEdocFile] where Caseid = (  SELECT caseid 
  FROM [CaseEdocFile] where filename = '"+ filename+"')";

                DataTable dtlist = Search(strSql);

                if (dtlist.Rows.Count > 0)
                {
                    if (dtlist.Rows[0][0].ToString() == "3")
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IList<SendTimeSection> GetSendTimeSectionList()
        {
            try
            {
                string strSql = @"SELECT * FROM [SendTimeSection] order by TimeSection desc";
                Parameter.Clear();
                return SearchList<SendTimeSection>(strSql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
