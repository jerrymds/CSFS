using CTBC.CSFS.Pattern;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using NPOI.SS.UserModel;
using NPOI.HSSF.Util;
using System.Data;
using NPOI.HSSF.UserModel;
using NPOI.SS.Util;
namespace CTBC.CSFS.BussinessLogic
{
    public class MeetingSummaryListBIZ : CommonBIZ
    {
        public MeetingSummaryListBIZ(AppController appController)
            : base(appController)
        { }
        public MeetingSummaryListBIZ()
        { }

        public MemoryStream ExportExcel(string GovDateS, string GovDateE)
        {
            #region
            string[] ColumnsName = new[]
                    {
                        "ReceiveDate",
                        "CaseNo",
                        "GovUnit",
                        "ObligorNo",
                        "BranchPaySave",
                        "BranchViptext",
                        "RmNotice",
                        "RmNoticeAndConfirm",
                        "Result1",
                        "Result2",
                        "Result3",
                        "Result4",
                        "Result5",
                        "Result6",
                        "Result7",
                        "Result8",
                        "StandardDate",
                        "AgentUser",
                        "MeetMemo"
                    };
            string CaseID = string.Empty;
            string ObligorNo = string.Empty;
            DataTable dt = new DataTable();
            dt = GetData(GovDateS, GovDateE);
            DataTable data = new DataTable();
            data.Columns.Add("ReceiveDate", typeof(string));
            data.Columns.Add("CaseNo", typeof(string));
            data.Columns.Add("GovUnit", typeof(string));
            data.Columns.Add("ObligorNo", typeof(string));
            data.Columns.Add("BranchPaySave", typeof(string));
            data.Columns.Add("BranchViptext", typeof(string));
            data.Columns.Add("RmNotice", typeof(string));
            data.Columns.Add("RmNoticeAndConfirm", typeof(string));
            data.Columns.Add("Result1", typeof(string));
            data.Columns.Add("Result2", typeof(string));
            data.Columns.Add("Result3", typeof(string));
            data.Columns.Add("Result4", typeof(string));
            data.Columns.Add("Result5", typeof(string));
            data.Columns.Add("Result6", typeof(string));
            data.Columns.Add("Result7", typeof(string));
            data.Columns.Add("Result8", typeof(string));
            data.Columns.Add("StandardDate", typeof(string));
            data.Columns.Add("AgentUser", typeof(string));
            data.Columns.Add("MeetMemo", typeof(string));
            if (dt.Rows.Count > 0)
            {
                int a = -1;
                int j = 1;
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    if (CaseID != Convert.ToString(dt.Rows[i]["CaseId"]) || ObligorNo != Convert.ToString(dt.Rows[i]["ObligorNo"].ToString()))
                    {
                        CaseID = Convert.ToString(dt.Rows[i]["CaseId"]);                    //* 當前caseid
                        ObligorNo = Convert.ToString(dt.Rows[i]["ObligorNo"].ToString());   //* 當前customerid
                        a++;        //* 行數
                        //* 因為caseId不同或者ObligorId不同.需要新起一行.
                        data.Rows.Add(data.NewRow());       
                        data.Rows[a]["ReceiveDate"] = dt.Rows[i]["ReceiveDate"];
                        data.Rows[a]["CaseNo"] = dt.Rows[i]["CaseNo"];
                        data.Rows[a]["GovUnit"] = dt.Rows[i]["GovUnit"];
                        data.Rows[a]["ObligorNo"] = dt.Rows[i]["ObligorNo"];
                        data.Rows[a]["BranchPaySave"] = dt.Rows[i]["BranchPaySave"];
                        data.Rows[a]["BranchViptext"] = dt.Rows[i]["BranchViptext"];
                        data.Rows[a]["RmNotice"] = dt.Rows[i]["RmNotice"];
                        data.Rows[a]["RmNoticeAndConfirm"] = dt.Rows[i]["RmNoticeAndConfirm"];
                        data.Rows[a]["AgentUser"] = dt.Rows[i]["AgentUser"];
                        data.Rows[a]["MeetMemo"] = dt.Rows[i]["MeetMemo"];
                        data.Rows[a]["Result1"] = dt.Rows[i]["Result"];
                        //* 基準日如果有一個為空白.則不用加中間杠
                        string StandardDateS = Convert.ToString(dt.Rows[i]["StandardDateS"]);
                        string StandardDateE = Convert.ToString(dt.Rows[i]["StandardDateE"]);
                        data.Rows[a]["StandardDate"] = !string.IsNullOrEmpty(StandardDateS) &&
                                                       !string.IsNullOrEmpty(StandardDateE)
                            ? StandardDateS + "-" + StandardDateE
                            : (!string.IsNullOrEmpty(StandardDateS) ? StandardDateS : StandardDateE);
                    }

                    string strNo = Convert.ToString(dt.Rows[i]["SortOrder"]);
                    data.Rows[a]["Result" + strNo] = Convert.ToBoolean(dt.Rows[i]["IsSelected"])? "1": "";
                }
            }
            #endregion
            IWorkbook workbook = new HSSFWorkbook();
            ISheet sheet = workbook.CreateSheet();

            #region def style
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

            

            ICellStyle styleBody = workbook.CreateCellStyle();
            IFont fontBody = workbook.CreateFont();
            fontBody.FontHeightInPoints = 6;
            fontBody.FontName = "新細明體";
            styleBody.WrapText = true;
            styleBody.Alignment = HorizontalAlignment.Center;
            styleBody.VerticalAlignment = VerticalAlignment.Center;
            styleBody.BorderTop = BorderStyle.Thin;
            styleBody.BorderLeft = BorderStyle.Thin;
            styleBody.BorderRight = BorderStyle.Thin;
            styleBody.BorderBottom = BorderStyle.Thin;
            styleBody.TopBorderColor = HSSFColor.Grey25Percent.Index;
            styleBody.LeftBorderColor = HSSFColor.Grey25Percent.Index;
            styleBody.RightBorderColor = HSSFColor.Grey25Percent.Index;
            styleBody.BottomBorderColor = HSSFColor.Grey25Percent.Index;
            styleBody.SetFont(fontBody);
            #endregion

            #region title
            SetExcelCell(sheet,0, 0, styleHead10, "收文日期");
            SetExcelCell(sheet, 1, 0, styleHead10, "");
            sheet.AddMergedRegion(new CellRangeAddress(0, 1, 0, 0));

            SetExcelCell(sheet, 0, 1, styleHead10, "案件編號");
            SetExcelCell(sheet, 1, 1, styleHead10, "");
            sheet.AddMergedRegion(new CellRangeAddress(0, 1, 1, 1));

            SetExcelCell(sheet, 0, 2, styleHead10, "來函機關");
            SetExcelCell(sheet, 1, 2, styleHead10, "");
            sheet.AddMergedRegion(new CellRangeAddress(0, 1, 2, 2));

            SetExcelCell(sheet, 0, 3, styleHead10, "ID");
            SetExcelCell(sheet, 1, 3, styleHead10, "");
            sheet.AddMergedRegion(new CellRangeAddress(0, 1, 3, 3));

            SetExcelCell(sheet, 0, 4, styleHead10, "分行");//1111111
            sheet.AddMergedRegion(new CellRangeAddress(0, 0, 4, 5));

            SetExcelCell(sheet, 1, 4, styleHead10, "支存");
            sheet.AddMergedRegion(new CellRangeAddress(1, 1, 4, 4));

            SetExcelCell(sheet, 1, 5, styleHead10, "VIP");
            sheet.AddMergedRegion(new CellRangeAddress(1, 1, 5, 5));

            SetExcelCell(sheet, 0, 6, styleHead10, "RM");//2222222
            sheet.AddMergedRegion(new CellRangeAddress(0, 0, 6, 7));

            SetExcelCell(sheet, 1, 6, styleHead10, "通知");
            sheet.AddMergedRegion(new CellRangeAddress(1, 1, 6, 6));

            SetExcelCell(sheet, 1, 7, styleHead10, "通知-且需確認是否抵銷債權");
            sheet.AddMergedRegion(new CellRangeAddress(1, 1, 7, 7));//--------------

            SetExcelCell(sheet, 0, 8, styleHead10, "01公債(含無實體公債)");//123
            sheet.AddMergedRegion(new CellRangeAddress(0, 0, 8, 8));

            SetExcelCell(sheet, 1, 8, styleHead10, "金融交易作業部(債票券與資金清算科)");
            sheet.AddMergedRegion(new CellRangeAddress(1, 1, 8, 8));

            SetExcelCell(sheet, 0, 9, styleHead10, "02公債(含無實體公債)");//123
            sheet.AddMergedRegion(new CellRangeAddress(0, 0, 9, 9));

            SetExcelCell(sheet, 1, 9, styleHead10, "法人信託作業客服部(債票券集保科)");
            sheet.AddMergedRegion(new CellRangeAddress(1, 1, 9, 9));

            SetExcelCell(sheet, 0, 10, styleHead10, "03信託財產");//123
            sheet.AddMergedRegion(new CellRangeAddress(0, 0, 10, 10));

            SetExcelCell(sheet, 1, 10, styleHead10, "法人信託作業客服部(信託服務二科)");
            sheet.AddMergedRegion(new CellRangeAddress(1, 1, 10, 10));

            SetExcelCell(sheet, 0, 11, styleHead10, "04信託財產");//123
            sheet.AddMergedRegion(new CellRangeAddress(0, 0, 11, 11));

            SetExcelCell(sheet, 1, 11, styleHead10, "個人信託部(保管銀行服務科)");
            sheet.AddMergedRegion(new CellRangeAddress(1, 1, 11, 11));

            SetExcelCell(sheet, 0, 12, styleHead10, "05基金");//123
            sheet.AddMergedRegion(new CellRangeAddress(0, 0, 12, 12));

            SetExcelCell(sheet, 1, 12, styleHead10, "個人信託部(信託服務科)");
            sheet.AddMergedRegion(new CellRangeAddress(1, 1, 12, 12));

            SetExcelCell(sheet, 0, 13, styleHead10, "06股務");//123
            sheet.AddMergedRegion(new CellRangeAddress(0, 0, 13, 13));

            SetExcelCell(sheet, 1, 13, styleHead10, "代理部");
            sheet.AddMergedRegion(new CellRangeAddress(1, 1, 13, 13));

            SetExcelCell(sheet, 0, 14, styleHead10, "07票券");//123
            sheet.AddMergedRegion(new CellRangeAddress(0, 0, 14, 14));

            SetExcelCell(sheet, 1, 14, styleHead10, "法人信託作業客服部(債票券集保科)");
            sheet.AddMergedRegion(new CellRangeAddress(1, 1, 14, 14));

            SetExcelCell(sheet, 0, 15, styleHead10, "08票券 ");//123
            sheet.AddMergedRegion(new CellRangeAddress(0, 0, 15, 15));

            SetExcelCell(sheet, 1, 15, styleHead10, "法人信託作業客服部(信託服務二科)");
            sheet.AddMergedRegion(new CellRangeAddress(1, 1, 15, 15));

            SetExcelCell(sheet, 0, 16, styleHead10, "基準日");
            SetExcelCell(sheet, 1, 16, styleHead10, "");
            sheet.AddMergedRegion(new CellRangeAddress(0, 1, 16, 16));

            SetExcelCell(sheet, 0, 17, styleHead10, "經辦");
            SetExcelCell(sheet, 1, 17, styleHead10, "");
            sheet.AddMergedRegion(new CellRangeAddress(0, 1, 17, 17));

            SetExcelCell(sheet, 0, 18, styleHead10, "備註");
            SetExcelCell(sheet, 1, 18, styleHead10, "");
            sheet.AddMergedRegion(new CellRangeAddress(0, 1, 18, 18));
            #endregion

            #region body
            if (dt.Rows.Count > 0)
            {
                for (int iRow = 0; iRow < data.Rows.Count; iRow++)
                {
                    for (int iCol = 0; iCol < ColumnsName.Length; iCol++)
                    {
                        SetExcelCell(sheet, iRow + 2, iCol, styleHead10, Convert.ToString(data.Rows[iRow][ColumnsName[iCol]]));
                    }
                }
            }
            #endregion

            MemoryStream ms = new MemoryStream();
            workbook.Write(ms);
            ms.Flush();
            ms.Position = 0;
            workbook = null;
            return ms;
        }

        public ICell SetExcelCell(ISheet sheet, int rowNum, int colNum, ICellStyle style, string value)
        {

            IRow row = sheet.GetRow(rowNum) ?? sheet.CreateRow(rowNum);
            ICell cell = row.GetCell(colNum) ?? row.CreateCell(colNum);
            cell.CellStyle = style;
            cell.SetCellValue(value);
            return cell;
        }

        public DataTable GetData(string GovDateS, string GovDateE)
        {
            try
            {
                string strSql = @"select distinct CMM.ModifiedDate,CMS.CaseId,COB.[ObligorNo],BranchPaySave,[BranchViptext],[RmNotice],[RmNoticeAndConfirm],StandardDateS,StandardDateE ,             
                                 [MeetMemo],CMD.Result,CMS.AgentUser,CMS.GovUnit,CMS.CaseId,(SELECT CONVERT(varchar(100),CMS.ReceiveDate, 111)) as ReceiveDate,CMS.CaseNo
                                ,CMD.IsSelected,CMD.MeetKind,CMD.MeetUnit,CMD.SortOrder
                                 from CaseMeetMaster CMM 
                                 left join CaseMeetDetails CMD on CMM.CaseId=CMD.CaseId
                                 left join CaseMaster CMS on CMM.CaseId=CMS.CaseId 
                                 left join CaseObligor COB on COB.CaseId=CMM.CaseId
                                where CMM.ModifiedDate>=@GovDateS and CMM.ModifiedDate<@GovDateE
                                ORDER BY CMS.CaseId,COB.ObligorNo,CMD.SortOrder";
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@GovDateS", GovDateS));
                base.Parameter.Add(new CommandParameter("@GovDateE", GovDateE));
                return base.Search(strSql);
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }
    }
}
