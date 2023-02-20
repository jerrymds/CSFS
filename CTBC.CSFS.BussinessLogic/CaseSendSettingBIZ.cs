using CTBC.CSFS.Models;
using CTBC.CSFS.Pattern;
using CTBC.FrameWork.Util;
using System;
using System.IO;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CTBC.CSFS.Resource;
using CTBC.CSFS.ViewModels;
using NPOI.HSSF.UserModel;
using NPOI.XSSF.UserModel;

namespace CTBC.CSFS.BussinessLogic
{
    public class CaseSendSettingBIZ : CommonBIZ
    {
        public CaseSendSettingBIZ(AppController appController)
            : base(appController)
        { }

        public CaseSendSettingBIZ()
        { }

        /// <summary>
        /// 通過CaseId查詢該CaseId下所有的發文設定信息
        /// </summary>
        /// <param name="caseId"></param>
        /// <returns></returns>
        public IList<CaseSendSettingQueryResultViewModel> GetSendSettingList(Guid caseId)
        {
            string strSql = @"SELECT [SerialID]
                                    ,S.[CaseId]
                                    ,[Template]
                                    ,[SendWord]
                                    ,[SendNo]
                                    ,[SendDate]
                                    ,S.[Speed]
                                    ,[Security]
                                    ,[Subject]
                                    ,[Description]
                                    ,[isFinish]
                                    ,[FinishDate]
                                    ,[Attachment]
                                    ,S.[CreatedUser]
                                    ,S.[CreatedDate]
                                    ,S.[ModifiedUser]
                                    ,S.[ModifiedDate]
                                    ,S.[SendKind]
	                                ,M.[CaseNo]
	                                ,M.[ApproveDate]
                                    ,M.[Status]
                                FROM [CaseSendSetting] AS S
                                LEFT OUTER JOIN [CaseMaster] AS M ON S.CaseId = M.CaseId
                                WHERE S.[CaseId] = @CaseId";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("caseId", caseId));
            return SearchList<CaseSendSettingQueryResultViewModel>(strSql);
        }

        public DataTable ImportXLSX(string pathSource, string strCaseTrsNewID, string filename)
        {
            //存入理債CaseEdocFile
            string UploadFile = pathSource;
            //using (FileStream FileStream = System.IO.File.OpenRead(UploadFile))
            //{
            //    byte[] Bytes = new byte[FileStream.Length];
            //    FileStream.Position = 0;
            //    FileStream.Read(Bytes, 0, Bytes.Length);
            //    CaseAccountBiz caseAccount = new CaseAccountBiz();
            //    Guid CaseId = Guid.Parse(strCaseTrsNewID);
            //    CaseEdocFile EdocFile = caseAccount.OpenDeadXlsx3(CaseId);
            //    //
            //    if (EdocFile != null)
            //    {
            //        DeleteXlsxCaseEdocFile(CaseId);
            //    }
            //    CaseEdocFile caseEdocFile = new CaseEdocFile();
            //    caseEdocFile.CaseId = CaseId;
            //    caseEdocFile.Type = "死亡";
            //    caseEdocFile.FileType = "xlsx3";
            //    caseEdocFile.FileName = Path.GetFileName(pathSource);
            //    caseEdocFile.FileObject = Bytes;
            //    caseEdocFile.SendNo = DocNo;
            //    InsertCaseEdocFile(caseEdocFile);

            //};


            DataTable dt = new DataTable();
            var fs = new FileStream(pathSource, FileMode.Open, FileAccess.ReadWrite);
            //FileStream fs = new FileStream(pathSource, FileMode.Open, FileAccess.ReadWrite);

            XSSFWorkbook templateWorkbook = new XSSFWorkbook(fs);
            string wh = templateWorkbook.GetSheetName(0);
            XSSFSheet sheet = (XSSFSheet)templateWorkbook.GetSheet(wh);


            //dataRow.GetCell(1).SetCellValue("foo");

            MemoryStream ms = new MemoryStream();
            //templateWorkbook.Write(ms);

            //HSSFWorkbook workbook = null;
            //HSSFSheet sheet = null;

            try
            {
                #region 讀Excel檔，逐行寫入DataTable

                //sheet = (HSSFSheet)workbook.GetSheetAt(0);   //0表示：第一個 worksheet工作表


                XSSFRow headerRow = (XSSFRow)sheet.GetRow(0);   //Excel 表頭列

                //金融遺產查詢單位	縣市代號	總分局處所代號	申請日期	流水號	被繼承人身分證字號	被繼承人姓名	被繼承人出生日期	被繼承人死亡日期	申請人身分證字號	申請人姓名	申請人電話號碼	申請人與被繼承人關係	代理人身分證字號	代理人姓名	代理人電話號碼	送達地址縣市名稱	送達地址鄉鎮市區名稱	送達地址村里名稱	送達地址鄰	送達地址街道門牌	合併Q-U，轉半型	存款	放款	投資型理財	戶名不符	保險箱	信用卡	會辦投資型理財
                for (int colIdx = 0; colIdx <= 28; colIdx++)
                {
                    if (headerRow.GetCell(colIdx) != null)
                        dt.Columns.Add(new DataColumn("Columns" + colIdx.ToString()));
                    //欄位名有折行時，只取第一行的名稱做法是headerRow.GetCell(colIdx).StringCellValue.Replace("\n", ",").Split(',')[0]
                }

                //For迴圈的「啟始值」為1，表示不包含 Excel表頭列
                for (int rowIdx = 1; rowIdx <= sheet.LastRowNum; rowIdx++)   //每一列做迴圈
                {
                    try
                    {
                        XSSFRow exlRow = (XSSFRow)sheet.GetRow(rowIdx); //不包含 Excel表頭列的 "其他資料列"
                        DataRow newDataRow = dt.NewRow();
                        for (int colIdx = exlRow.FirstCellNum; colIdx <= 28; colIdx++)   //exlRow.LastCellNum 每一個欄位做迴圈
                        {
                            if ((colIdx == exlRow.FirstCellNum) && (exlRow.GetCell(colIdx).ToString().Trim().Length == 0))
                            {
                                break;
                            }
                            if (exlRow.GetCell(colIdx) != null)
                            {
                                // checkdata(colIdx, exlRow.GetCell(colIdx).ToString().Trim());
                                newDataRow[colIdx] = exlRow.GetCell(colIdx).ToString().Trim();
                            }
                            else
                            {
                                newDataRow[colIdx] = "";
                                //err = err + colIdx.ToString() + "-" + exlRow.GetCell(colIdx).ToString() + ";";
                                //newDataRow[colIdx] = null;
                            }

                            //每一個欄位，都加入同一列 DataRow
                        }
                        dt.Rows.Add(newDataRow);
                    }
                    catch (Exception e)
                    {
                        throw e;
                    }

                }

                using (var file = new FileStream(pathSource, FileMode.Open, FileAccess.ReadWrite))
                {
                    templateWorkbook.Write(file);
                }
                templateWorkbook.Write(ms);

                #endregion 讀Excel檔，逐行寫入DataTable
            }
            catch (Exception e)
            {
                throw e;

            }
            finally
            {
                //釋放 NPOI的資源
                templateWorkbook = null;
                sheet = null;
                fs.Close();
                fs.Dispose();
                File.Delete(pathSource);
            }
            return dt;
        }

        public CaseSendSetting GetSendSetting(int serialId)
        {
            string strSql = @"SELECT [SerialID]
                                    ,S.[CaseId]
                                    ,[Template]
                                    ,[SendWord]
                                    ,[SendNo]
                                    ,[SendDate]
                                    ,S.[Speed]
                                    ,[Security]
                                    ,[Subject]
                                    ,[Description]
                                    ,[isFinish]
                                    ,[FinishDate]
                                    ,[Attachment]
                                    ,S.[CreatedUser]
                                    ,S.[CreatedDate]
                                    ,S.[ModifiedUser]
                                    ,S.[ModifiedDate]
                                    ,S.[SendKind]
                                FROM [CaseSendSetting] AS S
                                WHERE S.[SerialID] = @SerialID";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("SerialID", serialId));
            var rtnList = SearchList<CaseSendSetting>(strSql);
            return rtnList.FirstOrDefault();
        }

        /// <summary>
        /// 通過CaseId取得該CaseId下所有發文明細
        /// </summary>
        /// <param name="caseId"></param>
        /// <returns></returns>
        public IList<CaseSendSettingDetails> GetSendSettingDetails(Guid caseId)
        {
            string strSql = @"SELECT  [DetailsId]
                                ,[CaseId]
                                ,[SerialID]
                                ,[SendType]
                                ,[GovName]
                                ,[GovAddr]
                            FROM [CaseSendSettingDetails]
                            WHERE [CaseId] = @CaseId";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("caseId", caseId));
            return SearchList<CaseSendSettingDetails>(strSql);
        }



        /// <summary>
        /// 同上功能, 提供給AutoPay用 
        /// </summary>
        /// <param name="model"></param>
        /// <param name="trans"></param>
        /// <returns></returns>
        public bool SaveCreate2(CaseSendSettingCreateViewModel model, IDbTransaction trans = null)
        {
            //simon 2016/09/29
            if (model.SendKind != "電子發文")
                model.SendKind = "紙本發文";

            IDbConnection dbConnection = OpenConnection();
            bool rtn = true;
            bool needSubmit = false;
            try
            {
                if (trans == null)
                {
                    needSubmit = true;
                    trans = dbConnection.BeginTransaction();
                }
                rtn = rtn & InsertCaseSendSetting2(ref model, trans);
                if (model.ReceiveList != null && model.ReceiveList.Any())
                {
                    foreach (CaseSendSettingDetails item in model.ReceiveList.Where(item => !string.IsNullOrEmpty(item.GovAddr) && !string.IsNullOrEmpty(item.GovAddr)))
                    {
                        item.CaseId = model.CaseId;
                        item.SerialID = model.SerialId;
                        item.SendType = CaseSettingDetailType.Receive;  //*正本
                        rtn = rtn && InsertCaseSendSettingDetials(item, trans);
                    }
                }

                if (model.CcList != null && model.CcList.Any())
                {
                    foreach (CaseSendSettingDetails item in model.CcList.Where(item => !string.IsNullOrEmpty(item.GovAddr) && !string.IsNullOrEmpty(item.GovAddr)))
                    {
                        item.CaseId = model.CaseId;
                        item.SerialID = model.SerialId;
                        item.SendType = CaseSettingDetailType.Cc; //*副本
                        rtn = rtn && InsertCaseSendSettingDetials(item, trans);
                    }
                }

                if (needSubmit)
                {
                    if (rtn)
                        trans.Commit();
                    else
                        trans.Rollback();
                }

                return rtn;
            }
            catch (Exception)
            {
                try
                {
                    if (trans != null) trans.Rollback();
                }
                catch (Exception)
                {
                    // ignored
                }
                return false;
            }
        }

        public IList<CaseSendSettingDetails> GetSendSettingDetails(int serialId)
        {
            string strSql = @"SELECT  [DetailsId]
                                ,[CaseId]
                                ,[SerialID]
                                ,[SendType]
                                ,[GovName]
                                ,[GovAddr]
                            FROM [CaseSendSettingDetails]
                            WHERE [SerialID] = @SerialID";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("SerialID", serialId));
            return SearchList<CaseSendSettingDetails>(strSql);
        }
        public IList<CaseSendSettingDetails> GetSendSettingDetails(List<string> DetailIdList)
        {
            string strSql = @"SELECT  [DetailsId]
                                ,[CaseId]
                                ,[SerialID]
                                ,[SendType]
                                ,[GovName]
                                ,[GovAddr]
                            FROM [CaseSendSettingDetails]
                            WHERE 1=2 ";
            Parameter.Clear();
            for (int i = 0; i < DetailIdList.Count; i++)
            {
                strSql = strSql + " OR DetailsId = @DetailsId" + i + " ";
                Parameter.Add(new CommandParameter("@DetailsId" + i, DetailIdList[i].Split('|')[0]));
            }
            strSql = strSql + "order by GovName,GovAddr ";
            return SearchList<CaseSendSettingDetails>(strSql);
        }

        public IList<CaseSendSettingDetails> GetSendSettingDetailsByOrder(List<string> DetailIdList)
        {
            string strSql = @"select 0 as DetailsId,1 as No into #Map from CaseSendSettingDetails   WITH (NOLOCK) ";
            for (int i = 0; i < DetailIdList.Count; i++)
            {
                strSql = strSql + "insert #Map ( DetailsId,No) select DetailsId," + i + " from CaseSendSettingDetails   WITH (NOLOCK) where  DetailsId =" + DetailIdList[i].Split('|')[0] + " ";
            }
            strSql = strSql + @"SELECT  m.no,c.DetailsId
                                ,[CaseId]
                                ,[SerialID]
                                ,[SendType]
                                ,[GovName]
                                ,[GovAddr]
                            FROM [CaseSendSettingDetails] c   WITH (NOLOCK) 
							join #Map m   WITH (NOLOCK) on c.DetailsId = m.DetailsId
                            WHERE 1=2 ";
            Parameter.Clear();
            for (int i = 0; i < DetailIdList.Count; i++)
            {
                strSql = strSql + " OR c.DetailsId = @DetailsId" + i + " ";
                Parameter.Add(new CommandParameter("@DetailsId" + i, DetailIdList[i].Split('|')[0]));
            }
            strSql = strSql + "order by m.No drop table #Map ";
            return SearchList<CaseSendSettingDetails>(strSql);
        }
        public bool SaveDeadCreate(CaseSendSettingCreateViewModel model, IDbTransaction trans = null)
        {
            //simon 2016/09/29
            if (model.SendKind != "電子發文")
                model.SendKind = "紙本發文";

            //IDbConnection dbConnection = OpenConnection();
            using (IDbConnection  dbConnection = OpenConnection())
            {
                bool rtn = true;
                bool needSubmit = false;
                try
                {
                    if (trans == null)
                    {
                        needSubmit = true;
                        trans = dbConnection.BeginTransaction();
                    }
                    rtn = rtn & InsertDeadCaseSendSetting(ref model, trans);
                    if (model.ReceiveList != null && model.ReceiveList.Any())
                    {
                        foreach (CaseSendSettingDetails item in model.ReceiveList.Where(item => !string.IsNullOrEmpty(item.GovAddr) && !string.IsNullOrEmpty(item.GovAddr)))
                        {
                            item.CaseId = model.CaseId;
                            item.SerialID = model.SerialId;
                            item.SendType = CaseSettingDetailType.Receive;  //*正本
                            rtn = rtn && InsertDeadCaseSendSettingDetials(item, trans);
                        }
                    }

                    if (model.CcList != null && model.CcList.Any())
                    {
                        foreach (CaseSendSettingDetails item in model.CcList.Where(item => !string.IsNullOrEmpty(item.GovAddr) && !string.IsNullOrEmpty(item.GovAddr)))
                        {
                            item.CaseId = model.CaseId;
                            item.SerialID = model.SerialId;
                            item.SendType = CaseSettingDetailType.Cc; //*副本
                            rtn = rtn && InsertDeadCaseSendSettingDetials(item, trans);
                        }
                    }

                    // adam 20210528

                    //* CTBC的地址.電話.傳真
                    string Address = "";
                    string Fax = "";
                    string Title = "";
                    string MasterName = "";
                    CaseMasterBIZ master = new CaseMasterBIZ();
                    PARMCode codeItem = master.GetCodeData("REPORT_SETTING", "Address").FirstOrDefault();
                    if (codeItem == null)
                    {
                        Address = "";
                    }
                    else
                    {
                        Address = codeItem.CodeDesc;
                    }
                    codeItem = master.GetCodeData("REPORT_SETTING", "Fax").FirstOrDefault();
                    if (codeItem == null)
                    {
                        Fax = "";
                    }
                    else
                    {
                        Fax = codeItem.CodeDesc;
                    }

                    codeItem = master.GetCodeData("REPORT_SETTING", "ButtomLine").FirstOrDefault();
                    if (codeItem == null)
                    {
                        Title = "";
                    }
                    else
                    {
                        Title = codeItem.CodeDesc;
                    }
                    codeItem = master.GetCodeData("REPORT_SETTING", "ButtomLine2").FirstOrDefault();
                    if (codeItem == null)
                    {
                        MasterName = "";
                    }
                    else
                    {
                        MasterName = codeItem.CodeDesc;
                    }
                    LdapEmployeeBiz empBiz = new LdapEmployeeBiz();
                    string user = master.getAgentUser(model.CaseId);
                    LDAPEmployee empNow = empBiz.GetLdapEmployeeByEmpId(user);
                    string tel = empNow != null && !string.IsNullOrEmpty(empNow.TelNo) ? empNow.TelNo : "";
                    tel = tel + (empNow != null && !string.IsNullOrEmpty(empNow.TelExt) ? " 分機 " + empNow.TelExt : "");

                    SendEDocBiz _SendEDocBiz = new SendEDocBiz();
                    _SendEDocBiz.InsertCaseMasterDoc(model.CaseId, user, Address, tel, Fax, Title, MasterName);

                    // adam 20210528 end
                    if (needSubmit)
                    {
                        if (rtn)
                            trans.Commit();
                        else
                            trans.Rollback();
                    }

                    return rtn;
                }
                catch (Exception)
                {
                    try
                    {
                        if (trans != null) trans.Rollback();
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                    return false;
                }
            }
        }
        /// <summary>
        /// 儲存CaseSetting
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool SaveCreate(CaseSendSettingCreateViewModel model, IDbTransaction trans = null)
        {
            //simon 2016/09/29

            if (model.SendKind != "電子發文")
                model.SendKind = "紙本發文";
   
            IDbConnection dbConnection = OpenConnection();
            bool rtn = true;
            bool needSubmit = false;
            try
            {
                if (trans == null)
                {
                    needSubmit = true;
                    trans = dbConnection.BeginTransaction();
                }
                rtn = rtn & InsertCaseSendSetting(ref model, trans);
                if (model.ReceiveList != null && model.ReceiveList.Any())
                {
                    foreach (CaseSendSettingDetails item in model.ReceiveList.Where(item => !string.IsNullOrEmpty(item.GovAddr) && !string.IsNullOrEmpty(item.GovAddr)))
                    {
                        item.CaseId = model.CaseId;
                        item.SerialID = model.SerialId;
                        item.SendType = CaseSettingDetailType.Receive;  //*正本
                        rtn = rtn && InsertCaseSendSettingDetials(item, trans);
                    }
                }

                if (model.CcList != null && model.CcList.Any())
                {
                    foreach (CaseSendSettingDetails item in model.CcList.Where(item => !string.IsNullOrEmpty(item.GovAddr) && !string.IsNullOrEmpty(item.GovAddr)))
                    {
                        item.CaseId = model.CaseId;
                        item.SerialID = model.SerialId;
                        item.SendType = CaseSettingDetailType.Cc; //*副本
                        rtn = rtn && InsertCaseSendSettingDetials(item, trans);
                    }
                }
                // adam 20210528

                //* CTBC的地址.電話.傳真
                string Address = "";
                string Fax = "";
                string Title = "";
                string MasterName = "";
                CaseMasterBIZ master = new CaseMasterBIZ();
                PARMCode codeItem = master.GetCodeData("REPORT_SETTING", "Address").FirstOrDefault();
                if (codeItem == null)
                {
                    Address = ""; 
                    }
                else
                {
                    Address = codeItem.CodeDesc;
                }
                codeItem = master.GetCodeData("REPORT_SETTING", "Fax").FirstOrDefault();
                if (codeItem == null)
                {
                    Fax = "";
                }
                else
                {
                    Fax = codeItem.CodeDesc;
                }

                codeItem = master.GetCodeData("REPORT_SETTING", "ButtomLine").FirstOrDefault();
                if (codeItem == null)
                {
                    Title = "";
                }
                else
                {
                    Title = codeItem.CodeDesc;
                }
                codeItem = master.GetCodeData("REPORT_SETTING", "ButtomLine2").FirstOrDefault();
                if (codeItem == null)
                {
                    MasterName = "";
                }
                else
                {
                    MasterName = codeItem.CodeDesc;
                }
                LdapEmployeeBiz empBiz = new LdapEmployeeBiz();
                string user = master.getAgentUser(model.CaseId);
                LDAPEmployee empNow = empBiz.GetLdapEmployeeByEmpId(user);
                string tel = empNow != null && !string.IsNullOrEmpty(empNow.TelNo) ? empNow.TelNo : "";
                tel = tel + (empNow != null && !string.IsNullOrEmpty(empNow.TelExt) ? " 分機 " + empNow.TelExt : "");

                SendEDocBiz _SendEDocBiz = new SendEDocBiz();
                _SendEDocBiz.InsertCaseMasterDoc(model.CaseId,user,Address,tel,Fax,Title,MasterName);

                // adam 20210528 end
                if (needSubmit)
                {
                    if (rtn)
                        trans.Commit();
                    else
                        trans.Rollback();
                }

                return rtn;
            }
            catch (Exception)
            {
                try
                {
                    if (trans != null) trans.Rollback();
                }
                catch (Exception)
                {
                    // ignored
                }
                return false;
            }
        }
        public bool InsertDeadCaseSendSetting(ref CaseSendSettingCreateViewModel model, IDbTransaction trans)
        {
            bool result = true;
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    CaseSendSettingBIZ cssBIZ = new CaseSendSettingBIZ();
                    string sql = "";
                    sql = @"DECLARE @SendNoId bigint;
                    DECLARE @flag as timestamp;
                    SELECT TOP 1 @SendNoId=[SendNoId],@flag=[TimesFlag] FROM [SendNoTable] WHERE [SendNoYear] = @SendNoYear AND [SendNoNow] < [SendNoEnd] ORDER BY SendNoId;
                    UPDATE [SendNoTable] SET [SendNoNow] = [SendNoNow]+1 WHERE [SendNoId] = @SendNoId and [TimesFlag]=@flag;
                    INSERT INTO CaseSendSetting(CaseId,[Template],SendDate,SendWord,SendNo,Speed,Security,Subject,Description, Attachment,CreatedUser,CreatedDate,ModifiedUser,SendKind) 
                           VALUES (@CaseId,@Template,@SendDate,@SendWord,@SendNo1,@Speed,@Security,@Subject,@Description,@Attachment,@CreatedUser,GETDATE(),@ModifiedUser,@SendKind);
                    SELECT @@identity";
                    Parameter.Clear();
                    base.Parameter.Add(new CommandParameter("@CaseId", model.CaseId));
                    base.Parameter.Add(new CommandParameter("@Template", model.Template));
                    base.Parameter.Add(new CommandParameter("@SendDate", model.SendDate));
                    base.Parameter.Add(new CommandParameter("@SendWord", model.SendWord));
                    //adam 又改回取號
                    //if (model.flag == "AgentAccountInfo")//帳務資訊儲存時不產生發文字號，呈核時才產生
                    //{ 
                    //    model.SendNo = ""; 
                    //}
                    //else//發文資訊儲存時需要產生發文字號
                    {
                        if (model.SendKind == "電子發文")
                        {
                            //第四碼固定為2 --simon 2016/08/05
                            //model.SendNo = model.SendDate.Year + "00" + model.SendNo.Substring(9);
                            model.SendNo = cssBIZ.SendNo();
                            //20170630 緊急 RQ-2015-019666-0?? 發文字號擴碼 宏祥 update start
                            //model.SendNo = model.SendDate.Year + "20" + model.SendNo.Substring(9);
                            model.SendNo = (DateTime.Now.Year - 1911) + "2" + model.SendNo.Substring(9);
                            //20170630 緊急 RQ-2015-019666-0?? 發文字號擴碼 宏祥 update end
                        }
                        else
                        {
                            model.SendNo = cssBIZ.SendNo();
                        }
                    }
                    base.Parameter.Add(new CommandParameter("@SendNo1", model.SendNo));
                    base.Parameter.Add(new CommandParameter("@Speed", model.Speed));
                    base.Parameter.Add(new CommandParameter("@Security", model.Security));
                    base.Parameter.Add(new CommandParameter("@Subject", model.Subject));
                    base.Parameter.Add(new CommandParameter("@Description", model.Description));
                    base.Parameter.Add(new CommandParameter("@Attachment", model.Attachment));
                    base.Parameter.Add(new CommandParameter("@CreatedUser", Account));
                    base.Parameter.Add(new CommandParameter("@ModifiedUser", Account));
                    base.Parameter.Add(new CommandParameter("@SendNoYear", DateTime.Now.ToString("yyyy")));
                    base.Parameter.Add(new CommandParameter("@GovName", model.GovName));
                    base.Parameter.Add(new CommandParameter("@GovAddr", model.GovAddr));
                    base.Parameter.Add(new CommandParameter("@GovNameCc", model.GovNameCc));
                    base.Parameter.Add(new CommandParameter("@GovAddrCc", model.GovAddrCc));
                    base.Parameter.Add(new CommandParameter("@SendKind", model.SendKind));

                    model.SerialId = trans == null ? Convert.ToInt32(ExecuteScalar(sql)) : Convert.ToInt32(ExecuteScalar(sql, trans));
                    result = true;
                    break;
                }
                catch (Exception x)
                {
                    i++;
                    result = false;
                    trans.Rollback();
                }
            }
            return result;
        }

        public bool InsertCaseSendSetting(ref CaseSendSettingCreateViewModel model, IDbTransaction trans)
        {
            bool result = true;
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    CaseSendSettingBIZ cssBIZ = new CaseSendSettingBIZ();
                    string sql = "";
                    //sql = @"DECLARE @SendNo bigint;
                    //        DECLARE @SendNoId bigint;
                    //        SELECT TOP 1 @SendNo = [SendNoNow] + 1,@SendNoId=[SendNoId] FROM [SendNoTable] WHERE [SendNoYear] = @SendNoYear AND [SendNoNow] < [SendNoEnd] ORDER BY SendNoId;
                    //        UPDATE [SendNoTable] SET [SendNoNow] = @SendNo WHERE [SendNoId] = @SendNoId;
                    //        INSERT INTO CaseSendSetting(CaseId,[Template],SendDate,SendWord,SendNo,Speed,Security,Subject,Description, Attachment,CreatedUser,CreatedDate,ModifiedUser,SendKind) 
                    //               VALUES (@CaseId,@Template,@SendDate,@SendWord,@SendNo1,@Speed,@Security,@Subject,@Description,@Attachment,@CreatedUser,GETDATE(),@ModifiedUser,@SendKind);
                    //        SELECT @@identity";
                    sql = @"DECLARE @SendNoId bigint;
                    DECLARE @flag as timestamp;
                    SELECT TOP 1 @SendNoId=[SendNoId],@flag=[TimesFlag] FROM [SendNoTable] WHERE [SendNoYear] = @SendNoYear AND [SendNoNow] < [SendNoEnd] ORDER BY SendNoId;
                    UPDATE [SendNoTable] SET [SendNoNow] = [SendNoNow]+1 WHERE [SendNoId] = @SendNoId and [TimesFlag]=@flag;
                    INSERT INTO CaseSendSetting(CaseId,[Template],SendDate,SendWord,SendNo,Speed,Security,Subject,Description, Attachment,CreatedUser,CreatedDate,ModifiedUser,SendKind) 
                           VALUES (@CaseId,@Template,@SendDate,@SendWord,@SendNo1,@Speed,@Security,@Subject,@Description,@Attachment,@CreatedUser,GETDATE(),@ModifiedUser,@SendKind);
                    SELECT @@identity";
                    Parameter.Clear();
                    base.Parameter.Add(new CommandParameter("@CaseId", model.CaseId));
                    base.Parameter.Add(new CommandParameter("@Template", model.Template));
                    base.Parameter.Add(new CommandParameter("@SendDate", model.SendDate));
                    base.Parameter.Add(new CommandParameter("@SendWord", model.SendWord));
                    //adam 又改回取號
                    //if (model.flag == "AgentAccountInfo")//帳務資訊儲存時不產生發文字號，呈核時才產生
                    //{ 
                    //    model.SendNo = ""; 
                    //}
                    //else//發文資訊儲存時需要產生發文字號
                    {
                        if (model.SendKind == "電子發文")
                        {
                            //第四碼固定為2 --simon 2016/08/05
                            //model.SendNo = model.SendDate.Year + "00" + model.SendNo.Substring(9);
                            model.SendNo = cssBIZ.SendNo();
                            //20170630 緊急 RQ-2015-019666-0?? 發文字號擴碼 宏祥 update start
                            //model.SendNo = model.SendDate.Year + "20" + model.SendNo.Substring(9);
                            model.SendNo = (DateTime.Now.Year - 1911) + "2" + model.SendNo.Substring(9);
                            //20170630 緊急 RQ-2015-019666-0?? 發文字號擴碼 宏祥 update end
                        }
                        else
                        {
                            model.SendNo = cssBIZ.SendNo();
                        }
                    }
                    base.Parameter.Add(new CommandParameter("@SendNo1", model.SendNo));
                    base.Parameter.Add(new CommandParameter("@Speed", model.Speed));
                    base.Parameter.Add(new CommandParameter("@Security", model.Security));
                    base.Parameter.Add(new CommandParameter("@Subject", model.Subject));
                    base.Parameter.Add(new CommandParameter("@Description", model.Description));
                    base.Parameter.Add(new CommandParameter("@Attachment", model.Attachment));
                    base.Parameter.Add(new CommandParameter("@CreatedUser", Account));
                    base.Parameter.Add(new CommandParameter("@ModifiedUser", Account));
                    base.Parameter.Add(new CommandParameter("@SendNoYear", DateTime.Now.ToString("yyyy")));
                    base.Parameter.Add(new CommandParameter("@GovName", model.GovName));
                    base.Parameter.Add(new CommandParameter("@GovAddr", model.GovAddr));
                    base.Parameter.Add(new CommandParameter("@GovNameCc", model.GovNameCc));
                    base.Parameter.Add(new CommandParameter("@GovAddrCc", model.GovAddrCc));
                    base.Parameter.Add(new CommandParameter("@SendKind", model.SendKind));
                    
                    model.SerialId = trans == null ? Convert.ToInt32(ExecuteScalar(sql)) : Convert.ToInt32(ExecuteScalar(sql, trans));
                    result = true;
                    break;
                }
                catch (Exception)
                {
                    i++;
                    result = false;
                    trans.Rollback();
                }
            }
            return result;
        }

        /// <summary>
        /// 同上一個, 差別在由AutoPay.exe來呼叫
        /// </summary>
        /// <param name="model"></param>
        /// <param name="trans"></param>
        /// <returns></returns>
        public bool InsertCaseSendSetting2(ref CaseSendSettingCreateViewModel model, IDbTransaction trans)
        {
            bool result = true;
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    CaseSendSettingBIZ cssBIZ = new CaseSendSettingBIZ();
                    string sql = "";
                    //sql = @"DECLARE @SendNo bigint;
                    //        DECLARE @SendNoId bigint;
                    //        SELECT TOP 1 @SendNo = [SendNoNow] + 1,@SendNoId=[SendNoId] FROM [SendNoTable] WHERE [SendNoYear] = @SendNoYear AND [SendNoNow] < [SendNoEnd] ORDER BY SendNoId;
                    //        UPDATE [SendNoTable] SET [SendNoNow] = @SendNo WHERE [SendNoId] = @SendNoId;
                    //        INSERT INTO CaseSendSetting(CaseId,[Template],SendDate,SendWord,SendNo,Speed,Security,Subject,Description, Attachment,CreatedUser,CreatedDate,ModifiedUser,SendKind) 
                    //               VALUES (@CaseId,@Template,@SendDate,@SendWord,@SendNo1,@Speed,@Security,@Subject,@Description,@Attachment,@CreatedUser,GETDATE(),@ModifiedUser,@SendKind);
                    //        SELECT @@identity";
                    sql = @"DECLARE @SendNoId bigint;
                    DECLARE @flag as timestamp;
                    SELECT TOP 1 @SendNoId=[SendNoId],@flag=[TimesFlag] FROM [SendNoTable] WHERE [SendNoYear] = @SendNoYear AND [SendNoNow] < [SendNoEnd] ORDER BY SendNoId;
                    UPDATE [SendNoTable] SET [SendNoNow] = [SendNoNow]+1 WHERE [SendNoId] = @SendNoId and [TimesFlag]=@flag;
                    INSERT INTO CaseSendSetting(CaseId,[Template],SendDate,SendWord,SendNo,Speed,Security,Subject,Description, Attachment,CreatedUser,CreatedDate,ModifiedUser,SendKind) 
                           VALUES (@CaseId,@Template,@SendDate,@SendWord,@SendNo1,@Speed,@Security,@Subject,@Description,@Attachment,@CreatedUser,GETDATE(),@ModifiedUser,@SendKind);
                    SELECT @@identity";
                    Parameter.Clear();
                    base.Parameter.Add(new CommandParameter("@CaseId", model.CaseId));
                    base.Parameter.Add(new CommandParameter("@Template", model.Template));
                    base.Parameter.Add(new CommandParameter("@SendDate", model.SendDate));
                    base.Parameter.Add(new CommandParameter("@SendWord", model.SendWord));
                    //adam 又改回取號
                    //if (model.flag == "AgentAccountInfo")//帳務資訊儲存時不產生發文字號，呈核時才產生
                    //{ 
                    //    model.SendNo = ""; 
                    //}
                    //else//發文資訊儲存時需要產生發文字號
                    {
                        if (model.SendKind == "電子發文")
                        {
                            //第四碼固定為2 --simon 2016/08/05
                            //model.SendNo = model.SendDate.Year + "00" + model.SendNo.Substring(9);
                            model.SendNo = cssBIZ.SendNo();
                            //20170630 緊急 RQ-2015-019666-0?? 發文字號擴碼 宏祥 update start
                            //model.SendNo = model.SendDate.Year + "20" + model.SendNo.Substring(9);
                            model.SendNo = (DateTime.Now.Year - 1911) + "2" + model.SendNo.Substring(9);
                            //20170630 緊急 RQ-2015-019666-0?? 發文字號擴碼 宏祥 update end
                        }
                        else
                        {
                            model.SendNo = cssBIZ.SendNo();
                        }
                    }
                    base.Parameter.Add(new CommandParameter("@SendNo1", model.SendNo));
                    base.Parameter.Add(new CommandParameter("@Speed", model.Speed));
                    base.Parameter.Add(new CommandParameter("@Security", model.Security));
                    base.Parameter.Add(new CommandParameter("@Subject", model.Subject));
                    base.Parameter.Add(new CommandParameter("@Description", model.Description));
                    base.Parameter.Add(new CommandParameter("@Attachment", model.Attachment));
                    base.Parameter.Add(new CommandParameter("@CreatedUser", model.CreatedUser));
                    base.Parameter.Add(new CommandParameter("@ModifiedUser", model.ModifiedUser));
                    base.Parameter.Add(new CommandParameter("@SendNoYear", DateTime.Now.ToString("yyyy")));
                    base.Parameter.Add(new CommandParameter("@GovName", model.GovName));
                    base.Parameter.Add(new CommandParameter("@GovAddr", model.GovAddr));
                    base.Parameter.Add(new CommandParameter("@GovNameCc", model.GovNameCc));
                    base.Parameter.Add(new CommandParameter("@GovAddrCc", model.GovAddrCc));
                    base.Parameter.Add(new CommandParameter("@SendKind", model.SendKind));

                    model.SerialId = trans == null ? Convert.ToInt32(ExecuteScalar(sql)) : Convert.ToInt32(ExecuteScalar(sql, trans));
                    result = true;
                    break;
                }
                catch (Exception x)
                {
                    i++;
                    result = false;
                    trans.Rollback();
                }
            }
            return result;
        }

        public bool InsertDeadCaseSendSettingDetials(CaseSendSettingDetails model, IDbTransaction trans)
        {
            string strSql = @"insert into CaseSendSettingDetails([CaseId],[SerialID],[SendType],[GovName],[GovAddr])
                                    values(@CaseId, @SerialID, @SendType, @GovName,@GovAddr)";
            Parameter.Clear(); ;
            base.Parameter.Add(new CommandParameter("@CaseId", model.CaseId));
            base.Parameter.Add(new CommandParameter("@SerialID", model.SerialID));
            base.Parameter.Add(new CommandParameter("@SendType", model.SendType));
            base.Parameter.Add(new CommandParameter("@GovName", model.GovName));
            base.Parameter.Add(new CommandParameter("@GovAddr", model.GovAddr));
            return trans == null ? ExecuteNonQuery(strSql) > 0 : ExecuteNonQuery(strSql, trans) > 0;
        }

        public bool InsertCaseSendSettingDetials(CaseSendSettingDetails model, IDbTransaction trans)
        {
            string strSql = @"insert into CaseSendSettingDetails([CaseId],[SerialID],[SendType],[GovName],[GovAddr])
                                    values(@CaseId, @SerialID, @SendType, @GovName,@GovAddr)";
            Parameter.Clear(); ;
            base.Parameter.Add(new CommandParameter("@CaseId", model.CaseId));
            base.Parameter.Add(new CommandParameter("@SerialID", model.SerialID));
            base.Parameter.Add(new CommandParameter("@SendType", model.SendType));
            base.Parameter.Add(new CommandParameter("@GovName", model.GovName));
            base.Parameter.Add(new CommandParameter("@GovAddr", model.GovAddr));
            return trans == null ? ExecuteNonQuery(strSql) > 0 : ExecuteNonQuery(strSql, trans) > 0;
        }
        public bool SaveEdit(CaseSendSettingCreateViewModel model)
        {
            IDbConnection dbConnection = OpenConnection();
            IDbTransaction dbTransaction = null;
            bool rtn = true;
            try
            {
                using (dbConnection)
                {
                    dbTransaction = dbConnection.BeginTransaction();

                    rtn = rtn & UpdateCaseSendSetting(model, dbTransaction);
                    DeleteCaseSendSettingDetails(model.SerialId, dbTransaction);
                    if (model.ReceiveList != null && model.ReceiveList.Any())
                    {
                        foreach (CaseSendSettingDetails item in model.ReceiveList.Where(item => !string.IsNullOrEmpty(item.GovAddr) && !string.IsNullOrEmpty(item.GovAddr)))
                        {
                            item.CaseId = model.CaseId;
                            item.SerialID = model.SerialId;
                            item.SendType = CaseSettingDetailType.Receive;  //*正本
                            rtn = rtn && InsertCaseSendSettingDetials(item, dbTransaction);
                        }
                    }

                    if (model.CcList != null && model.CcList.Any())
                    {
                        foreach (CaseSendSettingDetails item in model.CcList.Where(item => !string.IsNullOrEmpty(item.GovAddr) && !string.IsNullOrEmpty(item.GovAddr)))
                        {
                            item.CaseId = model.CaseId;
                            item.SerialID = model.SerialId;
                            item.SendType = CaseSettingDetailType.Cc; //*副本
                            rtn = rtn && InsertCaseSendSettingDetials(item, dbTransaction);
                        }
                    }
                    // adam 20210528

                    //* CTBC的地址.電話.傳真
                    string Address = "";
                    string Fax = "";
                    string Title = "";
                    string MasterName = "";
                    CaseMasterBIZ master = new CaseMasterBIZ();
                    PARMCode codeItem = master.GetCodeData("REPORT_SETTING", "Address").FirstOrDefault();
                    if (codeItem == null)
                    {
                        Address = "";
                    }
                    else
                    {
                        Address = codeItem.CodeDesc;
                    }
                    codeItem = master.GetCodeData("REPORT_SETTING", "Fax").FirstOrDefault();
                    if (codeItem == null)
                    {
                        Fax = "";
                    }
                    else
                    {
                        Fax = codeItem.CodeDesc;
                    }

                    codeItem = master.GetCodeData("REPORT_SETTING", "ButtomLine").FirstOrDefault();
                    if (codeItem == null)
                    {
                        Title = "";
                    }
                    else
                    {
                        Title = codeItem.CodeDesc;
                    }
                    codeItem = master.GetCodeData("REPORT_SETTING", "ButtomLine2").FirstOrDefault();
                    if (codeItem == null)
                    {
                        MasterName = "";
                    }
                    else
                    {
                        MasterName = codeItem.CodeDesc;
                    }
                    LdapEmployeeBiz empBiz = new LdapEmployeeBiz();
                    string user = master.getAgentUser(model.CaseId);
                    LDAPEmployee empNow = empBiz.GetLdapEmployeeByEmpId(user);
                    string tel = empNow != null && !string.IsNullOrEmpty(empNow.TelNo) ? empNow.TelNo : "";
                    tel = tel + (empNow != null && !string.IsNullOrEmpty(empNow.TelExt) ? " 分機 " + empNow.TelExt : "");

                    SendEDocBiz _SendEDocBiz = new SendEDocBiz();
                    _SendEDocBiz.InsertCaseMasterDoc(model.CaseId, user, Address, tel, Fax, Title, MasterName);

                    // adam 20210528 end
                    if (rtn)
                        dbTransaction.Commit();
                    else
                        dbTransaction.Rollback();
                    return rtn;
                }
            }
            catch (Exception)
            {
                try
                {
                    if (dbTransaction != null) dbTransaction.Rollback();
                }
                catch (Exception)
                {
                    // ignored
                }
                return false;
            }
        }
        public bool UpdateCaseSendSetting(CaseSendSettingCreateViewModel model, IDbTransaction trans = null)
        {
            string sql = "";
            if (model.CaseKind2 == "支付")
            {
                sql = @"UPDATE [CaseSendSetting]
                           SET [Template] = @Template
                              ,[SendKind] = @SendKind
                              ,[SendDate] = @SendDate
                              ,[Speed] = @Speed
                              ,[Security] = @Security
                              ,[Subject] = @Subject
                              ,[Description] = @Description
                              ,[Attachment] = @Attachment
                              ,[ModifiedUser] = @ModifiedUser
                              ,[ModifiedDate] = GETDATE()
                              ,[SendWord] = @SendWord
                              ,[SendNo] = @SendNo
                              WHERE [SerialID] = @SerialID
	                          AND [CaseId] = @CaseId";
                Parameter.Clear();
                base.Parameter.Add(new CommandParameter("SerialID", model.SerialId));
                base.Parameter.Add(new CommandParameter("CaseId", model.CaseId));
                base.Parameter.Add(new CommandParameter("Template", model.Template));
                base.Parameter.Add(new CommandParameter("@SendKind", model.SendKind));
                base.Parameter.Add(new CommandParameter("@SendWord", model.SendWord));
                base.Parameter.Add(new CommandParameter("@SendNo", model.SendNo));
                base.Parameter.Add(new CommandParameter("@SendDate", model.SendDate));
                base.Parameter.Add(new CommandParameter("@Speed", model.Speed));
                base.Parameter.Add(new CommandParameter("@Security", model.Security));
                base.Parameter.Add(new CommandParameter("@Subject", model.Subject));
                base.Parameter.Add(new CommandParameter("@Description", model.Description));
                base.Parameter.Add(new CommandParameter("@Attachment", model.Attachment));
                base.Parameter.Add(new CommandParameter("@ModifiedUser", Account));               
            }
            else
            {
                sql = @"UPDATE [CaseSendSetting]
                           SET [Template] = @Template
                              ,[SendKind] = @SendKind
                              ,[SendDate] = @SendDate
                              ,[Speed] = @Speed
                              ,[Security] = @Security
                              ,[Subject] = @Subject
                              ,[Description] = @Description
                              ,[Attachment] = @Attachment
                              ,[ModifiedUser] = @ModifiedUser
                              ,[ModifiedDate] = GETDATE()
                              ,[SendNo] = @SendNo
                        WHERE [SerialID] = @SerialID
	                        AND [CaseId] = @CaseId";
                Parameter.Clear();
                base.Parameter.Add(new CommandParameter("SerialID", model.SerialId));
                base.Parameter.Add(new CommandParameter("CaseId", model.CaseId));
                base.Parameter.Add(new CommandParameter("Template", model.Template));
                base.Parameter.Add(new CommandParameter("@SendKind", model.SendKind));
                base.Parameter.Add(new CommandParameter("@SendWord", model.SendWord));
                base.Parameter.Add(new CommandParameter("@SendNo", model.SendNo));
                base.Parameter.Add(new CommandParameter("@SendDate", model.SendDate));
                base.Parameter.Add(new CommandParameter("@Speed", model.Speed));
                base.Parameter.Add(new CommandParameter("@Security", model.Security));
                base.Parameter.Add(new CommandParameter("@Subject", model.Subject));
                base.Parameter.Add(new CommandParameter("@Description", model.Description));
                base.Parameter.Add(new CommandParameter("@Attachment", model.Attachment));
                base.Parameter.Add(new CommandParameter("@ModifiedUser", Account));
            }
            return trans == null ? base.ExecuteNonQuery(sql) > 0 : base.ExecuteNonQuery(sql, trans) > 0;
        }
        public bool DeleteCaseSendSettingDetails(int serialId, IDbTransaction trans = null)
        {
            string sql = @"DELETE FROM [CaseSendSettingDetails] WHERE [SerialID] = @SerialID";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("SerialID", serialId));
            return trans == null ? ExecuteNonQuery(sql) > 0 : ExecuteNonQuery(sql, trans) > 0;
        }

        public bool DeleteCaseSendSetting(int serialId, IDbTransaction trans = null)
        {
            string sql = @"DELETE FROM CaseSendSetting WHERE [SerialID] = @SerialID";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("SerialID", serialId));
            return trans == null ? ExecuteNonQuery(sql) > 0 : ExecuteNonQuery(sql, trans) > 0;
        }

        public bool DeleteCaseSendSetting(Guid caseId, IDbTransaction dbtrans = null)
        {
            string strSql = "DELETE CaseSendSetting WHERE  CaseId = @CaseId ";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("CaseId", caseId));
            return dbtrans == null ? ExecuteNonQuery(strSql) > 0 : ExecuteNonQuery(strSql, dbtrans) > 0;
        }

        public bool DeleteCaseSendSettingDetails(Guid caseId, IDbTransaction dbtrans = null)
        {
            string strSql = "DELETE CaseSendSettingDetails WHERE  CaseId = @CaseId ";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("CaseId", caseId));
            return dbtrans == null ? ExecuteNonQuery(strSql) > 0 : ExecuteNonQuery(strSql, dbtrans) > 0;
        }

        public int Delete(int serialId,string userid)
        {
            //adam 20220811
            CSFSLog ApLog = new CSFSLog();
            CSFSLogBIZ _csfsLogBIZ = new CSFSLogBIZ();
            string strSendId = "";
            if (!String.IsNullOrEmpty(serialId.ToString()))
            {
                strSendId = serialId.ToString();
            }
            ApLog.Message = "發文刪除sendid:" + strSendId;
            _csfsLogBIZ.InsertCSFSLog("CasePayeeSetting", ApLog.Message,userid);
            //adam 20220811 end
            string strSql = @"UPDATE [CasePayeeSetting] SET [SendId] = NULL WHERE [SendId] = @SerialID;                            
                            DELETE FROM CaseSendSetting where SerialID=@SerialID";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("@SerialID", serialId));
            return ExecuteNonQuery(strSql) > 0 ? 1 : 0;
        }
        public int DeleteCheck(int serialId)
        {
            string strSql = @"Select * from [CasePayeeSetting]  WHERE [SendId] = @SerialID";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("@SerialID", serialId));
            DataTable dt = Search(strSql);
            if (dt.Rows.Count > 0)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }
        public CaseSendSettingCreateViewModel GetCaseSettingAndDetails(int serialId)
        {
            CaseSendSetting main = GetSendSetting(serialId);
            CaseSendSettingCreateViewModel rtn = new CaseSendSettingCreateViewModel
            {
                SerialId = main.SerialId,
                CaseId = main.CaseId,
                Template = main.Template,
                SendWord = main.SendWord,
                SendNo = main.SendNo,
                Speed = main.Speed,
                SendDate = main.SendDate,
                Security = "密(收到即解密)",//main.Security,
                Subject = main.Subject,
                Description = main.Description,
                Attachment = main.Attachment,
                CreatedUser = main.CreatedUser,
                CreatedDate = main.CreatedDate,
                ModifiedUser = main.ModifiedUser,
                ModifiedDate = main.ModifiedDate,
                SendKind = main.SendKind,
                ReceiveList = new List<CaseSendSettingDetails>(),
                CcList = new List<CaseSendSettingDetails>()
            };

            IList<CaseSendSettingDetails> list = GetSendSettingDetails(serialId);
            if (list != null && list.Any())
            {
                foreach (CaseSendSettingDetails details in list)
                {
                    if (details.SendType == CaseSettingDetailType.Receive)
                        rtn.ReceiveList.Add(details);
                    if (details.SendType == CaseSettingDetailType.Cc)
                        rtn.CcList.Add(details);
                }
            }
            return rtn;
        }

        public DataTable GetSendSettingByCaseIdListWithNoType(List<string> caseIdList, List<string> PayIdList, ref List<string> CaseIDSeq)
        {
            string strsql = @"SELECT M.[SerialID]
                                    , M.[CaseId]
                                    ,[Template]
                                    ,[SendWord]
                                    ,[SendNo]
                                    ,[SendDate]
                                    ,[Speed]
                                    ,[Security]
                                    ,[Subject]
                                    ,[Description]
                                    ,[isFinish]
                                    ,[FinishDate]
									,[Attachment]
                                    ,[GovName]
                                    ,[GovAddr]
                                    ,[SendType]
									,(select EmpName from [LDAPEmployee] emp where emp.EmpID=M.CreatedUser) as CreatedUser									
									,(select TelNo + ' 分機 '+TelExt from [LDAPEmployee] emp where emp.EmpID=M.CreatedUser) as TelNo 
                                FROM [CaseSendSetting] AS M with(Nolock)
                                LEFT OUTER JOIN [CaseSendSettingDetails] AS D ON M.CaseId = D.CaseId AND M.SerialID =D.SerialID                                
                                WHERE 1=2 ";
            Parameter.Clear();
            for (int i = 0; i < caseIdList.Count; i++)
            {
                strsql = strsql + " OR ( M.[CaseId] = @CaseId" + i + " " +  ") ";
                Parameter.Add(new CommandParameter("@CaseId" + i, caseIdList[i]));
            }
            DataTable Dt = Search(strsql);

            StringBuilder sbCaseids = new StringBuilder();
            foreach (var i in caseIdList)
            {
                sbCaseids.Append("'" + i + "',");
            }

            StringBuilder sbPayeeids = new StringBuilder();
            foreach (var i in PayIdList)
            {
                sbPayeeids.Append("'" + i + "',");
            }

            string strCaseids = string.Format("SELECT * FROM [dbo].[CasePayeeSetting] where caseid in ({0}) and PayeeId in ({1})", sbCaseids.ToString().TrimEnd(','), sbPayeeids.ToString().TrimEnd(','));

            // 由支付Payeeid中, 找到  支票號碼....
            var lstCheckNo = base.SearchList<CasePayeeSetting>(strCaseids).OrderBy(x => x.CheckNo).ToList();

            List<string> realCheckNo = new List<string>();
            foreach (var lst in lstCheckNo)
            {
                int icheckno = int.Parse(lst.CheckNo);
                realCheckNo.Add(string.Format("CT{0:0000000}", icheckno));
                CaseIDSeq.Add(lst.CaseId.ToString());
            }


            Dt.Columns.Add("Sort", typeof(int));
            int seq = 0;
            // 過濾Dt 中的Description 欄位...
            //foreach(DataRow dr in Dt.Rows)
            //{
            //    bool isExist = false;
            //    foreach(var c in realCheckNo)
            //    {
            //        if (dr["Description"].ToString().Contains(c))
            //        {
            //            isExist = true;
            //            dr["Sort"] = seq++.ToString();
            //        }
            //    }
            //    if (!isExist) // 不包括在裏面, 則刪除本筆...
            //        dr.Delete();                    
            //}

            foreach (var c in realCheckNo)
            {
                bool isExist = false;
                foreach (DataRow dr in Dt.Rows)
                {
                    if (dr["Description"].ToString().Contains(c))
                    {
                        isExist = true;
                        dr["Sort"] = seq++.ToString();
                    }
                }
            }

            foreach (DataRow dr in Dt.Rows)
            {
                if (string.IsNullOrEmpty(dr["Sort"].ToString()))
                    dr.Delete();
            }


            Dt.AcceptChanges();


            //20201012, 排序
            Dt.DefaultView.Sort = "Sort";
            DataTable newDT = Dt.DefaultView.ToTable();
            Dt = newDT;


            Dt.Columns.Add("Receive");
            Dt.Columns.Add("Cc");
            if (Dt != null && Dt.Rows.Count > 0)
            {
                string strSerialId = string.Empty;

                foreach (DataRow dr in Dt.Rows)
                {
                    strSerialId += "'" + dr["SerialID"].ToString() + "',";
                }

                strSerialId = strSerialId.TrimEnd(',');
                string sqlRecive = "SELECT GovName,SerialID FROM CaseSendSettingDetails WHERE SendType=1 and SerialID In (" + strSerialId + ")";
                string sqlCc = "SELECT GovName,SerialID FROM CaseSendSettingDetails WHERE SendType=2 and SerialID In (" + strSerialId + ")";
                List<CaseSendSettingDetails> listRecive = base.SearchList<CaseSendSettingDetails>(sqlRecive).ToList();
                List<CaseSendSettingDetails> listCc = base.SearchList<CaseSendSettingDetails>(sqlCc).ToList();
                if (listRecive != null && listRecive.Any())
                {
                    foreach (DataRow dr in Dt.Rows)
                    {
                        string strRecive = string.Empty;
                        foreach (CaseSendSettingDetails item in listRecive.Where(m => m.SerialID == Convert.ToInt32(dr["SerialID"])))
                        {
                            strRecive += item.GovName + "、";
                        }
                        strRecive = strRecive.TrimEnd('、');
                        dr["Receive"] = strRecive;
                    }
                }
                if (listCc != null && listCc.Any())
                {
                    foreach (DataRow dr in Dt.Rows)
                    {
                        string strCc = string.Empty;
                        foreach (CaseSendSettingDetails item in listCc.Where(m => m.SerialID == Convert.ToInt32(dr["SerialID"])))
                        {
                            strCc += item.GovName + "、";
                        }
                        strCc = strCc.TrimEnd('、');
                        dr["Cc"] = strCc;
                    }
                }
                return Dt;
            }
            else
            {
                return Dt = new DataTable();
            }
        }
        public DataTable GetSendSettingByCaseIdListWithType(string type, List<string> caseIdList, List<string> PayIdList,ref List<string> CaseIDSeq)
        {
            string strsql = @"SELECT M.[SerialID]
                                    , M.[CaseId]
                                    ,[Template]
                                    ,[SendWord]
                                    ,[SendNo]
                                    ,[SendDate]
                                    ,[Speed]
                                    ,[Security]
                                    ,[Subject]
                                    ,[Description]
                                    ,[isFinish]
                                    ,[FinishDate]
									,[Attachment]
                                    ,[GovName]
                                    ,[GovAddr]
                                    ,[SendType]
									,(select EmpName from [LDAPEmployee] emp where emp.EmpID=M.CreatedUser) as CreatedUser									
									,(select TelNo + ' 分機 '+TelExt from [LDAPEmployee] emp where emp.EmpID=M.CreatedUser) as TelNo 
                                FROM [CaseSendSetting] AS M with(Nolock)
                                LEFT OUTER JOIN [CaseSendSettingDetails] AS D ON M.CaseId = D.CaseId AND M.SerialID =D.SerialID                                
                                WHERE 1=2 ";
            Parameter.Clear();
            for (int i = 0; i < caseIdList.Count; i++)
            {
                strsql = strsql + " OR ( M.[CaseId] = @CaseId" + i + " and sendtype = "+type+ ") ";
                Parameter.Add(new CommandParameter("@CaseId" + i, caseIdList[i]));
            }
            DataTable Dt = Search(strsql);

            StringBuilder sbCaseids = new StringBuilder();
            foreach(var i in caseIdList)
            {
                sbCaseids.Append("'" + i + "',");
            }

            StringBuilder sbPayeeids = new StringBuilder();
            foreach(var i in PayIdList)
            {
                sbPayeeids.Append("'" + i + "',");
            }

            string strCaseids = string.Format("SELECT * FROM [dbo].[CasePayeeSetting] where caseid in ({0}) and PayeeId in ({1})", sbCaseids.ToString().TrimEnd(','), sbPayeeids.ToString().TrimEnd(','));

            // 由支付Payeeid中, 找到  支票號碼....
            var lstCheckNo = base.SearchList<CasePayeeSetting>(strCaseids).OrderBy(x=>x.CheckNo).ToList();
            
            List<string> realCheckNo = new List<string>();
            foreach(var lst in lstCheckNo)
            {
                int icheckno =int.Parse(lst.CheckNo);
                realCheckNo.Add(string.Format("CT{0:0000000}", icheckno));
                CaseIDSeq.Add(lst.CaseId.ToString());
            }


            Dt.Columns.Add("Sort", typeof(int));
            int seq = 0;
            // 過濾Dt 中的Description 欄位...
            //foreach(DataRow dr in Dt.Rows)
            //{
            //    bool isExist = false;
            //    foreach(var c in realCheckNo)
            //    {
            //        if (dr["Description"].ToString().Contains(c))
            //        {
            //            isExist = true;
            //            dr["Sort"] = seq++.ToString();
            //        }
            //    }
            //    if (!isExist) // 不包括在裏面, 則刪除本筆...
            //        dr.Delete();                    
            //}

            foreach(var c in realCheckNo)
            {
                bool isExist = false;
                foreach(DataRow dr in Dt.Rows)
                {
                    if (dr["Description"].ToString().Contains(c))
                    {
                        isExist = true;
                        dr["Sort"] = seq++.ToString();
                    }
                }
            }

            foreach(DataRow dr in Dt.Rows)
            {
                if (string.IsNullOrEmpty(dr["Sort"].ToString()))
                    dr.Delete();
            }


            Dt.AcceptChanges();


            //20201012, 排序
            Dt.DefaultView.Sort = "Sort";
            DataTable newDT = Dt.DefaultView.ToTable();
            Dt = newDT;


            Dt.Columns.Add("Receive");
            Dt.Columns.Add("Cc");
            if (Dt != null && Dt.Rows.Count > 0)
            {                
                string strSerialId = string.Empty;
             
                foreach (DataRow dr in Dt.Rows)
                {
                    strSerialId += "'" + dr["SerialID"].ToString() + "',";
                }
             
                strSerialId = strSerialId.TrimEnd(',');
                string sqlRecive = "SELECT GovName,SerialID FROM CaseSendSettingDetails WHERE SendType=1 and SerialID In (" + strSerialId + ")";
                string sqlCc = "SELECT GovName,SerialID FROM CaseSendSettingDetails WHERE SendType=2 and SerialID In (" + strSerialId + ")";
                List<CaseSendSettingDetails> listRecive = base.SearchList<CaseSendSettingDetails>(sqlRecive).ToList();
                List<CaseSendSettingDetails> listCc = base.SearchList<CaseSendSettingDetails>(sqlCc).ToList();
                if (listRecive != null && listRecive.Any())
                {
                    foreach (DataRow dr in Dt.Rows)
                    {
                        string strRecive = string.Empty;
                        foreach (CaseSendSettingDetails item in listRecive.Where(m => m.SerialID == Convert.ToInt32(dr["SerialID"])))
                        {
                            strRecive += item.GovName + "、";
                        }
                        strRecive = strRecive.TrimEnd('、');
                        dr["Receive"] = strRecive;
                    }
                }
                    if (listCc != null && listCc.Any())
                    {
                        foreach (DataRow dr in Dt.Rows)
                        {
                            string strCc = string.Empty;
                            foreach (CaseSendSettingDetails item in listCc.Where(m => m.SerialID == Convert.ToInt32(dr["SerialID"])))
                            {
                                strCc += item.GovName + "、";
                            }
                            strCc = strCc.TrimEnd('、');
                            dr["Cc"] = strCc;
                        }
                    }
                return Dt;
            }
            else
            {
                return Dt = new DataTable();
            }
        }
        public DataTable GetSendSettingByCaseIdList(List<string> caseIdList)
        {
            string strsql = @"SELECT M.[SerialID]
                                    , M.[CaseId]
                                    ,[Template]
                                    ,[SendWord]
                                    ,[SendNo]
                                    ,[SendDate]
                                    ,[Speed]
                                    ,[Security]
                                    ,[Subject]
                                    ,[Description]
                                    ,[isFinish]
                                    ,[FinishDate]
									,[Attachment]
                                    ,[GovName]
                                    ,[GovAddr]
                                    ,[SendType]
									,(select EmpName from [LDAPEmployee] emp where emp.EmpID=M.CreatedUser) as CreatedUser									
									,(select TelNo + ' 分機 '+TelExt from [LDAPEmployee] emp where emp.EmpID=M.CreatedUser) as TelNo 
                                FROM [CaseSendSetting] AS M with(Nolock)
                                LEFT OUTER JOIN [CaseSendSettingDetails] AS D ON M.CaseId = D.CaseId AND M.SerialID =D.SerialID
                                WHERE 1=2 ";
            Parameter.Clear();
            for (int i = 0; i < caseIdList.Count; i++)
            {
                strsql = strsql + " OR M.[CaseId] = @CaseId" + i + " ";
                Parameter.Add(new CommandParameter("@CaseId" + i, caseIdList[i]));
            }
            DataTable Dt = Search(strsql);
            Dt.Columns.Add("Receive");
            Dt.Columns.Add("Cc");
            if (Dt != null && Dt.Rows.Count > 0)
            {
                string strSerialId = string.Empty;
                foreach (DataRow dr in Dt.Rows)
                {
                    strSerialId += "'" + dr["SerialID"].ToString() + "',";
                }
                strSerialId = strSerialId.TrimEnd(',');
                string sqlRecive = "SELECT GovName,SerialID FROM CaseSendSettingDetails WHERE SendType=1 and SerialID In (" + strSerialId + ")";
                string sqlCc = "SELECT GovName,SerialID FROM CaseSendSettingDetails WHERE SendType=2 and SerialID In (" + strSerialId + ")";
                List<CaseSendSettingDetails> listRecive = base.SearchList<CaseSendSettingDetails>(sqlRecive).ToList();
                List<CaseSendSettingDetails> listCc = base.SearchList<CaseSendSettingDetails>(sqlCc).ToList();
                if (listRecive != null && listRecive.Any())
                {
                    foreach (DataRow dr in Dt.Rows)
                    {
                        string strRecive = string.Empty;
                        foreach (CaseSendSettingDetails item in listRecive.Where(m => m.SerialID == Convert.ToInt32(dr["SerialID"])))
                        {
                            strRecive += item.GovName + "、";
                        }
                        strRecive = strRecive.TrimEnd('、');
                        dr["Receive"] = strRecive;
                    }
                }

                if (listCc != null && listCc.Any())
                {
                    foreach (DataRow dr in Dt.Rows)
                    {
                        string strCc = string.Empty;
                        foreach (CaseSendSettingDetails item in listCc.Where(m => m.SerialID == Convert.ToInt32(dr["SerialID"])))
                        {
                           strCc += item.GovName + "、";
                        }
                        strCc=strCc.TrimEnd('、');
                        dr["Cc"]=strCc;
                    }
                }
                return Dt;
            }
            else
            {
                return Dt = new DataTable();
            }
        }

        public DataTable GetSeizurePayByCaseIdList(List<string> caseIdList)
        {
            string strsql = @"SELECT M.[SerialID]
                                    , M.[CaseId]
                                    ,[Template]
                                    ,[SendWord]
                                    ,[SendNo]
                                    ,[SendDate]
                                    ,[Speed]
                                    ,[Security]
                                    ,[Subject]
                                    ,[Description]
                                    ,[isFinish]
                                    ,[FinishDate]
									,[Attachment]
                                    ,[GovName]
                                    ,[GovAddr]
                                    ,[SendType]
									,(select EmpName from [LDAPEmployee] emp where emp.EmpID=M.CreatedUser) as CreatedUser
									,(select TelNo + ' 分機 '+TelExt from [LDAPEmployee] emp where emp.EmpID=M.CreatedUser) as TelNo                                    
                                FROM [CaseSendSetting] AS M
                                LEFT OUTER JOIN [CaseSendSettingDetails] AS D ON M.CaseId = D.CaseId AND M.SerialID =D.SerialID
                                WHERE (1=2 ";
            Parameter.Clear();
            for (int i = 0; i < caseIdList.Count; i++)
            {
                strsql = strsql + " OR M.[CaseId] = @CaseId" + i + " ";
                Parameter.Add(new CommandParameter("@CaseId" + i, caseIdList[i]));
            }
            strsql += @") and Template='支付'";
            DataTable Dt = Search(strsql);
            Dt.Columns.Add("Receive");
            Dt.Columns.Add("Cc");
            if (Dt != null && Dt.Rows.Count > 0)
            {
                string strSerialId = string.Empty;
                foreach (DataRow dr in Dt.Rows)
                {
                    strSerialId += "'" + dr["SerialID"].ToString() + "',";
                }
                strSerialId = strSerialId.TrimEnd(',');
                string sqlRecive = "SELECT GovName,SerialID,SendType FROM CaseSendSettingDetails WHERE SendType=1 and SerialID In (" + strSerialId + ")";
                string sqlCc = "SELECT GovName,SerialID,SendType FROM CaseSendSettingDetails WHERE SendType=2 and SerialID In (" + strSerialId + ")";
                List<CaseSendSettingDetails> listRecive = base.SearchList<CaseSendSettingDetails>(sqlRecive).ToList();
                List<CaseSendSettingDetails> listCc = base.SearchList<CaseSendSettingDetails>(sqlCc).ToList();

                if (listRecive != null && listRecive.Any())
                {
                    foreach (DataRow dr in Dt.Rows)
                    {
                        string strRecive = string.Empty;
                        foreach (CaseSendSettingDetails item in listRecive.Where(m => m.SerialID == Convert.ToInt32(dr["SerialID"])))
                        {
                            strRecive += item.GovName + "、";
                        }
                        strRecive = strRecive.TrimEnd('、');
                        dr["Receive"] = strRecive;
                    }
                }

                if (listCc != null && listCc.Any())
                {
                    foreach (DataRow dr in Dt.Rows)
                    {
                        string strCc = string.Empty;
                        foreach (CaseSendSettingDetails item in listCc.Where(m => m.SerialID == Convert.ToInt32(dr["SerialID"])))
                        {
                            strCc += item.GovName + "、";
                        }
                        strCc = strCc.TrimEnd('、');
                        dr["Cc"] = strCc;
                    }
                }
                return Dt;
            }
            else
            {
                return Dt = new DataTable();
            }
        }
        public string SendNo()
        {
            try
            {
                string sqlStr = @"select Top 1 (SendNoNow+1) as SendNoNow from SendNoTable where SendNoYear=@SendNoYear and SendNoNow<SendNoEnd";
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@SendNoYear", DateTime.Now.ToString("yyyy")));
                string result = Convert.ToString(base.ExecuteScalar(sqlStr));
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string UpdateSendNo(IDbTransaction trans = null)
        {
            IDbConnection dbConnection = OpenConnection();
            bool rtn = true;
            bool needSubmit = false;
            try
            {
                if (trans == null)
                {
                    needSubmit = true;
                    trans = dbConnection.BeginTransaction();
                }
                string sqlStr = @"select Top 1 (SendNoNow+1) as SendNoNow from SendNoTable where SendNoYear=@SendNoYear and SendNoNow<SendNoEnd";
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@SendNoYear", DateTime.Now.ToString("yyyy")));
                string result = Convert.ToString(base.ExecuteNonQuery(sqlStr));
                if (needSubmit)
                {
                    if (rtn)
                        trans.Commit();
                    else
                        trans.Rollback();
                }

                //return rtn;
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        
        /// <summary>
        /// 根據受款人資訊.和發文模版,生成一個發文的Model
        /// </summary>
        /// <param name="model">發文資訊</param>
        /// <param name="errMsg">錯誤訊息</param>
        /// <param name="trans"></param>
        /// <returns></returns>
        public CaseSendSettingCreateViewModel GetDefaultSendSetting(CasePayeeSetting model, out string errMsg, IDbTransaction trans = null, string _Account = null, bool autopay=false)
        {
            if (model == null || string.IsNullOrEmpty(model.CheckNo))
            {
                //* 理論上不會call到這裡.因為在外面就檢查了.這裡寫就是防呆
                errMsg = Lang.csfs_text_notnull;
                return null;
            }



            CaseSendSettingBIZ cssBiz = new CaseSendSettingBIZ();
            string sendNo = cssBiz.SendNo();
            if (string.IsNullOrEmpty(sendNo))
            {
                errMsg = Lang.csfs_sendno;
                return null;
            }
            CaseMasterBIZ masterBiz = new CaseMasterBIZ();
            SendSettingRefBiz refbiz = new SendSettingRefBiz();
            SendSettingRef basic;
            if( autopay)
            {
                basic = refbiz.GetSubjectAndDescription(model.CaseId, "支付電子回文", CaseKind2.CaseSeizureEdoc, trans, model);
            }
            else
            {
                basic = refbiz.GetSubjectAndDescription(model.CaseId, CaseKind2.CasePay, CaseKind2.CaseSeizureEdoc, trans, model);
            }                

            // 20210512 原發文邏輯執行產回文資訊，但若說明三超過480中文字落人。
            {
                int idxPosition = basic.Description.IndexOf("三、");
                if (idxPosition > 0 && basic.Description.Length > idxPosition)
                {
                    string tempStr = basic.Description.Substring(idxPosition);
                    if (tempStr.Length > 480)
                        errMsg = "說明三超過480個字，落人工";
                }
                else
                {
                    errMsg = "產生的說明有錯，落人工";
                }
            }


            CaseMaster master = masterBiz.MasterModel(model.CaseId, trans);
            DateTime sdate = new DateTime();
            if (string.IsNullOrEmpty(_Account))
            {
                if (String.IsNullOrEmpty(master.PayDate))
                {
                    sdate = masterBiz.GetPayDate(master.CaseKind2, master.CreatedDate);
                     CheckQueryAndPrintBIZ CKP = new CheckQueryAndPrintBIZ();
                    string strpayDate = CKP.GetWorkingDays(sdate.ToString("yyyy/MM/dd"));
                    sdate = Convert.ToDateTime(strpayDate);
                }
                else
                {
                    sdate = Convert.ToDateTime(master.PayDate);
                }
                CaseSendSettingCreateViewModel send = new CaseSendSettingCreateViewModel
                {
                    CaseId = model.CaseId,
                    Template = CaseKind2.CasePay,
                    SendWord = Lang.csfs_ctci_bank,
                    SendNo = sendNo,
                    SendDate=sdate,
                    //SendDate = String.IsNullOrEmpty(master.PayDate) ? masterBiz.GetPayDate(master.CaseKind2, master.CreatedDate) : Convert.ToDateTime(master.PayDate),
                    Speed = Lang.csfs_speed1,
                    Security = Lang.csfs_security1,
                    Attachment = "",
                    Subject = basic == null ? "" : basic.Subject,
                    ReceiveKind = String.IsNullOrEmpty(master.ReceiveKind)? "紙本" : master.ReceiveKind,
                    Description = basic == null ? "" : basic.Description,
                    CreatedUser = Account,
                    CreatedDate = DateTime.Now,
                    ModifiedUser = Account,
                    ModifiedDate = DateTime.Now,
                    ReceiveList = new List<CaseSendSettingDetails>(),
                    CcList = new List<CaseSendSettingDetails>()
                };
                //send.ReceiveList.Add(new CaseSendSettingDetails { CaseId = model.CaseId, GovAddr = model.Address, GovName = model.Receiver, SendType = 1 });
                //send.CcList.Add(new CaseSendSettingDetails { CaseId = model.CaseId, GovAddr = model.CCReceiver, GovName = model.Currency, SendType = 2 });
                //errMsg = "";
                //return send;
                // 20200804 Partrik

                string rList = model.Receiver;
                if (!string.IsNullOrEmpty(rList))
                {
                    if (rList.Contains("執行署") && master.CaseKind2 == "支付" && master.ReceiveKind == "電子公文")
                    {
                        rList += "(執)";
                    }
                }

                send.ReceiveList.Add(new CaseSendSettingDetails { CaseId = model.CaseId, GovAddr = model.Address, GovName = rList, SendType = 1 });
                if (model.Receiver != model.Currency) // 20200803, 若正本=副本機關, 則不秀副本, CASE 5, CASE 27
                {
                    string cList = model.Currency;
                    if (!string.IsNullOrEmpty(cList))
                    {
                        if (cList.Contains("執行署") && master.CaseKind2 == "支付" && master.ReceiveKind == "電子公文")
                        {
                            cList += "(執)";
                        }
                    }
                    send.CcList.Add(new CaseSendSettingDetails { CaseId = model.CaseId, GovAddr = model.CCReceiver, GovName = cList, SendType = 2 });
                }
                errMsg = "";
                return send;
            }
            else // 20200624, 原因是用Console程式呼叫, 無法由CommBiz, 取得HttpContext.Current.Session["LogonUser"], 是誰來產生, 所以由外部傳進來
            {
                if (String.IsNullOrEmpty(master.PayDate))
                {
                    sdate = masterBiz.GetPayDate(master.CaseKind2, master.CreatedDate);
                    CheckQueryAndPrintBIZ CKP = new CheckQueryAndPrintBIZ();
                    string strpayDate = CKP.GetWorkingDays(sdate.ToString("yyyy/MM/dd"));
                    sdate = Convert.ToDateTime(strpayDate);
                }
                else
                {
                    sdate = Convert.ToDateTime(master.PayDate);
                }
                CaseSendSettingCreateViewModel send = new CaseSendSettingCreateViewModel
                {
                    CaseId = model.CaseId,
                    Template = CaseKind2.CasePay,
                    SendWord = Lang.csfs_ctci_bank,
                    SendNo = sendNo,
                    SendDate = sdate,
                    //SendDate = String.IsNullOrEmpty(master.PayDate) ? masterBiz.GetPayDate(master.CaseKind2, master.CreatedDate) : Convert.ToDateTime(master.PayDate),
                    Speed = Lang.csfs_speed1,
                    Security = Lang.csfs_security1,
                    Attachment = "",
                    Subject = basic == null ? "" : basic.Subject,
                    Description = basic == null ? "" : basic.Description,
                    CreatedUser = _Account,
                    CreatedDate = DateTime.Now,
                    ModifiedUser = _Account,
                    ModifiedDate = DateTime.Now,
                    ReceiveList = new List<CaseSendSettingDetails>(),
                    CcList = new List<CaseSendSettingDetails>()
                };
                //send.ReceiveList.Add(new CaseSendSettingDetails { CaseId = model.CaseId, GovAddr = model.Address, GovName = model.Receiver, SendType = 1 });
                //send.CcList.Add(new CaseSendSettingDetails { CaseId = model.CaseId, GovAddr = model.CCReceiver, GovName = model.Currency, SendType = 2 });
                //errMsg = "";
                // 20200804 partrik

                string rList = model.Receiver;
                if (rList.Contains("執行署") && master.CaseKind2 == "支付" && master.ReceiveKind == "電子公文")
                {
                    rList += "(執)";
                }

                send.ReceiveList.Add(new CaseSendSettingDetails { CaseId = model.CaseId, GovAddr = model.Address, GovName = rList, SendType = 1 });
                if (model.Receiver != model.Currency) // 20200803, 若正本=副本機關, 則不秀副本 , CASE 5, CASE 27
                {
                    string cList = model.Currency;
                    if (cList.Contains("執行署") && master.CaseKind2 == "支付" && master.ReceiveKind == "電子公文")
                    {
                        cList += "(執)";
                    }
                    send.CcList.Add(new CaseSendSettingDetails { CaseId = model.CaseId, GovAddr = model.CCReceiver, GovName = cList, SendType = 2 });
                }
                errMsg = "";
                return send;
            }


        }

    }
}
