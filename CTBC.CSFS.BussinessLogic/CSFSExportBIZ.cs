/// <summary>
/// 程式說明：匯出資料
/// 維護部門:資訊管理處
/// CSFS Version:v3.0
/// 中國信託銀行 版權所有  ©  All Rights Reserved.
/// </summary>

using System;
using System.Web;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using CTBC.CSFS.Pattern;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using System.ComponentModel;
using System.Collections;
using NPOI.HSSF.Record.Formula.Functions;
using System.Reflection;


namespace CTBC.CSFS.Models
{
    public class CSFSExportBIZ : CommonBIZ
    {
        public CSFSExportBIZ(AppController appController)
            : base(appController)
        {}

        public CSFSExportBIZ()
        {}

        /// <summary>
        /// 產生輸出檔案
        /// </summary>
        /// <param name="result">匯出資料</param>
        /// <param name="state">匯出資料格式</param>
        /// <param name="fileNamePath">暫存檔案完整路徑</param>
        /// <returns></returns>
        public bool ExportData(DataTable result, string state, string filePath)
        {
            try
            {
                CommonBIZ commonBiz = new CommonBIZ();

                string msg = string.Empty;

                // 返回下載路徑
                string path = string.Empty;

                switch (state)
                {
                    // 列印EXCEL
                    case "XLS":

                        #region

                        byte[] data = null;

                        // 2012/09/26修改將datatable轉換為二進制流生成excel
                        using (HSSFWorkbook hssworkbook = new HSSFWorkbook())
                        {

                            // 創建sheet
                            ISheet sheet1 = hssworkbook.CreateSheet("sheet1");
                            ICellStyle style = hssworkbook.CreateCellStyle();
                            style.Alignment = HorizontalAlignment.CENTER;

                            IRow rowFirst = sheet1.CreateRow(0);

                            // 賦值表頭
                            for (int m = 0; m < result.Columns.Count; m++)
                            {
                                ICell cellFirst = rowFirst.CreateCell(m);

                                cellFirst.CellStyle = style;
                                cellFirst.SetCellValue(result.Columns[m].ColumnName);
                            }

                            // 賦值數據
                            for (int i = 0; i < result.Rows.Count; i++)
                            {
                                IRow row = sheet1.CreateRow(i + 1);

                                for (int j = 0; j < result.Columns.Count; j++)
                                {

                                    ICell cell = row.CreateCell(j);
                                    cell.SetCellValue(result.Rows[i][j].ToString());

                                }
                            }

                            using (MemoryStream ms = new MemoryStream())
                            {
                                hssworkbook.Write(ms);
                                data = ms.ToArray();
                            }


                        }

                        #endregion

                        CreateFile(data, filePath, ExportFormat.XLS);
                        break;
                    // 列印CSV
                    case "CSV":
                        //path = formatData(result, ExportFormat.CSV, fileName, Encoding.GetEncoding("UTF-8"), password);
                        break;
                    case "DOC":
                        //path = formatData(result, ExportFormat.DOC, fileName, Encoding.GetEncoding("UTF-8"), password);
                        break;
                    // 列印Txt
                    case "TXT":
                        //path = formatData(result, ExportFormat.TXT, fileName, Encoding.GetEncoding("UTF-8"), password);
                        break;
                }

                return true; //"Success:" + path.Replace("%5C", "/");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 建立匯出資料檔案
        /// </summary>
        /// <param name="buffer">匯出資料</param>
        /// <param name="filePath">暫存檔案完整路徑</param>
        /// <param name="exportFormat">匯出資料檔案格式</param>
        public void CreateFile(byte[] buffer, string filePath, ExportFormat exportFormat)
        {
            try{
                string fName = DateTime.Now.ToString("yyyyMMddHHmmssfff");

                // 需要壓縮的檔案
                //fileNamePath 範例:Server.MapPath(Config.GetValue("Upload_Path") + fileName + "." + exportFormat.ToString().ToLower());

                #region 保存文件

                // 判斷文件是否存在,存在則刪除
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }


                using (FileStream fstmObj = new FileStream(filePath, FileMode.Create))//;
                {
                    // 遍歷寫入文件
                    for (int i = 0; i <= buffer.Length - 1; i++)
                    {
                        fstmObj.WriteByte(buffer[i]);
                    }

                    // 釋放資源
                    fstmObj.Flush();
                    fstmObj.Close();
                }
                GC.Collect();

                #endregion                
                //return Url.Action(Config.GetValue("Upload_Path").Substring(2), "/").Replace("///", "/") + fileName + "." + exportFormat.ToString().ToLower();
                //return isOK;
            }
            catch(Exception ex)
            {                
                throw ex;
            }
        }


        #region --筱婷新增的區塊 --


        public IList<CSFSExportDataVO> GetDataList(CSFSExportDataVO qryExportDataVO, CSFSExportDataVO ExportDataObject, int pageIndex)
        {
            try
            {
                bool flag1 = false;
                bool flagDISPMaster = false;
                bool flagUDWRDerivedData = false;

                string strWhere1 = "";
                string strPaging = " where rownum between @pageS and @pageE";

                string LeftJOINDISPMaster = " left join DISPMaster d WITH (NOLOCK) on a.ApplNo = d.ApplNo and a.ApplNoB = d.ApplNoB ";
                string LeftJOINUDWRDerivedData = " left join UDWRDerivedData f  WITH (NOLOCK) on a.ApplNo = f.ApplNo and a.ApplNoB = f.ApplNoB";
                string CusDisplayFormat = "";


                string LogMsg = "";

                if (qryExportDataVO != null)
                {
                    base.Parameter.Clear();

                    //起始申請日期、結束申請日期
                    DateTime ApplDateS;
                    DateTime ApplDateE;
                    string sApplDateS;
                    string sApplDateE;
                    if (qryExportDataVO.ApplDateS != null && qryExportDataVO.ApplDateE != null)
                    {
                        flag1 = true;
                        ApplDateS = Convert.ToDateTime(qryExportDataVO.ApplDateS);
                        ApplDateE = Convert.ToDateTime(qryExportDataVO.ApplDateE);
                        sApplDateS = ApplDateS.ToString("yyyyMMdd");
                        sApplDateE = ApplDateE.AddDays(1).ToString("yyyyMMdd");
                        base.Parameter.Add(new CommandParameter("@ApplDateS", sApplDateS));
                        base.Parameter.Add(new CommandParameter("@ApplDateE", sApplDateE));
                        strWhere1 += " and (a.applno between @ApplDateS and @ApplDateE ) ";
                        LogMsg += "起始申請日期=" + ApplDateS + ";結束申請日期=" + ApplDateE + ";";
                    }

                
                    //案件種類
                    string ApplTypeCode;
                    if (qryExportDataVO.ApplTypeCode != null)
                    {
                        flag1 = true;
                        ApplTypeCode = qryExportDataVO.ApplTypeCode.ToString();
                        base.Parameter.Add(new CommandParameter("@ApplTypeCode", ApplTypeCode));
                        strWhere1 += " and (a.ApplTypeCode = @ApplTypeCode) ";
                        LogMsg += "案件種類=" + ApplTypeCode + ";";
                    }

                    //變簽類別(徵信變簽、產品變簽)
                    //結果區的主借款人ID及姓名不去識別化，但匯出的EXCEL檔要去識別化
                    string DISPType;
                    if (qryExportDataVO.DISPType != null)
                    {
                        flagDISPMaster = true;
                        DISPType = qryExportDataVO.DISPType.ToString();
                        base.Parameter.Add(new CommandParameter("@DISPType", DISPType));

                        strWhere1 += " and d.DISPType =  @DISPType ";
                        LogMsg += "變簽類別=" + DISPType + ";";
                    }

                    //案件審核結果
                    string ApproveAuditResult;
                    if (qryExportDataVO.ApproveAuditResult != null)
                    {
                        flagUDWRDerivedData = true;
                        ApproveAuditResult = qryExportDataVO.ApproveAuditResult.ToString();
                        base.Parameter.Add(new CommandParameter("@ApproveAuditResult", ApproveAuditResult));
                        strWhere1 += " and ((f.isClose='Y' and f.LatestApproveAuditResult = @ApproveAuditResult) ";


                        flagDISPMaster = true;
                        strWhere1 += " or d.FinalApprResult= @ApproveAuditResult) ";

                        LogMsg += "案件審核結果=" + ApproveAuditResult + ";";
                    }


                    if (qryExportDataVO.isPaging == true)
                    {
                        CusDisplayFormat = " case when isnull(c.CusId,'') = '' then '' else c.CusId end CusId, ";
                        CusDisplayFormat += " case when isnull(c.CusName,'') = '' then '' else c.CusName end CusName,  ";
                    }
                    else
                    {
                        CusDisplayFormat = " case when isnull(c.CusId,'') = '' then '' when len(c.CusId) >=10 then left(c.CusId,3)+'*****'+substring(c.CusId,8,2) end CusId, ";
                        CusDisplayFormat += " case when isnull(c.CusName,'') = '' then '' else left(c.CusName,1)+'**' end CusName,  ";
                    }

                }

                CSFSLogBIZ.WriteLog(new CSFSLog()
                {
                    Title = "CSFSExportData",
                    Message = LogMsg.ToString()
                });

                base.PageIndex = pageIndex;

                StringBuilder strSQLMain = new StringBuilder("");
                StringBuilder strSQLPage = new StringBuilder("");
                StringBuilder strSQL1 = new StringBuilder("");
                StringBuilder strSQL2 = new StringBuilder("");
                StringBuilder strSQL3 = new StringBuilder("");
                StringBuilder strSQL4 = new StringBuilder("");
                StringBuilder strSQL5 = new StringBuilder("");

                base.Parameter.Add(new CommandParameter("@pageS", (base.PageSize * (base.PageIndex - 1)) + 1));
                base.Parameter.Add(new CommandParameter("@pageE", base.PageSize * base.PageIndex));

                strSQL1 = new StringBuilder(@"
                                            ;with A1 as (
	                                            select a.ApplNo,a.ApplNoB
	                                            from CSFSMaster a WITH (NOLOCK)
	                                            left join (select ApplNo,ApplNoB,CusId,CusName from CSFSCustomerInfo WITH (NOLOCK) Where LoanRelation = '1') b on a.ApplNo = b.ApplNo and a.ApplNoB = b.ApplNoB
	                                        ");

                strSQL2 = new StringBuilder(@"  where 1=1 and isnull(a.CaseEndTime,'') <> '' ");

                strSQL3 = new StringBuilder(@"
                                            ), A2 as  
                                            (
                                                select ApplNo,ApplNoB, row_number() over (order by ApplNo) RowNum
                                                from A1
                                            ), A3 as --分頁
                                            (
                                                select ApplNo,ApplNoB,(select max(RowNum) from A2) maxnum from A2 ");
                strSQL4 = new StringBuilder(@"), ApproveLog as --產品變簽LFA放行科長
                                            (
                                                select Row_Number() over (PARTITION BY ApplNo,ApplNoB order by ModifiedDate desc) rownum,
                                                        ApplNo,ApplNoB,ApprUserid 
                                                from dbo.DISPProdApprLog WITH (NOLOCK)
                                                where ApprLevel = 'S2' and ApprResult = 'A' 
                                            ), ApproveLog2 as  --產品變簽LFA放行科長
                                            (
                                                select ApplNo,ApplNoB,ApprUserId ApproveBossId from ApproveLog where rownum = '1'
                                            ),LFAApproveBoss1 as --初審最後放行的LFA主管
                                            (
	                                            select Row_Number() over (PARTITION BY WorkItemID order by EndTime desc) rownum,WorkItemID,OwnerNo
	                                            from FlowDetail 
	                                            where StepID = '0300' and isnull(EndTime,'') <> '' 
                                            ), LFAApproveBoss2 as --初審最後放行的LFA主管 
                                            (
	                                            select f.WorkItemID,f.ApplNo,f.ApplNoB,f2.OwnerNo
	                                            from FlowCaseInformation f
                                                left join LFAApproveBoss1 f2 on f.WorkItemID = f2.WorkItemID and f2.rownum = 1
                                            ), A4 as 
                                            ( 
                                                select row_number() over (order by a.applno) RowNum,
                                                a.ApplNo,a.ApplNoB, ");

                strSQL5 = new StringBuilder(@"  b.ApplTypeCode,    --案件類型(新作/變簽件/申覆件)
                                                b.BusClassType,    --業務別
                                                case when ApplTypeCode = 'J' then d.DISPApplType else b.RushFlag end RushFlag, --一般案件之是否急件(一般件/急件/優選),變簽件之案件別(01=一般件/02=急件/03=抱怨件)
                                                d.DISPType,        --變簽種類(產品/徵信)
                                                d.DISPSourceType,  --進件來源
                                                b.ApplyDate,       --進件日(1:LFA,2:授服,3:二代)
                                                case when isnull(p.LFAUser,'') <> '' then p.LFAUser
                                                    when isnull(d.ApplUserId,'') <> '' then d.ApplUserId end LFAUser, --LFA
                                                b.KeyInEmpNo,      --一般案件之完整鍵檔鍵檔人員
                                                b.KeyInCompleteTime,--鍵檔完成時間
                                                b.PromotionID,     --一般案件之推廣人員
                                                case when isnull(l.OwnerNo,'') <> '' then l.OwnerNo  --LFA初審審核主管改抓FlowDetail-0300的FlowOwner
                                                    when isnull(g.ApproveBossId,'') <> '' then g.ApproveBossId else '' end LFABoss,
                                                e.ProcDateTime,    --鑑價完成時間
                                                fl.OwnerNo2 APRLOwner, --鑑價Owner
                                                f.LatestApproveApplEmpNo, --徵信處理人員
                                                f.LatestApproveApplDateTime, --徵信完成時間    
                                                case when f.isClose = 'Y' then u.CloseDate 
                                                    when isnull(d.FinalApprDT,'') <> '' then d.FinalApprDT end CloseDate, --核決主管完成時間
                                                case when isnull(f.LatestApproveSupervisorEmpNo,'')<>'' then f.LatestApproveSupervisorEmpNo 
                                                    when isnull(d.FinalApprUserId,'')<>'' then d.FinalApprUserId else '' end LatestApproveSupervisorEmpNo,
                                                case when f.isClose = 'Y' then  f.LatestApproveAuditResult  
                                                    when f.isClose = 'N' and (f.LatestApproveAuditResult = 'P' or f.LatestApproveAuditResult = 'S') then f.LatestApproveAuditResult
                                                    when isnull(d.FinalApprResult,'') <> '' then d.FinalApprResult
                                                    else '' end LatestApproveAuditResult,  --徵信審核結果
                                                (select max(RowNum) from A2) maxnum  --總筆數,
                                            from A3 a
                                            left join PSRNMaster p WITH (NOLOCK) on a.ApplNo = p.ApplNo and a.ApplNoB = p.ApplNoB
                                            left join CSFSMaster b WITH (NOLOCK) on a.ApplNo = b.ApplNo and a.ApplNoB = b.ApplNoB
                                            left join (select ApplNo,ApplNoB,CusId,CusName from CSFSCustomerInfo WITH (NOLOCK) Where LoanRelation = '1') c on a.ApplNo = c.ApplNo and a.ApplNoB = c.ApplNoB 
                                            left join DISPMaster d WITH (NOLOCK) on a.ApplNo = d.ApplNo and a.ApplNoB = d.ApplNoB 
                                            left join APRLMaster e WITH (NOLOCK) on a.ApplNo = e.ApplNo and a.ApplNoB = e.ApplNoB
                                            left join UDWRDerivedData f WITH (NOLOCK) on a.ApplNo = f.ApplNo and a.ApplNoB = f.ApplNoB
                                            left join UDWRMaster u WITH (NOLOCK) on a.ApplNo = u.ApplNo and a.ApplNoB = u.ApplNoB
                                            left join ApproveLog2 g WITH (NOLOCK) on a.applno = g.ApplNo and a.applnob = g.ApplNoB
                                            left join LFAApproveBoss2 l WITH (NOLOCK) on a.applno = l.ApplNo and a.applnob = l.ApplNoB
                                            left join FlowCaseInformation fl WITH (NOLOCK) on a.applno = fl.ApplNo and a.applnob = fl.ApplNoB
                                        )
                                        select row_number() over (order by a.ApplNo) rownum,
                                               a.ApplNo+'-'+a.ApplNoB ApplNo,
                                               a.CusId,a.CusName,
                                               isnull((select CodeDesc from PARMCODE where CodeType = 'ApplTypeCode' and PARMCODE.CodeNo = a.ApplTypeCode),'') ApplTypeCode,
                                               isnull((select CodeDesc from PARMCODE where CodeType = 'BusClassType' and PARMCODE.CodeNo = a.BusClassType),'') BusClassType,
                                               case when a.ApplTypeCode = 'J' then isnull((select CodeDesc from PARMCODE where CodeType = 'DISPAPPLTYPE' and PARMCODE.CodeNo = a.RushFlag),'') 
                                                    else isnull((select CodeDesc from PARMCODE where CodeType = 'URGENTTYPE' and PARMCODE.CodeNo = a.RushFlag),'') end RushFlag,
                                               case when isnull(a.DISPType,'') = '' then '' else isnull((select CodeDesc from PARMCODE where CodeType = 'DISPType' and PARMCODE.CodeNo = a.DISPType),'') end DISPType,
                                               ApplyDate,
                                               case when isnull(LFAUser,'')='' then '' else isnull((select EmpName from CSFSEmployee where EmpId = a.LFAUser),'')+'('+LFAUser+')' end  LFAUserName,
                                               case when isnull(LFABoss,'')='' then '' else isnull((select EmpName from CSFSEmployee where EmpId = a.LFABoss),'')+'('+LFABoss+')' end  LFABossName,
                                               case when isnull((select BUName from CSFSBU WITH (NOLOCK) where BUID = bu.BUparent),'') = '' then '' 
                                                    else isnull((select BUName from CSFSBU WITH (NOLOCK) where BUID = bu.BUparent),'') + '/'+ isnull(BUName,'') end BUDept,  --業務單位部門
                                               case when isnull(KeyInEmpNo,'')='' then '' else isnull((select EmpName from CSFSEmployee where EmpId = a.KeyInEmpNo),'')+'('+KeyInEmpNo+')' end  KeyInEmpName,
                                               case when isnull(KeyInCompleteTime,'')='' then null else KeyInCompleteTime end  KeyInCompleteTime,
                                               case when isnull(APRLOwner,'')='' then '' else isnull((select EmpName from CSFSEmployee where EmpId = a.APRLOwner),'')+'('+APRLOwner+')' end  APRLOwnerName,
                                               case when isnull(ProcDateTime,'')='' then null else ProcDateTime end  APRLProcDateTime,
                                               case when isnull(LatestApproveApplEmpNo,'')='' then '' else isnull((select EmpName from CSFSEmployee where EmpId = a.LatestApproveApplEmpNo),'')+'('+LatestApproveApplEmpNo+')' end  LatestApproveApplEmpName,
                                               case when isnull(LatestApproveApplDateTime,'')='' then null else LatestApproveApplDateTime end  LatestApproveApplDateTime,
                                               case when isnull(LatestApproveSupervisorEmpNo,'')='' then '' else isnull((select EmpName from CSFSEmployee where EmpId = a.LatestApproveSupervisorEmpNo),'')+'('+LatestApproveSupervisorEmpNo+')' end  LatestApproveSupervisorEmpName,
                                               case when isnull(CloseDate,'')='' then null else CloseDate end  CloseDate,
                                               isnull((select CodeDesc from PARMCODE where CodeType = 'UDWR_AuditResult' and PARMCODE.CodeNo = a.LatestApproveAuditResult),'') ApproveAuditResult,
                                               isnull(BUName,'') BUName, --業務單位名稱
                                               maxnum
                                        from A4 a
                                        left join CSFSBUToEmployee e WITH (NOLOCK) on e.EmpID = a.LFAUser and e.BUMasterID = '1'
                                        left join CSFSBU bu WITH (NOLOCK) on e.BUID = bu.BUID  
                                        order by rownum ");


                strSQLMain.Append(strSQL1);

                if ((flagUDWRDerivedData) && (flagDISPMaster))
                    strSQLMain.Append(LeftJOINDISPMaster).Append(LeftJOINUDWRDerivedData);
                if ((!flagUDWRDerivedData) && (flagDISPMaster))
                    strSQLMain.Append(LeftJOINDISPMaster);

                strSQLMain.Append(strSQL2);
                if (flag1)
                    strSQLMain.Append(strWhere1);
                strSQLMain.Append(strSQL3);

                if (qryExportDataVO.isPaging == true)
                    strSQLMain.Append(strPaging);
                strSQLMain.Append(strSQL4).Append(CusDisplayFormat).Append(strSQL5);

                

                IList<CSFSExportDataVO> _ilsit = base.SearchList<CSFSExportDataVO>(strSQLMain.ToString());

                if (ExportDataObject.TotalItemCount.Equals(0) || ExportDataObject == null)
                {
                    if (_ilsit.Count > 0)
                        base.DataRecords = _ilsit[0].maxnum;
                    else
                        base.DataRecords = 0;
                }
                else
                    base.DataRecords = ExportDataObject.TotalItemCount;

                return _ilsit;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //新增人員：莊筱婷
        //將IList集合物件轉成DataTable
        //entitys：集合物件、len：轉到第幾個欄位值的資料
        public DataTable ConvertListObjectToDataTable(IList entitys,int len)
        {
            DataTable dt = new DataTable();
            if (entitys.Count > 0)
            {
                Type entityType = entitys[0].GetType();
                PropertyInfo[] entityProperties = entityType.GetProperties();

               
                for (int i = 0; i < entityProperties.Length; i++)
                {
                    dt.Columns.Add(entityProperties[i].Name);
                }

                foreach (object entity in entitys)
                {
                    if (entity.GetType() != entityType)
                        throw new Exception("要轉換的集合類型不一致");

                    object[] entityValues = new object[entityProperties.Length];
                    for (int i = 0; i < len; i++)
                        entityValues[i] = entityProperties[i].GetValue(entity, null);

                    dt.Rows.Add(entityValues);
                }
            }

            return dt;  

        }


        /// <summary>
        /// 產生輸出檔案
        /// </summary>
        /// <param name="result">匯出資料</param>
        /// <param name="state">匯出資料格式</param>
        /// <param name="fileNamePath">暫存檔案完整路徑</param>
        /// 新增日期：2014/3/17
        /// 新增內容：針對匯出的EXCEL檔，賦予中文的欄位名稱
        /// <returns></returns>
        public bool ExportData(DataTable result, string state, string filePath, SortedList<int, string> excelTitileNM)
        {
            try
            {
                CommonBIZ commonBiz = new CommonBIZ();

                string msg = string.Empty;

                // 返回下載路徑
                string path = string.Empty;

                switch (state)
                {
                    // 列印EXCEL
                    case "XLS":

                        #region

                        byte[] data = null;

                        // 2012/09/26修改將datatable轉換為二進制流生成excel
                        using (HSSFWorkbook hssworkbook = new HSSFWorkbook())
                        {

                            // 創建sheet
                            ISheet sheet1 = hssworkbook.CreateSheet("sheet1");
                            ICellStyle style = hssworkbook.CreateCellStyle();
                            style.Alignment = HorizontalAlignment.CENTER;

                            IRow rowFirst = sheet1.CreateRow(0);

                            // 賦值表頭
                            foreach (KeyValuePair<int, string> TitleNM in excelTitileNM)
                            {
                                int Key = Convert.ToInt32(TitleNM.Key);
                                string Value = TitleNM.Value.ToString();
                                ICell cellFirst = rowFirst.CreateCell(Key-1);

                                cellFirst.CellStyle = style;
                                cellFirst.SetCellValue(Value);
                            }

                            // 賦值數據
                            for (int i = 0; i < result.Rows.Count; i++)
                            {
                                IRow row = sheet1.CreateRow(i + 1);

                                for (int j = 0; j < result.Columns.Count; j++)
                                {

                                    ICell cell = row.CreateCell(j);
                                    cell.SetCellValue(result.Rows[i][j].ToString());

                                }
                            }

                            using (MemoryStream ms = new MemoryStream())
                            {
                                hssworkbook.Write(ms);
                                data = ms.ToArray();
                            }


                        }

                        #endregion

                        CreateFile(data, filePath, ExportFormat.XLS);
                        break;
                    // 列印CSV
                    case "CSV":
                        //path = formatData(result, ExportFormat.CSV, fileName, Encoding.GetEncoding("UTF-8"), password);
                        break;
                    case "DOC":
                        //path = formatData(result, ExportFormat.DOC, fileName, Encoding.GetEncoding("UTF-8"), password);
                        break;
                    // 列印Txt
                    case "TXT":
                        //path = formatData(result, ExportFormat.TXT, fileName, Encoding.GetEncoding("UTF-8"), password);
                        break;
                }

                return true; //"Success:" + path.Replace("%5C", "/");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion


    }

    public enum ExportFormat
    {
        /// <summary>
        /// XLS
        /// </summary>
        XLS,
        /// <summary>
        /// CSV
        /// </summary>
        CSV,
        /// <summary>
        /// DOC
        /// </summary>
        DOC,
        /// <summary>
        /// TXT
        /// </summary>
        TXT,
        /// <summary>
        /// STP
        /// </summary>
        STP
    }


   

}
